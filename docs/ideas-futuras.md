# Ideas a futuro y deuda técnica conocida

> Actualizado: 2026-07-17 (incluye separacion futura entre catalogos de producto y overrides de usuario).
> Este documento junta (A) mejoras de producto propuestas y (B) hallazgos de la auditoría que se
> **difirieron a propósito** (necesitan validación en AutoCAD o una decisión de producto). Nada de esto
> está roto hoy; es el backlog recomendado.
>
> **2026-07-09 — hecho:** toda la sección "Limpieza de código (segura pero voluminosa)" quedó aplicada
> (verificada con recon multi-agente + doble check adversarial antes de borrar). Ver commits
> `7031a4e` (código muerto del dominio), `ee99a18` (superficie muerta de header-parameters + validación
> de elevaciones), `c26300d` (handlers muertos del configurador), `cfd6cd3` (partición de
> `RackFrameCommands` en partials por tipo), `70ccca2` (`BlockNaming` puro y testeable) y `b79e677`
> (`PreviewCanvasPainter` compartido).
>
> **2026-07-09 — batch quick wins + higiene + parciales (253 tests verdes):** cerrados los 6 quick wins
> (anotaciones centradas + escala configurable, altura del editor avanzado solo-lectura, matriz del
> selectivo que ya no descarta lo tecleado, combos de enums en español, `FindTreeViewItem` acotado,
> aviso de cabecera más baja que el nivel superior), las 3 deudas de higiene (purga de definiciones
> huérfanas del dinámico, validación de opcionales dinámico/cama, renombrado sincronizado del bloque
> entre vistas) y varias parciales (BOM de la cama, recompose que preserva fondos al cambiar la tarima,
> primer incremento de validación de ingeniería, baseline de cabecera sin placeholder). Se quitó el
> comando `RACKCABECERALATERAL`. Lo tachado abajo quedó HECHO en este batch.
>
> **2026-07-10 — rendimiento + UI/UX + catálogo unificado (267 tests verdes):** (a) editor selectivo fluido
> con 20-30 frentes (matriz sin rebuild por clic, Recompute coalescido, brushes congelados, memos de catálogo);
> (b) la vista PLANTA usa el patrón ARRAY (marcos idénticos = una definición anidada referenciada N veces,
> `SelectivePlantaBuilder.BuildPlan`); (c) el purge de huérfanas ya no paga coste fijo por redibujo (solo
> verifica defs que el contenido nuevo no re-referencia); (d) ~50 mejoras de UI/UX en las 6 ventanas
> (Esc cierra, tooltips, foco inicial, errores visibles, acentos, estilos unificados); (e) `secciones.csv`
> unifica los perfiles (ítem #12). **La agrupación ARRAY en la FRONTAL quedó HECHA (2026-07-11).** **Del mismo
> patrón, ya HECHO:** el patrón ARRAY también cubre los cortes laterales (`LateralHeaderDrawer.CreateSystemBlock`
> agrupa los cortes idénticos en una definición anidada referenciada N veces), antes solo frontal/planta; y
> `BlockLibraryImporter` cachea `blocks-library.dwg` por firma (ruta + fecha de modificación + tamaño),
> reutilizando el `Database` parseado entre dibujos hasta que el archivo cambie.

> **2026-07-11 — rendimiento del selectivo + cotas automáticas (347 tests verdes):** (a) el patrón ARRAY
> ya cubre la FRONTAL (`HeaderInstanceGrouper`, blindado por `Flatten==Build`), no solo la planta; (b) al
> encoger un rack se retiran las vistas fantasma y la purga de definiciones huérfanas usa `Database.Purge`
> post-commit (más barato y correcto); (c) **cotas automáticas por vista HECHO** — ver ítem #1. UX del editor:
> los escalares del tramo aplican al salir del campo y hay aviso de cambios sin aplicar antes de dibujar.

## A. Propuestas de producto

### Vistas y dibujo
1. ~~**Cotas automáticas por vista**~~ — ✅ **HECHO (2026-07-11):** las TRES vistas (frontal/lateral/planta)
   dibujan cotas según un combobox **Cotas** (Ninguna/Mínimo/Estándar/Detallado, persistido) + un combobox
   **Estilo de cota** (tomado de la `DimStyleTable` del dibujo abierto; "(Automático)" = estilo vigente
   escalado por la escala de anotación). `SelectiveDimensions` (puro, `HeaderBlockRole.Dimension`) emite las
   cotas y `LateralHeaderDrawer.AppendDimension` las materializa como `RotatedDimension` en la capa
   **`RACKCAD_COTAS`**. Frontal: alto/ancho totales, largo de CORTE del larguero por frente (desde el inicio
   del perfil, no el troquel), separaciones entre niveles, elevaciones (Detallado). Lateral (por corte): alto,
   fondo por cabecera, separaciones. Planta: largo total, ancho por frente, fondo total, fondo por fondo. El
   `*D` anónimo de cada cota se purga al redibujar. **Dinamico HECHO 2026-07-16:** numeracion, nombre y cotas
   centralizadas para lateral, frontal salida/entrada y planta. Cama independiente sigue diferida.
1b. **Pipeline de TEXTO para los toggles de anotación** — **hecho (frontal, planta y lateral):** existe
   `HeaderBlockRole.Annotation` (+ `Text`/`TextHeight`); un helper compartido `SelectiveAnnotations` emite las
   etiquetas y los tres builders las producen según los flags (frontal: frentes+niveles+nombre; planta:
   frentes+nombre; lateral por corte: niveles+poste+nombre). `LateralHeaderDrawer.AppendInstance` las
   materializa como `DBText` en la capa dedicada **`RACKCAD_ANOTACIONES`** (amarilla), regeneradas en cada
   `RedefineSystemBlock` (no se persisten). **(a) centrado y (b) escala configurable — ✅ HECHO
   (2026-07-09):** el `DBText` se centra con `HorizontalMode`/`VerticalMode` + `AdjustAlignment`, y la
   escala del texto es un campo del selectivo (`AnnotationScale`, persistido). "Dibujar placa base" es un
   toggle real de geometría (frontal/planta).
1c. **Dibujar tarima (toggle) — HECHO parcial** — `TARIMA_GENERICA` se dibuja como referencia visual en
   FRONTAL y LATERAL, incluida la tarima de piso, y nunca entra al BOM. Pendiente: bloque/regla de PLANTA.
2. **Planta del sistema dinámico y de camas** — **dinámico HECHO (2026-07-16):** builder puro, draw service,
   GUID/View/Section, `RACKEDITAR`, cotas y seguridad; por contrato no dibuja las camas. La planta de la cama
   independiente sigue diferida.
3. **Elementos de seguridad** — protector bota H/C, protector lateral H/C, desviador L/C, larguero tope,
   poste tope, guardas traseras, parrillas. **Fase 0 HECHA (2026-07-12, selectivo):** catálogo propio
   `seguridad.csv` (`SafetyElementCatalogEntry` → `RackCatalog.SafetyElements`), selección por cantidad en el
   editor (botón "Elementos de seguridad…" → `SelectiveSafetyWindow`), round-trip en el diseño, y entran al
   **BOM** como un componente "Seguridad". **Fase 1 arrancó (2026-07-12): la BOTA se DIBUJA en la frontal** —
   `HeaderBlockRole.Safety`; `SelectiveFrontalBuilder` coloca cada bota tipo BOTA habilitada (qty>0) en CADA
   poste, con su origen coincidente con el de la placa base (`origin − MONTAJE_POSTE`); bloque por convención
   `<id>_<VISTA>` (`blocks.csv`); el BOM cuenta lo DIBUJADO (no la cantidad manual). **Lado + por-poste + 3 vistas
   HECHO (2026-07-12):** `SafetySide` {None/Left/Right/Both} con `SelectiveSafetySelection.SideForPost(i)` (override
   por poste, si no el lado general). **La bota es un elemento del SISTEMA, no de la cabecera:** Izquierda = poste más
   al FRENTE del sistema (pasillo), Derecha = poste más al FONDO, Ambos = los dos extremos → **2 botas por frente**,
   nunca una por fondo. En planta/lateral se coloca UNA en el poste frontal del sistema y su ESPEJO (reflexión respecto
   al centro del fondo total, por frente sobre los fondos que lo alcanzan) en el trasero; en la frontal los dos extremos
   se traslapan (espejo sobre el origen del poste). Helper compartido `SelectiveSafetyPlacement` (mirrorAxisX null =
   frontal, con valor = reflexión). **BOM:** cada elemento es su PROPIO componente (la bota ES el componente, no un nodo
   "Elementos de seguridad"); el conteo sale de la PLANTA (placement real system-level). Diálogo: combo de lado general
   + "Por poste…". **Ojo:** `FondoSystemView` y el resolver copian `SafetySelections` con sus `PostSides`.
   **Protector lateral HECHO (2026-07-13):** type=LATERAL en seguridad.csv; se coloca IGUAL que la bota (mismo helper
   `SelectiveSafetyPlacement`, mismo espejo al centro del fondo, eje del fondo) pero (a) con LONGITUD = fondo de la
   cabecera (`SelectiveDepthLayout.TotalFondoDepth`/span del frente en planta/lateral), y (b) donde va REEMPLAZA a las
   botas de ese frente (`DrawsAt` → se dibuja el lateral y se omiten las botas). Diálogo: fila LATERAL solo con "Por
   poste…", pre-sembrado con las orillas (primer frente=Izquierda, último=Derecha) la primera vez. **El bloque del
   lateral YA trae la bota espejeada** (una sola pieza que cubre el fondo): se dibuja UN bloque con la longitud, NO dos
   como la bota. Izquierda/Derecha/Ambas = lado de la GUÍA de canal (Derecha = el bloque espejeado; Ambas = guía en los
   dos lados, para un frente-puente). BOM: es su propio componente; los elementos DIBUJABLES (bota/lateral) se cuentan
   SOLO del dibujo (0 = no se listan; una bota totalmente reemplazada por laterales no aparece), la cantidad manual es
   fallback solo para no-dibujables. **Larguero tope y separador HECHO (2026-07-13):** el larguero tope se dibuja en
   las tres vistas (frontal con toggle "Dibujar en frontal", lateral y planta), con su propio componente "Tope" en el
   BOM, rejilla nivel×frente, compartido o uno-por-fondo, lado izq/der/ambos, SAQUE configurable y LONGITUD = larguero
   + ¼" (mate en el punto `TROQUEL_TOPE`); el separador físico entre fondos se dibuja en lateral y planta (componente
   "Separador", cada 100"; en la frontal solo se deja el hueco, a propósito). **Parrilla HECHA en codigo
   (2026-07-14; validada en AutoCAD 2026-07-15):** frontal+lateral+BOM, una por tarima, ancho/cantidad manual
   y conteo vivo; falta PLANTA. **Pendiente:** guardas traseras (prioridad final). Desviadores A/L y poste tope: ✅ HECHOS (2026-07-15, ver HANDOFF §3 y §8).
4. **Layout de almacén** — **v1 HECHO (2026-07-13):** comando `RACKLAYOUT` replica la vista en planta de
   un rack en una rejilla filas × columnas con pasillos + numeración automática (A1, B2…), copias enlazadas
   o independientes; footprint leído de los extents del bloque; alimenta el BOM consolidado. Motor de rejilla
   puro en `RackCad.Application.Layout.WarehouseGridPlanner` (con tests). **Es el prerrequisito del optimizador
   de layout con IA + reglas** (el optimizador decide la rejilla; esto la materializa). **Pendiente v2+:** modelo
   de sitio/envolvente (muros, columnas), orientación frente-a-frente / back-to-back automática, y el optimizador
   (motor de reglas + puntuación beneficio/costo con un agente que propone candidatos). **Cimiento HECHO (2026-07-13):**
   el **modelo de sitio + chequeo de encaje** puro (`RackCad.Application.Layout.WarehouseSite` + `WarehouseFitChecker`,
   con tests): envolvente + columnas/obstáculos + holgura a muros + pasillo mínimo, y un validador de factibilidad
   (dentro de límites, libra obstáculos, pasillos ≥ mínimo) sobre un `WarehouseGridPlan`. Es la mitad "¿es factible?"
   del optimizador; falta la mitad "¿qué tan bueno?" (capacidad/costo) + de dónde sale el sitio (leer muros/columnas
   del dibujo, eso ya tocaría AutoCAD) + el optimizador en sí. El `WarehouseGridPlanner` (2026-07-13) ya soporta
   **hileras back-to-back** (pares que comparten flue, pasillo solo entre pares) y **orientación** (registrada en el
   plan) como ENTRADAS, ofrecidas en el diálogo de `RACKLAYOUT`. **Y el AUTO-RELLENO ya existe (2026-07-13):**
   comando `RACKRELLENAR` — lee el sitio de la capa `RACKCAD_SITIO` (polilínea cerrada = contorno, acepta naves en L
   vía `PolygonGeometry`/`WarehouseSite.FromBoundary`; círculos/rectángulos/bloques = columnas por bbox + holgura),
   calcula la **rejilla máxima que cabe** (`WarehouseAutoFill`: prueba ambas orientaciones, descarta celdas fuera del
   contorno o sobre columnas, opcional back-to-back) y coloca copias enlazadas + etiquetas. Es la primera versión
   determinista del optimizador (maximiza conteo). **Siguiente:** puntuar por beneficio/costo en vez de conteo
   (necesita la mitad "capacidad + costo"), anclajes alternativos de la rejilla (hoy: esquina del bbox), y el agente IA.

### Gestión de racks
5. **`RACKDUPLICAR` — duplicar un rack como uno INDEPENDIENTE** — ✅ **HECHO (2026-07-09, commit `1547254`).**
   Un `COPY` de AutoCAD comparte la *definición* del bloque y con ella el mismo GUID, así que `RACKEDITAR`
   edita todas las copias juntas (correcto para "réplicas"). `RACKDUPLICAR` cubre el caso opuesto: toma un
   rack, lee su diseño embebido y lo redibuja por el camino de inserción (jig) con un **GUID nuevo** y
   nombre "- copia", como su propio bloque; editar la copia no toca al original. Duplica la vista del
   bloque seleccionado y funciona para los 4 tipos. **Decisión (2026-07-09):** duplicar SOLO la vista
   clicada (guardando el sistema completo en el embed) es el comportamiento deseado — no se duplican todas
   las vistas. Mejora futura: usarlo como base del layout de almacén (#4: clonar N veces con pasillo).
6. ~~**`RACKLISTA`**~~ — ✅ **HECHO (2026-07-10):** ventana con la tabla de todos los racks del dibujo
   (Nombre, Tipo, Vistas presentes, nº de copias); `RackListBuilder` (puro, testeado) agrupa los sobres
   por GUID y "Ir al rack" hace zoom a la primera referencia en el modelo (frontal si existe).
7. ~~**Renombrado sincronizado**~~ — ✅ **HECHO (2026-07-09):** al editar/renombrar un rack, `RackBlockRenamer`
   sincroniza el nombre del bloque en TODAS sus vistas (frontal, lateral N, planta) en los 4 tipos
   (best-effort: no lanza, uniquifica evitando colisiones; las referencias apuntan por id, no se rompen).
8. ~~**Biblioteca de diseños**~~ — ✅ **HECHO (2026-07-09; ampliada 2026-07-10):** "Abrir de la biblioteca de
   diseños" en el menú `RACKCAD` lista los diseños `.rackcad.json` de una carpeta gestionada
   (`%AppData%\RackCad\Designs`, o la configurada) con nombre + tipo (`RackDesignLibrary`), y al elegir uno
   reabre el editor correcto precargado. **2026-07-10:** incluye TODOS los tipos — selectivo, cama y larguero
   ganaron persistencia a disco (`RackSystemKind.SelectiveRack/Cama/Larguero` en `RackProjectStore`), botón
   "Guardar en biblioteca" en selectivo/cama y apertura como rack nuevo (`LoadForNew`). Pendiente: miniaturas.
9. ~~**Plantillas de usuario**~~ — ✅ **HECHO (2026-07-10):** "Guardar como plantilla" en la configuración
   rápida del configurador de cabeceras guarda la cabecera actual como `RackFrameTemplate` reutilizable en
   `%AppData%\RackCad\user-templates.json` (ubicación escribible por usuario, no el `header-templates.json`
   compartido). `RackFrameTemplateFactory.FromConfiguration` es el inverso de la factory (captura forma,
   perfiles, poste, placa, diagonal y puntos de conexión; **no** las excepciones por panel); `UserTemplateStore`
   persiste (upsert por id). El desplegable "Tipo de cabecera" mezcla catálogo/internas + usuario (usuario gana
   por id).

### Ingeniería y datos
10. **Validación de capacidad de carga** — los CSVs ya llevan columnas Ix/Iy/norma; falta la regla que
    compare carga por nivel vs. capacidad del larguero/poste y avise en el editor.
11. ~~**BOM consolidado multi-rack**~~ — ✅ **HECHO (2026-07-10):** los BOM son **por COMPONENTES**
    (cabeceras + largueros como sub-ensambles expandibles a piezas; `BomComponent`, árbol en `RackBomWindow`)
    y el comando `RACKBOMTOTAL` genera el BOM de TODO el dibujo (desglose por rack via GUID x copias + gran
    total por componente, `RackConsolidatedBomWindow`). También existe el editor de **larguero** como
    componente (`RackLargueroWindow`, solo visual/BOM — sin bloque de AutoCAD todavía). **Export a Excel HECHO
    (2026-07-12):** botón "Exportar Excel" en ambas ventanas de BOM; `XlsxWriter` escribe un `.xlsx` real (OOXML,
    ZIP de XML con `System.IO.Compression`, SIN dependencias NuGet); `BomXlsxExporter` (1 hoja) y
    `ConsolidatedBomXlsxExporter` (hoja "Por rack" + hoja "Total del dibujo"). El CSV sigue disponible. Pendiente:
    el bloque de AutoCAD del larguero.
12. ~~**Unificar perfiles estructurales**~~ — ✅ **HECHO (2026-07-10):** `secciones.csv` es la única hoja de
    perfiles (columna `rol` = POSTE | CELOSIA | LARGUERO). El provider separa las filas en las tres listas
    de siempre (API de `RackCatalog` intacta) y mantiene los tres CSV legacy como fallback de lectura.

### Escalabilidad y problemas futuros anticipados

13. **Identidad estable de celdas de seguridad** — hoy `OffCells` usa indices `(frente,nivel)`. Insertar o quitar
    filas cambia su significado. Evolucionar a ids persistentes de frente/nivel (con migrador desde indices) antes de
    habilitar overrides por celda o edicion colaborativa de catalogos.
14. **Validador de catalogos con severidades** — validar ids duplicados, FKs, vistas/bloques faltantes, parametros
    requeridos y unidades al cargar. Mostrar un diagnostico unico en UI y permitir modo estricto para despliegues.
    **PARCIAL (I-19, 2026-07-21):** el MOTOR puro esta hecho (`CatalogValidator` en Application: severidades
    error/advertencia/informativa por las cinco categorias, filas descartadas por rol con aviso, `IsValid(strict)`
    y `Format()` como diagnostico unico de texto). PENDIENTE: cablearlo en la UI/WPF y en un comando de AutoCAD
    (superficie visual del "diagnostico unico") y la validacion de UNIDADES (no incluida en el alcance de I-19).
15. **Manifest de biblioteca DWG** — guardar junto al DWG una version/hash y la lista de bloques/parametros esperados.
    Asi un catálogo y una biblioteca incompatibles fallan antes de producir un dibujo parcial.
    **PARCIAL (I-19, 2026-07-21):** el modelo `CatalogBlockManifest` (esperado desde el catalogo + huella SHA-256 +
    round-trip JSON + comparacion contra un manifiesto real) esta hecho y probado. PENDIENTE: el paso de Plugin que
    EMITE el manifiesto real escaneando `blocks-library.dwg` y lo guarda junto al DWG (fuera de alcance de I-19: no
    se toca el DWG).

> **Hallazgo de I-19 (2026-07-21) — id de punto de conexion duplicado:** `connection-points.csv` trae `TROQUEL_TOPE`
> dos veces (roles `Poste` y `FlowBed`); `FindConnectionPoint` usa `FirstOrDefault`, asi que la fila `FlowBed` queda
> ensombrecida y su rol nunca se ve. El validador lo reporta como `DUPLICATE_ID` (error). No se corrige en I-19
> (editar un catalogo caliente append-only y cambiar el comportamiento de lookup por rol esta fuera de alcance);
> resolver luego: renombrar uno de los dos ids o unificar la definicion. La prueba de integridad
> (`ShippedCatalogIntegrityTests`) fija este estado conocido para que un id duplicado NUEVO falle el build.
16. **CI por capas** — ejecutar Domain/Application/tests en cualquier runner y reservar un smoke test Windows con
    AutoCAD para releases. El Plugin no debe impedir que las reglas puras tengan gate continuo.
17. **Benchmarks y presupuestos de complejidad** — medir resolver/builders/BOM con 10/30/100 frentes y el layout con
    5,000 candidatos. Convertir regresiones de tiempo/memoria en pruebas de benchmark antes de ampliar los limites UI.
18. **Migraciones de schema explicitas** — `SchemaGuard` hoy cubre compatibilidad por fallback. Antes del primer 2.x,
    crear una cadena de migradores idempotentes con fixtures de todos los documentos historicos soportados.
19. **Diagnostico por rack** — acumular piezas omitidas, parametros no aplicados y fallbacks usados en un reporte
    exportable asociado al GUID, en vez de depender solo de mensajes de la linea de comandos.
20. **Limites configurables con guardas** — `MaxDepthCount`, maximo de candidatos y tolerancias son limites de
    producto/rendimiento, no datos de geometria. Hacerlos configurables solo cuando existan benchmarks y validacion de
    compatibilidad; no eliminar las guardas para aparentar escalabilidad.

### Datos de producto y datos del usuario

- **Iniciativa futura: separar catalogos base y overrides de usuario** (`architecture/catalogos-usuario`) — hoy los
  CSV/JSON viven dentro de `RackCad.bundle` y cumplen dos papeles incompatibles: son datos versionados del producto,
  pero tambien pueden editarse localmente en una instalacion. I-04 mantiene el instalador simple y seguro: actualiza
  los catalogos como producto y solo preserva `blocks-library.dwg`. Disenar una capa separada y escribible bajo
  `%AppData%\RackCad` para overrides, con precedencia, validacion, migracion y una UI que muestre el origen efectivo de
  cada dato. No resolverlo mediante fusion oportunista de CSV durante la instalacion.

### Features nuevas no mapeadas antes (propuestas de la revisión 2026-07-15)

Ninguna de estas existía en este backlog; ordenadas por cercanía al flujo actual (cotizar → dibujar → instalar):

21. **Cotizador integrado** — los CSVs ya llevan `unitCost`/`currency`/`costUnit` (seguridad.csv) y el BOM ya
    exporta XLSX: falta la hoja de COTIZACIÓN (precios × cantidades + margen + IVA + totales por rack y por
    dibujo). Extender costos a secciones.csv/mensulas.csv y agregar una hoja "Cotización" al export.
22. **Peso por componente y total** — agregar peso/pie a `secciones.csv` (seguridad.csv ya tiene `weightEach`);
    el BOM sumaría peso por rack y del dibujo (dato de flete/instalación que hoy se calcula a mano).
23. **Anclas y tornillería en el BOM** — contar anclas por placa base (y tornillería por unión) como piezas
    del BOM + checklist de instalación exportable.
24. **Tabla-resumen de racks EN el dibujo** — una Table de AutoCAD auto-generada (nombre, tipo, tamaño,
    copias) desde los sobres embebidos, que se refresca al editar; hoy esa tabla se hace a mano en cada plano.
25. **Continuar hilera / snapping entre racks** — al insertar junto a un rack existente, ofrecer alinear al
    grid del vecino (o extender la hilera con pasillo estándar), en vez de colocar a pulso.
26. **Detección de colisiones en planta** — reutilizar `WarehouseFitChecker` contra los footprints de los
    racks YA dibujados para avisar traslapes/pasillos angostos al insertar o mover.
27. **Clear height del edificio** — agregar altura al modelo de sitio y verificar rack vs altura libre (con
    holgura a sprinklers) en `RACKLAYOUT`/`RACKRELLENAR`; hoy el sitio es solo 2D.
28. **Verificación normativa de pasillos/flue** — reglas configurables (incendios: flue space entre
    back-to-back, anchos mínimos por tipo de montacargas) evaluadas sobre el `WarehouseGridPlan`.
29. **Deshacer/rehacer en los editores** — snapshot del diseño por acción en las ventanas (la matriz del
    selectivo primero); hoy un cambio accidental de matriz no tiene vuelta atrás.
30. **Duplicar/copiar filas y columnas de la matriz** — duplicar un frente o un nivel con todo su contenido
    (celdas, tramos, overrides) en un clic.
31. **Actualización masiva por catálogo** — al cambiar un perfil en `secciones.csv`, comando que localice
    todos los racks del dibujo que lo usan y los regenere (batch `RACKEDITAR`), con reporte de qué cambió.
32. **Paquete de intercambio de diseño** — exportar `.rackcad.json` + el subconjunto de catálogo que usa
    (y la lista de bloques requeridos) en un solo archivo, para compartir entre máquinas sin desalinear
    catálogos. Complementa el manifest de biblioteca (#15).
33. **Plano de fabricación por pieza (shop drawings)** — dibujo de detalle por perfil (cortes, perforaciones,
    saques) generado desde el BOM; hoy el detalle de fabricación se dibuja aparte.
34. **Vista 3D / export IFC** — todo es 2D por diseño; un export 3D básico (extrusión de las vistas) abriría
    Navisworks/BIM para revisión de interferencias del cliente.

### Hallazgos de la revisión de código 2026-07-15

**Corregidos en el momento** (con test de regresión verificado fallando): lector CSV tomaba la primera fila
como header aunque fuera una fila en blanco (catálogo se vaciaba en silencio); paso de rodillo sin acotar
(un typo congelaba la UI); `RackFrameProjectDocument` perdía 4 campos de celosía/grid al guardar;
`RackFrameProjectStore` sin guard de esquema ni validación de degenerado ("{}" cargaba una cabecera de alto
0); el BOM materializaba tarimas/cotas/anotaciones solo para descartarlas (ahora las vistas de conteo van
sin decoración, con test de equivalencia); `RACKLISTA` sumaba referencias de TODAS las vistas como "Copias"
(ahora usa el máximo, igual que `RACKBOMTOTAL`).

**Diferidos con evidencia (implementar cuando toque, ya diagnosticados):**
- El reporte de inserción descuenta 1 por TIPO de bloque faltante, no por pieza omitida
  (`LateralHeaderDrawService.CreateBlock` vs dedup de `AppendInstance`): contador real de saltos.
- El catálogo se re-stat-ea por CADA vista en el redraw multi-vista (~290 stats de archivo por Actualizar
  con carpeta compartida): pasar el `RackCatalog` ya cargado a los `RedrawInPlace`.
- `Database.Purge` corre una vez POR VISTA en vez de una por edición: acumular candidatos y purgar una vez
  (medir antes, puede ser barato).
- Constantes de negocio hardcodeadas (alza del tope 8", holgura del lateral 4", paso del separador 100",
  tarima default 42×60×2): moverlas a `defaults.json`/columnas de `seguridad.csv` con los valores actuales
  como fallback.
- (hallazgo I-15, 2026-07-21) Asimetría de estilos de cota en el selectivo NUEVO desde el menú: el menú
  principal ("Diseñar sistema selectivo") abre `RackSelectiveWindow` **sin** `SetDimensionStyles`, mientras
  que el comando directo `RACKSELECTIVO` y el abrir-desde-biblioteca **sí** los fijan. Efecto: un selectivo
  nuevo creado por RACKCAD no ofrece los estilos de cota guardados del dibujo. I-15 lo preservó verbatim
  (`SelectiveEditorModule.OpenForNew`); corregir = pasar los estilos también en ese path, con validación en
  AutoCAD (cambia lo que ve el usuario al insertar cotas de un selectivo nuevo desde el menú).
- ~~(hallazgo I-24, 2026-07-22) Entrada obsoleta de I-21 en `docs/initiatives/README.md` (decía «abiertos… No
  integrada» pese a estar integrada el 2026-07-21)~~ — ✅ **CORREGIDO (2026-07-22)** en la reconciliación
  documental de la integración de I-24: el bullet de `I-21-dynamic-editor-state.md` ahora dice «aprobadas por el
  dueño; integrada en `main` el 2026-07-21».
- (hallazgo I-24, 2026-07-22) Laguna de cobertura PURA **comprobada** en `RackCad.Tests` (fuera del alcance de
  I-24, que sólo amplía `RackCad.UI.Tests`): `DynamicFrontMatrixTests` prueba los alcances `Cell`, `All` y
  `Selected` de `DynamicFrontMatrix.ApplyScope`, pero **no** los alcances `Level` ni `Front`, que sí existen en
  el enum `DynamicRackCellScope`. Añadir esas dos pruebas cuando se retome el dinámico. (`DynamicEditorCellTests`
  y `DynamicEditorSafetyTests` **sí** existen y cubren esas clases; no son una laguna.)

**Señalados pero NO verificados** (la verificación adversarial no alcanzó a correr; validar antes de actuar):
posible coma decimal mal parseada en campos del configurador; el editor del dinámico podría resetear
cabeceras custom en el round-trip; el redraw multi-vista parcial podría dejar vistas con diseños divergentes
tragándose el error; `RACKEDITAR` sobre capas bloqueadas (falta `forceOpenOnLockedLayer`); conteo de copias
de `RACKBOMTOTAL` confía en el cache sin validar; plan de desviadores reconstruido por corte (O(frentes²));
fórmula del snap-Y del tope duplicada en frontal/lateral; predicado "esta selección contribuye" duplicado
UI/resolver; ~13 puntos de contacto para agregar una familia de seguridad; dos costuras de extracción en
`RackSelectiveWindow.xaml.cs` (2,462 líneas): render del preview y máquina de estados de la matriz por fondo.

## B. Deuda técnica diferida de la auditoría (2026-07-08)

### Necesitan validación en AutoCAD (no tocar sin probar dibujando)
- ~~**Definiciones huérfanas al editar el dinámico**~~ — ✅ **HECHO (2026-07-09):** `RedefineSystemBlock`
  purga las definiciones anidadas que quedan sin referencia tras redefinir (los bloques de catálogo y los
  usados por otros racks se conservan). Conviene una verificación visual final en AutoCAD.
- ~~**Doble diagonal: preview vs dibujo**~~ — ✅ **HECHO (2026-07-13):** el preview/BOM
  (`BracingPanelMemberBuilder.CreateDoubleDiagonal`) usaba un offset **horizontal** 0.14·fondo a altura
  completa; ahora usa la **regla de troqueles** (dos diagonales de fondo completo, offset **vertical** por
  `DiagonalDoubleSpacingTroqueles`, con retranqueo start/end) vía un helper compartido en el dominio
  (`BracingDiagonalGeometry.DoubleDiagonal`) que **también** llama el builder lateral, así no pueden volver a
  divergir. El dibujo queda byte-idéntico; cambia la longitud de la doble diagonal en el BOM (ahora la real,
  fondo-completo). Con tests. Nota: el member builder no hace el *snap* a troqueles (necesitaría la Y-base del
  poste), así que preview y dibujo coinciden salvo ese ajuste sub-pulgada. **Verificar visualmente en AutoCAD.**

### Decisiones de producto pendientes
- ~~**Validación de esquema en los stores**~~ — ✅ **HECHO (2026-07-13):** `SchemaGuard.CheckReadable` rechaza
  archivos con un MAJOR de esquema más nuevo que el que este build entiende (mensaje claro); `RackDesignValidation`
  aporta los chequeos de mínimos por tipo (cabecera Height/Depth>0 + postes; selectivo con frentes; cama con largo
  de riel; larguero con perfil; dinámico con módulos) y `RackProjectStore`/`SelectivePalletDesignStore` lanzan
  `InvalidOperationException` en degenerado (antes `{}` daba una cabecera con alto 0 en silencio); el store de cama
  (tolerante) devuelve null. Migración: la retro-compat de los documentos (fallbacks legacy) es el camino de upgrade
  hoy; el gancho de transformación irá en `SchemaGuard` cuando aterrice el primer cambio de MAJOR. Con tests.
- ~~**`Recompose` del dinámico borra overrides**~~ — ✅ **HECHO (2026-07-09):** cambiar niveles/altura o
  alternar "Reforzar poste derivado" es no destructivo (`UpdateHeaderHeightInPlace`), y al cambiar la
  **especificación de tarima**/`PalletsDeep` (rebuild completo) ahora se **conservan los fondos
  personalizados** de las cabeceras (snapshot por orden + re-aplicar a la nueva altura). "Restaurar
  estándar" sigue reseteando todo. Solo las ediciones estructurales profundas de una cabecera se
  reconstruyen al estándar tras un cambio de malla (inherente).
- ~~**Altura editable en el editor avanzado de cabecera**~~ — ✅ **HECHO (2026-07-09):** el campo Altura del
  editor avanzado es **solo-lectura** (derivada de las horizontales); la altura objetivo real vive en la
  configuración rápida (`SimpleHeightText`).

### Limpieza de código — HECHA (2026-07-09)

Toda esta sección quedó aplicada (ver la nota fechada al inicio del documento). Notas de lo que se
conservó a propósito, por si vuelve a auditarse:
- Se mantuvo `BracingPanel.Index` (vivo en `RackFrameEngineeringPreviewLayout`); solo se borraron los
  alias `SideMode`/`DefaultMemberProfileId`. No confundir el `Configuration.BracingSegments` del dominio
  (borrado) con el `BracingSegments` del ViewModel (`ObservableCollection<BracingSegmentEditorRow>`, UI viva).
- `FrameMemberEnd.HorizontalPositionRatio` sigue vivo (geometría); solo se borró `FrameMember.PositionRatio`.
- De las primitivas de preview se compartieron `AddLine`/`AddRectangle` (`PreviewCanvasPainter`); cada
  ventana conserva su `Map` (proyección propia) y su etiqueta (estilos divergentes).

### UX menor — TODO HECHO (2026-07-09)

Toda esta lista quedó cerrada en el batch de quick wins + higiene:
- ✅ La matriz del selectivo ya no descarta lo tecleado al cambiar de celda (aplica si es válido o pregunta).
- ✅ Los combos de enums (`Cara`/`Dirección`/`Patrón`/`Estado`) se muestran en español (`EnumDisplayConverter`).
- ✅ Los campos opcionales del dinámico/cama avisan texto inválido en vez de tragarlo (`TryOptionalNum`).
- ✅ `FindTreeViewItem` solo expande la ruta al ítem objetivo (restaura ramas colapsadas).
- ✅ Los puntos de conexión del grid de paneles ya eran ComboBox (`ConnectionPointOptions`) — el ítem estaba obsoleto.
- ✅ Se avisa cuando la cabecera de un poste queda MÁS BAJA que el nivel de carga superior.

### I-22 — hallazgos adyacentes (2026-07-22, registrados sin corregir)

- **Rejilla de parrilla y de defensa sin `SelectionMatrix`**: I-22 adoptó `SelectionMatrix` en las tres
  rejillas frente/poste × nivel plano-on/off del editor selectivo (guía, tope, desviador). La de
  **parrilla** conserva su diálogo propio porque muestra un **contador vivo por celda** (cuántos decks
  caben), y la de **defensa** porque es un **formulario por poste** (dos toggles + dos longitudes), no una
  matriz plana on/off. Adoptarlas exigiría extender `SelectionMatrix` con contenido/adorno por celda
  (parrilla) o no aplica (defensa). Fuera de alcance de I-22; candidato a una iniciativa de UI posterior.
- **Accesos planos delegados en `SelectiveSafetySelection`**: la descomposición por subtipo (E7) conserva
  las propiedades planas (`TopeSaque`, `ParrillaFrente`, …) como accesos delegados a las configs para no
  tocar los consumidores restantes (UI y pruebas) ni el formato de alambre. Migrar esos consumidores a
  `selection.Tope.*`/`selection.Parrilla.*` y retirar los accesos planos es un barrido mecánico posterior.
- **"Pulgada par" vs paso de troquel**: `SelectiveDesviadorPlan.IsValidEvenAbove8` (`% 2`) y el consejo de
  la nota del desviador (`floor(claro/2)*2`) usan 2" como "pulgada par", concepto distinto del paso de
  troquel de rejilla (`SelectiveRackDefaults.TroquelPaso`, que I-22 unificó en los 5 snaps). Se dejaron sin
  enrutar a la constante a propósito (no son snaps de rejilla); si el paso de troquel dejara de ser 2",
  habría que revisar si estos deben seguirlo.

### I-03 — hallazgos adyacentes (2026-07-22, registrados sin corregir)

- **`UiSupport.LoadCatalogSafe` (capa UI) sigue tragando la carga de catálogo en silencio**: I-03 hizo
  diagnosticable el único punto de carga del **Plugin** (`RackCatalogLoader`, que ahora registra el fallo y
  el catálogo vacío), pero el editor WPF carga el catálogo por su propia ruta (`UiSupport.LoadCatalogSafe`),
  que captura y devuelve un `RackCatalog` vacío sin registrar. El ROADMAP acota I-03 a Plugin + Persistence,
  así que quedó fuera. Como `RackCad.UI` ya depende de `RackCad.Application`, adoptar `RackLog.Exception` ahí
  es una sola línea; hacerlo cuando se retome la UI (o al unificar ambas rutas de carga, hoy separadas por la
  dirección de dependencias Plugin↛UI).
- **Sin rotación ni retención del log**: `RackDiagnosticsLog` escribe un archivo por día
  (`rackcad-AAAAMMDD.log`) en `%AppData%\RackCad\logs` y nunca lo poda. Es "logging mínimo" a propósito (I-03
  excluye telemetría/retención), pero a lo largo de años la carpeta crece sin límite; una iniciativa posterior
  puede añadir poda por antigüedad/tamaño (p. ej. conservar N días) sin cambiar la superficie pública de
  `RackLog`.
- **Lecturas tolerantes de embeds NO instrumentadas a propósito**: `FlowBedConfigurationStore` y
  `SelectivePalletDesignStore` devuelven `null`/omiten ante un embed de MAJOR más nuevo o ilegible; eso es
  **diseño intencional de I-11** (un bloque ajeno se salta, no aborta el escaneo), no un "fallo silencioso"
  del tipo D2, por lo que I-03 no las tocó. Si en el futuro se quisiera un rastro de esos saltos, habría que
  medir el ruido (el escaneo recorre todos los bloques del dibujo) antes de registrar.

### I-32 — hallazgos de Push Back diferidos y limitaciones registradas (2026-07-25)

El Owner reportó catorce hallazgos sobre el Push Back integrado por I-18. I-32 corrigió diez
(PB-002…PB-006, PB-008…PB-010, PB-012, PB-013). Los otros cuatro quedan aquí como **candidatos futuros**,
no implementados, con lo que ya se sabe de cada uno:

- **PB-001 — Previews de Push Back (prioridad baja del Owner).** Las cuatro vistas del previsualizador
  siguen siendo insatisfactorias. Es el MISMO frente que I-18 ya había diferido a «una iniciativa
  transversal futura que abarque a los tres editores»
  ([`decisions/I-18.md`](automation/decisions/I-18.md), addendum final §3): el preview **no** está aprobado
  visualmente. Lo que I-18 dejó hecho y esa iniciativa hereda es la infraestructura compartida extraída del
  renderer Dinámico (`EditorPreviewPalette`/`Surface`/`Parts`), ya consumida por los dos editores y con la
  equivalencia del Dinámico medida. Se parte de una sola tubería, no de dos painters divergentes.
- ~~**PB-007 — Reconfigurador masivo de elementos de seguridad (prioridad alta del Owner, GENERAL, no solo
  Push Back).**~~ **RESUELTO por I-34** (rama `feature/edicion-masiva-seguridad`). La decisión de alcance
  que este punto pedía la dio el Owner al abrir la iniciativa, y la amplió por addendum durante la
  validación. Las **cuatro** matrices booleanas de seguridad —desviador (eje **poste**), tope (eje
  **frente**, que cubre a la vez el tope del Selectivo y el **tope posterior de Push Back**), guía y
  **parrilla**— ganan el estado **Activar/Desactivar** y los alcances **Celda / Nivel / Frente-o-Poste /
  Todo**, sobre `SelectionMatrix`/`SelectionMatrixModel` como se preveía. La infraestructura es
  **agnóstica a `RackSystemKind`**: cada diálogo declara sus etiquetas y capacidades. Contrato en
  [`initiatives/I-34-edicion-masiva-seguridad.md`](initiatives/I-34-edicion-masiva-seguridad.md).
- **Edición masiva de la DEFENSA de montacargas — candidato futuro INDEPENDIENTE** (nace del alcance que
  I-34 dejó deliberadamente fuera; no es deuda de I-34, que cerró completa). La defensa es el único
  elemento de seguridad que **no** es una matriz booleana: es un formulario **por poste** con dos
  longitudes independientes (salida/entrada), sus dos casillas de «Auto» y su propio DTO
  `SafetyPostDefense`. **No tiene eje de nivel**, así que los alcances «Celda» y «Nivel» de I-34 no
  significan nada en ella y el mecanismo compartido no le aplica tal cual. Antes de tocarla hay que
  decidir **qué es un alcance en un formulario por poste** —¿aplicar una longitud a todos los postes?,
  ¿a un rango?, ¿por extremo?—. El Owner la mantuvo fuera al aprobar I-34 y dejó constancia de que **no
  bloquea**.
- **PB-011 — Editor «avanzado» de módulos en Push Back (prioridad alta del Owner).** El Dinámico permite
  seleccionar un módulo (cabecera o separador) y personalizarlo —medida, cantidad de separadores, cabecera
  personalizada—; Push Back no. Nota técnica levantada al arreglar PB-013: hoy **toda** cabecera de Push
  Back es «calculada» (`DynamicRackSystemBuilder` la crea con `UseCalculatedHeaderConfiguration = true` y
  solo la ventana del Dinámico lo pone en false), y de ahí depende que el alto de tarima general sea inerte.
  Si Push Back gana cabeceras personalizadas, esa dependencia debe revisarse en el mismo cambio.
  **EN CURSO como I-35** (rama `feature/editor-avanzado-push-back`); contrato en
  [`initiatives/I-35-editor-avanzado-push-back.md`](initiatives/I-35-editor-avanzado-push-back.md).

  **La inconsistencia que la auditoría de apertura de I-35 encontró y que esta nota no anticipaba.** La
  nota anterior advertía de UNA dependencia (el alto de tarima general). Hay **dos caminos más** que hoy
  son inertes por la misma razón —ninguna cabecera de Push Back es personalizada— y que se vuelven reales
  **en la misma sesión** en que Push Back gane cabeceras personalizadas:

  1. **La reconciliación de módulos pierde la cabecera personalizada.**
     `DynamicEditorDesignAssembler.SnapshotHeaderFondos` guarda **solo el fondo** por ordinal, y
     `RestoreHeaderFondos` reasigna ese fondo, **fuerza `UseCalculatedHeaderConfiguration = true`** y
     **reconstruye la configuración desde la fábrica**. Es decir: un cambio de tarima o de fondos
     revierte a calculada cualquier cabecera personalizada. `PushBackEditorDesignAssembler` llama ese
     mismo camino. **Inconsistencia**: la bandera `UseCalculatedHeaderConfiguration` promete «preservar
     la configuración completa del usuario» (su propio XML-doc) y el snapshot no la preserva. El Dinámico
     convive con esto desde siempre; corregirlo **allí** cambiaría el Dinámico, así que la decisión de si
     Push Back diverge a propósito es del Owner (pregunta *b* de la sección 12 del contrato de I-35).
  2. **El clon del resolver no es el clon canónico de I-17.**
     `DynamicRackSystemResolver.CloneHeader` es `RackFrameProjectDocument.FromConfiguration(...)
     .ToConfiguration()`, no `RackFrameProjectStore.DeepCopy`. El documento **no persiste**
     `RackFrameConfiguration.Exceptions` —I-17 las declara estado *runtime* y por eso `DeepCopy` las
     rea­nexa— así que ese round-trip las **descarta**.
     `PushBackEditorDesignAssembler.CopyStructureSystem` lo recorre (`Snapshot` + `Resolve`) en **cada**
     recálculo sin cambio estructural. El modelo *derivado* sí se reconstruye (`builder.Refresh` llama
     `RefreshPhysicalModel` en cada cabecera), de modo que la pérdida es **exclusivamente** de
     `Exceptions`.

  Ninguna de las dos se corrige «de paso»: la primera necesita decisión del Owner y la segunda toca un
  tipo compartido con el Dinámico. Quedan aquí registradas con su evidencia para que I-35 las resuelva en
  su fase correspondiente y nadie las re-descubra.
- ~~**PB-014 — Frente «en blanco» (Push Back y Dinámico).**~~ **RESUELTO por I-33** (rama
  `feature/frente-en-blanco`). La decisión de alcance que este punto pedía la dio el Owner al abrir la
  iniciativa: aplica al **Dinámico y a Push Back**, no al Selectivo. Un frente en blanco conserva su claro
  y su estructura, desplaza a los frentes posteriores y no lleva ningún nivel ni componente de carga; su
  configuración queda dormida para reactivarlo. Contrato en
  [`initiatives/I-33-frente-en-blanco.md`](initiatives/I-33-frente-en-blanco.md).

Limitaciones y observaciones registradas al corregir, **sin tocar** por la restricción de no cambiar el
Selectivo ni el Dinámico:

- **El Dinámico sigue leyendo la celda del desviador por FRENTE, no por poste.** En Push Back esto quedó
  **corregido** (la off-cell es POSTE × NIVEL y la leen igual el lateral, los dos frontales, la planta y el
  BOM, a través de `SelectiveDesviadorPlan.CellKey`). El Dinámico conserva a propósito la lectura histórica
  `Math.Min(postIndex, Fronts.Count - 1)`, que colapsa el último poste sobre el último frente: apagar la
  celda del penúltimo poste suprime también la del último. La capacidad
  `SelectiveSafetySelection.DesviadorCellsAreByPost` es el interruptor; activarla para el Dinámico
  cambiaría su comportamiento y necesita decisión del Owner.
- ~~**El mismo defecto de contrato que PB-002 corrigió en Push Back sigue en el Dinámico**~~ **CORREGIDO por
  I-33**: `RackDynamicSystemWindow` entregaba al diálogo compartido una lista POR FRENTE marcada como por
  POSTE, así que el último poste caía a 1 nivel. Ahora entrega las dos listas por separado —por frente para
  la guía, por poste (`DynamicFrontActivation.EffectiveLevelsPerPost`) sólo para el desviador—. Esto corrige
  la **forma de la rejilla**; la **lectura de la celda** en el dibujo sigue siendo por FRENTE (ver el punto
  anterior sobre `DesviadorCellsAreByPost`, que continúa pendiente de decisión del Owner).
- **La regla «niveles en un poste» sigue duplicada**: `DynamicFrontGeometry.LoadLevelsAtPost` y la copia
  privada de `DynamicSafetyMultiViewBuilder`, con fallbacks distintos. I-32 unificó la primera con la
  versión pura que consume la UI, pero no colapsó la segunda. **I-33** hizo que **ambas** copias respeten
  el estado Activo/En blanco (vía `DynamicFrontActivation.EffectiveLoadLevels`), pero tampoco las colapsó:
  siguen siendo dos funciones con fallbacks distintos.
- **«Dibujar en frontal» sigue visible e inerte en el diálogo del tope para Push Back** (su adaptador solo
  lee SAQUE y las off-cells). Es de la misma clase que PB-006, pero el Owner no lo reportó y quedó fuera.
- **`INICIO_IZQUIERDO`/`INICIO_DERECHO` del `LARGUERO_ESCALON_TROQUEL_REDONDO` no dependen del PERALTE**
  (`localYPorParam = 0` en `connection-layout.csv`) aunque el editor deje variar el peralte entre 3 y 6.
  Tras PB-004 esa fila ya no afecta a la pendiente, pero conviene que el Owner mida en el DWG si esa cota
  debería seguir al peralte.
- **La tarima general de Push Back conserva Frente/Alto/Peso sin UI que los cambie.** Tras PB-013 esos tres
  quedan congelados en el valor cargado (42/60/1000 en un rack nuevo) y se siguen persistiendo así. Son
  inertes para la geometría porque la celda manda, pero si alguna vez dejaran de serlo habría que darles
  una superficie de edición o dejar de persistirlos.

## Referencias documentales muertas que dejó I-09 (hallazgo de I-23, fuera de alcance)

I-23 barrió los referentes de las rutas que movió y **no dejó ninguna referencia muerta nueva**. Al
comprobarlo aparecieron **siete** que ya estaban rotas en la base `b43b5d1`, todas por el reparto que
hizo **I-09** el 2026-07-20 y que nadie barrió entonces:

| Documento | Referencia muerta |
|---|---|
| `docs/WORKFLOW.md` §7 | `src/RackCad.Plugin/RackFrameCommands.cs` |
| `docs/guias/catalogos-y-plantillas.md` | `src/RackCad.Plugin/RackFrameCommands.cs` |
| `docs/guias/generacion-cabecera-lateral.md` (×2) | `src/RackCad.Plugin/RackFrameCommands.cs` |
| `docs/guias/generacion-cabecera-lateral.md` | `src/RackCad.UI/IHeaderDrawService.cs` |
| `docs/guias/modelo-de-datos.md` | `src/RackCad.Plugin/RackFrameCommands.cs` |
| `docs/guias/modelo-de-datos.md` | `src/RackCad.Plugin/RackFrameCommands.List.cs` |

No se arreglan aquí porque no es una sustitución mecánica: `RackFrameCommands` se partió en **nueve**
clases por área (`RackMenuCommands`, `RackCabeceraCommands`, `RackSelectivoCommands`, …), así que cada
referencia exige decidir a cuál de las nueve apuntaba, y la fila de archivos calientes de `WORKFLOW.md`
§7 además necesita saber si el riesgo de conflicto sigue siendo el mismo tras la partición. Es trabajo
de documentación con criterio, no de reemplazo de cadena, y I-23 está bajo congelación funcional.

---

## Dos comandos ausentes de la referencia en la app (hallazgo de I-36B, fuera de alcance)

`RackCommandReference.Commands` (`src/RackCad.UI/RackCommandReference.cs`) es la única fuente de verdad
de lo que muestra `RACKAYUDA`, y la tabla de comandos de `README.md` la acompaña. **Faltan dos** de los
comandos realmente registrados:

| Comando | Alias | Desde |
|---|---|---|
| `RACKPUSHBACK` | `RPB` | I-32 |
| `RACKSECCION` | — | I-36B |

`RACKSECCION` **sí quedó** en la tabla de `README.md`: WORKFLOW §8 obliga a actualizar ese archivo en la
misma rama cuando cambian los comandos de AutoCAD. Lo que no se tocó es `RackCommandReference`, que es
UI vigente y queda fuera del alcance de I-36B (`docs/initiatives/I-36B-*.md` §4 y §7).

Arreglarlo bien es una decisión de producto pequeña pero real, no un añadido mecánico: hay que decidir
**a qué grupo** pertenece cada uno —`RACKSECCION` no es «Diseñar» un rack, y podría justificar un grupo
nuevo— y si `RACKSECCION` merece alias corto, que hoy no tiene. Conviene hacerlo de una sola vez para
los dos, con el `README.md` en el mismo cambio.

---

## REQUISITO FUTURO OBLIGATORIO — Perfiles IPS/S y geometría visual mejorada de perfiles laminados

> **No es una idea opcional.** Es un requisito registrado por decisión expresa del Owner el
> **2026-07-28**, al aprobar el gate `owner-validation` de I-36B. El resto de este documento es backlog
> recomendado; esta sección no lo es.

**De dónde sale.** Al validar I-36B en AutoCAD, el Owner comparó los canales C que genera RackCad con
los perfiles comerciales de las librerías CAD y constató que **no reproducen completamente su
apariencia**: falta la **conicidad de los patines**, los **redondeos o chaflanes de punta** y las
**transiciones características del laminado**. La comparación visual **confirma** que esa diferencia es
la que explica el error de área conocido de la familia (5.545 % máximo, 3 filas de 32).

El Owner aceptó esa diferencia sin bloquear I-36B: los canales quedan como `TabulatedDerived`, la
geometría actual es **técnicamente honesta y suficiente como fundación**, y **no se inventan** radios,
chaflanes ni conicidades dentro de I-36B. Lo que sigue es lo que una iniciativa futura **deberá** hacer.

### Lo que esa iniciativa deberá hacer

1. **Incorporar perfiles IPS**, verificando primero su correspondencia con la familia AISC `S` o con el
   catálogo comercial que usa la empresa. Es una verificación, no una equivalencia asumida.
2. **Importar la fuente y las dimensiones** correspondientes, con la misma disciplina de I-36A:
   importador reproducible fuera del producto, sin descartes silenciosos.
3. **Modelar la inclinación característica de los patines.**
4. **Representar radios, chaflanes y transiciones visuales cuando exista una regla acreditada** — y solo
   entonces. La prohibición de inventar dimensiones no se levanta; lo que cambia es que esa iniciativa
   sí puede ir a buscar la regla a su fuente.
5. **Mejorar visualmente los canales C y los demás perfiles laminados.**
6. **Mantener separadas la geometría tabulada y la geometría visual o de presentación.** Son dos
   autoridades distintas y no deben mezclarse en un solo contorno.
7. **Declarar claramente cuándo una geometría visual es aproximada.** Misma regla que la fidelidad de
   I-36B: lo aproximado se dice, no se insinúa.
8. **No sustituir ni alterar la geometría tabulada de I-36B.** Es la fundación y se queda como está.
9. **No mezclarse con Cantilever (I-37)**, salvo que el contrato de aquella lo requiera explícitamente.

### Estado

**RECLAMADA como I-36D el 2026-07-28**, por instrucción directa del Owner: rama
`feature/perfiles-aisc-s`, [contrato](initiatives/I-36D-perfiles-aisc-s.md),
[ADR-0023](adr/0023-geometria-visual-derivada-perfiles-s.md) (`propuesto`),
[decisiones](automation/decisions/I-36D.md), [evidencia](automation/evidence/I-36D-auditoria-aisc-s.md)
y [estado](automation/state/I-36D.yml). **Deja de ser un pendiente sin rama.**

Lo que la auditoría de I-36D añadió a este registro, medido contra el libro y no supuesto: la fuente
**no publica** la pendiente del patín ni ningún radio explícito, así que el punto 5 («radios,
chaflanes y transiciones comerciales, sólo cuando exista una regla acreditada») **sigue sin regla
acreditada** y I-36D **no lo resuelve**: lo que hace es declarar la pendiente como **convención visual
de RackCad**, separada de la geometría tabulada, que es exactamente lo que los puntos 6, 7 y 8 exigían.
Los puntos 3, 4 y 5 **sólo se cubren para S**; la mejora visual de **C y de los demás laminados sigue
pendiente y sin rama**.

Registrado también en [ADR-0022](adr/0022-geometria-parametrica-de-secciones-estructurales.md), en la
[decisión versionada de I-36B](automation/decisions/I-36B.md), en su
[evidencia](automation/evidence/I-36B-geometria-secciones-estructurales.md), en su
[estado](automation/state/I-36B.yml) y en la
[guía de geometría](guias/geometria-secciones-estructurales.md).

---

## Pendientes de perfiles estructurales — el registro completo (I-36C, 2026-07-28)

> **Nada de esto invalida lo ya implementado.** El catálogo neutral (I-36A) y el generador paramétrico
> (I-36B) están **integrados, cerrados y validados por el Owner**: 983 perfiles AISC v16.0, geometría
> generada en código para W, HSS rectangular/cuadrado, C y L, inspector, preview, `RACKSECCION`,
> bloque interno del dibujo y —desde I-36C— acceso desde el menú principal. Lo que sigue son
> **ampliaciones futuras**, no defectos abiertos.

Los puntos **1 a 6** son el **requisito futuro obligatorio** que el Owner registró al cerrar I-36B (ver
la sección anterior de este documento). El resto es alcance planificado o diferimientos ya decididos.

| # | Pendiente | Estado |
|---|---|---|
| 1 | **Perfiles IPS/S** | **reclamado por I-36D** (2026-07-28) |
| 2 | **Verificación de correspondencia** de IPS con la familia AISC `S` o con el catálogo comercial de la empresa | **resuelto por decisión de I-36D**: `IPS = AISC S`, escrito y vinculante |
| 3 | **Geometría visual mejorada** para perfiles laminados | **parcial en I-36D: sólo S.** C y los demás laminados siguen pendientes y sin rama |
| 4 | **Conicidad de patines** | **parcial en I-36D: sólo S**, y como **convención visual de RackCad**, no como dato de fuente |
| 5 | **Radios, chaflanes y transiciones comerciales**, sólo cuando exista una **regla acreditada** | **sigue pendiente**: la auditoría de I-36D confirmó que AISC v16.0 **no publica ningún radio**; I-36D deriva un filete **visual** y **no** modela punta ni transiciones |
| 6 | **Separación** entre geometría **tabulada** y geometría **visual aproximada**, declarando cuándo la visual lo es | **cubierto por I-36D / ADR-0023**: eje de autoridad `TabulatedConstrained` frente a `VisualDerived`, ortogonal a la fidelidad |
| 7 | **Cantilever I-37** | planificado en ROADMAP Fase 6 |
| 8 | **Miembros estructurales** (`StructuralMember`, configuradores de columna, brazo y base) | nace con I-37 |
| 9 | **Materiales, conexiones y fabricación** (troqueles, ménsulas, perforaciones, soldaduras, placas) | nace con I-37 |
| 10 | **Cálculo resistente y selección estructural** | I-38; no reabre [ADR-0017](adr/0017-validacion-cargas-diferida-ram-elements.md) sin un ADR nuevo |
| 11 | **Sólidos 3D** (`Region`, `Solid3d`, extrusión, sweep) | prohibido por [ADR-0022](adr/0022-geometria-parametrica-de-secciones-estructurales.md); necesitaría un ADR que lo reemplace |
| 12 | **Round-trip de perfiles independientes**: hoy lo insertado es geometría plana y no se puede reabrir para editarla | sin decidir |
| 13 | **Posible incorporación de familias adicionales** más allá de W, HSS rect./cuad., C y L | sin decidir |

**No se ha abierto** rama, contrato ni worktree para ninguno de ellos.

## Documentación desalineada con el árbol (hallazgo de I-37A, fuera de alcance)

Detectados al preparar e implementar I-37A. Ninguno se arregló «de paso» (WORKFLOW §8): cada uno es un
cambio de texto visible o de una fuente normativa, y merece su propia decisión.

| # | Dónde | Qué dice | Qué es cierto |
|---|---|---|---|
| 1 | `docs/ARCHITECTURE.md` §4.4.3 (encabezado) | «I-36D, **en curso**» y ADR-0023 en estado `propuesto` | I-36D está **integrada** (merge `3c6ccf5`) y **ADR-0023 fue aceptado** el 2026-07-28. I-37A añadió §4.4.4 justo debajo y **no** tocó esa línea |
| 2 | `docs/ARCHITECTURE.md` §8 | «Apéndice temporal: agregar un tipo hoy», y apunta a `RackFrameCommands` | Ese archivo ya no existe con ese rol (I-09 lo partió). La fuente vigente es `docs/guias/agregar-un-sistema.md`, que lo sustituye desde I-18. Riesgo real: un ejecutor que lo lea seguirá un patrón retirado |
| 3 | `AGENTS.md` §«Pruebas» y `docs/WORKFLOW.md` §4.5.4, §8 | Citan `docs/HANDOFF.md` **§8-12** y **§14** | `HANDOFF.md` tiene **siete** secciones de primer nivel; los conteos viven en §5. Una sesión de integración que siga la letra escribiría en secciones inexistentes |
| 4 | `docs/WORKFLOW.md` §7 (archivos calientes) | Lista `src/RackCad.UI/RackSelectiveWindow.xaml.cs`, `src/RackCad.UI/RackFrameConfiguratorViewModel.cs` y `src/RackCad.Plugin/RackFrameCommands.cs` | Los dos primeros los **movió I-23** a sus carpetas por sistema; el tercero **no existe**. Complementa el bloque de referencias muertas de I-09 de más arriba |
| 5 | `docs/context-packs/ui-editors.md` | El Editor Shell «no se asume como existente» | I-14, I-15, I-30 e I-31 están integradas y `agregar-un-sistema.md` §3 **obliga** a componer sobre `RackEditorVisualShell`. Un ejecutor que cargue el pack literalmente concluirá lo contrario de la receta — importa para la subiniciativa del editor de Cantilever |
| 6 | `docs/guias/glosario.md` (tabla Sistemas) | Push Back es un «Sistema futuro que será la prueba del contrato modular nuevo» | Integrado desde 2026-07-25. I-37A tocó ese archivo para añadir la sección Cantilever y **dejó la fila como estaba**, a propósito |
| 7 | `src/RackCad.Application/Persistence/RackListBuilder.cs` (`KindLabel`) | Mapea cuatro `Kind` y devuelve el token crudo por omisión | Un Push Back se lista hoy en `RACKLISTA` como `"pushback"`. Es **texto visible al usuario**: arreglarlo necesita aprobación propia, no colarse en la iniciativa de otro sistema |
| 8 | `docs/context-packs/` | No existe un pack de **secciones estructurales** | Las cuatro iniciativas de I-36 y ahora I-37A declaran `catalogs-data` y `architecture-kernel` para cubrirlo. Puede estar bien; conviene decidirlo explícitamente en vez de heredarlo |

## Huecos de I-36 detectados al construir sobre él (hallazgo de I-37A, fuera de alcance)

Ninguno bloqueó I-37A: se rodearon dentro del namespace Cantilever, sin tocar I-36.

1. **`SectionRepresentationOptions.Detail` no lo consume el constructor del plan.** La opción existe y
   `StructuralSectionPlanBuilder` no la usa: el detalle real es el de la geometría que se le pasa. Es un
   parámetro que promete algo que no hace. Defecto acotado de I-36B.
2. **No existe composición de varios prismas ni envolvente de conjunto.** I-36 resuelve una sección a la
   vez; `Bounds2D.Union` es la única primitiva de unión y es 2D. I-37A declaró `CantileverEnvelope3D`
   **local a su namespace** en vez de añadir un bounds 3D transversal: un contrato compartido no debería
   nacer por la fuerza de un solo consumidor. Si aparece un segundo, promoverlo es un movimiento; retirar
   una abstracción prematura sería una migración.
3. **`SectionViewpoint.PreservesSectionShape` compara la cámara contra el Z del MUNDO**, no contra el eje
   del miembro. Es correcto en I-36, donde el marco por omisión es `LocalFrame3D.World`; deja de serlo para
   un miembro con marco rotado —la base de un cantilever lo está—. I-37A no dibuja y no lo consume, pero la
   subiniciativa de vistas **debe** calcularlo por su cuenta y **no** tocar I-36 para arreglarlo sin
   autorización expresa.




## I-42 — Push Back compuesto: hallazgos fuera de alcance

> Los hallazgos que esta sección listaba tras las dos primeras entregas —rangos no anidados, cotas del lateral,
> planta con todos los niveles corridos, reconstrucción de módulos y la capacidad geométrica del hueco— **quedaron
> corregidos** en las rondas de corrección de I-42 y ya no son deuda. Lo que queda:

1. **Una ranura ausente en una posición INTERIOR** deja, en el corte frontal de ese lado, la línea de postes de su
   frontera: la regla de I-33 conserva siempre los bordes exteriores de un frente en blanco. Las ausencias del final
   sí se retiran, que es el caso habitual (`A=3`, `B=4`). Resolver también el caso interior exige distinguir
   «ausente» de «en blanco» dentro de `DynamicFrontActivation`, que es contrato del sistema **Dinámico**. Se
   validará visualmente con el dueño: es estructura física compartida, no un defecto de I-42.

2. **Una entrada de topología creada SOLO para guardar el fondo de una cama corrida fija la topología por defecto
   vigente en ese momento.** `PushBackCompositeDesign.SetCorridaDepth` crea la entrada con
   `Topology = DefaultTopology`, de modo que si más adelante cambiara el default del rack esa celda se quedaría
   anclada al anterior. Hoy no hay ningún camino de producción que cambie el default —el editor lo deja fijo en
   *encontradas* / `A→B` y sólo la API lo permite—, así que no es un defecto observable. Resolverlo bien pide una
   topología **anulable** en la celda (el documento ya la guarda como texto y cae al default cuando no la reconoce),
   y eso toca los cuatro límites de persistencia: es una migración, no un ajuste.

## I-43 — Selectivo por alcance y fondos: follow-ups fuera de alcance

> Registrados en el **Gate 8.6A** de la corrección post-Gate 8 de I-43, a partir de la revisión
> arquitectónica (Gate 8.5) del candidato `de100ed`. **Ninguno de estos se implementa en I-43**: el plan
> aprobado los excluye expresamente y tocarlos "de paso" rechaza el gate. El contrato vigente es
> [ADR-0032](adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md); el alcance, el
> [contrato de I-43](initiatives/I-43-selectivo-scopes-fondos.md). Nada de lo de abajo está roto hoy.

### Deuda arquitectónica del Selectivo diferida (ARQ-43)

1. **ARQ-43-10 — el lector "efectivo" de cabecera muta durante una lectura.** Resolver la profundidad de
   una cabecera custom la escribe en la configuración y reconstruye su modelo físico, es decir, una
   lectura muta. Lo correcto es imponer la profundidad **una sola vez, en la frontera de escritura**, y
   que el lector sea puro; además, la copia profunda debería ser obligatoria en esa frontera. El Gate
   8.6F deja preparado el lector puro (`UsableCustomAt`) pero **no retira** la imposición del lector
   efectivo: eso es este follow-up.

2. **ARQ-43-11 — el valor global de elevación sigue en Domain.** Tras I-43 la autoridad es el frente; el
   global solo se conserva como compatibilidad de **lectura** para documentos antiguos. Retirarlo del
   modelo de dominio exige decidir qué hacer con los documentos que aún lo llevan, y por eso queda
   fuera.

3. **ARQ-43-12 — la ventana del Selectivo hace trabajo de aplicación.** La carga de un diseño, el número
   de fondos y los peraltes por poste viven en `RackSelectiveWindow`; los campos `WorkingDepth`,
   `WorkingCabeceraOverride` y `Fondo` de las entradas de diseño sobreviven solo por diff mínimo (I-43
   los alimenta desde el slot en vez de las cajas, pero no los borra). Moverlo a Application es un
   refactor con superficie propia.

4. **ARQ-43-14 — reglas duplicadas de destinos.** Hay varios tipos de resultado y varios resolvers que
   responden a la misma pregunta; la existencia de un fondo se decide en más de un sitio; el fallback
   cuando un conjunto explícito se queda vacío conserva el modo explícito apuntando al fondo visible; y
   la normalización de la selección merece unificarse. I-43 **caracteriza** ese fallback con pruebas,
   no lo cambia.

5. **ARQ-43-15 — superficie legacy y helpers muertos.** Alcance legacy, añadir/quitar nivel, la
   aplicación de altura por destino, el parser de texto de fondos (ya sin UI que lo use), el
   `Describe` con índices 0-based, el commit del cuadro de texto enfocado de la matriz, el registro de
   aplicaciones de frente y varios helpers de prueba sin usar. Es limpieza: no entra en una corrección.

6. **ARQ-43-16 — costuras de diálogo y duplicación en pruebas.** Faltan seams para los diálogos modales,
   hay aserciones tautológicas y helpers duplicados entre suites.

7. **ARQ-43-17 — política de versionado de esquema.** Un build anterior que lee un documento nuevo
   ignora los campos aditivos en silencio. Es la política preexistente del repo y no la cambia I-43;
   decidirla de verdad es un ADR aparte.

8. **ARQ-43-19 — estado de frente dedicado.** Un tipo propio para el estado de un frente (en lugar de
   listas paralelas por matriz) simplificaría el editor, pero es exactamente el refactor transversal
   que el plan prohíbe.

### Producto: pendientes conocidos del Selectivo

9. **ID12 — tope de tarima por lado (SPLIT).** El tope de tarima debe poder ser izquierdo, derecho o
   ambos. Queda **dividido**: la parte de modelo y colocación es una iniciativa propia, no un ajuste
   del editor. No se implementa en I-43.

10. **ID13 — frentes en blanco del Selectivo.** Igual que en el Dinámico, un frente debe poder quedar en
    blanco. Toca activación de frentes y cortes, y por eso no cabe en una corrección de alcance.

11. **Aviso "los tramos no caben" y el botón de medio frente tras un cálculo nulo.** Cuando el cálculo
    de tramos devuelve nulo el frente se dibuja completo, pero el usuario no recibe aviso y el botón de
    medio frente sigue ofreciendo la acción. Es una mejora de UX, no un defecto de dibujo.

12. **El resumen del editor describe siempre el fondo 0.** La línea de resumen se calcula sobre el
    primer fondo aunque el usuario esté editando otro. No afecta al dibujo ni al BOM.
