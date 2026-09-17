# Prompt Templates for Initiative Workflow V2

> **Materialized for I-56; not effective until WORKFLOW_V2_EFFECTIVE_SHA exists.**
>
> Estas plantillas son procedimientos subordinados a
> [INITIATIVE_LIFECYCLE.md](../INITIATIVE_LIFECYCLE.md),
> [ADR-0045](../adr/0045-workflow-v2-ciclo-evidencia-e-integracion.md) y
> [Proposal V4](I-56-proposal-v4.md). No crean politica ni activan Workflow V2.

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

## 2. Traza y vigencia

- Autoridad de lifecycle: [INITIATIVE_LIFECYCLE.md](../INITIATIVE_LIFECYCLE.md) §§1 y 10.
- Decision aceptada: [ADR-0045](../adr/0045-workflow-v2-ciclo-evidencia-e-integracion.md).
- Fuente aprobada: [Proposal V4](I-56-proposal-v4.md) P-16, P-18 y P-24.

```text
WORKFLOW V2 = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
```
