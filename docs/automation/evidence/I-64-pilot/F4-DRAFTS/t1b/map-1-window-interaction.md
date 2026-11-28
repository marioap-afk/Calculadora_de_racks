# Mapa 1 — Interacción de `RackPushBackSystemWindow` (F4-T1B-PILOT-CHARACTERIZATION, I-64)

- Commit leído: `c3ae1c970c908ef541840fc583680b86376cae0b` (lectura pura; no se compiló ni se ejecutó nada).
- Archivos principales: `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` (792 líneas),
  `RackPushBackSystemWindow.xaml.cs` (3630), `RackPushBackSystemWindow.Composite.cs` (911).
- Shell y apoyo leídos en lo que determina el comportamiento: `src/RackCad.UI/Shell/RackEditorVisualShell.cs`,
  `Shell/EditorActionBar.cs`, `Shell/EditorClosePolicy.cs`, `Shell/ShellResources.cs`, `Themes/Generic.xaml`,
  `Themes/AppStyles.xaml`, `Controls/NumericField.cs`, `Controls/CatalogCombo.cs`, `Controls/DialogWindowChrome.cs`,
  `UiSupport.cs`, `Editor/RackEditorSession.cs`, `Editor/EditorModules.cs`, y los subdiálogos.
- Convención: «medido» = leído en el código citado. «Inferencia WPF» = comportamiento por defecto del framework que el
  repositorio no sobrescribe; NO está medido y una prueba debe confirmarlo antes de fijarlo.

---

## 1. Enter

- **H-1.** La ventana no declara ningún botón `IsDefault`. Búsqueda: `IsDefault` en los tres archivos de la ventana →
  0 coincidencias. Los únicos botones del XAML son los 44 `<Button>` listados en §4; ninguno lleva `IsDefault`.
  No hay comentario en la ventana que lo prohíba (la prohibición documentada es del Selectivo, no de Push Back).
- **H-2.** La caracterización vigente ya fija «Push Back no tiene acción por defecto»:
  `tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs:106` (`Assert.Null(Default(PushBack()))`), con el comentario
  «Push Back además no maneja Enter en ningún sitio» (`:105`).
- **H-3.** No hay manejadores de teclado en la ventana. Búsqueda en los tres archivos de
  `KeyDown|PreviewKeyDown|Key\.|InputBindings|KeyBinding|CommandBindings` → la única coincidencia relacionada con el
  teclado es la lectura de modificadores para Ctrl+clic en la matriz
  (`src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:989`). En todo `src/` no existe
  `RegisterClassHandler`, `InputBindings`, `KeyBinding`, `CommandBindings` ni `AccessKeyManager` (búsqueda global).
- **H-4.** `NumericField` es un `TextBox` sin manejo de teclas (`src/RackCad.UI/Controls/NumericField.cs:21`); solo
  sobrescribe `OnTextChanged`, que revalida en cada pulsación (`:138-142` → `Revalidate` `:147-163`, que actualiza
  `Value`, `HasError`, `ErrorMessage` y el borde rojo). Enter dentro de un `NumericField` **no compromete** nada: no hay
  `IsDefault` que active (H-1), el `TextBox` no tiene `AcceptsReturn` y la ventana no escucha teclas.
- **H-5.** El compromiso al modelo de casi todos los `NumericField` es `LostFocus`:
  `LostFocus="Input_Changed"` (p. ej. `RackPushBackSystemWindow.xaml:68,103,106,111,114,278,306,309,312,316,319,342,345,347,351,354`),
  `LostFocus="FrontCount_Changed"` (`:100`), `LostFocus="Gap_Changed"` (`:148`), `LostFocus="CellFondo_Changed"` (`:419`),
  `LostFocus="AdvancedRackParameter_Changed"` (`:492,499,510,524,531`), `LostFocus="ModuleLength_Changed"` (`:554`).
  `Input_Changed` solo pide recálculo (`RackPushBackSystemWindow.xaml.cs:1207`).
- **H-6.** `NumericField` sin manejador `LostFocus` (se leen solo al pulsar su botón o son espejos):
  `StructureOverrideBox` (`.xaml:235`, leído por `ApplyStructure_Click` en `RackPushBackSystemWindow.Composite.cs:242-257`),
  `DerivedPostLineHeightBox` (`.xaml:645`, leído por `ApplyDerivedPostLines_Click`), y los espejos deshabilitados
  `FrontBox`, `PalletHeightBox`, `WeightBox` (`.xaml:266,270,281`, `IsEnabled="False"`).
- **H-7.** `NameBox` (`TextBox`, `.xaml:47`) usa `TextChanged="Name_Changed"`: cada pulsación escribe el nombre en la
  identidad de la sesión de inmediato, sin recálculo (`.xaml.cs:1201-1205`). Enter en `NameBox` no hace nada.
- **H-8.** Inferencia WPF (no medida): Enter o Espacio sobre un `Button` enfocado lo pulsa; Enter en un `ComboBox`
  abierto confirma la selección. El repositorio no altera esos defectos.

## 2. Escape

- **H-9.** Exactamente un botón con `IsCancel`: `CloseButton` «Cerrar», en `TrailingActions`, con `Click="Close_Click"`
  (`RackPushBackSystemWindow.xaml:789`). Búsqueda `IsCancel` en la ventana → 1 coincidencia.
- **H-10.** `Close_Click` llama a `Close()` (`.xaml.cs:3381`). Escape llega al mismo `Click` por ser `IsCancel`; el
  comentario de `OnClosing` lo declara (`.xaml.cs:3383-3394`).
- **H-11.** `OnClosing` (`.xaml.cs:3395-3404`): si `!InsertRequested && !EditorClosePolicy.MayClose(PendingWork())`
  entonces `e.Cancel = true` y retorna; si no, `base.OnClosing(e)`.
- **H-12.** El único trabajo pendiente declarado es la edición de módulo escenificada (`.xaml.cs:3417-3419`):
  `EditorPendingWork.When(moduleOwner.ModuleSession != null && moduleOwner.ModuleSession.HasPendingChanges,
  "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?")`. Matriz, seguridad, topes y
  campos numéricos sin comprometer NO cuentan (comentario `.xaml.cs:3406-3416`, ADR-0029 D8).
- **H-13.** `EditorClosePolicy.MayClose` (`src/RackCad.UI/Shell/EditorClosePolicy.cs:108-112`) pregunta con
  `EditorDiscardPrompt.Confirm(pending.Question)` **sin owner**. El prompt de producción (`:76-80`) es
  `MessageBox.Show(question, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning)` (rama `owner == null`, `:78`);
  «Sí» (`MessageBoxResult.Yes`) = se puede cerrar; «No» = cierre cancelado.
- **H-14.** Escape con texto inválido o sin comprometer en un `NumericField`: el cierre no consulta la validez de los
  campos (no hay compuerta en `OnClosing`); ese texto no es trabajo pendiente (H-12). Que se dispare o no `LostFocus`
  durante el cierre es una inferencia WPF no medida (ver R-11).
- **H-15.** Inferencia WPF (no medida): Escape con el desplegable de un `ComboBox` abierto lo cierra y no llega al botón
  `IsCancel`.

## 3. Foco inicial

- **H-16.** `FocusManager.FocusedElement="{Binding ElementName=NameBox}"` en la raíz `Window`
  (`RackPushBackSystemWindow.xaml:9`). `NameBox` es el primer control del panel lateral (`.xaml:47`).
- **H-17.** No hay `Focus()`, `MoveFocus`, `Keyboard.Focus` ni manejador `Loaded`/`Activated`/`ContentRendered` en los tres
  archivos (búsqueda `\.Focus\(|MoveFocus|Keyboard\.Focus|Loaded|Activated|ContentRendered|SourceInitialized` → 0
  coincidencias de código).
- **H-18.** Contenido de `NameBox` al abrir: vacío en `LoadNew` (`.xaml.cs:272` → `LoadFromModel(inputs, string.Empty)`,
  que asigna `NameBox.Text` bajo `suppressSync` en `:382`); el nombre recibido en `LoadDesignForNew`/`LoadExisting`
  (`:298`, `:315`).
- **H-19.** El repositorio ya fija la DECLARACIÓN, no el foco efectivo: `tests/RackCad.UI.Tests/RichEditorContractTests.cs:35-53`
  busca el texto `FocusManager.FocusedElement` en el XAML; su criterio (foco efectivo = frágil) está escrito en `:58-60`.

## 4. Orden de tabulación

- **H-20.** Ni la ventana ni el shell fijan orden: búsqueda de `TabIndex|IsTabStop|KeyboardNavigation|Focusable` en los tres
  archivos de la ventana y en `src/RackCad.UI/Shell`, `Themes`, `Controls/NumericField.cs`, `Controls/CatalogCombo.cs`
  → 0 coincidencias. Fijado hoy por `RichEditorCharacterizationTests.cs:320-336` (`TabIndex == int.MaxValue` en todos los
  `Control` que encuentra el helper).
- **H-21.** El orden lo da, por tanto, el árbol visual. La plantilla del shell coloca los hijos del `DockPanel` en este orden
  de documento (`src/RackCad.UI/Themes/Generic.xaml:151-196`): 1) barra de acciones `PART_ActionBar` (`:154-158`,
  `DockPanel.Dock="Bottom"`), 2) banda de estado `PART_StatusHost` (`:161-163`), 3) barra lateral con
  `PART_SidebarScroll` (`ScrollViewer`, `:166-177`), 4) zona de trabajo: matriz `PART_MatrixHost` (`:190`) y preview
  `PART_PreviewHost` (`:193`). Consecuencia deducida: en orden de tabulación la **barra de acciones va antes que la barra
  lateral**, aunque se dibuje abajo.
- **H-22.** La barra de acciones es un `WrapPanel` con cuatro `ContentPresenter` en orden fijo Leading, Secondary, Primary,
  Trailing (`src/RackCad.UI/Shell/EditorActionBar.cs:15,24-33`).
- **H-23.** `RackEditorVisualShell` deriva de `Control` sin tocar `Focusable`/`IsTabStop` (`Shell/RackEditorVisualShell.cs:19-41`).
  Inferencia WPF (no medida): un `Control` y un `ScrollViewer` son enfocables y tab stop por defecto, así que el propio
  shell, `PART_SidebarScroll` y el `ScrollViewer` de la matriz (`.xaml:713`) podrían recibir foco con Tab.
- **H-24.** Controles generados por código en la matriz: por cada frente, un `TextBlock` «Frente N» con clic
  (`.xaml.cs:921-929`, no enfocable) y un `CheckBox` «En blanco» (`:934-947`, enfocable). Las tarjetas son `Border`
  con `MouseLeftButtonDown` (`:973-991`), no enfocables: **la selección de celda no es alcanzable con teclado** salvo por
  los combos `SelectedFrontBox`/`SelectedLevelBox` (`.xaml:705,707`; manejadores `.xaml.cs:1150-1160`).
- **H-25.** Contenedores colapsados al abrir (fuera del orden de tabulación): `CompositeSection` (`.xaml:135`;
  `RackPushBackSystemWindow.Composite.cs:65-71` lo colapsa y deshabilita si no hay lado B), `AdvancedModulesPanel`
  (`.xaml:480`), `FrontalSideBox` (`.xaml:775-776`; `Composite.cs:91`). `LateralSectionBox` es visible solo con la vista
  Lateral (`.xaml.cs:3057,3074`; al abrir `ViewBox.SelectedIndex = 0`, `:146`).
- **H-26.** Orden lógico deducido (orden de documento y del árbol visual; [x] = omitido al abrir; a confirmar por prueba, ver R-4):
  1. Shell (si es tab stop, H-23).
  2. Barra de acciones: `RestoreButton` → `BomButton` → `SaveLibraryButton` → `UpdateButton` [deshabilitado salvo RACKEDITAR]
     → `InsertLateralButton` → `InsertFrontalEntradaButton` → `InsertFrontalPosteriorButton` → [`FrontalSideBox`] →
     `InsertPlantaButton` → `InsertBatchButton` → `InsertButton` → `CloseButton` (`.xaml:749-789`).
  3. Estado: solo `TextBlock` (`.xaml:682-684`), nada enfocable.
  4. Lateral: `PART_SidebarScroll` (si es tab stop) → **Rack**: `NameBox` → **Anotaciones**: `NumberFrontsCheck`,
     `NumberLevelsCheck`, `DrawRackNameCheck`, `AnnotationScaleBox`, `DimensionsBox`, `DimensionsFrontalCheck`,
     `DimensionsLateralCheck`, `DimensionsPlantaCheck` → **Estructura global**: `FrontCountBox`, `ToleranceBox`,
     `BeamDepthBox` (columna 0), `PalletsDeepBox`, `PostPeralteBox` (columna 2), `PostBox` → **Compuesto**:
     `SideBPresentCheck` → [sección compuesta: `SideSelectorBox` … `RestoreStructureButton`] → **Tarima**: [`FrontBox`],
     [`PalletHeightBox`], `WeightUnitBox`, `DepthBox`, [`WeightBox`] → **Frente seleccionado**: `PositionsBox`,
     `FondosBox`, `FirstLevelHeightBox`, `LevelsBox`, `DepthStartBox`, `ApplyFrontDataButton`,
     `ApplySelectedFrontDataButton`, `ApplyAllFrontDataButton` → **Celda**: `CellPalletFrontBox`, `CellPalletWeightBox`,
     `CellClearBox`, `CellPalletHeightBox`, `CellBeamLengthOverrideBox`, `CellInOutBeamBox`, `CellIntermediateBeamBox`,
     `CellInOutPeralteBox`, `RearPeralteBox`, `CellIntermediatePeralteBox`, `ApplyCellButton` … `ApplyAllButton` →
     **Fondo y tarima**: `CellFondoOverrideBox`, `CellDrawPalletCheck`, `CellPropertyScopeBox`, `ApplyCellFondoButton`,
     `RestoreCellFondoButton`, `ApplyCellPalletButton`, `RestoreCellPalletButton` → **Seguridad**: `SafetyButton` →
     **Avanzado**: `AdvancedModulesToggle` → [panel avanzado completo, `.xaml:492-664`].
  5. Matriz: `AddFrontButton`, `RemoveFrontButton`, `AddLevelButton`, `RemoveLevelButton`, `SelectedFrontBox`,
     `SelectedLevelBox` (`.xaml:700-707`) → `ScrollViewer` (si es tab stop) → «En blanco» del Frente 1..N (H-24).
  6. Preview: `ViewBox` → `LateralSectionBox` (`.xaml:731,733`).
  7. Inferencia WPF: `Window` cicla (Cycle) y vuelve a 1. Con esto, Shift+Tab desde `NameBox` caería en
     `PART_SidebarScroll` o, si no es tab stop, en `CloseButton`.
- **H-27.** Inventario del XAML por tipo (búsqueda por etiqueta de apertura): 1 `TextBox`, 30 `NumericField`, 19 `ComboBox`
  + 1 `CatalogCombo`, 11 `CheckBox`, 2 `RadioButton`, 44 `Button`, 2 `ListBox`, 1 `ScrollViewer`. Más los N `CheckBox`
  «En blanco» generados por código.

## 5. Caminos de cierre

Contrato público que ve el llamador (todo derivado de la sesión, `.xaml.cs:247-256`): `InsertRequested`, `UpdateOnly`,
`RackId`, `RackName`, `InsertView`, `InsertSection`, `InsertionRequest`, `SystemToInsert`, `DesignToInsert`,
`SourceProjectToInsert`. Valores iniciales de la sesión: `InsertRequested=false`, `UpdateOnly=false`, `InsertView=null`,
`InsertSection=-1`, `InsertionRequest=null` (`src/RackCad.UI/Editor/RackEditorSession.cs:106-121`).

- **H-28.** La ventana **nunca asigna `DialogResult`** (búsqueda `DialogResult` en los tres archivos → 0) y no se suscribe a
  `InsertRequestedRaised` (búsqueda `+=` en los dos `.cs`: solo eventos de la matriz). Los llamadores ignoran el valor de
  `ShowDialog()`/`ShowModalWindow` y leen `InsertRequested`/`InsertionRequest`:
  `src/RackCad.UI/Editor/EditorModules.cs:115-116,125-126,130-131`; `src/RackCad.Plugin/RackPushBackCommands.cs:48-52,170-174`.
- **H-29.** No hay `OnClosed`, manejador `Closed`/`Unloaded` ni `Hide()` (búsqueda en los tres archivos → solo `OnClosing`).
- **Tabla de caminos** (todas las llamadas a `Close()` del producto: `.xaml.cs:3320`, `:3378`, `:3381`):

| Camino | Disparo | Qué hace antes de cerrar | ¿Pregunta? | Estado que deja |
|---|---|---|---|---|
| Cerrar | `CloseButton` → `Close_Click` (`.xaml:789`; `.xaml.cs:3381`) | nada | solo si hay módulo pendiente (H-11..H-13) | `InsertRequested=false`, `InsertionRequest=null` |
| Escape | `IsCancel` del mismo botón (H-9, H-10) | igual que Cerrar | igual | igual |
| X / Alt+F4 | WM_CLOSE → `OnClosing` (comentario `.xaml.cs:3384-3385`) | nada | igual | igual |
| Insertar vista actual | `Insert_Click` (`.xaml.cs:3205-3209`) → `RequestDraw(view, section, false)` | confirma módulo pendiente (`:3346-3349`), recalcula (`:3351`), guardas (`:3325-3363`), `SetName`/`SetModel` (`:3365-3366`), `RequestInsert` (`:3374-3375`) | no (`InsertRequested` ya es true) | `InsertRequested=true`, `UpdateOnly=false`, `InsertView`/`InsertSection` de la vista elegida (`SelectedView`, `:3084-3097`), `InsertionRequest=PushBackInsertionRequest` |
| Insertar lateral / frontal E/S / frontal posterior / planta | `InsertViewAt(0..3)` (`:3214,3226-3230,3251,3253-3262`) | fija `ViewBox.SelectedIndex` y sigue por `RequestDraw` | no | como Insertar, con la vista del botón |
| Actualizar | `Update_Click` → `RequestDraw(null, -1, true)` (`:3292`) | igual que Insertar + `RequestUpdate` (`:3369-3370`) | no | `InsertRequested=true`, `UpdateOnly=true`, `InsertView=null`, `InsertSection=-1` (`RackEditorSession.cs:186-193`) |
| Insertar varias… | `InsertBatch_Click` (`:3294-3321`) | confirma módulo pendiente ANTES del diálogo (`:3296`), recalcula, guardas, abre `RackViewBatchDialog`; si se cancela, `return` sin cerrar (`:3311`); si se acepta, `RequestInsertViews` (`:3314-3319`) y `Close()` (`:3320`) | no | `InsertRequested=true`, `UpdateOnly=false`, `InsertView=null`, `InsertSection=-1`, `InsertViews` ordenadas (`RackEditorSession.cs:163-168`) |

- **H-30.** Guardas de `RequestDraw` que NO cierran y escriben en `StatusText` (rojo vía `UiSupport.SetStatus`, `UiSupport.cs:26-31`):
  `!canInsertInAutoCad` → «El dibujo en AutoCAD solo está disponible cuando la ventana se abre desde AutoCAD.» (`.xaml.cs:3325-3329`);
  `updateOnly && !isEditingExisting` → «Solo un sistema abierto con RACKEDITAR puede actualizarse en sitio.» (`:3331-3335`);
  entradas inválidas → «Corrige los datos: no se puede insertar un modelo inválido.» (`:3352-3356`);
  salida bloqueada → diagnóstico bloqueante o «Corrige los datos: no se puede dibujar un modelo bloqueado.» (`:3360-3363`, `:3512-3521`).
- **H-31.** `InsertBatch_Click` **no comprueba `canInsertInAutoCad`** (`.xaml.cs:3294-3321`), y `InsertBatchButton` no
  aparece en `UpdateButtons` (`:3523-3570`; búsqueda `InsertBatchButton` en el `.cs` → 0): está habilitado siempre,
  también con `canInsertInAutoCad=false` (constructor sin parámetros, `:123-126`).
- **H-32.** Política con pendientes (H-11..H-13): «Sí» → la ventana se cierra y la edición de módulo escenificada se pierde
  con ella (no se confirma); «No» → `e.Cancel = true`, la ventana sigue abierta y la edición sigue pendiente
  (`ConfirmModuleButton`/`CancelModuleButton` habilitados y «Cambios pendientes: confirma o cancela.», `.xaml.cs:2512-2517`).
  Insertar/Actualizar/Insertar varias confirman primero la edición pendiente (`:3346-3349`, `:3296`), así que nunca la
  descartan; además su cierre no pregunta porque `InsertRequested` ya es `true` (`:3397`).
- **H-33.** Cierre tras error: ningún camino de error cierra. `Recompute` captura excepciones del armado y deja la ventana
  abierta con «No se pudo generar el sistema: …» (`.xaml.cs:648-656`); `SaveLibrary_Click` captura y muestra
  «No se pudo guardar: …» (`:3471-3474`). Guardar en biblioteca y BOM nunca cierran el editor (`:3423-3475`).
- **H-34.** `RackName` refleja lo tecleado aunque se cancele: `Name_Changed` escribe en `session.Identity` en cada
  pulsación (`.xaml.cs:1201-1205`); `RackId` sigue sin asignar en un rack nuevo hasta un `EnsureId` (Insertar/Actualizar
  `RackEditorSession.cs:157,186`; guardar en biblioteca `.xaml.cs:3463`).
- **H-35.** Cobertura existente de cierre: sin pendientes no pregunta y no materializa nada
  (`tests/RackCad.UI.Tests/RichEditorCloseContractTests.cs:89-108,128-150`); `OnClosing` declarado
  (`:68-86`); `PendingWork()` vacío al abrir y el texto de la pregunta (`tests/RackCad.UI.Tests/EditorClosePolicyTests.cs:184-211`,
  que construye la pregunta por su cuenta, no la lee de la ventana con trabajo pendiente). **No existe** prueba de la ventana
  real con trabajo de módulo pendiente cerrando, ni del owner del prompt.
- **H-36.** Las pruebas antiguas `FiveOfTheSixCloseWithEscapeAndPushBackDoesNot`, `NoRichEditorInterceptsItsClosing`,
  `NeitherDeclaredScopeIsConsultedWhenTheWindowCloses` y `FiveDeclareAnInitialFocus…` están en `Skip = BaseEvidence`
  (`RichEditorCharacterizationTests.cs:114,138,197,288`) y describen el estado ANTERIOR a I-39B (Push Back sin `IsCancel`):
  no deben reactivarse.

## 6. Owner, ubicación y tamaño

### 6.1 La ventana

- **H-37.** `WindowStartupLocation="CenterOwner"` (`RackPushBackSystemWindow.xaml:8`). Ya fijado por
  `RichEditorCharacterizationTests.cs:276-286`.
- **H-38.** La ventana no asigna su propio `Owner` (búsqueda `Owner =` en los tres archivos: solo subdiálogos). Lo asigna el
  llamador: `EditorModules.cs:114,121` (`Owner = context.Owner`, ventana del menú o `null`) y `ShowDialog()` (`:115,125`);
  el Plugin NO asigna `Owner` WPF y usa `AcApplication.ShowModalWindow(window)` (`RackPushBackCommands.cs:47-48,168-170`).
- **H-39.** Tamaño por estilo, no por atributos: `Style="{DynamicResource EditorShellWindowStyle}"` (`.xaml:7`) →
  `Width=ShellInitialWidth` 1280, `Height=ShellInitialHeight` 720, `MinWidth=ShellMinWidth` 1120,
  `MinHeight=ShellMinHeight` 672, fondo `ShellWindowBackgroundBrush`, `Segoe UI` 12 (`src/RackCad.UI/Themes/AppStyles.xaml:41-51,165-173`).
  Ya fijado por `RichEditorCharacterizationTests.cs:235-255`.
- **H-40.** No se asignan `ShowInTaskbar`, `ResizeMode`, `SizeToContent`, `MaxWidth/MaxHeight`, `Topmost`, `Icon`,
  `WindowState`, `ShowActivated` (búsqueda en los tres archivos → 0; la única coincidencia de `WindowStyle` es el nombre
  `EditorShellWindowStyle`). Quedan los valores por defecto de WPF. Título: «Sistema Push Back - editor» (`.xaml:6`).

### 6.2 Subdiálogos que abre la ventana

| # | Diálogo (clase, archivo) | Abierto en | `Owner` | `WindowStartupLocation` | Modal | Seam de prueba |
|---|---|---|---|---|---|---|
| S-1 | Prompt de descarte (`MessageBox`, `Shell/EditorClosePolicy.cs:76-80`) | `OnClosing` (`.xaml.cs:3397`) vía `EditorClosePolicy.MayClose` (`EditorClosePolicy.cs:112`) | **ninguno** (`Confirm(question)` sin owner) | n/a (MessageBox Win32) | sí | `EditorDiscardPrompt.Substitute(Func<string,Window,bool>)` (`EditorClosePolicy.cs:65`) expone el owner |
| S-2 | `RackFrameConfiguratorWindow` (`RackFrames/RackFrameConfiguratorWindow.xaml(.cs)`) | `ShowHeaderConfigurator` (`.xaml.cs:2424-2447`) | `this` (`:2426`) | `CenterOwner` en su XAML (`RackFrameConfiguratorWindow.xaml:12`); 1320×820, mín 1160×560 (`:8-11`) | sí (`ShowDialog`, `:2443`) | `HeaderConfiguratorPresenter` (`:2410`) entrega la ventana REAL; `HeaderConfiguratorDialog` (`:1525`) la sustituye |
| S-3 | `SelectiveSafetyWindow` (`SelectiveSafetyWindow.cs:21`) | `ShowSafetyDialog` (`.xaml.cs:2993-3020`) desde `Safety_Click` (`:2534`) | `this` (`:3016`) | `CenterOwner` (`SelectiveSafetyWindow.cs:200`); 480×540, mín 400×300 (`:196-199`); chrome `DialogWindowChrome.Apply` (`:203`) | sí (`:3018`) | `SafetyWindowDialog` (`:3028`) entrega la ventana REAL; `SafetyDialog` (`:1493`) la sustituye |
| S-4 | `SafetyPerPostWindow` (`SelectiveSafetyWindow.cs:1028`) — botas por poste | `OpenBootPerPostDialog` (`.xaml.cs:2707-2735`), invocado desde `PushBackBootSection.Configure` (`PushBackBootSection.cs:279-281`) **mientras S-3 está abierto** | `this` = ventana Push Back, **no** S-3 (`.xaml.cs:2721`) | `CenterOwner` (`SelectiveSafetyWindow.cs:1079`); 340×460 (`:1075-1078`) | sí (`:2722`) | `BootPerPostWindowDialog` (`:2700`) entrega la ventana REAL |
| S-5 | `SafetyDefensaGridWindow` (`SafetyDefensaGridWindow.cs:14`) — defensa por lado | `ShowDefenseDialog` (`.xaml.cs:2852-2868`), desde `PushBackDefenseSection.Configure` (`PushBackDefenseSection.cs:205-207`) con S-3 abierto | `this` = ventana Push Back (`.xaml.cs:2865`) | `CenterOwner` (`SafetyDefensaGridWindow.cs:87`); 780/670×580 (`:83-86`) | sí (`:2867`) | `DefenseDialog` (`:1505`) SUSTITUYE la ventana (no se observa el owner) |
| S-6 | `SafetyTopeGridWindow` (`SafetyTopeGridWindow.cs:25`) — tope posterior por lado | `ShowRearTopeDialog` (`.xaml.cs:3031-3050`), desde `PushBackRearTopeSection.Configure` (`PushBackRearTopeSection.cs:152-154`) con S-3 abierto | `this` = ventana Push Back (`.xaml.cs:3047`) | `CenterOwner` (`SafetyTopeGridWindow.cs:89`); tamaño calculado (`:84-88`) | sí (`:3049`) | `RearTopeDialog` (`:1499`) SUSTITUYE la ventana |
| S-7 | `RackBomWindow` (`RackBomWindow.xaml(.cs)`) | `Bom_Click` (`.xaml.cs:3438`) | `this` | `CenterOwner` (`RackBomWindow.xaml:9`); 740×520, mín por tokens (`:5-8`) | sí | ninguno |
| S-8 | `RackViewBatchDialog` (`Views/RackViewBatchDialog.xaml(.cs)`) | `RackViewBatchDialogPresenter.TryShow(this, …)` (`.xaml.cs:3311`; `Views/RackViewBatchDialogPresenter.cs:52-63`) | `owner` = ventana Push Back (`RackViewBatchDialogPresenter.cs:59`) | `CenterOwner`, `CanResizeWithGrip`, 650×430 (`RackViewBatchDialog.xaml:4-5`) | sí (`:60`) | ninguno (estático) |
| S-9 | `Microsoft.Win32.SaveFileDialog` | `UiSupport.PromptSaveToLibrary(this, …)` (`.xaml.cs:3458`; `UiSupport.cs:64-82`) | `ShowDialog(owner)` con la ventana Push Back (`UiSupport.cs:81`) | n/a (Win32) | sí | ninguno |

- **H-41.** Subdiálogos de segundo nivel abiertos por S-3 (no por la ventana): llevan `Owner = this` de S-3
  (`SelectiveSafetyWindow.cs:587` por poste, `:602` tope, `:697` defensa, `:713` guía, `:750` desviador, `:768` parrilla),
  todos con `CenterOwner` (p. ej. `SafetyDesviadorGridWindow.cs:117`, `SafetyParrillaGridWindow.cs:85`). Contraste medido:
  los tres diálogos de las secciones propias de Push Back (S-4..S-6) toman como owner la ventana Push Back y no S-3.
- **H-42.** S-2 tiene su propio `OnClosing`, que pregunta con owner = el configurador (`RackFrameConfiguratorWindow.xaml.cs:182-196,210-219`).
  `ShowHeaderConfigurator` **ignora el resultado del diálogo** y devuelve `window.Configuration ?? copy` (`.xaml.cs:2446`),
  que no es nunca `null`; por eso cerrar el configurador por cualquier camino escenifica la configuración
  (`ConfigureModuleHeader_Click` `:1938-1946` → `StageHeaderConfiguration` `:2009-2019`).
- **H-43.** Botones de los subdiálogos: S-3 «Aceptar» `IsDefault` y «Cancelar» `IsCancel` (`SelectiveSafetyWindow.cs:220-222`;
  `DialogResult = true` solo en modal, `:1017-1020`); S-4 igual (`:1101-1103`, `:1164`); S-6 (`SafetyTopeGridWindow.cs:149-151`, `:254`);
  S-5 (`SafetyDefensaGridWindow.cs:116,124`, `:628,704`); S-7 «Cerrar» `IsCancel` + `Close_Click` (`RackBomWindow.xaml:160-164`;
  `RackBomWindow.xaml.cs:89-91`); S-8 «Cancelar» `IsCancel`, «Insertar cola» `IsDefault` (`RackViewBatchDialog.xaml:35-36`;
  `DialogResult = true` en `RackViewBatchDialog.xaml.cs:21-24`).
- **H-44.** Cancelar S-3 abandona todo el paso de Seguridad sin tocar selecciones ni topes y sin recalcular
  (`.xaml.cs:2613-2621`); aceptar aplica todo en una operación diferida (`:2626-2657`).

## 7. Recursos

- **H-45.** `Window.Resources` = un `ResourceDictionary` que fusiona `Source="/RackCad.UI;component/Themes/AppStyles.xaml"`
  (URI pack relativa, `RackPushBackSystemWindow.xaml:10-14`) y declara tres `Thickness` locales: `ActionGap` `0,0,6,4`,
  `ActionGroupGap` `0,0,16,4`, `ActionLast` `0,0,0,4` (`:20-22`).
- **H-46.** Claves usadas por la ventana (conteo en el XAML): `StaticResource` → `FieldLabel` 54, `SecondaryButtonStyle` 36,
  `SectionTitle` 13, `ActionGap` 6, `ActionLast` 4, `PrimaryButtonStyle` 4, `ActionGroupGap` 2, `FieldBox` 1.
  `DynamicResource` → `ShellTextPrimaryBrush` 18, `ShellTextSecondaryBrush` 11, `ShellBorderBrush` 4, `ShellSurfaceBrush` 3,
  `EditorShellWindowStyle` 1, `ShellPanelPadding` 1, `ShellPreviewBackgroundBrush` 1. El código no usa
  `FindResource`/`TryFindResource` (búsqueda en los `.cs` de la carpeta → 0).
- **H-47.** Colores literales fuera de tokens: en el XAML (`#7A8794`, `#F1F4F8`, `#D8DEE6`, `#1F2933`, p. ej. `.xaml:166,290-292`)
  y pinceles congelados de la matriz en código (`.xaml.cs:113-121`). Los cuatro botones de la barra de la matriz no tienen
  `Style` (aspecto WPF por defecto, `.xaml:700-703`).
- **H-48.** La plantilla del shell es el estilo implícito de `RackEditorVisualShell` en `Themes/Generic.xaml:139-202`, cargado
  como diccionario de tema por `ThemeInfo(None, SourceAssembly)` (`src/RackCad.UI/RackCad.UI.csproj:20-25`);
  `Generic.xaml` fusiona a su vez `AppStyles.xaml` (`:7-9`) y declara el convertidor `ShellSlotVisibility` (`:11`). Los tokens
  de la plantilla son `DynamicResource` (`ShellWindowBackgroundBrush`, `ShellFontSizeBase`, `ShellFontFamily`,
  `ShellZoneSpacing`, `ShellSidebarWidth`, `ShellPreviewMinHeight`).
- **H-49.** Respaldo del shell: en `OnApplyTemplate`, si `ShellZoneSpacing` no resuelve, fusiona `ShellResources.Shared` en sus
  propios recursos (`Shell/RackEditorVisualShell.cs:45-53`; carga por URI en `Shell/ShellResources.cs:31-34`). En Push Back
  el token resuelve contra el diccionario de la ventana (H-45), así que se espera que el shell NO fusione nada (deducido,
  a medir).
- **H-50.** Dependencias para resolver fuera de AutoCAD: hilo STA y una `Application` viva para las URI pack; la suite ya las
  da (`tests/RackCad.UI.Tests/StaTestRunner.cs:40-43`). La ventana no depende de `Application.Resources` (todo está en su
  propio diccionario). El catálogo se carga en el constructor (`RackEditorSession.cs:85` → `UiSupport.LoadCatalogSafe`,
  `UiSupport.cs:84-94`, que devuelve un catálogo vacío si faltan archivos); la suite copia los catálogos a la salida
  (`tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:33-36`).

## 8. Otros comportamientos observables (D13/D9)

- **H-51.** Estado de botones al abrir: lo fija `UpdateButtons` tras cada recálculo (`.xaml.cs:3523-3570`), que corre ya en
  el constructor (`LoadNew` `:262-281` → `Recompute` `:426` y `:280`). Con `canOutput = currentInputsAreValid && !outputIsBlocked`:
  `InsertButton` = `canInsertInAutoCad && canOutput`; `UpdateButton` = además `isEditingExisting`; las cuatro vistas copian
  `InsertButton` (`:3549-3553`); `BomButton` = `canOutput`; `SaveLibraryButton` = `currentInputsAreValid`;
  `RestoreButton` = `lastComputation != null`. `InsertBatchButton` y `CloseButton` no se tocan nunca (H-31).
- **H-52.** Tooltips del action bar reescritos en código (los del XAML solo valen hasta el primer recálculo): sin AutoCAD,
  Insertar/Actualizar/vistas = «Disponible solo cuando la ventana se abre desde AutoCAD.» (`.xaml.cs:3530-3534`); con AutoCAD,
  Insertar = bloqueado / «Inserta la vista seleccionada enlazada al sistema.» / «Corrige los campos numéricos marcados.»
  (`:3537-3541`), Actualizar = «Redibuja en sitio todas las vistas del sistema.» o «Disponible solo para un sistema abierto con
  RACKEDITAR.» (`:3542-3544`); BOM/biblioteca (`:3555-3564`); Restaurar (`:3566-3569`). `ToolTipService.ShowOnDisabled="True"`
  en todos los botones de acción salvo `CloseButton` (`.xaml:749-785`; ya fijado por `PushBackShellAdoptionTests.cs:85-91`).
  `CloseButton` no tiene tooltip (`.xaml:789`).
- **H-53.** Otros controles deshabilitados al abrir: espejos `FrontBox`/`PalletHeightBox`/`WeightBox` (`.xaml:266,270,281`);
  `ModuleCustomRadio` siempre (`.xaml:567`, `.xaml.cs:1846-1849`); `ConfirmModuleButton`/`CancelModuleButton` sin pendientes
  (`.xaml.cs:2513-2514`); `CopyHeaderFromBox/Button` sin origen personalizado (`:2399-2402`); la sección compuesta
  (`Composite.cs:68-69`). Un frente en blanco deshabilita con motivo los controles de nivel/celda (`.xaml.cs:492-529`,
  `SetBlankSensitive` `:535-550`).
- **H-54.** Sin mnemónicos: ningún `Content`/`Header` con `_` en el XAML ni en los `.cs` de la carpeta, y ningún `<Label>`
  (búsquedas → 0). El único atajo de teclado registrado es Escape vía `IsCancel` (H-9).
- **H-55.** Textos de estado al abrir: `StatusText` = «Vista recalculada.» o el primer aviso/bloqueo compuesto
  (`.xaml.cs:691`, `Composite.cs:764-776`); `GuidText` = «(se asigna al insertar)» (`.xaml:50`, `.xaml.cs:3572`); `PreviewHint`
  = «Vista previa técnica de la vista seleccionada.» (`:3115-3117`).
- **H-56.** La matriz se reconstruye entera (`Children.Clear()`) en cada recálculo válido y en cada mutación estructural
  (`.xaml.cs:669`, `:1187`, `:891-894`): los `CheckBox` «En blanco» se destruyen y se recrean, también el que tenga el foco.
- **H-57.** Validación que bloquea el recálculo: solo 21 campos (`AllNumericFields`, `.xaml.cs:728-734`); `GapBox`,
  `StructureOverrideBox`, los seis campos avanzados, `ModuleLengthBox` y `DerivedPostLineHeightBox` no participan. Un campo
  inválido de esos 21 bloquea cualquier recálculo, también uno disparado por un combo (`:611-619`), y la selección de celda
  (`:1054-1058`).

---

## Puntos que una prueba de caracterización puede fijar

Precondiciones comunes: `StaTestRunner.Run` (STA + `Application`, `tests/RackCad.UI.Tests/StaTestRunner.cs:40-43`);
catálogos copiados (H-50); constructor `new RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)`
(`.xaml.cs:128`) — el sin parámetros equivale a `canInsertInAutoCad=false` (`:123-126`); acceso `internal` por
`InternalsVisibleTo` (`src/RackCad.UI/RackCad.UI.csproj:12-16`). La suite instala un prompt que responde «sí» para todo el
proceso (`StaTestRunner.cs:57`): toda prueba que observe el prompt debe ir en `[Collection(EditorDiscardPromptCollection.Name)]`
y usar `EditorDiscardPrompt.Substitute`. Ventana sin mostrar salvo donde se indica.

| # | Aserción concreta | API observable | Precondición extra |
|---|---|---|---|
| P-1 | Ningún `Button` con `IsDefault` en la ventana, incluidos los generados por la matriz | recorrido del árbol tras `ApplyTemplate`/`UpdateLayout`; ampliar `RichEditorCharacterizationTests.cs:106` | mostrar o forzar plantilla para incluir el action bar |
| P-2 | Exactamente un `IsCancel`: `CloseButton`, contenido «Cerrar», habilitado, `ToolTip == null` | `FindName("CloseButton")`, `IsCancel`, `IsEnabled`, `ToolTip` | — |
| P-3 | Guarda de fuente: los tres archivos no contienen `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(`, `DialogResult` | lectura de fuente (idiom de `RichEditorContractTests.cs:21-31`) | raíz por `RackCad.sln` |
| P-4 | Enter no compromete: cambiar `Text` de un `NumericField` sin `LostFocus` no incrementa `RecomputePassesForTest` ni cambia `State`; `HasError` sí cambia en vivo; al elevar `LostFocus` incrementa | `RecomputePassesForTest` (`.xaml.cs:601`), `NumericField.HasError/Value`, `EditorWindowTestSupport.SetNumberAndCommit` (`tests/RackCad.UI.Tests/EditorWindowTestSupport.cs:102-108`) | — |
| P-5 | Foco inicial declarado = `NameBox` y vacío en `LoadNew` | `FocusManager.GetFocusedElement(window)` tras cargar (o la declaración en fuente, criterio `RichEditorContractTests.cs:58-60`); `NameBox.Text == ""` | para `GetFocusedElement`: ventana mostrada (resolución del binding) |
| P-6 | Orden de tabulación real desde `NameBox` (secuencia de `Keyboard.FocusedElement` tras `MoveFocus(new TraversalRequest(FocusNavigationDirection.Next))` repetido) y su ciclo; incluye si el shell y los `ScrollViewer` son paradas | `UIElement.MoveFocus`, `Keyboard.FocusedElement`, `KeyboardNavigation.GetIsTabStop`, `Focusable` | ventana mostrada y activada (R-3); no cambiar `WindowStartupLocation` antes de leerla (R-15) |
| P-7 | Propiedades de la ventana: `Owner == null` tras construir, `WindowStartupLocation.CenterOwner`, `ShowInTaskbar == true`, `ResizeMode.CanResize`, `SizeToContent.Manual`, `Title`, tamaños (ya en `RichEditorCharacterizationTests.cs:235-286`) | propiedades de `Window` | — |
| P-8 | Cierre sin pendientes: no pregunta, `InsertRequested == false`, `InsertionRequest == null`, `InsertView == null`, `InsertSection == -1`, `UpdateOnly == false`; `RackName` = lo tecleado en `NameBox` | contrato público (`.xaml.cs:247-256`) | ya parcialmente en `RichEditorCloseContractTests.cs:89-108` |
| P-9 | Cierre CON pendientes: tras crear trabajo (activar `AdvancedModulesToggle`, `SetNumberAndCommit("ModuleLengthBox", longitud+Δ)`), `PendingWork().HasPendingWork == true` y `Question` exacta; con `Substitute((q, owner) => …false)` → `Close()` pregunta UNA vez, `owner == null`, la ventana sigue abierta (sin `Closed`); con `true` → se cierra sin materializar | `PendingWork()` (`.xaml.cs:3417`), `EditorDiscardPrompt.Substitute(Func<string,Window,bool>)` (`EditorClosePolicy.cs:65`), evento `Closed` | `ModuleBox` con módulo seleccionado tras `LoadNew` (`.xaml.cs:683` → `RefreshModuleSelector`) |
| P-10 | Insertar/vistas/Actualizar dejan los valores de la tabla §5 y cierran sin preguntar aunque hubiera módulo pendiente (`PendingWork()` vacío después) | `ClickNamed("InsertButton")`, `…("InsertLateralButton")`, `LoadExisting` + `…("UpdateButton")`; contrato público | `canInsertInAutoCad: true`; para Actualizar `LoadExisting` |
| P-11 | Guardas que no cierran: textos exactos de H-30 en `StatusText` y `InsertRequested == false` | `FindName("StatusText")` | `canInsertInAutoCad: false` (o invocar el manejador) |
| P-12 | Estado inicial de botones y tooltips (H-51, H-52) en los cuatro escenarios: {con/sin AutoCAD} × {nuevo/RACKEDITAR}; `InsertBatchButton.IsEnabled == true` incluso sin AutoCAD | `IsEnabled`, `ToolTip`, `ToolTipService.GetShowOnDisabled` | — |
| P-13 | Owner/ubicación de S-2, S-3, S-4: `Owner == ventana Push Back`, `WindowStartupLocation == CenterOwner` (S-4: owner Push Back aunque S-3 esté abierto) | seams que entregan la ventana REAL: `HeaderConfiguratorPresenter` (`.xaml.cs:2410`), `SafetyWindowDialog` (`:3028`), `BootPerPostWindowDialog` (`:2700`; se alcanza llamando `BootSectionForTest.Configure()` dentro del seam de S-3) | módulo cabecera seleccionado para S-2 |
| P-14 | Owner de S-1 (`null`) por el overload con `Window`; owner/ubicación de S-5..S-9 solo por guarda de fuente (`Owner = this` en `.xaml.cs:2865,3047,3438`; `TryShow(this` `:3311`; `PromptSaveToLibrary(this` `:3458`) | lectura de fuente | — |
| P-15 | Cerrar el configurador sin aceptar escenifica igual (H-42): con `HeaderConfiguratorPresenter = _ => { }`, tras `ConfigureModuleHeader_Click` hay origen pendiente / cambio de módulo | `HeaderConfiguratorPresenter`, `ModuleStatusText`, `PendingWork()` | confirmar antes si la configuración idéntica marca pendiente (R-8) |
| P-16 | Recursos: `Resources.MergedDictionaries[0].Source` termina en `Themes/AppStyles.xaml`; `ActionGap/ActionGroupGap/ActionLast` con sus valores; `Style` es el `EditorShellWindowStyle` del diccionario de la ventana; tras aplicar la plantilla, `Shell.Resources.MergedDictionaries.Count == 0` (H-49) | `Window.Resources`, `FindResource`, `FindName("Shell")` | plantilla aplicada |
| P-17 | Colapsados y deshabilitados al abrir (H-25, H-53) | `Visibility`, `IsEnabled` de `CompositeSection`, `AdvancedModulesPanel`, `FrontalSideBox`, `LateralSectionBox`, espejos | — |
| P-18 | Cancelar Seguridad no cambia `SafetySelections` ni `RecomputePassesForTest` | `SafetyDialog = _ => null` (`.xaml.cs:1493`) + `ClickNamed("SafetyButton")` | — |

Nota de método: `EditorWindowTestSupport.Click` eleva `ButtonBase.ClickEvent` directamente
(`tests/RackCad.UI.Tests/EditorWindowTestSupport.cs:110-111`) y NO pasa por `Button.OnClick`; para fijar lo que hacen
Escape/Cerrar en modal (R-1) hay que usar `ButtonAutomationPeer`/`IInvokeProvider.Invoke` o
`AccessKeyManager.ProcessKey(window, "\x1B", false)` (API pública de WPF, sin uso previo en el repo: validar en la prueba).

## Riesgos / ambigüedades

- **R-1. Posible doble pregunta al cerrar en MODAL con Cerrar/Escape.** Inferencia WPF, no medida: tras el `Click`, un botón
  `IsCancel` sin `Command` ejecuta `Window.DialogCancelCommand`, que en una ventana mostrada con `ShowDialog` asigna
  `DialogResult = false` y vuelve a llamar a `Close()`. Si el primer `OnClosing` se canceló («No»), habría un SEGUNDO prompt.
  Afecta a los dos llamadores reales (ambos modales, H-38). Solo se puede caracterizar con `ShowDialog` real en el hilo STA
  (`Dispatcher.BeginInvoke` que invoque el botón por automatización y cuente prompts); con `Show()` o con
  `EditorWindowTestSupport.Click` no aparece. Si se confirma, es comportamiento actual que se fija, no se corrige (D13).
- **R-2. Owner real en AutoCAD.** El Plugin no asigna `Owner` WPF (`RackPushBackCommands.cs:47-48,168-170`); el owner Win32 y
  el centrado `CenterOwner` dependen de `AcApplication.ShowModalWindow`. Sin AutoCAD no se puede medir. Tampoco el
  comportamiento de `CenterOwner` con `Owner == null` (ruta del menú sin padre).
- **R-3. Foco efectivo y tabulación necesitan ventana mostrada y activada.** En sesiones sin escritorio interactivo la
  activación puede fallar y `Keyboard.FocusedElement` no ser fiable; el repo ya eligió caracterizar la declaración del foco
  (H-19). Para la tabulación no hay alternativa de fuente: el orden real depende de H-21/H-23.
- **R-4. Orden de tabulación deducido, no medido.** H-26 se apoya en el orden de la plantilla (barra de acciones antes que la
  barra lateral) y en defectos WPF (shell `Control` y `ScrollViewer` enfocables, ciclo de `Window`). Cualquiera de esos
  supuestos puede no cumplirse; la prueba debe registrar lo que mida. Nota D9: el ciclo pasa por `RestoreButton`
  («Restaurar», reinicio de valores) antes de volver al panel lateral, y Shift+Tab desde `NameBox` podría caer en `CloseButton`.
- **R-5. Diálogos anidados con owner «saltado».** S-4..S-6 se abren con owner = ventana Push Back mientras S-3 es modal
  (H-41). El efecto en z-order y en la activación al cerrarlos (¿vuelve el foco a S-3 o al editor deshabilitado?) no es
  caracterizable sin interacción real; solo es comprobable la asignación (`Owner`), y para S-5/S-6 solo en fuente, porque
  sus seams sustituyen la ventana.
- **R-6. Prompt de descarte sin owner (S-1).** Distinto del configurador, que pasa `this` (`RackFrameConfiguratorWindow.xaml.cs:193-195`).
  Su posición y su z-order sobre AutoCAD no son medibles en prueba; sí lo es el `owner == null` por la costura.
- **R-7. X y Alt+F4.** Se asume que llegan a `OnClosing` igual que `Close()` (comentario `.xaml.cs:3384-3385`; criterio del repo
  en `EditorClosePolicyTests.cs:160-180`). Probar WM_CLOSE real exige HWND y P/Invoke; `Close()` es el sustituto aceptado.
- **R-8. Configurador: cancelar no se distingue de aceptar (H-42).** Antes de fijarlo hay que medir si escenificar una
  configuración sin cambios marca `HasPendingChanges`; si lo hace, cerrar el configurador con X deja trabajo pendiente y
  provoca el prompt al cerrar el editor.
- **R-9. «Insertar varias…» confirma la edición de módulo antes de mostrar su diálogo** (`.xaml.cs:3296`): cancelar S-8 deja
  la edición ya confirmada y recalculada. S-8 no tiene seam (`RackViewBatchDialogPresenter.TryShow` es estático y llama a
  `ShowDialog`): solo caracterizable con automatización modal o por fuente.
- **R-10. `InsertBatchButton` sin compuerta** (H-31). El estado habilitado es fijable; el camino completo sin AutoCAD
  (aceptar S-8 → `InsertRequested = true` → cierre) necesita automatización modal. Fijarlo como comportamiento actual no
  implica aprobarlo: cualquier corrección es cambio de producto con decisión del Owner (D13).
- **R-11. Texto sin comprometer al cerrar.** Que Escape/Cerrar descarte en silencio un `NumericField` editado y no abandonado
  depende de si WPF eleva `LostFocus` durante el cierre (inferencia: no, porque el botón `IsCancel` no toma el foco al
  activarse por tecla). Solo medible con ventana mostrada y foco real.
- **R-12. Pérdida de foco al reconstruir la matriz** (H-56): marcar «En blanco» con Espacio destruye el `CheckBox` enfocado.
  A dónde va el foco es comportamiento WPF no medido; requiere ventana mostrada.
- **R-13. Validación parcial** (H-57): errores en los campos fuera de `AllNumericFields` no bloquean las salidas. Pertenece más
  al mapa de intención/puerto, pero condiciona qué textos de estado ve el usuario al pulsar Insertar.
- **R-14. Ayudantes de prueba que esquivan la ruta real**: `EditorWindowTestSupport.Click` no ejecuta `Button.OnClick`
  (método, arriba); `PushBackShellAdoptionTests.Shown()` cambia `WindowStartupLocation` a `Manual` y `ShowInTaskbar` a
  `false` antes de mostrar (`tests/RackCad.UI.Tests/PushBackShellAdoptionTests.cs:49-61`): una caracterización debe leer esas
  propiedades ANTES de reutilizar ese patrón.
- **R-15. Pruebas antiguas en `Skip`** (H-36) contradicen el estado actual; reactivarlas fallaría y no son caracterización
  válida del código de este commit.
