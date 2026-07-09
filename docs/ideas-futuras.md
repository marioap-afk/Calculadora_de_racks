# Ideas a futuro y deuda técnica conocida

> Actualizado: 2026-07-08 (auditoría nocturna multi-agente + propuestas).
> Este documento junta (A) mejoras de producto propuestas y (B) hallazgos de la auditoría que se
> **difirieron a propósito** (necesitan validación en AutoCAD o una decisión de producto). Nada de esto
> está roto hoy; es el backlog recomendado.

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
5. **`RACKLISTA`** — comando que tabula todos los racks del dibujo (Nombre, GUID, tipo, vistas
   presentes) con zoom-to al seleccionar. El escaneo por GUID ya existe (`FindRackBlocks`).
6. **Renombrado sincronizado** — cambiar el "Rack A" en cualquier vista debería renombrar los bloques
   de TODAS sus vistas (hoy el nombre viaja en el payload pero el nombre del bloque no se re-sincroniza).
7. **Biblioteca de diseños** — guardar/cargar diseños con nombre fuera del DWG (carpeta de proyectos
   .rackcad.json ya existe por tipo; falta un navegador unificado).
8. **Plantillas de usuario** — "Guardar como plantilla" desde el configurador de cabeceras hacia
   `header-templates.json`, para que las cabeceras personalizadas se reutilicen entre proyectos.

### Ingeniería y datos
9. **Validación de capacidad de carga** — los CSVs ya llevan columnas Ix/Iy/norma; falta la regla que
   compare carga por nivel vs. capacidad del larguero/poste y avise en el editor.
10. **BOM consolidado multi-rack** — un BOM de TODO el dibujo (agrupado por rack via GUID) con
    exportación a Excel, además del BOM por rack actual.
11. **Unificar perfiles estructurales** — fusionar post/truss/beam-profiles en un catálogo
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

### Limpieza de código (segura pero voluminosa)
- **Código muerto del dominio**: `RackFrameConfiguration.BracingSegments` (+ clase `BracingSegment` y
  la rama de migración legacy del VM que nunca corre), `FrameId`/`CreatedAt`/`UpdatedAt` (no se
  persisten ni se muestran), alias `Index/SideMode/DefaultMemberProfileId` en `BracingPanel`,
  `FrameMember.PositionRatio`/`MemberId`.
- **Handlers muertos del configurador**: `AddHorizontalSegment_Click`, `AddSegmentAbove/Below_Click`,
  `DeleteSelectedSegments_Click`, `QuickHorizontal_Click` (no aparecen en el XAML) y los métodos del
  VM que solo ellos llamaban.
- **Superficie muerta en `LateralHeaderParameters`**: `InicioCelosiaTroquel`, `ClaroPanel`,
  `Validate()`, `HasClosingHorizontal` (la factory los llena; el builder ya no los lee).
- **La factory valida elevaciones de plantilla que ya no usa** (exige >= 2 horizontales ascendentes
  aunque solo consume los perfiles).
- **Partir `RackFrameCommands` (~1,100 líneas)** en `partial class` por tipo de rack
  (Selective/Cabecera/Dynamic/Cama + shared).
- **Primitivas de preview triplicadas** (`Map`/`AddLine`/`AddRectangle`/labels) en las ventanas
  dinámico/cama/selectivo → helper compartido de canvas.
- **Lógica pura atrapada en el Plugin**: `SanitizeBlockName` y `NormalizeWhitespace` no son testeables
  desde la suite (el Plugin referencia AutoCAD); moverlas a Application.

### UX menor pendiente
- Clic en otra celda de la matriz del selectivo descarta lo tecleado sin aplicar/preguntar.
- Combos de enums (`Cara`, `Dirección`) muestran los nombres del enum en inglés → converter de display.
- Los campos opcionales del dinámico/cama tragan texto inválido en silencio (el selectivo sí valida).
- `FindTreeViewItem` expande todos los nodos del árbol como efecto colateral de sincronizar selección.
- Puntos de conexión como texto libre en el grid de paneles → ComboBox con `ConnectionPointOptions`.
- Avisar cuando la cabecera personalizada de un poste queda MÁS BAJA que el nivel superior resuelto
  (hoy solo se avisa la divergencia genérica de altura).
