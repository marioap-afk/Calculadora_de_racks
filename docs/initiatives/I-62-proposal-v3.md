# I-62 — Proposal V3: portabilidad del Principal y binding de roles independiente del proveedor

```text
Frozen: NO
Version: V3 (sustituye a V2 en la revisión; V1 y V2 se conservan sin cambios como versiones revisadas)
Unit: I-62   Workflow: V2 (T4)   Claim-Id: 5b661a17-8c18-4183-8554-3866059cba2b
Archetype: NEW ARCHITECTURE (Coordinator, C62-F0-09; el mandato conserva la etiqueta FOUNDATION EVOLUTION)
Base: origin/main 819955d6   Discovery: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65) y §21, base de diseño (C62-F0-08)
Reviews: Coordinator SEPARATE SESSION — V1 CHANGES REQUIRED (I-62-coordinator-review-v1.md); V2 CHANGES REQUIRED (I-62-coordinator-review-v2.md)
Author: sesión principal responsable de I-62 (Claude), redacción directa (C62-F0-20)
Review: PENDIENTE — Coordinator y Architect (paquete: docs/initiatives/I-62-architect-package-v3.md; revisión del Architect no realizada)
Owner: OD-6 pendiente (ambas alternativas delimitadas en §11.4); las demás OD bloquean en su frontera real (§18)
IMPLEMENTATION AUTHORIZATION = NO   ·   I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED
```

**Fuentes:**
- Alcance: el mandato ([I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt)).
- Hechos: el [Discovery](I-62-discovery.md) R1 y su §21.
- Decisiones: [decisiones](../automation/decisions/I-62.md) §§7-12 (C62-G0-02, C62-F0-01..20).
- Revisiones: [registro de V1](I-62-coordinator-review-v1.md) y [registro de V2](I-62-coordinator-review-v2.md).

**Precedencia:** las autoridades integradas mandan mientras I-62 no se integre.

**Diseño, no conducta:** no modifica el protocolo operativo, no publica esquemas ni cambia conducta. REFERENCE OVER REPETITION.

**Mapa de la corrección V2→V3** (los cierres de V1 se preservan: R62-V1-09 identidad por recibo + commit/ruta/blob; O62-V1-01 capacidades por rol y
acción):

| Hallazgo | Capítulos de V3 |
|---|---|
| R62-V2-01 | §14, Anexo E |
| R62-V2-02 | §§5-6, 8-9, Anexo F |
| R62-V2-03 | §§7, 11.1, Anexo B |
| R62-V2-04 | §9.3, §13, Anexo G |
| R62-V2-05 | §15, Anexo D.1-D.2 |
| R62-V2-06 | §12, Anexo D.3-D.7 |
| R62-V2-07 | §§16-17, Anexo C |

## 0. Delta respecto de I-61 (resumen)

| Tema | I-61 integrado | I-62 propone |
|---|---|---|
| Roles | Controller (Codex), Worker (Claude o Codex), sesión responsable | cinco roles sin proveedor (D-01) |
| Proveedor del Controller | fijado (ADR-0046) | binding por capacidad acreditada (D-05); ADR sucesor parcial (Anexo A) |
| Sesión principal | sin perfil ni obligación | perfil, autoverificación y `CONFIGURATION_STATUS` (D-03, D-04) |
| Preflight | solo en el relevo, con `config.toml` | observación de capacidad por runtime + comprobaciones de relevo sin cambio (D-06) |
| Hechos de proveedor | en §16.4, README, `relay-record` | frontera de adapters con descriptor y esquema de hechos (D-07) |
| Independencia | parcial, aceptada | cuatro dimensiones sobre la identidad observable del actor (D-12) |
| Custodia | deducida del relevo | estado durable por unidad en puntos quiescentes + diario transitorio encadenado, sin escrituras Git durante la cesión (D-08) |
| «Cierre de G2» | nombre de un gate de I-61 | MaterializationClose por unidad (D-15) |
| Versiones | cinco `/v1` | protocolo por unidad (I61/I62) con mapa de compatibilidad por cláusula (D-18, Anexo E) |
| Nivel | A | A, decidido antes del Freeze (D-16) |

## 1. Objetivo, no-objetivos y criterios de éxito

**Objetivo:** vincular cada rol a un proveedor, modelo o runtime según **capacidad acreditada**, y que otro Principal pueda reanudar la unidad sin memoria
privada (mandato, «GOAL»).

**No-objetivos:**
- producto;
- reemplazar al Master o quitar autoridad al Coordinator;
- bucles sin límite, compras, gestión de credenciales o scheduler;
- modelos fijos;
- auto-merge;
- reescribir I-61;
- motor, framework, scheduler, servicio de locks o plataforma;
- registro global entre unidades;
- migrar unidades activas;
- remedio general de C-F0-RED.

| # | Criterio (mandato) | Mecanismo | Gate | Obligaciones (Anexo C) |
|---|---|---|---|---|
| 1 | Principal independiente del proveedor | D-01, D-03 | F1 | C-01, C-02 |
| 2 | Roles reasignables por hechos observables | D-05, D-06 | F3 | C-11, C-12, C-24 |
| 3 | El Principal verifica su sesión | D-03, D-04, D-11 | F1 | C-03, C-04, C-23 |
| 4 | UNKNOWN nunca → MATCH | D-04 | F1 | C-03, C-04 |
| 5 | Preflight de portabilidad | D-06 | F2 | C-06, C-07 |
| 6 | Esquemas neutrales | D-07, D-14 | F2 | C-05, C-08 |
| 7 | Detalles en adapters y catálogos | D-07 | F2 | C-08, C-09 |
| 8 | Independencia por riesgo | D-12 | F3 | C-13 |
| 9 | Reconstrucción sin memoria privada | D-08, D-17 | F6 | C-25 |
| 10 | Recuperación determinista | D-09 | F4 | C-15, C-16 |
| 11 | Sin secretos | D-06, D-07 | F2 | C-10 |
| 12 | Dos topologías o límites honestos | D-17 | F6 | C-24, C-26 |
| 13 | I-61 intacto | D-14, D-18 | todas | C-19, C-20 |
| 14 | No bloquear producto | D-18 | todas | C-20; DC-07 por hito |

## 2. Roles (D-01) y acumulación

| Rol | Semántica | Declara | Nunca declara |
|---|---|---|---|
| **PRINCIPAL_COORDINATOR** | sesión responsable de la unidad: worktree, preflight, relevos, propuesta de bindings, trabajo directo autorizado, hechos y custodia | hechos del relevo, del remoto, del rebase y del preflight; `CONFIGURATION_STATUS` propio | `EXECUTION_*`; GATE PASS salvo SAME-SESSION ROLE de Coordinator declarado |
| **ARCHITECT** | revisión de diseño y conformidad (LIFECYCLE §5, §9) | veredicto de LIFECYCLE | GATE PASS, `EXECUTION_*` |
| **EXECUTION_CONTROLLER** | planificación y verificación, solo lectura (§16) | `EXECUTION_VERIFIED/REWORK_REQUIRED/BLOCKED` | GATE PASS, Candidato, cierre, integración |
| **WORKER** | escritura dentro del alcance | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS |
| **REVIEWER** | revisión operativa fuera de LIFECYCLE, sin autoridad de Architect ni de Controller | hallazgos | `EXECUTION_*`, GATE PASS, veredictos de LIFECYCLE |

El Coordinator de gates no es vinculable. **Acumulación**, comparando la identidad **observable** del actor (`ActorRef`, Anexo B.2) y no el `BindingId`:

| Mismo actor en … | Regla |
|---|---|
| WORKER + EXECUTION_CONTROLLER de la misma entrega | **prohibido**: `ActorRef` y `SessionRef` deben diferir (ADR-0046 #2) |
| WORKER + REVIEWER de su entrega | autorrevisión permitida, nunca independiente |
| PRINCIPAL_COORDINATOR + WORKER (subagente) | permitido (alternancia interna, 16.4) |
| PRINCIPAL_COORDINATOR + REVIEWER / ARCHITECT / Coordinator | SAME-SESSION ROLE declarado; nunca independiente |
| ARCHITECT + EXECUTION_CONTROLLER | permitido si §11 no exige independencia entre ambos |
| REVIEWER + EXECUTION_CONTROLLER del mismo trabajo | permitido; no añade independencia |

Un rebinding o una etiqueta nueva **no** crea un actor distinto.

## 3. Mapa de autoridad: anterior → delta (C62-F0-09 §3)

| Dominio | Hoy | Delta propuesto | Autoridad |
|---|---|---|---|
| LIFECYCLE §5/§9 | revisión del Architect y conformidad; modo declarado | predicado de independencia para revisiones mayores **solo si OD-6 elige la alternativa 1** (§11.4) | Owner (OD-6) + LIFECYCLE |
| AUTOMATION_PLAN §16 | ejecución delegada (ADR-0046 #1) | roles, binding y aceptación, observación de capacidad, autoverificación del Principal, contrato de adapter, custodia operativa, recuperación, conteo y adopción por unidad | AUTOMATION_PLAN; **ampliación de dominio** (abajo) |
| AUTOMATION_PLAN §8 | formato del estado | `rackcad-automation-state/v2` para unidades I62; `/v1` sigue para I61 (Anexo B.8) | AUTOMATION_PLAN |
| WORKFLOW §§3-4 | reclamo, worktree, apertura, relevo | solo referencias (paso «al abrir» → autoverificación de §16 para unidades I62) | WORKFLOW |
| WORKFLOW §10 | fila «Operación del ejecutor y ejecución delegada» | texto de la fila ampliado | WORKFLOW + Owner (OD-1) |
| `routing.md` / catálogo | selección por capacidades | clase y perfil de coordinación principal; binding de todos los roles; §7 al adapter | Freeze de I-61 §15 |
| PROMPT_TEMPLATES §G / adapters | representación | renderizado en el adapter; corrección del bloque factual de §2 (F1) | PROMPT_TEMPLATES |
| AGENTS | evidencia | sin cambios | — |

**Ampliación del dominio de §16:**
- **Fuente anterior:** ADR-0046 #1 y la fila de WORKFLOW §10.
- **Delta:** obligaciones de la sesión principal **fuera** de una delegación.
- **Autoridad:** Owner (OD-1). No se ejerce antes de la vigencia (D-15) y solo aplica a unidades I62 (§14).

## 4. Perfil del Principal y estado de configuración

### 4.1 Perfil (D-03) y capacidades (O62-V1-01, preservado)

Clase «Coordinación principal», perfil PRINCIPAL_COORDINATION (Long-horizon, Frontera). Requisitos **obligatorios**, como capacidades:
- nivel y effort;
- `repo-write`;
- `remote-facts`;
- `introspection` ≥ RUNTIME_OBSERVED.

`build-test` solo para el rol y la acción que deban producir evidencia local. El perfil y sus requisitos existen **aunque el binding del Principal no pueda
aceptarse**: la autoverificación produce su preflight y su disposición sin depender del binding (Anexo B.4).

### 4.2 `CONFIGURATION_STATUS` y disposición (D-04)

Estados exactos: **MATCH**, **ABOVE_REQUIRED**, **BELOW_REQUIRED**, **UNKNOWN**. Agregado solo sobre los requisitos **obligatorios**:
1. BELOW_REQUIRED si alguno es insuficiente;
2. si no, UNKNOWN si falta acreditar alguno;
3. si no, ABOVE_REQUIRED;
4. en otro caso, MATCH.

Las métricas opcionales no alteran el agregado. Se registran **todas** las causas. El estado no es la disposición.

| Situación | Disposición |
|---|---|
| Principal en BELOW_REQUIRED | STOP antes de trabajo sustantivo (P-09) |
| Rol en BELOW_REQUIRED o UNKNOWN | no hay binding ni trabajo dependiente (P-10); la investigación independiente sigue |
| Evidencia material contradictoria (incluidas dos filas del mismo requisito con valores distintos) | STOP S-04, **además** de UNKNOWN en ese requisito |
| MATCH / ABOVE_REQUIRED | habilita solo la configuración; los demás STOP siguen |
| Decisión del Owner | cambia requisitos o acepta riesgos dentro de su autoridad; nunca convierte lo no observado en MATCH |
| Recuperación | nueva observación; nunca reconfiguración automática ni reset |

Ejemplos con esperado independiente; una vez acordada, esta tabla es la fuente del oráculo de C-03, C-04 y C-23:

| Caso | Entrada | Agregado | Disposición |
|---|---|---|---|
| E1 | todo = requisito; RUNTIME_OBSERVED | MATCH | sigue |
| E2 | effort > requisito | ABOVE_REQUIRED | sigue y registra |
| E3 | effort < requisito | BELOW_REQUIRED | STOP P-09 (Principal) |
| E4 | effort sin fuente | UNKNOWN | P-10 |
| E5 | nivel < requisito; effort sin fuente | BELOW_REQUIRED; `Causes` = {nivel, effort} | STOP P-09 |
| E6 | dos observaciones RUNTIME_OBSERVED simultáneas y distintas | UNKNOWN; `Contradiction` | STOP S-04 |
| E7 | observación invalidada (§6) | UNKNOWN | revalidar |
| E8 | rebinding del Worker | Y evaluado desde cero | contadores intactos |
| E9 | cuota desconocida (opcional) | MATCH | registrada |
| E10 | el Owner rebaja un requisito | recálculo | lo no observado sigue UNKNOWN |
| E11 | requisito obligatorio omitido en el preflight | UNKNOWN (falta acreditar) | P-10; `Causes` lo nombra |

## 5. Binding (D-05)

**Entradas:**
- requisitos del rol;
- independencia (§11);
- observación de capacidad vigente (§6);
- **elegibilidad** de ADR-0046 #4, que se conserva: invocación **medida** de la celda, **consumo cubierto** y celda no `STALE`.

Una cuota desconocida no equivale a consumo autorizado, y una entrada de catálogo no acredita los hechos.

**Orden, sin ciclos.** El binding consume la observación; la observación **no** depende del binding (§6):
1. candidatas del catálogo;
2. observación de capacidad (preflight);
3. invocación medida previa, de una medición autorizada que no es un binding;
4. registro de binding (transitorio, §8);
5. aceptación A7' por el Coordinator (transitoria hasta la custodia);
6. cesión al rol (diario, §8).

Una celda sin medición previa no se vincula.

**Algoritmo:**
1. requisitos;
2. candidatas con descriptor;
3. filtro de los obligatorios en MATCH o ABOVE_REQUIRED y de la elegibilidad;
4. independencia sobre `ActorRef`, `SessionRef`, entradas y proveedor;
5. nivel más bajo adecuado y transporte;
6. registro `rackcad-binding/v1` (Anexo B.5);
7. aceptación.

**Sin celda elegible:** no se invoca. **Rebinding:** registro nuevo, mismo ámbito y misma `TaskId` (Anexo B.2), contadores intactos.

## 6. Observación de capacidad y comprobaciones de relevo (D-06)

Se separan dos objetos:

| Objeto | Qué observa | Invalidadores pertinentes | Quién y cuándo |
|---|---|---|---|
| **Observación de capacidad** (`rackcad-preflight/v1`) | un runtime candidato para un rol: localización y versión del ejecutable, autenticación (sin credenciales), introspección y nivel de garantía, permisos y sandbox, huella de configuración, celda del catálogo, perfil y requisitos | cambio de **instancia de host**, runtime, versión o ruta del binario, estado de autenticación, huella, blob de la entrada de catálogo o versión de `routing.md`. **No** la invalidan el binding (que la consume) ni el SHA de la rama | la sesión, antes del binding y al abrir (Principal) |
| **Comprobaciones de relevo** (16.4, sin cambio de semántica) | Git (`HEAD`, remoto, `origin/main`, árbol limpio, operaciones en curso), procesos, delegaciones abiertas y huella antes y después | se repiten en cada Exit/Entry | la sesión, en cada invocación |

Una CLI encontrada o un esquema válido no acreditan una invocación. Los esperados de cualquier control del host son **lo observado en el momento del ensayo**,
no una foto anterior.

## 7. Frontera de adapters (D-07)

**Contrato** (normativo en §16). Nueve operaciones:
1. describir;
2. observar;
3. renderizar;
4. invocar;
5. observar el resultado;
6. cancelar;
7. confirmar la terminación;
8. clasificar procesos;
9. declarar la huella.

El **descriptor** declara por operación DISPONIBLE, NO APLICA o UNVERIFIED. Una operación UNVERIFIED que el rol necesita hace que la celda no sea elegible para
ese rol. Una sesión existente no acredita por sí misma invocación, cancelación ni terminación.

| Adapter | Rol previsto | 1 | 2 | 3 | 4 | 5 | 6 | 7 (terminación) | 8 | 9 (huella) |
|---|---|---|---|---|---|---|---|---|---|---|
| `claude-desktop-session` | PRINCIPAL | DISP. | DISP. (`get_session`) | N/A | N/A | N/A | N/A | **por observador externo**: candidata `isRunning` de los metadatos de la app, observada por otra sesión autorizada (§9.1); UNVERIFIED hasta F2 | UNVERIFIED | UNVERIFIED (ajustes) |
| `claude-subagent` | WORKER, REVIEWER | DISP. | DISP. (transcripción) | DISP. | DISP. | DISP. | DISP. (60 min) | DISP. (notificación + procesos) | DISP. | UNVERIFIED |
| `claude-cli` | REVIEWER, ARCHITECT | DISP. | NOT_AUTHENTICATED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED |
| `codex-cli` | CONTROLLER, ARCHITECT, WORKER | DISP. | DISP. | DISP. | DISP. (binario a revalidar) | DISP. | DISP. (600 s) | DISP. | DISP. | **DISP.: `config.toml`, obligatoria** |
| `codex-desktop-session` | PRINCIPAL (B) | DISP. | candidata (SP-3) | N/A | N/A | N/A | N/A | UNVERIFIED (sin observador conocido) | UNVERIFIED | UNVERIFIED (¿comparte `config.toml`?) |

**Huella:** «ninguna» solo con la demostración del descriptor y la **aceptación registrada del Coordinator en el gate F2**. `codex-*` declara `config.toml`;
**P-01 se conserva**.

**Hechos del adapter y su validación:** Anexo B.3. El objeto `Facts` es el **único punto abierto** del esquema del núcleo, y su frontera estricta es el
esquema del adapter. Lo valida el productor; el aceptante lo contrasta con los hechos de Exit/Entry de 16.4, tomados en otro instante.

## 8. Custodia por unidad (D-08)

**Puntos durables** (commits en la rama de la unidad), solo donde 16.4 permite escrituras Git de la sesión:

| Punto | Momento | Contenido |
|---|---|---|
| **Q0** | antes de abrir una delegación: paso (1) de 16.4 | `state.custody`: delegación **planificada** (`TaskId`, `Attempt`), bindings ya aceptados (p. ej., el Controller de planificación), `record_version` n+1, contadores; preflight del Principal custodiado |
| **Q7** | tras la verificación y los controles: paso (7) de 16.4 | custodia de todos los artefactos transitorios de la delegación; `open_delegation = null`; `last_verified_sha`; `record_version` n+2; contadores |
| **QR** | recuperación o transferencia del Principal en un punto quiescente (sin delegación con participante activo) | transferencia por CAS (§9.2) |

**Entre Q0 y Q7 no hay escrituras Git de la sesión** (16.4 sin cambio). Las transiciones de cesión van al **diario transitorio encadenado**: los
`relay-record/v2` del directorio del intento, cada uno con `PrevRelaySha256` (Anexo B.7):
- cesión P→Controller (planificación);
- terminación del Controller;
- aceptación;
- cesión P→Worker;
- terminación del Worker;
- cesión P→Controller (verificación);
- terminación;
- controles.

En Q7 el diario se custodia en `docs/automation/evidence/<unit>-agent/<task>/…` y queda durable. Una referencia transitoria **nunca** se presenta como evidencia
custodiada (Anexo B.2, `Location`).

**SHAs distintos:**

| SHA | Qué es |
|---|---|
| `BaseSha` | `HEAD` al abrir la delegación = Q0 |
| SHA del trabajo | `RedSha`, `CurrentSha` = commits del Worker |
| SHA de estado | Q0 y Q7 |
| SHA evaluado del gate | `CurrentSha` + commits de la sesión limitados a `docs/automation/` y a los documentos de la unidad (16.1, sin cambio) |
| revisión de evidencia | Q7 |

`Identity` (`HEAD` = remoto = `CurrentSha`) **no cambia**: entre los commits del Worker y la verificación no hay commit de la sesión.

**CAS por Git en Q0, Q7 y QR:** se lee `record_version` n en `HEAD` = `origin/<rama>`, se escribe n+1 y se hace push sin force. Un **push rechazado** significa
transición **no acreditada en el remoto**, no inexistencia de cambios locales: el commit local se conserva. Se inspecciona la causa antes de repetir:

| Causa observada | Tratamiento |
|---|---|
| no fast-forward (el remoto avanzó) | `fetch` y clasificar según T10 |
| permisos o autenticación | S-06 |
| transporte | reintento acotado tras la inspección |
| estado remoto desconocido | STOP |

Sin servicio de locks. Los compromisos compartidos son solo referencias. No hay registro global. Git prueba identidad y remoto, **no** quién opera en otra
máquina.

## 9. Recuperación (D-09) y presupuestos (D-10)

### 9.1 Evidencia de fin de un escritor

| Evidencia | Definición | Basta para |
|---|---|---|
| **TERMINATION_ACCREDITED** | operación 7 del adapter ligada al escritor exacto (`ActorRef`/`RunId`, PID + `CreationDateUtc`) **y**, para invocaciones de trabajo, `Outcome` registrado. Para sesiones de Principal, la observa **otra sesión autorizada** (p. ej., la sesión de supervisión con los metadatos `isRunning` de la app) o el Owner, nunca la propia sesión observada | cerrar la cesión o transferir la custodia, con las demás comprobaciones |
| **ISOLATION_ACCREDITED** | no se admite (no hay mecanismo medido) | — |
| **NO_OBSERVATION** | ausencia de observación | nada |

Estados del escritor: **WRITER_ALIVE**, **TERMINATION_UNACCREDITED** y **ORPHAN_CONFIRMED** (terminación acreditada sin entrega ni cesión resuelta). Los dos
últimos no autorizan tomar posesión por sí mismos.

### 9.2 Transiciones (análisis completo con SHAs simbólicos en el Anexo F)

| # | Estado previo | Evento | Precondición / evidencia | Decisor | Registro | Permitido / prohibido |
|---|---|---|---|---|---|---|
| T1 | HELD(P) en Q0 | cesión a C (planificación) | Exit 16.4 en pass; binding de C aceptado | P | diario (relay r1) | P no opera en el alcance |
| T2 | cesión a C | terminación de C | TERMINATION_ACCREDITED + Entry | P | diario | — |
| T3 | tras planificar | aceptación A1'-A8' | registros válidos | Coordinator | diario (aceptación) | sin escritura Git |
| T4 | aceptado | cesión a W | Exit en pass | P | diario (r2) | P no opera |
| T5 | cesión a W | W publica R y G y entrega | handoff + `Outcome` COMPLETED + terminación acreditada + Entry (`HEAD` = remoto = G) | P | diario | sin escritura Git |
| T6 | tras T5 | cesión a C (verificación) | Exit; `Identity` sobre G | P | diario (r3) | — |
| T7 | verificado | controles negativos y Q7 | VERIFIED o disposición; controles | P | **Q7** (CAS) | — |
| T8 | cesión activa | tope vencido | operaciones 6 + 7 | P | diario | si 7 no es acreditada → T11 |
| T9 | cesión a W | W caído con commits sin handoff | terminación acreditada; commits en el remoto | P | diario | `Handoff` → BLOCKED; el Controller puede verificar commits verificables |
| T10 | cualquiera | `HEAD`/remoto ≠ lo esperado | clasificación: (a) avance del Worker dentro del alcance durante su cesión → normal (T5); (b) commit local de la sesión sin publicar → publicar en un punto permitido o, si estaba en cesión, P-08; (c) escritura ajena (commits no atribuibles al escritor vinculado) → STOP P-02/S-07; (d) `main` cambió → S-13 / 16.7 | — | según el caso | no todo desajuste va a rebase |
| T11 | cesión o HELD con P ausente | sin observación | NO_OBSERVATION | Coordinator | ninguno; STOP P-12 | prohibido tomar posesión, reset, borrado o matar procesos |
| T12 | ORPHAN_CONFIRMED(X) | recuperación | decisión del Coordinator que **designa** a N + inspección que preserva el trabajo | Coordinator / N | **QR** (CAS) | solo N, designado, opera tras ganar el CAS |
| T13 | cualquiera | dos recuperaciones concurrentes | — | — | gana el primer CAS | el perdedor **no opera**, aunque relea: sin designación vigente y transición nueva no hay operación; no hay dos sesiones en el mismo worktree ni en la misma rama |
| T14 | cualquiera | cambio de máquina sin acreditar exclusividad ni recuperación | — | Coordinator | ninguno; STOP P-13 | no se reconstruye trabajo inaccesible |
| T15 | cualquiera | registro que contradice el hecho físico | contradicción | Coordinator | ninguno; S-04/P-02; corrección por una transición nueva | prevalecen los hechos (WORKFLOW §10) |
| T16 | HELD(P) en punto quiescente | rebinding del Principal (A→B) | terminación de A acreditada por un observador externo + decisión del Coordinator | Coordinator / B | **QR** (CAS) | B opera solo tras el CAS |

**Fallos del mandato:**
- **Principal ausente:** T11 / T12 / T16.
- **Worker caído:** T9.
- **Controller sin contexto:** **S-12 = STOP**, `analysis.md` y decisión del Coordinator (16.11, sin cambio, Anexo G).
- **Cuota agotada:** P-06.
- **Autenticación perdida:** S-06.
- **Cambio de máquina:** T14.
- **Handoff ausente:** T9 / `Handoff`.
- **Artefactos transitorios:** se conservan y no se reutilizan.
- **Trabajo sin commit en el host:** se preserva.
- **SHA distinto:** T10.

### 9.3 Presupuestos y su autoridad

| Contador | Evento que cuenta | Autoridad | Reconstrucción y desacuerdo |
|---|---|---|---|
| `attempts` | **corrección lanzada** (16.8) | `state` en `origin/<rama>`, incrementado en el commit previo a emitir la corrección (16.8) | durable |
| por clase (`TaskId`, `FailureClass`) | **corrección lanzada** de esa clase, no verificaciones | el mismo commit de 16.8 registra `CorrectionLaunch {Seq, FailureClass, CorrectionOfRunId}` en `state.custody.counters` | durable. Dos verificaciones de la misma entrega no son dos correcciones. Una corrección lanzada y caída antes de verificar sí cuenta |
| reejecuciones BLOCKED (`TaskId`, fase) | reejecución lanzada | registro de lanzamientos en el diario (`relay-record`), custodiado en Q7 | si el diario se perdió (T14) → STOP; nunca cero |
| recuperaciones de 16.7 | recuperación | `RebaseMap` | ídem |
| invocaciones | **lanzamiento**, cada uno con `RunId` | diario (`Outcome.LaunchedPid` o `SessionRef`); un lanzamiento con resultado incierto se cuenta como lanzado (`LAUNCH_UNCERTAIN`) | P-07 compara lanzados + inciertos con el tope **antes** de lanzar |

Un rebinding conserva `TaskId` y los contadores. Una `TaskId` nueva solo por decisión del Coordinator. Si continúa el mismo trabajo, declara
`ContinuesTaskId` y **hereda** los contadores. No puede reiniciar la misma cadena ni la misma clase de fallo.

## 10. Fuentes de introspección (D-11)

Niveles:
- **REQUESTED**;
- **CONFIGURED**;
- **RUNTIME_OBSERVED** (metadato escrito por el runtime, ligado a sesión, turno o invocación e instante);
- **SERVICE_ATTESTED** (adicional, no exigida).

La autodeclaración del modelo no es fuente. Mínimo para los obligatorios: **RUNTIME_OBSERVED**; por debajo, UNKNOWN. Solo REQUESTED no se presenta como
efectivo.

| Adapter | Fuente | Nivel | Límite |
|---|---|---|---|
| `claude-desktop-session` | `get_session` (`model`, `effort`, `sessionId`) | RUNTIME_OBSERVED (verificar en F2) | configuración del cliente, no el backend |
| `claude-subagent` | transcripción | RUNTIME_OBSERVED | prompt enmarcado |
| `codex-cli` | registro de sesión sin `--ephemeral` | RUNTIME_OBSERVED | binario actual sin medir |
| `codex-desktop-session` | `turn_context.model/effort` (SP-3) | RUNTIME_OBSERVED, **candidata** | ligar a unidad y rol; no generalizar formatos |
| `claude-cli` | — | UNKNOWN | NOT_AUTHENTICATED |

## 11. Independencia por riesgo (D-12)

### 11.1 Dimensiones sobre la identidad observable

| Dimensión | Se satisface cuando … | Evidencia |
|---|---|---|
| **Actor** | `ActorRef` distinto (instancia de runtime observada: `sessionId`, `thread_id`, id del subagente, o PID + `CreationDate` + instancia de host). **No** basta un `BindingId` distinto | `ActorRef` con Assurance RUNTIME_OBSERVED |
| **Sesión** | `SessionRef` distinta (sesión de runtime de nivel superior; un subagente comparte la de su padre) | `SessionRef` RUNTIME_OBSERVED |
| **Contexto** | las entradas son solo artefactos canónicos enumerados (prompt con SHA-256 + rutas en un SHA), sin transcripción, memoria ni razonamiento de la referencia, y con las entradas automáticas del runtime enumeradas (§12) | `prompt.md` custodiado + lista de entradas + entradas automáticas + auditoría de lecturas cuando exista |
| **Proveedor** | proveedor distinto según los descriptores | descriptores |

Valores: **REQUIRED**, **PREFERRED**, **NOT_REQUIRED**. UNKNOWN en una dimensión REQUIRED no satisface. Un proveedor distinto con el contexto de la referencia
no satisface Contexto. Una marca no se prohíbe si la independencia se acredita.

### 11.2 Combinación y satisfacción

Por (referencia, dimensión) se toma el **máximo** (REQUIRED > PREFERRED > NOT_REQUIRED). Las referencias distintas se evalúan por separado. Si falta un
REQUIRED, el binding del revisor o verificador no se acepta; la revisión sigue pendiente y la operación dependiente se bloquea. Un PREFERRED no satisfecho se
registra.

### 11.3 Disparadores del dominio de §16

«Actor» se evalúa sobre `ActorRef`; «Contexto», con las entradas enumeradas de §12 y del Anexo D.6. Los casos de control están en el Anexo C (C-13).

| Disparador (mandato) | Rol o revisión | Referencia | Actor | Sesión | Contexto | Proveedor | Si falta un REQUIRED |
|---|---|---|---|---|---|---|---|
| Toda delegación | EXECUTION_CONTROLLER (verificación) | WORKER | REQUIRED | REQUIRED | REQUIRED | NOT_REQUIRED | no es posible VERIFIED: BLOCKED de planificación |
| Coordinator y Worker en la misma sesión | EXECUTION_CONTROLLER | sesión Coordinator/Worker | REQUIRED | REQUIRED | REQUIRED | PREFERRED | ídem |
| Cambio de autoridad compartida | EXECUTION_CONTROLLER + REVIEWER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | revisión pendiente; la tarea no se cierra |
| Operación destructiva o irreversible | REVIEWER de la autorización (además de S-05) | quien la propone | REQUIRED | REQUIRED | REQUIRED | NOT_REQUIRED | no se ejecuta |
| Cambio sensible a la seguridad | REVIEWER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | revisión pendiente |
| Evidencia ambigua de alto coste | REVIEWER | autor de la evidencia | REQUIRED | PREFERRED | REQUIRED | PREFERRED | la evidencia no se usa para decidir |
| Alto coste de fallo (dimensión `High`) | EXECUTION_CONTROLLER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | como «toda delegación», con el proveedor registrado |
| Acumulación de roles | según §2 | — | — | — | — | — | combinación prohibida → binding rechazado |
| Cableado rutinario acotado | REVIEWER (si se pide) | WORKER | NOT_REQUIRED | NOT_REQUIRED | NOT_REQUIRED | NOT_REQUIRED | — |

### 11.4 Revisiones de LIFECYCLE: OD-6, pendiente del Owner, con ambas alternativas delimitadas

| | Alternativa 1 (recomendación del Coordinator, no decisión) | Alternativa 2 |
|---|---|---|
| Revisiones afectadas | revisión de diseño de NEW ARCHITECTURE y de FOUNDATION EVOLUTION antes del Freeze; conformidad de READY-06 | ídem |
| Actor de referencia | la sesión autora del diseño o de la implementación revisada (`ActorRef`/`SessionRef` de su binding de PRINCIPAL_COORDINATOR) | ídem |
| Predicado | Actor, Sesión y Contexto **REQUIRED**; Proveedor **PREFERRED** | modo declarado; Contexto y Proveedor **PREFERRED** |
| Evidencia | identidad observable del revisor y del autor; entradas canónicas del revisor | declaración del modo |
| Alcance | **solo unidades I62**; no retroactivo a I-62, I-63, I-64 ni a ninguna unidad I61 | ídem |
| Efecto | SAME-SESSION ROLE deja de bastar para esas revisiones; una revisión del Coordinator no se convierte en dictamen del Architect | sin cambio de LIFECYCLE |

Una vez decidida, la alternativa queda identificada por versión y cláusula en el Freeze. Sin decisión, V3 conserva ambas y no hay AGREED ni Frozen: YES. No se
crea otro bloqueo externo.

## 12. Pilotos y portabilidad (D-17)

Especificación completa en el **Anexo D**:
- fixture arrancable con activación de prueba;
- semántica de `Ci`;
- hoja de invocaciones por escenario, rol y fase, con topes por ronda y totales;
- FX-04 integrado en el punto quiescente Q0 de FX-02, con terminación de A observada por un observador externo, CAS de la transferencia y continuación por B
  hasta verificar el SHA correcto;
- entradas canónicas, técnicas y privadas delimitadas, con la cobertura del registro;
- FX-05 distingue los gates legítimos del fixture de un intento de actuar sobre la unidad real;
- límites por topología, rol y escenario.

## 13. Semántica de fallo (D-13)

Los STOP de I-61 se conservan, con S-12 sin cambio. Ids nuevos:

| Id | Condición | Comportamiento |
|---|---|---|
| P-09 | Principal en BELOW_REQUIRED al abrir | STOP antes de trabajo sustantivo |
| P-10 | requisito obligatorio de un rol en UNKNOWN o BELOW_REQUIRED | no se vincula ni se invoca; no consume `attempts` |
| P-11 | huella declarada cambiada durante una cesión | STOP (P-01 para Codex, igual) |
| P-12 | TERMINATION_UNACCREDITED u ORPHAN_CONFIRMED sin decisión | STOP; sin toma de posesión |
| P-13 | cambio de máquina sin acreditación | STOP |
| P-14 | adapter desconocido, esquema ausente, versión incompatible o hechos inválidos | no elegible |
| P-15 | `ProtocolSet` ≠ protocolo de la unidad, o clasificación UNKNOWN en un paso que depende de ella | STOP |
| P-16 | un resultado del plano (c) intenta actuar sobre el plano (a) | rechazo, sin efecto real |

Matriz de cada cambio frente a `/v1` (conservado, ampliado o sustituido) y casos de cierre: **Anexo G**.

## 14. Adopción por unidad y lectura legacy (D-18)

**Protocolos:**
- `I61` = los cinco `/v1` + las cláusulas de I-61;
- `I62` = el conjunto del Anexo B.1 + las cláusulas de I-62.

`worker-handoff/v1` pertenece a ambos, con compatibilidad demostrada (B.6).

**Punto efectivo** (anclado como WORKFLOW §11.2): `I62_EFFECTIVE_SHA` es el primer merge en first-parent de `origin/main` de RackCad cuyo segundo padre alcanza
el **único** commit con el trailer `Agent-Protocol-Normative: I-62` y cuyo primer padre no lo alcanza. Vigencia: al aceptarse el push de `main`. **Ausencia,
duplicidad o derivación contradictoria = activación no válida**: ninguna unidad es I62, y STOP al Owner. La identidad se registra en el tag
`integration/I-62`.

**Clasificación** (WORKFLOW §11.3 como precedente):
- I-62 conserva snapshots **PRE** y **POST** de `git ls-remote --heads origin` alrededor del push normativo, con la tabla `ref / tip / commit de reclamo /
  Claim-Id`, en el cuerpo del merge y en el tag.

| Clase | Condición | Protocolo | Paso dependiente |
|---|---|---|---|
| **ANTERIOR_DEMOSTRADA** | rama en PRE con reclamo y Claim-Id identificables | I61, todo su recorrido | — |
| **POSTERIOR_DEMOSTRADA** | reclamo posterior probado (fuera de PRE, primer push tras el push normativo) y base que contiene `I62_EFFECTIVE_SHA` | I62 | — |
| posterior con base obsoleta | reclamo posterior, base sin el efectivo | I62 | **STOP** de toda delegación hasta el rebase |
| **DESCONOCIDA** | solo en POST sin prueba de orden, Claim-Id ausente o duplicado, o identidad contradictoria | **ninguno asignado** | **STOP** de los pasos que dependen de la clasificación (contratos y delegaciones); se pide la evidencia concreta. Sin default I61 ni I62 |

**Unidades sin campo `protocol`** (todas las anteriores, con estado `/v1`): se leen por su pertenencia a la tabla PRE (durable en I-62), sin editar su rama ni
su bootstrap y sin inferir desde su ascendencia actual.

**Sin efecto retroactivo:** OD-6 y todas las reglas nuevas aplican solo a unidades I62. La clasificación ANTERIOR **no** obliga a ninguna unidad a usar la
delegación de I-61: cada una conserva su régimen (I-52 su Workflow y su alcance autorizado; I-63, I-64 e I-62 lo suyo) y no gana trabajo que no tuviera
autorizado.

**Lectura de autoridades:** **mapa de compatibilidad por superficie y cláusula en el Anexo E**. Una unidad I61 lee, para las cláusulas que I-62 modifica, su
versión en `I62_EFFECTIVE_SHA^1`, y el resto en `main`. Las superficies nuevas de I62 no le aplican. El mapa de cláusulas modificadas se conserva en la
integración (como WORKFLOW §11.3 para V2). Trazas de cierre (análisis): Anexo E.3.

## 15. Planos, MaterializationClose y fixture (D-15)

| Plano | Qué incluye | Autoridad | Nunca puede |
|---|---|---|---|
| (a) real | la sesión de I-62 y la gobernanza | I-61 y Workflow vigentes | usar reglas de I-62 sin integrar como autoridad |
| (b) materializado inactivo | textos y contratos de F1-F4 en la rama, con cláusula de vigencia «desde `I62_EFFECTIVE_SHA`, solo para unidades I62» | ninguna hasta la vigencia | gobernar operaciones reales |
| (c) sistema bajo prueba | el **repositorio fixture** con su propia activación de prueba (Anexo D.1) | las reglas de (b) **activadas dentro del fixture** por su propio merge marcado `TEST-ACTIVATION` | acreditar GATE PASS, READY, aprobación del Owner o integración reales; cambiar `main`, contadores, decisiones o custodia reales (P-16) |

**MaterializationClose (regla de I62, generaliza «cierre de G2»):**
- para una unidad con cambios normativos propios: el commit que cierra el gate con el **último** cambio a superficies normativas o de plantilla, revisado por el
  Coordinator contra el Freeze;
- para una unidad sin cambios normativos: el commit de bootstrap o el último commit de `UNIT_DOC` aceptado por el Coordinator. `AuthorityRevision` sigue la
  regla de 16.3 con ese hito.

**Para I-62 (plano a):**
- **MC_I62** = cierre de F4;
- un cambio posterior a esas superficies lo invalida: se cierra de nuevo, se resiembra el fixture y se repiten los pilotos afectados;
- I-62 **no delega trabajo real**: su `AuthorityRevision` no se usa;
- la restricción de 16.3 queda registrada.

**En el fixture (plano c):**
- las reglas I62 son **EXTERNAL** en su `main` (activado por prueba);
- la unidad de prueba FX-U1 no tiene cambios normativos: su MaterializationClose es su bootstrap.

**OD-5, una sola semántica:**
- los ensayos son **pruebas del plano (c) en otro repositorio con sus propias autoridades**, no ejecución delegada de §16 de una unidad de RackCad;
- el Coordinator los autoriza dentro del plan congelado de F6;
- el Owner autoriza el consumo, la apertura de sesiones del sistema bajo prueba y las salvaguardas del host. Esas salvaguardas son las comprobaciones de 16.4
  aplicadas al worktree del fixture (procesos, huella antes y después, cesión).

Si el Owner no acepta esa semántica, F6 no procede y cualquier otra vía exige una A-n. **No** se congelan dos semánticas.

**OD-7** (infraestructura del remoto del fixture con CI) es un permiso de infraestructura, **no** una excepción normativa.

## 16. Invariantes y obligaciones (resumen; matriz única en el Anexo C)

**Clases según AGENTS, sin equivalencias nuevas:**
- **(i)** pruebas automatizadas Core: guardas nuevas con RED→GREEN, como en I-61 G2, y mutation check en memoria;
- **(ii)** controles registrados en la evidencia (MC: manual reproducible; FX: en el fixture; OV: del Owner). **No son pruebas automatizadas, no sustituyen
  ninguna clase de AGENTS** y no eximen de las condiciones de cierre de un gate (LIFECYCLE §7; AGENTS: Core Full al cierre de un gate funcional V2).

Las pruebas `/v1` vigentes (`AgentExecutionProtocolTests`) se conservan **con sus oráculos intactos**. Las guardas nuevas viven en una clase de prueba aparte.

**Un control que falla:**
- si la implementación se aparta del Freeze → **corrección** que devuelve al Freeze, sin A-n;
- si el Freeze es incorrecto o incompleto → **A-n** con sus autoridades.

## 17. Plan de gates (D-16)

**Level A, decidido antes del Freeze.** F4 verifica la viabilidad del procedimiento versionado (C-15, C-16, C-17). Si falla un control, se aplica la regla de
§16. F5 no se planifica.

| Gate | Resultado observable | Obligaciones (Anexo C) | Dependencias |
|---|---|---|---|
| **F0** | Proposal acordada y congelada (misma versión exacta) | — | OD-6 decidida; cero REQUIRED |
| **F1** | **ADR sucesor formal creado como «propuesto»** (número asignado en ese momento, comprobado contra `main` y las ramas activas), **sin** fila de índice: el índice se actualiza en el cierre documental (WORKFLOW §11.4); sin ventana. Materialización (b) de roles, perfil, estados, autoverificación, renderizado, cláusula de vigencia y corrección de PROMPT_TEMPLATES §2 | C-01..C-04 | OD-1 no bloquea |
| **F2** | Observación de capacidad + descriptores y esquemas de hechos; `preflight/v1`, `relay-record/v2`, `controller-verification/v2`; huellas aceptadas | C-05..C-10 | OD-2 solo para medir **invocando** `codex-cli` |
| **F3** | Binding, independencia, `binding/v1`, `gate-contract/v2`, `delegation/v2`, A1'-A8' y las 14 comprobaciones (Anexo G) | C-11..C-14 | — |
| **F4** | Custodia (state/v2, Q0/Q7/QR, diario encadenado), transiciones, presupuestos, adopción y mapa de compatibilidad, planos; **cierre = MC_I62** | C-15..C-21 | — |
| **F6** | Fixture arrancado (D.1) y escenarios FX-01..FX-05 | C-22..C-27 | OD-5; OD-7 (**precondición del cierre de F6**: sin CI no hay VERIFIED); OD-2 (A y B); OD-3 y OD-4 (B) |
| **F7** | Preparación del cierre sin cambio normativo: borrador factual de FOUNDATIONS en la evidencia, ideas-futuras, paquete OV | — | no depende de READY-06 |
| **READY-01..09** | LIFECYCLE §8 | — | OD-1 antes de READY-03 |
| **FINAL_CANDIDATE_SHA** | Full, CI exacta, OV final | OV-I62-01..05 | — |
| **Cierre documental** | FOUNDATIONS publicada, **índice ADR**, HANDOFF, ROADMAP (ventana) | — | WORKFLOW §11.4-11.5 |

No hay reutilización de evidencia por igualdad de árbol. Cada SHA tiene la suya.

## 18. Owner Validation y decisiones del Owner

**OV por escenario** (asignación a I-62; sin AutoCAD):

| Id | Lo prepara | Ensayo | Ejecución final sobre FINAL_CANDIDATE_SHA |
|---|---|---|---|
| OV-I62-01 autoverificación | F1 | C-23 (F6) | el Owner abre una sesión del sistema bajo prueba en el fixture resembrado desde el Candidato, con effort inferior y después correcto |
| OV-I62-02 preflight del host | F2 | C-06 | el Owner revisa el preflight producido con el procedimiento del Candidato, **con el estado observado en ese momento** |
| OV-I62-03 topología A | F6 | C-24 | ejecución compacta (D.5) |
| OV-I62-04 topología B o su limitación | F6 | C-26 | ídem si hay OD; si no, decisión del Owner sobre la limitación (el escenario no se retira) |
| OV-I62-05 portabilidad | F6 | C-25 | ejecución compacta |

**Decisiones del Owner:**

| Id | Decisión | Bloquea | Momento |
|---|---|---|---|
| **OD-6** | predicado de independencia de LIFECYCLE (§11.4) | acuerdo y Freeze de F0 | pendiente |
| OD-1 | ADR sucesor (ampliación de §16, fila de WORKFLOW §10; con OD-6 alternativa 1, LIFECYCLE) | READY-03 y vigencia | antes de READY-03 |
| OD-2 | línea base de huella por adapter (`config.toml`: `37DD3559…` registrada frente a `42E15A03…` observada) | toda invocación afectada (`codex-cli` en A y B; `codex-desktop-session` si comparte) | antes de la primera invocación afectada |
| OD-3 | autenticar Claude CLI | `claude-cli` en B | antes de F6 B |
| OD-4 | sandbox de Codex para escritura | Workers Codex (B y A→B) | antes de F6 B |
| OD-5 | permiso de ensayo con la semántica única de §15 y apertura de sesiones del sistema bajo prueba | F6 | antes de F6 |
| OD-7 | remoto del fixture con CI | **cierre de F6** (FX-02 no puede ser PASS sin CI) | antes de F6 |

Cada solicitud va aparte, con la ruta y el hash actuales, el efecto, el consumo, el alcance y las restricciones. El silencio no es decisión.

## 19. Riesgos

Los 12 retos del mandato, con su riesgo residual: [paquete V3](I-62-architect-package-v3.md) §5.

---

## Anexo A — Borrador del ADR sucesor PARCIAL de ADR-0046 (sin número; no es ADR formal)

- **Título:** «Ejecución delegada portable: roles independientes del proveedor, binding por capacidad acreditada y autoverificación del Principal».
- **Estado:** borrador. Se crea formal en F1 como **propuesto**, sin fila de índice hasta el cierre. La aceptación es del Owner (OD-1).

**Conserva de ADR-0046:**
- **#2:** declaraciones;
- **#3:** relevo;
- **#4:** elegibilidad por invocación medida, consumo cubierto y frescura;
- **#5:** esquemas estrictos, versionados por protocolo;
- **#6:** verificación fail-closed con 14 comprobaciones, delta en el Anexo G;
- **#7:** presupuesto único sin reinicios, con la autoridad de §9.3;
- **#8:** transporte;
- **#9:** Level A.

**Supera:**
- el proveedor fijo del Controller y el título;
- la «independencia parcial» aceptada → política por dimensiones.

**Amplía #1 (OWN-L):** obligaciones de la sesión principal fuera de una delegación; fila de WORKFLOW §10; con OD-6 alternativa 1, el predicado de LIFECYCLE.
Es materia **OWNER-RESERVED**.

**Vigencia:**
- desde `I62_EFFECTIVE_SHA`;
- solo unidades I62 (clasificación de §14);
- las unidades I61 terminan con I-61, con la lectura del Anexo E;
- `/v1` no se retira.

## Anexo B — Contratos de datos (DISEÑO)

### B.1 Conjunto `I62` y reglas comunes

| Esquema | Estado | Productor | Validación de forma | Comprobaciones entre artefactos |
|---|---|---|---|---|
| `rackcad-gate-contract/v2` | evoluciona | Coordinator | sesión | A2', A5', A6' |
| `rackcad-delegation/v2` | evoluciona | Controller | sesión (A1') | A2'-A8' |
| `rackcad-worker-handoff/v1` | sin cambios | Worker | Controller | B.6 |
| `rackcad-controller-verification/v2` | evoluciona | Controller | sesión (regla del README §8) | Coordinator |
| `rackcad-relay-record/v2` | evoluciona | sesión | sesión | Controller (`Termination`, `Remote`, `Routing`, `Denials`) |
| `rackcad-preflight/v1` | nuevo | sesión | sesión (dos fases, B.3) | aceptante (A7'), con contraste |
| `rackcad-binding/v1` | nuevo | sesión | sesión | Coordinator |
| `rackcad-adapter-<id>-facts/v<n>` | nuevo, por adapter | sesión | sesión (B.3) | aceptante |
| `rackcad-automation-state/v2` | evoluciona (§8) | sesión | Core (guarda) | CAS |

Reglas: JSON Schema 2020-12; `additionalProperties: false` en **todos** los objetos, **salvo el único punto abierto declarado** `AdapterFacts.Facts` (B.3);
todos los campos declarados son obligatorios; SHA de 40 hex; SHA-256 de 64 hex en minúsculas; instantes ISO-8601 con `Z`.

**Significado de los valores (sin ambigüedad):**

| Valor | Significado |
|---|---|
| ausencia de un campo | inválido |
| `null` | **solo** NOT_APPLICABLE, y solo en los campos marcados «nullable = no aplica» |
| desconocido u observable no observado | **nunca** `null`: se expresa con un **estado explícito** (`State`, `Assurance` = NONE, `Status` = UNKNOWN) |
| aceptación pendiente | `Acceptance.State` = PENDING (no `null`) |

### B.2 Identidades y referencias

| Identidad | Forma | Procedencia y resolución | Unicidad / inmutabilidad / errores |
|---|---|---|---|
| `ActorRef` | `{AdapterId, InstanceId, InstanceIdSource, Assurance}` | `InstanceId` observado del runtime: `sessionId` (escritorio), `thread_id` (Codex CLI), id del subagente, o PID + `CreationDateUtc` + instancia de host (proceso). Sin fuente → `Assurance` = NONE e `InstanceId` = `"UNOBSERVED"` | dos bindings con el mismo `ActorRef` = **mismo actor**; Actor no se satisface. `Assurance` NONE → la dimensión es UNKNOWN |
| `SessionRef` | `{AdapterId, SessionId, Assurance}` | sesión de nivel superior (la del padre para un subagente; la propia para una invocación CLI) | ídem |
| `HostRef` | `{HostLabelHash, HostInstanceHash, HostInstanceState, Os}` | `HostLabelHash` = SHA-256 del hostname (solo **etiqueta**). `HostInstanceHash` = SHA-256 de un identificador de instalación del SO (en Windows, el `MachineGuid`; su disponibilidad se verifica en F2). `HostInstanceState` = OBSERVED \| UNOBSERVED | **nunca acredita exclusividad**. `HostInstanceState` UNOBSERVED → no se hereda ninguna observación de capacidad entre sesiones. Dos máquinas homónimas tienen etiquetas iguales e instancias distintas: no heredan capacidad |
| `BindingRef` | `{UnitId, Scope (UNIT \| TASK), TaskId, Role, BindingId, Sha256, Location}` | `TaskId`: nullable = no aplica, **solo** si `Scope` = UNIT (Principal; Architect por unidad). `Location` = `{Kind (TRANSIENT \| CUSTODIED), Path, Commit, Blob}`, con `Commit` y `Blob` nullables = no aplica solo si TRANSIENT. TRANSIENT: ruta bajo el directorio del intento + SHA-256. CUSTODIED: blob en el commit Q7/QR | `BindingId` único por unidad. Contenido distinto con el mismo id → S-04. `UnitId` o `TaskId` ajenos → A2' rechaza. Un binding sustituido por un rebinding posterior del mismo rol y ámbito → **obsoleto**, rechazo. En Q7, toda referencia TRANSIENT usada por un artefacto custodiado se resuelve a CUSTODIED mediante el manifiesto de custodia (ruta + SHA-256 → ruta + blob). Un TRANSIENT nunca es evidencia custodiada |
| `PreflightRef` | `{PreflightId, Location, Sha256}` | `PreflightId` = `^P[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` | inválido si cambió un invalidador (§6) |
| `RunId` / `BindingId` | patrón de I-61 / `^B[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` | los asigna la sesión | únicos |
| `RequirementId` | `^[A-Z_]+\.[a-z0-9-]+$` | lista fija por perfil en `routing.md` (blob en la `AuthorityRevision`) | único por preflight. Dos filas del mismo id con valores distintos → `Contradiction` → UNKNOWN + S-04 |
| `AdapterId` | `^[a-z0-9-]+$` | descriptor `adapters/<AdapterId>.md` en la `AuthorityRevision`. La ruta **se deriva del id por patrón fijo**, nunca del contenido | inexistente → P-14 |
| `AdapterFactsSchemaRef` | `{SchemaId, Path, Blob}` | `Path` = `schemas/adapters/<AdapterId>.facts.v<n>.schema.json`, derivado del id y de la versión declarada por el descriptor; `Blob` en la `AuthorityRevision`. **Sin URLs ni descargas** | blob distinto o ausente → P-14 |
| `ProtocolSet` | `rackcad-protocol/I61` \| `rackcad-protocol/I62` | versiones de protocolo, no marcas | — |

### B.3 Validación de `AdapterFacts`

1. **Composición:** el esquema del núcleo declara `AdapterFacts = {SchemaRef (estricto), Facts: {"type": "object"}}`. `Facts` es el **único** objeto abierto
   del núcleo: es la frontera explícita de metadatos del adapter. El esquema del adapter es estricto (`additionalProperties: false`) y define los campos propios
   del adapter.
2. **Fase 1:** `Test-Json` del artefacto contra el esquema del núcleo.
3. **Fase 2:** `Test-Json` de `Facts` contra `SchemaRef.Path`, con el blob comprobado en la `AuthorityRevision`.
4. `Facts.SchemaId` = `SchemaRef.SchemaId`.
5. **Registro:** las dos validaciones (versión de PowerShell, resultado). Sin él, A1' falla.
6. **Contraste independiente:** el aceptante compara los hechos que coinciden (estado de autenticación, versión del binario, huella) con los del Exit/Entry de
   16.4 tomados en la invocación. Una discrepancia es S-04.
7. **Fallos:** adapter desconocido, esquema ausente, blob distinto, versión incompatible, propiedad inesperada o requisito omitido → **P-14**.

**Adapter nuevo** (`test-null`, con un campo propio `probe`): descriptor y esquema nuevos con `Facts` no vacío; valida sin tocar el núcleo (C-08).

### B.4 `rackcad-preflight/v1`

| Campo | Tipo y semántica |
|---|---|
| `Schema`, `PreflightId`, `UnitId`, `Role`, `ProtocolSet` | — |
| `Profile` | `{ProfileId, RoutingBlob}`: el perfil existe **aunque no haya binding** |
| `Host` | `HostRef` |
| `ObservedUtc` | instante |
| `Actor`, `Session` | `ActorRef` y `SessionRef`. Para una celda candidata sin sesión: `Assurance` = NONE e `InstanceId` = `"NOT_STARTED"` (estado, no `null`) |
| `Adapter` | `{AdapterId, AdapterVersion, BinaryPathHash, DescriptorRef}` |
| `AdapterFacts` | B.3 |
| `Fingerprint` | `{Kind, Sha256, KeyNames[]}` o `{Kind: "none", AcceptanceDecisionRef}` |
| `Requirements[]` | `{RequirementId, Mandatory (bool), Required (valor), Observation {State (OBSERVED \| NOT_OBSERVED), Value, Source, Assurance (REQUESTED \| CONFIGURED \| RUNTIME_OBSERVED \| SERVICE_ATTESTED \| NONE), ObservedUtc}, Status (MATCH \| ABOVE_REQUIRED \| BELOW_REQUIRED \| UNKNOWN), Contradiction (bool), ContradictionEvidence}`. `Value`, `Source`, `ObservedUtc` y `ContradictionEvidence` son nullables = no aplica solo si `State` = NOT_OBSERVED o `Contradiction` = false. Único por `RequirementId`; un obligatorio del perfil ausente → se añade con NOT_OBSERVED y UNKNOWN |
| `ConfigurationStatus` | agregado de §4.2 |
| `Causes[]` | ids de los obligatorios distintos de MATCH + contradicciones |
| `Disposition` | `ELIGIBLE` \| `NOT_ELIGIBLE` \| `STOP` |
| `StopConditions[]` | — |
| `Invalidators` | `{HostInstanceHash, AdapterVersion, BinaryPathHash, AuthState, FingerprintSha256, CatalogEntryBlob, RoutingBlob}` |

### B.5 `rackcad-binding/v1`

| Campo | Notas |
|---|---|
| `Schema`, `BindingId`, `UnitId`, `Scope`, `TaskId`, `Role`, `ProtocolSet` | `TaskId` nullable = no aplica solo con `Scope` = UNIT |
| `Actor` / `Session` | `ActorRef` / `SessionRef`, de la observación |
| `Cell` | `{CellId, CatalogBlob, AdapterId, Model, EffortSemantic, EffortProvider}` (`Model` y `EffortProvider` nullables = no aplica si el adapter no los controla) |
| `PreflightRef` | — |
| `Eligibility` | `{MeasuredInvocation {State (MEASURED \| NOT_MEASURED), RunRef}, ConsumptionCovered (OFFICIAL \| MEASURED \| UNKNOWN), CatalogVerifiedOn, Stale}`. Elegible solo con MEASURED, cobertura distinta de UNKNOWN y `Stale` = false |
| `Independence` | `{Requirements[] {ReferenceRole, ReferenceActor (ActorRef), Actor, Session, Context, Provider}, Satisfaction[] {ReferenceRole, Dimension, Result (SATISFIED \| NOT_SATISFIED \| UNKNOWN), Evidence}}`, único por (`ReferenceRole`, `Dimension`) |
| `RejectedAlternatives[]`, `RoutingReason`, `EscalationConditions` | — |
| `CountersSnapshot` | copia de §9.3 |
| `Custody` | `{FromBindingId, CessionRunId}` (nullables = no aplica en el primer binding) |
| `Acceptance` | `{State (PENDING \| ACCEPTED \| REJECTED), DecisionRef, Utc}` (`DecisionRef` y `Utc` nullables = no aplica solo con PENDING) |

### B.6 Correlaciones de `worker-handoff/v1` en `I62`

`worker-handoff/v1` no cambia. Sus claves bastan para correlacionarlo con los artefactos `I62`:

| Campo del handoff | Correlación |
|---|---|
| `TaskId`, `Attempt`, `DelegationRunId` | → `delegation/v2` |
| `RunId` | → relevo de trabajo |
| SHAs, rama y worktree | → `Identity` |
| `Worker.Provider` | → proveedor del descriptor del binding |
| `ModelRequested` / `EffortRequested` | → la delegación |
| `Trailer` | → el binding |

El efectivo, en `relay-record/v2.Participant.Observation`.

### B.7 Deltas de los esquemas que evolucionan

| Artefacto | Conservado | Sustituido | Añadido |
|---|---|---|---|
| `relay-record/v2` | `TaskId`, `RunId`, `Attempt`, `Phase`, `Cession`, `Outcome`, `Disposition`, `Notes`, `RemoteFacts` | `Participant.Kind` → `{Role, BindingRef, Actor, Session, AdapterId, AdapterVersion, Transport (session-internal \| external-process), Observation[]}`; `ConfigToml*` → `Fingerprint`; `RebaseMap.G2CloseSha` → `MaterializationCloseSha` | `PrevRelaySha256` (diario encadenado; nullable = no aplica solo en el primero del intento); `Host`; `TestArtifacts[].Skipped`; `Outcome.Kind` añade `LAUNCH_UNCERTAIN` |
| `controller-verification/v2` | todo | — | `Verifier {Role, BindingRef, Actor}` |
| `gate-contract/v2` | todo | `EligibleCells` → `RoleRequirements[]` (único por `Role`; `{Role, Profile, Mandatory[] (RequirementId), Independence[] (ReferenceRole + 4 dimensiones), EligibleCells[]}`) | `ProtocolSet`; `MaterializationCloseSha` |
| `delegation/v2` | todo salvo `Owner.Kind` | `Owner.Kind` → `session-internal` \| `external-process` | `Executor.BindingRef`; `RoleRequirements[]` (copia, **nunca** menor) |

## Anexo C — Matriz única: invariante → obligación → gate → entrada disponible → resultado → clase y autoridad

En cada gate, **entrada disponible** dice qué existe: datos de diseño (anexos de esta Proposal), materialización ejecutable (esquemas y procedimientos
publicados en ese gate) o el procedimiento ya disponible. Ninguna obligación presupone un gate futuro. Esperado independiente = la Proposal acordada (Freeze),
las reglas de I-61, o el oráculo del Coordinator (Anexo D). Nunca la salida del procedimiento bajo prueba.

| Id | Invariante | Obligación | Gate | Entrada disponible | Resultado esperado | Clase / autoridad |
|---|---|---|---|---|---|---|
| C-01 | roles sin proveedor | guarda: la tabla de roles de §16.1 y los enums del núcleo sin marcas (proveedores de los descriptores + patrones) | F1 | §16.1 materializado | 0 coincidencias; RED antes de materializar la tabla; mutation «Codex» detectada | (i) Core RG + mutation — AGENTS |
| C-02 | idem en el ADR | revisión del texto del ADR propuesto contra §2 | F1 | ADR formal propuesto | sin asociación rol↔proveedor | (ii) revisión del Coordinator |
| C-03 | tabla de estados fiel | guarda: la tabla normativa de §16 y la de ejemplos del README iguales, fila a fila, a §4.2 (Freeze) | F1 | §16 y README materializados | 0 diferencias; RED antes de materializar; mutation detectada | (i) Core RG + mutation (control de **fidelidad**, no de conducta) |
| C-04 | agregación correcta | aplicar el procedimiento de agregación a E1-E11 como **datos de diseño** (forma del Anexo B.4, **sin** validación de esquema) | F1 | procedimiento del README; datos de B.4 | `ConfigurationStatus`, `Causes`, `Disposition` = §4.2 | (ii) MC |
| C-05 | esquemas neutrales y estrictos | guarda: enums de `preflight/v1`, `relay-record/v2` y `controller-verification/v2` sin marcas; estrictos salvo `Facts`; `SchemaRef` obligatorio | F2 | esquemas F2 | 0 violaciones; RED/mutation | (i) Core RG + mutation |
| C-06 | preflight real | ejecutar el procedimiento por cada adapter en el host | F2 | esquemas y procedimiento F2 | registros válidos por `Test-Json`, con el **estado observado en el momento** | (ii) MC (+ OV-I62-02) |
| C-07 | invalidadores | cambiar un invalidador simulado (versión, huella) frente a un registro previo | F2 | ídem | requisitos → UNKNOWN; un binding hipotético **no** invalida | (ii) MC |
| C-08 | hechos validados sin tocar el núcleo | B.3: (a) adapter `test-null` con `Facts` no vacío; (b) propiedad inesperada; (c) id desconocido; (d) versión incompatible; (e) huella «ninguna» sin aceptación; (f) contraste con un Exit/Entry discrepante | F2 | esquemas F2 | (a) válido sin cambiar el núcleo; (b)-(e) P-14; (f) S-04 | (ii) MC |
| C-09 | descriptores completos | guarda: cada descriptor declara las 9 operaciones | F2 | descriptores F2 | 0 faltantes; RED/mutation | (i) Core RG + mutation |
| C-10 | sin secretos | valores **ficticios** sensibles en `Value`, `KeyNames`, `Evidence`, `Notes` y mensajes de error → saneamiento previo a la custodia (lista blanca + patrones) | F2 | procedimiento F2 | 100 % detectados o redactados; custodia rechazada | (ii) MC + (i) guarda sin campos de credencial |
| C-11 | sin binding con obligatorio no acreditado | bindings con: obligatorio UNKNOWN; `MeasuredInvocation` NOT_MEASURED; cobertura UNKNOWN; requisito omitido (E11) | F3 | esquemas F3 | REJECTED / P-10 | (ii) MC |
| C-12 | referencias | `BindingRef` de otra unidad, de otra tarea, de otra revisión u obsoleto; TRANSIENT presentado como custodiado | F3 | ídem | rechazo A2' | (ii) MC |
| C-13 | independencia | (1) dos requisitos simultáneos; (2) mismo proveedor con contexto separado; (3) distinto proveedor con contexto compartido; (4) misma instancia de actor con dos `BindingId` en Worker y verificador; (5) PREFERRED frente a REQUIRED no satisfechos | F3 | ídem | (1) máximo; (2) Contexto sí, Proveedor no; (3) Proveedor sí, Contexto no; (4) rechazo; (5) registro frente a bloqueo | (ii) MC |
| C-14 | delta de fallos fiel | casos de cierre del Anexo G.2 sobre registros sintéticos con la regla de 16.9/16.8 | F3 | Anexo G + esquemas F3 | esperados exactos de G.2 | (ii) MC |
| C-15 | custodia y CAS | repositorio Git **local desechable**: secuencia normal Q0→Q7 con SHAs reales locales; caídas antes y después de cada publicación; dos recuperaciones; push rechazado por causas distintas | F4 | state/v2 y procedimiento F4 | igual al Anexo F | (ii) MC (sin invocar modelos) |
| C-16 | contadores | correcciones lanzadas frente a verificaciones; lanzamiento incierto; `ContinuesTaskId`; contador ausente o contradictorio | F4 | ídem | §9.3; S-04 cuando corresponde | (ii) MC |
| C-17 | procesos | procesos locales ficticios (positivo: `pwsh` en la ruta; negativo: efímero) con el README §3.2 ampliado | F4 | procedimiento F4 | positivo detectado; efímero no vivo tras relectura | (ii) MC |
| C-18 | state/v2 bien formado | guarda Core de forma sobre `state/v2` | F4 | formato F4 | RED/mutation | (i) Core RG + mutation |
| C-19 | `/v1` intacto | guarda: blobs de los cinco `/v1` fijados; las pruebas de I-61 sin modificar | F2 | `/v1` | igualdad | (i) Core G |
| C-20 | adopción y lectura legacy | trazas del Anexo E.3 sobre el mapa materializado: unidad previa con una cadena nueva, unidad nueva, unidad sin metadata, unidad desconocida; contrato I62 en una unidad I61 | F4 | mapa y reglas F4 | E.3; P-15 | (ii) MC |
| C-21 | MaterializationClose | cambio normativo posterior a MC_I62 → invalidación | F4 | regla F4 | MC_I62 nuevo requerido | (ii) revisión del Coordinator |
| C-22 | fixture arrancable | traza D.1 hasta un contrato I62 válido | F6 | fixture | D.1 | (ii) FX |
| C-23 | autoverificación (FX-01) | D.3 | F6 | fixture | D.3 | (ii) FX + OV |
| C-24 | topología A (FX-02) | D.3 | F6 | fixture + OD | D.3 | (ii) FX + OV |
| C-25 | portabilidad (FX-04) | D.3, D.6 | F6 | ídem | D.3 | (ii) FX + OV |
| C-26 | topología B (FX-03) | D.3 | F6 | ídem | D.3 o limitación | (ii) FX + OV |
| C-27 | plano (c) sin efecto real (FX-05) | D.3 | F6 | ídem | rechazo; estado real intacto | (ii) FX |

## Anexo D — Pilotos y portabilidad (plano c)

### D.1 Arranque del fixture (análisis; nada se crea ahora)

Lo crea la sesión de supervisión (plano a) al empezar F6, con OD-5 y OD-7:
1. `fixture-origin.git` (remoto bare local) y, con OD-7, un remoto en GitHub del Owner con CI.
2. `main` del fixture, commit **F_seed**:
   - copias **byte a byte** de las autoridades de RackCad en MC_I62 (WORKFLOW, LIFECYCLE, AUTOMATION_PLAN, agent-execution, esquemas);
   - un **AGENTS del fixture** marcado `FIXTURE_LOCAL`, que declara los **jobs requeridos del fixture** (`fixture-build`, `fixture-tests`);
   - una biblioteca .NET mínima y su CI;
   - `FIXTURE-MANIFEST.json`: ruta → {`COPIED` con blob de origen en MC_I62 | `FIXTURE_LOCAL` con su razón} + commit de origen.
3. Rama `fixture/i62-norm` con el commit **F_norm**, que lleva el trailer `Agent-Protocol-Normative: I-62`, fusionada en el `main` del fixture como
   **F_eff**. Por la regla de §14, `I62_EFFECTIVE_SHA` del **fixture** = F_eff, marcado `TEST-ACTIVATION` en el manifiesto. La derivación se aplica **por
   repositorio**: en RackCad no hay activación hasta la integración real, así que el plano (a) no puede seleccionar I62.
4. Unidad de prueba **FX-U1**: reclamo en `fx/u1` desde F_eff (Claim-Id propio) y push al remoto del fixture → clase POSTERIOR_DEMOSTRADA en el fixture →
   protocolo I62.
5. Bootstrap de FX-U1: `state/v2` con `protocol: I62` y `custody`; contrato. MaterializationClose = bootstrap (sin cambios normativos).
6. `gate-contract/v2` de la tarea T: `ProtocolSet` I62 = protocolo de la unidad; `AuthorityRevision` = bootstrap de FX-U1; `EXTERNAL` = `main` del fixture
   (F_eff). **Contrato válido** (A1'/A6').

### D.2 Semántica de `Ci` (se resuelve antes del Freeze)

| Regla `/v1` (16.9) | Regla I62 | Tipo de cambio |
|---|---|---|
| «Corrida `push` de `CurrentSha` con `ref` exacta y los **cuatro jobs requeridos de `AGENTS.md`** en `success»; RED con el job Core en `failure` | «… y **los jobs requeridos por el `AGENTS.md` del repositorio de la unidad** en `success`; RED con el job de pruebas designado por ese `AGENTS.md` en `failure`» | **sustituido**: en RackCad significa exactamente lo mismo (cuatro jobs; Core); en el fixture, sus dos jobs. Requiere aprobación en el Freeze (Coordinator + Architect) |

- **Positivo:** push de G en `fx/u1` → corrida `push`, `refs/heads/fx/u1`, `head_sha` G, `fixture-build` y `fixture-tests` en `success` → `Ci` pass.
- **Negativo:** `fixture-tests` en `failure` en la corrida de G → `Ci` fail → REWORK tras leer los logs.
- **RED:** la corrida de R con `fixture-tests` en `failure` → `RedPart` pass.

La evidencia del fixture **nunca** sustituye las cuatro corridas de RackCad.

**Sin OD-7:** `Ci` = `not_run` → ninguna verificación VERIFIED → FX-02 no puede ser PASS → **F6 no se cierra**: queda pendiente con UNVERIFIED y vuelve al
Owner. Aceptar una limitación no convierte en PASS una verificación incompleta.

**Matriz de evidencia:**

| Evidencia | Fuente | Qué acredita |
|---|---|---|
| real | CI de RackCad sobre los SHAs de I-62 | los gates reales |
| del ensayo | CI del fixture sobre los SHAs del fixture | la conducta del protocolo en el plano (c) |
| limitada | escenarios UNVERIFIED o UNSUPPORTED con causa | nada más que su límite |

### D.3 Hoja de invocaciones y escenarios

**Topología A** (FX-01, FX-02, FX-05; FX-04 A→B, que comparte el arranque):

| Rol (adapter) | Fase | Lanzamientos base | Controles que usa | Reintentos de transporte | Tope de la ronda |
|---|---|---|---|---|---|
| Principal A (`claude-desktop-session`, sesión del sistema bajo prueba abierta por el Owner) | toda la ronda | 1 sesión | FX-01 (3 preflights **dentro** de esta sesión, por procedimiento; el Owner cambia el effort; cada observación por `get_session` ligada al instante; **se cuentan como turnos de la sesión, no como invocaciones nuevas**) | +1 reapertura | 2 sesiones |
| Controller (`codex-cli`) | planificación | 1 | — | pool | — |
| Controller | verificación | 1 | — | pool | — |
| Architect (`codex-cli`, invocación propia) | revisión del contrato | 1 | — | pool | — |
| Controller | negativos que exigen invocación | 8: nc1 (Identity; cubre «SHA cambiado»), nc2 (Scope), nc3 (FreeText), N4 handoff ausente → BLOCKED, N5 0 pruebas → REWORK, N6 trabajo sin commit → CleanTree REWORK, N7a otra corrida con Identity pass → REWORK, N7b otra corrida con Identity fail → BLOCKED/STOP | — | pool | — |
| **Codex, total** | | **11** | | pool **4** (≤ 2 por fase) | **15** |
| Sondas previas (`codex-cli`, binario actual) | preparación | 1 | — | +1 | 2 (fuera de la ronda; con OD-2) |
| Worker (`claude-subagent`) | trabajo | 1 | — | +1 solo si hay REWORK (corrección, cuenta en `attempts`) | 2 |
| Reviewer (`claude-subagent` nuevo) | revisión | 1 | — | +1 | 2 |
| Negativos de sesión (sin invocación) | aceptación y estado | nc4, N8 credencial → P-10, N9 TERMINATION_UNACCREDITED → P-12, N10 contadores tras cambio de proveedor → sin reinicio | — | — | 0 |
| FX-05 | — | 0 | — | — | 0 |

**FX-04 A→B** (integrado en FX-02, en el punto quiescente **Q0**):
1. A, en su sesión: preflight del Principal → binding del Controller de planificación aceptado → **Q0** (CAS; `open_delegation` planificada, `TaskId` T,
   `Attempt` 0; contadores 0) → push.
2. **A se detiene.** La sesión de supervisión (plano a) observa `isRunning = false` de A en los metadatos de la app (operación 7 por observador externo; si F2
   no la acredita → FX-04 **UNVERIFIED**, sin toma de posesión).
3. El Coordinator prepara el **oráculo fuera del host** desde el estado canónico del fixture en Q0 y publica solo su SHA-256 en la evidencia real. El oráculo
   contiene:
   - los hechos: base Q0, rama, Claim-Id, custodia, delegación planificada, `attempts` = 0, contadores = 0, STOP vigentes;
   - la siguiente decisión: «invocar CONTROLLER_PLANNING para `TaskId` T, `Attempt` 0, con el binding de Controller vigente si es elegible desde B (si no,
     rebinding), `ProtocolSet` I62, sin Worker todavía».
4. El Coordinator decide la transferencia del Principal (T16).
5. B (`codex-desktop-session`, abierta por el Owner, con las entradas permitidas de D.6) ejecuta su preflight. Si B queda en BELOW_REQUIRED o UNKNOWN →
   STOP/UNVERIFIED.
6. B publica **QR**: transferencia A→B por CAS, más su respuesta (hechos + siguiente decisión) en el fixture. Push **antes** de recibir el oráculo.
7. El Coordinator entrega el oráculo y se hace la comparación mecánica.
8. **B ejecuta el resto del minigate:**
   - planificación;
   - aceptación;
   - Worker: rebinding a un adapter que B puede lanzar, `codex-cli` con escritura (OD-4); B no puede lanzar subagentes de Claude;
   - verificación del **G'** producido bajo B → VERIFIED sobre G' → Q7.

| Rol en la continuación de B | Lanzamientos | Reintentos | Tope |
|---|---|---|---|
| Principal B | 1 sesión | +1 | 2 |
| Controller (`codex-cli`) | planificación 1 + verificación 1 | pool 2 | 4 |
| Worker (`codex-cli`, escritura) | 1 | +1 si hay REWORK | 2 |
| N11 hecho retirado: segunda sesión B2 sobre un fixture en Q0 con un hecho retirado del estado canónico | 1 sesión | 0 | 1 |

N11 esperado: B2 declara UNKNOWN o STOP sobre ese hecho; nunca el valor oculto.

**Topología B (FX-03):**

| Rol | Lanzamientos | Pool | Tope |
|---|---|---|---|
| Principal (`codex-desktop-session`) | 1 sesión | +1 | 2 |
| Controller (`codex-cli`): planificación, verificación, nc1-nc3 (los demás negativos del Controller se cubren en A, mismo adapter) | 5 | 3 | 8 |
| Worker (`codex-cli`, escritura) | 1 | +1 si hay REWORK | 2 |
| Reviewer (`claude-cli`) | 1 | +1 | 2 |
| Architect (`claude-cli`, invocación distinta) | 1 | +1 | 2 |
| Sondas previas | `claude-cli` autenticado 1 (+1); escritura de `codex-cli` 1 (+1) | — | 4 (fuera de la ronda) |
| Negativos de sesión N8-N10 | 0 | — | 0 |

**Totales por ronda de F6** (P-07 se comprueba **antes** de cada lanzamiento):

| Ronda | Codex | Subagentes Claude | Claude CLI | Sesiones de Principal |
|---|---|---|---|---|
| A | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |
| A→B (continuación) | ≤ 4 + Worker 2 = 6 | 0 | 0 | ≤ 2 (+1 B2) |
| B | ≤ 8 + Worker 2 + sonda 2 = 12 | 0 | ≤ 4 + sonda 2 | ≤ 2 |

Ningún tope autoriza gasto ni ejecución ahora.

### D.4 Resultados y cobertura

| Resultado | Cuándo |
|---|---|
| **PASS** | ejecutado y conforme al oráculo |
| **FAIL** | violación observada; se conserva; impide cerrar F6 hasta la corrección (§16) y la repetición dentro de los topes; nunca se convierte en limitación |
| **UNVERIFIED** | falta una precondición (OD, capacidad sin medir, registro incompleto) |
| **UNSUPPORTED** | capacidad medida ausente |

**Dependencias de la cobertura mínima para cerrar F6:**

| Escenario | Necesita | Si falta |
|---|---|---|
| FX-01 PASS | OD-5 + sesión A | UNVERIFIED; F6 pendiente |
| FX-02 PASS | OD-5 + **OD-7** + OD-2 + medición del binario | UNVERIFIED; F6 pendiente |
| FX-04 PASS (al menos A→B) | lo de FX-02 + terminación de A observable + Principal B (introspección acreditada) + OD-4 | UNVERIFIED; F6 pendiente |
| FX-05 PASS | OD-5 | ídem |
| FX-03 | OD-3, OD-4, OD-2, Principal B | PASS, o UNVERIFIED/UNSUPPORTED con causa para la decisión del Owner (OV-I62-04); el escenario no se retira |

**Límites separados por topología, rol y escenario:**
- una limitación de `claude-cli` afecta al Reviewer y al Architect de B;
- no demuestra que el Principal B esté indisponible ni acredita la portabilidad.

### D.5 Ejecución compacta para OV sobre FINAL_CANDIDATE_SHA

Se resiembra el fixture desde el Candidato y se ejecutan:
- FX-01;
- FX-02, pasos hasta VERIFIED: Codex planificación 1 + verificación 1 + nc1 1 + pool 2 = 5; Worker 1 (+1); Principal 1;
- FX-04 en un sentido: Principal B 1; Codex 2 + pool 1; Worker 1.

FX-03 solo con las OD concedidas.

### D.6 Entradas y aislamiento (cobertura declarada)

| Clase de entrada | Contenido | Tratamiento |
|---|---|---|
| **Canónicas permitidas** | clon del fixture en el SHA del punto de relevo; documentos del fixture | enumeradas en la respuesta de B |
| **Dependencias técnicas permitidas** | binarios y runtime; su configuración y autenticación propias (para el preflight); SDK, Git y GitHub CLI; **entradas automáticas del runtime** (p. ej., instrucciones globales de usuario, `AGENTS.md`/`CLAUDE.md` del directorio, memoria del proyecto) | enumeradas con su ruta y su **SHA-256** antes de que B empiece; ninguna puede contener hechos de la unidad. La memoria de proyecto de un directorio nuevo se comprueba vacía |
| **Privadas prohibidas** | transcripciones y sesiones de otras sesiones (`~/.claude/projects/*` de otros directorios, `~/.codex/sessions/*` ajenas), worktree real de I-62, `D:\IDs`, artefactos transitorios de A, memoria de otras sesiones | — |

**Auditoría:** comandos del registro de sesión de Codex y llamadas a herramientas de la transcripción de Claude. **Cobertura declarada:** el registro no
garantiza capturar lecturas internas del runtime.

| Hallazgo | Resultado |
|---|---|
| lectura registrada de una entrada prohibida | **FAIL** de aislamiento; ensayo inválido |
| entrada automática no enumerada, o registro sin lecturas | aislamiento **UNVERIFIED**; FX-04 no puede ser PASS |
| registro parcial | no es prueba completa de aislamiento |

El oráculo, fuera del host antes de QR, protege el esperado; **no** prueba la ausencia de memoria heredada. Eso lo cubren la enumeración de entradas
automáticas y la auditoría, con su límite.

### D.7 FX-05 y decisiones por runtime

**FX-05.**
- **Legítimo:** el Coordinator del fixture declara un gate **dentro del fixture**, para FX-U1, marcado como del sistema bajo prueba.
- **Violación:** cualquier artefacto o acción que nombre una unidad real (I-62 u otra), configure o use el remoto de RackCad, o modifique el estado,
  contadores, decisiones o custodia reales.
- **Detección:** comparación del estado real (refs remotas, `state/I-62.yml`, decisiones) antes y después, y búsqueda de identificadores reales en los
  artefactos del fixture.
- **Esperado:** rechazo (P-16) y estado real intacto.

**Decisiones del Owner por runtime:**

| Runtime | OD-2 | OD-3 | OD-4 | OD-5 | OD-7 |
|---|---|---|---|---|---|
| `codex-cli` | sí | — | escritura (B, A→B) | sí | `Ci` |
| `codex-desktop-session` | si comparte `config.toml` (F2) | por medir | — | sí | — |
| `claude-cli` | según su huella | sí | — | sí | — |
| `claude-desktop-session` / `claude-subagent` | según su huella | — | — | sí | — |

## Anexo E — Compatibilidad legacy por superficie y cláusula

### E.1 Mapa

«^1» = versión en `I62_EFFECTIVE_SHA^1`; «main» = versión actual de `main`. Una unidad I61 lee ^1 **solo** en las cláusulas que I-62 modifica; el resto, en
main.

| Superficie / cláusula | Unidad I61 lee | Unidad I62 lee | Vigencia | Lectores | Evidencia de resolución |
|---|---|---|---|---|---|
| AUTOMATION_PLAN §16 preámbulo, 16.1, 16.3-16.9, 16.11 (filas modificadas), 16.12 | ^1 | main | protocolo de la unidad | Coordinator, Controller, sesión | mapa de cláusulas en `integration/I-62`; `Authorities` del contrato con revisión por cláusula; comprobación `Authority` |
| §16.10 (términos de gate) | main (sin cambio) | main | — | Controller | — |
| §16, subsecciones nuevas (autoverificación, observación, adapters, independencia, custodia, adopción) | **no aplica** | main | solo I62 | ídem | — |
| AUTOMATION_PLAN §8 (formato del estado) | ^1 (`/v1`) | main (`/v2`) | protocolo | sesión, Core | ídem |
| `routing.md` §§1-3 (clase nueva añadida; filas existentes sin cambio) | main (las filas I-61 no cambian; la clase nueva no le aplica) | main | — | Controller, sesión | guarda: filas de I-61 sin cambio |
| `routing.md` §§4-5 (binding de todos los roles) | ^1 | main | protocolo | ídem | mapa |
| `routing.md` §7 (receta del Controller) | ^1 | adapter `codex-cli` | protocolo | ídem | mapa |
| `model-catalog.md` | main, con la **obligación de compatibilidad**: cada entrada conserva las claves de I-61 (OBL-05) y su «Estado local» por celda | main | — | sesión, Controller | guarda OBL-05 vigente |
| README de agent-execution (§§1, 3, 5, 6, 11) | ^1 | main | protocolo | sesión | mapa |
| `adapters/*`, `schemas/adapters/*`, `preflight`/`binding`, esquemas `/v2` | no aplica | main | solo I62 | — | — |
| esquemas `/v1` | main (intactos) | `worker-handoff/v1` | — | todos | guarda de blobs |
| PROMPT_TEMPLATES §G (si cambia) | ^1 | main | protocolo | sesión | mapa |
| PROMPT_TEMPLATES §2 (bloque factual) | main (texto de traza; sin efecto en lectores) | main | — | — | — |
| WORKFLOW §4 (referencia «al abrir») | ^1 (la referencia no le aplica) | main | solo I62 | sesión | mapa |
| WORKFLOW §10 (fila) | ^1 | main | protocolo | todos | mapa |
| LIFECYCLE §5/§9 (solo con OD-6 alternativa 1) | ^1 | main | solo I62 | Coordinator, Architect | mapa |
| ADR-0046 | completo | ADR-0046 en lo no superado + ADR sucesor | protocolo | todos | índice ADR (cierre) |
| FOUNDATIONS (entrada) | descriptiva; no es autoridad | ídem | — | — | — |

### E.2 Obligaciones

- La integración de I-62 publica el **mapa exacto de cláusulas modificadas** (ruta + encabezado de sección), como WORKFLOW §11.3.
- Ningún archivo se congela entero.
- Cada contrato (`gate-contract/v1` o `/v2`) cita sus autoridades con la revisión que corresponde a su protocolo.

### E.3 Trazas de cierre (análisis, no ensayo)

1. **Unidad anterior** (p. ej., I-64, en PRE y con estado `/v1`) abre una cadena después del merge:
   - clasificación ANTERIOR por PRE → protocolo I61;
   - su `gate-contract/v1` cita §16.3-16.9 en ^1, §16.10 en main, §8 en ^1, `routing.md` §§4-5 y §7 en ^1, el catálogo en main (claves de I-61 presentes),
     README en ^1 y `/v1` en main;
   - `Authority` comprueba cada cita contra el mapa → **todas sus autoridades son compatibles**; ninguna superficie I62 interviene;
   - un contrato `/v2` en esa unidad → P-15.
2. **Unidad nueva** (reclamada después, base con el efectivo) → POSTERIOR → I62: `state/v2` con `protocol: I62`; lee todo en main.
3. **Unidad sin metadata antigua** (estado `/v1` sin `protocol`) → se resuelve por PRE (durable en I-62), sin editar su rama.
4. **Unidad con orden o base desconocidos** (solo en POST sin prueba de orden) → DESCONOCIDA → STOP de contratos y delegaciones; se pide la evidencia concreta
   (registro del primer push, contraste de corridas); sin default.

## Anexo F — Secuencias con SHAs simbólicos (análisis, no ensayo)

**Notación:**
- `M`: `origin/main`;
- `Qk`: commits de estado de la sesión;
- `R`, `G`: commits del Worker;
- `T`: directorio transitorio del intento;
- `r1…rn`: `relay-record/v2` encadenados por `PrevRelaySha256`.

### F.1 Cadena normal (llega a verificar G)

| Paso | Actor | Git (rama) | Transitorio (T) | Comprobación |
|---|---|---|---|---|
| 0 | P | `HEAD` = `origin` = `Qp` (anterior) | — | autoverificación (preflight P) |
| 1 | P | commit **Q0** (planificada T; binding C aceptado; `rv` = n+1) → push (CAS) | — | `BaseSha` := Q0 |
| 2 | P→C | sin escritura | r1 (Exit, cesión, Outcome, Entry); delegación d | Exit/Entry 16.4 |
| 3 | P, Coordinator | sin escritura | binding W, preflight W, aceptación A1'-A8', nc4 | — |
| 4 | P→W | W: R → push; G → push | r2; handoff h | Entry: `HEAD` = `origin` = **G**; terminación de W acreditada; `Outcome` COMPLETED |
| 5 | P | sin escritura | `RemoteFacts` (CI de R y de G) | `Ci`, `RedPart` |
| 6 | P→C | sin escritura | r3; verificación v = **VERIFIED, `VerifiedSha` = G** | `Identity`: `HEAD` = `origin` = `CurrentSha` = G ✓ |
| 7 | P→C | sin escritura | r4-r6 (nc1-nc3) | oráculo relativo |
| 8 | P | commit **Q7** (custodia de T; `open_delegation` = null; `last_verified_sha` = G; `rv` = n+2) → push (CAS) | — | SHA evaluado = Q7 = G + documentos (16.1) |

### F.2 Caídas antes y después de cada publicación

| Momento | Estado remoto | Recuperación |
|---|---|---|
| antes del push de Q0 (Q0 local) | `Qp` | mismo host: T10(b) → publicar Q0. Otro host: Q0 inaccesible → P sin terminación acreditada → T11 (STOP); con terminación acreditada → T12 (QR desde `Qp`) y se registra un posible trabajo local inaccesible (T14) |
| después de Q0, antes de 2 | Q0 (planificada) | tras T12: siguiente acción = planificación; contadores sin cambio; 0 lanzamientos |
| durante 2 (C vivo) | Q0 | confirmar la terminación de C (operación 7) antes de lanzar nada (P-02). Host inaccesible → P-13 |
| después de 2, antes de 4 | Q0 | mismo host: seguir con T. Otro host: d perdido → **conteo de lanzamientos desconocido** → STOP. El Coordinator fija un conteo conservador (lanzamientos registrados + inciertos); nunca cero |
| durante 4, después de R | R (sobre Q0) | terminación de W → handoff ausente → `Handoff` BLOCKED (T9); trabajo R preservado |
| después de G, antes del handoff | G | T9; el Controller puede verificar los commits verificables; sin VERIFIED si `Handoff` falla |
| después de 6, antes de Q7 | G | mismo host: Q7. Otro host: v perdido → reverificación como reejecución de fase (BLOCKED/fase +1), tras confirmar terminaciones |
| Q7 local sin push | G | T10(b) → publicar. Rechazo: inspeccionar la causa (§8) |
| push de Q7 rechazado por avance ajeno | G' ≠ G | T10(c) → STOP P-02/S-07; Q7 local conservado |

### F.3 Dos recuperaciones sin dos escritores

| Paso | Estado |
|---|---|
| inicio | P ausente → NO_OBSERVATION → T11 STOP |
| terminación | un observador acredita la terminación de P → ORPHAN_CONFIRMED |
| designación | el Coordinator designa a **N1** (decisión `Dk`) |
| CAS de N1 | N1 publica QR (`rv` = n+1, designado N1) → push aceptado |
| intento de N2 | N2 prepara su QR desde `rv` = n → push **rechazado** (no fast-forward) → `fetch`: ve a N1 designado por `Dk` → **N2 no opera**: no hay designación vigente para N2 |
| resultado | un solo escritor (N1) |

### F.4 Relevo A→B hasta verificar el SHA correcto (FX-04)

| Paso | Actor | Git (fixture) | Evidencia |
|---|---|---|---|
| 1 | A | **Q0** (planificada T; Controller aceptado; contadores 0) → push | preflight A |
| 2 | supervisión | — | terminación de A: `isRunning = false` (observador externo) |
| 3 | Coordinator | — | oráculo fuera del host; su SHA-256 en la evidencia real |
| 4 | Coordinator | — | decisión de transferencia (T16) |
| 5 | B | — | preflight B (MATCH requerido); entradas enumeradas |
| 6 | B | **QR** (Principal A→B por CAS; respuesta de B) → push | — |
| 7 | Coordinator | — | oráculo entregado; comparación |
| 8 | B→C | sin escritura | planificación; aceptación; rebinding del Worker a `codex-cli` |
| 9 | B→W | W: R' → push; G' → push | Entry: `HEAD` = `origin` = **G'** |
| 10 | B→C | sin escritura | verificación **VERIFIED, `VerifiedSha` = G'**; `Identity` G' ✓ |
| 11 | B | **Q7'** → push | — |

`BaseSha` de la delegación de B = QR.

**Contadores esperados:** `attempts` = 0; por clase 0; lanzamientos: A = 0 (solo binding); B = planificación 1 + Worker 1 + verificación 1.

## Anexo G — Cambios frente a `/v1` y casos de cierre

### G.1 Matriz

| Cláusula `/v1` | Tratamiento I62 | Tipo |
|---|---|---|
| S-12 (16.11): STOP | STOP con `analysis.md` y decisión del Coordinator; una reejecución decidida sin cambio de trabajo cuenta en BLOCKED/fase | **conservado** |
| `Routing` (16.9 #12) con `required`/`advisory` | `required`: efectivo ≠ solicitado → fail (BLOCKED). `advisory`: pass con la discrepancia en `Evidence`, `Findings` y `Deviations`. **Caso añadido:** efectivo no observado al nivel mínimo → `required`: fail (BLOCKED); `advisory`: pass con la limitación anotada | conservado + **ampliado** (caso nuevo declarado) |
| `Handoff` (16.9 #2): «inválida u otra corrida → REWORK si `Identity` pasa; si no, STOP» | igual | **conservado** |
| `Termination` (16.9 #1): registro COMPLETED; si fue un proceso, muerte confirmada | `Outcome` COMPLETED **y**, si es proceso o sesión, operación 7 acreditada. Un proceso muerto con FAILED_TURN → fail (BLOCKED de transporte) | conservado + **ampliado** (operación 7 para sesiones) |
| `Ci` (16.9 #9): cuatro jobs de AGENTS | jobs requeridos por el AGENTS del repositorio de la unidad (D.2) | **sustituido** (mismo significado en RackCad) |
| `Tests` (16.9 #10) | igual + `Skipped` coherente | **ampliado** |
| `Trailer` (16.9 #11) | coherente con el proveedor y el modelo del binding | **ampliado** |
| `Contract` (16.9 #4) / A3-A5 | + `RoleRequirements` (delegación ⊇ contrato por rol: `Mandatory` ⊇; cada dimensión de `Independence` ≥ la del contrato con REQUIRED > PREFERRED > NOT_REQUIRED) | **ampliado** |
| A1 | + conjunto I62 + B.3 | **ampliado** |
| A2 | + `BindingRef` (B.2) | **ampliado** |
| A6 | + `ProtocolSet` = protocolo de la unidad | **ampliado** |
| A7 | elegibilidad vía binding aceptado + observación vigente | **sustituido** (misma regla de ADR-0046 #4) |
| A8 | + `CountersSnapshot` = autoridad | **ampliado** |
| `Identity`, `Remote`, `Scope`, `CleanTree`, `FreeText`, `Denials` | sin cambio (`Denials` incluye la pérdida de autenticación como S-06, como hoy) | **conservado** |
| 16.8 por clase: correcciones lanzadas | igual, registradas en el commit de 16.8 (§9.3) | conservado (autoridad explícita) |
| 16.8 tope de invocaciones | + registro de lanzamientos y LAUNCH_UNCERTAIN | **ampliado** |
| P-01 | conservado; P-11 lo generaliza a toda huella | conservado + ampliado |
| 16.4 orden de commits | **sin cambio**; la custodia usa Q0/Q7 y el diario | conservado |
| 16.9 precedencia y first failure | sin cambio | conservado |

### G.2 Casos de cierre (esperados exactos; análisis)

| Caso | Esperado |
|---|---|
| Misma entrega con dos verificaciones (la 1.ª BLOCKED/STOP S-04 resuelta sin cambio de trabajo; la 2.ª VERIFIED) | `attempts` sin cambio; por clase sin cambio; BLOCKED/verificación = 1; invocaciones +2 |
| Corrección lanzada sin entrega (el Worker cae) | `attempts` +1 y por clase(X) +1 en el commit de 16.8; después, `Handoff` BLOCKED; reejecución del trabajo = BLOCKED/WORK +1; por clase sin otro incremento |
| Pérdida de contexto del Controller | S-12 → STOP; `analysis.md`; decisión del Coordinator; si decide reejecutar sin cambio → BLOCKED/fase +1; `attempts` sin cambio |
| `Routing` con efectivo ≠ solicitado | `required` → `Routing` fail → `BLOCKED/BLOCKED`. `advisory` → pass con la discrepancia anotada; puede ser VERIFIED |
| Handoff de otra corrida, `Identity` pass | `FailureClass` = `Handoff`; `REWORK_REQUIRED/REWORK` |
| Handoff de otra corrida, `Identity` fail | `FailureClass` = `Handoff` (primera en fail); disposición STOP (STOP > REWORK) → `EXECUTION_BLOCKED/STOP` |
| Proceso terminado con turno fallido | `Outcome` FAILED_TURN → `Termination` fail → BLOCKED de transporte; sin veredicto de contenido |
