"""PB-T4, v4.3 (B-08 con ReadAt, y CA-T4a I-01): las guardas de PB-T4a medidas sobre el texto literal del GREEN que prescribe el paquete (D-PBT4-02 y 03).

Uso: python -I measure_pbt4_v43.py <checkout> <rev> <salida.txt>
Solo `git show` (objetos git). Aplica en memoria, sobre el texto de la revisión, las ediciones literales del GREEN de PB-T4a
en WHC, NH y RIA (cada sustitución tiene que casar exactamente una vez) y evalúa, traducidas a Python, las aserciones de las
guardas que leen esos tres archivos por letra: las que cuentan `ReadIntent` o `_editIntentReader` sin anclar (EIG, WLG y PEG),
las del lector anidado de NH, las de RIA (la firma de EIG:139-141), las de la tabla de motivos y las de G-9.
"""
import re
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]


def show(path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    if p.returncode:
        raise SystemExit('git show %s -> %d' % (path, p.returncode))
    return p.stdout.decode('utf-8').replace('\r\n', '\n')


def code_only(text):
    """WorkspaceHostSource.CodeOnly: quita /* */ y // (como la guarda, sin mirar cadenas)."""
    return re.sub(r'//[^\r\n]*', '', re.sub(r'/\*.*?\*/', '', text, flags=re.S))


def body_of(code, signature):
    """WorkspaceHostSource.BodyOf."""
    m = re.search(signature, code)
    assert m, 'member not found: ' + signature
    start = m.start()
    open_ = code.find('{', m.end() - 1)
    arrow = code.find('=>', m.end() - 1)
    if arrow >= 0 and (open_ < 0 or arrow < open_):
        return code[start:code.find(';', arrow) + 1]
    depth = 0
    for i in range(open_, len(code)):
        if code[i] == '{':
            depth += 1
        elif code[i] == '}':
            depth -= 1
            if depth == 0:
                return code[start:i + 1]
    raise AssertionError('unbalanced')


def count(code, pattern):
    return len(re.findall(pattern, code))


def in_order(body, *parts):
    at = -1
    for p in parts:
        i = body.find(p, at + 1)
        if i < 0:
            return False
        at = i
    return True


def replace_once(text, old, new, what):
    n = text.count(old)
    assert n == 1, '%s: %d coincidencias' % (what, n)
    return text.replace(old, new)


WHC_P = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
NH_P = 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs'
RIA_P = 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs'

# ------------------------------------------------------------------ el GREEN literal (green_pbt4a.py; v4.2, con ReadAt)
import importlib.util
_spec = importlib.util.spec_from_file_location('gp', __file__.replace('measure_pbt4_v43.py', 'green_pbt4a.py'))
gp = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(gp)
_base, _green = gp.green(repo, rev)
whc_base, nh_base, ria_base = _base[WHC_P], _base[NH_P], _base[RIA_P]
whc, nh, ria = _green[WHC_P], _green[NH_P], _green[RIA_P]
REREADS = gp.REREADS

# ------------------------------------------------------------------ las aserciones de las guardas
WHC, NH, RIA = code_only(whc), code_only(nh), code_only(ria)
rows = []


def check(where, what, ok):
    rows.append((where, what, 'verde' if ok else 'ROJA'))


def bodies(core):
    return {
        'intent': body_of(core, r'public\s+EditIntentOutcome\s+TryBeginPilotEdit\s*\('),
        'check': body_of(core, r'public\s+EditCheckOutcome\s+TryCheckPilotEdit\s*\('),
        'confirm': body_of(core, r'public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\('),
        'keep': body_of(core, r'public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\('),
        'discard': body_of(core, r'public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\('),
        'reload': body_of(core, r'public\s+EditIntentOutcome\s+TryReloadPilotEdit\s*\(\s*EditIntentRequest\s+request\s*\)'),
        'update': body_of(core, r'public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\('),
        'gate': body_of(core, r'private\s+EditGate\s+PassGate\s*\('),
        'drain': body_of(core, r'public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)'),
        'advise': body_of(core, r'CloseDecision\s+IWorkspaceLifecycleAdvisor\.AdviseClose\s*\(\s*SessionId\s+\w+\s*\)'),
        'destroyed': body_of(core, r'void\s+IWorkspaceLifecycleAdvisor\.SessionDestroyed\s*\(\s*SessionId\s+\w+\s*,\s*bool\s+closeSequenceIntact\s*\)'),
        'mark': body_of(core, r'private\s+void\s+MarkPilotDraftsPossiblyStale\s*\(\s*SessionId\s+\w+\s*\)'),
        'hostdiscard': body_of(core, r'public\s+EditDiscardOutcome\s+DiscardPilotEdits\s*\(\s*SessionId\s+session\s*\)'),
    }


B = bodies(WHC)
B0 = bodies(code_only(whc_base))
# EIG:563-579
check('EIG:568-574', 'Begin: orden, 1 `.ReadIntent(` y 1 `_editIntentReader` seguido de `.`/`!`',
      in_order(B['intent'], 'string.IsNullOrWhiteSpace(request.RackId)', 'PassGate(', '_drafts.Contains(', '_editIntentReader!.ReadIntent(', '_drafts.Begin(')
      and count(B['intent'], r'\.ReadIntent\s*\(') == 1 and count(B['intent'], r'_editIntentReader\s*[.!]') == 1)
check('EIG:569-574', '«Comprobar ahora»: orden, 1 `.ReadIntent(` y 1 `_editIntentReader` seguido de `.`/`!`',
      in_order(B['check'], 'string.IsNullOrWhiteSpace(request.RackId)', 'PassGate(', '_drafts.Find<', '_editIntentReader!.ReadIntent(', '.CheckNow(')
      and count(B['check'], r'\.ReadIntent\s*\(') == 1 and count(B['check'], r'_editIntentReader\s*[.!]') == 1)
check('EIG:578-579', 'PassGate no lee', B['gate'] == B0['gate'] and not re.search(r'ReadIntent|_reader\b|_drafts|_bridge\.TryGetSession', B['gate']))
check('EIG:604', 'el drenaje del núcleo no nombra la intención', B['drain'] == B0['drain'] and not re.search(
    r'TryBeginPilotEdit|TryCheckPilotEdit|TryConfirmPilotBase|TryKeepPilotEdit|TryDiscardAndReloadPilotEdit|TryUpdatePilot|_editIntentReader|ReadIntent|PassGate', B['drain']))
check('EIG:620-623', 'solo WHC llama a `.ReadIntent(` (NH y RIA del GREEN no)',
      count(WHC, r'\.ReadIntent\s*\(') > 0 and count(NH, r'\.ReadIntent\s*\(') == 0 and count(RIA, r'\.ReadIntent\s*\(') == 0)
check('EIG:636', 'el drenaje sigue limpio (autoprueba)', B['drain'] == B0['drain'])
BASE_OR_READ = r'\.StaleBase\b|\.InitialState\b|\b(?:pilot|result)\.Read\b|\bpilot\s*[?!]?\s*\.\s*Read\b|\.Entries\b'
ALLOWED = r'\.(?:Begin|DiscardAndReload|ConfirmBase|KeepMineOnCurrentVersion)\s*\([^;]*\)\s*;|\.CheckNow\s*\([^;]*\)\s*;'
ADMISSION = r'\bpilot\.Read\.IsAdmitted\s*\?\s*null\s*:\s*pilot\.Read\.Admission\.Diagnostic\b'


def without_allowed(code):
    return re.sub(ADMISSION, '', re.sub(ALLOWED, '', code))


check('EIG:671-688', 'la base solo llega a sus consumidores; las cuentas por operación de `pilot.Read` y `.StaleBase`',
      not re.search(BASE_OR_READ, without_allowed(WHC))
      and count(B['intent'], r'\bpilot\.Read\b') == 1 and count(B['check'], r'\.StaleBase\b') == 1
      and count(B['check'], ADMISSION) == 1 and count(B['check'], r'\bpilot\.Read\b') == 2)
check('EIG:695-718', 'las cuentas de los tres cuerpos de la reconciliación',
      count(B['confirm'], r'\.StaleBase\b') == 1 and count(B['confirm'], r'\bpilot\.Read\b') == 2
      and count(B['keep'], r'\bpilot\.Read\b') == 1 and count(B['discard'], r'\bpilot\.Read\b') == 1)
check('EIG:733-736', 'seis `var pilot = read.PilotRead;` y seis `.ReadIntent(` en WHC',
      count(WHC, r'var\s+pilot\s*=\s*read\.PilotRead\s*;') == 6 and count(WHC, r'\.ReadIntent\s*\(') == 6
      and not re.search(r'\.PilotRead\b', re.sub(r'var\s+pilot\s*=\s*read\.PilotRead\s*;', '', WHC)))
check('EIG:761-769', 'los tres cuerpos: orden, 1 `.ReadIntent(`, 1 `_editIntentReader` con `.`/`!` y la marca después de la lectura',
      in_order(B['confirm'], 'string.IsNullOrWhiteSpace(request.RackId)', 'HasWitnessWithoutKey(request)', 'PassGate(', '_drafts.Find<', 'NotCheckableReason', '_editIntentReader!.ReadIntent(', '.ConfirmBase(')
      and in_order(B['keep'], 'string.IsNullOrWhiteSpace(request.RackId)', 'HasWitnessWithoutKey(request)', 'PassGate(', '_drafts.Find<', 'DraftStatus.Applying', 'NotKeepableReason', '_editIntentReader!.ReadIntent(', '.KeepMineOnCurrentVersion(')
      and in_order(B['discard'], 'string.IsNullOrWhiteSpace(request.RackId)', 'HasWitnessWithoutKey(request)', 'PassGate(', '_drafts.Find<', 'DraftStatus.Applying', '_editIntentReader!.ReadIntent(', '.DiscardAndReload(')
      and all(count(B[b], r'\.ReadIntent\s*\(') == 1 and count(B[b], r'_editIntentReader\s*[.!]') == 1
              and B[b].find('_knownClean.Remove(') > B[b].find('_editIntentReader!.ReadIntent(') for b in ('confirm', 'keep', 'discard')))
check('EIG:1124-1130', '«Actualizar» no lee', B['update'] == B0['update'])
check('WLG:369', 'AdviseClose', B['advise'] == B0['advise'])
check('WLG:442', 'SessionDestroyed', B['destroyed'] == B0['destroyed'])
check('WLG:623', 'el drenaje', B['drain'] == B0['drain'])
check('WLG:634', 'MarkPilotDraftsPossiblyStale', B['mark'] == B0['mark'])
check('WLG:645-656', 'la recarga: orden, 1 `.ReadIntent(`, 1 `_editIntentReader` con `.`/`!`, 1 `pilot.Read`',
      in_order(B['reload'], 'string.IsNullOrWhiteSpace(request.RackId)', 'HasWitnessWithoutKey(request)', 'PassGate(', '_drafts.Find<PushBackPilotDraftState>(', '.NeedsLazyReload', '_editIntentReader!.ReadIntent(', '.DiscardAndReload(pilot.Read);')
      and count(B['reload'], r'\.ReadIntent\s*\(') == 1 and count(B['reload'], r'_editIntentReader\s*[.!]') == 1 and count(B['reload'], r'\bpilot\.Read\b') == 1)
check('WLG:687', 'el descarte explícito del anfitrión', B['hostdiscard'] == B0['hostdiscard'])
changed = [n for n in B if B[n] != B0[n]]
check('PEG:288 y :297', 'solo cambian los cinco cuerpos que ya leían (ninguna operación sin lectura ni de «Actualizar»): ' + ', '.join(sorted(changed)),
      sorted(changed) == sorted(['check', 'confirm', 'keep', 'discard', 'reload']))
check('Reflexión (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503, IG:427-428)', 'WHC no declara nada nuevo: los mismos miembros declarados',
      sorted(re.findall(r'^\s*(?:public|private|internal|protected)[^\n;=(]*?\b(\w+)\s*(?:\(|;|=|\{)', code_only(whc_base), re.M))
      == sorted(re.findall(r'^\s*(?:public|private|internal|protected)[^\n;=(]*?\b(\w+)\s*(?:\(|;|=|\{)', WHC, re.M)))
# NH
reader = body_of(NH, r'private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(')
check('EIG:409-431', 'el lector anidado: los ocho pasos en orden; 1 `LoadCatalog(`, 1 `RackPilotIntentAdapter.Read(` y 1 `_targets.Revalidate(` en NH',
      in_order(reader, 'MdiActiveDocument', 'CommandInProgress', 'RackPilotIntentAdapter.LoadCatalog()', 'LockDocument()', '_authorize(', '_targets.Revalidate(',
               'WorkspaceEditIntent.ChooseOriginDefinition(', 'RackPilotIntentAdapter.Read(')
      and count(NH, r'RackPilotIntentAdapter\.LoadCatalog\s*\(') == 1 and count(NH, r'RackPilotIntentAdapter\.Read\s*\(') == 1
      and count(NH, r'_targets\.Revalidate\s*\(') == 1)
check('EIG:441-453', 'NH: dos clases y el lector anidado implementa la interfaz', count(NH, r'\bclass\s+\w+') == 2
      and re.search(r'private\s+sealed\s+class\s+PilotIntentReader\s*:\s*IWorkspaceEditIntentReader\b', NH) is not None)
check('EIG:465-470', 'NH: un `read.Pilot` y ningún `PushBackPilot` ni `RackCad.Application.Systems`',
      count(NH, r'\bread\.Pilot\b') == 1 and not re.search(r'PushBackPilot|RackCad\.Application\.Systems', NH))
inside = body_of(NH, r'private\s+sealed\s+class\s+PilotIntentReader\s*:\s*IWorkspaceEditIntentReader')
FORM = r'var\s+decision\s*=\s*_authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*session\s*\)\s*,\s*document\s*\.\s*UnmanagedObject\s*\.\s*ToInt64\s*\(\s*\)\s*\)\s*;\s*if\s*\(\s*decision\s*!=\s*RequestDecision\.Accepted\s*\)\s*return\s+EditIntentReadResult\.Rejected\s*\('
check('RG:696-712', 'cada `_authorize(` del lector tiene la forma exacta', count(inside, r'\b_authorize\s*\(') >= 1 and count(inside, FORM) == count(inside, r'\b_authorize\s*\('))
SPANISH_NH = ['No hay documento activo.', 'Hay un comando en curso.', 'La petición no es del documento activo.']
check('CAG:410-451 (NH)', 'ReadOnActiveDocument: los tres motivos en español y ninguno en inglés', all('"%s"' % s in reader for s in SPANISH_NH))
check('NHT:418-559 y WBHG:72-168', 'las cuentas de NH (`RackLog.Exception(` 1, `ReturnFocusToDrawing(` 2, `.Focus(` 1, sin async/await; los `+=` 2 de NHT:984 y WBHG:218 son de la paleta, no de NH)',
      count(NH, r'RackLog\.Exception\s*\(') == 1 and count(NH, r'\bReturnFocusToDrawing\s*\(') == 2 and count(NH, r'\.Focus\s*\(') == 1
      and count(NH, r'\+=') == count(code_only(nh_base), r'\+=') and count(NH, r'\basync\b') == 0 and count(NH, r'\bawait\b') == 0)
check('IG:684-685 (G-9)', 'NH: `LockDocument` 2', count(NH, r'LockDocument') == count(code_only(nh_base), r'LockDocument') == 2)
# RIA
check('EIG:132-147', 'RIA: la firma de `Read` (:139-141), tres clases de resultado y nada público',
      re.search(r'internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(\s*Document\s+document\s*,\s*string\s+rackId\s*,\s*long\s+originDefinitionHandle\s*,\s*string\??\s+witnessKey\s*,\s*string\??\s+witnessText\s*,\s*RackCatalog\s+catalog\s*\)', RIA) is not None
      and count(RIA, r'internal\s+sealed\s+class\s+RackPilot\w+Result\b') == 3
      and not re.search(r'\bpublic\s+(?:sealed\s+|static\s+)*(?:class|struct|record|enum|interface)\b', RIA) and ria.startswith('#nullable enable'))
check('EIG:150-160', 'RIA: ningún campo de un objeto del anfitrión ni campo estático',
      not re.search(r'^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:[\w.]+\.)?(?:Transaction|DBObject|Document|Database|Entity|BlockReference|ObjectId|Editor)\b[^(]*[;=]', RIA, re.M)
      and not re.search(r'\bstatic\s+(?:readonly\s+)?[\w<>,.?]+\s+\w+\s*(?:;|=(?!>)|\{\s*get)', RIA))
check('EIG:384-388', 'RIA no sabe nada del Workspace ni de la navegación',
      not re.search(r'RackCad\.Application\.Workspace', RIA) and not re.search(
          r'\b(?:NavigationTargets|NavigationTargetSet|NavigationTarget|NavigationDefinitionFact|INavigationFactsPort|NavigationDiagnostic|NavigationViewOrder|NavigationRevalidation|NavigationActionPolicy|NavigationActionKind|INavigationEffects|NavigationEcho)\b|WorkspaceNavigation|IWorkspaceEditIntentReader|EditIntentReadResult|EditIntentWitness|WorkspaceHostCore|WorkspaceController', RIA))
read_body = body_of(RIA, r'internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(')
SPANISH_RIA = ['El testigo de la vista previa no tiene clave.', 'La exploración del dibujo no trajo las envolventes en bruto.', 'El documento está bloqueado por otra operación.', 'No se pudo leer el dibujo: ', 'No se pudo leer la intención de edición: ']
check('CAG:410-464 (RIA)', 'Read: los cinco motivos en español y dos `ex.Message`', all('"%s"' % s in read_body for s in SPANISH_RIA) and count(read_body, r'ex\.Message') == 2)
check('IG:684 (G-9)', 'RIA: `StartTransaction` 1 y `LockDocument` 1', count(RIA, r'StartTransaction') == count(code_only(ria_base), r'StartTransaction') == 1
      and count(RIA, r'LockDocument') == count(code_only(ria_base), r'LockDocument') == 1)
at_origin = body_of(RIA, r'internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\(')
check('EIG:350-364 (B-08)', 'RIA: un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(` en todo el archivo, y el orden de `Read`',
      count(RIA, r'\bnew\s+PushBackPilotIntentScan\s*\(') == 1 and count(RIA, r'PushBackPilotIntentReader\.Read\s*\(') == 1
      and re.search(r'new\s+PushBackPilotIntentScan\s*\(\s*rackId\s*,\s*originKey\s*,\s*scan\.Facts\s*,\s*scan\.RawEnvelopes\s*,\s*CompletenessEvidence\s*,\s*witness\s*\)', read_body) is not None
      and in_order(read_body, 'string.IsNullOrWhiteSpace(witnessKey)', 'CaptureDrawing(', 'new PushBackPilotIntentScan(', 'PushBackPilotIntentReader.Read(')
      and not re.search(r'new\s+RackEmbedDocument|RackEmbedDocument|RackEmbedStore|RackEmbedComposer', RIA))
check('EIG:322-336 y :338-347', 'RIA: un `CaptureDrawing(` (y en `Read`), un `new Handle(`, sin `Handle.Parse` ni `GetObjectId(`',
      count(RIA, r'\bCaptureDrawing\s*\(') == 1 and count(read_body, r'\bCaptureDrawing\s*\(') == 1 and count(RIA, r'\bnew\s+Handle\s*\(') == 1
      and not re.search(r'Handle\.Parse|Convert\.ToInt64|\bnew\s+ObjectId\b|(?<!Try)GetObjectId\s*\(', RIA))
check('CT-PBT4A-07 (nueva)', '`AtOrigin` solo llama a `ReadAt`: sin lecturas propias, sin `CaptureDrawing`, bloqueo, transacción, literal de cadena ni `ex.Message`; RIA nombra una vez cada lector, en `Read`',
      count(at_origin, r'PushBackOriginByText\.ReadAt\s*\(') == 1
      and not re.search(r'CaptureDrawing|LockDocument|Transaction|AssertNoOpenTransaction|"|ex\.Message|PushBackPilotIntentReader|PushBackFullDraftReader|BuildStaleBase|\.Choose\s*\(|\.AtKey\s*\(', at_origin)
      and count(RIA, r'PushBackFullDraftReader\.Read\s*\(') == 1
      and count(read_body, r'PushBackFullDraftReader\.Read\s*\(\s*input\s*,\s*catalog\s*\)') == 1
      and read_body.find('PushBackPilotIntentReader.Read(input, catalog)') < read_body.find('PushBackFullDraftReader.Read(input, catalog)'))
DRAFT = _green[gp.DRAFT_P]
check('CT-PBT4A-04 y 08 (Draft)', '`PushBackOriginByText`: `Choose`, `AtKey` y `ReadAt`, con un lector de cada; ningún término de la pureza de CT-PBT3A-14',
      count(DRAFT, r'PushBackPilotIntentReader\.Read\s*\(') == 1 and count(DRAFT, r'PushBackFullDraftReader\.Read\s*\(') == 1
      and not [t for t in ["Autodesk", "RackCad.Plugin", "RackCad.UI", "System.Windows", "Transaction", "Database", "async", "await", "Task", "File.", "System.IO", "RackEmbedComposer", "new RackEmbedDocument"] if t in DRAFT]
      and re.search(r'^\s*#nullable', DRAFT, re.M) is None
      and len(re.findall(r'^namespace RackCad\.Application\.Systems\.PushBack\.Draft\s*\n\{', DRAFT, re.M)) == 1)

VALUE_TYPES = {'bool', 'byte', 'sbyte', 'char', 'decimal', 'double', 'float', 'int', 'uint', 'long', 'ulong', 'short', 'ushort',
               'DateTime', 'Guid', 'TimeSpan', 'SessionId', 'EditIntentReadResult'}
REF_Q = re.compile(r'(?<![\w.])([A-Za-z_][\w.]*(?:<[^<>()]*>)?)\?(?=\s+[A-Za-z_]|\s*[,)>\]])')


def ref_annotations(path):
    """Las anotaciones `T?` de tipo de referencia en las líneas que el GREEN añade, si el archivo no lleva `#nullable enable`."""
    text = _green[path]
    if text.lstrip('﻿').startswith('#nullable enable'):
        return []
    old = set(_base.get(path, '').split('\n'))
    hits = []
    for line in text.split('\n'):
        if line in old:
            continue
        code = re.sub(r'//.*', '', re.sub(r'"(?:\\.|[^"\\])*"', '""', line))
        for m in REF_Q.finditer(code):
            if m.group(1).split('<')[0] not in VALUE_TYPES:
                hits.append(m.group(0))
    return hits


offenders = {gp.SHORT if False else p.rsplit('/', 1)[-1]: ref_annotations(p) for p in _green}
check('CA-T4a I-01 (la regla del §5.0)', 'ningún `?` de tipo de referencia en lo que el GREEN añade a un archivo sin `#nullable enable` (NH y `Draft/PushBackOriginByText.cs`; WHC, WEI, RIA y la interfaz lo llevan), y ningún `#nullable` en `Draft/`: '
      + ('; '.join('%s: %s' % (k, ', '.join(v)) for k, v in offenders.items() if v) or 'ninguno'),
      not any(offenders.values()) and '#nullable' not in _green[gp.DRAFT_P])
_v42_lines = ['        public static string? Choose(DraftStaleBase staleBase, string originText)',
              '                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.Ordinal),']
_v42_hits = [m.group(0) for l in _v42_lines for m in REF_Q.finditer(l)]
rows.append(('Contrafactual (CA-T4a I-01)', 'con el texto de la v4.2 de `PushBackOriginByText.cs`, la regla ve ' + ', '.join('`%s`' % h for h in _v42_hits),
             'ROJA (esperada)' if len(_v42_hits) == 2 else 'verde'))

SIG_RX = r'internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(\s*Document\s+document\s*,\s*string\s+rackId\s*,\s*long\s+originDefinitionHandle\s*,\s*string\??\s+witnessKey\s*,\s*string\??\s+witnessText\s*,\s*RackCatalog\s+catalog\s*\)'
for label, extra in [('obligatorio', ',\n            string? originText)'), ('con valor por defecto', ',\n            string? originText = null)')]:
    variant = code_only(ria_base).replace('            RackCatalog catalog)\n', '            RackCatalog catalog' + extra + '\n', 1)
    rows.append(('Contrafactual (EIG:139-141)', 'si `Read` ganara `originText` como parámetro ' + label + ', la firma fijada por la guarda deja de casar',
                 'ROJA (esperada)' if re.search(SIG_RX, variant) is None else 'verde'))

lines = ['# PB-T4a: las guardas sobre el texto literal del GREEN (rev %s)' % rev, '',
         'WHC: la sentencia de D-PBT4-03 en %d cuerpos (%s); NH y RIA: las ediciones literales de D-PBT4-02 y 03.' % (len(REREADS), ', '.join(REREADS)), '',
         '| Guarda | Qué se comprueba | Sobre el GREEN |', '|---|---|---|']
lines += ['| %s | %s | **%s** |' % r for r in rows]
lines += ['', 'rojas=%d de %d sobre el GREEN; contrafactuales en rojo=%d de 3' % (
    sum(1 for r in rows if r[2] == 'ROJA'), sum(1 for r in rows if not r[0].startswith('Contrafactual')),
    sum(1 for r in rows if r[2] == 'ROJA (esperada)'))]
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines[-1:]))
