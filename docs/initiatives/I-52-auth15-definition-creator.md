# I-52 AUTH-15 — Unidad de integracion: creacion caller-owned de definicion y sobre

Status: IMPLEMENTATION AUTHORIZED; CANDIDATE PENDING ARCHITECT EXACT-SHA REVIEW

Workflow: V2

Initiative: I-52 (RACKMIRROR, ID16). Unit: `I-52-AUTH15`.

Branch: `feature/i52-auth15-definition-creator`

BASE_SHA: `016bf46715cec45e644f88a22ef091b311a2bef1`

CLAIM_SHA: `4b20bda3`

Claim-Id: `1f035b6f-6891-48fd-8965-f7ff1f522767`

Decisiones: [`docs/automation/decisions/I-52-AUTH15.md`](../automation/decisions/I-52-AUTH15.md)

## Por que existe esta unidad

AUTH-15 es una autoridad **propiedad de I-52** y **fuera de la Shared View Foundation** (I-57, I-59 y
[FOUNDATIONS](../FOUNDATIONS.md) la dejan fuera). I-55 la necesita para su G15 (`RACKPROYECTAR`) y,
opcionalmente, para el modo 1 de G9b. El gate de I-55 exige que AUTH-15 este **integrada en `main`** y su
recibo verificado.

La rama propia de I-52 (`feature/rackmirror-espejo-semantico`) lleva la investigacion CT-DA y un producto
bloqueado, y no es integrable. Por eso el Coordinador autorizo esta **segunda unidad de integracion de I-52,
de proposito unico**, cortada de `origin/main`. No absorbe otro trabajo de I-52.

## Autoridad

- **Requisito congelado.** Freeze V17-post-I57 §4 de I-52: «`AUTH-15 = I-52 OWNED / NOT IMPLEMENTED` … Este
  Freeze fija el requisito, no una implementacion». Freeze V18 lo hereda.
- **Mecanica.** Proposal V17 §8.2 de I-52: `CreateInTransaction(db, tx, plan, nombreBloque, payload) →
  definitionId` en la transaccion del llamador, con despacho por familia de plan. Es PROVISIONAL, y la forma
  de la API no la congela I-52 sola.
- **Contrato normativo de esta unidad.** Revision de Arquitecto «AUTH-15 DESIGN / FREEZE REVIEW»: aclaracion
  de contrato dentro del requisito congelado, sin freeze, ADR ni decision de Owner nuevos.

Esos documentos viven en la rama de I-52 (`0d5df48b`) y no son alcanzables desde `main`. Esta unidad los cita;
no los copia ni los modifica.

## Contrato

Dentro de una transaccion que **aporta y posee el llamador**, AUTH-15 crea **exactamente una** definicion de
bloque RackCad nueva a partir de un plan ya preparado de una familia soportada. Persiste en ella el sobre
`RackEmbedDocument` ya compuesto y devuelve la identidad de la definicion creada y su nombre real, o un fallo
tipado.

- **Familias.** HeaderRun (`HeaderRunPlan`, dibujado por `LateralHeaderDrawer.CreateSystemBlock`) y
  Cantilever (`CantileverViewPlan`, por `CantileverViewMaterializer.CreateBlockDefinitionNamed`).
  - Cada familia conserva su politica de unicidad de nombre (I-09: no se unifican).
  - HeaderRun crea, como contenido de la definicion, una definicion anidada por cabecera distinta; esas no
    llevan sobre.
- **Posee el llamador:**
  - el lock, la transaccion, Commit y Abort;
  - Resolve, Prepare, la importacion y la verificacion de biblioteca;
  - la politica de nombre, el RackId, la composicion y el restamp del sobre;
  - la transformacion, la colocacion y presentacion de la referencia y el orden;
  - la semantica de lote;
  - UI y avisos, Regen y Purge;
  - el significado de producto de un fallo.
- **AUTH-15 nunca:**
  - commitea, aborta, abre transaccion, bloquea el documento, importa, regenera ni purga;
  - coloca referencias;
  - toca el RackId, el sobre ni la transformacion;
  - itera varias definiciones ni guarda estado entre llamadas;
  - muestra UI;
  - limpia tras un fallo: todo lo escrito queda en la transaccion del llamador para su rollback.
- **Fallos tipados:** `TransactionMismatch`, `InvalidPlan`, `InvalidBlockName`, `MissingLibraryBlocks` (con las
  instancias; el sobre no se escribe), `InvalidEnvelope`, `EnvelopeWriteFailed` (el sobre se relee tras
  escribirlo) y `WriteFailed`.
- **Desviacion registrada: AUTH15-DEV-01.** `BlockNameUnavailable` no se implemento. Queda pendiente de revision
  del Arquitecto; ver las decisiones.

## Superficie

- **Produccion, solo altas:**
  - `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs`
  - `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreationResult.cs`
- **Pruebas:** `tests/RackCad.Tests/RackDefinitionCreatorGuardTests.cs`, guardas de fuente conforme a ADR-0003.
- **Sin cambios:** los creadores de cada familia, `SystemBlockWriter`, `RackBlockData`, el sobre y su
  composicion, la Foundation (Application/Systems/Shared, FOUNDATIONS) y cualquier comando, UI o flujo de
  producto. AUTH-15 no tiene llamador de produccion hasta que I-52 o I-55 la cableen.

## Fuera de alcance

- el comando RACKMIRROR y cualquier producto de I-52;
- CT-DA;
- la rama `feature/rackmirror-espejo-semantico`;
- I-55 y sus decisiones (incluida la semantica de faltantes del modo 1 de G9b);
- mover AUTH-15 a la Foundation;
- un comando temporal de validacion en AutoCAD.

## Compuertas

1. Revision exacta del Arquitecto sobre el SHA de implementacion, incluida AUTH15-DEV-01.
2. Validacion en host, cuyo vehiculo de prueba debe decidir el Coordinador: todavia no hay llamador ni comando.
3. Candidato, integracion en `main`, verificacion posterior, recibo `integration/I-52-AUTH15` y reconciliacion
   con I-55.
