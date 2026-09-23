# I-52 — KindContract V25: matriz normativa y censo de consumidores

> **NORMATIVE FOR PROPOSAL V25 / NOT CLOSED.** Esta matriz describe el baseline observado; no implementa ni admite
> RACKMIRROR, CT-DA o ningún kind.

## 1. Identidad y versionado

| Campo | Valor |
|---|---|
| Proposal parent | `1be4852b9967412f5f187962887a919470b020e5` |
| source baseline | `origin/main = 7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Foundation/I-57 | object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 observed | closure `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`; product ID18 `991d1694c39377fe7f03d67b024b5a9fcc8dd093` |
| Selective authored schema | legacy `1.0`; promoted `2.0`; read major `2` |
| envelope schema | `1.0` |
| matrix revision | `KC-V25-1` |

Un cambio material de fuente no hereda esta matriz. La revisión usa referencias y rutas del baseline exacto; el SHA y
blob finales del artefacto se publican después del commit.

## 2. No-default normativo

```text
missing entry                         => UNKNOWN
conditional entry without authority  => UNKNOWN
uncensused consumer                   => UNKNOWN
unknown resource ownership            => UNKNOWN
unknown expected value                => UNKNOWN
unknown verifier                      => UNKNOWN
unknown fixture mapping               => UNKNOWN
unknown DA-P mapping                  => UNKNOWN

UNKNOWN => KindContract NOT CLOSED => kind NOT ADMISSIBLE
```

No existe default a `M`, `USER_CONTROLLED`, `NOT_APPLICABLE`, «no consumer», «same as another kind» o
«physical-only».

## 3. Censo y criterio de completitud

El censo fuente recorrió estos roots soportados y sus llamadas alcanzables:

- persistencia/envelope: `RackEmbedDocument`, `SelectivePalletDesignDocument/Store`, `RackBlockFinder`;
- comandos/routers: `RackCommandSupport`, `RackSelectivoCommands`, `RackInventarioCommands`, `RackVariablesCommands`,
  `RackPropiedadesCommands`, `RackDuplicarCommands`, `RackLayoutCommands`;
- semántica: `SelectiveKindHandler`, `SelectiveEffectiveDesignResolver`, `SelectiveGeometryResolver`,
  `SelectiveAuthoredAuthority`, `BomAuthoredAuthority`, `SelectiveBomBuilder`;
- vistas: `RackViewCodec`, `RackViewPreparation`, `RackViewFrameAdapters`, builders Selective y `ViewBlockDraw`;
- materialización: `LateralHeaderDrawer`, `BlockPlacement`, `SystemBlockWriter`;
- variables: consumer discovery, mutation preflight y `PlanReadSet`/I-49 por referencia.

También se censaron propiedades declaradas de DTO/envelope y accesos a `BlockReference`, dynamic properties, layer,
linetype, color, lineweight, dimension/text style y definition binding. El censo es reproducible mediante referencias
de símbolos y búsqueda de lecturas/escrituras sobre el baseline; una coincidencia de nombre no cuenta como consumo.

```text
ConsumerCensusComplete(surface, kind) iff
  declared storage members are enumerated
  AND every supported command/router root is classified
  AND reachable readers/comparators/materializers are traced
  AND metadata/dynamic/resource access sites are enumerated
  AND no reachable access remains UNCLASSIFIED
```

Una superficie sólo puede ser `M` cuando el predicado anterior es true, ningún consumidor semántico soportado la lee
y el contrato físico la posee. «No se encontró una lectura» sin universo cerrado produce UNKNOWN.

## 4. Leyenda

- Consumidores: `E` editor/Actualizar; `B` BOM/RACKLISTA; `I` Insertar/future view; `P` persistencia/save-reopen;
  `V` project variables; `D` duplication/layout; `MZ` materialization planning.
- Policy: `REQ`, `USER`, `NA`, `UNK`.
- Fixtures: `10S/M/SM`, `16S/M/SM`, `F1..F8` significan las familias V24.
- Estado `CLOSED` es por fila; un kind sólo cierra si todas sus filas relevantes cierran.

## 5. Matriz Selective

| Kind | Physical/View unit | Surface / field | Source / storage | Semantic consumers | Owner | Class | Resource policy | Expected value / independent authority | Verifier | Fixtures | DA-P | Status / limits |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Selective | all | RackId | envelope `Id`; authored `Id` | E,B,I,P,V,D | RackCad | S | NA | equality of envelope/authored identity; Foundation grouping | authoritative reader/comparator | 10S,16S | P1,5,6,12 | CLOSED |
| Selective | all | kind | envelope `Kind` | E,B,I,P,V | RackCad | S | NA | exact `selective` token / registry | envelope reader + registry | 10S,16S | P1,5,6,12 | CLOSED |
| Selective | Fondo/Post/Whole | RackViewAddress | envelope `View/Section`; `RackViewCodec` | E,I,P,D,MZ | RackCad/Foundation | S | NA | typed codec: Fondo/Post/Whole | codec + sibling reader | 10S,16S | P1,5,6,12 | CLOSED |
| Selective | all | name | envelope/authored `Name` | E,B,P,MZ | RackCad | S | NA | authored carrier | semantic comparator | 10S,16S | P1,5,6,12 | CLOSED |
| Selective | all | authored DTO geometry/options | `SelectivePalletDesignDocument` declared fields | E,B,I,P,V,MZ | RackCad | S | NA | persisted DTO + effective resolver | authoritative reader/comparator | 10S,16S | P1,4,5,6,10,12 | CLOSED as family; declared-member census required on change |
| Selective | all | authored SchemaVersion | design document | E,P,V | RackCad | S | NA | `SelectiveDesignSchema` 1.0/2.0 rules | schema guard + comparator | 10S,16S | P1,5,6,12 | CLOSED |
| Selective | all | PropertyValues / variable bindings | design document map | E,B,I,P,V,MZ | RackCad/I-49 | S | NA | per read variable `SymbolId + complete RootCauses(variable)` | binding reader + comparator | 10S,16S | P1,5,6,8,10,12 | CLOSED by I-49 reference; no global union |
| Selective | all | authored ExtensionData | design document JSON extension | P + authoritative equality | RackCad carrier | S | NA | exact preserved JSON | authoritative comparator | 10S,16S | P1,5,6,12 | CLOSED conservatively; never M |
| Selective | all | CustomProperties | envelope raw JSON / property store | E,B,I,P | RackCad | S | NA | custom-property authority/store | reader/comparator | 10S,16S | P1,5,6,12 | CLOSED |
| Selective | all | envelope SchemaVersion/ExtensionData | envelope | E,I,P | RackCad carrier | S | NA | embed schema + exact preserved extension | envelope comparator | 10S,16S | P1,5,6,12 | CLOSED |
| Selective | all siblings | sibling membership/presence | definitions + direct references + RackId | E,B,I,P,D,MZ | RackCad | SM | NA | expected address set from authored design/exposure | sibling reader + physical verifier | 10SM,16SM,F4 | P5,6,7,9,11,12 | CLOSED |
| Selective | all siblings | grouping/linkage | RackId/address/payload-definition association | E,B,I,P,V,D | RackCad/Foundation | SM | NA | Foundation identity/address rules | semantic + physical comparator | 10SM,16SM,F6 | P5,6,7,9,11,12 | CLOSED |
| Selective | outer reference | position/transform | AutoCAD BlockReference | D and placement operations; no authored reader proven | RackCad physical | M | NA | accepted frame/placement result | DB physical verifier | 10M,16M,F1 | P7,9,11,12 | OPEN: command-root proof excluding semantic interpretation not closed |
| Selective | outer reference | rotation | AutoCAD BlockReference | D; no authored reader proven | RackCad physical | M | NA | accepted placement/frame | DB physical verifier | 10M,16M,F2 | P7,9,11,12 | OPEN with transform census |
| Selective | outer reference | scale/determinant | AutoCAD BlockReference | D; no authored reader proven | RackCad physical | M | NA | positive determinant/unit contract V23 | DB physical verifier | 10M,16M,F2 | P7,9,11,12 | OPEN with transform census |
| Selective | nested piece refs | dynamic LONGITUD/PERALTE/ALTURA/SAQUE/FRENTE/FONDO | dynamic block properties; plan/builders | MZ; no semantic DB reader found | RackCad physical | M | NA | pure plan + catalog parameter policy | actual dynamic-property reader | 10M,16M,F3 | P7,9,11,12 | OPEN: catalog-driven aliases/external block key completeness not closed |
| Selective | all | definition identity/binding | block definition/ref + catalog block name | E,I,P,MZ | RackCad/catalog | SM | REQ | typed plan requirement + versioned catalog, independent of writer object | definition binding verifier | 10SM,16SM,F5 | P2,6,7,11,12 | CLOSED for named requirement; external library validity remains CT-DA input |
| Selective | annotations | layer | DBText in system definition | MZ only | RackCad physical | M | REQ | exact `RACKCAD_ANOTACIONES`, normative contract/source constant | entity layer verifier | 10M,16M,F8 | P7,9,11,12 | CLOSED |
| Selective | dimensions | layer | RotatedDimension in system definition | MZ only | RackCad physical | M | REQ | exact `RACKCAD_COTAS`, normative contract/source constant | entity layer verifier | 10M,16M,F8 | P7,9,11,12 | CLOSED |
| Selective | dimensions | dimension style binding | authored `DimensionStyle`; DB dimension style | E,P,MZ | RackCad + drawing authority | SM | REQ | named authored style when present; otherwise DB current style resolution | authored + DB style verifier | 10SM,16SM,F5/F8 | P5,6,7,9,11,12 | OPEN: fallback snapshot authority across final window not closed |
| Selective | outer reference | layer/linetype/color/lineweight | AutoCAD defaults/current properties | physical commands | UNKNOWN | UNKNOWN | UNK | none published independently | none complete | F8 + kind fixture | P7,9,11,12 | OPEN / no default to USER |
| Selective | nested piece refs | layer | BlockReference inside definition | physical materializer | UNKNOWN | UNKNOWN | UNK | constructor/default behavior is not a policy | none complete | F8 + kind fixture | P7,9,11,12 | OPEN |
| Selective | nested piece refs | linetype/color/lineweight | BlockReference inside definition | physical materializer | UNKNOWN | UNKNOWN | UNK | no independent assignment contract | none complete | F8 + kind fixture | P7,9,11,12 | OPEN |
| Selective | annotations | text style/color/linetype/lineweight | DBText | physical materializer | UNKNOWN | UNKNOWN | UNK | layer alone does not define all assignments | none complete | F8 + kind fixture | P7,9,11,12 | OPEN |
| Selective | dimensions | color/linetype/lineweight | RotatedDimension | physical materializer | UNKNOWN | UNKNOWN | UNK | layer/style resolution does not publish complete ownership | none complete | F8 + kind fixture | P7,9,11,12 | OPEN |
| Selective | all | per-view diagnostics/transient state | command results/logs | none as authority | not authority | M | NA | excluded explicitly from persisted contract | absence guard | kind fixture | P7 | CLOSED as non-authority; cannot be persisted silently |

### Selective verdict

```text
ConsumerCensusComplete(Selective) = FALSE
KindContractClosed(Selective) = FALSE
Selective product admission = OPEN / NOT ADMISSIBLE
Selective CT-DA-HOST fixture = NOT CLOSED
```

Las filas OPEN no se convierten en exclusión del scope. V26 debe resolver el censo de transform y las policies de
recursos/estilos/dynamic aliases o elegir un fixture acotado con todas sus superficies realmente cerradas.

## 6. Resource ownership Selective — sin celdas omitidas

| Unit | Layer | Linetype | Color | Lineweight | Definition/named block | Dimension style | Text style | Status |
|---|---|---|---|---|---|---|---|---|
| outer view reference | UNK | UNK | UNK | UNK | REQ: RackCad view definition | NA | NA | OPEN |
| nested piece reference | UNK | UNK | UNK | UNK | REQ: catalog block requirement | NA | NA | OPEN |
| annotation DBText | REQ `RACKCAD_ANOTACIONES` | UNK | UNK | UNK | NA | NA | UNK | OPEN |
| RotatedDimension | REQ `RACKCAD_COTAS` | UNK | UNK | UNK | NA | REQ: authored-or-DB resolver | NA | OPEN |
| system definition | NA | NA | NA | NA | REQ: unique RackCad definition + payload | NA | NA | CLOSED row |

`UNK` nunca significa current/default/user-controlled. M-F8 ya puede ejercitar las dos layers explícitas, pero eso no
cierra el resto del kind.

## 7. Otros kinds

| Kind | Units considered | Consumer census | Resource policy | Comparator/verifier | KindContract status |
|---|---|---|---|---|---|
| Dynamic/PalletFlow | Entrance, Exit, Post, Planta | OPEN | OPEN | authored comparator no cerrado para admisión I-52 | OPEN / NOT ADMISSIBLE |
| PushBack | cuts A/B, posterior/entrada-salida, Planta | OPEN | OPEN | paths cambiaron con iniciativas paralelas; re-census | OPEN / NOT ADMISSIBLE |
| Cantilever | Station, Frontal, Planta | OPEN | parcialmente explícita en materializer | consumer/verifier incompleto | OPEN / NOT ADMISSIBLE |
| Header/Cabecera | Lateral, Planta | OPEN | layers parciales | authored/sibling contract incompleto | OPEN / NOT ADMISSIBLE |
| Flow Bed/Cama | vista única soportada | OPEN | OPEN | no sisters; no se infiere NA global | OPEN / NOT ADMISSIBLE |

No se transfiere una fila Selective a otro kind.

## 8. Predicados de cierre y admisión

```text
KindContractClosed(k) iff
  every relevant surface/unit has exactly one row
  AND ConsumerCensusComplete(k)
  AND every Classification is exactly S, M or SM
  AND every resource cell is REQUIRED, USER_CONTROLLED or NOT_APPLICABLE
  AND every REQUIRED resource has independent expected authority
  AND every row has verifier and DA-P mapping
  AND every mutable row has fixture mapping
  AND no UNKNOWN or unresolved conditional remains

HostInvariantPass = DA-P1..DA-P12 PASS + T1..T16 coverage + exact tuple
KindContractPass(k) = KindContractClosed(k) + kind fixtures PASS
CTDA_PASS(k) = HostInvariantPass AND KindContractPass(k)
```

`CTDA_PASS_GLOBAL` no existe todavía. La investigación host puede reutilizar propiedades demostradas entre kinds sólo
si el scope exacto coincide. Product admission requiere `CTDA_PASS(k)` para cada kind del first-cut; un kind OPEN queda
excluido, sin reducir silenciosamente el producto. Cambiar first-cut exige decisión separada.

`CT-DA-HOST` puede usar un fixture kind sólo cuando todas las superficies que ejercen `10*/16*/M-F*` estén cerradas.
Esta matriz no deja ninguno listo; por ello la autorización de research permanece bloqueada.

## 9. Derivación de fixtures y DA-P

Cada fila mutable genera fixtures; no existe lista manual desconectada:

- S: `10S`, `16S` cuando cruce boundary, más P1/P5/P6/P12 y propiedades de storage aplicables.
- M: `10M`, `16M`, M-F correspondiente, más P7/P9/P11/P12.
- SM: `10SM`, `16SM`, M-F correspondiente, DA-P6 AND DA-P11 y DA-P12 en cruce.

Una fixture/verifier ausente cambia la fila a UNKNOWN.

## 10. Invalidación

Invalidan filas afectadas y exigen re-censo: consumidor semántico nuevo; campo DTO agregado/removido; cambio de
metadata; materializer que empieza a leer estado físico semánticamente; policy de recursos; nueva address/unit; cambio
relevante I-55/Foundation; schema, comparator o verifier; catálogo/alias dinámico material; o nueva ruta de comando.

I-55 ID18 añade `RackInsertionRequest.Views`, batch ordered, shared identity y prepare-all. No cambia host semantics,
pero su integración futura invalida las filas address, identity, grouping, preparation y placement. Su partial-batch
policy no reemplaza atomicidad I-52.

## 11. Autoridades preservadas

- Foundation AUTH-01..13: dirección, exposure, availability, frames, preparation y comparator donde esté demostrado.
- I-49: por variable realmente leída, `SymbolId + complete RootCauses(variable)`; sin unión global.
- AUTH-15: `I-52 OWNED / NOT IMPLEMENTED`; la matriz sólo enumera obligaciones futuras.
