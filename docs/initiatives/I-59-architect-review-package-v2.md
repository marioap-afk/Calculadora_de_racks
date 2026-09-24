# I-59 — Architect review package — Proposal V2

## Objeto exacto de revision

```text
Unit                 = I-59
Archetype            = FOUNDATION EVOLUTION
Branch               = architecture/shared-view-placement-block-facts
Claim-Id             = 9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a
F1_CLOSURE_SHA       = 54319d33810dba54ae3172624604a1e753fe86ae
F1 evidence path     = docs/automation/evidence/I-59-f1-characterization.md
F1 evidence blob     = 1ef85f1eb6591978921497ccd92579ee79c175e9
F1 harness blob      = d98dd9df4e1f49d861dbb28dc4ec192a53ef7827
Proposal V2 SHA      = ca54fca27cf639908d890e940e3d1d5b313780d3
Proposal V2 path     = docs/initiatives/I-59-proposal-v2.md
Proposal V2 blob     = d4949ac420fa66a2b542575a5ee9e7a0d48eaae0
Historic RED commit  = cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b
Frozen               = NO
Architect            = NOT YET INVOKED
F2                   = NOT OPEN
```

Revisar la Proposal V2 completa contra el codigo del mismo SHA, ADR-0044, I-52 y el G14 vigente de I-55.
Este paquete es indice y matriz de ataque; no repite la Proposal. F1 esta cerrado/aceptado y no forma parte
de la decision a reabrir.

## Disposicion de findings Coordinator

| Finding | Estado propuesto en V2 | Clausulas |
|---|---|---|
| CR59-V1-01 | RESOLVED IN DRAFT | P-16 enumera los 16 roles: 13 Required, Pallet OptionalVisual, Annotation/Dimension NotApplicable; futuro/desconocido -> `UnknownSourceRole`, nunca default silencioso. |
| CR59-V1-02 | RESOLVED IN DRAFT | P-17/P-18 fijan la address tipada de `Prepare` como unica autoridad y definen extractor/resultado V2 aditivos, sin romper `IRackBlockRequirementExtractor<TPayload>` V1. |
| CR59-V1-03 | RESOLVED IN DRAFT | P-11 adopta opcion B: no hay `PlanarTransform`/`Transform2D` compuesto V2; determinant/reflection se nombran como hechos de base local XY y los datos 3D quedan separados. |

Estas disposiciones requieren revision Coordinator. No constituyen acuerdo ni veredicto Architect.

## Foco de revision V2

1. La funcion de roles es total para el enum vigente y falla visible ante extension futura.
2. `ClosingHorizontal`, hoy sin productor localizado, se clasifica Required por la ruta block-backed total
   del drawer; evaluar si esa reconstruccion es suficiente sin decision Owner.
3. La address por requirement es exactamente la address ya validada por el contexto de preparacion.
4. Ningun string (`HeaderBlockInstance.View`, BaseName, generated name o UI index) puede adquirir autoridad.
5. El port V1 permanece intacto; V2 añade contexto sin reconstruir identidad desde keys deduplicadas.
6. AUTH-08 V2 no afirma un transform definition-to-world parcial o falso cuando origin/normal son no triviales.

## Matriz de ataques requerida

| ID | Ataque | Clausula/obligacion |
|---|---|---|
| AR59-V2-01 | un role vigente cae en default o queda sin disposicion | P-16 / CT59-11; contar exactamente 16 |
| AR59-V2-02 | enum futuro se clasifica Required/NotApplicable por conveniencia | P-16 / CT59-11; `UnknownSourceRole` conductual |
| AR59-V2-03 | Pallet se vuelve Required por ser block reference o se omite por ser visual | P-16/P-22; OptionalVisual trazable |
| AR59-V2-04 | Annotation/Dimension se consultan como library blocks aunque el drawer crea entidades nativas | P-16/P-22; NotApplicable |
| AR59-V2-05 | ClosingHorizontal sin producer demuestra que la regla block-backed es insuficiente | P-16; emitir REQUIRED/OWNER DECISION si corresponde, no inventar fallback |
| AR59-V2-06 | `HeaderBlockInstance.View` contradice address y gana por parseo | P-17 / CT59-10; debe ganar Prepare address |
| AR59-V2-07 | BaseName/nombre generado/indice UI se convierten en semantica de vista | P-17 / CT59-10; prohibicion cerrada |
| AR59-V2-08 | V2 llama al extractor V1 y pretende recuperar PieceId/address/role perdidos | P-18 / CT59-09/15 |
| AR59-V2-09 | añadir address rompe firma, constructor o resultado V1 | P-18/P-23 / CT59-15 |
| AR59-V2-10 | una key compartida deduplica dos piezas o dos addresses | P-15/P-18/P-22 / CT59-09 |
| AR59-V2-11 | PositionXY+rotation+scale reaparece como transform definition-to-world | P-11 / CT59-05 |
| AR59-V2-12 | determinant/reflection XY se presentan como proyeccion world con Normal inclinada | P-11/P-13 / CT59-05 |
| AR59-V2-13 | `Transform2D` queda prohibido incluso para una operacion 2D exacta o se usa sin probar exactitud | P-02/P-11; frontera proporcional |
| AR59-V2-14 | origin se aliasa con position o se omite pese a G14 | P-09 / CT59-02 |
| AR59-V2-15 | non-uniform inventa media/geometric mean | P-11/P-13 / CT59-04 |
| AR59-V2-16 | signs/half-turn/reflection/negative-Z vuelven a enum combinatorio | P-12 / CT59-06 |
| AR59-V2-17 | NonFinite construye Point3D/Vector3D y lanza antes del outcome | P-08/P-14 / CT59-08 |
| AR59-V2-18 | KeyMissing/FileMissing/BlockMissing/Unknown/LibraryUnavailable colapsan | P-19..22 / CT59-12/13 |
| AR59-V2-19 | query/import se fusionan o pre-import missing manda | P-21 / CT59-14 |
| AR59-V2-20 | V2 rompe I-52, I-55 G8/G9/G12 o Foundation actual | P-23 / CT59-15 |
| AR59-V2-21 | source guard sustituye prueba conductual futura | §12; sabotajes y matrices conductuales |
| AR59-V2-22 | F2 abre sin acuerdos exactos o revision intenta reabrir F1 | §13/§15; STOP |
| AR59-V2-23 | OV N/A oculta una ruta Plugin visible en el diff futuro | §14; revisar antes de READY |

## Respuestas expresas solicitadas

1. Confirmar/corregir DC-01..09, EXP-01..09 y M-01..08.
2. Aceptar/rechazar la funcion total de roles, especialmente `ClosingHorizontal`.
3. Confirmar que un role futuro falla visible y no hereda clasificacion por default.
4. Confirmar `RackViewAddress` de Prepare como unica procedencia y la compatibilidad del seam V2 aditivo.
5. Aceptar/rechazar la opcion B de AUTH-08 y los nombres/semantica de determinant/reflection de base XY.
6. Revisar matrices, failure semantics, compatibility, CT59-01..16, gates y OV.
7. Indicar cualquier expansion Discovery omitida o decision Owner realmente necesaria.

## Formato esperado

```text
Architect Review — I-59 Proposal V2
Reviewed Proposal SHA/path/blob = ca54fca27cf639908d890e940e3d1d5b313780d3 / docs/initiatives/I-59-proposal-v2.md / d4949ac420fa66a2b542575a5ee9e7a0d48eaae0
F1 closure identity = 54319d33810dba54ae3172624604a1e753fe86ae
SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN
Reviewer and author same person = YES | NO
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
AGREED POINTS = ...
DISAGREEMENTS = ...
MATERIAL RISKS = ...
REQUIRED CHANGES = NONE | AR59-V2-XX ...
OPTIONAL IMPROVEMENTS = ...
CR59-V1-01 = RESOLVED | OPEN
CR59-V1-02 = RESOLVED | OPEN
CR59-V1-03 = RESOLVED | OPEN
Discovery/EXP/M = ACCEPTABLE | CHANGES REQUIRED
AUTH-08 = ACCEPTABLE | CHANGES REQUIRED
AUTH-12 = ACCEPTABLE | CHANGES REQUIRED
Compatibility/invariants/gates/OV = ACCEPTABLE | CHANGES REQUIRED
Architect disposition = <to be supplied by the reviewer>
Freeze = DRAFT / NOT FROZEN
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```

Un futuro `AGREED` solo vale para commit/ruta/blob exactos y cero REQUIRED abiertos. Este paquete no
invoca ni suplanta al Architect y no emite acuerdo Coordinator.

```text
F1 RED = ESTABLISHED
F1 CLOSURE = ACCEPTED
Proposal V2 = PUBLISHED / PENDING COORDINATOR REVIEW
Freeze = DRAFT / NOT FROZEN
Architect = NOT YET INVOKED
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```
