# Mapa 2 — Intención, puerto y clausura de `RackPushBackSystemWindow` (F4-T1B)

- Commit medido: `c3ae1c970c908ef541840fc583680b86376cae0b` (worktree de arquitectura). Solo lectura: no se compiló ni se ejecutó nada.
- Rutas relativas al repositorio. «Ventana» = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml(.cs)`; abreviada `W.cs` (code-behind) y `W.xaml`.
- Recuentos por `git ls-tree -r --name-only HEAD -- <ruta>`. Los usos, por `git grep` sobre `HEAD`. Las carpetas `bin/` y `obj/` del worktree no se contaron.

---

## 1. Cómo se abre la ventana y qué devuelve

### 1.1 Sitios de construcción (censo completo)

Búsqueda: `git grep -n "RackPushBackSystemWindow" HEAD -- src`. Fuera de la propia ventana, hay **4 construcciones en producción** y ninguna más (ni RACKDUPLICAR, ni RACKMIRROR, ni el panel Workspace, ni reflexión).

- **H-1 · RACKPUSHBACK / RPB (rack nuevo, comando directo).** `src/RackCad.Plugin/RackPushBackCommands.cs:47` hace `new RackPushBackSystemWindow(canInsertInAutoCad: true)`.
  - No asigna `Owner`. Abre con `AcApplication.ShowModalWindow(window)` (`:48`).
  - El dueño efectivo lo decide la API de AutoCAD; desde el código no se puede medir.
  - RPB delega en RACKPUSHBACK (`:32-33`).
- **H-2 · RACKEDITAR (editar un rack existente).** `src/RackCad.Plugin/RackPushBackCommands.cs:168` hace `new RackPushBackSystemWindow(canInsertInAutoCad: true)`.
  - Luego `window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)` (`:169`) y `AcApplication.ShowModalWindow(window)` (`:170`).
  - No asigna `Owner`.
  - Se llega por `RackMenuCommands.RackEditar` (`src/RackCad.Plugin/RackMenuCommands.cs:154-189`) → `KindHandlerDispatch` → `PushBackKindHandler.Edit` (`src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs:23-24`) → `EditPushBack`.
- **H-3 · Menú RACKCAD, rack nuevo.** `src/RackCad.UI/Editor/EditorModules.cs:114` hace `new RackPushBackSystemWindow(context.CanInsertInAutoCad) { Owner = context.Owner }` y luego `window.ShowDialog()` (`:115`).
  - `context.Owner` es el `RackMainMenuWindow` (`src/RackCad.UI/RackMainMenuWindow.xaml.cs:65-66`).
  - `CanInsertInAutoCad` llega `true` desde el Plugin (`src/RackCad.Plugin/RackMenuCommands.cs:31-33`).
  - El módulo **ignora** `context.DimensionStyles`: no llama a `SetDimensionStyles`, a diferencia de Dinámico (`EditorModules.cs:76`).
- **H-4 · Menú RACKCAD, abrir desde la biblioteca.** `src/RackCad.UI/Editor/EditorModules.cs:121`, con el mismo constructor y el mismo `Owner`.
  - Luego `window.LoadDesignForNew(project.PushBackDesign, entry.Name, project)` (`:124`) y `ShowDialog()` (`:125`).
- **H-5 · Constructores.**
  - `RackPushBackSystemWindow()` → `this(false)` (`W.cs:123-126`).
  - `RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)` (`W.cs:128`).
  - El constructor sin argumentos (`canInsertInAutoCad=false`) solo lo usan las pruebas; producción siempre pasa `true`.
  - `newIdFactory` solo lo inyectan las pruebas: los cuatro sitios de producción usan el GUID por defecto.
  - El constructor crea la sesión con `catalog: null`, así que carga el catálogo por `UiSupport.LoadCatalogSafe()` (`W.cs:135-136`, `src/RackCad.UI/Editor/RackEditorSession.cs:83-88`, `src/RackCad.UI/UiSupport.cs:84-94`).
  - El constructor termina en `LoadNew()` (`W.cs:152`).
- **H-6 · Atributos de la ventana relevantes para el llamador.**
  - `WindowStartupLocation="CenterOwner"` (`W.xaml:8`).
  - `FocusManager.FocusedElement` = `NameBox` (`W.xaml:9`).
  - `Style="{DynamicResource EditorShellWindowStyle}"` (`W.xaml:7`).
  - Diccionario local `/RackCad.UI;component/Themes/AppStyles.xaml` (`W.xaml:13`).
  - `CloseButton` con `IsCancel="True"` (`W.xaml:789`). No hay ningún `IsDefault` en la ventana (0 apariciones en `W.xaml`).

### 1.2 Qué lee cada llamador al volver

La ventana **nunca asigna `DialogResult`** (0 apariciones en `Systems/PushBack/*.cs`). Ningún llamador lee el valor de retorno de `ShowDialog`/`ShowModalWindow`. El único canal de vuelta es el contrato público derivado de la sesión.

- **H-7 · RACKPUSHBACK** (`RackPushBackCommands.cs:50-65`).
  - Lee solo `window.InsertionRequest is PushBackInsertionRequest request`. Si no lo es, retorna sin tocar el DWG (`:50-53`).
  - Si lo es: `RackUnitsGuard.WarnIfNotInches(document)` (`:56`) y luego `DrawPushBackView(request.View, request.Section, request.System, request.Design, request.RackId, request.RackName, source: null, innerSource: request.SourceProject)` (`:57-65`).
  - **No lee** `UpdateOnly` ni `InsertionRequest.Views`.
- **H-8 · RACKEDITAR / `EditPushBack`** (`RackPushBackCommands.cs:172-383`).
  - Lee `InsertRequested` (`:172`; si es false, retorna), `DesignToInsert` y `SystemToInsert` (`:178-179`; si alguno es null, avisa y no modifica nada, `:180-184`).
  - `RackId` solo como respaldo de `embed.Id` (`:187`). `RackName`, con respaldo de `embed.Name` (`:188`).
  - `UpdateOnly` (`:193`, `:198`, `:241`, `:248`, `:369`) e `InsertionRequest.Views` (`:254`).
  - **No lee** `InsertView`, `InsertSection` ni `SourceProjectToInsert`: el proyecto interno es el deserializado del sobre (`:154`).
- **H-9 · Módulo del menú** (`EditorModules.cs:130-131`). Hace `window.InsertRequested ? window.InsertionRequest : null` y devuelve la petición tal cual.
  - El menú la guarda y se cierra (`RackMainMenuWindow.xaml.cs:155-160` y `:207-212`). Si es `null`, el menú sigue abierto.
- **H-10 · Plugin tras el menú** (`RackMenuCommands.cs:50-53` y `:131-143`).
  - Llama a `WarnIfNotInches`.
  - En el `case PushBackInsertionRequest pushBack`: `RackNewRackName.Resolve(...)`, luego `pushBack.System.Name = pushBackName`.
  - Después `RackViewBatchProducts.PushBack(document, pushBack.System, pushBack.Design, pushBack.RackId, pushBackName, null, pushBack.SourceProject, null)` y `RackViewBatchExecution.Run(..., Request(RackProductSourceKind.NewRack, pushBack.Kind, pushBack.RackId, pushBack.Views))`.
- **H-11 · Miembros públicos que ningún llamador de producción lee:** `InsertView`, `InsertSection` y `SourceProjectToInsert` (`W.cs:251-253`).
  - Búsqueda: `git grep "window\.(InsertView|InsertSection|SourceProjectToInsert)"` en `src/RackCad.Plugin` y `src/RackCad.UI/Editor`; solo aparecen otros sistemas.
  - El evento `RackEditorSession.InsertRequestedRaised` (`RackEditorSession.cs:124`) **no tiene suscriptores** en `src/`.
- **H-12 · Escritura del llamador sobre la salida de la ventana, después del cierre.** Los tres caminos mutan `System.Name` del objeto que entregó la ventana:
  - menú: `RackMenuCommands.cs:135`;
  - RACKPUSHBACK: `RackPushBackCommands.cs:98`;
  - RACKEDITAR: `RackPushBackCommands.cs:190`.

---

## 2. Intención: cómo la ventana expresa Actualizar, Insertar y las demás salidas

- **H-13 · No hay puerto, evento ni delegado de intención inyectado.** La intención se registra en la `RackEditorSession` propia de la ventana y luego la ventana se cierra con `Close()`.
  - El contrato público es una proyección de la sesión (`W.cs:247-256`):
    - `InsertRequested`;
    - `UpdateOnly`;
    - `RackId` / `RackName` (identidad);
    - `InsertView` / `InsertSection`;
    - `InsertionRequest`;
    - `SystemToInsert` / `DesignToInsert` (los dos, sacados del `PushBackInsertionRequest`);
    - `SourceProjectToInsert` (el `sourceProject` de la ventana).
- **H-14 · Semántica de la sesión** (`RackEditorSession.cs:138-195`).
  - `RequestInsert` y `RequestUpdate` pasan por `Complete`:
    1. `Identity.EnsureId()`;
    2. fijan `UpdateOnly`, `InsertView` (null si es update) e `InsertSection` (-1 si es update);
    3. construyen `InsertionRequest`;
    4. `InsertViews = InsertionRequest.Views`;
    5. `InsertRequested = true`;
    6. disparan el evento.
  - `RequestInsertViews` (`:145-170`) valida antes de acuñar la identidad, llama a `request.SetViews(ordered)` y deja `UpdateOnly=false`, `InsertView=null`, `InsertSection=-1`.
  - `PushBackInsertionRequest` deriva `Views` del par heredado (view, section) por `SetLegacySingleView` (`src/RackCad.UI/Editor/RackInsertionRequest.cs:178-197`, `:52-57`). Si el código no decodifica el par, `Views` queda vacío.
- **H-15 · Tabla de salidas posibles y su señal exacta.**

| # | Gesto (handler) | Guardas, en orden | Señal en la sesión | ¿Cierra? |
|---|---|---|---|---|
| S1 | `Insertar vista actual` → `Insert_Click` (`W.cs:3205-3209`) → `RequestDraw(view, section, false)` | `!canInsertInAutoCad` → estado y nada (`:3325-3329`). Si hay módulos pendientes, se confirman (`:3346-3349`). `RequestRecompute()` síncrono (`:3351`). Si es inválido → estado (`:3352-3356`). Si está bloqueado → estado (`:3360-3363`) | `SetName(NameBox.Text?.Trim())` (`:3365`), `SetModel` (`:3366`), `RequestInsert(view, section, …)` (`:3374-3375`): `InsertRequested=true`, `UpdateOnly=false`, `InsertView` = vista elegida, `Views` = 1 dirección (o 0 si es un lateral sin corte, sección -1) | Sí: `Close()` (`:3378`) |
| S2 | `Insertar lateral` / `Frontal entrada/salida` / `Frontal posterior` / `Planta` → `InsertViewAt(0..3)` (`W.cs:3214`, `:3226-3230`, `:3251-3262`) | Fijan `ViewBox.SelectedIndex` y luego todo igual que S1. La sección frontal codifica extremo y lado `frontalSide` (`:3084-3097`) | Como S1 | Sí |
| S3 | `Actualizar` → `Update_Click` (`W.cs:3292`) → `RequestDraw(null, -1, true)` | Las mismas de S1, más `updateOnly && !isEditingExisting` → estado (`:3331-3335`) | `RequestUpdate` (`:3369-3370`): `InsertRequested=true`, `UpdateOnly=true`, `InsertView=null`, `InsertSection=-1`, `Views` vacío | Sí (`:3378`) |
| S4 | `Insertar varias…` → `InsertBatch_Click` (`W.cs:3294-3321`) | Confirma módulos (`:3296`) y recalcula (`:3297`). Si es inválido o está bloqueado, retorna (`:3298-3299`). Diálogo de lote (`:3311`); si se cancela, retorna. **No comprueba `canInsertInAutoCad`** | `SetName` (`:3312`), `SetModel` (`:3313`), `RequestInsertViews(views, …)` (`:3314-3319`): `Views` = N direcciones ordenadas; `View`/`Section` de la petición = los de `Views[0]` | Sí (`:3320`) |
| S5 | `Cerrar`, Escape (`IsCancel`), botón de sistema, Alt+F4 → `Close_Click` (`W.cs:3381`) / `OnClosing` (`:3395-3404`) | Si `!InsertRequested` y hay módulos pendientes: `EditorClosePolicy.MayClose` pregunta (`src/RackCad.UI/Shell/EditorClosePolicy.cs:108-113`). Si la respuesta es «no», `e.Cancel = true` | Ninguna: `InsertionRequest == null` y `InsertRequested == false` | Sí, o se cancela |
| S6 | `Guardar en biblioteca` → `SaveLibrary_Click` (`W.cs:3441-3476`) | Recalcula, valida y comprueba el bloqueo (`:3443-3456`). `PromptSaveToLibrary` (`:3458`); si se cancela, retorna | **No marca inserción.** Sí muta la identidad: `EnsureId()` y `SetName` (`:3463-3464`) | No |
| S7 | `BOM` → `Bom_Click` (`W.cs:3423-3439`) | Recalcula, valida y comprueba el bloqueo | Ninguna | No |

- **H-16 · Habilitación de las salidas** (`UpdateButtons`, `W.cs:3523-3565`).
  - `InsertButton` = `canInsertInAutoCad && canOutput` (`:3526`).
  - `UpdateButton` = lo mismo, más `isEditingExisting` (`:3527`).
  - Los cuatro botones por vista copian `InsertButton` (`:3549-3553`).
  - `BomButton` = `canOutput` (`:3528`).
  - `SaveLibraryButton` = `currentInputsAreValid` (`:3529`): **no** mira `outputIsBlocked`, aunque el click sí lo comprueba de nuevo.
  - **`InsertBatchButton` no aparece en `UpdateButtons`.** Búsqueda: `grep InsertBatchButton src/RackCad.UI/Systems/PushBack/*.cs` da 0 resultados; solo existe en `W.xaml:782-783`. Siempre está habilitado.
- **H-17 · Modo de edición.**
  - `LoadNew` (`W.cs:262-281`): `isEditingExisting=false`, `Adopt(null,null)` y siembra la seguridad por defecto.
  - `LoadDesignForNew` (`:286-299`): `isEditingExisting=false`, `Adopt(null, rackName)`; el GUID se acuña al insertar.
  - `LoadExisting` (`:303-316`): `isEditingExisting=true`, `Adopt(rackId, rackName)`.
- **H-18 · El nombre entra a la identidad en cada edición de `NameBox`** (`Name_Changed`, `W.cs:1201-1205`), no solo al insertar.
- **H-19 · `RequestRecompute` es síncrono fuera de un `Defer`** (`W.cs:594`; `src/RackCad.UI/Editor/RecomputeGate.cs:5-13` y `:31-35`). `Recompute` publica el modelo en la sesión en cada recálculo válido (`W.cs:660`).

---

## 3. El puerto y sus escrituras

### 3.1 Lo que la ventana escribe ella misma

- **H-20 · La ventana no recibe ningún puerto, host ni servicio de escritura inyectable.**
  - Los únicos parámetros de construcción son `canInsertInAutoCad` y `newIdFactory` (H-5).
  - Lo inyectable son costuras internas de prueba, todas de diálogo (§5): ninguna escribe.
  - Mutable estático: solo `EditorDiscardPrompt.confirm` (`src/RackCad.UI/Shell/EditorClosePolicy.cs:51`), sustituible con `Substitute` (`:57-71`).
  - Ni la ventana, ni `Application/Systems/PushBack`, ni Shell, Editor o Controls registran (`RackLog`, `Logger`, `Trace`): 0 apariciones.
- **H-21 · Escritura en memoria de la intención** (S1-S4). La disparan los botones de inserción y Actualizar, dentro del click y **antes** del `Close()`. Datos:
  - `PushBackInsertionRequest(System, Design, Id, Name, View, Section, sourceProject)` (`W.cs:3317-3318`, `:3369-3375`);
  - `Design` y `System` son los de `lastComputation`, la última computación válida, recalculada en el mismo click.
  - No toca disco ni DWG. El DWG lo escribe el llamador **después** del cierre (§3.2).
- **H-22 · Escritura a disco: «Guardar en biblioteca»** (S6), **dentro** de la ventana, que sigue abierta. Secuencia:
  1. `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` (`W.cs:3458`):
     - lee `UserSettingsStore.Load()`, es decir `%APPDATA%\RackCad\settings.json` (`src/RackCad.UI/UiSupport.cs:66-67`; `src/RackCad.Application/Settings/UserSettings.cs:61-62` y `:77`);
     - **crea la carpeta de la biblioteca** con `Directory.CreateDirectory(libraryFolder)`, en modo best-effort y **antes** del diálogo, aunque luego se cancele (`UiSupport.cs:68`; la carpeta por defecto es `%APPDATA%\RackCad\Designs`, `UserSettings.cs:65-75`);
     - muestra un `SaveFileDialog` con `ShowDialog(owner)` (`UiSupport.cs:74-81`).
  2. Si se acepta: `session.Identity.EnsureId()` y `SetName(...)` (`W.cs:3463-3464`). El GUID queda acuñado en la sesión, y la próxima inserción lo reutiliza.
  3. `new RackProjectStore().Save(RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject), path)` (`W.cs:3467-3468`). Por dentro es `AtomicFile.WriteAllText` (`src/RackCad.Application/Persistence/RackProjectStore.cs:246-251`).
  4. Mensaje de estado; una excepción se convierte en un estado de error (`W.cs:3469-3474`).
  - Lateral: con un `settings.json` corrupto, `UserSettingsStore.Load` lo pone en cuarentena como `.bad` y lo registra (`CorruptFile.Quarantine`, `UserSettings.cs:108-113`; contrato en `:85`). Es una escritura que provoca una lectura.
  - No hay costura: `RackProjectStore`, `UserSettingsStore` y `SaveFileDialog` se crean o llaman directamente.
- **H-23 · Escrituras transitivas a través de subdiálogos** (las dispara el usuario dentro de la ventana, **antes** de cerrarla):
  - **`RackFrameConfiguratorWindow`** (desde «Configurar cabecera…», `W.cs:2424-2446`): en `OnClosed` llama **siempre** a `SaveCurrentLayout()` (`src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs:602-604`). Eso hace `RackFrameConfiguratorLayoutStore.Save`, que crea `%LOCALAPPDATA%\RackCad` y escribe `RackFrameConfigurator.layout` (`src/RackCad.UI/RackFrames/RackFrameConfiguratorLayoutStore.cs:25-29` y `:74-81`). Es una escritura de preferencias **en cada apertura y cierre** del configurador.
  - En el configurador, «Guardar proyecto» → `PromptSaveToLibrary` y `ViewModel.SaveProjectTo(path)` (`RackFrameConfiguratorWindow.xaml.cs:385-392`).
  - El configurador abierto desde Push Back usa el constructor de un argumento (`canInsertInAutoCad=false`, `:64-67`), así que su `RequestDraw` se queda en un `MessageBox` (`:299-305`).
  - **`RackBomWindow`** (desde `BOM`): exportar a xlsx o csv → `File.WriteAllBytes` / `File.WriteAllText` tras un `SaveFileDialog` (`src/RackCad.UI/RackBomWindow.xaml.cs:39-59` y `:65-85`).
- **H-24 · Mutación del estado del editor por los subdiálogos de seguridad.**
  - Ocurre **dentro** de la ventana: solo en memoria y solo si se acepta.
  - Ejemplo, `Safety_Click`: si se cancela, no cambia nada (`W.cs:2615-2621`); si se acepta, `PushBackSafetyAuthority.Authorize` + secciones de defensa, botas y tope bajo `Recompute.Defer()` (`:2626-2655`).
  - No es una escritura del puerto. Se lista para distinguirla.

### 3.2 Lo que escribe el llamador después del cierre (DWG)

- **H-25 · RACKPUSHBACK → `DrawPushBackView`** (`RackPushBackCommands.cs:75-138`), después de `ShowModalWindow`.
  - Lecturas: `RackNewRackName.Resolve` lee los nombres del DWG (`:95`).
  - Escrituras:
    - `PushBackPlantaDrawService.DrawAndPlace` (`:111`);
    - `PushBackFrontalDrawService.DrawAndPlace` (`:124`, tras validar la sección, `:117-121`);
    - `PushBackSystemDrawService.DrawAndPlace` (`:129`).
  - Un lateral con `section < 0` pide el poste en la línea de órdenes (`GetInteger`, `:417`) y luego `DrawAndPlace` (`:432`).
  - Una vista desconocida → `WriteMessage` sin dibujar (`:133-134`).
  - El sobre se construye con `BuildPushBackPayload` (`:439-461`), que es pura serialización (`RackProjectStore.Serialize` + `RackEmbedComposer` + `RackEmbedStore`).
- **H-26 · RACKEDITAR → `EditPushBack`** (`RackPushBackCommands.cs:147-384`), después de `ShowModalWindow`:
  - Si `!UpdateOnly`: `RackUnsupportedSiblingInsert.TryAuthorize(...)` (`:195`) da `authorized.Blocks`. Si `UpdateOnly`: `FindRackBlocks` (`:198`).
  - Comprobaciones previas, sin escribir: sobre (kind y GUID, `:208-217`), proyecto interno I-11 (`:221-226`) y descriptor (`:230-237`).
  - Si `!UpdateOnly`: `WarnIfNotInches` (`:241-244`) y preparación del lote ID18 con `window.InsertionRequest.Views` (`:248-260`).
  - **Escrituras:** `RedrawInPlace(..., regen:false)` de los tres servicios por cada vista ligada (`:279`, `:297`, `:336`), `RackBlockRenamer.SyncName` (`:282`, `:300`, `:339`), borrado de cortes obsoletos `EraseViewBlocks` (`:351`) y `Editor.Regen()` único (`:366`).
  - Si `!UpdateOnly`: `RackViewBatchExecution.Run(... _ => Applied())` inserta las vistas nuevas (`:371-373`).
  - Las vistas existentes se redibujan **también** en Insertar desde RACKEDITAR, no solo en Actualizar.
- **H-27 · Menú → `RackViewBatchProducts.PushBack` + `RackViewBatchExecution.Run`** (H-10).
  - La colocación pasa por `RackViewPlacement.PlacePushBack(document, product, regen:false)` (`src/RackCad.Plugin/Views/RackViewBatchProducts.cs:74-94`; `src/RackCad.Plugin/Views/RackViewPlacement.cs:36-39`).
  - Este camino **no** usa los tres servicios de dibujo de Push Back; usa `PlaceHeader` con `KindPushBack`.
- **H-28 · Asimetría medida en el código:** el lote S4 se consume de forma distinta según el llamador.
  - El menú usa `pushBack.Views` (todas las vistas) y RACKEDITAR usa `InsertionRequest.Views`.
  - RACKPUSHBACK solo usa `request.View/Section` (`RackPushBackCommands.cs:57-59`), que valen `Views[0]` (`W.cs:3316-3318`). Por tanto, «Insertar varias…» desde RACKPUSHBACK dibujaría **solo la primera vista**.
  - No se ejecutó: es una inferencia del código.
  - La guarda existente `MenuAndDirectCommand_ConvergeOnTheSameDrawPath` (`tests/RackCad.Tests/PushBackRoundTripSourceGuardTests.cs:377-390`) fija que RACKPUSHBACK termina en `DrawPushBackView`.

### 3.3 Fachadas de escritura de Push Back del Plugin (lleguen o no desde la ventana)

- **H-29 · Las tres fachadas propias de Push Back.** Son «las 3 de Push Back» de H-04 del paquete F4 (`docs/initiatives/I-64-f4-gate-contract-package.md:23`), todas en `src/RackCad.Plugin/Systems/PushBack/` y sobre `ViewBlockDraw`.

| Fachada (archivo) | Públicos (wrapper con transacción propia) | Internos caller-owned | Nombre |
|---|---|---|---|
| `PushBackSystemDrawService` (`PushBackSystemDrawService.cs`, 76 líneas) | `DrawAndPlace(Document, PushBackSystem, string payloadJson=null, string rackName=null, int postIndex=-1)` `:25`; `RedrawInPlace(Document, ObjectId, PushBackSystem, string payloadJson, bool regen=true, int postIndex=-1)` `:41` | `PrepareRedraw(Database, ObjectId, PushBackSystem, string, int postIndex=-1)` `:58`; `RedrawInTransaction(Database, Transaction, PreparedViewRedraw, out …)` `:64` | `internal static BlockName` `:73` |
| `PushBackFrontalDrawService` (`PushBackFrontalDrawService.cs`, 80 líneas) | `DrawAndPlace(Document, PushBackSystem, PushBackFrontalEnd, string payloadJson=null, string rackName=null, PushBackSide side=A)` `:22`; `RedrawInPlace(…, bool regen=true, PushBackSide side=A)` `:38` | `PrepareRedraw` `:56`; `RedrawInTransaction` `:62` | `BlockName` `:70` |
| `PushBackPlantaDrawService` (`PushBackPlantaDrawService.cs`, 63 líneas) | `DrawAndPlace(Document, PushBackSystem, string payloadJson=null, string rackName=null)` `:21`; `RedrawInPlace(Document, ObjectId, PushBackSystem, string, bool regen=true)` `:35` | `PrepareRedraw` `:51`; `RedrawInTransaction` `:56` | `BlockName` `:60` |

- **H-30 · Los escritores caller-owned de Push Back no tienen ningún llamador en producción.**
  - Búsqueda: `git grep "\.PrepareRedraw(\|\.RedrawInTransaction("`. Los únicos llamadores son de Selectivo (`RackSelectivoInsertIntegration.cs:220-249`, `ProjectVariableMutationExecutor.cs:237-256`) y de `ViewBlockDraw` dentro de las fachadas.
  - Coincide con H-01/R-09 del paquete F4.
  - `CallerOwnedFacadeGuardTests` solo cubre 4 fachadas: `ViewBlockDraw`, `SelectiveFrontal`, `SelectivePlanta` y `LateralHeader` (`tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:48-55`). Las tres de Push Back no están (H-04).
- **H-31 · Puntos de entrada de escritura propios de Push Back** fuera de `Systems/PushBack/`:
  - `RackPushBackCommands.RackPushBack` (`:37`, público, comando);
  - `DrawPushBackView` (`:75`, interno estático);
  - `EditPushBack` (`:147`, interno estático);
  - `InsertPushBackLateralSection` (`:387`, privado);
  - `BuildPushBackPayload` (`:439`, interno estático y puro).
  - `PushBackKindHandler`: `Edit` reenvía a `EditPushBack` (`:23-24`). `BuildBom` (`:31-51`) y `OutputBlockedReason` (`:64-65`) solo leen. `RestampDesign` devuelve el JSON intacto (`:67`).
  - En archivos compartidos, la parte específica de Push Back es `RackViewBatchProducts.PushBack` (`Views/RackViewBatchProducts.cs:74-94`), `RackViewPlacement.PlacePushBack` (`Views/RackViewPlacement.cs:36-39`) y el `case PushBackInsertionRequest` (`RackMenuCommands.cs:131-143`).
  - `RackPilotIntentAdapter` (`Views/RackPilotIntentAdapter.cs`) es lectura del piloto: no escribe.

---

## 4. Clausura de dependencias, medida

Definición: `docs/initiatives/I-64-f4-t2-gate-contract-package.md:58-66`, con la salvedad de `Pilot/` (`docs/initiatives/I-64-f4-t4-gate-contract-package.md:185-205`). Todas las rutas declaradas **existen con su nombre**.

| Grupo | Ruta | Archivos | Uso directo medido desde `Systems/PushBack/**` (tipos nombrados) |
|---|---|---|---|
| G1 | `src/RackCad.UI/Systems/PushBack/**` | **10** | La propia ventana (`.xaml`, `.xaml.cs`, `.Composite.cs`) y 7 auxiliares: `PushBackBootSection`, `PushBackDefenseSection`, `PushBackMatrixCardModel`, `PushBackPreviewModel`, `PushBackPreviewRenderer`, `PushBackRearTopeDialogAdapter`, `PushBackRearTopeSection` |
| G2a | `src/RackCad.UI/Controls/**` | **17** | `CatalogCombo`, `NumericField`, `PreviewCanvas` (3). Los otros 14 no se nombran directamente; puede haber uso transitivo, p. ej. `NumericField` → `NumericFieldValidation` |
| G2b | `src/RackCad.UI/Shell/**` | **9** | `RackEditorVisualShell` (`W.xaml:37`) y `EditorClosePolicy.cs` (`EditorClosePolicy`, `EditorPendingWork`, `EditorDiscardPrompt`) |
| G2c | `src/RackCad.UI/Themes/**` | **2** | `AppStyles.xaml` (fusionado en `W.xaml:13`) y `Generic.xaml` (plantilla de `RackEditorVisualShell` vía `ThemeInfo`, `src/RackCad.UI/RackCad.UI.csproj:19-26`) |
| G2d | `src/RackCad.UI/Editor/**` | **12** | `RackEditorSession`, `RackInsertionRequest.cs` (`PushBackInsertionRequest`), y a través de la sesión `RackEditorIdentity` y `RecomputeGate`. **`EditorModules.cs` (llamador, H-3/H-4) está en esta carpeta** |
| G2e | `src/RackCad.UI/Preview/**` | **3** | `EditorPreviewParts`, `EditorPreviewSurface` (2) |
| G3 | Subdiálogos (directos) | **8 clases en 11 archivos** (+2 de framework) | Ver H-32 |
| G4 | `src/RackCad.Application/Systems/PushBack/**` sin `Pilot/` | **52** (60 en total; 8 en `Pilot/`) | **Coincide con F4-E51 (52).** Todo es plano: no hay más subcarpetas que `Pilot/` |
| G5 | Fachadas de escritura de Push Back del Plugin | **3** núcleo (`Systems/PushBack/*.cs`) + **2** entradas propias (`RackPushBackCommands.cs`, `KindHandlers/PushBackKindHandler.cs`) | Más los fragmentos de Push Back en 3 archivos compartidos (H-31) |

- **H-32 · Subdiálogos que abre la ventana**: clase, archivo y sitio de apertura.

| Subdiálogo | Archivo | Abre en | `Owner` | Costura existente |
|---|---|---|---|---|
| `RackFrameConfiguratorWindow` | `src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml` + `.xaml.cs` | `W.cs:2426` (desde `ConfigureModuleHeader_Click`, `:1897`) | `this` | `HeaderConfiguratorDialog` (sustituye todo, `:1525`, `:1938-1940`) y `HeaderConfiguratorPresenter` (solo el `ShowDialog`, `:2410`, `:2437-2444`) |
| `SelectiveSafetyWindow` | `src/RackCad.UI/SelectiveSafetyWindow.cs` | `W.cs:2997-3018` (desde `Safety_Click`) | `this` (`:3016`) | `SafetyDialog` (todo, `:1493`, `:2615-2617`) y `SafetyWindowDialog` (solo el `ShowDialog`, `:3028`, `:3018`) |
| `SafetyPerPostWindow` (botas por lado) | `src/RackCad.UI/SelectiveSafetyWindow.cs:1028` | `W.cs:2716-2722`, invocada desde la sección de botas **alojada dentro** de `SelectiveSafetyWindow` (`:2680`) | `this` = la ventana Push Back, no el diálogo de seguridad | `BootPerPostWindowDialog` (solo el `ShowDialog`, `:2700`) |
| `SafetyDefensaGridWindow` | `src/RackCad.UI/SafetyDefensaGridWindow.cs` | `W.cs:2857-2867`, desde la sección de defensa (`:2818`), también alojada en el diálogo de seguridad | `this` (Push Back) | `DefenseDialog` (todo, `:1505`, `:2849`) |
| `SafetyTopeGridWindow` | `src/RackCad.UI/SafetyTopeGridWindow.cs` | `W.cs:3031-3049`, desde la sección del tope trasero (`:2594`, `:2605`), también alojada en el diálogo de seguridad | `this` (Push Back) | `RearTopeDialog` (todo, `:1499`, `:2981`) |
| `RackViewBatchDialog` (vía `RackViewBatchDialogPresenter.TryShow`) | `src/RackCad.UI/Views/RackViewBatchDialog.xaml` + `.xaml.cs`, `src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs` | `W.cs:3311` → `RackViewBatchDialogPresenter.cs:52-63` | `owner` = `this` (`:59`) | **Ninguna** (estático) |
| `RackBomWindow` | `src/RackCad.UI/RackBomWindow.xaml` + `.xaml.cs` | `W.cs:3438` | `this` | **Ninguna** |
| `SaveFileDialog` (Win32, framework) | vía `src/RackCad.UI/UiSupport.cs:64-82` | `W.cs:3458` | `ShowDialog(owner=this)` | **Ninguna** |
| `MessageBox` de descarte (framework) | vía `src/RackCad.UI/Shell/EditorClosePolicy.cs:76-80` | `OnClosing` (`W.cs:3397`) → `MayClose` → `Confirm(question)` **sin owner** (`EditorClosePolicy.cs:112`) | `null` (`MessageBox.Show` sin dueño) | `EditorDiscardPrompt.Substitute` |

- **H-33 · Subdiálogos transitivos que se alcanzan desde los subdiálogos directos.**
  - Desde `SelectiveSafetyWindow`, cada uno con `Owner = this` (= el diálogo de seguridad): `SafetyPerPostWindow` (`SelectiveSafetyWindow.cs:585-588`), `SafetyTopeGridWindow` (`:602-603`), `SafetyDefensaGridWindow` (`:695-698`), `SafetyGuiaEntradaGridWindow` (`src/RackCad.UI/SafetyGuiaEntradaGridWindow.cs`, `:711-714`), `SafetyDesviadorGridWindow` (`src/RackCad.UI/SafetyDesviadorGridWindow.cs`, `:736-751`) y `SafetyParrillaGridWindow` (`src/RackCad.UI/SafetyParrillaGridWindow.cs`, `:768-769`).
    - Push Back quita del diálogo GUIA, PARRILLA, TOPE y DEFENSA (`W.cs:220-232`) y lo abre con `includeDefensa:false, includeGuia:false` (`W.cs:3002`). Por tanto, de esas filas solo se esperan alcanzables la de por poste y la del desviador. No se midió fila por fila.
  - Desde `RackFrameConfiguratorWindow`: `RackBomWindow` (`:269-270`), `MessageBox`, `SaveFileDialog` y `OpenFileDialog` (`:387`, `:401-406`).
  - La carpeta `src/RackCad.UI/RackFrames/` tiene 10 archivos; el configurador depende de su ViewModel y de su `LayoutStore`.
- **H-34 · Dependencias directas de la ventana que no están en ninguna carpeta de la clausura:**
  - `src/RackCad.UI/UiSupport.cs` (11 apariciones: `SetStatus`, `FrozenBrush`, `PromptSaveToLibrary`, `LoadCatalogSafe`);
  - `src/RackCad.UI/CatalogOption.cs`;
  - `src/RackCad.UI/SafetyDefenseFace.cs`;
  - los subdiálogos de H-32;
  - `src/RackCad.UI/Views/**` (3 archivos).
  - `RackDynamicSystemWindow` solo aparece en un `<see cref>` (`W.cs:38`): no es dependencia de ejecución.
  - Los tipos de dominio (`SafetySide`, `SafetyPostDefense`, `SelectiveSafetySelection`) están en `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs`, fuera de la clausura declarada.
- **Totales por grupo:** G1 = 10; G2 = 17 + 9 + 2 + 12 + 3 = **43**; G3 = 11 archivos de producto (8 clases) más 2 del framework; G4 = **52**; G5 = 3 + 2.
  - Clausura declarada sin contar los transitivos: **10 + 43 + 11 + 52 + 5 = 121 archivos**.

---

## 5. Costuras para probar sin AutoCAD

- **H-35 · La ventana se construye sin AutoCAD en `tests/RackCad.UI.Tests`.** Ya se hace en 52 archivos de prueba: `git grep -l RackPushBackSystemWindow HEAD -- tests/RackCad.UI.Tests`. Hace falta:
  - **hilo STA**: `StaTestRunner.Run` (`tests/RackCad.UI.Tests/StaTestRunner.cs:78-110`);
  - **catálogos** junto a los binarios: el proyecto copia `assets/catalogs/*.json;*.csv` a `catalogs\` (`tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:30-35`). El catálogo **no es inyectable**: lo carga `UiSupport.LoadCatalogSafe()`, y si falta, el catálogo queda vacío;
  - **`InternalsVisibleTo("RackCad.UI.Tests")`** (`src/RackCad.UI/RackCad.UI.csproj:13-16`), que da acceso a las costuras internas: `Session`, `State`, `LastComputation`, `CurrentInputsAreValid`, `OutputIsBlockedForTest`, `BuildLibraryProjectForTest`, `PendingWork()`, `CompositeState`, `FrontalSideForTest` y los delegados de diálogo;
  - un **`newIdFactory`** determinista, si se quiere fijar el GUID.
  - `RackCad.UI` no referencia AutoCAD (ADR-0006). Ya existe la guarda `RackCadUi_ReferencesNoAutoCadAssembly` (`tests/RackCad.UI.Tests/PushBackEditorWindowTests.cs`, sección 19).
- **H-36 · La intención es observable sin dobles.**
  - Se hace click con `EditorWindowTestSupport.ClickNamed(w, "InsertButton")` (`tests/RackCad.UI.Tests/EditorWindowTestSupport.cs:22`), o con `RaiseEvent(ButtonBase.ClickEvent)`.
  - Después se leen el contrato público y `InsertionRequest` (por ejemplo `PushBackEditorWindowTests.cs:520-588`).
  - `Close()` sobre una ventana nunca mostrada no rompe las pruebas existentes.
- **H-37 · Los subdiálogos solo son observables con la ventana mostrada.**
  - `{ Owner = this }` lanza si `this` nunca se mostró. Las pruebas actuales lo usan como señal: «abrir el diálogo lanza» (`tests/RackCad.UI.Tests/PushBackLibrarySaveGateTests.cs:89-95`).
  - Para fijar `Owner` y `CenterOwner` hay que mostrar la ventana fuera de pantalla (patrón `Shown()`: `WindowStartupLocation.Manual`, `Left/Top = -10000`, `Show()`, en `tests/RackCad.UI.Tests/PushBackA1EditorTests.cs:27-38`).
  - Además hace falta una costura que sustituya **solo** el `ShowDialog`: `SafetyWindowDialog`, `BootPerPostWindowDialog` y `HeaderConfiguratorPresenter` reciben la ventana real con `Owner` ya asignado.
  - `RearTopeDialog`, `DefenseDialog`, `SafetyDialog` y `HeaderConfiguratorDialog` sustituyen el diálogo **entero**: no se construye ninguna ventana y el `Owner` no es observable.
  - BOM, el lote y `SaveFileDialog` **no tienen costura**. Ninguna prueba usa `EventManager.RegisterClassHandler` ni intercepta modales (0 resultados).
- **H-38 · Las escrituras a disco de la ventana (H-22/H-23) no tienen doble.**
  - `RackProjectStore`, `UserSettingsStore`, `Directory.CreateDirectory`, `RackFrameConfiguratorLayoutStore` y `File.Write*` se crean o llaman directamente.
  - Consecuencia: «Guardar en biblioteca» no se puede caracterizar de punta a punta en UI.Tests sin escribir en el perfil real.
  - Hasta el intento que «lanza en el diálogo» ya ejecuta `UserSettingsStore.Load()` y `Directory.CreateDirectory(%APPDATA%\RackCad\Designs)` en la máquina de pruebas, antes del `SaveFileDialog`.
  - Lo caracterizable en UI es **el contenido** que se escribiría: `BuildLibraryProjectForTest()` (`W.cs:240-243`). Coincide con `SaveLibrary_Click` (`:3467`), pero es una copia de la expresión, no la misma llamada.
- **H-39 · Las escrituras del DWG (§3.2) viven en el Plugin**, que referencia AutoCAD. `RackCad.Tests` no carga el Plugin (ADR-0003). Para ellas solo cabe una **guarda de código fuente**, con el patrón de `PushBackPluginSourceGuardTests` y `PushBackRoundTripSourceGuardTests`:
  - localizar la raíz por `RackCad.sln`;
  - `File.ReadAllText` sobre `src/RackCad.Plugin/...`;
  - `Assert.Contains`/`DoesNotContain` de literales exactos;
  - orden por `IndexOf` (por ejemplo, la comprobación previa antes del primer `RedrawInPlace`);
  - aislar el cuerpo de un método entre dos `IndexOf` (`tests/RackCad.Tests/PushBackPluginSourceGuardTests.cs:20-41` y `:53-67`; `tests/RackCad.Tests/PushBackRoundTripSourceGuardTests.cs:23-52`, `:198-208` y `:377-390`);
  - `CallerOwnedFacadeGuardTests.Body(source, marker)` extrae el cuerpo de un miembro (`tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:57-80`).
- **H-40 · Cobertura previa de la intención** (para no duplicar ni contradecir):
  - S1 lateral: `Insert_Lateral_ProducesTheTypedPayload`;
  - S3: `Update_KeepsGuidAndSource_RedrawsInPlace`;
  - vista adicional en un rack existente: `ExistingRack_CanRequestAnAdditionalView_KeepingTheGuid`;
  - `LoadDesignForNew` + inserción: `LoadDesignForNew_LoadsData_KeepsSource_MintsFreshGuidOnInsert`;
  - biblioteca: `LibraryProject_IsThePushBackPayload_PreservesMetadata_AndDoesNotInsert`. Las cinco están en `PushBackEditorWindowTests.cs`;
  - compuertas tras el recálculo: `PushBackOutputGateAfterRecomputeTests.cs:108-212` y `PushBackLibrarySaveGateTests.cs`;
  - cierre: `RichEditorCloseContractTests.cs:48-129` y `EditorClosePolicyTests.cs:185-210`.
  - **Sin cobertura encontrada** (búsqueda en `tests/RackCad.UI.Tests` filtrando por PushBack):
    - `InsertBatchButton` / `InsertBatch_Click`;
    - `InsertionRequest.Views`;
    - el mensaje «Solo un sistema abierto con RACKEDITAR…»;
    - `EnsureId` en el guardado;
    - los cuatro botones por vista como **productores de intención**. Solo se miran su disposición y su habilitación.

---

## Puntos que una prueba de caracterización puede fijar

| # | Aserción concreta | API observable | Dobles y preparación | Proyecto |
|---|---|---|---|---|
| P-1 | S1 por vista: cada uno de los 4 botones produce `InsertRequested=true`, `UpdateOnly=false`, `Kind=PushBack`, `InsertView` y `InsertSection` exactos (lateral = `PostIndex` del corte; frontal 0/1 con un solo sentido y 0..3 con lado B; planta -1), `Views.Count==1`, y `RackId` acuñado con `newIdFactory` | `ClickNamed(w,"Insert*Button")`, contrato público, `InsertionRequest.Views` | `newIdFactory = () => "ID-T1B"`; STA | UI.Tests |
| P-2 | S3: Actualizar con `LoadExisting` da `UpdateOnly=true`, `InsertView=null`, `InsertSection=-1`, `Views.Count==0`, GUID adoptado; con `LoadNew` o `LoadDesignForNew` **no** hay petición y aparece el texto exacto «Solo un sistema abierto con RACKEDITAR puede actualizarse en sitio.» | `ClickNamed(w,"UpdateButton")` + `StatusText` | El botón está deshabilitado: hay que lanzar `RaiseEvent` sobre él, igual que hacen las pruebas de compuerta | UI.Tests |
| P-3 | Con `canInsertInAutoCad=false`: Insertar y Actualizar no producen petición y muestran «El dibujo en AutoCAD solo está disponible cuando la ventana se abre desde AutoCAD.»; `InsertButton`, `UpdateButton` y los 4 por vista quedan deshabilitados con su `ToolTip` exacto | `new RackPushBackSystemWindow()` + `RaiseEvent` | — | UI.Tests |
| P-4 | **Estado actual**: `InsertBatchButton.IsEnabled == true` incluso con `canInsertInAutoCad=false` y con entradas inválidas. Se fija lo que hay, sin corregirlo | `FindName("InsertBatchButton")` | — | UI.Tests |
| P-5 | S4 (lote): `Views` ordenadas tal como se eligieron, `View`/`Section` = `Views[0]`, `UpdateOnly=false`, identidad acuñada **una** vez | Contrato público e `InsertionRequest.Views` | **Requiere** una costura nueva o interceptar el modal (`RackViewBatchDialogPresenter.TryShow` es estático, H-37). Alternativa: guarda de fuente sobre `W.cs:3294-3321` | UI.Tests (con costura) o guarda de fuente |
| P-6 | S5: cerrar sin trabajo pendiente no pregunta y no deja petición. Con módulos pendientes se pregunta con el texto exacto (`W.cs:3419`); «no» cancela el cierre y «sí» cierra sin petición. Tras Insertar o Actualizar, `OnClosing` no pregunta | `EditorDiscardPrompt.Substitute`, `PendingWork()`, `Close()` en una ventana mostrada | Colección `EditorDiscardPromptCollection` (estado estático) | UI.Tests |
| P-7 | La pregunta de descarte se muestra **sin owner**: el delegado de 2 argumentos de `Substitute` recibe `owner == null` | `EditorDiscardPrompt.Substitute((q, o) => …)` | Como P-6 | UI.Tests |
| P-8 | S6: guardar **no** marca inserción. El contenido que se guardaría es `RackProject.ForPushBack(design).WithSourceMetadataFrom(source)`, con los campos desconocidos preservados. Tras un guardado aceptado, `RackId` deja de ser null y la inserción siguiente lo **reutiliza** | `BuildLibraryProjectForTest()`; para el efecto del GUID hace falta guardar de verdad | **No hay doble** de disco ni de diálogo (H-38). Para el GUID tras guardar: costura nueva o guarda de fuente del orden `EnsureId` → `Save` (`W.cs:3463-3468`) | UI.Tests (contenido) + guarda de fuente (orden y destino) |
| P-9 | `Owner` y `CenterOwner` de los subdiálogos con costura de solo-`ShowDialog`: `SelectiveSafetyWindow`, `SafetyPerPostWindow` y `RackFrameConfiguratorWindow` tienen `Owner == w`. `SafetyPerPostWindow`, abierta desde una sección dentro del diálogo de seguridad, tiene `Owner == w`, **no** el diálogo de seguridad | `SafetyWindowDialog`, `BootPerPostWindowDialog`, `HeaderConfiguratorPresenter` | Ventana mostrada fuera de pantalla (patrón `Shown()`) | UI.Tests |
| P-10 | `Owner` de `SafetyTopeGridWindow`, `SafetyDefensaGridWindow`, `RackBomWindow`, `RackViewBatchDialog` y `SaveFileDialog`: hoy solo se puede fijar por **guarda de fuente** (`{ Owner = this }` en `W.cs:2721`, `:2865`, `:3047`, `:3438`; `TryShow(this, …)` en `:3311`; `PromptSaveToLibrary(this, …)` en `:3458`) | Texto del código | — | RackCad.Tests (guarda de fuente) |
| P-11 | La ventana no tiene `DialogResult` ni suscriptores de `InsertRequestedRaised`: el canal de vuelta es solo el contrato público | Guarda de fuente: 0 `DialogResult` en `Systems/PushBack/*.cs`; 0 `InsertRequestedRaised +=` en `src/` | — | RackCad.Tests o UI.Tests (lectura de fuente) |
| P-12 | Lo que lee cada llamador (H-7..H-10): RACKPUSHBACK solo `InsertionRequest` + `View/Section`; RACKEDITAR `InsertRequested`, `DesignToInsert`, `SystemToInsert`, `RackId` (respaldo), `RackName`, `UpdateOnly`, `InsertionRequest.Views`; el módulo `InsertRequested ? InsertionRequest : null`; el menú `pushBack.Views` | Guarda de fuente sobre `RackPushBackCommands.cs:47-65` y `:168-198`, `EditorModules.cs:112-131`, `RackMenuCommands.cs:131-143` | — | RackCad.Tests (patrón de `PushBackPluginSourceGuardTests`) |
| P-13 | Escrituras del DWG por camino, en orden: RACKPUSHBACK → los 3 `DrawAndPlace`; RACKEDITAR → comprobaciones previas < primer `RedrawInPlace(…, regen: false)` < `SyncName` < `EraseViewBlocks` < un único `Regen()` < `RackViewBatchExecution.Run` (solo si `!UpdateOnly`); menú → `RackViewBatchProducts.PushBack` + `PlacePushBack(regen:false)`. Parte ya cubierta en `PushBackRoundTripSourceGuardTests` | Guarda de fuente | — | RackCad.Tests |
| P-14 | Las tres fachadas de Push Back: firmas públicas exactas (H-29); los caller-owned existen y no tienen llamador en producción (H-30). Así T4B no las cambia sin que se note | Guarda de fuente, con el `Body()` de `CallerOwnedFacadeGuardTests` | — | RackCad.Tests |
| P-15 | El configurador de cabecera escribe su disposición al cerrarse (`OnClosed` → `SaveCurrentLayout`) | Guarda de fuente (`RackFrameConfiguratorWindow.xaml.cs:602-604`) | Ejecutarlo de verdad escribiría en `%LOCALAPPDATA%` | RackCad.Tests (guarda de fuente) |

## Riesgos y ambigüedades

- **R-1 · «Puerto» no existe como tipo.** La «intención y escrituras del puerto» de INV-19 y D-15 no tienen hoy una interfaz que doblar.
  - La intención es el estado de una `RackEditorSession` privada, más `Close()`.
  - Las escrituras de disco son llamadas directas.
  - Las del DWG están en el llamador, ya fuera de la ventana.
  - El contrato de T1B tiene que decir qué entiende por «escrituras del puerto»:
    - (a) la petición en memoria: UI.Tests;
    - (b) el guardado en biblioteca y la disposición del configurador: sin doble, solo contenido o guarda de fuente;
    - (c) el DWG en el Plugin: solo guarda de fuente.
- **R-2 · La clausura no contiene a todos los llamadores ni todas las dependencias.**
  - `EditorModules.cs` (un llamador) está **dentro** de `Editor/**`, pero `RackPushBackCommands.cs` es un llamador del Plugin. Si cuenta como «fachada de escritura», también está dentro.
  - `UiSupport.cs`, `CatalogOption.cs`, `SafetyDefenseFace.cs`, `Views/**`, los subdiálogos transitivos (Guía, Desviador, Parrilla), `RackFrames/**` y `Domain/Systems/Selective/SelectivePalletDesign.cs` están **fuera** de las carpetas declaradas (H-33/H-34).
  - Hay que fijar su estatus por ruta antes de emitir la tarea, como pide el propio paquete (`I-64-f4-t2-gate-contract-package.md:66`).
- **R-3 · «Fachadas de escritura de Push Back del Plugin» admite dos lecturas.**
  - Estricta: los 3 servicios de dibujo (H-04).
  - Amplia: además `RackPushBackCommands.cs`, `PushBackKindHandler.cs` y los fragmentos de Push Back de `RackViewBatchProducts.cs`, `RackViewPlacement.cs` y `RackMenuCommands.cs`.
  - El camino del menú (H-27) **no pasa por las 3 fachadas**, así que la lectura estricta deja fuera la escritura de un rack nuevo desde RACKCAD.
- **R-4 · Asimetría del lote** (H-28): RACKPUSHBACK dibujaría solo `Views[0]`. Caracterizarla la congela. Corregirla sería un cambio de comportamiento fuera de T1B. Conviene que la tarea la registre como «estado observado», no como contrato deseado.
- **R-5 · `InsertBatchButton` no tiene compuerta** (H-16, P-4) y `InsertBatch_Click` no comprueba `canInsertInAutoCad` (H-15 S4). En producción `canInsertInAutoCad` siempre es `true`, así que solo se nota en el modo autónomo o en pruebas.
- **R-6 · Lateral sin corte** (`section = -1`): en RACKPUSHBACK lleva a un `GetInteger` en la línea de órdenes (`RackPushBackCommands.cs:101-104`). En el menú, `Views` queda vacío, porque `DecodePushBack` exige `Section >= 0` para lateral (`src/RackCad.Application/Systems/Shared/RackViewCodec.cs:271-275`). No se midió cómo responde `RackViewBatchExecution` a un lote vacío.
- **R-7 · Efectos reales en las pruebas.** Cualquier prueba que pulse «Guardar en biblioteca» ejecuta `Directory.CreateDirectory` en el `%APPDATA%` real y lee `settings.json`, aunque luego «lance en el diálogo» (H-38). Abrir y cerrar el configurador real escribe en `%LOCALAPPDATA%`. La caracterización debe evitarlo o declararlo.
- **R-8 · `Owner` en los caminos de AutoCAD.** RACKPUSHBACK y RACKEDITAR no asignan `Owner`; lo hace `ShowModalWindow`, cuyo comportamiento no se puede medir desde el código. El `Owner` de la ventana clásica en esos caminos solo es comprobable en el host (smoke).
- **R-9 · Estado estático compartido.** `EditorDiscardPrompt` es estático (`EditorClosePolicy.cs:51`). Las pruebas que lo sustituyan deben ir en `EditorDiscardPromptCollection` para no correr en paralelo con otras.
- **R-10 · El recuento de G2 es por carpeta** (43), como declara el contrato; el uso directo medido es mucho menor (H-32/§4). Modificar un control no usado, por ejemplo `SelectionMatrix*`, cuenta igual como «modificar la clausura». Es una decisión del contrato, no un hallazgo de código.
