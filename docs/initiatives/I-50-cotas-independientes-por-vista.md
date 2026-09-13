---
schema: rackcad-initiative/v1
id: I-50
title: "Cotas independientes por vista"
type: feature
status: integrated
branch: feature/cotas-independientes-por-vista
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [ui-editors, persistence, system-selective, architecture-kernel]
automation_state_path:
decision_paths: [docs/automation/decisions/I-50.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-50 — Cotas independientes por vista

> **Fase actual: G8 — cierre documental para la integración (2026-09-12).** El producto está terminado y
> validado; este commit es **solo documentación** y **no** reemplaza al Candidate. El merge `--no-ff`, las
> compuertas posteriores al merge y la limpieza siguen [WORKFLOW](../WORKFLOW.md) §4.5 y §3.
>
> ```text
> I-50 = IMPLEMENTED / VALIDATED / READY FOR INTEGRATION
>
> FINAL_CANDIDATE_SHA = 6cd2970c36a906cb9e784797008bed5e8111e433
>
> Coordinator         = AGREED     (Proposal V1.2, FROZEN)
> Architect           = AGREED     (Proposal V1.2, MAT-A1 RESOLVED)
> Owner ADR           = ACCEPTED   (ADR-0035, aceptado)
> Automated Candidate = PASS
> Owner Validation    = PASS       (AutoCAD 2025, OV-1..OV-10)
>
> G0 / G1  = CLOSED
> G2 / G2A = CLOSED
> G4       = CLOSED
> G5       = CLOSED
> G6       = CLOSED
> G3       = CLOSED
> G7       = CLOSED
> G8       = IN PROGRESS   cierre documental; merge, compuertas posteriores al merge y limpieza pendientes
> ```
>
> **CLOSED todavía no.** Por la convención del repositorio, este cierre documental escribe `status: integrated`
> y la marca `integrada (2026-09-12)` de ROADMAP **antes** del merge, para que el merge lleve los documentos
> consigo ([WORKFLOW](../WORKFLOW.md) §4.5 paso 4): `integrada` significa que el merge existe en `main`, no que
> la integración esté verificada (§5). I-50 queda **CLOSED** solo cuando el CI del `MERGE_SHA` pase con su
> cobertura, pase la comprobación diferida de la cobertura del Candidate (§4.5 pasos 6 y 7) y se retiren rama y
> worktree (§3). Los conteos de pruebas viven en [HANDOFF](../HANDOFF.md) §5, su único sitio.

```text
BASE_SHA               = a4d88f18a1f42263d366c44dc05dd18a6786f152
CLAIM_SHA              = 97cc0ef66bf865175a8976e4e2452aa6a4e0d5e6   (Claim-Id 4d9fb22f-3c68-4fa9-b4bf-52ff9d9b8252)
BOOTSTRAP_SHA          = a2fba4f280027f36f32d04298a48f975c999a3ee
G1_SHA                 = fdaf2bc31be2232383d6e827c5ef8613fce8cfd4   (Discovery y decisiones CD-01..CD-11)
PROPOSAL_V1.2_SHA      = 0e91c52f41f26b3302af8cabd9dd8b293f2d3cbd   (FROZEN)
FREEZE_SHA             = 9b592e934e9293612de255b543f81943933bb6bd   (consenso sobre V1.2 y ADR-0035 aceptado)
CHARACTERIZATION_SHA   = 974c70916dd9db538b72b21439741e259a674e4e   (G4 Paso 1: T-03..T-05, solo pruebas)
MODEL_POLICY_SHA       = fb039e1eed4f7caac7288901078cd3ff54fa0a59   (G4 Paso 2, 1/2)
DRAW_SHA               = 11c04db1ba596ba974ca6dcbf096e9882a15d54c   (G4 Paso 2, 2/2)
G5_FINAL_SHA           = a4806ff4ca0e563b0496908fedb8f21d569ec9c5   (G5: Selectivo, Dinámico y Push Back)
PUSHBACK_COMPOSITE_SHA = 8ceb3a72c093b511d4f128510e2f4bfd07b78138   (G6: C-15, T-08, T-16)
FINAL_CANDIDATE_SHA    = 6cd2970c36a906cb9e784797008bed5e8111e433   (cierre de G3; Candidate de G7)
CI Candidate           = push 34732123814, head_sha = FINAL_CANDIDATE_SHA, 4/4 success
Coverage Candidate     = dispatch 34733326111, objetivo = checkout verificado = FINAL_CANDIDATE_SHA, rackcad-coverage-cobertura
Owner Validation       = PASS — AutoCAD 2025, OV-1..OV-10
MERGE_SHA              = PENDING hasta el merge
Branch                 = feature/cotas-independientes-por-vista
Discovery              = docs/initiatives/I-50-discovery.md
Decisiones             = docs/automation/decisions/I-50.md
ROADMAP                = fila «Cotas independientes por vista (ID1)»; alineada en el cierre: integrada (2026-09-12)
```

Los SHAs de gate intermedios (`94220fb`, `695d34b` y `7a9335c` en G5; `ed50cbd` y `2765a45` en G3) están en la
tabla de la sección 10, y la evidencia de cada gate en la sección 16.

## 1. Objetivo

Permitir decidir de forma independiente **en qué vistas de un rack se muestran las cotas**, reutilizando el motor de cotas existente y **sin reconstruirlo**.

ID1 no pide crear cotas automáticas. El pendiente es seleccionar la visibilidad de las cotas por vista real soportada por cada sistema.

## 2. Pregunta de contrato que Discovery debe resolver

La hipótesis inicial es una política persistida por `rack + view-kind`, pero **no queda aceptada por este contrato**. Discovery debe demostrar si la autoridad correcta pertenece:

- al **tipo de vista** del rack, de modo que dos instancias `Front` compartan política; o
- a **cada instancia dibujada**, de modo que dos vistas del mismo kind puedan diferir.

Alternativas a comparar: flags explícitos, set de `ViewKind`, policy object y metadata por view instance. Si el producto no queda determinado por el comportamiento y contratos actuales, se escala al Owner antes de G2. Si Discovery exige una decisión arquitectónica transversal material, se pide revisión de Arquitecto antes de implementación.

> **Resuelta en G1.** La autoridad es **rack × tipo de vista**; la metadata por instancia queda
> rechazada. Evidencia en [I-50-discovery.md](I-50-discovery.md) §9; decisiones `CD-01` y `CD-08` en la
> sección 12.

## 3. Compatibilidad obligatoria

Los DWG existentes deben conservar **exactamente** el comportamiento visual actual. Debe existir un default legacy equivalente a la configuración vigente. Abrir o actualizar un dibujo viejo no puede hacer desaparecer cotas.

La política acordada debe sobrevivir el flujo completo:

`crear -> insertar -> RACKEDITAR -> Actualizar -> save -> reopen -> linked view insertion`.

Un redraw no puede recuperar silenciosamente una política global histórica y perder la selección por vista.

## 4. Alcance de Discovery

Auditar **todos los sistemas/productos con cotas existentes** y concretar por sistema:

- tipos de vista;
- dónde se generan cotas;
- propiedad global vigente;
- quién decide `DrawDimensions` o equivalente;
- persistencia actual;
- metadata/identidad de vista;
- insert/update/redraw;
- save/reopen;
- `RACKEDITAR`;
- library/export cuando aplique.

Entregable mínimo de G1: matriz `Sistema × Vista × cotas existentes × autoridad actual × persistencia × cambio necesario`, inventario completo de call sites de drawing y trazado create/update/reopen.

> **Entregado**: [I-50-discovery.md](I-50-discovery.md).

## 5. Alcance de producto

La fila original dice **Todos**. I-50 no puede declararse completa si solo cubre Selectivo. Discovery debe identificar qué sistemas poseen cotas y qué vistas soportan; una solución shared + adaptadores es válida si cae naturalmente del diseño existente.

No se crean controles para vistas o cotas que un sistema no posee hoy.

> **Resultado de G1**: poseen cotas **Selectivo, Dinámico y Push Back**, en sus vistas frontal, lateral y
> planta. Cantilever, Cama, Larguero y Cabecera no dibujan cotas, así que no reciben controles
> ([I-50-discovery.md](I-50-discovery.md) §3).

## 6. UI

Buscar el punto mínimo y coherente de configuración: creación, editor o una única sección `Cotas` con controles por vista según la taxonomía real. Evitar duplicar configuración en varias ventanas salvo necesidad demostrada.

**No tocar `LinkedPropertyEditor`, Expression Engine ni Project Variables salvo necesidad real demostrada.** I-49 corre en paralelo **sin conflicto declarado**; la coordinación obligatoria por archivos compartidos está en la sección 11.

## 7. Fuera de alcance

- rediseñar el motor completo de cotas;
- ID17 first-view freedom;
- ID18 multi-view queue;
- ID19 multi-rack projection;
- ID2 pallet visibility;
- ID13 blank fronts;
- Expression Engine;
- cotas nuevas no pedidas;
- refactor general de UI;
- los hallazgos H1–H7 del Discovery (`CD-09`, sección 13);
- modificar `RACKLAYOUT` (confirmación del Coordinador): la variación de huella al ocultar las cotas de
  Planta es consecuencia explícita de ADR-0035 y punto de Owner Validation, y no se corrige.

## 8. Tests mínimos de aceptación

- default legacy;
- Front ON / Side OFF / Plan OFF;
- Front OFF / Side ON;
- combinaciones relevantes por sistema;
- insert;
- update;
- RACKEDITAR;
- save/reload;
- linked sibling views;
- nueva vista creada después del cambio;
- cada sistema incluido;
- no cross-write entre vistas;
- no pérdida de geometría ni BOM.

## 9. Owner Validation

AutoCAD 2025, por cada sistema acordado: crear rack, elegir cotas por vista, insertar, verificar solo vistas seleccionadas, `RACKEDITAR`/Actualizar, save/reopen, insertar vista adicional y smoke legacy.

Además, por confirmación del Coordinador: la **variación de huella de `RACKLAYOUT`** cuando se ocultan las cotas de Planta se verifica como consecuencia aceptada, sin corregirla.

## 10. Gates

| Gate | Entregable | Estado |
|---|---|---|
| G0 | Preflight, claim, worktree, bootstrap | **CLOSED** — reclamo `97cc0ef`, contrato `4e22d91`, fila de ROADMAP `a2fba4f`, worktree creado |
| G1 | Characterization / matriz completa | **CLOSED** — [I-50-discovery.md](I-50-discovery.md); decisiones del Coordinador en la sección 12 |
| G2 | Contrato de autoridad + persistencia + default legacy | **CLOSED** — congelado en la [Proposal V1.2](I-50-proposal-v1.2.md) (`0e91c52`) |
| G2A | Proposal + ADR `propuesto` (solo documentación) | **CLOSED** — Proposal **V1.2 FROZEN** (`0e91c52f41f26b3302af8cabd9dd8b293f2d3cbd`): Coordinator **AGREED**, Architect **AGREED** (MAT-A1 **RESOLVED**), Consensus **REACHED**; [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md) **`aceptado`** por el Owner el 2026-09-12 ([decisión](../automation/decisions/I-50.md)). Historial: V1 (`93c352a`) Coordinator **NOT AGREED**; V1.1 (`0d7670c`) Coordinator **AGREED**, Architect **NOT AGREED** (MAT-A1) |
| G3 | UI mínima | **CLOSED** — `ed50cbd` Selectivo (`C-01`, T-17), `2765a45` Dinámico (`C-07`, T-18) y `6cd2970` Push Back (`C-12`, T-19), con T-20 y T-21 en las tres ventanas |
| G4 | Builders / draw path | **CLOSED** — `974c709` caracterización T-03..T-05 (solo pruebas), `fb039e1` modelo y regla (T-01, T-02) y `11c04db` emisores y `C-05` (T-06, T-07, T-09, T-10) |
| G5 | Persistence / update | **CLOSED** — `94220fb` Selectivo (`C-02`..`C-04`, `C-06`; CI rojo: desviación **G5-01**, sección 16), `695d34b` corrección test-only, `7a9335c` Dinámico (`C-08`..`C-11`) y `a4806ff` Push Back (`C-13`, `C-14`); T-11..T-15 y los extremo a extremo de T-06 y T-07 |
| G6 | Cobertura de sistemas | **CLOSED** — `8ceb3a7`: `C-15` (Push Back compuesto A/B), T-08 y T-16 |
| G7 | Candidate + Owner AutoCAD 2025 | **CLOSED** — Candidate `6cd2970` **PASS** automatizado; Owner Validation **PASS** en AutoCAD 2025 (OV-1..OV-10) |
| G8 | Docs + integration + cleanup | **IN PROGRESS** — cierre documental; merge `--no-ff` sin rebase final, compuertas posteriores al merge y limpieza pendientes |

**No implementation before G1/G2.** G3+ queda bloqueado hasta cerrar el contrato y, si aparece una decisión arquitectónica transversal material, hasta la revisión de Arquitecto correspondiente. **Cerrar G1 no autoriza implementar**: G3+ sigue bloqueado hasta que el Coordinador y el Arquitecto revisen la Proposal.

**Orden de ejecución aprobado por el Coordinador** (revisión de V1): **G4 → G5 → G6 → G3 → G7 → G8**. Se conserva la numeración de la tabla; solo cambia el orden en que se abren.

Cada SHA de gate se registra en un commit **posterior** al que lo produce: un documento no puede contener el SHA
del commit que lo crea. Por eso el SHA de este cierre documental no figura aquí, y el `MERGE_SHA` todavía no existe.

**Contenido y cierre de cada gate** (archivos, sitios de copia C-xx y pruebas o validaciones que lo cierran): tabla de la [Proposal V1.2](I-50-proposal-v1.2.md), sección 14.3 (MAT-A1). El orden no cambia.

**Compuerta de código productivo**: consenso del Coordinador y del Arquitecto sobre la **misma** versión de la Proposal **y** ADR-0035 `aceptado` por el Owner (sección 14). Faltando cualquiera de las dos, no se escribe producción.

> **SATISFECHA para la Proposal V1.2 exacta** (`0e91c52f41f26b3302af8cabd9dd8b293f2d3cbd`): Coordinator
> **AGREED**, Architect **AGREED** y ADR-0035 **`aceptado`** por el Owner el 2026-09-12
> ([evidencia](../automation/decisions/I-50.md)).
>
> **La regla no se retira: sigue vigente hacia adelante.** Lo satisfecho es la compuerta **para esa versión y
> ese SHA**:
>
> ```text
> Cualquier cambio contractual posterior a Proposal V1.2
>   → invalida este consenso
>   → exige nueva revisión Coordinator + Architect
>   → y nueva decisión del Owner si cambia materialmente lo aceptado.
> ```
>
> Por eso [I-50-proposal-v1.2.md](I-50-proposal-v1.2.md) es **INMUTABLE**. Los MINOR editoriales m1 y m2 de
> la re-revisión del Arquitecto no la alteran.

## 11. Coordinación con I-49 e I-51

**Sin conflicto declarado** (`CD-10`, `CD-11`). `conflicts_with` queda vacío y la fila propia de I-50 en
`docs/ROADMAP.md` ya no lista I-49 en «Se estorba con».

**I-49** (`architecture/motor-expresiones-parametricas`) corre en paralelo bajo
`OWNER_OVERRIDE_I49_I50_PARALLEL`, registrado en `docs/automation/decisions/I-49.md` de **su** rama. Ese
registro asigna a I-50 la corrección documental que hace este contrato. Reglas vigentes para I-50:

- **Coordinación obligatoria por archivos compartidos**:
  `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml` y `RackSelectiveWindow.xaml.cs` —cruce
  material: los controles de cotas y los `LinkedPropertyEditor` conviven en la misma sección y en
  `BuildDesign`/`LoadDesign` ([I-50-discovery.md](I-50-discovery.md) §11.2)— y, posiblemente,
  `src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs`.
- **Protocolo antes de editar uno de esos archivos**, mientras I-49 tenga rama remota no integrada:
  1. `git fetch --all --prune`;
  2. `git diff --name-only origin/main...origin/architecture/motor-expresiones-parametricas` y, si el
     archivo aparece, su diff concreto;
  3. si I-49 lo modifica **materialmente**: **detenerse antes de editar** y reportar el archivo, el SHA
     observado de I-49 y la naturaleza del cruce.
- No refactorizar `LinkedPropertyEditor`; no tocar Project Variables ni Expression Engine salvo necesidad
  real demostrada: son alcance funcional de I-49.
- Commits pequeños.
- Integración **serializada** con I-49: la segunda en integrar se reconcilia con `main` en su rebase final.

**I-51** (`feature/rackduplicar-multiples-origenes`) solo tiene un conflicto **documental**: su fila de
ROADMAP se inserta en el mismo punto que la de I-50. **No es `conflicts_with`**. I-50 no prevé tocar
`RackDuplicarCommands.cs` ni `RackEnvelopeRestamp.cs`, y la política de I-50 vive en el **diseño**, no en
la referencia ni en el sobre, que es lo que la decisión AM-4 de I-51 necesitaba saber.

`docs/ROADMAP.md` y `docs/ideas-futuras.md` pueden chocar textualmente con I-49 e I-51 al integrar: quien
integre después conserva todas las filas y secciones.

## 12. Decisiones vinculantes del Coordinador (2026-09-12, cierre de G1)

| ID | Decisión |
|---|---|
| `CD-01` | Autoridad = **rack × tipo de vista**, NO instancia. |
| `CD-02` | Tipos de vista de I-50 = **Frontal / Lateral / Planta**. En el Dinámico, salida y entrada comparten Frontal. En Push Back, todos sus cortes frontales comparten Frontal. `Section` **no** crea un tipo de vista. |
| `CD-03` | `DimensionDetail` sigue siendo global del rack. |
| `CD-04` | `DimensionStyle` sigue siendo global del rack. |
| `CD-05` | La visibilidad es **solo ON/OFF** por tipo de vista. |
| `CD-06` | `Dimensions = None` **siempre gana** y produce cero cotas. |
| `CD-07` | Política nueva nula o ausente = **LEGACY exacto**: todas las vistas históricamente elegibles usan `Dimensions` como hoy. No se materializa «todas» al guardar un rack legacy sin tocar las cotas. |
| `CD-08` | Metadata por instancia **RECHAZADA**. |
| `CD-09` | H1 (estilo de cota de Push Back) y H2–H7 quedan **fuera de alcance**. |
| `CD-10` | I-49 **debe** poder correr en paralelo: se quita `conflicts_with: [I-49]` de este contrato y «I-49» de «Se estorba con» en la fila propia de I-50. Se mantiene la coordinación obligatoria por `RackSelectiveWindow.xaml/.cs` y el posible `SelectivePalletDesignDocument`. |
| `CD-11` | I-51 solo tiene conflicto documental de ROADMAP; no es `conflicts_with`. |

Lo que queda para G2: la **representación** del dato. La Proposal V1 compara solo dos formas —una lista
nula de tokens de vista y un `[Flags] DimensionViewVisibility` nulo— y recomienda una.

> **Resuelto en G2A.** Representación B: `[Flags] DimensionViewVisibility` nulo, persistido como
> `int? DimensionViews`. Queda congelada en la [Proposal V1.2](I-50-proposal-v1.2.md) y decidida en
> [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md), `aceptado`.

## 13. Hallazgos fuera de alcance (`CD-09`)

Registrados **sin corregir** en [ideas-futuras.md](../ideas-futuras.md), sección «I-50»; el detalle y la
evidencia están en [I-50-discovery.md](I-50-discovery.md) §13.

| ID | Hallazgo |
|---|---|
| H1 | Push Back no tiene control de estilo de cota y cada recálculo escribe `DimensionStyle = null` |
| H2 | El lateral de Push Back envía la posición en la lista de cortes y el Plugin la lee como índice de poste |
| H3 | Los bloques anónimos `*D` de las cotas no se encolan para purga al cancelar una inserción ni en `EraseViewBlocks` |
| H4 | `HeaderRunPlan.PlacedClone` no copia los campos de texto ni de cota |
| H5 | El Dinámico descarta un estilo de cota guardado que el DWG no tiene; el Selectivo lo conserva |
| H6 | El BOM del Selectivo calcula las cotas laterales y las descarta |
| H7 | Tres comentarios desfasados (`RackBlockData.cs:8`, `RackInsertionRequest.cs:152`, `PushBackSystemLateralBuilder.cs:147-151`) |

## 14. Decisión del Owner requerida

`requires_owner_decision: true` desde G2A ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) sección 11: un ADR
nuevo permanece `propuesto` hasta que el dueño lo acepta o lo rechaza).

- **Decisión**: aceptar o rechazar [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md).
- **Punto en que se necesita — CONFIRMADO por el Coordinador**: ADR-0035 debe estar `aceptado` por el Owner
  **antes de cualquier código productivo** de I-50, junto con el consenso del Coordinador y del Arquitecto
  sobre la misma versión de la Proposal (precedente: I-47, CF-2).
- Ninguna de las dos compuertas sustituye a la otra: el consenso técnico dice que la decisión está lista; la
  aceptación la ejerce solo el Owner.

> **SATISFECHA el 2026-09-12.** El Owner aceptó expresamente ADR-0035 sobre la Proposal V1.2:
> «**Sí, acepto ADR-0035 para I-50 sobre Proposal V1.2.**» Registro durable en
> [`docs/automation/decisions/I-50.md`](../automation/decisions/I-50.md), enlazado desde `decision_paths`.
>
> ```text
> Owner Decision      = ACCEPTED
> ADR-0035            = ACCEPTED
> owner-decision gate = RESOLVED
> ```
>
> **`requires_owner_decision` se conserva en `true`**, como en I-47 e I-48: declara que la iniciativa requirió
> una decisión del dueño, y no pasa a `false` porque esa decisión quede resuelta; lo que cambia es el estado
> del gate. **`requires_owner_validation` sigue pendiente** (G7, AutoCAD 2025) y es monotónica
> ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) sección 11). `status` se conserva en `claimed`: G4 no está
> iniciado.
>
> **Nota de G8 (2026-09-12).** La Owner Validation de G7 quedó en **PASS** y `status` pasa a `integrated` con el
> cierre documental. `requires_owner_decision` y `requires_owner_validation` se conservan en `true`: declaran lo
> que la iniciativa requirió, no lo que queda pendiente ([decisión](../automation/decisions/I-50.md) §9).

## 15. Historial de revisión de la Proposal

| Versión | Archivo | Revisión del Coordinador | Revisión del Arquitecto |
|---|---|---|---|
| V1 | [I-50-proposal-v1.md](I-50-proposal-v1.md) (`93c352a`), conservada **intacta** | **NOT AGREED**. MATERIAL-01: retirar «entero negativo ⇒ legacy» y conservar exactamente todo entero presente. MATERIAL-02: retirar la guarda de texto T-22 del Plugin. Confirmó el resto (representación B, sin `All`, `null` = legacy, orden de gates, `requires_owner_decision`, ADR aceptado antes de código, I-50 no modifica `RACKLAYOUT`) | no revisada |
| V1.1 | [I-50-proposal-v1.1.md](I-50-proposal-v1.1.md) (`0d7670c`), conservada **intacta** | **AGREED** | **NOT AGREED**. Único MATERIAL: **MAT-A1**, ejecutabilidad de los gates (faltaba el contenido y cierre de cada gate; T-06 dependía de C-05 y T-08 de C-10/C-14/C-15). MINOR MIN-1..MIN-9. Respondió QA-1, QA-3, QA-4 y QA-5 con AGREE. **El diseño de datos no se reabrió** |
| V1.2 | [I-50-proposal-v1.2.md](I-50-proposal-v1.2.md) (`0e91c52`), **FROZEN** | **AGREED** | **AGREED**. **MAT-A1 RESOLVED**; 0 BLOCKER, 0 MATERIAL; G4, G5, G6, G3 y G7 ejecutables. MINOR editoriales: m1 (matriz de trazabilidad, 13.6 A) y m2 (suite de T-20), que no alteran la Proposal congelada, y m3 (estado atrasado), corregido en este contrato, en ADR-0035 y en el registro de decisión |

**Consenso**: **REACHED** sobre V1.2 (`0e91c52f41f26b3302af8cabd9dd8b293f2d3cbd`), y **ADR-0035 `aceptado`** por
el Owner el 2026-09-12 ([decisión](../automation/decisions/I-50.md)). V1.2 hereda íntegramente el contrato técnico
de V1.1 e incorpora MAT-A1 (tabla de gates, con C-05 adelantado a G4) y MIN-1..MIN-8; MIN-9 se aplicó a
ADR-0035. **V1.2 queda congelada**: su cabecera conserva el estado con que se publicó (PENDING), y el estado
vigente lo registran este contrato, ADR-0035 y el registro de decisión.

## 16. Evidencia de implementación y cierre (G4 → G8)

Los SHAs completos están en el bloque del encabezado; los conteos de pruebas, solo en [HANDOFF](../HANDOFF.md)
§5. Cada commit de producto se vio **en rojo** antes de su cambio y en verde antes del commit, y tiene CI de
`push` sobre su SHA exacto, verde en todos salvo `94220fb` (desviación G5-01, abajo); el detalle de cada rojo vive
en el cuerpo de su commit.

**G4 — modelo, regla y emisores.** `974c709` caracteriza el legacy de cotas de los tres sistemas (T-03..T-05)
sobre producción intacta y fija sus pines, que **no** se re-fijaron en ningún gate posterior. `fb039e1` crea
`DimensionViewVisibility` y la regla única `DimensionViewPolicy` (T-01, T-02). `11c04db` hace que los emisores
dibujen con el detalle efectivo de cada vista y cablea `C-05` (T-06, T-07, T-09 y la guarda T-10: `None` gana).

**G5 — persistencia y actualización.** `94220fb` lleva la política por el Selectivo (`C-02`, `C-03`, `C-04`,
`C-06`), `7a9335c` por el Dinámico (`C-08`..`C-11`) y `a4806ff` por Push Back (`C-13`, `C-14`), con T-11..T-15 y
los extremo a extremo de T-06 y T-07: nulo sigue nulo, todo entero presente llega exacto, y `RACKEDITAR`,
guardar y reabrir, las vistas enlazadas y el re-estampado de `RACKDUPLICAR` lo conservan.

```text
PROCESS_DEVIATION I-50/G5-01

SHA:
94220fbb73c51f6b844713e1e89f5bea8a8d7fca

Hecho:
un SHA fue publicado antes de conocer que CI fallaba.

Fallo:
2 pruebas de pines JSON por CRLF Windows vs LF Linux.

Impacto:
test portability only; no defecto productivo demostrado.

Corrección:
695d34b, test-only, normalización CRLF→LF antes del hash.

Historia:
preservada; sin force-push.

Candidate final:
no afectado.
```

Es el único SHA de la rama con CI rojo (corrida **34724255311**); `695d34b` y todos los push posteriores quedaron
en `success`. La desviación es de proceso —publicar antes de conocer el CI—, no de producto.

**G6 — Push Back compuesto.** `8ceb3a7`: `C-15`, en `PushBackCompositeStructure.CopySharedStructuralIntent`, lleva
la política compartida a los lados A y B, y los cuatro cortes frontales siguen Frontal. T-08 se vio en rojo sobre
producción intacta antes de `C-15`; T-16 fija que la geometría física y el BOM **no** dependen de la política.

**G3 — editores.** `ed50cbd` Selectivo (`C-01`, T-17), `2765a45` Dinámico (`C-07`, T-18) y `6cd2970` Push Back
(`C-12`, T-19), con T-20 (superficie) y T-21 (dibujo por el «Actualizar» real). Cada ventana se vio en rojo sobre
su superficie inerte, y una prueba de sensibilidad demostró que la supresión de carga es lo que impide
materializar `7` en un rack legacy. H1 quedó caracterizado, no corregido. I-49 se revalidó antes de tocar el
Selectivo y de nuevo antes del último commit de G3: solo documentación, sin tocar las ventanas.

**G7 — Candidate y Owner Validation.** Candidate `6cd2970c36a906cb9e784797008bed5e8111e433`, sin commits
posteriores: suites Core y UI completas y focales en local, build Debug de UI y de Plugin, CE-01 vacío, CI de
`push` 4/4 sobre el SHA exacto (corrida **34732123814**) y cobertura del Candidate por dispatch (corrida
**34733326111**, 4/4: objetivo y checkout verificado = el Candidate; artifact `rackcad-coverage-cobertura`
presente). **Automated Candidate = PASS.** Owner Validation en AutoCAD 2025, sobre el DLL Debug construido desde
el Candidate: **PASS**, OV-1..OV-10 ([decisión](../automation/decisions/I-50.md) §9).

**G8 — cierre e integración.** Preflight: la rama es el Candidate, con el árbol limpio; `origin/main` = `46fcac2`,
nueve commits de I-51 sobre `BASE_SHA` cuyos archivos productivos (`RackDuplicationPlan.cs`,
`RackDuplicarCommands.cs`, `RackEnvelopeRestamp.cs` y tres archivos de pruebas) **no** intersecan los de I-50.
Guard de interacción: sin solape material con `DimensionViews`, que vive en el diseño y sobrevive al re-estampado
(T-15). Por orden del Coordinador, la integración es un merge `--no-ff` **sin rebase final**, con guard de
producto y validación local del árbol combinado antes del push de `main`, y sin repetir la Owner Validation
mientras no haga falta una resolución productiva ([decisión](../automation/decisions/I-50.md) §9). Este cierre es
solo documentación.
