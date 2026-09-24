# I-59 — Proposal V3 / Consensus Freeze DRAFT

Frozen: NO

Status: PUBLISHED / PENDING COORDINATOR REVIEW

Coordinator: PENDING V3 REVIEW

Architect: CHANGES REQUIRED / RE-REVIEW PENDING

F1 RED: ESTABLISHED

F1 CLOSURE: ACCEPTED

F2: NOT OPEN

IMPLEMENTATION AUTHORIZATION = NO

Esta es la version completa propuesta para evolucionar AUTH-08 y AUTH-12. Sustituye V2 como objeto
de revision, conserva sus decisiones no cuestionadas e incorpora AR59-V2-24, AR59-V2-25 y los dos
OPTIONAL. No es un Freeze acordado, no reabre F1, no abre F2 y no autoriza produccion.

Autoridades: ADR-0044, `docs/FOUNDATIONS.md`, el contrato I-59,
`docs/INITIATIVE_LIFECYCLE.md` §§4-6, la implementacion integrada de AUTH-08/AUTH-12, I-52 y el G14
vigente de I-55 en `origin/feature/creacion-de-vistas`.

## 1. Disposicion de findings Architect V2

| Finding | Disposicion V3 |
|---|---|
| AR59-V2-24 | RESOLVED IN V3 / pending Architect re-review: DC-08 y CT59-17 distinguen codigo integrado de registro factual pendiente; F4 produce y verifica la redaccion AUTH-08/AUTH-12 y bloquea READY-04 si falta. |
| AR59-V2-25 | RESOLVED IN V3 / pending Architect re-review: P-11/P-13/P-14 y CT59-04..06 fijan clasificacion por eje y reflection por signos opuestos, sin segunda tolerancia sobre el producto. |
| OPTIONAL-01 | INCORPORATED: los carriers 3D no garantizan finitud; la factory/clasificador V2 valida antes de publicar un snapshot valido. |
| OPTIONAL-02 | INCORPORATED: el contrato mutable enlaza V3 y conserva solo estado, alcance resumido y coordinacion. |

CR59-V1-01..03 permanecen resueltos por las clausulas completas de V3: funcion total de roles (§7),
procedencia tipada de ViewAddress (§8) y ausencia de un `Transform2D` compuesto ambiguo (§5).
Ninguna disposicion emite acuerdo en nombre de Coordinator o Architect.

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
| DC-01 | MEASURED | AUTH-08 V1 describe `Transform2D` uniforme/ortogonal no degenerado y expone Translation XY, angulo raw `Atan2`, determinant, reflection y UniformScale; non-uniform produce `NonUniformScale`. AUTH-12 V1 acepta Key no blank, deduplica case-insensitive, consulta `Found/Missing` y mantiene query/import separados. |
| DC-02 | MEASURED | `RackTransformFacts`/`Transform2D`, `RackViewAddress`, `RackViewPreparationAdapter` y `LibraryBlockRequirements.cs` son owners integrados; ADR-0044 asigna facts puros a Application y adapters AutoCAD a Plugin. |
| DC-03 | MEASURED | Ningun carrier AUTH-08/AUTH-12 V1 es DTO persistido o Xrecord. No participa fallback legacy ni unknown-field persistence. |
| DC-04 | RECONSTRUCTED | AUTH-08: matriz pura -> `RackTransformFacts.Describe` -> consumidor. AUTH-12: `Prepare(resolved,address,...)` valida address, construye payload, extractor V1 obtiene keys, y `RackPreparedView` conserva la misma address. Import opcional precede query final. |
| DC-05 | MEASURED | I-52 consume AUTH-08/AUTH-12 V1. I-55 G8/G9/G12 consume requirement/query/import. G14 necesita Position XYZ, DefinitionOrigin, rotation, scales y Normal. `LateralHeaderDrawer` materializa Annotation/Dimension como entidades nativas y trata los demas roles como block references; Pallet es referencia visual. |
| DC-06 | MEASURED | F1 aceptado en `54319d33810d...`: RED historico 36 selected/6 PASS/30 FAIL preservado; el harness corregido y evidencia de cierre permanecen durables. Los huecos V2 estan en §12. |
| DC-07 | MEASURED | I-55 no modifica archivos Shared objetivo, pero consume AUTH-12 y G14 tocara la frontera Plugin. I-52 tampoco los modifica. Orden: I-59 integra y publica receipt; I-55 reconcilia desde main. |
| DC-08 | MEASURED | Shared View Foundation integrada y el codigo AUTH-08/AUTH-12 vigente fueron verificados bajo ADR-0044. `docs/FOUNDATIONS.md` no contiene hoy una entrada factual AUTH-08/AUTH-12; solo registra AUTH-13 en esta familia. La ausencia no es EXP-01 ni contradiccion normativa: es el entregable pendiente del extender, gobernado por P-24/CT59-17/F4. |
| DC-09 | RECONSTRUCTED | `FOUNDATION EVOLUTION`. M-04/M-05/M-06 activados; M-01/02/03/07/08 no activados. Los findings V1/V2 precisan el contrato sin cambiar esa conclusion. |

## 4. Expansiones y materialidad

### EXP-01..09

| ID | Estado | Salida |
|---|---|---|
| EXP-01 | NEGATIVA RAZONADA | Sin contradiccion clase A. ADR-0044 exige evolucion neutral. Origin esta justificado por I-55 G14. La entrada factual AUTH-08/AUTH-12 ausente de FOUNDATIONS es un entregable del extender, no una contradiccion. |
| EXP-02 | NEGATIVA RAZONADA | Ownership inequivoco: Foundation facts, Plugin adapter, consumer policy. |
| EXP-03 | NEGATIVA RAZONADA | No existe ni se propone camino persistido. |
| EXP-04 | ACTIVADA / CLOSED FOR DRAFT | I-52, I-55 G8/G9/G12/G14 y consumidores Foundation inspeccionados; V2 aditiva. |
| EXP-05 | ACTIVADA / CLOSED FOR DRAFT | Invariantes sin cobertura actual enumerados en §12; RED compilable solo despues del Freeze acordado. |
| EXP-06 | ACTIVADA / CLOSED FOR DRAFT | §§6 y 9 hacen visibles NonFinite/Degenerate/InvalidTolerance y estados de key/library/block. |
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
    -> RackSourcePlacementInput (primitivos crudos capturados)
    -> factory/clasificador V2 (Application, valida finitud)
    -> RackSourcePlacementSnapshot valido (Application, finito, puro)
    -> RackSourceTransformFactsResult (hechos/clasificacion)
    -> consumer policy fuera de Foundation
```

Los nombres publicos son candidatos revisables de V2; nombres privados no se congelan.
`RackSourcePlacementInput` conserva los doubles crudos para Position XYZ, RotationRadians, Scale X/Y/Z,
Normal XYZ y DefinitionOrigin XYZ. La factory/clasificador V2 verifica explicitamente cada double antes
de publicar un snapshot valido. `Point3D` y `Vector3D` se reutilizan como carriers; sus constructores no
rechazan NaN/Infinity y no son autoridad de finitud. El helper exacto de validacion no se congela.

P-09. Un input finito produce `RackSourcePlacementSnapshot` con:

- `Point3D ReferencePosition`, posicion de la referencia insertada;
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
| SignX/Y/Z | por cada eje: `abs(scale) <= ScaleTolerance` => `ZeroWithinTolerance`; `scale > ScaleTolerance` => `Positive`; `scale < -ScaleTolerance` => `Negative` |
| ReferenceBasisDeterminantXY | producto numerico crudo `ScaleX * ScaleY`; determinant de la base local XY, no definition-to-world transform |
| ReferenceBasisIsReflectionXY | para input no degenerado, `true` si y solo si SignX y SignY son opuestos, equivalente a `ReferenceBasisDeterminantXY < 0`; sin segunda tolerancia sobre el producto |
| IsUniformScale | magnitudes X/Y/Z iguales dentro de `ScaleTolerance` |
| IsUnitScale | cada magnitud X/Y/Z igual a 1 dentro de `ScaleTolerance` |
| HasPlanarHalfTurnSign | SignX y SignY son `Negative`; independiente de unit/uniform/reflection |
| IsNegativeZ | SignZ es `Negative` |
| Normal | vector fuente intacto |
| NormalIsWorldZ | normal igual a `(0,0,1)` dentro de `NormalTolerance` |
| DefinitionOrigin | XYZ exacto, separado de ReferencePosition |

Si X o Y es `ZeroWithinTolerance`, el input es `Degenerate` y no se publica una conclusion valida de
reflection. La magnitud de `ScaleX * ScaleY` nunca se compara otra vez contra `ScaleTolerance`.

AUTH-08 V2 **no expone un `PlanarTransform` ni otro `Transform2D` compuesto**. Position XY + rotation +
Scale XY no representa exactamente definition-to-world con DefinitionOrigin no cero o Normal distinta
de +Z. `Transform2D` permanece autoridad V1 y puede reutilizarse solo cuando una operacion 2D concreta
pruebe que sus inputs estan en el mismo plano y no omite origin/orientacion 3D. I-59 no define proyeccion.

P-12. No hay enums combinatorios `UnitHalfTurn`, `UnitReflectionXY` o `UnitNegativeZ`. Un caso puede ser
unit, uniform, half-turn y no reflection simultaneamente. Half-turn no reescribe signos ni rotacion;
Foundation solo ofrece facts separados. Non-uniform conserva X/Y/Z y nunca inventa UniformScale.

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
| frontera opuesta | tol=.01; X=-.010001,Y=+.010001 | Negative/Positive; determinant crudo negativo; reflection true aunque abs(producto) << tol | Facts, no degenerado |
| frontera ambos positivos | tol=.01; X=+.010001,Y=+.010001 | Positive/Positive; no reflection | Facts, no degenerado |
| frontera ambos negativos | tol=.01; X=-.010001,Y=-.010001 | Negative/Negative; half-turn; no reflection | Facts, no degenerado |
| frontera cero | tol=.01; cualquier X o Y con abs(axis)<=.01, incluido exactamente .01 | ZeroWithinTolerance; no conclusion reflection | Degenerate |
| source `-pi` | no degenerado | rotation `+pi` | Facts |
| normal no world-Z | finita/no degenerada | Normal intacta, NormalIsWorldZ=false; sin proyeccion 2D | Facts |
| origin no cero | finito | DefinitionOrigin distinto de Position; sin transform definition-to-world | Facts |

## 6. AUTH-08 failure semantics

P-14. `RackSourceTransformFactsResult` distingue `Facts`, `NonFinite`, `Degenerate` e
`InvalidTolerance`. Son outcomes neutrales, no aceptacion/rechazo de dibujo.

- `NonFinite`: algun double crudo requerido no es finito. El clasificador lo detecta antes de publicar
  snapshot valido; no normaliza a cero ni atribuye la garantia a `Point3D`/`Vector3D`.
- `Degenerate`: input finito con cualquier eje `ZeroWithinTolerance` o Normal de longitud dentro de cero.
  Puede conservar input/ejes diagnosticos, pero no publica facts validos de orientacion, reflection o
  world-Z. En particular, X/Y degenerado no produce `ReferenceBasisIsReflectionXY` concluyente.
- `InvalidTolerance`: tolerancia no finita o negativa; no facts clasificados.
- Non-uniform, non-unit, reflection, half-turn, negative-Z y normal no world-Z NO son fallos.

El resultado no contiene `Supported`, `Rigid`, `Orthographic`, `Accepted`, `Remedy` o mensajes UI.

## 7. AUTH-12 V2 — RequirementRole total

P-15. `LibraryPieceRequirement` es carrier V2 por instancia, sin reemplazar V1:

| Campo | Contrato |
|---|---|
| PieceId | string original; identidad por instancia; blank se conserva como problema estructural |
| ViewAddress | `RackViewAddress` procedente exclusivamente del contexto tipado de preparacion (§8) |
| RequirementRole | `Required`, `OptionalVisual`, `NotApplicable` |
| LibraryKey | string original, incluido null/blank; sin trim/case rewrite |

No hay igualdad/dedup por LibraryKey. Dos piezas o vistas con la misma key producen dos requirements.
Una proyeccion separada puede deduplicar keys validas para I/O sin sustituir la lista trazable.

P-16. Funcion total de diseño I-59 para los 16 valores vigentes de `HeaderBlockRole`:

| HeaderBlockRole | RequirementRole | Evidencia reconstruida / razon de diseño |
|---|---|---|
| BasePlate | Required | pieza catalogada; drawer block-backed y reporta ausencia |
| Post | Required | pieza catalogada; misma ruta block-backed |
| Horizontal | Required | pieza estructural catalogada; misma ruta block-backed |
| Diagonal | Required | pieza estructural catalogada; misma ruta block-backed |
| ClosingHorizontal | Required | ruta total block-backed; hoy sin productor localizado |
| Separator | Required | pieza fisica emitida por builders; block-backed |
| Rail | Required | cama fisica; builder/BOM y drawer block-backed |
| Roller | Required | cama fisica; builder/BOM y drawer block-backed |
| Brake | Required | cama fisica; builder/BOM y drawer block-backed |
| Stop | Required | cama fisica; builder/BOM y drawer block-backed |
| Beam | Required | larguero/viga fisica; builders/BOM y drawer block-backed |
| Annotation | NotApplicable | drawer crea DBText; no usa library block |
| Dimension | NotApplicable | drawer crea RotatedDimension; no usa library block |
| Safety | Required | accesorio fisico catalogado; block-backed |
| Tope | Required | larguero tope fisico catalogado; block-backed/BOM |
| Pallet | OptionalVisual | referencia visual, nunca BOM; block-backed opcional |

```text
Annotation | Dimension -> NotApplicable
Pallet                 -> OptionalVisual
los otros 13 valores vigentes, enumerados uno por uno -> Required
```

La implementacion usa switch exhaustivo por cada valor nombrado; se prohibe un `default` silencioso.
Un valor futuro/desconocido produce `UnknownSourceRole` visible y ningun requirement clasificado hasta
revisar la matriz. Esta clasificacion es decision de diseño I-59 reconstruida desde comportamiento actual;
V1 no expresaba RequirementRole y no se presenta como hecho historico.

## 8. AUTH-12 V2 — procedencia de ViewAddress y evolucion aditiva

P-17. La autoridad semantica de la vista es el `RackViewAddress address` ya recibido por
`RackViewPreparationAdapter.Prepare(resolved, address, frame, baseName)`, validado por `supports(address)`
y conservado en `RackPreparedView.Address`:

```text
prepared/request context RackViewAddress
    -> extractor contextual por instancia (payload + la misma address)
    -> LibraryPieceRequirement(PieceId, ViewAddress, Role, LibraryKey)
    -> availability fact por la misma instancia/address
```

Se prohibe derivar autoridad semantica parseando `HeaderBlockInstance.View`, `BaseName`, nombre generado,
strings de vista, nombres de grupo o indices UI. Pueden seguir siendo metadata legacy, pero no originan
ni corrigen `LibraryPieceRequirement.ViewAddress`.

P-18. Coexistencia aditiva con V1:

- `IRackBlockRequirementExtractor<TPayload>.Extract(payload)` queda sin cambios y sigue produciendo
  `LibraryBlockRequirement` deduplicado para consumidores V1;
- V2 añade extractor contextual separado `(payload, RackViewAddress)` por instancia; no se congela nombre;
- la composicion V2 ocurre dentro de preparacion, tras validar address y construir payload;
- factories/constructores/resultado V1 no cambian; V2 usa overload/factory/resultado adicional;
- V2 no llama primero a V1 para reconstruir identidad ya perdida;
- la proyeccion V2 -> keys V1 puede deduplicar solo para I/O y conserva todos los requirements V2.

P-19. `Required + blank LibraryKey` permanece trazable con `KeyState=KeyMissing`, no construye V1 invalido
y no se omite. OptionalVisual/NotApplicable con blank tambien preservan carrier; policy decide.

## 9. AUTH-12 availability y failure semantics

P-20. Tres ejes independientes:

| Facto | Valores | Decision |
|---|---|---|
| `LibraryAvailability` | `Ok`, `FileMissing`, `Unknown` | disponibilidad neutral de biblioteca |
| `RequirementKeyState` | `Present`, `KeyMissing` | separado de biblioteca/bloque |
| `LibraryBlockPresence` | `Present`, `BlockMissing`, `Unknown` | por key consultable |
| `LibraryUnavailable` | — | no se congela como valor duplicado; queda causa/policy derivada |

Unknown nunca equivale a Ok/FileMissing/BlockMissing/import success. FileMissing describe biblioteca externa
no localizable. BlockMissing exige biblioteca consultable y key valida ausente. KeyMissing no llega a I/O.

P-21. Query/import siguen ports separados. Import opcional precede query final autoritativa. V2 consulta
keys presentes/deduplicadas y reatacha cada observacion a requirements originales, preservando identidad.
Exception no se convierte silenciosamente en BlockMissing: FileMissing solo con evidencia; demas fallos
de observacion son Unknown. Consumer policy queda fuera.

P-22. Matriz AUTH-12:

| Role | Key | Library | Block | Facts |
|---|---|---|---|---|
| Required | presente | Ok | Present | carrier completo + Ok/Present |
| Required | presente | Ok | ausente | carrier completo + Ok/BlockMissing |
| Required | blank | cualquiera | no consultable | carrier + KeyMissing; sin query/import |
| OptionalVisual | presente | Ok | ausente | carrier + Ok/BlockMissing; policy fuera |
| NotApplicable | cualquiera | cualquiera | no requerido | carrier; sin library obligation |
| consultable | presente | FileMissing | unknown | FileMissing + Unknown presence |
| consultable | presente | Unknown | unknown | Unknown + Unknown presence |
| dos piezas/vistas | misma key | cualquier | cualquier | dos carriers; una query permitida; dos facts reatados |
| source role futuro | cualquiera | cualquiera | cualquiera | UnknownSourceRole visible; sin default |

## 10. Compatibility y legacy

P-23. V2 es aditiva, no breaking:

- V1 `RackTransformFacts.Describe` conserva resultados, incluido non-uniform unsupported y angulo `-pi`;
- V2 no cambia ni sustituye `Transform2D` ni presenta un compuesto 2D ambiguo;
- V1 LibraryBlockRequirement/extractor/query/import/flow y enum Found/Missing no cambian;
- `RackViewPreparationAdapter.Prepare` V1 y `RackPreparedView.Address` conservan firma/semantica;
- V2 nace por overload/factory/port/resultado adicional, no por parametro obligatorio;
- I-55 G8/G9/G12 mantiene invalid key, missing after import, file missing y optional warning;
- I-52 conserva Transform2D/reflection policy, extractor/query/import y final-query authority;
- consumidores Foundation actuales conservan compilacion y comportamiento hasta migracion explicita.

No hay legacy persistido. Si F2 demuestra que la evolucion aditiva no preserva estos contratos, STOP y
nueva Proposal; no breaking silencioso.

P-24. Plan factual de `docs/FOUNDATIONS.md`: no se publica ahora una entrada `STABLE`. Durante F4 y antes
de READY-04 se prepara la redaccion exacta de la entrada Shared View Foundation AUTH-08/AUTH-12 con el
esquema vigente: Name, Status, Authority, Persistence, Mutation contract, Extension point, Decision source,
Protecting tests, Known limitations y Last changed by. Cada campo describe solo lo que exista en el
Candidate I-59 y se verifica contra codigo, este Freeze, pruebas, consumers y schema/persistence reales.
La redaccion queda revisable antes de READY-04 conforme al Lifecycle §4.1; READY-04 queda bloqueado si
no existe o no es verificable. La modificacion efectiva de `docs/FOUNDATIONS.md` ocurre en el commit de
cierre documental conforme a Workflow §11.4, despues de conformar implementacion, no en el Candidate.

## 11. No-goals y puntos de extension

No-goals: ID19/producto I-55, placement final, transform definition-to-world, common transform, dibujo,
reflection/window/anchor policy, recovery/mensajes/UI, persistencia, catalog repair, retry o batching global.

Puntos de extension:

1. factory/clasificador Application para input crudo;
2. adapter Plugin AutoCAD -> input primitivo sin policy;
3. extractor contextual V2 `(typed payload, RackViewAddress)` por instancia;
4. clasificador total `HeaderBlockRole` sin default;
5. proyector requirements -> keys consultables;
6. query/import V2 y reattachment por instancia/address.

## 12. Invariantes, pruebas y RED futuro

| ID | Invariante | Obligacion futura |
|---|---|---|
| CT59-01 | Plugin captura; Application clasifica; cero AutoCAD fuera de Plugin | boundary tests con doubles no triviales |
| CT59-02 | Position y DefinitionOrigin XYZ distintos sobreviven | mutar cada coordenada por separado |
| CT59-03 | NormalizePi(-pi)=+pi y rango `(-pi,pi]` | boundary theory -3pi/-pi/pi/3pi |
| CT59-04 | X/Y/Z/signos sobreviven; regla exacta `abs<=tol` cero, `>tol` positivo, `<-tol` negativo; non-uniform no inventa scalar | 2/3/4 y boundaries exactamente/dentro/fuera de tol |
| CT59-05 | Determinant es X*Y crudo; para no degenerado reflection iff signos X/Y opuestos, sin segunda tolerancia; no transform compuesto | tol=.01 con -.010001/+.010001 refleja aunque abs(producto)<<tol; normal inclinada + origin no cero |
| CT59-06 | unit/uniform/reflection/half-turn/negative-Z independientes | pequeños fuera de tol: +/- refleja, +/+ y -/- no; cualquier eje exactamente/dentro de tol degenera sin reflection concluyente |
| CT59-07 | Normal/NormalIsWorldZ usan tolerance declarada | exacto, dentro/fuera, degenerada |
| CT59-08 | NonFinite/ Degenerate/InvalidTolerance deterministas; carriers no son finitude authority | NaN/Infinity crudos, zero y tolerance negativa |
| CT59-09 | PieceId/address/role/key se conservan por instancia | dos piezas, dos addresses, misma key; no collapse |
| CT59-10 | ViewAddress es exactamente la address de Prepare | metadata strings contradictoria no cambia address |
| CT59-11 | los 16 HeaderBlockRole tienen mapping exacto; futuro falla visible | theory exhaustiva + guard enum + UnknownSourceRole conductual |
| CT59-12 | Required+blank queda KeyMissing | null/blank/whitespace sin query/import |
| CT59-13 | Ok/FileMissing/Unknown y Present/BlockMissing/Unknown no colapsan | query fake por estado |
| CT59-14 | query/import separados y query final manda | call order y pre-import missing no sticky |
| CT59-15 | V1 sigue source compatible; V2 no reconstruye desde extractor V1 | consumers actuales + repeated key identity V2 |
| CT59-16 | schema/persistence/product diffs NONE | scope guard complementaria + diff review |
| CT59-17 | F4 produce antes de READY-04 la redaccion factual AUTH-08/AUTH-12 completa y verificable; FOUNDATIONS se publica solo en cierre documental | comprobar diez campos contra Candidate, codigo, Freeze, pruebas, consumers y schema/persistence; READY-04 falla si falta |

F1 esta cerrado y no se reejecuta. El RED historico de `cbe817e...` permanece evidencia de gaps, no de
la firma final. CT59-01..17 son obligaciones futuras no compiladas; tras Freeze acordado se convierten
primero en RED ejecutable con seleccion >0 cuando correspondan. Guards de fuente no sustituyen conducta.

## 13. Gates D/F0, F1, F2, F3, F4 y READY

| Gate | Estado/resultado | Evidencia |
|---|---|---|
| D/F0 | Proposal V3 completa + paquete Architect V3 | Coordinator revisa V3; mismo Architect re-revisa completa; Frozen sigue NO |
| F1 | CLOSED / ACCEPTED | identidad aceptada `54319d33810d...`; no se reabre |
| F2 | NOT OPEN | tras ambos AGREED: CT59-01..14 RED->GREEN; modelo/adapter neutral; no producto/persistencia |
| F3 | NOT OPEN | CT59-15; integracion Foundation V2 aditiva y compatibilidad cross-consumer |
| F4 | NOT OPEN | matrices completas; CT59-16; CT59-17 produce redaccion exacta de los diez campos y la verifica contra Candidate/codigo/Freeze/tests/consumers/schema/persistence; conformance y readiness I-55 |
| READY | NOT OPEN | READY-01..09 en orden; READY-04 bloqueado si la redaccion CT59-17 no existe o no es verificable; publicacion FOUNDATIONS solo en cierre documental posterior |

No se abre F2 sin acuerdos expresos de Coordinator y Architect sobre la misma identidad Freeze.

## 14. Matriz Owner Validation propuesta

| Escenario | Aplicabilidad I-59 | Propuesta |
|---|---|---|
| captura neutral AutoCAD -> snapshot | adapter sin consumidor visible activo | N/A, automatizado con boundary tests |
| clasificacion AUTH-08 | pura Application | N/A, tests conductuales |
| requirements/address/availability AUTH-12 | Application/ports; policy fuera | N/A, tests conductuales |
| ID19/multi-rack | pertenece a I-55 | no acreditar ni reutilizar OV I-55 |
| schema/persistence | diff NONE | N/A |
| redaccion FOUNDATIONS | hecho documental verificable, no comportamiento Owner | N/A, CT59-17 y revision de conformidad |

`Owner Validation especifica I-59 = N/A` sigue siendo propuesta, no aprobacion. Si el diff futuro toca
ruta Plugin visible o altera dibujo/comando, la matriz se revisa antes de READY.

## 15. Estado y decisiones pendientes

Coordinator debe revisar Proposal V3 completa. El mismo Architect que emitio AR59-V2-24/25 debe decidir
`RESOLVED | OPEN` para cada uno y re-revisar V3 completa. Solo ese Architect puede rebajar sus REQUIRED.
F1 permanece cerrado; ninguna revision de diseño crea nuevo RED o autoriza produccion.

```text
F1 RED = ESTABLISHED
F1 CLOSURE = ACCEPTED
Proposal V3 = PUBLISHED / PENDING COORDINATOR REVIEW
AR59-V2-24 = RESOLVED IN DRAFT / PENDING ARCHITECT
AR59-V2-25 = RESOLVED IN DRAFT / PENDING ARCHITECT
Architect = CHANGES REQUIRED / RE-REVIEW PENDING
Coordinator = PENDING V3 REVIEW
Freeze = DRAFT / NOT FROZEN
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```
