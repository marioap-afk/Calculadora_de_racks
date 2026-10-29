# I-66 — Discovery Core (PlacedClone copia los campos de texto y de cota)

Unit: `I-66`. Workflow: V2. Rama `fix/placedclone-copia-anotaciones`, base `bb0d5522`. Contrato:
[I-66-placedclone-copia-anotaciones.md](I-66-placedclone-copia-anotaciones.md). Decisiones: [decisions/I-66.md](../automation/decisions/I-66.md).

Barrido de solo lectura sobre la base. Cada afirmación lleva su clase: `MEASURED` (leído en el código), `INFERENCE` o `UNKNOWN`. Las rutas son de
`src/` salvo que se diga otra cosa.

## DC-01. Comportamiento observable actual

- `HeaderRunPlan.Flatten` (`RackCad.Application/Drawing/HeaderRunPlan.cs:31-51`) emite `PlacedClone(instance, placement)` por cada instancia de cada
  grupo y cada colocación, y después añade `LooseInstances` **por referencia, sin clonar** (`:49`). MEASURED.
- `PlacedClone` (`:53-80`) copia `Role`, `PieceId`, `BlockName`, `View`, `RotationRadians`, `MirroredX` (invertido si la colocación es espejada),
  `MirroredY`, `ConnectionAnchor`, `Insertion` (X espejada respecto de `InsertionX`, Y desplazada por `InsertionY`) y `DynamicParameters`. **No copia**
  `Text`, `TextHeight`, `DimensionOffset` ni `DimensionStyleName`: el clon los deja en `null`, `0`, `0` y `null`. MEASURED.
- Ningún camino actual coloca dentro de un `HeaderGroup` una anotación, una cota ni una instancia con alguno de esos cuatro campos distinto del valor
  por defecto (DC-05). El defecto es **latente**. MEASURED.

## DC-02. Autoridad

- `HeaderRunPlan` (Application, sin tipos de AutoCAD) es dueño de `Flatten`. Su contrato documentado es reproducir el plan estructurado como un layout
  plano (`HeaderRunPlan.cs:27-30`); `HeaderInstanceGrouper` documenta que `Flatten` reproduce su entrada como multiconjunto
  (`HeaderInstanceGrouper.cs:10-18`). MEASURED.
- La semántica de los campos la fija `HeaderBlockInstance` (`HeaderBlockInstance.cs:86-101`): `DimensionOffset` es la distancia perpendicular con signo
  del segmento medido a la línea de cota, y el eje sale de los dos puntos. MEASURED.
- El dibujante del Plugin dibuja una cota con su línea en `p1.Y + DimensionOffset` si es horizontal y en `p1.X + DimensionOffset` si es vertical
  (`RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:338-376`); una colocación espejada inserta la referencia anidada con escala X −1 (`:59-62`, `:143-146`).
  MEASURED. Por eso, en el equivalente plano de una colocación espejada, una cota vertical tiene el desplazamiento con el signo opuesto. INFERENCE,
  derivada de las dos líneas anteriores.
- Precedente en el código: `PushBackMirror.Instance` (`RackCad.Application/Systems/PushBack/PushBackMirror.cs:43-75`) refleja respecto de un eje vertical,
  copia los cuatro campos e invierte el signo de `DimensionOffset` solo en cotas verticales (mismo criterio de eje que el dibujante). MEASURED.
- Ningún ADR ni Freeze integrado gobierna `PlacedClone`. MEASURED (sin referencias en `docs/adr/`).

## DC-03. Persistencia

`HeaderBlockInstance` y `HeaderRunPlan` son datos en memoria: nada en `src/` los serializa ni los deserializa. Sin DTO, Xrecord, store ni fallback
legacy afectados. MEASURED.

## DC-04. Camino

Builders de vista (Selectivo, Dinámico, Push Back, cama) → `HeaderGroup` + sueltos → `HeaderRunPlan` → dibujante (`LateralHeaderDrawer`, que **no** llama
a `Flatten`: construye la definición anidada desde `group.Instances`) o → `Flatten` en Application y UI (DC-05). MEASURED.

## DC-05. Llamadores, consumidores y lectores

- **Quién da valor a los cuatro campos:** solo `SelectiveAnnotations.Label` (Annotation), `SelectiveDimensions.Dim` y `DynamicViewDecorations.Dimension`
  (Dimension), siempre con `BlockName` nulo; los clones de `SelectivePlantaBuilder`, `SelectiveLateralBuilder` y `PushBackMirror` los arrastran con su rol.
  Plugin y UI no los escriben. MEASURED.
- **Constructores de `HeaderGroup`** (`HeaderInstanceGrouper:95`, `DynamicSystemLateralBuilder:490`, `DynamicFlowBedLateralBuilder:83`,
  `DynamicSystemPlantaBuilder:65`, `SelectivePlantaBuilder:319`, `PushBackFlowBedLateralBuilder:150`, `PushBackMirror:105`, `PushBackBootPlan:202`,
  `PushBackDefensePlan:171`, `PushBackPalletProjection:231`, `PushBackCompositeFrontal:124`): todos parten de piezas estructurales o de cama, o filtran y
  reflejan esos grupos. El agrupador deja sueltas las anotaciones, las cotas y las piezas sin bloque (`HeaderInstanceGrouper.cs:37-43`). Ninguno puede
  meter un decorado en un grupo. MEASURED (los cuatro filtros de Push Back: INFERENCE, heredan de grupos que no los tienen).
- **Llamadores de `Flatten` fuera de pruebas:** `DynamicSystemPlantaBuilder:84`, `DynamicSystemLateralBuilder:250`, `PushBackSystemPlantaBuilder:72`
  (aplana, filtra y reagrupa en `:157` antes de dibujar), `PushBackCompositePlanta:45/200/288` (aplana y reagrupa en `:62`), y en UI
  `PushBackPreviewModel:114`, `PushBackPreviewRenderer:107` y `RackDynamicSystemWindow.xaml.cs:2640`. Ninguno lee los cuatro campos de un clon. Ningún BOM
  llama a `Flatten`. MEASURED.
- **Otros clones que tampoco copian todos los campos** (fuera de alcance; DC-09): prototipo del agrupador (`HeaderInstanceGrouper.cs:72-89`), los dos
  `RigidClone` de cama, `CloneAt` de Push Back, y `Translated`/`Translate` del Selectivo (copian `Text` y `TextHeight` pero no la cota). MEASURED.
- Inventario: búsqueda de `new HeaderGroup(`, `Flatten()`, `new HeaderBlockInstance` y lecturas de los cuatro campos en `src/` y `tests/`. Sin huecos
  conocidos.

## DC-06. Pruebas y guardas existentes

- Ninguna prueba mete un decorado dentro de un grupo y aplana; ninguna nombra `PlacedClone`. MEASURED.
- Protegen los decorados **sueltos** a través de `Flatten`: la firma compartida `tests/RackCad.Tests/DimensionLegacySignature.cs:90` (con las
  caracterizaciones de cotas de Dinámico T04, Push Back T05 y Selectivo T03), `DimensionViewsPhysicalAndBomInvarianceTests` T16,
  `DynamicNullOverrideGoldenTests` (hash de `Flatten`), `DynamicSystemMultiViewBuilderTests`, varias pruebas de Push Back sobre `Text` y pruebas de UI.
  MEASURED.
- `HeaderInstanceGrouperTests.Group_PreservesMirroredY_InDefinitionAndThroughFlatten` es la única prueba de un campo que sobrevive a `Flatten`.
  **Hueco:** nada prueba los cuatro campos dentro de un grupo, ni el signo de la cota vertical bajo una colocación espejada.

## DC-07. Archivos calientes e intersecciones

`HeaderRunPlan.cs` no es archivo caliente de WORKFLOW §7. Al reclamar, ninguna rama remota activa tenía commits sobre él fuera de `main`
(decisiones de I-62 §215). La prueba nueva va en un archivo propio. MEASURED.

## DC-08. Fundaciones

No consume ni extiende ninguna fundación registrada: `HeaderRunPlan` y `HeaderBlockInstance` no son puntos de extensión declarados. MEASURED.

## DC-09. Materialidad y arquetipo

| M | Estado | Evidencia |
|---|---|---|
| M-01 | no activado | el dueño de `Flatten` sigue siendo `HeaderRunPlan`; ninguna segunda autoridad |
| M-02 | no activado | sin esquema, DTO, wire ni persistencia (DC-03) |
| M-03 | no activado | cambia solo la salida de `Flatten` para entradas que hoy no existen (DC-05); es exactamente H4, lo pedido; todas las salidas actuales quedan iguales |
| M-04 | no activado | no cambia qué falla ni cómo |
| M-05 | no activado | el contrato de `Flatten` (equivalente plano del plan) no cambia: el arreglo lo cumple para más entradas |
| M-06 | no activado | ningún punto de extensión |
| M-07 | no activado | ningún mecanismo transversal |
| M-08 | no activado | ningún ADR ni Freeze integrado gobierna `PlacedClone` |

**Arquetipo propuesto: EXTENSION.** Expansiones evaluadas: EXP-01..EXP-09 no se activan. EXP-08 (defecto preexistente que el cambio haría visible) se
evaluó en negativo: el arreglo no hace visible ningún otro defecto; los clones hermanos de DC-05 siguen igual de latentes y quedan fuera de alcance.
