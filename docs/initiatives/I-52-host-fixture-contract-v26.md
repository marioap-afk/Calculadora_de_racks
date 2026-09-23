# I-52 — Host Fixture Contract V26

> **HF-V26-1 / CLOSED FOR REVIEW / NOT EXECUTED.** Este contrato sólo define el fixture de caracterización host.
> No acredita ningún DA-P, no admite ningún kind y no autoriza CT-DA ni implementación de producto.

## 1. Identidad y alcance

| Campo | Valor |
|---|---|
| Contract version | `HF-V26-1` |
| Proposal parent | I-52 `58c6e02dca5bdbf62546fdc49b32a074e8d2204c` |
| Source baseline | `origin/main = 7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Foundation/I-57 | object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target main |
| I-55 observed | closure `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`; product `991d1694c39377fe7f03d67b024b5a9fcc8dd093` |
| Fixture schema | `rackcad.ctda.host-fixture/1` |
| Product dependency | none |

El scope afirmable es únicamente la semántica de las clases API, event classes, transaction paths y lifecycle phases
enumeradas aquí sobre el tuple exacto. Un resultado no se generaliza a otra primitive, evento, tuple, plugin o kind.

## 2. Lifetime e aislamiento

El harness futuro debe crear un DWG scratch nuevo en una ruta temporal de validación, sin abrir, importar o guardar
sobre un dibujo del usuario. El archivo se identifica con un nonce de ejecución, se cierra al terminar y se elimina
como unidad; si el proceso cae, queda como artefacto diagnóstico y nunca se reabre como proyecto. No se usa la
biblioteca externa del usuario, catálogos de producto, current layer, current color, current linetype o current style.

El harness y cualquier helper nativo viven en `eng/validation`, `tests` o un directorio dedicado de research. No se
registran comandos de producto ni se enlazan a RACKMIRROR/AUTH-15.

## 3. Primitivas y objetos exactos

| Id | Objeto/API real | Forma del fixture | Scope representado |
|---|---|---|---|
| F-DB | `Database` + `Document` | scratch DWG activo | DB/document lifecycle |
| F-T | `TransactionManager` + `Transaction` | T primaria y T secundaria/nested instrumentadas | commit/abort/RYOW |
| F-ID | `ObjectId` | ids registrados de todos los objetos | identidad DB estable durante run |
| F-SRC | `BlockTableRecord` | `RACKCAD_CTDA_V26_SOURCE` | fuente semántica controlada |
| F-A/F-B | `BlockTableRecord` | `RACKCAD_CTDA_V26_A/B` | dos hermanas destino |
| F-PIECE | `BlockTableRecord` | `RACKCAD_CTDA_V26_PIECE` | binding físico determinista |
| F-REF-A/B | `BlockReference` | referencias directas en ModelSpace | siblings, transforms, erase/add |
| F-NEST-A/B | `BlockReference` | una pieza anidada por definición hermana | definition binding/linkage |
| F-EXT | extension dictionary | diccionario en F-SRC/F-A/F-B | mismo storage family que payload de producto |
| F-XR | `Xrecord` + `ResultBuffer` | payload fuente y destino tipado | semantic reads/writes/bytes |
| F-NOD | `DBDictionary` NOD + `Xrecord` | key `RACKCAD_CTDA_V26_NOD` | semántica host NOD, no aplicabilidad de kind |
| F-LAYER | layer table + `LayerId` | layer `RACKCAD_CTDA_V26` creada explícitamente | resource assignment |
| F-EVT | Database/Document/Application events | handlers administrados instrumentados | T1/T4–T8/T10–T16 aplicables |
| F-NATIVE | reactor ObjectARX de research | helper nativo mínimo con build/hash registrado | T2 y callbacks nativos |
| F-LOCK | `DocumentLock` | acquisition/release registrados | exclusión y lifecycle |
| F-CMD | comando de research | entry/return/CommandEnded/idle/queued markers | C_decide/C_report/C_host |

`DynamicBlockReferenceProperty`, anonymous dimension internals y otros primitives específicos no están cubiertos por
HF-V26-1. Su comportamiento y cobertura permanecen en `KindSpecificCoveragePass(k)`. No se infiere desde F-REF.

## 4. Expected authority inmutable

Los expected values se construyen desde constantes del contrato, en una estructura read-only separada del writer:

```text
FixtureId       = "HF-V26-1-RUN-<nonce>"
RackLikeId      = "HF-V26-IDENTITY"
AddressA/B      = "A" / "B"
SourceValue     = 17
MirrorValueA/B  = 29 / 31
LayerName       = "RACKCAD_CTDA_V26"
TransformA      = translation(100,200,0)
TransformB      = translation(300,200,0)
SiblingSet      = { A, B }
DefinitionSet   = { RACKCAD_CTDA_V26_A, RACKCAD_CTDA_V26_B }
```

El writer recibe valores copiados. El verifier vuelve a leer DB y compara contra la tabla inmutable original; no lee
el plan mutable del writer ni acepta «lo que se escribió» como expected.

## 5. Payload y matriz S/M/SM cerrada

Cada Xrecord usa ResultBuffer con schema, fixture id, logical id, address, semantic value y monotonic fixture generation.
No contiene DTO de producto ni nombres Foundation.

| Surface | Storage | Owner | Class | Expected authority | Verifier | Probes | DA-P/H-P | Status |
|---|---|---|---|---|---|---|---|---|
| source semantic value | F-SRC/F-XR | fixture | S | immutable `SourceValue` | fresh Xrecord decoder | 01–05,10S,14,16S | H-P1..6,H-P10,H-P12 | CLOSED |
| destination semantic A/B | F-A/F-B/F-XR | fixture | S | immutable mirror values | full sibling Xrecord scan | 01–10S,16S | H-P1..6,H-P12 | CLOSED |
| logical id/address | F-A/F-B/F-XR | fixture | S | fixed id + A/B | decoder + uniqueness check | 08,10S,16S | H-P5,6,12 | CLOSED |
| outer transform A/B | F-REF-A/B | fixture | M | `TransformA/B` | fresh `BlockTransform` read | 10M,12,F1,F2,16M | H-P2,7obs,9,11,12 | CLOSED |
| layer assignment | outer/nested refs | fixture | M | F-LAYER ObjectId resolved by fixed name | fresh `LayerId` read | F8,10M,16M | H-P7obs,9,11,12 | CLOSED |
| definition binding | outer/nested refs | fixture | M | fixed DefinitionSet/F-PIECE ids | fresh BTR/ref traversal | F5,10M,16M | H-P7obs,9,11,12 | CLOSED for fixture; no product meaning |
| sibling membership/linkage | ModelSpace refs + id/address Xrecords | fixture | SM | fixed SiblingSet + one-to-one payload association | semantic scan + physical traversal | 08,10SM,F4,F6,16SM | H-P5,6,7obs,9,11,12 | CLOSED |
| NOD generation/value | F-NOD | fixture | S | fixed key/value sequence | fresh NOD/Xrecord read | 11,10S,16S | H-P8,H-P6,12 | CLOSED for host behavior only |

No fila contiene UNKNOWN, default corriente o dependencia de producto. `H-P7obs` significa observabilidad completa
del fixture; nunca DA-P7 coverage de un kind.

## 6. Recursos deterministas

| Resource | Policy | Expected authority | Verificación |
|---|---|---|---|
| validation layer | REQUIRED | exact name `RACKCAD_CTDA_V26`, created explicitly | LayerTable lookup + every fixture entity `LayerId` |
| source/A/B/piece definitions | REQUIRED | exact fixed names + fixture nonce marker | BlockTable lookup + Xrecord marker |
| linetype/color/lineweight/style | NOT_APPLICABLE to claimed host scope | no claim and no probe depends on value | guard that expected/verifier never reads them |

La layer puede crearse con un color explícito para diagnóstico, pero HF-V26-1 sólo reclama su identidad/asignación.

## 7. Verifiers independientes

1. `SemanticVerifier` abre nuevamente F-SRC/F-A/F-B, decodifica ResultBuffer y compara schema/id/address/value/generation.
2. `SiblingVerifier` enumera referencias directas y definiciones; no recibe la lista que escribió el writer.
3. `PhysicalVerifier` lee transform, erased state, definition id y LayerId de cada referencia real.
4. `NodVerifier` resuelve NOD desde Database y lee el Xrecord por key fija.
5. `LifecycleVerifier` ordena event sequence numbers, transaction ids, thread/context, locks, C points y DB before/after.

Un read failure, objeto extra/faltante, duplicate address, callback sin clasificación o gap de sequence produce FAIL o
UNKNOWN según el contrato de resultados; nunca se filtra.

## 8. Representatividad DA-P1..12

| DA-P | Propiedad host reclamada | Producción | Fixture | Rep. | Límite / obligación kind |
|---|---|---|---|---|---|
| 1 | afiliación de write same-context | Xrecord/dictionary + T | mismos primitives F-XR/F-T | YES | mapping de todas las surfaces del kind sigue aparte |
| 2 | commit/callback observable | DB objects + commit | Xrecord + refs + events | YES | callbacks de primitive no incluida no se heredan |
| 3 | secondary/nested containment | TransactionManager | mismo manager/DB | YES | estrategia T de producto debe coincidir |
| 4 | read-your-own-writes | extension dictionary/Xrecord/ResultBuffer | mismos paths | YES | production reader debe demostrar equivalencia de path |
| 5 | consistent semantic scan | varias hermanas | F-A/F-B scan | YES | shape de DTO no acreditada |
| 6 | semantic final fence | durable bytes + lifecycle | F-XR + 10S/16S | YES | sólo event classes ejecutadas |
| 7 | verifier observability | todos los fields físicos del kind | fields físicos del fixture | PARTIAL | sólo `DA-P7_HOST_OBSERVABILITY(f)`; kind coverage obligatoria |
| 8 | NOD transaction/snapshot behavior | NOD si el kind lo usa | F-NOD | YES for host semantics | aplicabilidad y keys/readers son por kind |
| 9 | consistent physical scan | múltiples entidades | F-REF-A/B + nested refs | YES | primitives kind-specific adicionales requieren fixture del kind |
| 10 | source lifecycle ordering | source/readset real | F-SRC controlada | PARTIAL | sólo ordering; complete product readset sigue I-49/kind |
| 11 | final material fence | DB physical state | transforms/layer/linkage | YES for included primitives | dynamic/dimension/otros primitives por kind |
| 12 | usable success boundary | command lifecycle | F-CMD/F-EVT/F-NATIVE | YES if probes pass | no generalización a scheduler no representado |

`PARTIAL` es una frontera resuelta, no UNKNOWN: especifica exactamente qué subpropiedad puede acreditarse y qué
obligación jamás sale de `KindSpecificCoveragePass(k)`.

## 9. Amenazas T1..T16

| T | Fixture mechanism / primitive | Clase | Probe family | Propiedad | Representative scope / límite |
|---|---|---|---|---|---|
| T1 | managed DB/Document handler writes F-XR/F-REF | S/M/SM | 01,02,10*,16* | H-P1,6,11,12 | exact managed event class |
| T2 | F-NATIVE ObjectARX reactor mutates same objects | S/M/SM | 02N,04N,09N,10N*,16N* | H-P1,2,3,6,11,12 | native helper mandatory; no managed emulation credit |
| T3 | nested/new T writes then main abort/commit | S/M/SM | 03–05,13 | H-P3 + affected fences | exact transaction strategy |
| T4 | commit-phase managed/native callback | S/M/SM | 05,09,09N | H-P2,6,11 | native leg required if event class native |
| T5 | scheduled mutation between preread and commit | S/M/SM | 01,05 | H-P1,2 | fixture objects only |
| T6 | commit-to-postread callback | S/M/SM | 09,09N | H-P2,5,9 | exact event class |
| T7 | orchestrated mutation between A/B scan steps | S/M/SM | 08,12 | H-P5,9 | proves selected snapshot strategy |
| T8 | postverification mutation before C_host | S/M/SM | 10S/M/SM | H-P6,11,12 | exact lifecycle candidate |
| T9 | F-NOD mutation | S | 11,10S,16S | H-P8,6,12 | host semantics only |
| T10 | F-SRC mutation after S | S | 14,16S | H-P10,6,12 | ordering only; no product readset |
| T11 | destination Xrecord/linkage mutation | S/SM | 01,09,10S/SM | H-P1,5,6,11 | fixture schema only |
| T12 | transform/layer mutation | M | 10M,12,F1,F8,16M | H-P7obs,9,11,12 | included primitives only |
| T13 | other command/context attempts writes under lock | S/M/SM | 13C,16* | H-P3,6,11,12 | exact supported context |
| T14 | module load without mutator | OUT | diagnostic load probe | none | load alone changes no protected object |
| T15 | module registers one T1–T13 mutator | inherited | registration + inherited probe | inherited | no credit for identity inventory |
| T16 | queued/idle/command-context work crosses candidate | S/M/SM | 16S/M/SM,16N* | H-P6,11,12 | only mechanisms enumerated/executed |

Todo threat IN tiene primitive, mechanism, probe y resultado. Si una event class disponible en el tuple no cabe en
una fila o no puede ejecutarse, coverage queda UNKNOWN y `CTDA_HOST_PASS=false`.

## 10. T2 — reactor nativo

T2 no se acredita con un handler managed. PASS exige uno de estos dos caminos, en orden de autoridad:

1. contrato oficial exacto que gobierne la misma clase de reactor y orden observado, más probe confirmatorio; o
2. helper ObjectARX de research F-NATIVE, construido para el exacto SDK/build, que registre su hash y mutile F-XR,
   F-REF o ambos en los puntos nativos definidos.

Sin helper cuando el contrato documental sea insuficiente: T2=`UNKNOWN`, `CTDA_HOST_PASS=false`. El helper no enumera
reactores de terceros ni prueba su ausencia; caracteriza la clase host que ejecuta.

## 11. Predicado de cierre

```text
HostFixtureContractClosed(f) iff
  FixtureVersion(f) = HF-V26-1
  AND every primitive/object/event class is enumerated
  AND every fixture surface has exactly one S/M/SM row
  AND every expected authority is immutable and independent from the writer
  AND every resource is REQUIRED or explicitly NOT_APPLICABLE
  AND every reader/verifier is defined over fresh DB state
  AND every T1..T16 threat is mapped or explicitly OUT with proof obligation
  AND every required probe is defined
  AND every claimed host property is YES or bounded PARTIAL with residual kind obligation
  AND exact tuple, lifetime and logging contracts are defined
  AND no UNKNOWN, current default, product assumption or external user dependency remains
```

HF-V26-1 satisface el cierre documental. Esto significa «se puede revisar/implementar como research», no PASS.

## 12. Resultados host y producto

```text
DA-P7_HOST_OBSERVABILITY(f) = verifier observes every physical surface declared by f
DA-P7_KIND_COVERAGE(k)      = verifier observes every RackCad-controlled product surface of k
DA-P8_HOST_NOD(f)           = NOD transaction/snapshot semantics for exact fixture path
DA-P8_KIND_APPLICABILITY(k) = kind census proves NOD required or demonstrably not applicable
DA-P10_HOST_ORDERING(f)     = lifecycle orders/detects mutation of controlled fixture source
DA-P10_KIND_READSET(k)      = complete source readset, including I-49 authority, is covered

CTDA_HOST_PASS(f) iff
  HostFixtureContractClosed(f)
  AND H-P1..H-P6 = PASS
  AND DA-P7_HOST_OBSERVABILITY(f) = PASS
  AND DA-P8_HOST_NOD(f) = PASS
  AND H-P9 = PASS
  AND DA-P10_HOST_ORDERING(f) = PASS
  AND H-P11..H-P12 = PASS
  AND every host-representable T1..T16 obligation = PASS
  AND exact host tuple and fixture version match
  AND every required managed/native/deferred probe executed

KindSpecificCoveragePass(k) iff
  DA-P7_KIND_COVERAGE(k) = PASS
  AND DA-P8_KIND_APPLICABILITY(k) is resolved and its applicable evidence = PASS
  AND DA-P10_KIND_READSET(k) = PASS
  AND every primitive/event/path outside HF-V26-1 required by k has kind evidence PASS

CTDA_PASS(k) iff
  CTDA_HOST_PASS(HF-V26-1)
  AND KindContractClosed(k)
  AND KindSpecificCoveragePass(k)
  AND all product-kind fixtures = PASS
  AND exact host tuple is compatible
```

`CTDA_HOST_PASS != CTDA_PASS(k)` y `CTDA_PASS(k1) != CTDA_PASS(k2)`. No existe global promotion.

## 13. Exact host tuple y logging

Cada run registra AutoCAD 2025, managed API 25.0.0.0, file/build versions y hashes de `acad.exe`, `acdbmgd.dll`,
`acmgd.dll` y helper nativo; program/product/vertical y registry identity; OS/runtime; harness/probe SHA;
HF-V26-1 blob; scratch DWG hash; event/context/thread; current/top transaction; object/open mode; sequence number;
before/after bytes; commit/abort; R/M points; method return; CommandEnded; lock release; idle/queued execution;
`F_usable`, `C_decide`, `C_report` y `C_host`.

Inference sola no produce PASS. Runtime observation aceptada como soporte build-specific se etiqueta así y nunca como
contrato universal.

## 14. Invalidación

Invalidan evidencia host: cambio de tuple/API/build/hash/product/vertical/OS-runtime material; primitive o storage path;
transaction strategy; command lifecycle integration; harness scheduling; helper nativo; fixture contract/schema;
expected resources; verifier semantics; threat/probe mapping; o probe SHA. Exige nueva ejecución.

Los invalidadores de KindContract V25 permanecen separados: consumers, DTO, metadata, resources, addresses,
Foundation/I-55, schema, comparator, materializer, aliases y rutas productivas no reescriben evidencia host salvo que
cambien también una primitive/event/path acreditada.

## 15. Autorefutación

1. `HostFixtureContractClosed=true` con host property dependiente de primitive/event ausente: **imposible**; la fila
   queda fuera del scope reclamado y la obligación residual es kind-specific, o el contrato deja de estar cerrado.
2. `CTDA_HOST_PASS=true` con Selective admitido y su KindContract abierto: **imposible** por la fórmula separada.
3. Synthetic fixture usado como DA-P7 product coverage: **imposible**; sólo existe `DA-P7_HOST_OBSERVABILITY(f)`.
4. Unknown plugin isolation inferida: **imposible**; sólo se acreditan clases ejecutadas/documentadas.

## 16. Estado

```text
HOSTFIXTURECONTRACT HF-V26-1 = CLOSED / NOT EXECUTED
CTDA_HOST_PASS = NOT EVALUATED
KindContractClosed(Selective) = FALSE
PRODUCT KINDCONTRACTS = STILL OPEN
CT-DA AUTHORIZATION = NOT GRANTED BY THIS DOCUMENT
```
