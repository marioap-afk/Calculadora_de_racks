import re
import subprocess
import sys

# Solo lectura. Uso: python check-invariants.py <BaseSha> <Revision>   (Revision = SHA o HEAD)
# Comprueba sobre git diff BaseSha..Revision: INV-PBT3A-01, 03, 04, 05, 06, 13 (PBT3A-C13), 14 y la regla C-1 (ForFullWrite).
BASE = sys.argv[1]
REV = sys.argv[2]
FOLDER = "src/RackCad.Application/Systems/PushBack/Draft/"
FILES = [
    "PushBackConflictClassifier.cs", "PushBackEditableStateSeat.cs", "PushBackFullComposition.cs", "PushBackFullDraftMaterializer.cs",
    "PushBackFullDraftReader.cs", "PushBackFullDraftState.cs", "PushBackFullDraftStateComparer.cs", "PushBackViewDisposition.cs",
]
ok = True


def git(*args):
    return subprocess.run(["git", *args], capture_output=True, text=True, encoding="utf-8", check=True).stdout


def show(path):
    if REV == "WT":
        return open(path, encoding="utf-8", newline="").read().replace(chr(13) + chr(10), chr(10))
    return git("show", REV + ":" + path)


def report(name, passed, detail=""):
    global ok
    ok = ok and passed
    print(("PASS " if passed else "FAIL ") + name + (" " + detail if detail else ""))


# INV-PBT3A-01
diff_args = ["diff", "--name-status", BASE, "--", "src"] if REV == "WT" else ["diff", "--name-status", BASE + ".." + REV, "--", "src"]
status = [line.split("\t") for line in git(*diff_args).splitlines() if line]
expected = sorted(("A", FOLDER + name) for name in FILES)
report("INV-PBT3A-01 git diff --name-status BaseSha..Rev -- src", sorted((s, p) for s, p in status) == expected,
       "[" + "; ".join(s + " " + p for s, p in status) + "]")

# INV-PBT3A-03 y PBT3A-C13: las declaraciones publicas e internas contra la lista cerrada de D-PBT3-02 a D-PBT3-07.
ALLOWED = {
    "PushBackFullDraftState.cs": {"PushBackFullDraftState", "Editable", "Origin"},
    "PushBackFullDraftStateComparer.cs": {"PushBackFullDraftStateComparer", "Instance", "Equals", "GetHashCode", "Copy"},
    "PushBackEditableStateSeat.cs": {"PushBackEditableStateSeat", "Pass", "LoadExisting"},
    "PushBackFullDraftMaterializer.cs": {"PushBackFullDraftMaterialization", "Kind", "IsAdmitted", "State", "Reason", "Admitted", "OutOfPilot",
                                         "Failed", "PushBackFullDraftMaterializer", "Materialize"},
    "PushBackFullDraftReader.cs": {"PushBackFullIntentResult", "StaleBase", "Read", "OriginKey", "Detail", "ClassicEditorHint",
                                   "PushBackFullDraftReader"},
    "PushBackFullComposition.cs": {"PushBackFullWriteComposition", "IsComposed", "Editor", "Inputs", "Name", "Reason", "Composed", "Rejected",
                                   "PushBackFullComposition", "ForFullWrite", "KeepOn"},
    "PushBackViewDisposition.cs": {"PushBackViewDispositionKind", "Redraw", "ObsoleteLateral", "FrontalBOfOneWay", "NoAddress",
                                   "PushBackViewDispositionPlan", "Members", "ObsoleteLaterals", "AllObsolete", "PushBackViewDispositions", "Of"},
    "PushBackConflictClassifier.cs": {"PushBackChangeGroup", "Name", "Rack", "Annotations", "Safety", "Fronts", "Modules", "SideB", "Composite",
                                      "OutsideEditor", "PushBackChangeClass", "ChangedExternally", "ChangedByUser", "Overlapping",
                                      "PushBackChangeEntry", "Level", "Path", "Group", "Class", "IsStructural", "BaseValue", "ExternalValue",
                                      "DraftValue", "PushBackConflictClassification", "Entries", "Structural", "ViewsToErase", "ViewsRejected",
                                      "AllViewsObsolete", "PushBackConflictClassifier", "Classify"},
}
TYPE = re.compile(r"^\s*(public|internal)\s+(?:static\s+|sealed\s+|abstract\s+)*(class|enum|struct|interface)\s+(\w+)")
MEMBER = re.compile(r"^\s{8}(public|internal)\s+(?:static\s+|readonly\s+|const\s+|override\s+)*[\w<>\[\],.? ]+?\s+(\w+)\s*(?:\(|\{|=>|;|=)")
CTOR = re.compile(r"^\s{8}(public|internal|private)\s+(\w+)\s*\(")
PRIVATE = re.compile(r"^\s{8}private\s+(.*)$")
print("--- declaraciones (archivo | accesibilidad | nombre)")
declared_ok = True
for name in FILES:
    text = show(FOLDER + name)
    names = set()
    for line in text.splitlines():
        m = TYPE.match(line)
        if m:
            print(name + " | " + m.group(1) + " " + m.group(2) + " | " + m.group(3))
            names.add(m.group(3))
            continue
        c = CTOR.match(line)
        if c and c.group(1) in ("public", "internal"):
            print(name + " | " + c.group(1) + " ctor | " + c.group(2))
            names.add(c.group(2))
            continue
        m = MEMBER.match(line)
        if m:
            print(name + " | " + m.group(1) + " | " + m.group(2))
            names.add(m.group(2))
        # miembros de enum, en una sola linea: { A = 0, B = 1 }
        e = re.match(r"^\s*public\s+enum\s+\w+\s*\{(.*)\}", line)
        if e:
            for part in e.group(1).split(","):
                names.add(part.split("=")[0].strip())
    extra = names - ALLOWED[name]
    if extra:
        declared_ok = False
        print("   FUERA DE LA LISTA CERRADA en " + name + ": " + ", ".join(sorted(extra)))
report("INV-PBT3A-03 / PBT3A-C13 publicas e internas dentro de la lista cerrada", declared_ok)

print("--- privadas (INV-PBT3A-13: metodos, tipos anidados sin estado, campos private static readonly de string o Regex)")
private_ok = True
for name in FILES:
    text = show(FOLDER + name)
    for line in text.splitlines():
        p = re.match(r"^\s{8}private\s+(.*)$", line) or re.match(r"^\s{4}private\s+(.*)$", line)
        if not p:
            continue
        rest = p.group(1)
        if re.match(r"^\w+\s*\(", rest):
            kind = "ctor-privado (en la lista cerrada)"
            if name not in ("PushBackFullDraftStateComparer.cs", "PushBackFullDraftMaterializer.cs", "PushBackFullComposition.cs"):
                private_ok = False
                kind = "CTOR-PRIVADO-NO-LISTADO"
        elif re.match(r"^static\s+readonly\s+(string|Regex)\s+\w+\s*=", rest):
            kind = "campo private static readonly inmutable"
        elif "(" in rest.split("=>")[0].split("{")[0]:
            kind = "metodo privado"
        else:
            kind = "OTRO (revisar)"
            private_ok = False
        print(name + " | private | " + kind + " | " + line.strip()[:110])
report("INV-PBT3A-13 privadas admitidas", private_ok)

# INV-PBT3A-04
texts = {name: show(FOLDER + name) for name in FILES}
namespace_ok = all(
    len(re.findall(r"^\s*namespace\s", t, re.M)) == 1
    and len(re.findall(r"^namespace RackCad\.Application\.Systems\.PushBack\.Draft\s*\r?\n\{", t, re.M)) == 1 for t in texts.values())
report("INV-PBT3A-04 un namespace de bloque RackCad.Application.Systems.PushBack.Draft por archivo", namespace_ok)
forbidden = ["#nullable", "Autodesk", "RackCad.Plugin", "RackCad.UI", "System.Windows", "Transaction", "Database", "async", "await", "Task",
             "File.", "System.IO", "RackEmbedComposer", "new RackEmbedDocument"]
hits = [(n, f) for n, t in texts.items() for f in forbidden if f in t]
report("INV-PBT3A-04 / D-PBT3-08 ningun termino prohibido (ni en comentarios)", not hits, str(hits))
compose = [(n, m) for n, t in texts.items() for m in re.findall(r"(?:\.Compose\(|ComposeMemberEnvelope|ComposeForWrite|return new\(|RackEmbedDocument\s+\w+\s*=\s*new)", t)]
report("INV-PBT3A-04 ningun archivo compone ni construye un RackEmbedDocument", not compose, str(compose))

# INV-PBT3A-05
reader = texts["PushBackFullDraftReader.cs"]
materializer = texts["PushBackFullDraftMaterializer.cs"]
reads = len(re.findall(r"PushBackPilotIntentReader\.Read\(", reader))
loads = len(re.findall(r"PushBackEditableStateSeat\.LoadExisting\(", materializer))
report("INV-PBT3A-05 PushBackPilotIntentReader.Read una vez en el lector; LoadExisting una vez en el materializador", reads == 1 and loads == 1,
       "Read=" + str(reads) + " LoadExisting=" + str(loads))

# INV-PBT3A-06
names = ["PushBackFullDraftState", "PushBackFullDraftStateComparer", "PushBackEditableStateSeat", "PushBackFullDraftMaterializer",
         "PushBackFullDraftMaterialization", "PushBackFullDraftReader", "PushBackFullIntentResult", "PushBackFullComposition",
         "PushBackFullWriteComposition", "PushBackViewDisposition", "PushBackViewDispositions", "PushBackViewDispositionPlan",
         "PushBackViewDispositionKind", "PushBackConflictClassifier", "PushBackConflictClassification", "PushBackChangeEntry",
         "PushBackChangeGroup", "PushBackChangeClass"]
pattern = r"\b(?:" + "|".join(names) + r")\b"
outside = []
for path in (git("ls-files", "--", "src") if REV == "WT" else git("ls-tree", "-r", "--name-only", REV, "--", "src")).splitlines():
    if path.startswith(FOLDER):
        continue
    if path in ("src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs",
                "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs"):
        continue
    if not path.endswith(".cs"):
        continue
    if re.search(pattern, show(path)):
        outside.append(path)
report("INV-PBT3A-06 ningun archivo de src/ fuera de Draft/, PP y UR nombra los tipos nuevos", not outside, str(outside))

# C-1
fw = [n for n, t in texts.items() if re.search(r"\bForWrite\b", t)]
sig = "public static PushBackFullWriteComposition ForFullWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog)"
report("C-1 ningun ForWrite en Draft/ y ForFullWrite con la firma de D-PBT3-05", not fw and sig in texts["PushBackFullComposition.cs"], str(fw))

# INV-PBT3A-14 (lectura): ninguna linea anadida muta un PushBackSystem ni un RackModuleReconciliationResult
mut = [(n, t_line.strip()) for n, t in texts.items() for t_line in t.splitlines()
       if re.search(r"(WorkingBaseline|LastModuleReconciliation|computation\.System|lastComputation\.System)\s*(\.\s*\w+)*\s*(=[^=>]|\+=|-=|\.(Add|Remove|Clear|Insert|Sort|Reverse)\()", t_line)]
report("INV-PBT3A-14 ninguna linea anadida muta el rack resuelto ni el informe de modulos", not mut, str(mut))

print("RESULTADO " + ("PASS" if ok else "FAIL"))
sys.exit(0 if ok else 1)
