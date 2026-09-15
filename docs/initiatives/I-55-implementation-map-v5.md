# I-55 — Implementation Map V5: View Placement & Projection sobre la Shared View Foundation

```text
PROPOSAL V5 — NOT CONSENSUS
Coordinator      = REVIEW REQUIRED
Architect formal = PENDING (Arquitecto independiente sobre el SHA exacto de V5)
Owner            = PENDING (SVF-MECH; M-01 con OD-7.e; OD-1..OD-8; ADR-0042)
Consensus        = NOT REACHED
Implementation   = BLOCKED
Open Material    = SVF-MECH · SVF-REC · X-2 y X-8 MATERIAL CONFLICT DE PROCESO · M-01

Acompana a     = docs/initiatives/I-55-proposal-v5.md (misma version; ninguno vale sin el otro)
Sustituye a    = docs/initiatives/I-55-implementation-map-v4.md (historico, sin modificar)
Foundation     = docs/architecture/shared-view-foundation/{specification,reconciliation,delivery-map,adr-draft}.md
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/ · SVF/ docs/architecture/shared-view-foundation/
```

> Este mapa **no autoriza** ningun gate. **Incorporacion:** los gates de producto marcados «= V4 Gx con enmiendas» conservan el contenido
> del mapa V4 (`fe70d7a`) salvo lo que se enmienda aqui; en conflicto gana V5. Rutas y simbolos existentes verificados sobre `CURRENT_BASE`.

## 0. Convenciones

Las de mapa V4 §0 (precondiciones, RED, GREEN con conteo > 0, rollback por `git revert` sin migrar datos, pruebas de UI sin rutas modales
reales, apertura con `git fetch --all --prune` y rebase si `main` avanzo). **Nuevo en V5:** todo gate de producto comprueba en su apertura
que cada autoridad de la foundation que consume tiene `Integration SHA` alcanzable desde `origin/main` (CLM-3) y lo cita en su evidencia;
si falta, STOP.

## A. Entrega de la Shared View Foundation

**Documento normativo del borrador:** `SVF/delivery-map.md`. Resumen:

| Gate | Contenido | Precondicion clave |
|---|---|---|
| F0 | Autorizacion del Owner, `FOUNDATION_ID`, reclamo, bootstrap; Proposal que reexpresa `SVF/specification.md`; `R1` | Owner (SVF-MECH) |
| (consenso) | Discovery breve, Proposal, Coordinator + Arquitecto independiente, ADR de foundation aceptado | Sin M-01, OD, ADR-0042 ni decisiones de `RACKMIRROR` |
| F1 | Caracterizaciones con dueño unico: CT-04, CT-05, CT-16, CT-RES, CT-PLAN, CT-NAME, CT-BLK, CT-SCAN, CT-GEO, CT-AUTH | Freeze de la foundation; `R1` EFFECTIVE |
| F2 | Taxonomia (renombre), variante y direccion, codec total, hechos del barrido | F1; serializado con I-49 G10 |
| F3 | Disponibilidad (hechos), `RackViewFrame`, migracion de las decodificaciones de `RACKEDITAR` y PVME al clasificador | F2 |
| F4 | Nucleo de seleccion y valor de colocacion | F2; I-49; CR-SVF-01 y CR-SVF-02 resueltas |
| F5a | Contrato `Resolve` y adaptadores por kind (ADR-0034 intacto) | F3 |
| F5b | Autoridad unica de `Plan` y de nombre base; tipo puro LibraryBlockRequirement/roles; delegacion de lambdas y helpers | F5a |
| F6 | Comparador authored y consulta; consume el requisito puro creado en F5b | F5b; CR-SVF-02, CR-SVF-06 |
| F7 | Candidato con OV-FND | F6 |
| F8 | Integracion en `main`; `R(n+1)` con `Integration SHA`; I-52 e I-55 registran el mismo SHA y rebasan | F7 |

I-55 **no** ejecuta ninguno de estos gates.

## B. Entrega de producto de I-55 (tras integrar la foundation)

### B.1 Secuencia

```text
[F8 foundation en main] → rebase de I-55 → G3 ─┬─ G4 (PR-1) ─┐
                                                └─ G5 (PR-2) ─┴─ G6 (consumo) ─ G7 (Prepare de producto) ─┬─ G8 (colocacion) ──────┐
                                                                                                           └─ G9a (seam atomico) ────┴─ G9b (Insertar)
   └─ G10 (ID17) ─ G11 (ID18 contrato) ─ G12 (ID18 UI) ─ G14 (ID19 puro) ─ G15 (ID19 comando) ─ G16
```

| Gate V4 | Gate V5 | Cambio |
|---|---|---|
| G3 | G3 | Solo caracterizaciones de producto; CT-04, CT-05, CT-16 y las de la foundation salen; precondicion foundation INTEGRATED |
| G4, G5 | G4, G5 | Tras la foundation (D-09 enmendada) |
| G6 (foundation pura) | **G6 (consumo)** | Ya no crea autoridades: verifica consumo y politicas de codec/disponibilidad de I-55 |
| G7a (`Resolve`) | **Retirado** | AUTH-09 (foundation F5a) |
| G7b (`Plan`, `Prepare`, requisito) | **G7 (`Prepare` de producto)** | `Plan`, nombre base y requisito son de la foundation; queda composicion del sobre, politica y ciclo de `RackId` |
| G8 | G8 | Consulta y requisito consumidos de la foundation |
| G9a, G9b | G9a, G9b | Precisiones de CQ-01 (Proposal V5 §4) |
| G10, G11, G12 | G10, G11, G12 | Sin cambio de alcance |
| G13 (X-1) | **Retirado** | AUTH-07 (foundation F4) |
| G14, G15 | G14, G15 | Orden §3.4, remedios §3.8, OD-7.e, INV-GRP-5, oraculos de CT-05 |
| G16 | G16 | Sin cambio |

| Coordinacion | Gates | Regla |
|---|---|---|
| Foundation integrada | Apertura de todos los gates | CLM-3; evidencia con `Integration SHA` |
| Reconciliacion | Antes del Consensus Freeze (precondicion de G3) | `Rn` EFFECTIVE (mismo SHA en I-52 e I-55) |
| X-4 crear en la transaccion del llamador | G15 | AUTH-15 INTEGRATED: Integration SHA alcanzable desde origin/main y reconciliacion registrada; si falta, STOP G15. No se crea autoridad alternativa con primitivas vigentes. G9b conserva sus dos modos |
| X-5 | G5..G15 | Reporte con los sitios nuevos (G-M24) |
| X-6 | G5, G7, G8, G9b, G12 | La segunda de I-52/I-55 en integrar re-establece C-2 sobre `main` con la foundation |
| Archivo caliente del Selectivo | G10, G12 | Serializar con I-49 G10 |

### G3 — Caracterizacion de producto (solo pruebas)

| Campo | Contenido |
|---|---|
| Precondiciones | Coordinator y Architect formal AGREED sobre la misma version; M-01 resuelta (con OD-7.e); OD-1..OD-8; ADR-0042 aceptado; Consensus Freeze; **AUTH-01..AUTH-14 INTEGRATED** y rebase; `Rn` EFFECTIVE |
| Produccion | Ninguna |
| Clases (= mapa V4 G3 menos las de la foundation) | `LegacyViewPayloadCompositionCharacterizationTests`; `RackCountInvariantCharacterizationTests`; `RackEditPreflightCharacterizationTests`; `FirstViewGateCharacterizationTests` (TU); **nuevas:** `InsertBlankIdFlowCharacterizationTests` (flujo vigente de `Id` en blanco por kind y vista elegida, base de IC-10), `InsertRedrawLayerAccessCharacterizationTests` (guarda de fuentes: sitios que abren para escritura referencias y entidades en el redibujo, base de §4.5) |
| Salen a la foundation | CT-04, CT-05, CT-16, `LegacyViewBlockNameCharacterizationTests`, `RackSiblingScanInputCharacterizationTests`, `HeadlessResolutionParityCharacterizationTests`, `BomResolutionCharacterizationTests`, `LibraryBlockRequirementCharacterizationTests`, `BuilderCatalogSkipCensusTests` |
| RED / GREEN | Caracterizaciones verdes sobre codigo intacto con teoria de mutacion en las guardas / conteo; Core + UI; CI 4/4 |

### G4 — PR-1 (= V4 G4)

Sin cambio de contenido; precondicion G3.

### G5 — PR-2 (= V4 G5 con enmienda)

**Produccion:** los dos llamadores del Plugin pasan `design.PlantaVisibility` en el **contexto de plan de la foundation** (tras F5b el plan
Cantilever se pide a la autoridad; nulo = comportamiento vigente). Pruebas: `CantileverPlantaVisibilityGuardTests` (guarda sobre los
llamadores), `CantileverPlantaVisibilityBuilderTests`. X-6: cambia un productor de C2-2a/C2-2b de I-52.

### G6 — Consumo de la foundation y politicas de codec y disponibilidad

| Campo | Contenido |
|---|---|
| Objetivo | Politicas de I-55 (Proposal V5 §3.1, §3.2) sobre las autoridades integradas, sin crear ninguna |
| Precondiciones | G4, G5; AUTH-01..AUTH-06 INTEGRATED |
| Produccion nueva | `A/Views/Policy/RackViewExposure.cs` (matriz de exposicion de V4 §5 como politica), `A/Views/Policy/ProjectionCodecPolicy.cs` (acepta `Canonical`/`Canonicalizable`), `A/Views/Policy/ProjectionAvailabilityPolicy.cs` |
| Pruebas | T/ `RackViewExposureTests`, `ProjectionCodecPolicyTests` (frontal Selectivo `−1` segun CT-04: `Coerced` → rechazo), `ProjectionAvailabilityPolicyTests` (solo `Available` continua); T/ `ProductPolicyIndependenceGuardTests` (ningun archivo de la foundation nombra politicas de I-55; I-55 no redefine hechos) |
| RED / GREEN | Vistas fallando / T0 `FullyQualifiedName~RackViewExposure\|FullyQualifiedName~ProjectionCodecPolicy\|FullyQualifiedName~ProjectionAvailabilityPolicy` y T1 `FullyQualifiedName~ProductPolicyIndependence`; Core + UI |
| Impacto | Codigo nuevo sin consumidores |

### G7 — `Prepare` de producto

| Campo | Contenido |
|---|---|
| Objetivo | Proposal V5 §3.9: politica + `Plan` de la foundation + composicion del sobre + metadatos |
| Precondiciones | G6; AUTH-09..AUTH-12 INTEGRATED |
| Produccion nueva | `A/Views/Preparation/*` (contexto, resultado, preparadores por sistema que llaman a la autoridad de plan), `RackViewEnvelopeComposition.cs` (unica llamada nueva a `Compose`; origen como parametro `RackEmbedDocument`) |
| Pruebas | T/ `RackViewPreparation{Selective,Dynamic,PushBack,Cantilever,Cabecera,Cama}Tests`, `PreparedViewPayloadGoldenTests` (byte a byte = formula legacy de G3), `RackIdLifecycleTests`; T/ `NoProductPlannerGuardTests` (I-55 no construye planes ni nombres base) |
| Guardas reapuntadas con motivo | `CustomPropertiesEnvelopeGuardTests.TGrd02_*` (+1 archivo y llamada), sin debilitar |
| GREEN | T0 `FullyQualifiedName~RackViewPreparation\|FullyQualifiedName~PreparedViewPayload\|FullyQualifiedName~RackIdLifecycle`; T1 `FullyQualifiedName~NoProductPlanner\|FullyQualifiedName~CustomPropertiesEnvelopeGuard`; Core + UI |

### G8 — Colocacion de vista unica (= V4 G8 con enmiendas)

- Consulta de bloques tras importar = `LibraryBlockQuery` de la foundation (no se crea); politica de colocar y reportar (Proposal V5 §3.5).
- Nombres por la autoridad de la foundation; `UniqueBlockName` sin cambio.
- Resto sin cambio (D-10, parametro de prompt, reporte estructural en todo camino, fixtures de Insertar, guardas reapuntadas).

### G9a — Seam atomico sin cablear (= V4 G9a con enmiendas)

| Enmienda | Contenido |
|---|---|
| Pertenencia | `A/Views/Redraw/RackSiblingMembership.cs`: **una** funcion (Proposal V5 §4.1) sobre los hechos del barrido de la foundation; `RackSiblingRedrawPlan` la consume |
| Puerto | `ISiblingRedrawPort.Prepare` / `Mutate(units) → Committed \| Discarded` / `Post`; el `using` de la transaccion vive en el adaptador (§4.4) |
| Capas bloqueadas | PREPARE devuelve `PREPARE_FAILED` con vista y capa (§4.5); el adaptador lee capas de referencias directas y entidades de cada unidad |
| Inyeccion de fallo | Solo Debug en el adaptador (§4.5) |
| Plan de unidades | Pedido a la autoridad de plan de la foundation; los envoltorios nuevos no replican lambdas |
| Pruebas nuevas o ampliadas | T/ `RackSiblingMembershipTests`: fuente elegida siempre miembro (tambien con `Id` vacio o de espacios); dependiente de xref → READ_ONLY; ilegible con `ProbeId == RackId` → bloqueante; con otro o sin `Id` → no atribuible; REDRAW sin referencias no es superviviente; `SURVIVORS = ∅ ∧ ERASE ≠ ∅` → borrado en el primer jig; mismo conjunto para gate y clasificacion. `RackSiblingRedrawRunTests` (V4 (1)-(4), (9), (10)) + `Mutate` descarta sin commit ante excepcion o fallo tipado + ninguna llamada al puerto despues de `Discarded` salvo el informe + capa bloqueada → `PREPARE_FAILED` sin llamar a `Mutate`. `RackSiblingRedrawUnits{...}Tests` (V4 (11)-(15)) con el plan de la foundation |
| Guardas | `SiblingRedrawSeamGuardTests` (V4) + el `using` de la transaccion solo dentro de `Mutate`; `CallerOwnedFacadeGuardTests` **extendida** a los siete envoltorios nuevos; `DebugFaultInjectionGuardTests` (el punto de inyeccion solo existe bajo `#if DEBUG`) |
| GREEN | T0 `FullyQualifiedName~RackSiblingMembership\|FullyQualifiedName~RackSiblingRedrawPlan\|FullyQualifiedName~RackSiblingRedrawRun\|FullyQualifiedName~RackSiblingRedrawUnits`; T1 `FullyQualifiedName~SiblingRedrawSeamGuard\|FullyQualifiedName~CallerOwnedFacadeGuard\|FullyQualifiedName~DebugFaultInjectionGuard`; Core + UI; Debug Plugin |
| X-5 / G-M24 | Sitios nuevos del redibujo atomico declarados a I-52 (AR4-37) |

### G9b — Insertar (= V4 G9b con enmiendas)

| Enmienda | Contenido |
|---|---|
| Barrido unico | INV-SCAN-1: un resultado de barrido por Insertar para preflight, variante, gates y clasificacion; guarda `SingleScanInsertGuardTests` |
| `Id` en blanco | Fuente elegida siempre curada y redibujada (Selectivo, Dinamico, Cabecera) (§4.2); `IsNullOrWhiteSpace` solo en la rama Insertar, Actualizar conserva el predicado por kind (Selectivo/Cabecera `IsNullOrEmpty`; los demas, el vigente); guarda `BlankIdPredicateScopeGuardTests` |
| INV-TX-1 | Comprobacion `TopTransaction == null` tras `Mutate` y antes de `PLACE(1)`; guarda |
| Primer jig del caso de huerfanas | Dos modos de Proposal §4.6: modo 1 SOLO si AUTH-15 esta integrado en main; el adaptador del jig consume ese primitivo, incluye REDRAW sin referencias de layout + ERASE + crear + sobre + referencia en una transaccion, cancelacion aborta. Sin AUTH-15: modo 2 de V4, definicion/sobre previos, limpieza best effort y R-25 residual; I-55 no crea su propia autoridad. Guarda por modo y OV-RED-04 |
| Informe | Huerfanas borradas y solo lectura no tocadas (§4.8); estado `PREFLIGHT_FAILED` |
| Productores nuevos | Declarados en G-M24 y X-5 |
| Guardas reapuntadas | Las de V4 G9b; ninguna se debilita |
| GREEN | Las de V4 G9b + las guardas nuevas; **OV-META-04..06**, **OV-RED-01..06**, **OV-ID18-10/11** |
| Si OV-RED-04 muestra que AutoCAD no admite el jig sobre una definicion sin confirmar | STOP; el Coordinador decide volver a V4 §11.3 con R-25 residual (R-28) |

### G10, G11, G12 (= V4 con enmiendas menores)

- G11: estado `PREFLIGHT_FAILED`; mensajes con huerfanas borradas; `PLACEMENT_FAILED_PARTIAL_BATCH` solo por excepcion.
- G12: el driver usa `ISiblingRedrawPort` e INV-TX-1; OV-RED-05 en lote.

### G14 — ID19 puro (= V4 G14 con enmiendas)

| Enmienda | Contenido |
|---|---|
| Consumo | Nucleo de seleccion (AUTH-07), valor de colocacion y hechos de la fuente (AUTH-08), comparador (AUTH-13), marco (AUTH-05), `Resolve` (AUTH-09) desde `main`; nada se extrae |
| Orden | CLASSIFY → GROUP → gates de autoridad → representante → RESOLVE → EDIT PREFLIGHT → AVAILABLE → FRAMES → VALIDATE → PLANS (Proposal V5 §3.4) |
| Politica de `Resolve` | `Resolved` sin `OutputBlocking` y forma `Current`/`LegacyAccepted`/`Canonicalizable`; resto falla con motivo (§3.3) |
| Remedios | `RackProjectionRemedyTests`: una fila por situacion de la tabla de §3.8, con los PLAUSIBLES verificados o degradados (OM-31) |
| Sentido del eje comun (OD-7.e) | `RackOrthographicPlacementPolicyTests` con la regla elegida por el Owner (recomendada A): casos C1..C5 de §5.2 con sus valores; limite `3π/4 ± ε` en radianes; empate en B → A; aviso de cercania al limite (OM-33); ida y vuelta: identidad o reflexion segun la regla (§5.1) |
| INV-GRP-5 | `RackPlacedGeometryInvariantTests`: matriz independiente; casos de §5.3 (tabla numerica incluida) y mutaciones que deben fallar |
| Oraculos | Anclas, tramos y centros tomados de los fixtures de CT-05 (AR4-48) |
| Terminologia | Mensajes «reflexion no admitida», «escala distinta de 1 no admitida», «Z negativa no admitida»; SCP con Z = +Z |
| GREEN | T0 de V4 G14 + `FullyQualifiedName~RackProjectionRemedy\|FullyQualifiedName~RackPlacedGeometryInvariant`; Core + UI |

### G15 — ID19 comando (= V4 G15 con enmiendas)

- SNAPSHOT con los hechos del barrido de la foundation; `Resolve` por el puerto una vez por `RackId`; claves de biblioteca sin sanear;
  `LibraryBlockQuery` y `LibraryAvailability` para el fallo antes de escribir.
- Materializacion: consumir AUTH-15 integrado desde main; si falta, STOP G15 (tabla de coordinacion de B.1).

### G16 (= V4 G16)

## T. Matriz de pruebas (producto)

| Gate | Clases | Tipo | Suite |
|---|---|---|---|
| G3 | payloads, conteos, preflights de `RACKEDITAR`, puertas de primera vista, flujo de `Id` en blanco, acceso a capas en el redibujo | comportamiento + guarda | Core + UI |
| G4 | `PushBackLateralPostIndexRegressionTests` | UI | UI |
| G5 | `CantileverPlantaVisibilityGuardTests`, `CantileverPlantaVisibilityBuilderTests` | guarda + comportamiento | Core |
| G6 | `RackViewExposureTests`, `ProjectionCodecPolicyTests`, `ProjectionAvailabilityPolicyTests`, `ProductPolicyIndependenceGuardTests` | comportamiento / guarda | Core |
| G7 | `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests`, `NoProductPlannerGuardTests`, `CustomPropertiesEnvelopeGuardTests` | comportamiento / guarda | Core |
| G8 | `RackViewPlacementGuardTests`; fixtures de Insertar; guardas reapuntadas | guarda | Core |
| G9a | `RackSiblingMembershipTests`, `RackSiblingRedrawPlanTests`, `RackSiblingRedrawRunTests`, `RackSiblingRedrawUnits*Tests`, `SiblingRedrawSeamGuardTests`, `CallerOwnedFacadeGuardTests`, `DebugFaultInjectionGuardTests` | comportamiento / guarda | Core |
| G9b | `RackSiblingCustomPropertiesGateTests`, `RackEnvelopeIdProbeTests` (consumo de la sonda como hecho), `RackSiblingGateWiringGuardTests`, `SingleScanInsertGuardTests`, `BlankIdPredicateScopeGuardTests`, `OrphanFirstJigTransactionGuardTests`, guardas reapuntadas | comportamiento / guarda | Core |
| G10..G12 | = V4 | — | — |
| G14 | = V4 G14 + `RackProjectionRemedyTests`, `RackPlacedGeometryInvariantTests` | comportamiento / guarda | Core |
| G15 | = V4 G15 | guarda | Core + UI |
| Todos | T2 suites completas; T3 CI | — | — |

## OV. Validacion del Owner (diseno; no ejecutar)

**Incorporada de mapa V4 §OV** con estos cambios:

| # | Cambio |
|---|---|
| OV-FND-01..04 | En la foundation (`SVF/delivery-map.md` §3), no en I-55 |
| OV-RED-03 | Capa bloqueada → «No se inserto ninguna vista y no se modifico ninguna vista existente: <vista> esta en la capa bloqueada <capa>» (`PREPARE_FAILED`), sin cambiar ninguna hermana |
| OV-RED-04 | Selectivo cuya unica vista propia es la frontal del fondo 2 → encoger a 1 fondo → Insertar planta → Esc en el jig: nada cambia (**ni definicion nueva ni borrado**; comprobar con `PURGE` que no queda una definicion de rack sin referencias); repetir y colocar: planta nueva y huerfana retirada en el mismo paso. Con el DLL Debug. Si AutoCAD rechaza el arrastre: registrar y detener (R-28) |
| **OV-RED-06** | DLL Debug con la inyeccion de fallo en la segunda unidad → Insertar en un Selectivo con F/L/P modificado. Esperado: `REDRAW_ROLLED_BACK`, ninguna hermana cambio (evidencia fisica del rollback) |
| **OV-RED-07** | Selectivo con una lateral elegida de `Id` en blanco (si el Owner dispone de uno) → `RACKEDITAR` → Insertar planta. Esperado: la lateral se redibuja con el `Id` curado y la planta nueva comparte ese `Id` (IC-10) |
| OV-ID19-11 | Error con disposicion y el remedio de la tabla de Proposal V5 §3.8 que corresponda (no siempre «RACKEDITAR») |
| **OV-ID19-22 (reescrita)** | Fila de plantas con un rack girado 180° → Frontal; despues, girar 180° racks suficientes para ser mayoria → Frontal. **Con OD-7.e = A:** el orden sobre la corrida no cambia en ninguno de los dos pasos. **Con B:** el orden se invierte en el segundo. Registrar lo observado |
| **OV-ID19-25** | Ida y vuelta Planta → Frontal → Planta de la fila anterior con mayoria girada. Esperado: con A conserva los tramos; no promete recuperar las orientaciones descartadas salvo traslacion; con B vuelve reflejada sobre la recta |
| **OV-ID19-26** | Layout con una planta de `Origin ≠ 0` (si el Owner dispone de uno; si no, `RACKLAYOUT` sobre un bloque redefinido con punto base desplazado) → Planta (`Rigid`). Esperado: cada vista nueva queda en la posicion del ancla fuente trasladada, sin el desplazamiento del `Origin` |

## R. Riesgos

Mapa V4 §R incorporado con los cambios de Proposal V5 §10.5 (R-03 reescrito; R-16 y R-20 a la foundation o retirados; R-25 eliminado sujeto a
OV-RED-04; R-26 mitigado; R-27, R-28, R-29 nuevos). Riesgos de la foundation: `SVF/specification.md` §8.

## F. Archivos y simbolos (producto)

### F.1 Produccion nueva (propuesta)

| Ruta | Gate |
|---|---|
| `A/Views/Policy/RackViewExposure.cs`, `ProjectionCodecPolicy.cs`, `ProjectionAvailabilityPolicy.cs` | G6 |
| `A/Views/Preparation/*`, `RackViewEnvelopeComposition.cs` | G7 |
| `P/Views/RackViewPlacement.cs` | G8 |
| `A/Views/Redraw/RackSiblingMembership.cs`, `RackSiblingRedrawPlan.cs`, `RackSiblingRedrawRun.cs` (con `ISiblingRedrawPort`); `P/Systems/Shared/SiblingRedrawUnits.cs`, `SiblingRedrawTransaction.cs` | G9a |
| `A/Views/RackSiblingCustomPropertiesGate.cs`; lectura de entradas en el Plugin sobre los hechos del barrido de la foundation | G9b |
| `A/Views/RackViewBatchPlan.cs` | G11 |
| `U/Views/RackViewBatchDialog.xaml(.cs)`, `U/Views/RackViewBatchDialogPresenter.cs`, `P/Views/RackViewBatchDriver.cs` | G12 |
| `A/Views/Placement/*` (`SourceGroupFrame`, `TargetGroupFrame`, `CommonTransform2D`, `RackGroupAnchorPolicy`, `RackGroupPlacementPlan`, remedios), `A/Views/RackSiblingAuthoredGate.cs` | G14 |
| `P/RackProyectarCommands.cs` | G15 |

**Ya no son de I-55:** `A/Views/RackViewKind.cs` y compania, `A/Views/Resolution/*`, `RackViewPlanner`, `RackViewNameSeed`,
`A/Drawing/LibraryBlockRequirement.cs`, la consulta de bloques, `RackEnvelopeIdProbe` (hecho del barrido), el nucleo de seleccion y el valor
de colocacion (foundation).

### F.2 Produccion modificada

Mapa V4 §F.2 menos lo que pasa a la foundation (renombre de `DimensionViewKind`; `P/KindHandlers/*`; decodificacion de los comandos;
`A/Persistence/RackDuplicationPlan.cs`), mas `P/Drawing/BlockPlacement.cs` (entrada del primer jig del caso de huerfanas, G9b).

### F.3 NO TOCAR sin nueva revision del Arquitecto

Mapa V4 §F.3, mas: los archivos de la foundation una vez integrados (solo consumo); `P/KindHandlers/*` (autoridad de resolucion de la
foundation); las autoridades de plan y nombre base.

## S. Estado

```text
Implementation Map V5 = NOT CONSENSUS · Open Material = SVF-MECH · SVF-REC · X-2/X-8 MATERIAL CONFLICT DE PROCESO · M-01
Coordinator = REVIEW REQUIRED · Architect formal = PENDING · Owner = PENDING · Consensus = NOT REACHED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
