# ADR-0026: La estación Cantilever — caras, niveles, altura y BOM por componentes

- **Estado:** propuesto
- **Fecha:** 2026-07-29 (redacción)
- **Decisores:** Mario Pérez, Owner del repositorio (decisiones de producto emitidas al abrir I-37C);
  Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-37C `architecture/cantilever-estacion-bom`
- **No reemplaza a ninguna ADR.** Extiende [ADR-0024](0024-fundacion-cantilever-base-columna.md) y
  [ADR-0025](0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md) hacia el primer **compositor**: ningún
  contrato que I-37A o I-37B congelaron se reabre.

## Contexto

I-37A resolvió columna y base. I-37B resolvió el brazo y su conexión, consumiendo los troqueles regulares
que la columna ya tenía. Las dos son piezas, y ninguna sabe formar un producto. Componerlas trae cinco
preguntas nuevas, y cada una tiene una respuesta equivocada que parece razonable.

### 1. Qué es una góndola doble

La respuesta cómoda es «dos estaciones espalda con espalda», o «dos subensambles columna–base». Las dos son
falsas y caras: en una góndola doble hay **una** columna física y **una** placa inferior de columna. Si el
modelo produce dos, el BOM pide el doble de columnas, la geometría solapa dos perfiles en el mismo sitio, y
el error no se ve hasta que alguien cuenta piezas.

### 2. Dónde vive la altura de la columna

`CantileverColumnBaseDesign` lleva su propia altura, porque en I-37A el usuario la escribía. En una estación
la altura se **calcula**. Si el template de estación conserva un campo de altura, existen dos autoridades
para un número, y una de las dos queda dormida esperando a que alguien la edite. El mismo problema aparece
con el `LowerColumnPunchIndex` del brazo: la estación lo calcula por nivel, así que un `CantileverArmDesign`
completo guardado dentro de la estación traería un índice que nadie usa.

### 3. Una dependencia circular real

```
altura de columna → cuántos troqueles regulares existen → dónde caen los niveles → altura mínima
```

No es una circularidad aparente: es real. Las salidas cómodas son todas trampas. Una altura provisional
mágica mete un número que nadie aprobó. Una columna arbitrariamente enorme funciona hasta que alguien pide
un nivel más alto que el margen inventado. Un bucle «que eventualmente converge» no tiene contrato y no se
puede probar. Y una segunda fórmula de pitch es exactamente el defecto PB-004 que I-37B evitó.

### 4. Qué mide el claro libre

Un claro puede medirse entre ejes de miembro, entre centros de troquel, entre bordes de placa o entre
cuerpos. Sólo una de esas cuatro es lo que el usuario ve cuando mira si su carga cabe.

### 5. Qué es un componente del BOM

El precedente del repositorio ya lo resolvió para cabeceras y largueros: el componente es lo que se compra o
se fabrica como una unidad y se atornilla al resto. Los troqueles no son piezas: son agujeros.

## Decisión

### D1 — La estación tiene dos modos de cara, y la doble comparte columna

`CantileverStationFaceMode` tiene exactamente dos valores: `Single` y `Double` —«góndola sencilla» y
«góndola doble» de cara al usuario—.

Una **góndola sencilla** tiene una columna, una base, una lista de niveles y **un brazo por nivel**, todos en
el mismo **lado activo**, que puede ser `PositiveY` o `NegativeY`.

Una **góndola doble** tiene **una sola columna física**, **una sola placa inferior de columna**, **dos bases
espejadas** —una en `PositiveY` y otra en `NegativeY`—, **una sola lista de niveles compartida** y **dos
brazos físicos por nivel**, uno por lado. Ambos lados comparten el mismo índice inferior de troquel y la
misma elevación lógica del nivel; cada brazo puede diferir por override de celda; y para la separación al
siguiente nivel **gobierna el lado más restrictivo**.

**Una góndola doble NO se modela como dos estaciones ni como dos subensambles columna–base completos.** La
base negativa se **deriva por espejo** respecto al plano central de la columna, mediante una autoridad de
espejo propia que compone o transforma miembro de base, placa frontal, placa posterior, cartabón y troqueles
de placa, preservando longitudes, espesores, patrón lógico, ids únicos, simetría y lado. Resolver un segundo
subensamble completo y «quitarle» la columna está **prohibido**: produciría dos columnas y dos placas
inferiores que luego habría que descartar a mano.

### D2 — Templates sin autoridades de posición duplicadas

La estación guarda **templates**, no diseños completos, exactamente donde ella es la autoridad del valor que
falta.

`CantileverStationColumnBaseTemplateDesign` lleva sección de columna, placa inferior, diseño de base y
diseño de conexión — y **no lleva altura**. La estación calcula la altura y **después** construye
explícitamente un `CantileverColumnBaseDesign` para consumir I-37A. **No existe una altura dormida dentro del
template.**

`CantileverArmTemplateDesign` lleva cuerpo, template de placa de conexión —espesor, cantidad de troqueles y
margen vertical— y placa final, y **no lleva `LowerColumnPunchIndex`**. La estación combina template, lado e
índice calculado y produce un `CantileverArmDesign` que consume I-37B. Ese mapeo vive en **un único
adaptador**: dos sitios que compongan el mismo diseño divergen.

El mismo principio gobierna la cantidad de niveles y la elevación del primero. `LevelCount` **no se
persiste**: es `Levels.Count`, y debe haber al menos un nivel. La elevación del primer nivel **no se
persiste**: se persiste `FirstLevelPunchIndex`, base cero y obligatorio, y la elevación visible se **deriva**
del índice.

### D3 — Un default y sólo overrides de celda

La estación persiste un `DefaultArmTemplate` y **únicamente los overrides de celda que difieren de él**:

```
EffectiveArm = CellOverride ?? DefaultArmTemplate
```

**No existen capas simultáneas** de override global, por nivel, por estación y por celda. Los alcances
—celda, nivel, estación, y sus restauraciones— son **operaciones** que escriben o borran overrides de celda,
no autoridades persistidas superpuestas. Es la lección de PB-014 y del tope que vivía en cuatro sitios de la
UI a la vez.

**Y «sólo lo que difiere» se cumple por comparación ESTRUCTURAL, no por referencia.** Guardar una copia
profunda sin condición parece inofensivo y no lo es: aplicar el default a una celda almacenaba una copia de
él, y desde ese momento la celda dejaba de **seguirlo**, así que cambiar el default la dejaba atrás sin que
nada en el diseño dijera qué celdas se fijaron a propósito. `CantileverArmTemplateComparer` compara los diez
campos editables por valor —y un margen **ausente** no es igual a cero, porque uno se rechaza y el otro no—;
aplicar el default guarda `null`, aplicar el mismo override dos veces no ensucia el diseño, y cada celda sigue
recibiendo su propia copia. El resultado agregado distingue las celdas **alcanzadas** de las realmente
**modificadas**: confundirlas hacía que una ventana informara «N celdas modificadas» de una operación que no
cambió ninguna.

La matriz pura expone las **celdas activas** de `nivel × lado`. En sencilla una operación de nivel afecta
sólo el lado activo y **el lado inactivo no aparece como celda falsa**; en doble afecta ambos lados. Una
operación de estación afecta todas las celdas activas, y aplicar un alcance produce **un solo resultado
agregado** en vez de N notificaciones. Ninguna operación cambia la cantidad de niveles, el claro libre, los
índices calculados ni el tipo de estación.

### D4 — El claro libre es cuerpo a cuerpo, y el ajuste es hacia arriba

`RequestedClearHeight` es finito, positivo y global para la estación en el MVP. Su definición aprobada es la
**distancia vertical entre la parte superior del cuerpo del brazo inferior y la parte inferior del cuerpo del
brazo superior, medidas en el plano de conexión con la columna**. No se mide desde ejes de miembro, centros
de troquel ni bordes de placa.

Además, dos condiciones que el claro solo no garantiza: las **placas de conexión** de dos niveles no pueden
traslaparse, y los **rangos de troqueles** de niveles consecutivos no pueden compartir datums —dos brazos de
niveles distintos no pueden atornillarse al mismo agujero—:

```
CandidateLowerIndex >= PreviousLowerIndex + max(PreviousVerticalPunchCount por lado activo)
```

El nivel 0 usa `FirstLevelPunchIndex` exacto, en ambos lados si la estación es doble. Para cada nivel
posterior se prueban candidatos **hacia arriba** y se acepta **el primero** que cumple las tres condiciones
en **todos** los lados activos. El ajuste es **obligatorio hacia arriba**: la elevación nunca se redondea
directamente, y la salida es siempre un **índice de la retícula**.

En doble, los dos claros se comprueban **por separado** —`ClearPositiveY` y `ClearNegativeY`— y el candidato
sólo vale si ambos cumplen. El lado más restrictivo gobierna porque el mínimo de los dos es el claro efectivo
del nivel. Mezclar el brazo superior de un lado con el inferior del otro sólo es legítimo para producir un
**resumen conservador**, nunca para decidir.

El cálculo usa sección, arreglo, peralte combinado, pendiente, margen de placa y cantidad de troqueles del
**brazo efectivo de cada celda**: no puede suponer que todos los niveles usan el default.

**Y lo mide UNA sola autoridad.** `CantileverArmConnectionMetricsResolver` valida y mide la conexión, y la
consumen **I-37B y el layout de I-37C**. Antes había dos: el layout traía su propio `Math.Max(2, count)` y su
propio `offset ?? 0`, que convertían una cantidad de filas de cero en dos y un margen **obligatorio** ausente
en cero — así que un diseño que I-37B habría rechazado producía un layout confiado, y el rechazo aparecía
después, contra números que ya habían dimensionado la columna. La autoridad es dueña de la cantidad mínima de
dos **sin ajustar**, del margen obligatorio finito no negativo y de al menos el radio, de la pendiente finita
no negativa, del marco representable, del arreglo y la orientación con regla, y de que el cuerpo quepa en su
placa. Una entrada inválida es **diagnóstico bloqueante antes del layout**, nunca una normalización y nunca una
excepción.

El **claro libre global** del MVP está **aprobado** y no es una decisión pendiente; un claro por nivel es una
iniciativa posterior.

### D5 — La retícula regular es una autoridad única, y la circularidad se resuelve explícitamente

Se **extrae** `CantileverColumnRegularPunchGrid` de `CantileverColumnBaseResolver`. Conoce, desde los
contratos ya resueltos de I-37A, la primera elevación regular, el pitch, las dos coordenadas X, el diámetro,
`ElevationAt(index)`, `DatumsAt(index)`, la altura mínima necesaria para incluir un índice, y la generación
limitada por una altura final.

**I-37A consume esa misma autoridad** para construir sus `ColumnRegularPunches`, preservando **exactamente**
sus firmas y sus resultados. La extracción es **mecánica**, va precedida de una **caracterización** de los
valores actuales, y una desviación numérica obliga a **detenerse**. La fórmula
`LastConnectionElevation + index × pitch` **no puede existir fuera de la autoridad**.

**Y la autoridad ACUMULA, no multiplica. Eso es normativo, no pendiente.** La caracterización lo forzó: con
un pitch **no diádico** las dos formas difieren —el índice 2 de un pitch de 3.7 in es `27.599999999999998`
acumulado y `27.6` multiplicado—, y acumular es lo que I-37A **ya hacía**. Cambiar a la multiplicación movería
**todos** los agujeros de una columna con pitch no diádico, que es un cambio de comportamiento en código
integrado. **La acumulación se preserva por compatibilidad con I-37A durante todo I-37C**, y sustituirla por
`FirstElevation + index × Pitch` sería una **normalización numérica separada** que no forma parte de esta
iniciativa. `ElevationAt(index)` recorre los mismos pasos que la generación, así que sigue habiendo **una**
definición y un índice no puede discrepar de la secuencia.

**La retícula tiene un DOMINIO, y no un máximo de producto.** Un índice sólo está definido mientras la
secuencia acumulada sigue siendo la retícula que dice ser: la desviación respecto al ideal crece como
`n · eps · z` y supera un **pitch entero** cuando `n > 1/√eps`, es decir `2²⁶`. Más allá, la secuencia se
movió más de un agujero completo. Ese bound se **deriva** de la precisión del `double` y del pitch, no de
ninguna decisión comercial —con 4 in son 268 millones de pulgadas—, y es también lo que permite rechazar un
índice como `int.MaxValue` en tiempo constante en vez de recorrer dos mil millones de sumas.

**No hay tope de candidatos.** Las elevaciones crecen estrictamente, así que cada regla se convierte en «la
elevación debe alcanzar Z» y el primer índice que lo cumple se **encuentra**; el índice del nivel es el mayor
de los mínimos por lado, y la monotonía garantiza que es el **primero** factible. La terminación sólo ocurre
por índice encontrado, dominio agotado, entrada no finita, o retícula que no crece —que es diagnóstico
bloqueante antes de cualquier búsqueda—. Un nivel válido trescientos índices más arriba **resuelve**.

Con eso, la circularidad se resuelve en una **secuencia explícita**, sin altura provisional y sin bucle de
convergencia: validar el diseño; resolver secciones y variantes; obtener la retícula canónica —que sólo
necesita el patrón de conexión, no la altura—; resolver índices y métricas de niveles; calcular
`MinimumColumnHeight`; elegir altura automática o validar la manual; construir el `CantileverColumnBaseDesign`
final; resolver I-37A con la altura final; componer una o dos bases; resolver todos los brazos con I-37B
contra la columna final; **verificar que los datums e índices finales coinciden con el layout previo**; y
construir estación, envolvente, BOM y firma.

Si el pase final difiere del layout previo, la resolución **falla cerrado**. Una recomputación distinta
aceptada en silencio es cómo un modelo empieza a mentir.

**El pase final compara TODO lo que decidió el layout**, y no una parte. Por nivel y lado: índice inferior,
índice superior, cantidad de troqueles, primera y última elevación, borde inferior y superior de placa, y
**borde inferior y superior del cuerpo**. Los dos últimos faltaban, y son precisamente aquello entre lo que se
mide el claro: una fórmula de cuerpo equivocada producía una estación cuyos claros reales eran menores que los
pedidos mientras todos los demás números seguían coincidiendo. Después vuelve a comprobar, **sobre los brazos
resueltos**, el claro por lado, su igualdad con el del layout, el traslape de placas, la disjunción de
troqueles y la ocupación final contra la altura mínima que se usó.

### D6 — Altura automática o manual, con margen superior parametrizado

`CantileverStationColumnHeightMode` tiene `Automatic` y `Manual`.

`TopClearFactor` se persiste, con default `1/3`, y debe ser finito y **≥ 1/3**:

```
RequestedTopClear   = RequestedClearHeight × TopClearFactor

LastOccupiedTop     = max( cuerpo superior de todos los brazos del último nivel,
                           borde superior de todas sus placas de conexión,
                           elevación del último troquel utilizado )

MinimumColumnHeight = max( LastOccupiedTop + RequestedTopClear,
                           HighestUsedPunchElevation + ColumnTopPunchOffset )
```

La **tapa y el tope del extremo libre no cuentan** para la altura de la columna: están fuera del plano de
conexión.

En `Automatic`, `ResolvedColumnHeight = MinimumColumnHeight`, **sin redondeo comercial** —ninguno está
aprobado—. En `Manual`, `ManualColumnHeight` es obligatorio, finito, positivo y **≥ MinimumColumnHeight**; si
es menor se **bloquea**, y no se recortan niveles, no se mueven brazos, no se reduce el claro y no se
normaliza en silencio. La altura manual puede **aumentar** la columna, nunca reducirla por debajo de la
mínima.

### D7 — La estación es un resultado inmutable, y el BOM se deriva de ella

`CantileverStationAssembly` es inmutable y determinista: modo, lado sencillo cuando aplique, columna–base
compuesta, niveles, brazos, altura mínima, altura resuelta, claro solicitado, claros reales, miembros,
placas, troqueles, envolvente, diagnósticos y firma.

**La firma incluye los brazos REALES.** Listar sólo los planes de layout hacía que dos estaciones cuyos
niveles caían en los mismos índices firmaran igual aunque cada brazo fuera otra pieza —otro corte, un tope en
vez de una tapa, otro espesor de placa final, un canal doble en vez de uno sencillo con el mismo peralte—.
Ahora lleva, en orden determinista: la firma de columna–base, la de cada nivel, la de cada **brazo resuelto**
por nivel y lado, la altura mínima y la resuelta, el claro solicitado, los claros reales, el modo y el lado
activo. El **BOM no** se usa como autoridad de geometría —agrupa, y agrupar esconde diferencias—, pero la
firma se mueve cuando se mueve cualquier pieza física. Una estación **bloqueada** conserva sus diagnósticos y
**no produce BOM**.

**No contiene** posición longitudinal X, índice dentro de una línea, separadores, arriostres ni referencias a
estaciones vecinas: eso es una iniciativa posterior, y meterlo ahora obligaría a quitarlo después.

El BOM se deriva **únicamente de la estación resuelta**. No del diseño, no de la matriz, no de vistas y no de
bloques: un BOM calculado desde la intención cuenta lo que el usuario pidió, no lo que el modelo resolvió.

### D8 — Columna–base y brazo son los componentes atornillables

Se usa el modelo compartido `BomComponent` / `BomLine` / `BillOfMaterials` **sin modificarlo**, salvo
necesidad objetiva demostrada.

Una estación produce **exactamente un** `ColumnBaseComponent`. Su receta por unidad es, en sencilla, una
columna, una placa inferior de columna, una base, una placa frontal, una placa posterior y un cartabón; en
doble, una columna, una placa inferior de columna, **dos** bases, dos placas frontales, dos placas
posteriores y dos cartabones. Producir **dos** componentes columna–base para una estación doble está
**prohibido**: una góndola doble es **un** componente columna–base con dos bases.

Cada **brazo físico atornillable** es un componente, con una receta de uno o dos perfiles de cuerpo, una
placa de conexión y cero o una placa final.

**Los troqueles no son piezas del BOM.**

Los brazos se agrupan por **receta física**, no por posición. La firma de BOM incluye arreglo,
`StructuralSectionId`, cantidad de perfiles, longitud nominal de corte, pendiente, espesor y dimensiones de
la placa de conexión, cantidad de troqueles, modo de placa final, espesor y dimensiones de la placa final, y
altura adicional de tope. **No incluye** lado, nivel, índice de estación, coordenadas mundiales ni el owner
token.

**Y una pieza plana se identifica por su receta, no por una etiqueta.** `BillOfMaterials` agrupa sus líneas
por `(Category, ProfileId, Length)`; con `Length = 0` —que es el precedente correcto para una placa— y un
`ProfileId` genérico, **todas** las placas de conexión de una estación colapsaban en una línea, y una placa
corta con dos agujeros y una alta con seis pasaban por la misma parte. El `ProfileId` de una placa lleva
tipo, sus **dos dimensiones en su propio plano**, espesor y el **patrón de perforaciones** cuando lo tiene
—cantidad, diámetro y coordenadas **relativas** al primer vértice, medidas sobre sus propios ejes, para que
dos placas iguales atornilladas a distinta altura o en caras opuestas salgan iguales—. El del cartabón lleva
sus dos piernas y su espesor. Los troqueles **siguen sin ser líneas del BOM**: son parte de la identidad de la
placa perforada, que es el único sitio donde un agujero pertenece.

**Y una placa se mide EN SU PROPIO PLANO.** Una caja alineada con los ejes del mundo sólo es correcta mientras
la placa es paralela a un plano del mundo, y la placa final de un brazo es perpendicular a un eje **inclinado**:
sus spans mundiales son proyecciones, así que al inclinar el brazo una tapa de 10 in reportaba 9.8 y luego 9.4
y el BOM partía una placa física en varias. La caja además devuelve los spans **ordenados**, de modo que una
sección más alta que ancha y otra más ancha que alta se describían igual.
`CantileverPlateInPlaneDimensions` mide sobre las aristas propias de la placa, validando contorno suficiente,
aristas no degeneradas, perpendicularidad y planitud; y la extensión de un tope se mide a lo largo del **up**
del brazo.

**Por decisión del Owner, un brazo idéntico en `PositiveY` y en `NegativeY` es el mismo componente del BOM.**
Sólo se separará por lado cuando exista una variante física derecha/izquierda, que I-37C no introduce.

La receta plana conserva componentes y piezas derivadas. Para perfiles estructurales,
`ProfileId = StructuralSectionId` y `Length = NominalCutLength`. Para placas y cartabones, categoría y
descripción deterministas y `Length = 0`, siguiendo el precedente vigente de `BomBuilder.AddPlate`: una placa
no tiene longitud lineal. **No se inventa** peso, material, costo, soldadura, tornillos ni anclas.

## Alternativas consideradas

**Dos estaciones para la góndola doble.** Descartada: duplica la columna y la placa inferior, y el BOM cuenta
mal. La comprobación de que hay exactamente una columna sería imposible de escribir.

**Altura provisional para romper la circularidad.** Descartada por ADR-0024 D-general: un número que nadie
aprobó, indistinguible de uno aprobado. La secuencia explícita de D5 no lo necesita.

**Redondear la elevación del nivel al múltiplo más cercano del pitch.** Descartada: el resultado tiene que
ser un **índice** de una retícula que ya existe, y redondear puede caer **por debajo** del claro pedido.

**Capas de override global + nivel + celda.** Descartada: es el modo de fallo de PB-014, donde el mismo dato
vivía en cuatro sitios. Los alcances son operaciones, no capas.

**Separar los brazos del BOM por lado.** Descartada por decisión expresa del Owner: sin variante física
derecha/izquierda, dos brazos idénticos son la misma pieza que se compra dos veces.

**Recalcular la retícula en la estación.** Descartada: sería la segunda fórmula del mismo espaciado, el
defecto que ADR-0025 D5 ya cerró para el brazo.

## Consecuencias

**Positivas.** La estación es el primer resultado de RackCad que se puede cotizar: un BOM por componentes
atornillables sobre geometría resuelta. La góndola doble sale de una sola columna, así que el conteo de
piezas es correcto por construcción y no por cuidado. La circularidad queda escrita como una secuencia de
once pasos que se puede leer y probar, y el pase final se **verifica** contra el layout previo en vez de
confiarse. Y la retícula regular pasa a tener **una** autoridad, que I-37A también consume: el brazo y la
estación no pueden divergir de la columna.

**Negativas, y asumidas.** Las extracciones tocan código integrado de I-37A y de I-37B —la retícula, la
costura independiente de la altura y las métricas de conexión—, así que exigen caracterización previa y
equivalencia numérica demostrada: trabajo que no añade ninguna función. Los
templates duplican la forma de los diseños de I-37A y I-37B sin heredarla, porque heredar traería los campos
que la estación gobierna; el precio es un adaptador y su prueba. El claro libre es global en el MVP, así que
una estación con niveles de claro distinto todavía no se puede pedir. Y el layout se resuelve **dos veces**
—una para calcular la altura y otra contra la columna final—, lo que cuesta tiempo de cálculo; se acepta
porque es exactamente lo que permite **verificar** que el segundo pase coincide con el primero en vez de
suponerlo.

**Lo que este ADR NO decide.** No decide varias estaciones, la separación longitudinal, la línea, los
separadores, los arriostres, las vistas, el preview, el editor, la persistencia, `RackSystemKind`, los
registros, la biblioteca, AutoCAD, los bloques, el peso, el costo, los materiales, la tornillería, las
anclas, las soldaduras, el cálculo estructural, las capacidades, la preparación de extremos, el CNC ni los
shop drawings. Tampoco registra ningún id de producción ni ninguna familia nueva de catálogo.

## Referencias

- [ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md) — sección ≠ miembro ≠ pieza comercial.
- [ADR-0024](0024-fundacion-cantilever-base-columna.md) — la fundación de columna y base.
- [ADR-0025](0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md) — el brazo y su conexión.
- [Contrato de I-37C](../initiatives/I-37C-cantilever-estacion-bom.md).
- [Decisiones del Owner para I-37](../automation/decisions/I-37.md).
