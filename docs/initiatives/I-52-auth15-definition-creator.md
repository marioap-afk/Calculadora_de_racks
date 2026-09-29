# I-52 AUTH-15 — Unidad de integracion: creacion caller-owned de definicion y sobre

Status: POST-RUN-2 CORRECTION (effective-name postcondition) PENDING ARCHITECT EXACT-SHA REVIEW; HOST VALIDATION = RUN-1 INVALID (harness defect), RUN-2 VALID FAIL (rollback leaks unresolved, empty effective name confirmed); RUN-3 REQUIRED; CANDIDATE PENDING HOST PASS

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
- **Transaccion.** La base de datos destino y la transaccion deben estar vivas, y la transaccion debe ser la
  transaccion superior de esa base. La identidad es la nativa (`UnmanagedObject`), no la del wrapper gestionado:
  `TopTransaction` devuelve un wrapper nuevo en cada lectura. Una `OpenCloseTransaction` no se admite.
- **Fallos tipados (lista normativa):** `TransactionMismatch`, `InvalidPlan`, `InvalidBlockName`, `InvalidEnvelope`,
  `MissingLibraryBlocks`, `EnvelopeWriteFailed` y `WriteFailed`. `BlockNameUnavailable` no forma parte del
  contrato (AUTH15-DEV-01, aceptada; ver las decisiones).
- **Clases de fallo:**
  - PRE-WRITE, sin escritura garantizada: `TransactionMismatch`, `InvalidPlan`, `InvalidBlockName` e
    `InvalidEnvelope`.
  - POST-WRITE, el llamador debe hacer rollback: `MissingLibraryBlocks` (el sobre no se escribe),
    `EnvelopeWriteFailed` (el sobre se relee tras escribirlo) y `WriteFailed`.
- **Aclaraciones:**
  - `InvalidBlockName` cubre el nombre nulo, vacio o solo espacios, antes de escribir. Un nombre no vacio que la
    politica de la familia vuelve invalido (por ejemplo `<>` en HeaderRun, que la familia reduce a `""`) es `WriteFailed`
    POST-ESCRITURA: AUTH-15 comprueba el nombre EFECTIVO que devolvio la familia y no lo da por exito si esta vacio. AUTH-15 no
    anade ni duplica politica de nombres: solo LEE el nombre efectivo.
  - Un exito devuelve SIEMPRE un `BlockName` real y no vacio.
  - `InvalidPlan` significa plan (o dibujante) ausente. Un plan no nulo mal formado, que debia llegar ya preparado,
    puede fallar durante la creacion como `WriteFailed`.
  - `MissingInstances` lleva una entrada por cada par distinto (BlockName|View), tal como las emite el creador
    HeaderRun existente; no cada ocurrencia fisica.

## Superficie

- **Produccion, solo altas:**
  - `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs`
  - `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreationResult.cs`
- **Pruebas:** `tests/RackCad.Tests/RackDefinitionCreatorGuardTests.cs`, guardas de fuente conforme a ADR-0003,
  incluidos los cuerpos exactos de los escritores delegados de cada familia, `LayerHelper.EnsureLayer` y
  `RackBlockData.Write`/`Read`.
- **Sin cambios:** los creadores de cada familia, `SystemBlockWriter`, `RackBlockData`, el sobre y su
  composicion, la Foundation (Application/Systems/Shared, FOUNDATIONS) y cualquier comando, UI o flujo de
  producto. AUTH-15 no tiene llamador de produccion hasta que I-52 o I-55 la cableen.

## Fuera de alcance

- el comando RACKMIRROR y cualquier producto de I-52;
- CT-DA;
- la rama `feature/rackmirror-espejo-semantico`;
- I-55 y sus decisiones (incluida la semantica de faltantes del modo 1 de G9b);
- mover AUTH-15 a la Foundation;
- un comando de validacion en AutoCAD como parte del producto: el arnes de validacion en host es TEMPORAL, vive solo en
  `eng/research/I52Auth15Host/` (README incluido), no es API de producto y se revierte antes de Candidate.

## Compuertas

1. Revision exacta del Arquitecto: `fed44e56` = CHANGES REQUIRED (B-1, corregida con C-1..C-5); re-revision del delta sobre
   `2d10de70` = APPROVED.
2. Validacion en host mediante un arnes temporal en esta misma rama (decision del Coordinador). Regla vinculante: `src/` y
   `tests/` del commit del arnes son byte-identicos a `2d10de70` (el script de construccion lo verifica y la evidencia registra
   los arboles). Estado: paquete construido, NO ejecutado; la ejecucion requiere orden propia del Coordinador.
3. Candidato, integracion en `main`, verificacion posterior, recibo `integration/I-52-AUTH15` y reconciliacion
   con I-55.
