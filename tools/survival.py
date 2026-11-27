"""Lists lines an earlier A35 step added that a later ref no longer has (silent reverts).

Usage: python -I tools/survival.py [target-ref] [base-branch] [subject-prefix]
Checks first-parent merges on base whose subject starts with the prefix, product code only.
A missing line is not always wrong; each needs a reason.
"""
import subprocess
import sys
import contextlib
import os
import shutil
import stat
import uuid
from pathlib import Path


@contextlib.contextmanager
def scratch_directory(parent):
    path = parent / ("survival-" + uuid.uuid4().hex)
    path.mkdir(mode=0o777)
    try:
        yield str(path)
    finally:
        def clear_readonly(function, name, error):
            os.chmod(name, stat.S_IWRITE)
            function(name)
        shutil.rmtree(path, onexc=clear_readonly)


def selftest():
    scratch = Path(__file__).resolve().parent.parent / "build"
    scratch.mkdir(exist_ok=True)
    with scratch_directory(scratch) as folder:
        def run(*args):
            return subprocess.run(args, cwd=folder, capture_output=True, text=True, check=True).stdout.strip()
        run("git", "init", "-q")
        run("git", "config", "user.name", "Test")
        run("git", "config", "user.email", "test@example.invalid")
        run("git", "config", "core.quotepath", "true")
        root = Path(folder)
        (root / "product").mkdir()
        (root / "product" / "a.txt").write_text("initial\n", encoding="utf-8")
        run("git", "add", ".")
        run("git", "commit", "-qm", "initial")
        base = run("git", "branch", "--show-current")
        run("git", "checkout", "-qb", "child")
        (root / "product" / "a.txt").write_text("initial\nadded line\n", encoding="utf-8")
        run("git", "commit", "-qam", "child")
        run("git", "checkout", "-q", base)
        run("git", "merge", "--no-ff", "-qm", "T1-child: merge", "child")
        script = str(Path(__file__).resolve())
        run(sys.executable, "-I", script, "HEAD", "HEAD", "T1")
        run("git", "checkout", "-q", "child")
        (root / "product" / "café.cs").write_text("initial\nadded unicode line\n", encoding="utf-8")
        run("git", "add", ".")
        run("git", "commit", "-qm", "unicode child")
        run("git", "checkout", "-q", base)
        run("git", "merge", "--no-ff", "-qm", "T1-unicode: merge", "child")
        (root / "product" / "café.cs").write_text("initial\n", encoding="utf-8")
        run("git", "add", ".")
        run("git", "commit", "-qm", "unicode remove")
        unicode_result = subprocess.run([sys.executable, "-I", script, "HEAD", "HEAD", "T1"], cwd=folder, capture_output=True, text=True)
        if unicode_result.returncode != 1 or "LOST" not in unicode_result.stdout:
            raise RuntimeError("non-ASCII path was not checked")
        (root / "product" / "a.txt").write_text("initial\n", encoding="utf-8")
        run("git", "config", "diff.noprefix", "true")
        run("git", "commit", "-qam", "remove")
        result = subprocess.run([sys.executable, "-I", script, "HEAD", "HEAD", "T1"], cwd=folder, capture_output=True, text=True)
        if result.returncode != 1 or "LOST" not in result.stdout:
            raise RuntimeError("missing line was not detected")
    print("SURVIVAL SELFTEST PASS")


if len(sys.argv) > 1 and sys.argv[1] == "--selftest":
    selftest()
    sys.exit(0)


def git(*args):
    return subprocess.run(["git", "-c", "core.quotepath=false", *args], capture_output=True, text=True, encoding="utf-8", check=True).stdout


target = sys.argv[1] if len(sys.argv) > 1 else "HEAD"
base = sys.argv[2] if len(sys.argv) > 2 else "dev"
prefix = sys.argv[3] if len(sys.argv) > 3 else ""
merges = [l.split(" ", 1) for l in git("log", "--first-parent", "--merges", "--format=%H %s", base).splitlines()]
merges = [(h, s) for h, s in merges if s.startswith(prefix)]
missing = 0
for sha, subject in reversed(merges):
    added = {}
    current = None
    for line in git("diff", "--src-prefix=a/", "--dst-prefix=b/", "-U0", f"{sha}^1", sha, "--", "product").splitlines():
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
