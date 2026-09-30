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

Unidad abierta. Falta: revision del Architect (paquete = contrato + este registro), Freeze del contrato correctivo, y solo despues RED → produccion → validacion → Candidato. No se fabrica ningun PASS.
