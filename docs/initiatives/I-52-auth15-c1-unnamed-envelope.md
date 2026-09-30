# I-52 AUTH-15-C1 — Unidad correctiva: el sobre de AUTH-15 admite un Name logico en blanco

Status: RECLAMADA Y BOOTSTRAPEADA; **ARCHITECT REVIEW REQUIRED**; contrato NO congelado; **implementacion NO iniciada** (sin RED, sin cambio de produccion); NOT INTEGRATED

Workflow: V2 (clasificacion T8-A: unidad nueva posterior, conceptualmente de I-52; reclamo, contrato, Freeze delta y evidencia propios; no hereda V1)

Initiative: I-52 (RACKMIRROR, ID16). Unit: `I-52-AUTH15-C1`.

Branch: `feature/i52-auth15-c1-unnamed-envelope`

BASE_SHA: `3375aadbbf929427a6d106b2fff275d64863b89c`

CLAIM_SHA: `141cdaf8` (commit vacio; primer push aceptado sin force)

Claim-Id: `b5f18dbd-a34f-4ba5-9146-5ddfcaaf2b27`

Decisiones: [`docs/automation/decisions/I-52-AUTH15-C1.md`](../automation/decisions/I-52-AUTH15-C1.md)

Consumidor: I-55 / G16 / `RACKPROYECTAR` (bloqueado en esta unidad; no se toca desde aqui).

## Por que existe esta unidad

`RackDefinitionCreator.Precheck` rechaza con `InvalidEnvelope` un sobre cuyo `Name` es nulo, vacio o solo espacios. Pero un `RackEmbedDocument` con `Name`
en blanco es un estado **valido y existente** del producto: `RACKLISTA` y `RACKBOMTOTAL` lo muestran «(sin nombre)», la identidad y el agrupamiento de racks son por
`RackId`/`Kind`, y las rutas de insercion pasan el nombre a `RackEmbedComposer` sin exigirlo. `RACKPROYECTAR` agrega otra vista ligada del MISMO `RackId`; no debe renombrar
un rack sin nombre solo para satisfacer a AUTH-15. Decision de producto del Owner: `RACKPROYECTAR` DEBE soportar racks sin nombre y conservarlos sin nombre.

Son dos autoridades independientes y AUTH-15 no las confunde:

- `RackEmbedDocument.Name` = nombre logico/visible del rack. **Puede estar vacio.**
- `requestedBlockName` / BaseName = nombre fisico de la definicion de bloque (AUTH-11). **No puede estar vacio** (`InvalidBlockName`).

## Cambio de contrato (unico cambio semantico)

| Condicion del sobre | Antes | Despues |
|---|---|---|
| `envelope == null` | `InvalidEnvelope` | `InvalidEnvelope` |
| `Id` nulo/vacio/solo espacios | `InvalidEnvelope` | `InvalidEnvelope` |
| `Kind` nulo/vacio/solo espacios | `InvalidEnvelope` | `InvalidEnvelope` |
| `Name` nulo/vacio/solo espacios | `InvalidEnvelope` | **valido** (camino de exito) |
| fallo de serializacion | `InvalidEnvelope` | `InvalidEnvelope` |

`Name` no se sustituye ni se normaliza: lo que el llamador compuso es lo que `RackEmbedStore.Serialize` escribe y lo que se relee. Sin respaldo, sin «Rack», sin «Sin nombre»,
sin copiar el BaseName al `Name`.

## Superficie propuesta (sujeta a Freeze)

- **Produccion:** solo `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs`, `Precheck`: quitar `|| string.IsNullOrWhiteSpace(envelope.Name)` y el texto del diagnostico
  («sobre ausente o sin Id/Kind/Name» → redaccion coherente con el contrato: «sobre ausente o sin Id/Kind»). Ninguna otra rama.
- **Comentarios/docs:** el contrato de [I-52-auth15-definition-creator.md](I-52-auth15-definition-creator.md) y su registro de decisiones NO se reescriben (fuente de la unidad anterior, ya integrada);
  este contrato es el delta. Se actualizan solo los textos vivos que afirmen que `Name` es obligatorio.
- **Pruebas:** `tests/RackCad.Tests/RackDefinitionCreatorGuardTests.cs` (guardas de fuente conforme a ADR-0003; hoy fija el precheck) y, si el Architect lo aprueba, una prueba de comportamiento
  (ver «Preguntas para el Architect»). RED antes de cualquier cambio de produccion.
- **Sin cambios:** propiedad del documento/lock/transaccion, `StartTransaction`, `OpenCloseTransaction` (no soportada), Commit/Abort, colocacion de referencias, RackId, BaseName, Resolve, Prepare,
  semantica de lote, politica de producto, Foundation, esquema, DTOs, `RackEmbedStore`/`RackEmbedDocument`, los creadores de familia, y todo I-55.

## Lo que AUTH-15 sigue sin hacer (a probar sin cambio)

No commitea, no aborta, no abre transaccion, no bloquea el documento, no coloca `BlockReference`, no elige RackId, no elige `Name` logico, no elige BaseName, no compone el sobre, no deriva
BaseName y no posee politica de producto ni semantica de lote. Escribe el sobre YA compuesto que le entrega el llamador.

## Evidencia exigida (V2 / orden del Owner)

- RED (antes de produccion): `Name` = null, `""` y solo espacios → NO `InvalidEnvelope`; se conservan `envelope` nulo, `Id` en blanco, `Kind` en blanco y fallo de serializacion → `InvalidEnvelope`;
  `requestedBlockName` en blanco → `InvalidBlockName`; transaccion distinta → `TransactionMismatch`; plan invalido → `InvalidPlan`.
- Con `Name` en blanco: `CreateInTransaction` → definicion creada → sobre escrito → la relectura es igual al JSON serializado suministrado (el `Name` en blanco sobrevive sin respaldo).
- Pruebas focales de AUTH-15, guardas, serializacion/relectura, transaccion del llamador; validacion en host segun el flujo de I-52 (el Architect fija el alcance); Core Full; UI Full si el flujo lo exige;
  Build UI; Build Plugin; CI exacto del Candidato; cobertura del Candidato; **sin skips nuevos**.
- Candidato exacto `FINAL_CANDIDATE_SHA` solo tras RED/GREEN completo, validacion local verde, arbol limpio y docs vigentes. Validacion del Owner segun el flujo de I-52; **no se fabrica PASS**.
- Integracion en `main` en serie: cierre documental, merge `--no-ff`, CI post-merge sobre el SHA exacto, verificacion de cobertura del Candidato, limpieza y tag anotado `integration/I-52-AUTH15-C1`
  que pele al `MERGE_SHA`. Una rama sin integrar NO desbloquea I-55.

## Preguntas para el Architect (revision requerida antes de implementar)

Ver el registro de decisiones §3.

## Fuera de alcance

- I-55 y su reconciliacion (quitar `UnnamedRackNotProjectable`, el servicio `LogicalName`; conservar C16-04): unidad y rama propias del Coordinador de I-55, **despues** de esta integracion.
- RACKMIRROR y el resto del producto de I-52, CT-DA, la rama `feature/rackmirror-espejo-semantico`.
- Cualquier cambio de la Foundation, del esquema, de DTOs o de la politica de nombres de bloque.
