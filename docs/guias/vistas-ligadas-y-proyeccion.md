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

1. Ejecuta `RACKPROYECTAR` (o `RPY`) y selecciona las vistas de rack a proyectar.
2. Elige la clase de vista destino: `Frontal`, `Lateral` o `Planta`.
3. Si algo impide la operación, el comando lo dice **antes** de pedir ningún punto y no escribe nada.
4. Se muestran los **avisos** (no bloquean): superposición entre las vistas nuevas, corrida cerca del límite del
   sentido, pieza visual opcional sin bloque.
5. Indica el **punto base** y el **punto de destino**. Todas las vistas se trasladan juntas con esa misma
   transformación.
6. El comando importa los bloques de biblioteca necesarios, comprueba que estén en el dibujo y crea todas las
   vistas en una sola operación.

### 4.2 Modos de proyección

| Cuando proyectas… | Qué ocurre |
|---|---|
| **A la misma clase** (Planta → Planta, etc.) | Modo rígido: se reproduce la disposición de las vistas, solo trasladada; cada una conserva su giro y la variante de su vista origen. |
| **Entre planta y una elevación** (Planta ↔ Frontal, Planta ↔ Lateral) | Modo ortográfico: las vistas quedan alineadas sobre una línea común, con el orden y la separación que tenían sobre la corrida, en la orientación natural de la vista destino. El sentido de la línea lo fija el marco de la vista origen, no el número de racks girados. |
| **Frontal ↔ Lateral** | No se expone: el comando lo dice y no escribe nada. |

Los rangos de cada rack sobre la corrida se conservan; no se recuperan coordenadas descartadas ni orientaciones
en un viaje de ida y vuelta, salvo la traslación.

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
