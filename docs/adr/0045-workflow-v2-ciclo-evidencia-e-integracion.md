# ADR-0045: Workflow V2 — Initiative lifecycle, evidence and integration governance

- **Estado:** **aceptado**
- **Fecha:** 2026-09-15
- **Decisor:** Owner del repositorio
- **Iniciativa relacionada:** I-56 — Initiative Workflow V2

## Status

**ACCEPTED.** El Owner acepto explicitamente este ADR real el 2026-09-15, tras revisar su
materializacion en el commit `5da6feef2b528aaa44ed5ce581f976ea63251609` y el blob
`0a74817ae1d48bb8a7f7ada8dfd6abeb173878af`. La aceptacion confirma la decision durable de Proposal
V4; no activa Workflow V2 ni asigna `WORKFLOW_V2_EFFECTIVE_SHA`.

## Contexto

El flujo V1 produjo Discovery repetido, ciclos reiterados de propuesta y revision, micro-gates por
archivo o helper, suites Full repetidas, Candidatos prematuros, evidencia y documentacion duplicadas,
ordenes sobredimensionadas y hechos posteriores al merge sin un registro durable. La
[Evidence Audit](../initiatives/I-56-evidence-audit.md) midio esos problemas; la
[Proposal V4](../initiatives/I-56-proposal-v4.md) los convirtio en un diseño completo. La
[revision independiente del Architect](../initiatives/I-56-architect-review-v4.md) acordo esa version,
el [dry-run historico](../initiatives/I-56-dry-run-v4.md) fue PASS y el
[Owner aprobo la propuesta](../automation/decisions/I-56.md) con sus diecisiete selecciones.

Hace falta un registro breve y durable de la decision de proceso, separado de la especificacion
operativa extensa. Este ADR la registra; no sustituye a Proposal V4 ni a las normas que todavia deben
materializarla.

## Decision

Workflow V2 se materializa con estas decisiones:

1. **Arquetipos.** Cada iniciativa se trata como Extension, Foundation Evolution o New Architecture.
2. **Discovery enfocado.** Toda iniciativa produce DC-1..DC-9 y solo expande EXP cuando se activa su
   disparador. Un dato requerido no determinable queda `UNKNOWN` y falla cerrado.
3. **Reutilizacion de fundaciones.** `docs/FOUNDATIONS.md` sera un registro descriptivo. Coordinator y
   Architect verifican cada entrada contra su fuente autoritativa, codigo y pruebas; el registro no
   crea autoridad normativa propia.
4. **Modelo del Architect.** La revision anterior al Freeze es proporcional al arquetipo y a sus
   disparadores; cada version completa permanece visible y la re-revision se enfoca en el delta sin
   blindar el resto. `REQUIRED` obliga. Architect y Coordinator realizan la conformidad final.
5. **Consensus Freeze.** El acuerdo se conserva en un artefacto Freeze inmutable. Los cambios posteriores
   se expresan mediante enmiendas append-only `A-n`.
6. **Gates funcionales.** Los gates cierran resultados de comportamiento verificables, no cambios
   aislados de DTO, helper o archivo.
7. **Cadencia de pruebas.** Core Full corre localmente al cerrar cada gate funcional. El Candidato final
   conserva toda la evidencia Full exigida por V1. No se introducen Quick CI, T0-T4 ni R0-R4.
8. **READY.** `READY-01`..`READY-09` se satisfacen en orden antes de fijar
   `FINAL_CANDIDATE_SHA`.
9. **Owner Validation.** Una matriz aditiva asigna escenarios a unidades; todos los escenarios aplicables
   se ejecutan sobre `FINAL_CANDIDATE_SHA`.
10. **Identidad de evidencia.** La identidad sigue siendo el SHA exacto. Igualdad de arbol no identifica
    evidencia ni binarios.
11. **Superficies documentales.** El proceso separa contrato mutable, Freeze inmutable, archivo de
    evidencia y tag durable de integracion posterior al merge.
12. **Referencia sobre repeticion.** Las ordenes citan con precision las fuentes normativas en vez de
    copiar politica general.
13. **Autoridad por dominio.** Cada regla tiene un unico dueño normativo segun su dominio; las demas
    superficies enlazan esa autoridad.
14. **Transicion.** El reclamo formal propio determina la version de workflow. Se adopta T8-A: I-49,
    I-52 e I-55 conservan solo sus unidades ya reclamadas; una unidad nueva posterior a la activacion
    usa V2. Los reclamos grandfathered no se reescriben retroactivamente.
15. **Activacion.** Antes de la integracion normativa y antes del paso V1 4.5.1 comienza la pausa
    obligatoria de reclamos seleccionada por el Owner. Los reclamos nuevos ordinarios quedan pausados;
    solo caben excepciones de emergencia o fix aprobadas explicitamente por el Owner. La pausa termina
    despues de completar la verificacion V1 4.5.6 y 4.5.7 y registrar durablemente su fin; un bloqueo
    prolongado vuelve al Owner. V2 se hace efectiva solo en el merge normativo definido por el
    mecanismo de transicion aprobado.
16. **Registro posterior al merge.** Tags anotados `integration/*` conservan los hechos de integracion y
    se protegen contra actualizacion y eliminacion.
17. **Activacion coherente.** El conjunto normativo V2 se integra y activa como una sola politica. No
    existe activacion parcial.

## Salvaguardas preservadas explicitamente

- Se conserva la validacion Full del Candidato.
- Se conserva Owner Validation donde aplique.
- La politica de cobertura no cambia.
- Se conserva identidad por SHA exacto.
- Se conserva la verificacion posterior al merge.
- No hay T0-T4.
- No hay R0-R4.
- No hay Quick CI.
- No hay merges automaticos.
- No hay rollback automatico.
- El CI disparado por tags no acredita evidencia de Candidato, rama ni post-merge.

## Alternativas consideradas

- **Mantener Workflow V1 sin cambios** — conserva la repeticion y los huecos medidos por la auditoria.
- **Modelos T0-T4 o R0-R4** — agregan una clasificacion cuyo retorno y seguridad no estan demostrados.
- **Quick CI** — reduce una compuerta sin resolver el costo dominante ni demostrar seguridad.
- **Core Full solo para el Candidato** — retira la retroalimentacion local exigida al cierre funcional.
- **Merge automatico** — elimina el control humano de una integracion deliberadamente serializada.
- **Reutilizar evidencia por igualdad de arbol** — confunde contenido con identidad del binario; RackCad
  estampa el SHA.
- **Registrar el post-merge con otro commit** — crea una recursion: ese commit tambien requeriria hechos
  posteriores a su propio merge.
- **Registros de integracion sin proteccion o mutables** — no preservan de forma durable el hecho publicado.

ADR-0033 no se reabre: sus reglas vigentes por otras fuentes se preservan y esta decision agrega el
ciclo completo de iniciativas.

## Consecuencias

**Positivas.** Se reduce Discovery repetido, rondas redundantes de revision, gates estructurales,
ejecuciones Full repetidas y duplicacion de documentacion y prompts. Las descripciones verificadas de
fundaciones pueden reutilizarse y los hechos post-merge quedan durables.

**Costos y riesgos aceptados.** Freeze y `A-n` requieren disciplina formal. Los controles siguen siendo
manuales hasta que exista tooling futuro. La transicion de activacion es compleja y depende de la
proteccion de tags. Si `main` avanza despues del cierre puede haber retrabajo. La eficacia de arquetipos
y revision proporcional se seguira evaluando empiricamente.

## Alcance y supersesion

Este ADR gobierna iniciativas futuras solo despues de que Workflow V2 sea efectivo. Las iniciativas
grandfathered terminan con el workflow bajo el que fueron reclamadas; I-56 tambien permanece bajo V1.
Este ADR no activa V2, no asigna `WORKFLOW_V2_EFFECTIVE_SHA` y no cambia por si mismo ninguna norma.
Los detalles normativos viviran en los documentos materializados desde Proposal V4.

## Referencias

- [I-56 — Evidence Audit](../initiatives/I-56-evidence-audit.md).
- [I-56 — Workflow V2 Proposal V4](../initiatives/I-56-proposal-v4.md).
- [I-56 — Architect Delta Review independiente de Proposal V4](../initiatives/I-56-architect-review-v4.md).
- [I-56 — Dry-run historico de Proposal V4](../initiatives/I-56-dry-run-v4.md).
- [I-56 — Owner Decision on Workflow V2 Proposal V4](../automation/decisions/I-56.md).
