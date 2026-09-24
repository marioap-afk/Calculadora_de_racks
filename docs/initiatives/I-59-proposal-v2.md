# I-59 — Proposal V2 / Consensus Freeze DRAFT

Frozen: NO

Status: PUBLISHED / PENDING COORDINATOR REVIEW

Coordinator: F1 CLOSURE ACCEPTED; PROPOSAL V2 REVIEW REQUIRED

Architect: NOT YET INVOKED

F1 RED: ESTABLISHED

F1 CLOSURE: ACCEPTED

F2: NOT OPEN

IMPLEMENTATION AUTHORIZATION = NO

Esta es la version completa propuesta para evolucionar AUTH-08 y AUTH-12. Sustituye V1 como objeto
de revision, incorpora CR59-V1-01..03 y no es un Freeze acordado. No abre F2 ni autoriza cambios de
produccion. La identidad aceptada de cierre F1 es
`54319d33810dba54ae3172624604a1e753fe86ae`; el RED historico permanece en
`cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b`.

Autoridades: ADR-0044, `docs/FOUNDATIONS.md`, el contrato I-59,
`docs/INITIATIVE_LIFECYCLE.md` §§4-6, la implementacion integrada de AUTH-08/AUTH-12, I-52 y el G14
vigente de I-55 en `origin/feature/creacion-de-vistas`.

## 1. Delta V2 y disposicion Coordinator

| Finding | Disposicion V2 |
|---|---|
| CR59-V1-01 | RESOLVED IN DRAFT: §7 congela una funcion total para los 16 valores vigentes de `HeaderBlockRole`, sin `default` silencioso; valores futuros/desconocidos fallan visibles. |
| CR59-V1-02 | RESOLVED IN DRAFT: §8 fija `RackViewAddress` del contexto tipado de `Prepare` como unica procedencia semantica; prohibe parsear strings/indices y define evolucion V2 aditiva. |
| CR59-V1-03 | RESOLVED IN DRAFT: §5 adopta opcion B; AUTH-08 V2 no expone `Transform2D` compuesto. Position, rotation, scale, normal y origin permanecen facts separados. |

Son correcciones de diseño pendientes de Coordinator y Architect; esta tabla no emite acuerdo en nombre
de ninguno.

## 2. Resultado, ownership y limites

P-01. Shared View Foundation conserva ownership de hechos neutrales reutilizables. Plugin captura datos
AutoCAD y los convierte en snapshots neutrales; Application clasifica. Los consumidores deciden
aceptacion, rechazo, remedio, anchor, proyeccion, dibujo y mensajes. I-59 no implementa ID19.

P-02. AUTH-08 y AUTH-12 evolucionan mediante contratos V2 aditivos. Los contratos V1 integrados siguen
disponibles y con el mismo comportamiento para I-52, I-55 G8/G9/G12 y consumidores actuales. No aparece
otra autoridad: se reutilizan `Point3D`, `Vector3D`, `RackViewAddress` y, solo para operaciones 2D
matematicamente exactas, el `Transform2D` existente.

P-03. Limites cerrados: sin Rigid, Orthographic, CommonTransform2D, Relative Frame Window,
accept/reject/remedy, anchor/projection policy, RACKMIRROR, AUTH-15, geometria, BOM, builders productivos,
Resolve, naming, I-52 o I-55 productivo. Sin cambio de schema, DTO persistido, store o migracion. Sin NuGet.

```text
Schema diff              = NONE
Persistence migration    = NONE
Plugin product policy    = NONE
I-55 / I-52 modification = NONE
```

## 3. Discovery Core DC-01..09

| ID | Confianza | Resultado |
|---|---|---|
| DC-01 | MEASURED | AUTH-08 V1 describe `Transform2D` uniforme/ortogonal no degenerado y expone Translation XY, angulo raw `Atan2`, determinant, reflection y UniformScale; no uniforme produce `NonUniformScale`. AUTH-12 V1 acepta Key no blank, deduplica case-insensitive, consulta `Found/Missing` y mantiene query/import separados. |
| DC-02 | MEASURED | `RackTransformFacts`/`Transform2D`, `RackViewAddress`, `RackViewPreparationAdapter` y `LibraryBlockRequirements.cs` son owners integrados; ADR-0044 asigna facts puros a Application y adapters AutoCAD a Plugin. |
| DC-03 | MEASURED | Ningun carrier AUTH-08/AUTH-12 V1 es DTO persistido o Xrecord. No participa fallback legacy ni unknown-field persistence. |
| DC-04 | RECONSTRUCTED | AUTH-08: matriz pura -> `RackTransformFacts.Describe` -> consumidor. AUTH-12: `Prepare(resolved,address,...)` valida address, construye payload, extractor V1 obtiene keys, y `RackPreparedView` conserva la misma address. Import opcional precede query final. |
| DC-05 | MEASURED | I-52 consume AUTH-08/AUTH-12 V1. I-55 G8/G9/G12 consume requirement/query/import. G14 necesita Position XYZ, DefinitionOrigin, rotation, scales y Normal. `LateralHeaderDrawer` materializa Annotation/Dimension como entidades nativas y trata todos los demas roles como block references; Pallet esta documentado como referencia visual. |
| DC-06 | MEASURED | F1 aceptado en `54319d33810d...`: RED historico 36 selected/6 PASS/30 FAIL preservado; harness corregido 14/14 y matriz relevante 41/41; Core Full y CI exactos verdes. Huecos V2 estan en §12. |
| DC-07 | MEASURED | I-55 no modifica archivos Shared objetivo, pero consume AUTH-12 y G14 tocara la frontera Plugin. I-52 tampoco los modifica. Orden: I-59 integra y publica receipt; I-55 reconcilia desde main. |
| DC-08 | MEASURED | Se extiende Shared View Foundation bajo ADR-0044: syntax/availability/policy separados, hechos puros en Application, AutoCAD solo Plugin y Unknown no equivale a ausencia/exito. |
| DC-09 | RECONSTRUCTED | `FOUNDATION EVOLUTION`. M-04/M-05/M-06 activados; M-01/02/03/07/08 no activados. CR59-V1-01..03 precisan la forma del contrato sin cambiar esa conclusion. |

## 4. Expansiones y materialidad

### EXP-01..09

| ID | Estado | Salida |
|---|---|---|
| EXP-01 | NEGATIVA RAZONADA | Sin contradiccion clase A. ADR-0044 exige la evolucion neutral. Origin queda justificado por I-55 G14. |
| EXP-02 | NEGATIVA RAZONADA | Ownership inequívoco: Foundation facts, Plugin adapter, consumer policy. |
| EXP-03 | NEGATIVA RAZONADA | No existe ni se propone camino persistido. |
| EXP-04 | ACTIVADA / CLOSED FOR DRAFT | I-52, I-55 G8/G9/G12/G14 y consumidores Foundation inspeccionados; V2 aditiva. |
| EXP-05 | ACTIVADA / CLOSED FOR DRAFT | Invariantes sin cobertura actual enumerados en §12; RED compilable solo despues del Freeze acordado. |
| EXP-06 | ACTIVADA / CLOSED FOR DRAFT | §6 y §9 hacen visibles NonFinite/Degenerate/InvalidTolerance y estados de key/library/block. |
| EXP-07 | ACTIVADA / CLOSED FOR DRAFT | I-55 es consumidor activo sin diff directo Shared; integracion secuencial. |
| EXP-08 | ACTIVADA / CLOSED FOR DRAFT | V1 rechaza non-uniform y pierde identidad por dedup; se preserva compatibilidad sin presentar V1 como suficiente. |
| EXP-09 | NEGATIVA RAZONADA | Ninguna M permanece UNKNOWN. |

Coordinator debe confirmar la poblacion EXP y señalar cualquier expansion omitida.

### M-01..M-08

| ID | Estado | Razon |
|---|---|---|
| M-01 | NO | Mismo owner Foundation; no segunda autoridad. |
| M-02 | NO | Sin schema, DTO/wire persistido, fallback o unknown preservation. |
| M-03 | NO | Sin cableado visible ni cambio de documentos/consumidores V1. |
| M-04 | SI | Nuevos outcomes neutrales y fallo visible para roles futuros/desconocidos. |
| M-05 | SI | AUTH-08/AUTH-12 son contratos consumidos por otros sistemas/kinds. |
| M-06 | SI | Nuevos carriers/clasificadores/extractors V2 reutilizables. |
| M-07 | NO | Sin framework, registry, kernel o mecanismo transversal generico. |
| M-08 | NO | ADR-0044 y su separacion facts/policy se conservan. |

## 5. AUTH-08 V2 — captura y hechos separados

P-08. La frontera propuesta tiene tres pasos y ningun tipo AutoCAD sale de Plugin:

```text
AutoCAD BlockReference + BlockTableRecord (Plugin)
    -> RackSourcePlacementInput (primitivos capturados)
    -> RackSourcePlacementSnapshot (Application, finito, puro)
    -> RackSourceTransformFactsResult (Application, hechos/clasificacion)
    -> consumer policy fuera de Foundation
```

Los nombres publicos son candidatos revisables de V2; nombres de helpers/metodos privados no se congelan.
`RackSourcePlacementInput` transporta doubles crudos para Position XYZ, RotationRadians, Scale X/Y/Z,
Normal XYZ y DefinitionOrigin XYZ. Application, no Plugin, clasifica non-finite antes de construir
`Point3D`/`Vector3D`, que exigen finitud.

P-09. Un input finito produce `RackSourcePlacementSnapshot` con:

- `Point3D ReferencePosition`, Position de la referencia insertada;
- `double SourceRotationRadians`, rotacion fuente alrededor de su normal;
- `Vector3D ScaleFactors`, preservando X/Y/Z y signos;
- `Vector3D Normal`;
- `Point3D DefinitionOrigin`, origin del BlockTableRecord/definicion.

`ReferencePosition` y `DefinitionOrigin` nunca son aliases ni fallback uno del otro. DefinitionOrigin se
incluye porque I-55 G14 exige `Origin != 0` y sabotajes que fallen al ignorarlo. Foundation no decide
como usarlo para placement.

P-10. El clasificador recibe `RackSourceTransformTolerance` con inputs finitos/no negativos `Scale`,
`AngleRadians` y `Normal`. No existe tolerancia global implicita. Input invalido produce
`InvalidTolerance` y no facts fabricados.

P-11. `RackSourceTransformFacts` conserva snapshot y expone hechos independientes:

| Hecho | Semantica propuesta |
|---|---|
| ReferencePosition | XYZ exacto finito capturado |
| RotationRadians | `NormalizePi(SourceRotationRadians)` en `(-pi,pi]`; `NormalizePi(-pi)=+pi` |
| ScaleX/Y/Z | valor por eje con signo; sin media/geometric mean |
| SignX/Y/Z | `Negative`, `ZeroWithinTolerance` o `Positive` |
| ReferenceBasisDeterminantXY | `ScaleX * ScaleY`; determinant de la base local XY, no definition-to-world transform |
| ReferenceBasisIsReflectionXY | determinant XY negativo fuera de tolerance |
| IsUniformScale | magnitudes X/Y/Z iguales dentro de `Scale` |
| IsUnitScale | cada magnitud X/Y/Z igual a 1 dentro de `Scale` |
| HasPlanarHalfTurnSign | X e Y negativas fuera de tolerance; independiente de unit/uniform/reflection |
| IsNegativeZ | Z negativa fuera de tolerance |
| Normal | vector fuente intacto |
| NormalIsWorldZ | normal igual a `(0,0,1)` dentro de `Normal` tolerance |
| DefinitionOrigin | XYZ exacto, separado de ReferencePosition |

AUTH-08 V2 **no expone un `PlanarTransform` ni otro `Transform2D` compuesto**. Position XY + rotation +
Scale XY no representa de forma exacta definition-to-world cuando DefinitionOrigin es no cero o Normal
no es +Z. Presentarlo como transform completo seria ambiguo. `Transform2D` permanece autoridad V1 y puede
reutilizarse internamente o por consumidores solo cuando una operacion 2D concreta pruebe matematicamente
que sus inputs ya estan en el mismo plano y no omite origin/orientacion 3D. I-59 no define esa proyeccion.

P-12. No hay enums combinatorios `UnitHalfTurn`, `UnitReflectionXY` o `UnitNegativeZ`. Un caso puede ser
unit, uniform, half-turn y no reflection simultaneamente. Half-turn no reescribe signos ni rotacion;
Foundation solo ofrece hechos separados. Non-uniform conserva X/Y/Z y nunca inventa UniformScale.

P-13. Matriz AUTH-08:

| Caso | Ejes/entrada | Facts minimos | Outcome |
|---|---|---|---|
| identity | +1,+1,+1; normal +Z; origin 0 | unit, uniform, determinantXY +1, no reflection/half-turn/negative-Z | Facts |
| rotation + position | +1,+1,+1 | rotation canonical + Position XYZ; sin transform compuesto | Facts |
| non-unit uniform | +2,+2,+2 | non-unit, uniform | Facts |
| non-uniform | +2,+3,+4 | ejes intactos, non-unit, non-uniform; sin scalar inventado | Facts |
| reflection XY | -1,+1,+1 | unit, uniform, determinantXY -1, reflection, no half-turn | Facts |
| half-turn signs | -1,-1,+1 | unit, uniform, determinantXY +1, half-turn, no reflection | Facts |
| negative Z | +1,+1,-1 | unit, uniform, determinantXY +1, negative-Z | Facts |
| source `-pi` | no degenerado | rotation `+pi` | Facts |
| normal no world-Z | finita/no degenerada | Normal intacta, NormalIsWorldZ=false; sin proyeccion 2D | Facts |
| origin no cero | finito | DefinitionOrigin distinto de Position; sin transform definition-to-world | Facts |

## 6. AUTH-08 failure semantics

P-14. `RackSourceTransformFactsResult` distingue `Facts`, `NonFinite`, `Degenerate` e
`InvalidTolerance`. Son outcomes neutrales, no aceptacion/rechazo de dibujo.

- `NonFinite`: algun double requerido no finito; no crea Point3D/Vector3D parcial ni normaliza a cero.
- `Degenerate`: input finito con eje `ZeroWithinTolerance` o Normal de longitud dentro de cero. Conserva
  snapshot/ejes para diagnostico y no fabrica orientacion/reflection/world-Z concluyentes.
- `InvalidTolerance`: tolerancia no finita o negativa; no facts clasificados.
- Non-uniform, non-unit, reflection, half-turn, negative-Z y normal no world-Z NO son fallos.

El resultado no contiene `Supported`, `Rigid`, `Orthographic`, `Accepted`, `Remedy` o mensajes UI.

## 7. AUTH-12 V2 — RequirementRole total

P-15. `LibraryPieceRequirement` es carrier V2 por instancia, sin reemplazar `LibraryBlockRequirement` V1:

| Campo | Contrato |
|---|---|
| PieceId | string original; identidad por instancia; blank se conserva como problema estructural, no se inventa id |
| ViewAddress | `RackViewAddress` procedente exclusivamente del contexto tipado de preparacion (§8) |
| RequirementRole | `Required`, `OptionalVisual`, `NotApplicable` |
| LibraryKey | string original, incluido null/blank; sin trim/case rewrite |

No hay igualdad/dedup por LibraryKey. Dos piezas o vistas con la misma key producen dos requirements.
Una proyeccion separada puede deduplicar keys validas para I/O, sin reemplazar la lista trazable.

P-16. Funcion total de diseño I-59 para los 16 valores vigentes de `HeaderBlockRole`:

| HeaderBlockRole | RequirementRole | Evidencia reconstruida / razon de diseño |
|---|---|---|
| BasePlate | Required | pieza catalogada; drawer la materializa como block reference y reporta ausencia |
| Post | Required | pieza catalogada; misma ruta block-backed |
| Horizontal | Required | pieza estructural catalogada; misma ruta block-backed |
| Diagonal | Required | pieza estructural catalogada; misma ruta block-backed |
| ClosingHorizontal | Required | rol block-backed por la ruta total del drawer; hoy sin productor localizado |
| Separator | Required | pieza fisica emitida por builders; block-backed |
| Rail | Required | cama fisica; builder/BOM y drawer block-backed |
| Roller | Required | cama fisica; builder/BOM y drawer block-backed |
| Brake | Required | cama fisica; builder/BOM y drawer block-backed |
| Stop | Required | cama fisica; builder/BOM y drawer block-backed |
| Beam | Required | larguero/viga fisica; builders/BOM y drawer block-backed |
| Annotation | NotApplicable | `LateralHeaderDrawer` crea DBText; no usa library block |
| Dimension | NotApplicable | `LateralHeaderDrawer` crea RotatedDimension; no usa library block |
| Safety | Required | accesorio fisico catalogado; block-backed y contabilizado segun sistema |
| Tope | Required | larguero tope fisico catalogado; block-backed/BOM |
| Pallet | OptionalVisual | XML-doc vigente: referencia VISUAL y nunca BOM; sigue siendo block-backed opcional |

Regla equivalente cerrada:

```text
Annotation | Dimension -> NotApplicable
Pallet                 -> OptionalVisual
los otros 13 valores vigentes, enumerados uno por uno -> Required
```

La implementacion debe usar switch exhaustivo por cada valor nombrado; queda prohibido `default => Required`,
`default => NotApplicable` o cast tolerante. Un valor futuro/desconocido produce fallo visible
`UnknownSourceRole` y ningun requirement clasificado hasta revisar la matriz. Esta clasificacion es una
**decision de diseño I-59**, reconstruida desde el comportamiento actual; V1 no expresaba RequirementRole
y no se presenta como hecho historico.

## 8. AUTH-12 V2 — procedencia de ViewAddress y evolucion aditiva

P-17. La autoridad semantica de la vista es el `RackViewAddress address` ya recibido por
`RackViewPreparationAdapter.Prepare(resolved, address, frame, baseName)`, validado por `supports(address)`
y conservado en `RackPreparedView.Address`. El flujo V2 obligatorio es:

```text
prepared/request context RackViewAddress
    -> extractor contextual por instancia (payload + la misma address)
    -> LibraryPieceRequirement(PieceId, ViewAddress, Role, LibraryKey)
    -> availability fact por la misma instancia/address
```

Se prohibe derivar autoridad semantica parseando `HeaderBlockInstance.View`, `BaseName`, nombre generado,
strings de vista, nombres de grupo o indices UI. Esos valores pueden seguir siendo metadata/inputs legacy
para sus owners actuales, pero no originan ni corrigen `LibraryPieceRequirement.ViewAddress`.

P-18. Coexistencia aditiva con V1:

- `IRackBlockRequirementExtractor<TPayload>.Extract(payload)` permanece sin cambios y sigue produciendo
  `LibraryBlockRequirement` deduplicado para consumidores V1;
- V2 añade un extractor contextual separado que recibe `(payload, RackViewAddress)` y devuelve requirements
  por instancia; el nombre/firma mecanica exactos no se congelan;
- el punto de composicion V2 esta dentro de preparacion, despues de validar address y construir payload;
- factories/constructores/resultado V1 no cambian. Un overload/factory/resultado V2 aditivo transporta
  la lista rica; no agrega parametro obligatorio al port V1 ni cambia su resultado;
- ninguna implementacion V2 llama primero al extractor V1 para reconstruir identidad perdida;
- una proyeccion V2 -> keys V1 solo incluye keys presentes y puede deduplicar para query/import, conservando
  aparte todos los requirements V2 para reatachar facts por instancia/address.

P-19. `Required + blank LibraryKey` permanece trazable con `KeyState=KeyMissing`, no construye un V1
invalido y no se omite. OptionalVisual/NotApplicable con blank tambien preservan carrier; policy decide.

## 9. AUTH-12 availability y failure semantics

P-20. Tres ejes independientes:

| Facto | Valores | Decision |
|---|---|---|
| `LibraryAvailability` | `Ok`, `FileMissing`, `Unknown` | congelar como disponibilidad neutral de biblioteca |
| `RequirementKeyState` | `Present`, `KeyMissing` | congelar, separado de biblioteca/bloque |
| `LibraryBlockPresence` | `Present`, `BlockMissing`, `Unknown` | congelar por key consultable |
| `LibraryUnavailable` | — | no congelar como valor duplicado; queda causa/policy I-55 derivada |

Unknown nunca equivale a Ok/FileMissing/BlockMissing/import success. FileMissing describe biblioteca externa
no localizable. BlockMissing exige biblioteca consultable y key valida ausente. KeyMissing no llega a query/import.

P-21. Query/import siguen ports separados. Import opcional precede query final autoritativa. V2 consulta una
proyeccion de keys presentes/deduplicadas y reatacha cada observacion a todos los requirements originales,
preservando PieceId + RackViewAddress. Exception no se convierte silenciosamente en BlockMissing:
FileMissing solo con evidencia; demas fallos de observacion son Unknown. Policy queda fuera.

P-22. Matriz AUTH-12:

| Role | Key | Library | Block | Facts |
|---|---|---|---|---|
| Required | presente | Ok | Present | carrier completo + Ok/Present |
| Required | presente | Ok | ausente | carrier completo + Ok/BlockMissing |
| Required | blank | cualquiera | no consultable | carrier + KeyMissing; sin query/import |
| OptionalVisual | presente | Ok | ausente | carrier + Ok/BlockMissing; policy fuera |
| NotApplicable | cualquiera | cualquiera | no requerido | carrier; sin convertirlo en library obligation |
| consultable | presente | FileMissing | unknown | FileMissing + Unknown presence |
| consultable | presente | Unknown | unknown | Unknown + Unknown presence |
| dos piezas/vistas | misma key | cualquier | cualquier | dos carriers; una query permitida; dos facts reatados |
| source role futuro | cualquiera | cualquiera | cualquiera | UnknownSourceRole visible; sin default |

## 10. Compatibility y legacy

P-23. V2 es aditiva, no breaking:

- V1 `RackTransformFacts.Describe` conserva resultados, incluido non-uniform unsupported y angulo `-pi`;
- V2 no cambia ni sustituye `Transform2D`; tampoco presenta un compuesto 2D ambiguo;
- V1 LibraryBlockRequirement/extractor/query/import/flow y enum Found/Missing no cambian;
- `RackViewPreparationAdapter.Prepare` V1 y `RackPreparedView.Address` conservan firma/semantica;
- V2 nace por overload/factory/port/resultado adicional, no por parametro obligatorio ni reinterpretacion;
- I-55 G8/G9/G12 mantiene invalid key, missing after import, file missing y optional warning;
- I-52 conserva Transform2D/reflection policy, extractor/query/import y final-query authority;
- consumidores Foundation actuales compilan y observan el mismo comportamiento hasta migracion explicita.

No hay legacy persistido. Si F2 demuestra que V2 aditiva no puede preservar estos contratos, STOP y nueva
Proposal; no breaking silencioso.

## 11. No-goals y puntos de extension

No-goals: ID19/producto I-55, placement final, transform definition-to-world, common transform, dibujo,
reflection/window/anchor policy, recovery/mensajes/UI, persistencia, catalog repair, retry o batching global.

Puntos de extension:

1. factory/clasificador Application para `RackSourcePlacementInput`;
2. adapter Plugin AutoCAD -> input primitivo sin policy;
3. extractor contextual V2 `(typed payload, RackViewAddress)` -> `LibraryPieceRequirement`;
4. clasificador total `HeaderBlockRole` sin default;
5. proyector requirements -> keys consultables;
6. query/import V2 y reattachment por instancia/address.

## 12. Invariantes, pruebas y RED futuro

| ID | Invariante | Obligacion F2 |
|---|---|---|
| CT59-01 | Plugin captura; Application clasifica; cero AutoCAD fuera de Plugin | reference/adapter tests con doubles no triviales |
| CT59-02 | Position y DefinitionOrigin XYZ distintos sobreviven | mutar cada coordenada por separado |
| CT59-03 | NormalizePi(-pi)=+pi y rango `(-pi,pi]` | boundary theory -3pi/-pi/pi/3pi |
| CT59-04 | X/Y/Z/signos sobreviven; non-uniform no inventa scalar | 2/3/4, reflection, half-turn, negative-Z |
| CT59-05 | determinant/reflection son explicitamente reference-basis XY; no transform 2D compuesto | normal inclinada + origin no cero; API no afirma definition-to-world |
| CT59-06 | unit/uniform/reflection/half-turn/negative-Z independientes | matriz AUTH-08 y sabotaje por fact |
| CT59-07 | Normal/NormalIsWorldZ usan tolerance declarada | exacto, dentro/fuera, degenerada |
| CT59-08 | NonFinite/Degenerate/InvalidTolerance deterministas | NaN/Infinity/zero/tolerance negativa |
| CT59-09 | PieceId/address/role/key se conservan por instancia | dos piezas, dos addresses, misma key; no collapse |
| CT59-10 | ViewAddress es exactamente la address de Prepare | payload.View/BaseName/nombre/indice contradictorios no cambian address |
| CT59-11 | los 16 HeaderBlockRole tienen mapping exacto; futuro falla visible | theory exhaustiva + enum guard complementaria + UnknownSourceRole conductual |
| CT59-12 | Required+blank queda KeyMissing | null/blank/whitespace sin query/import |
| CT59-13 | Ok/FileMissing/Unknown y Present/BlockMissing/Unknown no colapsan | query fake por estado |
| CT59-14 | query/import separados y query final manda | call order y pre-import missing no sticky |
| CT59-15 | V1 sigue source compatible; V2 no reconstruye desde extractor V1 | compile/behavior consumers actuales + repeated key identity V2 |
| CT59-16 | schema/persistence/product diffs NONE | scope guard complementaria + diff review |

F1 esta cerrado y no se reejecuta para V2. El RED historico de `cbe817e...` permanece evidencia de gaps,
no de la firma final. CT59-01..16 son obligaciones futuras no compiladas; al abrir F2, solo despues del
Freeze acordado, se convierten primero en RED ejecutable con seleccion >0. Source guards no sustituyen
conducta.

## 13. Gates D/F0, F1, F2, F3, F4 y READY

| Gate | Estado/resultado | Evidencia |
|---|---|---|
| D/F0 | Proposal V2 completa + paquete Architect | Coordinator revisa V2; Architect adversarial posterior; Frozen sigue NO |
| F1 | CLOSED / ACCEPTED | `F1_CLOSURE_SHA=54319d33810d...`; no se reabre |
| F2 | NOT OPEN | tras ambos AGREED: CT59-01..14 RED->GREEN; modelo/adapter neutral; no producto/persistencia |
| F3 | NOT OPEN | CT59-15; integracion Foundation V2 aditiva y compatibilidad cross-consumer |
| F4 | NOT OPEN | matrices completas, CT59-16, conformance y readiness I-55 |
| READY | NOT OPEN | READY-01..09 en orden, exact-SHA, Freeze identity, conformance y evidencia requerida |

No se abre F2 sin acuerdos expresos de Coordinator y Architect sobre la misma identidad Freeze.

## 14. Matriz Owner Validation propuesta

| Escenario | Aplicabilidad I-59 | Propuesta |
|---|---|---|
| captura neutral AutoCAD -> snapshot | adapter sin consumidor visible activo | N/A, automatizado con boundary tests |
| clasificacion AUTH-08 | pura Application | N/A, tests conductuales |
| requirements/address/availability AUTH-12 | Application/ports; policy fuera | N/A, tests conductuales |
| ID19/multi-rack | pertenece a I-55 | no acreditar ni reutilizar OV I-55 |
| schema/persistence | diff NONE | N/A |

`Owner Validation especifica I-59 = N/A` sigue siendo propuesta, no aprobacion. Si el diff futuro toca
ruta Plugin visible o altera dibujo/comando, la matriz se revisa antes de READY.

## 15. Estado y decisiones pendientes

Coordinator debe revisar la disposicion CR59-V1-01..03 y la Proposal completa. Solo despues se invoca al
Architect sobre commit/ruta/blob exactos. Cualquier REQUIRED produce Proposal completa nueva. F1 permanece
cerrado; ninguna revision de diseño crea nuevo RED o autoriza produccion.

```text
F1 RED = ESTABLISHED
F1 CLOSURE = ACCEPTED
Proposal V2 = PUBLISHED / PENDING COORDINATOR REVIEW
Freeze = DRAFT / NOT FROZEN
Architect = NOT YET INVOKED
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```
