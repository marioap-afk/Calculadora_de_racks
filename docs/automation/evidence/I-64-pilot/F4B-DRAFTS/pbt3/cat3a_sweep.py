import re
import subprocess
import sys
from collections import defaultdict

# Uso: python -I cat3a_sweep.py <checkout> <rev> <paquete.md> [--v44]
# Barrido independiente de colisiones: todo literal de cadena de las pruebas de tests/ que leen src/, probado como expresion
# (o como texto, si no compila) contra el texto que el paquete fija para los archivos nuevos o tocados de PB-T3a, b y c:
# los bloques de codigo y los fragmentos de codigo de D-PBT3-02 a 07 y 09 a 11, el esqueleto de §6.1 y §6.2 y el §5.4.
# Se quitan del corpus las listas de terminos PROHIBIDOS que el paquete nombra (D-PBT3-08, CT-14, CT-PBT3C-01) y las notas de la v4.4.
# Un control con los archivos existentes de Application/Systems/PushBack (Pilot/ incluido), que ya pasan las guardas, separa lo generico.
# --v44 anade la declaracion de la v4.4 (ForWrite) como control positivo del metodo.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO, REV, PKG = sys.argv[1], sys.argv[2], sys.argv[3]
V44 = "--v44" in sys.argv


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


pkg = open(PKG, encoding="utf-8").read().replace("\r\n", "\n").split("\n")


def section(start, stop):
    i = next(k for k, l in enumerate(pkg) if l.startswith(start))
    j = next(k for k in range(i + 1, len(pkg)) if pkg[k].startswith(stop))
    return pkg[i:j]


NOISE = {"Autodesk", "RackCad.Plugin", "RackCad.UI", "System.Windows", "Transaction", "Database", "async", "await", "Task", "File.",
         "System.IO", "RackEmbedComposer", "new RackEmbedDocument", "#nullable", "LockDocument", "StartTransaction", "StartOpenCloseTransaction",
         ".Commit(", "Purge", "Regen", "SyncName", "PushBack", "ForWrite"}


def code_of(lines):
    lines = [l for l in lines if "v4.4" not in l and "(A′)" not in l and "Con (A')" not in l]
    text = "\n".join(lines)
    blocks = re.findall(r"```(?:csharp|cs)?\n(.*?)```", text, re.S)
    spans = [s for s in re.findall(r"`([^`\n]+)`", re.sub(r"```.*?```", "", text, flags=re.S)) if s not in NOISE]
    return "\n".join(blocks) + "\n" + "\n".join(spans)


A = section("### D-PBT3-02", "### D-PBT3-08") + section("- **El esqueleto exacto del RED:**", "- **`RequiredTests`:**")
B = section("### D-PBT3-09.", "### D-PBT3-09b") + section("### D-PBT3-10", "### D-PBT3-11")
C = section("### D-PBT3-11", "**Lo que sigue en verde por su letra**") + section("### 5.4", "### 5.5")
groups = {
    "A Draft/": "namespace RackCad.Application.Systems.PushBack.Draft\n{\n}\nusing System;\nusing System.Collections.Generic;\nusing System.Linq;\n"
                "using System.Text.Json.Nodes;\nusing RackCad.Application.Systems.PushBack.Pilot;\nusing RackCad.Application.Workspace;\n"
                "using RackCad.Application.Persistence;\nusing RackCad.Application.Catalogs;\n" + code_of(A),
    "B PP/UR/IPushBackPilotViewEraser": "namespace RackCad.Application.Systems.PushBack.Pilot\n{\n}\nusing RackCad.Application.Systems.PushBack.Draft;\n" + code_of(B),
    "C WR/RackViewBlockEraser": "namespace RackCad.Plugin.Views\n{\n}\nusing Autodesk.AutoCAD.DatabaseServices;\n"
                                "reference.Erase();\ndefinition.UpgradeOpen();\ndefinition.Erase();\ntransaction.GetObject(id, OpenMode.ForWrite)\n" + code_of(C),
}
if V44:
    groups["A Draft/"] += "\npublic static PushBackFullWriteComposition ForWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog);\n"

files = git("ls-tree", "-r", "--name-only", REV).splitlines()
control_paths = [p for p in files if p.startswith("src/RackCad.Application/Systems/PushBack/") and p.endswith(".cs")]
control = "\n".join(git("show", REV + ":" + p) for p in control_paths)
tests = [p for p in files if p.startswith("tests/") and p.endswith(".cs")]

LIT = re.compile(r'(\$?@\$?"(?:[^"]|"")*"|\$?"(?:[^"\\\n]|\\.)*")')


def unescape(tok):
    verb = "@" in tok[:3]
    body = tok[tok.index('"') + 1:-1]
    if verb:
        return body.replace('""', '"')
    return re.sub(r'\\(.)', lambda m: {"n": "\n", "t": "\t", "r": "\r", "0": "\0"}.get(m.group(1), m.group(1)), body)


def to_py(rx):
    rx = re.sub(r"\(\?<([A-Za-z_]\w*)>", r"(?P<\1>", rx)
    rx = re.sub(r"\\k<([A-Za-z_]\w*)>", r"(?P=\1)", rx)
    return rx


hits = defaultdict(list)
readers = 0
n_lit = 0
for p in tests:
    src = git("show", REV + ":" + p)
    if not re.search(r"EnumerateFiles|GetFiles\(|\bSrc\.|ReadAllText|\"src/|AllProduct|PluginFiles|ApplicationFiles", src):
        continue
    readers += 1
    for ln, line in enumerate(src.split("\n"), 1):
        if line.strip().startswith("//"):
            continue
        for m in LIT.finditer(line):
            lit = unescape(m.group(1))
            if len(lit) < 4:
                continue
            n_lit += 1
            try:
                rx = re.compile(to_py(lit))
            except re.error:
                rx = re.compile(re.escape(lit))
            for g, text in groups.items():
                mm = rx.search(text)
                if not mm:
                    continue
                in_control = bool(rx.search(control))
                hits[(g, in_control)].append((p, ln, lit, text[max(0, mm.start() - 40):mm.end() + 40].replace("\n", " | ")))

print("pruebas que leen src/:", readers, "| literales probados:", n_lit, "| archivos de control:", len(control_paths))
for g in groups:
    print("corpus", g, len(groups[g]), "caracteres")
for (g, ic), lst in sorted(hits.items()):
    print("\n=====", g, "| tambien en el control:" if ic else "| SOLO en el texto nuevo:", len(lst))
    if ic:
        continue
    for p, ln, lit, ctx in lst:
        print("  %s:%d  lit=%r\n      -> %s" % (p, ln, lit[:140], ctx[:220]))
