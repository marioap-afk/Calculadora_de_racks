# I-52 AUTH-15-C1 — Unidad correctiva: el sobre de AUTH-15 admite un Name logico en blanco

Status: CONTRATO ACORDADO POR EL ARCHITECT (ver decisiones §5); implementacion NO iniciada hasta el commit de Freeze; NOT INTEGRATED

Frozen: YES

Workflow: V2 (clasificacion T8-A: unidad nueva posterior, conceptualmente de I-52; reclamo, contrato, Freeze delta y evidencia propios; no hereda V1)

Initiative: I-52 (RACKMIRROR, ID16). Unit: `I-52-AUTH15-C1`.

Branch: `feature/i52-auth15-c1-unnamed-envelope`

BASE_SHA: `3375aadbbf929427a6d106b2fff275d64863b89c`

CLAIM_SHA: `141cdaf8` (commit vacio; primer push aceptado sin force)

Claim-Id: `b5f18dbd-a34f-4ba5-9146-5ddfcaaf2b27`

Decisiones: [`docs/automation/decisions/I-52-AUTH15-C1.md`](../automation/decisions/I-52-AUTH15-C1.md)

Consumidor: I-55 / G16 / `RACKPROYECTAR` (bloqueado en esta unidad; no se toca desde aqui).

Este archivo es el **Freeze delta** de la unidad: una vez congelado es inmutable (INITIATIVE_LIFECYCLE §6); todo cambio posterior es una enmienda `A-n` en el registro de decisiones.

## 1. Por que existe esta unidad

`RackDefinitionCreator.Precheck` rechaza con `InvalidEnvelope` un sobre cuyo `Name` es nulo, vacio o solo espacios. Pero un `RackEmbedDocument` con `Name` en blanco es un estado
**valido y existente** del producto: `RACKLISTA` y `RACKBOMTOTAL` lo muestran «(sin nombre)», la identidad y el agrupamiento de racks son por `RackId`/`Kind`
(`RackPhysicalSelection` solo exige `Id`, `Kind` y `Design`; `RackDuplicationPlan` solo exige `Id`/`Kind`), y las rutas de insercion pasan el nombre a `RackEmbedComposer` sin exigirlo.
`RACKPROYECTAR` agrega otra vista ligada del MISMO `RackId`; no debe renombrar un rack sin nombre solo para satisfacer a AUTH-15. Decision de producto del Owner: `RACKPROYECTAR`
DEBE soportar racks sin nombre y conservarlos sin nombre.

Son dos autoridades independientes y AUTH-15 no las confunde:

- `RackEmbedDocument.Name` = nombre logico/visible del rack. **Puede estar vacio.**
- `requestedBlockName` / BaseName = nombre fisico de la definicion de bloque (AUTH-11). **No puede estar vacio** (`InvalidBlockName`).

## 2. Autoridad congelada

AUTH-15 sigue siendo propiedad de I-52, fuera de la Shared View Foundation. Su propiedad NO cambia: crea **exactamente una** definicion RackCad, persiste el sobre YA compuesto
que le entrega el llamador y opera dentro de la transaccion del llamador. **Nunca:** bloquea el documento, abre transaccion, hace Commit ni Abort, coloca `BlockReference`,
elige RackId, elige el `Name` logico, elige el BaseName, compone el sobre, hace Resolve ni Prepare, ni posee politica de producto o semantica de lote.
Quien elige el `Name` logico es el llamador (I-55 lo conserva tal como esta en el rack fuente).

## 3. Comportamiento observable congelado

| Condicion del sobre | Antes | Despues |
|---|---|---|
| `envelope == null` | `InvalidEnvelope` | `InvalidEnvelope` |
| `Id` nulo/vacio/solo espacios | `InvalidEnvelope` | `InvalidEnvelope` |
| `Kind` nulo/vacio/solo espacios | `InvalidEnvelope` | `InvalidEnvelope` |
| `Name` nulo | `InvalidEnvelope` | **valido** (camino de exito) |
| `Name` vacio | `InvalidEnvelope` | **valido** |
| `Name` solo espacios | `InvalidEnvelope` | **valido**, conservado tal cual |
| fallo de serializacion | `InvalidEnvelope` | `InvalidEnvelope` |
| `requestedBlockName` en blanco | `InvalidBlockName` | `InvalidBlockName` (sin cambio) |
| transaccion no superior / ajena | `TransactionMismatch` | sin cambio |
| plan o dibujante ausente | `InvalidPlan` | sin cambio |

- **Sin respaldo ni normalizacion.** AUTH-15 no lee, sustituye, recorta ni genera el `Name`: lo que el llamador compuso es lo que `RackEmbedStore.Serialize` escribe y lo que se relee.
  No hay «Rack», «Sin nombre», «Selectivo», ni el BaseName copiado al `Name`.
- **Diagnostico.** El fallo de precondicion del sobre conserva el tipo `InvalidEnvelope` y dice: «sobre ausente o sin Id/Kind». No debilita el diagnostico de `Id` ni de `Kind` y no da a entender que `Name` sea requerido.
- **Persistencia.** Ninguna: el formato del sobre, el esquema y los DTO no cambian. Un `Name` en blanco se persiste como el JSON que el llamador serializo y la relectura lo compara igual
  (`EnvelopeWriteFailed` solo si difiere).
- **Compatibilidad/legacy.** Los sobres existentes con `Name` en blanco ya son validos para el resto del producto; nada se migra ni se repara.

## 4. Superficie congelada

- **Produccion — exactamente TRES ediciones:**
  1. `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs`, `Precheck`: quitar `|| string.IsNullOrWhiteSpace(envelope.Name)`.
  2. `RackDefinitionCreator.cs`, `Precheck`: el diagnostico `"sobre ausente o sin Id/Kind/Name"` pasa a `"sobre ausente o sin Id/Kind"`.
  3. `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreationResult.cs`: el comentario XML de `InvalidEnvelope` («lacks Id/Kind/Name») deja de afirmar que `Name` es requerido.
  Cualquier diff de produccion materialmente mayor exige re-revision del Architect.
- **Pruebas:** guardas de fuente en `tests/RackCad.Tests/RackDefinitionCreatorGuardTests.cs` (§6). Sin abstraccion de produccion nueva: **no** se extrae un predicado puro para volver testeable esta correccion de una condicion.
- **Evidencia historica inmutable:** `docs/automation/evidence/I-52-AUTH15-run3/hostval-evidence.json` y los documentos de la unidad `I-52-AUTH15` no se reescriben; este contrato es su delta.
- **Sin cambios:** propiedad del documento/lock/transaccion, `StartTransaction`, `OpenCloseTransaction` (sigue sin soportarse), Commit/Abort, colocacion de referencias, RackId, BaseName, Resolve, Prepare,
  semantica de lote, politica de producto, Foundation, esquema, DTOs, `RackEmbedStore`/`RackEmbedDocument`/`RackEmbedComposer`, los creadores de familia, y todo I-55.

## 5. No-objetivos y puntos de extension

- No-objetivos: I-55 y su reconciliacion (quitar `UnnamedRackNotProjectable` y el servicio `LogicalName`; conservar C16-04; actualizar las pruebas de I-55 que fijan `Name` en la precondicion de AUTH-15,
  p. ej. `G16_ID19_TheAuth15PreconditionIsExactlyIdKindAndName`): unidad propia del Coordinador de I-55, **despues** de esta integracion. RACKMIRROR y el resto del producto de I-52, CT-DA,
  la rama `feature/rackmirror-espejo-semantico`. Cualquier cambio de Foundation, esquema, DTOs o politica de nombres de bloque. Reabrir la caracterizacion SIDE-DB/CT-DA.
- Puntos de extension: ninguno.

## 6. Obligaciones invariante → prueba (RED esperado)

Estrategia aceptada por el Architect: **guardas de fuente + validacion en host**. `Precheck` depende de `Database`/`Transaction` de AutoCAD; las guardas prueban la AUSENCIA de la condicion, y solo el host
prueba el camino de exito. Las guardas leen el cuerpo real de `Precheck` con el ayudante `Body(...)` existente.

| ID | Invariante | Prueba | RED sobre `3375aadb` |
|---|---|---|---|
| I-1 | `Precheck` no lee `Name` | el cuerpo de `Precheck` no contiene `envelope.Name` | FALLA (lo contiene) |
| I-2 | AUTH-15 nunca lee el `Name` | el codigo de `RackDefinitionCreator.cs` (sin comentarios) no contiene `envelope.Name` ni `.Name` del sobre en ninguna otra parte | FALLA |
| I-3 | `Id`, `Kind` y sobre nulo siguen validados | `Precheck` contiene `envelope == null`, `IsNullOrWhiteSpace(envelope.Id)` y `IsNullOrWhiteSpace(envelope.Kind)` → `InvalidEnvelope` | pasa (se conserva) |
| I-4 | diagnostico coherente | `Precheck` contiene `"sobre ausente o sin Id/Kind"` y ni el creador ni `RackDefinitionCreationResult.cs` contienen `Id/Kind/Name` | FALLA |
| I-5 | `requestedBlockName`, transaccion y plan no cambian | las guardas existentes de `Precheck` (`InvalidBlockName`, `TransactionMismatch`, `InvalidPlan`, orden) siguen verdes sin editarse | pasa |
| I-6 | el sobre se escribe sin tocarlo y se relee | las guardas existentes de serializacion/relectura y de `Envelope(...)` siguen verdes sin editarse | pasa |
| I-7 | propiedad sin cambio | las guardas existentes que prohiben Commit/Abort/`StartTransaction`/`LockDocument`/colocacion en AUTH-15 siguen verdes sin editarse | pasa |

Las guardas I-1, I-2 y I-4 son las unicas nuevas; deben verse FALLAR sobre la base antes de tocar produccion (RED) y pasar despues (GREEN). Las existentes no se debilitan ni se editan para que pasen.

## 7. Matriz de validacion en host (obligatoria)

Alcance: **base de datos de documento**, con `LockDocument` y `StartTransaction` del llamador. No se incluyen sondas de base lateral: la caracterizacion SIDE-DB anterior no se reinterpreta ni se reabre.
Las filas de control gobiernan el veredicto: una corrida con un control fallido es FAIL o INVALID segun corresponda, nunca PASS.

| ID | Entrada | Resultado esperado |
|---|---|---|
| H-1 | sobre valido, `Name = null` | exito; definicion creada; nombre real no vacio |
| H-2 | sobre valido, `Name = ""` | exito |
| H-3 | sobre valido, `Name = "   "` | exito |
| H-4 | H-1..H-3 | el JSON releido de la definicion == el JSON serializado suministrado; el `Name` deserializado es exactamente el suministrado (sin recorte ni respaldo) |
| H-5 | sobre con `Id` en blanco | `InvalidEnvelope`, sin escritura |
| H-6 | sobre con `Kind` en blanco | `InvalidEnvelope`, sin escritura |
| H-7 | sobre nulo | `InvalidEnvelope`, sin escritura |
| H-8 | `requestedBlockName` en blanco (con sobre valido) | `InvalidBlockName`, sin escritura |
| H-9 | transaccion sin ser la superior de la base (p. ej. `OpenCloseTransaction`) | `TransactionMismatch`, sin escritura (`OpenCloseTransaction` sigue sin soportarse) |
| H-10 | tras cada exito | la transaccion del llamador sigue viva y sin Commit; no hay `BlockReference` nuevo; un Abort o Dispose sin Commit del llamador descarta la definicion (base de documento) |

Reglas de la evidencia de host: el arnes es TEMPORAL, vive solo en `eng/research/I52Auth15C1Host/`, y su commit tiene `src/` y `tests/` byte-identicos a la implementacion validada (los arboles se
registran); se retira antes del Candidato (el diff del Candidato contra la base no contiene `eng/`); se registran ProductVersion y SHA-256 del binario probado, version de AutoCAD y biblioteca.
Una diferencia de identidad entre el binario del arnes y el del Candidato es una excepcion que el Owner debe ratificar por escrito; **no se fabrica PASS**.

## 8. Plan de gates (un solo gate funcional, G1)

Resultado verificable de G1: AUTH-15 acepta un `Name` en blanco y conserva todo lo demas.

1. RED: las guardas I-1, I-2 e I-4 fallan sobre la base (seleccion mayor que cero; evidencia registrada), antes de cualquier cambio de produccion.
2. GREEN: las tres ediciones de produccion; las guardas nuevas y las existentes de AUTH-15 pasan; **sin skips nuevos**.
3. Local sobre el arbol limpio del Candidato: pruebas focales de AUTH-15, guardas de propiedad, Core Full, **UI Full** (AGENTS.md: el `FINAL_CANDIDATE_SHA` exige Core Full y UI Full), Build UI Debug,
   Build Plugin Debug y Build Plugin Release.
4. Host (§7) sobre la implementacion validada, con evidencia por identidad.
5. `FINAL_CANDIDATE_SHA` exacto: CI de push 4/4 sobre ese SHA y cobertura del Candidato con `measured-sha.txt` igual al Candidato.
6. Aceptacion del Architect y del Owner segun Workflow V2; despues cierre documental (con su Core/UI Full y CI), merge `--no-ff` en serie, CI post-merge sobre el `MERGE_SHA`, verificacion de cobertura,
   limpieza y tag anotado `integration/I-52-AUTH15-C1` que pele al `MERGE_SHA`. Una rama sin integrar NO desbloquea I-55.

## 9. Recibo para I-55 (tras la integracion)

`AUTH15_C1_INTEGRATED = YES`; `MAIN = <MERGE_SHA>`; `AUTH15_NAME_CONTRACT = blank logical RackEmbedDocument.Name is valid`; `BASE_NAME_CONTRACT = requested block BaseName remains nonblank`;
`OWNERSHIP = unchanged`.
