# I-62 — Proposal V1: portabilidad del Principal y binding de roles independiente del proveedor

```text
Frozen: NO
Version: V1 (primera Proposal; sin rondas de revisión)
Unit: I-62   Workflow: V2 (T4)   Claim-Id: 5b661a17-8c18-4183-8554-3866059cba2b
Archetype: NEW ARCHITECTURE (decisión del Coordinator C62-F0-09; el mandato conserva la etiqueta FOUNDATION EVOLUTION)
Base: origin/main 819955d6   Discovery: docs/initiatives/I-62-discovery.md, ronda R1 (blob 86f24e65), aceptada como base de diseño (C62-F0-08)
Author: sesión principal responsable de I-62 (Claude), redacción directa (C62-F0-11)
Review: PENDIENTE — Coordinator y Architect; paquete en docs/initiatives/I-62-architect-package-v1.md (revisión no realizada)
IMPLEMENTATION AUTHORIZATION = NO   ·   I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED
```

**Fuentes y precedencia:**
- Alcance: el mandato del Owner ([I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt)), citado por secciones.
- Hechos: el [Discovery](I-62-discovery.md) (R1).
- Decisiones de partida: [decisiones](../automation/decisions/I-62.md) §§7-10 (C62-G0-02, C62-F0-01..12).
- Precedencia: las autoridades integradas (WORKFLOW, AGENTS, LIFECYCLE, AUTOMATION_PLAN §16, ADR-0046 y el Freeze de I-61) prevalecen mientras I-62 no se integre.
- Esta Proposal es **diseño**: no modifica el protocolo operativo, no publica esquemas operativos ni cambia ninguna conducta. Los contratos de datos
  se describen aquí como diseño. **REFERENCE OVER REPETITION:** las reglas de I-61 se citan; no se copian.

## 0. Delta respecto de I-61

| Tema | I-61 integrado (fuente) | I-62 propone |
|---|---|---|
| Roles | Worker (Claude o Codex), Controller (Codex), Coordinator, sesión responsable (§16.1) | cinco roles **sin proveedor** (D-01); PRINCIPAL_COORDINATOR formaliza a la sesión responsable |
| Proveedor del Controller | fijado (ADR-0046, Alternativas) | binding por capacidad acreditada (D-05); ADR sucesor parcial (anexo A) |
| Sesión principal | sin perfil, celda ni obligación (Discovery §2) | perfil, autoverificación y `CONFIGURATION_STATUS` (D-03, D-04) |
| Preflight | solo dentro del relevo y con `config.toml` de Codex (16.4) | núcleo agnóstico + hechos por adapter, antes del binding y en cada relevo (D-06) |
| Recetas y hechos de proveedor | en §16.4, README §§5-6 y `relay-record` (`codex-cli`, `ConfigToml*`) | **frontera de adapters** con descriptor y esquema de hechos por adapter (D-07) |
| Independencia | parcial, aceptada como coste (ADR-0046) | política por riesgo con dueño por dominio (D-12) |
| Custodia | deducida del relevo y del estado | registro durable **por unidad** (D-08) |
| Recuperación | 16.11 para la ejecución delegada | estados de escritor y recuperación sin toma de posesión implícita (D-09) |
| «cierre de G2» | nombre de un gate de I-61 en reglas generales (16.3, 16.5, 16.7) | hito de materialización por unidad (D-15) |
| Esquemas | cinco `rackcad-*/v1` | conjunto de protocolo `/v2` donde cambia el contrato; `/v1` intacto (D-14) |
| Nivel | A | **A** (D-16; F5 no planificado) |

## 1. Objetivo, no-objetivos y criterios de éxito

**Objetivo** (mandato, «GOAL»): que una iniciativa asigne cada rol a un proveedor, modelo o runtime según **capacidad acreditada**, sin supuestos fijos
como «Claude = Principal» o «Codex = Reviewer», y que otro Principal pueda reanudarla sin memoria privada.

**No-objetivos** (mandato, «OUT OF SCOPE» y «PARALLELISM WITH PRODUCT»):
- funcionalidad, UI o lógica de producto;
- reemplazar al Master Orchestrator o quitar autoridad al Coordinator;
- bucles autónomos sin límite, compras o suscripciones, gestión de credenciales o un scheduler en la nube;
- fijar modelos de forma permanente;
- auto-merge;
- reescribir I-61.

Además, **no** se diseña un motor, framework de código, scheduler, servicio de locks ni plataforma (C62-F0-09 §1), y **no** se crea un registro global de
roles, números, turnos o permisos entre unidades (C62-F0-09 §2).

| # | Criterio de éxito (mandato) | Mecanismo | Gate | Evidencia |
|---|---|---|---|---|
| 1 | El rol Principal es independiente del proveedor | D-01, D-03 | F1 | INV-I62-01; OBL-I62-01 |
| 2 | Los roles se reasignan por hechos de capacidad observables | D-05, D-06 | F3 | INV-I62-04; registro de binding del piloto |
| 3 | El Principal verifica si su sesión cumple los requisitos | D-03, D-04, D-11 | F1 | OBL-I62-03; OV-I62-01 |
| 4 | UNKNOWN nunca se convierte silenciosamente en MATCH | D-04 | F1 | INV-I62-02; OBL-I62-02 |
| 5 | La portabilidad del entorno se comprueba en preflight | D-06 | F2 | OBL-I62-05; preflight por runtime |
| 6 | Los esquemas del protocolo son neutrales respecto del proveedor | D-07, D-14 | F2 | INV-I62-03; OBL-I62-04 |
| 7 | Los detalles del proveedor viven en adapters y catálogos | D-07 | F2 | OBL-I62-06 |
| 8 | La independencia se exige según el riesgo | D-12 | F3 | OBL-I62-08; casos en la Proposal |
| 9 | Un segundo Principal reconstruye el estado sin memoria privada | D-08, D-17 | F6 | prueba de portabilidad (§12) |
| 10 | La recuperación de custodia y huérfanos es determinista | D-09 | F4 | INV-I62-07; controles negativos |
| 11 | No entran secretos al repositorio | D-06, D-07 | F2 | INV-I62-08; OBL-I62-07 |
| 12 | Al menos dos topologías probadas, o sus límites registrados honestamente | D-17 | F6 | resultados A/B (§12) |
| 13 | I-61 queda intacto como predecesor | D-14 | todas | INV-I62-09 |
| 14 | Las iniciativas de producto no se bloquean | D-14 (sin migración a mitad de camino) | todas | DC-07 en cada hito |

## 2. Roles (D-01) y acumulación

| Rol | Semántica (estable) | Declara | Nunca declara |
|---|---|---|---|
| **PRINCIPAL_COORDINATOR** | Sesión responsable de la unidad: tiene el worktree, ejecuta el preflight y los relevos, propone bindings, ejecuta el trabajo directo autorizado, registra hechos y custodia | hechos del relevo, del remoto, del rebase y del preflight; `CONFIGURATION_STATUS` propio; disposición de un fallo de transporte | `EXECUTION_*`, GATE PASS (salvo que tenga además el rol Coordinator, declarado SAME-SESSION ROLE) |
| **ARCHITECT** | Revisión de diseño y conformidad según LIFECYCLE §5 | veredicto de revisión de LIFECYCLE | GATE PASS, `EXECUTION_*` |
| **EXECUTION_CONTROLLER** | Planificación y verificación de una delegación, solo lectura (§16) | `EXECUTION_VERIFIED`, `EXECUTION_REWORK_REQUIRED`, `EXECUTION_BLOCKED` | GATE PASS, Candidato, cierre, integración |
| **WORKER** | Escritura dentro del alcance de la delegación | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS |
| **REVIEWER** | Revisión de una entrega, evidencia o decisión operativa fuera de las revisiones de LIFECYCLE, sin autoridad de Architect ni de Controller | hallazgos con severidad y evidencia | `EXECUTION_*`, GATE PASS, veredictos de LIFECYCLE |

El **Coordinator** (autoridad de gate de LIFECYCLE §7) no es un rol vinculable por el protocolo: lo asigna la gobernanza (Owner/Master) y conserva sus
facultades. Una misma sesión puede tener PRINCIPAL_COORDINATOR y Coordinator si se declara SAME-SESSION ROLE, como en I-61.

**Acumulación** (por entrega o decisión concreta):

| Mismo actor en … | Permitido | Condición |
|---|---|---|
| WORKER + EXECUTION_CONTROLLER | **no** | sería autoverificación (se conserva INV-01 de I-61, no autoaprobación; ADR-0046 #2) |
| WORKER + REVIEWER de su propia entrega | **no** como revisión independiente | puede autorrevisarse, pero no cuenta para D-12 |
| PRINCIPAL_COORDINATOR + WORKER (subagente) | sí | alternancia interna de la sesión (§16.4); la verificación sigue siendo del Controller |
| PRINCIPAL_COORDINATOR + REVIEWER / ARCHITECT | sí, como SAME-SESSION ROLE | declarado; nunca cuenta como independiente |
| PRINCIPAL_COORDINATOR + Coordinator | sí, como SAME-SESSION ROLE | precedente I-61 |
| ARCHITECT + EXECUTION_CONTROLLER | sí, si D-12 no exige independencia entre ambos | el veredicto de LIFECYCLE no se desplaza al Controller (C62-F0-09 §3) |
| REVIEWER + EXECUTION_CONTROLLER del mismo trabajo | sí | el REVIEWER no añade independencia si comparte contexto |

## 3. Mapa de autoridad: anterior → delta (asignaciones de diseño; C62-F0-09 §3)

| Dominio | Hoy | Delta propuesto | Autoridad para el cambio |
|---|---|---|---|
| LIFECYCLE §5 | participación y revisión del Architect | **predicado** de cuándo una revisión de diseño, gate, READY o conformidad exige independencia (D-12); el contrato lo transporta | Coordinator + Architect; LIFECYCLE según su autoridad |
| AUTOMATION_PLAN §16 | ejecución delegada (ADR-0046 #1) | roles operativos, binding y aceptación, preflight, **obligación de autoverificación del Principal**, frontera de adapters (contrato), custodia operativa, recuperación delegada y conteo | AUTOMATION_PLAN con sus autoridades; **ampliación de dominio** abajo |
| AUTOMATION_PLAN §8 | formato del estado durable | versión del estado con bloque de custodia por unidad (D-08); guardar un valor no le da autoridad | AUTOMATION_PLAN |
| WORKFLOW §§3-4 | reclamo, worktree, apertura y relevo entre sesiones | solo **referencias**: el paso «al abrir» enlaza la autoverificación de §16; el registro de custodia no adquiere ramas, ventanas ni excepciones al relevo | WORKFLOW |
| `routing.md` / catálogo | selección por capacidades; nombres y fechas en el catálogo | perfil PRINCIPAL_COORDINATION; binding de todos los roles; §7 pasa al adapter; celdas por rol | punto de extensión del Freeze de I-61 §15 |
| PROMPT_TEMPLATES §G y procedimientos de adapters | representación y recetas | renderizado por PromptProfile en el adapter; corrección del bloque factual obsoleto de §2 (C62-F0-10 E) | PROMPT_TEMPLATES |
| AGENTS | evidencia, exact-SHA e invalidadores | sin cambios; se referencia | — |

**Ampliación efectiva del dominio de §16** (C62-F0-09 §3):
- **Fuente anterior:** ADR-0046 #1 limita §16 a la «ejecución delegada bajo orden explícita del Coordinator» y lo refleja la fila de WORKFLOW §10.
- **Delta:** la autoverificación del Principal ocurre al abrir la sesión, **fuera de una delegación**, y la custodia operativa abarca la sesión principal y no
  solo a los participantes delegados.
- **Autoridad reservada:** **Owner**, en la aceptación del ADR sucesor (anexo A) y del texto de la fila de WORKFLOW §10. No se declara aprobación.

## 4. Perfil del Principal y estado de configuración

### 4.1 Perfil (D-03)

`routing.md` recibe la clase **«Coordinación principal»**, con perfil PRINCIPAL_COORDINATION:
- horizonte `High`, que fija Long-horizon;
- ambigüedad, sensibilidad arquitectónica y coste de fallo `High`;
- nivel **Frontera**.

Los nombres de modelo y effort están solo en el catálogo, con su fecha.

| Campo (mandato) | Fuente |
|---|---|
| `PRINCIPAL_SESSION_PROFILE` | `routing.md` |
| `REQUIRED_CAPABILITY_LEVEL` / `REQUIRED_EFFORT_CLASS` | `routing.md` §3 |
| `CURRENT_PROVIDER` / `CURRENT_MODEL` / `CURRENT_EFFORT` | observación del adapter (D-11), con nivel de garantía |
| `ROUTING_REASON` | clasificación, dimensiones, celda y fecha del catálogo |
| `ESCALATION_CONDITIONS` | `routing.md` §4 |
| `CONFIGURATION_STATUS` | D-04 |

Requisitos **obligatorios** del Principal: nivel y effort, acceso al repositorio con escritura, Git y GitHub CLI autenticado para los hechos remotos, y una
fuente de introspección con el nivel de garantía mínimo del perfil (D-11). Métricas opcionales: cuota y consumo observables.

### 4.2 `CONFIGURATION_STATUS` y disposición (D-04, C62-F0-10 A)

Estados exactos: **MATCH**, **ABOVE_REQUIRED**, **BELOW_REQUIRED**, **UNKNOWN**. Se agregan **solo sobre los requisitos obligatorios** del rol:
1. **BELOW_REQUIRED** si alguno es insuficiente;
2. si no, **UNKNOWN** si falta acreditar alguno;
3. si no, **ABOVE_REQUIRED** si alguno lo supera;
4. en otro caso, **MATCH**.

Las métricas opcionales no suben ni bajan el agregado.

**El estado de configuración no es la disposición.** El registro conserva **todas** las causas, no solo la primera fila que decidió:

| Situación | Disposición |
|---|---|
| BELOW_REQUIRED (Principal) | STOP antes de trabajo sustantivo (mandato) |
| BELOW_REQUIRED o UNKNOWN (rol vinculable) | no hay binding ni trabajo dependiente; la investigación independiente sigue permitida |
| Evidencia material contradictoria | su STOP (S-04) se conserva **aunque** otro campo diga MATCH o BELOW_REQUIRED |
| MATCH / ABOVE_REQUIRED | habilita solo el aspecto de configuración; custodia, autenticación, consumo, alcance, CI y demás STOP siguen aplicando |
| Decisión del Owner | puede cambiar requisitos o aceptar riesgos dentro de su autoridad; **nunca** convierte un hecho no observado en MEASURED o MATCH |
| Recuperación | nueva observación o revalidación pertinente; nunca reconfiguración automática ni reset |

**Ejemplos para la revisión adversarial:**

| Caso | Requisitos obligatorios (nivel; effort; fuente de introspección) | Agregado | Disposición |
|---|---|---|---|
| E1 positivo | Frontera = Frontera; Long-horizon = Long-horizon; observación del runtime ligada a la sesión | MATCH | sigue |
| E2 superior | Frontera; Maximum > Long-horizon; observación | ABOVE_REQUIRED | sigue y registra el motivo |
| E3 inferior | Frontera; Balanced < Long-horizon; observación | BELOW_REQUIRED | STOP (Principal) |
| E4 desconocido | Frontera; effort sin fuente observable | UNKNOWN | no hay trabajo sustantivo dependiente; se indica qué acreditar |
| E5 mezcla | nivel BELOW_REQUIRED; effort UNKNOWN | BELOW_REQUIRED | STOP; se registran **ambas** causas |
| E6 contradictorio | la app dice effort X y su registro de sesión dice Y en el mismo instante | UNKNOWN (no acreditado) | STOP S-04 por contradicción material, además de UNKNOWN |
| E7 obsoleto | observación anterior a un cambio de selector, máquina o binario | UNKNOWN | revalidar; no se hereda |
| E8 rebinding | Worker pasa de la celda X a la Y | se reevalúa Y desde cero | contadores intactos (D-10); custodia transferida (D-08) |
| E9 opcional | cuota desconocida y obligatorios en MATCH | MATCH | métrica registrada como desconocida, sin efecto |
| E10 Owner | el Owner acepta usar un nivel inferior para una tarea | el requisito cambia por decisión registrada; el agregado se recalcula contra el requisito nuevo | sin convertir lo no observado en MATCH |

## 5. Binding de roles (D-05)

**Entradas** (mandato, «ROLE BINDING» y «MODEL ROUTING»):
- TaskProfile y RequiredCapability/RequiredEffort (routing);
- IndependenceRequirement (D-12, recibido del dominio superior);
- RuntimeAvailability (preflight, D-06);
- política de coste o cuota, solo si es conocida.

**Algoritmo:**
1. **Requisitos:** clase, dimensiones y requisitos semánticos del rol.
2. **Candidatas:** celdas del catálogo (modelo × runtime × capacidad) **con adapter descrito**.
3. **Filtro:** cada requisito obligatorio debe estar en MATCH o ABOVE_REQUIRED con hechos de preflight vigentes; UNKNOWN no es elegible; la frescura del
   catálogo se aplica (`routing.md` §5).
4. **Independencia:** se aplica la exigencia recibida. El router **no la rebaja**.
5. **Elección:** el nivel más bajo adecuado; transporte por la jerarquía del mandato; coste o cuota solo con política conocida.
6. **Registro de binding:** rol, celda, RoutingReason con fechas, **alternativas descartadas con su causa**, condiciones de escalado, preflight citado
   (identidad) e independencia satisfecha.
7. **Aceptación:** el Coordinator, como extensión de A7.
8. **Transferencia de custodia** (D-08) antes de que el rol opere.

**Sin celda elegible:** no se invoca nada, igual que en `routing.md` §4. **Rebinding:** un registro nuevo con transferencia explícita de custodia; los
contadores no se reinician (D-10).

## 6. Preflight de entorno (D-06)

Cada hecho del preflight registra:
- el requisito al que responde y el alcance de la observación;
- el valor y la fuente (comando o metadato);
- el instante y el host (identificador no reversible);
- la disposición.

| Núcleo (agnóstico) | Por adapter |
|---|---|
| Git: ejecutable, repositorio, `origin/main`, rama, worktrees, árbol limpio, operaciones en curso | runtime: localización verificada del ejecutable (**exige que exista**, no «el directorio más nuevo»), versión |
| custodia: delegaciones abiertas y escritores activos (D-08) | autenticación: AUTHENTICATED / NOT_AUTHENTICATED / UNKNOWN, **sin credenciales** |
| procesos: relectura y atribución (punto 1/9 del triage) con la lista que aporte cada adapter | introspección: fuentes y nivel de garantía (D-11) |
| directorios de evidencia y área transitoria ignorada | permisos y herramientas del runtime; sandbox |
| herramientas del host requeridas (`gh` autenticado, SDK) | huella de configuración, si el adapter la declara (p. ej., `config.toml` de Codex: SHA-256 + nombres de claves) |

**Revalidación obligatoria** ante cualquiera de estos cambios: máquina, runtime o binario, credenciales, catálogo, binding, SHA relevante o huella de
configuración. Una CLI encontrada o un esquema válido **no** acreditan una invocación.

## 7. Frontera de adapters (D-07; M-07)

**Contrato** (normativo en §16; C62-F0-09 §3). Cada adapter debe aportar:
1. describir el runtime;
2. observar capacidades y autenticación;
3. renderizar el prompt para un PromptProfile;
4. invocar;
5. observar el resultado;
6. cancelar por tope;
7. confirmar la terminación;
8. clasificar sus procesos;
9. declarar su huella de configuración (o «ninguna»).

**Descriptor por adapter** (procedimiento subordinado): `docs/automation/agent-execution/adapters/<adapter-id>.md`. Tiene secciones fijas que
cubren los nueve puntos, más las limitaciones conocidas y la fecha de verificación.

**Esquema de hechos por adapter:** `rackcad-adapter-<adapter-id>-facts/v1`. El núcleo valida los hechos de cada adapter con **su** esquema.

**Neutralidad (C62-F0-10 E):**
- el núcleo identifica el adapter con `AdapterId` (patrón `^[a-z0-9-]+$`, **sin enum de marcas**) y `AdapterVersion`;
- un objeto `AdapterFacts` **validado** por el esquema que nombra `AdapterId`: no es un objeto libre que eluda la validación;
- un adapter desconocido no se valida y la celda no es elegible.

**Adapters iniciales** (cada uno con su estado real, sin fingir paridad):

| Adapter | Estado de partida (Discovery §11) |
|---|---|
| `claude-desktop-session` | observable (`get_session`) |
| `claude-subagent` | Worker medido en I-61 |
| `claude-cli` | NOT_AUTHENTICATED |
| `codex-cli` | Controller medido en I-61; binario y huella pendientes de revalidar |
| `codex-desktop-session` | fuente candidata (SP-3) |

Los nombres de originator, source y formatos **no se generalizan** a otras versiones sin verificar el adapter.

## 8. Custodia por unidad (D-08) y recuperación (D-09)

### 8.1 Custodia

Registro durable **por unidad** en el estado canónico (versión nueva del formato de AUTOMATION_PLAN §8). Contenido del bloque `custody`:
- titular actual de cada rol, con su referencia de binding;
- propietario de rama, worktree y alcance de escritura;
- delegación abierta;
- último SHA verificado;
- `attempts` y contadores;
- siguiente acción autorizada.

Cada valor distingue **declarado**, **observado** (con fuente e instante) y **autorizado** (con la decisión que lo autoriza). Las transiciones se anotan de
forma append-only en la evidencia de la unidad (cesión → terminación confirmada → verificación → aceptación).

**Superficies compartidas:** solo **referencias** a los compromisos (emisor, alcance, acuse, RELEASE, correlación, vigencia). **No** hay registro global que
asigne roles, números, turnos o permisos a otras unidades ni sustituya al Master o al Owner. Un cruce textual sin conflicto no da exclusividad. Cambiar de
proveedor no altera los compromisos existentes.

Los hechos de Git prueban identidad y estado del remoto, **no** quién sigue operando en otra máquina.

### 8.2 Estados del escritor y recuperación (C62-F0-10 C)

| Estado | Definición | Qué permite |
|---|---|---|
| **WRITER_ALIVE** | escritor confirmado vivo por el adapter en el host de la custodia | nada nuevo: un escritor por alcance |
| **TERMINATION_UNACCREDITED** | no hay prueba de terminación ni de aislamiento (incluida otra máquina, una conexión perdida o un tope vencido) | **no** permite tomar posesión. STOP de la operación; inspección sin sobrescribir |
| **ORPHAN_CONFIRMED** | terminación confirmada por el adapter y trabajo sin resolver (sin entrega ni cesión) | **no** permite tomar posesión por sí mismo. Exige decisión del Coordinator, inspección que preserve el trabajo y una transferencia de custodia registrada |

**Reglas:**
- se preservan el trabajo no publicado y los cambios sin commit;
- no se usan `reset`, `clean` ni borrados para fabricar un árbol limpio;
- no se matan procesos ajenos;
- si cambia la máquina y no se acredita exclusividad ni recuperación, la operación se detiene; no se descarta ni se reconstruye como existente el trabajo
  local inaccesible;
- un timeout nunca es un permiso.

## 9. Presupuestos (D-10)

Se conserva 16.8. `attempts` y los contadores por (`TaskId`, `FailureClass`) **sobreviven** a cambios de proveedor, modelo, rol, sesión y máquina. El registro
de binding (D-05) lleva una instantánea de los contadores, y un rebinding no los toca. El escalado es explícito (`ModelEscalationReason`). Los topes de
invocaciones y de BLOCKED no cambian.

## 10. Fuentes de introspección (D-11, C62-F0-10 B)

**Niveles de garantía** de una observación de modelo y effort:
- **REQUESTED**: lo pedido;
- **CONFIGURED**: el ajuste persistido;
- **RUNTIME_OBSERVED**: metadato que escribe el runtime, ligado a sesión, turno o invocación e instante;
- **SERVICE_ATTESTED**: atestación del servicio. Es evidencia adicional, **no** se exige.

La autodeclaración del modelo **no** es fuente. Cada observación se liga a unidad, rol, sesión real, turno o invocación, runtime/versión, instante,
procedencia y nivel, preservando la privacidad. Si solo se observa REQUESTED, no se presenta como ejecución efectiva. Si la fuente no alcanza el nivel que
fija el perfil, el estado es UNKNOWN.

| Adapter | Fuente candidata (Discovery) | Nivel máximo conocido | Límite |
|---|---|---|---|
| `claude-desktop-session` | `get_session` (`model`, `effort`, `sessionId`) | RUNTIME_OBSERVED (por verificar en el adapter) | la configuración aplicada por el cliente no prueba el backend |
| `claude-subagent` | transcripción: `"model"`/`"effort"` por mensaje | RUNTIME_OBSERVED | prompt enmarcado por el harness (I-61) |
| `codex-cli` | registro de sesión sin `--ephemeral`: `session_meta`/`turn_context` | RUNTIME_OBSERVED | binario actual sin medir |
| `codex-desktop-session` | `~/.codex/sessions/…`: `turn_context.model/effort` (SP-3) | RUNTIME_OBSERVED (candidata) | ligar la sesión a la unidad y al rol; no se barre de nuevo; sin generalizar formatos |
| `claude-cli` | no medido | UNKNOWN | NOT_AUTHENTICATED |

**Nivel mínimo propuesto** para los requisitos obligatorios del Principal y de los roles vinculados: **RUNTIME_OBSERVED**. Su suficiencia la revisa el
Freeze y la prueba F1/F2, sin suponer nada del backend.

## 11. Independencia por riesgo (D-12)

Clases propuestas (los nombres finales los fija el Freeze):
- **SAME_SESSION_ALLOWED**;
- **INDEPENDENT_CONTEXT_REQUIRED**;
- **INDEPENDENT_PROVIDER_PREFERRED**;
- **INDEPENDENT_PROVIDER_REQUIRED**: solo con riesgo justificado de forma explícita y un mecanismo viable.

| Disparador (mandato, «INDEPENDENCE BY RISK») | Dueño del predicado | Clase propuesta |
|---|---|---|
| Revisión de diseño de NEW ARCHITECTURE / FOUNDATION EVOLUTION, conformidad, READY | **LIFECYCLE** (§5) | la que fije LIFECYCLE; el contrato la transporta |
| Cambio de autoridad compartida | LIFECYCLE (revisión) + §16 (ejecución) | INDEPENDENT_CONTEXT_REQUIRED; proveedor preferido |
| Operación destructiva | §16 | INDEPENDENT_CONTEXT_REQUIRED + autorización explícita (S-05) |
| Candidato/READY | LIFECYCLE | la de LIFECYCLE |
| Cambio sensible a la seguridad | §16 | INDEPENDENT_PROVIDER_PREFERRED |
| Evidencia ambigua de alto coste | §16 | INDEPENDENT_CONTEXT_REQUIRED |
| Coordinator y Worker en la misma sesión | §16 | la verificación no puede quedar en la misma sesión (Controller distinto) |
| Cableado rutinario y acotado | §16 | SAME_SESSION_ALLOWED |

Una revisión SAME-SESSION ROLE se declara como tal y nunca se presenta como independiente. No se exige un proveedor distinto por microtarea.

## 12. Pilotos A/B y prueba de portabilidad (D-17)

Se ejecutan como **pruebas del sistema bajo prueba** dentro de un **fixture**: un repositorio Git local aparte, sembrado desde una plantilla, sin `src/`, fuera
del repositorio de RackCad. No conceden autoridad real a sus participantes (C62-F0-10 D).

| Topología | Composición | Precondiciones | Resultado admitido |
|---|---|---|---|
| A | Principal Claude (escritorio) + Workers Claude (subagente) + Codex externo (Controller o Reviewer) | OD-2 (`config.toml`) y revalidación del binario de Codex; OD-5 | PASS / UNVERIFIED / UNSUPPORTED con causa |
| B | Principal Codex (escritorio) + Workers Codex + Claude CLI externo (Reviewer) | OD-3 (autenticar Claude CLI), OD-4 (sandbox de Codex para escritura), fuente de introspección de Codex de escritorio acreditada; OD-5 | ídem; sin fingir paridad |

**Portabilidad** (A→B y B→A si es viable). **Aislamiento del oráculo** (C62-F0-10 C):
1. El esperado lo prepara el **Coordinator fuera del host**, antes del ensayo, desde el estado canónico del fixture y no desde respuestas.
2. En la evidencia de I-62 solo se compromete el **SHA-256** del esperado. El hash prueba integridad, **no** aislamiento.
3. B recibe **solo** el fixture (ruta y remoto propios) y sus documentos canónicos. Su runtime arranca limitado a ese directorio, y el registro de relevo
   anota sus entradas.
4. B publica su respuesta (artefacto con identidad) **antes** de que el Coordinator entregue el esperado.
5. La comparación es mecánica, hecho por hecho, y la evidencia se incorpora con su identidad.

El estado que B necesita debe estar en las fuentes canónicas del fixture. Un hecho ausente ahí no puede estar en el esperado.

**Controles negativos:**
- handoff ausente;
- SHA cambiado;
- credencial sin acreditar;
- 0 pruebas seleccionadas;
- escritor anterior en TERMINATION_UNACCREDITED;
- trabajo sin commit;
- artefacto de otra corrida;
- contadores tras cambiar de proveedor;
- un hecho retirado del estado canónico, que B debe declarar UNKNOWN.

## 13. Semántica de fallo (D-13)

Los STOP de I-61 se conservan. Ids nuevos propuestos (nombres y numeración se fijan en el Freeze):

| Id | Condición | Comportamiento |
|---|---|---|
| P-09 | Principal en BELOW_REQUIRED al abrir | STOP antes de trabajo sustantivo |
| P-10 | requisito obligatorio de un rol en UNKNOWN o BELOW_REQUIRED en el binding | no se invoca (BLOCKED de planificación); no consume `attempts` |
| P-11 | huella de configuración de un adapter cambiada durante una cesión | STOP (generaliza P-01 a cualquier adapter que declare huella) |
| P-12 | escritor en TERMINATION_UNACCREDITED o ORPHAN_CONFIRMED sin decisión de custodia | STOP; no hay toma de posesión |
| P-13 | cambio de máquina sin acreditar exclusividad ni recuperación | STOP de la operación |
| P-14 | adapter desconocido o hechos que no validan contra su esquema | no elegible |

## 14. Contratos de datos (DISEÑO; no se publican esquemas operativos) y transición desde `/v1` (D-14)

| Contrato | Cambio |
|---|---|
| `rackcad-relay-record/v2` | `Participant` → `{Role, BindingRef, AdapterId, AdapterVersion, Transport (semántico), Observation[] {Source, Assurance, Value, ObservedUtc}}`; `Exit`/`Entry`: `ConfigFingerprint {Kind, Sha256, KeyNames}`, obligatorio solo si el adapter declara huella; `Host {HostIdHash, Os}`; `RebaseMap.MaterializationCloseSha` (sustituye `G2CloseSha`); `TestArtifacts[].Skipped` |
| `rackcad-controller-verification/v2` | `Verifier {Role, BindingRef}`; las 14 comprobaciones sin cambios |
| `rackcad-gate-contract/v2` | `ProtocolSet`; `RoleRequirements[] {Role, Profile, Mandatory[], Independence}`; `EligibleCells` por rol; `MaterializationCloseSha` |
| `rackcad-delegation/v2` | `Executor.BindingRef`; `Independence`; `Owner.Kind` con valores semánticos de transporte |
| `rackcad-worker-handoff/v1` | **sin cambios**; se mantiene en el conjunto |
| `rackcad-preflight/v1` (nuevo) | hechos de núcleo + `AdapterFacts` validados; requisitos con estado; agregado y disposición (§4.2) |
| `rackcad-binding/v1` (nuevo) | §5, paso 6, incluidas las alternativas descartadas y la instantánea de contadores |
| `rackcad-adapter-<id>-facts/v1` (por adapter) | hechos propios de cada adapter |
| estado canónico, versión nueva | bloque `custody` (§8.1) |

**Convivencia:**
- `/v1` **no se toca**: ni esquemas ni evidencia histórica (INV-I62-09);
- cada instancia declara su `Schema`;
- `gate-contract/v2` fija el `ProtocolSet` de toda la cadena;
- una cadena `/v1` abierta termina en `/v1`;
- una unidad adopta `/v2` en su primer contrato posterior a la integración de I-62, nunca a mitad de una cadena;
- los consumidores y las superficies se revalidan en READY-04 y antes de integrar, sin inferir su estado futuro.

## 15. «Cierre de G2» y ruta de I-62 bajo I-61 (D-15, C62-F0-10 D)

**Hito:**
- **MaterializationClose**: el commit que cierra el último gate que materializa los cambios normativos de la unidad, revisado por el Coordinator contra el
  Freeze. Generaliza el papel del «cierre de G2» de I-61 en 16.3, 16.5 y 16.7.
- Clases: `UNIT_DOC` y `UNIT_CHANGE` se leen en `AuthorityRevision` (= MaterializationClose o un X posterior según 16.3); `EXTERNAL` se lee en `MainSha`.

**Para I-62:**
- **F1-F4 son trabajo directo** de la sesión principal, sin contrato ni delegación: la equivalencia G2 no aplica (C62-F0-10 D);
- no se fija `G2CloseSha` ni `AuthorityRevision` por analogía;
- **Restricción registrada:** 16.3 solo define `AuthorityRevision` para I-61. Una delegación real de I-62 bajo I-61 (los pilotos de F6) exige antes un
  tratamiento formal por su autoridad, en concreto la autorización OD-5. **Nunca** se usa I-62 sin integrar para concederse permiso;
- **C-F0-RED:** las tareas documentales siguen sin delegarse. El remedio general es SEPARATE UNIT propuesta, que no se abre ni se delega.

## 16. Invariantes → pruebas, controles y oráculos

Restricciones de las pruebas (las del Freeze de I-61 §13):
- `System.Text.Json`;
- sin paquetes nuevos;
- RED por mutación en memoria.

| Id | Invariante | Prueba / control | RED esperado |
|---|---|---|---|
| INV-I62-01 / OBL-I62-01 | La semántica de roles no nombra proveedores | Core: la tabla de roles de §16.1 y los enums del núcleo no contienen marcas; conjunto derivado de los proveedores del catálogo y los adapters, más patrones genéricos | mutación que inserta «Codex» en un rol |
| INV-I62-02 / OBL-I62-02 | UNKNOWN nunca → MATCH; BELOW_REQUIRED domina | Core: la regla de §4.2 aplicada a los ejemplos E1-E10, versionados como tabla del procedimiento (README) | ejemplo con UNKNOWN obligatorio que agregue MATCH |
| INV-I62-03 / OBL-I62-04 | Esquemas del núcleo neutrales; hechos de adapter validados | Core: ningún enum de `/v2` contiene marcas; `AdapterFacts` exige `AdapterId` con patrón y un esquema existente | enum con `codex-cli`; `AdapterFacts` sin esquema |
| INV-I62-04 | Requisito obligatorio UNKNOWN → no binding | Esquema de `binding/v1` (condicional) + control del piloto | binding aceptado con UNKNOWN obligatorio |
| INV-I62-05 | Transferencia de custodia explícita | Control por relevo en el fixture | rebinding sin cesión registrada |
| INV-I62-06 | Los contadores sobreviven al rebinding | Escenarios en el README + control del piloto | contador a 0 tras cambiar de proveedor |
| INV-I62-07 | Sin toma de posesión en TERMINATION_UNACCREDITED u ORPHAN_CONFIRMED | Control negativo en el fixture | posesión tomada |
| INV-I62-08 / OBL-I62-07 | Sin secretos | Core: ningún esquema tiene campos de credencial; control sobre la evidencia | campo `token` añadido |
| INV-I62-09 | `/v1` intacto | Core: blobs de los cinco esquemas `/v1` fijados por hash | cualquier cambio en un `/v1` |
| INV-I62-10 / OBL-I62-03 | Autoverificación al abrir | OV-I62-01 + registro de preflight del Principal en cada sesión de F1-F7 | sesión sin registro |
| INV-I62-11 / OBL-I62-05 | Cada hecho de preflight tiene fuente, instante y disposición | Core: esquema de `preflight/v1` | hecho sin `ObservedUtc` |
| INV-I62-12 / OBL-I62-06 | Cada adapter cubre los 9 puntos del contrato | Core: secciones fijas de cada descriptor | descriptor sin «confirmar terminación» |
| INV-I62-13 / OBL-I62-08 | La independencia exigida no se rebaja | Core: `gate-contract/v2` y `delegation/v2` con la misma clase o una más estricta | delegación con clase menor |

## 17. Plan de gates por resultado observable (D-16) y decisión sobre F5

| Gate | Resultado observable | Evidencia | Bloqueos externos (momento exacto) |
|---|---|---|---|
| **F0 (cierre)** | Proposal acordada (Coordinator + Architect, misma versión) y congelada; ADR sucesor formal **propuesto** (WORKFLOW §8: existe antes de implementar) | Freeze; revisiones | — |
| **F1** | Roles, perfil del Principal, `CONFIGURATION_STATUS` y autoverificación al abrir materializados (§16, `routing.md`, catálogo, WORKFLOW §4 por referencia); OBL-I62-01..03 en verde | RED→GREEN; Core Full al cierre; registro de preflight del Principal en cada sesión | aceptación del ADR (OD-1): **no** bloquea F1 (el ADR existe como propuesto); bloquea READY |
| **F2** | Preflight de núcleo + descriptores y esquemas de hechos de los adapters iniciales; `/v2` publicados; OBL-I62-04..07 | preflight real de cada adapter en el host, con UNVERIFIED honesto | OD-2 solo bloquea medir **invocando** `codex-cli`; el resto de F2 sigue |
| **F3** | Binding de todos los roles y política de independencia (`routing.md`, §16, LIFECYCLE por predicado); OBL-I62-08 | registro de binding de ejemplo con alternativas descartadas | — |
| **F4** | Custodia por unidad, estados del escritor y procedimiento de recuperación; **MaterializationClose** | controles negativos documentados; procedimiento versionado de procesos (positivo y negativo) | — |
| **F5** | **No planificado** | — | — |
| **F6** | Pilotos A/B y prueba de portabilidad en el fixture | resultados PASS / UNVERIFIED / UNSUPPORTED por topología; oráculo aislado | **antes de F6:** OD-5. **Antes de A:** OD-2. **Antes de B:** OD-3 y OD-4 |
| **F7** | Conformidad; actualización de la entrada de FOUNDATIONS **Agent Execution Protocol**, que se extiende (**propuesta:** los contratos nuevos forman parte de esa entrada; no se propone una entrada propia, para no crear una descripción paralela); corrección de PROMPT_TEMPLATES §2; ideas-futuras | READY-06 | — |
| **READY / Candidato** | WORKFLOW/LIFECYCLE | Full, CI exacta, OV | OD-1 aceptado antes de READY-03 (precedente I-61) |

**Decisión razonada sobre F5** (C62-F0-10 E): Level A, porque los puntos que motivaron Level B se resuelven con **procedimiento versionado**:
- relectura y atribución de procesos, y filtros: fragmentos de PowerShell en el README, como ya hace §3.2;
- coherencia: la regla del README §8.

F4 debe **probar su viabilidad** con un caso positivo y uno negativo de procesos, transporte y recuperación. Si la prueba muestra que hace falta un helper,
se propondrá **uno acotado** para decisión **antes del Freeze**, sin implementarlo. No se heredan scripts privados ni se fija un número mínimo de pilotos.

## 18. Owner Validation y decisiones del Owner

**Matriz OV propuesta** (se fija en el Freeze; no se requiere AutoCAD: no hay cambio de dibujo):

| Id | Escenario | Gate | Requiere acción del Owner |
|---|---|---|---|
| OV-I62-01 | Al abrir una sesión principal con un effort inferior al requerido se produce STOP (P-09); con el correcto, MATCH registrado | F1 | cambiar el selector |
| OV-I62-02 | El preflight del host muestra cada runtime con su estado real, incluidos NOT_AUTHENTICATED y UNVERIFIED, sin secretos | F2 | revisar el informe |
| OV-I62-03 | Topología A en el fixture (o su limitación registrada) | F6 | OD-2, OD-5 |
| OV-I62-04 | Topología B en el fixture (o su limitación registrada) | F6 | OD-3, OD-4, OD-5 |
| OV-I62-05 | Prueba de portabilidad A→B con el oráculo aislado | F6 | ninguna adicional |

**Decisiones del Owner** (OWNER-RESERVED), cada una con su momento exacto de bloqueo:

| Id | Decisión | Bloquea | Antes de |
|---|---|---|---|
| OD-1 | Aceptar el ADR sucesor parcial de ADR-0046, incluida la ampliación del dominio de §16 y la fila de WORKFLOW §10 | READY-03 | READY-03 |
| OD-2 | Disposición de la referencia de `config.toml` (`37DD3559…` registrada; `42E15A03…` observada), identificando archivo y alcance | cualquier invocación de `codex-cli` | la primera invocación de Codex (F2 medición o F6 A) |
| OD-3 | Autenticar Claude CLI | el rol externo Claude de B | F6 B |
| OD-4 | Configuración del sandbox de Codex para Workers con escritura (distinta de la autenticación) | Workers Codex de B | F6 B |
| OD-5 | Autorizar los pilotos de I-62 en el fixture bajo reglas no integradas (precedente: vigencia del piloto de I-61 en §16) | F6 | el inicio de F6 |

Cada solicitud al Owner se formula aparte, con la ruta y el hash actuales, el efecto, el consumo, el alcance y las restricciones (C62-F0-12). El silencio no
es decisión.

## 19. Riesgos y los 12 retos del Architect (mandato, «ARCHITECT»)

Las mitigaciones y el estado de cada reto están en el [paquete del Architect](I-62-architect-package-v1.md) §4. Riesgo principal de este diseño: el número de
contratos nuevos (`preflight`, `binding`, hechos por adapter). Mitigación: Level A, sin motor; un único conjunto de protocolo por cadena; `worker-handoff` sin
cambios.

---

## Anexo A — Borrador del ADR sucesor PARCIAL de ADR-0046 (sin número; no es un ADR formal)

- **Título propuesto:** «Ejecución delegada portable: roles independientes del proveedor, binding por capacidad acreditada y autoverificación del
  Principal».
- **Estado:** borrador dentro de la Proposal V1. **Sin número global**; no edita ADR-0046 ni el índice ADR. El ADR formal debe existir (como propuesto) antes de
  implementar su decisión (WORKFLOW §8), y su aceptación es del Owner (OD-1).

**Conserva de ADR-0046:**
- **#2:** declaraciones exclusivas de Worker y Controller; solo el Coordinator declara GATE PASS;
- **#3:** relevo, un participante a la vez; el paquete no amplía el contrato;
- **#4:** routing estable separado de un catálogo mutable; elegibilidad por celda;
- **#5:** esquemas estrictos, ahora con versión nueva donde cambia el contrato;
- **#6:** verificación fail-closed y multiseñal;
- **#7:** presupuesto único sin reinicios;
- **#8:** transporte;
- **#9:** Level A.

**Propone superar:**
- «el proveedor solo está fijado para el Controller» (Alternativas) y el título «Controller Codex»: el Controller y los demás roles se vinculan por capacidad
  acreditada (D-01, D-05);
- «la independencia de la verificación es parcial» como coste aceptado: se sustituye por la política por riesgo (D-12), con la limitación registrada cuando no
  haya mecanismo viable.

**Amplía #1 (OWN-L):** el dominio de AUTOMATION_PLAN §16 pasa a incluir obligaciones de la sesión principal **fuera** de una delegación: autoverificación al
abrir, preflight y custodia operativa por unidad. El texto de la fila de WORKFLOW §10 se ajusta. Es materia **OWNER-RESERVED**.

**Consecuencias:**
- más contratos (preflight, binding, adapters) y su mantenimiento;
- una revalidación por cambio de runtime;
- dos topologías que pueden quedar UNVERIFIED por precondiciones del Owner.

**Requiere del Owner:** aceptación (OD-1). No se declara.
