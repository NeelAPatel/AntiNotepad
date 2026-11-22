"""Classify tools/check-design-tokens.ps1 -Inventory rows against the docs/design-scale.md R scale.

Usage: python -I A35-ux-pass-s1-classify.py <inventory.tsv> <out.tsv>
Prints per-category and per-file counts; writes one row per literal number with its class.
"""
import csv, re, sys
from collections import Counter

SPACING = [4, 8, 12, 16, 24, 32]
FONT = [12, 14, 16, 18, 28]
RADIUS = [2, 4, 8, 12]
OPACITY = [.3, .5, .8, .9]
LAYOUT_PROPS = ("Width", "Height", "MinWidth", "MinHeight", "MaxWidth", "MaxHeight")
NUM = re.compile(r"(?<![\w.])(\d+(?:\.\d+)?|\.\d+)(?![\w.(])")
STEP_NAME = {4: "XS 4", 8: "S 8", 12: "M 12", 16: "L 16", 24: "XL 24", 32: "2XL 32"}


def nearest(value, steps):
    return min(steps, key=lambda s: (abs(s - value), -s))


def argument_span(code, match):
    start = code.find(match)
    if start < 0:
        return match
    rest = code[start:]
    if "Thickness(" in match or "CornerRadius(" in match:
        depth = 0
        for i, ch in enumerate(rest):
            if ch == "(":
                depth += 1
            elif ch == ")":
                depth -= 1
                if depth == 0:
                    return rest[:i + 1]
        return rest
    return re.match(r"[^;,}]*", rest).group(0)


def classify_spacing(prop, value, span):
    if value == 0:
        return "zero", ""
    if "BorderThickness" in span:
        if value in (1, 2):
            return "stroke-on-scale", "Thin 1" if value == 1 else "Thick 2"
        return "stroke-off-scale", str(nearest(value, [1, 2]))
    if prop in LAYOUT_PROPS or value > 32:
        return ("layout-on-grid" if value % 4 == 0 else "layout-off-grid"), ""
    if value == 2:
        return "nudge", "Nudge 2"
    if value in SPACING:
        return "on-scale", STEP_NAME[value]
    if value == 1:
        return "off-scale", "0 or Nudge 2 (optical)"
    return "off-scale", STEP_NAME[nearest(value, SPACING)]


def main(src, out):
    rows = list(csv.DictReader(open(src, encoding="utf-8"), delimiter="\t"))
    results = []
    for r in rows:
        cat, file, line, match, code = r["category"], r["file"], r["line"], r["matched text"], r["code"]
        span = argument_span(code, match)
        head = re.match(r"\s*(\w+)", match)
        prop = head.group(1) if head else ""
        if cat == "timer":
            results.append([cat, file, line, span, "timer", "", ""])
        elif cat == "font-size" and ("GlyphIcon(" in span or "Kit.Text(" in span):
            results.append([cat, file, line, span, "scanner-false-positive", "size is a token; the number is a weight or width", ""])
        elif cat == "color" and ("Alpha" in span or re.search(r"FromArgb\(\s*alpha\b", span)):
            results.append([cat, file, line, span, "token-or-variable", "alpha from a token or variable; 0 is black", ""])
        elif cat == "color":
            results.append([cat, file, line, span, "off-scale", "Palette color", ""])
        elif cat == "radius" and "Shape.Round(" in span:
            results.append([cat, file, line, span, "round-of-size", "Round (size / 2); the size is layout", ""])
        else:
            nums = [float(n) for n in NUM.findall(span)]
            if not nums:
                results.append([cat, file, line, span, "token-only", "", ""])
            for n in nums:
                v = int(n) if n == int(n) else n
                if cat == "spacing":
                    cls, step = classify_spacing(prop, v, span)
                elif cat == "radius":
                    cls, step = ("zero", "") if v == 0 else ("on-scale", str(v)) if v in RADIUS else ("off-scale", str(nearest(v, RADIUS)))
                elif cat == "font-size":
                    cls, step = ("on-scale", str(v)) if v in FONT else ("off-scale", str(nearest(v, FONT)))
                elif cat == "opacity":
                    cls, step = ("on-scale", str(v)) if v in OPACITY else ("off-scale", str(nearest(v, OPACITY)))
                else:
                    cls, step = "unclassified", ""
                results.append([cat, file, line, span, cls, step, v])
    with open(out, "w", encoding="utf-8", newline="") as f:
        w = csv.writer(f, delimiter="\t")
        w.writerow(["category", "file", "line", "expression", "class", "nearest step", "literal"])
        w.writerows(results)
    print("category\tclass\tcount")
    for (c, k), n in sorted(Counter((r[0], r[4]) for r in results).items()):
        print(f"{c}\t{k}\t{n}")
    print("\nfile\toff-scale or off-grid literals")
    for f_, n in Counter(r[1] for r in results if r[4] in ("off-scale", "stroke-off-scale", "layout-off-grid")).most_common():
        print(f"{f_}\t{n}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
