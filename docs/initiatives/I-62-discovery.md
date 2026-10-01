# I-62 — Discovery delta (tramo Discovery de F0)

Unit: `I-62`. Rama: `architecture/portabilidad-coordinador-principal`. Claim-Id `5b661a17-8c18-4183-8554-3866059cba2b`. Workflow V2.

Autorización: órdenes del Coordinator C62-F0-01 (tramo Discovery) y C62-F0-04 (ronda R1: expansiones acotadas), en las
[decisiones](../automation/decisions/I-62.md) §§7 y 9. Contrato: [I-62-portabilidad-coordinador-principal.md](I-62-portabilidad-coordinador-principal.md).
Mandato: [I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt). Evidencia: [I-62-evidence.md](../automation/evidence/I-62-evidence.md)
§§10-11.

> **No es** Proposal, revisión del Architect, AGREED, Freeze ni F0 GATE PASS. Las hipótesis y alternativas de §§15-17 están separadas de los hechos y
> no son decisiones aprobadas. Lo ejecutó directamente la sesión principal, sin Controller, Worker, Reviewer, subagentes ni sondas de modelos. La
> única inspección de capacidad (SP-3) fue pasiva, de metadatos.
>
> **Ronda R1** (revisión del Coordinator F0-R1 sobre `f25dd1d5`): se corrigieron las secciones que indica §20. Siguen abiertas las
> UNKNOWN de §9 con su decisor. **El Discovery no se declara completo.**

Clase de cada afirmación ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4):
- **MEASURED**: medido por esta sesión, con base y fecha;
- **RECONSTRUCTED**: tomado de evidencia versionada de otra unidad, sin reejecutarlo;
- **INFERENCE**: razonado a partir de lo anterior;
- **UNKNOWN**: no acreditado, con decisor.

## 0. Apertura y base

| Hecho | Entrega inicial (16:29Z) | Ronda R1 (17:03Z) |
|---|---|---|
| `origin/main` | `819955d61a6da4c811a11fbd11b5dca13f634b7c` | igual; sin rebase |
| Rama de I-62 | `85ae4324…` = remoto; árbol limpio | `f25dd1d5…` = remoto; árbol limpio |
| I-52 | `c1982b2a` (base `95690c28`) | igual |
| I-63 | `b557ea3b` | `dbfe1150` (su Discovery de F0; solo documentos propios) |
| I-64 | `d8a02163` | `cf034e59` (su Discovery de F0; solo documentos propios) |

Sesión principal: `get_session("self")` → `claude-opus-5-5` / `xhigh` (16:36Z). Es una observación comparada con el perfil propuesto; ningún perfil I-62 está
vigente.

Identidades de las fuentes, todas iguales en `85ae4324` y `f25dd1d5`, porque la rama solo cambió documentos de I-62 (blob abreviado):

| Fuente | Blob |
|---|---|
| `docs/AUTOMATION_PLAN.md` (§16) | `07b97bd3eef3` |
| `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` | `da275e1a141a` |
| `docs/initiatives/I-61-proposal-v9.md` (Freeze de I-61) | `8cf5f9f42eea` |
| `docs/automation/agent-execution/README.md` / `routing.md` / `model-catalog.md` / `prompting-guide.md` | `64ae952b6fb0` / `82ca9b38865b` / `166d978d114e` / `79754c724c6b` |
| Esquemas `gate-contract` / `delegation` / `worker-handoff` / `controller-verification` / `relay-record` | `56ced893586a` / `62c026764a3b` / `cc3c9cf76b22` / `72747225daa2` / `2deba19606d0` |
| `docs/initiatives/PROMPT_TEMPLATES.md` | `0e3ed262add5` |
| `tests/RackCad.Tests/AgentExecutionProtocolTests.cs` | `c4af8853b466` |
| `docs/FOUNDATIONS.md` / `docs/ideas-futuras.md` | `a6162ffb46f5` / `e5641f0e4f88` |
| Evidencia / decisiones de I-61 | `13a61c9112c3` / `566d18de908f` |
| `docs/WORKFLOW.md` / `docs/INITIATIVE_LIFECYCLE.md` / `AGENTS.md` / `.gitignore` | `884ed305ce24` / `6415c33bd2f8` / `dd0d5a2d9ac3` / `f1527b161f82` |

**Reutilización de DC-01..06 de I-61: no procede** (MEASURED). El Discovery de I-61 (blob `95fdc5c6…`, commit `07ef2a85`) describe la operación anterior
al protocolo. Desde entonces se crearon el área del protocolo, PROMPT_TEMPLATES §G y la prueba guardiana: 2 306 inserciones. Por eso DC-01..06 se midieron de
nuevo.

## 1. DC-01 — Comportamiento observable actual

El Agent Execution Protocol integrado (MEASURED en las fuentes de §0; uso real RECONSTRUCTED desde la evidencia de I-61 §§14-15):
1. El **Coordinator** redacta `gate-contract.json` (16.5).
2. El **Controller**, fijado a **Codex CLI de solo lectura** (preámbulo y 16.1 de §16; README §1; receta de 16.4), planifica y emite `delegation.json`.
3. El Coordinator acepta o rechaza con A1-A8.
4. El **Worker** (subagente de la sesión, o proceso) hace commit RED/GREEN, push y `worker-handoff.json`.
5. La **sesión responsable** cede el worktree y escribe `relay-record.json`.
6. El Controller verifica las 14 comprobaciones.
7. Se custodia en `docs/automation/evidence/<unit>-pilot/` y el Coordinator declara GATE PASS.

**Qué ya existe:**
- preflight **acotado al relevo**: Exit/Entry con Git, procesos, una delegación abierta y `config.toml` de Codex (16.4, README §3);
- routing por perfil semántico y elegibilidad por celda (`routing.md` §§1-6);
- catálogo mutable;
- composición de prompts neutral (§G);
- topes y conteo que no se reinician al cambiar de modelo, rol o sesión (16.8);
- custodia por blob (16.12).

**Qué no existe o está acoplado:**
- no hay perfil, clase ni celda para la **sesión principal** (I-61 ev. §16.7; ideas-futuras, sección I-61);
- no hay preflight del entorno **antes** de un binding;
- el Controller está fijado a un proveedor;
- el catálogo agrupa por proveedor **y** transporte;
- §16 usa en reglas generales el nombre de un gate de I-61 («cierre de G2», 16.3, 16.5, 16.7), ver §10.6.

**Uso real** (RECONSTRUCTED):
- un piloto de I-61 (`g3-cama-d1a`: 7 invocaciones de Codex y 1 Worker subagente), más las sondas PR-1 y U-04;
- I-63 e I-64 hicieron su Discovery de F0 **directamente**, sin Controller, Worker ni subagentes (I-63 `dbfe1150`; I-64 `cf034e59`,
  `I-64-discovery.md` l. 17-18). Ninguna delegación de unidades de producto está acreditada.

## 2. DC-02 — Dueños de reglas y valores, y dónde vive cada binding de proveedor

| Regla / valor | Dueño (MEASURED) | Binding de proveedor o sesión que contiene |
|---|---|---|
| Ejecución delegada | `AUTOMATION_PLAN.md` §16 (WORKFLOW §10, fila «Operación del ejecutor y ejecución delegada»); decisión ADR-0046 (aceptado) | Preámbulo: «un Controller Codex de solo lectura». 16.1: «Worker (Claude o Codex)», «Controller (Codex)», «La sesión Claude … no se presenta como Codex». 16.4: «participante externo (Codex)», «Worker subagente», `~/.codex/config.toml`, lista cerrada de procesos, «Receta de Codex». P-01 |
| Decisión del protocolo | ADR-0046 | Título «Controller Codex»; «el proveedor solo está fijado para el Controller, por mandato» (Alternativas) |
| Routing | `routing.md` (subordinado) | §§1-6 neutrales; §7 con flags de Codex (`-m`, `-c model_reasoning_effort=…`) |
| Traducción nivel → modelo | `model-catalog.md` (no normativo) | Encabezados por proveedor+transporte; sin rol ni sesión principal |
| Composición de prompts | PROMPT_TEMPLATES §G | Neutral; un único formato de texto |
| Procedimiento | README (subordinado) | §1 Controller = Codex CLI; §5 receta Codex; §6 subagente |
| Relevo entre sesiones | WORKFLOW §3 | Neutral |
| Evidencia, trailer | AGENTS.md | Neutral |
| Autoverificación del Principal | **sin dueño normativo**; regla interina en la memoria local de Claude y seguimiento formal en `ideas-futuras.md` | Memoria privada en lugar de una autoridad versionada |

La asignación de dueños para las obligaciones nuevas es EXP-02 (§10.2).

## 3. DC-03 — Artefactos de proceso, esquemas y persistencia

Los cinco esquemas son estrictos (`additionalProperties: false` recursivo) y **no preservan campos desconocidos**: un campo nuevo exige versión
nueva (MEASURED; OBL-02).

| Esquema | Neutral | Acoplamiento medido |
|---|---|---|
| `rackcad-gate-contract/v1` | `EligibleCells[].Provider`/`.Transport` libres | sin requisito de independencia ni binding de roles distintos del Worker |
| `rackcad-delegation/v1` | `Executor.Provider`/`.Transport`/`.Role`/`.Cell` libres; `Effort.Semantic` | `Owner.Kind` ∈ {`subagent`, `process`} (categoría de transporte); `ExpectedHandoffPath` bajo `artifacts/orchestration/` |
| `rackcad-worker-handoff/v1` | `Worker.Provider` libre | — |
| `rackcad-controller-verification/v1` | estados semánticos | **no registra** la identidad del verificador |
| `rackcad-relay-record/v1` | `Phase`, `Outcome`, `RemoteFacts` | `Participant.Kind` ∈ {`codex-cli`, `subagent`}; `Exit`/`Entry` exigen `ConfigTomlSha256`/`ConfigTomlKeys`; `RebaseMap.G2CloseSha`; `TestArtifacts` sin `Skipped`; sin host, máquina ni sesión principal |

**Otros artefactos:**
- tráfico transitorio en `artifacts/orchestration/…`, ignorado (`.gitignore` l. 3);
- custodia en `docs/automation/evidence/<unit>-pilot/…`;
- estado canónico `state/<I>.yml` (AUTOMATION_PLAN §8), sin campos de roles, bindings ni propietarios de alcance.

No hay **ningún registro durable de custodia de roles**: la custodia se deduce del relevo (`Owner`, `Cession`) y de `execution_context.worktree`.

## 4. DC-04 — Camino roles/autoridades → contrato → paquete → relevo → entrega → verificación → estado/custodia

| Paso | Símbolo / documento | Productor | Validación existente |
|---|---|---|---|
| Autoridades | `AuthorityRevision`, clases `UNIT_DOC`/`UNIT_CHANGE`/`EXTERNAL` (16.3) | Coordinator | `Authority` |
| Contrato | `gate-contract.json` (16.5) | Coordinator | esquema (sin prueba de instancia) |
| Paquete | `delegation.json` | Controller | A1 `Test-Json`; A2-A8 manuales |
| Prompt | `prompt.md` = §G.1 + §G.2 + delta | sesión | SHA-256 (P-05) |
| Relevo | `relay-record.json` | sesión | P-01, P-02, P-08 |
| Entrega | `worker-handoff.json` + commits | Worker | `Handoff`, `Identity`, `Scope`, `Trailer`, `FreeText` |
| Hechos remotos | `RemoteFacts` | sesión | `Ci`, `Tests` |
| Verificación | `controller-verification.json` | Controller | README §8 + `Test-Json` |
| Estado y custodia | `state/<I>.yml`, `evidence/<unit>-pilot/` | sesión | 16.12; sin prueba mecánica |

## 5. DC-05 — Productores, lectores, validadores, plantillas, normas y consumidores

- **Validadores:** `Test-Json`, `--output-schema` (solo Codex), regla de coherencia del README §8 y `AgentExecutionProtocolTests`.
- **Plantillas:** §G y README.
- **Normas:** §16, ADR-0046 y el Freeze de I-61.
- **Consumidores:** inventario completo en EXP-04 (§10.3).
- **Huecos:**
  - otras cuentas o repositorios no se inspeccionaron;
  - los scripts de relevo de I-61 no están versionados (solo su SHA-256, I-61 ev. §16.6).

## 6. DC-06 — Pruebas y guardas existentes

**Corrida focal** (MEASURED en la entrega inicial, sobre `85ae4324`, árbol `d277aaa4…`, SDK 8.0.423):
`AgentExecutionProtocolTests` con `FullyQualifiedName~` → 17 seleccionadas, 17/17 superadas, 0 omitidas (evidencia §10).
- Es una medición de la sesión para contrastar afirmaciones, **no** evidencia de gate, y **no se traslada** a `f25dd1d5` ni a commits posteriores.
- En R1 no se repitió: ninguna afirmación nueva lo exige y los archivos guardados no cambiaron.

| Obligación (Freeze de I-61 §13) | Protege | Relevancia para I-62 |
|---|---|---|
| OBL-01 | estados exactos; sin términos de gate en enums | neutral |
| OBL-02 | esquemas estrictos | **lista fija** de los cinco `/v1`: una versión nueva exige cambiar la prueba |
| OBL-03 | SHA de 40 hex | neutral |
| OBL-04 | `artifacts/` ignorado | neutral |
| OBL-05 | `routing.md` sin nombres de modelo | conjunto prohibido = catálogo + `gpt-`/`claude-`/`o<n>-`/`gemini-`. Un proveedor futuro fuera del catálogo y de esos patrones no se detecta (INFERENCE) |
| OBL-06 | §G: 8 perfiles y topes; subordinación; frases testigo | un perfil o clase nuevos deben respetar topes y la lista fija de `Profiles` |
| OBL-11 | 14 comprobaciones | neutral |

Huecos (MEASURED):
- sin prueba de neutralidad de `relay-record` ni de las recetas;
- sin prueba de autoverificación ni de preflight de entorno;
- OBL-07..10 son controles manuales.

## 7. DC-07 — Archivos calientes e intersecciones activas

| Rama | Rutas que toca respecto de su merge-base (superficies candidatas de I-62) | Delta R1 |
|---|---|---|
| I-52 (`c1982b2a`) | `docs/HANDOFF.md`, `docs/ROADMAP.md`, `docs/adr/README.md`, `docs/adr/0036-…` | sin cambio |
| I-63 (`b557ea3b` → `dbfe1150`) | `docs/ROADMAP.md` | `dbfe1150` solo añade documentos propios (`I-63-*`); ninguna superficie candidata |
| I-64 (`d8a02163` → `cf034e59`) | `docs/ROADMAP.md` | `cf034e59` solo cambia `I-64-discovery.md`, `I-64-evidence.md` y su contrato; ninguna superficie candidata |

**EXP-07:** negativo, sostenido en R1 (§10.7).

## 8. DC-08 — Fundaciones

Solo cuentan las entradas de `docs/FOUNDATIONS.md`. Las autoridades de proceso (WORKFLOW, LIFECYCLE, AGENTS) **no son fundaciones**: I-62 las obedece
como autoridades, no las «consume» como entradas.

| Entrada de FOUNDATIONS | Verificación en la base (MEASURED) | Propuesta |
|---|---|---|
| **Agent Execution Protocol** (l. 160-169, `STABLE`) | autoridad, persistencia, contrato de mutación, puntos de extensión (Freeze de I-61 §15) y pruebas coinciden con las fuentes; las limitaciones declaradas coinciden con DC-01..06 | **Extends** |
| Las otras 12 entradas (Rack Identity … Linked Properties) | de producto; «Unknown-field Preservation in Persisted Envelopes» se refiere a sobres persistidos del producto, no a esquemas de proceso | no consumidas |

```text
Consumes: ninguna entrada de FOUNDATIONS
Extends: Agent Execution Protocol
Introduces: UNKNOWN; depende de las decisiones de diseño de §9 (frontera de adapters y registro de custodia). Decisor: Coordinator + Architect en diseño
```

## 9. DC-09 y EXP-09 (continuación R1) — Evaluación semántica de materialidad

EXP-09 queda **abierta con UNKNOWN y decisor**; no se cierra en R1. Criterio (R62-F0-01): decide la semántica (quién decide y aplica la regla, quién
posee el estado, productores y consumidores, persistencia y alcance transversal), **no la ubicación**. Ni extender un YAML o escribir dentro de §16
prueba que no se cree autoridad o persistencia, ni crear un archivo prueba que se cree. Una versión `/v2` no fija el arquetipo.

### 9.1 Obligación del mandato → semántica actual → qué cambia

| Obligación (mandato) | Quién decide/aplica hoy · estado · consumidores (MEASURED) | Qué cambia semánticamente (INFERENCE) |
|---|---|---|
| Roles independientes del proveedor (cinco roles) | §16.1 fija participantes y declaraciones; ADR-0046 #2 | la semántica de declaraciones se mantiene; desaparece la asociación rol↔proveedor; REVIEWER y PRINCIPAL_COORDINATOR entran como roles con requisitos |
| Controller sin proveedor fijo | decisión aceptada (ADR-0046) | cambia una decisión integrada (M-08) |
| Role binding por hechos observables | la sesión elige la celda del Controller (`routing.md` §7); el Controller la del Worker; el Owner elige el modelo de la sesión principal con el selector | **quién decide o valida el binding del Principal** pasa del selector del Owner (sin regla) a una regla con requisito y verificación: aparece una regla nueva sobre un valor que hoy no tiene dueño normativo |
| Perfil y autoverificación del Principal | sin dueño normativo (§2) | regla y STOP nuevos. Estado nuevo: observaciones de configuración de cada sesión principal, consumidas por cualquier Principal posterior y por el Coordinator |
| Preflight de entorno agnóstico | 16.4 lo aplica la sesión solo en el relevo; los hechos se registran en `relay-record` | se aplica antes del binding y por runtime: hechos nuevos por adapter, que consumen el routing y el Coordinator |
| Frontera de adapters | no existe como concepto; recetas en el README y datos en el catálogo; el núcleo (§16.4, `relay-record`) contiene hechos de Codex | aparece un **contrato** entre las reglas del núcleo y cada proveedor: describir el runtime, observar capacidades, renderizar, invocar, observar el resultado, cancelar y confirmar la terminación. Lo consumen todas las unidades y todos los proveedores futuros («permit future providers without schema redesign») |
| Independencia por riesgo | sin regla; ADR-0046 acepta la «independencia parcial» | regla nueva; quién la aplica (Coordinator, Architect, lifecycle) es EXP-02 (§10.2) |
| Custodia, huérfanos y portabilidad de contexto | WORKFLOW §3 (worktree y relevo entre sesiones; dueño de hecho: refs remotas), 16.6 (un escritor), `state/<I>.yml` (por unidad) | estado nuevo: Principal, Controller, Worker y Reviewer; propietarios de rama, worktree y **alcance de escritura** («No simultaneous writers to same mutable scope»). Lo consume un Principal de otra máquina o proveedor. Alcance por unidad o entre unidades (ventanas como las de ROADMAP): **UNKNOWN** |
| Presupuestos | 16.8 «Sin reinicios» | nombrar al proveedor; no cambia quién decide |
| Prompts neutrales | §G neutral | renderizador por PromptProfile: concepto nuevo de procedimiento |

### 9.2 M-01..M-08

| M | Disparador | Estado | Evidencia, límite y decisor |
|---|---|---|---|
| M-01 | cambia quién posee una regla o valor, o aparece una segunda autoridad | **UNKNOWN** | Dos riesgos concretos. (a) Un registro de custodia que **declare** propietarios de worktree o rama sería una segunda autoridad frente a WORKFLOW §3 y los hechos de Git, salvo que quede subordinado a ellos (WORKFLOW §10). (b) La regla de binding del Principal da dueño a un valor que hoy decide el Owner con el selector. Decisor: Coordinator + Architect en diseño, tras EXP-02. Si cambia el mapa de dominios de WORKFLOW §10: Owner |
| M-02 | cambia esquema, wire o significado persistido (creador frente a modificación) | **ACTIVADO como modificación; creador UNKNOWN** | Modificación: `relay-record` (y quizá `gate-contract`/`delegation`) necesita versión nueva. Creador: las observaciones del Principal y la custodia de roles son **datos nuevos** con consumidores transversales. Que se guarden en el estado existente o en un artefacto nuevo no decide la cuestión. Decisor: Coordinator + Architect |
| M-03 | conducta observable fuera de lo pedido | **NO ACTIVADO** | proceso de desarrollo; sin producto |
| M-04 | qué falla o cómo | **ACTIVADO** | fail-closed exigido por el mandato (ROLE BINDING; criterio 4) frente al «no detiene por sí solo» del seguimiento de I-61 (ideas-futuras l. 1551). Es un delta propuesto de semántica de fallo, no una contradicción entre autoridades integradas (§10.1) |
| M-05 | contrato consumido por otros | **ACTIVADO** | §16 y los esquemas `/v1`: instancias custodiadas de I-61 y uso previsto por I-63 e I-64 (§10.3) |
| M-06 | punto de extensión | **ACTIVADO** | usa y amplía los puntos de I-61 §15. La frontera de adapters sería un punto de extensión para proveedores futuros |
| M-07 | framework, registro o mecanismo genérico transversal | **UNKNOWN** | La frontera de adapters es, por definición del mandato, un contrato genérico para todos los proveedores. Si es un **mecanismo nuevo** o una **extensión** del catálogo y las recetas existentes depende de cuánto dependan de ella las reglas del núcleo, y eso lo fija el diseño. Lo mismo vale para un registro de custodia entre unidades. Decisor: Coordinator + Architect en diseño |
| M-08 | modifica un ADR aceptado | **ACTIVADO** | ADR-0046 fija el proveedor del Controller y acepta la independencia parcial; desfijarlo exige un ADR nuevo con su autoridad |

### 9.3 Arquetipo y agrupación

**Arquetipo:** se mantiene **NEW ARCHITECTURE PROVISIONAL**: M-07 y M-01/M-02 creador siguen UNKNOWN, y LIFECYCLE §3 cuenta UNKNOWN como activado.

**FOUNDATION EVOLUTION** sigue siendo una **alternativa**, viable solo si el diseño demuestra que:
- la frontera de adapters y la custodia extienden contratos existentes sin crear uno nuevo del que dependan las reglas del núcleo; y
- ninguna declaración de custodia compite con WORKFLOW §3 o con Git.

No se propone un diseño mínimo para bajar la clasificación. La etiqueta del mandato no cambia. **Agrupación** (LIFECYCLE §2):
- **Iniciativa conceptual.** Las obligaciones del mandato cumplen las dos condiciones: el mismo objetivo observable (roles independientes del proveedor y
  reanudación sin memoria privada) y autoridad de diseño compartida (§16, los esquemas y el routing). Separadas, se diseñarían dos veces.
  Fuera de la agrupación quedan:
  - punto 6 del triage (filtros): no comparte el objetivo;
  - punto 7 (Level B): diferido;
  - remedio general de C-F0-RED: no comparte el objetivo (§12).
- **Unidades de entrega.** Es un concepto distinto del gate funcional: una unidad se divide por sistemas, superficies, archivos calientes o límites de
  rollback, y cada unidad entrega algo verificable por sí misma. Límite candidato: los **pilotos entre proveedores** (F6) dependen de acciones del Owner sobre
  runtimes (autenticación, sandbox) que pueden tener otro calendario y otro rollback. Si eso justifica otra unidad: **UNKNOWN**. Decisor: Coordinator antes del
  diseño. Esta ronda no abre ni divide unidades.

## 10. Expansiones (EXP-01..09)

| EXP | Resultado R1 |
|---|---|
| EXP-01 | verificada: **las dos observaciones se retiran**; ninguna discrepancia en cláusula consumida (§10.1) |
| EXP-02 | **activada (C62-F0-04)**: matriz en §10.2; hay dos casos de dos autoridades candidatas y dos sin dueño |
| EXP-03 | negativo sostenido (§10.7) |
| EXP-04 | **activada**: consumidores reales frente a previstos (§10.3) |
| EXP-05 | **activada**: tabla invariante → observable → oráculo → control (§10.4) |
| EXP-06 | **activada**: agregación de estados (§10.5) |
| EXP-07 | negativo sostenido con delta (§10.7) |
| EXP-08 | **activada**: impacto sobre las deudas (§10.6) |
| EXP-09 | continuada; **abierta** con UNKNOWN y decisor (§9) |

### 10.1 EXP-01 — verificación acotada

| Observación de la entrega inicial | Cláusulas exactas (blob `0e3ed262` / `ideas-futuras` `e5641f0e`) | Resultado |
|---|---|---|
| (1) PROMPT_TEMPLATES y la vigencia de Workflow V2 | **Preámbulo, l. 3:** «**Materialized for I-56; not effective until WORKFLOW_V2_EFFECTIVE_SHA exists.**», una condición que se cumple porque el SHA efectivo existe (`8a021fb6`, WORKFLOW §11.2). **§1, l. 13:** «materialized by later I-56 normative gate; hasta entonces se cita ademas la clausula correspondiente de Proposal V4», una condición temporal ya superada. **§2 «Traza y vigencia», l. 274-277:** bloque `text` con «WORKFLOW V2 = NOT EFFECTIVE» / «WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST», introducido por `7e9073e7` (2026-09-15), antes de la activación (2026-09-17). Nota: la cita «§2» de la entrega inicial sí localiza este bloque | **Retirada como EXP-01.** El bloque de §2 es una **afirmación factual obsoleta**, no una cláusula normativa. WORKFLOW §10 la resuelve: «Los hechos de Git … vencen una afirmación factual falsa». La vigencia que I-62 usa sale del preámbulo y de los hechos de Git, no de ese bloque. EXP-01 solo cubre contradicciones entre FOUNDATIONS, ADR aceptado, Freeze o código, y este bloque no es ninguno de los cuatro. Queda como **deuda** en EXP-08 (§10.6), porque I-62 editará §G y quizá §2 |
| (2) Fail-closed del mandato frente a «UNKNOWN no detiene» | ideas-futuras l. 1551 (seguimiento no normativo) y la memoria local (no versionada) frente al mandato de I-62 (ROLE BINDING: «Fail closed when a required capability cannot be accredited») | **Retirada como EXP-01.** No son dos autoridades integradas que se contradigan: es el **delta propuesto de M-04** (§9.2) y no se cuenta dos veces |
| Prosa histórica «materialized by later I-56 normative gate» (LIFECYCLE l. 21-24, 145, 150, 192, 225, 241-242; PROMPT_TEMPLATES §1 y plantillas A-F) | Marcadores de una materialización ya hecha. LIFECYCLE l. 150 («se materializan en otro gate de I-56») coincide con Git: FOUNDATIONS existe | **No es discrepancia.** Las cláusulas que I-62 usa (§4.1: reglas de Consumes/Extends/Introduces) no dependen del marcador |

Resultado: **ninguna clase A ni B** queda propuesta; no se fabrica una revisión B.

### 10.2 EXP-02 — Obligación → dueño actual o ausente → delta → autoridad decisora

Lo que sigue es una propuesta; no asigna autoridad normativa nueva.

| Obligación nueva o cambiada | Dueño actual | Situación | Delta propuesto (alternativas) | Decisor |
|---|---|---|---|---|
| Semántica de los cinco roles | §16.1 (AUTOMATION_PLAN) | un dueño | quitar los proveedores de la tabla y añadir PRINCIPAL_COORDINATOR y REVIEWER | Coordinator + Architect (Freeze); ADR que supere en parte ADR-0046: Owner |
| Obligación de autoverificación del Principal y su STOP | **ninguno** (seguimiento no normativo; memoria privada) | **sin dueño** | (a) regla en §16 + perfil en `routing.md` + celda en el catálogo + referencia en WORKFLOW §4 «al abrir»; (b) solo WORKFLOW §4 + routing | Coordinator + Architect; si se toca WORKFLOW §4 o el mapa de §10, sus autoridades (Owner si cambia el mapa) |
| Frontera de adapters (metadatos por proveedor) | ninguno; recetas en el README, datos en el catálogo | **sin dueño del contrato** | (a) contrato normativo en §16 con datos en el catálogo; (b) solo procedimiento subordinado (README/catálogo) | Coordinator + Architect |
| Independencia por riesgo | ninguno para la ejecución delegada; LIFECYCLE §5 rige la participación y revisión del Architect | **dos candidatas**: LIFECYCLE §5 (revisión/Architect) frente a AUTOMATION_PLAN §16 (verificación delegada) | una regla y un dueño; la otra solo enlaza | Coordinator; si cambia el mapa de WORKFLOW §10, Owner |
| Custodia de roles, propietarios y alcance de escritura | WORKFLOW §3 (worktree, relevo entre sesiones), AUTOMATION_PLAN 16.6 y §8 (un escritor; estado por unidad) | **dos candidatas** | registro subordinado a Git y a WORKFLOW §3; ubicación y alcance (por unidad o entre unidades) a decidir | Coordinator; alcance entre unidades = decisión de alcance (Owner si excede el mandato) |
| Huérfanos y recuperación | 16.11 (delegada) + WORKFLOW §3 (sesiones) | dueños divididos por dominio | delimitar: participante delegado → §16; sesión principal → WORKFLOW §3 | Coordinator + Architect |
| Preflight de entorno | 16.4 (solo relevo) | un dueño | extender 16.4 con núcleo + hechos por adapter | Coordinator + Architect |
| Role binding de todos los roles | `routing.md` (subordinado), elegibilidad normativa en el Freeze de I-61 | un dueño | extender §§4-5 | Coordinator + Architect |
| Presupuestos | 16.8 | un dueño | nombrar al proveedor | Coordinator |
| Renderizador de prompts | PROMPT_TEMPLATES §G | un dueño | procedimiento por PromptProfile | Coordinator |
| Estados de autenticación sin secretos | AUTOMATION_PLAN §3 + S-06 | un dueño | estados AUTHENTICATED / NOT_AUTHENTICATED / UNKNOWN en el preflight | Coordinator |

### 10.3 EXP-04 — Consumidores de `/v1` y compatibilidad

| Consumidor | Tipo | Qué lee o produce (MEASURED) | Versión | Delegación abierta |
|---|---|---|---|---|
| Custodia de I-61 (`docs/automation/evidence/I-61-pilot/`) | **real, histórico** | 2 `gate-contract.json`, 4 `delegation.json`, 3 `worker-handoff.json`, 6 `controller-verification.json` y 12 `relay-record.json` (más 2 `acceptance*.json`, 9 `prompt.md` y 4 `analysis.md` fuera de esquema) | `/v1` | ninguna (I-61 integrada) |
| `AgentExecutionProtocolTests` | **real** | los cinco esquemas (lista fija), no instancias | `/v1` | — |
| AUTOMATION_PLAN §16 / README / §G | **real** (normas y procedimiento) | nombran los cinco esquemas | `/v1` | — |
| I-63 (`dbfe1150`) | **previsto** | su contrato declara delegar bajo §16 (l. 134); su Discovery fue directo; C-F0-RED registrado como limitación (`decisions/I-63.md` l. 34, 61) | `/v1` cuando delegue | **ninguna acreditada** en lo publicado; lo no publicado: UNKNOWN |
| I-64 (`cf034e59`) | **previsto** | su contrato y su brief declaran usar I-61 (contrato l. 160; brief l. 44); su Discovery fue directo | `/v1` cuando delegue | **ninguna acreditada**; lo no publicado: UNKNOWN |
| I-52 (`c1982b2a`) | ninguno | no referencia los esquemas (`git grep`) | — | — |
| Otras ramas remotas | ninguno | solo AUTOMATION_PLAN menciona los esquemas fuera de I-61/I-62 | — | — |

**Qué protege la interpretación** durante una adopción futura (MEASURED en las fuentes):
- cada instancia lleva `Schema` con su versión exacta (enum de un valor);
- el Freeze de I-61 §15: «`/v1` sigue válido hasta su retirada explícita»;
- la custodia es por blob (16.12).

**Límites de adopción** (INFERENCE para diseño):
- una delegación `/v1` abierta termina en `/v1`;
- los lectores distinguen versiones;
- no se reinterpretan artefactos históricos;
- ninguna unidad activa cambia de protocolo a mitad de camino (mandato).

**Revalidación al integrar:** repetir este inventario en READY-04 (delegaciones `/v1` abiertas en las ramas activas). Su estado futuro no se predice.

### 10.4 EXP-05 — Observables no circulares (diseño de prueba, no ejecutado)

Distinción obligatoria:
- **solicitado**: selector, flag o petición;
- **configurado**: ajuste persistido del runtime;
- **observado**: metadato escrito por el runtime;
- **efectivo**: lo que el servicio aplicó; sin atestación del servidor: UNKNOWN;
- **identidad** (qué modelo/effort) ≠ **suficiencia** (si alcanza el nivel requerido).

**El modelo no se acredita porque él mismo lo declare.**

| Invariante | Observable · fuente · identidad (MEASURED salvo indicación) | Oráculo previo | Control negativo | Limitación |
|---|---|---|---|---|
| I-01 Identidad del Principal Claude (escritorio) | `get_session("self")` → `model`, `effort` (metadatos de la app, no del modelo), ligado a `sessionId` e instante; transcripción con `"model"` por mensaje (I-61 ev. §11) | perfil requerido + celda del catálogo, registrados antes de la sesión | perfil requerido distinto del observado → BELOW_REQUIRED o UNKNOWN, nunca MATCH | observado ≠ efectivo; el `effort` de la app es la configuración aplicada por el cliente |
| I-02 Identidad del Principal Codex (escritorio) | **SP-3** (§17): `~/.codex/sessions/AAAA/MM/DD/rollout-*.jsonl`; `session_meta.originator` (`Codex Desktop` / `codex_work_desktop`), `source`, `thread_source`, `cli_version`; cada `turn_context` con `model` y `effort` e instante | ídem | ídem | candidata, no acreditada: falta ligar la sesión a la unidad y a su rol; observado ≠ efectivo. Un registro de `codex_exec` (CLI) no acredita una sesión de escritorio |
| I-03 Suficiencia del nivel | no observable en el runtime: clasificación de RackCad en el catálogo (nivel por modelo, fuente y fecha) | entrada de catálogo vigente (no `STALE`) | entrada `STALE` o modelo fuera del catálogo → UNKNOWN | es una asignación de RackCad, no una propiedad medida |
| I-04 Reconstrucción sin memoria privada | respuestas de B en un artefacto propio, con su identidad (I-01/I-02) | **lista cerrada de hechos custodiada antes del ensayo**: commit en un SHA conocido, con SHA-256 del archivo, preparada por quien no es B y antes de que B empiece. B no la ve | quitar un hecho del estado versionado: B debe declarar UNKNOWN o STOP, no inferirlo | otro Principal real o una sesión sin contexto compartido; no se simula renombrando el autor |
| I-05 Terminación del escritor anterior | árbol de procesos + registro de relevo (16.4) en el mismo host | lista de PID y árbol en la salida | proceso vivo → P-02 | en otra máquina: UNVERIFIED; exige cesión registrada, push y prueba de terminación |
| I-06 Presupuestos tras cambiar de proveedor | `attempts` en el estado y contadores por clase en los registros | valores antes del cambio | cambio de proveedor con contador a 0 → fallo | existe la regla (16.8); el ensayo es PENDING |
| I-07 Preflight completo | por requisito: fuente, instante, valor y disposición | lista de requisitos del perfil | autenticación sin acreditar → NOT_AUTHENTICATED/UNKNOWN → no elegible | una CLI encontrada no acredita una invocación |

### 10.5 EXP-06 — Agregación de MATCH / ABOVE_REQUIRED / BELOW_REQUIRED / UNKNOWN

**Ya ordenado** por el mandato o por normas vigentes:
- BELOW_REQUIRED → STOP antes de trabajo sustantivo;
- UNKNOWN → no adivinar e indicar la configuración o verificación requerida;
- ABOVE_REQUIRED → permitido, registrando por qué;
- fail-closed si no se acredita una capacidad requerida;
- UNKNOWN nunca se convierte en MATCH;
- `routing.md` §5: «Lo desconocido no es elegible» y `STALE` no es elegible;
- 16.11: S-04 (evidencia contradictoria) → STOP;
- precedencia 16.9: STOP > BLOCKED > REWORK.

**Regla nueva propuesta** (para Freeze; no vigente):

| Estado de un requisito | Obligatorio | Opcional (métrica) |
|---|---|---|
| MATCH | satisfecho | registrado |
| ABOVE_REQUIRED | satisfecho; se registra el motivo | registrado |
| BELOW_REQUIRED | **no satisfecho** (insuficiencia conocida) | registrado |
| UNKNOWN | **no acreditado → no elegible** | registrado; no bloquea |
| Fuentes contradictorias | UNKNOWN + STOP S-04 si la contradicción es material | registrado |
| Observación obsoleta (cambio de máquina, runtime, credencial, catálogo `STALE` o SHA) | UNKNOWN hasta revalidar | registrado |

**Disposición global** (precedencia):
1. algún obligatorio BELOW_REQUIRED → **BELOW_REQUIRED → STOP**, aunque haya otros UNKNOWN;
2. si no, algún obligatorio UNKNOWN → **UNKNOWN** → el rol no se vincula ni ejecuta trabajo sustantivo que dependa de él; se indica qué acreditar;
3. si no, alguno ABOVE_REQUIRED → **ABOVE_REQUIRED**;
4. si no, **MATCH**.

**Recuperación:**
- BELOW_REQUIRED → reconfigurar y volver a observar;
- UNKNOWN → aportar una fuente acreditada o la decisión de la autoridad competente;
- en ningún caso hay ascenso automático.

**Delta frente al seguimiento de I-61:** el caso 2 aplicado a la sesión principal sustituye el «UNKNOWN no detiene por sí solo un Candidato». Es M-04; lo decide el
Freeze (Coordinator + Architect) dentro del mandato.

### 10.6 EXP-08 — Deudas que la evolución haría visibles o empeoraría

| Deuda | Camino de I-62 afectado | Riesgo | Disposición | Prueba o límite futuro |
|---|---|---|---|---|
| C-F0-RED (§12) | delegar tareas documentales o de caracterización de I-62 | presión para fabricar un RED; I-62 no puede delegar su documentación | evidencia + restricción de ruta; remedio general → SEPARATE UNIT propuesta | ninguno en I-62 |
| Lógica de relevo no recuperable (scripts de I-61) | preflight de procesos en relevos de I-62 y en otro host | **MEASURED:** el README §3.2 versionado no recoge los criterios aplicados en G3 (relectura a los 2 s, `conhost` de la cadena; `grep` sin coincidencias) → el procedimiento versionado **no basta** para repetir el resultado de I-61 | IN SCOPE (puntos 1 y 9) | la repetición se demuestra con procedimiento versionado y un caso positivo y uno negativo, no reconstruyendo bytes |
| «cierre de G2» en reglas generales (16.3, 16.5, 16.7; `RebaseMap.G2CloseSha`) | `AuthorityRevision` y reemisión de contratos en cualquier unidad distinta de I-61, incluida I-62 | sin valor inicial definido para otras unidades | IN SCOPE como evidencia y restricción. Hasta el diseño, un contrato de I-62 tendría que fijar su equivalente por decisión del Coordinator | coherencia del esquema nuevo de relevo |
| `RemoteFacts` sin `Skipped` | completitud de evidencia entre adapters | conteos no comparables | IN SCOPE (punto 5) | campo en la versión nueva |
| Sufijo `-pilot` de la custodia | custodia de unidades no piloto | ruta engañosa | diseño | — |
| Patrón hex de `AnalysisSha256` (`[0-9a-f]`) frente a `[0-9A-Fa-f]` del relevo | coherencia de esquemas si se versionan | rechazo de mayúsculas | independiente salvo que se versione `delegation` | — |
| Ruta del binario de Codex registrada en I-61 (obsoleta) | receta del Controller | invocar una ruta inexistente o heredar la medición | IN SCOPE (preflight) | revalidación por invocación |
| PROMPT_TEMPLATES §2, l. 274-277 (afirmación obsoleta) | edición de §G y de «Fuente aprobada de §G» en la misma sección | dejar una afirmación falsa junto a texto que I-62 edite | decisión del diseño (corregir al editar la sección o dejarla) | — |

### 10.7 EXP-03 y EXP-07

- **EXP-03, negativo sostenido.** Toda la persistencia de proceso está localizada (§3). Los scripts no versionados son contexto privado (EXP-08), no un
  legado ilocalizable.
- **EXP-07, negativo sostenido.** Con el delta de §7 (I-63 `dbfe1150`, I-64 `cf034e59`), ninguna rama activa toca §16, agent-execution, PROMPT_TEMPLATES,
  WORKFLOW, AGENTS, FOUNDATIONS ni la prueba guardiana. El cruce sigue siendo textual en ROADMAP, HANDOFF e índice ADR.

No se detectó ningún disparador nuevo sin cubrir.

## 11. Capacidades por host, runtime, rol y fecha

Host: workstation Windows del Owner. Cada fila separa capacidad **medida en I-61**, **observación de G0** (14:56Z), **observación de esta sesión** y
**elegibilidad ACTUAL** de una invocación. Una ruta o binario cambiado **no hereda** el resultado de una celda anterior, y la fecha del catálogo no renueva
una medición de host.

| Runtime (versión) | Rol | Capacidad / effort | Fuente y fecha | Consumo | Invalidadores aplicables | Elegibilidad actual · revalidación que falta |
|---|---|---|---|---|---|---|
| App de escritorio Claude (esta sesión) | PRINCIPAL_COORDINATOR | observado `claude-opus-5-5` / `xhigh` | `get_session`, 16:36Z (esta sesión) | sesión de la app | cambio de selector o de sesión | observación vigente (MATCH frente al perfil **propuesto**); sin perfil I-62 vigente |
| Subagente de la sesión Claude | WORKER | `claude-sonnet-5-5` × `write-commit-push` × `medium`/`high` | U-04 y G3 de I-61, 2026-10-01 (RECONSTRUCTED) | cubierto por suscripción (medido en I-61) | cambio de app o de sesión; catálogo `STALE` | **no revalidado en este host desde I-61**: elegible según el catálogo (2026-09-30/10-01), pero sin medición de esta sesión; requiere aceptación A7 al delegar |
| Subagente de la sesión Claude | REVIEWER / ARCHITECT (solo lectura) | `claude-opus-5-5` heredado, `xhigh`/solicitado `high` | I-61 ev. §§11, 13 (RECONSTRUCTED) | ídem | ídem | lectura acreditada en I-61. **Independencia: no**; un subagente de la misma sesión no es independiente por su marca |
| Claude CLI 2.1.270 (`~\.local\bin`) | rol externo (topología B) | — | `claude auth status` → NOT_AUTHENTICATED (G0, 14:56Z) | UNKNOWN | — | **UNVERIFIED**: autenticar es acción del Owner |
| Codex CLI 0.159.2 (`bin\de8a38d2…`; I-61 usó `bin\c6fe824d…`, ya inexistente) | EXECUTION_CONTROLLER (solo lectura) | `gpt-6-luna` × `high` | PR-1 y G3 de I-61, 2026-10-01 (RECONSTRUCTED); login «ChatGPT» (G0) | cubierto en I-61 sin aviso de límite | binario cambiado; `config.toml` distinto de la referencia (§13.1) | **no elegible ahora**: falta una invocación medida con el binario actual y la disposición de `config.toml` (Owner) |
| Codex CLI | WORKER (`write-commit-push`) | — | sonda P2 de I-61: el sandbox rechazó crear procesos (RECONSTRUCTED) | UNKNOWN | — | **UNVERIFIED** (SP-2 no autorizada) |
| App de escritorio Codex | PRINCIPAL_COORDINATOR (topología B) | metadatos `turn_context.model/effort` disponibles (SP-3, §17) | inspección pasiva entre 17:03Z y 17:15Z (esta sesión) | UNKNOWN | — | **UNVERIFIED**: fuente candidata de identidad; sin perfil, sin ligar a la unidad, sin invocación |
| Canal de coordinación entre sesiones de otro runtime u otra máquina | coordinación (acuses/RELEASE) | — | solo acreditado entre sesiones locales de Claude Desktop | — | — | **UNVERIFIED** para Codex u otra máquina; no se afirma que no exista |
| GitHub CLI 2.96.0 / Git 2.54.0 / SDK 8.0.423 de usuario | todos | lectura remota, Git, build/test | G0 y esta sesión | n/a | cambio de host o credencial | acreditados en este host |

`service_tier`: efecto UNKNOWN (I-61); no se modificó ni se infiere consumo cubierto.

## 12. Triage de los 13 puntos (disposición del Coordinator C62-F0-05)

| # | Punto | Disposición | Alcance en este Discovery |
|---|---|---|---|
| 1 | Relectura de procesos | IN SCOPE | contrato de observación y atribución, sin relajar el STOP. EXP-08: el procedimiento versionado no basta hoy |
| 2 | Criterio de huérfanos | IN SCOPE | distinguir la falta de prueba de terminación del huérfano confirmado; preservar custodia y trabajo |
| 3 | Reverificación | IN SCOPE | camino y presupuestos; no se aplica por adelantado |
| 4 | CONTROLLER_VERIFICATION | IN SCOPE | semántica Exit/Entry explícita e independiente del proveedor |
| 5 | `RemoteFacts`/`Skipped` | IN SCOPE | completitud y coherencia de evidencia entre adapters; diseño pendiente |
| 6 | Validación previa de filtros | DEFER | mejora normativa o de tooling propia. Siguen vigentes la selección suficiente y la evidencia real; validar un filtro no exige tooling Level B |
| 7 | Level B | DEFER F5 (preliminar) | sujeto a demostrar que Level A basta y al Freeze; ningún helper aprobado |
| 8 | Temporizador del Worker | IN SCOPE | obligación observable de timeout y terminación del adapter; no es implementación |
| 9 | Falsos positivos de P-02 | IN SCOPE | junto con 1, sin duplicar investigación ni quitar controles |
| 10 | Worker Codex con escritura | IN SCOPE | estudio de capacidad y diseño de sonda; **SP-2 no autorizada** |
| 11 | `service_tier` | IN SCOPE | observación y riesgo; sin modificarlo ni inferir consumo |
| 12 | Portabilidad del handoff | IN SCOPE | incluida la recuperabilidad de los hechos necesarios (§13.2) |
| 13 | Fricción de consumidores | IN SCOPE como evidencia | remedio general de C-F0-RED propuesto como SEPARATE UNIT (abajo) |

Ningún punto es OBSOLETE.

**C-F0-RED, acotado** (R62-F0-06):
- Es un conflicto **detectado al preparar un uso real** (I-63, `decisions/I-63.md` §5 en `b557ea3b`; aceptado en `dbfe1150` como limitación del protocolo)
  y por análisis estático de 16.8-16.9.
- **No** es una invocación delegada fallida observada.

| `ChainRedSha` / `ChainRedFiles` | ¿La entrega exige RED? (16.8) | `RequiredTests` / `ExpectRed` | `Ci`/`Tests` posibles (16.9) | Disposición |
|---|---|---|---|---|
| `null` (primera entrega de la cadena) y tarea sin prueba que pueda fallar legítimamente (documentación) | **sí** | `[]`, o `ExpectRed=false` (no exime: «exige RED» depende de la cadena) | sin `RedSha` → `RedPart = fail` → `Ci`/`Tests` `fail` | REWORK; no puede llegar a VERIFIED sin un RED artificial (que no se fabrica) |
| `null` y caracterización (pruebas que pasan sobre el código actual) | **sí** | pruebas en verde | el commit RED no falla por aserción → `RedPart = fail` | REWORK; mismo bloqueo |
| `null` tras un REWORK con `RedPart = fail` | **sí** | — | igual que arriba | igual |
| no nulo (RED acreditado) y la entrega **no** toca `ChainRedFiles` | **no** | cualquiera | `RedPart = not_applicable`; `Ci`/`Tests` según la corrida y la selección | puede llegar a VERIFIED (p. ej., una entrega documental posterior en una cadena con RED legítimo) |
| no nulo y la entrega toca `ChainRedFiles` | **sí** (caducado) | RED nuevo | `pass` solo con un RED nuevo acreditado | REWORK si falta |

Compilar un esquema no es un RED. **Disposición para I-62:** IN SCOPE como evidencia y como restricción de la ruta (§17). Remedio normativo general: **SEPARATE
UNIT** propuesta, sin reclamarla ni modificar I-61 o I-63. Si el diseño demuestra que es indispensable para el mandato, la dependencia se eleva antes del Freeze.

## 13. Configuración, contexto privado y portabilidad entre máquinas

### 13.1 `config.toml` (R62-F0-05)

| Hecho | Valor | Fuente |
|---|---|---|
| Relevos custodiados de I-61 (12, 02:45Z-04:31Z) | `89F375C6…`; `ConfigChanged=false` en todos | `I-61-pilot/**/relay-record.json` |
| Referencia registrada tras el retiro autorizado | `37DD3559E89681B253D8541AD2BFE43B452192E9AB2CD6F66C7503BE186E1E2D` | decisiones de I-61 §17 |
| Observación del host | `42E15A039EFD7E1A9197423AFB14B4358C7D60C89FEDDD734583891DB6E732A5` (G0; igual a las 16:33Z), mtime 05:30:20Z | lectura de la sesión |

**Resultado acotado:**
- **No se observa P-01 en la población inspeccionada** (los 12 relevos custodiados).
- El archivo actual difiere de la referencia registrada: es una **diferencia histórica**. **Origen y ventana exacta: UNKNOWN.**
- No encontrar otro registro custodiado después de 04:31Z ni observar el mtime **no demuestra** que el cambio no ocurriera durante una cesión, ni que no hubiera
  actividad sin custodiar.
- No se declara un P-01 retroactivo ni se levanta ninguno.

**Lo comparado:** nombres de claves, sin valores, de **`89F375C6…`** (el último relevo custodiado) frente al archivo actual. **No** es una comparación de bytes ni
de valores de `37DD3559…` frente al actual, que no está disponible.

| Diferencia | Clasificación |
|---|---|
| Falta la cabecera de proyecto y su `trust_level` | **retiro autorizado** por el Owner (I-61 §17) |
| 11 cabeceras de proyecto con texto distinto; nuevas `[tui]` y `screen_reader_detection_done`; otro orden | **otras diferencias**, de origen UNKNOWN |

De los nombres de claves no se infiere que permisos, modelos o `service_tier` sigan iguales.

**Consecuencia:**
- La referencia no se sustituye ni se acepta ninguna desviación en nombre del Owner.
- Una autorización futura debe identificar el archivo observado y su alcance.
- Esto **restringe la delegación** que dependa de esa disposición, no la documentación ni las lecturas.

### 13.2 Dependencias de contexto privado y de máquina (MEASURED)

| Dependencia | Dónde vive | Efecto |
|---|---|---|
| Regla interina de autoverificación | memoria local de Claude; seguimiento en ideas-futuras | recuperable en parte, sin autoridad normativa |
| Scripts de relevo de I-61 | scratchpad de I-61 (solo su SHA-256) | el procedimiento versionado no repite el resultado (EXP-08) |
| Ruta del binario de Codex | evidencia de I-61 (`bin\c6fe824d…`) | obsoleta en el mismo host |
| `pwsh` del runtime de Codex | README §5 | dependiente del host y del usuario |
| Canal de acuses y RELEASE | mensajería local de Claude Desktop | **UNVERIFIED** para Codex u otra máquina; los acuses quedan en la evidencia |
| Insumos del Owner | `D:\IDs\…` | lo necesario está versionado |
| Custodia de roles | no hay registro | se deduce de relevos y estado |

## 14. Level A frente a Level B (R62-F0-07)

**Fricción observada en un piloto** (RECONSTRUCTED, I-61 ev. §15.11-15.12). Un solo piloto aporta evidencia relevante:
- errores manuales: filtro `Name~` y prompt enmarcado;
- tres verificaciones casi idénticas, con dos S-04 conservadores injustificados, y un falso P-02;
- unos 27 min de relevo de la sesión en 67 min 31 s;
- 7 de 7 invocaciones de Codex.

**Coste de mantener helpers:** UNKNOWN.

**Dirección preliminar:** Level A y DEFER F5, ya aceptada por el Coordinator. **No está demostrado** que la documentación sola satisfaga los oráculos de
portabilidad y recuperación (§10.4). El Freeze debe justificar esa suficiencia o reabrir F5 con evidencia. No se propone ningún umbral de activación.

## 15. Hipótesis y preguntas para los contratos (preliminar; no aprobado)

| Tema | Hipótesis | Pregunta abierta |
|---|---|---|
| Roles | **Los cinco roles del mandato se mantienen** (PRINCIPAL_COORDINATOR, ARCHITECT, EXECUTION_CONTROLLER, WORKER, REVIEWER). El REVIEWER puede compartir actor o proveedor con otro rol, o limitarse a funciones permitidas, pero sigue siendo un requisito | ¿Qué funciones puede acumular un mismo actor sin perder la independencia exigida por riesgo? |
| Perfil del Principal | perfil y celda con los campos del mandato | ¿Qué fuente acredita la identidad por runtime (EXP-05, I-01/I-02)? |
| Estados | MATCH, ABOVE_REQUIRED, BELOW_REQUIRED y UNKNOWN por requisito, con la disposición global de §10.5 | ¿Se adopta la regla propuesta de §10.5? |
| Preflight | núcleo agnóstico + hechos por adapter; antes del binding y en cada relevo | ¿Qué se revalida al cambiar de máquina, runtime, credencial o catálogo? |
| Adapters | contrato a definir (§9.2, M-07) | ¿Contrato normativo o procedimiento subordinado (EXP-02)? |
| Routing | ampliar a todos los roles | ¿El binding se registra en el paquete o en el estado? |
| Independencia | clases del mandato; nombres finales del Freeze | ¿Dueño: LIFECYCLE §5 o §16 (EXP-02)? |
| Custodia | registro subordinado a Git y a WORKFLOW §3 | ¿Por unidad o entre unidades (M-01, M-07)? |
| Recuperación | huérfano = escritor sin terminación confirmada; STOP sin reset ni borrado | ¿Cómo se acredita la terminación en otra máquina (I-05)? |

## 16. Plan propuesto de portabilidad A→B y B→A (PENDING; no ejecutado)

**Precondiciones:**
- Freeze acordado;
- fixture sin `src/`;
- disposición de `config.toml` si interviene Codex;
- celdas acreditadas.

Para B hace falta Claude CLI autenticado por el Owner, la identidad del Principal Codex acreditada (I-02) y Worker Codex con escritura medido.

**Prueba:**
1. A ejecuta bootstrap y un gate y se detiene con su relevo de salida.
2. Se **custodia el oráculo**: lista cerrada de hechos (base, rama, Claim-Id, custodia, delegaciones abiertas, `attempts`, contadores, STOP vigentes y
   siguiente acción autorizada). Va comprometida en un SHA conocido, con su SHA-256, preparada por quien no es B y antes de que B empiece. B no la ve.
3. B empieza sin el contexto de A, solo con fetch y documentos versionados.
4. B escribe sus respuestas en un artefacto propio, con su identidad (EXP-05).

**Comparación:** mecánica, hecho por hecho, contra el oráculo custodiado. Un hecho que solo estaba en la narrativa de A, o una respuesta que reproduzca el
oráculo sin fuente versionada, hace fallar la prueba. No se aceptan frases tautológicas.

**Controles negativos:**
- handoff ausente;
- SHA cambiado;
- credenciales sin acreditar;
- 0 pruebas seleccionadas;
- escritor anterior no terminado;
- trabajo sin commit;
- artefacto de otra corrida;
- contador tras cambiar de proveedor.

Resultado esperado: STOP o no elegible, con causa.

Otro Principal no se simula cambiando la etiqueta del autor. Lo que no se pueda demostrar se registra UNSUPPORTED o UNVERIFIED con causa.

## 17. Ruta para ejecutar I-62 bajo I-61 y sondas

**Ruta** (INFERENCE sobre §16):
- **Tareas con prueba legítima** (p. ej., prueba de esquema de una versión nueva): ROUTINE_IMPLEMENTATION con RED, delegables a una celda Worker elegible en la
  fecha y con aceptación A7.
- **Tareas documentales o de caracterización** cuya primera entrega exija RED: no delegables (§12). Las ejecuta la sesión responsable directamente, sin
  excepción tácita.
- **Controller Codex:** no elegible hasta revalidar el binario y disponer de `config.toml` (§§11, 13.1).
- **Equivalente del «cierre de G2»** para los contratos de I-62: decisión del Coordinator (§10.6).
- **Fricción:** cada fricción se registra. Ningún binding o adapter hipotético de I-62 se usa como autoridad.

**SP-3, inspección pasiva autorizada** (MEASURED, entre 17:03Z y 17:15Z; sin invocar modelos, sin abrir sesiones ni crear procesos participantes; sin copiar conversaciones,
identificadores de cuenta, rutas de trabajo ni valores de configuración):
- **Ubicación:** `%USERPROFILE%\.codex\sessions\AAAA\MM\DD\rollout-*.jsonl`, 62 archivos, ya citada por el README §5.
- **Primera línea (`session_meta`):** `originator` (`Codex Desktop`, `codex_work_desktop`, `codex_exec`), `source` (`vscode`, `exec` o subagente),
  `thread_source` (`user`, `chatgpt_handoff`, `subagent`) y `cli_version`.
- **`turn_context`:** cada uno trae `model` y `effort` con instante. Ejemplo: una sesión `codex_work_desktop`/`chatgpt_handoff` con último `turn_context` a las
  14:35:09Z (2026-10-01): `model` `gpt-6-astra`, `effort` `ultra`. Una sesión `codex_exec` de I-61 (04:28Z): `gpt-6-luna`/`high`.
- **Resultado:** hay una **fuente candidata, escrita por el runtime y ligada a sesión e instante**, de modelo y effort de sesiones de escritorio Codex.
- **Límites:**
  - el rol de esas sesiones no consta;
  - observado ≠ efectivo;
  - ningún `turn_context` acredita la suficiencia;
  - un registro `codex_exec` no acredita una sesión principal de escritorio;
  - el modelo observado en una sesión del Owner no lo hace elegible: el catálogo marca `gpt-6-astra` como de créditos o API, no elegible.
- **Estado:** UNVERIFIED como introspección acreditada del Principal Codex. **No habilita ninguna celda.**

**SP-1 y SP-2: no autorizadas.** Precondiciones: autenticación del CLI y configuración del sandbox, ambas acciones del Owner; son distintas entre sí.

## 18. Asuntos pendientes que siguen abiertos y su autoridad

| Asunto | Autoridad |
|---|---|
| EXP-09: M-01, M-02 creador y M-07 (UNKNOWN), con la alternativa FOUNDATION EVOLUTION condicionada | Coordinator + Architect en diseño |
| EXP-02: dueño de independencia (LIFECYCLE §5 frente a §16), de la custodia (WORKFLOW §3 frente a §16/§8), de la autoverificación y de la frontera de adapters | Coordinator; Owner si cambia el mapa de WORKFLOW §10 |
| Agrupación: si los pilotos entre proveedores forman otra unidad de entrega | Coordinator antes del diseño |
| Alcance de la custodia entre unidades | Coordinator (decisión de alcance; Owner si excede el mandato) |
| Regla de agregación de estados (§10.5) | Freeze (Coordinator + Architect) |
| Equivalente del «cierre de G2» para contratos de I-62 | Coordinator |
| Disposición de `config.toml` para delegar Codex | **Owner** |
| Autenticación de Claude CLI; configuración del sandbox de Codex; SP-1/SP-2 | **Owner** (credenciales, configuración, consumo) + Coordinator |
| Remedio general de C-F0-RED | Coordinator (SEPARATE UNIT propuesta) |

No hace falta aceptar un ADR futuro para completar el Discovery; la evolución de ADR-0046 queda para el diseño.

**S62-G0-01..04:**
- **01:** NEW ARCHITECTURE provisional mantenido; FOUNDATION EVOLUTION como alternativa (§9.3).
- **02:** capacidades en §11 con su fecha; SP-3 da una fuente candidata.
- **03:** §13.1, resultado acotado.
- **04:** observación vigente; insumo de EXP-05.

## 19. Trazabilidad con «FIRST RESPONSE REQUIRED» del mandato

| Puntos | Dónde |
|---|---|
| 1-4 | evidencia §§1-4; §0 |
| 5-6 | §§1-6; §10.3; §12 |
| 7-12 | §11; §17 (SP-3) |
| 13-15 | §§2-3; §13.2 |
| 16-22 | §§10.4-10.5, 15 (hipótesis) |
| 23-24 | §§12, 14 |
| 25 | §16 |
| 26-27 | **no producidos** (fuera de este tramo) |
| 28 | §17 |
| 29 | `IMPLEMENTATION AUTHORIZATION = NO` |

## 20. Registro de la ronda R1 (F0-R1 sobre `f25dd1d5`)

| Hallazgo | Disposición | Secciones corregidas |
|---|---|---|
| R62-F0-01 | aceptado: M-07 y M-01/M-02 creador pasan a UNKNOWN con decisor; NEW ARCHITECTURE provisional mantenido; EXP-09 continuada y abierta; corregida la agrupación (unidad de entrega ≠ gate) | §§8, 9, 10 |
| R62-F0-02 | aceptado: EXP-02, EXP-04 y EXP-08 resueltas por expansión; cinco roles mantenidos; FOUNDATIONS separadas de las autoridades de proceso | §§2, 5, 8, 10.2, 10.3, 10.6, 15 |
| R62-F0-03 | aceptado en lo sustantivo: las dos observaciones se retiran, con cláusulas exactas. **Corrección factual:** el bloque incondicional existe en PROMPT_TEMPLATES §2, l. 274-277; queda como deuda (EXP-08), no como EXP-01 | §10.1, §10.6 |
| R62-F0-04 | aceptado: tabla por host, runtime, rol, fecha, consumo, invalidadores y revalidación; EXP-05 con solicitado/configurado/observado/efectivo; oráculo custodiado; canal UNVERIFIED; estados literales | §§10.4, 11, 13.2, 15, 16 |
| R62-F0-05 | aceptado: resultado acotado; comparación `89F375C6…` frente al actual; retiro autorizado separado | §13.1 |
| R62-F0-06 | aceptado: C-F0-RED acotado con tabla; SEPARATE UNIT propuesta sin A-n | §§10.6, 12, 17 |
| R62-F0-07 | aceptado: disposición C62-F0-05; sin umbral inventado; `Skipped` justificado por completitud | §§12, 14 |
| R62-F0-08 | aceptado: superficies vivas alineadas; DC-07 actualizado por delta | §7; contrato, estado, decisiones §9 |
