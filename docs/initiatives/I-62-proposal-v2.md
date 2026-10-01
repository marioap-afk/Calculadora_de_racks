# I-62 — Proposal V2: portabilidad del Principal y binding de roles independiente del proveedor

```text
Frozen: NO
Version: V2 (sustituye a V1 en la revisión; V1 se conserva sin cambios como la versión revisada)
Unit: I-62   Workflow: V2 (T4)   Claim-Id: 5b661a17-8c18-4183-8554-3866059cba2b
Archetype: NEW ARCHITECTURE (Coordinator, C62-F0-09; el mandato conserva la etiqueta FOUNDATION EVOLUTION)
Base: origin/main 819955d6   Discovery: docs/initiatives/I-62-discovery.md, ronda R1 (blob 86f24e65), base de diseño (C62-F0-08)
Previous review: Coordinator, SEPARATE SESSION, CHANGES REQUIRED sobre V1 (docs/initiatives/I-62-coordinator-review-v1.md; R62-V1-01..09, O62-V1-01)
Author: sesión principal responsable de I-62 (Claude), redacción directa (C62-F0-15)
Review: PENDIENTE — Coordinator y Architect (paquete: docs/initiatives/I-62-architect-package-v2.md; revisión del Architect no realizada)
Open Owner points: OD-6 BLOCKED — OWNER DECISION (§11.4); los demás OD bloquean en su frontera real (§18)
IMPLEMENTATION AUTHORIZATION = NO   ·   I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED
```

**Fuentes:**
- Alcance: el mandato ([I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt)).
- Hechos: el [Discovery](I-62-discovery.md) R1 y su §21.
- Decisiones: [decisiones](../automation/decisions/I-62.md) §§7-11 (C62-G0-02, C62-F0-01..16).
- Revisión de V1: el [registro](I-62-coordinator-review-v1.md).

**Precedencia:** WORKFLOW, AGENTS, LIFECYCLE, AUTOMATION_PLAN §16, ADR-0046 y el Freeze de I-61 mandan mientras I-62 no se integre.

**Diseño, no conducta:** esta Proposal no modifica el protocolo operativo, no publica esquemas ni cambia ninguna conducta. Los contratos de datos se fijan
como **diseño** en el Anexo B. REFERENCE OVER REPETITION.

**Mapa de la corrección V1→V2:**

| Hallazgo | Capítulos de V2 |
|---|---|
| R62-V1-01 | §14 (D-18) |
| R62-V1-02 | §15 (D-15) |
| R62-V1-03 | §§17-18 (D-16) |
| R62-V1-04 | §11 (D-12) |
| R62-V1-05 | §§5-7, Anexo B |
| R62-V1-06 | §§8-9 (D-08..D-10) |
| R62-V1-07 | §16, Anexo C |
| R62-V1-08 | §12, Anexo D |
| R62-V1-09 | paquete V2 |
| O62-V1-01 | §§4.1, 6 |

## 0. Delta respecto de I-61

| Tema | I-61 integrado (fuente) | I-62 propone |
|---|---|---|
| Roles | Worker (Claude o Codex), Controller (Codex), Coordinator, sesión responsable (§16.1) | cinco roles **sin proveedor**; PRINCIPAL_COORDINATOR formaliza a la sesión responsable (D-01) |
| Proveedor del Controller | fijado (ADR-0046, Alternativas) | binding por capacidad acreditada (D-05); ADR sucesor parcial (Anexo A) |
| Sesión principal | sin perfil, celda ni obligación | perfil, autoverificación y `CONFIGURATION_STATUS` (D-03, D-04) |
| Preflight | solo dentro del relevo y con `config.toml` de Codex (16.4) | núcleo agnóstico + hechos por adapter (D-06) |
| Hechos de proveedor | en §16.4, README §§5-6 y `relay-record` | frontera de adapters con descriptor y esquema de hechos (D-07) |
| Independencia | parcial, aceptada como coste | política por dimensiones con dueño por dominio (D-12) |
| Custodia y recuperación | deducidas del relevo y del estado | registro por unidad con transiciones ejecutables y CAS por Git (D-08, D-09) |
| «Cierre de G2» | nombre de un gate de I-61 en 16.3, 16.5 y 16.7 | MaterializationClose por repositorio y plano (D-15) |
| Versiones | cinco `rackcad-*/v1` | `ProtocolSet` por **unidad** (I61 o I62), adoptado en el bootstrap y sin migración (D-18) |
| Nivel | A | **A**, decidido antes del Freeze con obligaciones de viabilidad (D-16) |

## 1. Objetivo, no-objetivos y criterios de éxito

**Objetivo** (mandato, «GOAL»): que cada rol se vincule a un proveedor, modelo o runtime según **capacidad acreditada**, sin supuestos fijos, y que otro
Principal pueda reanudar la unidad sin memoria privada.

**No-objetivos** (mandato, «OUT OF SCOPE» y «PARALLELISM WITH PRODUCT»; C62-F0-09):
- producto;
- reemplazar al Master o quitar autoridad al Coordinator;
- bucles sin límite, compras, gestión de credenciales o scheduler en la nube;
- modelos fijos;
- auto-merge;
- reescribir I-61;
- motor, framework de código, scheduler, servicio de locks o plataforma;
- registro global de roles, números, turnos o permisos entre unidades;
- migrar unidades activas.

| # | Criterio (mandato) | Mecanismo | Gate | Evidencia (Anexo C) |
|---|---|---|---|---|
| 1 | Principal independiente del proveedor | D-01, D-03 | F1 | OBL-01; FX-01 |
| 2 | Roles reasignables por hechos observables | D-05, D-06 | F3 | MC-04, MC-05; FX-02 |
| 3 | El Principal verifica su sesión | D-03, D-04, D-11 | F1 | MC-01; FX-01; OV-I62-01 |
| 4 | UNKNOWN nunca se vuelve MATCH | D-04 | F1 | MC-01; FX-01 |
| 5 | Preflight de portabilidad del entorno | D-06 | F2 | MC-02, MC-03; OV-I62-02 |
| 6 | Esquemas neutrales | D-07, D-14 | F2 | OBL-03, MC-03 |
| 7 | Detalles de proveedor en adapters y catálogos | D-07 | F2 | OBL-06, MC-03 |
| 8 | Independencia por riesgo | D-12 | F3 | MC-06; FX-02 |
| 9 | Segundo Principal sin memoria privada | D-08, D-17 | F6 | FX-04; OV-I62-05 |
| 10 | Recuperación determinista | D-09 | F4 | MC-07, MC-08 |
| 11 | Sin secretos | D-06, D-07 | F2 | MC-09 |
| 12 | Dos topologías o límites honestos | D-17 | F6 | FX-02, FX-03; OV-I62-03/04 |
| 13 | I-61 intacto | D-14, D-18 | todas | OBL-09, MC-10 |
| 14 | No bloquear producto | D-18 | todas | MC-10; DC-07 por hito |

## 2. Roles (D-01) y acumulación

| Rol | Semántica (estable) | Declara | Nunca declara |
|---|---|---|---|
| **PRINCIPAL_COORDINATOR** | Sesión responsable de la unidad: worktree, preflight, relevos, propuesta de bindings, trabajo directo autorizado, hechos y custodia | hechos del relevo, del remoto, del rebase y del preflight; `CONFIGURATION_STATUS` propio; disposición de transporte | `EXECUTION_*`; GATE PASS (salvo que tenga además el rol Coordinator, declarado SAME-SESSION ROLE) |
| **ARCHITECT** | Revisión de diseño y conformidad (LIFECYCLE §5, §9) | veredicto de LIFECYCLE | GATE PASS, `EXECUTION_*` |
| **EXECUTION_CONTROLLER** | Planificación y verificación de una delegación, solo lectura (§16) | `EXECUTION_VERIFIED`, `EXECUTION_REWORK_REQUIRED`, `EXECUTION_BLOCKED` | GATE PASS, Candidato, cierre, integración |
| **WORKER** | Escritura dentro del alcance | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS |
| **REVIEWER** | Revisión de una entrega, evidencia o decisión operativa fuera de LIFECYCLE, sin autoridad de Architect ni de Controller | hallazgos con severidad y evidencia | `EXECUTION_*`, GATE PASS, veredictos de LIFECYCLE |

El **Coordinator** de gates no es un rol vinculable: lo asigna la gobernanza. Las declaraciones exclusivas de ADR-0046 #2 se conservan.

**Acumulación por entrega.** La independencia que exija cada combinación la da la tabla de §11. Aquí solo se fija lo que nunca puede acumularse.

| Mismo actor en … | Regla |
|---|---|
| WORKER + EXECUTION_CONTROLLER de la misma entrega | **prohibido** (no autoaprobación; ADR-0046 #2) |
| WORKER + REVIEWER de la misma entrega | permitido como autorrevisión; **nunca** cuenta como revisión independiente |
| PRINCIPAL_COORDINATOR + WORKER (subagente) | permitido (alternancia interna, 16.4) |
| PRINCIPAL_COORDINATOR + REVIEWER / ARCHITECT / Coordinator | permitido como SAME-SESSION ROLE declarado; nunca cuenta como independiente |
| ARCHITECT + EXECUTION_CONTROLLER | permitido si §11 no exige independencia entre ambos; el veredicto de LIFECYCLE no se desplaza al Controller |
| REVIEWER + EXECUTION_CONTROLLER del mismo trabajo | permitido; no añade independencia frente al Controller |

## 3. Mapa de autoridad: anterior → delta (C62-F0-09 §3)

| Dominio | Hoy | Delta propuesto | Autoridad |
|---|---|---|---|
| LIFECYCLE §5 y §9 | revisión del Architect y conformidad; modos de revisión declarados | predicado de independencia para revisiones de diseño y READY-06 (§11.4), **pendiente de OD-6** | Owner (OD-6: cambia un criterio de revisión) + autoridades de LIFECYCLE |
| AUTOMATION_PLAN §16 | ejecución delegada (ADR-0046 #1) | roles operativos, binding y aceptación, preflight, obligación de autoverificación del Principal, contrato de adapter, custodia operativa, recuperación delegada, conteo, adopción por unidad | AUTOMATION_PLAN; **ampliación de dominio** abajo |
| AUTOMATION_PLAN §8 | formato del estado | `rackcad-automation-state/v2` con `protocol` y bloque `custody` (Anexo B.8); un valor guardado no da autoridad: se distingue declarado, observado y autorizado | AUTOMATION_PLAN |
| WORKFLOW §§3-4 | reclamo, worktree, apertura y relevo | solo **referencias** (paso «al abrir» → autoverificación de §16); el registro de custodia no adquiere ramas ni ventanas ni excepciona el relevo | WORKFLOW |
| `routing.md` / catálogo | selección por capacidades | clase y perfil de coordinación principal; binding de todos los roles; §7 pasa al adapter; celdas por rol | punto de extensión del Freeze de I-61 §15 |
| PROMPT_TEMPLATES §G y procedimientos de adapters | representación y recetas | renderizado por PromptProfile en el adapter; corrección del bloque factual obsoleto de §2 (en F1) | PROMPT_TEMPLATES |
| AGENTS | evidencia | sin cambios | — |

**Ampliación efectiva del dominio de §16:**
- **Fuente anterior:** ADR-0046 #1 («ejecución delegada bajo orden explícita del Coordinator») y la fila de WORKFLOW §10.
- **Delta:** obligaciones de la **sesión principal fuera de una delegación** (autoverificación al abrir, preflight, custodia operativa por unidad).
- **Autoridad reservada:** Owner (OD-1). Antes de la vigencia (D-15), estas reglas existen solo como materialización inactiva y no se ejercen como
  autoridad.

## 4. Perfil del Principal y estado de configuración

### 4.1 Perfil (D-03) y capacidades por rol (O62-V1-01)

`routing.md` recibe la clase «Coordinación principal» con perfil PRINCIPAL_COORDINATION:
- horizonte `High`, que fija Long-horizon;
- ambigüedad, sensibilidad y coste de fallo `High`;
- nivel Frontera.

Nombres y fechas solo en el catálogo. Campos del mandato: Anexo B.4.

**Requisitos obligatorios del Principal**, expresados como **capacidades** (la herramienta concreta la fija la receta del adapter o del host):

| Capacidad | Por qué es obligatoria para el Principal | Herramienta en la receta actual |
|---|---|---|
| nivel y effort del perfil | mandato, SELF-VERIFICATION | fuente de introspección del adapter (§10) |
| `repo-write` (commit y push de la rama de la unidad) | custodia, relevos y transiciones por CAS (§8) | Git |
| `remote-facts` (CI de push de un SHA exacto) | 16.9 `Ci` y relevos | GitHub CLI autenticado |
| `introspection` con nivel mínimo RUNTIME_OBSERVED | D-11 | según el adapter |

`build-test` (SDK) **no** es obligatorio para el Principal en general. Lo es para el rol y la acción que deban producir evidencia local de build o pruebas:
el Worker de una tarea con código, o el Principal cuando la autoridad de evidencia exija una corrida local. No se retira ninguna herramienta de una receta
medida.

### 4.2 `CONFIGURATION_STATUS` y disposición (D-04)

Estados exactos: **MATCH**, **ABOVE_REQUIRED**, **BELOW_REQUIRED**, **UNKNOWN**. Agregado **solo sobre los requisitos obligatorios**:
1. BELOW_REQUIRED si alguno es insuficiente;
2. si no, UNKNOWN si falta acreditar alguno;
3. si no, ABOVE_REQUIRED si alguno lo supera;
4. en otro caso, MATCH.

Las métricas opcionales no alteran el agregado. El registro conserva **todas** las causas (Anexo B.4, `Causes[]`).

| Situación | Disposición |
|---|---|
| Principal en BELOW_REQUIRED | STOP antes de trabajo sustantivo (P-09) |
| Rol vinculable en BELOW_REQUIRED o UNKNOWN | no hay binding ni trabajo dependiente (P-10); la investigación independiente sigue |
| Evidencia material contradictoria | STOP S-04, **además** de su efecto sobre el estado; no lo tapa un MATCH ni un BELOW_REQUIRED de otro campo |
| MATCH / ABOVE_REQUIRED | habilita solo el aspecto de configuración; custodia, autenticación, consumo, alcance, CI y demás STOP siguen |
| Decisión del Owner | puede cambiar un requisito o aceptar un riesgo dentro de su autoridad; nunca convierte lo no observado en MEASURED o MATCH |
| Recuperación | nueva observación o revalidación; nunca reconfiguración automática ni reset |

**Ejemplos con esperado independiente** (el esperado lo fija esta Proposal; una vez acordada, es la fuente del oráculo de MC-01 y FX-01):

| Caso | Entrada (requisitos obligatorios) | Agregado esperado | Disposición esperada |
|---|---|---|---|
| E1 | nivel = req; effort = req; introspección RUNTIME_OBSERVED | MATCH | sigue |
| E2 | nivel = req; effort > req | ABOVE_REQUIRED | sigue y registra el motivo |
| E3 | effort < req | BELOW_REQUIRED | STOP P-09 (Principal) |
| E4 | effort sin fuente | UNKNOWN | P-10 / no hay trabajo sustantivo dependiente |
| E5 | nivel < req; effort sin fuente | BELOW_REQUIRED, `Causes` = {nivel, effort} | STOP P-09 |
| E6 | dos fuentes RUNTIME_OBSERVED del mismo instante con effort distinto | UNKNOWN; `Contradiction = true` | STOP S-04 |
| E7 | observación anterior a un cambio de selector, máquina o binario | UNKNOWN (invalidada) | revalidar |
| E8 | rebinding del Worker de X a Y | se evalúa Y desde cero | contadores intactos; custodia transferida |
| E9 | cuota desconocida (opcional); obligatorios en MATCH | MATCH | métrica registrada |
| E10 | el Owner rebaja el requisito de nivel para una tarea | recálculo contra el requisito nuevo | lo no observado sigue UNKNOWN |

## 5. Binding de roles (D-05)

**Entradas:**
- TaskProfile y requisitos semánticos;
- requisito de independencia (§11);
- preflight vigente (D-06);
- **evidencia de elegibilidad** (ADR-0046 #4, que se conserva): invocación **medida** de la celda con esa capacidad y ese effort, **consumo cubierto**
  (fila oficial del plan o invocación medida sin aviso de límite) y celda no `STALE`.

Una cuota opcional desconocida **no** equivale a consumo autorizado. Una entrada de catálogo **no** acredita los hechos.

**Orden, sin ciclos:**
1. Candidatas del catálogo, estáticas.
2. Preflight: observación sin invocar.
3. Evidencia de invocación medida, procedente de una medición previa **autorizada** que no es un binding (p. ej., una sonda).
4. Binding.
5. Aceptación por el Coordinator (A7').

Una celda sin medición previa no se vincula. Se mide primero con su propia autorización.

**Algoritmo:**
1. Requisitos del rol.
2. Candidatas con descriptor de adapter vigente.
3. Filtro: requisitos obligatorios en MATCH o ABOVE_REQUIRED; elegibilidad del paso 3 anterior.
4. Independencia satisfecha según §11.
5. Nivel más bajo adecuado y transporte por la jerarquía del mandato.
6. Registro `rackcad-binding/v1` (Anexo B.5): alternativas descartadas con causa, RoutingReason con fechas, escalado e instantánea de contadores.
7. Aceptación.
8. Transferencia de custodia (D-08).

**Sin celda elegible**, no se invoca. **Rebinding:** registro nuevo, misma `TaskId`, transferencia de custodia, contadores intactos (D-10).

## 6. Preflight de entorno (D-06)

Cada hecho declara:
- el requisito;
- el alcance de la observación;
- el valor y la fuente;
- el instante y el host;
- la disposición.

Campos exactos: Anexo B.4.

| Núcleo (todos los roles) | Por adapter |
|---|---|
| Git: ejecutable, repositorio, `origin/main`, rama, worktrees, árbol limpio, operaciones en curso | localización **verificada** del ejecutable (no «directorio más nuevo») y versión |
| custodia: delegaciones abiertas y escritores activos (D-08) | autenticación: AUTHENTICATED / NOT_AUTHENTICATED / UNKNOWN, sin credenciales |
| procesos: atribución con relectura (triage 1/9), lista aportada por el adapter | introspección: fuentes y nivel (§10) |
| área transitoria ignorada; directorios de evidencia | permisos y sandbox |
| capacidades del host que exija el **rol y la acción** (O62-V1-01) | huella de configuración (§7.3) |

**Invalidadores** de un preflight: cambio de máquina, runtime, versión o ruta del binario, credenciales, catálogo, binding, SHA relevante o huella. Un
preflight invalidado vuelve todos sus requisitos a UNKNOWN. Una CLI encontrada o un esquema válido **no** acreditan una invocación.

## 7. Frontera de adapters (D-07)

### 7.1 Contrato (normativo en §16)

Nueve operaciones:
1. describir el runtime;
2. observar capacidades y autenticación;
3. renderizar el prompt;
4. invocar;
5. observar el resultado;
6. cancelar por tope;
7. confirmar la terminación;
8. clasificar sus procesos;
9. declarar su huella de configuración.

Cada **descriptor** (`docs/automation/agent-execution/adapters/<adapter-id>.md`, procedimiento subordinado) declara, por operación, una de tres cosas:
- **DISPONIBLE**: con fuente y receta;
- **NO APLICA**: con razón;
- **UNVERIFIED**.

Una operación UNVERIFIED que el rol necesita hace que la celda **no sea elegible** para ese rol. Una sesión ya existente **no** acredita por sí misma un
mecanismo para invocarse, cancelarse ni confirmar su terminación.

### 7.2 Operaciones por adapter y rol (estado de partida; F2 lo mide)

| Adapter | Rol previsto | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |
|---|---|---|---|---|---|---|---|---|---|---|
| `claude-desktop-session` | PRINCIPAL_COORDINATOR | DISP. | DISP. (`get_session`) | N/A (no se invoca) | N/A | N/A | N/A | UNVERIFIED (posible `isRunning` de la app, por medir) | UNVERIFIED | UNVERIFIED (ajustes de la app) |
| `claude-subagent` | WORKER, REVIEWER (contexto propio) | DISP. | DISP. (transcripción) | DISP. (§G) | DISP. (llamada notificada) | DISP. | DISP. (tope 60 min) | DISP. (notificación + procesos) | DISP. (I-61) | UNVERIFIED (ajustes heredados) |
| `claude-cli` | REVIEWER, ARCHITECT externos | DISP. | NOT_AUTHENTICATED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED |
| `codex-cli` | EXECUTION_CONTROLLER, ARCHITECT, WORKER | DISP. | DISP. (login) | DISP. | DISP. (receta I-61; binario a revalidar) | DISP. (`-o`, `--output-schema`) | DISP. (600 s + `taskkill`) | DISP. (árbol de procesos) | DISP. | **DISP.: `config.toml` (obligatoria)** |
| `codex-desktop-session` | PRINCIPAL_COORDINATOR (B) | DISP. | candidata (SP-3) | N/A | N/A | N/A | N/A | UNVERIFIED | UNVERIFIED | UNVERIFIED (¿comparte `config.toml`? por medir) |

### 7.3 Huella de configuración

Toda configuración persistente que altere la conducta de un runtime entre invocaciones se declara como huella: SHA-256 + nombres de claves, sin valores.
«**Ninguna**» solo se admite si el descriptor demuestra que no existe tal configuración y el **Coordinator lo acepta en la revisión del gate F2**
(decisión registrada). Cambiar la declaración exige una nueva revisión. `codex-cli` y, si comparte el archivo, `codex-desktop-session` declaran
`config.toml`: **P-01 se conserva** (P-11 lo generaliza a toda huella declarada). Nunca se declara «ninguna» de forma unilateral.

### 7.4 Esquema de hechos y su validación

Mecanismo elegido: **`Test-Json -SchemaFile` de PowerShell 7**, el mismo validador que usa A1 en I-61. No se añade dependencia nueva. Detalle de
resolución y fallos en el Anexo B.3. Dos validaciones:
1. el artefacto contra su esquema de núcleo;
2. `AdapterFacts.Facts` contra el esquema que nombra `AdapterFacts.SchemaRef` (ruta y blob resueltos en la `AuthorityRevision`).

Las ejecuta el productor (la sesión) y quedan registradas. El aceptante (Coordinator, A1') y el Controller (`Authority`/`Contract`) verifican el registro.

**Fallos:**
- adapter desconocido, esquema ausente, versión incompatible o hechos malformados → **NO ELEGIBLE** (P-14);
- propiedad inesperada → inválido (esquemas estrictos).

## 8. Custodia por unidad (D-08)

**Registro:** bloque `custody` de `rackcad-automation-state/v2` (Anexo B.8), con `record_version` monótono. Cada valor distingue **declarado**,
**observado** (fuente e instante) y **autorizado** (decisión). Sin registro global. Los compromisos sobre superficies compartidas son solo referencias
(emisor, alcance, acuse, RELEASE, correlación, vigencia).

**Escritura durable con CAS por Git:** una transición de custodia es un commit en la rama de la unidad que:
1. lee `record_version = n` en `HEAD = origin/<rama>`;
2. escribe `n+1` con el estado previo esperado;
3. se publica con `git push` **sin force**.

Un push rechazado (la rama avanzó) significa **registro obsoleto**: la transición no ocurrió y se relee. No hace falta un servicio de locks.

Los hechos de Git prueban identidad y remoto, **no** quién sigue operando en otra máquina.

## 9. Recuperación (D-09) y presupuestos (D-10)

### 9.1 Evidencia de fin de un escritor

| Evidencia | Definición | ¿Basta para…? |
|---|---|---|
| **TERMINATION_ACCREDITED** | operación 7 del adapter, ligada al escritor exacto (`RunId` o `SessionRef`, PID + `CreationDateUtc`) y al alcance del binding | cerrar la cesión y transferir custodia, con las demás comprobaciones de entrada |
| **ISOLATION_ACCREDITED** | **no se admite en V2**: no hay mecanismo medido que impida escribir a un escritor vivo (un push fast-forward sigue siendo posible) | — |
| **NO_OBSERVATION** | ausencia de observación (otra máquina, conexión perdida, tope vencido) | **nada**: no permite tomar posesión |

Estados del escritor: **WRITER_ALIVE**, **TERMINATION_UNACCREDITED** (= NO_OBSERVATION sobre un escritor con custodia) y **ORPHAN_CONFIRMED**
(TERMINATION_ACCREDITED sin entrega ni cesión resuelta). Ni TERMINATION_UNACCREDITED ni ORPHAN_CONFIRMED autorizan tomar posesión por sí mismos.

### 9.2 Transiciones

Las escribe la sesión que tiene la custodia vigente o, en recuperación, la designada por decisión del Coordinator. Todas por CAS (§8).

| # | Estado previo | Evento | Precondición / evidencia | Decisor | Transición durable | Permitido / prohibido |
|---|---|---|---|---|---|---|
| T1 | HELD(P) | inicio de cesión a W | Exit de 16.4 en pass; una delegación abierta como máximo | P | CEDED(P→W), `record_version`+1, con el `RunId` de W | P no opera en el alcance (16.4 cesión) |
| T2 | CEDED(P→W) | W entrega | handoff válido + commits en el remoto | — | (sin transición: espera de 7) | — |
| T3 | CEDED(P→W) | terminación acreditada de W | TERMINATION_ACCREDITED + Entry de 16.4 en pass | P | HELD(P), `last_verified_sha` tras verificación | normal |
| T4 | CEDED(P→W) | tope vencido | operación 6 (cancelar) + operación 7 | P | HELD(P) solo si 7 = acreditada; si no → T6 | no lanzar otro escritor (16.4) |
| T5 | CEDED(P→W) | W caído con commits sin handoff | TERMINATION_ACCREDITED; commits en el remoto | P | HELD(P); `Handoff` → BLOCKED (16.9) | el Controller puede verificar commits verificables (NO_OUTPUT de I-61) |
| T6 | CEDED(P→W) o HELD(P) con P ausente | sin observación | NO_OBSERVATION | **Coordinator** | ninguna; STOP P-12 | prohibido tomar posesión, reset, borrado o matar procesos ajenos |
| T7 | ORPHAN_CONFIRMED(X) | solicitud de recuperación | decisión del Coordinator + inspección que preserva el trabajo (ramas, commits, árbol) | Coordinator | RECOVERED(X→N) por CAS; contadores copiados de su autoridad | prohibido descartar trabajo; si hay trabajo sin commit inaccesible → T9 |
| T8 | cualquiera | dos solicitudes de recuperación concurrentes | — | — | solo la primera que publica gana el CAS; la segunda relee y ve el estado nuevo | la segunda no repite la transición |
| T9 | cualquiera | cambio de máquina sin acreditar exclusividad ni recuperación | — | Coordinator | ninguna; STOP P-13 | no se reconstruye como existente trabajo local inaccesible |
| T10 | CEDED / HELD | `HEAD` o `origin/<rama>` ≠ SHA registrado | Identity/Remote | — | ninguna; STOP S-13 o 16.7 | rebase solo según 16.7 |
| T11 | cualquiera | registro que declara una transición sin el hecho físico (p. ej., cesión cerrada con el proceso vivo) | contradicción | Coordinator | ninguna; STOP P-02/S-04; corrección por una transición nueva (append-only) | los hechos físicos prevalecen (WORKFLOW §10) |
| T12 | HELD(P) | rebinding de un rol | binding nuevo aceptado | Coordinator | `roles[r]` actualizado por CAS; misma `TaskId` | prohibido abrir una tarea nueva para evadir topes |

**Fallos del mandato** (los que no requieren estado propio):
- **Controller sin contexto:** S-12 → reejecución de fase dentro del tope de BLOCKED.
- **Cuota agotada:** P-06.
- **Autenticación perdida:** S-06.
- **Artefactos transitorios restantes:** se conservan; no se reutilizan (`RunId` nuevo).
- **Trabajo sin commit en el host de la custodia:** se preserva y se inspecciona; nunca `clean`.

**Orden de publicación** (16.4) y conducta ante una caída **antes** o **después** de cada publicación:
- commit sin push → la siguiente sesión ve `HEAD` ≠ remoto: T10;
- push sin handoff: T5;
- handoff sin push: `Identity`, STOP.

### 9.3 Presupuestos y su autoridad

| Contador | Clave | Autoridad | Incremento | Reconciliación |
|---|---|---|---|---|
| `attempts` | unidad | `state/<I>.yml` en `origin/<rama>` (§8 y 16.8) | corrección lanzada (16.8) | la instantánea del binding es copia; si difiere → STOP S-04 |
| por clase | (`TaskId`, `FailureClass`) | verificaciones custodiadas de la cadena | corrección de esa clase | recalculable desde la custodia; ausente o contradictorio → STOP S-04; nunca «el menor» ni cero reconstruido |
| reejecuciones BLOCKED | (`TaskId`, fase) | registros de relevo custodiados | reejecución | ídem; tope 2 (16.8) |
| recuperaciones de 16.7 | `TaskId` | `RebaseMap` custodiados | recuperación | tope 2 |
| invocaciones | plan de gates de la unidad | Freeze (Anexo D para F6) | invocación | tope fijo (P-07) |

Un rebinding conserva `TaskId` y todos los contadores. Una `TaskId` nueva solo por decisión del Coordinator con `analysis.md` que la relacione con la
anterior.

## 10. Fuentes de introspección (D-11)

**Niveles de garantía:**
- **REQUESTED**: lo pedido;
- **CONFIGURED**: el ajuste persistido;
- **RUNTIME_OBSERVED**: metadato que escribe el runtime, ligado a sesión, turno o invocación e instante;
- **SERVICE_ATTESTED**: atestación del servicio; adicional, no exigida.

La autodeclaración del modelo **no** es fuente. Cada observación se liga a unidad, rol, `SessionRef` o `InvocationRef`, runtime/versión, instante,
procedencia y nivel (Anexo B.4). **Nivel mínimo para los requisitos obligatorios: RUNTIME_OBSERVED.** Si la fuente no lo alcanza → UNKNOWN. Si solo hay
REQUESTED, no se presenta como ejecución efectiva.

| Adapter | Fuente | Nivel máximo conocido | Límite |
|---|---|---|---|
| `claude-desktop-session` | `get_session` (`model`, `effort`, `sessionId`) | RUNTIME_OBSERVED (por verificar en F2) | configuración aplicada por el cliente; backend no observado |
| `claude-subagent` | transcripción (`"model"`, `"effort"`) | RUNTIME_OBSERVED | prompt enmarcado por el harness |
| `codex-cli` | registro de sesión (`session_meta`, `turn_context`) sin `--ephemeral` | RUNTIME_OBSERVED | binario actual sin medir |
| `codex-desktop-session` | `turn_context.model/effort` (SP-3) | RUNTIME_OBSERVED, **candidata** | ligar a unidad y rol; no generalizar el formato |
| `claude-cli` | — | UNKNOWN | NOT_AUTHENTICATED |

## 11. Independencia por riesgo (D-12)

### 11.1 Dimensiones y valores

La independencia se mide **respecto de un actor de referencia** (el autor, Worker o rol cuya decisión se revisa), en cuatro dimensiones:

| Dimensión | Se satisface cuando … | Evidencia que la acredita |
|---|---|---|
| **Actor** | el revisor o verificador tiene otro binding (`BindingId` distinto) que la referencia | registros de binding |
| **Sesión** | `SessionRef` distinta: otra sesión de runtime, no un subagente de la misma sesión | `SessionRef` RUNTIME_OBSERVED de ambos |
| **Contexto** | las entradas del revisor son **solo artefactos canónicos** enumerados (prompt con SHA-256 + rutas en un SHA); sin transcripción, memoria ni razonamiento de la referencia | `prompt.md` custodiado + lista de entradas + auditoría de lecturas del runtime si existe (§12) |
| **Proveedor** | `AdapterId` de proveedor distinto según los descriptores | descriptores |

Valores por dimensión: **REQUIRED**, **PREFERRED** o **NOT_REQUIRED**.

Un actor distinto **no** acredita contexto. Un proveedor distinto **no** acredita contexto si recibió el contexto de la referencia. Una marca no se prohíbe
si la independencia se acredita.

### 11.2 Combinación y satisfacción

- **Requisitos concurrentes:** por cada (actor de referencia, dimensión) se toma el **máximo** (REQUIRED > PREFERRED > NOT_REQUIRED). Las referencias
  distintas se evalúan por separado.
- **Satisfacción:** REQUIRED exige evidencia acreditada. PREFERRED no satisfecho se registra con su razón y no bloquea. UNKNOWN en una dimensión REQUIRED
  no satisface.
- **REQUIRED no satisfecho:** el binding del revisor o verificador **no se acepta**. La revisión exigida **sigue pendiente**: no se retira por falta de
  capacidad y la operación que depende de ella se bloquea con su causa.
- La verificación exclusiva del Controller (16.9) y la autoridad del Architect no cambian.

### 11.3 Tabla de disparadores (dominio de §16)

| Disparador (mandato) | Rol o revisión | Referencia | Actor | Sesión | Contexto | Proveedor | Si falta REQUIRED |
|---|---|---|---|---|---|---|---|
| Toda delegación | EXECUTION_CONTROLLER (verificación) | WORKER | REQUIRED | REQUIRED | REQUIRED | NOT_REQUIRED | sin VERIFIED posible: BLOCKED (planificación) |
| Coordinator y Worker en la misma sesión | EXECUTION_CONTROLLER | sesión Coordinator/Worker | REQUIRED | REQUIRED | REQUIRED | PREFERRED | ídem |
| Cambio de autoridad compartida | EXECUTION_CONTROLLER + REVIEWER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | revisión pendiente; no se cierra la tarea |
| Operación destructiva o irreversible | REVIEWER de la autorización (además de S-05) | quien la propone | REQUIRED | REQUIRED | REQUIRED | NOT_REQUIRED | no se ejecuta |
| Cambio sensible a la seguridad | REVIEWER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | ídem |
| Evidencia ambigua de alto coste | REVIEWER | autor de la evidencia | REQUIRED | PREFERRED | REQUIRED | PREFERRED | la evidencia no se usa para decidir |
| Alto coste de fallo (dimensión `High`) | EXECUTION_CONTROLLER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | ídem a toda delegación, con el proveedor registrado |
| Acumulación de roles (§2) | según §2 | — | — | — | — | — | combinación prohibida → binding rechazado |
| Cableado rutinario acotado | REVIEWER (si se pide) | WORKER | NOT_REQUIRED | NOT_REQUIRED | NOT_REQUIRED | NOT_REQUIRED | — |

### 11.4 Revisiones de LIFECYCLE: **BLOCKED — OWNER DECISION (OD-6)**

El mandato pide evaluar NEW ARCHITECTURE, FOUNDATION EVOLUTION y Candidate/READY. LIFECYCLE §5 admite hoy SAME-SESSION ROLE, SEPARATE SESSION o
EXTERNAL HUMAN, con la declaración del modo. Exigir independencia para esas revisiones **cambia un criterio de LIFECYCLE**, materia reservada.

| Alternativa | Predicado para las revisiones de diseño NEW ARCHITECTURE / FOUNDATION EVOLUTION y READY-06 | Efecto |
|---|---|---|
| **1 (recomendada)** | Contexto **REQUIRED** respecto del autor; Sesión **REQUIRED**; Proveedor **PREFERRED**; Actor REQUIRED | SAME-SESSION ROLE deja de satisfacer esas revisiones; coincide con la práctica de I-62 (Coordinator en otra sesión); puede bloquear si no hay revisor disponible |
| 2 | Sin cambio de LIFECYCLE: el modo se declara y Contexto y Proveedor quedan **PREFERRED** | no bloquea; independencia de las revisiones mayores no garantizada |

El Freeze requiere esta decisión: es un elemento congelable de autoridad. **Bloquea el acuerdo y el Freeze de F0, no la redacción.** Con la alternativa 1,
LIFECYCLE recibe el predicado y el contrato lo transporta. El router nunca lo rebaja.

## 12. Pilotos A/B y portabilidad (D-17)

Especificación completa en el **Anexo D**: roles por topología, repositorios y CI, pasos finitos, oráculos, topes, resultados (PASS, FAIL, UNVERIFIED o
UNSUPPORTED), aislamiento verificable y matriz de OD por runtime. Principios:
- los pilotos son **sistema bajo prueba** en el plano (c) de §15;
- **FAIL** (violación observada) ≠ limitación;
- el oráculo se custodia **fuera del host** antes de que B empiece;
- el aislamiento se verifica **auditando las lecturas registradas** del runtime de B, no por el `cwd`.

## 13. Semántica de fallo (D-13)

Los STOP de I-61 se conservan. Ids nuevos (numeración final en el Freeze):

| Id | Condición | Comportamiento |
|---|---|---|
| P-09 | Principal en BELOW_REQUIRED al abrir | STOP antes de trabajo sustantivo |
| P-10 | requisito obligatorio de un rol en UNKNOWN o BELOW_REQUIRED | no se vincula ni se invoca; no consume `attempts` |
| P-11 | huella declarada cambiada durante una cesión | STOP (P-01 para Codex sigue igual) |
| P-12 | TERMINATION_UNACCREDITED u ORPHAN_CONFIRMED sin decisión de custodia | STOP; sin toma de posesión |
| P-13 | cambio de máquina sin acreditar exclusividad ni recuperación | STOP de la operación |
| P-14 | adapter desconocido, esquema ausente, versión incompatible o hechos inválidos | no elegible |
| P-15 | `ProtocolSet` del contrato ≠ protocolo registrado de la unidad | STOP (como P-03) |
| P-16 | un resultado del plano (c) intenta actuar sobre el plano (a) | rechazo; sin efecto real (§15) |

## 14. Versiones, compatibilidad y adopción por unidad (D-14, D-18)

**Protocolo frente a esquema.** Un **protocolo** es un conjunto fijo de esquemas con reglas. `I61` = los cinco `/v1`. `I62` = los esquemas del Anexo B.1,
incluido `rackcad-worker-handoff/v1`, cuya **compatibilidad semántica** con `I62` se demuestra en el Anexo B.6 (correlaciones). `/v1` no se modifica.

**Punto efectivo.** `I62_EFFECTIVE_SHA`:
- es el primer merge, en orden first-parent de `origin/main`, cuyo segundo padre alcanza el commit único con el trailer `Agent-Protocol-Normative: I-62`
  y cuyo primer padre no lo alcanza (análogo a WORKFLOW §11.2);
- entra en vigor al aceptarse el push de `main` que lo publica;
- no hay activación parcial.

**Clasificación por unidad.** Se registra en el bootstrap (`state.protocol`, Anexo B.8) y en la evidencia:
- `I62` si y solo si la base del reclamo (`origin/main` en el reclamo) contiene `I62_EFFECTIVE_SHA`;
- `I61` en cualquier otro caso, incluida una duda de orden: conservador, sin default `I62`.

La clasificación es durable y **nunca se rederiva** por ascendencia posterior (como WORKFLOW §11.1).

**Recorrido completo:**
- una unidad `I61` (I-52, I-63, I-64 y cualquier otra reclamada antes) usa `I61` en **todas** sus cadenas, también en las que abra después de integrar I-62,
  hasta su cierre;
- tras la integración, lee las cláusulas de §16 modificadas por I-62 en `I62_EFFECTIVE_SHA^1`, como WORKFLOW §11.3 para V1;
- no hay migración, ni voluntaria ni automática, y esta Proposal no crea excepción;
- I-62 es `I61` en sus operaciones reales.

**Comprobación:** A6' exige `ProtocolSet` = protocolo de la unidad (P-15). **Control MC-10:** una unidad `I61` abre una cadena tras la integración con un
contrato `I62` → rechazo; con `I61` → aceptada bajo las reglas de I-61 leídas en `I62_EFFECTIVE_SHA^1`. Los artefactos históricos se interpretan por su
`Schema`. Consumidores y superficies se revalidan en READY-04 y antes de integrar.

## 15. Planos de operación, MaterializationClose y AuthorityRevision (D-15)

| Plano | Qué incluye | Autoridad que lo gobierna | Puede | Nunca puede |
|---|---|---|---|---|
| **(a) Real** | la sesión responsable de I-62 y la gobernanza: commits en la rama de I-62, CI, decisiones del Coordinator y del Owner, gates reales | I-61 y Workflow vigentes | desarrollar y materializar I-62; supervisar y custodiar la evidencia de los pilotos | usar reglas de I-62 sin integrar como autoridad; ejercer la ampliación de dominio antes de la vigencia |
| **(b) Materializado inactivo** | los textos y contratos nuevos en la rama de I-62 (F1-F4), con una cláusula de vigencia «desde `I62_EFFECTIVE_SHA`» | ninguna hasta la vigencia | ser revisados, probados con guardas estructurales y copiados al fixture | gobernar ninguna operación real |
| **(c) Sistema bajo prueba** | participantes, decisiones y artefactos dentro del fixture (Anexo D) | las reglas de (b) **copiadas** al fixture, solo dentro del fixture | producir evidencia de conducta del protocolo | acreditar GATE PASS, READY, aprobación del Owner o integración de una unidad real; cambiar `main`, contadores, decisiones o custodia reales (P-16) |

La autoverificación que se ensaye en (c) no da autoridad en (a). En (a), la sesión puede seguir aplicando como **práctica interina, no normativa** la
autoverificación del seguimiento de I-61, registrada solo como observación. Las declaraciones y la evidencia de (c) se custodian aparte (Anexo D.6).

**MaterializationClose y AuthorityRevision por repositorio:**

| Lado | MaterializationClose | AuthorityRevision |
|---|---|---|
| Repositorio real (I-62) | **MC_I62**: el commit que cierra el gate con el **último** cambio a superficies normativas o de plantilla (F4 en el plan). Cualquier cambio posterior a esas superficies **invalida** MC_I62: hay que cerrarlo de nuevo con revisión, resembrar el fixture y repetir los pilotos cuyas autoridades cambiaron | **no se usa**: I-62 no delega trabajo real (F1-F4 son trabajo directo). No se fija `G2CloseSha` ni `AuthorityRevision` por analogía. La restricción de 16.3 queda registrada |
| Fixture | **FIXTURE_BASE**: el commit del fixture con la copia **byte a byte** de las superficies de (b) en MC_I62 (manifiesto ruta → blob en el repositorio real) y las autoridades externas copiadas | dentro del fixture: FIXTURE_BASE o un commit posterior según la regla de 16.3 aplicada **al fixture**; `UNIT_CHANGE` = secciones copiadas de I-62; `EXTERNAL` = copias del fixture |

No se asigna ningún SHA futuro.

**OD-5, permiso exacto de ensayo.** La autorización cubre:
- invocaciones de las celdas del Anexo D, con sus topes, **solo** sobre el fixture;
- las salvaguardas del host de 16.4 (procesos, huella antes y después, cesión del worktree del fixture);
- sin escritura en el repositorio real salvo la evidencia que custodia la sesión de (a);
- consumo solo en celdas con consumo cubierto.

**Clasificación propuesta:**
- **A:** los ensayos **no** son ejecución delegada de §16, porque no hay trabajo de gate de una unidad real. No requieren excepción: los autoriza el
  Coordinator dentro del plan congelado de F6, y el Owner los componentes reservados (OD-2, OD-3, OD-4, OD-7 y abrir sesiones del sistema bajo prueba).
- **B:** si la autoridad de AUTOMATION_PLAN entendiera que §16 gobierna **toda** invocación de un participante, haría falta una **excepción**:
  - cláusula anterior: §16, preámbulo («Ninguna otra iniciativa lo adopta por estar escrito») y 16.3 (`AuthorityRevision` solo definida para I-61);
  - delta: permitir los ensayos del fixture con `AuthorityRevision` = FIXTURE_BASE;
  - alcance: F6 de I-62;
  - autoridad: Owner.

OD-5 = el Owner confirma A o concede B. **Bloquea F6, no el Freeze.**

**OD-1** (aceptar el ADR sucesor) puede seguir pendiente mientras solo se materializa (b). Bloquea READY-03 y la vigencia; no da permiso de ejercer la
ampliación antes.

## 16. Invariantes → pruebas y controles (resumen; detalle en el Anexo C)

**Clases de evidencia:**
- **G**: guarda estructural Core, con mutation check en memoria que demuestra que el oráculo puede fallar;
- **RG**: RED→GREEN de una guarda nueva, con el commit RED antes del cambio;
- **MC**: control manual reproducible, con procedimiento, entrada y esperado independiente en el Freeze;
- **FX**: control en el fixture (plano c);
- **OV**: validación del Owner.

Level A no tiene un componente ejecutable de agregación ni de binding: su conducta se ejerce en MC y FX con oráculos independientes. Las guardas G no se
presentan como prueba de conducta.

| Id | Invariante | Clases |
|---|---|---|
| INV-I62-01 | roles sin proveedor | G (OBL-01), FX-01 |
| INV-I62-02 | UNKNOWN nunca → MATCH; BELOW_REQUIRED domina; todas las causas | MC-01, FX-01 |
| INV-I62-03 | esquemas neutrales; hechos validados por su esquema | G (OBL-03), MC-03 |
| INV-I62-04 | sin binding con un obligatorio no acreditado | MC-04, FX-02 |
| INV-I62-05 | custodia por CAS y transferencia explícita | MC-07, FX-02 |
| INV-I62-06 | contadores con autoridad y sin reinicio | MC-08, FX-02 |
| INV-I62-07 | sin toma de posesión sin terminación acreditada | MC-07, FX-03 |
| INV-I62-08 | sin secretos (extracción mínima y saneamiento) | MC-09, G (OBL-07) |
| INV-I62-09 | `/v1` intacto y compatible | G (OBL-09), MC-10 |
| INV-I62-10 | independencia exigida no rebajada | MC-06, FX-02 |
| INV-I62-11 | preflight con fuente, instante y disposición | G (OBL-05), MC-02 |
| INV-I62-12 | adapters con las 9 operaciones declaradas | G (OBL-06), MC-03 |
| INV-I62-13 | adopción por unidad, sin migración | MC-10 |
| INV-I62-14 | el plano (c) no actúa sobre el (a) | FX-05 |
| INV-I62-15 | reconstrucción por otro Principal sin memoria privada | FX-04, OV-I62-05 |

## 17. Plan de gates por resultado observable (D-16)

**Decisión sobre Level A, antes del Freeze:** **Level A**. Lo que motivó Level B se cubre con **procedimiento versionado**:
- relectura y atribución de procesos, y filtros: fragmentos de PowerShell en el README, como ya hace §3.2;
- coherencia de la verificación: regla del README §8;
- CAS por Git (§8).

F4 lo **verifica** (MC-07, MC-08 y el MC de procesos del Anexo C). Si un control falla, es un hallazgo que vuelve por **A-n** con sus autoridades
(Coordinator + Architect; Owner si cambia el alcance), no por una decisión retroactiva. No se planifica F5.

| Gate | Resultado observable | Evidencia | Dependencias / bloqueos |
|---|---|---|---|
| **F0** | Proposal acordada (Coordinator + Architect, misma versión exacta) y congelada | revisiones; acuerdo con commit, ruta y blob | **OD-6 decidida** |
| **F1** | ADR sucesor **formal** creado como «propuesto», con número asignado en ese momento e índice ADR (ventana con I-52 sobre `docs/adr/README.md`). Materialización (b) de roles, perfil, `CONFIGURATION_STATUS`, autoverificación, renderizado por perfil y corrección de PROMPT_TEMPLATES §2. Guardas OBL-01 y OBL-02 en RG | RED→GREEN; Core Full al cierre; MC-01 | OD-1 no bloquea (el ADR existe como propuesto) |
| **F2** | Preflight de núcleo + descriptores y esquemas de hechos de los cinco adapters iniciales; conjunto `I62` (Anexo B); huellas aceptadas por el Coordinator | MC-02, MC-03, MC-09; preflight real del host con UNVERIFIED honesto (plano a: observación, no autoridad) | OD-2 solo si se mide **invocando** `codex-cli` |
| **F3** | Binding de todos los roles e independencia (§11.3; §11.4 según OD-6); A1'-A8' y las 14 comprobaciones con su delta (Anexo B.7) | MC-04, MC-05, MC-06 | — |
| **F4** | Custodia por unidad (state/v2), transiciones T1-T12, presupuestos, adopción por unidad y planos; **cierre = MC_I62** | MC-07, MC-08, MC-10 y MC de procesos (positivo y negativo, sin invocar modelos) | — |
| **F5** | **no planificado** | — | — |
| **F6** | Fixture sembrado en MC_I62; topologías A/B y portabilidad (Anexo D) | FX-01..FX-05 | OD-5; OD-2 (toda invocación Codex); OD-3, OD-4 (B); OD-7 (CI del fixture) |
| **F7** | Preparación del cierre **sin cambios normativos** (un cambio normativo invalidaría MC_I62): borrador factual de la entrada de FOUNDATIONS en la evidencia (WORKFLOW §11.4), ideas-futuras y paquete de OV | revisión del Coordinator | no depende de READY-06 |
| **READY-01..09** | LIFECYCLE §8, en orden; READY-06 = conformidad Architect + Coordinator | — | OD-1 aceptada antes de READY-03 |
| **FINAL_CANDIDATE_SHA** | Full, CI exacta, OV final (§18) sobre el Candidato | AGENTS | — |
| **Cierre documental** | publicación de la entrada de FOUNDATIONS, índice ADR, HANDOFF, ROADMAP (con ventana) | WORKFLOW §11.4-11.5 | — |

## 18. Owner Validation y decisiones del Owner

**OV, por escenario.** Un ensayo no sustituye a la ejecución final. La asignación de todos los escenarios es la unidad I-62; no requieren AutoCAD.

| Id | Escenario | Lo prepara | Ensayo y evidencia | Ejecución final sobre FINAL_CANDIDATE_SHA |
|---|---|---|---|---|
| OV-I62-01 | Autoverificación del Principal: STOP con effort inferior; MATCH con el requerido | F1 | FX-01 (F6) | el Owner abre una sesión del sistema bajo prueba en el fixture resembrado desde el Candidato, con effort inferior y después correcto |
| OV-I62-02 | Preflight del host: cada runtime con su estado real, sin secretos | F2 | MC-02 (F2) | el Owner revisa el preflight producido con el procedimiento del Candidato |
| OV-I62-03 | Topología A (Anexo D.2) | F6 | FX-02 | ejecución compacta (D.5) sobre el fixture resembrado desde el Candidato |
| OV-I62-04 | Topología B o su limitación registrada | F6 | FX-03 | ídem si las OD están concedidas; si no, el Owner decide sobre la **limitación** (el escenario no se retira) |
| OV-I62-05 | Portabilidad en al menos un sentido, con oráculo aislado | F6 | FX-04 | ejecución compacta sobre el Candidato |

**Decisiones del Owner** (OWNER-RESERVED), cada una con su frontera:

| Id | Decisión | Bloquea | Momento |
|---|---|---|---|
| **OD-6** | Predicado de independencia de LIFECYCLE (§11.4) | **acuerdo y Freeze de F0** | **ahora: BLOCKED — OWNER DECISION** |
| OD-1 | Aceptar el ADR sucesor (ampliación de §16, fila de WORKFLOW §10 y, con OD-6 alternativa 1, el predicado de LIFECYCLE) | READY-03 y vigencia | antes de READY-03 |
| OD-2 | Línea base de huella por adapter. Hoy para `config.toml`: `37DD3559…` registrada frente a `42E15A03…` observada; se identifica el archivo y su alcance | toda invocación de un adapter con esa huella (`codex-cli` en A y B; `codex-desktop-session` si la comparte) | antes de la primera invocación afectada |
| OD-3 | Autenticar Claude CLI | `claude-cli` en B | antes de F6 B |
| OD-4 | Sandbox de Codex para escritura (distinto de la autenticación) | Workers Codex en B | antes de F6 B |
| OD-5 | Permiso exacto de ensayo (§15: A o B) y abrir las sesiones del sistema bajo prueba | F6 | antes de F6 |
| OD-7 | Remoto del fixture con CI (infraestructura y cuenta del Owner) | las comprobaciones `Ci` de los pilotos | antes de F6; si se deniega, `Ci` queda UNVERIFIED (Anexo D.3) |

Cada solicitud se formula aparte, con la ruta y el hash actuales, el efecto, el consumo, el alcance y las restricciones. El silencio no es decisión.

## 19. Riesgos y retos del Architect

Los 12 retos del mandato, con la sección que los trata y el riesgo residual: [paquete V2](I-62-architect-package-v2.md) §5. Riesgo principal: el número de
contratos nuevos. Mitigación: un protocolo por unidad, `worker-handoff/v1` sin cambios, validación con una herramienta existente y ningún motor.

---

## Anexo A — Borrador del ADR sucesor PARCIAL de ADR-0046 (sin número; no es ADR formal)

- **Título:** «Ejecución delegada portable: roles independientes del proveedor, binding por capacidad acreditada y autoverificación del Principal».
- **Estado:** borrador. Sin número; no edita ADR-0046 ni el índice. El ADR formal se crea en F1 como **propuesto** (WORKFLOW §11.4: el ADR nace antes de
  implementar su decisión). La aceptación es del Owner (OD-1).

**Conserva de ADR-0046:**
- **#2:** declaraciones exclusivas; solo el Coordinator declara GATE PASS;
- **#3:** relevo, un participante a la vez; el paquete no amplía el contrato;
- **#4:** routing estable y catálogo mutable; **elegibilidad por invocación medida, consumo cubierto y frescura**;
- **#5:** esquemas estrictos, versionados por protocolo;
- **#6:** verificación fail-closed y multiseñal, 14 comprobaciones con el delta del Anexo B.7;
- **#7:** presupuesto único sin reinicios, con la autoridad de cada contador (§9.3);
- **#8:** transporte;
- **#9:** Level A.

**Supera:**
- el proveedor fijo del Controller (Alternativas) y el título «Controller Codex» → binding por capacidad acreditada;
- la «independencia parcial» como coste aceptado → política por dimensiones (§11).

**Amplía #1 (OWN-L):** obligaciones de la sesión principal **fuera** de una delegación (autoverificación, preflight, custodia operativa); fila de WORKFLOW
§10. Con OD-6 alternativa 1, además, el predicado de independencia de LIFECYCLE §5/§9. Es materia **OWNER-RESERVED**.

**Vigencia y adopción:**
- vigencia: desde `I62_EFFECTIVE_SHA`;
- adopción por unidad según la base del reclamo;
- las unidades `I61` terminan con I-61;
- `/v1` no se retira.

**Consecuencias:**
- más contratos y su mantenimiento;
- revalidación por cambio de runtime;
- topologías que pueden quedar UNVERIFIED por precondiciones del Owner;
- con OD-6 alternativa 1, revisiones mayores que requieren otra sesión.

## Anexo B — Contratos de datos (DISEÑO; no son esquemas operativos)

### B.1 Conjunto de protocolo `I62`

| Esquema | Estado frente a `/v1` | Productor | Validación de forma (`Test-Json`) | Comprobaciones entre artefactos |
|---|---|---|---|---|
| `rackcad-gate-contract/v2` | evoluciona | Coordinator | sesión, al emitir | A2', A6' |
| `rackcad-delegation/v2` | evoluciona | Controller (planificación) | sesión (A1') | A2'-A8' (Coordinator) |
| `rackcad-worker-handoff/v1` | **sin cambios** | Worker | Controller (`Handoff`) | B.6 |
| `rackcad-controller-verification/v2` | evoluciona | Controller | sesión (regla del README §8 + `Test-Json`) | Coordinator |
| `rackcad-relay-record/v2` | evoluciona | sesión | sesión | Controller (`Termination`, `Remote`, `Routing`, `Denials`) |
| `rackcad-preflight/v1` | nuevo | sesión | sesión (dos validaciones, §7.4) | aceptante (A7') |
| `rackcad-binding/v1` | nuevo | sesión (propuesta) | sesión | Coordinator (acepta) |
| `rackcad-adapter-<id>-facts/v<n>` | nuevo, por adapter | adapter (vía sesión) | sesión | — |
| `rackcad-automation-state/v2` | evoluciona (formato de §8) | sesión | Core (guarda de forma) | CAS (§8) |

Reglas comunes: JSON Schema 2020-12; `additionalProperties: false` en todos los objetos; **todos los campos declarados son obligatorios** (estilo de
I-61).

| Valor | Significado |
|---|---|
| ausencia de un campo | inválido |
| `null` | «no aplica», **solo** donde la tabla lo admite |
| UNKNOWN | valor de enum de estado; **nunca** `null` |
| SHA | `^[0-9a-f]{40}$` |
| SHA-256 | `^[0-9a-f]{64}$`; minúsculas en artefactos `I62` (corrige la deuda de mayúsculas de I-61) |
| instantes | ISO-8601 UTC con sufijo `Z` |

### B.2 Identidades

| Identidad | Forma | Resolución y vigencia |
|---|---|---|
| `UnitId` | `^I-[0-9]+[A-Z]?$` | la unidad del contrato |
| `RunId` | patrón de I-61 | lo asigna la sesión; único por invocación |
| `BindingId` | `^B[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` | lo asigna la sesión |
| `BindingRef` | `{UnitId, TaskId, Role, BindingId, Sha256}` | resuelve a `docs/automation/evidence/<UnitId>-agent/<TaskId>/bindings/<BindingId>.json` en el commit de custodia. `Sha256` del archivo y `UnitId`/`TaskId` deben coincidir con el contrato (A2'). Otra unidad u otra tarea → rechazo |
| `PreflightRef` | `{PreflightId, Path, Sha256}` | `Path` bajo el directorio del `RunId` o la custodia. Inválido si un invalidador cambió (§6) |
| `SessionRef` | `{AdapterId, SessionId \| null, StartedUtc \| null, Assurance}` | `SessionId` del runtime si existe; si no, `null` con `Assurance` = NONE (y la sesión no acredita la dimensión Sesión) |
| `InvocationRef` | `{AdapterId, RunId}` | — |
| `HostRef` | `{HostIdHash, Os}` | `HostIdHash` = SHA-256 de `hostname` en minúsculas + `\|` + OS. Límite: colisión de nombres de host |
| `RequirementId` | `^[A-Z_]+\.[a-z0-9-]+$` (p. ej., `PRINCIPAL_COORDINATOR.effort-class`) | lista fija por perfil en `routing.md` |
| `AdapterId` | `^[a-z0-9-]+$` | debe existir `adapters/<AdapterId>.md` en la `AuthorityRevision` (en el fixture, en FIXTURE_BASE) |
| `AdapterDescriptorRef` | `{AdapterId, Path, Blob}` | blob en la `AuthorityRevision` |
| `AdapterFactsSchemaRef` | `{SchemaId, Path, Blob}` | `SchemaId` = `rackcad-adapter-<AdapterId>-facts/v<n>`, con `n` = la versión vigente declarada por el descriptor; `Path` bajo `docs/automation/agent-execution/schemas/adapters/` |
| `ProtocolSet` | `rackcad-protocol/I61` \| `rackcad-protocol/I62` | enum de **versiones de protocolo**, no de marcas |

### B.3 Validación de `AdapterFacts`

1. **Resolver el descriptor:** `AdapterId` → descriptor en la `AuthorityRevision`. Si no existe → **P-14** (no elegible).
2. **Resolver el esquema:** desde la versión declarada por el descriptor → `SchemaRef`. Si `SchemaRef.Blob` ≠ blob en la `AuthorityRevision`, o el archivo
   no existe → **P-14**.
3. **Validar** con `Test-Json -SchemaFile <Path>` sobre `Facts`. Si no valida (propiedad inesperada, requisito omitido, tipo) → **P-14**.
4. Comprobar `Facts.SchemaId` = `SchemaRef.SchemaId`. Una versión distinta → **P-14**.
5. Registrar las dos validaciones (núcleo y hechos) con la versión de PowerShell y el resultado. Sin ese registro, A1' falla.

**Adapter nuevo de prueba:** solo añade descriptor y esquema de hechos. El núcleo no cambia (control MC-03).

### B.4 `rackcad-preflight/v1`

| Campo | Tipo | Notas |
|---|---|---|
| `Schema` | const | — |
| `PreflightId` | string | patrón `P<…>` |
| `UnitId`, `Role`, `ProtocolSet` | — | — |
| `Host` | `HostRef` | — |
| `ObservedUtc` | instante | — |
| `Session` | `SessionRef` \| `null` | `null` para una celda candidata sin sesión |
| `Adapter` | `{AdapterId, AdapterVersion, DescriptorRef}` | — |
| `CoreFacts` | objeto fijo | `GitVersion`, `RepoPath`, `OriginMainSha`, `HeadSha`, `LsRemoteSha`, `CleanTree`, `GitOperationInProgress`, `OpenDelegations` (entero), `ActiveWriters[]` (`{Role, BindingRef, State}`), `Processes[]` (clasificación de README §3.2 ampliada) |
| `AdapterFacts` | `{SchemaRef, Facts}` | `Facts` = objeto validado por B.3 |
| `Fingerprint` | `{Kind, Sha256, KeyNames[]}` \| `{Kind: "none", AcceptedBy}` | `none` solo con la decisión del Coordinator (§7.3) |
| `Requirements[]` | lista | `{RequirementId, Mandatory, Required, Observed \| null, Source, Assurance (REQUESTED\|CONFIGURED\|RUNTIME_OBSERVED\|SERVICE_ATTESTED\|NONE), ObservedUtc \| null, Status (MATCH\|ABOVE_REQUIRED\|BELOW_REQUIRED\|UNKNOWN), Contradiction (bool), ContradictionEvidence \| null}` |
| `ConfigurationStatus` | enum de los cuatro estados | §4.2 |
| `Causes[]` | lista | `RequirementId` de toda fila obligatoria distinta de MATCH, más las contradicciones |
| `Disposition` | `ELIGIBLE` \| `NOT_ELIGIBLE` \| `STOP` | — |
| `StopConditions[]` | lista | — |
| `Invalidators` | objeto | valores observados de máquina, runtime, binario, credencial (estado), catálogo (blob), binding, SHA y huella, para comparar |

Campos del mandato:

| Campo del mandato | Campo de B.4 |
|---|---|
| `PRINCIPAL_SESSION_PROFILE` | `Role` + perfil del binding |
| `REQUIRED_*` | `Requirements[].Required` |
| `CURRENT_*` | `Requirements[].Observed` + `Session` + `Adapter` |
| `ROUTING_REASON` / `ESCALATION_CONDITIONS` | binding |
| `CONFIGURATION_STATUS` | `ConfigurationStatus` |

### B.5 `rackcad-binding/v1`

| Campo | Tipo |
|---|---|
| `Schema`, `BindingId`, `UnitId`, `TaskId`, `Role`, `Profile`, `ProtocolSet` | — |
| `Cell` | `{CellId (entrada del catálogo), CatalogBlob, AdapterId, Model \| null, EffortSemantic, EffortProvider \| null}` |
| `PreflightRef` | — |
| `Eligibility` | `{MeasuredInvocationRef \| null, ConsumptionCovered (OFFICIAL\|MEASURED\|UNKNOWN), CatalogVerifiedOn, Stale}`. Elegible solo con `MeasuredInvocationRef` ≠ `null`, `ConsumptionCovered` ≠ UNKNOWN y `Stale` = false |
| `Independence` | `{Requirements[] {ReferenceBindingId, Actor, Session, Context, Provider}, Satisfaction[] {ReferenceBindingId, Dimension, Satisfied (true\|false\|UNKNOWN), Evidence}}` |
| `RejectedAlternatives[]` | `{CellId, Reason}` |
| `RoutingReason`, `EscalationConditions` | — |
| `CountersSnapshot` | `{Attempts, PerClass {}, BlockedReruns {}, Recoveries, Invocations}` |
| `Custody` | `{FromBindingId \| null, CessionRunId \| null}` |
| `Acceptance` | `{DecisionRef, Utc} \| null` (sin aceptar = `null` = no vigente) |
| `Disposition` | `ACCEPTED` \| `REJECTED` \| `PENDING` |

### B.6 Correlaciones de `rackcad-worker-handoff/v1` dentro de `I62`

| Campo del handoff | Debe coincidir con | Comprobación |
|---|---|---|
| `TaskId`, `Attempt`, `DelegationRunId` | `delegation/v2` | `Handoff` |
| `RunId` | relevo de trabajo (`relay-record/v2`, `Phase=WORK`) | `Handoff` |
| `BaseSha`, `RedSha`, `CurrentSha`, `Branch`, `Worktree` | delegación y Git | `Identity` |
| `Worker.Provider` | proveedor declarado por el descriptor del `AdapterId` del binding (`delegation.Executor.BindingRef`) | `Handoff` |
| `Worker.ModelRequested` / `EffortRequested` | `delegation.Model` / `Effort.Provider` | `Routing` |
| `Worker.Trailer` | proveedor y modelo del binding | `Trailer` |

El modelo y effort **efectivos** no están en el handoff: los da `relay-record/v2.Participant.Observation[]`. **Conclusión:** `/v1` lleva todas las claves
necesarias para correlacionar. No hace falta `/v2` para el handoff.

### B.7 Deltas: `relay-record/v2`, `controller-verification/v2`, `gate-contract/v2`, `delegation/v2`, A1'-A8' y las 14 comprobaciones

| Artefacto | Conservado de `/v1` | Reemplazado | Añadido | Retirado |
|---|---|---|---|---|
| `relay-record/v2` | `TaskId`, `RunId`, `Attempt`, `Phase`, `Cession`, `Outcome`, `Disposition`, `Notes`; `RemoteFacts` | `Participant.Kind` → `{Role, BindingRef, AdapterId, AdapterVersion, Transport (session-internal\|external-process), Observation[] {Source, Assurance, Model, Effort, ObservedUtc}}`; `Exit`/`Entry`: `ConfigToml*` → `Fingerprint` (§7.3); `RebaseMap.G2CloseSha` → `MaterializationCloseSha` | `Host`; `TestArtifacts[].Skipped` | — |
| `controller-verification/v2` | todo | — | `Verifier {Role, BindingRef}` | — |
| `gate-contract/v2` | todo | `EligibleCells` → `RoleRequirements[] {Role, Profile, Mandatory[], Independence[], EligibleCells[]}` | `ProtocolSet`; `MaterializationCloseSha` | — |
| `delegation/v2` | todo salvo `Owner.Kind` | `Owner.Kind` → `session-internal` \| `external-process` | `Executor.BindingRef`; `Independence` | — |

| Comprobación | Delta semántico |
|---|---|
| A1' | valida contra el **conjunto `I62`** y, además, `AdapterFacts` (B.3) |
| A2' | `BindingRef` resuelve a la misma unidad y tarea |
| A3-A5 | sin cambio, más `RoleRequirements` ⊆ |
| A6' | `ProtocolSet` = protocolo de la unidad (P-15) |
| A7' | el binding aceptado tiene `Eligibility` válida y el preflight vigente (sin invalidadores) |
| A8' | `CountersSnapshot` = autoridad (§9.3) |
| `Termination` | operación 7 del adapter |
| `Handoff` | más B.6 |
| `Authority` | `AuthorityRevision` con MaterializationClose |
| `Contract` | más `RoleRequirements` e `Independence` |
| `Identity`, `Remote`, `Scope`, `CleanTree` | sin cambio (más `Host` en `Identity`) |
| `Ci` / `Tests` | más `Skipped` coherente |
| `Trailer` | coherente con el binding |
| `Routing` | efectivo = solicitado con una `Observation` de nivel ≥ RUNTIME_OBSERVED; si no, **no pasa** (antes: «efectivo» sin nivel declarado) |
| `FreeText` | sin cambio |
| `Denials` | más pérdida de autenticación (S-06) |

### B.8 `rackcad-automation-state/v2` (formato de AUTOMATION_PLAN §8)

| Campo | Notas |
|---|---|
| `schema: rackcad-automation-state/v2` | — |
| `automation_state` | los campos de `/v1` sin cambio de significado |
| `protocol` | `I61` \| `I62`, fijado en el bootstrap; inmutable (§14) |
| `custody.record_version` | entero ≥ 0, monótono |
| `custody.roles[]` | `{role, binding_ref \| null, holder {declared, observed {source, utc} \| null, authorized {decision_ref} \| null}, state (HELD\|CEDED\|RECOVERED)}` |
| `custody.branch_owner` / `custody.worktree_owner` | `{declared, observed \| null}` |
| `custody.write_scopes[]` | `{scope, owner_binding_ref}` |
| `custody.open_delegation` | `RunId` \| `null` |
| `custody.last_verified_sha` | SHA \| `null` |
| `custody.next_authorized_action` | `{text, decision_ref}` |
| `custody.shared_commitments[]` | `{surface, issuer, scope, ack_ref, release_ref \| null, correlation_id, valid_until_event}` |

## Anexo C — Obligaciones de prueba y control

Esperado **independiente** = fuente fijada antes de ejecutar: tabla de §4.2, Anexo B, reglas de I-61, oráculo del Coordinator (Anexo D). Nunca la salida
del procedimiento bajo prueba.

| Id | Entrada | Componente / procedimiento real | Acción | Observable | Esperado (fuente) | Fallo legítimo | Gate | Clase |
|---|---|---|---|---|---|---|---|---|
| OBL-01 | tabla de roles de §16.1 y enums del núcleo | `AgentExecutionProtocolTests` (nueva guarda) | buscar marcas (proveedores de los descriptores + patrones) | coincidencias | ninguna (§2) | mutación que inserta «Codex» en un rol | F1 | G + RG |
| OBL-02 | tabla normativa de `CONFIGURATION_STATUS` en §16 y tabla E1-E10 del README | guarda Core | comparar fila a fila las dos tablas con la del Freeze (§4.2) | diferencias | 0 | fila alterada en memoria | F1 | G (consistencia; **no** prueba la conducta) |
| MC-01 | los diez casos E1-E10 como preflights sintéticos (B.4) | el procedimiento de agregación del README, aplicado por la sesión | agregar y disponer | `ConfigurationStatus`, `Causes`, `Disposition` | §4.2 (Freeze) | E5 con una sola causa; E6 sin S-04 | F1 | MC |
| OBL-03 | esquemas del conjunto `I62` | guarda Core | ningún enum con marca; `AdapterFacts.SchemaRef` obligatorio; estricto recursivo | violaciones | 0 | enum con `codex-cli`; `Facts` sin `SchemaRef` | F2 | G + RG |
| MC-02 | el host real | procedimiento de preflight (núcleo + adapter) | ejecutar por cada adapter | registros `preflight/v1` válidos por `Test-Json` | estados reales; NOT_AUTHENTICATED de `claude-cli` | registro sin `ObservedUtc` → inválido | F2 | MC (+ OV-I62-02) |
| MC-03 | (a) adapter de prueba `test-null` con descriptor y esquema; (b) hechos con propiedad inesperada; (c) `AdapterId` desconocido; (d) `SchemaId` v2 frente a un descriptor v1; (e) huella «ninguna» sin aceptación | B.3 con `Test-Json` | validar | resultado por caso | (a) válido sin cambiar el núcleo; (b)-(e) P-14 / no elegible | (b)-(e) aceptados | F2 | MC |
| MC-09 | registros con valores **ficticios** sensibles (`gho_FAKE0000…`, `sk-ant-FAKE…`, `-----BEGIN FAKE KEY-----`, `password=fake`) en `Value`, `KeyNames`, `Evidence`, `Notes` y mensajes de error | procedimiento de saneamiento previo a la custodia (lista blanca de campos extraídos + patrones) | sanear y custodiar | custodia rechazada o valor redactado | 100 % detectado | un valor ficticio que pase | F2 | MC (+ G OBL-07: sin campos de credencial) |
| MC-04 | binding con un requisito obligatorio UNKNOWN; otro con `MeasuredInvocationRef = null`; otro con `ConsumptionCovered = UNKNOWN` | procedimiento de binding + A7' | proponer y aceptar | `Disposition` | REJECTED en los tres | aceptado | F3 | MC |
| MC-05 | `BindingRef` de otra unidad o tarea; `RunId` de otra corrida | A2' | aceptar | resultado | rechazo | aceptado | F3 | MC |
| MC-06 | (1) dos requisitos simultáneos para un mismo revisor; (2) mismo proveedor con contexto separado; (3) distinto proveedor con contexto compartido; (4) Worker = verificador (misma sesión); (5) preferencia no satisfecha frente a obligación no satisfecha | §11 | evaluar `Satisfaction` | dimensión por dimensión | (1) máximo por dimensión; (2) Contexto sí, Proveedor no; (3) Proveedor sí, Contexto no; (4) rechazo (§2); (5) registro frente a bloqueo | otro resultado | F3 | MC |
| MC-07 | repositorio Git **local desechable** (no el real); transiciones T1-T12 simuladas con commits y dos clones | CAS por Git (§8) + tabla §9.2 | caída antes y después de cada publicación; dos recuperaciones concurrentes; registro con SHA anterior; relevo válido completo | push aceptado o rechazado; estado final | §9.2 (Freeze) | segundo push aceptado; posesión tomada sin terminación | F4 | MC (sin invocar modelos) |
| MC-08 | contador ausente o contradictorio entre el estado y la instantánea; rebinding | §9.3 | reconciliar | disposición | STOP S-04; contadores intactos tras rebinding | «el menor» o cero | F4 | MC |
| MC-procesos | procesos locales ficticios (`pwsh` en la ruta del fixture; proceso efímero) | README §3.2 ampliado (relectura a los 2 s, `conhost`) | atribuir | clasificación | positivo: participante detectado; negativo: efímero no atribuible → relectura → no vivo | falso P-02 o participante omitido | F4 | MC |
| OBL-09 / MC-10 | (a) blobs de los cinco `/v1`; (b) relay-record custodiado de I-61; (c) unidad `I61` con contrato `I62` | guarda Core (a); `Test-Json` con `/v1` (b); A6' (c) | comprobar | igualdad; validez; rechazo | (a) igual; (b) válido; (c) P-15 | cambio en `/v1`; (c) aceptado | F2/F4 | G (a), MC (b, c) |
| OBL-05 / OBL-06 / OBL-07 | esquema `preflight/v1`; descriptores; todos los esquemas | guardas Core | fuente, instante y disposición obligatorias; 9 operaciones declaradas; sin campos de credencial | violaciones | 0 | mutaciones | F2 | G + RG |
| FX-01..FX-05 | Anexo D | sistema bajo prueba | Anexo D | Anexo D | oráculos del Anexo D | Anexo D | F6 | FX |

Las pruebas `/v1` vigentes (`AgentExecutionProtocolTests` OBL-01..06 y OBL-11 de I-61) siguen ejecutándose sin cambios.

## Anexo D — Pilotos y portabilidad (plano c)

### D.1 Repositorios

| Repositorio | Dónde existen sus SHAs y remoto | Corridas | Evidencia |
|---|---|---|---|
| Real (RackCad) | `origin` de GitHub | CI de push de la rama de I-62 (plano a) | `docs/automation/evidence/I-62-pilot/…` (custodia de la sesión de a) |
| Fixture | repositorio Git aparte, fuera del árbol real; remoto **local** (bare) y, con OD-7, un remoto en GitHub del Owner | con OD-7: CI propia del fixture, cuyos jobs requeridos declara su contrato; sin OD-7: ninguna | dentro del fixture + copia custodiada en el real (D.6) |

**CI exigida por el protocolo probado:**
- con OD-7, la comprobación `Ci` se evalúa contra la CI del fixture;
- sin OD-7, `Ci` = `not_run`: ninguna verificación del fixture puede ser VERIFIED, y el resultado de los invariantes que dependen de `Ci` es **UNVERIFIED**
  (causa: OD-7), no PASS.

Un test local nunca se renombra como CI.

### D.2 Roles por topología

| Rol | Topología A | Topología B |
|---|---|---|
| PRINCIPAL_COORDINATOR | `claude-desktop-session` **nueva** abierta por el Owner en el fixture (≠ la sesión real de I-62) | `codex-desktop-session` abierta por el Owner en el fixture |
| WORKER | `claude-subagent` del Principal A | `codex-cli` con escritura (OD-4) |
| EXECUTION_CONTROLLER | `codex-cli` de solo lectura (sesión propia) | `codex-cli` de solo lectura, **otra** invocación y sesión que el Worker (Actor, Sesión y Contexto independientes; mismo proveedor permitido por §11.3) |
| REVIEWER | `claude-subagent` **nuevo**, solo con entradas canónicas (Contexto independiente del Worker) | `claude-cli` (OD-3) |
| ARCHITECT | `codex-cli` (invocación propia, perfil ARCHITECTURE_REVIEW) sobre el contrato del minigate | `claude-cli`, invocación distinta del REVIEWER |
| Coordinator del fixture | el Coordinator real **actuando solo en el plano (c)**, con decisiones marcadas como del sistema bajo prueba | ídem |

Declaraciones: las de §2. Acumulaciones de §2 respetadas: el Worker nunca es el verificador.

### D.3 Escenarios, pasos, oráculos, topes y STOP

**Tarea del minigate:** añadir una función pura y su prueba a una biblioteca .NET mínima del fixture, más una línea de documentación.

| Escenario | Precondiciones | Pasos finitos | Esperado / oráculo | Evidencia | Topes (invocaciones · reintentos · tiempo por invocación) | STOP |
|---|---|---|---|---|---|---|
| **FX-01** autoverificación (A y B) | sesión del sistema bajo prueba abierta | 1. preflight del Principal con effort requerido; 2. el Owner baja el effort; 3. preflight; 4. el Owner lo restaura; 5. preflight | MATCH → BELOW_REQUIRED + STOP P-09 → MATCH (§4.2) | tres `preflight/v1` válidos | 0 invocaciones de modelo · — · — | contradicción S-04 |
| **FX-02** topología A, minigate completo | OD-2 (`codex-cli`), OD-5, OD-7 o su limitación | 1. contrato (`gate-contract/v2`); 2. Architect; 3. planificación; 4. aceptación A1'-A8' + nc4; 5. Worker RED y GREEN; 6. hechos remotos; 7. verificación; 8. Reviewer; 9. nc1-nc3 | VERIFIED sobre el GREEN; nc con el oráculo relativo de I-61; Reviewer con hallazgos o sin ellos; bindings aceptados con independencia satisfecha | todos los artefactos `I62` + bindings + preflights | Codex: planificación 1, verificación 1, Architect 1, nc 3; +2 reejecuciones por fase; **tope 10**; 600 s cada una. Worker: 1 + 1 corrección; 60 min. Reviewer: 1 + 1; 60 min. Sondas previas: ≤ 2 por celda nueva | P-01/P-11, P-02, P-06, P-07 (tope), S-13 |
| **FX-03** topología B, minigate completo | OD-2, OD-3, OD-4, OD-5, OD-7 o su limitación; I-02 acreditada | como FX-02 con los roles de B | ídem | ídem | Codex Worker 1 + 1 (600 s); Codex Controller: planificación 1, verificación 1, nc 3, +2 por fase, tope 8; Claude CLI: Reviewer 1 + 1, Architect 1 + 1, 600 s | ídem + S-06 |
| **FX-04** portabilidad A→B (y B→A si es viable) | FX-02 iniciado; Principal B disponible | 1. A llega hasta Worker GREEN publicado y relevo de trabajo registrado, y se detiene con el Exit; 2. el Coordinator prepara el oráculo **fuera del host** desde el estado canónico del fixture en `S_A` y publica solo su SHA-256 en la evidencia real; 3. B empieza solo con el fixture y produce: (i) los hechos reconstruidos y (ii) **la siguiente decisión** (esperada: «invocar la verificación del Controller para `TaskId`/`DelegationRunId`/`WorkRunId` dados, con `ProtocolSet` `I62`, sin Worker nuevo y con `attempts` sin cambio»); 4. B publica su respuesta en el fixture (`S_B`); 5. el Coordinator entrega el oráculo; 6. comparación mecánica | coincidencia hecho a hecho y en la decisión | `S_A`, `S_B`, hash del oráculo, oráculo, comparación, auditoría de lecturas | B: 1 sesión + ≤ 2 reintentos de transporte | lecturas de B fuera de las rutas permitidas → **ensayo inválido** (FAIL de aislamiento) |
| **FX-05** el plano (c) intenta actuar sobre el (a) | FX-02 en curso | un artefacto del fixture que declara GATE PASS o propone un push al repositorio real | rechazo; `main`, contadores, decisiones y custodia reales intactos (P-16) | comparación del estado real antes y después | 0 invocaciones | — |

**Controles negativos adicionales**, en FX-02 y FX-03, con el oráculo relativo de I-61. Cada uno: 1 invocación + 2 reejecuciones de transporte.

| Control | Esperado |
|---|---|
| handoff ausente | BLOCKED |
| SHA cambiado | STOP |
| credencial sin acreditar | P-10 |
| 0 pruebas seleccionadas | REWORK |
| escritor en TERMINATION_UNACCREDITED | P-12 |
| trabajo sin commit | `CleanTree` REWORK |
| artefacto de otra corrida | `Handoff` STOP |
| contador tras cambiar de proveedor | sin reinicio |
| hecho retirado del estado canónico | **UNKNOWN o STOP**, no el valor oculto |

### D.4 Resultados admitidos

| Resultado | Cuándo | Efecto |
|---|---|---|
| **PASS** | se ejecutó y el oráculo coincide | cuenta |
| **FAIL** | se ejecutó y se observó una violación | se conserva con su evidencia; **impide cerrar F6** hasta corregir (A-n o corrección de I-62) y repetir dentro de los topes. Nunca se convierte en limitación |
| **UNVERIFIED** | falta una precondición (OD pendiente o denegada; capacidad sin medir) | se registra la causa; el escenario no se retira |
| **UNSUPPORTED** | capacidad medida como ausente | ídem, con la medición |

**Cobertura mínima para cerrar F6:**
- FX-01, FX-02, FX-04 (al menos A→B o B→A) y FX-05 con resultado **PASS**;
- FX-03 PASS, o UNVERIFIED/UNSUPPORTED con causa, para la decisión del Owner sobre la limitación en OV-I62-04. El escenario no se retira.

### D.5 Ejecución compacta para OV sobre FINAL_CANDIDATE_SHA

Se resiembra el fixture desde el Candidato y se ejecutan FX-01, FX-02 (pasos 1-7) y FX-04 en un sentido, con los mismos topes. FX-03 solo con las OD
concedidas.

### D.6 Aislamiento verificable y custodia de los dos planos

- B arranca con el fixture como único directorio de trabajo. **Además**, se auditan las lecturas que registre su runtime: comandos en el registro de
  sesión de Codex y llamadas a herramientas en la transcripción de Claude.
- Cualquier lectura fuera de las rutas permitidas (I-62, transcripciones de A, `D:\IDs`, memoria privada) invalida el ensayo.
- Si el runtime no registra lecturas, el aislamiento es **UNVERIFIED** (limitación declarada) y FX-04 no puede ser PASS.
- El oráculo nunca está en el host antes de `S_B`. Su hash prueba integridad, no aislamiento.
- Los artefactos del plano (c) se custodian en `docs/automation/evidence/I-62-pilot/<escenario>/…`, separados de la evidencia del plano (a), con la marca
  «sistema bajo prueba».

### D.7 Decisiones del Owner por runtime

| Runtime | OD-2 huella | OD-3 autenticación | OD-4 sandbox | OD-5 ensayo | OD-7 CI |
|---|---|---|---|---|---|
| `codex-cli` (A: Controller y Architect; B: Controller y Worker) | **sí** | — | sí, solo para escritura (B) | sí | para `Ci` |
| `codex-desktop-session` (B: Principal) | sí, si comparte `config.toml` (se mide en F2) | por medir | — | sí (abrir la sesión) | — |
| `claude-cli` (B: Reviewer y Architect) | según su huella (F2) | **sí** | — | sí | — |
| `claude-desktop-session` y `claude-subagent` (A) | según su huella (F2) | — | — | sí (abrir la sesión) | — |

Cambiar una configuración **no** acredita una capacidad aún no medida.
