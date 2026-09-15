# Initiative Lifecycle

> **Materialized for I-56; not effective until WORKFLOW_V2_EFFECTIVE_SHA exists.**
>
> Este documento materializa [Proposal V4](initiatives/I-56-proposal-v4.md) P-01..P-05, P-08..P-10,
> P-12, P-16, P-19 y P-22 conforme a [ADR-0045](adr/0045-workflow-v2-ciclo-evidencia-e-integracion.md).
> I-56 sigue bajo Workflow V1. La existencia de este archivo no activa Workflow V2.

## 1. Autoridad y limites

Este documento es la autoridad de **diseño y ciclo de vida** para iniciativas V2: intake, agrupacion,
arquetipo, materialidad, Discovery, revision de diseño, Freeze, gates funcionales, orden READY,
declaracion de Candidato, conformidad y evaluacion del proceso.

No gobierna mecanica de reclamo Git, worktrees, apertura o rebase de sesion, merge, post-merge, campos
exactos de evidencia de GitHub Actions, formato de tags, ubicacion de hashes, composicion de suites
Full locales, ejecucion de Owner Validation ni cadencia de cobertura. Esos dominios se referencian asi:

| Dominio referido | Autoridad destino | Estado durante I-56 |
|---|---|---|
| Git, reclamo, worktree, rebase, integracion, post-merge, tags y cadencia documental | `docs/WORKFLOW.md` | **materialized by later I-56 normative gate** |
| Evidencia, clases, invalidadores, suites Full y exact-SHA | `AGENTS.md` | **materialized by later I-56 normative gate** |
| Matriz y ejecucion de Owner Validation, entrega del DLL | `docs/guias/validacion-manual-autocad.md` | **materialized by later I-56 normative gate** |
| Cobertura | `docs/WORKFLOW.md` y workflow CI vigente | **materialized by later I-56 normative gate; politica sin cambio** |

Hasta que esos destinos se materialicen, la definicion aprobada completa sigue en Proposal V4. Una
contradiccion entre dominios detiene el gate y se eleva a la autoridad competente; no se resuelve
creando una segunda regla aqui.

Los arquetipos solo cambian profundidad de Discovery y diseño. Nunca reducen evidencia final, identidad
exact-SHA, Full del Candidato, Owner Validation, CI ni conformidad. No existen T0-T4, R0-R4, Quick CI,
merge automatico, reutilizacion por igualdad de arbol ni un modelo Full solo para el Candidato.

## 2. Intake, iniciativa conceptual y unidades de entrega

El **intake** es planificacion del Coordinator antes de trabajo sustantivo. Usa el plan, decisiones,
fundaciones declaradas y hechos Git disponibles; no sustituye el Discovery de codigo. Registra UNKNOWN
donde la planificacion no pueda resolver una afirmacion.

Un ID funcional expresa una necesidad del usuario. Una **iniciativa conceptual** es la unidad de diseño
y decision. Una **unidad de entrega** es la unidad Git que implementa y entrega una parte verificable.
No son identidades intercambiables.

Se agrupan IDs en una iniciativa conceptual solo cuando se cumplen a la vez:

1. resuelven sustancialmente el mismo problema de usuario u objetivo observable; y
2. comparten materialmente autoridad o fundacion de diseño que, separados, se diseñaria dos veces.

Infraestructura, UI o archivos calientes compartidos por si solos requieren reutilizacion, secuencia o
coordinacion; no agrupacion. Se separa cuando falta una condicion o cuando el resultado del Owner es
independiente. UNKNOWN en cualquiera de las dos condiciones detiene la decision: Discovery debe
resolverlo antes del Freeze, sin agrupar ni separar por conveniencia.

Una iniciativa conceptual se divide en unidades de entrega cuando difieren sistemas, superficies de UI,
archivos calientes o limites de rollback. Cada unidad debe entregar un resultado verificable por si
misma y mantiene su evidencia propia. Una unidad solo de fundacion se fusiona con su primer consumidor,
salvo que exista un limite de rollback, secuencia forzada u otro consumidor que necesite la fundacion
integrada antes.

La iniciativa conceptual hace una vez Discovery, diseño y Freeze. Cada unidad hace su propio bootstrap,
Discovery delta, gates, READY, Candidato e integracion. El Discovery delta vuelve a comprobar DC-01..06
en su base actual o demuestra estabilidad real mediante inventario completo y diff; DC-07..09 siempre
son actuales. Una unidad no consume un Freeze o enmienda de una hermana hasta que sea alcanzable desde
`origin/main`, salvo la unidad autora en su propia rama.

## 3. Arquetipos y materialidad

El Executor propone el arquetipo con evidencia, el Coordinator decide y el Architect puede elevarlo.
Antes del Freeze puede subir o bajar con evidencia registrada; bajar exige Coordinator y, si participo,
Architect. Despues del Freeze solo puede subir.

| Arquetipo | Condicion |
|---|---|
| **EXTENSION** | Consume fundaciones aceptadas e integradas sin activar M-01..M-08 |
| **FOUNDATION EVOLUTION** | Modifica materialmente una autoridad, persistencia, contrato reutilizable o punto de extension existente: M-01..M-06 o M-08 |
| **NEW ARCHITECTURE** | Crea una autoridad o persistencia transversal, framework, registro o contrato arquitectonico nuevo: M-07, o M-01/M-02 creador |

Ante duda se usa el arquetipo superior. UNKNOWN en un disparador cuenta como activado hasta que EXP-09
lo resuelva.

| ID | Disparador de MATERIAL CHANGE |
|---|---|
| **M-01** | Cambia quien posee una regla o valor, o aparece una segunda autoridad para el mismo dato |
| **M-02** | Cambia esquema, DTO o wire, significado persistido, fallback legacy o preservacion de campos desconocidos |
| **M-03** | Cambia comportamiento observable o trato de documentos existentes fuera de lo pedido |
| **M-04** | Cambia que falla o como falla: silencioso/visible, abortar/normalizar o fail-closed |
| **M-05** | Cambia un contrato consumido por otros sistemas o kinds |
| **M-06** | Añade, cambia o retira un punto de extension reutilizable |
| **M-07** | Introduce framework, registro, kernel, shell o mecanismo generico transversal |
| **M-08** | Modifica, reinterpreta o contradice un ADR aceptado o la fuente de decision integrada citada por una fundacion; una discrepancia solo en el registro descriptivo activa EXP-01 y no le concede autoridad |

Nombres de helpers, division de metodos privados, movimiento mecanico, tecnica local que conserva
invariantes, pruebas añadidas y redaccion no son normalmente materiales. Lo son si rompen el Freeze.
Un cambio de alcance, no-objetivos, decision Owner o retiro de escenario OV es `OWNER-RESERVED`.

## 4. Discovery Core y expansiones

Cada afirmacion usa `MEASURED`, `RECONSTRUCTED`, `INFERENCE` o `UNKNOWN`.

| ID | Salida obligatoria |
|---|---|
| **DC-01** | Comportamiento observable actual, con codigo y prueba existente cuando exista |
| **DC-02** | Simbolos dueños de reglas/valores y ADR o Freeze que los gobierna; FOUNDATIONS solo enlaza |
| **DC-03** | DTO, Xrecord o store, campos, fallback legacy y preservacion de desconocidos |
| **DC-04** | Camino entrada → estado → persistencia → dibujo, con simbolos |
| **DC-05** | Llamadores directos, consumidores entre sistemas y todos los lectores de campos, estructuras persistidas y artefactos de DC-03; inventario y huecos de busqueda |
| **DC-06** | Pruebas y guardas que protegen DC-01..05; huecos declarados |
| **DC-07** | Archivos calientes e intersecciones activas, conforme al preflight de la autoridad Git futura |
| **DC-08** | Fundaciones consumidas o extendidas, verificadas en la base actual contra fuente, codigo, simbolos y pruebas |
| **DC-09** | M-01..M-08 como activado/no activado/UNKNOWN, evidencia y arquetipo propuesto |

Solo se abre Discovery Conditional cuando el Core activa una expansion:

| ID | Disparador |
|---|---|
| **EXP-01** | FOUNDATIONS, ADR aceptado, Freeze o codigo se contradicen: clase A si afecta algo consumido; B solo si es demostrablemente irrelevante |
| **EXP-02** | Autoridad ambigua: dos candidatas o ninguna |
| **EXP-03** | Camino legacy o persistencia no localizable desde Core |
| **EXP-04** | Contrato con consumidores mas alla de un salto o en otro sistema |
| **EXP-05** | Invariante candidato a Freeze sin prueba protectora ni modo conocido de probarlo |
| **EXP-06** | Semantica de fallo no determinable o posible fallo silencioso |
| **EXP-07** | Rama activa toca la misma autoridad o contrato |
| **EXP-08** | Deuda o defecto preexistente que el cambio haria visible o empeoraria |
| **EXP-09** | M de DC-09 sigue UNKNOWN, incluida la distincion crear/modificar; se acota a esa pregunta |

El Coordinator autoriza cada expansion con disparador, pregunta, area acotada y criterio de salida.
FOUNDATION EVOLUTION y NEW ARCHITECTURE permiten que el Architect exija una expansion omitida. Una
ambiguedad sobre comportamiento deseado es decision de alcance, no expansion.

Antes del diseño, el Coordinator revisa DC-01..09, todas las evaluaciones EXP —incluidos los negativos
razonados— y pregunta expresamente cual debio activarse y no se activo. Solo despues confirma agrupacion,
materialidad y arquetipo. DC-09 UNKNOWN activa EXP-09.

Una contradiccion consumida **clase A** es STOP. Duda entre A y B cuenta como A. Clase B exige evidencia
desde DC-02..06 y confirmacion de Coordinator + Architect en todos los arquetipos antes de salir. El
Discovery termina solo con DC completo, cada expansion cerrada o UNKNOWN con autoridad decisora,
revision del Coordinator y ninguna clase A abierta.

### 4.1 Interaccion con FOUNDATIONS

Una fundacion es reutilizable solo si esta integrada en `main`, tiene ADR aceptado o Freeze integrado y
declara un punto de extension o un consumidor fuera de su iniciativa autora. El contrato declara
`Consumes:`, `Extends:` e `Introduces:`. Quien introduce o extiende redacta antes de READY-04 la entrada
factual que revisara la conformidad; un consumidor solo verifica y no edita la entrada. El momento de
publicacion en el cierre pertenece a `docs/WORKFLOW.md` (**materialized by later I-56 normative gate**).

DC-08 verifica en cada base que fuente, simbolos, pruebas y comportamiento coinciden. FOUNDATIONS nunca
prevalece sobre codigo para describir lo que existe ni sobre ADR/Freeze para definir lo que debe existir.
Todo desacuerdo activa EXP-01 con las reglas A/B anteriores. La poblacion inicial y el archivo
`docs/FOUNDATIONS.md` se materializan en otro gate de I-56; este documento no crea entradas.

## 5. Participacion del Architect

| Caso | Revision anterior al Freeze |
|---|---|
| Unidad bajo Freeze ya revisado | Sin ronda nueva si el delta no activa disparadores; se revisa compatibilidad |
| EXTENSION independiente | Revision de materialidad, invariantes, pruebas y matriz OV |
| FOUNDATION EVOLUTION | Una revision adversarial obligatoria |
| NEW ARCHITECTURE | Rondas Coordinator ↔ Architect hasta acuerdo |

La primera revision recibe Discovery completo, EXP positivos y negativos, revision del Coordinator,
fuentes consumidas, codigo de esa base y version completa del diseño. Toda re-revision recibe la
**version completa actual**, el **delta explicito** y la disposicion por ID de hallazgos previos. El
delta es foco minimo, no limite: un hallazgo material en una seccion intacta es valido.

Un hallazgo es `REQUIRED` si activa M-01..08 sobre un elemento congelable, deja un invariante sin
obligacion de prueba, produce comportamiento observable distinto o revela un elemento congelable
materialmente incorrecto, no ejecutable o no verificable. Incluye oraculos ciegos y planes de gates
imposibles o inconsistentes. Una precision sin cambio de significado puede ser `OPTIONAL`.

Solo quien emitio un REQUIRED puede rebajarlo, con ID, clasificacion, razon y evidencia. Si no esta
disponible, el finding sigue abierto. `AGREED` exige cero REQUIRED abiertos sobre la version exacta;
de otro modo es `CHANGES REQUIRED`, o `BLOCKED — OWNER DECISION` si falta autoridad reservada.

Toda revision declara `SAME-SESSION ROLE`, `SEPARATE SESSION` o `EXTERNAL HUMAN` y si revisor y autor
son la misma persona. El Coordinator sigue revisando Discovery y gates; esos controles no se sustituyen.

## 6. Consensus Freeze y enmiendas

Antes de implementar se congelan: autoridad; persistencia; comportamiento observable; semantica de
fallo; compatibilidad/legacy; no-objetivos; puntos de extension; obligaciones invariante→prueba con RED
esperado; matriz OV; y resultados verificables del plan de gates. Normalmente no se congelan nombres de
helpers, metodos, archivos exactos, mecanica interna ni orden de commits.

EXTENSION usa un `<I>-freeze.md` breve. FOUNDATION EVOLUTION y NEW ARCHITECTURE congelan su Proposal
autocontenida. Una unidad consume el Freeze conceptual y, si debe añadir, crea un
`<unidad>-freeze-delta.md`. El artefacto queda **inmutable desde su commit de Freeze**.

El acuerdo identifica commit, ruta y blob de la version acordada. El commit de Freeze solo puede cambiar
la linea `Frozen` y lineas de cabecera enumeradas literalmente en el acuerdo; ninguna clausula cambia.
La identidad y trailers exactos se verifican en READY-09. Los detalles Git de esa comprobacion pertenecen
a `docs/WORKFLOW.md` (**materialized by later I-56 normative gate**).

Todo cambio posterior a un elemento congelado usa una enmienda `A-n`:

- append-only, secuencia continua sin huecos ni duplicados;
- Freeze aplicable identificado;
- `Applies-to: all | <lista explicita de unidades>`;
- clausula anterior y delta exacto, motivo, M-01..08 y evidencia;
- autoridades y veredictos versionados antes de evidencia del Candidato;
- correccion, revocacion o sustitucion mediante la siguiente A-n, nunca edicion o borrado.

No existe enmienda semantica invisible. Una unidad lee todas las A-n, aplica solo las que la incluyen y
no consume las de una hermana hasta que sean visibles desde `origin/main`. Una A-n Coordinator-only
puede añadir pruebas u OV de comportamiento ya congelado o resecuenciar gates sin cambiar resultados.
Una M material exige Architect + Coordinator; una materia OWNER-RESERVED exige Owner. Tras READY-04,
cualquier A-n o decision nueva crea un SHA nuevo y reinicia READY-02.

## 7. Gates funcionales

Un gate entrega un resultado conductual util y verificable de forma independiente y admite varios
commits internos. Puede dividirse por
contrato revisable, limite de rollback, subsistema materialmente distinto, hito visible o secuencia por
archivos calientes. No se crea un gate solo por DTO, helper, resolver, mapper, archivo o capa, salvo que
esa unidad tenga un contrato observable verificable por si misma.

Un gate cierra cuando:

1. su resultado fue observado por pruebas o por un hito Owner registrado;
2. cada comportamiento nuevo y bugfix tiene RED→GREEN focal y seleccion mayor que cero;
3. pruebas y evidencia exigidas por la autoridad de evidencia estan verdes;
4. el CI propio del SHA de cierre fue leido y esta verde antes del siguiente gate;
5. el Coordinator reviso diff contra Freeze+A-n, hallazgos y desviaciones, sin EXP-01 A abierta;
6. se cumplio el orden exacto de commit, arbol limpio, evidencia local, push de punta y CI definido en
   `AGENTS.md` y `docs/WORKFLOW.md` (**materialized by later I-56 normative gate**);
7. todo rebase exigido entre gates produjo una punta nueva con su propia evidencia; nada se heredo.

Los hechos posteriores al commit se registran donde disponga la autoridad documental futura; no se
atribuyen al cuerpo del commit que aun no existia. Un CI rojo se diagnostica con sus logs y artefactos
antes de formular la correccion.

## 8. READY y frontera del Candidato

`READY-01`..`READY-09` se satisfacen **en este orden** antes de fijar `FINAL_CANDIDATE_SHA`:

| ID | Condicion |
|---|---|
| **READY-01** | Alcance congelado completo; diferimientos decididos por autoridad competente y A-n versionada cuando cambie Freeze |
| **READY-02** | Gates cerrados; decisiones, A-n y documentos de producto listos y versionados |
| **READY-03** | Sin REQUIRED abierto, discrepancia A ni decision material/Owner pendiente |
| **READY-04** | Fetch, preflight y rebase final completados bajo `docs/WORKFLOW.md`; si existe cierre previo se ejecuta su ruta de retorno (**materialized by later I-56 normative gate**; hasta entonces, Proposal V4 P-12 READY-04/ruta R) |
| **READY-05** | Focales/relevantes y CI propio del SHA resultante satisfacen la identidad y todos los jobs requeridos por `AGENTS.md`/`docs/WORKFLOW.md` (**materialized by later I-56 normative gate**; hasta entonces, Proposal V4 P-12 READY-05) |
| **READY-06** | Architect + Coordinator emiten conformidad completa `CONFORMING` de ese SHA contra Freeze/delta/A-n versionados |
| **READY-07** | Arbol limpio, sin cambio ni commit pendiente para el producto; HEAD identificado |
| **READY-08** | Matriz/asignacion OV completa: todo escenario conceptual tiene unidad, toda obligacion aplicable a esta unidad esta preparada para el SHA final y ningun escenario o ultima asignacion fue retirado sin Owner |
| **READY-09** | Acuerdo exacto, diff permitido, identidad/unicidad/inmutabilidad de Freeze, compatibilidad de fuente V1 y secuencia append-only, alcance y visibilidad de todas las A-n comprobados conforme a §6 |

Solo entonces se declara `FINAL_CANDIDATE_SHA`. La evidencia Full completa, exact-SHA, builds, CI y
Owner Validation aplicable siguen bajo sus autoridades futuras y Proposal V4 P-12/P-14. Ningun SHA
parcial se denomina Candidato. Cualquier correccion que cambie SHA invalida la identidad de evidencia y
reinicia READY-02; la lista de clases e invalidadores pertenece a `AGENTS.md` (**materialized by later
I-56 normative gate**). Un hito intermedio del Owner requiere su propio Candidato completo y no sustituye
el Candidato final.

La asignacion conceptual `OV-id → unidad` vive en Freeze, Freeze delta o A-n. Es aditiva respecto de la
guia; no la reduce. Reasignar exige A-n; retirar o sustituir un escenario o su ultima asignacion exige
Owner. Todo escenario aplicable, incluido el checklist de la guia, se ejecuta sobre
`FINAL_CANDIDATE_SHA`; un hito intermedio es adicional. La ejecucion y entrega exactas pertenecen a la
guia manual (**materialized by later I-56 normative gate**; hasta entonces, Proposal V4 P-14).

## 9. Conformidad final

En READY-06, Architect + Coordinator revisan **todos los arquetipos** sobre el SHA ya rebasado. Comparan
implementacion y evidencia contra Freeze + Freeze delta + A-n + decisiones aprobadas. La conformidad no
rediseña: una mejora queda como seguimiento salvo que revele un invariante violado o riesgo material no
cubierto.

El resultado es `CONFORMING` o `NON-CONFORMING`, con desviaciones exactas. Se clasifican como `EDITORIAL`,
`NON-MATERIAL`, `BEHAVIORAL-WITHIN-FREEZE`, `MATERIAL` u `OWNER-RESERVED`; decide la autoridad del dominio.
NON-CONFORMING impide READY-06. Una EXP-01 A no se acepta como desviacion. Correccion o aceptacion exige
registro versionado y A-n si cambia Freeze; despues de READY-04 crea SHA nuevo y reinicia READY-02.

Un rebase posterior obliga a repetir la conformidad completa sobre el SHA nuevo. Range-diff, patch-id o
igualdad de arbol no trasladan el resultado.

## 10. Referencia sobre repeticion

Una orden cita `ruta + seccion + clausula` de cada regla general que ejerce. No basta «seguir el
workflow». Si el significado depende de Candidato, cierre u otro contexto, la referencia lo nombra.

Toda orden declara:

1. objetivo;
2. alcance especifico;
3. invariantes del Freeze tocados por ID;
4. rutas y archivos calientes relevantes;
5. evidencia requerida y su autoridad;
6. no-touch especifico;
7. condiciones de parada no cubiertas por las referencias;
8. informe esperado.

No copia rutinariamente historia, WORKFLOW completo, ADR completos, politica general de integracion o
del Owner, limpieza ni revisiones del Architect: cita sus IDs. Una contradiccion produce STOP y cita
ambas fuentes; Coordinator resuelve una errata o interpretacion dentro de su dominio y Owner decide
politica.

## 11. Metricas y evaluacion

Cada unidad registra en su archivo de evidencia: arquetipo inicial/final y disparadores; rondas Discovery
y EXP; rondas Architect y modo; gates totales/verificables; Full locales por cierre/Candidato; intentos e
invalidaciones de Candidato; intentos CI y rojos no leidos; rondas Owner de politica/producto y hallazgos;
desviaciones; invalidaciones Freeze; y findings escapados despues del merge. El lugar y formato del
archivo se materializan en `docs/WORKFLOW.md` mediante un gate posterior de I-56.

Solo se registran tiempos medidos por Git/Actions y duracion activa declarada por el Owner. Ausencia es
`UNKNOWN`, nunca fallo ni bloqueo.

Hay dos evaluaciones documentales con orden propia:

1. aproximadamente tras cinco iniciativas conceptuales V2 integradas, evaluacion general; una iniciativa
   multiunidad cuenta al integrar la ultima y agrega las metricas de sus unidades;
2. cuando los tres arquetipos tengan una muestra integrada, evaluacion completa por arquetipo.

La falta de muestra deja conclusiones UNKNOWN y no bloquea iniciativas. Ninguna evaluacion usa «menos
pruebas» como exito. Un aumento de findings escapados obliga a revisar primero la cadencia Core y la
revision proporcional mas cercanas. Las conclusiones se presentan al Owner; estos puntos no son gates
de seguridad ordinarios.

La evaluacion conserva la traza de las hipotesis auditadas:

| Hipotesis | Comprobacion |
|---|---|
| **H1** | Defectos materiales posteriores al Freeze frente a rondas de diseño proporcionales |
| **H2** | Fallos CI que el chequeo por gate no detecto hasta el Candidato |
| **H3** | Defectos visibles solo en Core local intermedio frente a la cadencia por cierre |
| **H4** | Trabajo sobre base superada no detectado en los puntos de re-fetch |
| **H5** | Informacion de cierre que solo existia en una copia secundaria |
| **H6** | Desviaciones de reglas vinculantes que una orden por referencia no nombro |
| **H7** | Entradas FOUNDATIONS contradichas por codigo o ADR posterior en menos de dos iniciativas |
| **H8** | Revalidacion cruzada o fundacion integrada sin prueba de uso por unidades funcionales |
| **H9** | Hallazgos por modo de revision Architect en comparaciones controladas |

## 12. Traza y vigencia

- Fuente de diseño exacta: [I-56 Proposal V4](initiatives/I-56-proposal-v4.md).
- Decision durable aceptada: [ADR-0045](adr/0045-workflow-v2-ciclo-evidencia-e-integracion.md).
- Plantillas subordinadas: [PROMPT_TEMPLATES.md](initiatives/PROMPT_TEMPLATES.md).

```text
WORKFLOW V2 = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
```
