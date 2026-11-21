"""Arma la v4.5 del paquete de PB-T4 desde la v4.4 y los fragmentos de v45/ (solo lee archivos de hs65/pbt4 y escribe la v4.5).
Uso: python -I make_v45.py"""
import hashlib
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
V44 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v44.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v45.md')
raw = open(V44, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '517673faddd9d12d282404722cc0e2c08b0cf471473ed29619260ed25cf81498'
t = raw.decode('utf-8')
assert '\r' not in t


def frag(name):
    return open(os.path.join(HERE, 'v45', name), encoding='utf-8').read()


def sha(name):
    data = open(os.path.join(HERE, name), 'rb').read()
    assert b'\r' not in data, name
    return hashlib.sha256(data).hexdigest()


def once(text, old, new, what):
    n = text.count(old)
    assert n == 1, (what, n)
    return text.replace(old, new)


def line(text, prefix, new, what):
    """Sustituye la única línea que empieza por prefix."""
    lines = text.split('\n')
    hits = [i for i, l in enumerate(lines) if l.startswith(prefix)]
    assert len(hits) == 1, (what, len(hits))
    lines[hits[0]] = new
    return '\n'.join(lines)


def section(text, head, until):
    i = text.index('\n' + head) + 1
    j = text.index('\n' + until, i) + 1
    return text[i:j]


def replace_section(text, head, until, new_text):
    s = section(text, head, until)
    assert text.count(s) == 1, head
    return text.replace(s, new_text)


# lo que no puede cambiar: PB-T4a (§5.1, §6.1 con 6.1.1 y 6.1.2)
KEEP = [section(t, '### 5.1 PB-T4a', '### 5.2 PB-T4b'), section(t, '### 6.1 PB-T4a', '### 6.2 PB-T4b')]

# ---------------------------------------------------------------- §6.2: cabecera nueva, el bloque de la v4.4 con sus cambios, cola nueva
s62 = section(t, '### 6.2 PB-T4b', '### 6.3 PB-T4c')
b0 = s62.index('```text\nTaskId: F4B-PBT4-FULL-DRAFT-PANEL\n')
b1 = s62.index('\n```\n', b0) + len('\n```\n')
block = s62[b0:b1]
block = once(block, 'el segundo conserva con KeepOn solo si ese testigo sigue valiendo. El codigo del piloto',
             'el segundo conserva con KeepOn solo si ese testigo sigue valiendo; y una sola materializacion del modelo completo por lectura (el adaptador del '
             'Plugin difiere la del Primary, y la relectura con el origen encontrado no la hace). El codigo del piloto', 'OBJ 1')
block = once(block, 'segun el paquete del contrato de PB-T4 v4.4, las decisiones del Owner sobre PB-T4 (las tres tandas), la lista cerrada CT-PBT4B-01..20 y la lista exacta de 475 sustituciones',
             'segun el paquete del contrato de PB-T4 v4.5, las decisiones del Owner sobre PB-T4 (sus cinco tandas, con la del coste de la lectura de PB-T4a), la lista cerrada CT-PBT4B-01..21 y la lista exacta de 477 sustituciones', 'OBJ 2')
block = line(block, 'INV-PBT4B-01. ',
             'INV-PBT4B-01. Alcance: dentro de las 57 rutas. git diff BaseSha..CurrentSha -- src/RackCad.Domain vacio; en src/RackCad.Plugin solo '
             'Views/RackPilotIntentAdapter.cs; en src/RackCad.Application solo WHC, WEI, Formatting/NumericTextRule.cs y los tres archivos nuevos de '
             'Systems/PushBack/Draft/; en src/RackCad.UI solo FVM, Workspace/WorkspacePanelView.xaml y Controls/NumericFieldValidation.cs; ningun archivo de '
             'Pilot/; en tests/ solo las seis clases de las CT, el kit, las 20 copias y las lineas de atributo con Skip de los 20 archivos de la lista.', 'INV-01')
block = once(block, 'el kit y las 18 copias byte a byte', 'el kit y las 20 copias byte a byte', 'INV-02 a')
block = once(block, 'los 475 Skip y el esqueleto', 'los 477 Skip y el esqueleto', 'INV-02 b')
block = once(block, 'WHC, PV y NumericFieldValidation.cs sin cambio', 'WHC, RIA, PV y NumericFieldValidation.cs sin cambio', 'INV-02 c')
block = once(block, 'ni el kit, ni las copias, ni los 19 archivos de los Skip', 'ni el kit, ni las copias, ni los 20 archivos de los Skip', 'INV-02 d')
block = once(block, 'exactamente la de D-PBT4-04 a D-PBT4-09, con el texto literal del GREEN del paquete (green_pbt4b.py)',
             'exactamente la de D-PBT4-04 a D-PBT4-09 y D-PBT4-21, con el texto literal del GREEN del paquete (green_pbt4b_v45.py)', 'INV-03')
block = once(block, 'nada traduce un estado del piloto a uno completo.',
             'nada traduce un estado del piloto a uno completo. Una sola materializacion del modelo completo por lectura: Read de RIA difiere la del Primary '
             '(System.Lazy, y Full es _full?.Value) y AtOrigin no la pide; NH, AtOrigin y ReadAt no cambian (CT-PBT4B-21).', 'INV-04')
block = once(block, 'salvo las 475 de la lista cerrada (pbt4b-substitution-list-475.tsv, sha256 654c3b7b045c66c3d7bf0f1c3c1eac42721182aa85ee4c006de444314fd0707c)',
             'salvo las 477 de la lista cerrada (pbt4b-substitution-list-477.tsv, sha256 5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651)', 'INV-07 a')
block = once(block, 'de los 19 archivos de pruebas existentes', 'de los 20 archivos de pruebas existentes', 'INV-07 b')
block = once(block, 'the Skip of each of the 475 tests of pbt4b-substitution-list-475.tsv, with its literal reason, in its attribute line, and nothing else in those 19 files.',
             'the Skip of each of the 477 tests of pbt4b-substitution-list-477.tsv, with its literal reason, in its attribute line, and nothing else in those 20 files.', 'C02')
block = once(block, '(the self-tests of CT-PBT4B-12, CT-PBT4B-13, CT-PBT4B-15, CT-PBT4B-19 and CT-PBT4B-20)',
             '(the self-tests of CT-PBT4B-12, CT-PBT4B-13, CT-PBT4B-15, CT-PBT4B-19, CT-PBT4B-20 and CT-PBT4B-21)', 'C07')
block = once(block, '; a new member of the edit port of the panel; or translating a state of the pilot into a full one.',
             '; a new member of the edit port of the panel; translating a state of the pilot into a full one; or, for the one materialization per read (D-S4), '
             'touching NH, AtOrigin or ReadAt, or reading the drawing again.', 'C10')
block = once(block, 'touching a file of src/ other than WHC, WEI, FVM, PV, NumericFieldValidation.cs, Formatting/NumericTextRule.cs and the three new files of Draft/;',
             'touching a file of src/ other than WHC, WEI, FVM, PV, NumericFieldValidation.cs, RIA (src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs), '
             'Formatting/NumericTextRule.cs and the three new files of Draft/;', 'C13 a')
block = once(block, 'in WorkspacePanelView.xaml, the TextBlock BrowserFieldKeepMinePresentationText;',
             'in WorkspacePanelView.xaml, the TextBlock BrowserFieldKeepMinePresentationText; in RackPilotIntentAdapter.cs, in RackPilotIntentReadResult, a second '
             'internal constructor (PushBackPilotIntentResult pilot, System.Lazy<PushBackFullIntentResult> full, PushBackPilotIntentScan input) and the private '
             'readonly field _full;', 'C13 b')
block = once(block, 'the existing private constructor of EditIntentOutcome assigns the two new members;',
             'the existing private constructor of EditIntentOutcome assigns the two new members; the existing constructor (pilot, full, input) of '
             'RackPilotIntentReadResult assigns _full = new System.Lazy<PushBackFullIntentResult>(full), its property Full becomes => _full?.Value, and the '
             'success line of Read passes new System.Lazy<PushBackFullIntentResult>(() => PushBackFullDraftReader.Read(input, catalog));', 'C13 c')
block = once(block, '(the 475 of pbt4b-substitution-list-475.tsv)', '(the 477 of pbt4b-substitution-list-477.tsv)', 'C16')
block = once(block, 'y las autopruebas de CT-PBT4B-12, 13, 15, 19 y 20;', 'y las autopruebas de CT-PBT4B-12, 13, 15, 19, 20 y 21;', 'EE3')
block = once(block, 'el sha256 del kit y de cada una de las 18 copias', 'el sha256 del kit y de cada una de las 20 copias', 'EE6')
block = once(block, 'ni el kit, ni las 18 copias, ni los 19 archivos de los Skip.', 'ni el kit, ni las 20 copias, ni los 20 archivos de los Skip.', 'EE8')
block = once(block, 'el censo de declaraciones de WHC, WEI, FVM, PV y NumericFieldValidation.cs, y de los cuatro archivos nuevos',
             'el censo de declaraciones de WHC, WEI, FVM, PV, NumericFieldValidation.cs y RIA, y de los cuatro archivos nuevos', 'EE11')
block = line(block, '[13] ',
             '[13] la entrada "## PB-T4b" de superseding-characterization.md, escrita por la sesion en el commit de emision: la tabla por clase y por causa de las '
             '477 (C-0 con sus 9 clases, 368 copias y 60 a mano; C-5 con sus 22, las 2 de PB-T4a entre ellas; UI con sus 27), el sha256 de la lista, los motivos '
             'literales de los Skip, la sustituta de cada una y las 28 copias sin cambio (las 27 de la UI y RG:109-123), declaradas como tales; su tabla de '
             'ejecucion se completa al cerrar, con los recuentos por clase;', 'EE13')
block = line(block, '[14] ',
             '[14] Linea base de la CI: la del cierre de PB-T4a (F4B-E51, 78a7cef3): run 44814418683 sobre su GREEN faef7020, Core 19 137 resultados, con 19 130 '
             'Passed y 7 Skipped, y UI 2 451, con 2 430 Passed y 21 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4b pone 477 Skip (450 en '
             'Core y 27 en UI), cada uno sobre una prueba que pasa a un solo resultado omitido, anade 449 copias o equivalentes en Core y 27 en UI, con las mismas '
             'filas que sus originales, y 21 CT (18 de Core y 3 de UI), sin ninguna Theory nueva. El GREEN: Core 19 604 resultados, con 19 147 Passed, 0 Failed y '
             '457 Skipped; UI 2 481, con 2 433 Passed, 0 Failed y 48 Skipped. El RED: los mismos totales y omitidas, con 15 + R_C Failed en Core y 3 + R_UI en '
             'UI, donde R_C y R_UI son los resultados de las clases FullRedTests de la particion de Core y de UI: <medido en la precondicion 6>.', 'EE14')
block = line(block, '[15] ',
             '[15] la comprobacion mecanica de los Skip, check_skips_pbt4b_v45.py, sobre RedSha y sobre CurrentSha frente a BaseSha: los 20 archivos, iguales a su '
             'base con su Skip literal y nada mas (477 Skip).', 'EE15')
code = block[len('```text\n'):-len('\n```\n')]
assert all(ord(ch) < 0x80 for ch in code), [ch for ch in code if ord(ch) >= 0x80][:10]
assert code.count('\nINV-PBT4B-') == 14 and len(re.findall(r'(?m)^PBT4B-C\d\d:', code)) == 18 and len(re.findall(r'(?m)^\[\d+\] ', code)) == 16

# las filas de RequiredTests: las de la v4.4, con los n.º 1, 2 y 40 y cuatro filas nuevas de las copias de PB-T4a
req = [l for l in s62.split('\n') if re.match(r'^\| \d+ \| `tests/', l)]
assert len(req) == 82
CORE = '`tests/RackCad.Tests/RackCad.Tests.csproj`'
out_rows = []
for l in req:
    n = int(l.split('|')[1])
    if n == 1:
        assert l.endswith('| 5 | false |'), l
        l = l.replace('| 5 | false |', '| **4** | false |')
    elif n == 2:
        assert l.endswith('| **2** | false |'), l
        l = l.replace('| **2** | false |', '| **1** | false |')
    elif n == 40:
        assert 'PushBackFullPanelGuardTests' in l and l.endswith('| 3 | true |'), l
        l = l.replace('| 3 | true |', '| **4** | true |')
    out_rows.append(l)
k = 83
for stem in ('PushBackFullReadPath', 'PushBackFullReadGuard'):
    for part, e in (('Red', 'true'), ('Green', 'false')):
        out_rows.append('| %d | %s | `FullyQualifiedName~RackCad.Tests.FullPanelCopies.%sFull%sTests` | `<medido en la precondicion 6>` | %s |' % (k, CORE, stem, part, e))
        k += 1
REQ_ROWS = '\n'.join(out_rows)

# los BaseSha medidos
st = open(os.path.join(HERE, 'v45', 'selftest_pbt4b_v45-out.txt'), encoding='utf-8').read()
base_rows = re.findall(r'^   ([0-9a-f]{64})  (tests/\S+)  skip=(\d+)$', st, re.M)
assert len(base_rows) == 20 and sum(int(n) for _, _, n in base_rows) == 477
BASESHA_ROWS = '\n'.join('| `%s` | `%s` | %s |' % (p, h, n) for h, p, n in base_rows)
probe = [l.split()[1] for l in open(os.path.join(HERE, 'v45', 'copies_probe_v45-out.txt'), encoding='utf-8').read().split('\n') if l.startswith('conjunto ')]
assert len(probe) == 1
tail = frag('f45-62-tail.md')
for key, val in (('{BASESHA_ROWS}', BASESHA_ROWS), ('{KIT_SHA}', sha('pbt4b-kit-WorkspaceFullReadKit-v45.cs')), ('{GEN_SHA}', sha('make_copies_pbt4b_v45.py')),
                 ('{GEN_BASE_SHA}', sha('make_copies_pbt4b.py')), ('{EDITS_SHA}', sha('pbt4b_hand_edits_v45.py')), ('{EDITS_BASE_SHA}', sha('pbt4b_hand_edits.py')),
                 ('{PROBE_SET}', probe[0]), ('{REVIEW_SHA}', sha('pbt4b-equivalents-review-v45.txt')), ('{REQ_ROWS}', REQ_ROWS)):
    tail = once(tail, key, val, key)
assert '{' + 'KIT' not in tail and not re.search(r'\{[A-Z_]{3,}\}', tail)
new62 = frag('f45-62-head.md') + block + '\n' + tail

# ---------------------------------------------------------------- RR-3 (§6.0): las filas de PB-T4b que cambian
cmap = [l.split('\t') for l in open(os.path.join(HERE, 'pbt4b-substitution-list-477.tsv'), encoding='utf-8').read().split('\n')[1:] if l]
new_guard_rows = ''.join(
    '| PB-T4b | `%sFull`, `%s` (equivalente de una guarda, C-5; owner-pbt4b-list-477-decision.md) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n'
    % (os.path.basename(r[1])[:-len('Tests.cs')], r[2]) for r in cmap[-2:])

# ---------------------------------------------------------------- el armado
v = t
head_start = v.index('# I-64 F4B — Paquete del contrato de PB-T4')
head_end = v.index('## Registro de cambios v4.3 → v4.4')
v = v[:head_start] + frag('f45-head.md') + v[head_end:]
v = replace_section(v, '#### D-PBT4-11.', '### PB-T4c', frag('f45-d11.md') + frag('f45-d21.md'))
v = replace_section(v, '### 4.2 PB-T4b', '### 4.3 PB-T4c', frag('f45-42.md'))
v = replace_section(v, '### 5.2 PB-T4b', '### 5.3 PB-T4c', frag('f45-52.md'))
v = replace_section(v, '### 6.2 PB-T4b', '### 6.3 PB-T4c', new62)
v = replace_section(v, '### 8.5 ', '## 9. Límites', frag('f45-85.md'))

# §0 y §1
v = line(v, '| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) |',
         '| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala, en Application; «Actualizar» por '
         '`ExecuteFull`; «Conservar mi borrador» con dos «Sí», la presentación «Por grupos» y `KeepOn` (D-S2); una sola materialización del modelo completo '
         'por lectura (D-S4). **El código del piloto queda sin uso, sin retirarlo** | **477** (428 de las 9 clases, «Saltar y duplicar», 368 copias y 60 a '
         'mano; 22 de la letra; 27 de la UI), la lista que el Owner tomó como la que vio, con las cinco de la tercera tanda y las dos de la quinta | '
         'Condicional, con esa lista, sobre el cierre de PB-T4a (`78a7cef3`) |', '§0 T4b')
v = once(v, 'La sesión decidió D-S1 a D-S3 (`session-pbt4b-design-decisions.md`; §8.5).',
         'Después, «Cerrar y optimizar en PB-T4b» (`owner-pbt4a-cost-decision.md`) y, en la quinta tanda, «Añadirlas a la lista» (`owner-pbt4b-list-477-decision.md`). '
         'La sesión decidió D-S1 a D-S3 (`session-pbt4b-design-decisions.md`) y D-S4 (D-PBT4-21; §8.5).', '§0 decisiones')
v = line(v, '| **Las decisiones de diseño de la sesión** (`session-pbt4b-design-decisions.md`, 2026-11-20)',
         '| **La decisión del Owner sobre el coste de la lectura de PB-T4a** (`owner-pbt4a-cost-decision.md`, 2026-11-20; en `F4B-DRAFTS/coordinator/` del cierre `78a7cef3`) | '
         '«Cerrar y optimizar en PB-T4b»: una sola materialización del modelo completo por lectura (D-S4, D-PBT4-21) |\n'
         '| **La decisión del Owner sobre las dos de PB-T4a** (`owner-pbt4b-list-477-decision.md`, 2026-11-20) | «Añadirlas a la lista»: 477 en PB-T4b (§8.5) |\n'
         '| **Las decisiones de diseño de la sesión** (`session-pbt4b-design-decisions.md`, 2026-11-20) y las §17 y §18 de `independent-review-v1.md` | D-S1 a D-S3, '
         'y D-S4 (§8.5, D-PBT4-21); la verificación de las cinco roturas y la revisión de la v4.4 |', '§1')

# D-PBT4-03 (PB-T4a, el diseño de su relectura) y D-PBT4-09
v = once(v, '\n### PB-T4b\n\n#### D-PBT4-04.',
         '\n**En PB-T4b** (D-S4, D-PBT4-21), la lectura completa del `Primary` se difiere en RIA, y con el origen encontrado no se materializa: `AtOrigin`, NH, '
         '`ReadAt` y las cinco relecturas de WHC no cambian.\n\n### PB-T4b\n\n#### D-PBT4-04.', 'D-03')
v = once(v, '- `IsEmpty` es verdadero si no hay ninguna línea.\n',
         '- `IsEmpty` es verdadero si no hay ninguna línea.\n\n**Las vistas, en hasta tres líneas** (M-02 de la §18; V45-06): las líneas 5, 6 y 7 son independientes '
         '(se borran laterales, hay vistas que no se pueden conservar y todas quedarían obsoletas), así que las vistas ocupan de cero a tres líneas, y la '
         'presentación entera, hasta siete. La opción del Owner decía «más una línea de lo estructural y otra de las vistas»; **la sesión las deja en hasta '
         'tres, porque cada una dice una cosa distinta que el usuario decide por separado, y lo declara en F4B-E52.** Medido: CT-PBT4B-06 fija la '
         'clasificación fabricada con las tres (`of lineas=7`), y la sonda de la precondición 6 mide el caso del anfitrión (ninguna).\n', 'D-09 vistas')
v = once(v, 'Se confirman en la corrida de medición de la precondición 6; el contrato de PB-T4b las lleva literales.',
         'Se confirman en la precondición 6 con su sonda (`pbt4b-ct06-presentar-probe.cs`, §6.2.1 y §11); el contrato de PB-T4b las lleva literales.', 'D-09 sonda')

# §5.0 y §6.0
v = once(v, 'PB-T4b: el kit nuevo de «Saltar y duplicar» (`WorkspaceFullReadKit.cs`, D-PBT4-11, con `FullHandKit`);',
         'PB-T4b: el kit nuevo de «Saltar y duplicar» (`WorkspaceFullReadKit.cs`, D-PBT4-11, con `FullHandKit` y `FullReadRig`);', '5.0')
v = line(v, '- **PB-T4b:** WHC, WEI, FVM, PV y `src/RackCad.UI/Controls/NumericFieldValidation.cs`',
         '- **PB-T4b:** WHC, WEI, FVM, PV, `src/RackCad.UI/Controls/NumericFieldValidation.cs` y RIA (D-S4, V45-02; el GREEN literal no toca `WorkspacePilotSupport.cs`, '
         'WCo ni NH). **Ningún archivo de `Pilot/`:** la retirada es de PB-T4d;', '6.0 UC')
v = once(v, '**En PB-T4b,** lo mismo para los 19 archivos con sus `Skip`; y una reparación tampoco toca el kit ni las 18 copias, cuyo texto es el del paquete.',
         '**En PB-T4b,** lo mismo para los 20 archivos con sus `Skip`; y una reparación tampoco toca el kit ni las 20 copias, cuyo texto es el del paquete.', '6.0 RR-2')
v = once(v, 'en PB-T4b, ningún archivo de `src/`, ni los 19 archivos de los `Skip`, ni el kit, ni las copias;',
         'en PB-T4b, ningún archivo de `src/`, ni los 20 archivos de los `Skip`, ni el kit, ni las copias;', '6.0 RR')
v = line(v, '| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20) |',
         '| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20) | **Líneas:** ` igual-a-la-base=True`, ` igual-al-mapa=True`, ` sha256=True`, `kit FullOf-igual-al-lector=True '
         'escaneos=5`, los 20 nombres de clase de origen, las nueve parejas de M-00 a M-08 (con los seis tipos de M-00 de §5.2), los `BaseSha` de los 20 '
         'archivos de los `Skip` (§6.2.1) y los `CopySha` de las 20 copias y del kit; los cinco escaneos de M-05 |', 'RR-3 CT-20')
v = line(v, '| PB-T4b | `PushBackFullPanelGuardTests` (CT-12, 15 y 16) |',
         [l for l in v.split('\n') if l.startswith('| PB-T4b | `PushBackFullPanelGuardTests` (CT-12, 15 y 16) |')][0] + '\n'
         '| PB-T4b | `PushBackFullPanelGuardTests` (CT-21) | **Líneas:** las de CT-21 en §5.2 y sus expresiones literales; `System.Lazy<PushBackFullIntentResult>`, '
         '`_full?.Value`, `read.Full` y `var injected = read.Full;` |', 'RR-3 CT-21')
v = once(v, '| PB-T4b | Las 368 copias mecánicas, las 27 de la interfaz y el kit |', new_guard_rows + '| PB-T4b | Las 368 copias mecánicas, las 27 de la interfaz y el kit |', 'RR-3 PB-T4a')
v = once(v, 'segun el paquete del contrato de PB-T4 v4.4 y la lista cerrada CT-PBT4C-01..15', 'segun el paquete del contrato de PB-T4 v4.5 y la lista cerrada CT-PBT4C-01..15', '6.3')
v = line(v, '| R-T4-16, la doble lectura del piloto |',
         [l for l in v.split('\n') if l.startswith('| R-T4-16, la doble lectura del piloto |')][0][:-2]
         + '. En PB-T4b, D-S4 deja una sola materialización del modelo completo por lectura (D-PBT4-21), y la precondición 6 mide el coste con la sonda de `read-cost` |', 'R-T4-16')
v = line(v, '| R-T4-01, la lista |',
         '| R-T4-01, la lista | Las listas, guardadas método a método, con su causa y su destino; la de PB-T4b, fijada por sha256 y aprobada por el Owner (470; en la '
         'tercera tanda, 475; en la quinta, 477) |', 'R-T4-01')

# §8.3
v = line(v, '| PB-T4b | La letra de las guardas (C-5 y C-6) |',
         '| PB-T4b | La letra de las guardas (C-5 y C-6) | «unas 12»; en la tercera tanda, 20; en la quinta, **22** | **22** (15, las cinco de la tercera tanda y las '
         'dos de PB-T4a) | |', '8.3 C-5')
v = line(v, '| PB-T4b | **Total** |',
         '| PB-T4b | **Total** | **unas 465**; en la segunda vuelta, **470**; en la tercera, **475**; en la quinta, **477** | **477** | **Aprobada como la que vio** '
         '(segunda vuelta, pregunta 2, y tercera y quinta tandas); `pbt4b-substitution-list-477.tsv`, sha256 `5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651` '
         '(las de 475, `654c3b7b…0707c`, y 470, `44393b19…63e4`, quedan como historia) |', '8.3 total')
v = once(v, '**Las 475 no dependen de la retirada** (M-33):', '**Las 477 no dependen de la retirada** (M-33):', '8.3 M-33')

# §9
v = line(v, '- Nada se compiló ni se ejecutó, y el worktree no se leyó: todo es por objetos git en `7f0d9aa3`',
         '- Nada se compiló ni se ejecutó, y el worktree no se leyó: todo es por objetos git en `7f0d9aa3`, en `5aaacce3` (la v4.4, con el GREEN literal de PB-T4a '
         'en memoria) y, para la v4.5, en el cierre de PB-T4a, `78a7cef3` (su `src/` y sus `tests/` son los de `faef7020`, y su `src/` es el GREEN literal).', '9 objetos')
v = once(v, 'La de PB-T4b es la que aprobó el Owner (470, y 475 en la tercera tanda);', 'La de PB-T4b es la que aprobó el Owner (470, 475 en la tercera tanda y 477 en la quinta);', '9 listas')
v = line(v, '- **Quedan para la copia exportada:**',
         '- **Quedan para la copia exportada:** K, D y E de PB-T4c; en PB-T4b, la partición de las copias en el RED (R_C y R_UI), el RED exacto entero, que las '
         'copias pasen en el GREEN, las lecturas de las guardas que solo se ven al ejecutar (el recorrido por diferencia, §4.2, no las ve), las líneas «presentar» '
         'de CT-PBT4B-06, el coste de la lectura con D-S4, el coste de `IsDirty` y la duración de la suite de Core.', '9 copia')

# §11
v = line(v, '2. **Las respuestas del Owner:**',
         '2. **Las respuestas del Owner:** cumplidas (`owner-pbt4-decisions.md`, `owner-pbt4b-retire-list-decisions.md` y, para PB-T4b, `owner-pbt4b-breaks-decision.md`, '
         '`owner-pbt4a-cost-decision.md` y `owner-pbt4b-list-477-decision.md`), con su número F4B-E al custodiarlas; y las decisiones de la sesión, '
         '`session-pbt4b-design-decisions.md` y D-S4 (§8.5), declaradas en F4B-E52.', '11.2')
v = line(v, '   - **PB-T4b** (V44-09; el procedimiento de §6.2.1):',
         '   - **PB-T4b** (V45-04; los ocho pasos de §6.2.1), sobre copias exportadas del cierre de PB-T4a (`78a7cef3`): `apply_red_pbt4b_v45.py` en modo de medida y '
         '`partition_pbt4b_v45.py red` (con la regla de INV-PBT4B-02): la partición, su sha256 y R_C y R_UI; **el RED exacto** con la partición, sin las CT: la suite '
         'completa de Core y los filtros de UI, **Core 19 586 = (19 129 − R_C) + R_C + 457** y **UI 2 478 = (2 430 − R_UI) + R_UI + 48**; **el GREEN literal** '
         '(`apply_green_pbt4b_v45.py`): **Core 19 586 = 19 129 + 0 + 457** y **UI 2 478 = 2 430 + 0 + 48** (con las CT, 19 604 = 19 147 + 0 + 457 y 2 481 = 2 433 + '
         '0 + 48), cero `warning CS` y `partition_pbt4b_v45.py green`: todas las copias pasan; **las dos sondas** sobre la copia del GREEN, en clases temporales '
         'fuera de todo commit: las líneas «presentar» de CT-06 (`pbt4b-ct06-presentar-probe.cs`) y **el coste de la lectura** (`pbt4b-read-cost-probe.cs`: una '
         'materialización en los cuatro casos, y lo esperado de §6.2.1); `check_skips_pbt4b_v45.py`; el coste de `IsDirty` y la duración de la suite. **Una copia '
         'que no pase** pasa a equivalente escrito a mano (D-S1) y la emisión necesita una versión del paquete con su texto; **una prueba que falle fuera de la lista '
         'de 477** vuelve al Owner; **un coste que no cumpla lo esperado** vuelve a la sesión, que lo lleva al Owner;', '11.6')
v = line(v, '7. **La lista que se emite es la aprobada, o una parte:**',
         '7. **La lista que se emite es la aprobada, o una parte:** PB-T4a, 1; **PB-T4b, las 477 de `pbt4b-substitution-list-477.tsv`** (sha256 '
         '`5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651`), que el generador comprueba; PB-T4c, 2. **PB-T4d no se emite por la autorización '
         'condicional:** vuelve al Owner con su lista.', '11.7')
v = line(v, '9. **Para PB-T4b y PB-T4c:**',
         '9. **Para PB-T4b y PB-T4c:** la anterior CERRADA y su remedición (**PB-T4a, CERRADA: F4B-E51, `78a7cef3`**). **En el commit de emisión de PB-T4b,** además: '
         'esta versión (v4.5) y la v4.4, sus fragmentos de `v44/` y `v45/`, las listas (`pbt4b-substitution-list-477.tsv`, `pbt4b-substitution-list-475.tsv` y '
         '`pbt4d-retire-list-138.tsv`), el mapa de la copia v2, el kit de la v4.5, los guiones de la v4.4 y la v4.5 y sus salidas, la partición y las 20 copias '
         'generadas con ella (por sha256), `pbt4b-equivalents-review-v45.txt`, las dos sondas y sus TRX con su sha256, los registros del Owner de la tercera y la quinta '
         'tandas (`owner-pbt4b-breaks-decision.md` y `owner-pbt4b-list-477-decision.md`; `owner-pbt4a-cost-decision.md` ya está en el cierre de PB-T4a), la decisión '
         'de la sesión y la entrada «## PB-T4b» de `superseding-characterization.md`. **Para PB-T4d:** el cierre de PB-T4c, su lista vuelta a medir y la respuesta '
         'del Owner.', '11.9')

for k_ in KEEP:
    assert v.count(k_) == 1, 'PB-T4a cambió'
assert '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.5' in v
data = v.encode('utf-8')
open(OUT, 'wb').write(data)
print('v4.5', hashlib.sha256(data).hexdigest(), len(data), 'bytes')
print('lista 477', sha('pbt4b-substitution-list-477.tsv'))
print('kit v4.5', sha('pbt4b-kit-WorkspaceFullReadKit-v45.cs'), '| make_copies v4.5', sha('make_copies_pbt4b_v45.py'), '| ediciones v4.5', sha('pbt4b_hand_edits_v45.py'),
      '| revision', sha('pbt4b-equivalents-review-v45.txt'), '| conjunto de las copias', probe[0])
print('green_pbt4b_v45.py', sha('green_pbt4b_v45.py'), '| sondas', sha('pbt4b-read-cost-probe.cs'), sha('pbt4b-ct06-presentar-probe.cs'))
print('RequiredTests', len(out_rows), '| filas RR-3 de PB-T4b', sum(1 for l in v.split('\n') if l.startswith('| PB-T4b |')))
