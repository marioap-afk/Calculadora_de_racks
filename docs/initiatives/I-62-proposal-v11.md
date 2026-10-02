# I-62 — Proposal V11: portabilidad del Principal, binding de roles independiente del proveedor y orquestación autónoma de roles

```text
Frozen: NO
Version: V11 (sustituye a V10 en la revisión; V1 a V10 se conservan sin cambios como versiones históricas)
Unit: I-62   Workflow: V2 (T4)   Claim-Id: 5b661a17-8c18-4183-8554-3866059cba2b
Archetype: NEW ARCHITECTURE (Coordinator, C62-F0-09; el mandato conserva la etiqueta FOUNDATION EVOLUTION)
Base: origin/main 819955d6   Discovery: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65) y §21, base de diseño (C62-F0-08)
Reviews: Coordinator SEPARATE SESSION — V1 a V5 CHANGES REQUIRED; requisito nuevo R62-AUTO-01..20 sobre V8 (I-62-coordinator-requirement-auto.md).
         Architect SEPARATE SESSION — V6 y V7 CHANGES REQUIRED (I-62-architect-review-v6.md, -v7.md); V8 no se envió al Architect.
         Architect de V9 — BLOCKED — OWNER DECISION, no acreditada como revisión formal limpia; A62-V9-01..06 aceptados (I-62-architect-review-v9-disposition.md).
         Architect de V10 (revisión formal limpia, codex-cli) — CHANGES REQUIRED; válida con una excepción por hallazgo: A62-V10-02 NO aceptado (premisa
         inválida por degradación de la codificación); A62-V10-01, -03 y -04 aceptados; requisito nuevo R62-FIDELITY-01..06
         (I-62-architect-review-v10.md, I-62-architect-review-v10-disposition.md)
Author: sesión principal responsable de I-62 (Claude), corrección autónoma ordenada por el Owner y el Coordinator
Review: PENDIENTE — UNA revisión formal limpia del Architect, con el cierre efectivo de insumos y la fidelidad de los insumos acreditados por el invocador
        antes de lanzar (paquete: docs/initiatives/I-62-architect-package-v11.md)
Owner: OD-6 pendiente (ambas alternativas delimitadas en §11.4); las demás OD bloquean en su frontera real (§18)
IMPLEMENTATION AUTHORIZATION = NO   ·   I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED
```

**Fuentes:**
- Alcance: el mandato ([I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt)).
- Hechos: el [Discovery](I-62-discovery.md) R1 y su §21.
- Decisiones: [decisiones](../automation/decisions/I-62.md) §§7-22.
- Revisiones:
  - registros del Coordinator [V1](I-62-coordinator-review-v1.md) … [V5](I-62-coordinator-review-v5.md) y [requisito R62-AUTO](I-62-coordinator-requirement-auto.md);
  - registros del Architect de [V6](I-62-architect-review-v6.md), [V7](I-62-architect-review-v7.md), [V9](I-62-architect-review-v9.md) y
    [V10](I-62-architect-review-v10.md);
  - disposiciones del Owner y del Coordinator sobre las revisiones de [V9](I-62-architect-review-v9-disposition.md) y
    [V10](I-62-architect-review-v10-disposition.md).

**Precedencia:** las autoridades integradas mandan mientras I-62 no se integre.

**Diseño, no conducta:** no modifica el protocolo operativo, no publica esquemas ni cambia conducta. REFERENCE OVER REPETITION.

**Mapa de la corrección V10→V11.** Responde a la disposición del Owner y del Coordinator sobre la revisión formal de V10.

| Hallazgo o requisito | Corrección | Capítulos de V11 |
|---|---|---|
| A62-V10-01: sin equivalencia de evidencia de pruebas | una acción inicial incompatible con el aislamiento solo se omite por una **exención explícita y acotada** a esa invocación y esa acción; la CI exacta de publicación es una señal canónica **separada**, no equivalente a Core local, que no satisface ninguna clase de prueba local ni se propaga a gates, Candidato, cierre ni implementación; sin exención, no hay lanzamiento. Sin precedente de sustitución | §20.3.1; B.9; Anexo A; §18 (OD-1); G.1; C-41 |
| A62-V10-03: vigencia de la autorización frente a la acreditación histórica | **vigencia de acción** (materializar, reservar, lanzar) separada de la **acreditación histórica** (binding, invocación, resultado, disposición y ARCHITECT_SATISFIED ya alcanzados); `AuthorizationRef` con commit y blob exactos; regla determinista para los intentos en curso | §20.5.1; B.2; B.8.8 (I-S18, I-P13); F.8; C-40 |
| A62-V10-04: contrato de salida de PLAN del Controller | EXECUTION_CONTROLLER / PLAN → `rackcad-delegation/v2`; EXECUTION_CONTROLLER / VERIFY → `rackcad-controller-verification/v2`; no intercambiables; `role-invocation/v1` admite ambos | §20.7; B.9; C-30 |
| R62-FIDELITY-01..06: fidelidad de los insumos canónicos | contrato `CanonicalInputFidelity`; preflight de transporte en UTF-8 verificado por el mismo camino de lectura; fallo cerrado ante degradación (INPUT_FIDELITY_INVALID); invalidación acotada por hallazgo con `PremiseRefs`; evidencia observada por el invocador; casos de prueba | §20.3.3; B.1; B.8.8; B.9; B.10; §13 (P-24, P-25); C-42; D.8; G.1 |
| A62-V9-04: identidad lógica y presupuesto | **sin cambio de semántica**: `review_rounds` = versiones distintas revisadas; `logical_requests` = solicitudes lógicas; `review_rounds` ≤ `logical_requests`; pendiente de una disposición explícita sobre el texto fiel de V11 | §20.6 (ilustración añadida) |
| A62-V10-02 | **no aceptado** (premisa inválida por degradación de la codificación); no motiva ningún cambio y no es un REQUIRED abierto | — |

**Trazabilidad del requisito R62-AUTO-01..20** (incorporado en V9; capítulos de V11):

| Requisito | Capítulos |
|---|---|
| R62-AUTO-01 (el Owner no es el bus de mensajes) | §20.1; §13 (P-17..P-25) |
| R62-AUTO-02, -14 (el Principal orquesta; la autoridad no cambia) | §2; §20.2; §20.5.1; B.8.8 (I-S18); C-38, C-40 |
| R62-AUTO-03, -13 (contrato neutral de invocación; Architect limpio) | §20.3; B.1; B.9; C-30, C-41, C-42 |
| R62-AUTO-04, -16 (siguiente acción durable; estado de orquestación) | §20.4; B.8.1; B.8.8 |
| R62-AUTO-05, -06, -08 (bucle del Architect; resultado; linaje) | §20.5; B.10; B.8.8; F.8; C-31, C-35, C-38, C-42 |
| R62-AUTO-07 (presupuestos del bucle) | §9.3; §20.6; B.8.8 (I-P13); C-34, C-36 |
| R62-AUTO-09, -10 (Controller/Worker y Reviewer con el mismo orquestador) | §20.7 |
| R62-AUTO-11 (portabilidad del bucle) | §20.8; F.8; C-29 |
| R62-AUTO-12 (piloto de autonomía real) | §12; D.8 (FX-06); §18 (OV-I62-06); C-39 |
| R62-AUTO-15 (respaldo manual y AUTONOMY_GAP) | §20.9; C-37 |
| R62-AUTO-17 (Nivel A primero) | §20.10; §17 |
| R62-AUTO-18 (límite de autoalojamiento; fricción observada) | §20.11 |
| R62-AUTO-19 (coherencia de objetivos, autoridad, estado, gates, matrices, pilotos, OV, paquete) | §§1-3, 9.3, 12-14, 17-18; Anexos A, B, C, D, F, G; paquete V11 |
| R62-AUTO-20 (relación con DIRECT_ONLY) | §14.0; §20.12; C-28 |

**Cierres preservados sin cambio:** todos los de V10 y anteriores. Disposiciones formales del Architect de V10 no afectadas por la codificación, que se
conservan:
- CLOSED: A62-V9-02, A62-V9-03, A62-V9-05, A62-V9-06, A62-V7-01..03 y A62-V6-01..03;
- SUPERSEDED: A62-V9-01 → A62-V10-03.

A62-V9-04 **no** se registra como sustituido por A62-V10-02: queda pendiente de una disposición explícita. Los OPTIONAL abiertos (O-V7-01, O-V7-02, O-05,
O-07) no bloquean.

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
| Aplicabilidad | ejecución delegada opt-in («ninguna otra iniciativa lo adopta por estar escrito») | igual: DIRECT_ONLY o I62_DELEGATED registrado en G0, con adopción posterior explícita (§14.0, §8.7) |
| Orquestación entre roles | el Owner transporta prompts y resultados entre roles fuera de la unidad delegada | RELAY automático entre roles IA vinculables; el Owner solo por escalada; contrato de invocación con cierre y fidelidad de insumos, resultados de revisión por rol, materialización autorizada de bindings del Architect y estado `orchestration` durables (§20) |
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
| 14 | No bloquear producto | D-18 | todas | C-20c (las unidades I61 siguen delegando con su protocolo y con sus contratos `/v1` **sin cambios**); C-28 (una unidad nueva DIRECT_ONLY trabaja sin la maquinaria I62); DC-07 por hito |
| 15 | El Owner no es el bus de mensajes entre roles; otro Principal reanuda el bucle (requisito R62-AUTO del Coordinator) | D-19 | F4, F6 | C-29..C-42 |

## 2. Roles (D-01) y acumulación

| Rol | Semántica | Declara | Nunca declara |
|---|---|---|---|
| **PRINCIPAL_COORDINATOR** | sesión responsable de la unidad: worktree, preflight, relevos, propuesta de bindings, trabajo directo autorizado, hechos y custodia; en una unidad I62_DELEGATED, además, **orquesta** las invocaciones de rol elegibles (§20.2) sin ganar autoridad de gate | hechos del relevo, del remoto, del rebase y del preflight; `CONFIGURATION_STATUS` propio | `EXECUTION_*`; GATE PASS salvo SAME-SESSION ROLE de Coordinator declarado |
| **ARCHITECT** | revisión de diseño y conformidad (LIFECYCLE §5, §9) | veredicto de LIFECYCLE, en `architect-review-result/v1` (§20.7) | GATE PASS, `EXECUTION_*` |
| **EXECUTION_CONTROLLER** | planificación y verificación, solo lectura (§16) | `EXECUTION_VERIFIED/REWORK_REQUIRED/BLOCKED` | GATE PASS, Candidato, cierre, integración |
| **WORKER** | escritura dentro del alcance | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS |
| **REVIEWER** | revisión operativa fuera de LIFECYCLE, sin autoridad de Architect ni de Controller | hallazgos y recomendaciones, en `reviewer-result/v1` (§20.7) | `EXECUTION_*`, GATE PASS, veredictos de LIFECYCLE, ARCHITECT_SATISFIED, cierre o rebaja de hallazgos del ARCHITECT |

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
| LIFECYCLE §5/§9 | revisión del Architect y conformidad; modo declarado (SAME-SESSION ROLE, SEPARATE SESSION o EXTERNAL HUMAN) | predicado de independencia para revisiones mayores **solo si OD-6 elige la alternativa 1** (§11.4), con evidencia propia por modo y conjunto de referencia de todos los autores; **ningún modo se retira** | Owner (OD-6) + LIFECYCLE |
| AUTOMATION_PLAN §16 | ejecución delegada (ADR-0046 #1) | roles, binding y aceptación, observación de capacidad, autoverificación del Principal, contrato de adapter, custodia operativa, recuperación, conteo y adopción por unidad, en subsecciones nuevas para unidades I62 | AUTOMATION_PLAN; **ampliación de dominio** (abajo) |
| AUTOMATION_PLAN §16.3 y §16.13 (nueva) | lectura de autoridades en `MainSha` | **§16.13**: resolver de compatibilidad **independiente del protocolo**, leído en `MainSha` por toda unidad (Anexo E). §16.3 conserva **literalmente** el texto de I-61 y añade una frase puntero; otra frase puntero abre §16 | AUTOMATION_PLAN + Owner (OD-1) |
| AUTOMATION_PLAN §8 | formato del estado | `rackcad-automation-state/v2` para unidades I62 (Anexo B.8); `/v1` sigue para I61 | AUTOMATION_PLAN |
| AUTOMATION_PLAN §16, subsección I62 nueva «Orquestación de roles» | — (I-61 solo automatiza el relevo Controller/Worker de una delegación) | RELAY frente a escalada, orquestación por el Principal, `RoleInvocation`, `ReviewResult`, bucle del Architect, presupuestos y linaje, AUTONOMY_GAP (§20); solo unidades I62_DELEGATED | AUTOMATION_PLAN + Owner (OD-1) |
| WORKFLOW, sección `##` nueva «Coexistencia de protocolos de ejecución delegada» (§12 en esta Proposal) | — (WORKFLOW es dueño de «transición», §10) | **punto de entrada de compatibilidad** (Anexo E.1, E.3): obliga a todo evaluador de un contrato de ejecución delegada a aplicar §16.13 antes de 16.3 desde `I62_EFFECTIVE_SHA`. Delta normativo nuevo: `/v1` no aporta ningún punto de entrada con esa propiedad | WORKFLOW + Owner (OD-1) |
| WORKFLOW §§3-4 | reclamo, worktree, apertura, relevo | solo referencias (paso «al abrir» → autoverificación de §16 para unidades I62_DELEGATED, §14.0) | WORKFLOW |
| WORKFLOW §10 | fila «Operación del ejecutor y ejecución delegada» | texto de la fila ampliado | WORKFLOW + Owner (OD-1) |
| `routing.md` / catálogo | selección por capacidades | clase y perfil de coordinación principal; binding de todos los roles; §7 al adapter; el contenido nuevo del catálogo va en secciones nuevas (E.7) | Freeze de I-61 §15 |
| PROMPT_TEMPLATES §G / adapters | representación | renderizado en el adapter; corrección del bloque factual de §2 (F1) | PROMPT_TEMPLATES |
| AGENTS | evidencia | sin cambios | — |

**Ampliación del dominio de §16:**
- **Fuente anterior:** ADR-0046 #1 y la fila de WORKFLOW §10.
- **Delta:** obligaciones de la sesión principal **fuera** de una delegación.
- **Autoridad:** Owner (OD-1). No se ejerce antes de la vigencia (D-15) y solo aplica a unidades I62_DELEGATED (§14.0). §16.13 es la excepción declarada: gobierna la
  lectura de autoridades de **toda** unidad desde `I62_EFFECTIVE_SHA`, sin cambiar la conducta de las unidades I61 más allá de la revisión en la que leen las
  cláusulas que I-62 modificó.

## 4. Perfil del Principal y estado de configuración

### 4.1 Perfil (D-03) y capacidades por rol y acción (O62-V1-01, preservado)

Clase «Coordinación principal», perfil PRINCIPAL_COORDINATION (Long-horizon, Frontera). **Aplica a las unidades I62_DELEGATED** (§14.0) y a sus
ensayos; una unidad DIRECT_ONLY no lo necesita para su trabajo ordinario. Requisitos **obligatorios**, como capacidades **por acción**:

| Acción del Principal | Obligatorios |
|---|---|
| RESUME_DECISION: reconstruir el estado y proponer la siguiente decisión, sin tomar la custodia | nivel y effort; `remote-facts` (lectura); `introspection` ≥ RUNTIME_OBSERVED |
| CUSTODY: custodia, relevo y orquestación **del protocolo delegado** (puntos durables BOOTSTRAP, Q0, Q7, QU, QH y QR, cesiones, bindings) | lo anterior + `repo-write` |
| evidencia local | lo anterior + `build-test`, solo para la acción que deba producir evidencia local |

El perfil y sus requisitos existen **aunque el binding del Principal no pueda aceptarse**: la autoverificación produce su preflight y su disposición sin
depender del binding (Anexo B.4). P-09 y P-10 se evalúan **por acción**: un Principal en BELOW_REQUIRED para CUSTODY no toma la custodia, pero puede producir una
respuesta RESUME_DECISION si esa acción está en MATCH o ABOVE_REQUIRED.

**CUSTODY no es un permiso de trabajo directo.** El trabajo directo de una iniciativa (editar, probar, hacer commits, relevos de WORKFLOW §3) sigue las
reglas de WORKFLOW y de AGENTS, y ninguna acción de este perfil lo condiciona. Una CUSTODY en UNKNOWN o BELOW_REQUIRED bloquea solo la maquinaria
delegada (P-09/P-10 de esa acción), nunca el trabajo directo autorizado (C-28).

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
| E12 | Principal sin `repo-write` acreditado; lo demás en MATCH | CUSTODY: UNKNOWN; RESUME_DECISION: MATCH | no toma la custodia delegada (P-10); puede responder RESUME_DECISION |
| E13 | unidad DIRECT_ONLY; CUSTODY sin observar | CUSTODY: UNKNOWN | sin efecto sobre el trabajo directo; ningún contrato delegado (P-15) |

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
5. aceptación: A7' por el Coordinator, o **materialización autorizada** bajo una autorización vigente (§20.5.1 para ARCHITECT; §20.7 para REVIEWER),
   transitoria hasta la custodia;
6. cesión al rol (diario, §8).

Una celda sin medición previa no se vincula.

**Algoritmo:**
1. requisitos;
2. candidatas con descriptor;
3. filtro de los obligatorios en MATCH o ABOVE_REQUIRED y de la elegibilidad;
4. independencia sobre `ActorRef`, `SessionRef`, entradas y proveedor;
5. nivel más bajo adecuado y transporte;
6. registro `rackcad-binding/v1` (Anexo B.5);
7. aceptación: individual, o materialización autorizada con todos los criterios en SATISFIED; nunca inferida.

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
| **BOOTSTRAP** | bootstrap de la unidad I62_DELEGATED, **antes** de la revisión de G0 (§8.6), o BOOTSTRAP de adopción posterior (§8.7) | CLOSED | HELD | `protocol` con la **evidencia** de clasificación y `g0_acceptance` PENDING; titular con su preflight y su binding **propuesto** (`principal.acceptance` PENDING), custodiados en el mismo commit; contadores vacíos |
| **Q0** | paso (1) de 16.4, **obligatorio** antes de cada ventana de cesión | OPENABLE | HELD | `task_intent` con el contrato custodiado; si la intención es una corrección, `attempts` +1 y su `CorrectionLaunch` (16.8) en el mismo commit |
| **Q7** | paso (7) de 16.4: tras la verificación y los controles, o tras el cierre declarado de la ventana | CLOSED | HELD | `last_window` (cierre y referencias custodiadas), cadenas, contadores desde el diario, manifiesto de custodia |
| **QU** | actualización del titular **sin ventana** (lo que en `/v1` es «publicar al terminar cada ejecución») | CLOSED | HELD | fase, `state`, `gate`, `next_action`, `last_evidence_commit`, `attempts` fuera de la ejecución delegada (§8/§9) o la intención siguiente; no toca `window`, `last_window` ni los contadores de ventana. `custody.point_kind` = ORDINARY, o REBASE_RECONCILIATION para el QU obligatorio tras un rebase fuera de ventana (§8.8) |
| **QH** | punto quiescente en que el titular **libera** la unidad | CLOSED | RELEASED | la intención siguiente, si existe, y la liberación; el titular liberado no opera después |
| **QR** | recuperación o transferencia del titular, por CAS | CLOSED | HELD (nuevo titular) | designación del Coordinator; si había una ventana posiblemente abierta, su cierre ABANDONED con la reconstrucción de B.8.5. Con avance de `main`, es el **QR REBASE_RECONCILIATION** de una toma con rebase (T22, §8.9): registra la designación y reconcilia los SHA reescritos en el mismo punto |

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
| 4. Revisión de G0 | Coordinator | decisión: G0, más tres marcadores explícitos: `I62-DELEGATED-EXECUTION: I62_DELEGATED \| DIRECT_ONLY` (§8.7, §14.0), `I62-CLASSIFICATION: I62 \| REJECTED` y `I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED \| REJECTED` | fuera del repositorio (orden del Coordinator; su hash va a la evidencia) | decidida |
| 5. Registro + **QU** + push | la sesión | **en un solo commit**: la entrada en `docs/automation/decisions/<unit>.md` con los marcadores, el `Claim-Id` y la `record_version` del BOOTSTRAP; la versión del binding con `Acceptance` ACCEPTED o REJECTED, `DecisionRef` = {ruta de decisiones, marcador} y `Utc`; y un QU con `g0_acceptance` y `principal.acceptance` transicionados y sus `StateRef` a esos blobs | el mismo commit | ACCEPTED o REJECTED |

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
- **Plantilla de los marcadores.** Su texto literal (`I62-DELEGATED-EXECUTION`, `I62-CLASSIFICATION`, `I62-PRINCIPAL-BINDING` y, en §8.9,
  `I62-REBASE-TAKEOVER`) se materializa en F4 en la subsección I62 de AUTOMATION_PLAN §16 sobre arranque y adopción (sección ADDED de E.5). La decisión del
  Coordinator lo copia literalmente, y la entrada de `docs/automation/decisions/<unit>.md` lo conserva.
- **Clasificación REJECTED** → UNKNOWN → STOP. Solo la remedia una decisión posterior del Coordinator (E.2, paso 6).
- **Binding del Principal REJECTED** → la unidad no tiene Principal aceptado y Q0 sigue prohibido. Se remedia con:
  - observación y propuesta nuevas, aceptadas por una decisión posterior y registradas en un QU; o
  - una transferencia (QR).
- **Titular nuevo:** la designación del Coordinator (QR o T12a) incluye el marcador de aceptación de su binding. En el QR, el binding aceptado se custodia en el
  mismo commit. En T12a va en el registro TRANSFER del diario y se custodia en el Q7.
- **Transiciones:**
  - `protocol.g0_acceptance` cambia **una sola vez** (PENDING → ACCEPTED o REJECTED), y `principal.acceptance` una vez por titular;
  - `protocol.set`, `protocol.effective_sha` y `protocol.basis` son **inmutables** desde el BOOTSTRAP (B.8.4: I-P06, I-P09).

### 8.7 Aplicabilidad y adopción de I62_DELEGATED

**El esquema del estado sigue a la aplicabilidad** (§14.0):
- **DIRECT_ONLY** → `rackcad-automation-state/v1` ordinario (AUTOMATION_PLAN §8), sin `protocol`, `custody` ni `counters`. La aplicabilidad se registra en la
  decisión de G0, con el marcador `I62-DELEGATED-EXECUTION: DIRECT_ONLY` en `docs/automation/decisions/<unit>.md`.
- **I62_DELEGATED** → `rackcad-automation-state/v2` (B.8), con el arranque de §8.6.

**Adopción en G0.** La sesión que reclama y quiere delegar publica el BOOTSTRAP `/v2` de §8.6, con `protocol.basis.adoption_at` = G0. La decisión de G0
lleva el marcador `I62-DELEGATED-EXECUTION: I62_DELEGATED` además de los dos de §8.6. Si la decisión de G0 es DIRECT_ONLY, el commit que la registra
sustituye el estado por un `/v1` ordinario con los mismos nueve campos de `automation_state` (T21). No se pierde custodia: no hubo ninguna ventana, porque
I-S15 la impedía. El `/v2` anterior queda en la historia como evidencia.

**Adopción posterior, de DIRECT_ONLY a I62_DELEGATED a mitad de iniciativa.** Se admite con autoridad explícita y fallo cerrado, y sigue el mismo orden que el
Modelo A (T20):
1. **Precondiciones:**
   - unidad posterior a EFF: una unidad ANTERIOR sigue en I61 toda su vida, sin migración;
   - estado `/v1` y ninguna delegación abierta (una unidad DIRECT_ONLY no puede tenerla).
2. **Observación y propuesta:** preflight CUSTODY y binding del Principal propuesto (PENDING), como en §8.6, pasos 1-2.
3. **BOOTSTRAP de adopción:**
   - el commit sustituye el estado `/v1` por un `/v2` con `record_version` 1, `protocol.basis.adoption_at` = MID_INITIATIVE y las dos aceptaciones en
     PENDING;
   - copia los nueve campos de `automation_state`, incluidos `attempts` y `claim_id`;
   - los contadores de ventana empiezan vacíos: los commits directos anteriores **no** son trabajo delegado.
4. **Decisión de adopción del Coordinator**, con los tres marcadores: `I62-DELEGATED-EXECUTION: I62_DELEGATED`, `I62-CLASSIFICATION: I62` e
   `I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED`. En esta adopción, `g0_acceptance` registra la decisión que acepta la adopción, aunque no sea la de G0.
5. **QU** con las transiciones, como en §8.6, paso 5. Solo entonces se admite Q0.
6. **Rechazo:** con DIRECT_ONLY, el estado vuelve a `/v1` (T21).

**No hay transición inversa.** Una unidad I62_DELEGATED que deja de delegar simplemente no abre ventanas: su estado sigue en `/v2` y se actualiza con QU.
Ninguna adopción cambia el protocolo de una delegación abierta.

### 8.8 Rebase de la rama (16.7), dentro y fuera de una ventana, y QU de reconciliación

Se aplica a todo rebase de 16.7 hecho **sin ventana de delegación abierta**, es decir, con el último punto durable distinto de Q0: el rebase de apertura de
WORKFLOW §4 o cualquier otro.

**Rebase dentro de una ventana activa** (último punto Q0). Se conserva la vía de reverificación de 16.7 y **no** se inserta ningún QU dentro de la
ventana, con un añadido:
- el `RebaseMap` del diario incluye `StateFields[]` con **todo** SHA persistido del estado que la reescritura afecta. No solo `BaseSha`, `ChainBaseSha` y
  los commits del Worker: también las entradas de `chains` de otras tareas o anteriores, `chain_red_sha`, `last_window.verified_sha`,
  `unverified_commits[].sha` y `last_evidence_commit` (B.8.7);
- el **Q7 o QR que cierra la ventana** hace la reconciliación durable: fija `custody.last_rebase` con el mapa custodiado y escribe cada campo afectado con su
  imagen;
- los resultados propios de la ventana (`verified_sha`, el RED nuevo de la cadena) se registran ya sobre la rama rebasada, y la corrida de CI del RED se
  sigue citando por el SHA original (`CiRuns`). I-P12 lo comprueba.

**Rebase fuera de una ventana** (lo que sigue, por el titular vigente). La toma por un Principal entrante con avance de `main` usa §8.9.

**Secuencia:**
1. `git fetch`; se registran `branch_before` (= `origin/<rama>`), `main_before` y `main_after`.
2. Rebase. Con conflictos: `git rebase --abort` y STOP (16.7).
3. **Construcción del `RebaseMap`** (B.8.7):
   - cada commit reescrito, del original a su imagen, con `git patch-id` igual;
   - cada campo del estado con valor SHA de la rama, del original a su imagen.
4. Si alguna imagen o identidad de parche no se acredita: la rama local vuelve a `branch_before`, **no se publica nada** y STOP (16.7). No se continúa a Q0.
5. **Publicación** con `git push --force-with-lease=<rama>:<branch_before>`, es decir, con el SHA remoto esperado explícito. Si se rechaza, la rama remota
   cambió: STOP (T10), sin reintentar a ciegas. Este force-push **no** es un punto durable.
6. **QU de reconciliación** (`custody.point_kind` = REBASE_RECONCILIATION), obligatorio antes de cualquier Q0 o acción delegada (en una toma, §8.9, el
   mismo papel lo cumple el QR combinado):
   - custodia el `RebaseMap` en `docs/automation/evidence/<unit>-agent/rebase/<RunId>/rebase-map.json`;
   - fija `custody.last_rebase`;
   - reescribe cada campo SHA según la tabla siguiente;
   - deja intactos los contadores y las `TaskId`. Por 16.7, el rebase de apertura con cadena en curso pertenece a esa tarea y no consume el máximo de
     recuperaciones; sin cadena, `TaskId` = SESSION;
   - se publica por CAS normal, en fast-forward sobre `branch_after`.

Entre los pasos 5 y 6, el estado de `HEAD` es la imagen del último punto durable, con SHAs sin reconciliar. Un evaluador lo detecta (I-H02: un SHA de rama
del estado que no es ancestro de `HEAD`) y no admite Q0 ni acción delegada hasta el QU.

| Campo con valor SHA | Tratamiento en el QU de reconciliación |
|---|---|
| `chains[].chain_base_sha`, `chains[].chain_red_sha` | imagen |
| `last_window.verified_sha` (si no es `null`) | imagen |
| `unverified_commits[].sha` | imagen; `window_seq` y `superseded_by` iguales |
| `automation_state.last_evidence_commit` | imagen si es un commit reescrito de la rama; sin cambio si es ancestro de `main_before` |
| `task_intent.contract` y los SHAs del contrato (`AuthorityRevision`, `MainSha`, `SupersededBaseSha`, `SupersededCommits`) | el contrato custodiado es inmutable. Si contiene SHAs reescritos o un `MainSha` obsoleto se reemite (16.7, «antes de escribir»), y el QU lleva la intención con `kind` = REISSUE y el contrato nuevo, cuyos `SupersededBaseSha` y `SupersededCommits` son imágenes. Si no, `task_intent` = `null` hasta la reemisión |
| `counters.rebase_recoveries[].last_rebase_map` | la `StateRef` nueva, si el rebase pertenece a esa tarea; `count` según 16.7 |
| `protocol.effective_sha` | sin cambio: es un commit de `main`, no reescrito |
| `protocol.basis.claim_commit`, `protocol.basis.claim_parent_sha` | sin cambio: son hechos históricos del reclamo original (I-P06), no identidades vivas de la rama |
| `StateRef` (`{path, blob}`) | sin cambio: los blobs no cambian con el rebase; el QU comprueba que cada ruta existe con su blob en su propio árbol |

**Reglas:**
- **Ningún SHA se descarta en silencio.** Un campo con un SHA de la rama reescrita sin imagen acreditada es STOP, nunca `null` ni borrado.
- **RED y commits sin verificar:**
  - `chain_red_files` es durable (rutas) y no se recalcula;
  - la entrega siguiente usa las imágenes de `chain_base_sha` y `chain_red_sha`;
  - la corrida de CI del RED se cita por el SHA original que guarda el `RebaseMap`: es un registro de CI, no un objeto Git;
  - la regla de 16.7 de usar «siempre los SHA originales» se refiere a la reverificación de la misma entrega dentro de una ventana, y no cambia.
- **Otra máquina.** La reconciliación y la Q0 siguiente solo usan lo que hay en el remoto: las imágenes (en la rama), el `RebaseMap` custodiado y los
  `patch-id` registrados en él. Los commits originales no hacen falta (C-15).

### 8.9 Toma de custodia con avance de `main` (REBASE_TAKEOVER)

**Problema que resuelve.** Un Principal entrante tras un QH (titular liberado) o tras T12b (titular ausente) necesita un QR para operar. WORKFLOW §4 le
exige rebasar antes de escribir si `main` avanzó, y el QR es una escritura. Exigir a la vez QR antes del rebase y rebase antes del QR sería imposible. La toma
con rebase fija **un solo orden ejecutable**.

**Orden:**
1. **Designación acotada.** El Coordinator designa al Principal entrante para una operación **REBASE_TAKEOVER**. La decisión lleva:
   - el marcador `I62-REBASE-TAKEOVER: <BindingId>`;
   - la aceptación de su binding (§8.6, «Titular nuevo»);
   - para T12b, la constancia de la terminación acreditada del titular anterior.
2. **Autoridad de la operación,** limitada a cinco acciones:
   - `git fetch`;
   - el rebase que exige WORKFLOW §4;
   - la publicación con `git push --force-with-lease=<rama>:<branch_before>`, donde `branch_before` es la punta remota observada;
   - la construcción y custodia del `RebaseMap` (B.8.7);
   - la publicación de **un** QR combinado.

   Nada más: ni trabajo ordinario, ni Q0, ni relevos, ni delegaciones.
3. **QR combinado** (`point_kind` = REBASE_RECONCILIATION), que en el mismo punto:
   - registra al nuevo titular y su designación (`principal.*`, I-P07);
   - reconcilia todos los SHA reescritos con la tabla de §8.8;
   - en T12b con último punto Q0, cierra la ventana como ABANDONED con la reconstrucción de B.8.5, y los `unverified_commits` quedan ya como imágenes.
4. **Solo después del QR** el nuevo titular puede hacer trabajo ordinario o publicar Q0.

**Fallos:**
- una imagen o un `patch-id` no acreditables → la rama local vuelve a `branch_before`, no se publica nada y STOP (16.7);
- un `--force-with-lease` rechazado → otro escritor movió la rama: STOP (T10), sin reintento;
- una caída entre el force-push y el QR → la rama queda rebasada con el estado sin reconciliar (I-H02). Solo el mismo designado (designación vigente) o un
  designado nuevo por decisión del Coordinator puede completar el QR.

Una segunda toma concurrente fracasa en su `--force-with-lease` o en su CAS. Sin designación vigente, nadie opera (T13).

**Sin avance de `main`,** T16 y T12b siguen como hasta ahora: un QR ORDINARY sin rebase. Esto es una especialización declarada del orden de toma bajo OD-1, no
un permiso para trabajar antes de rebasar.

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
| T12b | ORPHAN_CONFIRMED(P) con último punto ≠ Q0, o con último punto Q0 y diario no disponible o roto | recuperación | designación + inspección que preserva el trabajo + reconstrucción B.8.5 si el último punto era Q0 | Coordinator / N | **QR** (CAS); con Q0 previo, el QR cierra la ventana ABANDONED | solo N opera, tras ganar el CAS; con avance de `main`: T22 |
| T13 | cualquiera | dos recuperaciones concurrentes | — | — | gana el primer CAS (T12b) o la única designación (T12a) | el perdedor **no opera**, aunque relea: sin designación vigente y transición nueva no hay operación; no hay dos sesiones en el mismo worktree ni en la misma rama |
| T14 | cualquiera | cambio de máquina sin acreditar exclusividad ni recuperación | — | Coordinator | ninguno; STOP P-13 | no se reconstruye trabajo inaccesible |
| T15 | cualquiera | registro que contradice el hecho físico | contradicción | Coordinator | ninguno; S-04/P-02; corrección por una transición nueva | prevalecen los hechos (WORKFLOW §10) |
| T16 | QH (P liberó), o Q7, QU o QR con titular P, **sin avance de `main` que exija rebase** | transferencia del titular (A→B) | terminación de P acreditada (observador externo u Owner) + designación del Coordinator con aceptación del binding de B (§8.6) | Coordinator / B | **QR** ORDINARY (CAS), con el binding aceptado custodiado | B opera solo tras el CAS; con avance de `main`: T22 |
| T17 | BOOTSTRAP, Q7, QU o QR con titular P | liberación | último punto ≠ Q0 | P | **QH** (CAS) | P no opera después |
| T18 | Q0 sin ninguna cesión registrada | retirada de la intención | decisión del Coordinator (p. ej., reemisión del contrato antes de planificar) | P | **Q7** con cierre WITHDRAWN | — |
| T19 | BOOTSTRAP, Q7, QU o QR (sin ventana) con titular P | rebase de 16.7 (apertura u otro) | `RebaseMap` con imágenes y `patch-id` acreditados; push con `--force-with-lease=<rama>:<branch_before>` | P | force-push (**no** es punto durable) y después **QU** REBASE_RECONCILIATION (CAS) (§8.8) | ningún Q0 ni acción delegada entre el force-push y el QU; imagen no acreditada → STOP sin publicar |
| T20 | `/v1` DIRECT_ONLY, unidad posterior, sin delegación | adopción de I62_DELEGATED | observación y binding propuesto; decisión de adopción con los tres marcadores | P / Coordinator | BOOTSTRAP de adopción (`/v2`, `adoption_at` = MID_INITIATIVE, PENDING) → decisión → **QU** (§8.7) | Q0 solo tras ACCEPTED en ambas aceptaciones |
| T21 | BOOTSTRAP `/v2` con aceptaciones PENDING y `window.seq` = 0 | decisión DIRECT_ONLY (en G0 o en una adopción) | marcador `I62-DELEGATED-EXECUTION: DIRECT_ONLY` | Coordinator / P | el commit que registra la decisión sustituye el estado por un `/v1` ordinario (§8.7) | sin pérdida de custodia: no hubo ventana (I-S15) |
| T22 | QH, o T12b (titular ausente; último punto ≠ Q0 o Q0 sin diario), o transferencia de T16, **con avance de `main`** | toma con rebase (REBASE_TAKEOVER, §8.9) | designación acotada con `I62-REBASE-TAKEOVER: <BindingId>` y aceptación del binding; `RebaseMap` acreditado; `--force-with-lease=<rama>:<branch_before>` | Coordinator / designado | force-push (no es punto durable) y **un** QR REBASE_RECONCILIATION (CAS) que registra al titular y reconcilia los SHA (y, en T12b con Q0, cierra la ventana ABANDONED) | antes del QR, solo las cinco acciones de §8.9; ni trabajo ordinario ni Q0 |

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

Los contadores del bucle de revisión del Architect (`review_rounds`, `logical_requests`, `architect_launches`, `transport_reruns`, `correction_rounds`,
`corrections_by_lineage`) están en §20.6 y B.8.8.

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
| **Contexto** | las entradas son solo el cierre efectivo de insumos (§20.3.1): artefactos canónicos y transitivos permitidos enumerados (prompt con SHA-256 + rutas en un SHA), sin transcripción, memoria ni razonamiento de la referencia, y con las entradas automáticas del runtime enumeradas (§12) | `prompt.md` custodiado + `EffectiveInputClosure` + entradas automáticas + auditoría de lecturas cuando exista |
| **Proveedor** | proveedor distinto según los descriptores | descriptores |

Valores: **REQUIRED**, **PREFERRED**, **NOT_REQUIRED**. UNKNOWN en una dimensión REQUIRED no satisface. Un proveedor distinto con el contexto de la referencia
no satisface Contexto. Una marca no se prohíbe si la independencia se acredita.

### 11.2 Combinación y satisfacción

Por (referencia, dimensión) se toma el **máximo** (REQUIRED > PREFERRED > NOT_REQUIRED). Las referencias distintas se evalúan por separado. Si falta un
REQUIRED, el binding del revisor o verificador no se acepta; la revisión sigue pendiente y la operación dependiente se bloquea. Un PREFERRED no satisfecho se
registra.

### 11.3 Disparadores del dominio de §16

«Actor» se evalúa sobre `ActorRef`; «Contexto», con las entradas enumeradas de §12 y del Anexo D.6. Los casos de control están en el Anexo C (C-13). La **referencia** es el conjunto de actores que
escribieron el rango evaluado (`BaseSha..CurrentSha` y, con `SupersededCommits`, también los de esos commits); la independencia se comprueba contra **cada**
uno.

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

Ambas alternativas conservan los tres modos de LIFECYCLE §5: SAME-SESSION ROLE, SEPARATE SESSION y EXTERNAL HUMAN. Retirar un modo sería un delta explícito
del Owner o de LIFECYCLE, y ninguna alternativa lo propone.

| | Alternativa 1 (recomendación del Coordinator, no decisión) | Alternativa 2 |
|---|---|---|
| Revisiones afectadas | revisión de diseño de NEW ARCHITECTURE y de FOUNDATION EVOLUTION antes del Freeze; conformidad de READY-06 | ídem |
| Objeto y conjunto de referencia | `ReviewSubject` (B.2): la versión **exacta** revisada y las identidades de autor de su **conjunto acotado a la unidad y al objeto**. En una revisión de diseño, los autores de las versiones de la Proposal de la unidad y de sus deltas aceptados. En una de implementación o conformidad, los de los commits de la unidad desde `merge-base(base, commit revisado)`, restringidos a sus rutas de cambio. Se excluye la historia anterior a la unidad. Incluye cada titular Principal, en cada sucesión o rebinding, cada Worker y cada autor humano de ese conjunto, y el **operador humano** de cada sesión IA autora. Un autor que no se puede establecer dentro del conjunto cuenta como UNKNOWN y no satisface | se registra igual, sin predicado |
| Predicado con revisor IA o runtime (SEPARATE SESSION) | **REQUIRED** las tres: **Actor**, el `ActorRef` del revisor difiere del de **cada** autor IA del conjunto; **Sesión**, su `SessionRef` difiere del de cada sesión autora; **Contexto**, solo el cierre efectivo de insumos (§20.3.1), con el contexto inyectado automáticamente declarado antes de revisar y la auditoría de lecturas (C-41). Proveedor **PREFERRED** | modo declarado; Contexto y Proveedor **PREFERRED** |
| Predicado con EXTERNAL HUMAN | **REQUIRED** las tres: **Actor**, el `HumanReviewerRef` difiere de toda identidad humana de autor del conjunto, **incluido el operador humano que dirigió materialmente una sesión IA autora** (y, trivialmente, de los autores IA); **Sesión**, una `ReviewInstanceRef` propia de esa revisión; **Contexto**, los insumos canónicos revisados, declarados. **Sin** `ActorRef` ni `SessionRef` ficticios. Proveedor: no aplica | modo declarado; Contexto **PREFERRED** |
| SAME-SESSION ROLE | sigue siendo un modo válido de LIFECYCLE para otras revisiones, pero no satisface el predicado de estas revisiones mayores | válido, como hoy |
| Evidencia | `ReviewSubject`, referencias del revisor e insumos, en la cabecera del registro recuperable de la revisión | **dónde se registran las PREFERRED:** en la cabecera del registro recuperable de la revisión, con el modo declarado, si revisor y autor son la misma persona (LIFECYCLE §5) y el estado SATISFIED, NOT_SATISFIED o UNKNOWN de cada dimensión PREFERRED |
| Alcance | revisiones de unidades que adoptan I62 (I62_DELEGATED, §14.0). No es retroactiva a I-62, I-63, I-64 ni a ninguna unidad I61. Las unidades DIRECT_ONLY siguen con la regla vigente de LIFECYCLE | ídem |
| Efecto sobre LIFECYCLE | **no retira ningún modo.** SEPARATE SESSION y EXTERNAL HUMAN satisfacen el predicado con su propia evidencia. SAME-SESSION ROLE deja de bastar solo para estas revisiones mayores (delta de LIFECYCLE bajo OD-6). Una revisión del Coordinator no se convierte en dictamen del Architect | sin cambio de LIFECYCLE |

**Persona y autor (alternativa 1).** LIFECYCLE §5 exige declarar si revisor y autor son la misma persona. En la alternativa 1 son identidades humanas de
autor quien escribió contenido del objeto (commits sin trailer de IA, o contribuciones humanas registradas) **y el humano (Owner u operador) que dirigió
materialmente una sesión IA autora**. Ese humano no satisface el requisito de Actor como revisor EXTERNAL HUMAN independiente del objeto que dirigió.
Esto forma parte de la definición de la alternativa 1; no decide OD-6.

Una vez decidida, la alternativa queda identificada por versión y cláusula en el Freeze. Sin decisión, V11 conserva ambas y no hay AGREED ni Frozen: YES. No se
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
- **FX-06, autonomía real** (D.8): el Principal publica un objeto, invoca a un Architect independiente materializado bajo la autorización del fixture, ingiere
  CHANGES REQUIRED, corrige, publica e invoca a otro Architect, materializado sin decisión intermedia, que da AGREED. Todo sin que el Owner transporte nada
  (`OWNER_AS_MESSAGE_BUS` = false); si no, UNVERIFIED o UNSUPPORTED con causa;
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
| P-15 | `ProtocolSet` o esquema del contrato ≠ protocolo de la unidad; clasificación UNKNOWN, PENDING_G0 o DIRECT_ONLY (sin protocolo delegado adoptado) en un paso que depende de ella; cita de una unidad I61 a una superficie solo I62; mapa de cláusulas inválido (Anexo E.4) | STOP |
| P-16 | un resultado del plano (c) intenta actuar sobre el plano (a) | rechazo, sin efecto real |
| P-17 | `NextAction` no derivable de forma única desde el estado canónico (§20.4) | STOP por ambigüedad material |
| P-18 | presupuesto del bucle de revisión agotado, o un intento nuevo lo superaría (§20.6) | STOP y escalada, antes de reservar o lanzar |
| P-19 | salida de rol inválida o incompleta (B.10, B.9 `OutputContract`) | no avanza; reejecución de transporte dentro del tope; agotado → STOP |
| P-20 | el Principal intenta un acto de autoridad que no tiene (declarar AGREED, cerrar o rebajar un REQUIRED, elegir una decisión del Owner, declarar Freeze o PASS, materializar un binding fuera de los criterios de la autorización, usar un resultado de REVIEWER para satisfacer al ARCHITECT) | rechazo y STOP |
| P-21 | relevo manual sin registro AUTONOMY_GAP | la evidencia de orquestación no vale para el criterio 15 |
| P-22 | lectura registrada fuera del `EffectiveInputClosure` (§20.3.1) | INVALID_REVIEW_CONTEXT: el resultado no se ingiere como dictamen; el intento cuenta; reejecución de transporte dentro del tope; agotado → STOP |
| P-23 | contradicción entre `ReviewerDeclaredIdentity` e `InvokerObservedRuntimeIdentity` (§20.3.2) | S-04: el resultado no se ingiere y el bucle se detiene hasta la decisión |
| P-24 | preflight de fidelidad no acreditado: transporte UTF-8 no establecido o camino de lectura que degrada algún carácter del corpus (§20.3.3) | no se lanza; no consume presupuesto si no se llegó a reservar |
| P-25 | degradación semántica observada en la representación que vio el revisor (§20.3.3) | INPUT_FIDELITY_INVALID: los hallazgos y disposiciones afectados son INVALID_PREMISE y no cambian linajes; si no se puede acotar, no se ingiere nada; el intento cuenta |

Matriz de cada cambio frente a `/v1` (conservado, ampliado o sustituido) y casos de cierre: **Anexo G**.

## 14. Adopción por unidad y lectura legacy (D-18)

### 14.0 Aplicabilidad: la ejecución delegada sigue siendo opt-in

I-62 **no** convierte la ejecución delegada en obligatoria para las iniciativas reclamadas después de su integración. Separa dos cosas:
- **A. Operación de la iniciativa:** reclamo, contrato, estado ordinario, relevos de WORKFLOW §3, gates de LIFECYCLE y evidencia de AGENTS. No cambia.
- **B. Adopción de la ejecución delegada I62:** Controller y Worker delegados con el protocolo de este diseño. Es explícita y durable.

**Registro en G0.** La decisión de G0 de toda unidad posterior a EFF lleva el marcador `I62-DELEGATED-EXECUTION: DIRECT_ONLY | I62_DELEGATED`.

| Aplicabilidad | Qué exige | Qué permite | Qué impide |
|---|---|---|---|
| **DIRECT_ONLY** | nada de la maquinaria I62: ni contrato de gate, ni delegación, ni bindings, ni preflight del Principal, ni custodia; estado `/v1` ordinario (§8.7) | todo el trabajo directo autorizado por WORKFLOW, LIFECYCLE y AGENTS. La capacidad CUSTODY del Principal no lo condiciona (§4.1, E13) | emitir cualquier contrato Controller/Worker (P-15) mientras no haya adopción (T20) |
| **I62_DELEGATED** | `state/v2` con el arranque de §8.6, bindings, preflight, custodia, autoverificación del Principal al abrir y el resto de este diseño | además, la ejecución delegada de §16 con el protocolo I62 | — |

**Transiciones:**
- DIRECT_ONLY → I62_DELEGATED: se admite a mitad de iniciativa por la adopción explícita de §8.7 (T20), con fallo cerrado. Una decisión DIRECT_ONLY sobre un
  BOOTSTRAP `/v2` devuelve el estado a `/v1` (T21).
- No hay transición inversa ni cambio del protocolo de una delegación abierta.

**Unidades ANTERIORES** (I61): siguen en I61 toda su vida. Su ejecución delegada es la opt-in de I-61, sin cambios, y no adoptan I62.

**Conciliación con AUTOMATION_PLAN §16.** «Ninguna otra iniciativa lo adopta por estar escrito» se conserva. Estar reclamada después de EFF no implica adoptar
nada: la clase de §14 solo fija qué protocolo usaría la unidad **si** adopta la ejecución delegada, y la adopción es una decisión registrada.

Las obligaciones de I-62 sobre la sesión principal (perfil, autoverificación al abrir, `CONFIGURATION_STATUS`) y la **orquestación autónoma de roles** (§20)
aplican **solo** a las unidades I62_DELEGATED y a los ensayos del fixture.

### 14.1 Protocolos, punto efectivo y clasificación

**Protocolos:**
- `I61` = los cinco `/v1` + las cláusulas de I-61;
- `I62` = el conjunto del Anexo B.1 + las cláusulas de I-62.

`worker-handoff/v1` pertenece a ambos, con compatibilidad demostrada (B.6).

**Punto efectivo** (anclado como WORKFLOW §11.2): `I62_EFFECTIVE_SHA` es el primer merge en first-parent de `origin/main` de RackCad cuyo segundo padre alcanza
el **único** commit con el trailer `Agent-Protocol-Normative: I-62` y cuyo primer padre no lo alcanza. Vigencia: al aceptarse el push de `main`. **Ausencia,
duplicidad o derivación contradictoria = activación no válida**: ninguna unidad es I62, y STOP al Owner. La identidad se registra en el tag
`integration/I-62`. No hay activación parcial: el punto de entrada de WORKFLOW, el resolver, sus punteros, el mapa y su esquema entran en ese único merge.

**Clasificación** (algoritmo en el Anexo E.2): durable y **nunca rederivada por ascendencia actual** (WORKFLOW §11.1).

| Clase | Fuente durable | Protocolo | Paso dependiente |
|---|---|---|---|
| **ANTERIOR_DEMOSTRADA** | el `Claim-Id` de la unidad aparece una sola vez en la tabla PRE del cuerpo del merge efectivo (snapshot `git ls-remote --heads origin` con `ref / tip / commit de reclamo / Claim-Id`, como WORKFLOW §11.3) | I61, todo su recorrido | — |
| **POSTERIOR_DEMOSTRADA** (I62_DELEGATED) | su `Claim-Id` no está en PRE; el BOOTSTRAP `state/v2` registra la **evidencia** (`protocol.basis`: el padre del commit de reclamo contiene `I62_EFFECTIVE_SHA`); el Coordinator la acepta en G0 y un QU posterior registra esa aceptación (`g0_acceptance` ACCEPTED, §8.6) | I62 | — |
| pendiente de G0 | BOOTSTRAP `/v2` con `g0_acceptance` PENDING | **ninguno todavía** (PENDING_G0) | **STOP** (P-15) de contratos y delegaciones hasta la aceptación |
| posterior DIRECT_ONLY | `Claim-Id` fuera de PRE; estado `/v1`; marcador `I62-DELEGATED-EXECUTION: DIRECT_ONLY` en la decisión de G0 (§14.0) | **ninguno** (sin protocolo delegado) | contratos y delegaciones: STOP (P-15); el trabajo directo no se ve afectado (C-28) |
| posterior con base obsoleta | `Claim-Id` fuera de PRE; el padre del reclamo no contiene el efectivo; prueba de orden del primer push (WORKFLOW §11.3) aceptada en la decisión de G0 | I62 | **STOP** de toda delegación hasta que la rama contenga el efectivo |
| **DESCONOCIDA** | ninguna de las anteriores (solo en POST sin prueba de orden, Claim-Id ausente o duplicado, identidad contradictoria) | **ninguno asignado** | **STOP** de los contratos y delegaciones; se pide la evidencia concreta. Sin default I61 ni I62 |

**Quién clasifica:** el **Coordinator**, en el G0 de cada unidad nueva (aceptación de la evidencia del BOOTSTRAP, §8.6) y antes de emitir cualquier contrato
posterior al efectivo. El Coordinator, al aceptar (A6'), y el Controller, en `Authority`, vuelven a aplicar la función sobre las mismas fuentes durables; una
discrepancia es S-04. Las unidades sin campo `protocol` (todas las anteriores, con estado `/v1`) se clasifican por su `claim_id` en PRE, **sin editar su rama
ni su bootstrap**.

**Resolución de autoridades, transparente para los contratos I61** (algoritmo en el Anexo E.3):
- el algoritmo vive en una subsección nueva, **AUTOMATION_PLAN §16.13**, independiente del protocolo, que toda unidad lee en `MainSha`;
- **punto de entrada** (E.1, E.3). `/v1` no aporta ninguno que valga para todo contrato: el contrato real de I-61 G3 lee §16 en `AuthorityRevision`, así que
  el puntero de §16 no lo alcanza. Por eso es un **delta normativo de I-62 bajo OD-1**: una sección nueva de WORKFLOW, «Coexistencia de protocolos de
  ejecución delegada». Se coloca ahí por dos reglas ya integradas que no dependen de las `Authorities` de ningún contrato:
  - **WORKFLOW §10**, con el mismo texto en el `AuthorityRevision` de G3 y en `main`: hace de WORKFLOW el dueño de «transición» y deja la ejecución
    delegada (AUTOMATION_PLAN) «dentro de las reglas de los dueños anteriores»;
  - toda unidad se rige por el WORKFLOW de `main` actual: §11.3 lo dice para las unidades V1 y V2 está activo (merge `8a021fb6`).

  Esa sección obliga a quien evalúa un contrato (el Coordinator, la sesión que releva y el Controller, a través de la entrada que le pasa la sesión) a aplicar
  §16.13 antes de 16.3;
- §16.3 conserva literalmente el texto de I-61 y añade una frase puntero; otra frase puntero abre §16. Son una vía **redundante** para los contratos que
  leen §16 en `MainSha`. La cadena no depende de ellas;
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
| **F3** | Binding, independencia, `binding/v1`, `gate-contract/v2`, `delegation/v2`, A1'-A8' y las 14 comprobaciones (Anexo G); contratos `role-invocation/v1`, `input-closure/v1`, `input-fidelity/v1`, `architect-review-result/v1` y `reviewer-result/v1` (B.9, B.10) | C-11..C-14, C-30, C-40 | — |
| **F4** | Custodia: `state/v2` completo con su validador semántico (B.8), arranque BOOTSTRAP → G0 → QU (§8.6), aplicabilidad y adopción (§8.7, §14.0), rebase dentro y fuera de ventana (§8.8), toma con rebase (§8.9), orquestación autónoma (§20: `orchestration`, `NextAction`, bucle del Architect, materialización autorizada, intentos con reserva y recuperación, presupuestos, linaje y disposiciones, cierre y fidelidad de insumos, identidad observada, AUTONOMY_GAP), puntos BOOTSTRAP/Q0/Q7/QU/QH/QR, diario encadenado, reconstrucción sin diario, commits sin verificar; transiciones; presupuestos. Adopción: punto de entrada de WORKFLOW, §16.13, punteros, mapa de cláusulas y su esquema (Anexo E). Planos. **Cierre = MC_I62** | C-15..C-21 (C-20a, C-20b, C-20c), C-28, C-29..C-38, C-40, C-41, C-42 | — |
| **F6** | Fixture arrancado (D.1) y escenarios FX-01, FX-02, FX-03, FX-04a, FX-04b, FX-05 y FX-06 (D.3, D.4, D.8) | C-22..C-27 (C-25a, C-25b), C-39, C-42 (7, 8) | OD-5; OD-7 (**precondición del cierre de F6** por FX-02: sin CI no hay VERIFIED); OD-2 (A y FX-04b); OD-3 (B); OD-4 (B y FX-04b). **FX-04a no depende de OD-3, OD-4 ni OD-7** |
| **F7** | Preparación del cierre sin cambio normativo: borrador factual de FOUNDATIONS en la evidencia, ideas-futuras, paquete OV | — | no depende de READY-06 |
| **READY-01..09** | LIFECYCLE §8 | — | OD-1 antes de READY-03 |
| **FINAL_CANDIDATE_SHA** | Full, CI exacta, OV final | OV-I62-01..06 | — |
| **Cierre documental** | FOUNDATIONS publicada, **índice ADR**, HANDOFF, ROADMAP (ventana) | — | WORKFLOW §11.4-11.5 |
| **Integración** | merge efectivo con el punto de entrada de WORKFLOW, §16.13, punteros, mapa regenerado y revisado sobre el merge local (C-20b) antes del push; PRE en el cuerpo; POST y blob del mapa en el tag | C-20b | WORKFLOW §11.5-11.6 |

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
| OV-I62-06 autonomía | F6 | C-39 (y C-32, C-37) | FX-06 compacto sobre el fixture resembrado desde el Candidato: PASS solo con `OWNER_AS_MESSAGE_BUS` = false; si no, decisión del Owner sobre la limitación (UNVERIFIED o UNSUPPORTED con causa), y el escenario no se retira |

**Decisiones del Owner:**

| Id | Decisión | Bloquea | Momento |
|---|---|---|---|
| **OD-6** | predicado de independencia de LIFECYCLE (§11.4) | acuerdo y Freeze de F0 | pendiente |
| OD-1 | ADR sucesor (ampliación de §16, §16.13, fila de WORKFLOW §10, **sección de WORKFLOW «Coexistencia de protocolos de ejecución delegada», punto de entrada de compatibilidad**, **aplicabilidad DIRECT_ONLY / I62_DELEGATED con su marcador de G0 y la adopción posterior**, **rebase fuera de ventana con QU de reconciliación, reconciliación en el cierre de una ventana con rebase y toma con rebase (REBASE_TAKEOVER)**, **orquestación autónoma de roles (§20), con la materialización autorizada de bindings del Architect, los contratos de salida por rol, el cierre efectivo de insumos con la exención acotada de acciones iniciales incompatibles, sin equivalencia de evidencia, y la fidelidad de los insumos**; con OD-6 alternativa 1, LIFECYCLE) | READY-03 y vigencia | antes de READY-03 |
| OD-2 | línea base de huella por adapter (`config.toml`: `37DD3559…` registrada frente a `42E15A03…` observada) | toda invocación afectada (`codex-cli` en A, B y FX-04b; `codex-desktop-session` si comparte) | antes de la primera invocación afectada |
| OD-3 | autenticar Claude CLI | `claude-cli` en B | antes de F6 B |
| OD-4 | sandbox de Codex para escritura | Workers Codex (B y FX-04b) | antes de F6 B y de FX-04b |
| OD-5 | permiso de ensayo con la semántica única de §15 y apertura de sesiones del sistema bajo prueba, incluidas las invocaciones de Architect de FX-06 (autorización previa de apertura y consumo, no transporte) | F6 | antes de F6 |
| OD-7 | remoto del fixture con CI | **cierre de F6** (FX-02 no puede ser PASS sin CI); FX-04b | antes de F6 |

Cada solicitud va aparte, con la ruta y el hash actuales, el efecto, el consumo, el alcance y las restricciones. El silencio no es decisión.

## 19. Riesgos

Los 12 retos de la sección ARCHITECT del [mandato](../automation/decisions/I-62-owner-mandate.txt), con dónde los trata V11 y su riesgo residual (A62-V9-O01):

| # | Reto del mandato | Dónde lo trata V10 | Riesgo residual |
|---|---|---|---|
| 1 | provider identity leaking into roles | §2 (roles sin proveedor); C-01, C-02, C-05 | los proveedores viven en descriptores y catálogo; las guardas RG lo vigilan |
| 2 | unverifiable current-model assumptions | §4.2 (UNKNOWN nunca es MATCH); §10; §20.3.2 | el modelo servido detrás del proveedor no es observable; límite declarado |
| 3 | fake self-introspection | §10 (la autodeclaración no es fuente); §20.3.2 (identidad observada por el invocador) | depende de que cada adapter tenga una fuente RUNTIME_OBSERVED (F2) |
| 4 | hidden private-memory dependencies | §20.3.1 (cierre de insumos); §20.4 (`NextAction` sin chat ni memoria); D.6 | la auditoría de lecturas no captura las lecturas internas del runtime (cobertura declarada) |
| 5 | over-complex role router | §5 (algoritmo de siete pasos); §20.10 (Level A, sin servicio) | número de contratos nuevos; se mitiga con Level A |
| 6 | excessive cross-provider requirements | §11 (Proveedor PREFERRED salvo política explícita) | — |
| 7 | security/auth leakage | §6; C-10; OD-2..OD-4 | las huellas de configuración solo registran nombres de claves y hashes |
| 8 | ambiguous custody | §8; B.8 (puntos durables, CAS); §20.6 (intentos) | Git no prueba quién opera en otra máquina (§8.5) |
| 9 | recovery that can overwrite work | §9.2 (T10-T22); B.8.5; §20.6 (recuperación por estado sin relanzar a ciegas) | depende de la terminación acreditada |
| 10 | Level B automation without demonstrated need | §17 (Level A); §20.10 | — |
| 11 | provider-specific schemas | B.1 (esquemas neutrales); B.3 (`Facts` es la única frontera abierta) | — |
| 12 | inability to resume on another machine | §8.9; §20.8; F.6-F.8; FX-04a | FX-04b y FX-06 dependen de OD |

## 20. Orquestación autónoma de roles (D-19; R62-AUTO-01..20)

**Por qué existe** (evidencia operativa de I-62, §20.11). El protocolo define roles, bindings, exact-SHA, reintentos, custodia y relevos. Sin embargo, el
bucle de control de la iniciativa todavía depende de que el Owner transporte a mano prompts y resultados entre los roles. La continuidad queda entonces en el
chat, en la memoria privada de un modelo o en la del Owner, y eso contradice el objetivo de portabilidad de contexto. I-61 automatizó una unidad de ejecución
delegada; este capítulo cierra el hueco a nivel de iniciativa.

**Alcance:** solo las unidades **I62_DELEGATED** y los contextos de revisión o piloto que adoptan este protocolo de forma explícita (§20.12). Una unidad
DIRECT_ONLY no se ve afectada.

### 20.1 Relevo frente a escalada (R62-AUTO-01)

| Concepto | Definición | Cuándo |
|---|---|---|
| **RELAY** | transporte de información canónica entre roles IA del protocolo (PRINCIPAL_COORDINATOR, ARCHITECT, EXECUTION_CONTROLLER, WORKER, REVIEWER) | **automático** siempre que la autoridad vigente permita la invocación, exista un binding elegible, se cumpla la independencia exigida y el presupuesto lo permita. El Owner **no** es el mecanismo normal |
| **ESCALATION_OWNER** | interacción con el Owner | solo ante una frontera que de verdad lo exige (tabla siguiente) |
| **COORDINATOR_DECISION** | decisión de la autoridad de gates (el Coordinator, que no es vinculable, §2) | cuando la acción exige autoridad del Coordinator fuera de la autorización vigente (§20.5): un GATE PASS, una corrección fuera del alcance autorizado, un binding que ninguna autorización de materialización cubre, una A-n. Es autoridad, no relevo; si su transporte depende del Owner, es un AUTONOMY_GAP (§20.9) |

**Condiciones de ESCALATION_OWNER**, como mínimo:
- decisión OWNER-RESERVED o elección de política;
- autorización destructiva o irreversible;
- credenciales, suscripción o compra;
- Owner Validation o interacción manual exigida;
- ambigüedad material cuya resolución pertenece al Owner;
- presupuesto de reintentos o de revisión agotado;
- STOP asignado a la autoridad del Owner.

**«Tiene que ejecutarse otro rol IA» no es, por sí solo, una condición de escalada.**

### 20.2 El Principal orquesta; la autoridad no cambia (R62-AUTO-02, R62-AUTO-14)

En una unidad I62_DELEGATED, el PRINCIPAL_COORDINATOR es dueño de la orquestación de las invocaciones de rol elegibles. Puede invocar, a través de los
adapters de §7, al ARCHITECT, al EXECUTION_CONTROLLER, al WORKER, al REVIEWER y a un PRINCIPAL sucesor cuando lo exige una transición autorizada de
portabilidad o de recuperación (§8.9, §9.2). En cada paso:
1. determina el rol requerido a partir del estado canónico (`NextAction`, §20.4);
2. resuelve un binding elegible (§5, §11);
3. lo invoca con un `RoleInvocation` (§20.3);
4. valida el artefacto devuelto contra su contrato de salida;
5. actualiza el estado canónico;
6. deriva la acción siguiente.

**Invariante de autoridad (AUT-I1).** La autonomía cambia el transporte y la orquestación, nunca la autoridad. El Principal **nunca**:
- declara un AGREED del Architect ni cierra o rebaja un REQUIRED del Architect, propio o ajeno (§20.5.2);
- materializa un binding que no cumpla exactamente la autorización vigente (§20.5.1);
- usa un resultado de REVIEWER para satisfacer al ARCHITECT (§20.7);
- elige una decisión del Owner;
- declara un Consensus Freeze ni un GATE PASS fuera de su autoridad;
- omite la independencia exigida, excede un presupuesto u omite un STOP.

Un estado que lo afirme es inválido (I-S18, I-P13; C-38).

### 20.3 Contrato neutral de invocación de rol (R62-AUTO-03, R62-AUTO-13)

`rackcad-role-invocation/v1` (B.9) es el contrato semántico de toda invocación de rol. Sus propiedades:
- **sin texto de prompt de ningún proveedor:** el adapter renderiza la invocación concreta (CLI o sesión). El prompt es representación, nunca autoridad;
- **objeto exacto obligatorio** cuando se revisa o verifica algo: `TargetSha` más ruta y blob;
- la salida del rol identifica el objeto exacto revisado o ejecutado;
- **tres identidades distintas** (§20.6): la solicitud lógica (`LogicalReviewRequestId`), el contrato de cada intento físico (`InvocationId`) y el lanzamiento
  del proceso o sesión (`RunId`, §9.1);
- **permisos mínimos:** los roles de solo lectura (ARCHITECT, EXECUTION_CONTROLLER, REVIEWER) siguen siendo de solo lectura;
- **contrato de salida fijado por el rol y la acción** (§20.7, B.10).

**Invocación limpia del Architect** (R62-AUTO-13):
- una sesión o proceso elegible y distinto;
- el SHA, la ruta y el blob exactos revisados;
- solo el **cierre efectivo de insumos** (§20.3.1), calculado y custodiado antes de lanzar: ni transcripción del autor, ni memoria privada del autor (p. ej.,
  una ruta de trabajo limpia sin la memoria de proyecto de la sesión autora);
- el contexto inyectado automáticamente, declarado en el resultado y contrastado con el cierre;
- la **fidelidad de los insumos**, acreditada por el invocador antes de lanzar y comprobada tras la corrida (§20.3.3);
- permisos de solo lectura;
- un resultado estructurado (B.10), la terminación acreditada (§9.1) y la identidad del runtime observada por el invocador (§20.3.2).

Si OD-6 elige la alternativa 1, se aplica además su predicado (§11.4). El Principal debe elegir un binding que cumpla la política aplicable; sin binding
elegible, no hay invocación (P-10).

#### 20.3.1 Cierre efectivo de insumos (GAP-07)

Una `RoleInvocation` distingue:

| Clase | Contenido | Quién la fija |
|---|---|---|
| `CanonicalInputs[]` | los artefactos versionados que la acción necesita: el objeto exacto, sus registros de revisión y sus autoridades | el Principal, según `NextAction` y la autorización |
| `AllowedTransitiveInputs[]` | los archivos que un insumo canónico, o una instrucción que el runtime inyecta automáticamente (`AGENTS.md`, `CLAUDE.md` del directorio), **obliga normativamente** a leer; cada uno con la obligación que lo exige (ruta, sección y blob de origen) | el cálculo del cierre, antes de lanzar |
| `DeclaredRuntimeContext[]` | instrucciones de sistema y del runtime, mensajes de herramientas y catálogos que el adapter inyecta, con su tamaño o hash cuando el adapter lo expone | el adapter (operación 1, describir) |
| `ForbiddenInputs[]` | transcripción y memoria del autor, sesiones ajenas, worktrees reales de otras unidades, artefactos transitorios (D.6) | el contrato, siempre |

**Cálculo del cierre.** Se hace antes de cualquier lanzamiento y se custodia (`rackcad-input-closure/v1`, B.9) a más tardar en el QU que reserva el intento
(§20.6):
1. Se parte de `CanonicalInputs` y de las instrucciones automáticas del runtime en el directorio de trabajo. Dependen del adapter: `codex-cli` carga
   `AGENTS.md`; un adapter de Claude carga además `CLAUDE.md`.
2. En cada archivo se identifican las **obligaciones de lectura o de acción** (p. ej., una sección «Leer primero») y se clasifican:
   - **READ:** obligación de leer otro archivo → entra en `AllowedTransitiveInputs` (**opción A**, por defecto);
   - **ACTION_COMPATIBLE:** acción compatible con los permisos de la invocación (p. ej., `git log`, de solo lectura) → se registra como acción permitida;
   - **ACTION_INCOMPATIBLE:** acción que las restricciones de aislamiento de la invocación impiden (p. ej., compilar o probar en una invocación de solo
     lectura) → solo puede omitirse mediante una **exención explícita y acotada** a esa invocación y esa acción, de una autoridad aplicable (**opción B**),
     identificada por ruta, sección y blob, o por la `DecisionRef` de la autorización de la invocación. Sin exención, no hay lanzamiento (STOP,
     COORDINATOR_DECISION);
   - **CONDITIONAL_NOT_TRIGGERED:** obligación condicionada a algo que la acción no hace (p. ej., «si tocas el plugin, lee …» en una revisión de solo lectura)
     → se registra con su motivo. Si la condición es ambigua, se trata como READ.
3. Se repite sobre los archivos añadidos hasta el punto fijo; un ciclo no añade nada.
4. El cierre fija la `AuthorityRevision`, el blob de cada archivo y el blob de cada instrucción de la que se derivó una obligación. Si cambia uno de esos blobs,
   el cierre se recalcula antes de lanzar.

Nunca se pide al revisor que incumpla una instrucción normativa del repositorio: lo que obliga a leer se incluye (opción A), y una acción incompatible se
exime con autoridad (opción B).

**Aplicación a RackCad.** Es una ilustración sobre `AGENTS.md` en la base `819955d6`; el cierre real se calcula sobre la `AuthorityRevision` de cada
invocación:

| Obligación de «Leer primero» | Clase | Resolución |
|---|---|---|
| 1. `docs/HANDOFF.md` | READ | opción A: transitivo permitido |
| 2. `README.md` | READ | opción A |
| 3. `docs/ARCHITECTURE.md` y los Context Packs que declara la iniciativa (en I-62, `documentation-governance`, con el índice `docs/context-packs/README.md`) | READ | opción A |
| 4. `git log --oneline -10` | ACTION_COMPATIBLE | permitida (lectura de Git) |
| 4. `dotnet test` | ACTION_INCOMPATIBLE con solo lectura | opción B: exención explícita y acotada en la autorización de la invocación. Sin exención, no hay lanzamiento |

**Exención acotada, sin equivalencia de evidencia** (A62-V10-01). Una exención de opción B **omite** una acción; no la sustituye por otra evidencia. Para el
Architect de solo lectura:
- `dotnet test` puede eximirse de forma explícita, solo para esa invocación;
- la CI exacta de publicación del `Target` puede entregarse como **señal canónica separada** de salud de publicación (`HealthSignals`, B.9);
- esa CI **no** es evidencia equivalente a Core local, **no** satisface ninguna clase de prueba local de AGENTS y **no** se propaga a la evidencia de
  gates, Candidato, cierre ni implementación;
- sin exención, la invocación no se lanza.

Bajo OD-1, el ADR sucesor puede proponer para §16 una regla permanente que autorice esta exención a los roles de solo lectura invocados por una
`RoleInvocation`. Esa regla tendría los mismos límites y declararía de forma expresa que no sustituye ninguna clase de evidencia de gate, Candidato o cierre.

**Verificación.** Tras la terminación, el invocador compara con el cierre las lecturas registradas por el adapter (comandos del registro de sesión de Codex,
llamadas a herramientas de la transcripción de Claude):
- una lectura registrada fuera del cierre → **INVALID_REVIEW_CONTEXT** (P-22): el resultado no se ingiere como dictamen y el intento cuenta;
- un registro sin lecturas o incompleto → la dimensión Contexto queda UNKNOWN (D.6), y bajo la alternativa 1 de OD-6 no satisface el predicado;
- la cobertura del registro se declara: no captura las lecturas internas del runtime.

#### 20.3.2 Identidad del revisor: declarada frente a observada (GAP-08)

| Identidad | Quién la produce | Valor como evidencia |
|---|---|---|
| `ReviewerDeclaredIdentity` | el revisor, en su resultado (B.10.0) | informativa. Puede ser UNKNOWN. Nunca acredita modelo, effort, sesión ni hilo, y nunca declara MATCH |
| `InvokerObservedRuntimeIdentity` | el invocador, desde los hechos del adapter (§10): modelo y effort del `turn_context` o de `get_session`, `thread_id` o `sessionId`, sandbox, versión y huella | la única fuente de modelo, effort, sesión y runtime efectivos, al nivel observado (RUNTIME_OBSERVED como mínimo para los obligatorios) |

El intento custodia la identidad observada en su `relay-record/v2` (`Participant.Observation`), y el estado la referencia como `RuntimeEvidenceRef` (B.8.8).
El resultado puede citar esa referencia, pero no la sustituye. El `CONFIGURATION_STATUS` del revisor se calcula con la identidad observada (§4.2).

**Contradicción** entre una declaración concreta del revisor y la identidad observada (p. ej., declara otro modelo, o la misma sesión que el autor) →
**S-04** (P-23): el resultado no se ingiere y el bucle se detiene hasta la decisión. Una declaración UNKNOWN no es contradicción.

#### 20.3.3 Fidelidad de los insumos canónicos (R62-FIDELITY-01..06)

**Por qué** (GAP-09, medido). En la revisión formal de V10, y antes en la de V9, el camino de lectura del runtime degradó el texto canónico:
- ≤ y ≥ se convirtieron en «=», y ≠ en «?»;
- las flechas y los símbolos lógicos se volvieron caracteres de control o se perdieron;
- las letras acentuadas pasaron a U+FFFD.

Un hallazgo (A62-V10-02) nació de una proposición que no existe en la Proposal. El bucle autónomo **no** acepta una revisión si el revisor no recibió una
representación fiel de los insumos canónicos, y no depende de que el revisor lo note.

**Contrato** (`rackcad-input-fidelity/v1`, B.1 y B.9). Un registro por cada insumo del cierre (§20.3.1) y por el prompt renderizado:

```text
CanonicalInputFidelity { InputRef, GitBlob, CanonicalSha256, Encoding, TransportRepresentation, TransportSha256 | VerificationMethod,
                         CharacterClassesChecked[], FidelityStatus, DegradedSpans }
```

| Campo | Contenido |
|---|---|
| `InputRef` | ruta del cierre, o `PROMPT` para la invocación renderizada |
| `GitBlob`, `CanonicalSha256` | identidad de los bytes canónicos en la `AuthorityRevision` (para `PROMPT`, el artefacto custodiado) |
| `Encoding` | codificación canónica del texto del repositorio: **UTF-8 según los bytes reales** del archivo, con la presencia de BOM y el fin de línea registrados |
| `TransportRepresentation` | cómo llega el texto al revisor (lectura por herramienta del runtime en el clon, salida de consola, inyección en el prompt) y con qué configuración de codificación |
| `TransportSha256` o `VerificationMethod` | hash de la representación capturada tras la normalización permitida, o el método equivalente de comparación por segmentos |
| `CharacterClassesChecked[]` | `{Character, CountCanonical, CountTransport}` por **cada carácter no ASCII distinto presente en el corpus** del cierre: letras latinas acentuadas, ≤, ≥, ≠, →, ⇒, ⇔, ∈ y los demás que aparezcan. No se exigen clases ausentes del corpus |
| `FidelityStatus` | FAITHFUL \| FAITHFUL_NORMALIZED \| DEGRADED_BOUNDED \| DEGRADED_UNBOUNDED \| UNVERIFIED |
| `DegradedSpans` | `StateRef` del mapa `{Path, Line, Column, Canonical, Visible, Class (SYMBOL \| LETTER \| STRUCTURE)}`; vacío con FAITHFUL o FAITHFUL_NORMALIZED |

**Normalización permitida** (distinción A de R62-FIDELITY-02): el único cambio de bytes que se acepta como semánticamente fiel es el fin de línea (CRLF ↔ LF)
y la presencia o ausencia del salto final. Da FAITHFUL_NORMALIZED. **Cualquier otra diferencia es degradación semántica** (distinción B), por ejemplo:
- ≤ → «=» o ≠ → «?»;
- un identificador o una ruta corrompidos;
- una negación eliminada;
- U+FFFD en una palabra normativa;
- la estructura de una tabla o de un operador perdida.

**Antes de lanzar** (R62-FIDELITY-01, -05), el invocador:
1. determina los bytes canónicos y el blob de cada insumo;
2. configura en UTF-8 el transporte y el runtime, donde aplique;
3. comprueba por el **mismo camino de lectura** que usará el revisor (mismo runtime, mismo shell, misma configuración y la forma de lectura que la
   invocación le indica) que se conservan todos los caracteres no ASCII presentes en el corpus, y que el prompt renderizado llega igual;
4. custodia la evidencia (`InputFidelityEvidenceRef`) a más tardar en el QU que reserva el intento (§20.6).

Sin un preflight FAITHFUL o FAITHFUL_NORMALIZED, no hay lanzamiento (P-24).

Para `codex-cli` con PowerShell en Windows, el adapter **establece y verifica** un transporte seguro en UTF-8 en lugar de asumir los valores por defecto de la
terminal. Puede usar salida de consola en UTF-8, lectura directa de archivos sin conversión con pérdida, lecturas estructuradas u otro método verificado. **No**
se congela ningún comando concreto: se congela el requisito observable de que el texto canónico que ve el revisor sea fiel (R62-FIDELITY-05).

**Después de la terminación**, el invocador compara con el texto canónico, tras la normalización permitida, cada segmento capturado de las lecturas del revisor
(las salidas de herramientas que registra el adapter). Fija `FidelityStatus` y `DegradedSpans` en el QU de RESULT_RECEIVED.

**Fallo cerrado e invalidación acotada por hallazgo** (R62-FIDELITY-02, -03):

| `FidelityStatus` tras la corrida | Ingestión |
|---|---|
| FAITHFUL o FAITHFUL_NORMALIZED | normal (§20.5.2) |
| DEGRADED_BOUNDED: hay degradación y `DegradedSpans` la localiza | **INPUT_FIDELITY_INVALID** para lo afectado (P-25). Cada hallazgo y cada disposición se evalúa por sus `PremiseRefs` (B.10.1): es **independiente** solo si todas sus premisas son fieles (abajo). Los independientes se ingieren; los demás quedan **INVALID_PREMISE / UNACCREDITED**, no abren, cierran ni sustituyen linajes, y el resultado registra sus `FindingId`. Un veredicto así nunca produce ARCHITECT_SATISFIED |
| DEGRADED_UNBOUNDED o UNVERIFIED: no se puede localizar la degradación, o no hay captura de las lecturas | **INPUT_FIDELITY_INVALID** para todo el resultado: no se ingiere nada; el intento cuenta |

Una premisa (`PremiseRefs[] {Path, Section, Quote}`) es **fiel** si:
- con `Quote` no vacío, la cita coincide con el texto canónico de `Path` dentro de `Section`, tras la normalización permitida. Una cita que solo coincide con
  la representación degradada, o con ninguna, no es fiel;
- con `Quote` vacío (premisa de sección completa), la representación que vio el revisor de esa sección no tiene ningún tramo en `DegradedSpans`.

Un hallazgo sin `PremiseRefs`, o una premisa que no se puede ubicar, cuenta como no independiente (conservador). Si no se puede establecer la independencia de
ninguno, se invalida todo el resultado.

**Observada por el invocador, no autodeclarada** (R62-FIDELITY-04). La fidelidad es evidencia de la capa de invocación y de transporte, como el
`RuntimeEvidenceRef` (§20.3.2). El revisor no la certifica. Su resultado puede citar el `InputFidelityEvidenceRef` que recibe en la `RoleInvocation`, pero no
crea esa evidencia. Cambiar de proveedor, de runtime o de adapter no exime de la validación: cada intento lleva la suya.

**Caso de V10 (medido).** La premisa de A62-V10-02 («`review_rounds` = `logical_requests`») no coincide con el texto canónico («≤»): bajo este contrato, es
INVALID_PREMISE. La independencia de A62-V10-01, -03 y -04 la estableció el Coordinator por su autoridad, antes de que existieran este contrato y los
`PremiseRefs`; no es una aplicación mecánica de §20.3.3.

### 20.4 Siguiente acción durable (R62-AUTO-04, R62-AUTO-16)

El estado `/v2` lleva una sección estructurada **`orchestration`** (B.8.8), no prosa en `automation_state.next_action`. Con ella, un Principal nuevo y sin
contexto previo determina:

| Campo | Contenido |
|---|---|
| `next_action.role` y `next_action.action` | el rol y la acción siguientes (p. ej., ARCHITECT / REVIEW; PRINCIPAL / CORRECT_AND_REREVIEW; EXECUTION_CONTROLLER / PLAN; OWNER / DECIDE) |
| `target` | `{commit, path, blob}` exacto, o `null` si no aplica |
| `unit`, `gate`, `task_id` | — |
| `required_inputs[]`, `required_capabilities[]`, `required_independence` | lo que exige la invocación; los insumos, como cierre efectivo (§20.3.1) |
| `invocation_permission` | la `StateRef` de la autorización vigente: el contrato de gate o la `ReviewLoopAuthorization` |
| `budget_remaining` | por contador (§20.6) |
| `expected_output` | el contrato de salida (B.9, B.10) |
| `completion_condition`, `stop_conditions[]`, `escalation_conditions[]` | — |

**Regla de unicidad.** `NextAction` es una función del estado y de los artefactos custodiados, y debe dar **una sola** acción. Si no es derivable de forma
única: STOP por ambigüedad material (P-17). Ningún hecho de continuación necesario puede existir solo en el chat anterior, en una memoria privada, en la
memoria del Owner o en un prompt copiado a mano.

**Puntos durables.** La orquestación se actualiza en los puntos de §8.1. En el bucle de revisión, cada intento de invocación recorre sus QU ORDINARY (§20.6):
la reserva antes de lanzar, el lanzamiento, el resultado recibido y custodiado, y la ingestión. En el bucle de ejecución, los Q0 y Q7 de siempre. La invocación
del Architect no es una ventana de §8.2: es de solo lectura y no escribe en Git. Los QU del Principal durante la invocación no cambian lo que lee el revisor,
que es un commit exacto.

### 20.5 Bucle autónomo de revisión del Architect (R62-AUTO-05, R62-AUTO-06, R62-AUTO-08)

**Autorización.** El bucle se ejecuta bajo una **`ReviewLoopAuthorization`** emitida por el Coordinator y registrada en `decisions/<unit>.md` con el marcador
`I62-REVIEW-LOOP-AUTHORIZATION: <AuthorizationId>` (custodiada como `StateRef`). Fija:
- el objeto, o la familia de versiones, a revisar;
- el **alcance de corrección permitido**, incluidos los cierres que no se reabren;
- los topes del bucle (§20.6), nunca mayores que los congelados;
- la **autorización de materialización** de bindings del Architect (§20.5.1): rol, acciones, capacidades, independencia, celdas, límites de modelo y effort,
  permisos, presupuesto, objeto y alcance temporal.
- las exenciones de opción B que autoriza para las invocaciones del bucle (§20.3.1), si las hay, cada una con la obligación exacta que exime.

Dentro de esa autorización, el Principal corrige y vuelve a pedir revisión sin relevo del Owner y sin una decisión del Coordinator por ronda. Una corrección que
excede el alcance es COORDINATOR_DECISION.

**Estados** (`orchestration.loop.phase`):

```text
REVIEW_PENDING → ARCHITECT_INVOKED → RESULT_INGESTED → { AGREED            → ARCHITECT_SATISFIED(objeto exacto) → siguiente gate / decisión del Owner
                                                        { CHANGES_REQUIRED  → CORRECTING → PUBLISHED → CI_VERIFIED → REREVIEW_PENDING → ARCHITECT_INVOKED …
                                                        { BLOCKED_OWNER     → ESCALATE_OWNER (decisión exacta requerida)
                                                        { INVALID           → reejecución de transporte dentro del tope; agotado → STOP (P-19)
```

**Puntos durables de cada fase:**

| Fase | Punto durable que la publica | Qué cambia en ese punto | Qué no cambia |
|---|---|---|---|
| REVIEW_PENDING / REREVIEW_PENDING | QU ORDINARY | solicitud lógica abierta; su intento en INVOCATION_PLANNED o BUDGET_RESERVED (§20.6) | `loop.object` |
| ARCHITECT_INVOKED | QU ORDINARY | el intento en LAUNCHING; después, LAUNCHED o RESULT_RECEIVED (§20.6) | `loop.object` |
| RESULT_INGESTED | QU ORDINARY | el intento en RESULT_INGESTED; las disposiciones del resultado, aplicadas al linaje; la fase siguiente según el veredicto | `loop.object` |
| CORRECTING | el mismo QU de la ingestión (CHANGES REQUIRED) | `next_action` = PRINCIPAL / CORRECT_AND_REREVIEW | `loop.object` |
| PUBLISHED | el QU que sigue al commit de la versión nueva X2. Ese commit se publica solo, para que su CI corra sobre él | **`loop.object` = X2** (commit, ruta, blob); `correction_rounds` +1; `corrections_by_lineage` +1 en cada linaje que la versión corrige; las respuestas del Principal, custodiadas | contadores de revisión |
| CI_VERIFIED | QU ORDINARY | la evidencia de la CI exacta del commit de X2 | `loop.object` |
| ARCHITECT_SATISFIED | el QU de la ingestión (AGREED) | fase final del bucle sobre el blob de `loop.object` | — |

**`loop.object` cambia en un solo par de puntos durables: CORRECTING → PUBLISHED** (A62-V9-06). Ni la apertura de la re-revisión ni el lanzamiento lo tocan:
el intento siguiente se planifica ya sobre X2. La primera fijación de `loop.object` es el QU que abre el bucle (NONE → REVIEW_PENDING).

**CHANGES REQUIRED por sí solo no exige intervención del Owner.** Con CHANGES REQUIRED, el Principal:
1. ingiere el resultado: **todas** las disposiciones y los REQUIRED nuevos (§20.5.2);
2. registra su **respuesta** a cada hallazgo abierto (qué corrige y dónde) dentro del alcance autorizado;
3. corrige y publica una versión exacta nueva (PUBLISHED);
4. valida la evidencia y la CI exigidas (CI_VERIFIED);
5. reutiliza o materializa un binding de Architect elegible (§20.5.1) y abre una solicitud lógica nueva (REREVIEW_PENDING);
6. repite dentro del presupuesto.

Si `review_rounds` ya está en su tope, no hay re-revisión posible: STOP y escalada (P-18) sin corregir.

**Contrato del resultado** (B.10.1, `rackcad-architect-review-result/v1`): el Principal lo ingiere mecánicamente. Un resultado inválido o incompleto no avanza
el estado (P-19), por ejemplo:
- falla el esquema, o el esquema no es el `OutputContract` del rol y la acción (§20.7);
- el blob o el commit no coinciden con el `RoleInvocation`;
- falta la declaración de contexto inyectado o la evidencia de independencia;
- un REQUIRED no tiene id, sección, evidencia o corrección;
- el veredicto es AGREED y falta la disposición de algún hallazgo abierto (§20.5.2);
- hay una lectura fuera del cierre (P-22) o una contradicción de identidad (P-23);
- la representación que vio el revisor no es fiel (P-25), para lo afectado (§20.3.3).

#### 20.5.1 Materialización autorizada de bindings del Architect (A62-V9-01)

Un binding ARCHITECT nuevo (por ejemplo, el del Architect de la re-revisión tras CHANGES REQUIRED) no necesita una aceptación individual del Coordinator en
cada ronda: la `ReviewLoopAuthorization` autoriza su **materialización** dentro de límites cerrados. La autorización de materialización fija, como mínimo:

| Campo | Contenido |
|---|---|
| `Role` | ARCHITECT (único valor admitido en una `ReviewLoopAuthorization`) |
| `AuthorizedActions[]` | REVIEW_DESIGN |
| `MinimumCapabilities[]` | `{RequirementId, Required}` del perfil del rol en `routing.md` (blob en la `AuthorityRevision`); cada uno, en MATCH o ABOVE_REQUIRED (§4.2) |
| `RequiredIndependence` | dimensiones de §11 frente a cada autor del objeto (`ReviewSubject`) y frente a los Architects anteriores del bucle, con REQUIRED, PREFERRED o NOT_REQUIRED; con la alternativa 1 de OD-6, al menos su predicado (§11.4) |
| `EligibleCells` | lista cerrada de celdas del catálogo, o el criterio cerrado de elegibilidad de ADR-0046 #4 (invocación medida, consumo cubierto, celda no `STALE`) aplicado al catálogo de la `AuthorityRevision` |
| `ModelEffortBounds` | `{MinLevel, MinEffort, MaxLevel, MaxEffort}`; los máximos, nullables = no aplica |
| `Permissions` | READ_ONLY, exactamente |
| `Budget` | topes del bucle (§20.6), menores o iguales que los congelados |
| `ObjectFamily` | unidad, ruta o patrón de rutas de las versiones (p. ej., `docs/initiatives/I-62-proposal-v*.md`), versión inicial y alcance de corrección |
| `Validity` | `{FromRecordVersion, Until}`: fija la **vigencia de acción** (abajo). Termina en ARCHITECT_SATISFIED, en el agotamiento, al pasar `Until`, o con la revocación, la sustitución o la enmienda por una decisión posterior del Coordinator, lo que ocurra antes |

El Principal puede hacer lo siguiente **solo si se cumple exactamente la autorización**:
1. observar un candidato (preflight de §6, acción REVIEW_DESIGN);
2. comprobar cada criterio y registrar `{CriterionId, Required, Observed, Result (SATISFIED | NOT_SATISFIED | UNKNOWN), Evidence}`;
3. materializar el binding: `Acceptance` = `{State: ACCEPTED, Basis: AUTHORIZED_MATERIALIZATION, DecisionRef: null, AuthorizationRef, MaterializationCheck,
   Utc}` (B.5);
4. custodiarlo, con su preflight y su comprobación, a más tardar en el QU que reserva el intento (§20.6);
5. invocarlo.

No puede relajar ningún criterio: un UNKNOWN cuenta como NOT_SATISFIED (fail-closed). Si **ningún candidato satisface** la autorización, no hay invocación y el
bucle se detiene: COORDINATOR_DECISION (una autorización nueva o enmendada) o ESCALATION_OWNER cuando lo que falta es materia del Owner (p. ej.,
autenticación, huella o compra; §20.1).

**Sin aceptación fingida.** Un binding materializado lleva `DecisionRef` = `null` y `AuthorizationRef` = `{Path, Marker, AuthorizationId, Commit, Blob}`
(B.2). Nunca simula una decisión individual del Coordinator que no ocurrió. Quien acepta o valida el binding (A7', el validador de `orchestration`)
**reproduce** la comprobación con el preflight custodiado y con la autorización tal como figura en `AuthorizationRef.Commit`/`Blob`; si no la reproduce, el
binding es inválido.

**Vigencia de acción frente a acreditación histórica** (A62-V10-03):

| Concepto | Qué decide | Cuándo termina y qué conserva |
|---|---|---|
| **Vigencia de acción** | si la autorización permite **acciones nuevas**: materializar un binding, reservar una solicitud lógica o un intento nuevos, publicar un LAUNCHING | termina en el primero de estos: el punto durable que registra ARCHITECT_SATISFIED; el que registra el agotamiento (P-18); el commit que custodia en `decisions/<unit>.md` la revocación (`I62-REVIEW-LOOP-REVOCATION: <AuthorizationId>`), la sustitución o la enmienda por el Coordinator; o el paso del instante `Until`. Se registra en `orchestration.loop.action_validity` (B.8.8) |
| **Acreditación histórica** | si un binding ya materializado y usado mientras la autorización estaba vigente, su invocación completada, su resultado, sus disposiciones y un ARCHITECT_SATISFIED ya alcanzado siguen acreditados | permanente: el fin posterior de la vigencia **no** los invalida retroactivamente. Se apoya en lo custodiado en el punto de materialización: `AuthorizationRef` con su `Commit` y su `Blob` exactos, `MaterializationCheck`, `record_version` e instante de la materialización, identidad del binding y evidencia de capacidad e independencia (su preflight) |

**Regla determinista para los intentos en curso.** Un intento que alcanzó LAUNCHING en un punto durable **anterior** al fin de la vigencia de acción puede
completarse. LAUNCHED, LAUNCH_UNCERTAIN, RESULT_RECEIVED y RESULT_INGESTED no son acciones nuevas, y su resultado queda acreditado históricamente. Un intento en
INVOCATION_PLANNED o BUDGET_RESERVED al fin de la vigencia no se lanza: pasa a CANCELLED_BEFORE_LAUNCH, sin liberar presupuesto. Si el caso B.1 prueba que
un intento en LAUNCHING no arrancó, ese intento vuelve a BUDGET_RESERVED solo si la vigencia sigue abierta; si no, pasa a CANCELLED_BEFORE_LAUNCH.

El instante de una acción nueva es el que declara su QU (`reserved_utc`, `launching_utc`), contrastable con el push. Un QU con una acción nueva y un instante
posterior a `Until` es inválido.


#### 20.5.2 Disposición explícita por hallazgo (A62-V9-03)

La `RoleInvocation` entrega al Architect los hallazgos abiertos (`OpenFindings[]`, B.9). El resultado contiene una disposición por cada uno y por cada
hallazgo nuevo:

```text
FindingDisposition { FindingId, LineageId, State (OPEN | CLOSED | STILL_OPEN | SUPERSEDED), SupersededBy[], Rationale, EvaluatedObject {commit, path, blob} }
```

| Regla | Efecto en la ingestión |
|---|---|
| hallazgo abierto omitido en el resultado | **sigue abierto** (OPEN o STILL_OPEN); se registra la omisión |
| AGREED con un hallazgo abierto omitido | AGREED no cierra nada de forma implícita. El resultado es INVALID (P-19): AGREED exige cero REQUIRED abiertos sobre la versión exacta (LIFECYCLE §5) |
| CLOSED | solo si el revisor tiene autoridad para cerrarlo: un `architect-review-result/v1` de un binding ARCHITECT aceptado o materializado del bucle, para un linaje emitido por el ARCHITECT. Un linaje emitido por el Coordinator se cierra por decisión del Coordinator |
| rebaja de REQUIRED a OPTIONAL | solo por quien emitió el REQUIRED (LIFECYCLE §5), con id, clasificación, razón y evidencia. Un Architect posterior distinto del emisor puede cerrar por corrección comprobada, pero no rebajar. Si el emisor no está disponible, el hallazgo sigue abierto |
| SUPERSEDED | `SupersededBy[]` no vacío, con hallazgos nuevos del mismo resultado en el **mismo** `LineageId`; el linaje y su presupuesto continúan |
| `EvaluatedObject` distinto del objeto de la solicitud | el resultado no puede cerrar nada: INVALID |
| `FindingId` desconocido, o de otro linaje | INVALID |
| resultado de REVIEWER (§20.7) | no cierra ni rebaja hallazgos del ARCHITECT; si se intenta, P-20 |

El Principal registra su **respuesta** a cada hallazgo (`findings[].response`), nunca su estado. Una versión nueva de la Proposal no borra la historia de los
REQUIRED. El linaje (`orchestration.findings[]`, B.8.8) conserva la cadena hallazgo → respuesta → objeto cambiado → revisión siguiente → disposición.

### 20.6 Identidad lógica, intentos y presupuestos del bucle de revisión (R62-AUTO-07; A62-V9-02, A62-V9-04)

**Tres identidades** (A62-V9-04):

| Identidad | Qué identifica | Cambia cuando … |
|---|---|---|
| `LogicalReviewRequestId` | una solicitud de revisión de **un** objeto exacto en el bucle | se pide una revisión nueva: la de una versión nueva, o una segunda sobre la misma versión tras una decisión que lo autoriza (p. ej., tras BLOCKED_OWNER) |
| `InvocationId` | el contrato `RoleInvocation` de **un** intento físico, inmutable y custodiado | en cada intento nuevo; también si se replanifica un intento antes de lanzarlo (binding o cierre) |
| `RunId` | el lanzamiento del proceso o la sesión de ese intento (§9.1) | en cada lanzamiento |

La solicitud lógica es **estable durante sus reejecuciones de transporte**. Cambiar `RunId`, `InvocationId`, proveedor, modelo, binding o Principal **no** crea
presupuesto nuevo: todos los intentos de una solicitud cuentan contra la misma solicitud.

**Estados durables de un intento** (A62-V9-02). Cada uno se publica en un QU ORDINARY (§8.1) por CAS; el `pending_invocation` de V9 desaparece:

| Estado | Significado | Se publica |
|---|---|---|
| INVOCATION_PLANNED | `RoleInvocation` construida y custodiada (objeto, cierre de insumos, binding), sin reserva | opcional: cuando la reserva todavía no es posible (p. ej., en espera de una materialización) |
| BUDGET_RESERVED | solicitud lógica fijada y presupuesto reservado (abajo); el intento **no** se ha lanzado | obligatorio antes de lanzar |
| LAUNCHING | registro previo al lanzamiento (write-ahead), con el `RunId` asignado; desde este punto el lanzamiento puede haber ocurrido | obligatorio, inmediatamente antes de lanzar |
| LAUNCHED | el adapter acredita el arranque ligado al `RunId` (PID + `CreationDateUtc`, `thread_id` o `sessionId`) | cuando se acredita; si el Principal cae antes, el sucesor ve LAUNCHING |
| LAUNCH_UNCERTAIN | cierre conservador de un intento en LAUNCHING o LAUNCHED cuya terminación o identidad no se puede acreditar | lo publica quien recupera (caso B) |
| RESULT_RECEIVED | resultado custodiado, o ausencia de salida registrada, con la terminación (§9.1), el `RuntimeEvidenceRef` (§20.3.2) y la auditoría de lecturas (§20.3.1); **sin aplicar** | obligatorio: la custodia precede a la ingestión |
| RESULT_INGESTED | resultado aplicado: disposiciones, linaje, fase y `next_action` | obligatorio |

CANCELLED_BEFORE_LAUNCH cierra un intento cuyo último estado durable es INVOCATION_PLANNED o BUDGET_RESERVED cuando la autorización se revoca o caduca. Nunca
libera presupuesto.

**Regla de lanzamiento.** Nadie lanza un intento sin un LAUNCHING durable en el remoto. Así, el último estado durable dice, por sí solo, si pudo haber un
lanzamiento.

**Recuperación por estado.** La aplica el mismo Principal o un sucesor (§20.8), sin artefactos transitorios:

| Caso | Último estado durable del intento | Recuperación |
|---|---|---|
| **A** | INVOCATION_PLANNED o BUDGET_RESERVED | no hubo lanzamiento: se reanuda **sin consumir otro lanzamiento**. Con BUDGET_RESERVED, el intento usa su reserva; antes de LAUNCHING puede replanificarse (`InvocationId` nuevo) dentro de la misma reserva |
| **B** | LAUNCHING o LAUNCHED | **no se relanza** hasta resolver la terminación y la identidad con la operación 7 del adapter ligada al `RunId`. (1) Se prueba que no arrancó → vuelve a BUDGET_RESERVED y se reanuda como en A. (2) Terminó y su salida es accesible → RESULT_RECEIVED. (3) Terminó sin salida accesible → RESULT_RECEIVED con salida ABSENT. (4) Sigue vivo → espera, u operación 6 dentro del tope. (5) No se puede acreditar → **LAUNCH_UNCERTAIN**: cuenta como lanzado, y cualquier intento nuevo es una reejecución de transporte. En un rol de solo lectura, ese cierre conservador permite el intento siguiente, y un resultado tardío del intento incierto se registra como evidencia sin ingerirse. En un rol con escritura rigen P-02, P-12 y P-13 sin cambio |
| **C** | RESULT_RECEIVED | **ingestión idempotente** desde el resultado custodiado, con la clave (`LogicalReviewRequestId`, intento, blob del resultado). Repetirla da el mismo estado; ingerir otro blob para el mismo intento es S-04 |
| **D** | RESULT_INGESTED | terminal: el intento **nunca** vuelve a contarse ni a lanzarse; una reingestión es un no-op |

**Presupuestos.** Son contadores durables en `orchestration.budgets` (B.8.8). Todos crecen en la **reserva**, de forma conservadora, y nunca decrecen:

| Constante | Valor congelado propuesto | Cuenta | Crece en |
|---|---|---|---|
| `MAX_REVIEW_ROUNDS` | 3 | versiones distintas del objeto sometidas a revisión (`review_rounds`) | BUDGET_RESERVED del primer intento de la primera solicitud sobre una versión |
| `MAX_ARCHITECT_LOGICAL_REQUESTS` | 3 | solicitudes lógicas (`logical_requests`), incluida una segunda solicitud sobre la misma versión | BUDGET_RESERVED del primer intento de cada solicitud |
| `MAX_TRANSPORT_RERUNS_PER_REQUEST` | 2 | intentos de una solicitud después del primero (`transport_reruns` por solicitud) | BUDGET_RESERVED de cada intento k ≥ 2 |
| `MAX_CORRECTIONS_PER_LINEAGE` | 2 | versiones publicadas que responden a un linaje (`corrections_by_lineage`) | PUBLISHED |
| tope derivado de correcciones | `MAX_REVIEW_ROUNDS` − 1 = **2** | versiones corregidas publicadas (`correction_rounds`); una corrección sin re-revisión posible no se publica | PUBLISHED |
| tope derivado de lanzamientos | `MAX_ARCHITECT_LOGICAL_REQUESTS` × (1 + `MAX_TRANSPORT_RERUNS_PER_REQUEST`) = **9** | intentos reservados (`architect_launches`) | BUDGET_RESERVED de cada intento |

- **Fórmula cerrada:** los dos topes derivados se calculan solo con constantes congeladas; nada depende de contadores en curso.
- **Constantes que coinciden:** con los valores propuestos, `review_rounds` ≤ `logical_requests` siempre. `MAX_REVIEW_ROUNDS` y `MAX_CORRECTIONS_PER_LINEAGE`
  se conservan porque la `ReviewLoopAuthorization` puede bajarlos por separado (p. ej., dos versiones con tres solicitudes). Ilustración, sin cambio de
  semántica: L1 y L2 sobre X, y después L3 sobre X2, dan `review_rounds` = 1 y `logical_requests` = 2, y después 2 y 3.
- **Comprobación antes de reservar**, nunca después de lanzar: un intento nuevo se rechaza si haría superar cualquier tope. Con
  `MAX_TRANSPORT_RERUNS_PER_REQUEST` = 2, el cuarto intento de una solicitud (la tercera reejecución) se rechaza **antes del lanzamiento** (P-18).
- **Qué cuenta:** todo intento reservado, se lance o no, falle, quede incierto o se cancele. Un intento ingerido no vuelve a contar.
- **Sin reinicios silenciosos:** cambiar de proveedor, modelo, sesión, binding de rol, Principal, redacción o etiqueta cosmética del hallazgo no reinicia la
  solicitud, el linaje ni los contadores.
- **Asignación de linaje:** un hallazgo nuevo se asocia a un linaje existente cuando apunta al mismo elemento y a la misma clase de defecto, o cuando el Architect
  lo declara en `SupersededBy`. Si la asociación es ambigua, cuenta como el mismo linaje (conservador).
- Los topes se congelan en el Freeze. La `ReviewLoopAuthorization` puede bajarlos, nunca subirlos sin A-n. Analogías en la autoridad vigente: `MaxReworkLoops` =
  3 y las dos reejecuciones BLOCKED de 16.8.
- **Agotamiento:** STOP y escalada (P-18).

### 20.7 El mismo orquestador para Controller, Worker y Reviewer; contratos de salida por rol (R62-AUTO-09, R62-AUTO-10; A62-V9-05)

El bucle de ejecución ya conocido (Principal → Controller de planificación → Worker → Controller de verificación → corrección o reejecución autorizada →
Principal) usa el mismo orquestador:
- **sin relevo del Owner** en las iteraciones normales autorizadas;
- se conservan todas las reglas de I-61 y de §§8-9 sobre reintentos, exact-SHA, STOP, autoridad y custodia.

Si la política de riesgo exige un REVIEWER independiente (§11.3), el Principal resuelve un binding elegible, lo invoca, ingiere el resultado y continúa, corrige
o escala. Usa el mismo contrato de invocación, los mismos estados de intento y el mismo modelo de presupuesto (§20.6). El binding del REVIEWER se acepta
individualmente (A7'), o se materializa bajo una autorización con los mismos campos de §20.5.1 que el Coordinator incluya en el contrato de gate para el rol
REVIEWER (B.7). No hay una vía de revisión manual especial.

**Correspondencia cerrada entre rol, acción y contrato de salida:**

| `RequestedRole` / `Action` | `OutputContract` | Vocabulario del resultado | Puede satisfacer | Nunca |
|---|---|---|---|---|
| ARCHITECT / REVIEW_DESIGN | `rackcad-architect-review-result/v1` | AGREED \| CHANGES REQUIRED \| BLOCKED — OWNER DECISION | ARCHITECT_SATISFIED sobre el blob exacto; cierre o sustitución de hallazgos del ARCHITECT (§20.5.2) | GATE PASS, `EXECUTION_*` |
| REVIEWER / REVIEW_CHANGE | `rackcad-reviewer-result/v1` | hallazgos y recomendaciones; `Disposition` operativa NO_FINDINGS \| FINDINGS | el requisito de revisión operativa de §11.3 que lo pidió; el cierre de sus propios hallazgos | AGREED ni ningún veredicto de LIFECYCLE, ARCHITECT_SATISFIED, cierre o rebaja de hallazgos del ARCHITECT |
| EXECUTION_CONTROLLER / PLAN | `rackcad-delegation/v2` | los de §16 para la planificación | la planificación que §16 define | GATE PASS, Candidato; nunca `controller-verification/v2` |
| EXECUTION_CONTROLLER / VERIFY | `rackcad-controller-verification/v2` | `EXECUTION_VERIFIED`, `EXECUTION_REWORK_REQUIRED`, `EXECUTION_BLOCKED` | la verificación que §16 define | GATE PASS, Candidato; nunca `delegation/v2` |
| WORKER / IMPLEMENT | `worker-handoff/v1` | los de §16 | — | verificación, GATE PASS |

PLAN y VERIFY **no** son intercambiables: una salida de PLAN con `controller-verification/v2`, o de VERIFY con `delegation/v2`, es INVALID (P-19). En una
unidad I61 el Controller sigue con sus contratos de I-61 sin cambio; `RoleInvocation` solo aplica a unidades I62_DELEGATED.

Los dos resultados de revisión comparten un sobre común (B.10.0). La semántica de autoridad la discrimina el triple `RequestedRole` + `Action` +
`OutputContract`, que debe coincidir con el binding aceptado y con la `RoleInvocation`. Un resultado cuyo esquema no es el `OutputContract` de su invocación
es INVALID (P-19). Usar un resultado de REVIEWER para satisfacer al ARCHITECT o para cerrar un hallazgo del ARCHITECT es un acto de autoridad que el
Principal no tiene: rechazo y STOP (P-20).

### 20.8 Portabilidad del bucle entre Principales (R62-AUTO-11)

Un Principal sucesor reconstruye un bucle activo **solo** desde el estado canónico y los artefactos custodiados (F.8). Ejemplo: A publica la Proposal X,
invoca al Architect B y B devuelve CHANGES REQUIRED; A desaparece antes de corregir. El sucesor C (por QR, T16 o T22) reconstruye:
- la versión exacta de X (`orchestration.loop.object`);
- la solicitud lógica, sus intentos y el estado de cada uno (`review_requests[]`), con el resultado custodiado (`attempts[].result`);
- los REQUIRED abiertos (`findings[]`) y el presupuesto consumido (`budgets`);
- el alcance de corrección y la autorización de materialización (`invocation_permission` → `ReviewLoopAuthorization`);
- `next_action` = PRINCIPAL / CORRECT_AND_REREVIEW.

Todo ello sin narrativa del Owner (C-29). Si A desaparece con un intento a medias, C aplica la recuperación por estado de §20.6 (casos A-D). C reconstruye
también, desde la custodia, la evidencia de fidelidad de cada intento (§20.3.3) y la acreditación histórica de los bindings y resultados (§20.5.1), aunque la
autorización haya dejado de estar vigente.

### 20.9 Respaldo manual y AUTONOMY_GAP (R62-AUTO-15)

El relevo manual solo se admite cuando el transporte automático no está disponible o no está autorizado de verdad. Cuando se usa, se persiste un
**`AUTONOMY_GAP`** en la evidencia, referenciado desde `orchestration.autonomy_gaps[]`:

```text
AUTONOMY_GAP { RequiredRole, RequiredAction, MissingCapabilityOrAuthority, AttemptedTransport, WhyAutomaticRelayUnavailable, ManualFallbackUsed }
```

El relevo manual permite avanzar bajo la autoridad vigente, pero **AUTONOMY_GAP ≠ PASS** para el criterio de orquestación autónoma (criterio 15). Los huecos
son evidencia para el trabajo de adapters y tooling.

### 20.10 Nivel A primero (R62-AUTO-17)

Nada de plataforma de agentes. La implementación inicial sigue en Level A, salvo que la evidencia real justifique Level B, y prefiere:
- contratos versionados y transiciones de estado pequeñas y deterministas;
- los adapters de CLI y runtime existentes;
- archivos estructurados;
- orquestación gestionada por la sesión del Principal.

**No** se construyen un servicio de scheduler, un daemon, un framework genérico de agentes ni un servicio de orquestación en la nube. Automatizar significa que
el Principal ejecuta el bucle documentado, no que RackCad necesite un servidor permanente.

### 20.11 Límite de autoalojamiento en I-62 y huecos observados (R62-AUTO-18)

I-62 sigue gobernada por I-61 hasta que se integre: estas reglas **no** están vigentes y no se usan como autoridad para desarrollar I-62. Durante su
desarrollo:
- se usa la autoridad vigente;
- cuando ya permite una invocación directa por CLI o sesión, se prefiere esa invocación;
- cuando no, se registra un AUTONOMY_GAP.

**Fricción real observada en I-62** (evidencia §§20-21):

| Id | Rol o acción | Qué faltó | Transporte usado |
|---|---|---|---|
| GAP-01 | ARCHITECT / REVIEW (V6, V7) | I-61 no tiene contrato de invocación del Architect ni de resultado de revisión (ADR-0046 solo cubre Controller y Worker). La sesión autora no puede abrir una sesión revisora limpia: `claude-cli` NOT_AUTHENTICATED (OD-3), y `codex-cli` como Architect no tiene invocación medida ni línea base de huella (OD-2) | el Owner abrió la revisión en otra sesión y trajo la orden del Coordinator |
| GAP-02 | resultado de revisión | el dictamen literal del Architect no llegó a la sesión autora; solo el resumen del Coordinator. No había artefacto estructurado | texto pegado |
| GAP-03 | COORDINATOR_DECISION (C62-F0-*, disposiciones) | el Coordinator es un chat externo, sin transporte invocable | el Owner pega cada orden |
| GAP-04 | continuidad | parte de los hechos de continuación estaban en la memoria de proyecto de la sesión autora y en la prosa de `next_action` | memoria de sesión |
| GAP-05 | independencia del revisor | Claude Code carga la memoria de proyecto de la carpeta; una sesión revisora abierta ahí no es limpia | aviso manual al Owner |
| GAP-06 | repetición de órdenes | el Owner volvió a pegar una orden ya ejecutada; sin estado de orquestación, la sesión tuvo que comprobar a mano que no había trabajo nuevo | comprobación manual |
| GAP-07 | ARCHITECT / REVIEW (V9, invocación acotada de `codex-cli` autorizada por el Owner) | la invocación dio `AGENTS.md` como insumo sin resolver sus lecturas iniciales obligatorias; el revisor leyó tres archivos fuera de la lista cerrada, y el Owner no acreditó la revisión como formal limpia | invocación directa desde la sesión autora; resultado estructurado |
| GAP-08 | identidad del revisor | el revisor no pudo acreditar desde dentro su modelo, su effort ni su hilo; solo los acredita el registro del runtime, que lee el invocador | medición del invocador |
| GAP-09 | lectura de los insumos por el revisor (V9 y V10, `codex-cli` con PowerShell) | el camino de lectura del runtime degradó los caracteres no ASCII (≤/≥ → «=», ≠ → «?», flechas → caracteres de control, acentos → U+FFFD); un hallazgo de V10 nació de una premisa que no existe en el texto canónico. El prompt sí llegó fiel | invocación directa desde la sesión autora; la degradación la midió después el invocador |

**Triaje para el backlog de I-61:** GAP-01, GAP-02 y GAP-05 son trabajo de adapters (Architect por `codex-cli` o `claude-cli`, ruta limpia, resultado
estructurado). GAP-04 y GAP-06 los resuelve la sección `orchestration`. GAP-03 queda fuera del alcance de I-62: el Coordinator sigue siendo autoridad no
vinculable, y su transporte es un límite declarado. GAP-07 y GAP-08 los corrigió V10 en el diseño de la invocación: cierre efectivo de insumos (§20.3.1) e
identidad observada por el invocador (§20.3.2). GAP-09 lo corrige V11 con la fidelidad de los insumos (§20.3.3).

La revisión de V9 fue la primera invocación medida de un Architect por `codex-cli`: la lanzó la sesión autora con una autorización acotada del Owner
(evidencia §21). El Owner no transportó ni el prompt ni el resultado, pero sí la orden y la disposición (GAP-03).

### 20.12 Relación con DIRECT_ONLY (R62-AUTO-20)

La orquestación autónoma solo aplica a las unidades I62_DELEGATED y a los contextos de revisión o piloto que la adoptan. Una unidad DIRECT_ONLY sigue siendo
directa y sin bloqueos: ni `orchestration`, ni `RoleInvocation`, ni presupuestos del bucle (C-28). Si adopta I62_DELEGATED más tarde, se aplican las reglas de
adopción de §8.7.

### 20.13 Flujo objetivo (requisito de autonomía)

V10 y V11 no rebajan el objetivo añadido en V9. El flujo objetivo sigue siendo:

```text
Principal → Architect → CHANGES REQUIRED → el Principal corrige → Architect nuevo (materializado) → … → AGREED → decisión del Owner o siguiente gate
```

Todo ello sin relevo del Owner ni del Coordinator mientras:
- la `ReviewLoopAuthorization` sigue vigente (§20.5.1, `Validity`);
- la corrección cabe en el alcance autorizado;
- queda presupuesto (§20.6);
- no aparece una decisión reservada (§20.1).

Lo que V9 dejaba sin ejecutar lo cierran:
- la materialización autorizada de bindings (A62-V9-01);
- los intentos con reserva y recuperación (A62-V9-02, A62-V9-04);
- las disposiciones explícitas (A62-V9-03);
- los contratos por rol (A62-V9-05);
- el orden del cambio de objeto (A62-V9-06).

V11 añade la fidelidad de los insumos: el bucle no acepta una revisión cuyo revisor no recibió fielmente el texto canónico (§20.3.3).

FX-06 (D.8) lo demuestra, o queda en UNVERIFIED o UNSUPPORTED con causa.

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

**Amplía #1 (OWN-L):** obligaciones de la sesión principal fuera de una delegación; fila de WORKFLOW §10; el **punto de entrada de compatibilidad** en una
sección nueva de WORKFLOW, que rige la evaluación de los contratos de toda unidad desde `I62_EFFECTIVE_SHA` (Anexo E.3); con OD-6 alternativa 1, el predicado
de LIFECYCLE. Conserva el carácter **opt-in** de la ejecución delegada (AUTOMATION_PLAN §16): solo la adopta una unidad con decisión registrada
I62_DELEGATED (§14.0). Añade la **orquestación autónoma de roles** (§20): RELAY automático, ESCALATION solo ante fronteras reales y autoridad sin
cambios, con la materialización autorizada de bindings del Architect, los contratos de salida por rol, el cierre efectivo de insumos con la exención acotada de
acciones iniciales incompatibles (sin equivalencia de evidencia), la fidelidad de los insumos y la identidad del runtime observada por el invocador. Es materia **OWNER-RESERVED**.

**Vigencia:**
- desde `I62_EFFECTIVE_SHA`;
- solo unidades I62_DELEGATED (§14.0), salvo el punto de entrada de WORKFLOW y §16.13, que rigen la lectura de autoridades de toda unidad;
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
| `rackcad-role-invocation/v1` | nuevo (§20.3, B.9) | sesión (Principal) | sesión | aceptante del rol invocado; Coordinator |
| `rackcad-input-closure/v1` | nuevo (§20.3.1, B.9) | sesión (Principal) | sesión | aceptante del rol invocado; auditoría de lecturas tras la terminación |
| `rackcad-input-fidelity/v1` | nuevo (§20.3.3, B.9) | sesión (Principal), con los hechos del adapter | sesión | preflight antes del lanzamiento; comparación por segmentos tras la terminación; validador de `orchestration` |
| `rackcad-architect-review-result/v1` | nuevo (§20.5, §20.7, B.10.1) | Architect invocado | sesión (Principal) | Principal (ingestión mecánica); Coordinator |
| `rackcad-reviewer-result/v1` | nuevo (§20.7, B.10.2) | Reviewer invocado | sesión (Principal) | Principal (ingestión mecánica) |

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
| `LogicalReviewRequestId` / `InvocationId` | `^L[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` / `^I[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` | los asigna la sesión (§20.6) | únicos por unidad; un `InvocationId` pertenece a una sola solicitud lógica y a un solo intento |
| `AuthorizationRef` | `{Path, Marker, AuthorizationId, Commit, Blob}` | la `ReviewLoopAuthorization` en `decisions/<unit>.md`, con el marcador `I62-REVIEW-LOOP-AUTHORIZATION: <AuthorizationId>`. `Commit` = un commit **ancestro** del punto que custodia el binding en el que la autorización ya figura, y `Blob` = el blob del archivo de decisiones en ese commit; sin ciclos: nunca el propio commit de custodia | una **acción nueva** (materializar, reservar, LAUNCHING) con la vigencia de acción terminada → rechazada. Un binding ya materializado conserva su acreditación histórica: se valida contra la autorización de `Commit`/`Blob`, no contra la vigente (§20.5.1) |
| `RequirementId` | `^[A-Z_]+\.[a-z0-9-]+$` | lista fija por perfil y acción en `routing.md` (blob en la `AuthorityRevision`) | único por preflight. Dos filas del mismo id con valores distintos → `Contradiction` → UNKNOWN + S-04 |
| `AdapterId` | `^[a-z0-9-]+$` | descriptor `adapters/<AdapterId>.md` en la `AuthorityRevision`. La ruta **se deriva del id por patrón fijo**, nunca del contenido | inexistente → P-14 |
| `AdapterFactsSchemaRef` | `{SchemaId, Path, Blob}` | `Path` = `schemas/adapters/<AdapterId>.facts.v<n>.schema.json`, derivado del id y de la versión declarada por el descriptor; `Blob` en la `AuthorityRevision`. **Sin URLs ni descargas** | blob distinto o ausente → P-14 |
| `ProtocolSet` | `rackcad-protocol/I61` \| `rackcad-protocol/I62` | versiones de protocolo, no marcas | — |
| `AuthorRef` | `{Kind (AI_SESSION \| HUMAN \| UNKNOWN), Actor (ActorRef) \| null, Session (SessionRef) \| null, HumanId \| null, Operator (HumanId \| "UNKNOWN") \| null, Commits[], Evidence}` | por cada commit del **conjunto acotado** de `ReviewSubject`: con trailer de IA → la sesión que lo escribió según la custodia (bindings del Principal o del Worker vigentes en ese commit) o, sin custodia, según la evidencia de la unidad, más `Operator` = el humano que dirigió materialmente esa sesión (la operó o le dio las órdenes); sin trailer de IA → el autor humano. `null` = no aplica según el `Kind` | un commit sin autor establecible → `Kind` = UNKNOWN → la dimensión queda UNKNOWN y no satisface (fail-closed); `Operator` = `"UNKNOWN"` → la comparación de Actor de un revisor humano queda UNKNOWN |
| `ReviewSubject` | `{Kind (DESIGN \| IMPLEMENTATION), Commit, Paths[], Blobs[], RangeBase, Authors[] (AuthorRef)}` | lo fija quien encarga la revisión antes de que empiece. **Conjunto acotado a la unidad y al objeto**, nunca la historia completa de sus rutas: **IMPLEMENTATION** (implementación o conformidad): `RangeBase` = `merge-base(<base o autoridad de main de la unidad>, Commit)`, y los commits son los de `RangeBase..Commit` restringidos a las rutas de cambio de la unidad; **DESIGN**: los commits que produjeron las versiones de la Proposal de la unidad o sus deltas aceptados (Freeze delta, A-n) que contribuyen a la versión exacta revisada, desde el reclamo; los registros de revisión no son autoría. Los commits anteriores a la unidad quedan fuera. Las sucesiones y los rebindings quedan dentro | inmutable por revisión; un blob distinto del revisado → revisión inválida; un autor UNKNOWN dentro del conjunto acotado → no satisface |
| `HumanReviewerRef` | `{HumanId, Source (OWNER_REGISTRY \| GIT_IDENTITY \| DECLARED), Assurance}` | `HumanId` = etiqueta estable registrada por el Owner o SHA-256 de una identidad verificada; **nunca** datos personales en claro | se compara con las identidades humanas de `ReviewSubject.Authors` |
| `ReviewInstanceRef` | `{ReviewId, Mode (SAME-SESSION ROLE \| SEPARATE SESSION \| EXTERNAL HUMAN), StartedUtc}` | la asigna quien encarga la revisión | única por revisión; es la «sesión» de una revisión humana |

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
| `Acceptance` | `{State (PENDING \| ACCEPTED \| REJECTED), Basis (INDIVIDUAL_DECISION \| AUTHORIZED_MATERIALIZATION), DecisionRef, AuthorizationRef, MaterializationCheck, Utc}`. Con PENDING: `Basis`, `DecisionRef`, `AuthorizationRef`, `MaterializationCheck` y `Utc` = `null` (no aplica). Con INDIVIDUAL_DECISION: `DecisionRef` = `{Path, Marker}`, la ruta del archivo de decisiones de la unidad y el marcador de la decisión del Coordinator (§8.6), sin blob para no crear ciclos; `AuthorizationRef` y `MaterializationCheck` = `null`. Con AUTHORIZED_MATERIALIZATION (§20.5.1; solo ARCHITECT, o REVIEWER bajo un contrato de gate que lo autorice): `DecisionRef` = `null`, `AuthorizationRef` (B.2) y `MaterializationCheck` = `{Criteria[] {CriterionId, Required, Observed, Result (SATISFIED \| NOT_SATISFIED \| UNKNOWN), Evidence}}` con **todos** en SATISFIED; nace ACCEPTED, sin estado PENDING previo. Para INDIVIDUAL_DECISION, transición única PENDING → ACCEPTED o REJECTED; ninguna otra |

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
| `relay-record/v2` | `TaskId`, `RunId`, `Attempt`, `Phase`, `Cession`, `Outcome`, `Disposition`, `Notes`, `RemoteFacts`, `Exit.OpenDelegations` (cuenta de delegaciones abiertas según B.8.2) | `Participant.Kind` → `{Role, BindingRef, Actor, Session, AdapterId, AdapterVersion, Transport (session-internal \| external-process), Observation[]}`; `ConfigToml*` → `Fingerprint`; `RebaseMap.G2CloseSha` → `MaterializationCloseSha` | `PrevRelaySha256` (diario encadenado; nullable = no aplica solo en el primero de la ventana); `WindowSeq`; `Exit.DelegationStatus` (B.8.2); `Phase` añade `TRANSFER` (T12a); `Host`; `TestArtifacts[].Skipped`; `Outcome.Kind` añade `LAUNCH_UNCERTAIN`; `RebaseMap.StateFields[]` y `RebaseMap.CiRuns[]` (B.8.7); `RoleInvocation {LogicalReviewRequestId, InvocationId, AttemptSeq}` y `ReadAudit {Coverage, ReadsOutsideClosure[]}` e `InputFidelity` (`StateRef` de `input-fidelity/v1`), nullables = no aplica fuera de una invocación de rol (§20.3, §20.6) |
| `controller-verification/v2` | todo | — | `Verifier {Role, BindingRef, Actor}`; `AuthorityResolution[]` (E.3, paso 6) |
| `gate-contract/v2` | todo | `EligibleCells` → `RoleRequirements[]` (único por `Role`; `{Role, Profile, Action, Mandatory[] (RequirementId), Independence[] (ReferenceRole + 4 dimensiones), EligibleCells[]}`) | `ProtocolSet`; `MaterializationCloseSha`; `SupersededCommits[] {Sha, WindowSeq}` y `SupersededBaseSha` (nullable = no aplica si la lista está vacía) (B.8.6); `RoleRequirements[].Materialization` (autorización de materialización con los campos de §20.5.1, solo para el rol REVIEWER; nullable = no aplica) |
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
| `protocol.basis.adoption_at` | G0 \| MID_INITIATIVE | 1 | G0: adopción en el bootstrap de la unidad (§8.6); MID_INITIATIVE: adopción posterior desde DIRECT_ONLY (§8.7) |
| `protocol.g0_acceptance.state` | PENDING \| ACCEPTED \| REJECTED | 1 | PENDING en BOOTSTRAP; **una sola transición** en un QU posterior (§8.6) |
| `protocol.g0_acceptance.decision` | `StateRef` \| `null` | 1 | entrada de `decisions/<unit>.md` con el marcador `I62-CLASSIFICATION`, custodiada en el mismo commit de la transición; `null` = no aplica solo con PENDING |
| `custody.record_version` | entero ≥ 1 | 1 | +1 exacto por punto durable (CAS) |
| `custody.point` | BOOTSTRAP \| Q0 \| Q7 \| QU \| QH \| QR | 1 | tipo del punto durable que es este commit (§8.1) |
| `custody.point_kind` | ORDINARY \| REBASE_RECONCILIATION \| `null` | 1 | solo con `point` = QU o QR; `null` = no aplica en los demás puntos. REBASE_RECONCILIATION: el QU obligatorio tras un rebase fuera de ventana del titular (§8.8) o el QR combinado de una toma con rebase (§8.9) |
| `custody.last_rebase` | objeto \| `null` | 0..1 | `{map: StateRef, main_before, main_after, branch_before, branch_after, record_version}` del último rebase reconciliado: fuera de ventana, en un QU o QR REBASE_RECONCILIATION; dentro de una ventana, en el Q7 o QR que la cierra. `record_version` = la del punto que lo reconcilió; `null` = no aplica: ningún rebase reconciliado todavía |
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
| `orchestration` | objeto (B.8.8) | 1 | estado durable del bucle de orquestación (§20.4); solo en unidades I62_DELEGATED |

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
| I-S15 | `point` = Q0 ⇒ `g0_acceptance.state` = ACCEPTED ∧ `principal.acceptance.state` = ACCEPTED (la condición de base obsoleta, que requiere historia, pasa a I-H01) |
| I-S16 | `g0_acceptance.decision` = `null` ⇔ `g0_acceptance.state` = PENDING; `principal.acceptance.decision` = `null` ⇔ `principal.acceptance.state` = PENDING; cada `decision` no nula contiene el marcador correspondiente y el `Claim-Id` |
| I-S17 | `point_kind` ≠ `null` ⇔ `point` ∈ {QU, QR}; `point_kind` = REBASE_RECONCILIATION ⇒ `last_rebase` ≠ `null`, `last_rebase.record_version` = `record_version` y `last_rebase.map` existe en el árbol con su blob |

**De pares** (dos puntos durables consecutivos p → n de la rama, o sus imágenes por `RebaseMap`):

| Id | Invariante |
|---|---|
| I-P01 | `n.record_version` = `p.record_version` + 1 |
| I-P02 | transición permitida: BOOTSTRAP → {Q0, QU, QH, QR}; Q0 → {Q7, QR}; Q7 → {Q0, QU, QH, QR}; QU → {Q0, QU, QH, QR}; QH → {QR}; QR → {Q0, QU, QH, QR}. Tras el force-push de un rebase fuera de ventana, p es la imagen del último punto anterior al rebase y n debe ser un QU REBASE_RECONCILIATION (titular, T19) o un QR REBASE_RECONCILIATION (toma, T22, desde QH, Q7, QU, QR, BOOTSTRAP o, en T12b, Q0) |
| I-P03 | p = Q0 ⇒ n cierra la ventana `p.window.seq` (`n.last_window.seq` = `p.window.seq`), y entre p y n solo hay commits del Worker o imágenes del rebase de 16.7 (W-2) — requiere historia |
| I-P04 | n = Q0 ⇒ `n.window.seq` = `p.window.seq` + 1 |
| I-P05 | `attempts` no decrece y solo sube en un Q0 con `kind` = CORRECTION (+1) o, fuera de la ejecución delegada, según §8/§9 en un QU; `correction_launches`, `chain_red_files` y los contadores no decrecen; un QU ORDINARY no cambia `window`, `last_window`, `chains` ni los contadores de ventana. **Excepción explícita:** un QU o QR con `point_kind` = REBASE_RECONCILIATION puede cambiar exactamente los campos SHA de `StateFields` (B.8.7) por sus imágenes, además de `point_kind`, `last_rebase`, `record_version` y `task_intent` según I-P10 |
| I-P06 | `protocol.set`, `protocol.effective_sha`, `protocol.basis` y los campos inmutables de `automation_state` no cambian |
| I-P07 | `principal.binding` cambia solo en QR (designación nueva), en el Q7 que cierra una ventana con TRANSFER (T12a), o en el QU de la transición de aceptación (mismo `BindingId`, versión con `Acceptance` decidida) |
| I-P08 | ningún commit del Worker modifica el archivo de estado (W-3) — requiere historia |
| I-P09 | `g0_acceptance.state` cambia como mucho una vez, de PENDING a ACCEPTED o REJECTED, y solo en un QU; `principal.acceptance.state` cambia como mucho una vez por titular (PENDING → ACCEPTED o REJECTED, en un QU); con un titular nuevo (QR, o Q7 tras T12a) entra ACCEPTED; la versión del binding cambia solo por esa transición o por el titular nuevo |
| I-P10 | n con `point_kind` = REBASE_RECONCILIATION (QU o QR) ⇒ cada campo de `StateFields` de n es la imagen, según `n.last_rebase.map`, del mismo campo de p, o igual si no se reescribió; ningún campo SHA desaparece ni pasa a `null`; `attempts`, `correction_launches`, contadores y `TaskId` son iguales; `task_intent` solo cambia a una reemisión (`kind` = REISSUE) o a `null`. Si n es un QR, además el titular cambia según I-P07 y, si p era Q0 (T12b), la ventana se cierra ABANDONED según B.8.5, con los `unverified_commits` como imágenes |
| I-P11 | `/v1` → `/v2` solo como BOOTSTRAP (`record_version` 1) de G0 o de adopción; `/v2` → `/v1` solo desde un BOOTSTRAP o QU con aceptaciones PENDING y `window.seq` = 0, en el commit que registra una decisión DIRECT_ONLY (T21); los nueve campos de `automation_state` se conservan en ambas direcciones |
| I-P12 | p = Q0 de la ventana k y n = el Q7 o QR que la cierra, con un rebase de 16.7 registrado en el diario de la ventana ⇒ `n.last_rebase.record_version` = `n.record_version` y `n.last_rebase.map` = el `RebaseMap` custodiado de la ventana; cada campo de `StateFields` que no es resultado propio de la ventana es en n la imagen de su valor en p; los resultados propios (`last_window.verified_sha`, el RED nuevo de la cadena) son SHAs de la rama rebasada; ningún campo SHA desaparece ni pasa a `null`; contadores y `TaskId` siguen el cierre normal |

**Con historia** (requieren objetos Git o ancestría; se comprueban en C-15, nunca en la guarda Core sin historia): I-P03 e I-P08, más:

| Id | Invariante |
|---|---|
| I-H01 | `point` = Q0 con `basis.claim_parent_contains_effective` = false ⇒ la rama contiene `effective_sha` |
| I-H02 | **fuera de una ventana activa** (último punto durable distinto de Q0): todo SHA de rama del estado (`chain_base_sha` y `chain_red_sha` de todas las cadenas, `verified_sha`, `unverified_commits[].sha`, `last_evidence_commit` cuando es de la rama) es ancestro de `HEAD`. Si alguno no lo es, el siguiente punto durable es el de reconciliación (QU del titular, T19, o QR de una toma, T22); antes no hay Q0 ni acción delegada, y en una toma tampoco trabajo ordinario. **Dentro de una ventana** (último punto Q0), los SHA del estado pueden quedar sin reconciliar tras un rebase de 16.7 hasta el Q7 o QR de cierre (I-P12) |

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

#### B.8.7 `RebaseMap` durable

Un rebase fuera de ventana lo custodia en `docs/automation/evidence/<unit>-agent/rebase/<RunId>/rebase-map.json`: el QU de reconciliación (§8.8) o el QR de
una toma (§8.9). Un rebase dentro de una ventana lo lleva en el `RebaseMap` del `relay-record/v2` del diario, con los mismos campos, custodiado por el Q7 o
QR de cierre (I-P12):

| Campo | Contenido |
|---|---|
| `RunId`, `TaskId` | el registro de 16.7; `TaskId` = SESSION sin cadena en curso |
| `MainBefore`, `MainAfter`, `BranchBefore`, `BranchAfter` | SHAs; `BranchBefore` es el SHA esperado del `--force-with-lease` |
| `Commits[]` | `{Original, Image, PatchId}` por cada commit reescrito de la rama; `PatchId` calculado en el rebase; en otra máquina se comprueba recalculándolo sobre la imagen |
| `StateFields[]` | `{Field, Original, Image}` por cada campo SHA del estado: `chain_base_sha` y `chain_red_sha` de **todas** las entradas de `chains`, sea cual sea su tarea o estado; `last_window.verified_sha`; `unverified_commits[].sha`; `last_evidence_commit` cuando es de la rama. Incluye los que no cambian (`Image` = `Original`) |
| `CiRuns[]` | `{OriginalSha, RunId}` de las corridas que se seguirán citando por el SHA original (p. ej., la del RED) |
| `Unmapped[]` | vacío por construcción: si no lo está, no hubo publicación (STOP de §8.8) |

Un campo SHA del estado sin entrada, una entrada duplicada o una `Image` que no está en la rama publicada hacen el mapa inválido: el QU no se publica y es
STOP (16.7).

#### B.8.8 Sección `orchestration` (R62-AUTO-04, R62-AUTO-16; A62-V9-02..06)

Solo en unidades I62_DELEGATED. Es de una unidad: no hay registro global.

| Campo | Tipo | Card. | Semántica |
|---|---|---|---|
| `orchestration.loop.type` | NONE \| ARCHITECT_REVIEW \| EXECUTION \| REVIEWER | 1 | bucle activo |
| `orchestration.loop.phase` | enum de §20.5 (revisión), o la fase de §8 (ejecución) | 1 | NONE si `type` = NONE |
| `orchestration.loop.object` | `{commit, path, blob}` \| `null` | 1 | objeto exacto actual; cambia solo en CORRECTING → PUBLISHED (I-P13); `null` = no aplica sin bucle |
| `orchestration.loop.authorization` | `StateRef` \| `null` | 1 | `ReviewLoopAuthorization` o contrato de gate vigente; `null` = no aplica sin bucle |
| `orchestration.loop.action_validity` | `{authorization_id, state (OPEN \| ENDED), ended_at, ended_reason (ARCHITECT_SATISFIED \| EXHAUSTED \| EXPIRED \| REVOKED \| SUPERSEDED), ended_by: StateRef}` \| `null` | 1 | vigencia de acción de la autorización actual (§20.5.1); `ended_at`, `ended_reason` y `ended_by` = `null` (no aplica) con OPEN; `null` = no aplica sin bucle |
| `orchestration.next_action` | objeto (§20.4) | 1 | `{role, action, target, unit, gate, task_id, required_inputs[], required_capabilities[], required_independence, invocation_permission, budget_remaining, expected_output, completion_condition, stop_conditions[], escalation_conditions[]}`; con `role` = OWNER o COORDINATOR, la decisión exacta requerida |
| `orchestration.review_requests[]` | objeto | 0..n | único por `logical_review_request_id`; append-only; como máximo una solicitud OPEN por bucle |
| `…review_requests.logical_review_request_id` | string (B.2) | 1 | — |
| `…review_requests.object` | `{commit, path, blob}` | 1 | = `loop.object` cuando la solicitud se abre; inmutable |
| `…review_requests.round` | entero ≥ 1 | 1 | número de la versión revisada en el bucle |
| `…review_requests.state` | OPEN \| INGESTED \| EXHAUSTED \| CANCELLED | 1 | INGESTED: un intento llegó a RESULT_INGESTED con resultado VALID |
| `…review_requests.attempts[]` | objeto | 1..n | `attempt_seq` consecutivo desde 1; como máximo un intento no terminal |
| `…attempts.attempt_seq` | entero ≥ 1 | 1 | — |
| `…attempts.invocation` | `StateRef` | 1 | `role-invocation/v1` custodiado, con su `EffectiveInputClosure`; inmutable desde LAUNCHING |
| `…attempts.binding` | `StateRef` | 1 | binding aceptado o materializado (B.5) |
| `…attempts.state` | INVOCATION_PLANNED \| BUDGET_RESERVED \| LAUNCHING \| LAUNCHED \| LAUNCH_UNCERTAIN \| RESULT_RECEIVED \| RESULT_INGESTED \| CANCELLED_BEFORE_LAUNCH | 1 | §20.6 |
| `…attempts.reserved_at` | entero \| `null` | 1 | `record_version` del QU en que el intento pasó a BUDGET_RESERVED por primera vez; `null` = no aplica: nunca reservado |
| `…attempts.run_id` | string \| `"UNKNOWN"` \| `null` | 1 | `null` = no aplica antes de LAUNCHING; `"UNKNOWN"` solo en LAUNCH_UNCERTAIN sin `RunId` correlacionable |
| `…attempts.launch_evidence` | `StateRef` \| `null` | 1 | `relay-record/v2` del arranque; `null` = no aplica antes de LAUNCHED |
| `…attempts.result` | `StateRef` \| `null` | 1 | resultado custodiado; `null` = no aplica antes de RESULT_RECEIVED o con salida ABSENT |
| `…attempts.output_state` | PRESENT \| ABSENT \| `null` | 1 | desde RESULT_RECEIVED; `null` = no aplica antes |
| `…attempts.runtime_evidence` | `StateRef` \| `null` | 1 | `RuntimeEvidenceRef` (§20.3.2), desde RESULT_RECEIVED; `null` = no aplica antes |
| `…attempts.read_audit` | `StateRef` \| `null` | 1 | auditoría de lecturas frente al cierre (§20.3.1), desde RESULT_RECEIVED; `null` = no aplica antes |
| `…attempts.input_fidelity` | `StateRef` | 1 | `InputFidelityEvidenceRef` (§20.3.3): el preflight desde la reserva y la comparación tras la corrida desde RESULT_RECEIVED |
| `…attempts.fidelity_status` | FAITHFUL \| FAITHFUL_NORMALIZED \| DEGRADED_BOUNDED \| DEGRADED_UNBOUNDED \| UNVERIFIED \| `null` | 1 | estado tras la corrida; `null` = no aplica antes de RESULT_RECEIVED |
| `…attempts.unaccredited[]` | string | 0..n | `FindingId` de hallazgos y disposiciones INVALID_PREMISE (§20.3.3); vacío salvo con DEGRADED_BOUNDED |
| `…attempts.reserved_utc`, `…attempts.launching_utc` | instante \| `null` | 1 | instantes de las acciones nuevas, declarados en su QU y contrastables con el push; `null` = no aplica antes |
| `…attempts.outcome` | VALID \| INVALID \| INVALID_REVIEW_CONTEXT \| INPUT_FIDELITY_INVALID \| CONTRADICTION \| NO_OUTPUT \| UNCERTAIN \| `null` | 1 | en RESULT_INGESTED o LAUNCH_UNCERTAIN; `null` = no aplica antes |
| `…attempts.ingested_at` | entero \| `null` | 1 | `record_version` del QU de ingestión; `null` = no aplica antes |
| `orchestration.findings[]` | `{lineage_id, finding_ids[], issuer (ARCHITECT \| COORDINATOR \| REVIEWER), opened_in: StateRef, severity (REQUIRED \| OPTIONAL), class, affected_section, state (OPEN \| CLOSED \| STILL_OPEN \| SUPERSEDED), response: StateRef \| null, last_disposition_in: StateRef \| null, omitted_in[]: StateRef, closed_by: StateRef \| null, downgraded_by: StateRef \| null}` | 0..n | linaje (§20.5.2). `response` es la respuesta del Principal, nunca un estado. `closed_by` y `downgraded_by` apuntan a un resultado de revisión o a una decisión con autoridad, nunca a un artefacto del Principal; nullables = no aplica mientras no ocurren |
| `orchestration.budgets` | `{review_rounds, logical_requests, architect_launches, transport_reruns[] {logical_review_request_id, count}, correction_rounds, corrections_by_lineage[] {lineage_id, count}, caps: StateRef}` | 1 | §20.6; enteros ≥ 0 que no decrecen; `caps` = las constantes congeladas o las rebajadas por la autorización |
| `orchestration.escalation` | `{state (NONE \| OWNER \| COORDINATOR), reason, required_decision}` | 1 | `reason` y `required_decision` = `null` (no aplica) con NONE |
| `orchestration.autonomy_gaps[]` | `StateRef` | 0..n | registros AUTONOMY_GAP (§20.9) |

**Invariantes** (se añaden a B.8.4):
- **I-S18** (sin historia):
  - todo linaje en CLOSED o SUPERSEDED tiene `closed_by` apuntando a un resultado con autoridad para él (§20.5.2): un `architect-review-result/v1` de un
    binding ARCHITECT del bucle, cuyo `EvaluatedObject` es el objeto de su solicitud, para un linaje con `issuer` ARCHITECT; una decisión del Coordinator para
    un linaje con `issuer` COORDINATOR. Nunca un `reviewer-result/v1` para un linaje ARCHITECT;
  - `severity` OPTIONAL con `downgraded_by` ≠ `null` ⇒ `downgraded_by` es del emisor (`opened_in`);
  - todo resultado ingerido de ARCHITECT tiene un `runtime_evidence` cuyo actor observado difiere del de cada autor IA del objeto (§11);
  - `loop.phase` = ARCHITECT_SATISFIED ⇒ el último intento ingerido tiene `outcome` VALID, su resultado es un `architect-review-result/v1` con AGREED sobre el
    blob de `loop.object` y ningún linaje REQUIRED queda en OPEN o STILL_OPEN;
  - `loop.phase` ∈ {REVIEW_PENDING, REREVIEW_PENDING} ⇒ la solicitud OPEN no tiene intento no terminal, o lo tiene en INVOCATION_PLANNED o BUDGET_RESERVED;
    `loop.phase` = ARCHITECT_INVOKED ⇒ su intento no terminal está en LAUNCHING, LAUNCHED o RESULT_RECEIVED;
  - como máximo una solicitud OPEN y, en ella, como máximo un intento no terminal; todo intento en LAUNCHING o posterior tiene `run_id` ≠ `null`;
  - `budgets`: `review_rounds` ≤ `logical_requests`; `architect_launches` = número de intentos con `reserved_at` ≠ `null`; `transport_reruns(r)` = intentos
    reservados de `r` − 1; ningún contador supera su tope;
  - todo binding con `Acceptance.Basis` = AUTHORIZED_MATERIALIZATION tiene un `AuthorizationRef` con `Commit` y `Blob` resolubles en un ancestro del punto
    y un `MaterializationCheck` con todos los criterios en SATISFIED contra esa autorización. Su acreditación **no** depende de la vigencia actual;
  - `loop.action_validity.state` = ENDED ⇒ ningún intento en INVOCATION_PLANNED ni en BUDGET_RESERVED;
  - todo intento en LAUNCHING o posterior tiene `input_fidelity` con un preflight FAITHFUL o FAITHFUL_NORMALIZED; todo intento ingerido con `outcome`
    VALID tiene `fidelity_status` ∈ {FAITHFUL, FAITHFUL_NORMALIZED, DEGRADED_BOUNDED}, y con DEGRADED_BOUNDED sus `unaccredited` no cambian ningún linaje;
  - `loop.phase` = ARCHITECT_SATISFIED ⇒ el resultado que lo produjo tiene `fidelity_status` FAITHFUL o FAITHFUL_NORMALIZED;
  - `escalation.state` = NONE ⇔ `next_action.role` ∉ {OWNER, COORDINATOR};
  - `NextAction` es derivable de forma única.
- **I-P13** (de pares p → n):
  - los contadores de `budgets` no decrecen y no se reinician por un cambio de proveedor, modelo, sesión, binding, Principal o etiqueta. Un linaje no
    desaparece; las solicitudes y sus intentos tampoco;
  - un intento avanza solo por las transiciones de §20.6:
    - INVOCATION_PLANNED → BUDGET_RESERVED o CANCELLED_BEFORE_LAUNCH;
    - BUDGET_RESERVED → LAUNCHING, CANCELLED_BEFORE_LAUNCH, o BUDGET_RESERVED (replanificado, con `invocation` nueva);
    - LAUNCHING → LAUNCHED, RESULT_RECEIVED, LAUNCH_UNCERTAIN, o BUDGET_RESERVED (caso B.1, con la prueba de no arranque custodiada);
    - LAUNCHED → RESULT_RECEIVED o LAUNCH_UNCERTAIN;
    - RESULT_RECEIVED → RESULT_INGESTED;
    - RESULT_INGESTED, LAUNCH_UNCERTAIN y CANCELLED_BEFORE_LAUNCH son terminales;
  - `architect_launches`, `logical_requests`, `review_rounds` y `transport_reruns` solo crecen en el par en que un intento pasa a BUDGET_RESERVED por primera
    vez (§20.6);
  - un linaje solo pasa a CLOSED o SUPERSEDED en el par en que un intento pasa a RESULT_INGESTED con una disposición con autoridad y premisas fieles
    (§20.3.3). Una omisión o una disposición INVALID_PREMISE no lo cambian;
  - un par que introduce una **acción nueva** (materializar un binding, la primera BUDGET_RESERVED de un intento, LAUNCHING) exige en n
    `action_validity.state` = OPEN y un instante de la acción no posterior a `Until`. Tras el fin de la vigencia solo son válidas las transiciones de
    intentos que ya estaban en LAUNCHING o después, y CANCELLED_BEFORE_LAUNCH. Para una misma autorización, `action_validity` pasa de OPEN a ENDED una sola
    vez; una autorización nueva (sustitución o enmienda) exige `ended_reason` = SUPERSEDED en la anterior y una decisión del Coordinator;
  - la validación de fidelidad se aplica a cada intento, sea cual sea su proveedor, runtime o adapter; cambiarlos no la omite;
  - **`loop.object` cambia solo en el par con `p.loop.phase` = CORRECTING y `n.loop.phase` = PUBLISHED**, y en ese par sube `correction_rounds`. En ningún
    otro par cambia `loop.object`, salvo NONE → REVIEW_PENDING al abrir el bucle; en particular, no cambia en REREVIEW_PENDING → ARCHITECT_INVOKED;
  - la ingestión es idempotente: dos puntos durables que ingieren el mismo intento lo hacen con el mismo blob de resultado; un blob distinto es S-04.

### B.9 `rackcad-role-invocation/v1` (R62-AUTO-03; A62-V9-04, GAP-07)

| Campo | Tipo y semántica |
|---|---|
| `Schema`, `InvocationId` | `InvocationId` (B.2) único por unidad: un contrato por intento físico, distinto del `RunId` del proceso o la sesión |
| `LogicalReviewRequestId`, `AttemptSeq` | la solicitud lógica estable y el número de intento (§20.6); nullables = no aplica fuera de un bucle de revisión |
| `UnitId`, `Gate`, `TaskId`, `ProtocolSet` | `TaskId` nullable = no aplica en una revisión de diseño |
| `RequestedRole`, `Action` | p. ej., ARCHITECT / REVIEW_DESIGN; REVIEWER / REVIEW_CHANGE; EXECUTION_CONTROLLER / PLAN \| VERIFY; WORKER / IMPLEMENT; PRINCIPAL / RESUME_DECISION. La pareja fija el `OutputContract` (§20.7) |
| `Target` | `{commit, path, blob}`; **obligatorio** en revisión y verificación; nullable = no aplica solo en la planificación |
| `AuthorityRevision` | SHA |
| `CanonicalInputs[]` | `{Path, Blob}` en la `AuthorityRevision` |
| `AllowedTransitiveInputs[]` | `{Path, Blob, RequiredBy {Path, Section, Blob}}` (§20.3.1) |
| `AllowedActions[]` | `{Action, RequiredBy, Class (ACTION_COMPATIBLE \| EXEMPTED), ExemptionRef, ExemptionScope}`; `ExemptionRef` y `ExemptionScope` nullables = no aplica salvo con EXEMPTED. Una acción EXEMPTED se **omite**: la exención es explícita y acotada a esa invocación y esa acción (§20.3.1) |
| `HealthSignals[]` | `{Kind (PUBLICATION_CI), RunRef, Sha}`: señales canónicas separadas; **no** son evidencia equivalente a ninguna clase de prueba local ni se propagan a gates, Candidato, cierre o implementación (§20.3.1) |
| `DeclaredRuntimeContext[]` | `{Kind, Source, SizeOrSha256}`, declarado por el adapter |
| `ForbiddenInputs[]` | clases y rutas prohibidas: transcripción y memoria del autor, sesiones ajenas y lo demás de D.6 |
| `EffectiveInputClosure` | `StateRef` del `rackcad-input-closure/v1`: las listas anteriores; las obligaciones encontradas, con su clase (READ, ACTION_COMPATIBLE, ACTION_INCOMPATIBLE, CONDITIONAL_NOT_TRIGGERED) y su resolución (INCLUDED, ALLOWED, EXEMPTED con autoridad, NOT_TRIGGERED con motivo); y los blobs de origen |
| `InputFidelityPreflight` | `StateRef` del `rackcad-input-fidelity/v1` del preflight (§20.3.3): sin estado FAITHFUL o FAITHFUL_NORMALIZED, la invocación no se lanza |
| `OpenFindings[]` | `{FindingId, LineageId, Severity, Issuer}`: los hallazgos abiertos que el revisor debe disponer (§20.5.2); vacío en la primera ronda |
| `RequiredCapabilities[]`, `IndependenceRequirements` | §5, §11; con la alternativa 1 de OD-6, el predicado de §11.4 y el `ReviewSubject` |
| `Permissions` | mínimas; READ_ONLY para ARCHITECT, EXECUTION_CONTROLLER y REVIEWER |
| `Binding` | `BindingRef` aceptado o materializado (B.5) |
| `OutputContract` | exactamente uno, según la correspondencia cerrada de §20.7: `rackcad-architect-review-result/v1` (ARCHITECT / REVIEW_DESIGN), `rackcad-reviewer-result/v1` (REVIEWER / REVIEW_CHANGE), `rackcad-delegation/v2` (EXECUTION_CONTROLLER / PLAN), `rackcad-controller-verification/v2` (EXECUTION_CONTROLLER / VERIFY), `rackcad-worker-handoff/v1` (WORKER / IMPLEMENT). Un valor que no corresponde a la pareja `RequestedRole` / `Action` invalida la invocación |
| `BudgetSnapshot` | copia de los contadores (§9.3, §20.6) en el momento de la reserva |
| `StopConditions[]`, `EscalationConditions[]` | por id (§13, §20.1) |
| `Authorization` | `StateRef` del contrato de gate o de la `ReviewLoopAuthorization` |

El adapter renderiza la invocación concreta, sin texto de prompt en el contrato. El renderizado declara que el revisor puede leer `CanonicalInputs` y
`AllowedTransitiveInputs` y ejecutar `AllowedActions`, y nada más. El registro de cada lanzamiento vive en un `relay-record/v2` con su `RunId` (B.7), y su
terminación es la de §9.1. La identidad observada del runtime se custodia ahí (§20.3.2).

### B.10 Resultados de revisión por rol (R62-AUTO-06; A62-V9-03, A62-V9-05, GAP-08)

La representación experimental `rackcad-review-result/v1` que usó la revisión de V9 no es un contrato: desde V10 la sustituyen dos contratos discriminados sobre
un sobre común.

#### B.10.0 Sobre común (definiciones compartidas; no es un esquema propio)

| Campo | Tipo y semántica |
|---|---|
| `Schema` | const del contrato concreto |
| `ResultId`, `LogicalReviewRequestId`, `InvocationId`, `AttemptSeq` | iguales a los de la `RoleInvocation` |
| `RequestedRole`, `Action` | iguales a los de la `RoleInvocation`; junto con `Schema`, discriminan la autoridad (§20.7) |
| `ReviewedUnit`, `ReviewedCommit`, `ReviewedPath`, `ReviewedBlob` | deben coincidir con `Target`; si no, INVALID |
| `ReviewerBinding` | `BindingRef` |
| `ReviewerDeclaredIdentity` | declaración informativa (§20.3.2): runtime, modelo, effort, sesión o hilo, o UNKNOWN; **sin** estado MATCH |
| `ReviewerMode` | SAME-SESSION ROLE \| SEPARATE SESSION \| EXTERNAL HUMAN |
| `InjectedContextDeclaration` | lista del contexto inyectado automáticamente (o «ninguno», declarado); obligatoria |
| `InputsRead[]` | lo que el revisor declara haber leído; el invocador lo contrasta con el cierre y con su auditoría (§20.3.1) |
| `InputFidelityEvidenceRef` | copia de la referencia que recibe en la `RoleInvocation` (§20.3.3); informativa: el revisor no crea ni certifica esa evidencia |
| `IndependenceEvidence` | dimensiones de §11 con su evidencia; `ReviewSubject` con la alternativa 1 |
| `KnownLimitations[]`, `RecommendedNextAction` | informativos; **no** son autoridad |

La identidad observada no forma parte del resultado: la aporta el invocador (`RuntimeEvidenceRef`, B.8.8).

#### B.10.1 `rackcad-architect-review-result/v1` (ARCHITECT / REVIEW_DESIGN)

| Campo | Tipo y semántica |
|---|---|
| sobre común | B.10.0, con `RequestedRole` = ARCHITECT y `Action` = REVIEW_DESIGN |
| `Verdict` | AGREED \| CHANGES REQUIRED \| BLOCKED — OWNER DECISION |
| `RequiredFindings[]`, `OptionalFindings[]` | hallazgos **nuevos**: `{FindingId, LineageId, Source, AffectedSection, Evidence, PremiseRefs[], WhyItMatters, CorrectionRequired}`. `PremiseRefs[]` = `{Path, Section, Quote}`, las premisas del hallazgo; `Quote` vacío = premisa de sección completa (§20.3.3). `FindingId` estable. `LineageId` = el del linaje que sustituye, o `null` = linaje nuevo o por asignar en la ingestión (§20.6). Un REQUIRED incompleto hace el resultado INVALID |
| `FindingDispositions[]` | §20.5.2: una por cada `OpenFindings` de la invocación y por cada hallazgo nuevo: `{FindingId, LineageId, State, SupersededBy[], Rationale, EvaluatedObject, PremiseRefs[]}` |
| `Downgrades[]` | `{FindingId, From (REQUIRED), To (OPTIONAL), Classification, Reason, Evidence}`; solo válidas si el revisor es el emisor (`opened_in`, LIFECYCLE §5) |
| `OwnerDecisions[]` | decisiones exactas requeridas (con BLOCKED — OWNER DECISION) |

Coherencia:
- AGREED ⇔ `RequiredFindings` vacío **y** ningún `OpenFindings` REQUIRED queda sin disposición CLOSED, o SUPERSEDED por hallazgos cerrados;
- CHANGES REQUIRED ⇒ algún REQUIRED abierto (nuevo, STILL_OPEN u omitido);
- BLOCKED — OWNER DECISION ⇒ `OwnerDecisions` no vacío;
- todo `FindingDispositions.EvaluatedObject` = el objeto revisado.

#### B.10.2 `rackcad-reviewer-result/v1` (REVIEWER / REVIEW_CHANGE)

| Campo | Tipo y semántica |
|---|---|
| sobre común | B.10.0, con `RequestedRole` = REVIEWER y `Action` = REVIEW_CHANGE |
| `Disposition` | NO_FINDINGS \| FINDINGS (operativa; **no** es un veredicto de LIFECYCLE) |
| `Findings[]` | `{FindingId, LineageId, Severity (BLOCKING \| ADVISORY), AffectedPath, Evidence, Recommendation}` |
| `FindingDispositions[]` | solo para linajes con `issuer` REVIEWER |
| `Recommendations[]` | informativas |

El esquema **no** admite `Verdict`, AGREED ni ARCHITECT_SATISFIED (`additionalProperties: false`). Su efecto lo fija la regla de §11.3 que pidió la revisión:
un hallazgo BLOCKING bloquea la operación dependiente.

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
| C-13 | independencia | (1) dos requisitos simultáneos; (2) mismo proveedor con contexto separado; (3) distinto proveedor con contexto compartido; (4) misma instancia de actor con dos `BindingId` en Worker y verificador; (5) PREFERRED frente a REQUIRED no satisfechos; con la alternativa 1 de OD-6: (6) EXTERNAL HUMAN ajeno, con `HumanReviewerRef` fuera del conjunto de autores e insumos canónicos, sin `ActorRef`; (7) versión producida por dos titulares sucesivos y revisor igual al primero, no al actual; (8) commit de autor no establecible dentro del conjunto acotado; (9) verificación de §16 con `SupersededCommits` y Controller igual al Worker de esos commits; (10) commit histórico anterior a la unidad hecho por la identidad del revisor; (11) el mismo humano que operó una sesión IA autora actúa como revisor EXTERNAL HUMAN | F3 | ídem | (1) máximo; (2) Contexto sí, Proveedor no; (3) Proveedor sí, Contexto no; (4) rechazo; (5) registro frente a bloqueo; (6) aceptado; (7) no satisfecho: el autor de la sucesión anterior está incluido; (8) UNKNOWN, no satisfecho; (9) rechazo; (10) excluido del conjunto, no descalifica; (11) rechazado | (ii) MC |
| C-14 | delta de fallos fiel | casos de cierre del Anexo G.2 sobre registros sintéticos con la regla de 16.9/16.8 | F3 | Anexo G + esquemas F3 | esperados exactos de G.2 | (ii) MC |
| C-15 | custodia, CAS y **semántica del estado** | repositorio Git **local desechable** con SHAs reales locales: **arranque BOOTSTRAP → G0 → QU** (§8.6: aceptación, rechazo, G0 sin marcadores), F.1, F.2, F.3, F.5 (R-0..R-3), T12a frente a T12b, T16/T17 (QH → QR), QU entre ventanas, push rechazado por causas distintas. **Rebase entre ventanas (F.6, A62-V6-01):** cadena IN_COURSE con el Q7 de la ventana anterior, `main` avanza, una sesión nueva rebasa, publica con `--force-with-lease=<rama>:<branch_before>`, publica el QU REBASE_RECONCILIATION y, en un **clon limpio en otra máquina** sin los objetos anteriores, emite la Q0 siguiente; RED, `chain_red_files` y commits sin verificar correctos con las imágenes; variantes: imagen no acreditable → STOP sin publicar; Q0 antes del QU → rechazado (I-H02). **Rebase dentro de una ventana:** 16.7 con `StateFields` que incluyen cadenas de otras tareas o anteriores, sin QU dentro de la ventana, reconciliación en el Q7 de cierre (I-P12) y reanudación en un clon limpio. **Toma con rebase (F.7, A62-V7-02):** QH + avance de `main` + designación REBASE_TAKEOVER + host limpio + QR combinado; y T12b con avance de `main`. **Adopción (§8.7):** DIRECT_ONLY `/v1` → BOOTSTRAP de adopción → decisión → QU → Q0; rechazo → `/v1` (T21). En **cada punto durable** se evalúan todas las invariantes de B.8.4 (de archivo, de pares y las que requieren historia: I-P03, I-P08, I-H01, I-H02), y en **cada paso** el `DS` de B.8.2 | F4 | state/v2, validador y procedimiento F4 | `DS`, invariantes y disposiciones = Anexo F, §8.6 y B.8; ninguna invariante violada en las secuencias válidas; cada mutación sembrada (Q0 omitido, Q0 con `g0_acceptance` PENDING, BOOTSTRAP que cita una decisión, aceptación inferida sin marcador, segunda transición de `g0_acceptance`, commit de la sesión dentro de la ventana, QU que toca `window`, `attempts` reiniciado, `record_version` saltado, ventana con dos Q0, QU de reconciliación que pierde un SHA, Q0 con SHAs sin reconciliar) detectada por la invariante que le corresponde | (ii) MC (sin invocar modelos) |
| C-16 | contadores | correcciones lanzadas frente a verificaciones; lanzamiento incierto; `ContinuesTaskId`; contador ausente o contradictorio; conteo conservador de B.8.5 | F4 | ídem | §9.3 y B.8.5; S-04 cuando corresponde | (ii) MC |
| C-17 | procesos | procesos locales ficticios (positivo: `pwsh` en la ruta; negativo: efímero) con el README §3.2 ampliado | F4 | procedimiento F4 | positivo detectado; efímero no vivo tras relectura | (ii) MC |
| C-18 | state/v2 **semánticamente válido** | guarda Core del validador: forma de B.8.1 + invariantes **de archivo** I-S01..I-S17 sobre archivos sintéticos (I-S13 contra el árbol de la prueba) + invariantes **de pares** I-P01, I-P02, I-P04..I-P07 e I-P09..I-P12 sobre pares sintéticos (I-P10 e I-P12 con el `RebaseMap` del árbol de la prueba), con **pares positivos simultáneos** de los tres tipos (QU ORDINARY que cumple I-P05; QU o QR REBASE_RECONCILIATION que cumple I-P05 por su excepción e I-P10; Q7 de cierre con rebase dentro de la ventana que cumple I-P12), todos GREEN a la vez; además, las invariantes de archivo sobre todo `docs/automation/state/*.yml` real del árbol con `schema: rackcad-automation-state/v2` (hoy ninguno: el conjunto vacío pasa; sin historia ni Level B); **nunca** las de la clase con historia (I-P03, I-P08, I-H01, I-H02); un caso positivo y uno negativo por invariante | F4 | formato y validador F4 | RED antes de materializar el validador; GREEN después; mutation por clase de invariante (omitir I-S03, omitir I-S15, invertir I-P02, aceptar `attempts` decreciente, permitir dos transiciones de `g0_acceptance`) detectada | (i) Core RG + mutation |
| C-19 | `/v1` intacto | guarda: blobs de los cinco `/v1` fijados; las pruebas de I-61 sin modificar | F2 | `/v1` | igualdad | (i) Core G |
| C-20a | punto de entrada, resolver y mapa bien formados | guarda Core **sin historia**: la sección de entrada de WORKFLOW existe una sola vez y nombra §16.13 por su línea exacta; §16.13 existe una sola vez; el mapa materializado valida contra su esquema; unicidad de `Files` y `Entries`; `Surfaces` = la lista cerrada de §16.13; ENTRY ⊆ la lista cerrada; `EffBlob` = hash del contenido del árbol; punteros presentes en §16 y §16.3; §16.3 = texto de I-61 (fijado como dato de la prueba con su blob de origen) + puntero; encabezados únicos por archivo en las superficies | F4 | punto de entrada, mapa, esquema y §16.13 materializados | RED antes de materializar; GREEN; mutation (sección de entrada borrada o duplicada, entrada del mapa duplicada, puntero borrado, ENTRY extra) detectada | (i) Core RG + mutation |
| C-20b | mapa = derivación | MC con historia: validación MV-3..MV-6 de E.4 entre `origin/main` (base) y la punta en F4, y comparación con el mapa previsto de E.5; **repetida sobre el merge local** (`I62_EFFECTIVE_SHA^1` → merge) antes del push de la integración | F4 + integración | mapa y textos materializados | igualdad; cada diferencia entre lo previsto y lo derivado, clasificada como corrección o como A-n | (ii) MC + revisión del Coordinator |
| C-20c | **descubrimiento** y resolución legacy **transparente** | MC: trazas de E.6 en un clon local desechable de RackCad con la historia real, el EFF local y M2 = EFF + X1 + X2 + X3. **Cada caso ejecuta `Evaluate` desde E1, nunca `Resolve` directamente**; el arnés obtiene §16.13 y el mapa del texto del punto de entrada. **C-20c-1:** el contrato `/v1` real de I-61 G3 **byte a byte**, antes de EFF y en la reverificación de 16.7 tras EFF. **C-20c-2:** el mismo contrato emitido para I-64 sin I-62 (`Path` y `Section` idénticos, documento completo incluido, sin citas nuevas). Negativos N-a..N-s (N-m..N-s, del descubrimiento) | F4 | punto de entrada, resolver, mapa y reglas F4 | registro de descubrimiento E1-E3 y tablas de E.6, cita por cita y unidad por unidad en las lecturas compuestas; ningún contrato reemitido; fallos con su id | (ii) MC |
| C-21 | MaterializationClose | cambio normativo posterior a MC_I62 → invalidación | F4 | regla F4 | MC_I62 nuevo requerido | (ii) revisión del Coordinator |
| C-22 | fixture arrancable | traza D.1 hasta un contrato I62 válido | F6 | fixture | D.1 | (ii) FX |
| C-23 | autoverificación (FX-01) | D.3 | F6 | fixture | D.3 | (ii) FX + OV |
| C-24 | topología A (FX-02) | D.3 | F6 | fixture + OD | D.3 | (ii) FX + OV |
| C-25a | portabilidad de reanudación y decisión (FX-04a) | D.3, D.6 | F6 | fixture + OD-5 | PASS por comparación mecánica con el oráculo; FAIL ante diferencia o lectura prohibida; UNVERIFIED si falta una precondición | (ii) FX + OV |
| C-25b | continuación (FX-04b) | D.3 | F6 | fixture + OD de la acción | PASS (VERIFIED sobre G' y Q7 conforme a B.8.4), FAIL, UNVERIFIED o UNSUPPORTED (D.4) | (ii) FX + OV |
| C-26 | topología B (FX-03) | D.3 | F6 | ídem | D.3 o limitación | (ii) FX + OV |
| C-27 | plano (c) sin efecto real (FX-05) | D.3 | F6 | ídem | rechazo; estado real intacto | (ii) FX |
| C-28 | la ejecución delegada sigue siendo opt-in | MC con los textos F4: unidad de producto nueva posterior a EFF, **DIRECT_ONLY**, sin participantes delegados y con la CUSTODY delegada del Principal en UNKNOWN: (a) trabajo directo autorizado (commit, push, CI, relevo de WORKFLOW §3); (b) intento de emitir un contrato Controller/Worker; (c) autoverificación del Principal ausente; (d) adopción T20 y después contrato; (e) ausencia de `orchestration`, `RoleInvocation` y presupuestos del bucle (§20.12) | F4 | aplicabilidad, clasificador y reglas F4 | (a) posible, sin STOP de I-62; (b) STOP P-15 (clasificador DIRECT_ONLY); (c) sin efecto en el trabajo directo; (d) contrato admitido solo tras el QU con aceptaciones; (e) ningún requisito de orquestación sobre la unidad DIRECT_ONLY | (ii) MC |
| C-29 | otro Principal reconstruye el bucle (R62-AUTO-11; A62-V9-02) | MC con artefactos sintéticos: A publica X, reserva y lanza el intento de B, B devuelve CHANGES REQUIRED, A lo custodia y lo ingiere, A termina; C, en un clon limpio y solo con el estado, deriva `NextAction`. Además, caídas en cada frontera de F.8: tras BUDGET_RESERVED; tras LAUNCHING, con no arranque probado y con terminación no acreditable; tras LAUNCHED con el resultado fuera de Git; tras RESULT_RECEIVED; tras RESULT_INGESTED. Cada una la recupera C en otro host sin artefactos transitorios | F4 | `orchestration`, contratos B.9-B.10 y procedimiento F4 | C reconstruye X exacto, el resultado, los REQUIRED abiertos, el presupuesto y el alcance; `next_action` = CORRECT_AND_REREVIEW. En las caídas: reanudación sin lanzamiento nuevo (A); sin relanzar antes de resolver y, sin acreditación, LAUNCH_UNCERTAIN contado (B); ingestión única e idempotente (C); sin recuento (D). C reconstruye también la evidencia de fidelidad y la acreditación histórica desde la custodia. Sin narrativa del Owner (F.8) | (ii) MC |
| C-30 | invocación del Architect independiente y limpia (R62-AUTO-13; A62-V9-05) | selección de binding y construcción de la `RoleInvocation` con: (a) binding de la misma sesión que el autor; (b) insumos con la transcripción del autor; (c) sin declaración de contexto inyectado; (d) binding válido con la alternativa 1 de OD-6 y con la 2; (e) `OutputContract` distinto de la correspondencia de §20.7 (REVIEWER con `architect-review-result/v1`, o ARCHITECT con `reviewer-result/v1`); (f) sin `EffectiveInputClosure`; (g) PLAN cuya salida es `controller-verification/v2`; (h) VERIFY cuya salida es `delegation/v2`; (i) PLAN con `delegation/v2` y VERIFY con `controller-verification/v2` | F3, F4 | contratos B.9, B.10 y reglas | (a)-(c), (e) y (f): rechazo (P-10, P-19); (d) aceptada según la política vigente; (g) y (h): INVALID (P-19); (i) aceptadas | (ii) MC |
| C-31 | CHANGES REQUIRED provoca corrección y re-revisión autónomas (R62-AUTO-05; A62-V9-06) | secuencia sintética de F.8: resultado con 2 REQUIRED → respuestas → versión nueva publicada en un commit propio → QU PUBLISHED con el `loop.object` nuevo → CI → QU CI_VERIFIED → solicitud lógica nueva con un binding materializado (C-40) → `RoleInvocation` nueva a un Architect distinto | F4 | ídem | transiciones CORRECTING, PUBLISHED, CI_VERIFIED, REREVIEW_PENDING y ARCHITECT_INVOKED sin `escalation`. **Caso positivo completo:** todas las invariantes de B.8.4 y B.8.8 se cumplen a la vez en cada punto; linaje conservado | (ii) MC |
| C-32 | el Owner no es el bus de mensajes (R62-AUTO-01) | auditoría del transporte de la secuencia de C-31 y de FX-06 | F4, F6 | ídem | ningún paso de RELAY pasa por el Owner (`OWNER_AS_MESSAGE_BUS` = false) | (ii) MC + FX |
| C-33 | OWNER-RESERVED provoca escalada (R62-AUTO-01) | resultado BLOCKED — OWNER DECISION; corrección que exige una decisión reservada | F4 | ídem | `escalation.state` = OWNER con la decisión exacta; ninguna corrección ni invocación | (ii) MC |
| C-34 | presupuesto agotado → STOP (R62-AUTO-07; A62-V9-02, A62-V9-04) | (a) cuarta versión a revisión; (b) cuarta solicitud lógica; (c) cuarto intento de una solicitud, con `InvocationId`, `RunId`, proveedor y binding nuevos en cada intento; (d) corrección de un linaje con su tope rebajado a 1 por la autorización; (e) CHANGES REQUIRED con `review_rounds` en el tope | F4 | ídem | STOP (P-18) y escalada **antes** de reservar o lanzar; en (e), sin corregir | (ii) MC |
| C-35 | una salida inválida del Architect no avanza (R62-AUTO-06; A62-V9-03) | resultados con blob distinto, sin declaración de contexto, con un REQUIRED sin corrección, con AGREED y REQUIRED a la vez, con AGREED y un hallazgo abierto omitido, con la disposición de un `FindingId` desconocido, con `EvaluatedObject` de otra versión, con el esquema de otro rol | F4 | ídem | INVALID; estado sin avance; reejecución dentro del tope (P-19) | (ii) MC |
| C-36 | cambiar de proveedor no reinicia el bucle (R62-AUTO-07; A62-V9-04) | segunda ronda con otro proveedor, otro modelo, otro Principal y etiquetas de hallazgo cambiadas para el mismo defecto; reejecuciones con `InvocationId` y `RunId` nuevos | F4 | ídem | contadores, solicitud y linaje continúan (I-P13); ninguna reejecución abre presupuesto | (ii) MC |
| C-37 | relevo manual → AUTONOMY_GAP, no PASS (R62-AUTO-15) | una ronda con transporte manual | F4, F6 | ídem | registro AUTONOMY_GAP; el criterio 15 no es PASS | (ii) MC + FX |
| C-38 | la autonomía no crea autoridad (R62-AUTO-14; A62-V9-03, A62-V9-05, A62-V9-06) | guarda Core del validador de `orchestration` (I-S18, I-P13) sobre archivos y pares sintéticos. **Positivo:** la secuencia completa de F.8, GREEN en cada par. **Negativos:** hallazgo cerrado sin un resultado con autoridad; cerrado por un `reviewer-result/v1`; rebaja por quien no es el emisor; omisión tratada como cierre; ARCHITECT_SATISFIED con un hallazgo abierto omitido; cierre por un resultado de otra versión; ARCHITECT_SATISFIED sin AGREED sobre el blob; escalada inconsistente; tope superado; contador reiniciado; transición de intento no permitida; **`loop.object` cambiado en REREVIEW_PENDING → ARCHITECT_INVOKED (cambio tardío)**; binding materializado sin `AuthorizationRef` o con un criterio no SATISFIED; ingestión doble con blobs distintos | F4 | formato y validador F4 | RED antes de materializar; GREEN; mutation por invariante detectada | (i) Core RG + mutation |
| C-39 | piloto de autonomía real (R62-AUTO-12; FX-06) | D.8 | F6 | fixture + OD | PASS solo con `OWNER_AS_MESSAGE_BUS` = false en los pasos 1-7; si no, UNVERIFIED o UNSUPPORTED con causa exacta | (ii) FX + OV |
| C-40 | materialización autorizada de bindings del Architect (A62-V9-01) | MC con la autorización de §20.5.1 y candidatos sintéticos: (a) Architect nuevo tras CHANGES REQUIRED, materializado bajo la autorización vigente; (b) candidatos no elegibles: capacidad UNKNOWN, effort fuera de `ModelEffortBounds`, misma sesión que un autor, celda fuera de `EligibleCells`, permisos de escritura, objeto fuera de `ObjectFamily`, autorización revocada o caducada; (c) ningún candidato satisface; (d) binding materializado con un `DecisionRef` fabricado o sin `AuthorizationRef`; (e) la autorización caduca después de AGREED; (f) revocación antes de un lanzamiento nuevo, con un intento en BUDGET_RESERVED; (g) un sucesor reconstruye una revisión antigua tras la caducidad; (h) reutilizar una autorización caducada para un binding nuevo; (i) un intento en LAUNCHED cuando se revoca la autorización (A62-V10-03) | F3, F4 | contratos B.5 y B.9, autorización y procedimiento F4 | (a) binding ACCEPTED con `Basis` AUTHORIZED_MATERIALIZATION, custodiado en el QU de la reserva e invocado **sin relevo humano ni decisión nueva**; (b) rechazo, sin invocación; (c) STOP y COORDINATOR_DECISION u OWNER según §20.1; (d) inválido (P-20); (e) el resultado y ARCHITECT_SATISFIED siguen acreditados; (f) LAUNCHING rechazado y el intento pasa a CANCELLED_BEFORE_LAUNCH; (g) la revisión antigua sigue acreditada históricamente; (h) rechazo; (i) el intento se completa y su resultado se ingiere | (ii) MC |
| C-41 | cierre de insumos e identidad del runtime (GAP-07, GAP-08) | MC: (a) cálculo del cierre sobre el `AGENTS.md` real de la `AuthorityRevision`: las tres lecturas iniciales y el Context Pack de la unidad entran como transitivas (opción A), `git log` queda permitido, `dotnet test` sin exención impide el lanzamiento y, con exención, se omite y la CI de publicación figura solo en `HealthSignals`; (b) registro de lecturas con un archivo fuera del cierre; (c) registro sin lecturas; (d) resultado con una identidad declarada distinta de la observada; (e) identidad declarada UNKNOWN; (f) cambio del blob de `AGENTS.md` después del cálculo | F4 | contratos B.9-B.10 y procedimiento F4 | (a) cierre = el esperado del Freeze; STOP sin exención; con exención, ninguna clase de evidencia local, de gate, de Candidato ni de cierre se da por satisfecha; (b) INVALID_REVIEW_CONTEXT (P-22), intento contado; (c) Contexto UNKNOWN; (d) S-04 (P-23), sin ingestión; (e) sin contradicción, identidad tomada del invocador; (f) cierre recalculado antes de lanzar | (ii) MC |
| C-42 | fidelidad de los insumos (R62-FIDELITY-01..06) | MC sobre insumos reales del corpus y capturas sintéticas: (1) ≤ canónico; (2) ≠ canónico; (3) texto acentuado; (4) transporte con pérdida deliberada (consola sin UTF-8); (5) un hallazgo cuya única premisa es texto degradado intenta abrir, cerrar o sustituir un linaje; (6) un hallazgo no afectado, con `PremiseRefs` cuyas citas coinciden con el texto canónico; (7) un sucesor reconstruye la evidencia de fidelidad desde la custodia; (8) cambio de proveedor o de runtime entre intentos | F4, F6 | contratos B.9-B.10, `input-fidelity/v1` y procedimiento F4 | (1)-(3) FAITHFUL: el carácter llega igual; (4) preflight no acreditado y sin lanzamiento (P-24); (5) INVALID_PREMISE, el linaje no cambia (P-25); (6) se ingiere solo con la independencia demostrada mecánicamente; si no, no; (7) mismo `FidelityStatus` y mismos `unaccredited`; (8) la validación se exige igual en el intento nuevo | (ii) MC + FX |

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
   - el Coordinator del fixture decide G0 con los tres marcadores (`I62-DELEGATED-EXECUTION: I62_DELEGATED`);
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
1. preflight de B para CUSTODY (`repo-write`); designación (T16, o T22 si el `main` del fixture avanzó); **QR** por CAS;
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
| **FX-06** (autonomía real) | OD-5, OD-7 y OD-2 u OD-3; un binding de Architect con invocación medida | PASS solo con `OWNER_AS_MESSAGE_BUS` = false; si no, UNVERIFIED o UNSUPPORTED con causa para la decisión del Owner (OV-I62-06); el escenario no se retira. FAIL impide cerrar F6 |

**Límites separados por topología, rol y escenario:**
- una limitación de `claude-cli` afecta al Reviewer y al Architect de B;
- una limitación de escritura de Codex afecta a FX-04b y a FX-03; no a FX-04a;
- ninguna limitación de B demuestra que el Principal B sea incapaz de reconstruir ni acredita la portabilidad: eso solo lo decide FX-04a.

### D.5 Ejecución compacta para OV sobre FINAL_CANDIDATE_SHA

Se resiembra el fixture desde el Candidato y se ejecutan:
- FX-01;
- FX-02, pasos hasta VERIFIED: Codex planificación 1 + verificación 1 + nc1 1 + pool 2 = 5; Worker 1 (+1); Principal 1;
- **FX-04a**: Principal B 1 sesión; 0 invocaciones (OV-I62-05 a);
- **FX-04b**, solo con sus OD concedidas: Codex 2 + pool 1; Worker 1 (OV-I62-05 b);
- **FX-06**, solo con sus OD concedidas: Architect 2 + reejecuciones 2 (OV-I62-06; D.8).

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

### D.8 FX-06 — piloto de autonomía real (R62-AUTO-12; C-39)

**Escenario mínimo**, en el fixture, con una unidad de prueba I62_DELEGATED y una `ReviewLoopAuthorization` del Coordinator del fixture con su autorización de
materialización (§20.5.1):
1. El Principal A publica un objeto revisable X: una Proposal de prueba con un defecto sembrado que conoce el oráculo del Coordinator.
2. A materializa un binding de Architect B bajo esa autorización, calcula el cierre de insumos (§20.3.1), acredita la fidelidad del transporte (§20.3.3),
   reserva el intento y lanza **automáticamente** la `RoleInvocation`: solo lectura, contexto inyectado declarado.
3. B devuelve un `rackcad-architect-review-result/v1` con CHANGES REQUIRED. A lo custodia (RESULT_RECEIVED) con la identidad observada del runtime y la
   auditoría de lecturas y de fidelidad.
4. A lo ingiere automáticamente: `orchestration` pasa a CORRECTING, con linaje, disposiciones y respuesta.
5. A publica X2 corregido (QU PUBLISHED con `loop.object` = X2), con su CI (CI_VERIFIED) y su evidencia.
6. A materializa **sin decisión intermedia del Coordinator** un Architect C distinto de B, abre la solicitud siguiente y lo invoca automáticamente.
7. C devuelve AGREED, con la disposición CLOSED de cada hallazgo de B.
8. El estado canónico deriva la decisión del Owner pendiente, si la hay, o el siguiente gate.

| Resultado | Cuándo |
|---|---|
| **PASS** | pasos 1-8 completos y **`OWNER_AS_MESSAGE_BUS` = false** en 1-7 (C-32): auditoría del transporte sin ningún relevo por el Owner ni decisión intermedia del Coordinator. El Owner puede autorizar antes la apertura y el consumo (OD-5), pero no actúa como transporte en tiempo de ejecución |
| **FAIL** | violación observada: el Principal cierra un hallazgo, se salta la independencia, materializa un binding fuera de los criterios, avanza con un resultado inválido, no detecta una lectura fuera del cierre, ingiere un resultado con premisas degradadas o reinicia un contador |
| **UNVERIFIED** | falta una precondición: una OD, un binding de Architect sin invocación medida, o B no produce CHANGES REQUIRED y la ruta de corrección no se ejerce |
| **UNSUPPORTED** | capacidad medida ausente: ningún runtime disponible puede actuar de Architect independiente invocado por A |

**No se finge el bucle:** cualquier relevo manual queda como AUTONOMY_GAP, y el resultado no es PASS (C-37).

**Bindings previstos** (a medir en F2):
- Architect B y C con `codex-cli` en solo lectura, en invocaciones distintas, cada una con su propio `thread_id`, o con `claude-cli` si OD-3 lo autentica.
  Proveedor distinto del de A: PREFERRED;
- Principal A con `claude-desktop-session`, que lanza `codex-cli` por su adapter.

| Rol | Lanzamientos | Reejecuciones de transporte | Tope |
|---|---|---|---|
| Principal A | 1 sesión | +1 reapertura | 2 |
| Architect B (solicitud 1) | 1 | +1 (`transport_reruns` de su solicitud) | 2 |
| Architect C (solicitud 2) | 1 | +1 | 2 |
| **Total de invocaciones de Architect** | 2 | 2 | **4** (≤ el tope derivado de lanzamientos, §20.6) |

Dependencias: OD-5 (ensayo y sesiones), OD-2 (huella de `codex-cli`) u OD-3 (`claude-cli`) y OD-7 (la CI del paso 5). Sin ellas, UNVERIFIED con causa.

**Compacto para OV** (OV-I62-06): la misma secuencia sobre el fixture resembrado desde el Candidato.

## Anexo E — Resolución de autoridades para unidades I61 (ejecutable)

La política de compatibilidad por superficie y cláusula se conserva. Este anexo la convierte en un algoritmo normativo que no edita unidades I61, no muta los esquemas `/v1` y
no congela archivos enteros.

### E.1 Componentes, ubicación e identidad

| Componente | Ubicación tras la integración | Se lee en | Identidad |
|---|---|---|---|
| **Punto de entrada** | WORKFLOW, sección `##` nueva **«Coexistencia de protocolos de ejecución delegada (I61/I62)»** (§12 en esta Proposal; el número se fija al materializar) | `MainSha_eval`, por **toda** unidad, como gobierno de proceso; no depende de las `Authorities` de ningún contrato (E.3, eslabón 1) | encabezado único en WORKFLOW en `MainSha_eval`; su texto nombra §16.13 por su línea de encabezado exacta; con `I62_EFFECTIVE_SHA` presente y la sección ausente, repetida o ambigua → fallo cerrado (ENTRY_INVALID) |
| Resolver | AUTOMATION_PLAN, subsección nueva **`### 16.13 Compatibilidad de protocolos de ejecución delegada`**, al final de §16 | `MainSha_eval`, por **toda** unidad (independiente del protocolo) | su texto en `MainSha_eval`; con `I62_EFFECTIVE_SHA` presente y §16.13 ausente o repetida → fallo cerrado (ENTRY_INVALID) |
| Punteros | primera frase de §16 («Antes de aplicar esta sección, toda unidad aplica §16.13») y última frase de §16.3 | `MainSha` | vía **redundante** para los contratos que leen §16 en `MainSha`; la cadena no depende de ellos; guarda C-20a |
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
      g0_acceptance.state = ACCEPTED y decision válida (blob presente; marcadores
        `I62-CLASSIFICATION: I62` e `I62-DELEGATED-EXECUTION: I62_DELEGATED`, y cid) → I62 (POSTERIOR_DEMOSTRADA; base obsoleta
                                                                                si basis.claim_parent_contains_effective = false)
      g0_acceptance.state = REJECTED, o decision inválida                    → paso 6
 5b. si no, estado de u en /v1 y cid fuera de PRE:
      decisión de G0 con `I62-DELEGATED-EXECUTION: DIRECT_ONLY`               → DIRECT_ONLY (sin protocolo delegado)
      sin ese marcador                                                       → UNKNOWN (solo para la ejecución delegada)
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
| (estado `/v1`, DIRECT_ONLY) | DIRECT_ONLY | STOP (P-15) hasta una adopción (T20); el trabajo directo no se ve afectado | — (sin puntos `/v2`) |

- **Quién:** el Coordinator, en el G0 de cada unidad nueva (acepta o rechaza la evidencia del BOOTSTRAP) y antes de emitir cualquier contrato posterior a EFF.
  El Coordinator al aceptar y el Controller en `Authority` la vuelven a aplicar sobre las mismas fuentes; una discrepancia es S-04.
- **Durabilidad:** nunca se rederiva por ascendencia actual (WORKFLOW §11.1). Las fuentes son la tabla PRE, el `protocol` del estado `/v2` (evidencia del
  BOOTSTRAP más la aceptación registrada) o una decisión registrada.
- **Unidades I61:** se clasifican sin escribir en su rama.

### E.3 Resolver (transparente para los contratos I61)

#### E.3.0 Punto de entrada: la cadena normativa que hace invocar el resolver

**Hecho de partida** (MEASURED): `/v1` no contiene ninguna regla que lleve a quien evalúa un contrato a un texto leído en `MainSha` con independencia de las
`Authorities` de ese contrato. El contrato real de I-61 G3 clasifica el preámbulo de AUTOMATION_PLAN, §2 y §16 como `UNIT_CHANGE`, así que lee 16.3 en su
`AuthorityRevision`: el puntero de §16 en `main` no lo alcanza. Por eso el punto de entrada es un **delta normativo de I-62 bajo OD-1**, no algo que `/v1` ya
aporte.

**Dónde se coloca y por qué no es circular.** Se pone en WORKFLOW, como sección `##` nueva, porque dos reglas ya integradas garantizan que se lee en `main`
actual y fuera de cualquier contrato:
- **WORKFLOW §10**, con texto idéntico en el `AuthorityRevision` de G3 (`7b8662c5`) y en `main` (`819955d6`). La fila «Git, reclamo, worktrees, integración,
  cierre, cadencia documental y transición» atribuye la transición a WORKFLOW. La fila «Operación del ejecutor y ejecución delegada» deja AUTOMATION_PLAN
  «dentro de las reglas de los dueños anteriores».
- **El WORKFLOW que rige toda sesión es el de `main` actual:**
  - §11.3: «Tras activación, una unidad V1 lee `main` actual»;
  - V2 está activo desde el merge `8a021fb6`;
  - §4: rebase sobre el trunk al abrir la sesión.

  En toda evaluación válida, `main` actual = `MainSha_eval`: A6 lo exige al aceptar, `Remote` al verificar y 16.7 en la reverificación.

**Cadena exacta:**

| Eslabón | Fuente | Se lee en | Por qué no depende de las `Authorities` del contrato |
|---|---|---|---|
| 1. Regla vigente | WORKFLOW §10 y gobierno de WORKFLOW en `main` actual (arriba) | `MainSha_eval` | es gobierno de proceso de la sesión y del Coordinator, que ningún contrato necesita citar; su contenido necesario es igual en el `AuthorityRevision` de G3 y en `main` |
| 2. Punto de entrada | WORKFLOW, sección «Coexistencia de protocolos de ejecución delegada (I61/I62)» (delta I-62, OD-1; texto abajo) | `MainSha_eval` | lo impone el eslabón 1; es regla de transición del dueño anterior y prevalece sobre la regla de lectura de la versión de 16.3 que gobierne el contrato |
| 3. `Classify` | §16.13 → E.2 | algoritmo en `MainSha_eval`; PRE en el cuerpo de EFF; estado de la unidad en `BaseSha` | lo nombra el eslabón 2 por la línea de encabezado exacta de §16.13 |
| 4. Mapa | §16.13 → E.4 | EFF (ruta fija) | lo nombra §16.13 |
| 5. `Authority` `/v1` | 16.3 de la revisión que gobierne el contrato (para G3, la de AR), leyendo «se lee en MainSha» como «se lee en la revisión resuelta» | Resolve, paso 3 | evaluación normal con el contrato intacto |

La misma cadena vale para una unidad de producto I61 cuyo §16 es `EXTERNAL`; para ella, los punteros de §16 son una segunda vía redundante. Ningún eslabón
exige que el autor del contrato conozca I-62.

**Texto normativo propuesto del punto de entrada** (se materializa en F4 en el plano b, inactivo hasta EFF):

> **Coexistencia de protocolos de ejecución delegada (I61/I62).** Desde `I62_EFFECTIVE_SHA` (AUTOMATION_PLAN `### 16.13 Compatibilidad de protocolos de
> ejecución delegada`), toda evaluación de un contrato de ejecución delegada aplica primero esa subsección, leída en el `MainSha` de evaluación, y después la
> lectura de autoridades de 16.3 con las revisiones que resulten. Son evaluaciones la emisión, la aceptación A1-A8, la comprobación `Authority` de la
> verificación y la decisión del Coordinator sobre un `EXECUTION_VERIFIED`. Es una regla de transición (§10) y prevalece sobre la regla de lectura de la
> versión de 16.3 que gobierne el contrato. El contrato no cambia. En cada invocación de verificación, la sesión responsable pasa al Controller la entrada de
> compatibilidad. Sin un punto de entrada válido, la evaluación se detiene.

**Procedimiento del evaluador** (C-20c lo ejecuta desde E1; `Resolve` nunca se invoca directamente):

```text
Evaluate(K, MainSha_eval):
 E1. W := docs/WORKFLOW.md en MainSha_eval                     -- eslabón 1: gobierno de proceso
 E2. EFF := Derive(MainSha_eval); X := Match(MainSha_eval, WORKFLOW, <línea de encabezado del punto de entrada>)
     (misma semántica que E.3.1: línea normalizada, nivel ##, fuera de bloques de código)
       EFF ausente y X = ∅           → PRE_ACTIVATION: 16.3 de la revisión que gobierne K, sin cambios (fin)
       EFF ausente y X ≠ ∅           → ACTIVATION_INVALID → STOP al Owner
       EFF duplicado o ambiguo        → ACTIVATION_INVALID → STOP al Owner
       EFF presente y |X| ≠ 1         → ENTRY_INVALID → STOP (S-12; P-15)
 E3. R := Match(MainSha_eval, AUTOMATION_PLAN, <línea de §16.13 que nombra X>), con la misma semántica
       |R| ≠ 1                        → ENTRY_INVALID → STOP (S-12; P-15)
 E4. Resolve(K, MainSha_eval), pasos 1-5, con el algoritmo de R
 E5. Registro de descubrimiento: blobs de WORKFLOW y de AUTOMATION_PLAN en MainSha_eval, EFF, encabezados hallados; se une al registro de Resolve
```

**Fallo cerrado del punto de entrada:**

| Estado en `MainSha_eval` | Resultado |
|---|---|
| sin EFF y sin punto de entrada | PRE_ACTIVATION: I-61 sin cambios |
| punto de entrada sin EFF derivable, o trailer duplicado | ACTIVATION_INVALID → STOP al Owner |
| EFF presente y punto de entrada ausente, repetido o con encabezado ambiguo | ENTRY_INVALID → STOP (S-12; P-15) |
| el punto de entrada nombra una §16.13 ausente o repetida | ENTRY_INVALID → STOP |
| mapa inválido | MAP_INVALID → STOP (E.4) |
| verificación de una unidad I61 tras EFF sin registro de descubrimiento y de resolución en `Authority.Evidence`, o con una resolución distinta de la del Coordinator | el Coordinator no la acepta (16.1); reverificación (BLOCKED, fase VERIFICATION) |

**Obligaciones por actor:**
- **Coordinator:** ejecuta `Evaluate` al aceptar (A1-A8) y antes de aceptar un VERIFIED.
- **Sesión responsable:** registra el descubrimiento en `Notes` del relevo. En cada invocación de verificación de una unidad I61 tras EFF, añade a las entradas
  canónicas del prompt la **entrada de compatibilidad**: la instrucción normativa de ejecutar `Evaluate` antes de 16.3 y las líneas de encabezado del
  punto de entrada y de §16.13. **No** le pasa el resultado esperado ni su propio registro, y no la añade al contrato ni a la delegación.
- **Controller:** ejecuta `Evaluate` de forma independiente (puede, es de solo lectura) y registra en `Authority.Evidence` el descubrimiento y la resolución.
- **Comparación posterior:** terminada la verificación, el Coordinator compara el registro del Controller con el suyo. Una diferencia hace que el VERIFIED no
  se acepte (16.1), según la tabla de fallo cerrado.

Es el **único cambio operativo** para las unidades I61, y lo ejecutan los evaluadores, no el autor del contrato.

#### E.3.1 Resolución

**Entradas:**
- el contrato K (`gate-contract/v1` o `/v2`), **tal como se emitió**;
- `AR` = `K.AuthorityRevision`;
- `MainSha_eval` = `K.MainSha`, salvo en una reverificación de 16.7 sin conflictos. Ahí es el `MainSha` nuevo del `RebaseMap` (16.7: «las comprobaciones usan
  las imágenes y el MainSha nuevo»), y el contrato no cambia.

El contrato **no** necesita citar §16.13 ni el mapa, ni campos nuevos, ni reemitirse: el resolver lo aplica quien lee el contrato.

```text
Resolve(K, MainSha_eval):                       -- solo desde Evaluate, E4 (E.3.0)
 0. Precondición: Evaluate pasó E1-E3; EFF y §16.13 ya están identificados.
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
| `##` con Kind ENTRY (el punto de entrada de WORKFLOW) | `MainSha_eval`, **incluida**: es gobierno de toda unidad |
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

**Cuándo se ejecuta y cómo se descubre:** siempre a través de `Evaluate` (E.3.0), por los actores y con las obligaciones de E.3.0. El autor del contrato
no necesita conocer I-62. Una delegación en curso que cruza EFF no exige nada nuevo al contrato:
- antes de escribir, el avance de `main` ya obliga a reemitir por 16.7 (S-13), igual que con cualquier otro avance de `main`, y el contrato reemitido conserva
  sus citas;
- después de escribir, la reverificación de 16.7 usa el `MainSha` nuevo con el contrato intacto, y `Evaluate` se ejecuta con ese `MainSha_eval`.

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
| MV-6 | **igualdad con la derivación**, por archivo MODIFIED p, con H1 = secciones en EFF^1 y H2 = secciones en EFF (todos los niveles + preámbulo): MODIFIED = {h ∈ H1 ∩ H2 : texto normalizado distinto}; REMOVED = H1 ∖ H2; ADDED ∪ ENTRY = H2 ∖ H1; ENTRY = {(AUTOMATION_PLAN, `### 16.13 …`), (WORKFLOW, el `##` del punto de entrada)}, ni más ni menos; encabezados únicos por archivo en ambas revisiones |
| MV-7 | el punto de entrada de WORKFLOW existe una sola vez en EFF y nombra §16.13 por su línea exacta; punteros presentes en §16 y §16.3 en EFF; §16.3 en EFF sin el puntero = §16.3 en EFF^1 (normalizado) |

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
| punto de entrada de WORKFLOW, §16.13 o punteros ausentes con EFF presente | MAP_INVALID |

**Con MAP_INVALID, ningún contrato I61 pasa `Authority` tras EFF.** STOP (S-12; P-15) al Coordinator. La corrección es un cambio normativo con su propia
autoridad, nunca un parche local del resolver.

### E.5 Mapa previsto (política del Freeze) frente a mapa derivado (F4)

El mapa materializado **es** la derivación de E.4. La tabla siguiente es la previsión del diseño. Una diferencia entre previsión y derivación se clasifica en
C-20b: es una corrección si la materialización se apartó del Freeze, y una A-n si el Freeze era incompleto.

| Archivo | MODIFIED previstas | ADDED previstas (solo I62) | ENTRY |
|---|---|---|---|
| `docs/AUTOMATION_PLAN.md` | `## 8. Implementacion, estado versionado y Pull Requests` (formato `/v2`); `## 16. …` (ancestro); 16.1, 16.3 (puntero), 16.4-16.9, 16.11 y 16.12 donde cambian | subsecciones de §16 para I62 (autoverificación, observación, adapters, independencia, custodia, adopción) | `### 16.13 …` |
| `docs/WORKFLOW.md` | `## 4. …` (referencia «al abrir»); `## 10. …` (fila) | — | `## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)` (punto de entrada) |
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

### E.6 Trazas de descubrimiento y resolución con un contrato `/v1` real sin cambios (análisis; C-20c las ejecuta en F4)

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

**Arnés:** C-20c **no llama a `Resolve`**. Cada caso ejecuta `Evaluate` (E.3.0) desde E1 con solo tres entradas: los bytes del contrato, el clon y
`MainSha_eval`. El arnés no conoce la ruta de §16.13 ni la del mapa: las obtiene del texto del punto de entrada y de §16.13. El registro de descubrimiento
(E5) forma parte del esperado.

**C-20c-1 — el contrato real, byte a byte** (blob `9b5ef6df…`; ningún campo cambia):

| Ejecución | `MainSha_eval` | Resultado esperado |
|---|---|---|
| (a) delegación aceptada antes de EFF | `K.MainSha` = `95690c28` | PRE_ACTIVATION: lectura de I-61 idéntica a la de hoy (regresión nula) |
| (b) reverificación de 16.7 sin conflictos tras EFF | `MainSha` nuevo del `RebaseMap` = M2 | descubrimiento E1-E3 en M2 y resolución de las tablas siguientes; `Authority` pass sin reemisión |

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

**Descubrimiento en (b)** — la cadena no pasa por §16, que este contrato lee en AR:

| Paso | Lee | Revisión | Esperado |
|---|---|---|---|
| E1 | `docs/WORKFLOW.md` (gobierno de proceso) | M2 | su blob en M2 |
| E2 | trailer normativo y sección de entrada de WORKFLOW | M2 | EFF único; una sola sección de entrada |
| E3 | AUTOMATION_PLAN `### 16.13 Compatibilidad de protocolos de ejecución delegada`, nombrada por el punto de entrada | M2 | una sola subsección |
| E4 · `Classify` | `claim_id` del estado de I-61 y PRE del cuerpo de EFF (fila de prueba) | `BaseSha`; EFF | I61 |
| E4 · mapa | `compatibility/I62-clause-map.json` | EFF | MV-1..MV-7 en pass |
| E4 · citas | `Authorities` del contrato | tabla anterior | — |
| E4 · 16.3 | §16.3 de I-61, con «se lee en MainSha» sustituido por la revisión resuelta | AR | `Authority` pass |
| E5 | registro de descubrimiento y resolución | — | en `Authority.Evidence` |

**C-20c-2 — el mismo contrato para una unidad I61 que no es I-61.** Es lo que escribiría un autor que solo conoce I-61. Solo cambian los datos de emisión:
- `Unit` = I-64 (ANTERIOR en PRE, con su `Claim-Id` real);
- `Gate`, `TaskId` y `Objective` del escenario;
- `AuthorityRevision` = un commit de la rama de I-64;
- `MainSha` = M2 (emitido tras EFF);
- la clase de las 14 citas no `UNIT_DOC`, que pasan a `EXTERNAL` (los cambios de I-61 son externos para I-64);
- las 3 `UNIT_DOC`, que pasan a ser los documentos de I-64.

**`Path` y `Section` son idénticos byte a byte, incluidas las citas de documento completo, y no se añaden citas de §16.13 ni del mapa.** Mismo arnés y
mismo descubrimiento E1-E3 que en C-20c-1. Los punteros de §16 en M2 existen, pero el esperado no los usa.

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
| N-m | §16.13 retirada en M2 con EFF presente | ENTRY_INVALID (E3) → STOP |
| N-n | M2 sin la sección de entrada de WORKFLOW (EFF presente), con C-20c-1 | ENTRY_INVALID (E2) → STOP |
| N-o | sección de entrada repetida en WORKFLOW | ENTRY_INVALID (E2) → STOP |
| N-p | la sección de entrada nombra un encabezado de §16.13 que no existe | ENTRY_INVALID (E3) → STOP |
| N-q | M2' = M2 + X4, que retira los punteros de §16 y §16.3; sección de entrada presente | C-20c-1 y C-20c-2 resuelven igual: los punteros no son necesarios |
| N-r | sección de entrada presente en una main sin trailer efectivo | ACTIVATION_INVALID (E2) → STOP al Owner |
| N-s | verificación cuyo `Authority.Evidence` no trae el registro de descubrimiento y resolución | el Coordinator no la acepta (16.1); reverificación |

### E.7 Obligaciones

- La integración publica el punto de entrada de WORKFLOW, §16.13, los punteros, el mapa y su esquema en **el mismo** merge efectivo; sin activación parcial (WORKFLOW §11.2).
- El mapa se genera por la derivación y se revisa contra E.5 en F4 (C-20b). Se **regenera y se revisa de nuevo sobre el merge local** antes del push de la
  integración.
- **Sin renombrar ni borrar encabezados existentes** en las superficies: una sección de I-61 modificada conserva su línea de encabezado exacta. Así los contratos
  I61, escritos con los encabezados de siempre, siguen coincidiendo, y REMOVED queda vacío por diseño (MV-6 lo comprueba igualmente).
- La materialización añade contenido nuevo en **secciones nuevas** cuando no necesita cambiar una existente, para no fijar en EFF^1 secciones que siguen
  evolucionando (catálogo en particular).
- Los encabezados son únicos por archivo en las superficies (C-20a).
- El encabezado del punto de entrada de WORKFLOW y el de §16.13 son fijos: cambiarlos es un cambio normativo con su propia compatibilidad (C-20a).
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

### F.6 Rebase entre ventanas y reanudación en otra máquina (A62-V6-01; C-15)

Notación adicional:
- `'` marca una imagen tras el rebase;
- `M1` es el `main` nuevo;
- la cadena T está IN_COURSE con un RED acreditado (`chain_red_sha` = R) y un commit sin verificar U de una ventana abandonada.

| Paso | Actor | Git (rama) | Estado | Comprobación |
|---|---|---|---|---|
| 1 | P | … → G → **Q7** (`rv` = n; `chain_base_sha` = Q0a, `chain_red_sha` = R, `unverified_commits` = {U}, `last_window.verified_sha` = G) → push | ventana k cerrada | B.8.4 en pass |
| 2 | — | `main` avanza a M1 | — | — |
| 3 | P2 (sesión nueva, mismo host) | rebase sobre M1: Q0a', R', U', G', Q7' (Q7' = imagen de Q7, mismo contenido de estado) | SHAs de rama del estado sin reconciliar | `patch-id` de R' = R, U' = U y G' = G; `RebaseMap` completo |
| 4 | P2 | `git push --force-with-lease=<rama>:<Q7>` → remoto = Q7' | sin cambio (no es punto durable) | I-H02 falla a propósito: Q0 prohibido |
| 5 | P2 | **QU** (`point_kind` = REBASE_RECONCILIATION; `rv` = n+1; `RebaseMap` custodiado; `chain_base_sha` = Q0a', `chain_red_sha` = R', `unverified_commits` = {U'}, `verified_sha` = G'; contadores iguales) → push (CAS) | reconciliado | I-P10 e I-H02 en pass |
| 6 | P3 (otra máquina, clon limpio) | clona el remoto: sin Q0a, R, U, G ni Q7 originales | lee el QU | recalcula los `patch-id` sobre R', U' y G' y coincide con el `RebaseMap`; cita la corrida de CI del RED por el SHA original R (registro de CI) |
| 7 | P3 (tras QR o T16, si cambia el titular) | **Q0** de la ventana k+1 (`task_intent` con contrato reemitido si contenía SHAs reescritos) | OPENABLE | I-S15, I-H01 e I-H02 en pass |
| 8 | P3 → C | sin escritura | — | la entrega siguiente exige RED si toca `chain_red_files`; con `SupersededCommits` = {U'}, el `Scope` acumulado parte de la imagen correspondiente (B.8.6) |

**Variantes:**
- en el paso 3, la imagen de U no tiene el mismo `patch-id` → la rama local vuelve a `branch_before`, no hay push y es STOP (16.7);
- en el paso 4, el remoto cambió entre el `fetch` y el push → `--force-with-lease` rechaza y es STOP (T10);
- un Q0 entre los pasos 4 y 5 → rechazado (I-H02).

### F.7 QH + avance de `main` + Principal nuevo en un host limpio (A62-V7-02; C-15)

| Paso | Actor | Git (rama) | Estado | Comprobación |
|---|---|---|---|---|
| 1 | A | **QH** (`rv` = n; titular RELEASED; cadenas T_old VERIFIED y T IN_COURSE con su RED) → push | sin ventana | B.8.4 en pass |
| 2 | — | `main` avanza a M1 | — | — |
| 3 | Coordinator | — | — | decisión: designa a B con `I62-REBASE-TAKEOVER: <BindingId de B>` y acepta su binding; terminación de A acreditada |
| 4 | B (host nuevo, clon limpio) | `fetch`; rebase sobre M1 → imágenes, incluida QH' | SHAs de rama sin reconciliar | `patch-id` iguales; `RebaseMap` con `StateFields` de **todas** las cadenas (T_old y T) |
| 5 | B | `git push --force-with-lease=<rama>:<QH>` → remoto = QH' | sin cambio (no es punto durable) | I-H02 falla a propósito: ni trabajo ordinario ni Q0 |
| 6 | B | **QR** (`point_kind` = REBASE_RECONCILIATION; `rv` = n+1; titular B con su designación; `RebaseMap` custodiado; SHAs de T_old y de T reconciliados) → push (CAS) | titular B, reconciliado | I-P02, I-P07, I-P10 e I-H02 en pass |
| 7 | B | trabajo ordinario o **Q0** de T | — | I-S15 e I-H01 en pass |

**Variantes:**
- en el paso 3, la designación sin el marcador REBASE_TAKEOVER, con `main` avanzado → B no puede ni rebasar ni publicar el QR: STOP y se pide la decisión;
- en el paso 5, `--force-with-lease` rechazado → STOP (T10);
- T12b (titular ausente con último punto Q0) más avance de `main` → el mismo orden. El QR combinado cierra además la ventana como ABANDONED (B.8.5), con los
  `unverified_commits` como imágenes.

### F.8 Bucle de revisión del Architect con cambio de Principal (R62-AUTO-05, R62-AUTO-11; A62-V9-02, A62-V9-06; C-29, C-31)

Notación: `r1` y `r2` son solicitudes lógicas; `a1.1` es el intento 1 de `r1`; `inv1` es su `RoleInvocation`. Cada **QU** es un punto durable publicado por
CAS.

| Paso | Actor | Git (rama de la unidad) | `orchestration` tras el paso | Comprobación |
|---|---|---|---|---|
| 1 | A | publica X (commit c1, blob b1) | — | — |
| 2 | A | **QU**: bucle abierto con `loop.object` = {c1, X, b1} y `phase` = REVIEW_PENDING; `r1` OPEN; `a1.1` BUDGET_RESERVED, con un binding de B materializado (C-40) e `inv1` con su cierre; `review_rounds` = 1, `logical_requests` = 1, `architect_launches` = 1 → push | `next_action` = ARCHITECT / REVIEW | B.9 válida; topes no superados |
| 3 | A | **QU**: `a1.1` LAUNCHING con `run_id` = R1; `phase` = ARCHITECT_INVOKED → push; después lanza B | — | ningún lanzamiento sin este QU |
| 4 | A | **QU**: `a1.1` LAUNCHED, con su evidencia → push | — | arranque ligado a R1 |
| 5 | B (Architect, solo lectura) | sin escritura | — | resultado res1: CHANGES REQUIRED, con los REQUIRED R1 y R2 (linajes L1 y L2) |
| 6 | A | **QU**: `a1.1` RESULT_RECEIVED, con res1 custodiado, la terminación, el `RuntimeEvidenceRef` y la auditoría de lecturas → push | — | todavía sin aplicar |
| 7 | A | **QU**: `a1.1` RESULT_INGESTED, `outcome` VALID; `r1` INGESTED; `findings` = {L1: OPEN, L2: OPEN}; `phase` = CORRECTING → push | `next_action` = PRINCIPAL / CORRECT_AND_REREVIEW | res1 válido (B.10.1); `loop.object` sin cambio |
| 8 | — | A termina antes de corregir | — | terminación acreditada |
| 9 | C (Principal sucesor) | **QR** por T16, o T22 si `main` avanzó → push | sin cambios en `findings`, `review_requests` ni `budgets` | C lee solo el estado y los artefactos custodiados |
| 10 | C | publica X2 (commit c2, blob b2) **solo**, con sus respuestas a L1 y L2; su CI corre sobre c2 | — | — |
| 11 | C | **QU**: `phase` = PUBLISHED; **`loop.object` = {c2, X2, b2}**; `correction_rounds` = 1; `corrections_by_lineage`: L1 = 1, L2 = 1 → push | — | I-P13: el único par en que cambia `loop.object` |
| 12 | C | **QU**: `phase` = CI_VERIFIED, con la CI exacta de c2 → push | — | `loop.object` sin cambio |
| 13 | C | **QU**: `phase` = REREVIEW_PENDING; `r2` OPEN sobre {c2, X2, b2}; `a2.1` BUDGET_RESERVED, con un Architect D materializado y distinto de B; `OpenFindings` = {L1, L2}; `review_rounds` = 2, `logical_requests` = 2, `architect_launches` = 2 → push | `next_action` = ARCHITECT / REVIEW | `loop.object` sin cambio |
| 14 | C | **QU**: `a2.1` LAUNCHING con `run_id` = R2; `phase` = ARCHITECT_INVOKED → push; lanza D | — | `loop.object` sin cambio; cambiarlo aquí es el negativo «tardío» de C-38 |
| 15 | D | sin escritura | — | res2: AGREED, con las disposiciones L1 CLOSED y L2 CLOSED sobre b2 |
| 16 | C | **QU**: `a2.1` RESULT_RECEIVED → push | — | — |
| 17 | C | **QU**: `a2.1` RESULT_INGESTED; `closed_by` de L1 y L2 = res2; `phase` = ARCHITECT_SATISFIED sobre b2 → push | `next_action` = OWNER / DECIDE (p. ej., OD-6), o el siguiente gate | I-S18 e I-P13 en pass |

C nunca marca L1 o L2 como CLOSED: lo hace res2 con sus disposiciones. Si en el paso 15 D omitiera L2 y devolviera AGREED, el resultado sería INVALID
(§20.5.2). Si D devolviera CHANGES REQUIRED con L1 en STILL_OPEN, la corrección siguiente subiría `corrections_by_lineage(L1)` a 2; con `review_rounds` = 3 en
el tope ya no habría más correcciones, y sería STOP (P-18).

**Caídas en cada frontera.** La recuperación la hace un sucesor en otro host, sin artefactos transitorios (§20.6):

| Caída | Último QU | Recuperación | Presupuesto |
|---|---|---|---|
| entre 2 y 3 | `a1.1` BUDGET_RESERVED | caso A: el sucesor publica LAUNCHING y lanza con la misma reserva; puede replanificar `inv1` si su host cambia el binding | sin lanzamiento nuevo |
| entre 3 y el lanzamiento, o entre el lanzamiento y 4 | `a1.1` LAUNCHING | caso B: se resuelve con la operación 7 ligada a R1. Si se prueba que no arrancó → BUDGET_RESERVED y caso A. Si terminó con salida accesible → RESULT_RECEIVED. Sin acreditación (host de A inaccesible) → **LAUNCH_UNCERTAIN**, y `a1.2` como reejecución | con LAUNCH_UNCERTAIN y `a1.2` reservado: `architect_launches` = 2, `transport_reruns(r1)` = 1 |
| entre 5 y 6 | `a1.1` LAUNCHED | caso B: el resultado de B está en el host de A, fuera de Git; si es inaccesible → RESULT_RECEIVED con salida ABSENT, y reejecución | `transport_reruns(r1)` = 1 al reservar `a1.2` |
| entre 6 y 7 | `a1.1` RESULT_RECEIVED | caso C: el sucesor ingiere res1 desde Git; repetir la ingestión da el mismo estado | sin cambio |
| después de 7 | `a1.1` RESULT_INGESTED | caso D: nada que recontar ni relanzar | sin cambio |
| entre 10 y 11 | `phase` CORRECTING, con c2 ya en el remoto | el sucesor puede adoptar c2 como X2 si su ruta está en `ObjectFamily` y su autor es el titular anterior según la custodia, y publica el QU PUBLISHED; si no, corrige de nuevo desde el estado | `correction_rounds` sube solo en el QU PUBLISHED |

**Vigencia de la autorización** (A62-V10-03; C-40). Variantes sobre la misma secuencia:

| Variante | Estado | Resultado |
|---|---|---|
| la autorización caduca después del paso 17 | ARCHITECT_SATISFIED sobre b2 ya publicado | res2, las disposiciones de L1 y L2 y ARCHITECT_SATISFIED siguen acreditados (acreditación histórica); ninguna acción nueva es posible |
| el Coordinator revoca la autorización entre los pasos 13 y 14 | `a2.1` en BUDGET_RESERVED | el LAUNCHING del paso 14 se rechaza; `a2.1` pasa a CANCELLED_BEFORE_LAUNCH; la reserva sigue contada; COORDINATOR_DECISION |
| el Coordinator revoca la autorización entre los pasos 14 y 15 | `a2.1` en LAUNCHING, con D lanzado | `a2.1` se completa (pasos 15-17) y res2 se ingiere: no son acciones nuevas |
| un Principal sucesor reconstruye tras la caducidad | estado custodiado del paso 17 | valida el binding de D y res2 contra `AuthorizationRef.Commit`/`Blob`; la revisión sigue acreditada |
| reutilizar la autorización caducada para materializar un Architect nuevo | `action_validity` = ENDED | rechazo (I-P13); hace falta una autorización nueva del Coordinator |

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
| `Authority` (16.9 #3) y 16.3 | `Evaluate` (E.3.0): punto de entrada de WORKFLOW → §16.13 → 16.3, aplicado por quien evalúa el contrato; el contrato `/v1` no cambia | **ampliado** (sin efecto en el contrato) |
| regla de entrada para la lectura de autoridades | `/v1` no tiene ninguna independiente del contrato; I-62 añade la sección de entrada de WORKFLOW (transición, §10), bajo OD-1 | **nuevo** (delta declarado) |
| prompt de verificación del Controller en una unidad I61 tras EFF | + la entrada de compatibilidad (instrucción de `Evaluate` y encabezados, **sin** resultado esperado; comparación posterior), como entrada canónica, no en el contrato | **ampliado** (único cambio operativo para unidades I61; lo ejecutan los evaluadores) |
| 16.3: «`EXTERNAL` se lee en `MainSha`» | para unidades I61 tras EFF, las cláusulas del mapa se leen en `I62_EFFECTIVE_SHA^1`; el resto en `MainSha`; una cita de documento completo de un archivo modificado se lee compuesta (E.3); `MainSha` y `AuthorityRevision` sin cambio de significado | **sustituido solo en la revisión de lectura de las cláusulas del mapa** (compatibilidad declarada, con su compromiso) |
| contrato `/v1` existente de una unidad I61 | sin cambios: ni campos, ni citas nuevas, ni reemisión para acogerse al resolver | **conservado** |
| A1 | + conjunto I62 + B.3 | **ampliado** |
| A2 | + `BindingRef` (B.2) | **ampliado** |
| A6 | + `ProtocolSet` = protocolo de la unidad (E.2) | **ampliado** |
| A7 | elegibilidad vía binding aceptado (individual o materializado bajo autorización, con acreditación histórica, §20.5.1) + observación vigente | **sustituido** (misma regla de ADR-0046 #4) |
| A8 | + `CountersSnapshot` = autoridad | **ampliado** |
| `Identity`, `Remote`, `CleanTree`, `FreeText`, `Denials` | sin cambio (`Denials` incluye la pérdida de autenticación como S-06, como hoy) | **conservado** |
| 16.1 «trabajo delegado terminado» | + sin `unverified_commits` pendientes de la tarea (B.8.6) | **ampliado** |
| 16.4 paso (1): «commit de estado de la sesión, si hace falta» | Q0 **obligatorio** antes de cada ventana (W-1) | **ampliado** |
| 16.4 orden de commits (pasos 2-7) | **sin cambio**; la custodia usa Q0/Q7 y el diario | conservado |
| 16.6 «delegación abierta»: aceptada y sin verificación válida ni cierre; una por unidad | **igual**; derivada del diario (B.8.2), nunca durable; el estado registra la intención planificada (`task_intent`) y el cierre (`last_window`) | **conservado** |
| 16.6 «cadena en curso» | `chains[].state` = IN_COURSE, con `correction_authorized_pending` | conservado (registro explícito) |
| 16.8 por clase: correcciones lanzadas | igual, registradas en el Q0 de la corrección (`correction_launches`) | conservado (autoridad explícita) |
| 16.7 rebase de la rama (apertura u otro) **fuera de una ventana** | rebase; publicación con `--force-with-lease` y SHA remoto esperado explícito (no es punto durable); **QU REBASE_RECONCILIATION** con el `RebaseMap` custodiado y todos los SHA del estado mapeados, antes de cualquier Q0; imagen no acreditable → STOP (§8.8). **Dentro de una ventana:** misma vía de 16.7, con `StateFields` de todo SHA persistido afectado y reconciliación en el Q7 o QR de cierre (I-P12). **Toma por un Principal entrante con avance de `main`:** REBASE_TAKEOVER y un QR combinado (§8.9, T22) | **ampliado** (OD-1) |
| transporte entre roles (I-61: relevo automático solo del Controller y del Worker dentro de una delegación; el resto, por el Owner) | RELAY automático entre roles IA vinculables con `RoleInvocation`, resultado estructurado y estado `orchestration`; el Owner solo por ESCALATION (§20) | **ampliado** (OD-1) |
| revisión del Architect (I-61: sin contrato de invocación ni de resultado) | bucle autónomo acotado (§20.5) con `ReviewLoopAuthorization`, materialización autorizada de bindings, intentos con reserva y recuperación, presupuestos congelados con fórmula cerrada, disposiciones explícitas y linaje; el Principal nunca cierra un REQUIRED | **nuevo** (OD-1) |
| resultado de una revisión (I-61: no existe) | `architect-review-result/v1` y `reviewer-result/v1`, discriminados por rol, acción y contrato de salida (§20.7, B.10) | **nuevo** (OD-1) |
| insumos de una invocación (I-61: `prompt.md` custodiado con SHA-256) | cierre efectivo de insumos con las lecturas transitivas obligatorias, y exención explícita y acotada de las acciones iniciales incompatibles, sin equivalencia de evidencia (§20.3.1) | **ampliado** (OD-1) |
| fidelidad de los insumos (I-61: no existe) | `input-fidelity/v1`: preflight de transporte en UTF-8 por el mismo camino de lectura, comparación tras la corrida, fallo cerrado e invalidación acotada por hallazgo (§20.3.3) | **nuevo** (OD-1) |
| identidad efectiva de un participante (I-61: `relay-record` con solicitado y efectivo) | igual; además, la identidad que declara un revisor no acredita nada, y su contradicción con la observada es S-04 (§20.3.2) | **ampliado** |
| AUTOMATION_PLAN §16: «Ninguna otra iniciativa lo adopta por estar escrito» (opt-in) | igual: la ejecución delegada I62 solo se adopta con decisión registrada (marcador de G0 o adopción T20); DIRECT_ONLY trabaja sin la maquinaria I62 (§14.0) | **conservado** (aplicabilidad explícita bajo OD-1) |
| LIFECYCLE §5, modos de revisión | con la alternativa 1 de OD-6: los tres modos se conservan; SEPARATE SESSION y EXTERNAL HUMAN con su evidencia propia frente a todos los autores; SAME-SESSION ROLE no basta para las revisiones mayores (§11.4) | **ampliado** solo si OD-6 elige la alternativa 1 |
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
| Intento de revisión en LAUNCHING con el host del Principal anterior inaccesible (rol de solo lectura) | LAUNCH_UNCERTAIN; el intento ya contó en su reserva; el intento siguiente es una reejecución: `transport_reruns` +1 y `architect_launches` +1 al reservarlo; un resultado tardío del incierto no se ingiere |
| Cuarto intento de la misma solicitud lógica, con `InvocationId`, `RunId` y proveedor nuevos | rechazado antes de reservar (P-18); contadores sin cambio |
| AGREED que omite un REQUIRED abierto | INVALID (P-19); el linaje sigue abierto; reejecución de transporte dentro del tope |
| La autorización caduca después de ARCHITECT_SATISFIED | el resultado, las disposiciones y ARCHITECT_SATISFIED siguen acreditados; ninguna acción nueva es posible |
| Resultado con ≤ leído como «=» en la premisa de un hallazgo | ese hallazgo es INVALID_PREMISE y no abre linaje (P-25); los hallazgos con premisas fieles se ingieren; sin ARCHITECT_SATISFIED |
