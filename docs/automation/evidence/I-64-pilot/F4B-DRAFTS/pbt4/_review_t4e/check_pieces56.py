import sys, os, hashlib, re
sys.stdout.reconfigure(encoding="utf-8")
P = sys.argv[1]   # pbt4 folder
R = sys.argv[2]   # review folder with *_base.cs

def rd(p):
    with open(p, "rb") as f:
        return f.read().decode("utf-8")

green = rd(os.path.join(P, "pbt4e-green-v56.txt")).replace("\r\n", "\n")
red = rd(os.path.join(P, "pbt4e-red-v56.txt")).replace("\r\n", "\n")
base = {
    "src/RackCad.Application/Workspace/WorkspaceHostCore.cs": "WHC_base.cs",
    "src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs": "FVM_base.cs",
    "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs": "WEI_base.cs",
    "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs": "PORT_base.cs",
}
texts = {k: rd(os.path.join(R, v)).replace("\r\n", "\n") for k, v in base.items()}
lines = green.split("\n")
blocks = []
i = 0
while i < len(lines):
    l = lines[i]
    if l.startswith("@@@@ EDIT "):
        path = l[len("@@@@ EDIT "):].strip()
        assert lines[i + 1] == "@@@@ OLD"
        j = i + 2
        old = []
        while lines[j] != "@@@@ NEW":
            old.append(lines[j]); j += 1
        j += 1
        new = []
        while lines[j] != "@@@@ END":
            new.append(lines[j]); j += 1
        blocks.append((path, "\n".join(old) + "\n", ("\n".join(new) + "\n") if new else ""))
        i = j + 1
        continue
    i += 1
print("blocks", len(blocks), "files", len(set(b[0] for b in blocks)))
out = dict(texts)
for path, old, new in blocks:
    c = out[path].count(old)
    print("OLD count", c, path.split("/")[-1], repr(old[:50]))
    out[path] = out[path].replace(old, new, 1)
os.makedirs(os.path.join(R, "green56"), exist_ok=True)
for k in out:
    with open(os.path.join(R, "green56", os.path.basename(k)), "wb") as f:
        f.write(out[k].encode("utf-8"))
    print("green56", hashlib.sha256(out[k].encode("utf-8")).hexdigest(), k)
rl = red.split("\n")
newfiles = {}
i = 0
while i < len(rl):
    if rl[i].startswith("@@@@ NEWFILE "):
        path = rl[i][len("@@@@ NEWFILE "):].strip()
        j = i + 1
        body = []
        while rl[j] != "@@@@ END":
            body.append(rl[j]); j += 1
        newfiles[path] = "\n".join(body) + "\n"
        i = j + 1
        continue
    i += 1
for k, v in newfiles.items():
    h = hashlib.sha256(v.encode("utf-8")).hexdigest()
    ct = os.path.join(P, "v56", "ct", os.path.basename(k))
    hc = hashlib.sha256(rd(ct).encode("utf-8")).hexdigest()
    print("NEWFILE", k.split("/")[-1], h, "ct56", hc, "same" if h == hc else "DIFF")
pkg = rd(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v56.md")).replace("\r\n", "\n")
idx = pkg.index("## Anexo V56-A.")
fences = re.findall(r"```[a-z]*\n(.*?)\n```", pkg[idx:], re.S)
print("annex fenced blocks", len(fences))
for n, f in enumerate(fences):
    if f == green.rstrip("\n"):
        print("fence", n, "== green piece v56")
    for k, v in newfiles.items():
        if f == v.rstrip("\n"):
            print("fence", n, "== newfile", k.split("/")[-1])
# old v55 newfile for core: diff check
v55core = rd(os.path.join(P, "v55", "ct", "PushBackEditAllReloadTests.cs"))
v56core = rd(os.path.join(P, "v56", "ct", "PushBackEditAllReloadTests.cs"))
v55ui = rd(os.path.join(P, "v55", "ct", "PushBackEditAllReloadFieldTests.cs"))
v56ui = rd(os.path.join(P, "v56", "ct", "PushBackEditAllReloadFieldTests.cs"))
print("ui ct identical v55/v56:", v55ui == v56ui)
import difflib
for l in difflib.unified_diff(v55core.split("\n"), v56core.split("\n"), lineterm="", n=0):
    print("CTDIFF", l[:220])
