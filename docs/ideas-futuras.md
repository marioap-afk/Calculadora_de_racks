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
16. **CI por capas** — **ABSORBIDO por [I-45](initiatives/I-45-test-validation-workflow.md) (2026-09-04).**
    El enunciado original se conserva: ejecutar Domain/Application/tests en cualquier runner y reservar un smoke
    test Windows con AutoCAD para releases; el Plugin no debe impedir que las reglas puras tengan gate continuo.
    Entra en I-45 como **problema y como alternativa a estudiar**, no como solución adoptada: absorberlo **no**
    decide que la estrategia final sea CI por capas. **Resuelto en I-45 (2026-09-07):** la estrategia NO es
    CI por capas ni un carril rápido. El CI sigue ejecutando las dos suites completas en cada push; lo que
    cambió es **dónde** se produce la evidencia intermedia de UI (LC-UI) y **cuándo** se recolecta cobertura.
    Un CI rápido separado solo se reabre con los criterios reproducidos en
    [I-45-conformance.md](initiatives/I-45-conformance.md) §9 —cuya fuente es ADR-0033 §13, hoy en
    estado `propuesto`—, no por conveniencia.
16.bis **Advertencias de analizadores xUnit en las suites de prueba — DEUDA CON REGISTRO, no excepción**
    (hallazgo de I-45 `G0B`, medido de nuevo en `G7`, 2026-09-07).

    **Cifra reproducible: 30 advertencias propias**, únicas por archivo+línea+código:
    `xUnit2031` 14, `xUnit2029` 9, `xUnit2013` 6, `xUnit2000` 1. Se obtienen con:

    ```powershell
    dotnet build tests/RackCad.Tests/RackCad.Tests.csproj    -c Debug -t:Rebuild -v:minimal
    dotnet build tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug -t:Rebuild -v:minimal
    ```

    y contando líneas `warning <código>` **deduplicadas por archivo+línea+código**.

    > **Corrección de una cifra propia.** `G0B` registró «60 (28/18/12/2)» en el cuerpo de su commit.
    > Eran **líneas de aviso**, no advertencias distintas: MSBuild las emite dos veces, y el desglose
    > nuevo es exactamente la mitad en **cada** categoría. El número correcto es **30**. Se corrige aquí
    > en vez de dejarlo vivir en un mensaje de commit, que no es un artefacto consultable.

    **Por qué se difirió y sigue diferida:** corregirlas **reescribe aserciones**
    (`Assert.Empty` → `Assert.DoesNotContain`, `Assert.Equal(1, x.Count)` → `Assert.Single`) en ~25
    archivos. Eso es trabajo propio con su propio riesgo, no higiene de paso, y `G0B` tenía prohibido
    tocar aserciones. **La meta de `AGENTS.md` de 0 advertencias propias NO se debilita y esto NO es una
    excepción permitida.** Los `MSB3277` del Plugin siguen bajo la excepción que ya existía.


16.ter **Nadie hace cumplir automáticamente «0 pruebas seleccionadas = FALLO»** (hallazgo de I-45 `G7`).

    `AGENTS.md` volvió normativa esa regla en `G0A` y anunció que el mecanismo automático sería
    «trabajo de un gate posterior de I-45». **Ese gate no existe:** `G7` es el último del plan, y
    ninguno construyó el mecanismo. Hoy la obligación es **enteramente de quien ejecuta**; el único
    sitio donde la guarda es automática es `eng/validation/measure-validation.ps1`, y solo sobre sus
    propias corridas de medición.

    Se registra aquí para que la promesa no se evapore. **La regla sigue siendo normativa** —lo que
    falta es su comprobación automática, no su vigencia—. Un gate o una iniciativa futura puede
    tomarlo; ninguno lo tiene asignado.

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
     **Resuelto**: en Push Back, por I-35 (reconciliación por `ModuleId + Kind`); en el Dinámico, por I-53D
     (2026-09-13), que reconstruye por `DynamicRackRebuild` y retiró el par ordinal. Los comentarios de código que
     aún lo citan quedan en **N-05**, más abajo.
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
  cambiaría su comportamiento y necesita decisión del Owner. **Decidido el 2026-11-26** («Sí, corrígelo en
  Dinámico») y **corregido en la cuarta tanda de I-64, parte seg**: la marca persistida
  `SelectiveDesviadorConfig.CellsByPost` lleva el Dinámico a la misma regla de `CellKey`; un rack nuevo la
  trae, y uno existente se lee como antes hasta que su desviador se edita en la ventana (ver `construido.md`
  de la ventana RackCad, «Corrección del desviador de Dinámico»).
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

### Deuda registrada por la SEGUNDA revisión arquitectónica (Gate 8.9)

13. **R2-04 — celda pendiente y commit estructural en escenarios que el arreglo no cubre.** El Gate 8.6H hizo que un
    gesto estructural que compromete una celda recalcule, pero la segunda revisión dejó anotados escenarios vecinos
    —otras combinaciones de campo pendiente y celda editada— que no se cubrieron. No hay defecto observado: es
    cobertura por ampliar.

14. **R2-09 — visibilidad del lector crudo de cabeceras.** El plan preveía bajar `CustomAt` a `internal`; se conservó
    su visibilidad para no cambiar API por estética. Desviación aceptada y registrada, no deuda funcional.

15. **ARQ-43-08B — limpieza de código muerto del Selectivo.** El Gate 8.6G corrigió solo los textos que afirmaban un
    contrato falso (08A). Retirar el código que ya nadie usa quedó expresamente fuera.

16. **UX de «Fondo de tarima» / «Fondo de cabecera» al navegar entre fondos.** El dueño reportó una vez que las cajas
    parecían no seguir al fondo visible. **No se pudo reproducir**: ocho pruebas con gestos reales —teclear, cargar
    con `LoadForNew` y con `RACKEDITAR`, dos y tres fondos, navegación repetida— dan el valor correcto del fondo en
    pantalla. Si el comportamiento vuelve a verse, hace falta el escenario exacto: cómo se fijaron las profundidades,
    qué decía «Fondos destino» y por qué punto de entrada se abrió el editor.

17. **Vocabulario de la BOTA en el sistema DINÁMICO — pendiente de decisión del dueño.** El Selectivo volvió a
    `Ninguno / Izquierda / Derecha / Ambas` en el Gate 8.6H, que es como se llamó siempre. El Dinámico se quedó con
    `Ninguno / Entrada/Salida / Posterior / Ambas`, que llegó como efecto colateral de I-42 (Push Back) y **no tiene
    ninguna decisión registrada**. Se conservó a propósito, declarado de forma explícita en su llamada y fijado con
    una prueba de caracterización. Push Back mantiene el suyo en su propia sección.

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
||||||| 085ca2f

## I-44 — AMBIGÜEDAD DE PRODUCTO: quién gobierna el intermedio de una cama CORRIDA (2026-09-03)

**Pendiente de DECISIÓN DEL DUEÑO. No es un defecto con arreglo obvio: es una pregunta de producto que
nadie ha contestado todavía, y por eso I-44 no la tocó.**

Una cama **corrida** es **una sola cama** que cruza la interfaz del rack compuesto. Su larguero
intermedio es una pieza física única, así que su `IntermediateBeamCatalogId` y su `IntermediateBeamDepth`
sólo pueden salir de **un** lado. Cuando A y B declaran valores distintos, ¿cuál manda?

**Hoy manda el lado BAJO, y no porque se decidiera.** `PushBackRuns.BuildCorrida` copia al frente
sintético los `DynamicRackLevel` del lado bajo (`elevations = lowFront ?? highFront`) porque las
**ELEVACIONES** son de ese lado —esa sí fue decisión del dueño en I-42, commit `82e918b`, «corrida anclada
en su lado BAJO»—, y ese mismo objeto transporta además el par del intermedio, la tarima, el claro y el
IN/OUT. Antes de `82e918b` la fuente era `highFront`, así que la autoridad del intermedio **ya cambió de
lado como efecto colateral** de una decisión sobre otra cosa.

En el mismo `BuildCorrida` conviven dos elecciones **explícitas y opuestas**: las elevaciones salen del
lado **bajo** y el peralte del larguero **alto** sale del lado **alto** (`highResolved.HighEndBeamPeraltes`).
El intermedio nunca se nombró en ninguna de las dos.

**Qué se revisó buscando un contrato previo, sin encontrarlo:**

- [ADR-0031](adr/0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md) §8-bis dice sólo
  que un intermedio pertenece a **una cama** y que BOM y plano usan el mismo builder. No nombra al dueño
  del par cuando A y B discrepan.
- `PushBackCompositeCorridaAuthorityTests` (I-42, ronda 4, «pruebas vinculantes») consagra el **fondo
  propio**, el **ancla en el apoyo real** y la **reversibilidad**. Nada del peralte intermedio.

**Efecto observable.** Con A (bajo) = 3.5" y B (alto) = 4.5", el BOM publica `[3.5]` y el 4.5" authored
desaparece. Es un descenso que ningún máximo explica, y es **independiente** del defecto que I-44 corrigió
(aquél sólo podía subir el peralte).

**Estado.** I-44 **congeló** el comportamiento actual con una prueba de caracterización,
`PushBackIntermediateBeamPeralteTests.Characterization_ACorridaTakesTheLowSideCellValues_ContractPending`,
que además comprueba la simetría: intercambiar los lados intercambia el resultado, de modo que gobierna el
lado bajo y no «el mayor» ni «el que lo declaró primero». Esa prueba **no es especificación**: fija lo que
hay para que un cambio futuro sea deliberado y visible. Cambiar la semántica exige la decisión del dueño y,
por su alcance, probablemente un ADR.

---

## I-47 — hallazgos y dirección futura (2026-09-09, registrados sin implementar)

### Deuda técnica: `ToProjectVariables()` no es autoridad del tipo persistido — **RESUELTA por I-48 G4D (2026-09-12)**

*Registro histórico. `ProjectVariablesDocument.ToProjectVariables()` hardcodeaba `VariableType.Length` e
ignoraba el campo `Type` del registro. Era inocuo mientras ID22A soportara un solo tipo, pero un tipo
futuro leído como `Length` habría ofrecido para vincular una variable cuyo número significa otra cosa, y
el vínculo habría resuelto sin fallar.*

**Cómo quedó.** La proyección lee el `Type` persistido a través de **la misma** primitiva
`token → VariableType` que usan el store y la factoría del registro —una sola tabla, no tres— y **falla
cerrado**: un token que esta versión no soporta lanza, nombrando el `VariableId` y el token, en vez de
inventar un tipo, saltarse la entrada o reinterpretarla.

**Alcance real del arreglo, dicho sin inflar.** `ToProjectVariables()` **no tiene hoy ningún consumidor
en `src/`** —sólo pruebas—, porque G4B y G4C movieron la lectura del registro a otras superficies. Así
que esto no cambió ningún comportamiento observable: cierra la trampa **antes** de que exista un segundo
`VariableType` que la active. Esa parte sigue siendo futura.

### Project Variables — Excel-like Property Input UX — **ENTREGADA por I-48 (2026-09-12)**

**Objetivo.** Reemplazar progresivamente los controles específicos de vínculo —hoy un `ComboBox` de
variables compatibles más botones **Vincular** / **Desvincular**, sólo para
`selective.verticalClearance`— por **un único input reutilizable** para cualquier propiedad vinculable.

**Concepto.** El mismo campo acepta las dos cosas, y el marcador es `=`:

```text
6                    → Literal(6)
=Holgura General     → ProjectVariableReference(VariableId)
```

Al escribir `=`, el control entra en modo referencia y ofrece autocompletado de las variables
**compatibles**, mostrando nombre **y** valor:

```text
Holgura
[ = ]

  Holgura General     10 in
  Holgura Cliente A    8 in
  Holgura Especial    12 in
```

**Alcance tentativo.** Input numérico inteligente · modo literal · modo referencia con `=` ·
selector/autocompletado de variables · **presentación por nombre, autoridad por `VariableId`** ·
transición literal ↔ variable **mediante intents** · reutilizable en varias propiedades · **sin cambiar
la persistencia de ID22A**.

**Reglas que NO puede saltarse** (son las de I-47, y siguen valiendo):

- se persiste `ProjectVariableReference(VariableId)`, **nunca** el texto `"=Holgura General"`;
- los **nombres duplicados siguen permitidos**, así que la lista debe dar contexto suficiente para
  distinguir dos homónimas;
- **renombrar** deja el `VariableId` y el vínculo intactos; la UI sólo vuelve a dibujar `=NuevoNombre`;
- literal → variable es un **`Link`** (el literal queda congelado);
- variable → literal es **explícito**: `Unlink` —que materializa el efectivo actual— y, sólo después, un
  `SetLiteral` si el usuario escribió otro número. **Nunca** editar `PropertyValues` directamente ni caer
  a un valor por defecto en silencio;
- el input no puentea el preflight semántico ni el ejecutor.

**Compatibilidad con ID22B, como dirección y no como permiso.** El mismo campo admitiría más adelante
`=Holgura General + 2`. Eso **no autoriza** añadir ahora parser, AST, fórmulas, grafo de dependencias,
detección de ciclos, persistencia de expresiones ni referencias a propiedades de otros racks (**ID21**).

**Cómo quedó.** I-48 numeró y reclamó esta dirección y entregó su núcleo: un **único control
reutilizable** (`LinkedPropertyEditor`) que acepta literal o `=Nombre` en el mismo campo, con
autocompletado de las variables compatibles **sin auto-selección**, sufijo corto del `VariableId` como
desambiguador **obligatorio** cuando dos variables se llaman igual, y transición literal ↔ referencia
por intents. Lo consumen **dos** propiedades reales: `selective.verticalClearance` —que dejó de ser un
caso especial— y `selective.palletTolerance`. Las reglas de I-47 listadas arriba se conservaron todas.

**Lo que sigue fuera, y no lo autoriza nada de lo anterior**: fórmulas (**ID22B**) —ni parser, ni AST,
ni grafo de dependencias, ni detección de ciclos—, referencias rack a rack (**ID21**), y generalizar el
mecanismo a los otros sistemas (Dinámico, Push Back, Cama de rodamiento, Cantilever).

## I-48 — hallazgo fuera de alcance (2026-09-12, registrado sin implementar)

### Selectivo / compatibilidad de catálogo: un `BeamId` persistido que ya no existe no se recupera

**Hallazgo fuera de I-48. NO implementado. Candidato a fix o iniciativa separada.** No es un defecto
resuelto: sigue vivo en `main`.

`RackSelectiveWindow.LoadCellEditor` asigna el larguero guardado del diseño directamente al combo:

```text
CellBeamBox.SelectedValue = cell.BeamId;     // sin red de seguridad
RefreshPeralteCombo(cell.BeamPeralte);       // PeralteOptions(null) -> lista vacía
```

Si ese `BeamId` **no está en el catálogo cargado** —porque el larguero se renombró o se retiró; el
catálogo compartido es de sólo lectura y puede cambiar **bajo** diseños ya guardados— WPF deja el combo
**sin selección** y los peraltes se vacían. A partir de ahí, «Actualizar» e «Insertar» no pueden
completarse: `ReadCellEditor` rechaza la celda y el editor pide *«Selecciona un larguero.»* en un diálogo
modal, sin decir que el problema es que el larguero del rack ya no existe.

**La asimetría es lo que lo delata**: el **poste** sí tiene red, y en dos sitios
(`if (PostBox.SelectedItem == null && PostBox.Items.Count > 0) PostBox.SelectedIndex = 0`), tanto al
construir la ventana como en `LoadDesign`. El larguero no tiene ninguna, y el constructor sí lo
preselecciona —`CellBeamBox.SelectedIndex = 0`— sólo para que `LoadCellEditor` destruya esa selección
después.

**Qué habría que decidir, no dar por supuesto**: si el comportamiento correcto es caer al primer
larguero del catálogo (como hace el poste, silenciosamente) o **reportar** que el diseño referencia un
larguero desaparecido. Lo segundo parece mejor —un rack no debería cambiar de larguero sin que nadie lo
diga— pero es decisión de producto.

### Relacionado: `ConfirmPendingCellEdits` usa `MessageBox` directo, sin costura de prueba

Las dos llamadas de `ConfirmPendingCellEdits` —y las de `TryCommitEditedCell` y `RequestDraw`— van a
`MessageBox.Show` **sin** la costura reemplazable que el repositorio ya usa para esto en
`SelectiveCabeceraHeightPrompt` (I-43 gate 8.6E) y `EditorDiscardPrompt`. Consecuencia medida en I-48
G4G: cualquier estado que llegue a uno de esos avisos **cuelga** un agente headless hasta que
`--blame-hang` aborta el testhost, y el síntoma no es un fallo legible sino una corrida abortada. Ese
cuelgue costó tres gates de diagnóstico y sólo se resolvió leyendo los minidumps.

**No implementado tampoco.** Ponerlos detrás de la costura existente es barato, pero es cambio de
producción y no entra en el cierre de I-48.

## I-50 — hallazgos fuera de alcance (2026-09-12, registrados sin corregir)

Encontrados en el Discovery de I-50 ([I-50-discovery.md](initiatives/I-50-discovery.md)) y declarados
**fuera de alcance** por la decisión `CD-09` del Coordinador. **Ninguno está corregido**: siguen vivos en
`main`. Las líneas se refieren a `a4d88f1`.

### H1 — Push Back: el estilo de cota no se puede elegir y cada recálculo lo borra

La ventana de Push Back solo tiene el combo de nivel (`DimensionsBox`, `RackPushBackSystemWindow.xaml:73`):
no hay combo de estilo, y `EditPushBack` no lee los estilos del dibujo, cosa que sí hacen Selectivo y
Dinámico con `RackCommandSupport.ReadDimensionStyleNames`. Al cargar, `PushBackEditorState.Load.cs:208`
copia el estilo guardado a las opciones, pero `ReadInputs` construye opciones nuevas **sin** estilo
(`RackPushBackSystemWindow.xaml.cs:810-817`) y `DynamicEditorDesignAssembler.cs:175` las asigna al diseño:
cada recálculo deja `DimensionStyle = null`. Un Push Back que traiga un estilo lo pierde en cuanto se abre
con `RACKEDITAR`, y «Actualizar» escribe `null` en todas sus vistas; el dibujo pasa a usar el estilo vigente
del DWG.

**Qué habría que decidir**: si Push Back debe ofrecer el mismo combo que el Dinámico, o si su estilo es
deliberadamente automático, en cuyo caso conviene no persistir un valor que nadie puede elegir.

### H2 — Push Back: el lateral envía la posición del corte y el Plugin la lee como poste

`RackPushBackSystemWindow.xaml.cs:3005` pide el lateral con `Section = LateralSectionBox.SelectedIndex`, que
es la **posición** en la lista `LateralCortes` (`:2976-2983`). El Plugin trata ese número como **índice de
poste** (`RackPushBackCommands.cs:284-285`), y `PushBackSystemLateralBuilder.Cortes` omite postes
(`PushBackSystemLateralBuilder.cs:91`, `continue` en `:107`). En cuanto se omite un poste, posición e índice
pueden diferir y se insertaría otro corte. *(Sin verificar en ejecución.)*

### H3 — Bloques anónimos `*D` de las cotas que no se encolan para purga

Al redefinir, `LateralHeaderDrawer` registra el `DimBlockId` de cada cota para purgarlo
(`LateralHeaderDrawer.cs:107-111`). Los otros dos caminos que borran vistas solo recogen las definiciones de
bloques anidados, no los `*D` de las cotas: la inserción cancelada (`BlockPlacement.cs:133-162`) y
`RackCommandSupport.EraseViewBlocks` (`RackCommandSupport.cs:210-250`). Si AutoCAD los elimina al guardar,
el efecto es nulo. *(Sin verificar en ejecución.)*

### H4 — `HeaderRunPlan.PlacedClone` no copia los campos de texto y de cota

`PlacedClone` (`HeaderRunPlan.cs:53-72`) copia rol, pieza, bloque, vista, rotación, espejos y puntos, pero
no `Text`, `TextHeight`, `DimensionOffset` ni `DimensionStyleName`. Hoy es inocuo: cotas y etiquetas quedan
siempre sueltas (`HeaderInstanceGrouper.cs:35-43`) y `Flatten` las añade sin clonar (`HeaderRunPlan.cs:49`).
Se volvería un defecto el día que una anotación entrara en un grupo ARRAY.

### H5 — El Dinámico descarta un estilo guardado que el DWG no tiene; el Selectivo lo conserva

`RackSelectiveWindow.SelectDimStyle` añade al combo un estilo guardado que falta en el dibujo, para que
sobreviva al volver a guardar (`RackSelectiveWindow.xaml.cs:1733-1746`).
`RackDynamicSystemWindow.SelectDimensionStyle` cae a «(Automático)»
(`RackDynamicSystemWindow.xaml.cs:263-282`), así que abrir y guardar un Dinámico en un DWG sin ese estilo
borra la elección. Asimetría de producto sin decisión registrada; relacionada con la entrada de I-15 sobre
los estilos de cota del Selectivo creado desde el menú (sección «Hallazgos de la revisión de código
2026-07-15»).

### H6 — El BOM del Selectivo calcula las cotas laterales y las descarta

`SelectiveBomBuilder` fuerza `None` para contar planta y frontal (`SelectiveBomBuilder.cs:60-68`, `:508`),
pero al contar separadores recorre `SelectiveLateralBuilder.Cortes` **sin** forzarlo (`:219`): cada corte
emite sus cotas con el nivel real del rack y el BOM las descarta al filtrar por rol. Es coste de
rendimiento, no de resultado.

### H7 — Comentarios desfasados

- `RackBlockData.cs:8` dice que el payload vive en el diccionario de extensión de la «block reference»;
  vive en el de la **definición** (`SystemBlockWriter.cs:13`). Ya lo señaló I-47 como H-01 en su Discovery,
  sin corregirlo.
- `RackInsertionRequest.cs:152` describe la `Section` frontal de Push Back como `(int)PushBackFrontalEnd`;
  desde I-42 es `(int)end + (B ? 2 : 0)` (`PushBackSystemFrontalBuilder.cs:138-139`).
- `PushBackSystemLateralBuilder.cs:147-151` dice que las decoraciones compartidas usan el contexto de
  elevaciones del lado A; el código emite las cotas por lado (`:156-158`, `:196-231`).

## I-51 — hallazgos fuera de alcance (2026-09-12, registrados sin corregir)

Encontrados en el Discovery de I-51 ([I-51-discovery.md](initiatives/I-51-discovery.md) §17) y declarados
**fuera de alcance** en su G2 ([decisiones de I-51](automation/decisions/I-51.md);
[contrato](initiatives/I-51-rackduplicar-multiples-origenes.md) §4). **Ninguno está corregido**: siguen
vivos en `main`. Las líneas se refieren a `a4d88f1`.

### L-1 — `RackBlockData`: el comentario dice «referencia» y los datos viven en la definición

El `<summary>` de `RackBlockData.cs:8-11` afirma que el payload se guarda en el diccionario de extensión de
una «block **reference**». Todos los llamadores escriben y leen en la **definición**
(`LateralHeaderDrawService.cs:258`, `SystemBlockWriter.cs:33` y `:114`, `RackCantileverCommands.cs:156` y
`:324`, `RackCloner.cs:66`; lectores en `RackDuplicarCommands.cs:170`, `RackCommandSupport.cs:93`,
`RackBlockFinder.cs:71` y `RackLayoutCommands.cs:191`). Es el mismo hallazgo que **H-01** del Discovery de
I-47 y que el primer punto de **H7** que I-50 registra en su rama: se anota aquí para que I-51 no lo pierda,
no como una deuda distinta.

### L-2 — `CantileverKindHandler`: documentación XML desplazada

`CantileverKindHandler.cs:70-78` tiene dos `<summary>` seguidos: la documentación del re-estampado quedó
pegada a `OutputBlockedReason`, y `RestampDesign` no tiene la suya. Cosmético.

### L-3 — `RACKLAYOUT` y `RACKRELLENAR` no conservan la capa ni la elevación del semilla

Las copias de `RACKLAYOUT` copian `Rotation` y `ScaleFactors`, pero no `LayerId`, y fijan **Z = 0**
(`RackLayoutCommands.cs:250-254`); `RACKRELLENAR` hace lo mismo (`RackLayoutCommands.Fill.cs:454-457`).
`RACKDUPLICAR`, en cambio, conserva la capa del origen (`RackDuplicarCommands.cs:205`).

**Qué habría que decidir**: si las copias de layout deben heredar capa y elevación del semilla, como las de
`RACKDUPLICAR`, o si su capa y su plano de trabajo son deliberados.

### L-4 — El selector de `RACKDUPLICAR` admite MINSERT y la copia sale como referencia simple

`RackDuplicarCommands.cs:151` usa `AddAllowedClass(typeof(BlockReference), exactMatch: false)`, que admite
`MInsertBlock`. La copia crea una `BlockReference` simple (`:201-206`), así que se pierden las filas y
columnas del MINSERT. I-51 **no** lo cambia: su contrato conserva el comportamiento histórico de una
referencia (INV-16).

## I-53 — hallazgos fuera de alcance (2026-09-12, registrados sin corregir)

Encontrados en el Discovery de I-53 ([I-53-discovery.md](initiatives/I-53-discovery.md) §19) y en su G2
([Proposal V2](initiatives/I-53-proposal-v2.md) §17), y declarados **fuera de alcance** en el freeze de G2
([decisiones de I-53](automation/decisions/I-53.md) §10.10; [contrato](initiatives/I-53-cabeceras-configurables-multidestino.md)
§4). **Ninguno está corregido**: siguen vivos en `main`. Las líneas de L-2..L-14 se refieren a `46fcac2`; las de
N-02 y N-03, a `f8deb67`. L-1 y L-7 no figuran aquí: están dentro de la línea I-53. *(2026-09-13: N-03 quedó resuelto por
I-53D; ver su entrada.)*

### L-2 — Push Back: `RACKBOMTOTAL` cotizaría sin celosía las cabeceras con override por línea

`BuildPushBack` refresca solo las cabeceras de módulo; el resolver clona los overrides con `CloneHeader`, sin
refresco; `BomBuilder` itera `Members`, vacío, y emite solo postes y placas. El BOM del editor (que sí refresca)
y el total divergirían. Ninguna prueba cubre esta ruta: la de BOM usa el ensamblador vivo
(`PushBackKindHandler.cs:40-50`; `SystemRegistry.Default.cs:172-189`; `DynamicRackSystemResolver.cs:226-238` y
`:512-516`; `RackFrameProjectDocument.cs:81-120`; `BomBuilder.cs:74-93`; `TC/PushBackDerivedPostAndLineTests.cs:388-403`).
*(TRAZA; no ejecutada.)* I-53 **no** lo corrige ni lo fija con una prueba: corregirlo toca el resolver
compartido y el comportamiento integrado de Push Back.

### L-3 — El comentario del resolver dice «COPIA canónica» y el código usa la no canónica

`DynamicRackSystemResolver.cs:212` frente a `:235`. Complementa la entrada existente «El clon del resolver no es
el clon canónico de I-17».

### L-4 — «Ausente cuando está vacío» es inexacto: las listas vacías se escriben como `null`

`DynamicRackSystemDocument.cs:47-56`; `RackProjectStore.cs:402-410`; `RackEmbedDocument.cs:70-74`. *(Hecho en
las opciones; inferencia sobre el comportamiento por defecto de System.Text.Json.)*

### L-5 — HANDOFF afirma que el Dinámico comparte `RackModuleEditSession`, y no la usa

`HANDOFF.md:2096-2098`.

### L-6 — Push Back: `SaveLibrary_Click` dice que confirma la sesión y solo recalcula; `Bom_Click` tampoco confirma

`RackPushBackSystemWindow.xaml.cs:3304-3337`.

### L-8 — I-40 prometió informar omisiones en la ronda 2 y su modelo final no lo hace

Discovery de I-53, §9.10, punto 1.

### L-9 — Comentarios del Selectivo que siguen hablando de la «representación master del fondo 0» en la frontal

La prueba vigente `TC/SelectiveFondoCabeceraTests.cs:680` fija que cada frontal representa **su propio** fondo;
los comentarios de `SelectiveEditorState.cs:1304-1305`, `SelectiveBomBuilder.cs:422-423`,
`RackSelectiveWindow.xaml:170` y `SelectivePalletDesign.cs:41-42` siguen describiendo el modelo anterior.
*(Hecho en la prueba; los comentarios, según la auditoría del Discovery.)*

### L-10 — WORKFLOW §7 desactualizado

En `46fcac2`, `RackSelectiveWindow.xaml.cs` tenía **3.159** líneas (la tabla dice 2.867) y el patrón
`*Commands*.cs` del Plugin daba **16** archivos (dice 14). Conteo directo.

### L-11 — Marcador de conflicto diff3 en este mismo archivo

La línea 813 de `docs/ideas-futuras.md` conserva un marcador de conflicto diff3 (siete barras verticales seguidas
de `085ca2f`), introducido por el merge `d582dee` (I-43 incorporando `main`); lo muestra `git blame -L 813,813`.
Sigue presente en `f8deb67`.

### L-12 — `RackModuleHeaderScope` no tiene consumidor: solo aparece citado en comentarios

`RackModuleHeaderApply.cs:19`; búsqueda con `git grep`.

### L-13 — `RackCommandReference` (RACKAYUDA) no lista `RACKPUSHBACK`, `RACKSECCION` ni `RACKVARIABLES`

Búsqueda sin coincidencias en `src/RackCad.UI/RackCommandReference.cs`. Amplía con `RACKVARIABLES` la entrada
existente «Dos comandos ausentes de la referencia en la app (hallazgo de I-36B)».

### L-14 — Push Back: los overrides por línea no se podan por `PostIndex`

Sobreviven a un cambio de tipo de cabecera, no adaptan fondo ni peralte y su descarte no se informa
(`PushBackEditorDesignAssembler.cs:348-364`). Es comportamiento integrado de I-40: observación, **no** reapertura.

### N-02 — Dinámico: «Calculada» regenera la configuración y también borra la longitud manual

`ConfigBox_SelectionChanged`, en su rama «Calculada», regenera la cabecera y fija `IsManualOverride = false`
(`RackDynamicSystemWindow.xaml.cs:1876-1881`, `:1879`). Restablecer la **configuración** borra así también la
**longitud manual** del módulo, que la reconciliación lee de ese flag (`RackModuleReconciliation.cs:164`). Es la
misma sobrecarga que I-53 corrigió para ID6/ID7 (H-04), en sentido inverso, pero es un restablecimiento y no una
reutilización: I-53 lo **caracteriza sin cambiarlo** (prueba D-26).

**Qué habría que decidir**: si restablecer la cabecera a «Calculada» debe conservar la longitud manual, ya que
ADR-0037 fija que `IsManualOverride` significa longitud manual.

**Sigue vigente tras I-53D (2026-09-13).** G7 conserva sin cambios la rama «Calculada» —en `a57bd50`,
`ConfigBox_SelectionChanged` en `RackDynamicSystemWindow.xaml.cs:2331-2358`, con la asignación en `:2344`— y la
caracteriza con `D26_Calculada_RegeneraLaCabecera_YQuitaLaLongitudManual_ComoAntes`. La decisión sigue pendiente.

### N-03 — Dinámico: el desplegable de configuración muestra siempre «Calculada» — **RESUELTO por I-53D G7 (2026-09-13)**

Al seleccionar un módulo, `UpdateSelectedPanel` llama a `SelectConfigCalculated`
(`RackDynamicSystemWindow.xaml.cs:1823`), que pone el índice 0 sin mirar la procedencia (`:1844-1860`): un
módulo personalizado aparece como «Calculada». Es una lectura engañosa, no una reutilización. I-53D decidirá la
forma de ese control al retirar los presets «Personalizada N» (OD-8).

**Cómo quedó.** Al retirar los presets (OD-8), «Configuración de cabecera» pasó a mostrar la **procedencia** del módulo:
«Calculada» o «Personalizada», esta última no seleccionable, porque solo se obtiene editando la cabecera o aplicándole un
origen (`SelectConfigProvenance`, `RackDynamicSystemWindow.xaml.cs:2312-2325` en `a57bd50`; ítems en
`RackDynamicSystemWindow.xaml:311-312`). Un módulo personalizado ya no aparece como «Calculada»; lo fija
`D26_LaPersonalizacionSeVeComoProcedencia_NoComoLongitudManual`.

## I-53S — hallazgo fuera de alcance (2026-09-13, registrado sin corregir)

Detectado en el Candidato de I-53S (E2-C, sobre `e528ef2`) y registrado en su cierre
([decisiones de I-53](automation/decisions/I-53.md) §12.5). **No está corregido**: el Candidato validado ya estaba fijado y
el cierre de E2 no admite cambios en `src`. Continúa la numeración de hallazgos de la línea I-53.

### N-04 — Selectivo: el XML-doc de `ResolvedCabeceraFondo` atribuye la profundidad guardada a `ApplyCabeceraToTargets`

`RackSelectiveWindow.xaml.cs:1287-1289` (en `e528ef2`) dice que la profundidad final de lo que se guarda la sigue
imponiendo `ApplyCabeceraToTargets` por fondo destino. Desde I-53S, «Personalizar» es el EDIT del lote: la profundidad de cada copia la
impone PREPARE (`SelectiveHeaderBatchPlanner`, con `SelectiveCabeceraAuthority.ImposeFondoDepth`), y
`ApplyCabeceraToTargets` solo lo usa «Restablecer poste». Es solo un comentario, sin efecto en el comportamiento; el texto
viene de I-43 y quedó desfasado con el G5 de I-53S.

**Qué habría que hacer**: alinear ese comentario con PREPARE, sin cambio de comportamiento, en la próxima iniciativa que
toque la ventana.

## I-53D — hallazgos fuera de alcance (2026-09-13, registrados sin corregir)

Detectados en G7 y en el Candidato de I-53D (E3-C.1, sobre `a57bd50`) y registrados en su cierre
([decisiones de I-53](automation/decisions/I-53.md) §13). **No están corregidos**: el Candidato validado ya estaba fijado y
el cierre de E3 no admite cambios en `src` ni en `tests`. Continúa la numeración de hallazgos de la línea I-53; las líneas
se refieren a `a57bd50`.

### N-05 — Comentarios y XML-doc que siguen citando el par ordinal retirado

I-53D retiró `DynamicEditorDesignAssembler.SnapshotHeaderFondos` / `RestoreHeaderFondos`, pero varios textos siguen
hablando de él como vigente:

- `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs:17-22`: el resumen de la clase enlaza con
  `<see cref>` los dos métodos, que ya no existen; `:117` describe una reconstrucción que restaura los fondos «by ordinal»,
  y `:307-310` dice que el par «is also the dynamic editor's».
- `src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs:81-86`, `:94-96` y `:104-105`: presenta el par como la
  reconciliación «base» del Dinámico y dice que cambiarlo cambiaría ese editor.
- `src/RackCad.Application/Systems/Dynamic/DynamicRackRebuild.cs:118-119`: «G6 does not wire the window, which keeps its
  historical ordinal pair until G7».
- Pruebas: el `<see cref>` de `tests/RackCad.Tests/PushBackEditorCorrectionTests.cs:356` y el comentario de `:219`; en
  `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs`, la documentación del `Fact5` de Push Back
  (`:146-154`, «El DINAMICO conserva el comportamiento historico»), el comentario de `Fact3` (`:361`) y la documentación de
  `Fact7` (`:465-467`, que aún justifica el snapshot `beforeEdit` por el preset «Personalizada N»); el resumen de
  `tests/RackCad.Tests/DynamicHeaderBatchReconciliationTests.cs:16`; y `tests/RackCad.Tests/RackModuleEditSessionTests.cs:550-551`.

Es solo documentación en código, sin efecto en el comportamiento: las aserciones de esas pruebas siguen vigentes y verdes.

**Qué habría que hacer**: alinear esos textos con `DynamicRackRebuild`, sin cambio de comportamiento, en la próxima
iniciativa que toque esos archivos. Los textos históricos de `docs/` —el contrato, el estado y la entrada del índice de
I-35, el contrato de I-21 y el Discovery de I-53— describen el estado de su momento y no se corrigen.

### N-06 — Dinámico: tras un fallo de recomposición, la reconstrucción siguiente no tiene intenciones que conservar

Si `RecomposeCore` lanza, su `catch` pone `system = null` (`RackDynamicSystemWindow.xaml.cs:582-590`). La recomposición
siguiente reconstruye —`DynamicEditorDesignAssembler.MustRebuild` es verdadero sin sistema (`:44-45`)— y
`DynamicRackRebuild` solo toma las intenciones del sistema previo si existe (`DynamicRackRebuild.cs:151`): sin él
reconstruye desde cero, así que las personalizaciones de módulos —cabeceras personalizadas y longitudes manuales— se
pierden y el informe de reconciliación no tiene nada que nombrar. *(Traza sobre el código; no ejecutada.)* Es comportamiento
previo a I-53D: el par ordinal tampoco tenía qué restaurar sin sistema. I-53D **no** lo cambia ni amplía su alcance para
incluirlo.

**Qué habría que decidir**: si una recomposición fallida debe conservar el último sistema válido, o reconstruir desde el
último diseño que la ventana conserva, para que la reconstrucción siguiente conserve e informe lo que pueda.

## I-54 — hallazgos fuera de alcance (2026-09-13, registrados sin corregir)

F-01..F-13 salen del Discovery de I-54 ([I-54-discovery.md](initiatives/I-54-discovery.md) §15; F-13 se añadió en su
addendum de G2A). F-14a y F-14b salen de las revisiones exact-SHA de sus Proposals V3 y V4 y quedan descritos en
[I-54-proposal-v5.md](initiatives/I-54-proposal-v5.md) D-08.4. El [contrato de
I-54](initiatives/I-54-propiedades-personalizadas.md) §4 los declara **fuera de alcance** y reserva su registro al
freeze de G2 ([decisiones de I-54](automation/decisions/I-54.md)). **Ninguno está corregido**: siguen vivos en `main`.
Las líneas se refieren a `46fcac2`, la base de I-54, y los archivos citados son idénticos en `f8deb67`.

### F-01 — `RackBlockData`: el comentario dice «block reference» (ya registrado)

Es el mismo hallazgo que **L-1** de I-51 y que el primer punto de **H7** de I-50, ambos más arriba, y que H-01 del
Discovery de I-47. I-54 lo confirma sin cambios en `RackBlockData.cs:8-11`. **No** es una deuda distinta y no se
duplica.

### F-02 — `RackCloner`: el comentario habla de un «cloned (old) payload» que no existe

`RackCloner.CloneDefinition` crea un `BlockTableRecord` nuevo, clona solo las entidades con `DeepCloneObjects` y
escribe el payload recibido (`RackCloner.cs:34-36`, `:38-50`, `:66`). El comentario de `:66` («replace the cloned (old)
payload») describe un payload clonado que no existe: según el Discovery, el diccionario de extensión de la definición
de origen no viaja *(inferido, sin verificar en ejecución)*. El Discovery de I-51 repite esa descripción
(`I-51-discovery.md:119`). Es cosmético, pero despista a quien razone sobre qué datos sobreviven a una copia
independiente.

### F-03 — I-47 describe `RACKCAD_PROJECT` como sub-diccionario; el código usa un Xrecord directo

La Proposal V4 de I-47 (D-01, A1: `I-47-proposal-v4.md:602`) y su plan (G7: `I-47-implementation-plan.md:418-419`,
`:427`, `:969`) describen «sub-`DBDictionary` `RACKCAD_PROJECT` → `Xrecord`». El código implementó un **Xrecord
directo** del NOD (`ProjectVariablesData.cs:36`, `:59-68`), y ningún documento registra ese cambio. No es inocuo como
descripción: migrar la entrada a un sub-diccionario la dejaría ilegible en todos los builds existentes y bloquearía
tres comandos (Discovery de I-54 §4.5 y §4.6).

### F-04 — `View` nulo se interpreta distinto según el consumidor

La documentación XML de `RackEmbedDocument.cs:40` dice que nulo es frontal; `RackListBuilder` lo trata como lateral
(`:117-118`); el Selectivo, como frontal; el Dinámico, como lateral legado; y la Cama siempre escribe `view: null`
(Discovery de I-54 §5.5, lectura de auditoría).

### F-05 — El despacho por `Kind` distingue mayúsculas en unos comandos y en otros no

`KindDispatch` ofrece una búsqueda ordinal y otra sin distinguir mayúsculas (`KindDispatch.cs:75-96`). `RACKEDITAR` y
`RACKBOMTOTAL` despachan de forma ordinal; `RACKLAYOUT`, `RACKDUPLICAR` y el re-estampado, sin distinguir mayúsculas.
Un `Kind` escrito con otra capitalización se trata distinto según el comando.

### F-06 — `RackListBuilder.KindLabel` sin caso Push Back (ya registrado)

Es el mismo hallazgo que la fila 7 de «Documentación desalineada con el árbol (hallazgo de I-37A)», más arriba; el
propio código lo declara hueco adyacente (`RackListBuilder.cs:92-95`). No se duplica.

### F-07 — `RACKVARIABLES` falta en la ayuda de la app

`RackCommandReference.Commands` (`RackCommandReference.cs:30-47`), la fuente de `RACKAYUDA`, no incluye `RACKVARIABLES`
ni su alias `RVA`; en `f8deb67` tampoco figura en la tabla de comandos de `README.md`. Es la misma clase de hueco que
«Dos comandos ausentes de la referencia en la app (hallazgo de I-36B)», que registra `RACKPUSHBACK` y `RACKSECCION`:
conviene resolver los tres en un mismo cambio.

### F-08 — La biblioteca omite en silencio un elemento de major incompatible

`RackDesignLibrary` recorre los `.rackcad.json`, y un `catch` genérico salta cualquier archivo que no pueda leer
(`RackDesignLibrary.cs:86-97`, con el `catch` en `:94-97`): un elemento de major incompatible desaparece del listado
sin diagnóstico. Contradice **C4.7-5** de I-47 ([decisiones de I-47](automation/decisions/I-47.md):557), según la cual
un archivo incompatible no se ofrece como diseño abrible pero **sí** produce un diagnóstico visible, y nada desaparece
en silencio.

### F-09 — Un Cantilever importado de la biblioteca podría conservar un `Line.Id` interior distinto del de su sobre

*(Inferido en el Discovery de I-54, sin verificar en ejecución.)* El `Id` interior de `CantileverLineDesign` se
re-estampa al hacer una copia independiente (`CantileverKindHandler.cs:105`), pero importar desde la biblioteca acuña
un GUID nuevo para el sobre y compone con `source = null`. Si ese camino no alinea también el `Id` interior, la línea
importada quedaría con dos identidades distintas, dentro y fuera.

### F-10 — INV-09 de I-51 (preservación del sobre en el re-estampado) sin prueba de comportamiento

Ninguna prueba hace pasar un campo desconocido del **sobre exterior** por `RestampEnvelope`, ni afirma `View`,
`Section` o `SchemaVersion` tras un re-estampado, porque ningún proyecto de pruebas carga el Plugin
(`tests/RackCad.Tests/RackCad.Tests.csproj:20-27`). La más cercana replica a mano la mitad que maneja el sobre
(`PersistenceUniformityTests.cs:193-217`), y la `DimensionViewsRestampTests.cs` que añadió I-50 declara que no ejecuta
las líneas del Plugin. El contrato de I-51 declara la preservación como INV-09
(`I-51-rackduplicar-multiples-origenes.md:244-248`), pero T1-T15 y M1-M9 no la cubren. La Proposal V5 de I-54 tampoco la
cierra: sus pruebas cubren la propiedad del store y la forma del re-estampado, y declaran que no demuestran el
comportamiento de `RackEnvelopeRestamp.cs` (D-11.5). Ver la mejora de D-11.6, más abajo.

### F-11 — `RackProjectVariablesWindow` no adopta `DialogWindowChrome` ni `EditorActions`

La ventana es de arquetipo C y ADR-0029 D11 exige adoptar la infraestructura existente (Discovery de I-54 §11.2). En
`f8deb67`, ni su XAML ni su code-behind referencian `DialogWindowChrome` ni `EditorActions`.

### F-12 — Comentarios desfasados

- «five kinds» en `KindHandlerDispatch.cs:12-13` y `RackMenuCommands.cs:138-140`: el registro de handlers incluye
  también Cantilever.
- `RackPushBackCommands.cs:317-318`: el comentario sobre la regeneración de una vista nueva a través de
  `ViewBlockDraw.DrawAndPlace` (señalado como desfasado en el Discovery; sin verificar en ejecución).
- `CantileverLineDesign.cs:335` documenta que el nombre de la línea se muestra en el listado de la biblioteca («Shown
  in the library list»), pero ese listado toma el nombre de la cabecera, del Selectivo o del larguero, o el del
  archivo (`RackDesignLibrary.cs:86-89`).

### F-13 — Troceado de Xrecord duplicado con conducta distinta

`RackBlockData` devuelve nulo, hace un cast duro y no captura (`RackBlockData.cs:18`, `:35-39`, `:43`, `:73`,
`:79-88`); `ProjectVariablesData` es tri-estado y nunca lanza (`ProjectVariablesData.cs:38`, `:59-89`, `:115-122`); y
hay una tercera copia en pruebas (`ProjectVariablesRegistryAccessTests.cs:202-212`). La implementación de I-54
reimplementará ese troceado en su lector del NOD, con la semántica de `ProjectVariablesData` (Proposal V5 D-07.2): la
duplicación es deliberada, y extraerla queda como esta deuda y no como trabajo de I-54.

### F-14a — UTF-16 crudo inválido en el sobre exterior: `RackEmbedStore` lanza al leer

Residual **preexistente**, no introducido por I-54 (Proposal V5 D-08.4 y P-31):

- `RackEmbedStore.Deserialize` solo captura `JsonException` (`RackEmbedDocument.cs:98`). Con UTF-16 crudo inválido en
  el texto del sobre —un surrogate suelto sin escapar— el parseo lanza `ArgumentException`, y **la excepción escapa al
  leer** (sonda sobre .NET 8.0.29).
- Un barrido que lee ese sobre, también el de I-54, **falla antes de cualquier mutación**, sin diagnóstico por
  definición. Los consumidores existentes que leen ese sobre fallan en su propia lectura.
- **I-54 no lo corrige** y no modifica `RackEmbedStore`.

Candidato a una posible iniciativa futura de **robustez del sobre**.

### F-14b — Escape JSON de un surrogate suelto en el sobre exterior: se lee, pero reserializarlo lanza

Residual **preexistente**, no introducido por I-54 (Proposal V5 D-08.4, P-36 y P-37):

- el sobre **se puede deserializar**, tanto si el escape está dentro de `CustomProperties` como en cualquier otro campo;
- **reserializar ese sobre puede lanzar**: `Serialize` del mismo objeto, `Compose(source)` seguido de `Serialize` y el
  re-estampado lanzan `JsonException`, porque `JsonElement.WriteTo` lanza `InvalidOperationException` (sonda sobre .NET
  8.0.29);
- el sobre afectado **no se escribe**;
- los consumidores existentes **conservan su granularidad transaccional**: un flujo con una sola transacción global
  aborta según esa transacción;
- un flujo que escribe **por vista o por etapa puede conservar cambios ya confirmados** antes del sobre afectado;
- `RACKEDITAR` procesa las vistas en secuencia y `SystemBlockWriter.RedrawInPlace` confirma una transacción por vista
  (`RackSelectivoCommands.cs:166-223`, `SystemBlockWriter.cs:55-65`), así que **puede quedar parcialmente actualizado
  entre vistas**;
- esto **ya existe en la base**, sin I-54;
- **I-54 no lo introduce ni lo corrige**;
- el **ejecutor de Rack propio de I-54 evita escrituras parciales**: serializa todos los payloads antes de la primera
  escritura y, si alguno lanza, no confirma ni escribe ningún miembro (D-22.10). Esa garantía es solo suya y no se
  extiende a `RACKEDITAR`, `RACKDUPLICAR`, `RACKLAYOUT`, Project Variables ni otros consumidores existentes.

Mismo candidato futuro que F-14a.

### F-15 — La documentación de usuario de `RACKPROPIEDADES` quedó en `README.md` y en la ayuda de la app

Registrado al integrar I-54 (G10), sin corregir:

- La tabla de comandos de [despliegue.md](guias/despliegue.md), espejo declarado de `RackCommandReference`
  (`RackCommandReference.cs`), ya omitía `RACKPUSHBACK`, `RACKSECCION` y `RACKVARIABLES`, y tampoco lista
  `RACKPROPIEDADES`/`RPR`. El Coordinador lo difirió como no bloqueante al cerrar G7 (disposición D,
  [decisiones de I-54](automation/decisions/I-54.md) §18).
- La guía de uso que preveía el mapa orientativo de la [Proposal V5](initiatives/I-54-proposal-v5.md) §11 no se escribió
  en ningún gate, así que la frontera con la biblioteca de D-12.3 —guardar no lleva las propiedades; abrir o insertar
  crea el rack sin ellas— solo consta en la Proposal y la observó OV-11.
- `RACKPROPIEDADES` sí figura en la tabla de `README.md` y en `RACKAYUDA`.

Conviene resolverlo junto con «Dos comandos ausentes de la referencia en la app» y F-07: una sola pasada por las
tablas de comandos.

### Mejora registrada por la Proposal V5 D-11.6 — el re-estampado del sobre, ejecutable en Core

**No es un hallazgo F** ni deuda de I-54: es la mejora futura que la Proposal V5 registra en D-11.6 y que su freeze
traslada aquí (§14). Hoy ninguna suite ejecuta `RackEnvelopeRestamp.cs` (P-08), y la preservación del sobre en el
re-estampado se cubre con una prueba de la propiedad del store, una guarda estructural y la validación en AutoCAD
(D-11.5). Extraer a Application la mitad del re-estampado que maneja el sobre la haría ejecutable en Core.
**Disparador**: cualquier cambio futuro al manejo del sobre en `RackEnvelopeRestamp.cs`. **No se hace en I-54**,
porque tocaría ese archivo y re-apuntaría la guarda G-R5 mientras I-52 lo reutiliza. Relacionada con F-10.

## I-49 — hallazgos fuera de alcance y seguimientos (2026-09-13, registrados sin corregir)

Encontrados en el Discovery de I-49 ([I-49-discovery.md](initiatives/I-49-discovery.md) §5.8, §7, §8.1 y §11.5, con
los riesgos R7, R13 y R15 de §17) y declarados **fuera de alcance** por su Proposal V6 congelada
([I-49-proposal-v6.md](initiatives/I-49-proposal-v6.md) P30.17;
[ADR-0038](adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md) D25), que manda registrarlos aquí en
G4. Al final van los dos seguimientos que V6 manda registrar también en G4 y que no son hallazgos de P30.17 (P24.5 y
P9.6). Las líneas se refieren a `a4d88f1`.

P30.17 fija dos **excepciones acotadas**, que no son arreglos laterales sino preservación de invariantes en código que
I-49 reescribe: L2 en la rama del reconciliador (P23.15, A-09) y la tabla explícita del token `Type` en la rama `Add`
(P17.10, A-17).

**Ninguno está corregido**: L1, L2, L3, L5, el camino heredado y el factor duplicado de `tools/` siguen vivos en
`main`, y el seguimiento de P24.5 parte de lo que I-49 todavía tiene que implementar.

El Discovery rotula como hallazgos laterales L1, L2, L3 y L5, y **no rotula ninguno como L4**. V6 P30.17 remite a §7,
§5.8, §8.1 y §11.5; lo que §8.1 añade fuera de su tabla es la nota sobre el camino heredado sin disparador, que se
registra con su propio nombre, sin atribuirle una etiqueta que el Discovery no le dio.

### L1 — Asimetría de colocación

La ventana de RACKVARIABLES descarta las entradas (definiciones de bloque) no colocadas (`DirectReferenceCount == 0`)
cuyo sobre no es interpretable (`ProjectVariablesWorkspace.cs:282,337-355`), pero el preflight recibe **todas** las
entradas (`RackVariablesCommands.cs:59-60,122`) y aborta ante cualquier sobre ilegible, esté colocado o no. Además, un
registro bloqueado por una definición colocada ilegible sigue ofreciendo reparaciones
(`ProjectVariablesWorkspace.cs:284-294`) que `ResolveTargetRack` abortaría. *(Lo que descarta la ventana es un hecho
del código citado; la asimetría es inferencia, y ninguna prueba la cubre.)*

**Por qué queda fuera de I-49**: V6 no corrige los hallazgos laterales L1–L5; los registra aquí (P30.17).

### L2 — Posible degradación de minor `2.x` en RACKEDITAR

`Link` calcula la versión con `SelectiveDesignSchema.ResolveWriteVersion(stored, true)`
(`ProjectVariableMutationPreflight.cs:356`), pero el reconciliador fija `"2.0"` sin condición
(`LinkedPropertyReconciler.cs:218`), y el store vuelve a resolverla al serializar
(`SelectivePalletDesignStore.cs:29-31`). Por esa asignación incondicional, un documento guardado como `2.x` (x > 0)
por una build futura podría reescribirse como `2.0` al pasar por RACKEDITAR. *(Los tres puntos citados son hechos;
la degradación es inferencia, y ninguna prueba la cubre.)*

**Relación con I-49**: no se corrige como arreglo lateral. La rama del reconciliador que I-49 reescribe calculará la
versión con `SelectiveDesignSchema.ResolveWriteVersion` o con la autoridad vigente equivalente, y nunca fijará `"2.0"`.
V6 lo acepta como **preservación de la invariante C4-9, no como deuda lateral** (P23.15, A-09 y P30.17; ADR-0038 D13,
condición C3). Hasta que esa implementación llegue, sigue vivo en `main`.

### L3 — Diccionario `PropertyValues` compartido por `WithDesign`

`SelectivePalletDesignDocument.WithDesign` comparte la **misma instancia** del diccionario `PropertyValues` en vez de
clonarla (`SelectivePalletDesignDocument.cs:321`) y solo congela el literal de la holgura (`:324-327`). El reconciliador
la llama sobre el authored inicial (`LinkedPropertyReconciler.cs:146`) y `Apply` la muta (`:206-218`). Hoy un fallo no
persiste nada, porque el comando aborta (`RackSelectivoCommands.cs:147-150`), pero el objeto que recibió el
reconciliador puede quedar mutado en memoria (V6 P23.16).

El Discovery describe al reconciliador como único llamador de `WithDesign` en `src/` (§5.8). Hay una segunda llamada,
en `RackSelectivoCommands.SerializeSelectiveDesign` (`RackSelectivoCommands.cs:373`), pero solo en la rama con
`authored`, y su único llamador, `DrawSelectiveView` (`:409`), no lo pasa: hoy esa rama no se alcanza, y el
reconciliador es el único llamador alcanzable. Quien corrija el portador tendrá que revisar también esa rama. *(Hecho,
comprobado en `a4d88f1` y en `f8deb67`.)*

**Por qué queda fuera de I-49**: el portador no se toca —I-50 lo modificó y ya está integrado en `main`—. I-49 aislará
su propio estado clonando el authored inicial antes de `WithDesign` y de `Apply` (V6 P23.16 y ALT-30).

### L5 — Campos sembrados con `"0.###"` que pierden dígitos al releerse

Los campos sembrados con `"0.###"` y releídos pierden los dígitos que no se muestran: `ValueBox` de RACKVARIABLES,
separadores y editor de celda del Selectivo (Discovery §11.5, inferencia). En RACKVARIABLES, «Cambiar valor» sin
reescribir envía el valor redondeado (`RackProjectVariablesWindow.xaml.cs:73,158-168`; riesgo R7). Las ventanas de
Cantilever lo evitan con `Keep()` (`RackCantileverWindow.xaml.cs:366-382`; hecho).

**Por qué queda fuera de I-49**: V6 no corrige los hallazgos laterales L1–L5 (P30.17). `"0.###"` es formato de
**presentación** y redondea (V6 P4, Base); el texto de una expresión lo producirá un solo formatter, que nunca usará
`"0.###"` (P4.2).

### Camino heredado `Link` / `Unlink` sin disparador en producción (Discovery §8.1)

`Link` y `Unlink` existen en el preflight, pero el camino heredado que los invocaba
—`SelectiveBindingIntentPreflight.Run` (`SelectiveBindingIntent.cs:136-158`), llamado desde
`RackSelectivoCommands.ApplyBinding` (`:305-326`)— **no tiene disparador en producción**:
`RackSelectiveWindow.BindingIntent` (`RackSelectiveWindow.xaml.cs:2398`) no se asigna en ningún archivo de `src/`.
Desde I-48, vincular y desvincular en RACKEDITAR entra por el reconciliador. *(Hecho, Discovery §8.1.)* El riesgo R15
agrupa con él `SelectiveBindingOptions.ForLength` (con `Enum.TryParse`) y `VerticalClearanceBindingState`
(`SelectiveBindingIntent.cs:92-124`; `SelectiveEditorOpen.cs:26-50`).

**Por qué queda fuera de I-49**: V6 no extiende estos caminos heredados (P29.6), porque ampliarlos reabriría una
gramática de tipo que I-48 retiró (riesgo R15). Lo único que V6 fija para `Link` y `Unlink` heredados es qué
observaciones del `PlanReadSet` llevan (P21.6; ADR-0038 D19).

### Seguimiento — unificar en el descriptor la comprobación de dominio de la ventana (V6 P24.5)

**Qué exige V6 a I-49** (lo nuevo, todavía sin implementar): el descriptor de cada propiedad vinculable declarará su
dominio **como dato** —hoy `>= 0` en las dos propiedades Selectivas— y ese dominio se aplicará al valor efectivo de las
tres fuentes. Al escribir, la comprobación histórica de la ventana del Selectivo (`TryEffective`,
`RackSelectiveWindow.xaml.cs:2481-2505`), que hoy ya rechaza `< 0` sobre el efectivo de la sesión del literal y de la
referencia, se aplicará también a la expresión; la referencia y la expresión pasarán además por el reconciliador, y la
expresión, por el aviso del pipeline de la sesión. Al resolver lo persistido, un literal **no** se re-valida, como hoy,
y una referencia o una expresión fuera de dominio darán `OutOfRange`. Una prueba de paridad fijará que, para cada
propiedad, la ventana y el resolver aceptan exactamente el mismo conjunto de efectivos.

**Seguimiento**: V6 deja una sola declaración del dominio, en el descriptor, pero la ventana conserva su propia
comprobación. Unificarla en el descriptor tocaría `RackSelectiveWindow`, un archivo caliente, así que V6 lo deja como
seguimiento. **I-49 no lo hace.**

### Seguimiento — factor lb/ft→kg/m duplicado en `tools/` (V6 P9.6)

`StructuralSectionUnits.PoundsPerFootToKilogramsPerMeter` declara el factor exacto lb/ft→kg/m como el cociente
`0.45359237d / 0.3048d` (`src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs:25`), y el importador
de secciones repite ese cociente como literal
(`tools/RackCad.StructuralSections.Import/AiscShapesImporter.cs:108`; Discovery §12.3). Ese duplicado está fuera de
los ensamblados de producto.

**Seguimiento**: V6 lo deja registrado para G4 (P9.6); I-49 **no** modifica `tools/`. Tampoco lo absorberá la
autoridad neutral de unidades del motor, a la que V6 asigna **únicamente** las conversiones genéricas de longitud (P9.5;
ADR-0038 D3): un factor de masa por longitud de las secciones de catálogo no parece una de ellas *(inferencia)*.

## I-61 — hallazgos fuera de alcance y seguimientos (2026-10-01, registrados sin corregir)

Origen: [evidencia de I-61](automation/evidence/I-61-evidence.md) §§15.12 y 16, [decisiones](automation/decisions/I-61.md) §§18-19 y
[contrato](initiatives/I-61-protocolo-ejecucion-agentes.md) §13. Nada de esto cambia el Freeze de I-61 ni su Candidato validado; cada punto exige una unidad posterior o una
A-n con la autoridad que corresponda.

### Seguimiento formal — autoverificación de la sesión principal (PRINCIPAL SESSION SELF-VERIFICATION)

**Hallazgo** (medido el 2026-10-01 en la sesión principal de I-61, evidencia §16.7): `docs/automation/agent-execution/routing.md` no tiene clase de tarea ni perfil para la
propia sesión principal (Coordinator o Principal), y `model-catalog.md` no tiene una celda «sesión principal». El modelo y el effort de esa sesión dependían de que el Owner o
el Master Orchestrator recordaran el selector. Con el perfil requerido resuelto por analogía, la sesión corría por debajo de lo requerido y se detuvo; tras el cambio del Owner
quedó por encima y después igual.

**Obligación futura, en términos semánticos.** Al inicio de cada sesión principal, antes de cualquier trabajo sustantivo, se resuelve y se registra:

| Campo | Contenido |
|---|---|
| `PRINCIPAL_SESSION_PROFILE` | perfil semántico de la sesión (propuesto: PRINCIPAL_COORDINATION, coordinación larga con decisiones de gate, interpretación de Architect/Controller/Worker y arquitectura sensible) |
| `REQUIRED_CAPABILITY_LEVEL` | nivel de capacidad de `routing.md` §3 (hoy, Frontera para ese perfil) |
| `REQUIRED_EFFORT_CLASS` | effort semántico de `routing.md` §3 (hoy, Long-horizon: horizonte `High`) |
| `CURRENT_MODEL` / `CURRENT_EFFORT` | lo observado en una fuente fiable del runtime; sin ella, `UNKNOWN`, nunca inferido |
| `ROUTING_REASON` | clasificación, dimensiones, celda del catálogo elegida y su fecha de verificación |
| `ESCALATION_CONDITIONS` | cuándo subir, en el orden de `routing.md` §4 (effort → nivel → maximum) y solo con evidencia |
| `CONFIGURATION_STATUS` | `MATCH`; `ABOVE_REQUIRED` (sigue y se registra); `BELOW_REQUIRED` (STOP antes de trabajo sustantivo con `PRINCIPAL_SESSION_CONFIGURATION_REQUIRED`, lo requerido, lo observado y el motivo); `UNKNOWN` (no detiene por sí solo un Candidato; se indica al Owner qué seleccionar) |

Los modelos concretos se resuelven siempre en el catálogo mutable vigente; la regla estable trabaja con perfiles y niveles y no fija nombres de modelo.

**La unidad futura evalúa, según sus autoridades:** `routing.md` (clase y perfil de la sesión principal; punto de extensión del Freeze de I-61 §15); `model-catalog.md`
(celda «sesión principal» por modelo y effort, con fuente y fecha); AUTOMATION_PLAN (si la obligación y su STOP son reglas, su texto normativo vive en §16, con las autoridades
de ese documento); WORKFLOW (los pasos de apertura de sesión, §4 «Cada sesión — al abrir»); CLAUDE.md (índice de arranque); y cualquier otra autoridad realmente aplicable.
Mientras tanto, la memoria local de Claude conserva una ayuda operativa con la misma regla, que **no** sustituye a la autoridad versionada.

### Propuestas del piloto de G3 (evidencia §15.12)

1. README §3.2 de la ejecución delegada: relectura a los 2 s de los procesos ilegibles, host de consola de la cadena del comprobador y alcance del criterio de huérfano
   (DEV-G3-02); un huérfano de otra sesión no queda exento sin A-n.
2. AUTOMATION_PLAN 16.11: camino explícito «STOP resuelto sin cambio del trabajo → reverificación como reejecución de la fase» (DEV-G3-04).
3. Perfil CONTROLLER_VERIFICATION o README §8: semántica Exit/Entry del registro de relevo, el bicondicional VERIFIED ⇔ 14 `pass` y la lectura de `StopCondition` cuando alguna
   comprobación falla.
4. `RemoteFacts`: número de pruebas omitidas de cada TRX.
5. Validar cada filtro de `RequiredTests` con `dotnet test --list-tests --filter` antes de emitir el contrato (DEV-G3-03: `Name~` no filtra con xunit).

### Seguimientos de la conformidad final y de la decisión de G3

- Patrón hexadecimal de `CorrectionOf.AnalysisSha256` (`[0-9a-f]`) frente al del registro de relevo (`[0-9A-Fa-f]`).
- Sufijo `-pilot` de la ruta de custodia `<unit>-pilot/` para unidades que no son piloto.
- PROMPT_TEMPLATES §2 puede citar ADR-0046, ya aceptado.
- Nivel B (scripts de relevo: snapshot con relectura, validación de filtros y regla de coherencia), recomendado por la decisión de G3 sin efecto en I-61.

### Hallazgos fuera de alcance del Discovery (contrato §13)

- Regla `-text`/fin de línea para copias byte a byte bajo `core.autocrlf=true`.
- 13 contratos V1 (más `TEMPLATE.md`) con `automation.enabled: true` sin ejecutor real; 35 estados obsoletos, 3 con YAML inválido y enums desviados; `.agent/` no ignorado;
  prosa «Workflow V2 no efectivo» desactualizada (Discovery §18.3, EXP-01 F-02).
- D-4: `EditCama` redibuja solo la definición elegida (Discovery §19).

### Registro de cierre ajeno desactualizado (hallazgo de la revisión del cierre de I-61)

- La fila de I-59 en `docs/ROADMAP.md` de `main` conserva «integracion en proceso … merge/post-merge/cleanup/receipt pendientes» aunque I-59 está integrada. WORKFLOW §2 impide
  corregir una fila ajena fuera de sus tres momentos: lo corrige la propia I-59 o una unidad documental con autoridad. Lección para los cierres preparados antes de integrar:
  la sesión de integración debe reescribir el registro a «integrada (fecha)» en su commit documental final (decisiones de I-61 §20).

## I-63 — hallazgos fuera de alcance y seguimientos (2026-10-05, registrados sin corregir)

Origen: [evidencia de I-63](automation/evidence/I-63-evidence.md) §§37, 46-47 y 50-52 y [decisiones](automation/decisions/I-63.md) §2. Nada de
esto cambia el Freeze de I-63 (Proposal V3 + A-1..A-4). Cada punto exige una unidad posterior o una A-n con la autoridad que corresponda.

### DEBT-I63-PROTOCOL-01 — discriminación de la verificación del Controller (protocolo de I-61)

**Hallazgo.** Los controles negativos nc1 y nc2 de la verificación del Controller no discriminaron en dos gates de I-63.

| Desviación | Qué pasó |
|---|---|
| DEV-G2-01 | nc2 no detectó un `AllowedWriteScope` mutado |
| DEV-I63-G4-02 | nc1 no detectó un `CurrentSha` inexistente: el Controller tomó el SHA del GREEN que el prompt citaba como contexto narrativo |
| DEV-I63-G4-03 | nc2 no detectó el `AllowedWriteScope` mutado |

En los tres casos la entrega real se verificó de forma independiente, con la comprobación mecánica de la sesión. El defecto es del
mecanismo de verificación, no del producto.

**Recomendaciones para la iniciativa propietaria del protocolo (o el Master):**
1. `Identity` lee el `CurrentSha` de la entrega (mutada en nc1), nunca de la prosa del prompt.
2. `Scope` se calcula desde la delegación concreta que se verifica, no desde la prosa del contrato ni desde notas de la sesión.
3. `Identity` y `Scope` toman como fuente con autoridad campos de entrada parseados mecánicamente.

I-63 es consumidor del protocolo: no edita sus documentos (AUTOMATION_PLAN §16, `docs/automation/agent-execution/`) ni su entrada de
FOUNDATIONS.

### Opcionales de la revisión de arquitectura de G3, diferidos por el Coordinator

- **O-G3-1:** declarar los nombres de miembro `Frentes` y `FrentesVacios` en el catálogo (`RackMetricIds`) y no en
  `RackComputedExpressionContext`, para que D-03 tenga un solo sitio.
- **O-G3-2:** error de programación si dos entradas `rack` comparten nombre (`OrdinalIgnoreCase`). Hoy el catálogo cerrado lo impide y el
  binder toma la primera.
- **O-G3-3:** quitar la llamada duplicada a `RejectComputedEntries` en `RegistryEvaluation`. Es limpieza no observable.
- **O-G3-4 — obligación del PRIMER consumidor de `RackComputedExpressionContext`:**
  - imponer que las entradas `projectVariable` y los valores de `Evaluate` vengan del mismo documento de registro que la Φ2 del rack;
  - decidir cómo se propaga una variable de proyecto fallida;
  - abrir `Create`, hoy interno, con su propia iniciativa.
- **O-G3-5:** aserción de que una entrada `rack` es de ámbito `Rack`.

### Riesgo R-14 de la Proposal V3

Con `ProjectSummary` `Full`, los Selectivos excluidos o no colocados también reciben sus métricas D-28 y, si D-28 llega al paso 5, su
resolución. El coste se midió en G4 (caracterización con N = 1000). Limitar el paso 5 a los racks incluidos exigiría un estado nuevo, y
eso queda para una decisión futura.

### FOLLOWUP-I63-01 — una sola fuente para el soporte por sistema del agregador (DEV-1 de READY-06)

**Hallazgo** (Architect, READY-06; NON-MATERIAL, aceptado por el Coordinator). `ProjectPopulationAggregator` escribe a mano el estado de los
totales de frentes por sistema: `NotApplicable` para Cabecera y Cama y `NotSupported` para el resto. No lo deriva de las declaraciones de
soporte de los providers (D-06/D-08). Los valores coinciden hoy con la matriz congelada D-06, pero la matriz queda representada en dos
sitios.

**Seguimiento** (texto del Coordinator): unificar la fuente de soporte usada por `ProjectPopulationAggregator` con las declaraciones de los
providers, evitando dos representaciones de la matriz D-06. **No cambiar semántica observable.** O-R6-1 queda unido a este seguimiento.

### Opcionales de READY-06 diferidos por el Coordinator

- **O-R6-3:** precisar en la nota posterior de ADR-0043 que el namespace `rack` existe en el núcleo y que sus símbolos solo se ofrecen en
  `RackComputedExpressionContext`. Sería solo una corrección editorial de la nota.
- **O-R6-4:** añadir a la entrada de FOUNDATIONS las rutas de las clases de prueba, cuyos archivos tienen otro nombre (por ejemplo,
  `ComputedParametersRackMetricIdsTests` vive en `RackMetricIdsTests.cs`).

Ninguno es necesario para READY ni corrige una afirmación normativa falsa; no se aplicaron antes de integrar.

## I-65 — hallazgos fuera de alcance y seguimientos (2026-11-03, registrados sin corregir)

### FOLLOWUP-I65-01 — desinstalar el bundle del Autoloader no limpia el registro de usuario

**Hallazgo** (W-R1b, HB-05c; aceptado por el Owner en las decisiones §122). Al primer arranque de AutoCAD con el bundle en
`%APPDATA%\Autodesk\ApplicationPlugins`, AutoCAD escribe en HKCU `Applications\RackCad` (`LOADCTRLS` = 2 y `LOADER` hacia el DLL del bundle),
`Loaded\…\RackCad.bundle` y el valor `AssemblyMap\RackCad.Plugin`, aunque se pulse «Load Once». Borrar la carpeta del bundle no limpia esas
entradas, y AutoCAD intenta cargar en cada arranque un DLL que ya no existe. Los perfiles de baja integridad
`HKCU\Software\appdatalow\…\CoreUser\*` conservan además copias de instalaciones anteriores.

**Seguimiento:** documentar en la guía de despliegue un procedimiento de desinstalación que cubra esas entradas, o un script de
desinstalación con respaldo previo, y decidir con el Owner qué se limpia. En el equipo de validación quedan, por decisión del Owner,
`AssemblyMap\RackCad.Plugin` y las copias de `appdatalow`.

### FOLLOWUP-I65-02 — guarda de productor único del modelo estructural

`StructuralModelBuilder.BuildStructuralModel` es hoy el único productor del `StructuralModel`, pero ninguna guarda lo impone (FOUNDATIONS,
«Structural Analysis Model», Known limitations). Una guarda de fuente lo fijaría.

### FOLLOWUP-I65-03 — `RACKRAMPAQUETE` imprimiría `Valid` sin tabla (A65-F4R-07)

Limitación documentada (decisiones CD-I65-F4-168). `RackRamElementsCommands` mostraría «Valid.» si un resultado `Valid` llegara con
`Table == null`; el Architect recomendó informar `Invalid`. Sin cambio en V1.

### Ampliaciones de alcance diferidas por el Freeze de I-65

- Más casos Cantilever: doble cara, `NegativeY`, inclinación de más de 10°, C/L y canales pareados (hoy rechazados con su código, OV-I65-03).
- Peso propio, combinaciones, momentos y cargas distribuidas.
- Análisis y lectura de resultados (D-14; F6 queda fuera del alcance, Q-O5).
- Otros sistemas (implementar `IStructuralModelSource`) y otros proveedores (carpeta propia bajo `Providers/`).
- RACKAYUDA con el comando nuevo (R-D8 = A).

### Limitaciones de ingeniería que siguen abiertas

- Edición formal del catálogo de RAM: UNKNOWN (DIF-04).
- DIF-01 (peso unitario de RAM frente a AISC, −0.62 %): informativa; no vale si interviene el peso propio.
- Trazabilidad de las sesiones de RAM: operativa, no criptográfica (R-D3, opción 0).
- Propiedades de sección y material: la API no las expone; dependen de D-09.

### Precisión documental de los procedimientos de READY

Los procedimientos de READY (§1.2) citan los árboles de las bases de RAM con el método de Python, pero dicen medirlos con
`w2_hostsnap.ps1`, que usa otro formato (A65-F5F-03). W-R2 midió con los dos (decisiones CD-I65-FC-69 (a)). Conviene unificar el texto si
se reutilizan.

### I-67 — alturas fijadas a mano frente al último larguero

Hallazgo fuera de alcance de I-67 (EXP-08). Ninguna altura fijada a mano se valida contra el último larguero en Dinámico y Push Back:
la altura manual (`ManualHeaderHeightOverride`), las cabeceras personalizadas por módulo (`UseCalculatedHeaderConfiguration = false`),
`HeaderLineOverrides` y las alturas de poste derivado. La corrección de H58 sube los niveles superiores, así que un valor ajustado puede
quedar por debajo del último larguero sin aviso. También queda sin aviso el estado mixto tras `RACKPROYECTAR` sobre un rack sin actualizar.

## I-68 — hallazgos fuera de alcance (2026-11-09, registrados sin corregir)

### `Total del dibujo` une partidas con distinta descripción

Hallazgo de I-68 (EXP-08). `ConsolidatedBomBuilder` une componentes por categoría, longitud y receta, sin mirar Perfil ni Descripción: camas de distinto BFR, o
de Dinámico y Push Back con igual longitud, colapsan bajo la primera descripción, y lo mismo ocurre con largueros de distinto peralte. `Por rack` y `Resumen` los mantienen separados.

### Un total físico consolidado en el BOM

Con Nivel en (Todas), `Resumen` suma componentes y sus piezas relacionadas, y ningún estado del filtro da el BOM físico completo: Pieza omite las Camas de Dinámico y Push Back, y
Componente omite los racks planos. El Owner lo aceptó con advertencia visible y excluyó cambiar la semántica del BOM en I-68. Tampoco existe nivel físico en las filas del BOM.

### La exportación de un solo rack no tiene Table ni tabla dinámica

Decisión D-2 de I-68: las ocho entradas a `RackBomWindow` conservan el XLSX simple, que no tiene Rack ni Copias.

## I-64 F4-T2A — pérdidas de la ruta de carga de la ventana clásica de Push Back (2026-11-12, registradas sin corregir)

La caracterización ventana frente a piloto de F4-T2A (`WorkspaceDraftPilotWindowLoadCharacterizationTests`) compara `RackPushBackSystemWindow.LoadExisting` con la materialización del piloto, que sigue solo la parte de Application. Al cargar un rack, la ventana pasa los valores por sus controles y luego recalcula, y el siguiente Actualizar escribe lo que quedó. El piloto conserva el valor authored. Ninguna de estas pérdidas se corrige en I-64 F4.

### (a) Redondeo al formato «0.###» de los controles

Un valor authored con más de tres decimales (por ejemplo, una escala de anotación de 1.23456 o una altura de tarima de 61.23456) vuelve como 1.235 y 61.235, y un RACKEDITAR seguido de Actualizar lo escribe redondeado. El fixture `pb-08-redondeo` lo mide.

### (b) `ReadInputs` pierde `DimensionStyle`

`ReadInputs` (`RackPushBackSystemWindow.xaml.cs:840-848`) reconstruye `Annotations` sin `DimensionStyle`, mientras que `RecoverInputs` lo recupera y el ensamblador lo escribe. Un rack con estilo de cota propio lo pierde en el siguiente Actualizar clásico.

### (c) y (d) Ajustes de `LoadFromModel` y celda vuelta a comprometer

`LoadFromModel` (`:382-419`) sustituye y ajusta valores al poblar los controles, y `CommitCurrentCell` vuelve a escribir la celda desde ellos. Con el corpus de F4-T2A no hay ningún diseño que provoque un delta de estas clases. La caracterización de F4-T1B o la migración de F5 pueden medirlas.

## I-64 F4-T4C-H0 — las lecturas del panel vacían Rehacer (2026-11-15, registrada sin corregir)

La sonda de host T4C-H0 (F4-E63) vio que las lecturas del panel de F4-T3 y F4-T4A, «Editar» y «Comprobar ahora», dejan entradas «Group of commands» en la pila de Deshacer y vacían la de Rehacer. Son lecturas que se suponían sin efecto. Un `U` seguido de «Comprobar ahora» deja sin nada que rehacer. Toca S2-05 y la obligación de IC-02 de observar Deshacer y Rehacer.

El Owner la dispuso aparte («Registrar aparte», O-18 del paquete de F4-T4C, F4-E65). No cambia el alcance de F4-T4C. Smoke-2 la declara y prueba Redo justo después de Undo. Queda como candidata para una corrección aparte de T3 y T4A.

## I-64 Smoke-2 — la fila de navegación del panel se mueve con la selección (2026-11-17, mejora de uso futura)

Con un rack seleccionado, la sección «Selección» del panel Workspace pasa de dos líneas a tres (`Rack seleccionado: <GUID partido>`). Eso desplaza hacia abajo, unos 12 px, todo lo que hay debajo, incluida la fila Seleccionar / Ver / Centrar. Los avisos que aparecen y desaparecen («Hay un comando en curso.», las confirmaciones) también mueven el panel. En Smoke-2 eso hizo que un operador automático pulsara por encima de «Ver» y diera un falso FAIL (paso 50, F4-E80).

Mejora posible: mantener vertical y estable la fila de navegación y acciones mientras cambia el contenido de «Selección», por ejemplo con una altura fija o reservada para esa sección. El Owner la registra como **FUTURE UX IMPROVEMENT**: no reabre F4 y no es un defecto de corrección.

## I-64 F4B PB-T1a — hallazgos de la ventana clásica de Push Back (2026-11-18, registrados sin corregir)

La red de caracterización de PB-T1a (`tests/RackCad.UI.Tests/PushBackStateNet*.cs`) fija estos comportamientos tal como son hoy. El Owner decidió reproducirlos en PB-T1 sin cambiar RACKEDITAR y anotarlos aquí como mejoras futuras (F4B-E14, preguntas 2 y 3). Corregirlos sería un cambio declarado de RACKEDITAR, en una tarea propia.

### La unidad «ton» se pierde al reconstruir la estructura (c-09)

Un rack guardado con `PalletWeightUnit = "ton"` la conserva al cargar y con «Actualizar». Pero cuando otra edición reconstruye la estructura (por ejemplo, el fondo del pallet), la salida pasa a «kg» con el mismo número: 1000 ton se convierten en 1000 kg. La caja `WeightUnitBox` solo ofrece kg y lb. PB-T1b lo reproduce como una regla explícita del estado (F4B-E14, pregunta 2). La fija CT-PBT1A-02.

### «lb» y la caja de fondos de pallet no llegan solas a la salida

- **«lb» en `WeightUnitBox`:** elegirla no cambia la salida, ni con «Actualizar», mientras nada reconstruye la estructura (`SamePallet` ignora la unidad). La elección del usuario se pierde en silencio hasta la siguiente reconstrucción.
- **`PalletsDeepBox`:** editarla no mueve la salida, ni siquiera el fondo de un frente que se añade después.

La fija CT-PBT1A-02.

### Aceptar Seguridad sin cambios añade el portador de botas

En `pb-05-topes`, aceptar el diálogo de Seguridad sin tocar nada añade a la salida el portador de botas (`Structure.SafetySelections` y `DefensePieceId`), sin cambiar el BOM. La fija CT-PBT1A-11 (es la observación de CT-16 de PB-T0). Si el portador llega al dibujo es algo para comprobar en Smoke-PB.

### Una caja de largo manual vaciada no se vacía (M-07 de PB-TC2)

Con un indicador mixto obsoleto, vaciar «Largo manual» deja el override en la salida, y una caja obligatoria vacía no marca error. La fija CT-PBT1A-10 como INCUMPLIMIENTO CARACTERIZADO.

### El poste fuera de catálogo se ve vacío (c-02)

Un `HeaderPostCatalogId` que no está en el catálogo pasa al poste por defecto en la salida, pero `PostBox` se ve vacía. PB-T1b lo reproduce como una regla explícita (F4B-E09, pregunta 2). Mostrar en la caja el poste que de verdad se usa sería una mejora de uso.

### Higiene de pruebas: `Abrir` en CT-PB0-17 (M-01 de PB-T0)

CT-PB0-17 sigue usando el ayudante `Abrir` (M-01 de la revisión de PB-T0). Las pruebas nuevas de PB-T1 usan `Ventana()` con la carga dentro del `try`. Queda sin corregir porque PB-T1 no edita pruebas existentes.

## I-64 F4B PB-T1b — limitación conocida y mejoras de pruebas (2026-11-19, registradas sin corregir)

### RACKEDITAR pierde en silencio los campos desconocidos anidados (F4B-E24)

Un campo que el plugin no conoce, dentro de un módulo, de un lado o de otra pieza anidada del diseño Push Back, se pierde al reescribir desde RACKEDITAR («Actualizar» o «Guardar en biblioteca»). Puede venir de una versión más nueva del plugin o de una edición a mano.
- **Por qué:** el lector tolerante descarta el campo al leer, y el escritor reconstruye cada pieza anidada desde cero. Solo el sobre, el proyecto y el diseño de primer nivel tienen `[JsonExtensionData]` (I-11 no es recursiva).
- **El panel no tiene el problema:** no escribe esos racks. Los rechaza con aviso (AUTH-13, I-58) y remite a RACKEDITAR. PB-T3 fija ese rechazo con una prueba.
- **Decisión del Owner:** «Basta así». Es una limitación conocida, anterior a F4B, y no se corrige en F4B. Conservar lo anidado sería una iniciativa propia: 21 tipos de datos, algunos compartidos con Dinámico y Selectivo.

### Mejoras de las pruebas de PB-T1b (revisión independiente del intento 2, menores)

- **PBT1B2-IR-M01:** la autoprueba nueva de CT-PBT1B-15 (`escritor-fuera-de-DrawPushBackView`) no distingue la comprobación nueva («dentro del método») de la anterior.
- **PBT1B2-IR-M02:** al caso corregido de CT-10 (I02) le falta una línea de control que demuestre que la sesión de módulos es la única diferencia.
- **PBT1B2-IR-M03:** las corridas de medición previas al RED (p2) se lanzaron con `--no-build` y la entrega no trae su log de compilación. Solo p3, el RED y el GREEN prueban qué texto se compiló.
- **PBT1B2-IR-M04:** la regla «EXCEPCION» sigue sin aplicarse en CT-11. No es una regresión.
- **PBT1B2-IR-M05:** la línea de declaración de CT-PBT1B-01 (M08) calcula 16 y 19 en lugar de escribirlos.

## I-64 F4B PB-T1c — aislamiento de pruebas y un desfase del editor (2026-11-19, registrados sin corregir)

### Las pruebas de UI escriben la disposición del configurador en el perfil del usuario

Varias pruebas de UI que ya existían crean un `RackFrameConfiguratorWindow` real y lo cierran: las clases RichEditor*, EditorShellAdoptionTests, FirstView* y PushBackHeaderOwnerRoundTwoTests, entre otras.
- **Qué pasa:** `OnClosed` llama a `SaveCurrentLayout`, y `RackFrameConfiguratorLayoutStore` escribe `RackCad/RackFrameConfigurator.layout` bajo los datos locales del usuario.
- **El efecto:** cada corrida local de esas pruebas sobrescribe la disposición del configurador (anchos de columna y alto de paneles) que el usuario tiene guardada. No afecta a dibujos ni a datos de racks.
- **En la CI no tiene efecto,** porque el runner es efímero.
- **Cómo se vio:** en la prueba de limpieza del perfil de los Workers de F4B.
- **Lo que haría falta:** una tarea propia que redirija la ubicación de la disposición en las pruebas. Por ejemplo, una costura de prueba en `RackFrameConfiguratorLayoutStore` o una carpeta temporal por prueba. PB-T1c no podía cambiar pruebas existentes.

### El diseño calculado del compuesto va una pasada por detrás tras tres gestos

En la ventana clásica de Push Back, tras cambiar `GapBox`, `CentralSeparatorCheck` o la estructura propia (`StructureOverrideBox`), la secuencia de módulos de `LastComputation` de la parte compuesta se reconverge en la pasada **siguiente**. Por eso, hasta el recálculo siguiente, el diseño calculado va una pasada por detrás del estado.
- **Ya existía antes de PB-T1c** y es igual en la base.
- **Las salidas que escriben no se ven afectadas:** «Actualizar», el BOM y «Guardar en biblioteca» recalculan antes de usar el diseño.
- **«Restaurar» sí:** lee `lastComputation.Design` sin una pasada previa, así que un «Restaurar» justo después de esos gestos parte del diseño que va una pasada por detrás. Lo vio la revisión independiente de PB-T1c, OWNER-01, como dato informativo.
  - Por el mecanismo, la intención compuesta debería volver a ganar en la pasada siguiente.
  - Ninguna prueba lo ejerce sobre un compuesto.
  - El comportamiento es el mismo antes y después de PB-T1c.
- **Lo fija CT-PBT1C-02,** que compara tras un recálculo neutro (F4B-E30).
- **Conviene revisarlo** en PB-T2, cuando «Aplicar al borrador» devuelva el estado en el modo WORKSPACE.

### Mejoras de las pruebas de PB-T1c (revisión independiente del intento 2, menores)

- **PBT1CC2-IR-M01 y M02:** dos comentarios quedan desfasados en W.
  - El de I-42 en `Restore_Click` describe `LoadCompositeFromDesign`, que se retiró.
  - El `<summary>` del estado compuesto documenta ahora `editable` en lugar del reenvío `composite`.
- **PBT1CC2-IR-M03:** el gesto «PostBox=otro» de CT-PBT1C-02 es inerte con el catálogo que se entrega, porque solo trae un poste. Con un catálogo de varios postes, ninguna prueba de la ventana fija el traspaso al estado.
- **PBT1CC2-IR-M04:** CT-PBT1C-01 deriva sus líneas esperadas de la misma enumeración del corpus que las reales.
- **PBT1CC2-IR-M05:** la parte 2 de CT-PBT1C-02 captura las referencias tras cada paso del gesto, no tras cada pasada interna.

## I-64 F4B PB-T2a — mejoras de las pruebas del puerto al borrador (2026-11-19, registradas sin corregir)

Son los menores de la revisión independiente del intento 2 de PB-T2a. El código es correcto por lectura; lo que falta es cobertura o precisión.

- **PBT2A-IR2-M01:** CT-PBT2A-02 no discrimina dos cosas que fija la API:
  - la línea `launchfor` no mira `SourceProject`;
  - cada caso de excepción pasa un solo nulo, así que el **orden** de las tres comprobaciones del constructor de `PushBackDraftEditorLaunch` (D-PBT2-03) no se ejerce.
- **PBT2A-IR2-M02:** CT-PBT2A-03 podría pasar en vacío para X-01 y F-01. Si sus diseños dan `null` cuando la pasada traga una excepción, la fila usa un estado nuevo y vacío, y las líneas salen iguales sin ejercer nada.
- **PBT2A-IR2-M03:** dos comentarios XML imprecisos, heredados del esqueleto:
  - `NotEditable` no cubre todos los estados del núcleo (`Applying`, `Unknown`);
  - `IsDirty` simplifica lo que hace el núcleo.
- **PBT2A-IR2-M04:** INV-PBT2A-01 no se cumple a la letra contra una sola base en una corrección:
  - desde ChainBaseSha entran los documentos de la sesión;
  - desde la BaseSha, los archivos ya existen con el esqueleto.

  La lectura correcta, que es la de la verificación, mira `src/` desde ChainBaseSha. Conviene escribirla así en los contratos de las subtareas siguientes.
- **PBT2A-IR2-M05:** el Worker lanzó las corridas largas como tareas en segundo plano. Ninguna se solapó, y los árboles medidos coinciden con los del RED y el GREEN.

## I-64 F4B — el kit de nc2 frente a F4B-E21 (2026-11-19, registrado sin corregir)

- **El kit de los controles recalcula el hash del relevo de la planificación con la regla de F4:** en la copia mutada, `relay-record-planning.json` → `Outcome.OutputSha256` pasa a ser el SHA de la delegación mutada. Pero desde F4B-E21 el relevo real guarda el SHA de la salida cruda del Controller, con las colecciones opacas vacías. **La copia de nc2 no es, por tanto, del todo fiel a F4B-E21.** No tuvo efecto en PB-T2a: el Controller falló Contract y Scope por la ruta quitada.
- **El sustituto adaptado `PBT2A-NC2-EXACT-WHITELIST-F4B-01` exige exactamente ese recálculo** y lo declara (`Nc2KitRelayRecompute`). Si se corrige el kit, hay que revisar el sustituto.
- **Además, el sustituto depende de la serialización de `inject_opaque.py`,** porque exige que la salida cruda más las colecciones reproduzca la delegación byte a byte.

## I-64 F4B PB-T2b — los menores de la revisión independiente y un arrastre del Owner (2026-11-19, registrados sin corregir)

Son los menores de la revisión independiente de PB-T2b, intento 1. El código cumple el paquete; lo que falta es precisión de las pruebas o de la evidencia.

- **PBT2B-IR-M01:** tres comprobaciones de CT-PBT2B-08 no prueban lo que dicen.
  - **R11b:** la autoprueba quita la primera aparición de la llamada a `ApplyEditsToEditingState`, que está en `ApplyToDraft` y no en `RequestDraw`.
  - **R6 y otra autoprueba:** tampoco aíslan la regla que nombran.
- **PBT2B-IR-M02:** el `Vigia` de CT-PBT2B-05 captura con menos frecuencia que el de CT-PBT1C-02: tras el gesto entero, no tras cada paso, en `calculo-invalido` y `bloqueante`.
- **PBT2B-IR-M03:** las cinco corridas de diagnóstico del Worker no siguieron el régimen de las corridas temporales (INV-PBT2B-10):
  - sin TRX;
  - no todas terminan en `Assert.Fail`;
  - con un filtro de método en lugar del filtro del contrato.

  Cumplen 16.8, RR y la limpieza del árbol.
- **PBT2B-IR-M04:** la evidencia de INV-PBT2B-06 cuenta 27 costuras con una expresión que solo ve declaraciones `internal` de una línea. El contrato dice 41. En lo material se cumple.
- **PBT2B-IR-M05:** `DisenoDe` y `DisenoDeVentana` comparten el centinela `<sin diseno>`. Si los dos fallaran, `diseno-igual` daría True. Hoy no se ejerce.
- **PBT2B-IR-M06:** el §5.0 del paquete dice que la primera línea de cada ventana de WORKSPACE es `modo=…`, y las listas literales del §5.3 solo la llevan en algunas. El Worker siguió las listas, como manda C07.
- **A-T2b-OWNER-01, para PB-T4** (decisión del Owner, F4B-E38): con un módulo escenificado y cambios sin aplicar al borrador a la vez, «Cerrar» solo pregunta por el módulo, y «Sí» pierde también lo no aplicado sin nombrarlo. En PB-T4, una sola pregunta que nombre las dos cosas, con su prueba.
- **El nc1 de F4B:** es la tercera variación de PD-F4-04 (PB-TC, PB-T2a y PB-T2b). En PB-T2b, el Controller además leyó mal `Worker.Trailer` y `RedRepairSha` en `Deviations`. Conviene estudiar, fuera de F4B, si el texto del control nc1 o el esquema de la entrega pueden dejar menos margen a esas lecturas.

## I-64 F4B PB-T2c — menores de la revisión, un límite de cobertura y una diferencia entre modos (2026-11-19, registrados sin corregir)

- **PBT2C-IR-M01:** en CT-PBT2C-01, el gesto y el clic de «Aplicar» no pasan por `Paso`; en CT-02 y CT-03 sí. Si el producto lanzara ahí, CT-01 fallaría por excepción y no dejaría una línea `EXCEPCION`. Además, un comentario de la clase describe mal dónde va la carga.
- **PBT2C-IR-M02:** el Worker leyó INV-PBT2C-10 de forma más estricta que el contrato. Creyó que solo admitía el filtro de la clase nueva en las corridas aisladas. La atribución de los fallos existentes del RED es correcta igualmente.
- **CT-PBT2C-03, un límite de cobertura (M-13 de CA-T2c):** `PostBox=otro`, `CellInOutBeamBox=otro` y `CellInOutPeralteBox=otro` son gestos inertes con el catálogo de prueba, que tiene una sola opción en cada uno. Esos tres controles no se prueban en el camino al borrador. Conviene una prueba con un catálogo de prueba de dos opciones.
- **OWNER-01 de la revisión de PB-T2c, informado al Owner:** en WORKSPACE, un lado B declarado sin frentes sobrevive en el estado del borrador mientras este vive, y una reapertura del editor desde ese borrador vuelve a mostrar el aviso. En CLASSIC, «Actualizar» escribe el diseño no compuesto y la declaración se pierde al reabrir. Qué escribe el panel lo deciden PB-T3 y PB-T4.

## I-64 F4B PB-T3a — menores de la revisión independiente y dos lecciones de método (2026-11-20, registrados sin corregir)

Son los menores de la revisión independiente de PB-T3a, intento 2. El código cumple el paquete v4.5; lo que falta es precisión de las pruebas.

- **PBT3A-IR-M01:** el caso «modulo» de CT-PBT3A-06 no es exactamente la receta F-01 que cita el §5.0.
- **PBT3A-IR-M02:** `cambio-llego` es tautológico en el caso «modulo»: el getter de `ModuleSession` abre la sesión si es nula.
- **PBT3A-IR-M03:** varias pruebas llaman directamente a ayudantes de producto (`PushBackEditableStateSeat.Pass`, constructores y `Copy`), donde el paquete prescribe su propio camino.
- **PBT3A-IR-M04:** CT-PBT3A-12 cubre menos que INV-PBT3A-06. Busca los cinco nombres del §5.2 con `\b` y no ve el comparador.
- **PBT3A-IR-M05:** el fragmento de RR-3 ` reglas-sin-coincidencias=True`, con el espacio delante, no aparece literal en la clase. No hubo reparación, así que no tuvo efecto.
- **PBT3A-IR-M06:** hay una divergencia latente entre el clasificador de producto y el oráculo que prescribe CT-PBT3A-09 en un caso de `DiffPaths`.
- **PBT3A-IR-M07:** `PushBackFullDraftReader.Read` lanza `ArgumentNullException` con un escaneo nulo, y D-PBT3-04 dice «total». Es lo mismo que hace el lector del piloto.
- **PBT3A-IR-M08:** en CT-PBT3A-06, la lectura «después» se toma tres operaciones más tarde de lo que dice el paquete.
- **PBT3A-IR-M09:** ningún CT ejerce las ramas del paso 2 de D-PBT3-04 en las que el lector completo difiere del piloto sin admitir.

**Dos lecciones de método,** con su traducción a los paquetes siguientes:
- **Comprobar los archivos nuevos contra todas las guardas existentes antes de fijar el esqueleto.** La parada del intento 1 vino de G9, que barre también Application. Ya está en el §5.0 de la v4.5.
- **Los sustitutos de control con literales de contrato:** cada paquete redacta la evidencia a su manera, así que un sustituto que exige un literal debe llevar la lista cerrada de los literales de cada paquete.

## I-64 F4B PB-T3b — menores de la revisión independiente y dos lecciones de método (2026-11-20, registrados sin corregir)

Son los menores de la revisión independiente de PB-T3b, intento 2. El código cumple el paquete v4.6, con la variante (A) de D-1; ninguno pide cambiar `src/`.

- **PBT3B-IR-M01:** tres recetas no son las del §5.3, y la entrega las declara solo en `UnexpectedFindings`.
  - CT-PBT3B-04 (iii) y (iv) usan el rack compuesto de CT-PB0-10, sobre `pb-01-base`, en lugar de `pb-20-ab`.
  - CT-PBT3B-11, en su parte principal, y CT-PBT3B-05, con el nombre en blanco, fabrican la relectura: AUTH-13 solo admite la versión de sobre `1.0` y el lector rechaza los nombres distintos.
- **PBT3B-IR-M02:** la línea «hueco-negativo» de CT-PBT3B-06 depende de la cultura del proceso. El producto forma el texto con `CurrentCulture`, y la prueba no fija la cultura invariante, como sí hace CT-PBT1B-12.
- **PBT3B-IR-M03:** el reparto de los dos delegados de la corrida no sigue la letra de D-PBT3-10. La instantánea de la entrada se crea en `_originOf` y `_prepare` no usa su parámetro. Conserva el momento de lectura de la base, pero acopla las dos lambdas por orden de llamada.
- **PBT3B-IR-M04:** la guarda E-3 es algo más débil que la letra del §5.3. Adapta la aguja a `BuildCore` sin comprobar que `Build` y `BuildFull` pasen `reread.StaleBase`.
- **PBT3B-IR-M05:** la receta «celda-no-cabe» de CT-PBT3B-06 no es la de CT-PBT1B-12, y la línea no prueba que el bloqueante sea el de la celda.
- **PBT3B-IR-M06:** CT-PBT3B-09 compara el piloto en 2 casos, no en todos los del §5.3. Faltan las claves distintas y el sobre distinto; las pruebas de F4 de `ComparePost` mitigan el hueco.
- **PBT3B-IR-M07:** la regla del §5.0 de comprobar `IsDirty` justo antes de la operación solo se cumple en CT-01 (i), CT-07 (ii), CT-10, CT-11 y CT-15.
- **PBT3B-IR-M08:** CT-PBT3B-13 (ii) detecta `Precheck` con el borrador completo solo si el tipo aparece en el texto de la llamada. Lo mitiga CT-PBT3A-12.
- **PBT3B-IR-M09:** la entrega no cuadra del todo con los hechos. Habla de «tres commits», y hay dos. Hay corridas locales (`g0`, `d0` y `t0`) que no están en `TestsExecuted`. Y la cronología indica que el GREEN se redactó antes del commit RED; ninguna regla lo prohíbe.

**Dos lecciones de método,** con su traducción a los paquetes siguientes:
- **Compilar el esqueleto y ejecutar la suite completa de Core antes de emitir.** La parada del intento 1 vino de dos CS0121 de una prueba existente, y un tercer choque era de reflexión. El barrido por nombre no ve ni la resolución de sobrecargas ni los escaneos de ensamblado. Ya está en F4B-E45; la redacción del §5.0 pasa a la revisión del paquete para PB-T3c (M-07 de CA-T3b).
- **Una evidencia esperada que dependa de cómo cuente la TRX lleva el total exacto de cada rama.** El contrato de PB-T3b decía «19 128 resultados», con la alternativa «+ 6 o + 10 omitidas»; en la rama «+ 6», la real, el total es 19 124. Un Controller lo leyó de forma estricta en nc3 (F4B-E46, `owner-pbt3b-nc3-decision.md`).

## I-64 F4B PB-T3c — menores de la revisión independiente, riesgos para el Smoke-PB y menores de CA-T3c (2026-11-20, registrados sin corregir)

Son los menores de la revisión independiente del código de PB-T3c, intento 1. El código cumple D-PBT3-11 punto por punto; ninguno pide cambiar `src/`.

- **PBT3C-IR-M01:** CT-PBT3C-01 cuenta las declaraciones de la fachada por modificador de acceso. Un miembro declarado sin modificador (privado implícito) no lo ve. La lista cerrada de PBT3C-C13 la sostiene hoy el censo de la entrega, no una guarda duradera.
- **PBT3C-IR-M02:** en CT-PBT3C-04, la regla «la llamada está dentro de `EraseUnit`» no tiene una autoprueba que la aísle: las tres del §5.4 suben el total a 2. Además, el censo solo ve el literal `RackViewBlockEraser.EraseInTransaction(`, y un `using static` o un grupo de métodos se le escaparían.
- **PBT3C-IR-M03:** CT-PBT3C-03 es menos preciso que su nombre. Cuenta los `Failed` en todo `PrepareUnits`, no dentro del bucle de las claves a borrar, y no comprueba que la relectura compare las claves a borrar con el `ObjectId` resuelto. El código real sí lo hace.
- **PBT3C-IR-M04:** las autopruebas de CT-PBT3C-05 usan 8 de los 15 ejemplos de CT-PBT1B-14. Faltan los `as`/`is` de la regla 1b y los dos que ejercen la regla 2b (alias más molde). Es el mismo subconjunto que CT-PBT3B-13, y el paquete no fija la lista.
- **PBT3C-IR-M05:** tres líneas de WR que D-PBT3-11 no prescribe: los dos `_erasures.Clear()` (al empezar PREPARE y en `Dispose`) y el bucle de las claves a borrar antes del de los miembros. Copian el patrón de `_prepared` y no cambian nada observable. El orden lo declaró el Worker; la línea de `Dispose`, no.
- **PBT3C-IR-M06:** la fachada no lleva el salto del clásico para una definición nula o borrada; con ella lanza una excepción de AutoCAD. Es conforme al paquete y no se alcanza, porque la relectura exige cada clave. Pero el comentario de la clase dice que hace «lo que hace el borrado clásico, en el mismo orden», y hay dos diferencias: el layout, que lanza, y este salto.

**Para el Smoke-PB de PB-T5 (D-PB-09),** riesgos del borrado real que no son defectos de la entrega (la fachada reproduce el clásico):
- **PBT3C-IR-OWNER-1:** `GetBlockReferenceIds(directOnly: true)` devuelve también las referencias de la vista en los layouts de papel y las anidadas en bloques del usuario, y la fachada las borra todas sin aviso. Probar una de cada, con un solo `U`. El Owner decide si quiere un aviso.
- **PBT3C-IR-OWNER-2:** la purga de las anidadas es de un nivel: las definiciones de segundo nivel pueden quedar sin referencias y sin purgar, como en el clásico. Contar los candidatos de `PURGE` antes y después.
- **PBT3C-IR-OWNER-3 (baja):** las representaciones anónimas `*U` de un bloque dinámico no se devuelven ni se escanean. Confirmar que las vistas de RackCad son estáticas.

**De la comprobación acotada de la emisión (CA-T3c),** sin efecto en el contrato:
- **CA-T3c M-02:** la nota de V47-06 del paquete de PB-T3 da por hecho que `pb-20-ab` no tiene un lateral que exista solo por B, y la revisión de PB-T3b dice que no lo verificó. Le falta además el rechazo de sobres con nombres distintos (`DivergentDiagnostic`, OWNER-2 de PB-T3b).
- **CA-T3c M-05:** el corte de argumentos corregido de `overload_sweep_t3c.py` sigue tomando por genérica una comparación sin espacios como `(a<b, c>d)`. No afecta a PB-T3c. La salida custodiada de PB-T3b (`overload-sweep-eab27a9e.txt`) cuenta 5 parámetros en el constructor de `RackPilotMutationScope`, que tenía 4: el mismo defecto, sin efecto en sus conclusiones.

## I-64 F4B PB-T4a — el coste de la lectura, riesgos para el Smoke-PB y menores de la revisión (2026-11-20, registrados sin corregir)

**El coste de la lectura (OWNER-02, medido; decisión del Owner «Cerrar y optimizar en PB-T4b»):**
- **La medida:** cada lectura del panel hace ahora el piloto y además la lectura completa con su materialización. Medido sin AutoCAD en los fixtures mayores (`F4B-DRAFTS/pbt4/read-cost-pbt4a/`): de +40 a +80 ms en `Begin`, de +35 a +86 ms en la relectura habitual y de +80 a +175 ms con el origen encontrado (p95 hasta 257 ms), dentro del bloqueo del documento.
- **De dónde sale:** casi todo es `PushBackEditableStateSeat.LoadExisting`, de 25 a 78 ms por llamada, que se hace dos veces cuando se encuentra el origen.
- **Lo que viene:** PB-T4b elige el origen antes de leer, para materializar una sola vez, y se mide otra vez allí y en el Smoke-PB.
- **Una lección de método:** una medición prevista en el paquete («se mide en la precondición 6») tiene que tener su salida custodiada antes de emitir. Si no, es un efecto sin medir.

**Para el Smoke-PB de PB-T5 (D-PB-09):**
- **PBT4A-IR-OWNER-01:** el origen «deriva» al `Primary` y se queda allí. Si el texto del origen del borrador deja de estar, byte a byte, en un miembro mutable (el rack editado en RACKEDITAR, o un «Comprobar ahora» en `Unknown`), la relectura cae al `Primary`. `Reload`, `DiscardAndReload` y `KeepMine` reconstruyen el borrador desde él, y las relecturas siguientes ya no vuelven a la vista de origen. No es una regresión: hoy siempre es el `Primary`. Para probarlo: abrir desde una vista que no sea el `Primary`, editar en RACKEDITAR y conservar, descartar o comprobar, y ver qué vista refleja el panel.
- **PBT4A-IR-OWNER-03:** dos subcasos de la relectura con origen sin CT en PB-T4a:
  - sin destinos frescos de navegación, la relectura se admite donde hoy no;
  - el texto del origen tras el rebase de un POST.

**Los menores de la revisión independiente del código** (ninguno pide cambiar `src/`):
- **PBT4A-IR-M01:** CT-PBT4A-02 es casi tautológica respecto del Plugin. El kit hace él mismo las dos lecturas sobre un escaneo y las une con `FromReads`, así que no prueba el cableado de RIA, que solo cubren la guarda de texto CT-07 y el Smoke-PB.
- **PBT4A-IR-M02:** CT-PBT4A-03 no distingue `CurrentDraftState.Origin` de `InitialDraftState.Origin`, porque el doble responde siempre con el mismo texto de origen.
- **PBT4A-IR-M03:** CT-PBT4A-04 no fija la comparación byte a byte: no tiene un caso casi igual, como mayúsculas o espacios.
- **PBT4A-IR-M04:** el oráculo de las líneas de CT-02, CT-04 y CT-07 salió de clases temporales que llevaban el GREEN literal en memoria. Se borraron antes del RED y la entrega lo declara.
- **PBT4A-IR-M05:** en CT-03 y CT-05, el constructor del rig corre fuera de `Paso`. Si lanzara, la CT caería por excepción y no por su aserción.
- **PBT4A-IR-M06:** la entrega dice «the three commits» y son dos. Cosmético.
- **PBT4A-IR-M07:** la relectura con origen descarta `ChosenReference` (pasa `null`). Hoy no se nota, porque los gestos del panel construyen la petición con `null`.

**Dos lecciones de la cadena** (aplicadas en la de PB-T4b):
- **El prompt de un Controller no debe pasar de 30 000 bytes:** Windows corta la línea de órdenes y Codex no arranca. Los bloques grandes van a `inputs/` con su sha256, y una guarda de tamaño en los guiones previos lo impide.
- **Las reglas del prompt se escriben como condición explícita** («si … es PBT4A-C07»), nunca como «<hecho>, <parada>.». Además, el prompt de la verificación aclara que una ruta del diff de emisión que no figura en Authorities la admite 16.3.

## I-64 F4B PB-T4b — el texto de «Conservar», menores de la revisión, RR-3 pendiente, el sustituto de nc1 y riesgos para el Smoke-PB (2026-11-21, registrados sin corregir)

**El texto de la presentación de «Conservar» (PBT4B-IR-OWNER-01; decisión del Owner «Es lo esperado»):**
- **El comportamiento:** con «Conservar mi borrador» y después «Actualizar», el rack se queda con el nombre del borrador aunque alguien lo renombrara en el dibujo fuera del panel. Es D-PB-05, paso 4: gana el diseño editable completo, y el nombre es editable.
- **Lo que falta en el texto:** la presentación lo avisa antes del segundo «Sí» como «Cambiado fuera: el nombre.», pero no dice qué valor gana.
- **La mejora,** para la etapa de la interfaz: que el texto diga que se conserva el valor del borrador.

**Los menores de la revisión independiente del código** (ninguno pide cambiar `src/`):
- **PBT4B-IR-M01:** tras la reparación, la línea `resto-del-rack-actual` de CT-PBT4B-08 no discrimina.
  - El escenario solo cambia el nombre entre el rack de base y el actual, así que los cinco campos que quedan del sobre son iguales en el viejo y en el actual.
  - **La mejora:** una variante en la que el rack actual difiera del borrador en un campo del sobre que no sea del editor.
  - La procedencia del sobre la cubre hoy el equivalente de WPP `TheOutgoingDesignAndEnvelopeOfEachViewComeFromTheRereadEnvelopeAndNeverFromTheDraft`.
- **PBT4B-IR-M02:** CT-PBT4B-20 es más débil que el §5.2 en (1) y en (7), sin declararlo.
  - (1) acepta cualquier motivo `I-64 F4B PB-T4b: …` en vez del motivo literal de cada causa.
  - (7) no compara las dos mitades de cada `Theory` de D-S5 salvo sus filas.
  - La revisión lo comprobó aparte y está bien.
  - **La mejora,** si se reabre CT-20: añadir las dos comprobaciones.
- **PBT4B-IR-M03:** las autopruebas de CT-PBT4B-20 no aíslan (3). `autoprueba-asercion-cambiada` y `autoprueba-sin-copia` cambian el texto de la copia, así que `sha256=False` basta, y un `ApplyMap` que siempre diera `True` pasaría.
  - **La mejora:** una autoprueba que mute la copia y recalcule el hash, o que compare sin el sha.
- **PBT4B-IR-M07:** el registro del relevo del trabajo etiqueta mal «PorAsercion».
  - Cuenta solo los fallos con mensaje `Assert.*`: 135.
  - Con la regla del contrato (la primera línea del StackTrace bajo `tests/`) son 306 de 306.
  - **La mejora:** renombrar el campo, o añadir el recuento por la regla del contrato.
  - **Decidido:** se corrige en la cadena de PB-T4c (decisión del Owner sobre nc3, F4B-E53). Es lo que confundió a nc3, que leyó como excepciones 171 de los 306 fallos del RED.
- **PBT4B-IR-M08:** el texto «El valor debe ser > 0.» tiene dos fuentes:
  - la constante `NotPositiveMessage` de `PushBackFullFieldAdapter`, que exige el paquete;
  - el mensaje que compone `NumericTextRule` con `Format(0)`.

  Hoy coinciden en toda cultura. **La mejora:** que PB-T4d lo derive, o que una prueba lo fije.

**RR-3: tres clases de PB-T4b pendientes en la lectura de la cadena** (`pbt4b_rr3_reading.py`, F4B-E53). Sus filas de §6.0 de la v4.9 remiten a literales que el paquete no enumera, así que la lectura no las resuelve sin interpretar, y la cadena para si una reparación del RED las toca:
- **`WorkspaceFullCopyGuardTests` (CT-20):**
  - «los 21 nombres de clase de origen»;
  - «las nueve parejas de M-00 a M-08 (con los seis tipos de M-00 de §5.2)»;
  - «los `BaseSha` de los 21 archivos de los `Skip` (§6.2.1) y los `CopySha` de las 21 copias y del kit»;
  - «los cinco escaneos de M-05»;
  - «las cuatro `particion-por-filas` con …».
- **`PushBackFullPanelGuardTests`:**
  - «los seis nombres de CT-12»;
  - «las de CT-21 en §5.2 y sus expresiones literales»;
  - el marcador `<nombre>` de una línea de CT-12 en §5.2.
- **`PushBackFullPanelFieldTests` (UI):**
  - «Valores: los diez textos de CT-PBT2B-10 y los once casos de CT-18»;
  - los marcadores `<n>` de CT-04 y de CT-18 en §5.2.

**La mejora:** que el paquete de la próxima subtarea enumere en la tabla de RR-3 los literales de cada fila, o que su generador traiga `rr3_table_check`. La lectura de PB-T4a no leía las filas de PB-T4b.

**Para la lista de PB-T4d** (decisión del Owner «Llevarlas a PB-T4d»; la lista vuelve al Owner antes de emitirse): los vacíos de PBT4B-IR-M04, guardas de PEG que en el GREEN siguen en verde sin comprobar nada:
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:592-597`, `TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark`: su `Replace` de `FindPilotDraft` ya no encuentra nada, así que el control positivo no inyecta nada;
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:906-909` y `:917-920`, `TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing`: la expresión de `FindPilotDraft` sigue con el tipo del piloto y el bucle la salta;
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardFull.cs:255-258`: el mismo vacío, heredado en la copia.

**Para el Smoke-PB de PB-T5 (D-PB-09):**
- **PBT4B-IR-OWNER-01:** el nombre tras «Conservar» y «Actualizar» en el dibujo real.
- **PBT4B-IR-OWNER-04:** el segundo «Sí» sin cambios en el dibujo real.
  - Ninguna CT ejecuta RIA ni NH evaluando el testigo tras `AtOrigin`.
  - Si el testigo no volviera `Unchanged`, «Conservar» presentaría en bucle.
  - «Conservar» lee ahora el dibujo dos veces.
- **El coste real de la lectura,** con una sola materialización por lectura (D-S4).
- **Lo que ya estaba:** PBT3C-IR-OWNER-1 a OWNER-3 de PB-T3c y PBT4A-IR-OWNER-01 y OWNER-03 de PB-T4a.

**nc1, candidato a revisar su sustituto** (sin cambio ahora; decisión del Owner «Aceptarlo solo esta vez»):
- **El caso:** nc1 lleva en F4B **seis** variaciones de la cascada del SHA falso: PB-TC, PB-TC2, PB-T2a, PB-T2b, PB-T4a y PB-T4b.
- **La de PB-T4b:** la cascada llega también a Trailer, con la misma causa dicha con otras palabras («no existe el objeto CurrentSha»). El sustituto de PD-F4B-01 (ScriptSha256 `41440edc…`) da FAIL en `d1_FailSetWithinAllowed` y `e2_OtherFailCitesFakeSha`.
- **La lección:** la firma exacta del sustituto no sigue la variación natural de cómo el Controller redacta la cascada. Cada variación vuelve al Owner.
- **Lo que se podría revisar:** que el sustituto admita la cascada en Trailer cuando su evidencia nombre la ausencia del objeto del CurrentSha falso. La firma nueva volvería al Owner con esa revisión.

**Tres lecciones de la cadena:**
- **Una entrega grande no cabe en el tope de 600 s de la verificación.** La primera corrió 600 s, con 50 órdenes, y el lanzador la cortó. Ahora:
  - el tope de los lanzadores es de 1800 s;
  - el relevo registra un TIMEOUT como lanzamiento no productivo;
  - el cierre lo custodia como unidad propia.
- **Un esqueleto que el paquete fija con un generador, y no con un parche,** se compara con lo que da ese generador custodiado y con el registro de la precondición 6, archivo a archivo.
- **Una etiqueta de un registro de la sesión puede confundir a un Controller:** «PorAsercion» solo contaba los `Assert`, y nc3 la leyó contra la regla del contrato. Los registros nombran lo que cuentan, con la regla del contrato.
## I-64 F4B PB-T4c — la recarga de «Editar todo», menores de la revisión, la prueba lenta y riesgos para el Smoke-PB (2026-11-21, registrados sin corregir)

**La recarga de «Editar todo» (PBT4C-IR-OWNER-02; decisión del Owner «Aceptarlo y corregir luego»):**
- **El comportamiento:** con un borrador Clean marcado como posiblemente obsoleto (el dibujo cambió fuera), «Editar» recarga antes de editar (`TryReload`, la carga perezosa), y «Editar todo» no.
  - `EditAllOfPilot` no mira `IsPossiblyStale` ni `NeedsLazyReload`, y el botón se ofrece igual.
  - El editor se abre sobre la base vieja. Al editar, el borrador pasa a sucio y obsoleto, y al aplicar sale el camino de «Conservar mi borrador», que avisa de lo que cambió fuera antes de escribir.
  - No se escribe nada sin avisar, pero el usuario no recibe la recarga silenciosa que da «Editar».
- **La corrección,** para PB-T4d o una tarea posterior, que vuelve al Owner con su lista: que «Editar todo» recargue antes, como «Editar».

**Los menores de la revisión independiente del código que son mejoras** (ninguno pide cambiar `src/` en PB-T4c):
- **PBT4C-IR-M01:** §5.3 pide para CT-PBT4C-10 «el puerto real (`WorkspacePilotEditGate` sobre WHC)», y la CT llama a `TryEditAll` sobre el anfitrión, sin la compuerta (`tests/RackCad.Tests/PushBackEditAllTests.cs:106` y :425).
  - Las líneas no cambian, y el reenvío de la compuerta lo fija CT-PBT4C-12.
  - **La mejora:** que CT-10 vaya por la compuerta, como pide el paquete.
- **PBT4C-IR-M02:** dos debilidades de discriminación que vienen de la letra del paquete.
  - **CT-PBT4C-07:** la autoprueba inyecta el nombre en el texto del sujeto, así que «falla» aunque el sujeto esté vacío. No vería una extracción que dejara de casar, y la guarda pasaría en vacío. Hoy las cinco expresiones casan en el GREEN.
  - **CT-PBT4C-04,** `estado-igual`: el estado del lanzamiento es, por referencia, el estado editable actual del borrador, así que la comparación da `True` por identidad.
  - **La mejora,** en una versión futura del paquete: una línea «sujeto … longitud>0» en el GREEN, y la comparación por valor.
- **PBT4C-IR-M03:** `VentanaBorrador` (`tests/RackCad.UI.Tests/PushBackEditAllWindowTests.cs:343-361`) construye la ventana y carga el borrador fuera del `try`.
  - §5.0 pide la carga dentro del `try` y «Cerrar» en `finally`.
  - Si la carga lanzara, la excepción escaparía de la CT y la ventana no se cerraría. La rama clásica sí sigue la regla.
  - **La mejora:** mover la carga dentro del `try`.
- **PBT4C-IR-M04:** si `ShowModal` lanza después de aplicar, `EditAllOfPilot` devuelve `Failed` con `Applied` a 0.
  - Pero el borrador ya tiene la edición y `NotePilotEdit` ya corrió.
  - El campo dice «No se abrió el editor: <mensaje>.», que es falso: el editor se abrió y hay cambios.
  - Es la letra del GREEN del paquete (`WorkspaceHostCore.cs:1210-1222`; FVM:1344).
  - **La mejora,** en una versión futura: distinguir el fallo tras aplicar. El Smoke-PB mira el aviso.

**Para la lista de PB-T4d** (vuelve al Owner antes de emitirse):
- **La recarga de «Editar todo»** (arriba).
- **PBT4C-IR-OWNER-05, la prueba lenta frente al detector de cuelgue de la CI:**
  - la prueba es `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild…` (`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs:1912-1945`, del RED de PB-T4b), que hace un `Prepare` completo por cada número de toques del catálogo;
  - tarda 276 s en la CI de PB-T4b, y 321 s y 188 s en las del RED y el GREEN de PB-T4c;
  - el detector de cuelgue de la CI está en 5 min (`ci.yml`), y el comentario del job dice que su máximo observado es de unos 2,5 min;
  - en el RED de PB-T4c pasó del umbral sin abortar, probablemente porque el detector mide la inactividad y otras pruebas terminaban en paralelo;
  - es frágil: si cambian el paralelismo o la máquina, la CI de PB-T4d podría abortar. PB-T4c no lo causa.
- **Los vacíos de PBT4B-IR-M04** de PB-T4b, que ya estaban en la lista (F4B-E53).

**Para el Smoke-PB de PB-T5 (D-PB-09):**
- **PBT4C-IR-OWNER-02:** «Editar todo» con un borrador Clean y un cambio externo del dibujo.
- **PBT4C-IR-OWNER-03:** el BOM y «Guardar en biblioteca» desde la ventana de «Editar todo».
  - En WORKSPACE siguen visibles (D-PB-07): solo se ocultan las inserciones.
  - Escriben en disco: «Guardar en biblioteca» un `.rackcad.json`, y el BOM puede exportar XLSX o CSV.
  - «Guardar en biblioteca» ejecuta además `EnsureId` y `SetName` sobre la identidad de la ventana. Hay que mirar si ese `SetName` llega al borrador con «Aplicar al borrador».
- **PBT4C-IR-OWNER-04:**
  - la pregunta combinada de «Cerrar», con el aviso real de AutoCAD;
  - el resumen y «Modificado» del campo tras cerrar;
  - el aviso «No se abrió el editor» cuando sí hubo cambios (M-04).
- **Lo que solo se ve en AutoCAD:** el modal real, con la paleta deshabilitada; el foco y el teclado; «Actualizar» tras editar; los eventos del puente durante el modal; el redibujo.
- **Lo que ya estaba:**
  - PBT3C-IR-OWNER-1 a OWNER-3, de PB-T3c;
  - PBT4A-IR-OWNER-01 y OWNER-03, de PB-T4a;
  - PBT4B-IR-OWNER-01, el nombre tras «Conservar» y «Actualizar», de PB-T4b;
  - PBT4B-IR-OWNER-04, el segundo «Sí» sin cambios en el dibujo real, de PB-T4b;
  - el coste real de la lectura, de PB-T4b.

**Dos lecciones de la cadena:**
- **Un alcance sin copias:**
  - los equivalentes se comparan con su `EquivalentSha`;
  - los `Skip`, con el comprobador que nombra la ExpectedEvidence;
  - el GREEN literal, con lo que da el generador custodiado (igual al `GreenFiles` de la precondición 6).
  La cadena lee la forma del contrato y no escribe nada del régimen a mano.
- **La copia para los revisores** (M-06) no llevó `relay-subst-check.json` ni la carpeta de la prueba de limpieza. **La mejora:** que la cadena siguiente las copie con la entrega.

## I-64 F4B PB-T4d — los nombres que aún describen el piloto retirado, riesgos para el Smoke-PB y lecciones de la retirada (2026-11-22, registrados sin corregir)

**La limpieza de los nombres de prueba y los comentarios que siguen describiendo como presente el código retirado del piloto** (PBT4D-IR-M03; decisión del Owner «Apuntarla en ideas futuras»). Es opcional: no se hace en PB-T4d ni en PB-T5.
- **Los ejemplos de la revisión** (en `7db6fd0c`):
  - **nombres de prueba:**
    - `tests/RackCad.Tests/PushBackFullWritePrepareTests.cs:549`, `CT_PBT3B_06_…OfTheFullModelAndOfThePilot`;
    - `tests/RackCad.Tests/PushBackFullWriteRunTests.cs:246`, `CT_PBT3B_09_…AndThePilotKeepsItsComparison`;
    - `tests/RackCad.Tests/PushBackRetirePilotRunTests.cs:79`, `AViewThatIsNotAvailable…`, que ahora prueba la frontal B (el cambio a mano que declara §5.6);
    - la clase `WorkspacePilotUpdateRunTests`;
  - **los resúmenes de las guardas** en `tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`: el de `E3Violations` (`:104-106`) dice que el piloto nunca borra una vista que no está disponible, y el de `E5Violations` (`:183`) nombra como las dos corridas que deben existir `Execute`, la del piloto, y `ExecuteFull`;
  - **fuera del alcance de PB-T4d,** en `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`:
    - la etiqueta `"WorkspaceHostCore.PilotUpdateRun.Execute"` (`:1783`);
    - «the adapter of the pilot» (`:1240` y `:1268`), que es el campo `PilotFieldAdapter`, de tipo `PushBackFullFieldAdapter`.
- **Por qué no es trivial:**
  - casi todo es texto literal de la pieza del RED o de un archivo que PB-T4d tenía prohibido (C10);
  - renombrar una prueba cambia su identidad y los filtros `FullyQualifiedName~` de los contratos.
- **La mejora:** una tarea de limpieza que renombre esas pruebas, corrija los comentarios y ponga al día sus filtros, si el Owner la quiere.

**Los menores de la revisión independiente del código que quedan abiertos:**
- **PBT4D-IR-M01, `RedFiles` de WPUR** (M-16 de la emisión, abierta):
  - `RedFiles` de `F4B-DRAFTS/pbt4/pbt4d-precondition6.json` lleva, para `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`, el sha256 del RED de la v5.3 (`62c6ff80…`);
  - el RED de la pieza v5.4, que manda el contrato, da `7ea61704…`, y solo difiere en comentarios;
  - cualquier comprobación contra `RedFiles` daría un falso «no coincide» en 1 de 28 archivos. Por eso la disposición de la verificación de PB-T4d recuerda que se compara con la pieza: `red-literal.json` lleva su sha256.
  - **La mejora:** si se corrige el JSON, que sea en una tarea de documentos.
- **Lo que la revisión no comprobó:** que no queden en `tests/` ayudantes `internal` o `public` sin uso. IDE0051 solo ve los privados, y la compilación no lo ve.

**Para el Smoke-PB de PB-T5 (D-PB-09):**
- **PBT4D-IR-OWNER-02** (decisión del Owner «Sí, sumarlos»): tres caminos de `BuildCore` cuyo texto cambia en PB-T4d, sin cambio de comportamiento.
  - Van en una pasada de «Editar todo» → «Actualizar» en el dibujo real, junto a la que ya estaba prevista:
    - un plan que borra un lateral obsoleto;
    - un borrador con el nombre en blanco, que escribe el del sobre de origen en todas las vistas;
    - el rechazo de la frontal B de un rack sin lado B.
  - Core ya los cubre: CT-PBT3B-02, 04 y 05, y la sustituta n.º 10.
- **Lo que ya estaba:**
  - PBT3C-IR-OWNER-1 a OWNER-3, de PB-T3c;
  - PBT4A-IR-OWNER-01 y OWNER-03, de PB-T4a;
  - PBT4B-IR-OWNER-01 y OWNER-04, y el coste real de la lectura, de PB-T4b;
  - PBT4C-IR-OWNER-02 a OWNER-04 y lo que solo se ve en AutoCAD, de PB-T4c.

**RR-3 de las dos CT de PB-T4d, pendiente en la lectura de la cadena** (`pbt4d_rr3_reading.py`). Una reparación del RED en cualquiera de las dos clases habría parado la cadena (RR3_PENDING), sin gastar intento. No la hubo, así que no tuvo efecto. Lo que la lectura no resuelve:
- **«las 35 de CT-01 en §5.6, literales»** y **«las 11 de CT-02»:** §5.6 las da, literales, en la columna de líneas de su fila, con «(35)» y «(11)» al final de la celda. La regla de la lectura solo admite «las de CT-NN en §5.n», sin el número.
- **«las expresiones e inyecciones de su tabla `Retired`»** (`RetiredUses`, en CT-02): §5.6 no las da. Están en el texto de la clase, en el anexo V54-A, y leerlas exige interpretar el C#.
- **La mejora:** que el paquete de la próxima subtarea enumere en la tabla de RR-3 los literales de cada fila, como ya pedía la nota de PB-T4b.

**Cuatro lecciones de la retirada:**
- **Una retirada es una forma del alcance propia.** Lleva:
  - las rutas de `Pilot/` en el AllowedWriteScope;
  - los borrados del RED y del GREEN (D en `git diff --name-status`);
  - C02 con la lista cerrada de lo que se borra y de lo que cambia;
  - un RED sin esqueleto.

  La cadena la lee del contrato y deja intactas las formas de PB-T4b y PB-T4c.
- **Una autoprueba de una guarda de ausencia no discrimina mientras lo ausente todavía existe** (PBT4D-IR-M02). Las dos autopruebas de archivo de CT-PBT4D-01 inyectan un archivo retirado: en el RED ese archivo aún existe, así que la inyección no cambia nada y solo discriminan en el GREEN. **La mejora,** en una próxima retirada: que el paquete diga en qué rama discrimina cada autoprueba, o que la autoprueba de archivo inyecte la ausencia.
- **Una prueba lenta frente al detector de cuelgue** puede tumbar la CI de un commit que solo lleva documentos: le pasó a la emisión de PB-T4d (`45043090710`). La cota de D-S14 lo resolvió: la prueba pasó de minutos a menos de 10 s en la CI.
- **Un sustituto que enumera los subconjuntos del alcance no escala** con el tamaño del contrato. La v2 del sustituto de nc2 enumeraba los 2^n subconjuntos del AllowedWriteScope en su propiedad 10: con las 21 rutas de PB-T4c terminó, y con las 37 de PB-T4d acabó en MemoryError, sin veredicto. La v3 (F4B-E58) calcula la misma proposición en su forma cerrada. **La mejora:** que la revisión de un sustituto mida también su coste frente al alcance más grande previsto, PB-T5 incluida.

## I-64 F4B PB-T4e — la paleta durante «Editar todo», los resúmenes del puerto, riesgos para el Smoke-PB y lecciones de la forma literal (2026-11-23, registrados sin corregir)

**Para el Smoke-PB de PB-T5 (D-PB-09):**
- **PBT4E-IR-OWNER-01** (= M-01 de la revisión independiente del código; informativo, sin decisión del Owner): los pasos 3 y 8 de la guía (§5.5 del paquete de PB-T4) no dicen qué se ve en la paleta mientras la ventana de «Editar todo» está abierta.
  - **Por el código** (en `0eac7911`; la revisión no lo ejecutó): la recarga ocurre dentro de `TryEditAll` y el modal se abre justo después, en la misma llamada síncrona. El campo solo se repinta (`Render()`, `WorkspacePilotFieldViewModel.cs:489`) cuando `TryEditAll` vuelve, al cerrarse el editor; entrar en el modal solo pone la marca, sin refrescar el panel.
  - **Lo que se verá:** con la ventana abierta, la paleta, deshabilitada, sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes del cambio de fuera, mientras la ventana enseña el valor recargado. Al cerrar, el campo queda bien (CT-PBT4E-03, `recargado-y-cerrado`).
  - Es la misma falta de repintado que PB-T4c ya tiene mientras el editor entrega cambios al borrador, y no es un texto ni un estado nuevo.
  - **La mejora:** que la guía lo diga en esos dos pasos, para que no se lea como un fallo de la recarga. Si en AutoCAD se viera otra cosa, sería una observación del Smoke, no un defecto de PB-T4e.
- **El paso 5, sin verificar:** borrar fuera todas las vistas del rack y pulsar «Editar todo» supone que la pestaña del rack y su borrador Clean marcado siguen en el panel después del borrado. CT-PBT4E-02 lo modela con el lector (`OriginMissing`), sin cambiar la superficie. Si en AutoCAD el navegador cambiara la pestaña activa al borrar las vistas, el paso no se podría hacer tal cual, pero no sería un fallo del código.
- **Lo que ya estaba:**
  - R-T4-39 (F4B-E60), que se le cuenta al Owner como información junto al paso 6: con un borrador Dirty y un cambio de fuera que reescriba el sobre de origen, «Actualizar» puede dar `NotAppliedPreflight` y no «Conservar mi borrador»;
  - PBT4D-IR-OWNER-02, los tres caminos de `BuildCore`, de PB-T4d;
  - lo de PB-T3c, PB-T4a, PB-T4b y PB-T4c, en sus notas de arriba.

**Los resúmenes del puerto y de `BeginEdit`** (una observación de la revisión, que no cuenta como hallazgo; anterior a PB-T4e y fuera de su alcance, C10):
- `IWorkspacePilotEditPort.cs:19` dice que solo las seis operaciones de la intención de edición leen el dibujo, y el de `BeginEdit` (`WorkspacePilotFieldViewModel.cs:375`), que es la única entrada que lo lee. Los dos son inexactos desde PB-T4c: «Editar todo» lee al empezar, y desde PB-T4e también al recargar.
- La primera frase la fija `WorkspacePilotEditGuardTests.cs:1083`, y D-PBT4-22, punto 3, decidió conservarla. El comentario nuevo de `TryEditAll` (`IWorkspacePilotEditPort.cs:117-124`) sí lo dice bien.
- **La mejora:** una tarea de documentación que corrija los dos resúmenes y la frase que fija la guarda, si el Owner la quiere.

**El sustituto de nc1, la séptima variación** (decisión del Owner «Aceptarlo solo esta vez», sin cambiar el sustituto): su propiedad `e2_OtherFailCitesFakeSha` falló porque las evidencias de Ci y de Tests citan el SHA falso por el nombre del campo («not the handoff CurrentSha», «CurrentSha is not a commit») y no por su valor, como ya pasó con Tests en PB-T4a. Sigue siendo candidato a revisarse (F4B-E53), sin cambio ahora; si la variación vuelve a aparecer, vuelve al Owner.

**Dos lecciones de PB-T4e:**
- **Una lista vacía es una forma del alcance propia.** El RED solo añade las clases de las CT, sin esqueleto, y el GREEN es el texto literal de `src/`. La cadena la lee del contrato (INV-PBT4E-02 y 07), deja intactas las formas de PB-T4b, PB-T4c y PB-T4d, y para limpia en lo que depende de la versión hasta su seco sobre el contrato emitido.
- **El participante falso de la propia sesión** (la lección de I-61) volvió a salir en la preparación de la verificación: la línea de órdenes de la sesión llevaba la ruta del worktree, y la medida de los procesos contó 2 participantes. Repetida desde el scratchpad, dio 0/0, antes de lanzar. **La mejora:** que la preparación avise cuando la línea de órdenes de quien la llama nombra el worktree, o lanzarla siempre desde el scratchpad.
