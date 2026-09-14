# I-55 — Implementation Map V3: View Placement & Projection

```text
PROPOSAL V3 — NOT CONSENSUS
Coordinator      = REVIEW REQUIRED
Architect formal = PENDING
Owner            = PENDING
Consensus        = NOT REACHED
Implementation   = BLOCKED
Open Material    = M-01 (propio) · adopcion de X-1..X-8 por I-52
Coordinador      = CQ-01 pendiente (redibujo de Insertar; Proposal V3 §22.3)

Acompana a     = docs/initiatives/I-55-proposal-v3.md (misma version del plan; ninguno vale sin el otro)
Sustituye a    = docs/initiatives/I-55-implementation-map-v2.md (historico, sin modificar)
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
    └─ G5 (PR-2, H-02) ─┴─ G6 (foundation) ─ G7a (Resolve) ─ G7b (Plan + Prepare) ─ G8 (colocacion) ─ G9 (precondiciones de Insertar)
                                                                                                                     │
    ┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
    └─ G10 (ID17) ─ G11 (ID18 contrato) ─ G12 (ID18 UI) ─┐
                                   G13 (X-1, con acuerdo) ─┴─ G14 (ID19 puro) ─ G15 (ID19 comando) ─ G16
```

| Gate V2 | Gate V3 | Cambio |
|---|---|---|
| G3 | G3 | Anade paridad de `Resolve`, caracterizacion del BOM, roles que requieren bloque, tramos por variante y CT-16 unica |
| G4, G5 | G4, G5 | Sin cambio |
| G6 | G6 | Codec **solo sintactico** + disponibilidad; `RackViewFrame` con `[K_min, K_max]`; pruebas de politica fuera del codec |
| G7 | **G7a** + **G7b** | G7a: `Resolve` compartido y delegacion de los handlers del BOM. G7b: `Plan` con semilla, `Prepare` y requisito estructural (parte pura) |
| G8 | G8 | Anade el reporte estructural en la insercion vigente y precisa D-10 |
| G9 | G9 | Variante antes; PREPARE ALL (con planes de hermanas) antes del redibujo; D-15 con STOP, informe y `Regen`; huerfanas con la primera colocacion |
| G10..G13 | G10..G13 | G11 y G12 con redibujo parcial y Esc/Enter |
| G14 | G14 | Etapa FRAMES; politica ortografica corregida; ocho pares; tramos; superposicion con `ε_ov`; recta comun y sentido por mayoria; INV-GRP-3 e INV-GRP-4; signos de escala; valor de colocacion |
| G15 | G15 | SNAPSHOT completo; verificacion estructural tras importar (X-4) |

| Coordinacion | Gates de I-55 | Contraparte (I-52 V13, `dd45b0f`) | Regla |
|---|---|---|---|
| Reconciliacion obligatoria de las autoridades compartidas | Antes del Consensus Freeze (precondicion de G3) | [V11-D10], [V12-D11], [V13-D08], §14.1, §15.4, R-45 | Propiedad registrada por ambas sin duplicar; STOP simetrico; tabla de adopcion en la Proposal V3 §17.2.1 |
| X-1 nucleo de seleccion | G3 (CT-16), G13 | §5.1, CT-16 (provisionales) | Custodio I-52; primer gate I-52 G4 o I-55 G13; el asignador de identidad queda en I-52; politicas por llamador |
| X-2 taxonomia, codec, disponibilidad, `Resolve`, `Plan` | G3 (CT-04, paridad), G6, G7a, G7b | §3.7, §3.8, §6.1, §8.1, §8.4, §20.1, CT-04 (provisionales en V13) | Custodio I-55 en `A/Views`; **extractor unico I-55** (I-52 excluye `KindHandlers/*` y editores WPF); I-52 consume en G4, G5 y G6; `Build` = `Resolve` + `Plan` |
| X-3 comparador authored por kind | G14 | §5.4, §6.1, §8.5 (provisionales en V13) | En el contrato del kind; el reflector delega; primer gate I-52 G5 o I-55 G14 |
| X-4 primitivo + requisito estructural | G7b (parte pura), G15 | §8.2 (provisional), §8.7 | Primitivo: custodio I-52, I-55 consume. Requisito puro en `A/Drawing/LibraryBlockRequirement.cs`: primer gate I-55 G7b o I-52 G6 |
| X-5 protocolo | G5, G6, G7a, G7b, G8, G9, G10, G12, G13, G14, G15 | §15.2, §15.3, §19 | Comprobacion de existencia y reporte antes de fijar archivos con la lista de re-evaluacion (§8.1, §8.7, `MaterializationContextReadSet`, PRE-17, PRE-18, T-GRD-02, censo `B`); I-52 anade G4 y G6 |
| X-6 juego unico de C-2 | G5, G7b, G8, G9, G12 | §8.5 (provisional) | La segunda en integrar re-establece C-2; fixture de planta Cantilever con visibilidad |
| X-7 valor de colocacion y transformacion fuente | G14, G15 | §3.3, §8.3 (provisionales en V13); §3.2 consumido sin cambio; §4.3 (tolerancia de escala) | Custodio I-52; primer gate I-52 G4 o I-55 G14; un valor, una descomposicion de la fuente y una tolerancia de escala |
| X-8 `RackViewFrame` con tramos | G3 (CT-05), G6, G14 | §6.1 (`c`), §8.1 (tramo), CT-05 (provisionales en V13), CT-06 | Custodio I-55; extractor unico I-55 G6; `c` por el origen; una caracterizacion |
| Archivo caliente del Selectivo | G6, G10, G12 | I-49 G10 | Serializar |
| Censos | G7b, G12, G15 | I-52 (`Compose`, comandos, ayuda) | La segunda en integrar re-basa |

## 2. Gates

### G3 — Caracterizacion (solo pruebas)

| Campo | Contenido |
|---|---|
| Objetivo | Fijar sobre produccion intacta lo que I-55 preserva, cambia o necesita medir |
| Precondiciones | Coordinator = AGREED y Architect formal = AGREED sobre la misma version; **M-01 resuelta**; reconciliacion con I-52 registrada ([V11-D10], [V12-D11], [V13-D08]); Consensus Freeze; ADR-0042 aceptado; OD-1..OD-8 decididas |
| Produccion | Ninguna |
| T/ `RackViewEnvelopeReadingCharacterizationTests` (= CT-04) | `PushBackSystemFrontalBuilder.EncodeSection/DecodeSection/IsValidSection`, `CantileverViewPlanBuilder.SectionFor`, `RackListBuilder` con `View` vacio (`A/Persistence/RackListBuilder.cs:117-118`). Guarda de las lecturas del Plugin con la lista de I-52 V13 §8.4: `P/RackSelectivoCommands.cs:126-177`; `P/RackDinamicoCommands.cs:209-239`, `:392-393`; `P/RackPushBackCommands.cs:218-258`, `:427-450`; `P/RackCantileverCommands.cs:295-314`, `:501-540`; `P/RackCabeceraCommands.cs:239-240`; `P/ProjectVariableMutationExecutor.cs:180-218`; mas, fuera de esa lista, `P/RackSelectivoCommands.cs:216`, `P/RackBlockFinder.cs:23-24` y `P/RackLayoutCommands.cs:71`. Incluye los tokens con espacios y la regla de la peor disposicion (Proposal §7.3 A). Separa lo sintactico (§7.3 A) de lo que depende del sistema resuelto (§7.3 B) |
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

### G9 — Precondiciones nuevas de `RACKEDITAR` → Insertar (vista unica)

| Campo | Contenido |
|---|---|
| Objetivo | Variante antes del gate; gate de propiedades acotado al `RackId` (D-07); PREPARE ALL (vistas nuevas, planes y payloads de hermanas) antes del redibujo; D-15 con STOP, informe y `Regen`; huerfanas con la primera colocacion; Actualizar sin cambio |
| Precondiciones | G8; X-5 reportado; X-6 (cambia a proposito la linea base C2-2b) |
| Produccion nueva | `A/Views/RackSiblingCustomPropertiesGate.cs` (consume `CustomPropertiesStore.ReadElement`, `A/Persistence/CustomPropertiesStore.cs:90`, y `CustomPropertiesCanonicalForm`, `A/CustomProperties/CustomPropertiesCanonicalForm.cs:24-70`); `A/Views/RackEnvelopeIdProbe.cs`; `A/Views/RackSiblingRedrawOutcome.cs` y un ejecutor puro del bucle de redibujo sobre unidades (actualizadas, actualizadas con aviso, fallida, motivo, `Regen`, huerfanas); lectura de entradas del gate en el Plugin (sobre, texto del payload, dependencia de xref), p. ej. `P/Views/RackSiblingScan.cs` |
| Produccion modificada | `P/Drawing/BlockPlacement.cs` y los `DrawAndPlace` de los servicios de dibujo: gancho para borrar huerfanas dentro de la transaccion del jig antes del commit (la purga de anidadas sigue despues del commit). Ramas de insercion de `EditSelective`, `EditDynamic`, `EditPushBack`, `EditCantilever` y `EditCabecera`. Orden: variante (prompt legacy movido antes; Esc → nada modificado) → gate → PREPARE ALL (vista nueva, planes y payloads de hermanas) → redibujo por unidades con STOP en el primer fallo de una hermana propia del dibujo (tambien fuera del `try`; las dependientes de xref, aviso) → `REDRAW_FAILED` con informe y `Regen` si alguna confirmo, o huerfanas (borrado verificado; incompleto → STOP) y colocacion (si las unicas vistas son huerfanas, su borrado va en la transaccion del jig). Blanco = `IsNullOrWhiteSpace` |
| T/ `RackSiblingCustomPropertiesGateTests` | Sin sobre fuente ni miembros → sin gate; el sobre elegido siempre es miembro (incluida la cura de un `Id` en blanco o solo espacios); comun → hereda; misma forma canonica con distinta version menor → comun; divergente → fallo con remedio condicionado; miembro no escribible → fallo; `Kind` en blanco o varios → fallo; dependiente de xref ignorada; payload no interpretable con `Id` igual → fallo; sin `Id` legible o con otro `Id` → no bloquea |
| T/ `RackEnvelopeIdProbeTests` | `Id` de nivel superior sin distinguir mayusculas; `Id` anidado ignorado; duplicados; JSON roto despues o antes del `Id`; MAJOR mas nuevo → legible |
| T/ `RackSiblingRedrawOutcomeTests` | Comportamiento del ejecutor puro con unidades falsas: fallo en la unidad k → informe con 1..k-1 y la fallida, cero colocaciones, ninguna huerfana borrada, `Regen` solo si alguna confirmo; error posterior al commit → «actualizada con aviso»; fallo de una dependiente de xref → aviso sin STOP; todas confirman → huerfanas y cola; borrado de huerfanas incompleto → STOP; rack cuyas unicas vistas son huerfanas → borrado ligado a la transaccion del jig de la primera colocacion; jig cancelado → huerfanas intactas |
| T/ `RackSiblingGateWiringGuardTests` | Guarda con teoria de mutacion: variante antes del gate; gate y PREPARE ALL antes del primer redibujo en cada rama Insertar; nunca en Actualizar; cada rama usa el ejecutor de redibujo; ninguna definicion nueva tras un fallo |
| T/ `RackSiblingGateIndependenceGuardTests` | El gate de propiedades vive en su archivo y no nombra Project Variables ni `RackCustomPropertiesAuthority` (fuera de la clasificacion de `TGrd05`, `T/CustomPropertiesEdgeGuardTests.cs:287-298`); G14 la extiende al gate authored |
| Guardas que deben seguir verdes | `CustomPropertiesEdgeGuardTests.TGrd04_*`, `TGrd05_*`; `RackUnitsGuardSourceTests.EditInsert_IsGatedByUpdateOnly_AndWarnsBeforeTheFirstRedraw` |
| RED | Esqueletos y guardas vistos fallando |
| GREEN | T0 `FullyQualifiedName~RackSiblingCustomPropertiesGate\|FullyQualifiedName~RackEnvelopeIdProbe\|FullyQualifiedName~RackSiblingRedrawOutcome` y T1 `FullyQualifiedName~RackSiblingGate\|FullyQualifiedName~CustomPropertiesEdgeGuard\|FullyQualifiedName~RackUnitsGuardSource`, con conteo; Core + UI; Debug Plugin; **OV-META-04..06**, **OV-RED-01** |
| Impacto | Insertar en un rack con propiedades divergentes o ilegibles falla con motivo; el prompt de variante llega antes del redibujo; un redibujo fallido detiene la insercion con informe y sin rollback de lo confirmado; las huerfanas ya no quedan tras insertar (R-05, R-12, R-15, R-17, R-19) |

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
| Objetivo | D-06 y D-15 sin UI |
| Precondiciones | G10 |
| Produccion | `A/Views/RackViewBatchPlan.cs`: orden; estados `VARIANT_CANCELLED`, `SIBLING_GATE_FAILED`, `ABORTED_BEFORE_WRITE` y `REDRAW_FAILED` con informe; cola con `CANCELLED_BEFORE_ANY` y `FAILED_BEFORE_ANY`; Esc y Enter; `PromptStatus.Error` como fallo; mensajes deterministas. `U/Editor/RackEditorSession.cs` (`RequestInsertViews`); `U/Editor/RackInsertionRequest.cs` (`Views`; historicos = primera direccion) |
| Pruebas | T/ `RackViewBatchPlanTests`: maquina completa; mensajes exactos (en un rack existente, con los redibujos confirmados); Esc y Enter detienen; Error falla; `REDRAW_FAILED` sin colocaciones y con actualizadas y fallida; `ABORTED_BEFORE_WRITE` si falla el plan o la composicion de una hermana o la variante no esta disponible. TU/ `EditorViewBatchRequestTests`: una aceptacion → un `RackId`. Reapuntadas: `RackEditorSessionTests`, `RackInsertionRequestTests` |
| RED | Vistas fallando |
| GREEN | T0 `FullyQualifiedName~RackViewBatch\|FullyQualifiedName~EditorViewBatch` con conteo; Core + UI |
| Impacto | Codigo nuevo sin consumidores en el Plugin |

### G12 — ID18: dialogo, presentador, driver y rutas de entrada

| Campo | Contenido |
|---|---|
| Objetivo | ID18 de extremo a extremo |
| Precondiciones | G11; serializado con I-49 G10; X-5 reportado; X-6 |
| Produccion nueva | `U/Views/RackViewBatchDialog.xaml(.cs)` (arquetipo C), `U/Views/RackViewBatchDialogPresenter.cs` (fuera de los archivos de ventana), `P/Views/RackViewBatchDriver.cs` (variantes → gate → PREPARE ALL → redibujo con el ejecutor de G9 → `REDRAW_FAILED` o huerfanas y cola; `Regen` segun la Proposal §11) |
| Produccion modificada | Ventanas con vistas y configurador; `U/Editor/EditorModules.cs` (`:53-56`, `:90-95` reenvian `Views`); `RACKSELECTIVO` (`P/RackSelectivoCommands.cs:25-45`); cabecera (un acunado por intencion); comandos de creacion; `P/RackMenuCommands.cs`; ramas `Edit*` |
| Pruebas | TU/ `RackViewBatchDialogTests`; T/ `RackViewBatchDriverGuardTests` (gate y PREPARE ALL antes de escribir; STOP y ninguna definicion nueva tras `REDRAW_FAILED`; Esc y Enter detienen), `RackViewEntryPathGuardTests` (toda ruta reenvia `Views` y acuna una vez); reapuntadas: `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `EditorModuleRegistryTests`, `RackUnitsGuardSourceTests`, `CantileverPluginSourceGuardTests.ExactlyOneRegenForTheWholeBatch_AndItAgreesWithWhatIsReported` |
| RED | Vistas fallando |
| GREEN | Focales con conteo; Core + UI; Debug UI + Plugin; **OV-ID18**, **OV-META-01..03** |
| Impacto | Boton nuevo en cinco editores |

### G13 — Nucleo neutral de seleccion (X-1)

| Campo | Contenido |
|---|---|
| Objetivo | Nucleo neutral disponible sin cambiar `RACKDUPLICAR` |
| Precondiciones | Reconciliacion X-1; X-5 reportado; coordinacion con I-49 antes de editar `A/Persistence/RackDuplicationPlan.cs` |
| Si I-52 ya lo extrajo | Consumirlo; T/ `RackSelectionCoreConsumptionTests` |
| Si no | Extraer con CT-16 (G3) y la fachada verde, sobre la especificacion de I-52 V13 §5.1; pruebas de I-51 sin cambios |
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
| G9 | `RackSiblingCustomPropertiesGateTests`, `RackEnvelopeIdProbeTests`, `RackSiblingRedrawOutcomeTests`; `RackSiblingGateWiringGuardTests`, `RackSiblingGateIndependenceGuardTests`; `CustomPropertiesEdgeGuardTests`; `RackUnitsGuardSourceTests` | comportamiento / guarda | Core |
| G10 | `FirstViewFreedomTests`; guardas de modales y shell | UI / guarda | UI |
| G11 | `RackViewBatchPlanTests`, `EditorViewBatchRequestTests`; `RackEditorSessionTests`, `RackInsertionRequestTests` | comportamiento | Core + UI |
| G12 | `RackViewBatchDialogTests`, `RackViewBatchDriverGuardTests`, `RackViewEntryPathGuardTests`; censos y guardas de ventana | UI / guarda | Core + UI |
| G13 | `RackDuplicationPlanTests` + consumo o CT-16 | comportamiento | Core |
| G14 | `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests`, `RackGroupPlacementPlanTests`, `RackSiblingAuthoredGateTests`, `RackSiblingGateIndependenceGuardTests`, `RackViewCountInvariantTests`, `RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests` | comportamiento / guarda | Core |
| G15 | `RackProjectionCommandGuardTests`; censos de comandos y ayuda; `RackUnitsGuardSourceTests` | guarda | Core + UI |
| Todos | T2 suites completas; T3 CI | — | — |
| G4, G5, G7a, G8, G9, G10, G12, G15, G16 | T4 §OV | — | AutoCAD 2025 |

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

### OV-ID18 (G12) y OV-RED (G9)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID18-01 | Selectivo nuevo → lote frontal fondo 1 + lateral poste 2 + planta | Tres prompts con nombre y posicion; 1 rack, 3 vistas |
| OV-ID18-02 / 03 | Esc en la segunda / Enter en la ultima | Solo frontal / frontal y lateral; mensaje de cola parcial |
| OV-ID18-04 / 05 | `RACKEDITAR` desde la lateral → Actualizar; guardar, reabrir, editar desde la planta | Las tres cambian; mismo diseno |
| OV-ID18-06 | `U` tras un lote | Registrar lo observado (R-07) |
| OV-ID18-07 | Dinamico existente → lote entrada + planta → Esc en la primera colocacion | Redibujo conservado; sin vistas nuevas; el mensaje lo dice |
| OV-ID18-10 | Selectivo existente de 2 fondos → Insertar frontal → Esc en el prompt «que frontal» | Nada modificado: ni redibujo ni vistas nuevas |
| OV-ID18-11 | Selectivo cuya unica vista es la frontal del fondo 2 → encoger a 1 fondo → Insertar planta | La planta nueva se coloca y la frontal huerfana desaparece en el mismo paso; `RACKBOMTOTAL` cuenta el rack |
| OV-ID18-08 | Push Back compuesto → frontal A, frontal B y lateral tras una frontera suprimida | Tres vistas; poste real |
| OV-ID18-09 | Cantilever → laterales de dos estaciones + planta | Tres vistas; una regeneracion al final |
| OV-RED-01 | Solo si se puede provocar un fallo real de redibujo (p. ej. una hermana cuyo redibujo falla por datos): `RACKEDITAR` → Insertar | Ninguna vista nueva; mensaje con las hermanas actualizadas y la que fallo, sin proponer Actualizar como remedio; si no es reproducible, la evidencia es `RackSiblingRedrawOutcomeTests` y `RackSiblingGateWiringGuardTests` |

### OV-META (G9 y G12)

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
| R-05 | D-15 cambia el caso raro de redibujo fallido | Mensaje con informe; guarda | G9, G12 |
| R-06 | ID19 con selecciones grandes | Un barrido, una resolucion por `RackId`, una importacion; OV-ID19-18 | G14, G15 |
| R-07 | `U` no agrupa el lote como espera el usuario | OV-ID18-06 | G12 |
| R-08 | D-10 no observable por el Owner | Guarda con teoria de mutacion | G8 |
| R-09 | Bloques de Push Back insertados con H-01 conservan su `Section` | Sin migracion; documentado en el cierre de PR-1 | G4 |
| R-10 | Un builder cambia su marco local o sus tramos | Caracterizacion de marcos | G3, G6 |
| R-11 | Un par o una paridad resultan inconsistentes al caracterizar | Fuera de ID19 hasta Proposal nueva o decision posterior | G3 |
| R-12 | El gate de propiedades rechaza Insertar donde hoy funciona | Motivo y remedio; Actualizar intacto; OV-META-04 | G9 |
| R-13 | M-01 se resuelve con opciones que cambian validacion y OV-ID19 | La foundation no cambia; se reescriben politicas, pruebas de politica y OV-ID19 | G14 |
| R-14 | Un payload no interpretable sin `Id` legible que si pertenece al `RackId` no se detecta | Consecuencia de CR-03; la sonda cubre los que conservan el `Id` | G9 |
| R-15 | El remedio `RACKPROPIEDADES` exige reparar antes un payload ajeno o una unificacion bloqueada | Mensaje condicionado; OV-META-06 | G9 |
| R-16 | La delegacion de los handlers del BOM en `Resolve` cambia un BOM o un motivo de bloqueo | Caracterizacion del BOM en G3 byte a byte; guarda de delegacion; OV-BOM-01 | G7a |
| R-17 | Un rack queda redibujado en parte tras `REDRAW_FAILED` y el usuario no lo nota | Informe exacto que pide corregir la causa; OM-3; CQ-01 | G9, G12 |
| R-18 | La paridad de `Resolve` con el editor deja fuera de ID19 casos que el usuario espera | Diagnostico explicito; decision posterior por caso | G3, G14 |
| R-19 | Un sobre legible con claves duplicadas en distinta capitalizacion se lee con la ultima (riesgo compartido con I-54) | Registrado; no es regresion | G9 |
| R-20 | El acoplamiento de X-2 retrasa gates de I-52 si I-55 no ha extraido la pieza | STOP del gate y decision de los Coordinadores (adelantar el gate de I-55 o excepcion registrada) | G6, G7a, G7b |
| R-21 | El sentido por mayoria sorprende al usuario | Consecuencia normativa documentada, prueba y OV-ID19-22; OM-24 | G14, G15 |
| R-22 | Una hermana dependiente de xref queda con el diseno viejo hasta recargar la xref | Aviso en el informe; OM-25 | G9, G12 |

## F. Archivos y simbolos

### F.1 Produccion nueva (propuesta)

| Ruta | Gate |
|---|---|
| `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDisposition.cs`, `RackViewAddressCodec.cs`, `RackViewAvailability.cs`, `RackViewSupport.cs`, `RackViewFrame.cs` | G6 |
| `A/Views/Resolution/RackSystemResolution.cs`, `RackSystemResolver.cs` | G7a |
| `A/Views/Preparation/*` (incluidos `RackViewNameSeed.cs`, `RackViewPlanner.cs`), `A/Drawing/LibraryBlockRequirement.cs` (primer gate con I-52 G6) | G7b |
| `P/Views/RackViewPlacement.cs`; consulta de la tabla de bloques tras importar en `P/Systems/Shared/` (primer gate con I-52 G6) | G8 |
| `A/Views/RackSiblingCustomPropertiesGate.cs`, `A/Views/RackEnvelopeIdProbe.cs`, `A/Views/RackSiblingRedrawOutcome.cs` y el ejecutor puro del bucle de redibujo; lectura de entradas del gate en el Plugin | G9 |
| `A/Views/RackViewBatchPlan.cs` | G11 |
| `U/Views/RackViewBatchDialog.xaml(.cs)`, `U/Views/RackViewBatchDialogPresenter.cs`, `P/Views/RackViewBatchDriver.cs` | G12 |
| `A/Views/Placement/*` (`SourceGroupFrame`, `TargetGroupFrame`, `CommonTransform2D`, `RackGroupAnchorPolicy`, `RackGroupPlacementPlan`), `A/Views/RackSiblingAuthoredGate.cs`; consumo o creacion (primer gate con I-52 G4) del valor de colocacion, la descomposicion de la fuente y la tolerancia de escala en `A/Geometry/` | G14 |
| `P/RackProyectarCommands.cs` | G15 |

### F.2 Produccion modificada

| Ruta | Gate |
|---|---|
| `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | G4, G6, G12 |
| `P/RackCantileverCommands.cs` | G5, G8, G9, G12 |
| `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs` | G6 |
| `P/KindHandlers/SelectiveKindHandler.cs`, `DynamicKindHandler.cs`, `PushBackKindHandler.cs`, `CantileverKindHandler.cs`, `CabeceraKindHandler.cs`, `CamaKindHandler.cs` | G7a |
| `P/Drawing/BlockPlacement.cs` | G8, G9, G12 |
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
`Id` interior del Cantilever; `DynamicEditorDesignAssembler` y el lote de cabeceras de I-53D; `A/Bom/*` salvo consumo; los builders del BOM;
`P/ProjectVariableMutationExecutor.cs`; `docs/HANDOFF.md` hasta la integracion.

## P. Rendimiento

| Operacion | Coste maximo |
|---|---|
| Catalogo, registro de variables | 1 por comando |
| Barrido de sobres | ID19: 1 (SNAPSHOT, del que salen tambien los gates); edicion: los vigentes + 1 lectura de las entradas del gate por Insertar |
| `Resolve` y serializacion authored | 1 por `RackId` |
| `CommonTransform2D` | 1 por ejecucion (seleccion proyectada) |
| Importacion de bloques | ID19: 1 con la union de claves; verificacion estructural lineal en piezas |
| Transacciones | ID18: redibujo por unidad legacy (CD2-03; CQ-01) + 1 por colocacion, ninguna abierta entre jigs; ID19: 1 |

## S. Estado

```text
Implementation Map V3 = NOT CONSENSUS · Open Material = M-01 · adopcion de X-1..X-8 por I-52 · CQ-01 del Coordinador
Coordinator = REVIEW REQUIRED · Architect formal = PENDING · Owner = PENDING · Consensus = NOT REACHED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
