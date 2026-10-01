# I-61 — Discovery G1: protocolo de ejecución de agentes

Unit: `I-61`. Workflow: V2 (T4). Rama: `architecture/protocolo-ejecucion-agentes`. Claim-Id: `0e2923de-e1a7-41bf-b7db-50ec84217850`.
Contrato: [I-61-protocolo-ejecucion-agentes.md](I-61-protocolo-ejecucion-agentes.md). Mandato (fuente del alcance):
[I-61-owner-mandate.txt](../automation/decisions/I-61-owner-mandate.txt). Decisiones: [I-61.md](../automation/decisions/I-61.md).
Evidencia: [I-61-evidence.md](../automation/evidence/I-61-evidence.md).

**Naturaleza de este documento.** Discovery de solo lectura ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4). **No** es una Proposal, un Freeze,
una revisión del Architect ni un GATE PASS de G1. No implementa scripts, esquemas operativos, catálogo ni piloto. Toda afirmación lleva una etiqueta:
**MEASURED** (observada en esta sesión con un comando o lectura citable), **RECONSTRUCTED** (derivada de artefactos históricos o de varias lecturas),
**INFERENCE** (razonamiento propio sobre hechos medidos) o **UNKNOWN** (no determinable con la evidencia actual). Los antecedentes de D0/G0 se
citan como tales; las observaciones de D0 que se repiten aquí fueron **re-medidas** y se marca su fecha.

Base del Discovery: `origin/main` `95690c28dc6268e61dff32a0cbc33cc9fde3d47f`; rama de I-61 en `6f1ef9815ffa20f8bea946c55622efa165994f7c` (SHA revisado por el
Coordinator para G0). Fecha de las mediciones: 2026-09-30.

## 0. Preflight delta (MEASURED)

| Hecho | Valor | Cambio frente a G0 |
|---|---|---|
| `HEAD` = `origin/architecture/protocolo-ejecucion-agentes` | `6f1ef981…`, árbol limpio, sin operaciones Git en curso | ninguno |
| `origin/main` | `95690c28…` (0 detrás, 3 delante para la rama) | ninguno |
| Rama de I-52 | `65e465a71e8e91d60fc9315dace1aac55c0db5de` (0 detrás, 140 delante de main) | ninguno |
| Worktrees | principal, I-52 (ajeno, intacto) y el de I-61 | ninguno |
| Ownership | el worktree de I-61 lo usa solo esta sesión; ningún escritor ajeno en archivos de I-61 | ninguno |
| Diff de I-52 frente a documentos de proceso | nada en `AGENTS.md`, `WORKFLOW.md`, `AUTOMATION_PLAN.md`, `INITIATIVE_LIFECYCLE.md`, `FOUNDATIONS.md`, `PROMPT_TEMPLATES.md`, `context-packs/`, `guias/`, `.gitignore`, `.github/`, `tools/`, `eng/ci`; sí en `ROADMAP.md`, `HANDOFF.md`, `adr/README.md` | ninguno |

## 1. DC-01 — Comportamiento actual de la operación de agentes

**Qué existe hoy (MEASURED salvo indicación):**

- **Un único ejecutor documentado**, no una jerarquía controller→worker: [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) define selección, reclamo atómico, una fase por
  ejecución, archivo de estado, reintentos (§9), paradas (§12) e informe (§14). **No está activo**: el plan lo declara («no existe actualmente una automatización
  nocturna activa», §«Estado actual de activación»), `ci.yml` tiene dos disparadores, `push` y `workflow_dispatch` (este último solo con el input
  `candidate_sha` para medir cobertura de un Candidato; `.github/workflows/ci.yml:15-22`, MEASURED — **corregido en G1-C**: la versión anterior decía «solo `push`»),
  y los scripts de I-56 P-21 «siguen siendo solo diseño». Que `ci.yml` no tenga `schedule` **no** acredita ausencia de recurrencia externa (C61-D0-06); esa parte
  es **UNKNOWN** (no inspeccioné tareas programadas del equipo ni de las herramientas).
- **El desarrollo real es manual**: sesiones de agente relevadas por el humano sobre un mismo worktree, de forma secuencial ([WORKFLOW](../WORKFLOW.md) §3), con
  prompts redactados por el Coordinator y trasladados a mano (las órdenes D0/G0/G1 de esta iniciativa son un ejemplo directo) — **RECONSTRUCTED**.
- **Sin enrutamiento de modelo/effort**: `git grep` de `worker|effort|reasoning|model routing|execution controller|prompt profile|subdeleg` sobre `AGENTS.md`,
  `WORKFLOW.md`, `AUTOMATION_PLAN.md`, `INITIATIVE_LIFECYCLE.md`, `FOUNDATIONS.md`, `PROMPT_TEMPLATES.md`, `context-packs/`, `guias/`, `ARCHITECTURE.md`, `README.md` y
  `CLAUDE.md` no devuelve **nada** (la única mención de modelo es el trailer `Co-Authored-By`, AGENTS.md:95-96 y WORKFLOW.md:32).
- **Relevos e informes actuales** (renombrado en G1-C para no usar «handoff» con un cuarto sentido; ver §18.1): cuerpo de commit, archivo de evidencia, archivo de estado, informe ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §14; [PROMPT_TEMPLATES](PROMPT_TEMPLATES.md) §B
  «Informe», §E). Todos son prosa o YAML libre; **no existe un esquema de delegación ni de handoff legible por máquina**.
- **Esquemas declarados en el repositorio** (MEASURED, `git grep 'schema: rackcad-'`): `rackcad-automation-state/v1` (35 archivos de estado anidados),
  `rackcad-initiative/v1` (58 contratos), `rackcad-initiative/v2` (TEMPLATE, I-58, I-61) y `rackcad-context-pack/v1` (9). El conteo «41» de D0 incluía
  menciones en otros documentos; el correcto para estados es **36 archivos: 35 anidados + 1 plano (I-58)**.

**Permisos y paradas vigentes:** [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §3 (sin merge, sin force salvo `--force-with-lease` tras el rebase, sin decisiones reservadas) y §12
(condiciones de parada); [WORKFLOW](../WORKFLOW.md) §§2-4; `AGENTS.md` (identidad de evidencia exact-SHA, trailer `Co-Authored-By`).

## 2. DC-02 — Dueños de reglas y valores

| Regla / valor | Dueño acreditado | Fuente |
|---|---|---|
| Git, reclamo, worktrees, integración, transición | `WORKFLOW.md` | WORKFLOW §10 |
| Composición Full, evidencia, exact-SHA | `AGENTS.md` | WORKFLOW §10 |
| Ciclo de diseño, Discovery, Freeze, READY | `INITIATIVE_LIFECYCLE.md` | WORKFLOW §10 |
| Prompts | `PROMPT_TEMPLATES.md` (subordinada a los anteriores) | WORKFLOW §10 «Prompts» |
| Selección y operación del ejecutor (estado, intentos, paradas, informe) | `AUTOMATION_PLAN.md` | AUTOMATION_PLAN §«Este documento define solamente el ejecutor» y §2 «Este plan solo decide selección y operación del ejecutor» |
| Decisiones del Owner | `docs/automation/decisions/<I>.md` | WORKFLOW §10 |
| Arquitectura | `AGENTS.md`, ADR aceptado, Freeze+A-n | WORKFLOW §10 |
| Enrutamiento modelo/effort, catálogo de modelos, esquemas de delegación/handoff, responsabilidades de un Execution Controller | **no existe un artefacto** (esquema, catálogo, perfil) para estos mecanismos; **sí existe autoridad operativa** que los gobierna: operación del ejecutor → `AUTOMATION_PLAN`, relevo y Git → `WORKFLOW`, prompting → `PROMPT_TEMPLATES`, evidencia → `AGENTS` (decisión Q-03 del Coordinator) | WORKFLOW §10; AUTOMATION_PLAN §2 |

**Corregido en G1-C:** la versión anterior decía «ninguno existe hoy» y confundía la ausencia de un **esquema o artefacto nuevo** con la ausencia de **autoridad operativa**.
Los artefactos que cree I-61 serán documentos **subordinados** a esas autoridades, no autoridades paralelas (Q-03).

## 3. DC-03 — Persistencia, campos y legado

- **Persistido y versionado:** `docs/automation/state/<I>.yml`, `decisions/<I>.md`, `evidence/<unit>-evidence.md`, contratos de `docs/initiatives/`, tags `integration/*` ([WORKFLOW](../WORKFLOW.md) §11.4, §11.6).
- **Estado:** el esquema mínimo de [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §8 (`initiative, branch, claim_id, current_phase, state, gate, attempts, next_action, last_evidence_commit`).
  En la práctica 21 de 36 archivos añaden `execution_context` y otros campos libres (`gates`, `owner_validation`, `evidence`, `preflight`, …) y I-58 usa una forma plana sin `schema`
  (MEASURED). **No hay validador ni política de campos desconocidos** para estos YAML (DC-05/DC-06).
- **Tráfico transitorio hoy:** no existe ubicación normativa. `docs/automation/runs/` contiene 8 bitácoras históricas (I-06, I-26, I-30, I-31) — **RECONSTRUCTED** como precedente de registro de ejecución versionado, no como
  canal de mensajes.
- **Ubicación candidata del tráfico transitorio (auditoría pedida por el mandato, MEASURED con `git check-ignore`):**

  | Ruta | ¿Ignorada por Git? |
  |---|---|
  | `artifacts/orchestration/I-61/t1/delegation.json` | **sí**, por `.gitignore:3` (`artifacts/`); hoy contiene `TestResults` y `coverage` |
  | `.agent/orchestration/I-61/t1/delegation.json` | **no** (`git check-ignore` sin coincidencia); añadirla exigiría editar `.gitignore` |
  | `.claude/worktrees/…` | sí, pero solo por `.git/info/exclude` **local** (no versionado) |

  Nota: `git check-ignore -v .agent/` (con la barra final) imprime una coincidencia en `.gitignore:13` con patrón vacío (línea en blanco); es una peculiaridad de la salida y no cambia el resultado para
  los archivos, que es el que importa. **No se adopta `.agent/` por analogía** (mandato); la decisión quedaba para el diseño (texto de G1; **superado**: D-03 eligió `artifacts/orchestration/<unit>/<task>/<attempt>/`, ya ignorada).
- **Bytes exactos:** el repositorio tiene `core.autocrlf=true`; hay precedentes de `-text` en `.gitattributes` para artefactos de evidencia JSON (p. ej. `I-52-AUTH15-run3/*.json`, **MEASURED**). Un handoff versionado como
  evidencia heredaría el problema de fin de línea que ya afectó al mandato (hallazgo fuera de alcance de G0).

## 4. DC-04 — Camino entrada → estado → persistencia → salida

**RECONSTRUCTED (observado en el desarrollo de I-60 e I-61):** orden del Coordinator (chat, trasladada a mano) → sesión Executor lee autoridades → worktree de la iniciativa → commits/push → CI sobre el SHA
exacto → evidencia/estado versionados → informe al Coordinator → **GATE PASS solo del Coordinator**. El Architect interviene antes del Freeze ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §5). El punto de fricción que motiva el mandato es el
traslado manual de la orden y del informe (mandato, «MOTIVATION» 3-4); **no medí tiempos ni costes** de ese traslado (**UNKNOWN**; métricas aún no disponibles).

## 5. DC-05 — Llamadores y consumidores

- **Lectores en código:** ninguno. `git grep` de `automation/(state|decisions|evidence)|automation_state|max_attempts|claim_id` fuera de `docs/` y de `*.md` solo devuelve líneas de `.gitattributes` (MEASURED).
- **Consumidores documentales:** `AUTOMATION_PLAN` se referencia en 40 archivos rastreados (excluido `docs/archivo`), `PROMPT_TEMPLATES` en 16 y `automation.enabled|max_attempts` aparecen en 71; **13 contratos V1 más `TEMPLATE.md`** (corregido en G1-C: la cifra «14» contaba la plantilla) tienen
  `automation.enabled: true` (MEASURED). **Riesgo (INFERENCE):** un controller que aplique literalmente [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §5 podría tratar esos 13 contratos como elegibles (en G1-C se midió que hoy ninguno lo es: §18.3); la deuda preexiste y
  se vería agravada por cualquier automatización real (EXP-08).
- **Consumidores futuros:** el criterio de éxito 14 del mandato (uso por una futura iniciativa V2 «sin mega-prompt») crea consumidores que hoy no existen (EXP-04).
- **Huecos de búsqueda (UNKNOWN):** configuración de usuario de Codex/Claude fuera del repositorio más allá de claves de modelo leídas en §8; lectores dentro de las conversaciones de Coordinator/Architect (no inspeccionables).

## 6. DC-06 — Pruebas y guardas

- **Ninguna prueba ni guarda** lee `docs/automation/state`, `docs/initiatives`, `AUTOMATION_PLAN` o `PROMPT_TEMPLATES` (MEASURED: las coincidencias en `tests/` son comentarios incidentales de I-16/I-60).
- CI = 4 jobs (Core, UI, Build UI, Build Plugin sin AutoCAD); no hay «docs lint». La comprobación «solo documentación» es un comando manual ([WORKFLOW](../WORKFLOW.md) §4.5.4).
- **Consecuencia:** todo invariante de proceso candidato a Freeze (exact-SHA, propiedad exclusiva de escritura, worker sin auto-aprobación, parada/rework determinista) **no tiene prueba protectora ni forma ya conocida de
  probarse** → activa EXP-05 (§11).

## 7. DC-07 — Archivos calientes e intersecciones activas

| Archivo / zona | Quién más lo toca hoy (MEASURED) | Comentario |
|---|---|---|
| `docs/ROADMAP.md` | I-52 (líneas propias de su fila y la fila de I-57, según D0) | I-61 ya escribió su fila bajo acuse (§3 de la evidencia); toda edición futura exige nueva coordinación |
| `docs/HANDOFF.md`, `docs/adr/README.md` | I-52 | I-61 solo los tocaría al cerrar/integrar o si nace un ADR |
| `AGENTS.md`, `WORKFLOW.md`, `AUTOMATION_PLAN.md`, `INITIATIVE_LIFECYCLE.md`, `PROMPT_TEMPLATES.md`, `FOUNDATIONS.md` | nadie (I-52 no los modifica) | candidatos a modificación por I-61; no son «archivos calientes» de WORKFLOW §7 pero son **normas globales** (autoridad de dominio: Coordinator/Owner) |
| `.gitignore` | nadie | solo si se elige una ruta transitoria no ignorada (p. ej. `.agent/`) |
| Piloto Cama: `src/RackCad.Plugin/RackCamaCommands.cs`, `src/RackCad.UI/Systems/FlowBed/RackFlowBedWindow.xaml.cs`, `…/KindHandlers/CamaKindHandler.cs` | ninguna rama activa (I-52 no toca `src/` de Cama) | `src/RackCad.Plugin/*Commands*.cs` es archivo caliente declarado (WORKFLOW §7) |

## 8. DC-08 — Fundaciones consumidas o extendidas

- `FOUNDATIONS.md` tiene **12 entradas** (MEASURED): Rack Identity, View Identity, RACKDUPLICAR / Restamp Identity, Project Variables, Shared View Foundation (AUTH-13 y AUTH-08/12), Authored vs Effective,
  Custom Properties, DimensionViews, Header Mutation / Reconciliation, Unknown-field Preservation in Persisted Envelopes y Linked Properties. **Ninguna describe ejecución de agentes, prompts, enrutamiento ni handoff.**
- *(Texto de G1, superado por §20.3 y §19.1 en G1-C.)* Por tanto el **proceso** de I-61 no consume ni extiende ninguna fundación; introduce mecanismos nuevos (DC-09). `Consumes/Extends` queda `none` **para el proceso**.
- **El piloto sí podría consumir** Rack Identity / View Identity / Unknown-field Preservation / Custom Properties (la ruta de edición de Cama compone el sobre con `RackEmbedComposer.Compose`). Esa verificación DC-08 se hace
  en el Discovery **delta del piloto**, sobre su base, no aquí.

## 9. DC-09 y EXP-09 — Materialidad y arquetipo

**Pregunta EXP-09:** ¿I-61 **crea** o **modifica**, y qué M activa? (M-07 estaba UNKNOWN.) Área acotada: la infraestructura de ejecución existente (DC-01/02) y los cambios mínimos que exige el mandato.

**Resolución (corregida en G1-C; la versión anterior partía de «no existe autoridad alguna», premisa retirada):** I-61 **crea artefactos nuevos subordinados** y
**modifica autoridades existentes**. Hecho (MEASURED): hoy no existe ningún artefacto (esquema, catálogo, perfil, documento de protocolo) para delegación, entrega de
worker, enrutamiento ni catálogo. Decisión (Q-03, Coordinator): la **autoridad operativa ya existe** (operación del ejecutor → `AUTOMATION_PLAN`; relevo y Git →
`WORKFLOW`; prompting → `PROMPT_TEMPLATES`; evidencia → `AGENTS`), así que los artefactos nuevos son **extensiones subordinadas**, no autoridades nuevas. El mandato exige
crearlos (entregables B, D, E, G) y, con ellos, modificar `PROMPT_TEMPLATES` (sección nueva), `AUTOMATION_PLAN` (ejecución delegada) y la tabla de `WORKFLOW` §10. No
exige tocar `.gitignore` (D-03 eligió una ruta ya ignorada).

| M | Estado | Evidencia / condición |
|---|---|---|
| M-01 (dueño/segunda autoridad) | **ACTIVADO** (actualizado en G1-C) | Ya hay dueños del presupuesto de reintentos (`attempts`/`max_attempts`, AUTOMATION_PLAN §§8-9), de las paradas (§12) y del informe (§14). El mandato añade `MAX_REWORK_LOOPS=3` con «misma clase de fallo», su lista STOP y la entrega de worker. D-04 fija `attempts` como canónico, pero **no** fija aún la regla de conteo, su ámbito ni la precedencia entre ambos topes (§20, pregunta al Freeze); hasta que el Freeze la fije, el riesgo de segunda autoridad sigue abierto |
| M-02 (esquema/DTO/wire) | **ACTIVADO (creador)** | Esquemas nuevos de delegación, entrega de worker y verificación (mandato D, E). No se extiende `automation_state` v1 (35 archivos anidados sin política de campos desconocidos). Para el piloto: **no activado** (no cambia el esquema del sobre; ver §19) |
| M-03 (comportamiento observable/trato de documentos existentes) | **NO activado** (actualizado en G1-C) | Infraestructura: no reescribe contratos ni registros existentes. Piloto: cambia el comportamiento de `RACKEDITAR` de Cama ante un nombre en blanco, pero **es lo pedido** (mandato «preserve logical Name across edit/update/save/reopen»); M-03 solo se activa por cambios «fuera de lo pedido». La intención de producto está respondida por el Owner (Q-06, §15) |
| M-04 (qué falla y cómo) | **ACTIVADO (creador de semántica)** | Un protocolo con STOP, rework y handoff define semántica de fallo nueva; además hay modos de fallo silencioso documentados en los transportes (EXP-06) |
| M-05 (contrato consumido por otros) | **ACTIVADO** (reevaluado en G1-C tras EXP-04) | EXP-04 (§18.1) mostró consumidores reales a más de un salto de las superficies que se modifican: `PROMPT_TEMPLATES` (WORKFLOW §10, LIFECYCLE, packs, órdenes), `AUTOMATION_PLAN` (packs, `CLAUDE.md` vía packs, `initiatives/README`, estados) y `WORKFLOW` §10. Añadir secciones consumidas por otros cambia contratos consumidos |
| M-06 (punto de extensión reutilizable) | **ACTIVADO** | Perfiles de prompt y catálogo de modelos son puntos de extensión reutilizables por diseño (mandato C, B) |
| M-07 (mecanismo genérico transversal) | **ACTIVADO** | Política de enrutamiento + catálogo + esquemas reutilizables por «una futura iniciativa V2» (éxito 14) son un mecanismo transversal. El matiz sobre su aplicación a artefactos de proceso quedó **decidido por el Coordinator (Q-01: NEW ARCHITECTURE confirmado)** |
| M-08 (contradice ADR aceptado) | **NO activado** por el plan actual | ADR-0045 (aceptado) fija controles manuales hasta «tooling futuro» y prohíbe merge/rollback automático, Quick CI y T0-T4/R0-R4; el mandato es compatible. ADR-0033 sigue `propuesto` (no es autoridad). Reevaluar en el Freeze |

**Arquetipo: NEW ARCHITECTURE, confirmado por el Coordinator (Q-01)** ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §3). Tras la corrección de la premisa, el sostén es
**M-07** (mecanismo transversal) y **M-02 creador** (esquemas nuevos); M-01, M-04, M-05 y M-06 también activos. La etiqueta FOUNDATION EVOLUTION del mandato se
conserva como fuente del Owner y no se reescribe (C61-G0-02); la clasificación superior prevalece por la regla «ante duda, el superior». **Estado de EXP-09:** resuelta para
la infraestructura; el delta del piloto se resuelve en §19.

## 10. Piloto — Cama / `RACKEDITAR` / `Name` (caracterización estática; sin reproducción en AutoCAD)

**Cadena entrada → edición → persistencia → reapertura (MEASURED por lectura de código, `95690c28`):**

1. `RACKEDITAR` → `RackMenuCommands.RackEditar` (`RackMenuCommands.cs:153`): elige el bloque, `PickRackBlock` obtiene el sobre `RackEmbedDocument` y despacha por `KindHandlerDispatch` → `CamaKindHandler.Edit` → `RackCamaCommands.EditCama`.
2. `EditCama` (`RackCamaCommands.cs:205`): decodifica el diseño de cama, crea `RackFlowBedWindow` y llama `window.LoadExisting(config, embed.Id, embed.Name, sourceDesign)`.
3. `RackFlowBedWindow.LoadExisting` (`RackFlowBedWindow.xaml.cs:285`): `session.Identity.Adopt(id, name)` y `NameBox.Text = name ?? ""`.
4. Botón «Actualizar en AutoCAD» (`…xaml.cs:276`): `session.Identity.SetName(NameBox.Text.Trim())` y `RequestInsert(…)`; `window.RackName` = `session.Identity.Name`.
5. `BuildCamaPayload(window.FlowBedToInsert, window.RackId, window.RackName, embed, sourceDesign)` → `RackEmbedComposer.Compose(sourceEmbed, Kind, id, **name**, …)` (`RackEmbedComposer.cs:43-60`) asigna `Name = name`.
6. `FlowBedDrawService.RedrawInPlace` → `ViewBlockDraw.RedrawInPlace` → `SystemBlockWriter.RedefineInTransaction` → `RackBlockData.Write(transaction, blockId, payloadJson)` (`SystemBlockWriter.cs:136`) persiste el sobre completo en el Xrecord.
7. Reapertura: `RACKEDITAR` vuelve a leer `embed.Name`.

**Hallazgo de G1 (texto superado por §19 en G1-C):** «el nombre se conserva en toda la cadena mientras el sobre lo traiga y el usuario no lo borre». La lectura siguió el camino
feliz y no comparó con las otras cinco familias; §19 muestra la asimetría concreta (D-1a) y la procedencia de la frase de I-60 (deducción estática, sin reproducción).

**Pruebas protectoras (MEASURED):** `FlowBedEditorWindowTests.LoadExisting_AdoptsDrawnGuidAndName` y `ExistingBed_Insert_ViaButton_KeepsGuidName_…` protegen la ventana (UI). **No hay prueba que cubra `EditCama`/`BuildCamaPayload` conservando el nombre**
(solo guardas de fuente de composición: `CustomPropertiesEnvelopeGuardTests`, `LegacyViewPayloadCompositionCharacterizationTests`, `I60RackNameWiringGuardTests`). **Corregido en G1-C:** un RED
**conductual** del tramo Plugin exige host (el proyecto de pruebas no referencia el Plugin); una guarda de fuente de paridad no lo exige (§19).

**Idoneidad como piloto (texto original de G1, corregido en G1-C):** la versión anterior afirmaba que reproducirlo «probablemente requiere AutoCAD», que una corrección
«en el Plugin» haría la tarea «no pequeña» y que Owner Validation la agrandaba. **Esas conclusiones se retiran**: no estaba demostrado dónde vivía el defecto ni que la
corrección exigiera Plugin o refactor, y Owner Validation no implica por sí sola una tarea grande (decisión Q-02 del Coordinator). La caracterización completa, con el
defecto concreto que la lectura del camino feliz no veía, está en §19.

Pruebas ejecutadas para este Discovery: ver [evidencia](../automation/evidence/I-61-evidence.md) §9.

## 11. Evaluación de EXP-01..09

| EXP | Resultado | Razonamiento y salida |
|---|---|---|
| **EXP-01** (contradicción entre FOUNDATIONS/ADR/Freeze/código) — **autorizada** | **G1-C: sin clase A abierta** (F-01 resuelta sin contradicción residual; F-02 clase B específica; F-06 es condición; ver tabla y §20.2). Texto de G1, superado: «Hallazgos abiertos; ninguno cerrado como B» | Disposición final en la tabla «Cláusulas consumidas» y en C61-G1-08 (§20.2). *(Texto de G1, superado por C61-G1-08: «F-01 … clase A si afecta algo consumido; F-02 … no la cierro como B».)* |
| **EXP-02** (dos dueños incompatibles o ninguno) — **autorizada** | **ACTIVADA y resuelta en G1-C** (la versión de G1 decía «No activada» y razonaba con la premisa retirada de «ninguno»; se retira) | *Operación concreta:* un Controller invocado por la sesión responsable bajo una orden manual del Coordinator. *Dos dueños con consecuencias incompatibles:* si es «el ejecutor» de `AUTOMATION_PLAN`, rigen sus prerrequisitos de activación («Antes de activar cualquier ejecutor…») y §3 («No iniciar trabajo sin … `automation.enabled: true`»), incompatibles con `automation.enabled: false`; si es trabajo manual, rige `WORKFLOW`. *Resolución:* la orden de continuidad fija que `automation.enabled: false` «impide selección/recurrencia automática, no esta sesión manual autorizada», y Q-03 asigna la operación del ejecutor a `AUTOMATION_PLAN`. Decisión del Coordinator C61-G1-02 (§20): la ejecución delegada bajo orden explícita **no** es el ejecutor nocturno (no selecciona, no requiere activación ni `enabled: true`), pero sí aplica las cláusulas de AUTOMATION_PLAN enumeradas en C61-G1-02 (§20.2): §3 salvo la exigencia `automation.enabled: true`, §§8-9, y §12 salvo su primera viñeta (corregido en G1-C). La norma que lo escribe se prepara en `AUTOMATION_PLAN` §16 (diseño) |
| EXP-03 (legado/persistencia no localizable) | No activada | La persistencia está localizada (`docs/automation/*`); no hay lectores ni fallback legacy que rastrear. Se nota la doble forma de estado (35 anidados / 1 plano) sin consumidores |
| EXP-04 (contrato con consumidores a más de un salto) | **ACTIVADA (revisada en G1-C, §18.1)** — el texto siguiente es el de G1, superado | No hay consumidores del contrato de delegación/handoff; se activa al proponer los esquemas (éxito 14). Proponer como expansión del diseño, no de G1 |
| **EXP-05** (invariante sin prueba protectora ni forma conocida de probarlo) | **ACTIVADA — completada en G1-C (§18.2)** | *Disparador:* DC-06. *Pregunta:* ¿qué mecanismo protege los invariantes de proceso (exact-SHA, propiedad exclusiva, sin auto-aprobación, STOP/rework determinista)? Opciones a evaluar: validación de esquema en `tests/`, guardas de fuente sobre documentos, lista de comprobación verificada por el Coordinator. *Área:* `tests/`, CI, `docs/automation/`. *Salida:* invariante → prueba u obligación verificable con RED esperado, o decisión explícita de control manual (ADR-0045 lo admite «hasta que exista tooling futuro»). Autorizada por Q-05 y completada en §18.2 (texto de G1 superado: «Opciones a evaluar», «Requiere autorización») |
| **EXP-06** (semántica de fallo no determinable / fallo silencioso) | **ACTIVADA — completada en G1-C (§18)** | *Evidencia (corregida en G1-C):* según la documentación oficial de Claude Code, `claude -p` sale con código 0 en éxito y distinto de cero cuando la ejecución falla, y un fallo interno como la falta de autenticación se **describe** en el resultado impreso en stdout. **Ni stdout ni el código de salida determinan por sí solos** el éxito del trabajo: hay que cruzar varias señales (§18). Hoy `claude auth status` devuelve `loggedIn:false` en este equipo (MEASURED). Tampoco había semántica para un handoff ausente, parcial o con SHA obsoleto. La versión anterior de esta fila presentaba stdout como la señal del fallo: **se retira** |
| EXP-07 (rama activa toca la misma autoridad/contrato) | No activada | I-52 no toca ningún documento de proceso (§0). Reverificar en el Freeze |
| **EXP-08** (deuda preexistente que el cambio haría visible) | **ACTIVADA — acotada, sin expansión separada** | (a) 13 contratos V1 más TEMPLATE con `automation.enabled:true` (completada en G1-C, §18.3) sin ejecutor real (§5); (b) `.agent/` no ignorado mientras el mandato lo cita como ejemplo (§3); (c) `-text`/CRLF para artefactos byte-exactos (§3); (d) prosa «V2 no efectivo» (F-02); (e) `claude` no está en `PATH` y no está autenticado (§12). Se registran para el diseño; no se corrigen aquí |
| **EXP-09** — **autorizada** | **Resuelta para la infraestructura; delta del piloto en §19** (actualizado en G1-C) | Ver §9: crea artefactos subordinados y modifica autoridades existentes; M-01, M-02, M-04, M-05, M-06 y M-07 activados; M-03 y M-08 no activados; NEW ARCHITECTURE confirmado (Q-01). La regla de conteo `attempts`/`MAX_REWORK_LOOPS` queda para el Freeze (§20) |

**Cláusulas consumidas evaluadas por EXP-01** (clasificación = propuesta del Executor; **no** cierra clase B ni corrige normas):

| ID | Cláusulas | Consumo por I-61 | Tipo | Impacto / pregunta |
|---|---|---|---|---|
| F-01 | [WORKFLOW](../WORKFLOW.md) §3: «Lo prohibido es tener dos sesiones **activas a la vez** sobre el mismo worktree (o la misma rama)» frente al mandato: un Controller que espera y un Worker que escribe en el mismo worktree («OWNERSHIP»: nunca dos workers escribiendo a la vez) | **Sí**, el modelo controller→worker la consume directamente | En G1, posible conflicto material (clase A provisional). **Resultado en G1-C: resuelta sin contradicción residual** | **Disposición (C61-G1-01):** se aplica el relevo de WORKFLOW §3 a cada invocación de un participante externo, como exige la cláusula 5 del Owner (decisiones §9.1), **sin reinterpretar** «sesión activa» (D-01) y con el Worker terminando antes de la verificación (D-05). Evidencia: DC-02 (WORKFLOW §3 define el relevo; cláusula 5 del Owner). Confirmación de Coordinator y Architect en SAME-SESSION ROLE. Las sondas de G1-C incumplieron ese relevo (DEV-G1C-02, §17) |
| F-02 | `AUTOMATION_PLAN.md:20`, `AGENTS.md:111`, cabeceras de LIFECYCLE/PROMPT_TEMPLATES/FOUNDATIONS y `WORKFLOW.md` título de §11 y §11.2: «V2 **no efectivo** / V1 sigue efectivo» frente al hecho Git (`WORKFLOW_V2_EFFECTIVE_SHA=8a021fb6`, tag `integration/I-56` con `Claim pause … end=2026-09-17T22:30:00Z`) | Sí, determina el workflow de I-61 (T4) | **Historia/prosa desactualizada** frente a un hecho | WORKFLOW §10 manda que los hechos de Git vencen una afirmación factual falsa; la clasificación T4 de I-61 no cambia. Riesgo para el diseño: un prompt que copie esa prosa induciría al worker a tratar V2 como dormido. Clase **pendiente** en G1; tratada como A hasta su clasificación. **Disposición en G1-C (§20):** clase B **específica de esta cláusula**, con evidencia de DC-02 (WORKFLOW §10: los hechos de Git vencen una afirmación factual falsa) y del tag `integration/I-56`; el diseño añade la obligación de que los prompts lean la vigencia de Git y no copien esa prosa |
| F-03 | [WORKFLOW](../WORKFLOW.md) §4.3 («push de rama = respaldo, siempre») y §3 relevo («commit + push + árbol limpio») frente al mandato E («commit debe normalmente existir antes del handoff») | Sí, define qué entrega un worker | **Condición** (el mandato dice «normalmente»; no contradice) | Decisión de diseño: ¿quién hace el push y cuándo para habilitar la evidencia exact-SHA? (D-05) |
| F-04 | [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §9 (`attempts` = corrección de un fallo, tope `max_attempts`) frente a `MAX_REWORK_LOOPS=3` + «misma clase de fallo» | Sí | **Condición** (mismo dato, dos nombres) | Es M-01 (§9); no hay contradicción hasta que el Freeze defina la relación |
| F-05 | AUTOMATION_PLAN §2/§5/§15 (modo normal, límite de dos activas, registro hasta I-25) | §2 «Modo normal» lo consumen los packs; la ejecución delegada **no** lo consume (C61-G1-02) | **Historia/condicional** | §2 rige al ejecutor nocturno. La ejecución delegada lee las autoridades de la base de su orden (C61-G1-02), bajo las cláusulas 2-4 y 8 del Owner (C61-G1-04); el diseño extiende §2 solo para declararlo. §5 y §15 no aplican (I-61 es manual). *(Corregido en G1-C: la versión anterior decía que la ejecución delegada consume §2.)* |
| F-06 | Mandato «METRICS» («ElapsedTime if reliably available», «ToolCalls», «Tokens/cost») frente a [LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §11 («Solo se registran tiempos medidos por Git/Actions y duración activa declarada por el Owner») | Sí, métricas del piloto | **Condición** (el mandato condiciona a «reliably available»; LIFECYCLE regula qué se registra como métrica de la unidad) | **Disposición (C61-G1-08):** es una condición y no se clasifica A/B. Tiempos de proceso y tokens del agente se registran como **datos del piloto**, no como métrica de la unidad de LIFECYCLE §11, cuya fila queda en `UNKNOWN` para lo que no sea Git, Actions u Owner |

## 12. Transportes (instalado ≠ autenticado ≠ invocado)

**Medidas de hoy (MEASURED, sin ejecutar ningún worker); D0 se conserva como antecedente:**

| Transporte | Instalado | Autenticado | Invocado realmente | Notas |
|---|---|---|---|---|
| `codex` CLI | sí: `%LOCALAPPDATA%\OpenAI\Codex\bin\…\codex.exe` **0.159.2** (y `~\.codex\.sandbox-bin` 0.154.0-alpha); **no está en `PATH`** | «Logged in using ChatGPT» | **No** en G1 (no se ejecutó `codex exec`); **superado por §17**: 6 invocaciones en G1-C | `exec` ofrece `-m`, `-c key=value`, `-s`, `-C`, `--output-schema`, `--json`, `-o`, `--ephemeral`, `--ignore-user-config` |
| `claude` CLI | sí: `%USERPROFILE%\.local\bin\claude.exe` **2.1.270**; **no está en `PATH`** | **No**: `claude auth status` → `loggedIn:false, authMethod:none` (exit 1) | **No** | `-p`, `--output-format json|stream-json`, `--json-schema`, `--model`, `--effort`, `--max-budget-usd`, `--permission-mode`, `--allowedTools`, `--bare`, `--resume`, `--no-session-persistence` |
| Mensajería entre sesiones locales (Desktop) | sí | n/a | **Sí, probada en G0** (solicitud de ventana y acuse/RELEASE con la sesión de I-52) | Es un canal entre **sesiones interactivas**, no una invocación de worker; su fiabilidad como bus de protocolo es **UNKNOWN** (el canal no confirma lectura) |
| `gh` | sí, en `PATH` | sesión iniciada | sí (lecturas de CI y ruleset en G0) | Usado para CI, no para delegar |
| Computer Use | disponible en esta sesión (herramientas del escritorio) | n/a | no usado | Fallback por mandato |

- **Delta frente a D0:** sin cambios en versiones ni autenticación. **Nuevo:** `~\.codex\models_cache.json` (obtenido hoy, 2026-09-30T22:11Z) y `~\.codex\config.toml` (**de él solo se leyeron** las claves `model` y `model_reasoning_effort`; el archivo contiene más secciones: `service_tier`, `notify`, `[windows] sandbox`, `[projects.*]`, `[mcp_servers.*]`, etc. — corregido en G1-C) — datos **locales**, no documentación oficial.
- **¿Puede Codex invocar a Claude de forma fiable? — UNKNOWN en el G1 original.** En esa orden la invocación estaba **prohibida**, así que no probarla **no demostraba incapacidad** (corrección de G1-C). La orden de continuidad autorizó invocaciones acotadas: las sondas medidas están en §17. **No se convierte la autenticación de Claude en prerrequisito**; el piloto usa los transportes realmente disponibles.
- **Nota de entorno:** la sesión de escritorio de Claude usa su propia autenticación distinta de la del CLI; que la app funcione **no** prueba que el CLI esté autenticado.

## 13. Documentación oficial consultada (capacidad publicada ≠ disponibilidad local)

Fecha de consulta: **2026-09-30**. Ninguna página muestra fecha propia de publicación («no date shown»). Las páginas se leyeron con una herramienta de recuperación que puede **resumir**; donde el texto resultó íntegro se indica. Esto es **guía del proveedor, no autoridad de RackCad**.

| Fuente | Proveedor | Cubre | Puntos relevantes para el protocolo |
|---|---|---|---|
| `platform.claude.com/docs/en/models/overview` (redirigida desde `docs.claude.com`) | Anthropic | Claude Fable 5.1 (`claude-fable-5-1`), Opus 5.5 (`claude-opus-5-5`), Sonnet 5.5 (`claude-sonnet-5-5`), Haiku 4.5 (`claude-haiku-4-5-20251001`) | Descripciones, contexto (1M; 200K Haiku), salida máx., precios, *default effort* (Fable 5.1 `high`, Opus 5.5 `medium`, Sonnet 5.5 `high`, Haiku 4.5 sin effort), retiro no anterior a 2027 (Haiku: oct-2026) |
| `platform.claude.com/docs/en/build-with-claude/effort` | Anthropic | generación 4.5→5.5 | Niveles `low/medium/high/xhigh/max`; effort afecta **todos** los tokens, incluidas llamadas a herramientas; *low* recomendado p. ej. para subagentes; Opus 5.5 default `medium`; `xhigh` para agentes de larga duración; «effort is a behavioral signal, not a strict token budget»; cambio de effort a mitad de conversación (beta) |
| `platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices` | Anthropic | Fable 5.1 a Haiku 4.5 | Instrucciones claras y explícitas; etiquetas XML; uso explícito de herramientas; «Overeagerness» (evitar sobreingeniería); estado de horizonte largo (progreso incremental, archivos de estado, contexto nuevo vs. compactación); autonomía y reversibilidad (confirmar acciones destructivas); orquestación de subagentes y su sobreuso; no fijarse en pasar pruebas |
| `code.claude.com/docs/en/headless` | Anthropic | Claude Code 2.1.x | `claude -p`, `--output-format json/stream-json`, `--json-schema`, `--allowedTools`, `--permission-mode`, `--permission-prompts none`, `--resume`, `--bare` (sin OAuth/keychain; exige `ANTHROPIC_API_KEY`); fallo interno como resultado en stdout |
| `learn.chatgpt.com/docs/developer-commands?surface=cli` (redirigida desde `developers.openai.com`) | OpenAI | Codex CLI | `codex exec`: `-m`, `--output-schema`, `-o`, `--json`, `--sandbox`, `resume`; **la página no documenta una bandera dedicada de effort** (se configura con `-c`/`config.toml`) |
| `learn.chatgpt.com/docs/models` (redirigida desde `developers.openai.com/codex/models`) | OpenAI | `gpt-6-astra`, `gpt-6.1-sol`, `gpt-6-luna`; GPT-5.5 se retira el 14-oct-2026 de ChatGPT/Codex | Effort «Light a Ultra» (Luna hasta Max); disponibilidad por plan |
| `learn.chatgpt.com/guides/best-practices` | OpenAI | Codex | Objetivo + contexto + restricciones + criterio de terminado; verificación antes de aceptar; `AGENTS.md` para guía duradera; worktrees para trabajo largo; subagentes para trabajo acotado; aprobaciones y sandbox restrictivos por defecto |

**Disponibilidad local (RECONSTRUCTED, caché de Codex de hoy; no oficial):** `gpt-6.1-sol`, `gpt-6-astra`, `gpt-6-sol`, `gpt-6-luna`, `gpt-5.6-{sol,terra,luna}`, `gpt-5.5` con niveles `low…max` (y `ultra` en algunos); modelo configurado por defecto en este equipo: `gpt-5.6-sol`, effort `medium`.
**Hueco de consistencia (UNKNOWN):** el modelo por defecto local (`gpt-5.6-sol`) no figura entre los «recomendados» de la página de modelos; ese catálogo debe **reverificarse** en el diseño, no copiarse. La transcripción histórica 04 **no se usó** como fuente.

## 14. Recomendación mínima A/B/C (no es una Proposal)

**Recomendación (corregida en G1-C conforme a Q-04 del Coordinator):** la elección A/B se hace **en el diseño** con las capacidades medidas (§17) y **sin** exigir un estudio de costes previo para B. El texto de G1 condicionaba B a «sobrecoste de relevo demostrado por el piloto»; **se retira**.

- **Por qué no B/C ahora (texto de G1; su primer argumento se corrige en G1-C):** la versión anterior argumentaba que «no hay invocación de worker demostrada». Ese argumento **se retira**, porque la invocación estaba prohibida en G1 y no probarla no demostraba incapacidad. La decisión A/B se toma en el diseño con las sondas de §17 y la semántica de fallos de §18. El resto se mantiene: la semántica de fallo y la protección de invariantes se completan en §§17-18, y el mandato pide «no simular automatización» y «la opción más pequeña».
- **Qué entregaría A (actualizado en G1-C con §17):** guía de prompting con fuentes registradas, política de enrutamiento por clase de tarea con catálogo actualizable separado y **no normativo**, perfiles de prompt, esquemas JSON de delegación, entrega de worker y verificación, jerarquía de transporte y política de STOP/rework. El piloto usa artefactos de archivo (nivel 1) e invocación por CLI o directa (nivel 2, medida en §17): Controller Codex por CLI en solo lectura y Worker como subagente invocado por la sesión responsable. El texto de G1 hablaba de «relevo manual de la invocación (nivel 5)», lo que **queda superado** por las mediciones.
- **Requisitos antes del Freeze (actualizado en G1-C):** las decisiones Q/D ya están emitidas (§15, columna «Disposición»). Queda la regla de conteo de `attempts`/`MAX_REWORK_LOOPS` para el Freeze (C61-G1-05); la necesidad de ADR está decidida (C61-G1-04). EXP-05 y EXP-06 están completadas (§18).
- **Criterios de éxito 7 y 12 (Q-04):** con las mediciones de §17, el nivel A usa archivos + CLI/invocación directa como transporte primario (criterio 7) y un piloto real con Controller Codex y Worker invocado (criterio 12). La respuesta del Coordinator a Q-04 («A permitida; exige piloto real») es una lectura del mandato, **no** una aceptación del Owner de que A cumpla 7 y 12. Esa aceptación es del Owner y se ejerce en la Owner Validation del Candidato; no es una decisión previa pendiente (Q-04, §15).
- **Límites del piloto:** pequeño, sin convertirlo en benchmark entre proveedores (C61-G0-06), sin integración automática y con Owner Validation, porque cambia el comportamiento de dibujo (§19).

## 15. Lagunas, preguntas y decisiones explícitas

| ID | Pregunta | Autoridad decisora | Disposición (G1-C; [decisiones](../automation/decisions/I-61.md) §§10-11; correspondencia línea→ID RECONSTRUCTED) |
|---|---|---|---|
| Q-01 | ¿M-07 (y NEW ARCHITECTURE) aplican a mecanismos de **proceso**? (§9) | Coordinator con Architect | **Decidida**: NEW ARCHITECTURE confirmado |
| Q-02 | ¿Cama/`Name` se conserva como piloto supeditado a reproducción por el Owner, o se sustituye? (§10) | Coordinator con Owner | Decidida en G1 («preferido; defecto no demostrado»). La nueva evidencia de §19 (D-1a) motivó una **nueva decisión del Coordinator** (C61-G1-06, §20.2), que **aplica el criterio fijado por el Owner** («Preferir Cama/Name si se establece un caso concreto y verificable», decisiones §9.1, cláusula 3) |
| Q-03 | ¿Dónde viven las reglas normativas nuevas? ¿Hace falta ADR? | Coordinator; Owner si hay ADR | **Decidida** la ubicación (autoridades existentes + subordinados). La necesidad de ADR **no** la contestó Q-03: se evalúa en §20 (C61-G1-04) |
| Q-04 | ¿A satisface los criterios de éxito 7 y 12? (§14) | **Owner** (resultado de la iniciativa) | **No es una decisión previa pendiente** (corregido en G1-C). Los criterios de éxito se comprueban en la conformidad (READY-06) contra el mandato, y el Owner acepta o rechaza el resultado en la Owner Validation del Candidato, como en cualquier entrega. La línea del Coordinator («A permitida; exige piloto real») es una lectura del mandato, no esa aceptación |
| Q-05 | ¿Se autorizan EXP-05 y EXP-06? (§11) | Coordinator | **Decidida y ejecutada** (§18) |
| Q-06 | ¿Un nombre editado en blanco conserva el nombre lógico (paridad con las otras cinco familias) o lo borra? | **Owner** (intención de producto) | **Respondida por el Owner** (corregido en G1-C). MEASURED: el mandato describe el piloto como «Cama / RACKEDITAR → preserve logical Name across edit/update/save/reopen» y lo llama **«that bug»** («If current repo state shows that bug is already fixed or unsuitable», `I-61-owner-mandate.txt:555-557`); el único «borrar el nombre» documentado en `RACKEDITAR` de cama es el residual de I-60 (`I-60-evidence.md:130`), que nace de «escribe el campo tal cual» (§19). **INFERENCE del Coordinator (SAME-SESSION):** «that bug» es D-1, y conservar el nombre ante un campo en blanco es la preservación pedida, en paridad con las otras cinco familias. Esta inferencia sostiene que M-03 no se activa (§9). Si el Owner la rechaza, la OV del Candidato la invalida |
| Q-07 | ¿Se añade a WORKFLOW §10 la fila «Operación del ejecutor y ejecución delegada → AUTOMATION_PLAN»? | Coordinator para prepararla; **Owner** para su vigencia (aceptación de ADR-0046 e integración) | Preparada en el diseño (C61-G1-03). No tiene efecto antes de que el Owner acepte ADR-0046 y se integre |
| D-01 | F-01: «sesión activa» frente a un Controller no escritor | Autoridad de WORKFLOW; cláusula 5 del Owner | **Decidida**: relevo secuencial; «activa» no se reduce a «escritora». El relevo con participantes externos aplica WORKFLOW §3 como exige la cláusula 5 del Owner (C61-G1-01, §20.2) |
| D-02 | F-02: clase A/B de la prosa «V2 no efectivo» | Coordinator + Architect | Resuelta en §20 como clase B **específica de esa cláusula** |
| D-03 | Ubicación del tráfico transitorio | Diseño/Coordinator | **Decidida**: `artifacts/orchestration/<unit>/<task>/<attempt>/` |
| D-04 | Relación `attempts`/`MAX_REWORK_LOOPS` y «misma clase de fallo» | Coordinator; regla fina en el Freeze | **Decidida** en su principio (`attempts` canónico, sin reinicio). La regla de conteo, su ámbito y la precedencia quedan para el Freeze (§20) |
| D-05 | Quién hace el push del worker y cuándo | Coordinator | **Decidida**: el Worker valida, hace commit y push y termina; el Controller verifica después |
| U-01 | Recurrencia externa u otro escritor (tareas programadas, app de Codex) | **Coordinator** (preflight de cada relevo) | UNKNOWN. **No se deja abierto por defecto**: el diseño exige comprobar otros escritores en la salida de cada relevo, o STOP |
| U-02 | Fiabilidad de invocación Codex→Claude y de la mensajería entre sesiones | Coordinator | Codex→Claude: **no posible con la configuración actual** (INFERENCE sobre P3 sin red y Claude CLI sin autenticar). Mensajería: UNKNOWN (no confirma lectura) |
| U-03 | Métricas de coste y tiempo del relevo manual | Owner (LIFECYCLE §11) | UNKNOWN, no bloquea |
| U-04 | ¿Un subagente invocado con modelo y effort elegidos los aplica y puede escribir, hacer commit y push? | Coordinator | UNKNOWN hasta medirlo. Señales: la transcripción del subagente (`model`, `effort`) y Git. Se mide en el piloto antes de dar por válida la elección de executor |

## 16. Trazabilidad al mandato («FIRST RESPONSE REQUIRED», 24 puntos)

Cubiertos por este Discovery (actualizado en G1-C): 1 (preflight, §0), 2 (main, §0), 3 (ID, evidencia G0 §1), 4 (documentos de proceso, §§1-2), 5-6 (transportes, §§12 y 17), 7 («¿Codex puede
invocar a Claude de forma fiable?»: **no con la configuración actual**, INFERENCE sobre §17; U-02), 8 (modelos y effort oficiales, §13), 15 (ubicación transitoria: D-03), 16 (jerarquía: la del mandato, con disponibilidad medida en §17), 17 **parcial** (semántica de fallos y STOP en §18.4; la política de rework y su regla de conteo quedan para el diseño),
18 (piloto, §19), 19 (A/B/C, §14) y 20 (riesgos, §§11, 14, 18). **No** producidos a propósito (pertenecen al diseño/Proposal, no al Discovery): 9-14, la parte de política de 17, y 21-24 (clasificación de tareas, algoritmo de enrutamiento, catálogo
propuesto, perfiles, esquemas, política STOP/rework, propuesta de Freeze, paquete del Architect, primer prompt corto) y la confirmación final de no-transferencia de autoridad, que **se conserva como restricción**: ninguna transferencia de autoridad del Coordinator, sin
autoaprobación del worker, sin protocolo exclusivo de Computer Use y sin plataforma de automatización grande antes de evidencia del piloto.

---

# Consolidación de G1 (G1-C, 2026-09-30)

Consolidación autorizada por la orden de continuidad del 2026-09-30 ([decisiones](../automation/decisions/I-61.md) §§9-10). Incorpora las decisiones Q-01..Q-05 y D-01..D-05 del
Coordinator, corrige en su sitio las conclusiones señaladas (marcadas «corregido en G1-C») y completa EXP-04, EXP-05, EXP-06, EXP-08 y el piloto. Base: `origin/main`
`95690c28`; rama en `9228bf30` al empezar. Herramientas: lecturas y `git` directos; un workflow de investigación **de solo lectura** con subagentes internos de esta
misma sesión (`wf_ede5e4fb-ea0`, 11 agentes, sin escrituras), usados como herramienta de búsqueda y **no** como revisores independientes; y sondas reales de
`codex exec` (§17). Los hechos medidos por esos subagentes se marcan «MEASURED (subagente)».

## 17. Sondas de transporte (MEASURED, 2026-09-30)

Antes de invocar se fijaron la tarea, los permisos, la salida y los límites de cada sonda: una ejecución y un tope de 300 s. Los resultados se verificaron **contra Git**, nunca
por la mera existencia de un JSON ni por el código de salida. **Mapa sonda → directorio** (precisado en G1-C): P1-a1..a4 → `artifacts/orchestration/I-61/g1-transport-probe/1..4/`,
P3 → `…/g1-transport-probe/5/` (todos ignorados por Git, `.gitignore:3`); P2 → réplica en `%TEMP%\i61-g1\p2\` (salida en `p2\out\`), fuera del repositorio. Hashes, `thread_id`,
tokens, línea de invocación y entorno de cada sonda figuran en la [evidencia](../automation/evidence/I-61-evidence.md) §10.

| Sonda | Invocación | Resultado | Hallazgo |
|---|---|---|---|
| P1-a1 | `codex exec -s read-only --ephemeral -m gpt-6-luna -c model_reasoning_effort="low" --output-schema … -o … --json`, con stdin **abierto** | exit **124** (tope de 300 s); sin `-o` ni eventos | `codex exec` queda esperando si stdin no se cierra (la causa sale del contraste entre a1 y a2; el mensaje «Reading additional input from stdin...» aparece en todas las corridas y no discrimina): **terminación incierta**. No quedó ningún proceso huérfano de la sonda; los dos `codex.exe` vivos eran de la app del Owner y no se tocaron |
| P1-a2 | igual, con stdin cerrado (`< /dev/null`) | exit **0**, `turn.completed`, JSON **válido según el esquema** pero con campos vacíos | **No se ejecutó ningún comando.** El sandbox `unelevated` no pudo lanzar el `pwsh.exe` de `WindowsApps`, (`CreateProcessAsUserW failed: 5`; que sea un alias de la Store es INFERENCE sobre la ruta y el tamaño 0 del archivo). **Exit 0 + JSON conforme ≠ trabajo hecho**; solo el campo `notes` lo declaraba |
| P1-a3 | igual, anteponiendo al PATH del proceso hijo el `pwsh` del runtime de Codex | idéntico a a2 | Mi prefijo estaba mal formado: una ruta Windows con `:` dentro de la lista POSIX. El log del sandbox muestra que siguió usando `WindowsApps` |
| P1-a4 | igual, con el prefijo en forma POSIX (`/c/Users/…/codex-primary-runtime/dependencies/native/powershell`) | exit 0; `branch`, `head_sha` = `9228bf30…` y los 36 estados **coinciden con Git**; la escritura de prueba fue **denegada** («Access to the path … is denied») y el archivo no existe | **Controller Codex de solo lectura operativo**, con dos condiciones que se fijan por proceso: stdin cerrado y el `pwsh` del runtime primero en el PATH del hijo. No se cambió el PATH ni la configuración global |
| P2 | `-s workspace-write` en una **réplica desechable** (repo + worktree anidado + remoto local, en `%TEMP%`) | exit 0; 6 pasos `failed` | **Reclasificado en G1-C** (la versión anterior sobregeneralizaba). **Cuatro** rechazos del sandbox al crear procesos constan en `stderr.txt` (MEASURED, 17:07:46-58 local) con «windows unelevated restricted-token sandbox cannot enforce split writable root sets directly; refusing to run unsandboxed». Que esos cuatro sean `write_file`, `network`, `dotnet --version` y `temp_write` sale del autoinforme del modelo en `last.json` (**INFERENCE**). `git commit` **sí se ejecutó** (`events.jsonl`) y falló en cadena (no existía el archivo). `git push` **sí se ejecutó** y falló por un **defecto de la sonda** (remoto relativo `../remote.git` inválido desde el worktree). Conclusión: `workspace-write` con worktree anidado **rechaza lanzar ciertos procesos, incluso de solo lectura**; la capacidad de escribir, hacer commit y push de un worker Codex queda **UNKNOWN** en este entorno y **no se probó con éxito**. Efecto lateral grave: ver DEV-G1C-01 abajo |
| P3 | `-s read-only` con pasos de red, `gh`, `dotnet --version` y `git diff` | red y `gh` **fallan**; `dotnet` 8.0.423 y `git diff` **funcionan** | El Controller no puede consultar CI ni GitHub; sí puede leer el repositorio y ejecutar herramientas locales sin escribir. El perfil de PowerShell del usuario añade ruido («Cannot dot-source … language mode») sin impedir los comandos |

- **Identidad del modelo de Codex:** los eventos JSONL no nombran el modelo; solo se conoce el **solicitado** (`gpt-6-luna`). Con `--ephemeral`, el modelo efectivo es **UNKNOWN** (MEASURED
  en las sondas; la documentación leída por subagente tampoco lo incluye en el JSONL, §18.4). **MEASURED en G1-C:** sin `--ephemeral`, el registro de sesión de Codex
  (`~/.codex/sessions/…/rollout-*.jsonl`) guarda `session_meta.model` y `turn_context.model`/`effort` (comprobado en un registro existente, sin invocar). *Propuesta del Executor para
  el diseño* (decide Coordinator/Architect): invocar sin `--ephemeral` y leer ese registro.
- **Configuración de Codex (corregido en G1-C):** `~/.codex/config.toml` fija `[windows] sandbox = "unelevated"` y `service_tier = "priority"`. La versión anterior decía que las sondas
  no la cambiaron: es **falso**. **DEV-G1C-01:** durante P2 (17:07:42 local), Codex añadió al archivo **global** la entrada
  `[projects.'c:\users\alejandra-mendoza\appdata\local\temp\i61-g1\p2\repo']` con `trust_level = "trusted"` (líneas 114-115; SHA-256 actual del archivo `89F375C6…DA5281`). Es una
  modificación de configuración global **no autorizada**, efecto lateral de invocar Codex en un directorio nuevo. **No se revierte por cuenta propia**: editar el archivo sería otra
  modificación global y la app de Codex del Owner puede estar escribiéndolo. Queda como decisión del Owner (decisiones §11). **INFERENCE** (corregido en G1-C): P1 y P3, ejecutadas en el worktree de I-61, no añadieron entradas, porque el archivo no tiene ninguna `[projects]` para ese worktree y el repositorio principal ya era de confianza; el mtime no lo prueba para P1, que corrió antes que P2. Sin cambios en el
  archivo atribuibles a P3 (su mtime sigue siendo el de P2).
- **Transporte Claude:** el CLI `2.1.270` no está autenticado (`loggedIn:false`, MEASURED), así que su invocación **no se probó** (usarlo exigiría cambiar la autenticación, lo que no está
  autorizado). Los **subagentes internos de la sesión de escritorio** sí se invocan: MEASURED que el workflow `wf_ede5e4fb-ea0` corrió 11 agentes **de solo lectura**, y sus
  transcripciones registran `"model":"claude-opus-5-5"` y `"effort":"xhigh"` (heredados de la sesión; MEASURED en G1-C). Que un subagente con **modelo y effort elegidos** escriba,
  haga commit y push **no se ha medido todavía** (INFERENCE: tiene las herramientas de la sesión).
- **Mensajería entre sesiones:** se probó en G0 (acuse y RELEASE con I-52); no confirma la lectura.
- **DEV-G1C-02 (desviación del relevo, registrada en G1-C):** mientras corrían P1-a1..a4 y P3 —Codex operando en el worktree de I-61—, los subagentes de investigación de **esta** sesión
  (`wf_ede5e4fb-ea0`) leían ese mismo worktree. No hubo escrituras de esta sesión durante las sondas (árbol limpio y `HEAD` = `origin` = `9228bf30`, MEASURED al final de P1-a4 y P3), y
  Codex estaba en sandbox de solo lectura. Aun así, con D-01 dos participantes operaron a la vez sobre el worktree. Sin impacto de integridad; el diseño debe impedirlo (§20).

**Mapa de capacidad resultante** (etiquetas por celda, corregidas en G1-C):

| Rol posible | Codex CLI (`exec`) | Claude CLI (`-p`) | Subagente Claude de la sesión |
|---|---|---|---|
| Controller de solo lectura (clasificar, planificar, verificar leyendo Git y archivos) | **sí** (MEASURED, P1-a4 y P3, con stdin cerrado y el `pwsh` del runtime) | no probado (sin autenticación, MEASURED) | posible (INFERENCE), pero el mandato asigna el Controller a Codex |
| Emitir un archivo de salida estructurado (`-o` con `--output-schema`) | **sí** (MEASURED: el CLI escribió `last.json` aunque el sandbox era de solo lectura) | no probado | n/a |
| Worker con escritura, commit y push | **UNKNOWN** (P2: el sandbox rechazó lanzar procesos; push sin probar por defecto de la sonda) | no probado | INFERENCE: sí (herramientas de la sesión), **pendiente de medir** con una tarea acotada |
| Red / consultar CI (`gh`) | **no** (MEASURED, P3) | no probado | sí, a través de la sesión responsable (MEASURED: `gh` funciona en esta sesión) |
| Confirmación del modelo efectivo | sí, sin `--ephemeral` (registro de sesión) | n/a | sí (transcripción del subagente, MEASURED) |

## 18. Expansiones completadas

### 18.1 EXP-04 revisada: consumidores reales (ACTIVADA)

**Conclusión** (INFERENCE sobre hechos MEASURED por subagente en `9228bf30`): el contrato **nuevo** de delegación y handoff no tiene consumidores, pero las superficies
**existentes** que I-61 tocaría sí los tienen, a más de un salto y en otros sistemas. Que ningún código las lea **no** significa que no tengan consumidores.

| Superficie | Consumidores relevantes | Impacto para el diseño |
|---|---|---|
| `AUTOMATION_PLAN` (§§2, 8, 9, 12) | Lo citan 41 archivos. `context-packs/README.md:8-10` («Base obligatoria») y `CLAUDE.md:13` lo alcanzan a través de los packs (dos saltos hasta el agente). `initiatives/README.md:9-38` repite sus reglas en prosa. Los 9 context packs usan su enum de `gate` en `usual_gates`. 36 archivos de estado | Un documento nuevo solo es descubrible si §2 del plan o el README de packs lo enlazan. No cambiar el enum de `gate` ni el esquema de estado: romperían la coherencia de packs y estados |
| `PROMPT_TEMPLATES` | `WORKFLOW.md:396` (fila Prompts), `INITIATIVE_LIFECYCLE.md:337`, `documentation-governance.md:12,35`, `initiatives/README.md:5`, y citas por letra a §A y §C (I-58, I-61) | Añadir una sección **nueva** sin renombrar letras y actualizar su §2 «Traza y vigencia». Su traza (P-16, P-18 y P-24, según su §2; P-17 lo restringe además en Proposal V4) lo limita a procedimiento: los perfiles deben ser procedimentales, sin requisitos ni estados propios |
| `WORKFLOW` §3 | ADR-0001, ADR-0002:53, AUTOMATION_PLAN §7/§12, guía §1 y glosario:125-136 («Worktree: checkout exclusivo») | WORKFLOW §3 **no se modifica**. Su relevo (salida: commit + push + árbol limpio + resumen; entrada: verificaciones) se **aplica** a cada invocación de un participante externo, como exige la cláusula 5 del Owner (decisiones §9.1); ver C61-G1-01 |
| `WORKFLOW` §10 | Todas las órdenes; `documentation-governance.md` repite el mapa de dueños | La tabla **no** lista `AUTOMATION_PLAN` (MEASURED, `WORKFLOW.md:388-399`), aunque Q-03 lo reconoce como dueño de la operación. Decidido en G1-C: se prepara la fila (C61-G1-03), vinculada a ADR-0046 y efectiva solo al integrar |
| `WORKFLOW` §8 | Toda iniciativa | «Cambia el proceso mismo → este documento + ADR si es decisión de fondo → **antes de aplicar el proceso nuevo**» (`WORKFLOW.md:364`): el protocolo y su ADR propuesto deben estar escritos en la rama antes de aplicarse; el ADR, **antes de implementar** su decisión (C61-G1-04) |
| `AGENTS.md` | Punto de entrada de Claude y de Codex (según su documentación, Codex lo usa como guía duradera). La regla `Co-Authored-By` se repite en WORKFLOW, AUTOMATION_PLAN y ADR-0001 | El commit de un worker lleva el trailer de **quien lo ejecuta**, y el handoff debe declarar esa identidad para que el Controller la verifique. Modificar `AGENTS.md` no cuenta como commit documental: WORKFLOW §4.5.4 solo admite `docs/`, `README.md` y `CLAUDE.md` |
| `artifacts/` | CI (`artifacts/TestResults`, `coverage`, `UiTestResults`) y `eng/validation` (`artifacts/validation`); nada lo borra | `artifacts/orchestration/<unit>/<task>/<attempt>/` convive sin conflicto y queda fuera de `git status --porcelain` (árbol limpio); no es visible entre worktrees ni entre máquinas |
| Término «handoff» | `HANDOFF.md` (estado vivo) y PROMPT_TEMPLATES §E | Un `handoff.json` de worker sería un **tercer** significado: el diseño debe nombrarlo sin ambigüedad |
| Guía de validación manual | §7.2 («el Executor verifica» la identidad del DLL; la versión anterior citaba §7.1, corregido en G1-C) y §8 (el agente pregunta la duración activa) | Con Controller y Worker, esos deberes deben asignarse a un rol; no recaen en el Worker |
| Práctica previa | Decisiones de I-51/I-53 (jerarquía Orquestador → Coordinator ↔ Architect → Codex), I-55/I-60 (`SAME-SESSION ROLE` con subagentes) y la rama de I-52 (versiona `prompts.json` y `task-inputs.json` como evidencia) | El protocolo debe convivir con esa práctica sin reescribir registros históricos |

### 18.2 EXP-05: invariantes y comprobaciones realizables (ACTIVADA, completada)

Es una propuesta de Discovery; el Freeze fija las obligaciones definitivas invariante→prueba.

| ID | Invariante | Comprobación realizable | Positivo | Negativo (el oráculo puede fallar) | Resultado esperado | Responsable |
|---|---|---|---|---|---|---|
| INV-01 | Ningún Worker ni Controller puede declarar GATE PASS, Candidato, cierre ni integración | (a) Prueba Core que lee los esquemas versionados y exige estados exactos `IMPLEMENTATION_COMPLETE`/`PARTIAL`/`BLOCKED` (worker) y `EXECUTION_VERIFIED`/`EXECUTION_REWORK_REQUIRED`/`EXECUTION_BLOCKED` (controller), sin término de gate; (b) **añadido en G1-C:** comprobación de los campos de **texto libre** de la entrega y de la verificación (p. ej. `WorkCompleted`, `RecommendedNextAction`, `Findings`) contra una lista de términos de gate (GATE PASS, Candidato/Candidate, cierre/closure, integración/integration como declaración), aplicada por el Controller a la entrega y por el Coordinator a la verificación | Esquemas y artefactos del piloto | Copia en memoria del esquema con `GATE_PASS`; fixture de entrega con «GATE PASS» en texto libre: ambos deben detectarse | RED si el esquema deriva o el texto libre declara un gate | Prueba automatizada; Controller; Coordinator |
| INV-02 | Identidad exact-SHA de la delegación y de la entrega | Esquema con SHA de 40 hex. **Reasignado en G1-C por actor medido:** el Controller (sin red, P3) comprueba contra el repositorio **local**: `rev-parse HEAD` = `CurrentSha`, `BaseSha` ancestro, rama y worktree esperados y la ref de seguimiento local `refs/remotes/origin/<rama>` = `CurrentSha` (la actualiza el push del Worker; limitación declarada: no prueba el estado del remoto). La **sesión responsable** (con red) comprueba el remoto con `git ls-remote` y la corrida de CI, y lo deja en un archivo del paquete con su procedencia; el Controller lo contrasta | Ejecución real del piloto | **Control negativo real:** el Controller Codex recibe una entrega con `CurrentSha` falso y **no** puede devolver `EXECUTION_VERIFIED` | VERIFIED solo con identidad coincidente | Controller (local); sesión responsable (remoto y CI); prueba del esquema |
| INV-03 | Propiedad exclusiva de escritura | Preflight de relevo: árbol limpio, sin operación Git, sin otro escritor vivo, una sola delegación abierta por unidad. El Controller comprueba que `git diff --name-only Base..Current` ⊆ `AllowedWriteScope` y que el árbol queda limpio | Piloto | Control negativo: un archivo fuera de alcance debe dar un veredicto distinto de VERIFIED | Fuera de alcance ⇒ REWORK o STOP | Sesión responsable (preflight); Controller |
| INV-04 | `attempts` es canónico; el rework no reinicia el presupuesto | La delegación lleva `Attempt`; cada corrección incrementa `attempts` en el estado; cambiar modelo, rol o sesión no lo reinicia; la «misma clase de fallo» se identifica por el ID de la comprobación fallida y su clase. **Control manual declarado** (no hay prueba automatizada que lo proteja); la regla fina de conteo la fija el Freeze (§20) | Registro del piloto | La revisión del Coordinator detecta `Attempt` ≠ `attempts`+1 | STOP al repetirse la misma clase | Coordinator (control manual) |
| INV-05 | Tráfico transitorio fuera de Git | Prueba Core: `.gitignore` contiene `artifacts/` y el protocolo solo usa `artifacts/orchestration/…`, nunca `.agent/`. El Controller ejecuta `git check-ignore` sobre la ruta del paquete | Documentos del diseño | Una ruta no ignorada hace fallar la prueba | Árbol limpio tras cada relevo | Prueba automatizada; Controller |
| INV-06 | Enrutamiento estable separado del catálogo mutable | Prueba Core: el documento de enrutamiento no contiene identificadores de modelo, y el catálogo se declara no normativo con fecha y fuente en cada entrada | Documentos | Un patrón de modelo inyectado en memoria es detectado | RED ante la mezcla | Prueba automatizada |
| INV-07 | Prompts autolocalizados, no autocontenidos | Prueba Core sobre la sección nueva de PROMPT_TEMPLATES: contrato base + perfil + delta, con los 8 campos de LIFECYCLE §10 y referencias por ruta. **Añadido en G1-C (anti-repetición):** la prueba comprueba además que la sección no reproduce cláusulas normativas identificadas por un conjunto de frases testigo de WORKFLOW, AGENTS y AUTOMATION_PLAN (p. ej. el bloque de evidencia CI de rama) y que cada perfil queda por debajo de un tope de líneas fijado en el Freeze | Documentos | Si falta un campo o se inserta una frase testigo, se detecta | RED ante una plantilla incompleta o que copie normas | Prueba automatizada; Architect |
| INV-08 | Verificación fail-closed (VERIFIED exige todas las señales) | Controles negativos reales durante el piloto (INV-02 e INV-03) y la regla multiseñal de §18.4 escrita en el protocolo, con cada señal asignada a un actor medido (§18.4) | Piloto | Una entrega inválida o ausente nunca da VERIFIED | Fail-closed | Controller; sesión responsable para las señales remotas; el Coordinator lee la salida |
| INV-09 | Esquemas compatibles con `--output-schema` estricto | Prueba Core: `additionalProperties:false` y todas las propiedades en `required` | P1-a4 (formato aceptado) | Una propiedad fuera de `required` es detectada | RED ante un esquema no estricto | Prueba automatizada |

### 18.3 EXP-08: deuda preexistente que el protocolo haría visible (ACTIVADA, acotada)

MEASURED por subagente en `9228bf30`. I-61 no corrige nada de esto.

- **Contratos antiguos habilitados.** Hay 13 contratos V1 con `automation.enabled: true`, más `TEMPLATE.md`, que lo trae por defecto. Los 13 figuran como integrados en ROADMAP, aunque I-35
  conserva `status: integration-ready` e I-37D `status: implementing`, y ninguno tiene `priority`. Aplicando AUTOMATION_PLAN §5 al pie de la letra, **ninguno es seleccionable hoy**: la
  capacidad de dos activas ya está ocupada, están integrados y no tienen prioridad. El riesgo real no es una selección errónea, sino un estado derivado indeterminable.
  **Propuesta del Executor para el diseño** (decide Coordinator/Architect): el protocolo **no selecciona** iniciativas; cada delegación nace de una orden explícita del Coordinator.
- **Estados existentes.** 35 de los 36 archivos de estado están obsoletos:
  - 3 no son YAML válido (I-36C, I-39A e I-39B);
  - 13 usan un `state` fuera del enum y 3 un `gate` fuera del enum;
  - en 12, `last_evidence_commit` no es un SHA de 40 caracteres;
  - I-19 registra `attempts` 4 con `max_attempts` 3, e I-32 usa `attempts` para contar rondas.
  - *Denominadores (precisado en G1-C):* los recuentos los produjo un subagente con un script no versionado sobre los 36 archivos en `9228bf30`; los de `state` y `gate` fuera del enum
    se calcularon sobre los 35 archivos con bloque `automation_state` (I-58 es plano y no tiene esos campos), y el de `last_evidence_commit` sobre los 36. Quien no los reejecute debe
    tratarlos como RECONSTRUCTED.

  **Propuesta del Executor para el diseño** (decide Coordinator/Architect): un Controller no debe tomar como verdad el estado de otras iniciativas. El estado se deriva de Git (AUTOMATION_PLAN §4), y el protocolo solo lee el de la unidad delegada.
- **Custodia de evidencia.** La evidencia está repartida entre cuerpos de commit, `docs/automation/evidence` (279 archivos, solo 19 con `-text`), tags `integration/*` (8), HANDOFF y rutas
  fuera del repositorio. El tráfico transitorio ignorado **no deja rastro en Git**: ni su presencia, ni su ausencia, ni una sobrescritura. **Propuesta del Executor para el diseño** (la ubicación de hashes y hechos la rigen WORKFLOW §11.4 y AGENTS): la evidencia duradera del piloto
  copia los campos clave y el SHA-256 de cada artefacto transitorio al archivo de evidencia. Con `core.autocrlf=true`, una copia byte-exacta necesitaría `-text`.
- **Disponibilidad de herramientas.**
  - `claude` y `codex` están fuera del PATH; se invocan por rutas verificadas.
  - Claude CLI no está autenticado.
  - Codex exige stdin cerrado y el `pwsh` del runtime, y, en `workspace-write` con worktree anidado, el sandbox rechazó lanzar ciertos procesos, incluso de solo lectura (P2); la escritura de un worker Codex es UNKNOWN.
  - El SDK de .NET está instalado a nivel de usuario (`%LOCALAPPDATA%\Microsoft\dotnet`, 8.0.423).
  - AutoCAD bloquea el DLL que tenga cargado.

### 18.4 EXP-06: semántica de fallos (ACTIVADA, completada)

La parte documental está MEASURED por subagente: documentación oficial y código de `openai/codex` (etiqueta `rust-v0.159.2`) y de Claude Code, leídos el 2026-09-30. La parte local
son las sondas de §17. Donde la documentación no dice nada, el dato queda **UNKNOWN**.

| Modo | Señales fiables (varias; nunca stdout ni el código de salida por sí solos) | Clasificación propuesta |
|---|---|---|
| Proceso colgado o con stdin abierto (P1-a1) | Tope de tiempo del invocador; ningún evento terminal; `-o` sin escribir | Tras demostrar que el proceso murió: BLOCKED (transporte) |
| Salida conforme pero vacía o falsa (P1-a2/a3) | Exit 0 y JSON válido **junto con** comandos fallidos en los eventos `command_execution`, o contradicción con Git | Nunca VERIFIED; el invocador vuelve a verificar contra Git |
| Turno fallido o interrumpido (Codex) | `turn.failed` o **ningún** evento terminal (un turno interrumpido no emite nada). `-o` no se escribe, con lo que queda uno anterior obsoleto, o se escribe vacío si no hubo mensaje final | BLOCKED; ignorar cualquier `-o` con fecha anterior a la invocación |
| Resultado de Claude sin datos | `subtype` ≠ `success`, `is_error`, `success` sin `structured_output`, `error_max_structured_output_retries`, o exit 143 sin resultado tras SIGTERM | BLOCKED o REWORK según la causa |
| Permisos o credenciales | `permission_denials`, eventos de denegación, `authentication_failed`; en Codex, un `command_execution` fallido con error de acceso | BLOCKED (falta una precondición externa) |
| Handoff ausente | No existe en `ExpectedHandoffPath` una vez muerto el proceso | BLOCKED; no se reintenta con otro escritor hasta confirmar la terminación |
| Handoff inválido o de otra corrida | No parsea, no valida contra el esquema del repositorio, `TaskId`, `Attempt` o `RunId` distintos, o fecha anterior a la invocación | REWORK si la identidad Git es verificable; si no, STOP |
| Identidad errónea | Rama, worktree, `BaseSha` o `CurrentSha` no coinciden con Git, o el SHA no existe | STOP (integridad) |
| Éxito declarado sin commit | `IMPLEMENTATION_COMPLETE` con `HEAD` = `BaseSha`, árbol sucio o archivos fuera de alcance | REWORK, o STOP si hay escritura fuera de alcance |
| Corridas o escritores duplicados | Proceso previo vivo, o dos handoffs para la misma tarea e intento | STOP; no lanzar otro escritor si no se sabe si el anterior sigue vivo |
| Base obsoleta | `origin/main` avanzó respecto del `main` registrado en la delegación (corregido en G1-C: basta **cualquier** avance; la intersección con el alcance solo decide si hay conflicto) | Antes de escribir: STOP y rebase según [WORKFLOW](../WORKFLOW.md) §4.2 («si el trunk avanzó, rebase … antes de escribir una línea») con nueva delegación; después de escribir: STOP (mandato: «main movement invalidates exact-SHA assumptions») |
| Tarea que requiere Owner Validation o interacción con AutoCAD (STOP del mandato «Owner Validation required» y «required AutoCAD/user interaction») | La delegación declara la parte observable solo en AutoCAD | **No es un fallo** (añadido en G1-C). El Worker no intenta la OV ni usa AutoCAD: entrega `IMPLEMENTATION_COMPLETE`, con la OV pendiente en `KnownLimitations`, si completó lo automatizable. El Controller puede devolver `EXECUTION_VERIFIED` sobre lo automatizable y anota «OV pendiente» en `Findings`. No consume `attempts`. El control vuelve al Coordinator, que cierra el gate con la OV asignada al Candidato |
| Modelo o effort distinto del enrutado | Reroute declarado en los eventos o en el registro de sesión | Desviación; BLOCKED solo si la delegación lo exigía |

**Regla multiseñal (propuesta).** `EXECUTION_VERIFIED` exige que se cumplan **todas** estas condiciones:

- el proceso terminó y emitió un evento terminal de éxito;
- el handoff está presente, es válido y corresponde a esta tarea e intento;
- la identidad Git coincide: rama, worktree, base, HEAD y SHA publicado;
- el diff está dentro del alcance y el árbol queda limpio;
- las pruebas requeridas seleccionan más de cero casos y sus resultados son coherentes con el diff;
- no queda ninguna denegación pendiente.

Ante señales mixtas, la precedencia es **STOP > BLOCKED > REWORK REQUIRED**. El Freeze fija la relación exacta entre esta clasificación y la lista STOP del mandato.

**Actor que produce cada señal** (añadido en G1-C con las capacidades medidas de §17):

| Señal | Actor | Motivo |
|---|---|---|
| Proceso terminado, código de salida, eventos terminales, tope de tiempo | Sesión responsable (invocador) | Es quien lanza y espera el proceso |
| Entrega presente, válida, de esta tarea e intento; campos de texto libre sin términos de gate | Controller Codex (lectura) | Lee archivos del paquete en el worktree |
| Rama, worktree, `BaseSha` ancestro, `HEAD` = `CurrentSha`, ref local `origin/<rama>`, diff dentro del alcance, árbol limpio | Controller Codex (lectura, sin red) | Git local funciona en su sandbox (P1-a4, P3) |
| SHA publicado en el **remoto**, corrida de CI del SHA exacto, `origin/main` actual | Sesión responsable (con red), registrado en un archivo del paquete con su procedencia; el Controller lo contrasta | El Controller no tiene red (P3) |
| Pruebas requeridas (selección > 0 y resultados) | **CI de `push` del SHA exacto**, en un runner de GitHub independiente del Worker. La registra la sesión responsable con su procedencia, y el Controller contrasta `head_sha`, jobs, nombres y conteos con el diff. Los resultados locales del Worker son evidencia de iteración RED→GREEN, **no** la señal (corregido en G1-C) | El Controller no puede compilar ni ejecutar pruebas (sin escritura) |
| Core Full de cierre de gate sobre el SHA de cierre | Sesión responsable, después del commit y con árbol limpio ([AGENTS.md](../../AGENTS.md) «Orden para la evidencia local») | Es evidencia de gate, no de la entrega |

**Limitación declarada:** la independencia de la verificación es parcial. Las señales remotas y de CI, **incluida la señal de pruebas**, llegan al Controller a través de la sesión responsable. Cuando
el Worker es un subagente de esa misma sesión, Worker, sesión responsable y Coordinator no son independientes entre sí. La independencia del veredicto descansa en tres apoyos: el Controller
Codex, la ejecución de las pruebas en el runner de CI, ajeno a la sesión, y los controles negativos. El Controller puede detectar una incoherencia entre la CI registrada y el diff, pero no una
falsificación del archivo de procedencia por la sesión responsable. Esa garantía la da la verificación directa de la corrida de CI por el Coordinator (`gh run view`) (completado en G1-C).


## 19. Piloto: caracterización completa (Cama / `RACKEDITAR` / `Name`)

**Fuerza de la evidencia (corregido en G1-C).** Los defectos se hallaron por lectura de código en `9228bf30`. Para cada uno, **dos subagentes de esta misma sesión** (SAME-SESSION, no
independientes) intentaron refutarlo y no lo lograron. No hay reproducción en AutoCAD.

- **D-1a (MEASURED): asimetría de código.** `RackCamaCommands.EditCama` es la **única** de las seis rutas de edición sin respaldo al nombre del sobre ante un nombre editado en blanco:
  - Cama pasa `window.RackName` directo al payload (`RackCamaCommands.cs:237`) y a `SyncName` (`:241`).
  - Selectivo (`RackSelectivoCommands.cs:127`), Dinámico (`RackDinamicoCommands.cs:183`), Push Back (`RackPushBackCommands.cs:188`) y Cantilever (`RackCantileverCommands.cs:242`) usan
    `string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName`.
  - Cabecera usa el patrón equivalente sobre su configuración: `string.IsNullOrWhiteSpace(config?.Name) ? embed.Name : config.Name` (`RackCabeceraCommands.cs:270`).
  - Origen: el patrón de respaldo nace en `4d2ca82b` (Selectivo, 2026-07-08) y se extiende al resto. El código de edición de Cama viene de `df09e229` (2026-07-08) y nunca lo tuvo;
    `d54cba69` (I-11 F3) lo conservó así. (Precisado en G1-C: la versión anterior atribuía el origen de la asimetría solo a `df09e229` y citaba una expresión única para las cinco rutas.)
- **D-1b (INFERENCE, pendiente de OV): efecto en ejecución.** Con el campo vacío, `Compose` escribe `Name = ""`, `RackBlockData.Write` lo persiste en el Xrecord y, al reabrir,
  `RACKLISTA` muestra «(sin nombre)» (`RackListBuilder.cs:62`). Es la composición de piezas leídas; **no se observó** en AutoCAD (Q-02: una lectura estática no prueba el ciclo completo).
- **D-2 (MEASURED por lectura):** una cama heredada con `Name = null` pasa a `""` tras «Actualizar», aunque nadie toque el campo; sin efecto semántico (el asignador ignora blancos). El
  mismo cambio lo evita.
- **D-3 (INFERENCE):** tras vaciar el nombre, el sobre y la definición divergen (`SyncName` no hace nada con un nombre en blanco) y el asignador de I-60 puede volver a entregar el mismo
  «Cama N», que recibiría un sufijo en el bloque. El mismo cambio lo evita.
- **D-4 (UNKNOWN, fuera del piloto):** `EditCama` redibuja solo la definición elegida, no todas las del mismo `RackId`. Ninguna ruta de RackCad crea dos definiciones de cama con el
  mismo GUID.
- **Procedencia de la frase de I-60 (RECONSTRUCTED por `git log -S`):** nace en `fb0cef1b` («I-60: Discovery y Freeze propuesto») con el razonamiento «escribe el campo tal cual», perdido
  en la R2 del Freeze (`7cca141b`). Fue una **deducción estática correcta**, no una observación en AutoCAD; ninguna fila OV de I-60 cubre `RACKEDITAR` de cama.
- **Pruebas protectoras (MEASURED):** `FlowBedEditorWindowTests` cubre la ventana con el campo relleno. Ninguna prueba cubre `EditCama` con el campo en blanco ni la paridad del respaldo
  entre las seis rutas. `RackCad.Tests.csproj` no referencia el Plugin.

### 19.1 Discovery del delta del piloto (añadido en G1-C; misma unidad I-61)

| DC | Resultado |
|---|---|
| DC-01 | D-1a (MEASURED) y D-1b (INFERENCE), arriba |
| DC-02 | La regla «nombre editado en blanco → conservar el del sobre» **no tiene autoridad central**: está escrita en línea en cinco rutas `Edit*` (MEASURED). El nombre de un rack **nuevo** lo decide `RackLogicalNameAllocator` (I-60). La ventana guarda el texto tal cual (`RackEditorIdentity.SetName`). **AUTH-15** (I-52-AUTH15-C1) admite un sobre con `Name` en blanco (`RackProjectionEnvelopeName.cs:9-12`); el piloto no lo toca, porque `EditCama` no crea definiciones con AUTH-15: redibuja con `RedrawInPlace` |
| DC-03 | `RackEmbedDocument.Name` (string, admite `null`; `RackEmbedDocument.cs:53`) se serializa con `RackEmbedStore` y se persiste en el Xrecord de la definición con `RackBlockData.Write` (`SystemBlockWriter.cs:136`). Legado: `null` (camas anteriores a I-60) frente a `""`. `Compose` conserva `ExtensionData` y `CustomProperties` del sobre de origen (`RackEmbedComposer.cs:43-60`) |
| DC-04 | La cadena de §10 (pasos 1-7) |
| DC-05 | Lectores del `Name` del sobre (MEASURED por `git grep`): `RACKLISTA` (`RackListBuilder`), el escaneo de I-60 (`RackNewRackName`), `RACKBOMTOTAL` (`RackInventarioCommands.BomTotal.cs:180,219`), las etiquetas `UnreadablePayload` de los seis `*KindHandler`, `RACKPROYECTAR` (`RackProjectionEnvelopeName.cs:44-45`, que también aplica AUTH-15 a nombres en blanco), `RACKDUPLICAR` (`RackDuplicationPlan.cs:395`), la selección física (`RackPhysicalSelection.cs:370`) y `RACKLAYOUT` (`RackLayoutCommands.cs:84,233`). El cambio solo altera lo que `EditCama` **escribe**; no cambia ningún lector |
| DC-06 | Ver «Pruebas protectoras». Huecos: la paridad entre las seis rutas y `EditCama` con el campo en blanco |
| DC-07 | `src/RackCad.Plugin/RackCamaCommands.cs` es archivo caliente (`*Commands*.cs`, WORKFLOW §7). Ninguna rama activa lo toca: I-52 en `c5e97b8e` al cierre de G1-C cruza solo en documentos de estado y en el índice (`ROADMAP.md`, `HANDOFF.md` y `adr/README.md`; MEASURED, corregido en G1-C: se omitía `HANDOFF.md`) |
| DC-08 | FOUNDATIONS consumidas, verificadas por lectura en la base: **Rack Identity** (el `Name` no es identidad; el GUID no cambia), **Unknown-field Preservation** (`Compose` conserva `ExtensionData`) y **Custom Properties** (`Compose` conserva `CustomProperties`). Ninguna cambia |
| DC-09 | Corrección propuesta (decide el diseño): extraer a una **función pura** de Application la resolución del nombre editado, con la misma semántica que la expresión de las otras cinco rutas, y usarla **solo** en `EditCama` para el payload y para `SyncName`. Evaluación: **M-01 no activado**, porque no cambia quién posee la regla (cada ruta ya tenía su propia copia en línea y ninguna otra consume la función nueva), y LIFECYCLE §3 trata los helpers y las técnicas locales que conservan invariantes como no materiales. Tampoco se activan M-02 (no cambia el esquema ni el significado persistido), M-03 (es lo pedido, Q-06), M-04..M-08. Arquetipo del delta: **EXTENSION**. Unificar las seis rutas sería otro cambio, fuera de alcance |

### 19.2 Agrupación y unidad de entrega (corregido en G1-C)

LIFECYCLE §2 agrupa solo si se cumplen dos condiciones (mismo problema de usuario y autoridad de diseño compartida), y divide una iniciativa en unidades cuando difieren sistemas,
archivos calientes o límites de rollback. La corrección de Cama **no cumple** esas condiciones: otro problema de usuario, otro archivo caliente y otro límite de rollback. Por sí sola sería
**otra unidad**.

**El Owner decidió el alcance** en sus propias palabras, y el Coordinator solo lo registra (C61-G1-07):

- **El mandato** («PILOT»): «Use one small real Extension as pilot», dentro de esta iniciativa, cuyo éxito incluye «one real pilot completes».
- **La orden de continuidad** (decisiones §9.1, cláusulas 1-3):
  - «No hagas otro reclamo ni crees un worktree por cambio de rol»;
  - el objetivo pasa por «implementación y piloto» hasta «FINAL_CANDIDATE_SHA» y el «paquete preparado para Owner Validation»;
  - «Preferir Cama/Name si se establece un caso concreto y verificable».

Un cambio de alcance o de unidades es OWNER-RESERVED (LIFECYCLE §3) y aquí lo fijó el Owner. Que las cláusulas 1-2 impliquen una sola unidad es **INFERENCE** del Coordinator: la cláusula 1
podría leerse limitada al «cambio de rol», pero la 2 sitúa piloto y Candidato en este mismo recorrido. **Reglas de fusión y consumo** (LIFECYCLE §2), que apoyan la unidad única:

- una unidad solo de fundación se fusiona con su primer consumidor, y el protocolo sin piloto no entrega por sí solo el criterio de éxito 12;
- una unidad no consume el Freeze de una hermana hasta que sea alcanzable desde `origin/main`, así que un piloto en otra unidad no podría usar el protocolo antes de integrarlo, y el mandato exige
  el piloto antes de construir una plataforma.

El criterio de éxito 12 («one real pilot completes») solo se alcanza dentro de esta unidad. **Mitigación:** un gate funcional propio y commits de producto separados de los documentales.
El límite de rollback queda como un rango de commits; **se declara** que no es una unidad de entrega separada.

### 19.3 Obligaciones del piloto (corregido en G1-C)

- **Prueba conductual RED→GREEN, obligatoria.** Una guarda de fuente no sustituye una prueba de comportamiento, y la Owner Validation no sustituye la evidencia automatizada
  ([AGENTS.md](../../AGENTS.md) «Guardia de fuente y prueba de comportamiento» y «Pruebas — definición de terminado» punto 1). Por eso el piloto exige:
  - (a) **pruebas conductuales** de la función pura (blanco → nombre del sobre; espacios → nombre del sobre; `null` en el sobre se conserva; nombre propio → el editado);
  - (b) una **guarda de cableado** que hoy está en **RED** con una aserción real: `EditCama` no usa el nombre resuelto. Es la regresión «verificada fallando con el fix desactivado» que
    exige AGENTS punto 2;
  - (c) la **Owner Validation** del comportamiento de extremo a extremo.

  **Se retira la opción de G1-C «guarda + OV» como verificación conductual: no es conforme.**
- **Owner Validation:** obligatoria, porque el cambio altera el comportamiento de dibujo (el `Name` del sobre y, a través de `SyncName`, el nombre del bloque). Se ejerce sobre
  `FINAL_CANDIDATE_SHA` (AGENTS punto 5; guía de validación manual). Que una tarea requiera OV no es un fallo del Worker ni del Controller: el gate del piloto cierra con su evidencia
  automatizada y la OV queda asignada al Candidato.
- **Build Debug del Plugin:** obligatorio (AGENTS punto 3). Viabilidad MEASURED en G1-C: 0 errores y solo los dos `MSB3277` conocidos, con AutoCAD 2025 instalado y sin ejecutarse.
- **Metadatos del contrato:** `requires_plugin_build`, `requires_autocad` y `requires_owner_validation` pasan a `true` (regla monotónica, AUTOMATION_PLAN §11).
- **Intención de producto:** respondida por el Owner (Q-06, §15).

### 19.4 Alternativas evaluadas (descritas aquí para que la referencia quede versionada)

Un subagente las propuso **sin conocer D-1a**: corrió en paralelo y partía de que el defecto de Cama no estaba demostrado.

| ID | Candidato | Fuente | Motivo de descarte o reserva |
|---|---|---|---|
| ALT-1 | Etiqueta «Push Back» en `RACKLISTA` (falta el caso en `RackListBuilder.KindLabel`) | `RackListBuilder.cs:82-96` | Reserva natural: pura y sin OV. Su texto visible requiere un «sí» del Owner según `ideas-futuras.md` |
| ALT-2 | Estilos de cota del Selectivo nuevo abierto desde el menú | `EditorModules.cs:15-19, 36-42` | Cambia el dibujo; exige una costura de prueba nueva |
| ALT-3 | `RACKAYUDA` omite comandos y apunta a un archivo inexistente | `RackCommandReference.cs:24, 28-50` | Choca con el plan de I-52; hay decisiones de producto abiertas |
| ALT-4 | El Dinámico descarta el estilo de cota guardado ausente del DWG | `ideas-futuras.md:1021-1029` | Requiere antes una decisión de producto |
| ALT-5 | `RackEmbedStore.Deserialize` deja escapar `ArgumentException` con UTF-16 inválido | `RackEmbedDocument.cs:97-126` | Activa M-04; sería FOUNDATION EVOLUTION |
| ALT-6 | La biblioteca omite en silencio un archivo de *major* incompatible | `RackDesignLibrary.cs:67-99` | Dos capas; más de un gate |

## 20. Revisión de G1-C, decisiones y desviaciones

### 20.1 Revisión del Discovery

LIFECYCLE §4 asigna al Coordinator la revisión del Discovery: preguntar qué expansión debió activarse y confirmar después agrupación, materialidad y arquetipo. LIFECYCLE §5 permite al
Architect exigir expansiones en NEW ARCHITECTURE. Las dos funciones se ejercieron en `SAME-SESSION ROLE` (revisor = autor) con ayudas de solo lectura de esta sesión, que son herramientas
y no revisores independientes: `wf_459a09da-c13` (ronda 1, tres enfoques) y `wf_88af0461-b57` (ronda 2: verificación de disposiciones y búsqueda de defectos nuevos).

| Ronda | Versión revisada | Resultado |
|---|---|---|
| 1 | `I-61-discovery.md` blob `6a5d23a6`; `decisions/I-61.md` blob `24f013c1`; contrato blob `f0d8cdb7` | **CHANGES REQUIRED.** 52 hallazgos propuestos (41 REQUIRED y 11 OPTIONAL); todos los REQUIRED se aceptaron |
| 2 | `I-61-discovery.md` blob `d072b396`; decisiones `a961ccd6`; contrato `d8dcedff`; evidencia `d71b6c2a` | **CHANGES REQUIRED.** De los REQUIRED de la ronda 1, una parte quedó resuelta y otra parcial, y aparecieron defectos nuevos (13 REQUIRED). Correcciones principales: las cláusulas del Owner pasan a versionarse literalmente (decisiones §9.1) y se citan como su fuente; el piloto exige prueba conductual; la independencia de las pruebas descansa en la CI del SHA exacto; C61-G1-05 pasa a ser una propuesta para el Freeze; las decisiones del Coordinator declaran SAME-SESSION; Q-04 y Q-06 se reclasifican; se marcan los residuos superados |
| 3 | La versión que se publica con el cierre de G1 | Verificación focalizada de los REQUIRED pendientes de la ronda 2; resultado y decisión en [decisiones](../automation/decisions/I-61.md) §12 |

El detalle de cada hallazgo vive en el tráfico de los workflows (no versionado). Las disposiciones se reflejan en este documento, en el que cada corrección lleva la marca «corregido en G1-C»
o «texto de G1, superado».

### 20.2 Decisiones del Coordinator en G1-C (SAME-SESSION ROLE; NO del Owner)

Las emitió esta sesión en el rol de Coordinator. Cuando aplican una cláusula del Owner, la citan.

- **C61-G1-01 — Relevo con participantes externos.** Aplica la cláusula 5 del Owner («Un worker externo requiere un relevo válido conforme a WORKFLOW; una etiqueta «Controller esperando»
  no acredita por sí sola ese relevo») y WORKFLOW §3, **sin modificarlo**:
  - antes de cada invocación de un participante externo (Codex), la sesión responsable completa el **relevo de salida** de WORKFLOW §3: commit + push + árbol limpio + resumen de estado en
    el cuerpo del último commit;
  - **deja de operar** sobre el worktree hasta que el participante termina, incluidos sus subagentes;
  - al volver, aplica las **verificaciones de entrada** de WORKFLOW §3;
  - el participante externo empieza con esas mismas verificaciones.

  No se reinterpreta «sesión activa» (D-01): una sesión que completó el relevo de salida y no opera ha **cedido** el worktree, como cualquier sesión saliente de WORKFLOW §3. Sin el relevo
  completo, la etiqueta «esperando» no basta, y el diseño lo hace verificable con registros de tiempos y con las verificaciones de entrada. Un Worker que sea subagente de la sesión
  responsable es alternancia interna (cláusula 5: «una sola sesión responsable»): la sesión responsable no opera mientras el subagente escribe y lo verifica después. **F-01** queda
  resuelta sin contradicción residual, con evidencia de DC-02 (WORKFLOW §3 define el relevo; cláusula 5 del Owner) y confirmación de Coordinator y Architect en SAME-SESSION ROLE.
  **DEV-G1C-02** fue una desviación de esta regla.
- **C61-G1-02 — Controller frente al ejecutor de AUTOMATION_PLAN** (EXP-02). La ejecución delegada bajo una orden explícita **no** es el ejecutor nocturno: no selecciona iniciativas y no
  requiere la activación ni `automation.enabled: true`. De AUTOMATION_PLAN se aplican **estas cláusulas**:
  - **§3**, todas las viñetas salvo, de la tercera, la exigencia `automation.enabled: true` (que sí aplica en lo demás: contrato detallado, dependencias satisfechas y conflictos libres);
  - **§§8-9**: archivo de estado, `attempts` y reintentos;
  - **§12**, todas salvo la primera viñeta («no hay iniciativa elegible o ya existen dos activas»), que pertenece a la selección.

  §2 «Modo normal» rige la lectura de normas del ejecutor nocturno. La ejecución delegada lee las autoridades de la base de su orden. **Ampliar el alcance de AUTOMATION_PLAN** a la ejecución
  delegada deriva de Q-03 (Coordinator externo: «AUTOMATION_PLAN conserva la operación del ejecutor»). Como cambio normativo, solo tiene efecto con la aceptación de ADR-0046 por el Owner y
  la integración.
- **C61-G1-03 — Fila de WORKFLOW §10** (Q-07). Se prepara en el diseño y queda vinculada a ADR-0046. No tiene efecto hasta su aceptación y la integración.
- **C61-G1-04 — ADR.** Se cumplen los criterios 1 y 3 del README de ADR, con precedentes en 0001 y 0045, así que **corresponde ADR-0046 `propuesto`**, creado **antes de implementar** su
  decisión (README de ADR y WORKFLOW §8): con la Proposal, en el diseño. **La autoridad para aplicar el protocolo en el piloto** antes de integrarlo es del Owner, no del Freeze ni del
  ADR propuesto: el mandato exige el piloto antes de construir una plataforma y la orden de continuidad lo autoriza («implementación y piloto»; decisiones §9.1, cláusulas 2-4 y 8), sin
  declarar el protocolo efectivo globalmente ni rebajar reglas. La aceptación es exclusiva del Owner. No bloquea el diseño, G2 ni el piloto. Sí bloquea READY-03 y `FINAL_CANDIDATE_SHA`,
  porque registrar la aceptación modifica el ADR y crea un SHA nuevo.
- **C61-G1-05 — Regla de conteo: propuesta para el Freeze, no decisión.** Opciones que el diseño debe resolver con el Architect:
  1. `attempts` es por unidad (canónico, D-04).
  2. **Restricción obligatoria del Owner** (corregido en G1-C; no es una alternativa): un defecto nuevo no relacionado se **analiza antes de imputarlo** (mandato: «Do not count a completely
     new unrelated defect as merely another retry without analysis»). Lo que decide el Freeze es solo cómo se combina ese análisis con el incremento de `attempts` (presupuesto único,
     D-04).
  3. `MaxReworkLoops` = 3 por defecto (mandato) **por clase de fallo**, acotado por los intentos restantes de la unidad.
  4. «Misma clase de fallo» = mismo identificador de comprobación y misma clase; al repetirse hasta el tope → STOP y causa raíz (mandato «On repeated failure: STOP»).
  5. Precedencia entre topes.

  F-04 **no** se da por resuelta hasta el Freeze.
- **C61-G1-06 — Piloto.** Aplica el criterio del Owner (cláusula 3) a la evidencia nueva D-1a: el caso es **concreto** (asimetría medida) y **verificable** (prueba conductual RED→GREEN de la
  función pura + guarda de cableado RED hoy + OV). Se mantiene Cama/`Name`.
- **C61-G1-07 — Agrupación.** La fijó el Owner (§19.2); el Coordinator la registra.
- **C61-G1-08 — Clases de EXP-01.** F-01, resuelta (C61-G1-01). F-02, clase **B específica de esta cláusula**: la contradicción es demostrablemente irrelevante para el elemento consumido (la
  clasificación T4 de I-61), porque WORKFLOW §10 manda que los hechos de Git venzan una afirmación factual falsa (DC-02, MEASURED) y el tag `integration/I-56` registra el SHA efectivo y el fin
  de la pausa (DC-03, MEASURED); el riesgo de diseño es otra cosa y se mitiga con una obligación (los prompts leen la vigencia de Git). F-06 es una **condición** y no se clasifica A/B: tiempos
  y tokens van como datos del piloto, no como métrica de LIFECYCLE §11. F-03, F-04 y F-05 son condiciones (D-05, C61-G1-05 y C61-G1-02). Confirmación de Coordinator y Architect en SAME-SESSION
  ROLE.
- **C61-G1-09 — Restricciones de fase.** Las exclusiones de G0/G1 eran de fase y desde la orden de continuidad rigen sus restricciones (decisiones §9). Su efecto sobre el nivel A/B/C: B ya no
  está excluido, pero siguen prohibidos instalar software, cambiar PATH o configuración global y lanzar procesos recurrentes; un script de B no podría depender de nada de eso.

### 20.3 DC-08 del proceso (corregido en G1-C)

Criterio de LIFECYCLE §4.1. En el frontmatter, `consumes`, `extends` e `introduces` se reservan para **fundaciones**; las normas que se modifican se listan aparte.

- **Fundaciones consumidas** (por el piloto): Rack Identity, Unknown-field Preservation in Persisted Envelopes y Custom Properties (§19.1).
- **Fundaciones extendidas:** ninguna.
- **Introduce** el mecanismo de ejecución delegada. Con M-06 y M-07 activados y el criterio de éxito 14 (uso por una futura iniciativa V2), es reutilizable: quien lo introduce **redacta**
  su entrada factual de `FOUNDATIONS.md` antes de READY-04, y se publica al cierre (LIFECYCLE §4.1).
- **Normas que se modifican** (no son fundaciones): `PROMPT_TEMPLATES` (sección nueva), `AUTOMATION_PLAN` (§16 y §2) y `WORKFLOW` §10 (fila). Base consumida: los artefactos V2 de ADR-0045,
  verificados en la base (V2 efectivo, `8a021fb6`).

### 20.4 Responsabilidades G.1..G.12 del Controller frente a los actores medidos

| G | Responsabilidad (mandato) | Actor posible hoy (§17) | Nota |
|---|---|---|---|
| 1 | Recibir el contrato del gate | Controller Codex: lee un archivo del paquete (MEASURED: lee el worktree en `read-only`, P1-a4) | Nivel 1 |
| 2 | Leer la autoridad vigente | Controller Codex (ídem) | La vigencia V2 se lee de Git, no de la prosa (F-02) |
| 3 | Clasificar la tarea | Controller Codex, con salida estructurada (MEASURED: `--output-schema` + `-o`) | — |
| 4 | Elegir executor, modelo, effort y perfil | Controller Codex, con la disponibilidad medida como entrada (INFERENCE: puede razonar sobre ella) | No puede medir la disponibilidad; la mide la sesión responsable |
| 5 | Crear el paquete de delegación | Controller Codex vía `-o` (MEASURED: el CLI escribe `-o` aunque el sandbox sea de solo lectura) | El archivo lo escribe el CLI, no el sandbox |
| 6 | Verificar la propiedad exclusiva de escritura | Sesión responsable (relevo de salida) + Controller (lectura) | El Controller no ve procesos ni el remoto (MEASURED: sin red, P3) |
| 7 | Invocar al worker | **Sesión responsable**, como relevo, exactamente según el paquete | INFERENCE sobre §17: el Controller invocado en `read-only` no tiene red ni escritura, y Claude CLI no está autenticado |
| 8 | Recibir la entrega | Controller Codex: lee `worker-handoff.json` | — |
| 9 | Inspeccionar SHA, estado, diff, alcance, pruebas, evidencia e invariantes | Controller Codex para Git local; sesión responsable para el remoto y la CI; **pruebas: CI del SHA exacto**, ejecutada por un runner independiente del Worker, más los registros RED/GREEN del Worker | §18.4 «Actor que produce cada señal» |
| 10 | Clasificar la ejecución | Controller Codex (`controller-verification.json`) | — |
| 11 | Enrutar correcciones si está autorizado | Decide el Controller Codex; ejecuta la sesión responsable | Igual que G.7 |
| 12 | Devolver el resultado al Coordinator | Archivos del paquete en el worktree de la unidad + informe de la sesión responsable | Raíz del área y canal de retorno: en el diseño (CR-17) |

**Independencia de las pruebas (corregido en G1-C).** El Controller no puede compilar ni ejecutar pruebas. Por eso la señal independiente es la **CI de `push` del SHA exacto**, que corre en un
runner de GitHub ajeno al Worker. La sesión responsable la registra con su procedencia, y el Controller contrasta su `head_sha`, `event`, jobs y conclusión, y los nombres y conteos de las
pruebas, con el diff. Los resultados locales del Worker son evidencia de iteración (RED→GREEN), **no** la señal de verificación.

### 20.5 Decisión de G1

Condición de cierre (LIFECYCLE §4): DC completo, expansiones cerradas o UNKNOWN con autoridad, revisión del Coordinator y **ninguna clase A abierta** (EXP-01: F-01 resuelta, F-02 B específica, F-06 condición). La decisión la toma el Coordinator y se registra en [decisiones](../automation/decisions/I-61.md) §12, con la identidad del blob revisado. No se escribe aquí, para que el blob publicado
coincida con el revisado.
