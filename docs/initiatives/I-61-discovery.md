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
  nocturna activa», §«Estado actual de activación»), `ci.yml` solo tiene `push` como disparador (MEASURED) y los scripts de I-56 P-21 «siguen siendo solo diseño».
  Que `ci.yml` no tenga `schedule` **no** acredita ausencia de recurrencia externa (C61-D0-06); esa parte es **UNKNOWN** (no inspeccioné tareas programadas del
  equipo ni de las herramientas).
- **El desarrollo real es manual**: sesiones de agente relevadas por el humano sobre un mismo worktree, de forma secuencial ([WORKFLOW](../WORKFLOW.md) §3), con
  prompts redactados por el Coordinator y trasladados a mano (las órdenes D0/G0/G1 de esta iniciativa son un ejemplo directo) — **RECONSTRUCTED**.
- **Sin enrutamiento de modelo/effort**: `git grep` de `worker|effort|reasoning|model routing|execution controller|prompt profile|subdeleg` sobre `AGENTS.md`,
  `WORKFLOW.md`, `AUTOMATION_PLAN.md`, `INITIATIVE_LIFECYCLE.md`, `FOUNDATIONS.md`, `PROMPT_TEMPLATES.md`, `context-packs/`, `guias/`, `ARCHITECTURE.md`, `README.md` y
  `CLAUDE.md` no devuelve **nada** (la única mención de modelo es el trailer `Co-Authored-By`, AGENTS.md:95-96 y WORKFLOW.md:32).
- **Handoffs actuales**: cuerpo de commit, archivo de evidencia, archivo de estado, informe ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §14; [PROMPT_TEMPLATES](PROMPT_TEMPLATES.md) §B
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
| Enrutamiento modelo/effort, catálogo de modelos, esquemas de delegación/handoff, responsabilidades de un Execution Controller | **ninguno existe hoy** (no hay norma: ver DC-01) | — |

**INFERENCE:** la última fila describe mecanismos **nuevos**, no una operación existente con dueño ambiguo; ver EXP-02 (§11).

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
  los archivos, que es el que importa. **No se adopta `.agent/` por analogía** (mandato); la decisión queda para el diseño.
- **Bytes exactos:** el repositorio tiene `core.autocrlf=true`; hay precedentes de `-text` en `.gitattributes` para artefactos de evidencia JSON (p. ej. `I-52-AUTH15-run3/*.json`, **MEASURED**). Un handoff versionado como
  evidencia heredaría el problema de fin de línea que ya afectó al mandato (hallazgo fuera de alcance de G0).

## 4. DC-04 — Camino entrada → estado → persistencia → salida

**RECONSTRUCTED (observado en el desarrollo de I-60 e I-61):** orden del Coordinator (chat, trasladada a mano) → sesión Executor lee autoridades → worktree de la iniciativa → commits/push → CI sobre el SHA
exacto → evidencia/estado versionados → informe al Coordinator → **GATE PASS solo del Coordinator**. El Architect interviene antes del Freeze ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §5). El punto de fricción que motiva el mandato es el
traslado manual de la orden y del informe (mandato, «MOTIVATION» 3-4); **no medí tiempos ni costes** de ese traslado (**UNKNOWN**; métricas aún no disponibles).

## 5. DC-05 — Llamadores y consumidores

- **Lectores en código:** ninguno. `git grep` de `automation/(state|decisions|evidence)|automation_state|max_attempts|claim_id` fuera de `docs/` y de `*.md` solo devuelve líneas de `.gitattributes` (MEASURED).
- **Consumidores documentales:** `AUTOMATION_PLAN` se referencia en 40 archivos rastreados (excluido `docs/archivo`), `PROMPT_TEMPLATES` en 16 y `automation.enabled|max_attempts` aparecen en 71; 14 contratos V1 tienen
  `automation.enabled: true` (MEASURED). **Riesgo (INFERENCE):** un controller que aplique literalmente [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §5 podría tratar esos 14 contratos como elegibles; la deuda preexiste y
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
- Por tanto el **proceso** de I-61 no consume ni extiende ninguna fundación; introduce mecanismos nuevos (DC-09). `Consumes/Extends` queda `none` **para el proceso**.
- **El piloto sí podría consumir** Rack Identity / View Identity / Unknown-field Preservation / Custom Properties (la ruta de edición de Cama compone el sobre con `RackEmbedComposer.Compose`). Esa verificación DC-08 se hace
  en el Discovery **delta del piloto**, sobre su base, no aquí.

## 9. DC-09 y EXP-09 — Materialidad y arquetipo

**Pregunta EXP-09:** ¿I-61 **crea** o **modifica**, y qué M activa? (M-07 estaba UNKNOWN.) Área acotada: la infraestructura de ejecución existente (DC-01/02) y los cambios mínimos que exige el mandato.

**Resolución: CREA y además MODIFICA.** No existe autoridad alguna para delegación, handoff, enrutamiento ni catálogo (DC-01/02, MEASURED); el mandato exige crearlos (Entregables B, D, E, G). Además exige o puede exigir modificar
`PROMPT_TEMPLATES` (perfiles), `AUTOMATION_PLAN` (relación con `attempts`/paradas), `WORKFLOW` §3 y §10 y, según el transporte elegido, `.gitignore`.

| M | Estado | Evidencia / condición |
|---|---|---|
| M-01 (dueño/segunda autoridad) | **ACTIVADO (condicional, tratado como activo)** | Existen ya dueños del presupuesto de reintentos (`attempts`/`max_attempts`, AUTOMATION_PLAN §§8-9), de las paradas (§12) y del informe (§14). El mandato define `MAX_REWORK_LOOPS=3`, su propia lista de STOP y un handoff. **Sin una regla de reutilización en el Freeze aparecería una segunda autoridad** para el mismo dato. Puede resolverse a «no activado» si el Freeze declara una autoridad única (p. ej. reutilizar `attempts`); es una decisión de diseño (§13, D-04) |
| M-02 (esquema/DTO/wire) | **ACTIVADO (creador)** | Esquemas nuevos de delegación y handoff (mandato D, E). Si se extendiera `automation_state` v1 (35 archivos) pasaría a modificación de un esquema existente sin política de campos desconocidos: se recomienda artefactos separados |
| M-03 (comportamiento observable/trato de documentos existentes) | **NO activado para la infraestructura**; **UNKNOWN para el piloto** | No se reescriben contratos existentes. El piloto (Cama/`Name`) puede cambiar comportamiento de dibujo; depende de su caracterización (§10) |
| M-04 (qué falla y cómo) | **ACTIVADO (creador de semántica)** | Un protocolo con STOP, rework y handoff define semántica de fallo nueva; además hay modos de fallo silencioso documentados en los transportes (EXP-06) |
| M-05 (contrato consumido por otros) | **NO activado hoy; se activará** si se modifican las plantillas A–F o se consume el esquema fuera de I-61 | Hoy no hay consumidores del contrato nuevo (DC-05) |
| M-06 (punto de extensión reutilizable) | **ACTIVADO** | Perfiles de prompt y catálogo de modelos son puntos de extensión reutilizables por diseño (mandato C, B) |
| M-07 (mecanismo genérico transversal) | **ACTIVADO (por lectura literal)** | Política de enrutamiento + catálogo + esquemas reutilizables por «una futura iniciativa V2» (éxito 14) son un mecanismo transversal. **Matiz abierto (Q-01):** el texto de M-07 no limita el término a código; aplicarlo a artefactos de proceso no tiene precedente explícito (I-56 es anterior al lifecycle V2). Es una decisión del Coordinator con el Architect |
| M-08 (contradice ADR aceptado) | **NO activado** por el plan actual | ADR-0045 (aceptado) fija controles manuales hasta «tooling futuro» y prohíbe merge/rollback automático, Quick CI y T0-T4/R0-R4; el mandato es compatible. ADR-0033 sigue `propuesto` (no es autoridad). Reevaluar en el Freeze |

**Arquetipo propuesto: NEW ARCHITECTURE** ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §3: M-07 o M-01/M-02 creador; ante duda, el superior). El mandato etiqueta FOUNDATION EVOLUTION: esa etiqueta solo sería
coherente si el Coordinator decide que delegación/handoff/enrutamiento **modifican** la autoridad de `AUTOMATION_PLAN`/`PROMPT_TEMPLATES` en lugar de crear una nueva. La evidencia (§2) apunta a **crear**. **Decide el Coordinator**
(propone el Executor; puede elevarlo el Architect).

## 10. Piloto — Cama / `RACKEDITAR` / `Name` (caracterización estática; sin reproducción en AutoCAD)

**Cadena entrada → edición → persistencia → reapertura (MEASURED por lectura de código, `95690c28`):**

1. `RACKEDITAR` → `RackMenuCommands.RackEditar` (`RackMenuCommands.cs:153`): elige el bloque, `PickRackBlock` obtiene el sobre `RackEmbedDocument` y despacha por `KindHandlerDispatch` → `CamaKindHandler.Edit` → `RackCamaCommands.EditCama`.
2. `EditCama` (`RackCamaCommands.cs:205`): decodifica el diseño de cama, crea `RackFlowBedWindow` y llama `window.LoadExisting(config, embed.Id, embed.Name, sourceDesign)`.
3. `RackFlowBedWindow.LoadExisting` (`RackFlowBedWindow.xaml.cs:285`): `session.Identity.Adopt(id, name)` y `NameBox.Text = name ?? ""`.
4. Botón «Actualizar en AutoCAD» (`…xaml.cs:276`): `session.Identity.SetName(NameBox.Text.Trim())` y `RequestInsert(…)`; `window.RackName` = `session.Identity.Name`.
5. `BuildCamaPayload(window.FlowBedToInsert, window.RackId, window.RackName, embed, sourceDesign)` → `RackEmbedComposer.Compose(sourceEmbed, Kind, id, **name**, …)` (`RackEmbedComposer.cs:43-60`) asigna `Name = name`.
6. `FlowBedDrawService.RedrawInPlace` → `ViewBlockDraw.RedrawInPlace` → `SystemBlockWriter.RedefineInTransaction` → `RackBlockData.Write(transaction, blockId, payloadJson)` (`SystemBlockWriter.cs:136`) persiste el sobre completo en el Xrecord.
7. Reapertura: `RACKEDITAR` vuelve a leer `embed.Name`.

**Hallazgo (MEASURED, por lectura): el nombre se conserva en toda la cadena** mientras el sobre lo traiga y el usuario no lo borre. Un rack heredado **sin** nombre se reabre con `NameBox` vacío y se re-guarda sin nombre (consistente con I-60 §3). **No encontré
el punto donde `RACKEDITAR` de la cama «borraría» el nombre**; el residual de `I-60-evidence.md` §7 («puede borrar el nombre») **no documenta su procedencia ni una reproducción** (**UNKNOWN**: ¿observado por el Owner en AutoCAD o deducido?).

**Pruebas protectoras (MEASURED):** `FlowBedEditorWindowTests.LoadExisting_AdoptsDrawnGuidAndName` y `ExistingBed_Insert_ViaButton_KeepsGuidName_…` protegen la ventana (UI). **No hay prueba que cubra `EditCama`/`BuildCamaPayload` conservando el nombre**
(solo guardas de fuente de composición: `CustomPropertiesEnvelopeGuardTests`, `LegacyViewPayloadCompositionCharacterizationTests`, `I60RackNameWiringGuardTests`). El tramo Plugin depende de AutoCAD, por lo que un RED de esa capa exigiría host o refactor.

**Idoneidad como piloto (INFERENCE):** el defecto **no está demostrado** en el código actual; reproducirlo probablemente requiere AutoCAD y una corrección en el Plugin exigiría **Owner Validation** ([AGENTS.md](../../AGENTS.md) punto 5; [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11). Eso convierte el piloto en una tarea **no pequeña
ni verificable solo por CI**. El mandato prevé este caso («si … ya está corregido o no es adecuado, seleccionar otro Extension pequeño y explicar por qué»). **Pregunta exacta (Q-02):** ¿se conserva Cama/`Name` como piloto supeditado a una reproducción
observada por el Owner, o se sustituye por otro Extension pequeño verificable por CI? Autoridad decisora: Coordinator con el Owner (alcance del piloto). Este Discovery **no** elige otro candidato ni corrige nada.

Pruebas ejecutadas para este Discovery: ver [evidencia](../automation/evidence/I-61-evidence.md) §9.

## 11. Evaluación de EXP-01..09

| EXP | Resultado | Razonamiento y salida |
|---|---|---|
| **EXP-01** (contradicción entre FOUNDATIONS/ADR/Freeze/código) — **autorizada** | **Hallazgos abiertos; ninguno cerrado como B** | Ver la tabla «Cláusulas consumidas» abajo: F-01 (WORKFLOW §3 vs modelo controller+worker) es el único con posible conflicto material (clase A si afecta algo consumido); F-02 (prosa «V2 no efectivo») es condición/historia frente a hechos de Git y no cambia la clasificación T4 de I-61, pero **no la cierro como B** |
| **EXP-02** (dos dueños incompatibles o ninguno) — **autorizada** | **No activada** | Las operaciones existentes tienen un único dueño acreditado (DC-02). «Ninguno» solo aparece en mecanismos **nuevos** (routing, catálogo, esquemas), cuya autoridad la fija el Freeze/ADR (WORKFLOW §10 «Arquitectura»; «Prompts»→PROMPT_TEMPLATES): no hay ambigüedad demostrada, solo falta de rótulo. **Decisión de diseño pendiente (no EXP-02):** dónde viven las reglas normativas nuevas (Q-03) |
| EXP-03 (legado/persistencia no localizable) | No activada | La persistencia está localizada (`docs/automation/*`); no hay lectores ni fallback legacy que rastrear. Se nota la doble forma de estado (35 anidados / 1 plano) sin consumidores |
| EXP-04 (contrato con consumidores a más de un salto) | **No activada hoy; pre-registrada** | No hay consumidores del contrato de delegación/handoff; se activa al proponer los esquemas (éxito 14). Proponer como expansión del diseño, no de G1 |
| **EXP-05** (invariante sin prueba protectora ni forma conocida de probarlo) | **ACTIVADA — propuesta de expansión** | *Disparador:* DC-06. *Pregunta:* ¿qué mecanismo protege los invariantes de proceso (exact-SHA, propiedad exclusiva, sin auto-aprobación, STOP/rework determinista)? Opciones a evaluar: validación de esquema en `tests/`, guardas de fuente sobre documentos, lista de comprobación verificada por el Coordinator. *Área:* `tests/`, CI, `docs/automation/`. *Salida:* invariante → prueba u obligación verificable con RED esperado, o decisión explícita de control manual (ADR-0045 lo admite «hasta que exista tooling futuro»). Requiere autorización del Coordinator |
| **EXP-06** (semántica de fallo no determinable / fallo silencioso) | **ACTIVADA — propuesta de expansión** | *Evidencia:* la documentación oficial de Claude Code indica que, ante un fallo dentro de la ejecución como falta de autenticación, **el fallo se imprime como resultado en stdout** mientras el código de salida es distinto de cero solo «cuando la ejecución falla» (página `headless`, 2026-09-30); hoy `claude auth status` devuelve `loggedIn:false` en este equipo (MEASURED). Además no existe semántica definida para handoff ausente, parcial o con SHA obsoleto. *Pregunta:* tabla fail-closed por transporte y por estado del handoff. *Área:* jerarquía de transporte + verificación del handoff. *Salida:* semántica por caso. Requiere autorización del Coordinator |
| EXP-07 (rama activa toca la misma autoridad/contrato) | No activada | I-52 no toca ningún documento de proceso (§0). Reverificar en el Freeze |
| **EXP-08** (deuda preexistente que el cambio haría visible) | **ACTIVADA — acotada, sin expansión separada** | (a) 14 contratos `automation.enabled:true` sin ejecutor real (§5); (b) `.agent/` no ignorado mientras el mandato lo cita como ejemplo (§3); (c) `-text`/CRLF para artefactos byte-exactos (§3); (d) prosa «V2 no efectivo» (F-02); (e) `claude` no está en `PATH` y no está autenticado (§12). Se registran para el diseño; no se corrigen aquí |
| **EXP-09** — **autorizada** | **Resuelta** | Ver §9: crea y modifica; M-01/02/04/06/07 activados; NEW ARCHITECTURE propuesto; preguntas Q-01 y D-04 |

**Cláusulas consumidas evaluadas por EXP-01** (clasificación = propuesta del Executor; **no** cierra clase B ni corrige normas):

| ID | Cláusulas | Consumo por I-61 | Tipo | Impacto / pregunta |
|---|---|---|---|---|
| F-01 | [WORKFLOW](../WORKFLOW.md) §3: «Lo prohibido es tener dos sesiones **activas a la vez** sobre el mismo worktree (o la misma rama)» frente al mandato: un Controller que espera y un Worker que escribe en el mismo worktree («OWNERSHIP»: nunca dos workers escribiendo a la vez) | **Sí**, el modelo controller→worker la consume directamente | **Posible conflicto material** (la redacción no define «sesión activa» ni distingue lectura de escritura) | Clase A provisional (afecta algo consumido). *Pregunta:* ¿el Controller no-escritor cuenta como «sesión activa»? Decide la autoridad de WORKFLOW (Coordinator/Owner); resolver exige norma o enmienda, no una interpretación silenciosa |
| F-02 | `AUTOMATION_PLAN.md:20`, `AGENTS.md:111`, cabeceras de LIFECYCLE/PROMPT_TEMPLATES/FOUNDATIONS y `WORKFLOW.md` título de §11 y §11.2: «V2 **no efectivo** / V1 sigue efectivo» frente al hecho Git (`WORKFLOW_V2_EFFECTIVE_SHA=8a021fb6`, tag `integration/I-56` con `Claim pause … end=2026-09-17T22:30:00Z`) | Sí, determina el workflow de I-61 (T4) | **Historia/prosa desactualizada** frente a un hecho | WORKFLOW §10 manda que los hechos de Git vencen una afirmación factual falsa; la clasificación T4 de I-61 no cambia. Riesgo para el diseño: un prompt que copie esa prosa induciría al worker a tratar V2 como dormido. Clase **pendiente** de confirmación por Coordinator + Architect |
| F-03 | [WORKFLOW](../WORKFLOW.md) §4.3 («push de rama = respaldo, siempre») y §3 relevo («commit + push + árbol limpio») frente al mandato E («commit debe normalmente existir antes del handoff») | Sí, define qué entrega un worker | **Condición** (el mandato dice «normalmente»; no contradice) | Decisión de diseño: ¿quién hace el push y cuándo para habilitar la evidencia exact-SHA? (D-05) |
| F-04 | [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §9 (`attempts` = corrección de un fallo, tope `max_attempts`) frente a `MAX_REWORK_LOOPS=3` + «misma clase de fallo» | Sí | **Condición** (mismo dato, dos nombres) | Es M-01 (§9); no hay contradicción hasta que el Freeze defina la relación |
| F-05 | AUTOMATION_PLAN §2/§5/§15 (modo normal, límite de dos activas, registro hasta I-25) | No (I-61 es manual, `automation.enabled:false`) | **Historia/condicional** | Sin impacto; no se lee como regla para I-61 |

## 12. Transportes (instalado ≠ autenticado ≠ invocado)

**Medidas de hoy (MEASURED, sin ejecutar ningún worker); D0 se conserva como antecedente:**

| Transporte | Instalado | Autenticado | Invocado realmente | Notas |
|---|---|---|---|---|
| `codex` CLI | sí: `%LOCALAPPDATA%\OpenAI\Codex\bin\…\codex.exe` **0.159.2** (y `~\.codex\.sandbox-bin` 0.154.0-alpha); **no está en `PATH`** | «Logged in using ChatGPT» | **No** (no se ejecutó `codex exec`) | `exec` ofrece `-m`, `-c key=value`, `-s`, `-C`, `--output-schema`, `--json`, `-o`, `--ephemeral`, `--ignore-user-config` |
| `claude` CLI | sí: `%USERPROFILE%\.local\bin\claude.exe` **2.1.270**; **no está en `PATH`** | **No**: `claude auth status` → `loggedIn:false, authMethod:none` (exit 1) | **No** | `-p`, `--output-format json|stream-json`, `--json-schema`, `--model`, `--effort`, `--max-budget-usd`, `--permission-mode`, `--allowedTools`, `--bare`, `--resume`, `--no-session-persistence` |
| Mensajería entre sesiones locales (Desktop) | sí | n/a | **Sí, probada en G0** (solicitud de ventana y acuse/RELEASE con la sesión de I-52) | Es un canal entre **sesiones interactivas**, no una invocación de worker; su fiabilidad como bus de protocolo es **UNKNOWN** (el canal no confirma lectura) |
| `gh` | sí, en `PATH` | sesión iniciada | sí (lecturas de CI y ruleset en G0) | Usado para CI, no para delegar |
| Computer Use | disponible en esta sesión (herramientas del escritorio) | n/a | no usado | Fallback por mandato |

- **Delta frente a D0:** sin cambios en versiones ni autenticación. **Nuevo:** `~\.codex\models_cache.json` (obtenido hoy, 2026-09-30T22:11Z) y `~\.codex\config.toml` (solo claves `model`/`model_reasoning_effort`) — datos **locales**, no documentación oficial.
- **¿Puede Codex invocar a Claude de forma fiable? — UNKNOWN.** Claude CLI no está autenticado aquí y no se probó ninguna invocación (fuera de alcance: «no workers, sin invocaciones de pago»). Ni `codex exec` se ejecutó. **No se convierte la autenticación de Claude en prerrequisito de G0/G1**; sí será prerrequisito de cualquier opción B/C o del piloto con worker Claude.
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

**Recomendación: A — documentación + esquemas + piloto manual** como base del Freeze; **B solo si** el piloto demuestra sobrecoste de relevo atribuible al traslado manual.

- **Por qué no B/C ahora (INFERENCE sobre hechos medidos):** no hay invocación de worker demostrada (Claude CLI sin autenticar; `codex exec` no ejecutado); no hay prueba de que un Controller pueda verificar un handoff de forma independiente; los modos de fallo de los transportes aún no tienen semántica (EXP-06) y los invariantes no tienen protección (EXP-05). El mandato pide «no simular automatización» y «la opción más pequeña».
- **Qué entregaría A:** guía de prompting con fuentes registradas, política de enrutamiento por clase de tarea con catálogo actualizable separado y **no normativo**, perfiles de prompt, esquemas JSON de delegación y handoff, jerarquía de transporte y política de STOP/rework; el piloto ejecuta el protocolo con artefactos de archivo (nivel 1) y relevo manual de la invocación (nivel 5) documentando esa limitación.
- **Requisitos pendientes (antes del Freeze):** Q-01..Q-05 y D-01..D-05 (§15); decidir si EXP-05/EXP-06 se expanden; verificación oficial reciente del catálogo; definición del piloto (Q-02).
- **Riesgo de A (INFERENCE):** los criterios de éxito 7 («filesystem/CLI es el transporte primario») y 12 («un piloto real se completa») podrían cumplirse solo parcialmente si la invocación sigue siendo manual. **Pregunta (Q-04):** ¿acepta el Owner que A cumpla 7 con «filesystem + relevo manual» y 12 con el piloto manual?
- **Límites del piloto:** pequeño, sin benchmark entre proveedores, sin integración automática, con Owner Validation si cambia comportamiento de dibujo.

## 15. Lagunas, preguntas y decisiones explícitas

| ID | Pregunta | Autoridad decisora |
|---|---|---|
| Q-01 | ¿M-07 (y NEW ARCHITECTURE) aplican a mecanismos de **proceso**? (§9) | Coordinator con Architect |
| Q-02 | ¿Cama/`Name` se conserva como piloto supeditado a reproducción por el Owner, o se sustituye? (§10) | Coordinator con Owner |
| Q-03 | ¿Dónde viven las reglas normativas nuevas (extensión de `AUTOMATION_PLAN`, de `PROMPT_TEMPLATES`, documento nuevo o ADR)? Y ¿hace falta ADR? | Coordinator; Owner si hay ADR |
| Q-04 | ¿A satisface los criterios de éxito 7 y 12? (§14) | Owner |
| Q-05 | ¿Se autorizan las expansiones EXP-05 y EXP-06 y con qué área? (§11) | Coordinator |
| D-01 | F-01: definición de «sesión activa» frente a un Controller no escritor (norma o enmienda de WORKFLOW §3) | Autoridad de WORKFLOW (Coordinator/Owner) |
| D-02 | F-02: clase A/B de la prosa «V2 no efectivo» | Coordinator + Architect |
| D-03 | Ubicación del tráfico transitorio (`artifacts/…` ya ignorado vs `.agent/…` con edición de `.gitignore`) | Diseño/Coordinator |
| D-04 | Relación `attempts`/`MAX_REWORK_LOOPS` (M-01) y «misma clase de fallo» | Freeze |
| D-05 | Quién hace el push del worker y cuándo (F-03) | Freeze |
| U-01 | Recurrencia externa (tareas programadas) | UNKNOWN, no bloquea |
| U-02 | Fiabilidad de invocación Codex→Claude y de la mensajería entre sesiones | UNKNOWN, requiere prueba autorizada |
| U-03 | Métricas de coste/tiempo del relevo manual actual | UNKNOWN, no bloquea |

## 16. Trazabilidad al mandato («FIRST RESPONSE REQUIRED», 24 puntos)

Cubiertos por este Discovery: 1 (preflight, §0), 2 (main, §0), 3 (ID, evidencia G0 §1), 4 (documentos de proceso, §§1-2), 5-7 (transportes, §12), 8 (modelos/effort oficiales, §13), 18 (piloto, §10), 19 (A/B/C, §14), 20 (riesgos, §§11, 14),
parcialmente 15 (ubicación transitoria, §3/D-03) y 16 (jerarquía: decisión del mandato, sin cambios). **No** producidos a propósito (pertenecen al diseño/Proposal, no al Discovery): 9-14, 17, 21-24 (clasificación de tareas, algoritmo de enrutamiento, catálogo
propuesto, perfiles, esquemas, política STOP/rework, propuesta de Freeze, paquete del Architect, primer prompt corto) y la confirmación final de no-transferencia de autoridad, que **se conserva como restricción**: ninguna transferencia de autoridad del Coordinator, sin
autoaprobación del worker, sin protocolo exclusivo de Computer Use y sin plataforma de automatización grande antes de evidencia del piloto.
