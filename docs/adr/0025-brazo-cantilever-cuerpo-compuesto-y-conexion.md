# ADR-0025: El brazo Cantilever — cuerpo simple o compuesto, y su conexión a la columna

- **Estado:** **aceptado**
- **Fecha:** 2026-07-29 (redacción y **aceptación**)
- **Decisores:** **Mario Pérez, Owner del repositorio** (acepta); Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-37B `architecture/cantilever-brazo`
- **No reemplaza a ninguna ADR.** Extiende [ADR-0024](0024-fundacion-cantilever-base-columna.md) de forma
  **aditiva**: ningún contrato que I-37A congeló se reabre.

## Aceptación del Owner (2026-07-29)

**Decisor:** Mario Pérez, Owner del repositorio. **Gate:** aprobado.
**Veredicto normativo registrado:** `OWNER_APPROVED_ADR_0025_WITH_CURRENT_DATUM`.
**SHA técnico aprobado:** `00d8126eb687a46bafc156480ea6f080f295a771`.

> **Aceptado sobre el código, no sobre el dibujo**, igual que ADR-0024: I-37B no dibuja nada, así que no
> hubo `requires_autocad` ni validación visual. El gate se resolvió con CI verde sobre el SHA exacto,
> 2 355 pruebas de `RackCad.Tests`, 544 de `RackCad.UI.Tests` y las regresiones comprobadas **en rojo**.

Aceptado expresamente:

| # | Lo aceptado |
|---|---|
| 1 | El **cuerpo del brazo es una colección de miembros**, no un miembro con un segundo opcional |
| 2 | Los **tres arreglos**: perfil sencillo, canal doble encontrado y canal doble espalda con espalda |
| 3 | Los **canales dobles son de la misma sección y quedan en contacto**, **sin campo de separación** |
| 4 | Toda **colocación derivada de `StructuralSectionGeometry.Bounds`**, nunca de `d`, `bf` ni la designación |
| 5 | La **longitud capturada es el corte del perfil** y no incluye espesores de placa ni extensión de tope |
| 6 | **`NominalCutLength == GeometricLength == Body.CutLength`** |
| 7 | **`SlopeRisePer12` como única autoridad** de pendiente; los grados son derivados |
| 8 | El **extremo libre asciende en AMBOS lados** — simetría especular, no rotación de 180° |
| 9 | **Selección contigua** de troqueles regulares **ya existentes** de la columna |
| 10 | El **pitch se OBSERVA de la columna** y nunca se cablea en el brazo |
| 11 | La **placa de conexión crece hacia ARRIBA** al aumentar la cantidad de troqueles |
| 12 | **Bloqueo** cuando el cuerpo no cabe en la placa: no se estira sin más agujeros |
| 13 | **Tapa y tope son modos de una misma placa**, no dos piezas |
| 14 | La **placa final es perpendicular al eje inclinado**, con su alto en la dirección de peralte |
| 15 | **Elegibilidad por `StructuralSectionId` + `Arrangement`**, inyectable y sin ids de producción |
| 16 | **Validación exhaustiva** de modos y enums: un valor no declarado se rechaza, nunca se materializa |
| 17 | **Rango de índices sin overflow**: la comprobación es una resta, no una suma |
| 18 | **Rechazo diagnóstico** de una pendiente cuyo marco colapsa; nunca una excepción |
| 19 | El **datum actual**: el plano de corte tiene su **origen sobre la cara exterior** de la placa de conexión |
| 20 | **Intrusión y holgura** como **aproximación visual declarada**, con las dos magnitudes **medidas por separado** |
| 21 | La **coincidencia se compara por `CantileverPunchDatum`**, nunca por centros 3D |
| 22 | El **PTR no se trata como alias de HSS**; queda como candidato de catálogo futuro |

**El datum se conserva expresamente tal como está.** La cara exterior de la placa de conexión es el
**origen del plano de corte** y el eje centroidal del cuerpo arranca en ese datum. Con pendiente y corte a
escuadra, una zona del perfil **penetra visualmente** la placa y la opuesta deja **holgura**; ambas
magnitudes se reportan **por separado**; **no** se afirma que la cara quede a ras; y el **corte inclinado**
junto con cualquier **preparación de extremo** permanecen **fuera de alcance**. La aceptación es de la
aproximación **declarada**, no de una geometría exacta.

**La aceptación NO autoriza**, y ninguna fase posterior puede darlas por concedidas: estación completa;
lista de niveles; doble cara como sistema; separadores; arriostres; línea Cantilever; BOM; peso;
persistencia de proyecto; `RackSystemKind`; registros globales; editor; preview; vistas; AutoCAD; bloques;
preparación de extremos; fabricación; cálculo resistente; ni **cambios funcionales en I-36 o I-37A**. Cada
una es una decisión de su propia iniciativa.

## Contexto

I-37A fundó el primer miembro: base y columna, con `PrismaticSectionInstance` como única autoridad de
colocación, un patrón de agujeros con una sola autoridad y toda cota exterior derivada de
`StructuralSectionGeometry.Bounds`. El brazo es la segunda pieza, y trae cuatro preguntas que la base no
tenía.

### 1. Una pieza que puede ser dos perfiles

Un brazo comercial puede ser un perfil suelto o **dos canales apareados**. Si el modelo asume «un brazo, un
miembro», la variante de dos canales no cabe sin romper el contrato; si se le da una subclase por arreglo,
cada consumidor gana un `switch` de tipo — el modo de fallo que ADR-0024 D2 evita.

### 2. Qué mide la longitud que el usuario captura

Un brazo lleva una placa de conexión en la raíz y puede llevar una tapa en la punta. Si la longitud
capturada incluyera esos espesores, cambiar el espesor de una placa cambiaría el corte del perfil, y el
número que el usuario escribe dejaría de ser el que le pide al proveedor.

### 3. De dónde salen los agujeros del brazo

La columna ya resolvió su retícula regular en I-37A. Si el brazo calcula la suya, hay dos algoritmos para
los mismos tornillos — el defecto PB-004, otra vez, en otro sitio.

### 4. Qué pasa cuando el perfil es más alto que su patrón de agujeros

Una placa de conexión con dos agujeros no puede sujetar un perfil muy aperaltado. Ensanchar la placa en
silencio produce una pieza que parece resuelta y no lo está.

## Decisión

### D1 — El cuerpo es una COLECCIÓN de miembros, con el arreglo como enum

`CantileverArmBodyPlan.Members` es una lista de `CantileverStructuralMemberPlan`. El arreglo
—`Single`, `DoubleChannelFacing`, `DoubleChannelBackToBack`— determina **cuántos** hay:

| Arreglo | Etiqueta de producto | Miembros |
|---|---|---|
| `Single` | perfil sencillo | 1 |
| `DoubleChannelFacing` | canal doble encontrado | 2 |
| `DoubleChannelBackToBack` | canal doble espalda con espalda | 2 |

**No hay subclase por arreglo.** Un consumidor que quiera dibujar, contar o verificar recorre `Members` sin
preguntar de qué arreglo vienen; el arreglo solo importa a quien los coloca. Es la misma decisión que
ADR-0024 D2 tomó para el rol, aplicada a la cardinalidad.

**Un lado por pieza.** `CantileverArmSide` es `PositiveY` o `NegativeY`, y **no existe `Both`**: una pieza
física está de un lado. Una estación de doble cara tendrá **dos brazos resueltos**, uno por lado, y eso es
trabajo de la estación, no del brazo.

### D2 — Los canales dobles se tocan, sin separación, y el contacto se deriva de `Bounds`

Los arreglos dobles usan **dos instancias de la misma sección**, con el **mismo corte** y la **misma
pendiente**, y **en contacto**. La separación libre es **cero y no es un campo**:

```
ChannelClearGap = 0
```

No se persiste, no se ofrece y no se puede editar. Un campo con un único valor legal es una invitación a
cambiarlo.

**La orientación canónica del canal se leyó de I-36, no se supuso por el nombre.**
`ChannelSectionGeometryBuilder` la documenta y el contorno la implementa: **el dorso del alma mira a −X y
los patines abren hacia +X**, con `d` a lo largo de Y y `bf` a lo largo de X. Después el contorno se
traslada al centroide tabulado, así que en coordenadas de sección **`Bounds.MinX` es el dorso del alma** y
**`Bounds.MaxX` la punta de los patines**.

De ahí salen las dos colocaciones, sin leer una sola dimensión:

- **`DoubleChannelFacing`** — las aberturas se enfrentan y **las puntas de los patines** se tocan en el
  plano central. El primer miembro va sin espejo y se desplaza `−Bounds.MaxX`; el segundo va **espejado** y
  se desplaza `+Bounds.MaxX`.
- **`DoubleChannelBackToBack`** — **los dorsos de las almas** se tocan y las aberturas miran hacia afuera.
  El primer miembro va **espejado** y se desplaza `+Bounds.MinX`; el segundo va sin espejo y se desplaza
  `−Bounds.MinX`.

En los dos casos el conjunto queda simétrico respecto al plano central, con contacto exacto y sin traslape,
y eso se comprueba numéricamente en vez de afirmarse.

**Los dobles solo admiten familia `Channel`.** No es purismo: las colocaciones de arriba están escritas
contra la anatomía de un canal —dorso y patines— y no significan nada en una W o un HSS.

### D3 — La longitud capturada es el CORTE del perfil

```
NominalCutLength = GeometricLength = Body.CutLength
```

y **no** incluye el espesor de la placa de conexión, el de la tapa ni la extensión del tope. El **origen
del plano de corte** del cuerpo se coloca sobre la **cara exterior** de la placa de conexión.

**Y eso no es lo mismo que quedar a ras.** El perfil va cortado **a escuadra** y la placa de conexión es un
plano **vertical**, así que en cuanto hay pendiente la cara de arranque no coincide con el plano de la placa:
la parte de la sección por encima de su propio origen **penetra** la placa, y la de por debajo deja
**holgura**. Las dos magnitudes son distintas —una sección no tiene por qué ser simétrica respecto a su
origen; un ángulo no lo es— y el resolver **informa las dos**, derivadas de los dos extremos de la
envolvente, sin suponer simetría.

Es una **aproximación visual declarada**. Resolver un **corte inclinado** o cualquier **preparación de
extremo** sigue fuera de alcance, en I-37B igual que en I-37A. Este ADR **no** afirma que la placa y el
cuerpo no se traslapen: afirma dónde está el datum.

Consecuencia deliberada: cambiar el espesor de una placa **mueve** el brazo, no lo **acorta**. El número
que el usuario captura sigue siendo el que le pide al proveedor. Y sigue vigente ADR-0024 D4:
`NominalCutLength` **no está liberada para fabricación**.

### D4 — La pendiente es `RisePer12` y es la única autoridad

`SlopeRisePer12` admite **cero y valores positivos**, y **los grados no se persisten**: se derivan.

```
angle = atan(SlopeRisePer12 / 12)
```

Guardar las dos magnitudes sería una segunda verdad que se desincroniza en la primera edición. Y el extremo
libre **sube en los dos lados**, que es lo que hace de la pendiente una contraflecha y no un vuelco:

```
PositiveY: axis = (0, +cos angle, +sin angle)
NegativeY: axis = (0, −cos angle, +sin angle)
```

La componente vertical es `+sin angle` en ambos: es simetría especular respecto al plano X-Z, **no** una
rotación de 180°, que invertiría la pendiente en el lado negativo y sería un defecto invisible de frente y
evidente de perfil.

### D5 — El brazo SELECCIONA troqueles existentes de la columna; no crea retícula

El brazo consume `CantileverColumnBaseAssembly.ColumnRegularPunches` y elige una **secuencia contigua hacia
arriba**:

```
selected[i] = elevaciones_regulares[LowerColumnPunchIndex + i]      i = 0 … VerticalPunchCount − 1
```

`LowerColumnPunchIndex` es **base cero**. `VerticalPunchCount` es entero, **mínimo 2**, sin máximo, con
default **2**.

Los datums seleccionados son **los objetos de la columna**, no copias recalculadas, y el **pitch se
observa** de ellos. Ninguna constante de espaciado vive en el resolver del brazo: hoy vale 4 in porque lo
gobierna la columna, y si la columna cambia, el brazo la sigue sin editarse. Una guarda de fuente lo
comprueba.

La coincidencia se demuestra con `CantileverPunchDatum.ApproxEquals` entre el datum de la columna y el de la
placa del brazo. **El brazo no duplica los troqueles de la columna** dentro de su subensamble: genera solo
los de su placa.

### D6 — Más troqueles alarga la placa HACIA ARRIBA, y un cuerpo que no cabe se rechaza

```
PlateBottomZ = FirstSelectedPunchZ − MountingPlateVerticalEndOffset
PlateTopZ    = LastSelectedPunchZ  + MountingPlateVerticalEndOffset
```

El cuerpo se coloca de modo que **su envolvente inferior en el plano de conexión coincida con
`PlateBottomZ`**. Por tanto añadir filas crece la placa **hacia `+Z`** y nunca hacia abajo: el borde
inferior está anclado al cuerpo.

Y si el perfil es demasiado aperaltado para las filas pedidas, es decir si

```
PlateTopZ < BodyEnvelopeTopAtConnection
```

la resolución **se bloquea** con un diagnóstico que pide **aumentar `VerticalPunchCount`**. La placa **no**
se estira en silencio: una placa más alta sin más agujeros es una pieza que parece resuelta y no lo está.
Esto codifica la decisión del Owner — más troqueles siempre extiende la placa hacia arriba.

`MountingPlateVerticalEndOffset` es **obligatorio y sin default aprobado**, igual que los dos de I-37A. No
se inventa.

### D7 — Tapa y tope son MODOS de la misma placa, y esa placa es perpendicular al brazo

`CantileverArmEndPlateMode` es `None`, `Cap` o `Stop`. `Cap` toma el ancho y la altura de la envolvente
combinada del cuerpo; `Stop` es la misma placa con `ExtraStopHeight > 0` **creciendo hacia arriba** desde el
mismo borde inferior. `ExtraStopHeight` **no altera el corte del perfil** (D3).

Modos y no dos tipos, porque las tres variantes se diferencian en dos números, no en su naturaleza.

**La placa final es perpendicular al EJE INCLINADO**, no al mundo: su normal es el eje del brazo, su
horizontal es la transversal, y **su dirección de altura es la proyección de `+Z` mundial sobre el plano de
la placa**, normalizada. Con la convención de marco de D4 esa proyección es exactamente el eje local Y del
brazo, así que el tope crece visualmente hacia arriba aun con pendiente. Un rectángulo del mundo X-Z
dejaría la placa sin ser perpendicular al brazo, que es lo que se rechaza.

### D8 — Elegibilidad por combinaciones exactas, con el arreglo dentro de la variante

`ICantileverArmSectionPolicy` es inyectable y registra variantes por **id exacto de sección más arreglo**.
Una variante declara sección, arreglo permitido, orientación y detalle de geometría; las
**transformaciones de cada miembro** no se escriben a mano en la variante: las deriva
`CantileverArmBodyArrangementResolver` de `Bounds`, porque un espejo escrito a mano sería una segunda
autoridad frente a D2.

El **perfil sencillo es independiente de la familia**: cualquier sección con variante registrada sirve. El
perfil de producto será HSS, pero **I-37B no registra ningún id de producción**: el default visible se fija
cuando exista editor y se registren los diseños aprobados.

**PTR no se equipara a HSS.** Si el catálogo neutral no contiene la familia o la fuente de un PTR
requerido, no se inventa la sección, no se usa `secciones.csv` y no se amplía I-36: se registra como
candidato de catálogo futuro. Esto no bloquea la arquitectura, que trabaja por `StructuralSectionId`.

## Alternativas consideradas

| # | Alternativa | Por qué se descarta |
|---|---|---|
| A1 | Un campo `Member` único y un segundo campo opcional para el canal apareado | Un campo que a veces está y a veces no obliga a cada consumidor a comprobarlo, y el día del tercer perfil no escala. La lista lo resuelve sin condicionales |
| A2 | Subclase por arreglo (`SingleArm`, `DoubleChannelArm`) | Multiplica los `switch` de tipo en cada consumidor: el modo de fallo que ADR-0024 D2 evita |
| A3 | Persistir la separación entre canales, con default 0 | Un campo con un único valor legal se acaba editando. Y no hay producto detrás de un valor distinto de cero |
| A4 | Persistir la pendiente en grados, o en grados **y** `RisePer12` | Dos verdades que se desincronizan en la primera edición |
| A5 | Longitud capturada = longitud total con placas | Cambiar el espesor de una placa acortaría el perfil, y el número dejaría de ser el que se le pide al proveedor |
| A6 | Que el brazo calcule su propio pitch de 4 in | Dos algoritmos para los mismos tornillos. Es PB-004 |
| A7 | Estirar la placa en silencio cuando el cuerpo no cabe | Produce una pieza que parece resuelta y no lo está, y contradice la decisión del Owner de que más filas es lo que la alarga |
| A8 | Tapa y tope como tipos distintos | Se diferencian en dos números, no en su naturaleza |
| A9 | Placa final como rectángulo del mundo X-Z | Deja de ser perpendicular al brazo en cuanto hay pendiente |
| A10 | Deducir la orientación del canal de su nombre | El nombre no dice de qué lado abre. Se leyó el constructor de I-36, que lo documenta y lo implementa |

## Consecuencias

**Positivas.** Un consumidor recorre `Members` sin saber del arreglo. El corte que el usuario captura es el
que compra. El brazo hereda el pitch de la columna sin acoplarse a su valor. Una combinación imposible se
rechaza con un mensaje que dice qué subir. Y la tercera y cuarta variante de cuerpo son un valor de enum
más una rama en una autoridad, no una capa.

**Negativas, y asumidas.** `Members` obliga a un índice en los ids de pieza incluso cuando hay uno solo, y
la simetría de los dobles se comprueba numéricamente en vez de garantizarse por tipo — dos pruebas en vez
de una firma.

Y la más importante, porque es una **imprecisión geométrica real y no una limitación de forma**: con
pendiente, el corte a escuadra del perfil **no queda a ras** de la placa de conexión. Una zona penetra la
placa y la opuesta deja holgura, y el modelo **no las resuelve** — las **declara**, con las dos magnitudes
medidas, en un diagnóstico informativo. Corregirlo exige un corte inclinado o una preparación de extremo, y
las dos cosas están fuera de alcance. Cualquiera que consuma esta geometría para fabricar tiene que saberlo,
que es exactamente por qué el diagnóstico existe en vez de un comentario.

**Lo que este ADR NO decide.** No decide la estación, la lista de niveles, la doble cara como sistema, los
separadores, los arriostres, la línea, el BOM, el peso, la persistencia, las vistas, el editor ni AutoCAD.
Tampoco registra ningún id de producción ni ninguna familia nueva de catálogo.

## Referencias

- [ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md) — sección ≠ miembro ≠ pieza comercial.
- [ADR-0022](0022-geometria-parametrica-de-secciones-estructurales.md) — ejes locales y origen en el centroide.
- [ADR-0024](0024-fundacion-cantilever-base-columna.md) — la fundación que este ADR extiende.
- [Contrato de I-37B](../initiatives/I-37B-cantilever-brazo.md).
- [Decisiones del Owner para I-37](../automation/decisions/I-37.md).
