# Ideas a futuro y deuda técnica conocida

> Actualizado: 2026-07-15 (rejilla de seguridad resuelta y backlog de escalabilidad consolidado).
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
   y conteo vivo; falta PLANTA. **Desviadores y poste tope HECHOS. Pendiente:** guardas traseras.
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
15. **Manifest de biblioteca DWG** — guardar junto al DWG una version/hash y la lista de bloques/parametros esperados.
    Asi un catálogo y una biblioteca incompatibles fallan antes de producir un dibujo parcial.
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
