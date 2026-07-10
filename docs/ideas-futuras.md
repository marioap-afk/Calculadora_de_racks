# Ideas a futuro y deuda técnica conocida

> Actualizado: 2026-07-09 (limpieza de código muerto ejecutada).
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

## A. Propuestas de producto

### Vistas y dibujo
1. **Cotas automáticas por vista** — al insertar frontal/lateral/planta, dibujar cotas de alto total,
   fondo, largo de tramo y claros entre niveles (AutoCAD `RotatedDimension` en el mismo bloque o en
   un layer de cotas). Es el paso natural después de que las tres vistas ya se generan solas.
1b. **Pipeline de TEXTO para los toggles de anotación** — **hecho (frontal, planta y lateral):** existe
   `HeaderBlockRole.Annotation` (+ `Text`/`TextHeight`); un helper compartido `SelectiveAnnotations` emite las
   etiquetas y los tres builders las producen según los flags (frontal: frentes+niveles+nombre; planta:
   frentes+nombre; lateral por corte: niveles+poste+nombre). `LateralHeaderDrawer.AppendInstance` las
   materializa como `DBText` en la capa dedicada **`RACKCAD_ANOTACIONES`** (amarilla), regeneradas en cada
   `RedefineSystemBlock` (no se persisten). **(a) centrado y (b) escala configurable — ✅ HECHO
   (2026-07-09):** el `DBText` se centra con `HorizontalMode`/`VerticalMode` + `AdjustAlignment`, y la
   escala del texto es un campo del selectivo (`AnnotationScale`, persistido). "Dibujar placa base" es un
   toggle real de geometría (frontal/planta).
1c. **Dibujar tarima (toggle) — DIFERIDO/no implementable** — la `Tarima` del dominio es abstracta (solo
   `Frente`/`Alto`), sin bloque de catálogo ni representación por vista. Requiere crear un bloque de tarima
   con puntos de conexión (FRONTAL/LATERAL/PLANTA) antes de poder dibujarla. El toggle aún no se expone.
2. **Planta del sistema dinámico y de camas** — replicar la lógica multi-vista (GUID + View) que ya
   comparten selectivo y cabecera. El patrón está listo: builder puro + draw service + rama en
   `RACKEDITAR`.
3. **Elementos de seguridad en los cortes laterales** — protectores de poste, topes de tarima, mallas
   anticaída. Los **largueros** del corte lateral ya se dibujan (`SelectiveLateralBuilder.BuildLargueros`);
   falta solo la seguridad, que necesita bloques `_LATERAL` nuevos + puntos de conexión en el catálogo
   (dependencia de assets, no de código).
4. **Layout de almacén** — colocar varios racks con pasillos y numeración automática ("Rack A",
   "Rack B"...); hoy el nombre es manual. Un comando que clone un rack N veces con espaciado de
   pasillo sería un gran ahorro.

### Gestión de racks
5. **`RACKDUPLICAR` — duplicar un rack como uno INDEPENDIENTE** — ✅ **HECHO (2026-07-09, commit `1547254`).**
   Un `COPY` de AutoCAD comparte la *definición* del bloque y con ella el mismo GUID, así que `RACKEDITAR`
   edita todas las copias juntas (correcto para "réplicas"). `RACKDUPLICAR` cubre el caso opuesto: toma un
   rack, lee su diseño embebido y lo redibuja por el camino de inserción (jig) con un **GUID nuevo** y
   nombre "- copia", como su propio bloque; editar la copia no toca al original. Duplica la vista del
   bloque seleccionado y funciona para los 4 tipos. **Decisión (2026-07-09):** duplicar SOLO la vista
   clicada (guardando el sistema completo en el embed) es el comportamiento deseado — no se duplican todas
   las vistas. Mejora futura: usarlo como base del layout de almacén (#4: clonar N veces con pasillo).
6. **`RACKLISTA`** — comando que tabula todos los racks del dibujo (Nombre, GUID, tipo, vistas
   presentes) con zoom-to al seleccionar. El escaneo por GUID ya existe (`FindRackBlocks`).
7. ~~**Renombrado sincronizado**~~ — ✅ **HECHO (2026-07-09):** al editar/renombrar un rack, `RackBlockRenamer`
   sincroniza el nombre del bloque en TODAS sus vistas (frontal, lateral N, planta) en los 4 tipos
   (best-effort: no lanza, uniquifica evitando colisiones; las referencias apuntan por id, no se rompen).
8. ~~**Biblioteca de diseños**~~ — ✅ **HECHO (2026-07-09):** "Abrir de la biblioteca de diseños" en el menú
   `RACKCAD` lista los diseños `.rackcad.json` de una carpeta gestionada (`%AppData%\RackCad\Designs`, o la
   configurada) con nombre + tipo (`RackDesignLibrary`), y al elegir uno reabre el editor correcto precargado.
   Guardar cabecera/dinámico apunta por defecto a esa carpeta. Pendiente: incluir selectivo y cama (hoy solo
   viven embebidos en el DWG — les falta persistencia a disco) y miniaturas/vista previa.
9. **Plantillas de usuario** — "Guardar como plantilla" desde el configurador de cabeceras hacia
   `header-templates.json`, para que las cabeceras personalizadas se reutilicen entre proyectos.

### Ingeniería y datos
10. **Validación de capacidad de carga** — los CSVs ya llevan columnas Ix/Iy/norma; falta la regla que
    compare carga por nivel vs. capacidad del larguero/poste y avise en el editor.
11. **BOM consolidado multi-rack** — los 4 tipos ya tienen BOM por rack (selectivo, dinámico, cabecera y
    **cama** — `FlowBedBomBuilder`, 2026-07-09). Falta el BOM de TODO el dibujo (agrupado por rack via
    GUID) y la exportación a Excel (hoy CSV).
12. ~~**Unificar perfiles estructurales**~~ — ✅ **HECHO (2026-07-10):** `secciones.csv` es la única hoja de
    perfiles (columna `rol` = POSTE | CELOSIA | LARGUERO). El provider separa las filas en las tres listas
    de siempre (API de `RackCatalog` intacta) y mantiene los tres CSV legacy como fallback de lectura.

## B. Deuda técnica diferida de la auditoría (2026-07-08)

### Necesitan validación en AutoCAD (no tocar sin probar dibujando)
- ~~**Definiciones huérfanas al editar el dinámico**~~ — ✅ **HECHO (2026-07-09):** `RedefineSystemBlock`
  purga las definiciones anidadas que quedan sin referencia tras redefinir (los bloques de catálogo y los
  usados por otros racks se conservan). Conviene una verificación visual final en AutoCAD.
- **Doble diagonal: preview vs dibujo** (`BracingPanelMemberBuilder` ratios 0.14/0.86 vs
  `DiagonalDoubleSpacingTroqueles` del builder lateral): las dos geometrías de doble diagonal se
  calculan distinto; unificar sobre la regla de troqueles y verificar visualmente.

### Decisiones de producto pendientes
- **Validación de esquema en los stores**: hoy `Deserialize("{}")` produce una configuración vacía
  (alto 0) sin error, y `SchemaVersion` se escribe pero nunca se lee. Definir política de versionado
  /migración y validar campos mínimos.
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
