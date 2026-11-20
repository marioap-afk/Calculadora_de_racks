# -*- coding: utf-8 -*-
"""Arma la v4.6 del paquete de PB-T3 desde la v4.5 revisada: cada cambio es una sustitucion o una insercion exacta, que tiene que aplicar una sola vez.
Dos tramos, en orden: el borrador (V46-01 a V46-05: CA-T3b M-01 a M-03 y sus avisos, solo de texto, para PB-T3c) y la correccion de PB-T3b (V46-06 a
V46-10: la decision del Owner F4B-E45, "Renombrar los metodos nuevos", con los nombres de la sesion, ExecuteFull y ComparePostFull, y PrepareFull, que
pide el barrido ampliado del section 4.1; la reemision de PB-T3b en el intento 2). Las listas que la v4.5 derivaba del section 5.3 (la tabla de RR-3
de PB-T3b) se sacan del propio texto de la v4.5, y el bloque citado de PBT3x-C11 es el texto de PBT3B-C11 del contrato emitido de PB-T3b (eab27a9e),
leido de su copia por objetos git.
Uso (desde hs65/pbt3): python -I make_v46.py  -> escribe I-64-f4b-pbt3-gate-contract-package-v46.md"""
import hashlib, json, os, re

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v45.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v46.md')
C3B = os.path.join(HERE, '_ref', 'pbt3b-contract-eab27a9e.json')  # git -C <checkout> show eab27a9e:<contrato emitido de PB-T3b>
raw = open(SRC, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '64ab03df27cc8f441883442ca999036e5f7555ede9269eb3f4f768ab8a07fdbd', 'la v4.5 no es la revisada'
assert hashlib.sha256(open(C3B, 'rb').read()).hexdigest().startswith('ae1e7462'), 'el contrato de PB-T3b no es el emitido'
T = raw.decode('utf-8')
assert '\r' not in T
C11_T3B = [s['Text'] for s in json.load(open(C3B, encoding='ascii'))['StopConditions'] if s['Id'] == 'PBT3B-C11'][0]


def sub(old, new):
    global T
    assert T.count(old) == 1, ('no aplica una sola vez', old[:80], T.count(old))
    T = T.replace(old, new)


def after_line(prefix, new_lines):
    """Inserta lineas nuevas tras la unica linea que empieza por prefix."""
    global T
    L = T.split('\n')
    k = [i for i, l in enumerate(L) if l.startswith(prefix)]
    assert len(k) == 1, ('linea', prefix[:60], len(k))
    T = '\n'.join(L[:k[0] + 1] + new_lines + L[k[0] + 1:])


# ---------------------------------------------------------------- las listas de la tabla de RR-3 de PB-T3b, del section 5.3 de la v4.5 (V46-02)
S53 = T[T.index('\n### 5.3 '):T.index('\n### 5.4 ')]
row = {m.group(1): m.group(0) for m in re.finditer(r'(?m)^\| \*\*(\d\d)\*\* \|.*$', S53)}
ex = re.search(r'el exacto en diez filas \(([^)]*)\) y `([^`]+)` en (F\d+)', row['01'])
pairs = sorted(re.findall(r'(F\d+) (\d+\.\d+)', ex.group(1)) + [(ex.group(3), ex.group(2))], key=lambda p: int(p[0][1:]))
FLINES = ['%s prepare=Ready diseno=[%s]' % p for p in pairs]
assert len(FLINES) == 11 and FLINES[3] == 'F4 prepare=Ready diseno=[6]'
CT06 = re.findall(r'`(piloto-bloqueado admitido=True bloqueado=True prepare=Invalid motivo=\[[^`]*\])`', row['06'])
assert len(CT06) == 1
k10 = re.search(r'Las l\S{1,2}neas, literales:', row['10'])
part = row['10'][k10.end():].split('**Se comprueba')[0]
base10 = [x for x in re.findall(r'`([^`]+)`', part) if x.startswith('modulo-anidado ')]
twin = re.search(r'y las dos iguales con `([^`]+)`', part).group(1)
CT10 = base10 + [twin + x[len('modulo-anidado'):] for x in base10]
assert len(CT10) == 4 and twin == 'ladoB-anidado'
q = lambda xs: ', '.join('`%s`' % x for x in xs[:-1]) + ' y `%s`' % xs[-1]

# ---------------------------------------------------------------- la cabecera (V46-05)
sub('# I-64 F4B — Paquete del contrato de PB-T3 (la escritura completa del Workspace), v4.5',
    '# I-64 F4B — Paquete del contrato de PB-T3 (la escritura completa del Workspace), v4.6')
sub('**Estado:** v4.5. Aplica la decisión del Owner **F4B-E42**',
    '**Estado:** v4.6, solo de texto y para PB-T3c (registro v4.5 → v4.6, abajo). Aplica las menores M-01 a M-03 de la comprobación acotada de '
    'PB-T3b (CA-T3b, `F4B-DRAFTS/pbt3/independent-review-pbt3b-check.md`: **EMITIBLE, B 0 / I 0 / M 4**, y su §12, sobre el seco r2: **EMITIBLE, '
    'B 0 / I 0 / M 1**, sin decisiones nuevas del Owner), pone sus dos avisos para el Worker como lección del §5.0 y barre las guardas de PB-T3c sobre '
    'la emisión de PB-T3b. **No cambian el diseño, D-1, ninguna guarda o prueba existente, ni el contrato emitido de PB-T3b**, que sigue sobre la '
    'v4.5: lo que la v4.6 toca de PB-T3b es texto de referencia, declarado en `v46-delta.md`. La v4.5 aplicó la decisión del Owner **F4B-E42**')
sub('Viene de ocho revisiones:', 'Viene de diez revisiones:')
sub('- su recomprobación (§10 de CA-T3a), sobre la v4.4: **EMITIBLE, B 0 / I 0 / M 1** (F4B-E41). Su menor, M-05, era del generador.\n',
    '- su recomprobación (§10 de CA-T3a), sobre la v4.4: **EMITIBLE, B 0 / I 0 / M 1** (F4B-E41). Su menor, M-05, era del generador;\n'
    '- su recomprobación de la v4.5 (§11 de CA-T3a), para la reemisión de PB-T3a: **EMITIBLE, B 0 / I 0 / M 2** (F4B-E42);\n'
    '- **la comprobación acotada de PB-T3b** (CA-T3b, sobre la v4.5 y el contrato en seco de PB-T3b): **EMITIBLE, B 0 / I 0 / M 4**, y su §12, '
    'sobre el seco r2: **EMITIBLE, B 0 / I 0 / M 1** (F4B-E44). La v4.6 aplica M-01, M-02 y M-03; M-04 y M-05 se resolvieron en la emisión de PB-T3b.\n')
old17 = [l for l in T.split('\n') if l.startswith('**Dónde está PB-T3, medido en `18e4b783`:**')]
assert len(old17) == 1
sub(old17[0], '**Dónde está PB-T3, medido en `eab27a9e`:** PB-T1 y PB-T2 están CERRADAS. **PB-T3a está CERRADA** (F4B-E43, custodia `c40eea4a`; '
              'GREEN `6919e17a` en el intento 2 de 3, tras la reemisión con la v4.5 y `ForFullWrite`; CI `44668584248`: Core 19 107 y UI 2 451, con 21 '
              'omitidas). **PB-T3b está EMITIDA** con la v4.5 (F4B-E44; X `05b6f04e`, BASE `eab27a9e`), y su cadena está en curso. **Esta v4.6 es para '
              'PB-T3c**, que se emite tras el cierre de PB-T3b, con su remedición y su comprobación acotada (precondiciones 2, 3, 5 y 10): lo que depende '
              'de ese cierre lleva el marcador `<tras el cierre de PB-T3b>`. La emisión de PB-T3 tiene la autorización condicional del Owner (F4B-E35, '
              'pregunta 3). No declara GATE PASS.')
sub('`v44-delta.md` (de la v4.3 a la v4.4) y `v45-delta.md` (de la v4.4 a la v4.5).',
    '`v44-delta.md` (de la v4.3 a la v4.4), `v45-delta.md` (de la v4.4 a la v4.5) y `v46-delta.md` (de la v4.5 a la v4.6).')
sub('## Registro de cambios v4.4 → v4.5 (la decisión del Owner F4B-E42: renombrar `ForWrite`)',
    '## Registro de cambios v4.5 → v4.6 (la comprobación acotada de PB-T3b, CA-T3b; solo de texto, para PB-T3c)\n\n'
    '**Origen.** CA-T3b (`F4B-DRAFTS/pbt3/independent-review-pbt3b-check.md`, §1 a §12) dio EMITIBLE sin importantes ni decisiones nuevas del Owner, '
    'con cinco menores: M-01 a M-03 piden esta v4.6 antes de PB-T3c; M-04 y M-05 se resolvieron en la emisión de PB-T3b (F4B-E44). **El contrato de '
    'PB-T3b está emitido sobre la v4.5 y no cambia:** donde la v4.6 toca una sección de PB-T3b, lo hace como texto de referencia o en una nota que no '
    'va al contrato. El código se leyó por objetos git en `eab27a9e` (y en `c40eea4a`, el cierre de PB-T3a: `src/` y `tests/` son los mismos).\n\n'
    '| Id | Origen | Qué cambia | Dónde |\n'
    '|---|---|---|---|\n'
    '| **V46-01** | **CA-T3b M-01** (F4B-E40 y F4B-E43, decisiones del Owner ya tomadas) | **El bloque citado de `PBT3x-C11`** es ahora el texto que lleva '
    '`PBT3B-C11` en el contrato emitido de PB-T3b: nc1 con la firma PD-F4B-01 (con PD-F4-04 como caso particular) y su sustituto '
    '`PBT3x-NC1-CASCADE-SIGNAL-01` (SHA-256 `41440edc…`), y nc2 con la v2 de `PBTx-NC2-EXACT-WHITELIST-F4B-01` (SHA-256 `c033e96c…`) y `--task`. En '
    'PB-T3c va con su prefijo. La v4.5 nombraba el sustituto de nc2 v1 (F4B-E34) y solo la firma exacta de PD-F4-04. §1 y §8.1 ganan F4B-E40 y '
    'F4B-E43 | §6.0; §1; §8.1 |\n'
    '| **V46-02** | **CA-T3b M-02** | **La tabla de RR-3, filas de PB-T3b:** las tres entradas que se sacaban del §5.3 van enumeradas, literales: las '
    'once de CT-01 (ii), la de CT-06 (ii) entera y las cuatro de CT-10. **El doble con borrado** (`PilotEraseScopeDouble`, en '
    '`PushBackFullWriteSupport.cs`) gana su fila, con los dos literales que le prescribe el §5.0: `EraseUnit:` y `PreparedBlockMatches`. Son las '
    'mismas 16 líneas que la cadena de PB-T3b ya usa por la derivación de su generador: para PB-T3b es texto de referencia | §6.0 (la tabla de RR-3 y '
    'su nota) |\n'
    '| **V46-03** | **CA-T3b M-03** | **Lo heredado de §6.1, en PB-T3c:** en la `ExpectedEvidence`, las autopruebas de CT-PBT3C-01, 04 y 05 en lugar '
    'de «las tres guardas»; la UI de paridad de PB-T3a (`PushBackFullDraftWindowParityTests`, 1, false) vuelve a sus `RequiredTests`; y el GREEN '
    'previsto en cadena, Core 19 133 o 19 129, según cuente la TRX de PB-T3b la `Theory` S-2. **En PB-T3b, solo como lectura,** en una nota que no va '
    'al contrato | §6.2 (nota); §6.3 |\n'
    '| **V46-04** | **CA-T3b §7** (los dos avisos para el prompt del Worker de PB-T3b) | **La lección, en el §5.0:** `Transaction` en `Pilot/` y los '
    'cinco tipos del modelo fuera de `Draft/`, PP y UR, también en los comentarios, con las dos guardas que los fijan '
    '(`WorkspacePilotUpdateGuardTests.cs:117-124` y `PushBackDraftWriteGuardTests.cs:42-61` en `c40eea4a`) | §5.0 |\n'
    '| V46-05 | La sesión | **La versión y el estado:** el `Objective` de PB-T3c dice «v4.6» (los de PB-T3a y PB-T3b, emitidos con la v4.5, no '
    'cambian); la cabecera y la precondición 1 dicen el estado medido en `eab27a9e`; la precondición 9, la custodia de la emisión de PB-T3c; el '
    '§4.1, el barrido de las guardas de PB-T3c sobre `eab27a9e`, a repetir sobre el cierre de PB-T3b; y los marcadores `<tras el cierre de PB-T3b>` '
    'de lo que depende de ese cierre | Cabecera; precondiciones 1 y 9; §4.1; §6.3 |\n\n'
    '## Registro de cambios v4.4 → v4.5 (la decisión del Owner F4B-E42: renombrar `ForWrite`)')
# ---------------------------------------------------------------- las precondiciones 1 y 9 (V46-05)
after_line('   - **En `18e4b783`** (V45-05):',
           ['   - **En `eab27a9e`** (V46-05): **PB-T3a, CERRADA** (F4B-E43, custodia `c40eea4a`; GREEN `6919e17a`, intento 2 de 3; CI `44668584248`: Core '
            '19 107, UI 2 451, con 21 `Skipped`). **PB-T3b, EMITIDA** con la v4.5 (F4B-E44; X `05b6f04e`, BASE `eab27a9e`): `src/` y `tests/` son los '
            'del cierre de PB-T3a. **La `BaseSha` de PB-T3c es el cierre de PB-T3b:** `<tras el cierre de PB-T3b>`.'])
after_line('   - **el registro de mediciones** `pbt3-measurements.md`, puesto al día por la remedición.',
           ['   - **en la emisión de PB-T3c** (V46-05): el paquete en su versión v4.6, sobre la v4.5 que custodió `5bda5569`, con `v46-delta.md`; su '
            'remedición sobre el cierre de PB-T3b, su comprobación acotada con su encargo y el seco de su generador: `<tras el cierre de PB-T3b>`.'])
# ---------------------------------------------------------------- section 1: F4B-E40 y F4B-E43 (V46-01)
after_line('| **F4B-E34** (`F4B-DRAFTS/coordinator/owner-pbt2a-controls-decisions.md`;',
           ['| **F4B-E40** (`F4B-DRAFTS/coordinator/owner-pbt2c-nc1-decision.md`; el cierre de PB-T2c) | «Aceptar y ampliar la firma (Recomendado)»: la '
            'firma nueva de nc1 PD-F4B-01, la cascada del SHA falso (condiciones (a) a (f)), con PD-F4-04 como caso particular, para el resto de F4B, y su '
            'sustituto `PBT3x-NC1-CASCADE-SIGNAL-01`, revisado APTO (`F4B-DRAFTS/nc1-cascade-substitute/`, SHA-256 `41440edc…`, custodiado en el cierre '
            'de PB-T3a). PBT3x-C11 (V46-01) |',
            '| **F4B-E43** (`F4B-DRAFTS/coordinator/owner-pbt3a-nc2-decision.md`; el cierre de PB-T3a, `c40eea4a`) | «Aceptar y ampliar el sustituto '
            '(Recomendado)»: nc2 de PB-T3a por disposición del Owner, solo para PB-T3a; de PB-T3b en adelante, el sustituto de nc2 admite una lista '
            'cerrada de los literales exactos de cada paquete. La v2, revisada APTO, con sus límites M-V1 a M-V4 (`F4B-DRAFTS/nc2-substitute-f4b-v2/`, '
            'SHA-256 `c033e96c…`, custodiada en la emisión de PB-T3b, `05b6f04e`). PBT3x-C11 (V46-01) |'])
# ---------------------------------------------------------------- section 4.1: el barrido de las guardas de PB-T3c sobre eab27a9e (V46-05)
after_line('**El nombre nuevo, contra los mismos literales:**',
           ['',
            '**El barrido de PB-T3c, otra vez, sobre la emisión de PB-T3b** (`eab27a9e`; V46-05). `guard_sweep.py`, el método de este apartado, con el texto '
            'que el paquete da a WR y a la fachada (D-PBT3-11), sobre todas las clases de `tests/` que leen código, comparado con su salida en `18e4b783` '
            '(`sweep_t3b_diff.py`; la salida, `sweep-pbt3c-eab27a9e.txt`):',
            '- `src/` y `tests/` de `eab27a9e` son los del cierre de PB-T3a (`c40eea4a`). Desde `18e4b783` solo cambian los ocho archivos de `Draft/` y las '
            'seis clases de PB-T3a;',
            '- **una sola guarda nueva lee código: `PushBackDraftWriteGuardTests`, de PB-T3a.** Añade 10 coincidencias con el texto de PB-T3c, todas de '
            'expresiones genéricas de sus reglas de D2 y de pureza (CT-13 y CT-14), que solo leen los ocho archivos de `Draft/`. Su CT-12 barre todo '
            '`src/` y prohíbe fuera de `Draft/`, PP y UR los cinco tipos del modelo: el texto de WR y de la fachada no nombra ninguno (§5.0, V46-04). '
            'Ninguna otra guarda cambia sus coincidencias con WR o la fachada;',
            '- **sin colisión.** Las del §4 y de la tabla de arriba siguen como estaban: la fachada no nombra el piloto (G-9), no contiene `PushBack` '
            '(CT-PBT1B-15) y lleva sus términos de escritura (`OpenMode.ForWrite`, `UpgradeOpen`, `Erase()`) porque G-9 solo los mira en los archivos que '
            'nombran el piloto;',
            '- **las guardas que añade PB-T3b** (CT-PBT3B-13 y CT-PBT3B-14) no existen todavía en `eab27a9e`. Por su diseño (§5.3) leen PP, UR, la interfaz '
            'nueva y WHC, y su CT-13 (ii) barre `src/` buscando `PushBackFullPrepareInput` y las llamadas de la corrida completa, que WR no hace. **El '
            'barrido se repite sobre el cierre de PB-T3b,** con esas clases ya en `tests/`: `<tras el cierre de PB-T3b>`.'])
# ---------------------------------------------------------------- section 5.0: la leccion de los dos avisos (V46-04)
after_line('- **lo que lo hizo falta:** R-6 (`pbt3-measurements.md`) y la fila de G-9 del §4 leyeron G-9',
           ['',
            '**Dos guardas que leen también los comentarios** (V46-04; los dos avisos de CA-T3b, §7, para el prompt del Worker de PB-T3b). Valen para el '
            'texto de cada subtarea:',
            '- **`Transaction` en `Pilot/`:** ningún archivo de `src/RackCad.Application/Systems/PushBack/Pilot/` contiene la palabra `Transaction`, con '
            'mayúscula, ni siquiera en un comentario. Lo fija `NoFileOfThePilotFolderNamesTheWordTransactionNotEvenInAComment` '
            '(`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:117-124` en `c40eea4a`), que busca la expresión `Transaction` en el texto '
            'crudo de cada archivo de la carpeta (y IntegrationGuard:81). En PB-T3b: PP, UR y la interfaz nueva;',
            '- **los cinco tipos del modelo, solo en `Draft/`, PP y UR:** `CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes` '
            '(`tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs:42-61` en `c40eea4a`) busca `PushBackFullDraftState`, `PushBackFullDraftReader`, '
            '`PushBackFullComposition`, `PushBackConflictClassifier` y `PushBackViewDispositions`, como palabra, en el texto crudo de todo archivo de '
            '`src/`, comentarios incluidos, y solo deja nombrarlos a `Draft/`, PP y UR. En PB-T3b, la interfaz nueva no los nombra; en PB-T3c, ni WR ni la '
            'fachada.'])
# ---------------------------------------------------------------- section 6.0: el bloque citado de PBT3x-C11 (V46-01)
sub('- **PBT3x-C11, con el sustituto de nc2 adaptado a F4B** (F4B-E34; V43-09). Es el texto de PBT2C-C11 de PB-T2 con el prefijo cambiado, salvo la '
    'salida de nc2 y la lista de las aceptaciones puntuales. En PB-T3a (y con su prefijo en PB-T3b y PB-T3c):',
    '- **PBT3x-C11, con los sustitutos de las decisiones del Owner** (F4B-E40 y F4B-E43; V46-01, CA-T3b M-01; en la v4.5, el sustituto de nc2 '
    'adaptado a F4B, F4B-E34, V43-09). Es el texto de PBT2C-C11 de PB-T2 con el prefijo cambiado, salvo las salidas de nc1 y nc2, las diferencias '
    'admitidas y la lista de las aceptaciones puntuales. Es el de `PBT3B-C11` del contrato emitido de PB-T3b (F4B-E44); en PB-T3c, con su prefijo '
    '(`PBT3C`, `--task PBT3C` y «PB-T3c»):')
q11 = [l for l in T.split('\n') if l.startswith('  > a negative control (nc1, nc2 or nc3)')]
assert len(q11) == 1
sub(q11[0], '  > ' + C11_T3B)
sub('  - **Nota (no va al contrato).** nc1 no cambia: solo la firma exacta de PD-F4-04 y su sustituto de F4. Las tres aceptaciones puntuales de nc1 '
    '(PB-TC, PB-T2a y PB-T2b) no se extienden, y la de PB-TC2 tampoco. El sustituto de nc2 es el de F4B-E34 y solo vale con la firma exacta de PD-F4-01 '
    '(su «Alcance»): cualquier otra firma vuelve al Owner. El contrato de PB-T2c conserva el sustituto de F4, porque se escribió antes; no es de este '
    'paquete.',
    '  - **Nota (no va al contrato).** nc1: la firma PD-F4B-01 (F4B-E40), con la exacta de PD-F4-04 como caso particular, y su sustituto '
    '`PBT3x-NC1-CASCADE-SIGNAL-01` (`F4B-DRAFTS/nc1-cascade-substitute/`, revisado APTO, con sus límites declarados), con `--task`. nc2: la v2 de '
    '`PBTx-NC2-EXACT-WHITELIST-F4B-01` (F4B-E43; `F4B-DRAFTS/nc2-substitute-f4b-v2/`, revisada APTO, con sus límites M-V1 a M-V4), con `--task` y su '
    'lista cerrada de literales, uno por paquete: la primera entrada de la `ExpectedEvidence` de cada contrato de PB-T3 es la de PB-T3a, que esa lista '
    'lleva. Las aceptaciones puntuales de nc1 (PB-TC, PB-TC2, PB-T2a, PB-T2b y PB-T2c) y la disposición del Owner sobre el nc2 de PB-T3a no se '
    'extienden. El contrato de PB-T3a llevó el bloque de la v4.5 (F4B-E34 y PD-F4-04), con las dos decisiones como autoridades; el de PB-T2c conserva '
    'el sustituto de F4, porque se escribió antes.')
# ---------------------------------------------------------------- section 6.0: la tabla de RR-3, filas de PB-T3b (V46-02)
sub('**La lista cerrada de la ayuda de RR-3, por clase** (V43-06; V44-02 y V44-03, CA-T3a M-02 y M-03).',
    '**La lista cerrada de la ayuda de RR-3, por clase** (V43-06; V44-02 y V44-03, CA-T3a M-02 y M-03; V46-02, CA-T3b M-02, las filas de PB-T3b '
    'enumeradas y el doble).')
sub('las once `<Fn> prepare=Ready diseno=[<valor>]` con los valores de CT-01 (ii) y las dos de estilo, `diseno=STANDARD-ISO sistema=STANDARD-ISO` y '
    '`diseno=SIMPLEX sistema=SIMPLEX`; la línea literal de CT-06 (ii), `piloto-bloqueado admitido=True bloqueado=True prepare=Invalid motivo=[…]`; las '
    'cuatro líneas literales de CT-10 y ` base-igual=True dirty=True`;',
    'las once de CT-01 (ii), ' + q(FLINES) + ', y las dos de estilo, `diseno=STANDARD-ISO sistema=STANDARD-ISO` y `diseno=SIMPLEX sistema=SIMPLEX`; '
    'la de CT-06 (ii), `' + CT06[0] + '`; las cuatro de CT-10, ' + q(CT10) + ', y ` base-igual=True dirty=True`;')
after_line('| PB-T3b | `PushBackPilotUpdateEquivalenceGuardTests` (CT-14) |',
           ['| PB-T3b | `PilotEraseScopeDouble` (el doble con borrado de CT-07 a 09, en `tests/RackCad.Tests/PushBackFullWriteSupport.cs`; §5.0) | '
            '**Líneas:** `EraseUnit:` y `PreparedBlockMatches` |'])
sub('`PushBackFullWriteSupport.cs`, el doble con borrado de PB-T3b, lo añade el RED: una reparación puede tocarlo, y no tiene literales en la tabla.',
    '`PushBackFullWriteSupport.cs`, el doble con borrado de PB-T3b, lo añade el RED: una reparación puede tocarlo, y su fila lleva los dos literales '
    'que le prescribe el §5.0 (V46-02).')
# ---------------------------------------------------------------- section 6.2: la lectura de M-03 para PB-T3b, en una nota (V46-03)
after_line('  - **Nota (no va al contrato).** Con (A′), no elegida, habría sido Core + 18.',
           ['  - **Nota (no va al contrato).** CA-T3b M-03 (V46-03), solo como lectura, porque el contrato de PB-T3b está emitido sobre la v4.5: en PB-T3b, '
            '«las autopruebas de las tres guardas» de §6.1 son las de CT-13 y las de E-2 y E-6; la UI de paridad de PB-T3a '
            '(`PushBackFullDraftWindowParityTests`, 1) no vuelve a sus `RequiredTests`, sin perder red, porque la CI de UI corre la clase entera; y el '
            'GREEN previsto de Core es 19 128 resultados, o 19 124 si la TRX cuenta la `Theory` S-2 como una fila. «O la diferencia explicada por el diff» '
            'lo cubre.'])
# ---------------------------------------------------------------- section 6.3: PB-T3c (V46-03 y V46-05)
sub('segun el paquete del contrato de PB-T3 v4.5 y la lista cerrada CT-PBT3C-01..05', 'segun el paquete del contrato de PB-T3 v4.6 y la lista cerrada CT-PBT3C-01..05')
sub('  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` | los medidos tras PB-T3b | false |',
    '  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` | los medidos tras PB-T3b: `<tras el cierre de PB-T3b>` | false |')
after_line('  | Core | `PushBackSharedReferenceGuardTests` (CT-PBT1B-15: 14 archivos y 7 asignaciones) | 2 | false |',
           ['  | UI | `PushBackFullDraftWindowParityTests` (PB-T3a; V46-03) | 1 | false |'])
after_line('  | | Y los de §6.2 sin `ExpectRed` | | false |',
           ['  - **Nota (no va al contrato).** Los `MinSelected` de lo heredado de §6.2 se miden en la emisión de PB-T3c, sobre el cierre de PB-T3b '
            '(`Workspace` de Core, con los `Skip` de D-1 ya en el código): `<tras el cierre de PB-T3b>`. La UI de paridad de PB-T3a vuelve a la red de '
            'PB-T3c (CA-T3b M-03): PB-T3b no la llevaba por la letra de §6.2.'])
sub('«En PB-T3c, además: `RedSha..RedRepairSha` no toca WR ni la fachada (`RackViewBlockEraser.cs`)» (V44-04);',
    '«En PB-T3c, además: `RedSha..RedRepairSha` no toca WR ni la fachada (`RackViewBlockEraser.cs`)» (V44-04); y, en la tabla «CT → la aserción que '
    'falla en el RED», las autopruebas de CT-PBT3C-01, 04 y 05, en lugar de «las autopruebas de las tres guardas» (V46-03; CA-T3b M-03);')
sub('- **Línea base de la CI:** la del cierre de PB-T3b. **GREEN previsto:** Core + 5; UI sin cambio. En cadena (V42-01): Core **19 133** y UI '
    '**2 451**, previstos.',
    '- **Línea base de la CI:** la del cierre de PB-T3b: `<tras el cierre de PB-T3b>`. **GREEN previsto:** Core + 5; UI sin cambio. En cadena '
    '(V42-01): Core **19 133** resultados, o **19 129** si la TRX de PB-T3b cuenta la `Theory` S-2 como una fila (V46-03; CA-T3b M-03), y UI '
    '**2 451**, previstos.')
# ---------------------------------------------------------------- section 8.1: F4B-E40 y F4B-E43 (V46-01)
after_line('- **F4B-E34** (literal «Adaptar el sustituto (Recomendado)», para nc2) → PBT3x-C11 (V43-09).',
           ['- **F4B-E40** (literal «Aceptar y ampliar la firma (Recomendado)», para nc1: la firma PD-F4B-01 y su sustituto) y **F4B-E43** (literal '
            '«Aceptar y ampliar el sustituto (Recomendado)», para nc2: la v2 con su lista cerrada de literales) → PBT3x-C11 desde PB-T3b (V46-01).'])

# ================================================================ la correccion de PB-T3b (F4B-E45; V46-06 a V46-10)
# ExecuteFull y ComparePostFull los fija la sesion (F4B-E45); PrepareFull lo pide el barrido ampliado (V46-07: WorkspacePilotPrepareTests:2156)
PREPARE_FULL, EXECUTE_FULL, COMPARE_FULL = 'PrepareFull', 'ExecuteFull', 'ComparePostFull'
PF, EF, CF = '`' + PREPARE_FULL + '`', '`' + EXECUTE_FULL + '`', '`' + COMPARE_FULL + '`'


def line_sub(prefix, new):
    """Sustituye entera la unica linea que empieza por prefix."""
    global T
    L = T.split('\n')
    k = [i for i, l in enumerate(L) if l.startswith(prefix)]
    assert len(k) == 1, ('linea', prefix[:60], len(k))
    L[k[0]] = new
    T = '\n'.join(L)


def whole_line(old, new):
    """Sustituye la unica linea que es exactamente old."""
    global T
    L = T.split('\n')
    k = [i for i, l in enumerate(L) if l == old]
    assert len(k) == 1, ('linea entera', old[:60], len(k))
    L[k[0]] = new
    T = '\n'.join(L)


# ---------------------------------------------------------------- la cabecera y el registro de cambios (V46-05 a V46-10)
_est = [l for l in T.split('\n') if l.startswith('**Estado:** v4.6, solo de texto y para PB-T3c')]
assert len(_est) == 1 and _est[0].count('La v4.5 aplicó la decisión del Owner **F4B-E42**') == 1
line_sub('**Estado:** v4.6, solo de texto y para PB-T3c',
         '**Estado:** v4.6 (registro v4.5 → v4.6, abajo): **el paquete de la corrección de PB-T3b, en su intento 2 de 3, y el de PB-T3c.** Aplica la '
         'decisión del Owner **F4B-E45** sobre la parada PBT3B-C02 y PBT3B-C07 del intento 1 de PB-T3b (`F4B-DRAFTS/coordinator/'
         'owner-pbt3b-overload-decision.md`, literal «Renombrar los métodos nuevos (Recomendado)»): los métodos públicos nuevos del modelo completo que '
         'llevaban el nombre de uno existente pasan a tener nombre propio, ' + EF + ' y ' + CF + ' (los fija la sesión) y ' + PF + ' (lo pide el barrido '
         'ampliado), con la misma firma, y el barrido del §4.1 se amplía a la resolución de sobrecargas y a la reflexión. Aplica también las menores M-01 '
         'a M-03 de la comprobación acotada de PB-T3b (CA-T3b, `F4B-DRAFTS/pbt3/independent-review-pbt3b-check.md`: **EMITIBLE, B 0 / I 0 / M 4**, y su '
         '§12, sobre el seco r2: **EMITIBLE, B 0 / I 0 / M 1**, sin decisiones nuevas del Owner), pone sus dos avisos para el Worker como lección del §5.0 '
         'y barre las guardas de PB-T3c sobre la emisión de PB-T3b. **No cambian el diseño, D-1, ni ninguna guarda, prueba o firma existente.** '
         + _est[0][_est[0].index('La v4.5 aplicó la decisión del Owner **F4B-E42**'):])
line_sub('**Dónde está PB-T3, medido en `eab27a9e`:**',
         '**Dónde está PB-T3, medido en `eab27a9e`:** PB-T1 y PB-T2 están CERRADAS. **PB-T3a está CERRADA** (F4B-E43, custodia `c40eea4a`; GREEN '
         '`6919e17a` en el intento 2 de 3, tras la reemisión con la v4.5 y `ForFullWrite`; CI `44668584248`: Core 19 107 y UI 2 451, con 21 omitidas). '
         '**PB-T3b se emitió** con la v4.5 (F4B-E44; X `05b6f04e`, BASE `eab27a9e`), y **su intento 1 paró antes del RED**, sin commits ni push: con el '
         'esqueleto del §6.2 de la v4.5, `RackCad.Tests` no compila (dos CS0121, en `WorkspacePilotUpdateRunTests.cs:669` y `:799`; F4B-E45). **Esta v4.6 '
         'es el paquete de la reemisión de PB-T3b, para la corrección en el intento 2 de 3** (precondición 12), **y el de PB-T3c**, que se emite tras el '
         'cierre de PB-T3b, con su remedición y su comprobación acotada (precondiciones 2, 3, 5 y 10): lo que depende de ese cierre lleva el marcador '
         '`<tras el cierre de PB-T3b>`. La emisión de PB-T3 tiene la autorización condicional del Owner (F4B-E35, pregunta 3). No declara GATE PASS.')
whole_line('## Registro de cambios v4.5 → v4.6 (la comprobación acotada de PB-T3b, CA-T3b; solo de texto, para PB-T3c)',
           '## Registro de cambios v4.5 → v4.6 (la decisión del Owner F4B-E45: renombrar los métodos nuevos; y la comprobación acotada de PB-T3b, CA-T3b)')
line_sub('**Origen.** CA-T3b (`F4B-DRAFTS/pbt3/independent-review-pbt3b-check.md`, §1 a §12) dio EMITIBLE',
         '**Origen.** Dos, en este orden. **(1)** CA-T3b (`F4B-DRAFTS/pbt3/independent-review-pbt3b-check.md`, §1 a §12) dio EMITIBLE sin importantes ni '
         'decisiones nuevas del Owner, con cinco menores: M-01 a M-03 piden esta v4.6; M-04 y M-05 se resolvieron en la emisión de PB-T3b (F4B-E44). El '
         'borrador de la v4.6 (V46-01 a V46-05) era solo de texto, para PB-T3c. **(2)** El Worker `R20261120T055235Z-a45b` de PB-T3b (delegación '
         '`R20261120T054837Z-814c`, intento 1) paró con PBT3B-C02 y PBT3B-C07 antes del commit RED: con el esqueleto del §6.2 de la v4.5, `RackCad.Tests` '
         'no compila. `PushBackPilotUpdateRun.Execute(null!, setup.Request, setup.Port)` (`WorkspacePilotUpdateRunTests.cs:669`) y '
         '`PushBackPilotUpdateRun.ComparePost(plan, null)` (`:799`) son ambiguas (CS0121) entre la sobrecarga nueva del modelo completo y la existente. El '
         'Owner eligió «Renombrar los métodos nuevos (Recomendado)» (F4B-E45; `F4B-DRAFTS/coordinator/owner-pbt3b-overload-decision.md`), y la v4.6 pasa a '
         'ser el paquete de la reemisión de PB-T3b: **lo que su borrador tocaba de PB-T3b como texto de referencia rige ahora el contrato reemitido** '
         '(V46-10). **No cambian el diseño, D-1, ni ninguna guarda, prueba o firma existente.** El código se leyó por objetos git en `eab27a9e` (`src/` y '
         '`tests/` son los de `c40eea4a`, el cierre de PB-T3a).')
sub('Son las mismas 16 líneas que la cadena de PB-T3b ya usa por la derivación de su generador: para PB-T3b es texto de referencia | §6.0 (la tabla de '
    'RR-3 y su nota) |',
    'Son las mismas 16 líneas que la cadena de PB-T3b usaba por la derivación de su generador; desde F4B-E45 rigen la reemisión de PB-T3b (V46-10) | '
    '§6.0 (la tabla de RR-3 y su nota) |')
sub('**En PB-T3b, solo como lectura,** en una nota que no va al contrato | §6.2 (nota); §6.3 |',
    '**En PB-T3b, solo como lectura,** en una nota que no va al contrato, también en la reemisión (V46-10) | §6.2 (nota); §6.3 |')
line_sub('| V46-05 | La sesión |',
         '| V46-05 | La sesión | **La versión y el estado:** el `Objective` de PB-T3b y el de PB-T3c dicen «v4.6» (el de PB-T3a, cerrada, no cambia); la '
         'cabecera y la precondición 1 dicen el estado medido en `eab27a9e`; la precondición 9, la custodia de la reemisión de PB-T3b y la de la emisión '
         'de PB-T3c; el §4.1, el barrido de las guardas de PB-T3c sobre `eab27a9e`, a repetir sobre el cierre de PB-T3b; y los marcadores `<tras el cierre '
         'de PB-T3b>` de lo que depende de ese cierre | Cabecera; precondiciones 1 y 9; §4.1; §6.2; §6.3 |')
after_line('| V46-05 | La sesión |', [
    '| **V46-06** | **F4B-E45 (Owner)** | **Los nombres nuevos:** la sobrecarga completa de `Execute` pasa a ' + EF + ' y la de `ComparePost`, a ' + CF +
    ' (los fija la sesión), con la misma firma; y la de `Prepare`, a ' + PF + ' (V46-07). Cambian D-PBT3-09 (' + PF + ') y D-PBT3-10 (el API que se añade, '
    'los delegados en `Execute` y ' + EF + ', el patrón de M-01 y `ComparePostCore`); D-PBT3-13 (CT-10); CT-PBT3B-01, 07, 09, 10, 12, 13 y 15 y E-5 (§5.3); '
    'la fila de CT-13 de la tabla de RR-3 (`PushBackPilotUpdateRun.' + EXECUTE_FULL + '`); el esqueleto del RED y la lista cerrada de PBT3B-C13 (§6.2).<br>'
    '**No cambian:** ninguna firma existente; `Precheck`, que sigue como sobrecarga (V46-08); el `Prepare`, el `Execute` y el `ComparePost` del piloto; '
    'y los registros de (A′) y de D-1 (D-PBT3-09b y §8.2), que llevan una nota | D-PBT3-09; D-PBT3-09b; D-PBT3-10; D-PBT3-13; §5.3; §6.0; §6.2 |',
    '| **V46-07** | **F4B-E45 (Owner)** | **El barrido ampliado del §4.1:** la resolución de sobrecargas y la reflexión, sobre `eab27a9e` '
    '(`overload_sweep.py`, con su salida `overload-sweep-eab27a9e.txt`), y los nombres nuevos contra los literales de las guardas (`name_check_v46.py`, '
    'con `name-check-v46-eab27a9e.txt`). **El control positivo:** con los nombres de la v4.5 encuentra exactamente las dos llamadas del CS0121. **Y un '
    'tercer choque, por reflexión:** `WorkspacePilotPrepareTests.ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing` '
    '(`:2155-2160`) exige con `Assert.Single` un solo método público estático `Prepare` en `PushBackPilotUpdatePlanner`. La sobrecarga completa de '
    '`Prepare` lo haría fallar al ejecutar, y el compilador no lo ve: por eso pasa a ' + PF + '. Con los tres nombres: ninguna llamada ambigua, ningún '
    'grupo de métodos, ninguna reflexión rota y ningún literal de guarda que prohíba los nombres | §4.1 |',
    '| **V46-08** | La sesión (la pregunta de F4B-E45 por `Precheck`) | **`Precheck` sigue como sobrecarga.** El barrido no encuentra ningún uso que '
    'cambie: sus cinco llamadas (WHC:864 y `WorkspacePilotUpdateRunTests.cs:34`, `:42`, `:51` y `:58`) pasan un borrador tipado del piloto; ninguna '
    'pasa `null` ni `default`; no hay grupos de métodos ni reflexión sobre `PushBackPilotUpdateRun`; y la guarda que cuenta '
    '`PushBackPilotUpdateRun.Precheck(` lee el cuerpo de `TryUpdatePilot` en WHC, que PB-T3b no toca. Renombrarla cambiaría CT-13, CT-15 y la fila '
    'de CT-13 de la tabla de RR-3 sin necesidad, y la decisión del Owner nombra los métodos que chocan. Lo que queda: una llamada nueva con `null` o '
    '`default` sin tipo en las pruebas de PB-T3b no compila, y se arregla en el RED, en un archivo del alcance (§5.0) | D-PBT3-10; §5.0 |',
    '| **V46-09** | **F4B-E45 (Owner)** | **La lección, en el §5.0:** una sobrecarga nueva con el nombre de un método existente rompe sin cambiar '
    'ninguna firma: un `null` o un `default` sin tipo (CS0121), un grupo de métodos, o una reflexión por nombre, que solo falla al ejecutar. Antes de '
    'fijar un esqueleto: el barrido de §4.1 y la compilación del esqueleto sobre una copia exportada de la base, con las pruebas que hacen reflexión '
    'sobre los tipos tocados. Y las propiedades nuevas de los tipos del plan, de solo lectura '
    '(`ThePlanIsImmutableAndItsEqualityIsExplicitOrdinalElementByElementAndInOrder`, `WorkspacePilotPrepareTests.cs:1958-1962`) | §5.0 |',
    '| **V46-10** | **F4B-E45 (Owner)** | **La reemisión de PB-T3b en el intento 2 de 3, como corrección,** con el análisis de la sesión: '
    '`FailureClass` `StopCondition` (PBT3B-C02 y PBT3B-C07), `ChainRedSha` null y `ChainRedFiles` vacío, porque no hubo RED; `BaseSha` y '
    '`ChainBaseSha`, el commit BASE de la reemisión. La delegación nueva puede partir de `blocked-skeleton.patch` del intento 1, con los tres nombres '
    'cambiados. La precondición 12 es nueva.<br>**Lo que el borrador tocaba de PB-T3b como referencia, revisado para el contrato reemitido:** la tabla '
    'de RR-3 (V46-02), con sus filas enumeradas, la del doble y el literal nuevo de CT-13, rige la reparación del RED; la nota de M-03 (V46-03) sigue '
    'fuera del contrato y sigue siendo cierta, porque la reemisión no cambia la `ExpectedEvidence`, los `RequiredTests` ni la línea base | §6.0; §6.2; '
    'precondiciones 1, 9 y 12; §8.1 |'])
# ---------------------------------------------------------------- D-PBT3-01, las precondiciones 1, 9 y 12, y la identidad del section 6.0 (V46-10)
sub('(OB-10.1; PB-T3a va en el intento 2 desde F4B-E42, V45-06)', '(OB-10.1; PB-T3a va en el intento 2 desde F4B-E42, V45-06, y PB-T3b desde F4B-E45, V46-10)')
sub('**La `BaseSha` de PB-T3c es el cierre de PB-T3b:** `<tras el cierre de PB-T3b>`.',
    '**Su intento 1 paró antes del RED, sin commits** (F4B-E45): la reemisión de PB-T3b va sobre `eab27a9e` (precondición 12). **La `BaseSha` de '
    'PB-T3c es el cierre de PB-T3b:** `<tras el cierre de PB-T3b>`.')
after_line('   - **en la emisión de PB-T3c** (V46-05):',
           ['   - **en la reemisión de PB-T3b** (F4B-E45; V46-10): el paquete en su versión v4.6, sobre la v4.5, con `v46-delta.md`, `make_v46.py`, '
            '`check_v46.py` y su salida; el barrido ampliado del §4.1 (`overload_sweep.py`, `name_check_v46.py` y sus salidas) y R-22 en el registro de '
            'mediciones; la decisión del Owner, `F4B-DRAFTS/coordinator/owner-pbt3b-overload-decision.md`; el análisis de la sesión del intento 1; la '
            'carpeta del Worker del intento 1, con su manifiesto; la compilación de comprobación del esqueleto corregido; la recomprobación de la v4.6 de '
            'CA-T3b, con su encargo; y el seco de la reemisión.'])
after_line('    La decisión ya está tomada, así que la reemisión no vuelve al Owner salvo por lo que dice la precondición 10.', [
    '12. **La reemisión de PB-T3b con la v4.6** (F4B-E45; V46-10). Antes de reemitir:',
    '    - **el barrido ampliado del §4.1** sobre la `BaseSha` (V46-07): ninguna llamada ambigua, ningún grupo de métodos, ninguna reflexión por nombre '
    'rota y ningún literal de guarda que prohíba los nombres nuevos;',
    '    - **la compilación de comprobación** del esqueleto del §6.2 de la v4.6 sobre una copia exportada de la base, fuera del worktree: Core, las '
    'pruebas de Core, UI, las pruebas de UI y el Plugin, sin errores. Y, porque una reflexión rota solo se ve al ejecutar, `WorkspacePilotPrepareTests` '
    'sobre esa copia, en verde (V46-07);',
    '    - **la comprobación acotada de la v4.6** (la recomprobación de CA-T3b) da 0 bloqueantes y 0 importantes sin resolver. Mira los tres nombres, '
    'que no queda ninguna sobrecarga nueva de un método público existente salvo `Precheck`, y el barrido;',
    '    - **la remedición** sobre la `BaseSha` nueva: `git diff --name-only eab27a9e <BaseSha> -- src tests` vacío;',
    '    - **las autoridades:** F4B-E45 y el análisis entran en la `AuthorityRevision` nueva, la del commit que custodia la v4.6.',
    '    ',
    '    La decisión ya está tomada, así que la reemisión no vuelve al Owner salvo por lo que dice la precondición 10.'])
sub('**PB-T3a, en su reemisión, va en el intento 2 de 3** (F4B-E42; V45-06);',
    '**PB-T3a y PB-T3b, en su reemisión, van en el intento 2 de 3** (F4B-E42 y V45-06; F4B-E45 y V46-10);')
after_line('- **La reemisión:** con la precondición 11 y el mismo `TaskId`.', [
    '',
    '**La reemisión de PB-T3b en el intento 2** (F4B-E45; V46-10). La misma forma que la de PB-T3a: el contrato de `F4B-PBT3B-FULL-WRITE-RUN` se emite '
    'otra vez con la v4.6, y la delegación nueva corrige el intento 1.',
    '- **El análisis de la sesión, sobre el intento 1:** el Worker `R20261120T055235Z-a45b` (delegación `R20261120T054837Z-814c`) paró con **PBT3B-C02 '
    'y PBT3B-C07 antes del commit RED**, sin commits ni push; HEAD sigue en `eab27a9e` y el árbol, limpio. Con el esqueleto exacto del §6.2 de la v4.5, '
    '`RackCad.Tests` no compila: dos CS0121 en `WorkspacePilotUpdateRunTests.cs:669` y `:799`, una prueba de F4 que el contrato prohíbe editar. **Es un '
    'defecto del paquete, no del Worker:** las firmas existentes no cambian, pero una sobrecarga nueva con un parámetro de tipo referencia vuelve '
    'ambiguo un `null`, y el barrido del §4.1 de la v4.5 solo miraba el texto. Las líneas base en `BaseSha`: Core, UI y Plugin compilan en Release con '
    '0 errores, y el filtro `Workspace` da 3 482 de 3 482.',
    '- **La clasificación (16.8):** `FailureClass` `StopCondition`. `ChainRedSha` null y `ChainRedFiles` vacío, porque no hubo RED. **`BaseSha` y '
    '`ChainBaseSha`: el commit BASE de la reemisión**, como en PB-T3a (V45-06); en `src/` y `tests/` es igual a `eab27a9e`, y toda medición de la cadena '
    'se hace con `-- src tests`. Es **el intento 2 de 3** (`attempts` 2); un intento 3 vuelve al Owner (C12; D-PB-10).',
    '- **La corrección, C-1:** los nombres de V46-06, y nada más: ' + EF + ', ' + CF + ' y ' + PF + ', con la misma firma. **Ninguna firma ni prueba '
    'existente cambia.**',
    '- **El RED del intento 2 es el RED entero del contrato** (§6.2): el esqueleto de PP, UR y la interfaz, las cinco clases nuevas de `tests/` y los '
    'seis `Skip`. El criterio es el de siempre (INV-PBT3B-02).',
    '- **El esqueleto del intento 1** (`blocked-skeleton.patch`): la delegación nueva puede partir de él **con los tres nombres cambiados** (sus '
    'declaraciones y sus usos en las pruebas nuevas). Es un punto de partida, no una autoridad: se escribió contra la v4.5, y lo que difiera de la v4.6 '
    'se rige por el contrato reemitido. La sesión lo cita con su ruta y su sha256.',
    '- **La reemisión:** con la precondición 12 y el mismo `TaskId`. Lleva una `AuthorityRevision` nueva (el commit que custodia la v4.6 y F4B-E45) y '
    'una `BaseSha` nueva (el de la reemisión). La planificación se repite con el procedimiento de F4B-E21. `UNIT_CHANGE` sigue siendo PP y UR '
    '(F4B-E25).'])
# ---------------------------------------------------------------- D-PBT3-09, D-PBT3-09b, D-PBT3-10 y D-PBT3-13 (V46-06 y V46-08)
sub('**Ninguna firma pública o interna existente cambia**, porque las pruebas de F4 compilan contra ellas; solo se añade.',
    '**Ninguna firma pública o interna existente cambia**, porque las pruebas de F4 compilan contra ellas; solo se añade. **Y ningún método público '
    'nuevo lleva el nombre de uno existente** (F4B-E45; V46-06 y V46-09): una sobrecarga cambia lo que encuentran el compilador y la reflexión de las '
    'pruebas de F4.')
sub('**El planificador** gana `public static PushBackPilotPrepareResult Prepare(PushBackFullPrepareInput input, PushBackFullIntentResult reread, '
    'RackCatalog catalog)`:',
    '**El planificador** gana `public static PushBackPilotPrepareResult ' + PREPARE_FULL + '(PushBackFullPrepareInput input, PushBackFullIntentResult '
    'reread, RackCatalog catalog)` (V46-06 y V46-07: con el nombre `Prepare` de la v4.5, una sobrecarga, `WorkspacePilotPrepareTests.cs:2156` falla al '
    'ejecutar):')
after_line('| Las declaraciones nuevas de PBT3B-C13 |', [
    '',
    '**Con los nombres de F4B-E45** (V46-06; nota sobre el registro, que no se reescribe): lo que esta tabla y la de D-1 del §8.2 llaman «las dos '
    '`Execute`» son ahora `Execute` y ' + EF + '; lo que PB-T4 quita del piloto con (A) es su `Execute` y su `Precheck`; y la segunda `Prepare` de la '
    'lista de (A′) sería ' + PF + '.'])
sub('**Se añade:**\n\n```csharp\npublic static PushBackPilotPrecheck Precheck(DraftSession<PushBackFullDraftState> draft);\n'
    'public static DraftApplyOutcome<PushBackFullDraftState> Execute(\n',
    '**Se añade** (con los nombres de F4B-E45, V46-06: la v4.5 los llamaba `Execute` y `ComparePost`, y sus `null` chocaban con los del piloto):\n\n'
    '```csharp\npublic static PushBackPilotPrecheck Precheck(DraftSession<PushBackFullDraftState> draft);\n'
    'public static DraftApplyOutcome<PushBackFullDraftState> ' + EXECUTE_FULL + '(\n')
sub('public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead);',
    'public static string? ' + COMPARE_FULL + '(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead);')
sub('PushBackPilotPrepareResult>`, la entrada y el `Prepare` del modelo;',
    'PushBackPilotPrepareResult>`, la entrada y la planificación del modelo (`Prepare` en el piloto, ' + PF + ' en el completo);')
sub('- los tres delegados son **lambdas dentro de cada `Execute`**, sin miembros con nombre;',
    '- los tres delegados son **lambdas dentro de `Execute` y de ' + EF + '**, sin miembros con nombre;')
sub('- las dos `Execute` conservan el patrón de M-01:', '- `Execute` y ' + EF + ' conservan el patrón de M-01:')
sub('- `ComparePost` (las dos sobrecargas) delega en un único privado nuevo,', '- `ComparePost` y ' + CF + ' delegan en un único privado nuevo,')
after_line('**Ningún otro miembro nuevo** en UR, ni ningún ayudante privado (INV-13).', [
    '',
    '**`Precheck` sigue siendo una sobrecarga** (V46-08): el barrido del §4.1 no encuentra ningún uso suyo que cambie (cinco llamadas, todas con un '
    'borrador tipado del piloto; ni grupos de métodos ni reflexión sobre `PushBackPilotUpdateRun`), y la decisión del Owner (F4B-E45) nombra los métodos '
    'que chocan. Las pruebas nuevas lo llaman con un borrador completo tipado, nunca con `null` o `default` sin tipo (§5.0).'])
whole_line('**`ComparePost` genérico:**', '**`ComparePost` genérico** (`ComparePostCore`, detrás de `ComparePost` y ' + CF + '):')
sub('el `Prepare` completo da `NotAdmitted`', PF + ' da `NotAdmitted`')
# ---------------------------------------------------------------- section 5.3: las CT y E-5 (V46-06)
for _a, _b in [('dos `Prepare` iguales dan planes iguales.', 'dos ' + PF + ' iguales dan planes iguales.'),
               ('para que esté en Dirty sin tocar el campo; `Prepare` → `Ready`.', 'para que esté en Dirty sin tocar el campo; ' + PF + ' → `Ready`.'),
               ('y un segundo `Prepare` sobre esa base da `Ready` con `OriginKey` = esa clave.', 'y un segundo ' + PF + ' sobre esa base da `Ready` con '
                                                                                                    '`OriginKey` = esa clave.'),
               ('| D-PBT3-09 (h); D-PBT3-10; UR:250-289, :481 | `Execute` completo da `NotAppliedFailure` |',
                '| D-PBT3-09 (h); D-PBT3-10; UR:250-289, :481 | ' + EF + ' da `NotAppliedFailure` |'),
               ('| **09** | **`ComparePost` genérico:** la clave borrada sigue', '| **09** | **`ComparePost` genérico** (' + CF + '; el piloto, con '
                                                                                  '`ComparePost`): la clave borrada sigue'),
               ('| UR:163-183 | la sobrecarga completa da `PostRereadMissingReason` |', '| UR:163-183 | ' + CF + ' da `PostRereadMissingReason` |'),
               ('(4) el `Prepare` completo con la relectura', '(4) ' + PF + ' con la relectura'),
               ('y el borrador está en Dirty justo antes del `Prepare` (la línea', 'y el borrador está en Dirty justo antes de ' + PF + ' (la línea'),
               ('| el esqueleto da `Invalid` en el `Prepare` y `NotAppliedFailure` en la corrida |',
                '| el esqueleto da `Invalid` en ' + PF + ' y `NotAppliedFailure` en la corrida |'),
               ('iguales antes y después de un `Prepare` y de una corrida. **Las referencias se toman antes del `Prepare`,**',
                'iguales antes y después de un ' + PF + ' y de una corrida (' + EF + '). **Las referencias se toman antes de ' + PF + ',**'),
               ('ni llama a `PushBackPilotUpdateRun.Execute` o `Precheck` con un `DraftSession<PushBackFullDraftState>`',
                'ni llama a `PushBackPilotUpdateRun.' + EXECUTE_FULL + '`, ni a `Precheck` con un `DraftSession<PushBackFullDraftState>`'),
               ('| **15** | **El `Precheck` y el orden del `Prepare` completos** (V2-16):', '| **15** | **El `Precheck` completo y el orden de ' + PF +
                '** (V2-16):'),
               ('El `Prepare` completo: con el borrador Clean', PF + ': con el borrador Clean'),
               ('el esqueleto de `Precheck` da `Go()` y el del `Prepare`, `Invalid` |', 'el esqueleto de `Precheck` da `Go()` y el de ' + PF + ', `Invalid` |'),
               ('Las mismas reglas de `RunEscapeViolations` sobre **las dos** `Execute`, la del piloto y la completa, **que tienen que existir las dos**',
                'Las mismas reglas de `RunEscapeViolations` sobre `Execute`, la del piloto, y ' + EF + ', la completa, **que tienen que existir las dos**')]:
    sub(_a, _b)
# ---------------------------------------------------------------- section 6.0: la fila de CT-13 de la tabla de RR-3 (V46-06)
sub('| PB-T3b | `PushBackWritePathGuardTests` (CT-13) | **Líneas:** `PushBackFullPrepareInput`, `PushBackPilotUpdateRun.Execute`, `Precheck`,',
    '| PB-T3b | `PushBackWritePathGuardTests` (CT-13) | **Líneas:** `PushBackFullPrepareInput`, `PushBackPilotUpdateRun.' + EXECUTE_FULL + '`, `Precheck`,')
# ---------------------------------------------------------------- section 6.2: el Objective, el esqueleto, PBT3B-C13 y la nota de M-03 (V46-05, V46-06 y V46-10)
sub('segun el paquete del contrato de PB-T3 v4.5, la decision del Owner D-1 (F4B-E29, un solo codigo) y la lista cerrada CT-PBT3B-01..15',
    'segun el paquete del contrato de PB-T3 v4.6, la decision del Owner D-1 (F4B-E29, un solo codigo) y la lista cerrada CT-PBT3B-01..15')
sub('la sobrecarga completa de `Prepare` devuelve `WithInvalid(MissingInputReason)`', PF + ' devuelve `WithInvalid(MissingInputReason)`')
sub('`Execute` completo devuelve `NotAppliedFailure(UpdateNotRunReason)`; la sobrecarga completa de `ComparePost` devuelve `PostRereadMissingReason`',
    EF + ' devuelve `NotAppliedFailure(UpdateNotRunReason)`; ' + CF + ' devuelve `PostRereadMissingReason`')
sub('in `PushBackPilotUpdatePlanner`, a second `Prepare` (the one that takes a `PushBackFullPrepareInput`), `BuildCore`,',
    'in `PushBackPilotUpdatePlanner`, ' + PF + ', `BuildCore`,')
sub('in UR: a second `Precheck`, a second `Execute` and a second `ComparePost` (the ones that take the full model), `ComparePostCore`',
    'in UR: a second `Precheck` (the one that takes the full model), ' + EF + ', ' + CF + ', `ComparePostCore`')
line_sub('  - **Nota (no va al contrato).** CA-T3b M-03 (V46-03), solo como lectura,',
         '  - **Nota (no va al contrato).** CA-T3b M-03 (V46-03), como lectura también en la reemisión (V46-10): el contrato reemitido conserva la '
         '`ExpectedEvidence` y los `RequiredTests` del emitido, que CA-T3b dio por fieles. En PB-T3b, «las autopruebas de las tres guardas» de §6.1 son '
         'las de CT-13 y las de E-2 y E-6; la UI de paridad de PB-T3a (`PushBackFullDraftWindowParityTests`, 1) no vuelve a sus `RequiredTests`, sin '
         'perder red, porque la CI de UI corre la clase entera; y el GREEN previsto de Core es 19 128 resultados, o 19 124 si la TRX cuenta la `Theory` '
         'S-2 como una fila. «O la diferencia explicada por el diff» lo cubre. La línea base no cambia: el intento 1 no dejó commits, y `src/` y `tests/` '
         'de la BASE de la reemisión son los de `eab27a9e`, los del cierre de PB-T3a.')
# ---------------------------------------------------------------- section 4.1: el barrido de la resolucion de sobrecargas y de la reflexion (V46-07)
after_line('- **las guardas que añade PB-T3b** (CT-PBT3B-13 y CT-PBT3B-14) no existen todavía', [
    '',
    '**El barrido de la resolución de sobrecargas y de la reflexión** (F4B-E45; V46-07). La parada del intento 1 de PB-T3b mostró que el barrido de '
    'arriba, sobre el texto, no ve lo que decide el compilador. `overload_sweep.py`, por objetos git en `eab27a9e` (los 1 796 archivos C# de `src/` y '
    '`tests/`; la salida, `overload-sweep-eab27a9e.txt`), toma cada método o constructor existente que recibe en PB-T3b o en PB-T3c una sobrecarga nueva '
    'o una firma cambiada, y lista cada uso con sus argumentos: un `null` o un `default` sin tipo donde los tipos difieren, un grupo de métodos, un '
    'literal que es el nombre exacto (la reflexión o una guarda de texto), y la reflexión sobre los tipos tocados (`typeof(T)` con `GetMethods`, '
    '`GetProperties` y los demás). También comprueba que los tipos viejos y nuevos no se convierten entre sí (todos `sealed`, sin `implicit operator`) '
    'y que los nombres nuevos están libres en sus archivos.',
    '- **El control positivo:** con los nombres de la v4.5, el barrido encuentra exactamente las dos llamadas del CS0121, '
    '`WorkspacePilotUpdateRunTests.cs:669` (`Execute(null!, …)`) y `:799` (`ComparePost(plan, null)`).',
    '- **Un tercer choque, que el compilador no ve:** '
    '`WorkspacePilotPrepareTests.ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing` (`:2155-2160`) toma los '
    'métodos públicos estáticos de `PushBackPilotUpdatePlanner` y exige `Assert.Single(members, m => m.Name == "Prepare")`, con la firma del piloto. '
    'La sobrecarga completa de `Prepare` de la v4.5 la habría hecho fallar en el RED: una prueba de comportamiento de F4 (PBT3B-C14). **Por eso pasa a '
    + PF + '** (V46-06).',
    '- **Con los nombres de la v4.6:**',
    '  - `Precheck` (cinco usos: WHC:864 y `WorkspacePilotUpdateRunTests.cs:34`, `:42`, `:51` y `:58`): todos con un borrador tipado del piloto; '
    'ningún grupo de métodos; ninguna reflexión sobre `PushBackPilotUpdateRun`;',
    '  - los constructores internos de `PushBackPilotUpdatePlan` (cuatro usos, de 7 argumentos) y de `PushBackPilotPostExpectation` (cuatro, de 2): '
    'la sobrecarga nueva tiene otra aridad, y ninguno lleva parámetros opcionales;',
    '  - el constructor de `RackPilotMutationScope`, que en PB-T3c gana un parámetro: un solo uso, en WR:156;',
    '  - la reflexión sobre los tipos tocados: la de `:2155` y la de `ThePlanIsImmutableAndItsEqualityIsExplicitOrdinalElementByElementAndInOrder` '
    '(`:1958-1962`), que exige propiedades sin `set` y campos `readonly` (§5.0, V46-09);',
    '  - los nombres nuevos de PBT3B-C13 y PBT3C-C13: libres en PP, UR y WR (`BuildCore` existe en `DynamicSystemLateralBuilder`, otro tipo);',
    '  - **ninguna llamada ambigua, ningún grupo de métodos y ninguna reflexión rota.**',
    '- **Los nombres nuevos contra los literales de las guardas:** `name_check_v46.py`, el método de `name_check.py` con un nombre neutro de control '
    '(la salida, `name-check-v46-eab27a9e.txt`; 165 archivos de `tests/` que leen código y 22 548 literales). Un literal que coincide con el nombre y no '
    'con el neutro es propio del nombre: 8 filas para ' + EF + ', 3 para ' + CF + ' y 1 para ' + PF + '. Las de ' + EF + ' son el prefijo '
    '`PushBackPilotUpdateRun\\.Execute` en las reglas de los adaptadores del anfitrión (IntegrationGuard:57 y :175) y en la de `TryUpdatePilot` de WHC '
    '(`WorkspaceEditIntentGuardTests:1088`), la lista de verbos de `IWorkspaceHostPort` (`WorkspaceHostPortGuardTests:26`), el `"Execute"` de un archivo '
    'del Plugin (`CustomPropertiesEnvelopeGuardTests:199`) y un fragmento de una prueba de análisis estructural; ' + EF + ' no se llama fuera de UR '
    '(CT-13 (ii)). Las de ' + CF + ' son las mismas que las del nombre viejo, y la de ' + PF + ', una clase de caracteres de '
    '`CustomPropertiesCommandGuardTests:131`, que lee los comandos. **Ninguna lee PP ni UR: sin colisión.**'])
# ---------------------------------------------------------------- section 5.0: la leccion (V46-09)
after_line('- **los cinco tipos del modelo, solo en `Draft/`, PP y UR:**', [
    '',
    '**Una sobrecarga nueva rompe sin cambiar ninguna firma** (V46-09; F4B-E45, la parada del intento 1 de PB-T3b). Las firmas existentes se conservan, '
    'y aun así una sobrecarga nueva con el nombre de un método existente cambia lo que encuentran:',
    '- **el compilador:** un `null`, `null!`, `default` o `default!` en una posición donde los tipos de las dos sobrecargas difieren es ambiguo '
    '(CS0121). Así paró PB-T3b: `WorkspacePilotUpdateRunTests.cs:669` y `:799`;',
    '- **la reflexión, al ejecutar:** `GetMethod("X")` lanza `AmbiguousMatchException`, y `Assert.Single(… m.Name == "X")` falla. El compilador no lo ve: '
    'así habría caído `WorkspacePilotPrepareTests.cs:2156` con un segundo `Prepare`;',
    '- **un grupo de métodos:** una conversión a delegado elige por el tipo del delegado, y un `var` con dos candidatos no compila.',
    '',
    '**Por eso los métodos públicos nuevos del modelo completo llevan nombre propio** (' + PF + ', ' + EF + ' y ' + CF + '; V46-06), salvo `Precheck` '
    '(V46-08). Antes de fijar un esqueleto: el barrido de §4.1 (`overload_sweep.py`: las llamadas con `null` o `default`, los grupos de métodos y la '
    'reflexión sobre los tipos tocados) y la compilación del esqueleto sobre una copia exportada de la base, con las pruebas que hacen reflexión sobre '
    'esos tipos. **Las pruebas nuevas** llaman al `Precheck` completo con un borrador tipado, nunca con `null` o `default` sin tipo: no compilaría, y se '
    'arregla en el RED. **Las propiedades nuevas** de los tipos del plan (`Erasures`, `PostOriginKey` y `ErasedKeys`) son de solo lectura, `{ get; }`, '
    'sin `set` ni `init`: `ThePlanIsImmutableAndItsEqualityIsExplicitOrdinalElementByElementAndInOrder` (`WorkspacePilotPrepareTests.cs:1958-1962`) '
    'exige que ninguna propiedad tenga `SetMethod` y que todo campo de instancia sea `readonly`.'])
# ---------------------------------------------------------------- section 8.1: F4B-E45 (V46-06 a V46-10)
after_line('- **F4B-E42, la parada PBT3A-C02 del intento 1 de PB-T3a**', [
    '- **F4B-E45, la parada PBT3B-C02 y PBT3B-C07 del intento 1 de PB-T3b** (2026-11-20, literal «Renombrar los métodos nuevos (Recomendado)»; '
    '`F4B-DRAFTS/coordinator/owner-pbt3b-overload-decision.md`): las sobrecargas nuevas del modelo completo pasan a llamarse ' + EF + ' y ' + CF +
    ' (los nombres que fija la sesión), y la de `Prepare`, ' + PF + ', por el barrido ampliado, sin tocar ninguna firma ni prueba existente; el barrido '
    'de la resolución de sobrecargas y de la reflexión; la reemisión de PB-T3b y la corrección en el intento 2 de 3 → D-PBT3-09, D-PBT3-10, §4.1, '
    '§5.0, §5.3, §6.0, §6.2 y la precondición 12 (V46-06 a V46-10).'])
sub('→ las tres subtareas empiezan en 1/3, y C12 (PB-T3a va en el intento 2 desde F4B-E42).',
    '→ las tres subtareas empiezan en 1/3, y C12 (PB-T3a va en el intento 2 desde F4B-E42, y PB-T3b desde F4B-E45).')
# ---------------------------------------------------------------- lo que no puede quedar del API de la v4.5 en lo normativo de PB-T3b
def _norm_pb3b():
    """Lo normativo de PB-T3b: D-PBT3-09 y D-PBT3-10 (sin D-PBT3-09b, el registro de (A')), D-PBT3-13, section 5.3 y section 6.2, sin las notas que no
    van al contrato ni lo que sigue a una nota con sangria mayor (el registro de PBT3B-C13 con (A'))."""
    out = []
    for a, b in (('### D-PBT3-09. ', '### D-PBT3-09b. '), ('### D-PBT3-10. ', '### D-PBT3-11. '), ('### D-PBT3-13. ', '### D-PBT3-14. '),
                 ('### 5.3 ', '### 5.4 '), ('### 6.2 ', '### 6.3 ')):
        s = T[T.index('\n' + a):T.index('\n' + b)].split('\n')
        skip = None
        for l in s:
            ind = len(l) - len(l.lstrip(' '))
            if skip is not None and (l.strip() == '' or ind > skip):
                continue
            skip = None
            if l.lstrip().startswith('- **Nota (no va al contrato)'):
                skip = ind
                continue
            out.append(l)
    return '\n'.join(out)


_N = _norm_pb3b()
for _old in ('a second `Execute`', 'a second `ComparePost`', 'a second `Prepare`', 'sobrecarga completa de', '`Execute` completo', '`Prepare` completo',
             'las dos sobrecargas', 'las dos `Execute`', '(the ones that take the full model)', 'PushBackPilotUpdateRun.Execute`'):
    assert _N.count(_old) == 0, ('resto del API de la v4.5 en lo normativo de PB-T3b', _old, _N.count(_old))
for _new, _n in ((PF, 1), (EF, 1), (CF, 1)):
    assert _N.count(_new) >= _n, _new

out = T.encode('utf-8')
open(OUT, 'wb').write(out)
print('v4.6:', len(out), 'bytes, sha256', hashlib.sha256(out).hexdigest(), '| lineas', T.count('\n'), '| v4.5:', len(raw), 'bytes')
