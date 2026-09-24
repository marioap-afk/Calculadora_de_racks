# I-59 — Architect review package — Proposal V1

## Objeto de revision

```text
Unit       = I-59
Archetype  = FOUNDATION EVOLUTION
Branch     = architecture/shared-view-placement-block-facts
Claim-Id   = 9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a
Proposal source SHA = 7efb585bb83f62f7e7aa2a62681f7f1f25408893
Proposal   = docs/initiatives/I-59-proposal-v1.md
Proposal blob = e56e0207ca32fe9a6b12ea9f76a1154dd84d99e0
Evidence   = docs/automation/evidence/I-59-f1-characterization.md
Evidence blob = 1ef85f1eb6591978921497ccd92579ee79c175e9
Corrected harness blob = d98dd9df4e1f49d861dbb28dc4ec192a53ef7827
Historic RED commit = cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b
Frozen     = NO
F2         = NOT OPEN
```

Revisar la Proposal completa contra el codigo del mismo SHA, ADR-0044, I-52 y el G14 vigente de I-55 en
`origin/feature/creacion-de-vistas`. Este paquete es indice y lista de ataques; no repite la Proposal.

## Correcciones F1 incluidas

- CR59-F1-01: non-uniform conserva ejes/determinant observables y failure V1; V2 propone ejes + clasificacion,
  sin `UniformScale = sqrt(6)`.
- CR59-F1-02: se retiraron scans de cualquier property/enum del namespace. El harness activo apunta a
  contracts V1 concretos; obligaciones V2 quedan RED documental hasta que el Freeze defina la API.
- CR59-F1-03: se retiraron nombres combinatorios. Unit, uniform, reflection, half-turn, sign/negative-Z son
  hechos independientes. El mapping Beam/Pallet/Annotation/Dimension queda explicitamente como propuesta.

El RED historico no fue reescrito: commit, conteo y fuente recuperable estan en la evidencia.

## Ataques requeridos

| ID | Ataque | Clausula objetivo |
|---|---|---|
| AR59-01 | un double no finito intenta construir Point3D/Vector3D y lanza antes del outcome neutral | P-08/P-14, CT59-07 |
| AR59-02 | non-uniform se colapsa a media, geometric mean o scalar uniforme | P-11/P-13, CT59-04 |
| AR59-03 | half-turn vuelve a ser enum combinatorio o reescribe angulo/signos | P-11/P-12, CT59-03/05 |
| AR59-04 | determinant/reflection ignora signos XY o confunde negative-Z con reflection planar | P-11/P-13, CT59-05 |
| AR59-05 | NormalIsWorldZ o degeneracy usa tolerancia implicita/inconsistente | P-10/P-14, CT59-06/07 |
| AR59-06 | Position y DefinitionOrigin se aliasan o origin se incluye sin demostrar consumidor | P-09, DC-05, CT59-02 |
| AR59-07 | V2 rompe `RackTransformFacts` V1 y cambia I-52 antes de migracion | P-02/P-21, CT59-13 |
| AR59-08 | extractor deduplica requirements y pierde PieceId/view | P-15/P-20, CT59-08 |
| AR59-09 | Required+blank se omite, se trimmea o llega al importer | P-17/P-20, CT59-09 |
| AR59-10 | tabla Beam/Pallet/Annotation/Dimension se presenta como historia en vez de decision | P-16, CT59-10 |
| AR59-11 | FileMissing, BlockMissing, KeyMissing, Unknown y LibraryUnavailable se colapsan o duplican policy | P-18/P-19, CT59-11 |
| AR59-12 | query/import se fusionan o pre-import missing queda autoritativo | P-19, CT59-12 |
| AR59-13 | contrato V2 aditivo no puede coexistir con consumidores Foundation/I-55/I-52 | P-21, DC-04/05, CT59-13 |
| AR59-14 | un scope guard reemplaza prueba conductual o el plan compila oraculos contra nombres imaginarios | §11, EXP-05 |
| AR59-15 | F2 puede abrir sin ambos acuerdos exactos o sin F1/CI revisado | §12/§14 |
| AR59-16 | OV N/A oculta una ruta Plugin visible realmente modificada | §13 |

## Respuestas expresas solicitadas

1. Confirmar/corregir DC-01..09, EXP-01..09 y M-01..08; identificar una expansion omitida.
2. Aceptar/rechazar el carrier primitivo pre-finitud y los outcomes NonFinite/Degenerate/InvalidTolerance.
3. Aceptar/rechazar hechos AUTH-08 independientes y la regla `NormalizePi(-pi)=pi`.
4. Confirmar que la evidencia I-55 G14 justifica DefinitionOrigin separado de ReferencePosition.
5. Aceptar/rechazar la tabla de roles propuesta, sin tratarla como hecho historico.
6. Aceptar/rechazar la decision: congelar KeyMissing y BlockMissing; no congelar LibraryUnavailable como
   valor neutral duplicado.
7. Confirmar que V2 aditiva preserva I-55 G8/G9/G12, I-52 y contratos Foundation vigentes.
8. Revisar invariantes/RED, failure semantics, gates y propuesta OV N/A.

## Formato de respuesta esperado

```text
Architect Review — I-59 Proposal V1
Reviewed SHA = <sha exacto>
Proposal path/blob = <ruta> / <blob>
Evidence path/blob = <ruta> / <blob>
SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN
Reviewer and author same person = YES | NO
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
AGREED POINTS = ...
DISAGREEMENTS = ...
MATERIAL RISKS = ...
REQUIRED CHANGES = NONE | AR59-V1-XX ...
OPTIONAL IMPROVEMENTS = ...
Discovery/EXP/M = ACCEPTABLE | CHANGES REQUIRED
AUTH-08 = ACCEPTABLE | CHANGES REQUIRED
AUTH-12 = ACCEPTABLE | CHANGES REQUIRED
Compatibility = ACCEPTABLE | CHANGES REQUIRED
Invariants/gates/OV = ACCEPTABLE | CHANGES REQUIRED
Architect = AGREED | PENDING
Freeze = DRAFT / NOT FROZEN
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```

Un `AGREED` solo vale para commit/ruta/blob exactos y con cero REQUIRED abiertos. Este paquete no emite
un veredicto en nombre del Architect ni del Coordinator.

```text
F1 RED = ESTABLISHED
F1 CLOSURE = PENDING COORDINATOR REVIEW
Freeze = DRAFT / NOT FROZEN
Architect = PENDING
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```
