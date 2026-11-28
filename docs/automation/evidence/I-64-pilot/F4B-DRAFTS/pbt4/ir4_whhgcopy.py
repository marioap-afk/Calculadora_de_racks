"""CA-T4b item 5: the probe copy of WHHG (WorkspaceHostHardeningGuardFull.cs) against the original at the revision: the lines of the copy
that are not lines of the original, and whether the test body is the original's (read-only).

Usage: python -I ir4_whhgcopy.py <repo> <rev> <copy.cs> <out.txt>
"""
import difflib
import subprocess
import sys

repo, rev, copy_p, out_p = sys.argv[1:5]
orig = subprocess.run(["git", "-C", repo, "show", "%s:tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs" % rev],
                      capture_output=True).stdout.decode("utf-8").replace("\r\n", "\n").split("\n")
copy = open(copy_p, encoding="utf-8-sig").read().replace("\r\n", "\n").split("\n")
o_set = set(l.strip() for l in orig)
L = ["copy lines %d; original lines %d" % (len(copy), len(orig))]
extra = [(i + 1, l) for i, l in enumerate(copy) if l.strip() and l.strip() not in o_set]
L.append("copy lines that are not lines of the original: %d" % len(extra))
L.extend("  :%d %s" % (i, l[:200]) for i, l in extra)
# the test body: from [Fact] to the closing brace, in both
def body(lines, name):
    d = next(i for i, l in enumerate(lines) if ("void " + name + "(") in l)
    s = d - 1
    e = next(i for i in range(d, len(lines)) if lines[i].strip() == "}" and len(lines[i]) - len(lines[i].lstrip()) == len(lines[d]) - len(lines[d].lstrip()))
    return [l.strip() for l in lines[s:e + 1]]
name = "NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept"
bo, bc = body(orig, name), body(copy, name)
L.append("test body equal to the original (trimmed): %s" % (bo == bc))
L.extend("  " + l for l in difflib.unified_diff(bo, bc, lineterm="", n=0))
# the self-test of the guard: is it in the copy?
L.append("self-test TheGuardOfTheLiteralsTrips... in the copy: %s" % any("TheGuardOfTheLiteralsTrips" in l for l in copy))
L.append("tests in the copy: %s" % [l.strip() for l in copy if l.strip().startswith("public void ")])
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
