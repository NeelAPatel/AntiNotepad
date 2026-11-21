// Checks icon font ownership and the bundled font's glyph names.
using System.Buffers.Binary;
using System.Text;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;

namespace AntiNotepad;

internal static partial class SelfTest
{
    /// <summary>Checks that each registered code exists in its font and bundled Fluent names match the font.</summary>
    private static bool IconRegistrySelfTest(out string report)
    {
        var failures = new List<string>();
        Dictionary<int, int> fluentMap;
        Dictionary<int, string> fluentNames;
        try
        {
            if (Application.Current is null)
                AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
            using var stream = AssetLoader.Open(new Uri(AppInfo.FontUri));
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            var font = FluentFont.Read(memory.ToArray());
            fluentMap = font.CharacterToGlyph;
            fluentNames = font.GlyphNames;
        }
        catch (Exception ex)
        {
            report = "bundled Fluent font could not be read: " + ex.Message;
            return false;
        }

        foreach (var icon in Icons.All)
        {
            if (!FontManager.Current.TryGetGlyphTypeface(new Typeface(Icons.FontFamily(icon.Font)), out var typeface))
            {
                failures.Add($"{icon.Name}: {icon.Font} font is unavailable");
                continue;
            }
            if (!typeface.CharacterToGlyphMap.ContainsGlyph(icon.Code))
            {
                failures.Add($"{icon.Name}: U+{icon.Code:X4} is missing from {icon.Font}");
                continue;
            }

            if (icon.Font != IconFont.Fluent) continue;
            var glyphId = fluentMap.GetValueOrDefault(icon.Code);
            if (glyphId == 0 || !fluentNames.TryGetValue(glyphId, out var name))
                failures.Add($"{icon.Name}: U+{icon.Code:X4} has no bundled post name");
            else if (!string.Equals(name, icon.FontGlyphName, StringComparison.Ordinal))
                failures.Add($"{icon.Name}: U+{icon.Code:X4} is {name}, expected {icon.FontGlyphName}");
        }

        report = failures.Count == 0
            ? $"{Icons.All.Count} icons exist in their declared fonts; Fluent glyph names match"
            : string.Join("; ", failures.Take(8));
        return failures.Count == 0;
    }

    /// <summary>Holds the cmap and post data needed to verify the bundled icon font.</summary>
    private sealed class FluentFont
    {
        private FluentFont(Dictionary<int, int> characterToGlyph, Dictionary<int, string> glyphNames)
        {
            CharacterToGlyph = characterToGlyph;
            GlyphNames = glyphNames;
        }

        public Dictionary<int, int> CharacterToGlyph { get; }
        public Dictionary<int, string> GlyphNames { get; }

        /// <summary>Reads the cmap and post tables needed to verify registered Fluent icons.</summary>
        public static FluentFont Read(byte[] data)
        {
            var tables = ReadTables(data);
            if (!tables.TryGetValue("cmap", out var cmapOffset) || !tables.TryGetValue("post", out var postOffset))
                throw new InvalidDataException("The font has no cmap or post table.");
            return new FluentFont(ReadCharacterMap(data, cmapOffset), ReadGlyphNames(data, postOffset));
        }

        /// <summary>Locates the cmap and post tables and validates their byte ranges.</summary>
        private static Dictionary<string, int> ReadTables(byte[] data)
        {
            Require(data, 0, 12);
            var count = ReadUInt16(data, 4);
            Require(data, 12, count * 16);
            var tables = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < count; index++)
            {
                var entry = 12 + index * 16;
                var tag = Encoding.ASCII.GetString(data, entry, 4);
                var offset = ReadUInt32(data, entry + 8);
                var length = ReadUInt32(data, entry + 12);
                Require(data, offset, length);
                tables[tag] = checked((int)offset);
            }
            return tables;
        }

        /// <summary>Selects and reads a supported cmap format.</summary>
        private static Dictionary<int, int> ReadCharacterMap(byte[] data, int tableOffset)
        {
            var records = ReadUInt16(data, tableOffset + 2);
            Require(data, tableOffset + 4, records * 8);
            (int Offset, ushort Format)? chosen = null;
            for (var index = 0; index < records; index++)
            {
                var record = tableOffset + 4 + index * 8;
                var platform = ReadUInt16(data, record);
                var encoding = ReadUInt16(data, record + 2);
                var subtable = checked(tableOffset + (int)ReadUInt32(data, record + 4));
                var format = ReadUInt16(data, subtable);
                if ((platform, encoding, format) is (3, 10, 12) or (0, 4, 12) || chosen is null && format == 4)
                    chosen = (subtable, format);
            }

            if (chosen is not { } selected)
                throw new InvalidDataException("The font has no supported cmap format 4 or 12.");
            return selected.Format == 12
                ? ReadFormat12(data, selected.Offset)
                : ReadFormat4(data, selected.Offset);
        }

        /// <summary>Expands format 12 groups into character-to-glyph mappings.</summary>
        private static Dictionary<int, int> ReadFormat12(byte[] data, int offset)
        {
            var groupCount = ReadUInt32(data, offset + 12);
            Require(data, offset + 16, checked((int)groupCount * 12));
            var map = new Dictionary<int, int>();
            for (var index = 0; index < groupCount; index++)
            {
                var group = checked(offset + 16 + (int)index * 12);
                var start = ReadUInt32(data, group);
                var end = ReadUInt32(data, group + 4);
                var firstGlyph = ReadUInt32(data, group + 8);
                for (var code = start; code <= end; code++)
                    map[checked((int)code)] = checked((int)(firstGlyph + code - start));
            }
            return map;
        }

        /// <summary>Reads format 4 segments into character-to-glyph mappings.</summary>
        private static Dictionary<int, int> ReadFormat4(byte[] data, int offset)
        {
            var segmentCount = ReadUInt16(data, offset + 6) / 2;
            var endsOffset = offset + 14;
            var startsOffset = endsOffset + segmentCount * 2 + 2;
            var deltasOffset = startsOffset + segmentCount * 2;
            var rangesOffset = deltasOffset + segmentCount * 2;
            Require(data, endsOffset, segmentCount * 8 + 2);
            var map = new Dictionary<int, int>();
            for (var index = 0; index < segmentCount; index++)
            {
                var start = ReadUInt16(data, startsOffset + index * 2);
                var end = ReadUInt16(data, endsOffset + index * 2);
                var delta = ReadInt16(data, deltasOffset + index * 2);
                var rangeAddress = rangesOffset + index * 2;
                var range = ReadUInt16(data, rangeAddress);
                for (var code = (int)start; code <= end && code != 0xFFFF; code++)
                {
                    var glyph = range == 0
                        ? (code + delta) & 0xFFFF
                        : ReadUInt16(data, rangeAddress + range + 2 * (code - start));
                    if (range != 0 && glyph != 0) glyph = (glyph + delta) & 0xFFFF;
                    if (glyph != 0) map[code] = glyph;
                }
            }
            return map;
        }

        /// <summary>Resolves post version 2 glyph indices to their custom Pascal-string names.</summary>
        private static Dictionary<int, string> ReadGlyphNames(byte[] data, int offset)
        {
            if (ReadUInt32(data, offset) != 0x00020000)
                throw new InvalidDataException("The post table is not version 2.0.");
            var glyphCount = ReadUInt16(data, offset + 32);
            var namesOffset = offset + 34;
            Require(data, namesOffset, glyphCount * 2);
            var indices = new ushort[glyphCount];
            var customCount = 0;
            for (var glyph = 0; glyph < glyphCount; glyph++)
            {
                var nameIndex = ReadUInt16(data, namesOffset + glyph * 2);
                indices[glyph] = nameIndex;
                if (nameIndex >= 258) customCount = Math.Max(customCount, nameIndex - 257);
            }

            var stringsOffset = namesOffset + glyphCount * 2;
            var customNames = new string[customCount];
            for (var index = 0; index < customCount; index++)
            {
                Require(data, stringsOffset, 1);
                var length = data[stringsOffset++];
                Require(data, stringsOffset, length);
                customNames[index] = Encoding.Latin1.GetString(data, stringsOffset, length);
                stringsOffset += length;
            }

            var result = new Dictionary<int, string>();
            for (var glyph = 0; glyph < indices.Length; glyph++)
            {
                var nameIndex = indices[glyph];
                if (nameIndex >= 258) result[glyph] = customNames[nameIndex - 258];
            }
            return result;
        }

        /// <summary>Reads one big-endian 16-bit value after checking its bounds.</summary>
        private static ushort ReadUInt16(byte[] data, int offset)
        {
            Require(data, offset, 2);
            return BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, 2));
        }

        /// <summary>Reads one signed big-endian 16-bit value after checking its bounds.</summary>
        private static short ReadInt16(byte[] data, int offset)
        {
            Require(data, offset, 2);
            return BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(offset, 2));
        }

        /// <summary>Reads one big-endian 32-bit value after checking its bounds.</summary>
        private static uint ReadUInt32(byte[] data, int offset)
        {
            Require(data, offset, 4);
            return BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, 4));
        }

        /// <summary>Rejects byte ranges outside the font data.</summary>
        private static void Require(byte[] data, int offset, int length)
        {
            if (offset < 0 || length < 0 || offset > data.Length - length)
                throw new InvalidDataException("The font table has invalid offsets.");
        }

        /// <summary>Validates unsigned table ranges before narrowing them to array indices.</summary>
        private static void Require(byte[] data, uint offset, uint length)
        {
            if (offset > int.MaxValue || length > int.MaxValue) throw new InvalidDataException("The font table is too large.");
            Require(data, (int)offset, (int)length);
        }
    }
}
