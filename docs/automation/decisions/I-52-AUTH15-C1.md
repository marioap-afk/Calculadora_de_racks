# I-52 AUTH-15-C1 — Registro de decisiones de la unidad correctiva

Unit: `I-52-AUTH15-C1` (iniciativa I-52). Rama: `feature/i52-auth15-c1-unnamed-envelope`. Workflow: V2 (T8-A).

Contrato de la unidad: [I-52-auth15-c1-unnamed-envelope.md](../../initiatives/I-52-auth15-c1-unnamed-envelope.md).

## 1. Autorizacion del Owner

```text
Owner = OPEN UNIT I-52-AUTH15-C1
Purpose = a valid RackEmbedDocument may have a blank logical Name (AUTH-15 Precheck)
Product requirement = RACKPROYECTAR MUST support unnamed racks and keep them unnamed
Consumer = I-55 / G16 / RACKPROYECTAR (blocked on this unit; not modified here)
Ownership = I-52 owns the unit; Architect I-52 owns technical review and Freeze;
            Coordinator I-55 owns only the consumer requirement and the integration-receipt acceptance
Gate = IMPLEMENTATION MUST NOT BEGIN until ARCHITECT REVIEW = AGREED and the corrective contract is frozen
Not a new Shared View Foundation initiative
```

- No es una iniciativa nueva ni de la Foundation. Base: `origin/main` en `3375aadb` (contiene la integracion de AUTH-15 y `integration/I-52-AUTH15`).
- La revocacion de la politica C16-05 de I-55 (`UnnamedRackNotProjectable`) y su reconciliacion son de I-55, posteriores a esta integracion.

## 2. Reclamo y clasificacion Workflow

- **Reclamo.** Commit vacio `141cdaf8`, primer push aceptado sin force, `Claim-Id: b5f18dbd-a34f-4ba5-9146-5ddfcaaf2b27`.
- **Clasificacion: V2, T8-A.** Unidad nueva posterior conceptualmente perteneciente a I-52: reclamo, contrato, Freeze delta y evidencia propios; no hereda el Freeze ni la evidencia de `I-52-AUTH15`.
- **Estado.** Bootstrap hecho (contrato, este registro, fila de ROADMAP). **Implementacion, RED y cambios de produccion: NO iniciados.** HANDOFF no se toca hasta la integracion.

## 3. Hechos verificados para la revision y preguntas para el Architect

Hechos (leidos de `3375aadb`, sin cambio):

- `RackDefinitionCreator.Precheck` valida, en orden: base/transaccion vivas, transaccion superior por identidad nativa (`TransactionMismatch`), plan/dibujante (`InvalidPlan`), `requestedBlockName`
  (`InvalidBlockName`), y sobre nulo o `Id`/`Kind`/`Name` en blanco (`InvalidEnvelope`, texto «sobre ausente o sin Id/Kind/Name»); despues `RackEmbedStore.Serialize` (un fallo es `InvalidEnvelope`).
- `RackEmbedStore.Serialize` solo exige documento no nulo: no valida ni normaliza `Name`. El sobre se escribe con `RackBlockData.Write` y se relee comparando el JSON exacto (`EnvelopeWriteFailed` si difiere).
- El unico llamador de produccion previsto es I-55 (G15/RACKPROYECTAR); AUTH-15 no tiene otro llamador.
- Las pruebas actuales de AUTH-15 son guardas de fuente (ADR-0003) porque `Precheck` toca `Database`/`Transaction`; el comportamiento real se probo en host con un arnes temporal (`eng/research/I52Auth15Host/`,
  ultimo estado `fcca6e6c`, paquete inmutable `D:\I52-AUTH15-HV\fcca6e6c`), retirado del Candidato anterior.

Preguntas (ARCHITECT REVIEW REQUIRED; ninguna se decide sin el Architect):

- **Q1 — Contrato.** ¿Se acepta la tabla de cambio del contrato (solo `Name` deja de ser causa de `InvalidEnvelope`; `Id`, `Kind`, nulo y serializacion no cambian)?
- **Q2 — Texto del diagnostico.** ¿«sobre ausente o sin Id/Kind» para el fallo de precondicion, conservando el mismo tipo `InvalidEnvelope` y sin degradar el diagnostico de `Id`/`Kind`? ¿O diagnosticos separados por campo?
- **Q3 — Forma del RED.** Precheck no es ejecutable sin AutoCAD. Opciones: (a) solo guardas de fuente (aserciones sobre el cuerpo de `Precheck`); (b) extraer un predicado puro interno
  (por ejemplo `EnvelopeIsUsable(RackEmbedDocument)`) que permita RED de comportamiento con `null`/`""`/espacios/`Id`/`Kind` en blanco, a costa de tocar mas que la linea de `Name`; (c) (a) mas validacion en host.
  Recomendacion del implementador: (a) + host, sin extraccion, para mantener el cambio minimo; decide el Architect.
- **Q4 — Alcance de la validacion en host.** ¿Que corrida en host exige el flujo de I-52 para este cambio (reuso del arnes `fcca6e6c` adaptado, en rama y con `src/`+`tests/` byte-identicos al Candidato, o corrida sobre el Candidato)?
  El sobre con `Name` en blanco debe crear definicion, escribir y releer igual; `Id`/`Kind`/BaseName en blanco deben seguir fallando. La caracterizacion SIDE-DB anterior no se reinterpreta.
- **Q5 — Documentos vivos.** ¿Se corrigen en esta unidad los textos que afirmen que `Name` es obligatorio (comentario de `Precheck`, contrato/decisiones de `I-52-AUTH15` solo por nota delta, no reescritura)?
- **Q6 — Suites.** ¿Se exige UI Full ademas de Core Full? El cambio esta en el Plugin; el Plugin se valida por Build Plugin (Debug y Release) y CI 4/4.

## 4. Estado y siguiente paso

Ver §5 (revision del Architect) y §6 (Freeze). Solo despues del commit de Freeze: RED → produccion → validacion → Candidato. No se fabrica ningun PASS.

## 5. Revision del Architect (AUTH-15-C1)

```text
ROLE DECLARATION = SAME-SESSION ROLE
  El paquete (contrato + §3 de este registro) y esta revision los produce la MISMA sesion del agente implementador, por instruccion expresa del Owner
  («You are the independent Architect for I-52-AUTH15-C1»). Revisor y autor NO son independientes; no es SEPARATE SESSION ni EXTERNAL HUMAN.
  El Coordinator sigue revisando el consumo y el recibo; ese control no se sustituye. El Owner puede exigir una re-revision separada; hasta entonces este acuerdo es el vigente.
  Esta sesion NO implementa ni escribe RED en esta ronda.
```

### Ronda R1 — version `c102ed0c` (blob del contrato `da779d4fb13a18a99fe91d0f0ed805e62ad315ec`): CHANGES REQUIRED

- **F-1 (REQUIRED).** La «Superficie propuesta» del contrato limitaba la produccion a `RackDefinitionCreator.cs` (`Precheck`). El comentario XML de `InvalidEnvelope` en
  `RackDefinitionCreationResult.cs:33` («lacks Id/Kind/Name») quedaba fuera y contradecia el contrato corregido; ademas dejaba sin fijar la superficie exacta congelable. Sin cambio de
  significado del comportamiento, pero un elemento congelable incompleto.
- **F-2 (REQUIRED).** El contrato no fijaba obligaciones invariante→prueba con RED esperado ni la matriz de host (INITIATIVE_LIFECYCLE §6): quedaban como preguntas abiertas.
- **F-3 (OPTIONAL).** Declarar por escrito que AUTH-15 lee `Name` en un solo punto: verificado, `RackDefinitionCreator.cs:180` es la unica lectura de `.Name` en el creador y en el resultado.

### Ronda R2 — version `a6a3e96a0860e0bd9196b257f81480a574ae91a9` (`docs/initiatives/I-52-auth15-c1-unnamed-envelope.md`, blob `2f4fa9459d8796f7ef88862dccf5a2ce1b61fdfd`): AGREED

Disposicion: F-1 y F-2 resueltas en esa version; F-3 incorporada (invariante I-2). Cero REQUIRED abiertos sobre la version exacta.

Resolucion de las preguntas (§3):

- **Q1 = AGREED.** Un `Name` en blanco es un estado valido del producto (`RackPhysicalSelection` exige solo `Id`/`Kind`/`Design`; `RackDuplicationPlan` solo `Id`/`Kind`; `RACKLISTA`/`RACKBOMTOTAL` lo muestran «(sin nombre)»;
  `RackEmbedStore.Serialize` solo exige documento no nulo y no valida ni normaliza `Name`). `Id` y `Kind` siguen siendo el requisito estructural. Es el unico cambio semantico.
- **Q2 = AGREED.** «sobre ausente o sin Id/Kind», mismo tipo `InvalidEnvelope`; sin debilitar `Id`/`Kind`.
- **Q3 = AGREED, estrategia de prueba aceptada.** Guardas de fuente sobre el cuerpo real de `Precheck` + host. **Sin predicado puro nuevo** (abstraccion sin valor de producto para una condicion). Limite reconocido: una guarda prueba la ausencia
  de la condicion, no el camino de exito; por eso el host es obligatorio y su veredicto gobierna el exito. Guardas nuevas: I-1, I-2, I-4; deben verse fallar sobre la base.
- **Q4 = AGREED con matriz fijada** (contrato §7, H-1..H-10). Base de documento solamente, controles dentro de la matriz; sin reabrir SIDE-DB/CT-DA (no se encontro dependencia concreta). Se anade H-9 (`OpenCloseTransaction` → `TransactionMismatch`) y H-10 (sin Commit/Abort/referencia).
- **Q5 = AGREED con superficie de TRES ediciones** (contrato §4): condicion de `Name`, texto del diagnostico y comentario XML de `InvalidEnvelope`. La evidencia historica y las notas de `I-52-AUTH15` no se reescriben (no hay contradiccion normativa: sus
  afirmaciones de «nombre no vacio» hablan del `BlockName`, no del `Name` del sobre). Un diff de produccion mayor exige re-revision.
- **Q6 = AGREED.** Candidato: focales de AUTH-15, guardas de propiedad, Core Full, **UI Full obligatoria** (AGENTS.md: el `FINAL_CANDIDATE_SHA` y el cierre documental exigen Core Full y UI Full; LC-UI no reduce el Candidato), Build UI Debug, Build Plugin Debug y Release, CI de push 4/4 exacto,
  cobertura del Candidato; antes de integrar: host y aceptacion Architect/Owner; post-merge: CI exacto y cobertura del merge.

Objeciones materiales: ninguna abierta sobre la version acordada.

**Freeze.** El commit de Freeze cambia UNICAMENTE la linea `Frozen: NO` → `Frozen: YES` del contrato; ninguna clausula ni otra linea de cabecera cambia. Su SHA (`FROZEN CONTRACT SHA`) se registra en §6, porque un commit no puede contenerse a si mismo.

## 6. Freeze

```text
FROZEN CONTRACT SHA = 6e667d02e14182d1fe232e10e53e0d447a6c98e7
Frozen artifact     = docs/initiatives/I-52-auth15-c1-unnamed-envelope.md
Agreed version      = a6a3e96a0860e0bd9196b257f81480a574ae91a9 (blob 2f4fa9459d8796f7ef88862dccf5a2ce1b61fdfd)
Freeze change       = only the line `Frozen: NO` -> `Frozen: YES` (no clause changed)
IMPLEMENTATION AUTHORIZED = YES (RED first; production only after RED is observed)
```

- El contrato es inmutable desde ese commit. Todo cambio posterior es una enmienda `A-n` append-only en este registro (INITIATIVE_LIFECYCLE §6); un diff de produccion materialmente mayor que las TRES ediciones del contrato §4 exige re-revision del Architect.
- Estado: implementacion NO iniciada; sin RED escrito; sin cambios de produccion.

## 7. Implementacion y validacion en host

- **RED `93b93f78`** (4 de 5 guardas fallan sobre la base) y **GREEN / implementacion `7e7e9178`** (las tres ediciones del Freeze). Detalle en [I-52-AUTH15-C1-evidence.md](../evidence/I-52-AUTH15-C1-evidence.md).
- **Corrida 1 (arnes `579de1af`) = HOST RUN VALID / OVERALL RESULT = UNKNOWN.** Defecto del arnes (veredicto de 15 casos fijos con una matriz de 16; HV-06/HV-07/HV-09 en bases laterales). El Owner aprobo la Ruta 1: **no se deriva PASS de ella** y su evidencia se conserva sin editar.
- **Corrida 2 (arnes corregido `b7ee683ecef8efc3e41c8b962fa172846daea614`) = HOST RUN VALID / OVERALL RESULT = PASS**, emitido por el propio arnes y confirmado por el lanzador (30/30 comprobaciones): 16/16 casos, 7/7 controles DOCUMENT, 0 fugas, H-1..H-10 = PASS sobre la base del documento.
- **Produccion sin cambio entre ambas corridas:** `src` `29265f70…` y `tests` `3ea8617c…`, identicos a `7e7e9178`.
- **Excepcion de identidad host → Candidato: NO ratificada.** Se decide tras formar el Candidato comparando los arboles de produccion (ver la evidencia §4). Esta sesion no la auto-ratifica.
- El arnes temporal se retira en un commit propio antes del Candidato.

## 8. Aceptacion del Candidato y cierre

- **Ratificacion (Coordinator/Owner).** Candidato `8f7847ad5114aec9262bfc44012f56bfd1e6f966` ACEPTADO. **Excepcion de identidad host → Candidato: RATIFICADA** (arboles `src` `29265f70…` y `tests` `3ea8617c…` identicos a los probados en host; diferencias limitadas a retiro del arnes, docs, evidencia y `.gitattributes`).
- **Corrida 1 (`579de1af`) sigue siendo VALID / UNKNOWN**: historica, no se reinterpreta ni se borra. La corrida canonica es la 2 (`b7ee683e`, VALID / PASS).
- **Contrato congelado inmutable.** El archivo del contrato no se edita al cerrar (INITIATIVE_LIFECYCLE §6): su linea `Status` describe el momento del Freeze y el estado vivo se lee en el ROADMAP, el HANDOFF, la evidencia y el tag. La orden de cierre listaba ese archivo entre los «minimos a actualizar»;
  no se actualiza porque hacerlo alteraria el blob congelado. No hay enmiendas A-n: ningun elemento congelado cambio.
- **Estado escrito en el cierre.** Conforme a WORKFLOW §4.5.4 el commit de cierre marca la unidad `integrada` antes de que exista el merge. Esa afirmacion la respaldan las compuertas de despues del merge (CI de `main` sobre el `MERGE_SHA`, cobertura y tag). Si alguna fallara, la correccion se hace por la rama
  y por una ronda completa, nunca directamente en `main`.
- **Sin cambios de producto tras el Candidato.** El cierre es solo `docs/`; `src/` y `tests/` no cambian.
