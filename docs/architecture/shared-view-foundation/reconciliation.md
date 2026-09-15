# Shared View Foundation — Artefacto de reconciliacion

```text
RECONCILIATION_VERSION = R0
DATE                  = 2026-09-15
Foundation claim      = UNCLAIMED (ningun commit de reclamo autorizado)
STATUS                 = PROPOSED by I-55 (G2F) · NOT REGISTERED by I-52 · NOT EFFECTIVE
FOUNDATION_ID          = PENDING (Owner)
Mecanismo              = PENDING (Owner) · recomendado B (Proposal V5 de I-55 §2)

I-52 Proposal SHA      = b7a6d9fe897dae4d29ae29ada7f60127bca365e5   (Proposal V17)
I-55 Proposal SHA      = commit que publica este archivo en R0 (PROPOSAL_V5_SHA; decisions/I-55.md §G2F lo registra)
Foundation spec SHA    = el mismo commit, ruta docs/architecture/shared-view-foundation/specification.md
Base de codigo         = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
```

> Este archivo es la **unica** fuente de la reconciliacion de autoridades compartidas entre I-52 e I-55. Ninguna de las dos Proposals
> vuelve a leer la otra para fijar X-1..X-8: ambas citan una version de este artefacto por SHA.

## 1. Protocolo (sin livelock)

| # | Regla |
|---|---|
| REC-1 | **Versiones inmutables.** `Rn` queda fija en cuanto se publica. Un cambio crea `R(n+1)`; `Rn` no se edita |
| REC-2 | **Registro.** Una iniciativa registra `SVF-RECONCILIATION = Rn @ <commit SHA>` en su archivo de decisiones (`docs/automation/decisions/I-52.md`, `docs/automation/decisions/I-55.md` y, cuando exista, el de la foundation) |
| REC-3 | **Vigencia.** `Rn` es **EFFECTIVE** solo cuando I-52 e I-55 (y la foundation, si ya existe) registraron **el mismo SHA** |
| REC-4 | **Cambios por solicitud, no por relectura.** Quien quiera cambiar una fila abre una solicitud `CR-SVF-nn` en §5 de una version nueva; su Proposal remite a la solicitud y **no** reescribe su tabla X contra la Proposal de la otra |
| REC-5 | **Disparador unico de relectura:** un SHA nuevo de este artefacto. Tras adoptar este protocolo, una Proposal nueva no reinicia por si sola el consenso. Cambios materiales detectados en el preflight disparan CR-SVF; hasta la adopcion, sigue vigente la obligacion actual de I-52 de releer Proposals |
| REC-6 | **Limite.** Cualquier Coordinador puede escalar una version publicada no registrada por la otra parte inmediatamente, sin esperar otra Proposal. Owner resuelve desacuerdos, pero RESOLVED_BY_OWNER NO es EFFECTIVE: siguen siendo obligatorios ambos registros del mismo SHA |
| REC-7 | **Autoria de versiones.** Antes de F0, I-55 publica las versiones; I-52 aporta solicitudes CR-SVF para evitar dos autores de Rn. Tras F0, las publica la iniciativa de la foundation |
| REC-8 | **Consumo.** Una fila con `Integration SHA` vacio no es consumible (CLM-3 de la especificacion) |

## 2. Tabla X-1..X-8 (R0)

| X | Autoridad | Contrato (spec) | AUTH | Authority owner | Characterization owner | Extraction claim | Integration SHA | Politica de I-52 (V17) | Politica de I-55 (V5) | Estado R0 |
|---|---|---|---|---|---|---|---|---|---|---|
| X-1 | Nucleo neutral de seleccion | §3.7 | AUTH-07 | FOUNDATION_ID (propuesto; FQ-02) | FOUNDATION_ID (CT-16) | UNCLAIMED | — | ST-*; payload antes del filtro de Model Space; `Origin ≠ 0` falla; asignador de identidad T-M05 (propio) | Orden estable; `RackId` en blanco falla; kind con `TryResolve` sensible a mayusculas | I-55: PROPOSED · I-52: V17 dice «custodio I-52; extrae I-52 G4» → requiere Proposal posterior |
| X-2 | Taxonomia, direccion, codec, disponibilidad, clasificador del barrido, `Resolve`, `Plan`, nombre base | §3.1-§3.6, §3.9-§3.11 | AUTH-01..06, 09..11 | FOUNDATION_ID | FOUNDATION_ID (CT-04, CT-RES, CT-PLAN, CT-NAME, CT-SCAN) | UNCLAIMED | — | σ′; fallo cerrado; `Expone`, `F`, filtro μ_k; `A_k` = disponible ∩ expuesto; `Build` = `Resolve` + `Plan` sobre el diseño reflejado; PRE-06/07/08/11; E4, E6 | ID19 acepta `Canonical`/`Canonicalizable` + `Available` + `Resolved` sin `OutputBlocking` y con forma aceptada; `RACKEDITAR` conserva su lectura; BOM sin cambio | **MATERIAL CONFLICT DE PROCESO** (V17 «extrae la primera que llegue», CT-04 «primer G3 que llegue») |
| X-3 | Comparador authored por kind | §3.13 | AUTH-13 | FOUNDATION_ID (propuesto; FQ-02) | FOUNDATION_ID (CT-AUTH) | UNCLAIMED | — | Miembros = vistas seleccionadas; E3 | Miembros = conjunto de hermanas mutables | I-55: PROPOSED · I-52: V17 «extrae I-52 G5 o I-55 G14» → requiere Proposal posterior |
| X-4 | Requisito estructural de bloques (parte pura) y consulta tras importar | §3.12 | AUTH-12 | FOUNDATION_ID | FOUNDATION_ID (CT-BLK) | UNCLAIMED | — | Fallo antes de escribir; ST-13, ST-19, ST-20, PRE-17..19; `EnsureForPlan` (§9.5) reconciliado con la consulta | ID19 falla antes de escribir; ID17/ID18 colocan y reportan con la misma verificacion | I-55: PROPOSED · I-52: compatible con precisiones (primitivo **crear** `CreateInTransaction` sigue siendo de I-52, fuera de la foundation) |
| X-5 | Protocolo de comprobacion y reporte | §4, §6 | — | ambas | — | — | — | §15.2/§15.3/§19: la foundation como iniciativa rastreada; sus cambios preregistrados (§4 abajo) no son STOP | Cada gate de producto comprueba `Integration SHA` y reporta | Compatible |
| X-6 | Lineas base de C-2 y mapa G-M9 | — | — | I-52 (juego unico de fixtures) | I-52 | — | — | C-2 por defecto; la segunda de I-52/I-55 en integrar re-establece C-2; linea base = `main` con la foundation | Productores de I-55 declarados (PR-2, Insertar, redibujo atomico, lote) | Compatible |
| X-7 | Valor de colocacion, descomposicion de la transformacion fuente, normalizacion de angulos | §3.8 | AUTH-08 | FOUNDATION_ID (propuesto; FQ-02) | FOUNDATION_ID (CT-GEO) | UNCLAIMED | — | Reflexion y algebra `ReflectionAboutLine` (propias); acepta reflexiones y `s ≠ 1`; **valor de la tolerancia de escala** (hoy fijado por I-52 G4) | Acepta `Unit` y `UnitHalfTurn`; rigidez; Z por politica | Compatible con precisiones (valor de tolerancia registrado aqui: CR-SVF-01) |
| X-8 | Marco y tramo de una vista (`RackViewFrame`, `[K_min, K_max]`, centro `c`, desplazamientos) | §3.5 | AUTH-05 | FOUNDATION_ID | FOUNDATION_ID (CT-05) | UNCLAIMED | — | `c` = `Center` del marco; sin forma afin; δ registrado por CT-06 (propio), nunca ajustado | Anclas por `K_min`/`K_max` segun σ; superposicion por tramos | **MATERIAL CONFLICT DE PROCESO** (V17 «extrae la primera», CT-05 «primer G3 que llegue») |

**Clasificacion vinculante del Coordinador de I-55 para V5:** X-1, X-3, X-5, X-6 compatibles; X-4 y X-7 compatibles con precisiones; X-2 y
X-8 MATERIAL CONFLICT DE PROCESO hasta que ambas iniciativas acuerden el mecanismo y el orden de integracion. La columna «Estado R0» añade
lo que la relectura de V17 muestra: las clausulas de extraccion de V17 para X-1, X-3 y X-7 («extrae I-52 G4», «I-52 G5 o I-55 G14»)
tambien quedan subordinadas al mecanismo; su contenido es compatible. Si I-52 no acuerda la entrega neutral, FQ-02 bloquea F1;
no se habilita extraerlas dentro del producto I-52 ni se duplica CT-16.

## 3. Registro de reclamos (R0)

| AUTH | Authority | Foundation initiative/branch | Claim SHA | Owner | Consumers | Characterization owner | Integration SHA | Consumer policy refs | Status |
|---|---|---|---|---|---|---|---|---|---|
| AUTH-01 | `RackViewKind` | PENDING | — | FOUNDATION_ID | I-52, I-55 | FOUNDATION_ID (CT-04) | — | I-55 V5 §3.1; I-52 V17 §3.7 | UNCLAIMED |
| AUTH-02 | Variante y direccion | PENDING | — | FOUNDATION_ID | I-52, I-55 | CT-04 | — | idem | UNCLAIMED |
| AUTH-03 | Codec sintactico total | PENDING | — | FOUNDATION_ID | I-52, I-55, `RACKEDITAR`, PVME | CT-04 | — | I-55 V5 §3.1; I-52 V17 §3.7, §8.4 | UNCLAIMED |
| AUTH-04 | Hechos de disponibilidad | PENDING | — | FOUNDATION_ID | I-52, I-55 | CT-04 | — | I-55 V5 §3.2; I-52 V17 §3.8 | UNCLAIMED |
| AUTH-05 | `RackViewFrame` | PENDING | — | FOUNDATION_ID | I-52, I-55 | CT-05 | — | I-55 V5 §5; I-52 V17 §3.7 (`c`), §6.1, §8.1 | UNCLAIMED |
| AUTH-06 | Hechos del barrido | PENDING | — | FOUNDATION_ID | I-52, I-55, `RACKEDITAR`, PVME | CT-04, CT-SCAN | — | I-55 V5 §3.7, §4.1 | UNCLAIMED |
| AUTH-07 | Nucleo de seleccion | PENDING | — | FOUNDATION_ID (FQ-02) | I-52, I-55, `RACKDUPLICAR` | CT-16 | — | I-55 V5 §5; I-52 V17 §5.1 | UNCLAIMED |
| AUTH-08 | Valor de colocacion y transformacion fuente | PENDING | — | FOUNDATION_ID (FQ-02) | I-52, I-55 | CT-GEO | — | I-55 V5 §5; I-52 V17 §3.3, §4.1, §4.3, §8.3 | UNCLAIMED |
| AUTH-09 | `Resolve` | PENDING | — | FOUNDATION_ID | I-52, I-55, BOM | CT-RES | — | I-55 V5 §3.3; I-52 V17 §8.1, §9.1 | UNCLAIMED |
| AUTH-10 | `Plan` | PENDING | — | FOUNDATION_ID | I-52, I-55 | CT-PLAN | — | I-55 V5 §3.9; I-52 V17 §8.1 | UNCLAIMED |
| AUTH-11 | Nombre base | PENDING | — | FOUNDATION_ID | I-52, I-55 | CT-NAME | — | I-55 V5 §3.6 | UNCLAIMED |
| AUTH-12 | Requisito de bloques y consulta | PENDING | — | FOUNDATION_ID | I-52, I-55 | CT-BLK | — | I-55 V5 §3.5; I-52 V17 §8.1, §8.2, §9.5 | UNCLAIMED |
| AUTH-13 | Comparador authored | PENDING | — | FOUNDATION_ID (FQ-02) | I-52, I-55 | CT-AUTH | — | I-55 V5 §3.4; I-52 V17 §5.4, §6.1 | UNCLAIMED |
| AUTH-14 | Caracterizaciones | PENDING | — | FOUNDATION_ID | I-52, I-55 | FOUNDATION_ID | — | spec §3.14 | UNCLAIMED |

**Campos comunes por fila AUTH-01..14:** Implementation owner = FOUNDATION_ID (propuesto); Responsible branch = PENDING;
Foundation claim = UNCLAIMED; Integration requirement = merge completo alcanzable desde origin/main + CI posterior verde + recibo
registrado con el mismo SHA por ambos consumidores. Characterization owner = FOUNDATION_ID para todas las CT nombradas.
Ningun estado UNCLAIMED equivale a reclamo aceptado ni a autorizacion de implementar.

| AUTH | Authority | Foundation initiative/branch | Claim SHA | Owner | Consumers | Characterization owner | Integration SHA | Consumer policy refs | Status |
|---|---|---|---|---|---|---|---|---|---|
| AUTH-15 | Crear definicion y sobre caller-owned (FUERA de foundation actual) | I-52 / feature/rackmirror-espejo-semantico | UNREGISTERED para esta autoridad | I-52 (propuesto por V17) | I-52; I-55 G9b modo 1 y G15 | I-52 | PENDING | I-55 §4.6; I-52 §8.2/G6 | PROPOSED OWNER; NOT CONSUMABLE |

AUTH-15: implementation owner I-52 propuesto; responsible branch la indicada; requisito de integracion igual al anterior. El reclamo
de la iniciativa I-52 no sustituye un registro acordado de esta autoridad. CR-SVF-08 no anade codigo al alcance de F1..F8.

## 4. Cambios de la foundation preregistrados frente a listas de I-52

Para que I-52 los clasifique como esperados (no STOP de su §19) al adoptar el mecanismo:

| Archivo | Cambio de la foundation | Lista de I-52 V17 |
|---|---|---|
| `A/Persistence/RackDuplicationPlan.cs` | Nucleo neutral + fachada identica | §19 STOP |
| Resolvers y `P/KindHandlers/*` | `ResolveSystem` por kind, sin cambio observable | §19 (resolvers); §20.3 excluye `KindHandlers/*` **para I-52** |
| `P/Systems/*/*DrawService.cs`, `P/Drawing/LateralHeaderDrawService.cs`, `P/Drawing/PlantaHeaderDrawService.cs`, `P/Systems/FlowBed/FlowBedDrawService.cs`, `P/RackCantileverCommands.cs`, comandos `RACKEDITAR` | Delegacion de plan y de nombre base | §8.1 builders solo consumo; §19 si main cambia materialmente |
| `A/Systems/Shared/DimensionViewPolicy.cs` y ventanas WPF | Renombre de `DimensionViewKind` | §20.3 excluye editores WPF **para I-52**; guarda de enums de §6.7 |
| `P/ProjectVariableMutationExecutor.cs` | Decodificacion con el clasificador, sin cambio de transacciones | — |
| `A/Drawing/LibraryBlockRequirement.cs`, `P/Systems/Shared/LibraryBlockQuery.cs` | Nuevos | §8.2, §9.5 |

**No se tocan:** `P/Systems/Shared/SystemBlockWriter.cs`, `P/Drawing/LateralHeaderDrawer.cs`, `P/Drawing/BlockLibraryImporter.cs`,
la materializacion de `P/Drawing/Cantilever/CantileverViewMaterializer.cs`. Excepcion acotada preregistrada: F5b delega o retira
solo sus helpers de nombres `SuggestName`/`Sanitize`, sin cambiar creacion, redefinicion, transacciones ni geometria.

## 5. Solicitudes de cambio abiertas

| CR | Solicitud | Afecta | Quien decide |
|---|---|---|---|
| CR-SVF-01 | Valor de la tolerancia de escala registrado aqui (hoy fijado por I-52 G4, no provisional en V17 §4.3) | X-7, F4 | I-52 lo propone; ambos registran |
| CR-SVF-02 | Acordar titularidad y entrega neutral de X-1, X-3 y X-7 antes de F1; si I-52 no acepta, conflicto bloqueante, sin fallback al producto | AUTH-07, AUTH-08, AUTH-13 | Coordinadores de I-52 e I-55 |
| CR-SVF-03 | Precondicion «AUTH-xx INTEGRATED» en los gates de I-52 (§14.1, §14.2 G3..G7) y de I-55 (mapa V5 parte B) | Orden de integracion | Cada iniciativa en su Proposal; I-52 en Proposal posterior |
| CR-SVF-04 | CT-04, CT-05 y CT-16 pasan a la foundation (V17: «primer G3 que llegue») | X-1, X-2, X-8 | I-52 en Proposal posterior |
| CR-SVF-05 | `c` de I-52 = `Center` del marco (sin forma afin; CT-06 sigue en I-52) | X-8 | I-52 en Proposal posterior |
| CR-SVF-06 | `EnsureForPlan` de I-52 (§9.5) reconciliado con `LibraryBlockQuery` | X-4 | I-52 en Proposal posterior |
| CR-SVF-07 | Mecanismo de la foundation e ID | todas | Owner |
| CR-SVF-08 | Retirar la carrera de X-4 para crear caller-owned. AUTH-15 queda fuera de la foundation actual, propuesto a I-52; G15 espera su integracion. Moverlo a entrega neutral independiente exige decision de alcance explicita y version nueva; no se supone autorizado | AUTH-15; G9b modo 1 y G15 | Ambos Coordinadores; Owner si cambia alcance/titularidad |

## 6. Historial de versiones

| Version | SHA | Autor | Registrada por | Estado |
|---|---|---|---|---|
| R0 | commit de G2F de I-55 (decisions/I-55.md §G2F) | I-55 (Proposal V5) | I-55: propuesta, no registrada como EFFECTIVE; I-52: no | PROPOSED |

## 7. Plantilla para versiones siguientes

```text
RECONCILIATION_VERSION = R<n>
STATUS                 = PROPOSED | REGISTERED(I-52) | REGISTERED(I-55) | EFFECTIVE | SUPERSEDED
FOUNDATION_ID          = <ID asignado por el Owner>
I-52 Proposal SHA      = <SHA>
I-55 Proposal SHA      = <SHA>
Foundation spec SHA    = <SHA>@<ruta>
Cambios respecto de R<n-1> = <CR-SVF-nn resueltas>

§2 Tabla X-1..X-8          (mismas columnas)
§3 Registro de reclamos    (mismas columnas; Claim SHA e Integration SHA cuando existan)
§4 Cambios preregistrados
§5 Solicitudes abiertas
§6 Historial
```
