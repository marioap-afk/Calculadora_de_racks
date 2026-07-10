# Roadmap operativo

RackCad ya no es "solo un configurador de cabeceras": es un plugin de AutoCAD
(.NET, `net8.0-windows`, WPF) que diseña, dibuja y **edita en sitio** cuatro tipos
de rack. Las fases originales de este roadmap ya estan cerradas (incluidas las
vistas lateral y planta del selectivo); lo que queda vivo es el refinamiento continuo.

Menu principal: comando `RACKCAD` (`RackMainMenuWindow`).

## Estado actual - lo ya hecho

### Configurador de cabecera (marco)

- `RackFrameConfiguratorWindow`: un marco = 2 postes + placas base + celosia.
  Horizontales = fuente de verdad; paneles derivados.
- Peralte de la placa base **editable por placa** (`BasePlatePlacement.PeralteOverride`;
  `null` = derivado con `StandardPeralte` = base + slope * peralte del poste).
- Modo "Configuracion rapida": `Insertar` genera de un clic.
- Comandos: `RACKCABECERA`, `QUICKCABECERA`.

### Sistema dinamico (pallet flow)

- `RackDynamicSystemWindow`: cabeceras a lo largo del tramo + separadores por nivel.
- Comando `RACKSISTEMADINAMICO` (y opcion en el menu).

### Cama de rodamiento (flow bed)

- `RackFlowBedWindow`: riel + rodillos + frenos + tope (dinamica o pushback).
- Comando `QUICKCAMA` (y opcion en el menu).

### Selectivo (editor avanzado pallet-driven)

- `RackSelectiveWindow`: matriz **FRENTES x niveles** (el termino es "frente",
  no "bahia"). Cada celda = tarima (frente/alto) + tarimas por nivel + larguero +
  peralte de larguero (combo desde la lista `peraltes` del catalogo).
- Geometria en `SelectiveGeometryResolver` + builders frontal/lateral/planta: largueros por
  troquel, claro/separacion por tarima + holgura, altura por frente medida desde el
  escalon (1/3 de la tarima superior), datum de PISO en Y=0, "larguero a piso" por
  frente (elevacion editable). Overrides manuales opcionales (vacio = auto): longitud
  de larguero y claro por celda, altura por frente, elevacion de larguero a piso.
- Cada poste (N frentes -> N+1 postes) puede referenciar una **cabecera por poste**
  (`RackFrameConfiguration` embebida) de la que sale su placa/peralte; en la vista
  frontal se usa la placa, y de ella sale el corte lateral de ese poste.
- BOM: `SelectiveBomBuilder` + `RackBomWindow` (postes, placas, largueros, mensulas;
  grid + export CSV).
- **Doble profundidad (espalda con espalda), Fase 1**: `DepthCount` (1..4 fondos; 1 = sencillo
  clasico) a lo largo del eje de fondo, unidos por separadores **por hueco** (`SeparatorLengths`,
  uno por hueco entre fondos consecutivos; el bloque separador aun NO se dibuja, solo se deja el
  hueco vacio). **Cada fondo tiene sus propios niveles/alturas** (`ExtraFondoBays` guarda la matriz
  de los fondos 1..N-1; vacio = ese fondo hereda las `Bays` del fondo 0), pero **todos comparten la
  rejilla horizontal del fondo 0** (anchos de frente -> posicion de postes) para que los postes
  alineen; un frente sin niveles = **columna vacia** (no dibuja larguero ahi). El sistema resuelto
  expone `FondoBays` (bahias por fondo) y el helper puro `SelectiveDepthLayout` calcula
  offsets/separadores (`BaysOfFondo`, `FondoSystemView`). La **frontal** sigue mostrando el fondo 0;
  **lateral y planta dibujan TODOS los fondos** (cada uno con su altura de cabecera y sus largueros).
  El **BOM suma el contenido real de CADA fondo x2** (frente/atras), no un multiplicador plano por
  numero de fondos. UI: selector "Editando fondo" (los frentes se editan en Fondo 1, compartidos por
  todos) + campos de separador por hueco; la vista previa frontal sigue al fondo en edicion.
- Comando `RACKSELECTIVO`.

### Identidad + round-trip (los cuatro tipos)

- Cada rack dibujado = una definicion de bloque; las copias son referencias a ella.
- En la **definicion** del bloque se embebe (extension dictionary, Xrecord troceado
  <=255) un sobre unificado `RackEmbedDocument { SchemaVersion, Kind, View, Section,
  Id (GUID), Name, Design }`. Kinds: `selective`, `dynamic`, `cabecera`, `cama`;
  views: `frontal`, `lateral`, `planta` (`Section` = indice de corte lateral del
  selectivo; -1 = vista no seccionada).
- Comando `RACKEDITAR`: selecciona un rack -> lee el sobre -> **despacha por Kind** ->
  reabre el editor correcto precargado (`LoadExisting`) -> al confirmar **redefine la
  definicion en sitio** (`RedefineSystemBlock` + Regen) => todas las copias se
  actualizan a la vez, ninguna se mueve.
- **`RACKDUPLICAR`**: copia un rack como INDEPENDIENTE (GUID nuevo, nombre "- copia");
  editar la copia no afecta al original. Distinto del `COPY` de AutoCAD, que comparte
  definicion/GUID y edita todas las copias juntas.
- **`RACKLISTA`**: tabla de todos los racks del dibujo (nombre, tipo, vistas presentes,
  numero de copias; `RackListBuilder` agrupa los sobres por GUID) con zoom a la primera
  referencia del rack elegido.
- El nombre "Rack A" (campo en cada editor) = nombre del bloque; el GUID va en el
  sobre para evitar colisiones.
- Stores del diseno: `SelectivePalletDesignStore` (selectivo),
  `RackProjectStore` (dinamico/cabecera), `FlowBedConfigurationStore` (cama).

### Catalogos externos

Cargados por `JsonRackCatalogProvider` a `RackCatalog` desde `assets/catalogs/*.csv`:
`secciones` (todos los perfiles estructurales en una hoja con columna `rol`: POSTE = postes
y refuerzos, CELOSIA = horizontales + diagonales, LARGUERO = largueros; columna `peraltes` =
valores permitidos, FK a mensula), `mensulas`, `base-plates` (peralteBase /
peraltePorPeraltePoste -> `StandardPeralte`), `connection-points` + `connection-layout`
(por vista, X e Y: local + localPorParam * valor(param)), `blocks`, `views`,
`flow-bed-profiles`, `spacers-profiles`. Excel-first: el `.csv` gana sobre el `.json`,
acepta UTF-8 y ANSI, y la cache se invalida por firma de archivos (editar el CSV y
relanzar el comando recarga).
Persistencia de proyecto: `RackProjectStore` -> `.rackcad.json`.

### Vistas lateral y planta del selectivo (Fase 5, cerrada)

El selectivo se dibuja hoy en **TRES vistas** ligadas por el mismo GUID:

- **Frontal**: un bloque (postes + placas + largueros por nivel).
- **Lateral**: cortes **por poste** — un bloque por poste; cada corte es la
  cabecera de ese poste en perfil + las secciones de largueros frente/atras por
  nivel. Al insertar se pregunta que corte (por numero de poste) y se coloca con jig.
- **Planta**: un bloque (una cabecera-planta por frente apilada en Y + largueros
  frente/atras por bahia a lo largo de Y; X = fondo, Y = frente).

Las vistas lateral/planta solo se insertan desde `RACKEDITAR` de una vista
existente (los botones se deshabilitan con tooltip si no aplica), asi nunca
quedan huerfanas. `RACKEDITAR` sobre cualquier vista reabre el editor del sistema
completo y al confirmar redibuja **todas** las vistas (encontradas por GUID
escaneando las definiciones de bloque). La cabecera, por su parte, tiene vistas
lateral y planta ligadas igual; dinamico y cama dibujan lateral.

Convencion permanente de botones del editor: **Actualizar** redibuja la vista en
sitio (redefine su definicion, sin mover copias); **Insertar {vista}** agrega una
vista nueva enlazada al mismo rack (mismo GUID) y refresca. Asi el editor distingue
"actualizar lo que ya existe" de "agregar una vista mas del mismo rack".

## Refinamientos (estado 2026-07-10)

Configurador de cabecera:

- ✅ Multiseleccion de paneles + aviso de elevacion duplicada (ya implementado; `SelectionMode=Extended` +
  `FrameModelValidator`).
- ✅ Restaurar estandar cubre todos los campos tecnicos (verificado: `CopyConfiguration` clona campo por campo).
- ✅ Edicion masiva de HORIZONTALES (multiseleccion + aplicar perfil/cantidad/cara/offset de elevacion en lote).
- ✅ Primer incremento de validacion de ingenieria: el refuerzo del poste no puede superar la altura del marco.

Vista previa / dibujo:

- ✅ Cara Front/Back/Both y horizontales dobles se diferencian en el preview (colores + offset).
- ✅ Puntos de conexion seleccionables en el preview (clic sobre el punto lo selecciona en el editor).

BOM y cotizacion:

- ✅ Agrupacion por perfil/longitud/cantidad extendida a los 4 tipos (cama: `FlowBedBomBuilder`).
- Pendiente: integrar con el archivo cotizador existente; BOM consolidado multi-rack + export a Excel.
- Validaciones de ingenieria: seguir agregando reglas (capacidad de carga, holguras) de forma gradual.

Selectivo (doble profundidad):

- ✅ Fase 1 (espalda con espalda): N fondos con niveles por fondo, rejilla horizontal compartida del
  fondo 0, separadores por hueco y BOM que suma el contenido real de cada fondo.
- Pendiente (Fase 2): **"medio frente"** — un fondo que subdivide una bahia con un poste intermedio
  (media carga) y realinea en el siguiente poste compartido. El bloque separador fisico tambien queda
  pendiente de dibujar (hoy solo se deja el hueco).

## Como agregar un tipo de rack nuevo

Patron reutilizable ya probado con los cuatro tipos:

1. Definir su `Kind` en `RackEmbedDocument`.
2. Agregar `Edit<Kind>` en `RackFrameCommands` (despacho de `RACKEDITAR`).
3. Implementar `LoadExisting` en su ventana editora.
4. Implementar embed + redibujo en sitio en su draw service.
