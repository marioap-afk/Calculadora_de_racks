# I-63 — Proposal V3 · Amendment A-2: secuenciación de verificación de INV-11 e INV-32

```text
Amendment          = A-2 (segunda de la secuencia; append-only)
Tipo               = Coordinator-only (INITIATIVE_LIFECYCLE §6: resecuencia obligaciones de prueba entre gates sin cambiar
                     resultados congelados)
Freeze aplicable   = Proposal V3 congelada: docs/initiatives/I-63-proposal-v3.md
                     commit de Freeze f61d0aca859a11b15cbe1797069a83cba873ba95, blob 4d5dedce15363fa6378e460c5b63005fe41b858b
Amendments previas = A-1 docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md (commit 658b35ad498520ba3c832a96eea4389df16dfa60)
Applies-to         = all
Autoridad          = Orden del Coordinator de autorización de G2 (decisiones de I-63 §2, «Autorización de G2 y A-2»)
Origen             = Preguntas Q-G2-01 (INV-32) y Q-G2-02 (INV-11) de la preparación del contrato de G2 (evidencia de I-63 §35.3)
Materialidad       = ninguna de M-01..M-08 (§4)
```

## 1. Qué es y qué no es

- **Es** una enmienda solo del Coordinator que **reparte entre gates** la verificación de dos obligaciones ya congeladas, INV-11 e INV-32,
  porque sus oráculos citan `ProjectSummary` `Full`, que es de G4 (Proposal V3 §14 y §21).
- **No** cambia comportamiento, autoridad, persistencia, semántica de producto, semántica de métricas, semántica de fallo, alcance ni
  arquitectura. No edita la Proposal V3 congelada ni la A-1: se lee junto a ellas.
- No autoriza implementar por sí misma; la autorización de G2 es de la orden del Coordinator.

## 2. Texto autorizado por el Coordinator (literal)

```text
A-2 — secuenciación de verificación INV-11 e INV-32

Freeze aplicable:
I-63 Proposal V3
Freeze f61d0aca...
A-1 658b35ad...

Tipo:
Coordinator-only

Materialidad:
M-01..M-08 = no activados

INV-32:
No cambia el comportamiento congelado.

G2 verifica:
- RackMetricRequest directo -> population counter = 0
- ProjectPopulation sobre el mismo rack -> el mismo counter > 0

G4 completa:
- ProjectSummary Full sobre el mismo rack -> el mismo counter > 0

INV-11:
No cambia el determinismo congelado.

G2 verifica, ante inversión del orden de entrada:
- misma Membership
- mismo RepresentativeDefinitionId
- mismo DisplayName
- misma grafía canónica de RackId
- mismo ProjectPopulation
- mismos agregados materializados por G2

G4 completa:
- mismo ProjectSummary Full

Reason:
verification/gate sequencing only; G2 does not own ProjectSummary Full.

No cambia:
behavior, authority, persistence, product semantics,
metric semantics, failure semantics, scope or architecture.
```

## 3. Deltas

### A-2.1 — INV-32: control positivo por gate

**Cláusula congelada.** INV-32 usa **un solo contador** en el orquestador real de población: `RackMetricRequest` directo → **0**; control
positivo con el mismo contador: una petición encaminada a propósito por `ProjectSummary` (`Full`) para obtener las métricas del mismo rack
→ **> 0**.

**Precisión añadida:**
- **G2:** `RackMetricRequest` directo → contador de población = 0; `ProjectPopulation` sobre el mismo rack → el **mismo** contador > 0.
- **G4:** `ProjectSummary` `Full` sobre el mismo rack → el **mismo** contador > 0 (el control congelado, sin cambio).

### A-2.2 — INV-11: determinismo por gate

**Cláusula congelada.** INV-11: hermana no colocada y divergente → `Excluded(SiblingsDivergent)`; con el orden de entrada invertido, mismo
representante, `DisplayName`, grafía y `ProjectSummary`.

**Precisión añadida:**
- **G2**, ante inversión del orden de entrada: misma `Membership`, mismo representante (`RepresentativeDefinitionId`), mismo `DisplayName`,
  misma grafía canónica de RackId, mismo `ProjectPopulation` y mismos agregados materializados por G2.
- **G4:** mismo `ProjectSummary` `Full` (el oráculo congelado, sin cambio).

**Por qué no cambia nada congelado:** las reglas de D-11, D-17, D-20 e INV-32 son las mismas; solo se fija en qué gate se observa cada parte
de los oráculos, según el gate que materializa cada tipo.

## 4. Obligaciones afectadas y gates

| INV | Gate | Lo que añade A-2 |
|---|---|---|
| INV-11 | G2 (población) y G4 (`ProjectSummary` `Full`) | A-2.2: parte de G2 sobre `ProjectPopulation`; parte de G4 sobre `Full` |
| INV-32 | G2 (`ProjectPopulation`) y G4 (`ProjectSummary` `Full`) | A-2.1: control positivo de G2 con `ProjectPopulation`; el de `Full` en G4 |

## 5. Materialidad

| ID | Estado | Razón |
|---|---|---|
| M-01..M-08 | **No activados** | Solo se resecuencia la verificación de comportamiento ya congelado entre G2 y G4. No cambia quién posee una regla, ningún DTO, el comportamiento observable, la semántica de fallo, contratos consumidos, puntos de extensión, mecanismos ni ADR |

Por eso basta con el Coordinator (LIFECYCLE §6). No necesita Architect ni Owner.

## 6. Evidencia

- Orden del Coordinator de autorización de G2 y A-2: decisiones de I-63 §2.
- Preguntas de origen: evidencia de I-63 §35.3 (Q-G2-01 y Q-G2-02).
