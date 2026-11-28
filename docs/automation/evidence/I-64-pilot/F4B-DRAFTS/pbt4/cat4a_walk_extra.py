"""CA-T4a: the walk's differential method applied to the test files the walk did NOT select (read-only).

Usage: python -I cat4a_walk_extra.py <repo> <rev> <files.txt> <skeleton.patch> <package.md> <out.txt>

1. Checks that the GREEN texts of green_pbt4a.py are the literal code blocks of the package (D-PBT4-02/03).
2. For each test file of <files.txt> (whole file, a superset of any one test): every string literal, as a regex and as a
   substring, counted in WHC, WEI, NH and RIA at the base and in the GREEN, and in the two new files in the skeleton and in
   the GREEN. A file with no difference is green by construction for any literal-based check.
Only `git show` / `git cat-file` through green_pbt4a.py and here; writes only <out.txt>.
"""
import importlib.util
import re
import subprocess
import sys

repo, rev, lst, patch_p, pkg_p, out = sys.argv[1:7]
here = __file__.rsplit("cat4a_walk_extra.py", 1)[0]
spec = importlib.util.spec_from_file_location("green_pbt4a", here + "green_pbt4a.py")
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green = gp.green(repo, rev)
L = []
w = L.append

# ---------------------------------------------------------------- 1. the GREEN builder against the package blocks
pkg = open(pkg_p, "rb").read().decode("utf-8").replace("\r\n", "\n")
blocks = re.findall(r"```csharp\n(.*?)```", pkg, re.S)
def has_block(text):
    return any(text.strip("\n") == b.strip("\n") for b in blocks)
w("package csharp blocks: %d" % len(blocks))
w("DRAFT == package block: %s" % has_block(gp.DRAFT))
w("IFACE == package block: %s" % has_block(gp.IFACE))
at_origin_pkg = [b for b in blocks if "AtOrigin(RackPilotIntentReadResult read" in b]
w("RIA AtOrigin == package block: %s" % (bool(at_origin_pkg) and gp.RIA_AT_ORIGIN.strip("\n") == at_origin_pkg[0].strip("\n")))
nh_pkg = [b for b in blocks if "public EditIntentReadResult ReadIntentAtOrigin" in b]
w("NH method == package block (indent-insensitive): %s" % (bool(nh_pkg) and [l.strip() for l in gp.NH_NEW_METHOD.strip().split("\n")] == [l.strip() for l in nh_pkg[0].strip().split("\n")]))
wei_pkg = [b for b in blocks if "FromReads(PushBackPilotIntentResult pilotRead" in b and "private EditIntentReadResult(" in b]
if wei_pkg:
    b = wei_pkg[0]
    ok = all(piece.strip() in b for piece in (gp.WEI_CTOR_NEW, gp.WEI_PROP_NEW.split("\n\n", 1)[1]))
    fr = gp.WEI_FROMPILOT_NEW.split("\n\n", 1)[1]
    w("WEI ctor+props in package block: %s; FromReads in package block: %s" % (ok, fr.strip() in b))
ria_ctor_pkg = [b for b in blocks if "internal RackPilotIntentReadResult(PushBackPilotIntentResult pilot, PushBackFullIntentResult full" in b]
w("RIA ctor/props in package block: %s" % (bool(ria_ctor_pkg) and gp.RIA_CTOR.strip() in ria_ctor_pkg[0] and all(p.strip() in ria_ctor_pkg[0] for p in gp.RIA_PROPS.split("\n\n"))))
read_pkg = "var read = _editIntentReader is IWorkspaceEditIntentOriginReader originReader"
w("WHC statement in package: %s; in GREEN x%d" % (read_pkg in pkg, green[gp.WHC_P].count(read_pkg)))

# ---------------------------------------------------------------- skeleton texts of the two new files, from the patch
patch = open(patch_p, "rb").read().decode("utf-8").replace("\r\n", "\n")
skel = {}
for m in re.finditer(r"\+\+\+ b/(\S+)\n@@ -0,0 \+1,\d+ @@\n((?:\+[^\n]*\n)+)", patch):
    skel[m.group(1)] = "".join(l[1:] + "\n" for l in m.group(2).splitlines())
w("skeleton new files: %s" % sorted(skel))
w("IFACE skeleton == GREEN: %s" % (skel.get(gp.IFACE_P) == green[gp.IFACE_P]))

def code_only(t):
    return re.sub(r"//[^\n]*", "", re.sub(r"/\*.*?\*/", "", t, flags=re.S))

pairs = [(p, code_only(base[p]), code_only(green[p])) for p in (gp.WHC_P, gp.WEI_P, gp.NH_P, gp.RIA_P)]
pairs += [(p, code_only(skel.get(p, "")), code_only(green[p])) for p in (gp.IFACE_P, gp.DRAFT_P)]
pairs_raw = [(p, base[p], green[p]) for p in (gp.WHC_P, gp.WEI_P, gp.NH_P, gp.RIA_P)]
pairs_raw += [(p, skel.get(p, ""), green[p]) for p in (gp.IFACE_P, gp.DRAFT_P)]

LIT = re.compile(r'@"(?:[^"]|"")*"|\$"(?:\\.|[^"\\\n])*"|"(?:\\.|[^"\\\n])*"')
ESC = {"\\\\": "\\", '\\"': '"', "\\n": "\n", "\\r": "\r", "\\t": "\t", "\\0": "\0", "\\'": "'"}
def decode(l):
    if l.startswith('@"'):
        return l[2:-1].replace('""', '"')
    inner = l[l.index('"') + 1:-1]
    inner = re.sub(r"\\[\\\"nrt0']", lambda m: ESC[m.group(0)], inner)
    if l.startswith("$"):
        inner = re.sub(r"\{[^{}]*\}", "", inner)
    return inner

def cnt(lit, text):
    r = []
    try:
        r.append(len(re.findall(lit, text)))
    except re.error:
        r.append(None)
    r.append(text.count(lit))
    return r

paths = [l.strip() for l in open(lst, encoding="utf-8") if l.strip().endswith(".cs")]
batch = subprocess.run(["git", "-C", repo, "cat-file", "--batch"], input="".join("%s:%s\n" % (rev, p) for p in paths).encode(), capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b"\n", pos)
    size = int(data[pos:nl].split()[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode("utf-8", "replace")
    pos = nl + 1 + size + 1
clean = 0
for p in paths:
    lits = sorted({decode(l) for l in LIT.findall(texts[p])} - {""})
    diffs = []
    for lit in lits:
        if not re.search(r"[A-Za-z_]{3,}|\\[sbw(]|[(){}.;]", lit):
            continue
        for path, a, b in pairs + pairs_raw:
            ca, cb = cnt(lit, a), cnt(lit, b)
            if ca != cb:
                diffs.append("%r %s %s->%s" % (lit[:60], path.rsplit("/", 1)[1], ca, cb))
                break
    diffs = sorted(set(diffs))
    if diffs:
        w("FLAG %s (%d): %s" % (p, len(diffs), " || ".join(diffs[:8])))
    else:
        clean += 1
w("clean %d of %d" % (clean, len(paths)))
open(out, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("ok", len(L))
