"""Arma la v4.4 del paquete de PB-T4 desde la v4.3 y los fragmentos de v44/ (solo lee archivos de hs65/pbt4 y escribe la v4.4).
Uso: python -I make_v44.py"""
import hashlib, os, re, sys
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
V43 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v43.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v44.md')
raw = open(V43, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '4f4822798920d3f15e3c78f349b702549bef4eced6c5f26009252272247cb9d2'
t = raw.decode('utf-8')
assert '\r' not in t


def frag(name):
    return open(os.path.join(HERE, 'v44', name), encoding='utf-8').read()


def sha(name):
    return hashlib.sha256(open(os.path.join(HERE, name), 'rb').read()).hexdigest()


def once(text, old, new, what):
    n = text.count(old)
    assert n == 1, (what, n)
    return text.replace(old, new)


def section(text, head, until):
    """El tramo desde la línea que empieza por head hasta la siguiente que empieza por until (sin incluirla)."""
    i = text.index('\n' + head) + 1
    j = text.index('\n' + until, i) + 1
    return text[i:j]


# lo que no puede cambiar: PB-T4a (§5.1, §6.1 con 6.1.1 y 6.1.2)
KEEP = [section(t, '### 5.1 PB-T4a', '### 5.2 PB-T4b'), section(t, '### 6.1 PB-T4a', '### 6.2 PB-T4b')]

# ---------------------------------------------------------------- los literales que se derivan del contrato de PB-T4a (§6.1.1 de la v4.3)
block = section(t, '#### 6.1.1 ', '#### 6.1.2 ')
lines = {l.split(':', 1)[0]: l for l in block.split('\n') if re.match(r'^PBT4A-C\d\d:', l)}
EE0 = [l for l in block.split('\n') if l.startswith('[0] ')][0]


def to_b(line):
    return line.replace('PBT4A-', 'PBT4B-', 1)


C07 = to_b(lines['PBT4A-C07'])
C07 = once(C07, '(the self-tests of CT-PBT4A-07 and CT-PBT4A-08)', '(the self-tests of CT-PBT4B-12, CT-PBT4B-13, CT-PBT4B-15, CT-PBT4B-19 and CT-PBT4B-20)', 'C07 recetas')
C07 = once(C07, 'a writing error in a new test of this task', 'a writing error in a new CT of this task (one of its six classes; never the kit nor the copies, whose text is the package one)', 'C07 RR')
C07 = C07 + ' A copy or a hand-written equivalent of the list that fails in the GREEN is this stop too: the session issues a package version with its hand-written text (D-S1).'
C08, C12, C17, C18 = (to_b(lines[k]) for k in ('PBT4A-C08', 'PBT4A-C12', 'PBT4A-C17', 'PBT4A-C18'))
C11 = to_b(lines['PBT4A-C11']).replace('PB-T4a', 'PB-T4b')
assert C11.endswith('no one-off acceptance extends to PB-T4b') and not re.search(r'PBT3C|PB-T3c|PB-T4a|PBT4A', C11)

OBJECTIVE = (
    'Pasar el borrador del panel del Workspace de Push Back al modelo completo de PB-T3 con la lectura completa de PB-T4a, sin traducir estados: '
    'el borrador nace de la lectura completa y el anfitrion lo busca como completo, con la admision y el diagnostico de esa lectura; OB-03.2 (los '
    'racks A/B se editan en el panel; la cola y AUTH-13 siguen rechazados); la regla unica de la escala en Application (la logica de la validacion '
    'numerica pasa de la UI a NumericTextRule y la UI delega en ella sin cambiar su comportamiento; el adaptador del campo usa '
    'PushBackAnnotationScaleRule por defecto); Actualizar con Precheck y ExecuteFull; y Conservar mi borrador con dos Si: el primero llama a '
    'TryKeepMine sin testigo y presenta los cambios por grupos, clasificados por una funcion pura de Draft, con el testigo de lo releido; el '
    'segundo conserva con KeepOn solo si ese testigo sigue valiendo. El codigo del piloto queda sin uso, sin retirarlo (lo retira PB-T4d), segun '
    'el paquete del contrato de PB-T4 v4.4, las decisiones del Owner sobre PB-T4 (las tres tandas), la lista cerrada CT-PBT4B-01..20 y la lista '
    'exacta de 475 sustituciones que el Owner tomo como la que vio: cada una con su Skip de motivo literal y su copia o su equivalente, literales '
    'del paquete, sin editar ni borrar ninguna prueba ni ningun ayudante. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro '
    'de la vida del borrador: STOP y se reabre L-1. Ni Editar todo, ni RACKEDITAR, ni la ventana; no se ejecuta AutoCAD.')

# ---------------------------------------------------------------- RequiredTests: los 38 heredados (con los Skip) y los nuevos
req_rows = [l for l in block.split('\n') if re.match(r'^\| \d+ \| `tests/', l)]
assert len(req_rows) == 38
NEWMIN = {2: '**2**', 5: '**85**', 6: '**121**', 11: '**1**', 12: '**2719**', 15: '**521**', 1: '5', 14: '2'}
CORE, UIP = '`tests/RackCad.Tests/RackCad.Tests.csproj`', '`tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj`'
out_rows = []
for l in req_rows:
    c = [x.strip() for x in l.strip('|').split('|')]
    n = int(c[0])
    mins = NEWMIN.get(n, c[3].replace('**', ''))
    out_rows.append('| %d | %s | %s | %s | false |' % (n, c[1], c[2], mins))
new = [
    (CORE, 'RackCad.Tests.PushBackFullPanelTests', '11', 'true'),
    (CORE, 'RackCad.Tests.PushBackFullPanelGuardTests', '3', 'true'),
    (CORE, 'RackCad.Tests.PushBackFullPanelCensusGuardTests', '1', 'false'),
    (CORE, 'RackCad.Tests.PushBackPanelDraftGuardTests', '1', 'false'),
    (CORE, 'RackCad.Tests.WorkspaceFullCopyGuardTests', '1', 'false'),
    (UIP, 'RackCad.UI.Tests.PushBackFullPanelFieldTests', '3', 'true'),
    (UIP, 'RackCad.UI.Tests.NumericFieldValidationTests', '13', 'false'),
    (UIP, 'RackCad.UI.Tests.NumericFieldTests', '10', 'false'),
]
k = 39
for proj, f, m, e in new:
    out_rows.append('| %d | %s | `FullyQualifiedName~%s` | %s | %s |' % (k, proj, f, m, e))
    k += 1
STEMS = ['WorkspaceLifecycleHost', 'WorkspacePilotPrepare', 'WorkspaceEditIntentHost', 'WorkspacePilotEditHost', 'WorkspaceCommandAdvisor',
         'WorkspacePilotUpdateRequest', 'WorkspacePilotUpdateHost', 'WorkspacePilotReconciliationHost', 'WorkspaceAssociativeBookkeeping',
         'PushBackWritePathGuard', 'WorkspacePilotUpdateIntegrationGuard', 'WorkspaceEditIntentGuard', 'WorkspacePilotEditGuard',
         'WorkspacePilotReconciliationGuard', 'WorkspaceLifecycleGuard', 'WorkspacePilotUpdateGuard', 'WorkspacePilotReconciliation',
         'WorkspacePilotReconciliationView']
for s in STEMS:
    ui = s in ('WorkspacePilotReconciliation', 'WorkspacePilotReconciliationView')
    ns = 'RackCad.UI.Tests.FullPanelCopies' if ui else 'RackCad.Tests.FullPanelCopies'
    for part, e in (('Red', 'true'), ('Green', 'false')):
        out_rows.append('| %d | %s | `FullyQualifiedName~%s.%sFull%sTests` | `<medido en la precondicion 6>` | %s |' % (k, UIP if ui else CORE, ns, s, part, e))
        k += 1
REQ_ROWS = '\n'.join(out_rows)

# ---------------------------------------------------------------- los sha256 de lo custodiado
KIT_SHA = sha('pbt4b-kit-WorkspaceFullReadKit.cs')
GEN_SHA = sha('make_copies_pbt4b.py')
EDITS_SHA = sha('pbt4b_hand_edits.py')
REVIEW_SHA = sha('pbt4b-equivalents-review.txt')
PROBE_SET = [l.split()[1] for l in open(os.path.join(HERE, 'v44', 'copies_probe_v44-out.txt'), encoding='utf-8').read().split('\n') if l.startswith('conjunto ')]
assert len(PROBE_SET) == 1
PROBE_SET = PROBE_SET[0]

f62 = frag('f-62.md')
for key, val in (('{OBJECTIVE}', OBJECTIVE), ('{C07}', C07), ('{C08}', C08), ('{C11}', C11), ('{C12}', C12), ('{C17}', C17), ('{C18}', C18),
                 ('{EE0}', EE0), ('{REQ_ROWS}', REQ_ROWS), ('{KIT_SHA}', KIT_SHA), ('{GEN_SHA}', GEN_SHA), ('{EDITS_SHA}', EDITS_SHA),
                 ('{REVIEW_SHA}', REVIEW_SHA), ('{PROBE_SET}', PROBE_SET)):
    f62 = once(f62, key, val, key)
code = f62[f62.index('```text\n') + 8:f62.index('\n```', f62.index('```text\n'))]
bad = [ch for ch in code if ord(ch) >= 0x80]
assert not bad, ('el bloque de PB-T4b no es ASCII', bad[:10])
assert code.count('\nINV-PBT4B-') == 14 and len(re.findall(r'(?m)^PBT4B-C\d\d:', code)) == 18 and len(re.findall(r'(?m)^\[\d+\] ', code)) == 16

# ---------------------------------------------------------------- RR-3: las filas de PB-T4b (§6.0)
cmap = [l.split('\t') for l in open(os.path.join(HERE, 'pbt4b-copy-map-v2.tsv'), encoding='utf-8').read().split('\n')[1:] if l]
hand = [(c[0], c[1], c[4]) for c in cmap if c[4] != 'mecanica']
assert len(hand) == 60
lst = [l.split('\t') for l in open(os.path.join(HERE, 'pbt4b-substitution-list-475.tsv'), encoding='utf-8').read().split('\n')[1:] if l]
guards = [(os.path.basename(r[1])[:-3], r[2]) for r in lst if r[0] == 'PB-T4b C-5' and not r[1].startswith('(PB-T4a)')]
assert len(guards) == 19
rr = [
    '| PB-T4b | `PushBackFullPanelTests` (CT-01, 02, 03, 05 a 11 y 17) | **Líneas:** las de esas CT en §5.2, literales; los fragmentos ` estado=Presented`, ` huellas-iguales=True`, `borrado EraseUnit:B2`, `presentar Cambiado fuera: el nombre.`, `presentar Cambiado por ti: las anotaciones.`, las once `of…` y `No editable aquí: la lectura no trae el modelo completo.` **Valores:** `2.25`, `2.5` y `Otro nombre` |',
    '| PB-T4b | `PushBackFullPanelGuardTests` (CT-12, 15 y 16) | **Líneas:** las de esas CT en §5.2; los seis nombres de CT-12, `PushBackPilotUpdateRun.ExecuteFull(`, `PushBackPilotUpdateRun.Precheck(`, `kind == ChoiceKind.KeepMine ? keepWitness : null`, `PushBackFullFieldAdapter` y `PushBackAnnotationScaleRule.Validate` |',
    '| PB-T4b | `PushBackFullPanelCensusGuardTests` (CT-13) | **Líneas:** `sin-conectar archivos-revisados>0=True coincidencias=[]`, los cinco nombres (`PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` y `PushBackViewDispositions`) y los dos destinos (`WorkspacePilotSupport.cs` y `WorkspaceEditIntent.cs`) |',
    '| PB-T4b | `PushBackFullPanelFieldTests` (UI; CT-04, 14 y 18) | **Líneas:** las de esas CT en §5.2; `BrowserFieldKeepMinePresentationText` y `Borrador conservado sobre la versión actual del dibujo.` **Valores:** los diez textos de CT-PBT2B-10 y los once casos de CT-18 |',
]
for cls, meth, why in hand:
    rr.append('| PB-T4b | `%sFull`, `%s` (a mano, %s) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |'
              % (cls[:-len('Tests')], meth, why))
for cls, meth in guards:
    rr.append('| PB-T4b | `%sFull`, `%s` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |'
              % (cls[:-len('Tests')], meth))
rr.append('| PB-T4b | Las 368 copias mecánicas, las 27 de la interfaz y el kit | **El texto entero, el del guion o el custodiado** (`CopySha` y el sha256 del kit): no admiten reparación (RR-2) |')
RR3 = '\n'.join(rr) + '\n'

# ---------------------------------------------------------------- el armado
v = t
head_start = v.index('# I-64 F4B — Paquete del contrato de PB-T4')
head_end = v.index('## Registro de cambios v4.2 → v4.3')
v = v[:head_start] + frag('f-head.md') + v[head_end:]


def replace_section(text, head, until, new_text):
    s = section(text, head, until)
    assert text.count(s) == 1, head
    return text.replace(s, new_text)


v = replace_section(v, '#### D-PBT4-04.', '#### D-PBT4-05.', frag('f-d04.md'))
v = replace_section(v, '#### D-PBT4-08.', '#### D-PBT4-09.', frag('f-d08.md'))
v = replace_section(v, '#### D-PBT4-11.', '### PB-T4c', frag('f-d11.md'))
v = replace_section(v, '### 4.2 PB-T4b', '### 4.3 PB-T4c', frag('f-42.md'))
v = replace_section(v, '### 5.2 PB-T4b', '### 5.3 PB-T4c', frag('f-52.md'))
v = replace_section(v, '### 6.2 PB-T4b', '### 6.3 PB-T4c', f62)
v = once(v, '\n## 9. Límites\n', '\n' + frag('f-85.md') + '## 9. Límites\n', '8.5')

# §0 y §1
v = once(v, "| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala, en Application; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con la presentación «Por grupos» y `KeepOn`. **El código del piloto queda sin uso, sin retirarlo** | **470** (428 de las 9 clases, «Saltar y duplicar»; 15 de la letra; 27 de la UI), la lista que el Owner tomó como «la que vio» | Condicional, con esa lista |",
         "| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala, en Application; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con dos «Sí», la presentación «Por grupos» y `KeepOn` (D-S2). **El código del piloto queda sin uso, sin retirarlo** | **475** (428 de las 9 clases, «Saltar y duplicar», 368 copias y 60 a mano; 20 de la letra; 27 de la UI), la lista que el Owner tomó como la que vio, con las cinco de la tercera tanda | Condicional, con esa lista |", '§0 T4b')
v = once(v, '| Retira el código del piloto que el panel ya no usa | **139** sustituciones, **55** originales y **29** miembros de apoyo borrados con registro (esbozo, §6.4) |',
         '| Retira el código del piloto que el panel ya no usa | **138** sustituciones (la v4.3, 139), **55** originales y **29** miembros de apoyo borrados con registro (esbozo, §6.4) |', '§0 T4d')
v = once(v, '; y, después, «Retirar tras PB-T4c» y «Sí, esa lista» (`owner-pbt4b-retire-list-decisions.md`).',
         '; después, «Retirar tras PB-T4c» y «Sí, esa lista» (`owner-pbt4b-retire-list-decisions.md`); y, en la tercera tanda, «Añadirlas a la lista» (`owner-pbt4b-breaks-decision.md`). La sesión decidió D-S1 a D-S3 (`session-pbt4b-design-decisions.md`; §8.5).', '§0 decisiones')
v = once(v, '| **Las decisiones del Owner sobre la retirada y la lista de PB-T4b** (`owner-pbt4b-retire-list-decisions.md`, 2026-11-20) | «Retirar tras PB-T4c» y «Sí, esa lista» (§8.2) |\n',
         '| **Las decisiones del Owner sobre la retirada y la lista de PB-T4b** (`owner-pbt4b-retire-list-decisions.md`, 2026-11-20) | «Retirar tras PB-T4c» y «Sí, esa lista» (§8.2) |\n'
         '| **La decisión del Owner sobre las cinco roturas** (`owner-pbt4b-breaks-decision.md`, 2026-11-20) | «Añadirlas a la lista»: 475 en PB-T4b, 138 en PB-T4d y WPUG en 85 (§8.5) |\n'
         '| **Las decisiones de diseño de la sesión** (`session-pbt4b-design-decisions.md`, 2026-11-20) y la §17 de `independent-review-v1.md` | D-S1 a D-S3 (§8.5); la verificación de las cinco roturas |\n', '§1')

# D-PBT4-06, D-PBT4-09 y D-PBT4-10
v = once(v, '  - `public static NumericTextCheck Check(string text, bool isOptional, bool integerOnly, double? minimum, double? maximum, bool minimumInclusive, bool maximumInclusive)`;\n',
         '  - `public static NumericTextCheck Check(string text, bool isOptional, bool integerOnly, double? minimum, double? maximum, bool minimumInclusive, bool maximumInclusive)`;\n'
         '  - `NumericTextCheck`, un `readonly struct` con constructor interno y `Status` (`NumericTextStatus`: los seis estados de `NumericFieldStatus`, en el mismo orden), `Value` (`double?`), `Message` e `IsValid`; la UI traduce el estado por su posición (V44-09; el texto literal, en `green_pbt4b.py`);\n', 'D-06')
v = once(v, '`PushBackKeepMinePresentation.Of(PushBackConflictClassification c)` es pura, en `Draft/PushBackKeepMinePresentation.cs`. Devuelve `Lines` (`IReadOnlyList<string>`) e `IsEmpty`. `PushBackKeepMinePresentation.Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)` clasifica (D-PBT4-08) y llama a `Of`.',
         '`PushBackKeepMinePresentation.Of(PushBackConflictClassification c)` es pura, en `Draft/PushBackKeepMinePresentation.cs`. Devuelve `Lines` (`IReadOnlyList<string>`) e `IsEmpty`. `PushBackKeepMinePresentation.Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)` clasifica (D-PBT4-08) y llama a `Of`; con una relectura admitida guarda además `OriginText`, el texto del sobre del origen de la relectura, que es el del testigo, y con una que no se admite no clasifica y devuelve su `Diagnostic` (`IsAdmitted` falso), para que WHC no nombre la lectura fuera de sus consumidores (D-S3, V44-06).', 'D-09 Of')
v = once(v, '**Las líneas de CT-PBT4B-06** se miden antes de emitir PB-T4b con el clasificador real (PB-T3a, ya cerrado), en la corrida de medición de la precondición 6, porque cambiar los frentes cambia también hojas de otros grupos (P3:713). El contrato de PB-T4b las lleva literales.',
         '**Las líneas de CT-PBT4B-06** (§5.2): las de la función pura, literales, sobre una clasificación fabricada; y las del anfitrión, con un cambio que no toca los frentes (el nombre, fuera; la escala, en el borrador), deducidas del clasificador real, porque cambiar los frentes cambia también hojas de otros grupos (P3:713). Se confirman en la corrida de medición de la precondición 6; el contrato de PB-T4b las lleva literales.', 'D-09 CT-06')
v = once(v, '- **sustituye con registro 139 pruebas** (causa C-7): 72 que nombran lo retirado por texto,',
         '- **sustituye con registro 138 pruebas** (causa C-7; la v4.3, 139, antes de que WPUG:822-840 pasara a PB-T4b, V44-01): 71 que nombran lo retirado por texto,', 'D-10 138')
v = once(v, '- **la lista,** en `pbt4d-retire-list.tsv` (§8.3), es un esbozo medido en `7f0d9aa3`:',
         '- **la lista,** en `pbt4d-retire-list-138.tsv` (§8.3; la de la v4.3, `pbt4d-retire-list.tsv`, queda como historia), es un esbozo medido en `7f0d9aa3`:', 'D-10 lista')

# §5.0, §6.0, §6.3 y §6.4
v = once(v, '  - PB-T4b: el kit nuevo de «Saltar y duplicar» (`WorkspaceFullReadKit.cs`, D-PBT4-11) y el doble del puerto del panel;',
         '  - PB-T4b: el kit nuevo de «Saltar y duplicar» (`WorkspaceFullReadKit.cs`, D-PBT4-11, con `FullHandKit`); las CT de UI usan `WorkspacePilotEditPortDouble`, sin editarlo;', '5.0')
v = once(v, '- **PB-T4b:** WHC, WEI, `WorkspacePilotSupport.cs`, FVM, PV, WCo y `src/RackCad.UI/Controls/NumericFieldValidation.cs`. **Ningún archivo de `Pilot/`:** la retirada es de PB-T4d;',
         '- **PB-T4b:** WHC, WEI, FVM, PV y `src/RackCad.UI/Controls/NumericFieldValidation.cs` (V44-09: el GREEN literal no toca `WorkspacePilotSupport.cs` ni WCo). **Ningún archivo de `Pilot/`:** la retirada es de PB-T4d;', '6.0 UC')
v = once(v, '**En PB-T4b,** lo mismo para las 9 clases con sus `Skip`.',
         '**En PB-T4b,** lo mismo para los 19 archivos con sus `Skip`; y una reparación tampoco toca el kit ni las 18 copias, cuyo texto es el del paquete.', '6.0 RR-2')
v = once(v, 'en PB-T4b, ningún archivo de `src/` ni las 9 clases;', 'en PB-T4b, ningún archivo de `src/`, ni los 19 archivos de los `Skip`, ni el kit, ni las copias;', '6.0 RR')
old_rr = [l for l in v.split('\n') if l.startswith('| PB-T4b | `PushBackPanelDraftGuardTests` (CT-19)') or l.startswith('| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20)')]
assert len(old_rr) == 2
v = once(v, old_rr[1] + '\n',
         '| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20) | **Líneas:** ` igual-a-la-base=True`, ` igual-al-mapa=True`, ` sha256=True`, los 18 nombres de clase de origen, las ocho parejas de M-01 a M-08, los `BaseSha` de los 19 archivos de los `Skip` y los `CopySha` de las 18 copias y del kit |\n' + RR3, '6.0 RR-3')
v = once(v, 'Las filas de las demás clases de PB-T4b y PB-T4c, con sus líneas medidas, van en sus contratos.',
         'Las filas de PB-T4c, con sus líneas medidas, van en su contrato.', '6.0 RR-3 cola')
v = once(v, 'segun el paquete del contrato de PB-T4 v4.3 y la lista cerrada CT-PBT4C-01..15', 'segun el paquete del contrato de PB-T4 v4.4 y la lista cerrada CT-PBT4C-01..15', '6.3')
v = once(v, '  - **139 sustituciones con registro** (C-7): 72 por texto, con E-3, E-5 y E-6 de D-1, y 67 que dejan de compilar (R-36).',
         '  - **138 sustituciones con registro** (C-7): 71 por texto, con E-3, E-5 y E-6 de D-1, y 67 que dejan de compilar (R-36); WPUG:822-840 pasó a PB-T4b (V44-01).', '6.4 138')
v = once(v, '- **Su lista, medida en `7f0d9aa3`** (`pbt4d-retire-list.tsv`, sha256 `1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71`, 55 225 bytes; la genera `split_lists_pbt4.py`):',
         '- **Su lista, medida en `7f0d9aa3`** (`pbt4d-retire-list-138.tsv`, sha256 `d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`, 54 952 bytes, la de la v4.3 sin WPUG:822-840; la genera `make_lists_475.py` desde `pbt4d-retire-list.tsv`, `1c9d67e8…8b71`, que hizo `split_lists_pbt4.py`):', '6.4 lista')

# §8.3
v = once(v, '| PB-T4b | Las 9 clases (C-0), «Saltar y duplicar»: 375 copias mecánicas y 53 equivalentes a mano | 428 | **428** | |',
         '| PB-T4b | Las 9 clases (C-0), «Saltar y duplicar»: **368 copias mecánicas y 60 equivalentes a mano** (D-S1; la v4.3, 375 y 53; `pbt4b-copy-map-v2.tsv`) | 428 | **428** | |', '8.3 C-0')
v = once(v, '| PB-T4b | La letra de las guardas (C-5 y C-6) | «unas 12» | **15** | |',
         '| PB-T4b | La letra de las guardas (C-5 y C-6) | «unas 12»; en la tercera tanda, **20** | **20** (15 y las cinco nuevas) | |', '8.3 C-5')
v = once(v, '| PB-T4b | **Total** | **unas 465**; en la segunda vuelta, **470** | **470** | **Aprobada como «la que vio»** (segunda vuelta, pregunta 2); `pbt4b-substitution-list.tsv`, sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4` |',
         '| PB-T4b | **Total** | **unas 465**; en la segunda vuelta, **470**; en la tercera, **475** | **475** | **Aprobada como la que vio** (segunda vuelta, pregunta 2, y tercera tanda); `pbt4b-substitution-list-475.tsv`, sha256 `654c3b7b045c66c3d7bf0f1c3c1eac42721182aa85ee4c006de444314fd0707c` (la de 470, `44393b19…63e4`, queda como historia) |', '8.3 total')
v = once(v, '| PB-T4d | C-7: 72 por texto (con E-3, E-5 y E-6) y 67 por compilación | «unas 100»; en la segunda vuelta, 139 | **139** | **Vuelve al Owner con su lista** (esbozo en `pbt4d-retire-list.tsv`, sha256 `1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71`) |',
         '| PB-T4d | C-7: 71 por texto (con E-3, E-5 y E-6) y 67 por compilación | «unas 100»; en la segunda vuelta, 139; en la tercera, 138 | **138** | **Vuelve al Owner con su lista** (esbozo en `pbt4d-retire-list-138.tsv`, sha256 `d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`) |', '8.3 T4d')
v = once(v, '**Las 470 no dependen de la retirada** (M-33): las 9 de la letra que nombran lo que se retirará caen por el cambio del anfitrión (§4.2).',
         '**Las 475 no dependen de la retirada** (M-33): las de la letra que nombran lo que se retirará caen por el cambio del anfitrión (§4.2), también WPUG:822-840, que la v4.3 ponía en PB-T4d.', '8.3 M-33')

# §11
v = once(v, '2. **Las respuestas del Owner:** cumplidas (`owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`), con su número F4B-E al custodiarlas.',
         '2. **Las respuestas del Owner:** cumplidas (`owner-pbt4-decisions.md`, `owner-pbt4b-retire-list-decisions.md` y, para PB-T4b, `owner-pbt4b-breaks-decision.md`), con su número F4B-E al custodiarlas; y la decisión de la sesión, `session-pbt4b-design-decisions.md`, declarada en F4B-E52.', '11.2')
v = once(v, '   - en PB-T4b y PB-T4c, además: K, D y E, la partición del RED de las copias (PB-T4b), las líneas de CT-PBT4B-06, el coste de `IsDirty` y la duración de la suite.',
         '   - **PB-T4b** (V44-09; el procedimiento de §6.2.1): sobre una copia exportada del cierre de PB-T4a, `apply_red_pbt4b.py` en modo de medida y `partition_pbt4b.py red`: la partición, su sha256 y los resultados de cada clase (R_C y R_UI); sobre otra, el RED con la partición, `apply_green_pbt4b.py`, la suite completa de Core y los filtros de UI de la subtarea (Core **19 601 = 19 146 + 0 + 455** y UI **2 481 = 2 433 + 0 + 48** con las CT; sin ellas, 17 y 3 menos) y `partition_pbt4b.py green`: todas las copias pasan; cero `warning CS`; las líneas de CT-PBT4B-06 con el clasificador real; `check_skips_pbt4b.py`; el coste de `IsDirty` y la duración de la suite. **Una copia que no pase** pasa a equivalente escrito a mano (D-S1) y la emisión necesita una versión del paquete con su texto; **una prueba que falle fuera de la lista de 475** vuelve al Owner;\n'
         '   - en PB-T4c, además: K, D y E, el coste de `IsDirty` y la duración de la suite.', '11.6')
v = once(v, '**PB-T4b, las 470 de `pbt4b-substitution-list.tsv`** (sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`), que el generador comprueba;',
         '**PB-T4b, las 475 de `pbt4b-substitution-list-475.tsv`** (sha256 `654c3b7b045c66c3d7bf0f1c3c1eac42721182aa85ee4c006de444314fd0707c`), que el generador comprueba;', '11.7')
v = once(v, '9. **Para PB-T4b y PB-T4c:** la anterior CERRADA y su remedición.',
         '9. **Para PB-T4b y PB-T4c:** la anterior CERRADA y su remedición. **En el commit de emisión de PB-T4b,** además: esta versión (v4.4), sus fragmentos de `v44/`, las listas nuevas (`pbt4b-substitution-list-475.tsv` y `pbt4d-retire-list-138.tsv`), el mapa de la copia v2, el kit, los guiones de la v4.4 y sus salidas, la partición y las 18 copias generadas con ella (por sha256), `pbt4b-equivalents-review.txt`, el registro del Owner de la tercera tanda y la decisión de la sesión, y la entrada «## PB-T4b» de `superseding-characterization.md`.', '11.9')

# las demás menciones vigentes de la lista de PB-T4b (las de los registros de cambios y de §8.2 son historia y no cambian)
v = once(v, 'La de PB-T4b va en un archivo custodiado que el contrato cita por su sha256 (`pbt4b-substitution-list.tsv`, `44393b19…63e4`; D-PBT4-11),',
         'La de PB-T4b va en un archivo custodiado que el contrato cita por su sha256 (`pbt4b-substitution-list-475.tsv`, `654c3b7b…0707c`; la v4.3, `pbt4b-substitution-list.tsv`, `44393b19…63e4`; D-PBT4-11),', 'tamano')
v = once(v, '| R-T4-01, la lista | Las listas, guardadas método a método, con su causa y su destino; la de PB-T4b, fijada por sha256 y aprobada por el Owner (470) |',
         '| R-T4-01, la lista | Las listas, guardadas método a método, con su causa y su destino; la de PB-T4b, fijada por sha256 y aprobada por el Owner (470; en la tercera tanda, 475) |', 'R-T4-01')
v = once(v, '- Nada se compiló ni se ejecutó, y el worktree no se leyó: todo es por objetos git en `7f0d9aa3`.\n',
         '- Nada se compiló ni se ejecutó, y el worktree no se leyó: todo es por objetos git en `7f0d9aa3` y, para la v4.4, en `5aaacce3` (cuyo `src/` y `tests/` son los de `7f0d9aa3`) con el GREEN literal de PB-T4a aplicado en memoria (`green_pbt4a.green`).\n', '9 objetos')
v = once(v, 'La de PB-T4b es la que aprobó el Owner (470); la de PB-T4d es un esbozo que se vuelve a medir.',
         'La de PB-T4b es la que aprobó el Owner (470, y 475 en la tercera tanda); la de PB-T4d (138) es un esbozo que se vuelve a medir.', '9 listas')
v = once(v, '- **Quedan para la copia exportada:** K, D y E de PB-T4b y PB-T4c, la partición de las copias en el RED, la letra de C-5 sobre el GREEN de PB-T4b, las líneas de CT-PBT4B-06, el coste de `IsDirty` y la duración de la suite de Core.\n',
         '- **Quedan para la copia exportada:** K, D y E de PB-T4c; en PB-T4b, la partición de las copias en el RED (R_C y R_UI), que las copias pasen en el GREEN, las lecturas de las guardas que solo se ven al ejecutar (el recorrido por diferencia de la v4.4, §4.2, no las ve), la confirmación de las líneas de CT-PBT4B-06, el coste de `IsDirty` y la duración de la suite de Core.\n', '9 copia')

for k_ in KEEP:
    assert v.count(k_) == 1, 'PB-T4a cambió'
assert '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.4' in v
open(OUT, 'wb').write(v.encode('utf-8'))
print('v4.4', hashlib.sha256(v.encode('utf-8')).hexdigest(), len(v.encode('utf-8')), 'bytes')
print('kit', KIT_SHA, '| generador', GEN_SHA, '| ediciones', EDITS_SHA, '| revision', REVIEW_SHA)
print('filas RR-3 de PB-T4b', sum(1 for l in v.split('\n') if l.startswith('| PB-T4b |')), '| RequiredTests', len(out_rows))
