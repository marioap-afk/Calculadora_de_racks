# Mapa 4: el movimiento de `Traverse` y lo que T1B cubre

**Base medida:** HEAD `7c29a5e5`. `git diff c3ae1c97 HEAD -- src` está vacío: el producto es el de `c3ae1c97`. Los únicos cambios de `tests/` son los siete archivos de T1B.
**Método:** solo lectura de archivos y `git grep/show/diff`. No se compiló ni se ejecutó ninguna prueba.
**Pregunta:** ¿se cumple la condición (3) del Owner, «la caracterización de T1B protege el comportamiento existente relevante» para el movimiento de `Traverse` que propone T4B?

---

## 1. `Traverse` hoy y lo que propone T4B

### 1.1 La definición actual (`src/RackCad.Plugin/Views/RackSiblingScan.cs`)

- **`Capture(...)`** (`:79-92`): `Traverse(document, selectedDefinition, eraseByKind)` (`:87`), y después `RackSiblingMembership.Classify(scan.Facts, rackId, originalSourceId, originalSourceIdIsAttributable)` (`:88-89`). Devuelve un `RackSiblingScanSnapshot`.
- **`CaptureDrawing(Document)`** (`:95-96`): `=> Traverse(document, ObjectId.Null, null)`.
- **`private static RackSiblingDrawingScan Traverse(Document, ObjectId, Func<RackEmbedDocument,bool>)`** (`:98-170`):
  - `:103-110`: seis diccionarios con `StringComparer.Ordinal`, más `new RackEmbedStore()` y `new CustomPropertiesStore()` como locales.
  - `:112`: `using (document.LockDocument())`.
  - `:113`: `using (var transaction = document.Database.TransactionManager.StartTransaction())`.
  - `:115-161`, el bucle sobre la `BlockTable`:
    - omite las definiciones `IsLayout`/`IsAnonymous` (`:119`) y las que tienen el JSON vacío (`:121-122`);
    - `Deserialize` (`:124`); la clave es `Handle.ToString()` (`:125`);
    - cuenta las referencias directas de layout y las anidadas, con `GetBlockReferenceIds(directOnly: true, forceValidity: true)` (`:129-135`);
    - las capas de las referencias y de las entidades, con `AddLayer` (`:134`, `:137-139`);
    - `probe = envelope?.Id ?? RackEnvelopeIdProbe.Probe(json)` (`:141`);
    - construye `RackSiblingScanFact` (`:142-152`): `definitionId == selectedDefinition`, `IsFromExternalReference` y el cortocircuito `envelope != null && eraseByKind != null && eraseByKind(envelope)`;
    - rellena `definitions`, `rawEnvelopes` y `layers`, y `envelopes`/`properties` solo si hay sobre, con `ReadElement` (`:153-160`).
  - `:163`: `transaction.Commit()`, el único del archivo.
  - `:166-169`: la única construcción `new RackSiblingDrawingScan(...) { RawEnvelopes = new ReadOnlyDictionary<string,string>(rawEnvelopes) }`.
- **Ayuda privada:** `AddLayer` (`:172-179`). Deduplica por `Use` y por el nombre con `OrdinalIgnoreCase`, y captura `IsLocked`.
- **Dependencias externas:**
  - `RackBlockData.Read` (`src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:54`);
  - `RackEnvelopeIdProbe` (`src/RackCad.Application/Views/Insertion/RackEnvelopeIdProbe.cs:7`);
  - `RackSiblingScanFact` (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:16`);
  - `RackSiblingLayerRequirement`/`RackSiblingLayerUse` (`.../Views/Redraw/ISiblingRedrawPort.cs:6-12`).
  - Los dos últimos están en la costura de I-55, que T4B prohíbe tocar. El movimiento no los necesita.
- **No hay ningún `try`/`catch`** en el archivo: una excepción del bucle sube a cada llamador, que tiene su propio `catch`.

### 1.2 La propuesta de T4B

Paquete `docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/I-64-f4-t4b-gate-contract-package.md`:

- **D-T4B-04 (`:287-298`):**
  - en `RackSiblingScan.cs`, «un núcleo interno de recorrido que recibe `(Transaction, Database)`»;
  - `Traverse` conserva su firma, su bloqueo, su transacción y su único `Commit`, y llama al núcleo (`:289-290`);
  - se conservan las guardas de cuenta (`:291`);
  - es un «movimiento de código sin cambio de comportamiento» de Insertar, del insertar de RACKEDITAR y de RACKPROYECTAR (`:292`);
  - lo cubren una **guarda nueva** («`Traverse` solo contiene el bloqueo, la transacción, la llamada al núcleo y el `Commit`», más `AssertInOrder` sobre `RackSiblingScan.cs:115-161`, `:293`) y Smoke-2 (`:294`);
  - `eraseByKind` sigue nulo para el piloto (`:295`).
- **D-T4B-03 (`:259`):** `Reread` del escritor nuevo `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs` usa ese núcleo dentro de la transacción de MUTATE.
- **D-T4B-09 (`:408-427`):**
  - la guarda «del movimiento de `Traverse`» va en `TW/WorkspacePilotUpdateGuardTests.cs`, que es nuevo (`:410`, `:419`);
  - «la variante de lectura caller-owned: sin `StartTransaction`, `LockDocument` ni `Commit`» (`:418`);
  - **las pruebas de T1B quedan sin cambios** (`:427`).
- **§6 (`:444-522`):**
  - `RackSiblingScan.cs` está permitido (`:460`, `:480`);
  - INV-13: «los caminos clásicos, Insertar y RACKPROYECTAR no cambian. El movimiento de `Traverse` lo fija su guarda. Las pruebas de T1B y de I-55 siguen en verde» (`:504`);
  - la tabla `RequiredTests` (`:482-489`);
  - Smoke-2 (`:534`): Insertar de Selectivo, el insertar desde RACKEDITAR Push Back y RACKPROYECTAR.
- **Mapa M1 (`map-1-plugin-write-path.md`):** H-18 (`:264-276`), H-25 (`:324-337`) y la implicación 1 (`:363-367`), que ya pide «declarar si alguna aserción `AssertInOrder` … cambia de cuerpo».

**Huecos de redacción, que conviene cerrar en la emisión:**
- La firma del núcleo nombra solo `(Transaction, Database)`, pero `Capture` necesita además `selectedDefinition` y `eraseByKind`.
- No se dice a dónde van las líneas `:103-110` y `:166-169`, que quedan fuera del rango `:115-161` de la guarda.

---

## 2. Llamadores (directos y transitivos en `src/`)

`git grep` da **cuatro** llamadas directas al barrido, y ninguna más:

| # | Llamada directa | Camino del usuario (cadena) | Parámetros |
|---|---|---|---|
| A | `RackSelectivoInsertIntegration.cs:81` `RackSiblingScan.Capture` (dentro de `SelectiveInsertPort.ScanAndClassifyOnce`, `:79-88`) | `RACKEDITAR`/`RED` (`RackMenuCommands.cs:153`, `:22`) → `KindHandlerDispatch.TryResolve` (`:179`) → `handler.Edit` (`:184`) → `SelectiveKindHandler.cs:20` → `RackSelectivoCommands.EditSelective` (`:55`) → rama `if (!window.UpdateOnly)` (`:132`) → `new SelectiveInsertPort` (`:147`) → `port.ScanAndClassifyOnce()` (`:149`) | Con la fuente seleccionada; `eraseByKind` no trivial (`VariantNotPresent`, `:82-86`); la identidad original atribuible si hay `Id`. **Es el único llamador con miembros `Erase`.** Usa `snapshot.Definitions`, `Envelopes`, `Properties` y **`Layers`** (preflight de capas bloqueadas, `:202-255`) |
| B | `Views/RackUnsupportedSiblingInsert.cs:43` `RackSiblingScan.Capture(..., _ => false)` (dentro de `TryAuthorize`, `:36-82`) | `RACKEDITAR` → handler → `EditPushBack` (`RackPushBackCommands.cs:147`, `:193-195`), `EditDynamic` (`RackDinamicoCommands.cs:187-189`), `EditCantilever` (`RackCantileverCommands.cs:246-248`) y `EditCabecera` (`RackCabeceraCommands.cs:279-281`), solo con `!window.UpdateOnly` | `eraseByKind = _ => false`; la identidad original es atribuible solo en Cabecera (`:41-42`). Usa `MutableMembers` → `authorized.Blocks` (`:74-78`) y `AuthoredInput` (AUTH-13). En Push Back los dos alimentan el lote (`RackPushBackCommands.cs:198`, `:250`) |
| C | `Views/RackProjectionSnapshotReader.cs:127` `RackSiblingScan.CaptureDrawing` | `RACKPROYECTAR` (`RackProyectarCommands.cs:23`, `:34`) → `RackProjectionCommandRun.Execute` → `port.Capture()` (`RackProjectionCommandRun.cs:171`) → `AutoCadRackProjectionCommandPort.Capture` → `RackProjectionSnapshotReader.Read` (`RackProjectionCommandPort.cs:73`) | Sin selección; `eraseByKind` nulo. Se llama **después** de que cierre `InDocumentTransaction.Run` (`:52`), sin transacción exterior. El escaneo lo consume `RackProjectionKindSessions.cs:116-244` |
| D | `Views/RackPilotIntentAdapter.cs:116` `RackSiblingScan.CaptureDrawing` | Panel Workspace (piloto): `WorkspaceHostCore.TryBeginPilotEdit`/`TryCheckPilotEdit`/`TryReloadPilotEdit`/`TryConfirmPilotBase`/`TryKeepPilotEdit`/`TryDiscardAndReloadPilotEdit` (`:282`, `:367`, `:440`, `:528`, `:602`, `:687`) → `_editIntentReader.ReadIntent` → `WorkspaceNavigationHost.PilotIntentReader.ReadOnActiveDocument` (`:220-253`, dentro de `using LockDocument` en `:232`) → `RackPilotIntentAdapter.Read` (`:242`). Lo cablea `WorkspaceController.cs:105` | Sin selección; `eraseByKind` nulo. El bloqueo anidado ya existe. Usa `Facts` y `RawEnvelopes` |

**Caminos que no llegan a `Traverse`:**
- Actualizar (`UpdateOnly`) de los cinco tipos usa `RackCommandSupport.FindRackBlocks`/`ScanEnvelopes`.
- No llegan RACKCAMA (Cama no llama a `TryAuthorize`), el menú `RACKCAD` de rack nuevo (los cinco `RackViewBatchExecution.Run` de `RackMenuCommands.cs`, `RackProductSourceKind.NewRack`), RACKPUSHBACK/RPB, RACKDUPLICAR, el BOM y RACKLAYOUT.

**Observaciones:**
- La cobertura de Smoke-2 que propone T4B (A, el B de Push Back y C) **no nombra** los insertar de Dinámico, Cantilever y Cabecera. Comparten exactamente `TryAuthorize`, así que el de Push Back los representa por construcción, pero habría que declararlo.
- La lectura D queda cubierta por «la equivalencia de las tres lecturas».

---

## 3. Pruebas existentes (fuera de T1B) que tocan estos caminos

ADR-0003: `tests/RackCad.Tests/RackCad.Tests.csproj:23-31` no referencia el Plugin, y la CI no tiene AutoCAD. **Ninguna prueba ejecuta `Traverse`.** La CI corre la suite Core completa sin filtro (`.github/workflows/ci.yml:131-138`).

### 3.1 Guardas de texto sobre `RackSiblingScan.cs`

`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs`, con `Src.Code`, que quita los comentarios. **Es el único archivo de pruebas que lee `RackSiblingScan.cs`** (`git grep`).

- **`:74-86`:**
  - `RawEnvelopes { get; init; }`;
  - `rawEnvelopes` con `Ordinal`;
  - **un** `rawEnvelopes.Add(key, json)`, **un** `new ReadOnlyDictionary<string,string>(rawEnvelopes)` y **un** `RackBlockData.Read(`;
  - el orden `RackBlockData.Read(` < `string.IsNullOrEmpty(json)` < `rawEnvelopes.Add(key, json)`. El `AssertInOrder` de `:60-70` busca la **primera aparición** en todo el archivo.
- **`:88-118`:**
  - las firmas exactas de `Capture`, `CaptureDrawing(Document document)` y `private static … Traverse(Document, ObjectId, Func<…>)`;
  - el constructor de 5 parámetros, sin lógica;
  - **una** `new RackSiblingDrawingScan(` y **un** `RawEnvelopes =`, en el inicializador.
- **`:120-128`:** sin `ForWrite`, `UpgradeOpen`, `Erase` ni `RackBlockData.Write`, y **un** `.Commit(` en el archivo.

**Qué fijan:** texto (firmas, cuentas y un orden parcial). Son compatibles con el movimiento, y además lo **obligan a no duplicar**: la variante caller-owned no puede tener su propio `RackBlockData.Read`, su `rawEnvelopes.Add`, su construcción ni su `Commit`.

**Qué no fijan:**
- los cuerpos de `Capture`/`CaptureDrawing` (solo las firmas);
- `LockDocument` y `StartTransaction`, ni su orden respecto al bucle y al `Commit`;
- los filtros `IsLayout`/`IsAnonymous`;
- la clave `Handle.ToString()`;
- `directOnly: true, forceValidity: true` y el conteo de layout frente a anidadas;
- `AddLayer` (los usos, la deduplicación y `IsLocked`);
- el respaldo del `probe`;
- los argumentos del `RackSiblingScanFact`, incluidos `== selectedDefinition` y el cortocircuito de `eraseByKind`;
- que `envelopes`/`properties` se rellenen solo con sobre, y `ReadElement`;
- el `Ordinal` de los otros cinco diccionarios;
- que los almacenes sean locales;
- la ausencia de `try`/`catch`.

### 3.2 Guardas de texto sobre los llamadores (el cableado del escaneo)

| Prueba | Fija (texto) |
|---|---|
| `SingleScanInsertGuardTests` (archivo `tests/RackCad.Tests/G9bInsertWiringGuardTests.cs:7-35`) | A: **un** `RackSiblingScan.Capture(` en `RackSelectivoInsertIntegration.cs` y la reutilización de `snapshot.Membership/Properties/Envelopes/Definitions` (`:10-18`). B: `RackUnsupportedSiblingInsert.TryAuthorize(` presente en los 4 comandos (`:20-26`) |
| `RackViewBatchDriverGuardTests` (`tests/RackCad.Tests/RackViewBatchDriverGuardTests.cs:37-50`) | B: **un** `RackSiblingScan.Capture(` en `RackUnsupportedSiblingInsert.cs`, `snapshot.Membership.Members` y **4** `authorized.Blocks` en los comandos. `:23-35`: los 4 comparadores AUTH-13 |
| `G16FirstViewCorrectiveTests:159-166` | B: `snapshot.Membership.MutableMembers` y la forma del bucle de `Definitions` (la corrección C16-02) |
| `RackProjectionCommandGuardTests:118-128` | C: **un** `RackSiblingScan.CaptureDrawing(` en el lector, sin `GetPoint` ni `StartOpenCloseTransaction` |
| `G16OrientationGuardTests:15-60` | C: el orden clase → orientación → `RackProjectionSnapshotReader.Read(` → puntos (no toca el barrido) |
| `WorkspaceEditIntentGuardTests:190-365` | D: `Read` sin `LockDocument`/`StartTransaction` propios (`:190`); `AssertNoOpenTransaction` antes y después de `CaptureDrawing(` (`:207-268`); **un** `CaptureDrawing(` en el adaptador (`:323-349`); `CompletenessEvidence = "RackSiblingScan/CaptureDrawing"` y el orden de construcción (`:351-365`) |
| `LinkedPropertyViewPayloadTests:254-266`, `CustomPropertiesEnvelopeGuardTests:206`, `I60BatchCreationNamingTests:96-106` | A/B: la sentencia `new SelectiveInsertPort`, la composición del sobre y «no se nombra el rack». Son periféricas |

### 3.3 Pruebas de comportamiento (puras) sobre los consumidores de los hechos

Todas parten de `RackSiblingScanFact` **construidos a mano**. Fijan qué se hace **con** los hechos, nunca **cómo los produce** `Traverse`.

- `RackSiblingMembershipTests` (`tests/RackCad.Tests/RackSiblingRedrawTests.cs:9-79`): el superviviente exige una referencia directa de layout y la anidada no cuenta; los xref son ReadOnly; la atribución de la identidad.
- `RackSiblingRedrawPlanTests` (`:81-112`) y `RackSiblingRedrawRunTests` (`:114-…`, incluido `LockedLayerFailsBeforeMutate`, `:138`), con un puerto simulado.
- `RackSiblingInsertRunTests`. Ojo: `RackSiblingInsertRun` **no tiene llamador de producto** en `src/`, porque Selectivo encadena los pasos a mano en `RackSelectivoCommands.cs:147-206`.
- `RackSiblingCustomPropertiesGateTests`, `G16FirstViewCorrectiveTests` (las filas de `Classify` y de preflight), `RackEnvelopeIdProbeTests`, y `Workspace/WorkspaceDraftPilotIntentStaleBaseTests`/`WorkspaceDraftPilotIntentReadTests` (la `BuildStaleBase` del piloto).

### 3.4 Las pruebas que nombra el paquete y no tocan el barrido

- `SiblingRedrawSeamGuardTests`, `RackSiblingRedrawUnitsTests` y `DebugFaultInjectionGuardTests` (`tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs`): solo leen `Systems/Shared/SiblingRedraw*.cs` y los `*Commands*.cs`.
- `CallerOwnedFacadeGuardTests` y `CallerOwnedTransactionGuardTests`: las fachadas, `SystemBlockWriter` y `LateralHeaderDrawService`.
- `I55G14FoundationConsumerTests` (solo `Application/Views`), `I55G15Auth15ConsumerTests` (`RackDefinitionCreator`/`RackProjectionWriteScope`) e `I55CantileverPlantaVisibility…`/`I55DeferredProductFix…`. **Ninguna I55\* lee `RackSiblingScan.cs` ni el cableado del escaneo.**

### 3.5 La evidencia física

- **I-55 OV-RED** (el Owner, sobre el código actual de A) es histórica: un movimiento la invalida para el código nuevo.
- **Smoke-2** está propuesto y lo ejecuta T4C.

---

## 4. Lo que cubren los 7 archivos de T1B

**El contrato de T1B ya excluía el barrido.**
- `docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json` pone en `ForbiddenWriteScope` `RackSiblingScan.cs` (`:222`), `RackUnsupportedSiblingInsert.cs` (`:223`), `RackSelectivoInsertIntegration.cs` (`:208`) y `RackProjectionSnapshotReader.cs` (`:220`).
- Los mutantes del RED (`92f5f5db`) no tocaron ninguno de esos archivos. En el Plugin solo tocaron `RackPushBackCommands.cs`, `RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs`, `Views/RackViewPlacement.cs` y las 3 fachadas de Push Back.
- La clausura medida (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/t1b/closure-measured.json`, G1/G2/G3/G6) es **solo de `src/RackCad.UI`**.

**Los archivos que leen las guardas de T1B:**

- **`tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`**:
  - `RackPushBackCommands.cs`, con los cuerpos `public void RackPushBack()`, `internal static void DrawPushBackView(` e `internal static void EditPushBack(`;
  - `RackMenuCommands.cs`, solo el `case PushBackInsertionRequest pushBack:`;
  - `Views/RackViewBatchProducts.cs` y `Views/RackViewPlacement.cs`;
  - `Systems/PushBack/PushBack{System,Frontal,Planta}DrawService.cs`;
  - `KindHandlers/PushBackKindHandler.cs`.
- **`tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs`**:
  - solo `src/RackCad.UI`: `Systems/PushBack/**` (`.cs`/`.xaml`), `RackFrames/RackFrameConfiguratorWindow.xaml(.cs)`, `Views/RackViewBatchDialogPresenter.cs`, `UiSupport.cs` y `Editor/EditorModules.cs`;
  - además, un barrido de todo `src/**/*.cs`, pero solo para el token `InsertRequestedRaised +=` (`:251-264`).
- **Keyboard, Close, Intent, Owner y Resource**: ejecutan `RackPushBackSystemWindow` (WPF) y no leen ni cargan el Plugin.

**Ningún archivo de T1B nombra** `RackSiblingScan`, `Traverse`, `Capture`, `CaptureDrawing`, `TryAuthorize`, `authorized` ni `SelectiveInsertPort` (`grep`).

**Lo relevante, con precisión:**

- **CT-25** (`PluginGuardTests:234-270`), sobre el camino B de Push Back:
  - fija las lecturas de la ventana;
  - fija que `window.UpdateOnly` y `window.InsertionRequest.Views` aparecen;
  - fija el orden **posterior** al escaneo: `batchProducts.PrepareAll(` < `.RedrawInPlace(` < `SyncName(` < `EraseViewBlocks(` < `Editor.Regen();` < `RackViewBatchExecution.Run(`, con 3 `regen: false` y 1 `Regen()`.
  - **No fija** la llamada a `TryAuthorize` (`RackPushBackCommands.cs:193-195`), anterior a `PrepareAll`, ni `authorized.Blocks`/`authorized.AuthoredInput` (`:198`, `:250`), que son el producto del barrido.
- **CT-26** (`:274-311`): el menú de **rack nuevo** y la colocación. **No llega a `Traverse`.**
- **CT-24** (RACKPUSHBACK) y **CT-27** (las firmas de las fachadas y que los caminos clásicos no llaman a `PrepareRedraw`/`RedrawInTransaction`): ajenos al barrido. CT-27 sí importa para el escritor de T4B.
- **Indirecto (UI):**
  - CT-07 (`IntentTests:111-145`: Insertar → `InsertRequested`, `UpdateOnly == false`);
  - CT-06 (`CloseTests:578-640`: Actualizar → `UpdateOnly == true`).
  - Fijan el **selector de la ventana** que decide si `EditPushBack` entra en la rama del escaneo. No fijan el escaneo.
- **Nada** sobre A (Selectivo), C (RACKPROYECTAR), D (el lector del piloto) ni los B de Dinámico, Cantilever y Cabecera.

---

## 5. Conclusión sobre la condición (3)

**No se cumple.** T1B no protege el comportamiento relevante para el movimiento de `Traverse`:
- no lee `RackSiblingScan.cs` ni ningún archivo del cableado del escaneo;
- su contrato lo puso en el prohibido;
- sus mutantes no lo tocaron;
- su único punto de contacto es CT-25, que fija lo que `EditPushBack` hace **después** de `TryAuthorize`, y el selector `UpdateOnly` de la ventana.

Que «las pruebas de T1B siguen en verde» (INV-13, §6 `:504`) es necesario, pero **no prueba nada** sobre el movimiento. **Nada en T1B revela una diferencia de comportamiento** relevante: T1B no cambia producto (`src` es idéntico a `c3ae1c97`) y no observa el barrido.

**Lo que sí protege hoy, y es solo texto:**
- `WorkspaceEditIntentGuardTests:74-128`: las firmas, las cuentas antiduplicación y el orden Read/IsNullOrEmpty/Add;
- `SingleScanInsertGuardTests`, `RackViewBatchDriverGuardTests:37-50`, `G16FirstViewCorrectiveTests:159-166`, `RackProjectionCommandGuardTests:118-128` y las guardas del adaptador: que cada llamador llama una vez y reutiliza el escaneo;
- las pruebas puras (§3.3) protegen a los consumidores a partir de hechos sintéticos.

**Lo que no protege nada automatizado:**
- el cuerpo del recorrido, fuera de las anclas del §3.1;
- los cuerpos de `Capture`/`CaptureDrawing`;
- el orden bloqueo → transacción → recorrido → `Commit`.

Eso solo lo protegerían la guarda nueva de D-T4B-04 y Smoke-2.

**Por qué el `AssertInOrder` de D-T4B-04 es insuficiente como prueba de equivalencia:**
- comprueba una **subsecuencia** de anclas, así que no detecta sentencias insertadas, ni borradas si no son ancla, ni tokens cambiados (`directOnly: false`, `|| definition.IsAnonymous` eliminado, `envelope != null &&` eliminado);
- el helper existente busca la primera aparición en todo el archivo, y con un núcleo y una variante en el mismo archivo una aparición duplicada podría engañarlo;
- además deja fuera `:103-110` y `:163-169`.

### Protección mínima adicional que T4B puede exigir sin tocar las pruebas de T1B

Todo cabe en los archivos que ya permite el §6 (`TW/WorkspacePilotUpdateGuardTests.cs`, nuevo, y `TW/WorkspaceEditIntentGuardTests.cs`, modificado).

1. **Una guarda de equivalencia exacta del núcleo** (en lugar de `AssertInOrder`, o además de él):
   - el cuerpo del núcleo, normalizado (sin comentarios, con CRLF→LF y el espacio colapsado, como en T1B), es **idéntico** a un literal congelado de `c3ae1c97:RackSiblingScan.cs:103-161` y `:166-169`;
   - solo se admiten las sustituciones declaradas: `document.Database.BlockTableId` → `database.BlockTableId`, y la eliminación de los `using`/`Commit`;
   - lleva una autoprueba negativa (inyectar `directOnly: false` o quitar `|| definition.IsAnonymous` hace fallar).
2. **Una guarda exacta del envoltorio:**
   - el cuerpo de `Traverse` es exactamente `using (document.LockDocument())`, `using (var transaction = document.Database.TransactionManager.StartTransaction())`, una llamada `Núcleo(transaction, document.Database, selectedDefinition, eraseByKind)`, `transaction.Commit();` después de la llamada y dentro de los `using`, y el `return` del resultado;
   - sin `try`/`catch` en el archivo.
   - Se declara en el paquete que la firma del núcleo incluye `selectedDefinition` y `eraseByKind`.
3. **Congelar como texto exacto los cuerpos de `Capture` y `CaptureDrawing`**, que hoy solo tienen la firma fijada (`WorkspaceEditIntentGuardTests:93-96`): `Traverse(document, ObjectId.Null, null)`, y `Classify(scan.Facts, rackId, originalSourceId, originalSourceIdIsAttributable)` con su construcción.
4. **Un censo del núcleo:**
   - exactamente dos llamadas, desde `Traverse` y desde la variante caller-owned o `RackPilotUpdateWriter.cs`;
   - la variante pasa `ObjectId.Null, null`;
   - ningún otro archivo de `src/` lo nombra.
   - Se conservan las cuentas de `WorkspaceEditIntentGuardTests:74-128`, que ya impiden duplicar el recorrido.
5. **`RequiredTests` (§6 `:487`):** añadir una fila por clase para `SingleScanInsertGuardTests`, `RackViewBatchDriverGuardTests`, `G16FirstViewCorrectiveTests`, `RackSiblingCustomPropertiesGateTests` y `RackEnvelopeIdProbeTests`.
   - Hoy no figuran, aunque la CI completa los corra.
   - `WorkspaceEditIntentGuardTests` ya entra por el filtro `RackCad.Tests.Workspace`.
6. **Smoke-2 y la evidencia:**
   - declarar que el B de Dinámico, Cantilever y Cabecera queda cubierto por construcción con el de Push Back (el mismo `TryAuthorize`), o añadir uno;
   - dejar explícito que D («Comprobar ahora»/Begin) es una de «las tres lecturas»;
   - opcional: una evidencia `git diff --color-moved=zebra --color-moved-ws=allow-indentation-change c3ae1c97..<GREEN> -- src/RackCad.Plugin/Views/RackSiblingScan.cs` que muestre el bloque como movido.

Con 1 a 4, la condición (2) del Owner (una guarda que prueba la equivalencia) quedaría cubierta por texto exacto. La condición (3) se apoyaría entonces en las guardas existentes (§3.1 y §3.2) más las nuevas, y en Smoke-2 para lo físico. **No en T1B**, y así debe declararse.
