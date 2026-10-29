# I-66 — Freeze: PlacedClone copia los campos de texto y de cota

Frozen: NO

Unit: `I-66`. Workflow: V2. Archetype: EXTENSION. Rama `fix/placedclone-copia-anotaciones`, base `bb0d5522`.
Contrato: [I-66-placedclone-copia-anotaciones.md](I-66-placedclone-copia-anotaciones.md). Decisiones: [decisions/I-66.md](../automation/decisions/I-66.md).
Discovery: [I-66-discovery.md](I-66-discovery.md).

Una vez congelado es inmutable; todo cambio posterior es una enmienda `A-n` en el registro de decisiones.

## 1. Autoridad

`HeaderRunPlan` (Application) sigue siendo el único dueño de `Flatten` y de su clon por colocación (`PlacedClone`, nombre no congelado). Su contrato no
cambia: `Flatten` es el equivalente plano del plan estructurado, es decir, de lo que dibuja `LateralHeaderDrawer` al insertar cada grupo como referencia
anidada (escala X −1 en una colocación espejada). Sin autoridad nueva.

## 2. Comportamiento observable

Para cada instancia `s` de un grupo y cada colocación `p`, el clon plano `c`:

1. **F-1.** `c.Text = s.Text`, `c.TextHeight = s.TextHeight` y `c.DimensionStyleName = s.DimensionStyleName`, con o sin espejo.
2. **F-2.** `c.DimensionOffset = −s.DimensionOffset` si y solo si `p` es espejada, `s.Role = Dimension` y `s` es vertical
   (`|s.ConnectionAnchor.X − s.Insertion.X| < |s.ConnectionAnchor.Y − s.Insertion.Y|`, el mismo criterio de eje que el dibujante y que
   `PushBackMirror.Instance`). En cualquier otro caso, `c.DimensionOffset = s.DimensionOffset`.
3. **F-3 (conservación).** Los demás campos del clon se calculan exactamente como en la base: rol, pieza, bloque, vista, rotación, `MirroredX` (invertido
   con el espejo), `MirroredY`, puntos y parámetros dinámicos. Las instancias sueltas se siguen añadiendo por referencia.
4. **F-4 (sin cambio hoy).** Ningún constructor actual mete un decorado en un grupo (Discovery DC-05), así que todas las salidas actuales de `Flatten`,
   del dibujo, de la vista previa y del BOM quedan iguales.

## 3. Persistencia, fallo y compatibilidad

Sin persistencia, esquema, DTO ni fallback legacy (DC-03). La semántica de fallo no cambia: no hay validación nueva ni excepción nueva. Compatibilidad:
total, porque F-4 deja iguales todas las salidas actuales.

## 4. No-objetivos

Meter decorados en grupos; cambiar el agrupador (prototipo, firma o regla de sueltos), los `RigidClone` de cama, `CloneAt` de Push Back, los clones
`Translated`/`Translate` del Selectivo, `PushBackMirror`, el `MirroredX` y la rotación de los decorados espejados, el dibujante, la UI o la vista previa.
Los clones hermanos que tampoco copian los campos de cota (DC-05) se registran como hallazgo lateral en `docs/ideas-futuras.md` al cerrar, sin arreglarlos.

## 5. Puntos de extensión

Ninguno nuevo ni cambiado.

## 6. Obligaciones invariante → prueba (RED esperado)

Pruebas Core en un archivo propio, sobre `HeaderRunPlan` construido a mano con un grupo y colocaciones normal y espejada:

| Id | Invariante | Prueba | RED esperado en la base |
|---|---|---|---|
| N-1 | F-1 para una anotación | `Text` y `TextHeight` del clon iguales a los de la fuente, normal y espejada | falla: `null` y `0` |
| N-2 | F-1 y F-2 para una cota horizontal | `DimensionOffset`, `DimensionStyleName` y `TextHeight` iguales, normal y espejada | falla: `0`, `null` y `0` |
| N-3 | F-2 para una cota vertical | normal: `DimensionOffset` igual; espejada: con el signo opuesto; `DimensionStyleName` y `TextHeight` iguales | falla: `0` y `null` |
| N-4 | F-2 equivale al espejo de Push Back | para una cota vertical y una horizontal en una colocación espejada con `InsertionY = 0`, los puntos y los cuatro campos del clon son iguales a los de `PushBackMirror.Instance(s, p.InsertionX)`, que refleja `x → InsertionX − x` igual que la colocación | falla: los campos del clon quedan por defecto |
| N-5 | F-3 | una pieza de bloque: los campos actuales del clon (puntos, espejos, rotación, parámetros) son los de la base y los cuatro campos quedan por defecto; los sueltos son las mismas referencias | pasa en la base y después (conservación) |
| N-6 | F-4 | las suites Core existentes (incluidas las firmas y los hashes de `Flatten`) siguen verdes sin cambios | pasa en la base y después |

N-1..N-4 deben fallar en la base por la causa indicada antes de la corrección; la prueba N-4 no puede ser un oráculo ciego porque compara contra una
implementación independiente ya existente.

## 7. Matriz OV

| Id | Escenario | Evidencia |
|---|---|---|
| OV-I66-01 | El Owner revisa el RED → GREEN de N-1..N-4 y la conservación N-5/N-6 sobre el Candidato | pruebas Core y CI del SHA del Candidato; sin escenario de AutoCAD porque ninguna salida dibujada cambia (F-4) |

## 8. Plan de gates

| Gate | Resultado verificable | Cierre |
|---|---|---|
| G1 | `Flatten` conserva los cuatro campos según F-1..F-3, probado por N-1..N-6 | RED → GREEN focal, Core Full sobre el SHA de cierre, CI de ese SHA en verde y revisión del Coordinator (INITIATIVE_LIFECYCLE §7) |

Después de G1: READY, Candidato y Owner Validation según WORKFLOW; I-66 no se integra sin su Owner Validation.
