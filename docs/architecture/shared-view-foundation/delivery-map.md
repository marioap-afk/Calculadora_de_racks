# Shared View Foundation — Mapa de entrega (borrador)

```text
FOUNDATION DELIVERY MAP — DRAFT — NOT CLAIMED — NOT CONSENSUS
FOUNDATION_ID   = PENDING (Owner)
Acompana a      = specification.md y reconciliation.md (mismo commit)
Base            = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
Implementation  = BLOCKED (no existe la iniciativa)
```

> Este mapa **no autoriza** ningun gate. Lo adoptara, reexpresado, la Proposal de la iniciativa de la foundation cuando el Owner la
> autorice. Convenciones de RED/GREEN, apertura y rollback: las de los mapas de iniciativa (I-55 mapa V5 §0). Ningun gate migra datos.

## 1. Secuencia

```text
F0 reclamo + bootstrap ─ (Discovery breve + Proposal + consenso + ADR de foundation aceptado) ─ F1 caracterizacion
   ─ F2 taxonomia + direccion + codec + hechos del barrido
   ─ F3 disponibilidad + marco/tramo + migracion de decodificaciones
   ─ F4 nucleo de seleccion + valor de colocacion          (serializable con F3)
   ─ F5a Resolve + adaptadores por kind ─ F5b Plan + nombre base + delegacion
   ─ F6 comparador authored + requisito de bloques + consulta
   ─ F7 candidato ─ F8 integracion en main ─ artefacto R(n+1) con Integration SHA
   ─ I-52 rebasa · I-55 rebasa
```

## 2. Gates

### F0 — Autorizacion, reclamo y bootstrap

| Campo | Contenido |
|---|---|
| Objetivo | Existir como iniciativa segun WORKFLOW |
| Precondiciones | **Owner:** autoriza (WORKFLOW §2 caso d, o planifica la fila con un arreglo documental en `main`), asigna `FOUNDATION_ID`, slug y prefijo (propuesto `architecture/`) |
| Pasos | Rama y worktree desde `origin/main`; commit de reclamo vacio con ID, `Claim-Id` UUID y trailer de agente; **primer push remoto aceptado** (`git push -u`, sin force) constituye el reclamo; si existe, STOP sin apropiarselo; bootstrap inmediato: contrato desde `docs/initiatives/TEMPLATE.md`, fila de ROADMAP (momento 2), `docs/automation/decisions/<ID>.md` |
| Documentos | La Proposal de la foundation **reexpresa** `specification.md` citando la ruta y el SHA de I-55 donde se publico; no se copia codigo ni se hace cherry-pick |
| Reconciliacion | Publica `R1` con `Claim SHA` de las autoridades aceptadas por I-52 e I-55 y las CR-SVF resueltas; I-52 e I-55 registran el mismo SHA |
| Salida | `R1` EFFECTIVE (o MATERIAL CONFLICT escalado) |

### Consenso de la foundation (entre F0 y F1)

Discovery breve sobre `main` (reutiliza la evidencia de I-55 e I-52 citada por SHA), Proposal, veredictos de su Coordinador y de un
Arquitecto independiente, ADR de foundation aceptado por el Owner (numero asignado al congelar). **No** requiere M-01, OD-1..OD-8,
ADR-0042 ni decisiones de `RACKMIRROR`.

### F1 — Caracterizacion (solo pruebas)

| Campo | Contenido |
|---|---|
| Objetivo | Fijar sobre produccion intacta todo lo que la foundation debe preservar |
| Precondiciones | Consenso + freeze de la foundation; ADR de foundation aceptado; `Rn` EFFECTIVE por ambos registros del mismo SHA; CR-SVF-01/02 resueltas |
| Produccion | Ninguna |
| T/ `RackViewEnvelopeReadingCharacterizationTests` (**CT-04**) | Codec total: producto kind × forma de `View` × particion de `Section` con una fila por celda; lecturas del Plugin (`P/RackSelectivoCommands.cs:126-177`, `:216`; `P/RackDinamicoCommands.cs:209-239`, `:392-393`; `P/RackPushBackCommands.cs:218-258`, `:427-450`; `P/RackCantileverCommands.cs:295-315`, `:500-540`; `P/RackCabeceraCommands.cs:239-243`; `P/ProjectVariableMutationExecutor.cs:180-218`; `P/RackBlockFinder.cs:23-24`; `P/RackLayoutCommands.cs:71`) y de `RACKLISTA` (`A/Persistence/RackListBuilder.cs:105-128`); frontal legacy del Selectivo con `−1` frente a la geometria que produce `FondoSystemView(0)` (FQ-06) |
| T/ `RackViewFrameCharacterizationTests` (**CT-05**) | Ejes con signo, origen, desplazamiento por variante, `[K_min, K_max]` por variante, convencion de extremos, centro, envolvente dibujada; `anchorOffset` de esquina; cuadricula de fondo como prefijo de la maestra; Push Back compuesto; Dinamico sin postes de frontera; cabecera; Cantilever con origen D interior |
| T/ `RackDuplicationPlanObservableSemanticsTests` (**CT-16**) | Semantica observable de `RackDuplicationPlan.Build` sobre codigo intacto (coordinada con I-49) |
| T/ `HeadlessResolutionParityCharacterizationTests` (CT-RES) | Por kind: payload persistido frente a lo dibujado al insertar y a lo que abre el editor; fixtures legacy (Dinamico solo-sistema, cabecera legacy); lista cerrada de formas; precedencia por kind; ¿produce sistema en cada caso bloqueante? (FQ-04) |
| T/ `BomResolutionCharacterizationTests` (CT-RES) | Una llamada efectiva Selectiva por rack en BuildBom, ninguna en preflight; BOM y motivos vigentes de los seis handlers, incluidas las dos pasadas de `RACKBOMTOTAL` con el mismo snapshot (`P/RackInventarioCommands.BomTotal.cs:147-215`) |
| T/ `RackViewPlanCharacterizationTests` (CT-PLAN) | Plan de cada direccion de los seis kinds igual a la salida de builders y lambdas vigentes, incluida la fusion de largueros y el prefijo leido del dibujo en un redibujo lateral |
| T/ `LegacyViewBlockNameCharacterizationTests` (CT-NAME) | Todas las funciones de nombre base cadena a cadena (spec §3.11), incluida H-15 y la idempotencia de `Sanitize` sobre el nombre de insercion del Cantilever |
| T/ `LibraryBlockRequirementCharacterizationTests` + `BuilderCatalogSkipCensusTests` (CT-BLK) | Roles con bloque en la salida real; `Pallet` (FQ-05); censo de omisiones antes del plan; claves con punto de `assets/catalogs/blocks.csv:16-17` |
| T/ `RackSiblingScanInputCharacterizationTests` (CT-SCAN) | `ScanEnvelopes` omite xref pero no dependientes, cuenta referencias desde el registro (`P/RackBlockFinder.cs:57-91`); `FindRackBlocks` sin dependencia ni texto (`P/RackCommandSupport.cs:109-134`) |
| T/ `PlacementValueCharacterizationTests` (CT-GEO) | `Transform2D` vigente: composicion, `RotationAngle` en ±π, determinante y escala |
| T/ `AuthoredComparatorCharacterizationTests` (CT-AUTH) | `SelectiveAuthoredAuthority`; comparacion de diseños por kind excluyendo la metadata interior (`P/RackCommandSupport.cs:40-67`) |
| RED | Caracterizaciones verdes sobre codigo intacto; cada guarda demuestra deteccion por teoria de mutacion |
| GREEN | Clases nuevas con conteo; Core + UI; CI 4/4 |
| Si una caracterizacion contradice la especificacion | Gana la caracterizacion; la especificacion se corrige en una version nueva antes de F2 (nunca se ajusta el codigo a la tabla) |

### F2 — Taxonomia, direccion, codec total y hechos del barrido

| Campo | Contenido |
|---|---|
| Objetivo | AUTH-01, AUTH-02, AUTH-03 y AUTH-06 (parte sintactica), sin cambio observable |
| Precondiciones | F1; serializado con I-49 G10 (`RackSelectiveWindow.xaml.cs`); X-5 reportado a I-52 e I-55 |
| Produccion nueva | `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDisposition.cs`, `RackViewAddressCodec.cs`, `RackViewScanFacts.cs` |
| Produccion modificada (renombre) | `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs`, `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs`, `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`, `U/Systems/Selective/RackSelectiveWindow.xaml.cs` |
| Pruebas | T/ `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewAddressCodecTests` (reproduce CT-04; **prueba de totalidad** sobre el producto del dominio; ida y vuelta `Decode(Encode(a))`; ninguna prueba consulta un sistema resuelto), `RackViewScanFactsTests` (ambos predicados de blanco como hechos; cuenta de referencias; dependencia de xref) |
| RED / GREEN | Esqueleto y pruebas vistos fallando / T0 `FullyQualifiedName~RackView` y T1 `FullyQualifiedName~DimensionView` con conteo; Core + UI; Debug UI |
| Impacto | Ninguno observable |

### F3 — Disponibilidad, marco y migracion de decodificaciones

| Campo | Contenido |
|---|---|
| Objetivo | AUTH-04, AUTH-05 y AUTH-06 (con sistema); los caminos que decodifican vistas consumen el clasificador sin cambiar transacciones |
| Precondiciones | F2 |
| Produccion nueva | `A/Views/RackViewAvailability.cs` (solo `Available`, `VariantNotPresent`, `SystemDoesNotSupportKind`, `Unavailable`), `A/Views/RackViewFrame.cs` (con `Center` y `VariantOffset`) |
| Produccion modificada | Decodificacion de vistas en los bucles de `EditSelective`, `EditDynamic`, `EditPushBack`, `EditCantilever`, `EditCabecera` (Actualizar e Insertar) y en `P/ProjectVariableMutationExecutor.cs:180-218`: consumen `RackViewAddressCodec` y `RackViewAvailability`; la politica (borrar fantasma, redibujar el lado B como A, abortar ante `Invalid`) queda en cada comando sin cambio |
| Pruebas | T/ `RackViewAvailabilityTests` (sin politica de consumidor: no hay `Unsupported` ni canonica de ID19), `RackViewFrameTests` (oraculo = fixtures de CT-05), T/ `ViewDecodeMigrationGuardTests` (guarda de fuentes: ningun comando conserva una decodificacion propia; lista de productores legacy vacia o con prueba diferencial), `ViewDecodeDifferentialTests` para cualquier camino que no migre |
| Guardas reapuntadas con motivo | Las que fijan el texto de los bucles de redibujo (`T/PushBackRoundTripSourceGuardTests.cs:235-258`, `:349-358`; `T/CantileverPluginSourceGuardTests.cs:140-155`, `:263-271`), sin debilitarlas |
| GREEN | Focales con conteo; CT-04 y CT-05 sin cambio; Core + UI; Debug Plugin; **OV-FND-01** (Actualizar e Insertar en los cinco kinds igual que antes) |
| Impacto | Ninguno observable (SVF-RK1) |

### F4 — Nucleo de seleccion y valor de colocacion

| Campo | Contenido |
|---|---|
| Objetivo | AUTH-07 y AUTH-08 |
| Precondiciones | F2; coordinacion con I-49 antes de editar `A/Persistence/RackDuplicationPlan.cs`; **CR-SVF-01 resuelta** (tolerancia de escala registrada) o STOP; CR-SVF-02 resuelta antes de F1 (AUTH-07/08 y CT-16 en la entrega neutral; desacuerdo bloquea, no elimina F4) |
| Produccion | `A/Persistence/RackDuplicationPlan.cs` dividido en nucleo neutral + fachada con API y mensajes identicos; `A/Geometry/PlacementValue.cs`, `SourceTransformFacts.cs`, `AngleMath.cs` |
| Pruebas | T/ `RackSelectionCoreTests` (hechos neutrales, predicado y mensajes inyectados), `RackDuplicationPlanTests` sin cambios (25 hechos y teorias), CT-16 verde antes y despues; T/ `PlacementValueTests` (composicion/descomposicion; `NormalizePi(−π) = π`; ocho combinaciones de signo de escala clasificadas; tolerancia registrada) |
| GREEN | `FullyQualifiedName~RackDuplicationPlan\|FullyQualifiedName~RackSelectionCore\|FullyQualifiedName~PlacementValue` con conteo; Core + UI |
| Impacto | Ninguno observable en `RACKDUPLICAR` |

### F5a — Contrato `Resolve` y adaptadores por kind

| Campo | Contenido |
|---|---|
| Objetivo | AUTH-09 sin cambio observable y sin reabrir ADR-0034 (spec §3.9.3) |
| Precondiciones | F3 |
| Produccion nueva | `A/Views/Resolution/RackResolveRequest.cs`, `RackResolveResult.cs`, `RackLegacyShape.cs` y `RackPersistedShapeClassifier.cs` (lista cerrada, clasificacion por payload independiente de Resolve), `IRackSystemResolver.cs`; `P/KindHandlers/IRackKindSystemResolver.cs` (interfaz hermana, sin nombres de kind); `P/Views/KindHandlerSystemResolver.cs` |
| Produccion modificada | Los seis `P/KindHandlers/*KindHandler.cs`: `ResolveSystem(...)`; Selectivo lo usa en `BuildBom` y mantiene `OutputBlockedReason => null`; los otros kinds conservan sus llamadas actuales; en `SelectiveKindHandler.cs` sigue habiendo **una** construccion de `SelectiveEffectiveDesignResolver` |
| Pruebas | T/ `RackResolveContractTests` (con puerto falso: resultados, precedencia, diagnosticos, formas legacy, contexto con registro ausente o ilegible, catalogo de secciones no disponible), T/ `KindHandlerSystemResolverGuardTests` (guarda de fuentes con teoria de mutacion) |
| Guardas que deben seguir verdes sin cambio | `SelectiveBomAuthorityTests` completo (en particular `:403-408`); `PushBackRoundTripSourceGuardTests.cs:90-186`, `:318-345` (o reapunte con motivo de una cadena exacta, sin debilitar); `CantileverPluginSourceGuardTests.cs:234-283`; `PushBackBomCommandGuardTests.cs:50-66`; `PushBackCellConfigurationDeliveryTests.cs:275-288`; `SelectiveDuplicationFailClosedTests.cs:307`, `:313`; `ProjectVariableMutationExecutorTests.cs:319`; CT-RES byte a byte |
| GREEN | Focales con conteo; Core + UI; Debug Plugin; **OV-FND-02** (`RACKBOMTOTAL` y `RACKLISTA` identicos con los seis kinds, un Push Back bloqueado y una linea Cantilever invalida) |
| Impacto | Ninguno observable (SVF-RK2) |

### F5b — Autoridad de `Plan`, nombre base y delegacion

| Campo | Contenido |
|---|---|
| Objetivo | AUTH-10 y AUTH-11; una sola autoridad de plan y de nombre base |
| Precondiciones | F5a |
| Produccion nueva | `A/Views/Planning/RackViewPlanAuthority.cs` (y contexto/resultado con marco, requisitos y nombre sugerido), `A/Views/Naming/RackViewBaseNames.cs`; `A/Drawing/LibraryBlockRequirement.cs` y roles puros de AUTH-12 (necesarios para devolver Plan) |
| Produccion modificada | Lambdas de plan de `P/Systems/Selective/*DrawService.cs`, `P/Systems/Dynamic/*DrawService.cs`, `P/Systems/PushBack/*DrawService.cs`, `P/Drawing/PlantaHeaderDrawService.cs`, `P/Systems/FlowBed/FlowBedDrawService.cs`, `P/Drawing/LateralHeaderDrawService.cs` (con `Merge` movido a Application), plan Cantilever en `P/RackCantileverCommands.cs`: **delegan**; helpers `BlockName`/`FrontalName`/`ViewBlockName` y concatenaciones de nombres de los comandos: delegan; insercion Cantilever por `CreateBlockDefinitionNamed` con el nombre de la autoridad; solo helpers `SuggestName`/`Sanitize` de `CantileverViewMaterializer.cs` delegan o se retiran (materializacion intacta) |
| Pruebas | T/ `RackViewPlanAuthorityTests` (reproduce CT-PLAN; el marco devuelto coincide con CT-05), `RackViewBaseNamesTests` (reproduce CT-NAME cadena a cadena), T/ `PlanAuthorityDelegationGuardTests` (ningun servicio construye planes fuera de la autoridad; registro de productores legacy vacio), `BaseNameAuthorityGuardTests` (ningun helper conserva logica de nombre; el Cantilever no reaplica `SuggestName`) |
| Guardas reapuntadas con motivo | `CustomPropertiesEnvelopeGuardTests.TGrd02_*` si cambia el censo de `Compose` (no deberia: `Plan` no compone sobres); `CallerOwnedFacadeGuardTests` (las fachadas siguen caller-owned) |
| GREEN | Focales con conteo; CT-PLAN y CT-NAME sin cambio; Core + UI; Debug Plugin; **OV-FND-03** (insertar y redibujar en los seis kinds: mismos nombres y geometria) |
| Impacto | Ninguno observable |

### F6 — Comparador authored, requisito de bloques y consulta

| Campo | Contenido |
|---|---|
| Objetivo | AUTH-12 y AUTH-13 |
| Precondiciones | F5b; CR-SVF-02 (X-3) y CR-SVF-06 resueltas |
| Produccion nueva | `A/Systems/<Kind>/*AuthoredComparator.cs`; `P/Systems/Shared/LibraryBlockQuery.cs` |
| Pruebas | T/ `AuthoredComparatorTests` (reproduce CT-AUTH), `LibraryBlockRequirementTests` (creadas en F5b, verificadas aqui junto a la consulta; rol `Required` con clave nula o en blanco → incumplido; clave con punto valida; `Annotation`/`Dimension` no aplican; `Pallet` segun CT-BLK; instancias aplanadas; `CantileverCurves` sin requisitos), T/ `LibraryBlockQueryGuardTests` (la consulta no importa ni sanea; `BlockLibraryImporter.cs` sin cambios) |
| GREEN | Focales con conteo; Core + UI; Debug Plugin |
| Impacto | Codigo nuevo sin consumidores de producto |

### F7 — Candidato

Registro de productores legacy vacio (una sola autoridad); arbol limpio, commit candidato creado antes de evidencias;
T2 sobre el SHA exacto; CI 4/4; cobertura; **OV-FND** completa (§3) sobre ese SHA; contrato y ADR de foundation cerrados; artefacto de
reconciliacion listo para `R(n+1)`.

### F8 — Integracion en `main`

Rebase final si `main` avanzo: cualquier SHA nuevo vuelve a F7 y exige sus evidencias propias, incluida validacion Owner cuando aplica; integracion `--no-ff`; CI del merge y coberturas del `MERGE_SHA` y del Candidato antes de limpiar; ROADMAP en
el momento 3; publicar `R(n+1)` con `Integration SHA` por autoridad y estado `INTEGRATED`; I-52 e I-55 registran el mismo SHA y rebasan.

El recibo posterior de integracion se publica mediante el flujo de iniciativa autorizado, nunca editando `main` directamente.
Actualizar Integration SHA exige un nuevo artefacto y los dos registros; no cambia por si solo los contratos congelados.

## 3. Validacion del Owner (diseno; no ejecutar)

| # | Gate | Pasos | Esperado |
|---|---|---|---|
| OV-FND-01 | F3 | `RACKEDITAR` → Actualizar e Insertar en Selectivo (2 fondos), Dinamico, Push Back compuesto, Cantilever y cabecera, incluida una vista huerfana | Igual que antes de F3 |
| OV-FND-02 | F5a | `RACKBOMTOTAL` y `RACKLISTA` con los seis kinds, un Push Back con diagnostico bloqueante y una linea Cantilever invalida | Igual que antes de F5a |
| OV-FND-03 | F5b | Insertar cada vista de cada kind y redibujarla | Mismos nombres de bloque (incluido el Cantilever) y misma geometria |
| OV-FND-04 | F7 | `RACKDUPLICAR` con varias fuentes | Igual que antes |

## 4. Riesgos y matriz

Riesgos: `specification.md` §8. Matriz de pruebas: la de cada gate de este mapa. Archivos: `specification.md` §7.

## 5. Estado

```text
Foundation Delivery Map = DRAFT · FOUNDATION_ID = PENDING · NOT CLAIMED · NOT CONSENSUS · IMPLEMENTATION BLOCKED
```
