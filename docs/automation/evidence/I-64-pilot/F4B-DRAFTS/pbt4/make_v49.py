"""Arma la v4.9 del paquete de PB-T4 desde la v4.8, el fragmento v49/f49-head.md y la revisión pbt4b-equivalents-review-v49.txt (el anexo V49-A, que
sustituye al V48-A: el mismo, con el texto nuevo del equivalente de WHHG, CA-T4b I-01). Solo lee archivos de hs65/pbt4 y escribe la v4.9.
Uso: python -I make_v49.py"""
import hashlib
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
V48 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v48.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v49.md')
raw = open(V48, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '6ddc239d4db7509d50b2a2497a7adc91b6808ea4ee3511098a66446c9b25e0a2'
t = raw.decode('utf-8')


def sha(name):
    data = open(os.path.join(HERE, name), 'rb').read()
    assert b'\r' not in data, name
    return hashlib.sha256(data).hexdigest()


SESSION, OWNERCOST = sha('session-pbt4b-design-decisions.md'), sha('owner-pbt4b-cost-decision.md')
assert SESSION == '8b18add602766c705260e976e3566b151e13567ee3f2b40a0c0ff12bd193e934', SESSION
assert OWNERCOST == 'd0f4ca259f26fbf91676538d0895eca7acf921b76d5c56eb47885965213f5576', OWNERCOST
GEN, EDITS, REVIEW = sha('make_copies_pbt4b_v49.py'), sha('pbt4b_hand_edits_v49.py'), sha('pbt4b-equivalents-review-v49.txt')
probe = [l.split()[1] for l in open(os.path.join(HERE, 'v49', 'copies_probe_v49-out.txt'), encoding='utf-8').read().split('\n') if l.startswith('conjunto ')]
assert len(probe) == 1
PROBE = probe[0]

# ---------------------------------------------------------------- el anexo V49-A, de la revisión de la v4.9
SEP = '=' * 100
entries = {}
parts = open(os.path.join(HERE, 'pbt4b-equivalents-review-v49.txt'), encoding='utf-8').read().split(SEP + '\n')
assert parts[0] == ''
for k in range(1, len(parts), 2):
    path, name = parts[k][:-1].split(' :: ', 1)
    body = parts[k + 1]
    entries[(path, name)] = body[:-1] if body.endswith('\n') else body
assert len(entries) == 114, len(entries)
WPP = 'tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs'
WLHT = 'tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs'
PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
WHHG = 'tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs'
INVALID_E = 'InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView'
NEVER = 'ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan'
NONE30 = 'NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept'
assert entries[(WHHG, NONE30)].count('Assert.NotEmpty(') == 13
annex = ['## Anexo V49-A. El texto literal de lo escrito a mano de la v4.8 y la v4.9, y de las cuatro `Theory` partidas por filas', '',
         'Copiado por `make_v49.py` de la revisión del guion, `pbt4b-equivalents-review-v49.txt` (sha256 `%s`, 114 entradas). **Sustituye al anexo '
         'V48-A:** es el mismo salvo el equivalente de WHHG `NoneOfThe30…`, que trae la autoprueba de la guarda (CA-T4b I-01; V49-01). Cada prueba va en '
         'su clase de la copia, en el lugar de la prueba original y con la indentación de la clase; lo que manda es el `CopySha` de su archivo.' % REVIEW, '']
n_annex = 0


def block(title, key):
    global n_annex
    body = entries[key]
    assert '```' not in body
    annex.extend([title, '', '```csharp', body.strip('\n'), '```', ''])
    n_annex += 1


annex.extend(['### D-S1 en dos copias de WPP (V48-02)', ''])
block('**`FullPanelCopyWorkspacePilotPrepare<Red|Green>Tests.%s`** (la clase, la de la partición):' % INVALID_E, (WPP, INVALID_E))
block('**`FullPanelCopyWorkspacePilotPrepare<Red|Green>Tests.%s`**:' % NEVER, (WPP, NEVER))
annex.extend(['### La guarda de la sexta tanda (V48-03; C-5), con su autoprueba (CA-T4b I-01; V49-01)', ''])
block('**`FullPanelCopyWorkspaceHostHardeningGuard<Red|Green>Tests.%s`** (la prueba del original y, tras su `Assert.Empty`, la autoprueba WHHG:504-525, con sus 13 `Assert.NotEmpty`):' % NONE30, (WHHG, NONE30))
block('**`WorkspaceHostHardeningGuardFull`, el ayudante `Table`** (la tabla, con tres cuentas nuevas):', (WHHG, 'ayudante Table'))
annex.extend(['### Las cuatro `Theory` partidas por filas (D-S5) y los dos miembros nuevos (con los nombres de D-S6)', ''])
for path, name, stem in ((WLHT, 'EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively', 'WorkspaceLifecycleHost'),
                         (PEG, 'TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance', 'WorkspacePilotEditGuard'),
                         (WPP, 'ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing', 'WorkspacePilotPrepare'),
                         (WPP, 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading', 'WorkspacePilotPrepare')):
    for half in ('Red', 'Green'):
        block('**`FullPanelCopy%s%sTests.%s`** (la mitad %s):' % (stem, half, name, half), (path, '%s (%s, D-S5)' % (name, half)))
for name in ('StatusNamesRed', 'StatusNamesGreen'):
    block('**`WorkspacePilotPrepareFull`, el miembro nuevo `%s`** (solo en la copia; en la clase de la partición cuyas pruebas lo alcanzan):' % name,
          (WPP, 'ayudante nuevo ' + name))
assert n_annex == 14
ANNEX = '\n'.join(annex)

# ---------------------------------------------------------------- la cabecera (y V48-02, M-02)
head = open(os.path.join(HERE, 'v49', 'f49-head.md'), encoding='utf-8').read().replace('@@SESSION@@', SESSION).replace('@@OWNERCOST@@', OWNERCOST)
assert '@@' not in head
TITLE = '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.8\n'
assert t.count(TITLE) == 1
t = t.replace(TITLE, TITLE.replace('v4.8', 'v4.9') + '\n' + head + '\n## La cabecera de la v4.8, sin cambio salvo lo que dice el registro v4.8 → v4.9\n')
for old, new in (('**Estado:** v4.8. Corrige la parte de', '**Estado de la v4.8:** Corrige la parte de'),
                 ('**Lo que sale de la v4.8 (PB-T4b):**', '**Lo que salió de la v4.8 (PB-T4b):**'),
                 ('así que irían a la clase Green | D-PBT4-11; §6.0 (RR-3); §8.3 |',
                  'así que irían a la clase Green. **Sus nombres** («…NeverErasesAView…», «…MakesTheUpdateInvalid…») dicen lo contrario de lo que afirman: '
                  'se conservan por el mecanismo (CT-20 (2) y la partición), y la regla es la del cuerpo (CA-T4b M-02; V49-03) | D-PBT4-11; §6.0 (RR-3); §8.3 |')):
    assert t.count(old) == 1, old[:60]
    t = t.replace(old, new)

cut = t.index('\n## 0. En una página')
a48 = t.index('\n## Anexo V48-A.')
headpart, body = t[:cut], t[cut:a48 + 1]
done = []


def R(old, new, times=1, what=None):
    global body
    n = body.count(old)
    assert n == times, (what or old[:90], n, times)
    body = body.replace(old, new)
    done.append(what or old[:60])


def SEC(head_, until, pairs, what):
    global body
    i = body.index('\n' + head_) + 1
    j = body.index('\n' + until, i) + 1
    s = body[i:j]
    for old, new, times in pairs:
        n = s.count(old)
        assert n == times, (what, old[:90], n, times)
        s = s.replace(old, new)
    body = body[:i] + s + body[j:]
    done.append(what)


# ---- §1
R('y en la sexta, «Añadirla a la lista» (`owner-pbt4b-list-478-decision.md`). La sesión decidió',
  'y en la sexta, «Añadirla a la lista» (`owner-pbt4b-list-478-decision.md`); y en la séptima, sobre el coste de la lectura de PB-T4b, «Darlo por cumplido» (`owner-pbt4b-cost-decision.md`). La sesión decidió')
R('| «Añadirla a la lista»: **478** en PB-T4b (§8.5) |\n',
  '| «Añadirla a la lista»: **478** en PB-T4b (§8.5) |\n'
  '| **La decisión del Owner sobre el coste de la lectura de PB-T4b** (`owner-pbt4b-cost-decision.md`, 2026-11-21, la séptima tanda) | «Darlo por cumplido»: el paso 7 de la precondición 6, con dos de tres corridas (§8.5) |\n')
# ---- D-PBT4-03 (M-04)
R('y con el origen encontrado no se materializa: `AtOrigin`, NH, `ReadAt` y las cinco relecturas de WHC no cambian.',
  'y con el origen encontrado no se materializa. **D-S4 no toca** `AtOrigin`, NH, `ReadAt` ni las cinco relecturas de WHC (CA-T4b M-04; V49-05); WHC sí cambia en el GREEN de PB-T4b, por D-PBT4-04 y D-PBT4-08.')
# ---- D-PBT4-11
R('V47-01; V48-01 a V48-03)', 'V47-01; V48-01 a V48-03; V49-01)')
R('**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v48.py`** (con las piezas de `make_copies_pbt4b.py` de la v4.4, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b_v47.py`, sin tocarlos; las ediciones de `pbt4b_hand_edits_v48.py`,',
  '**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v49.py`** (el de la v4.8, `make_copies_pbt4b_v48.py`, con las piezas de `make_copies_pbt4b.py` de la v4.4, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b_v47.py`, sin tocarlos; las ediciones de `pbt4b_hand_edits_v49.py`,')
R('con su prueba sin cambio y su tabla `Table` con tres cuentas nuevas (2 → 0, 5 → 6 y 1 → 0, cada una con su `// before N`; el anexo V48-A); y, de antes,',
  'con su tabla `Table` con tres cuentas nuevas (2 → 0, 5 → 6 y 1 → 0, cada una con su `// before N`) y, **desde la v4.9** (CA-T4b I-01; V49-01), su prueba con la '
  '**autoprueba de la guarda** tras su `Assert.Empty`: las líneas WHHG:504-525, literales (sus dos variables y sus 13 `Assert.NotEmpty`), sobre su `LiteralViolations` y '
  'esa tabla, como hace el equivalente de G-4 (el anexo V49-A); y, de antes,')
R('**Su texto literal:** el de `make_copies_pbt4b_v48.py` con `pbt4b_hand_edits_v48.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v48.txt`',
  '**Su texto literal:** el de `make_copies_pbt4b_v49.py` con `pbt4b_hand_edits_v49.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v49.txt`')
R('anexo V48-A', 'anexo V49-A', 2)
# ---- §4.2 (I-01, M-01, M-03)
R('| Su equivalente en `WorkspaceHostHardeningGuardFull`, con la misma tabla y esas tres cuentas |',
  '| Su equivalente en `WorkspaceHostHardeningGuardFull`, con la misma tabla y esas tres cuentas, y con la autoprueba de la guarda (WHHG:504-525) tras su `Assert.Empty` (CA-T4b I-01) |')
R('**No entra en la lista:** es una autoprueba que inyecta textos sobre el anfitrión y **pasa en las dos suites completas del GREEN** (`/d/r4m` y `/d/r4o`);',
  '**No entra en la lista.** Pasa en las dos suites completas del GREEN (`/d/r4m` y `/d/r4o`), **pero en vacío** (CA-T4b I-01; V49-01): llama a `LiteralViolations` '
  'sobre el anfitrión y el soporte reales (WHHG:504-505), con la tabla del original (2, 5 y 1), y solo exige `NotEmpty`, 13 veces, sin una línea base vacía. En el GREEN '
  'esas tres cuentas ya no casan (0, 6 y 0), así que `LiteralViolations` sin mutar ya da tres violaciones, y las 13 se cumplen con cualquier mutación. **Por eso el '
  'equivalente de `NoneOfThe30…` trae esa autoprueba sobre su tabla nueva**, como el de G-4 (la v4.8 daba otra razón: «es una autoprueba que inyecta textos sobre el '
  'anfitrión y pasa»);')
R('(WCAG:408 y :447)', '(WCAG:408-419; las filas que cuenta el paseo son :415-419; CA-T4b M-03)')
R('por eso el equivalente de :504-508 trae su autoprueba sobre `ExecuteFull` (§17.2 de la revisión).',
  'por eso el equivalente de :504-508 trae su autoprueba sobre `ExecuteFull` (§17.2 de la revisión). **Lo mismo pasa con las autopruebas de `HostViolations` de PEG '
  'que quedan fuera de la lista** (CA-T4b M-01; V49-02): PEG:401-416, :418-431, :433-451, :453-468, :470-492 y :516-526 (6 métodos, 31 resultados) **pasan en vacío en '
  'el GREEN**, porque el anfitrión del GREEN ya incumple la regla del adaptador estático del piloto (PEG:279-280), así que sus `NotEmpty` se cumplen sin la mutación. '
  '**La mitigación:** CT-20 (1) y (4) fijan el texto de `HostViolations` del original y de la copia; la copia conserva parte del lado que salta (la `Theory` partida de '
  'D-S5, la del orden del vuelo y la de la trampa); y PB-T4d (C-7) las sustituye. **La autoprueba de la guarda de los textos de WHHG** (WHHG:501-526) pasa en vacío por '
  'la misma razón, y esa sí la trae el equivalente de `NoneOfThe30…` (CA-T4b I-01; arriba).')

# ---- §6.2 y §6.2.1
SEC('### 6.2 PB-T4b', '### 6.3 PB-T4c', [
    ('V47-01, V47-02; V48-01 a V48-05)', 'V47-01, V47-02; V48-01 a V48-05; V49-01, V49-06)', 1),
    ('  - **las 21 copias,** byte a byte las que genera `make_copies_pbt4b_v48.py`', '  - **las 21 copias,** byte a byte las que genera `make_copies_pbt4b_v49.py`', 1),
    ('apply_red_pbt4b_v48.py', 'apply_red_pbt4b_v49.py', 3),
    ('segun el paquete del contrato de PB-T4 v4.8, las decisiones del Owner sobre PB-T4 (sus seis tandas, con la del coste de la lectura de PB-T4a)',
     'segun el paquete del contrato de PB-T4 v4.9, las decisiones del Owner sobre PB-T4 (sus siete tandas, con las del coste de la lectura de PB-T4a y de PB-T4b)', 1),
    ('**Si no se cumple,** la emisión se detiene y vuelve a la sesión, que lo lleva al Owner (un efecto material, pregunta 4); las TRX y su sha256 van con la emisión;',
     '**Si no se cumple,** la emisión se detiene y vuelve a la sesión, que lo lleva al Owner (un efecto material, pregunta 4); las TRX y su sha256 van con la emisión. '
     '**En la v4.8, la primera de tres corridas no cumplió un caso, y el Owner lo dio por cumplido** (`owner-pbt4b-cost-decision.md`, la séptima tanda; las tres '
     'corridas, en `pbt4bcc/compile/compile-check.md`); **la v4.9 conserva ese paso 7**, porque no cambia ninguna de sus entradas, comprobado por sha256 (V49-06);', 1),
], '§6.2')
s62 = body[body.index('\n### 6.2 PB-T4b'):body.index('\n### 6.3 PB-T4c')]
gen_line = [l for l in s62.split('\n') if l.startswith('**El guion de la copia y los equivalentes, literales:**')]
assert len(gen_line) == 1
R(gen_line[0],
  '**El guion de la copia y los equivalentes, literales:** `make_copies_pbt4b_v49.py` (sha256 `%s`), el de la v4.8 (`make_copies_pbt4b_v48.py`, sha256 `%s`) con '
  '`pbt4b_hand_edits_v49.py` (sha256 `%s`), que carga `pbt4b_hand_edits_v48.py` (`%s`); y, como en la v4.8, las piezas de `make_copies_pbt4b_v47.py` (`%s`), '
  '`make_copies_pbt4b_v46.py` (`%s`) y `make_copies_pbt4b.py` de la v4.4 (`%s`), sin tocarlas, con `pbt4b_hand_edits_v47.py` (`%s`), `pbt4b_hand_edits_v46.py` (`%s`), '
  '`pbt4b_hand_edits_v45.py` (`%s`) y `pbt4b_hand_edits.py` (`%s`), `copy_map_defs.py`, `lists_core.py`, la lista de 478 y el mapa de la copia v5. En el modo de medida, '
  'sobre `78a7cef3`, da 21 archivos (`v49/copies_probe_v49`; el sha256 del conjunto de las líneas «sha256  ruta», ordenadas, `%s`; `v49/copies_probe_v49-out.txt`): '
  'las de la v4.8 salvo `WorkspaceHostHardeningGuardFull.cs`. Su revisión, `pbt4b-equivalents-review-v49.txt` (sha256 `%s`), lleva el texto final de las 114 entradas. '
  'Las de la v4.5 a la v4.8 quedan como historia. **Las autopruebas:** `selftest_pbt4b_v48.py` (`v48/selftest_pbt4b_v48-out.txt`) y `selftest_pbt4b_v49.py` '
  '(`v49/selftest_pbt4b_v49-out.txt`), que comprueba que la copia de WHHG es la de la v4.8 con un solo bloque tras el `Assert.Empty`: el comentario de I-01 y las '
  'líneas WHHG:504-525 del objeto git, literales, con sus 13 `Assert.NotEmpty`.'
  % (GEN, sha('make_copies_pbt4b_v48.py'), EDITS, sha('pbt4b_hand_edits_v48.py'), sha('make_copies_pbt4b_v47.py'), sha('make_copies_pbt4b_v46.py'),
     sha('make_copies_pbt4b.py'), sha('pbt4b_hand_edits_v47.py'), sha('pbt4b_hand_edits_v46.py'), sha('pbt4b_hand_edits_v45.py'), sha('pbt4b_hand_edits.py'),
     PROBE, REVIEW), 1, 'generador')
blk = body[body.index('\n#### 6.2.1 '):body.index('\n#### 6.2.2 ')]
code = blk[blk.index('```text\n') + 8:blk.index('\n```', blk.index('```text\n'))]
assert all(ord(ch) < 0x80 for ch in code)
SEC('#### 6.2.2 ', '### 6.3 PB-T4c', [
    ('cita el paquete v4.8, CT-PBT4B-01..21,', 'cita el paquete v4.9, CT-PBT4B-01..21,', 1),
    ('`make_copies_pbt4b_v48.py`, `make_copies_pbt4b_v47.py`, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v48.py`, `pbt4b_hand_edits_v47.py`,',
     '`make_copies_pbt4b_v49.py`, `make_copies_pbt4b_v48.py`, `make_copies_pbt4b_v47.py`, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v49.py`, `pbt4b_hand_edits_v48.py`, `pbt4b_hand_edits_v47.py`,', 1),
    ('y `owner-pbt4b-list-478-decision.md` (la sexta), que la emisión', ', `owner-pbt4b-list-478-decision.md` (la sexta) y `owner-pbt4b-cost-decision.md` (la séptima, el coste), que la emisión', 1),
    ('este paquete (v4.8), con su comprobación acotada de PB-T4b (CA-T4b)', 'este paquete (v4.9), con su comprobación acotada de PB-T4b (CA-T4b: la §20 de `independent-review-v1.md`, sobre la v4.8)', 1),
], '§6.2.2')

# ---- §6.3, §8 y §11
R('segun el paquete del contrato de PB-T4 v4.8 y la lista cerrada CT-PBT4C-01..15', 'segun el paquete del contrato de PB-T4 v4.9 y la lista cerrada CT-PBT4C-01..15')
R('la v4.7, 358 y 70; `pbt4b-copy-map-v5.tsv`) | 428 | **428** | |',
  'la v4.7, 358 y 70; `pbt4b-copy-map-v5.tsv`). Los nombres de las dos de WPP de la v4.8 se conservan por el mecanismo (CT-20 (2) y la partición), y la regla es la del cuerpo (CA-T4b M-02) | 428 | **428** | |')
R('vuelve al Owner |\n\n**Las decisiones de la sesión,**',
  'vuelve al Owner |\n'
  '| **7.ª** (2026-11-21) | `owner-pbt4b-cost-decision.md` | «Coste T4b»: «Darlo por cumplido (Recomendado)». «Sonda dedicada antes» y «No emitir PB-T4b» no se hacen | '
  '**El coste de la lectura de la precondición 6 se da por cumplido** con dos de tres corridas que cumplen todo (la primera incumple un solo caso, `pb-20-ab x4 vistas +200 '
  'racks ajenos`, `origen-es-Primary`); las tres corridas y sus TRX, por sha256, van con la emisión; el coste real en AutoCAD queda para el Smoke-PB; **no cambia ningún '
  'criterio del paquete** |\n\n**Las decisiones de la sesión,**')
R('que custodia los seis registros del Owner sobre PB-T4', 'que custodia los siete registros del Owner sobre PB-T4')
R('la v4.8 aplica la 6.ª como dice (la guarda de los textos del anfitrión, en la lista con su equivalente).',
  'la v4.8 aplica la 6.ª como dice (la guarda de los textos del anfitrión, en la lista con su equivalente), y la v4.9, la 7.ª (el coste, dado por cumplido, sin cambiar ningún criterio).')
R('`owner-pbt4b-list-477-decision.md` y `owner-pbt4b-list-478-decision.md`), con su número F4B-E al custodiarlas;',
  '`owner-pbt4b-list-477-decision.md`, `owner-pbt4b-list-478-decision.md` y `owner-pbt4b-cost-decision.md`), con su número F4B-E al custodiarlas;')
p11 = [l for l in body.split('\n') if l.startswith('   - **PB-T4b** (V45-04, V46-07, V47-05 y V48-06;')]
assert len(p11) == 1
new11 = (p11[0].replace('(V45-04, V46-07, V47-05 y V48-06; los ocho pasos de §6.2.1, con los guiones de la v4.8;',
                        '(V45-04, V46-07, V47-05, V48-06 y V49-06; los ocho pasos de §6.2.1, con los guiones de la v4.9, y el paso 7, el de la v4.8 (V49-06);')
         .replace('apply_red_pbt4b_v48.py', 'apply_red_pbt4b_v49.py')
         .replace('**un coste que no cumpla lo esperado** vuelve a la sesión, que lo lleva al Owner;',
                  '**un coste que no cumpla lo esperado** vuelve a la sesión, que lo lleva al Owner (el de la v4.8 lo dio por cumplido el Owner, `owner-pbt4b-cost-decision.md`);'))
assert 'apply_red_pbt4b_v48.py' not in new11 and new11.count('v4.9') == 1, new11
R(p11[0], new11, 1, '11.6')
p9 = [l for l in body.split('\n') if l.startswith('9. **Para PB-T4b y PB-T4c:**')]
assert len(p9) == 1
new9 = (p9[0].replace('esta versión (v4.8), la v4.7, la v4.6, la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/`, `v46/`, `v47/` y `v48/`,',
                      'esta versión (v4.9), la v4.8, la v4.7, la v4.6, la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/`, `v46/`, `v47/`, `v48/` y `v49/`,')
        .replace('los guiones de la v4.4 a la v4.8 y sus salidas,', 'los guiones de la v4.4 a la v4.9 y sus salidas,')
        .replace('`pbt4b-equivalents-review-v47.txt` y `pbt4b-equivalents-review-v48.txt`,', '`pbt4b-equivalents-review-v47.txt`, `pbt4b-equivalents-review-v48.txt` y `pbt4b-equivalents-review-v49.txt`,')
        .replace('`owner-pbt4b-list-477-decision.md` y `owner-pbt4b-list-478-decision.md`;', '`owner-pbt4b-list-477-decision.md`, `owner-pbt4b-list-478-decision.md` y, la séptima, `owner-pbt4b-cost-decision.md`;')
        .replace('los registros del Owner de la tercera, la quinta y la sexta tandas', 'los registros del Owner de la tercera, la quinta, la sexta y la séptima tandas')
        .replace('el análisis de la v4.7 (`v47/analysis-outside-477.md`),', 'el análisis de la v4.7 (`v47/analysis-outside-477.md`), la regla de la tercera corrida del coste (`v48/session-cost-rerun-rule.md`),'))
assert new9.count('v4.9') == 2 and 'review-v49' in new9 and 'owner-pbt4b-cost-decision.md' in new9 and 'session-cost-rerun-rule' in new9, new9
R(p9[0], new9, 1, '11.9')

# ---- comprobaciones y escritura
assert 'anexo V48-A' not in body
KEEP = []
for h_, u_ in (('### 5.1 PB-T4a', '### 5.2 PB-T4b'), ('### 6.1 PB-T4a', '### 6.2 PB-T4b')):
    i_ = t.index('\n' + h_) + 1
    KEEP.append(t[i_:t.index('\n' + u_, i_) + 1])
v = headpart + body + ANNEX + '\n'
for k_ in KEEP:
    assert v.count(k_) == 1, 'PB-T4a cambió'
data = v.encode('utf-8')
assert b'\r' not in data
open(OUT, 'wb').write(data)
print('v4.9', hashlib.sha256(data).hexdigest(), len(data), 'bytes; reemplazos', len(done))
print('sesion', SESSION, '| coste del Owner', OWNERCOST, '| generador', GEN, '| ediciones', EDITS, '| revision', REVIEW, '| conjunto', PROBE)
print('anexo', n_annex, 'bloques | CA-T4b I-01', v.count('CA-T4b I-01'), '| V49-', v.count('V49-'))
