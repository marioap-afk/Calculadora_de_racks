import re
import subprocess
import sys

# Solo lectura. Uso: python rr3-literals.py <Revision> [<Revision2>]
# Cuenta, por clase de prueba nueva, los literales de la tabla de RR-3 (lineas/fragmentos: subcadena exacta; valores: token).
# Con dos revisiones, compara los conteos (la ayuda mecanica de RR-3; una diferencia es parada C07).
NESTED = [
    "PushBack.Structure.Fronts.0", "PushBack.Structure.Fronts.0.Levels.0", "PushBack.Structure.Modules.0",
    "PushBack.Structure.Modules.0.Header", "PushBack.Structure.Modules.0.Header.LeftPost",
    "PushBack.Structure.Modules.0.Header.RightBasePlate", "PushBack.Structure.Modules.0.Header.Horizontals.0",
    "PushBack.Structure.Modules.0.Header.Panels.0", "PushBack.Structure.SafetySelections.0",
    "PushBack.Structure.SafetySelections.0.PostSides.0", "PushBack.Structure.SafetySelections.0.DesviadorOffCells.0",
    "PushBack.Structure.SafetySelections.0.DefensaPosts.0", "PushBack.Structure.SafetySelections.0.GuiaEntradaOffCells.0",
    "PushBack.Structure.SafetySelections.0.ParrillaOffCells.0", "PushBack.Structure.SafetySelections.0.BotaPosts.0",
    "PushBack.Structure.SafetySelections.0.TopeOffCells.0", "PushBack.Structure.SafetySelections.0.BotaBPosts.0",
    "PushBack.Structure.HeaderLineOverrides.0", "PushBack.Structure.HeaderLineOverrides.0.Header",
    "PushBack.Structure.DerivedPostLineOverrides.0",
    "PushBack.Structure", "PushBack.Fronts.0", "PushBack.SideB", "PushBack.SideB.Fronts.0", "PushBack.SideB.FrontConfigs.0",
    "PushBack.Composite", "PushBack.Composite.Topologies.0", "PushBack.RearTopeOffCells.0",
]
OWN27 = [
    "Envelope $.CustomProperties fila=", "Envelope $.Design fila=", "Envelope $.Id fila=", "Envelope $.Kind fila=", "Envelope $.Name fila=",
    "Envelope $.SchemaVersion fila=", "Envelope $.Section fila=", "Envelope $.View fila=", "Project $.Cantilever fila=",
    "Project $.DynamicSystem fila=", "Project $.FlowBed fila=", "Project $.Header fila=", "Project $.Kind fila=", "Project $.Larguero fila=",
    "Project $.PushBack fila=", "Project $.SchemaVersion fila=", "Project $.SelectiveRack fila=", "Payload $.Composite fila=",
    "Payload $.DefensePieceId fila=", "Payload $.Fronts fila=", "Payload $.LegacyHighEndBeamPeralte fila=", "Payload $.RearTopeOffCells fila=",
    "Payload $.RearTopePieceId fila=", "Payload $.RearTopeSaque fila=", "Payload $.SchemaVersion fila=", "Payload $.SideB fila=",
    "Payload $.Structure fila=",
]
CLASSES = {
    "tests/RackCad.UI.Tests/PushBackFullDraftWindowParityTests.cs": (
        [" estado-igual=True diseno-igual=True", "pb-21-cola OutOfPilot motivo=", "DormantTailOutOfPilotReason"], []),
    "tests/RackCad.Tests/PushBackFullDraftLoadTests.cs": (
        [" completo=", " piloto=", " motivo=", "UnreadableOrFutureDiagnostic", "KindDivergentDiagnostic", "NoDesignDiagnostic",
         "CatalogRequiredDiagnostic", "CompositeOutOfPilotReason", "DormantTailOutOfPilotReason", "MaterializeFailurePrefix",
         "pb-20-ab piloto=NotAdmitted completo=Admitted base-igual=True", " iguales="] + NESTED +
        [".FutureNested", "\"Future\"", "\"PushBack.Future\"", "No editable aquí: ilegible o de versión futura",
         "Usa RACKEDITAR para editarlo con el editor clásico.", "control completo=Admitted"], []),
    "tests/RackCad.Tests/PushBackFullDraftCompositionTests.cs": (
        ["InvalidScaleReason", "BaseChangedReason", " huellas-iguales=True"], ["\"PB-NUEVO\"", "30", "4.5"]),
    "tests/RackCad.Tests/PushBackFullDraftViewTests.cs": (
        ["base", "+frente", "-frente", "frentes 1 y 2 en blanco", "declarar B", "B ranura 1 activa", "B ranura 1 en blanco", "retirar B",
         "FrontalBOfOneWay", "NoAddress", "AllObsolete=True"], []),
    "tests/RackCad.Tests/PushBackFullDraftClassifierTests.cs": (
        ["ancla Payload $.Structure.PalletTolerance grupo=Rack clase=ChangedExternally estructural=False",
         "ancla Payload $.Structure.PalletDepth grupo=Rack clase=ChangedByUser estructural=False",
         "ancla Payload $.Structure.PostPeralte grupo=Rack clase=Overlapping", "ancla Envelope $.Name grupo=Name clase=ChangedExternally",
         "grupo=OutsideEditor clase=ChangedExternally", " presente=True", " igual-al-oraculo=", "ViewsToErase", "ViewsRejected",
         "AllViewsObsolete=False"] + OWN27, ["4.5", "48.5", "3.25", "4.25"]),
    "tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs": (
        ["PushBackFullDraftState", "PushBackFullDraftReader", "PushBackFullComposition", "PushBackConflictClassifier",
         "PushBackViewDispositions", "WorkspaceHostCore.cs", "WorkspacePilotSupport.cs", " reglas-sin-coincidencias=True",
         "#nullable", "namespace RackCad.Application.Systems.PushBack.Draft", "Autodesk", "RackCad.Plugin", "RackCad.UI", "System.Windows",
         "Transaction", "Database", "async", "await", "Task", "File.", "System.IO", "RackEmbedComposer", "new RackEmbedDocument"], []),
}


def text_at(rev, path):
    return subprocess.run(["git", "show", rev + ":" + path], capture_output=True, text=True, encoding="utf-8", check=True).stdout


def counts(rev):
    result = {}
    for path, (lines, values) in CLASSES.items():
        text = text_at(rev, path)
        row = {"lineas-total": len(lines), "valores-total": len(values)}
        for literal in lines:
            row["L:" + literal] = text.count(literal)
        for value in values:
            row["V:" + value] = len(re.findall(r"(?<![\w.])" + re.escape(value) + r"(?![\w.])", text))
        result[path] = row
    return result


first = counts(sys.argv[1])
if len(sys.argv) == 2:
    for path, row in first.items():
        print(path.split("/")[-1], "lineas=" + str(row["lineas-total"]), "valores=" + str(row["valores-total"]),
              "ausentes=" + str([k for k, v in row.items() if k[:2] in ("L:", "V:") and v == 0]))
    sys.exit(0)

second = counts(sys.argv[2])
changed = [(p.split("/")[-1], k, first[p][k], second[p][k]) for p in first for k in first[p] if k[:2] in ("L:", "V:") and first[p][k] != second[p][k]]
print("literales con conteo distinto entre " + sys.argv[1][:8] + " y " + sys.argv[2][:8] + ": " + str(changed))
sys.exit(1 if changed else 0)
