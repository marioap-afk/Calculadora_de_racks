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
- Base modular: `DynamicRackDesign` (entradas editables) -> `DynamicRackSystemResolver` -> sistema resuelto multivista.
- Persistencia y payload DWG guardan el diseno; cabeceras calculadas se regeneran y las personalizadas se conservan.
- Largueros especiales de entrada/salida: una pareja completa `LARGUERO_IN_OUT_C6` por nivel, mate por origen en
  `X=0` y `X=TotalLength`, salida baja a la izquierda y entrada alta/espejeada a la derecha. El peralte sale del catalogo;
  no se dibuja una mensula separada. El BOM cuenta el bloque completo por frente, nivel y extremo, con longitud/peralte.
- Cama dinamica integrada en lateral: `DynamicFlowBedLateralBuilder` reutiliza la cama completa de
  `FlowBedLateralBuilder`, alinea `TROQUEL_IN` con `TROQUEL_CAMA`, conserva la pendiente resuelta, fija
  `LONGITUD = tramo longitudinal del frente - 4"` y comparte una
  definicion entre niveles. El BOM cuenta componentes `Cama` por posicion/nivel, sin despiece, con longitud y BFR.
- Poste derivado reforzado centrado en el limite entre separadores: `FIN_POSTE` del perfil principal coincide con
  ese limite y el refuerzo comienza ahi; el poste sencillo no se desplaza.
- Largueros intermedios en lateral: un `LARGUERO_ESCALON_INFINITO` por limite interno de modulo/poste y nivel; los
  extremos son IN/OUT y el derivado reforzado central cuenta como una sola posicion. El segundo poste usa espejo y
  `INICIO_DERECHO`; el primero usa `INICIO_IZQUIERDO`. Los contactos siguen la linea inclinada del **origen del
  riel**, no la linea de troqueles, y se agrupan por orientacion para compartir definiciones.
- Varios frentes/anchos/niveles: cantidad total por input entero, matriz frente x nivel y editor de celda inspirado en
  el selectivo. Posiciones/niveles/fondos/inicio/largo siguen al frente; el peralte intermedio se aplica a celda,
  nivel, frente o todas.
  Al aumentar la cantidad se conservan los frentes existentes y los nuevos copian el seleccionado. Cada posicion usa
  `BFR = frenteTarima + 2"`; el IN/OUT automatico mide `BFR * posiciones + 6"`, con override manual opcional.
  `DynamicFrontGeometry` comparte la reticula con frontal y planta y el DTO conserva fallback legacy de un frente.
- Fondos variables por frente: `Fondos` + `Inicio en fondo` viven en el panel de celda. `DynamicDepthGeometry`
  hace que el frente mas corto gobierne los dos `+6"` y el patron estructural; los frentes mayores contienen ese rango
  y prolongan la estructura. Un extremo en separador recibe poste limite sin cambiar el modulo a cabecera. Pendiente,
  camas, IN/OUT, intermedios, seguridad, cortes laterales y BOM consumen los rangos `StartX/EndX` resueltos.
- Peralte global de poste: campo numerico junto al tipo de poste, aplicado a cabeceras calculadas y personalizadas;
  el DTO nullable conserva documentos anteriores mediante el ancho del perfil catalogado.
- Frontal: dos cortes ligados (`Section=0` salida, `Section=1` entrada), solo postes/placas e IN/OUT; cada poste usa
  la altura del frente adyacente mas alto, no el maximo global.
- Planta: estructura longitudinal repetida en los limites de frente, IN/OUT e intermedios; camas omitidas por contrato.
  El refuerzo derivado continua despues del perfil principal sobre X, en la misma linea transversal y sin espejo.
- Peralte intermedio por frente y nivel: selector de celda + botones Celda/Nivel/Frente/Todas, con opciones de `secciones.csv`;
  lateral aplica el maximo de los frentes activos del nivel y planta el maximo del frente dibujado.
- Seguridad multivista: BOTA, LATERAL, DESVIADOR, DEFENSA y GUIA se resuelven desde una sola seleccion. DEFENSA
  conserva activacion/longitud independientes por poste y extremo; GUIA se habilita por frente/nivel en la entrada.
- Preliminares lateral/frontal salida/frontal entrada: lateral limitada al rango real del poste, frontales con alturas
  por adyacencia y resaltado de celda; numeracion de frentes/niveles, nombre y cotas configurables.
- BOM dinamico por componentes: cabeceras por linea transversal, apoyos derivados con estado reforzado, separadores,
  IN/OUT por frente/nivel/extremo, intermedios por longitud/peralte, camas por longitud/BFR y BOTA/LATERAL/DESVIADOR.
  BOTA/LATERAL usan planta como inventario fisico; DESVIADOR usa salida + entrada para conservar sus niveles. Ninguna
  familia se duplica por proyecciones alternativas.
- Estado de producto: el usuario valido visualmente el lote durante su desarrollo. Solo queda reconfirmar con los
  bloques DWG reales la correccion final que evita espejar el desviador en la frontal de entrada.
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
- BOM por componentes: `SelectiveBomBuilder` + `RackBomWindow` (cabeceras + largueros como
  componentes expandibles a piezas; arbol + CSV a dos niveles). `RACKBOMTOTAL` da el BOM
  consolidado de TODO el dibujo (desglose por rack x copias + gran total por componente,
  `RackConsolidatedBomWindow`, export CSV); ademas hay editor de larguero (`RackLargueroWindow`,
  menu "Disenar larguero"; solo visual/BOM, sin bloque de AutoCAD aun).
- **Doble profundidad (espalda con espalda), Fase 1**: `DepthCount` (1..4 fondos; 1 = sencillo
  clasico) a lo largo del eje de fondo, unidos por separadores **por hueco** (`SeparatorLengths`,
  uno por hueco entre fondos consecutivos; el bloque separador ya se dibuja en lateral y planta y entra
  al BOM (componente "Separador"); en la frontal solo se deja el hueco vacio, a proposito). **Cada fondo
  tiene sus propios niveles/alturas** (`ExtraFondoBays` guarda la matriz
  de los fondos 1..N-1; vacio = ese fondo hereda las `Bays` del fondo 0) **y su propio numero de
  frentes** (layout en esquina); el **fondo mas largo define la rejilla horizontal compartida**
  (anchos de frente -> posicion de postes) y los mas cortos son un **prefijo** de ella, asi los
  postes que se traslapan alinean; un frente sin niveles = **columna vacia** (no dibuja larguero
  ahi; si el frente maestro es una columna vacia, su ancho lo da la bahia real mas ancha de los
  otros fondos en ese indice). El sistema resuelto
  expone `FondoBays` (bahias por fondo) y el helper puro `SelectiveDepthLayout` calcula
  offsets/separadores (`BaysOfFondo`, `FondoSystemView`). La **frontal** dibuja un fondo (el fondo 0
  u otro; ver "Frontal por fondo"); **lateral y planta dibujan TODOS los fondos** (cada uno con su
  altura de cabecera y sus largueros). El **BOM suma el contenido real de CADA fondo x2** (frente/atras),
  no un multiplicador plano por numero de fondos. UI: selector "Editando fondo" (el numero de frentes
  se edita **por fondo**: `BayCountBox` habilitado en cualquier fondo, layouts en esquina) + campos de
  separador por hueco; la vista previa frontal sigue al fondo en edicion.
- **Fondo (profundidad) por fondo**: cada fondo tiene su PROPIO fondo de tarima ("Fondo de tarima"), no
  solo el fondo 0. Diseno `ExtraFondoDepths`; sistema resuelto `FondoDepths`. Los offsets, la lateral y
  la planta avanzan cada uno segun el fondo propio de ese fondo. El **frente de tarima por defecto es 42**
  (antes 40) y la **separacion entre fondos por defecto es 12** (antes 8).
- **Regla del fondo de cabecera**: la cabecera dibujada (marco) usa un fondo = fondo de tarima −
  `SelectiveRackDefaults.CabeceraFondoAllowance` (6"). Asi una tarima de 48" ("Fondo de tarima") da una
  cabecera de 42". TODA la geometria (offsets, lateral, planta y la vista previa de fondo) usa el fondo de
  CABECERA (`SelectiveDepthLayout.CabeceraDepthOfFondo`); "Fondo de tarima" sigue siendo la tarima. La
  cabecera personalizada por poste tambien obedece esta regla (su fondo ya no se fija aparte).
- **Override de fondo de cabecera por linea**: campo opcional por fondo "Fondo de cabecera (in, opcional)".
  Vacio = derivado por la regla; un valor fuerza ese fondo de cabecera en esa linea. Diseno
  `CabeceraFondoOverrides`; sistema `FondoCabeceraOverrides`. `CabeceraDepthOfFondo` = el override cuando
  es > 0, si no la regla.
- **Frontal por fondo**: se puede insertar una **frontal por cada fondo** (cada cara espalda-con-espalda
  con su propia elevacion). El bloque frontal lleva su fondo en el campo `Section` del sobre; al insertar
  se pregunta el numero de fondo solo si hay 2+ fondos, y `RACKEDITAR` redibuja cada frontal en su fondo.
  Comando `RACKSELECTIVO` (frontal) via `InsertSelectiveFrontal` (`RackFrameCommands.Selective.cs`); el
  dibujo sale de `SelectiveDepthLayout.FondoSystemView`.
- **Vista previa de lateral en el editor**: `RackSelectiveWindow` tiene un toggle **Frontal/Lateral** sobre
  la vista previa. La lateral es una vista de costado **esquematica**: cada fondo se dibuja como su cabecera
  (poste frente + poste atras a su propio fondo/altura) con una celosia en zigzag y marcas de larguero. El
  patron real de celosia sigue apareciendo solo en la vista lateral insertada.
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
`flow-bed-profiles`. Excel-first: el `.csv` gana sobre el `.json`,
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
- ✅ BOM consolidado multi-rack: comando `RACKBOMTOTAL` (desglose por rack x copias + gran total
  por componente, `RackConsolidatedBomWindow`, export CSV).
- ✅ Export a Excel (.xlsx) desde las ventanas de BOM y consolidado (`XlsxWriter` propio, OOXML sin
  dependencias NuGet).
- Pendiente: integrar con el archivo cotizador existente.
- Validaciones de ingenieria: seguir agregando reglas (capacidad de carga, holguras) de forma gradual.

Selectivo (doble profundidad):

- ✅ Fase 1 (espalda con espalda): N fondos con niveles y numero de frentes por fondo, rejilla
  horizontal compartida definida por el **fondo mas largo** (los mas cortos son un prefijo; layout
  en esquina), separadores por hueco y BOM que suma el contenido real de cada fondo. Ahora tambien: **fondo
  de tarima por fondo**, la regla **cabecera = tarima − 6"** (con override por linea), **frontal por
  fondo** y **vista previa lateral** (toggle Frontal/Lateral) en el editor.
- ✅ Fase 2, **"medio frente"** (generalizado a N tramos): un frente se parte en N tramos con N-1
  postes intermedios (solo de ese fondo, asi los postes compartidos siguen alineados) y el ultimo
  tramo calculado; si los tramos no caben se dibuja el frente completo. `SelectiveMedioFrente.Resolve`
  centraliza el layout (frontal y planta consumen el mismo helper), boton "Medio frente..." por
  frente abre `SelectiveSegmentsWindow` y el round-trip va via `SelectiveSegmentDocument`
  (+ fallback del `MedioFrenteLength` legacy).
- ✅ El bloque separador fisico entre fondos ya se dibuja en lateral y planta y entra al BOM (componente
  "Separador"); en la frontal solo se deja el hueco, a proposito.

## Como agregar un tipo de rack nuevo

Patron reutilizable ya probado con los cuatro tipos:

1. Definir su `Kind` en `RackEmbedDocument`.
2. Agregar `Edit<Kind>` en `RackFrameCommands` (despacho de `RACKEDITAR`).
3. Implementar `LoadExisting` en su ventana editora.
4. Implementar embed + redibujo en sitio en su draw service.
