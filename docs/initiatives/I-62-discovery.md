# I-62 — Discovery delta (tramo Discovery de F0)

Unit: `I-62`. Rama: `architecture/portabilidad-coordinador-principal`. Claim-Id `5b661a17-8c18-4183-8554-3866059cba2b`. Workflow V2.
Autorización: orden del Coordinator C62-F0-01 ([decisiones](../automation/decisions/I-62.md) §7), que abre **solo** el tramo Discovery de F0.
Contrato: [I-62-portabilidad-coordinador-principal.md](I-62-portabilidad-coordinador-principal.md). Mandato:
[I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt). Evidencia: [I-62-evidence.md](../automation/evidence/I-62-evidence.md) §10.

> **No es** Proposal, revisión del Architect, AGREED, Freeze ni F0 GATE PASS. Las hipótesis y alternativas de §§15-18 están **separadas** de los hechos y
> no son decisiones aprobadas. Lo ejecutó directamente la sesión principal, sin Controller, Worker, Reviewer, subagentes ni sondas de modelos.

Clase de cada afirmación ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4):
- **MEASURED**: medido en esta sesión sobre la base indicada;
- **RECONSTRUCTED**: tomado de evidencia versionada de otra unidad, sin reejecutarlo;
- **INFERENCE**: razonado a partir de lo anterior;
- **UNKNOWN**: no acreditado.

## 0. Apertura y base (MEASURED, 2026-10-01 16:29Z)

| Hecho | Valor |
|---|---|
| `origin/main` | `819955d61a6da4c811a11fbd11b5dca13f634b7c` (sin avance desde el reclamo; no hizo falta rebase) |
| Rama de I-62 | `HEAD` = `origin/…` = `85ae4324988ac7564719664f8f05b3fa9c6cafbe`; árbol limpio; mismo worktree del reclamo |
| Ramas activas | I-52 `c1982b2a` (base `95690c28`, 21 por detrás de `main`); I-63 `b557ea3b`; I-64 `d8a02163` |
| Sesión principal | `get_session("self")` 16:36Z → `claude-opus-5-5` / `xhigh`; MATCH frente al perfil **propuesto** (no es un perfil I-62 vigente) |

Las fuentes se leyeron en `85ae4324`, cuyo árbol de esas rutas es el de `819955d6`. Identidades (blob abreviado):

| Fuente | Blob |
|---|---|
| `docs/AUTOMATION_PLAN.md` (§16) | `07b97bd3eef3` |
| `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` | `da275e1a141a` |
| `docs/initiatives/I-61-proposal-v9.md` (Freeze de I-61) | `8cf5f9f42eea` |
| `docs/automation/agent-execution/README.md` / `routing.md` / `model-catalog.md` / `prompting-guide.md` | `64ae952b6fb0` / `82ca9b38865b` / `166d978d114e` / `79754c724c6b` |
| Esquemas `gate-contract` / `delegation` / `worker-handoff` / `controller-verification` / `relay-record` | `56ced893586a` / `62c026764a3b` / `cc3c9cf76b22` / `72747225daa2` / `2deba19606d0` |
| `docs/initiatives/PROMPT_TEMPLATES.md` (§G) | `0e3ed262add5` |
| `tests/RackCad.Tests/AgentExecutionProtocolTests.cs` | `c4af8853b466` |
| `docs/FOUNDATIONS.md` / `docs/ideas-futuras.md` | `a6162ffb46f5` / `e5641f0e4f88` |
| Evidencia / decisiones de I-61 | `13a61c9112c3` / `566d18de908f` |
| `docs/WORKFLOW.md` / `docs/INITIATIVE_LIFECYCLE.md` / `AGENTS.md` / `.gitignore` | `884ed305ce24` / `6415c33bd2f8` / `dd0d5a2d9ac3` / `f1527b161f82` |

**Reutilización de DC-01..06 de I-61: no procede** (MEASURED). El Discovery de I-61 (blob `95fdc5c6…`, commit `07ef2a85`) describe la operación **anterior** al
protocolo. Desde ese commit se crearon el README, routing, catálogo, guía, los cinco esquemas, PROMPT_TEMPLATES §G (128 líneas cambiadas) y
`AgentExecutionProtocolTests` (773 líneas): 2 306 inserciones en el área del protocolo. Como el objeto cambió, DC-01..06 se miden de nuevo aquí. El Discovery de
I-61 solo se cita como hecho histórico.

## 1. DC-01 — Comportamiento observable actual

El Agent Execution Protocol integrado funciona así (MEASURED en las fuentes de §0; uso real RECONSTRUCTED desde la evidencia de I-61 §§14-15):

1. El **Coordinator** redacta `gate-contract.json`, con celdas elegibles, alcance, invariantes, pruebas y STOP (AUTOMATION_PLAN 16.5).
2. El **Controller** planifica (`CONTROLLER_PLANNING`) y emite `delegation.json`. AUTOMATION_PLAN §16 (preámbulo y 16.1), el README §1 y la receta de 16.4 lo
   fijan a **Codex CLI de solo lectura**.
3. El Coordinator acepta o rechaza el paquete con A1-A8 (`Test-Json` + inclusión de alcances).
4. El **Worker** (subagente de la sesión, o proceso) hace commit RED/GREEN, push y `worker-handoff.json`.
5. La **sesión responsable** cede el worktree y registra `relay-record.json` (Exit, Cession, Outcome, Entry, RemoteFacts, RebaseMap).
6. El **Controller** verifica 14 comprobaciones (`controller-verification.json`).
7. Se custodia en `docs/automation/evidence/<unit>-pilot/…` y solo el Coordinator declara GATE PASS.

**Qué ya existe (no está ausente):**
- **Preflight acotado al relevo:** Exit/Entry con `HEAD` = remoto, árbol limpio, sin operación Git, `origin/main`, procesos, una delegación abierta y hash más
  claves de `~/.codex/config.toml` (16.4, README §3).
- **Routing por perfil semántico:** clases, siete dimensiones, effort semántico, niveles y elegibilidad por celda, sin nombres de modelo (`routing.md` §§1-6).
- **Catálogo mutable** por modelo y celda, con fuente y fecha.
- **Composición de prompts** neutral (PROMPT_TEMPLATES §G).
- **Topes y conteo** que no se reinician al cambiar de modelo, rol o sesión (16.8, «Sin reinicios»).
- **Custodia** por blob (16.12).

**Qué no existe o está acoplado:**
- No hay clase, perfil ni celda para la **sesión principal** (MEASURED en I-61 §16.7; ideas-futuras, sección I-61).
- El preflight solo existe dentro del relevo y para Codex (`config.toml`). No hay preflight del **entorno** antes de un binding.
- El Controller está fijado a un proveedor.
- El catálogo agrupa por proveedor **y** transporte: Anthropic solo como «subagentes de la sesión de escritorio», OpenAI solo como «Codex CLI, solo lectura».

**Uso real** (RECONSTRUCTED):
- un piloto de G3 de I-61 (`g3-cama-d1a`: 7 invocaciones de Codex y 1 Worker subagente), más las sondas PR-1 y U-04;
- I-63 e I-64 no han delegado: sus evidencias publicadas dicen «Ningún Controller, Worker, Architect ni subagente se invocó» (I-63 `b557ea3b`,
  `I-63-evidence.md` l. 123 y 188; I-64 `d8a02163`, `I-64-evidence.md` l. 154-155). Un bootstrap no prueba uso de la delegación.

## 2. DC-02 — Dueños de reglas y valores, y dónde vive cada binding de proveedor

| Regla / valor | Dueño (MEASURED) | Binding de proveedor o sesión que contiene |
|---|---|---|
| Ejecución delegada: participantes, relevo, aceptación, conteo, verificación, STOP | `AUTOMATION_PLAN.md` §16 (WORKFLOW §10, fila «Operación del ejecutor y ejecución delegada»); decisión ADR-0046 (aceptado) | Preámbulo: «un Controller Codex de solo lectura». 16.1: «Worker (Claude o Codex)», «Controller (Codex)», «La sesión Claude puede actuar como Coordinator, Architect y Executor (`SAME-SESSION ROLE`), pero no se presenta como Codex». 16.4: «participante externo (Codex)», «Worker subagente», `~/.codex/config.toml`, lista cerrada de procesos y «Receta de Codex». P-01 (`config.toml`). 16.12: ruta `<unit>-pilot/` |
| Decisión del protocolo | ADR-0046 | Título «Controller Codex»; «el proveedor solo está fijado para el Controller, por mandato» (Alternativas); «Invocar Codex … modificó la configuración global» (Contexto) |
| Routing (clase, dimensiones, nivel, elegibilidad) | `routing.md` (subordinado) | §§1-6 neutrales; **§7** «Se pasan con `-m` y `-c model_reasoning_effort=…`» (flags de Codex en el texto estable) |
| Traducción nivel → modelo; estado local por celda | `model-catalog.md` (no normativo) | Encabezados por proveedor+transporte; ningún campo de rol ni de sesión principal |
| Composición de prompts | PROMPT_TEMPLATES §G | Neutral; un único formato de texto, sin renderizador por proveedor |
| Procedimiento | README (subordinado) | §1 Controller = Codex CLI; Worker = subagente o proceso; §5 receta Codex; §6 subagente con «transcripción» |
| Relevo entre sesiones | WORKFLOW §3 | Neutral («Claude, Codex u otra herramienta pueden relevarse») |
| Evidencia, trailer | AGENTS.md | Neutral (`Co-Authored-By` de quien ejecuta) |
| Autoverificación del Principal | **ninguno normativo**; regla interina del Owner en la memoria local de Claude y seguimiento formal en `ideas-futuras.md` | La memoria privada sustituye hoy a una autoridad versionada |

**EXP-02** (autoridad ambigua, dos candidatas o ninguna): para la ejecución delegada hay una sola autoridad (AUTOMATION_PLAN §16). Para la
**autoverificación del Principal no hay ninguna** autoridad normativa. INFERENCE: no es una ambigüedad entre dueños, porque el seguimiento formal ya señala
las candidatas por dominio: `routing.md`/catálogo para el perfil, AUTOMATION_PLAN §16 para la obligación y su STOP, y WORKFLOW §4 para el paso «al abrir».
Propuesta: **no activar EXP-02**. La asignación de dueño es una decisión de diseño dentro de los dominios existentes.

## 3. DC-03 — Artefactos de proceso, esquemas y persistencia

Los cinco esquemas son JSON Schema 2020-12 con `additionalProperties: false` recursivo. **No preservan campos desconocidos:** un lector `/v1` rechaza un
campo nuevo, así que cualquier campo añadido exige versión nueva (MEASURED; OBL-02).

| Esquema | Campos neutrales | Acoplamiento medido |
|---|---|---|
| `rackcad-gate-contract/v1` | `EligibleCells[].Provider` y `.Transport` como texto libre; `Capabilities` enum semántico | Ninguno de proveedor. **Falta:** requisito de independencia y binding de roles distintos del Worker |
| `rackcad-delegation/v1` | `Executor.Provider`, `.Transport`, `.Role`, `.Cell` texto libre; `Effort.Semantic` enum semántico; `PromptProfile` enum de perfiles RackCad | `Owner.Kind` ∈ {`subagent`, `process`}: categoría de transporte, no de marca. `ExpectedHandoffPath` fijado a `artifacts/orchestration/…` |
| `rackcad-worker-handoff/v1` | `Worker.Provider` texto libre | Ninguno de proveedor |
| `rackcad-controller-verification/v1` | estados `EXECUTION_*` semánticos | **No registra la identidad** del verificador (proveedor, modelo, effort): solo la conserva el registro de relevo |
| `rackcad-relay-record/v1` | `Phase`, `Outcome`, `RemoteFacts` | `Participant.Kind` ∈ {`codex-cli`, `subagent`}; `Exit`/`Entry` **exigen** `ConfigTomlSha256` y `ConfigTomlKeys` (configuración de Codex); `RebaseMap.G2CloseSha` usa el nombre de un gate de I-61; `RemoteFacts.TestArtifacts` **sin** `Skipped`; ningún campo de host, máquina ni identidad de la sesión principal |

**Otros artefactos de proceso** (MEASURED):
- tráfico transitorio en `artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/`, ignorado por `.gitignore` (`artifacts/`, línea 3);
- custodia en `docs/automation/evidence/<unit>-pilot/<task>/<RunId>/`; el sufijo `-pilot` está pendiente según ideas-futuras;
- estado canónico `docs/automation/state/<I>.yml` (AUTOMATION_PLAN §8: `attempts`, `next_action`, `last_evidence_commit`). Sin campo de roles, bindings ni
  propietario de alcance.

**No hay ningún registro durable de custodia de roles** (Principal, Controller, Worker, Reviewer, propietario de rama/worktree/alcance). La custodia se
deduce del relevo (`Owner`, `Cession`) y de `execution_context.worktree`. INFERENCE: hoy no hay un sitio canónico donde otro Principal lea quién posee
qué.

## 4. DC-04 — Camino roles/autoridades → contrato → paquete → relevo → entrega → verificación → estado/custodia

| Paso | Símbolo / documento | Productor | Validación existente |
|---|---|---|---|
| Autoridades y revisión | `AuthorityRevision`, clases `UNIT_DOC`/`UNIT_CHANGE`/`EXTERNAL` (16.3) | Coordinator | `Authority` (16.9 #3) |
| Contrato | `gate-contract.json` (16.5) | Coordinator | ninguna mecánica salvo el esquema (sin test de instancia) |
| Paquete | `delegation.json` | Controller (planificación) | A1 `Test-Json`; A2-A8 manuales (16.5) |
| Prompt | `prompt.md` = §G.1 + §G.2 + delta (≤ 200 líneas) | sesión | SHA-256 registrado (P-05) |
| Relevo | `relay-record.json` (Exit/Cession/Entry, procesos, `config.toml`) | sesión | P-01, P-02, P-08 (16.4, 16.11) |
| Entrega | `worker-handoff.json` + commits RED/GREEN | Worker | `Handoff`, `Identity`, `Scope`, `Trailer`, `FreeText` |
| Hechos remotos | `RemoteFacts` (`gh run view`, TRX) | sesión | `Ci`, `Tests` |
| Verificación | `controller-verification.json` | Controller | README §8 (coherencia, OBL-11) + `Test-Json` |
| Estado y custodia | `state/<I>.yml`, `evidence/<unit>-pilot/` | sesión | 16.12; sin prueba mecánica |

## 5. DC-05 — Productores, lectores, validadores, plantillas, normas y consumidores

- **Productores y lectores:** los de §4.
- **Validadores:** `Test-Json` (PowerShell 7), `--output-schema` de Codex (solo en invocaciones Codex), la regla de coherencia del README §8 (manual, por
  script de la sesión) y `AgentExecutionProtocolTests` (estáticos).
- **Plantillas:** PROMPT_TEMPLATES §G y README §§2-10.
- **Normas:** AUTOMATION_PLAN §16, ADR-0046 y el Freeze de I-61 (Proposal V9, en especial §2.3, §8, §13 y §15).
- **Consumidores** (MEASURED):
  - ningún archivo de `src/` ni de `tests/` fuera de `AgentExecutionProtocolTests.cs` cita los esquemas `rackcad-*` ni `agent-execution` (`git grep` en
    `819955d6`);
  - las instancias versionadas son las 12 `relay-record.json` y demás JSON de `docs/automation/evidence/I-61-pilot/`;
  - consumidores **previstos**: los contratos de I-63 e I-64, que declaran usar I-61 (I-63 `b557ea3b` contrato l. 134; I-64 `d8a02163` contrato l. 160 y
    `I-64-owner-brief.txt` l. 44).
- **Huecos de búsqueda:** no se inspeccionaron otras cuentas ni repositorios. Los scripts de relevo de I-61 no están versionados: solo su SHA-256
  (I-61 evidencia §16.6). Su lógica no se puede reconstruir desde el proyecto (UNKNOWN).

**EXP-04** (consumidores a más de un salto o en otro sistema): los artefactos `/v1` los consumen las instancias custodiadas de I-61 y, previsiblemente, otras
unidades (I-63, I-64) a mitad de iniciativa. El mandato prohíbe cambiarles el protocolo a mitad de camino («EVIDENCE FROM CURRENT PRODUCT INITIATIVES»).
INFERENCE: un consumidor en otro sistema es una unidad que adopte `/v1`; el riesgo es de compatibilidad, no de código. **Propuesta: no abrir EXP-04 ahora.**
La compatibilidad `/v1` → nueva versión es una obligación de diseño (convivencia; ninguna reinterpretación de artefactos históricos, Freeze de I-61 §15). Si
el Coordinator la quiere medida aparte: pregunta «¿qué unidades tendrán delegaciones `/v1` abiertas al integrar I-62?», área ramas activas y su
evidencia, salida un inventario por unidad.

## 6. DC-06 — Pruebas y guardas existentes

**Corrida focal** (MEASURED, en `85ae4324`, árbol `d277aaa4…`, SDK 8.0.423 de usuario):
`dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter "FullyQualifiedName~RackCad.Tests.AgentExecutionProtocolTests"` → **17 seleccionadas, 17
superadas, 0 fallidas, 0 omitidas** (TRX `notExecuted=0`, SHA-256 en la evidencia §10). Clase de evidencia: prueba existente, para contrastar afirmaciones;
no es RED→GREEN ni evidencia de gate.

| Obligación (Freeze de I-61 §13) | Qué protege | Relevancia para I-62 |
|---|---|---|
| OBL-01 | estados exactos; ningún término de gate en enums | neutral; se conserva |
| OBL-02 | esquemas estrictos recursivos | **lista fija** de los cinco archivos `/v1`. Una versión nueva exige ampliar la lista o generalizarla (cambio de prueba en implementación) |
| OBL-03 | patrón SHA de 40 hex | neutral |
| OBL-04 | `artifacts/` ignorado; rutas operativas bajo `artifacts/orchestration/` | neutral respecto de proveedor |
| OBL-05 | `routing.md` no nombra modelos; el catálogo está completo | el conjunto prohibido se deriva del catálogo más los patrones `gpt-`, `claude-`, `o<n>-`, `gemini-`. Un proveedor futuro con otro patrón y fuera del catálogo **no se detecta** (INFERENCE) |
| OBL-06 | §G (8 perfiles, base ≤ 40, perfiles ≤ 25); cláusula de subordinación; frases testigo | un perfil o una clase nuevos (p. ej. de la sesión principal) deben respetar los topes y la lista fija de `Profiles` |
| OBL-11 | exactamente 14 comprobaciones | neutral |

**Huecos** (MEASURED):
- ninguna prueba cubre la neutralidad de `relay-record` ni la del README o la receta;
- ninguna prueba cubre la autoverificación del Principal ni el preflight de entorno;
- OBL-07..10 son controles manuales o del piloto (nc1-nc4, OBL-08, OBL-09).

## 7. DC-07 — Archivos calientes e intersecciones activas (MEASURED, 16:3xZ)

| Rama | Rutas que toca respecto de su merge-base, dentro de las superficies candidatas de I-62 |
|---|---|
| I-52 `feature/rackmirror-espejo-semantico` (`c1982b2a`) | `docs/HANDOFF.md`, `docs/ROADMAP.md`, `docs/adr/README.md`, `docs/adr/0036-…` |
| I-63 (`b557ea3b`) | `docs/ROADMAP.md` |
| I-64 (`d8a02163`) | `docs/ROADMAP.md` |

Superficies candidatas medidas: `AUTOMATION_PLAN.md`, `docs/automation/agent-execution/**`, PROMPT_TEMPLATES, WORKFLOW, AGENTS, CLAUDE.md, FOUNDATIONS,
`docs/adr/**`, ROADMAP, HANDOFF, ideas-futuras, LIFECYCLE, `AgentExecutionProtocolTests.cs`, `.gitignore` y context packs.

**EXP-07** (rama activa que toca la misma autoridad o contrato): **negativo** para AUTOMATION_PLAN, agent-execution, PROMPT_TEMPLATES, WORKFLOW, AGENTS,
FOUNDATIONS y la prueba guardiana. Solo hay **cruce textual** en ROADMAP, HANDOFF y el índice ADR (I-52), que se resuelve por ventanas de acuse en el
cierre (precedente de I-61). Propuesta: no abrir EXP-07; DC-07 se repite antes de diseño y antes de READY-04.

## 8. DC-08 — Fundaciones

| Fundación | Verificación en la base (MEASURED) | Propuesta |
|---|---|---|
| **Agent Execution Protocol** (FOUNDATIONS l. 160-169, `STABLE`) | La autoridad declarada (AUTOMATION_PLAN §16), la persistencia (`artifacts/orchestration/`, `<unit>-pilot/`, esquemas `/v1`), el contrato de mutación, los puntos de extensión (Freeze de I-61 §15) y las pruebas (`AgentExecutionProtocolTests`, 17/17) coinciden con las fuentes. Limitaciones declaradas: independencia parcial, `service_tier` UNKNOWN, Worker Codex con escritura UNKNOWN, recetas Windows, Level A, falsos positivos de procesos, sin clase de la sesión principal. Coincide con DC-01..06 | **Extends** |
| Workflow V2 (ADR-0045; WORKFLOW §11; LIFECYCLE) | efectivo (`8a021fb6`, pausa terminada) | **Consumes** |
| Prompt composition (PROMPT_TEMPLATES §G) | parte del Agent Execution Protocol (FOUNDATIONS lo incluye en el punto de extensión) | dentro de **Extends** |

No se encontró discrepancia entre FOUNDATIONS, ADR-0046, el Freeze y las fuentes: **EXP-01 sin clase A consumida**, con un matiz en §10.

```text
Consumes: Workflow V2; INITIATIVE_LIFECYCLE (proceso)
Extends: Agent Execution Protocol (AUTOMATION_PLAN §16; ADR-0046; Freeze de I-61 §15)
Introduces: ninguna fundación nueva propuesta (condicionado a §9; se reabre si el diseño crea un registro o mecanismo transversal propio)
```

## 9. DC-09 y EXP-09 — Materialidad, crear o modificar, y arquetipo

**EXP-09** (autorizada, acotada a S62-G0-01). Pregunta: ¿la portabilidad pedida extiende puntos existentes del Agent Execution Protocol o necesita crear otra
autoridad, persistencia o mecanismo genérico transversal?

### 9.1 Mapa requisito → autoridad o punto de extensión actual → delta necesario

| Requisito del mandato | Autoridad / punto actual (MEASURED) | Delta necesario (INFERENCE salvo indicación) | ¿Crea algo transversal? |
|---|---|---|---|
| Roles independientes del proveedor | AUTOMATION_PLAN 16.1 (tabla de participantes); ADR-0046 #2 | Quitar «(Codex)»/«(Claude o Codex)» de las filas; la semántica de declaraciones no cambia; REVIEWER y PRINCIPAL_COORDINATOR se añaden a la tabla (Discovery no prueba la necesidad de otros) | No: es la misma tabla |
| Controller fijado a Codex | AUTOMATION_PLAN preámbulo y 16.4; ADR-0046 (título, Alternativas) | Desfijar el proveedor del Controller. Afecta a una **decisión aceptada** (ver M-08) | No |
| Role binding por hechos observables | `routing.md` §§4-5 (algoritmo y elegibilidad por celda), aplicado hoy al Worker y al Controller (§7) | Ampliar el algoritmo a cualquier rol (incluida la sesión principal y el Reviewer), con la entrada «requisito de independencia» | No, si el binding sigue siendo un paso del routing existente; **sí** si se crea un resolutor o registro de bindings propio |
| Autoverificación del Principal | ninguno normativo; seguimiento formal en ideas-futuras (I-61) | Clase y perfil en `routing.md` (Freeze de I-61 §15: «Clase de tarea o perfil nuevo», sin Freeze nuevo), celda en el catálogo (sin Freeze) y obligación + STOP en AUTOMATION_PLAN §16 | No: dos puntos de extensión declarados y una regla en la sección dueña |
| Preflight de entorno agnóstico | 16.4 Exit/Entry (relevo); README §3.1-3.4 | Separar el núcleo (Git, árbol, remoto, procesos, delegaciones abiertas) de los hechos por adapter (`config.toml` de Codex, transcripción de subagente); extenderlo **antes** del binding | No, si vive en 16.4 y en el README; un helper de Level B sería aparte (§14) |
| Frontera de adapters | catálogo (por modelo y celda), README §§5-6 (recetas), relay-record (`Participant`, `ConfigToml*`) | Organizar el catálogo y las recetas por adapter y mover lo específico del proveedor fuera de los campos requeridos del núcleo: **nueva versión** de `relay-record` (Freeze de I-61 §15: «Cambio de esquema → `/v2` con ADR o A-n») | No crea autoridad: usa el punto «cambio de esquema». Crea un **concepto** (adapter) sin dueño nuevo |
| Routing portable | `routing.md` (neutral salvo §7) | Llevar §7 al adapter | No |
| Independencia por riesgo | ninguno; ADR-0046 acepta «independencia parcial» como coste | Regla nueva en AUTOMATION_PLAN §16 (u obligación en el gate-contract) con las clases que fije el Freeze | No crea autoridad; es una regla en la sección dueña |
| Portabilidad de contexto, custodia y huérfanos | relevo (16.4), propiedad exclusiva (16.6), estado `state/<I>.yml` (§8), custodia 16.12 | Registro durable de roles, bindings y propietarios: o como **extensión** del estado canónico (formato de AUTOMATION_PLAN §8) o como artefacto nuevo | **Depende del diseño**: extender `state/<I>.yml` no crea persistencia transversal nueva; un registro propio sí podría (M-07/M-02 creador) |
| Presupuestos que sobreviven al proveedor | 16.8 «Sin reinicios» (ya cubre modelo, rol o sesión) | Nombrar el «proveedor» explícitamente; el mecanismo ya existe | No |
| Prompts neutrales | §G (neutral) | Concepto de renderizador por PromptProfile; §G ya es neutral (MEASURED §2) | No, si el renderizador es procedimiento del README; sí si se versiona un motor |
| Seguridad / credenciales | AUTOMATION_PLAN §3; S-06 | Estados AUTHENTICATED / NOT_AUTHENTICATED / UNKNOWN en el preflight | No |

### 9.2 Evaluación individual M-01..M-08 (sobre el delta mínimo de §9.1)

| M | Disparador | Evaluación | Evidencia / razón |
|---|---|---|---|
| M-01 | cambia quién posee una regla o aparece una segunda autoridad | **NO ACTIVADO (condicional)** | Todo el delta cabe en los dueños actuales (§16, routing, catálogo, README, §G). Se activaría si el diseño saca el binding o la autoverificación a un documento propio con autoridad |
| M-02 | cambia esquema, wire o significado persistido | **ACTIVADO (modificación, no creador)** | `relay-record` necesita versión nueva (`Participant.Kind`, `ConfigToml*` requeridos, `G2CloseSha`, `Skipped`). Probablemente también `gate-contract` o `delegation` si el binding y la independencia viajan en el paquete. `/v1` sigue válido (Freeze I-61 §15) |
| M-03 | cambia comportamiento observable fuera de lo pedido | **NO ACTIVADO** | Proceso de desarrollo; no hay producto. Lo pedido es exactamente el cambio de conducta del protocolo |
| M-04 | cambia qué falla o cómo (fail-closed) | **ACTIVADO** | El mandato exige fail-closed cuando una capacidad requerida no se acredita (ROLE BINDING, criterio 4). El seguimiento de I-61 decía que UNKNOWN «no detiene por sí solo un Candidato» (ideas-futuras, l. 1551). Hay delta material de semántica de fallo (§10) |
| M-05 | cambia un contrato consumido por otros sistemas | **ACTIVADO** | Los esquemas `/v1` y AUTOMATION_PLAN §16 los consumen otras unidades (I-63 e I-64 los declaran) y las instancias custodiadas de I-61 |
| M-06 | añade, cambia o retira un punto de extensión | **ACTIVADO** | Se usan y amplían los puntos de I-61 §15 (perfil o clase nuevos, celdas, esquema `/v2`). El «adapter» sería un punto de extensión nuevo para proveedores futuros (mandato: «permit future providers without schema redesign») |
| M-07 | framework, registro, kernel o mecanismo genérico transversal | **NO ACTIVADO con el delta mínimo; UNKNOWN en dos decisiones de diseño** | Ninguna fila de §9.1 lo exige si (a) el registro de custodia extiende el estado canónico y (b) el binding es un paso del routing. Un resolutor o registro de bindings versionado como mecanismo propio, o un motor de renderizado, lo activarían. Esto lo fija el diseño, no Discovery |
| M-08 | modifica o contradice un ADR aceptado o la fuente de decisión integrada | **ACTIVADO** | ADR-0046 fija el proveedor del Controller («el proveedor solo está fijado para el Controller, por mandato»; título «Controller Codex») y acepta la independencia parcial. Desfijar el proveedor modifica esa decisión: exige un ADR nuevo que lo supere en parte (los ADR aceptados no se reescriben) |

### 9.3 Arquetipo propuesto

**FOUNDATION EVOLUTION** (activados M-02, M-04, M-05, M-06 y M-08; M-01 y M-07 no activados con el delta mínimo). Coincide con la etiqueta del mandato.
**Límite que depende del diseño:** si el Freeze crea un registro o resolutor transversal propio, M-07 (y M-01/M-02 creador) se activan y el arquetipo
vuelve a NEW ARCHITECTURE. Propuesta para el Coordinator: confirmar FOUNDATION EVOLUTION **con esa restricción de diseño explícita**, o mantener la
clasificación provisional hasta el Freeze. Bajar el arquetipo antes del Freeze exige al Coordinator (y al Architect si ya participó), según LIFECYCLE §3. La
etiqueta del mandato no se cambia.

**Agrupación:** una sola unidad (I-62). Todo el delta recae en el mismo protocolo y las mismas autoridades. Partirlo en unidades de entrega es asunto
del plan de gates, no de Discovery.

## 10. EXP-01..09

| EXP | Disparador | Resultado | Razón (positivos y negativos) |
|---|---|---|---|
| EXP-01 | contradicción FOUNDATIONS / ADR / Freeze / código | **Sin clase A consumida; dos observaciones clase B propuestas, a confirmar por Coordinator + Architect** | (1) PROMPT_TEMPLATES §2 conserva `WORKFLOW V2 = NOT EFFECTIVE` (texto histórico) frente a Git (V2 efectivo). No lo consume I-62 y ya está registrado (ideas-futuras, sección de I-61; Discovery de I-61 §18.3) → clase B. (2) La regla interina de autoverificación (memoria local) dice que UNKNOWN «no para», igual que ideas-futuras; el mandato de I-62 pide fail-closed. **No es contradicción de fuentes integradas**: la interina no es normativa y el mandato es la fuente de I-62. Es el delta M-04, no EXP-01 |
| EXP-02 | autoridad ambigua | **No activada** | §2: una sola autoridad para la ejecución delegada; para el Principal no hay dueño, pero los dominios candidatos están definidos |
| EXP-03 | legado o persistencia no localizable | **No activada** | Toda la persistencia de proceso está localizada (§3). Los scripts de relevo de I-61 no están versionados: es una dependencia de contexto privado (§13.2), no un legado ilocalizable |
| EXP-04 | consumidores más allá de un salto | **Propuesta: no activar** | §5. Compatibilidad `/v1` como obligación de diseño; inventario opcional definido |
| EXP-05 | invariante sin prueba ni modo de probarlo | **Propuesta: ACTIVAR antes del Freeze** (autorización del Coordinator) | Pregunta: ¿cómo se prueba de forma no circular que el modelo y effort efectivos del Principal son los requeridos, y que otro Principal reconstruye el estado sin memoria privada? Área: fuentes de introspección por runtime (`get_session`, registro de sesión de Codex, transcripciones de subagente) y el oráculo de la prueba de portabilidad. Salida: tabla invariante → observable → prueba/control → limitación. Hoy la única fuente medida para el Principal Claude es `get_session` de la app; para un Principal Codex no hay ninguna medida (UNKNOWN) |
| EXP-06 | semántica de fallo no determinable o fallo silencioso | **Propuesta: ACTIVAR acotada** | Pregunta: ¿qué disposición tiene cada estado de configuración (MATCH / ABOVE / BELOW / UNKNOWN) por requisito y para la disposición global? ¿Cuándo detiene UNKNOWN (requisito obligatorio frente a métrica opcional)? Área: mandato (ROLE BINDING, SELF-VERIFICATION, PREFLIGHT), AUTOMATION_PLAN 16.9/16.11 y el seguimiento de I-61. Salida: tabla estado × tipo de requisito → disposición, con el caso «UNKNOWN de un requisito obligatorio = no elegible» del routing actual (`routing.md` §5: «Lo desconocido no es elegible») como precedente MEASURED |
| EXP-07 | rama activa sobre la misma autoridad | **No activada** | §7: solo cruce textual en ROADMAP, HANDOFF e índice ADR |
| EXP-08 | deuda preexistente que el cambio haría visible | **Propuesta: no activar como expansión; registrar** | Deuda medida que I-62 tocaría: `G2CloseSha` (nombre de I-61), sufijo `-pilot`, `Skipped` ausente, patrón hex de `AnalysisSha256` distinto, scripts de relevo no versionados, C-F0-RED (§12). Se resuelven en el triage (§12) sin investigación adicional |
| EXP-09 | M UNKNOWN | **Cerrada con límite** | §9: M-07 depende de dos decisiones de diseño explícitas; no queda UNKNOWN de Discovery |

Ninguna expansión salvo EXP-09 queda abierta por esta orden. EXP-05 y EXP-06 se **proponen** para autorización. **Discovery no se declara completo**
mientras el Coordinator no las autorice, las resuelva o las descarte.

## 11. Matriz de capacidades provider × runtime × rol (host del Owner)

Fuentes:
- G0: mediciones del 2026-10-01 14:56Z (evidencia §4), no reejecutadas;
- I-61: catálogo verificado el 2026-09-30 y evidencia §§10-15;
- esta sesión: 16:36Z.

«Disponible» ≠ «autenticado» ≠ «selección solicitada» ≠ «observado» ≠ «invocación real».

| Proveedor / runtime | Rol | Disponible | Autenticado | Modelo/effort observable | Invocación real | Estado |
|---|---|---|---|---|---|---|
| Anthropic / app de escritorio (sesión principal) | PRINCIPAL_COORDINATOR | sí (esta sesión) | sí (la app) | sí: `get_session` → `claude-opus-5-5`/`xhigh` (MEASURED 16:36Z) | esta sesión | **acreditado como observación**; sin perfil I-62 vigente |
| Anthropic / subagente de la sesión | WORKER (write-commit-push) | sí | hereda la app | transcripción `"model"`/`"effort"` | `claude-sonnet-5-5` `medium`/`high` (U-04, G3) | **acreditado** (catálogo 2026-09-30/10-01) |
| Anthropic / subagente de la sesión | REVIEWER / ARCHITECT (read) | sí | hereda | transcripción | `claude-opus-5-5` heredado `xhigh`/solicitado `high` (I-61 §11, §13) | **acreditado** (read/tool-use); como revisión **independiente**: no (misma sesión, ADR-0046) |
| Anthropic / Claude CLI (`~\.local\bin`, 2.1.270) | rol externo (topología B) | sí, fuera del PATH | **NOT_AUTHENTICATED** (G0) | no medido | no | **UNVERIFIED** por precondición (no permanente) |
| OpenAI / Codex CLI (`codex-cli 0.159.2`) | CONTROLLER (read) | sí, fuera del PATH; ruta del binario **cambió** (I-61 usó `bin\c6fe824d…`, que ya no existe; hoy `bin\de8a38d2…`) | «Logged in using ChatGPT» (G0) | registro de sesión `session_meta`/`turn_context` sin `--ephemeral` | `gpt-6-luna`/`high` (PR-1, G3; 7 invocaciones) | **acreditado en I-61**; antes de usarlo hay que revalidar la ruta y resolver la línea base de `config.toml` (§13.1) |
| OpenAI / Codex CLI | WORKER (write-commit-push) | sí | sí | — | **no demostrado**: el sandbox `workspace-write` rechazó crear procesos (I-61 P2) | **UNVERIFIED** |
| OpenAI / app de escritorio de Codex | PRINCIPAL_COORDINATOR (topología B) | procesos de la app vivos (G0) | UNKNOWN | **UNKNOWN**: no hay fuente medida de modelo/effort de una sesión principal Codex | no | **UNVERIFIED** |
| GitHub CLI 2.96.0 | hechos remotos (todos) | sí | sí (keyring) | n/a | sí (`gh run view`) | **acreditado** |
| Git 2.54.0 / .NET SDK 8.0.423 de usuario | todos | sí | n/a | n/a | sí (esta sesión) | **acreditado** |

Consumo cubierto y `service_tier`: lo de I-61 (celdas medidas sin aviso de límite; efecto del `service_tier` heredado UNKNOWN). No se sondeó nada nuevo.

## 12. Triage de los 13 puntos del backlog de I-61 (propuestas, no ampliaciones)

| # | Punto | Fuente | ¿Necesario para la portabilidad? | Propuesta | Razón |
|---|---|---|---|---|---|
| 1 | Relectura de procesos | I-61 ev. §15.12 DEV-G3-02; ideas-futuras 1 | sí: el preflight en otro host o con otras apps produce los mismos falsos P-02 | **IN SCOPE** | Es parte del contrato de preflight; se conserva el STOP |
| 2 | Criterio de huérfanos | ídem (alcance del huérfano) | sí: custodia y recuperación (mandato ORPHAN) | **IN SCOPE** | Necesario para «orphan detection» determinista |
| 3 | Camino formal de reverificación | DEV-G3-04; ideas-futuras 2 | sí: recuperación y presupuestos tras STOP sin cambio de trabajo, también al cambiar de proveedor | **IN SCOPE** | Toca 16.11 y los contadores |
| 4 | Aclarar CONTROLLER_VERIFICATION | ev. §15.6, §15.12; ideas-futuras 3 | parcial: un Controller de otro proveedor necesita la semántica Exit/Entry explícita | **IN SCOPE** | Un perfil neutral debe llevar la semántica, no depender del modelo |
| 5 | `RemoteFacts` sin cobertura de omitidas | ideas-futuras 4 | no directamente | **IN SCOPE (oportunista)** | Coincide con la versión nueva de `relay-record` (M-02): añadir `Skipped` sale casi gratis en el mismo cambio de esquema. Si el Freeze no versiona `relay-record`, **DEFER** |
| 6 | Validar filtros antes de usarlos | DEV-G3-03; ideas-futuras 5 | no | **DEFER** como regla de I-62; **IN SCOPE** solo como requisito del preflight si el Freeze lo incluye | Defecto de contrato del Coordinator, no de portabilidad; tooling solo con F5 |
| 7 | Automatización Level B | Freeze I-61 §14; ideas-futuras | — | **DEFER** (§14) | Evidencia insuficiente para justificar mantener scripts |
| 8 | Temporizador real del Worker | ev. §15.12 (hallazgo) | sí: con un Worker de otro runtime no hay notificación equivalente | **IN SCOPE** (como requisito del adapter: observar timeout y terminación) | Sin implementar un temporizador en Discovery |
| 9 | Falsos positivos de P-02 | DEV-G3-02; FOUNDATIONS (limitaciones) | sí | **IN SCOPE** (junto con 1) | Atribución con positivos y negativos; no se quita el STOP |
| 10 | Worker Codex con escritura | ADR-0046 Contexto; catálogo; P2 | sí: la topología B necesita Workers Codex | **IN SCOPE: sonda autorizada** (§17) | No acreditado; solo en un fixture aprobado |
| 11 | Comportamiento del `service_tier` | catálogo; Freeze I-61 §17 | sí, para consumo cubierto por adapter | **IN SCOPE: observación** | Sin cambiar la configuración del Owner |
| 12 | Problemas de portabilidad del handoff | mandato CONTEXT PORTABILITY; §13.2 | sí: es el objetivo central | **IN SCOPE** | Registro de custodia, scripts no versionados, ruta del binario |
| 13 | Fricción del relevo en iniciativas de producto | I-63 `b557ea3b` `decisions/I-63.md` l. 55-60 (C-F0-RED) | indirecto | **IN SCOPE como evidencia**; **C-F0-RED → SEPARATE UNIT o A-n de AUTOMATION_PLAN**, decisión del Coordinator | C-F0-RED: 16.8 exige RED cuando `ChainRedSha` es `null` y 16.9 no da `pass` con `RedPart = fail`, así que una tarea DOCUMENTATION o CHARACTERIZATION no puede llegar a VERIFIED sin un RED artificial. No depende del proveedor, pero **bloquea delegar bajo I-61 cualquier tarea documental, también de I-62** (§16). I-64 aún no delegó; sin más fricción observada (ausencia de evidencia ≠ ausencia de fricción) |

Ninguno se declara OBSOLETE.

## 13. Configuración, contexto privado y portabilidad entre máquinas

### 13.1 `config.toml` (MEASURED; solo nombres de claves, sin valores)

| Momento | SHA-256 | Fuente |
|---|---|---|
| Relevos de I-61, 02:45Z-04:31Z | `89F375C6…` estable (`ConfigChanged=false` en las 12 entradas custodiadas; 103 nombres) | `docs/automation/evidence/I-61-pilot/**/relay-record.json` |
| Tras eliminar DEV-G1C-01 (READY-03 de I-61) | `37DD3559…`, «nueva referencia de `config.toml` para los relevos posteriores» (101 nombres) | decisiones de I-61 §17 |
| Hoy | `42E15A03…`, mtime 2026-10-01T05:30:20Z (103 nombres) | lectura de esta sesión |

Comparación de nombres con el último relevo de I-61 (16:33Z):
- 26 diferencias de entradas: 12 cabeceras `[projects.*]` en el registro frente a 11 hoy, **reescritas textualmente**;
- hoy hay `[tui]` y `screen_reader_detection_done`, que no estaban;
- falta la cabecera y el `trust_level` eliminados por orden del Owner.

El orden de las entradas comunes cambió. **No hay ningún relevo custodiado después de 04:31Z**: el cambio a `42E15A03` no ocurrió durante una cesión, así que
no es un P-01 pendiente de un relevo. Es una **diferencia histórica** frente a la referencia `37DD3559` (MEASURED + INFERENCE). Causa: **UNKNOWN**. INFERENCE no
acreditada: los directorios `bin` de Codex se actualizaron a las 05:16-05:17Z y aparece una clave de UI. El mtime no acredita autor ni causa.

**Consecuencia:** ninguna delegación que use Codex puede tomar `42E15A03` como referencia sin decisión. La referencia vigente la fijó una decisión registrada en
I-61 bajo autoridad del Owner. Este Discovery no la sustituye y no es un impedimento para él mismo, que no delega.

### 13.2 Dependencias de contexto privado y de máquina (MEASURED)

| Dependencia | Dónde vive hoy | Efecto para otro Principal u otra máquina |
|---|---|---|
| Regla interina de autoverificación | memoria local de Claude; seguimiento en ideas-futuras | Recuperable en parte (ideas-futuras), pero sin autoridad normativa ni referencia en WORKFLOW §4 o CLAUDE.md |
| Scripts de snapshot y relevo de I-61 | scratchpad de la sesión de I-61, solo su SHA-256 versionado (I-61 ev. §16.6) | No reconstruibles. Otro Principal no puede repetir la medición de procesos con la misma lógica |
| Ruta del binario de Codex | evidencia de I-61 (`bin\c6fe824d…`) | **Obsoleta** en el mismo host: hoy el ejecutable está en otro directorio y el más reciente no lo contiene |
| Ruta del `pwsh` del runtime de Codex | README §5 (`.cache/codex-runtimes/...`) | Dependiente del host y del usuario |
| Canal de coordinación entre sesiones (acuses y RELEASE) | mensajería local entre sesiones de Claude Desktop | No existe para un Principal Codex ni en otra máquina. Los acuses quedan en la evidencia, pero el **canal** no es portable (INFERENCE) |
| Insumos del Owner | `D:\IDs\…` (fuera del repositorio) | Lo necesario se versionó (mandato, decisiones, evidencia); el resto no es necesario para continuar |
| Custodia de roles y bindings | no hay registro (§3) | Otro Principal debe deducirla de relevos y estado |

## 14. Level A frente a Level B

**Fricción observada** (RECONSTRUCTED, I-61 ev. §15.11-15.12; un solo piloto):
- del contrato al fin de nc3 pasaron 67 min 31 s; cesiones de Codex 27 min 7 s y del Worker 13 min 35 s;
- relevo, registros y resolución de tres STOP por la sesión: **unos 27 min**;
- tres STOP: dos S-04 conservadores injustificados del Controller y un falso P-02 por apps del host. Un defecto de filtro (`Name~`) y la identidad del
  prompt enmarcado por el harness;
- 7 de 7 invocaciones de Codex (tope 5 + 2) y 0 avisos de límite.

**Coste de mantener helpers:** UNKNOWN. No hay medición de mantenimiento, y un helper de procesos o de filtros tendría que probarse en dos runtimes y en
otro host para servir a la portabilidad (INFERENCE).

**Recomendación preliminar:** **mantener Level A** y **DEFER F5**. Evidencia insuficiente: un piloto, y sin ninguna delegación en iniciativas de producto.
Disparadores que el Freeze podría fijar para reconsiderarlo, sin inventar umbrales: repetición de la misma fricción de relevo en ≥ 2 unidades, o necesidad de
reproducir la medición de procesos en otro host. Mientras tanto, lo que hoy se pierde (los scripts de I-61) se resolvería con **procedimiento versionado**
(README), no con tooling. Esta decisión de documentación es del diseño.

## 15. Hipótesis y preguntas para los contratos (preliminar; no aprobado)

| Tema | Hipótesis | Pregunta abierta |
|---|---|---|
| Roles | Cinco roles estables en la tabla de 16.1, con PRINCIPAL_COORDINATOR y REVIEWER añadidos y sin proveedor por rol | ¿El Reviewer es un rol propio o una forma de ARCHITECTURE_REVIEW sin autoridad de Architect? (mandato: «Possibly others only if Discovery proves a need»: no se probó la necesidad de otros) |
| Perfil del Principal | Clase y perfil PRINCIPAL_COORDINATION en `routing.md` (Long-horizon → Frontera) y celda en el catálogo; campos del mandato | ¿Qué fuente de introspección acredita el modelo y effort por runtime (EXP-05)? |
| Estados | MATCH / ABOVE / BELOW / UNKNOWN por dimensión más disposición global | ¿UNKNOWN de un requisito obligatorio = STOP (fail-closed, como «Lo desconocido no es elegible» de `routing.md` §5) frente al «no detiene» del seguimiento? (EXP-06) |
| Preflight | Núcleo agnóstico en 16.4 más hechos por adapter; antes del binding y en cada relevo | ¿Qué se revalida al cambiar de máquina, de runtime, de credenciales o de catálogo? |
| Adapters | Frontera por catálogo y README, sin «motor» | ¿Dónde se declara un adapter nuevo: entrada de catálogo (sin Freeze) o sección de README (con revisión del Coordinator)? |
| Routing | Ampliar `routing.md` §4-5 a todos los roles; §7 al adapter | ¿El binding se registra en el paquete (`delegation`) o en el estado canónico? |
| Independencia | Clases del mandato (nombres finales del Freeze); proveedor distinto solo con justificación | ¿Qué riesgos exigen contexto independiente frente a proveedor independiente? |
| Custodia | Extender `state/<I>.yml` con roles, bindings, propietarios y la siguiente acción autorizada | ¿Basta el formato de AUTOMATION_PLAN §8 o hace falta una versión nueva del estado (M-02)? |
| Recuperación | Huérfano = escritor sin terminación confirmada; STOP sin reset ni borrado | ¿Cómo se acredita la muerte de un participante en otra máquina? |

## 16. Plan propuesto de portabilidad A→B y B→A (PENDING; no ejecutado)

**Precondiciones:**
- Freeze acordado;
- fixture de infraestructura sin `src/`;
- línea base de `config.toml` resuelta;
- celdas acreditadas.

En concreto, para B hace falta Claude CLI autenticado por el Owner, una fuente de introspección del Principal Codex y Worker Codex con escritura medido.

**Prueba:**
1. El Principal A abre la unidad de prueba, ejecuta bootstrap y un gate, escribe estado canónico y se detiene con su relevo de salida.
2. El Principal B empieza sin el contexto de A: solo `git clone`/fetch y documentos versionados.
3. B produce la siguiente decisión de gate o delegación.

**Oráculo**, fijado antes del ensayo: lista cerrada de hechos que B debe reconstruir.
- SHA de base, rama y Claim-Id;
- custodia (propietario y delegaciones abiertas);
- `attempts` y contadores por clase;
- restricciones y STOP vigentes;
- siguiente acción autorizada.

Cada hecho se compara contra el estado registrado. Un hecho que solo estaba en la narrativa de A hace fallar el oráculo.

**Controles negativos:**
- handoff ausente;
- SHA cambiado;
- credenciales no acreditadas;
- selección de 0 pruebas;
- escritor anterior no terminado;
- trabajo sin commit;
- artefacto de otra corrida;
- contador tras cambiar de proveedor.

Resultado esperado: STOP o no elegible, con causa.

**Otro Principal no se simula** cambiando la etiqueta del autor: B debe ser otro runtime real o, como mínimo, otra sesión sin contexto compartido,
declarada como tal. Lo que no se pueda demostrar se registra UNSUPPORTED o UNVERIFIED con su causa.

## 17. Ruta para ejecutar I-62 bajo I-61 y sondas mínimas solicitadas

**Ruta** (INFERENCE sobre AUTOMATION_PLAN §16):
- **Tareas con pruebas** (prueba de esquema `/v2`, guardas): ROUTINE_IMPLEMENTATION con RED, delegables con la celda Worker acreditada (subagente
  `claude-sonnet-5-5`).
- **Tareas documentales o de caracterización:** **no delegables mientras C-F0-RED siga abierto** (§12, punto 13). Las ejecuta la sesión responsable
  directamente, con la evidencia de AGENTS.
- **Controller Codex:** exige revalidar el binario y resolver la línea base de `config.toml` (§13.1) antes de la primera invocación.
- **Fricción:** cada fricción se registra como evidencia. Ningún comportamiento de I-62 sin aprobar se usa para implementar I-62.

**Sondas** (solicitudes; **no autorizadas ni ejecutadas**; su preparación no habilita celdas):

| Id | Pregunta | Celda / transporte | Precondiciones | Observables | Límite | Consumo | STOP |
|---|---|---|---|---|---|---|---|
| SP-1 | ¿Claude CLI puede actuar como rol externo de solo lectura con salida estructurada? | Anthropic × Claude CLI × read | el Owner autentica el CLI (acción del Owner); política de consumo conocida | modelo y effort efectivos en una fuente del CLI; salida válida contra el esquema; sin escritura | 2 invocaciones | solo por suscripción, sin clave de API; si es desconocido, no se sondea | aviso de límite (P-06); denegación (S-06); cambio de configuración |
| SP-2 | ¿Codex puede ser Worker con escritura, commit y push en un worktree real? | OpenAI × Codex CLI × write-commit-push | fixture aprobado; línea base de `config.toml` resuelta; sandbox decidido por el Owner | commit y push contra Git; hash de configuración antes y después | 2 | ídem | P-01; escritura fuera del fixture |
| SP-3 | ¿Existe una fuente fiable de modelo y effort de una sesión principal Codex? | OpenAI × app de escritorio | ninguna invocación de pago; solo lectura de metadatos | campo y ubicación de la fuente | lectura | ninguno | si exige iniciar sesión o cambiar configuración |

## 18. Decisiones pendientes y autoridad competente

| Asunto | Autoridad |
|---|---|
| Revisar DC/EXP y preguntar por expansiones omitidas; autorizar EXP-05 y EXP-06 (o descartarlas con razón) | Coordinator |
| Confirmar el arquetipo FOUNDATION EVOLUTION con la restricción de §9.3, o mantener NEW ARCHITECTURE provisional | Coordinator (Architect si ya participó) |
| Clase B de EXP-01 (§10) | Coordinator + Architect |
| Triage (§12): disposición final, incluido C-F0-RED como SEPARATE UNIT o A-n de AUTOMATION_PLAN | Coordinator (A-n de una norma ajena: su autoridad) |
| Línea base de `config.toml` (`37DD3559` → ¿`42E15A03`?) antes de cualquier delegación Codex | **OWNER-RESERVED** (la referencia la fijó una decisión del Owner en I-61) |
| Autenticar Claude CLI, sandbox de Codex y sondas SP-1..3 | **OWNER-RESERVED** (credenciales, configuración, consumo) más autorización del Coordinator |
| ADR nuevo que supere en parte ADR-0046 (M-08) | Owner (aceptación), redactado en diseño |
| Revisión del Architect antes del Freeze, con los 12 puntos del mandato («ARCHITECT») | Architect |

**Disposición de S62-G0-01..04:**
- **S62-G0-01 (arquetipo):** propuesta FOUNDATION EVOLUTION con límite de diseño (§9.3); decide el Coordinator.
- **S62-G0-02 (capacidades):** actualizada en §11. Claude CLI sigue NOT_AUTHENTICATED, así que la topología B queda UNVERIFIED. El hallazgo del binario de
  Codex queda ampliado (ruta de I-61 obsoleta).
- **S62-G0-03 (`config.toml`):** diferencia histórica, no un P-01 de relevo; causa UNKNOWN; queda pendiente del Owner para cualquier delegación Codex (§13.1).
- **S62-G0-04 (perfil del Principal):** observación MATCH vigente a las 16:36Z; insumo de F1 y de EXP-05.

## 19. Trazabilidad con «FIRST RESPONSE REQUIRED» del mandato

| Puntos | Dónde |
|---|---|
| 1-4 (preflight, `origin/main`, reclamo, recibo de I-61) | evidencia §§1-4; §0 |
| 5-6 (documentos y esquemas; evidencia de uso) | §§1-6, §12 (13) |
| 7-12 (runtimes, CLI, autenticación, modelos, effort) | §11; evidencia §4 |
| 13-15 (bindings, huecos de portabilidad, contexto privado) | §§2-3, §13.2 |
| 16-22 (perfil, estados, preflight, adapters, binding, independencia, custodia) | §15 (hipótesis; diseño pendiente) |
| 23-24 (backlog, Level B) | §§12, 14 |
| 25 (plan de dos topologías) | §16 |
| 26-27 (Freeze, paquete del Architect) | **no producidos** (fuera de este tramo) |
| 28 (ruta de I-61 para I-62) | §17 |
| 29 (estado) | `IMPLEMENTATION AUTHORIZATION = NO` |
