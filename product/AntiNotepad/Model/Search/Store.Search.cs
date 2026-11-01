using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntiNotepad.Model;

public sealed partial class Store
{
    private readonly SearchBodyCache _searchBodyCache = new();
    /// <summary>Captures only note metadata and text already in memory; this method never reads a body file.</summary>
    internal SearchNoteSnapshot[] CreateSearchSnapshot(IEnumerable<Note> notes)
    {
        var snapshot = new List<SearchNoteSnapshot>();
        foreach (var note in notes)
        {
            string? text = null;
            var hasText = !note.BodyUnavailable && note.TryGetSearchText(out text);
            var knownTitle = note.TitleCache is not null || hasText;
            var title = note.TitleCache ?? (hasText ? Note.MakeTitle(text!) : "");
            snapshot.Add(new SearchNoteSnapshot(note.Id, title, knownTitle, note.Modified, note.SearchRevision,
                hasText ? text : null, note.BodyUnavailable));
        }
        return snapshot.ToArray();
    }

    /// <summary>Captures the immutable body-folder path and search cache on the caller thread before background work begins.</summary>
    internal SearchContext CreateSearchContext(IReadOnlyList<SearchNoteSnapshot> notes)
    {
        lock (_searchBodyCache.Gate) return new(notes.ToArray(), _bodies, _searchBodyCache, _searchBodyCache.Epoch);
    }

    /// <summary>Searches from one body read per note at most; snippets and matching share that same text.</summary>
    internal static SearchMatchResult[] MatchSearch(SearchContext context, string query, CancellationToken cancellationToken)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matches = new List<SearchMatchResult>();
        foreach (var note in context.Notes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? text = note.InMemoryText;
            var status = text is null ? BodyReadStatus.Unreadable : BodyReadStatus.Available;
            if (text is not null) CacheSearchBody(context.Cache, context.CacheEpoch, note, text);
            else
            {
                var body = ReadSearchBody(context.BodyFolder, context.Cache, context.CacheEpoch, note, cancellationToken);
                status = body.Status;
                text = body.Text;
            }

            var (mode, keywordLength, modeArgument) = status == BodyReadStatus.Available ? Modes.Detect(text ?? "") : (NoteMode.Plain, 0, (string?)null);
            var title = note.TitleKnown ? note.Title : status == BodyReadStatus.Available ? Note.MakeTitle(text ?? "", keywordLength) : "Note text unavailable";
            var matchesQuery = terms.Length == 0 || terms.All(term =>
                title.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                status == BodyReadStatus.Available && (text ?? "").Contains(term, StringComparison.CurrentCultureIgnoreCase));
            if (!matchesQuery) continue;

            var snippet = status == BodyReadStatus.Available ? MakeSearchSnippet(text ?? "", terms, keywordLength) : "Note text unavailable";
            matches.Add(new SearchMatchResult(note.Id, title, snippet, mode, modeArgument, status));
        }
        return matches.ToArray();
    }

    internal int SearchBodyReadCount => Volatile.Read(ref _searchBodyCache.ReadCount);

    internal void ResetSearchBodyReadCount() => Interlocked.Exchange(ref _searchBodyCache.ReadCount, 0);

    private void InvalidateSearchBody(string id)
    {
        lock (_searchBodyCache.Gate) _searchBodyCache.Entries.Remove(id);
    }

    private static void CacheSearchBody(SearchBodyCache cache, long epoch, SearchNoteSnapshot note, string text)
    {
        lock (cache.Gate)
        {
            if (cache.Epoch != epoch) return;
            if (cache.Entries.TryGetValue(note.Id, out var existing) &&
                (existing.Revision > note.Revision || existing.Revision == note.Revision && existing.Modified > note.Modified)) return;
            cache.Entries[note.Id] = new SearchBodyCacheEntry(note.Modified, note.Revision, text);
        }
    }

    private static BodyReadResult ReadSearchBody(string folder, SearchBodyCache cache, long epoch, SearchNoteSnapshot note, CancellationToken cancellationToken)
    {
        lock (cache.Gate)
            if (cache.Epoch == epoch && cache.Entries.TryGetValue(note.Id, out var cached) && cached.Modified == note.Modified && cached.Revision == note.Revision)
                return new(cached.Text, BodyReadStatus.Available);

        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref cache.ReadCount);
        BodyReadResult result;
        try { result = new(File.ReadAllText(SearchBodyPath(folder, note.Id)), BodyReadStatus.Available); }
        catch (FileNotFoundException) { result = new(null, BodyReadStatus.Missing); }
        catch (DirectoryNotFoundException) { result = new(null, BodyReadStatus.Missing); }
        catch { result = new(null, BodyReadStatus.Unreadable); }

        cancellationToken.ThrowIfCancellationRequested();
        if (result.Status == BodyReadStatus.Available) CacheSearchBody(cache, epoch, note, result.Text ?? "");
        return result;
    }

    private static string SearchBodyPath(string folder, string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !string.Equals(Path.GetFileName(id), id, StringComparison.Ordinal) || id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidDataException("The note has an invalid id.");
        return Path.Combine(folder, id + ".md");
    }

    private static string MakeSearchSnippet(string text, string[] terms, int keywordLength)
    {
        var lines = text.Replace("\r", "").Split('\n').Take(400)
            .Skip(keywordLength > 0 ? 2 : 1).Where(line => line.Trim().Length > 0).ToList();
        if (terms.Length > 0)
        {
            var hit = lines.FindIndex(line => line.Contains(terms[0], StringComparison.CurrentCultureIgnoreCase));
            if (hit > 0) lines = lines.Skip(hit).ToList();
        }
        return string.Join("  ·  ", lines.Take(6).Select(line => line.Trim().Replace("[ ]", "☐").Replace("[x]", "☑")));
    }

    private void ClearSearchBodyCache()
    {
        lock (_searchBodyCache.Gate)
        {
            _searchBodyCache.Epoch++;
            _searchBodyCache.Entries.Clear();
        }
    }
}
