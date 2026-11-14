# I-64 — Revisión independiente adversarial del paquete de F4-T4B-MUTATE-POST (v1)

**Objeto:** `I-64-f4-t4b-gate-contract-package.md` v1, con los mapas M1, M2 y M3 como insumo, no como verdad.

**Base medida:** `c3ae1c970c908ef541840fc583680b86376cae0b`, en solo lectura (lectura de fuentes y `git`). No se compiló ni se ejecutó ninguna prueba.

**Rutas:** relativas a la raíz del repositorio. `T4P`, `A2P` y `FRZ` como en el paquete. «Paquete» = el paquete v1.

## Veredicto

**NO EMITIBLE (v1).** Recuento: **4 bloqueantes, 8 importantes y 14 menores.**

- Las precondiciones de emisión del propio paquete ya lo bloquean: T4A2 está en STOP y T1B no se ha ejecutado. Pero, aunque se cumplieran, el diseño tal como está escrito provoca un STOP o un REWORK previsible.
- Tres bloqueantes son de forma (alcance y guardas). El cuarto (B-3) obliga a rediseñar el puerto, que es el centro del paquete.
- Todos los hallazgos llevan una corrección concreta. Recomiendo una v2 y una revisión focalizada de esa v2 antes de emitir.

---

## Bloqueantes

### B-1. «`Operations` (15 → 16)» exige tocar archivos que el alcance no permite o prohíbe

**Evidencia:**
- `Operations` son las operaciones de la **interfaz** `IWorkspacePilotEditPort` (`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:42-61`).
- Cada operación tiene que estar implementada explícitamente en el host (`:236-240`), y `WriteViolations` exige además el cuerpo en la compuerta (`:695-711`).
- La interfaz tiene cinco implementadores:
  - `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:153` (`WorkspacePilotEditGate`);
  - `src/RackCad.Plugin/Workspace/WorkspaceController.cs:278` (`PilotEditAdapter`);
  - `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:17` y `:1107`;
  - `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25`.
- El §6 del paquete no admite `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs` ni `tests/RackCad.UI.Tests/`, y prohíbe `src/RackCad.Plugin/Workspace/`. El filtro de UI declara `ExpectRed` false «sin cambios de UI».

**Problema:** una operación 16 rompe la compilación de UI.Tests y del Plugin, o cae en un STOP de Scope o de C03. Además contradice «sin llamador de producto hasta T4C» (§3).

**Corrección:**
- `TryUpdatePilot` es **solo un método público del núcleo** (como `TryConfirmPilotBase`) y **no** una operación del puerto.
- Se borra «`Operations` (15 → 16)». La operación del puerto, la compuerta, el adaptador del controlador y los dobles pasan a T4C, junto con la UI.
- Se declaran los censos de cuerpos públicos donde sí entra:
  - `PortMutatingOperations` (`tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:463-474`);
  - un cuerpo de la guarda de escrituras como `ReconciliationBodies` (`WorkspacePilotEditGuardTests.cs:686-691`, adaptado: el cuerpo **sí** delega una escritura en el puerto, pero no nombra ninguna API de escritura);
  - la contención como Failed (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:583-596`);
  - y nada desde el drenaje ni desde el Idle (`:599-640`).
- Si el Owner prefiere la operación en el puerto ya en T4B, hay que meter en el alcance los cinco implementadores y UI.Tests, y cambiar el `ExpectRed` de UI.

### B-2. El puerto de actualización no puede ser un campo del host: una guarda no declarada lo prohíbe

**Evidencia:**
- `NoFieldOfTheHostCoreIsOfATypeOfThePilotOrOfTheResultOfTheReadAndTheFieldOfTheReaderIsNotCalledReader` (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:1002-1020`) rechaza **cualquier** campo del núcleo cuyo tipo esté en `RackCad.Application.Systems*`.
- `IPushBackPilotUpdatePort` vive en `Systems/PushBack/Pilot/` (D-T4B-02), y D-T4B-07 lo inyecta «como `SetEditIntentReader`», es decir, en un campo (precedente: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:73`).
- El constructor tampoco puede cambiar (`WorkspaceEditIntentGuardTests.cs:555-559`).
- El paquete cita esa misma guarda solo para el plan (D-T4B-07), y no la lista entre los cambios declarados.

**Problema:** el GREEN rompe una guarda vigente sin declararla: STOP por C03.

**Corrección (elegir una y declararla):**
- **(a)** El puerto entra como **parámetro** de la operación: `TryUpdatePilot(EditIntentRequest, IPushBackPilotUpdatePort)`. No hay campo, y el adaptador de T4C en `Plugin/Views` lo pasa.
- **(b)** Un cambio declarado de la guarda: exactamente un campo `IPushBackPilotUpdatePort?`, con su autoprueba en negativo (un segundo campo, u otro tipo de `Systems`, la hacen saltar). El setter nuevo, con su propio censo, como el de `SetEditIntentReader` (`WorkspaceEditIntentGuardTests.cs:500-502`).
- **(c)** La interfaz se declara en `WorkspaceEditIntent.cs`. Es uno de los tres archivos que pueden nombrar el piloto (`:509-520`), pero su texto no puede contener `Transaction`, `Database` ni `Commit(`, ni siquiera en comentarios (`:1002-1015`), y la corrida nombraría un tipo `IWorkspace*`.
- Recomiendo (a) o (b).

### B-3. La granularidad del puerto deja sin oráculo de Core las invariantes del Freeze y obliga a guardar objetos del host entre llamadas

**Evidencia:**
- El Freeze pide oráculos **de Core**:
  - INV-06: «Core con lector y escritor simulados» (`FRZ:744`);
  - INV-12: «Core con escritor transaccional simulado: fallo en la unidad k → cero escrituras» (`FRZ:750`);
  - INV-30: «Core: excepción antes de `Commit()` → No aplicado; en `Commit()` → Desconocido» (`FRZ:768`).
- En el paquete, `Mutate(plan)` hace **dentro del Plugin** la relectura, `CompareWith`, el destino, C-F4-05, las unidades y el `try` de `Commit()` (D-T4B-02 (3) y D-T4B-03, pasos 1 a 7). Core no carga el Plugin (ADR-0003; `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:24-27`).
- Por tanto, las pruebas de Core solo verían que «el doble devolvió `Failure` y la corrida devolvió `NotAppliedFailure`». Es una tautología: la decisión real queda cubierta solo por guardas de fuente.
- **Estado entre llamadas:**
  - `Import` produce `PreparedViewRedraw` (con `BlockId`, el drawer, el plan y el catálogo; `src/RackCad.Plugin/Systems/Shared/PreparedViewRedraw.cs:22-49`), que tiene que sobrevivir hasta `Mutate`;
  - `RedrawInTransaction` devuelve `staleDefinitions` (`src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:97-109`), que necesita `PurgeAfterCommit` en `Post` (`SystemBlockWriter.cs:147-150`);
  - `Import(plan)` no recibe el catálogo de `ReadForPrepare`.
  - Con un escritor inyectado una vez, esos objetos de AutoCAD viven en campos entre llamadas. Eso contradice INV-F4T4B-08 («los `ObjectId` … solo viven dentro de la operación del escritor») e INV-14 (`FRZ:752`).
- **Excepciones que escapan:** no se dice qué significa una excepción que sale de `Mutate` o de `Post`. Una que sale de `Mutate` no prueba que `Commit()` no corriera. D-13 dice «`Commit()` lanzó → Desconocido» y «nunca se supone aplicado» (`FRZ:526`, `:532`).

**Corrección:** un puerto **con forma de ámbito por corrida**, como el precedente `IRackProjectionWriteScope` / `RackProjectionMaterializationRun` (`src/RackCad.Application/Views/Placement/RackProjectionMaterialization.cs:153-168`, `:228-303`).
- `Begin(...)` abre un ámbito `IDisposable` con operaciones de valores: `ReadForPrepare`, `Import`, `BeginMutation()` y `Post`.
- `BeginMutation()` da un ámbito de mutación con:
  - `Reread()` (hechos y textos crudos, como valores);
  - `WriteUnit(key)` (una por miembro, que devuelve las piezas faltantes);
  - `Confirm()` (el commit);
  - y `Dispose` = abortar.
- **La corrida pura decide:** `BuildStaleBase` + `CompareWith` (INV-06), el destino, C-F4-05 sobre los hechos (`IsSurvivor`), el bucle de unidades (INV-12) y `Confirm()` en su propio `try` (INV-30).
- **Clasifica por frontera:**
  - excepción antes de `Confirm` → `NotAppliedFailure`;
  - en `Confirm`, o que escapa después de llamarlo → `CommitThrew`;
  - en `Post` → `AppliedPostRereadFailed`, nunca No aplicado.
- **Los dobles prueban de verdad** INV-06, INV-12 e INV-30. El escritor solo guarda `ObjectId` dentro del ámbito, y los suelta en `Dispose`.
- **Las guardas que acompañan el cambio:**
  - la guarda de INV-14 (transacción y bloqueo solo en `using`, y ningún campo `Transaction`, `DBObject`, `Document` u `ObjectId` fuera del ámbito) se extiende a `RackPilotUpdateWriter.cs`;
  - la guarda de pureza de la corrida admite el nombre de confirmación del puerto. Mejor no llamarlo `Commit`, por la regla `\.\s*Commit\s*\(` copiada de `WorkspacePilotPrepareGuardTests.cs:39`.

### B-4. `ReadForPrepare()` no tiene entradas, y la lectura que necesita no se puede hacer desde `Plugin/Views` como hoy

**Evidencia:**
- La admisión exige el `OriginKey` «chosen by the host» (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentScan.cs:84-85`). La regla 2 da OriginMissing sin él (`PushBackPilotIntentReader.cs:163-166`).
- Hoy el origen y la autorización de la sesión salen de `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:232-248`: `_authorize`, `_targets.Revalidate` y `ChooseOriginDefinition`.
  - Ese archivo está en `ForbiddenWriteScope`.
  - `NavigationTargets` no se puede nombrar fuera de las carpetas del panel (`tests/RackCad.Tests/Workspace/WorkspaceNavigationPrivacyGuardTests.cs:14-42`, `:73-83`).
- El host tampoco puede pasarle el texto del origen: nunca nombra `.InitialDraftState` (`WorkspaceEditIntentGuardTests.cs:712`).
- El paquete no dice qué documento (¿el activo?, ¿el autorizado?), qué rack ni qué testigo usa la lectura. Tampoco dice cómo se mantiene la promesa de D-T4A2-06: «la admisión de Actualizar … lo rechaza» (`A2P:349`).

**Problema:** el Worker tendría que inventar una decisión material (la elección del origen y el enlace con el documento) o tocar `Plugin/Workspace`. En los dos casos es STOP o REWORK.

**Corrección:**
- **La firma, de valores y llamada por la corrida**, que tiene `input.InitialDraftState.Origin`: `ReadForPrepare(long authorizedDocumentNativeId, string rackId, string originText)`.
- **La regla declarada:**
  - el escritor toma como `OriginKey` el **único** miembro Mutable cuyo texto crudo es, byte a byte, `originText` (la misma regla que Build (b), `PushBackPilotUpdatePlan.cs:342-345`);
  - con cero o con varios, NotAdmitted o Preflight, con su causa;
  - el escritor comprueba `document.UnmanagedObject.ToInt64() == authorizedDocumentNativeId` antes de leer (el host ya autorizó en `PassGate`).
- **Al §9:** esta elección del origen difiere de la de «Comprobar ahora» (referencia elegida + `NavigationTargets`), y R-T4-01 solo exige la equivalencia de la StaleBase.
- **Alternativa:** leer por el lector del host. Exige declarar D-1 (6 → 7), `PilotForms` para `Prepare(input, pilot, catalog)` y el catálogo, que `EditIntentReadResult` no lleva (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:34-40`).

---

## Importantes

### I-1. «Aplicado con avisos» no es observable con el diseño

**Evidencia:**
- **Las piezas faltantes:**
  - `ViewBlockDraw.PrepareRedraw` descarta el resultado de la importación (`ViewBlockDraw.cs:86`), y `EnsureForPlan` solo devuelve el número de importadas (`src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:25-39`);
  - las faltantes salen en MUTATE, en `LateralHeaderDrawOutcome.MissingInstances/HasMissingBlocks` (`src/RackCad.Plugin/Drawing/LateralHeaderDrawOutcome.cs:31-33`), que devuelve `RedrawInTransaction`;
  - el paquete las pone en `Import` (D-T4B-02 (2)), y el `Committed` de `Mutate` no tiene canal de avisos.
- **`SyncName` y la purga** devuelven `void` y se tragan todo: `src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:21-58` (también se salta en silencio un nombre ocupado, `:42-45`) y `LateralHeaderDrawer.cs:193-238`.
  - «Purga o `SyncName` fallidos» (§9 I-5, POST de D-T4B-03) no puede dar avisos sin tocar archivos fuera del alcance.
- D-13 exige «avisos visibles en el panel, no solo en el registro» (`FRZ:528`).

**Corrección:**
- las faltantes salen del resultado de cada unidad (`WriteUnit` o `Committed` con avisos);
- `SyncName` se verifica releyendo el nombre de la definición en POST, y un nombre distinto da un aviso; un `Regen` que lanza también da un aviso;
- **o** se declara al Owner (§9) que los fallos de `SyncName` y de la purga solo van al registro, como limitación de la fila de D-13.

### I-2. La corrección de F4-E25 (4) generaliza Unknown(PostReread) a todas las ramas

**Evidencia:**
- D-T4B-06 dice: «si el copiador o el comparador lanzan, el borrador queda en Unknown(PostReread)».
- El comparador se evalúa en `Status` en **todas** las ramas de `Classify` (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:298-300`; `:564`, `:570`, `:576`, `:588`, `:597`, `:604`) y en `ClosedDetached` (`:540`).
- Unknown(PostReread) se presenta como «Aplicado; recarga para seguir editando» (A2-12 de `A2P` D-T4A2-06; `FRZ:529`).

**Problema:** un NotApplied o un `CommitThrew` con un comparador que lanza diría «aplicado». Eso contradice «nunca se supone aplicado» (`FRZ:532`).

**Corrección:**
- **por rama:** solo Applied y AppliedWithWarnings (con la escritura confirmada) van a Unknown(PostReread) con causa;
- **en las demás**, se conserva la clasificación, y si el cálculo del estado lanza se usa un respaldo definido (por ejemplo Error con `ErrorReason`, según R-3, o mantener Unknown(CommitThrew) en su rama);
- una prueba por fila;
- si el respaldo es un estado nuevo para D-13, va al §9.

### I-3. El alcance es incoherente y tiene declaraciones condicionales que provocan STOP falsos (precedente de esta cadena)

**Evidencia y corrección:**
- **`Workspace/WorkspaceController.cs`** figura en `AllowedWriteScope` como de Application.
  - El archivo real es `src/RackCad.Plugin/Workspace/WorkspaceController.cs`: está bajo la raíz prohibida `src/RackCad.Plugin/Workspace/` y bajo la prohibición de escritura de archivo entero (`WorkspaceLifecycleGuardTests.cs:30`, `:672-678`).
  - Se quita: con B-1 no hace falta.
- **«El resto de la costura de I-55» (prohibido) frente a `RackSiblingScan.cs` (permitido):**
  - `RackSiblingScan.cs` es el barrido de Insertar de I-55 (`src/RackCad.Plugin/Views/RackSiblingScan.cs:76`; lo usa `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs:81`), y C02 para si «haría falta tocar la costura de I-55»;
  - se enumera la costura por ruta exacta: `src/RackCad.Plugin/Systems/Shared/{AutoCadSiblingRedrawPort,SiblingRedrawTransaction,SiblingRedrawUnits,SiblingRedrawDebugFaultInjection}.cs`, `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs` y `src/RackCad.Application/Views/Redraw/**`;
  - y se dice expresamente que `RackSiblingScan.cs` está permitido.
- **«La clausura de T1B (… entre otros archivos)»** no es exacta: se usan las raíces G1 a G6 y los fragmentos compartidos del paquete de T1B.
- **«`AllowedConsumers` y `PilotForms`, solo si hace falta»** y **«si la inyección lo pide»** son declaraciones condicionales, frente a INV-F4T4B-14 y C03.
  - En el diseño del paquete el host nunca nombra `pilot`, así que no hacen falta: se quitan y se fija la lista exacta de guardas que cambian, por prueba y archivo.
- **M-1 «El mapa de evidencia se corrige»:** está en `docs/`, que es prohibido. Se dice que lo hace la sesión en la custodia, no el Worker.
- **C01 («un bloqueo de documento») e INV-F4T4B-12 («sin bloqueo … en el host»)** son ambiguos.
  - Se declara la política del escritor: no toma `LockDocument`, porque el `using` único es del adaptador de T4C (R-T4-06). Los bloqueos anidados de `CaptureDrawing` y `SyncName` ya existen.
  - «Host» debe decir `WorkspaceHostCore` o las carpetas Workspace.
- **Faltan explícitamente:**
  - `tests/RackCad.UI.Tests/`, los `.csproj` y `tests/RackCad.Tests/Fixtures/` en el prohibido;
  - en el permitido, «los dobles de Core» por ruta, y `tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs` si se amplía el rig.

### I-4. `RequiredTests` no selecciona lo que prometen los invariantes, y el filtro 2 choca con la guarda de INV-39

**Evidencia:**
- **El filtro 2 tiene `ExpectRed` false**, pero D-T4B-09 pone la guarda de la variante de lectura en `CallerOwnedFacadeGuardTests` (en «Extendidas»). Esa guarda está en rojo en el `RedSha`.
- **Clases que ningún filtro selecciona:**
  - `DebugFaultInjectionGuardTests` (`tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:37`);
  - `RackSiblingMembershipTests` (`tests/RackCad.Tests/RackSiblingRedrawTests.cs:9`);
  - `CallerOwnedTransactionGuardTests`.
- **Las pruebas de T1B** (`PushBackClassicCharacterization*`, en Core y en UI) **no se seleccionan**, aunque INV-F4T4B-13 las promete en verde.
- **Faltan guardas que el cambio toca:** `PushBackPluginSourceGuardTests`, `PushBackRoundTripSourceGuardTests`, `RackProjectionCommandGuardTests` (un `CaptureDrawing`), `NamespaceFolderGuardTests`, `CustomPropertiesEnvelopeGuardTests`, `RackUnitsGuardSourceTests` e `I55G15Auth15ConsumerTests`.

**Corrección:**
- **La guarda de INV-39** va en una clase nueva `WorkspacePilotUpdate*` (filtro 1), o el filtro 2 pasa a `ExpectRed` true con su `MinSelected`.
- **Un filtro de Core con `ExpectRed` false** con esas clases: `PushBackClassicCharacterization`, `DebugFaultInjectionGuardTests`, `RackSiblingMembershipTests`, `CallerOwnedTransactionGuardTests`, `PushBackPluginSourceGuardTests`, `PushBackRoundTripSourceGuardTests`, `RackProjectionCommandGuardTests`, `NamespaceFolderGuardTests`, `CustomPropertiesEnvelopeGuardTests`, `RackUnitsGuardSourceTests` e `I55G15Auth15ConsumerTests`.
- **Otro de UI con `ExpectRed` false** para `RackCad.UI.Tests.PushBackClassicCharacterization`.

### I-5. La CI nunca compila el Plugin en Debug, así que la inyección de INV-30 no se verifica

**Evidencia:**
- El Plugin solo se compila en Release (`eng/ci/verify-autocad-references.ps1:197`, `:203`, `:595`, `:600`).
- En Debug solo se compilan Core y la UI (`.github/workflows/ci.yml:133`, `:211`, `:241`).
- Los cuerpos `#if DEBUG` de D-T4B-03 (paso 7) no los compila ningún job, y Smoke-2 usa el DLL Debug. El precedente `SiblingRedrawDebugFaultInjection.cs` tiene el mismo riesgo.

**Corrección (una de las dos):**
- **una evidencia exigida:** una compilación Debug del Plugin con `-p:UseAutoCADNuGetReferences=true`, sin avisos, registrada por el Worker o por la sesión;
- **o un diseño** en el que la lógica que se llama compila en las dos configuraciones (el patrón `[Conditional("DEBUG")]`, precedente `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:195`) y solo la lectura de la variable va en `#if DEBUG`. Se declara frente a la regla «`#if DEBUG` sin `#else`».

### I-6. Extraer el núcleo de `Traverse` cambia el barrido de Insertar (I-55), del insertar de RACKEDITAR y de RACKPROYECTAR, solo con guardas de fuente

**Evidencia:**
- Los llamadores son:
  - `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs:81`;
  - `src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs:43`;
  - `src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs:127`;
  - `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:116`.
- D-12 pide una variante «aditiva, sin cambiar el barrido para sus consumidores» (`FRZ:468`), e INV-F4T4B-13 promete «Insertar no cambia».
- La extracción es correcta, y es la única forma de cumplir F4-E26 por construcción. Pero es un movimiento de código del camino de I-55, que el Owner probó físicamente (OV-RED), y CI no lo ejecuta.

**Corrección:**
- se declara en el §9 como movimiento sin cambio de comportamiento;
- una guarda fija que `Traverse` solo contiene el bloqueo, la transacción, la llamada al núcleo y el `Commit`, y que el núcleo conserva, en orden, los pasos de hoy (`AssertInOrder` sobre las sentencias de `RackSiblingScan.cs:115-161`);
- Smoke-2 añade un Insertar de Selectivo, un insertar desde RACKEDITAR Push Back y un RACKPROYECTAR.

### I-7. Una transacción ajena abierta no se rechaza: riesgo de un «Aplicado» falso

**Problema:**
- Si `TopTransaction != null` al entrar en MUTATE, `StartTransaction` anida, y el `Commit()` interior no persiste hasta que confirma la exterior.
- La relectura de POST vería el estado sin confirmar y daría Applied. Si la exterior aborta, el cambio se pierde con el borrador en Clean.

**Evidencia:**
- Los precedentes lo rechazan: `src/RackCad.Plugin/Views/RackViewBatchDriver.cs:52-53` (`TOP_TRANSACTION_OPEN`) y `src/RackCad.Application/Views/Insertion/RackSiblingInsertRun.cs:117-118`.
- El paquete solo lo comprueba al salir, y solo en el registro (D-T4B-03, paso 8).

**Corrección:**
- una precondición `TopTransaction == null` antes de `StartTransaction`, que da NotAppliedPreflight(«TOP_TRANSACTION_OPEN») con cero escrituras;
- su guarda de fuente y su prueba de Core por el ámbito de B-3.

### I-8. El §9 I-4 se queda corto ante el abandono de la vía declarada de D-13

**Evidencia:**
- D-13 declara como «vía implementable» la costura de I-55 (`ISiblingRedrawPort`, `RackSiblingRedrawRun` y `SiblingRedrawTransaction`), «con prueba física del Owner (OV-RED)» (`FRZ:507-509`).
- «La mecánica no se congela» (`FRZ:506`) permite otra vía. Pero entonces OV-RED no cubre el escritor del piloto, y la primera prueba física es Smoke-2 (R-09, `FRZ:540-541`).
- I-4 lo presenta solo como «dónde va INV-30». Tampoco discute reutilizar `SiblingRedrawUnit` y `ApplyInTransaction` sin tocar sus archivos (M1, «Costuras»).

**Corrección:** se reescribe I-4.
- El piloto no usa la vía implementable declarada de D-13: usa una vía propia con la mecánica de los pasos 3 a 5.
- OV-RED no se transfiere, y Smoke-2 es la primera prueba física.
- Alternativas:
  - (a) la costura, con una frontera de `Commit()` aditiva y un cambio declarado de `SiblingRedrawSeamGuardTests`;
  - (b) la vía propia reutilizando el tipo de unidad.

---

## Menores

- **M-1.** `DraftApplyOutcome<PushBackPilotState>` (D-T4B-02): ese tipo no existe. Es `PushBackPilotDraftState` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs`).
- **M-2.** La correspondencia de `Prepare` dentro de la corrida bajo Applying está incompleta.
  - NothingToApply da NotAppliedPreflight (`T4P:403`).
  - NotApplicable y PendingText con `ConsideredStatus` = `StatusBeforeAttempt` (`PushBackPilotUpdatePlan.cs:69`) no tienen resultado: hay que fijarlo.
  - El Conflicto de MUTATE no lleva causa (`src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs:69-74`), y D-13 pide «la causa»: aceptarlo de forma expresa.
- **M-3.** C-F4-05 solo se comprueba en MUTATE, y eso deja un residuo OM-5 evitable.
  - `ReadForPrepare` (un `PushBackPilotIntentResult`) no lleva los hechos ni las cuentas.
  - Que devuelva el número de supervivientes y que la corrida lo compruebe también antes de importar. La autoridad sigue en MUTATE.
- **M-4.** Hay dos catálogos: `PrepareRedraw` carga el suyo (`ViewBlockDraw.cs:84`), distinto del de `ReadForPrepare` que usan `Prepare` y el sistema (§9 I-8). Declararlo (riesgo 3 de M1).
- **M-5.** Reconstruir el `PushBackSystem` en el Plugin no se puede probar en Core.
  - Un auxiliar puro en `Pilot/` (deserializar `DesignText` + `PushBackResolver`), con una prueba de Core frente a lo que resolvió `Build`; el escritor solo lo llama.
  - El nombre base de `SyncName` sale del `Name` del sobre del plan. RACKEDITAR usa el de la ventana (`src/RackCad.Plugin/RackPushBackCommands.cs:187-189`).
- **M-6.** `EditUpdateOutcome` lleva su propio enumerado en `RackCad.Application.Workspace` y las diferencias como textos, como `EditCheckOutcome` (`WorkspaceEditIntent.cs:237-256`). Se añade al censo de portadores (`WorkspaceEditIntentHostTests.cs:982`; D-T2B2-06).
- **M-7.** Nomenclatura antigua: IC-08 (`docs/initiatives/I-64-implementation-clarifications.md:145-150`, «obligación vinculante para F4-T4B»: las pistas de la mutación propia no marcan los borradores; INV-28, `FRZ:766`) y `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:108` (el contexto de comando «en T4B») usan el sentido anterior a F4-E51. Decir en el §1 o el §8 que pasan a T4C (R-T4-08 y R-T4-05).
- **M-8.** La tabla de literales: reutilizar textos que ya existen (por ejemplo `NoDraftReason`) cambia sus cuentas exactas (`tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs:473-480`) aunque no haya filas nuevas. «30 → 30 + n» no es obligatorio: fijar las filas y las cuentas exactas.
- **M-9.** La inyección en `Commit()`: decir si lanza **antes** del `Commit` real (sin escritura: «Comprobar ahora» da MATCH y recupera) o **después** (con escritura: Conflict). S2-R-10 depende de ello. Nombrar también las variables de entorno.
- **M-10.** Con la operación del host, la parte de R-T4-07 «Actualizar llama a `NotePilotEdit`» pasa a T4B.
  - Fijar `_knownClean.Remove` en los resultados que cambian el borrador.
  - Ampliar `ChangesADraft` con `.TryBeginApply(` y `.CompleteApply(` (`WorkspaceLifecycleGuardTests.cs:476`), con su autoprueba.
- **M-11.** La guarda «`PrepareRedraw(` antes de `StartTransaction`» es textual entre métodos distintos. Hay que anclarla por cuerpo.
- **M-12.** La Theory de `WorkspaceDraftPilotOwnershipTests.cs:417-425` mezcla las filas `discard` y `keep` (que deben seguir lanzando) con `applied` y `warnings`.
  - Declarar la división y dejar intactas las dos primeras.
  - Todo cambio de una prueba existente va en el commit RED (`docs/AUTOMATION_PLAN.md` §16.8).
- **M-13.** La justificación de I-3 se apoya en la regla «T4B if mutation/write code owns it», que es la de los menores de T4A1 (`owner-f4t4a2-authorization.md:49-70`).
  - El diagnóstico de `TryCheckNow` viene de `A2P:657` y de F4-E55 (`f4-dispositions.md:1913`).
  - Moverlo a T4C añade «Comprobar ahora» a T4C, más allá de su fila (`T4P:173`). Decirlo en el §9.
- **M-14.** H-06 es impreciso. En los comentarios solo se prohíben `Transaction`, `\bDatabase\b` y `\bCommit\s*\(` (`WorkspacePilotEditGuardTests.cs:838-842`; `WorkspaceEditIntentGuardTests.cs:1011`). `LockDocument` y `RackLog` son trampas solo del código (`WorkspacePilotEditGuardTests.cs:812`).

---

## Lo comprobado y correcto

- **Los hechos H-01 a H-13 se sostienen en `c3ae1c97`.**
  - Las líneas de `DraftSession.cs` (`:508-524`, `:515`, `:531-554`, `:537-538`).
  - `PrepareRedraw` importa en `ViewBlockDraw.cs:86`.
  - `Traverse` bloquea, abre su transacción y confirma en `RackSiblingScan.cs:112-113` y `:163`.
  - Las fachadas de Push Back no tienen ningún llamador de su costura caller-owned.
  - `LegacyDeltas` solo existe en pruebas.
- **El núcleo interno de `Traverse` es compatible con las cuentas** de `WorkspaceEditIntentGuardTests.cs:75-128`, si el bucle y la única construcción `new RackSiblingDrawingScan(...) { RawEnvelopes = ... }` pasan al núcleo y `Traverse` conserva su firma.
- **Ampliar `CallerOwnedFacadeGuardTests.Facades`** con las tres fachadas de Push Back cumple sus propiedades: las primeras apariciones de `PrepareRedraw(` y `RedrawInTransaction(` son los miembros `=>`.
- **El escritor en `Plugin/Views/`** no choca con:
  - el censo de `Plugin/Workspace`;
  - `WorkspaceHostSourceGuardTests`;
  - `SiblingRedrawSeamGuardTests` (no es un `*Commands*`);
  - ni con CT-27 de T1B (solo las firmas y los caminos clásicos).

  No puede nombrar `WorkspaceHostCore` (`WorkspaceHostSourceGuardTests.cs:156-166`) ni `NavigationTargets`.
- **`Pilot/`:** las guardas son por archivo, y el censo de `Compose` solo admite el plan. Un archivo nuevo que no componga cabe.
- **`Drafts/`:** admite el cambio de F4-E25 (4) y de F4-E16 en `DraftSession.cs` y `DraftApplyOutcome.cs`, sin archivo nuevo y sin `Debug.` ni `Transaction`. El parámetro opcional de `AppliedPostRereadFailed` no rompe ninguna guarda de firma.
- **D-1 no cambia** si la lectura va por el puerto. Que el host llame a `PushBackPilotPrepareInput.From(draft)` no dispara `BaseOrReadUse`, `PilotForms` ni la prohibición de `.InitialDraftState`.
- **M-5 y M-6** caben en la guarda de orden de `Prepare` (`WorkspacePilotPrepareGuardTests.cs:194-226`). La prueba de totalidad sigue dando `MissingInputReason`.
- **No hace falta tocar ningún `.csproj`:** son SDK y recogen los archivos solos.
- **`ExpectRed` true con el filtro amplio de Workspace** tiene precedente en T4A1 y T4A2.
- **El comparador de POST** (las claves de `PostExpectation.Envelopes`, `EnvelopeText` ordinal y la escala) cuadra con `PushBackPilotPostExpectation` y con D-13. La tabla de D-13 queda cubierta, salvo los avisos (I-1).
- **El reparto del §3** respeta la fila de T4B, las decisiones 3 y 4, la orden TOCTOU (relectura dentro de MUTATE) y el plan inmutable. I-1, I-2 e I-3 están bien llevadas al §9 como interpretaciones del Owner.
- **Privacidad:** el paquete y los mapas no llevan rutas locales ni nombres de usuario.
