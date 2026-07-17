# Modelo tecnico vigente

## Vision general

RackCad es un plugin de AutoCAD (.NET `net8.0-windows`, WPF) para disenar **y dibujar**
racks. Ya no es solo un configurador de cabeceras: hoy maneja **cuatro tipos de rack**, cada
uno con su ventana editora, su dibujo real en AutoCAD y round-trip de edicion.

| Tipo | Ventana editora | Vista | Comandos |
| --- | --- | --- | --- |
| Cabecera (marco) | `RackFrameConfiguratorWindow` | Lateral + Planta | `RACKCABECERA`, `QUICKCABECERA` |
| Sistema dinamico (pallet flow) | `RackDynamicSystemWindow` | Lateral + Frontal salida/entrada + Planta | `RACKSISTEMADINAMICO` |
| Cama de rodamiento (flow bed) | `RackFlowBedWindow` | Lateral | `QUICKCAMA` |
| Selectivo (editor avanzado) | `RackSelectiveWindow` | Frontal + Lateral + Planta | `RACKSELECTIVO` |

El dinamico separa `DynamicRackDesign` (intencion editable, sin coordenadas) de `DynamicRackSystem`
(representacion resuelta). `DynamicRackSystemResolver` valida y materializa el layout; UI, biblioteca y
payload DWG persisten el diseno. `DynamicFrontGeometry` resuelve frentes con distinto numero de posiciones y niveles:
calcula `BFR = frenteTarima + 2"`, largo IN/OUT `= BFR * posiciones + 6"` (o su override manual) y comparte la reticula de postes entre `DynamicSystemFrontalBuilder` y
`DynamicSystemPlantaBuilder`. `PostPeralte` es global al rack y el resolver lo impone sobre todas las cabeceras,
incluidas las personalizadas; si falta en un documento legacy, hereda el ancho catalogado del poste.
`DynamicDepthGeometry` resuelve `PalletsDeep` y `DepthStartPosition` por frente. El rango mas corto define los dos
modulos con `+6"` y el patron estructural base; todos los rangos mayores deben contenerlo y solo prolongan el patron.
Por eso un extremo de frente puede ser separador: se conserva como separador y se agrega el poste limite sencillo.
El helper tambien entrega el rango por linea transversal y las coordenadas `StartX/EndX` que consumen dibujo, camas,
seguridad y BOM. `DynamicRackLevelGeometry` resuelve la celda frente x nivel: tarima, claro, tipo/peralte de IN/OUT,
tipo/peralte intermedio y largo manual. El frente conserva posiciones, niveles, fondos, inicio longitudinal e inicio
del primer larguero; su largo fisico es la envolvente maxima de sus celdas. Los campos nuevos del DTO son nullable:
documentos legacy heredan los valores globales, el fondo global y el inicio 1.
`DynamicLoadBeamGeometry` resuelve una pareja IN/OUT por
nivel y frente: perfil fijo C6 desde catalogo, salida en `StartX`, entrada elevada por la pendiente propia en `EndX`
y bloque de entrada espejeado en X. Las elevaciones nominales de salida y entrada se ajustan
al `TROQUEL_LARGUERO` mas cercano (base catalogada + paso de 2"); la altura comercial se recalcula desde esos mates
reales. `DynamicFlowBedLateralBuilder` compone la cama completa
existente por nivel: hace mate `TROQUEL_IN` (riel) -> `TROQUEL_CAMA` (larguero de salida), gira el conjunto con la
pendiente resuelta y reutiliza una definicion anidada entre niveles. `DynamicFlowBedGeometry` fija ademas la regla
comercial unica `LONGITUD = (EndX - StartX) - 4"`; los offsets de catalogo y la diagonal no cambian ese corte. El
helper conserva tanto la linea de troqueles como la linea paralela del **origen del riel**. `DynamicIntermediateBeamGeometry`
enumera todos los limites internos entre modulos y excluye los dos extremos IN/OUT;
`DynamicIntermediateBeamLateralBuilder` coloca exactamente un `LARGUERO_ESCALON_INFINITO` por posicion y nivel.
Un segundo poste usa `INICIO_DERECHO` y espejo; un primer poste usa `INICIO_IZQUIERDO` sin espejo; el derivado
reforzado central cuenta como una posicion y conserva solo el normal del perfil principal. El mate superior se eleva hasta
la linea del origen del riel, no hasta `TROQUEL_IN/TROQUEL_CAMA`. Los dos cortes frontales usan las elevaciones de
salida/entrada y solo dibujan IN/OUT; la planta repite las cabeceras longitudinales en cada limite transversal y omite
las camas. El refuerzo derivado de planta conserva la misma linea transversal del poste principal: este termina en
el limite `FIN_POSTE` y el segundo perfil/placa comienza ahi, consecutivo sobre X y sin espejo.
Todas las vistas comparten GUID y se redibujan con un solo `Regen`.
`DynamicDerivedPostGeometry` centra los postes intermedios reforzados haciendo coincidir el limite entre separadores
con `FIN_POSTE`, que es la interfaz fisica entre el poste principal y su refuerzo.
`DynamicSafetyLateralBuilder` compone el corte longitudinal y `DynamicSafetyMultiViewBuilder` proyecta la misma
seleccion sobre frontal y planta. Toman los origenes reales de los postes/placas, colocan BOTA, dejan que LATERAL
sustituya esas botas, proyectan DESVIADOR segun los niveles reales y agregan DEFENSA con longitudes independientes
de salida/entrada por poste mediante `DynamicForkliftDefensePlan`. `DynamicEntranceGuidePlan` coloca GUIA por
frente/nivel en la entrada, 8" sobre el IN/OUT, con una pareja espejeada cuya `LONGITUD` es el ultimo tramo
longitudinal ocupado por ese frente. `DynamicRackDesign` persiste las
mismas `SelectiveSafetySelection` mediante `SafetySelectionDocument`, con lista nula como fallback legacy vacio.
`DynamicViewDecorations` centraliza numeracion, nombre y cotas de las vistas ligadas; la cota del corte IN/OUT arranca
en el `INICIO_PERFIL` catalogado, no en el troquel del poste. Detalle, escala y estilo de cota forman parte del diseno versionado.
`DynamicIntermediateBeamGeometry.ResolvePeraltes` normaliza un peralte catalogado por frente y nivel. Cada lateral
por poste envia a cada apoyo el maximo de sus frentes adyacentes activos en ese nivel; la planta usa el maximo de los niveles
del frente dibujado. La lista nullable de cada frente acepta la lista comun de la version anterior y cae a 3.5" por
nivel en documentos mas antiguos.
`DynamicRackSystemResolver` calcula la altura comercial requerida por cada frente. `DynamicFrontGeometry.PostHeight`
aplica al poste el maximo de sus frentes adyacentes, igual que el contrato del selectivo. `DynamicSystemLateralBuilder.Cortes`
produce N+1 cortes para N frentes; cada bloque lateral persiste `Section=postIndex`, se inserta eligiendo numero de poste y
se redibuja con su propia altura/niveles. Un payload dinamico legacy sin seccion se migra al poste 0 al actualizar.
`DynamicSystemPreviewGeometry` expone a WPF las mismas alturas adyacentes, rango longitudinal y plan lateral puro.
Por eso la preliminar lateral no vuelve a dibujar el largo global y los frontales no inflan todos los postes a la
altura maxima. La seleccion WPF es una celda frente x nivel; Celda/Nivel/Frente/Todas replica solo tarima, claro y
ambos largueros. Posiciones, niveles, fondos, inicio longitudinal e inicio del primer larguero se editan aparte y
pertenecen exclusivamente al frente seleccionado. Ctrl + clic conserva una seleccion multiple de celdas; el editor
puede aplicar la celda a las seleccionadas y los datos estructurales a sus frentes distintos o a todos los frentes.

Las vistas extra (laterales por poste/planta) estan ligadas por el mismo GUID del sistema y solo se
insertan desde `RACKEDITAR` sobre una vista ya dibujada (los botones se deshabilitan con
tooltip si no aplica), asi nunca quedan huerfanas. `RACKEDITAR` sobre cualquier vista reabre
el editor del sistema completo y al confirmar redibuja todas las vistas (encontradas por GUID
escaneando las definiciones de bloque).

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
         -> builders (frontal / lateral / planta + SelectiveFrontalDrawService) -> dibujo CAD
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
  vista de canto, y es la base de los cortes de la vista lateral. "Personalizar" siembra la
  cabecera resuelta (alto del poste + fondo del tramo bloqueado; warning si cambias el alto;
  cancelable).
- **Doble profundidad (espalda con espalda, Fase 1)**: `DepthCount` (numero de fondos, 1..4;
  1 = sencillo clasico) a lo largo del eje de fondo, y `SeparatorLengths` (**una por hueco** entre
  fondos consecutivos, no un valor global; el bloque separador fisico ya se dibuja en las vistas lateral
  y planta y entra al BOM (componente "Separador"); en la frontal solo se deja el hueco vacio, a
  proposito). `ExtraFondoBays` guarda la matriz de niveles de los fondos 1..N-1 (vacio = ese
  fondo hereda las `Bays` del fondo 0). **Cada fondo tiene sus propios niveles/alturas Y su propio
  numero de frentes** (layout en esquina: p. ej. una linea con 3 frentes y otra con 6, unidas solo en
  los 3 que se traslapan). El **fondo MAS LARGO define la rejilla horizontal compartida** (anchos de
  frente -> posicion de postes); cada fondo mas corto es un **prefijo** de esa rejilla, asi los postes
  que se traslapan alinean y una linea mas larga simplemente se extiende. Si el frente maestro de un
  indice es una **columna vacia** (sin niveles, ancho 0), el ancho lo da la bahia real mas ancha de los
  otros fondos en ese indice (una columna nunca colapsa la rejilla); un frente sin niveles = **columna
  vacia** (no dibuja larguero). Ademas cada fondo tiene su **propio fondo de tarima**: `ExtraFondoDepths` (fondos 1..N-1;
  vacio = el del fondo 0). El **fondo de cabecera** (marco dibujado) = fondo de tarima −
  `SelectiveRackDefaults.CabeceraFondoAllowance` (6"); `CabeceraFondoOverrides` fuerza, por linea, un
  fondo de cabecera concreto (vacio = la regla). Defaults del run: frente de tarima 42, separacion
  entre fondos 12. El helper puro `SelectiveDepthLayout` calcula offsets/separadores por fondo
  (`BaysOfFondo`, `FondoSystemView`) y el fondo de cabecera de cada fondo
  (`CabeceraDepthOfFondo` = override cuando > 0, si no la regla tarima − 6"); toda la geometria (offsets,
  lateral, planta, vista previa) usa el fondo de CABECERA, no el de la tarima.

### resolver: `SelectiveGeometryResolver`

Puro (sin AutoCAD; solo lee del catalogo la base de la rejilla de troquel). Aplica las reglas:

1. **Larguero**: `LONGITUD = Frente*Count + Tolerance*(Count+1)`; por frente gobierna el nivel
   mas ancho (todos los largueros de un frente comparten longitud = separacion entre postes,
   y la mensula de ese nivel gobierna el espaciado de postes).
2. **Separacion** entre niveles: `roundUpTroquel( roundUpEven(Alto + Clearance) + peralte(larguero de arriba) )`.
3. **Piso**: el nivel 0 es el suelo, datum en `Y=0`. Por defecto no lleva larguero (la tarima
   descansa en el piso y el primer larguero salta a la rejilla de arriba); con "larguero a piso"
   el nivel 0 recibe larguero al troquel mas bajo (elevado `FloorBeamRise`, snappeado al paso
   de troquel).
4. **Altura de la cabecera**: `roundUpFoot( superficieDeCarga + altoTarimaSuperior/3 )`, medida
   desde el escalon del larguero superior; el frente mas alto gobierna un poste compartido.
5. **Redondeos**: par (paso de troquel 2") hacia arriba y pie (12") hacia arriba.

### resolved: `SelectiveRackSystem`

Geometria ya calculada que consume el builder: `Bays` (`SelectiveBay` con `BeamLength`,
`Height` y `Levels`), cada `SelectiveLevel` con su `Y` ya snappeada al troquel, `BeamId` y
`BeamPeralte`; mas `PostCabeceras` pasadas tal cual. N frentes -> N+1 cabeceras (cada una un
poste en frontal). En doble profundidad expone ademas `FondoBays` (las bahias ya resueltas de
cada fondo, cada uno con su PROPIO numero de frentes; el fondo mas largo define la rejilla y los mas
cortos son un prefijo de ella — `SelectiveDepthLayout.MasterFondoIndex` / `MasterGrid`), `FondoDepths`
(el fondo de tarima de cada fondo) y `FondoCabeceraOverrides` (el override de fondo de cabecera por
linea; vacio = derivado por la regla tarima − 6"). Cada `SelectiveBay` lleva ademas `Segments` (los
tramos del "medio frente", ver mas abajo).

### builder + BOM

- `SelectiveFrontalBuilder` produce las instancias de bloque; `SelectiveFrontalDrawService` las
  dibuja/coloca en AutoCAD. El selectivo tiene **tres vistas** ligadas por GUID:
  - **frontal**: un bloque (postes + placas + largueros por nivel);
  - **lateral**: cortes, **un bloque por poste** — cada corte es la cabecera del poste en
    perfil + las secciones de largueros frente/atras por nivel; al insertar se pregunta que
    corte por numero de poste y se coloca con jig;
  - **planta**: un bloque (una cabecera-planta por frente apilada en Y + largueros
    frente/atras por bahia a lo largo de Y; X = fondo, Y = frente).
- **BOM por COMPONENTES**: `BomLine` (pieza) + `BomComponent` (sub-ensamble con `Pieces` = receta por
  unidad); `BillOfMaterials` tiene dos constructores (lineas planas / componentes) y en el modo componente
  `Lines` es el total de piezas aplanado (`FlattenToPieceTotals`, x cantidad). Los BOM de SISTEMA listan
  componentes: el **selectivo** = **cabeceras** (una por posicion de marco por fondo + intermedias de medio
  frente, via `BomBuilder.Components` — marcos identicos por receta colapsan; incluye SU celosia, materializada
  con `RefreshPhysicalModel` si viene de persistencia) + **largueros** (perfil + 2 mensulas, receta unica en
  `LargueroBomBuilder.Component`); el **dinamico** = cabeceras repetidas por cada linea transversal de postes +
  apoyos derivados (identificados como reforzados cuando llevan dos perfiles/placas) + separadores por corte/nivel +
  largueros IN/OUT por frente/nivel/extremo + intermedios agrupados por longitud/peralte + una unidad de componente
  **Cama** por posicion y nivel + botas/laterales/desviadores/defensas/guias de entrada. BOTA/LATERAL/DEFENSA se
  cuentan desde planta (un lateral sustituye ambas botas de su linea), DESVIADOR desde la union fisica de salida y
  entrada, y GUIA solo desde la frontal de entrada para conservar sus dos piezas fisicas por celda frente x nivel sin
  duplicar proyecciones. Cada `Cama` muestra longitud y BFR y no tiene receta interna; el IN/OUT
  es el bloque completo y no agrega una mensula separada. La cabecera y la cama
  standalone siguen siendo BOM de piezas (ellas SON el componente). `RackBomWindow` muestra arbol
  (fila por componente -> RowDetails con sus piezas) o grid plano segun `IsComponentBased`; CSV a dos niveles
  (fila `Componente` + filas `Pieza` con total = por-unidad x cantidad), CRLF RFC-4180.
- **BOM consolidado del dibujo**: comando `RACKBOMTOTAL` (`RackFrameCommands.BomTotal.cs`) — escanea las
  definiciones de bloque como RACKLISTA, toma UN representante por GUID (toda vista lleva el diseno completo),
  copias = MAX de referencias entre las vistas del rack (0 colocadas = se salta: definicion sin purgar),
  reconstruye el BOM por tipo (selectivo->resolver, dinamico, cabecera, cama->`FlowBedLateralBuilder`) y
  muestra `RackConsolidatedBomWindow`: desglose por rack (doble clic abre su BOM) + **gran total POR
  COMPONENTE** (`ConsolidatedBomBuilder` fusiona componentes identicos x copias; una cama/cabecera suelta
  cuenta como UN componente de su tipo) + export CSV (`ConsolidatedBomCsvExporter`).
- **Larguero como componente con editor**: `LargueroDesign` (perfil + peralte + longitud a corte + mensula
  override; SOLO visual/BOM — aun sin bloque de AutoCAD) editado en `RackLargueroWindow` (menu "Disenar
  larguero": esquema + BOM + guardar en biblioteca).
- **Biblioteca de disenos ampliada**: `RackSystemKind` gano `SelectiveRack`/`Cama`/`Larguero`;
  `RackProject`/`RackProjectDocument`/`RackProjectStore` cargan esos payloads con round-trip probado, y
  `RackDesignLibrary` los lista. El selectivo y la cama tienen boton **"Guardar en biblioteca"**, y "Abrir de
  la biblioteca" abre el editor correcto: selectivo/cama precargados como rack NUEVO (`LoadForNew`, GUID
  fresco al insertar), larguero con `LoadExisting`.
- **Doble profundidad (Fase 1)**: la **frontal se puede insertar por fondo** (cada cara espalda-con-espalda
  con su propia elevacion); el bloque frontal lleva su numero de fondo en el campo `Section` del sobre, al
  insertar se pregunta el fondo solo si hay 2+ fondos, y `RACKEDITAR` redibuja cada frontal en su fondo
  (`SelectiveDepthLayout.FondoSystemView`; comando `RACKSELECTIVO` / `InsertSelectiveFrontal` en
  `RackFrameCommands.Selective.cs`). **Lateral y planta recorren TODOS los fondos** (cada uno con su fondo
  de cabecera propio y sus largueros, separados por el hueco de `SeparatorLengths`). El **BOM suma el
  contenido real de CADA fondo x2** (frente/atras), no un multiplicador plano por numero de fondos. La
  persistencia hace round-trip de `ExtraFondoBays`, `ExtraFondoDepths`, `CabeceraFondoOverrides` y los
  `Segments` por frente (los disenos legacy sin `DepthCount` caen a un solo fondo).
- **Frentes por fondo (layout en esquina)**: cada fondo tiene su **propio numero de frentes**; el fondo
  mas largo es el maestro de la rejilla y los mas cortos son un prefijo. La **planta** dibuja cada linea
  con su tramo real (cada fondo coloca sus marcos + largueros hasta SU conteo, sobre la rejilla maestra);
  la **lateral** genera un corte por poste del maestro y cada corte lejano se **ancla al primer fondo que
  llega** a ese frente (asi siempre hay cabecera primaria valida aunque el fondo 0 no alcance). En la UI,
  el numero de frentes se edita **por fondo** (`BayCountBox` habilitado en cualquier fondo).
- **Medio frente (generalizado a N tramos)**: un frente se parte en **N tramos** con N-1 **postes
  intermedios** (solo de ese fondo, asi los fondos siguen alineados en los postes compartidos). Cada
  `SelectiveSegment` lleva `Length` + `Loaded`; el **ultimo tramo es calculado** (el resto de la bahia,
  descontando lo que consume cada poste intermedio). Marcar que tramos cargan largueros permite amarrar
  un lado, el otro o ambos (modulo intermedio). `SelectiveMedioFrente.Resolve(bay, troquelX, inicioX)`
  centraliza el layout y devuelve null cuando no hay tramos o **no caben** (se dibuja el frente completo
  — esa es la validacion "el larguero especial no puede exceder el completo"). Frontal y planta consumen
  el mismo helper, asi postes/cabeceras intermedias coinciden entre vistas. UI: boton "Medio frente..."
  por frente abre `SelectiveSegmentsWindow` (lista de tramos, el ultimo "(resto)"). Round-trip via
  `SelectiveSegmentDocument` (+ fallback del `MedioFrenteLength` escalar viejo).

La cabecera, por su parte, tiene **dos vistas** ligadas por GUID: lateral y planta (planta =
2 huellas de poste, frente en 0 / atras en fondo, + placas + celosia colapsada a un miembro
con longitud = A-corte del travesano = fondo - 2*(inset troquel - mensula), p.ej. 38.8125
para fondo 42; peralte de la celosia = peralte del poste - 1").

## Identidad y round-trip (los cuatro tipos)

Misma logica reutilizable para cabecera, dinamico, cama y selectivo:

- Cada rack dibujado = **una** definicion de bloque; las copias son referencias a ella.
- En la **definicion** del bloque (diccionario de extension) se embebe un sobre unificado
  `RackEmbedDocument { SchemaVersion, Kind, View, Section, Id (GUID), Name, Design (JSON del diseno) }`,
  serializado por `RackEmbedStore` y guardado como `Xrecord` troceado en cadenas <= 255 chars
  (`RackBlockData`). `Kind` es `"selective"`, `"dynamic"`, `"cabecera"` o `"cama"`; `View` es
  `"frontal"`, `"lateral"` o `"planta"`; `Section` es el indice de corte/fondo segun la vista (en lateral
  selectivo y dinamico, indice de poste; `-1` = vista no seccionada o legacy).
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

- `secciones` (TODOS los perfiles estructurales en una hoja, columna `rol` = POSTE | CELOSIA | LARGUERO;
  el provider los separa en las tres listas de siempre. Los refuerzos son postes; los largueros llevan
  `peraltes` = valores permitidos y `mensula` = FK).
- `mensulas`.
- `base-plates` (`peralteBase` / `peraltePorPeraltePoste` -> `StandardPeralte`).
- `connection-points` + `connection-layout` (puntos de conexion parametricos en X **y** Y:
  `X = localX + localXPorParam*valor(paramX)`, `Y = localY + localYPorParam*valor(paramY)`;
  esquema `pieceId,connectionPointId,view,localX,localXPorParam,paramX,localY,localYPorParam,paramY`).
- `blocks` (bloque por pieza y vista; `blockName` debe coincidir exacto con el nombre del
  bloque en la libreria DWG — `FindBlock(pieceId, view)` es la ruta activa de los cuatro
  tipos), `views` (que vistas dibuja cada tipo), `flow-bed-profiles`. El perfil del separador de cabecera
  vive en `secciones` con rol `SEPARADOR` (cargado en `RackCatalog.SpacerProfiles`).

Los catalogos son "Excel-first": el `.csv` gana sobre el `.json`, se aceptan UTF-8 y
ANSI/Windows-1252 de Excel, y la cache se invalida por firma de archivos (editar el CSV y
relanzar el comando recarga).

Persistencia de proyecto: `RackProjectStore` -> archivo `.rackcad.json`.
