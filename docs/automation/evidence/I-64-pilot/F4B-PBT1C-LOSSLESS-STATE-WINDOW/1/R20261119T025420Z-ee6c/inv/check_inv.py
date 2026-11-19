#!/usr/bin/env python3
"""Read-only checks of INV-PBT1C-01 and 03 to 06 and 07c over git diff BASE..CURRENT (run from the worktree root).

usage: python3 check_inv.py BASE CURRENT
Never writes anything: it only prints. Exit code 0 when every check passes.
"""
import re
import subprocess
import sys

BASE = sys.argv[1]
CUR = sys.argv[2]
W = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs"
WC = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs"
ALLOWED = {
    W, WC,
    "tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs",
    "tests/RackCad.Tests/PushBackWindowOnStateSourceGuardTests.cs",
    "tests/RackCad.Tests/PushBackWindowSharedReferenceGuardTests.cs",
}
failures = []


def git(*args):
    out = subprocess.run(["git"] + list(args), capture_output=True)
    return out.stdout.decode("utf-8", errors="replace").replace("\r\n", "\n")


def show(rev, path):
    if rev == "WT":  # the working tree, only for the dry run before the commit
        return open(path, "rb").read().decode("utf-8").replace(chr(13) + chr(10), chr(10))
    return git("show", rev + ":" + path)


def rng(base, cur):
    return base if cur == "WT" else base + ".." + cur


def check(name, ok, detail=""):
    print(("PASS " if ok else "FAIL ") + name + (" :: " + detail if detail else ""))
    if not ok:
        failures.append(name)


# ---------------------------------------------------------------------------------------- INV-PBT1C-01
names = [n for n in git("diff", "--name-only", rng(BASE, CUR)).split("\n") if n]
print("rutas cambiadas:")
for n in names:
    print("  " + n)
check("INV-01 las rutas cambiadas estan dentro del AllowedWriteScope", set(names) <= ALLOWED, str(sorted(set(names) - ALLOWED)))
src_names = [n for n in names if n.startswith("src/")]
check("INV-01 en src/ solo W y WC", set(src_names) <= {W, WC}, str(src_names))
empty = git(
    "diff", rng(BASE, CUR), "--",
    "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml", "src/RackCad.Application", "src/RackCad.Domain", "src/RackCad.Plugin",
    "src/RackCad.UI/Editor", "src/RackCad.UI/Controls")
check("INV-01 el diff de XAML, Application, Domain, Plugin, Editor y Controls esta vacio", empty.strip() == "")

# ---------------------------------------------------------------------------------------- INV-PBT1C-03
RANGES = {
    W: [
        ("composite+editable", 49, 57, "any"), ("safetySelections", 84, 84, "any"), ("generalPallet", 94, 101, "any"),
        ("rackTolerance..rackAnnotationScale", 103, 109, "any"), ("advanced", 111, 114, "any"), ("firstLevelDatum", 375, 380, "any"),
        ("loadedDimensionViews", 1244, 1249, "any"), ("loadedDimensionStyle", 1251, 1256, "any"),
        ("dimensionViewsTouched", 1258, 1260, "any"),
        ("LoadNew", 268, 289, "any"), ("LoadDesignForNew", 291, 307, "any"), ("LoadExisting", 309, 324, "any"),
        ("LoadFromModel", 385, 440, "any"), ("ReadInputs", 833, 890, "any"), ("Restore_Click", 3304, 3330, "any"),
        ("Name_Changed", 1234, 1238, "sync:1237"), ("InsertBatch_Click", 3334, 3361, "sync:3352"),
        ("RequestDraw", 3363, 3419, "sync:3405"), ("SaveLibrary_Click", 3481, 3515, "sync:3504"),
        ("SideAOnly", 325, 374, "remove"), ("SelectedDimensionViews", 1288, 1298, "remove"),
    ],
    WC: [
        ("CompositeDiagnostics", 779, 786, "any"), ("CompositeStatusOr", 788, 801, "any"), ("CompositeHasBlocking", 803, 807, "any"),
        ("LoadCompositeFromDesign", 811, 934, "remove"),
    ],
}
DECLS = {
    "private void SyncName() => editable.Name = session.Identity.Name;",
    "internal PushBackEditableState EditableStateForTest => editable;",
}

diff = git("diff", "-U0", rng(BASE, CUR), "--", "src")
hunks = []  # (file, old_start, old_len, new_start, new_len, added_lines, removed_lines)
current_file = None
current = None
for line in diff.split("\n"):
    if line.startswith("+++ b/"):
        current_file = line[6:]
        continue
    m = re.match(r"^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@", line)
    if m:
        current = [current_file, int(m.group(1)), int(m.group(2)) if m.group(2) is not None else 1,
                   int(m.group(3)), int(m.group(4)) if m.group(4) is not None else 1, [], []]
        hunks.append(current)
        continue
    if current is None:
        continue
    if line.startswith("+") and not line.startswith("+++"):
        current[5].append(line[1:])
    elif line.startswith("-") and not line.startswith("---"):
        current[6].append(line[1:])

print("")
print("hunks de git diff -U0 (src), con su miembro y su rango de BaseSha:")
members_touched = {}
bad_hunks = []
for f, a, n, b, c, added, removed in hunks:
    if n > 0:
        lo, hi = a, a + n - 1
        member = next((r for r in RANGES.get(f, []) if r[1] <= lo and hi <= r[2]), None)
    else:
        member = next((r for r in RANGES.get(f, []) if r[1] <= a < r[2]), None)
    label = member[0] if member else "(entre miembros)"
    print("  %s -%d,%d +%d,%d %s  [+%d -%d]" % (f.split("/")[-1], a, n, b, c, label, len(added), len(removed)))
    if member is None:
        decl_ok = n == 0 and all(x.strip() == "" or x.strip() in DECLS for x in added) and any(x.strip() in DECLS for x in added)
        if not decl_ok:
            bad_hunks.append((f, a, n, "fuera de todo rango y no es una de las dos declaraciones"))
        continue
    name, s, e, kind = member
    members_touched.setdefault((f, name), []).append((a, n, added, removed))
    if kind == "remove" and added:
        bad_hunks.append((f, a, n, "un hunk de %s anade lineas" % name))
    if kind.startswith("sync"):
        site = int(kind.split(":")[1])
        if not (n == 0 and a == site and [x.strip() for x in added] == ["SyncName();"]):
            bad_hunks.append((f, a, n, "%s: el unico hunk debe ser la insercion de SyncName(); tras W:%d" % (name, site)))
for name_kind in [("Name_Changed", "sync"), ("InsertBatch_Click", "sync"), ("RequestDraw", "sync"), ("SaveLibrary_Click", "sync")]:
    hs = members_touched.get((W, name_kind[0]), [])
    if len(hs) != 1:
        bad_hunks.append((W, 0, 0, "%s debe tener exactamente un hunk, tiene %d" % (name_kind[0], len(hs))))
check("INV-03 cada hunk cae en su rango y respeta su forma", not bad_hunks, str(bad_hunks))

# the new declarations appear exactly once, between members
added_all = [x.strip() for h in hunks for x in h[5]]
for d in DECLS:
    check("INV-03 la declaracion '%s' se anade una vez" % d.split("=>")[0].strip(), added_all.count(d) == 1)
# comments: no new comment outside the range of composite (W:49-57)
comment_hits = []
for f, a, n, b, c, added, removed in hunks:
    for x in added:
        if x.strip().startswith("//") or x.strip().startswith("///") or x.strip().startswith("/*") or x.strip().startswith("*"):
            in_composite = f == W and 49 <= a <= 57
            if not in_composite:
                comment_hits.append((f.split("/")[-1], a, x.strip()[:60]))
check("INV-03 ningun comentario nuevo fuera del rango de composite (W:49-57)", not comment_hits, str(comment_hits))

# ---------------------------------------------------------------------------------------- INV-PBT1C-04
w_base, w_cur = show(BASE, W), show(CUR, W)
wc_base, wc_cur = show(BASE, WC), show(CUR, WC)


def method_body(text, signature):
    start = text.find(signature)
    if start < 0:
        return ""
    open_ = text.find("{", start)
    depth = 0
    for i in range(open_, len(text)):
        if text[i] == "{":
            depth += 1
        elif text[i] == "}":
            depth -= 1
            if depth == 0:
                return text[start:i + 1]
    return ""


save = method_body(w_cur, "private void SaveLibrary_Click(")
seq22 = [
    "UiSupport.PromptSaveToLibrary(this,", "session.Identity.EnsureId();", "session.Identity.SetName(NameBox.Text?.Trim());",
    "var project = RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);", "new RackProjectStore().Save(project, path);",
]
idx22 = [save.find(s) for s in seq22]
check("INV-04 las cinco sentencias de CT-22 siguen en W y en su orden", all(i >= 0 for i in idx22) and idx22 == sorted(idx22), str(idx22))
check("INV-04 S-9", 'UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")' in w_cur)
check("INV-04 S-7", "new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();" in w_cur)
batch = method_body(w_cur, "private void InsertBatch_Click(")
seq23 = [
    "if (moduleOwner.ModuleSession.HasPendingChanges) moduleOwner.CommitModuleEdits();",
    "Views.RackViewBatchDialogPresenter.TryShow(this, RackSystemKind.PushBack, options, out var views)",
    "session.RequestInsertViews(views",
]
idx23 = [batch.find(s) for s in seq23]
check("INV-04 las sentencias de CT-23 siguen en W y en su orden", all(i >= 0 for i in idx23) and idx23 == sorted(idx23) and batch.rfind("Close();") > idx23[2], str(idx23))
check("INV-04 CT-23 literales", all(s in batch for s in [
    "RackViewAddress.PushBackCut(RackCad.Application.Systems.Shared.RackPushBackEnd.EntradaSalida, RackCad.Application.Systems.Shared.RackPushBackSide.A)",
    "RackViewAddress.PushBackCut(RackCad.Application.Systems.Shared.RackPushBackEnd.Posterior, RackCad.Application.Systems.Shared.RackPushBackSide.B)",
    "RackViewAddress.Whole(DimensionViewKind.Planta)", "RackViewAddress.Post(cut.PostIndex)",
    "RackViewCodec.Encode(RackSystemKind.PushBack, ctx.Views[0])"]) and "canInsertInAutoCad" not in batch)

# SyncName(); right after each session.Identity.SetName( and session.Identity.Adopt( (the seven sites)
lines = w_cur.split("\n")
sites = [i for i, l in enumerate(lines) if "session.Identity.SetName(" in l or "session.Identity.Adopt(" in l]
ok_sites = [i for i in sites if i + 1 < len(lines) and lines[i + 1].strip() == "SyncName();"]
check("INV-04 SyncName(); inmediatamente despues de cada SetName( y Adopt( (siete sitios)", len(sites) == 7 and len(ok_sites) == 7, "sitios=%d con-SyncName=%d" % (len(sites), len(ok_sites)))
base_sites = [i + 1 for i, l in enumerate(w_base.split("\n")) if "session.Identity.SetName(" in l or "session.Identity.Adopt(" in l]
print("  sitios en BaseSha: " + str(base_sites))

# ---------------------------------------------------------------------------------------- INV-PBT1C-05
check("INV-05 W con una sola .RequestUpdate(", w_cur.count(".RequestUpdate(") == 1, str(w_cur.count(".RequestUpdate(")))
tokens = ["forceRebuild", "KeyDown", "PreviewKeyDown", "InputBindings", "KeyBinding", "TabIndex", "IsTabStop", "KeyboardNavigation", ".Focus(", "DialogResult"]
hits = [(t, x.strip()) for h in hunks for x in h[5] for t in tokens if t in x]
check("INV-05 ninguna linea anadida con los tokens prohibidos", not hits, str(hits))
check("INV-05 I-55", "PhysicalPostIndex(LateralSectionBox.SelectedItem)" in w_cur and "corte.PostIndex == section" in w_cur and "Math.Max(0, LateralSectionBox.SelectedIndex)" not in w_cur)
check("INV-05 Fact3 (W sin forceRebuild: ni forceRebuild))", "forceRebuild:" not in w_cur and "forceRebuild)" not in w_cur)
check("INV-05 desviador", "desviadorLevelsPerPost: DesviadorLevelsPerPost()" in w_cur and "showDesviadorSide: false" in w_cur)
setnum = [(h[0].split("/")[-1], x.strip()) for h in hunks for x in h[5] + h[6] if "SetNumber(" in x]
check("INV-05 ninguna linea con SetNumber( anadida ni quitada", not setnum, str(setnum))
lfm = method_body(w_cur, "private void LoadFromModel(")
check("INV-05 editable.AdoptCarriers(inputs, catalog); es la primera sentencia del try de LoadFromModel",
      re.search(r"suppressSync = true;\s*try\s*\{\s*editable\.AdoptCarriers\(inputs, catalog\);", lfm) is not None)
check("INV-05 las dos variables locales y las siembras de LoadFromModel",
      "var pallet = inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, \"kg\");" in lfm and "var options = inputs.Annotations ?? new DynamicAnnotationOptions();" in lfm)
ldf = [i for i, l in enumerate(lines) if "LoadDesignEditor(" in l or "RestoreEditor(" in l]
ok_u = [i for i in ldf if lines[i + 1].strip() == "UpdateCompositeEnabled();"]
check("INV-05 UpdateCompositeEnabled(); en la linea siguiente a LoadDesignEditor y a RestoreEditor", len(ldf) == 3 and len(ok_u) == 3, "sitios=%d ok=%d" % (len(ldf), len(ok_u)))
for w, label in ((w_base, "BaseSha"),):
    pass
witness = [("W", "del lado A SOLO, sin la parte compuesta."), ("W", "que se guarda: la cargada, exacta, si el usuario no"),
           ("WC", "Recupera la intencion compuesta de un diseno"), ("WC", "CARGAR OTRO RACK Y RESTAURAR EL ACTUAL NO SON LO MISMO")]
for which, s in witness:
    base_text = w_base + "\n" + wc_base
    cur_text = w_cur + "\n" + wc_cur
    check("INV-05 linea testigo (%s) unica en BaseSha y ausente en CurrentSha" % which, base_text.count(s) == 1 and cur_text.count(s) == 0, s)


def declarations(text):
    """The member declarations (depth 2) of a class, by name; an overload counts as one more."""
    out = []
    depth = 0
    buf = []
    for raw in text.split("\n"):
        stripped = re.sub(r"//.*$", "", raw)
        before = depth
        depth += stripped.count("{") - stripped.count("}")
        if before >= 2 or depth > 2:
            buf.append(stripped)
        if (before > 2 and depth == 2) or (before == 2 and depth == 2 and (stripped.strip().endswith(";") or stripped.strip().endswith("}"))):
            out.append(" ".join(buf))
            buf = []
    names_ = []
    for member in out:
        member = re.sub(r"\[[^\]]*\]\s*", "", " ".join(member.split()), count=1) if member.strip().startswith("[") else " ".join(member.split())
        header = re.split(r"\(|=>|=|\{|;", member, maxsplit=1)[0].strip()
        ident = re.findall(r"[A-Za-z_]\w*", header)
        if ident:
            names_.append(ident[-1])
    return names_


from collections import Counter
base_decl = Counter(declarations(re.sub(r"/\*.*?\*/", "", w_base, flags=re.S)) + declarations(re.sub(r"/\*.*?\*/", "", wc_base, flags=re.S)))
cur_decl = Counter(declarations(re.sub(r"/\*.*?\*/", "", w_cur, flags=re.S)) + declarations(re.sub(r"/\*.*?\*/", "", wc_cur, flags=re.S)))
expected = base_decl.copy()
for gone in ("SideAOnly", "SelectedDimensionViews", "LoadCompositeFromDesign", "LoadCompositeFromDesign"):
    expected[gone] -= 1
for new in ("editable", "SyncName", "EditableStateForTest"):
    expected[new] += 1
expected = Counter({k: v for k, v in expected.items() if v > 0})
print("  declaraciones de miembro en BaseSha: %d; en CurrentSha: %d" % (sum(base_decl.values()), sum(cur_decl.values())))
check("INV-05 el censo de declaraciones es el de BaseSha sin las cuatro retiradas y con las tres prescritas", expected == cur_decl,
      str({k: (expected.get(k, 0), cur_decl.get(k, 0)) for k in set(expected) | set(cur_decl) if expected.get(k, 0) != cur_decl.get(k, 0)}))
forwarders = ["composite", "safetySelections", "generalPallet", "rackTolerance", "rackPostPeralte", "rackBeamDepth", "rackAnnotationScale",
              "advanced", "firstLevelDatum", "loadedDimensionViews", "loadedDimensionStyle", "dimensionViewsTouched"]
check("INV-05 los doce reenvios conservan su nombre (una declaracion cada uno)", all(cur_decl[n] == 1 for n in forwarders), str({n: cur_decl[n] for n in forwarders}))

# ---------------------------------------------------------------------------------------- INV-PBT1C-06
internal_base = [l.strip() for l in (w_base + "\n" + wc_base).split("\n") if l.strip().startswith("internal ")]
internal_cur = [l.strip() for l in (w_cur + "\n" + wc_cur).split("\n") if l.strip().startswith("internal ")]
print("  costuras internal en BaseSha: %d" % len(internal_base))
check("INV-06 las costuras internal de BaseSha siguen con su nombre y tipo, mas EditableStateForTest",
      Counter(internal_cur) == Counter(internal_base + ["internal PushBackEditableState EditableStateForTest => editable;"]) or
      Counter(internal_cur) == Counter(internal_base + ["internal PushBackEditableState EditableStateForTest => new PushBackEditableState();"]))

# ---------------------------------------------------------------------------------------- INV-PBT1C-07c
added_w = [x.strip() for h in hunks for x in h[5]]
mut = [x for x in added_w if re.search(r"(lastComputation\s*\.\s*System|session\s*\.\s*System|lastCompositeSystem|[Ww]orkingBaseline|LastModuleReconciliation)[^;]*(\.\s*Name\s*=(?!=)|\.Add\(|\.Clear\(|\.Remove\()", x)]
check("INV-07c ninguna linea anadida asigna el Name ni muta el rack resuelto o el informe", not mut, str(mut))
names_assigned = [x for x in added_w if re.search(r"\.Name\s*=(?!=)", x)]
check("INV-07c la unica asignacion de .Name anadida es la de SyncName", names_assigned == ["private void SyncName() => editable.Name = session.Identity.Name;"], str(names_assigned))

print("")
print("RESULTADO: " + ("TODO PASA" if not failures else "FALLAN: " + ", ".join(failures)))
sys.exit(0 if not failures else 1)
