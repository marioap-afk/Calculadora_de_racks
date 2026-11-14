# Mapa 3 — Cobertura existente e infraestructura de pruebas (F4-T1B-PILOT-CHARACTERIZATION)

- **Base medida:** commit `c3ae1c970c908ef541840fc583680b86376cae0b` del worktree de I-64 (lectura pura: sin compilar, sin
  ejecutar pruebas, sin tocar git). Rutas relativas a la raíz del repositorio. `bin/` y `obj/` ignorados.
- **Objeto:** `RackPushBackSystemWindow` (`src/RackCad.UI/Systems/PushBack/`), frente a INV-19
  (`docs/initiatives/I-64-proposal-v5.md:757`), D-15 (`:557-575`) y ADR-0029 D9/D13
  (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md:132-150`, `:172-182`).
- **Convención:** H-n = hecho medido con cita. Las cifras «estáticas» cuentan atributos en el código fuente, no casos en
  tiempo de ejecución (una `[Theory]` se expande).

---

## 1. Infraestructura de `tests/RackCad.UI.Tests`

### 1.1 Proyecto

- **H-1.** `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:5-6`: `net8.0-windows` con `UseWPF=true`; `:8` `RollForward=LatestMajor`.
- **H-2.** Paquetes: solo `Microsoft.NET.Test.Sdk 17.11.1`, `coverlet.collector 6.0.4`, `xunit 2.9.2`,
  `xunit.runner.visualstudio 2.8.2` (`RackCad.UI.Tests.csproj:15-21`). No hay FlaUI, `xunit.stafact` ni otro paquete de UI.
  ADR-0029 lo descarta por ADR-0012 y «porque el repositorio ya resuelve STA con su propio runner»
  (`docs/adr/0029-...md:196-197`). AGENTS.md: «No agregar dependencias sin acuerdo explícito» (`AGENTS.md:100-105`).
- **H-3.** Referencias: Domain, Application y UI (`RackCad.UI.Tests.csproj:25-27`). No referencia el Plugin ni AutoCAD.
- **H-4.** Los catálogos reales (`assets/catalogs/*.json;*.csv`) se copian junto al binario de pruebas
  (`RackCad.UI.Tests.csproj:32-36`). Las ventanas construidas en prueba cargan datos reales.
- **H-5.** `tests/RackCad.Tests` es `net8.0` sin referencia a UI (`tests/RackCad.Tests/RackCad.Tests.csproj:3,23-31`). Desde
  Core, la ventana solo se puede caracterizar **leyendo su fuente como texto** (guardas de fuente).
- **H-6.** No hay `xunit.runner.json` ni `[assembly: CollectionBehavior]` en la suite UI. La única `CollectionDefinition` es
  `EditorDiscardPromptCollection` (`tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs:27`, `DisableParallelization = true`),
  usada por dos clases: `EditorClosePolicyTests.cs:18` y `RichEditorCloseContractTests.cs:29`.

### 1.2 STA: `StaTestRunner`

- **H-7.** `tests/RackCad.UI.Tests/StaTestRunner.cs:20-110`: un único hilo STA de fondo (`:64-69`) con un `Dispatcher` vivo
  (`:59-61`) creado una sola vez por proceso (`:25-75`). `Run(Action)` (`:78-99`) y `Run<T>(Func<T>)` (`:102-109`) hacen
  `Dispatcher.Invoke` síncrono, capturan la excepción y la relanzan en el hilo de xUnit con su traza (`:95-98`).
- **H-8.** No hay atributos propios (no existe `[StaFact]`): toda prueba UI es `[Fact]`/`[Theory]` normal que envuelve su
  cuerpo en `StaTestRunner.Run(...)`. Autoprueba del runner: `tests/RackCad.UI.Tests/StaTestRunnerTests.cs` (construye controles
  y una `Window` en el hilo STA).
- **H-9.** Todas las clases de prueba que corren en paralelo encolan su trabajo WPF en **el mismo** dispatcher. No hay en la suite
  ningún patrón de bombeo (`DispatcherFrame`, `PushFrame`, `BeginInvoke`, `Application.Current.Windows`): búsqueda vacía en
  `tests/`.

### 1.3 `Application` y recursos

- **H-10.** `StaTestRunner.cs:40-43`: crea `new System.Windows.Application { ShutdownMode = OnExplicitShutdown }` si no existe.
  **No** carga ningún diccionario en `Application.Resources`: los recursos vienen de cada ventana.
- **H-11.** La ventana Push Back fusiona `/RackCad.UI;component/Themes/AppStyles.xaml` en `Window.Resources`
  (`RackPushBackSystemWindow.xaml:10-14`), declara tres tokens locales `ActionGap` (`0,0,6,4`), `ActionGroupGap` (`0,0,16,4`)
  y `ActionLast` (`0,0,0,4`) (`:20-22`) y aplica `Style="{DynamicResource EditorShellWindowStyle}"` (`:7`;
  definido en `src/RackCad.UI/Themes/AppStyles.xaml:165`).
- **H-12.** La plantilla de `RackEditorVisualShell` vive en el diccionario de tema `src/RackCad.UI/Themes/Generic.xaml:139-200`.
  `RackEditorVisualShell.OnApplyTemplate` fusiona `ShellResources.Shared` **solo si** `ShellZoneSpacing` no resuelve
  (`src/RackCad.UI/Shell/RackEditorVisualShell.cs:45-51`), porque un `DynamicResource` con clave de texto no cae al diccionario
  de tema (`src/RackCad.UI/Shell/ShellResources.cs:43-52`). En la ventana clásica el token resuelve por `Window.Resources`.
- **H-13.** Las pruebas que necesitan la plantilla en una ventana **no mostrada** la aplican a mano:
  `shell.Style = (Style)new ResourceDictionary { Source = ".../Generic.xaml" }[typeof(RackEditorVisualShell)]` y
  `Measure/Arrange` (`PushBackShellAdoptionTests.cs:168-172`, `PushBackEditorLayoutTests.cs:243`). Las que comparan con tokens
  cargan `AppStyles.xaml` aparte (`RichEditorCharacterizationTests.cs:240-247`, `PushBackShellAdoptionTests.cs:155`).

### 1.4 Búsqueda en el árbol

- **H-14.** `tests/RackCad.UI.Tests/EditorWindowTestSupport.cs` (I-24, ampliado en I-31 e I-39A):
  - `ClickNamed(window, name)` por `FindName` (`:22-27`); `ClickByContent(window, content)` (`:31-36`).
  - `FindByContent` recorre el árbol **lógico** y, para un `RackEditorVisualShell` no mostrado, **los slots** (propiedades de
    dependencia) (`:113-157`, slots `:159-170`; shell acotado `:172-181`).
  - `Descendants(root)` = árbol lógico + slots de ambos shells (`:192-231`); `Find<T>`/`FindAll<T>` (`:234-239`). Es lo que usan
    las caracterizaciones de I-39B/I-39D para ser «ciegas a la composición».
  - `BlankFrontBox`/`SetFrontBlank` (`:43-68`), `SetText` (`:89-95`), `SetNumberAndCommit` = `SetNumber` + `LostFocus` real
    (`:102-108`).
- **H-15.** Árbol **visual** solo en ventanas mostradas: `PushBackShellAdoptionTests.cs:23-45` (`VisualTreeHelper`, `IsUnder`).
- **H-16.** `SafetyDialogTestSupport.cs:14-43`: descendientes lógicos y anclas por texto para las rejillas de seguridad.
- **H-17.** `PushBackHeaderTestSupport.cs:18-115`: lectura de la cabecera física `(PostIndex, ModuleId)` en sesión y diseño.

### 1.5 Clics, texto, foco perdido

- **H-18.** Un clic es `button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button))` (`EditorWindowTestSupport.cs:110-111`;
  copias locales en `PushBackOutputGateAfterRecomputeTests.cs:56-57`, `PushBackModuleEditorWindowTests.cs:32`, etc.).
  **Esto lanza el evento enrutado pero no pasa por `Button.OnClick`**, que es donde WPF ejecuta `Window.DialogCancelCommand`
  para un botón `IsCancel` (comportamiento de WPF, no medido en este repo: ninguna prueba lo ejercita).
- **H-19.** «Salir del campo» = `RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element))`
  (`PushBackOutputGateAfterRecomputeTests.cs:53-54`, `PushBackModuleEditorWindowTests.cs:117`). Escribir sin salir
  («edición escenificada») = `NumericField.SetNumber` sin `LostFocus` (`PushBackOutputGateAfterRecomputeTests.cs:96-97`).
- **H-20.** Selección de celdas por seam interno `w.SelectMatrixCell(...)` (`PushBackCellConfigurationWindowTests.cs:371`); clics de
  ratón reales sobre celdas solo en Dinámico/Selectivo (`DynamicShellMigrationTests.cs:201`, `SelectiveShellMigrationTests.cs:225`).

### 1.6 Teclas

- **H-21.** No hay en la suite **ninguna** simulación de Enter, Escape o Tab sobre `RackPushBackSystemWindow`. Los patrones que
  existen en otras ventanas:
  - `KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0,0,0,0,0,"t",IntPtr.Zero), 0, Key.Enter) { RoutedEvent = UIElement.KeyDownEvent }`
    (`SelectiveCommitCoverageTests.cs:34-43`, `SelectivePendingEditorsTests.cs:127-132`).
  - Helper con `PreviewKeyDown`/`KeyDown`, `HwndSource` desechable y marca de repetición por reflexión
    (`Workspace/WorkspacePilotFieldViewTests.cs:87-107`; Enter/Escape/Tab en `:214-215`, `:397-401`, `:467`).
  - Ninguna usa `AccessKeyManager`, `AutomationPeer`/`IInvokeProvider`, `MoveFocus`/`TraversalRequest` ni P/Invoke
    (búsqueda vacía en `tests/`).
- **H-22.** La ventana Push Back **no declara manejadores de teclado**: búsqueda de `KeyDown|PreviewKeyDown|Key.Enter|Key.Escape|InputBindings|KeyBinding`
  vacía en `src/RackCad.UI/Systems/PushBack/`; usa 30 `controls:NumericField` y 1 `controls:CatalogCombo`, y ninguno de los dos
  controles maneja teclas (los únicos `Key.Enter` de `src/RackCad.UI/Controls` están en `LinkedPropertyEditor.cs:289-294` y
  `StructuralSectionPicker.cs:244`, que la ventana no usa).

### 1.7 Foco

- **H-23.** Convención I-39A/I-39B: el foco inicial declarado por **Binding `ElementName`** se caracteriza **leyendo el XAML**, no con
  `FocusManager.GetFocusedElement`, porque el Binding «no resuelve hasta que la ventana se muestra, y una caracterización no debe
  mostrar ventanas» (`BoundedEditorCharacterizationTests.cs:251-256`; misma razón en `RichEditorContractTests.cs:58-60`).
- **H-24.** `FocusManager.GetFocusedElement(window)` sí se usa cuando el foco se declara en código o para afirmar su ausencia
  (`BoundedEditorContractTests.cs:343`, `CustomPropertiesWindowTests.cs:840`, `DialogWindowCharacterizationTests.cs:481`,
  `StructuralSectionInspectorWindowTests.cs:554`).
- **H-25.** Push Back declara `FocusManager.FocusedElement="{Binding ElementName=NameBox}"` (`RackPushBackSystemWindow.xaml:9`);
  `NameBox` es un `TextBox` (`:47`).

### 1.8 Mostrar ventanas y cerrarlas

- **H-26.** Patrón «mostrada fuera de pantalla»: `WindowStartupLocation = Manual, ShowInTaskbar = false, Left = -10000, Top = -10000`,
  `Show()`, `UpdateLayout()` (`PushBackShellAdoptionTests.cs:49-61`, `PushBackBootUiFlowTests.cs:38-51`). Lo usan 14 archivos
  Push Back (A1Editor, BootSidesUiFlow, BootUiFlow, DefensePerPostEndToEnd, DefenseSideSections, DefenseTypePerSide,
  RearTopePerSideAndNone, RichPreview, SafetyFlow, SafetyPerPostFlow, ShellAdoption, SidebarComposition, TopeOnlyInSafety,
  TopeSectionInSafety). **Sobrescribe `WindowStartupLocation`**, así que esas pruebas no pueden leer `CenterOwner` después.
- **H-27.** Diez archivos muestran la ventana **en pantalla**, sin `Manual`: `PushBackEditorLayoutTests.cs:70-75`,
  `PushBackModuleEditorWindowTests.cs:35-47` (y AdvancedRackParameters, HeaderAuthority, HeaderCartesian, HeaderLifecycle,
  HeaderLine, HeaderOwnerRoundTwo, MatrixAndPreview, PhysicalHeaderScope). `PushBackEditorLayoutTests.cs:213-214` deja escrito que
  `Show()` recorta la ventana al área de trabajo del host, por eso esa prueba no la muestra.
- **H-28.** El cierre se ejerce siempre con `window.Close()` directo (p. ej. `finally { w.Close(); }`,
  `PushBackShellAdoptionTests.cs:93`); el cierre por botón «Cerrar» no se ejerce nunca (ver §3). La detección de cierre usa
  `window.Closed += ...` (`PushBackOutputGateAfterRecomputeTests.cs:113-114`, `:137-138`; `PushBackLibrarySaveGateTests.cs:112-113`).
- **H-29.** `Close()` sobre una ventana **nunca mostrada** pasa por `OnClosing`: lo fija `EditorClosePolicyTests.cs:135-157`
  (`ClosingCount == 1` sobre una `ProbeWindow` no mostrada).

### 1.9 `ShowDialog` sin bloquear: costuras de la propia ventana

No hay mecanismo genérico; la ventana expone **costuras internas** que sustituyen o el diálogo entero o solo su `ShowDialog`:

| Costura (`RackPushBackSystemWindow.xaml.cs`) | Tipo | Qué sustituye | Recibe la instancia real | Uso en pruebas |
|---|---|---|---|---|
| `SafetyDialog` `:1493` (uso `:2615-2617`) | `Func<selecciones, selecciones>` | todo el diálogo de seguridad | no | SafetyFlow, SafetyPerPostFlow, DefenseSideSections, DefenseTypePerSide, RearTopePerSideAndNone, TopeSectionInSafety |
| `SafetyWindowDialog` `:3028` (uso `:3018`) | `Func<SelectiveSafetyWindow, bool?>` | solo `ShowDialog` | **sí** (ya con `Owner = this`, `:3016`) | BootUiFlow, BootSidesUiFlow, DefensePerPostEndToEnd, DefenseSideSections, DefenseTypePerSide |
| `RearTopeDialog` `:1499` (uso `:2981`) | `Func<config, niveles, TopeResult>` | todo `SafetyTopeGridWindow` | no | SafetyFlow, RearTopePerSideAndNone, TopeSectionInSafety, Defense* |
| `DefenseDialog` `:1505` (uso `:2848-2849`) | `Func<sección, defensas>` | todo `SafetyDefensaGridWindow` | no | DefensePerPostEndToEnd, DefenseSideSections, DefenseTypePerSide |
| `BootPerPostWindowDialog` `:2700` (uso `:2722`) | `Func<SafetyPerPostWindow, bool?>` | solo `ShowDialog` | **sí** (`Owner = this`, `:2721`) | BootUiFlow, BootSidesUiFlow, SafetyPerPostFlow |
| `HeaderConfiguratorDialog` `:1525` (uso `:1938-1940`) | `Func<config, config>` | todo el configurador | no | ModuleEditorWindow |
| `HeaderConfiguratorPresenter` `:2410` (uso `:2437-2444`) | `Action<RackFrameConfiguratorWindow>` | solo `ShowDialog` | **sí** (`Owner = this`, `:2426`) | HeaderAuthority, HeaderCartesian, HeaderLifecycle, HeaderLine, HeaderOwnerRoundTwo, PhysicalHeaderScope |

- **H-30.** Diálogos **sin costura**: `RackBomWindow { Owner = this }.ShowDialog()` (`:3438`), `RackViewBatchDialogPresenter.TryShow(this, ...)`
  (`:3311`; dentro: `new RackViewBatchDialog(presenter) { Owner = owner }` + `ShowDialog()`,
  `src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:52-63`), `UiSupport.PromptSaveToLibrary(this, ...)` (`:3458`; dentro
  `SaveFileDialog.ShowDialog(owner)`, `src/RackCad.UI/UiSupport.cs:64-82`), y el `MessageBox` de descarte (§1.10).
- **H-31.** Señal «llegó al diálogo» por excepción: en banco la ventana no está mostrada y abrir el `SaveFileDialog` lanza; la prueba
  usa `Record.Exception(...) != null` como señal (`PushBackLibrarySaveGateTests.cs:89-95`). Igualmente, `Bom_Click` bloqueado se
  verifica con `Record.Exception == null` (`PushBackOutputGateAfterRecomputeTests.cs:161-164`). Las pruebas que reciben instancias
  de subdiálogo por costura **muestran antes la ventana padre** (H-26/H-27), coherente con la regla de WPF de que `Owner` no
  admite una ventana nunca mostrada (regla de WPF; en este repo solo está medida indirectamente por esas dos señales).
- **H-32.** La costura `SafetyWindowDialog` de las pruebas **reescribe** la ubicación del subdiálogo antes de mostrarlo
  (`PushBackBootUiFlowTests.cs:103-109`: `dialog.WindowStartupLocation = Manual`, `Left/Top = -10000`, `Show()`).

### 1.10 `MessageBox` y la confirmación de descarte

- **H-33.** `src/RackCad.UI/Shell/EditorClosePolicy.cs:49-95`: `EditorDiscardPrompt` es un delegado **estático de proceso**
  `Func<string, Window, bool>` (`:51`) con `Substitute(Func<string,bool>)` (`:57-62`) y `Substitute(Func<string,Window,bool>)`
  para inspeccionar el dueño (`:65-72`); `Restore` no reentrante (`:82-94`). Producción: `MessageBox.Show` con o sin dueño (`:76-80`).
- **H-34.** `EditorClosePolicy.MayClose` llama a `EditorDiscardPrompt.Confirm(pending.Question)` **sin dueño** (`:108-113`), así
  que en Push Back el `MessageBox` de descarte sale por la rama `owner == null` (`:77-78`). El configurador de cabecera sí pasa
  `this` (`src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs:193-195`).
- **H-35.** **Suelo de la suite:** `StaTestRunner.cs:45-57` instala para siempre `EditorDiscardPrompt.Substitute(_ => true)`. Toda prueba que
  cierra una ventana con trabajo pendiente y no sustituye el prompt **acepta el descarte en silencio**. Las clases que sustituyen el
  prompt deben entrar en `EditorDiscardPromptCollection` para no correr en paralelo (`EditorDiscardPromptCollection.cs:5-26`).
- **H-36.** No hay `MessageBox` directo en `src/RackCad.UI/Systems/PushBack/` (búsqueda vacía). `ci.yml:218-221` describe el riesgo de
  colgarse en un `MessageBox` modal real.

### 1.11 Guardas de fuente

- **H-37.** Idioma: localizar la raíz por `RackCad.sln` desde `AppContext.BaseDirectory` y leer `src/...` como texto
  (`RichEditorCharacterizationTests.cs:75-87`, `DialogWindowCharacterizationTests.cs:136-145`). Ejemplo de guarda de dueño de
  subdiálogos: buscar `new X(` y exigir `Owner = this` en los 1400 caracteres siguientes (`DialogWindowCharacterizationTests.cs:587-605`).
  Las guardas de D12 retiran comentarios antes de buscar (`DialogWindowContractTests.cs:279-287`).

### 1.12 Documentación

- **H-38.** No hay guía de pruebas de UI en `docs/guias/` (12 archivos, ninguno de pruebas). La documentación operativa está en
  `AGENTS.md:25,33` (la suite UI exige Windows y STA), en `docs/initiatives/I-24-ui-tests-editores.md` (clics reales, un único seam
  autorizado, `:119-162`) y en las evidencias de I-39 (`docs/automation/evidence/I-39B-caracterizacion-base-vs-contrato.md`,
  `I-39C-...`, `I-39D-...`).

---

## 2. Cómo construyen y caracterizan la ventana las pruebas existentes

Constructores reales: `RackPushBackSystemWindow()` → `this(false)` y `RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)`
(`RackPushBackSystemWindow.xaml.cs:123-152`; el constructor llama a `LoadNew()` en `:151`). **Ninguna prueba pasa `newIdFactory`**
(búsqueda vacía). Cargas: `LoadNew`, `LoadDesignForNew(design, name, source)`, `LoadExisting(design, id, name[, source])`.
Estático: 44 archivos `PushBack*.cs` con 446 atributos `[Fact]/[Theory]`; la suite UI tiene 1893 atributos en 181 archivos `.cs`.

### Grupo A — Contrato transversal de las seis ventanas ricas (I-39A/B)

| Archivo | Construcción | Qué fija de Push Back |
|---|---|---|
| `RichEditorCharacterizationTests.cs` | `new RackPushBackSystemWindow(canInsertInAutoCad: true)` (`:53`), no mostrada | sin `IsDefault` (`:106`); `Close()` no materializa (`:173-175`); `ModuleSession` sin pendientes al abrir (`:187-189`); `Content` es `RackEditorVisualShell` (`:224`); tamaño = tokens (`:249-255`); `CenterOwner` (`:281-284`); sin `TabIndex` explícito (`:327-332`); sin `EditorStatusPresenter` (`:347-350`). Cuatro pruebas `Skip = BaseEvidence` (`:114`, `:138`, `:197`, `:288`) |
| `RichEditorCloseContractTests.cs` (colección del prompt) | ídem (`:34`) | botón `IsCancel` con texto «Cerrar» (`:55-63`); declara `OnClosing` por reflexión (`:71-74`); `Close()` sin pendientes no pregunta y no deja petición (`:97-99`, `:141-143`) |
| `RichEditorContractTests.cs` | — (lee XAML) | el XAML contiene `FocusManager.FocusedElement` (`:43`, `:49-51`) |
| `EditorClosePolicyTests.cs` (colección del prompt) | `new RackPushBackSystemWindow(canInsertInAutoCad: true)` (`:189`, `:202`) | `PendingWork()` falso al abrir (`:191`); el resto usa una `ProbeWindow` (`:115-132`) |
| `WindowCensusGuardTests.cs` | reflexión sobre el ensamblado | Push Back en el arquetipo A (`:29-33`); `RackViewBatchDialog` en C (`:64`); 6+6+13+6 = 31 tipos (`:178-183`) |
| `UiSystemBoundaryGuardTests.cs` | `canInsertInAutoCad: false` (`:71`) | carga del XAML y namespace de frontera (`:98`) |

### Grupo B — Shell, composición y tamaño (I-18b, PB-VAL-01, I-42)

| Archivo | Construcción | Qué fija |
|---|---|---|
| `PushBackShellAdoptionTests.cs` | `new RackPushBackSystemWindow()` no mostrada y `Shown()` fuera de pantalla (`:49-61`) | **identidad** de `PrimaryButtonStyle`/`SecondaryButtonStyle` por `w.FindResource` (`:71-83`); `ShowOnDisabled` (`:87-91`); superficies = tokens (`:105-120`); estado en `StatusHost` (`:138-144`); tamaño, scroll y ≥10 botones visibles en la barra (`:163-185`); x:Names (`:200-208`) |
| `PushBackActionBarRhythmTests.cs` | `canInsertInAutoCad: true`, no mostrada | márgenes ∈ {`ActionGap`, `ActionGroupGap`, `ActionLast`} vía `FindResource` (`:45-67`); anchos múltiplos de 8 con el compuesto (`:88-123`). Lista visual de 11 controles **sin `InsertBatchButton`** (`:29-37`) |
| `PushBackEditorLayoutTests.cs` | `new RackPushBackSystemWindow()`; `Shown()` **en pantalla** (`:70-75`) | cuatro zonas, matriz central, vistas como botones, barra visible al mínimo (`:157-185`), anchos útiles, tamaño declarado sin mostrar (`:209-226`), seguridad por defecto |
| `PushBackSidebarCompositionTests.cs` | fuera de pantalla | composición del panel lateral |

### Grupo C — Intención, payload y compuertas de salida

| Archivo | Construcción / dobles | Qué fija |
|---|---|---|
| `PushBackEditorWindowTests.cs` | `canInsertInAutoCad: true`, no mostrada; clics por `ClickNamed` | `LoadDesignForNew` + «Insertar» → GUID nuevo, nombre, `InsertRequested`, `SourceProject` por referencia (`:141-163`); `LoadExisting` conserva id/nombre y habilita «Actualizar» (`:169-182`); campo inválido apaga Insertar/Actualizar/BOM/Biblioteca y no produce petición (`:314-350`); `ViewBox`→`InsertView/InsertSection` (`:502-520`); lateral tipado (`:523-542`); Update: `UpdateOnly`, vista nula, sección −1, mismo GUID y fuente (`:546-568`); vista adicional en existente (`:572-590`); biblioteca por **seam** `BuildLibraryProjectForTest` sin `InsertRequested` (`:593-616`) |
| `PushBackOutputGateAfterRecomputeTests.cs` | `LoadExisting` opcional; dobles: ninguno | Insert/Update bloqueados tras el recálculo del clic: sin petición y **sin cerrar** (`closed == false`, `:108-129`, `:132-150`); BOM bloqueado sin abrir diálogo (`:153-168`); salida válida o con aviso sí pide (`:173-209`); la edición escenificada se conserva (`:212-233`) |
| `PushBackLibrarySaveGateTests.cs` | ídem | misma compuerta para «Guardar en biblioteca»; señal por excepción del diálogo (`:89-95`); ventana sigue viva (`:123`) |
| `PushBackA1EditorTests.cs` | fuera de pantalla | diagnóstico bloqueante apaga Insert/Update/BOM (`:149-195`); Restaurar (`:101-147`); ida y vuelta de defensa por `LoadExisting` (`:78`, `:196`) |
| `PushBackCellConfigurationWindowTests.cs` | `Loaded(...)` con `LoadExisting` | cerrar con ediciones aplicadas → sin petición, `DialogResult != true` (`:387-405`) |
| `PushBackInsertionRequestTests.cs` | datos puros | forma del `PushBackInsertionRequest` (`:17-60`) |
| `PushBackEditorModuleTests.cs` | metadatos y registro | contrato del módulo (`:49-56`); no abre ventanas |
| `RackMainMenuPushBackTests.cs` | `RackMainMenuWindow` | botón del menú y su manejador; `InsertionRequest` único |

### Grupo D — Sesión de módulos y cabeceras (I-35, I-40)

`PushBackModuleEditorWindowTests.cs` (mostrada en pantalla, `:35-47`; doble `HeaderConfiguratorDialog`, `:174-178`, `:206`, `:230`),
`PushBackHeaderAuthorityWindowTests.cs`, `PushBackHeaderCartesianTests.cs`, `PushBackHeaderLifecycleTests.cs`, `PushBackHeaderLineWindowTests.cs`,
`PushBackHeaderOwnerRoundTwoTests.cs`, `PushBackPhysicalHeaderScopeTests.cs` (todas con `HeaderConfiguratorPresenter`, que recibe el
configurador real; p. ej. `PushBackHeaderAuthorityWindowTests.cs:100`), `PushBackAdvancedRackParametersWindowTests.cs`.
Fijan la transacción confirmar/cancelar (`ModuleSession.HasPendingChanges`, p. ej. `PushBackModuleEditorWindowTests.cs:119-128`), que
dibujar confirma lo escenificado (`PushBackHeaderLifecycleTests.cs:113-140`) y la ida y vuelta por `LoadExisting`. **Dejan trabajo
pendiente y cierran en `finally` sin aserción**: el cierre lo acepta el suelo de la suite (H-35).

### Grupo E — Seguridad y subdiálogos

`PushBackSafetyFlowTests`, `PushBackSafetyPerPostFlowTests`, `PushBackBootUiFlowTests`, `PushBackBootSidesUiFlowTests`,
`PushBackDefensePerPostEndToEndTests`, `PushBackDefenseSideSectionsTests`, `PushBackDefenseTypePerSideTests`,
`PushBackRearTopePerSideAndNoneTests`, `PushBackTopeSectionInSafetyTests`, `PushBackTopeOnlyInSafetyTests` (ventana fuera de pantalla;
costuras de §1.9). Fijan lo que se acepta/cancela, el resolve, el dibujo y el BOM. **Ninguna** afirma `Owner` ni
`WindowStartupLocation` del subdiálogo (búsqueda de `.Owner` y `CenterOwner` vacía en `PushBack*.cs`).
`PushBackDefensaDialogTests`, `PushBackDesviadorGridTests`, `PushBackTopeDialogOptionsTests`, `BlankFrontSafetyGridTests`,
`BlankFrontDesviadorHandoffTests` y `SafetyGridBulkAdoptionTests.cs:574-579` construyen los diálogos **directamente, replicando los
argumentos** que usa la ventana.

### Grupo F — Compuesto, estado, matriz y preview (funcional)

`PushBackCompositeWindowTests` (incluye `FrontalSideForTest`, `:1255-1275`), `PushBackCompositeLifecycleWiringTests`,
`PushBackCompositeLoadExistingModulesTests`, `PushBackDormantRestoreTests`, `PushBackDormantTailTransactionTests`,
`PushBackEditorSideOwnershipTests`, `PushBackBothModeCellBeamOverrideTests`, `PushBackMatrixAndPreviewTests`, `PushBackRichPreviewTests`,
`PushBackPalletScopeTests`, `PushBackDimensionViewsWindowTests`, `BlankFrontEditorTests`. No caracterizan teclado, foco, cierre ni dueños.

### Grupo G — I-64 (ventana frente a piloto)

`Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:68-73` construye la ventana con `LoadExisting` sobre el corpus
`tests/RackCad.Tests/Fixtures/I64/` y fija el delta de diseño por clases (a)-(d); un delta de otra clase es STOP, «se informa, nunca se
fija» (`:18-26`). `Workspace/WorkspaceDraftPilotValidationParityTests.cs:88-91` lee el XAML de `AnnotationScaleBox`. No cierra las ventanas.

---

## 3. Matriz de cobertura frente a INV-19

Hechos de producto que sostienen las filas: `OnClosing` `:3395-3404` (no pregunta si `InsertRequested`); `PendingWork()` `:3417-3419`
(solo la sesión de módulos); `Close_Click` `:3381`; `RequestDraw` `:3323-3379` (fuera de AutoCAD `:3325-3329`, no existente `:3331-3335`,
confirma lo escenificado `:3346-3349`, cierra `:3378`); `InsertBatch_Click` `:3294-3321` (cierra `:3320`; **no comprueba
`canInsertInAutoCad`** y `UpdateButtons` `:3523-3567` **no gobierna `InsertBatchButton`**); `IsCancel` en `CloseButton`
(`RackPushBackSystemWindow.xaml:789`).

| # | Punto | Pruebas que ya lo fijan (archivo:línea, método) | Estado | Razón |
|---|---|---|---|---|
| 1 | Enter — ventana | `RichEditorCharacterizationTests.cs:92` `TheDefaultActionOfEachRichEditorIsWhatItIsToday` (`:106`) | PARCIAL | Fija que no hay `IsDefault`. No fija que Enter en `NameBox`, un `NumericField` o un `ComboBox` no haga nada (ni cierre ni pida); la ausencia de manejadores (H-22) no tiene guarda |
| 2 | Enter — subdiálogos | `DialogWindowCharacterizationTests.cs:185` `LosDiezDeCDeclaranUnaAccionPorDefectoYUnaDeCancelacion`; `:201` `LaEtiquetaPrimaria...` ("Aceptar"); configurador: `RichEditorCharacterizationTests.cs:108`; BOM (D): `DialogWindowCharacterizationTests.cs:256` | PARCIAL | A nivel de **tipo** y con argumentos distintos de los de Push Back. `RackViewBatchDialog` (`IsDefault` «Insertar cola», `src/RackCad.UI/Views/RackViewBatchDialog.xaml:36`): SIN COBERTURA |
| 3 | Escape — ventana | `RichEditorCloseContractTests.cs:48` `TheSixCloseWithEscape` | PARCIAL | Solo la declaración `IsCancel`+«Cerrar». No ejercita Escape → `Close_Click` → `OnClosing`, ni con trabajo pendiente, ni el `DialogCancelCommand` de WPF en modal (H-18) |
| 4 | Escape — subdiálogos | `DialogWindowCharacterizationTests.cs:185` (un `IsCancel` por tipo) | PARCIAL | Igual que la fila 2; `RackViewBatchDialog` (`IsCancel` «Cancelar», `RackViewBatchDialog.xaml:35`): SIN COBERTURA |
| 5 | Foco inicial — ventana | `RichEditorContractTests.cs:36` `TheSixDeclareAnInitialFocus` | PARCIAL | Solo que el XAML **contiene** `FocusManager.FocusedElement`. No fija el destino (`NameBox`), que no sea una acción (como hace para la Cabecera `RichEditorContractTests.cs:56-81`) ni el foco efectivo |
| 6 | Foco inicial — subdiálogos | `DialogWindowCharacterizationTests.cs:473` `NingunaDeLasDieciseisDeclaraFocoInicialNiOrdenDeTabulacion`; configurador `RichEditorContractTests.cs:56` | PARCIAL | Por tipo; `RackViewBatchDialog` SIN COBERTURA |
| 7 | Orden de tabulación — ventana | `RichEditorCharacterizationTests.cs:321` `NoRichEditorDeclaresAnExplicitTabOrder` | PARCIAL | Fija «ningún `TabIndex` explícito» en el árbol lógico+slots. No fija el orden efectivo, `IsTabStop` ni `KeyboardNavigation`. El orden efectivo depende de la plantilla: en `Generic.xaml:153-197` el primer hijo del `DockPanel` es la barra de acciones, luego estado, panel lateral y zona de trabajo |
| 8 | Orden de tabulación — subdiálogos | `DialogWindowCharacterizationTests.cs:482-484` | PARCIAL | Igual: ausencia de `TabIndex`, por tipo |
| 9 | Cierre: botón «Cerrar» (`Close_Click`) | — | SIN COBERTURA | Ninguna prueba pulsa `CloseButton` de Push Back; todas llaman a `Close()` |
| 10 | Cierre: `Close()` sin trabajo pendiente | `RichEditorCloseContractTests.cs:90` `ClosingWithoutPendingWorkStillAsksNothing`; `:129` `NoCloseRouteMaterialisesAnything`; `RichEditorCharacterizationTests.cs:159` `ClosingARichEditorNeverMaterialisesAnything`; `PushBackCellConfigurationWindowTests.cs:387` `ClosingWithoutInserting_LeavesNoResult_AndDoesNotTouchTheDrawing`; `EditorClosePolicyTests.cs:185` `PushBackDeclaresNoPendingWorkWhenNothingIsStaged` | CUBIERTO | No pregunta, no deja petición, `DialogResult != true` |
| 11 | Cierre con trabajo pendiente (aceptar / rechazar / texto / dueño del prompt) | `EditorClosePolicyTests.cs:135`, `:160` (sobre `ProbeWindow`); `:196` `PushBackPendingWorkQuestionNamesWhatIsLost` | SIN COBERTURA | La de `:196` es **tautológica**: afirma un literal construido en la propia prueba (`:203-208`), no `window.PendingWork().Question`, y afirma `PendingWork()` falso. Nadie fija que con módulo escenificado `Close()` pregunte una vez, con qué texto, sin dueño (H-34), que rechazar deje la ventana viva ni que aceptar no materialice |
| 12 | Escape / botón de sistema / Alt+F4 → misma política | `RichEditorCloseContractTests.cs:69` `OnlyTheEditorsWithADeclaredScopeInterceptTheirClosing` (reflexión); `EditorClosePolicyTests.cs:160` `EveryCloseRouteReachesTheSamePolicy` (`ProbeWindow`) | PARCIAL | Que Push Back **declara** `OnClosing`, y el mecanismo en una ventana sonda. Ninguna ruta real (tecla, WM_CLOSE) sobre Push Back |
| 13 | Cierre por Insertar/Actualizar (sin preguntar) | Intención: filas 21-22. Negativo: `PushBackOutputGateAfterRecomputeTests.cs:108`, `:132` (`closed == false` si bloqueado) | PARCIAL | Ninguna afirma positivamente que la ventana **se cierra** (`Closed`) tras pedir, ni que no pregunta aunque hubiera pendientes (rama `!InsertRequested`, `:3397`) |
| 14 | Cierre por «Insertar varias…» | — | SIN COBERTURA | `RackViewBatchDialogPresenter.TryShow` no tiene costura; el botón no aparece en ninguna lista de pruebas (H-30, Grupo B) |
| 15 | Salidas rechazadas fuera de AutoCAD / sin RACKEDITAR | `PushBackEditorWindowTests.cs:169` (Actualizar habilitado en existente) | PARCIAL | No se fijan el estado ni los tooltips con `canInsertInAutoCad: false` (`:3526-3534`) ni con rack nuevo (`:3542-3544`), ni los mensajes de `RequestDraw` (`:3327`, `:3333`). Tampoco el estado de `InsertBatchButton` (no gobernado) |
| 16 | `Owner` de la ventana (hosts) | — | SIN COBERTURA | RACKPUSHBACK/RPB y RACKEDITAR: `AcApplication.ShowModalWindow(window)` sin `Owner` (`src/RackCad.Plugin/RackPushBackCommands.cs:47-48`, `:168-170`); las guardas solo comprueban la construcción (`tests/RackCad.Tests/PushBackPluginSourceGuardTests.cs:58-71`, `PushBackRoundTripSourceGuardTests.cs:191-196`). Menú: `{ Owner = context.Owner }` + `ShowDialog` (`src/RackCad.UI/Editor/EditorModules.cs:114-115`, `:121-126`), sin prueba (`PushBackEditorModuleTests.cs:49` solo metadatos) |
| 17 | `CenterOwner` de la ventana | `RichEditorCharacterizationTests.cs:277` `AllSixDeclareCenterOwner` | CUBIERTO | Declaración sobre la instancia |
| 18 | Subdiálogo `SelectiveSafetyWindow` (`:2997-3018`) | `CenterOwner` por tipo: `DialogWindowCharacterizationTests.cs:547` `QuinceDeLasDieciseisDeclaranCenterOwner` (fijado en `SelectiveSafetyWindow.cs:200`) | PARCIAL | `Owner = this` (`:3016`) sin prueba aunque la costura `SafetyWindowDialog` entrega la instancia |
| 19 | Subdiálogo `SafetyPerPostWindow` de botas (`:2716-2722`) | `CenterOwner` por tipo (`DialogWindowCharacterizationTests.cs:547`; `SelectiveSafetyWindow.cs:1079`) | PARCIAL | Su `Owner` es **la ventana Push Back**, no el diálogo de seguridad abierto encima (se abre desde una sección incrustada, `:2564-2573`). Sin prueba aunque `BootPerPostWindowDialog` entrega la instancia |
| 20 | Subdiálogos sin costura de instancia: `SafetyDefensaGridWindow` (`:2857-2867`), `SafetyTopeGridWindow` (`:3033-3049`), `RackBomWindow` (`:3438`), `RackViewBatchDialog` (`RackViewBatchDialogPresenter.cs:59`), `SaveFileDialog` (`UiSupport.cs:81`), `MessageBox` de descarte (`EditorClosePolicy.cs:77-78`) | `CenterOwner` por tipo para Defensa, Tope y BOM (`DialogWindowCharacterizationTests.cs:547`; `SafetyDefensaGridWindow.cs:87`, `SafetyTopeGridWindow.cs:89`, `RackBomWindow.xaml:9`). Guarda de dueño solo para los subdiálogos que abre **`SelectiveSafetyWindow`** (`DialogWindowCharacterizationTests.cs:587-605`) | PARCIAL / SIN COBERTURA | Ningún `Owner = this` de la ventana Push Back está guardado. Defensa y tope se abren desde secciones incrustadas en el diálogo de seguridad y su dueño es la ventana Push Back. `RackViewBatchDialog` (`CenterOwner` en `RackViewBatchDialog.xaml:5`) no tiene ninguna prueba de ventana. El prompt de descarte no tiene dueño |
| 21 | Subdiálogo `RackFrameConfiguratorWindow` (`:2426`) | `CenterOwner`: `RichEditorCharacterizationTests.cs:277` (constructor de dos argumentos; Push Back usa el de uno, que encadena: `RackFrameConfiguratorWindow.xaml.cs:64-65`) | PARCIAL | `Owner = this` sin prueba aunque `HeaderConfiguratorPresenter` entrega la instancia |
| 22 | Diccionarios de recursos — ventana | Indirecto: `PushBackShellAdoptionTests.cs:71-83` (identidad de estilos por `FindResource`), `:105-120` (tokens), `:163-166` y `RichEditorCharacterizationTests.cs:249-255` (tamaño = tokens ⇒ estilo aplicado); `PushBackActionBarRhythmTests.cs:45-47` (tokens locales) | PARCIAL | Nada afirma `MergedDictionaries` (fuente `AppStyles.xaml`), los valores de los tres tokens locales, que `Style` sea `EditorShellWindowStyle`, ni que el shell **no** haya activado su respaldo (H-12) |
| 23 | Diccionarios de recursos — subdiálogos | `DialogWindowCharacterizationTests.cs:329` (`MergedDictionaries` no vacío, 9 de C); `DialogWindowContractTests.cs:66`, `:163` (estilo del arquetipo C, Defensa incluida) | PARCIAL | Por tipo. `RackViewBatchDialog` y `RackBomWindow` no entran en esas aserciones de recursos |
| 24 | Intención: Insertar (vista del selector) | `PushBackEditorWindowTests.cs:141`, `:502`, `:523`, `:572`; `PushBackOutputGateAfterRecomputeTests.cs:173`, `:194`, `:212` | CUBIERTO | `InsertRequested`, `UpdateOnly = false`, `InsertView`, `InsertSection`, GUID, nombre, tipo del payload |
| 25 | Intención: Actualizar | `PushBackEditorWindowTests.cs:546`; `PushBackOutputGateAfterRecomputeTests.cs:132` | CUBIERTO | `UpdateOnly`, vista nula, sección −1, mismo GUID y `SourceProject` |
| 26 | Intención: botones por vista (Lateral, Frontal E/S, Posterior, Planta) y `FrontalSideBox` | `PushBackCompositeWindowTests.cs:1255` `TheFourFrontalCuts_AreRequestableWithAnExplicitSide` (solo `FrontalSideForTest`) | PARCIAL | Ninguna prueba pulsa los cuatro botones; solo se pulsa `InsertButton` (16 usos) |
| 27 | Intención: «Insertar varias…» (`RequestInsertViews`, `:3314`) | — (el Plugin lo consume: `PushBackRoundTripSourceGuardTests.cs:252-258`) | SIN COBERTURA | Sin costura en la ventana |
| 28 | Escrituras: payload de inserción | `PushBackEditorWindowTests.cs:141-163` (referencia de `SourceProject`), `:546-568`; `PushBackInsertionRequestTests.cs:17` | CUBIERTO | — |
| 29 | Escrituras: biblioteca (`EnsureId` `:3463`, `RackProjectStore.Save` `:3468`) | Compuerta: `PushBackLibrarySaveGateTests.cs:107-212`; contenido por seam: `PushBackEditorWindowTests.cs:593` | PARCIAL | La escritura real no se ejerce (sin costura del `SaveFileDialog`); el efecto de `EnsureId` sobre la identidad no se fija |
| 30 | Escrituras: ningún cierre escribe | filas 10 y 13 (negativas) | CUBIERTO para `Close()` | No para el botón ni para Escape |
| 31 | BOM (salida no persistente) | Compuerta: `PushBackOutputGateAfterRecomputeTests.cs:153`; estado: `PushBackA1EditorTests.cs:149` | PARCIAL | La apertura (y su dueño) no se ejerce |
| 32 | «Puerto» | — | NO APLICA en la ventana clásica | Ver R-1 |

### 3.1 Pruebas de Core que caracterizan o guardan Push Back

- **H-39.** `tests/RackCad.Tests/PushBackPluginSourceGuardTests.cs`: comandos RACKPUSHBACK/RPB (`:47-55`), la ventana, el sobre, el payload y los tres
  servicios de dibujo (`:58-71`), menú tipado (`:90`), RACKEDITAR por el registro (`:106`). No mira `ShowModalWindow` ni `Owner`.
- **H-40.** `tests/RackCad.Tests/PushBackRoundTripSourceGuardTests.cs`: `EditPushBack` usa `new RackPushBackSystemWindow(canInsertInAutoCad: true)` y
  `LoadExisting(...)` (`:191-196`), prevalida antes de redibujar (`:199-209`), valida tipo/vista/sección (`:211-233`), consume
  `window.InsertionRequest.Views` en lote (`:252-258`), menú y comando convergen (`:378`).
- **H-41.** `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs`: caracterización pura del modelo de módulos; las únicas lecturas de la
  ventana son guardas de texto del XAML/código (`Fact3` `:382-404`, `Fact1` `:412-466`); `Fact7` fija que el configurador compartido no tiene
  Aceptar/Cancelar ni `DialogResult` (`:470-488`).
- **H-42.** `tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs:9-15` `Pr1_PushBackPersistsTheSelectedPhysicalPostIndex`: guarda de texto sobre
  `RackPushBackSystemWindow.xaml.cs` (que I-55 «invertiría a propósito», `:5`).
- **H-43.** `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs:290`: reproduce a mano la parte de Application de `LoadExisting`.
- **H-44.** Censos: el único censo de ventanas es UI (`WindowCensusGuardTests.cs`). En `tests/RackCad.Tests` no hay censo ni guarda de `Owner`,
  `CenterOwner`, `IsDefault`, `IsCancel` o `FocusManager` sobre Push Back (las coincidencias de esas palabras están en guardas de otras
  superficies: `CustomProperties*`, `MainMenuStructuralSectionAccessGuardTests`, `StructuralSectionPluginSourceGuardTests`, `Workspace/*Guard*`).

---

## 4. Patrones previos de caracterización

- **H-45.** **I-14** creó el proyecto y `StaTestRunner` sin paquetes nuevos (`StaTestRunner.cs:9-19`). **I-15** fijó identidad y contrato de inserción por
  la sesión (`EditorShellAdoptionTests.cs:22-95`). **I-24** (`docs/initiatives/I-24-ui-tests-editores.md:119-162`): clics reales por `RaiseEvent`
  sobre los manejadores verdaderos (no `session.RequestInsert`), verificación de `InsertRequested`, GUID, nombre, vista, `UpdateOnly`; **un único
  seam interno autorizado**, y cualquier otro es «detención por alcance» (`:150-162`, `:181`). Ayudante: `EditorWindowTestSupport.cs`.
- **H-46.** **I-30/I-31**: pruebas del shell con la plantilla real de `Generic.xaml` aplicada a mano (`EditorVisualShellTests.cs:28-30`,
  `DynamicShellMigrationTests.cs:221-223`), `FindByContent` ampliado a los slots (`EditorWindowTestSupport.cs:132-154`).
- **H-47.** **I-39A/B/C/D** — el patrón anti-«editar para que pase» es triple y está escrito:
  1. **Clases separadas** base / contrato: `RichEditorCharacterizationTests` frente a `RichEditorCloseContractTests` y `RichEditorContractTests`;
     `BoundedEditorCharacterizationTests` frente a `BoundedEditorContractTests`; `DialogWindowCharacterizationTests` frente a `DialogWindowContractTests`.
  2. La prueba base que el cambio autorizado deja obsoleta **no se reescribe**: queda con `[Fact(Skip = BaseEvidence)]` y una constante que apunta a
     la evidencia (`RichEditorCharacterizationTests.cs:34-47`; `DialogWindowCharacterizationTests.cs:40-52`). Si el tipo desaparece, el cuerpo se
     transcribe en la evidencia porque un `Skip` tiene que compilar (`DialogWindowCharacterizationTests.cs:648-659`).
  3. **Evidencia versionada** con el commit en que la base corría en verde (`b61182f` para I-39B, `f6c4d12` para I-39D) y el enfrentamiento aserción
     por aserción (`docs/automation/evidence/I-39B-caracterizacion-base-vs-contrato.md:1-20`).
  Además: «fija lo que HAY, no lo que ADR-0029 querría», con cada incumplimiento comentado como «INCUMPLIMIENTO CARACTERIZADO»
  (`RichEditorCharacterizationTests.cs:27-31`, `:129-132`); aserciones «ciegas a la composición» por rol, contenido y tipo
  (`DialogWindowCharacterizationTests.cs:31-32`).
- **H-48.** **I-64 F4-T2A**: delta de ventana frente a piloto clasificado; lo que cae fuera de las clases conocidas es STOP y se informa, no se fija
  (`Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:18-26`).
- **H-49.** Nombres: sin convención única. I-39B en inglés (`TheSixCloseWithEscape`), I-39A/D en español en PascalCase
  (`LasDieciseisSeConstruyenYNingunaEsUnCascaronVacio`), I-55/G en mayúsculas con prefijo de gate (`G10LegacyEditorControlCharacterizationTests.cs:14-26`).
  Clases con sufijo `CharacterizationTests` para la base y `ContractTests` para lo nuevo.
- **H-50.** `Trait`: casi sin uso en UI; solo `Trait("Gate", "G10-Legacy")` y similares en `G10LegacyEditorControlCharacterizationTests.cs` y
  `G10RecoveryTextContractTests.cs`. Ningún `PushBack*.cs` usa `Trait`. La CI no filtra por `Trait` (§5).

---

## 5. CI

- **H-51.** Un solo flujo `.github/workflows/ci.yml`, en todo `push` y en `workflow_dispatch` con `candidate_sha` (`:15-22`).
- **H-52.** Trabajos:
  - `tests` — «Tests (Domain + Application)», `ubuntu-latest`, 20 min (`:35-42`): `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug`
    **sin `--filter`**, TRX y `--blame-hang 5m`; cobertura solo en dispatch y en trunk (`:131-138`, `:87-96`).
  - `build-ui` — «Build UI (WPF, valida API de Application)», `windows-latest`, 15 min: `dotnet build src/RackCad.UI/RackCad.UI.csproj` (`:193-211`).
  - `ui-tests` — «UI Tests (WPF controls, net8.0-windows)», `windows-latest`, 25 min: `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug`
    **sin `--filter`** («La población de pruebas NO cambia», `:235-244`). No es requisito de `build-plugin` (`:216`).
  - `build-plugin` — «Build Plugin without AutoCAD», `needs: [tests, build-ui]`, 30 min, verifica referencias de AutoCAD por script (`:254-317`).
- **H-53.** Los conteos del Worker `R20261114T162137Z-b48d` (`docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/ci-filter-counts.json`)
  son de **filtros de gate**, no de la CI: `FullyQualifiedName~RackCad.Tests.Workspace` = 2923/2923 y `FullyQualifiedName~RackCad.UI.Tests.Workspace` = 601
  (RED 598 + 3 fallos; GREEN 601). El `relay-record.json` de la misma carpeta registra las corridas completas de CI: Core 18461/18461 y UI
  **2255 seleccionadas, 2238 pasadas, 0 fallos** (`RunRef` `ci-43617317131-*`). La diferencia de 17 coincide con los 17 `Skip =` estáticos de la suite UI.
  Esas corridas son de `ae0a7b40` (HEAD~1 de la base medida), no de `c3ae1c97`.
- **H-54.** **El filtro de gate es por subcadena del nombre completo:** todas las clases UI están en el namespace `RackCad.UI.Tests`, también las de
  `Workspace/` (24 archivos) y las `PushBack*` (44). `~RackCad.UI.Tests.Workspace` selecciona por el **prefijo `Workspace` del nombre de clase**, y no
  incluye ninguna prueba `PushBack*`.

---

## Huecos que T1B debe cubrir (concretos, priorizados)

**Prioridad 1 — caminos de cierre e intención (ADR-0029 D7/D13; INV-19)**

1. **Cierre con trabajo pendiente en la ventana real** (fila 11): escenificar un módulo (p. ej. `ModuleLengthBox` + `LostFocus`, como
   `PushBackModuleEditorWindowTests.cs:114-117`), cerrar y fijar: el prompt se pide una vez; el texto exacto es el de `:3417-3419`; el dueño es `null`
   (sustitución con `Substitute((q, owner) => ...)`, `EditorClosePolicy.cs:65`); rechazar → no se cierra, sigue pendiente y no hay petición; aceptar →
   se cierra sin petición. La clase **tiene que** ir en `[Collection(EditorDiscardPromptCollection.Name)]` (H-35, H-6).
2. **Botón «Cerrar» real** (fila 9): `ClickNamed(w, "CloseButton")` sin y con trabajo pendiente, con las mismas aserciones del punto 1.
3. **Cierre al pedir** (fila 13): Insertar, Actualizar y un botón por vista disparan `Closed`, sin prompt, y con un módulo escenificado lo confirman
   antes y tampoco preguntan.
4. **Escape** (filas 3 y 12): ejercitar la ruta de `IsCancel` y no solo su declaración. Ojo con H-18: `RaiseEvent(ClickEvent)` no reproduce Escape.
   Medir primero, en una ventana no modal, qué gesto reproducible sin paquetes llega a `Close_Click` (tecla Escape por `KeyEventArgs` con
   `HwndSource`, o `Button.OnClick`). El comportamiento en modal se informa como límite si no se puede medir (R-3).
5. **Intenciones sin cobertura** (filas 26, 27, 15): los cuatro botones por vista (`InsertView/InsertSection` y el cambio de `ViewBox.SelectedIndex`,
   `:3253-3261`), la vista frontal del lado B con `FrontalSideBox`, y el estado actual de `InsertBatchButton` (no gobernado por `UpdateButtons` y
   habilitado aun con `canInsertInAutoCad: false`). Fijarlo tal cual, sin corregirlo (D13).

**Prioridad 2 — dueños y ubicación de subdiálogos (D9; D-15 punto 4)**

6. Por **costura con instancia** (sin tocar producto): leer `dialog.Owner` y `dialog.WindowStartupLocation` **al entrar** en la costura, antes de que
   la prueba los reescriba (H-32), para `SelectiveSafetyWindow` (`SafetyWindowDialog`), `SafetyPerPostWindow` de botas (`BootPerPostWindowDialog`; dueño
   = ventana Push Back, no el diálogo de seguridad) y `RackFrameConfiguratorWindow` (`HeaderConfiguratorPresenter`). La ventana padre debe estar
   mostrada fuera de pantalla (H-26, H-31).
7. Por **guarda de fuente** (idioma H-37), los que no tienen costura de instancia: `SafetyDefensaGridWindow` (`:2857-2867`), `SafetyTopeGridWindow`
   (`:3033-3049`), `RackBomWindow` (`:3438`), `RackViewBatchDialogPresenter.TryShow(this, ...)` (`:3311`) con su `Owner = owner` (`RackViewBatchDialogPresenter.cs:59`),
   `PromptSaveToLibrary(this, ...)` (`:3458`) con `ShowDialog(owner)` (`UiSupport.cs:81`) y `EditorClosePolicy.MayClose` sin dueño (`EditorClosePolicy.cs:112`).
8. **`RackViewBatchDialog`**: su `CenterOwner`, `IsDefault` («Insertar cola») e `IsCancel` («Cancelar») construyendo el diálogo con su presentador
   (`RackViewBatchDialogPresenter.cs:28-34`), sin `ShowDialog`.
9. **Dueño de la ventana en sus hosts** (fila 16): guardas de fuente sobre `RackPushBackCommands.cs:47-48`, `:168-170` (`ShowModalWindow`, sin `Owner =`) y
   `EditorModules.cs:114`, `:121` (`Owner = context.Owner` + `ShowDialog`).

**Prioridad 3 — foco, tabulación, Enter y recursos**

10. **Foco inicial** (fila 5): fijar la declaración exacta `FocusManager.FocusedElement="{Binding ElementName=NameBox}"` y que `FindName("NameBox")` es un
    `TextBox` y no una acción (patrón `RichEditorContractTests.cs:56-81`). El foco efectivo, solo si se mide estable (R-4).
11. **Tabulación efectiva** (fila 7): decidir si se fija el orden del árbol lógico y de los slots (estable, sin mostrar) o la secuencia efectiva en una
    ventana mostrada (depende de `Generic.xaml:153-197`, R-4). Fijar también la ausencia de `IsTabStop=false` y de `KeyboardNavigation` en el XAML.
12. **Enter en un campo** (fila 1): `KeyDown`/`PreviewKeyDown` Enter sobre `NameBox` y un `NumericField` → no cierra, no pide y no cambia el estado (H-21, H-22).
13. **Recursos de la ventana** (fila 22): la fuente de `MergedDictionaries`, los valores de los tres tokens locales (`xaml:20-22`), `Style` igual a
    `FindResource("EditorShellWindowStyle")` y, en ventana mostrada, que el shell no activó su respaldo (`RackEditorVisualShell.cs:47-50`).
14. **Biblioteca** (fila 29): `EnsureId` cambia `RackId` en el camino de guardado, solo si se puede medir sin escribir en el perfil (R-10). Si no, guarda de
    fuente del orden `PromptSaveToLibrary` → `EnsureId` → `SetName` → `Save`.

## Riesgos y ambigüedades

- **R-1. «Escrituras del puerto» no tiene referente en la ventana clásica.** El único puerto de edición es el del panel, `IWorkspacePilotEditPort`
  (`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:8-26`), que la ventana no usa. Interpretación que propongo para el paquete (a confirmar
  por el Coordinator): «intención» = `InsertRequested`/`UpdateOnly`/`InsertionRequest` (con `Views`) y el cierre que la acompaña; «escrituras» = lo que la
  ventana deja al llamador (el payload) más la única escritura propia (biblioteca), y la ausencia de escrituras en todos los cierres.
- **R-2. `Owner = this` sobre una ventana no mostrada.** Las pruebas que necesitan una instancia de subdiálogo tienen que mostrar la ventana padre. Las que usan
  «la excepción es la señal» (`PushBackLibrarySaveGateTests.cs:89-95`) no distinguen qué excepción saltó; T1B no debería apoyar una aserción nueva en esa señal.
- **R-3. Escape real y modo modal sin medir.** El ayudante de clic no pasa por `Button.OnClick` (H-18). En modal (`ShowModalWindow`/`ShowDialog`), el
  `IsCancel` de WPF ejecuta además `DialogCancelCommand` después de `Close_Click`, lo que con trabajo pendiente rechazado podría producir un segundo intento de
  cierre. Es una hipótesis de comportamiento de WPF, **no medida** en el repo y sin forma de medirla aquí sin `ShowDialog`. Si T1B no puede observarlo, debe
  declararlo como límite, no fijarlo por analogía.
- **R-4. Foco y tabulación en STA.** El Binding `ElementName` del foco no resuelve hasta mostrar la ventana (H-23); el foco de teclado exige una ventana activa,
  y la ventana fuera de pantalla en el hilo STA compartido puede no activarse; las clases paralelas comparten el dispatcher (H-9). Diez archivos ya muestran
  ventanas **en pantalla** (H-27). Recomendación: fijar declaraciones y orden estructural sin mostrar, y dejar el foco efectivo para el smoke del Owner si no
  es estable.
- **R-5. `CenterOwner` invisible en ventanas mostradas por las pruebas**: el patrón fuera de pantalla lo sustituye por `Manual` (H-26, H-32).
- **R-6. El suelo del prompt oculta pérdidas.** Con `Substitute(_ => true)` permanente (H-35), una regresión que hiciera preguntar o perder en otro sitio
  no rompe ninguna prueba existente. Una tercera clase que sustituya el prompt fuera de la colección reabre la carrera documentada en
  `EditorDiscardPromptCollection.cs:8-15`.
- **R-7. Sin AutoCAD:** el dueño efectivo (ventana principal de AutoCAD vía `ShowModalWindow`) y el comportamiento modal real solo se pueden fijar por guarda de
  fuente o validarse en el smoke manual (`docs/guias/validacion-manual-autocad.md`).
- **R-8. Identidad de estilos frente a la extracción.** `PushBackShellAdoptionTests.cs:76-83` fija **identidad** (`Assert.Same`) contra el diccionario de la
  ventana. Si la superficie extraída resolviera contra otra instancia (el respaldo `ShellResources.Shared`, H-12), esas pruebas cambiarían. INV-19 exige verde
  antes y después, y D-15 dice que la ventana clásica conserva sus diccionarios: T1B debe decidir si su prueba de recursos fija identidad o equivalencia.
- **R-9. Filtro del gate** (H-54): una clase T1B con prefijo `PushBack...` no entra en `~RackCad.UI.Tests.Workspace`. El gate necesita un filtro propio (prefijo
  único). La CI la ejecuta igualmente porque corre la suite UI entera (H-52).
- **R-10. Efectos en el perfil real.** `PromptSaveToLibrary` lee `%APPDATA%\RackCad\settings.json` y crea la carpeta de biblioteca antes del diálogo
  (`UiSupport.cs:66-68`; `src/RackCad.Application/Settings/UserSettings.cs:62`, `:74`). Las pruebas positivas de `PushBackLibrarySaveGateTests` ya llegan ahí.
- **R-11. Réplica de argumentos.** Varias pruebas construyen los subdiálogos copiando los argumentos de la ventana (Grupo E), y la cobertura de C/D es por tipo
  (filas 2, 6, 18-23). Si la extracción cambia esos argumentos, esas pruebas no lo ven. Las costuras con instancia (§1.9) evitan la deriva.
- **R-12. Pruebas tautológicas o vacías no cuentan.** `EditorClosePolicyTests.cs:196-211` no prueba la pregunta real de la ventana. Por D13 no se edita: T1B añade
  una prueba nueva en vez de «arreglar» esta.
- **R-13. No hay costura para «Insertar varias…», BOM ni el diálogo de guardado**, y T1B es «solo pruebas»: sin seam nuevo (precedente I-24, H-45), esos caminos solo
  admiten guarda de fuente o la señal frágil de R-2.
- **R-14. Conteos estáticos frente a ejecución.** Los 446 atributos `PushBack*.cs` y 1893 de la suite no son casos ejecutados; la referencia medida es la de H-53.
