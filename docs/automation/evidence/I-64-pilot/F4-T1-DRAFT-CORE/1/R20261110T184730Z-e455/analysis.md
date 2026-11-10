# Análisis de la entrega del intento 1 de F4-T1-DRAFT-CORE

## Hechos

- Entrega: RED `a362744c`, GREEN `aefc34a5`, sobre `13775d8f`. Diez archivos nuevos, exactamente los del contrato.
- CI push del SHA exacto `42877329473`: cuatro jobs en success. Filtro `FullyQualifiedName~RackCad.Tests.Workspace` 1165/1165; Core 16703/16703; el RED (`42877124150`) falla en 181 pruebas, todas dentro del filtro.
- Verificación del Controller `R20261110T184730Z-e455`: `EXECUTION_VERIFIED`, 14/14 en pass. Se conserva y no se reinterpreta.
- nc1 `R20261110T185024Z-3b7f`, nc2 `R20261110T185322Z-af9d` y nc3 `R20261110T185608Z-7ec7`: CONTROL PASS, sin puentes.
- Régimen de revisión (F4-E02): la verificación es necesaria pero no suficiente. Se hicieron la revisión directa de la sesión y una revisión adversarial independiente del diff `13775d8f..aefc34a5`.

## Revisión directa de la sesión

| Id | Hallazgo | Equivale a |
|---|---|---|
| DR-01 | `Discard`, `RetireSession` y `EnterError` liberan la ranura de Aplicando con la operación del host aún en vuelo | I-2 |
| DR-02 | La guarda `TheCoreFilesAreTheOnlyDraftFilesAndNoExistingWorkspaceFileMentionsThem` prohíbe todo consumidor del núcleo; T2 tendría que debilitarla | M-10 |
| DR-03 | `PendingText` se documenta ordenado pero se expone como `Dictionary` | M-8 |
| DR-04 | `EnterError` desde Conflicto es una salida de Conflicto | M-1 |
| DR-05 | `DraftIntentRead.Admitted` y `DraftStaleBase.Create` son públicos: hacen falta para el lector de T2, pero la regla «una preview no tiene vía» queda sin guarda | M-3, M-9 |

## Revisión adversarial independiente

Revisor independiente sin conclusiones previas. Exportó los SHA con `git archive` a un directorio temporal fuera del repositorio, corrió el filtro sobre el GREEN (1165/1165), comprobó que las 181 fallas del RED son de aserción y reprodujo los escenarios con un programa de sondeo contra el núcleo GREEN. Ese programa no se versiona. Resultado: 0 BLOQUEANTE, 4 IMPORTANTE y 10 MENOR. El texto íntegro del revisor está en `independent-review.md`, junto a este archivo.

### Importantes

- **I-1 (D-10, D-17, INV-20).** Al nacer (`DraftSession.cs:237-238`), en `DiscardAndReload` (`:374`) y en Applied (`:478`), `CurrentDraftState` e `InitialDraftState` pasan a ser el mismo objeto. El estado editable que ya existe en Application para el piloto es una clase mutable con mutadores en sitio. Una edición en sitio del actual cambia también el inicial: el comparador ve igualdad, el borrador nunca queda Dirty y el cierre no se veta. Secuencia reproducida: `TState` mutable, `d.CurrentDraftState.W = 45` → mismo objeto, `initial.W=45`, `IsDirty=False`, `veto=False`. Las pruebas fijan la identidad (`Assert.Same(Initial, Current)`).
- **I-2 (D-13).** `EnterError`, `Discard`, `DiscardAll` y `RetireSession` liberan la ranura de Aplicando mientras la operación del host sigue en vuelo, y `CompleteApply` rechaza después su resultado. Secuencia: `A.TryBeginApply`, `A.EnterError("surface threw")`, `B.TryBeginApply` → true; `A.CompleteApply(CommitThrew)` → no aceptado, A en Error y sin origen Desconocido. Hay dos Actualizar a la vez y se pierde la clasificación de D-13.
- **I-3 (D-12).** `DraftStaleBase.Create` resuelve una clave repetida como «gana la última», y un NotMember posterior borra un miembro anterior. Secuencia: base `[D1 Mutable "{a}"]` y relectura `[D1 "{b external}", D1 "{a}"]` → MATCH. Es una aceptación falsa, que D-12 excluye («solo conflictos falsos, nunca aceptaciones falsas»).
- **I-4 (INV-F4T1-16, INV-F4T1-18).** La guarda nueva no detecta un campo `static readonly` de un tipo mutable (por ejemplo una lista) ni `Bom` dentro de un identificador (`DraftBomLines`). El código actual cumple; la guarda no detectaría una violación real de dos familias que el contrato nombra.

### Menores

- **M-1.** `EnterError` admite cualquier origen. Desde Desconocido deja `UnknownOrigin=CommitThrew` con estado Error.
- **M-2 (D-12 punto 2).** Una entrada Blocking conserva y compara su texto de sobre. Blocking es `BlockingUnreadable`, no interpretable; D-12 solo guarda el texto de los miembros interpretables.
- **M-3.** `ThereIsNoPublicPathFromAPreviewOrFromABaseAloneToADraft` pasa en el RED. La prueba de quitar un fondo compara la base consigo misma, y la de recuentos de referencias compara dos bases idénticas.
- **M-4.** La guarda tampoco detecta `ValueTask`, `lock`, `Monitor`, `Interlocked`, `PeriodicTimer`, `Console`, `Trace`, `Debug`, `Database` dentro de otro identificador ni `Commit (` con espacio. La guarda de «solo SessionId del namespace padre» no tiene control negativo.
- **M-5.** `ConflictDifferences` y `Warnings` guardan la lista del llamador por referencia. Además, los avisos de un Actualizar anterior sobreviven a uno fallido posterior.
- **M-6.** `RetireSession` marca la sesión como retirada antes de evaluar el comparador inyectado. Si este lanza, la sesión queda a medio retirar.
- **M-7.** `NeedsLazyReload` es verdadero en un borrador ya desligado.
- **M-8.** `PendingText` y la instantánea que recibe `Apply` se documentan ordenados, pero son un `Dictionary`.
- **M-9.** `Entries` expone el texto crudo del sobre.
- **M-10.** Es la misma observación que DR-02.

## Disposición

Regla de la orden del Owner (F4-E02): se corrige, gastando un intento, cuando un hallazgo incumple el Freeze, el contrato, la seguridad o el comportamiento observable. No se gasta por una preferencia cosmética.

| Hallazgo | Disposición | Motivo | Invariante nuevo |
|---|---|---|---|
| I-1 | CORREGIR | D-10: el inicial es el estado leído y la referencia de dirty; D-17 e INV-20: veto de cierre | INV-F4T1-19 |
| I-2 | CORREGIR | D-13: no puede haber un Actualizar mientras otro está en curso; si `Commit()` lanzó, Desconocido | INV-F4T1-20 |
| I-3 | CORREGIR | D-12: nunca aceptaciones falsas | INV-F4T1-21 |
| I-4 y M-4 | CORREGIR | INV-F4T1-16 e INV-F4T1-18: la guarda detecta cada familia prohibida | INV-F4T1-22 |
| M-2 | CORREGIR | D-12 punto 2 e INV-F4T1-06: solo el texto de miembros interpretables | INV-F4T1-21 |
| M-1 (origen), M-5, M-6 (orden), M-7, M-8 | CORREGIR | coherencia de estado y orden de D6, de coste bajo dentro de la misma corrección | INV-F4T1-23 |
| M-3 y M-10 | CORREGIR | INV-F4T1-03 e INV-F4T1-18: escenarios reales y guarda de preview duradera | INV-F4T1-24 |
| M-1 (salida de Conflicto por Error) | SIN CAMBIO | INV-25 e INV-F4T1-14: la contención lleva a Error con el borrador, y de Error se sale con DiscardAndReload o Discard | — |
| M-6 (veto ante una excepción del comparador) | ARRASTRAR a T2/T4 | es la conexión del veto en el host | — |
| M-9 | ARRASTRAR a T2 | guarda de que nada fuera de Drafts materializa estado editable desde la StaleBase | — |

## Obligaciones que pasan a tareas posteriores

- **T2:**
  - la decisión de veto en el host, ante una excepción, se inclina a Veto;
  - solo la lectura de la intención de edición llama a `DraftIntentRead.Admitted` y a `DraftStaleBase.Create`;
  - nada materializa estado editable desde `Entries` ni desde el texto de los sobres;
  - el lector aporta texto de sobre solo para miembros interpretables.
- **T4B:**
  - el host informa `CompleteApply` de cada Actualizar empezado, también ante una excepción (`finally`), porque la ranura solo se libera ahí;
  - una excepción contenida durante Aplicando llama a `EnterError`, que el núcleo difiere hasta `CompleteApply`.

## Límites

Sin AutoCAD. Este análisis no cambia la verificación `R20261110T184730Z-e455` ni los controles nc1..nc3, que se conservan.
