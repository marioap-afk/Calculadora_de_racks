"""CA-T4c item 1: UI test classes outside the UI filters of the v5.0 RequiredTests that name what the GREEN of PB-T4c changes in the UI
(the window's PendingWork and its questions, the WORKSPACE mode, ForDraft, the panel view and its field view model) or sweep the UI tree
(read-only: git show at the revision).

Usage: python -I ir4_t4c_ui.py <repo> <rev> <package-v50.md> <out.txt>
"""
import re
import subprocess
import sys

repo, rev, pkg_p, out_p = sys.argv[1:5]


def git(*a):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True).stdout.decode("utf-8", "replace").replace("\r\n", "\n")


pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
start = pkg.index("**`RequiredTests`** (76 heredados + 9 nuevos = 85)")
rows = re.findall(r"^\| (\d+) \| `([^`]+)` \| `FullyQualifiedName~([\w.]+)` \| ([^|]+) \| (true|false) \|$", pkg[start:start + 40000], re.M)
ui = [r for r in rows if "UI.Tests" in r[1]]
L = ["rows %d; UI filters %d: %s" % (len(rows), len(ui), [r[2].replace("RackCad.UI.Tests.", "") for r in ui])]
filters = [r[2] for r in ui]
PAT = {
    "pendingwork": re.compile(r"PendingWork|EditorClosePolicy|cambios de m\S+dulo sin confirmar|cambios sin aplicar al borrador"),
    "workspace-mode": re.compile(r"PushBackEditorMode\.Workspace|ForDraft\(|PushBackDraftEditorLaunch"),
    "panel": re.compile(r"WorkspacePanelView|WorkspacePilotFieldViewModel|BrowserFieldEdit"),
    "sweep": re.compile(r'\*\.xaml|Directory\.(?:Get|Enumerate)Files|EnumerateFiles\('),
}
files = [f for f in git("ls-tree", "-r", "--name-only", rev, "--", "tests/RackCad.UI.Tests").split("\n") if f.endswith(".cs")]
outside = []
for f in files:
    text = git("show", "%s:%s" % (rev, f))
    kinds = [k for k, p in PAT.items() if p.search(text)]
    if not kinds:
        continue
    for c in re.findall(r"^\s*(?:public |internal )?(?:sealed |static |partial )*class (\w+)", text, re.M):
        if not re.search(r"class %s\b[\s\S]*?\[(?:Fact|Theory|StaFact|WpfFact)" % c, text):
            continue
        fq = "RackCad.UI.Tests." + c
        sel = [x for x in filters if x in fq + ".X"]
        if not sel:
            outside.append((f, c, kinds))
L.append("test classes outside the UI filters that match: %d" % len(outside))
for f, c, kinds in outside:
    L.append("  %-62s %-52s %s" % (f.replace("tests/RackCad.UI.Tests/", ""), c, ",".join(kinds)))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
