# I-64 — Paquete del contrato de F4-T1B (caracterización de la ventana clásica de Push Back)

**Estado:** v5. **Autorizada para emisión por el Owner** («AUTHORIZE EMISSION OF T1B PACKAGE v4 subject to the interpretations below», disposición `owner-f4t4-disposition-reverify-t1b-t4b.md`). La v5 es la v4 con esas interpretaciones aplicadas (§10.4); se emite después de cerrar F4-T4A2, en el orden de la disposición. La prepara la sesión principal el 2026-11-14. No declara GATE PASS. Se preparó y se revisó sin emitir (`owner-f4t4a2-overnight-decisions.md`, pregunta 4: «Solo paquetes»). **Se emite el 2026-11-14** por la autorización de emisión y ejecución del Owner y del Coordinator «F4-T1B v5 OWNER / COORDINATOR AUTHORIZATION» (`owner-f4t1b-authorization.md`, F4-E58), que confirma sus interpretaciones (§9 y §10.4). **El contrato manda:** conserva la numeración de los invariantes del §6 y añade INV-F4T1B-14 a INV-F4T1B-16 y las paradas F4T1B-C09 y F4T1B-C10.
- **v1:** revisión adversarial independiente, **EMITIBLE TRAS CORRECCIONES (2/13/20)** (`independent-review.md`).
- **v2:** aplicó todos los hallazgos. La comprobación focalizada dio **EMITIBLE TRAS CORRECCIONES (1/7/11)** (`independent-rereview.md`).
- **v3:** aplicó todos los hallazgos de esa comprobación. Su propia comprobación focalizada dio **EMITIBLE TRAS CORRECCIONES (0/2/11)** (`independent-check-v3.md`).
- **v4:** aplica todos los hallazgos de la comprobación de la v3 (§10.3). Todos son de texto, sin rediseño; la sesión la considera emitible cuando el Owner lo ordene.

**Código base:** `c3ae1c97`, la custodia del intento 2 de F4-T4A2.
- El producto es el de `ae0a7b40`; entre los dos no cambia nada en `src/` ni en `tests/`.
- Si F4-T4A2 cambiara producto antes de emitir T1B (por ejemplo, con un intento 3), los hechos del §2 y la clausura del §3 se vuelven a medir sobre la base nueva (`measure_closure.py`).

**Mapas previos** (se custodian con el contrato): `map-1-window-interaction.md`, `map-2-intent-port-closure.md` y `map-3-tests-coverage.md`.
- Las referencias «M1 H-n», «M2 H-n» y «M3 H-n» remiten a sus hechos.
- «IR B-n», «IR I-n» e «IR M-n» remiten a la revisión.

**Presupuesto:** F4-T1B es una tarea contratada propia y empieza en 1 de 3 (F4-E09). Su orden lo fija F4-E51: **antes de F4-T4B**.

## 1. Autoridades

- **El paquete de F4** (`docs/initiatives/I-64-f4-gate-contract-package.md`):
  - la fila de F4-T1B (`:44`): «Caracterización de `RackPushBackSystemWindow` (INV-19, ADR-0029 D13): Enter, Escape, foco inicial, tabulación, caminos de cierre, `Owner`/`CenterOwner`, recursos, intención y escrituras del puerto»; capa «Pruebas»; «Escrituras authored: Ninguna» (columna `:41`);
  - su «Antes de tocar el estado del piloto» quedó **sustituido** por F4-E18 («antes de la primera tarea que modifique un archivo de esa clausura») y después por F4-E51 («antes de F4-T4B»);
  - B1 (`:125`).
- **El Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - D-15 (`:557-582`): el contrato de adaptador (la superficie alojable, la intención separada del cierre, los diálogos cuyo dueño decide el host y el escritor caller-owned) y la caracterización previa por sistema;
  - D-16 (`:584-593`): sin cambio observable en RACKEDITAR ni en los comandos clásicos;
  - INV-19 (`:757`): «caracterización por sistema antes de extraer»; «en verde antes y después»;
  - M-03 (`:996`).
- **ADR-0029** (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md`):
  - D6 (`:110-117`): las acciones declaran su semántica;
  - D7 (`:119-125`): Enter, Escape, «Escape no puede provocar pérdida silenciosa de cambios», la misma política en los cuatro caminos de cierre, y que ningún cierre inserta, actualiza ni guarda;
  - D8 (`:127-130`): el dirty pertenece a un ámbito;
  - D9 (`:132-146`);
  - D13 (`:172-180`);
  - el rechazo de los paquetes de UI (`:194-197`, ADR-0012).
- **El paquete de F4-T2** (`docs/initiatives/I-64-f4-t2-gate-contract-package.md`): la clausura (`:58-66`, «el contrato de la tarea la fija por su ruta real antes de emitirse») y la reubicación de T1B (`:73-77`).
- **El paquete de F4-T4** (`docs/initiatives/I-64-f4-t4-gate-contract-package.md`):
  - §3 (`:172`): T4B es dueña de «la transacción atómica, … las fachadas, las unidades»;
  - §3.1 (`:185-205`);
  - R-A2-06 (`:524`);
  - R-T4-12 (`:581`).
- **Las disposiciones** F4-E18, F4-E31, F4-E43 y F4-E51 (`F4-DRAFTS/f4-dispositions.md`).
- **El protocolo** (`docs/AUTOMATION_PLAN.md`): §16.8 (`:594-599`) y §16.9 (`:601-641`); el perfil del Worker y el contrato base (`docs/initiatives/PROMPT_TEMPLATES.md:158`, `:164`, `:184`).
- **El patrón de caracterización de I-39A/B/C/D** (`tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs:27-47`; `docs/automation/evidence/I-39B-caracterizacion-base-vs-contrato.md`).

## 2. Hechos medidos en `c3ae1c97`

Todo lo de esta sección está medido en el código citado y comprobado por la revisión (IR, «Lo comprobado y correcto»). Lo que es comportamiento por defecto de WPF y no está medido se marca **(inferencia WPF)**: T1B lo mide antes de fijarlo (INV-F4T1B-04).

### 2.1 La ventana y su contrato público

**H-01.** La ventana tiene tres archivos: `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` (792 líneas), `.xaml.cs` (3630) y `RackPushBackSystemWindow.Composite.cs` (911).
- Constructores: `()` equivale a `this(false)` (`.xaml.cs:123-126`), y `(bool canInsertInAutoCad, Func<string> newIdFactory = null)` (`:128`) termina en `LoadNew()`.
- Producción pasa siempre `true` y nunca `newIdFactory`.

**H-02. Sitios de apertura en producción: 4, y ninguno más** (M2 H-1..H-4):

| Sitio | Archivo | Cómo se muestra | `Owner` |
|---|---|---|---|
| RACKPUSHBACK/RPB | `src/RackCad.Plugin/RackPushBackCommands.cs:47-48` | `AcApplication.ShowModalWindow(window)` | no se asigna |
| RACKEDITAR | `RackPushBackCommands.cs:168-170`, con `LoadExisting` | `ShowModalWindow` | no se asigna |
| Menú, rack nuevo | `src/RackCad.UI/Editor/EditorModules.cs:114-115` | `ShowDialog()` | `context.Owner` |
| Menú, desde la biblioteca | `EditorModules.cs:121-125`, con `LoadDesignForNew` | `ShowDialog()` | `context.Owner` |

**H-03. La intención no tiene puerto.**
- No hay evento, delegado ni interfaz inyectada. La ventana anota la petición en su `RackEditorSession` privada y llama a `Close()`.
- **Nunca asigna `DialogResult`**, y `InsertRequestedRaised` no tiene suscriptores en `src/`.
- El contrato público es `.xaml.cs:247-256`. Ningún llamador de producción lee `InsertView`, `InsertSection` ni `SourceProjectToInsert`.
- Los tres llamadores **mutan `System.Name`** del objeto que entrega la ventana (`RackPushBackCommands.cs:98`, `:190`; `RackMenuCommands.cs:135`).

**H-04. Las siete salidas** (M2 H-15):

| # | Gesto | Señal | ¿Cierra? |
|---|---|---|---|
| S1 | «Insertar» → `RequestDraw(view, section, false)` | `InsertRequested=true`, `UpdateOnly=false`, vista del selector y `Views` con 1 dirección | sí (`:3378`) |
| S2 | Los cuatro botones por vista, `InsertViewAt(0..3)` | como S1, con `ViewBox.SelectedIndex` fijado por el botón | sí |
| S3 | «Actualizar» → `RequestDraw(null, -1, true)` | `UpdateOnly=true`, `InsertView=null`, `InsertSection=-1` y `Views` vacío | sí |
| S4 | «Insertar varias…» (`:3294-3321`) | `RequestInsertViews`; `View/Section` = los de `Views[0]` | sí, si se acepta el diálogo de lote (`:3320`) |
| S5 | «Cerrar», Escape, X y Alt+F4 → `OnClosing` (`:3395-3404`) | ninguna | sí, o se cancela |
| S6 | «Guardar en biblioteca» (`:3441-3476`) | sin petición; `EnsureId` y `SetName` | no |
| S7 | «BOM» (`:3423-3439`) | ninguna | no |

**H-05. Guardas de `RequestDraw` que no cierran** (`:3323-3379`):
- Las guardas 1 y 2 vuelven **sin** confirmar el módulo escenificado:
  - (1) «El dibujo en AutoCAD solo está disponible cuando la ventana se abre desde AutoCAD.» (`:3325-3329`);
  - (2) «Solo un sistema abierto con RACKEDITAR puede actualizarse en sitio.» (`:3331-3335`).
- Después se confirma el módulo escenificado (`:3346-3349`) y se recalcula. Entonces actúan:
  - (3) «Corrige los datos: no se puede insertar un modelo inválido.» (`:3352-3356`);
  - (4) el diagnóstico bloqueante compuesto del escenario. El literal «Corrige los datos: no se puede dibujar un modelo bloqueado.» solo cubre el caso sin diagnóstico (`:3360-3363`, `:3507-3521`).
- El patrón «confirmar y no salir» de (3) y (4) es una decisión de I-40.

**H-06. Habilitación** (`UpdateButtons`, `:3523-3570`):
- `InsertButton` = `canInsertInAutoCad && canOutput`, y los cuatro botones por vista lo copian;
- `UpdateButton` = lo mismo, más `isEditingExisting`;
- `BomButton` = `canOutput`; `SaveLibraryButton` = `currentInputsAreValid`; `RestoreButton` = `lastComputation != null`.
- **`InsertBatchButton` no aparece:** está siempre habilitado.
- `InsertBatch_Click` (`:3294-3299`) confirma el módulo, recalcula y vuelve **sin diálogo** si la entrada es inválida o la salida está bloqueada. **Nunca comprueba `canInsertInAutoCad`.**
- Los tooltips se reescriben en el código (`:3530-3569`). «Cerrar» no tiene tooltip.

### 2.2 Teclado, foco y tabulación

**H-07. Enter.**
- Ningún botón es `IsDefault`.
- No hay manejadores de teclado ni `InputBindings` en ninguno de los **10** archivos de G1.
- `NumericField` revalida en cada pulsación, y la ventana compromete el valor en `LostFocus` (M1 H-5).
- `NameBox` escribe el nombre (recortado, `:1203`) en la identidad en cada pulsación.

**H-08. Escape.**
- Solo `CloseButton` («Cerrar», `.xaml:789`) es `IsCancel`, con `Close_Click` → `Close()` (`.xaml.cs:3381`).
- **(Inferencia WPF)** En `ShowDialog`, un botón `IsCancel` sin `Command` ejecuta además `DialogCancelCommand`, que asigna `DialogResult=false` e intenta otro cierre. Con un «No» podría salir un segundo aviso.
- **(Inferencia WPF)** El ámbito de una tecla de acceso es por defecto su `PresentationSource`. `AccessKeyManager.ProcessKey` solo llega al botón con el ámbito correcto (IR I-7).
- **(Inferencia WPF)** `ButtonAutomationPeer.Invoke` encola el clic con `BeginInvoke` a prioridad `Input` (IR I-7).
- **(Inferencia WPF)** Un `KeyDown` elevado con `RaiseEvent` no pasa por `InputManager` y nunca activa `IsDefault` ni `IsCancel` (IR I-7).

**H-09. Foco inicial:** `FocusManager.FocusedElement="{Binding ElementName=NameBox}"` (`.xaml:9`); no hay ningún `Focus()`.

**H-10. Tabulación.**
- Nadie fija `TabIndex`, `IsTabStop`, `KeyboardNavigation` ni `Focusable` en G1, `Shell/`, `Themes/` ni `Controls/`.
- La plantilla (`Themes/Generic.xaml:151-196`) pone primero `PART_ActionBar`, luego `PART_StatusHost`, la zona lateral y `PART_WorkArea`.
- **El ayudante `EditorWindowTestSupport.Descendants` recorre los slots en otro orden** (lateral → matriz → vista previa → estado → acciones, `:159-170`), así que no sirve como oráculo de la tabulación (IR I-9).

### 2.3 Cierre y su política

**H-11.** `OnClosing` llama a `EditorClosePolicy.MayClose(PendingWork())` salvo que `InsertRequested` sea verdadero.
- El único ámbito declarado es la edición de módulo escenificada, con la pregunta «Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?» (`:3417-3419`).
- El texto sin comprometer de un `NumericField` y las ediciones de la matriz, la seguridad y los topes **no cuentan** (`:3406-3416`, D8).

**H-12.** `MayClose` llama a `Confirm(pending.Question)` sin dueño (`Shell/EditorClosePolicy.cs:108-113`).
- Producción usa `MessageBox.Show(question, "Confirmar", YesNo, Warning)` por la rama `owner == null`.
- **(Inferencia WPF)** El dueño efectivo de ese `MessageBox` es la ventana activa.

**H-13.** El suelo de la suite es `EditorDiscardPrompt.Substitute(_ => true)` (`StaTestRunner.cs:57`), permanente.
- Las clases que lo sustituyen van en `EditorDiscardPromptCollection` (`DisableParallelization = true`).

**H-14.** `EditorWindowTestSupport.Click` eleva `ButtonBase.ClickEvent` (`:110-111`) y **no** pasa por `Button.OnClick`.
- En `tests/` no se usa `AccessKeyManager`, `AutomationPeer`, `IInvokeProvider` ni `PushFrame`.

### 2.4 Dueños y ubicación

**H-15. La ventana:** `CenterOwner` (`.xaml:8`); no se asigna `Owner` (H-02). Su tamaño sale del estilo `EditorShellWindowStyle`.

**H-16. Los subdiálogos:**

| # | Diálogo | `Owner` | Ubicación | Costura con la instancia real |
|---|---|---|---|---|
| S-1 | Aviso de descarte (`MessageBox`) | ninguno WPF explícito | n/a | `EditorDiscardPrompt.Substitute(Func<string,Window,bool>)` |
| S-2 | `RackFrameConfiguratorWindow` (`.xaml.cs:2426`) | `this` | `CenterOwner` | `HeaderConfiguratorPresenter` (`:2410`) |
| S-3 | `SelectiveSafetyWindow` (`:3016`) | `this` | `CenterOwner` | `SafetyWindowDialog` (`:3028`) |
| S-4 | `SafetyPerPostWindow`, de botas (`:2721`), con S-3 abierto | `this` = Push Back, no S-3 | `CenterOwner` | `BootPerPostWindowDialog` (`:2700`) |
| S-5 | `SafetyDefensaGridWindow` (`:2865`) | `this` | `CenterOwner` | ninguna |
| S-6 | `SafetyTopeGridWindow` (`:3047`) | `this` | `CenterOwner` | ninguna |
| S-7 | `RackBomWindow` (`:3438`) | `this` | `CenterOwner` | ninguna |
| S-8 | `RackViewBatchDialog` vía `TryShow(this, …)` (`:3311`; `Views/RackViewBatchDialogPresenter.cs:52-63`) | `owner` | `CenterOwner` | ninguna |
| S-9 | `SaveFileDialog` vía `PromptSaveToLibrary(this, …)` (`:3458`; `UiSupport.cs:64-82`) | `ShowDialog(owner)` | n/a | ninguna |

**H-17.**
- `Owner = this` lanza si la ventana nunca se mostró.
- El patrón `Shown()` de las pruebas sobrescribe `WindowStartupLocation`, y la costura `SafetyWindowDialog` de las pruebas existentes reescribe también la del subdiálogo.
- `PushBackLibrarySaveGateTests.cs:89-95` usa como señal que `SaveFileDialog.ShowDialog(owner)` **lanza** con un dueño nunca mostrado. Si un mutante quitara ese dueño, se abriría un diálogo real (IR I-2).

**H-18. El configurador real** (IR I-6):
- su constructor se suscribe a `Dispatcher.UnhandledException` (`RackFrames/RackFrameConfiguratorWindow.xaml.cs:73`), cuyo manejador marca `Handled = true` (`:756-760`);
- se desuscribe y escribe la disposición en `%LOCALAPPDATA%` solo en `OnClosed` (`:602-606`);
- la suite existente ya lo cierra en cada CI, y por tanto ya hace esa escritura (`tests/RackCad.UI.Tests/PushBackHeaderAuthorityWindowTests.cs:68-74`);
- **(No medido)** si cerrar la ventana dueña cierra una poseída que nunca se mostró.

### 2.5 Recursos

**H-19.** `Window.Resources` fusiona `/RackCad.UI;component/Themes/AppStyles.xaml` y declara tres `Thickness`: `ActionGap` `0,0,6,4`, `ActionGroupGap` `0,0,16,4` y `ActionLast` `0,0,0,4` (`.xaml:10-22`).
- El shell fusiona `ShellResources.Shared` solo si `ShellZoneSpacing` no resuelve (`Shell/RackEditorVisualShell.cs:45-53`).

### 2.6 Escrituras

**H-20. Las de la ventana**, sin doble:
- **Guardar en biblioteca:** `PromptSaveToLibrary` crea la carpeta **antes** del diálogo; después vienen `EnsureId`, `SetName` y `RackProjectStore.Save` (`:3458-3468`). El contenido es observable con `BuildLibraryProjectForTest()`.
- **El configurador:** escribe en `OnClosed` (H-18).
- **El BOM:** exporta tras un `SaveFileDialog`.

**H-21. Las del DWG**, siempre en el llamador y después del cierre:
- **RACKPUSHBACK:** `window.InsertionRequest is PushBackInsertionRequest request` (`:50`), luego `View/Section`, y `DrawPushBackView`. «Insertar varias…» dibujaría solo `Views[0]` (inferencia del código).
- **RACKEDITAR:** prevalida; prepara el lote con `InsertionRequest.Views` (`PrepareAll`, `:248-260`) antes del primer `RedrawInPlace(…, regen:false)`; después `SyncName` < `EraseViewBlocks` < un único `Regen()` < `RackViewBatchExecution.Run`, este último solo si `!UpdateOnly`.
- **El menú:** `RackViewBatchProducts.PushBack` y `PlacePushBack(regen:false)`, sin pasar por las tres fachadas.

**H-22. Las tres fachadas** (`src/RackCad.Plugin/Systems/PushBack/*DrawService.cs`):
- públicos `DrawAndPlace` y `RedrawInPlace`, con las firmas de M2 H-29;
- internos caller-owned `PrepareRedraw` y `RedrawInTransaction`, hoy **sin ningún llamador en producción**.

Ese hecho es para T4B, que añadirá el primero (§3 del paquete de T4): **T1B no lo congela** (CT-27).

### 2.7 Lo que ya está cubierto, y CI

**H-23.** La matriz de M3 §3:
- **cubierto:** `Close()` sin trabajo pendiente, `CenterOwner` de la ventana, Insertar y Actualizar con su payload, y las salidas bloqueadas;
- **parcial:** Enter, Escape, el foco, la tabulación y los recursos;
- **sin cobertura:** el resto de lo que pide INV-19.

`EditorClosePolicyTests.cs:196` es tautológica. Las cuatro `Skip = BaseEvidence` de `RichEditorCharacterizationTests.cs` describen el estado anterior a I-39B. Ninguna de ellas se edita ni se reactiva.

**H-24. CI** (`.github/workflows/ci.yml`):
- `tests` corre en Ubuntu y `ui-tests` en Windows, con las suites **enteras**, `--blame-hang` de 5 minutos (`:239-244`) y sin filtro;
- `build-plugin` tiene `needs: [tests, build-ui]` (`:254-258`);
- los espacios de nombres raíz son `RackCad.UI.Tests` y `RackCad.Tests`;
- ningún nombre de clase empieza por `PushBackClassic`;
- el repositorio trabaja con `core.autocrlf=true`.

## 3. La clausura de la ventana, por ruta real (D-T1B-02)

**La regla:**
- **dentro de la clausura:** los grupos declarados en F4-T2 y las dependencias **directas y transitivas** de esos grupos dentro de `src/RackCad.UI/`, medidas por referencia de tipos hasta un punto fijo (`measure_closure.py`, salida en `closure-measured.json`);
- `Editor/EditorModules.cs` está en la clausura por su carpeta, pero no propaga: es el registro de módulos, que llama a la ventana y no es dependencia suya;
- **fuera de la clausura:** `Pilot/` (F4-E51), `src/RackCad.Domain/**` y los archivos compartidos del Plugin donde solo un fragmento es de Push Back.

| Grupo | Rutas | Archivos |
|---|---|---|
| G1 | `src/RackCad.UI/Systems/PushBack/**` | 10 |
| G2 | `src/RackCad.UI/{Controls,Shell,Themes,Editor,Preview}/**` | 17 + 9 + 2 + 12 + 3 = 43 |
| G3 | `RackFrames/RackFrameConfiguratorWindow.xaml(.cs)`, `SelectiveSafetyWindow.cs`, `SafetyDefensaGridWindow.cs`, `SafetyTopeGridWindow.cs`, `RackBomWindow.xaml(.cs)`, `Views/RackViewBatchDialog.xaml(.cs)` y `Views/RackViewBatchDialogPresenter.cs` | **10** |
| G4 | `src/RackCad.Application/Systems/PushBack/**` sin `Pilot/` | 52 |
| G5 | `src/RackCad.Plugin/Systems/PushBack/*.cs` (3), `RackPushBackCommands.cs` y `KindHandlers/PushBackKindHandler.cs` | 5 |
| G6 (medido) | `UiSupport.cs`, `CatalogOption.cs`, `SafetyDefenseFace.cs`, `ObservableObject.cs`, `EnumDisplayConverter.cs`, `PreviewCanvasPainter.cs`, `SafetyGuiaEntradaGridWindow.cs`, `SafetyDesviadorGridWindow.cs`, `SafetyParrillaGridWindow.cs` y 8 de `RackFrames/` (`BracingSegmentEditorRow.cs`, `ConfiguratorNavigationItem.cs`, `FrameExceptionEditorRow.cs`, `FrameExceptionGroup.cs`, `HorizontalEditorRow.cs`, `RackFrameConfiguratorLayoutStore.cs`, `RackFrameConfiguratorViewModel.cs`, `RackFrameEngineeringPreviewLayout.cs`), todos bajo `src/RackCad.UI/` | 17 |

**Fragmentos compartidos fuera de la clausura:** `src/RackCad.Plugin/RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs` y `Views/RackViewPlacement.cs`.
- Su parte de Push Back se **caracteriza** (CT-26).
- No entran en la clausura porque el resto de esos archivos pertenece a otros sistemas.

**Para qué sirve la clausura:** T1B no modifica la clausura **en neto**. Los mutantes del RED tocan archivos de G1, G2, G3, G5 y G6 y los fragmentos compartidos del Plugin, y el GREEN los revierte (IR R-M-10, V-M-11). La fija como referencia de INV-19 y de la regla de orden de F4-E51. **El alcance de T4B lo decide el paquete de T4B** (§8, IR I-5).

## 4. Decisiones de F4-T1B

### D-T1B-01. Qué son «la intención y las escrituras del puerto»

En la ventana clásica no hay ningún puerto como tipo (H-03). INV-19 se cumple así:

| Elemento | Qué se fija | Dónde |
|---|---|---|
| **Intención** | Las salidas de H-04 con su señal exacta en el contrato público, el cierre que la acompaña y lo que no cierra | UI.Tests, en ejecución |
| **Escrituras de la ventana** | El orden `PromptSaveToLibrary(this, …)` → `EnsureId` → `SetName` → `Save(…)` y la escritura del configurador en `OnClosed`. El contenido de la biblioteca ya está cubierto | UI.Tests, guarda de fuente |
| **Escrituras del llamador (DWG)** | Lo que lee cada llamador; la mutación de `System.Name`; el orden de escritura por camino; las firmas de las fachadas; y que los caminos clásicos no usan sus caller-owned | RackCad.Tests, guarda de fuente |
| **Ningún cierre escribe** | S5 no deja petición ni materializa nada. S6 y S7 solo se ejercen en sus caminos bloqueados, que ya están cubiertos: el camino libre llega a un modal o a una escritura (IR M-14) | UI.Tests, en ejecución |

### D-T1B-02. La clausura por ruta real

La del §3. G6 es una ampliación medida y se declara al Owner (§9, I-3).

### D-T1B-03. El RED de una caracterización: mutantes revertidos

**El problema.** El protocolo exige RED en la primera delegación (§16.8), y el GREEN no puede tocar `RT ∪ ChainRedFiles`. Una caracterización, en cambio, pasa sobre el producto sin cambios.

**La decisión** (interpretación declarada, §9 I-2). La revisión la encuentra compatible con §16.8 y §16.9 y honesta respecto a D13; no ve una alternativa mejor dentro del protocolo.
- **El commit RED** contiene todas las pruebas definitivas y los **mutantes** deliberados del producto, solo en los archivos exactos de `MutantScope`.
  - Su mensaje dice «RED con mutantes deliberados, revertidos en el GREEN».
  - Ese SHA no sirve nunca para un smoke ni para la validación del Owner.
- **Con los mutantes aplicados a la vez** (el RED es su conjunción), todas las pruebas nuevas fallan por aserción.
  - **Granularidad:** `Failed == Selected` se cuenta por resultado del TRX, es decir, por método o por fila de una `Theory` (IR R-B-1).
  - El criterio mecánico es INV-F4T1B-02.
- **Pruebas existentes que fallen en el RED:** se admiten, solo por los mutantes. Se miden como **unión**, con todos los mutantes aplicados, y se atribuyen a cada mutante por un análisis declarado (IR R-M-9). El GREEN las devuelve a verde.
- **El commit GREEN** es el inverso exacto:
  - `git diff RedSha..CurrentSha` toca solo rutas de `MutantScope` en `src/` y ninguna de `tests/`;
  - **`git diff ChainBaseSha..CurrentSha -- src` queda vacío** (IR R-I-7).
- **La CI del RED:**
  - `tests` y `ui-tests` en `failure`, sin `blame-hang` ni timeout;
  - `build-ui` en `success`;
  - `build-plugin` **omitido** por su `needs`.
- **La compilación de los mutantes del Plugin** la prueba el build Release local de la solución, con el Plugin incluido, sobre el RED (`ExpectedEvidence`; IR M-10).

**`MutantScope`**, por archivo exacto, con la clase de mutante permitida en cada uno:

| Archivo | Clase de mutante permitida |
|---|---|
| `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` | de comportamiento |
| `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | de comportamiento |
| `src/RackCad.UI/Views/RackViewBatchDialog.xaml` | de comportamiento (`IsDefault`, `IsCancel`, ubicación) |
| `src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs`, `src/RackCad.UI/UiSupport.cs`, `src/RackCad.UI/Editor/EditorModules.cs` y `src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs` | **solo textual con comportamiento idéntico** |
| `src/RackCad.Plugin/RackPushBackCommands.cs`, `RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs`, `Views/RackViewPlacement.cs` y `Systems/PushBack/PushBack{System,Frontal,Planta}DrawService.cs` | **solo textual con comportamiento idéntico** |

**Las reglas de los mutantes** (IR I-2, R-B-1, R-I-2, R-I-3):
- **(a)** Un mutante textual de comportamiento idéntico rompe una guarda de fuente sin cambiar nada observable. Puede ser:
  - un argumento con nombre, o un paréntesis o una conversión redundante;
  - una expresión equivalente;
  - una **cualificación redundante** (`this.`, el espacio de nombres o `global::`);
  - **código muerto**: un **método** privado que nunca se llama, o una rama que en la práctica no se alcanza. Ninguno de los dos da aviso del compilador: sin CS0162, CS0168, CS0169, CS0219, CS0414 ni CS8321. Un campo sin uso o una función local no valen (V-M-3).
- **(b)** Un mutante de comportamiento, incluso en los archivos que lo permiten:
  - **no toca:**
    - una asignación de `Owner`;
    - un `ShowDialog` ni un `MessageBox`;
    - `EditorDiscardPrompt`;
    - la comprobación de nulo de una costura;
  - **no desvía** un gesto (`IsDefault`, `IsCancel` o una tecla de acceso) hacia un botón cuyo manejador abre un diálogo o escribe;
  - **no retira ni debilita** una guarda que hoy separa una prueba de un modal sin costura o de una escritura: la validez, el bloqueo, `canInsertInAutoCad`;
  - **no provoca ninguna escritura de datos del usuario ni locales** (orden del Owner: «incapable of writing unexpected user/local data»);
  - **no edita el cuerpo del constructor** ni hace que la construcción lance, y no retira recursos del XAML (V-M-9);
  - **no cambia un camino de cierre** de modo que una prueba de la suite deje un modal abierto. Esto incluye las pruebas de T1B y el respaldo de CT-04.
- **(c)** La CI del RED termina sin `blame-hang` ni timeout. Si no termina así, es REWORK.
- **Para el Controller** (IR R-I-3, V-M-8):
  - las filas de la tabla prevista son conformes a (a) y (b) **por construcción**;
  - (c) se juzga siempre sobre la CI del RED;
  - la tabla es exhaustiva (v5): todo mutante de la entrega instancia una fila; un mutante que no instancia ninguna es F4T1B-C04.
  - La tabla de mutantes de la entrega marca cada mutante con la fila prevista que instancia, o con «fuera de tabla».
- **La tabla se razona mutante a mutante, pero el RED es su conjunción:** el Worker comprueba que, con todos los mutantes aplicados, ningún mutante anula a otro (V-M-1).

**La tabla prevista** «CT → archivo → mutante» (IR B-1, R-B-1). Demuestra que cada CT obligatorio tiene al menos un mutante posible dentro del ámbito.
- **La tabla es exhaustiva** (v5, orden del Owner: «Every RED mutation must be explicitly enumerated in the contract»).
  - El Worker solo puede usar los mutantes de esta tabla, o las alternativas que la propia fila enumera.
  - **Un mutante fuera de la tabla es F4T1B-C04.**
  - Si un mutante previsto no basta, es una parada y no una improvisación.
- Una `Theory` solo está permitida si cada una de sus filas tiene su mutante; si no, se escribe un `Fact`.

| CT | Archivo | Mutante previsto |
|---|---|---|
| CT-01 | `.xaml` | `IsDefault="True"` **solo en `RestoreButton`** (`Restore_Click` → `LoadFromModel` → `Recompute()`), nunca en un botón que abre un diálogo o escribe (IR R-I-2, V-M-1) |
| CT-02, CT-03 | `.xaml.cs` | El texto de la pregunta de `PendingWork()`, que matan **todos** los casos con trabajo pendiente, porque cada uno asevera la cuenta y el texto de la pregunta (V-M-1); `Close_Click` sin efecto, para los casos sin trabajo pendiente. El modal de CT-04 lo cubre su respaldo |
| CT-02 (respaldo) | `.xaml` | Quitar `IsCancel` de `CloseButton`, o ponerlo en `RestoreButton` (V-M-10) |
| CT-04 | `.xaml.cs` | El texto de la pregunta de `PendingWork()`, que cambia el texto o la cuenta medidos (IR R-I-5) |
| CT-05 | `.xaml.cs` | `Name_Changed` sin `Trim()` |
| CT-06 | `.xaml.cs` | Sin el `Close()` de `RequestDraw` (`:3378`), que no deja ningún modal abierto en la suite |
| CT-07 | `.xaml.cs` | **Una rotación de los cuatro índices** de `InsertViewAt`, que mata las cuatro filas si es una `Theory` |
| CT-08 | `.xaml.cs` | El texto de una guarda; el orden «confirmar antes» de las guardas 1 y 2 |
| CT-09 | `.xaml.cs` | El texto de un tooltip; `InsertBatchButton.IsEnabled = canOutput` en `UpdateButtons` |
| CT-10 | `.xaml` y `.xaml.cs` | `ElementName` del foco apuntando a otro control existente; el texto que se asigna a `NameBox` en `LoadExisting` |
| CT-11 | `.xaml` | Dos controles del panel lateral intercambiados, conservando su `x:Name` (orden dentro del slot) |
| CT-12 | `.xaml` | `Title` |
| CT-13 | `.xaml.cs` | `WindowStartupLocation = Manual` en el subdiálogo antes de entregarlo a la costura (no se toca `Owner`) |
| CT-14 | `Views/RackViewBatchDialog.xaml` | `IsDefault` o `IsCancel` en otro botón; la ubicación |
| CT-15 | `.xaml` | El valor de `ActionGap`; un diccionario fusionado en los recursos del shell, frente al oráculo «`Shell.Resources.MergedDictionaries.Count == 0`» (IR R-M-6) |
| CT-16 | `.xaml.cs` | Textuales: `(window.Configuration) ?? copy` y `{ Owner = (Window)this }` en `ShowHeaderConfigurator` (v5) |
| CT-17 | `.xaml.cs` | Un recálculo en la rama de cancelar de Seguridad |
| CT-18 (si se escribe) | `.xaml.cs` | `SaveLibrary_Click` marca la identidad (`EnsureId`) antes de la guarda de validez (IR R-M-5) |
| CT-19 | `.xaml.cs` | Código muerto: un miembro privado sin uso con `.Focus(`, `DialogResult` o una suscripción `session.InsertRequestedRaised +=` |
| CT-20 | `.xaml.cs`, `Views/RackViewBatchDialogPresenter.cs` y `UiSupport.cs` | `{ Owner = (Window)this }`, `TryShow((Window)this`, en el presentador `Owner = (Window)owner` (`Views/RackViewBatchDialogPresenter.cs:59`, V-M-10) y `ShowDialog((System.Windows.Window)owner)` (todos textuales; `UiSupport.cs` no importa `System.Windows`, IR R-M-1) |
| CT-21 | `Editor/EditorModules.cs` | `Owner = (context.Owner)` (textual) |
| CT-22 | `.xaml.cs` y `RackFrames/RackFrameConfiguratorWindow.xaml.cs` | `PromptSaveToLibrary((Window)this`, `this.SaveCurrentLayout()` (textuales) |
| CT-23 | `.xaml.cs` | `InsertBatch_Click` sin confirmar el módulo, con la ventana **sin mostrar** (IR R-I-2) |
| CT-24 | `RackPushBackCommands.cs` | `ShowModalWindow((System.Windows.Window)window)`; `RackPushBackCommands.DrawPushBackView(` o `global::RackCad.Plugin.RackPushBackCommands.DrawPushBackView(`, textuales. Nunca `this.`, porque el método es estático: CS0176 (V-M-2) |
| CT-25 | `RackPushBackCommands.cs` | `regen: (false)` (textual) |
| CT-26 | `RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs` y `Views/RackViewPlacement.cs` | Una cualificación redundante; `regen: (false)` (textuales) |
| CT-27 | `Systems/PushBack/*DrawService.cs` y `RackPushBackCommands.cs` | (1) Un valor por defecto entre paréntesis en una firma. (2) Código muerto: un miembro privado sin uso en `RackPushBackCommands.cs` que llama a un caller-owned. Las dos son textuales |
| CT-28 | `.xaml.cs` | Un recálculo en `OnClosing` |

**El patrón de la guarda de fuente** tiene que ser lo bastante preciso como para que el mutante textual previsto lo rompa.

**Aserciones ancla** (IR R-B-1, V-I-1): algunos subpuntos obligatorios no tienen ningún mutante posible dentro de las reglas.
- **Regla general:** es ancla admisible toda aserción cuyo observable no puede cambiar ningún mutante permitido por (a) y (b). Por ejemplo:
  - un `Owner` o el dueño del aviso;
  - el tipo de un control con manejadores;
  - código solo de prueba;
  - un observable decidido en un archivo fuera de `MutantScope`.
- **Dónde va:** siempre declarada, y **en el mismo método** que una aserción que sí se detecta. Nunca en un método propio.
- **La lista que sigue no es exhaustiva.** La entrega declara todas las aserciones sin mutante propio.

Casos conocidos:
- **CT-12, «`Owner` en `null`»:** la regla (b) prohíbe tocar `Owner`. Su ancla es `Title`.
- **CT-13, el `Owner` de S-3 y de S-4** (también con S-3 abierto): la regla (b) prohíbe tocar `Owner`. Su ancla es `CenterOwner`, con el mutante de ubicación.
- **CT-16:** es una guarda de fuente, y su mutante es textual (véase la tabla).
- **CT-02 y CT-03, el dueño `null` del aviso:** lo decide `Shell/EditorClosePolicy.cs:112`, fuera de `MutantScope`. Su ancla es el texto y la cuenta de la pregunta.
- **CT-10, «`NameBox` es un `TextBox`»:** su ancla es la declaración del foco.
- **La ventana sonda opcional de Enter** (CT-01): si se escribe, va en el método de CT-01 como ancla.
- **CT-11 (2), el orden de las zonas:** vive en `Themes/Generic.xaml`, fuera de `MutantScope`. Su ancla es el orden dentro del slot.
- **CT-24, «sin `Owner =`»:** añadir una asignación no es textual con comportamiento idéntico. Su ancla es `ShowModalWindow`.
- **CT-27 (2), en lo que toca a `KindHandlers/PushBackKindHandler.cs`:** ese archivo no está en `MutantScope`. Su ancla es la parte de (2) sobre `RackPushBackCommands.cs`.

**Correcciones** (IR I-3, R-I-7):
- Toda corrección que exija RED según §16.8 (porque `ChainRedSha` es nulo o porque toca `ChainRedFiles`) **vuelve a aplicar todos los mutantes de los filtros con `ExpectRed`** de su contrato.
- `Failed == Selected` vale para esos filtros, y el GREEN vuelve a ser su inverso exacto.
- **Core pasa a `ExpectRed` false** solo si no se vuelven a aplicar los mutantes del Plugin, y se declara en el contrato de la corrección, como en F4-E55 (V-M-11).
  - En ese caso, la CI del RED tiene `tests` en `success` y `build-plugin` **sí** corre.
  - El texto de §16.9 #9 («el job Core en `failure`») se lee entonces con el `ExpectRed` del contrato. Así lo aceptó la verificación del intento 2 de F4-T4A2, con UI en `failure` y Core en `success` (F4-E56).
- **Las comprobaciones de producto se anclan a `ChainBaseSha`** (la base de la primera delegación), no al `BaseSha` de la corrección, que es el GREEN anterior. Así una corrección que cambia 2 de las 7 pruebas sigue cumpliendo INV-F4T1B-01.

**Por qué no las alternativas:**
- un RED dispensado necesitaría una dispensa del Owner sobre el `RedPart` de Ci y de Tests;
- un esqueleto con `Assert.Fail` obliga a que el GREEN toque las pruebas del RED.

**El coste, declarado:** un commit intermedio con el producto mutado queda en la historia de la rama.

### D-T1B-04. Los puntos que se caracterizan (CT-01 a CT-28)

- Cada CT tiene **al menos un método propio** en las clases del D-T1B-05.
- **«Ejecución»:** ejerce el código real. **«Fuente»:** es una guarda sobre el texto, que es la última opción (INV-F4T1B-08).
- **Los gestos de teclado se validan con un control positivo** (IR I-7, R-I-4):
  - **Es una medición sobre `BaseSha`**, que decide el diseño antes del RED. En la prueba versionada se convierte en una aserción incondicional: **sin ramas** del tipo «si falla el control, tomar el respaldo».
  - **Para Escape**, el control es que el mismo gesto, sin trabajo pendiente, cierra la ventana.
  - **Para Enter**, el control es la misma ruta: `ProcessKey` sobre el mismo `PresentationSource` llega con «\x1B» a un botón `IsCancel` (el control de CT-02). Opcionalmente, una `Window` sonda privada de la prueba, con un botón `IsDefault`, demuestra que «\r» llega por esa ruta a un `IsDefault`.
  - El valor de vuelta de `ProcessKey` («hay más coincidencias») **no es un oráculo**.
  - **Lo que caracteriza:** la ruta de la tecla de acceso, no el enrutado desde el elemento con foco (IR R-M-8).
- **Toda precondición es una aserción** (IR R-I-1). Por ejemplo, `Assert.True(button.IsEnabled)` antes del peer y `Assert.NotNull(PresentationSource.FromVisual(w))`.
- **Ningún `Assert` va dentro de un delegado que ejecuta el producto** (la sustitución del aviso y las costuras): el valor se captura dentro y se asevera fuera (IR R-I-5, R-I-6).
- **El código de medición local** del foco, de la tabulación efectiva y de CT-04 es temporal: vive en los siete archivos mientras se mide y **no entra en ningún commit** (IR R-M-7).

| CT | Punto | Qué fija (comportamiento ACTUAL) | Tipo | Obligatorio |
|---|---|---|---|---|
| CT-01 | Enter | `AccessKeyManager.ProcessKey(PresentationSource.FromVisual(w), "\r", false)` con la ventana mostrada no cierra la ventana, no pide nada y no cambia `RecomputePassesForTest`. En todo el árbol (también lo que genera la matriz) no hay ningún `Button` `IsDefault`. Un `KeyDown` elevado con `RaiseEvent` **no** cuenta como evidencia. <br>**Respaldo** (V-I-2), decidido por la misma medición sobre `BaseSha` que el de CT-02: si el control de Escape no pasa, CT-01 conserva su método y fija solo la declaración (ningún `IsDefault` en el árbol, con el mutante `IsDefault` en `RestoreButton`). El gesto de Enter pasa a `KnownLimitations` y a D-T1B-07, sin STOP. La sonda, si existe, no sustituye al control sobre la ventana de Push Back | Ejecución | Sí |
| CT-02 | Escape, no modal | Con el gesto medido, `ProcessKey(PresentationSource.FromVisual(w), "\x1B", false)`, y su control positivo: <br>• **sin trabajo pendiente:** se cierra sin preguntar y sin petición; <br>• **con un módulo escenificado:** pregunta **una vez**, con el texto exacto de H-11 y el dueño `null`; <br>• **«No»:** sigue abierta, con el trabajo pendiente y sin petición; <br>• **«Sí»:** se cierra sin petición y la edición no se confirma. <br>**Respaldo** (decidido por la medición sobre `BaseSha`, no por una rama): si ningún gesto de Escape supera el control positivo, **CT-02 conserva su método** y fija que `CloseButton` es el único `IsCancel`, con su mutante. Los cuatro casos quedan en CT-03 y el límite en `KnownLimitations`, sin STOP (IR R-M-4) | Ejecución | Sí |
| CT-03 | «Cerrar» real | Los mismos cuatro casos de CT-02, pulsando `CloseButton` por `ButtonAutomationPeer` + `IInvokeProvider.Invoke`. El dispatcher se bombea por prioridad, dentro de la colección, sin esperas fijas | Ejecución | Sí |
| CT-04 | Modal (H-08) | **Primero se mide** con código temporal (IR R-M-11): un `ShowDialog` real con las acciones encoladas antes. Con un módulo escenificado y «No» por «Cerrar» (y por Escape si el gesto de CT-02 es válido; si no, solo por el peer), se mide cuántos avisos salen y el estado final. <br>**Si se promueve a prueba** (IR R-I-5): <br>• cada acción programada captura su excepción en una variable local, y la prueba la relanza cuando `ShowDialog` vuelve; <br>• el respaldo, encolado a `ApplicationIdle`, es `try { bandera = true; cancelar el módulo } catch (Exception ex) { capturada ??= ex; } finally { try { si sigue abierta, Close(); } catch (Exception ex) { capturada ??= ex; } }`. Así **no lanza nunca** (V-M-6) y conmuta una **bandera local** que lee la sustitución, sin volver a llamar a `Substitute`; <br>• cuando `ShowDialog` vuelve, se llama a `Abort()` sobre toda `DispatcherOperation` que siga en `Pending`; <br>• las aserciones van después; no hay temporizadores ni aserciones dentro de los delegados | Ejecución | Solo si es estable e independiente de la activación (INV-F4T1B-04). Si no, es evidencia y límite. **Riesgo aceptado y declarado:** si pasa en local y falla solo en la CI del GREEN, es un REWORK |
| CT-05 | `Close()`, que hace las veces de X y Alt+F4 | No mostrada y mostrada (con HWND, `Close()` pasa por `WM_CLOSE`; a medir). Sin trabajo pendiente: no pregunta; los valores iniciales del contrato quedan intactos; `RackName` es lo tecleado y recortado; `RackId` es `null` en un rack nuevo | Ejecución | Sí |
| CT-06 | Cierre al pedir | Insertar, Actualizar (con `LoadExisting`) y cada botón por vista disparan `Closed` sin preguntar. Con un módulo escenificado, se confirma antes y `PendingWork()` queda vacío | Ejecución | Sí |
| CT-07 | Intención por vista | Los cuatro botones: `InsertView`, `InsertSection`, `ViewBox.SelectedIndex` y `Views.Count`. El frontal del lado B con `FrontalSideBox`. GUID con `newIdFactory` | Ejecución | Sí |
| CT-08 | Guardas que no cierran | Los textos de H-05: los literales de 1 a 3 y el diagnóstico del escenario concreto en 4. Las guardas 1 y 2 vuelven sin confirmar el módulo, y la 3 y la 4 lo confirman antes (se ve con `PendingWork()`). Ninguna pide nada ni cierra | Ejecución | Sí |
| CT-09 | Estado inicial de la barra | `IsEnabled` y `ToolTip` exactos de cada botón de acción en {con o sin AutoCAD} × {nuevo o RACKEDITAR}. `InsertBatchButton` está habilitado siempre, también con entradas inválidas. «Cerrar» no tiene tooltip | Ejecución | Sí |
| CT-10 | Foco inicial | La declaración exacta (la `BindingExpression` de `FocusManager.FocusedElement` apunta a `NameBox`); `NameBox` es un `TextBox` y no una acción; su texto en `LoadNew` y en `LoadExisting`. **El foco efectivo no entra en las pruebas:** se mide en local y se registra como evidencia (IR I-12) | Ejecución | Sí, la declaración |
| CT-11 | Tabulación | (1) El orden de las paradas de tabulación **dentro de cada slot**, independiente de la composición. (2) **El orden de las zonas** en la plantilla aplicada: el índice de `PART_ActionBar`, `PART_StatusHost`, la zona lateral y `PART_WorkArea` en el `DockPanel`. (3) Ningún valor **local** (`ReadLocalValue`, o `BaseValueSource.Local`) de `TabIndex`, `IsTabStop` ni `KeyboardNavigation` (IR R-M-7). La parte (2) es un ancla en el mismo método que la (1). **El recorrido efectivo con `MoveFocus`** se mide en local y se registra como evidencia, no como prueba (IR I-9, I-12). OD-9 depende de esa medición | Ejecución | Sí, (1) a (3) |
| CT-12 | Propiedades de la ventana | `Owner` en `null`, `CenterOwner`, `ShowInTaskbar`, `ResizeMode`, `SizeToContent`, `Title` y los tamaños, leídos **antes** de mostrarla | Ejecución | Sí |
| CT-13 | Dueños con instancia | S-3 y S-4 (S-2 va por fuente, véase CT-16): `Owner` es la ventana Push Back y la ubicación es `CenterOwner`, leídos **al entrar** en la costura. S-4 los tiene aunque S-3 esté abierto | Ejecución | Sí |
| CT-14 | `RackViewBatchDialog` | Construido con su presentador, con `RackSystemKind.PushBack` y unas opciones representativas, sin `ShowDialog`: `CenterOwner`, `IsDefault` «Insertar cola» e `IsCancel` «Cancelar». Los argumentos reales de la ventana se fijan en la guarda de CT-23 (IR M-4) | Ejecución | Sí |
| CT-15 | Recursos | La fuente del diccionario fusionado, los valores de los tres tokens, que `Style` es el `EditorShellWindowStyle` resuelto y, con la plantilla aplicada, que el shell no ha activado su respaldo. **Por equivalencia, no por identidad** (§9 I-6) | Ejecución | Sí |
| CT-16 | El configurador | **Por guarda de fuente** (v5, contención del Owner; §9 I-8). <br>• S-2 se crea con `{ Owner = this }` en `ShowHeaderConfigurator` y lleva `CenterOwner` en su XAML. <br>• `ShowHeaderConfigurator` devuelve `window.Configuration ?? copy`, de modo que cerrar el configurador por cualquier camino escenifica su configuración (OD-3). <br>**Por qué no en ejecución:** el configurador real escribe su disposición en `OnClosed`, en la ruta `static readonly` `Environment.GetFolderPath(LocalApplicationData)`, que no se puede redirigir a una ubicación aislada sin cambiar producto. La contención del Owner prohíbe escribir en la configuración real. Por eso **ninguna prueba de T1B instancia `RackFrameConfiguratorWindow`** ni dispara `ConfigureModuleHeader` (D-T1B-07: la ejecución de OD-3 queda como límite declarado) | Fuente | Sí |
| CT-17 | Cancelar Seguridad | No cambia las selecciones ni `RecomputePassesForTest` | Ejecución | Sí |
| CT-18 | Biblioteca sin petición | El camino de guardar no marca inserción y deja `RackId` en `null` (V-M-5), sin llegar a `PromptSaveToLibrary`. Si no se puede observar sin llegar al diálogo, solo queda CT-22 | Ejecución | No |
| CT-19 | Ausencias | En G1, normalizado a LF y sin comentarios: no hay `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` ni `DialogResult`. En `src/` no hay ningún `InsertRequestedRaised +=` | Fuente | Sí |
| CT-20 | Dueños sin instancia | S-5, S-6 y S-7 con `{ Owner = this }`; S-8 con `TryShow(this, …)` y su `Owner = owner`; S-9 con `PromptSaveToLibrary(this, …)` → `ShowDialog(owner)`. Las búsquedas van por las raíces de la clausura. El dueño nulo de S-1 se observa en ejecución en CT-02 y CT-03, así que no lleva guarda (IR M-20) | Fuente | Sí |
| CT-21 | El host del menú | `{ Owner = context.Owner }` + `ShowDialog()` en los dos caminos; `InsertRequested ? InsertionRequest : null` | Fuente | Sí |
| CT-22 | El orden de las escrituras de la ventana | `PromptSaveToLibrary(this` → `EnsureId` → `SetName` → `Save(RackProject.ForPushBack(…).WithSourceMetadataFrom(sourceProject)…)`; el `OnClosed` del configurador → `SaveCurrentLayout` | Fuente | Sí |
| CT-23 | «Insertar varias…» | **En ejecución** (IR I-10), con `new RackPushBackSystemWindow()` **sin mostrar** (así, un escape hacia `TryShow` lanza en `Owner = owner` en lugar de colgar). La variante bloqueada usa un bloqueo **independiente** de la edición de módulo, como `StageBlockingEdit` (IR R-I-2). Con un módulo escenificado y un campo inválido: al pulsar `InsertBatchButton`, `PendingWork()` queda vacío, no aparece el texto de AutoCAD, `StatusText` muestra el error del campo, y no hay `Closed` ni petición. Lo mismo con la salida bloqueada y su diagnóstico. **La fuente** solo cubre el tramo diálogo → `RequestInsertViews` → `Close()` y los argumentos del lote | Ejecución + fuente | Sí |
| CT-24 | RACKPUSHBACK/RPB | `new RackPushBackSystemWindow(canInsertInAutoCad: true)` sin `Owner =`; `ShowModalWindow(window)`; `window.InsertionRequest is PushBackInsertionRequest request`, `View/Section` (no `Views`); `System.Name =` y `DrawPushBackView` | Fuente | Sí |
| CT-25 | RACKEDITAR | Lo que lee `EditPushBack` (`InsertRequested`, `DesignToInsert`, `SystemToInsert`, `RackId` y `RackName` como respaldo, `UpdateOnly` e `InsertionRequest.Views`), `System.Name =` y el orden de H-21 con `PrepareAll` antes del primer `RedrawInPlace` | Fuente | Sí |
| CT-26 | El menú en el Plugin | `case PushBackInsertionRequest` → `RackNewRackName.Resolve` → `System.Name =` → `RackViewBatchProducts.PushBack` → `RackViewBatchExecution.Run`; `PlacePushBack(…, regen:false)` | Fuente | Sí |
| CT-27 | Las tres fachadas | (1) Las firmas públicas exactas y la existencia de los caller-owned. (2) Los **caminos clásicos** no llaman a los caller-owned de Push Back: `RackPushBackCommands.cs`, `KindHandlers/PushBackKindHandler.cs`, `RackMenuCommands.cs` y `Views/RackViewBatchProducts.cs`. Que no tengan ningún llamador en producción no se congela, porque T4B añadirá el primero (IR I-4) | Fuente | Sí |
| CT-28 | Cierre con ediciones fuera del ámbito (IR I-11) | Con un `NumericField` cambiado por `SetNumber` sin `LostFocus` y con una edición ya aplicada de la matriz: Escape (con el gesto de CT-02, o «Cerrar» por el peer si se aplica el respaldo) y `Close()` cierran sin preguntar ni pedir, y el texto no se compromete (`RecomputePassesForTest` no cambia) | Ejecución | Sí |

**Lo que T1B no repite:** lo que ya está cubierto (H-23), salvo donde lo pide un CT.

### D-T1B-05. Las clases, los filtros y la colección

Solo archivos nuevos. Ninguna prueba ni ayudante existente se toca.

| Archivo nuevo | Proyecto | CT | Colección |
|---|---|---|---|
| `tests/RackCad.UI.Tests/PushBackClassicCharacterizationKeyboardTests.cs` | UI | CT-01, CT-10, CT-11 | `EditorDiscardPromptCollection` (muestra la ventana) |
| `tests/RackCad.UI.Tests/PushBackClassicCharacterizationCloseTests.cs` | UI | CT-02 a CT-06, CT-28 | `EditorDiscardPromptCollection` |
| `tests/RackCad.UI.Tests/PushBackClassicCharacterizationIntentTests.cs` | UI | CT-07 a CT-09, CT-17, CT-18, CT-23 | `EditorDiscardPromptCollection` (CT-17 muestra la ventana: S-3 lleva `Owner = this`, V-M-11) |
| `tests/RackCad.UI.Tests/PushBackClassicCharacterizationOwnerTests.cs` | UI | CT-12 a CT-14 | `EditorDiscardPromptCollection` (CT-13 muestra la ventana) |
| `tests/RackCad.UI.Tests/PushBackClassicCharacterizationResourceTests.cs` | UI | CT-15 | ninguna (la plantilla se aplica sin mostrar) |
| `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs` | UI | CT-16 (v5), CT-19 a CT-22 y la parte de fuente de CT-23 | ninguna |
| `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs` | Core | CT-24 a CT-27 | ninguna |

- **Filtros**, con los espacios de nombres raíz `RackCad.UI.Tests` y `RackCad.Tests`: `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` y `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization`. Hoy seleccionan 0 pruebas.
- **La colección:** con `DisableParallelization`, ninguna otra clase ejecuta trabajo WPF dentro del bucle anidado de un `ShowDialog` o de un bombeo.
- **Los ayudantes:** todo lo nuevo es privado de las clases nuevas.
- **Las guardas de fuente:** normalizan CRLF a LF, porque Core corre en Ubuntu y en local se trabaja con `core.autocrlf=true`.
- **Nombres:** se siguen las convenciones de I-39. Lo contrario a ADR-0029 lleva el comentario «INCUMPLIMIENTO CARACTERIZADO» con su regla. **Las clases nuevas no usan `Skip`.**

### D-T1B-06. Lo que se congela y se comunica al Owner como deuda (sin corregirlo)

| # | Comportamiento | Regla |
|---|---|---|
| OD-1 | `InsertBatchButton` siempre habilitado, e `InsertBatch_Click` no comprueba `canInsertInAutoCad` | D6 |
| OD-2 | «Insertar varias…» confirma el módulo antes de su diálogo, aunque luego se cancele. `RequestDraw` hace lo mismo en las guardas 3 y 4, por decisión de I-40 | D6, el efecto declarado de la acción (IR M-11) |
| OD-3 | Cerrar el configurador por cualquier camino escenifica su configuración | D6 |
| OD-4 | El aviso de descarte no tiene dueño WPF explícito; el dueño efectivo solo se ve en el host (IR M-12) | D9 |
| OD-5 | S-4, S-5 y S-6 tienen como dueño la ventana Push Back mientras S-3 es modal | D9 |
| OD-6 | RACKPUSHBACK con «Insertar varias…» dibujaría solo `Views[0]` (inferencia del código) | Coherencia entre llamadores |
| OD-7 | La doble pregunta en modal: **solo si CT-04 la mide** | D7 |
| OD-8 | Las celdas de la matriz no se alcanzan con el teclado | Observación; D9 no trata la accesibilidad (IR M-13) |
| OD-9 | La barra de acciones va antes que el panel lateral, y el ciclo pasa por «Restaurar»: **solo si se mide** (CT-11) | D9 |
| OD-10 | Guardar en biblioteca crea la carpeta antes del diálogo | — |
| OD-11 | `InsertView`, `InsertSection` y `SourceProjectToInsert` no se leen, e `InsertRequestedRaised` no tiene suscriptores | — |
| OD-12 | `PushBackPendingWorkQuestionNamesWhatIsLost` es tautológica; no se edita, y T1B añade la prueba real | D13 |
| OD-13 | El texto sin comprometer de un campo se pierde en silencio al cerrar con Escape: **solo si CT-28 lo mide** | D7 |

### D-T1B-07. Lo que solo se puede validar en el host

Se declaran como límites y como candidatos a Smoke-2. Lo decide el Owner (§9 I-5):
- el dueño efectivo con `ShowModalWindow` y `CenterOwner` sin dueño WPF;
- WM_CLOSE y Alt+F4 reales;
- el orden Z de los diálogos anidados;
- el foco y la tabulación efectivos;
- la doble pregunta, si CT-04 no se puede medir;
- **la ejecución de OD-3 y de S-2** (v5): el configurador real no se instancia, porque su disposición iría a la configuración real del usuario. Lo cubre la guarda de fuente de CT-16. Ejecutarlo exigiría una costura de producto, que es una decisión aparte;
- **el enrutado de Escape y Enter desde el elemento con foco** (IR R-M-8). `ProcessKey` caracteriza la ruta de la tecla de acceso; por ejemplo, un `CatalogCombo` abierto consume Escape antes que esa ruta. Va también en `KnownLimitations`.

## 5. Trazabilidad

| Punto de INV-19 | CT |
|---|---|
| Enter | CT-01, CT-14 |
| Escape | CT-02, CT-04, CT-14, CT-28 |
| Foco inicial | CT-10 |
| Tabulación | CT-11, CT-19 |
| Caminos de cierre | CT-02 a CT-06, CT-16, CT-23, CT-28 |
| `Owner`/`CenterOwner` | CT-12, CT-13, CT-14, CT-16, CT-20, CT-21, CT-24 |
| Recursos | CT-15 |
| Intención | CT-06 a CT-09, CT-18, CT-21, CT-23 a CT-26 |
| Escrituras del puerto (D-T1B-01) | CT-05, CT-17, CT-22, CT-24 a CT-28 |

## 6. Contrato propuesto de F4-T1B (a fijar en la emisión)

**Identidad:**
- `TaskId` `F4-T1B-PILOT-CHARACTERIZATION`;
- I-64, F4;
- `attempts` 1 de 3;
- `CorrectionsAuthorized` como en F4.

**Celdas:** las de F4-T4A2.
- El Controller `openai/process/gpt-6-luna`/high, en la planificación y en la verificación.
- El Worker `anthropic/subagent/claude-sonnet-5-5`/high.
- **nc4 en la primera delegación** y nc1..nc3 (IR M-9).

**`AllowedWriteScope`** (IR B-2):
- los **siete** archivos nuevos del D-T1B-05;
- **los 14 archivos exactos de `MutantScope`**, marcados en INV-F4T1B-02 como «solo mutante temporal, neto cero».

**`ForbiddenWriteScope`:**
- por **raíces de `src/`, excluidos los 14 archivos de `MutantScope`**, generadas por el script del contrato como en F4-T4A2;
- `.github/` y `docs/`;
- `tests/RackCad.Tests/Fixtures/`;
- los `.csproj`;
- los ayudantes y bases existentes, por nombre: `StaTestRunner.cs`, `EditorWindowTestSupport.cs`, `EditorDiscardPromptCollection.cs`, `EditorClosePolicyTests.cs`, `RichEditorCharacterizationTests.cs`, `RichEditorCloseContractTests.cs`, `RichEditorContractTests.cs`, `PushBackShellAdoptionTests.cs`, `PushBackPluginSourceGuardTests.cs`, `PushBackRoundTripSourceGuardTests.cs` y `CallerOwnedFacadeGuardTests.cs`.

**`RequiredTests`:**

| Proyecto | Filtro | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| Core | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 4 (CT-24 a CT-27) | true |
| UI | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 22 (CT-01 a CT-23 y CT-28, menos CT-04 y CT-18) | true |

**Invariantes:**
- **INV-F4T1B-01. Sin cambio de producto** (anclado a `ChainBaseSha`, IR R-I-7).
  - `git diff ChainBaseSha..CurrentSha -- src` está vacío.
  - `git diff --name-only ChainBaseSha..CurrentSha` es exactamente la lista de los siete archivos de prueba.
  - `git diff --name-only BaseSha..CurrentSha` ⊆ las 21 rutas de `AllowedWriteScope`.
  - Este control atrapa también un cambio de producto que quedara escondido en un archivo de `MutantScope`. En la primera delegación, `BaseSha` = `ChainBaseSha`.
- **INV-F4T1B-02. El RED por mutantes revertidos (D-T1B-03).**
  - Los mutantes van solo en `MutantScope`, con la clase permitida en cada archivo y las reglas (a) a (c).
  - **Criterio mecánico**, sobre lo que el TRX contiene y con todos los mutantes aplicados a la vez (IR R-I-1). En los TRX del RED de cada filtro de T1B, contando por resultado (método o fila):
    - todos los resultados con `outcome = Failed`;
    - un `<Message>` **sin** el prefijo de tipo que el runner pone a cualquier excepción ajena a `Xunit.Sdk`. Se reconoce como nombre de tipo cualificado seguido de « : », con la expresión ``^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : ``, para no confundirlo con un mensaje de usuario que contenga « : » (V-M-7);
    - la primera línea de `<StackTrace>` en un archivo bajo `tests/`, medido respecto a la raíz del checkout y con cualquier separador (en la CI, una ruta absoluta del runner que contiene la carpeta `tests`);
    - ninguno `NotExecuted` ni `Skipped`;
    - `Failed == Selected ≥ MinSelected`, contando los resultados cuyo `FullyQualifiedName` coincide con el filtro. En la corrida local filtrada, además, `Selected == Total`. En el TRX completo de la CI filtrado por nombre, `Total` es el de toda la suite (V-M-7).
  - **Los delegados:** la regla «ningún `Assert` dentro de un delegado» vale también para los delegados que encola la propia prueba.
  - Los mensajes de usuario en `Assert.True`, `Assert.False` y `Assert.Fail` se admiten (convención de I-39).
  - El GREEN es el inverso exacto: no toca `tests/` y deja `src/` igual que `ChainBaseSha`.
  - **La tabla de mutantes** va en la entrega: mutante → archivo:línea → clase → resultados de T1B que lo detectan, medidos con los filtros de T1B.
    - Las pruebas existentes rotas se dan como **unión**, con todos los mutantes aplicados, y su atribución por análisis declarado.
    - La tabla declara también las aserciones ancla sin mutante propio.
  - **Para el Controller:**
    - las escrituras en `src/` del commit RED, dentro de `MutantScope` y revertidas por el GREEN, **no son una escritura fuera de alcance**;
    - las filas de la tabla prevista del D-T1B-03 son conformes a (a) y (b) por construcción, y (c) se juzga sobre la CI del RED. La tabla es exhaustiva: un mutante que no instancia ninguna fila es F4T1B-C04.
- **INV-F4T1B-03. Se fija lo que hay** (D13).
  - Ninguna prueba existente se edita, se reactiva ni se borra.
  - Las clases nuevas no usan `Skip`.
  - Lo contrario a ADR-0029 se comenta como «INCUMPLIMIENTO CARACTERIZADO», y la deuda del D-T1B-06 no se corrige.
- **INV-F4T1B-04. Medido, no inferido.**
  - Toda inferencia WPF de H-08, H-10, H-12 y H-18 se fija solo si se mide.
  - Un gesto de teclado vale solo con su control positivo.
  - «Estable» quiere decir tres corridas locales consecutivas **en verde**.
  - **El foco y la tabulación efectivos nunca entran en las pruebas.** Se miden en local y se registran como evidencia, alineado con CT-10 y CT-11 (V-M-11).
  - **CT-04** entra solo si es estable y no depende de que la ventana esté activa. Si no, se registra como evidencia y como límite en `KnownLimitations`, sin STOP.
- **INV-F4T1B-05. El aviso.**
  - Las clases que lo sustituyen van en `EditorDiscardPromptCollection`.
  - Nunca se muestra un `MessageBox` real.
  - El aviso de producción no se restaura.
- **INV-F4T1B-06. Ninguna escritura nueva** en el perfil ni en disco.
  - Ninguna prueba ejecuta `PromptSaveToLibrary`, exporta un BOM ni llama a `RackProjectStore.Save`.
  - **Sin excepciones** (v5, contención del Owner): ninguna prueba instancia `RackFrameConfiguratorWindow` ni dispara `ConfigureModuleHeader`, `Bom_Click`, `SaveLibrary_Click` con entradas válidas ni `InsertBatch_Click` con entradas válidas.
  - **La prueba de la limpieza:** antes y después de la corrida local del filtro de UI se registra el SHA-256 y la fecha de modificación de `%LOCALAPPDATA%\RackCad\RackFrameConfigurator.layout` y de la carpeta de biblioteca de `%APPDATA%\RackCad`. Que no cambien es la evidencia de que ningún estado persistente escapa de las pruebas de T1B. Se registra sin rutas locales en la entrega.
  - Ningún modal queda vivo tras una prueba: el respaldo de CT-04 y `Abort()` lo garantizan.
- **INV-F4T1B-07. Las ventanas.**
  - Se muestran fuera de pantalla, con `ShowActivated = false` salvo donde se mide el foco.
  - `Owner`, `WindowStartupLocation` y `ShowInTaskbar` se leen antes de cualquier reescritura.
  - No hay esperas fijas.
  - Todo `ShowDialog` lleva el respaldo de CT-04.
  - **Con la ventana mostrada no se invoca ningún gesto que abra un modal real sin costura:** el BOM, «Insertar varias…» con entradas válidas, «Guardar en biblioteca», el configurador sin presentador ni Seguridad sin sus costuras.
  - **Las corridas locales que muestran ventanas** se hacen sin uso interactivo del equipo, porque mostrar ventanas roba el foco y una tecla del usuario podría alterar el resultado. Se declara en la entrega (IR M-17, R-M-3).
- **INV-F4T1B-08. La ejecución va antes que la fuente.**
  - **Cuándo cabe una guarda de fuente:** solo para el Plugin, los diálogos sin costura de instancia, los hosts, las ausencias y el tramo de CT-23 sin costura.
  - **Cómo se escribe:**
    - normaliza CRLF a LF y quita los comentarios;
    - busca por las raíces de la clausura, sin depender de la composición de archivos;
    - lleva una constante que la marca como «guarda de texto».
  - **En F5:** se sustituye siguiendo el patrón de I-39 (§8).
- **INV-F4T1B-09. Textos exactos**, copiados del producto.
- **INV-F4T1B-10. Sin infraestructura nueva.** Sin costuras, sin cambios en `InternalsVisibleTo`, sin paquetes (ADR-0012) y sin P/Invoke.
- **INV-F4T1B-11. Determinismo.** El GUID sale de `newIdFactory`, y la cultura se fija y se restaura cuando un texto depende de ella.
- **INV-F4T1B-12. La CI del GREEN.**
  - Los cuatro trabajos en `success`, con las suites completas, y las pruebas existentes que rompió el RED de vuelta en verde.
  - **Sin avisos nuevos de las clases que mide la compuerta** (CS, MSB, NU y NETSDK; `eng/ci/verify-autocad-references.ps1:110-113`), porque `build-plugin` compila la solución en Release con `-NoWarnings` y la solución incluye los proyectos de prueba.
  - Vale también para los mutantes: sin CS0162, CS0168, CS0169, CS0219, CS0414 ni CS8321 (IR R-M-2, V-M-3).
  - Los avisos de los analizadores (`xUnit*`, `CA*`) no son criterio de la compuerta, pero tampoco se introducen a sabiendas.
- **INV-F4T1B-13. Correcciones:** según el D-T1B-03 (IR I-3).

**`ExpectedEvidence`:**
- **Antes del Worker:** el contrato y la delegación son válidos, con la comparación byte a byte, A1-A8 y nc4 (primera delegación).
- **El commit RED y su push**, antes del GREEN, con el mensaje del D-T1B-03.
  - CI del RED: `tests` y `ui-tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido, sin `blame-hang`.
  - Los TRX de los filtros cumplen el criterio mecánico de INV-F4T1B-02.
  - Las pruebas existentes que fallan están todas en la tabla de mutantes.
- **El commit GREEN, inverso exacto de los mutantes**, y su push:
  - `git diff ChainBaseSha..CurrentSha -- src` vacío y `--name-only` igual a los siete archivos;
  - CI del GREEN 4/4 en `success`, con los filtros en verde.
- **La igualdad de bytes del producto** (orden del Owner: «Base product bytes = final GREEN product bytes for every intentionally mutated production file»). Una tabla, por cada archivo de `MutantScope` que el RED tocó, con el SHA-256 del blob en `ChainBaseSha`, en `RedSha` (distinto) y en `CurrentSha`, que tiene que ser **igual** al de `ChainBaseSha`. Cualquier delta residual de producto es FAIL.
- **La prueba de la limpieza** de INV-F4T1B-06: los SHA-256 y las fechas, antes y después.
- **En local:**
  - el build Release de la solución, con el Plugin incluido y **sin avisos nuevos**, sobre el RED (con los mutantes) y sobre el GREEN;
  - los dos filtros sobre el RED y el GREEN;
  - tres corridas consecutivas en verde del filtro de UI.
- **`worker-handoff.json`** según su esquema, con:
  - el RED, el GREEN, el alcance real, el trailer y los conteos;
  - la tabla de mutantes;
  - el mapa CT → método;
  - las mediciones (los gestos y su control positivo, el foco y la tabulación efectivos, CT-04 y CT-16/R-8);
  - `KnownLimitations`.
- **Después:** la verificación del Controller válida y nc1..nc3 según el README §10.
- **Sin AutoCAD:** el Plugin solo se cubre con guardas de fuente.

**Paradas:**
- **F4T1B-C01:** un CT obligatorio exige cambiar el producto o una costura nueva, más allá de los mutantes del RED.
- **F4T1B-C02:** haría falta editar, reactivar o borrar una prueba o un ayudante existente, o tocar un `.csproj`, `InternalsVisibleTo` o los paquetes.
- **F4T1B-C03:** un CT obligatorio necesitaría AutoCAD, un `MessageBox` real o una escritura de un tipo nuevo en el perfil o en disco (INV-F4T1B-06).
- **F4T1B-C04** (V-M-8). Cualquiera de estos casos:
  - un mutante que no instancia ninguna fila de la tabla prevista (v5: la tabla es exhaustiva), que cae fuera de `MutantScope`, que es de una clase que su archivo no permite o que viola el texto de (a) o de (b);
  - un resultado obligatorio del TRX (método o fila) que ningún mutante válido detecta **en la conjunción**;
  - una aserción sin mutante propio que no está declarada como ancla en un método que sí se detecta.
- **F4T1B-C05** (IR I-13): la CI completa del GREEN falla fuera de las pruebas nuevas, **se reproduce** en una segunda corrida y **no es atribuible** a las pruebas nuevas.
  - Si es atribuible, es REWORK por Ci o por Tests.
  - Si no se reproduce, es una reejecución por BLOCKED dentro de los topes de §16.8.
- **F4T1B-C06:** una ruta de la clausura o de `MutantScope` no existe o cambió de nombre.
- **F4T1B-C07:** los hechos del §2 no se sostienen sobre la base de un modo que cambie un CT obligatorio.
- **F4T1B-C08:** una salida de planificación rechazada solo por la comparación literal byte a byte de las colecciones, antes del Worker, permite **una** replanificación con prompt reforzado sin consumir intento de producto. Una segunda falla de fidelidad de la planificación es STOP (F4-E31, F4-E43).

**Comunes, sin parada propia:** la regla de `config.toml` y del binario acreditado (P-01).

## 7. Smoke-2

T1B no añade escenarios. Los candidatos del D-T1B-07 los decide el Owner.

## 8. Lo que queda para F4-T4B y F5 (IR I-5, M-16)

- **Antes de T4B** (orden del Owner: «If T1B reveals a behavior difference: STOP and reconcile before T4B»):
  - la sesión contrasta lo que T1B mide con los supuestos del paquete de T4B (el barrido que se mueve, los caminos clásicos, los dueños y las escrituras);
  - una diferencia de comportamiento respecto a lo que el paquete de T4B da por hecho es un STOP de T4B y se reconcilia con el Owner antes de emitir.

- **F4-T4B:** T1B fija la clausura (§3) y sus pruebas, y **el paquete de T4B decide su alcance**.
  - Si T4B toca un archivo de la clausura (por ejemplo, las fachadas, de las que es dueña), las pruebas de T1B siguen en verde (INV-19).
  - Si una tuviera que cambiar, es un cambio funcional y se declara al Owner (D13).
  - CT-27 ya no impide el primer llamador de los caller-owned (IR I-4).
- **F5:** antes de extraer la superficie, las guardas de fuente de T1B que dependen de la composición se sustituyen siguiendo el patrón de I-39:
  - la guarda vieja queda con `Skip = BaseEvidence` y su evidencia versionada;
  - la prueba de ejecución equivalente va en una clase de contrato;
  - no se borra nada.

  Se arrastra como obligación de F5.
- **La deuda del D-T1B-06:** a decisión del Owner.

## 9. Interpretaciones declaradas que se comunican al Owner (decisiones para la emisión)

| # | Interpretación | Alternativa |
|---|---|---|
| I-1 | «Escrituras del puerto» = las capas del D-T1B-01 | Un puerto como tipo, que es un cambio de producto fuera de T1B |
| I-2 | **El RED por mutantes revertidos**, con `MutantScope` exacto y las reglas del D-T1B-03 | Dispensar el `RedPart` de Ci y de Tests |
| I-3 | La clausura con G6 medido y los fragmentos compartidos fuera (§3) | Solo lo declarado en F4-T2 |
| I-4 | X y Alt+F4 por `Close()`, con la variante mostrada (CT-05), sin P/Invoke | P/Invoke de WM_CLOSE |
| I-5 | Lo que solo se ve en el host, como candidato a Smoke-2 | Ampliar Smoke-2 ya |
| I-6 | Recursos por equivalencia | Por identidad |
| I-7 | Las guardas de fuente frágiles a la extracción se arrastran a F5 | Prohibirlas |
| I-8 | **Resuelto por la contención del Owner** («AUTHORIZED WITH CONTAINMENT»: ubicación temporal aislada, nunca la configuración real del usuario). Esa escritura no se puede redirigir sin cambiar producto, así que **S-2 y OD-3 van por guarda de fuente** (IR I-6, opción b) y la ejecución queda como límite declarado | — |

## 10. Disposición de las revisiones

### 10.1 La revisión de la v1 (EMITIBLE TRAS CORRECCIONES, 2/13/20)

| Hallazgo | Disposición |
|---|---|
| B-1. CT-26 sin un mutante posible | **Aceptado.** `MutantScope` por archivo exacto, con los tres archivos compartidos del Plugin (solo textuales) y la tabla prevista CT → archivo → mutante (D-T1B-03) |
| B-2. Los campos prohíben las escrituras del RED | **Aceptado.** Los 14 archivos de `MutantScope` en `AllowedWriteScope`; `ForbiddenWriteScope` por raíces sin ellos; INV-01 sobre el diff neto; la frase para el Controller (INV-02) |
| I-1. Pruebas existentes rotas en el RED; «por aserción» | **Aceptado.** Admitidas y enumeradas; el criterio mecánico del TRX; prohibido `Skip`; `ExpectedEvidence` redactada |
| I-2. Mutantes que cuelgan la CI | **Aceptado.** Reglas (a) a (c), clase por archivo y `MutantScope` reducido a 14 archivos |
| I-3. Sin regla RED para las correcciones | **Aceptado.** D-T1B-03 e INV-13 |
| I-4. CT-27 congela lo que T4B cambia | **Aceptado.** CT-27 limitado a las firmas y a los caminos clásicos; «sin llamador» queda como hecho (H-22) |
| I-5. El §8 dictaba el alcance de T4B | **Aceptado.** §8 reescrito; §3 aclara para qué sirve la clausura |
| I-6. Los efectos del configurador | **Aceptado, opción (a)**, con la recomendación de la revisión: CT-16 cierra el configurador; INV-06 con la excepción declarada; §9 I-8 |
| I-7. Los gestos de teclado sin medir | **Aceptado.** `ProcessKey` con `PresentationSource`, el control positivo obligatorio, el peer bombeado y el respaldo sin STOP; el `RaiseEvent(KeyDown)` no cuenta |
| I-8. CT-04 puede colgar la CI | **Aceptado.** El respaldo especificado y los gestos prohibidos con la ventana mostrada (INV-07) |
| I-9. CT-11 caracterizaba el ayudante | **Aceptado.** El orden dentro de cada slot y el de las zonas de la plantilla; el recorrido efectivo, solo como evidencia |
| I-10. CT-23 tenía camino de ejecución | **Aceptado.** CT-23 en ejecución; la fuente solo en el tramo sin costura |
| I-11. Faltaba el cierre con ediciones fuera del ámbito | **Aceptado.** CT-28 nuevo, OD-13 condicional, `MinSelected` UI = 22 |
| I-12. «Estable» solo en local | **Aceptado.** Tres corridas en verde; el foco y la tabulación efectivos fuera de las pruebas; CT-04 solo si es estable e independiente de la activación |
| I-13. C05 demasiado amplia | **Aceptado.** C05 acotada |
| M-1 a M-20 | **Aceptados todos:** <br>• M-1: G3 = 10, G6 medido (17) con su regla y `measure_closure.py`; <br>• M-2: `is`; <br>• M-3: CT-08; <br>• M-4: CT-14; <br>• M-5: CT-05; <br>• M-6: `System.Name` y `PrepareAll`; <br>• M-7: colección por clase; <br>• M-8: espacios de nombres y LF; <br>• M-9: C08 literal y nc4; <br>• M-10: `ExpectedEvidence` con el build Release; <br>• M-11: OD-2 y OD-3 por D6; <br>• M-12: OD-4 recalificado; <br>• M-13: OD-7, OD-8 y OD-9; <br>• M-14: S6 y S7; <br>• M-15: §1; <br>• M-16: §8; <br>• M-17: `ShowActivated`, y las corridas locales fuera del uso del Owner; <br>• M-18: el mensaje del RED y su SHA sin uso (el commit de mutantes aparte, opcional, no se adopta: un solo RED simplifica el relevo); <br>• M-19: «en verde»; <br>• M-20: sin guarda de S-1 |

### 10.2 La comprobación focalizada de la v2 (EMITIBLE TRAS CORRECCIONES, 1/7/11)

**Lo que resolvió la v2:**
- **Resueltos:** B-2, I-4, I-5, I-9, I-10, I-11 e I-13, y casi todos los menores.
- **A medias, que la v3 cierra:** B-1, I-1, I-2, I-3, I-6, I-7 e I-8.

| Hallazgo | Disposición en la v3 |
|---|---|
| R-B-1. Un mutante por CT frente a un criterio por método o fila | **Aceptado.** <br>• Granularidad declarada por resultado del TRX. <br>• Lista de **aserciones ancla** (CT-12 «`Owner` en `null`», CT-11 (2), CT-24 «sin `Owner =`», CT-27 (2) de `PushBackKindHandler.cs`), siempre en el mismo método que un subpunto con mutante. <br>• `Theory` solo con un mutante por fila (CT-07 con rotación). <br>• Mutantes alternativos de cualquier archivo de `MutantScope`. <br>• Regla (a) ampliada con la cualificación redundante y el código muerto. <br>• CT-27 (2) se mata con un miembro muerto en `RackPushBackCommands.cs`. <br>• C04 reescrita |
| R-I-1. El criterio del TRX | **Aceptado.** <br>• Criterio sobre lo que el TRX contiene: sin el prefijo `«<Tipo> : »` y con la primera línea de la traza en `tests/`. <br>• Se admiten los mensajes de usuario. <br>• Todas las precondiciones son aserciones. <br>• Se comprueba con todos los mutantes a la vez |
| R-I-2. Mutantes que desvían un gesto a un modal o rompen la carga | **Aceptado.** <br>• La regla (b) prohíbe desviar gestos, debilitar guardas, actuar en el constructor y retirar recursos. <br>• CT-01 lleva `IsDefault` solo en «Cerrar» o «Restaurar». <br>• CT-23 va sin mostrar la ventana y con un bloqueo independiente |
| R-I-3. Las reglas literales prohibían filas de la tabla | **Aceptado.** <br>• La regla (b) se reformula en términos observables (ningún modal queda abierto en la suite). <br>• La regla (a) se amplía. <br>• Frase para el Controller: la tabla es conforme por construcción, y C04 solo juzga lo que va fuera de la tabla |
| R-I-4. El control positivo de Enter, y las ramas | **Aceptado.** <br>• El control es una medición sobre `BaseSha`, y en la prueba versionada es una aserción incondicional sin ramas. <br>• El control de Enter es la ruta de `ProcessKey` con «\x1B» (y, opcionalmente, una ventana sonda). <br>• Con el respaldo, CT-04 y CT-28 usan el peer |
| R-I-5. Operaciones vivas y excepciones en CT-04 | **Aceptado.** <br>• Primero se mide. <br>• Al promoverlo a prueba: las excepciones se capturan y se relanzan; el respaldo usa `try/finally` con una bandera local; `Abort()` de las operaciones pendientes; ninguna aserción en los delegados. <br>• Mutante de texto. <br>• Riesgo residual aceptado y declarado (R-M-11) |
| R-I-6. Dos caminos que dejan el configurador sin cerrar | **Aceptado.** <br>• Se captura en variables locales. <br>• `try/finally Close()` dentro del presentador. <br>• La sustitución responde «Sí» a la pregunta del configurador. <br>• El configurador nunca se muestra, y es la condición de la excepción de INV-06 |
| R-I-7. El ancla en `BaseSha` | **Aceptado.** <br>• INV-01, el GREEN y la evidencia se anclan a `ChainBaseSha`. <br>• `BaseSha..CurrentSha` ⊆ las 21 rutas. <br>• Regla de correcciones con §16.8: se vuelven a aplicar todos los mutantes de los filtros con `ExpectRed`, y Core pasa a false solo sin los del Plugin |
| R-M-1 a R-M-11 | **Aceptados:** <br>• R-M-1: `(System.Windows.Window)owner`; <br>• R-M-2: sin avisos nuevos, también con los mutantes; <br>• R-M-3: corridas locales sin uso interactivo; <br>• R-M-4: con el respaldo, CT-02 conserva su método; <br>• R-M-5: fila para CT-18; <br>• R-M-6: oráculo del respaldo del shell fijado; <br>• R-M-7: valor local, y el código de medición fuera de los commits; <br>• R-M-8: límite de `ProcessKey` en D-T1B-07; <br>• R-M-9: rotura de las existentes medida como unión; <br>• R-M-10: «en neto»; <br>• R-M-11: riesgo de CT-04 aceptado y declarado |

### 10.3 La comprobación focalizada de la v3 (EMITIBLE TRAS CORRECCIONES, 0/2/11)

**Lo que dejó resuelto la v3:**
- R-I-1, R-I-2, R-I-3, R-I-5, R-I-6 y R-I-7.
- R-B-1 quedó parcial: el bloqueante ya no existe, pero la lista de anclas estaba incompleta.
- R-I-4 quedó parcial.

**Lo que confirmó la comprobación:**
- el criterio del TRX es correcto para xunit.runner.visualstudio 2.8.2, porque `StaTestRunner` relanza conservando la traza;
- el código muerto de CT-27 (2) en `RackPushBackCommands.cs` compila sin avisos;
- `-NoWarnings` solo mide CS, MSB, NU y NETSDK;
- las especificaciones de CT-04 y CT-16 son realizables.

| Hallazgo | Disposición en la v4 |
|---|---|
| V-I-1. La lista de anclas estaba incompleta y se leía como cerrada | **Aceptado.** <br>• Regla general de ancla admisible. <br>• La lista se declara no exhaustiva. <br>• Se añaden el `Owner` de S-2, S-3 y S-4, el dueño nulo del aviso, «`NameBox` es un `TextBox`» y la sonda de Enter, que va en el método de CT-01 |
| V-I-2. CT-01 sin respaldo | **Aceptado.** El respaldo lo decide la misma medición que el de CT-02: CT-01 conserva su método con la declaración, y el gesto de Enter pasa a `KnownLimitations` sin STOP |
| V-M-1. La conjunción | **Aceptado.** <br>• `IsDefault` solo en `RestoreButton`. <br>• Los casos con trabajo pendiente aseveran la cuenta y el texto. <br>• Frase sobre la conjunción. <br>• Paréntesis corregido |
| V-M-2. CT-24 con `this.` | **Aceptado.** Sin `this.`, porque el método es estático |
| V-M-3. Código muerto y avisos | **Aceptado.** <br>• «Método» privado. <br>• Lista completa de avisos CS. <br>• Las clases de aviso que mide la compuerta |
| V-M-4. El mutante de CT-16 | **Aceptado.** `null` si `DialogResult != true` |
| V-M-5. CT-18 | **Aceptado.** También asevera `RackId` nulo |
| V-M-6. El respaldo de CT-04 podía lanzar | **Aceptado.** Un `catch` que captura en los dos tramos |
| V-M-7. Precisiones del TRX | **Aceptado.** <br>• Expresión del prefijo de tipo. <br>• Rutas relativas al checkout. <br>• De qué TRX se cuenta. <br>• Delegados de la prueba |
| V-M-8. La frase para el Controller y C04 | **Aceptado.** <br>• (a) y (b) por construcción, (c) sobre la CI. <br>• C04 en tres casos. <br>• Marca de fila en la tabla de la entrega |
| V-M-9. «No actúa en el constructor» | **Aceptado.** «No edita el cuerpo del constructor ni hace que la construcción lance» |
| V-M-10. Faltaban dos mutantes | **Aceptado.** <br>• El del respaldo de CT-02 (`IsCancel`). <br>• El del presentador (`Owner = (Window)owner`) |
| V-M-11. Coherencia | **Aceptado.** <br>• CT-17 muestra la ventana. <br>• INV-04 alineado: el foco y la tabulación efectivos nunca van como prueba. <br>• Correcciones con Core en false: `build-plugin` corre, y la lectura de §16.9 #9 se cita con F4-E56. <br>• El §3 incluye los fragmentos compartidos |

### 10.4 Las interpretaciones del Owner en la autorización de emisión (v5)

| Interpretación del Owner (literal resumida) | Cómo queda en la v5 |
|---|---|
| T1B es SOLO caracterización: fija el comportamiento actual antes de que T4B y T4C dependan de sus costuras, y no es un rediseño | INV-F4T1B-01 y -03, sin cambios |
| El RED especial (pruebas permanentes y mutaciones temporales enumeradas) y el GREEN que revierte solo esas mutaciones: **AUTORIZADO** | D-T1B-03, con la tabla **exhaustiva**. Un mutante fuera de ella es C04 |
| Los archivos mutados son idénticos byte a byte a la BaseSha en el GREEN, y cualquier delta residual de producto es FAIL | `ExpectedEvidence`: la tabla de SHA-256 por archivo (`ChainBaseSha` = `CurrentSha`) e INV-01 |
| Cada mutación está enumerada, es reversible, se limita a lo caracterizado, no puede abrir UI sin control en la CI y no puede escribir datos del usuario o locales | La tabla exhaustiva y las reglas (a), (b) y (c), con la cláusula nueva de «ninguna escritura de datos del usuario ni locales» |
| La clausura: la medida, con sus dependencias transitivas, y no toda la UI. No se extiende a otros editores por compartir ayudas genéricas: **AUTORIZADO** | §3 sin cambios (G6 medido; `EditorModules.cs` no propaga) |
| El configurador real, **con contención**: ubicación temporal aislada; nunca la configuración real del usuario; limpieza demostrada; ningún modal vivo; ningún estado persistente fuera de la prueba. Si la implementación escribe por fuerza, se redirige | **La escritura no se puede redirigir sin cambiar producto** (`static readonly`, `GetFolderPath(LocalApplicationData)`). Por eso T1B no instancia el configurador real: CT-16 y S-2 van por guarda de fuente (§9 I-8). INV-06 lleva la prueba de limpieza por SHA-256 y fecha, y D-T1B-07 el límite |
| No arreglar mientras se caracteriza (Enter, Escape y cierre, «Insertar varias» siempre habilitado, rarezas de cancelar tras confirmar, configurador que aplica al cerrar…): se registran como comportamiento heredado | INV-F4T1B-03 y D-T1B-06, sin cambios |
| Si T1B revela una diferencia de comportamiento: STOP y reconciliar antes de T4B | §8, nueva |
