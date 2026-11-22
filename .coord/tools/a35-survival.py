"""Lists lines an earlier A35 step added that a later ref no longer has (silent reverts).

Usage: python -I .coord/tools/a35-survival.py [target-ref] [base-branch]
Defaults: target HEAD, base docs/A35-ux-pass. Checks every first-parent merge on the base whose subject starts
with A35, product code only. A missing line is not always wrong (a later step may own it); each needs a reason.
"""
import subprocess
import sys


def git(*args):
    return subprocess.run(["git", *args], capture_output=True, text=True, encoding="utf-8", check=True).stdout


target = sys.argv[1] if len(sys.argv) > 1 else "HEAD"
base = sys.argv[2] if len(sys.argv) > 2 else "docs/A35-ux-pass"
merges = [l.split(" ", 1) for l in git("log", "--first-parent", "--merges", "--format=%H %s", base).splitlines()]
merges = [(h, s) for h, s in merges if s.startswith("A35")]
missing = 0
for sha, subject in reversed(merges):
    added = {}
    current = None
    for line in git("diff", "-U0", f"{sha}^1", sha, "--", "product").splitlines():
        if line.startswith("+++ "):
            current = line[6:] if line.startswith("+++ b/") else None
        elif current and line.startswith("+") and line[1:].strip() not in ("", "{", "}", "};", ");", "]", "],"):
            added.setdefault(current, []).append(line[1:])
    for path, lines in added.items():
        try:
            text = git("show", f"{target}:{path}")
        except subprocess.CalledProcessError:
            print(f"GONE  {subject.split(':')[0]}  {path} (file removed)")
            missing += 1
            continue
        norm = lambda s: s.strip().rstrip(",;")
        have = set(norm(l) for l in text.splitlines())
        for l in lines:
            if norm(l) not in have:
                print(f"LOST  {subject.split(':')[0]}  {path}: {l.strip()[:110]}")
                missing += 1
print(f"SURVIVAL: {len(merges)} step merges checked against {target}; {missing} added line(s) missing")
sys.exit(1 if missing else 0)
