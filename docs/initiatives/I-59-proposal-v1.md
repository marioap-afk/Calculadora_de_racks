# I-59 — Proposal V1 / Consensus Freeze DRAFT

Frozen: NO

Status: DRAFT FOR COORDINATOR + ARCHITECT REVIEW

Coordinator: REVIEW REQUIRED

Architect: PENDING

F1 RED: ESTABLISHED

F1 CLOSURE: PENDING COORDINATOR REVIEW

F2: NOT OPEN

IMPLEMENTATION AUTHORIZATION = NO

Esta es la version completa propuesta para evolucionar AUTH-08 y AUTH-12. No es un Freeze acordado,
no abre F2 y no autoriza cambios de produccion. Autoridades: ADR-0044, `docs/FOUNDATIONS.md`, el
contrato I-59, `INITIATIVE_LIFECYCLE.md` §§4-6, la implementacion integrada de AUTH-08/AUTH-12,
I-52 y el G14 vigente de I-55 en `origin/feature/creacion-de-vistas`.

## 1. Resultado, ownership y limites

P-01. Shared View Foundation conserva ownership de hechos neutrales reutilizables. Plugin captura datos
de AutoCAD y los convierte en snapshots neutrales; Application clasifica. Los consumidores deciden
aceptacion, rechazo, remedio, anchor, proyeccion, dibujo y mensajes. I-59 no implementa ID19.

P-02. Se extienden AUTH-08 y AUTH-12 mediante contratos V2 aditivos. Los contratos V1 integrados siguen
disponibles y con el mismo comportamiento para I-52, I-55 G8/G9/G12 y consumidores actuales. No se crea
una segunda autoridad de transformacion: `Transform2D`, `Point3D` y `Vector3D` se reutilizan.

P-03. Limites cerrados: sin Rigid, Orthographic, CommonTransform2D, Relative Frame Window, accept/reject,
remedy, RACKMIRROR, AUTH-15, geometria, BOM, builders, Resolve, Prepare, naming, I-52 o I-55 productivo.
Sin cambio de schema, DTO persistido, store o migracion. Sin NuGet.

```text
Schema diff            = NONE
Persistence migration  = NONE
Plugin product policy  = NONE
I-55 / I-52 modification = NONE
```

## 2. Discovery Core DC-01..09

| ID | Confianza | Resultado |
|---|---|---|
| DC-01 | MEASURED | AUTH-08 V1 describe `Transform2D` uniforme/ortogonal no degenerado y expone Translation XY, angulo raw `Atan2`, determinant, reflection y UniformScale; no uniforme produce `NonUniformScale`. AUTH-12 V1 acepta solo Key no blank, deduplica case-insensitive, consulta `Found/Missing` y mantiene query/import separados. |
| DC-02 | MEASURED | `RackTransformFacts`/`Transform2D` y `LibraryBlockRequirements.cs` son los owners integrados; ADR-0044 asigna hechos puros a Application y adaptadores AutoCAD a Plugin. I-55 no adquiere ownership por consumirlos. |
| DC-03 | MEASURED | Ningun tipo AUTH-08/AUTH-12 V1 es DTO persistido ni Xrecord. La busqueda de documentos/stores no encontro serializacion de estos carriers. Schema/preservation de unknowns no participa. |
| DC-04 | RECONSTRUCTED | AUTH-08 actual: matriz pura -> `RackTransformFacts.Describe` -> consumidor. AUTH-12 actual: plan tipado -> extractor -> requirements deduplicados -> import opcional -> query final -> preparacion/placement consumidor. La captura AutoCAD 3D requerida por G14 aun no cruza esta frontera. |
| DC-05 | MEASURED | I-52 consume AUTH-08 y AUTH-12 V1. I-55 G8/G9/G12 consume requirement/query/import y reporta invalid key, missing block y unavailable library. G14 necesita Position XYZ, origin de definicion, rotacion, escalas y Normal. Tests Foundation y preparacion leen `Key`, igualdad, orden, disponibilidad y separacion de ports. |
| DC-06 | MEASURED | Tests vigentes cubren V1; F1 historico `cbe817e...` selecciono 36, 6 PASS/30 FAIL. El harness corregido cubre directamente contratos actuales y deja las obligaciones V2 como RED documental hasta que la API se congele. Huecos: snapshot 3D, clasificacion independiente y requirement por pieza/vista. |
| DC-07 | MEASURED | I-55 remoto no modifica los archivos Shared objetivo, pero consume AUTH-12 y G14 tocara la frontera Plugin; integrar secuencialmente y reconciliar desde main. I-52 no modifica esos archivos. No se autoriza cherry-pick a consumidores. |
| DC-08 | MEASURED | Se extiende Shared View Foundation integrada bajo ADR-0044: syntax/availability/policy separados; hechos puros en Application; AutoCAD solo Plugin; Unknown no equivale a ausencia/exito. El codigo actual coincide para V1 y es insuficiente para G14. |
| DC-09 | RECONSTRUCTED | Arquetipo `FOUNDATION EVOLUTION`. M-05 y M-06 activados; M-04 activado por nuevos resultados neutrales de fallo; M-01/02/03/07/08 no activados bajo esta propuesta. Detalle en §4. |

## 3. Expansiones EXP-01..09

| ID | Estado | Pregunta y salida |
|---|---|---|
| EXP-01 | NEGATIVA RAZONADA | No hay contradiccion clase A: ADR-0044 exige precisamente la evolucion neutral. El bootstrap dice origin solo con consumidor demostrado; I-55 G14 lo demuestra. La taxonomia combinatoria del primer RED no era autoridad y se corrige. |
| EXP-02 | NEGATIVA RAZONADA | Ownership no ambiguo: Foundation posee facts; Plugin adapta; consumer policy queda fuera. |
| EXP-03 | NEGATIVA RAZONADA | No existe camino persistido para los carriers y no se propone uno. |
| EXP-04 | ACTIVADA / CLOSED FOR DRAFT | Se inspeccionaron I-52, I-55 G8/G9/G12/G14 y consumidores Foundation. La decision resultante es V2 aditiva con V1 preservada. Requiere confirmacion Coordinator. |
| EXP-05 | ACTIVADA / CLOSED FOR DRAFT | Los invariantes sin pruebas actuales quedan enumerados en §11. No se compilan contra nombres imaginarios; seran RED ejecutable al abrir F2 sobre el Freeze acordado. |
| EXP-06 | ACTIVADA / CLOSED FOR DRAFT | §8 separa NonFinite, Degenerate, InvalidTolerance, FileMissing, Unknown, KeyMissing y BlockMissing sin convertirlos en policy. |
| EXP-07 | ACTIVADA / CLOSED FOR DRAFT | I-55 es rama activa consumidora y G14 es trigger; no hay diff directo en los archivos Shared objetivo. Orden: I-59 integra, publica receipt, I-55 reconcilia. |
| EXP-08 | ACTIVADA / CLOSED FOR DRAFT | Deuda visible: V1 rechaza non-uniform y pierde identidad por dedup; se preserva compatibilidad y no se presenta V1 como suficiente. El RED historico defectuoso se conserva, no se oculta. |
| EXP-09 | NEGATIVA RAZONADA | Ninguna M queda UNKNOWN en V1; Architect puede reabrir si identifica cambio material omitido. |

Coordinator debe confirmar las expansiones y responder cual expansion debio activarse y no fue activada.

## 4. Materialidad M-01..M-08

| ID | Estado | Razon |
|---|---|---|
| M-01 | NO | No cambia owner ni aparece otra autoridad; V2 sigue en Foundation. |
| M-02 | NO | Sin DTO/wire persistido, schema, fallback ni unknown-field change. |
| M-03 | NO | No se cablea comportamiento visible ni se altera documento existente; consumidores V1 siguen iguales. |
| M-04 | SI | Se congelan resultados neutrales explicitos para non-finite/degenerate y disponibilidad que V1 colapsa o rechaza. |
| M-05 | SI | AUTH-08/AUTH-12 son contratos consumidos por I-52/I-55 y otros kinds. |
| M-06 | SI | Se añaden carriers/clasificadores/ports V2 reutilizables. |
| M-07 | NO | No hay framework, registry, kernel ni mecanismo transversal generico. |
| M-08 | NO | ADR-0044 se conserva; no se reinterpreta ownership ni policy boundary. |

## 5. AUTH-08 V2 — captura y carriers propuestos

P-08. La frontera propuesta tiene tres pasos y no admite tipos AutoCAD fuera de Plugin:

```text
AutoCAD BlockReference + BlockTableRecord (Plugin)
    -> RackSourcePlacementInput (primitivos capturados)
    -> RackSourcePlacementSnapshot (Application, finito, puro)
    -> RackSourceTransformFactsResult (Application, hechos/clasificacion)
    -> consumer policy fuera de Foundation
```

Los nombres publicos anteriores son parte de esta propuesta V1 y deben revisarse; helpers/metodos privados
no se congelan. `RackSourcePlacementInput` transporta doubles crudos para Position XYZ, RotationRadians,
Scale X/Y/Z, Normal XYZ y DefinitionOrigin XYZ. Existe para que Application, no Plugin, clasifique
non-finite sin intentar construir `Point3D`/`Vector3D`, cuyos contratos exigen finitud.

P-09. Cuando todos los campos requeridos son finitos, `RackSourcePlacementSnapshot` reutiliza:

- `Point3D ReferencePosition`: Position de la referencia insertada;
- `double SourceRotationRadians`;
- `Vector3D ScaleFactors`, preservando X/Y/Z y sus signos;
- `Vector3D Normal`;
- `Point3D DefinitionOrigin`: origin del BlockTableRecord/definicion.

`ReferencePosition` y `DefinitionOrigin` nunca son aliases ni fallbacks uno del otro. DefinitionOrigin se
incluye porque I-55 G14 exige casos `Origin != 0` y pruebas de mutacion que fallen al ignorarlo. I-59 no
decide como se usa para colocar; solo lo preserva.

P-10. El clasificador recibe `RackSourceTransformTolerance` con tres inputs finitos y no negativos:
`Scale`, `AngleRadians` y `Normal`. No hay tolerancia global implicita. Un input invalido produce
`InvalidTolerance` y no hechos falsos.

P-11. `RackSourceTransformFacts` conserva el snapshot y expone hechos independientes, no nombres
combinatorios:

| Hecho | Semantica propuesta |
|---|---|
| ReferencePosition | XYZ exacto finito capturado |
| RotationRadians | `NormalizePi(SourceRotationRadians)` en `(-pi, pi]`; `NormalizePi(-pi) = +pi` |
| ScaleX/Y/Z | valores por eje con signo, sin sustituirlos por media/geometric mean |
| SignX/Y/Z | `Negative`, `ZeroWithinTolerance` o `Positive` por eje |
| PlanarTransform | `Transform2D` construido de rotacion, ScaleX/Y y Position XY; no borra Z/origin/normal |
| PlanarDeterminant | `ScaleX * ScaleY`; la rotacion no cambia determinant |
| IsReflection | `PlanarDeterminant < 0` fuera de tolerancia |
| IsUniformScale | magnitudes X/Y/Z iguales dentro de `Scale` |
| IsUnitScale | cada magnitud X/Y/Z igual a 1 dentro de `Scale` |
| HasPlanarHalfTurnSign | X e Y negativas fuera de tolerancia; independiente de unit/uniform/reflection |
| IsNegativeZ | Z negativa fuera de tolerancia |
| NormalIsWorldZ | normal igual a `(0,0,1)` dentro de `Normal` |
| DefinitionOrigin | XYZ exacto, separado de ReferencePosition |

No se congelan enums `UnitHalfTurn`, `UnitReflectionXY`, `UnitNegativeZ` ni cualquier producto cartesiano.
Un caso puede ser simultaneamente unit, uniform, half-turn y no reflection; los hechos se consultan aparte.
Non-uniform conserva ScaleX/Y/Z y clasificacion; nunca exige ni inventa `UniformScale = sqrt(X*Y)`.

P-12. Media vuelta de signos no reescribe los signos ni la rotacion fuente. `RotationRadians` canonicaliza
solo el angulo capturado. El consumidor puede componer signos y angulo; Foundation ofrece
`HasPlanarHalfTurnSign` como hecho adicional y no elige representacion geometrica equivalente.

P-13. Matriz AUTH-08 propuesta:

| Caso | Ejes | Clasificaciones minimas | Resultado neutral |
|---|---|---|---|
| identity | +1,+1,+1 | unit, uniform, no reflection, no half-turn, !negative-Z, world-Z | Facts |
| rotation + translation | +1,+1,+1 | unit/uniform; angulo canonical; Position XYZ | Facts |
| non-unit uniform | +2,+2,+2 | non-unit, uniform | Facts |
| non-uniform | +2,+3,+4 | non-unit, non-uniform; ejes intactos | Facts, no scalar uniforme inventado |
| reflection XY | -1,+1,+1 | unit, uniform, reflection, no half-turn | Facts |
| planar half-turn signs | -1,-1,+1 | unit, uniform, no reflection, half-turn | Facts |
| negative Z | +1,+1,-1 | unit, uniform, no planar reflection, negative-Z | Facts |
| source `-pi` | cualquier no degenerado | angulo `+pi` | Facts |
| normal no world-Z | cualquier no degenerado | `NormalIsWorldZ=false` | Facts |
| origin no cero | cualquier no degenerado | origin preservado distinto de position | Facts |

## 6. AUTH-08 failure semantics

P-14. `RackSourceTransformFactsResult` distingue `Facts`, `NonFinite`, `Degenerate` e
`InvalidTolerance`. Son outcomes neutrales, no accept/reject del dibujo.

- `NonFinite`: cualquier double requerido no finito. No crea Point3D/Vector3D parcial, no normaliza a cero,
  no lanza desde el adapter como sustituto del resultado.
- `Degenerate`: input finito donde algun eje esta en `ZeroWithinTolerance` o Normal tiene longitud dentro
  de cero. El resultado conserva el snapshot y los ejes finitos para diagnostico, marca degeneracy y no
  fabrica orientacion, reflection o world-Z concluyentes.
- `InvalidTolerance`: cualquier tolerancia no finita o negativa. No hay hechos clasificados.
- Non-uniform, non-unit, reflection, half-turn, negative-Z y normal no world-Z NO son fallos.

El resultado no contiene `Supported`, `Rigid`, `Orthographic`, `Accepted`, `Remedy` ni mensajes de UI.

## 7. AUTH-12 V2 — requirement por instancia

P-15. Se añade `LibraryPieceRequirement` como carrier V2 por instancia, sin reemplazar
`LibraryBlockRequirement` V1:

| Campo | Contrato propuesto |
|---|---|
| PieceId | string no null; conserva identidad de pieza; blank es structural invalid input, no se inventa id |
| ViewAddress | `RackViewAddress` cuando existe; puede usarse un value object neutral equivalente solo si Architect acredita que no crea dependencia circular |
| RequirementRole | `Required`, `OptionalVisual`, `NotApplicable` |
| LibraryKey | string original, incluido null/blank; no trim/case rewrite |

No existe igualdad/dedup del carrier por `LibraryKey`. Dos piezas o dos vistas con la misma clave producen
dos requirements. Una proyeccion separada puede deduplicar claves validas para I/O de query/import, pero
nunca sustituye ni reduce la lista trazable.

P-16. El mapping candidato desde `HeaderBlockRole` es una PROPUESTA DE DISEÑO, no hecho historico:

| Source role | RequirementRole propuesto |
|---|---|
| Beam | Required |
| Pallet | OptionalVisual |
| Annotation | NotApplicable |
| Dimension | NotApplicable |

Architect debe aceptar o cambiar esta tabla antes de Frozen=YES. F1 solo midio que la instancia fuente
conserva `Role`, `PieceId`, `View` y `BlockName` antes del extractor V1.

P-17. `Required + blank LibraryKey` permanece como requirement trazable con `KeyState=KeyMissing`.
No se construye un `LibraryBlockRequirement` V1 invalido y no se omite. OptionalVisual/NotApplicable con
blank tambien preservan el carrier; la policy decide si reporta o ignora.

## 8. AUTH-12 availability y failure semantics

P-18. Se congelan tres ejes independientes:

| Carrier/facto | Valores | Decision V1 |
|---|---|---|
| `LibraryAvailability` | `Ok`, `FileMissing`, `Unknown` | SI, enum neutral minimo pedido |
| `RequirementKeyState` | `Present`, `KeyMissing` | SI, separado de disponibilidad de archivo/bloque |
| `LibraryBlockPresence` | `Present`, `BlockMissing`, `Unknown` | SI, por clave consultable |
| `LibraryUnavailable` | — | NO como cuarto valor: es causa/policy de I-55 derivada de FileMissing/Unknown, no hecho neutral adicional |

`Unknown` nunca equivale a Ok, FileMissing, BlockMissing o import success. `FileMissing` describe la
biblioteca externa no localizable. `BlockMissing` solo se concluye cuando la biblioteca fue consultable
y la clave valida no existe. `KeyMissing` se concluye antes de query y no se envia al importer.

P-19. Query e import siguen ports separados. El orden V1 compatible continua siendo import opcional y
query final autoritativa. V2 recibe una proyeccion de claves presentes/deduplicadas para eficiencia y
reatacha cada observacion a todos los `LibraryPieceRequirement` correspondientes sin perder identidad.
Un import exception no se convierte silenciosamente en `BlockMissing`; el adapter devuelve
`FileMissing` cuando puede acreditarlo y `Unknown` en los demas fallos de observacion. Consumer policy,
warning, abort, place-and-report y retry quedan fuera.

P-20. Matriz AUTH-12 propuesta:

| Role | Key | Library | Block | Hechos neutrales |
|---|---|---|---|---|
| Required | presente | Ok | Present | carrier completo + Ok/Present |
| Required | presente | Ok | ausente | carrier completo + Ok/BlockMissing |
| Required | blank | cualquiera | no consultable | carrier completo + KeyMissing; sin query/import |
| OptionalVisual | presente | Ok | ausente | carrier completo + Ok/BlockMissing |
| NotApplicable | cualquiera | cualquiera | no requerido | carrier completo; policy fuera |
| cualquiera consultable | presente | FileMissing | unknown | FileMissing + Unknown presence |
| cualquiera consultable | presente | Unknown | unknown | Unknown + Unknown presence |
| dos PieceId/vistas | misma clave | cualquier | cualquier | dos carriers; una query permitida; dos facts reatados |

## 9. Compatibility y legacy

P-21. Se elige evolucion V2 aditiva, no breaking:

- `LibraryBlockRequirement`, `IRackBlockRequirementExtractor<T>`, `ILibraryBlockQuery`,
  `ILibraryBlockImporter`, `LibraryBlockAvailabilityFlow` y sus valores V1 no cambian;
- `RackTransformFacts.Describe(Transform2D, tolerance)` conserva resultados V1, incluido rechazo typed
  de non-uniform y angulo `-pi`; la canonicalizacion nueva vive en el clasificador V2;
- no se cambian constructores, enum values ni igualdad V1;
- adaptadores V2 nuevos pueden proyectar a V1 solo cuando Key esta presente, preservando orden estable;
- I-55 G8/G9/G12 mantiene invalid key, missing after import, file missing y optional warning; V2 suministra
  mejores hechos, no decide esos outcomes;
- I-52 conserva Transform2D/reflection policy, extractor/query/import y final-query authority;
- Foundation consumers actuales compilan y observan el mismo comportamiento hasta migracion explicita.

No hay persisted legacy. La compatibilidad es de API/runtime. Si F2 demuestra que V2 aditiva no puede
preservar estos contratos, STOP: requiere nueva Proposal y acuerdo, no breaking silencioso.

## 10. No-goals y puntos de extension

No-goals: producto I-55, ID19, placement final, common transform, dibujo, policy de reflection, window,
anchor, recovery, mensajes, UI, persistencia, catalogos, library repair, retry, batching global o naming.

Puntos de extension propuestos:

1. factory/clasificador puro Application para `RackSourcePlacementInput`;
2. adapter Plugin AutoCAD -> input primitivo, sin policy;
3. extractor V2 typed plan -> `LibraryPieceRequirement`;
4. proyector puro requirements -> claves consultables;
5. query/import V2 que produce disponibilidad neutral y reatacha facts.

Ningun punto permite que Application referencie AutoCAD ni que Plugin decida Required/Optional outcome.

## 11. Invariantes, pruebas y RED

| ID | Invariante congelable | RED/obligacion F2 |
|---|---|---|
| CT59-01 | Plugin captura, Application clasifica; cero AutoCAD fuera de Plugin | test de referencias + adapter con doubles no triviales |
| CT59-02 | Position y DefinitionOrigin XYZ distintos sobreviven | mutation test cambia cada coordenada por separado |
| CT59-03 | `NormalizePi(-pi)=+pi`, rango `(-pi,pi]` | boundary theory `-3pi,-pi,pi,3pi` |
| CT59-04 | X/Y/Z y signos sobreviven; non-uniform no inventa UniformScale | `2,3,4`, reflexion, half-turn, negative-Z |
| CT59-05 | unit/uniform/reflection/half-turn/negative-Z son hechos independientes | matriz AUTH-08 completa; sabotaje de cada boolean |
| CT59-06 | Normal y NormalIsWorldZ usan tolerancia declarada | exacto, dentro/fuera, normal degenerada |
| CT59-07 | NonFinite/Degenerate/InvalidTolerance tienen outcomes neutrales deterministas | NaN/Infinity/zero-axis/zero-normal/tolerance negativa |
| CT59-08 | PieceId/view/role/key se conservan por instancia | dos piezas, dos vistas, misma key; ningun collapse |
| CT59-09 | Required+blank queda visible como KeyMissing | blank/null/whitespace sin query/import |
| CT59-10 | roles se mapean solo segun tabla acordada | Beam/Pallet/Annotation/Dimension con oraculo typed concreto |
| CT59-11 | Ok/FileMissing/Unknown y Present/BlockMissing/Unknown no colapsan | query fake por estado y unknown injection |
| CT59-12 | query/import separados; query final manda | call order y pre-import missing no sticky |
| CT59-13 | V1 sigue binary/source compatible en consumidores actuales | suites Foundation, I-52 contract y I-55-compatible fixtures |
| CT59-14 | schema/persistence/product diffs permanecen NONE | scope guard complementaria + diff review |

El RED historico exacto permanece en `cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b`: 36 selected,
6 PASS, 30 FAIL. No es la API congelada: contiene oraculos ciegos y nombres combinatorios rechazados.
Su fuente se recupera exactamente con:

```powershell
git show cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b:tests/RackCad.Tests/I59F1CharacterizationTests.cs
```

El harness activo corregido caracteriza solo tipos concretos existentes. CT59-01..14 permanecen RED
documental no compilado hasta que Coordinator y Architect acuerden esta API; al abrir F2 se convierten
primero en pruebas compilables rojas, con seleccion >0, antes de produccion. Una guarda de fuente no
sustituye las pruebas conductuales.

## 12. Gates D/F0, F1, F2, F3, F4 y READY

| Gate | Resultado verificable | Evidencia de cierre |
|---|---|---|
| D/F0 | Discovery/EXP/M completos, Proposal autocontenida y paquete Architect | Coordinator revisa; Architect adversarial; Freeze sigue NO hasta ambos AGREED mismo SHA/ruta/blob |
| F1 | caracterizacion V1, RED historico durable y harness corregido verde | evidencia exact-SHA; focal >0; Core Full del cierre; CI propio leido |
| F2 | modelo neutral AUTH-08/12 + adapter de captura/clasificacion | CT59-01..12 RED->GREEN; no producto/persistencia; CI propio |
| F3 | integracion Foundation V2 aditiva y compatibilidad consumidores | CT59-13; suites relevantes I-52/I-55-compatible; CI propio |
| F4 | conformance cross-consumer y readiness I-55 | matrices completas, scope CT59-14, Core Full, Architect/Coordinator gate review, CI propio |
| READY | `INITIATIVE_LIFECYCLE` READY-01..09 en orden | rebase final, conformidad exact-SHA, Full requerido, builds, CI, Freeze identity, Candidate solo entonces |

No se abre F2 sin `Coordinator = AGREED` y `Architect = AGREED` sobre la misma identidad Freeze.
Cada cierre V2 cumple orden commit -> arbol limpio -> evidencia exact-SHA -> push -> CI propio leido.

## 13. Matriz Owner Validation propuesta

| OV | Aplicabilidad I-59 | Propuesta |
|---|---|---|
| captura neutral AutoCAD -> snapshot | No visible por si sola; adapter sin consumidor activo | N/A para I-59, cubrir automatizadamente con fakes/boundary tests |
| clasificacion AUTH-08 | pura Application | N/A, pruebas conductuales |
| requirements/availability AUTH-12 | pura Application/ports; policy fuera | N/A, pruebas conductuales |
| dibujo ID19 / multi-rack | fuera de I-59; pertenece a I-55 | no acreditar ni reutilizar OV de I-55 |
| schema/persistence | diff NONE | N/A |

Se propone `Owner Validation especifica I-59 = N/A` porque I-59 no cablea comportamiento visible ni
cambia comandos/dibujo. No esta aprobada. Si el diff F2-F4 termina tocando una ruta Plugin activa o altera
un resultado visible, la matriz debe revisarse antes de READY; no se puede retirar una obligacion aplicable
por conveniencia.

## 14. Estado y decisiones pendientes

Architect debe atacar al menos: suficiencia del input primitivo para non-finite; semantica de degeneracy;
independencia de half-turn/signs; inclusion justificada de DefinitionOrigin; tabla de roles; separacion
KeyMissing/BlockMissing/LibraryUnavailable; compatibilidad V2 aditiva; y viabilidad de CT59/gates/OV.

Coordinator debe revisar DC/EXP/M, aceptar o modificar alcance y gates, y cerrar F1 solo despues de leer
la evidencia exact-SHA y CI. Cualquier REQUIRED produce Proposal completa nueva; no se edita una clausula
material en un supuesto commit de Freeze.

```text
F1 RED = ESTABLISHED
F1 CLOSURE = PENDING COORDINATOR REVIEW
Freeze = DRAFT / NOT FROZEN
Architect = PENDING
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```
