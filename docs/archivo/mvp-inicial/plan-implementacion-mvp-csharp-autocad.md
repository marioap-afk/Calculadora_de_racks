# Plan de implementación del MVP en C# para AutoCAD .NET

> **Archivo histórico; no es una fuente vigente.** Consulta [ROADMAP](../../ROADMAP.md) y
> [WORKFLOW](../../WORKFLOW.md).

## 1. Objetivo del MVP

Construir el primer prototipo funcional de `RACKCABECERA` como configurador manual asistido basado en:

- Cargar una cabecera estándar predeterminada.
- Permitir modificar excepciones.
- Validar consistencia mínima.
- Mostrar BOM preliminar.
- Generar un plan de dibujo.
- Insertar una representación 2D básica en AutoCAD.
- Guardar metadatos suficientes para edición futura.

El objetivo no es implementar todavía el sistema completo ni convertir la macro VBA. El objetivo es probar la arquitectura correcta con un flujo real de extremo a extremo.

Principio rector:

> Partir de lo estándar y modificar excepciones.

## 2. Alcance exacto del primer MVP

### Incluido

| Área | Alcance |
|---|---|
| Comando AutoCAD | `RACKCABECERA`. |
| UI | Configurador mínimo WPF dentro de AutoCAD. |
| Estándar base | Cabecera estándar con claros de 44 in, celosía estándar, placa base atornillable y offsets conocidos. |
| Excepciones | Cambiar claro de tramo, marcar tramo sin celosía, cambiar patrón a doble/X, agregar refuerzo a un poste, cambiar placa, cambiar punto de conexión. |
| Catálogo | JSON inicial o SQLite simple con postes, placas, perfiles, puntos y offsets. |
| Vista previa | Esquemática, suficiente para validar la configuración antes de insertar. |
| BOM preliminar | Conteo básico de postes, placas, diagonales, horizontales y refuerzos. |
| Dibujo AutoCAD | Representación 2D básica con bloques si existen o geometría simple si todavía no hay bloques definitivos. |
| Metadatos | JSON serializado en entidad raíz mediante XRecord/Extension Dictionary. |

### Excluido

- Cálculo estructural completo.
- Generación completa de racks.
- Integración con Excel.
- Cotización.
- Edición completa con `RACKEDITAR`.
- Administración avanzada de catálogos.
- Bloques definitivos de producción.
- Conversión de VBA.

## 3. Orden exacto de desarrollo

### Fase 0: Preparación del proyecto

Objetivo: crear la solución y dejarla lista para compilar.

Entregables:

1. Crear solución `RackCad`.
2. Crear proyectos mínimos:
   - `RackCad.Plugin`
   - `RackCad.Domain`
   - `RackCad.Application`
   - `RackCad.Catalogs`
   - `RackCad.Drawing`
   - `RackCad.AutoCad`
   - `RackCad.UI`
   - `RackCad.Tests`
3. Configurar referencias a AutoCAD .NET API en `RackCad.Plugin` y `RackCad.AutoCad`.
4. Definir versión objetivo de .NET compatible con la versión de AutoCAD usada.
5. Agregar carpeta `assets/catalogs`.
6. Agregar carpeta `assets/blocks`.
7. Agregar carpeta `assets/templates`.

Criterio de avance:

- La solución compila vacía.
- AutoCAD puede cargar una DLL mínima sin ejecutar lógica de negocio.

### Fase 1: Modelo de dominio mínimo

Objetivo: representar una cabecera estándar con excepciones sin depender de AutoCAD.

Clases mínimas:

| Clase | Proyecto | Responsabilidad |
|---|---|---|
| `RackFrameConfiguration` | Domain | Modelo final de cabecera. |
| `RackFrameGeneralData` | Domain | Tipo, altura, fondo, unidades, postes. |
| `StandardRackFrameBaseline` | Domain | Estándar base cargado automáticamente. |
| `FrameExceptionOverride` | Domain | Excepción respecto al estándar. |
| `PostAssembly` | Domain | Poste izquierdo/derecho. |
| `PostReinforcement` | Domain | Refuerzo por poste. |
| `BasePlatePlacement` | Domain | Placa por poste. |
| `BracingSegment` | Domain | Tramo vertical de celosía. |
| `SegmentSideBracing` | Domain | Celosía por lado del tramo. |
| `HorizontalMember` | Domain | Horizontales por tramo o globales. |
| `ConnectionPointReference` | Domain | Punto usado en el modelo. |
| `ManualOffsetOverride` | Domain | Offset manual. |
| `ComponentSelection` | Domain | Selección de catálogo. |

Enums mínimos:

- `PostSide`: `Left`, `Right`.
- `FrameSide`: `Front`, `Back`, `Both`, `None`.
- `BracingPattern`: `None`, `SingleDiagonal`, `DoubleDiagonal`, `X`, `K`.
- `ExceptionType`: `SpecialClear`, `NoBracing`, `DoubleBracing`, `Reinforcement`, `PlateChange`, `ProfileChange`, `ConnectionPointChange`, `OffsetChange`, `HorizontalChange`.
- `SelectionSource`: `Standard`, `Template`, `Suggested`, `Manual`, `Imported`.

Criterio de avance:

- Se puede crear en memoria una cabecera estándar con tres tramos de 44 in.
- Se puede aplicar una excepción cambiando el segundo tramo a 70 in.
- Se puede serializar y deserializar el modelo a JSON.

### Fase 2: Catálogo mínimo en archivo externo

Objetivo: eliminar números mágicos y nombres hardcodeados.

Formato recomendado para el prototipo:

- JSON primero, por velocidad de implementación.
- SQLite después, cuando el esquema estabilice.

Archivos iniciales:

| Archivo | Contenido |
|---|---|
| `assets/catalogs/posts.json` | Postes mínimos. |
| `assets/catalogs/base-plates.json` | Placas base mínimas. |
| `assets/catalogs/profiles.json` | Perfiles de diagonales/horizontales. |
| `assets/catalogs/connection-points.json` | Puntos como `TroquelCelosia_01`. |
| `assets/catalogs/offset-rules.json` | Offsets conocidos. |
| `assets/catalogs/standard-baselines.json` | Cabecera estándar predeterminada. |
| `assets/catalogs/block-definitions.json` | Mapeo de bloques, aunque al inicio puede apuntar a representación simple. |

Repositorios mínimos:

| Clase | Proyecto | Responsabilidad |
|---|---|---|
| `JsonCatalogLoader` | Catalogs | Cargar archivos JSON. |
| `PostCatalogRepository` | Catalogs | Consultar postes. |
| `BasePlateCatalogRepository` | Catalogs | Consultar placas. |
| `ProfileCatalogRepository` | Catalogs | Consultar perfiles. |
| `ConnectionPointRepository` | Catalogs | Consultar puntos de conexión. |
| `OffsetRuleRepository` | Catalogs | Consultar offsets. |
| `StandardBaselineRepository` | Catalogs | Obtener estándar inicial. |

Criterio de avance:

- El estándar base se crea desde catálogo.
- Los puntos `TroquelCelosia_01` y `TroquelCelosia_02` se leen desde archivo.
- Ningún offset del primer prototipo vive como número mágico en código.

### Fase 3: Servicio de estándar base y excepciones

Objetivo: generar la cabecera estándar inicial y aplicar excepciones.

Servicios mínimos:

| Servicio | Proyecto | Responsabilidad |
|---|---|---|
| `StandardBaselineService` | Application | Crear `RackFrameConfiguration` estándar. |
| `FrameExceptionService` | Application | Aplicar, listar y remover excepciones. |
| `RackFrameConfigurationService` | Application | Orquestar carga estándar, cambios y estado actual. |

Flujo interno:

1. Recibir datos generales: altura, fondo, tipo de rack, poste.
2. Buscar estándar aplicable.
3. Crear tramos de 44 in hasta cubrir altura.
4. Asignar placa atornillable.
5. Asignar celosía estándar.
6. Asignar puntos y offsets conocidos.
7. Devolver `RackFrameConfiguration`.

Excepciones mínimas a implementar:

| Excepción | Resultado |
|---|---|
| Cambiar claro de tramo | Actualiza altura del tramo y marca `SpecialClear`. |
| Sin celosía | Cambia patrón a `None`. |
| Doble celosía | Cambia patrón a `DoubleDiagonal`. |
| Refuerzo en poste derecho | Agrega `PostReinforcement`. |
| Cambiar punto de conexión | Actualiza `ConnectionPointReference`. |

Criterio de avance:

- La configuración estándar se modifica sin perder referencia al estándar original.
- La lista de excepciones muestra exactamente qué cambió.
- Se puede restaurar un tramo al estándar.

### Fase 4: Validación mínima

Objetivo: detectar errores de consistencia sin intentar aprobar ingeniería completa.

Clase mínima:

| Clase | Proyecto | Responsabilidad |
|---|---|---|
| `RackFrameValidationService` | Application | Ejecutar validaciones. |
| `ValidationMessage` | Domain | Error, advertencia o nota. |
| `ValidationResult` | Domain | Lista de mensajes y estado general. |

Validaciones mínimas:

- Altura y fondo positivos.
- Poste seleccionado.
- Placa seleccionada.
- Tramos no vacíos.
- Tramos con altura positiva.
- La suma de tramos no excede altura sin advertencia clara.
- Patrón válido.
- Perfil requerido cuando hay celosía.
- Punto de conexión existe.
- Offset manual tiene unidad.
- Bloque o representación disponible.

Criterio de avance:

- Un tramo con punto inexistente bloquea inserción.
- Un claro de 70 in aparece como advertencia o excepción, no como error.
- Un tramo sin celosía es válido si el usuario lo definió como excepción.

### Fase 5: BOM preliminar

Objetivo: contar piezas desde el modelo configurado.

Clases mínimas:

| Clase | Proyecto | Responsabilidad |
|---|---|---|
| `RackFrameBomService` | Application | Generar BOM preliminar. |
| `BomItem` | Domain | Código, descripción, cantidad, longitud, origen. |

BOM mínimo:

- 2 postes.
- Placas base.
- Refuerzos agregados.
- Horizontales estándar.
- Horizontales por excepción.
- Diagonales por tramo según patrón.

Criterio de avance:

- Al cambiar un tramo de celosía estándar a sin celosía, baja la cantidad de diagonales.
- Al agregar refuerzo, aparece en BOM.
- La BOM distingue origen `Standard` y `ManualException`.

### Fase 6: UI mínima del configurador

Objetivo: permitir el flujo real de usuario antes de dibujar.

Tecnología recomendada:

- WPF.
- MVVM simple.
- Ventana modal para MVP.
- Paleta acoplable en fase posterior.

Pantallas mínimas:

| Pantalla/sección | Contenido |
|---|---|
| Datos generales | Altura, fondo, tipo de rack, poste base, botón `Generar estándar`. |
| Estándar base | Resumen de estándar aplicado: claro 44 in, placa atornillable, celosía estándar, offsets. |
| Tramos | Tabla editable de tramos. |
| Inspector de tramo | Patrón, lado, perfil, puntos de conexión, offsets. |
| Postes y placas | Poste izquierdo/derecho, refuerzo, placa. |
| Excepciones | Lista de cambios contra estándar. |
| Vista previa | Esquema 2D simple. |
| Validación/BOM | Pestañas inferiores. |
| Inserción | Botón `Insertar en AutoCAD`. |

Orden de construcción UI:

1. Datos generales.
2. Botón para cargar estándar.
3. Tabla de tramos.
4. Editor de tramo seleccionado.
5. Lista de excepciones.
6. Validación.
7. BOM.
8. Vista previa simple.
9. Insertar.

Criterio de avance:

- El usuario abre `RACKCABECERA`.
- Se muestra una cabecera estándar.
- El usuario cambia el segundo tramo de 44 in a 70 in.
- La excepción aparece en la lista.
- La BOM cambia.
- La vista previa se actualiza.

### Fase 7: Plan de dibujo abstracto

Objetivo: convertir el modelo en instrucciones independientes de AutoCAD.

Clases mínimas:

| Clase | Proyecto | Responsabilidad |
|---|---|---|
| `RackFrameDrawingPlan` | Drawing | Plan raíz. |
| `DrawingBlockPlacement` | Drawing | Inserción de bloque o componente. |
| `DrawingLineSegment` | Drawing | Línea generada por código. |
| `DrawingConnectionInstruction` | Drawing | Conexión entre puntos. |
| `ResolvedConnectionPoint` | Drawing | Punto resuelto en coordenadas locales. |
| `DrawingMetadataInstruction` | Drawing | Metadatos a guardar. |
| `RackFrameDrawingPlanService` | Application/Drawing | Crear el plan desde el modelo. |

Contenido mínimo del plan:

- Dos postes como líneas/bloques.
- Placas base como rectángulos/bloques.
- Tramos como bandas verticales.
- Horizontales como líneas.
- Diagonales como líneas entre puntos resueltos.
- Refuerzos como líneas/bloques junto al poste.
- Metadatos del modelo completo.

Criterio de avance:

- El plan de dibujo se puede imprimir o inspeccionar en pruebas sin AutoCAD.
- El plan no depende de `ObjectId`, `Transaction` ni `BlockReference`.

### Fase 8: Adaptador AutoCAD mínimo

Objetivo: ejecutar el plan de dibujo dentro de AutoCAD.

Clases mínimas:

| Clase | Proyecto | Responsabilidad |
|---|---|---|
| `RackFrameCommands` | Plugin | Registrar `RACKCABECERA`. |
| `AutoCadDocumentService` | AutoCad | Acceso a documento, base de datos y editor. |
| `AutoCadLayerService` | AutoCad | Crear capas mínimas. |
| `AutoCadDrawingPlanExecutor` | AutoCad | Ejecutar `RackFrameDrawingPlan`. |
| `AutoCadMetadataService` | AutoCad | Guardar JSON en XRecord/Extension Dictionary. |
| `AutoCadPromptService` | AutoCad | Pedir punto de inserción. |

Capas mínimas:

- `RACK-COMP-POSTES`
- `RACK-COMP-DIAGONALES`
- `RACK-COMP-HORIZONTALES`
- `RACK-COMP-PLACAS`
- `RACK-COMP-REFUERZOS`
- `RACK-ANNO-TEXTOS`
- `RACK-CONSTRUCTION`

Criterio de avance:

- AutoCAD pide punto de inserción.
- Inserta una cabecera 2D básica.
- Guarda metadatos JSON en una entidad raíz o bloque contenedor.
- No usa constantes geométricas ocultas en el adaptador.

### Fase 9: Pruebas y estabilización del prototipo

Objetivo: asegurar que el circuito mínimo funciona.

Pruebas mínimas:

| Prueba | Tipo |
|---|---|
| Crear estándar con tramos de 44 in | Unit test. |
| Cambiar tramo a 70 in | Unit test. |
| Marcar tramo sin celosía | Unit test. |
| Agregar refuerzo derecho | Unit test. |
| Resolver punto `TroquelCelosia_01` | Unit test. |
| Generar BOM estándar | Unit test. |
| Generar plan de dibujo | Unit test. |
| Ejecutar `RACKCABECERA` en AutoCAD | Manual/integración. |
| Verificar metadatos guardados | Manual/integración. |

Criterio de avance:

- El prototipo permite crear al menos tres cabeceras:
  - Estándar sin excepciones.
  - Segundo tramo de 70 in.
  - Un tramo sin celosía y refuerzo en poste derecho.

## 4. Pantallas mínimas del MVP

### 4.1 Ventana principal `RackFrameConfiguratorWindow`

Estructura recomendada:

| Zona | Contenido |
|---|---|
| Encabezado | Tipo de rack, altura, fondo, poste, estándar activo. |
| Izquierda | Lista de tramos y componentes. |
| Centro | Vista previa esquemática. |
| Derecha | Inspector de propiedades. |
| Inferior | Validación, BOM y excepciones. |
| Pie | Botones: `Restaurar estándar`, `Validar`, `Guardar configuración`, `Insertar`. |

### 4.2 Datos generales

Campos mínimos:

- Tipo de rack.
- Altura.
- Fondo.
- Poste base.
- Unidad.
- Estándar aplicado.

Acciones:

- `Generar estándar`.
- `Recalcular estándar`.
- `Cargar configuración`.

### 4.3 Tabla de tramos

Columnas:

- Índice.
- Inicio.
- Fin.
- Claro.
- Patrón.
- Lado.
- Perfil.
- Estado: estándar o excepción.

Acciones:

- Cambiar claro.
- Cambiar patrón.
- Restaurar estándar.
- Copiar excepción.

### 4.4 Inspector de tramo

Campos:

- Claro.
- Patrón.
- Lado.
- Perfil diagonal.
- Perfil horizontal.
- Punto inferior izquierdo.
- Punto superior derecho.
- Offset X/Y.
- Notas.

### 4.5 Panel de excepciones

Columnas:

- Tipo.
- Elemento.
- Valor estándar.
- Valor actual.
- Motivo.

Este panel es clave para ingeniería. Debe mostrar el trabajo real del usuario.

### 4.6 Vista previa

Debe mostrar:

- Postes.
- Placas.
- Tramos.
- Celosías.
- Horizontales.
- Refuerzos.
- Excepciones resaltadas.

Para el primer prototipo puede ser una vista 2D simple en WPF.

## 5. Archivos iniciales sugeridos

### 5.1 Solución

```text
RackCad.sln
src/
  RackCad.Plugin/
  RackCad.Domain/
  RackCad.Application/
  RackCad.Catalogs/
  RackCad.Drawing/
  RackCad.AutoCad/
  RackCad.UI/
tests/
  RackCad.Tests/
assets/
  catalogs/
  blocks/
  templates/
docs/
```

### 5.2 Domain

```text
src/RackCad.Domain/
  RackFrames/
    RackFrameConfiguration.cs
    RackFrameGeneralData.cs
    StandardRackFrameBaseline.cs
    FrameExceptionOverride.cs
    BracingSegment.cs
    SegmentSideBracing.cs
    HorizontalMember.cs
    PostAssembly.cs
    PostReinforcement.cs
    BasePlatePlacement.cs
  Connections/
    ConnectionPointDefinition.cs
    ConnectionPointReference.cs
    DrawingOffsetRule.cs
    ManualOffsetOverride.cs
  Components/
    ComponentSelection.cs
  Validation/
    ValidationMessage.cs
    ValidationResult.cs
  Bom/
    BomItem.cs
  Enums/
    BracingPattern.cs
    FrameSide.cs
    PostSide.cs
    ExceptionType.cs
    SelectionSource.cs
```

### 5.3 Application

```text
src/RackCad.Application/
  RackFrames/
    RackFrameConfigurationService.cs
    StandardBaselineService.cs
    FrameExceptionService.cs
    RackFrameValidationService.cs
    RackFrameBomService.cs
    RackFrameMetadataService.cs
  Drawing/
    RackFrameDrawingPlanService.cs
  Connections/
    ConnectionPointResolverService.cs
    DrawingOffsetService.cs
```

### 5.4 Catalogs

```text
src/RackCad.Catalogs/
  JsonCatalogLoader.cs
  PostCatalogRepository.cs
  BasePlateCatalogRepository.cs
  ProfileCatalogRepository.cs
  ConnectionPointRepository.cs
  OffsetRuleRepository.cs
  StandardBaselineRepository.cs
  BlockDefinitionRepository.cs
```

### 5.5 Drawing

```text
src/RackCad.Drawing/
  RackFrameDrawingPlan.cs
  DrawingBlockPlacement.cs
  DrawingLineSegment.cs
  DrawingConnectionInstruction.cs
  ResolvedConnectionPoint.cs
  DrawingMetadataInstruction.cs
```

### 5.6 AutoCAD

```text
src/RackCad.AutoCad/
  AutoCadDocumentService.cs
  AutoCadLayerService.cs
  AutoCadDrawingPlanExecutor.cs
  AutoCadMetadataService.cs
  AutoCadPromptService.cs
```

### 5.7 Plugin

```text
src/RackCad.Plugin/
  RackFrameCommands.cs
  PluginInitializer.cs
```

### 5.8 UI

```text
src/RackCad.UI/
  RackFrameConfiguratorWindow.xaml
  RackFrameConfiguratorViewModel.cs
  SegmentListViewModel.cs
  SegmentInspectorViewModel.cs
  ExceptionsViewModel.cs
  BomViewModel.cs
  ValidationViewModel.cs
  Preview/
    RackFramePreviewControl.xaml
    RackFramePreviewViewModel.cs
```

### 5.9 Catálogos iniciales

```text
assets/catalogs/
  posts.json
  base-plates.json
  profiles.json
  connection-points.json
  offset-rules.json
  standard-baselines.json
  block-definitions.json
```

## 6. Primer prototipo funcional

### Objetivo del prototipo 1

Probar el flujo completo más pequeño posible:

1. Ejecutar `RACKCABECERA`.
2. Abrir UI.
3. Cargar cabecera estándar.
4. Cambiar un tramo de 44 in a 70 in.
5. Ver excepción.
6. Ver BOM preliminar.
7. Insertar dibujo 2D básico.
8. Guardar metadatos.

### Geometría mínima del prototipo

No requiere bloques definitivos.

Dibujar con líneas:

- Poste izquierdo.
- Poste derecho.
- Placa izquierda.
- Placa derecha.
- Horizontales.
- Diagonales según patrón.
- Texto mínimo con nombre de cabecera.

Esto permite validar arquitectura antes de invertir tiempo en bloques dinámicos.

### Datos mínimos del estándar

| Dato | Valor inicial |
|---|---|
| Claro estándar | 44 in |
| Placa | Placa base atornillable |
| Celosía | Diagonal sencilla estándar |
| Poste | Seleccionado por usuario |
| Primer punto de celosía | `TroquelCelosia_01` |
| Offset conocido | Desde catálogo |

### Definición de terminado

El prototipo 1 está terminado cuando:

- Compila la solución.
- AutoCAD carga el plugin.
- `RACKCABECERA` abre la ventana.
- La ventana muestra estándar base.
- Se puede cambiar un claro a 70 in.
- Se registra una excepción.
- Se genera BOM.
- Se pide punto de inserción.
- Se dibuja la cabecera.
- Se guardan metadatos JSON.

## 7. Orden recomendado de commits

1. `docs`: plan de implementación.
2. `solution`: estructura de solución y proyectos vacíos.
3. `domain`: entidades mínimas del modelo.
4. `catalogs`: carga JSON y catálogos iniciales.
5. `application`: estándar base y excepciones.
6. `validation`: validaciones mínimas.
7. `bom`: BOM preliminar.
8. `drawing`: plan de dibujo abstracto.
9. `ui`: configurador mínimo.
10. `autocad`: comando e inserción básica.
11. `metadata`: guardar modelo en AutoCAD.
12. `tests`: pruebas del flujo estándar/excepción.

## 8. Decisiones que deben tomarse antes de codificar

1. Versión exacta de AutoCAD objetivo.
2. Framework .NET compatible.
3. Si la UI será ventana modal WPF o palette desde el inicio.
4. Unidad interna oficial: pulgadas o milímetros.
5. Formato inicial de catálogo: JSON confirmado o SQLite desde el inicio.
6. Estándar base MVP:
   - Altura/fondo de ejemplo.
   - Poste inicial.
   - Placa atornillable.
   - Primeros puntos de conexión.
   - Offsets conocidos.
7. Si el primer dibujo usará bloques existentes o geometría simple.

Recomendación:

Usar JSON, WPF modal y geometría simple para el prototipo 1. Después de validar el flujo, conectar bloques dinámicos.

