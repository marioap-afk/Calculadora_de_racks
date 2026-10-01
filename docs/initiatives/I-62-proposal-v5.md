# I-62 — Proposal V5: portabilidad del Principal y binding de roles independiente del proveedor

```text
Frozen: NO
Version: V5 (sustituye a V4 en la revisión; V1, V2, V3 y V4 se conservan sin cambios como versiones revisadas)
Unit: I-62   Workflow: V2 (T4)   Claim-Id: 5b661a17-8c18-4183-8554-3866059cba2b
Archetype: NEW ARCHITECTURE (Coordinator, C62-F0-09; el mandato conserva la etiqueta FOUNDATION EVOLUTION)
Base: origin/main 819955d6   Discovery: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65) y §21, base de diseño (C62-F0-08)
Reviews: Coordinator SEPARATE SESSION — V1, V2, V3 y V4 CHANGES REQUIRED (I-62-coordinator-review-v1.md, -v2.md, -v3.md y -v4.md)
Author: sesión principal responsable de I-62 (Claude), redacción directa (C62-F0-24)
Review: PENDIENTE — Coordinator y Architect (paquete: docs/initiatives/I-62-architect-package-v5.md; revisión del Architect no realizada)
Owner: OD-6 pendiente (ambas alternativas delimitadas en §11.4); las demás OD bloquean en su frontera real (§18)
IMPLEMENTATION AUTHORIZATION = NO   ·   I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED
```

**Fuentes:**
- Alcance: el mandato ([I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt)).
- Hechos: el [Discovery](I-62-discovery.md) R1 y su §21.
- Decisiones: [decisiones](../automation/decisions/I-62.md) §§7-14 (C62-G0-02, C62-F0-01..24).
- Revisiones: registros de [V1](I-62-coordinator-review-v1.md), [V2](I-62-coordinator-review-v2.md), [V3](I-62-coordinator-review-v3.md) y
  [V4](I-62-coordinator-review-v4.md).

**Precedencia:** las autoridades integradas mandan mientras I-62 no se integre.

**Diseño, no conducta:** no modifica el protocolo operativo, no publica esquemas ni cambia conducta. REFERENCE OVER REPETITION.

**Mapa de la corrección V4→V5:**

| Hallazgo | Capítulos de V5 |
|---|---|
| R62-V4-01 (resolución transparente para los contratos I61) | §14; Anexo E (E.3 sin citas obligatorias, lectura compuesta de «documento completo», `MainSha` de evaluación; E.6 con el contrato real sin cambios; E.7); Anexo G; obligación C-20c |
| R62-V4-02 (orden bootstrap/G0 ejecutable: **Modelo A**) | §8.1, §8.6 (nueva), §9.2 (T0); Anexo B.2 y B.5 (transición única de aceptación); B.8.1 (`protocol.basis`, `protocol.g0_acceptance`, `principal.acceptance`); B.8.4 (I-S01, I-S14..I-S16, I-P06, I-P09); E.2; D.1; obligaciones C-15 y C-18 |

**Cierres preservados sin cambio:**
- de V1: R62-V1-09 (identidad por recibo + commit/ruta/blob) y O62-V1-01 (capacidades por rol y acción);
- de C62-F0-21 y C62-F0-23:
  - exact-SHA y custodia: Q0 → diario transitorio → Q7; modelo Q0/Q7/QU/QH/QR;
  - intención de tarea separada de ACCEPTED_OPEN; especificación semántica de `state/v2` (salvo lo corregido aquí en el arranque);
  - FX-04a/FX-04b (R62-V3-03, cerrado por completo); RESUME_DECISION separado de CUSTODY;
  - S-12 conservado; `Routing` advisory; semántica de `Handoff`; conteo por correcciones lanzadas;
  - activación de prueba del fixture; delta semántico de `Ci`; momento del índice ADR; matriz de obligaciones;
  - identidad del actor independiente del `BindingId`.

## 0. Delta respecto de I-61 (resumen)

| Tema | I-61 integrado | I-62 propone |
|---|---|---|
| Roles | Controller (Codex), Worker (Claude o Codex), sesión responsable | cinco roles sin proveedor (D-01) |
| Proveedor del Controller | fijado (ADR-0046) | binding por capacidad acreditada (D-05); ADR sucesor parcial (Anexo A) |
| Sesión principal | sin perfil ni obligación | perfil, autoverificación y `CONFIGURATION_STATUS` (D-03, D-04) |
| Preflight | solo en el relevo, con `config.toml` | observación de capacidad por runtime + comprobaciones de relevo sin cambio (D-06) |
| Hechos de proveedor | en §16.4, README, `relay-record` | frontera de adapters con descriptor y esquema de hechos (D-07) |
| Independencia | parcial, aceptada | cuatro dimensiones sobre la identidad observable del actor (D-12) |
| Custodia | deducida del relevo | `state/v2` en puntos durables BOOTSTRAP/Q0/Q7/QU/QH/QR + diario transitorio encadenado; la delegación abierta conserva el significado de 16.6 y solo existe en el diario (D-08, Anexo B.8) |
| «Cierre de G2» | nombre de un gate de I-61 | MaterializationClose por unidad (D-15) |
| Versiones | cinco `/v1` | protocolo por unidad (I61/I62); resolver normativo independiente del protocolo (§16.13), **transparente para los contratos I61** (ningún contrato cambia), con mapa de cláusulas leído en `I62_EFFECTIVE_SHA` (D-18, Anexo E) |
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
| 9 | Reconstrucción sin memoria privada | D-08 (estado canónico suficiente), D-17 | F4, F6 | C-15, C-18 (reconstrucción desde el estado), **C-25a** (FX-04a) |
| 10 | Recuperación determinista | D-09 | F4 | C-15, C-16 |
| 11 | Sin secretos | D-06, D-07 | F2 | C-10 |
| 12 | Dos topologías o límites honestos | D-17 | F6 | C-24 (A), C-26 (B), **C-25b** (continuación A→B) |
| 13 | I-61 intacto | D-14, D-18 | todas | C-19, C-20a, C-20b, C-20c |
| 14 | No bloquear producto | D-18 | todas | C-20c (las unidades I61 siguen delegando con su protocolo y con sus contratos `/v1` **sin cambios**); DC-07 por hito |

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
| AUTOMATION_PLAN §16 | ejecución delegada (ADR-0046 #1) | roles, binding y aceptación, observación de capacidad, autoverificación del Principal, contrato de adapter, custodia operativa, recuperación, conteo y adopción por unidad, en subsecciones nuevas para unidades I62 | AUTOMATION_PLAN; **ampliación de dominio** (abajo) |
| AUTOMATION_PLAN §16.3 y §16.13 (nueva) | lectura de autoridades en `MainSha` | **§16.13**: resolver de compatibilidad **independiente del protocolo**, leído en `MainSha` por toda unidad (Anexo E). §16.3 conserva **literalmente** el texto de I-61 y añade una frase puntero; otra frase puntero abre §16 | AUTOMATION_PLAN + Owner (OD-1) |
| AUTOMATION_PLAN §8 | formato del estado | `rackcad-automation-state/v2` para unidades I62 (Anexo B.8); `/v1` sigue para I61 | AUTOMATION_PLAN |
| WORKFLOW §§3-4 | reclamo, worktree, apertura, relevo | solo referencias (paso «al abrir» → autoverificación de §16 para unidades I62) | WORKFLOW |
| WORKFLOW §10 | fila «Operación del ejecutor y ejecución delegada» | texto de la fila ampliado | WORKFLOW + Owner (OD-1) |
| `routing.md` / catálogo | selección por capacidades | clase y perfil de coordinación principal; binding de todos los roles; §7 al adapter; el contenido nuevo del catálogo va en secciones nuevas (E.7) | Freeze de I-61 §15 |
| PROMPT_TEMPLATES §G / adapters | representación | renderizado en el adapter; corrección del bloque factual de §2 (F1) | PROMPT_TEMPLATES |
| AGENTS | evidencia | sin cambios | — |

**Ampliación del dominio de §16:**
- **Fuente anterior:** ADR-0046 #1 y la fila de WORKFLOW §10.
- **Delta:** obligaciones de la sesión principal **fuera** de una delegación.
- **Autoridad:** Owner (OD-1). No se ejerce antes de la vigencia (D-15) y solo aplica a unidades I62 (§14). §16.13 es la excepción declarada: gobierna la
  lectura de autoridades de **toda** unidad desde `I62_EFFECTIVE_SHA`, sin cambiar la conducta de las unidades I61 más allá de la revisión en la que leen las
  cláusulas que I-62 modificó.

## 4. Perfil del Principal y estado de configuración

### 4.1 Perfil (D-03) y capacidades por rol y acción (O62-V1-01, preservado)

Clase «Coordinación principal», perfil PRINCIPAL_COORDINATION (Long-horizon, Frontera). Requisitos **obligatorios**, como capacidades **por acción**:

| Acción del Principal | Obligatorios |
|---|---|
| RESUME_DECISION: reconstruir el estado y proponer la siguiente decisión, sin tomar la custodia | nivel y effort; `remote-facts` (lectura); `introspection` ≥ RUNTIME_OBSERVED |
| CUSTODY: tomar o conservar la custodia (BOOTSTRAP, Q0, Q7, QU, QH, QR), relevar y operar | lo anterior + `repo-write` |
| evidencia local | lo anterior + `build-test`, solo para la acción que deba producir evidencia local |

El perfil y sus requisitos existen **aunque el binding del Principal no pueda aceptarse**: la autoverificación produce su preflight y su disposición sin
depender del binding (Anexo B.4). P-09 y P-10 se evalúan **por acción**: un Principal en BELOW_REQUIRED para CUSTODY no toma la custodia, pero puede producir una
respuesta RESUME_DECISION si esa acción está en MATCH o ABOVE_REQUIRED.

### 4.2 `CONFIGURATION_STATUS` y disposición (D-04)

Estados exactos: **MATCH**, **ABOVE_REQUIRED**, **BELOW_REQUIRED**, **UNKNOWN**. Agregado solo sobre los requisitos **obligatorios** de la acción:
1. BELOW_REQUIRED si alguno es insuficiente;
2. si no, UNKNOWN si falta acreditar alguno;
3. si no, ABOVE_REQUIRED;
4. en otro caso, MATCH.

Las métricas opcionales no alteran el agregado. Se registran **todas** las causas. El estado no es la disposición.

| Situación | Disposición |
|---|---|
| Principal en BELOW_REQUIRED para la acción | STOP de esa acción antes de trabajo sustantivo (P-09) |
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
| E12 | Principal sin `repo-write` acreditado; lo demás en MATCH | CUSTODY: UNKNOWN; RESUME_DECISION: MATCH | no toma la custodia (P-10); puede responder RESUME_DECISION |

## 5. Binding (D-05)

**Entradas:**
- requisitos del rol y de la acción;
- independencia (§11);
- observación de capacidad vigente (§6);
- **elegibilidad** de ADR-0046 #4, que se conserva: invocación **medida** de la celda, **consumo cubierto** y celda no `STALE`.

Una cuota desconocida no equivale a consumo autorizado, y una entrada de catálogo no acredita los hechos.

**Orden, sin ciclos.** El binding consume la observación; la observación **no** depende del binding (§6):
1. candidatas del catálogo;
2. observación de capacidad (preflight);
3. invocación medida previa, de una medición autorizada que no es un binding;
4. registro de binding (transitorio hasta su custodia, §8);
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
| **Observación de capacidad** (`rackcad-preflight/v1`) | un runtime candidato para un rol y una acción: localización y versión del ejecutable, autenticación (sin credenciales), introspección y nivel de garantía, permisos y sandbox, huella de configuración, celda del catálogo, perfil y requisitos | cambio de **instancia de host**, runtime, versión o ruta del binario, estado de autenticación, huella, blob de la entrada de catálogo o versión de `routing.md`. **No** la invalidan el binding (que la consume) ni el SHA de la rama | la sesión, antes del binding y al abrir (Principal) |
| **Comprobaciones de relevo** (16.4, sin cambio de semántica) | Git (`HEAD`, remoto, `origin/main`, árbol limpio, operaciones en curso), procesos, **estado de delegación derivado** (B.8.2: como máximo una delegación abierta; ninguna aceptación nueva si es desconocido) y huella antes y después | se repiten en cada Exit/Entry | la sesión, en cada invocación |

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
| `claude-desktop-session` | PRINCIPAL | DISP. | DISP. (`get_session`) | N/A | N/A | N/A | N/A | **por observador externo**: candidata `isRunning` de los metadatos de la app, observada por otra sesión autorizada, o atestación del Owner (§9.1); UNVERIFIED hasta F2 | UNVERIFIED | UNVERIFIED (ajustes) |
| `claude-subagent` | WORKER, REVIEWER | DISP. | DISP. (transcripción) | DISP. | DISP. | DISP. | DISP. (60 min) | DISP. (notificación + procesos) | DISP. | UNVERIFIED |
| `claude-cli` | REVIEWER, ARCHITECT | DISP. | NOT_AUTHENTICATED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED |
| `codex-cli` | CONTROLLER, ARCHITECT, WORKER | DISP. | DISP. | DISP. | DISP. (binario a revalidar) | DISP. | DISP. (600 s) | DISP. | DISP. | **DISP.: `config.toml`, obligatoria** |
| `codex-desktop-session` | PRINCIPAL (B) | DISP. | candidata (SP-3) | N/A | N/A | N/A | N/A | UNVERIFIED (sin observador conocido; atestación del Owner admitida, §9.1) | UNVERIFIED | UNVERIFIED (¿comparte `config.toml`?) |

**Huella:** «ninguna» solo con la demostración del descriptor y la **aceptación registrada del Coordinator en el gate F2**. `codex-*` declara `config.toml`;
**P-01 se conserva**.

**Hechos del adapter y su validación:** Anexo B.3. El objeto `Facts` es el **único punto abierto** del esquema del núcleo, y su frontera estricta es el
esquema del adapter. Lo valida el productor; el aceptante lo contrasta con los hechos de Exit/Entry de 16.4, tomados en otro instante.

## 8. Custodia por unidad (D-08)

El estado canónico de una unidad I62 es `docs/automation/state/<unit>.yml` con `rackcad-automation-state/v2`. **Contrato completo: Anexo B.8** (campos,
tipos, cardinalidad, estado de delegación derivado, invariantes, reconstrucción). Cada escritura de ese archivo es un **punto durable**: un commit de la sesión
en la rama de la unidad, publicado por CAS. Solo hay puntos durables donde 16.4 permite escrituras Git de la sesión.

### 8.1 Puntos durables

| Punto | Cuándo | `window.state` | `principal.state` | Fija |
|---|---|---|---|---|
| **BOOTSTRAP** | bootstrap de la unidad I62, **antes** de la revisión de G0 (§8.6) | CLOSED | HELD | `protocol` con la **evidencia** de clasificación y `g0_acceptance` PENDING; titular con su preflight y su binding **propuesto** (`principal.acceptance` PENDING), custodiados en el mismo commit; contadores vacíos |
| **Q0** | paso (1) de 16.4, **obligatorio** antes de cada ventana de cesión | OPENABLE | HELD | `task_intent` con el contrato custodiado; si la intención es una corrección, `attempts` +1 y su `CorrectionLaunch` (16.8) en el mismo commit |
| **Q7** | paso (7) de 16.4: tras la verificación y los controles, o tras el cierre declarado de la ventana | CLOSED | HELD | `last_window` (cierre y referencias custodiadas), cadenas, contadores desde el diario, manifiesto de custodia |
| **QU** | actualización del titular **sin ventana** (lo que en `/v1` es «publicar al terminar cada ejecución») | CLOSED | HELD | fase, `state`, `gate`, `next_action`, `last_evidence_commit`, `attempts` fuera de la ejecución delegada (§8/§9) o la intención siguiente; no toca `window`, `last_window` ni los contadores de ventana |
| **QH** | punto quiescente en que el titular **libera** la unidad | CLOSED | RELEASED | la intención siguiente, si existe, y la liberación; el titular liberado no opera después |
| **QR** | recuperación o transferencia del titular, por CAS | CLOSED | HELD (nuevo titular) | designación del Coordinator; si había una ventana posiblemente abierta, su cierre ABANDONED con la reconstrucción de B.8.5 |

Todo commit que modifica el archivo de estado es exactamente uno de estos puntos, con `record_version` + 1. Los demás commits de la sesión fuera de una ventana
(documentos, evidencia) no son puntos durables; tras un Q0, cualquier commit de la sesión impide abrir la ventana (W-1) y obliga a cerrarla (T18) y a emitir un
Q0 nuevo.

### 8.2 Ventana de cesión

La ventana se abre con la primera cesión posterior a un Q0 y se cierra con el Q7 o el QR siguiente.

- **W-1:** una ventana solo se abre con `HEAD` = `origin/<rama>` = un commit Q0 (la salida de 16.4 lo comprueba con `git ls-remote`).
- **W-2:** entre el Q0 y el cierre no hay escrituras Git de la sesión (16.4 sin cambio). La única excepción es el rebase de 16.7, que no escribe el archivo de
  estado.
- **W-3:** el Worker nunca escribe el archivo de estado (está en el alcance prohibido de toda delegación).

**Consecuencia:** el último punto durable de la rama dice, por sí solo, si puede existir una ventana: **solo si es un Q0**. Entre el `CurrentSha` del Worker y la
verificación del Controller no hay ningún commit de la sesión.

### 8.3 Tres situaciones de delegación, sin redefinir la de I-61

Función completa en B.8.2.

| Situación | Cómo se conoce | Significado |
|---|---|---|
| **Tarea planificada** (intención de delegación) | durable: `task_intent` ≠ `null` (en Q0, Q7, QU, QH o QR) | el Coordinator fijó la siguiente tarea y su contrato; **no** hay delegación aceptada |
| **Delegación abierta aceptada** | solo en el diario de la ventana: aceptación A1'-A8' de `d` sin verificación válida registrada ni cierre declarado | **exactamente la de 16.6**: aceptada y sin verificación válida ni cierre. Nunca es durable: por W-2, entre la aceptación y la verificación no se escribe el estado |
| **Ninguna delegación abierta** | durable si el último punto no es Q0; dentro de una ventana, por el diario (sin aceptación, o con verificación o cierre registrados) | — |
| **Desconocida** | último punto Q0 sin diario disponible o con la cadena rota | se trata como posiblemente abierta: ninguna aceptación nueva (P-02) y reconstrucción de B.8.5 |

### 8.4 Qué es durable y qué es transitorio

Tabla completa en B.8.3.

- **Durable en Q0:** la intención (`TaskId`, `Attempt`, clase, contrato custodiado, roles planificados con los bindings ya aceptados), `attempts` y
  `CorrectionLaunch` de una corrección, `record_version` y `window` OPENABLE con su número.
- **Transitorio entre Q0 y Q7** (diario encadenado: los `relay-record/v2` del directorio del intento, cada uno con `PrevRelaySha256` y `WindowSeq`):
  - aceptación de bindings nuevos;
  - delegación `d` y su aceptación;
  - cesiones, terminaciones, handoff, verificación y controles negativos;
  - lanzamientos, incluidos los inciertos; reejecuciones; `RebaseMap`;
  - en su caso, el relevo del titular dentro de la ventana (T12a).
- **Durable en Q7:** el cierre de la ventana (`last_window`), la custodia del diario (manifiesto TRANSIENT → CUSTODIED), las cadenas (`ChainBaseSha`,
  `ChainRedSha`, `ChainRedFiles`), los contadores de reejecución, recuperación e invocaciones, y el titular vigente.

En Q7 el diario se custodia en `docs/automation/evidence/<unit>-agent/<task>/…` y queda durable. Una referencia transitoria **nunca** se presenta como evidencia
custodiada (Anexo B.2, `Location`). Ningún SHA se escribe dentro de su propio commit: `ChainBaseSha` (el Q0 de la primera ventana de la tarea) y `VerifiedSha`
se fijan en el Q7 siguiente.

### 8.5 SHAs y CAS

| SHA | Qué es |
|---|---|
| `BaseSha` | `HEAD` al abrir la delegación = el Q0 de la ventana |
| SHA del trabajo | `RedSha`, `CurrentSha` = commits del Worker |
| SHA de estado | los puntos durables |
| SHA evaluado del gate | `CurrentSha` + commits de la sesión limitados a `docs/automation/` y a los documentos de la unidad (16.1, sin cambio) |
| revisión de evidencia | Q7 |

`Identity` (`HEAD` = remoto = `CurrentSha`) **no cambia**.

**CAS por Git en cada punto durable:** se lee `record_version` n en `HEAD` = `origin/<rama>`, se escribe n+1 y se hace push sin force. Un **push rechazado**
significa transición **no acreditada en el remoto**, no inexistencia de cambios locales: el commit local se conserva. Se inspecciona la causa antes de repetir:

| Causa observada | Tratamiento |
|---|---|
| no fast-forward (el remoto avanzó) | `fetch` y clasificar según T10 |
| permisos o autenticación | S-06 |
| transporte | reintento acotado tras la inspección |
| estado remoto desconocido | STOP |

Sin servicio de locks. Los compromisos compartidos son solo referencias. No hay registro global. Git prueba identidad y remoto, **no** quién opera en otra
máquina.

### 8.6 Arranque de una unidad I62: bootstrap y después G0 (Modelo A)

**Modelo elegido: A.** Es el orden normal de WORKFLOW (§3: rama, commit de reclamo y push; §11.1: el primer push aceptado del commit de reclamo fija el
`Claim-Id`) y de LIFECYCLE (el Coordinator revisa G0 sobre el bootstrap publicado). I-62 no lo altera.

El Modelo B se descarta. Exigiría una decisión del Coordinator sobre una unidad que todavía no tiene `Claim-Id` ni bootstrap publicado, y ninguna autoridad
vigente prevé ese artefacto.

**Secuencia (observación → propuesta → aceptación → referencia durable):**

| Paso | Quién | Artefacto | Dónde existe | Aceptación |
|---|---|---|---|---|
| 1. Observación | la sesión que reclama | su `preflight/v1` con `Action` CUSTODY | TRANSIENT: `artifacts/orchestration/<unit>/bootstrap/<RunId>/` | — |
| 2. Propuesta | la sesión | `binding/v1` del Principal: `Scope` UNIT, `Role` PRINCIPAL_COORDINATOR, `Acceptance.State` PENDING (`DecisionRef` y `Utc` = `null`, no aplica) | ídem | PENDING |
| 3. **BOOTSTRAP** + push | la sesión | custodia de 1 y 2 en `docs/automation/evidence/<unit>-agent/bootstrap/` y `state/v2` (`record_version` 1). El estado contiene: `protocol.basis` con la evidencia; `g0_acceptance` PENDING; `principal.preflight` y `principal.binding` apuntando a esos blobs; `principal.acceptance` PENDING | el mismo commit (CUSTODIED) | PENDING / PENDING |
| 4. Revisión de G0 | Coordinator | decisión: G0, más dos marcadores explícitos: `I62-CLASSIFICATION: I62 \| REJECTED` y `I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED \| REJECTED` | fuera del repositorio (orden del Coordinator; su hash va a la evidencia) | decidida |
| 5. Registro + **QU** + push | la sesión | **en un solo commit**: la entrada en `docs/automation/decisions/<unit>.md` con los dos marcadores, el `Claim-Id` y la `record_version` del BOOTSTRAP; la versión del binding con `Acceptance` ACCEPTED o REJECTED, `DecisionRef` = {ruta de decisiones, marcador} y `Utc`; y un QU con `g0_acceptance` y `principal.acceptance` transicionados y sus `StateRef` a esos blobs | el mismo commit | ACCEPTED o REJECTED |

**Reglas:**
- **Sin referencias futuras ni propias.** En el paso 3, los `StateRef` apuntan a blobs del mismo commit (preflight y binding PENDING). En el paso 5, a blobs del
  mismo commit (decisiones y binding aceptado). El estado nunca cita su propio blob ni el SHA de su propio commit.
- `protocol.basis.claim_commit` = el SHA del commit de reclamo si es anterior al BOOTSTRAP. Es `null` (no aplica) cuando el reclamo **es** el BOOTSTRAP.
  `claim_parent_sha` es el padre del commit de reclamo y existe antes de él.
- **Antes del paso 5:**
  - el clasificador devuelve PENDING_G0 (E.2): STOP (P-15) de contratos y delegaciones;
  - Q0 está prohibido (I-S15); solo se admiten QU, QH y QR;
  - el trabajo no delegado de G0 (contrato, documentos) sigue según WORKFLOW.
- **Aceptación nunca inferida.** Un G0 GATE PASS sin los marcadores **no** transiciona nada: la aceptación sigue PENDING y la sesión pide la decisión
  explícita.
- **Clasificación REJECTED** → UNKNOWN → STOP. Solo la remedia una decisión posterior del Coordinator (E.2, paso 6).
- **Binding del Principal REJECTED** → la unidad no tiene Principal aceptado y Q0 sigue prohibido. Se remedia con:
  - observación y propuesta nuevas, aceptadas por una decisión posterior y registradas en un QU; o
  - una transferencia (QR).
- **Titular nuevo:** la designación del Coordinator (QR o T12a) incluye el marcador de aceptación de su binding. En el QR, el binding aceptado se custodia en el
  mismo commit. En T12a va en el registro TRANSFER del diario y se custodia en el Q7.
- **Transiciones:**
  - `protocol.g0_acceptance` cambia **una sola vez** (PENDING → ACCEPTED o REJECTED), y `principal.acceptance` una vez por titular;
  - `protocol.set`, `protocol.effective_sha` y `protocol.basis` son **inmutables** desde el BOOTSTRAP (B.8.4: I-P06, I-P09).

## 9. Recuperación (D-09) y presupuestos (D-10)

### 9.1 Evidencia de fin de un escritor

| Evidencia | Definición | Basta para |
|---|---|---|
| **TERMINATION_ACCREDITED** | operación 7 del adapter ligada al escritor exacto (`ActorRef`/`RunId`, PID + `CreationDateUtc`) **y**, para invocaciones de trabajo, `Outcome` registrado. Para sesiones de Principal, la observa **otra sesión autorizada** (p. ej., la sesión de supervisión con los metadatos `isRunning` de la app) o la atesta el Owner, nunca la propia sesión observada | cerrar la cesión o transferir la custodia, con las demás comprobaciones |
| **ISOLATION_ACCREDITED** | no se admite (no hay mecanismo medido) | — |
| **NO_OBSERVATION** | ausencia de observación | nada |

Estados del escritor: **WRITER_ALIVE**, **TERMINATION_UNACCREDITED** y **ORPHAN_CONFIRMED** (terminación acreditada sin entrega ni cesión resuelta). Los dos
últimos no autorizan tomar posesión por sí mismos.

### 9.2 Transiciones (análisis completo con SHAs simbólicos en el Anexo F)

| # | Estado previo | Evento | Precondición / evidencia | Decisor | Registro | Permitido / prohibido |
|---|---|---|---|---|---|---|
| T0 | BOOTSTRAP (`g0_acceptance` y `principal.acceptance` PENDING) | decisión de G0 con los marcadores (§8.6) | decisión explícita del Coordinator; sin marcadores no hay transición | Coordinator / P | **QU** (CAS) con las aceptaciones y el binding aceptado | Q0 solo tras ACCEPTED en ambas |
| T1 | Q0 (titular P, ventana k OPENABLE) | cesión a C (planificación) | Exit 16.4 en pass con `HEAD` = `origin` = Q0; binding de C aceptado | P | diario (r1, `WindowSeq` k) | P no opera en el alcance |
| T2 | cesión a C | terminación de C | TERMINATION_ACCREDITED + Entry | P | diario | — |
| T3 | tras planificar | aceptación A1'-A8' | registros válidos | Coordinator | diario: aceptación de `d` (**delegación abierta**) | sin escritura Git |
| T3' | tras planificar | rechazo en la aceptación | alguna de A1'-A8' en fail | Coordinator | diario; Q7 con cierre NOT_ACCEPTED | ningún Worker |
| T4 | `d` abierta | cesión a W | Exit en pass | P | diario (r2) | P no opera |
| T5 | cesión a W | W publica R y G y entrega | handoff + `Outcome` COMPLETED + terminación acreditada + Entry (`HEAD` = remoto = G) | P | diario | sin escritura Git |
| T6 | tras T5 | cesión a C (verificación) | Exit; `Identity` sobre G | P | diario (r3) | — |
| T7 | verificado | controles negativos y Q7 | VERIFIED o disposición; controles | P | **Q7** (CAS): `last_window` | — |
| T8 | cesión activa | tope vencido | operaciones 6 + 7 | P | diario | si 7 no es acreditada → T11 |
| T9 | cesión a W | W caído con commits sin handoff | terminación acreditada; commits en el remoto | P | diario | `Handoff` → BLOCKED; el Controller puede verificar commits verificables |
| T10 | cualquiera | `HEAD`/remoto ≠ lo esperado | clasificación: (a) avance del Worker dentro del alcance durante su cesión → normal (T5); (b) commit local de la sesión sin publicar → publicar en un punto permitido o, si estaba en cesión, P-08; (c) escritura ajena (commits no atribuibles al escritor vinculado) → STOP P-02/S-07; (d) `main` cambió → S-13 / 16.7 | — | según el caso | no todo desajuste va a rebase |
| T11 | cesión o titular ausente | sin observación | NO_OBSERVATION | Coordinator | ninguno; STOP P-12 | prohibido tomar posesión, reset, borrado o matar procesos |
| T12a | ORPHAN_CONFIRMED(P) con último punto Q0, diario accesible y cadena íntegra | recuperación dentro de la ventana | designación de N por el Coordinator + terminación de P acreditada | Coordinator / N | diario: registro TRANSFER encadenado; el Q7 fija el titular nuevo | N opera solo tras el TRANSFER; sin escritura Git dentro de la ventana |
| T12b | ORPHAN_CONFIRMED(P) con último punto ≠ Q0, o con último punto Q0 y diario no disponible o roto | recuperación | designación + inspección que preserva el trabajo + reconstrucción B.8.5 si el último punto era Q0 | Coordinator / N | **QR** (CAS); con Q0 previo, el QR cierra la ventana ABANDONED | solo N opera, tras ganar el CAS |
| T13 | cualquiera | dos recuperaciones concurrentes | — | — | gana el primer CAS (T12b) o la única designación (T12a) | el perdedor **no opera**, aunque relea: sin designación vigente y transición nueva no hay operación; no hay dos sesiones en el mismo worktree ni en la misma rama |
| T14 | cualquiera | cambio de máquina sin acreditar exclusividad ni recuperación | — | Coordinator | ninguno; STOP P-13 | no se reconstruye trabajo inaccesible |
| T15 | cualquiera | registro que contradice el hecho físico | contradicción | Coordinator | ninguno; S-04/P-02; corrección por una transición nueva | prevalecen los hechos (WORKFLOW §10) |
| T16 | QH (P liberó), o Q7, QU o QR con titular P | transferencia del titular (A→B) | terminación de P acreditada (observador externo u Owner) + designación del Coordinator con aceptación del binding de B (§8.6) | Coordinator / B | **QR** (CAS), con el binding aceptado custodiado | B opera solo tras el CAS |
| T17 | BOOTSTRAP, Q7, QU o QR con titular P | liberación | último punto ≠ Q0 | P | **QH** (CAS) | P no opera después |
| T18 | Q0 sin ninguna cesión registrada | retirada de la intención | decisión del Coordinator (p. ej., reemisión del contrato antes de planificar) | P | **Q7** con cierre WITHDRAWN | — |

**Fallos del mandato:**
- **Principal ausente:** T11 / T12a / T12b / T16.
- **Worker caído:** T9.
- **Controller sin contexto:** **S-12 = STOP**, `analysis.md` y decisión del Coordinator (16.11, sin cambio, Anexo G).
- **Cuota agotada:** P-06.
- **Autenticación perdida:** S-06.
- **Cambio de máquina:** T14.
- **Handoff ausente:** T9 / `Handoff`.
- **Diario perdido:** B.8.5 (nunca cero lanzamientos; commits sin verificar declarados).
- **Artefactos transitorios:** se conservan y no se reutilizan.
- **Trabajo sin commit en el host:** se preserva.
- **SHA distinto:** T10.

### 9.3 Presupuestos y su autoridad

| Contador | Evento que cuenta | Autoridad durable (B.8.1) | Entre Q0 y Q7 | Sin diario (B.8.5) |
|---|---|---|---|---|
| `attempts` | **corrección lanzada** (16.8) | `automation_state.attempts`, +1 en el Q0 de la corrección | — | durable |
| por clase (`TaskId`, `FailureClass`) | **corrección lanzada** de esa clase, no verificaciones | `counters.correction_launches`: entrada en el mismo Q0 | — | durable. Dos verificaciones de la misma entrega no son dos correcciones. Una corrección lanzada y caída antes de verificar sí cuenta |
| reejecuciones BLOCKED (`TaskId`, fase) | reejecución lanzada | `counters.blocked_reruns`, en Q7 | diario | la ventana abandonada cuenta como reejecución de la fase en que se perdió; nunca cero |
| recuperaciones de 16.7 | recuperación | `counters.rebase_recoveries` (`RebaseMap`), en Q7 | diario | las que prueben los commits reescritos observables |
| invocaciones | **lanzamiento**, cada uno con `RunId`; un lanzamiento con resultado incierto se cuenta como lanzado (`LAUNCH_UNCERTAIN`) | `counters.invocations`, en Q7 | diario | `Launched` probado + `Uncertain` por fase según B.8.5; P-07 compara lanzados + inciertos con el tope **antes** de lanzar |

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

Una vez decidida, la alternativa queda identificada por versión y cláusula en el Freeze. Sin decisión, V5 conserva ambas y no hay AGREED ni Frozen: YES. No se
crea otro bloqueo externo.

## 12. Pilotos y portabilidad (D-17)

Especificación completa en el **Anexo D**:
- fixture arrancable con activación de prueba;
- semántica de `Ci`;
- hoja de invocaciones por escenario, rol y fase, con topes por ronda y totales;
- portabilidad en **dos escenarios separados**:
  - **FX-04a, reanudación y decisión:** A libera la unidad en un punto quiescente canónico (QH) y termina con terminación acreditada. B empieza sin el
    contexto privado de A, reconstruye los hechos y produce la siguiente decisión de gate o delegación. Publica su respuesta antes de ver el oráculo, y una
    comparación mecánica decide PASS o FAIL. **PASS no exige que B, ni un Worker que B pueda lanzar, sea capaz de ejecutar esa decisión**;
  - **FX-04b, continuación:** se intenta ejecutar la decisión hasta VERIFIED y Q7. Depende de las capacidades y de las OD que exija esa acción, y su resultado
    es PASS, FAIL, UNVERIFIED o UNSUPPORTED con la semántica existente;
- FX-03 sigue siendo la prueba completa de la topología B;
- entradas canónicas, técnicas y privadas delimitadas, con la cobertura del registro;
- FX-05 distingue los gates legítimos del fixture de un intento de actuar sobre la unidad real;
- límites por topología, rol y escenario.

La falta de una capacidad de escritura (de B o de un Worker que B pueda lanzar) **nunca** se informa como fallo de la reconstrucción del contexto: solo puede
dejar FX-04b o FX-03 en UNVERIFIED o UNSUPPORTED.

## 13. Semántica de fallo (D-13)

Los STOP de I-61 se conservan, con S-12 sin cambio. Ids nuevos:

| Id | Condición | Comportamiento |
|---|---|---|
| P-09 | Principal en BELOW_REQUIRED para la acción al abrir | STOP de esa acción antes de trabajo sustantivo |
| P-10 | requisito obligatorio de un rol o acción en UNKNOWN o BELOW_REQUIRED | no se vincula ni se invoca; no consume `attempts` |
| P-11 | huella declarada cambiada durante una cesión | STOP (P-01 para Codex, igual) |
| P-12 | TERMINATION_UNACCREDITED u ORPHAN_CONFIRMED sin decisión | STOP; sin toma de posesión |
| P-13 | cambio de máquina sin acreditación | STOP |
| P-14 | adapter desconocido, esquema ausente, versión incompatible o hechos inválidos | no elegible |
| P-15 | `ProtocolSet` o esquema del contrato ≠ protocolo de la unidad; clasificación UNKNOWN en un paso que depende de ella; cita de una unidad I61 a una superficie solo I62; mapa de cláusulas inválido (Anexo E.4) | STOP |
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
`integration/I-62`. No hay activación parcial: el resolver, sus punteros, el mapa y su esquema entran en ese único merge.

**Clasificación** (algoritmo en el Anexo E.2): durable y **nunca rederivada por ascendencia actual** (WORKFLOW §11.1).

| Clase | Fuente durable | Protocolo | Paso dependiente |
|---|---|---|---|
| **ANTERIOR_DEMOSTRADA** | el `Claim-Id` de la unidad aparece una sola vez en la tabla PRE del cuerpo del merge efectivo (snapshot `git ls-remote --heads origin` con `ref / tip / commit de reclamo / Claim-Id`, como WORKFLOW §11.3) | I61, todo su recorrido | — |
| **POSTERIOR_DEMOSTRADA** | su `Claim-Id` no está en PRE; el BOOTSTRAP `state/v2` registra la **evidencia** (`protocol.basis`: el padre del commit de reclamo contiene `I62_EFFECTIVE_SHA`); el Coordinator la acepta en G0 y un QU posterior registra esa aceptación (`g0_acceptance` ACCEPTED, §8.6) | I62 | — |
| pendiente de G0 | BOOTSTRAP `/v2` con `g0_acceptance` PENDING | **ninguno todavía** (PENDING_G0) | **STOP** (P-15) de contratos y delegaciones hasta la aceptación |
| posterior con base obsoleta | `Claim-Id` fuera de PRE; el padre del reclamo no contiene el efectivo; prueba de orden del primer push (WORKFLOW §11.3) aceptada en la decisión de G0 | I62 | **STOP** de toda delegación hasta que la rama contenga el efectivo |
| **DESCONOCIDA** | ninguna de las anteriores (solo en POST sin prueba de orden, Claim-Id ausente o duplicado, identidad contradictoria) | **ninguno asignado** | **STOP** de los contratos y delegaciones; se pide la evidencia concreta. Sin default I61 ni I62 |

**Quién clasifica:** el **Coordinator**, en el G0 de cada unidad nueva (aceptación de la evidencia del BOOTSTRAP, §8.6) y antes de emitir cualquier contrato
posterior al efectivo. El Coordinator, al aceptar (A6'), y el Controller, en `Authority`, vuelven a aplicar la función sobre las mismas fuentes durables; una
discrepancia es S-04. Las unidades sin campo `protocol` (todas las anteriores, con estado `/v1`) se clasifican por su `claim_id` en PRE, **sin editar su rama
ni su bootstrap**.

**Resolución de autoridades, transparente para los contratos I61** (algoritmo en el Anexo E.3):
- vive en una subsección nueva, **AUTOMATION_PLAN §16.13**, independiente del protocolo, que toda unidad lee en `MainSha`;
- §16.3 conserva literalmente el texto de I-61 y añade una frase puntero; otra frase puntero abre §16. Quien **lee** un contrato I61 (el Coordinator al
  aceptar, el Controller en `Authority`) llega a ellas por la propia regla de I-61, que lee §16 como `EXTERNAL` en `MainSha`;
- **el contrato I61 no cambia:** ni esquema `/v1`, ni campos nuevos, ni citas de §16.13 o del mapa, ni reemisión para acogerse al resolver. Su autor no
  necesita conocer I-62;
- el **mapa de cláusulas modificadas** es un archivo de ruta fija que se lee **en `I62_EFFECTIVE_SHA`**, inmutable. Se valida contra la derivación mecánica
  `I62_EFFECTIVE_SHA^1` → `I62_EFFECTIVE_SHA`, y cualquier ausencia, duplicado o contradicción falla cerrado (E.4);
- `MainSha` y `AuthorityRevision` **conservan su significado `/v1`**. Para unidades I61 solo cambia, en las cláusulas del mapa, la revisión en la que se lee el
  texto `EXTERNAL`: `I62_EFFECTIVE_SHA^1` en lugar de `MainSha`;
- una cita **«documento completo»** de un archivo que I-62 modificó se conserva tal cual y se lee **compuesta**: el preámbulo y cada sección `##`, con la regla
  de la cita individual (E.3). **Compromiso declarado:**
  - una sección `##` que I-62 modificó se lee entera en `EFF^1`, incluidas sus subsecciones no tocadas, que dejan de evolucionar para las unidades I61;
  - un archivo modificado que no es Markdown se lee entero en `EFF^1`;
  - las secciones nuevas solo I62 no forman parte de la lectura I61;
- no se editan unidades I61 ni esquemas `/v1`, y no se congela ningún archivo entero: las cláusulas no modificadas se leen en `MainSha`, con su evolución
  posterior.

**Sin efecto retroactivo:** OD-6 y todas las reglas nuevas aplican solo a unidades I62. La clasificación ANTERIOR **no** obliga a ninguna unidad a usar la
delegación de I-61: cada una conserva su régimen (I-52 su Workflow y su alcance autorizado; I-63, I-64 e I-62 lo suyo) y no gana trabajo que no tuviera
autorizado.

## 15. Planos, MaterializationClose y fixture (D-15)

| Plano | Qué incluye | Autoridad | Nunca puede |
|---|---|---|---|
| (a) real | la sesión de I-62 y la gobernanza | I-61 y Workflow vigentes | usar reglas de I-62 sin integrar como autoridad |
| (b) materializado inactivo | textos y contratos de F1-F4 en la rama, con cláusula de vigencia «desde `I62_EFFECTIVE_SHA`, solo para unidades I62» (§16.13: «desde `I62_EFFECTIVE_SHA`, para toda unidad») | ninguna hasta la vigencia | gobernar operaciones reales |
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
Las guardas Core no dependen de la historia de Git: la CI hace checkout superficial del SHA exacto, y lo que exige historia es MC (C-15, C-20b, C-20c).

**Un control que falla:**
- si la implementación se aparta del Freeze → **corrección** que devuelve al Freeze, sin A-n;
- si el Freeze es incorrecto o incompleto → **A-n** con sus autoridades.

## 17. Plan de gates (D-16)

**Level A, decidido antes del Freeze.** F4 verifica la viabilidad del procedimiento versionado (C-15, C-16, C-17, C-20b, C-20c). Si falla un control, se aplica
la regla de §16. F5 no se planifica.

| Gate | Resultado observable | Obligaciones (Anexo C) | Dependencias |
|---|---|---|---|
| **F0** | Proposal acordada y congelada (misma versión exacta) | — | OD-6 decidida; cero REQUIRED |
| **F1** | **ADR sucesor formal creado como «propuesto»** (número asignado en ese momento, comprobado contra `main` y las ramas activas), **sin** fila de índice: el índice se actualiza en el cierre documental (WORKFLOW §11.4); sin ventana. Materialización (b) de roles, perfil por acción, estados, autoverificación, renderizado, cláusula de vigencia y corrección de PROMPT_TEMPLATES §2 | C-01..C-04 | OD-1 no bloquea |
| **F2** | Observación de capacidad + descriptores y esquemas de hechos; `preflight/v1`, `relay-record/v2`, `controller-verification/v2`; huellas aceptadas | C-05..C-10 | OD-2 solo para medir **invocando** `codex-cli` |
| **F3** | Binding, independencia, `binding/v1`, `gate-contract/v2`, `delegation/v2`, A1'-A8' y las 14 comprobaciones (Anexo G) | C-11..C-14 | — |
| **F4** | Custodia: `state/v2` completo con su validador semántico (B.8), arranque BOOTSTRAP → G0 → QU (§8.6), puntos BOOTSTRAP/Q0/Q7/QU/QH/QR, diario encadenado, reconstrucción sin diario, commits sin verificar; transiciones; presupuestos. Adopción: §16.13, punteros, mapa de cláusulas y su esquema (Anexo E). Planos. **Cierre = MC_I62** | C-15..C-21 (C-20a, C-20b, C-20c) | — |
| **F6** | Fixture arrancado (D.1) y escenarios FX-01, FX-02, FX-03, FX-04a, FX-04b y FX-05 (D.3, D.4) | C-22..C-27 (C-25a, C-25b) | OD-5; OD-7 (**precondición del cierre de F6** por FX-02: sin CI no hay VERIFIED); OD-2 (A y FX-04b); OD-3 (B); OD-4 (B y FX-04b). **FX-04a no depende de OD-3, OD-4 ni OD-7** |
| **F7** | Preparación del cierre sin cambio normativo: borrador factual de FOUNDATIONS en la evidencia, ideas-futuras, paquete OV | — | no depende de READY-06 |
| **READY-01..09** | LIFECYCLE §8 | — | OD-1 antes de READY-03 |
| **FINAL_CANDIDATE_SHA** | Full, CI exacta, OV final | OV-I62-01..05 | — |
| **Cierre documental** | FOUNDATIONS publicada, **índice ADR**, HANDOFF, ROADMAP (ventana) | — | WORKFLOW §11.4-11.5 |
| **Integración** | merge efectivo con §16.13, punteros, mapa regenerado y revisado sobre el merge local (C-20b) antes del push; PRE en el cuerpo; POST y blob del mapa en el tag | C-20b | WORKFLOW §11.5-11.6 |

No hay reutilización de evidencia por igualdad de árbol. Cada SHA tiene la suya.

## 18. Owner Validation y decisiones del Owner

**OV por escenario** (asignación a I-62; sin AutoCAD):

| Id | Lo prepara | Ensayo | Ejecución final sobre FINAL_CANDIDATE_SHA |
|---|---|---|---|
| OV-I62-01 autoverificación | F1 | C-23 (F6) | el Owner abre una sesión del sistema bajo prueba en el fixture resembrado desde el Candidato, con effort inferior y después correcto |
| OV-I62-02 preflight del host | F2 | C-06 | el Owner revisa el preflight producido con el procedimiento del Candidato, **con el estado observado en ese momento** |
| OV-I62-03 topología A | F6 | C-24 | ejecución compacta (D.5) |
| OV-I62-04 topología B o su limitación | F6 | C-26 | ídem si hay OD; si no, decisión del Owner sobre la limitación (el escenario no se retira) |
| OV-I62-05 portabilidad | F6 | C-25a, C-25b | **(a)** FX-04a compacto (D.5): obligatorio; **(b)** FX-04b compacto si sus OD están concedidas; si no, decisión del Owner sobre la limitación de la continuación, que no afecta a (a). El escenario no se retira |

**Decisiones del Owner:**

| Id | Decisión | Bloquea | Momento |
|---|---|---|---|
| **OD-6** | predicado de independencia de LIFECYCLE (§11.4) | acuerdo y Freeze de F0 | pendiente |
| OD-1 | ADR sucesor (ampliación de §16, §16.13, fila de WORKFLOW §10; con OD-6 alternativa 1, LIFECYCLE) | READY-03 y vigencia | antes de READY-03 |
| OD-2 | línea base de huella por adapter (`config.toml`: `37DD3559…` registrada frente a `42E15A03…` observada) | toda invocación afectada (`codex-cli` en A, B y FX-04b; `codex-desktop-session` si comparte) | antes de la primera invocación afectada |
| OD-3 | autenticar Claude CLI | `claude-cli` en B | antes de F6 B |
| OD-4 | sandbox de Codex para escritura | Workers Codex (B y FX-04b) | antes de F6 B y de FX-04b |
| OD-5 | permiso de ensayo con la semántica única de §15 y apertura de sesiones del sistema bajo prueba | F6 | antes de F6 |
| OD-7 | remoto del fixture con CI | **cierre de F6** (FX-02 no puede ser PASS sin CI); FX-04b | antes de F6 |

Cada solicitud va aparte, con la ruta y el hash actuales, el efecto, el consumo, el alcance y las restricciones. El silencio no es decisión.

## 19. Riesgos

Los 12 retos del mandato, con su riesgo residual: [paquete V5](I-62-architect-package-v5.md) §5.

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
- solo unidades I62 (clasificación de §14), salvo §16.13, que rige la lectura de autoridades de toda unidad;
- las unidades I61 terminan con I-61, con la resolución del Anexo E;
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
| `rackcad-automation-state/v2` | evoluciona (§8, B.8) | sesión | Core (C-18) + validador semántico (C-15) | CAS |
| `rackcad-clause-map/v1` | nuevo, **independiente del protocolo** (Anexo E.4) | sesión de I-62 en F4 y en la integración | Core (C-20a) + resolver | derivación (C-20b) |

Reglas: JSON Schema 2020-12; `additionalProperties: false` en **todos** los objetos, **salvo el único punto abierto declarado** `AdapterFacts.Facts` (B.3);
todos los campos declarados son obligatorios; SHA de 40 hex; SHA-256 de 64 hex en minúsculas; instantes ISO-8601 con `Z`.

**Significado de los valores (sin ambigüedad):**

| Valor | Significado |
|---|---|
| ausencia de un campo | inválido |
| `null` | **solo** NOT_APPLICABLE, y solo en los campos marcados «nullable = no aplica» |
| desconocido u observable no observado | **nunca** `null`: se expresa con un **estado explícito** (`State`, `Assurance` = NONE, `Status` = UNKNOWN) o con el literal `"UNKNOWN"` donde el campo es un identificador |
| aceptación pendiente | `Acceptance.State` = PENDING (no `null`) |

### B.2 Identidades y referencias

| Identidad | Forma | Procedencia y resolución | Unicidad / inmutabilidad / errores |
|---|---|---|---|
| `ActorRef` | `{AdapterId, InstanceId, InstanceIdSource, Assurance}` | `InstanceId` observado del runtime: `sessionId` (escritorio), `thread_id` (Codex CLI), id del subagente, o PID + `CreationDateUtc` + instancia de host (proceso). Sin fuente → `Assurance` = NONE e `InstanceId` = `"UNOBSERVED"` | dos bindings con el mismo `ActorRef` = **mismo actor**; Actor no se satisface. `Assurance` NONE → la dimensión es UNKNOWN |
| `SessionRef` | `{AdapterId, SessionId, Assurance}` | sesión de nivel superior (la del padre para un subagente; la propia para una invocación CLI) | ídem |
| `HostRef` | `{HostLabelHash, HostInstanceHash, HostInstanceState, Os}` | `HostLabelHash` = SHA-256 del hostname (solo **etiqueta**). `HostInstanceHash` = SHA-256 de un identificador de instalación del SO (en Windows, el `MachineGuid`; su disponibilidad se verifica en F2). `HostInstanceState` = OBSERVED \| UNOBSERVED | **nunca acredita exclusividad**. `HostInstanceState` UNOBSERVED → no se hereda ninguna observación de capacidad entre sesiones. Dos máquinas homónimas tienen etiquetas iguales e instancias distintas: no heredan capacidad |
| `BindingRef` | `{UnitId, Scope (UNIT \| TASK), TaskId, Role, BindingId, Sha256, Location}` | `TaskId`: nullable = no aplica, **solo** si `Scope` = UNIT (Principal; Architect por unidad). `Location` = `{Kind (TRANSIENT \| CUSTODIED), Path, Commit, Blob}`, con `Commit` y `Blob` nullables = no aplica solo si TRANSIENT. TRANSIENT: ruta bajo el directorio del intento + SHA-256. CUSTODIED: blob en un punto durable | `BindingId` único por unidad. Contenido distinto con el mismo id → S-04, **salvo** la transición única de `Acceptance` (PENDING → ACCEPTED o REJECTED, con `DecisionRef` y `Utc`); el binding aceptado es una versión nueva del mismo `BindingId` con el resto del contenido idéntico (§8.6). `UnitId` o `TaskId` ajenos → A2' rechaza. Un binding sustituido por un rebinding posterior del mismo rol y ámbito → **obsoleto**, rechazo. En Q7, toda referencia TRANSIENT usada por un artefacto custodiado se resuelve a CUSTODIED mediante el manifiesto de custodia (ruta + SHA-256 → ruta + blob). Un TRANSIENT nunca es evidencia custodiada |
| `StateRef` (`ref` del estado) | `{path, blob}` | artefacto custodiado en el mismo commit del punto durable o en un ancestro | la ruta existe y su blob coincide; el estado `/v2` **nunca** contiene referencias TRANSIENT |
| `PreflightRef` | `{PreflightId, Location, Sha256}` | `PreflightId` = `^P[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` | inválido si cambió un invalidador (§6) |
| `RunId` / `BindingId` | patrón de I-61 / `^B[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` | los asigna la sesión | únicos |
| `RequirementId` | `^[A-Z_]+\.[a-z0-9-]+$` | lista fija por perfil y acción en `routing.md` (blob en la `AuthorityRevision`) | único por preflight. Dos filas del mismo id con valores distintos → `Contradiction` → UNKNOWN + S-04 |
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
| `Schema`, `PreflightId`, `UnitId`, `Role`, `Action`, `ProtocolSet` | `Action` ∈ RESUME_DECISION \| CUSTODY \| `<acción del rol>` (§4.1) |
| `Profile` | `{ProfileId, RoutingBlob}`: el perfil existe **aunque no haya binding** |
| `Host` | `HostRef` |
| `ObservedUtc` | instante |
| `Actor`, `Session` | `ActorRef` y `SessionRef`. Para una celda candidata sin sesión: `Assurance` = NONE e `InstanceId` = `"NOT_STARTED"` (estado, no `null`) |
| `Adapter` | `{AdapterId, AdapterVersion, BinaryPathHash, DescriptorRef}` |
| `AdapterFacts` | B.3 |
| `Fingerprint` | `{Kind, Sha256, KeyNames[]}` o `{Kind: "none", AcceptanceDecisionRef}` |
| `Requirements[]` | `{RequirementId, Mandatory (bool), Required (valor), Observation {State (OBSERVED \| NOT_OBSERVED), Value, Source, Assurance (REQUESTED \| CONFIGURED \| RUNTIME_OBSERVED \| SERVICE_ATTESTED \| NONE), ObservedUtc}, Status (MATCH \| ABOVE_REQUIRED \| BELOW_REQUIRED \| UNKNOWN), Contradiction (bool), ContradictionEvidence}`. `Value`, `Source`, `ObservedUtc` y `ContradictionEvidence` son nullables = no aplica solo si `State` = NOT_OBSERVED o `Contradiction` = false. Único por `RequirementId`; un obligatorio de la acción ausente → se añade con NOT_OBSERVED y UNKNOWN |
| `ConfigurationStatus` | agregado de §4.2 para la `Action` |
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
| `Acceptance` | `{State (PENDING \| ACCEPTED \| REJECTED), DecisionRef, Utc}` (`DecisionRef` y `Utc` nullables = no aplica solo con PENDING). `DecisionRef` = `{Path, Marker}`: la ruta del archivo de decisiones de la unidad y el marcador de la decisión del Coordinator (§8.6), sin blob, para no crear ciclos. Transición única PENDING → ACCEPTED o REJECTED; ninguna otra |

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
| `relay-record/v2` | `TaskId`, `RunId`, `Attempt`, `Phase`, `Cession`, `Outcome`, `Disposition`, `Notes`, `RemoteFacts`, `Exit.OpenDelegations` (cuenta de delegaciones abiertas según B.8.2) | `Participant.Kind` → `{Role, BindingRef, Actor, Session, AdapterId, AdapterVersion, Transport (session-internal \| external-process), Observation[]}`; `ConfigToml*` → `Fingerprint`; `RebaseMap.G2CloseSha` → `MaterializationCloseSha` | `PrevRelaySha256` (diario encadenado; nullable = no aplica solo en el primero de la ventana); `WindowSeq`; `Exit.DelegationStatus` (B.8.2); `Phase` añade `TRANSFER` (T12a); `Host`; `TestArtifacts[].Skipped`; `Outcome.Kind` añade `LAUNCH_UNCERTAIN` |
| `controller-verification/v2` | todo | — | `Verifier {Role, BindingRef, Actor}`; `AuthorityResolution[]` (E.3, paso 6) |
| `gate-contract/v2` | todo | `EligibleCells` → `RoleRequirements[]` (único por `Role`; `{Role, Profile, Action, Mandatory[] (RequirementId), Independence[] (ReferenceRole + 4 dimensiones), EligibleCells[]}`) | `ProtocolSet`; `MaterializationCloseSha`; `SupersededCommits[] {Sha, WindowSeq}` y `SupersededBaseSha` (nullable = no aplica si la lista está vacía) (B.8.6) |
| `delegation/v2` | todo salvo `Owner.Kind` | `Owner.Kind` → `session-internal` \| `external-process` | `Executor.BindingRef`; `RoleRequirements[]` (copia, **nunca** menor); `SupersededCommits[]` (copia exacta del contrato) |

### B.8 `rackcad-automation-state/v2` (contrato semántico completo)

Archivo YAML `docs/automation/state/<unit>.yml`, solo para unidades I62. Las claves propias siguen el estilo `snake_case` de `/v1`. Las referencias
complejas no se incrustan: son `StateRef` (`{path, blob}`) a artefactos custodiados.

#### B.8.1 Campos, tipos y cardinalidad

| Campo | Tipo | Card. | Semántica |
|---|---|---|---|
| `schema` | const `rackcad-automation-state/v2` | 1 | — |
| `automation_state` | objeto | 1 | los **nueve campos** de AUTOMATION_PLAN §8 con sus tipos y reglas, **sin cambio**: `initiative`, `branch`, `claim_id` (inmutables); `current_phase`; `state`; `gate`; `attempts` (entero ≥ 0; en tareas delegadas sube +1 en el Q0 de una corrección, 16.8); `next_action`; `last_evidence_commit` |
| `protocol.set` | const `rackcad-protocol/I62` | 1 | una unidad I61 nunca usa `/v2` |
| `protocol.effective_sha` | SHA | 1 | `I62_EFFECTIVE_SHA` del repositorio |
| `protocol.basis.claim_id` | UUID | 1 | = `automation_state.claim_id`; **no** figura en la tabla PRE del merge efectivo |
| `protocol.basis.claim_commit` | SHA \| `null` | 1 | commit de reclamo **original** si es anterior al BOOTSTRAP; `null` = no aplica cuando el reclamo es el propio BOOTSTRAP (sin autorreferencia) |
| `protocol.basis.claim_parent_sha` | SHA | 1 | padre del commit de reclamo (existe antes del reclamo) |
| `protocol.basis.claim_parent_contains_effective` | bool | 1 | resultado de `git merge-base --is-ancestor <effective_sha> <claim_parent_sha>`; false = base obsoleta (exige prueba de orden en la decisión de G0) |
| `protocol.g0_acceptance.state` | PENDING \| ACCEPTED \| REJECTED | 1 | PENDING en BOOTSTRAP; **una sola transición** en un QU posterior (§8.6) |
| `protocol.g0_acceptance.decision` | `StateRef` \| `null` | 1 | entrada de `decisions/<unit>.md` con el marcador `I62-CLASSIFICATION`, custodiada en el mismo commit de la transición; `null` = no aplica solo con PENDING |
| `custody.record_version` | entero ≥ 1 | 1 | +1 exacto por punto durable (CAS) |
| `custody.point` | BOOTSTRAP \| Q0 \| Q7 \| QU \| QH \| QR | 1 | tipo del punto durable que es este commit (§8.1) |
| `custody.principal.state` | HELD \| RELEASED | 1 | RELEASED solo en QH |
| `custody.principal.binding` | `StateRef` | 1 | `binding/v1` custodiado: `Scope` UNIT, `Role` PRINCIPAL_COORDINATOR; su `Acceptance.State` = `principal.acceptance.state` |
| `custody.principal.acceptance.state` | PENDING \| ACCEPTED \| REJECTED | 1 | PENDING para el titular inicial hasta G0; un titular designado (QR, T12a) entra ACCEPTED; una transición por titular (§8.6) |
| `custody.principal.acceptance.decision` | `StateRef` \| `null` | 1 | entrada de decisiones con el marcador `I62-PRINCIPAL-BINDING` (o la designación), custodiada en el mismo commit de la transición; `null` = no aplica solo con PENDING |
| `custody.principal.preflight` | `StateRef` | 1 | `preflight/v1` custodiado del titular, `Action` CUSTODY |
| `custody.principal.designation` | `StateRef` \| `null` | 1 | decisión del Coordinator que designó al titular; nullable = no aplica solo para el titular inicial (`since_record_version` = 1) |
| `custody.principal.since_record_version` | entero ≥ 1 | 1 | punto en que el titular actual tomó la custodia |
| `custody.window.seq` | entero ≥ 0 | 1 | número de la última ventana abierta o por abrir; 0 = ninguna todavía |
| `custody.window.state` | CLOSED \| OPENABLE | 1 | OPENABLE ⇔ `point` = Q0 |
| `custody.task_intent` | objeto \| `null` | 0..1 | intención planificada; nullable = no aplica: no hay tarea planificada |
| `…task_intent.task_id` | string | 1 | — |
| `…task_intent.attempt` | entero ≥ 0 | 1 | = `automation_state.attempts` del mismo commit |
| `…task_intent.kind` | FIRST \| CORRECTION \| RERUN \| REISSUE | 1 | REISSUE = contrato reemitido (p. ej., 16.7 «antes de escribir») |
| `…task_intent.contract` | `StateRef` | 1 | `gate-contract/v2` custodiado en este punto |
| `…task_intent.continues_task_id` | string \| `null` | 1 | nullable = no aplica |
| `…task_intent.planned_roles[]` | `{role, binding: StateRef \| null}` | 1..n | único por `role`; incluye EXECUTION_CONTROLLER; `binding` nullable = no aplica: binding aún no aceptado |
| `custody.last_window` | objeto \| `null` | 0..1 | nullable = no aplica: ninguna ventana cerrada todavía |
| `…last_window.seq` | entero ≥ 1 | 1 | ventana cerrada |
| `…last_window.task_id`, `…attempt` | string, entero | 1 | — |
| `…last_window.closure` | VERIFIED \| REWORK \| BLOCKED \| STOP \| NOT_ACCEPTED \| WITHDRAWN \| ABANDONED | 1 | de la verificación (`Disposition` de 16.9) o del cierre declarado |
| `…last_window.closure_source` | CONTROLLER \| SESSION \| COORDINATOR | 1 | quién cerró: verificación, cierre declarado por la sesión, o decisión (ABANDONED, WITHDRAWN) |
| `…last_window.delegation_run_id` | string \| `"UNKNOWN"` \| `null` | 1 | `null` = no aplica (NOT_ACCEPTED, WITHDRAWN); `"UNKNOWN"` = la ventana se perdió sin poder establecerlo (B.8.5) |
| `…last_window.verification` | `StateRef` \| `null` | 1 | nullable = no aplica sin verificación |
| `…last_window.verified_sha` | SHA \| `null` | 1 | solo con VERIFIED |
| `…last_window.journal` | `StateRef` \| `null` | 1 | manifiesto de custodia del diario; nullable = no aplica solo en ABANDONED por diario no disponible |
| `…last_window.reconstruction` | `StateRef` \| `null` | 1 | registro y decisión de B.8.5; nullable = no aplica salvo en ABANDONED |
| `custody.chains[]` | objeto | 0..n | único por `task_id` |
| `…chains.task_id` | string | 1 | — |
| `…chains.state` | IN_COURSE \| VERIFIED \| ABANDONED | 1 | «cadena en curso» de 16.6 = IN_COURSE, incluida una corrección autorizada aún no emitida |
| `…chains.chain_base_sha` | SHA | 1 | Q0 de la primera ventana de la tarea (o su imagen, 16.7) |
| `…chains.chain_red_sha` | SHA \| `null` | 1 | RED vigente (16.8); nullable = no aplica: sin RED vigente |
| `…chains.chain_red_files[]` | ruta | 0..n | nunca decrece; vacío solo si la cadena nunca acreditó un RED |
| `…chains.continues_task_id` | string \| `null` | 1 | nullable = no aplica |
| `…chains.correction_authorized_pending` | bool | 1 | corrección autorizada aún no emitida |
| `custody.unverified_commits[]` | `{sha, task_id, window_seq, reason, superseded_by}` | 0..n | commits del Worker de una ventana abandonada (B.8.6); `reason` = JOURNAL_UNAVAILABLE; `superseded_by` = `DelegationRunId` de una delegación VERIFIED que los cubre, o `null` = no aplica todavía |
| `counters.correction_launches[]` | `{seq, task_id, failure_class, correction_of_run_id, attempts_after, record_version}` | 0..n | append-only; `seq` consecutivo desde 1; el contador por clase = número de entradas con el mismo (`task_id`, `failure_class`) |
| `counters.blocked_reruns[]` | `{task_id, phase, count}` | 0..n | único por (`task_id`, `phase`); `phase` ∈ PLANNING \| WORK \| VERIFICATION \| REVERIFICATION \| `NEGATIVE:<id>`; `count` ∈ 0..2 |
| `counters.rebase_recoveries[]` | `{task_id, count, last_rebase_map: StateRef}` | 0..n | único por `task_id`; `count` ∈ 0..2 |
| `counters.invocations[]` | `{scope, launched, uncertain, cap, cap_source: StateRef, reconstructed, reconstruction: StateRef \| null}` | 0..n | único por `scope` (gate del plan de la unidad); enteros ≥ 0; `reconstruction` nullable = no aplica si `reconstructed` = false |
| `execution_context` | mapa | 0..1 | descriptivo; **ningún lector del protocolo lo consume** |

#### B.8.2 Estado de delegación derivado (no se almacena)

`DS(L, J)`, con `L` = el último punto durable en `origin/<rama>` y `J` = el diario de la ventana `L.window.seq` (los `relay-record/v2` con ese `WindowSeq`,
encadenados por `PrevRelaySha256`):

| Condición | `DS` | Equivale en I-61 (16.6) |
|---|---|---|
| `L.window.state` = CLOSED y `L.task_intent` = `null` | NONE | ninguna delegación abierta |
| `L.window.state` = CLOSED y `L.task_intent` ≠ `null` | PLANNED | ninguna delegación abierta (solo intención) |
| OPENABLE; `J` íntegro; sin registro de aceptación | PLANNED | ninguna delegación abierta |
| OPENABLE; `J` íntegro; aceptación de `d`; sin verificación válida de `d` ni cierre declarado | **ACCEPTED_OPEN** | **delegación abierta** (aceptada, sin verificación válida ni cierre) |
| OPENABLE; `J` íntegro; aceptación de `d` + verificación válida o cierre declarado | CLOSED_PENDING_CUSTODY | ninguna delegación abierta; falta el Q7 |
| OPENABLE; `J` no disponible o con la cadena rota | UNKNOWN | posiblemente abierta: ninguna aceptación nueva (P-02); B.8.5 |

`Exit.OpenDelegations` = 1 si `DS` = ACCEPTED_OPEN, 0 si es NONE, PLANNED o CLOSED_PENDING_CUSTODY. Con UNKNOWN no hay Exit válido (STOP).

#### B.8.3 Durable en Q0, transitorio entre Q0 y Q7, durable en Q7

| Dato | Q0 (durable) | Q0 → Q7 (diario transitorio) | Q7 (durable) |
|---|---|---|---|
| `attempts` y `correction_launches` | +1 y entrada nueva si `kind` = CORRECTION | — | sin cambio |
| `task_intent` | fijado, con el contrato custodiado | — | `null` o la intención siguiente (decisión del Coordinator) |
| `window` | OPENABLE, `seq` = k | registros con `WindowSeq` = k | CLOSED, `seq` = k |
| bindings | los ya aceptados, en `planned_roles` | aceptación de bindings nuevos (Worker, Controller de verificación) | custodiados por el manifiesto |
| delegación | ninguna aceptada (hecho durable) | `d`, aceptación A1'-A8', cesiones, terminaciones, handoff, verificación, controles | `last_window` con el cierre y las referencias |
| reejecuciones, recuperaciones, invocaciones | valores durables previos | lanzamientos (`RunId`, `LAUNCH_UNCERTAIN`), reejecuciones, `RebaseMap` | actualizados desde el diario |
| `chains` | sin cambio | — | entrada nueva si la ventana tuvo la primera delegación aceptada de la tarea (`chain_base_sha` = su Q0), o actualizada: RED, `chain_red_files`, estado |
| `principal` | HELD | registro TRANSFER si ocurre T12a | titular vigente (nuevo tras T12a, con su designación) |
| `unverified_commits` | sin cambio | — | `superseded_by` fijado si la ventana termina en VERIFIED con `SupersededCommits` |

#### B.8.4 Invariantes semánticas

**De un archivo** (evaluables sobre el estado y el árbol de su commit, sin historia):

| Id | Invariante |
|---|---|
| I-S01 | `schema` = `/v2` ⇔ `protocol.set` = I62; `basis.claim_id` = `automation_state.claim_id`; `point` = BOOTSTRAP ⇒ `g0_acceptance.state` = PENDING ∧ `principal.acceptance.state` = PENDING |
| I-S02 | `automation_state` cumple AUTOMATION_PLAN §8 |
| I-S03 | `point` = Q0 ⇔ `window.state` = OPENABLE; `point` = Q0 ⇒ `task_intent` ≠ `null` ∧ `principal.state` = HELD |
| I-S04 | `principal.state` = RELEASED ⇔ `point` = QH |
| I-S05 | `task_intent.attempt` = `attempts`; `kind` = CORRECTION ∧ `point` = Q0 ⇒ existe una entrada de `correction_launches` con `record_version` = la del archivo y `attempts_after` = `attempts` |
| I-S06 | `planned_roles` único por `role` y con EXECUTION_CONTROLLER |
| I-S07 | `point` ≠ Q0 ⇒ (`last_window` = `null` ⇔ `window.seq` = 0) y `last_window.seq` = `window.seq`; `point` = Q0 ⇒ (`last_window` = `null` ⇔ `window.seq` = 1) y, si no, `last_window.seq` = `window.seq` − 1 |
| I-S08 | VERIFIED ⇒ `verification` y `verified_sha` ≠ `null`; NOT_ACCEPTED o WITHDRAWN ⇒ `delegation_run_id` = `null`; ABANDONED ⇔ `reconstruction` ≠ `null`; `journal` = `null` ⇒ ABANDONED |
| I-S09 | `chains` único por `task_id`; `chain_red_sha` ≠ `null` ⇒ `chain_red_files` ≠ ∅ |
| I-S10 | cada `unverified_commits.task_id` está en `chains`; `superseded_by` ≠ `null` ⇒ corresponde a una ventana VERIFIED registrada |
| I-S11 | unicidad y rangos de `blocked_reruns`, `rebase_recoveries` e `invocations`; `reconstructed` ⇔ `reconstruction` ≠ `null` |
| I-S12 | `correction_launches.seq` consecutivos desde 1; `attempts_after` estrictamente creciente y ≤ `attempts` |
| I-S13 | todo `StateRef` existe en el árbol del commit y su blob coincide |
| I-S14 | `principal.binding` es un `binding/v1` con `Scope` UNIT y `Role` PRINCIPAL_COORDINATOR cuyo `Acceptance.State` = `principal.acceptance.state`; `designation` = `null` ⇔ `since_record_version` = 1 |
| I-S15 | `point` = Q0 ⇒ `g0_acceptance.state` = ACCEPTED ∧ `principal.acceptance.state` = ACCEPTED ∧ (`basis.claim_parent_contains_effective` ∨ la rama contiene `effective_sha`) |
| I-S16 | `g0_acceptance.decision` = `null` ⇔ `g0_acceptance.state` = PENDING; `principal.acceptance.decision` = `null` ⇔ `principal.acceptance.state` = PENDING; cada `decision` no nula contiene el marcador correspondiente y el `Claim-Id` |

**De pares** (dos puntos durables consecutivos p → n de la rama, o sus imágenes por `RebaseMap`):

| Id | Invariante |
|---|---|
| I-P01 | `n.record_version` = `p.record_version` + 1 |
| I-P02 | transición permitida: BOOTSTRAP → {Q0, QU, QH, QR}; Q0 → {Q7, QR}; Q7 → {Q0, QU, QH, QR}; QU → {Q0, QU, QH, QR}; QH → {QR}; QR → {Q0, QU, QH, QR} |
| I-P03 | p = Q0 ⇒ n cierra la ventana `p.window.seq` (`n.last_window.seq` = `p.window.seq`), y entre p y n solo hay commits del Worker o imágenes del rebase de 16.7 (W-2) — requiere historia |
| I-P04 | n = Q0 ⇒ `n.window.seq` = `p.window.seq` + 1 |
| I-P05 | `attempts` no decrece y solo sube en un Q0 con `kind` = CORRECTION (+1) o, fuera de la ejecución delegada, según §8/§9 en un QU; `correction_launches`, `chain_red_files` y los contadores no decrecen; un QU no cambia `window`, `last_window`, `chains` ni los contadores de ventana |
| I-P06 | `protocol.set`, `protocol.effective_sha`, `protocol.basis` y los campos inmutables de `automation_state` no cambian |
| I-P07 | `principal.binding` cambia solo en QR (designación nueva), en el Q7 que cierra una ventana con TRANSFER (T12a), o en el QU de la transición de aceptación (mismo `BindingId`, versión con `Acceptance` decidida) |
| I-P08 | ningún commit del Worker modifica el archivo de estado (W-3) — requiere historia |
| I-P09 | `g0_acceptance.state` cambia como mucho una vez, de PENDING a ACCEPTED o REJECTED, y solo en un QU; `principal.acceptance.state` cambia como mucho una vez por titular (PENDING → ACCEPTED o REJECTED, en un QU); con un titular nuevo (QR, o Q7 tras T12a) entra ACCEPTED; la versión del binding cambia solo por esa transición o por el titular nuevo |

#### B.8.5 Reconstrucción cuando el diario no está disponible

**Fuentes admitidas:**
- la historia de `origin/<rama>`;
- el último punto durable `L` y los artefactos custodiados hasta `L`;
- las corridas de CI;
- los procesos de los hosts accesibles.

Las transcripciones y los registros de otras sesiones **no** se admiten. El hueco lo cubre el conteo conservador.

1. **`L` ≠ Q0:** no puede haber ventana (W-1). `DS` = NONE o PLANNED, y los contadores son los durables. **La reconstrucción es completa desde Git.** Es el caso
   de FX-04a en QH.
2. **`L` = Q0 (ventana k OPENABLE) y diario no disponible o roto:**
   - **a. Terminación:**
     - titular sin terminación acreditada → STOP P-12 (T11);
     - host inaccesible para comprobar participantes → STOP P-13 (T14);
     - si no, los procesos de la lista cerrada de 16.4 en ese host se comprueban sin participantes vivos.
   - **b. Commits posteriores a `L` en el remoto:**
     - **R-1:** ninguno;
     - **R-2:** solo commits cuyas rutas caben en el `AllowedWriteScope` del contrato de la intención y cuyo trailer corresponde a una celda del catálogo.
       Hubo un Worker y, por 16.5, una delegación aceptada;
     - **R-3:** cualquier otro → T10(c), STOP P-02/S-07.
   - **c. `DS` reconstruido:** R-1 → UNKNOWN_ACCEPTANCE; R-2 → ACCEPTED_OPEN con el paquete perdido, no verificable.
   - **d. Cierre:**
     - la decisión del Coordinator designa a N, y N publica un **QR** (CAS) que cierra la ventana k con `closure` ABANDONED, `closure_source` COORDINATOR,
       `delegation_run_id` = `"UNKNOWN"` (o el valor si un artefacto custodiado lo prueba) y `reconstruction` = el registro;
     - en R-2, los commits del Worker van a `unverified_commits` (B.8.6) y la cadena sigue IN_COURSE.
   - **e. Contadores, por fase de la ventana en orden (PLANNING, WORK, VERIFICATION, NEGATIVE):**
     - `launched_p` = los lanzamientos que prueba una fuente admitida (R-2 prueba al menos PLANNING = 1 y WORK = 1);
     - `uncertain_p` = 1 si la fase pudo lanzarse (es PLANNING, o la fase anterior está probada) y ninguna fuente la prueba ni la excluye; 0 en otro caso;
     - `reconstructed` = true;
     - el Coordinator puede fijar valores mayores en su decisión, **nunca menores**;
     - `attempts` no cambia en la reconstrucción: si el Q0 era una corrección, ya contó.
   - **f. Reejecución:** la siguiente ventana de la misma tarea cuenta como reejecución de la fase perdida (R-1: PLANNING; R-2: WORK) en `blocked_reruns`.
     La tercera → P-04.
3. **Diario íntegro hasta un registro y roto después**, o con un registro que contradice un hecho físico: S-04, y se trata como no disponible desde el primer
   registro inválido. Los anteriores se admiten.

#### B.8.6 Commits sin verificar y su sustitución

Mientras `unverified_commits` tenga entradas de la tarea T con `superseded_by` = `null`, se aplican tres reglas.

1. **Sin trabajo terminado:** ninguna verificación de T cuenta como «trabajo delegado terminado» (16.1, ampliado).
2. **Contrato de continuación:** la siguiente delegación de T exige un contrato con `SupersededCommits` = esas entradas y `SupersededBaseSha` = el Q0 de la
   ventana abandonada. Si no, el Coordinator decide con A-n.
3. **Verificación acumulada:** la verificación de esa delegación evalúa, además de lo de 16.9:
   - **`Scope` acumulado:** la unión de `git diff-tree --name-only` de cada commit de `SupersededBaseSha..CurrentSha` que no sea de la sesión ⊆
     `AllowedWriteScope`, sin intersección con `ForbiddenWriteScope`. Es commit de la sesión el que solo toca `docs/automation/` o los documentos de la
     unidad (16.1);
   - **`Trailer` de los sustituidos:** presente y de una celda del catálogo. La coherencia con su binding no es verificable y se registra como límite en
     `Evidence`;
   - **`Ci` y `Tests`:** sin cambio, sobre `CurrentSha`, que contiene el árbol acumulado;
   - **RED:** reglas de 16.8 con `chain_base_sha`, que precede a la ventana abandonada.

Con VERIFIED, el Q7 fija `superseded_by`. Con otro resultado, las entradas siguen pendientes.

## Anexo C — Matriz única: invariante → obligación → gate → entrada disponible → resultado → clase y autoridad

En cada gate, **entrada disponible** dice qué existe: datos de diseño (anexos de esta Proposal), materialización ejecutable (esquemas y procedimientos
publicados en ese gate) o el procedimiento ya disponible. Ninguna obligación presupone un gate futuro. Esperado independiente = la Proposal acordada (Freeze),
las reglas de I-61, o el oráculo del Coordinator (Anexo D). Nunca la salida del procedimiento bajo prueba.

| Id | Invariante | Obligación | Gate | Entrada disponible | Resultado esperado | Clase / autoridad |
|---|---|---|---|---|---|---|
| C-01 | roles sin proveedor | guarda: la tabla de roles de §16.1 y los enums del núcleo sin marcas (proveedores de los descriptores + patrones) | F1 | §16.1 materializado | 0 coincidencias; RED antes de materializar la tabla; mutation «Codex» detectada | (i) Core RG + mutation — AGENTS |
| C-02 | idem en el ADR | revisión del texto del ADR propuesto contra §2 | F1 | ADR formal propuesto | sin asociación rol↔proveedor | (ii) revisión del Coordinator |
| C-03 | tabla de estados fiel | guarda: la tabla normativa de §16 y la de ejemplos del README iguales, fila a fila, a §4.2 (Freeze) | F1 | §16 y README materializados | 0 diferencias; RED antes de materializar; mutation detectada | (i) Core RG + mutation (control de **fidelidad**, no de conducta) |
| C-04 | agregación correcta | aplicar el procedimiento de agregación a E1-E12 como **datos de diseño** (forma del Anexo B.4, **sin** validación de esquema) | F1 | procedimiento del README; datos de B.4 | `ConfigurationStatus`, `Causes`, `Disposition` = §4.2, por acción | (ii) MC |
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
| C-15 | custodia, CAS y **semántica del estado** | repositorio Git **local desechable** con SHAs reales locales: **arranque BOOTSTRAP → G0 → QU** (§8.6: aceptación, rechazo, G0 sin marcadores), F.1, F.2, F.3, F.5 (R-0..R-3), T12a frente a T12b, T16/T17 (QH → QR), QU entre ventanas, push rechazado por causas distintas. En **cada punto durable** se evalúan todas las invariantes de B.8.4 (de archivo, de pares y las que requieren historia: I-P03, I-P08), y en **cada paso** el `DS` de B.8.2 | F4 | state/v2, validador y procedimiento F4 | `DS`, invariantes y disposiciones = Anexo F, §8.6 y B.8; ninguna invariante violada en las secuencias válidas; cada mutación sembrada (Q0 omitido, Q0 con `g0_acceptance` PENDING, BOOTSTRAP que cita una decisión, aceptación inferida sin marcador, segunda transición de `g0_acceptance`, commit de la sesión dentro de la ventana, QU que toca `window`, `attempts` reiniciado, `record_version` saltado, ventana con dos Q0) detectada por la invariante que le corresponde | (ii) MC (sin invocar modelos) |
| C-16 | contadores | correcciones lanzadas frente a verificaciones; lanzamiento incierto; `ContinuesTaskId`; contador ausente o contradictorio; conteo conservador de B.8.5 | F4 | ídem | §9.3 y B.8.5; S-04 cuando corresponde | (ii) MC |
| C-17 | procesos | procesos locales ficticios (positivo: `pwsh` en la ruta; negativo: efímero) con el README §3.2 ampliado | F4 | procedimiento F4 | positivo detectado; efímero no vivo tras relectura | (ii) MC |
| C-18 | state/v2 **semánticamente válido** | guarda Core del validador: forma de B.8.1 + invariantes **de archivo** I-S01..I-S16 sobre archivos sintéticos (I-S13 contra el árbol de la prueba) + invariantes **de pares** I-P01, I-P02, I-P04..I-P07 e I-P09 sobre pares sintéticos; un caso positivo y uno negativo por invariante | F4 | formato y validador F4 | RED antes de materializar el validador; GREEN después; mutation por clase de invariante (omitir I-S03, omitir I-S15, invertir I-P02, aceptar `attempts` decreciente, permitir dos transiciones de `g0_acceptance`) detectada | (i) Core RG + mutation |
| C-19 | `/v1` intacto | guarda: blobs de los cinco `/v1` fijados; las pruebas de I-61 sin modificar | F2 | `/v1` | igualdad | (i) Core G |
| C-20a | resolver y mapa bien formados | guarda Core **sin historia**: el mapa materializado valida contra su esquema; unicidad de `Files` y `Entries`; `Surfaces` = la lista cerrada de §16.13; ENTRY ⊆ la lista cerrada; `EffBlob` = hash del contenido del árbol; punteros presentes en §16 y §16.3; §16.3 = texto de I-61 (fijado como dato de la prueba con su blob de origen) + puntero; encabezados únicos por archivo en las superficies | F4 | mapa, esquema y §16.13 materializados | RED antes de materializar; GREEN; mutation (entrada duplicada, puntero borrado, ENTRY extra) detectada | (i) Core RG + mutation |
| C-20b | mapa = derivación | MC con historia: validación MV-3..MV-6 de E.4 entre `origin/main` (base) y la punta en F4, y comparación con el mapa previsto de E.5; **repetida sobre el merge local** (`I62_EFFECTIVE_SHA^1` → merge) antes del push de la integración | F4 + integración | mapa y textos materializados | igualdad; cada diferencia entre lo previsto y lo derivado, clasificada como corrección o como A-n | (ii) MC + revisión del Coordinator |
| C-20c | resolución legacy **transparente** | MC: trazas de E.6 en un clon local desechable de RackCad con la historia real, el EFF local y M2 = EFF + X1 + X2 + X3. **C-20c-1:** el contrato `/v1` real de I-61 G3 **byte a byte**, antes de EFF y en la reverificación de 16.7 tras EFF. **C-20c-2:** el mismo contrato emitido para I-64 sin I-62 (`Path` y `Section` idénticos, documento completo incluido, sin citas nuevas). Negativos N-a..N-m | F4 | resolver, mapa y reglas F4 | tablas de E.6, cita por cita y unidad por unidad en las lecturas compuestas; ningún contrato reemitido; fallos con su id | (ii) MC |
| C-21 | MaterializationClose | cambio normativo posterior a MC_I62 → invalidación | F4 | regla F4 | MC_I62 nuevo requerido | (ii) revisión del Coordinator |
| C-22 | fixture arrancable | traza D.1 hasta un contrato I62 válido | F6 | fixture | D.1 | (ii) FX |
| C-23 | autoverificación (FX-01) | D.3 | F6 | fixture | D.3 | (ii) FX + OV |
| C-24 | topología A (FX-02) | D.3 | F6 | fixture + OD | D.3 | (ii) FX + OV |
| C-25a | portabilidad de reanudación y decisión (FX-04a) | D.3, D.6 | F6 | fixture + OD-5 | PASS por comparación mecánica con el oráculo; FAIL ante diferencia o lectura prohibida; UNVERIFIED si falta una precondición | (ii) FX + OV |
| C-25b | continuación (FX-04b) | D.3 | F6 | fixture + OD de la acción | PASS (VERIFIED sobre G' y Q7 conforme a B.8.4), FAIL, UNVERIFIED o UNSUPPORTED (D.4) | (ii) FX + OV |
| C-26 | topología B (FX-03) | D.3 | F6 | ídem | D.3 o limitación | (ii) FX + OV |
| C-27 | plano (c) sin efecto real (FX-05) | D.3 | F6 | ídem | rechazo; estado real intacto | (ii) FX |

## Anexo D — Pilotos y portabilidad (plano c)

### D.1 Arranque del fixture (análisis; nada se crea ahora)

Lo crea la sesión de supervisión (plano a) al empezar F6, con OD-5 y OD-7:
1. `fixture-origin.git` (remoto bare local) y, con OD-7, un remoto en GitHub del Owner con CI.
2. `main` del fixture, commit **F_seed**:
   - copias **byte a byte** de las autoridades de RackCad en MC_I62 (WORKFLOW, LIFECYCLE, AUTOMATION_PLAN, agent-execution, esquemas, `compatibility/`);
   - un **AGENTS del fixture** marcado `FIXTURE_LOCAL`, que declara los **jobs requeridos del fixture** (`fixture-build`, `fixture-tests`);
   - una biblioteca .NET mínima y su CI;
   - `FIXTURE-MANIFEST.json`: ruta → {`COPIED` con blob de origen en MC_I62 | `FIXTURE_LOCAL` con su razón} + commit de origen.
3. Rama `fixture/i62-norm` con el commit **F_norm**, que lleva el trailer `Agent-Protocol-Normative: I-62`, fusionada en el `main` del fixture como
   **F_eff**. Por la regla de §14, `I62_EFFECTIVE_SHA` del **fixture** = F_eff, marcado `TEST-ACTIVATION` en el manifiesto. La derivación se aplica **por
   repositorio**: en RackCad no hay activación hasta la integración real, así que el plano (a) no puede seleccionar I62.
4. Unidad de prueba **FX-U1**: reclamo en `fx/u1` desde F_eff (Claim-Id propio) y push al remoto del fixture → clase POSTERIOR_DEMOSTRADA en el fixture →
   protocolo I62.
5. Bootstrap de FX-U1 con el arranque de §8.6:
   - `state/v2` en BOOTSTRAP con la evidencia de `protocol.basis`, `g0_acceptance` PENDING y el binding del Principal propuesto (PENDING);
   - el Coordinator del fixture decide G0 con los dos marcadores;
   - el QU registra las aceptaciones;
   - contrato. MaterializationClose = bootstrap (sin cambios normativos).
6. `gate-contract/v2` de la tarea T1: `ProtocolSet` I62 = protocolo de la unidad; `AuthorityRevision` = bootstrap de FX-U1; `EXTERNAL` = `main` del fixture
   (F_eff). **Contrato válido** (A1'/A6').

### D.2 Semántica de `Ci` (se resuelve antes del Freeze)

| Regla `/v1` (16.9) | Regla I62 | Tipo de cambio |
|---|---|---|
| «Corrida `push` de `CurrentSha` con `ref` exacta y los **cuatro jobs requeridos de `AGENTS.md`** en `success»; RED con el job Core en `failure` | «… y **los jobs requeridos por el `AGENTS.md` del repositorio de la unidad** en `success`; RED con el job de pruebas designado por ese `AGENTS.md` en `failure`» | **sustituido**: en RackCad significa exactamente lo mismo (cuatro jobs; Core); en el fixture, sus dos jobs. Requiere aprobación en el Freeze (Coordinator + Architect) |

- **Positivo:** push de G en `fx/u1` → corrida `push`, `refs/heads/fx/u1`, `head_sha` G, `fixture-build` y `fixture-tests` en `success` → `Ci` pass.
- **Negativo:** `fixture-tests` en `failure` en la corrida de G → `Ci` fail → REWORK tras leer los logs.
- **RED:** la corrida de R con `fixture-tests` en `failure` → `RedPart` pass.

La evidencia del fixture **nunca** sustituye las cuatro corridas de RackCad.

**Sin OD-7:** `Ci` = `not_run` → ninguna verificación VERIFIED → FX-02 y FX-04b no pueden ser PASS → **F6 no se cierra**: queda pendiente con UNVERIFIED y
vuelve al Owner. Aceptar una limitación no convierte en PASS una verificación incompleta. FX-04a no usa `Ci`.

**Matriz de evidencia:**

| Evidencia | Fuente | Qué acredita |
|---|---|---|
| real | CI de RackCad sobre los SHAs de I-62 | los gates reales |
| del ensayo | CI del fixture sobre los SHAs del fixture | la conducta del protocolo en el plano (c) |
| limitada | escenarios UNVERIFIED o UNSUPPORTED con causa | nada más que su límite |

### D.3 Hoja de invocaciones y escenarios

**Topología A** (FX-01, FX-02, FX-05):

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

**FX-04a — reanudación y decisión** (sigue a FX-02 en la misma unidad FX-U1; si FX-02 no llegó a su Q7, parte de un QH tras BOOTSTRAP con T1 planificada,
con hechos más pobres y registrado así):
1. **A llega a un punto quiescente canónico.** Tras el Q7 de FX-02, el Coordinator del fixture emite el contrato de la tarea T2. A publica **QH** por CAS:
   `task_intent` = T2 (`Attempt` vigente, FIRST, contrato custodiado, Controller de planificación con binding aceptado o pendiente), `principal.state` =
   RELEASED y `window` CLOSED.
2. **A termina.** La terminación la acredita la sesión de supervisión (metadatos `isRunning` de la app) o la atesta el Owner (§9.1). Sin ninguna de las dos →
   FX-04a UNVERIFIED.
3. **Oráculo fuera del host.** El Coordinator lo prepara desde el estado canónico en QH y publica solo su SHA-256 en la evidencia real **antes** de que B empiece.
   Contenido:
   - **hechos:** rama, Claim-Id, último punto (QH) y su `record_version`, protocolo, titular liberado, `attempts`, `correction_launches`, contadores,
     cadenas con su RED, `last_window` de FX-02 con su `VerifiedSha`, `task_intent` T2 y los STOP vigentes;
   - **siguiente decisión:**
     - QR (custodia por CAS) → Q0 de la ventana k+1 → CONTROLLER_PLANNING de T2, con el `Attempt` vigente y `ProtocolSet` I62, sin Worker todavía;
     - binding del Controller: REUSE si los invalidadores de su `PreflightRef` no cambian desde el host y el runtime de B; si cambian, re-observar y REBIND
       con los requisitos del contrato.
4. **B empieza sin contexto privado.** El Owner abre B (`codex-desktop-session`; una sesión de Claude nueva es una variante de mismo proveedor que se registra
   aparte) en un clon limpio del fixture en QH, con solo las entradas de D.6.
5. **Preflight de B para la acción RESUME_DECISION** (§4.1: nivel, effort, `remote-facts` de lectura, introspección ≥ RUNTIME_OBSERVED; **no** `repo-write`).
   BELOW_REQUIRED o UNKNOWN → FX-04a UNVERIFIED.
6. **Respuesta y publicación previa al oráculo.** B produce hechos, siguiente decisión, entradas enumeradas y su propio preflight. La sesión de supervisión
   registra el SHA-256 de la respuesta en la evidencia real; si B tiene `repo-write` para esa acción, además la publica en la rama del fixture **sin tomar la
   custodia**. El oráculo no se entrega antes de ese registro.
7. **Comparación mecánica** campo a campo:
   - hechos, con igualdad exacta;
   - decisión: tipo de acción, `TaskId`, `Attempt`, rol, disposición del binding, rama condicional coherente con las observaciones registradas de B,
     `ProtocolSet`, precondiciones y STOP.

   **PASS** solo con igualdad total y aislamiento no UNVERIFIED (D.6). **FAIL** ante una diferencia o una lectura prohibida registrada.
8. **N11, hecho retirado:** una segunda sesión B2 sobre un clon en QH al que se retira del estado canónico un hecho (p. ej., una entrada de
   `correction_launches`). Esperado: B2 declara UNKNOWN o STOP sobre ese hecho, nunca el valor oculto.

PASS de FX-04a **no** exige que B ni ningún Worker que B pueda lanzar sean capaces de ejecutar la decisión.

**FX-04b — continuación** (desde la respuesta de FX-04a; ejecuta B o el Principal que designe el Coordinator):
1. preflight de B para CUSTODY (`repo-write`); designación (T16); **QR** por CAS;
2. **Q0** de la ventana k+1; planificación (Controller `codex-cli`); aceptación;
3. Worker: B no puede lanzar subagentes de Claude, así que se hace rebinding a `codex-cli` con escritura (OD-4) o a otro adapter que B pueda lanzar con
   escritura acreditada;
4. verificación del G' producido bajo B → VERIFIED sobre G' → **Q7**.

| Resultado | Cuándo |
|---|---|
| **PASS** | VERIFIED sobre G' y un Q7 conforme a B.8.4 |
| **FAIL** | violación observada del protocolo (p. ej., ventana sin Q0, escritura de la sesión dentro de la ventana, contadores reiniciados) |
| **UNVERIFIED** | falta una OD o una capacidad sin medir |
| **UNSUPPORTED** | capacidad medida ausente (p. ej., la escritura de Codex denegada por el sandbox) |

Un UNVERIFIED o UNSUPPORTED de FX-04b **no** altera el resultado de FX-04a.

| Escenario | Rol | Lanzamientos | Reintentos | Tope |
|---|---|---|---|---|
| FX-04a | Principal B | 1 sesión | +1 reapertura | 2 |
| FX-04a | N11: B2 | 1 sesión | 0 | 1 |
| FX-04a | invocaciones de modelo | **0** | — | 0 |
| FX-04b | Principal B | continúa la sesión de FX-04a (comparte su reapertura) | — | — |
| FX-04b | Controller (`codex-cli`) | planificación 1 + verificación 1 | pool 2 | 4 |
| FX-04b | Worker (`codex-cli`, escritura) | 1 | +1 si hay REWORK | 2 |

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
| FX-04a | 0 | 0 | 0 | ≤ 2 (+1 B2) |
| FX-04b | ≤ 4 + Worker 2 = 6 | 0 | 0 | 0 nuevas (sesión de FX-04a) |
| B (FX-03) | ≤ 8 + Worker 2 + sonda 2 = 12 | 0 | ≤ 4 + sonda 2 | ≤ 2 |

Ningún tope autoriza gasto ni ejecución ahora.

### D.4 Resultados y cobertura

| Resultado | Cuándo |
|---|---|
| **PASS** | ejecutado y conforme al oráculo |
| **FAIL** | violación observada; se conserva; impide cerrar F6 hasta la corrección (§16) y la repetición dentro de los topes; nunca se convierte en limitación |
| **UNVERIFIED** | falta una precondición (OD, capacidad sin medir, registro incompleto) |
| **UNSUPPORTED** | capacidad medida ausente |

**Matriz de cierre de F6:**

| Escenario | Necesita | Si falta |
|---|---|---|
| FX-01 PASS | OD-5 + sesión A | UNVERIFIED; F6 pendiente |
| FX-02 PASS | OD-5 + **OD-7** + OD-2 + medición del binario | UNVERIFIED; F6 pendiente |
| **FX-04a PASS** | OD-5 + QH alcanzado por A + terminación de A acreditada (observador externo u Owner) + B en MATCH o ABOVE_REQUIRED para RESUME_DECISION + aislamiento no UNVERIFIED | UNVERIFIED; F6 pendiente. **No** depende de OD-3, OD-4, OD-7 ni de la escritura de B |
| **FX-04b** | OD-5, OD-2, OD-4, OD-7, `repo-write` de B para CUSTODY, Controller y Worker lanzables desde B | PASS, o UNVERIFIED/UNSUPPORTED con causa para la decisión del Owner (OV-I62-05 b); el escenario no se retira. FAIL impide cerrar F6 |
| FX-05 PASS | OD-5 | UNVERIFIED; F6 pendiente |
| FX-03 | OD-3, OD-4, OD-2, Principal B | PASS, o UNVERIFIED/UNSUPPORTED con causa para la decisión del Owner (OV-I62-04); el escenario no se retira |

**Límites separados por topología, rol y escenario:**
- una limitación de `claude-cli` afecta al Reviewer y al Architect de B;
- una limitación de escritura de Codex afecta a FX-04b y a FX-03; no a FX-04a;
- ninguna limitación de B demuestra que el Principal B sea incapaz de reconstruir ni acredita la portabilidad: eso solo lo decide FX-04a.

### D.5 Ejecución compacta para OV sobre FINAL_CANDIDATE_SHA

Se resiembra el fixture desde el Candidato y se ejecutan:
- FX-01;
- FX-02, pasos hasta VERIFIED: Codex planificación 1 + verificación 1 + nc1 1 + pool 2 = 5; Worker 1 (+1); Principal 1;
- **FX-04a**: Principal B 1 sesión; 0 invocaciones (OV-I62-05 a);
- **FX-04b**, solo con sus OD concedidas: Codex 2 + pool 1; Worker 1 (OV-I62-05 b).

FX-03 solo con las OD concedidas.

### D.6 Entradas y aislamiento (cobertura declarada)

| Clase de entrada | Contenido | Tratamiento |
|---|---|---|
| **Canónicas permitidas** | clon del fixture en el SHA del punto de relevo (QH); documentos del fixture, incluido el diario de FX-02 ya custodiado en Git | enumeradas en la respuesta de B |
| **Dependencias técnicas permitidas** | binarios y runtime; su configuración y autenticación propias (para el preflight); SDK, Git y GitHub CLI; **entradas automáticas del runtime** (p. ej., instrucciones globales de usuario, `AGENTS.md`/`CLAUDE.md` del directorio, memoria del proyecto) | enumeradas con su ruta y su **SHA-256** antes de que B empiece; ninguna puede contener hechos de la unidad. La memoria de proyecto de un directorio nuevo se comprueba vacía |
| **Privadas prohibidas** | transcripciones y sesiones de otras sesiones (`~/.claude/projects/*` de otros directorios, `~/.codex/sessions/*` ajenas), worktree real de I-62, `D:\IDs`, artefactos transitorios de A, memoria de otras sesiones | — |

En QH no hay ventana abierta: todo lo que B necesita está en Git. Un artefacto transitorio de A que B leyera sería entrada privada.

**Auditoría:** comandos del registro de sesión de Codex y llamadas a herramientas de la transcripción de Claude. **Cobertura declarada:** el registro no
garantiza capturar lecturas internas del runtime.

| Hallazgo | Resultado |
|---|---|
| lectura registrada de una entrada prohibida | **FAIL** de aislamiento; ensayo inválido |
| entrada automática no enumerada, o registro sin lecturas | aislamiento **UNVERIFIED**; FX-04a no puede ser PASS |
| registro parcial | no es prueba completa de aislamiento |

El oráculo, fuera del host antes de la respuesta, protege el esperado; **no** prueba la ausencia de memoria heredada. Eso lo cubren la enumeración de entradas
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
| `codex-cli` | sí | — | escritura (B, FX-04b) | sí | `Ci` |
| `codex-desktop-session` | si comparte `config.toml` (F2) | por medir | — | sí | — |
| `claude-cli` | según su huella | sí | — | sí | — |
| `claude-desktop-session` / `claude-subagent` | según su huella | — | — | sí | — |

## Anexo E — Resolución de autoridades para unidades I61 (ejecutable)

La política de compatibilidad por superficie y cláusula se conserva. Este anexo la convierte en un algoritmo normativo que no edita unidades I61, no muta los esquemas `/v1` y
no congela archivos enteros.

### E.1 Componentes, ubicación e identidad

| Componente | Ubicación tras la integración | Se lee en | Identidad |
|---|---|---|---|
| Resolver | AUTOMATION_PLAN, subsección nueva **`### 16.13 Compatibilidad de protocolos de ejecución delegada`**, al final de §16 | `MainSha`, por **toda** unidad (independiente del protocolo) | su texto en `MainSha`; con `I62_EFFECTIVE_SHA` presente y §16.13 ausente → fallo cerrado |
| Punteros | primera frase de §16 («Antes de aplicar esta sección, toda unidad aplica §16.13») y última frase de §16.3 | `MainSha` (descubrimiento) | guarda C-20a |
| Mapa de cláusulas | `docs/automation/agent-execution/compatibility/I62-clause-map.json` | **`I62_EFFECTIVE_SHA`**, nunca `MainSha` | `git rev-parse <I62_EFFECTIVE_SHA>:<ruta>`: ruta fija en un commit inmutable. El tag `integration/I-62` repite el blob. §16.13 no cita el blob: el mapa contiene el blob de AUTOMATION_PLAN y la cita crearía un ciclo |
| Esquema del mapa | `docs/automation/agent-execution/compatibility/clause-map.schema.json` (`rackcad-clause-map/v1`) | `I62_EFFECTIVE_SHA` | ruta fija; el mapa lo lista como ENTRY con su blob |
| Tabla PRE | cuerpo del commit `I62_EFFECTIVE_SHA` (precedente: WORKFLOW §11.3) | `git show -s --format=%B <I62_EFFECTIVE_SHA>` | el propio commit |
| Tabla POST | informe posterior al merge y tag | solo para casos DESCONOCIDA | tag |

§16.3 en `main` = el texto de I-61, **literal**, + la frase puntero. La guarda lo comprueba. Las reglas de autoridad propias de I62 (MaterializationClose,
`ProtocolSet`) viven en las subsecciones nuevas, no en §16.3.

### E.2 Clasificador

```text
Classify(unidad u, MainSha_eval):
 1. EFF := Derive(MainSha_eval)                     -- primer merge first-parent con el trailer único (§14)
    ninguno            → PRE_ACTIVATION (esa main no contiene I-62: lectura de I-61 sin resolver)
    duplicado/ambiguo  → ACTIVATION_INVALID → STOP al Owner
 2. cid := claim_id del estado de u en BaseSha (o en la punta de su rama fuera de una tarea)
 3. PRE := tabla del cuerpo de EFF; mal formada o con Claim-Id duplicado → ACTIVATION_INVALID
 4. cid aparece exactamente una vez en PRE                                   → I61 (ANTERIOR_DEMOSTRADA)
 5. si no, estado de u en /v2 con protocol.set = I62, protocol.effective_sha = EFF y basis.claim_id = cid:
      g0_acceptance.state = PENDING                                          → PENDING_G0
      g0_acceptance.state = ACCEPTED y decision válida
        (blob presente; marcador `I62-CLASSIFICATION: I62` y cid)            → I62 (POSTERIOR_DEMOSTRADA; base obsoleta
                                                                                si basis.claim_parent_contains_effective = false)
      g0_acceptance.state = REJECTED, o decision inválida                    → paso 6
 6. si no: decisión del Coordinator para cid, con marcador `I62-CLASSIFICATION: I61|I62`
    y su evidencia (WORKFLOW §11.3), emitida tras un STOP                     → ese valor
 7. si no                                                                    → UNKNOWN
```

**Conducta antes y después de la transición de G0** (fallo cerrado, sin ambigüedad):

| `g0_acceptance.state` | Resultado | Contratos y delegaciones | Puntos durables admitidos |
|---|---|---|---|
| PENDING (desde BOOTSTRAP hasta el QU de §8.6) | PENDING_G0 | STOP (P-15) | QU, QH, QR (I-S15 prohíbe Q0) |
| ACCEPTED | I62 | permitidos; con base obsoleta, STOP de delegaciones mientras la rama no contenga `effective_sha` | todos |
| REJECTED | UNKNOWN, salvo una decisión posterior (paso 6) | STOP | QU, QH, QR |

- **Quién:** el Coordinator, en el G0 de cada unidad nueva (acepta o rechaza la evidencia del BOOTSTRAP) y antes de emitir cualquier contrato posterior a EFF.
  El Coordinator al aceptar y el Controller en `Authority` la vuelven a aplicar sobre las mismas fuentes; una discrepancia es S-04.
- **Durabilidad:** nunca se rederiva por ascendencia actual (WORKFLOW §11.1). Las fuentes son la tabla PRE, el `protocol` del estado `/v2` (evidencia del
  BOOTSTRAP más la aceptación registrada) o una decisión registrada.
- **Unidades I61:** se clasifican sin escribir en su rama.

### E.3 Resolver (transparente para los contratos I61)

**Entradas:**
- el contrato K (`gate-contract/v1` o `/v2`), **tal como se emitió**;
- `AR` = `K.AuthorityRevision`;
- `MainSha_eval` = `K.MainSha`, salvo en una reverificación de 16.7 sin conflictos. Ahí es el `MainSha` nuevo del `RebaseMap` (16.7: «las comprobaciones usan
  las imágenes y el MainSha nuevo»), y el contrato no cambia.

El contrato **no** necesita citar §16.13 ni el mapa, ni campos nuevos, ni reemitirse: el resolver lo aplica quien lee el contrato.

```text
Resolve(K, MainSha_eval):
 0. Descubrimiento: EFF := Derive(MainSha_eval).
    PRE_ACTIVATION → lectura de 16.3 de I-61 sin cambios (fin del resolver).
    EFF presente y §16.13 o punteros ausentes en MainSha_eval → fallo cerrado (S-12).
 1. P := Classify(K.Unit, MainSha_eval). UNKNOWN o PENDING_G0 → STOP. P = I62 con K /v1, o P = I61 con K /v2 → P-15.
 2. M := mapa en EFF (ruta fija); válido contra el esquema en EFF; Validate(M) (E.4).
    Cualquier fallo → MAP_INVALID → `Authority` fail; STOP (S-12; P-15).
 3. Para cada cita a = (Path, Section, Class) de K.Authorities, en el orden del contrato:
      Class ∈ {UNIT_DOC, UNIT_CHANGE}            → AR                               (sin cambio)
      EXTERNAL y P = I62                         → MainSha_eval                     (normal)
      EXTERNAL y P = I61                         → R61(Path, Section)
 4. Comprobaciones de 16.3 con el texto de §16.3 resuelto (para I61, el de EFF^1), donde
    «se lee en MainSha» se lee como «se lee en la revisión resuelta en el paso 3».
    MB = merge-base(MainSha_eval, AR) y las demás comprobaciones de 16.3, sin cambio.
 5. Registro: lista ordenada (Path | Section | Class | NORMAL|COMPAT|COMPUESTA|ENTRY | revisión, o revisión por unidad si es compuesta)
    + EFF + blob del mapa + clase. /v1: `Authority.Evidence` de la verificación. /v2: `AuthorityResolution[]` (B.7).

R61(Path, Section):
 a. Path = ruta del mapa, o archivo con FileKind ENTRY                  → EFF
 b. Path con FileKind ADDED (no existía en EFF^1)                       → NOT_APPLICABLE → fallo (P-15)
 c. Path con FileKind MODIFIED:
    c1. Section = "documento completo":
          archivo Markdown                                             → LECTURA COMPUESTA (abajo)
          otro archivo                                                 → EFF^1, entero (compromiso declarado)
    c2. (Path, Section) con Kind ENTRY                                 → MainSha_eval          (§16.13)
    c3. Match(EFF^1, Path, Section) = un único h:
          (Path, h) con Kind MODIFIED o REMOVED                        → EFF^1                 (compatibilidad)
          si no, Match(MainSha_eval) = uno                             → MainSha_eval          (normal)
          si no                                                        → fallo (S-12)
    c4. Match(EFF^1) = 0:
          Match(EFF) = uno, con Kind ADDED                             → NOT_APPLICABLE → fallo (P-15)
          Match(EFF) = 0 y Match(MainSha_eval) = uno                   → MainSha_eval  (sección posterior a EFF, de otra iniciativa)
          cualquier otro caso                                          → fallo (S-12)
    c5. Match(EFF^1) > 1                                               → fallo (S-12)
 d. Path fuera de M.Files:
      dentro de M.Surfaces                                             → MainSha_eval          (I-62 no lo modificó)
      fuera: blob(EFF^1,Path) = blob(EFF,Path) → MainSha_eval; si difiere → fallo (S-12)
```

**Lectura compuesta de «documento completo»** (archivo Markdown modificado por I-62). La cita se conserva tal cual y se lee como la lista de sus secciones en el
sentido de 16.3, el preámbulo y cada `##`:

| Unidad de lectura | Revisión |
|---|---|
| preámbulo, o `##` de EFF^1 con Kind MODIFIED o REMOVED | EFF^1 (texto de I-61) |
| preámbulo, o `##` de EFF^1 sin entrada en el mapa | `MainSha_eval`; si ya no existe allí, se omite, como en la lectura de I-61 de un documento completo en `MainSha` |
| `##` con Kind ADDED (solo I62) | **omitida**: no forma parte de la lectura I61 |
| `##` presente en `MainSha_eval` y ausente de EFF (posterior a EFF, de otra iniciativa) | `MainSha_eval` |

Orden: el de EFF^1, seguido de las secciones posteriores a EFF en el orden de `MainSha_eval`. El registro (paso 5) enumera cada unidad con su revisión.

**Compromiso declarado:**
- una sección `##` que I-62 modificó se lee **entera** en EFF^1, incluidas sus subsecciones no tocadas, que dejan de evolucionar para las unidades I61. Lo
  mismo vale para la cita individual de esa sección;
- un archivo modificado que no es Markdown se lee entero en EFF^1;
- las secciones solo I62 no se leen;
- todo lo demás sigue la evolución normal de `MainSha`, exactamente como en I-61.

La alternativa de leer en EFF^1 el documento completo entero se descarta: congelaría para las unidades I61 secciones que I-62 no tocó.

`Match(rev, Path, Section)`:
- «preámbulo» designa el texto anterior al primer `##`;
- si no, cuenta los encabezados del archivo en `rev` cuya línea, normalizada, es igual a `Section` normalizado. Normalizar = espacios colapsados y extremos
  recortados;
- se ignoran las líneas dentro de bloques de código.

**Significado `/v1` conservado:**
- `MainSha` sigue siendo `origin/main` al emitir. A6 lo compara con un `origin/main` recién obtenido, la comprobación `Remote` lo usa y `MB` se calcula con él
  (o con el nuevo, en la reverificación de 16.7, como ya hace I-61).
- `AuthorityRevision` sigue siendo el commit de los documentos y cambios propios de la unidad.
- Lo único que cambia, y solo para unidades I61 y cláusulas del mapa, es la revisión en la que se lee el texto `EXTERNAL`.

**Cuándo se ejecuta:**
- el **Controller**, en `Authority` (16.9 #3), antes de las comprobaciones de 16.3;
- el **Coordinator**, al evaluar A1-A8 antes de invocar al Worker (16.5), puede ejecutarlo para anticipar un fallo, sin cambiar el contrato;
- la **sesión** I61 puede registrar en `Notes` del relevo la clase y `EFF`, sin campos nuevos.

**Cómo lo descubre quien lee un contrato I61** (el autor del contrato no necesita conocer I-62):
1. Su propia regla (16.3 de I-61) lee AUTOMATION_PLAN §16 como `EXTERNAL` en `MainSha`. I-61 está cerrada, así que §16 no es `UNIT_CHANGE` de ninguna otra
   unidad. Tras EFF, la primera frase de ese §16 remite a §16.13.
2. El resolver reenvía §16 a `EFF^1` para gobernar la delegación. §16.13, independiente del protocolo, se sigue leyendo en `MainSha`: es el punto fijo y no
   hay regresión.
3. Una delegación en curso que cruza EFF no exige nada nuevo:
   - antes de escribir, el avance de `main` ya obliga a reemitir por 16.7 (S-13), igual que con cualquier otro avance de `main`, y el contrato reemitido conserva
     sus citas;
   - después de escribir, la reverificación de 16.7 usa el `MainSha` nuevo con el contrato intacto.

### E.4 Formato del mapa y validación con fallo cerrado

```json
{
  "Schema": "rackcad-clause-map/v1",
  "Protocol": "rackcad-protocol/I62",
  "LegacyProtocol": "rackcad-protocol/I61",
  "Surfaces": ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md",
               "docs/WORKFLOW.md", "docs/adr/", "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"],
  "Files":   [ { "Path": "...", "FileKind": "MODIFIED | ADDED | ENTRY", "BaseBlob": "<40 hex> | null", "EffBlob": "<40 hex>" } ],
  "Entries": [ { "Path": "...", "Section": "<línea de encabezado exacta | preámbulo>", "Level": 0, "Kind": "MODIFIED | REMOVED | ADDED | ENTRY" } ]
}
```

`Surfaces` es una lista cerrada que también figura en el texto de §16.13. `Files` excluye el propio mapa. `BaseBlob` es nullable (no aplica) solo con ADDED o
ENTRY. `Level` 0 corresponde al preámbulo.

**`Validate(M)`, en EFF:**

| Id | Regla |
|---|---|
| MV-1 | el mapa existe en EFF en la ruta fija, se interpreta como JSON y valida contra su esquema en EFF |
| MV-2 | `Surfaces` = la lista cerrada de §16.13 |
| MV-3 | **completitud de archivos:** {p bajo `Surfaces` : blob(EFF^1, p) ≠ blob(EFF, p)} ∖ {mapa} = {`Files.Path`}, sin duplicados; un archivo borrado no tiene clase → inválido |
| MV-4 | MODIFIED ⇒ `BaseBlob` = blob(EFF^1, p) ∧ `EffBlob` = blob(EFF, p); ADDED o ENTRY ⇒ `BaseBlob` = `null` ∧ `EffBlob` = blob(EFF, p); ENTRY de archivo ⊆ {esquema del mapa} |
| MV-5 | `Entries` único por (`Path`, `Section`), con `Path` de un archivo MODIFIED |
| MV-6 | **igualdad con la derivación**, por archivo MODIFIED p, con H1 = secciones en EFF^1 y H2 = secciones en EFF (todos los niveles + preámbulo): MODIFIED = {h ∈ H1 ∩ H2 : texto normalizado distinto}; REMOVED = H1 ∖ H2; ADDED ∪ ENTRY = H2 ∖ H1; ENTRY ⊆ {(AUTOMATION_PLAN, `### 16.13 …`)}; encabezados únicos por archivo en ambas revisiones |
| MV-7 | punteros presentes en §16 y §16.3 en EFF; §16.3 en EFF sin el puntero = §16.3 en EFF^1 (normalizado) |

**Sección y normalización:** como 16.3. Una sección va desde su encabezado hasta el siguiente con el mismo número de `#` o menos, e incluye sus subsecciones. El
texto se normaliza con CRLF → LF y espacios colapsados.

| Defecto | Efecto |
|---|---|
| mapa ausente, ilegible o inválido contra su esquema | MAP_INVALID |
| entrada o archivo duplicado | MAP_INVALID |
| archivo modificado ausente de `Files`, o listado sin modificarse | MAP_INVALID |
| `BaseBlob` o `EffBlob` distintos de los observados | MAP_INVALID |
| entrada que contradice la derivación (MODIFIED con texto igual; modificada no listada; ADDED que existe en EFF^1; REMOVED que existe en EFF) | MAP_INVALID |
| ENTRY fuera de la lista cerrada; encabezados duplicados | MAP_INVALID |
| §16.13 o punteros ausentes con EFF presente | MAP_INVALID |

**Con MAP_INVALID, ningún contrato I61 pasa `Authority` tras EFF.** STOP (S-12; P-15) al Coordinator. La corrección es un cambio normativo con su propia
autoridad, nunca un parche local del resolver.

### E.5 Mapa previsto (política del Freeze) frente a mapa derivado (F4)

El mapa materializado **es** la derivación de E.4. La tabla siguiente es la previsión del diseño. Una diferencia entre previsión y derivación se clasifica en
C-20b: es una corrección si la materialización se apartó del Freeze, y una A-n si el Freeze era incompleto.

| Archivo | MODIFIED previstas | ADDED previstas (solo I62) | ENTRY |
|---|---|---|---|
| `docs/AUTOMATION_PLAN.md` | `## 8. Implementacion, estado versionado y Pull Requests` (formato `/v2`); `## 16. …` (ancestro); 16.1, 16.3 (puntero), 16.4-16.9, 16.11 y 16.12 donde cambian | subsecciones de §16 para I62 (autoverificación, observación, adapters, independencia, custodia, adopción) | `### 16.13 …` |
| `docs/WORKFLOW.md` | `## 4. …` (referencia «al abrir»); `## 10. …` (fila) | — | — |
| `docs/INITIATIVE_LIFECYCLE.md` | §5/§9 solo con OD-6 alternativa 1 | — | — |
| `docs/initiatives/PROMPT_TEMPLATES.md` | `## G. …` si cambia; `## 2. …` (bloque factual, F1) | — | — |
| `agent-execution/README.md` | §§1, 3, 5, 6 y 11 | secciones I62 | — |
| `agent-execution/routing.md` | §§4-5 y §7 | clase nueva en sección nueva | — |
| `agent-execution/model-catalog.md` | ninguna sección existente | secciones nuevas (E.7) | — |
| `agent-execution/adapters/*`, `schemas/adapters/*`, esquemas `/v2`, `preflight`, `binding` | — | archivos ADDED | — |
| `compatibility/clause-map.schema.json` | — | — | archivo ENTRY |
| esquemas `/v1` | ninguna (C-19) | — | — |
| `docs/adr/` | índice (cierre documental) | ADR sucesor | — |
| `docs/FOUNDATIONS.md` | entrada (cierre documental; descriptiva) | — | — |
| `AGENTS.md`, `CLAUDE.md` | ninguna | — | — |

### E.6 Trazas con un contrato `/v1` real sin cambios (análisis; C-20c las ejecuta en F4)

**Contratos `/v1` reales disponibles** (MEASURED): solo los de I-61 (G2 y G3); ninguna otra unidad ha usado la ejecución delegada. Punto de partida: el
`gate-contract.json` de I-61 G3 (`docs/automation/evidence/I-61-pilot/g3-cama-d1a/R20261001T032333Z-4a2d/`). Es válido contra `gate-contract.schema.json`
`/v1`, que I-62 no cambia (C-19), tiene 17 autoridades y usa tres formas de `Section`: la línea de encabezado exacta, «preámbulo» y «documento completo».

**Repositorio del escenario:** un clon local desechable de RackCad, nunca publicado.
- `main` local = el `origin/main` real, que contiene el `MainSha` del contrato (`95690c28`).
- EFF = merge local de los textos materializados de I-62, con el trailer normativo y una tabla PRE en el cuerpo. La PRE lleva las unidades activas reales y,
  **declarada como dato de prueba**, una fila de I-61 con su `Claim-Id` real, para que el contrato de G3 pertenezca a una unidad ANTERIOR.
- M2 = EFF + X1 + X2 + X3:
  - X1 modifica `## 3. Limites de seguridad` (no modificada por I-62);
  - X2 modifica `### 16.4 …` (modificada por I-62);
  - X3 añade a WORKFLOW una sección `##` nueva (posterior a EFF, de otra iniciativa).

**C-20c-1 — el contrato real, byte a byte** (blob `9b5ef6df…`; ningún campo cambia):

| Ejecución | `MainSha_eval` | Resultado esperado |
|---|---|---|
| (a) delegación aceptada antes de EFF | `K.MainSha` = `95690c28` | PRE_ACTIVATION: lectura de I-61 idéntica a la de hoy (regresión nula) |
| (b) reverificación de 16.7 sin conflictos tras EFF | `MainSha` nuevo del `RebaseMap` = M2 | resolución de la tabla siguiente; `Authority` pass sin reemisión |

| # | Cita (Path · Section) | Clase en el contrato | Revisión en (b) | Motivo |
|---|---|---|---|---|
| 1, 2, 4 | AUTOMATION_PLAN · preámbulo, `## 2. Fuentes del plan y modos de ejecucion`, `## 16. Ejecución delegada bajo orden del Coordinator` | UNIT_CHANGE | AR | sin cambio |
| 3 | AUTOMATION_PLAN · `## 3. Limites de seguridad` | EXTERNAL | M2 (ve X1) | no modificada por I-62 |
| 5 | WORKFLOW · `## 3. Worktrees` | EXTERNAL | M2 | no modificada |
| 6 | WORKFLOW · `## 4. Ciclo de vida de una iniciativa (el proceso repetible)` | EXTERNAL | EFF^1 | MODIFIED |
| 7 | WORKFLOW · `## 10. Autoridad por dominio` | UNIT_CHANGE | AR | sin cambio |
| 8 | AGENTS.md · documento completo | EXTERNAL | M2 | archivo sin cambios |
| 9-14 | PROMPT_TEMPLATES `## G. …`, README, `routing.md` y tres esquemas `/v1` | UNIT_CHANGE | AR | sin cambio |
| 15-17 | documentos de I-61 | UNIT_DOC | AR | sin cambio |

Las comprobaciones de 16.3 sobre los archivos con secciones `UNIT_CHANGE` no cambian: `MB` y la igualdad de las secciones `EXTERNAL` entre `MB` y `AR`.

**C-20c-2 — el mismo contrato para una unidad I61 que no es I-61.** Es lo que escribiría un autor que solo conoce I-61. Solo cambian los datos de emisión:
- `Unit` = I-64 (ANTERIOR en PRE, con su `Claim-Id` real);
- `Gate`, `TaskId` y `Objective` del escenario;
- `AuthorityRevision` = un commit de la rama de I-64;
- `MainSha` = M2 (emitido tras EFF);
- la clase de las 14 citas no `UNIT_DOC`, que pasan a `EXTERNAL` (los cambios de I-61 son externos para I-64);
- las 3 `UNIT_DOC`, que pasan a ser los documentos de I-64.

**`Path` y `Section` son idénticos byte a byte, incluidas las citas de documento completo, y no se añaden citas de §16.13 ni del mapa.**

| # | Cita (Path · Section) | Revisión (I61) | Motivo |
|---|---|---|---|
| 1 | AUTOMATION_PLAN · preámbulo | M2 | no modificado |
| 2 | AUTOMATION_PLAN · `## 2. Fuentes del plan y modos de ejecucion` | M2 | no modificado |
| 3 | AUTOMATION_PLAN · `## 3. Limites de seguridad` | M2 (ve X1) | evolución normal |
| 4 | AUTOMATION_PLAN · `## 16. Ejecución delegada bajo orden del Coordinator` | EFF^1 (no ve I-62 ni X2) | MODIFIED; compromiso declarado |
| 5 | WORKFLOW · `## 3. Worktrees` | M2 | no modificado |
| 6 | WORKFLOW · `## 4. Ciclo de vida de una iniciativa (el proceso repetible)` | EFF^1 | MODIFIED |
| 7 | WORKFLOW · `## 10. Autoridad por dominio` | EFF^1 | MODIFIED |
| 8 | AGENTS.md · documento completo | M2 | archivo sin cambios |
| 9 | PROMPT_TEMPLATES · `## G. Delegación de ejecución` | EFF^1 si §G cambia; M2 si no | derivación |
| 10 | agent-execution/README.md · documento completo | **compuesta**: preámbulo y `##` no modificados en M2; §§1, 3, 5, 6 y 11 en EFF^1; secciones solo I62 omitidas | archivo modificado; cita intacta |
| 11 | agent-execution/routing.md · documento completo | **compuesta**: §§1-3 y 6 en M2; §§4-5 y 7 en EFF^1; la sección de la clase nueva, omitida | ídem |
| 12-14 | `delegation`, `worker-handoff` y `controller-verification` `.schema.json` · documento completo | M2 | sin cambios (C-19) |
| 15-17 | documentos de I-64 | AR | sin cambio |

**Resultado:** `Authority` pass con el contrato **sin cambios**, y la tabla queda en `Authority.Evidence`. `MainSha` = M2 cumple A6 y `Remote`, y `MB` se calcula
con M2. **Positivo adicional:** una variante que cita WORKFLOW «documento completo» incluye la sección de X3, leída en M2.

**Negativos (el fallo cerrado se conserva):**

| Id | Caso | Esperado |
|---|---|---|
| N-a | C-20c-2 para una unidad sin `Claim-Id` en PRE, sin `/v2` y sin decisión | UNKNOWN → STOP |
| N-b | C-20c-2 (`/v1`) para una unidad I62 | P-15 |
| N-c | mapa con una entrada duplicada | MAP_INVALID → STOP |
| N-d | mapa sin `### 16.4 …`, que sí cambió | MAP_INVALID (MV-6) |
| N-e | mapa que lista `## 3. Limites de seguridad` como MODIFIED (texto igual) | MAP_INVALID (MV-6) |
| N-f | cita individual de una subsección ADDED de §16 (solo I62) | P-15 |
| N-g | cita de un encabezado inexistente en EFF^1, EFF y M2, o repetido | S-12 |
| N-h | C-20c-2 con `MainSha` = M0 (antes de EFF) | PRE_ACTIVATION: todo `EXTERNAL` en M0, como I-61 hoy |
| N-i | cita «documento completo» de un archivo ADDED por I-62 | P-15 |
| N-j | dos commits con el trailer normativo | ACTIVATION_INVALID → STOP al Owner |
| N-k | cita `EXTERNAL` de una ruta fuera de `Surfaces` cambiada por I-62 (p. ej., `docs/ROADMAP.md`) | S-12 |
| N-l | contrato para una unidad I62 con `g0_acceptance` PENDING | PENDING_G0 → STOP (P-15) |
| N-m | §16.13 retirada en M2 con EFF presente | fallo cerrado (paso 0) |

### E.7 Obligaciones

- La integración publica §16.13, los punteros, el mapa y su esquema en **el mismo** merge efectivo; sin activación parcial (WORKFLOW §11.2).
- El mapa se genera por la derivación y se revisa contra E.5 en F4 (C-20b). Se **regenera y se revisa de nuevo sobre el merge local** antes del push de la
  integración.
- **Sin renombrar ni borrar encabezados existentes** en las superficies: una sección de I-61 modificada conserva su línea de encabezado exacta. Así los contratos
  I61, escritos con los encabezados de siempre, siguen coincidiendo, y REMOVED queda vacío por diseño (MV-6 lo comprueba igualmente).
- La materialización añade contenido nuevo en **secciones nuevas** cuando no necesita cambiar una existente, para no fijar en EFF^1 secciones que siguen
  evolucionando (catálogo en particular).
- Los encabezados son únicos por archivo en las superficies (C-20a).
- **Ningún contrato I61 necesita cambiar** por la integración de I-62 (C-20c).
- El tag repite la ruta y el blob del mapa, y POST (después del merge).

## Anexo F — Secuencias con SHAs simbólicos (análisis, no ensayo)

**Notación:**
- `M`: `origin/main`;
- `Qk`, `QH`, `QR`: commits de estado de la sesión;
- `R`, `G`: commits del Worker;
- `T`: directorio transitorio del intento;
- `r1…rn`: `relay-record/v2` encadenados por `PrevRelaySha256`;
- `rv`: `record_version`;
- `DS`: estado de delegación derivado (B.8.2).

### F.1 Cadena normal (llega a verificar G)

| Paso | Actor | Git (rama) | Transitorio (T) | `DS` | Comprobación |
|---|---|---|---|---|---|
| 0 | P | `HEAD` = `origin` = `Qp` (Q7, QU, QR o BOOTSTRAP anterior; `window` CLOSED, `seq` = k−1) | — | NONE o PLANNED | autoverificación (preflight P, CUSTODY) |
| 1 | P | commit **Q0** (`task_intent` T; `window` OPENABLE, `seq` = k; binding de C en `planned_roles`; `rv` = n+1) → push (CAS) | — | PLANNED | `BaseSha` := Q0 |
| 2 | P→C | sin escritura | r1 (Exit, cesión, Outcome, Entry; `WindowSeq` k); delegación d | PLANNED | Exit/Entry 16.4 |
| 3 | P, Coordinator | sin escritura | binding W, preflight W, aceptación A1'-A8' de d, nc4 | **ACCEPTED_OPEN** | — |
| 4 | P→W | W: R → push; G → push | r2; handoff h | ACCEPTED_OPEN | Entry: `HEAD` = `origin` = **G**; terminación de W acreditada; `Outcome` COMPLETED |
| 5 | P | sin escritura | `RemoteFacts` (CI de R y de G) | ACCEPTED_OPEN | `Ci`, `RedPart` |
| 6 | P→C | sin escritura | r3; verificación v = **VERIFIED, `VerifiedSha` = G** | CLOSED_PENDING_CUSTODY | `Identity`: `HEAD` = `origin` = `CurrentSha` = G ✓ |
| 7 | P→C | sin escritura | r4-r6 (nc1-nc3) | CLOSED_PENDING_CUSTODY | oráculo relativo |
| 8 | P | commit **Q7** (`window` CLOSED, `seq` = k; `last_window` VERIFIED con `verified_sha` = G y el diario custodiado; `chains` T con `chain_base_sha` = Q0; `rv` = n+2) → push (CAS) | — | NONE o PLANNED | SHA evaluado = Q7 = G + documentos (16.1); B.8.4 en pass |

### F.2 Caídas antes y después de cada publicación

| Momento | Estado remoto (último punto) | `DS` para un tercero | Recuperación |
|---|---|---|---|
| antes del push de Q0 (Q0 local) | `Qp` (≠ Q0) | NONE o PLANNED | mismo host: T10(b) → publicar Q0. Otro host: no hubo ventana (W-1); P sin terminación acreditada → T11 (STOP); con terminación → T12b (QR desde `Qp`), registrando el posible trabajo local inaccesible (T14) |
| después de Q0, antes de 2 | Q0 | UNKNOWN sin diario; PLANNED con diario | con diario (mismo host): T12a o T18. Sin diario: B.8.5 R-1 → QR ABANDONED; `uncertain` PLANNING = 1 |
| durante 2 (C vivo) | Q0 | ídem | confirmar la terminación de C (operación 7) antes de lanzar nada (P-02). Host inaccesible → P-13 |
| después de 2, antes de 4 | Q0 | ídem | mismo host: seguir con T (o T12a). Otro host: B.8.5 R-1; **nunca cero lanzamientos** |
| durante 4, después de R | Q0 + R | ACCEPTED_OPEN o UNKNOWN | con diario: terminación de W → handoff ausente → `Handoff` BLOCKED (T9); trabajo R preservado. Sin diario: B.8.5 R-2 → R en `unverified_commits` |
| después de G, antes del handoff | Q0 + R + G | ídem | con diario: T9; el Controller puede verificar los commits verificables; sin VERIFIED si `Handoff` falla. Sin diario: R-2 |
| después de 6, antes de Q7 | Q0 + R + G | CLOSED_PENDING_CUSTODY con diario; UNKNOWN sin él | mismo host: Q7. Otro host: v perdido → R-2 (G sin verificar, sustitución de B.8.6) |
| Q7 local sin push | Q0 + R + G | ídem | T10(b) → publicar. Rechazo: inspeccionar la causa (§8.5) |
| push de Q7 rechazado por avance ajeno | G' ≠ G | — | T10(c) → STOP P-02/S-07; Q7 local conservado |

### F.3 Dos recuperaciones sin dos escritores

| Paso | Estado |
|---|---|
| inicio | P ausente → NO_OBSERVATION → T11 STOP |
| terminación | un observador acredita la terminación de P → ORPHAN_CONFIRMED |
| designación | el Coordinator designa a **N1** (decisión `Dk`) |
| CAS de N1 | N1 publica QR (`rv` = n+1, designado N1) → push aceptado |
| intento de N2 | N2 prepara su QR desde `rv` = n → push **rechazado** (no fast-forward) → `fetch`: ve a N1 designado por `Dk` → **N2 no opera**: no hay designación vigente para N2 |
| resultado | un solo escritor (N1) |

En T12a no hay CAS dentro de la ventana: la designación es única por ventana, y un N no designado no opera ni escribe en el diario. Las comprobaciones de
procesos de 16.4 detectan un participante ajeno (P-02).

### F.4 Portabilidad A→B: FX-04a y FX-04b (fixture)

| Paso | Actor | Git (fixture) | Evidencia | Escenario |
|---|---|---|---|---|
| 1 | A | Q7 de FX-02 (ventana k cerrada, VERIFIED sobre G) | — | — |
| 2 | A | **QH** (`task_intent` T2; titular RELEASED; `window` CLOSED; `rv` = n+1) → push | — | FX-04a |
| 3 | supervisión u Owner | — | terminación de A acreditada | FX-04a |
| 4 | Coordinator | — | oráculo fuera del host; su SHA-256 en la evidencia real | FX-04a |
| 5 | B | — (clon limpio en QH) | preflight RESUME_DECISION; entradas enumeradas | FX-04a |
| 6 | B y supervisión | — | respuesta; su SHA-256 registrado **antes** del oráculo | FX-04a |
| 7 | Coordinator | — | oráculo entregado; comparación mecánica → **PASS/FAIL de FX-04a** | FX-04a |
| 8 | B | **QR** (titular B por CAS; `rv` = n+2) → push | preflight CUSTODY; designación (T16) | FX-04b |
| 9 | B | **Q0** (ventana k+1; `rv` = n+3) → push | — | FX-04b |
| 10 | B→C | sin escritura | planificación; aceptación; rebinding del Worker a `codex-cli` | FX-04b |
| 11 | B→W | W: R' → push; G' → push | Entry: `HEAD` = `origin` = **G'** | FX-04b |
| 12 | B→C | sin escritura | verificación **VERIFIED, `VerifiedSha` = G'**; `Identity` G' ✓ | FX-04b |
| 13 | B | **Q7'** (`rv` = n+4) → push | B.8.4 en pass → **resultado de FX-04b** | FX-04b |

`BaseSha` de la delegación de B = el Q0 del paso 9.

**Contadores esperados:**
- `attempts`, `correction_launches` y por clase: sin cambio respecto de FX-02;
- lanzamientos de FX-04a: 0;
- lanzamientos de FX-04b: planificación 1 + Worker 1 + verificación 1.

Si FX-04b se detiene en el paso 8 por falta de `repo-write`, o en el 10-11 por falta de escritura del Worker, su resultado es UNVERIFIED o UNSUPPORTED. El
resultado de FX-04a (paso 7) no cambia.

### F.5 Reconstrucción sin diario (B.8.5)

| Caso | Último punto y remoto | `DS` reconstruido | QR de cierre | Contadores |
|---|---|---|---|---|
| R-0 | último punto ≠ Q0 | NONE o PLANNED | no hace falta cerrar ventana; QR solo si hay transferencia | durables |
| R-1 | Q0; sin commits posteriores | UNKNOWN_ACCEPTANCE | ABANDONED, `delegation_run_id` = `"UNKNOWN"`, `journal` = `null`, `reconstruction` = registro | PLANNING: `launched` 0, `uncertain` 1; el relanzamiento suma BLOCKED/PLANNING +1 |
| R-2 | Q0 + R (+ G) del Worker, en alcance y con trailer de catálogo | ACCEPTED_OPEN (paquete perdido) | ABANDONED; `unverified_commits` = {R, G}; cadena IN_COURSE | PLANNING `launched` 1; WORK `launched` 1; VERIFICATION `uncertain` 1; el relanzamiento suma BLOCKED/WORK +1; la siguiente delegación lleva `SupersededCommits` (B.8.6) |
| R-3 | Q0 + commits fuera de alcance o no atribuibles | — | ninguno | STOP P-02/S-07 (T10 c) |

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
| `Trailer` (16.9 #11) | coherente con el proveedor y el modelo del binding; con `SupersededCommits`, presencia y celda de catálogo para los sustituidos (B.8.6) | **ampliado** |
| `Scope` (16.9 #7) | sin cambio; con `SupersededCommits`, además el alcance acumulado (B.8.6) | **ampliado** (solo con sustitución) |
| `Contract` (16.9 #4) / A3-A5 | + `RoleRequirements` (delegación ⊇ contrato por rol: `Mandatory` ⊇; cada dimensión de `Independence` ≥ la del contrato con REQUIRED > PREFERRED > NOT_REQUIRED) + `SupersededCommits` igual | **ampliado** |
| `Authority` (16.9 #3) y 16.3 | resolver de §16.13 antes de 16.3 (E.3), aplicado por quien lee el contrato; el contrato `/v1` no cambia | **ampliado** (sin efecto en el contrato) |
| 16.3: «`EXTERNAL` se lee en `MainSha`» | para unidades I61 tras EFF, las cláusulas del mapa se leen en `I62_EFFECTIVE_SHA^1`; el resto en `MainSha`; una cita de documento completo de un archivo modificado se lee compuesta (E.3); `MainSha` y `AuthorityRevision` sin cambio de significado | **sustituido solo en la revisión de lectura de las cláusulas del mapa** (compatibilidad declarada, con su compromiso) |
| contrato `/v1` existente de una unidad I61 | sin cambios: ni campos, ni citas nuevas, ni reemisión para acogerse al resolver | **conservado** |
| A1 | + conjunto I62 + B.3 | **ampliado** |
| A2 | + `BindingRef` (B.2) | **ampliado** |
| A6 | + `ProtocolSet` = protocolo de la unidad (E.2) | **ampliado** |
| A7 | elegibilidad vía binding aceptado + observación vigente | **sustituido** (misma regla de ADR-0046 #4) |
| A8 | + `CountersSnapshot` = autoridad | **ampliado** |
| `Identity`, `Remote`, `CleanTree`, `FreeText`, `Denials` | sin cambio (`Denials` incluye la pérdida de autenticación como S-06, como hoy) | **conservado** |
| 16.1 «trabajo delegado terminado» | + sin `unverified_commits` pendientes de la tarea (B.8.6) | **ampliado** |
| 16.4 paso (1): «commit de estado de la sesión, si hace falta» | Q0 **obligatorio** antes de cada ventana (W-1) | **ampliado** |
| 16.4 orden de commits (pasos 2-7) | **sin cambio**; la custodia usa Q0/Q7 y el diario | conservado |
| 16.6 «delegación abierta»: aceptada y sin verificación válida ni cierre; una por unidad | **igual**; derivada del diario (B.8.2), nunca durable; el estado registra la intención planificada (`task_intent`) y el cierre (`last_window`) | **conservado** |
| 16.6 «cadena en curso» | `chains[].state` = IN_COURSE, con `correction_authorized_pending` | conservado (registro explícito) |
| 16.8 por clase: correcciones lanzadas | igual, registradas en el Q0 de la corrección (`correction_launches`) | conservado (autoridad explícita) |
| 16.8 tope de invocaciones | + registro de lanzamientos, LAUNCH_UNCERTAIN y conteo conservador sin diario (B.8.5) | **ampliado** |
| 16.12 custodia | + manifiesto en Q7 y referencias CUSTODIED en el estado | **ampliado** |
| P-01 | conservado; P-11 lo generaliza a toda huella | conservado + ampliado |
| 16.9 precedencia y first failure | sin cambio | conservado |

### G.2 Casos de cierre (esperados exactos; análisis)

| Caso | Esperado |
|---|---|
| Misma entrega con dos verificaciones (la 1.ª BLOCKED/STOP S-04 resuelta sin cambio de trabajo; la 2.ª VERIFIED) | `attempts` sin cambio; por clase sin cambio; BLOCKED/verificación = 1; invocaciones +2 |
| Corrección lanzada sin entrega (el Worker cae) | `attempts` +1 y por clase(X) +1 en el Q0 de la corrección; después, `Handoff` BLOCKED; reejecución del trabajo = BLOCKED/WORK +1; por clase sin otro incremento |
| Pérdida de contexto del Controller | S-12 → STOP; `analysis.md`; decisión del Coordinator; si decide reejecutar sin cambio → BLOCKED/fase +1; `attempts` sin cambio |
| `Routing` con efectivo ≠ solicitado | `required` → `Routing` fail → `BLOCKED/BLOCKED`. `advisory` → pass con la discrepancia anotada; puede ser VERIFIED |
| Handoff de otra corrida, `Identity` pass | `FailureClass` = `Handoff`; `REWORK_REQUIRED/REWORK` |
| Handoff de otra corrida, `Identity` fail | `FailureClass` = `Handoff` (primera en fail); disposición STOP (STOP > REWORK) → `EXECUTION_BLOCKED/STOP` |
| Proceso terminado con turno fallido | `Outcome` FAILED_TURN → `Termination` fail → BLOCKED de transporte; sin veredicto de contenido |
| Ventana perdida sin diario y sin commits del Worker (R-1) | QR con ABANDONED; `attempts` sin cambio; PLANNING `uncertain` 1; al relanzar, BLOCKED/PLANNING +1 |
| Ventana perdida sin diario con commits del Worker (R-2) | QR con ABANDONED; `unverified_commits` = los commits; al relanzar, BLOCKED/WORK +1; la siguiente delegación exige `SupersededCommits`; ninguna verificación cuenta como trabajo terminado hasta un VERIFIED que los sustituya |
| Contrato `/v1` de una unidad I61 tras EFF que cita entero un archivo modificado | lectura compuesta (E.3); `Authority` según el resto de 16.3; **sin reemisión** |
| Unidad I62 en PENDING_G0 que intenta abrir una ventana | I-S15 → Q0 inválido; ningún contrato (P-15); STOP hasta el QU de §8.6 |
