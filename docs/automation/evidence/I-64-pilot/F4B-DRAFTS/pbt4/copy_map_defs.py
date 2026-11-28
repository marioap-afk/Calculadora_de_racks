"""PB-T4b, «Saltar y duplicar»: el mapa cerrado M de la copia mecánica y lo que el mapa no cubre (las firmas de residuo).

Lo usan copy_map_pbt4.py y probe_copy_residue.py. Es el mismo mapa que fija el §D-PBT4-11 del paquete v4.1.
"""
import re

NINE = ['WorkspaceLifecycleHostTests', 'WorkspacePilotPrepareTests', 'WorkspaceEditIntentHostTests', 'WorkspacePilotEditHostTests',
        'WorkspaceCommandAdvisorTests', 'WorkspacePilotUpdateRequestTests', 'WorkspacePilotUpdateHostTests',
        'WorkspacePilotReconciliationHostTests', 'WorkspaceAssociativeBookkeepingTests']

# El mapa M, cerrado y literal, en este orden.
M = [
    ('M-01', r'\bEditIntentFakeReader\b', 'FullEditIntentFakeReader'),
    ('M-02', r'\bEditIntentKit\b', 'FullEditIntentKit'),
    ('M-03', r'\bPrepareKit\b', 'FullPrepareKit'),
    ('M-04', r'\bPushBackPilotDraftState\b', 'PushBackFullDraftState'),
    ('M-05', r'\bPushBackPilotStateComparer\b', 'PushBackFullDraftStateComparer'),
    ('M-06', r'\bPushBackPilotFieldAdapter\b', 'PushBackFullFieldAdapter'),
    ('M-07', r'\.Inputs\.Annotations!?\.AnnotationScale\b', '.Editable.AnnotationScale'),
    ('M-08', r'\.Inputs\.PalletTolerance\b', '.Editable.PalletTolerance'),
]
MRX = [(k, re.compile(a), b) for k, a, b in M]

# Lo que el mapa no cubre, mirado sobre el texto ya mapeado.
RESIDUAL = [
    ('R-retirado', re.compile(r'KeepPilotEditsOn|ComposeForWrite|PushBackPilotUpdatePlanner\.Prepare\s*\(|PushBackPilotUpdateRun\.(?:Execute|Precheck)\s*\(')),
    ('R-estado', re.compile(r'\.Inputs\b|\bnew\s+PushBackFullDraftState\s*\(|\b(?:state|DraftState)\.Editor\b')),
    ('R-C2', re.compile(r'OutOfPilotDiagnostic|CompositeOutOfPilotReason')),
    ('R-C3', re.compile(r'\bTryKeepMine\b')),
    ('R-C4', re.compile(r'NonPilotContentReason|TryUpdatePilot\b')),
    ('R-C9', re.compile(r'\bEditIntentReadResult\.FromPilot\s*\(')),
]


KIT_TYPES = ('EditIntentFakeReader', 'EditIntentKit', 'PrepareKit')
TOP = re.compile(r'^(?:(?:public|internal|static|sealed|abstract|file)\s+)*(?:partial\s+)?(?:class|record|struct|interface|enum)\s+(\w+)', re.M)


def without_kit(code, masked):
    """El texto del archivo con los tipos del kit vaciados (no se copian: los sustituye el kit nuevo). Conserva las líneas.
    `masked` es el mismo texto sin cadenas ni comentarios, para contar las llaves."""
    keep = list(code)
    for m in TOP.finditer(masked):
        if m.group(1) in KIT_TYPES:
            i = masked.index('{', m.end())
            d, j = 1, i + 1
            while d:
                d += {'{': 1, '}': -1}.get(masked[j], 0)
                j += 1
            for k in range(m.start(), j):
                if keep[k] != '\n':
                    keep[k] = ' '
    return ''.join(keep)


def top_types(masked):
    return [m.group(1) for m in TOP.finditer(masked)]


def apply_map(text):
    used = []
    for k, rx, rep in MRX:
        text, n = rx.subn(rep, text)
        if n:
            used.append(k)
    return text, used
