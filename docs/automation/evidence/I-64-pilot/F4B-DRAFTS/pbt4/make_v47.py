"""Arma la v4.7 del paquete de PB-T4 desde la v4.6, el fragmento v47/f47-head.md y la revisión pbt4b-equivalents-review-v47.txt (el anexo V47-A,
el texto literal de las cuatro `Theory` partidas por filas y de los dos miembros nuevos). Solo lee archivos de hs65/pbt4 y escribe la v4.7.
Uso: python -I make_v47.py"""
import hashlib
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
V46 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v46.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v47.md')
raw = open(V46, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == 'e58df774f417ebdb2a0d5cf0e10adab4ead84b2045fea8504eb886fdef54f08f'
t = raw.decode('utf-8')


def sha(name):
    data = open(os.path.join(HERE, name), 'rb').read()
    assert b'\r' not in data, name
    return hashlib.sha256(data).hexdigest()


def once(text, old, new, what, times=1):
    n = text.count(old)
    assert n == times, (what, n)
    return text.replace(old, new)


def section(text, head, until):
    i = text.index('\n' + head) + 1
    j = text.index('\n' + until, i) + 1
    return text[i:j]


def in_section(text, head, until, pairs, what):
    s = section(text, head, until)
    assert text.count(s) == 1, what
    new = s
    for old, rep, times in pairs:
        new = once(new, old, rep, what + ': ' + old[:60], times)
    return text.replace(s, new)


KEEP = [section(t, '### 5.1 PB-T4a', '### 5.2 PB-T4b'), section(t, '### 6.1 PB-T4a', '### 6.2 PB-T4b')]
GEN = sha('make_copies_pbt4b_v47.py')
EDITS = sha('pbt4b_hand_edits_v47.py')
REVIEW = sha('pbt4b-equivalents-review-v47.txt')
MAP4 = sha('pbt4b-copy-map-v4.tsv')
assert MAP4 == '0c5abc6d00d9dfc9943812aae4c9fd31338c3b64f17cbff632f949378e9d9b1c'
SESSION = sha('session-pbt4b-design-decisions.md')
assert SESSION == '47e4bf86d426e0f3903c14ecc7d971999d610dd05c8eb595f37feeb85f3ffa5b'
probe = [l.split()[1] for l in open(os.path.join(HERE, 'v47', 'copies_probe_v47-out.txt'), encoding='utf-8').read().split('\n') if l.startswith('conjunto ')]
assert len(probe) == 1
PROBE = probe[0]
assert PROBE == 'f72b1f2f7b9e7ce2fced8caef466ecec4d87016ec07333fb1ad91c1301e4fb58'  # el mismo que la v4.6: en el modo de medida van enteras

# ---------------------------------------------------------------- la revisión: el anexo V47-A
SEP = '=' * 100
entries = {}
rtext = open(os.path.join(HERE, 'pbt4b-equivalents-review-v47.txt'), encoding='utf-8').read()
parts = rtext.split(SEP + '\n')
assert parts[0] == ''
for k in range(1, len(parts), 2):
    head, body = parts[k], parts[k + 1]
    assert head.endswith('\n')
    path, name = head[:-1].split(' :: ', 1)
    assert (path, name) not in entries
    entries[(path, name)] = body[:-1] if body.endswith('\n') else body
assert len(entries) == 102, len(entries)
WLHT = 'tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs'
PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
WPP = 'tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs'
FOUR = [(WLHT, 'EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively', 'WorkspaceLifecycleHostFull'),
        (PEG, 'TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance', 'WorkspacePilotEditGuardFull'),
        (WPP, 'ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing', 'WorkspacePilotPrepareFull'),
        (WPP, 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading', 'WorkspacePilotPrepareFull')]
annex = ['## Anexo V47-A. El texto literal de las cuatro `Theory` partidas por filas (D-S5) y de los dos miembros nuevos',
         '',
         'Es el de `make_copies_pbt4b_v47.py` con `pbt4b_hand_edits_v47.py`, copiado de su revisión, `pbt4b-equivalents-review-v47.txt` (sha256 `%s`), '
         'por `make_v47.py`. Cada mitad va en su clase de la copia, `<Clase>FullRedTests` o `<Clase>FullGreenTests`, en el lugar de la prueba original y '
         'con la indentación de la clase; lo que manda es el `CopySha` de su archivo.' % REVIEW, '']
n_annex = 0
for path, name, cls in FOUR:
    for half in ('Red', 'Green'):
        body = entries[(path, '%s (%s, D-S5)' % (name, half))]
        assert '```' not in body
        annex += ['**`%s%sTests.%s`** (la mitad %s):' % (cls, half, name, half), '', '```csharp', body.strip('\n'), '```', '']
        n_annex += 1
for name in ('StatusNamesRed', 'StatusNamesGreen'):
    body = entries[(WPP, 'ayudante nuevo ' + name)]
    annex += ['**`WorkspacePilotPrepareFull`, el miembro nuevo `%s`** (solo en la copia; en la clase de la partición cuyas pruebas lo alcanzan):' % name,
              '', '```csharp', body.strip('\n'), '```', '']
    n_annex += 1
assert n_annex == 10
ANNEX = '\n'.join(annex)

v = t
# ---------------------------------------------------------------- la cabecera y el registro de cambios
head = open(os.path.join(HERE, 'v47', 'f47-head.md'), encoding='utf-8').read()
head = head.replace('@@SESSION@@', SESSION).replace('@@MAP4@@', MAP4).replace('@@MAP4S@@', '%s…%s' % (MAP4[:8], MAP4[-4:]))
assert '@@' not in head
v = once(v, '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.6\n',
         '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.7\n\n' + head
         + '\n## La cabecera de la v4.6, sin cambio salvo lo que dice el registro v4.6 → v4.7\n', 'título')
v = once(v, '**Estado:** v4.6. Corrige la parte de', '**Estado de la v4.6:** Corrige la parte de', 'estado v4.6')
v = once(v, '**Lo que sale de la v4.6 (PB-T4b):**', '**Lo que salió de la v4.6 (PB-T4b):**', 'lo que sale')

# ---------------------------------------------------------------- §0 y §1
v = once(v, '(428 de las 9 clases, «Saltar y duplicar», 360 copias y 68 a mano; 22 de la letra; 27 de la UI)',
         '(428 de las 9 clases, «Saltar y duplicar», 358 copias y 70 a mano; 22 de la letra; 27 de la UI)', '§0')
v = once(v, 'La sesión decidió D-S1 a D-S3 (`session-pbt4b-design-decisions.md`) y D-S4 (D-PBT4-21; §8.5).',
         'La sesión decidió D-S1 a D-S3 y D-S5 (`session-pbt4b-design-decisions.md`) y D-S4 (D-PBT4-21; §8.5).', '§1 decisiones')
v = once(v, '| D-S1 a D-S3, y D-S4 (§8.5, D-PBT4-21); la verificación de las cinco roturas y la revisión de la v4.4 |',
         '| D-S1 a D-S3 y D-S5, y D-S4 (§8.5, D-PBT4-21); la verificación de las cinco roturas y la revisión de la v4.4 |', '§1 autoridades')

# ---------------------------------------------------------------- D-PBT4-11
D11 = [
    ('V45-01, V45-05, V45-07, V45-08; V46-01, V46-02)', 'V45-01, V45-05, V45-07, V45-08; V46-01, V46-02; V47-01)', 1),
    ('| **C-0**, las 9 clases | 428 | **360 copias mecánicas** y **68 equivalentes escritos a mano** (los 53 de la v4.3, los 7 de D-S1 y los 8 de la v4.6), por el guion de la copia |',
     '| **C-0**, las 9 clases | 428 | **358 copias mecánicas** y **70 equivalentes escritos a mano** (los 53 de la v4.3, los 7 de D-S1, los 8 de la v4.6 y los 2 de D-S5), por el guion de la copia |', 1),
    ('Los motivos, en ASCII, los fija `pbt4b_skips_v46.py` (con el mapa de la copia v3)',
     'Los motivos, en ASCII, los fija `pbt4b_skips_v47.py` (con el mapa de la copia v4)', 1),
    ('**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v46.py`** (con las piezas de `make_copies_pbt4b.py` de la v4.4, sin tocarlo; las ediciones de `pbt4b_hand_edits_v46.py`, las dos clases de PB-T4a, la lista de 477, el mapa v3, las ediciones de ayudante de C-0 y MU)',
     '**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v47.py`** (con las piezas de `make_copies_pbt4b.py` de la v4.4, sin tocarlo, y las de `make_copies_pbt4b_v46.py`, sin tocarlo; las ediciones de `pbt4b_hand_edits_v47.py`, las dos clases de PB-T4a, la lista de 477, el mapa v4, las ediciones de ayudante de C-0, MU, los miembros nuevos de C-0 y las `Theory` partidas por filas de D-S5)', 1),
    ('según la partición cerrada (en el modo de medida, una sola, `<Clase>FullProbeTests`),',
     'según la partición cerrada (en el modo de medida, una sola, `<Clase>FullProbeTests`; **una `Theory` partida por filas, D-S5, va en las dos**, cada mitad con sus filas, y entera en la de medida),', 1),
    ('**Los 68 a mano** (el mapa de la copia v3, `pbt4b-copy-map-v3.tsv`, sha256 `11b26ab1ecbfbf4a289c1485c14a803529549b2e066e13588668763448390743`, que lleva las siete de D-S1 de la v4.4 con el motivo `R-D-S1` y las ocho de la v4.6 con `R-D-S1-compila`; el v2, `3ffadf5b…9e01`, y `pbt4-copy-map.tsv`, `1091ae50…`, quedan como historia):',
     '**Los 70 a mano** (el mapa de la copia v4, `pbt4b-copy-map-v4.tsv`, sha256 `%s`, que lleva las siete de D-S1 de la v4.4 con el motivo `R-D-S1`, las ocho de la v4.6 con `R-D-S1-compila` y las dos de D-S5 que eran mecánicas con `R-D-S5`; la `Keep…` de WPP, ya a mano por D-S1, lleva `R-D-S1,R-D-S5`; el v3, `11b26ab1…0743`, el v2, `3ffadf5b…9e01`, y `pbt4-copy-map.tsv`, `1091ae50…`, quedan como historia):' % MAP4, 1),
    ('- **38 sin más que M:** en la v4.6,',
     '- **40 sin más que M:** en la v4.7 (V47-01), las dos que D-S5 parte por filas y eran copias mecánicas, WLHT `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` y WPP `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing`: su texto es el de M, y son a mano por el reparto de sus filas; en la v4.6,', 1),
    ('**Los 21 equivalentes de las guardas**',
     '**Las `Theory` partidas por filas** (D-S5; V47-01; `SPLIT`, `RED_KEYS`, `SPLIT_COMMENT` y `C0_NEW_HELPERS` de `pbt4b_hand_edits_v47.py`): las cuatro '
     'que mezclaban resultados en el RED de la v4.6. Dos son de C-0 y eran mecánicas (WLHT `EachOperation…` y WPP `Confirm…`), una es de C-0 y ya era a mano '
     '(WPP `Keep…`, D-S1) y una es un equivalente de guarda (PEG `TheHostGuardTrips…`). **Con la partición, el guion pone su texto dos veces,** en '
     '`<Clase>FullRedTests` con las filas que fallan en el RED y en `<Clase>FullGreenTests` con las que pasan: el mismo nombre, el mismo cuerpo y las mismas '
     'aserciones, con una línea de comentario (`SPLIT_COMMENT`) antes de `[Theory]` que dice a qué mitad pertenece. En las de `InlineData`, cada mitad lleva '
     'sus líneas `[InlineData(…)]` en el orden del original; en las dos de `MemberData`, `StatusNamesRed` o `StatusNamesGreen` en lugar de `StatusNames`. '
     '**Los dos miembros nuevos** van solo en la copia de WPP, tras `StatusNames`, y filtran sus filas por `Clean` sin escribir ninguna. **El guion para** si la '
     'partición no da las dos filas, Red y Green, exactamente para estas cuatro, o si sus filas Red no son las declaradas. En el modo de medida van enteras. '
     'Su texto literal, en el anexo V47-A; la tabla, en la cabecera de la v4.7.\n\n**Los 21 equivalentes de las guardas**', 1),
    ('**Su texto literal:** el de `make_copies_pbt4b_v46.py` con `pbt4b_hand_edits_v46.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v46.txt` lleva el texto final de las 90 entradas (68 a mano, 21 de guarda y el ayudante editado), generado en el modo de medida (§6.2.1).',
     '**Su texto literal:** el de `make_copies_pbt4b_v47.py` con `pbt4b_hand_edits_v47.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v47.txt` lleva el texto final de las 102 entradas (70 a mano, 21 de guarda, el ayudante editado, los dos miembros nuevos y las ocho mitades de D-S5), generado en el modo de medida (§6.2.1).', 1),
]
v = in_section(v, '#### D-PBT4-11.', '#### D-PBT4-21.', D11, 'D-11')

# ---------------------------------------------------------------- §5.2: CT-20, los totales y la regla de la partición
v = in_section(v, '### 5.2 PB-T4b', '### 5.3 PB-T4c', [
    ('### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 21 (entera; V44-07; V45-02, V45-03, V45-11)',
     '### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 21 (entera; V44-07; V45-02, V45-03, V45-11; V47-02, V47-03)', 1),
    ('(2) Cada prueba de la lista, salvo CT-PBT4A-06, tiene en su copia una prueba de igual nombre.',
     '(2) Cada prueba de la lista, salvo CT-PBT4A-06, tiene en su copia una prueba de igual nombre (las cuatro de D-S5, dos: una en `<Clase>FullRedTests` y otra en `<Clase>FullGreenTests`).', 1),
    ('(3) En las 9 clases de C-0, cada copia mecánica (las 360 del mapa de la copia v3)',
     '(3) En las 9 clases de C-0, cada copia mecánica (las 358 del mapa de la copia v4)', 1),
    ('que fija sus equivalentes a mano y las ediciones de ayudante de C-0 (`Env.OpenUndecidable` en `WorkspacePilotEditHostFull`, V46-02). (5)',
     'que fija sus equivalentes a mano, las ediciones de ayudante de C-0 (`Env.OpenUndecidable` en `WorkspacePilotEditHostFull`, V46-02) y los dos miembros nuevos de D-S5 (`StatusNamesRed` y `StatusNamesGreen` en `WorkspacePilotPrepareFull`). (5)', 1),
    ('y el de `FullEditIntentKit.OriginMissing` (`IntentKit.Scan(FullEditIntentKit.RackA, "ZZ", …)`) |',
     'y el de `FullEditIntentKit.OriginMissing` (`IntentKit.Scan(FullEditIntentKit.RackA, "ZZ", …)`). (7) **D-S5** (V47-03): cada una de las cuatro `Theory` '
     'partidas por filas (WLHT `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively`, PEG '
     '`TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` y las dos de WPP, '
     '`ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` y '
     '`KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading`) está en `<Clase>FullRedTests` y en '
     '`<Clase>FullGreenTests`, y sus dos mitades son iguales salvo sus filas y su línea `SPLIT_COMMENT`. En las de `InlineData`, las filas de las dos, '
     'juntas, son las del original con su M o su edición de guarda, una a una y sin repetirse. En las de `MemberData`, `StatusNamesRed` y '
     '`StatusNamesGreen` dan, juntas y sin repetirse, las filas de `StatusNames`, por reflexión en ejecución. Ninguna otra prueba de la lista está en las dos clases |', 1),
    ('(9, con los números del mapa de la copia v3)', '(9, con los números del mapa de la copia v4; `pruebas` cuenta los nombres, y una prueba partida por filas, uno)', 1),
    ('`kit sha256=True`;',
     '`kit sha256=True`; por cada `Theory` partida por filas, `particion-por-filas <Clase>.<método> red=<n> green=<m> filas-del-original=True` (4 líneas: '
     '`WorkspaceLifecycleHost` 3 y 7, `WorkspacePilotEditGuard` 1 y 3, y las dos de `WorkspacePilotPrepare`, 7 y 1);', 1),
    ('y su sustituta repite sus filas (una `Theory` copiada tiene los mismos datos).',
     'y su sustituta repite sus filas (una `Theory` copiada tiene los mismos datos; las cuatro partidas por filas, D-S5, las reparten entre sus dos mitades sin cambiar ninguna).', 1),
    ('con la partición (`partition_pbt4b_v45.py red`, que da el número de resultados de cada clase, y R_C y R_UI). **La regla:** una prueba cuyos resultados del RED son todos `Failed` por aserción va a `…FullRedTests`; todos `Passed`, a `…FullGreenTests`; una que mezcla, se salta o falla sin aserción para la emisión (no se parte una `Theory`; C04).',
     'con la partición (`partition_pbt4b_v47.py red`, que da el número de resultados de cada clase, y R_C y R_UI). **La regla:** una prueba cuyos resultados '
     'del RED son todos `Failed` por aserción va a `…FullRedTests`; todos `Passed`, a `…FullGreenTests`; **una `Theory` que mezcla se parte por filas en dos '
     'equivalentes escritos a mano** (D-S5; V47-02): uno en `…FullRedTests` con las filas que fallan y otro en `…FullGreenTests` con las que pasan, con el '
     'mismo cuerpo y las mismas aserciones; la partición da para ella dos filas, Red y Green, y lo admite solo para las declaradas en el paquete (las cuatro '
     'de D-S5, con sus filas Red: `SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v47.py`). **Para la emisión** una prueba que mezcla sin estar declarada (se le '
     'aplica D-S5 con una versión del paquete), una declarada que no mezcla o mezcla otras filas, y una que se salta o falla sin aserción (C04).', 1),
], '§5.2')

# ---------------------------------------------------------------- RR-3 (§6.0)
v = once(v, '| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20) | **Líneas:** ` igual-a-la-base=True`, ` igual-al-mapa=True`, ` sha256=True`,',
         '| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20) | **Líneas:** ` igual-a-la-base=True`, ` igual-al-mapa=True`, ` sha256=True`, las cuatro `particion-por-filas` con ` filas-del-original=True` (D-S5),', 'RR-3 CT-20')
v = once(v, '| PB-T4b | `WorkspacePilotPrepareFull`, `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` (a mano, R-D-S1) | **El texto entero de la prueba, el del guion**',
         '| PB-T4b | `WorkspacePilotPrepareFull`, `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` (a mano, R-D-S1 y R-D-S5; partida por filas en las dos clases) | **El texto entero de sus dos mitades, el del guion**', 'RR-3 Keep')
v = once(v, '| PB-T4b | `WorkspacePilotEditGuardFull`, `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion**',
         '| PB-T4b | `WorkspacePilotEditGuardFull`, `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` (equivalente de una guarda, C-5; partido por filas en las dos clases, D-S5) | **El texto entero de sus dos mitades, el del guion**', 'RR-3 PEG')
rows = [
    '| PB-T4b | `WorkspaceLifecycleHostFull`, `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` (a mano, R-D-S5; partida por filas en las dos clases) | **El texto entero de sus dos mitades, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n',
    '| PB-T4b | `WorkspacePilotPrepareFull`, `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` (a mano, R-D-S5; partida por filas en las dos clases) | **El texto entero de sus dos mitades, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n',
    '| PB-T4b | `WorkspacePilotPrepareFull`, los miembros nuevos `StatusNamesRed` y `StatusNamesGreen` (solo en la copia, D-S5, V47-01) | **El texto entero de cada miembro, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n',
]
v = once(v, '| PB-T4b | Las 360 copias mecánicas, las 27 de la interfaz y el kit |', ''.join(rows) + '| PB-T4b | Las 358 copias mecánicas, las 27 de la interfaz y el kit |', 'RR-3')

# ---------------------------------------------------------------- §6.2
T62 = [
    ('### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL` (entero; V44-09; V45-01, V45-02, V45-10; V46-01, V46-02)',
     '### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL` (entero; V44-09; V45-01, V45-02, V45-10; V46-01, V46-02; V47-01, V47-02)', 1),
    ('**las 20 copias,** byte a byte las que genera `make_copies_pbt4b_v46.py`', '**las 20 copias,** byte a byte las que genera `make_copies_pbt4b_v47.py`', 1),
    ('apply_red_pbt4b_v46.py', 'apply_red_pbt4b_v47.py', 3),
    ('check_skips_pbt4b_v46.py', 'check_skips_pbt4b_v47.py', 3),
    ('check_skips_copy_v46.py', 'check_skips_copy_v47.py', 1),
    ('pbt4b_skips_v46.py', 'pbt4b_skips_v47.py', 2),
    ('partition_pbt4b_v45.py', 'partition_pbt4b_v47.py', 2),
    ('segun el paquete del contrato de PB-T4 v4.6,', 'segun el paquete del contrato de PB-T4 v4.7,', 1),
    ('(C-0 con sus 9 clases, 360 copias y 68 a mano;', '(C-0 con sus 9 clases, 358 copias y 70 a mano, con cuatro Theory partidas por filas en las dos clases de la particion, D-S5;', 1),
    ('anade 449 copias o equivalentes en Core y 27 en UI, con las mismas filas que sus originales, y 21 CT (18 de Core y 3 de UI), sin ninguna Theory nueva.',
     'anade 449 copias o equivalentes en Core (cuatro de ellas, Theory partidas por filas en dos metodos, uno en cada clase de la particion, que reparten sus filas sin cambiar ninguna; D-S5) y 27 en UI, con las mismas filas que sus originales, y 21 CT (18 de Core y 3 de UI), sin ninguna Theory nueva en las CT.', 1),
    ('| C-0, copia mecánica (360) |', '| C-0, copia mecánica (358) |', 1),
    ('| C-0, a mano (68) |', '| C-0, a mano (70) |', 1),
    ('Para si una prueba mezcla resultados, no corre, se salta o falla sin aserción (la primera línea, en `src/` o sin ruta);',
     '**Una `Theory` que mezcla se parte por filas** (D-S5; V47-02) si es una de las cuatro declaradas y sus filas que fallan son las declaradas (`SPLIT` y '
     '`RED_KEYS` de `pbt4b_hand_edits_v47.py`): da dos filas de la partición, Red y Green, y la salida dice cuántas parte (`4 partidas por filas, D-S5`). **Para** si otra '
     'prueba mezcla resultados (se le aplica D-S5 con una versión del paquete, y el paso 4 se repite), si una de las cuatro no mezcla o mezcla otras filas, o '
     'si una prueba no corre, se salta o falla sin aserción (la primera línea, en `src/` o sin ruta);', 1),
    ('**Las que no pasan** pasan a equivalentes escritos a mano (D-S1): la emisión necesita una versión del paquete con su texto, y el paso 4 se repite.',
     '**Las que no pasan** pasan a equivalentes escritos a mano (D-S1; si una `Theory` pasa en unas filas y no en otras, D-S5): la emisión necesita una versión del paquete con su texto, y el paso 4 se repite.', 1),
    ('| **`Objective`** | §6.2.1 | ASCII, byte a byte; cita el paquete v4.6,', '| **`Objective`** | §6.2.1 | ASCII, byte a byte; cita el paquete v4.7,', 1),
    ('| **El mapa de la copia** | `pbt4b-copy-map-v3.tsv` | sha256 `11b26ab1…0743`; **360** mecánicas y **68** a mano (los motivos de la v4.3, siete `R-D-S1` y ocho `R-D-S1-compila`) |',
     '| **El mapa de la copia** | `pbt4b-copy-map-v4.tsv` | sha256 `%s…%s`; **358** mecánicas y **70** a mano (los motivos de la v4.3, siete `R-D-S1`, ocho `R-D-S1-compila`, dos `R-D-S5` y uno `R-D-S1,R-D-S5`) |' % (MAP4[:8], MAP4[-4:]), 1),
    ('`make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v46.py`, `pbt4b_hand_edits_v45.py` y `pbt4b_hand_edits.py`,',
     '`make_copies_pbt4b_v47.py`, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v47.py`, `pbt4b_hand_edits_v46.py`, `pbt4b_hand_edits_v45.py` y `pbt4b_hand_edits.py`,', 1),
    ('`session-pbt4b-design-decisions.md` (D-S1 a D-S3; D-S4, en §8.5 y D-PBT4-21)', '`session-pbt4b-design-decisions.md` (D-S1 a D-S3 y D-S5; D-S4, en §8.5 y D-PBT4-21)', 1),
    ('este paquete (v4.6), con su comprobación acotada de PB-T4b (CA-T4b), y la §19 de `independent-review-v1.md`.',
     'este paquete (v4.7), con su comprobación acotada de PB-T4b (CA-T4b), y la §19 de `independent-review-v1.md`.', 1),
]
v = in_section(v, '### 6.2 PB-T4b', '### 6.3 PB-T4c', T62, '§6.2')
s62 = section(v, '### 6.2 PB-T4b', '### 6.3 PB-T4c')
gen_line = [l for l in s62.split('\n') if l.startswith('**El guion de la copia y los equivalentes, literales:**')]
assert len(gen_line) == 1
v = once(v, gen_line[0],
         '**El guion de la copia y los equivalentes, literales:** `make_copies_pbt4b_v47.py` (sha256 `%s`), que usa las piezas de `make_copies_pbt4b_v46.py` '
         '(sha256 `%s`) y de `make_copies_pbt4b.py` de la v4.4 (sha256 `%s`) sin tocarlas, con `pbt4b_hand_edits_v47.py` (sha256 `%s`), que carga '
         '`pbt4b_hand_edits_v46.py` (`%s`), `pbt4b_hand_edits_v45.py` (`%s`) y `pbt4b_hand_edits.py` (`%s`), `copy_map_defs.py`, `lists_core.py`, la lista de '
         '477 y el mapa de la copia v4. En el modo de medida, sobre `78a7cef3`, da 20 archivos (`v47/copies_probe_v47`; el sha256 del conjunto de las líneas '
         '«sha256  ruta», ordenadas, `%s`, el mismo que el de la v4.6, porque en el modo de medida las cuatro `Theory` de D-S5 van enteras y ninguna prueba '
         'alcanza los miembros nuevos; `v47/copies_probe_v47-out.txt`), y su revisión, `pbt4b-equivalents-review-v47.txt` (sha256 `%s`), lleva el texto final '
         'de las 102 entradas (70 equivalentes a mano, 21 de guarda, el ayudante editado, los dos miembros nuevos y las ocho mitades de D-S5). Las de la v4.5 y '
         'la v4.6, `pbt4b-equivalents-review-v45.txt` y `pbt4b-equivalents-review-v46.txt`, quedan como historia.'
         % (GEN, sha('make_copies_pbt4b_v46.py'), sha('make_copies_pbt4b.py'), EDITS, sha('pbt4b_hand_edits_v46.py'), sha('pbt4b_hand_edits_v45.py'),
            sha('pbt4b_hand_edits.py'), PROBE, REVIEW), 'generador')
blk = section(v, '#### 6.2.1 ', '#### 6.2.2 ')
code = blk[blk.index('```text\n') + 8:blk.index('\n```', blk.index('```text\n'))]
assert all(ord(ch) < 0x80 for ch in code)

# ---------------------------------------------------------------- §6.3, §8.3, §8.5 y §11
v = once(v, 'segun el paquete del contrato de PB-T4 v4.6 y la lista cerrada CT-PBT4C-01..15', 'segun el paquete del contrato de PB-T4 v4.7 y la lista cerrada CT-PBT4C-01..15', '6.3')
v = once(v, '**360 copias mecánicas y 68 equivalentes a mano** (D-S1; la v4.3, 375 y 53; la v4.5, 368 y 60; `pbt4b-copy-map-v3.tsv`)',
         '**358 copias mecánicas y 70 equivalentes a mano** (D-S1 y D-S5; la v4.3, 375 y 53; la v4.5, 368 y 60; la v4.6, 360 y 68; `pbt4b-copy-map-v4.tsv`)', '8.3')
v = once(v, '- **D-S1 a D-S3** (`session-pbt4b-design-decisions.md`, 2026-11-20): **D-S1**,', '- **D-S1 a D-S3 y D-S5** (`session-pbt4b-design-decisions.md`, 2026-11-20): **D-S1**,', '8.5 a')
v = once(v, 'y admita ediciones de ayudante en las copias de C-0, sin volver al Owner;',
         'y admita ediciones de ayudante en las copias de C-0, sin volver al Owner; **D-S5** (tras la v4.6; V47-01 y V47-02), las cuatro `Theory` que mezclan '
         'resultados en el RED (WLHT, PEG y las dos de WPP) se parten por filas en dos equivalentes escritos a mano, con el mismo cuerpo y las mismas aserciones '
         '(las dos de `MemberData`, con dos miembros de datos nuevos en la copia), y la regla de la partición se amplía solo para eso, con el mecanismo de D-S1 y '
         'sin volver al Owner, porque la lista, las aserciones, las filas y los totales no cambian (358/70 en C-0);', '8.5 b')
v = once(v, 'y D-S1 a D-S4 como las escribió la sesión.', 'y D-S1 a D-S5 como las escribió la sesión.', '8.5 c')
v = once(v, 'y las decisiones de la sesión, `session-pbt4b-design-decisions.md` y D-S4 (§8.5), declaradas en F4B-E52.',
         'y las decisiones de la sesión, `session-pbt4b-design-decisions.md` (D-S1 a D-S3 y D-S5) y D-S4 (§8.5), declaradas en F4B-E52.', '11.2')
p11 = [l for l in v.split('\n') if l.startswith('   - **PB-T4b** (V45-04 y V46-07; los ocho pasos de §6.2.1, con los guiones de la v4.6;')]
assert len(p11) == 1
new11 = (p11[0].replace('apply_red_pbt4b_v46.py', 'apply_red_pbt4b_v47.py').replace('check_skips_pbt4b_v46.py', 'check_skips_pbt4b_v47.py')
         .replace('partition_pbt4b_v45.py red` (con la regla de INV-PBT4B-02)', 'partition_pbt4b_v47.py red` (con la regla de INV-PBT4B-02 y el reparto por filas de D-S5)')
         .replace('partition_pbt4b_v45.py green', 'partition_pbt4b_v47.py green')
         .replace('**Una copia que no pase** pasa a equivalente escrito a mano (D-S1)', '**Una copia que no pase** pasa a equivalente escrito a mano (D-S1), **una `Theory` que mezcle** se parte por filas (D-S5)')
         .replace('   - **PB-T4b** (V45-04 y V46-07; los ocho pasos de §6.2.1, con los guiones de la v4.6;', '   - **PB-T4b** (V45-04, V46-07 y V47-05; los ocho pasos de §6.2.1, con los guiones de la v4.7;'))
assert 'v46' not in new11 and 'v45.py red' not in new11 and new11.count('D-S5') == 2, new11
v = once(v, p11[0], new11, '11.6')
p9 = [l for l in v.split('\n') if l.startswith('9. **Para PB-T4b y PB-T4c:**')]
assert len(p9) == 1
new9 = (p9[0].replace('esta versión (v4.6), la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/` y `v46/`,', 'esta versión (v4.7), la v4.6, la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/`, `v46/` y `v47/`,')
        .replace('los mapas de la copia v2 y v3,', 'los mapas de la copia v2, v3 y v4,')
        .replace('los guiones de la v4.4 y la v4.5 y sus salidas,', 'los guiones de la v4.4 a la v4.7 y sus salidas,')
        .replace('`pbt4b-equivalents-review-v45.txt` y `pbt4b-equivalents-review-v46.txt`,', '`pbt4b-equivalents-review-v45.txt`, `pbt4b-equivalents-review-v46.txt` y `pbt4b-equivalents-review-v47.txt`,'))
assert new9.count('v4.7') == 2 and 'v47.txt' in new9 and 'v2, v3 y v4' in new9, new9
v = once(v, p9[0], new9, '11.9')

# ---------------------------------------------------------------- el anexo V47-A, al final
assert v.endswith('\n')
v = v + '\n' + ANNEX + ('' if ANNEX.endswith('\n') else '\n')

for k_ in KEEP:
    assert v.count(k_) == 1, 'PB-T4a cambió'
data = v.encode('utf-8')
assert b'\r' not in data
open(OUT, 'wb').write(data)
print('v4.7', hashlib.sha256(data).hexdigest(), len(data), 'bytes')
print('mapa v4', MAP4, '| generador', GEN, '| ediciones', EDITS, '| revision', REVIEW, '| conjunto', PROBE, '| sesion', SESSION)
print('filas RR-3 de PB-T4b', sum(1 for l in v.split('\n') if l.startswith('| PB-T4b |')))
print('D-S5 en el paquete', v.count('D-S5'), '| anexo', n_annex, 'bloques')
