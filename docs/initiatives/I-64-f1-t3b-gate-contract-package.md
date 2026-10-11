# I-64 — Paquete del contrato de gate de F1-T3B-HOST-UI

```text
Estado: PAQUETE NOT EMITTED — REVISADO con la decisión del Coordinator (evidencia §54, commit c7f389cb)
Emisor: Coordinator (AUTOMATION_PLAN 16.5; agent-execution/README §1)
Preparado por: sesión responsable de I-64 (Claude), por la revisión del cierre de T3A (evidencia §52)
Rama al preparar: architecture/workspace-persistente-rackcad en ee8018e0 (CI 38120717985 4/4), sobre origin/main bb0d5522
Producto de T3A inspeccionado: ba52b3b983fb7d2a02393df643e8323b125b3d03 (src/ y tests/ idénticos en ee8018e0)
Decisiones C-T3B-01..C-T3B-09: resueltas por el Coordinator (§54). Lo que vale para la emisión es la sección 15
Siguiente: emisión del contrato (AuthorityRevision c7f389cb), una planificación, A1-A8 y nc4; STOP en COORDINATOR_T3B_ACCEPTANCE_REQUIRED
```

Este documento no es un `gate-contract.json`, no emite delegaciones y no autoriza trabajo. **No se ha invocado ninguna planificación ni
ningún Worker de T3B.** Deriva el contrato del repositorio actual tras T3A. No se apoya en los supuestos del paquete monolítico de T3
(`I-64-f1-t3-gate-contract-package.md`): cada hecho de la sección 3 está comprobado en `ba52b3b9`.

> **Revisión tras la decisión del Coordinator (§54):** las secciones 1-14 se conservan como el paquete que se presentó. Donde difieran
> de la decisión manda la **sección 15**, que es la versión que se emite. En particular, la opción C de C-T3B-02 (sondeo de 500 ms)
> quedó **rechazada** y la sustituye un relevo por eventos.

**Conclusión principal (paquete original):** T3B **no se puede implementar sin reabrir archivos de producto de T3A**, al menos `WorkspacePanelCommand.cs`
(sección 5). Por la orden de la revisión, esto es un STOP al Coordinator antes de emitir el contrato. El paquete propone deltas exactos
para que el Coordinator los autorice o los rechace; no amplía el alcance por su cuenta.

## 1. Fuentes

- Revisión del cierre de T3A (literal en `docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/coordinator-close-review.md`; evidencia
  §52): T3B autorizada solo para preparar el contrato; objetivo conceptual y áreas de prueba esperadas.
- Decisión de descomposición (`coordinator-decision-t3-decomposition.md`, evidencia §49): contenido de T3B y presupuesto por `TaskId`.
- Freeze V5 (`docs/initiatives/I-64-proposal-v5.md`, blob `bd4e9626`): D-01 (host, foco, perfil), D-02 (propiedad y capas), D-03 (cambio
  de documento activo), D-17 (contención), D-18 (P-01b, P-02 e instrumentación), INV-13, INV-24, INV-25 e INV-33.
- Producto de T3A en `ba52b3b9`, sus guardas y el resto del repositorio en `ee8018e0`.
- `docs/automation/agent-execution/routing.md` y `model-catalog.md` en `bb0d5522` (sin cambios en la rama).

## 2. Objetivo de T3B

Host visual de F1 del panel, sin escrituras en el DWG, sin navegación y sin AutoCAD en ejecución (orden de la revisión y D-01, D-02, D-17,
D-18):

- **exactamente un `PaletteSet` por proceso**, creado de forma perezosa desde la ruta del controlador de T3A y `RACKPANEL`; sin `toolID` y
  sin manejadores `Load`/`Save`; acoplable y flotante; cerrar = ocultar (las sesiones siguen en memoria); un fallo al crearlo se informa y
  los comandos clásicos siguen intactos;
- **un único control raíz `UserControl` de `RackCad.UI`** alojado con `AddVisual`;
- **view-model** que presenta el estado del puerto de T3A: documento y sesión activos, estado vacío sin documento, `NotStarted`, `Ready`,
  `Degraded`, disponibilidad de acciones y su motivo, PICKFIRST desactivado y los cinco contextos de selección;
- **visible/oculto** a través del controlador de T3A (`ShowPanel`/`HidePanel`), nunca directamente sobre el puente;
- **política de foco** en la frontera host/UI (mecánica no congelada; comprobación solo de host en Smoke-1, INV-24);
- **contención de excepciones** originadas en el panel y **estado Error** (D-17, INV-25 panel);
- **fusión local de recursos WPF** (no hay `App.xaml`);
- **presentación de la instrumentación** en memoria de T3A (D-18);
- sin escrituras authored, sin navegación (F2), sin `Autodesk.AutoCAD.Internal`.

**Fuera de T3B:** navegación (F2), pestañas (F3), borradores, Actualizar y veto de cierre (F4), editores (F5), inventario (F6), F1-T3C
(RESERVED / NOT AUTHORIZED), Smoke-1 y cualquier uso de AutoCAD, perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL o registro.

## 3. Superficie real de T3A en `ba52b3b9`

| Pieza | Archivo | Superficie pública |
|---|---|---|
| Puerto | `src/RackCad.Application/Workspace/IWorkspaceHostPort.cs` | `GetSnapshot()`, `Authorize(WorkspaceRequest)`, `GetSelectionContext(WorkspaceRequest)`, `GetMeasurementReport()`; solo lectura; **sin evento de cambio** |
| Valores | `WorkspaceHostSnapshot.cs` | `WorkspaceHostStatus` {`NotStarted`, `NoDocument`, `Ready`, `Degraded`}; `WorkspaceActionBlock` {`None`, `NotStarted`, `NoDocument`, `Degraded`, `ModalActive`, `CommandActive`}; `WorkspaceActionAvailability(CanReadDrawing, CanWriteDrawing, Block)`; `WorkspaceHostSnapshot(Status, ActiveSession, PanelVisible, Actions, Selection, PickFirstDisabled, InstrumentationEnabled)`; `WorkspaceSelectionResponse(Decision, Context)` |
| Contexto | `SelectionContext.cs` (T1) | `SelectionContextKind` {`None`, `One`, `Selection`, `NoIdentity`, `Diagnostic`}; `Kind`, `RackId` (solo en `One`), `RackCount`, `OtherCount` |
| Instrumentación | `WorkspaceInstrumentation.cs` | `WorkspaceMeasurementReport(Enabled, Points, CurrentProcessMemoryBytes)`; `WorkspaceMeasurement(Sequence, Point, Milliseconds, ProcessMemoryBytes, Count)`; puntos `panel.open`, `panel.show`, `panel.hide`, `drain`, `selection.read`, `cache.invalidate`; capacidad 256 |
| Núcleo de host | `WorkspaceHostCore.cs` | `OpenPanel`, `ShowPanel`, `HidePanel`, `RefreshPanel`, `SetModalRackCadActive`, `SetInstrumentationEnabled`, `OnDrained`, `GetSnapshot`, `Authorize`, `GetSelectionContext`, `GetMeasurementReport`; no notifica cambios |
| Perezoso | `WorkspaceLazyInstance.cs` | `WorkspaceLazyInstance<T>(Func<T>)`: `GetOrCreate()`, `IsCreated`, `CreateCount` |
| Controlador | `src/RackCad.Plugin/Workspace/WorkspaceController.cs` | `GetOrCreate()`, `IsCreated`, `Port`, `PanelVisible`, `OpenPanel`, `ShowPanel`, `HidePanel`, `RefreshPanel`, `SetModalRackCadActive`, `SetInstrumentationEnabled`; única suscripción: `_bridge.Drained += OnDrained`; **no expone ningún evento** |
| Puerto (Plugin) | `WorkspaceHostPort.cs` | delega en el núcleo |
| Comando | `WorkspacePanelCommand.cs` | `[CommandMethod("RACKPANEL")]`: `WorkspaceController.GetOrCreate().OpenPanel()` y el mensaje «panel de trabajo iniciado (esta version aun no muestra la interfaz)»; excepciones a `RackCommandSupport.Report` |
| Lector | `WorkspaceSelectionReader.cs` | lectura fina solo desde el drenaje |

Espacios de nombres: `RackCad.Application.Workspace` y `RackCad.Plugin.Workspace`. `RackCad.UI` todavía no tiene carpeta `Workspace/`.

## 4. Hallazgos que condicionan el contrato

### 4.1 Hechos del código

| # | Hecho | Efecto en T3B |
|---|---|---|
| H-B1 | `RACKPANEL` solo abre el estado y escribe que la interfaz aún no se muestra. La guarda de T3A `TheCommandOnlyObtainsTheControllerAndOpensThePanel` exige `GetOrCreate()` y `OpenPanel()` y prohíbe `PaletteSet`, `AddVisual` y `System.Windows` en el comando; ninguna prueba fija el texto del mensaje | mostrar la paleta exige **cambiar `WorkspacePanelCommand.cs`** (archivo de producto de T3A). Su guarda sigue en verde si el comando llama a un host de paleta que no nombra `PaletteSet` |
| H-B2 | El puerto es de solo lectura y de consulta; el controlador solo se suscribe a `Drained` y no expone ningún evento | la UI no puede enterarse de un drenaje sin un cambio en T3A o un sondeo propio (C-T3B-02) |
| H-B3 | `EventBridgeCore.OnDocumentActivated` solo encola si ya había pistas pendientes: activar un documento sin pistas no produce drenaje | el Freeze (D-03: «Cambio de documento activo: el panel muestra la sesión del nuevo documento») no se cumple solo con un aviso tras el drenaje (C-T3B-02). La prueba de T2 `WorkspaceBridgeHintAndDrainTests.OnlyTheActiveDocumentDrainsAndTheOthersKeepTheirHintsUntilActivated` fija el recuento de `Idle` tras una activación |
| H-B4 | `SetInstrumentationEnabled` solo existe en el controlador; el puerto no lo expone | la UI puede presentar `GetMeasurementReport()`; un interruptor exige un delegado inyectado por el host del Plugin, sin cambiar el puerto (C-T3B-04) |
| H-B5 | `SetModalRackCadActive` existe, pero nadie lo llama | conectar los modales existentes no está en T3B (cambiaría comandos clásicos: M-03). Se registra |
| H-B6 | El Plugin ya referencia `RackCad.UI` y `AcMgd` (donde vive `Autodesk.AutoCAD.Windows.PaletteSet`); la CI compila el Plugin con `AutoCAD.NET` 25.0.1 y `verify-autocad-references.ps1 -NoWarnings` | no hace falta tocar ningún `csproj`. Ninguna prueba referencia el Plugin: la paleta solo se comprueba con guardas de fuente y en Smoke-1 |
| H-B7 | No hay `App.xaml`. Las ventanas fusionan `"/RackCad.UI;component/Themes/AppStyles.xaml"` en sus `Resources.MergedDictionaries` (XAML o código); `Shell/ShellResources` es `internal` y explica que un `DynamicResource` no cae al diccionario del tema | el control raíz fusiona `AppStyles.xaml` en sus propios recursos; prueba de UI sin `Application` |
| H-B8 | `RackCad.UI` no tiene ningún `UserControl`; `Nullable` = disable (`Directory.Build.props`); base `ObservableObject` | el control raíz es el primer `UserControl`; el código nuevo de UI sigue la convención del proyecto |
| H-B9 | `tests/RackCad.UI.Tests` no tiene clases `Workspace*`: el filtro `FullyQualifiedName~RackCad.UI.Tests.Workspace` selecciona 0 hoy | RED de UI con clases `Workspace*` en el espacio de nombres `RackCad.UI.Tests` |
| H-B10 | La mecánica de foco no está congelada (D-01); `Autodesk.AutoCAD.Internal` está prohibido por la orden; INV-24 es solo de host | T3B fija solo la configuración documentada de la paleta y la ausencia de manejo de teclado propio (C-T3B-05) |
| H-B11 | Un manejador global (`Dispatcher.UnhandledException`, `AppDomain.UnhandledException`) en el proceso de AutoCAD capturaría excepciones ajenas | la contención va en las fronteras del panel, no en un manejador global (INV-F1-T3B-07) |

### 4.2 Guardas y censos afectados

Inventario de solo lectura sobre `ee8018e0` (los archivos de `tests/` no los recorre ninguna de estas guardas; XAML tampoco).

**Fallan por construcción y exigen un cambio exacto en una prueba de T3A** (`tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs`):

| Guarda (T3A) | Regla | Por qué falla con T3B |
|---|---|---|
| `NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType` | en todo `Plugin/Workspace/` y `Application/Workspace/`: `Autodesk\.AutoCAD\.Internal\|PaletteSet\|AddVisual\|System\.Windows\|RackCad\.UI\|Autodesk\.AutoCAD\.Windows\|\bUserControl\b\|\bDispatcher\b` | el dueño de la paleta necesita `PaletteSet`, `AddVisual`, `Autodesk.AutoCAD.Windows` y el control de `RackCad.UI`. Hace falta una **excepción de archivo exacto** que siga prohibiendo `Internal` y `Dispatcher` (G1) |
| `NothingOutsideThePluginWorkspaceReachesTheController` | ningún archivo de `src/` fuera de las dos carpetas Workspace contiene `WorkspaceController\|WorkspaceHostPort\|WorkspaceSelectionReader\|WorkspaceHostCore` (sin `\b`) | `IWorkspaceHostPort` contiene `WorkspaceHostPort`: **la UI no puede ni nombrar el puerto** que D-02 le asigna. Hace falta acotar la regla a identificadores completos, de modo que siga prohibiendo el controlador, el puerto concreto, el lector y el núcleo (G2) |

**Se cumplen si T3B respeta sus tokens** (aplican a cualquier archivo nuevo de `src/RackCad.Plugin/Workspace/`):

| Origen | Regla que el archivo de la paleta debe respetar |
|---|---|
| T3A `WorkspaceHostSourceGuardTests` | sin `[CommandMethod`; sin `WriteMessage` (solo el comando escribe en la línea de comandos); sin constructor estático `static Workspace…()` ni `ModuleInitializer`/`IExtensionApplication`/`[assembly`; sin APIs del lector; sin `Guid.NewGuid` (un `toolID` generado fallaría); sin escritura de selección o vista; sin `Timer`, `async`, `Task.Run`, hilos ni `SynchronizationContext`; sin `System.IO`, `Registry`, `UserSettings`, XData ni NOD; sin `RackLog.Warning`; `RackLog.` solo dentro de `static void LogFault(...)`; ningún tipo cuyo nombre contenga `Log` (también `Dialog`, `Catalog`, `Logic`); sin `Metric`, `Provider`, `Aggregat`, `ProjectSummary`, `TotalRacks` ni `Bom` (también `IFormatProvider`, `AggregateException`) |
| T2 `WorkspaceBridgeSourceGuardTests` (con los refinamientos de T3A) | `+=`/`-=` sobre los eventos gobernados de AutoCAD solo en `AutoCadEventBridge.cs` (`PaletteSet.StateChanged` no está en la lista; `DocumentActivated` sí); sin `GetString`, `ShowDialog`, `ShowModelessDialog`, `MessageBox`, `GetObject`, `Zoom`, `Regen`, `Highlight`…; ninguna firma pública con tipos de AutoCAD ni `…EventArgs` (el manejador de `StateChanged` tiene que ser privado); sin `Save(`, `SaveAs`, `Registry.`, `UserSettings`; sin `Autodesk.AutoCAD.Internal` |
| T3A `WorkspaceSourceGuardExceptionTests` | las únicas excepciones de la guarda de T2 siguen siendo `WorkspacePanelCommand.cs` y `WorkspaceSelectionReader.cs` (recuento exacto 2): T3B no añade una tercera |
| T3A `WorkspaceBridgeFacadeSourceGuardTests` | ningún archivo del Plugin nombra `WorkspaceSessionRegistry` |
| T3A `WorkspaceHostSourceGuardTests` (comando) | un único `[CommandMethod("RACKPANEL")]`; el comando contiene `WorkspaceController.GetOrCreate()` y `.OpenPanel()`, `catch (System.Exception` y `RackCommandSupport.Report(`, y no contiene `PaletteSet`, `AddVisual`, `System.Windows`, `RackCad.UI` ni `Autodesk.AutoCAD.Windows`. Llamar a una clase `WorkspacePaletteHost` la cumple; ninguna prueba fija el texto del mensaje actual |
| T3A `TheControllerSubscribesOnlyToTheDrainedEventOfTheBridge` | exactamente un `+=` en el controlador y ningún `-=` (relevante para la opción B de C-T3B-02) |

**Opciones de refresco que chocan con guardas existentes** (C-T3B-02):

| Opción | Resultado frente a las guardas |
|---|---|
| Evento en `IWorkspaceHostPort` o una operación nueva | falla cuatro pruebas de `WorkspaceHostPortGuardTests` (sin eventos, prefijos de método, sin delegados, un solo `WorkspaceRequest`) y `ThePluginImplementationOnlyDelegatesToTheHostCore` (4 llamadas `_core.`) |
| Evento en `AutoCadEventBridge` o en `EventBridgeCore` | falla la superficie pública exacta y los recuentos de `+=`/`-=` de `WorkspaceBridgeFacadeSourceGuardTests` |
| Encolar una reconciliación al activar un documento | falla `StartAndActivationOfTheCoreDoNotQueueAReconciliation`, que codifica la regla de la corrección de alcance del Coordinator («Do not change Start() or OnDocumentActivated() reconciliation behavior»), y altera recuentos de `Idle` de pruebas de T2 |
| Evento en `WorkspaceController` (sin nuevo `+=` en él) | pasa las guardas, pero reabre un archivo de producto de T3A y no resuelve el cambio de documento sin pistas (H-B3) |
| Refresco en memoria con temporizador del dispatcher dentro de `src/RackCad.UI/Workspace/` | pasa todas las guardas (ninguna recorre `RackCad.UI` por temporizadores); el mismo código en las carpetas Workspace del Plugin o de Application fallaría |

**Guardas globales que el código nuevo debe respetar:**
- **UI sin AutoCAD** (INV-13): `ProjectVariablesConformanceTests.LA_UI_ENTERA_SIGUE_SIN_CONOCER_AUTOCAD` (texto, también literales),
  `CantileverRoundTwoSourceGuardTests.NingunProyectoFueraDelPluginNombraAutodesk`, `CustomPropertiesEdgeGuardTests.TGrd06_…`,
  `ComputedParametersSummaryGuardTests.INV29_…` (cierre de referencias de `RackCad.UI.csproj` = WPF) y las guardas de ensamblado de
  `EditorVisualShellTests`, `PushBackEditorWindowTests`, `RackMainMenuStructuralSectionTests` y `StructuralSectionInspectorTests`. Todas
  recorren el proyecto entero: la carpeta nueva queda cubierta sin cambios.
  - **Discrepancia registrada:** el Freeze (tabla de INV-13) asigna INV-13 a `UiSystemBoundaryGuardTests`, que solo comprueba `x:Class`,
    carpetas y URIs. La protección real es la lista anterior.
- **`NamespaceFolderGuardTests`:** `src/RackCad.UI/Workspace/*.cs` declara `namespace RackCad.UI.Workspace` con bloque (no file-scoped);
  las pruebas declaran exactamente `RackCad.UI.Tests` o `RackCad.Tests` (con carpeta o sin ella); no usar `Systems/` como carpeta.
- **`UiSystemBoundaryGuardTests`:** cada `.xaml` con `x:Class` = `RackCad.UI.Workspace.<Archivo>` y su `.xaml.cs`; URIs con
  `;component/` hacia archivos existentes.
- **Censos:** `WindowCensusGuardTests` cuenta subclases concretas de `Window` (31): un `UserControl` no cambia nada; una ventana nueva sí.
  Censo de comandos: 38 con `RACKPANEL` (`CustomPropertiesCommandGuardTests.TGrd08_…` y `SelectiveEditorOpenTests.GUARDA_EL_CENSO_…`,
  que leen texto crudo: un comentario con `[CommandMethod("` en un archivo nuevo rompe los dos). Censo de ayuda
  (`CustomPropertiesHelpCensusTests`): `RACKPANEL` no está en la ayuda y añadirlo cambiaría el censo; fuera de T3B. No hay censo de XAML
  ni de controles.
- **`PluginInitializer.cs`:** `ThePluginInitializerIsUntouched` prohíbe `Workspace`, `RACKPANEL` y `Controller` en él. La paleta no se
  libera en `Terminate()`: vive hasta que AutoCAD termina, como el controlador (D-02).

## 5. ¿Hay que reabrir archivos de T3A?

**Sí.** Lista mínima por opción de C-T3B-02 (todos como deltas exactos, como hizo T3A con los dos archivos de T2):

| Archivo (T3A) | Tipo | Delta exacto propuesto | Opción |
|---|---|---|---|
| `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs` | producto | tras `WorkspaceController.GetOrCreate().OpenPanel()`, mostrar la paleta mediante `WorkspacePaletteHost` (sin nombrar `PaletteSet`); sustituir el mensaje «esta version aun no muestra la interfaz»; mismo `catch` y mismo `RackCommandSupport.Report` | **todas** (C-T3B-01) |
| `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs` | prueba | G1: excepción de archivo exacto para `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` en `NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType`, que en ese archivo sigue prohibiendo `Autodesk.AutoCAD.Internal` y `Dispatcher`. G2: en `NothingOutsideThePluginWorkspaceReachesTheController`, identificadores completos (`\bWorkspaceController\b\|\bWorkspaceHostPort\b\|\bWorkspaceSelectionReader\b\|\bWorkspaceHostCore\b`), de modo que `IWorkspaceHostPort` quede permitido fuera y el resto siga prohibido. Ninguna prueba se borra ni se salta | **todas** (C-T3B-03) |
| `src/RackCad.Plugin/Workspace/WorkspaceController.cs` | producto | un evento público aditivo, sin `+=` nuevo en el controlador, disparado tras `_host.OnDrained(outcome)` | solo B |
| `EventBridgeCore.cs`, `AutoCadEventBridge.cs` y sus pruebas de T2/T3A | producto y pruebas | fachada o encolado en la activación | solo A (desaconsejada) |

Con la opción recomendada (C) se reabren **un archivo de producto de T3A** (`WorkspacePanelCommand.cs`) y **una prueba de T3A**
(`WorkspaceHostSourceGuardTests.cs`). Ningún archivo de T1 ni de T2 cambia. Ninguna de estas reaperturas está autorizada todavía.

## 6. Alcance de escritura propuesto

Para la opción recomendada de C-T3B-02 (C). Formato del contrato: prefijos con `/` final y archivos exactos.

**`AllowedWriteScope` (6 entradas):**
  - `src/RackCad.UI/Workspace/`
  - `tests/RackCad.UI.Tests/Workspace/`
  - `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs`
  - `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs`
  - `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs`

- `src/RackCad.UI/Workspace/` y `tests/RackCad.UI.Tests/Workspace/` son carpetas nuevas: solo archivos nuevos.
- `WorkspacePaletteHost.cs` y `WorkspacePaletteHostSourceGuardTests.cs` son archivos nuevos con nombre fijo.
- `WorkspacePanelCommand.cs` y `WorkspaceHostSourceGuardTests.cs` solo con los deltas exactos de la sección 5.

**`ForbiddenWriteScope` (148 entradas),** derivado del de T3A: se quitan `src/RackCad.UI/` y
`tests/RackCad.UI.Tests/` (se enumera todo lo que no es `Workspace/`), `src/RackCad.Application/Workspace/` pasa a prohibirse entero, se
prohíben los otros cuatro archivos de `src/RackCad.Plugin/Workspace/`, los dos censos de comandos y todas las pruebas de
`tests/RackCad.Tests/Workspace/` salvo la guarda de G1/G2:
  - `docs/`
  - `.github/`
  - `assets/`
  - `eng/`
  - `deploy/`
  - `tools/`
  - `global.json`
  - `Directory.Build.props`
  - `Directory.Build.targets`
  - `RackCad.sln`
  - `src/RackCad.Domain/`
  - `src/RackCad.UI/CatalogOption.cs`
  - `src/RackCad.UI/Controls/`
  - `src/RackCad.UI/Editor/`
  - `src/RackCad.UI/EnumDisplayConverter.cs`
  - `src/RackCad.UI/MainMenuAction.cs`
  - `src/RackCad.UI/ObservableObject.cs`
  - `src/RackCad.UI/Preview/`
  - `src/RackCad.UI/PreviewCanvasPainter.cs`
  - `src/RackCad.UI/ProjectVariableRepairText.cs`
  - `src/RackCad.UI/RackBomWindow.xaml`
  - `src/RackCad.UI/RackBomWindow.xaml.cs`
  - `src/RackCad.UI/RackCad.UI.csproj`
  - `src/RackCad.UI/RackCommandHelpWindow.cs`
  - `src/RackCad.UI/RackCommandReference.cs`
  - `src/RackCad.UI/RackConsolidatedBomWindow.xaml`
  - `src/RackCad.UI/RackConsolidatedBomWindow.xaml.cs`
  - `src/RackCad.UI/RackCustomPropertiesWindow.xaml`
  - `src/RackCad.UI/RackCustomPropertiesWindow.xaml.cs`
  - `src/RackCad.UI/RackDesignLibraryWindow.xaml`
  - `src/RackCad.UI/RackDesignLibraryWindow.xaml.cs`
  - `src/RackCad.UI/RackFrames/`
  - `src/RackCad.UI/RackListRow.cs`
  - `src/RackCad.UI/RackListWindow.xaml`
  - `src/RackCad.UI/RackListWindow.xaml.cs`
  - `src/RackCad.UI/RackMainMenuWindow.xaml`
  - `src/RackCad.UI/RackMainMenuWindow.xaml.cs`
  - `src/RackCad.UI/RackProjectVariablesWindow.xaml`
  - `src/RackCad.UI/RackProjectVariablesWindow.xaml.cs`
  - `src/RackCad.UI/RackWarehouseFillWindow.cs`
  - `src/RackCad.UI/RackWarehouseLayoutWindow.cs`
  - `src/RackCad.UI/SafetyDefensaGridWindow.cs`
  - `src/RackCad.UI/SafetyDefenseFace.cs`
  - `src/RackCad.UI/SafetyDesviadorGridWindow.cs`
  - `src/RackCad.UI/SafetyGuiaEntradaGridWindow.cs`
  - `src/RackCad.UI/SafetyParrillaGridWindow.cs`
  - `src/RackCad.UI/SafetyTopeGridWindow.cs`
  - `src/RackCad.UI/SelectiveSafetyWindow.cs`
  - `src/RackCad.UI/Shell/`
  - `src/RackCad.UI/StructuralSections/`
  - `src/RackCad.UI/Systems/`
  - `src/RackCad.UI/Themes/`
  - `src/RackCad.UI/UiSupport.cs`
  - `src/RackCad.UI/Views/`
  - `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj`
  - `tests/RackCad.UI.Tests/StaTestRunner.cs`
  - `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`
  - `tests/RackCad.UI.Tests/UiSystemBoundaryGuardTests.cs`
  - `tests/RackCad.UI.Tests/CustomPropertiesHelpCensusTests.cs`
  - `src/RackCad.Application/RackCad.Application.csproj`
  - `src/RackCad.Application/BlockNaming.cs`
  - `src/RackCad.Application/Bom/`
  - `src/RackCad.Application/Catalogs/`
  - `src/RackCad.Application/ComputedParameters/`
  - `src/RackCad.Application/CustomProperties/`
  - `src/RackCad.Application/Diagnostics/`
  - `src/RackCad.Application/Drawing/`
  - `src/RackCad.Application/Expressions/`
  - `src/RackCad.Application/Formatting/`
  - `src/RackCad.Application/Geometry/`
  - `src/RackCad.Application/Layout/`
  - `src/RackCad.Application/Persistence/`
  - `src/RackCad.Application/ProjectVariables/`
  - `src/RackCad.Application/RackFrames/`
  - `src/RackCad.Application/Settings/`
  - `src/RackCad.Application/StructuralSections/`
  - `src/RackCad.Application/Systems/`
  - `src/RackCad.Application/Units/`
  - `src/RackCad.Application/Views/`
  - `src/RackCad.Application/Workspace/`
  - `src/RackCad.Plugin/CustomPropertiesData.cs`
  - `src/RackCad.Plugin/CustomPropertiesExecutor.cs`
  - `src/RackCad.Plugin/InDocumentTransaction.cs`
  - `src/RackCad.Plugin/LayerHelper.cs`
  - `src/RackCad.Plugin/PluginInitializer.cs`
  - `src/RackCad.Plugin/ProjectVariableMutationExecutor.cs`
  - `src/RackCad.Plugin/ProjectVariablesData.cs`
  - `src/RackCad.Plugin/ProjectVariablesRegistry.cs`
  - `src/RackCad.Plugin/RackAyudaCommands.cs`
  - `src/RackCad.Plugin/RackBlockFinder.cs`
  - `src/RackCad.Plugin/RackCabeceraCommands.cs`
  - `src/RackCad.Plugin/RackCad.Plugin.csproj`
  - `src/RackCad.Plugin/RackCamaCommands.cs`
  - `src/RackCad.Plugin/RackCantileverCommands.cs`
  - `src/RackCad.Plugin/RackCatalogLoader.cs`
  - `src/RackCad.Plugin/RackCloner.cs`
  - `src/RackCad.Plugin/RackCommandSupport.cs`
  - `src/RackCad.Plugin/RackDinamicoCommands.cs`
  - `src/RackCad.Plugin/RackDuplicarCommands.cs`
  - `src/RackCad.Plugin/RackEnvelopeRestamp.cs`
  - `src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs`
  - `src/RackCad.Plugin/RackInventarioCommands.cs`
  - `src/RackCad.Plugin/RackLayoutCommands.Fill.cs`
  - `src/RackCad.Plugin/RackLayoutCommands.cs`
  - `src/RackCad.Plugin/RackMenuCommands.cs`
  - `src/RackCad.Plugin/RackPropiedadesCommands.cs`
  - `src/RackCad.Plugin/RackProyectarCommands.cs`
  - `src/RackCad.Plugin/RackPushBackCommands.cs`
  - `src/RackCad.Plugin/RackSeccionCommands.cs`
  - `src/RackCad.Plugin/RackSelectivoCommands.cs`
  - `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs`
  - `src/RackCad.Plugin/RackUnitsGuard.cs`
  - `src/RackCad.Plugin/RackVariablesCommands.cs`
  - `src/RackCad.Plugin/StructuralSectionCatalogAccess.cs`
  - `src/RackCad.Plugin/StructuralSectionCommandFlow.cs`
  - `src/RackCad.Plugin/Drawing/`
  - `src/RackCad.Plugin/KindHandlers/`
  - `src/RackCad.Plugin/Systems/`
  - `src/RackCad.Plugin/Views/`
  - `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`
  - `src/RackCad.Plugin/Workspace/WorkspaceController.cs`
  - `src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs`
  - `src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs`
  - `tests/RackCad.Tests/RackCad.Tests.csproj`
  - `tests/RackCad.Tests/NamespaceFolderGuardTests.cs`
  - `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs`
  - `tests/RackCad.Tests/SelectiveEditorOpenTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceBridgeDegradedModeTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceBridgeHintAndDrainTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceBridgeReconciliationTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceBridgeSubscriptionLifecycleTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceBridgeTestSupport.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostAuthorizationTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostDrainTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostLifecycleTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostPolicyTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostPortGuardTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceInstrumentationTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceSelectionMappingTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs`

Comprobado mecánicamente: ninguna entrada prohibida cubre una permitida, todas las rutas de archivo prohibidas existen y las permitidas
nuevas no existen todavía.

**Variaciones por opción:**
- **B:** se añade `src/RackCad.Plugin/Workspace/WorkspaceController.cs` a `AllowedWriteScope` (delta exacto) y se quita de
  `ForbiddenWriteScope`.
- **A:** se añaden `src/RackCad.Application/Workspace/EventBridgeCore.cs`, `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` y las
  pruebas de T2/T3A que fijan la activación y la superficie del puente; exigiría además que el Coordinator revoque su regla sobre
  `OnDocumentActivated`.

## 7. Invariantes propuestas (INV-F1-T3B-01..14)

Para la opción C. Cada una cita su origen.

| ID | Contenido | Origen |
|---|---|---|
| 01 | Exactamente un `PaletteSet` por proceso, en `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` y en ningún otro archivo, creado de forma perezosa con `WorkspaceLazyInstance` en la ruta de `RACKPANEL`; nada al cargar el plugin (`PluginInitializer.cs` intacto, sin constructor estático); construido sin `toolID` y sin manejadores `Load`/`Save`; `RACKPANEL` repetido no crea una segunda paleta ni un segundo control raíz | D-01, P-01, D-02 |
| 02 | `AddVisual` exactamente una vez, con un único control raíz `RackCad.UI.Workspace.WorkspacePanelView` (`UserControl`); acoplable y flotante; cerrar = ocultar; el manejador privado de `StateChanged` traduce mostrar/ocultar a `WorkspaceController.ShowPanel`/`HidePanel` y avisa al view-model; el host de la paleta solo usa la superficie pública de `WorkspaceController` (ni puente, ni núcleo, ni lector, ni registro) | D-01, D-02 |
| 03 | `RACKPANEL` sigue siendo el único comando, en `WorkspacePanelCommand.cs`, sin alias; tras `GetOrCreate().OpenPanel()` muestra la paleta mediante `WorkspacePaletteHost`; el mensaje deja de decir que la interfaz no se muestra; un fallo al crear la paleta se informa con `RackCommandSupport.Report` y los comandos clásicos siguen intactos; censo de comandos 38 y censo de ayuda sin cambios | D-01, D-17, decisión de descomposición |
| 04 | `src/RackCad.UI/Workspace/` (`namespace RackCad.UI.Workspace`) no contiene `Autodesk` (ni en literales), ni `RackCad.Plugin`, ni `PaletteSet`; consume solo `IWorkspaceHostPort`, los valores de `RackCad.Application.Workspace` y delegados inyectados; nunca nombra `WorkspaceController`, el `WorkspaceHostPort` concreto, el lector ni el núcleo; `RackCad.UI.csproj` sin cambios | D-02, ADR-0006, INV-13 |
| 05 | El view-model presenta el estado del puerto: `NotStarted`, `NoDocument` (estado vacío, acciones deshabilitadas), `Ready` (sesión activa) y `Degraded`; la disponibilidad de acciones y su motivo (`WorkspaceActionBlock`); PICKFIRST desactivado; la visibilidad. La misma instantánea da siempre la misma presentación | D-02, D-03 |
| 06 | Los cinco contextos se presentan de forma determinista: Ninguno, Uno(R) con el `RackId` exacto de la instantánea, Selección (N racks, M otros) rotulada como selección actual y nunca como total del proyecto, Sin identidad y Diagnóstico; ningún RackId se inventa ni se estampa | D-06, INV-16, INV-17, INV-36 (rótulos) |
| 07 | La presentación no crea trabajo sobre el DWG: el panel solo llama a `GetSnapshot` y `GetMeasurementReport`; nunca a `RefreshPanel` ni a `RequestReconciliation`; la visibilidad solo pasa por `ShowPanel`/`HidePanel` del controlador desde el host de la paleta | D-04, INV-01, INV-33 |
| 08 | Refresco (opción C): con la paleta visible, el view-model relee la instantánea y el informe en memoria en el dispatcher de WPF a un intervalo fijo (constante propuesta: 500 ms); detenido mientras está oculta y antes del primer `RACKPANEL`; cada ciclo contenido; el temporizador se abstrae para pruebas deterministas | D-03, P-01, P-01b, P-02 (C-T3B-02) |
| 09 | Toda entrada originada en el panel queda contenida en su frontera: ciclo de refresco, acciones del view-model (reintentar, interruptor), manejadores de la vista, manejador de `StateChanged` y delegados. Ninguna excepción llega al bucle de AutoCAD. **Sin manejadores globales** (`Dispatcher.UnhandledException`, `Application.DispatcherUnhandledException`, `AppDomain.UnhandledException`). Los fallos del host de la paleta solo se registran en `LogFault` (`RackLog.Exception`) | D-17, INV-25 (panel), H-B11 |
| 10 | Estado Error del panel: estado de presentación con mensaje y «Reintentar»; reintentar relee la instantánea; Error nunca escribe ni cambia sesiones, y se recupera a la presentación normal | D-17, PV2-13 |
| 11 | El control raíz fusiona `/RackCad.UI;component/Themes/AppStyles.xaml` en sus propios recursos y resuelve sin `App.xaml` ni recursos de aplicación; `Themes/` y `Shell/` sin cambios; URIs con `;component/` | H-B7, ADR-0019 |
| 12 | Foco: según C-T3B-05 (propuesta: `PaletteSet.KeepFocus` = true, API documentada); sin `Autodesk.AutoCAD.Internal`, sin `IKeyboardInputSink` propio ni ganchos Win32; el panel nunca devuelve el foco al dibujo en F1 (no hay acciones de navegación). Comprobación solo de host en Smoke-1 (S1-06a..h) | D-01, INV-24, O-10 |
| 13 | Instrumentación: el panel presenta `WorkspaceMeasurementReport` (activado, puntos en orden de secuencia con tiempo, memoria y recuento, y memoria actual del proceso) en solo lectura; el interruptor, si C-T3B-04 lo autoriza, pasa por un delegado inyectado por el host hacia `WorkspaceController.SetInstrumentationEnabled`; nada se escribe en el registro, en archivos ni en la configuración; `RackLog.cs` sin cambios; sin nombres con `Metric`, `Provider`, `Bom`, `ProjectSummary`, `TotalRacks` ni `Aggregat` | D-18, decisión de descomposición, C-T3-INSTRUMENTATION |
| 14 | Sin navegación, sin escritura de selección o vista, sin escrituras authored, sin persistencia (ni estado de la paleta, ni registro, ni perfil, ni archivos) y sin conceptos de I-63; ninguna ventana nueva (censo 31); solo los deltas exactos de T3A autorizados; todas las pruebas de T1, T2 y T3A en verde; ninguna prueba se borra ni se debilita; sin exclusiones por carpeta | D-16, M-03, INV-19, INV-21, MASTER-I63-I64-02 |

## 8. Pruebas requeridas y RED esperado

| Proyecto | Filtro | `MinSelected` propuesto | `ExpectRed` | Hoy en `ee8018e0` |
|---|---|---|---|---|
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 1 | true | 0 seleccionadas |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | 487 | true | 486 seleccionadas, todas en verde |

Los mínimos siguen el criterio de T3A (lo seleccionado hoy + 1); el número real lo fija la planificación. Los dos proyectos llevan
`ExpectRed` porque cada uno recibe pruebas nuevas.

**Cadena:** `TaskId` nuevo, `ChainRedSha` null: la primera entrega **exige RED** (16.8), con fallos por aserción contra un esqueleto que
compila y sin pruebas artificiales.

**Pruebas de UI** (clases `Workspace*`, `namespace RackCad.UI.Tests`, en `tests/RackCad.UI.Tests/Workspace/`, puerto simulado en las
pruebas). Derivadas de la superficie real de T3A:

| Área (orden de la revisión) | Comportamiento que se prueba | RED esperado contra el esqueleto |
|---|---|---|
| Estados vacío, documento y sesión | instantáneas con `NotStarted`, `NoDocument`, `Ready` y `Degraded` → presentación, sesión activa, acciones y motivo, aviso de PICKFIRST | el esqueleto devuelve valores por defecto: aserciones fallidas |
| Cinco contextos | `None`, `One("R")`, `Selection(3, 2)`, `NoIdentity` y `Diagnostic` → textos deterministas; Uno(R) muestra exactamente R; Selección se rotula como selección, no como total | fallan por aserción |
| Estado Error por excepción contenida | un puerto que lanza en `GetSnapshot` o un delegado que lanza → no se propaga, Error con mensaje, reintentar recupera | fallan por aserción (el esqueleto no tiene estado Error) |
| El estado visible no crea trabajo sobre el DWG | puerto que cuenta llamadas: refrescar solo llama a `GetSnapshot` y `GetMeasurementReport`; oculto → cero llamadas por ciclo; mostrar → un refresco; nunca `Authorize` ni `GetSelectionContext` | fallan por aserción |
| Recursos sin `App.xaml` | `WorkspacePanelView` construido en STA (`StaTestRunner`) resuelve un recurso con clave de `AppStyles.xaml` desde sus propios recursos y carga su contenido | falla por aserción (el esqueleto no fusiona recursos) |
| UI sin Autodesk | fuentes de `src/RackCad.UI/Workspace/` sin `Autodesk`, `RackCad.Plugin`, `PaletteSet` ni manejadores globales de excepciones | pasa ya en el RED si el esqueleto es limpio: es una guarda, no cuenta como RED |
| Instrumentación | informe activado y desactivado → puntos y memoria presentados; el interruptor llama al delegado | fallan por aserción |

**Pruebas de Core** (texto de las fuentes, `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs`):
- la estructura del host de la paleta: holder perezoso; un único `new PaletteSet(` sin `Guid`; sin `Load`/`Save`; `AddVisual` una vez con
  `WorkspacePanelView`; manejador privado de `StateChanged` contenido que llama a `ShowPanel`/`HidePanel`; `KeepFocus` según C-T3B-05; sin
  `Internal` ni `Dispatcher`; sin acceso al puente, al núcleo, al lector ni al registro;
- el delta del comando: muestra la paleta tras `OpenPanel()` y el mensaje ya no dice que la interfaz no se muestra;
- G1 y G2 en `WorkspaceHostSourceGuardTests.cs` entran en el commit RED para que las 486 existentes sigan en verde.
- **RED esperado:** fallan por aserción las guardas nuevas que exigen la estructura de la paleta y el delta del comando; las 486 existentes
  pasan.

**Fuera de los filtros:** censos, guardas globales de UI y de espacios de nombres; los ejecuta la suite completa de la CI (sin filtro).

## 9. Enrutamiento (routing.md §§1-5 en `bb0d5522`)

- **Clase:** «Implementación transversal a capas», perfil corto (ROUTINE_IMPLEMENTATION), effort semántico de partida **Deep**. Cruza
  `RackCad.UI` (control raíz y view-model), el Plugin (host de la paleta y el delta de `RACKPANEL`), `RackCad.UI.Tests` y las guardas de
  `RackCad.Tests`.
- **Dimensiones (§2):**

  | Dimensión | Valor | Motivo |
  |---|---|---|
  | Ambigüedad | Medium | la mecánica de foco no está congelada y el disparador de refresco depende de C-T3B-02; con las decisiones tomadas, el resto está fijado |
  | Sensibilidad arquitectónica | High | frontera UI/AutoCAD (ADR-0006), host único por proceso y contención dentro del proceso de AutoCAD |
  | Amplitud | High | dos proyectos de producción y dos de pruebas |
  | Uso de herramientas | High | builds Release (Plugin incluido), dos suites, Git |
  | Coste de fallo | High | un fallo de la paleta afectaría la sesión de AutoCAD del Owner en Smoke-1 |
  | Repetición mecánica | Low | — |
  | Horizonte | Medium | T3A: 30 archivos en unos 54 min con `claude-sonnet-5-5`/`high`; T3B se estima en 10 a 18 archivos según C-T3B-02 |

- **Effort:** Deep. La regla 1 sube un escalón con tope en Deep, y la clase ya parte de Deep. Horizonte Medium: **no es Long-horizon**. Si
  el Coordinator eligiera una opción de C-T3B-02 que reabra también el puente y sus pruebas, el horizonte seguiría en Medium (de 4 a 6
  archivos más), pero conviene reevaluarlo antes de emitir.
- **Nivel:** Deep → Equilibrado o Frontera. Ninguna celda Frontera tiene `write-commit-push` medido (catálogo: `claude-opus-5-5` y
  `claude-fable-5-1` «no medido»), así que la celda adecuada más baja es Equilibrado.
- **Comprobación fresca de elegibilidad (2026-10-11):**
  - `docs/automation/agent-execution/routing.md` y `model-catalog.md` sin cambios en la rama desde `bb0d5522` (último cambio `5cda24f3`,
    2026-09-30).
  - Catálogo verificado el 2026-09-30: `STALE` el 2026-12-29, así que ninguna entrada usada está caducada.
  - **Worker `claude-sonnet-5-5` | subagente | `high`:** `write-commit-push` medido en la sonda U-04 de I-61 (2026-10-01; `medium` y `high`);
    consumo cubierto por suscripción; usado como Worker real en T2 (2026-10-10) y en T3A (2026-10-11: 175 respuestas con
    `claude-sonnet-5-5`/`high`, 54 min, sin aviso de límite ni denegaciones). Elegible.
  - **Controller `gpt-6-luna` | Codex CLI | `high` | read-only:** sonda `R20261011T035128Z-515d` PASS con el CLI `0.162.0-alpha.17.2`; en
    T3A, 8 invocaciones (dos planificaciones, verificación, nc1, nc2 y tres nc3) sin aviso de límite. El CLI instalado hoy sigue siendo el
    único `codex.exe` en `bin/2e5e00daee91c61d` (`codex-cli 0.162.0-alpha.17.2`); los otros tres directorios de `bin` son `node_repl`,
    `node` y `rg`. Elegible, con la deuda de protocolo conocida de nc2 (Scope) y nc3 (FreeText), cubierta por A-4 y A-6.
- **Celdas propuestas:** Worker `claude-sonnet-5-5`/subagente/`high`/`write-commit-push`; Controller `gpt-6-luna`/`high`/read-only con Codex
  CLI `0.162.0-alpha.17.2`; `RoutingEnforcement` = `required`. Ninguna se da por elegible solo por haberse usado en T3A: las dos se
  comprueban de nuevo antes de cada invocación (`config.toml` en A/B/C con los 105 nombres; CLI y modelo efectivos en el relevo).

## 10. Presupuesto, `attempts` y controles

- **TaskId nuevo:** `F1-T3B-HOST-UI`. `ChainRedSha` = null y `ChainRedFiles` = [], así que la primera entrega **exige un RED de
  comportamiento real** (16.8).
- **Unidad:** `attempts` = 3 y `max_attempts` = 3, sin reinicio ni aumento. Propuesta para la primera delegación: **`Attempt` 3,
  `AttemptsRemaining` 0, `MaxReworkLoops` 3, `CorrectionsAuthorized` false.** Cualquier resultado válido que exija un Worker de corrección
  da **STOP S-11**, salvo autoridad nueva.
- **Presupuesto de invocaciones propuesto por `TaskId`** (mismo modelo acotado que T3A; lo emite el Coordinator):

  | Fase | Tope |
  |---|---|
  | Planificación | 1 salida válida + hasta 2 invocaciones adicionales solo por transporte o INVALID_OUTPUT |
  | Trabajo | 1 salida válida + hasta 2 adicionales solo por transporte o INVALID_OUTPUT |
  | Verificación | 1 salida válida + hasta 2 adicionales solo por transporte o INVALID_OUTPUT |
  | nc4 | sin invocación (REJECTED_BEFORE_INVOCATION) |
  | nc1, nc2, nc3 | 1 salida válida cada uno + hasta 2 adicionales solo por transporte o INVALID_OUTPUT |
  | I64-SCOPE-BRIDGE-01 (A-4) | 1 ejecución si el Controller nc2 no discrimina |
  | I64-FREETEXT-BRIDGE-01 (A-6) | 1 ejecución si el Controller nc3 se agota (P-04), con sus precondiciones |

  Una segunda salida válida de planificación, trabajo o verificación no se autoriza para mejorar la primera.
- **Riesgo conocido de nc3:** en T2 y en T3A el Controller convirtió el REWORK sintético en STOP por C-15/S-11 en los seis intentos. La
  revisión del cierre de T3A (§52) dice que la sesión debió devolver ese modo de fallo antes de clasificarlo. Para T3B se propone fijar la
  regla en el contrato (C-T3B-07), de modo que la clasificación no dependa de una analogía de la sesión.

## 11. STOP

S-01..S-14 y P-01..P-08 sin cambios, más estos (propuesta; numeración propia de T3B):

| ID | Condición |
|---|---|
| C-01 | `origin/main` cambia antes de escribir (S-13, 16.7) |
| C-02 | hay que tocar un archivo de producto de T1, T2 o T3A distinto de los deltas exactos que autorice C-T3B-01/C-T3B-02, o estos fuera de su delta; `PluginInitializer.cs`, un `csproj`, el `sln`, `Directory.Build.*`, un comando existente, `RackCommandSupport.cs`, `RackLog.cs`, `Themes/`, `Shell/` o cualquier archivo de `src/RackCad.UI` fuera de `Workspace/` |
| C-03 | se necesita AutoCAD, cargar el DLL, perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL o registro |
| C-04 | se necesita ampliar el alcance |
| C-05 | aparece una ambigüedad material del Freeze |
| C-06 | `config.toml` fuera del conjunto aceptado A/B/C o con otro inventario de nombres (P-01) |
| C-07 | conflicto de propiedad, archivo caliente (`docs/HANDOFF.md`, `docs/ROADMAP.md`, `docs/adr/README.md`) o código de I-52, I-62 o I-65 |
| C-08 | la tarea deriva hacia navegación, selección o vista (F2), pestañas (F3), borradores o Actualizar (F4), editores (F5), inventario (F6) o F1-T3C |
| C-09 | tipos o métricas de I-63 |
| C-10 | cambio observable en comandos existentes, RACKEDITAR, editores o ventanas existentes; un censo de ventanas o de comandos cambia |
| C-11 | escritura authored, NOD, XData, archivos, registro, perfil o estado persistido de la paleta |
| C-12 | uso de `Autodesk.AutoCAD.Internal`, `IKeyboardInputSink` propio o ganchos de teclado de Win32 |
| C-13 | presupuesto agotado |
| C-14 | una guarda de T1, T2 o T3A necesita un cambio distinto de las excepciones de archivo exacto autorizadas, o se excluye un directorio entero; se borra o debilita una prueba existente |
| C-15 | cualquier resultado válido del Worker o del verificador exige corrección, o una verificación válida devuelve REWORK (`CorrectionsAuthorized` false; `AttemptsRemaining` 0; S-11) |
| C-16 | `main` se mueve de forma que exige una reconciliación semántica no resuelta |
| C-17 | el control raíz o el view-model necesitan un tipo de AutoCAD, del Plugin o un cambio del puerto `IWorkspaceHostPort` |
| C-18 | la contención exigiría un manejador global de excepciones del proceso (`Dispatcher.UnhandledException`, `AppDomain.UnhandledException`) |
| C-T3-INSTRUMENTATION | presentar o activar la instrumentación exigiría escribir en el registro, en archivos o en la configuración, modificar `RackLog.cs` o crear una API global de registro |

## 12. Autoridades

- **`UNIT_DOC`, en `AuthorityRevision`:**
  - Freeze V5, discovery y contrato de la iniciativa;
  - A-2, A-4, A-5 y A-6;
  - evidencia §18 y §§46-52;
  - estado `docs/automation/state/I-64.yml`;
  - propuesta del contrato de F1, paquete monolítico de T3 (solo como antecedente), este paquete;
  - `coordinator-decision-t3-decomposition.md`, `coordinator-scope-correction.md`, `coordinator-close-review.md` y `task-close-package.md`
    de `docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/`;
  - ADR 0047 (propuesto, de la unidad).
- **`EXTERNAL`, en `MainSha`:** `AGENTS.md`, `docs/WORKFLOW.md`, `docs/INITIATIVE_LIFECYCLE.md`, `docs/AUTOMATION_PLAN.md` §16,
  `docs/automation/agent-execution/README.md`, `routing.md`, `model-catalog.md`, ADR-0006 (AutoCAD solo en el Plugin), ADR-0019 (shell
  visual por composición) y ADR-0029 (contrato funcional común de ventanas WPF).
  - Las secciones de I-55 V5 (§4.4-§4.10) y el ADR-0044 que citaba T3A no aplican: T3B no abre transacciones ni lee vistas compartidas.
  - Ninguna ruta `EXTERNAL` cambió en la rama desde `bb0d5522`.
- **Sin `UNIT_CHANGE`.**

## 13. Coordinación (solo lectura, 2026-10-11, tras `git fetch --all`)

| Rama | Punta | Base | Solape con T3B |
|---|---|---|---|
| `origin/main` | `bb0d5522` (2026-10-04) | — | — |
| I-64 `architecture/workspace-persistente-rackcad` | `ee8018e0` = HEAD | `bb0d5522` | — |
| I-65 `architecture/interop-analisis-estructural` | `16fa9e7649e139798d41188db5bbe8b3be94e086` (igual a la observada por el Coordinator) | `bb0d5522` | ninguno: 151 archivos, en `StructuralAnalysis/`, `Systems/Cantilever/`, `StructuralSections/StructuralSectionUnits.cs`, `Units/`, `assets/structural-analysis/` y sus documentos. No se consume |
| I-62 `architecture/portabilidad-coordinador-principal` | `4b76383161e1404fcad2eeffd673908e0881207f` | `bb0d5522` | ninguno: en `src/` y `tests/` solo `tests/RackCad.Tests/I62/` y `tests/RackCad.Tests/I62F4*Tests.cs`. La deuda de protocolo sigue siendo externa al producto |
| I-52 `feature/rackmirror-espejo-semantico` | `fb6b56483a421e84c4bc44dd0aa1761813b7e457` (2026-10-02) | `95690c28` (main anterior) | ninguno: no cambia `src/RackCad.UI`, `src/RackCad.Plugin`, `tests/RackCad.UI.Tests` ni los censos de comandos. Su RELEASE de host no hace falta para T3B (estático), pero sí para Smoke-1 |

- Ninguna rama toca las rutas candidatas de T3B (`src/RackCad.UI/Workspace/`, `src/RackCad.Plugin/Workspace/`, las pruebas `Workspace*`,
  `Themes/`, `Shell/`, `PluginInitializer.cs`, los censos ni los `csproj`).
- **Riesgo de integración, no de T3B:** T3A llevó el censo de comandos a 38 (`RACKPANEL`). Si otra iniciativa añade un comando antes de
  integrar I-64, habrá que reconciliar el total al integrar; hoy ninguna de las tres ramas toca los archivos del censo.

## 14. Decisiones del Coordinator (STOP COORDINATOR_T3B_GATE_CONTRACT_REQUIRED)

| ID | Decisión | Opciones | Recomendación |
|---|---|---|---|
| C-T3B-01 | Reabrir `WorkspacePanelCommand.cs` (producto de T3A) con un delta exacto | autorizar el delta; o no autorizar, y entonces T3B no puede mostrar la paleta (cualquier otro punto de entrada exigiría un comando nuevo y cambiaría el censo) | **autorizar** el delta de la sección 5 |
| C-T3B-02 | Cómo se entera el panel de los cambios (drenaje y cambio de documento activo, D-03) | **A:** encolar en la activación o eventos en el puente (revoca la regla de la corrección de alcance y reabre T2/T3A con sus guardas); **B:** evento aditivo en `WorkspaceController` (reabre otro archivo de T3A y no cubre el cambio de documento sin pistas); **C:** refresco en memoria del view-model con un temporizador del dispatcher solo mientras la paleta está visible | **C**: cumple D-03 con una latencia acotada, no añade suscripciones de AutoCAD, no reabre más T3A y pasa todas las guardas. Exige que el Coordinator confirme que releer estado en memoria no infringe P-02 (cuya lista es resolver, redibujo, `Regen`, recálculo, índice y lectura del DWG) y fije el intervalo (propuesta: 500 ms) |
| C-T3B-03 | Refinar dos guardas de T3A (G1, G2) con cambios exactos | autorizar; o no, y entonces la paleta no puede vivir en `Plugin/Workspace/` y la UI no puede consumir `IWorkspaceHostPort` | **autorizar** G1 y G2 tal como están en la sección 5 |
| C-T3B-04 | Instrumentación | solo presentación; o presentación + interruptor mediante un delegado inyectado por el host (sin cambiar el puerto) | **presentación + interruptor por delegado**: el interruptor de T3A no tiene hoy ningún punto de uso |
| C-T3B-05 | Mecánica de foco (no congelada) | `PaletteSet.KeepFocus` = true; el valor por defecto; o diferir a Smoke-1 | **`KeepFocus` = true**: API documentada, coherente con la obligación congelada de D-01 (ninguna tecla llega a AutoCAD con el foco en el panel; el foco vuelve al dibujo solo por acción explícita). Se comprueba en Smoke-1 (S1-06a..h) |
| C-T3B-06 | Nombre visible de la paleta y de su pestaña | lo elige el Owner (D-01); no bloquea | propuesta «RackCad» / «Trabajo», modificable sin cambio de contrato |
| C-T3B-07 | Clasificación de nc3 fijada de antemano | regla explícita en el contrato: si el Controller detecta FreeText como única diferencia y convierte el REWORK sintético en STOP por C-15/S-11 → INVALID_OUTPUT y reintento, hasta P-04 y A-6; o STOP al Coordinator en el primer caso | **fijar la regla en el contrato**, tal como la aceptó la revisión de T3A (§52), para que la sesión no la aplique por analogía |
| C-T3B-08 | `RequiredTests` | dos proyectos con `ExpectRed` true y mínimos 1 (UI) y 487 (Core) | **aceptar**; la planificación fija los recuentos reales |
| C-T3B-09 | Confirmar lo que queda fuera de T3B | ayuda de `RACKPANEL` (censo de ayuda), conexión de `SetModalRackCadActive` con los modales existentes (reabriría comandos clásicos, M-03) y liberación de la paleta en `Terminate()` (prohibida por la guarda de `PluginInitializer`) | **confirmar fuera de T3B** y registrarlo para F1-T3C o para gates posteriores |

**Propuesta de cabecera del contrato (no emitido):** `TaskId` F1-T3B-HOST-UI; `Gate` F1; `AuthorityRevision` = el commit que versione la
decisión del Coordinator sobre este paquete (patrón X/Y de T3A); `MainSha` = `origin/main` en la emisión (hoy `bb0d5522`; si cambia,
S-13/16.7); `CorrectionOf` null; `Attempt` 3, `AttemptsRemaining` 0, `MaxReworkLoops` 3, `CorrectionsAuthorized` false;
`RoutingEnforcement` required; celdas de la sección 9.

**Siguiente:** STOP en **COORDINATOR_T3B_GATE_CONTRACT_REQUIRED**. Sin planificación ni Worker de T3B hasta que el Coordinator decida
C-T3B-01..C-T3B-09 y emita el contrato.

## 15. Versión para la emisión (decisión del Coordinator, §54)

### 15.1 Decisiones resueltas

| ID | Resolución del Coordinator |
|---|---|
| C-T3B-01 | **Autorizado:** delta mínimo de `WorkspacePanelCommand.cs` (obtener el controlador, `OpenPanel()`, crear u obtener el único `WorkspacePaletteHost`, mostrar la paleta); sin lógica de `PaletteSet` en el comando ni alias |
| C-T3B-02 | **Opción C rechazada** (ni `DispatcherTimer`, ni `System.Threading.Timer`, ni sondeo, ni `GetSnapshot` por paso del tiempo). **Relevo por eventos** sobre la suscripción existente a `DocumentActivated` del puente (15.2), con reapertura exacta de `AutoCadEventBridge.cs` y `WorkspaceController.cs` |
| C-T3B-03 | **Autorizados G1 y G2**, y **G3** (las dos suscripciones del controlador) |
| C-T3B-04 | Interruptor de instrumentación como **delegado neutral inyectado** por el host visual; `IWorkspaceHostPort` sin cambios; medidas refrescadas solo por notificación de estado o interacción explícita |
| C-T3B-05 | Solo mecanismos públicos (`PaletteSet.KeepFocus` o equivalente); ni `Internal`, ni `SetFocusToDwgView`, ni reflexión; verificación estática en T3B, foco real en Smoke-1 |
| C-T3B-06 | Título de la paleta «RackCad»; rótulo del control raíz «Workspace». No es una decisión de marca |
| C-T3B-07 | **Regla de nc3 explícita en el contrato** (15.6) |
| C-T3B-08 | **Mínimos aceptados:** UI 1, Core 487; los dos con `ExpectRed` |
| C-T3B-09 | **Fuera de T3B:** ayuda de `RACKPANEL`, indicador de modal en comandos clásicos, liberación en `Terminate()`, navegación, pestañas, borradores, escrituras authored, inventario, diff de perfil y Smoke-1 |

### 15.2 Diseño de refresco por eventos (exacto)

1. **`AutoCadEventBridge.cs`** (delta exacto):
   - declara **una** notificación en memoria `ActiveSessionChanged` (`Action` sin argumentos, sin tipos de AutoCAD);
   - la declara **`internal`** (interna del puente, visible solo dentro de `RackCad.Plugin`). Así la superficie **pública** que fija
     `WorkspaceBridgeFacadeSourceGuardTests.TheBridgePublicSurfaceIsTheT2OneAndTheFourFacadesAndNothingElse` (prueba de T3A, no
     autorizada) no cambia. Si la notificación tuviera que ser pública: STOP al Coordinator;
   - la disparan solo dos manejadores existentes, **dentro del mismo `Contain(...)` y después** de actualizar el núcleo:
     - `DocumentActivated`, tras `_core.OnDocumentActivated(...)`;
     - `DocumentToBeDestroyed`, tras `_core.OnDocumentToBeDestroyed(...)`, **solo si** `_core.ActiveSession` cambió (se compara antes y
       después);
   - `Start()` no la dispara; no hay `+=`/`-=` nuevos (16 y 16), ni líneas `handler = (` nuevas (16), ni cambios en `Hook`, `OnIdle`,
     `Dispose`, `BeginDocumentClose` ni en las fachadas; `EventBridgeCore.cs` no cambia y su `OnDocumentActivated` no encola nada.
2. **`WorkspaceController.cs`** (delta exacto):
   - se suscribe exactamente a `_bridge.Drained` (ya existía) y a `_bridge.ActiveSessionChanged`: dos `+=`, ningún `-=`, ninguna
     suscripción directa a AutoCAD;
   - expone **una** notificación en memoria pública `StateChanged` (`Action` sin argumentos, sin tipos de AutoCAD);
   - con `ActiveSessionChanged`: si `PanelVisible`, llama al `_host.RefreshPanel()` existente; después avisa con `StateChanged`. Con el
     panel oculto no pide reconciliación;
   - con `Drained`: conserva `_host.OnDrained(outcome)` y su contención y, después, avisa con `StateChanged`;
   - `OpenPanel`, `ShowPanel` y `HidePanel` avisan con `StateChanged` tras su transición;
   - cada aviso y cada manejador quedan contenidos (`try`/`catch` con `LogFault`); sin hilos, temporizadores ni persistencia.
3. **`WorkspacePaletteHost.cs`** (archivo nuevo exacto):
   - posee la paleta perezosa única (`WorkspaceLazyInstance`), construida sin `toolID` y sin `Load`/`Save`, título «RackCad», cerrar =
     ocultar, `KeepFocus` o equivalente público;
   - crea el control raíz de `RackCad.UI.Workspace` con el puerto (`controller.Port`) y el delegado del interruptor
     (`controller.SetInstrumentationEnabled`), y lo aloja con un único `AddVisual` rotulado «Workspace»;
   - sincroniza mostrar/ocultar de la paleta con `ShowPanel`/`HidePanel` del controlador (manejador privado y contenido);
   - se suscribe a `controller.StateChanged` y entonces pide al view-model que **consulte** `GetSnapshot()` y, si hace falta,
     `GetMeasurementReport()`;
   - contiene toda excepción; sin transacciones ni lectura o escritura del DWG; sin temporizador.
4. **Ciclo:**
   - **oculto:** una activación de documento solo produce un aviso en memoria, sin reconciliación ni lectura del DWG;
   - **visible:** la activación pide una reconciliación, el drenaje normal la ejecuta en el siguiente reposo, `Drained` avisa y el panel
     consulta. Después el reposo vuelve a cero trabajo hasta una nueva pista o evento de estado. Sin `StateChanged` no hay consultas.

### 15.3 `AllowedWriteScope` (8 entradas, las de la decisión)

- `src/RackCad.UI/Workspace/` (solo archivos nuevos)
- `tests/RackCad.UI.Tests/Workspace/` (solo archivos nuevos)
- `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` (nuevo exacto)
- `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs` (delta mínimo de 15.1)
- `src/RackCad.Plugin/Workspace/WorkspaceController.cs` (relevo de 15.2.2)
- `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` (notificación de 15.2.1)
- `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs` (nuevo exacto)
- `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs` (solo G1/G2/G3)

**`ForbiddenWriteScope`:** la lista de la sección 6 sin `src/RackCad.Plugin/Workspace/WorkspaceController.cs` ni
`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` (146 entradas). Cualquier otro archivo exacto indispensable: STOP al Coordinator.

### 15.4 Deltas exactos de las guardas de T3A (`WorkspaceHostSourceGuardTests.cs`)

| Guarda | Delta |
|---|---|
| G1 `NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType` | para `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` y solo para él, la regla admite `PaletteSet`, `AddVisual`, `Autodesk.AutoCAD.Windows`, `RackCad.UI` y los tipos públicos de host WPF necesarios, y sigue prohibiendo `Autodesk\.AutoCAD\.Internal`, cualquier `Dispatcher` (sondeo) y `Timer`; el trabajo asíncrono o en segundo plano y la lectura o escritura del DWG siguen prohibidos por las demás guardas, que no cambian. Los demás archivos de las dos carpetas conservan la regla completa |
| G2 `NothingOutsideThePluginWorkspaceReachesTheController` | la búsqueda pasa a identificadores completos: `\bWorkspaceController\b\|\bWorkspaceHostPort\b\|\bWorkspaceSelectionReader\b\|\bWorkspaceHostCore\b`. `IWorkspaceHostPort` deja de coincidir y queda permitido fuera; el controlador, el puerto concreto, el lector y el núcleo siguen prohibidos |
| G3 `TheControllerSubscribesOnlyToTheDrainedEventOfTheBridge` | exactamente dos `+=` en `WorkspaceController.cs`: `_bridge.Drained +=` y `_bridge.ActiveSessionChanged +=`; ningún `-=` |

Ninguna prueba se borra ni se salta; sin exclusiones por carpeta. Las demás pruebas de T1, T2 y T3A no cambian, entre ellas
`WorkspaceBridgeFacadeSourceGuardTests`, `WorkspaceBridgeSourceGuardTests` y `WorkspaceSourceGuardExceptionTests`.

### 15.5 Invariantes y pruebas

Las invariantes del contrato (INV-F1-T3B-01..16) sustituyen a las de la sección 7. Sobre la sección 8, cambian dos áreas:
- «el estado visible no crea trabajo sobre el DWG»: sin notificación no hay consultas; con una notificación, una consulta;
- se añaden las guardas de fuente del relevo: la notificación interna del puente y las dos suscripciones del controlador.

Mínimos: UI `FullyQualifiedName~RackCad.UI.Tests.Workspace` ≥ 1 y Core `FullyQualifiedName~RackCad.Tests.Workspace` ≥ 487, los dos con
`ExpectRed`. Los dos RED compilan, fallan por aserción, ejercitan comportamiento real de T3B y no son artificiales; el GREEN conserva la
cadena RED.

### 15.6 Regla de nc3 (explícita en el contrato de T3B)

- **Oráculo:** FreeText = fail, FailureClass = FreeText, EXECUTION_REWORK_REQUIRED / REWORK.
- **Salida inválida:** si el Controller detecta la violación, informa FailureClass FreeText y conserva las demás relaciones, pero
  convierte el REWORK sintético en STOP solo por aplicar `AttemptsRemaining`, `CorrectionsAuthorized` o S-11 de la tarea real →
  INVALID_OUTPUT, no PASS.
- **Agotamiento:** dos reintentos normales; si se agotan en el mismo modo: Controller nc3 = NOT PASSED / P-04, y A-6 solo con todas sus
  precondiciones.
- **Alcance:** no se extiende por analogía a otras tareas.

### 15.7 Enrutamiento, presupuesto y orden

- **Enrutamiento:** sin cambios respecto de la sección 9: clase, Deep, Worker `claude-sonnet-5-5`/subagente/`high`, Controller
  `gpt-6-luna`/`high`/read-only con CLI `0.162.0-alpha.17.2`, `RoutingEnforcement` required. El relevo por eventos añade dos archivos
  pequeños al alcance; el horizonte sigue en Medium.
- **Presupuesto:** sección 10. `Attempt` 3, `AttemptsRemaining` 0, `CorrectionsAuthorized` false, `ChainRedSha` null.
- **Revisiones:**
  - **`AuthorityRevision`:** `c7f389cb`, el commit de la decisión.
  - **`BaseSha`:** el commit que versiona esta revisión del paquete (el siguiente).
  - **Por qué difieren:** 16.4 exige el árbol limpio antes de ceder el worktree al Controller; 16.3 admite una `AuthorityRevision`
    ancestro de `BaseSha`, y el diff entre ambos no contiene rutas `EXTERNAL`.
  - **Este paquete no es autoridad del contrato:** en `c7f389cb` todavía es la versión previa a la decisión; la autoridad es la decisión
    literal.
