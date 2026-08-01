# ADR-0027: La línea Cantilever — intervalos, distribución de paneles y arriostramiento

- **Estado:** propuesto
- **Fecha:** 2026-07-29 (redacción)
- **Decisores:** Mario Pérez, Owner del repositorio (decisiones de producto emitidas al abrir I-37D);
  Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-37D `feature/cantilever-mvp-final`
- **No reemplaza a ninguna ADR.** Extiende [ADR-0024](0024-fundacion-cantilever-base-columna.md),
  [ADR-0025](0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md) y
  [ADR-0026](0026-estacion-cantilever-niveles-altura-y-bom.md). **Ninguna de las tres se reabre**: la línea
  **compone** estaciones ya resueltas.

## Contexto

I-37C dejó una estación que se puede resolver y cotizar, y que deliberadamente **no** sabe dónde está: no
tiene posición longitudinal, ni índice dentro de nada, ni vecinos. Eso fue correcto — meterlo entonces habría
significado quitarlo ahora — y deja cinco preguntas que sólo aparecen cuando hay más de una estación.

### 1. Qué es una línea, y de quién son los separadores

Dos estaciones adyacentes comparten el espacio entre ellas. Si el separador y sus tensores pertenecen a la
estación izquierda, la última estación de la línea es distinta de las demás sin ninguna razón física; si
pertenecen a la derecha, lo es la primera. Y si pertenecen a las dos, se cuentan dos veces.

### 2. Cuántos paneles arriostrados lleva una columna

El dato existe: la tabla de producto del Cantilever da la cantidad de paneles para doce alturas. Lo que no
existe es la regla, y una tabla codificada como doce `if` deja de servir a la altura trece.

### 3. Dónde caen los paneles a lo largo de la columna

Los paneles no se reparten uniformemente. Se agrupan **desde abajo en bloques de dos**, con un espacio vacío
entre bloques, y el bloque incompleto queda **arriba**. Repartir el remanente uniformemente, o meter el
espacio central en los extremos, produce un dibujo que no es el producto.

### 4. Qué mide el corte de un separador

La respuesta cómoda es «la separación entre centros de columna menos algo». Es falsa: el separador se
atornilla a **placas** que están en las caras interiores de las columnas, así que su corte lo determinan los
**agujeros de esas placas** y no la retícula de la línea. Restar un ancho de columna cableado es exactamente
el error que ADR-0024 evitó al derivar toda cota exterior de `Bounds`.

### 5. Qué es un tensor

Hay dos productos distintos con la misma función: un perfil estructural atornillado por sus extremos, y una
varilla **cold rolled** que necesita un **adaptador** en cada punta porque no se puede taladrar y atornillar
como un perfil. Modelar sólo el primero y «aproximar» el segundo daría un BOM que no se puede comprar.

## Decisión

### D1 — La línea es una secuencia de estaciones con una topología compartida

`CantileverLineDesign` lleva la cantidad de estaciones, la separación entre centros de columna, **una sola**
topología de estación, **un solo** template de brazo por omisión, los overrides de celda y el arriostramiento.

Una línea tiene **al menos dos** estaciones — con una no hay intervalo, y sin intervalo no hay separadores ni
arriostramiento, que es la mitad de lo que esta iniciativa entrega. Las posiciones son

```
StationOriginX[i] = i × ColumnCentreSpacing
```

con **X** longitudinal, **Y** la profundidad de brazos y bases, y **Z** vertical — el mismo marco que
ADR-0026 fijó para la estación, extendido con la única dirección que a ella le faltaba.

**Todas las estaciones comparten** el modo de cara, el lado sencillo, la columna y la base, la cantidad de
niveles, el claro solicitado, la retícula y **la altura final**. Lo único que puede diferir entre ellas es el
brazo de una celda. Una línea cuyas columnas tuvieran alturas distintas no sería una línea: sería un conjunto
de estaciones que comparten un dibujo.

`ColumnCentreSpacing` tiene default **48 in**, debe ser finita y positiva, y es de **centro a centro** de
columna. No es la longitud de un separador (ver D5).

### D2 — La altura común se resuelve en una secuencia, y la más restrictiva gobierna

Los overrides de brazo pueden hacer que una estación necesite más columna que sus vecinas, y el
arriostramiento tiene su propio mínimo. La altura común se resuelve así:

1. construir el diseño efectivo de cada estación;
2. resolver su altura mínima con el resolver de I-37C, **sin cambiarlo**;
3. tomar el máximo de todas;
4. considerar el mínimo que exige el arriostramiento;
5. elegir la altura común automática, o validar la manual;
6. **volver a resolver todas** las estaciones con esa altura común;
7. **verificar** que sus niveles e índices no cambiaron.

```
CommonMinimumHeight = max( max(StationMinimumHeight[i]), BracingMinimumHeight )
```

Una altura manual menor **bloquea**. No se recortan estaciones, ni paneles, ni niveles. Es la misma regla que
ADR-0026 D6 fijó para una estación sola, aplicada al conjunto — y el paso 7 es el mismo pase final
verificado: si la altura común mueve un índice, la línea **falla cerrado** en vez de construir algo que su
propio layout no predijo.

### D3 — Los intervalos son del PAR, no de una estación

```
IntervalCount = StationCount − 1
```

`CantileverIntervalAssembly` conoce su índice y las dos estaciones que lo delimitan, y **es dueño** de sus
separadores, sus paneles y sus tensores. Un separador no pertenece a la estación izquierda ni a la derecha:
pertenece al intervalo. Eso es lo que hace que la primera y la última estación no sean casos especiales, y lo
que impide contar un separador dos veces al recorrer intervalos vecinos.

### D4 — La distribución vertical de paneles es una REGLA, no una tabla

```
StandardBracedPanelCount = max(1, ceil((ColumnHeight − 72 in) / 60 in))
```

Reproduce **las doce filas** de la tabla de producto aprobada, y sigue respondiendo para la altura trece. Eso
es la diferencia entre una regla y doce `if`.

| Altura | Paneles | | Altura | Paneles |
|---|---|---|---|---|
| 96 | 1 | | 216 | 3 |
| 120 | 1 | | 240 | 3 |
| 132 | 1 | | 252 | 3 |
| 144 | 2 | | 264 | 4 |
| 168 | 2 | | 288 | 4 |
| 192 | 2 | | 336 | 5 |

`PanelCountMode` admite `Automatic` y `Manual`; la cantidad manual debe ser positiva.

`BracedPanelHeight` y `CentralEmptySpaceHeight` valen **40 in** por omisión, y las dos deben ser finitas y
positivas.

Los paneles **se agrupan desde abajo en bloques de máximo dos**, con un espacio vacío central entre bloques,
y el bloque incompleto queda **arriba**:

```
CentralEmptySpaceCount = floor((BracedPanelCount − 1) / 2)
```

| Paneles | Espacios centrales | Secuencia desde abajo |
|---|---|---|
| 1 | 0 | P |
| 2 | 0 | P P |
| 3 | 1 | P P · V · P |
| 4 | 1 | P P · V · P P |
| 5 | 2 | P P · V · P P · V · P |
| 6 | 2 | P P · V · P P · V · P P |

**Sólo el remanente se reparte a los extremos**, y por igual:

```
CoreHeight          = BracedPanelCount × BracedPanelHeight
                    + CentralEmptySpaceCount × CentralEmptySpaceHeight

BottomExternalSpace = TopExternalSpace = (CommonColumnHeight − CoreHeight) / 2
```

Si el remanente es **negativo**: en altura automática el `CoreHeight` gobierna el mínimo; en manual, se
**bloquea**. No se comprimen paneles, no se reducen espacios centrales y **no se cambia la cantidad en
silencio** — las tres serían maneras de entregar un dibujo que no es el que se pidió.

**Ejemplo normativo, columna de 264 in:** 4 paneles, 1 espacio central, 32 in externos.

```
32 externo · 40 arriostrado · 40 arriostrado · 40 VACÍO · 40 arriostrado · 40 arriostrado · 32 externo
```

### D5 — Un separador por frontera interna, y su corte lo dictan las placas

Cada frontera interna de esa secuencia lleva un separador:

```
SeparatorCountPerInterval = BracedPanelCount + CentralEmptySpaceCount + 1
```

Para el caso de 264 in son **seis**. Dos paneles arriostrados adyacentes **comparten** el separador que los
separa: se cuenta **una vez**.

La placa de columna del separador es una pieza propia — 3 in × 3 in × 3/8 in, con **un** agujero centrado de
9/16 in — colocada a la elevación del separador y centrada en la cara de conexión longitudinal de la columna
**hacia ese intervalo**. Qué superficie es esa sale de la **orientación registrada** de la columna y de su
geometría resuelta: no se lee `d`, `bf`, `tw` ni `tf`, y no se supone una cara por el nombre de la sección.
Una estación extrema lleva **una** placa por elevación; una interior, **dos**.

Y el corte del separador se deriva de **los agujeros de esas dos placas**, nunca de la separación entre
columnas:

```
ConnectionPunchDistance = distancia entre los datums de los agujeros de las dos placas
SeparatorCutLength      = ConnectionPunchDistance + 2 × 1.25 in
```

Será normalmente **menor** que `ColumnCentreSpacing`, porque las placas están en las caras interiores. Restar
un ancho de columna cableado está **prohibido**.

Cada extremo lleva **dos** agujeros de 9/16 in: el de columna a 1.25 in del borde, y el de tensor a 4 in de
aquél. Cuatro en total, y el patrón derecho es el **espejo exacto** del izquierdo:

```
izq. columna = 1.25      der. columna = L − 1.25
izq. tensor  = 5.25      der. tensor  = L − 5.25
```

La coincidencia con la placa se comprueba por **datum lógico**, nunca comparando centros 3D de superficies
distintas — la regla que ADR-0024 D6 fijó y que ADR-0026 volvió a aplicar.

### D6 — Los dos tensores de un panel forman una X en el MISMO plano

Cada panel arriostrado conoce su separador inferior y su superior, y sus dos tensores van cruzados de agujero
de tensor a agujero de tensor: `BraceA` de abajo-izquierda a arriba-derecha, `BraceB` de abajo-derecha a
arriba-izquierda.

Los dos **ocupan el mismo plano**, **pueden solaparse visualmente**, **no tienen unión central** y **no
llevan offset** para esquivarse. El MVP **no calcula** la interferencia entre ambos, y eso está declarado, no
disimulado: separarlos con un desplazamiento inventado sería dibujar algo que nadie fabrica.

### D7 — Dos clases de tensor, y el cold rolled necesita adaptadores

`CantileverBraceBodyKind` tiene `StructuralSection` y `ColdRolledRound`, y el **default es cold rolled de
3/4 in**.

Un **tensor de perfil estructural** admite familias `Channel` y `Angle`, con ids exactos registrados por la
política. Lleva un agujero de 9/16 in a 1.25 in de cada extremo, y su corte es

```
BraceCutLength = distancia entre los datums de tensor de los dos separadores + 2 × 1.25 in
```

Un **tensor cold rolled** tiene diámetro editable, finito y positivo, y su eje conecta los agujeros
correspondientes de sus **adaptadores**; su longitud nominal es la distancia entre los datums de paso del
cold rolled en ambos. **No se inventan** tolerancias, roscas, tuercas ni extensiones de fabricación.

El **adaptador** es un ángulo de 2 in × 2 in, cortado a 2 in, de 3/16 in de espesor, con un agujero de
9/16 in **centrado** en cada una de sus dos caras cuadradas —una mira al separador, la otra al cold
rolled— y **dos cartabones**, uno en cada extremo longitudinal. Puede modelarse con geometría propia: **no es
obligatorio** añadirlo al catálogo estructural, porque no es una sección de catálogo sino una pieza fabricada.

Por cada tensor cold rolled: **2 adaptadores y 4 cartabones calibre 10**. El **calibre 10 es la autoridad de
producto**: se conserva como `GaugeNumber = 10` en la identidad y la descripción, y **no** se convierte a un
decimal en silencio. La vista 2D no necesita esa conversión para dibujar su contorno.

#### D7-bis — El eje es el datum, no el dibujo · `OWNER_REVISED_CANTILEVER_BRACE_VISUAL_REPRESENTATION`

**Revisado por el dueño en la ronda 3 de I-37D.** El ADR sigue **propuesto**: esta revisión no lo acepta, lo
corrige.

De D7 se venía leyendo, con razón, que un cold rolled sin fila de catálogo no debía dibujarse con una
sección inventada; y de ahí se pasó a dibujarlo **como su eje**, una recta de dos puntos, y a dibujar su
adaptador como un **cuadrado** de 2 × 2. Lo primero era una inferencia sensata; lo segundo, una
simplificación que el dueño rechazó al verla.

La regla que sustituye a esa lectura:

> **El eje continúa siendo el datum geométrico del tensor, pero la geometría visible debe tener ancho
> físico.**

Y lo que eso quiere decir, exactamente:

- El **eje conserva todo lo que ya gobernaba**: la longitud nominal, los dos extremos, la conexión con los
  agujeros de tensor de los separadores. Nada de eso se recalcula desde el contorno.
- El **cuerpo cold rolled se dibuja como una banda centrada en el eje**, de ancho igual a su **diámetro**.
  No es una sección inventada: es el número que el diseño ya declaraba, puesto a los dos lados del eje que ya
  existía. Dos bordes paralelos, un cierre perpendicular en cada extremo, polígono cerrado.
- El **adaptador se dibuja como el ángulo que es**: contorno en L de seis puntos, con su talón, sus dos alas
  de 2 in y su espesor de 3/16 in, orientado según el extremo que ocupe. Sus **dos cartabones** se dibujan
  como triángulos, uno en cada extremo de su corte.
- La **orientación no se declara, se deriva** de hacia dónde queda el agujero de la varilla respecto del del
  separador. Los cuatro casos —abajo-izquierda, abajo-derecha, arriba-izquierda, arriba-derecha— salen
  exhaustivos por construcción.

**Lo que esta revisión NO autoriza.** Sigue sin haber preparación de bordes, destijeres, soldadura del talón,
roscas, tuercas ni tolerancias de armado: es **representación visual, no fabricación**. Y sigue sin cambiar
nada del producto — longitud nominal, diámetro, sección, número de adaptadores y de cartabones e identidad
comercial son los mismos, y los contornos **no se persisten**: se rederivan.

#### D7-ter — El adaptador FÍSICO manda sobre el tensor · ronda 4 de I-37D

**Revisado por el dueño en la ronda 4 de I-37D.** El ADR sigue **propuesto**: esta revisión, como la
anterior, no lo acepta — lo corrige.

D7-bis dejó el adaptador dibujado «como el ángulo que es», y la implementación lo hizo con un **contorno en
L de seis puntos construido a mano**: a escuadra, sin filete de raíz ni radios de punta. Y el agujero de la
varilla se situaba con esta regla:

```
RodHoleAxialOffset = CutLength / 2
```

cuya justificación escrita era «medio corte, porque las dos caras son perpendiculares». **Esa justificación
es justamente la que no se sostiene:** si las dos caras son perpendiculares, la separación entre dos agujeros
centrados cada uno en SU ala tiene componente en los dos ejes del ángulo, no en uno. De ahí salía un
`ΔY = 0` forzado — una pieza plana metida en el plano del panel — y un módulo de exactamente 1.0 in.

**Queda REVOCADA.** La regla que la sustituye:

> **El adaptador físico manda sobre el tensor.** El adaptador es un prisma real de la sección de catálogo
> `AISC-L-L2X2X3_16` cortado a 2 in, y sus dos agujeros están **centrados cada uno en su propia ala**, en el
> **plano medio real** de esa ala. El centro del agujero de varilla es el **datum físico** del extremo del
> tensor.

En coordenadas de talón, midiendo `a` a lo largo del ala apoyada y `b` a lo largo de la del tensor:

| agujero | a | b |
|---|---|---|
| separador | `L/2` | `t/2` |
| varilla | `t/2` | `L/2` |

De ahí sale todo lo demás sin decidir nada más:

- **Separación** `(L − t)/2` en **cada** eje. Para el `L2×2×3/16`: `0.90625 in` por eje, módulo
  `1.281631 in`, medida `Δ = (0.820358, −0.906250, 0.385099)`. **ΔY ya no es cero.**
- **El agujero del separador no está sobre la cara** del separador, sino medio espesor más afuera, en el
  plano medio del ala apoyada. El **troquel** sigue marcando la cara, porque tiene que coincidir con el del
  separador: son el mismo agujero físico, y el datum de un troquel no lleva la Y.
- **La longitud nominal del tensor CRECE `0.1875 in`**, que es exactamente el espesor del ángulo. No es una
  coincidencia: la aproximación revocada medía hasta la **cara** del ala y la física mide hasta su **plano
  medio**, medio espesor por extremo. Sobre el caso normativo: `92.131526 in` → `92.319026 in`.
- **El BOM cambia con ella, y es legítimo.** Las cantidades **no** cambian —4 varillas, 8 adaptadores, 16
  cartabones— sólo la longitud que se ordena. No se conserva el BOM anterior por compatibilidad artificial.

**El marco se deriva, no se declara**, de tres cosas: la normal de la cara del separador da el eje del ala
que recibe la varilla; el eje de la diagonal da el del ala apoyada, **en contra** del otro extremo, para que
el cuerpo del adaptador no invada el vano por el que pasa la varilla; y su producto da el eje del corte.

**El eje del corte queda PERPENDICULAR a la diagonal y dentro del plano del panel.** No es una elección de
dibujo: es la única orientación en la que el agujero del ala del tensor tiene por eje la **propia varilla**,
que es como se sujeta una varilla roscada. Con el corte a lo largo de la diagonal la varilla no podría
enhebrarse, y además el cuerpo del adaptador se comería media pulgada de vano.

**Consecuencia visual, declarada y no disimulada.** Ninguna de las tres vistas de la línea mira por el eje del
corte, así que **el adaptador no se lee como una L en ninguna de ellas**: la frontal ve el ala apoyada de
frente y la del tensor de canto. El dueño decidió que **no se deforma ninguna vista** para que parezca una L
—frontal, lateral y planta siguen siendo proyecciones físicas— y que el configurador de tensor gana una vista
propia, **«Sección del adaptador»**, que mira a lo largo del eje de corte y muestra la sección real. Esa
vista consume la **misma** `StructuralSectionGeometry` que el prisma: ni otra L a mano, ni otra fórmula de
radios.

**Implementada** en la misma ronda. Su cámara sale del **marco de la pieza** —mira por `AlongCut` con
`AlongRodLeg` como vertical— y por eso `CantileverViewKind.AdapterSection` **no tiene cámara fija**: pedirla
por la puerta de las vistas de línea lanza, con un caso explícito y su motivo. Los dos agujeros se ven **de
canto** en esa proyección —sus ejes son perpendiculares a la dirección de vista— y se dibujan como su
**traza**, no como círculos. Las tres vistas de la línea **no se tocaron**, y una prueba lo comprueba sobre
sus cámaras.

**Lo que esta revisión NO autoriza.** Lo mismo que D7-bis: sigue sin haber preparación de bordes, destijeres,
soldadura del talón, roscas, tuercas ni tolerancias de armado. Y sigue sin persistirse ningún contorno.

### D9 — La secuencia vertical de paneles: regla o lista declarada · ronda 4 de I-37D

**Decisión del dueño en la ronda 4.** Hasta aquí la secuencia vertical de un intervalo la decidía entera la
regla de D4. Sigue siendo la de por omisión; deja de ser la única.

> Los **dos modos producen la misma cosa** —una lista de tramos contigua, de abajo arriba— y el resolver
> posterior **sólo conoce esa lista**.

```
CantileverPanelLayoutMode    { Automatic, Advanced }
CantileverPanelBracingMode   { None, CrossBraced }
CantileverPanelSegmentDesign { StartElevation, EndElevation, BracingMode }
```

- **Un vacío es un TRAMO** con los tensores apagados, nunca la ausencia de uno: un hueco implícito no se
  distingue de un tramo que alguien olvidó escribir.
- **La lista cubre el NÚCLEO**, del primer tramo al último, no la columna entera. Lo que queda por debajo y
  por encima son los espacios externos, que **no** llevan separador. Si fueran tramos,
  `Distinct(inicios + fines)` pondría un separador en el piso y otro en la punta, y eso **no** es el
  producto: D5 coloca `paneles + huecos + 1`.
- **Derivaciones:** `SeparatorElevations = Distinct(inicios + fines)` —que es toda la regla de no duplicar
  separadores en fronteras compartidas—, `BracedPanels = tramos CrossBraced` y `BraceCount = 2 × BracedPanels`.
- **El tramo no lleva su altura.** Es derivada de sus dos cotas, y persistirla sería una tercera autoridad
  sobre el mismo hecho.
- **Automático → avanzado materializa** la lista que se está viendo; **avanzado → automático avisa** antes,
  porque la lista manual deja de mandar, y la **conserva** como dato dormido.

**El modo automático no cambia el producto.** Los pines de línea, BOM y las seis vistas siguen exactamente
donde estaban: la lista efectiva es otra forma de escribir la misma secuencia.

### D8 — El BOM de la línea, y de quién es cada pieza

Los componentes de una línea son **cuatro**: columna–base, brazo, separador y tensor.

El componente **columna–base** incluye la columna, su placa inferior, una o dos bases con sus placas y
cartabones, y **las placas de conexión de separador que correspondan a esa estación**. Las placas **no** son
componentes propios, **no** pertenecen al separador y **no** aparecen como categoría aparte. Su consecuencia
es real y se acepta: una estación extrema y una interior tienen recetas **distintas**, así que agrupan por
separado — normalmente dos grupos, las dos extremas y las interiores.

Los **brazos** se agregan de todas las estaciones con la identidad que ADR-0026 D8 aprobó: por receta física,
sin lado ni posición.

Cada **separador** físico es un componente con su perfil; sus cuatro agujeros son parte de su identidad y no
líneas del BOM. Cada **tensor** es un componente, separado por clase, sección o diámetro, corte, patrón de
agujeros y —en el cold rolled— sus adaptadores y cartabones.

**Los agujeros nunca son líneas.** Las placas de columna **nunca** son componentes independientes.

## Alternativas consideradas

**El separador pertenece a una de las dos estaciones.** Descartada: convierte la primera o la última estación
en un caso especial sin razón física, y al recorrer intervalos vecinos se cuenta dos veces.

**Codificar la tabla de doce alturas.** Descartada: deja de responder en la altura trece, y la regla derivada
reproduce las doce exactamente.

**Repartir el remanente uniformemente entre todos los huecos.** Descartada: el producto agrupa los paneles de
dos en dos con un vacío central de altura fija; repartir uniformemente da otro dibujo.

**Corte del separador = separación entre columnas − ancho de columna.** Descartada por la misma razón que
ADR-0024 D5: sería una cota exterior cableada en vez de derivada de la geometría resuelta, y dejaría de
valer al cambiar de sección.

**Separar los dos tensores de la X con un pequeño offset.** Descartada: nadie fabrica eso. El MVP declara que
no calcula la interferencia en lugar de fingir que la resolvió.

**Tratar el cold rolled como un perfil taladrado.** Descartada: una varilla no se atornilla así. Sin
adaptadores el BOM no se puede comprar.

**Añadir el adaptador al catálogo estructural.** Descartada: el catálogo neutral de I-36 es de **secciones**
de norma (ADR-0020), y el adaptador es una pieza fabricada. Meterlo ahí obligaría a inventar una procedencia.

## Consecuencias

**Positivas.** La línea es el primer resultado de RackCad que un cliente reconocería como su rack, y sale de
componer estaciones ya probadas: la geometría de columna, base, brazo y estación no se vuelve a calcular. Que
los separadores sean del **intervalo** hace que no existan estaciones especiales ni conteos dobles. Que la
distribución de paneles sea una **regla** hace que la altura trece funcione sin editar nada. Y que el corte
del separador se derive de los **agujeros de las placas** lo hace correcto al cambiar de sección de columna,
que es exactamente donde una resta cableada fallaría.

**Negativas, y asumidas.** La altura común obliga a resolver las estaciones **dos veces** —una para conocer
el máximo y otra con la altura definitiva—, con su coste de cálculo; se acepta porque es lo que permite
**verificar** que los índices no se movieron en vez de suponerlo. Las placas de separador hacen que las
columnas extremas e interiores agrupen distinto en el BOM, lo que sorprende al leerlo hasta que se entiende
por qué. Y el MVP **no** calcula la interferencia en el cruce de tensores: los dos ocupan el mismo plano y se
solapan visualmente, lo que es un hecho declarado y no un descuido.

**Lo que este ADR NO decide.** No decide la persistencia, los registros, las vistas, el editor ni la
materialización en AutoCAD — eso es [ADR-0028](0028-cantilever-persistencia-vistas-editor-y-dibujo.md). Y no
decide cálculo resistente, cargas, capacidad, peso, costo, optimización, soldaduras, tornillería, anclas,
roscas, tolerancias, preparación de extremos, CNC ni shop drawings: siguen fuera de alcance incluso al cerrar
I-37.

## Referencias

- [ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md) — sección ≠ miembro ≠ pieza comercial.
- [ADR-0024](0024-fundacion-cantilever-base-columna.md) — el datum de troquel y la geometría desde `Bounds`.
- [ADR-0025](0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md) — el brazo y su conexión.
- [ADR-0026](0026-estacion-cantilever-niveles-altura-y-bom.md) — la estación que esta línea compone.
- [Contrato de I-37D](../initiatives/I-37D-cantilever-mvp-final.md).
- [Decisiones del Owner para I-37](../automation/decisions/I-37.md).
