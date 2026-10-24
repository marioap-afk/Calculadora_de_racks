# I-62 A-7 ronda 4 — matriz de modalidades del cierre de VERIFY (decisiones §98, punto 3)

Verificación estática de todas las modalidades del ámbito de A7-1, con las entradas, la procedencia, el cierre, las mutaciones permitidas, las comprobaciones y el resultado esperado de cada una, y los casos del preflight de la ronda 4 que la ejercen (fase:caso, con su resultado). No se inventan escenarios: las modalidades salen de AUTOMATION_PLAN 16.4, 16.7, 16.8, 16.9 y 16.13, RAE §10, V14 D.3 y G.2, los contratos del Controller de FX-02 y A-6.

## M1 — Verificación (VERIFY ordinaria)

- **Entradas:** Categorías (a) en la AuthorityRevision, `MainSha_eval`, EFF y EFF^1; (b) objeto del `Target`, `git diff --name-only BaseSha..Target.commit`, decisiones y estado; (c) contrato, delegación, entrega, TRX de `CurrentSha` y, si la entrega exige RED, de `RedSha`, hechos del relevo; (d) instrumento, si se admite.
- **Procedencia:** `Target` = `CurrentSha` de la entrega; artefactos custodiados en la ruta de la tarea (directorio del `RunId` de sus bytes); hechos del relevo de la invocación.
- **Cierre:** A7-1 sin cambio desde la ronda 3; `MainSha_eval` = `K.MainSha`; sin registro de rebase.
- **Mutaciones permitidas:** Ninguna.
- **Comprobaciones:** Constructor, validador, designate_a7 y derivación de A6-9; un registro de rebase designado no se admite (ajeno si se inyecta).
- **Resultado esperado:** Cierre válido.
- **Casos:** `q1:TRX-POS` (se cumple), `q2:RELAY-POS` (se cumple), `q5:INV-DISTINCT` (se cumple), `q8:RB-MISUSE-V` (se cumple), `q10:O1-V-CLAUSEMAP` (se cumple)

## M2 — VERIFY con y sin el instrumento

- **Entradas:** Las de cada invocación; (d) solo si el cierre admite el instrumento (A6-2, regla 1).
- **Procedencia:** La designación (`Instrument` o `null`).
- **Cierre:** A7-1, punto 1 (d): la entrada del instrumento solo en la invocación que lo admite.
- **Mutaciones permitidas:** Ninguna.
- **Comprobaciones:** Cierre con y sin (d) en V, RV y un control sobre RV; una entrada del instrumento en un cierre sin él es ajena.
- **Resultado esperado:** Cierre válido en los dos casos; ajena la entrada inyectada.
- **Casos:** `q1:TRX-POS` (se cumple), `q10:MOD-V-NOINST` (se cumple), `q10:MOD-V-INST-INJECTED` (se cumple), `q10:MOD-RV-NOINST` (se cumple), `q10:MOD-RVC-NOINST` (se cumple)

## M3 — Controles nc1, nc2, nc3, N4, N5, N6, N7a y N7b de la verificación ordinaria

- **Entradas:** Copias de las entradas de la verificación, con el campo mutado del control (RAE §10; V14 D.3), y sus hechos del relevo.
- **Procedencia:** Copias en `artifacts/orchestration/<UnitId>/<TaskId>-<control>/`; `Target` de la verificación real (contratos de FX-02, L23).
- **Cierre:** A7-1 como en la verificación; `MainSha_eval` = `K.MainSha`; sin registro de rebase.
- **Mutaciones permitidas:** nc1 `CurrentSha` inexistente; nc2 `AllowedWriteScope`; nc3 término de gate en `WorkCompleted`; N4 entrega ausente; N5 0 pruebas; N6 ninguna (estado del `-C`); N7a y N7b otra corrida.
- **Comprobaciones:** Cierre de cada control; un registro de rebase designado no se admite.
- **Resultado esperado:** Cierre válido; el registro de rebase, no admitido.
- **Casos:** `q3:N4-E` (se cumple), `q3:NC1-E` (se cumple), `q5:INV-DISTINCT` (se cumple), `q10:MOD-NC3-V` (se cumple), `q10:MOD-NC3-V-REBASE` (se cumple), `q8:RB-MISUSE-N4` (se cumple), `q9:RVC-MISUSE-V-NC1` (se cumple)

## M4 — Rebase de 16.7 antes de escribir (contrato y delegación reemitidos)

- **Entradas:** Las de una verificación ordinaria con el contrato y la delegación nuevos.
- **Procedencia:** Reemisión de 16.7; el registro de rebase no es entrada de la verificación.
- **Cierre:** El de M1 con el `K.MainSha` del contrato reemitido.
- **Mutaciones permitidas:** Ninguna.
- **Comprobaciones:** Las de M1.
- **Resultado esperado:** Cierre válido (igual que M1).
- **Casos:** `q1:TRX-POS` (se cumple), `q8:RB-MISUSE-V` (se cumple)

## M5 — Reverificación inicial de 16.7, sin conflictos

- **Entradas:** Las de la verificación más el registro de rebase (`relay-record/v2`, `Phase: REBASE`, `RebaseMap`); TRX de `CurrentSha'` (imagen) y de `RedSha` original.
- **Procedencia:** Registro en el directorio del intento de la tarea, con su `RunId` registrado; `Target` = la imagen de `CurrentSha`.
- **Cierre:** A7-1, punto 2 (c) de la ronda 3, sin cambio: registro coherente con su entrega, su `Target` y su contrato; `MainSha_eval` = `MainAfterSha`.
- **Mutaciones permitidas:** Ninguna.
- **Comprobaciones:** Registro ausente, sustituido, de otra corrida, `RunId` incorrecto, blob alterado, incoherente o sin entrega: sin cierre.
- **Resultado esperado:** Cierre válido solo con el registro coherente.
- **Casos:** `q8:RB-POS` (se cumple), `q8:RB-MSE-READS` (se cumple), `q8:RB-ABSENT` (se cumple), `q8:RB-SUBST-MAINBEFORE` (se cumple), `q8:RB-NO-H` (se cumple), `q8:RB-ACCEPTOR` (se cumple)

## M6 — Controles nc1, nc2, nc3, N4, N5, N6, N7a y N7b sobre las entradas de la reverificación (A62-A7R3-R1)

- **Entradas:** Copias de las entradas de la reverificación VERIFIED (RAE §10), con la copia de su registro de rebase, el campo mutado del control y sus hechos del relevo.
- **Procedencia:** Designación con `Kind` REVERIFICATION_CONTROL y `Source` (`RunId` y SHA-256 de la designación de la reverificación); copias de la delegación, la entrega y el registro en `<TaskId>-<control>/<Attempt>/`; contrato y TRX en sus rutas; `Target` de la reverificación.
- **Cierre:** A7-1 ronda 4, punto 2 (c): reverificación identificada y coherente con sus propias entradas; mismo `UnitId`, `TaskId`, `Attempt`, `AuthorityRevision` y `Target`; `RunId` propio; copia del registro con el mismo blob y `RunId`; copias del resto, salvo los hechos del relevo y el artefacto mutado; `MainSha_eval` = `MainAfterSha`.
- **Mutaciones permitidas:** Las de M3, en el mismo artefacto y campo; ninguna otra (mutación ausente, de más, en otro papel o de otro control: sin cierre).
- **Comprobaciones:** Cierre de los ocho; cotejo de entradas con RV (oráculo relativo estático); sustitución y manipulación de la copia, de la identidad, del vínculo y de las copias; formato; aceptante.
- **Resultado esperado:** Cierre válido con `MainSha_eval` = `MainAfterSha` y entradas iguales a las de RV salvo la mutación; sin cierre en todo lo demás.
- **Casos:** `q9:RVC-NC1` (se cumple), `q9:RVC-NC2` (se cumple), `q9:RVC-NC3` (se cumple), `q9:RVC-N4` (se cumple), `q9:RVC-N5` (se cumple), `q9:RVC-N6` (se cumple), `q9:RVC-N7A` (se cumple), `q9:RVC-N7B` (se cumple), `q9:REL-NC1` (se cumple), `q9:REL-NC2` (se cumple), `q9:REL-N4` (se cumple), `q9:RVC-REBASE-BYTES` (se cumple), `q9:RVC-REBASE-OTHER` (se cumple), `q9:RVC-SOURCE-INCOHERENT` (se cumple), `q9:RVC-SUBST-DELEG` (se cumple), `q9:RVC-MUT-NONE` (se cumple), `q9:RVC-ACCEPTOR` (se cumple)

## M7 — Segunda recuperación de 16.7 y sus controles

- **Entradas:** Las de la reverificación con el registro del segundo rebase.
- **Procedencia:** 16.7 la permite (como máximo dos recuperaciones por `TaskId`, AP L598).
- **Cierre:** El registro no es coherente (punto 2 (c): `MainBeforeSha` ≠ `K.MainSha`; encadenado, tampoco asigna el `CurrentSha` de la entrega): sin cierre ni lanzamiento; exclusión declarada en §3.5 (A62-A7R3-O2), sin enmienda nueva.
- **Mutaciones permitidas:** Ninguna.
- **Comprobaciones:** Registro encadenado, registro compuesto, control sobre la segunda reverificación y los dos registros juntos.
- **Resultado esperado:** Sin cierre en todos los casos; el caso vuelve al Coordinator.
- **Casos:** `q10:O2-FIXTURE` (se cumple), `q10:O2-RV2-CHAINED` (se cumple), `q10:O2-RV2-COMPOSED` (se cumple), `q10:O2-RVC2` (se cumple), `q10:O2-RV2-BOTH` (se cumple)

## M8 — Medición de A6-7

- **Entradas:** Las de una VERIFY sin hechos del relevo (no produce una verificación).
- **Procedencia:** Kit de la medición; `Kind` MEASUREMENT.
- **Cierre:** A7-1 sin cambio; `MainSha_eval` = `K.MainSha`; sin registro de rebase (A-7 no la admite sin una reverificación aplicable).
- **Mutaciones permitidas:** Ninguna.
- **Comprobaciones:** Hechos del relevo y registro de rebase designados: no admitidos.
- **Resultado esperado:** Cierre válido sin hechos del relevo ni registro.
- **Casos:** `q2:RELAY-MEASUREMENT` (se cumple), `q2:RELAY-MEASUREMENT-DESIGNATED` (se cumple), `q8:RB-MISUSE-M` (se cumple)

## M9 — Reejecución de transporte de una invocación (16.8)

- **Entradas:** Las mismas entradas, con un `RunId` nuevo y sus propios hechos del relevo.
- **Procedencia:** La designación de la reejecución.
- **Cierre:** El de la invocación reejecutada.
- **Mutaciones permitidas:** Ninguna nueva.
- **Comprobaciones:** nc1 sobre RV reejecutado.
- **Resultado esperado:** Cierre válido.
- **Casos:** `q10:MOD-RVC-RERUN` (se cumple)

## M10 — A62-A7R3-O1: C1 de la derivación del aceptante en la reverificación y en sus controles

- **Entradas:** Una ejecución del instrumento que lee en `MainSha_eval`.
- **Procedencia:** El instrumento y el aceptante fijados (A-6 sin cambio).
- **Cierre:** La tabla de A6-9 no lleva `MainAfterSha` ni las imágenes distintas de `Target.commit` a C1 (residuo declarado en §3.5; no se da por resuelto).
- **Mutaciones permitidas:** Ninguna.
- **Comprobaciones:** C1 estático; clausemap con `--main MainAfterSha` en RV y en nc2 sobre RV; contraste con `--main K.MainSha` y con la verificación ordinaria.
- **Resultado esperado:** R9 MISMATCH en la reverificación y en sus controles (rechazo falso, nunca aceptación falsa); R9 MATCH en la verificación ordinaria.
- **Casos:** `q10:O1-C1-STATIC` (se cumple), `q10:O1-RV-CLAUSEMAP` (se cumple), `q10:O1-RVC-CLAUSEMAP` (se cumple), `q10:O1-RV-CLAUSEMAP-KMAIN` (se cumple), `q10:O1-V-CLAUSEMAP` (se cumple)

## Exclusiones documentadas

- **Recuperación con conflictos:** 16.7: `git rebase --abort` y STOP al Coordinator; no hay reverificación ni invocación VERIFY que cerrar.
- **nc4:** RAE §10: control de aceptación sin invocación; fuera del ámbito de A7-1 (solo invocaciones VERIFY y la medición).
- **N8, N9 y N10:** V14 D.3: negativos de sesión sin invocación; fuera del ámbito.
- **PLAN:** Fuera del ámbito de A-7 (la planificación no tiene cierre de VERIFY).

## Presupuestos

A-7 no cambia presupuestos (A7-1, punto 8). V14 D.3 cuenta 11 lanzamientos base de Codex (planificación, verificación, Architect y los 8 controles con invocación), un pool de 4 reejecuciones de transporte (como máximo 2 por fase) y un tope de 15; no reserva lanzamientos para la reverificación de 16.7. Una reverificación, sus controles y una segunda recuperación solo se lanzan si P-07 lo permite con ese tope (16.8; 16.7: «suma al tope de invocaciones lo mismo que una recuperación»). Este preflight no fija si el tope alcanza en la ronda real (riesgo no demostrado).

**Cierre total de la matriz: sí** (10 modalidades; 56 casos referenciados).

