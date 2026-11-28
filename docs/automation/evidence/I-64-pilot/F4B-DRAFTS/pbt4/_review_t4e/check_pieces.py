import sys, os, hashlib, re
sys.stdout.reconfigure(encoding="utf-8")
P = sys.argv[1]   # pbt4 folder
R = sys.argv[2]   # review folder with *_base.cs

def rd(p):
    with open(p, "rb") as f:
        return f.read().decode("utf-8")

green = rd(os.path.join(P, "pbt4e-green-v55.txt")).replace("\r\n", "\n")
red = rd(os.path.join(P, "pbt4e-red-v55.txt")).replace("\r\n", "\n")
print("green sha", hashlib.sha256(rd(os.path.join(P, "pbt4e-green-v55.txt")).encode("utf-8")).hexdigest())

base = {
    "src/RackCad.Application/Workspace/WorkspaceHostCore.cs": "WHC_base.cs",
    "src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs": "FVM_base.cs",
    "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs": "WEI_base.cs",
    "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs": "PORT_base.cs",
}
texts = {}
crlf = {}
for k, v in base.items():
    raw = rd(os.path.join(R, v))
    crlf[k] = "\r\n" in raw
    texts[k] = raw.replace("\r\n", "\n")
print("crlf in base:", crlf)

# parse green blocks
lines = green.split("\n")
blocks = []
i = 0
while i < len(lines):
    l = lines[i]
    if l.startswith("@@@@ EDIT "):
        path = l[len("@@@@ EDIT "):].strip()
        assert lines[i + 1] == "@@@@ OLD", lines[i + 1]
        j = i + 2
        old = []
        while lines[j] != "@@@@ NEW":
            old.append(lines[j]); j += 1
        j += 1
        new = []
        while lines[j] != "@@@@ END":
            new.append(lines[j]); j += 1
        blocks.append((path, "\n".join(old), "\n".join(new)))
        i = j + 1
        continue
    i += 1
print("blocks", len(blocks), "files", sorted(set(b[0] for b in blocks)))
out = dict(texts)
for path, old, new in blocks:
    c = out[path].count(old)
    print("OLD count", c, path, repr(old[:60]))
    out[path] = out[path].replace(old, new, 1)
for k in out:
    os.makedirs(os.path.join(R, "green"), exist_ok=True)
    with open(os.path.join(R, "green", os.path.basename(k)), "wb") as f:
        f.write(out[k].encode("utf-8"))

# red
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
    ct = os.path.join(P, "v55", "ct", os.path.basename(k))
    hc = hashlib.sha256(rd(ct).encode("utf-8")).hexdigest()
    print("NEWFILE", k, h, "ct", hc, "same" if h == hc else "DIFF")

# annex V55-A vs pieces
pkg = rd(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v55.md")).replace("\r\n", "\n")
idx = pkg.index("## Anexo V55-A.")
annex = pkg[idx:]
fences = re.findall(r"```[a-z]*\n(.*?)\n```", annex, re.S)
print("annex fenced blocks", len(fences), [len(f) for f in fences])
gtxt = green.rstrip("\n")
for n, f in enumerate(fences):
    if f == gtxt:
        print("fence", n, "== green piece")
    for k, v in newfiles.items():
        if f == v.rstrip("\n"):
            print("fence", n, "== newfile", k)
