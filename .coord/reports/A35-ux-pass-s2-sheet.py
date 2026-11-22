"""Build the A35 icon contact sheet: every icon the app draws, outlined from the font files themselves.

Usage: python -I A35-ux-pass-s2-sheet.py <icons.tsv> <bundled.ttf> <segoe.ttf> <out.html>
icons.tsv columns: code, font (S or F), name, meaning, size, screens. Each cell shows the glyph in the font the app uses
and, beside it, what the same code draws in the other font (the A29 failure mode).
"""
import csv, html, struct, sys
sys.path.insert(0, __import__("os").path.dirname(__file__))
fontcodes = __import__("A35-ux-pass-s2-fontcodes")


class Font:
    def __init__(self, path):
        self.data = open(path, "rb").read()
        t = fontcodes.tables(self.data)
        self.t = t
        self.cmap = fontcodes.cmap(self.data, t["cmap"][0])
        self.names = fontcodes.post_names(self.data, t["post"][0]) if "post" in t else {}
        head = t["head"][0]
        self.upm = struct.unpack(">H", self.data[head + 18:head + 20])[0]
        self.long_loca = struct.unpack(">h", self.data[head + 50:head + 52])[0] == 1
        hhea = t["hhea"][0]
        self.ascent, self.descent = struct.unpack(">hh", self.data[hhea + 4:hhea + 8])

    def loca(self, gid):
        off = self.t["loca"][0]
        if self.long_loca:
            a, b = struct.unpack(">II", self.data[off + 4 * gid:off + 4 * gid + 8])
        else:
            a, b = (2 * v for v in struct.unpack(">HH", self.data[off + 2 * gid:off + 2 * gid + 4]))
        return a, b

    def contours(self, gid, dx=0, dy=0):
        a, b = self.loca(gid)
        if a == b:
            return []
        p = self.t["glyf"][0] + a
        d = self.data
        n = struct.unpack(">h", d[p:p + 2])[0]
        p += 10
        if n < 0:
            out = []
            while True:
                flags, child = struct.unpack(">HH", d[p:p + 4]); p += 4
                if flags & 1:
                    x, y = struct.unpack(">hh", d[p:p + 4]); p += 4
                else:
                    x, y = struct.unpack(">bb", d[p:p + 2]); p += 2
                if flags & 8: p += 2
                elif flags & 0x40: p += 4
                elif flags & 0x80: p += 8
                out += self.contours(child, dx + x, dy + y)
                if not flags & 0x20:
                    return out
        ends = struct.unpack(f">{n}H", d[p:p + 2 * n]); p += 2 * n
        ilen = struct.unpack(">H", d[p:p + 2])[0]; p += 2 + ilen
        count = ends[-1] + 1 if n else 0
        flags = []
        while len(flags) < count:
            f = d[p]; p += 1
            flags.append(f)
            if f & 8:
                r = d[p]; p += 1
                flags += [f] * r
        def coords(short, same):
            nonlocal p
            vals, v = [], 0
            for f in flags:
                if f & short:
                    delta = d[p]; p += 1
                    v += delta if f & same else -delta
                elif not f & same:
                    v += struct.unpack(">h", d[p:p + 2])[0]; p += 2
                vals.append(v)
            return vals
        xs = coords(2, 16)
        ys = coords(4, 32)
        out, start = [], 0
        for e in ends:
            out.append([(xs[i] + dx, ys[i] + dy, flags[i] & 1) for i in range(start, e + 1)])
            start = e + 1
        return out

    def path(self, code):
        gid = self.cmap.get(code)
        if not gid:
            return None
        parts = []
        for pts in self.contours(gid):
            if not pts:
                continue
            k = next((i for i, q in enumerate(pts) if q[2]), None)
            if k is None:
                a, b = pts[0], pts[1]
                pts = [((a[0] + b[0]) / 2, (a[1] + b[1]) / 2, 1)] + pts
                k = 0
            pts = pts[k:] + pts[:k]
            fy = lambda y: self.ascent - y
            parts.append(f"M{pts[0][0]} {fy(pts[0][1])}")
            i, m = 1, len(pts)
            while i <= m:
                q = pts[i % m]
                if q[2]:
                    parts.append(f"L{q[0]} {fy(q[1])}")
                    i += 1
                else:
                    r = pts[(i + 1) % m]
                    end = r if r[2] else ((q[0] + r[0]) / 2, (q[1] + r[1]) / 2, 1)
                    parts.append(f"Q{q[0]} {fy(q[1])} {end[0]} {fy(end[1])}")
                    i += 2 if r[2] else 1
            parts.append("Z")
        return " ".join(parts)

    def name(self, code):
        gid = self.cmap.get(code)
        return self.names.get(gid, "") if gid else ""


def svg(font, code, px):
    p = font.path(code)
    if p is None:
        return f'<span class="missing" style="width:{px}px;height:{px}px">none</span>'
    h = font.ascent - font.descent
    return (f'<svg width="{px}" height="{px}" viewBox="0 0 {font.upm} {h}" aria-hidden="true">'
            f'<path d="{p}" fill="currentColor"/></svg>')


def main(icons_path, bundled_path, segoe_path, out):
    fonts = {"F": Font(bundled_path), "S": Font(segoe_path)}
    label = {"F": "Fluent System Icons (bundled)", "S": "Segoe Fluent Icons (system)"}
    rows = list(csv.DictReader(open(icons_path, encoding="utf-8"), delimiter="\t"))
    cards = []
    for r in rows:
        code, f = int(r["code"], 16), r["font"]
        other = "S" if f == "F" else "F"
        px = int(r["size"])
        own = fonts[f]
        cards.append(f"""<article class="card" data-font="{f}">
  <div class="faces"><div class="face">{svg(own, code, px)}<small>{px} px</small></div>
  <div class="face big">{svg(own, code, 48)}<small>48 px</small></div>
  <div class="face other" title="Same code in {label[other]}">{svg(fonts[other], code, 32)}<small>other font</small></div></div>
  <h3>{html.escape(r["name"])}</h3>
  <p>{html.escape(r["meaning"])}</p>
  <p class="meta">U+{code:04X} · {f} · {html.escape(own.name(code) or "no glyph name in font")}</p>
  <p class="meta">{html.escape(r["screens"])}</p>
</article>""")
    page = f"""<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>A35 icon contact sheet</title>
<style>
:root {{ --bg:#f3f3f3; --card:#fff; --ink:#1b1b1b; --muted:#5f5f5f; --line:#d6d6d6; --warn:#b3261e; }}
@media (prefers-color-scheme: dark) {{ :root {{ --bg:#1c1c1c; --card:#2a2a2a; --ink:#f2f2f2; --muted:#a8a8a8; --line:#3d3d3d; --warn:#ff8a80; }} }}
:root[data-theme=light] {{ --bg:#f3f3f3; --card:#fff; --ink:#1b1b1b; --muted:#5f5f5f; --line:#d6d6d6; }}
:root[data-theme=dark] {{ --bg:#1c1c1c; --card:#2a2a2a; --ink:#f2f2f2; --muted:#a8a8a8; --line:#3d3d3d; }}
body {{ margin:0; padding:16px; background:var(--bg); color:var(--ink); font:14px/20px "Segoe UI Variable Text","Segoe UI",sans-serif; }}
header {{ display:flex; flex-wrap:wrap; gap:8px 16px; align-items:center; margin-bottom:16px; }}
h1 {{ font-size:20px; margin:0; }} h3 {{ font-size:14px; margin:8px 0 0; }} p {{ margin:2px 0; }}
.meta {{ color:var(--muted); font-size:12px; line-height:16px; word-break:break-word; }}
.grid {{ display:grid; grid-template-columns:repeat(auto-fill,minmax(220px,1fr)); gap:12px; }}
.card {{ background:var(--card); border:1px solid var(--line); border-radius:8px; padding:12px; }}
.faces {{ display:flex; gap:12px; align-items:flex-end; }}
.face {{ display:flex; flex-direction:column; align-items:center; gap:4px; }} .face small {{ color:var(--muted); font-size:12px; }}
.other {{ color:var(--warn); opacity:.75; margin-left:auto; }}
.missing {{ display:inline-flex; align-items:center; justify-content:center; border:1px dashed var(--line); font-size:10px; color:var(--muted); }}
button {{ font:inherit; padding:4px 12px; border-radius:4px; border:1px solid var(--line); background:var(--card); color:var(--ink); }}
</style></head><body>
<header><h1>A35 icon contact sheet</h1>
<span class="meta">{len(rows)} icons. Each card shows the icon at the size the app draws it, at 48 px, and (red, right) what the same code draws in the other icon font.</span>
<button onclick="document.documentElement.dataset.theme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark'">Light / dark</button>
<button onclick="filter('all')">All</button><button onclick="filter('S')">Segoe only</button><button onclick="filter('F')">Bundled only</button></header>
<main class="grid">{"".join(cards)}</main>
<script>function filter(f){{document.querySelectorAll('.card').forEach(c=>c.hidden=f!=='all'&&c.dataset.font!==f)}}</script>
</body></html>"""
    open(out, "w", encoding="utf-8").write(page)
    print(f"wrote {out}: {len(rows)} icons")


if __name__ == "__main__":
    main(*sys.argv[1:5])
