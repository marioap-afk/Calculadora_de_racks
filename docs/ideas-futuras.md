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
> (`PreviewCanvasPainter` compartido). 235 tests verdes.

## A. Propuestas de producto

### Vistas y dibujo
1. **Cotas automáticas por vista** — al insertar frontal/lateral/planta, dibujar cotas de alto total,
   fondo, largo de tramo y claros entre niveles (AutoCAD `RotatedDimension` en el mismo bloque o en
   un layer de cotas). Es el paso natural después de que las tres vistas ya se generan solas.
2. **Planta del sistema dinámico y de camas** — replicar la lógica multi-vista (GUID + View) que ya
   comparten selectivo y cabecera. El patrón está listo: builder puro + draw service + rama en
   `RACKEDITAR`.
3. **Elementos de seguridad en los cortes laterales** — protectores de poste, topes de tarima, mallas
   anticaída. El corte se regenera del sistema, así que agregarles piezas es extender
   `SelectiveLateralBuilder` + catálogo (bloques `_LATERAL` + puntos de conexión).
4. **Layout de almacén** — colocar varios racks con pasillos y numeración automática ("Rack A",
   "Rack B"...); hoy el nombre es manual. Un comando que clone un rack N veces con espaciado de
   pasillo sería un gran ahorro.

### Gestión de racks
5. **`RACKDUPLICAR` — duplicar un rack como uno INDEPENDIENTE** — hoy un `COPY` de AutoCAD comparte la
   *definición* del bloque y con ella el **mismo GUID** embebido, así que `RACKEDITAR` trata las copias
   como el mismo rack y editar una edita TODAS (correcto para "réplicas" del mismo rack). Falta el caso
   opuesto: tomar un rack (elemento o sistema) y crear una copia **nueva e independiente** con su **GUID
   propio** — para poder editarla sin afectar al original. Implementación: leer el payload embebido
   (`RackEmbedStore` → `RackProjectStore`/`SelectivePalletDesignStore`), re-serializar con
   `Guid.NewGuid()` y un nombre nuevo ("Rack A - copia"), y redibujar por el mismo camino de inserción
   (`DrawSelectiveView` / `DrawAndPlace*`) para que nazca con su propia definición y bloque(s). Debe
   funcionar para los 4 tipos y arrastrar TODAS las vistas del rack (frontal/lateral/planta) con el GUID
   nuevo. Es también la base natural del layout de almacén (#4: clonar N veces con espaciado de pasillo).
6. **`RACKLISTA`** — comando que tabula todos los racks del dibujo (Nombre, GUID, tipo, vistas
   presentes) con zoom-to al seleccionar. El escaneo por GUID ya existe (`FindRackBlocks`).
7. **Renombrado sincronizado** — cambiar el "Rack A" en cualquier vista debería renombrar los bloques
   de TODAS sus vistas (hoy el nombre viaja en el payload pero el nombre del bloque no se re-sincroniza).
8. **Biblioteca de diseños** — guardar/cargar diseños con nombre fuera del DWG (carpeta de proyectos
   .rackcad.json ya existe por tipo; falta un navegador unificado).
9. **Plantillas de usuario** — "Guardar como plantilla" desde el configurador de cabeceras hacia
   `header-templates.json`, para que las cabeceras personalizadas se reutilicen entre proyectos.

### Ingeniería y datos
10. **Validación de capacidad de carga** — los CSVs ya llevan columnas Ix/Iy/norma; falta la regla que
    compare carga por nivel vs. capacidad del larguero/poste y avise en el editor.
11. **BOM consolidado multi-rack** — un BOM de TODO el dibujo (agrupado por rack via GUID) con
    exportación a Excel, además del BOM por rack actual.
12. **Unificar perfiles estructurales** — fusionar post/truss/beam-profiles en un catálogo
    `secciones.csv` con columna de rol (idea previa, sigue vigente).

## B. Deuda técnica diferida de la auditoría (2026-07-08)

### Necesitan validación en AutoCAD (no tocar sin probar dibujando)
- **Definiciones huérfanas al editar el dinámico** (`LateralHeaderDrawer.RedefineSystemBlock`):
  cada edición crea definiciones anidadas uniquificadas (`nombre_1`, `_2`, ...) y nunca purga las
  viejas → se acumulan para siempre. Falta un purge seguro (borrar defs anidadas sin referencias
  tras redefinir).
- **Doble diagonal: preview vs dibujo** (`BracingPanelMemberBuilder` ratios 0.14/0.86 vs
  `DiagonalDoubleSpacingTroqueles` del builder lateral): las dos geometrías de doble diagonal se
  calculan distinto; unificar sobre la regla de troqueles y verificar visualmente.

### Decisiones de producto pendientes
- **Validación de esquema en los stores**: hoy `Deserialize("{}")` produce una configuración vacía
  (alto 0) sin error, y `SchemaVersion` se escribe pero nunca se lee. Definir política de versionado
  /migración y validar campos mínimos.
- **`Recompose` del dinámico borra overrides**: alternar "Reforzar poste derivado" o cambiar el tipo
  de poste reconstruye el sistema y pierde overrides manuales y cabeceras personalizadas. Preservar
  overrides re-aplicándolos tras el rebuild (mapeo por índice/posición).
- **Altura editable en el editor avanzado de cabecera**: el campo Altura es editable pero la altura
  se deriva de las horizontales (cualquier cambio estructural la pisa). Decidir: solo lectura, o que
  editarla recalcule las horizontales.

### Limpieza de código — HECHA (2026-07-09)

Toda esta sección quedó aplicada (ver la nota fechada al inicio del documento). Notas de lo que se
conservó a propósito, por si vuelve a auditarse:
- Se mantuvo `BracingPanel.Index` (vivo en `RackFrameEngineeringPreviewLayout`); solo se borraron los
  alias `SideMode`/`DefaultMemberProfileId`. No confundir el `Configuration.BracingSegments` del dominio
  (borrado) con el `BracingSegments` del ViewModel (`ObservableCollection<BracingSegmentEditorRow>`, UI viva).
- `FrameMemberEnd.HorizontalPositionRatio` sigue vivo (geometría); solo se borró `FrameMember.PositionRatio`.
- De las primitivas de preview se compartieron `AddLine`/`AddRectangle` (`PreviewCanvasPainter`); cada
  ventana conserva su `Map` (proyección propia) y su etiqueta (estilos divergentes).

### UX menor pendiente
- Clic en otra celda de la matriz del selectivo descarta lo tecleado sin aplicar/preguntar.
- Combos de enums (`Cara`, `Dirección`) muestran los nombres del enum en inglés → converter de display.
- Los campos opcionales del dinámico/cama tragan texto inválido en silencio (el selectivo sí valida).
- `FindTreeViewItem` expande todos los nodos del árbol como efecto colateral de sincronizar selección.
- Puntos de conexión como texto libre en el grid de paneles → ComboBox con `ConnectionPointOptions`.
- Avisar cuando la cabecera personalizada de un poste queda MÁS BAJA que el nivel superior resuelto
  (hoy solo se avisa la divergencia genérica de altura).
