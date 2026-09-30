# I-55 — G2I post-I-57 integration reconciliation

Fecha: 2026-09-21
Alcance: documentacion y reconciliacion; ninguna implementacion de ID17, ID18 o ID19.

## 1. Objetos exactos

```text
CURRENT_MAIN = 7097057cf8685bf5ecc09083cba37379d4a4aae8
PRE_REBASE_TIP = c20173cb254030c6abfcb0018a279a693fce812c
ARCHIVE_TAG = archive/i-55-pre-i57-integration-c20173c
POST_REBASE_BASE_TIP = 1b29f227f1ecb040a99dce428e26aadd09f303f6
PROPOSAL_V5_PUBLICATION = f49671e29c6cc817166c720fe3f975deb92d4c3b
PROPOSAL_V5_BLOB = 38bee23a3a886a5026664bfda6d260ba5c65fc26
IMPLEMENTATION_MAP_V5_BLOB = 17f6969ad2272dca5530d8533b6cbfd2afc05d23
ADR_0042_BLOB = a5088a3aa1422e081abc612cea793379c2fba3bc
R3_BLOB = cd42db03becff42f98b047e61c46689c17a69670
ADR_0044_BLOB = 30eacca0dc43263100ee03b0cd3972b175a879b9
I52_REMOTE_TIP = 766841f8f9d5d29f807966d6796cd08c2363676d
```

El tag de archivo es anotado y apunta exactamente a `PRE_REBASE_TIP`. El rebase conserva 19 commits propios antes y
despues. `range-diff` empareja los 19 commits; solo marca como distintos los commits que tocaron `ROADMAP` o el indice
de ADR porque esos conflictos exigieron conservar simultaneamente el estado integrado de `main` y la historia de I-55.

Conflictos resueltos:

- `docs/ROADMAP.md`: se conservaron todas las iniciativas de `main`, I-57 integrada y la fila historica viva de I-55.
- `docs/adr/README.md`: se conservaron ADR-0042, ADR-0044, ADR-0045 y los ADR posteriores ya integrados.

Proposal V1..V5, mapa V5 y ADR-0042 no fueron reescritos. Proposal V5, mapa V5, R3 y ADR-0044 conservan los blobs
indicados arriba.

## 2. Recibo `integration/I-57`

| Comprobacion | Resultado |
|---|---|
| Nombre | `integration/I-57` |
| Tipo | annotated tag |
| Peeling | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Ancestro de `origin/main` | YES |
| `FINAL_CANDIDATE_SHA` en mensaje | `419bf7d82569bc0740db3db39bf6b3ee8fd788d5` |
| `CLOSURE_SHA` en mensaje | `e925c916863aca73cbe261f921b08877a5edbee7` |
| `MERGE_SHA` en mensaje | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Post-merge CI | `35641803734 / SUCCESS` |
| Candidate coverage | `35642390360`, measured SHA = Candidate |
| R3 | `EFFECTIVE / UNCHANGED`, blob exacto |

Resultado: **I-57 INTEGRATED / VERIFIED / CONSUMABLE**. El `Integration SHA = EMPTY` dentro de R3 permanece intacto;
para I-57 grandfathered, la senal durable autorizada es el tag.

## 3. Matriz de consumo AUTH-01..13

| Necesidad I-55 | Foundation authority integrada | Evidencia concreta en `main` | Estado |
|---|---|---|---|
| ViewKind | AUTH-01 | `RackViewKind` en `RackViewAddress.cs`; CT-04 V2 | SATISFIED |
| Address/variant | AUTH-02 | `RackViewAddress` y variantes discriminadas en `RackViewAddress.cs` | SATISFIED |
| Codec | AUTH-03 | `RackViewCodec.cs`; disposicion Canonical/Canonicalizable/Coerced/Invalid | SATISFIED |
| Availability | AUTH-04 | `RackViewAvailability.cs`; sintaxis separada de existencia efectiva | SATISFIED |
| Frame/span | AUTH-05 | `RackViewFrame.cs` y `RackViewFrameAdapters.cs`; CT-05 | SATISFIED |
| Scan/classification | AUTH-06 | hechos neutrales en `RackPhysicalSelection.cs`; CT-SCAN | SATISFIED |
| Selection facts | AUTH-07 | nucleo neutral de seleccion en `RackPhysicalSelection.cs`; CT-16 | SATISFIED |
| Transform facts | AUTH-08 | `RackTransformFacts.cs`; `Transform2D` conservado; CT-GEO | SATISFIED |
| Resolve | AUTH-09 | port/result y adapters en `RackResolveContract.cs`; CT-RES | SATISFIED |
| Prepare carrier | AUTH-10 | payload discriminado y carrier en `RackViewPreparation.cs`; CT-PLAN | SATISFIED |
| BaseName | AUTH-11 | `RackViewBaseName.cs`; separado de collision policy; CT-NAME | SATISFIED |
| Block requirements/query | AUTH-12 | `LibraryBlockRequirements.cs` + `AutoCadLibraryBlockQuery.cs`; CT-BLK | SATISFIED |
| Authored comparator | AUTH-13 | `RackAuthoredComparator.cs`; resultado por kind sin elegir hermana; CT-AUTH | SATISFIED |

No se encontro un hecho neutral requerido por Proposal V5 que falte en AUTH-01..13. Las decisiones de exposicion,
aceptacion, prompts, remedios, colocacion, Relative Frame Window, CQ-01 y materializacion siguen siendo policy de I-55.

## 4. AUTH-14

AUTH-14 no es un tipo productivo adicional. Su materializacion final es la propiedad y evidencia compartida de las
caracterizaciones CT-04, CT-05, CT-16, CT-RES, CT-PLAN, CT-NAME, CT-SCAN, CT-GEO, CT-BLK y CT-AUTH. Sus fixtures y
pruebas estan integrados en `tests/RackCad.Tests`; los receipts F1..F6 los registran verdes. Esta interpretacion satisface
la precondicion historica `AUTH-01..14 INTEGRATED` de G3 sin inventar una autoridad productiva numero 14.

## 5. AUTH-15 e I-52

AUTH-15 sigue fuera de I-57 y no esta integrado en `main`. `SystemBlockWriter` expone el seam caller-owned de
**redefinicion**, pero `CreateBlock` todavia toma lock, importa, abre y confirma su propia transaccion; no existe el
primitivo caller-owned de **creacion** que exige AUTH-15.

I-52 avanzo durante G2I a `766841f8f9d5d29f807966d6796cd08c2363676d`: rebaso sobre I-57 y publico su propia
reconciliacion de consumo. El commit nuevo toca solo `ROADMAP`, decisiones I-52 y su receipt; declara AUTH-15 bajo
ownership de I-52, pero no entrega `CreateInTransaction` ni implementacion productiva. Por tanto:

- G15 = **BLOCKED BY AUTH-15 / I-52**.
- G9b no queda bloqueado: conserva el modo 2 historico con R-25 mientras AUTH-15 no este integrado.
- G3..G14 no esperan a I-52 por AUTH-01..13.

Cruces materiales futuros con I-52: `SystemBlockWriter`, el materializador compartido, los seams del Plugin y la
revalidacion C-2. Antes de editarlos, la iniciativa que llegue segunda debe re-fetch/reconcile y serializar archivos
calientes. No existe hoy una edicion productiva concurrente en la rama de I-52.

## 6. PR-1 y PR-2 contra `main`

### PR-1 / H-01 — OPEN, permanece G4

La UI Push Back sigue poblando el selector lateral con posiciones `1..count` y `SelectedView()` persiste
`LateralSectionBox.SelectedIndex`. El builder produce cortes identificados por `PostIndex` y el Plugin ya consume el
poste real; una lista con postes no contiguos puede todavia confundir posicion de UI con identidad fisica. I-57 conserva
el comportamiento caracterizado y no corrige esta policy de producto.

### PR-2 / H-02 — OPEN, permanece G5

`CantileverViewPlanBuilder.Build` acepta `CantileverPlantaVisibilityDesign`, pero los dos llamadores de
`RackCantileverCommands` siguen invocandolo sin `design.PlantaVisibility`. El valor nulo aplica el comportamiento
vigente de planta y pierde la seleccion persistida del diseno en el dibujo materializado. I-57 lleva el carrier/plan;
la conexion de policy sigue siendo responsabilidad de I-55.

## 7. Readiness de gates V5

| Gate | Estado G2I | Motivo falsable |
|---|---|---|
| G3 | BLOCKED | Architect formal PENDING y ADR-0042 PROPOSED; Foundation, M-01, OD y R3 ya satisfechos |
| G4 | NOT YET OPEN | depende de G3; PR-1 sigue reproducible |
| G5 | NOT YET OPEN | depende de G3; PR-2 sigue reproducible |
| G6 | NOT YET OPEN | depende de G4 y G5; AUTH-01..06 disponibles |
| G7 | NOT YET OPEN | depende de G6; AUTH-09..12 disponibles |
| G8 | NOT YET OPEN | depende de G7 |
| G9a | NOT YET OPEN | depende de G7 y G5 |
| G9b | NOT YET OPEN | depende de G8 y G9a; puede usar modo 2 sin AUTH-15 |
| G10 | NOT YET OPEN | depende de G9b |
| G11 | NOT YET OPEN | depende de G10 |
| G12 | NOT YET OPEN | depende de G11 |
| G14 | NOT YET OPEN | depende de G12; AUTH-05/07/08/09/13 disponibles |
| G15 | BLOCKED | depende de G14 y de AUTH-15 integrado por I-52 |
| G16 | NOT YET OPEN | depende de la entrega y evidencia de los gates previos |

Ningun gate historico se elimina: los gates de extraccion neutral ya fueron retirados por V5 y sustituidos por consumo de
I-57. Este receipt no abre G3.

## 8. Estado tecnico de Proposal V5 y ADR-0042

```text
Proposal V5 = STILL GOVERNING
Coordinator = AGREED
Architect formal = PENDING
Owner product decisions = ACCEPTED / RECORDED
ADR-0042 = PROPOSED
R3 = EFFECTIVE
Foundation gaps = NONE
Material contradictions = NONE
```

ADR-0042 conserva su decision de producto y su blob historico. Todavia no puede presentarse como decision final del
Owner: primero requiere el verdict formal independiente del Architect sobre V5 + este receipt. Si ese verdict es
`AGREED`, la reconciliacion no deja otra precondicion tecnica pendiente para presentar ADR-0042 al Owner.

## 9. Target de review formal del Architect

El objeto de review es **Proposal V5 historica e inmutable** (`f49671e…`) junto con este receipt G2I. El Architect debe
emitir un verdict independiente sobre:

A. AUTH-01..13 integradas satisfacen las premisas neutrales de V5.
B. Su forma concreta no contradice ID17/ID18/ID19, M-01, OD o CQ-01.
C. ADR-0034 permanece intacta: Resolve → adapter por kind → autoridad del handler.
D. Foundation y product policy siguen separadas.
E. El mapa G3..G16 sigue coherente con PR-1/PR-2 abiertos.
F. AUTH-15 queda aislada en G15 y no se recrea dentro de I-55.

Salida solicitada: `AGREED | AGREED WITH CHANGES | CHANGES REQUIRED`, con findings materiales identificados. El
Architect no acepta ADR-0042 ni abre G3 por cuenta propia.

## 10. Salida G2I

```text
I-57 = INTEGRATED / CONSUMABLE
I-55 Proposal V5 = STILL GOVERNING
Coordinator = AGREED
Architect = PENDING
ADR-0042 = PROPOSED
G3 execution = NOT OPEN
G2I package = READY FOR COORDINATOR REVIEW
Open Material = Architect verdict; ADR-0042 Owner decision; AUTH-15 for G15 only
Open Minor = pre-merge F8 wording remains in the immutable I-57 closure documents; integration/I-57 is the durable signal
```
