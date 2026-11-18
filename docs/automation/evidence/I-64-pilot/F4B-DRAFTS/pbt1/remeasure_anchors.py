# Solo lectura: remedicion (v2.3) de los textos de los mutantes de PB-T1a y de otras anclas textuales de W y WC.
# Para cada ancla: la linea esperada (la de la v2.2), las lineas donde aparece hoy el texto (exacto, tras quitar la
# sangria) y si es unico. No escribe nada.
# Uso: python -I remeasure_anchors.py <raiz-del-worktree>
import os
import sys

root = sys.argv[1]
W = os.path.join(root, "src", "RackCad.UI", "Systems", "PushBack", "RackPushBackSystemWindow.xaml.cs")
WC = os.path.join(root, "src", "RackCad.UI", "Systems", "PushBack", "RackPushBackSystemWindow.Composite.cs")


def load(path):
    with open(path, encoding="utf-8-sig") as f:
        return f.read().split("\n")


files = {"W": load(W), "WC": load(WC)}

ANCHORS = [
    ("MU-T1A-01", "W", 874, "NumberFronts = NumberFrontsCheck.IsChecked == true,"),
    ("MU-T1A-02", "W", 852, 'generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),'),
    ("MU-T1A-03", "W", 817, "DepthStartPosition = IntVal(DepthStartBox, front.DepthStartPosition),"),
    ("MU-T1A-04", "W", 827, "IntermediateBeamDepth = SelectedPeralte(CellIntermediatePeralteBox, cell.IntermediateBeamDepth)"),
    ("MU-T1A-05", "WC", 209, "composite.SetCentralSeparator(CentralSeparatorCheck.IsChecked == true);"),
    ("MU-T1A-06", "W", 2133, "var height = DerivedPostLineHeightBox.Value;"),
    ("MU-T1A-07a", "W", 3686, "=> !field.Value.HasValue || SameShown(field.Value.Value, current) ? current : field.Value.Value;"),
    ("MU-T1A-07b", "W", 696, "var standardRestoreLanded = moduleOwner.ModuleCommit?.StandardRestoreRequested == true; // I-64 F4B PB-TC (D-PB-11)"),
    ("MU-T1A-08", "W", 1510, "ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues(side).Dynamic, frontTargets));"),
    ("MU-T1A-09", "W", 1171, "=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 1));"),
    ("MU-T1A-10", "W", 3668, ": (IsMixed(field) ? sideValue : null);"),
    ("MU-T1A-11", "W", 2682, "ApplyBootSections(bootSectionA, bootSectionB);"),
    ("MU-T1A-12", "W", 3568, "BomButton.IsEnabled = canOutput;"),
    ("MU-T1A-13", "W", 313, "if (design == null) return;"),
    ("MU-T1A-14", "W", 419, "rackAnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;"),
    # Zonas [TC2] (seccion 2.8 de la v2.2): el texto de hoy, para registrar como cambio.
    ("TC2-Gap", "WC", 198, "composite.SetGap(Keep(GapBox, composite.Gap));"),
    ("TC2-ModuleLength", "W", 1905, "var length = Keep(ModuleLengthBox, module.Length);"),
    ("TC2-Header", "W", 1639, "advanced.ManualHeaderHeightOverride = KeepOptional(RackHeaderHeightBox, advanced.ManualHeaderHeightOverride);"),
    ("TC2-Reinf", "W", 1641, "advanced.DerivedPostReinforcementHeight = KeepOptional(DerivedPostReinforcementHeightBox, advanced.DerivedPostReinforcementHeight);"),
    ("TC2-PostH", "W", 1642, "advanced.DerivedPostHeight = KeepOptional(DerivedPostHeightBox, advanced.DerivedPostHeight);"),
    ("TC2-SepSp", "W", 1646, "advanced.SeparatorSpacingOverride = KeepOptional(RackSeparatorSpacingBox, advanced.SeparatorSpacingOverride);"),
    ("TC2-Cell-818", "W", 818, "FirstLevelHeight = KeepCell(FirstLevelHeightBox, front.FirstLevelHeight),"),
    ("TC2-Cell-822", "W", 822, "ClearHeight = KeepCell(CellClearBox, cell.ClearHeight),"),
    ("TC2-OptionalVal-mid", "W", 3665, "? (MixedBoth(field) ? field.Value : KeepOptional(field, sideValue))"),
    # Anclas de D-PBT1-03, D-PBT1-13, HT1-09, HT1-10b y c-02 que nombra la v2.2 (texto de referencia).
    ("Pasada-659", "W", 659, None),
    ("Pasada-675", "W", 675, None),
    ("Pasada-679", "W", 679, None),
    ("Pasada-697", "W", 697, None),
    ("Pasada-704", "W", 704, None),
    ("ReadInputs-868", "W", 868, None),
    ("ReadInputs-823", "W", 823, None),
    ("ReadInputs-826", "W", 826, None),
    ("LoadFromModel-391", "W", 391, None),
    ("LoadFromModel-427", "W", 427, None),
    ("Post-399", "W", 399, None),
    ("Keep-838", "W", 838, None),
    ("Keep-844", "W", 844, None),
    ("New-280", "W", 280, None),
    ("New-288", "W", 288, None),
    ("Design-439", "W", 439, None),
    ("Update-3391", "W", 3391, None),
    ("Reentrant-3277", "W", 3277, None),
    ("Reentrant-3289", "W", 3289, None),
    ("Module-1688", "W", 1688, None),
    ("WC-117", "WC", 117, None),
    ("WC-121", "WC", 121, None),
    ("WC-933", "WC", 933, None),
    ("WC-783", "WC", 783, None),
    ("WC-789", "WC", 789, None),
    ("WC-803", "WC", 803, None),
    ("WC-807", "WC", 807, None),
]

for ident, fname, expected, text in ANCHORS:
    lines = files[fname]
    if text is None:
        got = lines[expected - 1].strip() if expected <= len(lines) else "<fuera>"
        print(f"{ident}\t{fname}:{expected}\t{got}")
        continue
    hits = [i + 1 for i, l in enumerate(lines) if l.strip() == text]
    status = "IGUAL" if hits == [expected] else ("MOVIDA" if len(hits) == 1 else ("AUSENTE" if not hits else "REPETIDA"))
    print(f"{ident}\t{fname}:{expected}\t{status}\t{hits}")

print(f"W lineas={len(files['W'])} WC lineas={len(files['WC'])}")
