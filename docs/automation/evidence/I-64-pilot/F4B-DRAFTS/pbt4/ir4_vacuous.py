"""CA-T4b item 7: self-tests outside the 478 list that call, with Assert.NotEmpty / NotNull / Contains, the same checking helper that a C-5
test of the list calls with Assert.Empty on the real text. If the real text of the GREEN already trips the helper (which is why the C-5 test is
in the list), such a self-test can pass in the GREEN whatever its mutation does. Text only (read-only: git show).

Usage: python -I ir4_vacuous.py <repo> <rev> <list-478.tsv> <out.txt>
"""
import re
import subprocess
import sys

repo, rev, list_p, out_p = sys.argv[1:5]
L = []


def show(path):
    return subprocess.run(["git", "-C", repo, "show", "%s:%s" % (rev, path)], capture_output=True).stdout.decode("utf-8").replace("\r\n", "\n")


def strip_strings(line):
    line = re.sub(r'@"(?:[^"]|"")*"', '""', line)
    line = re.sub(r'"(?:\\.|[^"\\])*"', '""', line)
    line = re.sub(r"'(?:\\.|[^'\\])'", "''", line)
    return re.sub(r"//.*", "", line)


def tests_of(text):
    lines = text.split("\n")
    out = {}
    for i, l in enumerate(lines):
        m = re.search(r"\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(", l)
        if not m:
            continue
        s = i
        while s > 0 and re.match(r"^\s*(\[|//)", lines[s - 1]):
            s -= 1
        if not any(re.match(r"^\s*\[(Fact|Theory)", lines[k]) for k in range(s, i)):
            continue
        depth, seen = 0, False
        for j in range(i, len(lines)):
            c = strip_strings(lines[j])
            depth += c.count("{") - c.count("}")
            seen = seen or "{" in c
            if seen and depth == 0:
                break
        out[m.group(1)] = (s + 1, j + 1, "\n".join(lines[s:j + 1]))
    return out


rows = [l.split("\t") for l in open(list_p, encoding="utf-8").read().replace("\r\n", "\n").split("\n")[1:] if l]
c5 = {}
for r in rows:
    if r[0] == "PB-T4b C-5" and not r[1].startswith("("):
        c5.setdefault(r[1], set()).add(r[2])
listed_all = {}
for r in rows:
    listed_all.setdefault(r[1].replace("(PB-T4a) ", ""), set()).add(r[2])

CALL = re.compile(r"Assert\.(Empty|NotEmpty|Single|Contains|DoesNotContain|True|False|Null|NotNull)\(\s*(?:\w+\s*,\s*)?(\w+)\(")
for path in sorted(c5):
    text = show(path)
    tests = tests_of(text)
    empty_helpers = {}
    for name in sorted(c5[path]):
        if name not in tests:
            L.append("%s: listed test %s not found" % (path, name))
            continue
        for kind, fn in CALL.findall(tests[name][2]):
            if kind in ("Empty", "True", "Null", "DoesNotContain"):
                empty_helpers.setdefault(fn, set()).add(name)
    L.append("== %s: %d listed C-5 tests; helpers checked for emptiness/truth on them: %s" % (
        path.split("/")[-1], len(c5[path]), sorted(empty_helpers)))
    for name, (s, e, body) in sorted(tests.items(), key=lambda kv: kv[1][0]):
        if name in listed_all.get(path, set()):
            continue
        hits = sorted(set(fn for kind, fn in CALL.findall(body) if kind in ("NotEmpty", "NotNull", "False", "Contains", "Single") and fn in empty_helpers))
        if hits:
            real = bool(re.search(r"Src\.Code\(|Host\(\)|File\.ReadAllText|Code\(\w*Path\)", body))
            pre = bool(re.search(r"Assert\.Contains\(\s*from\s*,|Assert\.Contains\(\s*\w+\s*,\s*(host|code|text|source)\b", body))
            baseline = bool(re.search(r"Assert\.Empty\(\s*(%s)\(" % "|".join(map(re.escape, hits)), body))
            L.append("  outside the list: %s (:%d-%d) calls %s; real text %s; Contains precondition %s; empty baseline inside %s" % (
                name, s, e, hits, real, pre, baseline))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
