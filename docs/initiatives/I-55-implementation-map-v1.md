# I-55 — Implementation Map V1: View Placement & Projection

```text
PROPOSAL V1 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED

Acompana a     = docs/initiatives/I-55-proposal-v1.md (misma version del plan; ninguno vale sin el otro)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/
```

> Este mapa **no autoriza** ningun gate. Describe, para cuando exista consenso de Coordinador y Arquitecto sobre la misma
> version del plan, que se toca, en que orden, con que prueba vista fallando, con que evidencia y como se revierte. Tipos y
> pruebas nuevos son **propuestos**; rutas y simbolos existentes estan verificados sobre `CURRENT_BASE`. Los conteos de pruebas
> se registran en la evidencia de cada gate, no aqui (AGENTS.md).

## 0. Convenciones de gate y de evidencia

| Campo | Contenido |
|---|---|
| Objetivo | Que decision de la Proposal materializa |
| Precondiciones | Gates previos, acuerdos entre iniciativas, decisiones del Owner |
| Produccion | Archivos nuevos y modificados, con simbolos |
| Pruebas | Clases nuevas y guardas existentes que se reapuntan **con motivo** |
| RED | Prueba vista **fallar** sobre el SHA previo al cambio (I-45). Tipos nuevos: esqueleto sin comportamiento + pruebas. Caracterizaciones: verdes sobre codigo intacto y rojas **solo** por teoria de mutacion |
| GREEN | Focales con conteo > 0 (AGENTS.md), suites Core + UI completas, builds Debug que correspondan, CI de `push` 4/4 sobre el SHA exacto |
| Impacto | Comportamiento observable que cambia; archivos calientes |
| Rollback | `git revert` de los commits del gate: **ningun gate migra datos** |
| Coordinacion | Cruces con I-49, I-52 e I-54 a re-medir antes de fijar archivos |

**Clases de evidencia.** T0 focal y T1 sistema/impacto: corridas filtradas por nombre de clase (el proyecto usa el namespace
plano `RackCad.Tests`/`RackCad.UI.Tests`) con conteo; T2: suites completas sobre el SHA del Candidato; T3: CI (`push` 4/4, merge
y coberturas de WORKFLOW §4.5); T4: Owner en AutoCAD 2025. No se sustituyen entre si; un rebase invalida lo previo.

**Apertura de cada gate.** `git fetch --all --prune`; si `main` avanzo, rebase al abrir la sesion (WORKFLOW §4.2) y re-medicion;
ROADMAP y HANDOFF solo cuando WORKFLOW lo permita.

**Pruebas de UI.** Ninguna prueba recorre una ruta que abra un modal real (`MessageBox`, `ShowDialog`): colgaria el hilo STA
compartido (`TU/DynamicHeaderBatchSeamGuardTests.cs:13-15`). Se observan habilitacion, tooltips y costuras.

## 1. Secuencia y dependencias

```text
G3 ─┬─ G4 (PR-1, H-01) ─┐
    ├─ G5 (PR-2, H-02) ─┼─ G7 (foundation) ─ G8 (plan + preparacion) ─ G9 (colocacion) ─ G10 (ID17) ─ G11 ─ G12 (ID18)
    └─ G6 (PR-3, H-13) ─┘                                                                                    │
                           G13 (X-1, nucleo de seleccion) ────────────────────────────────────────── G14 ─ G15 (ID19) ─ G16
```

| Punto de coordinacion | Gate de I-55 | Contraparte | Regla |
|---|---|---|---|
| X-1 nucleo de seleccion | G3 (condicional), G13 | I-52 V10 §5.1 | Una extraccion; hechos neutrales, politicas por llamador |
| X-2 plan por vista + codec | G3, G7, G8 | I-52 V10 §3.7, §8.1, §8.4, CT-04 | Resolucion separada; un paso comun «sistema → plan»; un codec neutral |
| X-3 comparador por kind | G14 | I-52 V10 §5.4 | Declarado por el contrato del kind; miembros como parametro |
| X-4 primitivo de materializacion | G15 | I-52 V10 §8.2 | Politicas de equivalencia visual fuera del primitivo |
| X-5 archivos STOP / NO TOCAR de I-52 | G7, G10, G12, G13 | I-52 V10 §19, §20.3, §15.3 | Reportar antes de editar y secuenciar |
| X-6 lineas base de C-2 | G9, G12 | I-52 V10 §8.5 (C2-2a, C2-2b, C2-4, G-M9) | La segunda en integrar re-establece C-2 |
| Archivo caliente del Selectivo | G7, G10, G12 | I-49 G10 | Serializar |
| Censos | G8, G12, G15 | I-52 (comandos, ayuda, `Compose`) | La segunda en integrar re-basa |

## 2. Gates

### G3 — Caracterizacion (solo pruebas)

| Campo | Contenido |
|---|---|
| Objetivo | Fijar sobre produccion intacta lo que I-55 preserva, cambia o necesita medir |
| Precondiciones | Coordinator = AGREED y Architect = AGREED sobre la misma version; Consensus Freeze; ADR-0042 aceptado; OD-1..OD-8 decididas; secuencia X-1..X-6 acordada |
| Produccion | Ninguna |
| T/ `RackViewEnvelopeReadingCharacterizationTests` | Comportamiento de `PushBackSystemFrontalBuilder.EncodeSection/DecodeSection/IsValidSection` y `CantileverViewPlanBuilder.SectionFor`; comportamiento de `RackListBuilder` con `View` vacio (`A/Persistence/RackListBuilder.cs:117-118`); guarda de fuentes de las lecturas del Plugin: `P/RackSelectivoCommands.cs:126`, `:168`; `P/RackDinamicoCommands.cs:209-239`, `:392-393`; `P/RackPushBackCommands.cs:427-450`; `P/RackCantileverCommands.cs:530-540`; `P/RackCabeceraCommands.cs:239-240`; `P/ProjectVariableMutationExecutor.cs:181-186`; `P/RackBlockFinder.cs:23-24`; `P/RackLayoutCommands.cs:71` — **una sola** caracterizacion con la CT-04 de I-52 |
| T/ `LegacyViewPayloadCompositionCharacterizationTests` | Formula de composicion por sistema con APIs de Application y guarda que fija `BuildSelectivePayload`/`WrapSelectivePayload` (`P/RackSelectivoCommands.cs:344`, `:382`), `BuildDynamicPayload` (`P/RackDinamicoCommands.cs:361`), `BuildPushBackPayload` (`P/RackPushBackCommands.cs:397`), `BuildCantileverPayload` (`P/RackCantileverCommands.cs:472`), `BuildCabeceraPayload` (`P/RackCabeceraCommands.cs:181`), `BuildCamaPayload` (`P/RackCamaCommands.cs:175`, `:183`) |
| T/ `LegacyViewBlockNameCharacterizationTests` | Nombres base de los servicios de dibujo (`P/Systems/Selective/`, `P/Systems/Dynamic/`, `P/Systems/PushBack/`, `P/Systems/FlowBed/`, `P/Drawing/LateralHeaderDrawService.cs`, `P/Drawing/PlantaHeaderDrawService.cs`) |
| T/ `RackCountInvariantCharacterizationTests` | Vistas frente a racks en `RackListBuilder`; representante de `BomAuthoredAuthority` (`A/Bom/BomAuthoredAuthority.cs:104-109`) |
| T/ `RackViewFrameCharacterizationTests` | **Descriptores de marco de §12.4**, medidos sobre la salida de los builders con disenos de fixture por sistema: eje del poste 0 en frontal y planta; cara delantera en lateral y planta; `anchorOffset` de un corte de esquina del Selectivo (`A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`); cuadricula de la frontal de cada fondo frente a la maestra (`A/Systems/Selective/SelectiveFrontalBuilder.cs:50`, `A/Systems/Selective/SelectiveDepthLayout.cs:52-55`); Push Back compuesto (lado A y B); Dinamico; cabecera; ejes y signos del Cantilever (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`) |
| T/ + TU/ `HeadlessResolutionParityCharacterizationTests` | Por kind, la resolucion sin editor (camino del BOM) frente al sistema que entrega el editor; en el Dinamico, a traves de la ventana (reconstruccion de I-53D). Compartida con la C2-4 de I-52 si ambas avanzan |
| TU/ `FirstViewGateCharacterizationTests` | Habilitacion y tooltips vigentes de las puertas de primera vista en Selectivo, Dinamico y cabecera, **sin** pulsar rutas que abren `MessageBox` |
| Condicional X-1 | T/ `RackDuplicationPlanObservableSemanticsTests` (textos, orden de grupos y referencias, avisos y errores; como la CT-16 de I-52) |
| RED | Caracterizaciones verdes sobre codigo intacto; cada guarda demuestra deteccion por teoria de mutacion |
| GREEN | Clases nuevas con conteo; Core + UI completas; CI 4/4 |
| Impacto | Ninguno |
| Rollback | Revert |
| Si falla una caracterizacion de marco | El par afectado queda **fuera** de ID19 (fallo cerrado) hasta una Proposal V2; nunca se ajusta el descriptor para que pase |

### G4 — PR-1: poste real en la lateral de Push Back (H-01)

| Campo | Contenido |
|---|---|
| Objetivo | La seccion lateral de Push Back es el `PostIndex` del corte, no la posicion en la lista |
| Precondiciones | G3 |
| Produccion | `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`: lista de cortes (`:3036-3045`) y `SelectedView` (`:3054-3067`, hoy `:3065`), con enteros de hoy |
| Pruebas | TU/ `PushBackLateralPostIndexRegressionTests.SelectedLateral_UsesTheCutPostIndex_WhenABoundaryIsSuppressedBeforeIt` (la frontera suprimida queda **antes** del corte elegido, para que posicion e indice difieran) |
| RED | Vista fallando |
| GREEN | Focal + `FullyQualifiedName~PushBack` UI con conteo; Core + UI; Debug UI + Plugin; **OV-PR1** |
| Impacto | Cambia el corte insertado en disenos con fronteras suprimidas; los bloques ya insertados conservan su `Section` (R-09) |
| Rollback | Revert |

### G5 — PR-2: visibilidad de la planta Cantilever (H-02)

| Campo | Contenido |
|---|---|
| Objetivo | La planta del Plugin respeta `PlantaVisibility` como la vista previa |
| Precondiciones | G3 |
| Produccion | Regla pura de entradas del plan en Application (p. ej. `A/Systems/Cantilever/CantileverViewPlanRequest.cs`), consumida por `P/RackCantileverCommands.cs:131`, `:312` y por la vista previa (`A/Systems/Cantilever/CantileverLineEditorAssembler.cs:148`) |
| Pruebas | T/ `CantileverPlantaVisibilityParityTests.PluginPlanInputs_CarryThePlantaVisibility_WhenArmsAndBracesAreShown` |
| RED | Se extrae la regla reproduciendo la omision; la paridad con brazos y tensores **visibles** se ve fallar |
| GREEN | Focal + `FullyQualifiedName~Cantilever` Core y UI con conteo; Debug Plugin; **OV-PR2** |
| Impacto | Cambian las plantas cuyo diseno **muestra** brazos o tensores (hoy el Plugin siempre los oculta: `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:293-295`) |
| Rollback | Revert |

### G6 — PR-3: `Id` interior de un Cantilever nuevo (H-13)

| Campo | Contenido |
|---|---|
| Objetivo | Toda linea nueva nace con `Id` interior = `RackId` |
| Precondiciones | G3 |
| Produccion | Construccion de la peticion de insercion de un rack **nuevo** en `U/Systems/Cantilever/RackCantileverWindow.xaml.cs` (sobre una copia del diseno; el diseno del editor no se muta) |
| Pruebas | TU/ `CantileverNewRackIdentityTests.InsertionRequestOfANewLine_CarriesTheRackIdAsInnerId`; TU/ caso de linea abierta desde el dibujo: `Id` interior intacto |
| RED | Vista fallando |
| GREEN | Focal + `FullyQualifiedName~Cantilever` UI con conteo; Core + UI |
| Impacto | Solo el `Id` interior de lineas nuevas; sin cambio de dibujo |
| Rollback | Revert |

### G7 — Foundation pura

| Campo | Contenido |
|---|---|
| Objetivo | D-02, D-03, D-04 y los tipos de D-16, sin cambio de comportamiento |
| Precondiciones | G4, G5, G6; X-5 reportado a I-52 por el renombre |
| Produccion nueva | `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDecodeDisposition.cs`, `RackViewEnvelopeCodec.cs`, `RackViewSupport.cs`, `RackViewFrame.cs` |
| Produccion modificada (renombre) | `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs`, `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs`, `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`, `U/Systems/Selective/RackSelectiveWindow.xaml.cs` |
| Pruebas nuevas | T/ `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewEnvelopeCodecTests` (bytes legacy de G3; disposiciones de G3), `RackViewSupportMatrixTests` (una por celda de §5), `RackViewFrameTests` (los descriptores reproducen `RackViewFrameCharacterizationTests`) |
| Pruebas modificadas | Renombre en las seis clases de pruebas que nombran el tipo |
| RED | Esqueleto + pruebas vistas fallando |
| GREEN | T0 `FullyQualifiedName~RackView`; T1 `FullyQualifiedName~DimensionView`; ambos con conteo; Core + UI; Debug UI |
| Impacto | Ninguno observable; conflicto textual trivial con I-49 G10 |
| Rollback | Revert |

### G8 — Plan por vista desde sistema resuelto y preparacion (X-2)

| Campo | Contenido |
|---|---|
| Objetivo | D-01 en Application |
| Precondiciones | G7; acuerdo X-2 (si I-52 ya integro su autoridad, se consume re-verificando su API y cubriendo laterales) |
| Produccion nueva | `A/Views/Preparation/IRackViewPreparer.cs`, `RackViewPreparationContext.cs`, `PreparedRackView.cs`, `ViewPreparationResult.cs`, `RackViewPreparers.cs`, seis preparadores, `RackViewEnvelopeComposition.cs` (la unica llamada nueva a `Compose`, con el origen como **parametro `RackEmbedDocument`**), `RackViewBlockNaming.cs`; paso comun «sistema resuelto → plan por vista» en la ubicacion acordada |
| Pruebas nuevas | T/ `RackViewPreparation{Selective,Dynamic,PushBack,Cantilever,Cabecera,Cama}Tests`; `PreparedViewPayloadGoldenTests` (byte a byte = formula de G3, que tras PR-3 ya alinea el `Id` interior); `RackIdLifecycleTests` |
| Guardas reapuntadas con motivo | `CustomPropertiesEnvelopeGuardTests.TGrd02_CensoDeCompose_SonLasSieteLlamadasDeD21_PorArchivo` (+1 archivo y llamada) y `TGrd02_CadaOrigenEstaClasificado_YNingunaLlamadaPasaNullLiteral` (parametro `RackEmbedDocument`), sin debilitarlas |
| RED | Esqueleto + goldens vistos fallando; `TGrd02` falla hasta reapuntarse |
| GREEN | T0 `FullyQualifiedName~RackViewPreparation\|FullyQualifiedName~PreparedViewPayload\|FullyQualifiedName~RackIdLifecycle` con conteo; T1 `FullyQualifiedName~CustomPropertiesEnvelopeGuard`; Core + UI |
| Impacto | Codigo nuevo sin consumidores |
| Rollback | Revert |

### G9 — Colocacion sobre la preparacion (vista unica)

| Campo | Contenido |
|---|---|
| Objetivo | Toda insercion de vista unica pasa por preparacion + colocacion sin cambio observable; D-10; prompt |
| Precondiciones | G8; coordinacion G-M9 de I-52 (X-6) |
| Produccion nueva | `P/Views/RackViewPlacement.cs` |
| Produccion modificada | `P/Drawing/BlockPlacement.cs`: `PlaceAndReport` (limpieza en la rama de excepcion `:49-52` y parametro de prompt con el texto por defecto intacto) y `PlaceDefinition` (limpieza ante excepcion); `P/RackSelectivoCommands.cs` (`DrawSelectiveViewFromAuthored` `:426-453`); `P/RackDinamicoCommands.cs` (`DrawDynamicView`); `P/RackPushBackCommands.cs` (`DrawPushBackView`); `P/RackCantileverCommands.cs` (`DrawCantileverView`, incluida la definicion confirmada en `:151-157` cuando falla despues); `P/RackCabeceraCommands.cs` (`DrawAndPlace` `:164-176`, insercion de `EditCabecera` `:294-310`); `P/RackCamaCommands.cs` (`QUICKCAMA` `:102-106`) |
| Pruebas nuevas | T/ `RackViewPlacementGuardTests` (insercion por preparacion + colocacion; ninguna insercion compone el sobre; limpieza ante excepcion en **ambas** primitivas), con teorias de mutacion; fixtures de equivalencia de Insertar (X-6) |
| Guardas reapuntadas con motivo | `SelectiveAuthoredCarrierAdoptionTests.GUARDA_LaInsercionNUEVA_CONSERVA_From` (el `From(design, id, name)` pasa al preparador); `PushBackPluginSourceGuardTests` (servicios y firmas de insercion) |
| RED | Guardas vistas fallando |
| GREEN | Guardas y goldens con conteo; Core + UI; Debug Plugin; **OV-LEG** |
| Impacto | **Alto riesgo de regresion**; D-10 no es observable por el Owner (evidencia = guarda) |
| Rollback | Revert (G8 queda sin consumidores) |

### G10 — ID17: primera vista libre

| Campo | Contenido |
|---|---|
| Objetivo | Puertas de §10 |
| Precondiciones | G9; serializado con I-49 G10 |
| Produccion | `U/Systems/Selective/RackSelectiveWindow.xaml.cs` (`UpdateInsertButtons` `:274-292`; `RequestDraw` `:2550-2570`); `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs` (`:189-194`; `:3394-3403`); `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs` (`:85-93`, `:265-269`); `U/Editor/RackInsertionRequest.cs` (`HeaderInsertionRequest` gana direccion, `:36-53`); `P/RackCabeceraCommands.cs` (`RackCabecera` y `QuickCabecera` respetan la direccion); `P/RackMenuCommands.cs` |
| Pruebas nuevas | TU/ `FirstViewFreedomTests` (cada celda `CanStartWith = SI` de §5 habilitada y con la direccion en la peticion) |
| Guardas reapuntadas con motivo | Inversion de `FirstViewGateCharacterizationTests`; `DynamicHeaderBatchSeamGuardTests.D34_GUARD_ElCensoDeModalesDirectosNoCrece_YLosGestosNuevosNoAbrenNinguno` (un `MessageBox` → cero); `SelectiveHeaderBatchSeamTests.S30_GUARD_TheBatchGestureOpensNoModalOfItsOwn_AndConfirmsThroughTheSeam` (cuatro → tres); `DynamicShellMigrationTests`; `RichEditorBlockedActionTests`; `RackInsertionRequestTests` |
| RED | `FirstViewFreedomTests` vistas fallando |
| GREEN | TU/ `FullyQualifiedName~FirstView` con conteo; Core + UI; Debug UI + Plugin; **OV-ID17** |
| Impacto | Cambio intencional de UX de creacion |
| Rollback | Revert |

### G11 — ID18: contrato puro

| Campo | Contenido |
|---|---|
| Objetivo | D-06 y D-15 sin UI |
| Precondiciones | G10 |
| Produccion | `A/Views/RackViewBatchPlan.cs` (orden, estados incluido `REDRAW_FAILED`, mensajes); `U/Editor/RackEditorSession.cs` (`RequestInsertViews`); `U/Editor/RackInsertionRequest.cs` (`Views`; historicos = primera direccion) |
| Pruebas nuevas | T/ `RackViewBatchPlanTests` (maquina completa, mensajes exactos, Esc detiene, `REDRAW_FAILED` sin placements); TU/ `EditorViewBatchRequestTests` (una aceptacion → un `RackId`) |
| Pruebas reapuntadas | `RackEditorSessionTests`, `RackInsertionRequestTests` |
| RED / GREEN | Vistas fallando / T0 `FullyQualifiedName~RackViewBatch\|FullyQualifiedName~EditorViewBatch` con conteo; Core + UI |
| Rollback | Revert |

### G12 — ID18: dialogo, presentador, driver y rutas de entrada

| Campo | Contenido |
|---|---|
| Objetivo | ID18 de extremo a extremo |
| Precondiciones | G11; serializado con I-49 G10 |
| Produccion nueva | `U/Views/RackViewBatchDialog.xaml` + `.xaml.cs` (arquetipo C); `U/Views/RackViewBatchDialogPresenter.cs` (fuera de los archivos de ventana); `P/Views/RackViewBatchDriver.cs` (PREPARE ALL; en edicion preparar → redibujar → `REDRAW_FAILED` o cola; `Regen` segun §11.4) |
| Produccion modificada | Ventanas Selectivo, Dinamico, Push Back, Cantilever y configurador de cabecera (boton y presentador); **rutas de entrada**: `U/Editor/EditorModules.cs` (`Build` del Selectivo `:53-56` y del Dinamico `:90-95` reenvian `Views`); `P/RackSelectivoCommands.cs` (`RACKSELECTIVO` `:25-45`); `P/RackCabeceraCommands.cs` (un acunado por intencion); comandos de creacion de Dinamico, Push Back y Cantilever; `P/RackMenuCommands.cs`; ramas `Edit*` |
| Pruebas nuevas | TU/ `RackViewBatchDialogTests`; T/ `RackViewBatchDriverGuardTests` (PREPARE ALL antes de escribir; `REDRAW_FAILED`; Esc detiene); T/ `RackViewEntryPathGuardTests` (toda ruta de entrada reenvia `Views` y acuna una vez) |
| Guardas reapuntadas con motivo | `WindowCensusGuardTests` y `DialogWindowCharacterizationTests` (ventana nueva del arquetipo C); `EditorModuleRegistryTests`; `RackUnitsGuardSourceTests` (guarda de unidades antes del primer redibujo, conteos por archivo); `CantileverPluginSourceGuardTests.ExactlyOneRegenForTheWholeBatch_AndItAgreesWithWhatIsReported` (sigue en uno dentro de la edicion). Las guardas de modales directos de las ventanas **no cambian** |
| GREEN | Focales con conteo; Core + UI; Debug UI + Plugin; **OV-ID18** |
| Impacto | Boton nuevo en cinco editores; D-15 en la edicion |
| Rollback | Revert |

### G13 — Nucleo neutral de seleccion (X-1)

| Campo | Contenido |
|---|---|
| Objetivo | Nucleo neutral disponible sin cambiar `RACKDUPLICAR` |
| Precondiciones | Acuerdo X-1; X-5 reportado; coordinacion con I-49 antes de editar `A/Persistence/RackDuplicationPlan.cs` |
| Si I-52 ya lo extrajo | Consumirlo; T/ `RackSelectionCoreConsumptionTests` |
| Si no | Extraer con `RackDuplicationPlanObservableSemanticsTests` (G3) y la fachada verde; pruebas de I-51 sin cambios |
| GREEN | `FullyQualifiedName~RackDuplicationPlan\|FullyQualifiedName~RackSelectionCore` con conteo; Core + UI |
| Rollback | Revert |

### G14 — ID19: plan puro

| Campo | Contenido |
|---|---|
| Objetivo | D-07 (ID19), D-08, D-13, D-16 y D-17 sin AutoCAD |
| Precondiciones | G12, G13; OD-6 y OD-7 decididas; acuerdo X-3 |
| Produccion nueva | `A/Views/RackProjectionPlan.cs` (grupos, clasificacion, autoridad authored por grupo sobre todas las hermanas, pares, familias, orientacion, variante canonica, salidas bloqueadas, mensajes, aviso de superposicion); `A/Views/RackProjectionTransform.cs` (`O_r`, `l_r`, `T_common`, `P_r`); comparador por kind de X-3 |
| Pruebas nuevas | T/ `RackProjectionPlanTests`: layout independiente; layout **enlazado** (una definicion nueva, N referencias); hermanas distintas de un `RackId` → fallo; tipos fuente mezclados, par no soportado, familias mezcladas, orientaciones distintas, MINSERT, espejo, escala no uniforme, normal no Z, `RackId` en blanco, authored divergente, salida bloqueada → fallo; Cama → fallo; aviso de filas. T/ `RackProjectionTransformTests`: invariante de layout por par soportado y por familia, `Origin ≠ 0`, rotaciones comunes, UCS. T/ `RackViewCountInvariantTests`, `RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests` |
| RED / GREEN | Vistas fallando / T0 `FullyQualifiedName~RackProjection\|FullyQualifiedName~RackViewCountInvariant\|FullyQualifiedName~RackViewAuthoredEquivalence\|FullyQualifiedName~RackViewPartialRack` con conteo; Core + UI |
| Rollback | Revert |

### G15 — ID19: comando y primitivo de materializacion

| Campo | Contenido |
|---|---|
| Objetivo | ID19 de extremo a extremo |
| Precondiciones | G14; OD-1 y OD-8 decididas; acuerdo X-4 |
| Produccion nueva | `P/RackProyectarCommands.cs` (`RACKPROYECTAR` + `RPY` si OD-1 = A), SNAPSHOT propio; primitivo «definicion + sobre + referencia en la transaccion del llamador» (X-4) o su consumo |
| Produccion modificada | `U/RackCommandReference.cs` (ayuda) |
| Pruebas nuevas | T/ `RackProjectionCommandGuardTests` (validar antes de pedir puntos; importar despues; una transaccion; sin `Regen` ni purga; kind con `TryResolve`) |
| Guardas reapuntadas con motivo | `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`; `CustomPropertiesCommandGuardTests.TGrd08_*` (censo por **nombre**); `CustomPropertiesHelpCensusTests`; `RackUnitsGuardSourceTests` (el comando nuevo llama la guarda de unidades una vez) |
| GREEN | Focales con conteo; Core + UI; Debug Plugin; **OV-ID19** |
| Rollback | Revert |

### G16 — Candidato, validacion del Owner e integracion

Documentacion de usuario (README, `RACKAYUDA`, guia de validacion) y cierre del contrato; Candidato con T2 sobre el SHA exacto;
CI 4/4; validacion completa del Owner (§OV) sobre ese SHA; integracion `--no-ff`; CI del merge y coberturas del `MERGE_SHA` y del
Candidato **antes** de limpiar rama y worktree; ROADMAP y HANDOFF solo en ese cierre.

## T. Matriz de pruebas

| Gate | Clase | Pruebas | Tipo | Suite |
|---|---|---|---|---|
| G3 | T1 | `RackViewEnvelopeReadingCharacterizationTests`, `LegacyViewPayloadCompositionCharacterizationTests`, `LegacyViewBlockNameCharacterizationTests`, `RackCountInvariantCharacterizationTests`, `RackViewFrameCharacterizationTests`, `HeadlessResolutionParityCharacterizationTests` | comportamiento + guarda | Core (+ UI en paridad) |
| G3 | T1 | `FirstViewGateCharacterizationTests` | UI sin rutas modales | UI |
| G3 (cond.) | T1 | `RackDuplicationPlanObservableSemanticsTests` | comportamiento | Core |
| G4 | T0 | `PushBackLateralPostIndexRegressionTests` | UI | UI |
| G5 | T0 | `CantileverPlantaVisibilityParityTests` | comportamiento | Core |
| G6 | T0 | `CantileverNewRackIdentityTests` | UI | UI |
| G7 | T0 / T1 | `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewEnvelopeCodecTests`, `RackViewSupportMatrixTests`, `RackViewFrameTests` / `*DimensionView*` | comportamiento | Core |
| G8 | T0 / T1 | `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests` / `CustomPropertiesEnvelopeGuardTests` | comportamiento / guarda | Core |
| G9 | T1 | `RackViewPlacementGuardTests`, fixtures de Insertar; `SelectiveAuthoredCarrierAdoptionTests`, `PushBackPluginSourceGuardTests` | guarda | Core |
| G10 | T0 / T1 | `FirstViewFreedomTests` / `DynamicHeaderBatchSeamGuardTests`, `SelectiveHeaderBatchSeamTests`, `DynamicShellMigrationTests`, `RichEditorBlockedActionTests`, `RackInsertionRequestTests` | UI / guarda | UI |
| G11 | T0 / T1 | `RackViewBatchPlanTests`, `EditorViewBatchRequestTests` / `RackEditorSessionTests`, `RackInsertionRequestTests` | comportamiento | Core + UI |
| G12 | T0 / T1 | `RackViewBatchDialogTests`, `RackViewBatchDriverGuardTests`, `RackViewEntryPathGuardTests` / `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `EditorModuleRegistryTests`, `RackUnitsGuardSourceTests`, `CantileverPluginSourceGuardTests` | UI / guarda | Core + UI |
| G13 | T1 | `RackDuplicationPlanTests` + consumo o semantica observable | comportamiento | Core |
| G14 | T0 | `RackProjectionPlanTests`, `RackProjectionTransformTests`, `RackViewCountInvariantTests`, `RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests` | comportamiento | Core |
| G15 | T1 | `RackProjectionCommandGuardTests`, `SelectiveEditorOpenTests`, `CustomPropertiesCommandGuardTests`, `CustomPropertiesHelpCensusTests`, `RackUnitsGuardSourceTests` | guarda | Core + UI |
| Todos | T2 / T3 | suites completas / CI | — | — |
| G4, G5, G9, G10, G12, G15, G16 | T4 | §OV | — | AutoCAD 2025 |

## OV. Validacion del Owner en AutoCAD 2025 (diseno; no ejecutar)

**Preparacion comun.** DLL Debug del SHA exacto; AutoCAD 2025; biblioteca de bloques del Owner; dibujo en pulgadas. Registrar
SHA, version de AutoCAD y biblioteca (condiciones de reutilizacion de AGENTS.md) y, si el Owner acepta, la duracion activa
(metrica experimental de I-45, opcional).

### OV-LEG — Regresion de insercion de vista unica (G9)

| # | Pasos | Esperado |
|---|---|---|
| OV-LEG-01 | Selectivo de 2 fondos: insertar frontal (fondo 2); `RACKEDITAR` → lateral (poste) y planta | Mismo dibujo y nombres que antes de G9; `RACKLISTA` 1 rack |
| OV-LEG-02 | Dinamico: lateral; `RACKEDITAR` → frontal salida, frontal entrada, planta | Igual que antes |
| OV-LEG-03 | Push Back compuesto: cada vista | Igual que antes |
| OV-LEG-04 | Cantilever: frontal, lateral estacion 2, planta | Igual que antes |
| OV-LEG-05 | `RACKCABECERA` lateral; `RACKEDITAR` → planta | Igual que antes |
| OV-LEG-06 | `QUICKCAMA`; `RACKEDITAR` → Actualizar | Igual que antes |
| OV-LEG-07 | Esc en cada jig anterior | No queda definicion con datos de RackCad |

### OV-ID17 — Primera vista libre (G10)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID17-01 | Selectivo nuevo (2 fondos) → **Insertar planta**; `RACKEDITAR` sobre la planta → frontal fondo 2 | Planta sin aviso; despues `RACKLISTA` 1 rack, 2 vistas; `RACKEDITAR` desde la frontal abre el mismo diseno |
| OV-ID17-02 | Selectivo nuevo → **Insertar lateral** → `RACKEDITAR` → frontal y planta | 1 rack, 3 vistas |
| OV-ID17-03 | Dinamico nuevo → frontal salida primero → `RACKEDITAR` → lateral | 1 rack, 2 vistas |
| OV-ID17-04 | Dinamico nuevo → frontal entrada primero | Frontal de entrada correcta |
| OV-ID17-05 | Dinamico nuevo → planta primero → `RACKEDITAR` → lateral | 1 rack, 2 vistas |
| OV-ID17-06 | `RACKCABECERA` → planta primero → `RACKEDITAR` → lateral | 1 cabecera, 2 vistas |
| OV-ID17-07 | Push Back y Cantilever: cada vista como primera | Sin regresion |
| OV-ID17-08 | Esc en el jig de una primera vista no frontal | Nada queda |
| OV-ID17-09 | Guardar, cerrar y reabrir el DWG de OV-ID17-01 → `RACKBOMTOTAL` | Cantidades de 1 rack |
| OV-ID17-10 | Rack legado con `Id` en blanco (si el Owner dispone de uno) → `RACKEDITAR` → Insertar | Se cura como hoy y la vista nueva comparte el id curado |

### OV-ID18 — Varias vistas en un flujo (G12)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID18-01 | Selectivo nuevo → «Insertar varias vistas…» → frontal fondo 1 + lateral poste 2 + planta | Tres prompts con nombre y posicion; `RACKLISTA` 1 rack, 3 vistas; BOM de 1 rack |
| OV-ID18-02 | Repetir y Esc en la **segunda** | Solo la frontal; mensaje de cola parcial; no quedan definiciones de las otras |
| OV-ID18-03 | Repetir y Esc en la **ultima** | Frontal y lateral |
| OV-ID18-04 | `RACKEDITAR` desde la lateral → cambio → Actualizar | Las tres cambian |
| OV-ID18-05 | Guardar, cerrar, reabrir → `RACKEDITAR` desde la planta | Mismo diseno |
| OV-ID18-06 | Tras un lote, `U` | Registrar lo observado (esperado: deshace el comando) |
| OV-ID18-07 | Dinamico existente → `RACKEDITAR` → lote entrada + planta → Esc en la primera | Redibujo conservado; sin vistas nuevas; mensaje |
| OV-ID18-08 | Push Back compuesto → lote frontal A, frontal B y lateral de un poste tras una frontera suprimida | Tres vistas correctas; lateral del poste real |
| OV-ID18-09 | Cantilever → lote lateral estacion 1, lateral estacion 3 y planta | Tres vistas; 1 rack; una sola regeneracion al final |

### OV-ID19 — Proyeccion multi-rack (G15)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID19-01 | Planta de Selectivo → `RACKLAYOUT` **independientes**, **una fila** de 4 columnas → `RACKPROYECTAR` → las 4 plantas → Frontal → base y destino | 4 frontales **lado a lado** sobre una misma base; la distancia entre ejes de poste 0 de dos frontales = la de sus plantas a lo largo de la corrida; `RACKLISTA` y `RACKBOMTOTAL` identicos antes y despues |
| OV-ID19-02 | Igual con **dos filas** (2 × 4) | Aviso de filas superpuestas; las frontales de ambas filas coinciden en posicion |
| OV-ID19-03 | Las 8 plantas → **Lateral** | Laterales de las dos filas separadas a lo largo de la profundidad; aviso de superposicion dentro de cada fila |
| OV-ID19-04 | Las 4 frontales de OV-ID19-01 → **Planta** | 4 plantas con la corrida en su orientacion natural y la misma separacion a lo largo de la corrida |
| OV-ID19-05 | Layout **enlazado** 1 × 4 → Frontal | 4 referencias de una definicion frontal nueva; `RACKLISTA` 1 rack con 4 copias antes y despues |
| OV-ID19-06 | `RACKEDITAR` sobre una frontal proyectada → cambio → Actualizar | Planta y frontal del rack cambian (en enlazado, las 4 colocaciones) |
| OV-ID19-07 | Layout con semilla girada 90° → Frontal | Frontales rectas, alineadas segun la corrida girada |
| OV-ID19-08 | Plantas y una frontal mezcladas; frontal y planta del mismo rack; una Cama; un Cantilever junto a Selectivos; una planta espejada; un MINSERT | Error explicito en cada caso, **sin pedir puntos** y sin escribir |
| OV-ID19-09 | Esc en punto base o destino | Nada escrito ni importado |
| OV-ID19-10 | Plantas de Selectivo, Dinamico y Push Back en una fila → Frontal | Frontal canonica de cada sistema, alineadas por el eje del poste 0 |
| OV-ID19-11 | Linea Cantilever y otra Cantilever en fila → Frontal | Frontales de Cantilever alineadas por la columna de la estacion 0 |
| OV-ID19-12 | Rack con variable de proyecto vinculada → proyectar → cambiar la variable | La vista usa el valor efectivo y se actualiza con las demas |
| OV-ID19-13 | Push Back con diagnostico bloqueante en la seleccion | Error que nombra el rack; nada escrito |
| OV-ID19-14 | Seleccion de 100 o mas plantas → Frontal | Termina; registrar duracion y numero de racks |
| OV-ID19-15 | Opcional, con biblioteca de prueba sin un bloque | Error con la lista de faltantes; nada escrito |

### OV-META y OV-PR

| # | Pasos | Esperado |
|---|---|---|
| OV-META-01 | `RACKPROPIEDADES` en un rack → insertar una vista (unica y en lote) y proyectar otra | Las vistas nuevas muestran las mismas propiedades |
| OV-META-02 | Desactivar cotas en frontal → lote F + L + P | Frontal sin cotas; las demas segun su politica |
| OV-META-03 | Rack con vinculo a variable → lote de vistas | El vinculo sigue en todas las vistas |
| OV-PR1-01 | Push Back con una frontera suprimida **antes** del corte elegido → insertar esa lateral | Se dibuja el poste real |
| OV-PR2-01 | Cantilever con brazos y tensores **visibles** en planta → insertar planta | Coincide con la vista previa (antes de PR-2 no los dibujaba) |

## R. Matriz de riesgos

| # | Riesgo | Prob. | Impacto | Mitigacion | Gate |
|---|---|---|---|---|---|
| R-01 | Regresion al re-enrutar todas las inserciones | Media | Alto | Goldens, guardas, OV-LEG | G9 |
| R-02 | Conflicto con I-49 G10 en la ventana del Selectivo | Alta | Bajo | Serializar | G7, G10, G12 |
| R-03 | I-52 congela autoridades incompatibles con X-1..X-6 | Media | Alto | STOP → Proposal V2 | G8, G9, G13, G14, G15 |
| R-04 | Censos en conflicto al integrar | Alta | Bajo | La segunda re-basa | G8, G12, G15 |
| R-05 | D-15 cambia el caso raro de redibujo fallido en la insercion de una vista | Baja | Bajo | Mensaje explicito; decision del Coordinador | G12 |
| R-06 | ID19 lento con selecciones grandes | Media | Medio | Un barrido, una lectura del registro, una importacion; OV-ID19-14 | G14, G15 |
| R-07 | `U` no agrupa el lote como espera el usuario | Baja | Bajo | OV-ID18-06 | G12 |
| R-08 | D-10 no observable por el Owner | Alta | Bajo | Guarda con teoria de mutacion | G9 |
| R-09 | Bloques de Push Back insertados con H-01 conservan su `Section` | Media | Medio | Sin migracion; documentado en el cierre de PR-1 | G4 |
| R-10 | Un builder cambia su marco local y desalinea ID19 | Baja | Medio | `RackViewFrameCharacterizationTests` y `RackViewFrameTests` fallan | G3, G7 |
| R-11 | La caracterizacion de marcos encuentra un par inconsistente | Media | Medio | El par queda fuera de ID19 hasta V2; nunca se ajusta el descriptor | G3 |
| R-12 | El Owner elige OD-7 = B | Baja | Medio | Diseno de §12.4 se reduce a una traslacion; OV-ID19 se reescribe | G14 |

## F. Mapa de archivos y simbolos

### F.1 Produccion nueva (propuesta)

| Ruta | Gate |
|---|---|
| `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDecodeDisposition.cs`, `RackViewEnvelopeCodec.cs`, `RackViewSupport.cs`, `RackViewFrame.cs` | G7 |
| `A/Views/Preparation/*` | G8 |
| `P/Views/RackViewPlacement.cs` | G9 |
| `A/Views/RackViewBatchPlan.cs` | G11 |
| `U/Views/RackViewBatchDialog.xaml(.cs)`, `U/Views/RackViewBatchDialogPresenter.cs`, `P/Views/RackViewBatchDriver.cs` | G12 |
| `A/Views/RackProjectionPlan.cs`, `A/Views/RackProjectionTransform.cs` | G14 |
| `P/RackProyectarCommands.cs` | G15 |
| Paso comun de plan, comparador por kind, nucleo neutral y primitivo de materializacion | segun X-1..X-4 |

### F.2 Produccion modificada

| Ruta | Simbolos | Gate |
|---|---|---|
| `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | `SelectedView` (G4); renombre (G7); lote (G12) | G4, G7, G12 |
| `P/RackCantileverCommands.cs`; `A/Systems/Cantilever/CantileverLineEditorAssembler.cs` | entradas del plan (G5); `DrawCantileverView` (G9); edicion (G12) | G5, G9, G12 |
| `U/Systems/Cantilever/RackCantileverWindow.xaml.cs` | peticion de rack nuevo (G6); lote (G12) | G6, G12 |
| `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs` | renombre | G7 |
| `P/Drawing/BlockPlacement.cs` | `PlaceAndReport`, `PlaceDefinition` | G9 |
| `P/RackSelectivoCommands.cs` | `DrawSelectiveViewFromAuthored` (G9); `RackSelectivo` y `EditSelective` (G12) | G9, G12 |
| `P/RackDinamicoCommands.cs`, `P/RackPushBackCommands.cs` | insercion (G9); creacion y edicion (G12) | G9, G12 |
| `P/RackCabeceraCommands.cs` | `DrawAndPlace` (G9); `RackCabecera`, `QuickCabecera` (G10, G12); `EditCabecera` (G9, G12) | G9, G10, G12 |
| `P/RackCamaCommands.cs` | insercion de `QUICKCAMA` | G9 |
| `U/Systems/Selective/RackSelectiveWindow.xaml.cs`, `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs` | renombre (G7); puertas (G10); lote (G12) | G7, G10, G12 |
| `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs` | puerta de planta (G10); lote (G12) | G10, G12 |
| `U/Editor/RackInsertionRequest.cs` | `HeaderInsertionRequest` (G10); `Views` (G11) | G10, G11 |
| `U/Editor/RackEditorSession.cs` | `RequestInsertViews` | G11 |
| `U/Editor/EditorModules.cs`, `P/RackMenuCommands.cs` | reenvio de peticiones | G10, G12 |
| `A/Persistence/RackDuplicationPlan.cs` | fachada (solo si I-55 extrae X-1) | G13 |
| `U/RackCommandReference.cs` | ayuda | G15 |

### F.3 NO TOCAR sin nueva revision de Arquitecto

`P/RackDuplicarCommands.cs` y `RackEnvelopeRestamp` (guardas de I-51); `P/RackCloner.cs`; `P/RackLayoutCommands.cs` y
`P/RackLayoutCommands.Fill.cs`; la API de `A/Persistence/RackEmbedComposer.cs`; `P/CustomPropertiesData.cs`,
`P/CustomPropertiesExecutor.cs`, `A/CustomProperties/*` y sus guardas; el contenido de `P/Drawing/LateralHeaderDrawer.cs` y
`P/Drawing/Cantilever/CantileverViewMaterializer.cs`; `P/Drawing/BlockLibraryImporter.cs` (solo consumo); la geometria de todos los
builders; `DynamicEditorDesignAssembler` y el lote de cabeceras de I-53D; `P/ProjectVariableMutationExecutor.cs`; `docs/HANDOFF.md`
hasta la integracion.

## P. Propiedad de rendimiento

| Operacion | Coste maximo | Dueno |
|---|---|---|
| Catalogo | 1 carga por comando | comando |
| Registro de variables | 1 lectura por comando | comando |
| Barrido de sobres | ID19: 1 (SNAPSHOT, del que sale tambien la autoridad authored); edicion: los barridos vigentes | SNAPSHOT / comando |
| Resolucion del sistema | 1 por `RackId` | editor o contexto de preparacion |
| Serializacion authored | 1 por `RackId` | contexto de preparacion |
| Importacion de bloques | ID19: 1 con la union de nombres | colocacion |
| Transacciones de escritura | ID18: 2 por vista, ninguna abierta entre jigs; ID19: 1 | colocacion |
| `Regen` | segun §11.4 y §15 de la Proposal | comando |

## S. Estado

```text
Implementation Map V1 = NOT CONSENSUS
Coordinator = REVIEW REQUIRED · Architect = REVIEW REQUIRED · Consensus = NOT REACHED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
