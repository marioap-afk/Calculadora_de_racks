> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-TC (correcciones declaradas)

**Estado:** v1. Lo prepara la sesión principal el 2026-11-17 para la revisión adversarial independiente. No declara GATE PASS. **No se emite** sin las decisiones del Owner del §8 (F4B-E05, «Siguiente», punto 4: «PB-TC no se emite sin la orden del Owner»).

**Patrón:** el paquete del contrato de PB-T0, `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md` (**[PK0]**), y su contrato emitido, `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json` (**[PK0-C]**).
- PB-TC **no** es una caracterización por mutantes.
- Su RED son pruebas nuevas que fallan por aserción en la base, y su GREEN es la corrección del producto (la cadena ordinaria de F4, como F4-T3B1).

**Lo que desarrolla:**
- D-PB-06 (a) y (b), y D-PB-08, de [OD];
- la fila de PB-TC del §5 de [P] y OB-06.1 a OB-06.6, OB-08.1 a OB-08.4, OB-T1B.1 a OB-T1B.3 y OB-RE.1.

**Base:** rama `architecture/workspace-persistente-rackcad` en `f9f2afbb`, con `main` en `812baf87`.
- `git diff 5ccaa770 f9f2afbb -- src` y `git diff f2eedd9a f9f2afbb -- src` están vacíos: el producto es el de `5ccaa770`.
- `tests/` añade a esa base los 10 archivos de PB-T0.
- Todo el §2 está medido ahí, en solo lectura y sin compilar. El detalle y los comandos están en `pbtc-measurements.md` (se custodia con este paquete).

**Precondiciones de emisión:**
1. **Las decisiones del Owner del §8** (OD-PBTC-1 a OD-PBTC-4), transcritas literales en `F4B-DRAFTS/coordinator/owner-pbtc-decisions.md` y anotadas como F4B-E06 en `f4b-dispositions.md`. Si OD-PBTC-4 mete H-PB-28 en PB-TC, este paquete pasa a una v2 (§8, OD-PBTC-4, opción A) antes de emitir.
2. **La revisión independiente** de este paquete, cerrada con 0 bloqueantes y 0 importantes sin resolver.
3. **Custodia en el commit de emisión,** que es la `AuthorityRevision`:
   - este paquete, en `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md`, con `pbtc-measurements.md`;
   - el registro declarado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, creado por la sesión con las filas S-01 a S-03 en estado DECLARADA (§5.4);
   - el contrato y su script generador, en `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/`. El script sigue el patrón de `make_pbt0_contract.py` y comprueba que ninguna ruta permitida queda bajo un prefijo prohibido y que toda ruta prohibida existe.
4. `git diff f9f2afbb <BaseSha> -- src tests` vacío. Si no lo está, se vuelve a medir el §2 (PBTC-C07).
5. Las celdas del §6.8 no están `STALE`; el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | D-PB-06: «ACCEPT THE PRODUCT CORRECTIONS, BUT NOT AS AN INCIDENTAL REFACTOR» y sus seis pasos, con la vuelta al Owner si una corrección altera la geometría «materially» (`:130-155`). D-PB-08 (`:62-74`). D-PB-10 (`:233-236`). T1B (`:237-243`). RACKEDITAR (`:244-248`) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | OB-06.1 a OB-06.6 (`:369-398`); OB-08.1 a OB-08.4 (`:436-440`); la fila de PB-TC y «Por qué PB-TC va antes de PB-T1» (`:502`, `:509-513`); OB-T1B.1 a OB-T1B.3 (`:700-710`); OB-RE.1 (`:722`); R-PB-07 (`:734`); §9 (`:740-750`); §10 (`:766`) |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | La replanificación única por la comparación literal (pregunta 2, su respuesta y la descripción de la opción). Los sustitutos de nc1 y nc2 solo con la firma EXACTA de PD-F4-04 y PD-F4-01 (pregunta 1, ídem) |
| `F4B-DRAFTS/f4b-dispositions.md` | F4B-E01 a F4B-E05. En F4B-E05, «Siguiente»: la vuelta al Owner antes de emitir PB-TC |
| El cierre de PB-T0 (`F4B-PBT0-CHARACTERIZATION/task-close-package.md`, §5, `:79-107`) | Los arrastres a PB-TC (M-05) y la vuelta al Owner. La lección: «el prompt del Worker no puede dar latitudes que el contrato no tiene» (`:107`) |
| La entrega del Worker de PB-T0 (`…/1/R20261117T185616Z-3c1b/worker-handoff.json`, Evidence 0, 24, 31 y 32) | El mapa CT → método, M10, T-3 y T-4 |
| La revisión independiente de PB-T0 (`…/1/R20261117T215307Z-3cff/independent-review.md`, §8.3 y §9) | OWNER-2 (H-PB-28) y OWNER-4 (la lectura de T-3) |
| El bloque M (`F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md:29`) | M-7: 0 de 22 sobres con `DimensionStyle` |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`) y su contrato | La caracterización vigente de la ventana clásica, que sigue en verde (OB-T1B.3) |
| ADR-0029 D13 (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md:172-180`) | «la caracterización no se edita para que pase»; un cambio de comportamiento exige decisión del Owner |
| El patrón de sustitución de I-39 (`docs/automation/evidence/I-39B-caracterizacion-base-vs-contrato.md`; `tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs:45-47`) | La prueba vieja queda con `Skip` y un motivo que nombra el registro. El registro anota la base, el contrato, la autoridad, la aserción literal y la nueva |
| `docs/AUTOMATION_PLAN.md` §16.5, §16.8, §16.9 (`:600-646`) y §16.11 | A1-A8; RED de la cadena; las 14 comprobaciones; las paradas |
| Las fuentes de las firmas (`F4-DRAFTS/f4-dispositions.md:520`, `:562`, `:1103-1106`, `:1540`, `:1657-1660`; `F4-DRAFTS/coordinator/owner-f4t3-authorization.md:255-265`) | PD-F4-01 y PD-F4-04, transcritas en PBTC-C11 |
| `docs/automation/agent-execution/model-catalog.md` (`:14`, `:48-61`, `:94-108`) | Celdas vigentes |

## 2. Hechos medidos en `f9f2afbb`

**Alias:**
- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3630 líneas);
- **WC** = `….Composite.cs` (911);
- **WX** = `….xaml` (792);
- **NF** = `src/RackCad.UI/Controls/NumericField.cs` (213);
- **WLC** = `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`;
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs` (el ayudante de PB-T0).

### 2.1 La pérdida (a): dónde se siembra y dónde se relee

- **H-01. El formato.** `SetNumber(double? value, string format = "0.###")` escribe `Text` con la cultura actual (NF:129-136), y `Value` sale de volver a validar ese texto (NF:138-163).
- **H-02. Las once cajas de F1-F11** (la tabla de M10 de PB-T0, [PK0]:410-421) se siembran con exactamente once llamadas, todas con el formato por defecto:
  - las del rack, en `LoadFromModel`: `DepthBox` (W:386), `ToleranceBox` (W:389), `PostPeralteBox` (W:391), `BeamDepthBox` (W:392) y `AnnotationScaleBox` (W:406);
  - las del frente y la celda, en `LoadSelectedFront`: `FirstLevelHeightBox` (W:454), `CellPalletFrontBox`, `CellPalletHeightBox` y `CellPalletWeightBox` (W:456-458), `CellClearBox` (W:460) y `CellBeamLengthOverrideBox` (W:463).

  Ningún otro código escribe su `Text`. `SetMixed` vacía cinco de ellas con `SetNumber(null)` en el modo «Ambos» (WC:540-565, `:556`).
- **H-03. Dónde se releen.** Solo en dos métodos:
  - `ReadInputs` (W:812-857): `Val(DepthBox, …)` (W:819), `PostPeralte`, `PalletTolerance` y `BeamDepth` (W:837-839) y `AnnotationScale` (W:845);
  - `ReadCellValues` (W:781-810): `FirstLevelHeight` y las cuatro de la celda (W:797-801) y `OptionalVal(CellBeamLengthOverrideBox, …)` (W:804).

  `Val` es `field.Value ?? fallback` (W:3594) y `OptionalVal` es `field.Value ?? (IsMixed(field) ? sideValue : null)` (W:3625-3626).
- **H-04. Quién llama a esas lecturas:**
  - `ReadInputs`, solo `Recompute` (W:646);
  - `ReadCellValues`, `CommitCurrentCell` en cada pasada (W:621, W:737-741), el alcance (W:1306) y los datos del frente (W:1470).

  Las once cajas usan `LostFocus="Input_Changed"` (WX:68, :103, :106, :114, :278, :312, :342, :345, :347, :351 y :354), que es `RequestRecompute()` (W:1207).
- **H-05. Las cajas espejo no se releen.** `FrontBox`, `PalletHeightBox` y `WeightBox` (W:558-564) solo muestran. Frente, alto y peso del rack viajan en `generalPallet` (W:94-101, W:818-820), y por eso G1 a G3 conservan su valor (CT-PB0-20).
- **H-06. Portadores que ya existen en la ventana:**
  - `generalPallet` (W:101);
  - `advanced`, «the SAME transport type the assembler consumes» (W:103-106);
  - `firstLevelDatum` (W:372);
  - el par `loadedDimensionViews` y `dimensionViewsTouched` de I-50, que conserva la política cargada exacta mientras el usuario no toque las casillas (W:1211-1257).
- **H-07. Las cargas pasan todas por `LoadFromModel`** (W:377-427):
  - `LoadNew` (W:262-281);
  - `LoadDesignForNew`, la biblioteca (W:286-299);
  - `LoadExisting`, RACKEDITAR (W:303-316);
  - `Restore_Click`, «Restaurar» (W:3267-3290).
- **H-08. La altura del primer nivel no la reescribe nadie más.** El ajuste al troquel va a `LoadBeamLevels` (`src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:104-114`). El snapshot devuelve `front.FirstLevelHeight` tal cual (`:375-383`), y `BuildFrontDesigns` copia `row.FirstLevelHeight` (`src/RackCad.Application/Systems/Dynamic/DynamicFrontMatrix.cs:449-462`).
- **H-09. RACKEDITAR escribe lo que la ventana entrega:** `window.DesignToInsert` y `window.SystemToInsert` (`src/RackCad.Plugin/RackPushBackCommands.cs:168-190`). «Actualizar» es `RequestDraw(null, -1, updateOnly: true)` (W:3292): segunda pasada (W:3351), `SetName` (W:3365), `RequestUpdate` (W:3369) y `Close()` (W:3378).

### 2.2 La pérdida (b): el estilo de cotas

- **H-10. La carga lo recupera:** `Annotations.DimensionStyle = s.DimensionStyle` (`src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs:209`).
- **H-11. La ventana lo pierde.** `LoadFromModel` lee `options` (W:402) y no guarda el estilo; `ReadInputs` arma `Annotations` sin él (W:840-848). Un rack nuevo nace sin estilo (`PushBackEditorInputs.cs:33`).
- **H-12. Del diseño al dibujo:**
  - `design.DimensionStyle = options.DimensionStyle` (`src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs:121`);
  - `system.DimensionStyle = design.DimensionStyle` (`DynamicRackSystemResolver.cs:246`);
  - `DimensionStyleName = style` en cada cota (`src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs:117`, `:206`, `:285` y `:402`).
- **H-13. Cómo dibuja el Plugin** (`src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`). Push Back dibuja por aquí: `PushBackSystemDrawService.cs:21`, `PushBackFrontalDrawService.cs:20` y `PushBackPlantaDrawService.cs:19`; y `AppendDimension`, desde `:289-291`.
  - **Con un estilo nombrado que existe en el dibujo:** la cota usa ese estilo tal cual, sin sobrescrituras (`:353-356`, `:364`, `:384-390`).
  - **Sin estilo, o con uno que el dibujo no tiene:** el estilo actual del dibujo (`:393`) con sobrescrituras: `Dimscale` 1, `Dimtxt` = alto de texto escalado, `Dimasz` 0,7×, `Dimexe` 0,4×, `Dimexo` 0,4×, `Dimgap` 0,3×, `Dimtad` 1 y `Dimdec` 2 (`:366-374`).
  - **Lo que no depende del estilo:** los puntos, la línea de cota y su desfase, y la capa (`:343-359`).
- **H-14. Sin cotas no hay estilo que aplicar.** `DimensionDetail.None = 0` (`src/RackCad.Domain/Systems/Shared/DimensionDetail.cs:7-10`), y con `None` el detalle efectivo es `None` en todas las vistas (`src/RackCad.Application/Systems/Shared/DimensionViewPolicy.cs:34-41`).

### 2.3 D-PB-08 y las guardas que mira un elemento nuevo

- **H-15. La casilla y su sección:**
  - `SideBPresentCheck` (WX:127-131);
  - `CompositeSection`, colapsada salvo con B declarado (WX:135; WC:65-71);
  - `SlotPresentText`, el texto por celda dentro de la sección (WX:166-167; WC:577-615).
- **H-16. La regla** (`src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`, **CS**):
  - declarar B por primera vez inicializa el lado B entero en blanco (CS:352-386);
  - un lado B dormido vuelve con su presencia (CS:364-374);
  - `IsSlotPresent(B, s)` es `false` con B sin declarar (CS:428-437);
  - son públicos `SideB` y `Of` (CS:164, :269).

  Medido en PB-T0 en la CI de HEAD:
  - en `pb-01-base` (CT-PB0-09, `PushBackFullEditorCharacterizationCompositeTests.cs:163-206`), declarar B da `B=3:000` y `composite=False`; con una ranura de B activa, `True`; con B otra vez en blanco, `False`;
  - en `pb-20-ab` (CT-PB0-07, `:97-127`), declarar B tras retirarlo da `composite=True`, porque B estaba dormido con sus frentes; todas las ranuras de B en blanco dan `composite=False` con la casilla marcada.
- **H-17. Dónde puede vivir el aviso:**
  - `Recompute` cuenta cada pasada efectiva en W:607, después de la salida por `suppressSync` (W:605). Todo gesto que mueve la presencia termina en una pasada: la casilla (WC:101-120), «En blanco» (W:1089-1133 → W:1175-1197) y las cargas (W:426).
  - La línea de estado **no** sirve: `SetFrontActive` escribe su propio estado después de recalcular (W:1125-1132).
- **H-18. Guardas medidas frente a un `TextBlock` con nombre** debajo de la casilla:
  - **T-KB** (`tests/RackCad.UI.Tests/PushBackClassicCharacterizationKeyboardTests.cs`): las paradas solo cuentan `Control` con nombre, `IsTabStop` y `Focusable` (`:166-179`), y `SideStops` es esa lista (`:181-201`). Un `TextBlock` no es `Control`. La comprobación (3) mira solo valores locales de navegación (`:255-277`). **No salta.**
  - **T-SG** (`tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs:165-176`, `:234-248`, `:265-381`): los nueve tokens prohibidos y los literales de dueños, BOM, lote y biblioteca. **No salta** si el código nuevo no contiene ninguno.
  - **I-55** (`tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs:8-15`), **módulos** (`tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:382-455`; prohíbe en WX los nombres del Dinámico), **desviador** (`tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs:312-318`), **foco** (`tests/RackCad.UI.Tests/RichEditorContractTests.cs:36-53`; `RichEditorCharacterizationTests.cs:288-317`) y **`AnnotationScaleBox`** (`tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs:82-99`): ninguna mira esa zona. **No saltan.**
  - **Censo de ventanas** (`tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:86-120`): solo tipos `Window`. **No salta.**
  - **G-10** (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`): una sola `.RequestUpdate(` en W (W:3369). **No salta** sin una nueva.
  - **Las que recorren `src`** (unidades, censo de `Task`): no cubren lo que se añade. **No saltan.**
  - **Las que enumeran textos del panel** (`PushBackAdvancedRackParametersWindowTests.cs:82-84`) usan `Assert.Contains`. **No saltan.**

### 2.4 Las pruebas que fijan hoy la pérdida, y las que siguen válidas

- **H-19. WLC:**
  - la Theory `TheWindowAndThePilotDifferOnlyInTheKnownClasses` (WLC:231-252) recorre 14 filas, con lo esperado en WLC:184-204: pb-06 `DimensionStyle: null -> "STANDARD-ISO"` (`:191`), pb-07 `-> "SIMPLEX"` (`:192`), pb-08 `AnnotationScale: 1.235 -> 1.23456` y `PalletHeight: 61.235 -> 61.23456` (`:193-197`), y las otras 11 vacías;
  - `ClassAIsEvidenced…` (WLC:254-264) solo lee el diccionario estático `Expected`;
  - `…KeepMine…` (WLC:270-313) usa escala 2,25 y tolerancia +2;
  - el corpus (WLC:323-332).
- **H-20. `T19_H1_TheDimensionStyleStillComesOutNull`** (`tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:306-322`): diseño de un sentido con `DimensionStyle = "ESTILO_PB"` (constructor en `:65-92`); asevera `null` tras la carga y tras desmarcar `DimensionsLateralCheck`.
- **H-21. PB-T0 en UI:**
  - **CT-PB0-19** (`PushBackFullEditorCharacterizationRoundingLossTests.cs:129-151`): F1-F11 en `REDONDEA`;
  - **CT-PB0-20** (`:153-173`): G1-G9 en `CONSERVA`;
  - **CT-PB0-01** (`PushBackFullEditorCharacterizationStateTests.cs:115-168`): `lineas-retiradas(a/b)=[]` en `:144`, `:147`, `:161` y `:164`;
  - **CT-PB0-02** (`:195-242`): pb-06, pb-07 y pb-08 con `delta=[]`, filtrado entre dos salidas de la misma ventana (`:227-229`);
  - **CT-PB0-17** (`PushBackFullEditorCharacterizationCatalogTests.cs:88`): `retiradas(a/b)=[]` (`:128-151`);
  - **CT-PB0-05** (`PushBackFullEditorCharacterizationModuleTests.cs:171-208`): solo los seis avanzados;
  - **CT-PB0-13 a CT-PB0-15:** deltas entre dos salidas de la misma ventana (`PushBackFullEditorCharacterizationSubdialogTests.cs:34-35`, `:104-160`).
- **H-22. PB-T0 en Core:** CT-PB0-18 (`tests/RackCad.Tests/PushBackFullEditorCharacterizationDimensionStyleLossTests.cs:100`) y CT-PB0-21 (`tests/RackCad.Tests/PushBackFullEditorCharacterizationRoundingEffectTests.cs:254-301`) usan Application, sin ventana.
- **H-23. El corpus** (`tests/RackCad.Tests/Fixtures/I64`, 17 diseños):
  - solo `pb-08-redondeo` tiene valores con más de tres decimales: `AnnotationScale` y `Fronts[0].Levels[0].PalletHeight`;
  - `pb-08` tiene 3 frentes de 3 niveles, con `Fronts[1].Levels[0].PalletHeight = 60`;
  - solo `pb-06` (`STANDARD-ISO`, `Dimensions = 3`) y `pb-07` (`SIMPLEX`, `Dimensions = 1`) traen estilo;
  - `pb-20-ab`, `pb-21-cola`, `pb-31-v2-modulos` y `pb-01-base` no tienen ni estilo ni valores con más de tres decimales.
- **H-24. La búsqueda en todo `tests/`:**
  - `DimensionStyle` aparece en 66 líneas, y solo `T19_H1` y WLC fijan la pérdida en la ventana. Las demás son del Dinámico o del Selectivo, de Application o del Plugin;
  - de las pistas de redondeo, en UI solo WLC y PB-T0 pasan por la ventana;
  - el corpus I64 solo lo cargan en UI PB-T0, WLC y `WorkspaceDraftPilotCanonicalLineEndingTests`, que mira los finales de línea;
  - T1B no carga el corpus ni usa valores con más de tres decimales.

### 2.5 CI e infraestructura

- **H-25. La CI de HEAD** (run `44188678597`, medida con `gh run view`): `push` sobre `f9f2afbb`, los cuatro trabajos en `success`.
  - **Core:** 19 070 Passed de 19 070.
  - **UI:** 2 404 Passed, 17 Skipped, 2 421 en total.
- **H-26. Un Theory saltado cuenta como un resultado.** En UI hay 17 atributos `Skip` (`BoundedEditorCharacterizationTests` 8, `DialogWindowCharacterizationTests` 3, `RichEditorCharacterizationTests` 4 y `StructuralSectionInspectorWindowTests` 2), uno de ellos un `Theory` de 4 filas (`BoundedEditorCharacterizationTests.cs:161`). La CI da 17 Skipped.
- **H-27. Recuentos estáticos** (por atributo, sin ejecutar):
  - `PushBackDimensionViewsWindowTests`: 9 Fact y 1 Theory de 4, 13 casos;
  - WLC: 3 Fact y 1 Theory de 14, 17 casos;
  - las tres clases de Cantilever en UI: 67 casos;
  - `PushBackBlankFrontAuthorityTests` (Core): 9;
  - T1B: 55 en UI y 10 en Core (cierre de PB-T0, `:22`).
- **H-28. Ayudantes y costuras existentes:**
  - S0 da `Ventana()` (`:63-68`), `Cerrar` (`:71-77`), `Sta` (`:42-58`), `Fixture` (`:93-98`), `EditInner` (`:108-115`), `Persist` (`:122-123`), `Salida` (`:159-162`), `Delta` (`:175-185`), `Field`, `Check`, `Combo` y `Btn` (`:327-330`), `StatusText` (`:331`), `Click` (`:333`) y `SetNumberR` (`:336-341`). Su `Abrir` (`:126-133`) deja viva la ventana si `LoadExisting` lanza (M-01 de PB-T0);
  - la ventana: `Session` (W:157), `LastComputation` y `CurrentInputsAreValid` (W:182-184), `SelectMatrixCell` (W:1047, ya usada en `PushBackCellConfigurationWindowTests.cs:104` y `BlankFrontEditorTests.cs:330`) y `CompositeState` (WC:43);
  - `EditorWindowTestSupport.SetFrontBlank` (`tests/RackCad.UI.Tests/EditorWindowTestSupport.cs:67`);
  - la colección `EditorDiscardPromptCollection` (`tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs:27-30`).

### 2.6 Hallazgos nuevos (continúan la numeración de [P] §2.1)

| Id | Hallazgo | Fuente | Consecuencia |
|---|---|---|---|
| **H-PB-34** | **Cantilever no usa el formato por defecto de `NumericField`.** Sus 33 llamadas a `SetNumber` pasan un formato explícito: `"0"`, o una constante propia `NumberFormat = "0.###"` (`RackCantileverWindow.xaml.cs:37`, `Components/CantileverArmWindow.xaml.cs:26`, `CantileverBraceWindow.xaml.cs:24` y `CantileverColumnBaseWindow.xaml.cs:28`). El valor por defecto solo lo usan las 29 llamadas de Push Back (24 en W y 5 en WC) y `EditorWindowTestSupport.SetNumberAndCommit` (`:106`) | `git grep "SetNumber("` en `src` y lectura | La premisa de OB-06.6 vale a medias: cambiar el valor por defecto no movería Cantilever. Pero sí cambiaría lo que muestran las 29 cajas de Push Back, un cambio visible más allá de la corrección, y el ayudante que usan muchas pruebas de UI. La corrección no toca `NumericField` (D-PBTC-01) |
| **H-PB-35** | **El repositorio ya resolvió esta misma pérdida en Cantilever:** `Keep` devuelve el valor del modelo si la caja muestra lo mismo que él con el formato de la caja (`RackCantileverWindow.xaml.cs:368-384`, usado en `:333`, `:344-345` y `:360-361`). Lo fijan `CantileverEditorWindowTests.cs:214` («A field nobody edited must not change the design») y `:234` (la edición manda) | Lectura | Es el precedente del diseño de (a). PB-TC lo copia en W y no lo comparte, porque es privado de Cantilever |
| **H-PB-36** | **La pérdida (a) no es solo de la carga.** Seleccionar una celda la siembra con «0.###» (W:456-463), y la pasada siguiente la escribe redondeada (W:621, W:740). El alcance y los datos del frente propagan el valor redondeado a otras celdas (W:1306, W:1470). G9 conserva su valor en CT-PB0-20 solo porque la celda (1,0) nunca se selecciona | Lectura de W:1047-1067 y W:431-484 | El diseño cubre la siembra y la relectura, no solo `LoadExisting`. CT-PBTC-03 lo fija |
| **H-PB-37** | **El panel del Workspace ya escribe hoy los valores exactos y el estilo.** WLC mide la ventana frente al piloto: el piloto conserva `STANDARD-ISO`, `SIMPLEX`, `1.23456` y `61.23456` (WLC:191-197), y la escritura del panel redibuja desde el diseño del piloto (H-PB-05, H-PB-30) | WLC; [P]:81, `:120` | Hoy RACKEDITAR y el «Actualizar» del panel ya difieren en (a) y (b). PB-TC alinea RACKEDITAR con lo que el panel ya hace |
| **H-PB-38** | **Los 22 sobres reales medidos** no tienen estilo **y** tienen `Dimensions = 0`, así que no dibujan ninguna cota. Los 22 tienen `AnnotationScale = 1`. Los inventarios no recogen los demás campos de F1-F11 | `block-m/inventories/m7-dimkeys.txt`; resúmenes `M1-pre` y `M7-D2-pre` | (b) no puede cambiar ningún dibujo medido, ni aunque tuviera estilo. R0 y R5 de T-3 (la escala) tampoco. No se sabe si F1-F4 o F6-F11 traen más de tres decimales en dibujos reales |
| **H-PB-39** | **La línea de estado no puede llevar el aviso de D-PB-08:** «En blanco» la reescribe después de recalcular (W:1125-1132). En cambio, un `TextBlock` con nombre debajo de la casilla no hace saltar ninguna guarda medida (H-18) | H-17, H-18 | El aviso es un elemento propio de WX (D-PBTC-03) |
| **H-PB-40** | **La línea base de la CI:** Core 19 070 **ya incluye** las 3 pruebas de Core de PB-T0. El GREEN de PB-T0 ya daba 19 070 de 19 070 con sus 3 nuevos (cierre de PB-T0, `:22`). Además, un Theory saltado cuenta como un resultado (H-26) | H-25, H-26 | La cifra del encargo («19 070 + 3») no es la de la CI. Las cifras previstas del §6.9 se derivan de 19 070 y 2 421 |
| **H-PB-41** | **`ClassAIsEvidenced…` de WLC sigue en verde** tras la corrección: solo asevera sobre el diccionario estático de la Theory (WLC:257-263) | H-19 | No choca con la corrección, así que no se sustituye (OB-T1B.2). El registro lo anota (§5.3) |

## 3. Decisiones del paquete

### D-PBTC-01. La corrección (a): «exacto mientras la caja muestre lo mismo»

**Qué hace:** una caja que el usuario no editó no cambia el diseño. Una caja editada manda. Es la regla de `Keep` de Cantilever (H-PB-35), aplicada a las once cajas de F1-F11 en W, sin tocar `NumericField`.

**Cómo, con su línea:**
1. **Un comparador privado nuevo, en W, junto a `Val` (W:3594):**
   - `ShownFormat` = `"0.###"`, el formato con el que `SetNumber` pinta esas cajas (NF:131);
   - `SameShown(a, b)`: igualdad ordinal de `a` y `b` formateados con `ShownFormat` y la cultura invariante;
   - `Keep(field, current)`: `current` si `field.Value` es nulo o si `SameShown(field.Value, current)`; si no, `field.Value`.
2. **`ReadCellValues` (W:797-801):** `Val(<caja>, front.FirstLevelHeight)` y `Val(<caja>, cell.<campo>)` pasan a `Keep(<caja>, …)`, con el mismo segundo argumento: el valor del lado que se escribe.
3. **`OptionalVal` (W:3625-3626)** se queda con el mismo comportamiento cuando la caja está vacía. Si la caja tiene valor y el del lado también, y los dos se ven iguales, devuelve el del lado.
4. **Las cinco cajas del rack** reciben cada una un portador privado de la ventana, como `generalPallet` y `firstLevelDatum` (H-06):
   - en `LoadFromModel` (W:384-406), cada portador toma el **mismo** valor que hoy recibe su `SetNumber`, y la llamada pinta ese portador;
   - en `ReadInputs` (W:818-845), antes de armar los inputs, cada portador pasa a `Keep(<caja>, <portador>)` y los inputs usan los portadores.
5. **`Val` (W:3594)** se queda sin usos y se retira.

**Lo que no cambia:**
- las 29 llamadas a `SetNumber` de W y WC, ni su formato ni su valor: la caja muestra lo mismo que en la base;
- la validación, el error y el modo «Ambos»;
- `IntVal` (W:3628) y las cajas enteras.

**El resultado, por gesto:**
- **Carga, «Restaurar» y selección de celda:** el valor exacto del modelo llega a la salida.
- **El usuario escribe un número que se ve distinto:** manda su número, y el portador o el estado pasan a ese número.
- **El usuario escribe el mismo número que se ve,** por ejemplo «61.235» sobre 61,23456: se conserva el exacto. Es la regla de Cantilever. La diferencia que no se puede expresar es menor que 0,0005.
- **«Ambos lados»:** con una caja no mixta, cada lado conserva su propio valor exacto si los dos se ven iguales. Hoy los dos reciben el redondeo del lado activo.
- **Alcance y datos del frente** (W:1306, W:1470): aplican el valor exacto de la celda primaria. Hoy aplican su redondeo.

**Por qué queda en la ruta de Push Back (OB-06.6):** todo vive en W. Solo cambia la lectura de las cajas que la propia ventana de Push Back siembra desde su modelo: carga, restauración y selección (H-02, H-07). Ningún archivo compartido ni de otro sistema cambia.

**Por qué Cantilever no cambia:**
1. Ningún archivo de `src/RackCad.UI/Controls/` ni de `src/RackCad.UI/Systems/Cantilever/` entra en el diff (INV-PBTC-01; están en `ForbiddenWriteScope`).
2. Cantilever no depende del formato por defecto (H-PB-34) y tiene su propio `Keep`, que no se toca (H-PB-35).
3. CT-PBTC-09 fija el contrato de `NumericField`, y sus 67 pruebas de UI van en `RequiredTests` (§6.4).

**Alternativas descartadas:**
- **Cambiar el valor por defecto de `NumericField`:** cambia lo que muestran las 29 cajas de Push Back y el ayudante de pruebas (H-PB-34). Además, OB-06.6 lo reserva al Owner.
- **Sembrar con un formato largo** (`"R"` o `"0.##########"`) en W: cambia el texto visible de RACKEDITAR más allá de la corrección.
- **Una sombra por texto con un manejador de `TextChanged`:** es una suscripción nueva, y en las pruebas no distingue al usuario de la siembra. `Keep` ya está probado en el repositorio.

### D-PBTC-02. La corrección (b): el estilo cargado viaja en cada pasada

- **Un portador privado** `loadedDimensionStyle` en W, junto a `loadedDimensionViews` (W:1216):
  - en `LoadFromModel`, tras W:402, toma `options.DimensionStyle`;
  - en `ReadInputs` (W:840-848), `Annotations` lleva `DimensionStyle = loadedDimensionStyle`.
- **Sin control nuevo:** la ventana de Push Back no tiene selector de estilo (MB H-082), y no lo gana.
- **Un rack nuevo** sigue sin estilo (H-11).
- **Por construcción,** el diseño, el sistema y cada cota llevan el estilo cargado (H-12). RACKEDITAR dibuja entonces con el estilo nombrado si el dibujo lo tiene, y sin sobrescrituras (H-13). El efecto, para el Owner, está en el §8, OD-PBTC-2.
- **Rutas:** las cuatro cargas de H-07. Con la biblioteca, «Insertar» y «Guardar en biblioteca» también conservan el estilo y los valores exactos, porque salen de `lastComputation.Design`. Hoy los pierden. Es la misma corrección y se declara.

### D-PBTC-03. D-PB-08: el aviso no bloqueante

- **El elemento:** en WX, inmediatamente después de `SideBPresentCheck` (WX:127-131) y antes de `CompositeSection` (WX:135):

  `<TextBlock x:Name="SideBEmptyNoticeText" Visibility="Collapsed" TextWrapping="Wrap" FontSize="9.5" Margin="0,0,0,6" Foreground="{DynamicResource ShellTextSecondaryBrush}" />`

  Sin `Text` en el XAML. Usa el recurso que ya usa `StatusText` (WX:684) y el tamaño de `SlotPresentText` (WX:166).
- **El texto,** una constante de WC (propuesta, a confirmar por el Owner en OD-PBTC-3):

  > «Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.»

- **La regla, en un método privado nuevo de WC:** visible si `composite.SideBPresent` y ninguna ranura `s` del lado B cumple `composite.IsSlotPresent(PushBackSide.B, s)`; colapsado en cualquier otro caso. Con visible, el texto es la constante; con colapsado, vacío. Es la regla de capacidad sin presencia de H-16, solo leída.
- **Dónde se evalúa:** una llamada en `Recompute`, justo después de W:607. Así se evalúa en toda pasada efectiva, válida o no (H-17).
- **Lo que no hace:**
  - no es un `Control` ni es enfocable;
  - no cambia ningún botón, ni el estado, ni el diseño;
  - no toca la casilla ni `StatusText`.
- **Los dos modos:** es la misma ventana. PB-T2 lo conserva en modo WORKSPACE (OB-08.2), y el registro lo anota como obligación de PB-T2.

### D-PBTC-04. La sustitución declarada (OB-T1B.2, OB-06.3b)

- **Se sustituyen exactamente tres pruebas, S-01 a S-03 (§5.2).** Son las únicas que chocan con la corrección (H-19 a H-24).
- **La única edición** en cada uno de sus tres archivos es el atributo de la prueba: `[Theory]` o `[Fact]` pasa a `[Theory(Skip = "<literal>")]` o `[Fact(Skip = "<literal>")]`, con el literal del §5.2. Nada más cambia.
- **El registro lo crea la sesión** en el commit de emisión (precondición 3), con el contenido del §5.4. El Worker no escribe en `docs/`.
- **Lo que choca y lo que no:** lo que sigue en verde no se toca (§5.3), incluida `ClassAIsEvidenced…` (H-PB-41).

### D-PBTC-05. El RED y el GREEN

- **RED:**
  - los seis archivos nuevos (§4) y los tres atributos `Skip`;
  - ningún archivo de `src/`;
  - los ocho métodos del filtro con `ExpectRed` fallan **por aserción** en la base (§5.1).
- **GREEN:**
  - solo W, WC y WX;
  - ningún archivo de `tests/`.
- **Core no tiene RED,** porque la corrección es solo de UI. Su filtro va con `ExpectRed` false. Precedente: F4-T3B1, cuyo RED con Core en `success` se aceptó con `RedPart = pass`.

### D-PBTC-06. El paso 4 en el Workspace queda para PB-T2 (OB-06.3)

- El piloto no pasa por la ventana (H-PB-30). PB-TC prueba el paso 4 solo en RACKEDITAR, es decir, en la ventana en modo clásico: CT-PBTC-02 y CT-PBTC-05.
- CT-PBTC-06 deja además fijado que la ventana y el piloto escriben lo mismo en los 14 diseños.
- El registro anota el paso 4 en WORKSPACE como **pendiente de PB-T2**.
- PB-TC no añade el modo WORKSPACE.

### D-PBTC-07. H-PB-28 queda fuera

H-PB-28 no entra en PB-TC sin la decisión del Owner (OD-PBTC-4). Si el Worker lo encontrara necesario, es PBTC-C10.

### D-PBTC-08. Las ventanas de las pruebas (M-01 de PB-T0)

- Toda ventana se abre como `var w = Ventana(); try { <carga>; … } finally { Cerrar(w); }`, con la carga **dentro** del `try`.
- `Abrir` y `AbrirFueraDePantalla` de S0 no se usan.
- Ninguna ventana se muestra.

### D-PBTC-09. Sin teclado ni foco real

Las pruebas usan solo mecánicas ya probadas en la CI de HEAD:
- `SetNumberR` de S0, que es `SetNumber(v, "R")` más `LostFocus` elevado con `RaiseEvent` (PB-T0, CT-03 y CT-04);
- el mismo `RaiseEvent` de `LostFocus` tras asignar `Text`;
- `IsChecked` y `SelectedIndex` asignados por código (PB-T0, CT-07 y CT-09);
- `SetFrontBlank`;
- `SelectMatrixCell`;
- `ClickNamed`.

Ninguna depende de `WM_KEYDOWN`, de `TextInput` ni del foco efectivo.

## 4. Las pruebas nuevas (lista cerrada: CT-PBTC-01 a CT-PBTC-09)

### 4.1 Definiciones comunes

- **De S0, solo de uso:** `Ventana`, `Cerrar`, `Sta`, `Fixture`, `EditInner`, `Persist`, `Salida`, `Delta`, `Field`, `Check`, `Combo`, `Btn`, `Click`, `SetNumberR` y `StatusText` (H-28).
- **Del ayudante nuevo `PushBackDeclaredCorrectionSupport`** (`internal static`, sin pruebas):
  - `Cargar(w, t)`: deserializa el sobre `t` (`RackEmbedStore` y `RackProjectStore`) y llama a `w.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)`;
  - `CargarBiblioteca(w, t)`: lo mismo con `w.LoadDesignForNew(project.PushBackDesign, embed.Name, project)`;
  - `Num(json, ruta)`: el número en esa ruta del JSON persistido, escrito con «R» y la cultura invariante, o `<null>`;
  - `PilotoEscribe(t, catalog)`: copia de WLC:84-91, sin referenciarla;
  - `EstiloPb()`: copia de `PushBackDimensionViewsWindowTests.cs:65-92` con `policy = null` y `dimensionStyle = "ESTILO_PB"`;
  - `Aviso(w)`: `Assert.IsType<TextBlock>(w.FindName("SideBEmptyNoticeText"))`.
- **Filas F** (copiadas de CT-PB0-19, `RoundingLossTests.cs:55-65`): sobre `pb-01-base`, el valor exacto es la base más 0,00044.

  | Fila | Ruta del JSON interior | Base |
  |---|---|---|
  | F1 | `Structure.PalletDepth` | 48 |
  | F2 | `Structure.PostPeralte` | 3 |
  | F3 | `Structure.PalletTolerance` | 4 |
  | F4 | `Structure.BeamDepth` | 6 |
  | F5 | `Structure.AnnotationScale` | 1,5 |
  | F6 | `Structure.Fronts[0].FirstLevelHeight` | 4 |
  | F7 | `Structure.Fronts[0].Levels[0].PalletFront` | 42 |
  | F8 | `Structure.Fronts[0].Levels[0].PalletHeight` | 60 |
  | F9 | `Structure.Fronts[0].Levels[0].PalletWeight` | 1000 |
  | F10 | `Structure.Fronts[0].Levels[0].ClearHeight` | 6 |
  | F11 | `Structure.Fronts[0].Levels[0].BeamLengthOverride` | 100 |

- **Comparación:** cada método arma una lista de líneas y termina en **un** `Assert.Equal(<lista esperada>, <lista>)`. Las precondiciones son aserciones. Ninguna aserción va dentro de una costura.
- **Las clases:**
  - las cuatro de Push Back van en `[Collection(EditorDiscardPromptCollection.Name)]`, con el `namespace` `RackCad.UI.Tests`;
  - todas son `public sealed class`, con métodos `[Fact]` y sin `Theory`.
- **Cultura:** la invariante, por `Sta`.
- **Números:** con «R».
- **Avisos:** ninguno nuevo del analizador (por ejemplo, xUnit2013).

### 4.2 Los CT

**CT-PBTC-01. (a) Los once campos conservan su valor exacto tras la carga.** `PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheElevenFieldsKeepTheirExactValueAfterLoading`.
- **Pasos:** por fila F1 a F11, `EditInner(Fixture("pb-01-base"), …)`, `Cargar` y `Salida`.
- **Fija** las once líneas con el formato de CT-PB0-19 y la clase `CONSERVA`. Las dos primeras:
  - `F1 pb-01-base Structure.PalletDepth exacto=48.00044 salida=48.00044 CONSERVA`;
  - `F2 pb-01-base Structure.PostPeralte exacto=3.00044 salida=3.00044 CONSERVA`.

  Las demás siguen igual, con `exacto=` igual al de CT-PB0-19 (`:138-148`) y `salida=` igual a `exacto=`.
- **Sustituye a** CT-PB0-19 (S-03).

**CT-PBTC-02. (a) RACKEDITAR y la biblioteca llevan los valores exactos (paso 4 en RACKEDITAR).** `…RoundingTests.CT_PBTC_02_UpdateInRackeditarAndTheLibraryLoadCarryTheExactValues`.
- **(i) Por fila F1 a F11,** cada una en su ventana: `Cargar`, lectura de `UpdateButton.IsEnabled` (precondición) y `Click(w, "UpdateButton")`.

  La línea es `Fn RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=<exacto>`, con `<exacto>` igual al de CT-PBTC-01, leído de `Persist(w.DesignToInsert)`.
- **(ii) `pb-08-redondeo` tal cual:** lo mismo. La línea es `pb-08-redondeo RACKEDITAR peticion=True updateOnly=True vistas=0 AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456`. `sistema` es `w.SystemToInsert.Structure`.
- **(iii) `pb-08-redondeo` por la biblioteca:** `CargarBiblioteca` y `Salida`, **sin** pulsar nada. La línea es `pb-08-redondeo biblioteca AnnotationScale=1.23456 PalletHeight=61.23456 peticion=False`.

**CT-PBTC-03. (a) La edición manda; una caja sin editar o vuelta a sembrar conserva el exacto.** `…RoundingTests.CT_PBTC_03_AnEditedBoxWinsAndAnUneditedOrReselectedBoxKeepsTheExactValue`.
- **Diseño X-TC-03:** `pb-08-redondeo` con `Structure.Fronts[1].Levels[0].PalletHeight = 60.00044`, por `EditInner`.
- **Notación:** `c00` y `c10` son `Structure.Fronts[0].Levels[0].PalletHeight` y `Structure.Fronts[1].Levels[0].PalletHeight` de `Salida`.
- **Pasos y líneas fijadas:**
  1. carga: `carga AnnotationScale=1.23456 c00=61.23456 c10=60.00044`;
  2. `SetNumberR(w, "AnnotationScaleBox", 2.5)`: `AnnotationScaleBox=2.5 AnnotationScale=2.5 c00=61.23456`;
  3. precondición `Field(w, "CellPalletHeightBox").Text == "61.235"`; se asigna el mismo `Text` y se eleva `LostFocus`: `CellPalletHeightBox mismo texto c00=61.23456`;
  4. `SetNumberR(w, "CellPalletHeightBox", 70.5)`: `CellPalletHeightBox=70.5 c00=70.5`;
  5. `w.SelectMatrixCell(1, 0, false)`: `seleccion(1,0) caja=60 c00=70.5 c10=60.00044`, con `caja` = `Field(w, "CellPalletHeightBox").Text`;
  6. `w.SelectMatrixCell(0, 0, false)`: `seleccion(0,0) caja=70.5 c00=70.5 c10=60.00044 AnnotationScale=2.5`.

**CT-PBTC-04. (b) El estilo cargado sobrevive a las cargas y a los gestos.** `PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures`.
- **Pasos** (el estilo se lee de `w.LastComputation.Design.Structure.DimensionStyle`):
  - `EstiloPb()` con `LoadExisting(…, "GUID-PB-H1", "PB H1")`, tras la carga y después de `Check(w, "DimensionsLateralCheck").IsChecked = false`. Es la construcción y el gesto de `T19_H1`;
  - `pb-06-seguridad` y `pb-07-combinado` con `Cargar`;
  - `pb-07-combinado` con `CargarBiblioteca`;
  - `Ventana()` sin carga (rack nuevo).
- **Fija:**
  - `ESTILO_PB carga=ESTILO_PB`;
  - `ESTILO_PB gesto DimensionsLateralCheck=False estilo=ESTILO_PB`;
  - `pb-06-seguridad carga=STANDARD-ISO`;
  - `pb-07-combinado carga=SIMPLEX`;
  - `pb-07-combinado biblioteca=SIMPLEX`;
  - `nuevo estilo=<null>`.
- **Sustituye a** `T19_H1` (S-02).

**CT-PBTC-05. (b) RACKEDITAR lleva el estilo nombrado a cada cota (paso 4 en RACKEDITAR).** `…DimensionStyleTests.CT_PBTC_05_UpdateInRackeditarCarriesTheNamedStyleToEveryDimension`.
- **Diseños:** `pb-06-seguridad` (`Dimensions = 3`) y `pb-07-combinado` (`Dimensions = 1`), cada uno en su ventana. Se carga con `Cargar` y se pulsa `UpdateButton`.
- **Las cotas:** las instancias `Role == HeaderBlockRole.Dimension` de `w.LastComputation`: `PlantaPlan`, `FrontalEntradaSalida`, `FrontalPosterior`, `LateralPlan` y cada `LateralCortes[*].Plan`, con `Flatten().Instances`.
- **Fija:**
  - `pb-06-seguridad peticion=True updateOnly=True diseno=STANDARD-ISO sistema=STANDARD-ISO cotas>0=True estilos=[STANDARD-ISO]`;
  - `pb-07-combinado peticion=True updateOnly=True diseno=SIMPLEX sistema=SIMPLEX cotas>0=True estilos=[SIMPLEX]`.

  `diseno` es `DesignToInsert.Structure.DimensionStyle`; `sistema` es `SystemToInsert.Structure.DimensionStyle`; `estilos` es el conjunto ordinal de `DimensionStyleName`, con `<null>` para el nulo.

**CT-PBTC-06. La ventana y el piloto escriben lo mismo en los 14 diseños de WLC.** `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns`.
- **Diseños:** los 14 de WLC, por nombre y en su orden: `pb-01-base` a `pb-08-redondeo`, y después `pb-10-sin-datum` a `pb-15-sin-composite`.
- **Pasos:** por diseño, `Cargar`. `admitida` = `w.CurrentInputsAreValid && w.LastComputation.IsValid`. Después, `PilotoEscribe(t, w.Session.Catalog)`, y `Delta(Persist(w.LastComputation.Design), piloto)`.
- **Fija:** 14 líneas `<nombre> admitida=True delta=[]`.
- **Sustituye a** la Theory de WLC (S-01), y cubre sus 14 filas.

**CT-PBTC-07. D-PB-08: el aviso se ve exactamente mientras B está declarado sin frentes activos.** `PushBackDeclaredCorrectionSideBNoticeTests.CT_PBTC_07_TheSideBNoticeShowsExactlyWhileSideBIsDeclaredWithoutAnActiveFront`.
- **`pb-01-base`:** los gestos de CT-PB0-09, copiados (`CompositeTests.cs:171-189`).
- **`pb-20-ab`:** los de CT-PB0-07, copiados (`:103-113`).
- **Por paso:** `aviso=<Visibility>` y `compuesto=<LastComputation.Design.IsComposite>`.
- **Fija:**
  - `pb-01-base carga aviso=Collapsed compuesto=False`;
  - `pb-01-base declarar B aviso=Visible compuesto=False`;
  - `texto=Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.`;
  - `pb-01-base anadir frente aviso=Visible compuesto=False`;
  - `pb-01-base lado B, ranura 1 de B activa aviso=Collapsed compuesto=True`;
  - `pb-01-base ranuras 1 y 2 de A en blanco aviso=Collapsed compuesto=True`;
  - `pb-01-base ranura 1 de B en blanco aviso=Visible compuesto=False`;
  - `pb-01-base retirar B aviso=Collapsed compuesto=False`;
  - `pb-20-ab carga aviso=Collapsed compuesto=True`;
  - `pb-20-ab retirar B aviso=Collapsed compuesto=False`;
  - `pb-20-ab declarar B aviso=Collapsed compuesto=True`;
  - `pb-20-ab todas las ranuras de B en blanco aviso=Visible compuesto=False`.

  Los valores de `compuesto` son los que PB-T0 midió (H-16) y anclan la regla (OB-08.1). La línea `texto=` lleva el literal del §3 byte a byte.

**CT-PBTC-08. D-PB-08: el aviso es solo texto y no bloquea nada.** `…SideBNoticeTests.CT_PBTC_08_TheSideBNoticeIsTextOnlyAndBlocksNoAction`.
- **Diseño:** `pb-01-base` con `SideBPresentCheck` marcada.
- **Fija:**
  - `aviso=Visible control=False enfocable=False actualizar-habilitado=True`, donde `control` es si el aviso es un `Control` y `enfocable` es su `Focusable`;
  - tras `Click(w, "UpdateButton")`: `peticion=True updateOnly=True compuesto=False`, con `compuesto` = `DesignToInsert.IsComposite`.

**CT-PBTC-09. `NumericField` conserva su contrato compartido** (la prueba de que Cantilever y los demás usuarios del control no cambian). `NumericFieldSharedContractTests.CT_PBTC_09_SetNumberKeepsItsDefaultFormatForEveryOtherCaller`. Va fuera del filtro con `ExpectRed`.
- **Pasos:** en el hilo STA, con la cultura invariante, un `NumericField` nuevo.
- **Fija:**
  - `1.23456 por defecto -> 1.235 valor=1.235`;
  - `1.23456 con 0.### -> 1.235 valor=1.235`;
  - `48.00044 por defecto -> 48 valor=48`.
- **Pasa en la base y en el GREEN.**

### 4.3 Trazabilidad

| Obligación | CT |
|---|---|
| OB-06.3, paso 2: un RED por pérdida | (a): 01, 02, 03 y 06. (b): 04, 05 y 06 |
| OB-06.3, paso 4 sobre RACKEDITAR | 02 y 05 |
| OB-06.3, paso 5, y OB-06.3b | 01, 04 y 06 (S-03, S-02 y S-01) |
| OB-06.6, Cantilever sin cambio | 09, más el filtro de Cantilever y INV-PBTC-01 |
| OB-08.1 a OB-08.3 | 07 y 08, más `PushBackBlankFrontAuthorityTests`, CT-PB0-07 y CT-PB0-09 en verde |
| M-05 de PB-T0 | El filtro de PB-T0 en UI en verde (INV-PBTC-08) |

## 5. El RED, las sustituciones y el registro

### 5.1 Por qué cada método nuevo falla en la base, por aserción

| CT | En la base | Primera diferencia |
|---|---|---|
| 01 | Las once filas dan `salida=` igual a R3 del exacto (CT-PB0-19, `:138-148`) | El `Assert.Equal` final, en F1 |
| 02 | `DesignToInsert` sale de la segunda pasada, que relee las cajas redondeadas (H-03, H-09). En (iii), la carga redondea igual (H-07) | El `Assert.Equal` final, en F1 |
| 03 | `carga` da `1.235` y `61.235` (WLC:195-196). El paso 5 da `c10=60` (H-PB-36) | El `Assert.Equal` final, en la línea 1 |
| 04 | El estilo sale `null` tras la carga y tras el gesto (`T19_H1`, H-11) | El `Assert.Equal` final, en la línea 1 |
| 05 | `diseno`, `sistema` y `estilos` dan `<null>` (H-11, H-12) | El `Assert.Equal` final |
| 06 | pb-06, pb-07 y pb-08 dan las líneas de WLC:191-197 | El `Assert.Equal` final, en `pb-06-seguridad` |
| 07 | `FindName("SideBEmptyNoticeText")` es `null` | `Assert.IsType<TextBlock>` en `Aviso(w)` |
| 08 | Igual que 07 | `Assert.IsType<TextBlock>` |

Ninguno lanza antes de su aserción:
- las cargas, los gestos y `UpdateButton` son los que ya pasan en la CI de HEAD (PB-T0, CT-02, CT-07, CT-09 y CT-19; `T19_H1`; WLC);
- `Aviso(w)` es la primera sentencia que toca el aviso.

### 5.2 Las sustituciones (vieja → nueva)

**El literal del `Skip`,** con `<n>` y `<Clase.Metodo>` de la tabla:

`SUSTITUIDA por PB-TC (D-PB-06), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila S-0<n>: la ejecuta <Clase.Metodo>`

| Fila | Prueba vieja (archivo:línea del atributo) | Aserción vieja | Prueba nueva | Aserción nueva | Cobertura comparada |
|---|---|---|---|---|---|
| S-01 | WLC `TheWindowAndThePilotDifferOnlyInTheKnownClasses`, `[Theory]` en WLC:231, 14 filas | 11 filas con delta vacío; pb-06 y pb-07 con la línea de `DimensionStyle` y pb-08 con dos líneas de clase (a) (WLC:184-204); ninguna línea `OUTSIDE` | `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns` | Los 14, admitidos y con delta vacío | **Las mismas 14 filas.** Las 11 sin cambio, igual. Las 3 corregidas, con delta vacío. Es **más fuerte**: no admite ninguna línea, ni de clase (a) ni (b). La rama `WINDOW-NOT-ADMITTED` (WLC:237-241), que ninguna fila ejercía, pasa a la aserción `admitida=True` en las 14 |
| S-02 | `PushBackDimensionViewsWindowTests.T19_H1_TheDimensionStyleStillComesOutNull`, `[Fact]` en `:308` | `Assert.Null` del estilo tras la carga y tras desmarcar `DimensionsLateralCheck` (`:319-320`) | `PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures` | El estilo cargado tras la carga y tras el mismo gesto | **La misma construcción y el mismo gesto,** más pb-06, pb-07, la biblioteca y un rack nuevo |
| S-03 | `PushBackFullEditorCharacterizationRoundingLossTests.CT_PB0_19_TheFieldsThatPassThroughAFormattedControlComeBackRoundedToThreeDecimals`, `[Fact]` en `:129` | F1-F11 en `REDONDEA` (`:135-150`) | `PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheElevenFieldsKeepTheirExactValueAfterLoading` | F1-F11 en `CONSERVA` | **Las mismas once filas, con la misma construcción.** CT-PBTC-02 y CT-PBTC-03 añaden la petición de RACKEDITAR, la biblioteca, la edición y la selección |

**Lo que comprueba el Worker en local, con código temporal que no entra en ningún commit:** sobre el GREEN, cada prueba vieja sin su `Skip` falla por aserción exactamente en lo que cambia:
- S-01: en pb-06, pb-07 y pb-08 (las otras 11 filas pasan);
- S-02: en la línea del estilo;
- S-03: en las once filas.

### 5.3 Lo que sigue válido sin cambio (medido)

| Prueba | Por qué no choca |
|---|---|
| WLC `ClassAIsEvidenced…` (`:254-264`) | Solo asevera sobre el diccionario estático (H-PB-41). Queda en verde y el registro lo anota |
| WLC `…KeepMine…` (`:270-313`) y el corpus (`:323-332`) | Escala 2,25 y tolerancia +2, sin estilo; el corpus no cambia |
| CT-PB0-20, G1-G9 | Esos campos no pasan por cajas que se relean (H-05). Siguen en `CONSERVA` |
| CT-PB0-01 y CT-PB0-17 (**M-05**) | Sus diseños no tienen ni estilo ni valores con más de tres decimales (H-23). Las listas (a)/(b) **siguen vacías** |
| CT-PB0-02, CT-PB0-13 a CT-PB0-15 | Comparan dos salidas de la misma ventana. Las dos llevan ya el estilo y los exactos |
| CT-PB0-05 | Solo fija los seis avanzados, que no son F1-F11 |
| CT-PB0-18 y CT-PB0-21 (Core) | Usan Application sin ventana (H-22). CT-PB0-21 queda como la medida histórica del efecto |
| `PushBackDimensionLegacyCharacterizationTests.cs:249-251` y las demás de `PushBackDimensionViewsWindowTests` | Application, o diseños sin estilo |
| T1B (55 + 10) y las guardas de H-18 | El diseño no toca nada de lo que miran (H-18) |
| `PushBackBlankFrontAuthorityTests` (OB-08.1) | Application. La regla no cambia |

### 5.4 El registro declarado (lo crea la sesión en la emisión)

`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, con el formato de I-39:
- **Cabecera:**
  - autoridad: [OD] D-PB-06 (`:130-155`) y D-PB-08 (`:62-74`), OB-T1B.2;
  - «cambio intencionado del editor clásico, no "no observable change"» ([OD]:149-150, paso 6);
  - estado: DECLARADA. Pasa a EJECUTADA con los SHA del RED y del GREEN al cerrar.
- **S-01, S-02 y S-03:** prueba vieja (clase, método, `archivo:línea` y filas), aserción vieja literal, prueba nueva, aserción nueva, autoridad y por qué es legítimo (§5.2). S-01 anota además las 11 filas que no cambian.
- **Sin sustituir, con su motivo:** WLC `ClassAIsEvidenced…` (H-PB-41).
- **Pendientes de PB-T2:**
  - el paso 4 de D-PB-06 en modo WORKSPACE (OB-06.3);
  - el aviso de D-PB-08 en modo WORKSPACE (OB-08.2).
- **El cambio de comportamiento declarado de RACKEDITAR:**
  - (a) en las once cajas, con sus consecuencias (D-PBTC-01);
  - (b), con su efecto en el dibujo (H-13);
  - el aviso;
  - las rutas de la biblioteca y de «Insertar» (D-PBTC-02).

## 6. Contrato propuesto (a fijar en la emisión)

### 6.1 Identidad

- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; `TaskId` **`F4B-PBTC-DECLARED-CORRECTIONS`**.
- Evidencia en `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/`.
- Attempts 1 de 3; `RoutingEnforcement` `required`; `CorrectionsAuthorized` true.
- **Objective** (ASCII en el JSON): «Corregir en la ventana clasica de Push Back, como cambio declarado de RACKEDITAR, las perdidas (a) redondeo y (b) estilo de cotas de D-PB-06, y anadir el aviso no bloqueante de D-PB-08, segun el paquete del contrato de PB-TC y la lista cerrada CT-PBTC-01..09. El RED son pruebas nuevas que fallan por asercion en la base y la sustitucion declarada S-01..S-03; el GREEN corrige solo la ventana. NumericField, Cantilever, Application y el Plugin no cambian; el paso 4 en el Workspace queda para PB-T2; no se ejecuta AutoCAD.»

### 6.2 `AllowedWriteScope` (12 rutas exactas)

- **Producto, tres:**
  1. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`
  2. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs`
  3. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml`
- **Pruebas nuevas, seis:**
  4. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`
  5. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionRoundingTests.cs`
  6. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionDimensionStyleTests.cs`
  7. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionPilotParityTests.cs`
  8. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSideBNoticeTests.cs`
  9. `tests/RackCad.UI.Tests/NumericFieldSharedContractTests.cs`
- **Sustituciones, tres** (solo la línea del atributo):
  10. `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`
  11. `tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs`
  12. `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationRoundingLossTests.cs`

### 6.3 `ForbiddenWriteScope` (45 entradas, compacto)

Por F4-E21, la lista blanca exacta es la barrera primaria y esta lista es la secundaria. Un prefijo entra solo si ninguna ruta permitida queda debajo, y el script generador lo comprueba. Por eso `tests/RackCad.UI.Tests/Workspace/` **no** es prefijo: WLC está debajo.
- **`src/`, diez:** `src/RackCad.Domain/`, `src/RackCad.Application/`, `src/RackCad.Plugin/`, `src/RackCad.UI/Controls/`, `src/RackCad.UI/Editor/`, `src/RackCad.UI/Shell/`, `src/RackCad.UI/Themes/`, `src/RackCad.UI/Workspace/`, `src/RackCad.UI/Systems/Cantilever/` y `src/RackCad.UI/RackCad.UI.csproj`.
- **`tests/`, dieciocho:**
  - `tests/RackCad.Tests/`, entero; ninguna ruta permitida queda debajo;
  - `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj`, `tests/RackCad.UI.Tests/StaTestRunner.cs`, `tests/RackCad.UI.Tests/EditorWindowTestSupport.cs`, `tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs`, `tests/RackCad.UI.Tests/NumericFieldTests.cs` y `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`;
  - las cinco clases de PB-T0 de UI que no se sustituyen: `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationStateTests.cs`, `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationModuleTests.cs`, `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationCompositeTests.cs`, `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSubdialogTests.cs` y `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationCatalogTests.cs`;
  - las seis de T1B de UI: `tests/RackCad.UI.Tests/PushBackClassicCharacterizationKeyboardTests.cs`, `tests/RackCad.UI.Tests/PushBackClassicCharacterizationCloseTests.cs`, `tests/RackCad.UI.Tests/PushBackClassicCharacterizationIntentTests.cs`, `tests/RackCad.UI.Tests/PushBackClassicCharacterizationOwnerTests.cs`, `tests/RackCad.UI.Tests/PushBackClassicCharacterizationResourceTests.cs` y `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs`.
- **Raíz, diecisiete:** `docs/`, `.github/`, `eng/`, `tools/`, `assets/`, `deploy/`, `RackCad.sln`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`, `NuGet.Config`, `README.md`, `AGENTS.md`, `CLAUDE.md`, `.editorconfig`, `.gitattributes` y `.gitignore`.

Las 45 existen en HEAD (medición I-1).

### 6.4 `RequiredTests`

| Proyecto | Filtro | `MinSelected` | `ExpectRed` | Para qué |
|---|---|---|---|---|
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 8 | true | CT-PBTC-01 a CT-PBTC-08, los ocho fallan en el RED |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldSharedContractTests` | 1 | false | CT-PBTC-09 |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 | false | Cantilever no cambia (OB-06.6); recuento estático (H-27) |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false | T1B en verde (OB-T1B.3) |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 17 | false | PB-T0 en verde: 17 Passed y CT-PB0-19 Skipped (S-03); M-05 |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.WorkspaceDraftPilotWindowLoadCharacterizationTests` | 3 | false | 3 Passed y la Theory, 1 Skipped (S-01) |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDimensionViewsWindowTests` | 12 | false | 12 Passed y `T19_H1` Skipped (S-02) |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 10 | false | T1B en verde |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 | false | CT-PB0-10, CT-PB0-18 y CT-PB0-21 |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackBlankFrontAuthorityTests` | 9 | false | OB-08.1 |

Los `MinSelected` de las filas con `Skip` cuentan solo los que pasan. Así valen tanto si el Controller cuenta como seleccionado un resultado `Skipped` como si no (R-09).

### 6.5 Invariantes

- **INV-PBTC-01. Alcance.**
  - `git diff --name-only BaseSha..CurrentSha`, dentro de las 12 rutas.
  - En `src/`, solo W, WC y WX.
  - `git diff BaseSha..CurrentSha -- src/RackCad.UI/Controls src/RackCad.UI/Systems/Cantilever src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin`, vacío.
- **INV-PBTC-02. RED y GREEN.**
  - **El RED:** solo pruebas, es decir, los seis archivos nuevos y la línea del atributo de S-01 a S-03, sin ningún archivo de `src/`. Se publica antes del GREEN.
  - **El GREEN:** solo W, WC y WX, sin ningún archivo de `tests/`.
  - **Criterio mecánico del RED,** solo sobre el filtro con `ExpectRed` y con los resultados de la CI del RED:
    - todos `Failed`;
    - `<Message>` sin el prefijo de tipo `^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : `;
    - la primera línea de `<StackTrace>`, bajo `tests/`;
    - ninguno `NotExecuted` ni `Skipped`;
    - `Failed == Selected ≥ 8`.
  - **En el RED,** los filtros sin `ExpectRed` pasan, y sus únicos `Skipped` son S-01 a S-03.
- **INV-PBTC-03. La corrección (a)** (D-PBTC-01).
  - **Las once cajas de F1-F11:** el mismo texto que en la base, y las 29 llamadas a `SetNumber` de W y WC sin cambio de formato ni de valor.
  - **Su lectura:** el valor del modelo si la caja muestra lo mismo que ese valor con «0.###» en la cultura invariante; si no, el valor de la caja. El modelo es el estado del lado para las seis cajas del frente y de la celda, y el portador sembrado en `LoadFromModel` para las cinco del rack.
  - **`NumericField`:** sin cambio.
- **INV-PBTC-04. La corrección (b)** (D-PBTC-02).
  - El estilo de cotas que trae la carga llega sin cambio a cada pasada.
  - Un rack nuevo, sin estilo.
  - Ningún control nuevo para el estilo.
- **INV-PBTC-05. D-PB-08** (D-PBTC-03).
  - **El elemento:** un `TextBlock` llamado `SideBEmptyNoticeText`, inmediatamente después de `SideBPresentCheck` en WX.
  - **La regla:** visible si el lado B está declarado y ninguna de sus ranuras está activa; colapsado en cualquier otro caso.
  - **El texto:** el literal confirmado por el Owner (OD-PBTC-3), byte a byte.
  - **Lo que no es ni hace:** no es un `Control` ni es enfocable. No cambia la habilitación de ningún botón, el estado, el diseño, la casilla ni `StatusText`.
  - **La regla de capacidad sin presencia:** sin cambio.
- **INV-PBTC-06. La sustitución declarada.**
  - Solo S-01, S-02 y S-03 llevan `Skip`, cada una con su literal del §5.2.
  - En esos tres archivos, el diff es exactamente la línea del atributo.
  - Ninguna otra prueba existente se edita, se salta, se reactiva ni se borra.
- **INV-PBTC-07. T1B y guardas.**
  - T1B en verde y sin sustitución: 55 en UI y 10 en Core.
  - G-10, I-55, módulos, desviador, foco, `AnnotationScaleBox`, el censo de ventanas y el corpus de WLC, en verde en la CI completa.
- **INV-PBTC-08. Lo que sigue válido:**
  - CT-PB0-01 a CT-PB0-18, CT-PB0-20 y CT-PB0-21, en verde;
  - las listas (a)/(b) de CT-PB0-01 y CT-PB0-17, vacías (M-05);
  - lo del §5.3, en verde.
- **INV-PBTC-09. Sin efecto no medido.** La salida de la ventana, igual a la del piloto en los 14 diseños de WLC (CT-PBTC-06). Una diferencia en cualquier otro campo es PBTC-C03.
- **INV-PBTC-10. Las pruebas de UI.**
  - **La colección:** las cuatro clases nuevas de Push Back, en `EditorDiscardPromptCollection`.
  - **Las ventanas:** cada una con `Ventana()` y su carga dentro del `try`, y cerrada en `finally`, también si la carga lanza. Sin `Abrir` ni `AbrirFueraDePantalla` de S0. Ninguna ventana mostrada.
  - **Los gestos:** sin teclado ni foco real (D-PBTC-09).
  - **Prohibido:** un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar».
  - **Las costuras:** ninguna aserción dentro de una.
- **INV-PBTC-11. Las costuras.**
  - Solo las de H-28 y el contrato público de la ventana.
  - Ninguna costura nueva, y ningún cambio en `InternalsVisibleTo`, los paquetes ni los `.csproj`.
- **INV-PBTC-12. Determinismo:**
  - el GUID fijo de `Ventana()`;
  - la cultura invariante;
  - los números con «R»;
  - el orden ordinal;
  - ni hora ni aleatoriedad.
- **INV-PBTC-13. Las corridas locales.**
  - **Nunca la suite completa de UI en local,** solo los filtros del §6.4. La de Core, sí.
  - **MAX_PATH:** sin copias de árboles.
  - **Las corridas temporales** (las de §5.2, sin el `Skip`) no entran en ningún commit.
  - **El perfil del usuario:** sin cambios, con la prueba de la limpieza de INV-PBT0-06.
- **INV-PBTC-14. CI y compilación.**
  - **GREEN:** 4/4 con las suites completas.
  - **RED:** `ui-tests` en `failure`; `tests`, `build-ui` y `build-plugin` en `success`; sin `blame-hang` ni timeout.
  - **Avisos:** los conteos por código, iguales a la línea base (Core 66 xUnit, UI 28 xUnit y Plugin 104 MSB3277; PB-T0, Evidence 26).
- **INV-PBTC-15. El alcance de la tarea,** medido sobre el diff propio:
  - ni el modo WORKSPACE, ni el piloto, ni el panel;
  - ni H-PB-28, ni (c), ni (d), ni la cola de `pb-21-cola`, ni D-3, ni M-01.
- **INV-PBTC-16. El registro.**
  - El Worker no escribe en `docs/`.
  - El registro, su paso a EJECUTADA y la anotación del paso 6 en `f4b-dispositions.md` son de la sesión.
- **INV-PBTC-17. Las reglas de F4B** ([DP]; D-PB-10):
  - la planificación, según PBTC-C08;
  - los controles, según PBTC-C11;
  - antes de un intento 3, siempre el Owner (PBTC-C12).
- **INV-PBTC-18. Sin AutoCAD.** La comprobación física es de Smoke-PB ([P] §9).

### 6.6 `ExpectedEvidence`

- **Antes del Worker:**
  - el contrato y la delegación válidos;
  - la comparación byte a byte, A1-A8 y nc4 (primera delegación de la cadena);
  - el `AllowedWriteScope` de la delegación igual al del contrato;
  - si hubo una replanificación por PBTC-C08, su registro.
- **El commit RED y su push, antes del GREEN:**
  - la CI del RED de INV-PBTC-14;
  - los TRX del filtro con `ExpectRed` cumplen el criterio de INV-PBTC-02;
  - los demás filtros, en verde, con sus `Skipped` de S-01 a S-03.
- **El commit GREEN y su push:**
  - `git diff --name-only RedSha..CurrentSha` = W, WC y WX;
  - la CI del GREEN 4/4, con los conteos del §6.9 o la diferencia explicada por el diff.
- **En local:**
  - el build Release de la solución, con el Plugin y sin avisos nuevos, sobre el RED y sobre el GREEN;
  - los filtros del §6.4 sobre el RED y sobre el GREEN;
  - tres corridas seguidas en verde del filtro con `ExpectRed` sobre el GREEN;
  - la comprobación temporal del §5.2 (S-01 a S-03 sin `Skip` sobre el GREEN);
  - la prueba de la limpieza del perfil.
- **`worker-handoff.json`, con:**
  - el RED, el GREEN, el alcance real, el trailer y los conteos por filtro, con Passed, Failed y Skipped;
  - el mapa CT → método;
  - por cada CT, la primera diferencia en la base, con el mensaje del TRX;
  - la tabla de S-01 a S-03, con la prueba vieja en la base (pasa) y sin `Skip` sobre el GREEN (falla en las filas declaradas);
  - el literal de D-PB-08 tal como quedó;
  - `KnownLimitations`.
- **Después:**
  - la verificación del Controller, válida;
  - nc1, nc2 y nc3 según el README §10, conservando cada salida ordinaria;
  - un control que no da CONTROL PASS se resuelve **solo** como dice PBTC-C11;
  - cada uso, registrado en `f4b-dispositions.md`;
  - la verificación 14/14 sola no basta para cerrar.

### 6.7 `StopConditions`

**Las estándar,** como en [PK0-C]:
- S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14;
- P-02 a P-08;
- P-01 con el texto de [PK0-C]: «config.toml no longer matches the accepted Snapshot in force (today Snapshot I, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)».

**Las propias:**
- **PBTC-C01:** la corrección necesitaría cambiar un archivo fuera de las tres rutas de producto. Por ejemplo `NumericField`, otra ventana, Application o el Plugin. Es una decisión del Owner (OB-06.6).
- **PBTC-C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente fuera de S-01 a S-03, o tocar un `.csproj`, `InternalsVisibleTo`, los paquetes o `tests/RackCad.Tests/`. **Una guarda de T1B que no queda en verde vuelve al Owner** (OB-T1B.3).
- **PBTC-C03 (OB-06.4):** aparece un efecto material no medido. Es decir, una diferencia en el diseño, en las piezas, en los niveles o posiciones o en el BOM que no sale de los valores exactos de F1-F11 ni del estilo; o falla, en el GREEN, CT-PBTC-06 o una prueba del §5.3. STOP y vuelta al Owner.
- **PBTC-C04:** un método del filtro con `ExpectRed` pasa en la base, o falla por excepción y no por aserción. Es un defecto del contrato: el Worker no cambia la prueba para que falle.
- **PBTC-C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o una escritura en el perfil o en el disco.
- **PBTC-C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce en una segunda corrida y no es atribuible a ellas. Si es atribuible, es REWORK; si no se reproduce, BLOCKED, dentro de los límites de 16.8.
- **PBTC-C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; o una línea esperada de un CT no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **PBTC-C08** ([DP], pregunta 2):
  - un rechazo de la planificación **solo** por la comparación literal de las colecciones permite **una** replanificación con el prompt reforzado, sin gastar intento;
  - un segundo rechazo de fidelidad es STOP y vuelve al Owner;
  - cualquier otro rechazo de la planificación (A1-A8 u otra causa) es STOP y vuelve al Owner.
- **PBTC-C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **PBTC-C10:** hacerlo bien exigiría corregir algo fuera de D-PB-06 (a) y (b) y de D-PB-08: H-PB-28, (c), (d), la cola de `pb-21-cola`, D-3 o M-01.
- **PBTC-C11** ([DP], pregunta 1). Un control que no da CONTROL PASS se conserva tal cual, y solo hay dos salidas sin volver al Owner:
  - **nc2 con la firma exacta de PD-F4-01.** El texto literal de su fuente (`f4-dispositions.md:1103-1106`):

    > «- Scope en fail sobre la ruta quitada;
    > - Contract en fail solo por la cláusula de exactitud;
    > - `EXECUTION_BLOCKED`/STOP.»

    Y su aplicación literal (`:1540`): «ninguna otra comprobación distinta de la verificación real».

    **Qué cuenta como la cláusula de exactitud:** forma parte de la firma que la evidencia de Contract cite el contraste del `AllowedWriteScope` estrechado con el contrato o con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`). No forma parte que Contract cite cualquier otra discrepancia.

    → El sustituto **`PBTC-NC2-EXACT-WHITELIST-01`**: copia de `F4T1B-NC2-EXACT-WHITELIST-01` (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc2-substitute/1/R20261114T235423Z-083e/nc2_exact_whitelist_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc2 = SATISFIED_UNDER_PBTC_EXACT_WHITELIST_SUBSTITUTE`.
  - **nc1 con la firma exacta de PD-F4-04.** El texto literal de su fuente (`owner-f4t3-authorization.md:255-265`):

    > «* false CurrentSha correctly detected;
    > * Identity = fail;
    > * Handoff also fails first only because the handoff itself contains the false
    > SHA;
    > * outcome BLOCKED / STOP;
    > * all other intended checks match;»

    Su aplicación en F4, transcrita literal (`f4-dispositions.md:1657-1660`):

    > «- el CurrentSha falso se detecta, e Identity está en fail con una evidencia que nombra el SHA falso y el HEAD;
    > - Handoff falla primero solo porque la entrega contiene el SHA falso;
    > - `EXECUTION_BLOCKED` / STOP con S-04;
    > - solo difieren consumidores del CurrentSha: Handoff, Identity y Ci, con Remote en pass, un subconjunto de la cascada de F4-T2C.»

    Por tanto, Ci en fail solo como consumidor del `CurrentSha` falso forma parte de la firma. Ci en fail por cualquier otra causa no.

    → El sustituto **`PBTC-NC1-IDENTITY-SIGNAL-01`**: copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR-nc1-substitute/1/R20261113T215949Z-935f/nc1_identity_signal_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc1 = SATISFIED_UNDER_PBTC_NC1_IDENTITY_SIGNAL_SUBSTITUTE`.
  - **Cualquier otra firma o variación vuelve al Owner antes de cualquier sustituto** ([DP], pregunta 1, descripción de la opción). Por ejemplo:
    - PD-F4-05 y PD-F4-06;
    - nc2 con Ci o Trailer en fail además;
    - cualquier nc1 que no reproduzca la firma literal;
    - cualquier fallo de nc3.
  - **Cómo se decide la coincidencia.** Nunca por el nombre de la comprobación:
    - **qué se compara:** la salida del control frente a la de la verificación real, comprobación por comprobación, con el conjunto de comprobaciones en fail, la primera en fail, el `Outcome`, la `StopClass` y el texto de la evidencia;
    - **las únicas diferencias admitidas:**
      - PD-F4-01: Scope, en fail sobre la ruta quitada, y Contract, en fail solo por el alcance;
      - PD-F4-04: Identity (nombra el SHA falso y el HEAD), Handoff (primera en fail, solo porque contiene el SHA falso) y Ci (solo como consumidor del `CurrentSha` falso);
      - en las dos, el resultado `EXECUTION_BLOCKED` / STOP;
    - **quién decide y dónde queda:** la sesión registra la tabla en `f4b-dispositions.md`. Una diferencia fuera de esa lista vuelve al Owner.
- **PBTC-C12:** una corrección sería el intento 3. Vuelve al Owner antes de emitirse (D-PB-10).
- **PBTC-C13:** el aviso de D-PB-08 necesitaría otro texto u otro lugar distintos de los confirmados por el Owner (OD-PBTC-3), ser enfocable o bloquear una acción. Es una decisión de presentación del Owner (OB-08.4).

### 6.8 `EligibleCells`

Las de [PK0-C]:
- **Worker:** `anthropic/subagent/claude-sonnet-5-5`, effort `high` (VerifiedOn 2026-09-30).
- **Controller:** `openai/process/gpt-6-luna`, effort `high` (VerifiedOn 2026-11-11, por la sonda neutra de F4).
- **Vigencia:** las dos son `STALE` desde el 2026-12-29, por la fecha del catálogo más 90 días (`model-catalog.md:14`).
- nc4 en la primera delegación, y nc1 a nc3.

### 6.9 Línea base de la CI y conteos previstos

**La línea base,** medida con `gh` sobre la CI de `f9f2afbb` (run `44188678597`, H-25):
- Core: 19 070 Passed de 19 070. Esa cifra **ya incluye** las 3 de PB-T0 (H-PB-40).
- UI: 2 404 Passed, 17 Skipped y 2 421 en total.

**La previsión.** Sale del diff, no de una ejecución: entran 9 métodos nuevos en UI, y la Theory de WLC pasa de 14 resultados a 1 (H-26).

| CI | Core | UI |
|---|---|---|
| RED | 19 070 Passed (sin cambio) | Total 2 417; Failed 8, los de CT-PBTC-01 a CT-PBTC-08; Skipped 20, los 17 de la base más S-01, S-02 y S-03; Passed 2 389 |
| GREEN | 19 070 Passed | Total 2 417; Failed 0; Skipped 20; Passed 2 397 |

Una cifra distinta en la CI se registra y se explica por el diff. Si no se explica, es PBTC-C06 o PBTC-C07.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-01 | nc2 con la firma de PD-F4-01 | [DP]: el sustituto solo con la firma exacta; cualquier variación, al Owner (PBTC-C11) |
| R-02 | **Las líneas esperadas de los RED salen de leer el código.** No se compiló ni se ejecutó nada. Una normalización que no se vio (H-08 lo descarta para F6) haría que un CT no se cumpliera en el GREEN | PBTC-C07: STOP, sin ajustar lo esperado. El contrato no da al Worker ninguna latitud para corregir una prueba (lección de PB-T0) |
| R-03 | **«Escribir el mismo número que se ve» conserva el exacto.** Lo fija CT-PBTC-03, paso 3 | Es la regla de Cantilever (H-PB-35), y la diferencia es menor que 0,0005. Se declara al Owner (§8, OD-PBTC-1) |
| R-04 | **«Ambos lados», el alcance y los datos del frente** llevan ahora el valor exacto | Consecuencia declarada (D-PBTC-01). Una prueba existente que la contradiga es PBTC-C03 |
| R-05 | **La biblioteca y «Insertar»** conservan también el estilo y los exactos | Consecuencia declarada (D-PBTC-02), anotada en el registro |
| R-06 | **El literal de D-PB-08 queda fijado por CT-PBTC-07** | El Owner lo confirma antes de emitir (OD-PBTC-3). Cambiarlo después es otro cambio declarado |
| R-07 | **F4-E59 / I-02:** la suite completa de UI escribe en el perfil | Solo los filtros en local; la suite completa de UI, solo en la CI (INV-PBTC-13) |
| R-08 | **PB-T1 moverá los portadores al estado de Application** | Los CT aseveran sobre la salida, la petición y los controles, nunca sobre los portadores privados |
| R-09 | **Cómo cuenta el Controller un resultado `Skipped`** en un filtro sin `ExpectRed` | `MinSelected` sobre los que pasan (§6.4) |
| R-10 | **El RED con Core en `success`** | Precedente de F4-T3B1, con Core `ExpectRed` false (D-PBTC-05) |
| R-11 | **El recuento de 67 de Cantilever** es estático | `MinSelected` es un mínimo. La entrega da la cifra medida |
| R-12 | **Las celdas vencen el 2026-12-29** | Se comprueba en la emisión y en cada delegación (A7) |
| R-13 | **MAX_PATH** | Sin copias de árboles; las mediciones temporales del §5.2 se hacen y se revierten en el mismo worktree |

## 8. Decisiones para el Owner antes de emitir

Es la vuelta ya prevista: [P]:766, `OD:153-155` y F4B-E05, «Siguiente», punto 2. El paquete propone; el Owner decide.

### OD-PBTC-1. El efecto de corregir (a), el redondeo

**Qué cambia, en lenguaje llano.** Hoy, al abrir un rack con RACKEDITAR, once números se guardan redondeados a tres decimales aunque el usuario no los toque:
- del rack: el fondo de la tarima, el peralte del poste, la tolerancia, el peralte IN/OUT base y la escala de anotación;
- del frente seleccionado: la altura del primer nivel;
- de la celda seleccionada: frente, alto y peso de la tarima, el claro y el largo manual de viga.

Con la corrección se guardan exactos, y las cajas siguen mostrando tres decimales. Esto solo afecta a valores con más de tres decimales: un rack con valores de tres decimales o menos no cambia en nada.

**El efecto medido** (T-3 de PB-T0, CT-PB0-21, sobre `pb-08` con +0,00044, exacto frente a redondeado):

| Fila | Campo | Qué cambia | Máx. \|Δ\| | «Material» |
|---|---|---|---|---|
| R0 | `pb-08` tal cual: la escala 1,23456 y el alto 61,23456 | Los textos y las cotas de los 8 planos se mueven y cambian de tamaño | 0,01848 | Sí (K4) |
| R1 | Fondo de la tarima | Nada | 0 | No |
| R2 | Peralte del poste | Las piezas de los dos frontales y de la planta se mueven | 0,00132 | Sí (K4) |
| R3 | Tolerancia | Nada | 0 | No |
| R4 | Peralte IN/OUT base | Nada | 0 | No |
| R5 | Escala de anotación | Lo mismo que R0: es la escala la que mueve los textos y las cotas | 0,01848 | Sí (K4) |
| R6 | Altura del primer nivel | Nada: el ajuste al troquel la absorbe | 0 | No |
| R7 | Frente de la tarima | **Cambia el BOM** (K3). Cambia el orden interno de las piezas en el frontal posterior y la planta (K1o). En los demás planos el movimiento es de 0,001 o menos | 106,988 (ver abajo) | Sí (K3) |
| R8 | Alto de la tarima | Nada | 0 | No |
| R9 | Peso de la tarima | Nada | 0 | No |
| R10 | Claro | Nada | 0 | No |
| R11 | Largo manual de viga | **Cambia el BOM** (K3): la línea lleva 100,00044 en vez de 100. Las piezas se mueven menos de 0,001 | 0,00044 | Sí (K3) |

**Lo que dice esta tabla:**
- **Ninguna fila** cambia el número, el tipo o la capa de las piezas, ni los niveles o posiciones (K1 y K2, vacías en todas).
- **R7 y R11 son «materiales» por el BOM exacto.** El BOM se compara con todas sus propiedades exactas, y sus longitudes difieren en diezmilésimas. Es la lectura OWNER-4 de la revisión independiente de PB-T0; las líneas exactas no se midieron.
- **El 106,988 de R7 no es un desplazamiento físico.** Es un artefacto del emparejamiento por orden de aparición cuando hay K1o (OWNER-4; `RoundingEffectTests.cs:188-201`). El desplazamiento real de la planta en R7 no se midió.
- **En los dibujos reales medidos** (H-PB-38), la escala de los 22 sobres es 1: R0 y R5 no se dan en ellos. Los demás campos no se midieron.
- **El panel del Workspace ya escribe hoy los valores exactos** (H-PB-37). La corrección alinea RACKEDITAR con él.
- **La regla declarada (R-03):** si el usuario escribe el mismo número que ve, se conserva el exacto. Si escribe uno que se ve distinto, manda el suyo.
- **Las consecuencias declaradas:** «Ambos lados», el alcance y la biblioteca (D-PBTC-01, D-PBTC-02).

**Opciones:**
- **(i) Seguir con la corrección de los once campos.** *Recomendada:* el efecto es menor que 0,02 pulgadas; el BOM pasa a reflejar el valor guardado, y RACKEDITAR queda igual que el panel.
- **(ii) No corregir (a).** PB-T1 tendría que reproducir el redondeo desde un estado que ya no depende del control, lo que [P]:509 llama «imposible sin fingir el redondeo». Habría que rehacer [P].

### OD-PBTC-2. El efecto de corregir (b), el estilo de cotas

**Qué cambia, en lenguaje llano.** Hoy, RACKEDITAR olvida el estilo de cotas guardado en el sobre y dibuja las cotas con el estilo actual del dibujo, forzando el tamaño del texto (el de la escala de anotación), las flechas, los huecos, el texto encima de la línea y dos decimales (H-13). Con la corrección, si el sobre trae un estilo y el dibujo tiene un estilo con ese nombre, las cotas se dibujan con él tal cual: su tamaño de texto, sus flechas, su precisión. Si el dibujo no tiene ese estilo, todo sigue como hoy.
- **No cambian** los puntos medidos, la posición de la línea de cota, la capa, el número de cotas ni el BOM.
- **En los dibujos medidos no cambia nada.** M-7 midió 0 de 22 sobres con estilo, y los 22 tienen las cotas desactivadas (`Dimensions = 0`; H-PB-38).
- **Hoy ningún camino de la interfaz de Push Back escribe un estilo** (MB H-082). Solo puede venir de un documento que ya lo traiga.
- **El panel del Workspace ya lo conserva** (H-PB-37).

**Opciones:**
- **(i) Seguir.** *Recomendada.*
- **(ii) No corregir (b).**

### OD-PBTC-3. D-PB-08: el texto y el lugar del aviso

- **El lugar:** justo debajo de la casilla «Rack de dos sentidos (lado B)», con letra pequeña y color secundario. Solo se ve mientras el lado B está declarado y no tiene ningún frente activo. Es solo texto: no bloquea ninguna acción ni se puede enfocar (D-PBTC-03).
- **El texto propuesto:**

  > «Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.»

- **Variante más explícita,** que añade la consecuencia de reabrir y solo es exacta en el modo clásico (WC:812):

  > «Lado B declarado sin ningún frente activo: así no se guardará, y al reabrir el rack la casilla aparecerá desmarcada. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.»

- **A juicio de este paquete no es una decisión material:** no cambia datos ni el dibujo. Pero CT-PBTC-07 fija el literal byte a byte. Se presenta ahora para que el Owner lo confirme o lo cambie antes de emitir. *Recomendado:* el texto propuesto, porque vale en los dos modos.

### OD-PBTC-4. H-PB-28: «Restaurar estándar» se pierde al Actualizar

**El defecto medido** (T-4 de PB-T0; G-1 y G-2 de las mediciones):
- **`pb-06`:**
  - tras «Restaurar estándar» y «Confirmar», la vista previa ya muestra reiniciados los seis parámetros avanzados (altura manual de cabecera, poste derivado, refuerzo, número y separación de separadores);
  - los controles, en cambio, siguen mostrando 150, false, 3 y 18;
  - al Actualizar, la petición vuelve a escribir 150, false, 3 y 18: la restauración se pierde.
- **`pb-07`:** `DerivedPostHeight` pasa de 140 a nulo y vuelve a 140.
- **La causa:** el reinicio se aplica a los inputs de una sola pasada (DA:300-303). El portador `advanced` de la ventana y sus controles no se tocan, y la pasada siguiente lo vuelve a copiar (W:830-835).

**El arreglo** sería hacer, tras una pasada válida que consume «Restaurar estándar», lo que ya hace «Restaurar parámetros» (W:1628-1634). Son unas pocas líneas en W. Cambia lo que RACKEDITAR escribe en ese flujo: la altura de cabecera, el poste derivado y los separadores pasan a ser los estándar, que es lo que el usuario pidió. Por eso es una corrección declarada nueva y la decide el Owner.

**Opciones, con su coste:**
- **(A) Meterlo en PB-TC, como corrección declarada nueva** (D-PB-11).
  - Este paquete pasa a una v2: un CT nuevo, la sustitución S-04 de CT-PB0-05 (sus aserciones A1 y A2 cambian) y dos rutas más en `AllowedWriteScope` (`PushBackFullEditorCharacterizationModuleTests.cs` para el `Skip`, y la clase nueva). Necesita otra revisión.
  - Coste: una revisión más, y ninguna cadena nueva.
- **(B) Una tarea propia, PB-TC2,** después de PB-TC y antes de PB-T1.
  - Coste: una cadena completa más (paquete, revisión, planificación, Worker, CI, verificación, revisiones y controles), con su presupuesto de 1/3.
- **(C) PB-T1 lo reproduce tal cual, como deuda.**
  - El estado extraído tendría que conservar a propósito que el reinicio solo vale una pasada.
  - CT-PB0-05 sigue en verde.
  - Arreglarlo después costaría la misma sustitución de CT-PB0-05, más tarde.
- **(D) Arreglarlo dentro de PB-T1.** Choca con «Do not hide these corrections inside PB-T1 state extraction» ([OD]:152), aplicado por analogía. **No recomendada.**

*Recomendada:* (A) si el Owner decide en esta misma vuelta, porque la emisión de PB-TC ya espera su orden; (B) si prefiere no tocar el alcance de PB-TC. **Este paquete no la incluye** (D-PBTC-07, PBTC-C10).

### Informativo: las guardas de T1B

Ninguna guarda de T1B deja de estar en verde con este diseño (H-18). No hay vuelta al Owner por OB-T1B.3.

## 9. Límites de esta medición

Están en `pbtc-measurements.md`, apartado J. En resumen:
- no se compiló ni se ejecutó nada: los RED, los GREEN y las cifras del §6.9 son predicciones a partir del código (R-02);
- no se midieron las líneas exactas del BOM de R7 y R11, ni el desplazamiento real de R7 en la planta;
- no se midió cuántos sobres reales tienen F1-F4 o F6-F11 con más de tres decimales;
- no se midió el aspecto en AutoCAD de una cota con estilo nombrado; es de Smoke-PB;
- el recuento de 67 de Cantilever es estático;
- no se sabe si el Controller cuenta un `Skipped` como seleccionado.
