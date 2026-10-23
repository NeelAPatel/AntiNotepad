"""Sync the live board with docs/feedback/TRACKER.md.

Usage: python .coord/tools/tracker-sync.py
1. Rewrites the embedded snapshot in docs/tracker-board.html (the local file then shows the current tracker on open or Refresh).
2. Writes the db body to .coord/scratch/tracker-db.json and prints its path. The Claude manager then writes it to the
   claude.ai board: ArtifactData get tracker/current (for its version), then set with file_path and if_version.
"""
import datetime, json, pathlib, re

root = pathlib.Path(__file__).resolve().parents[2]
md = (root / "docs/feedback/TRACKER.md").read_text(encoding="utf-8")
now = datetime.datetime.now(datetime.timezone.utc).isoformat(timespec="seconds")

# Only the tables feed the board; drop the file's title and preamble so the seed matches what the parser reads.
seed = md[md.index("\n## ") + 1:] if "\n## " in md else md
board = root / "docs/tracker-board.html"
html = board.read_text(encoding="utf-8")
pattern = re.compile(r'(<script type="text/markdown" id="seed" data-updated=")[^"]*(">\n).*?(</script>)', re.S)
if not pattern.search(html):
    raise SystemExit("seed block not found in docs/tracker-board.html")
html = pattern.sub(lambda m: m.group(1) + now + m.group(2) + seed.rstrip("\n") + "\n" + m.group(3), html, count=1)
board.write_text(html, encoding="utf-8")

out = root / ".coord/scratch/tracker-db.json"
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps({"markdown": md, "updatedAt": now}, ensure_ascii=False), encoding="utf-8")
print(out)
