# Mapa 1 — La vía de escritura del Plugin y sus precedentes (F4-T4B-MUTATE-POST)

- **Repositorio:** worktree de la iniciativa I-64, `HEAD` = `c3ae1c970c908ef541840fc583680b86376cae0b` (verificado con `git show -s`).
- **Modo:** solo lectura (Read/Grep/Glob, `git show|grep|ls-tree`). No se compiló ni se ejecutó nada.
- **Integridad:** se compararon los 60 archivos citados con `git show HEAD:<ruta>`. Son idénticos salvo por los finales de línea CRLF.
- **Rutas:** relativas a la raíz del repositorio. `a:b-c` = archivo, líneas.
- **Reglas de referencia:** `docs/initiatives/I-64-f4-t4-gate-contract-package.md` §3/§3.2 (:166-214) y §7 (:526-583).
- Los hechos son medidos; lo que es inferencia va marcado como **(inferencia)**.

---

## 1. Las tres fachadas de Push Back y `ViewBlockDraw`

**H-1. Las tres fachadas son adaptadores finos sobre `ViewBlockDraw` y ya tienen la costura caller-owned.**

| Fachada | Clase | `DrawAndPlace` (público) | `RedrawInPlace` (público) | `PrepareRedraw` (internal) | `RedrawInTransaction` (internal) | Nombre de bloque |
|---|---|---|---|---|---|---|
| Lateral | `src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs:18` | :25-38 | :41-56 (`regen = true` por defecto) | :58-62 `(Database, ObjectId blockId, PushBackSystem, string payloadJson, int postIndex = -1)` | :64-66 `(Database, Transaction, PreparedViewRedraw, out IReadOnlyCollection<ObjectId> staleDefinitions)` → `LateralHeaderDrawOutcome` | :73-74 → `RackViewBaseName.PushBackLateral` |
| Frontal | `src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs:17` | :22-36 | :38-54 | :56-60 `(Database, ObjectId, PushBackSystem, PushBackFrontalEnd end, string payloadJson, PushBackSide side = A)` | :62-64 (misma firma) | :70-78 → `RackViewBaseName.PushBackFrontal` |
| Planta | `src/RackCad.Plugin/Systems/PushBack/PushBackPlantaDrawService.cs:16` | :21-33 | :35-49 | :51-54 `(Database, ObjectId, PushBackSystem, string payloadJson)` | :56-58 (misma firma) | :60-61 → `RackViewBaseName.PushBackPlanta` |

- Cada fachada tiene un único builder de Application: `PushBackSystemLateralBuilder` (System:20), `PushBackSystemFrontalBuilder` (Frontal:19), `PushBackSystemPlantaBuilder` (Planta:18). Cada una crea su `LateralHeaderDrawer`.
- La fachada no toca AutoCAD por sí misma. Todo baja a `ViewBlockDraw` (los `using` de AutoCAD son `ApplicationServices` y `DatabaseServices`: System:2-3, Frontal:2-3, Planta:2-3).
- El nombre de bloque solo se usa en `DrawAndPlace`. `RedrawInPlace`, `PrepareRedraw` y `RedrawInTransaction` no renombran.

**H-2. `ViewBlockDraw` está en `src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:24` (`internal static class`).**
- `DrawAndPlace` (:28-57): carga el catálogo (:49), luego `SystemBlockWriter.CreateBlock` (:50) y `BlockPlacement.PlaceAndReport` (:51). Una excepción se convierte en `HeaderPlacementResult.Failure` (:53-56).
- **`PrepareRedraw` (:65-89), el PREPARE:**
  - lanza `InvalidOperationException` si la base es nula o no hay destino (:74-82): no devuelve un resultado tipado;
  - carga **su propio** catálogo con `RackCatalogLoader.Load()` (:84);
  - construye el plan (:85);
  - **importa la biblioteca con `BlockLibraryImporter.EnsureForPlan(database, built)` (:86)**;
  - devuelve `new PreparedViewRedraw(blockId, drawer, built, payloadJson, catalog)` (:88).
  - Su comentario (:60-64) dice que debe llamarse dentro del bloqueo y antes de la transacción semántica.
- **`RedrawInTransaction` (:97-109), el MUTATE:** baja directamente a `SystemBlockWriter.RedefineInTransaction`. No bloquea, no abre transacción, no confirma, no regenera y no importa (comentario :91-96).
- `RedrawInPlace` (:114-143): carga el catálogo (:136) y llama a `SystemBlockWriter.RedrawInPlace(..., regen)` (:137). Una excepción se convierte en `Failure` (:139-142).

**H-3. `PreparedViewRedraw` (`src/RackCad.Plugin/Systems/Shared/PreparedViewRedraw.cs:22`, `internal sealed`)** guarda `BlockId` (:39), `Drawer` (:41), `Plan` ya importado (:44), `PayloadJson`, que es el sobre de esa vista y nunca se comparte (:46-47), y `Catalog` (:49).
- **El `ObjectId` de destino queda fijado en PREPARE**, antes de la transacción (:24-36).

**H-4. `SystemBlockWriter` (`src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:15`) es la primitiva compartida.**
- **`RedefineInTransaction` (:126-138)** hace dos cosas: `drawer.RedefineSystemBlock(...)` (:135) y `RackBlockData.Write(transaction, blockId, payloadJson)` (:136). No confirma, no regenera, no anida transacciones, no bloquea y no importa (:108-124).
- **`RedrawInPlace` (:67-102), el ciclo propio que usa RACKEDITAR:**
  - `LockDocument` (:77) → `EnsureForPlan` fuera de la transacción (:81) → `StartTransaction` (:83) → `RedefineInTransaction` (:86) → `Commit()` (:87);
  - después, `PurgeAfterCommit` (:91) y `ApplyRegen(document, regen)` (:92), todavía dentro del bloqueo;
  - todo en un único `try`, con `catch` → `Failure` (:71, :98-101).
- `PurgeAfterCommit` (:147-150) delega en `LateralHeaderDrawer.PurgeUnreferenced`.
- `ApplyRegen` (:157-163) es la única autoridad del regen en la vía de escritura: `document.Editor.Regen()` solo si `regen` es verdadero.
- `CreateBlock` (:18-40) y `CreatePreparedBlock` (:46-62) poseen su propio bloqueo y su transacción.

**H-5. Lo que AutoCAD toca por debajo.**
- **`LateralHeaderDrawer.RedefineSystemBlock`** (`src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:88-182`):
  - abre `BlockTable` y la definición `ForWrite` (:91-92) y borra el contenido (:115-118);
  - crea definiciones de cabecera nuevas con `NewBlock` (:132);
  - fuerza `RecordGraphicsModified(true)` en cada referencia directa (:167-171);
  - devuelve en `staleDefs` las definiciones anidadas huérfanas (:177-178) y un `LateralHeaderDrawOutcome` con las piezas que faltan (:180-181);
  - no abre transacción.
- **`LateralHeaderDrawer.PurgeUnreferenced`** (:193-238): abre **su propia** transacción (:202), llama a `db.Purge` (:220), confirma (:230) y traga cualquier excepción con `RackLog` (:233-237). Es *best effort*.
- **`RackBlockData.Write`** (`src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:20-52`):
  - **vuelve en silencio** si el id es nulo o el JSON está vacío (:22-25);
  - trocea el JSON en cadenas de ≤255 caracteres (`ChunkSize` :18, `Substring` :36-39) dentro de un `Xrecord` bajo la clave `RACKCAD_SELECTIVE` (:16, :41-51).
- **`RackBlockData.Read`** (:54-89) concatena los trozos `DxfCode.Text` (:79-88).

---

## 2. Precedentes caller-owned en producción y sus guardas

**H-6. Las fachadas de Push Back no tienen ningún llamador de su costura caller-owned.** En `src`, `PrepareRedraw(` y `RedrawInTransaction` de Push Back solo aparecen en sus propios archivos (`git grep`).

Sus usos reales son:
- `RedrawInPlace`: `src/RackCad.Plugin/RackPushBackCommands.cs:279`, `:297`, `:336`;
- `DrawAndPlace`: `:111`, `:124`, `:129`, `:432`.

Los demás llamadores de la costura caller-owned son:
- Selectivo: `RackSelectivoInsertIntegration.cs:220-249`;
- variables de proyecto: `ProjectVariableMutationExecutor.cs:239-257`;
- `LateralHeaderDrawService` (consigo mismo): `src/RackCad.Plugin/Drawing/LateralHeaderDrawService.cs:135`, `:140`.

**H-7. Precedente A, la costura de I-55 (Insertar de Selectivo).** Archivos: `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs`, `src/RackCad.Plugin/Systems/Shared/{AutoCadSiblingRedrawPort,SiblingRedrawTransaction,SiblingRedrawUnits,SiblingRedrawDebugFaultInjection}.cs` y `src/RackCad.Application/Views/Redraw/*`.

- **El orden puro está en Application.** `RackSiblingRedrawRun.Execute` (`src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs:30-119`):
  1. corta ante un miembro `BlockingUnreadable` (:37-46);
  2. hace PREPARE de cada `Redraw`/`Erase` (:48-80). Una excepción o un fallo dan `PrepareFailed`, y una capa bloqueada da `LOCKED_LAYER` (:71-77);
  3. construye el plan con su disposición (:82-86);
  4. llama a `port.Mutate(units)` y convierte **cualquier** excepción en `Discarded` (:92-108);
  5. llama a `port.Post`. **Un fallo de POST sigue siendo `Committed`**, con el diagnóstico `POST_FAILED` (:110-118).
- **El puerto** `ISiblingRedrawPort<TUnit>` (`ISiblingRedrawPort.cs:66-71`) ofrece `Prepare`, `Mutate` y `Post`. Los resultados son `RackSiblingMutationResult` (`Committed` o `Discarded`, :48-64).
- **El adaptador AutoCAD** `AutoCadSiblingRedrawPort` (`src/RackCad.Plugin/Systems/Shared/AutoCadSiblingRedrawPort.cs:8-47`) toma **un bloqueo propio en cada paso**: Prepare (:23), Mutate (:31) y Post (:39). No existe un bloqueo único alrededor de los tres.
- **La transacción:** `SiblingRedrawTransaction.Mutate` (`SiblingRedrawTransaction.cs:11-31`):
  - abre una transacción (:18), llama a `ApplyInTransaction` (:20) y vuelve sin confirmar si el resultado es `Discarded` (:21);
  - `Commit()` (:23) está **dentro del mismo `try`** que la mutación;
  - `catch (Exception)` convierte en `Discarded` también una excepción de `Commit()` (:27-30). Es la H-02 de F4: INV-30 aún no existe.
- **`ApplyInTransaction`** (:34-53) es la costura sin transacción propia (modo 2). Recorre las unidades con la inyección `#if DEBUG SiblingRedrawDebugFaultInjection.ThrowIfRequested(index + 1)` (:44-46) **antes de cada unidad**, no en la frontera de `Commit()`.
- `Post` (:55-66) ejecuta `unit.Post()` de cada unidad y después **un** `document.Editor.Regen()` (:65).
- **La unidad:** `SiblingRedrawUnit` (`SiblingRedrawUnits.cs:8-26`) lleva `DefinitionKey`, `Apply(Transaction)` y `Post()`.
- **`SelectiveInsertPort.PrepareMember`** (`RackSelectivoInsertIntegration.cs:198-256`):
  - el sobre se compone **por hermano**, sobre el sobre leído de ese hermano (:209-211);
  - según el tipo de vista llama a `*.PrepareRedraw(document.Database, id, ...)` (:218-238): **la importación ocurre aquí, en PREPARE**;
  - la unidad aplica `*.RedrawInTransaction(...)` (:241-250);
  - su `post` es `SystemBlockWriter.PurgeAfterCommit` + `RackBlockRenamer.SyncName(document, id, targetName)` (:251-255).
- La unidad de borrado está en `EraseDefinition` (:281-289).
- `HasOpenTopTransaction` (:164) informa a `RackSiblingInsertRun`, que bloquea la colocación con `TOP_TRANSACTION_OPEN` (`src/RackCad.Application/Views/Insertion/RackSiblingInsertRun.cs:79`, :117-118).
- **El cableado desde el comando:** `src/RackCad.Plugin/RackSelectivoCommands.cs:147-175` (`new SelectiveInsertPort` → `ScanAndClassifyOnce` → `TryPrepare` → `CreateRedrawPort` → `RackSiblingRedrawRun.Execute`).

**H-8. Precedente B, `ProjectVariableMutationExecutor`** (`src/RackCad.Plugin/ProjectVariableMutationExecutor.cs:101-349`).
- Un único bloqueo (:141) envuelve **PREPARE**, **MUTATE** y **POST**.
- **PREPARE** (:143-260):
  - carga el catálogo (:144);
  - por rack, `RackCommandSupport.FindRackBlocks`, que tiene **su propio** bloqueo, transacción y `Commit` (`RackCommandSupport.cs:117-142`);
  - **`MutationDestinationBinding.Bind(...)` en PREPARE, fuera de la transacción** (:160-165);
  - resuelve la geometría (:168), serializa el diseño una vez por rack (:174), compone un sobre por vista (:223-233) y hace `PrepareRedraw` por vista (:235-258).
- **MUTATE** (:262-313):
  - una transacción (:266);
  - **la relectura acreditada se hace dentro de esa misma transacción:** `ProjectVariablesRegistry.Read(transaction, database)` + `RackBlockFinder.ScanEnvelopes(transaction, database, ...)` (:270-277) → `RegistryCommit.Prepare` (:282-287);
  - escribe el registro (:289-296) y llama a `destination.Write(...)` por vista (:301-310);
  - **`transaction.Commit()` (:312), en el mismo `try` exterior (:139-327)**;
  - sin `#if DEBUG` y sin inyección.
- **POST** (:315-320): `PurgeAfterCommit` (:316) y `ApplyRegen(document, prepared.Count > 0)` (:317). No hay `SyncName`.
- `catch` → `Aborted` (:323-327).

**H-9. `MutationDestinationBinding`** (`src/RackCad.Application/ProjectVariables/MutationDestinationBinding.cs:56-136`) es puro y todo o nada.
- Aborta en estos casos:
  - el plan no tiene destinos (:67-72);
  - un id está vacío (:78-83);
  - un id se repite (:85-91);
  - un id presente está vacío (:100-105);
  - una vista del plan ya no está presente (:111-119);
  - **una vista presente no está en el plan** (:121-132).
- Su entrada es `IReadOnlyList<ProjectVariableScanEntry>`. Ese tipo es de Selectivo: lleva `SelectivePalletDesignDocument Authored` (`src/RackCad.Application/ProjectVariables/ProjectVariableScanEntry.cs:21-38`) y su constructor es `private` (:23).
- Su único llamador en `src` es `ProjectVariableMutationExecutor.cs:160`.
- **(inferencia)** Para Push Back es un patrón que hay que replicar, no un tipo reutilizable tal cual. Esto cuadra con la redacción «es un patrón de Push Back» (R-T4-02).

**H-10. Precedente C, el alcance de escritura de RACKPROYECTAR**, que es el patrón de puerto simulable más cercano.
- `IRackProjectionWriteScope : IDisposable` con `CreateDefinition`, `PlaceReference` y `Commit()`, más `IRackProjectionWriteScopeFactory.Begin()` (`src/RackCad.Application/Views/Placement/RackProjectionMaterialization.cs:153-168`).
- La corrida pura `RackProjectionMaterializationRun.Execute` (:228-303):
  - hace `Begin()` en su propio `try` (:236-243);
  - **`scope.Commit()` (:287) va en el mismo `try` que las escrituras** (:252-293), y una excepción da `RolledBack/UnexpectedException`.
- El adaptador `AutoCadProjectionWriteScopeFactory` (`src/RackCad.Plugin/Views/RackProjectionWriteScope.cs:17-116`):
  - `Begin` toma el bloqueo (:26) y la transacción (:29) y libera el bloqueo si la transacción falla (:32-36);
  - `Scope.Commit` es la única confirmación (:108);
  - `Dispose` descarta (:110-114).
- La importación previa del mismo comando es `ImportAndObserve` (`src/RackCad.Plugin/Views/RackProjectionCommandPort.cs:106-117`): bajo bloqueo, **sin transacción**, con `LibraryBlockAvailabilityFlow.Observe`. Se declara en `src/RackCad.Application/Views/Placement/RackProjectionCommandRun.cs:121` y se llama en :226.
- Hay pruebas con un alcance falso: `tests/RackCad.Tests/RackProjectionMaterializationRunTests.cs:32-74` y `tests/RackCad.Tests/G15CommandTestSupport.cs:200`.

**H-11. `TopTransaction` y la identidad por `UnmanagedObject`.**
- El único sitio que compara transacciones es `RackDefinitionCreator.Precheck` (`src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs:147-164`). Su comentario (:156-158) dice que «TopTransaction returns a NEW managed wrapper on every read». Compara `top.UnmanagedObject != transaction.UnmanagedObject` (:161) y rechaza con `TransactionMismatch`.
- Los demás usos de `TopTransaction` en `src` son:
  - `RackSelectivoInsertIntegration.cs:164`;
  - `Views/RackViewBatchDriver.cs:52-53`, que lanza `TOP_TRANSACTION_OPEN`;
  - las comprobaciones Debug de «ninguna abierta»: `Views/RackPilotIntentAdapter.cs:195-201`, `Workspace/WorkspaceController.cs:267-273`, `Workspace/WorkspaceSelectionReader.cs:115`;
  - `Workspace/AutoCadEventBridge.cs:288-302`, que **solo registra** en el log: un `Assert` fallido terminaría AutoCAD (:286-287).
- La relectura con lectura de vuelta en la misma transacción tiene un precedente: `RackDefinitionCreator.Envelope` escribe y vuelve a leer el sobre (:200-213).

**H-12. La inyección de fallos Debug.**
- El único inyector de `src` es `SiblingRedrawDebugFaultInjection`: todo el archivo va en `#if DEBUG` (`SiblingRedrawDebugFaultInjection.cs:1-18`), con la variable de entorno `RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT` (:7) y la granularidad de **una unidad**.
- No hay ninguna inyección en la frontera de `Commit()`.
- `INV-30` aparece en `src/RackCad.Application/ComputedParameters/*`, pero es **el INV-30 de I-63**, un contador de métricas que no tiene relación con este.

**H-13. Las guardas caller-owned existentes.**
- **`tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs`:**
  - la lista `Facades` (:50-56) tiene cuatro fachadas: `ViewBlockDraw`, `SelectiveFrontalDrawService`, `SelectivePlantaDrawService` y `LateralHeaderDrawService`. **Las tres de Push Back no están**;
  - `Body(source, marker)` (:58-92) toma la **primera** aparición del marcador. Si hay `=>` antes de `{`, devuelve hasta el `;` (:63-70); si no, empareja llaves (:72-89);
  - fija lo siguiente:
    - la fachada expone `RedrawInTransaction` y `PrepareRedraw` (:96-108);
    - la primitiva no tiene `Commit()`, `StartTransaction`, `LockDocument`, `Regen`, `EnsureForPlan` ni `BlockLibraryImporter`, y no baja por `RedrawInPlace(` (:112-161);
    - la preparación importa o delega (:165-175) y no tiene `StartTransaction` (:177-182);
    - el wrapper histórico se conserva (:186-191 y :198-223);
  - G9A (:225-249) fija que `SiblingRedrawUnits`/`SiblingRedrawTransaction` no replanifican ni llaman a `PrepareRedraw(`/`EnsureForPlan`, y que `Apply` no tiene transacción, `Commit`, `Regen` ni importación.
- **`tests/RackCad.Tests/CallerOwnedTransactionGuardTests.cs`:** fija las mismas propiedades sobre `SystemBlockWriter` y `LateralHeaderDrawService` (MUTATE :85-138, una sola autoridad :142-163, PREPARE antes de la transacción y POST después del commit, purga → un regen :167-238, compatibilidad :242-262). Su `Body` (:53-81) solo empareja llaves.
- **`tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs`** congela la costura de I-55:
  - `SiblingRedrawTransaction.cs` tiene **exactamente un** `StartTransaction(` y **un** `Commit()`, ni `Prepare(`, ni `RackId`, ni `LockDocument` (:10-21);
  - un `Regen()`, y ninguno en `Mutate` (:24-30);
  - el inyector va en `#if DEBUG`, sin `#else` y terminado en `#endif`, y la llamada está envuelta en `#if DEBUG` (:37-56);
  - **ningún archivo `*Commands*.cs` del Plugin nombra `SiblingRedrawTransaction`** (:61-68).
- **`tests/RackCad.Tests/ProjectVariableMutationExecutorTests.cs`** (:212-337) fija sobre el ejecutor:
  - un `Commit()`, una `StartTransaction()` y un `ApplyRegen`, y ningún `Editor.Regen()`;
  - `PrepareRedraw(` va antes de `StartTransaction()`;
  - no aparecen `EnsureForPlan` ni `BlockLibraryImporter`;
  - el `Commit()` va antes de la purga;
  - `MutationDestinationBinding.Bind(` está presente.
- **`tests/RackCad.Tests/I55G15Auth15ConsumerTests.cs:44-56`:** el único archivo del Plugin que llama a `RackDefinitionCreator.CreateInTransaction(` es `RackProjectionWriteScope.cs`.

---

## 3. El camino clásico de RACKEDITAR Push Back (`EditPushBack`)

**H-14. El orden de `src/RackCad.Plugin/RackPushBackCommands.cs:147-384`.**
1. Deserializa el proyecto y comprueba que exista `PushBackDesign` (:151-166).
2. Abre la ventana modal y sale si no se pide insertar (:168-175).
3. Toma `design`/`system` **solo de la ventana**. Nunca se vuelve a resolver (:177-184).
4. La identidad: conserva el GUID del sobre elegido (:187) y toma el nombre de la ventana o del sobre (:188-190).
5. Solo con `!window.UpdateOnly`: `RackUnsupportedSiblingInsert.TryAuthorize(...)` (:193-196). Hace `RackSiblingScan.Capture(..., _ => false)`, la puerta de CustomProperties y la autorización (`src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs:36-82`). Solo deja los `MutableMembers` de ese rack (:71-78).
6. Los bloques: `authored.Blocks` si inserta, y `FindRackBlocks` si actualiza (:198), con el origen como respaldo (:199-202).
   - `FindRackBlocks` usa `ScanEnvelopes`, que **omite los bloques de xref** (`RackBlockFinder.cs:66`) y empareja por `Id` (`RackCommandSupport.cs:130-136`).
7. Prevalidaciones, todas antes de la geometría:
   - el sobre es Push Back y de este GUID (:208-217);
   - el proyecto interior, con `RackCommandSupport.PreflightInnerSources` (:221-226);
   - el descriptor de vista (:230-237).
8. La unidad INSUNITS solo se avisa cuando se inserta (:241-244). La actualización pura no avisa: lo fija `tests/RackCad.Tests/RackUnitsGuardSourceTests.cs:122-137`.
9. El lote ID18 se prepara solo si inserta (:246-260).
10. Un catálogo y los cortes laterales se calculan una sola vez, con `PushBackViewAvailabilityFacts` (:268-271).
11. **Se redibuja cada vista con `RedrawInPlace(..., regen: false)`**, es decir, **un bloqueo, una transacción y un `Commit` por vista**, con la importación propia en cada `RedrawInPlace` (`SystemBlockWriter.cs:77-93`):
    - planta (:276-285);
    - frontal, con `end`/`side` sacados de `decoded.Address.Variant.PushBackEnd/PushBackSide` y la sección reescrita tal cual (:286-319);
    - lateral, con `postIndex = decoded.Address.Variant.Index` y un corte ausente que pasa a obsoleto (:320-345).
    - Después de cada éxito se llama a **`RackBlockRenamer.SyncName`** con `RackViewBaseName.LinkedPlanta`, `LinkedPushBackCut` o `LinkedLateral` (:282, :300-309, :339-342).
    - Si una vista falla, `result.Success` es falso y el bucle **sigue con las demás**. La actualización no es atómica (H-13 del paquete).
12. Los obsoletos se borran con `EraseViewBlocks`, solo si quedan supervivientes contados por bloques (:349-356, `RackCommandSupport.cs:218-…`, con su propia transacción).
13. **Hay un único `Editor.Regen()`** cuando hubo cambio (:362-367).
14. Si inserta, `RackViewBatchExecution.Run`; si no, el mensaje de recuento (:369-383).

- La carga por vista es `BuildPushBackPayload` (:439-461): `RackProjectStore.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(innerSource))`, después `RackEmbedComposer.Compose` y `RackEmbedStore.Serialize`.

**H-15. `RackBlockRenamer.SyncName`** (`src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:21-58`).
- Abre **su propio** bloqueo (:32), su transacción (:33) y su `Commit` (:50).
- No hace nada si el nombre ya coincide o está ocupado (:35-46).
- Traga toda excepción con `RackLog` (:53-57). Es cosmético y no cambia el sobre.

---

## 4. La importación de la biblioteca de bloques (OM-5)

**H-16. `BlockLibraryImporter`** (`src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:20-143`). Su comentario dice «Must be called inside the document lock but OUTSIDE the drawing transaction» (:18).
- `EnsureForPlan` (:25-39): extrae los requisitos con `RackBlockRequirementExtractors.HeaderRun` y llama a `LibraryBlockAvailabilityFlow.Observe(..., allowImport: true, new AutoCadLibraryBlockQuery(db), new ImportAdapter(db))` (:32-37).
- **`EnsureBlocks`** (:50-132):
  1. abre una **transacción propia** de lectura con `Commit` (:68-80);
  2. con la biblioteca ausente vuelve con `0` en silencio (:87-91);
  3. si no, ejecuta `source.WblockCloneObjects(..., DuplicateRecordCloning.Ignore, ...)` sin transacción del dibujo (:123);
  4. toda excepción se registra y devuelve `0` (:126-131).
- **El flujo puro y sus puertos** (`src/RackCad.Application/Systems/Shared/LibraryBlockRequirements.cs`):
  - `ILibraryBlockQuery` (:113-117), un puerto de observación pura;
  - `ILibraryBlockImporter` (:120-123), un puerto de mutación aparte;
  - `LibraryBlockAvailabilityFlow.Observe` (:156-181): importa y después consulta. La consulta final es la autoridad (:153-155).
- **Los puntos de importación de cada camino:**

| Camino | Dónde importa | Transacción en curso |
|---|---|---|
| Selectivo, Insertar (I-55) | `*.PrepareRedraw` en `PrepareMember` (`RackSelectivoInsertIntegration.cs:220-236`), bajo el bloqueo de `AutoCadSiblingRedrawPort.Prepare` (:23) | Ninguna; la de MUTATE se abre después (`SiblingRedrawTransaction.cs:18`) |
| Selectivo, primera colocación | `HeaderRequirements.Evaluate` → `LibraryBlockAvailabilityFlow.Observe` (`Views/RackViewPlacement.cs:66-83`) | Ninguna |
| Variables de proyecto | `PrepareRedraw` por vista (`ProjectVariableMutationExecutor.cs:235-258`) | Ninguna; la transacción va en :266 |
| RACKPROYECTAR | `ImportAndObserve` (`Views/RackProjectionCommandPort.cs:106-117`) | Ninguna |
| RACKEDITAR Push Back | `SystemBlockWriter.RedrawInPlace:81`, **una vez por vista** | Ninguna, pero cada vista tiene su `Commit` |

- **Por qué la importación queda fuera de MUTATE:** `SystemBlockWriter.cs:120-124` y `CallerOwnedTransactionGuardTests.cs:114-126`. Con la biblioteca ausente, la importación devuelve 0 en silencio y deja la geometría incompleta.
- La primitiva `RedefineInTransaction` solo **informa** de las piezas que faltan, en `LateralHeaderDrawOutcome.MissingInstances`/`HasMissingBlocks` (`src/RackCad.Plugin/Drawing/LateralHeaderDrawOutcome.cs:33`). `ProjectVariableMutationExecutor` no lo consulta; `RackDefinitionCreator` sí lo rechaza (:67-72).

---

## 5. El lado Plugin del piloto ya existente

**H-17. El adaptador de lectura del piloto** (`src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:69-202`, `internal static`).
- `LoadCatalog()` (:74-87) va fuera del bloqueo y devuelve un fallo tipado. Usa el mismo `RackCatalogLoader.Load()` que las fachadas (`LateralHeaderDrawService.cs:299`).
- **`Read(Document, string rackId, long originDefinitionHandle, string? witnessKey, string? witnessText, RackCatalog)`** (:94-142):
  1. `AssertNoOpenTransaction(document, "before")` (:104);
  2. `RackSiblingScan.CaptureDrawing(document)` (:116);
  3. el fallo tipado si `RawEnvelopes == null` (:117-118);
  4. `new PushBackPilotIntentScan(rackId, originKey, scan.Facts, scan.RawEnvelopes, CompletenessEvidence, witness)` (:120) → `PushBackPilotIntentReader.Read` (:121);
  5. los `catch` de `LockViolation`, AutoCAD y `System.Exception` (:125-136);
  6. `finally` con `AssertNoOpenTransaction(..., "after")` (:137-141).
- Devuelve solo `PushBackPilotIntentResult`: **descarta `scan.Definitions`**, el mapa clave → `ObjectId`.
- `ReadPreviewWitness` (:149-189) tiene su propio bloqueo (:160) y una transacción corta **sin `Commit`** (:161-170).
- `ToHandle(long)` (:192) es la única conversión de handle. `CompletenessEvidence = "RackSiblingScan/CaptureDrawing"` (:71).

**H-18. El barrido que comparten las lecturas** (`src/RackCad.Plugin/Views/RackSiblingScan.cs`).
- `CaptureDrawing(Document)` (:95-96) = `Traverse(document, ObjectId.Null, null)`: **`eraseByKind` es nulo** (C-F4-05) y no hay ninguna fuente seleccionada.
- `Traverse` (:98-170) **toma su propio `LockDocument` (:112), abre su transacción (:113) y confirma (:163)**. Hace un único recorrido de la `BlockTable`:
  - lee con `RackBlockData.Read` (:121);
  - la clave es `definitionId.Handle.ToString()` (:125);
  - cuenta las referencias de layout y las anidadas (:129-135) y las capas (:134, :137-139);
  - construye `RackSiblingScanFact` (:142-152); `IsFromExternalReference` entra como xref y **no se omite**, a diferencia de `ScanEnvelopes`;
  - rellena `RawEnvelopes` (:154, :166-169).
- `Capture(...)` (:79-92) hace el mismo recorrido con una fuente seleccionada y `eraseByKind`, y lo clasifica con `RackSiblingMembership.Classify`.
- **No existe ninguna variante caller-owned** de `CaptureDrawing`/`Traverse`: `git grep CaptureDrawing` da una sola definición (:95). Los otros llamadores son `Views/RackProjectionSnapshotReader.cs:127` y la cadena de evidencia de `Views/RackProjectionKindSessions.cs:236`.
- Aparte existe `RackBlockFinder.ScanEnvelopes(Transaction, Database, bool)` (`src/RackCad.Plugin/RackBlockFinder.cs:57-91`), que sí es caller-owned. No sirve para la equivalencia de R-T4-01:
  - omite los xref (:66), y `Traverse` no;
  - no produce `RackSiblingScanFact`, ni capas, ni `RawEnvelopes`.

**H-19. Las tres vías de lectura de R-T4-01, tal como existen hoy.**

| Vía | Estado en `c3ae1c97` | Camino medido |
|---|---|---|
| Lector de «Comprobar ahora» | **Existe** | `WorkspaceHostCore.TryCheckPilotEdit` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:367`; solo para `Unknown(CommitThrew)`, :390) → `_editIntentReader.ReadIntent` (:394) → `CheckNow(pilot.StaleBase)` (:412). En el Plugin: `WorkspaceNavigationHost.PilotIntentReader.ReadOnActiveDocument` (`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:220-253`): la **puerta de reposo** `CommandInProgress`/`IsQuiescent` (:225-226), el catálogo fuera del bloqueo (:228), `using LockDocument` (:232), `_authorize` (:234), `_targets.Revalidate` (:239), `ChooseOriginDefinition` (:240) y `RackPilotIntentAdapter.Read` (:242-248). Confirmar la base usa la misma lectura (`WorkspaceHostCore.cs:528`, :574) |
| O-04 en contexto de comando (R-T4-06) | **No existe** (es de T4C) | — |
| Lectura caller-owned (INV-39) | **No existe** | — |

- Todas producen hoy la base con `PushBackPilotIntentReader.BuildStaleBase`/`Analyze` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:64-68`, :113-146), sobre `RackSiblingMembership.Classify(scan.Facts, scan.RackId, null, false)` (:119).
- La entrada es `(Key, Membership, View, Section, EnvelopeText)` (`DraftStaleBase.cs:25`). **La base no lee el registro de variables** (:62-63), así que `VariableRegistryText` queda nulo.

**H-20. `RackSiblingMembership`** (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:91-156`) es puro.
- Clasifica en `Redraw`, `ReadOnly` (xref), `Erase`, `NotMember` y `BlockingUnreadable` (:127-149).
- `IsSurvivor` = `Redraw` con `LayoutReferenceCount >= 1` (:64).
- Con `eraseByKind` nulo no hay miembros `Erase`.
- `RackSiblingRedrawPlan` da `DeferToFirstPlacement` solo si `survivors == 0 && erase.Count != 0` (`RackSiblingRedrawPlan.cs:91-95`). **(inferencia)** Por tanto, C-F4-05 («cero supervivientes → No aplicado») no sale de la disposición del plan de I-55. T4B debe decidirlo sobre los hechos de su relectura. La `DraftStaleBaseEntry` no lleva el recuento de referencias; los `scan.Facts` sí lo llevan.

**H-21. `NavigationTargets`** (`src/RackCad.Application/Workspace/NavigationTargets.cs:23-…`, con `Revalidate` en :108).
- Solo se instancia en `WorkspaceNavigationHost.cs:52`.
- **Fuera de las carpetas del panel** (`Application/Workspace`, `Plugin/Workspace`, `UI/Workspace`) ningún archivo puede nombrarlo. Lo fija `tests/RackCad.Tests/Workspace/WorkspaceNavigationPrivacyGuardTests.cs:22-25` y :30-42. Ningún comando del Plugin lo nombra (:73-83).

**H-22. El plan inmutable que consumiría MUTATE** (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`).
- `PushBackPilotMemberPlan` (:144-178): `Key`, `View`, `Section`, `DesignText` y **`EnvelopeText`** (el sobre por vista, compuesto sobre el sobre releído, :414-418).
- `PushBackPilotUpdatePlan` (:208-260): `RackId`, `OriginKey`, `ExpectedBase` ordenada por clave (:234), `ExpectedVariableRegistryText`, `AnnotationScale`, `Members` y `PostExpectation`, que lleva `AnnotationScale` y `Envelopes (Key, Text)` (:181-200).
- **El plan no lleva el `PushBackSystem`.** El sistema se construye en `Build` (:364-367) y se descarta. Tampoco lleva `PushBackFrontalEnd`/`PushBackSide` ni `ObjectId`.
- Las fachadas sí piden el sistema y, en el frontal, `end`/`side` (H-1).
- Existe un decodificador de sección en Application: `PushBackSystemFrontalBuilder.DecodeSection(int)` (`src/RackCad.Application/Systems/PushBack/PushBackSystemFrontalBuilder.cs:142`), con `IsValidSection` en :155. RACKEDITAR usa en su lugar `decoded.Address.Variant.PushBackEnd/Side` (`RackPushBackCommands.cs:288-293`).

**H-23. La máquina de estados que consume el resultado** (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs`, como referencia de frontera).
- `TryBeginApply` (:508-524) y `CompleteApply` (:531-554) → `Classify` (:556-606).
- Los resultados de `DraftApplyOutcome` (`src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs:69-93`) son `NotAppliedConflict`, `NotAppliedAdmission`, `NotAppliedPreflight`, `NotAppliedFailure`, `CommitThrew`, `AppliedPostRereadFailed`, `Applied(postRead)` y `AppliedWithWarnings`.
- En `CompleteApply`, la ranura se libera (:537-538) **antes** de las copias de `Applied` (:591-592), que son el punto de F4-E25 (4).

---

## 6. Lo que hoy está prohibido o guardado para el piloto en el Plugin

**H-24. La carpeta `src/RackCad.Plugin/Workspace/`** tiene hoy 9 archivos (`ls`).
- `WorkspaceEditIntentGuardTests.cs:986-998` fija **exactamente 9 archivos** y ninguno con «Intent». `WorkspacePilotEditGuardTests.cs:981-1001` fija la lista por nombre, sin techo.
- `WorkspaceHostSourceGuardTests.cs` prohíbe, en todos los archivos de `Plugin/Workspace` y `Application/Workspace`:
  - `Regen`, `SetSystemVariable`, `Zoom*` y `SendStringToExecute` (:382-396);
  - `async`, `await`, `Task.Run`, `Timer` y similares, y `.Result` en el Plugin (:423-443 y :445-455);
  - `System.IO`, `File.`, `Xrecord`, `XData`, `DBDictionary` y `NamedObjectsDictionary` (:457-464);
  - cualquier `StartTransaction`/`LockDocument` fuera de un `using`, y cualquier campo de tipo `Transaction`/`Document`/`Database`/`ObjectId` (:466-479).
- `ExecuteInCommandContextAsync` está prohibido **en todo `src`** (`tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:436`).

**H-25. Las guardas sobre el adaptador del piloto y el barrido** (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs`).
- **Sobre `RackSiblingScan.cs`:**
  - `RawEnvelopes` es `get; init;`, hay un solo `rawEnvelopes.Add(key, json)` y un solo `RackBlockData.Read(` (:75-87);
  - **las firmas exactas** `Capture(...)`, `CaptureDrawing(Document document)` y `private static ... Traverse(Document document, ObjectId selectedDefinition, Func<RackEmbedDocument,bool> eraseByKind)`, el constructor de 5 parámetros y **un solo `new RackSiblingDrawingScan(`** (:89-118);
  - ninguna escritura, y **un solo `.Commit(`** en el archivo (:121-128).
- **Sobre `RackPilotIntentAdapter.cs`:**
  - la forma (:133-148) y que no guarde objetos del host en campos (:155-160);
  - `Read` no tiene `LockDocument` ni `StartTransaction` (:190-205);
  - el `Assert` Debug antes y después de leer (:207-268);
  - los `catch` ordenados (:270-291);
  - **`AdapterWrites`**: no puede haber `ForWrite`, `UpgradeOpen`, `.Erase(`, `.Commit(`, `RackBlockData.Write`, `Regen`, `UpdateScreen` y otros (:290-301);
  - un solo `CaptureDrawing(` y un solo `new Handle(`; prohibidos `Handle.Parse`, `Convert.ToInt64`, `new ObjectId` y `GetObjectId(` (:323-339);
  - nada de Workspace ni de navegación (:379-…).
- **(inferencia)** La escritura no puede vivir en `RackPilotIntentAdapter.cs`, y la variante caller-owned de la lectura tiene que encajar en las cuentas fijas de `RackSiblingScan.cs`.

**H-26. Las guardas del PREPARE puro** (`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`).
- La lista `Forbidden` (:30-51) se aplica **solo a `PushBackPilotUpdatePlan.cs`** (:18, :61-79). Prohíbe `Transaction`, `LockDocument`, `Commit`, `RackBlockData.Write`, `Database`, `Autodesk`, `RackCad.Plugin`, las mutaciones del borrador (incluidas `TryBeginApply` y `CompleteApply`) y `async`.
- Fija sin asincronía los archivos de la tarea y `WorkspaceController.cs` (:407-430).
- `WorkspacePilotEditGuardTests.cs:354-362` **ya admite** que el siguiente paso añada `IWorkspacePilotEditPort.Update(...)` con escritura.

**H-27. Otras restricciones transversales que afectarían a archivos nuevos.**
- **`NamespaceFolderGuardTests.cs`:**
  - el namespace coincide con la carpeta (:101-110);
  - **un único namespace de bloque**, nunca *file-scoped* (:113-134);
  - las subcarpetas de `Systems/` solo pueden ser los seis buckets (:151-165).
- **`RackUnitsGuardSourceTests.cs`:** solo `RackUnitsGuard.cs` lee `Insunits` (:88-101), y hay un censo exacto de llamadas a `WarnIfNotInches` por archivo (:105-118).
- **`PushBackPluginSourceGuardTests.cs`:**
  - cada fachada de Push Back llama a `ViewBlockDraw.DrawAndPlace(` y a `ViewBlockDraw.RedrawInPlace(` (:120-128);
  - cada una usa su builder (:131-136).
- **`PushBackRoundTripSourceGuardTests.cs:237-239`:** RACKEDITAR sigue usando los tres `RedrawInPlace`.
- **`RackProjectionCommandGuardTests.cs:123`:** el lector de RACKPROYECTAR tiene un solo `RackSiblingScan.CaptureDrawing(`.
- **ADR-0003:** el proyecto de pruebas no referencia el Plugin. `tests/RackCad.Tests/RackCad.Tests.csproj:23-31` solo referencia Domain, Application y herramientas. Cualquier prueba del Plugin es una **guarda de fuente**.

---

## Implicaciones para el contrato de T4B

### Archivos que tocaría (por medición de las costuras)

1. **Variante caller-owned de la lectura** (R-T4-01, INV-39) en `src/RackCad.Plugin/Views/RackSiblingScan.cs`.
   - Hoy `Traverse` toma bloqueo, transacción y `Commit` (H-18). La variante necesita separar el recorrido en un método que reciba `(Transaction, Database)`, y que `Traverse` conserve su firma y su único `Commit` como envoltorio.
   - Las guardas de H-25 (firmas exactas, un `new RackSiblingDrawingScan(`, un `RackBlockData.Read(`, un `rawEnvelopes.Add`, un `.Commit(`) **son compatibles con esa extracción**, siempre que la construcción y la lectura se muevan al método común y no se dupliquen.
   - Hay que declarar si alguna aserción (por ejemplo `AssertInOrder` sobre el texto del archivo) cambia de cuerpo.
   - Debe conservar `eraseByKind` nulo (C-F4-05) y la misma `CompletenessEvidence`, o una evidencia declarada.
2. **Un archivo nuevo del Plugin para MUTATE/POST**: no puede ir en `Plugin/Workspace/`, que tiene 9 archivos fijos y prohíbe `Regen` (H-24). Tampoco en `RackPilotIntentAdapter.cs` (H-25).
   - **(inferencia)** El sitio coherente es `src/RackCad.Plugin/Views/`, donde ya están `RackProjectionWriteScope.cs` y el adaptador del piloto, o `src/RackCad.Plugin/Systems/PushBack/`.
   - No puede llamarse `*Commands*.cs` si nombra `SiblingRedrawTransaction` (H-13).
   - No puede nombrar `NavigationTargets` (H-21).
   - Necesita un namespace de bloque (H-27).
3. **Una corrida pura en Application** para el orden y la clasificación de INV-30, probada con puertos simulados.
   - Precedentes de forma: `RackSiblingRedrawRun` + `ISiblingRedrawPort` (H-7) y `RackProjectionMaterializationRun` + `IRackProjectionWriteScope(Factory)` (H-10).
   - **(inferencia)** Iría en `src/RackCad.Application/Systems/PushBack/Pilot/`, que queda fuera de la clausura de T1B (§3.1 :197). No en `Application/Workspace/`:
     - allí `Regen` está prohibido en todos los archivos (H-24);
     - y `Transaction`, `Database` y `Commit(` lo están, incluso en comentarios, en `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs` (`WorkspaceEditIntentGuardTests.cs:1003-1011`).
   - Hay que decidir si la guarda de pureza de `PlanPath` (H-26) se extiende al archivo nuevo, o si este nombra `Commit` como método de puerto.
4. **Las fachadas de Push Back no necesitan cambiar**: ya exponen `PrepareRedraw` y `RedrawInTransaction` (H-1). Solo se consumen. Si se tocaran, entrarían en la clausura de T1B (R-T4-12).
5. **Las guardas que hay que extender:**
   - `CallerOwnedFacadeGuardTests.Facades` (:50-56): añadir las tres fachadas de Push Back. Las propiedades actuales ya se cumplen sobre su texto, porque los cuerpos son expresiones `=>` hacia `ViewBlockDraw` (H-1). Es la «guarda de fachadas» de §3.2 :213;
   - una guarda de la variante de lectura, con el patrón `CallerOwnedFacadeGuardTests` (INV-39: sin `StartTransaction`, `LockDocument` ni `Commit`);
   - una guarda de fuente del archivo nuevo de MUTATE/POST, con el patrón `ProjectVariableMutationExecutorTests:212-337`:
     - una `StartTransaction` y un `Commit()`;
     - `Commit()` en su propio `try`;
     - `PrepareRedraw(` antes de `StartTransaction`;
     - en el orden de MUTATE: la relectura, la comparación con `ExpectedBase`, el destino y las unidades;
     - `PurgeAfterCommit` y `SyncName` después del `Commit`, y un solo `ApplyRegen`;
     - ningún `EraseViewBlocks`/`Erase(`, porque el piloto nunca borra (R-T4-11);
     - ninguna `RackUnitsGuard`, porque es una actualización pura (H-14 paso 8);
   - una guarda de la inyección Debug de INV-30, con el patrón `DebugFaultInjectionGuardTests` (`SiblingRedrawSeamGuardTests.cs:37-56`).

### Costuras y puertos simulables que ya existen

- **PREPARE con importación:** las tres `*DrawService.PrepareRedraw` importan con `EnsureForPlan` antes de cualquier transacción (H-2). Es el punto natural de R-T4-03.
  - Para probarlo sin AutoCAD, el puerto puro equivalente es `LibraryBlockAvailabilityFlow` con `ILibraryBlockQuery` e `ILibraryBlockImporter` (H-16).
- **MUTATE caller-owned:** las tres `*DrawService.RedrawInTransaction` y `SystemBlockWriter.RedefineInTransaction` (H-4). La unidad `SiblingRedrawUnit(key, Func<Transaction,…>, Action post)` puede reutilizarse (H-7).
  - `SiblingRedrawTransaction.ApplyInTransaction` aplica unidades sin confirmar, y su inyección Debug es por unidad (H-7, H-12).
  - **(inferencia)** Reutilizarla sin tocar su archivo respeta «sin cambiar la costura para Insertar» (INV-30). La propia `Mutate`, en cambio, no sirve: su `Commit()` comparte `try` y cualquier excepción da `Discarded` (H-7).
- **La identidad de la transacción:** el patrón `top.UnmanagedObject != transaction.UnmanagedObject` (H-11) sirve para que la variante de lectura y las unidades comprueben que operan dentro de la transacción poseída.
- **La comprobación de destino:** replicar la semántica de `MutationDestinationBinding.Bind` (H-9) sobre las claves `plan.Members` frente a los `Mutable` de la relectura de MUTATE, **dentro de** la transacción. En Selectivo se hace en PREPARE (H-8), lo que es una diferencia declarable.
- **POST:** `PurgeAfterCommit` y `SyncName` son *best effort*, con su propia transacción y tragándose las excepciones (H-5, H-15). `ApplyRegen` es la autoridad única (H-4). La relectura final puede hacerse con `CaptureDrawing`, sin cambios, después del `Commit`.
- **Los nombres de POST:** `RackViewBaseName.LinkedPlanta`, `LinkedPushBackCut` y `LinkedLateral` (`src/RackCad.Application/Systems/Shared/RackViewBaseName.cs:164-180…`), como en RACKEDITAR (H-14 paso 11).

---

## Riesgos y ambigüedades

1. **El plan no lleva el `PushBackSystem`, ni `end`/`side`, ni `ObjectId` (H-22).** Las fachadas sí los piden (H-1).
   - Hay que decidir quién reconstruye el sistema: deserializar `DesignText` y resolver con `PushBackResolver`, o ampliar el plan.
   - RACKEDITAR prohíbe volver a resolver lo que dio la ventana (`RackPushBackCommands.cs:177`). Variables de proyecto sí resuelve en su PREPARE (H-8).
   - Además, `end`/`side` tienen dos decodificadores posibles: `DecodeSection` o `RackViewCodec`. Hay que fijar uno y demostrar que son equivalentes.
2. **`PreparedViewRedraw` fija el `ObjectId` en PREPARE (H-3).** R-T4-02 quiere la relectura y el destino dentro de la transacción.
   - Hace falta una clave → `ObjectId` antes de la transacción. `RackPilotIntentAdapter.Read` descarta `Definitions` (H-17) y no hay conversión de una clave hexadecimal a `Handle` (H-25 prohíbe `Convert.ToInt64` y `Handle.Parse` en el adaptador).
   - Dentro de MUTATE hay que comparar el `BlockId` preparado con el `ObjectId` de la relectura caller-owned.
3. **Cada `PrepareRedraw` carga su propio catálogo** (`ViewBlockDraw.cs:84`), distinto del catálogo con el que T4A1 construyó el plan. Si el archivo del catálogo cambia entre ambos momentos, la geometría escrita podría no corresponder al plan. El riesgo es bajo, pero existe.
4. **Conservación de bytes (R-T4-04):**
   - `RackBlockData.Write` trocea con `Substring` cada 255 caracteres UTF-16 (H-5). Un par sustituto partido en una frontera de trozo podría no sobrevivir en el `Xrecord`. Solo se mide en host (Smoke-2);
   - `Write` **vuelve en silencio** con un JSON vacío (:22-25). La comparación de POST lo detectaría;
   - si se mide pérdida, es STOP según §7 :543. El precedente de lectura de vuelta es `RackDefinitionCreator.Envelope:200-213`.
5. **INV-30 no tiene ningún precedente.** Ningún camino del Plugin pone `Commit()` en su propio `try` (H-7, H-8, H-10), y el único inyector Debug es por unidad (H-12).
   - La clasificación (antes de `Commit` = No aplicado; en `Commit` = `CommitThrew`) solo se puede probar en Core, con una corrida pura y un puerto cuyo `Commit` lance.
   - En el Plugin solo cabe una guarda de fuente. Sin AutoCAD no hay ejecución.
6. **Bloqueos anidados.**
   - `CaptureDrawing` toma su propio bloqueo dentro del bloqueo del host (H-18), aunque el comentario del adaptador diga que «takes no lock of its own» (`RackPilotIntentAdapter.cs:66`).
   - `AutoCadSiblingRedrawPort`, `SyncName` y `EraseViewBlocks` también bloquean por paso.
   - R-T4-06 (T4C) pide un único `using LockDocument`. **(inferencia)** Los bloqueos anidados son reentrantes en AutoCAD, pero no se ha medido aquí. La variante caller-owned no debe bloquear (R-T4-01).
7. **C-F4-05 no sale de la disposición de I-55.** Con `eraseByKind` nulo, `DeferToFirstPlacement` no puede darse (H-20). T4B debe contar los supervivientes (`IsSurvivor`) sobre los `Facts` de su relectura, y la base no lleva el recuento de referencias.
8. **`TopTransaction == null` al salir.** Los precedentes Debug usan `Debug.Assert` (adaptador, controlador) o **solo registran en el log** (puente, porque un `Assert` terminaría AutoCAD; H-11). Hay que fijar cuál rige en MUTATE.
9. **La expectativa de POST incluye `AnnotationScale`** (H-22), pero ningún lector del Plugin la extrae por separado. Va dentro del JSON del diseño. Hay que fijar si se compara solo por los bytes del sobre o también tras materializar la relectura.
10. **`ExpectedVariableRegistryText` siempre llega nulo**, porque el lector no lee el registro (H-19). La comparación con `ExpectedBase` lo hereda.
11. **«Unidades» en R-T4-02 son las unidades de escritura por vista (INV-10)**, no INSUNITS. La actualización pura de RACKEDITAR no avisa de INSUNITS (H-14 paso 8), y el censo de `WarnIfNotInches` es exacto (H-27).
12. **`LegacyDeltas` (decisión 3)** no es un símbolo de `src` ni de `tests` en `c3ae1c97` (`git grep -i LegacyDelta`: solo aparece en documentos). Su canonicalización hay que localizarla en Application. Queda fuera de este mapa.
13. **Residuo OM-5:** la importación usa `WblockCloneObjects` fuera de la transacción authored (H-16), así que un Conflicto que solo detecte MUTATE deja definiciones importadas. D-13 lo admite (§3.2 :210). El agrupamiento de Undo se mide en T4C y en Smoke-2.
