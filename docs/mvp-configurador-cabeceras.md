# Especificación actualizada del MVP: configurador manual asistido de cabeceras de rack

> Estado: documento histórico de una etapa previa. Para el estado vigente ver docs/00-indice-contexto.md y docs/01-estado-actual-mvp.md.

Actualización de criterio: el MVP debe ser principalmente un configurador manual asistido. No debe intentar resolver automáticamente toda la ingeniería de la cabecera desde el inicio.

Actualización de flujo: la interfaz no debe iniciar desde una cabecera vacía. El sistema debe cargar siempre una cabecera estándar predeterminada basada en las reglas actuales de ingeniería y permitir que el usuario modifique únicamente las excepciones.

Principio rector:

> Partir de lo estándar y modificar excepciones.

El sistema debe permitir que ingeniería revise y ajuste la cabecera estándar pieza por pieza, tramo por tramo y punto de conexión por punto de conexión, usando catálogos, plantillas editables y datos versionados. Las reglas finas de armado se incorporarán gradualmente conforme ingeniería las formalice.

## 1. Cambio de enfoque

El MVP inicial no debe ser una función cerrada que genera una cabecera estándar a partir de una fórmula única. Tampoco debe ser una calculadora automática que intente tomar todas las decisiones de ingeniería. Debe iniciar con una cabecera estándar ya armada y convertir cada cambio del usuario en una excepción trazable.

El MVP debe ser un configurador visual paramétrico y manual asistido de cabeceras de rack dentro de AutoCAD, ejecutado desde el comando `RACKCABECERA`.

La diferencia es importante:

| Enfoque anterior | Enfoque corregido |
|---|---|
| El usuario captura algunos datos y el sistema genera una cabecera fija. | El sistema carga una cabecera estándar y el usuario modifica solo las excepciones. |
| La lógica principal es una fórmula de generación. | La lógica principal es un modelo editable definido por el usuario con asistencia del sistema. |
| La celosía se calcula como patrón único. | La celosía se define por tramos variables. |
| Los postes son simétricos por defecto. | Cada poste puede tener configuración propia. |
| El dibujo es el resultado principal. | El modelo configurado de cabecera es el resultado principal; el dibujo es una representación. |
| La edición posterior es difícil. | La edición posterior se planea desde el primer MVP mediante metadatos. |

`RACKCABECERA` debe entenderse como:

> Abrir un editor de cabecera, permitir configurar postes, refuerzos, placas, tramos de celosía, horizontales y perfiles, validar la configuración, mostrar BOM preliminar e insertar una representación AutoCAD trazable y regenerable.

Más exactamente:

> Abrir una cabecera estándar editable, detectar y registrar excepciones, validar consistencia mínima, mostrar BOM preliminar e insertar una representación AutoCAD trazable y regenerable.

En este enfoque, "validar" no significa decidir toda la memoria de cálculo. Significa detectar errores de consistencia, datos faltantes, incompatibilidades evidentes, puntos de conexión inexistentes, geometría imposible y problemas de dibujo.

## 2. Objetivo funcional del MVP corregido

El MVP debe permitir crear una cabecera 2D configurable en AutoCAD partiendo de una configuración estándar predeterminada y aplicando excepciones manuales de ingeniería.

Debe incluir:

- Captura de datos generales.
- Carga automática de una cabecera estándar base.
- Configuración independiente de poste izquierdo y poste derecho.
- Refuerzo opcional por poste o lado.
- Lista editable de tramos verticales de celosía.
- Celosía variable por tramo.
- Horizontales por tramo o por elevación.
- Selección de placas base.
- Selección o sugerencia de perfiles.
- Selección de puntos de conexión configurables.
- Edición manual de offsets y reglas de dibujo permitidas.
- Validación mínima.
- BOM preliminar.
- Inserción en AutoCAD.
- Persistencia de metadatos para edición o regeneración posterior.

No debe incluir todavía:

- Diseño completo de racks con múltiples bahías.
- Cálculo estructural completo.
- Decisión automática completa de celosías, claros, refuerzos o placas.
- Integración con Excel.
- Cotización automática.
- Administración avanzada de catálogos.
- Optimización automática global.
- Interfaz multiusuario.

El MVP debe resolver bien una cabecera estándar editable con excepciones. Ese es el cimiento.

## 3. Principios de diseño para el configurador

1. El usuario controla la configuración final.
2. El sistema inicia desde la configuración estándar vigente.
3. El usuario modifica excepciones, no arma todo desde cero en el flujo normal.
4. El sistema puede sugerir, pero no debe sustituir ni ocultar decisiones de ingeniería.
5. Los tramos de celosía son entidades del modelo, no geometría accidental.
6. Cada poste puede tener propiedades independientes.
7. Cada lado de la cabecera puede tener celosía diferente.
8. Las configuraciones estándar deben existir como plantillas editables, no como caminos rígidos.
9. El dibujo debe generarse desde el modelo, no al revés.
10. El modelo debe persistirse para poder editar o regenerar.
11. Las reglas cambiantes deben vivir en catálogos o configuración.
12. Los puntos de conexión y offsets deben ser datos versionados, no números mágicos en código.
13. Las reglas estructurales del software deben vivir en código probado.
14. El MVP debe permitir trabajo manual ordenado antes de automatizar reglas de ingeniería.

## 4. Modelo de datos correcto para una cabecera configurable

### 4.1 Entidad raíz

La entidad principal debe ser `RackFrameConfiguration`.

Representa una cabecera completa como configuración de ingeniería.

Información recomendada:

| Campo | Descripción |
|---|---|
| FrameId | Identificador único de la cabecera. |
| ProjectId | Proyecto o dibujo al que pertenece. |
| Name | Nombre legible de la cabecera. |
| FrameTypeId | Tipo de cabecera: formada, reforzada, estructural, etc. |
| RackTypeId | Tipo de rack o familia de producto. |
| Units | Unidad interna, recomendada en pulgadas o milímetros según estándar definido. |
| Height | Altura total de la cabecera. |
| Depth | Fondo de la cabecera. |
| WidthMode | Si el fondo es nominal, exterior, entre ejes, entre caras o definido por catálogo. |
| CatalogVersion | Versión del catálogo usada. |
| ConnectionPointCatalogVersion | Versión del catálogo de puntos de conexión usada. |
| DrawingRuleSetId | Conjunto de reglas de dibujo usado para esta configuración. |
| StandardBaselineId | Identificador de la cabecera estándar cargada al iniciar. |
| StandardBaselineVersion | Versión del estándar aplicado. |
| StandardRuleSetId | Reglas actuales de ingeniería usadas para crear el estándar. |
| TemplateId | Plantilla estándar de origen. En el MVP debe existir siempre salvo configuración importada excepcional. |
| TemplateVersion | Versión de la plantilla de origen. |
| SavedConfigurationId | Configuración reutilizable de origen, si se cargó una. |
| LeftPost | Configuración del poste izquierdo. |
| RightPost | Configuración del poste derecho. |
| BasePlates | Placas base por poste/lado. |
| BracingSegments | Lista ordenada de tramos de celosía. |
| HorizontalMembers | Horizontales globales o intermedios fuera de tramos específicos. |
| ComponentOverrides | Cambios manuales hechos por el usuario. |
| ConnectionOverrides | Cambios manuales a puntos de conexión u offsets. |
| DrawingRuleOverrides | Cambios manuales a reglas de dibujo. |
| ExceptionOverrides | Lista normalizada de excepciones respecto al estándar. |
| Notes | Notas técnicas del usuario o ingeniería. |
| ValidationState | Resultado de la última validación. |
| CreatedBy | Usuario o estación que creó la cabecera. |
| CreatedAt | Fecha de creación. |
| UpdatedAt | Fecha de última edición. |

### 4.2 Postes

Cada poste debe modelarse como `PostAssembly`, no como simple selección de bloque.

Campos recomendados:

| Campo | Descripción |
|---|---|
| Side | Izquierdo o derecho. |
| PostCatalogId | Referencia al poste base en catálogo. |
| Height | Altura efectiva del poste. Normalmente igual a la cabecera, pero debe poder diferir si ingeniería lo permite. |
| BottomOffset | Offset inferior respecto al origen de la cabecera. |
| TopOffset | Offset superior si aplica. |
| Orientation | Orientación del perfil. |
| HolePatternReference | Patrón de perforación aplicable. |
| ConnectionPointSetId | Conjunto de puntos de conexión disponible para este poste. |
| LocalCoordinateSystemId | Sistema local de coordenadas usado por el poste. |
| Reinforcement | Refuerzo aplicado al poste, si existe. |
| BasePlateId | Placa base asociada. |
| AnchorSetId | Anclas o tornillería base asociada. |
| ComponentOverrides | Overrides manuales del poste. |

### 4.3 Refuerzos

El refuerzo no debe ser un booleano simple. Debe ser una entidad porque puede variar por tipo, longitud, posición y lado.

Entidad sugerida: `PostReinforcement`.

| Campo | Descripción |
|---|---|
| ReinforcementId | Identificador de la instancia. |
| ReinforcementCatalogId | Referencia al componente o perfil de refuerzo. |
| AppliesToPostSide | Poste izquierdo o derecho. |
| PlacementSide | Cara/lado del poste donde se coloca. |
| StartElevation | Elevación inicial. |
| EndElevation | Elevación final. |
| LengthMode | Longitud total, parcial, por catálogo o calculada. |
| ConnectionRuleId | Regla de conexión o barrenos. |
| Quantity | Cantidad de refuerzos si se maneja por pares o piezas. |
| IsUserDefined | Indica si fue elegido manualmente o sugerido. |

Ejemplos:

- Poste izquierdo estándar, poste derecho con refuerzo completo.
- Refuerzo solo en zona inferior.
- Refuerzo en ambos postes.
- Refuerzo en una cara específica por interferencia o criterio de ingeniería.

### 4.4 Tramos de celosía

Los tramos son el corazón del nuevo MVP.

Entidad sugerida: `BracingSegment`.

Un tramo representa una zona vertical entre dos elevaciones, con reglas propias de celosía, horizontales y perfiles.

| Campo | Descripción |
|---|---|
| SegmentId | Identificador único del tramo. |
| Index | Orden vertical dentro de la cabecera. |
| StartElevation | Elevación inferior del tramo. |
| EndElevation | Elevación superior del tramo. |
| ClearHeight | Altura del claro/tramo. Puede calcularse como `EndElevation - StartElevation`. |
| SegmentRole | Principal, cierre inferior, cierre superior, transición, refuerzo, especial. |
| BracingPattern | Sin celosía, diagonal sencilla, doble diagonal, X, K, personalizada. |
| BracingDensity | Sencilla, doble, reforzada u otra categoría interna. |
| BracingSideMode | Frente, posterior, ambos lados, ninguno. |
| FrontBracing | Configuración de celosía del lado frontal. |
| BackBracing | Configuración de celosía del lado posterior. |
| HorizontalMembers | Horizontales asociados al tramo. |
| PreferredBraceProfileId | Perfil preferido de diagonal. |
| PreferredHorizontalProfileId | Perfil preferido de horizontal. |
| DefaultConnectionPointSetId | Conjunto de puntos sugerido para el tramo. |
| ConnectionSetId | Tornillería/conexión aplicable. |
| ConnectionOverrides | Overrides de puntos de conexión para el tramo. |
| DrawingOverrides | Overrides de dibujo para el tramo. |
| IsLocked | Si el tramo fue bloqueado manualmente por el usuario. |
| Source | Manual, sugerido por sistema, plantilla estándar, importado. |
| Notes | Observaciones de ingeniería. |

### 4.5 Configuración de celosía por lado

Entidad sugerida: `SegmentSideBracing`.

| Campo | Descripción |
|---|---|
| Side | Frente o posterior. |
| Pattern | None, SingleDiagonal, DoubleDiagonal, X, K, Custom. |
| Orientation | Ascendente, descendente, alternada, simétrica, definida por usuario. |
| BraceProfileId | Perfil de diagonal. |
| BraceCount | Cantidad de diagonales del patrón. |
| StartConnection | Punto o regla de conexión inferior. |
| EndConnection | Punto o regla de conexión superior. |
| IntermediateConnections | Puntos intermedios si el patrón lo requiere. |
| OffsetRuleId | Offsets desde postes/horizontales. |
| ManualOffsets | Offsets manuales aplicados por el usuario. |
| AllowMirror | Si se permite espejear al otro lado. |
| IsMirroredFromOtherSide | Si este lado se generó copiando el otro. |
| UserOverrides | Cambios manuales. |

Este nivel permite representar:

- Celosía solo al frente.
- Celosía solo atrás.
- Celosía en ambos lados.
- Frente con X y posterior sin celosía.
- Frente con diagonal sencilla y posterior con doble celosía.

### 4.6 Horizontales

Los horizontales deben poder pertenecer a un tramo o a la cabecera completa.

Entidad sugerida: `HorizontalMember`.

| Campo | Descripción |
|---|---|
| HorizontalId | Identificador de instancia. |
| Scope | Global, por tramo, inferior, superior, intermedio, especial. |
| SegmentId | Tramo asociado, si aplica. |
| Elevation | Elevación donde se coloca. |
| ProfileId | Perfil usado. |
| SideMode | Frente, posterior, ambos, centro, interno. |
| LeftConnectionPointId | Punto de conexión en poste izquierdo o componente origen. |
| RightConnectionPointId | Punto de conexión en poste derecho o componente destino. |
| LengthMode | Calculada por fondo, seleccionada de catálogo o manual. |
| EffectiveLength | Longitud calculada o elegida. |
| ConnectionSetId | Tornillería o conexión. |
| IsRequiredByPattern | Si es requerido por el patrón de celosía. |
| Source | Manual, sugerido, plantilla, regla automática. |

Ejemplos:

- Horizontal inferior.
- Horizontal superior.
- Horizontal intermedio en una K.
- Horizontal adicional entre tramos.
- Horizontal solo de un lado.

### 4.7 Placas

Las placas deben configurarse por poste, no como propiedad global.

Entidad sugerida: `BasePlatePlacement`.

| Campo | Descripción |
|---|---|
| PlatePlacementId | Identificador de instancia. |
| PostSide | Poste izquierdo o derecho. |
| PlateCatalogId | Referencia a la placa. |
| Orientation | Rotación/orientación. |
| Elevation | Normalmente 0, pero debe poder parametrizarse. |
| ConnectionPointId | Punto de conexión/base asociado. |
| AnchorSetId | Conjunto de anclas. |
| HolePatternOverride | Override si ingeniería lo permite. |
| IsSameAsOppositeSide | Si se copió desde la otra placa. |

Esto permite:

- Placas distintas por poste.
- Placa reforzada en un lado.
- Cambios futuros por tipo de piso, anclaje o carga.

### 4.8 Selección de componentes

Para no amarrar el modelo a nombres visibles de AutoCAD, toda pieza seleccionada debe usar referencias a catálogo.

Entidad sugerida: `ComponentSelection`.

| Campo | Descripción |
|---|---|
| CatalogId | Identificador estable del componente. |
| Code | Código comercial o interno visible. |
| Description | Descripción visible. |
| Revision | Revisión del componente. |
| Quantity | Cantidad. |
| Length | Longitud si aplica. |
| Source | Manual, sugerido, calculado, plantilla. |
| IsOverride | Indica si el usuario cambió una recomendación del sistema. |

El código visible puede cambiar; el `CatalogId` debe permanecer estable.

### 4.9 Puntos de conexión

Los puntos de conexión deben ser entidades formales del catálogo y del modelo. No deben quedar representados como coordenadas sueltas en código.

Entidad sugerida: `ConnectionPointDefinition`.

| Campo | Descripción |
|---|---|
| ConnectionPointId | Identificador estable del punto. |
| Code | Código legible, por ejemplo `TroquelCelosia_01`. |
| ComponentCatalogId | Componente al que pertenece el punto: poste, placa, horizontal, refuerzo, etc. |
| PointType | Troquel de celosía, placa base, horizontal inferior, horizontal superior, nodo intermedio, ancla, especial. |
| LocalX | Coordenada X local del punto. |
| LocalY | Coordenada Y local del punto. |
| LocalZ | Coordenada Z local, aunque el MVP sea 2D. |
| Units | Unidad de las coordenadas. |
| CoordinateSystemId | Sistema local al que pertenecen las coordenadas. |
| Face | Frente, posterior, interior, exterior, centro o no aplica. |
| Side | Izquierdo, derecho, ambos o no aplica. |
| Direction | Dirección preferente de conexión, si aplica. |
| AllowedMemberTypes | Diagonal, horizontal, refuerzo, placa, ancla, etc. |
| HoleDiameter | Diámetro o referencia al troquel, si aplica. |
| HolePatternId | Patrón de perforación asociado. |
| Sequence | Orden dentro del patrón, por ejemplo 1, 2, 3. |
| Status | Activo, obsoleto, experimental. |
| Version | Versión del punto o del conjunto. |
| EffectiveFrom | Fecha desde la que aplica. |
| EffectiveTo | Fecha hasta la que deja de aplicar. |
| Notes | Notas de ingeniería. |

Ejemplo conceptual:

| Code | LocalX | LocalY | PointType |
|---|---:|---:|---|
| TroquelCelosia_01 | 2.75 in | 7.58 in | Troquel de celosía |
| TroquelCelosia_02 | 2.75 in | 11.58 in | Troquel de celosía |
| PlacaBase_01 | 0 in | 0 in | Placa base |
| HorizontalInferior_01 | 2.75 in | 7.58 in | Horizontal inferior |
| HorizontalSuperior_01 | 2.75 in | 51.58 in | Horizontal superior |

### 4.10 Referencias a puntos de conexión

El modelo de cabecera no debe copiar coordenadas por defecto. Debe referenciar puntos de catálogo y guardar overrides solo cuando el usuario cambie algo.

Entidad sugerida: `ConnectionPointReference`.

| Campo | Descripción |
|---|---|
| ReferenceId | Identificador de la referencia dentro del modelo. |
| ConnectionPointId | Punto de catálogo usado. |
| ComponentInstanceId | Instancia del componente donde se aplica. |
| SegmentId | Tramo relacionado, si aplica. |
| Role | Inicio, fin, nodo intermedio, apoyo, anclaje, referencia. |
| ResolvedX / ResolvedY / ResolvedZ | Coordenadas resueltas al momento de dibujar. |
| OffsetOverride | Offset manual aplicado sobre el punto. |
| IsUserOverride | Indica si el usuario cambió el punto sugerido o su offset. |
| Source | Manual, plantilla, sugerido, catálogo, importado. |

### 4.11 Offsets y reglas de dibujo

Los offsets deben manejarse como datos con nombre, unidad, alcance y versión.

Entidad sugerida: `DrawingOffsetRule`.

| Campo | Descripción |
|---|---|
| OffsetRuleId | Identificador de la regla. |
| Code | Código legible, por ejemplo `OffsetCelosiaPosteOmega`. |
| AppliesTo | Diagonal, horizontal, refuerzo, placa, etiqueta, bloque. |
| ComponentFamilyId | Familia a la que aplica. |
| DeltaX | Desplazamiento X. |
| DeltaY | Desplazamiento Y. |
| DeltaZ | Desplazamiento Z. |
| Units | Unidad. |
| CoordinateSystemId | Sistema local donde se interpreta. |
| Priority | Prioridad si varias reglas aplican. |
| Status | Activo, obsoleto, experimental. |
| Version | Versión. |
| Notes | Explicación técnica. |

Entidad sugerida para overrides manuales: `ManualOffsetOverride`.

| Campo | Descripción |
|---|---|
| OverrideId | Identificador del override. |
| TargetType | Tramo, diagonal, horizontal, placa, refuerzo, punto. |
| TargetId | Elemento afectado. |
| BaseConnectionPointId | Punto base que se modifica. |
| DeltaX / DeltaY / DeltaZ | Desplazamiento manual. |
| Reason | Motivo capturado por el usuario, opcional pero recomendado. |
| CreatedBy | Usuario que aplicó el override. |
| CreatedAt | Fecha. |

### 4.12 Configuraciones reutilizables

Además de plantillas estándar, el usuario debe poder guardar configuraciones reales para reutilizarlas.

Entidad sugerida: `SavedRackFrameConfiguration`.

| Campo | Descripción |
|---|---|
| SavedConfigurationId | Identificador estable. |
| Name | Nombre legible. |
| Description | Descripción. |
| FrameConfiguration | Copia serializada del modelo. |
| CatalogVersion | Versión de catálogo usada. |
| ConnectionPointCatalogVersion | Versión de puntos usada. |
| Tags | Etiquetas: cliente, tipo de rack, altura, fondo, caso especial. |
| CreatedBy | Usuario. |
| ApprovedBy | Ingeniería, si aplica. |
| Status | Borrador, aprobada, obsoleta. |
| CreatedAt / UpdatedAt | Trazabilidad. |

Una plantilla estándar representa una configuración recomendada por ingeniería. Una configuración guardada representa una solución real reutilizable, que puede o no ser estándar.

### 4.13 Estándar base y excepciones

El configurador debe distinguir entre:

- La cabecera estándar base que el sistema carga automáticamente.
- El modelo final que se dibuja.
- Las excepciones que el usuario aplicó sobre el estándar.

Entidad sugerida: `StandardRackFrameBaseline`.

| Campo | Descripción |
|---|---|
| StandardBaselineId | Identificador de la cabecera estándar. |
| Name | Nombre del estándar, por ejemplo `Cabecera estándar omega`. |
| Version | Versión del estándar. |
| AppliesToRackTypeId | Tipo de rack al que aplica. |
| AppliesToPostFamilyId | Familia de poste. |
| DefaultClearHeight | Claro estándar, por ejemplo `44 in`. |
| DefaultBasePlateId | Placa base predeterminada, por ejemplo placa atornillable. |
| DefaultBracingPattern | Celosía estándar. |
| DefaultBracingSideMode | Lado estándar de celosía. |
| DefaultOffsetRuleSetId | Offsets conocidos aplicables. |
| DefaultConnectionPointSetId | Puntos de conexión predeterminados. |
| DefaultHorizontalRules | Horizontales estándar. |
| Source | Regla actual de ingeniería, macro histórica, catálogo, plantilla aprobada. |
| Status | Activo, obsoleto, pruebas. |

Entidad sugerida: `FrameExceptionOverride`.

| Campo | Descripción |
|---|---|
| ExceptionId | Identificador de la excepción. |
| ExceptionType | Claro especial, sin celosía, doble celosía, refuerzo, placa, perfil, punto, offset, horizontal, regla de dibujo. |
| TargetType | Tramo, poste, placa, diagonal, horizontal, punto, regla. |
| TargetId | Elemento afectado. |
| StandardValue | Valor que venía del estándar. |
| OverrideValue | Valor elegido por el usuario. |
| Reason | Motivo opcional: paso de persona, memoria de cálculo, interferencia, criterio de ingeniería. |
| Source | Manual, configuración guardada, plantilla especial, importado. |
| CreatedBy | Usuario. |
| CreatedAt | Fecha. |

Ejemplos de excepciones:

| Excepción | StandardValue | OverrideValue |
|---|---|---|
| Claro especial | `44 in` | `70 in` |
| Tramo sin celosía | `Celosía estándar` | `Sin celosía` |
| Doble celosía | `Sencilla` | `Doble` |
| Refuerzo unilateral | `Sin refuerzo` | `Refuerzo poste derecho` |
| Cambio de placa | `Placa atornillable estándar` | `Placa especial` |
| Cambio de punto | `TroquelCelosia_01` | `TroquelCelosia_02` |

El editor debe mostrar estas excepciones explícitamente, porque representan el trabajo real del ingeniero.

## 5. Estructura de clases sugerida

La estructura sugerida no implica generar código ahora. Define responsabilidades conceptuales.

### 5.1 Dominio

| Clase | Responsabilidad |
|---|---|
| RackFrameConfiguration | Raíz del modelo de cabecera configurable. |
| RackFrameGeneralData | Datos generales: tipo, altura, fondo, unidades, rack, cliente/proyecto si aplica. |
| PostAssembly | Poste izquierdo o derecho como ensamble. |
| PostReinforcement | Refuerzo aplicado a un poste. |
| BasePlatePlacement | Placa base por poste. |
| BracingSegment | Tramo vertical configurable. |
| SegmentSideBracing | Celosía de un lado dentro de un tramo. |
| HorizontalMember | Horizontal global o por tramo. |
| ComponentSelection | Selección de componente de catálogo. |
| EngineeringOverride | Cambio manual respecto a sugerencia o plantilla. |
| ConnectionPointDefinition | Punto de conexión definido por catálogo. |
| ConnectionPointReference | Uso concreto de un punto de conexión dentro de una cabecera. |
| DrawingOffsetRule | Offset versionado aplicable a dibujo o conexión. |
| ManualOffsetOverride | Desplazamiento manual aplicado por el usuario. |
| RackFrameTemplate | Configuración estándar predefinida editable. |
| StandardRackFrameBaseline | Cabecera estándar predeterminada generada por reglas actuales. |
| FrameExceptionOverride | Excepción trazable respecto al estándar. |
| SavedRackFrameConfiguration | Configuración guardada para reutilización futura. |
| RackFrameValidationResult | Errores, advertencias y recomendaciones. |

### 5.2 Aplicación

| Servicio | Responsabilidad |
|---|---|
| RackFrameConfiguratorService | Orquesta crear, editar, validar, sugerir e insertar cabeceras. |
| StandardBaselineService | Crea la cabecera estándar inicial según reglas actuales de ingeniería. |
| FrameExceptionService | Registra, muestra y aplica excepciones sobre el estándar. |
| RackFrameTemplateService | Carga plantillas estándar y las convierte en configuraciones editables. |
| RackFrameSuggestionService | Sugiere tramos, celosía, refuerzos, placas y perfiles. |
| RackFrameValidationService | Ejecuta validaciones de modelo, catálogo y dibujo. |
| RackFrameBomService | Genera BOM preliminar desde el modelo. |
| RackFrameDrawingPlanService | Convierte el modelo validado en plan de dibujo. |
| RackFrameMetadataService | Serializa, guarda y recupera metadatos desde AutoCAD. |
| ConnectionPointResolverService | Resuelve puntos de conexión de catálogo a coordenadas locales del modelo. |
| DrawingOffsetService | Aplica reglas de offset y overrides manuales. |
| SavedConfigurationService | Guarda, carga y versiona configuraciones reutilizables. |

### 5.3 Catálogos

| Repositorio | Responsabilidad |
|---|---|
| PostCatalogRepository | Postes, dimensiones, patrones de barreno y bloques. |
| BraceCatalogRepository | Diagonales/celosías disponibles. |
| HorizontalCatalogRepository | Horizontales disponibles. |
| ReinforcementCatalogRepository | Refuerzos disponibles. |
| BasePlateCatalogRepository | Placas y anclas compatibles. |
| CompatibilityRepository | Compatibilidades entre componentes. |
| TemplateRepository | Configuraciones estándar predefinidas. |
| StandardBaselineRepository | Estándares vigentes por tipo de rack, poste, altura, fondo y familia. |
| BlockDefinitionRepository | Mapeo entre componentes y bloques AutoCAD. |
| ConnectionPointRepository | Puntos de conexión por componente, versión y sistema de coordenadas. |
| DrawingOffsetRuleRepository | Reglas de offset por familia, componente, patrón y versión. |
| SavedConfigurationRepository | Configuraciones guardadas por usuarios o ingeniería. |

### 5.4 Dibujo

| Clase | Responsabilidad |
|---|---|
| RackFrameDrawingPlan | Resultado abstracto que describe qué se dibuja. |
| DrawingBlockPlacement | Inserción de bloque, posición, rotación, escala y propiedades dinámicas. |
| DrawingLineSegment | Geometría generada por código, si aplica. |
| DrawingAnnotation | Textos, etiquetas, marcas y cotas. |
| DrawingLayerAssignment | Asignación de capas por tipo de elemento. |
| DrawingMetadataInstruction | Instrucciones para escribir metadatos. |
| AutoCadRackFrameDrawer | Adaptador que ejecuta el plan usando AutoCAD .NET API. |
| ResolvedConnectionPoint | Punto ya transformado a coordenadas locales de la cabecera. |
| DrawingConnectionInstruction | Instrucción para conectar dos puntos mediante diagonal, horizontal, refuerzo o placa. |

## 6. Representación de tramos de celosía variables

### 6.1 Modelo recomendado

La cabecera debe tener una lista ordenada de `BracingSegment`.

Ejemplo conceptual:

| Tramo | Inicio | Fin | Claro | Patrón | Lado | Perfil |
|---|---:|---:|---:|---|---|---|
| 1 | 0 | 44 | 44 | Diagonal sencilla | Frente | Perfil A |
| 2 | 44 | 114 | 70 | X | Ambos | Perfil B |
| 3 | 114 | 158 | 44 | Doble diagonal | Posterior | Perfil A |
| 4 | 158 | 180 | 22 | Sin celosía + horizontal | Ambos | Perfil C |

El sistema no debe asumir que todos los claros son iguales.

### 6.2 Reglas del modelo

Cada tramo debe cumplir:

- Tener elevación inicial.
- Tener elevación final.
- Tener altura positiva.
- No traslaparse con otro tramo.
- Mantener orden vertical.
- Estar dentro de la altura total de la cabecera.
- Poder marcarse como manual o sugerido.
- Poder usar perfiles diferentes.
- Poder tener celosía distinta por lado.

### 6.3 Edición de tramos

La interfaz debe permitir:

- Agregar tramo.
- Eliminar tramo.
- Duplicar tramo.
- Dividir tramo.
- Fusionar tramos compatibles.
- Reordenar por elevación.
- Cambiar altura del tramo.
- Bloquear tramo manualmente.
- Copiar configuración de un tramo a otro.
- Copiar lado frontal al posterior o viceversa.
- Aplicar una plantilla de celosía a un tramo.

### 6.4 Tramos sugeridos

El sistema puede sugerir tramos con base en:

- Altura total.
- Fondo.
- Tipo de cabecera.
- Tipo de rack.
- Poste seleccionado.
- Plantilla estándar.
- Reglas de claro mínimo/máximo.
- Patrones permitidos por ingeniería.

Pero el usuario debe poder modificar la propuesta antes de dibujar.

## 7. Manejo de patrones de celosía

### 7.1 Tipos mínimos del MVP

| Patrón | Descripción | Requisitos típicos |
|---|---|---|
| Sin celosía | El tramo no lleva diagonales. | Puede requerir horizontales. |
| Diagonal sencilla | Una diagonal en el tramo. | Orientación definida. |
| Doble celosía | Dos diagonales paralelas o configuración reforzada según catálogo. | Perfil y separación definidos. |
| X | Dos diagonales cruzadas. | Revisión de interferencias y conexión central si aplica. |
| K | Dos diagonales que llegan a un punto intermedio o horizontal. | Requiere punto medio u horizontal asociado. |
| Personalizada | Reservada para casos especiales. | Puede quedar fuera del MVP operativo. |

### 7.2 Celosía sencilla

Debe representarse como una o más `BraceMember` dentro de `SegmentSideBracing`.

Datos mínimos:

- Perfil.
- Punto de conexión inferior.
- Punto de conexión superior.
- Orientación.
- Lado.
- Longitud calculada.
- Ángulo calculado.

### 7.3 Celosía doble

La palabra "doble" debe definirse formalmente con ingeniería, porque puede significar:

- Dos diagonales en el mismo lado.
- Diagonal en ambos lados.
- Perfil doble.
- Dos piezas paralelas.
- Doble patrón dentro del mismo claro.

Para el MVP se recomienda no codificar "doble" como booleano ambiguo.

Modelo recomendado:

| Campo | Uso |
|---|---|
| BracingDensity | Sencilla, doble, reforzada. |
| SideMode | Frente, posterior, ambos. |
| BraceCount | Número de diagonales reales en ese lado. |
| BraceArrangement | Paralelas, cruzadas, espejadas, separadas. |

### 7.4 Celosía en X

El patrón X debe generar dos diagonales dentro del mismo tramo y mismo lado.

Validaciones mínimas:

- El tramo debe tener altura suficiente.
- El perfil debe permitir cruce o separación.
- Debe definirse si el cruce es geométrico, con separación, o solo representación 2D.
- Deben existir puntos de conexión válidos en ambos postes.

### 7.5 Celosía en K

El patrón K requiere un punto intermedio.

Datos adicionales:

- Elevación del nodo intermedio.
- Si el nodo cae sobre horizontal intermedio.
- Si el nodo se conecta a poste izquierdo o derecho.
- Perfiles de las dos diagonales.
- Reglas de simetría.

Para el MVP, si se incluye K, debe apoyarse en una regla simple: nodo al 50% del tramo, con horizontal intermedio requerido. Si no se puede validar bien, K puede estar habilitado como patrón manual con advertencia.

### 7.6 Sin celosía

Un tramo sin celosía no significa vacío.

Puede contener:

- Horizontal inferior.
- Horizontal superior.
- Horizontal intermedio.
- Notas.
- Componentes especiales.

Debe validarse si el tipo de cabecera permite claros sin celosía.

## 8. Refuerzos por lado y por poste

### 8.1 Reglas de modelado

Los refuerzos deben estar asociados al poste al que pertenecen:

- Poste izquierdo.
- Poste derecho.

Y también a una ubicación:

- Cara frontal.
- Cara posterior.
- Cara interior.
- Cara exterior.
- Ambas caras.

### 8.2 Casos que debe soportar el MVP

| Caso | Requisito |
|---|---|
| Ambos postes estándar | Sin refuerzos. |
| Solo poste izquierdo reforzado | Refuerzo asociado al poste izquierdo. |
| Solo poste derecho reforzado | Refuerzo asociado al poste derecho. |
| Ambos postes reforzados | Refuerzos independientes o copiados. |
| Refuerzo parcial | Elevación inicial y final configurables. |
| Refuerzo completo | Ocupa la altura total del poste o regla de catálogo. |

### 8.3 Sugerencias automáticas

El sistema puede sugerir refuerzo según:

- Altura.
- Fondo.
- Tipo de rack.
- Tipo de poste.
- Criterio de ingeniería.
- Plantilla estándar.

Pero el usuario debe poder aceptar, cambiar o retirar el refuerzo si tiene permisos para hacerlo.

## 9. Horizontales y placas

### 9.1 Horizontales

Los horizontales pueden originarse por:

- Patrón de celosía.
- Regla de cabecera.
- Plantilla estándar.
- Selección manual del usuario.

El modelo debe distinguir:

| Tipo | Ejemplo |
|---|---|
| Horizontal inferior | Cierre o base de celosía. |
| Horizontal superior | Cierre superior del tramo o cabecera. |
| Horizontal intermedio | Requerido por K o por criterio de ingeniería. |
| Horizontal adicional | Agregado manualmente. |
| Horizontal de cierre | Ajuste de remate superior. |

Cada horizontal debe tener elevación, lado, perfil y longitud.

### 9.2 Placas

La placa base debe poder variar por poste.

El MVP debe permitir:

- Elegir placa para poste izquierdo.
- Elegir placa para poste derecho.
- Copiar placa izquierda a derecha.
- Validar compatibilidad placa-poste.
- Asociar anclas o tornillería mínima.
- Usar sugerencia automática por tipo de poste y cabecera.

Las placas deben aparecer en BOM preliminar aunque su dibujo sea inicialmente simplificado.

## 10. Puntos de conexión y offsets configurables

### 10.1 Decisión de almacenamiento

Los puntos de conexión deben guardarse con una combinación de catálogo, configuración, atributos de bloque y metadatos. Cada nivel tiene un propósito distinto.

| Ubicación | Qué debe guardar | Motivo |
|---|---|---|
| Catálogo externo | Definición oficial de puntos: código, coordenadas locales, componente, cara, tipo, versión y vigencia. | Es la fuente de verdad técnica. Permite cambiar puntos sin recompilar. |
| Configuración | Rutas, unidades por defecto, conjunto activo de puntos, tolerancias visuales, permisos de override. | Controla el ambiente de trabajo, no el producto. |
| Atributos de bloque | Datos visibles o consultables mínimos: código de componente, instancia, orientación, tal vez punto base. | Útil para inspección AutoCAD, pero no debe ser la fuente de verdad. |
| Metadatos AutoCAD | Modelo completo usado para generar la cabecera, referencias a puntos y overrides aplicados. | Permite editar o regenerar después. |
| Código C# | Resolución geométrica, transformación de coordenadas, validación e interpretación de reglas. | Es comportamiento del software, no catálogo de producto. |

Regla principal:

Los offsets y puntos como `X = 2.75 in` y `Y = 7.58 in` deben vivir en catálogo o en overrides del modelo, no como literales dentro del código C#.

### 10.2 Estructura de catálogo para puntos de conexión

Estructura mínima recomendada:

| Tabla | Propósito |
|---|---|
| ConnectionPointSets | Conjunto versionado de puntos para una familia o componente. |
| ConnectionPoints | Puntos individuales con coordenadas locales. |
| ConnectionPointAliases | Alias legibles o históricos para puntos. |
| ConnectionPointCompatibility | Qué perfiles, diagonales, horizontales o placas pueden conectarse a cada punto. |
| CoordinateSystems | Sistemas locales de coordenadas usados por componentes. |
| DrawingOffsetRules | Offsets estándar de dibujo o colocación. |
| ConnectionPointVersions | Historial de cambios de puntos. |

Campos clave de `ConnectionPointSets`:

| Campo | Descripción |
|---|---|
| ConnectionPointSetId | Identificador del conjunto. |
| ComponentCatalogId | Componente asociado. |
| ComponentFamilyId | Familia asociada si aplica a varios componentes. |
| Version | Versión del conjunto. |
| Units | Unidad. |
| CoordinateSystemId | Sistema local usado. |
| Status | Activo, obsoleto, pruebas. |
| Notes | Notas técnicas. |

Campos clave de `ConnectionPoints`:

| Campo | Descripción |
|---|---|
| ConnectionPointId | Identificador estable. |
| ConnectionPointSetId | Conjunto al que pertenece. |
| Code | Código, por ejemplo `TroquelCelosia_01`. |
| PointType | Celosía, placa, horizontal, refuerzo, ancla, referencia. |
| LocalX / LocalY / LocalZ | Coordenadas locales. |
| Face | Frente, posterior, interior, exterior, centro. |
| Side | Izquierdo, derecho, ambos o no aplica. |
| Sequence | Orden dentro del patrón. |
| IsPreferred | Si se sugiere por defecto. |
| Status | Activo, obsoleto, bloqueado. |

Ejemplo:

| ConnectionPointSet | Code | LocalX | LocalY | Uso |
|---|---|---:|---:|---|
| PosteOmega3x3_v1 | TroquelCelosia_01 | 2.75 in | 7.58 in | Inicio de celosía. |
| PosteOmega3x3_v1 | TroquelCelosia_02 | 2.75 in | 11.58 in | Siguiente troquel. |
| PosteOmega3x3_v1 | PlacaBase_01 | 0 in | 0 in | Referencia de placa base. |

### 10.3 Reglas para manejar offsets

Los offsets deben resolverse en este orden:

1. Punto de conexión seleccionado por el usuario.
2. Override manual del usuario, si existe.
3. Offset de plantilla, si la configuración viene de una plantilla.
4. Offset de catálogo o regla de dibujo.
5. Valor por defecto seguro, solo si está definido explícitamente.

No debe existir fallback silencioso a números hardcodeados.

Cada offset debe registrar:

- Qué elemento afecta.
- Desde qué punto base se mide.
- En qué sistema de coordenadas se interpreta.
- Unidad.
- Origen: catálogo, plantilla, sugerencia o manual.
- Si fue modificado por el usuario.

Ejemplos de offsets que deben ser datos:

| Offset | Dónde vivir |
|---|---|
| Distancia desde cara de poste a línea de celosía. | Catálogo o regla de dibujo por familia de poste. |
| Corrección visual para insertar bloque dinámico. | Manifiesto de bloque o regla de dibujo. |
| Desplazamiento manual por caso especial. | Override guardado en metadatos del modelo. |
| Punto de primer troquel válido. | Catálogo de puntos de conexión. |

### 10.4 Sistema de coordenadas

Cada componente debe declarar su sistema local:

- Origen local.
- Sentido positivo de X.
- Sentido positivo de Y.
- Cara frontal/posterior.
- Punto base de bloque, si no coincide con el origen técnico.

El motor debe transformar:

1. Coordenadas locales del componente.
2. Coordenadas locales de la cabecera.
3. Coordenadas del dibujo AutoCAD.

Esta separación evita confundir punto técnico de conexión con punto base de bloque.

### 10.5 Recomendación sobre bloques y puntos

Los bloques pueden tener atributos que ayuden a identificar componente y punto base, pero no deben ser la única fuente de puntos de conexión.

Recomendación:

- Catálogo: define puntos técnicos.
- Bloque: representa visualmente el componente.
- Manifiesto de bloque: indica punto base, propiedades dinámicas y offsets visuales.
- Metadatos: guardan qué puntos se usaron en una cabecera específica.

Si en el futuro se desea extraer puntos desde bloques, puede hacerse como herramienta auxiliar, pero no debe ser obligatorio para el MVP.

## 11. Qué debe controlar la interfaz

### 11.1 Estructura recomendada de UI

La UI debe funcionar como configurador, no como formulario lineal.

Vistas o secciones recomendadas:

| Sección | Contenido |
|---|---|
| Datos generales | Tipo de cabecera, tipo de rack, altura, fondo, unidades, postes y estándar base aplicado. |
| Estándar base | Resumen de la cabecera estándar cargada: claros 44 in, placa atornillable, celosía estándar, offsets conocidos. |
| Postes | Poste izquierdo, poste derecho, orientación, refuerzos, placas. |
| Tramos | Lista editable de claros/tramos de celosía. |
| Editor de tramo | Altura, patrón, lado, perfil, horizontales, puntos de conexión y offsets del tramo seleccionado. |
| Puntos de conexión | Selección y revisión de puntos por poste, tramo y componente. |
| Offsets/reglas de dibujo | Overrides manuales permitidos y reglas activas. |
| Excepciones | Lista de cambios realizados respecto al estándar. |
| Vista previa | Representación esquemática de la cabecera antes de insertar. |
| Validación | Errores, advertencias y recomendaciones. |
| BOM preliminar | Componentes, cantidades, longitudes y origen de selección. |
| Inserción | Punto de inserción, escala, orientación y capas. |

### 11.2 Controles mínimos

La interfaz debe permitir:

- Crear cabecera estándar automáticamente.
- Recalcular estándar base después de cambiar datos generales.
- Aplicar plantilla estándar alternativa.
- Cargar configuración guardada.
- Guardar configuración para reutilizar.
- Agregar tramos.
- Editar altura de cada tramo.
- Seleccionar patrón de celosía por tramo.
- Seleccionar lado de aplicación.
- Seleccionar perfiles.
- Seleccionar puntos de conexión.
- Editar offsets permitidos.
- Copiar configuración entre lados.
- Marcar tramos como bloqueados.
- Aceptar sugerencias del sistema.
- Ver lista de excepciones respecto al estándar.
- Restaurar un tramo o componente al estándar.
- Ver errores antes de insertar.
- Ver BOM preliminar.
- Insertar en AutoCAD.

### 11.3 Flujo recomendado de interfaz

El flujo debe ser manual asistido:

1. El usuario abre `RACKCABECERA`.
2. Captura o confirma datos generales: tipo de rack, altura, fondo y postes.
3. El sistema carga una cabecera estándar predeterminada con claros estándar de 44 in, placa base atornillable, celosía estándar, offsets conocidos y puntos de conexión preferidos.
4. El usuario revisa la vista previa estándar.
5. El usuario modifica solo excepciones: claro especial, tramo sin celosía, doble celosía, refuerzo, placa, perfil, punto de conexión, offset u horizontal.
6. Cada modificación se registra como `FrameExceptionOverride`.
7. El sistema actualiza vista previa y BOM preliminar.
8. El sistema valida consistencia mínima.
9. El usuario corrige o acepta advertencias.
10. El usuario inserta la cabecera en AutoCAD.
11. El sistema guarda modelo final, estándar base, excepciones, puntos usados, offsets y metadatos.

El sistema puede ofrecer botones como "Restaurar estándar", "Aplicar estándar vigente", "Copiar excepción", "Usar puntos preferidos" o "Calcular longitud", pero esas acciones deben dejar resultados editables.

### 11.4 Vista previa

La vista previa no necesita ser AutoCAD completo.

Para el MVP basta una vista esquemática que muestre:

- Postes.
- Tramos.
- Diagonales.
- Horizontales.
- Refuerzos.
- Placas.
- Diferencia entre frente/posterior si aplica.
- Marcadores de error.

La vista previa debe ayudar a configurar. El dibujo final sigue ocurriendo en AutoCAD.

### 11.5 Diseño orientado a excepciones

La interfaz debe optimizar el caso más frecuente: la cabecera estándar ya es casi correcta.

Por eso debe mostrar:

| Elemento | Comportamiento |
|---|---|
| Tramos estándar | Se muestran como normales o sin marca especial. |
| Tramos modificados | Se resaltan como excepción. |
| Componentes cambiados | Muestran valor estándar y valor actual. |
| Puntos modificados | Muestran punto estándar, punto seleccionado y offset aplicado. |
| BOM | Distingue piezas estándar de piezas agregadas o cambiadas por excepción. |
| Validación | Advierte sobre excepciones inusuales, pero no las bloquea si son consistentes. |

Acciones rápidas recomendadas:

- Cambiar claro de tramo.
- Marcar tramo como "sin celosía".
- Convertir celosía sencilla a doble.
- Cambiar lado de celosía.
- Agregar refuerzo a poste izquierdo o derecho.
- Cambiar placa.
- Cambiar perfil en tramo seleccionado.
- Cambiar punto de conexión.
- Restaurar tramo al estándar.
- Copiar excepción a otro tramo.

La lista de excepciones debe funcionar como una bitácora de diseño: el ingeniero puede ver exactamente qué se apartó de la cabecera estándar antes de insertar.

## 12. Validaciones mínimas

Las validaciones del MVP deben enfocarse en consistencia, completitud y posibilidad de dibujo. No deben pretender reemplazar la memoria de cálculo ni aprobar automáticamente una configuración de ingeniería.

Debe distinguirse:

| Tipo | Descripción |
|---|---|
| Error bloqueante | Falta un dato necesario, hay traslape de tramos, punto inexistente, bloque faltante o geometría imposible. |
| Advertencia | La configuración es inusual, usa override manual o sale de una recomendación estándar. |
| Nota | Información útil: longitud calculada, ángulo resultante, plantilla de origen, punto usado. |

### 12.1 Validaciones de datos generales

- Altura obligatoria y positiva.
- Fondo obligatorio y positivo.
- Tipo de cabecera obligatorio.
- Tipo de rack obligatorio si afecta reglas.
- Unidad definida.
- Catálogo disponible.

### 12.2 Validaciones de postes

- Poste izquierdo seleccionado.
- Poste derecho seleccionado.
- Postes compatibles con tipo de cabecera.
- Postes compatibles con altura y fondo.
- Refuerzo compatible con poste.
- Refuerzo dentro de la altura del poste.
- Refuerzos no duplicados indebidamente.

### 12.3 Validaciones de tramos

- La lista de tramos no debe estar vacía.
- Cada tramo debe tener altura positiva.
- Los tramos no deben traslaparse.
- Los tramos deben estar dentro de la altura total.
- Debe detectarse si hay huecos verticales no definidos.
- El tramo debe tener patrón válido.
- El patrón debe ser compatible con la altura del tramo.
- El patrón debe ser compatible con el lado seleccionado.

### 12.4 Validaciones de celosía

- Perfil de diagonal seleccionado cuando el patrón lo requiera.
- Longitud calculada dentro de límites del componente.
- Ángulo dentro de rango permitido.
- Puntos de conexión compatibles con patrón de barrenos.
- Puntos de conexión seleccionados existen en el catálogo activo.
- Puntos de inicio y fin pertenecen a componentes compatibles.
- Offsets manuales tienen unidad y sistema de coordenadas.
- Offsets no generan geometría claramente imposible.
- X y K no deben crear geometría imposible.
- Doble celosía debe tener significado técnico definido por el catálogo.
- Celosía en ambos lados debe revisar interferencias básicas.

### 12.5 Validaciones de horizontales

- Horizontales requeridos por patrón presentes.
- Elevaciones dentro de la cabecera.
- Longitud compatible con fondo.
- Perfil compatible con poste y conexiones.
- Puntos izquierdo y derecho seleccionados.
- Puntos compatibles con horizontal.
- No duplicar horizontales equivalentes sin intención.

### 12.6 Validaciones de placas

- Placa seleccionada para cada poste, si es obligatoria.
- Placa compatible con poste.
- Punto de placa base válido.
- Anclas compatibles con placa.
- Orientación válida.

### 12.7 Validaciones de dibujo

- Bloques requeridos disponibles.
- Propiedades dinámicas requeridas disponibles.
- Manifiesto de bloque compatible con el componente.
- Punto base de bloque conocido.
- Offset visual de bloque definido si se requiere.
- Capas disponibles o creables.
- Nombre de bloque o identificador de ensamble no entra en conflicto.
- Punto de inserción válido.
- Escala y unidades consistentes.

## 13. Generación del plan de dibujo

### 13.1 Separar modelo, plan y dibujo

El flujo correcto debe ser:

1. El usuario configura `RackFrameConfiguration`.
2. El sistema valida el modelo.
3. El sistema resuelve referencias de catálogo.
4. El sistema resuelve puntos de conexión y offsets.
5. El sistema calcula geometría y cantidades.
6. El sistema genera `RackFrameDrawingPlan`.
7. El adaptador AutoCAD ejecuta el plan.
8. El sistema guarda metadatos.

El modelo no debe contener `ObjectId`, `BlockReference`, `Transaction` ni objetos propios de AutoCAD.

### 13.2 Contenido del plan de dibujo

`RackFrameDrawingPlan` debe incluir:

| Elemento | Descripción |
|---|---|
| AssemblyBlock | Bloque o grupo raíz de la cabecera. |
| ComponentPlacements | Inserciones de postes, refuerzos, placas, diagonales y horizontales. |
| DynamicProperties | Valores de propiedades dinámicas por bloque. |
| ConnectionInstructions | Conexiones entre puntos resueltos. |
| ResolvedConnectionPoints | Puntos de conexión transformados a coordenadas locales de cabecera. |
| OffsetApplications | Offsets de catálogo, plantilla y overrides manuales aplicados. |
| GeneratedGeometry | Líneas o geometría auxiliar si no se usa bloque. |
| Layers | Capas por componente. |
| Annotations | Etiquetas, marcas o textos mínimos. |
| Dimensions | Cotas si entran en el MVP. |
| BomTable | Tabla preliminar si se decide dibujarla. |
| MetadataInstructions | Qué metadatos se escriben y dónde. |

### 13.3 Reglas para coordenadas

Se recomienda definir un sistema local de cabecera:

- Origen local en la base del poste izquierdo.
- Eje X hacia el fondo.
- Eje Y hacia la altura.
- Eje Z reservado para futuras vistas 3D o lados.

Luego, al insertar en AutoCAD:

- Se transforma el sistema local al punto de inserción.
- Se aplica rotación.
- Se aplica escala si procede.
- Se insertan los objetos.

### 13.4 Bloques vs geometría por código

Usar bloques para:

- Postes.
- Refuerzos.
- Placas.
- Perfiles estándar de diagonales.
- Horizontales estándar.

Generar por código:

- Líneas guía de vista previa.
- Ejes.
- Etiquetas.
- Cotas.
- Geometría simplificada cuando no exista bloque.
- Marcadores de validación.
- Conectores temporales o representaciones esquemáticas basadas en puntos.

El plan de dibujo debe permitir ambos.

Regla práctica:

| Caso | Recomendación |
|---|---|
| Componente estándar con geometría estable | Bloque normal o dinámico. |
| Componente con variación visual simple | Bloque dinámico con propiedades mapeadas. |
| Conexión entre dos puntos configurados | Geometría calculada o bloque colocado entre puntos resueltos. |
| Patrón manual especial todavía no estandarizado | Geometría por código o bloque genérico con metadatos claros. |
| Punto de conexión técnico | Catálogo y metadatos, no geometría escondida dentro del bloque. |

Los bloques deben obedecer al modelo. No deben decidir por sí solos la ingeniería de la cabecera.

## 14. Metadatos para edición posterior

### 14.1 Qué debe guardarse

Para editar o regenerar una cabecera después, se debe guardar una copia completa del modelo.

Metadatos mínimos:

| Dato | Uso |
|---|---|
| FrameId | Identificador estable de la cabecera. |
| SchemaVersion | Versión del formato de metadatos. |
| PluginVersion | Versión del plugin que generó la cabecera. |
| CatalogVersion | Versión de catálogo usada. |
| ConnectionPointCatalogVersion | Versión de puntos de conexión usada. |
| TemplateId | Plantilla usada, si aplica. |
| RackFrameConfiguration | Modelo completo serializado. |
| ConnectionPointReferences | Puntos de conexión usados por cada componente. |
| OffsetRulesApplied | Reglas de offset aplicadas. |
| ManualOffsetOverrides | Offsets manuales capturados. |
| DrawingPlanVersion | Versión del generador de dibujo. |
| ObjectMap | Relación entre elementos del modelo y objetos AutoCAD generados. |
| UserOverrides | Cambios manuales relevantes. |
| CreatedAt / UpdatedAt | Trazabilidad temporal. |

### 14.2 Dónde guardar metadatos

Recomendación:

- En el bloque raíz o entidad contenedora de la cabecera mediante Extension Dictionary/XRecord.
- En cada componente principal, guardar identificadores mínimos.
- Opcionalmente, mantener una copia externa del modelo en archivo de proyecto futuro.

El objeto raíz debe contener el modelo completo.
Los componentes individuales deben contener:

- FrameId.
- ComponentInstanceId.
- CatalogId.
- ComponentType.
- SegmentId si pertenece a un tramo.
- ConnectionPointIds usados por el componente, si aplica.

### 14.3 Edición posterior

El comando futuro `RACKEDITAR` debe:

1. Seleccionar una cabecera existente.
2. Leer sus metadatos.
3. Reconstruir `RackFrameConfiguration`.
4. Abrir el configurador con los datos actuales.
5. Permitir cambios.
6. Regenerar el dibujo.
7. Actualizar metadatos.

Por eso el MVP debe guardar metadatos desde el primer corte, aunque la edición completa llegue después.

## 15. Catálogo, configuración y código

### 15.1 Debe vivir en catálogo

| Información | Motivo |
|---|---|
| Postes disponibles | Cambian por producto. |
| Dimensiones de postes | Son datos técnicos. |
| Patrones de perforación | Dependen del producto. |
| Refuerzos disponibles | Cambian por catálogo. |
| Placas disponibles | Cambian por producto/proveedor. |
| Diagonales/perfiles disponibles | Son componentes. |
| Horizontales disponibles | Son componentes. |
| Longitudes estándar | Dato técnico/comercial. |
| Pesos | BOM y cotización futura. |
| Compatibilidades | Regla técnica mantenible. |
| Bloques AutoCAD asociados | Activo externo. |
| Propiedades dinámicas esperadas | Dependen de bloques. |
| Plantillas estándar de cabecera | Reglas de empresa editables. |
| Criterios de sugerencia configurables | Deben poder ajustarse sin recompilar. |
| Puntos de conexión | Son datos técnicos del componente. |
| Conjuntos de puntos versionados | Permiten cambios sin romper modelos anteriores. |
| Sistemas locales de coordenadas | Necesarios para resolver puntos. |
| Reglas de offset estándar | Cambian por componente, bloque o criterio de dibujo. |
| Compatibilidad punto-componente | Define qué puede conectarse dónde. |

### 15.2 Debe vivir en configuración

| Información | Motivo |
|---|---|
| Rutas de catálogos | Ambiente local/departamental. |
| Rutas de bloques | Instalación. |
| Capas | Estándar CAD. |
| Estilos de texto/cota | Estándar CAD. |
| Unidad por defecto | Preferencia o estándar. |
| Colores de vista previa | UI. |
| Permisos de override | Política de uso. |
| Feature flags | Activar/desactivar patrones o módulos. |
| Catálogo de puntos activo | Ambiente o versión seleccionada. |
| Tolerancias de validación visual | Preferencia técnica o estándar CAD. |
| Reglas de bloqueo de plantillas | Política interna. |

### 15.3 Debe vivir en atributos de bloque

| Información | Motivo |
|---|---|
| Código de componente | Inspección visual o consulta rápida. |
| Id de instancia | Relación con el modelo. |
| Id de cabecera | Trazabilidad. |
| Punto base del bloque, si se desea exponer | Ayuda de diagnóstico. |

Los atributos de bloque no deben ser la fuente principal de puntos técnicos. Pueden ayudar a inspeccionar o depurar, pero el catálogo debe mandar.

### 15.4 Debe vivir en metadatos del dibujo

| Información | Motivo |
|---|---|
| Modelo completo de cabecera | Edición posterior. |
| Puntos de conexión usados | Regeneración trazable. |
| Offsets manuales | Repetibilidad del dibujo. |
| Plantilla o configuración de origen | Auditoría. |
| Versiones de catálogo y puntos | Compatibilidad futura. |
| Overrides del usuario | Distinguir estándar vs caso especial. |

### 15.5 Debe vivir en código C#

| Información | Motivo |
|---|---|
| Modelo de dominio | Estructura estable del sistema. |
| Validación de invariantes | Integridad del modelo. |
| Motor de cálculo geométrico | Comportamiento del producto de software. |
| Generación de plan de dibujo | Traducción modelo-dibujo. |
| Adaptador AutoCAD | Integración técnica. |
| Serialización de metadatos | Control de versiones del modelo. |
| Flujo de comandos | Orquestación de casos de uso. |
| Manejo de errores | Robustez. |
| Resolución de puntos de conexión | Transformación geométrica desde catálogo a dibujo. |
| Aplicación de offsets | Orden, precedencia y validación. |

No deben vivir en código:

- Nombres de postes.
- Medidas específicas de perfiles.
- Listas de plantillas estándar.
- Nombres de bloques.
- Propiedades dinámicas sin mapeo de catálogo.
- Reglas de compatibilidad que ingeniería pueda cambiar.
- Coordenadas como `X = 2.75 in`, `Y = 7.58 in`.
- Offsets de conexión sin nombre ni versión.

## 16. Manual vs sugerido por el sistema

### 16.1 Debe controlar manualmente el usuario

| Decisión | Motivo |
|---|---|
| Aceptar configuración final | Responsabilidad de ingeniería. |
| Cambiar altura de tramos | Variaciones reales no siempre formulaicas. |
| Elegir patrón por tramo | Puede depender de criterio técnico. |
| Decidir lado de celosía | Puede depender de acceso, interferencia o estándar. |
| Aplicar o quitar refuerzo | Puede depender de criterio de proyecto. |
| Cambiar placa base | Puede depender de obra o anclaje. |
| Bloquear un tramo | Evita que sugerencias lo alteren. |
| Aceptar overrides | Trazabilidad. |
| Seleccionar puntos de conexión | La lógica fina de armado será definida gradualmente por ingeniería. |
| Ajustar offsets permitidos | Puede depender del sistema de dibujo, bloque o caso especial. |
| Elegir horizontales manuales | Puede depender de memoria de cálculo o criterio de armado. |
| Guardar configuración reutilizable | Captura conocimiento real de ingeniería. |

### 16.2 Puede sugerir automáticamente el sistema

| Sugerencia | Base |
|---|---|
| Tramos iniciales | Altura, plantilla, tipo de rack. |
| Claro recomendado | Catálogo y reglas de ingeniería. |
| Patrón de celosía recomendado | Tipo de cabecera y altura de tramo. |
| Perfil recomendado | Longitud, ángulo, compatibilidad. |
| Refuerzo recomendado | Altura, poste, criterio configurado. |
| Placa recomendada | Poste y tipo de cabecera. |
| Horizontales requeridos | Patrón de celosía. |
| Puntos preferidos | Catálogo de puntos y plantilla seleccionada. |
| Offsets estándar | Reglas de dibujo del componente o bloque. |
| BOM preliminar | Modelo configurado. |

### 16.3 Regla de oro

Toda sugerencia debe ser visible y editable.

Para el MVP, una sugerencia no debe convertirse en obligación salvo que sea una validación bloqueante de consistencia: dato faltante, punto inexistente, componente incompatible o geometría imposible.

El sistema debe indicar si un dato viene de:

- Plantilla.
- Catálogo.
- Cálculo.
- Selección manual.
- Override.

Esto evita que el configurador se vuelva una caja negra.

## 17. Configuraciones estándar predefinidas

### 17.1 Plantillas editables

Las configuraciones estándar deben representarse como `RackFrameTemplate`.

Una plantilla puede incluir:

- Tipo de cabecera.
- Tipo de rack.
- Rango de alturas aplicable.
- Rango de fondos aplicable.
- Postes recomendados.
- Refuerzos recomendados.
- Placas recomendadas.
- Tramos de celosía sugeridos.
- Patrones por tramo.
- Horizontales por tramo.
- Reglas de perfiles.
- Puntos de conexión sugeridos.
- Offsets estándar.
- Reglas de dibujo sugeridas.
- Indicación de si es la plantilla predeterminada para el MVP.

### 17.2 Uso correcto de plantillas

Flujo recomendado:

1. El sistema elige automáticamente la plantilla estándar vigente según tipo de rack, altura, fondo y poste.
2. El sistema crea una copia editable como `RackFrameConfiguration`.
3. El usuario modifica lo necesario.
4. El sistema guarda:
   - TemplateId.
   - TemplateVersion.
   - Versiones de catálogo y puntos.
   - Overrides hechos por el usuario.

La plantilla no debe limitar al usuario, salvo que existan permisos o reglas bloqueantes definidas por ingeniería.

En el MVP, "crear desde cero" no debe ser el flujo principal. Puede existir más adelante como modo avanzado, pero el comportamiento normal debe ser cargar el estándar vigente.

### 17.3 Beneficios

- Permite iniciar rápido con configuraciones comunes.
- Conserva flexibilidad para casos especiales.
- Ayuda a entrenar usuarios.
- Facilita auditoría: se sabe qué se cambió respecto al estándar.
- Evita que el MVP dependa de una fórmula única.

## 18. Alcance recomendado del MVP corregido

### 18.1 Funcionalidades incluidas

| Funcionalidad | MVP |
|---|---|
| Comando `RACKCABECERA` | Sí. |
| Interfaz de configuración | Sí. |
| Cargar estándar predeterminado | Sí, obligatorio. |
| Crear desde cero | No como flujo principal; posponer o dejar como modo avanzado. |
| Crear desde plantilla | Sí, pero la plantilla estándar se carga automáticamente. |
| Cargar/guardar configuración reutilizable | Sí. |
| Postes izquierdo/derecho independientes | Sí. |
| Refuerzo por poste | Sí. |
| Lista de tramos variables | Sí. |
| Patrones sin celosía, diagonal sencilla, X | Sí. |
| Doble celosía | Sí, si se define formalmente en catálogo. |
| K | Opcional en MVP; recomendable como fase 1.1 si requiere más validación. |
| Celosía por lado | Sí. |
| Horizontales por tramo | Sí. |
| Puntos de conexión configurables | Sí. |
| Offsets manuales controlados | Sí. |
| Lista de excepciones contra estándar | Sí. |
| Placas por poste | Sí. |
| Validación mínima | Sí. |
| BOM preliminar | Sí. |
| Inserción AutoCAD | Sí. |
| Metadatos | Sí. |
| Edición posterior completa | Preparada, puede quedar para siguiente fase. |

### 18.2 Catálogos mínimos

| Catálogo | Mínimo necesario |
|---|---|
| Postes | 2 o 3 postes reales. |
| Refuerzos | 1 o 2 refuerzos compatibles. |
| Diagonales | 1 o 2 perfiles. |
| Horizontales | 1 o 2 perfiles. |
| Placas | 2 placas base. |
| Tornillería | Conjuntos mínimos por unión. |
| Patrones de celosía | None, SingleDiagonal, X y Double si está definido. |
| Plantillas | 2 o 3 configuraciones estándar editables. |
| Estándar predeterminado | Configuración base vigente similar a la macro VBA actual. |
| Puntos de conexión | Puntos mínimos por poste y placa. |
| Reglas de offset | Offsets estándar para dibujo inicial. |
| Bloques | Bloques mínimos para representar cada componente. |

### 18.3 Entregables

| Entregable | Descripción |
|---|---|
| Especificación del modelo | Definición de `RackFrameConfiguration` y entidades relacionadas. |
| Prototipo de UI | Configurador funcional que inicia desde cabecera estándar y permite excepciones. |
| Catálogo mínimo | Componentes y plantillas iniciales. |
| Catálogo de puntos de conexión | Puntos versionados para postes/componentes mínimos. |
| Estándar base MVP | Cabecera estándar con claros de 44 in, placa atornillable, celosía estándar y offsets conocidos. |
| Motor de validación mínimo | Errores y advertencias principales. |
| Motor de BOM preliminar | Conteo de postes, diagonales, horizontales, refuerzos, placas y tornillería. |
| Motor de plan de dibujo | Traducción del modelo a instrucciones de dibujo. |
| Inserción AutoCAD | Dibujo 2D de la cabecera configurada. |
| Metadatos persistentes | Modelo guardado en el dibujo. |
| Configuraciones reutilizables | Guardar y cargar configuraciones definidas por usuario. |
| Casos de prueba | Cabecera estándar sin cambios y cabeceras con excepciones: claro 70, puntos manuales, refuerzo unilateral, X, sin celosía. |

## 19. Casos de prueba mínimos

El MVP debe probar al menos:

| Caso | Objetivo |
|---|---|
| Cabecera estándar sin excepciones | Verificar que el estándar se carga y dibuja correctamente. |
| Primer claro estándar 44 y segundo claro cambiado a 70 | Verificar excepción de claro especial. |
| Tramo sin celosía | Verificar hueco permitido con horizontales. |
| Tramo con diagonal sencilla al frente | Verificar lado único. |
| Tramo con celosía en ambos lados | Verificar duplicación controlada. |
| Tramo con X | Verificar dos diagonales y cruce. |
| Celosía con punto `TroquelCelosia_01` | Verificar uso de catálogo de puntos. |
| Offset manual en diagonal | Verificar override y metadatos. |
| Poste izquierdo con refuerzo | Verificar asimetría de postes. |
| Placas distintas por poste | Verificar selección independiente. |
| Horizontal intermedio manual | Verificar edición manual. |
| Plantilla estándar modificada | Verificar overrides. |
| Configuración guardada y reutilizada | Verificar persistencia fuera del dibujo. |
| Restaurar tramo al estándar | Verificar eliminación de excepción. |
| Error de tramo fuera de altura | Verificar validación. |
| Punto de conexión inexistente | Verificar error bloqueante. |
| Bloque faltante | Verificar error de dibujo claro. |

## 20. Roadmap: de configurador manual a calculadora semi-automática

### 20.1 Fase 1: Configurador estándar con excepciones

Objetivo:

Permitir que el usuario parta de una cabecera estándar vigente y modifique únicamente excepciones con ayuda de catálogos, puntos de conexión, plantillas editables y validaciones mínimas.

Capacidades:

- Cargar cabecera estándar automáticamente.
- Editar claros especiales.
- Marcar tramos sin celosía o con doble celosía.
- Agregar refuerzos por lado/poste.
- Seleccionar puntos de conexión.
- Editar offsets permitidos.
- Seleccionar perfiles, refuerzos, placas y horizontales.
- Registrar excepciones respecto al estándar.
- Restaurar elementos al estándar.
- Guardar configuraciones reutilizables.
- Generar dibujo desde el modelo.
- Guardar metadatos completos.

### 20.2 Fase 2: Plantillas inteligentes

Objetivo:

Convertir configuraciones frecuentes en plantillas con puntos, offsets y componentes sugeridos.

Capacidades:

- Plantillas por tipo de rack, altura y fondo.
- Plantillas aprobadas por ingeniería.
- Comparación entre plantilla y configuración modificada.
- Registro de overrides.

### 20.3 Fase 3: Reglas parciales de sugerencia

Objetivo:

Automatizar solo decisiones acotadas y verificables.

Ejemplos:

- Sugerir puntos preferidos para un poste.
- Sugerir horizontales requeridos por patrón.
- Sugerir longitud de diagonal entre dos puntos.
- Sugerir placa compatible con poste.
- Advertir si un tramo supera un rango definido.

### 20.4 Fase 4: Validación de ingeniería más rica

Objetivo:

Agregar reglas de memoria de cálculo sin quitar control al usuario.

Capacidades:

- Validaciones por tipo de rack.
- Rangos por patrón de celosía.
- Reglas de compatibilidad estructural.
- Advertencias por configuración inusual.
- Reporte de criterios usados.

### 20.5 Fase 5: Configuración semi-automática

Objetivo:

El sistema puede proponer una cabecera completa, pero el usuario la revisa y edita antes de dibujar.

Capacidades:

- Generar propuesta inicial por altura/fondo/tipo de rack.
- Elegir entre alternativas.
- Bloquear tramos manuales.
- Recalcular solo partes no bloqueadas.
- Mantener trazabilidad de decisiones automáticas y manuales.

### 20.6 Fase 6: Integración con cotización y diseño completo

Objetivo:

Usar el modelo validado para alimentar BOM, cotización y diseño de racks completos.

Capacidades:

- BOM formal.
- Exportación a Excel/cotizador.
- Configuraciones por proyecto.
- Reutilización de cabeceras aprobadas.
- Diseño de módulos y bahías.

## 21. Riesgos del nuevo MVP

| Riesgo | Mitigación |
|---|---|
| El configurador crece demasiado | Limitar patrones y catálogos iniciales. |
| "Doble celosía" queda ambiguo | Definirlo como datos de catálogo antes de implementarlo. |
| La UI se vuelve compleja | Usar lista de tramos + editor de tramo seleccionado. |
| Se mezclan sugerencias con decisiones manuales | Registrar `Source` y `IsOverride`. |
| El dibujo no puede editarse después | Guardar modelo completo como metadatos desde MVP. |
| Las plantillas se vuelven rígidas | Cargarlas como copia editable. |
| Validaciones insuficientes | Separar errores bloqueantes de advertencias. |
| Bloques no coinciden con el modelo | Usar manifiesto de bloques y validación previa. |
| Automatizar ingeniería demasiado pronto | Mantener el MVP manual asistido y automatizar solo reglas formalizadas. |
| Offsets convertidos en números mágicos | Exigir catálogo de puntos, reglas de offset y overrides versionados. |
| Catálogo de puntos mal gobernado | Versionar puntos y guardar la versión usada en cada cabecera. |
| El usuario pierde control por sugerencias agresivas | Toda sugerencia debe ser editable y trazable. |
| Configuraciones especiales se vuelven invisibles | Guardar notas, overrides y origen de cada decisión. |

### 21.1 Riesgos de automatizar demasiado pronto

Automatizar demasiado pronto puede provocar:

- Reglas incompletas que parecen oficiales.
- Usuarios forzando el sistema con geometría manual.
- Dificultad para representar casos reales.
- Errores de ingeniería ocultos detrás de una propuesta automática.
- Catálogos rígidos antes de entender todas las variantes.
- Código lleno de excepciones para casos especiales.

La estrategia correcta es capturar primero buenas configuraciones manuales, guardar sus datos, aprender patrones reales y automatizar gradualmente solo lo que ya esté formalizado.

## 22. Conclusión

El MVP corregido debe construir una base más realista para ingeniería: un configurador manual asistido de cabeceras por tramos.

La cabecera debe modelarse como un ensamble compuesto por postes independientes, refuerzos, placas, tramos de celosía, patrones por lado, horizontales, puntos de conexión, offsets y selecciones de catálogo.

El sistema debe poder sugerir configuraciones estándar, pero siempre como punto de partida editable. La flexibilidad del usuario y la captura explícita del criterio de ingeniería son parte central del producto.

La salida AutoCAD debe generarse desde el modelo configurado y guardar metadatos suficientes para permitir edición, regeneración, auditoría de puntos usados y reutilización posterior.

Esta orientación evita encerrar el proyecto en una fórmula única o en una calculadora prematura. Primero se captura el conocimiento real de ingeniería; después se automatiza gradualmente lo que ya esté definido, probado y versionado.
