# I-61 — Proposal V1: protocolo de ejecución delegada de agentes

```text
Frozen: NO
Unit: I-61   Workflow: V2 (T4)   Claim-Id: 0e2923de-e1a7-41bf-b7db-50ec84217850
Archetype: NEW ARCHITECTURE (Q-01)   Base: origin/main 95690c28
Author: EXECUTOR / DESIGN AUTHOR (Claude, sesión responsable de I-61)
Review: rondas Coordinator ↔ Architect en SAME-SESSION ROLE hasta AGREED (LIFECYCLE §5)
```

Diseño **autocontenido** que se congela (LIFECYCLE §6). La **fuente del alcance** es el mandato del Owner
([I-61-owner-mandate.txt](../automation/decisions/I-61-owner-mandate.txt)); A–G se citan por referencia. Hechos y decisiones de partida: [Discovery](I-61-discovery.md), incluidos
G1-C, §§17-20, y [decisiones](../automation/decisions/I-61.md) §§7-11. No repite normas: las cita.

## 1. Objetivo verificable y no-objetivos

**Objetivo.** Un protocolo estable y subordinado a las autoridades vigentes con el que:

- un Coordinator delega un gate;
- un **Controller Codex** clasifica y enruta la tarea, emite un **paquete de delegación** legible por máquina, recibe una **entrega de worker** legible por máquina y la verifica de forma
  independiente y fail-closed;
- se preservan exact-SHA, la propiedad exclusiva de escritura y la autoridad del Coordinator.

Se demuestra con **un piloto real** (§12).

**No-objetivos:**

- los del mandato («NO PRODUCT SCOPE CREEP»);
- ninguna autoridad paralela (Q-03);
- ningún cambio en reglas de evidencia, estados, enums de gate, esquema de estado v1, contratos ajenos, `AGENTS.md`, `CLAUDE.md`, `.gitignore` ni CI;
- ningún script operativo ni plataforma (nivel A, §14);
- ninguna selección automática de iniciativas ni recurrencia;
- no convertir el piloto en benchmark entre proveedores (C61-G0-06).

## 2. Autoridades y límites (Q-03; C61-G1-02, C61-G1-03)

| Dominio | Dueño (sin cambio de dueño) | Cambio que se prepara en esta rama |
|---|---|---|
| Operación del ejecutor, incluida la **ejecución delegada** | `AUTOMATION_PLAN.md` | **§16 «Ejecución delegada bajo orden del Coordinator»**, breve y por referencia. No es el ejecutor nocturno: no selecciona, no requiere activación ni `automation.enabled: true`. Aplica §3 salvo la exigencia `automation.enabled: true`, §§8-9 y §12 salvo su primera viñeta (C61-G1-02). §2 «Modo normal» añade la lectura de `docs/automation/agent-execution/README.md`. La ampliación del alcance del plan deriva de Q-03 y solo es efectiva con ADR-0046 aceptado e integración |
| Git, relevo, worktrees | `WORKFLOW.md` | §3 **sin cambios**. §10 añade la fila que falta: «Operación del ejecutor y ejecución delegada → `AUTOMATION_PLAN.md`; subordinados: `docs/automation/agent-execution/`, PROMPT_TEMPLATES §G». Cumple WORKFLOW §8 |
| Prompting | `PROMPT_TEMPLATES.md` | **§G «Delegación a un worker»** y actualización de su §2 de traza. No renombra A–F |
| Evidencia | `AGENTS.md` | **Ninguno.** Ni la entrega de worker ni la verificación del Controller son evidencia de gate |
| Ciclo de diseño | `INITIATIVE_LIFECYCLE.md` | Ninguno |
| Decisión de fondo | ADR | **ADR-0046 `propuesto`** (C61-G1-04), creado **con esta Proposal**, antes de implementar su decisión. Ni el piloto ni este Freeze se apoyan en él como autoridad (WORKFLOW §10 solo reconoce ADR aceptados): la autoridad para aplicar el protocolo en el piloto es del **Owner** (mandato «PILOT»; decisiones §9.1, cláusulas 2-4 y 8) |

**Subordinados nuevos** en `docs/automation/agent-execution/`:

- `README.md`: protocolo, roles, relevo, propiedad, fallos, STOP y rework, transporte, almacenamiento y recetas de invocación;
- `routing.md`: enrutamiento **estable**;
- `model-catalog.md`: catálogo **mutable y no normativo**;
- `prompting-guide.md`: guía del entregable A;
- `schemas/`: `delegation`, `worker-handoff` y `controller-verification` (`.schema.json`).

**Cláusula de subordinación**, obligatoria en cada subordinado: no crean requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del
dominio y el conflicto se eleva como STOP.

`documentation-governance.md` recibe una línea que refleja esta división. La vigencia llega con la integración. Dentro de I-61 se aplica solo en su piloto y **sin rebajar** ninguna regla
vigente. Ninguna otra iniciativa lo adopta por estar escrito.

## 3. Roles, declaraciones y relevo (D-01, D-05, C61-G1-01)

| Participante | Puede declarar | Nunca declara |
|---|---|---|
| Worker (Claude o Codex) | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS, Candidato, cierre, integración |
| Controller (Codex) | `EXECUTION_VERIFIED`, `EXECUTION_REWORK_REQUIRED`, `EXECUTION_BLOCKED` | GATE PASS, Candidato, cierre, integración |
| Coordinator | GATE PASS, tras revisar la verificación y la evidencia | Candidato, cierre o integración fuera del workflow |

**Sesión responsable** es la que tiene asignado el worktree de la unidad. **Relevo con un participante externo** (Codex), en aplicación de WORKFLOW §3 —sin modificarlo— y de la cláusula 5
del Owner (C61-G1-01):

1. **Relevo de salida** (WORKFLOW §3):
   - commit y push, con un resumen de estado en el cuerpo del último commit;
   - árbol limpio y ninguna operación Git en curso;
   - `HEAD` = `origin/<rama>`, comprobado con `git ls-remote`;
   - `git fetch` y registro de `origin/main`;
   - ningún otro participante vivo: se busca en la lista de procesos un `codex.exe exec` o `claude` sobre el worktree, y ante una duda se hace STOP (U-01);
   - una sola delegación abierta para la unidad;
   - SHA-256 de `~/.codex/config.toml` registrado.
2. **Cesión del worktree.** Durante la invocación síncrona la sesión responsable **no opera**: no lee, no escribe ni ejecuta nada sobre el worktree, **incluidos sus propios subagentes**.
   Registra la hora de inicio y fin y la lista de procesos antes y después. No se reinterpreta «sesión activa» (D-01): una sesión que completó el relevo de salida y no opera ha **cedido** el
   worktree. La etiqueta «esperando» no basta sin el relevo completo.
3. **Relevo de entrada** (WORKFLOW §3):
   - el participante empieza verificando rama, upstream y divergencia, `git status` y `git log`;
   - al volver, la sesión responsable repite esas verificaciones, confirma que el participante terminó, compara el hash de `config.toml` y declara cualquier cambio como desviación.

Tipos de participante:

- **Controller:** proceso Codex **externo**, de un solo turno y en sandbox `read-only`; planifica o verifica.
- **Worker:** escribe solo en su alcance, valida, hace commit y push de la rama autorizada, escribe su entrega y **termina** (D-05). El Controller verifica **después**.
  - Si el Worker es un **subagente** de la sesión responsable, la operación es **alternancia interna** de una sola sesión responsable (cláusula 5): la sesión no opera mientras el subagente
    escribe y lo verifica después.
  - Si es un **proceso externo** (Codex), aplica el relevo anterior.

**Responsabilidades G.1..G.12:** se asignan como en el Discovery §20.4.

- **G.7 «invoke worker»** y **G.11 «route corrections»:** la **sesión responsable** los ejecuta como relevo, **exactamente** según el paquete emitido por el Controller. Una divergencia es
  una desviación y, si es material, STOP. Limitación declarada: Codex no puede invocar en este entorno.
- **Alternancia de roles:** la sesión Claude de desarrollo puede actuar como Coordinator, Architect y Executor (`SAME-SESSION ROLE`), pero no se presenta como Codex.

## 4. Enrutamiento estable (entregable B, `routing.md`)

Primero la **clase de tarea**, luego la **elegibilidad** (capacidades requeridas frente a disponibilidad real) y al final el **nivel más bajo adecuado**. `routing.md` **no contiene
identificadores de modelo** (INV-06).

| Clase (mandato) | Perfil | Effort semántico de partida | Capacidades mínimas |
|---|---|---|---|
| Implementación mecánica | ROUTINE_IMPLEMENTATION | Routine | escribir, commit/push, pruebas |
| Propagación repetitiva / cableado | ROUTINE_IMPLEMENTATION | Routine | ídem |
| Implementación de pruebas | ROUTINE_IMPLEMENTATION | Balanced | ídem |
| Documentación | DOCUMENTATION | Routine | escribir, commit/push |
| Caracterización | CHARACTERIZATION | Balanced | lectura, pruebas locales |
| Depuración | DEBUGGING | Balanced | lectura, pruebas; escribir si corrige |
| Causa raíz incierta | DEBUGGING | Deep | lectura, pruebas |
| Implementación transversal a capas | ROUTINE_IMPLEMENTATION (corta) / LONG_HORIZON_IMPLEMENTATION (larga) | Deep | escribir, commit/push, pruebas |
| Revisión de arquitectura | ARCHITECTURE_REVIEW | Deep | lectura |
| Conformidad adversarial | ARCHITECTURE_REVIEW | Deep | lectura |
| Implementación agéntica larga | LONG_HORIZON_IMPLEMENTATION | Long-horizon | escribir, commit/push, pruebas |
| (Controller) planificación / verificación | CONTROLLER_PLANNING / CONTROLLER_VERIFICATION | Balanced | solo lectura |

**Dimensiones.** Se registran siete dimensiones en el paquete, cada una Baja, Media o Alta: ambigüedad, sensibilidad arquitectónica, amplitud, uso de herramientas, coste de fallo,
repetición mecánica y horizonte.

- Ambigüedad, sensibilidad arquitectónica o coste de fallo **Altos** suben el effort un escalón; el tope es Deep, salvo con horizonte largo.
- Repetición **Alta** lo baja un escalón.
- Horizonte **largo** fija Long-horizon.
- Uso intensivo de herramientas exige un executor agéntico con esas herramientas.

**Effort semántico:** `Routine`, `Balanced`, `Deep`, `Long-horizon` y `Maximum`. El catálogo lo traduce a los controles de cada proveedor.

**Nivel de capacidad:** Eficiente, Equilibrado o Frontera.

- `Routine` → Eficiente.
- `Balanced` → Equilibrado, o Eficiente con más effort.
- `Deep` → Equilibrado o Frontera.
- `Long-horizon` y `Maximum` → Frontera.

**Algoritmo:**

1. Clasificar la tarea y puntuar las dimensiones.
2. Filtrar por **elegibilidad**: capacidades × estado local del catálogo (publicado, instalado, autenticado, invocación probada). Si no queda ningún candidato elegible → `BLOCKED`, sin subir de
   nivel en silencio.
3. Elegir el nivel más bajo adecuado entre los elegibles y registrar `RoutingReason`.
4. **Escalar** solo con `MODEL_ESCALATION_REASON` respaldado por evidencia, en este orden: effort → nivel → long-horizon → maximum. No es obligatorio recorrer todos los escalones.
5. **Enrutar hacia abajo** está permitido, con razón registrada.
6. Escalar, bajar o cambiar de modelo, rol o sesión **no reinicia** `attempts` (D-04).

**Frescura.** Una entrada del catálogo verificada hace más de 90 días está `STALE` y no se usa para enrutar hasta reverificarla con la documentación oficial.

## 5. Catálogo mutable (`model-catalog.md`)

**NO NORMATIVO** respecto de los nombres de modelo. Cada entrada registra:

- proveedor, modelo y controles de effort;
- fortalezas y debilidades **según la fuente oficial**;
- perfiles recomendados;
- fecha de verificación y URL de la fuente;
- **estado local** en cuatro columnas: publicado, instalado, autenticado e invocación probada.

Contenido inicial, verificado el 2026-09-30 (Discovery §§13, 17), por proveedor:

- **Anthropic:** Fable 5.1, Opus 5.5, Sonnet 5.5 y Haiku 4.5. En la sesión se invocan como subagentes (alias `fable`, `opus`, `sonnet`, `haiku`); el modelo efectivo se confirma en la
  transcripción. El CLI no está autenticado.
- **OpenAI/Codex:** `gpt-6-astra`, `gpt-6.1-sol` y `gpt-6-luna`, más `gpt-5.6-*` y `gpt-5.5`. En el CLI hay invocación probada solo en `read-only`. Advertencia: `gpt-6-astra` se ofrece «con
  ChatGPT Credits y acceso API» según la fuente oficial. Queda **fuera** de la ruta por defecto porque podría consumir créditos, y no está autorizado gastar más allá de la cuota.

Traducción de effort:

- Anthropic: `low`/`medium`/`high`/`xhigh`/`max`; Haiku 4.5 no tiene effort.
- Codex: `low`/`medium`/`high`/`xhigh`/`max`, y `ultra` donde exista.

El catálogo se actualiza sin tocar Workflow ni routing.

## 6. Guía de prompting (entregable A, `prompting-guide.md`)

Guía breve, basada en la documentación oficial vigente, con URL, proveedor, fecha y generación cubierta de cada fuente (Discovery §13). Reglas:

- encuadre de la tarea: objetivo, contexto, restricciones y criterio de terminado;
- rol y criterios de éxito explícitos;
- estructura con etiquetas cuando el prompt mezcla instrucciones y datos;
- instrucciones de herramienta como acción, no como sugerencia;
- evitar sobreingeniería y reinvestigación: se referencian el Discovery y la entrega previa;
- en horizonte largo, estado en archivos y progreso incremental;
- STOP explícito;
- **autolocalización**: se citan autoridades por ruta y sección, sin copiarlas;
- la **vigencia del workflow se lee de Git**, no de prosa desactualizada (F-02);
- ningún secreto.

## 7. Composición de prompts (entregable C, PROMPT_TEMPLATES §G)

`prompt = contrato base de delegación + perfil + delta de la tarea`.

- **Contrato base.** Fija rol; identidad (unidad, gate, tarea, intento, `RunId`, base, rama, worktree); autoridades por referencia; alcance permitido y prohibido; invariantes; criterios;
  pruebas requeridas con selección > 0; evidencia esperada; STOP; ruta de la entrega; prohibición de autoaprobación; trailer `Co-Authored-By` de quien ejecuta; la regla de terminar tras la
  entrega. Cubre los 8 campos de LIFECYCLE §10.
- **Perfiles** (8, solo instrucciones de método): `ROUTINE_IMPLEMENTATION`, `DEBUGGING`, `LONG_HORIZON_IMPLEMENTATION`, `ARCHITECTURE_REVIEW`, `CHARACTERIZATION`, `DOCUMENTATION`,
  `CONTROLLER_PLANNING` y `CONTROLLER_VERIFICATION`. Cada uno tiene un tope de 25 líneas y no reproduce cláusulas normativas (INV-07).
- **Delta.** Son los campos del paquete.

La composición es determinista: el prompt compuesto y su SHA-256 se guardan junto al paquete.

## 8. Esquemas (entregables D, E y verificación)

JSON Schema 2020-12 **estrictos**: `additionalProperties: false`, todas las propiedades en `required` y `null` explícito para «no aplica». Son compatibles con `--output-schema` (INV-09) y el
relevo los valida mecánicamente con `Test-Json` de PowerShell 7 (MEASURED en G1-C: rechaza un patrón inválido y propiedades extra).

- **`rackcad-delegation/v1`** (`delegation.schema.json`). Campos:
  - identidad y base: `Schema`, `TaskId`, `RunId`, `Initiative`, `Unit`, `Gate`, `Attempt`, `AuthorityRevision`, `MainSha`, `BaseSha`, `ExpectedBranch`, `ExpectedWorktree`;
  - enrutamiento: `TaskClass`, `Dimensions`, `Executor` (proveedor, transporte, rol), `Model`, `Effort` (semántico y del proveedor), `PromptProfile`, `RoutingReason`, `ModelEscalationReason`;
  - encargo: `Authorities[]`, `Objective`, `AllowedWriteScope[]`, `ForbiddenWriteScope[]`, `Invariants[]`, `AcceptanceCriteria[]`, `RequiredTests[]` (filtro y mínimo seleccionado),
    `ExpectedEvidence[]`, `StopConditions[]`;
  - control: `MaxReworkLoops`, `ExpectedHandoffPath`, `IssuedBy`.
- **`rackcad-worker-handoff/v1`** (`worker-handoff.schema.json`). Se llama **entrega de worker**. Campos:
  - identidad y resultado Git: `Schema`, `TaskId`, `RunId`, `Initiative`, `Gate`, `Attempt`, `BaseSha`, `CurrentSha`, `Branch`, `Worktree`, `Pushed`, `FilesChanged[]`;
  - trabajo y pruebas: `WorkCompleted`, `TestsExecuted[]` (comando, fase `RED`/`GREEN`/`RELEVANT`, seleccionadas, superadas, fallidas, omitidas, archivo de resultados), `TestResults`,
    `Evidence[]`;
  - hallazgos: `UnexpectedFindings[]`, `KnownLimitations[]`, `Deviations[]`, `OpenQuestions[]`;
  - cierre: `WorkerStatus`, `RecommendedNextAction`, `Worker` (proveedor, modelo solicitado, effort, trailer usado).
- **`rackcad-controller-verification/v1`** (`controller-verification.schema.json`). Campos: `Schema`, `TaskId`, `RunId`, `Attempt`, `VerifiedSha`, `Checks[]` (id, resultado
  `pass`/`fail`/`not_run`, evidencia), `Classification`, `FailureClass`, `Findings[]`, `RecommendedNextAction`.

Ningún esquema admite un valor de gate. El texto libre también se comprueba contra términos de gate (INV-01).

## 9. Identidad, propiedad, rework y STOP (C61-G1-05)

- **Identidad.** La delegación fija `MainSha` (`origin/main` en la salida del relevo), `BaseSha` (= `origin/<rama>`), la rama y el worktree. La entrega declara `BaseSha`, `CurrentSha` y
  `Pushed`. Las señales se verifican con el actor medido que corresponda (Discovery §18.4).
- **Propiedad exclusiva.** Cada paquete fija `OWNER`, `BRANCH`, `WORKTREE`, `BASE_SHA` y `ALLOWED_WRITE_SCOPE`. Nunca hay dos escritores. Si no se sabe si un participante sigue vivo, no se
  lanza otro.
- **Regla de conteo** (resuelve C61-G1-05 y F-04; se congela):
  - **Presupuesto único.** `attempts` del estado canónico ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §§8-9), por unidad, sube en 1 **cada vez que se lanza una corrección** tras
    `EXECUTION_REWORK_REQUIRED` (D-04). Una delegación nueva sin fallo previo no lo incrementa, y un `BLOCKED` tampoco, hasta que se intenta una corrección.
  - **Clase de fallo.** Cada verificación fallida declara `FailureClass` y los `Checks[].id` fallidos.
  - **Defecto nuevo no relacionado.** Si la clase o las comprobaciones fallidas son distintas de las del intento anterior, se **registra un análisis** antes de lanzar la corrección:
    causa, por qué no es la misma clase y nuevo enrutamiento si procede. Es la condición del mandato «not merely another retry without analysis». La corrección **sí** consume `attempts`,
    porque el presupuesto es único.
  - **`MaxReworkLoops`.** Vale 3 por defecto (mandato), cuenta las correcciones de **una misma clase** dentro de la cadena y está acotado por los intentos restantes:
    `min(3, max_attempts − attempts)`.
  - **STOP.** Cuando una misma clase agota `MaxReworkLoops`, o `attempts` alcanza `max_attempts` (lo que ocurra primero), se aplica STOP, se analiza la causa raíz y se devuelve al
    Coordinator (mandato «On repeated failure: STOP → root-cause investigation → Coordinator»).
  - **Sin reinicios.** Cambiar de modelo, rol o sesión, o renombrar el error, no reinicia ningún contador.
- **STOP.** La lista del mandato y AUTOMATION_PLAN §12 aplican **por referencia**, más los modos de §10 clasificados como STOP. No hay continuación especulativa: el bloqueo se entrega
  estructurado al Coordinator. Que un cambio requiera Owner Validation **no** es un fallo: el gate cierra con la OV asignada al Candidato.

## 10. Semántica de fallos y recuperación

Se congela la tabla de modos y señales del [Discovery](I-61-discovery.md) §18.4, con su corrección de «base obsoleta» (cualquier avance de `origin/main` → STOP y rebase antes de
escribir), su tabla de actores y su limitación de independencia.

- **Regla multiseñal.** `EXECUTION_VERIFIED` exige todas las señales. Ante señales mixtas: **STOP > BLOCKED > REWORK REQUIRED**.
- **Recuperación:**
  - `REWORK` → nuevo intento según §9;
  - `BLOCKED` → el Coordinator resuelve la precondición;
  - `STOP` → Coordinator.
- **Nunca bastan** stdout, un código de salida o un JSON conforme. Se ignora cualquier `-o`, entrega o verificación con fecha anterior a la invocación.

## 11. Transporte y almacenamiento (entregable F; D-03)

**Jerarquía** del mandato, sin cambios. Disponibilidad medida (Discovery §17):

- Controller Codex por CLI `read-only` (nivel 2).
- Worker Claude como **subagente invocado por la sesión responsable** con modelo y effort del paquete (nivel 2).
- Sin worker Codex con escritura.
- Sin Claude CLI.

**Receta de invocación de Codex** (operativa; vive en `README.md` del protocolo):

- binario por ruta verificada;
- `-C <worktree de la unidad>` **únicamente**, porque un directorio nuevo altera `config.toml` (DEV-G1C-01);
- `-s read-only`, `--output-schema`, `-o` y `--json`, **sin** `--ephemeral`, para leer el modelo efectivo en el registro de sesión;
- stdin cerrado;
- `pwsh` del runtime de Codex antepuesto al PATH **solo del proceso hijo**;
- tope de tiempo;
- hash de `config.toml` antes y después.

**Almacenamiento transitorio:** `artifacts/orchestration/<unit>/<task>/<attempt>/`, en el worktree de la unidad. Contiene `gate-contract.md`, `availability.json`, `delegation.json`,
`prompt.md`, `worker-handoff.json`, `remote-facts.json` (remoto y CI, escrito por la sesión responsable), `controller-verification.json`, eventos y registros. Está ignorado por Git y el
canal de retorno son esos archivos más el informe de la sesión responsable.

**Custodia duradera:** antes de cerrar un gate, el archivo de evidencia registra los campos clave y el SHA-256 de cada artefacto transitorio que respalde la decisión. `git worktree remove`
borraría el tráfico ignorado, por eso se copia antes (CR-17).

## 12. Piloto (C61-G1-06, C61-G1-07)

**Tarea real:** corregir **D-1a** (Discovery §19).

- **Función pura.** Se añade a `src/RackCad.Application/Persistence/` una función pura de resolución del nombre editado, con la **misma semántica** que la expresión de las otras cinco rutas
  (`IsNullOrWhiteSpace(editado) ? nombreDelSobre : editado`).
- **Uso.** `EditCama` la usa para el payload y para `SyncName`. No se tocan las otras cinco rutas.
- **Materialidad** (Discovery §19.1): M-01..M-08 no activados; EXTENSION.
- **Efecto:** evita D-2 (un `null` del sobre se conserva) y D-3. D-4 queda fuera.
- **Intención de producto:** respondida por el Owner (Discovery §15, Q-06).

**Flujo (G3):**

1. El Coordinator redacta `gate-contract.md`.
2. Relevo de salida.
3. **Controller Codex de planificación** (`CONTROLLER_PLANNING`): recibe el contrato, `availability.json` y las autoridades, y emite `delegation.json`, que el relevo valida con el esquema.
4. Relevo: el prompt compuesto se guarda con su hash.
5. **Worker enrutado** (subagente según el paquete), en este orden:
   - escribe las pruebas conductuales de la función (OBL-P1) y la guarda de cableado (OBL-P2);
   - registra el **RED**: OBL-P2 falla con una aserción real, y OBL-P1 falla porque la función aún no existe;
   - implementa la función y su uso en `EditCama`;
   - obtiene el **GREEN** con selección > 0 y ejecuta las pruebas relevantes;
   - hace commit con su propio trailer y push;
   - escribe `worker-handoff.json` y termina.
6. Relevo de entrada. `remote-facts.json` recoge `ls-remote` y la corrida de CI de `push` de `CurrentSha` (jobs y nombres/conteos de pruebas). Esta es la señal de pruebas **independiente** del
   Worker (Discovery §20.4).
7. **Controller Codex de verificación** (`CONTROLLER_VERIFICATION`) → `controller-verification.json`.
8. **Controles negativos** (INV-02/03/08), sobre copias y sin modificar la rama:
   - una entrega con un `CurrentSha` que no existe;
   - una delegación con `AllowedWriteScope` que excluye el archivo de prueba.

   Ambas deben dar una clasificación ≠ `EXECUTION_VERIFIED`.
9. Decisión del Coordinator.
10. Commit de cierre con la evidencia duradera del piloto.
11. Core Full sobre el SHA de cierre y su CI.

**Métricas** (F-06): TaskProfile, Executor, Model (solicitado y efectivo), Effort, PromptProfile, ReworkLoops, ExecutionResult y CoordinatorResult en la evidencia del piloto. Tiempos de proceso
y tokens figuran como **datos del piloto**, no como métrica de LIFECYCLE §11 (cuya fila queda `UNKNOWN` salvo Git/Actions/Owner).

## 13. Obligaciones invariante → prueba (LIFECYCLE §6)

| ID | Obligación | Prueba / control | RED esperado |
|---|---|---|---|
| OBL-01 | INV-01: no autoaprobación | `AgentExecutionProtocolTests` (Core): estados exactos de worker y controller; ningún término de gate en los esquemas; detecta una copia mutada en memoria; comprobación de texto libre sobre fixtures | falla sin esquemas o con un término de gate |
| OBL-02 | INV-09: esquemas estrictos | ídem: `additionalProperties:false`, `required` completo; detecta una copia mutada | falla sin esquemas |
| OBL-03 | INV-02: identidad | ídem: patrón de 40 hex en `MainSha`/`BaseSha`/`CurrentSha`/`VerifiedSha`; **control negativo real** del piloto | falla sin esquemas; el control negativo debe dar ≠ VERIFIED |
| OBL-04 | INV-05: tráfico transitorio | ídem: `.gitignore` contiene `artifacts/`; ningún documento del protocolo usa `.agent/`; las rutas del protocolo empiezan por `artifacts/orchestration/` | falla sin documentos |
| OBL-05 | INV-06: separación | ídem: `routing.md` sin identificadores de modelo; `model-catalog.md` con declaración NO NORMATIVO, fecha y URL por entrada; detecta un identificador inyectado | falla sin documentos |
| OBL-06 | INV-07: composición | ídem: PROMPT_TEMPLATES §G con los 8 perfiles, el contrato base con los 8 campos de LIFECYCLE §10, perfiles ≤ 25 líneas y ninguna frase testigo normativa | falla sin §G |
| OBL-07 | INV-03/INV-08: fail-closed | controles negativos del piloto (alcance estrecho, SHA falso) | clasificación ≠ VERIFIED |
| OBL-08 | INV-04: conteo | control manual del Coordinator: `Attempt` y `attempts` coherentes en el estado y en la evidencia | — (declarado manual) |
| OBL-P1 | Piloto: regla conductual de resolución del nombre editado | Pruebas Core de la función pura: blanco y espacios → nombre del sobre; `null` del sobre se conserva; nombre propio → el editado | RED: la función no existe |
| OBL-P2 | Piloto: cableado de `EditCama` | Guarda Core: `EditCama` usa el nombre resuelto por la función para el payload y para `SyncName`; la paridad semántica con las otras cinco rutas se documenta en la prueba | **RED** con aserción real antes del cambio (regresión «verificada fallando», AGENTS punto 2) |
| OBL-P3 | Piloto: contrato de entrada de la ventana | Prueba STA: con el campo vacío, `window.RackName` es `""` (la resolución vive en `EditCama`, no en la ventana) | GREEN (caracterización) |
| OBL-P4 | Piloto: comportamiento de extremo a extremo | **Owner Validation** en AutoCAD (§16) | no automatizable: el proyecto de pruebas no referencia el Plugin |

**Conformidad con AGENTS.** El invariante conductual (la regla) tiene prueba conductual (OBL-P1). La guarda (OBL-P2) protege solo una frontera estática, el cableado del Plugin, al que las
pruebas no llegan. La OV cubre el extremo a extremo y no sustituye la evidencia automatizada. OBL-01..06 protegen fronteras estáticas de artefactos que son el propio producto del protocolo.

## 14. Nivel de solución: **A**

**A = documentación + esquemas + piloto real con relevo.** Motivos: el Controller Codex funciona por CLI en solo lectura; el Worker se invoca directamente; la validación de esquemas usa una
herramienta existente (`Test-Json`), y los fallos medidos del transporte (stdin, `pwsh`, JSON conforme pero vacío, efecto sobre `config.toml`) se neutralizan con la receta y las
comprobaciones del relevo. Con un solo piloto no hay evidencia de que mantener scripts compense.

**Disparadores de B**, que se evalúan con la evidencia del piloto:

- errores de relevo atribuibles a pasos manuales;
- verificaciones idénticas repetidas;
- tiempo de relevo dominante.

**C** no está justificado.

## 15. ADR-0046 (propuesto; C61-G1-04)

Se crea **con esta Proposal**, antes de implementar, en estado `propuesto`. Su fila en el índice de ADR se escribe dentro de una ventana acordada con I-52 por su canal.

- **No bloquea** el Freeze, G2 ni G3: el README de ADR y WORKFLOW §8 exigen crearlo, no aceptarlo, y la autoridad del piloto es del Owner (§2).
- **Bloquea `READY-03`** («sin decisión material/Owner pendiente»), y por tanto `FINAL_CANDIDATE_SHA`. Registrar la aceptación modifica el ADR y crea un SHA nuevo, así que la aceptación del
  Owner debe **preceder** al Candidato.

## 16. Plan de gates (se congela), READY y Owner Validation

| Gate | Resultado verificable | Evidencia de cierre |
|---|---|---|
| **G2 — Protocolo materializado** | Existen los subordinados, los tres esquemas, PROMPT_TEMPLATES §G, AUTOMATION_PLAN §16/§2, la fila de WORKFLOW §10 y la línea del pack; `AgentExecutionProtocolTests` pasa de RED a GREEN (OBL-01..06) | RED→GREEN con selección > 0; Core Full local sobre el SHA de cierre; CI exacta de `push`; revisión del Coordinator contra el Freeze |
| **G3 — Piloto real** | Flujo de §12 completo, con Controller Codex real, Worker enrutado y modelo efectivo confirmado (U-04), OBL-P1/P2 RED→GREEN, OBL-P3, controles negativos con clasificación ≠ VERIFIED, métricas y decisión del Coordinator | Evidencia duradera del piloto; Core Full local sobre el SHA de cierre; CI exacta; build Debug del Plugin sobre el SHA de cierre |
| **READY-01..09** | En orden. `READY-03` bloqueado hasta la aceptación de ADR-0046 por el Owner | — |
| **Candidato** | Tras READY-09: Core Full y UI Full locales, builds Debug de UI y Plugin, CI exacta, cobertura por `workflow_dispatch` con el SHA medido, y paquete de OV | AGENTS.md; guía manual §7.1 |

**Asignación OV** (aditiva a la guía manual; se ejecuta sobre `FINAL_CANDIDATE_SHA`):

- **OV-I61-01:** `QUICKCAMA` → «Cama N»; `RACKEDITAR` → vaciar el nombre → Actualizar → `RACKLISTA` conserva «Cama N»; guardar, cerrar y reabrir → `RACKEDITAR` muestra «Cama N». Comprueba
  el comportamiento que fija el mandato (Q-06).
- **OV-I61-02:** renombrar a un nombre propio → se conserva tras guardar y reabrir.
- **OV-I61-03:** si existe un DWG con una cama heredada sin nombre → sigue «(sin nombre)».
- **OV-I61-04:** checklist general aplicable de la guía §5.
- **OV-I61-05:** revisión por el Owner de los artefactos del protocolo y de la evidencia del piloto: aceptación del resultado frente a los criterios de éxito (Q-04).

## 17. Materialidad y riesgos

**Materialidad:** M-01, M-02 (creador), M-04, M-05, M-06 y M-07 → **NEW ARCHITECTURE**. El piloto es EXTENSION.

**Riesgos y mitigaciones:**

| Riesgo | Mitigación |
|---|---|
| Relevo mal ejecutado o actividad solapada (DEV-G1C-02) | Relevo con registro de tiempos; suspensión explícita de la sesión |
| Efectos laterales de Codex en la configuración (DEV-G1C-01) | Solo el worktree de la unidad; hash antes y después |
| Catálogo obsoleto | Regla de 90 días |
| Oráculos de guarda | Limitación declarada en §13 |
| Duplicación normativa | Cláusula de subordinación; OBL-06 |
| Contaminación de Git | Ruta ignorada |
| Métricas como burocracia | Las no disponibles no bloquean |
| Independencia parcial de la verificación | Controles negativos; Controller de otro proveedor; señal de pruebas por CI del SHA exacto |
