# F4-S2-03: mapa de código (solo lectura)

Worktree `architecture/workspace-persistente-rackcad`, HEAD `e67fb511`. Las rutas son relativas a la raíz del worktree.
No se ejecutó nada (ni build ni tests) y no se leyó `docs/automation/evidence/`.

## 0. Hallazgo principal

El candidato «abrir para escritura cada referencia de nivel superior de la BTR redefinida y llamar a `RecordGraphicsModified(true)` dentro
de la misma transacción» **ya existe**. Está en el código compartido de redefinición
(`src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:163-171`) y se ejecuta dentro de la transacción del Actualizar. La medición F4-S2-03 se
hizo con ese código activo, así que ese candidato solo no basta. Ningún arreglo del lado del escritor está medido. Lo único probado es
`Editor.Regen()` en idle después de U/UNDO/REDO/MREDO (lo dice el encargo).

## 1. Ruta de escritura de «Actualizar», de punta a punta

**Contexto de comando, bloqueo y paso de UNDO**
- La ejecuta `ExecuteInCommandContextAsync`: `src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs:33-47`. Es el único lugar del
  producto (lo fija `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:270`).
- Desde el callback se encadenan:
  - `PushBackPilotHostRunner.OnCallback` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs:157-174`);
  - `RunLocked` (`:234-293`), con `target.Lock()` en `:241`, que es `_document.LockDocument()` en `RackPilotUpdateCommandContext.cs:114`;
  - `_run.Begin()` en `:246`, que abre la mutación propia (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:1618-1631`);
  - `_run.Execute(writer)` en `:251`, y en el `finally` el retiro del escritor y el desbloqueo (`:268-291`).
- Todo ocurre dentro del comando interno `EXECUTEFUNCTION`, que el puente reconoce por nombre
  (`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:207`). Es un solo paso de UNDO (`docs/initiatives/I-64-proposal-v5.md:542-543`).
- Orden del run: `Prepare ?? Mutate ?? Post`
  (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:264`).

**PREPARE** (`src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:101-143`)
- `PrepareRedraw` de las tres fachadas Push Back (`:122-136`). Cada una llama a `ViewBlockDraw.PrepareRedraw`, que importa la biblioteca
  con `EnsureForPlan` fuera de la transacción (`src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:65-89`, `:86`).
- Fachadas:
  - `src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs:58-66`;
  - `src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs:56-64`;
  - `src/RackCad.Plugin/Systems/PushBack/PushBackPlantaDrawService.cs:51-58`.
- Los planes los construyen los builders puros de Application, por ejemplo
  `src/RackCad.Application/Systems/PushBack/PushBackSystemFrontalBuilder.cs:88`. Pueden traer `HeaderGroup`, es decir, BTR anidadas nuevas.

**MUTATE: una sola transacción**
- `BeginMutation` comprueba `TopTransaction` y luego llama a `StartTransaction()` (`RackPilotUpdateWriter.cs:146-157`, `:152`, `:155`).
- `WriteUnit` (`:262-299`) llama a `RedrawInTransaction` de la fachada (`:275-286`). La fachada llama a `ViewBlockDraw.RedrawInTransaction`
  (`ViewBlockDraw.cs:97-109`).
- Después viene `SystemBlockWriter.RedefineInTransaction` (`src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:126-138`), que hace dos
  cosas:
  - `drawer.RedefineSystemBlock` (`:135`);
  - `RackBlockData.Write` sobre la BTR (`:136`). Abre la BTR `ForWrite` y escribe el Xrecord de su diccionario de extensión
    (`src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:27-50`).
- `Seal()` llama a `_transaction.Commit()` (`RackPilotUpdateWriter.cs:301-307`, `:305`).
- El run libera el ámbito con `mutation.Dispose()` (`PushBackPilotUpdateRun.cs:372`).

**Qué se hace en `LateralHeaderDrawer.RedefineSystemBlock`** (`src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:88-182`)
- Abre la `BlockTable` `ForWrite` (`:91`) y la BTR de la vista `ForWrite` (`:92`).
- Recoge las definiciones anidadas y los `*D` de las cotas (`:97-113`) y **borra todo el contenido** de la BTR (`:115-118`).
- Crea BTR de cabecera nuevas con nombre único (`NewBlock`, `:132`, `:240-247`) y les añade referencias (`:143-152`).
- Añade las piezas sueltas con `AppendInstance` (`:155-161`, `:252-321`).
- **Referencias de nivel superior** (`:163-171`):
  ```csharp
  foreach (ObjectId referenceId in systemDef.GetBlockReferenceIds(directOnly: true, forceValidity: true))
  {
      var reference = (BlockReference)tr.GetObject(referenceId, OpenMode.ForWrite);
      reference.RecordGraphicsModified(true);
  }
  ```
  - Se abren `ForWrite` y se marcan con `RecordGraphicsModified(true)`. Ninguna propiedad cambia.
  - **No se borran, no se recrean y no se reapuntan.** Solo se reemplaza el contenido de su BTR, que conserva su id y su nombre
    (comentario en `:84-86`).
  - `directOnly: true` devuelve todas las referencias directas de la BTR, sean del espacio modelo, del espacio papel o de otra definición.
- `RecordGraphicsModified(true)` aparece en otros dos sitios, y en ninguno sobre referencias de nivel superior:
  - en las referencias anidadas nuevas de bloques dinámicos dentro de la BTR, solo si se aplicó un parámetro (`:445-448`);
  - en Cantilever, con el mismo patrón, fuera de Push Back (`src/RackCad.Plugin/Drawing/Cantilever/CantileverViewMaterializer.cs:81-86`).
- Las definiciones obsoletas se calculan como conjunto (`:177-178`) y se purgan en POST.
- Las capas de esas referencias directas ya entran en la comprobación de capas bloqueadas del piloto:
  - `RackPilotUpdateWriter.cs:57-62`;
  - la línea base de la travesía, en `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:994-999`.

**POST: fuera de MUTATE, dentro del mismo bloqueo y del mismo `EXECUTEFUNCTION`** (`RackPilotUpdateWriter.cs:160-207`)
1. Ningún ámbito ajeno abierto (`:169`).
2. `PurgeAfterCommit` (`:174`): una transacción propia que llama a `Purge` y borra BTR (`LateralHeaderDrawer.cs:193-238`).
3. `RackBlockRenamer.SyncName` (`:177-180`): `LockDocument` más una transacción propia, y luego `UpgradeOpen`, `Name` y `Commit`
   (`src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:31-50`).
4. Lectura de nombres con `StartOpenCloseTransaction` (`:184-191`).
5. **`SystemBlockWriter.ApplyRegen(document, true)`** (`:194-202`), que es `document.Editor.Regen()` (`SystemBlockWriter.cs:157-163`).
   Ocurre después del `Commit` (`:305`), de la purga y del renombrado. Ocurre antes de la relectura y antes de soltar el bloqueo
   (`PushBackPilotHostRun.cs:281-285`) y el contexto de comando. Por tanto, **el Regen forma parte del mismo paso de UNDO**, y un U posterior
   no lo vuelve a ejecutar.
6. `RackSiblingScan.CaptureDrawing` (`:205`).

## 2. Ruta clásica (`RACKEDITAR` de Push Back)

**Entrada**
- `[CommandMethod("RACKEDITAR")]` en `src/RackCad.Plugin/RackMenuCommands.cs:153`, con despacho en `:179-184`.
- Pasa por `src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs:23-24` y llega a `RackPushBackCommands.EditPushBack`
  (`src/RackCad.Plugin/RackPushBackCommands.cs:147`).

**Redibujo por vista** (`RackPushBackCommands.cs:272-346`)
- Cada vista llama a `RedrawInPlace(..., regen: false)`: planta en `:279`, frontal en `:297` y lateral en `:336`.
- Recorrido: `PushBack*DrawService.RedrawInPlace` (`PushBackSystemDrawService.cs:41-56`, `PushBackFrontalDrawService.cs:38-54`,
  `PushBackPlantaDrawService.cs:35-49`), luego `ViewBlockDraw.RedrawInPlace` (`ViewBlockDraw.cs:114-143`) y luego
  `SystemBlockWriter.RedrawInPlace` (`SystemBlockWriter.cs:67-102`).
- `SystemBlockWriter.RedrawInPlace` hace, **por vista**: `LockDocument` (`:77`), `EnsureForPlan` (`:81`), `StartTransaction` (`:83`),
  `RedefineInTransaction` (`:86`), `Commit` (`:87`), purga (`:91`) y `ApplyRegen(false)` (`:92`).
- Después, `SyncName` por vista: `:282`, `:300-309` y `:339-342`.
- `EraseViewBlocks` de los cortes obsoletos (`:349-352`): esto solo lo hace la ruta clásica.
- **Un único `document.Editor.Regen()`** al final (`:362-367`), dentro del comando `RACKEDITAR`.

**Qué comparte con «Actualizar»**
- El mismo primitivo de mutación: `SystemBlockWriter.RedefineInTransaction`, luego `LateralHeaderDrawer.RedefineSystemBlock`, que incluye
  el toque de `:163-171`, y `RackBlockData.Write`.
- La misma purga (`PurgeUnreferenced`) y el mismo renombrado (`SyncName`).
- La misma posición del Regen: uno solo, después de todos los commits, purgas y renombrados, y dentro del comando, es decir, dentro de su
  propio paso de UNDO.

**Qué no comparte**
- El número de transacciones: la clásica abre una por vista; el piloto abre una para todo.
- El tipo de comando: comando registrado frente a `ExecuteInCommandContextAsync`.
- El borrado de fantasmas, que solo hace la clásica.

**`SiblingRedrawTransaction` no está en ninguna de las dos rutas.**
- Sus únicos usos son selectivos: `src/RackCad.Plugin/Systems/Shared/AutoCadSiblingRedrawPort.cs:33` y `:44`, y
  `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs:177` y `:180`.
- Su `Post` hace un Regen (`src/RackCad.Plugin/Systems/Shared/SiblingRedrawTransaction.cs:55-66`).

**Consecuencia:** por la mecánica compartida, cabe esperar el mismo «obsoleto tras U» en la ruta clásica. **No está medido.** El paquete C1
lo registra como hecho pendiente (`docs/initiatives/I-64-f4-t4-host-c1-gate-contract-package.md:174`).

## 3. Candidatos del lado del escritor, dentro de la transacción del Actualizar

Un `Regen` dentro del propio paso nunca cubre un U posterior: el U no reejecuta el Regen (§1, POST paso 5). La mecánica de Actualizar no está
congelada (`I-64-proposal-v5.md:506`). Sí lo están el resultado «todo o nada» (`:502-503`) y el paso de UNDO único (`:542-543`).

**C1. Abrir `ForWrite` cada referencia directa y llamar a `RecordGraphicsModified(true)`**
- **Ya implementado** en `LateralHeaderDrawer.cs:163-171`. No hay que cambiar ningún archivo.
- La medición S2-03 lo incluye, así que no corrige el defecto.
- Lo fija `tests/RackCad.Tests/InsertRedrawLayerAccessCharacterizationTests.cs:22-28`, que exige el texto
  `tr.GetObject(referenceId, OpenMode.ForWrite)` en `:27`.
- Ningún test nombra `RecordGraphicsModified`: grep en `tests/` sin `bin` ni `obj`, cero resultados.

**C2. Otro toque registrado en el undo de cada referencia directa, en el mismo bucle**
- Por ejemplo, reasignar una propiedad de la referencia a su mismo valor. Su efecto sobre U **no está medido**.
- Archivo que cambia: solo `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`, en el bucle `:167-171`. Ese archivo lo comparten todas las
  redefiniciones: Selectivo, Dinámico, Cabecera, Cama, Push Back y el ejecutor de variables.
- Tests:
  - `InsertRedrawLayerAccessCharacterizationTests.cs:27` sigue pasando mientras se conserve ese texto.
  - `tests/RackCad.Tests/RackDefinitionCreatorGuardTests.cs:218-227` y `:248-253` prohíben `Regen`, `Editor`, `ModelSpace`, `PaperSpace`,
    `Commit(`, etc. en `CreateSystemBlock`, `NewBlock`, `AppendInstance`, `ApplyDynamicParameters` y demás. Solo saltan si el toque se pone
    en uno de esos métodos o en un helper que se añada a esa lista. No saltan si va en `RedefineSystemBlock`.
  - `tests/RackCad.Tests/CallerOwnedTransactionGuardTests.cs:91-125` exigen que el cuerpo de `RedefineInTransaction` no contenga
    `Commit()`, `Regen`, `StartTransaction`, `LockDocument`, `EnsureForPlan` ni `BlockLibraryImporter`.
  - `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:50-62` y `:119-175` imponen lo mismo a `RedrawInTransaction` de las 7 fachadas,
    incluidas las 3 de Push Back.
  - Mientras el toque no use ninguno de esos nombres, ninguno salta.

**C3. Borrar y recrear las referencias de nivel superior**
- Habría que copiar posición, escala, rotación, capa, XData y atributos.
- Archivos: `LateralHeaderDrawer.cs`, en el mismo bucle o en otro nuevo.
- Cambia los handles, y eso afecta a varias autoridades:
  - los objetivos de navegación, ordenados «por handle» (`I-64-proposal-v5.md:302` y D-07);
  - el eco por conjunto de handles (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:215-217`);
  - el comentario del escritor «It never erases a view» (`RackPilotUpdateWriter.cs:26`).
- Si el código se pone en el escritor, saltan:
  - `WorkspacePilotUpdateGuardTests.cs:189` (`\bErase\s*\(`), `:195` (`ForWrite`, `UpgradeOpen`, `AppendEntity`) y `:185`
    (`GetBlockReferenceIds`);
  - los conteos exactos de G9 (`WorkspacePilotUpdateIntegrationGuardTests.cs:681-687`, entrada del escritor en `:683`).
- Si se pone en el drawer, salta `InsertRedrawLayerAccessCharacterizationTests.cs:27` en caso de que desaparezca ese texto.

**C4. BTR nueva y reapuntar cada referencia (`BlockTableRecord = nuevo`) en lugar de redefinir en sitio**
- Archivos: `LateralHeaderDrawer.cs`, `SystemBlockWriter.cs` (`RackBlockData.Write` sobre otro id) y `RackPilotUpdateWriter.cs`.
- Rompe la clave por handle de la definición:
  - la travesía congelada, con `key = definitionId.Handle` (`WorkspacePilotUpdateGuardTests.cs:990`);
  - `PreparedBlockMatches` (`RackPilotUpdateWriter.cs:256-258`);
  - el comentario «the definition does not change» (`PushBackPilotUpdateRun.cs:480`).
- El cambio en los tests es amplio: la travesía, G9 y la estructura del escritor (`WorkspacePilotUpdateGuardTests.cs:215-255`).
  No lo enumero entero: excede un arreglo acotado.

**C5. Hacer el toque C1 o C2 en `RackPilotUpdateWriter.cs`, en lugar del drawer**
- Saltan `WorkspacePilotUpdateGuardTests.cs:185` (`GetBlockReferenceIds`), `:195` (`ForWrite`, `UpgradeOpen`) y su teoría negativa
  (`:312-326`).
- Saltan los conteos de `WorkspacePilotUpdateIntegrationGuardTests.cs:683`.
- Además, separaría el piloto de la ruta clásica.

**Tests de la ruta clásica que solo cambian si cambia la ruta clásica**
- `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs:235-270`: un único `Editor.Regen()` y redibujos con
  `regen: false`.
- `tests/RackCad.Tests/PushBackRoundTripSourceGuardTests.cs:235-248` y `:349-357`.
- `tests/RackCad.Tests/CallerOwnedTransactionGuardTests.cs:213-231`: Regen después de la purga, uno por ruta.

## 4. Diseño con Regen en el drenaje (opción a)

**Operaciones del drenaje en código**
- `src/RackCad.Application/Workspace/Hints.cs:14-21`: `ReadImplicitSelection`, `RecomputeSelectionContext`, `InvalidateNavigationCaches` y
  `MarkDraftsPossiblyStale`.
- Las produce `WorkspaceSession.TryDrain` (`src/RackCad.Application/Workspace/WorkspaceSession.cs:52-72`, `:59-68`).

**Puntos de entrada**
- `EventBridgeCore` (`src/RackCad.Application/Workspace/EventBridgeCore.cs`):
  - `OnSourceEvent` (`:312-366`), con `CommandEnded` en `:330-345` y los eventos de objeto en `:346-353`;
  - `OnObjectEvent` (`:376-392`);
  - `OnIdle` (`:430-451`);
  - `CanDrainNow` (`:461-473`);
  - `OpenOwnMutation` (`:489`), `CloseOwnMutation` (`:496`) y `NoteForeignCommandStart` (`:506`).
- `AutoCadEventBridge` (`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`):
  - `Hook` (`:148-284`), con `CommandWillStart` en `:199-211` y `CommandEnded` en `:212-218`;
  - el manejador de `CommandEnded` es exactamente
    `ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source))` en `:215` y no lee el nombre del comando;
  - los manejadores de objeto (`:240-274`) y `SubjectOf`, que clasifica solo por tipo (`:351`);
  - `Idle` (`:275-280`), `OnIdleTick` (`:287-290`), `RunIdleContinuation` (`:294-298`) y `OnIdle`, que lanza `Drained` (`:318-331`).
- `WorkspaceController.OnDrained` (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:226-249`):
  - suscripción en `:65`;
  - `_host.OnDrained` en `:230`, `_navigation.OnDrained` en `:231` y `AssertNoOpenTransaction` en `:232`;
  - la ventana de aviso (`:241-248`).
- `WorkspaceHostCore.OnDrained` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:173-229`).

**Guardas que clasifican `Regen` como escritura de vista o lo prohíben en manejadores y drenaje**
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs`:
  - sección INV-F1-T2-03 (`:137`);
  - `ForbiddenCallNames`, que incluye `Regen` (`:139-142`);
  - excepciones por archivo exacto (`:163-168`);
  - test sobre todo `src/RackCad.Plugin/Workspace/` (`:170-180`).
- `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:382-396` (INV-F1-T3A-08): prohíbe `Regen` en todos los archivos de
  `Plugin/Workspace` y `Application/Workspace`, incluido el adaptador de efectos.
- `tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs:109-124` y `:173-193`: los adaptadores lector y de efectos siguen sin
  poder hacer `ed.Regen();`.
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs:119-129`: el puente no regenera.
  `:106-117` fija los conteos 16/16/16 de `+=`, `-=` y `handler`.
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeObjectSubjectGuardTests.cs`:
  - `:125` y `:227-234`: «the bridge regenerates»;
  - `:61-67` y `:101-104`: el manejador de `CommandEnded` debe ser exactamente la línea de `OnSourceEvent`. Leer `e.GlobalCommandName` ahí
    para detectar U, UNDO, REDO o MREDO lo rompe;
  - `:86-98`: `SubjectOf` solo prueba dos tipos;
  - `:121-124`: 16, 16, 6 y 5.
- `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:683-692` (F2): el adaptador de efectos no hace `Regen`.
- `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs:109-212` (IC-04): el callback de Idle solo programa.
- Otras guardas que también prohíben `Regen`, aunque no por los manejadores ni el drenaje:
  - `tests/RackCad.Tests/Workspace/WorkspaceHostPortGuardTests.cs:24-28` (nombres de método `Regen*` en el puerto);
  - `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:289-297` (adaptador de intención);
  - `tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs:258-259` y `:316` (núcleo de borradores).
- La pertenencia de `Regen` a esta clasificación también está escrita en `docs/initiatives/I-64-f1-t3-gate-contract-package.md:116-117`.

**Guardas que fijan el conjunto de operaciones del drenaje o la superficie del puente**
- `tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs:92-102`: el enum es exactamente las 4 operaciones.
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeHintAndDrainTests.cs:98-100` y `:105-116`: listas exactas por pista.
  `CommandEnded` u `ObjectModified` dan exactamente `{InvalidateNavigationCaches, MarkDraftsPossiblyStale}`.
- `tests/RackCad.Tests/Workspace/WorkspaceOwnMutationTests.cs:44-57` y `:350`.
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs:101-103` (superficie pública) y `:106-117`.
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs:79-84` y `:101-123`: 16 eventos gobernados, cada uno enganchado una vez.
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:590-630` (G6): `WorkspaceController.cs` tiene cero APIs de
  escritura y bloqueo. Añadir ahí `LockDocument` salta (`:640`).
- Cuerpos de `OnDrained` fijados:
  - `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:175-195`;
  - `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:457-468`;
  - `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:716-724`;
  - `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:1200-1225`.

**Cláusulas congeladas afectadas** (`docs/initiatives/I-64-proposal-v5.md`)
- `:227-232`: drenaje, «Operaciones permitidas en el drenaje, y solo estas» (1 a 4), con P-04 en `:632`.
- `:236-237`: Undo/redo. `CommandEnded` de U, UNDO, REDO y MREDO «invalidan y marcan».
- `:239-240`: «Reentrada estructural: ningún evento ni cambio de contexto produce una escritura de selección o de vista (D-06, D-07)».
- `:244`: ningún manejador vuelve a disparar el mismo evento.
- `:339`: D-06, «Los eventos nunca causan escritura de selección ni de vista».
- `:363-364`: D-07, eco congelado: «no existe ningún camino desde un evento … hacia una escritura de selección o de vista».
- `:217-219` y P-03 en `:631`: los manejadores solo encolan.
- P-02 en `:630` e INV-01 en `:739`: `Regen` solo con pistas pendientes. INV-03 en `:741`.
- Antecedente: el paquete C1 ya analizó las dos formas (diag2 en el puente y la forma con operación nueva) y lo que rompe cada una
  (`docs/initiatives/I-64-f4-t4-host-c1-gate-contract-package.md:161-176`). La parada `C1-S04` prohíbe cualquier tratamiento de U, UNDO,
  REDO o MREDO o cualquier `Regen` (`:302`).

## 5. Código de `src/` que reacciona a undo y redo

- **No encontrado** ningún tratamiento específico. Un grep de `"U"`, `UNDO`, `REDO`, `MREDO`, `UndoRecording` e `IsUndoing` en `src/` no
  da ningún resultado funcional. Las únicas coincidencias con `U` son comentarios de Cantilever sin relación
  (`src/RackCad.Application/Systems/Cantilever/CantileverPunchPlan.cs:65-66`).
- La única reacción es genérica:
  - el `CommandWillStart` de U cuenta como comando ajeno (`AutoCadEventBridge.cs:207`);
  - el `CommandEnded` de U encola `HintKind.CommandEnded` como ajeno. La mutación propia ya se cerró con el primer `CommandEnded` tras
    abrirse (`EventBridgeCore.cs:333-343`);
  - los eventos de objeto que produce el undo encolan `DatabaseObjectChanged` (`EventBridgeCore.cs:346-353`; filtro asociativo en
    `:376-392`);
  - en el drenaje se ejecutan `InvalidateNavigationCaches` y `MarkDraftsPossiblyStale` (`WorkspaceSession.cs:66-68`). Eso termina en
    `MarkPilotDraftsPossiblyStale` y `_reader.Invalidate` (`WorkspaceHostCore.cs:182` y `:195-200`), y en las invalidaciones de
    `src/RackCad.Application/Workspace/NavigationTargets.cs:87` y `src/RackCad.Application/Workspace/BrowserRackSummaries.cs:94`.
- Ninguna de esas reacciones regenera ni escribe la vista.
