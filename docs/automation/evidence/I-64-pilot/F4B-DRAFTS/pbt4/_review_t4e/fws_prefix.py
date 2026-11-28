import sys, json, os
sys.stdout.reconfigure(encoding="utf-8")
R = sys.argv[1]
c = json.load(open(os.path.join(R, "pbt4d-contract.json"), encoding="utf-8"))
f = c["ForbiddenWriteScope"]
scope = [
    "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
    "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs",
    "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs",
    "src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs",
    "tests/RackCad.Tests/PushBackEditAllReloadTests.cs",
    "tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs",
]
for s in scope:
    hits = [x for x in f if s == x or (x.endswith("/") and s.startswith(x))]
    print(s, "covered by forbidden:", hits)
print("src entries", sum(1 for x in f if x.startswith("src/")), "tests entries", sum(1 for x in f if x.startswith("tests/")), "other", [x for x in f if not x.startswith("src/") and not x.startswith("tests/")])
