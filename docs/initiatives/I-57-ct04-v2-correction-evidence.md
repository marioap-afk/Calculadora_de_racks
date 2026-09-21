# I-57 — CT-04 V2 — correccion de caracterizacion

```text
Trigger             = MATERIAL CONTRADICTION encontrada al abrir F2
F2 start            = 6448af15b07eddc9ca784d3cf3a67a7923ea1e78
Production changes  = NONE
Reader migrations   = NONE
F1 historical state = COMPLETE
F2 = BLOCKED
F3                  = NOT OPEN
```

## 1. Hallazgo que corrige esta evidencia

El fixture original de F1 declaro `unknown View token -> Invalid` y `known View with unsupported
Section/variant -> Invalid` para todos los kinds. Al abrir F2, la lectura directa de los consumidores mostro que esa
generalizacion no describe el producto:

- Selectivo lleva cualquier token que no sea lateral ni planta al bucket frontal y convierte cualquier Section
  frontal negativa a fondo 0.
- Dinamico lleva cualquier token que no sea frontal ni planta al bucket lateral durante redraw, convierte Section
  lateral negativa a poste 0 y cualquier Section frontal distinta de Entrance a Exit.
- Cabecera lleva cualquier token no planta a lateral.
- Cama no consulta View ni Section; su writer historico produce `null/-1`.
- Push Back y Cantilever validan estrictamente sus descriptores persistidos antes de redibujar.

Implementar CT-04 original habria cambiado comportamiento observable. El Coordinator declaro la contradiccion
material, exigio Proposal V5 y mantuvo F2 bloqueado. El fixture y evidencia F1 originales quedan intactos como
registro historico del hallazgo.

## 2. Objeto corregido y alcance

La matriz ejecutable es
`tests/RackCad.Tests/Fixtures/I57/ct04-v2-coercion-matrix.json`. Cubre los seis kinds y los ocho lectores censados:

| Lector | Lectura persistida | Frontera adicional |
|---|---|---|
| `RackSelectivoCommands` | no lateral/no planta → frontal; Section frontal negativa → fondo 0 | insercion elige fondo y puede pedir input |
| `RackDinamicoCommands` | redraw no frontal/no planta → lateral; negativa → poste 0 | insercion lateral negativa pide poste; unknown/negativa usa whole-system lateral |
| `RackPushBackCommands` | descriptor estricto; frontal 0..3, lateral base 0, planta -1 | lateral negativa pide poste; planta directa ignora Section |
| `RackCantileverCommands` | descriptor estricto; Whole -1, Station base 0 | existencia de Station usa la linea resuelta |
| `RackCabeceraCommands` | planta o lateral por fallback no-planta; Section ignorada | solicitud no planta inserta lateral |
| `RackCamaCommands` | kind fija una unica vista; descriptor historico `null/-1` | todo descriptor alterno se ignora |
| `RackCommandSupport` | predicado case-insensitive de planta; no es codec completo | cada consumer conserva su reaccion |
| `ProjectVariableMutationExecutor` | misma lectura Selectiva | un corte/fondo ausente aborta en vez de borrar |

La matriz separa `kindRows`, `decodeRows`, `consumerFallbacks` y `availabilityBoundaries`. Las solicitudes
interactivas no se presentan como envelope persistido y no se convierten artificialmente en address.

## 3. Taxonomia corregida

| Disposition | Address | Significado |
|---|---|---|
| `Canonical` | presente | sintaxis exacta que emite el writer canonico |
| `Canonicalizable` | presente | solo difiere una representacion normalizable, como casing; no aplica fallback de producto |
| `Coerced` | presente | el reader vigente transforma deterministamente la sintaxis a otra address soportada |
| `Invalid` | ausente | no existe una interpretacion persistida segura y neutral |

`Coerced` conserva la sintaxis original, la address resultante y un codigo estable. Si tambien existe una diferencia
de casing, gana `Coerced`; la sintaxis original sigue permitiendo observar el casing. `Canonicalizable` y `Coerced`
son hechos, no permisos para que un consumer continue.

Codigos caracterizados:

- `SelectiveNegativeFondoToZero`
- `SelectiveNonLateralNonPlantaToFrontal`
- `SelectiveDefaultFrontalAndFondoZero`
- `DynamicNonEntranceToExit`
- `DynamicNegativePostToZero`
- `DynamicNonFrontalNonPlantaToLateral`
- `DynamicDefaultLateralAndPostZero`
- `WholeViewSectionIgnored`
- `CabeceraNonPlantaToLateral`
- `CamaDescriptorIgnored`

## 4. Codec, availability y policy

```text
persisted syntax
    -> AUTH-03 Decode: original syntax + semantic address? + disposition + coercion code?
    -> AUTH-04 Availability(resolved system, address): physical facts
    -> consumer policy: accept/reject/warn/prompt/erase/abort/continue
```

Una address `Fondo(999)`, `Post(999)` o `Station(999)` puede ser sintacticamente `Canonical`. La inexistencia de
esa variante pertenece a AUTH-04 y produce `VariantNotPresent`; no vuelve invalida la sintaxis. Un address valido
contra otro kind resuelto produce `SystemDoesNotSupportKind`.

La seleccion interactiva del primer corte no pertenece al codec. En particular, Dinamico demuestra dos rutas:

- redraw persistido con view ausente/unknown y Section negativa → address coerced `Lateral/Post(0)`;
- solicitud de insercion unknown/negativa → builder lateral completo; una solicitud lateral/negativa pide al usuario
  un corte.

Confundirlas introduciria policy y estado del sistema en AUTH-03.

## 5. Round-trip

Decode nunca reescribe el DWG. Para `Canonical`, el encode canonico reproduce la sintaxis canonica. Para
`Canonicalizable` y `Coerced`, el encode de la address produce sintaxis canonica y puede diferir del input original.
No se promete round-trip literal de entradas coercionadas. F2 no autoriza una rescritura automatica.

Casos relevantes:

- Selectivo `future-view/3` → `Frontal/Fondo(3)` → `frontal/3`.
- Dinamico `frontal/27` → `Frontal/FlowEnd(Exit)` → `frontal/0`.
- Cabecera `future-view/-1` → `Lateral/Whole` → `lateral/-1`.
- Cama `future-view/42` → `Lateral/Whole` → `null/-1`.

Push Back no adopta el fallback interno de `DecodeSection` para secciones fuera de 0..3: el preflight del descriptor
persistido impide llegar a ese helper. Cantilever `AdapterSection` sigue fuera porque es una camara de pieza, no una
vista de rack.

## 6. Resultado de la correccion

La nueva suite `SharedViewFoundationCt04V2CharacterizationTests` verifica que:

1. los seis kinds, ocho lectores y cuatro dispositions estan presentes;
2. cada `Invalid` carece de address y cada `Coerced` lleva codigo estable;
3. los ataques obligatorios tienen fila explicita;
4. availability aparece despues de un decode exitoso;
5. cada fila apunta a un seam que sigue existiendo en produccion;
6. el fixture F1 original permanece historico y esta correccion es adicional.

No se cambio `src/`, schema, reader, builder, resolver, geometria, BOM, nombre, importacion o transaccion. R3 no
define la particion Invalid/Coerced y conserva ownership neutral; no requiere R4. La separacion de ADR-0044 entre
syntax, availability y policy queda reforzada y su decision no cambia.

```text
Material contradiction resolution = DEFINED IN PROPOSAL V5; REVIEW REQUIRED
Open Material                      = NONE inside the V5 draft
Open Minor                         = NONE
F2                                 = BLOCKED pending Coordinator + Architect AGREED on exact V5 SHA
```
