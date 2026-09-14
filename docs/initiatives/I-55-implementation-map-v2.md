# I-55 — Implementation Map V2: View Placement & Projection

```text
PROPOSAL V2 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED
Open Material  = M-01 (propio) · X-1..X-8 (entre iniciativas, con I-52)

Acompana a     = docs/initiatives/I-55-proposal-v2.md (misma version del plan; ninguno vale sin el otro)
Sustituye a    = docs/initiatives/I-55-implementation-map-v1.md (historico, sin modificar)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/
```

> Este mapa **no autoriza** ningun gate. Tipos y pruebas nuevos son propuestos; rutas y simbolos existentes estan verificados sobre
> `CURRENT_BASE`. Los conteos de pruebas se registran en la evidencia de cada gate (AGENTS.md).

## 0. Convenciones

| Campo | Contenido |
|---|---|
| Precondiciones | Gates previos, acuerdos entre iniciativas, decisiones del Owner |
| RED | Prueba vista **fallar** sobre el SHA previo (I-45). Tipos nuevos: esqueleto + pruebas. Caracterizaciones: verdes sobre codigo intacto; sus guardas demuestran deteccion por teoria de mutacion |
| GREEN | Focales con conteo > 0 (filtro por nombre de clase; namespace plano), Core + UI completas, builds Debug que correspondan, CI `push` 4/4 sobre el SHA exacto |
| Rollback | `git revert` de los commits del gate; **ningun gate migra datos** |
| Pruebas de UI | Nunca recorren rutas con `MessageBox`/`ShowDialog` reales (hilo STA compartido, `TU/DynamicHeaderBatchSeamGuardTests.cs:13-15`) |
| Apertura | `git fetch --all --prune`; rebase si `main` avanzo (WORKFLOW §4.2); re-medicion; reporte de X-5 si el gate lo exige |

## 1. Secuencia y dependencias

```text
G3 ─┬─ G4 (PR-1, H-01) ─┐
    └─ G5 (PR-2, H-02) ─┴─ G6 (foundation) ─ G7 (plan + preparacion) ─ G8 (colocacion) ─ G9 (precondiciones de Insertar)
                                                                                                     │
    ┌────────────────────────────────────────────────────────────────────────────────────────────────┘
    └─ G10 (ID17) ─ G11 (ID18 contrato) ─ G12 (ID18 UI) ─┐
                                   G13 (X-1, con acuerdo) ─┴─ G14 (ID19 puro) ─ G15 (ID19 comando) ─ G16
```

| Gate V1 | Gate V2 | Cambio |
|---|---|---|
| G3, G4, G5 | G3, G4, G5 | G5 (PR-2) pasa a cambiar solo el Plugin |
| G6 (PR-3, H-13) | — | **Retirado** (CR-07) |
| G7, G8, G9 | G6, G7, G8 | Renumerados |
| — | G9 | **Nuevo**: precondiciones de Insertar (gate de propiedades acotado al `RackId` y D-15) en `RACKEDITAR` |
| G10..G16 | G10..G16 | G14 y G15 redisenados (Group Placement y politicas) |

| Coordinacion | Gates de I-55 | Contraparte (I-52 V11) | Regla |
|---|---|---|---|
| Reconciliacion obligatoria de las seis autoridades compartidas | Antes del Consensus Freeze (precondicion de G3) | [V11-D10], §14.1, R-45 | Propiedad registrada sin duplicar; STOP simetrico si cualquiera congela antes una autoridad incompatible (Proposal §17.2) |
| X-1 nucleo de seleccion | G3 (condicional), G13 | §5.1 | Una extraccion; hechos neutrales; politicas por llamador |
| X-2 plan por vista + codec | G3, G6, G7 | §3.7, §8.1, §8.4, §20.1, CT-04 | Resolucion separada; paso comun «sistema resuelto → plan»; codec neutral; politica por consumidor |
| X-3 comparador por kind | G14 | §5.4 | Contrato del kind; miembros como parametro |
| X-4 primitivo de materializacion | G15 | §8.2, §8.7 | Politicas de equivalencia visual fuera del primitivo |
| X-5 clausula de I-55 | G5, G6, G7, G8, G9, G10, G12, G13, G14, G15 | §15.2, §15.3, §19 | Reporte antes de fijar archivos, con la lista de re-evaluacion (§8.1, §8.7, `MaterializationContextReadSet`, PRE-17, PRE-18, T-GRD-02, censo `B`) |
| X-6 lineas base de C-2 | G5, G8, G9, G12 | §8.5 (C2-2a, C2-2b, C2-4, G-M9) | La segunda en integrar re-establece C-2 |
| X-7 valor de colocacion y composicion | G14, G15 | §3.2, §3.3, §8.3 | Un valor de colocacion y una composicion sobre `Transform2D` |
| X-8 origen y tramo del eje de una vista | G3, G6, G14 | §8.1 (`ViewPlanResult`), CT-05 | Un descriptor y una sola caracterizacion |
| Archivo caliente del Selectivo | G6, G10, G12 | I-49 G10 | Serializar |
| Censos | G7, G12, G15 | I-52 (`Compose`, comandos, ayuda) | La segunda en integrar re-basa |

## 2. Gates

### G3 — Caracterizacion (solo pruebas)

| Campo | Contenido |
|---|---|
| Objetivo | Fijar sobre produccion intacta lo que I-55 preserva, cambia o necesita medir |
| Precondiciones | Coordinator = AGREED y Architect = AGREED sobre la misma version; **M-01 resuelta**; reconciliacion con I-52 registrada ([V11-D10]); Consensus Freeze; ADR-0042 aceptado; OD-1..OD-8 decididas; X-1..X-8 acordados |
| Produccion | Ninguna |
| T/ `RackViewEnvelopeReadingCharacterizationTests` | `PushBackSystemFrontalBuilder.EncodeSection/DecodeSection/IsValidSection`, `CantileverViewPlanBuilder.SectionFor`, `RackListBuilder` con `View` vacio (`A/Persistence/RackListBuilder.cs:117-118`); guarda de las lecturas del Plugin, con la lista de I-52 V10 §8.4 (`P/RackSelectivoCommands.cs:126-177`, `:216`; `P/RackDinamicoCommands.cs:209-239`, `:392-393`; `P/RackPushBackCommands.cs:216-262`, `:427-450`; `P/RackCantileverCommands.cs:295-314`, `:501-540`; `P/RackCabeceraCommands.cs:239-240`; `P/ProjectVariableMutationExecutor.cs:180-218`) mas `P/RackBlockFinder.cs:23-24` y `P/RackLayoutCommands.cs:71` — **una sola** caracterizacion con la CT-04 de I-52 (V11); fija las disposiciones de la Proposal §7.3 y las variantes huerfanas |
| T/ `LegacyViewPayloadCompositionCharacterizationTests` | Formula de composicion por sistema + guarda de `BuildSelectivePayload`/`WrapSelectivePayload` (`P/RackSelectivoCommands.cs:344`, `:382`), `BuildDynamicPayload` (`P/RackDinamicoCommands.cs:361`), `BuildPushBackPayload` (`P/RackPushBackCommands.cs:397`), `BuildCantileverPayload` (`P/RackCantileverCommands.cs:472`), `BuildCabeceraPayload` (`P/RackCabeceraCommands.cs:181`), `BuildCamaPayload` (`P/RackCamaCommands.cs:175`, `:183`) |
| T/ `LegacyViewBlockNameCharacterizationTests` | Nombres base de los servicios de dibujo |
| T/ `RackCountInvariantCharacterizationTests` | Vistas frente a racks en `RackListBuilder`; representante de `BomAuthoredAuthority` (`A/Bom/BomAuthoredAuthority.cs:104-109`) |
| T/ `RackSiblingScanInputCharacterizationTests` | Guarda de fuentes: `ScanEnvelopes` recorre definiciones con payload no interpretable (sobre nulo) y omite las de xref pero no las dependientes (`P/RackBlockFinder.cs:57-91`); `FindRackBlocks` no expone dependencia ni texto (`P/RackCommandSupport.cs:109-134`) |
| T/ `RackViewFrameCharacterizationTests` | Descriptores de la Proposal §12.3 sobre la salida de los builders: ejes y signos, origen fisico, `anchorOffset` de esquina (`A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`), cuadricula de la frontal de un fondo frente a la maestra (`A/Systems/Selective/SelectiveFrontalBuilder.cs:50`; `A/Systems/Selective/SelectiveDepthLayout.cs:52-55`), Push Back compuesto, Dinamico, cabecera, Cantilever (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`), **y extension fisica sobre R y D** — compartida con la CT-05 de I-52 (X-8) |
| T/ + TU/ `HeadlessResolutionParityCharacterizationTests` | Resolucion sin editor frente al sistema del editor, por kind (compartida con la C2-4 de I-52) |
| TU/ `FirstViewGateCharacterizationTests` | Habilitacion y tooltips de las puertas de primera vista, sin rutas modales |
| Condicional X-1 | T/ `RackDuplicationPlanObservableSemanticsTests` |
| RED | Caracterizaciones verdes sobre codigo intacto; cada guarda demuestra deteccion por teoria de mutacion |
| GREEN | Clases nuevas con conteo; Core + UI; CI 4/4 |
| Impacto | Ninguno |
| Si falla una caracterizacion de marco | El par queda fuera de ID19 hasta una Proposal nueva; nunca se ajusta el descriptor |

### G4 — PR-1: poste real en la lateral de Push Back (H-01)

| Campo | Contenido |
|---|---|
| Objetivo | La seccion lateral de Push Back es el `PostIndex` del corte, no la posicion en la lista |
| Precondiciones | G3 |
| Produccion | `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (`:3036-3045`; `SelectedView` `:3054-3067`, hoy `:3065`), enteros de hoy |
| Pruebas | TU/ `PushBackLateralPostIndexRegressionTests.SelectedLateral_UsesTheCutPostIndex_WhenABoundaryIsSuppressedBeforeIt` |
| RED | Vista fallando |
| GREEN | Focal + `FullyQualifiedName~PushBack` UI con conteo; Core + UI; Debug UI + Plugin; **OV-PR1** |
| Impacto | Corte insertado correcto con fronteras suprimidas; bloques existentes sin migrar (R-09) |

### G5 — PR-2: visibilidad de la planta Cantilever (H-02), solo en el Plugin

| Campo | Contenido |
|---|---|
| Objetivo | La planta que inserta y redibuja el Plugin respeta la visibilidad que guarda el diseno, como ya hace la vista previa |
| Precondiciones | G3; X-5 reportado; X-6 (cambia un productor de C2-2a/C2-2b de I-52) |
| Produccion | `P/RackCantileverCommands.cs:131` y `:312`: pasar `design.PlantaVisibility` (`D/Systems/Cantilever/CantileverLineDesign.cs:362-364`) a `CantileverViewPlanBuilder.Build`. **Sin** cambios en Application: `A/Systems/Cantilever/CantileverLineEditorAssembler.cs` (`:148`, linea base C2-4 de I-52) queda intacto |
| Pruebas | T/ `CantileverPlantaVisibilityGuardTests.BothPluginPlanCalls_PassTheDesignPlantaVisibility` (guarda de fuentes con teoria de mutacion: quitar el argumento de cualquiera de las dos llamadas la hace fallar); T/ `CantileverPlantaVisibilityBuilderTests.Planta_DrawsArmsAndBraces_OnlyWhenTheDesignShowsThem` (comportamiento del builder: `null` apaga, `ShowArms`/`ShowBraces` encienden; frontal y lateral siempre completas, `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:293-295`) |
| RED | La guarda vista fallando sobre el codigo previo; la prueba del builder es caracterizacion (verde) |
| GREEN | T0 `FullyQualifiedName~CantileverPlantaVisibility` con conteo; `FullyQualifiedName~Cantilever` Core; Core + UI; Debug Plugin; **OV-PR2** |
| Impacto | Solo plantas cuyo diseno muestra brazos o tensores; ninguna otra vista ni la vista previa |

### G6 — Foundation pura

| Campo | Contenido |
|---|---|
| Objetivo | D-02, D-03 y D-04, con los descriptores de marco, sin cambio de comportamiento |
| Precondiciones | G4, G5; X-5 reportado por el renombre; serializado con I-49 G10 |
| Produccion nueva | `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDecodeDisposition.cs`, `RackViewEnvelopeCodec.cs` (disposicion + politicas por consumidor de la Proposal §7.3), `RackViewSupport.cs` (incluida la disponibilidad de variantes), `RackViewFrame.cs` (ejes, origen y extension fisica; X-8) |
| Produccion modificada (renombre) | `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs`, `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs`, `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`, `U/Systems/Selective/RackSelectiveWindow.xaml.cs` |
| Pruebas nuevas | T/ `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewEnvelopeCodecTests` (bytes y disposiciones de G3; **politica de ID19: `Canonical` y `Canonicalizable` aceptan sin reescribir; `Coerced`, `Invalid` y variantes huerfanas fallan con diagnostico**; `RACKEDITAR` sin cambio), `RackViewSupportMatrixTests` (una por celda de la Proposal §5), `RackViewFrameTests` (reproducen `RackViewFrameCharacterizationTests`) |
| Pruebas modificadas | Renombre en las seis clases de pruebas que nombran el tipo |
| RED | Esqueleto + pruebas vistas fallando |
| GREEN | T0 `FullyQualifiedName~RackView`; T1 `FullyQualifiedName~DimensionView`; con conteo; Core + UI; Debug UI |
| Impacto | Ninguno observable |

### G7 — Plan por vista desde el sistema resuelto y preparacion (X-2)

| Campo | Contenido |
|---|---|
| Objetivo | D-01 en Application |
| Precondiciones | G6; acuerdo X-2 (si I-52 ya integro su autoridad, se consume re-verificando su API y cubriendo laterales); X-5 reportado |
| Produccion nueva | `A/Views/Preparation/*` (contexto, resultado, registro, seis preparadores, `RackViewEnvelopeComposition.cs` con la unica llamada nueva a `Compose` y el origen como parametro `RackEmbedDocument`, `RackViewBlockNaming.cs`); paso comun «sistema resuelto → plan por vista» en la ubicacion acordada |
| Pruebas nuevas | T/ `RackViewPreparation{Selective,Dynamic,PushBack,Cantilever,Cabecera,Cama}Tests`, `PreparedViewPayloadGoldenTests` (**byte a byte = formula legacy de G3**), `RackIdLifecycleTests` |
| Guardas reapuntadas con motivo | `CustomPropertiesEnvelopeGuardTests.TGrd02_*` (+1 archivo y llamada; primer argumento parametro `RackEmbedDocument`), sin debilitarlas |
| RED | Esqueleto + goldens vistos fallando; `TGrd02` falla hasta reapuntarse |
| GREEN | T0 `FullyQualifiedName~RackViewPreparation\|FullyQualifiedName~PreparedViewPayload\|FullyQualifiedName~RackIdLifecycle` con conteo; T1 `FullyQualifiedName~CustomPropertiesEnvelopeGuard`; Core + UI |
| Impacto | Codigo nuevo sin consumidores |

### G8 — Colocacion de vista unica

| Campo | Contenido |
|---|---|
| Objetivo | Toda insercion de vista unica pasa por preparacion + colocacion sin cambio observable; D-10; prompt |
| Precondiciones | G7; coordinacion G-M9 (X-6); X-5 reportado |
| Produccion nueva | `P/Views/RackViewPlacement.cs` |
| Produccion modificada | `P/Drawing/BlockPlacement.cs` (`PlaceAndReport`: limpieza ante excepcion `:49-52` y parametro de prompt; `PlaceDefinition`: limpieza ante excepcion); insercion en `P/RackSelectivoCommands.cs` (`:426-453`), `P/RackDinamicoCommands.cs`, `P/RackPushBackCommands.cs`, `P/RackCantileverCommands.cs` (incluida la definicion confirmada en `:151-157`), `P/RackCabeceraCommands.cs` (`:164-176`, `:294-310`), `P/RackCamaCommands.cs` (`:102-106`) |
| Pruebas | T/ `RackViewPlacementGuardTests` (limpieza ante excepcion en ambas primitivas; ninguna insercion compone el sobre), con teorias de mutacion; fixtures de equivalencia de Insertar (X-6); reapuntadas con motivo: `SelectiveAuthoredCarrierAdoptionTests.GUARDA_LaInsercionNUEVA_CONSERVA_From`, `PushBackPluginSourceGuardTests` |
| RED | Guardas vistas fallando |
| GREEN | Guardas y goldens con conteo; Core + UI; Debug Plugin; **OV-LEG** |
| Impacto | Alto riesgo de regresion (R-01); D-10 no observable por el Owner (R-08) |

### G9 — Precondiciones nuevas de `RACKEDITAR` → Insertar (vista unica)

| Campo | Contenido |
|---|---|
| Objetivo | Gate de propiedades acotado al `RackId` (D-07, Proposal §9.2) y D-15 en las ramas de insercion; Actualizar sin cambio |
| Precondiciones | G8; X-5 reportado; X-6 (cambia a proposito la linea base C2-2b) |
| Produccion nueva | `A/Views/RackSiblingCustomPropertiesGate.cs` (consume `CustomPropertiesStore.ReadElement`, `A/Persistence/CustomPropertiesStore.cs:90`, y `CustomPropertiesCanonicalForm`, `A/CustomProperties/CustomPropertiesCanonicalForm.cs:24-70`); `A/Views/RackEnvelopeIdProbe.cs`; lectura de entradas del gate en el Plugin (sobre, texto del payload, dependencia de xref), p. ej. `P/Views/RackSiblingScan.cs` |
| Produccion modificada | Ramas de insercion de `EditSelective`, `EditDynamic`, `EditPushBack`, `EditCantilever` y `EditCabecera` (`P/RackSelectivoCommands.cs`, `P/RackDinamicoCommands.cs`, `P/RackPushBackCommands.cs`, `P/RackCantileverCommands.cs`, `P/RackCabeceraCommands.cs`): gate antes de preparar y antes del primer redibujo; un redibujo fallido de un miembro del gate impide insertar (D-15) |
| T/ `RackSiblingCustomPropertiesGateTests` | Sin sobre fuente ni miembros → sin gate; el sobre elegido siempre es miembro (incluida la cura de un `Id` en blanco); comun → hereda la coleccion del sobre elegido; misma forma canonica con distinta version menor → comun; divergente → fallo con remedio condicionado; miembro no escribible (`PresentButUnreadable`, `IncompatibleMajor`, `AmbiguousIdentity`, `DepthLimitExceeded`) → fallo; `Kind` en blanco o varios kinds (`OrdinalIgnoreCase`) → fallo; definicion dependiente de xref ignorada; payload no interpretable con `Id` de nivel superior igual → fallo; sin `Id` legible o con otro `Id` → no bloquea |
| T/ `RackEnvelopeIdProbeTests` | `Id` de nivel superior sin distinguir mayusculas; `Id` de un diseno anidado ignorado; duplicados (cualquier coincidencia cuenta); JSON roto despues del `Id` → legible; roto antes → no atribuible; MAJOR mas nuevo → legible |
| T/ `RackSiblingGateWiringGuardTests` | Guarda de fuentes con teoria de mutacion: gate en cada rama Insertar antes de preparar y del primer redibujo; nunca en Actualizar; D-15 sobre los mismos miembros |
| T/ `RackSiblingGateIndependenceGuardTests` | El gate de propiedades vive en su archivo y no nombra Project Variables ni `RackCustomPropertiesAuthority` (fuera de la clasificacion de `TGrd05`, `T/CustomPropertiesEdgeGuardTests.cs:287-298`); G14 la extiende al gate authored |
| Guardas que deben seguir verdes | `CustomPropertiesEdgeGuardTests.TGrd04_*`, `TGrd05_*`; `RackUnitsGuardSourceTests.EditInsert_IsGatedByUpdateOnly_AndWarnsBeforeTheFirstRedraw` |
| RED | Esqueletos y guardas vistos fallando |
| GREEN | T0 `FullyQualifiedName~RackSiblingCustomPropertiesGate\|FullyQualifiedName~RackEnvelopeIdProbe` y T1 `FullyQualifiedName~RackSiblingGate\|FullyQualifiedName~CustomPropertiesEdgeGuard\|FullyQualifiedName~RackUnitsGuardSource`, con conteo; Core + UI; Debug Plugin; **OV-META-04..06** |
| Impacto | Insertar en un rack con propiedades divergentes o ilegibles, o con un redibujo fallido, falla con motivo (R-05, R-12, R-15) |

### G10 — ID17: primera vista libre

| Campo | Contenido |
|---|---|
| Objetivo | Puertas de la Proposal §10 |
| Precondiciones | G9; serializado con I-49 G10; X-5 reportado |
| Produccion | `U/Systems/Selective/RackSelectiveWindow.xaml.cs` (`:274-292`, `:2550-2570`); `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs` (`:189-194`, `:3394-3403`); `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs` (`:85-93`, `:265-269`); `U/Editor/RackInsertionRequest.cs` (`HeaderInsertionRequest`, `:36-53`); `U/Editor/EditorModules.cs`; `P/RackCabeceraCommands.cs`; `P/RackMenuCommands.cs` |
| Pruebas | TU/ `FirstViewFreedomTests`; reapuntadas con motivo: `FirstViewGateCharacterizationTests`, `DynamicHeaderBatchSeamGuardTests.D34_GUARD_*` (un `MessageBox` → cero), `SelectiveHeaderBatchSeamTests.S30_GUARD_*` (cuatro → tres), `DynamicShellMigrationTests`, `RichEditorBlockedActionTests`, `RackInsertionRequestTests` |
| RED | `FirstViewFreedomTests` vistas fallando |
| GREEN | TU/ `FullyQualifiedName~FirstView` con conteo; Core + UI; Debug UI + Plugin; **OV-ID17** |
| Impacto | Cambio intencional de UX de creacion |

### G11 — ID18: contrato puro

| Campo | Contenido |
|---|---|
| Objetivo | D-06 sin UI, con los estados de gate y D-15 |
| Precondiciones | G10 |
| Produccion | `A/Views/RackViewBatchPlan.cs` (orden, estados `SIBLING_GATE_FAILED`, `ABORTED_BEFORE_WRITE`, `REDRAW_FAILED`, cola, mensajes); `U/Editor/RackEditorSession.cs` (`RequestInsertViews`); `U/Editor/RackInsertionRequest.cs` (`Views`; historicos = primera direccion) |
| Pruebas | T/ `RackViewBatchPlanTests` (maquina completa, mensajes exactos, Esc detiene); TU/ `EditorViewBatchRequestTests` (una aceptacion → un `RackId`); reapuntadas: `RackEditorSessionTests`, `RackInsertionRequestTests` |
| RED | Vistas fallando |
| GREEN | T0 `FullyQualifiedName~RackViewBatch\|FullyQualifiedName~EditorViewBatch` con conteo; Core + UI |
| Impacto | Codigo nuevo sin consumidores en el Plugin |

### G12 — ID18: dialogo, presentador, driver y rutas de entrada

| Campo | Contenido |
|---|---|
| Objetivo | ID18 de extremo a extremo |
| Precondiciones | G11; serializado con I-49 G10; X-5 reportado; X-6 |
| Produccion nueva | `U/Views/RackViewBatchDialog.xaml(.cs)` (arquetipo C), `U/Views/RackViewBatchDialogPresenter.cs` (fuera de los archivos de ventana), `P/Views/RackViewBatchDriver.cs` (gate → PREPARE ALL → redibujo → `REDRAW_FAILED` o cola; `Regen` segun la Proposal §11) |
| Produccion modificada | Ventanas Selectivo, Dinamico, Push Back, Cantilever y configurador de cabecera; `U/Editor/EditorModules.cs` (`:53-56`, `:90-95` reenvian `Views`); `RACKSELECTIVO` (`P/RackSelectivoCommands.cs:25-45`); cabecera (un acunado por intencion); comandos de creacion; `P/RackMenuCommands.cs`; ramas `Edit*` |
| Pruebas | TU/ `RackViewBatchDialogTests`; T/ `RackViewBatchDriverGuardTests` (gate y PREPARE ALL antes de escribir; `REDRAW_FAILED`; Esc detiene), `RackViewEntryPathGuardTests` (toda ruta reenvia `Views` y acuna una vez); reapuntadas: `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `EditorModuleRegistryTests`, `RackUnitsGuardSourceTests`, `CantileverPluginSourceGuardTests.ExactlyOneRegenForTheWholeBatch_AndItAgreesWithWhatIsReported` |
| RED | Vistas fallando |
| GREEN | Focales con conteo; Core + UI; Debug UI + Plugin; **OV-ID18**, **OV-META-01..03** |
| Impacto | Boton nuevo en cinco editores |

### G13 — Nucleo neutral de seleccion (X-1)

| Campo | Contenido |
|---|---|
| Objetivo | Nucleo neutral disponible sin cambiar `RACKDUPLICAR` |
| Precondiciones | Acuerdo X-1; X-5 reportado; coordinacion con I-49 antes de editar `A/Persistence/RackDuplicationPlan.cs` |
| Si I-52 ya lo extrajo | Consumirlo; T/ `RackSelectionCoreConsumptionTests` |
| Si no | Extraer con `RackDuplicationPlanObservableSemanticsTests` (G3) y la fachada verde; pruebas de I-51 sin cambios |
| RED | Pruebas de consumo vistas fallando; caracterizacion verde antes y despues |
| GREEN | `FullyQualifiedName~RackDuplicationPlan\|FullyQualifiedName~RackSelectionCore` con conteo; Core + UI |

### G14 — ID19 puro: Group Placement y politicas

| Campo | Contenido |
|---|---|
| Objetivo | D-07 (ID19), D-08, D-13, D-16, D-17 y D-18 sin AutoCAD |
| Precondiciones | G12, G13; **M-01 resuelta**; acuerdos X-3, X-7 y X-8; X-5 reportado |
| Produccion nueva | `A/Views/Placement/SourceGroupFrame.cs`, `TargetGroupFrame.cs`, `CommonTransform2D.cs` (sobre `Transform2D`, `A/Geometry/Transform2D.cs:20-116`; `Determinant` `:77`, `ScaleFactor` `:83`, `Then` `:86-93`, `RotationAngle` `:111-115`), `RackGroupAnchorPolicy.cs` (`Rigid`, `Orthographic`), `RackGroupPlacementPlan.cs` (orden RESOLVE → VALIDATE → GATES → FRAMES; grupos D-08b; clasificacion D-08f/g/h; gates de la Proposal §9 por grupo; pares, modos y variantes segun M-01; salidas bloqueadas D-17; anclas, colocaciones, aviso de superposicion, mensajes); `A/Views/RackSiblingAuthoredGate.cs`; comparador por kind de X-3 |
| T/ `CommonTransform2DTests` | Rigidez (`Determinant = +1`, `ScaleFactor = 1`, sin cizalla); composicion `Translation(−B).Then(Rotation(α)).Then(Translation(T))` lleva `B` a `T`; distancias y angulos entre anclas preservados; **una sola instancia por ejecucion, compartida por todos los grupos `RackId`** |
| T/ `RackGroupFrameTests` | `B` y `T` de SCP a universal; `φ_s`, `φ_t` segun OD-6.b/c; SCP cuyo plano XY no es paralelo al universal → fallo |
| T/ `RackRigidPlacementPolicyTests` | Misma clase con `α = 0` y `α ≠ 0`; layout fuente rotado; orientaciones distintas entre racks (`ρ_r = φ_r + α`); escala (−1,−1) como giro de π; `Z_r = T.Z + (A_r.Z − B.Z)`; familias mezcladas; frontal ↔ lateral como sustitucion; `Origin ≠ 0`; variante destino segun OD-2.b; INV-GRP-1 para todo par de referencias de grupos distintos; INV-GRP-2 |
| T/ `RackOrthographicPlacementPolicyTests` | Planta → frontal (una fila; dos filas segun OD-7.b); planta → lateral (superposicion cuando comparten D); frontal → planta; lateral → planta; recta `e` rotada; **sentido de `e` elegido por el destino** (mayoria de `σ_s = σ_t`; empate en [−45°, 135°)); una planta a 0 y a 2π − ε dan el mismo resultado; ida y vuelta planta → frontal → planta conserva el orden y la separacion; tramos distintos alinean los postes 0; resultado independiente del orden de seleccion; rack girado 180° respecto de la mayoria (mismo intervalo, ancla en el extremo opuesto); orientaciones no paralelas → fallo (OM-13); sentidos perpendiculares en destino → fallo (OM-12); familias mezcladas segun OD-7.d; `Z_r = T.Z`; invariante sobre K; `ρ_r` depende solo de `φ_t` y de la vista destino |
| T/ `RackGroupPlacementPlanTests` | Resolucion antes de validar; todo fallo de clasificacion, resolucion, validacion (incluida la superposicion con OD-7.b = B), gates y planes antes de los puntos; layout independiente y **enlazado** (una definicion nueva, N referencias); varias definiciones de un `RackId` y tipos fuente mezclados segun OD-7.c; `Coerced`, `Invalid` y variante huerfana → fallo; MINSERT; espejo; escala de valor absoluto ≠ 1 o no uniforme; normal ≠ Z; `RackId` en blanco; authored divergente o ilegible; propiedades divergentes o ilegibles del grupo (incluido un payload con `Id` legible igual); payload sin `Id` atribuible que no bloquea; Cama; salida bloqueada; par, modo o variante no expuestos; miembro no soportado lista los miembros (OD-3) |
| T/ otras | `RackSiblingAuthoredGateTests`; `RackSiblingGateIndependenceGuardTests` extendida (el gate authored no nombra Custom Properties); `RackViewCountInvariantTests`, `RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests` |
| RED | Vistas fallando |
| GREEN | T0 `FullyQualifiedName~CommonTransform2D\|FullyQualifiedName~RackGroup\|FullyQualifiedName~RackRigidPlacement\|FullyQualifiedName~RackOrthographicPlacement\|FullyQualifiedName~RackSiblingAuthoredGate\|FullyQualifiedName~RackViewCountInvariant\|FullyQualifiedName~RackViewAuthoredEquivalence\|FullyQualifiedName~RackViewPartialRack`; T1 `FullyQualifiedName~RackSiblingGateIndependence`; con conteo; Core + UI |

### G15 — ID19: comando y primitivo de materializacion

| Campo | Contenido |
|---|---|
| Objetivo | ID19 de extremo a extremo |
| Precondiciones | G14; OD-1 y OD-8 decididas; acuerdos X-4 y X-7; X-5 reportado |
| Produccion nueva | `P/RackProyectarCommands.cs` (`RACKPROYECTAR` + `RPY` si OD-1 = A), SNAPSHOT propio (transformacion completa, dependencia de xref, texto del payload, MINSERT, escala, normal); primitivo «definicion + sobre + referencia con rotacion en la transaccion del llamador» (X-4) y valor de colocacion (X-7), o su consumo |
| Produccion modificada | `U/RackCommandReference.cs` |
| Pruebas | T/ `RackProjectionCommandGuardTests` (resolver, validar, gates y planes antes de los puntos; importar y re-verificar despues; una transaccion; sin `Regen` ni purga; `TryResolve`; definicion nueva con `Origin = 0`); reapuntadas: `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`, `CustomPropertiesCommandGuardTests.TGrd08_*` (censo por nombre), `CustomPropertiesHelpCensusTests`, `RackUnitsGuardSourceTests` |
| RED | Guarda vista fallando |
| GREEN | Focales con conteo; Core + UI; Debug Plugin; **OV-ID19** |
| Impacto | Comando nuevo |

### G16 — Candidato, validacion del Owner e integracion

Documentacion de usuario (README, `RACKAYUDA`, guia de validacion) y cierre del contrato; Candidato con T2 sobre el SHA exacto; CI 4/4;
validacion completa del Owner (§OV) sobre ese SHA; integracion `--no-ff`; CI del merge y coberturas del `MERGE_SHA` y del Candidato antes
de limpiar; ROADMAP y HANDOFF solo en ese cierre.

## T. Matriz de pruebas

| Gate | Clases | Tipo | Suite |
|---|---|---|---|
| G3 | Caracterizaciones de lecturas, payloads, nombres, conteos, entradas del barrido, marcos (con extension fisica), paridad sin editor, puertas de primera vista; condicional X-1 | comportamiento + guarda | Core + UI |
| G4 | `PushBackLateralPostIndexRegressionTests` | UI | UI |
| G5 | `CantileverPlantaVisibilityGuardTests`, `CantileverPlantaVisibilityBuilderTests` | guarda + comportamiento | Core |
| G6 | `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewEnvelopeCodecTests`, `RackViewSupportMatrixTests`, `RackViewFrameTests`; `*DimensionView*` | comportamiento | Core |
| G7 | `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests`; `CustomPropertiesEnvelopeGuardTests` | comportamiento / guarda | Core |
| G8 | `RackViewPlacementGuardTests`; fixtures de Insertar; `SelectiveAuthoredCarrierAdoptionTests`, `PushBackPluginSourceGuardTests` | guarda | Core |
| G9 | `RackSiblingCustomPropertiesGateTests`, `RackEnvelopeIdProbeTests`; `RackSiblingGateWiringGuardTests`, `RackSiblingGateIndependenceGuardTests`; `CustomPropertiesEdgeGuardTests`; `RackUnitsGuardSourceTests` | comportamiento / guarda | Core |
| G10 | `FirstViewFreedomTests`; guardas de modales y shell | UI / guarda | UI |
| G11 | `RackViewBatchPlanTests`, `EditorViewBatchRequestTests`; `RackEditorSessionTests`, `RackInsertionRequestTests` | comportamiento | Core + UI |
| G12 | `RackViewBatchDialogTests`, `RackViewBatchDriverGuardTests`, `RackViewEntryPathGuardTests`; censos y guardas de ventana | UI / guarda | Core + UI |
| G13 | `RackDuplicationPlanTests` + consumo o semantica observable | comportamiento | Core |
| G14 | `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests`, `RackGroupPlacementPlanTests`, `RackSiblingAuthoredGateTests`, `RackSiblingGateIndependenceGuardTests`, `RackViewCountInvariantTests`, `RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests` | comportamiento / guarda | Core |
| G15 | `RackProjectionCommandGuardTests`; censos de comandos y ayuda; `RackUnitsGuardSourceTests` | guarda | Core + UI |
| Todos | T2 suites completas; T3 CI | — | — |
| G4, G5, G8, G9, G10, G12, G15, G16 | T4 §OV | — | AutoCAD 2025 |

## OV. Validacion del Owner en AutoCAD 2025 (diseno; no ejecutar)

**Preparacion comun.** DLL Debug del SHA exacto; AutoCAD 2025; biblioteca de bloques del Owner; dibujo en pulgadas; registrar SHA,
version y biblioteca (AGENTS.md) y, opcional, la duracion activa (I-45).

### OV-LEG (G8)

| # | Pasos | Esperado |
|---|---|---|
| OV-LEG-01..06 | Insercion de vista unica y `RACKEDITAR` → Insertar en Selectivo, Dinamico, Push Back compuesto, Cantilever, cabecera y `QUICKCAMA` | Igual que antes de G8 |
| OV-LEG-07 | Esc en cada jig | No queda definicion con datos de RackCad |

### OV-ID17 (G10)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID17-01 | Selectivo nuevo (2 fondos) → Insertar planta; `RACKEDITAR` → frontal fondo 2 | 1 rack, 2 vistas; mismo diseno desde cualquier vista |
| OV-ID17-02 | Selectivo nuevo → Insertar lateral → `RACKEDITAR` → frontal y planta | 1 rack, 3 vistas |
| OV-ID17-03..05 | Dinamico nuevo con frontal salida, frontal entrada o planta primero → `RACKEDITAR` → lateral | 1 rack por caso |
| OV-ID17-06 | `RACKCABECERA` → planta primero → `RACKEDITAR` → lateral | 1 cabecera, 2 vistas |
| OV-ID17-07 | Push Back y Cantilever: cada vista como primera | Sin regresion |
| OV-ID17-08 | Esc en el jig de una primera vista no frontal | Nada queda |
| OV-ID17-09 | Guardar, cerrar, reabrir → `RACKBOMTOTAL` | Cantidades de 1 rack |
| OV-ID17-10 | Rack legado con `Id` en blanco (si existe) → `RACKEDITAR` → Insertar | Se cura como hoy; la vista nueva comparte el id y sus propiedades |

### OV-ID18 (G12)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID18-01 | Selectivo nuevo → lote frontal fondo 1 + lateral poste 2 + planta | Tres prompts con nombre y posicion; 1 rack, 3 vistas |
| OV-ID18-02 / 03 | Esc en la segunda / en la ultima | Solo frontal / frontal y lateral; mensaje de cola parcial |
| OV-ID18-04 / 05 | `RACKEDITAR` desde la lateral → Actualizar; guardar, reabrir, editar desde la planta | Las tres cambian; mismo diseno |
| OV-ID18-06 | `U` tras un lote | Registrar lo observado (R-07) |
| OV-ID18-07 | Dinamico existente → lote entrada + planta → Esc en la primera | Redibujo conservado; sin vistas nuevas |
| OV-ID18-08 | Push Back compuesto → frontal A, frontal B y lateral tras una frontera suprimida | Tres vistas; poste real |
| OV-ID18-09 | Cantilever → laterales de dos estaciones + planta | Tres vistas; una regeneracion al final |

### OV-META (G9 y G12)

| # | Pasos | Esperado |
|---|---|---|
| OV-META-01 | `RACKPROPIEDADES` en un rack → insertar una vista y un lote | Las vistas nuevas tienen las mismas propiedades |
| OV-META-02 | Cotas desactivadas en frontal → lote F + L + P | Cada vista segun su politica |
| OV-META-03 | Rack con vinculo a variable → lote | El vinculo sigue en todas |
| OV-META-04 | Rack cuyas vistas tienen propiedades distintas (si el Owner dispone de uno) → `RACKEDITAR` → Insertar | Rechazo con remedio; nada cambia; Actualizar sigue funcionando; tras unificar con `RACKPROPIEDADES`, Insertar procede |
| OV-META-05 | Dibujo con un bloque RackCad de **otro** rack que este build no interpreta (p. ej. de una version mayor), si el Owner dispone de uno → `RACKEDITAR` → Insertar en un rack sano | La insercion procede; si no hay bloque disponible, la evidencia es `RackSiblingCustomPropertiesGateTests` |
| OV-META-06 | Rack con propiedades distintas y, en el mismo dibujo, un bloque no interpretable (si el Owner dispone de ambos) → Insertar → `RACKPROPIEDADES` | Insertar rechaza con el remedio condicionado; `RACKPROPIEDADES` queda en solo lectura y dice la causa; tras retirar el bloque ajeno y unificar, Insertar procede |

### OV-ID19 (G15; con el paquete recomendado de M-01; se reescribe si M-01 decide otra cosa)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID19-01 | `RACKLAYOUT` independiente de 1 fila × 4 (columnas a lo largo de la corrida) → proyectar las plantas a Frontal | Modo ortografico: frontales lado a lado sobre una base comun, **en el mismo orden** que las plantas a lo largo de la corrida; distancia entre ejes de poste 0 = la de las plantas; conteos identicos |
| OV-ID19-02 | Dos filas (2 × 4) → Frontal | Aviso de superposicion **antes de pedir puntos**; frontales de filas distintas superpuestas |
| OV-ID19-03 | Las 8 plantas → Lateral | Aviso de superposicion antes de pedir puntos; laterales de las dos filas separadas a lo largo de la profundidad; las de una misma fila superpuestas |
| OV-ID19-04 | Las 4 frontales de OV-ID19-01 → Planta | Plantas en orientacion natural (OD-6.d = A), sobre una linea comun, con el mismo orden y la misma separacion que las plantas originales |
| OV-ID19-05 | Plantas → **Planta** (misma clase) con destino desplazado | Modo rigido: copia enlazada del layout con la orientacion de cada rack; conteos identicos |
| OV-ID19-06 | Layout de plantas **girado 30° con `ROTATE`** (la rejilla de `RACKLAYOUT` solo es correcta a 0/90/180/270, `P/RackLayoutCommands.cs:170-171`) → Planta (rigido) y → Frontal (ortografico) | Rigido: layout y giros conservados; ortografico: frontales rectas con la separacion medida a lo largo de la corrida girada |
| OV-ID19-07 | `RACKLAYOUT` enlazado 1 × 4 → Frontal | 4 referencias de una definicion nueva; 1 rack con 4 copias antes y despues (`RACKLISTA`, `RACKBOMTOTAL`) |
| OV-ID19-08 | `RACKEDITAR` sobre una vista proyectada → Actualizar | Todas las vistas del rack cambian |
| OV-ID19-09 | Frontal ↔ Lateral | Con OD-7.a = A no se expone: mensaje sin escribir |
| OV-ID19-10 | Mezclas invalidas: plantas y una frontal; frontal y planta del mismo rack; Cama; MINSERT; escala ≠ 1 | Error explicito **sin pedir puntos** y sin escribir |
| OV-ID19-11 | Vista con `View` desconocido en la seleccion | Error con remedio `RACKEDITAR`; nada escrito |
| OV-ID19-12 | Rack con propiedades divergentes en la seleccion; y aparte un bloque ajeno no interpretable | Lo primero falla con remedio; lo segundo no bloquea |
| OV-ID19-13 | Esc en punto base o destino | Nada escrito ni importado |
| OV-ID19-14 | Selectivo, Dinamico y Push Back de longitudes distintas en una corrida → Frontal | Frontales canonicas alineadas por el eje del poste 0, en el orden de la corrida |
| OV-ID19-15 | Cantilever y Selectivo con corridas paralelas → Frontal | Con OD-7.d = A: error explicito sin pedir puntos |
| OV-ID19-16 | Rack con variable vinculada → proyectar → cambiar la variable | La vista usa el valor efectivo y se actualiza |
| OV-ID19-17 | Push Back con diagnostico bloqueante | Error que nombra el rack |
| OV-ID19-18 | 100 o mas plantas → Frontal | Termina; registrar duracion y numero de racks |
| OV-ID19-19 | `RACKLAYOUT` enlazado con una celda espejada con `MIRROR` → cualquier clase | Error de espejo sin pedir puntos |
| OV-ID19-20 | Dos plantas del mismo rack (duplicado legal) en la seleccion | Con OD-7.c = A: error que nombra el rack |

### OV-PR

| # | Pasos | Esperado |
|---|---|---|
| OV-PR1-01 | Push Back con frontera suprimida **antes** del corte elegido → lateral | Poste real |
| OV-PR2-01 | Cantilever con brazos y tensores **visibles** en planta → insertar planta; `RACKEDITAR` → Actualizar | Coincide con la vista previa en ambos casos |
| OV-PR2-02 | Cantilever con brazos y tensores ocultos → insertar planta | Igual que antes de G5 |

## R. Riesgos

| # | Riesgo | Mitigacion | Gate |
|---|---|---|---|
| R-01 | Regresion al re-enrutar inserciones | Goldens, guardas, OV-LEG | G8 |
| R-02 | Archivo caliente con I-49 G10 | Serializar | G6, G10, G12 |
| R-03 | I-52 congela autoridades incompatibles con X-1..X-8 | STOP → Proposal nueva | G5, G6, G7, G8, G9, G10, G12, G13, G14, G15 |
| R-04 | Censos en conflicto al integrar | La segunda re-basa | G7, G12, G15 |
| R-05 | D-15 cambia el caso raro de redibujo fallido | Mensaje explicito; guarda | G9, G12 |
| R-06 | ID19 con selecciones grandes | Un barrido, una lectura del registro, una importacion; OV-ID19-18 | G14, G15 |
| R-07 | `U` no agrupa el lote como espera el usuario | OV-ID18-06 | G12 |
| R-08 | D-10 no observable por el Owner | Guarda con teoria de mutacion | G8 |
| R-09 | Bloques de Push Back insertados con H-01 conservan su `Section` | Sin migracion; documentado en el cierre de PR-1 | G4 |
| R-10 | Un builder cambia su marco local | Caracterizacion de marcos | G3, G6 |
| R-11 | Un par resulta inconsistente al caracterizar | Fuera de ID19 hasta Proposal nueva | G3 |
| R-12 | El gate de propiedades rechaza Insertar donde hoy funciona | Motivo y remedio; Actualizar intacto; OV-META-04 | G9 |
| R-13 | M-01 se resuelve con opciones que cambian validacion y OV-ID19 | La foundation no cambia; se reescriben politicas, pruebas de politica y OV-ID19 | G14 |
| R-14 | Un payload no interpretable sin `Id` legible que si pertenece al `RackId` no se detecta | Consecuencia de CR-03; la sonda cubre los que conservan el `Id`; `RACKPROPIEDADES` lo muestra en solo lectura | G9 |
| R-15 | El remedio `RACKPROPIEDADES` exige reparar antes un payload ajeno o una unificacion bloqueada | Mensaje condicionado; OV-META-06 | G9 |

## F. Archivos y simbolos

### F.1 Produccion nueva (propuesta)

| Ruta | Gate |
|---|---|
| `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDecodeDisposition.cs`, `RackViewEnvelopeCodec.cs`, `RackViewSupport.cs`, `RackViewFrame.cs` | G6 |
| `A/Views/Preparation/*` | G7 |
| `P/Views/RackViewPlacement.cs` | G8 |
| `A/Views/RackSiblingCustomPropertiesGate.cs`, `A/Views/RackEnvelopeIdProbe.cs`; lectura de entradas del gate en el Plugin | G9 |
| `A/Views/RackViewBatchPlan.cs` | G11 |
| `U/Views/RackViewBatchDialog.xaml(.cs)`, `U/Views/RackViewBatchDialogPresenter.cs`, `P/Views/RackViewBatchDriver.cs` | G12 |
| `A/Views/Placement/*` (`SourceGroupFrame`, `TargetGroupFrame`, `CommonTransform2D`, `RackGroupAnchorPolicy`, `RackGroupPlacementPlan`), `A/Views/RackSiblingAuthoredGate.cs` | G14 |
| `P/RackProyectarCommands.cs` | G15 |

### F.2 Produccion modificada

| Ruta | Gate |
|---|---|
| `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | G4, G6, G12 |
| `P/RackCantileverCommands.cs` | G5, G8, G9, G12 |
| `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs` | G6 |
| `P/Drawing/BlockPlacement.cs` | G8 |
| `P/RackSelectivoCommands.cs`, `P/RackDinamicoCommands.cs`, `P/RackPushBackCommands.cs`, `P/RackCabeceraCommands.cs` | G8, G9, G10, G12 |
| `P/RackCamaCommands.cs` | G8 |
| `U/Systems/Selective/RackSelectiveWindow.xaml.cs`, `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs` | G6, G10, G12 |
| `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs` | G10, G12 |
| `U/Systems/Cantilever/RackCantileverWindow.xaml.cs` | G12 |
| `U/Editor/RackInsertionRequest.cs`, `U/Editor/RackEditorSession.cs`, `U/Editor/EditorModules.cs`, `P/RackMenuCommands.cs` | G10, G11, G12 |
| `A/Persistence/RackDuplicationPlan.cs` (solo si I-55 extrae X-1) | G13 |
| `U/RackCommandReference.cs` | G15 |

### F.3 NO TOCAR sin nueva revision de Arquitecto

`P/RackDuplicarCommands.cs`, `RackEnvelopeRestamp`, `P/RackCloner.cs`; `P/RackLayoutCommands.cs` y `P/RackLayoutCommands.Fill.cs`; la API de
`A/Persistence/RackEmbedComposer.cs` y el store de `A/Persistence/RackEmbedDocument.cs` (solo consumo); `P/CustomPropertiesData.cs`,
`P/CustomPropertiesExecutor.cs`, `P/RackPropiedadesCommands.cs`, `A/CustomProperties/*` (solo consumo de `CustomPropertiesCanonicalForm`) y
sus guardas; `A/Persistence/CustomPropertiesStore.cs` (solo consumo); el contenido de `P/Drawing/LateralHeaderDrawer.cs` y
`P/Drawing/Cantilever/CantileverViewMaterializer.cs`; `P/Drawing/BlockLibraryImporter.cs` (solo consumo); la geometria de los builders;
`A/Systems/Cantilever/CantileverLineEditorAssembler.cs` (linea base C2-4 de I-52); `D/Systems/Cantilever/CantileverLineDesign.cs` y el
`Id` interior del Cantilever; `DynamicEditorDesignAssembler` y el lote de cabeceras de I-53D; `P/ProjectVariableMutationExecutor.cs`;
`docs/HANDOFF.md` hasta la integracion.

## P. Rendimiento

| Operacion | Coste maximo |
|---|---|
| Catalogo, registro de variables | 1 por comando |
| Barrido de sobres | ID19: 1 (SNAPSHOT, del que salen tambien los gates); edicion: los vigentes + 1 lectura de las entradas del gate por Insertar |
| Resolucion y serializacion authored | 1 por `RackId` |
| `CommonTransform2D` | 1 por ejecucion (seleccion proyectada) |
| Importacion de bloques | ID19: 1 con la union de nombres |
| Transacciones | ID18: 2 por vista, ninguna abierta entre jigs; ID19: 1 |

## S. Estado

```text
Implementation Map V2 = NOT CONSENSUS · Open Material = M-01 · X-1..X-8
Coordinator = REVIEW REQUIRED · Architect = REVIEW REQUIRED · Consensus = NOT REACHED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
