# Prompt Templates for Initiative Workflow V2

> **Materialized for I-56; not effective until WORKFLOW_V2_EFFECTIVE_SHA exists.**
>
> Estas plantillas son procedimientos subordinados a
> [INITIATIVE_LIFECYCLE.md](../INITIATIVE_LIFECYCLE.md),
> [ADR-0045](../adr/0045-workflow-v2-ciclo-evidencia-e-integracion.md) y
> [Proposal V4](I-56-proposal-v4.md) y, para §G, a [AUTOMATION_PLAN.md](../AUTOMATION_PLAN.md) §16. No crean politica ni activan Workflow V2.

## 1. Uso

Sustituir cada marcador y citar siempre `ruta + seccion + clausula`. Las referencias a `WORKFLOW`,
`AGENTS` y la guia manual señalan texto **materialized by later I-56 normative gate**; hasta entonces se
cita ademas la clausula correspondiente de Proposal V4. Si dos fuentes se contradicen: STOP, citar ambas
y volver a la autoridad competente.

Las plantillas evitan copiar historia, normas completas, ADR completos, politica Owner, integracion o
revisiones previas. Los findings se citan por ID estable. Ninguna plantilla autoriza Quick CI, T0-T4,
R0-R4, merge automatico, evidencia por igualdad de arbol ni reduccion de Full/Owner Validation.

## A. Coordinator bootstrap / intake

```text
I-NN — BOOTSTRAP / INTAKE
Objetivo e IDs: <necesidad observable>; agrupacion provisional: <ambas condiciones, evidencia/UNKNOWN>
Arquetipo provisional: <EXTENSION|FOUNDATION EVOLUTION|NEW ARCHITECTURE>; M-01..08: <resultado>
Autorizacion: <Owner/fila ROADMAP>; workflow determinado por transicion, nunca elegido
Preflight y reclamo: docs/WORKFLOW.md <clausulas; materialized by later I-56 normative gate>
Contrato: alcance, enlaces, Consumes/Extends/Introduces, coordinacion; sin evidencia duplicada
Discovery: INITIATIVE_LIFECYCLE §§3-4, DC-01..09 y EXP-01..09 con negativos razonados
Revision: Coordinator revisa DC/EXP omitidos antes de confirmar agrupacion, materialidad y arquetipo
No-touch: <ramas/rutas>
STOP: reclamo existente; interseccion funcional; EXP pendiente; A abierta; M UNKNOWN; pausa activa;
      orden temporal ambiguo; contradiccion de fuentes
Informe: base/reclamo/Claim-Id segun WORKFLOW; clasificacion; archivos; DC/EXP; revision requerida
```

## B. Executor functional gate

La orden ordinaria apunta a unas 15-40 lineas cuando el trabajo lo permite.

```text
I-NN — G<n>: <objetivo verificable>
Alcance: <F-x..F-y>; no-objetivos: <lista>
Freeze/delta: <ruta e identidad>; invariantes tocados: <IDs>; A-n aplicables: <IDs>
Rutas/hotspots: <lista>; ultima observacion de paralelas: <referencia>
Normas: INITIATIVE_LIFECYCLE §§6-8; WORKFLOW <sesion/orden Git, materialized later>;
        AGENTS <RED, seleccion >0, evidencia, materialized later>
Evidencia requerida: <focal/relevante/Full/build/CI/Owner, con autoridad exacta>
Orden de cierre: <referencia a WORKFLOW/AGENTS; no copiar la mecanica>
No-touch: <rutas y Freeze>
STOP adicional: CI anterior sin leer/verde; interseccion cambiada; M nueva; EXP-01 A;
                MATERIAL/OWNER-RESERVED; contradiccion; <condiciones propias>
Diagnostico: ante rojo leer logs/TRX/volcados antes de proponer correccion
Informe: SHA; seleccion >0; resultados/tiempos medidos; evidencia exacta; findings/desviaciones
```

No pegar rutinariamente WORKFLOW, ADR, historia de iniciativa, politica Owner, politica de integracion
ni prosa de una revision anterior. Citar cada finding por ID.

## C. Architect design / re-review

```text
I-NN — ARCHITECT REVIEW <ronda>
Review mode: <SAME-SESSION ROLE|SEPARATE SESSION|EXTERNAL HUMAN>; revisor=autor: <si/no>
Entrada: version completa actual <commit/ruta/blob>; delta <referencia>;
         findings previos y disposicion <IDs>; Discovery/revision Coordinator <referencias>
Foco minimo: delta. Alcance disponible: version completa; el delta no limita la revision.
Comprobar: INITIATIVE_LIFECYCLE §§3-6; M; invariantes/pruebas/OV; REQUIRED y oraculos.
Cada finding: <ID; elemento; razon; evidencia; REQUIRED|OPTIONAL>.
Una rebaja solo la emite su autor con razon y evidencia versionadas.

AGREED POINTS
<lista>
DISAGREEMENTS
<lista>
MATERIAL RISKS
<lista>
REQUIRED CHANGES
<lista>
OPTIONAL IMPROVEMENTS
<lista>
CONSENSUS STATUS
<AGREED|CHANGES REQUIRED|BLOCKED — OWNER DECISION>, ligado a commit/ruta/blob
```

Un hallazgo material en una seccion sin cambios sigue siendo valido.

## D. Conformance review

```text
I-NN — CONFORMANCE <SHA>
Revisores: Architect + Coordinator; Review mode: <modo>; revision completa
Fuentes: Freeze <ref> + Freeze delta <ref> + A-n aplicables <IDs> + decisiones aprobadas <refs>
Integridad: INITIATIVE_LIFECYCLE §6 y READY-09; OV/asignaciones: READY-08
Traza: <invariante -> codigo -> prueba/guarda>; confirmar oraculo capaz de fallar
Comprobar no-objetivos, puntos de extension y FOUNDATIONS consumidas; EXP-01 A = ninguna
Resultado: CONFORMING | NON-CONFORMING
Desviaciones exactas: <ID; clase; fuente; implementacion/evidencia; autoridad que decide>
Tras READY-04, cambio/aceptacion/A-n: versionar y reiniciar READY-02
```

La conformidad compara contra fuentes ya aprobadas. No rediseña la iniciativa.

## E. Integration handoff / report

Los hashes y hechos se escriben en la futura autoridad de evidencia/tag, no en HANDOFF. Esta plantilla
solo enumera campos compactos y sus referencias; no cambia la politica vigente de HANDOFF.

```text
Initiative: <I-NN>; Unit: <unidad>; Workflow: <V1|V2>; Status: <estado>
BASE / CLAIM / FINAL_CANDIDATE / CLOSURE / MERGE: <referencias a evidencia/tag futuro>
Final-main / verified merge identity: <referencia durable>
delivered contracts: <Freeze/delta/A-n/decisiones>
Evidence reference: <archivo de evidencia; materialized by later I-56 normative gate>
Owner decisions: <IDs/refs>; Owner Validation: <IDs/veredicto/ref>
ADR: <refs/estado>; product areas: <lista>; follow-ups: <IDs/refs>
Cleanup: <estado/ref>; unverified prior merge rounds: <refs|none>
Integration tag: <ref al formato/verificacion de WORKFLOW, materialized later>
```

La corrida de un tag no acredita evidencia de rama, Candidato o post-merge. El reporte referencia la
evidencia exacta definida por WORKFLOW/AGENTS en un gate posterior; no inventa sus campos aqui.

## F. Return to Candidate after main moved

```text
I-NN — RETURN TO CANDIDATE AFTER MAIN MOVED
Disparador: main avanzo despues del cierre; ronda afectada: <evidence ref>
Normas: INITIATIVE_LIFECYCLE §8 READY-02..09;
        docs/WORKFLOW.md <ruta R e integracion; materialized by later I-56 normative gate>;
        AGENTS.md <exact-SHA/evidencia; materialized by later I-56 normative gate>;
        Proposal V4 P-12 ruta R hasta esa materializacion
Objetivo: preservar la ronda anterior y producir nuevo Candidato sobre base actual
Alcance/no-touch: <lista>; no reusar evidencia ni cierre por igualdad de arbol
Secuencia operativa: ejecutar por referencia la ruta R; no copiarla ni abreviarla
STOP: cierre mezclado con producto; trabajo no preservado; conflicto material; A/Owner pendiente;
      READY falso/UNKNOWN; evidencia exacta ausente
Informe: ronda invalidada; nueva base/Candidato/cierre por referencias; conformidad; evidencia; estado
```

## G. Delegación de ejecución

Prompt de un participante delegado = **contrato base** (G.1) + **perfil** (G.2) + **delta** (los campos del paquete de delegación, renderizados). Reglas en
[AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §16; procedimiento en [docs/automation/agent-execution/](../automation/agent-execution/README.md). El prompt compuesto no supera 200 líneas y se
guarda como `prompt.md` con su SHA-256.

### G.1 Contrato base de delegación

```text
I-NN — DELEGACION <TaskId> — <Worker|Controller>, perfil <PROFILE>
Identidad: unidad <unit>; gate <gate>; tarea <TaskId>; intento <Attempt>; RunId <RunId>;
           DelegationRunId <id>; AuthorityRevision <sha>; MainSha <sha>; BaseSha <sha>;
           rama <branch>; worktree <path>; Owner <kind:id>
Autoridades (leer desde Git, no copiar): <ruta + seccion + clase> por cada una;
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: <objetivo de la delegacion>
Alcance: permitido <AllowedWriteScope>; prohibido <ForbiddenWriteScope>
Invariantes del Freeze: <ids>
Rutas y archivos calientes: <rutas relevantes y calientes>
Criterios de aceptacion: <lista>
Evidencia requerida: pruebas <proyecto + filtro + minimo seleccionado + RED esperado>;
           cada corrida filtrada demuestra seleccion > 0; <evidencia esperada>
No-touch: <lista especifica>; nada fuera del alcance permitido
Condiciones de parada: <ids S-xx, P-xx y del contrato>; ante cualquiera, parar sin
           continuar y devolver el bloqueo estructurado en la entrega
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» en el texto libre
Trailer: Co-Authored-By de quien ejecuta, nunca de otro participante
Informe esperado: la salida exacta de su esquema en <ruta de la entrega o -o>;
           Worker: commit y push antes de la entrega, y terminar tras escribirla;
           Controller: solo lectura, sin escribir en Git
```

### G.2 Perfiles

Cada perfil añade solo instrucciones de método; ninguno repite cláusulas normativas.

#### ROUTINE_IMPLEMENTATION

```text
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.
```

#### DEBUGGING

```text
Metodo: reproducir, aislar, explicar y solo entonces corregir.
1. Reproduce el fallo con una prueba o una orden; registra la salida.
2. Formula la causa con evidencia (lineas, valores, commits), no con suposiciones.
3. Si corriges: prueba de regresion vista fallando antes del arreglo, y despues verde.
4. Si la causa queda incierta: entrega PARTIAL con lo medido y las hipotesis
   descartadas; no fuerces un arreglo.
```

#### LONG_HORIZON_IMPLEMENTATION

```text
Metodo: progreso incremental con estado en archivos.
1. Divide el trabajo en pasos verificables; escribe el plan en el area transitoria.
2. Tras cada paso: pruebas focales, commit y nota de progreso.
3. Si el contexto se agota o cambia la base: deja el estado y entrega PARTIAL.
4. No amplies el alcance aunque aparezcan mejoras; registralas como hallazgos.
```

#### ARCHITECTURE_REVIEW

```text
Metodo: revision adversarial contra las autoridades citadas, sin escribir.
1. Lee el Freeze, los documentos dueños y el codigo afectado desde la revision fijada.
2. Cada hallazgo: ubicacion (archivo:linea), defecto, autoridad que contradice y
   correccion minima.
3. Severidad: REQUIRED solo si contradice una autoridad o un hecho medido, deja algo
   congelado ambiguo, impide ejecutar un paso o deja una garantia sin verificar.
4. No reabras lo ya resuelto; prefiere pocos hallazgos de alta confianza.
```

#### CHARACTERIZATION

```text
Metodo: describir el comportamiento actual sin cambiarlo.
1. Escribe pruebas que fijan lo que hoy ocurre, incluidos los casos borde.
2. Las pruebas pasan sobre el codigo actual; si una falla, el hallazgo es el fallo.
3. Registra lo observado como hecho medido y lo inferido como inferencia.
```

#### DOCUMENTATION

```text
Metodo: documento breve que cita y no copia.
1. Cita las autoridades por ruta y seccion; no reproduzcas sus clausulas.
2. Separa hechos medidos de inferencias y marca lo desconocido como UNKNOWN.
3. Sigue el estilo del documento que editas (idioma, tablas, longitud de linea).
```

#### CONTROLLER_PLANNING

```text
Metodo: clasificar y enrutar dentro del contrato de gate, sin escribir en Git.
1. Lee el contrato, docs/automation/agent-execution/routing.md y el catalogo.
2. Clasifica la tarea, puntua las siete dimensiones y elige entre las celdas del
   contrato el nivel mas bajo adecuado; registra RoutingReason con fechas.
3. Copia del contrato el alcance, los invariantes, las pruebas y las paradas; nunca
   los amplies. Nombra los archivos nuevos exactos.
4. Rellena el esquema de delegacion completo; null solo donde el esquema lo admite.
```

#### CONTROLLER_VERIFICATION

```text
Metodo: verificar contra Git y los registros, sin escribir.
1. Evalua las 14 comprobaciones en su orden fijo con ordenes de git reales
   (rev-parse, merge-base, diff --name-only, log, check-ignore, show).
2. Cada comprobacion: pass, fail o not_run, con la evidencia concreta.
3. Rellena Classification, Disposition y FailureClass segun AUTOMATION_PLAN 16.9,
   mirando todas las comprobaciones, no una sola.
4. Contrasta la CI y los conteos del registro de relevo con el diff; no te fies de
   lo que declare la entrega.
```

## 2. Traza y vigencia

- Autoridad de lifecycle: [INITIATIVE_LIFECYCLE.md](../INITIATIVE_LIFECYCLE.md) §§1 y 10.
- Decision aceptada: [ADR-0045](../adr/0045-workflow-v2-ciclo-evidencia-e-integracion.md).
- Fuente aprobada: [Proposal V4](I-56-proposal-v4.md) P-16, P-18 y P-24.
- Fuente aprobada de §G: Freeze de I-61 ([Proposal V9](I-61-proposal-v9.md)) y [AUTOMATION_PLAN.md](../AUTOMATION_PLAN.md) §16.

```text
WORKFLOW V2 = EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = merge normativo de I-56, derivado según WORKFLOW.md §11.2
```
