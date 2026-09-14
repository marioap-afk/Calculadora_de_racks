# I-55 — Implementation Map V4: View Placement & Projection

```text
PROPOSAL V4 — NOT CONSENSUS
Coordinator      = REVIEW REQUIRED
Architect formal = PENDING
Owner            = PENDING
Consensus        = NOT REACHED
Implementation   = BLOCKED
Open Material    = M-01 (propio) · adopcion de X-1..X-8 por I-52
CQ-01            = decidida por el Coordinador en G2E: redibujo atomico de hermanas en Insertar (Proposal V4 §0.2, §11)

Acompana a     = docs/initiatives/I-55-proposal-v4.md (misma version del plan; ninguno vale sin el otro)
Sustituye a    = docs/initiatives/I-55-implementation-map-v3.md (historico, sin modificar)
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
    └─ G5 (PR-2, H-02) ─┴─ G6 (foundation) ─ G7a (Resolve) ─ G7b (Plan + Prepare) ─┬─ G8 (colocacion) ─────────────┐
                                                                                    └─ G9a (seam atomico, sin cablear) ─┴─ G9b (Insertar)
                                                                                                                              │
    ┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
    └─ G10 (ID17) ─ G11 (ID18 contrato) ─ G12 (ID18 UI) ─┐
                                   G13 (X-1, con acuerdo) ─┴─ G14 (ID19 puro) ─ G15 (ID19 comando) ─ G16
```

| Gate V3 | Gate V4 | Cambio |
|---|---|---|
| G3..G8 | G3..G8 | Sin cambio |
| G9 | **G9a** + **G9b** | G9a: seam de redibujo atomico (plan y ejecucion puros con puerto, unidades `HeaderRun`, `CantileverCurves` y `Erase`, envoltorios de Dinamico, Push Back y planta de cabecera, adaptador), sin cablear. G9b: variante, gate, clasificacion, PREPARE, redibujo atomico, huerfanas e informe en Insertar |
| G10 | G10 | Precondicion G9b |
| G11, G12 | G11, G12 | Estados de CQ-01; el driver de lote usa el seam de G9a; cancelar tras el commit conserva el redibujo |
| G13..G16 | G13..G16 | Sin cambio |

| Coordinacion | Gates de I-55 | Contraparte (I-52 V14, `e59bc89`) | Regla |
|---|---|---|---|
| Reconciliacion obligatoria de las autoridades compartidas | Antes del Consensus Freeze (precondicion de G3) | [V11-D10], [V12-D11], [V13-D08], [V14-D08], [V14-D14], §14.1, §15.4, R-45 | Propiedad registrada por ambas sin duplicar; STOP simetrico; tabla de adopcion en la Proposal V4 §17.2.1 |
| X-1 nucleo de seleccion | G3 (CT-16), G13 | §5.1, CT-16 (provisionales) | Custodio I-52; primer gate I-52 G4 o I-55 G13; el asignador de identidad queda en I-52; politicas por llamador |
| X-2 taxonomia, codec, disponibilidad, `Resolve`, `Plan` | G3 (CT-04, paridad), G6, G7a, G7b | §3.7, §3.8, §6.1, §8.1, §8.4, §14.2 G4..G6, §20.1, CT-04 (provisionales en V14) | Custodio I-55 en `A/Views`; **extractor unico I-55** (I-52 excluye `KindHandlers/*` y editores WPF, §20.3); I-52 consume en G4, G5 y G6; `Build` = `Resolve` + `Plan` |
| X-3 comparador authored por kind | G14 | §5.4, §6.1, §8.5 (provisionales en V14) | En el contrato del kind; el reflector delega; primer gate I-52 G5 o I-55 G14 |
| X-4 primitivo + requisito estructural | G7b (parte pura), G8 (consulta), G9a (redefinir, consumo), G15 | §8.2 (provisional; `CreateInTransaction`), §8.7, §19 (lista STOP) | Crear en la transaccion del llamador: custodio I-52, I-55 consume en G15. Redefinir en la transaccion del llamador: ya existe en `main` (I-47 G9), nadie lo extrae, I-55 G9a lo llama sin modificar `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs` ni `CantileverViewMaterializer.cs`. Requisito puro en `A/Drawing/LibraryBlockRequirement.cs`: primer gate I-55 G7b o I-52 G6 |
| X-5 protocolo | G5, G6, G7a, G7b, G8, G9a, G9b, G10, G12, G13, G14, G15 | §15.2, §15.3 (re-evaluacion si un gate de I-55 toca rutas de materializacion, `SystemBlockWriter.cs` o comandos), §19 | Comprobacion de existencia y reporte antes de fijar archivos con la lista de re-evaluacion (§8.1, §8.7, `MaterializationContextReadSet`, PRE-17, PRE-18, T-GRD-02, censo `B`); I-52 anade G4 y G6 |
| X-6 juego unico de C-2 | G5, G7b, G8, G9b, G12 | §8.5 (provisional), G-M9/CT-26, G-M24 ([V14-D05]) | La segunda en integrar re-establece C-2 y el mapa de invocaciones de Insertar; los sitios nuevos del redibujo atomico se declaran en G-M24; fixture de planta Cantilever con visibilidad; C2-2b con redibujo todo o nada |
| X-7 valor de colocacion y transformacion fuente | G14, G15 | §3.3, §8.3 (provisionales en V14); §3.2 consumido sin cambio; §4.3 (tolerancia de escala) | Custodio I-52; primer gate I-52 G4 o I-55 G14; un valor, una descomposicion de la fuente y una tolerancia de escala |
| X-8 `RackViewFrame` con tramos | G3 (CT-05), G6, G14 | §6.1 (`c`), §8.1 (tramo), CT-05 (provisionales en V14), CT-06 | Custodio I-55; extractor unico I-55 G6; `c` por el origen; una caracterizacion |
| Archivo caliente del Selectivo | G6, G10, G12 | I-49 G10 | Serializar |
| Censos | G7b, G12, G15 | I-52 (`Compose`, comandos, ayuda) | La segunda en integrar re-basa |

## 2. Gates

### G3 — Caracterizacion (solo pruebas)

| Campo | Contenido |
|---|---|
| Objetivo | Fijar sobre produccion intacta lo que I-55 preserva, cambia o necesita medir |
| Precondiciones | Coordinator = AGREED y Architect formal = AGREED sobre la misma version; **M-01 resuelta**; reconciliacion con I-52 registrada ([V11-D10], [V12-D11], [V13-D08], [V14-D08]); Consensus Freeze; ADR-0042 aceptado; OD-1..OD-8 decididas |
| Produccion | Ninguna |
| T/ `RackViewEnvelopeReadingCharacterizationTests` (= CT-04) | `PushBackSystemFrontalBuilder.EncodeSection/DecodeSection/IsValidSection`, `CantileverViewPlanBuilder.SectionFor`, `RackListBuilder` con `View` vacio (`A/Persistence/RackListBuilder.cs:117-118`). Guarda de las lecturas del Plugin con la lista de I-52 V14 §8.4: `P/RackSelectivoCommands.cs:126-177`; `P/RackDinamicoCommands.cs:209-239`, `:392-393`; `P/RackPushBackCommands.cs:218-258`, `:427-450`; `P/RackCantileverCommands.cs:295-314`, `:501-540`; `P/RackCabeceraCommands.cs:239-240`; `P/ProjectVariableMutationExecutor.cs:180-218`; mas, fuera de esa lista, `P/RackSelectivoCommands.cs:216`, `P/RackBlockFinder.cs:23-24` y `P/RackLayoutCommands.cs:71`. Incluye los tokens con espacios y la regla de la peor disposicion (Proposal §7.3 A). Separa lo sintactico (§7.3 A) de lo que depende del sistema resuelto (§7.3 B) |
| T/ `LegacyViewPayloadCompositionCharacterizationTests` | Formula de composicion por sistema + guarda de `BuildSelectivePayload`/`WrapSelectivePayload` (`P/RackSelectivoCommands.cs:344`, `:382`), `BuildDynamicPayload` (`P/RackDinamicoCommands.cs:361`), `BuildPushBackPayload` (`P/RackPushBackCommands.cs:397`), `BuildCantileverPayload` (`P/RackCantileverCommands.cs:472`), `BuildCabeceraPayload` (`P/RackCabeceraCommands.cs:181`), `BuildCamaPayload` (`P/RackCamaCommands.cs:175`, `:183`) |
| T/ `LegacyViewBlockNameCharacterizationTests` | Todas las funciones de nombre base de la Proposal §4.4 (servicios de dibujo, sufijo «frente F{n}», valores por defecto, cabecera desde catalogo, prefijo, saneado y sufijo `_2` del Cantilever); prefijos de agrupacion (`A/Drawing/HeaderInstanceGrouper.cs:21`, `:96`); nombre unico en el Plugin (`P/Drawing/LateralHeaderDrawer.cs:240-247`); predicado de nombre de tabla de bloques frente a los dos saneadores (`A/BlockNaming.cs:19-28`; `P/Drawing/Cantilever/CantileverViewMaterializer.cs:298-312`) |
| T/ `RackCountInvariantCharacterizationTests` | Vistas frente a racks en `RackListBuilder`; representante de `BomAuthoredAuthority` (`A/Bom/BomAuthoredAuthority.cs:104-109`) |
| T/ `RackSiblingScanInputCharacterizationTests` | Guarda de fuentes: `ScanEnvelopes` recorre definiciones con payload no interpretable y omite las de xref pero no las dependientes (`P/RackBlockFinder.cs:57-91`); `FindRackBlocks` no expone dependencia ni texto y solo excluye el `Id` vacio (`P/RackCommandSupport.cs:109-134`) |
| T/ `RackViewFrameCharacterizationTests` (= CT-05) | Descriptores de la Proposal §12.3 sobre la salida de los builders: ejes y signos; origen fisico; **tramo `[K_min, K_max]` por (sistema, tipo, variante)**; convencion de extremos; `anchorOffset` de esquina (`A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`); cuadricula de la frontal de un fondo frente a la maestra (`A/Systems/Selective/SelectiveDepthLayout.cs:28-33`, `:52-55`); Push Back compuesto; Dinamico; cabecera; Cantilever con origen D interior (`A/Systems/Cantilever/CantileverColumnBaseDatum.cs:19-24`, `:45`) |
| T/ + TU/ `HeadlessResolutionParityCharacterizationTests` | Por kind: la resolucion sin editor del payload persistido frente al sistema dibujado al insertar y frente al sistema del editor al abrir (compartida con la C2-4 de I-52), **con fixtures legacy**, entre ellos un documento Dinamico solo con `DynamicSystem` comparado con el sistema dibujado. **Una diferencia no explicada saca ese caso de ID19** (`UnsupportedLegacy`) |
| T/ `RackEditPreflightCharacterizationTests` | Preflights de hermanas de `RACKEDITAR`: Push Back y Cantilever (otro kind, otro `Id`, descriptor invalido; `P/RackPushBackCommands.cs:194-203`, `:216-223`; `P/RackCantileverCommands.cs:243-252`, `:264-271`); Dinamico y Cabecera (interior incompatible; `P/RackDinamicoCommands.cs:186-191`; `P/RackCabeceraCommands.cs:250-256`) |
| T/ `BuilderCatalogSkipCensusTests` | Censo de los sitios donde un builder omite una pieza sin bloque en el catalogo (`A/Systems/Selective/SelectiveLateralBuilder.cs:287-290`, `:393-396`; `A/Systems/PushBack/PushBackLoadBeamGeometry.cs:139-143`; resto) frente a los que emiten nombre nulo; insumo de OM-22 |
| T/ `BomResolutionCharacterizationTests` | BOM y motivos vigentes de los handlers de Selectivo, Dinamico (incluido el documento solo-sistema), Push Back, Cantilever, Cabecera y Cama, como linea base de la delegacion de G7a: Push Back bloqueado aborta el total; linea Cantilever invalida y catalogo de secciones ausente se saltan con aviso; referencia rota aborta (`P/RackInventarioCommands.BomTotal.cs:175-190`, `:200-206`, `:209-215`) |
| T/ `LibraryBlockRequirementCharacterizationTests` | Roles que requieren bloque en la salida real de los builders; ninguna instancia con rol de bloque sin nombre en los fixtures vigentes |
| TU/ `FirstViewGateCharacterizationTests` | Habilitacion y tooltips de las puertas de primera vista, sin rutas modales |
| T/ `RackDuplicationPlanObservableSemanticsTests` (= CT-16) | Semantica observable del nucleo de seleccion; **una sola**: la corre el primer G3 que llegue, I-52 o I-55 |
| RED | Caracterizaciones verdes sobre codigo intacto; cada guarda demuestra deteccion por teoria de mutacion |
| GREEN | Clases nuevas con conteo; Core + UI; CI 4/4 |
| Impacto | Ninguno |
| Si falla una caracterizacion de marco o de paridad | El par o el caso queda fuera de ID19 hasta una Proposal nueva o una decision posterior; nunca se ajusta el descriptor |

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
| Pruebas | T/ `CantileverPlantaVisibilityGuardTests.BothPluginPlanCalls_PassTheDesignPlantaVisibility` (guarda con teoria de mutacion); T/ `CantileverPlantaVisibilityBuilderTests.Planta_DrawsArmsAndBraces_OnlyWhenTheDesignShowsThem` (caracterizacion del builder, `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:293-295`) |
| RED | La guarda vista fallando; la prueba del builder es caracterizacion (verde) |
| GREEN | T0 `FullyQualifiedName~CantileverPlantaVisibility` con conteo; `FullyQualifiedName~Cantilever` Core; Core + UI; Debug Plugin; **OV-PR2** |
| Impacto | Solo plantas cuyo diseno muestra brazos o tensores |

### G6 — Foundation pura

| Campo | Contenido |
|---|---|
| Objetivo | D-02, D-03 y D-04, con descriptores de marco, sin cambio de comportamiento |
| Precondiciones | G4, G5; X-5 reportado por el renombre (si la guarda de enums de I-52 ya lo alcanza en la base, decide su Coordinador); serializado con I-49 G10; extractor unico de X-2 y X-8 (se consume solo si se registro la excepcion (b) de la Proposal §17.2) |
| Produccion nueva | `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDisposition.cs`, `RackViewAddressCodec.cs` (**solo sintaxis**), `RackViewAvailability.cs`, `RackViewSupport.cs` (matriz), `RackViewFrame.cs` (ejes, origen y `[K_min, K_max]` por variante) |
| Produccion modificada (renombre) | `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs`, `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs`, `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`, `U/Systems/Selective/RackSelectiveWindow.xaml.cs` |
| Pruebas nuevas | T/ `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewAddressCodecTests` (tabla de §7.3 A con tokens con espacios, peor disposicion y el Dinamico sin seccion como `Coerced`; ninguna prueba consulta un sistema resuelto), `RackViewAvailabilityTests` (§7.3 B con catalogo: `Fondo(8)` en un rack de 3 fondos, `Post(5)` fuera de `Cortes`, `Station(9)` ausente, lado B en un Push Back de un sentido → `Orphaned`; sistema sin cortes → `Unsupported` con motivo; Cama en ID19 → `Unsupported`), `RackViewSupportMatrixTests`, `RackViewFrameTests` (reproducen CT-05 con tramos independientes de las fronteras omitidas) |
| Pruebas modificadas | Renombre en las seis clases de pruebas que nombran el tipo |
| RED | Esqueleto + pruebas vistas fallando |
| GREEN | T0 `FullyQualifiedName~RackView`; T1 `FullyQualifiedName~DimensionView`; con conteo; Core + UI; Debug UI |
| Impacto | Ninguno observable |

### G7a — `Resolve` compartido (D-17)

| Campo | Contenido |
|---|---|
| Objetivo | Una autoridad de resolucion sin editor con resultado tipado; los handlers del BOM delegan en ella sin cambio observable |
| Precondiciones | G6; reconciliacion X-2 (extractor unico I-55, salvo la excepcion registrada (b) de la Proposal §17.2, en cuyo caso se consume); X-5 reportado |
| Produccion nueva | `A/Views/Resolution/RackSystemResolution.cs` (resultado `Resolved`, `UnsupportedLegacy`, `Blocked`, `BrokenReference`, `DependencyUnavailable`, `Unreadable`), `A/Views/Resolution/RackSystemResolver.cs` (por kind sobre `KindDispatch<T>`); el catalogo de secciones del Cantilever llega como dependencia (hoy `StructuralSectionCatalogAccess.TryLoad`, `P/KindHandlers/CantileverKindHandler.cs:60`) |
| Produccion modificada | `P/KindHandlers/SelectiveKindHandler.cs`, `DynamicKindHandler.cs`, `PushBackKindHandler.cs`, `CantileverKindHandler.cs`, `CabeceraKindHandler.cs`, `CamaKindHandler.cs`: delegan la resolucion; su BOM y sus motivos no cambian (politica del BOM por kind: Cantilever `Blocked` → salto con aviso) |
| Pruebas nuevas | T/ `RackSystemResolutionTests` (por kind y resultado; documento Dinamico solo-sistema → `Resolved` con el diseno reconstruido, o `UnsupportedLegacy` si G3 registro la diferencia; referencia rota → `BrokenReference`; registro ilegible en el contexto; Push Back bloqueado y linea Cantilever invalida → `Blocked`; catalogo de secciones ausente → `DependencyUnavailable`; payload ilegible → `Unreadable`; diseno en memoria); T/ `KindHandlerResolutionDelegationGuardTests` (ninguno de los seis handlers conserva resolucion propia, con teoria de mutacion) |
| Pruebas que deben seguir verdes | `BomResolutionCharacterizationTests` (G3) byte a byte; suites del BOM |
| RED | Esqueleto + pruebas vistas fallando; la guarda de delegacion falla hasta migrar |
| GREEN | T0 `FullyQualifiedName~RackSystemResolution`; T1 `FullyQualifiedName~KindHandlerResolutionDelegation\|FullyQualifiedName~BomResolutionCharacterization`; con conteo; Core + UI; Debug Plugin; **OV-BOM-01** |
| Impacto | Ninguno observable en el BOM (R-16) |

### G7b — `Plan`, `Prepare` y requisito estructural (X-2, D-19, D-13)

| Campo | Contenido |
|---|---|
| Objetivo | D-01 en Application: semilla, plan desde el sistema resuelto, sobre y requisitos de bloque |
| Precondiciones | G7a; reconciliacion X-2 (extractor unico; se consume solo con la excepcion (b)) y X-4; X-5 reportado |
| Produccion nueva | `A/Views/Preparation/*`: contexto, resultado, registro, `RackViewNameSeed.cs`, `RackViewPlanner.cs` con todas las direcciones y la planta Cantilever con `PlantaVisibility`, seis preparadores, `RackViewEnvelopeComposition.cs` con la unica llamada nueva a `Compose` y el origen como parametro `RackEmbedDocument`. Tambien `A/Drawing/LibraryBlockRequirement.cs` (parte pura y predicado de nombre; primer gate con I-52 G6: si ya existe, se consume) |
| Pruebas nuevas | T/ `RackViewNameSeedTests` (determinista; sin I/O; **byte a byte igual a los nombres base de G3**; prefijo de agrupacion), `RackViewPlannerTests` (semilla → prefijo; nombre sugerido; ninguna geometria depende de la semilla; lateral Selectivo con largueros fusionados), `LibraryBlockRequirementTests` (rol de bloque con nombre nulo, vacio, en blanco o invalido → requisito incumplido; clave fuera de la union; `Annotation` y `Dimension` no requieren; planes `CantileverCurves` sin requisitos), `RackViewPreparation{Selective,Dynamic,PushBack,Cantilever,Cabecera,Cama}Tests`, `PreparedViewPayloadGoldenTests` (**byte a byte = formula legacy de G3**), `RackIdLifecycleTests` |
| Guardas reapuntadas con motivo | `CustomPropertiesEnvelopeGuardTests.TGrd02_*` (+1 archivo y llamada; primer argumento parametro `RackEmbedDocument`), sin debilitarlas |
| RED | Esqueleto + goldens vistos fallando; `TGrd02` falla hasta reapuntarse |
| GREEN | T0 `FullyQualifiedName~RackViewNameSeed\|FullyQualifiedName~RackViewPlanner\|FullyQualifiedName~LibraryBlockRequirement\|FullyQualifiedName~RackViewPreparation\|FullyQualifiedName~PreparedViewPayload\|FullyQualifiedName~RackIdLifecycle` con conteo; T1 `FullyQualifiedName~CustomPropertiesEnvelopeGuard`; Core + UI |
| Impacto | Codigo nuevo sin consumidores |

### G8 — Colocacion de vista unica

| Campo | Contenido |
|---|---|
| Objetivo | Toda insercion de vista unica pasa por `Prepare` + colocacion sin cambio observable; D-10; prompt; reporte estructural |
| Precondiciones | G7b; coordinacion G-M9 (X-6); X-5 reportado |
| Produccion nueva | `P/Views/RackViewPlacement.cs`; consulta de la tabla de bloques tras importar en `P/Systems/Shared/` (primer gate con I-52 G6: si ya existe, se consume) |
| Produccion modificada | `P/Drawing/BlockPlacement.cs` (`PlaceAndReport`: limpieza ante excepcion `:49-52`, solo definiciones sin referencias, best effort y registrada (`:130-169`), y parametro de prompt; `PlaceDefinition`: limpieza ante excepcion). Insercion en `P/RackSelectivoCommands.cs` (`:426-453`), `P/RackDinamicoCommands.cs`, `P/RackPushBackCommands.cs`, `P/RackCantileverCommands.cs` (limpieza dentro de `PlaceDefinition`, porque `definitionId` vive dentro del `try`, `:142-161`), `P/RackCabeceraCommands.cs` (`:164-176`, `:294-310`) y `P/RackCamaCommands.cs` (`:102-106`) |
| Pruebas | T/ `RackViewPlacementGuardTests`: limpieza ante excepcion en ambas primitivas; ninguna insercion compone el sobre; **todo camino de insercion de ID17/ID18 imprime el reporte de faltantes** con el requisito estructural, incluidos los que hoy no lo hacen (`P/RackSelectivoCommands.cs:572-575`; `P/RackDinamicoCommands.cs:349-357`; `P/RackCabeceraCommands.cs:302-313`); con teorias de mutacion. Fixtures de equivalencia de Insertar (X-6). Reapuntadas con motivo: `SelectiveAuthoredCarrierAdoptionTests.GUARDA_LaInsercionNUEVA_CONSERVA_From`, `PushBackPluginSourceGuardTests` |
| RED | Guardas vistas fallando |
| GREEN | Guardas y goldens con conteo; Core + UI; Debug Plugin; **OV-LEG** |
| Impacto | Alto riesgo de regresion (R-01). D-10 no es observable por el Owner (R-08), y su purga de anidadas esta declarada (OM-21) |

### G9a — Seam de redibujo atomico de hermanas (D-15; CQ-01), sin cablear

| Campo | Contenido |
|---|---|
| Objetivo | Disponer, sin cambio observable, del redibujo atomico PREPARE → una MUTATE → POST para las hermanas existentes de todos los kinds con Insertar, con su comportamiento probado sin AutoCAD |
| Por que gate propio | RED/GREEN claros: la parte pura se prueba con un puerto de transaccion falso y los envoltorios del Plugin se verifican con guardas de fuentes, antes de tocar ningun flujo de usuario; G9b solo cablea |
| Precondiciones | G7b (composicion de payloads en Application); G5 (plan Cantilever con `PlantaVisibility`); X-5 reportado a I-52 (§15.3 de V14: rutas de materializacion); **sin modificar** `P/Systems/Shared/SystemBlockWriter.cs`, `P/Drawing/LateralHeaderDrawer.cs` ni `P/Drawing/Cantilever/CantileverViewMaterializer.cs` (lista STOP de I-52 V14 §19) |
| Produccion nueva (Application) | `A/Views/Redraw/RackSiblingRedrawPlan.cs` (entrada: barrido con `Id == RackId` + definicion elegida que el flujo vigente anade + mismas `Id` de espacios; clasificacion READ-ONLY / REDRAW / ERASE; **descriptores tipados por vista y kind** `HeaderRun` / `CantileverCurves` / `Erase` con la peticion de plan del builder vigente y el payload compuesto igual al vigente; ubicacion del borrado de huerfanas; orden; resultados `PREPARE_FAILED` / `REDRAW_ROLLED_BACK` / `REDRAW_APPLIED` / `REDRAW_NOT_REQUIRED` y mensajes); `A/Views/Redraw/RackSiblingRedrawRun.cs` (secuencia PREPARE de todas → abrir → escribir cada unidad → commit o descarte → POST en captura propia) con el puerto `IRackSiblingMutationTransaction` (una excepcion o un fallo tipado descartan; una excepcion de POST no descarta) |
| Produccion nueva (Plugin) | `P/Systems/Shared/SiblingRedrawUnits.cs` (unidades concretas a partir de los descriptores: `HeaderRun` sobre `PreparedViewRedraw`, `CantileverCurves` sobre `CantileverViewMaterializer.RedefineBlock` + `RackBlockData.Write`, `Erase` sobre un borrado nuevo en la transaccion del llamador que recoge tambien las anidadas *D; `RackCommandSupport.EraseViewBlocks` no se toca, OM-29); `P/Systems/Shared/SiblingRedrawTransaction.cs` (adaptador del puerto: un `LockDocument` sobre PREPARE, MUTATE y POST, una transaccion, un `Commit`; POST con `SystemBlockWriter.PurgeAfterCommit` excluyendo las definiciones que piden las vistas nuevas preparadas, `RackBlockRenamer.SyncName` y `SystemBlockWriter.ApplyRegen`; el informe de faltantes con la consulta de G8 se cablea en G9b) |
| Produccion modificada (envoltorios minimos) | `PrepareRedraw`/`RedrawInTransaction` con la **misma** lambda de plan que su `RedrawInPlace` en `P/Systems/Dynamic/DynamicSystemDrawService.cs` (`:51-59`), `DynamicFrontalDrawService.cs` (`:40-48`), `DynamicPlantaDrawService.cs` (`:38-46`), `P/Systems/PushBack/PushBackSystemDrawService.cs` (`:47-55`), `PushBackFrontalDrawService.cs` (`:45-53`), `PushBackPlantaDrawService.cs` (`:40-48`) y `P/Drawing/PlantaHeaderDrawService.cs` (`:34-43`). Selectivo y lateral de cabecera ya los tienen (`P/Systems/Selective/SelectiveFrontalDrawService.cs:42-59`; `SelectivePlantaDrawService.cs:37-54`; `P/Drawing/LateralHeaderDrawService.cs:60-107`) |
| T/ `RackSiblingRedrawPlanTests` | **(5)** una definicion dependiente de xref va a READ-ONLY y ninguna unidad la referencia; **(6)** con alguna hermana REDRAW, las huerfanas son unidades ERASE de la misma MUTATE; sin ninguna REDRAW (aunque queden dependientes de xref), el borrado se asigna al primer jig; la cabecera no produce huerfanas; `PushBackCut(e, B)` en un rack de un sentido y la lateral legacy del Dinamico van a REDRAW; la definicion elegida sin `Id` que el flujo vigente anade entra como REDRAW con el `Id` curado (Selectivo, Dinamico, Cabecera); en Selectivo y Cabecera las de `Id` de espacios identico se curan; sin duplicados por `BlockId`; sin unidades → `REDRAW_NOT_REQUIRED` y mensajes sin «vistas existentes actualizadas»; orden estable; mensajes de `PREPARE_FAILED` y `REDRAW_ROLLED_BACK` («No se inserto ninguna vista y no se aplico ninguna actualizacion a las vistas existentes») |
| T/ `RackSiblingRedrawRunTests` (puerto falso que registra llamadas) | **(1)** 3 hermanas, falla PREPARE de la segunda (tambien por payload vacio o `BlockId` nulo) → ninguna llamada a abrir, escribir ni confirmar; `PREPARE_FAILED`. **(2)** 3 hermanas, la escritura de la segunda lanza **o devuelve un fallo tipado** → la primera se escribio, no hay `Commit`, la transaccion se descarta, POST no corre; `REDRAW_ROLLED_BACK`. **(3)** todas pasan → exactamente un `Commit`, despues POST una vez; `REDRAW_APPLIED`. **(4)** el `Regen` de POST lanza → `REDRAW_APPLIED` con aviso, ningun descarte ni `REDRAW_ROLLED_BACK`; la purga y el renombre fallidos no producen aviso (se registran). **(9)** tras el exito, el payload escrito en cada hermana lleva el mismo authored que el de las vistas nuevas preparadas. **(10)** tras el rollback, el puerto no confirmo ningun payload y el authored persistido de cada hermana es el anterior. Ademas: no se abre la transaccion antes de terminar PREPARE; POST excluye de la purga las definiciones que piden las vistas nuevas |
| T/ `RackSiblingRedrawUnits{Selective,Dynamic,PushBack,Cantilever,Cabecera}Tests` (Application, Core) | **(11)-(15)** por kind: cada vista produce el descriptor que le corresponde (Selectivo frontal por fondo, lateral por poste con largueros y planta; Dinamico lateral, frontal por extremo y planta; Push Back lateral, frontal por extremo y lado, y planta; Cantilever frontal, lateral por estacion y planta como `CantileverCurves`; cabecera lateral y planta), con la peticion de plan del builder vigente y un payload **igual byte a byte** al de la formula vigente caracterizada en G3 (sobre propio e interior de `ResolvedByBlock`) y el nombre destino de renombre igual al vigente |
| T/ `SiblingRedrawSeamGuardTests` (guarda de fuentes, con teoria de mutacion) | El adaptador confirma una sola vez y despues de escribir todas las unidades; POST solo tras el `Commit` y en captura propia; en las unidades y dentro del bloque MUTATE no aparecen `EnsureForPlan`, `EnsureBlocks`, `StartTransaction`, `StartOpenCloseTransaction`, `LockDocument`, `.Commit(`, `RedrawInPlace`, `CreateBlock`, `PurgeUnreferenced`, `PurgeAfterCommit`, `Regen`, `SyncName`, `EraseViewBlocks` ni `catch`; el adaptador traduce cada descriptor a la llamada del envoltorio de su kind con sus argumentos (extremo y lado de Push Back, poste, estacion); cada envoltorio nuevo delega en `ViewBlockDraw.PrepareRedraw`/`RedrawInTransaction` con la lambda de plan de su `RedrawInPlace`; `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs` y `CantileverViewMaterializer.cs` sin cambios |
| RED | Esqueletos y pruebas vistos fallando; la guarda falla hasta crear los envoltorios |
| GREEN | T0 `FullyQualifiedName~RackSiblingRedrawPlan\|FullyQualifiedName~RackSiblingRedrawRun\|FullyQualifiedName~RackSiblingRedrawUnits` y T1 `FullyQualifiedName~SiblingRedrawSeamGuard`, con conteo; Core + UI; Debug Plugin |
| Impacto | Ninguno observable: nada llama todavia al seam |

### G9b — Precondiciones nuevas de `RACKEDITAR` → Insertar (vista unica)

| Campo | Contenido |
|---|---|
| Objetivo | Variante antes del gate; gate de propiedades acotado al `RackId` (D-07); clasificacion; PREPARE; redibujo atomico con el seam de G9a (D-15); huerfanas; informe; Actualizar sin cambio |
| Precondiciones | G8, G9a; X-5 reportado; X-6 (cambia a proposito la linea base C2-2b y el mapa de invocaciones G-M9/CT-26 de Insertar) |
| Produccion nueva | `A/Views/RackSiblingCustomPropertiesGate.cs` (consume `CustomPropertiesStore.ReadElement`, `A/Persistence/CustomPropertiesStore.cs:90`, y `CustomPropertiesCanonicalForm`, `A/CustomProperties/CustomPropertiesCanonicalForm.cs:24-70`); `A/Views/RackEnvelopeIdProbe.cs`; lectura de entradas del gate y de la clasificacion en el Plugin (sobre, texto del payload, dependencia de xref), p. ej. `P/Views/RackSiblingScan.cs` |
| Produccion modificada | Ramas Insertar de `EditSelective`, `EditDynamic`, `EditPushBack`, `EditCantilever` y `EditCabecera`: preflights vigentes (sobre, descriptor, interior I-11, catalogo de secciones, reconciliador, aviso de unidades) → variante (prompt legacy movido antes; Esc → nada modificado) → gate → clasificacion (con la definicion elegida del fallback) → PREPARE de la vista nueva y de las unidades → seam atomico de G9a → `REDRAW_ROLLED_BACK`, `REDRAW_APPLIED` o `REDRAW_NOT_REQUIRED` → colocacion. Gancho para borrar huerfanas en `P/Drawing/BlockPlacement.cs` (`PlaceBlockWithJig`, solo con el jig en OK y antes de su `Commit`, seguido de `Regen`), con las firmas que lo transportan en `P/Systems/Shared/ViewBlockDraw.cs` (`DrawAndPlace`; ubicacion provisional de I-52 G6 y disparador de su §15.3: X-5), `P/Drawing/LateralHeaderDrawService.cs` (`DrawAndPlace`), los `DrawAndPlace` de los servicios por kind y `PlaceDefinition` del Cantilever (`P/RackCantileverCommands.cs:161`). La rama Actualizar conserva su bucle `RedrawInPlace` vigente. Blanco = `IsNullOrWhiteSpace` |
| T/ `RackSiblingCustomPropertiesGateTests` | Sin sobre fuente ni miembros → sin gate; el sobre elegido siempre es miembro (incluida la cura de un `Id` en blanco o solo espacios); comun → hereda; misma forma canonica con distinta version menor → comun; divergente → fallo con remedio condicionado; miembro no escribible → fallo; `Kind` en blanco o varios → fallo; dependiente de xref fuera del gate; payload no interpretable con `Id` igual → fallo; sin `Id` legible o con otro `Id` → no bloquea |
| T/ `RackEnvelopeIdProbeTests` | `Id` de nivel superior sin distinguir mayusculas; `Id` anidado ignorado; duplicados; JSON roto despues o antes del `Id`; MAJOR mas nuevo → legible |
| T/ `RackSiblingGateWiringGuardTests` | Guarda con teoria de mutacion: en cada rama Insertar, variante antes del gate, gate y clasificacion antes de PREPARE, PREPARE antes del seam, colocacion solo tras `REDRAW_APPLIED` o `REDRAW_NOT_REQUIRED`; ninguna llamada a `RedrawInPlace`, `SyncName` ni `EraseViewBlocks` en la rama Insertar; la rama Actualizar sin cambios |
| T/ `RackSiblingGateIndependenceGuardTests` | El gate de propiedades vive en su archivo y no nombra Project Variables ni `RackCustomPropertiesAuthority` (fuera de la clasificacion de `TGrd05`, `T/CustomPropertiesEdgeGuardTests.cs:287-298`); G14 la extiende al gate authored |
| Guardas reapuntadas con motivo | `PushBackRoundTripSourceGuardTests` (`T/PushBackRoundTripSourceGuardTests.cs:235-241`, `:244-249`, con conteos de todo el archivo en `:240` y `:248`, que exigen un cuerpo acotado por metodo; `:252-258`, llamada exacta a `DrawPushBackView`; `:349-358`, conteo de `changedInPlace`); `CantileverPluginSourceGuardTests` (`T/CantileverPluginSourceGuardTests.cs:140-155`, `:263-271`: posicion de `RedefineBlock` y orden escritura/borrado); `RackUnitsGuardSourceTests.EditInsert_IsGatedByUpdateOnly_AndWarnsBeforeTheFirstRedraw` (`T/RackUnitsGuardSourceTests.cs:127-138`: se re-ancla en la importacion de PREPARE de la rama Insertar, que pasa a ser el primer cambio del dibujo); `LinkedPropertyViewPayloadTests` (`T/LinkedPropertyViewPayloadTests.cs:220-262`: ancla en `window.InsertView` de `EditSelective`); `PushBackPluginSourceGuardTests` (`T/PushBackPluginSourceGuardTests.cs:73-84`); ninguna se debilita |
| Guardas que deben seguir verdes | `CustomPropertiesEdgeGuardTests.TGrd04_*`, `TGrd05_*` |
| RED | Esqueletos y guardas vistos fallando |
| GREEN | T0 `FullyQualifiedName~RackSiblingCustomPropertiesGate\|FullyQualifiedName~RackEnvelopeIdProbe` y T1 `FullyQualifiedName~RackSiblingGate\|FullyQualifiedName~CustomPropertiesEdgeGuard\|FullyQualifiedName~RackUnitsGuardSource\|FullyQualifiedName~PushBackRoundTripSourceGuard\|FullyQualifiedName~CantileverPluginSourceGuard\|FullyQualifiedName~PushBackPluginSourceGuard\|FullyQualifiedName~LinkedPropertyViewPayload`, con conteo; Core + UI; Debug Plugin; **OV-META-04..06**, **OV-RED-01..04** y **OV-ID18-10/11** (vista unica) |
| Impacto | Insertar en un rack con propiedades divergentes o ilegibles falla con motivo; el prompt de variante llega antes del redibujo; las hermanas existentes cambian todas o ninguna antes de colocar la vista nueva; las huerfanas ya no quedan tras insertar; las dependientes de xref no se tocan (R-05, R-12, R-15, R-17, R-19, R-22) |

### G10 — ID17: primera vista libre

| Campo | Contenido |
|---|---|
| Objetivo | Puertas de la Proposal §10 |
| Precondiciones | G9b; serializado con I-49 G10; X-5 reportado |
| Produccion | `U/Systems/Selective/RackSelectiveWindow.xaml.cs` (`:274-292`, `:2550-2570`); `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs` (`:189-194`, `:3394-3403`); `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs` (`:85-93`, `:265-269`); `U/Editor/RackInsertionRequest.cs` (`HeaderInsertionRequest`, `:36-53`); `U/Editor/EditorModules.cs`; `P/RackCabeceraCommands.cs`; `P/RackMenuCommands.cs` |
| Pruebas | TU/ `FirstViewFreedomTests`; reapuntadas con motivo: `FirstViewGateCharacterizationTests`, `DynamicHeaderBatchSeamGuardTests.D34_GUARD_*` (un `MessageBox` → cero), `SelectiveHeaderBatchSeamTests.S30_GUARD_*` (cuatro → tres), `DynamicShellMigrationTests`, `RichEditorBlockedActionTests`, `RackInsertionRequestTests` |
| RED | `FirstViewFreedomTests` vistas fallando |
| GREEN | TU/ `FullyQualifiedName~FirstView` con conteo; Core + UI; Debug UI + Plugin; **OV-ID17** |
| Impacto | Cambio intencional de UX de creacion |

### G11 — ID18: contrato puro

| Campo | Contenido |
|---|---|
| Objetivo | D-06 y D-15 sin UI |
| Precondiciones | G10 |
| Produccion | `A/Views/RackViewBatchPlan.cs`: orden; estados `VARIANT_CANCELLED`, `SIBLING_GATE_FAILED`, `PREPARE_FAILED`, `REDRAW_ROLLED_BACK`, `REDRAW_APPLIED`, `REDRAW_NOT_REQUIRED`, `PLACEMENT_CANCELLED_PARTIAL_BATCH`, `PLACEMENT_FAILED_PARTIAL_BATCH` y `COMPLETED`; cola que solo arranca tras `REDRAW_APPLIED` o `REDRAW_NOT_REQUIRED` (rack existente); Esc y Enter; `PromptStatus.Error` como fallo; mensajes deterministas. `U/Editor/RackEditorSession.cs` (`RequestInsertViews`); `U/Editor/RackInsertionRequest.cs` (`Views`; historicos = primera direccion) |
| Pruebas | T/ `RackViewBatchPlanTests`: maquina completa y mensajes exactos; `PREPARE_FAILED` y `REDRAW_ROLLED_BACK` sin colocaciones; **(7)** `REDRAW_APPLIED` → Esc en el primer jig → `PLACEMENT_CANCELLED_PARTIAL_BATCH` con cero vistas nuevas y el redibujo conservado; **(8)** `REDRAW_APPLIED` → primera vista colocada → Esc en la segunda → redibujo y primera vista conservados, sin rollback; Error → `PLACEMENT_FAILED_PARTIAL_BATCH`; variante no disponible → `PREPARE_FAILED`; `REDRAW_NOT_REQUIRED` → cola con mensajes sin «vistas existentes actualizadas». TU/ `EditorViewBatchRequestTests`: una aceptacion → un `RackId`. Reapuntadas: `RackEditorSessionTests`, `RackInsertionRequestTests` |
| RED | Vistas fallando |
| GREEN | T0 `FullyQualifiedName~RackViewBatch\|FullyQualifiedName~EditorViewBatch` con conteo; Core + UI |
| Impacto | Codigo nuevo sin consumidores en el Plugin |

### G12 — ID18: dialogo, presentador, driver y rutas de entrada

| Campo | Contenido |
|---|---|
| Objetivo | ID18 de extremo a extremo |
| Precondiciones | G11; serializado con I-49 G10; X-5 reportado; X-6 |
| Produccion nueva | `U/Views/RackViewBatchDialog.xaml(.cs)` (arquetipo C), `U/Views/RackViewBatchDialogPresenter.cs` (fuera de los archivos de ventana), `P/Views/RackViewBatchDriver.cs` (variantes → gate → clasificacion → PREPARE → seam atomico de G9a → `REDRAW_ROLLED_BACK` o cola de colocaciones con su propia transaccion por jig; `Regen` segun la Proposal §11) |
| Produccion modificada | Ventanas con vistas y configurador; `U/Editor/EditorModules.cs` (`:53-56`, `:90-95` reenvian `Views`); `RACKSELECTIVO` (`P/RackSelectivoCommands.cs:25-45`); cabecera (un acunado por intencion); comandos de creacion; `P/RackMenuCommands.cs`; ramas `Edit*` |
| Pruebas | TU/ `RackViewBatchDialogTests`; T/ `RackViewBatchDriverGuardTests` (gate y PREPARE antes de escribir; seam atomico antes de la primera colocacion; ninguna definicion nueva tras `REDRAW_ROLLED_BACK`; ninguna transaccion abierta entre jigs; Esc y Enter detienen), `RackViewEntryPathGuardTests` (toda ruta reenvia `Views` y acuna una vez); reapuntadas: `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `EditorModuleRegistryTests`, `RackUnitsGuardSourceTests`, `CantileverPluginSourceGuardTests.ExactlyOneRegenForTheWholeBatch_AndItAgreesWithWhatIsReported` |
| RED | Vistas fallando |
| GREEN | Focales con conteo; Core + UI; Debug UI + Plugin; **OV-ID18**, **OV-RED-05**, **OV-META-01..03** |
| Impacto | Boton nuevo en cinco editores |

### G13 — Nucleo neutral de seleccion (X-1)

| Campo | Contenido |
|---|---|
| Objetivo | Nucleo neutral disponible sin cambiar `RACKDUPLICAR` |
| Precondiciones | Reconciliacion X-1; X-5 reportado; coordinacion con I-49 antes de editar `A/Persistence/RackDuplicationPlan.cs` |
| Si I-52 ya lo extrajo | Consumirlo; T/ `RackSelectionCoreConsumptionTests` |
| Si no | Extraer con CT-16 (G3) y la fachada verde, sobre la especificacion de I-52 V14 §5.1; pruebas de I-51 sin cambios |
| RED | Pruebas de consumo vistas fallando; CT-16 verde antes y despues |
| GREEN | `FullyQualifiedName~RackDuplicationPlan\|FullyQualifiedName~RackSelectionCore` con conteo; Core + UI |

### G14 — ID19 puro: Group Placement y politicas

| Campo | Contenido |
|---|---|
| Objetivo | D-07 (ID19), D-08, D-16, D-17 (consumo), D-18 y D-19 sin AutoCAD |
| Precondiciones | G12, G13; **M-01 resuelta**; reconciliacion X-3, X-7 y X-8; X-5 reportado |
| Produccion nueva | `A/Views/Placement/SourceGroupFrame.cs`, `TargetGroupFrame.cs`, `CommonTransform2D.cs` (sobre `Transform2D`, `A/Geometry/Transform2D.cs:20-116`, con el valor de colocacion compartido de X-7 en `A/Geometry/`), `RackGroupAnchorPolicy.cs` (`Rigid`, `Orthographic`), `RackGroupPlacementPlan.cs` (orden CLASSIFY → GROUP → RESOLVE → AVAILABLE → FRAMES → VALIDATE → GATES → PLANS; politicas de ID19 sobre codec, disponibilidad y `Resolve`; mensajes deterministas; avisos mostrados antes de PICK), `A/Views/RackSiblingAuthoredGate.cs`, comparador por kind de X-3 (se consume si I-52 G5 ya lo creo), valor de colocacion, descomposicion de la fuente y tolerancia de escala de X-7 (se consumen si I-52 G4 ya los creo) |
| T/ `CommonTransform2DTests` | Rigidez (`Determinant = +1`, `ScaleFactor = 1`, sin cizalla); composicion `Translation(−B).Then(Rotation(α)).Then(Translation(T))` lleva `B` a `T`; expresion con el valor de colocacion `(T − R(α)·B, α, 1)`; **una sola instancia por ejecucion, compartida por todos los grupos `RackId`** |
| T/ `RackGroupFrameTests` | `B` y `T` de SCP a universal; `φ_s`, `φ_t` segun OD-6.b/c; SCP no paralelo → fallo; angulo comun pedido si OD-6.c = B |
| T/ `RackRigidPlacementPolicyTests` | Misma clase con `α = 0` y `α ≠ 0`; layout fuente rotado; orientaciones distintas entre racks; las ocho combinaciones de signo de escala (`(−1,−1,+1)` como giro de π; las otras seis con algun signo negativo, incluidas `(−1,+1,+1)`, `(+1,−1,+1)` y `(+1,+1,−1)`, fallan con «escala negativa no admitida»); angulos modulo 2π; `Z_r`; familias mezcladas con su descriptor; frontal ↔ lateral; `Origin ≠ 0`; variante segun OD-2.b; INV-GRP-1; INV-GRP-2 |
| T/ `RackOrthographicPlacementPolicyTests` | **Direccion destino desde el marco destino de cada referencia.** Casos: **Rack Planta→Frontal**, **Rack Planta→Lateral**, **Rack Frontal→Planta**, **Rack Lateral→Planta**, **Cantilever Planta→Frontal**, **Cantilever Planta→Lateral**, **Cantilever Frontal→Planta**, **Cantilever Lateral→Planta** (valores de `k_s`, `k_t`, `w`, `e` y `α` de la tabla de la Proposal §12.5). Ida y vuelta Planta→Frontal→Planta y Planta→Lateral→Planta en ambas familias: restituye el intervalo de cada referencia sobre K; con layouts escalonados en R o en D segun el caso; casos que **no** restituyen (fila apilada al volver de Lateral, layout girado 30°, rack girado 180°). Recta comun por suma alineada, independiente del orden de seleccion y de la referencia inicial; sentido de `e` por mayoria, con un caso donde la mayoria de un grupo `RackId` es opuesta a la global y otro donde anadir un rack girado invierte el orden (Proposal §12.5); empate en la frontera con `ε_ang` (rotacion π/4 y π/4 − 1e-15 dan el mismo sentido); planta a 0 y a 2π − ε iguales. Rack girado 180° con tramo destino (mismo intervalo, ancla opuesta). Racks de tramos distintos alineados por `K_min`; variante con tramo distinto de la fuente. Superposicion por interseccion de intervalos con `ε_ov` (intervalos que se tocan sin aviso; filas escalonadas sin aviso; misma fila con rack girado sin aviso; intervalos solapados con aviso o fallo segun OD-7.b), calculada en FRAMES. Tramos independientes de fronteras omitidas. Orientaciones no paralelas → fallo; familias mezcladas segun OD-7.d (perpendiculares → fallo con B); `Z_r = T.Z`; **INV-GRP-3 e INV-GRP-4 sobre la colocacion real**, con mutaciones de `a_t` y de `ρ` que deben hacer fallar la prueba |
| T/ `RackGroupPlacementPlanTests` | Todo fallo de clasificacion, resolucion, disponibilidad, marcos, validacion, gates o planes (incluidas las comprobaciones puras de bloques) antes de los puntos, con la etapa correcta; un aviso de superposicion no se muestra si una etapa posterior anterior a los puntos falla; layout independiente y **enlazado** (una definicion nueva por definicion fuente, N referencias; copias del BOM sin cambio, `k ≤ max` tambien con varias definiciones fuente si OD-7.c = B); varias definiciones de un `RackId` y tipos fuente mezclados segun OD-7.c; `Coerced` e `Invalid` → fallo en CLASSIFY; `Orphaned` y `Unsupported` → fallo en AVAILABLE; `Resolve` distinto de `Resolved` → fallo; hermana que el preflight de `RACKEDITAR` rechazaria → fallo; MINSERT; referencia dinamica, anonima o anotativa; reflexion; escala ≠ 1; normal ≠ Z; `RackId` en blanco; authored divergente o ilegible; propiedades divergentes o ilegibles del grupo; payload sin `Id` atribuible que no bloquea; Cama; miembro no soportado lista todos los miembros (OD-3); orden estable de mensajes |
| T/ otras | `RackSiblingAuthoredGateTests`; `RackSiblingGateIndependenceGuardTests` extendida (el gate authored no nombra Custom Properties); `RackViewCountInvariantTests`, `RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests` |
| RED | Vistas fallando |
| GREEN | T0 `FullyQualifiedName~CommonTransform2D\|FullyQualifiedName~RackGroup\|FullyQualifiedName~RackRigidPlacement\|FullyQualifiedName~RackOrthographicPlacement\|FullyQualifiedName~RackSiblingAuthoredGate\|FullyQualifiedName~RackViewCountInvariant\|FullyQualifiedName~RackViewAuthoredEquivalence\|FullyQualifiedName~RackViewPartialRack`; T1 `FullyQualifiedName~RackSiblingGateIndependence`; con conteo; Core + UI |

### G15 — ID19: comando y primitivo de materializacion

| Campo | Contenido |
|---|---|
| Objetivo | ID19 de extremo a extremo |
| Precondiciones | G14; OD-1 y OD-8 decididas; reconciliacion X-4 y X-7; X-5 reportado |
| Produccion nueva | `P/RackProyectarCommands.cs` (`RACKPROYECTAR` + `RPY` si OD-1 = A) y SNAPSHOT propio con todos los datos de la Proposal §12.1/D-08f. Consumo del primitivo compartido de X-4 (o su extraccion con la firma acordada si I-52 no lo creo), con Z y presentacion de creacion explicitas y verificacion estructural por pieza tras importar |
| Produccion modificada | `U/RackCommandReference.cs` |
| Pruebas | T/ `RackProjectionCommandGuardTests`: SNAPSHOT completo; resolver, disponibilidad, marcos, validar, gates y planes antes de los puntos; `GetPoint` con `AllowNone = true` y `None`, `Cancel` y `Error` tratados segun la Proposal §12.1; importar y comprobar presencia despues, con fallo antes de escribir definiciones o referencias de rack y causa «biblioteca no disponible» distinta; una transaccion; sin `Regen` ni purga; `TryResolve`; definicion nueva con `Origin = 0`; aviso final de vistas enlazadas, no copias. Reapuntadas: `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`, `CustomPropertiesCommandGuardTests.TGrd08_*` (censo por nombre), `CustomPropertiesHelpCensusTests`, `RackUnitsGuardSourceTests` |
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
| G3 | CT-04, CT-05 (tramos), CT-16, paridad de `Resolve` con fixtures legacy, BOM vigente, preflights de `RACKEDITAR`, censo de omisiones de builders, roles con bloque, payloads, nombres base, conteos, entradas del barrido, puertas de primera vista | comportamiento + guarda | Core + UI |
| G4 | `PushBackLateralPostIndexRegressionTests` | UI | UI |
| G5 | `CantileverPlantaVisibilityGuardTests`, `CantileverPlantaVisibilityBuilderTests` | guarda + comportamiento | Core |
| G6 | `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewAddressCodecTests`, `RackViewAvailabilityTests`, `RackViewSupportMatrixTests`, `RackViewFrameTests`; `*DimensionView*` | comportamiento | Core |
| G7a | `RackSystemResolutionTests`; `KindHandlerResolutionDelegationGuardTests`; `BomResolutionCharacterizationTests` | comportamiento / guarda | Core |
| G7b | `RackViewNameSeedTests`, `RackViewPlannerTests`, `LibraryBlockRequirementTests`, `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests`; `CustomPropertiesEnvelopeGuardTests` | comportamiento / guarda | Core |
| G8 | `RackViewPlacementGuardTests`; fixtures de Insertar; `SelectiveAuthoredCarrierAdoptionTests`, `PushBackPluginSourceGuardTests` | guarda | Core |
| G9a | `RackSiblingRedrawPlanTests`, `RackSiblingRedrawRunTests`, `RackSiblingRedrawUnits{Selective,Dynamic,PushBack,Cantilever,Cabecera}Tests`; `SiblingRedrawSeamGuardTests` | comportamiento / guarda | Core |
| G9b | `RackSiblingCustomPropertiesGateTests`, `RackEnvelopeIdProbeTests`; `RackSiblingGateWiringGuardTests`, guardas reapuntadas de Push Back, Cantilever y unidades, `RackSiblingGateIndependenceGuardTests`; `CustomPropertiesEdgeGuardTests`; `RackUnitsGuardSourceTests` | comportamiento / guarda | Core |
| G10 | `FirstViewFreedomTests`; guardas de modales y shell | UI / guarda | UI |
| G11 | `RackViewBatchPlanTests`, `EditorViewBatchRequestTests`; `RackEditorSessionTests`, `RackInsertionRequestTests` | comportamiento | Core + UI |
| G12 | `RackViewBatchDialogTests`, `RackViewBatchDriverGuardTests`, `RackViewEntryPathGuardTests`; censos y guardas de ventana | UI / guarda | Core + UI |
| G13 | `RackDuplicationPlanTests` + consumo o CT-16 | comportamiento | Core |
| G14 | `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests`, `RackGroupPlacementPlanTests`, `RackSiblingAuthoredGateTests`, `RackSiblingGateIndependenceGuardTests`, `RackViewCountInvariantTests`, `RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests` | comportamiento / guarda | Core |
| G15 | `RackProjectionCommandGuardTests`; censos de comandos y ayuda; `RackUnitsGuardSourceTests` | guarda | Core + UI |
| Todos | T2 suites completas; T3 CI | — | — |
| G4, G5, G7a, G8, G9b, G10, G12, G15, G16 | T4 §OV | — | AutoCAD 2025 |

## OV. Validacion del Owner en AutoCAD 2025 (diseno; no ejecutar)

**Preparacion comun.** DLL Debug del SHA exacto; AutoCAD 2025; biblioteca de bloques del Owner; dibujo en pulgadas; registrar SHA,
version y biblioteca (AGENTS.md) y, opcional, la duracion activa (I-45).

### OV-LEG (G8) y OV-BOM (G7a)

| # | Pasos | Esperado |
|---|---|---|
| OV-LEG-01..06 | Insercion de vista unica y `RACKEDITAR` → Insertar en Selectivo, Dinamico, Push Back compuesto, Cantilever, cabecera y `QUICKCAMA` | Igual que antes de G8 |
| OV-LEG-07 | Esc y Enter en cada jig | No queda definicion con datos de RackCad |
| OV-BOM-01 | `RACKBOMTOTAL` y `RACKLISTA` sobre un dibujo con los seis kinds, incluido un Push Back con diagnostico bloqueante y una linea Cantilever invalida (y un Dinamico solo-sistema si el Owner dispone de uno) | Igual que antes de G7a |

### OV-ID17 (G10)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID17-01 | Selectivo nuevo (2 fondos) → Insertar planta; `RACKEDITAR` → frontal fondo 2 | 1 rack, 2 vistas; mismo diseno desde cualquier vista |
| OV-ID17-02 | Selectivo nuevo → Insertar lateral → `RACKEDITAR` → frontal y planta | 1 rack, 3 vistas |
| OV-ID17-03..05 | Dinamico nuevo con frontal salida, frontal entrada o planta primero → `RACKEDITAR` → lateral | 1 rack por caso |
| OV-ID17-06 | `RACKCABECERA` → planta primero → `RACKEDITAR` → lateral | 1 cabecera, 2 vistas |
| OV-ID17-07 | Push Back y Cantilever: cada vista como primera | Sin regresion |
| OV-ID17-08 | Esc o Enter en el jig de una primera vista no frontal | Nada queda |
| OV-ID17-09 | Guardar, cerrar, reabrir → `RACKBOMTOTAL` | Cantidades de 1 rack |
| OV-ID17-10 | Rack legado con `Id` en blanco (si existe) → `RACKEDITAR` → Insertar | Se cura como hoy; la vista nueva comparte el id y sus propiedades |

### OV-ID18 (G12) y OV-RED (G9b vista unica; OV-RED-05 en G12)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID18-01 | Selectivo nuevo → lote frontal fondo 1 + lateral poste 2 + planta | Tres prompts con nombre y posicion; 1 rack, 3 vistas |
| OV-ID18-02 / 03 | Esc en la segunda / Enter en la ultima | Solo frontal / frontal y lateral; mensaje de cola parcial |
| OV-ID18-04 / 05 | `RACKEDITAR` desde la lateral → Actualizar; guardar, reabrir, editar desde la planta | Las tres cambian; mismo diseno |
| OV-ID18-06 | `U` tras un lote | Registrar lo observado (R-07) |
| OV-ID18-07 | Dinamico existente → lote entrada + planta → Esc en la primera colocacion | Redibujo conservado; sin vistas nuevas; el mensaje dice que las vistas existentes se actualizaron y no se inserto ninguna |
| OV-ID18-10 (G9b) | Selectivo existente de 2 fondos → Insertar frontal → Esc en el prompt «que frontal» | Nada modificado: ni redibujo ni vistas nuevas |
| OV-ID18-11 (G9b) | Selectivo cuya unica vista es la frontal del fondo 2 → encoger a 1 fondo → Insertar planta | La planta nueva se coloca y la frontal huerfana desaparece en el mismo paso; `RACKBOMTOTAL` cuenta el rack |
| OV-ID18-08 | Push Back compuesto → frontal A, frontal B y lateral tras una frontera suprimida | Tres vistas; poste real |
| OV-ID18-09 | Cantilever → laterales de dos estaciones + planta | Tres vistas; una regeneracion al final |
| OV-RED-01 | Selectivo con frontal, lateral y planta → `RACKEDITAR` → cambiar el diseno → Insertar **una** vista; repetir en Dinamico, Push Back compuesto, Cantilever y cabecera | En cada kind, las hermanas cambian juntas antes de pedir el punto; despues el jig de la vista nueva |
| OV-RED-02 | Igual que OV-RED-01 → Esc en el jig | Las hermanas quedan actualizadas; ninguna vista nueva |
| OV-RED-03 | Igual que OV-RED-01 con una hermana en una capa bloqueada | «No se inserto ninguna vista y no se aplico ninguna actualizacion a las vistas existentes», con la vista y el remedio; ninguna hermana cambio (si no ocurre, registrar lo observado: la evidencia de rollback es `RackSiblingRedrawRunTests`) |
| OV-RED-04 | Selectivo cuya unica vista propia es la frontal del fondo 2 → encoger a 1 fondo → Insertar planta → Esc en el jig; repetir y colocar | Con Esc, nada cambia y la frontal huerfana sigue; al colocar, la planta aparece y la huerfana desaparece en el mismo paso |
| OV-RED-05 | (G12, lote) Selectivo con F/L/P → cambiar → Insertar dos vistas → colocar la primera → Esc en la segunda; repetir en Dinamico, Push Back compuesto, Cantilever y cabecera | Hermanas actualizadas antes del primer jig; redibujo y primera vista conservados; la segunda no aparece; registrar la duracion (R-17) |

### OV-META (G9b y G12)

| # | Pasos | Esperado |
|---|---|---|
| OV-META-01 | `RACKPROPIEDADES` en un rack → insertar una vista y un lote | Las vistas nuevas tienen las mismas propiedades |
| OV-META-02 | Cotas desactivadas en frontal → lote F + L + P | Cada vista segun su politica |
| OV-META-03 | Rack con vinculo a variable → lote | El vinculo sigue en todas |
| OV-META-04 | Rack cuyas vistas tienen propiedades distintas (si el Owner dispone de uno) → `RACKEDITAR` → Insertar | Rechazo con remedio; nada cambia; Actualizar sigue funcionando; tras unificar con `RACKPROPIEDADES`, Insertar procede |
| OV-META-05 | Dibujo con un bloque RackCad de **otro** rack que este build no interpreta (si el Owner dispone de uno) → `RACKEDITAR` → Insertar en un rack sano | La insercion procede; si no hay bloque disponible, la evidencia es `RackSiblingCustomPropertiesGateTests` |
| OV-META-06 | Rack con propiedades distintas y un bloque no interpretable en el dibujo (si el Owner dispone de ambos) → Insertar → `RACKPROPIEDADES` | Insertar rechaza con el remedio condicionado; `RACKPROPIEDADES` queda en solo lectura y dice la causa |

### OV-ID19 (G15; con el paquete recomendado de M-01; se reescribe si M-01 decide otra cosa)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID19-01 | `RACKLAYOUT` independiente de 1 fila × 4 (columnas a lo largo de la corrida) → proyectar las plantas a Frontal | Frontales lado a lado sobre una base comun, **en orden de R ascendente**, con la separacion entre ejes de poste 0 de las plantas; conteos identicos |
| OV-ID19-02 | Dos filas (2 × 4) → Frontal | Aviso de superposicion **antes de pedir puntos**, con los pares; frontales de filas distintas superpuestas |
| OV-ID19-03 | Filas escalonadas sin tramos comunes → Frontal | **Sin** aviso de superposicion |
| OV-ID19-04 | Las 4 frontales de OV-ID19-01 → Planta | Plantas en orientacion natural (OD-6.d = A), sobre una linea comun, con el orden y la separacion originales |
| OV-ID19-05 | Plantas → **Planta** (misma clase) con destino desplazado | Vistas enlazadas del layout con la orientacion de cada rack; conteos identicos; aviso final de que no son copias |
| OV-ID19-06 | Layout de plantas **girado 30° con `ROTATE`** (la rejilla de `RACKLAYOUT` solo es correcta a 0/90/180/270, `P/RackLayoutCommands.cs:170-171`) → Planta (rigido) y → Frontal (ortografico) | Rigido: layout y giros conservados; ortografico: frontales rectas con la separacion medida a lo largo de la corrida girada |
| OV-ID19-07 | `RACKLAYOUT` enlazado 1 × 4 → Frontal | 4 referencias de una definicion nueva; 1 rack con 4 copias antes y despues (`RACKLISTA`, `RACKBOMTOTAL`) |
| OV-ID19-08 | `RACKEDITAR` sobre una vista proyectada → Actualizar | Todas las vistas del rack cambian |
| OV-ID19-09 | Frontal ↔ Lateral | Con OD-7.a = A no se expone: mensaje sin escribir |
| OV-ID19-10 | Mezclas invalidas: plantas y una frontal; frontal y planta del mismo rack; Cama; MINSERT; escala ≠ 1 | Error explicito **sin pedir puntos** y sin escribir |
| OV-ID19-11 | Vista con `View` desconocido; frontal de un fondo que ya no existe | Error con disposicion (`Coerced` / `Orphaned`) y remedio `RACKEDITAR`; nada escrito |
| OV-ID19-12 | Rack con propiedades divergentes en la seleccion; y aparte un bloque ajeno no interpretable | Lo primero falla con remedio; lo segundo no bloquea |
| OV-ID19-13 | Esc o Enter en punto base o destino | Nada escrito ni importado; Enter no vuelve a pedir el punto |
| OV-ID19-14 | Selectivo, Dinamico y Push Back de longitudes distintas en una corrida → Frontal | Frontales alineadas por el eje del poste 0, en orden de R ascendente |
| OV-ID19-15 | Cantilever y Selectivo con corridas paralelas → Frontal | Con OD-7.d = A: error explicito sin pedir puntos |
| OV-ID19-16 | Rack con variable vinculada → proyectar → cambiar la variable | La vista usa el valor efectivo y se actualiza |
| OV-ID19-17 | Push Back con diagnostico bloqueante | Error que nombra el rack y el motivo |
| OV-ID19-18 | 100 o mas plantas → Frontal | Termina; registrar duracion y numero de racks |
| OV-ID19-19 | `RACKLAYOUT` enlazado con una celda espejada con `MIRROR` → cualquier clase | Error de reflexion sin pedir puntos |
| OV-ID19-20 | Dos plantas del mismo rack (duplicado legal) en la seleccion | Con OD-7.c = A: error que nombra el rack |
| OV-ID19-21 | **Cantilever: plantas → Frontal y → Lateral; frontales → Planta; laterales → Planta**, con un layout de lineas escalonadas en R (para Frontal) y otro escalonado en D (para Lateral) | Cada caso coloca con la separacion y el orden de la fuente sobre K; la ida y vuelta restituye el intervalo de cada linea sobre K (no la coordenada descartada ni las orientaciones) |
| OV-ID19-22 | Rack girado 180° en una fila de plantas → Frontal; despues, girar 180° racks suficientes para ser mayoria → Frontal | Primero: su frontal ocupa el mismo tramo de la corrida que su planta. Despues: el orden de todas las frontales se invierte (Proposal §12.5); registrar si el Owner lo acepta (OM-24) |
| OV-ID19-23 | Dinamico solo-sistema (si el Owner dispone de uno) → proyectar | Se proyecta si su paridad de G3 lo permite; si no, error `UnsupportedLegacy` que nombra el rack y nada escrito |
| OV-ID19-24 | Push Back con una hermana de descriptor invalido (si el Owner dispone de uno) → proyectar | Error que nombra el rack y remite a `RACKEDITAR`; nada escrito |

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
| R-03 | I-52 congela autoridades incompatibles con X-1..X-8 o no adopta la tabla | STOP simetrico → escalar a los Coordinadores; Proposal nueva si hace falta | G3 (freeze), G5..G15 |
| R-04 | Censos en conflicto al integrar | La segunda re-basa | G7b, G12, G15 |
| R-05 | D-15 cambia el caso raro de redibujo fallido: ahora no se aplica ninguna actualizacion | Mensaje `REDRAW_ROLLED_BACK`; pruebas de G9a; guarda | G9a, G9b, G12 |
| R-06 | ID19 con selecciones grandes | Un barrido, una resolucion por `RackId`, una importacion; OV-ID19-18 | G14, G15 |
| R-07 | `U` no agrupa el lote como espera el usuario | OV-ID18-06 | G12 |
| R-08 | D-10 no observable por el Owner | Guarda con teoria de mutacion | G8 |
| R-09 | Bloques de Push Back insertados con H-01 conservan su `Section` | Sin migracion; documentado en el cierre de PR-1 | G4 |
| R-10 | Un builder cambia su marco local o sus tramos | Caracterizacion de marcos | G3, G6 |
| R-11 | Un par o una paridad resultan inconsistentes al caracterizar | Fuera de ID19 hasta Proposal nueva o decision posterior | G3 |
| R-12 | El gate de propiedades rechaza Insertar donde hoy funciona | Motivo y remedio; Actualizar intacto; OV-META-04 | G9b |
| R-13 | M-01 se resuelve con opciones que cambian validacion y OV-ID19 | La foundation no cambia; se reescriben politicas, pruebas de politica y OV-ID19 | G14 |
| R-14 | Un payload no interpretable sin `Id` legible que si pertenece al `RackId` no se detecta | Consecuencia de CR-03; la sonda cubre los que conservan el `Id` | G9b |
| R-15 | El remedio `RACKPROPIEDADES` exige reparar antes un payload ajeno o una unificacion bloqueada | Mensaje condicionado; OV-META-06 | G9b |
| R-16 | La delegacion de los handlers del BOM en `Resolve` cambia un BOM o un motivo de bloqueo | Caracterizacion del BOM en G3 byte a byte; guarda de delegacion; OV-BOM-01 | G7a |
| R-17 | Una transaccion unica para todas las hermanas de un rack grande tarda mas o usa mas memoria que las transacciones por vista | Mismo patron que `ProjectVariableMutationExecutor`; OV-RED-05 registra la duracion | G9a, G9b, G12 |
| R-18 | La paridad de `Resolve` con el editor deja fuera de ID19 casos que el usuario espera | Diagnostico explicito; decision posterior por caso | G3, G14 |
| R-19 | Un sobre legible con claves duplicadas en distinta capitalizacion se lee con la ultima (riesgo compartido con I-54) | Registrado; no es regresion | G9b |
| R-20 | El acoplamiento de X-2 retrasa gates de I-52 si I-55 no ha extraido la pieza | STOP del gate y decision de los Coordinadores (adelantar el gate de I-55 o excepcion registrada) | G6, G7a, G7b |
| R-21 | El sentido por mayoria sorprende al usuario | Consecuencia normativa documentada, prueba y OV-ID19-22; OM-24 | G14, G15 |
| R-22 | Una hermana dependiente de xref queda con el diseno viejo hasta recargar la xref | El informe la nombra; OM-25 | G9b, G12 |
| R-23 | Las guardas de fuentes vigentes de Push Back, Cantilever y unidades fijan la forma del bucle de redibujo | Reapuntarlas con motivo y acotadas a Actualizar en G9b, sin debilitarlas | G9b |
| R-24 | Un envoltorio nuevo construye un plan distinto del de su `RedrawInPlace` | La guarda de G9a exige la misma lambda de plan; las pruebas por kind comparan el descriptor y el payload | G9a |
| R-25 | Tras cancelar el primer jig de un rack sin hermanas que redibujar, falla la limpieza best effort de la definicion nueva y queda una definicion sin referencias con el authored nuevo junto a las huerfanas | Limpieza vigente (D-10) registrada; alternativa futura: crear la definicion dentro de la transaccion del jig con la operacion de crear de X-4 | G9b |
| R-26 (PLAUSIBLE) | Una referencia o entidad en capa bloqueada revierte todo el redibujo en cada Insertar, donde hoy solo se saltaba esa vista | Mensaje con la vista y el remedio; sin caracterizacion automatica posible sin AutoCAD; OV-RED-03 | G9b |

## F. Archivos y simbolos

### F.1 Produccion nueva (propuesta)

| Ruta | Gate |
|---|---|
| `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDisposition.cs`, `RackViewAddressCodec.cs`, `RackViewAvailability.cs`, `RackViewSupport.cs`, `RackViewFrame.cs` | G6 |
| `A/Views/Resolution/RackSystemResolution.cs`, `RackSystemResolver.cs` | G7a |
| `A/Views/Preparation/*` (incluidos `RackViewNameSeed.cs`, `RackViewPlanner.cs`), `A/Drawing/LibraryBlockRequirement.cs` (primer gate con I-52 G6) | G7b |
| `P/Views/RackViewPlacement.cs`; consulta de la tabla de bloques tras importar en `P/Systems/Shared/` (primer gate con I-52 G6) | G8 |
| `A/Views/Redraw/RackSiblingRedrawPlan.cs`, `A/Views/Redraw/RackSiblingRedrawRun.cs` (con su puerto); `P/Systems/Shared/SiblingRedrawUnits.cs`, `P/Systems/Shared/SiblingRedrawTransaction.cs` | G9a |
| `A/Views/RackSiblingCustomPropertiesGate.cs`, `A/Views/RackEnvelopeIdProbe.cs`; lectura de entradas del gate y de la clasificacion en el Plugin | G9b |
| `A/Views/RackViewBatchPlan.cs` | G11 |
| `U/Views/RackViewBatchDialog.xaml(.cs)`, `U/Views/RackViewBatchDialogPresenter.cs`, `P/Views/RackViewBatchDriver.cs` | G12 |
| `A/Views/Placement/*` (`SourceGroupFrame`, `TargetGroupFrame`, `CommonTransform2D`, `RackGroupAnchorPolicy`, `RackGroupPlacementPlan`), `A/Views/RackSiblingAuthoredGate.cs`; consumo o creacion (primer gate con I-52 G4) del valor de colocacion, la descomposicion de la fuente y la tolerancia de escala en `A/Geometry/` | G14 |
| `P/RackProyectarCommands.cs` | G15 |

### F.2 Produccion modificada

| Ruta | Gate |
|---|---|
| `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | G4, G6, G12 |
| `P/RackCantileverCommands.cs` | G5, G8, G9b, G12 |
| `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs` | G6 |
| `P/KindHandlers/SelectiveKindHandler.cs`, `DynamicKindHandler.cs`, `PushBackKindHandler.cs`, `CantileverKindHandler.cs`, `CabeceraKindHandler.cs`, `CamaKindHandler.cs` | G7a |
| `P/Drawing/BlockPlacement.cs`; `P/Systems/Shared/ViewBlockDraw.cs` y `P/Drawing/LateralHeaderDrawService.cs` (firmas de `DrawAndPlace` para el gancho de huerfanas); `DrawAndPlace` de `P/Systems/Selective/SelectiveFrontalDrawService.cs`, `SelectivePlantaDrawService.cs`, `P/Systems/Dynamic/*DrawService.cs`, `P/Systems/PushBack/*DrawService.cs` y `P/Drawing/PlantaHeaderDrawService.cs` | G8, G9b, G12 |
| `P/RackCommandSupport.cs` (lectura de la dependencia de xref para la clasificacion; `EraseViewBlocks` sin cambio) | G9b |
| `P/Systems/Dynamic/DynamicSystemDrawService.cs`, `DynamicFrontalDrawService.cs`, `DynamicPlantaDrawService.cs`, `P/Systems/PushBack/PushBackSystemDrawService.cs`, `PushBackFrontalDrawService.cs`, `PushBackPlantaDrawService.cs`, `P/Drawing/PlantaHeaderDrawService.cs` (envoltorios minimos) | G9a |
| `P/RackSelectivoCommands.cs`, `P/RackDinamicoCommands.cs`, `P/RackPushBackCommands.cs`, `P/RackCabeceraCommands.cs` | G8, G9b, G10, G12 |
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
`Id` interior del Cantilever; `DynamicEditorDesignAssembler` y el lote de cabeceras de I-53D; `A/Bom/*` salvo consumo; los builders del BOM;
`P/ProjectVariableMutationExecutor.cs`; `P/Systems/Shared/SystemBlockWriter.cs` (solo llamadas, lista STOP de I-52); `docs/HANDOFF.md` hasta la integracion.

## P. Rendimiento

| Operacion | Coste maximo |
|---|---|
| Catalogo, registro de variables | 1 por comando |
| Barrido de sobres | ID19: 1 (SNAPSHOT, del que salen tambien los gates); edicion: los vigentes + 1 lectura de las entradas del gate por Insertar |
| `Resolve` y serializacion authored | 1 por `RackId` |
| `CommonTransform2D` | 1 por ejecucion (seleccion proyectada) |
| Importacion de bloques | ID19: 1 con la union de claves; verificacion estructural lineal en piezas |
| Transacciones | Insertar en rack existente: 0 o 1 para todo el redibujo de hermanas y el borrado de huerfanas + 2 por vista nueva (definicion y jig) + 1 por renombre cosmetico, ninguna abierta entre jigs; Actualizar: las vigentes; ID19: 1 |

## S. Estado

```text
Implementation Map V4 = NOT CONSENSUS · Open Material = M-01 · adopcion de X-1..X-8 por I-52
Coordinator = REVIEW REQUIRED · Architect formal = PENDING · Owner = PENDING · Consensus = NOT REACHED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
