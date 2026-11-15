# I-64 — F4-T4B-MUTATE-POST: paquete de cierre

Lo prepara la sesión responsable el 2026-11-15. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E60 y F4-E61.

**Autoridades del cierre:**
- La autorización de emisión del Owner (`F4-DRAFTS/coordinator/owner-f4t4b-emission-authorization.md`): emitir F4-T4B v7 con su §9 aprobado y ejecutar la cadena completa, como en T1B. Siguen vinculantes la disposición de reverificación de T1B y T4B, la vía propia del piloto sin la costura gruesa de I-55, «Guardas exactas en T4B» y las reglas de F4-E59.
- La disposición del Owner antes del cierre (`F4-DRAFTS/coordinator/owner-f4t4b-close-decision.md`): I-01 «Ratificar + a T4C», nc2 «Sustituto + deuda» y las decisiones y los menores «Ratificar todo».

Este paquete cierra una subtarea, no un gate. No declara GATE PASS. F4-T4B da al piloto de Push Back su primera escritura authored: MUTATE y POST del Actualizar por su vía propia. **No tiene todavía llamador de producto, contexto de comando ni `LockDocument`**: eso es de F4-T4C. Su primera validación física es Smoke-2.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `4314c1803b7a047f0521c34d631950a475e7cd32`; CI `43668817337` push 4/4 en success |
| Verificación | `R20261115T035009Z-45f0`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha = BaseSha `3633f08d` (contrato); RED `f4476bc4` (esqueleto inerte y pruebas); GREEN `4314c180` |
| Alcance | 21 de las 23 rutas exactas del AllowedWriteScope; ninguna del prohibido; `git diff RED..GREEN -- tests` vacío |
| Contrato | `F4-T4B-MUTATE-POST/coordinator/gate-contract.json` (AuthorityRevision `56041cd0`, el paquete v7) |
| Intentos | 1 de 3 |
| Config | Snapshot I (`0B47701A…`) en todos los relevos; binario de Codex acreditado `5449c1f0` |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E61; las limitaciones en los §5 a §7) |

## 2. Corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `93b8` (planificación) | aceptada a la primera | Comparación byte a byte PASS en las 8 colecciones, A1-A8 8/8. La nota de la sesión sobre AcceptanceCriteria[12] (sobres en pruebas, lectura conservadora) va en el prompt del Worker |
| `df36` (nc4) | oráculo cumplido | Solo A3 en fail con la copia que amplía AllowedWriteScope con `src/RackCad.UI/` |
| `7a4a` (Worker) | IMPLEMENTATION_COMPLETE | `claude-sonnet-5-5`/high, 433 turnos y 0 denegaciones. **RED** `f4476bc4` y **GREEN** `4314c180`, con push |
| `45f0` (verificación) | `EXECUTION_VERIFIED` / NONE, 14/14 | Sin hallazgos |

**CI exacta:**
- **RED** `43667312134`: Core (`tests`) en failure, UI (`ui-tests`) y build-ui en success, y build-plugin omitido por su `needs`.
  - Core: 132 fallos de 18684, todos en el filtro de Workspace (3103 seleccionadas), por aserción y de pruebas nuevas o cambiadas. Lo midieron la sesión (`_trx_check_t4b.py`) y la revisión independiente:
    - `WorkspacePilotUpdate*`: 119;
    - `WorkspaceDraftPilotOwnershipTests`: 4, la `Theory` dividida;
    - `WorkspaceEditIntentGuardTests`: 3;
    - `WorkspaceLifecycleGuardTests`: 3, por `PortMutatingOperations` y `ChangesADraft`;
    - `WorkspacePilotEditGuardTests`: 1, por `InlineData(3)`;
    - `WorkspacePilotPrepareTests`: 2, M-5 y M-6.
  - Las 21 clases de regresión pasan, y los dos filtros de UI también: 601/601 y 55/55.
- **GREEN** `43668817337`: los cuatro jobs en success. Filtros: Core 3103/3103 y UI 601/601. Suites completas: Core 18684/18684 y UI 2293, con las 17 omitidas de la línea base.
- **Relevo de entrada:** al terminar, el Worker hizo `git restore --source=HEAD -- src` y dejó cinco archivos con CRLF en disco y LF en el índice, sucios solo por stat. La sesión comprobó con `hash-object` que el contenido es idéntico y volvió a añadir los mismos blobs. El árbol del índice sigue siendo el de HEAD (`0cb46ccc`).

## 3. Revisiones y controles

- **Revisión directa** (`45f0/direct-review.md`): conforme. Tiene 3 menores (DR-m1..DR-m3) y 5 observaciones (DR-O1..DR-O5).
  - Comprueba con `_check_traverse_literals.py` que los literales de `TraverseBaseline` son los de `c3ae1c97`.
  - Comprueba con `_trx_check_t4b.py` el criterio del RED.
  - Los dos scripts se custodian con ella.
- **Revisión adversarial independiente** (`45f0/independent-review.md`): **ACEPTABLE CON MENORES (0/1/10)**. La importante, I-01, la dispuso el Owner (§5).
- **Análisis de la sesión para el Owner:** `45f0/analysis.md`.

**Controles** sobre las entradas de `45f0`, conservando cada salida ordinaria:

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261115T035939Z-880b` | **CONTROL PASS**: Identity primera en fail, con `EXECUTION_BLOCKED` / STOP (S-04) |
| nc2 (sin la primera ruta del diff en el alcance) | `R20261115T040533Z-855b` | **CONTROL NOT PASSED**: `EXECUTION_VERIFIED` 14/14, con Scope y Contract en pass ante la lista de 22 entradas sin `src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs`. El Controller imprimió las dos listas y no las comparó. **No es la firma de PD-F4-01.** Se conserva tal cual |
| sustituto de nc2, `F4T4B-NC2-EXACT-WHITELIST-01` | `R20261115T042006Z-dba3` | **PASS**: 10/10 propiedades y 17/17 autopruebas deterministas. Es el clon de F4T1B-NC2-EXACT-WHITELIST-01 extendido por el Owner a la firma PD-F4-07: la propiedad 9 exige Scope y Contract en pass, `EXECUTION_VERIFIED` y las mismas comprobaciones que la real; los casos G y O a Q de la autoprueba la discriminan. nc2 = SATISFIED_UNDER_F4T4B_EXACT_WHITELIST_SUBSTITUTE |
| nc3 («GATE PASS» en `WorkCompleted`) | `R20261115T041135Z-fb91` | **CONTROL PASS**: FreeText única en fail, con `EXECUTION_REWORK_REQUIRED` / REWORK |

- **Participantes:** 0 en todas las comprobaciones de salida y de entrada de la cadena.
- **Avisos de límite** en los registros de nc2 y nc3: los «limit reached» que cuentan son el texto de la parada S-11 que el Controller leyó de AUTOMATION_PLAN; no son avisos del CLI.

## 4. Lo que entrega F4-T4B

- **Application:**
  - `IPushBackPilotUpdatePort` por ámbitos de corrida, con `IPushBackPilotUpdateScope` e `IPushBackPilotMutationScope`, y los tipos de valor de D-T4B-02;
  - `PushBackPilotUpdateRun`, pura y total: la comprobación previa; el origen elegido en puro; los supervivientes y las capas antes de importar y en MUTATE; la relectura como autoridad; un `WriteUnit` por miembro; `Seal`; POST con los nombres por vista del clásico; el comparador ordinal sin normalizar; la tabla de frontera con las filas del aborto; los dos `finally`; la precedencia y sus dos salvedades;
  - `PushBackPilotSystemRebuild`;
  - el plan con `Envelope(key)`, M-5 y M-6;
  - `CompleteApply` por rama en `DraftSession`, con el bloque del error diferido idéntico;
  - `AppliedPostRereadFailed(cause, warnings)`;
  - `EditUpdateStatus`/`EditUpdateOutcome`;
  - `WorkspaceHostCore.TryUpdatePilot`, con su puerta propia y sin llamador de producto.
- **Plugin:**
  - `RackPilotUpdateWriter`, en tres clases y con operaciones primitivas: una transacción poseída; `Reread` por el núcleo; `WriteUnit` por las fachadas; `Seal`; y `Post` en el orden TopTransaction → la purga (b) una vez → `SyncName` → la lectura de nombres de solo lectura → el regen → `CaptureDrawing`;
  - la inyección Debug por métodos `[Conditional("DEBUG")]`;
  - en `RackSiblingScan.cs`, el núcleo `internal static TraverseCore`, movido sin cambio de comportamiento.
- **Pruebas:**
  - cuatro archivos nuevos: la corrida, el host con las filas de `CompleteApply`, las guardas con el literal de `c3ae1c97` embebido y el doble del ámbito;
  - los cambios de guarda declarados en D-T4B-06 a D-T4B-09.
- **Contención (INV-F4T4B-18):**
  - en local, solo los filtros de Core (Debug y Release), la suite completa de Core en Debug y los dos filtros de UI en Debug; ninguna suite completa de UI;
  - las pruebas nuevas no escriben fuera de memoria ni usan la configuración real.

## 5. Adenda de la sesión (disposición del Owner)

**I-01, «Ratificar + a T4C»: desviación declarada de D-T4B-07, «Textos».**
- `WorkspaceHostCore.cs` gana tres textos que el apartado no prevé, y la entrega no los declaró como desviación (`Deviations` vacío):
  - la constante `UpdateNotRunReason` (`:48`), la causa de respaldo de `:852`, `:861` y `:870`;
  - los motivos de `UpdateGate` sin documento activo (`:915`) y con un modal abierto (`:921`).
- La justificación de WorkCompleted («no alterar la tabla de 30 literales») no se sostiene: esa tabla solo cuenta sus 30 textos.
- El comportamiento no cambia, y la guarda de los 30 literales pasa.
- **Arrastre a F4-T4C:** los tres textos pasan a constantes de `PushBackPilotUpdateRun`, y la guarda de textos se extiende a `UpdateGate` con su autoprueba en negativo.

**M-08, la evidencia de la entrega, completada** (medida de la revisión independiente):
- `TestsExecuted` omite tres corridas locales, todas de Core y dentro de INV-F4T4B-18:
  - `g0`: 2923 seleccionadas y 7 fallos;
  - `g1`: 18678;
  - `r1`: 18684 y 130 fallos, un estado del RED anterior a su commit.
- La lista «Guardas y pruebas que pasan ya en el RED»:
  - omite cinco métodos nuevos o cambiados que pasan en la CI del RED:
    - `TheNoReadGuardOfTheUpdateDetectsEachWayToReadInTheRealBody`;
    - la fila nueva de `TheDrainGuardDetectsACallOfTheIntentionInTheRealTextOfTheDrainOfTheHostCore`;
    - `NoOperationOfTheIntentionIsCalledFromTheDrainOrTheIdleOfAnyFile`;
    - `TheSignaturesOfTheTwoOperationsAndTheirValuesNameNoTypeOfADraftNorOfThePilot`;
    - y la de `TheUpdateOfThePilotIsAPublicMethodOfTheCore…`;
  - y da por pasada `TheGuardOfTheWrapperTripsOnEachChangeOfTheWrapper`, que tiene 2 de sus 5 filas en rojo.
- La medida de la base en Debug («0 avisos y 0 errores») no tiene registro conservado. El GREEN en Debug, sí.

## 6. Decisiones e interpretaciones ratificadas por el Owner («Ratificar todo»)

- **DR-O1 / M-04.** Si el cálculo de los nombres deseados lanza, POST sigue, con el aviso `DesiredNamesFailedPrefix`, y puede terminar en AppliedWithWarnings. Los nombres no tocan el contenido authored, que la relectura verifica byte a byte.
- **DR-O2 / M-04.** Una importación con `Ok = false`, o nula, es NotAppliedPreflight con su causa. Una lectura o un inicio de MUTATE nulos dan NotAppliedFailure. El residuo OM-5 sigue declarado.
- **M-02.** El registro de TopTransaction «al salir» (§9 I-7) no existe. Lo sustituye la clasificación AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`) de `Post`.
- **M-03.** Cualquier `Status == Error` tras la corrida, también un error diferido sobre un Applied o un Conflict, se responde como ClosedInError, sin la clasificación, los avisos ni las diferencias. Que T4C muestre también la clasificación queda como candidato.
- **M-10.** La letra de INV-14:
  - el censo de portadores se amplía por reflexión, sin autoprueba negativa;
  - la contención como Failed va en guardas nuevas y no como cambio de `WorkspaceEditIntentGuardTests.cs:583-596`;
  - hay una prueba de reflexión nueva, sin declarar e inocua.
- **nc2, «Sustituto + deuda».** Ver el §3 y la deuda PD-F4-07 de F4-E61.

## 7. Limitaciones y arrastres

**A F4-T4C:**
- I-01 (el §5);
- M-01: la totalidad defensiva. El `catch` exterior de `Execute` y `!closed.Accepted` dan «no aplicado» sin mirar si `Seal` volvió. Hoy es inalcanzable;
- M-03: mostrar la clasificación de un ClosedInError;
- M-06: pruebas más fuertes, a saber:
  - la causa exacta;
  - la escala sola;
  - el caso compuesto y el posterior en los nombres deseados;
  - renombrar o completar `TheAnswerOfAnUpdateAlwaysComesFromTheClose…` y `…CopiesOfTheLists`, con `EditUpdateOutcome.Closed` copiando al recibir (DR-m3).

**A ideas futuras:**
- M-05: el comparador se evalúa dos veces en `CompleteApply`, y la rama CommitThrew de `Recover` es inalcanzable;
- M-07: endurecer las guardas, a saber:
  - la fila de la negativa del resto que cae en silencio en el respaldo;
  - «el núcleo justo después de `Traverse`» sobre el texto crudo;
  - el censo ante un grupo de métodos;
  - la normalización dentro de los literales;
- M-09 / DR-m1: los espacios perdidos en `PrepareFailurePrefix` y `NotKeepableReason`, y las líneas en blanco del `CoreLiteral` sin efecto.

**KnownLimitations de la entrega:**
- un Conflicto no lleva causa: la excepción de un `Dispose` exterior se descarta, como salvedad declarada;
- el residuo OM-5 y la purga que falla quedan declarados y no se observan;
- el escritor del Plugin solo lo cubren las guardas de fuente y la compilación;
- solo el Plugin Release se compiló contra las referencias reales de AutoCAD; la CI las verifica con su script.

**Smoke-2** (§7 del paquete, después de T4C):
- los bytes y los handles;
- una transacción y `TopTransaction == null`;
- las tres lecturas;
- R-09;
- el Undo de la importación;
- S2-R-10 con `SEAL=before` y `SEAL=after`;
- la capa bloqueada;
- la purga;
- los cuatro llamadores del barrido (A, B, C y D).

**Deudas:**
- **PD-F4-07**, nueva (F4-E61): el Controller puede dar Scope y Contract en pass ante una lista blanca recortada;
- **H-1** sigue abierta: la suite de UI existente escribe en el perfil real.

## 8. Siguiente

1. El paquete de F4-T4C (§8 del paquete de T4B). Incluye:
   - R-T4-05 a R-T4-10;
   - la operación del puerto `IWorkspacePilotEditPort`, con la puerta de reposo;
   - el cableado de la instancia del puerto, con su cambio de guarda declarado;
   - la llamada de la UI;
   - el diagnóstico de `TryCheckNow`;
   - IC-08;
   - los arrastres del §7.
2. Smoke-2, después de T4C.
