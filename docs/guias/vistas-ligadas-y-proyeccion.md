# Vistas ligadas y proyección de racks

Guía de uso de las vistas de un rack en AutoCAD: cómo elegir la primera vista, cómo agregar varias vistas
de una vez y cómo proyectar racks ya dibujados a otra clase de vista con `RACKPROYECTAR`. Los detalles de
diseño están en [I-55](../initiatives/I-55-creacion-de-vistas.md); esta guía cuenta lo que el usuario ve.

## 1. Idea general

Un rack es **uno solo** aunque tenga varias vistas. Cada vista (frontal, lateral, planta) es un bloque
**ligado** al mismo rack: comparten identidad, diseño, propiedades personalizadas y BOM. Por eso:

- editar un rack (`RACKEDITAR`) y elegir `Actualizar` cambia **todas** sus vistas;
- agregar una vista nueva **no** agrega un rack: `RACKLISTA` y `RACKBOMTOTAL` siguen contando el mismo rack;
- para obtener un rack **independiente** se usa `RACKDUPLICAR`, no una vista nueva.

## 2. Elegir la primera vista

Al crear un rack nuevo desde `RACKCAD`, `RACKSELECTIVO`, `RACKSISTEMADINAMICO` o `RACKCABECERA` no hace
falta empezar por una vista fija: la ventana ofrece como primera vista cualquiera de las que el sistema
admite.

| Sistema | Vistas que se pueden colocar primero |
|---|---|
| Selectivo | Frontal (por fondo), Lateral (por poste) y Planta |
| Dinámico | Frontal de entrada, Frontal de salida, Lateral (por poste) y Planta |
| Cabecera | Lateral y Planta (`QUICKCABECERA` conserva su lateral de siempre) |
| Push Back | Las vistas que ya ofrecía su editor |
| Cantilever | Las vistas que ya ofrecía su editor |
| Cama de rodamiento | Solo su vista actual: no admite vistas adicionales |

Si en el jig de la primera vista pulsas `Esc` o `Enter`, no queda ninguna definición con datos de rack. Elegir
Planta o Lateral primero no convierte esa vista en otro rack: el resto de vistas se agrega después con
`RACKEDITAR` → `Insertar`.

## 3. Agregar varias vistas de una vez

Con `RACKEDITAR`, o al crear un rack nuevo, el editor permite **seleccionar y ordenar** varias vistas y
colocarlas una tras otra (cola de vistas). Cada vista se coloca con su propio punto de inserción.

- La cola respeta el orden que elegiste y conserva la variante de cada vista (fondo, poste, entrada o salida,
  lado y extremo del Push Back, estación del Cantilever).
- Antes de colocar la primera se **preparan todas**: si algo no se puede preparar, no se modifica nada.
- Si el rack ya existía y el editor cambió su diseño, las vistas hermanas se **actualizan juntas, en un solo
  paso**, antes de pedir el primer punto. Si esa actualización falla, no cambia ninguna.
- `Esc` o `Enter` en una colocación detiene la cola: lo ya actualizado y las vistas ya colocadas se
  **conservan**; las que faltaban no se crean. El mensaje lo dice.
- Una biblioteca de bloques incompleta no impide colocar la vista: se coloca y se **informa** qué pieza falta.
- Un rack con propiedades personalizadas distintas entre sus vistas no admite vistas nuevas hasta unificarlas
  con `RACKPROPIEDADES`.

La cama de rodamiento no tiene cola de vistas.

## 4. Proyectar racks con `RACKPROYECTAR`

`RACKPROYECTAR` (alias `RPY`) toma vistas de racks **ya dibujados** y crea, a partir de ellas, **vistas
enlazadas** de otra clase, colocadas juntas en una nueva ubicación. Sirve, por ejemplo, para sacar las frontales
de una fila de plantas de un layout.

> Las vistas nuevas son vistas enlazadas del mismo rack, **no copias**: el rack no se duplica y el BOM no cambia.
> Para copiar un rack, usa `RACKDUPLICAR`.

### 4.1 Cómo se usa

1. Ejecuta `RACKPROYECTAR` (o `RPY`) y selecciona las vistas de rack a proyectar. Termina la selección con `Enter`
   (durante la selección, `F` es el «Fence» de AutoCAD, no la clase Frontal).
2. Elige la clase de vista destino: `Frontal`, `Lateral` o `Planta`.
3. Elige la **orientación**: `Orientacion [PRoyectada/PREdeterminada] <PRoyectada>`. `Enter` (o `PR`) usa
   **Proyectada**; para Predeterminada escribe `PRE` (o la palabra completa) o elígela en la lista. `Esc` cancela sin
   leer ni escribir nada.
4. Si algo impide la operación, el comando lo dice **antes** de pedir ningún punto y no escribe nada.
5. Se muestran los **avisos** (no bloquean): superposición entre las vistas nuevas, corrida cerca del límite del
   sentido, pieza visual opcional sin bloque.
6. Indica el **punto base** y el **punto de destino**. Todas las vistas se colocan juntas con esa misma
   operación.
7. El comando importa los bloques de biblioteca necesarios, comprueba que estén en el dibujo y crea todas las
   vistas en una sola operación. El mensaje final dice qué orientación se usó.

### 4.2 Modos de proyección

| Cuando proyectas… | Qué ocurre |
|---|---|
| **A la misma clase** (Planta → Planta, etc.) | Modo rígido: se reproduce la disposición de las vistas con la variante de su vista origen. Con orientación **Proyectada** (por defecto) solo se traslada y cada vista conserva su giro; con **Predeterminada** el grupo se gira para que sus vistas queden sin girar (ver 4.2.1). |
| **Entre planta y una elevación** (Planta ↔ Frontal, Planta ↔ Lateral) | Modo ortográfico: las vistas quedan alineadas sobre una línea común, con el orden y la separación que tenían sobre el eje que comparten con la vista origen. Con **Proyectada** (por defecto) esa línea sigue a la vista origen y cada referencia gira para seguirla; con **Predeterminada** las vistas quedan en la orientación natural de la vista destino y el sentido de la línea lo fija el marco de la vista origen, no el número de racks girados (ver 4.2.1). |
| **Entre las dos elevaciones** (Frontal ↔ Lateral) | Modo ortográfico sobre la **altura**, el único eje que comparten: la vista nueva se alinea por su **línea de suelo** con la de la vista origen. Con **Proyectada** su altura queda paralela y en el mismo sentido que la de la vista origen (la referencia conserva el giro de la vista origen); con **Predeterminada** queda sin girar. Solo en los sistemas que tienen las dos clases: la cabecera no tiene frontal y el comando lo rechaza antes de pedir puntos (`PairNotExposed`). Si varias frontales de una misma fila comparten el suelo, sus laterales caen en el mismo lugar y se avisa de la superposición. |

Los rangos de cada rack sobre el eje compartido se conservan; no se recuperan coordenadas descartadas en un viaje
de ida y vuelta. Con Predeterminada tampoco se recuperan las orientaciones (salvo la traslación); con Proyectada
la vista de vuelta sigue a la de ida.

### 4.2.1 Orientación: Proyectada (por defecto) o Predeterminada

La definición de bloque de la vista nueva es **siempre la normal de RackCad**: su geometría, textos, cotas y
números no cambian. Lo único que cambia entre los dos modos es el **giro de la referencia de bloque** colocada.

- **Proyectada** (por defecto): la vista nueva se comporta como una vista proyectada del dibujo. El eje del rack
  que comparte con su vista origen (la corrida entre planta y frontal, el fondo entre planta y lateral, la altura entre
  frontal y lateral) queda
  **paralelo y en el mismo sentido** que en la vista origen, y las vistas nuevas quedan alineadas sobre una recta
  que pasa por el punto de destino. Para lograrlo gira **toda** la referencia: geometría, textos, cotas y
  etiquetas giran con ella (puede quedar vertical o de cabeza; RackCad no endereza los textos). Por ejemplo, la
  planta de un Selectivo tal como se inserta tiene la corrida en vertical, así que su frontal proyectada queda
  girada 90°; si giras la planta hasta que la corrida quede horizontal (con el poste 1 a la izquierda), la
  frontal proyectada sale en su posición normal.
  - En la **misma clase** (Planta → Planta, etc.) cada vista conserva el giro de su vista origen.
  - Entre clases distintas, si en la selección hay racks paralelos pero **opuestos** (por ejemplo, espalda con
    espalda, uno girado 180°), no existe una orientación proyectada común: la operación se rechaza antes de pedir
    puntos (`SourceOrientationDivergent`). Proyecta cada sentido por separado o usa Predeterminada. (En la misma
    clase no hay rechazo: se copia el giro de cada rack.)
- **Predeterminada**: la presentación normal de RackCad, sea cual sea el giro de la vista origen. Entre clases
  distintas es exactamente el comportamiento anterior (vistas sin girar, en su orientación natural, alineadas sobre
  una línea común). En la misma clase el
  grupo se copia girado de modo que sus vistas quedan sin girar: el punto base cae en el punto de destino y la
  disposición entre racks se conserva. Si en la misma clase los racks tienen **giros distintos**, no pueden quedar
  todos sin girar en una sola operación y se rechaza antes de pedir puntos (`SourceRotationsDiffer`): usa Proyectada
  o proyecta por separado cada grupo de igual giro. Nunca se rechaza por la divergencia de Proyectada.

### 4.3 Qué sistemas admite

Selectivo, Dinámico, Push Back, Cabecera y Cantilever. La cama de rodamiento no se proyecta. Una selección con
familias mezcladas en modo ortográfico (por ejemplo, Cantilever y Selectivo en la misma fila) se rechaza.

### 4.4 Cancelar y errores

- `Esc` (cancelar), `Enter` (sin punto) o un error al pedir un punto: **no se escribe ni se importa nada**;
  `Enter` no vuelve a pedir el punto.
- Todo el trabajo se confirma **en una sola transacción**. Si algo falla al escribir, se deshace la operación
  completa y no queda ninguna vista, definición sin uso ni referencia parcial. Es posible que los bloques de
  biblioteca ya importados permanezcan en el dibujo.
- Se ignoran, con aviso, los objetos que no son bloques, los que están fuera del espacio modelo y los bloques sin
  datos de RackCad.
- Falla **toda** la operación, listando todo lo que la causa, si la selección incluye: referencias externas
  (xref), arreglos `MINSERT`, bloques dinámicos, anónimos o anotativos con datos de RackCad, escala distinta de
  1, reflexión (`MIRROR`), un mismo rack con varias definiciones, o vistas de clases distintas mezcladas.
- Un rack **sin nombre** (nulo, vacío o solo espacios; `RACKLISTA` lo muestra como «(sin nombre)») **sí se proyecta**:
  la vista nueva conserva el mismo rack (mismo GUID) y sigue **sin nombre**. RACKPROYECTAR no inventa nombres
  («Rack», «Selectivo», «Sin nombre»…) ni modifica las vistas existentes, y `RACKLISTA` sigue mostrando «(sin nombre)».
  El nombre del bloque de AutoCAD es otra cosa y siempre tiene un nombre descriptivo.
- Un rack con variables rotas, un diseño bloqueado, propiedades o diseño divergentes entre sus vistas, o una
  vista que `RACKEDITAR` rechazaría, no se proyecta: el mensaje nombra el rack y el remedio concreto.
- Si una pieza **requerida** no tiene su bloque (o la biblioteca no está disponible), el comando falla antes de
  pedir puntos con esa causa. Una pieza **opcional** solo produce un aviso.

### 4.5 Limitaciones

- El SCP actual debe tener su plano XY paralelo al universal (se admite un SCP girado en Z).
- No hay regeneración ni limpieza automática al terminar; el dibujo no se purga.
- `UNDO` deshace la operación como una sola escritura; comprueba lo observado en tu versión de AutoCAD.
- Las vistas proyectadas heredan el nombre base de la autoridad de nombres de RackCad; el comando no permite
  renombrarlas.
