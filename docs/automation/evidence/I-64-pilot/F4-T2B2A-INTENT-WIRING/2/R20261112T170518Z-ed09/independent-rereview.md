# Re-revisión adversarial independiente — I-64 F4-T2B2A-INTENT-WIRING, corrección (intento 2)

- **Rangos revisados:**
  - corrección `36b497ad..56b0d497`: RED `568f845d` (4 archivos, todos de `tests/`) y GREEN `56b0d497` (4 archivos, todos de `src/`);
  - entrega completa `e277dd42..56b0d497`.
- **Worktree:** `architecture/workspace-persistente-rackcad`, leído en solo lectura (`git show`, `git diff`, `git log`, `git grep` y lectura de archivos). No compilé, no ejecuté pruebas, no abrí AutoCAD y no cambié nada. `git status` sigue limpio.
- **Autoridades leídas:**
  - `gate-contract-attempt2.json`: los 13 invariantes, con las correcciones de INV-01, 02, 06, 07, 08 y 09 y el INV-13 nuevo;
  - `analysis.md` e `independent-review.md` de `R20261112T155354Z-46e1`;
  - del paquete v5: §2 (H-01 a H-14), §3, §4, §5 (D-T2B2-09 a 14) y §6;
  - el código de `DraftSession`, `DraftRegistry`, `EventBridgeCore.OnIdle`, `AutoCadEventBridge.OnIdle` y `WorkspaceHostSourceSupport`, para la simulación.

## Veredicto

**NO ACEPTABLE.** Recuento: **0 BLOQUEANTE, 1 IMPORTANTE, 4 MENOR.**

**Lo que está bien.** La corrección cierra los seis puntos concretos de I-1 que listó la revisión anterior, y cierra M-2 a M-7 y M-9. El producto del GREEN es correcto, y el RED cumple INV-13 en lo que se puede verificar sin compilar.

**Por qué no es aceptable.** Queda una prueba nueva de esta tarea que prohíbe para siempre una parte de lo que el paquete asigna a F4-T2B2B: los estados de la recarga (N-1). Es el mismo tipo de defecto que I-1, y con el texto corregido de INV-09 es una violación directa. Si F4-T2B2B se contrata como dice el §5, su parada salta con certeza.

**Cómo se arregla.** El arreglo es pequeño y cabe entero en `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs`.

---

## Estado de los hallazgos anteriores

| Id | Severidad original | Estado | Comprobación |
|---|---|---|---|
| I-1 | IMPORTANTE | **PARCIAL** | Se cerraron los seis puntos listados (ver abajo). La regla de I-1 (INV-09 corregido) no se cumple del todo: `WorkspaceEditIntentHostTests.cs:1018` fija el conjunto exacto de `EditIntentStatus` e impide los estados de la recarga de D-T2B2-12 (N-1). Hacia F4-T3 quedan prohibiciones menores (N-5) |
| M-2 | MENOR | CERRADO | `WorkspaceEditIntentGuardTests.cs:688-713` prohíbe `.StaleBase`, `.Read.` e `InitialState` en el adaptador y en el host de navegación, con una teoría de detección sobre el texto real. Es la letra exacta de INV-08/M-2. Hoy, `.Read.` no aparece en ninguno de los dos archivos |
| M-3 | MENOR | CERRADO | `WorkspaceHostCore.cs:271` y `:348` (`HasWitnessWithoutKey`, `:401`) responden Failed antes de `PassGate`, sin tocar el lector ni el registro y con NotEvaluated. Hay pruebas en las dos operaciones con `null`, `""`, `"  "` y `"\t"` (`WorkspaceEditIntentHostTests.cs:589-635`). Queda un residuo inalcanzable en el adaptador (ver «Residuos») |
| M-4 | MENOR | CERRADO | `Read` lleva el «after» en un `finally` que va detrás del último `catch` (`RackPilotIntentAdapter.cs:137-141`). `ReadPreviewWitness` hace «before» antes de su `using` (`:153-154`) y «after» en un `finally` por fuera de los dos `using` (`:184-188`). Lo fija la guarda `:226-259`. El `finally` introduce una vía nueva de excepción en Debug (N-2) |
| M-5 | MENOR | CERRADO | El constructor recupera exactamente la firma original de `e277dd42`, de cinco parámetros. `RawEnvelopes` pasa a ser `{ get; init; }` y se asigna en el único `new RackSiblingDrawingScan(` (`RackSiblingScan.cs:166-169`). Si faltara, la lectura falla de forma tipada (`RackPilotIntentAdapter.cs:117-118`), y ya no se inventa un diccionario vacío. La guarda se renombró por lo que comprueba (`:80-110`) |
| M-6 | MENOR | CERRADO | Los tres puntos están corregidos: la cabecera ya no afirma pruebas de inyección inexistentes, el comentario de la guarda de hilos de T2B1 ya no dice «SUBSET», y el par (9,11) comprueba su premisa con el comparador de AUTH-13 (`WorkspaceDraftPilotIntentReadTests.cs:742-750`). La frase nueva de la cabecera sobre F4-T3 es inexacta (N-5) |
| M-7 | MENOR | CERRADO (lo contratado) | `Differences => _differences ?? Array.Empty<string>()` (`WorkspaceEditIntent.cs:211`), con una prueba que incluye `default` (`WorkspaceEditIntentHostTests.cs:1022-1043`). La segunda observación, que el cero de `EditIntentReadStatus` es `Read`, no se tocó: el contrato del intento 2 no la pide y el host la defiende (residuo aceptado) |
| M-8 | MENOR | REGISTRADO | La disposición fue registrarlo sin cambio. El RED de la corrección corre sobre producto existente y no necesita esqueleto |
| M-9 | MENOR | CERRADO | Hay una prueba de comportamiento: con un borrador de A y B activo, la intención de A da Rejected y no AlreadyOpen, con cero lecturas y el borrador intacto (`WorkspaceEditIntentHostTests.cs:562-585`). El comentario de `Rejected` documenta la rama SessionRetired. El comentario nuevo omite la tercera vía a Rejected (N-4) |

**Detalle de I-1: los puntos cerrados** (contra `independent-review.md` §1):
- `Assert.False(draft.IsPossiblyStale)` se eliminó de la prueba del drenaje (`:925-945`).
- La guarda del drenaje ya no incluye `_drafts` (`WorkspaceEditIntentGuardTests.cs:596` y `:624`). Solo prohíbe `TryBeginPilotEdit|TryCheckPilotEdit|_editIntentReader|ReadIntent|PassGate`.
- Los conteos de `ReadIntent` y `_editIntentReader` son por cuerpo (`:562-566`), no por archivo.
- La obligación 10 se fija como está contratada: Begin, CheckNow o DiscardAndReload. `DiscardAndReload` ya no está prohibido (`:631-686`).
- `IWorkspaceLifecycleAdvisor`, `AdviseClose`, `SessionDestroyed`, `EvaluateClose`, las palabras prohibidas en `EventBridgeCore`/`AutoCadEventBridge` y el conteo de `Drafts/` desaparecieron. La guarda del ciclo de vida se limita a los cuerpos de las dos operaciones (`:775-801`).
- Desapareció la condición «`Try(Begin|Check)PilotEdit(` solo en el core», que era el punto de F4-T3.

---

## Simulación de F4-T2B2B (paquete v5, §5) contra las pruebas y guardas nuevas

| Trabajo de T2B2B | Pruebas y guardas consultadas | ¿Rompe? |
|---|---|---|
| D-T2B2-09: `IWorkspaceLifecycleAdvisor.cs` nuevo; `WorkspaceHostCore` lo implementa con `AdviseClose = _closePolicy.EvaluateClose(...)` y `SessionDestroyed` con retirada y `DraftLossNotice` registrado en `_logFault` | `AtMostTheHostCore…NameThePilot` (la interfaz nueva no nombra el piloto); `TheThreeFilesOfTheApplication…` (las mismas reglas de carpeta que ya existían); `NoFieldOfTheHostCore…` (una cola de `DraftLossNotice` es del espacio `Drafts`, que está permitido); `TheBodiesOfTheTwoOperations…` (solo mira los dos cuerpos) | No |
| `SetLifecycleAdvisor` en `EventBridgeCore` y un `internal` que reenvía en `AutoCadEventBridge`; el controlador lo llama al crear el host | La guarda del Idle solo lee los cuerpos de `OnIdle` (`:599-602`), que T2B2B no toca. La guarda del controlador solo prohíbe `SetEditIntentReader\|EditIntentReader` en su constructor | No |
| D-T2B2-11: marcar en `OnDrained` antes de los retornos tempranos, con `RackIdsOf`, `Find<PushBackPilotDraftState>` y `MarkPossiblyStale()` | La guarda del drenaje solo prohíbe los nombres de esta tarea. `MarkPossiblyStale` no cambia `Status` (`DraftSession.cs:393-399`), así que `Assert.Equal(status, draft.Status)` (`:943`) sigue valiendo. `CheckNowHasTheSameGate…` no drena tras `CommandEnds` | No |
| D-T2B2-12: `TryReloadPilotEdit` con `PassGate`, una tercera `_editIntentReader!.ReadIntent(` y `draft.DiscardAndReload(result.Read)` | Los conteos son por cuerpo; `.ReadIntent(` sigue solo en el core; `AllowedConsumers` borra `.DiscardAndReload(...);`; `NotAdmitted` usa `reload.Admission`, `pilot.Detail` y `pilot.ClassicEditorHint`, que no casan con `BaseOrReadUse` | No |
| D-T2B2-12 y la tabla de D-T2B2-06, filas 6 y 8 de la recarga: `NotFound`, `NotNeeded`, `Reloaded` y `NotAdmitted` como estados de un **`EditIntentOutcome`** | `TheOutcomesAreValuesAndTheStatesHaveTheExactNamesAndOrderOfThePackage` (`WorkspaceEditIntentHostTests.cs:1018`) exige el conjunto **exacto** de siete nombres de `EditIntentStatus` | **SÍ (N-1)** |
| D-T2B2-10: operaciones explícitas en `DraftRegistry.cs` | Ninguna prueba nueva lee `Drafts/` | No |

**F4-T3** (exponer `TryBeginPilotEdit` y `TryCheckPilotEdit` a la UI):
- **Camino abierto.** La exposición desde el controlador no está prohibida.
- **Caminos prohibidos para siempre** (N-5, menor):
  - exponerlas por `IWorkspaceHostPort`;
  - nombrarlas en el host de navegación;
  - añadir una tercera clase en ese archivo.

  T3 puede declarar esos cambios de guarda (H-03), así que no es un bloqueo cierto.

---

## Hallazgos nuevos

### N-1. IMPORTANTE — La prueba del conjunto exacto de `EditIntentStatus` prohíbe para siempre los estados de la recarga de F4-T2B2B

- **Archivo y línea:** `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:1018`, dentro de `TheOutcomesAreValuesAndTheStatesHaveTheExactNamesAndOrderOfThePackage` (`:1012-1020`). La línea es:
  `Assert.Equal(new[] { "Unavailable", "Waiting", "Rejected", "AlreadyOpen", "NotAdmitted", "Admitted", "Failed" }, Enum.GetNames<EditIntentStatus>());`
- **Escenario.** El paquete v5 fija la firma de la recarga:
  - D-T2B2-12: `EditIntentOutcome TryReloadPilotEdit(EditIntentRequest)`;
  - tabla de D-T2B2-06, fila 6: «recarga: borrador ausente → `NotFound` o sin `NeedsLazyReload` → `NotNeeded`»;
  - fila 8: «recarga: `DiscardAndReload` → `Reloaded` o `NotAdmitted`».

  El estado de un `EditIntentOutcome` es un `EditIntentStatus`. Por eso T2B2B tiene que añadir `NotFound`, `NotNeeded` y `Reloaded` a ese enum, y por eso el §6 incluye `WorkspaceEditIntent.cs` en el `AllowedWriteScope` de T2B2B. En cuanto los añada, esta aserción falla.
- **Por qué T2B2B queda bloqueado:**
  - `WorkspaceEditIntentHostTests.cs` no está en el `AllowedWriteScope` de T2B2B (§6).
  - El §3 solo le declara dos cambios: un nombre en la lista pública de `EventBridgeCore`, y que ninguna prueba de F4-T1 cambie.
  - La parada «una guarda o prueba existente tendría que cambiar fuera de las declaradas» se activa con certeza, igual que con I-1.
- **Invariantes afectados:**
  - **INV-F4T2B2A-09 corregido:** «ninguna prueba ni guarda nueva prohíbe de forma permanente lo que el paquete asigna a F4-T2B2B (… la recarga con DiscardAndReload y su lectura …)».
  - **INV-F4T2B2A-13:** el GREEN no puede corregirlo, porque es un archivo de `tests/`.
- **Antecedente.** El hallazgo ya existía en el intento 1. La revisión anterior no lo listó, y la corrección no lo detectó al aplicar la regla general.
- **Corrección sugerida (solo en pruebas):**
  - fijar que los siete estados de la intención existen, en ese orden relativo, como prefijo del enum, por ejemplo con `Enum.GetNames<EditIntentStatus>().Take(7)`;
  - fijar que `default` es `Unavailable`;
  - no fijar el conjunto exacto.

  `EditCheckStatus` y `EditIntentReadStatus` pueden seguir exactos, porque el §5 no les añade estados.

### N-2. MENOR — El `finally` de M-4 abre en Debug una vía por la que una excepción escapa del contrato «fallo tipado, nunca una excepción»

- **Archivo y línea:**
  - `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:137-141` y `:184-188` (el `finally`);
  - `:195-201` (`AssertNoOpenTransaction` desreferencia `document.Database.TransactionManager` sin protección).
- **Escenario** (solo en Debug, porque el helper es `[Conditional("DEBUG")]`):
  1. Llega una llamada con `document` nulo, o con un documento cuyo `Database` ya no es utilizable.
  2. El «before», dentro del `try`, lanza una `NullReferenceException`, y la captura `catch (System.Exception)`.
  3. El `finally` vuelve a llamar al helper, y la excepción sale del método.
- **Diferencia con `2d743e46`:**
  - `ReadPreviewWitness(null, h)` daba un fallo tipado (la NRE de `LockDocument` se capturaba). Ahora lanza.
  - En `Read` pasa lo mismo.
- **Impacto acotado:**
  - el único llamador de `Read`, el lector anidado, pasa el documento activo no nulo y tiene un `catch` general (`WorkspaceNavigationHost.cs:208-219`);
  - `ReadPreviewWitness` todavía no tiene llamador; lo tendrá en F4-T3.
- **Por qué importa:** la validación manual con AutoCAD usa el DLL Debug (CLAUDE.md), así que Smoke-2 y T3 correrán este camino.
- **Invariante afectado:** INV-F4T2B2A-02, «captura … y devuelve un fallo tipado». Los tipos de resultado se documentan como «A typed failure, never an exception».
- **Arreglo:** que el helper no pueda lanzar, con `document?.Database?.TransactionManager?.TopTransaction == null` o con su propio `try/catch`.

### N-3. MENOR — La guarda de la obligación 10 reconoce los consumidores solo por el nombre del método y borra hasta el primer `);`

- **Archivo y línea:** `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:637-639`. La expresión es:
  `AllowedConsumers = \.(?:Begin|DiscardAndReload)\s*\([^;]*\)\s*;|\.CheckNow\s*\([^;]*\)\s*;`
- **Qué hace.** Antes de buscar `BaseOrReadUse`, borra todo lo que hay desde cualquier `.Begin(`, de cualquier receptor, hasta el primer `;` precedido de `)`.
- **Escenario.** En el core hay varios `using (_instrumentation.Begin(...))`. Si se inyecta
  `using (_instrumentation.Begin(WorkspaceMeasurementPoints.Drain)) { Keep(pilot.StaleBase); }`,
  el borrado se lleva `.Begin(…)) { Keep(pilot.StaleBase);`. El uso de la base deja de verse y la guarda pasa.
- **Qué no detecta, por tanto:**
  - la base pasada a `_instrumentation.Begin(...)`;
  - la base usada en la primera sentencia que acaba en `)` dentro de un bloque instrumentado.
- **Severidad.** Ningún escenario realista de T2B2B lo explota, porque el marcado no usa la base. Es una pérdida de precisión de una guarda de esta tarea, no un bloqueo.
- **Invariante afectado:** INV-F4T2B2A-08, obligación 10.
- **Arreglo:** acotar el receptor (`_drafts\.Begin`, `\w+\.CheckNow`, `\w+\.DiscardAndReload`) y la llamada (paréntesis equilibrados, sin cruzar `{`).

### N-4. MENOR — El comentario nuevo de `EditIntentStatus.Rejected` omite la tercera vía a Rejected y afirma un orden que en esa vía no se da

- **Archivo y línea:** `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:85-90`. El comentario dice que el rechazo por INV-22 «is decided before anything about the drafts and before any read of the drawing», y que la única otra vía es SessionRetired.
- **Escenario.** Hay una tercera vía:
  1. La preautorización del core pasa (`WorkspaceHostCore.cs:278`).
  2. El paso 6 consulta el registro (`:281`).
  3. El lector anidado, ya dentro del bloqueo, vuelve a autorizar con el id nativo y responde `Rejected` (`WorkspaceNavigationHost.cs:234-236`). Pasa, por ejemplo, si el documento activo cambió entre la llamada y el bloqueo, o si la sesión se destruyó en medio.
  4. El core lo traduce a `EditIntentOutcome.Rejected` (`WorkspaceHostCore.cs:292-293`).

  En esa vía el rechazo llega **después** del paso de los borradores y después de `LoadCatalog`, aunque antes de leer el dibujo.
- **Por qué importa.** Es justo el tipo de inexactitud que M-9 pedía corregir: un comentario que dice más de lo que es cierto.
- **Invariante afectado:** INV-F4T2B2A-06, la documentación de Rejected (M-9).

### N-5. MENOR — Quedan guardas nuevas que deciden de antemano cómo expone F4-T3, y la cabecera nueva dice que no las hay

- **Archivo y línea** (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs`):
  - `:551`: `IWorkspaceHostPort.cs` nunca puede nombrar `TryBeginPilotEdit|TryCheckPilotEdit|EditIntent`;
  - `:463`: `WorkspaceNavigationHost.cs` nunca puede nombrar `TryBeginPilotEdit|TryCheckPilotEdit|WorkspaceHostCore`;
  - `:435`: el host de navegación tiene exactamente dos clases;
  - `:16-17`: la cabecera afirma «what the package assigns … to F4-T3 (the exposure to the UI) is not prohibited here».
- **Escenario.** H-03 dice que T3 «decide su entrada desde la UI». Si T3 la expone por el puerto `IWorkspaceHostPort` o por el host de navegación, tiene que cambiar estas guardas además de las suyas. El camino del controlador sigue libre, y T3 puede declarar cambios de guarda, así que no es un bloqueo cierto.
- **Invariantes afectados:**
  - INV-F4T2B2A-09 corregido: «… ni la exposición de las dos operaciones en F4-T3»;
  - M-6: la cabecera afirma más de lo que se cumple.
- **Arreglo:** acotar `:463` y `:551` a lo que esta tarea necesita (que el puerto no cambie en T2B2A ya lo acreditan el diff y las guardas del puerto que existían), o corregir la cabecera y declararlas como arrastre a T3.

---

## Comprobaciones con resultado correcto

### 2. La corrección no debilita lo que sí es de esta tarea

**INV-22.**
- Las pruebas de A con B activo, del id nativo reciclado y de la sesión inexistente siguen intactas.
- M-9 añade la discriminación del orden compuerta → AlreadyOpen.

**Una sola lectura.**
- Por texto:
  - el conteo pasa a ser por cuerpo, 1 y 1;
  - `.ReadIntent(` sigue solo en el core (`:613-615`);
  - el drenaje y los dos `OnIdle` no nombran el lector ni las operaciones.
- Por comportamiento: `Assert.Single(rig.Reader.Calls)` sigue en las pruebas de la obligación 4.
- Lo que se pierde: el conteo de todo el archivo, que habría detectado un tercer `ReadIntent` fuera del drenaje, por ejemplo en `GetSnapshot`. Esta relajación la manda INV-09 corregido («sin conteos exactos sobre todo el archivo»), y no la clasifico como defecto.

**Orden.** Sin cambios: `AssertInOrder` del lector anidado, de las dos operaciones y de `PassGate`.

**Sin escritura.** La guarda del barrido se dividió, pero conserva la prohibición completa y el único `.Commit(` (`:112-120`). Las del adaptador no cambian.

**Testigo INV-27.**
- Las pruebas de `NotSupplied`, `Unchanged`, `Changed` y NotEvaluated siguen intactas.
- M-3 añade que un testigo sin clave nunca se evalúa.
- Ningún camino nuevo da `Unchanged`.

**Ciclo de vida.** Lo que se cumple:
- el diff de toda la entrega no toca `Drafts/`, `EventBridgeCore.cs`, `AutoCadEventBridge.cs`, `IWorkspaceHostPort.cs` ni `WorkspaceHostTestSupport.cs`;
- `Plugin/Workspace` sigue con 9 archivos.

Lo que no se cumple: el comentario `// Drafts do not exist in F1` de `WorkspaceHostCore.cs:186` ya es inexacto, porque desde T2B2A hay borradores en el host. Es texto anterior y T2B2B lo reemplazará con el marcado.

### 3. INV-13 (verificado por diff y por análisis estático; no compilé)

**Separación de los commits.**
- `568f845d` cambia solo `tests/`: los 4 archivos están en el `AllowedWriteScope`.
- `56b0d497` cambia solo `src/`: `WorkspaceEditIntent.cs`, `WorkspaceHostCore.cs`, `RackPilotIntentAdapter.cs` y `RackSiblingScan.cs`. Ningún archivo de `tests/` cambia después del RED.

**Fallo por aserción contra `2d743e46`:**
- **M-3:** el doble no valida la clave, así que el resultado es Admitted y falla `Assert.Equal(Failed, …)`.
- **M-4:** no existe `finally {`, así que falla `Assert.True(match.Success, "missing: …")`.
- **M-5:** fallan la expresión `{ get; init; }` y la del constructor de cinco parámetros que termina en `layers\s*\)`.
- **M-7:** falla `Assert.NotNull(unset.Differences)`.

**Compilación.** Ninguna de estas pruebas usa un miembro que no existiera en `2d743e46`, y `BodyOf` y `Code` fallan por aserción si falta el miembro.

**I-1, M-2, M-6 y M-9.** Pasan contra `2d743e46`, como permite INV-13.

**Límite de esta revisión.** No puedo acreditar que el resto siga en verde ni el build Release sin avisos nuevos, porque no ejecuté nada. El commit dice que la CI exacta está pendiente.

### 4. Los cambios de producto

**`init` de `RawEnvelopes` y firma original del constructor.**
- La firma es idéntica a la de `e277dd42`.
- `init` es válido con `LangVersion latest` y net8.
- `RackSiblingScan.cs` no tiene `#nullable` (en `Directory.Build.props` está `Nullable=disable`), así que no hay aviso CS8618.
- En el adaptador (`#nullable enable`), `scan.RawEnvelopes == null` compara un tipo oblivious, sin aviso.

**`finally` de `TopTransaction`.**
- En `Read`, el `finally` va detrás de los tres `catch` y cubre el retorno anticipado del testigo, la falta de `RawEnvelopes` y los `catch`.
- En `ReadPreviewWitness`, los dos `using` están dentro del `try`, así que el «after» corre con el bloqueo y la transacción ya cerrados.
- Es correcto, salvo N-2.

**Clave del testigo en el paso 1.**
- Va después del RackId en blanco y antes de `PassGate`, en las dos operaciones.
- Responde Failed con motivo y NotEvaluated, y no registra ningún fallo (no es una excepción).

**`Differences` nunca null.**
- Lo es también en `default`, y `Conflict` sigue rechazando null.
- `using System;` está presente para `Array.Empty`.

---

## Residuos de los menores

- **M-3:**
  - En el adaptador sigue la asimetría: con clave nula, `NotSupplied`; con clave en blanco, fallo tipado (`RackPilotIntentAdapter.cs:109-112`). Desde el core ya es inalcanzable, porque lo filtra el paso 1.
  - La documentación de `EditIntentStatus.Failed` (`WorkspaceEditIntent.cs:102`) sigue diciendo «A failure of the host during the operation», aunque ahora también cubre un error de la petición.
- **M-6:** el nombre `NoOperationOfTheIntentionIsCalledFromTheDrainOrTheIdleOfAnyFile` (`WorkspaceEditIntentGuardTests.cs:591`) promete «any file», pero comprueba cinco cuerpos concretos. La cabecera se trata en N-5.
- **M-7:** el cero de `EditIntentReadStatus` sigue siendo `Read`. Lo fija el orden del paquete y lo defiende el host (`WorkspaceHostCore.cs:299` y `:379`). El contrato del intento 2 lo acepta.
- **M-9:** el comentario nuevo de Rejected; ver N-4.

## Riesgos que se mantienen (no son defectos; vienen de la revisión anterior)

- **Bloqueos y transacciones:**
  - tres niveles de `LockDocument`;
  - el `Commit` del barrido, con DBMOD y UNDO;
  - el `Debug.Assert` dentro del bloqueo, que M-4 extiende a los caminos de fallo y a `ReadPreviewWitness`.
- **Coste y dependencias:**
  - dos recorridos de la BlockTable por operación;
  - «Comprobar ahora» depende del catálogo.
- **Para T3:**
  - `ReadPreviewWitness` no tiene compuerta propia;
  - el origen solo cuenta referencias de Model Space.

---

**Línea final:** NO ACEPTABLE — 0 BLOQUEANTE, 1 IMPORTANTE, 4 MENOR.
