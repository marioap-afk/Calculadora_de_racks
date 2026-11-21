"""CA-T4b item 9: UI test classes outside the UI filters of the precondition 6 that name or sweep what the GREEN of PB-T4b changes in
src/RackCad.UI (NumericFieldValidation.cs, WorkspacePanelView.xaml, WorkspacePilotFieldViewModel.cs) or read the UI tree as text
(read-only: git grep / git show at the revision).

Usage: python -I ir4_uiout.py <repo> <rev> <package-v48.md> <out.txt>
"""
import re
import subprocess
import sys

repo, rev, pkg_p, out_p = sys.argv[1:5]
L = []


def git(*a):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True).stdout.decode("utf-8", "replace").replace("\r\n", "\n")


pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
start = pkg.index("**`RequiredTests`** (38 heredados + 8 nuevos")
filters = re.findall(r"^\| \d+ \| `tests/RackCad\.UI\.Tests/RackCad\.UI\.Tests\.csproj` \| `FullyQualifiedName~([\w.]+)` \|", pkg[start:], re.M)
filters = sorted(set(f if not f.startswith("RackCad.UI.Tests.FullPanelCopy") else "RackCad.UI.Tests.FullPanelCopy" for f in filters))
L.append("UI filters of the table (copies folded into one): %d" % len(filters))
L.extend("  " + f for f in filters)
files = [f for f in git("ls-tree", "-r", "--name-only", rev, "--", "tests/RackCad.UI.Tests").split("\n") if f.endswith(".cs")]
PAT = {
    "names-changed": re.compile(r"NumericFieldValidation|WorkspacePanelView|WorkspacePilotFieldViewModel"),
    "sweeps-ui-text": re.compile(r'\*\.xaml|"\.xaml"|EnumerateFiles\([^)]*src|Directory\.(?:Get|Enumerate)Files|RackCad\.UI[/\\"]|"src"'),
}
hits = []
for f in files:
    text = git("show", "%s:%s" % (rev, f))
    kinds = [k for k, p in PAT.items() if p.search(text)]
    if not kinds:
        continue
    classes = re.findall(r"^\s*(?:public |internal )?(?:sealed |static |partial )*class (\w+)", text, re.M)
    test_classes = [c for c in classes if re.search(r"class %s\b[\s\S]*?\[(?:Fact|Theory|StaFact|WpfFact)" % c, text)]
    for c in test_classes or classes[:1]:
        fq = "RackCad.UI.Tests." + c
        sel = [x for x in filters if x in fq + ".X"]
        hits.append((f, c, kinds, sel))
L.append("UI test files that name a changed UI file or sweep the UI tree: %d classes" % len(hits))
for f, c, kinds, sel in hits:
    L.append("  %-60s %-50s %-28s %s" % (f.replace("tests/RackCad.UI.Tests/", ""), c, ",".join(kinds), ("in " + sel[0]) if sel else "OUTSIDE the filters"))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
