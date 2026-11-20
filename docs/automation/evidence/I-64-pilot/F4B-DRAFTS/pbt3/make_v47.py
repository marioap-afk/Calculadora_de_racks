# -*- coding: utf-8 -*-
"""Arma la v4.7 del paquete de PB-T3 (el de la emision de PB-T3c) desde la v4.6 custodiada (09210abe, la de la correccion de PB-T3b): cada cambio es
una sustitucion o una insercion exacta, que tiene que aplicar una sola vez.
- V47-01: los marcadores <tras el cierre de PB-T3b>, rellenos con lo medido sobre el cierre de PB-T3b (fe1c67f4, F4B-E46) por objetos git, cada uno con
  su fuente: la BaseSha y el estado (precondicion 1), la custodia (precondicion 9), el barrido de las guardas (section 4.1), los MinSelected y la nota
  de lo heredado (section 6.3) y la linea base de la CI (section 6.3);
- V47-02: M-07 de la seccion 13 de CA-T3b (F4B-E45): la suite completa de Core sobre la copia exportada con el esqueleto; la precondicion 13 de PB-T3c;
- V47-03: la leccion de la decision del Owner F4B-E46 (el total exacto de cada rama), con las cuentas previstas de PB-T3c en una sola rama;
- V47-04: la fila de PB-T3c de la tabla de RR-3 (CT-05), enumerada con sus lineas en fe1c67f4;
- V47-05: el barrido de sobrecargas y de reflexion de PB-T3c (T3C-1) y el de nombres, sobre fe1c67f4;
- V47-06: las lecciones de la revision independiente de PB-T3b (PBT3B-IR-M01, M02 y M07) para las recetas de PB-T3c;
- V47-07: la version, el estado y la numeracion (F4B-E47, las secciones 274 y 275 de la evidencia, R-23).
Uso (desde hs65/pbt3): python -I make_v47.py  -> escribe I-64-f4b-pbt3-gate-contract-package-v47.md"""
import hashlib, os

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v46.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v47.md')
raw = open(SRC, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '09210abe2ed5482c87bec1829f5798790d31090de490e615574c6c845d9e804a', 'la v4.6 no es la custodiada'
T = raw.decode('utf-8')
assert '\r' not in T
MARK = '`<tras el cierre de PB-T3b>`'
assert T.count(MARK) == 8, T.count(MARK)  # seis sitios y dos menciones del propio marcador (la cabecera y V46-05)


def sub(old, new):
    global T
    assert T.count(old) == 1, ('no aplica una sola vez', old[:80], T.count(old))
    T = T.replace(old, new)


def line_sub(prefix, new):
    global T
    L = T.split('\n')
    k = [i for i, l in enumerate(L) if l.startswith(prefix)]
    assert len(k) == 1, ('linea', prefix[:60], len(k))
    L[k[0]] = new
    T = '\n'.join(L)


def after_line(prefix, new_lines):
    global T
    L = T.split('\n')
    k = [i for i, l in enumerate(L) if l.startswith(prefix)]
    assert len(k) == 1, ('linea', prefix[:60], len(k))
    T = '\n'.join(L[:k[0] + 1] + new_lines + L[k[0] + 1:])


CLOSE, GREEN, RED, RX, RB = '`fe1c67f4`', '`c7582785`', '`9fda277e`', '`2bb3b759`', '`c7441653`'
CI_RUN = '`44707482956`'
ROWS = '`F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt`'
# ---------------------------------------------------------------- la cabecera (V47-07)
sub('# I-64 F4B — Paquete del contrato de PB-T3 (la escritura completa del Workspace), v4.6',
    '# I-64 F4B — Paquete del contrato de PB-T3 (la escritura completa del Workspace), v4.7')
_est = [l for l in T.split('\n') if l.startswith('**Estado:** v4.6 (registro v4.5 → v4.6, abajo): ')]
assert len(_est) == 1
line_sub('**Estado:** v4.6 (registro v4.5 → v4.6, abajo): ',
         '**Estado:** v4.7 (registro v4.6 → v4.7, abajo): **el paquete de la emisión de PB-T3c**, sobre el cierre de PB-T3b (' + CLOSE + ', F4B-E46). '
         'Rellena los marcadores `<tras el cierre de PB-T3b>` con lo medido en ' + CLOSE + ', cada uno con su fuente; aplica M-07 de la §13 de CA-T3b, la '
         'lección de la decisión del Owner **F4B-E46** (el total exacto de cada rama de recuento) y las de la revisión independiente de PB-T3b que tocan a '
         'las recetas; y enumera la fila de PB-T3c de la tabla de RR-3. **No cambian el diseño, D-1, ni ninguna guarda, prueba o firma existente; lo de '
         'PB-T3a y PB-T3b es historia.** La v4.6 ' + _est[0][len('**Estado:** v4.6 '):].replace('(registro v4.5 → v4.6, abajo): **el paquete de la corrección',
                                                                                        '(registro v4.5 → v4.6, abajo) fue **el paquete de la corrección', 1))
sub('Viene de diez revisiones:', 'Viene de once revisiones:')
_c13 = [l for l in T.split('\n') if l.startswith('- **la comprobación acotada de PB-T3b** (CA-T3b, sobre la v4.5 y el contrato en seco de PB-T3b)')]
assert len(_c13) == 1
after_line(_c13[0][:80], ['- su recomprobación de la v4.6 (§13 de CA-T3b), para la reemisión de PB-T3b: **EMITIBLE, B 0 / I 0 / M 2** (F4B-E45). M-06 se '
                          'corrigió en la custodia; M-07 lo aplica esta v4.7 (V47-02).'])
line_sub('**Dónde está PB-T3, medido en `eab27a9e`:**',
         '**Dónde está PB-T3, medido en ' + CLOSE + ':** PB-T1 y PB-T2 están CERRADAS. **PB-T3a está CERRADA** (F4B-E43, custodia `c40eea4a`; GREEN '
         '`6919e17a`, intento 2 de 3). **PB-T3b está CERRADA** (F4B-E46, custodia ' + CLOSE + ', hija directa de su GREEN ' + GREEN + '; RED ' + RED +
         '; intento 2 de 3, tras la reemisión con la v4.6, X ' + RX + ' y BASE ' + RB + '; CI ' + CI_RUN + ': Core 19 124, con 6 omitidas, y UI 2 451, '
         'con 21). **Esta v4.7 es el paquete de la emisión de PB-T3c**, por la autorización condicional del Owner (F4B-E35, pregunta 3), con su '
         'remedición sobre ' + CLOSE + ' y su comprobación acotada (precondiciones 2, 3, 5, 10 y 13). La emisión de PB-T3c es **F4B-E47**, con la '
         'evidencia 274 (X) y 275 (BASE), y la remedición, R-23 del registro. No declara GATE PASS.')
sub('`v45-delta.md` (de la v4.4 a la v4.5) y `v46-delta.md` (de la v4.5 a la v4.6).',
    '`v45-delta.md` (de la v4.4 a la v4.5), `v46-delta.md` (de la v4.5 a la v4.6) y `v47-delta.md` (de la v4.6 a la v4.7).')
sub('## Registro de cambios v4.5 → v4.6 (la decisión del Owner F4B-E45: renombrar los métodos nuevos; y la comprobación acotada de PB-T3b, CA-T3b)',
    '## Registro de cambios v4.6 → v4.7 (el cierre de PB-T3b, F4B-E46, y la §13 de CA-T3b)\n\n'
    '**Origen.** PB-T3b cerró (F4B-E46, custodia ' + CLOSE + ') con la decisión del Owner sobre su nc3: desde PB-T3c, una evidencia esperada que dependa '
    'de cómo cuente la TRX lleva el total exacto de cada rama (`F4B-DRAFTS/coordinator/owner-pbt3b-nc3-decision.md`). La §13 de CA-T3b dejó M-07 para '
    'esta revisión, y la revisión independiente de PB-T3b, tres menores sobre las recetas (PBT3B-IR-M01, M02 y M07, en `docs/ideas-futuras.md`). Todo '
    'se leyó por objetos git en ' + CLOSE + '. **No cambian el diseño, D-1, ninguna guarda ni prueba existente, ni lo emitido de PB-T3a y PB-T3b.**\n\n'
    '| Id | Origen | Qué cambia | Dónde |\n'
    '|---|---|---|---|\n'
    '| **V47-01** | **F4B-E46** (el cierre de PB-T3b) | **Los marcadores `<tras el cierre de PB-T3b>`, rellenos** con lo medido en ' + CLOSE + ', cada uno '
    'con su fuente: la `BaseSha` y el estado (precondición 1); la custodia de la emisión (precondición 9); el barrido de las guardas sobre el cierre '
    '(§4.1); los `MinSelected` de `WorkspacePilotUpdateGuardTests` (86) y `WorkspacePilotUpdateIntegrationGuardTests` (123) y la nota de lo heredado '
    '(§6.3); y la línea base de la CI (§6.3) | Precondiciones 1 y 9; §4.1; §6.3 |\n'
    '| **V47-02** | **CA-T3b §13 M-07** | **La comprobación de un esqueleto ejecuta la suite completa de Core** sobre la copia exportada, no solo las '
    'pruebas de reflexión: la reflexión rota solo se ve al ejecutar, y los escaneos de ensamblado de `tests/` no los ve ningún barrido por nombre. En '
    'el §5.0 y en la precondición 12 (historia); la precondición 13, nueva, lo pide para PB-T3c con el esqueleto de la fachada | §5.0; precondiciones '
    '12 y 13 |\n'
    '| **V47-03** | **F4B-E46 (Owner)** | **El total exacto de cada rama:** la regla, en el §5.0 y el §6.0; en el §5.4, CT-PBT3C-01 a 05 son un `[Fact]` '
    'cada una; y en el §6.3, los recuentos previstos del RED y del GREEN de PB-T3c, en su única rama, sin alternativa | §5.0; §5.4; §6.0; §6.3 |\n'
    '| **V47-04** | **CA-T3a M-03** (la fila aplazada de la v4.6) | **La fila de PB-T3c de la tabla de RR-3** (`PushBackWritePathPluginGuardTests`, '
    'CT-05), enumerada: las ocho fichas de D2 y los nombres de las reglas de CT-PBT1B-14, con sus líneas en `PushBackSharedReferenceGuardTests.cs` de '
    + CLOSE + ' | §6.0 |\n'
    '| **V47-05** | La sesión (V46-07 para PB-T3c) | **El barrido de sobrecargas y de reflexión de PB-T3c** (T3C-1) y el de nombres, sobre ' + CLOSE +
    ': sin colisión | §4.1 |\n'
    '| **V47-06** | **La revisión independiente de PB-T3b** (PBT3B-IR-M01, M02 y M07) | **Las lecciones de las recetas** (la cultura de los números, '
    'las recetas por construcción y `IsDirty` justo antes): las CT de PB-T3c son guardas de texto y no tienen recetas con borradores, pero sus '
    'autopruebas, si formatean un número, usan `InvariantCulture` | §5.4 (nota) |\n'
    '| V47-07 | La sesión | **La versión, el estado y la numeración:** el `Objective` de PB-T3c dice «v4.7»; la cabecera; §8.1 gana F4B-E46; la emisión '
    'de PB-T3c es F4B-E47, con la evidencia 274 (X) y 275 (BASE), y la remedición, R-23 | Cabecera; precondición 9; §6.3; §8.1 |\n\n'
    '## Registro de cambios v4.5 → v4.6 (la decisión del Owner F4B-E45: renombrar los métodos nuevos; y la comprobación acotada de PB-T3b, CA-T3b)')
# ---------------------------------------------------------------- la precondicion 1 (V47-01)
sub('**La `BaseSha` de PB-T3c es el cierre de PB-T3b:** `<tras el cierre de PB-T3b>`.',
    '**La `BaseSha` de PB-T3c es la BASE de su emisión, sobre el cierre de PB-T3b** (V47-01): en `src/` y `tests/`, igual que ' + CLOSE + ', que '
    'tiene los de su GREEN ' + GREEN + '.')
after_line('   - **En `eab27a9e`** (V46-05):', [
    '   - **En ' + CLOSE + '** (V47-01): **PB-T3b, CERRADA** (F4B-E46: custodia ' + CLOSE + ', hija directa del GREEN ' + GREEN + '; RED ' + RED + '; '
    'reemisión con la v4.6, X ' + RX + ' y BASE ' + RB + '; intento 2 de 3; verificación 14/14; nc1 por el sustituto de PD-F4B-01, nc2 PASS y nc3 por '
    'disposición del Owner). **La CI del GREEN** ' + CI_RUN + ': Core 19 124 resultados, con 19 118 `Passed` y 6 `Skipped`, y UI 2 451, con 2 430 '
    '`Passed` y 21 `Skipped`. Fuente: `f4b-dispositions.md` (F4B-E46) y ' + ROWS + ', en ' + CLOSE + '.'])
# ---------------------------------------------------------------- la precondicion 9 (V47-01 y V47-07)
line_sub('   - **en la emisión de PB-T3c** (V46-05): ',
         '   - **en la emisión de PB-T3c** (F4B-E47; V46-05 y V47-01): el paquete en su versión v4.7, sobre la v4.6 que custodió ' + RX + ', con '
         '`v47-delta.md`, `make_v47.py`, `check_v47.py` y su salida; su remedición sobre ' + CLOSE + ' (`pbt3c-anchors-out.txt`, con `measure_pbt3c.py`, '
         'y R-23 del registro); el barrido de las guardas (`sweep-pbt3c-close.txt`), el de sobrecargas y reflexión (`overload_sweep_t3c.py` y '
         '`overload-sweep-pbt3c-fe1c67f4.txt`) y el de nombres (`name_check_t3c.py` y `name-check-pbt3c-fe1c67f4.txt`); el esqueleto de la fachada '
         'como parche (`pbt3c-skeleton.patch`, con `make_pbt3c_skeleton.py`) y la compilación de comprobación de la sesión (precondición 13), en '
         '`compile-check-pbt3c/`, con sus registros redactados y su manifiesto; su comprobación acotada (CA-T3c) con su '
         'encargo; y el seco de su generador. La evidencia: 274 (X) y 275 (BASE).')
# ---------------------------------------------------------------- las precondiciones 12 (historia) y 13 (V47-02)
sub('Y, porque una reflexión rota solo se ve al ejecutar, `WorkspacePilotPrepareTests` sobre esa copia, en verde (V46-07);',
    'Y, porque una reflexión rota solo se ve al ejecutar, `WorkspacePilotPrepareTests` sobre esa copia, en verde (V46-07). **Lo que se hizo, y lo que '
    'cubre también los escaneos de ensamblado de `tests/`, fue ejecutar la suite completa de Core sobre esa copia:** 19 107 de 19 107 con los tres '
    'nombres (V47-02; CA-T3b §13 M-07; `F4B-DRAFTS/pbt3/compile-check-pbt3b/compile-check.md` y su `out-run3.txt`);')
_pre12_end = [i for i, l in enumerate(T.split('\n')) if l == '    La decisión ya está tomada, así que la reemisión no vuelve al Owner salvo por lo que dice la precondición 10.']
assert len(_pre12_end) == 2, len(_pre12_end)  # las de las precondiciones 11 y 12
_L = T.split('\n')
_L[_pre12_end[1] + 1:_pre12_end[1] + 1] = [
    '13. **Para PB-T3c, antes de emitir** (V47-02 y V47-05):',
    '    - **el barrido de sobrecargas y de reflexión** con lo que PB-T3c cambia (T3C-1: el constructor de `RackPilotMutationScope`; los nombres '
    '`RackViewBlockEraser`, `EraseInTransaction`, `EraseUnit` en WR y `_erasures`), sobre la `BaseSha`: ninguna llamada fuera de WR, ningún grupo de '
    'métodos, ninguna reflexión sobre los tipos tocados y ningún literal de guarda que prohíba los nombres (§4.1);',
    '    - **la compilación de comprobación** del esqueleto de la fachada (§5.4) sobre una copia exportada de la base, fuera del worktree: Core, UI y el '
    'Plugin (con `-o`), sin errores, **y la suite completa de Core ejecutada sobre esa copia**, en verde. La sesión fija el esqueleto como parche '
    '(`pbt3c-skeleton.patch`);',
    '    - **la comprobación acotada de la v4.7** (CA-T3c) da 0 bloqueantes y 0 importantes sin resolver.']
T = '\n'.join(_L)
# ---------------------------------------------------------------- section 4.1: el barrido de las guardas sobre el cierre (V47-01) y el de PB-T3c (V47-05)
sub('**El barrido se repite sobre el cierre de PB-T3b,** con esas clases ya en `tests/`: `<tras el cierre de PB-T3b>`.',
    '**Repetido sobre el cierre de PB-T3b** (' + CLOSE + '; V47-01; `guard_sweep.py` y `sweep_t3b_diff.py`, la salida en `sweep-pbt3c-close.txt`): de '
    '92 a 94 clases que leen código, y las dos nuevas son las de PB-T3b, `PushBackWritePathGuardTests` y `PushBackPilotUpdateEquivalenceGuardTests`. '
    'Con el texto de PB-T3c hay 9 coincidencias nuevas, todas de `PushBackWritePathGuardTests` y de expresiones genéricas de sus reglas de D2 (G-L1 '
    'de CT-PBT3B-13), que solo leen PP, UR y la interfaz; ninguna se pierde. Su CT-13 (ii) busca fuera de PP, UR y `Draft/` `PushBackFullPrepareInput`, '
    '`ExecuteFull` y `Precheck` con un borrador completo, que WR y la fachada no nombran. **Sin colisión.**')
after_line('- **Los nombres nuevos contra los literales de las guardas:** `name_check_v46.py`', [
    '',
    '**El barrido de sobrecargas, de reflexión y de nombres de PB-T3c** (V47-05; precondición 13), sobre ' + CLOSE + ', por objetos git:',
    '- **`overload_sweep_t3c.py`** (la salida, `overload-sweep-pbt3c-fe1c67f4.txt`; 1 802 archivos C# de `src/` y `tests/`):',
    '  - **T3C-1, el constructor de `RackPilotMutationScope`,** que gana un parámetro: un solo uso, en WR:156, el archivo que lo cambia; ningún grupo '
    'de métodos; el único literal con su nombre es `Class(code, "RackPilotMutationScope")` de `WorkspacePilotUpdateGuardTests.cs:38`, que toma el '
    'cuerpo de la clase y no cuenta constructores;',
    '  - **la reflexión sobre los tipos tocados** (`RackPilotMutationScope`, `RackPilotUpdateScope`, `RackPilotUpdateWriter`, '
    '`IPushBackPilotMutationScope`, `IPushBackPilotViewEraser` y `PushBackPilotUpdateRun`): ninguna, porque las pruebas leen el Plugin como texto;',
    '  - **los nombres nuevos:** `RackViewBlockEraser`, `EraseInTransaction` y `_erasures`, libres en todo `src/` y `tests/`; `EraseUnit`, libre en WR '
    '(existe desde PB-T3b en la interfaz y en los dobles, que es lo que WR implementa);',
    '  - **la regresión de PB-T3b:** `Precheck`, con sus dos sobrecargas ya en el código, tiene 9 usos, todos con un argumento tipado, y ningún grupo de '
    'métodos;',
    '- **`name_check_t3c.py`** (la salida, `name-check-pbt3c-fe1c67f4.txt`): los literales propios de cada nombre son subcadenas genéricas '
    '(`Transaction`, `Erase`, `Erase()`, `Rack`, `lock`) de guardas cuyo alcance no es WR ni la fachada: las prohibiciones de `Transaction` miran la '
    'carpeta `Pilot/` de Application, la corrida, el puerto, el anfitrión y el borrador; el término `Erase` de G-9 es `\\.Erase\\s*\\(`, que '
    '`.EraseInTransaction(` no cumple; y `Erase()` y `lock` son de otros comandos y de una lista de palabras clave. **Sin colisión.**'])
# ---------------------------------------------------------------- section 5.0: M-07 (V47-02) y el total por rama (V47-03)
sub('y la compilación del esqueleto sobre una copia exportada de la base, con las pruebas que hacen reflexión sobre esos tipos.',
    'y la compilación del esqueleto sobre una copia exportada de la base, **ejecutando la suite completa de Core sobre esa copia** (V47-02; CA-T3b '
    '§13 M-07): la reflexión rota solo se ve al ejecutar, y los escaneos de ensamblado de `tests/` (`Assembly.GetTypes()` y `GetExportedTypes()`) no '
    'los ve ningún barrido por nombre.')
after_line('**Por eso los métodos públicos nuevos del modelo completo llevan nombre propio**', [
    '',
    '**El total exacto de cada rama** (V47-03; la decisión del Owner F4B-E46, sobre el nc3 de PB-T3b). Una evidencia esperada que depende de cómo '
    'cuente la TRX, por ejemplo una `Theory` con `Skip`, lleva el total exacto de cada rama de recuento, y no un solo total con una alternativa en '
    'las omitidas. Así lo lee un control de forma estricta. **Medido en el cierre de PB-T3b:** la TRX cuenta la `Theory` S-2 con `Skip` como **una** '
    'fila (' + ROWS + '). Si una subtarea quita un `Skip` de D-1 o añade una `Theory`, calcula cada rama.'])
# ---------------------------------------------------------------- section 5.4: un [Fact] por CT (V47-03) y las lecciones de las recetas (V47-06)
after_line('**Las clases:** `PushBackViewEraseGuardTests` (01 a 04) y `PushBackWritePathPluginGuardTests` (05)', [
    '',
    '**Una prueba, una fila** (V47-03): CT-PBT3C-01 a 05 son **un `[Fact]` cada una**, sin `Theory`. Así los recuentos previstos del RED y del GREEN '
    '(§6.3) tienen una sola rama.',
    '',
    '- **Nota (no va al contrato).** Las lecciones de la revisión independiente de PB-T3b para las recetas (PBT3B-IR-M02: los números formateados '
    'dependen de la cultura, y se fijan con `InvariantCulture`, como CT-PBT1B-12; PBT3B-IR-M01 y OWNER-2: `pb-20-ab` no da un lateral que exista solo '
    'por un hueco B, y AUTH-13 solo admite la versión de sobre `1.0`, así que esas recetas van «por construcción» y lo dicen; PBT3B-IR-M07: `IsDirty` '
    'cierto justo antes de la operación) son de recetas con borradores y diseños. Las CT de PB-T3c son guardas de texto sobre WR y la fachada, sin '
    'recetas de esa clase. Si una autoprueba formatea un número, usa `InvariantCulture` (V47-06).'])
# ---------------------------------------------------------------- section 6.0: el total por rama (V47-03) y la fila de CT-05 de RR-3 (V47-04)
after_line('- `ExpectedEvidence` lo pide así: `tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido y `ui-tests` en `failure` (PB-T3a)', [
    '- **El total exacto de cada rama** (F4B-E46; V47-03): un recuento previsto que dependa de cómo cuente la TRX lleva el total exacto de cada rama, sin '
    'una alternativa en las omitidas (§5.0). En PB-T3c hay una sola rama (§6.3);'])
sub('| PB-T3c | `PushBackWritePathPluginGuardTests` (CT-05) | **Líneas:** las fichas de D2 y las reglas de CT-PBT1B-14 que copia. Se enumeran con su '
    'referencia (`PushBackSharedReferenceGuardTests.cs`, con sus líneas en la base de PB-T3c) en la emisión de PB-T3c, como pide CA-T3a M-03 para las '
    'filas de PB-T3a |',
    '| PB-T3c | `PushBackWritePathPluginGuardTests` (CT-05) | **Líneas:** las ocho fichas de D2 que copia (`PushBackSharedReferenceGuardTests.cs:35-36` '
    'en ' + CLOSE + '; V47-04), `WorkingBaseline`, `workingBaseline`, `LastModuleReconciliation`, `computation.System`, `lastComputation.System`, '
    '`session.System`, `lastCompositeSystem` y `SystemToInsert`; y los nombres de las reglas de CT-PBT1B-14 que copia (sus líneas 57 a 69), `Rule1`, `Rule1b` y '
    '`Rule1c` |')
# ---------------------------------------------------------------- section 6.3: el Objective (V47-07), los MinSelected y su nota y la CI (V47-01 y V47-03)
sub('segun el paquete del contrato de PB-T3 v4.6 y la lista cerrada CT-PBT3C-01..05', 'segun el paquete del contrato de PB-T3 v4.7 y la lista cerrada CT-PBT3C-01..05')
sub('  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` | los medidos tras PB-T3b: `<tras el cierre de PB-T3b>` | false |',
    '  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` | **86, 123**: los `Passed` de cada clase en la CI del GREEN '
    'de PB-T3b (' + CI_RUN + '), sin los `Skip` de D-1 (1 y 2) | false |')
sub('(`Workspace` de Core, con los `Skip` de D-1 ya en el código): `<tras el cierre de PB-T3b>`.',
    '(`Workspace` de Core, con los `Skip` de D-1 ya en el código): medidos en ' + CLOSE + ' con el criterio de `count_tests_git.py` (el seco del '
    'generador, `make_pbt3c_contract-dry.txt`), ninguno baja del mínimo del contrato reemitido de PB-T3b (`Workspace` de Core, 3 261: 3 271 − 10 de '
    'D-1). Los 86 y 123 de arriba son los de la TRX (' + ROWS + ').')
line_sub('- **Línea base de la CI:** la del cierre de PB-T3b: `<tras el cierre de PB-T3b>`.',
         '- **Línea base de la CI:** la del cierre de PB-T3b (F4B-E46): run ' + CI_RUN + ' sobre el GREEN ' + GREEN + ', Core **19 124** resultados, con '
         '19 118 `Passed` y 6 `Skipped`, los de D-1 (la TRX cuenta la `Theory` S-2 con `Skip` como una fila; ' + ROWS + '), y UI **2 451**, con 2 430 '
         '`Passed` y 21 `Skipped`. **Previstos, con el total exacto de cada rama** (F4B-E46; V47-03): PB-T3c no quita ningún `Skip` de D-1 ni añade '
         'ninguna `Theory`, así que hay **una sola rama**. **El RED:** Core **19 129** resultados, con 19 119 `Passed`, **4** `Failed` (CT-01 a 04) y 6 '
         '`Skipped`. **El GREEN:** Core **19 129**, con 19 123 `Passed`, 0 `Failed` y 6 `Skipped`. **UI, en los dos:** **2 451**, con 2 430 `Passed` y '
         '21 `Skipped`.')
# ---------------------------------------------------------------- section 8.1: F4B-E46 (V47-07)
after_line('- **F4B-E45, la parada PBT3B-C02 y PBT3B-C07 del intento 1 de PB-T3b**', [
    '- **F4B-E46, el cierre de PB-T3b, con la decisión del Owner sobre su nc3** (2026-11-20, literal «Aceptar por disposición (Recomendado)»; '
    '`F4B-DRAFTS/coordinator/owner-pbt3b-nc3-decision.md`): nc3 de PB-T3b por disposición; **desde PB-T3c, una evidencia esperada que dependa de '
    'cómo cuente la TRX lleva el total exacto de cada rama** → §5.0, §5.4, §6.0 y §6.3 (V47-03).'])
assert T.count(MARK) == 3 and all(l.startswith(('**Estado:** v4.7', '| **V47-01**', '| V46-05')) for l in T.split(chr(10)) if MARK in l), \
    ('quedan marcadores', T.count(MARK))  # solo las menciones del propio marcador (la cabecera, V47-01 y V46-05)
out = T.encode('utf-8')
open(OUT, 'wb').write(out)
print('v4.7:', len(out), 'bytes, sha256', hashlib.sha256(out).hexdigest(), '| lineas', T.count('\n'))
