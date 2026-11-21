"""CA-T4b item 2 (D-S5): the eight halves of the annex V48-A against the whole Theory of the measurement-mode copy (copies_probe_v48) and
against the original at the revision; and StatusNamesRed/Green against StatusNames (read-only: git show and local reads).

Usage: python -I ir4_split.py <repo> <rev> <package-v48.md> <copies-dir> <out.txt>
"""
import difflib
import re
import subprocess
import sys

repo, rev, pkg_p, copies_dir, out_p = sys.argv[1:6]
L = []
SPECS = [
    ("WorkspaceLifecycleHost", "tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs",
     "EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively", "inline",
     ["a-check-that-finds-a-conflict", "a-check-that-recovers", "a-reload"]),
    ("WorkspacePilotEditGuard", "tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs",
     "TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance", "inline",
     ["private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();"]),
    ("WorkspacePilotPrepare", "tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs",
     "ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing", "member", None),
    ("WorkspacePilotPrepare", "tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs",
     "KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading", "member", None),
]


def show(path):
    return subprocess.run(["git", "-C", repo, "show", "%s:%s" % (rev, path)], capture_output=True).stdout.decode("utf-8").replace("\r\n", "\n")


def strip_strings(line):
    line = re.sub(r'@"(?:[^"]|"")*"', '""', line)
    line = re.sub(r'"(?:\\.|[^"\\])*"', '""', line)
    line = re.sub(r"'(?:\\.|[^'\\])'", "''", line)
    return re.sub(r"//.*", "", line)


def method(text, name):
    lines = text.split("\n")
    decl = [i for i, l in enumerate(lines) if re.search(r"\bpublic\s+(?:async\s+)?(?:void|Task)\s+%s\s*\(" % re.escape(name), l)]
    assert len(decl) == 1, (name, decl)
    d = decl[0]
    s = d
    while s > 0 and re.match(r"^\s*(\[|//)", lines[s - 1]):
        s -= 1
    depth, seen, e = 0, False, d
    for i in range(d, len(lines)):
        c = strip_strings(lines[i])
        depth += c.count("{") - c.count("}")
        seen = seen or "{" in c
        if seen and depth == 0:
            e = i
            break
    return lines[s:e + 1]


def code_block_after(pkg, heading):
    i = pkg.index(heading)
    a = pkg.index("```csharp\n", i) + len("```csharp\n")
    b = pkg.index("\n```", a)
    return pkg[a:b].split("\n")


INLINE = re.compile(r'^\s*\[InlineData\(\s*("(?:\\.|[^"\\])*"|@"(?:[^"]|"")*")')


def first_arg(line):
    m = INLINE.match(line)
    if not m:
        return None
    lit = m.group(1)
    if lit.startswith('@"'):
        return lit[2:-1].replace('""', '"')
    return re.sub(r"\\(.)", lambda x: x.group(1), lit[1:-1])


pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
for stem, orig_path, name, kind, red_keys in SPECS:
    L.append("== %s.%s (%s)" % (stem, name, kind))
    orig = method(show(orig_path), name)
    copy_path = copies_dir + "/" + orig_path.replace("Tests.cs", "Full.cs")
    probe = method(open(copy_path, encoding="utf-8-sig").read().replace("\r\n", "\n"), name)
    red = code_block_after(pkg, "**`FullPanelCopy%sRedTests.%s`**" % (stem, name))
    green = code_block_after(pkg, "**`FullPanelCopy%sGreenTests.%s`**" % (stem, name))
    # original -> probe copy (M only, for the mechanical ones)
    d = [l for l in difflib.unified_diff(orig, probe, lineterm="", n=0) if l[:1] in "+-" and l[:3] not in ("+++", "---")]
    L.append("  original -> probe copy: %d changed lines" % len(d))
    L.extend("    " + l[:200] for l in d)
    comment = [l for l in red + green if "(D-S5): this Theory is split by rows" in l]
    L.append("  split comments: red %d, green %d" % (sum(1 for l in red if l in comment), sum(1 for l in green if l in comment)))
    rr = [l for l in red if l not in comment]
    gg = [l for l in green if l not in comment]
    if kind == "member":
        rr = [l.replace("nameof(StatusNamesRed)", "nameof(StatusNames)") for l in rr]
        gg = [l.replace("nameof(StatusNamesGreen)", "nameof(StatusNames)") for l in gg]
        L.append("  MemberData: red %s, green %s" % ([l.strip() for l in red if "MemberData" in l], [l.strip() for l in green if "MemberData" in l]))
        L.append("  red half == probe (with StatusNames): %s; green half == probe: %s" % (rr == probe, gg == probe))
    else:
        body_p = [l for l in probe if first_arg(l) is None]
        body_r = [l for l in rr if first_arg(l) is None]
        body_g = [l for l in gg if first_arg(l) is None]
        rows_p = [l for l in probe if first_arg(l) is not None]
        rows_r = [l for l in rr if first_arg(l) is not None]
        rows_g = [l for l in gg if first_arg(l) is not None]
        L.append("  body (all but the InlineData rows): red == probe %s, green == probe %s" % (body_r == body_p, body_g == body_p))
        L.append("  rows: probe %d, red %d, green %d; red+green == probe as a multiset %s; each half keeps the probe order %s" % (
            len(rows_p), len(rows_r), len(rows_g), sorted(rows_r + rows_g) == sorted(rows_p),
            [l for l in rows_p if l in rows_r] == rows_r and [l for l in rows_p if l in rows_g] == rows_g))
        L.append("  red first arguments == RED_KEYS: %s" % (sorted(first_arg(l) for l in rows_r) == sorted(red_keys)))
        L.append("  any row both halves: %s" % sorted(set(rows_r) & set(rows_g)))
        # the rows of the probe against the original (no row changed)
        rows_o = [l for l in orig if first_arg(l) is not None]
        L.append("  rows of the probe == rows of the original: %s" % (rows_o == rows_p))

# StatusNames: original vs the new members (text) and STATUS_RED
wpp = show("tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs")
sn = method_text = None
m = re.search(r"public static TheoryData<string> StatusNames\(\)\s*\{(.*?)\n    \}", wpp, re.S)
L.append("== StatusNames in the original: %s" % (m is not None))
if m:
    L.append("  " + " | ".join(x.strip() for x in m.group(1).strip().split("\n") if x.strip()))
    names = re.findall(r'"(\w+)"', m.group(1))
    enum_names = re.findall(r"nameof\(\w+\.(\w+)\)", m.group(1))
    L.append("  string rows: %s; nameof rows: %s" % (names, enum_names))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
