import sys, json, os
sys.stdout.reconfigure(encoding="utf-8")
R = sys.argv[1]
c = json.load(open(os.path.join(R, "pbt4d-contract.json"), encoding="utf-8"))
f = c["ForbiddenWriteScope"]
a = c["AllowedWriteScope"]
print("pbt4d allowed", len(a))
print("pbt4d forbidden", len(f), "non-tests", [x for x in f if not str(x).startswith("tests/")][:10])
scope = [
    "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
    "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs",
    "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs",
    "src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs",
    "tests/RackCad.Tests/PushBackEditAllReloadTests.cs",
    "tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs",
]
for s in scope:
    print(s, "in pbt4d forbidden:", s in f, "in pbt4d allowed:", s in a)
new_t4d = [x for x in a if str(x).startswith("tests/") and "RetirePilot" in str(x)]
print("pbt4d new RetirePilot files in allowed:", new_t4d)
print("of them already forbidden:", [x for x in new_t4d if x in f])
print("dups in forbidden", len(f) - len(set(f)))
