import sys, difflib, os
sys.stdout.reconfigure(encoding="utf-8")
R = sys.argv[1]
for name in ("WorkspaceHostCore.cs", "WorkspacePilotFieldViewModel.cs"):
    base = {"WorkspaceHostCore.cs": "WHC_base.cs", "WorkspacePilotFieldViewModel.cs": "FVM_base.cs"}[name]
    a = open(os.path.join(R, base), encoding="utf-8").read().split("\n")
    b = open(os.path.join(R, "green", name), encoding="utf-8").read().split("\n")
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes():
        if tag != "equal":
            print(name, tag, "base", i1 + 1, i2, "green", j1 + 1, j2)
        elif i1 + 1 <= 1201 <= i2 and name == "WorkspaceHostCore.cs":
            print(name, "equal block containing base 1201:", i1 + 1, i2, "->", j1 + 1, j2, "base1201->green", 1201 - (i1 + 1) + j1 + 1)
