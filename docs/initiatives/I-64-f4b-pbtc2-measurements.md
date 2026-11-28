# PB-TC2: mediciones del paquete v1 (qué, cómo y resultado)

**Base medida:** rama `architecture/workspace-persistente-rackcad`, HEAD `5597c05b5b2f4a24d0baa05555334ff2a66e6c9c`, árbol limpio, en solo lectura (`git -C <worktree>` y lectura de archivos).
- No se compiló ni se ejecutó ninguna prueba.
- La única consulta externa fue `gh run view`, de solo lectura, sobre la CI de la propia rama.
- Los scripts de Python son de solo lectura, se escribieron con la herramienta de archivos y se corren con `python -I`. No se custodian; lo que importa de su salida está en esta tabla.

**Convención:**
- `W` = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`;
- `WC` = `….Composite.cs`;
- `WX` = `….xaml`;
- `S0` = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`;
- `SC1` = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs` (el ayudante de PB-TC).

Las líneas son las de HEAD. Para reproducir, se fija `$W=<worktree>` y se corren las órdenes desde un shell POSIX.

## A. Identidad

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| A-1 | HEAD y árbol | `git -C $W rev-parse HEAD`; `git -C $W status --porcelain` | `5597c05b5b2f4a24d0baa05555334ff2a66e6c9c`; 0 líneas |
| A-2 | El producto es el GREEN de PB-TC | `git -C $W diff --stat 4440f67d 5597c05b -- src tests` | Vacío |
| A-3 | Lo que cambió PB-TC respecto a `f9f2afbb` | `git -C $W diff --name-only f9f2afbb 4440f67d -- src tests` | W, WC y WX; los seis archivos `PushBackDeclaredCorrection*.cs`; las cuatro líneas de SUS (WLC, `PushBackDimensionViewsWindowTests.cs`, `…RoundingLossTests.cs` y `…ModuleTests.cs`) |
| A-4 | Tamaños | `wc -l` | W 3690, WC 936, WX 793 |
| A-5 | `main` | `git -C $W rev-parse origin/main` | `812baf879612320653094a8aaf46aca0bac81383` |

## B. Las seis cajas: siembra, lectura y escritura

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| B-1 | Todo lector y escritor de las seis cajas en el producto | `grep -n "GapBox\|ModuleLengthBox\|RackHeaderHeightBox\|DerivedPostReinforcementHeightBox\|DerivedPostHeightBox\|RackSeparatorSpacingBox\|RackSeparatorCountBox\|DerivedPostReinforcedCheck"` sobre W, WC y WX | **Siembra:** `GapBox.SetNumber(composite.Gap)` (WC:746); `ModuleLengthBox.SetNumber(module.Length)` (W:1833), y `SetNumber(0.0)` sin módulo (W:1813); las seis avanzadas, en `LoadAdvancedRackParameters` (W:1606-1611), las numéricas con `SetOptional` = `SetNumber(valor)` (W:1622-1629). **Lectura:** `Gap_Changed` (WC:184-200; `composite.SetGap(GapBox.Value.Value)` en WC:198); `ModuleLength_Changed` (W:1892-1913; `Val(ModuleLengthBox, module.Length)` en W:1905, tolerancia en W:1906, `SetLength` en W:1911); `ReadAdvancedRackParameters` (W:1632-1647; las cuatro anulables en W:1639, :1641, :1642 y :1646). **Sensibilidad:** `UpdateReinforcementHeightSensitivity` (W:1650-1654). No hay más lectores ni escritores |
| B-2 | Manejadores en el XAML | Lectura de WX | `GapBox` → `Gap_Changed` en `LostFocus` (WX:148). Las cinco cajas avanzadas → `AdvancedRackParameter_Changed` en `LostFocus` (WX:492, :499, :510, :524 y :531); la casilla, en `Click` (WX:505-507). `ModuleLengthBox` → `ModuleLength_Changed` en `LostFocus` (WX:554) |
| B-3 | El orden de la siembra, siempre bajo `suppressSync` | `grep -n "suppressSync"` sobre W y WC | `RefreshCompositePanel` (WC:738-…): siembra `GapBox` en WC:746, dentro de su `suppressSync`. `LoadSelectedModule` (W:1803-1863): `suppressSync = true` antes de W:1833. `LoadAdvancedRackParameters` (W:1595-1619): `suppressSync` en W:1602-1603 y restaurado en W:1615. Los tres manejadores salen en su primera línea si `suppressSync` (WC:186, W:1894, W:1658) |
| B-4 | Ninguna de las seis está en la validación global | Lectura de `AllNumericFields` (W:749-755) y de `AllFieldsValid` (W:727-745) | La lista tiene 21 cajas, y ninguna de las seis. Una avanzada vacía no es opcional (no lleva `IsOptional`, WX:492-535), así que da `EmptyRequired` y `HasError`, pero la pasada sigue: su lectura da nulo, es decir, «el cálculo». Es el comportamiento de hoy, y PB-TC2 no lo cambia |
| B-5 | `SetLength` pasa un módulo de calculado a manual | Lectura de `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs:185-203` y `:162-167` | `SetLength` exige longitud > 0, y después asigna `Length`, `IsManualOverride = true`, `IsCalculated = false` y quita la restauración individual. `HasPendingChanges` es cierto mientras lo escenificado difiere de lo confirmado. «Actualizar» confirma lo pendiente antes de la segunda pasada (W:3386-3389) |
| B-6 | El resolver conserva las longitudes guardadas | Lectura de `src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:135-200` | Si el número de módulos coincide con las posiciones, cada módulo se copia con su `Length`, `IsCalculated` e `IsManualOverride` guardados (`:165-172`). Un módulo calculado con 47,99213 llega así a la sesión y a `ModuleLengthBox` |
| B-7 | `Val` | `grep -n "Val(\|cref=\"Val\""` sobre W | Un solo uso: W:1905. Declarado en W:3634. Dos `cref` en comentarios: W:3641 (documentación de `OptionalVal`) y W:3683 (documentación de `Keep`) |
| B-8 | El proyecto no genera documentación XML | `grep -n "GenerateDocumentationFile\|TreatWarningsAsErrors\|NoWarn"` en `src/RackCad.UI/RackCad.UI.csproj`, `Directory.Build.props` y `Directory.Build.targets` | Ninguna coincidencia en el csproj ni en las props. Un `cref` a un miembro retirado no da aviso de compilación, pero se corrige igual (D-PBTC2-05) |
| B-9 | Los comparadores de PB-TC | Lectura de W:3634-3688 | `Val` W:3634; `OptionalVal` W:3665-3668 (documentación desde W:3636); `ShownFormat` W:3671; `SameShown` W:3674-3678; `Keep` W:3685-3686 (documentación W:3680-3684); `IntVal` W:3688. Final de línea LF |

## C. «Ambos lados» y el indicador mixto (OWNER-1)

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| C-1 | Quién pone y quita el indicador | `grep -n "ApplyMixedSideState()\|mixedNow\.\|IsMixed("` sobre W y WC | `mixedNow` solo cambia en `SetMixed` (WC:571-590: `Add` en WC:579, `Remove` en WC:588). `SetMixed` solo se llama desde `ApplyMixedSideState` (WC:445-504), que solo se llama desde `LoadCompositeCellPanel` (WC:602-640, llamada en WC:634), que solo se llama desde `LoadSelectedFront` (W:489) |
| C-2 | Quién llama a `LoadSelectedFront` | `grep -n "LoadSelectedFront()"` y lectura de cada llamador | `LoadFromModel` (W:431), `SelectMatrixCell` (W:1096), `SelectSingleCell` (W:1201), `MutateStructure` (W:1221), `ApplyScope` (W:1351), el desplegable de frente (W:1464), `ApplyFrontData` (W:1515) y `SideSelector_Changed` (WC:173, bajo `suppressSync`). **No lo llama `Input_Changed`** (W:1240, que es `RequestRecompute()`) |
| C-3 | **El indicador sigue en pie cuando corre `ReadCellValues`** | Lectura del camino «escribir + salir del campo»: `LostFocus` → `Input_Changed` (W:1240) → `RequestRecompute()` → `Recompute()` → `AllFieldsValid` → `CommitCurrentCell` (W:758-762) → `ForEachEditedSide` (W:774-788) → `ReadCellValues(lado)` (W:802-831) | Nada de ese camino llama a `SetMixed`, y escribir en la caja no tiene ningún manejador de `TextChanged` en la ventana. Así que, cuando `ReadCellValues` lee una caja que estaba mixta y recibió un valor, `IsMixed(caja)` es **cierto** para los dos lados. Es la condición que usa el diseño de OWNER-1 |
| C-4 | El estado mixto vuelve opcional la caja y la vacía | Lectura de WC:571-590 | `IsOptional = true` y `SetNumber(null)`. La ayuda dice «Escribe uno para aplicarlo a A y a B» (WC:583-584). Al quitarlo, se devuelve el `IsOptional` original |
| C-5 | Qué cajas pueden estar mixtas | Lectura de WC:456-503 | De las que llevan `Keep`: `FirstLevelHeightBox`, `CellPalletFrontBox`, `CellPalletHeightBox`, `CellPalletWeightBox` y `CellClearBox`. Con `OptionalVal`: `CellBeamLengthOverrideBox`. Las enteras (`PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` y `CellFondoOverrideBox`) ya aplican el valor escrito con `IntVal` |
| C-6 | Las lecturas de la celda hoy | Lectura de W:818-822 y W:825 | `Keep(<caja>, <valor del lado>)` en las cinco, y `OptionalVal(CellBeamLengthOverrideBox, cell.BeamLengthOverride)` |
| C-7 | El selector de lado | Lectura de WC:50, WC:147-175 | Los elementos son «Lado A», «Lado B» y «Ambos lados» (índices 0, 1 y 2). Cambiar el índice llama a `LoadSelectedFront` bajo `suppressSync` sin recalcular. Ya lo usan las pruebas con `SelectedIndex` (`PushBackBothModeCellBeamOverrideTests.cs:60-65`) |

## D. Corpus y rutas de los diseños

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| D-1 | La forma de los sobres que usan los CT | `pbtc2_fixture_scan.py tests/RackCad.Tests/Fixtures/I64` | **`pb-20-ab`:** `PushBack.Composite.Gap = 54`; el lado A en `PushBack.Structure.Fronts[*].Levels[*]` y el lado B en `PushBack.SideB.Fronts[*].Levels[*]`, con 2 frentes y 3 niveles; en `Fronts[0].Levels[0]`, `PalletHeight = 60`, `BeamLengthOverride = null` y `PalletFront = 42` en los dos lados. **`pb-31-v2-modulos`:** `Modules[1]` = M2 `Separator` 48, calculado y no manual; M3, manual. **`pb-06-seguridad`:** `ManualHeaderHeightOverride = 150`, `SeparatorCountOverride = 3`, `SeparatorSpacingOverride = 18`, `DerivedPostReinforced = false`. **`pb-07-combinado`:** `DerivedPostHeight = 140`, `DerivedPostReinforced = true`, `DerivedPostReinforcementHeight = null` |
| D-2 | Las rutas de lado B, comprobadas a mano | Una línea de Python de solo lectura con `-c` sobre `pb-20-ab` (la regla pide escribirlo con la herramienta de archivos; se anota como desviación) | `SideB.Fronts[0]` tiene las mismas claves que `Structure.Fronts[0]`, y sus `Levels` tienen 3 elementos |
| D-3 | Los ayudantes que reutilizan los CT | Lectura de S0 (`:41-58`, `:63-77`, `:93-115`, `:122-123`, `:159-162`, `:327-352`), SC1 (`:65-126`) y `tests/RackCad.UI.Tests/EditorWindowTestSupport.cs:89-95` | `Sta`, `Ventana`, `Cerrar`, `Fixture`, `EditInner`, `Persist`, `Salida`, `Field`, `Check`, `Combo`, `Click`, `SetNumberR` y `SelectModule` (S0); `Cargar`, `Num` y `Poner` (SC1, `internal static`); `EditorWindowTestSupport.SetText`. `Num`/`Poner` navegan con puntos e índices bajo `PushBack` |
| D-4 | La costura del aviso de descarte (M-05) | Lectura de `src/RackCad.UI/Shell/EditorClosePolicy.cs:49-94` y W:3457-3459 | `EditorDiscardPrompt.Substitute(Func<string, bool>)` reemplaza el diálogo mientras dura el ámbito devuelto. `PendingWork()` es «pendiente» cuando `ModuleSession.HasPendingChanges`. Las clases que lo usan van en `EditorDiscardPromptCollection` |
| D-5 | La validación del refuerzo | Lectura de `src/RackCad.Application/Systems/PushBack/PushBackAdvancedRackParameters.cs:60-104` | Con refuerzo, su altura no puede superar la altura resuelta del poste. Las demás anulables solo exigen > 0. `ApplyTo` deja el refuerzo en nulo si `DerivedPostReinforced` es falso (`:116-118`) |

## E. Las pruebas existentes que tocan esas cajas (sustituciones)

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| E-1 | Archivos de prueba que nombran cada caja | `git grep -c "\"<caja>\"" -- 'tests/*.cs'` por caja | **`GapBox`:** 12 archivos de UI. **`ModuleLengthBox`:** 7 (1 de Core, que es una guarda de nombres). **Avanzadas:** `PushBackAdvancedRackParametersWindowTests`, `PushBackEditorSideOwnershipTests`, `PushBackHeaderLineWindowTests`, CT-PB0-05, CT-PBTC-14, T-KB y la guarda de módulos de Core |
| E-2 | `GapBox` | `git grep -n -A2 "\"GapBox\""` en UI | Solo hacen tres cosas: salir del campo sin editar, escribir `w.CompositeState.Gap` (que es 0 o un hueco entero del diseño: el valor por defecto es 0, CS:248 y :584) o escribir enteros (54, 14 y 12). Los archivos son `PushBackA1EditorTests.cs:163`, `:172`; `PushBackBothModeCellBeamOverrideTests.cs:81-83`, `:98`; `PushBackClassicCharacterizationIntentTests.cs:95-97`; `PushBackCompositeLifecycleWiringTests.cs:47`, `:55-57`; `PushBackCompositeLoadExistingModulesTests.cs:58-60`, `:114`, `:130`, `:137`, `:153`, `:159`; `PushBackCompositeWindowTests.cs:119-120`, `:233-234`, `:442-444`; `PushBackDormantRestoreTests.cs:54`, `:86-88`; `PushBackDormantTailTransactionTests.cs:44`, `:70-72`; `PushBackEditorSideOwnershipTests.cs:39-41`; `PushBackLibrarySaveGateTests.cs:67-69`; `PushBackOutputGateAfterRecomputeTests.cs:76-78`. Con tres decimales o menos, `Keep` da el mismo número. Ninguna asevera el hueco exacto |
| E-3 | `ModuleLengthBox` | `git grep -n -A3 "\"ModuleLengthBox\""` | Escriben `Length + 8` o `+ 5` (`PushBackModuleEditorWindowTests.cs:115-117`, `:146-148`, `:259-261`, `:289-291`, `:392-394`; `PushBackClassicCharacterizationCloseTests.cs:95-97`; `…IntentTests.cs:71-73`), o `ValueOf(…) + increment` (CT-PB0-03 y CT-PB0-04, `ModuleTests.cs:34-37`). Todas son longitudes que se ven distintas. El resto solo lee `IsEnabled`, la ayuda o el valor (`:94`, `:155`, `:338-341`, `:364-366`). Ningún módulo del corpus tiene más de tres decimales (D-1) |
| E-4 | Las avanzadas | `git grep -n -B1 -A3` sobre las cuatro cajas en UI | `PushBackAdvancedRackParametersWindowTests.cs:98-249` escribe valores con el portador nulo o vacío (`PostHeight + 24`, `PostHeight / 2`, `PostHeight + 12`, 5 y 21,5) y lee los valores tras «Restaurar». `PushBackEditorSideOwnershipTests.cs:370-373` escribe 100000. `PushBackHeaderLineWindowTests.cs:170-172`, `:272-274` escribe 137. CT-PB0-05 y CT-PBTC-14 solo leen textos. Si una caja se ve igual a su portador, el portador es el valor que la caja recibió, así que `KeepOptional` da el mismo número |
| E-5 | «Ambos lados» con cajas mixtas | `git grep -l "PushBackSideSelection.Both\|Ambos\|SideSelectorBox" -- tests/RackCad.UI.Tests` y lectura de `PushBackBothModeCellBeamOverrideTests.cs:100-260` | Siete archivos. Las ediciones en una caja mixta escriben valores enteros que no se ven como ninguno de los dos lados (130 sobre 100/nulo). También vacían una caja no mixta, o salen sin editar. Con valores de tres decimales o menos, que la caja se vea igual a un lado implica que es igual, así que la regla nueva da el mismo resultado |
| E-6 | Las guardas de fuente que miran esas zonas | `git grep -n -F -e '"Val(' -e '"Keep(' -e 'SetGap(GapBox' -e 'ModuleLength_Changed' -e 'Gap_Changed' -e 'AdvancedRackParameter_Changed' -- tests`; `git grep -l "RackPushBackSystemWindow.xaml.cs\|RackPushBackSystemWindow.Composite.cs" -- tests` | Ninguna cita esos textos. Las que nombran W o WC son I-55, la de módulos (Fact1 y Fact3), G-10, el desviador, y CT-PB0-01 y CT-PB0-17, que solo lo hacen en comentarios o literales de líneas. Fact3 (`tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:381-401`) prohíbe `forceRebuild:` y `forceRebuild)` en W. T-SG prohíbe sus nueve tokens en `Systems/PushBack` |
| E-7 | **Sustituciones** | E-1 a E-6 | **Ninguna.** Ninguna prueba existente fija un redondeo de esas seis cajas ni un «Ambos lados» con un valor que se vea igual a un lado. Coincide con el expediente (§2.1 y §2.2) y con ICT2 N-07 |

## F. CI, recuentos y celdas

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| F-1 | La línea base de la CI | `gh run view 44232324035 --json headSha,status,conclusion,event,headBranch,jobs` | `push` sobre `5597c05b…`, rama exacta, `success`. Los cuatro trabajos en `success` |
| F-2 | Los recuentos | `gh run view 44232324035 --log` filtrado por `Passed!` | **Core:** 0 Failed, 19 070 Passed, 0 Skipped, 19 070 Total. **UI:** 0 Failed, 2 397 Passed, 21 Skipped, 2 418 Total |
| F-3 | Recuentos estáticos de las clases de no regresión | `grep -c '\[Fact'`, `'\[Theory'`, `'\[InlineData'`, `'Skip *='` y `MemberData` | `PushBackCompositeWindowTests` 40 Fact + 4 Theory con 12 `InlineData` (sin `MemberData`) = 52 resultados; `PushBackCompositeLifecycleWiringTests` 7; `PushBackCompositeLoadExistingModulesTests` 3; `PushBackBothModeCellBeamOverrideTests` 10; las cinco de PB-TC 3, 2, 2, 2 y 1; `PushBackAdvancedRackParametersWindowTests` 9; `PushBackModuleEditorWindowTests` 13. Ninguna con `Skip`. Cada nombre corresponde a una sola clase en `tests/` |
| F-4 | El filtro de PB-TC choca con las clases nuevas | Lectura de los filtros del contrato de PB-TC | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` también seleccionaría `PushBackDeclaredCorrection2*`, que fallan en el RED. Por eso PB-TC entra en `RequiredTests` de PB-TC2 por clase. `…PushBackDeclaredCorrectionRoundingTests` no es subcadena de `…PushBackDeclaredCorrection2RoundingTests` |
| F-5 | Las celdas | Lectura de `docs/automation/agent-execution/model-catalog.md:14`, `:48-61` y `:94-108`, y de `EligibleCells` del contrato de PB-TC | `claude-sonnet-5-5` y `gpt-6-luna`, verificadas el 2026-09-30. Las dos pasan a `STALE` el 2026-12-29; hoy (2026-11-18) están vigentes. El contrato de PB-TC las copia del de T1B, con `VerifiedOn` 2026-09-30 y 2026-11-11 |
| F-6 | `ForbiddenWriteScope` | `forbidden_pbtc2.py <worktree>`, con el algoritmo de `make_pbtc_contract.py`: un prefijo solo si ninguna ruta permitida queda debajo; existencia en HEAD con `git cat-file -e`; cobertura de todo `src/` no permitido; rutas nuevas inexistentes | `allowed 4 forbidden 129 (src 59 tests 53 (ui files 51) root 17)`. En `src/` entran los 7 hermanos de la ventana **y WX**, que PB-TC2 no toca. `tests/RackCad.UI.Tests/Workspace/` entra como un solo prefijo, porque ninguna ruta permitida queda debajo (24 archivos). Todas las comprobaciones pasan |
| F-7 | Las rutas nuevas no existen | Mismo script | `PushBackDeclaredCorrection2RoundingTests.cs` y `PushBackDeclaredCorrection2MixedBoxTests.cs` no existen en HEAD. Ninguna prueba ni fuente nombra `DeclaredCorrection2` |

## G. Registro y decisiones

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| G-1 | El registro declarado | Lectura de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md` (211 líneas) | Estado EJECUTADA para PB-TC (`:8`). «Pendientes de PB-T2» en `:169-173`. «Pendiente: decisión del Owner antes de PB-T1» (H-31) en `:175-182`. «Limitación conocida declarada al cerrar (OWNER-1, F4B-E08)» en `:209-211` |
| G-2 | El anexo A custodiado | `grep -n "^## Anexo A\|^### A\."` en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbtc/package-v2.md` | El anexo A está en `:1217-1302` (A.1 en `:1221`, A.4 en `:1285`, A.5 en `:1294`) |
| G-3 | La decisión sobre OWNER-1 de F4B-E08 | Lectura de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbtc-mixed-decision.md` | «Registrar y arreglarlo luego (Recomendado)». La propuesta vuelve al Owner antes del contrato de PB-T1, con H-31 |
| G-4 | Las lecciones del cierre de PB-TC | Lectura de `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md` §4 (`:62-100`) | M-02 (sin CT tautológicos), M-03 (una extensión, un CT, con caso negativo), M-04 (rutas relativas comprobadas y `TreeSha` real), M-05 (sin cerrar con cambios de módulos pendientes) |

## H. Lo que no se pudo medir

- **Nada se compiló ni se ejecutó.** Que los cuatro métodos nuevos fallen por aserción en la base y pasen tras la corrección, y las cifras de la CI, son predicciones a partir del código.
- **El efecto en el dibujo y en el BOM** del hueco, la longitud del módulo y las cuatro avanzadas exactas. Por lectura es menor que 0,0005", con el BOM en diezmilésimas; la cadena lo mide en su paso 1 (D-PBTC2-07).
- **Que `BeamLengthOverride = 100,00044` y `= 120` sean válidos en `pb-20-ab`.** `100,00044` lo fue en `pb-01-base` (CT-PB0-19, F11), y 100/120 en el compuesto construido en código de `PushBackBothModeCellBeamOverrideTests`. En `pb-20-ab`, sin medir.
- **Que `DerivedPostReinforcementHeight = 24,00044` sea válido en `pb-07-combinado`.** Solo exige no superar la altura resuelta del poste (D-5), que no medí.
- **Si una pasada válida vuelve a pintar el estado mixto.** El diseño no depende de ello (C-3).
- **Los recuentos de F-3,** que son estáticos.
- **Si el Controller cuenta un resultado `Skipped` como seleccionado.**

## I. Mediciones de la v2 (tras la revisión independiente IRT2-1)

Misma base (`5597c05b`, árbol limpio) y mismas reglas. Todo es `git -C`, `git grep`, `sed` y `grep` de solo lectura. El único Python es el generador, con la orden `--dry --print`, que no escribe nada (I-7). **Ninguna línea `python -c` en la v2.**

| Id | Qué | Cómo | Resultado |
|---|---|---|---|
| I-1 | La condición con la que se pinta el estado mixto (I-01) | Lectura de WC:445-503 y WC:602-640 | `var mixed = composite.ActiveSelection == PushBackSideSelection.Both && composite.SideBPresent;` (WC:452). `LoadCompositeCellPanel` **sale al principio sin lado B**: `if (SlotPresentText == null \|\| !composite.SideBPresent) return;` (WC:604-607). Así que sin B, `ApplyMixedSideState` no se ejecuta y el indicador no se limpia |
| I-2 | El camino del indicador obsoleto | Lectura de WC:126-145, WC:147-180, `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs:182-190` y `:294-302`, W:1080-1100 y WX:127-129 | `SideBPresentCheck` lleva `Checked` y `Unchecked` (WX:129), así que asignar `IsChecked` dispara `SideBPresent_Changed`. `SetSideBPresent(false)` pone `ActiveSelection = A`. `SideSelectorBox.SelectedIndex = 0` entra en `SideSelector_Changed` y sale en WC:157-160, porque la selección ya es A. `SelectMatrixCell` hace `CommitCurrentCell()` (W:1094) **antes** de `LoadSelectedFront()` (W:1096) y de `RequestRecompute()` (W:1099). `EditTargets` da solo `Active` sin «Ambos» o sin B (CS:184-187) |
| I-3 | Anclas corregidas (M-01) | `grep -n` de las firmas y `awk` del primer `}` de cierre de cada miembro | `SetMixed` WC:565-590; `ApplyMixedSideState` WC:445-503; `SideSelector_Changed` WC:147-180, con `LoadSelectedFront()` en WC:172; `ApplyTopology_Click` WC:215, con `LoadSelectedFront()` en WC:237; `LoadSelectedModule` W:1803-1866; el refuerzo de `ApplyTo`, `PushBackAdvancedRackParameters.cs:120-122`. **Corrige a D-5** (`:116-118`) y a C-2, que omitía `ApplyTopology_Click` |
| I-4 | Las cuentas de INV-PBTC2-04 en la base | `git grep -nP '\bVal\(' -- W`; `git grep -nF 'cref="Val"' -- W`; `git grep -nP '\bIsMixed\(' -- W WC` | `\bVal\(`: exactamente W:1905 y W:3634; la palabra de `OptionalVal(` e `IntVal(` no coincide. `cref="Val"`: W:3641 y W:3683. `\bIsMixed\(`: W:3668 y su declaración en WC:432. `KeepOptional`, `KeepCell` y `MixedBoth` no aparecen en `src/`. Tras D-PBTC2-05 se esperan 0, 0 y 2 en W, y 6, 6 y 3 para los miembros nuevos |
| I-5 | `NumericField.SetNumber` y el texto | Lectura de `src/RackCad.UI/Controls/NumericField.cs:129-140` | `SetNumber` escribe `Text` con el formato y la cultura actual, y `OnTextChanged` vuelve a validar. No hay reformateo al perder el foco, así que «61.2350» se queda tal cual en la caja |
| I-6 | El generador de PB-TC custodiado | `git show HEAD:docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/make_pbtc_contract.py \| diff - <borrador de PB-TC>` | Solo difiere la línea `WT`, redactada como `'<worktree-de-I-64>'`. `make_pbtc2_contract.py` sigue ese patrón |
| I-7 | El generador de PB-TC2, en seco | `python make_pbtc2_contract.py 2026-11-18T00:00:00Z <40 ceros> <40 ceros> --dry --print`; después, `git -C $W status --porcelain` y `ls $W/artifacts/orchestration/I-64/` | `DRY allowed 4 forbidden 129 (src 59 tests 53 root 17 ) inv 14 stops 31 required 18 evidence 8 authorities 30 bytes 37420`. La lista numerada de 1 a 129 coincide con F-6 (WX en la entrada 54; `PushBackAdvancedRackParametersWindowTests.cs` en la 95; `tests/RackCad.UI.Tests/Workspace/` en la 112). Pasan todos los `assert`: las paradas estándar y las celdas iguales a las de PB-TC, existencia en HEAD, cobertura de `src/`, forma del §6.3, rutas nuevas inexistentes, ASCII, términos de cierre y rutas locales. Después, 0 líneas de estado y ninguna carpeta de PB-TC2 en `artifacts/`. Corrió **a la primera** |

### I.1 Lo que de A a H queda corregido

| Fila | Qué decía | Qué es cierto |
|---|---|---|
| C-1 | `SetMixed` en WC:571-590 | WC:565-590 (I-3) |
| C-2 | Los llamadores de `LoadSelectedFront`, sin `ApplyTopology_Click`, y `SideSelector_Changed` con la llamada en WC:173 | Falta `ApplyTopology_Click` (WC:215, llamada en WC:237), y la llamada del selector está en WC:172 (I-3) |
| C-3 | «El indicador sigue en pie cuando corre `ReadCellValues`» | Cierto **con B presente y «Ambos»**. Sin B, el indicador puede quedar obsoleto (I-1, I-2). El diseño v2 lo exige en `MixedBoth` |
| D-2 | La ruta de lado B, comprobada con una línea `python -c` | El resultado se sostiene. La v2 no repite esa forma |
| D-5 | `ApplyTo`, el refuerzo en `:116-118` | `:120-122` (I-3) |
| H | Sin medir: la validez de 100,00044, 120 y 24,00044 | IRT2-1 lo resuelve por lectura: el largo manual exige > 0 (`DynamicEditorCell.cs:25`), y el refuerzo no supera el poste derivado de `pb-07` (`PushBackAdvancedRackParameters.cs:85-105`). No está ejecutado |
