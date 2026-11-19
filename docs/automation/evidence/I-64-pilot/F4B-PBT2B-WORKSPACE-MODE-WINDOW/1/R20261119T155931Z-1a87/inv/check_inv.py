#!/usr/bin/env python3
"""Comprobaciones de solo lectura de INV-PBT2B-01, 03, 04, 06, 07c y 14 y de PBT2B-C13 sobre git diff BaseSha..<destino>.

Uso (desde la raiz del worktree): python3 check_inv.py <BaseSha> [<destino>]
Sin destino compara contra el arbol de trabajo. Las rutas son relativas al worktree. Solo lee: no escribe nada.
"""
import re
import subprocess
import sys

BASE = sys.argv[1]
TARGET = sys.argv[2] if len(sys.argv) > 2 else None

W = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs"
MODE = "src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs"
ALLOWED = {
    W,
    MODE,
    "tests/RackCad.UI.Tests/PushBackWorkspaceSupport.cs",
    "tests/RackCad.UI.Tests/PushBackWorkspaceModeTests.cs",
    "tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs",
    "tests/RackCad.UI.Tests/PushBackFieldValidationConcordanceTests.cs",
    "tests/RackCad.Tests/PushBackWorkspaceModeSourceGuardTests.cs",
    "tests/RackCad.Tests/PushBackWorkspaceSharedReferenceGuardTests.cs",
}
EMPTY_PATHS = [
    "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml",
    "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs",
    "src/RackCad.Application", "src/RackCad.Domain", "src/RackCad.Plugin", "src/RackCad.UI/Editor", "src/RackCad.UI/Controls",
    "src/RackCad.UI/Workspace", "src/RackCad.UI/Shell",
]
# Los miembros que cambian, con sus rangos en BaseSha (INV-PBT2B-03).
MEMBERS = [
    ("LoadNew", 269, 287), ("LoadDesignForNew", 289, 304), ("LoadExisting", 306, 320), ("Update_Click", 3215, 3215),
    ("InsertBatch_Click", 3217, 3245), ("RequestDraw", 3247, 3304), ("PendingWork", 3331, 3344), ("Bom_Click", 3348, 3364),
    ("SaveLibrary_Click", 3366, 3401), ("UpdateButtons", 3449, 3496),
]
NEW_NAMES = {
    "mode", "draftPort", "appliedState", "BomDialog", "LibraryPathDialog", "ModeForTest", "ForDraft", "ApplyModeChrome",
    "RefuseInWorkspace", "ApplyEditsToEditingState", "ApplyToDraft", "HasUnappliedDraftChanges", "WorkspaceApplyLabel",
    "WorkspaceApplyToolTip", "WorkspaceApplyBlockedToolTip", "WorkspaceRefusalMessage", "WorkspaceLoadRefusalMessage",
    "WorkspaceInvalidMessage", "WorkspaceBlockedMessage", "WorkspaceNothingToApplyMessage", "WorkspaceAppliedMessage",
    "WorkspaceNotEditableMessage", "WorkspaceUnappliedQuestion", "PushBackEditorMode", "Classic", "Workspace",
}
FORBIDDEN_ADDED = ["forceRebuild", "KeyDown", "PreviewKeyDown", "InputBindings", "KeyBinding", "TabIndex", "IsTabStop", "KeyboardNavigation", ".Focus(", "DialogResult"]
GL1_TOKENS = ["WorkingBaseline", "workingBaseline", "LastModuleReconciliation", "computation.System", "lastComputation.System", "session.System", "lastCompositeSystem", "SystemToInsert"]


def git(*args):
    out = subprocess.run(["git"] + list(args), capture_output=True, check=True)
    return out.stdout.decode("utf-8")


def show(rev, path):
    if rev is None:
        with open(path, encoding="utf-8", newline="") as handle:
            return handle.read().replace("\r\n", "\n")
    try:
        return git("show", "%s:%s" % (rev, path)).replace("\r\n", "\n")
    except subprocess.CalledProcessError:
        return None


def diff_args(*extra, paths=()):
    return ["diff"] + list(extra) + [BASE] + ([TARGET] if TARGET else []) + (["--"] + list(paths) if paths else [])


ok_all = True


def report(name, ok, detail=""):
    global ok_all
    ok_all = ok_all and ok
    print("%s: %s%s" % (name, "OK" if ok else "FALLA", (" " + detail) if detail else ""))


# ---- INV-PBT2B-01 ------------------------------------------------------------------------------------------------
names = [line for line in git(*diff_args("--name-only")).splitlines() if line]
if not TARGET:
    names += [line[3:] for line in git("status", "--short", "--untracked-files=all").splitlines() if line.startswith("??")]
names = sorted(set(names))
outside = [n for n in names if n not in ALLOWED]
report("INV-PBT2B-01 rutas dentro del alcance", not outside, "cambiadas=%d fuera=%s" % (len(names), outside))
src = [n for n in names if n.startswith("src/")]
report("INV-PBT2B-01 en src/ solo W y el archivo del modo", set(src) <= {W, MODE}, str(src))
empty = git(*diff_args("--stat", paths=EMPTY_PATHS)).strip()
report("INV-PBT2B-01 vacio git diff de WX, WC, Application, Domain, Plugin, Editor, Controls, Workspace y Shell", empty == "", empty)

# ---- INV-PBT2B-03 ------------------------------------------------------------------------------------------------
patch = git(*diff_args("-U0", paths=[W]))
hunks = re.findall(r"^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@", patch, flags=re.M)
blocks = re.split(r"^@@ .*$", patch, flags=re.M)[1:]
bad = 0
print("INV-PBT2B-03 hunks de W (-U0):")
for (old_start, old_count, new_start, new_count), block in zip(hunks, blocks):
    old_start = int(old_start)
    old_count = 1 if old_count == "" else int(old_count)
    added = [l[1:] for l in block.splitlines() if l.startswith("+")]
    declared = []
    for line in added:
        match = re.match(r"^ {8}(?:(?:public|private|internal|protected|static|readonly|const|override)\s+)+[\w<>\[\],.?() ]*?\b(\w+)\s*(?:\(|=>|=|;|\{)", line)
        if match:
            declared.append(match.group(1))
    removed_lines = [l[1:] for l in block.splitlines() if l.startswith("-")]
    only_blank_removed = old_count > 0 and all(r.strip() == "" for r in removed_lines)
    if old_count == 0 or only_blank_removed:
        first, last = old_start, old_start       # insercion tras la linea old_start (o sustituyendo solo lineas en blanco)
    else:
        first, last = old_start, old_start + old_count - 1
    member = None
    for name, start, end in MEMBERS:
        if first >= start and last <= end:
            member = name
    if member:
        verdict = "en %s" % member
    elif (old_count == 0 or only_blank_removed) and len(declared) <= 1:
        verdict = "insercion entre miembros que declara %s" % (declared or "nada")
    else:
        verdict = "FUERA DE LA LISTA (declara %s)" % declared
        bad += 1
    print("  @@ -%d,%d +%s,%s  %s  declara=%s" % (old_start, old_count, new_start, new_count or "1", verdict, declared))
report("INV-PBT2B-03 cada hunk cae en un miembro de la lista o es una insercion entre miembros de una sola declaracion", bad == 0, "hunks=%d" % len(hunks))

# ---- INV-PBT2B-04 ------------------------------------------------------------------------------------------------
new_w = show(TARGET, W)
base_w = show(BASE, W)
new_mode = show(TARGET, MODE)


def method(text, signature):
    start = text.find(signature)
    if start < 0:
        return None
    brace = text.find("{", start)
    depth = 0
    for index in range(brace, len(text)):
        if text[index] == "{":
            depth += 1
        elif text[index] == "}":
            depth -= 1
            if depth == 0:
                return text[start:index + 1]
    return None


witnesses = {
    "S-7": "new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();",
    "S-9": 'UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")',
    "pregunta del modulo": "sin confirmar que se perder",
    "rechazo invalido": "Corrige los datos: no se puede insertar un modelo inv",
    "rechazo bloqueado": "Corrige los datos: no se puede dibujar un modelo bloqueado.",
}
for name, text in witnesses.items():
    report("INV-PBT2B-04 testigo " + name, (new_w or "").count(text) >= 1)
save = method(new_w or "", "private void SaveLibrary_Click(") or ""
order = [save.find(s) for s in ["UiSupport.PromptSaveToLibrary(this,", "session.Identity.EnsureId();", "session.Identity.SetName(NameBox.Text?.Trim());",
                                "var project = RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);",
                                "new RackProjectStore().Save(project, path);"]]
report("INV-PBT2B-04 las cinco sentencias de CT-22 en su orden", all(v >= 0 for v in order) and order == sorted(order), str(order))
batch = method(new_w or "", "private void InsertBatch_Click(") or ""
confirm = batch.find("if (moduleOwner.ModuleSession.HasPendingChanges) moduleOwner.CommitModuleEdits();")
dialog = batch.find("Views.RackViewBatchDialogPresenter.TryShow(this, RackSystemKind.PushBack, options, out var views)")
report("INV-PBT2B-04 literales de CT-23 (confirmacion antes del dialogo y sin canInsertInAutoCad ni Close() antes de la guarda)",
       0 <= confirm < dialog and "canInsertInAutoCad" not in batch and "RefuseInWorkspace" in batch[:batch.find("if (moduleOwner")])
for name, text in (("firma publica 1", "public RackPushBackSystemWindow()"),
                   ("firma publica 2", "public RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)")):
    report("INV-PBT2B-04 " + name + " sin cambio", (base_w or "").count(text) == 1 and (new_w or "").count(text) == 1)
report("INV-PBT2B-04 una sola .RequestUpdate( en W", (new_w or "").count(".RequestUpdate(") == 1)
consts = {
    "WorkspaceApplyLabel": "Aplicar al borrador",
    "WorkspaceApplyToolTip": "Guarda los cambios en el borrador. El dibujo cambia con ",
    "WorkspaceApplyBlockedToolTip": " bloqueante: el rack no puede pasar al borrador as",
    "WorkspaceRefusalMessage": "En el editor del borrador no se dibuja: aplica al borrador y actualiza desde el panel.",
    "WorkspaceLoadRefusalMessage": "La ventana del borrador solo carga el estado del borrador.",
    "WorkspaceInvalidMessage": "Corrige los datos: no se puede aplicar al borrador un modelo inv",
    "WorkspaceBlockedMessage": "Corrige los datos: no se puede aplicar al borrador un modelo bloqueado.",
    "WorkspaceNothingToApplyMessage": "No hay cambios que aplicar al borrador.",
    "WorkspaceAppliedMessage": "Cambios aplicados al borrador. El dibujo cambia con ",
    "WorkspaceNotEditableMessage": "El borrador no admite cambios ahora: cierra el editor y revisa el panel.",
    "WorkspaceUnappliedQuestion": "Hay cambios sin aplicar al borrador que se perder",
}
for name, witness in consts.items():
    declaration = re.search(r"private const string %s = \"([^\"]*)\";" % name, new_w or "")
    report("INV-PBT2B-04 constante %s una vez y con su testigo" % name,
           bool(declaration) and (new_w or "").count("string %s " % name) == 1 and witness in declaration.group(1))
added_w = [l[1:] for l in patch.splitlines() if l.startswith("+") and not l.startswith("+++")]
removed_w = [l[1:] for l in patch.splitlines() if l.startswith("-") and not l.startswith("---")]
mode_added = (new_mode or "").splitlines()
for token in FORBIDDEN_ADDED:
    hits = [l for l in added_w + mode_added if token in l]
    report("INV-PBT2B-04 ninguna linea anadida con %s" % token, not hits, str(hits[:2]))
report("INV-PBT2B-04 ninguna linea con SetNumber( anadida ni quitada",
       not [l for l in added_w + removed_w if "SetNumber(" in l], str([l for l in added_w + removed_w if "SetNumber(" in l][:2]))
for signature in ("private void LoadFromModel(", "private PushBackEditorInputs ReadInputs(", "protected override void OnClosing(", "private void Recompute("):
    before = method(base_w or "", signature)
    after = method(new_w or "", signature)
    if before is None:
        # ReadInputs / Recompute pueden tener otro tipo de retorno: se busca por el nombre.
        name = re.search(r"(\w+)\($", signature).group(1)
        found_b = re.search(r"\n( {8}[^\n]*\b%s\([^\n]*)\n" % name, base_w or "")
        before = method(base_w or "", found_b.group(1)) if found_b else None
        found_a = re.search(r"\n( {8}[^\n]*\b%s\([^\n]*)\n" % name, new_w or "")
        after = method(new_w or "", found_a.group(1)) if found_a else None
    report("INV-PBT2B-04 %s sin cambio" % signature.split("(")[0].split()[-1], before is not None and before == after)
report("INV-PBT2B-04 el archivo del modo solo declara el enum",
       re.sub(r"\s+", " ", re.sub(r"//[^\n]*", "", new_mode or "")).strip()
       == "namespace RackCad.UI.Systems.PushBack { public enum PushBackEditorMode { Classic = 0, Workspace = 1 } }")

# ---- INV-PBT2B-06 y PBT2B-C13 ------------------------------------------------------------------------------------------
DECL = re.compile(r"^ {8}(?:(?:public|private|internal|protected|static|readonly|const|override|sealed|partial|abstract|virtual|async)\s+)+[\w<>\[\],.?() ]*?\b(\w+)\s*(?:\(|=>|=|;|\{|:)", re.M)


def declared(text):
    return set(m.group(1) for m in DECL.finditer(text or ""))


new_names = declared(new_w) - declared(base_w)
unknown = sorted(n for n in new_names if n not in NEW_NAMES)
report("PBT2B-C13 toda declaracion nueva de W esta en la lista cerrada", not unknown, "nuevas=%s sin lista=%s" % (sorted(new_names), unknown))
ctors = re.findall(r"^ {8}(\w+\s+)?RackPushBackSystemWindow\(([^)]*)\)", new_w or "", flags=re.M)
report("PBT2B-C13 el unico constructor nuevo es el privado de PushBackDraftEditorLaunch",
       sorted(re.findall(r"^ {8}(?:public|private|internal)\s+RackPushBackSystemWindow\(([^)]*)\)", new_w or "", flags=re.M))
       == sorted(["", "bool canInsertInAutoCad, Func<string> newIdFactory = null", "PushBackDraftEditorLaunch launch"]), str(ctors))
seams_base = set(re.findall(r"^ {8}internal [^\n(]*?\b(\w+)\b(?: \{ get; (?:private )?set; \}| =>[^\n]*;|;)$", base_w or "", flags=re.M))
seams_new = set(re.findall(r"^ {8}internal [^\n(]*?\b(\w+)\b(?: \{ get; (?:private )?set; \}| =>[^\n]*;|;)$", new_w or "", flags=re.M))
report("INV-PBT2B-06 las costuras de hoy siguen y solo se anaden ModeForTest, BomDialog y LibraryPathDialog",
       seams_base <= seams_new and (seams_new - seams_base) == {"ModeForTest", "BomDialog", "LibraryPathDialog"},
       "base=%d nuevas=%s" % (len(seams_base), sorted(seams_new - seams_base)))

# ---- INV-PBT2B-07c y 14 -----------------------------------------------------------------------------------------
MUT = re.compile(r"(?:%s)(?:\s*\??\.\s*\w+(?:\s*\([^()]*\))?|\s*\[[^\]]*\])*\s*(?:\??\.\s*\w+|\[[^\]]*\])\s*(?:=(?![=>])|\+=|-=|\+\+|--)|(?:%s)[\w.\s?\[\]()]*\.\s*(?:Add|AddRange|Clear|Remove|RemoveAt|RemoveAll|Insert|Sort|Reverse|RecalculatePositions)\s*\(" % ("|".join(re.escape(t) for t in GL1_TOKENS), "|".join(re.escape(t) for t in GL1_TOKENS)))
hits = [l for l in added_w if MUT.search(l) or re.search(r"\.Name\s*=(?!=)", l)]
report("INV-PBT2B-07c/14 ninguna linea anadida toca un token de G-L1 ni asigna un Name", not hits, str(hits[:3]))

print("RESULTADO GLOBAL:", "OK" if ok_all else "FALLA")
