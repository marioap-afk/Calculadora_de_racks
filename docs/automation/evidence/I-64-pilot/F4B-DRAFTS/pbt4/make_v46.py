"""Arma la v4.6 del paquete de PB-T4 desde la v4.5 y los fragmentos de v46/ (solo lee archivos de hs65/pbt4 y escribe la v4.6).
Uso: python -I make_v46.py"""
import hashlib
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
V45 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v45.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v46.md')
raw = open(V45, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '6543b99d6278ad95d5ce77c1f225a1e893932795d687560d6e77d3c0b7ef31cb'
t = raw.decode('utf-8')


def frag(name):
    return open(os.path.join(HERE, 'v46', name), encoding='utf-8').read()


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
GEN = sha('make_copies_pbt4b_v46.py')
EDITS = sha('pbt4b_hand_edits_v46.py')
REVIEW = sha('pbt4b-equivalents-review-v46.txt')
MAP3 = sha('pbt4b-copy-map-v3.tsv')
assert MAP3 == '11b26ab1ecbfbf4a289c1485c14a803529549b2e066e13588668763448390743'
probe = [l.split()[1] for l in open(os.path.join(HERE, 'v46', 'copies_probe_v46-out.txt'), encoding='utf-8').read().split('\n') if l.startswith('conjunto ')]
assert len(probe) == 1
PROBE = probe[0]

v = t
# ---------------------------------------------------------------- la cabecera y el registro de cambios
v = once(v, '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.5\n',
         '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.6\n\n' + frag('f46-head.md')
         + '\n## La cabecera de la v4.5, sin cambio salvo lo que dice el registro v4.5 → v4.6\n', 'título')
v = once(v, '**Estado:** v4.5. Cierra la parte de', '**Estado de la v4.5:** Cierra la parte de', 'estado v4.5')
v = once(v, '**Lo que sale de esta versión (PB-T4b):**', '**Lo que salió de la v4.5 (PB-T4b):**', 'lo que sale')
v = once(v, '`FullHandKit.FullOfMatchesTheReader`, que CT-20 corre sobre seis escaneos', '`FullHandKit.FullOfMatchesTheReader`, que CT-20 corre sobre cinco escaneos (V46-03)', 'M-07 a')

# ---------------------------------------------------------------- §0
v = once(v, '(428 de las 9 clases, «Saltar y duplicar», 368 copias y 60 a mano; 22 de la letra; 27 de la UI)',
         '(428 de las 9 clases, «Saltar y duplicar», 360 copias y 68 a mano; 22 de la letra; 27 de la UI)', '§0')

# ---------------------------------------------------------------- D-PBT4-11
D11 = [
    ('V45-01, V45-05, V45-07, V45-08)', 'V45-01, V45-05, V45-07, V45-08; V46-01, V46-02)', 1),
    ('| **C-0**, las 9 clases | 428 | **368 copias mecánicas** y **60 equivalentes escritos a mano** (los 53 de la v4.3 y los 7 de D-S1), por el guion de la copia |',
     '| **C-0**, las 9 clases | 428 | **360 copias mecánicas** y **68 equivalentes escritos a mano** (los 53 de la v4.3, los 7 de D-S1 y los 8 de la v4.6), por el guion de la copia |', 1),
    ('Los motivos, en ASCII, los fija `pbt4b_skips_v45.py`, uno por causa',
     'Los motivos, en ASCII, los fija `pbt4b_skips_v46.py` (con el mapa de la copia v3), uno por causa', 1),
    ('**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v45.py`** (`make_copies_pbt4b.py` de la v4.4, sin tocarlo, con las ediciones de `pbt4b_hand_edits_v45.py`, las dos clases de PB-T4a y la lista de 477)',
     '**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v46.py`** (con las piezas de `make_copies_pbt4b.py` de la v4.4, sin tocarlo; las ediciones de `pbt4b_hand_edits_v46.py`, las dos clases de PB-T4a, la lista de 477, el mapa v3, las ediciones de ayudante de C-0 y MU)', 1),
    ('CT-PBT4B-20 lo corre sobre seis escaneos', 'CT-PBT4B-20 lo corre sobre cinco escaneos (V46-03)', 1),
    ('| M-08 | `.Inputs.PalletTolerance` | `.Editable.PalletTolerance` |\n',
     '| M-08 | `.Inputs.PalletTolerance` | `.Editable.PalletTolerance` |\n'
     '| **MU** (v4.6) | Una copia que nombra un tipo de `Draft/` (un tipo declarado en `src/RackCad.Application/Systems/PushBack/Draft/`, fuera de cadenas y comentarios) y no lleva la directiva | `using RackCad.Application.Systems.PushBack.Draft;` tras el último `using` de su cabecera (V46-01): M-04 y M-06 cambian el nombre del tipo, no su espacio de nombres |\n', 1),
    ('M-01 a M-08 son de palabra completa.', 'M-01 a M-08 son de palabra completa; MU no cambia ningún nombre, solo añade la directiva (en la v4.6, diez copias la llevan: nueve la ganan y `PushBackFullReadPathFull` la hereda).', 1),
    ('**Los 60 a mano** (el mapa de la copia v2, `pbt4b-copy-map-v2.tsv`, sha256 `3ffadf5b1f19461564d2e3f2f1c3e829395b8ade2f6d43293742619ffe359e01`, que lleva las siete de D-S1 con el motivo `R-D-S1`; `pbt4-copy-map.tsv`, `1091ae50…`, queda como historia):',
     '**Los 68 a mano** (el mapa de la copia v3, `pbt4b-copy-map-v3.tsv`, sha256 `%s`, que lleva las siete de D-S1 de la v4.4 con el motivo `R-D-S1` y las ocho de la v4.6 con `R-D-S1-compila`; el v2, `3ffadf5b…9e01`, y `pbt4-copy-map.tsv`, `1091ae50…`, quedan como historia):' % MAP3, 1),
    ('- **26 con texto propio:** 14 de WPP',
     '- **30 con texto propio:** en la v4.6 (V46-02), dos de WPEH (`TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull` y `TheReasonOfAFieldThatCannotBeReadIsInSpanish`, que abren el borrador con la lectura completa, `FullEditIntentKit.Admitted("R9").FullRead!.Read`, en lugar de la del piloto) y dos de WPP (`TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying`, con H-02, y `PrepareDoesNotPutTheDraftInConflictNorCleanDirtyNorTakeTheSlotOfTheUpdate`, que abre el borrador con `FullHandKit.FullOf(read)!.Read`, la lectura completa de la misma base); y, de la v4.5, 14 de WPP', 1),
    ('- **34 sin más que M:** 28 de las 29 de R-C4',
     '- **38 sin más que M:** en la v4.6, las cuatro de WPEH que usan el ayudante `Env.OpenUndecidable` (`DiscardRackNeedsNoComparerAndRemovesAnUndecidableDraftToo`, `AComparerThatThrowsMakesTheFieldUndecidableProtectedAndNotEditableAndGetFieldNeverThrows`, `TheRacksAtRiskAreOrderedOrdinallyAndAComparerThatThrowsMakesThemUncertainApart` y `TheFallbackByDraftIsKnownAndKeepsTheUncertainOnesApart`): su texto es el de M, y son a mano por la edición del ayudante; y, de la v4.5, 28 de las 29 de R-C4', 1),
    ('**Los 21 equivalentes de las guardas**',
     '**Las ediciones de ayudante en las copias de C-0** (V46-02; `C0_HELPER_EDITS` de `pbt4b_hand_edits_v46.py`): sobre el texto del ayudante ya con M, en la copia y nunca en el original, cada una las veces que dice en el conjunto de los ayudantes de su archivo; si no casa, el guion para. **Una sola:** `Env.OpenUndecidable` de `WorkspacePilotEditHostFull` abre el borrador con `FullEditIntentKit.Admitted(rack).FullRead!.Read` en lugar de `.PilotRead!.Read`, porque con el comparador completo `DraftRegistry.Begin<TState>` no infiere el tipo (CS0411). Las pruebas que la usan pasan a «a mano»; la revisión lleva el ayudante editado, y CT-PBT4B-20 lo nombra (su punto 4: lo fija el `CopySha`).\n\n**Los 21 equivalentes de las guardas**', 1),
    ('**Su texto literal:** el de `make_copies_pbt4b_v45.py` con `pbt4b_hand_edits_v45.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v45.txt` lleva el texto final de los 81 (60 a mano y 21 de guarda), generado en el modo de medida (§6.2.1).',
     '**Su texto literal:** el de `make_copies_pbt4b_v46.py` con `pbt4b_hand_edits_v46.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v46.txt` lleva el texto final de las 90 entradas (68 a mano, 21 de guarda y el ayudante editado), generado en el modo de medida (§6.2.1).', 1),
    ('y `FullOf` igual al lector (M-05).',
     'y `FullOf` igual al lector (M-05). Las ediciones de ayudante de C-0 las fija el `CopySha` de su archivo.', 1),
]
v = in_section(v, '#### D-PBT4-11.', '#### D-PBT4-21.', D11, 'D-11')

# ---------------------------------------------------------------- D-PBT4-21 (M-08)
v = once(v, '**Las guardas, medidas** (`check_ria_v45.py`,',
         '**El mecanismo, frente a la letra de la decisión** (M-08 de la §19; V46-04; se declara en F4B-E52): la decisión dice «elegir la vista de origen antes '
         'de leer». D-S4 lee el piloto del `Primary` (lo necesita para el testigo y para saber si la lectura vale), elige el origen por texto sobre ese escaneo y '
         'difiere solo el modelo completo hasta saberlo. El resultado es el que pide la decisión: una materialización completa por lectura. El coste residual '
         'del piloto (de 2 a 10 ms en `read-cost`) lo mide la sonda en su línea `parte: solo el piloto`.\n\n**Las guardas, medidas** (`check_ria_v45.py`,', 'D-21 M-08')

# ---------------------------------------------------------------- §5.2: CT-20
v = in_section(v, '### 5.2 PB-T4b', '### 5.3 PB-T4c', [
    ('(3) En las 9 clases de C-0, cada copia mecánica es **el texto de su método en la base**',
     '(3) En las 9 clases de C-0, cada copia mecánica (las 360 del mapa de la copia v3) es **el texto de su método en la base**', 1),
    ('que fija sus equivalentes a mano. (5)', 'que fija sus equivalentes a mano y las ediciones de ayudante de C-0 (`Env.OpenUndecidable` en `WorkspacePilotEditHostFull`, V46-02). (5)', 1),
    ('(9, con los números del mapa de la copia v2)', '(9, con los números del mapa de la copia v3)', 1),
], '§5.2')

# ---------------------------------------------------------------- RR-3 (§6.0)
rows = []
for c in [l.split('\t') for l in open(os.path.join(HERE, 'pbt4b-copy-map-v3.tsv'), encoding='utf-8').read().split('\n')[1:] if l]:
    if c[4] == 'R-D-S1-compila':
        rows.append('| PB-T4b | `%sFull`, `%s` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n'
                    % (c[0][:-len('Tests')], c[1]))
assert len(rows) == 8
rows.append('| PB-T4b | `WorkspacePilotEditHostFull`, el ayudante `Env.OpenUndecidable` (editado en la copia, D-S1, V46-02) | **El texto entero del ayudante, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n')
v = once(v, '| PB-T4b | Las 368 copias mecánicas, las 27 de la interfaz y el kit |', ''.join(rows) + '| PB-T4b | Las 360 copias mecánicas, las 27 de la interfaz y el kit |', 'RR-3')

# ---------------------------------------------------------------- §6.2
T62 = [
    ('### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL` (entero; V44-09; V45-01, V45-02, V45-10)', '### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL` (entero; V44-09; V45-01, V45-02, V45-10; V46-01, V46-02)', 1),
    ('**las 20 copias,** byte a byte las que genera `make_copies_pbt4b_v45.py`', '**las 20 copias,** byte a byte las que genera `make_copies_pbt4b_v46.py`', 1),
    ('apply_red_pbt4b_v45.py', 'apply_red_pbt4b_v46.py', 3),
    ('check_skips_pbt4b_v45.py', 'check_skips_pbt4b_v46.py', 3),
    ('pbt4b_skips_v45.py', 'pbt4b_skips_v46.py', 2),
    ('segun el paquete del contrato de PB-T4 v4.5,', 'segun el paquete del contrato de PB-T4 v4.6,', 1),
    ('(C-0 con sus 9 clases, 368 copias y 60 a mano;', '(C-0 con sus 9 clases, 360 copias y 68 a mano;', 1),
    ('| C-0, copia mecánica (368) |', '| C-0, copia mecánica (360) |', 1),
    ('| C-0, a mano (60) |', '| C-0, a mano (68) |', 1),
    ('(D = 0 en el RED); `check_skips_pbt4b_v46.py` sobre la copia, por su texto;',
     '(D = 0 en el RED); `check_skips_pbt4b_v46.py` sobre la copia, por su texto (en la corrida, `check_skips_copy_v46.py`, que compara la copia con la base); '
     '**la UI, solo con sus filtros** (M-10 de la §19; V46-06): una prueba de UI de fuera de ellos que cayera solo se vería en la CI del RED o del GREEN (C16);', 1),
    ('la sonda de `read-cost` con las dos órdenes, la de PB-T4a y la de PB-T4b.',
     'la sonda de `read-cost` con las dos órdenes, la de PB-T4a y la de PB-T4b. **Lo que mide** (M-09 de la §19; V46-05): el recuento `materializaciones` es el de '
     'su propia réplica del orden de RIA, NH y `AtOrigin`, porque las pruebas no referencian el Plugin; lo de RIA lo fija CT-PBT4B-21, por texto, y la sonda '
     'mide el coste del orden.', 1),
    ('| **`Objective`** | §6.2.1 | ASCII, byte a byte; cita el paquete v4.5,', '| **`Objective`** | §6.2.1 | ASCII, byte a byte; cita el paquete v4.6,', 1),
    ('| **El mapa de la copia** | `pbt4b-copy-map-v2.tsv` | sha256 `3ffadf5b…9e01`; **368** mecánicas y **60** a mano (los motivos de la v4.3 y siete `R-D-S1`) |',
     '| **El mapa de la copia** | `pbt4b-copy-map-v3.tsv` | sha256 `%s…%s`; **360** mecánicas y **68** a mano (los motivos de la v4.3, siete `R-D-S1` y ocho `R-D-S1-compila`) |' % (MAP3[:8], MAP3[-4:]), 1),
    ('`make_copies_pbt4b_v45.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v45.py` y `pbt4b_hand_edits.py`,',
     '`make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v46.py`, `pbt4b_hand_edits_v45.py` y `pbt4b_hand_edits.py`,', 1),
    ('| **La numeración** | — |',
     '| **La precondición 6** | `pbt4bcc/compile/pbt4b-precondition6.json` | Schema `rackcad-pbt4b-precondition6/v1`, Revision (`78a7cef3…`), PartitionSha256, CopySha (las 20), R_C, R_UI y `MinSelected` de las 40 clases de las copias (0: la fila sale); la corrida, en su `compile-check.md` |\n'
     '| **La numeración** | — |', 1),
    ('este paquete (v4.5), con su comprobación acotada de PB-T4b (CA-T4b).', 'este paquete (v4.6), con su comprobación acotada de PB-T4b (CA-T4b), y la §19 de `independent-review-v1.md`.', 1),
]
v = in_section(v, '### 6.2 PB-T4b', '### 6.3 PB-T4c', T62, '§6.2')
s62 = section(v, '### 6.2 PB-T4b', '### 6.3 PB-T4c')
gen_line = [l for l in s62.split('\n') if l.startswith('**El guion de la copia y los equivalentes, literales:**')]
assert len(gen_line) == 1
v = once(v, gen_line[0],
         '**El guion de la copia y los equivalentes, literales:** `make_copies_pbt4b_v46.py` (sha256 `%s`), que usa las piezas de `make_copies_pbt4b.py` de la v4.4 '
         'sin tocarlo (sha256 `%s`), con `pbt4b_hand_edits_v46.py` (sha256 `%s`), que carga `pbt4b_hand_edits_v45.py` (`%s`) y `pbt4b_hand_edits.py` (`%s`), '
         '`copy_map_defs.py`, `lists_core.py`, la lista de 477 y el mapa de la copia v3. En el modo de medida, sobre `78a7cef3`, da 20 archivos '
         '(`v46/copies_probe_v46`; el sha256 del conjunto de las líneas «sha256  ruta», ordenadas, `%s`; `v46/copies_probe_v46-out.txt`), y su revisión, '
         '`pbt4b-equivalents-review-v46.txt` (sha256 `%s`), lleva el texto final de las 90 entradas (68 equivalentes a mano, 21 de guarda y el ayudante editado). '
         'La de la v4.5, `pbt4b-equivalents-review-v45.txt`, queda como historia.'
         % (GEN, sha('make_copies_pbt4b.py'), EDITS, sha('pbt4b_hand_edits_v45.py'), sha('pbt4b_hand_edits.py'), PROBE, REVIEW), 'generador')
blk = section(v, '#### 6.2.1 ', '#### 6.2.2 ')
code = blk[blk.index('```text\n') + 8:blk.index('\n```', blk.index('```text\n'))]
assert all(ord(ch) < 0x80 for ch in code)

# ---------------------------------------------------------------- §6.3, §8.3, §8.5 y §11
v = once(v, 'segun el paquete del contrato de PB-T4 v4.5 y la lista cerrada CT-PBT4C-01..15', 'segun el paquete del contrato de PB-T4 v4.6 y la lista cerrada CT-PBT4C-01..15', '6.3')
v = once(v, '**368 copias mecánicas y 60 equivalentes a mano** (D-S1; la v4.3, 375 y 53; `pbt4b-copy-map-v2.tsv`)',
         '**360 copias mecánicas y 68 equivalentes a mano** (D-S1; la v4.3, 375 y 53; la v4.5, 368 y 60; `pbt4b-copy-map-v3.tsv`)', '8.3')
v = once(v, '**D-S1**, las siete copias mecánicas que no pueden pasar en el GREEN, y las que encuentre la precondición 6, pasan a equivalentes escritos a mano, con su registro y su texto literal, y la lista no cambia por eso;',
         '**D-S1**, las siete copias mecánicas que no pueden pasar en el GREEN, y las que encuentre la precondición 6, pasan a equivalentes escritos a mano, con su '
         'registro y su texto literal, y la lista no cambia por eso; **en la v4.6**, la sesión aplica D-S1 a ocho más, que no compilaban sobre el modelo '
         'completo (V46-02), y decide que el guion añada la directiva de `Draft/` (MU, V46-01) y admita ediciones de ayudante en las copias de C-0, sin volver '
         'al Owner;', '8.5')
p11 = [l for l in v.split('\n') if l.startswith('   - **PB-T4b** (V45-04; los ocho pasos de §6.2.1)')]
assert len(p11) == 1
v = once(v, p11[0], p11[0].replace('apply_red_pbt4b_v45.py', 'apply_red_pbt4b_v46.py').replace('check_skips_pbt4b_v45.py', 'check_skips_pbt4b_v46.py')
         .replace('   - **PB-T4b** (V45-04; los ocho pasos de §6.2.1)', '   - **PB-T4b** (V45-04 y V46-07; los ocho pasos de §6.2.1, con los guiones de la v4.6; la corrida, en `pbt4bcc/compile/compile-check.md`)'), '11.6')
p9 = [l for l in v.split('\n') if l.startswith('9. **Para PB-T4b y PB-T4c:**')]
assert len(p9) == 1
v = once(v, p9[0], p9[0].replace('esta versión (v4.5) y la v4.4, sus fragmentos de `v44/` y `v45/`,', 'esta versión (v4.6), la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/` y `v46/`,')
         .replace('el mapa de la copia v2,', 'los mapas de la copia v2 y v3,')
         .replace('`pbt4b-equivalents-review-v45.txt`,', '`pbt4b-equivalents-review-v45.txt` y `pbt4b-equivalents-review-v46.txt`, la corrida de la precondición 6 (`pbt4bcc/compile/`: `compile-check.md`, `pbt4b-precondition6.json`, sus guiones, sus registros y el sha256 de sus TRX y binarios),'), '11.9')

for k_ in KEEP:
    assert v.count(k_) == 1, 'PB-T4a cambió'
data = v.encode('utf-8')
assert b'\r' not in data
open(OUT, 'wb').write(data)
print('v4.6', hashlib.sha256(data).hexdigest(), len(data), 'bytes')
print('mapa v3', MAP3, '| generador', GEN, '| ediciones', EDITS, '| revision', REVIEW, '| conjunto', PROBE)
print('filas RR-3 de PB-T4b', sum(1 for l in v.split('\n') if l.startswith('| PB-T4b |')))
