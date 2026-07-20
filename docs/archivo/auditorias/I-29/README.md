# Archivo de auditoria — I-29

> **Archivo historico; no es fuente operativa vigente.** Para el estado actual consultar la
> [decision final del Owner](../../../automation/decisions/I-29.md), el
> [contrato canonico de I-29](../../../initiatives/I-29-licencia-procedencia-autocad-ci.md) y
> [ADR-0003](../../../adr/0003-referencias-autocad-para-ci.md).

## Proposito y alcance

Este directorio conserva la evidencia documental P1-P4 que sustento la decision interna sobre
licencia y procedencia de referencias AutoCAD para CI. Incluye el paquete de decision, el registro
de entrega, la hoja de revision y la matriz de evidencia; no sustituye las fuentes normativas ni el
estado integrado del repositorio.

## Corte y decision final

- **Fecha de corte:** 2026-07-20.
- **Decision final:** B. Aprobado con restricciones.
- **Naturaleza:** aceptacion interna de riesgo para RackCad y uso interno; no es una conclusion
  juridica ni una afirmacion de autorizacion expresa de Autodesk.

I-13 fue integrada y cerrada despues de la elaboracion de estos documentos. ADR-0003 fue aceptado
posteriormente y aplica la excepcion limitada y revocable a la politica cero NuGet. La promocion
concluida no amplia las restricciones, el alcance ni el riesgo residual registrados aqui.

## Documentos archivados

- [Paquete de decision interna](I-29-paquete-decision-interna.md): evidencia historica del caso,
  usos evaluados, guardas y rollback.
- [Registro de entrega y revision](I-29-registro-entrega-revision.md): evidencia historica de
  entrega, receptor, roles y aprobacion.
- [Hoja de revision interna](I-29-hoja-revision-interna.md): papel de trabajo P3, incluidas las
  quince propuestas y la recomendacion preliminar D.
- [Matriz maestra de evidencia y evaluacion](I-29-matriz-evidencia-evaluacion.md): auditoria de
  fuentes, revalidacion, composicion mixta, limites y evaluacion P3/P4.

## Advertencia y regla de consulta futura

Las afirmaciones sobre ADR-0003 propuesto, I-13 abierta o bloqueada, excepcion cero NuGet no activa,
merge pendiente, ramas de promocion o trabajo futuro reflejan el corte historico. No deben leerse
como estado actual. Para una decision, cambio material o revision se consulta primero la decision
canonica y ADR-0003; este archivo solo aporta trazabilidad.

## Trazabilidad

| Hito | Commit |
|---|---|
| Reclamo | `715d473721d216b55b21fc4aa80eea13da218371` |
| P1 — Paquete | `530a517fc8fb3c57cee20834c216f4f78b259dcf` |
| Indice P1 | `195cc8b26e58e191eeb4c3f5af8fa325ad43a77d` |
| P2 — Entrega | `056f9c0129b7483e8131e02eed3dfc4d6d4f78ea` |
| P3 — Evaluacion | `af45dc27387791868a5d013d8b571717b66b9da9` |
| P4 — Decision final | `2fa1d5b9716a601eea3d6f0fd8d9e90658c29fbf` |
