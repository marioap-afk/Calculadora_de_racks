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
- El nombre "Rack A" (campo en cada editor) = nombre del bloque; el GUID va en el
  sobre para evitar colisiones.
- Stores del diseno: `SelectivePalletDesignStore` (selectivo),
  `RackProjectStore` (dinamico/cabecera), `FlowBedConfigurationStore` (cama).

### Catalogos externos

Cargados por `JsonRackCatalogProvider` a `RackCatalog` desde `assets/catalogs/*.csv`:
`post-profiles` (postes; refuerzos = postes), `truss-profiles` (una sola lista de
celosia: horizontales + diagonales), `beam-profiles` (largueros; columna `peraltes` =
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

## Refinamientos pendientes (menores, en curso)

Configurador de cabecera:

- Probar casos con muchas horizontales y mejorar la edicion masiva.
- Validar seleccion multiple de paneles y advertencias de duplicados de elevacion.
- Confirmar que restaurar estandar cubre todos los campos tecnicos.

Vista previa / dibujo:

- Diferenciar mejor cara Front/Back/Both y horizontales dobles.
- Dibujar puntos de conexion seleccionables.

BOM y cotizacion:

- Extender agrupacion por perfil/longitud/cantidad/origen a los demas tipos.
- Integrar con el archivo cotizador existente.
- Agregar validaciones de ingenieria de forma gradual.

## Como agregar un tipo de rack nuevo

Patron reutilizable ya probado con los cuatro tipos:

1. Definir su `Kind` en `RackEmbedDocument`.
2. Agregar `Edit<Kind>` en `RackFrameCommands` (despacho de `RACKEDITAR`).
3. Implementar `LoadExisting` en su ventana editora.
4. Implementar embed + redibujo en sitio en su draw service.
