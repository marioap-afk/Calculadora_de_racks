# Solo lectura: imprime la primera linea (recortada) de cada ancla ruta:linea del paquete de PB-T1, para la revision.
# Uso: python -I anchors.py <raiz-del-worktree>
import sys
import os

root = sys.argv[1]
A = {
    "W": "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs",
    "WC": "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs",
    "CS": "src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs",
    "ES": "src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs",
    "EL": "src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs",
    "DA": "src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs",
    "CA": "src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorAssembler.cs",
    "PC": "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs",
    "PM": "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs",
    "RMES": "src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs",
    "BOG": "src/RackCad.Application/Bom/RackBomOutputGate.cs",
    "REI": "src/RackCad.UI/Editor/RackEditorIdentity.cs",
    "DLG": "src/RackCad.Application/Systems/Dynamic/DynamicRackLevelGeometry.cs",
    "PGT": "tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs",
    "I55": "tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs",
    "DSV": "tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs",
    "WCG": "tests/RackCad.UI.Tests/WindowCensusGuardTests.cs",
    "RAM": "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs",
    "DRG": "tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs",
    "AP": "docs/AUTOMATION_PLAN.md",
}
CHECKS = [
    ("W", 56), ("W", 82), ("W", 84), ("W", 87), ("W", 91), ("W", 101), ("W", 106), ("W", 109), ("W", 114),
    ("W", 131), ("W", 136), ("W", 160), ("W", 165), ("W", 193), ("W", 270), ("W", 273), ("W", 276), ("W", 279), ("W", 280),
    ("W", 286), ("W", 288), ("W", 294), ("W", 299), ("W", 302), ("W", 305), ("W", 311), ("W", 316), ("W", 319), ("W", 322),
    ("W", 342), ("W", 373), ("W", 380), ("W", 385), ("W", 390), ("W", 391), ("W", 399), ("W", 401), ("W", 406), ("W", 411),
    ("W", 413), ("W", 414), ("W", 420), ("W", 421), ("W", 423), ("W", 427), ("W", 439), ("W", 616), ("W", 635), ("W", 659),
    ("W", 674), ("W", 675), ("W", 679), ("W", 680), ("W", 700), ("W", 711), ("W", 802), ("W", 818), ("W", 822), ("W", 823),
    ("W", 826), ("W", 833), ("W", 838), ("W", 846), ("W", 853), ("W", 859), ("W", 868), ("W", 878), ("W", 880), ("W", 884),
    ("W", 1234), ("W", 1237), ("W", 1249), ("W", 1256), ("W", 1260), ("W", 1268), ("W", 1291), ("W", 1303), ("W", 1346),
    ("W", 1639), ("W", 1646), ("W", 1905), ("W", 2038), ("W", 2039), ("W", 2535), ("W", 2539), ("W", 3044), ("W", 3048),
    ("W", 3131), ("W", 3135), ("W", 3224), ("W", 3261), ("W", 3277), ("W", 3307), ("W", 3320), ("W", 3327), ("W", 3328),
    ("W", 3336), ("W", 3352), ("W", 3405), ("W", 3409), ("W", 3478), ("W", 3498), ("W", 3503), ("W", 3504), ("W", 3507),
    ("W", 3524), ("W", 3552), ("W", 3565), ("W", 3620), ("W", 3634), ("W", 3665), ("W", 3667),
    ("WC", 43), ("WC", 119), ("WC", 198), ("WC", 416), ("WC", 429), ("WC", 432), ("WC", 452), ("WC", 580), ("WC", 783),
    ("WC", 789), ("WC", 803), ("WC", 806), ("WC", 810), ("WC", 813), ("WC", 827), ("WC", 829), ("WC", 860), ("WC", 868),
    ("WC", 870), ("WC", 927), ("WC", 931), ("WC", 933),
    ("CS", 54), ("CS", 58), ("CS", 61), ("CS", 85), ("CS", 95), ("CS", 116), ("CS", 147), ("CS", 167), ("CS", 179),
    ("CS", 629), ("CS", 631), ("CS", 1026),
    ("ES", 65), ("ES", 110), ("ES", 112), ("ES", 143), ("ES", 169), ("ES", 178), ("ES", 259),
    ("EL", 187), ("EL", 195), ("EL", 197), ("EL", 229),
    ("DA", 57), ("DA", 81), ("DA", 101), ("DA", 105), ("DA", 155), ("DA", 159), ("DA", 169), ("DA", 171), ("DA", 198),
    ("DA", 210), ("DA", 279), ("DA", 302), ("DA", 383), ("DA", 436), ("DA", 508), ("DA", 529), ("DA", 535), ("DA", 571),
    ("CA", 52), ("CA", 89), ("CA", 182),
    ("PC", 37), ("PC", 49), ("PC", 53), ("PC", 58), ("PC", 64), ("PC", 83), ("PC", 85), ("PC", 111), ("PC", 134),
    ("PC", 171), ("PC", 177), ("PC", 309), ("PC", 350),
    ("PM", 106), ("PM", 108), ("PM", 148), ("PM", 150), ("PM", 160), ("PM", 170), ("PM", 172), ("PM", 196), ("PM", 217),
    ("PM", 227), ("PM", 234), ("PM", 242), ("PM", 251),
    ("RMES", 16), ("RMES", 50), ("RMES", 74), ("RMES", 87), ("RMES", 185), ("RMES", 732), ("RMES", 742), ("RMES", 744),
    ("RMES", 750), ("RMES", 769), ("RMES", 799), ("RMES", 811), ("RMES", 818), ("RMES", 846),
    ("BOG", 49), ("BOG", 59), ("REI", 36), ("REI", 39), ("DLG", 200), ("DLG", 204),
    ("PGT", 235), ("PGT", 272), ("I55", 8), ("I55", 15), ("DSV", 311), ("DSV", 318), ("WCG", 86), ("WCG", 120),
    ("RAM", 84), ("RAM", 92), ("DRG", 232), ("DRG", 247), ("AP", 557), ("AP", 558),
]
cache = {}
for alias, line in CHECKS:
    rel = A[alias]
    if rel not in cache:
        path = os.path.join(root, *rel.split("/"))
        if not os.path.exists(path):
            cache[rel] = None
        else:
            with open(path, encoding="utf-8-sig") as f:
                cache[rel] = f.read().split("\n")
    lines = cache[rel]
    if lines is None:
        print(f"{alias}:{line}\tFALTA {rel}")
        continue
    text = lines[line - 1].strip() if 0 < line <= len(lines) else "<fuera de rango>"
    print(f"{alias}:{line}\t{text[:150]}")
