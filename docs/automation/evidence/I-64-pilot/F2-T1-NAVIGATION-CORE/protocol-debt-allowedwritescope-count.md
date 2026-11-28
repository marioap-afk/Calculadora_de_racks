# I-64 — F2-T1: deuda de protocolo «AllowedWriteScope (2 entradas)» en el prompt de verificación

Registro ordenado por el Coordinator en la disposición «I-64 — F2-T1 FINAL nc2 RETRY / A-4 DISPOSITION»
(literal en `coordinator/coordinator-f2t1-nc2-0706-a4-disposition.md`).

**Clasificación del Coordinator:** PROTOCOL / PROMPT DEBT.

## Hecho

El generador del prompt normativo de verificación de F2-T1 escribe el número de entradas de la delegación REAL. La línea de Scope dice
literalmente:

```text
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (2 entradas) y ForbiddenWriteScope (17 entradas) de la
  delegacion.
```

- README §10 fija el mismo prompt para los controles negativos; solo cambian la ruta de entrada y el RunId.
- En nc2 el esquema de delegación permite que el `AllowedWriteScope` mutado tenga otro número de entradas. En F2-T1 la mutación de
  nc2 tiene 4 entradas: `NavigationFacts.cs`, `NavigationTargetSet.cs`, `NavigationTargets.cs` y `tests/RackCad.Tests/Workspace/`.
- **Motivo del Coordinator:** el recuento fijo «(2 entradas)» es frágil y puede inducir a error al Controller.

## Observación, sin afirmar causalidad

Con el mismo texto:
- nc2 `R20261109T032830Z-fa85` y nc2 `R20261109T042615Z-5b90` evaluaron Scope contra las 4 entradas de la delegación mutada (Scope en
  fail, solo `NavigationActions.cs` fuera).
- nc2 `R20261109T043701Z-0706` evaluó Scope contra los 2 prefijos del contrato (Scope en pass): el defecto histórico de nc2, que el
  Coordinator clasificó como CONTROL FAIL. Lo suplió I64-SCOPE-BRIDGE-01 `R20261109T044628Z-34cd` (PASS).

## Órdenes del Coordinator

- No modificar el prompt normativo durante la cadena actual de F2-T1.
- No reabrir 0706 por esta deuda.
- Entregarla a I-62 / evolución del protocolo cuando la cadena actual se cierre.

## Defectos del Controller en la misma cadena (clasificados por el Coordinator; para el mismo destino)

| Run | Defecto | Clasificación |
|---|---|---|
| verificación `R20261108T185002Z-0e1c` | contó el filtro `FullyQualifiedName~Workspace` por el nombre visible y distinguiendo mayúsculas (726 frente a 724) y paró con S-04 | INVALID_OUTPUT (SCHEMA_VALID_BUT_MATERIALLY_FALSE) |
| nc1 `R20261108T222313Z-39c9` | evaluó el `CurrentSha` mutado dentro de Handoff (es criterio de Identity); recuento por nombre visible (727/676) | INVALID_OUTPUT (SCHEMA_VALID_BUT_ORACLE_INVALID) |
| nc2 `R20261109T032830Z-fa85` | transcribió `CurrentSha` con 34 caracteres en sus órdenes y en la Evidence de Identity y Remote; recuento por nombre visible | INVALID_OUTPUT (SCHEMA_VALID_BUT_MATERIALLY_FALSE) |
| nc2 `R20261109T042615Z-5b90` | atribuyó al Worker, en Termination, la hora de fin de la cesión de planificación (18:24:25Z) | INVALID_OUTPUT (SCHEMA_VALID_BUT_MATERIALLY_FALSE) |
| nc2 `R20261109T043701Z-0706` | evaluó Scope contra el `AllowedWriteScope` del contrato en lugar del de la delegación mutada | CONTROL FAIL / HISTORICAL CONTROLLER SCOPE DEFECT REPRODUCED |
