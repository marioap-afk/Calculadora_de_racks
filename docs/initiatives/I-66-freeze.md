# I-66 — Freeze: PlacedClone copia los campos de texto y de cota

Frozen: NO

Unit: `I-66`. Workflow: V2. Archetype: EXTENSION. Rama `fix/placedclone-copia-anotaciones`, base `bb0d5522`.
Contrato: [I-66-placedclone-copia-anotaciones.md](I-66-placedclone-copia-anotaciones.md). Decisiones: [decisions/I-66.md](../automation/decisions/I-66.md).
Discovery: [I-66-discovery.md](I-66-discovery.md). Versión R3, tras la revisión R1 (ARCH-A-01..06, ARCH-B-01..10) y la re-revisión R2
(ARCH-A-07..12, ARCH-B-11..14) del Architect; decisiones §6, §6.1 y §6.2.

Una vez congelado es inmutable; todo cambio posterior es una enmienda `A-n` en el registro de decisiones.

## 1. Autoridad

`HeaderRunPlan` (Application) sigue siendo el único dueño de `Flatten` y de su clon por colocación (`PlacedClone`, nombre no congelado). Gobierna
**ADR-0011** (aceptado): «aplanar el plan debe reproducir las instancias originales». Literalmente cubre la ida y vuelta del agrupador, que nunca crea
colocaciones espejadas (`HeaderInstanceGrouper.cs:91-93`). Este Freeze lo extiende, como decisión propia, a las colocaciones espejadas de los builders:
`Flatten` es el equivalente plano de lo que dibuja `LateralHeaderDrawer` al insertar cada grupo como referencia anidada (escala X −1 en una colocación
espejada). Esa equivalencia es **geométrica** y vale para los puntos, los espejos, la
identidad de la pieza, los parámetros dinámicos y, con F-1 y F-2, los cuatro campos de texto y de cota y la posición de la línea de cota. **No** vale, y
este Freeze no la cambia, para:

- `RotationRadians` de las piezas rotadas en una colocación espejada: el clon conserva θ y el dibujo produce −θ (Discovery DC-02, EXP-01 clase B);
- la orientación del texto y el lado del texto de una cota dentro de una referencia espejada: el dibujante no aplica `MirroredX` ni `RotationRadians` a
  los decorados (`LateralHeaderDrawer.cs:258-282`, `:338-376`).

Sin autoridad nueva. `PlacedClone` no deriva los cuatro campos de `PushBackMirror` (sin dependencia de `Drawing` hacia `Systems.PushBack`).

## 2. Comportamiento observable

Para cada instancia `s` de un grupo y cada colocación `p`, el clon plano `c`:

1. **F-1.** Para **todo rol**, `c.Text = s.Text`, `c.TextHeight = s.TextHeight` y `c.DimensionStyleName = s.DimensionStyleName`, con o sin espejo.
2. **F-2.** `c.DimensionOffset = −s.DimensionOffset` si y solo si `p` es espejada, `s.Role = Dimension` y `s` es vertical:
   `|s.ConnectionAnchor.X − s.Insertion.X| < |s.ConnectionAnchor.Y − s.Insertion.Y|`, evaluado sobre la fuente. El empate (`|dx| = |dy|`, incluido el
   segmento de longitud cero) no es vertical y conserva el desplazamiento. Es el mismo criterio que el dibujante, que trata el empate como horizontal
   (`LateralHeaderDrawer.cs:345`) y lo evalúa sobre la instancia de la definición anidada, es decir, sobre la fuente. En cualquier otro caso,
   `c.DimensionOffset = s.DimensionOffset`. F-2 es la forma de «copiar» que respeta el espejo; la decide el Coordinator (decisiones §6.1).
3. **F-3 (conservación).** Los demás campos del clon se calculan exactamente como en la base: rol, pieza, bloque, vista, rotación, `MirroredX` (invertido
   con el espejo), `MirroredY`, puntos (`x → InsertionX ± x`, `y → InsertionY + y`) y parámetros dinámicos. El cuerpo de `Flatten` no cambia: orden de
   iteración, sueltos añadidos por referencia y contadores de `LateralHeaderLayout`.
4. **F-4 (sin cambio hoy).** Ningún constructor actual pone en un grupo una anotación ni una cota, ni da a una instancia de grupo un valor distinto del
   de defecto en los cuatro campos (Discovery DC-01, DC-05), así que todas las salidas actuales de `Flatten`, del dibujo, de la vista previa y del BOM
   quedan iguales.
5. **F-5.** `Flatten` no modifica la fuente: tras aplanar, las instancias del grupo conservan sus valores, y dos llamadas a `Flatten` dan resultados
   iguales.

## 3. Persistencia, fallo y compatibilidad

Sin persistencia, esquema, DTO ni fallback legacy (DC-03). La semántica de fallo no cambia: no hay validación nueva ni excepción nueva. Compatibilidad:
total, porque F-4 deja iguales todas las salidas actuales.

## 4. No-objetivos

Meter decorados en grupos; cambiar el agrupador (prototipo, firma o regla de sueltos), los `RigidClone` de cama, `CloneAt` de Push Back, los clones
`Translated`/`Translate` del Selectivo, `PushBackMirror`, el cuerpo de `Flatten`, el dibujante, la UI o la vista previa; corregir la rotación de las
piezas rotadas en colocaciones espejadas, que cambiaría los hashes de `Flatten` y la vista previa (M-03); y el `MirroredX`, la rotación, la orientación
del texto o el lado del texto de los decorados espejados.

Hallazgos laterales que se registran en `docs/ideas-futuras.md` al cerrar, sin arreglarlos: los clones hermanos que tampoco copian los campos de cota
(DC-05); la rotación conservada de las piezas rotadas en colocaciones espejadas (EXP-01), con su efecto probable en la vista previa de Push Back:
`EditorPreviewParts.Structure` dibuja las diagonales con `DrawSpan` (`EditorPreviewParts.cs:204-206`, `:290`), que aplica espejo y rotación
(`EditorPreviewSurface.cs:170-178`), así que la diagonal de una cabecera espejada apuntaría al revés (inferido del código, sin ejecutar; se comunica al
Owner); y la orientación y el lado del texto de los decorados dentro de una referencia espejada.

## 5. Puntos de extensión

Ninguno nuevo ni cambiado.

## 6. Obligaciones invariante → prueba (RED esperado)

Pruebas Core en un archivo propio, sobre `HeaderRunPlan` construido a mano. Todos los valores esperados son literales calculados a mano, nunca
obtenidos llamando al código bajo prueba, salvo N-9, que compara dos llamadas a `Flatten` entre sí. El RED se observa en el commit RED de la rama: las
pruebas nuevas sobre el código de producto de la base `bb0d5522`.

| Id | Invariante | Prueba | RED esperado en el commit RED |
|---|---|---|---|
| N-1 | F-1 para una anotación | `Text` y `TextHeight` del clon iguales a los de la fuente, normal y espejada | falla: `null` y `0` |
| N-2 | F-1 y F-2 para una cota horizontal | `Text`, `DimensionOffset`, `DimensionStyleName` y `TextHeight` iguales, normal y espejada | falla: `null`, `0`, `null` y `0` |
| N-3 | F-2 para una cota vertical | normal: `DimensionOffset` igual; espejada: con el signo opuesto; `Text`, `DimensionStyleName` y `TextHeight` iguales. Es el oráculo principal del signo | falla: `0` y `null` |
| N-4 | F-2 coherente con el espejo de Push Back | para una cota vertical y una horizontal en una colocación espejada con `InsertionY = 0`, los puntos y los cuatro campos del clon son iguales a los de `PushBackMirror.Instance(s, p.InsertionX)`, que refleja `x → InsertionX − x` igual que la colocación. Es una prueba de coherencia, no el oráculo principal; es independiente porque §1 prohíbe derivar los campos de `PushBackMirror` | falla: los campos del clon quedan por defecto |
| N-5 | F-3 | una pieza de bloque, normal y espejada con `InsertionY ≠ 0`: rol, pieza, bloque, vista, rotación, espejos, puntos y parámetros con los valores literales de la base, y los cuatro campos por defecto; los sueltos son las mismas referencias | pasa en la base y después (conservación) |
| N-6 | F-4 | las suites Core existentes (incluidas las firmas y los hashes de `Flatten`) y la suite de UI del CI (job `UI Tests`) siguen verdes sin cambios | pasa en la base y después |
| N-7 | F-2, empate | cota en una colocación espejada con `\|dx\| = \|dy\| ≠ 0` (valores exactos) y cota de longitud cero, ambas con desplazamiento ≠ 0: el desplazamiento se conserva | falla: `0` |
| N-8 | F-1 y F-2, rol | una anotación y una pieza de bloque con puntos verticales y los cuatro campos distintos del de defecto, en una colocación espejada: los cuatro se copian tal cual y el desplazamiento no cambia de signo | falla: los campos quedan por defecto |
| N-9 | F-5 | tras aplanar un plan con colocaciones normal y espejada de una cota vertical, la fuente conserva sus cuatro campos y dos llamadas a `Flatten` dan los mismos valores | pasa en la base y después (guarda contra una negación en el sitio) |
| N-10 | F-3 para los decorados | una anotación y una cota con `RotationRadians ≠ 0`, en una colocación espejada: `MirroredX` invertido y `RotationRadians` igual al de la fuente | pasa en la base y después (conservación) |

N-1..N-4, N-7 y N-8 deben fallar en el commit RED por la causa indicada antes de la corrección; los casos de conservación (N-5, N-9 y N-10) pasan en él.

## 7. Matriz OV

```text
OV-id | Escenario | Disparador de aplicabilidad | Sistemas | Datos nuevo/legacy |
Resultado esperado | Unidad(es) asignada(s) | Hito intermedio adicional: si/no
```

| OV-id | Escenario | Disparador de aplicabilidad | Sistemas | Datos nuevo/legacy | Resultado esperado | Unidad(es) | Hito intermedio |
|---|---|---|---|---|---|---|---|
| OV-I66-01 | El Owner revisa el RED → GREEN de I-66 sobre el Candidato | reserva del Owner: I-66 no se integra sin su Owner Validation (decisiones §1). No hay comportamiento dibujado que cambie (F-4), así que el checklist de §5 de `docs/guias/validacion-manual-autocad.md` no aplica: es una decisión de aplicabilidad, no una exención | ninguno dibujado; `Flatten` en Application para Dinámico y Push Back, y sus vistas previas de UI (la planta del Selectivo usa grupos de `HeaderRunPlan`, pero no `Flatten`) | n/a (sin persistencia, DC-03) | N-1..N-4, N-7 y N-8 en RED sobre el commit RED de la rama (pruebas nuevas sobre el código de producto de `bb0d5522`), con su SHA y conteos registrados en `I-66-evidence.md`, y en GREEN sobre `FINAL_CANDIDATE_SHA`; N-5, N-6, N-9 y N-10 en verde; la CI de ese SHA exacto en verde, con su run id; el Owner declara aprobado o rechazado | I-66 | no |

Se registran todos los campos de la guía §7 que aplican, entre ellos el SHA del commit RED, el `FINAL_CANDIDATE_SHA`, el run id de su CI, los conteos
de las pruebas focales y de Core Full, el validador y su declaración explícita. Solo no aplican los propios de AutoCAD (ruta y SHA-256 del DLL,
versión de AutoCAD, DWG, bloques reales y checklist de §5), porque no hay validación en AutoCAD.

## 8. Plan de gates

| Gate | Resultado verificable | Cierre |
|---|---|---|
| G1 | `Flatten` conserva los cuatro campos según F-1..F-5, probado por N-1..N-10 | RED → GREEN focal, Core Full sobre el SHA de cierre, CI de ese SHA en verde y revisión del Coordinator (INITIATIVE_LIFECYCLE §7) |

Después de G1: READY, Candidato y Owner Validation según WORKFLOW; I-66 no se integra sin su Owner Validation.
