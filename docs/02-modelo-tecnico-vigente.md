# Modelo tecnico vigente

## Vision general

RackCad es un plugin de AutoCAD (.NET `net8.0-windows`, WPF) para disenar **y dibujar**
racks. Ya no es solo un configurador de cabeceras: hoy maneja **cuatro tipos de rack**, cada
uno con su ventana editora, su dibujo real en AutoCAD y round-trip de edicion.

| Tipo | Ventana editora | Vista | Comandos |
| --- | --- | --- | --- |
| Cabecera (marco) | `RackFrameConfiguratorWindow` | Lateral | `RACKCABECERA`, `RACKCABECERALATERAL`, `QUICKCABECERA` |
| Sistema dinamico (pallet flow) | `RackDynamicSystemWindow` | Lateral | `RACKSISTEMADINAMICO` |
| Cama de rodamiento (flow bed) | `RackFlowBedWindow` | Lateral | `QUICKCAMA` |
| Selectivo (editor avanzado) | `RackSelectiveWindow` | Frontal | `RACKSELECTIVO` |

- Menu principal (`[CommandMethod("RACKCAD")]`, `RackMainMenuWindow`): punto de entrada donde
  se elige que disenar e insertar.
- Edicion de lo ya dibujado: `[CommandMethod("RACKEDITAR")]` (ver "Identidad y round-trip").
- Los comandos viven en `RackFrameCommands`; el dibujo CAD en los `*DrawService` /
  `*Drawer` de `RackCad.Plugin`.

Las dos secciones siguientes describen los dos modelos con mas contenido tecnico: el de la
**cabecera** (horizontales como fuente de verdad) y el **motor del selectivo**
(design -> resolver -> resolved -> builder). Al final, la mecanica de **identidad y
round-trip** compartida por los cuatro tipos.

## Cabecera: las horizontales son la fuente de verdad

Un marco (cabecera) = 2 postes + placas base + celosia. Su modelo se rige por un principio:

Las horizontales son la fuente de verdad. Los paneles no son entidades libres: un panel es el
espacio entre dos horizontales consecutivas.

```text
Horizontales ordenadas por elevacion
        ->
Paneles consecutivos derivados
        ->
Miembros fisicos para vista previa y dibujo/BOM
```

El PERALTE de la placa base es editable por placa en el configurador
(`BasePlatePlacement.PeralteOverride`; `null` = derivado del poste con
`StandardPeralte = PeralteBase + PeraltePorPeraltePoste * PostPeralte`). En modo
"Configuracion rapida", "Insertar" genera y coloca el marco en un clic.

### Horizontales

Clase principal: `FrameHorizontal`.

Campos relevantes:

- `Id`: identificador secuencial por elevacion (`H1`, `H2`, `H3`...).
- `Number`: numero visual.
- `Elevation`: elevacion fisica.
- `ProfileId`: perfil/catalogo.
- `Quantity`: cantidad fisica en esa elevacion.
- `MountingFace`: `Front`, `Back`, `Both`.
- `State`: `Standard`, `Manual`, `Rule`, `Exception`.
- `Notes`.
- `IsStandard`.

Regla actual:

1. Ordenar horizontales por `Elevation`.
2. Renombrar secuencialmente:
   - menor elevacion = `H1`;
   - siguiente = `H2`;
   - siguiente = `H3`.
3. Reconstruir paneles desde esas horizontales ya renombradas.

### Paneles

Clase principal: `BracingPanel`.

Campos relevantes:

- `PanelId`: `P1`, `P2`, `P3`.
- `Number`.
- `LowerHorizontalId`.
- `UpperHorizontalId`.
- `StartElevation`.
- `EndElevation`.
- `ClearHeight`: derivado de `EndElevation - StartElevation`.
- `Arrangement`: arreglo de panel.
- `MountingFace`: cara de montaje.
- `DiagonalProfileId`.
- `DiagonalDirection`.
- `StartConnectionPointId`.
- `EndConnectionPointId`.
- `IsStandard`.
- `IsException`.
- `Members`: diagonales fisicas derivadas del panel.

Los paneles se regeneran completamente despues de operaciones que cambian horizontales:

- agregar;
- eliminar;
- duplicar;
- mover;
- dividir panel;
- combinar paneles.

### Arreglos de panel

Enum actual: `BracingPattern`.

Valores:

- `NoBracing`
- `SingleDiagonal`
- `DoubleDiagonal`
- `XBracing`
- `KBracing`
- `Custom`

Notas:

- `DoubleDiagonal` no es X.
- `XBracing` es cruce.
- `NoBracing` conserva horizontales pero no genera diagonales.

### Direccion diagonal

Enum actual: `DiagonalDirection`.

Valores:

- `AutoAlternating`
- `UpRight`
- `UpLeft`

`AutoAlternating` alterna por numero de panel:

```text
P1 = /
P2 = \
P3 = /
P4 = \
```

### Miembros fisicos

Clase principal: `FrameMember`.

Los genera `BracingPanelMemberBuilder`.

Actualmente se generan para:

- horizontales;
- diagonales de panel.

Estos miembros alimentan la vista previa WPF y son la base para el dibujo AutoCAD, el BOM,
las validaciones fisicas y los metadatos.

### Invariantes del modelo

Siempre debe cumplirse:

```text
Paneles = funcion(Horizontales)
```

Si las horizontales son:

```text
H1 = 0
H2 = 44
H3 = 88
H4 = 132
```

Entonces los paneles validos son:

```text
P1 = H1-H2
P2 = H2-H3
P3 = H3-H4
```

No debe existir:

```text
P2 = H2-H4
```

si `H3` existe.

### Excepciones

Las excepciones se recalculan desde filas editoras:

- `HorizontalEditorRow`.
- `BracingSegmentEditorRow`.
- propiedades de postes y placas.

La restauracion de cabecera estandar limpia todas las excepciones porque reconstruye el
modelo desde un snapshot inicial limpio.

## Motor del selectivo: design -> resolver -> resolved -> builder

El editor avanzado del selectivo (`RackSelectiveWindow`, comando `RACKSELECTIVO`) es
**pallet-driven**: el usuario ya no teclea longitudes de larguero, separaciones ni alturas.
Describe una matriz **FRENTES x niveles** (el termino en la UI es "frente", no "bahia"), y la
geometria se deriva. El flujo tiene cuatro etapas:

```text
design  (SelectivePalletDesign)   -> lo que edita el usuario (tarimas por celda)
   -> resolver (SelectiveGeometryResolver)  -> aplica las reglas de geometria
      -> resolved (SelectiveRackSystem)     -> geometria ya calculada, sin AutoCAD
         -> builder (SelectiveFrontalBuilder + SelectiveFrontalDrawService) -> dibujo CAD
```

### design: `SelectivePalletDesign`

- `Bays` (columnas = frentes), cada una un `SelectiveBayDesign` con su lista de
  `SelectiveCell` (niveles, abajo->arriba).
- Cada celda (`SelectiveCell`) = tarima (`Tarima` con `Frente`/`Alto`), `PalletCount`
  (tarimas por nivel), `BeamId` (larguero) y `BeamPeralte` (peralte de larguero, elegido de
  la lista "peraltes" del catalogo).
- Parametros del run: `PostId`, `PostPeralte`, `PalletTolerance` (holgura horizontal, def. 4"),
  `VerticalClearance` (holgura vertical, def. 6"), `FloorBeamRise` (def. 4").
- **Overrides manuales opcionales** (vacio = auto): `BeamLengthOverride` (longitud de larguero
  por celda), `ClearOverride` (claro/separacion por celda), `HeightOverride` (altura por
  frente), `FloorBeam` (larguero a piso por frente).
- `PostCabeceras`: cabecera opcional por poste (`RackFrameConfiguration` embebida; N frentes ->
  N+1 postes). De ahi sale la placa/peralte de cada poste; en frontal el poste es esa cabecera
  vista de canto, y es la base para la futura vista lateral.

### resolver: `SelectiveGeometryResolver`

Puro (sin AutoCAD; solo lee del catalogo la base de la rejilla de troquel). Aplica las reglas:

1. **Larguero**: `LONGITUD = Frente*Count + Tolerance*(Count+1)`; por frente gobierna el nivel
   mas ancho (todos los largueros de un frente comparten longitud = separacion entre postes).
2. **Separacion** entre niveles: `roundUpTroquel( roundUpEven(Alto + Clearance) + peralte(larguero de arriba) )`.
3. **Piso**: el nivel 0 es el suelo, datum en `Y=0`. Por defecto no lleva larguero (la tarima
   descansa en el piso y el primer larguero salta a la rejilla de arriba); con "larguero a piso"
   el nivel 0 recibe larguero al troquel mas bajo (elevado `FloorBeamRise`).
4. **Altura de la cabecera**: `roundUpFoot( superficieDeCarga + altoTarimaSuperior/3 )`, medida
   desde el escalon del larguero superior; el frente mas alto gobierna un poste compartido.
5. **Redondeos**: par (paso de troquel 2") hacia arriba y pie (12") hacia arriba.

### resolved: `SelectiveRackSystem`

Geometria ya calculada que consume el builder: `Bays` (`SelectiveBay` con `BeamLength`,
`Height` y `Levels`), cada `SelectiveLevel` con su `Y` ya snappeada al troquel, `BeamId` y
`BeamPeralte`; mas `PostCabeceras` pasadas tal cual. N frentes -> N+1 cabeceras (cada una un
poste en frontal).

### builder + BOM

- `SelectiveFrontalBuilder` produce las instancias de bloque; `SelectiveFrontalDrawService` las
  dibuja/coloca en AutoCAD (vista **frontal**).
- BOM: `SelectiveBomBuilder` (postes por altura, una placa por poste, largueros por
  longitud+peralte, dos mensulas por larguero) mostrado en `RackBomWindow` (grid + export CSV).

**Pendiente (Fase 5):** vista **lateral** del selectivo: cada poste desplegado como su cabecera
completa, enlazado por el mismo GUID.

## Identidad y round-trip (los cuatro tipos)

Misma logica reutilizable para cabecera, dinamico, cama y selectivo:

- Cada rack dibujado = **una** definicion de bloque; las copias son referencias a ella.
- En la **definicion** del bloque (diccionario de extension) se embebe un sobre unificado
  `RackEmbedDocument { SchemaVersion, Kind, Id (GUID), Name, Design (JSON del diseno) }`,
  serializado por `RackEmbedStore` y guardado como `Xrecord` troceado en cadenas <= 255 chars
  (`RackBlockData`). `Kind` es `"selective"`, `"dynamic"`, `"cabecera"` o `"cama"`.
- El JSON del diseno lo produce el store propio de cada tipo: `SelectivePalletDesignStore`
  (selectivo), `RackProjectStore` (dinamico/cabecera; persistencia de proyecto en
  `.rackcad.json`), `FlowBedConfigurationStore` (cama). Los `Build*Payload` de
  `RackFrameCommands` envuelven ese JSON en el sobre.
- **`RACKEDITAR`**: seleccionas un rack -> lee el sobre desde la definicion del bloque ->
  despacha por `Kind` (`EditSelective` / `EditDynamic` / `EditCabecera` / `EditCama`) -> reabre
  el editor correcto precargado (`LoadExisting`) -> al confirmar redefine la definicion en sitio
  (`RedrawInPlace` -> `RedefineSystemBlock` + `Regen`) => todas las copias se actualizan a la
  vez, ninguna se mueve ni se recoloca.
- El nombre ("Rack A", campo en cada editor) es el nombre del bloque; el GUID va en el sobre
  (evita colisiones si dos racks comparten nombre).
- **Escalable**: agregar un tipo nuevo = definir su `Kind` + `Edit<Kind>` en `RackFrameCommands`
  + `LoadExisting` en su ventana + embed/`RedrawInPlace` en su draw service.

## Catalogos

`assets/catalogs/*.csv` (+ algunos `.json`), cargados por `JsonRackCatalogProvider` a
`RackCatalog`:

- `post-profiles` (postes; los refuerzos son postes).
- `truss-profiles` (una sola lista de celosia = horizontales + diagonales).
- `beam-profiles` (largueros; columna "peraltes" = valores permitidos, FK a mensula).
- `mensulas`.
- `base-plates` (`peralteBase` / `peraltePorPeraltePoste` -> `StandardPeralte`).
- `connection-points` + `connection-layout` (puntos de conexion parametricos:
  `X = localX + slope*param`).
- `blocks` (bloque por pieza y vista), `views`, `flow-bed-profiles`, `spacers-profiles`.

Persistencia de proyecto: `RackProjectStore` -> archivo `.rackcad.json`.
