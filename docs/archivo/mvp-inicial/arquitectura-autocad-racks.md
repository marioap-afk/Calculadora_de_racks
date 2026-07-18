# Documento técnico: Plataforma AutoCAD .NET para diseño de racks industriales

> **Archivo histórico; no es una fuente vigente.** Consulta [ARCHITECTURE](../../ARCHITECTURE.md)
> y los [ADRs](../../adr/README.md).

## 1. Propósito del sistema

El objetivo es construir una plataforma profesional para automatizar tareas de ingeniería y dibujo de sistemas de racks industriales dentro de AutoCAD completo, usando AutoCAD .NET API y C#.

El sistema debe comenzar con un MVP acotado: cargar una cabecera estándar predeterminada y permitir que el usuario modifique excepciones como claros especiales, tramos sin celosía, doble celosía, refuerzos, placas, perfiles, puntos de conexión y offsets. Después debe dibujar automáticamente desde ese modelo. A partir de esa base, la arquitectura debe permitir crecer hacia diseño completo de racks, validaciones, catálogos, listas de materiales, integración con Excel y conexión con un archivo cotizador existente.

La decisión más importante es separar el sistema en dos mundos:

- Lógica de ingeniería independiente de AutoCAD.
- Adaptadores específicos para leer, dibujar y modificar objetos dentro de AutoCAD.

Esto permite probar cálculos, catálogos y reglas sin abrir AutoCAD, reduce riesgos de mantenimiento y evita que el complemento se convierta en una colección de comandos difíciles de extender.

## 2. Principios de arquitectura

1. AutoCAD debe ser el entorno de ejecución y dibujo, no el lugar donde vive toda la lógica.
2. Los catálogos, dimensiones, capacidades, compatibilidades y códigos comerciales deben vivir fuera del código C#.
3. El código C# debe contener reglas estables, flujos de trabajo, validaciones, composición geométrica y comunicación con AutoCAD.
4. Cada elemento dibujado debe poder rastrearse contra un componente de catálogo y contra una instancia del modelo de ingeniería.
5. Los bloques deben ser versionados, parametrizados y administrados como una biblioteca controlada.
6. El dibujo generado debe ser editable, pero también regenerable.
7. El MVP debe resolver un problema real sin cerrar el camino hacia un sistema departamental.
8. Las decisiones deben favorecer mantenibilidad, pruebas, trazabilidad y despliegue controlado.

## 3. Arquitectura general propuesta

### 3.1 Capas principales

La solución debería organizarse en capas lógicas:

| Capa | Responsabilidad |
|---|---|
| AutoCAD Plugin | Registrar comandos, cargar la aplicación, abrir pantallas, iniciar transacciones y coordinar con AutoCAD. |
| UI | Formularios, paletas, validación visual de datos, selección de catálogos y presentación de errores. |
| Application Services | Casos de uso: crear cabecera, validar configuración, generar dibujo, actualizar cabecera existente. |
| Domain Model | Representación conceptual de racks, cabeceras, componentes, dimensiones, reglas y resultados. |
| Calculation Engine | Cálculos dimensionales, compatibilidades, validaciones y selección de componentes. |
| Drawing Engine | Traduce el modelo de ingeniería a instrucciones de dibujo independientes de AutoCAD. |
| AutoCAD Adapter | Inserta bloques, crea capas, dibuja geometría, escribe atributos, dimensiones y metadatos. |
| Catalog/Data Access | Lee componentes, bloques, configuraciones, reglas tabulares y versiones de catálogo. |
| Integration Services | Integraciones futuras con Excel, cotizador, bases de datos, ERP o reportes. |
| Infrastructure | Logging, configuración, manejo de errores, rutas de archivos, unidades, migraciones y utilerías. |

### 3.2 Estructura sugerida de solución

La solución de Visual Studio podría organizarse así:

| Proyecto | Tipo | Responsabilidad |
|---|---|---|
| RackCad.Plugin | Class Library para AutoCAD | Comandos AutoCAD, inicialización, ribbon/palette, conexión con el documento activo. |
| RackCad.UI | WPF / MVVM | Pantallas, view models, controles, validaciones visuales. |
| RackCad.Application | Class Library | Casos de uso y orquestación. |
| RackCad.Domain | Class Library | Entidades, value objects, interfaces de reglas y modelos base. |
| RackCad.Calculation | Class Library | Cálculos, validaciones y selección de componentes. |
| RackCad.Drawing | Class Library | Modelo de dibujo abstracto: líneas, bloques, cotas, textos, tablas y layers. |
| RackCad.AutoCad | Class Library | Implementación AutoCAD .NET API: transactions, BlockTableRecord, layers, attributes, XData, extension dictionaries. |
| RackCad.Catalogs | Class Library | Acceso a SQLite/JSON/CSV/SQL, repositorios de componentes y bloques. |
| RackCad.Infrastructure | Class Library | Configuración, logging, rutas, serialización, unidades y errores. |
| RackCad.Tests | Test Project | Pruebas unitarias de dominio, cálculo, catálogos y dibujo abstracto. |
| RackCad.Assets | Carpeta o proyecto de contenido | Bloques DWG, plantillas, archivos de capas, catálogos de ejemplo. |

Para el MVP se puede iniciar con menos proyectos físicos, pero conviene mantener la separación lógica desde el principio. Una estructura inicial razonable podría tener cinco proyectos: Plugin, UI, Domain/Application, AutoCad, Tests. Los módulos de cálculo y catálogos pueden crecer después sin romper el diseño.

### 3.3 Flujo de información

El flujo recomendado para crear una cabecera es:

1. El usuario ejecuta un comando desde AutoCAD.
2. El plugin abre una interfaz de configuración.
3. La UI genera una solicitud de diseño con dimensiones, selección de componentes y opciones.
4. Application Services valida la solicitud.
5. Catalog/Data Access carga postes, diagonales, horizontales, placas, tornillería y bloques requeridos.
6. Calculation Engine calcula posiciones, longitudes, compatibilidad y advertencias.
7. Domain Model produce una cabecera válida como modelo de ingeniería.
8. Drawing Engine convierte el modelo en un plan de dibujo abstracto.
9. AutoCAD Adapter crea capas, inserta bloques, genera geometría, agrega cotas y escribe metadatos.
10. El dibujo queda vinculado a una instancia de cabecera con identificadores persistentes.

El punto clave: el motor de cálculo no debe saber qué es un `BlockReference`, una `Transaction` o un `DocumentLock`. Esos detalles pertenecen al adaptador de AutoCAD.

### 3.4 Modelo conceptual base

El sistema debería manejar, como mínimo, estos conceptos:

| Concepto | Descripción |
|---|---|
| RackProject | Proyecto de ingeniería: cliente, obra, unidades, versión de catálogo, fecha, responsable. |
| RackSystem | Sistema de rack dentro de un proyecto. |
| RackFrame / Cabecera | Marco lateral o cabecera compuesta por postes, diagonales, horizontales, placas y tornillería. |
| RackBay | Módulo entre dos cabeceras, donde vivirán largueros, niveles y cargas. |
| ComponentDefinition | Componente de catálogo: poste, larguero, diagonal, placa, tornillo, etc. |
| ComponentInstance | Instancia colocada en un diseño con posición, orientación, parámetros y referencia a catálogo. |
| DrawingInstance | Relación entre una instancia de ingeniería y objetos reales en AutoCAD. |
| ValidationResult | Errores, advertencias y recomendaciones generadas por el motor. |
| BillOfMaterialsItem | Renglón de lista de materiales calculado desde instancias. |

### 3.5 Comandos AutoCAD previstos

No se debe comenzar con muchos comandos. Para el MVP bastan pocos:

| Comando | Propósito |
|---|---|
| RACKCABECERA | Abrir el configurador visual con una cabecera estándar predeterminada, editar excepciones e insertarla en AutoCAD. |
| RACKEDITAR | Seleccionar una cabecera existente y modificar sus parámetros. Puede quedar para una fase posterior del MVP. |
| RACKVALIDAR | Revisar objetos seleccionados y mostrar inconsistencias. |
| RACKINFO | Inspeccionar metadatos de un componente dibujado. |

En fases posteriores podrían agregarse comandos para generar racks completos, importar Excel, exportar materiales, sincronizar catálogos o actualizar bloques.

## 4. Base de datos de componentes

### 4.1 Enfoque recomendado

Para un sistema departamental, los componentes deben almacenarse en catálogos externos versionados. La recomendación es:

- MVP: SQLite local o archivos JSON/CSV controlados, con estructura clara.
- Fase intermedia: SQLite con migraciones y validación de esquema.
- Fase departamental: SQL Server, PostgreSQL u otra base central, con permisos, historial y control de versiones.

SQLite es muy conveniente para comenzar porque:

- No requiere servidor.
- Es fácil de distribuir con el plugin.
- Permite consultas relacionales.
- Permite migrar después a una base central.
- Es mejor que dejar todo en hojas dispersas de Excel.

Excel puede seguir existiendo como fuente de intercambio o integración, pero no debería ser la base interna principal del sistema a largo plazo.

### 4.2 Entidades comunes para todos los componentes

Todo componente debería tener campos comunes:

| Campo | Descripción |
|---|---|
| ComponentId | Identificador interno estable. No debe depender del nombre comercial. |
| Code | Código de catálogo o código de fabricación. |
| Description | Descripción legible. |
| ComponentType | Poste, larguero, diagonal, horizontal, tornillo, placa, especial. |
| Family | Familia o serie del componente. |
| Revision | Revisión del componente dentro del catálogo. |
| Status | Activo, obsoleto, experimental, bloqueado. |
| Unit | Unidad principal: pieza, par, metro, kg, kit. |
| MaterialId | Referencia al material. |
| FinishId | Referencia al acabado: galvanizado, pintado, zincado, etc. |
| Manufacturer | Fabricante o proveedor. |
| SupplierCode | Código del proveedor si aplica. |
| Weight | Peso unitario o peso por metro. |
| DrawingBlockId | Referencia al bloque o representación gráfica. |
| GeometryProfileId | Referencia a parámetros geométricos. |
| CreatedAt | Fecha de alta. |
| UpdatedAt | Fecha de última modificación. |
| EffectiveFrom | Fecha desde la que aplica. |
| EffectiveTo | Fecha hasta la que aplica, si se descontinúa. |
| Notes | Notas técnicas. |

También conviene tener tablas generales:

| Tabla | Propósito |
|---|---|
| Materials | Material, grado, esfuerzo, densidad y propiedades mecánicas. |
| Finishes | Acabados, color estándar, espesor o tratamiento. |
| Units | Unidades permitidas y factores de conversión. |
| BlockDefinitions | Relación entre componentes y archivos/bloques AutoCAD. |
| CompatibilityRules | Compatibilidades entre componentes. |
| CapacityTables | Capacidades por configuración, claro, altura, carga o condición. |
| PriceReferences | Información futura para cotización, separada de ingeniería. |
| CatalogVersions | Versiones de catálogo usadas para trazabilidad. |

### 4.3 Postes

Los postes son componentes críticos porque determinan altura, capacidad, conexión con diagonales/horizontales, patrón de perforación y compatibilidad con placas.

Información recomendada:

| Campo | Descripción |
|---|---|
| PostCode | Código del poste. |
| Series | Serie o familia de rack. |
| ProfileType | Tipo de sección: C, omega, tubular, perfil propietario, etc. |
| NominalWidth | Ancho nominal del poste. |
| NominalDepth | Fondo o peralte nominal. |
| Thickness | Espesor/calibre. |
| MaterialGrade | Grado del acero. |
| HolePatternId | Patrón de perforación. |
| HolePitch | Paso vertical entre perforaciones. |
| HoleShape | Redondo, ranurado, lágrima, rectangular, personalizado. |
| HoleDimensions | Dimensiones de perforación. |
| FirstHoleOffset | Distancia desde base al primer barreno válido. |
| MaxLength | Longitud máxima fabricable o estándar. |
| StandardLengths | Longitudes comerciales disponibles. |
| SectionArea | Área de sección. |
| Ix / Iy | Momentos de inercia. |
| Rx / Ry | Radios de giro. |
| WeightPerMeter | Peso por metro. |
| CompatibleBasePlates | Placas compatibles. |
| CompatibleBracing | Diagonales/horizontales compatibles. |
| CompatibleBeams | Largueros compatibles si aplica. |
| CapacityReference | Tabla o documento técnico asociado. |
| BlockSideElevation | Bloque para elevación lateral. |
| BlockFrontElevation | Bloque para elevación frontal. |
| BlockPlan | Bloque para planta. |

Además debe poder almacenarse un perfil geométrico simplificado para dibujo 2D y, en fases posteriores, una representación 3D o de fabricación.

### 4.4 Largueros

Los largueros serán centrales en fases posteriores, aunque no sean imprescindibles para el MVP de cabecera.

Información recomendada:

| Campo | Descripción |
|---|---|
| BeamCode | Código del larguero. |
| BeamType | Tipo: caja, escalonado, estructural, tubular, especial. |
| ConnectorType | Tipo de conector o garra. |
| NominalLength | Largo nominal. |
| ClearOpening | Claro útil asociado. |
| BeamHeight | Altura del perfil. |
| BeamDepth | Fondo del perfil. |
| Thickness | Espesor. |
| PairOrSingle | Si se maneja por pieza individual o par. |
| LoadCapacity | Capacidad por par, por nivel o por condición. |
| DeflectionLimit | Criterio de flecha. |
| SafetyLockType | Seguro compatible. |
| CompatiblePostSeries | Series de poste compatibles. |
| StandardColors | Colores/acabados disponibles. |
| Weight | Peso unitario. |
| BlockElevation | Bloque en elevación. |
| BlockPlan | Bloque en planta. |
| HookGeometry | Datos geométricos del conector si se requiere detalle. |

Las capacidades no deberían estar como un solo campo simple. Conviene tener una tabla relacionada de capacidad por longitud, tipo de carga, condición de apoyo, factor de seguridad, norma o criterio.

### 4.5 Diagonales

Las diagonales conectan postes y dan rigidez a la cabecera. Para automatización son importantes sus reglas de longitud, ángulo y barrenos.

Información recomendada:

| Campo | Descripción |
|---|---|
| BraceCode | Código de diagonal. |
| ProfileType | Ángulo, canal, solera, tubular, perfil especial. |
| Width / Height | Dimensiones de sección. |
| Thickness | Espesor. |
| MaterialGrade | Material. |
| LengthMode | Longitud fija, calculada o seleccionada por tabla. |
| StandardLengths | Longitudes disponibles. |
| HoleDiameter | Diámetro de barrenos. |
| EndHoleOffset | Distancia del extremo al barreno. |
| CompatiblePostSeries | Postes compatibles. |
| CompatibleBoltDiameter | Tornillo compatible. |
| MinAngle / MaxAngle | Rango admisible de inclinación. |
| MinFrameDepth / MaxFrameDepth | Rango de fondos donde aplica. |
| MinFrameHeight / MaxFrameHeight | Rango de alturas donde aplica. |
| Weight | Peso unitario o por metro. |
| BlockDefinition | Bloque o símbolo asociado. |

La selección de diagonales puede depender de la altura de la cabecera, profundidad, patrón de arriostramiento y posiciones permitidas de perforación.

### 4.6 Horizontales

Los horizontales de cabecera suelen compartir lógica con diagonales, pero con colocación, ángulo y función distinta.

Información recomendada:

| Campo | Descripción |
|---|---|
| HorizontalCode | Código del horizontal. |
| ProfileType | Tipo de perfil. |
| CrossSectionDimensions | Dimensiones. |
| Thickness | Espesor. |
| LengthMode | Fijo, calculado, por tabla. |
| StandardLengths | Longitudes comerciales. |
| HolePattern | Patrón de barrenos. |
| CompatiblePostSeries | Series compatibles. |
| CompatibleBolts | Tornillería compatible. |
| PlacementRules | Reglas de colocación: base, superior, intermedio, cada cierta altura. |
| Weight | Peso. |
| BlockDefinition | Bloque asociado. |

En el MVP, los horizontales deben configurarse por el usuario con apoyo de sugerencias, puntos de conexión y offsets versionados.

### 4.7 Tornillería

La tornillería debe modelarse con cuidado porque suele afectar listas de materiales y compatibilidad.

Información recomendada:

| Campo | Descripción |
|---|---|
| FastenerCode | Código interno o comercial. |
| FastenerType | Tornillo, tuerca, rondana, ancla, seguro, kit. |
| Standard | Norma: ASTM, ISO, SAE, DIN u otra. |
| Diameter | Diámetro nominal. |
| ThreadPitch | Paso de cuerda. |
| Length | Longitud. |
| Grade | Grado mecánico. |
| Finish | Acabado. |
| HeadType | Hexagonal, Allen, especial. |
| NutType | Tipo de tuerca compatible. |
| WasherType | Tipo de rondana compatible. |
| Torque | Torque recomendado, si aplica. |
| HoleCompatibility | Rango de perforaciones compatibles. |
| UsedFor | Uso: diagonal-poste, placa-poste, anclaje a piso, larguero, etc. |
| QuantityRule | Regla de cantidad por unión. |
| KitCode | Kit asociado cuando se compra o cotiza como conjunto. |

Conviene distinguir piezas sueltas de kits. Ingeniería puede contar tornillos individualmente, mientras cotización puede agruparlos.

### 4.8 Placas

Las placas incluyen placas base, placas de unión, placas especiales y refuerzos.

Información recomendada:

| Campo | Descripción |
|---|---|
| PlateCode | Código de placa. |
| PlateType | Base, unión, empalme, refuerzo, especial. |
| Length | Largo. |
| Width | Ancho. |
| Thickness | Espesor. |
| MaterialGrade | Material. |
| HolePatternId | Patrón de barrenos. |
| HoleCount | Número de barrenos. |
| HoleDiameter | Diámetro. |
| HoleCoordinates | Coordenadas de barrenos respecto a punto base. |
| CompatiblePostSeries | Postes compatibles. |
| CompatibleAnchors | Anclas compatibles. |
| WeldInfo | Información de soldadura si aplica. |
| Weight | Peso. |
| BlockTopView | Bloque en planta. |
| BlockElevation | Bloque en elevación. |

Para placas, el patrón de barrenos debería vivir en una entidad separada, no como texto libre.

### 4.9 Componentes especiales

Los componentes especiales pueden crecer mucho: protectores, separadores, topes, anclas, seguros, mallas, parrillas, accesorios sísmicos, distanciadores, protecciones de puntal, guías y elementos de seguridad.

Información recomendada:

| Campo | Descripción |
|---|---|
| SpecialCode | Código. |
| SpecialType | Protector, separador, seguro, malla, parrilla, guía, accesorio, etc. |
| GeometryType | Bloque fijo, bloque dinámico, geometría generada, ensamble. |
| PlacementRule | Dónde y cuándo se coloca. |
| RequiredComponents | Componentes requeridos asociados. |
| OptionalComponents | Componentes opcionales. |
| CompatibilityRules | Compatibilidades por serie, dimensiones o configuración. |
| QuantityRule | Regla de cantidad. |
| DrawingRepresentation | Bloque o regla de dibujo. |
| BomBehavior | Si aparece como pieza, kit, accesorio o nota. |
| PriceBehavior | Si tiene precio directo o se calcula. |

La arquitectura debe permitir agregarlos sin cambiar la lógica central de cabeceras o largueros.

## 5. Manejo de bloques

### 5.1 Organización de biblioteca

La biblioteca de bloques debe tratarse como un activo técnico versionado, no como una carpeta informal.

Estructura recomendada:

| Carpeta | Contenido |
|---|---|
| CadLibrary/Blocks/Postes | Bloques de postes por vista y serie. |
| CadLibrary/Blocks/Largueros | Bloques de largueros. |
| CadLibrary/Blocks/Arriostres | Diagonales y horizontales. |
| CadLibrary/Blocks/Placas | Placas base, unión y refuerzos. |
| CadLibrary/Blocks/Tornilleria | Símbolos o detalles de tornillería. |
| CadLibrary/Blocks/Especiales | Accesorios especiales. |
| CadLibrary/Templates | Plantillas DWG, estilos de texto, cotas, tablas y layers. |
| CadLibrary/Standards | Archivos de estándares CAD, CTB/STB si aplica, lineweights. |

Convención de nombres sugerida:

| Elemento | Ejemplo |
|---|---|
| Bloque | RACK_POSTE_SERIE90_ELEV_LAT_REV01 |
| Archivo DWG | RACK_POSTE_SERIE90_ELEV_LAT_REV01.dwg |
| Atributo | RACK_CODE, RACK_INSTANCE_ID, RACK_COMPONENT_ID |
| Layer | RACK-COMP-POSTES |

El nombre del bloque no debe contener datos variables como altura específica, cantidad o número de proyecto. Esos datos pertenecen a atributos o metadatos.

### 5.2 Registro de bloques

Cada bloque debería estar registrado en una tabla o manifiesto:

| Campo | Descripción |
|---|---|
| BlockId | Identificador interno. |
| BlockName | Nombre dentro de AutoCAD. |
| FilePath | Ruta relativa dentro de la biblioteca. |
| ComponentType | Tipo de componente asociado. |
| ViewType | Planta, elevación frontal, elevación lateral, detalle, 3D. |
| Units | Unidades del bloque. |
| BasePoint | Punto base esperado. |
| ScaleBehavior | Escala fija, escala por unidad o escala parametrizada. |
| DynamicProperties | Propiedades dinámicas disponibles. |
| Attributes | Atributos esperados. |
| DefaultLayer | Capa por defecto. |
| Version | Versión del bloque. |
| Status | Activo, obsoleto, pruebas. |

Esto evita que el código dependa de rutas o nombres escritos a mano.

### 5.3 Cuándo usar bloques dinámicos

Usar bloques dinámicos cuando:

- La geometría pertenece a una familia estable.
- Existen pocos parámetros controlables: largo, espejo, visibilidad, stretch, rotación.
- El componente debe seguir siendo editable con herramientas nativas de AutoCAD.
- La variación geométrica es visual, no una regla compleja de ingeniería.
- El usuario final se beneficia de grips o parámetros dentro del bloque.

Ejemplos:

- Largueros con largo variable dentro de rangos conocidos.
- Placas con variantes de visibilidad.
- Símbolos de accesorios con orientación o espejo.
- Representaciones simplificadas de perfiles por vista.

### 5.4 Cuándo generar geometría por código

Generar geometría por código cuando:

- La geometría depende de reglas o cálculos.
- Hay muchas combinaciones posibles y sería impráctico mantener bloques para todas.
- Se necesitan cotas, ejes, líneas auxiliares o tablas calculadas.
- La geometría es de layout, no de componente.
- Se requiere regeneración controlada desde datos del modelo.

Ejemplos:

- Posicionamiento completo de postes, diagonales y horizontales.
- Líneas de eje, cotas generales y etiquetas.
- Tablas de materiales generadas.
- Vista esquemática de una cabecera con alturas y offsets específicos.

### 5.5 Enfoque híbrido recomendado

La solución más robusta es híbrida:

- Bloques para componentes estándar.
- Geometría por código para composición, layout, ejes, cotas, tablas y anotaciones.
- Metadatos persistentes para vincular todo al modelo de ingeniería.

El motor no debería "dibujar por dibujar". Debe generar una representación a partir de un modelo de cabecera.

### 5.6 Atributos y metadatos

Cada componente dibujado debe poder identificarse después. Para eso se recomiendan tres niveles de información:

| Nivel | Uso |
|---|---|
| Atributos de bloque | Datos visibles o consultables: código, descripción, marca, cantidad, largo. |
| XData o Extension Dictionaries | Identificadores internos, parámetros, versión de catálogo, instancia de diseño. |
| Archivo externo de proyecto | Modelo completo serializado, historial, configuración y referencias a objetos AutoCAD. |

Atributos mínimos sugeridos:

| Atributo | Propósito |
|---|---|
| RACK_COMPONENT_ID | Id del componente en catálogo. |
| RACK_CODE | Código visible del componente. |
| RACK_INSTANCE_ID | Id único de la instancia dibujada. |
| RACK_PROJECT_ID | Id del proyecto o modelo. |
| RACK_REVISION | Revisión de catálogo o diseño. |
| RACK_QTY | Cantidad si el bloque representa más de una pieza. |
| RACK_LENGTH | Longitud calculada si aplica. |

Metadatos internos sugeridos:

| Dato | Propósito |
|---|---|
| ModelEntityType | Cabecera, poste, diagonal, horizontal, placa, tornillo. |
| CatalogVersion | Versión de catálogo usada. |
| Parameters | Parámetros de generación. |
| ParentAssemblyId | Ensamble al que pertenece. |
| GeneratedBy | Versión del plugin que creó el objeto. |
| LastUpdatedAt | Fecha de última regeneración. |

No conviene depender únicamente de handles de AutoCAD como identificadores de negocio. Son útiles para localizar objetos en un dibujo, pero el identificador de ingeniería debe vivir en el modelo.

## 6. Estructura de capas

### 6.1 Criterio general

La estructura de capas debe permitir:

- Separar componentes por tipo.
- Controlar impresión y visibilidad.
- Distinguir geometría, cotas, textos, ejes y validaciones.
- Soportar fases futuras: planta, elevación, detalles, revisión, existente/nuevo.
- Mantener compatibilidad con estándares CAD internos.

Prefijo recomendado: `RACK-`.

### 6.2 Capas base sugeridas

| Capa | Uso |
|---|---|
| RACK-GRID | Ejes, líneas de referencia y grillas. |
| RACK-COMP-POSTES | Postes y puntales. |
| RACK-COMP-LARGUEROS | Largueros. |
| RACK-COMP-DIAGONALES | Diagonales de cabecera. |
| RACK-COMP-HORIZONTALES | Horizontales de cabecera. |
| RACK-COMP-PLACAS | Placas base, placas de unión y refuerzos. |
| RACK-COMP-TORNILLERIA | Tornillos, tuercas, rondanas, anclas y seguros. |
| RACK-COMP-ESPECIALES | Componentes especiales y accesorios. |
| RACK-COMP-PROTECCIONES | Protectores y elementos de seguridad. |
| RACK-ANNO-COTAS | Cotas. |
| RACK-ANNO-TEXTOS | Textos, etiquetas y notas. |
| RACK-ANNO-TABLAS | Tablas de materiales y datos. |
| RACK-ANNO-SIMBOLOS | Símbolos generales. |
| RACK-HIDDEN | Líneas ocultas. |
| RACK-CENTER | Líneas de centro. |
| RACK-CONSTRUCTION | Geometría auxiliar no imprimible. |
| RACK-VALIDATION-ERROR | Marcadores de error. |
| RACK-VALIDATION-WARNING | Marcadores de advertencia. |
| RACK-REVISION | Nubes, marcas y notas de revisión. |
| RACK-EXISTENTE | Elementos existentes. |
| RACK-NUEVO | Elementos nuevos. |

### 6.3 Propiedades de capas

Las propiedades exactas deben alinearse con el estándar de impresión de la empresa, pero se recomienda definir:

| Propiedad | Recomendación |
|---|---|
| Color | Por tipo de elemento o por estándar CTB/STB. |
| Lineweight | Definido por capa para impresión consistente. |
| Linetype | Continuo para componentes, center para ejes, hidden para ocultos. |
| Plot | Desactivado para construcción y validaciones temporales. |
| Freeze/Lock | Controlado por comandos de visualización, no manualmente en el flujo normal. |

El plugin debe crear capas faltantes automáticamente a partir de configuración, no depender únicamente de que el usuario abra una plantilla correcta.

## 7. Motor de cálculo y reglas

### 7.1 Separación de responsabilidades

La pregunta clave es qué información debe vivir en tablas, catálogos, configuración o código C#.

### 7.2 Información que debe vivir en tablas

Usar tablas para información técnica, comercial o dimensional que cambia con el tiempo:

- Dimensiones de componentes.
- Longitudes estándar.
- Capacidades por longitud, altura, serie o configuración.
- Compatibilidades entre postes, largueros, diagonales, placas y tornillería.
- Patrones de barrenos.
- Pesos unitarios.
- Acabados disponibles.
- Versiones de catálogo.
- Relación componente-bloque.
- Reglas simples de cantidad.
- Códigos comerciales o de proveedor.

Ejemplo: si cambia el espesor de una placa o se agrega un nuevo poste, no debería recompilarse el plugin.

### 7.3 Información que debe vivir en catálogos

Los catálogos son el conjunto controlado de componentes aprobados. Deben incluir:

- Componentes disponibles.
- Series de producto.
- Estado de aprobación.
- Revisión.
- Propiedades físicas.
- Representaciones gráficas.
- Restricciones de uso.
- Compatibilidades.

El catálogo no es solo una lista de materiales. Es una fuente de verdad técnica.

### 7.4 Información que debe vivir en archivos de configuración

La configuración debe controlar comportamiento del sistema, no datos técnicos del producto:

- Rutas de biblioteca de bloques.
- Ruta de catálogos.
- Unidades por defecto.
- Capas por tipo de elemento.
- Estilos de texto.
- Estilos de cota.
- Plantilla DWG base.
- Preferencias de UI.
- Tolerancias generales de dibujo.
- Formatos de exportación.
- Feature flags para activar módulos futuros.
- Parámetros de logging.

Debe existir una configuración global departamental y, si hace falta, una configuración local por usuario.

### 7.5 Información que debe vivir en código C#

El código debe contener reglas estables y comportamiento:

- Casos de uso: crear, validar, editar, regenerar.
- Algoritmos de colocación.
- Cálculo de posiciones geométricas.
- Conversión de unidades.
- Validación de consistencia.
- Selección de componentes a partir de reglas.
- Creación del plan de dibujo.
- Transacciones AutoCAD.
- Escritura de atributos y metadatos.
- Manejo de errores.
- Interfaces de repositorios y servicios.

No deberían vivir en código:

- Listas de códigos de postes.
- Medidas comerciales de componentes.
- Capacidades tabulares.
- Rutas absolutas de bloques.
- Nombres de capas rígidos.
- Precios.
- Datos del cotizador.

### 7.6 Validaciones

El motor debería distinguir:

| Tipo | Ejemplo |
|---|---|
| Error bloqueante | La altura es menor que la mínima permitida o falta un componente obligatorio. |
| Advertencia | La configuración es posible, pero usa un componente obsoleto. |
| Recomendación | Existe una alternativa más común o más eficiente. |
| Nota informativa | Longitud calculada o criterio utilizado. |

Para el MVP, las validaciones pueden ser dimensionales y de catálogo. En fases posteriores se agregarán validaciones estructurales más profundas.

## 8. MVP inicial

### 8.1 Definición exacta del MVP

Actualización de enfoque: el primer MVP debe ser un configurador visual paramétrico y manual asistido de cabeceras basado en estándar más excepciones, no una función cerrada ni una calculadora automática completa.

La especificación detallada actualizada vive en [mvp-configurador-cabeceras.md](mvp-configurador-cabeceras.md). Si hay conflicto entre esta sección y ese documento, debe prevalecer la especificación del configurador.

El comando `RACKCABECERA` debe abrir una interfaz que cargue una cabecera estándar predeterminada: claros de 44 in, placa base atornillable, celosía estándar, postes según selección, offsets conocidos y configuración base similar a la macro VBA actual. El usuario debe modificar excepciones: claros especiales, tramos sin celosía, doble celosía, refuerzos, placas, perfiles, puntos de conexión, offsets y horizontales. Después de revisar la configuración y el BOM preliminar, el sistema debe insertar la cabecera en AutoCAD y guardar metadatos para edición o regeneración posterior.

El primer MVP debe generar una cabecera de rack en AutoCAD desde un modelo configurable validado por el usuario, usando catálogos mínimos, bloques controlados y metadatos persistentes.

Para efectos del MVP, se asume "cabecera" como marco lateral de rack compuesto por:

- Dos postes.
- Horizontales principales.
- Diagonales de arriostramiento.
- Placas base.
- Tornillería asociada como dato de materiales, aunque no necesariamente dibujada con detalle.
- Cotas y etiquetas básicas.

El MVP debe producir un dibujo 2D en elevación lateral o vista de cabecera, pero el entregable principal debe ser el modelo editable de configuración. No debe intentar resolver todavía el rack completo, múltiples bahías, carga estructural avanzada, integración con Excel o cotización completa.

### 8.2 Funcionalidades del MVP

| Funcionalidad | Descripción |
|---|---|
| Comando de creación | Comando AutoCAD para abrir la pantalla de nueva cabecera. |
| Captura de datos | Datos generales de cabecera: altura, fondo, tipo de rack, tipo de cabecera, unidades y plantilla opcional. |
| Configuración de postes | Selección independiente de poste izquierdo y derecho, con refuerzo opcional por poste. |
| Tramos variables | Lista editable de claros/tramos de celosía con altura, patrón, lado, horizontales y perfiles. |
| Selección desde catálogo | Selección de componentes activos desde un catálogo mínimo; el sistema puede sugerir, pero el usuario decide. |
| Validación básica | Revisión de dimensiones, compatibilidad y datos obligatorios. |
| Inserción en AutoCAD | Selección de punto de inserción y generación del dibujo. |
| Capas automáticas | Creación/uso de capas estándar del sistema. |
| Bloques parametrizados | Inserción de postes, placas y componentes base. |
| Geometría calculada | Colocación de diagonales, horizontales, ejes y cotas. |
| Atributos | Código, descripción e identificador de instancia en cada componente principal. |
| Metadatos | Identificador de cabecera, versión de catálogo y parámetros usados. |
| Tabla simple | BOM preliminar de componentes de la cabecera configurada. |
| Regeneración mínima | Idealmente seleccionar una cabecera y reconstruirla desde sus parámetros. Si no cabe en el primer corte, debe quedar preparado el modelo. |

### 8.3 Pantallas del MVP

Pantallas recomendadas:

| Pantalla | Contenido |
|---|---|
| Datos generales | Tipo de cabecera, tipo de rack, altura, fondo, unidad y plantilla inicial. |
| Postes | Poste izquierdo, poste derecho, refuerzos, orientación y placas. |
| Tramos | Lista editable de claros/tramos de celosía. |
| Editor de tramo | Altura, patrón de celosía, lado, perfil, horizontales y conexiones. |
| Validación | Lista de errores, advertencias y datos calculados antes de dibujar. |
| Resumen | BOM preliminar de componentes y cantidades. |

La UI puede ser modal al inicio. Una paleta WPF será más cómoda en fases posteriores, especialmente para edición y navegación de proyecto.

### 8.4 Catálogos mínimos del MVP

Catálogos mínimos necesarios:

| Catálogo | Contenido mínimo |
|---|---|
| Postes | 2 o 3 modelos reales con ancho, fondo, espesor, paso de perforación y bloque. |
| Diagonales | Perfiles disponibles, patrones permitidos, reglas de longitud y tornillería compatible. |
| Horizontales | Perfiles disponibles, reglas de longitud, lado y colocación. |
| Refuerzos | Refuerzos compatibles por poste, lado, altura y tipo de cabecera. |
| Placas base | Placas compatibles por serie de poste, con selección independiente por lado. |
| Tornillería | Tornillos/tuercas/rondanas para uniones de cabecera. |
| Bloques | Referencia a bloques DWG o nombres internos. |
| Capas | Definición de capas y propiedades. |
| Configuración | Unidades, rutas, estilos de cota/texto y plantilla. |

No se necesitan en el MVP:

- Catálogo completo de largueros.
- Capacidades estructurales avanzadas.
- Precios.
- Integración con cotizador.
- Importación desde Excel.
- Base de datos central.

### 8.5 Entregables del MVP

| Entregable | Descripción |
|---|---|
| Solución Visual Studio | Proyecto AutoCAD .NET organizado por capas. |
| Plugin cargable en AutoCAD | DLL o paquete de desarrollo para cargar y probar. |
| Comando RACKCABECERA | Flujo completo para cargar una cabecera estándar, editar excepciones e insertarla. |
| Catálogo mínimo | Archivo SQLite/JSON/CSV con componentes mínimos. |
| Biblioteca mínima de bloques | Bloques para postes, placas y representaciones base. |
| Plantilla DWG | Capas, estilos y configuración de dibujo. |
| Documento de arquitectura | Este documento como guía viva del proyecto. |
| Documento de usuario MVP | Instrucciones breves para instalar, cargar y ejecutar. |
| Casos de prueba | Escenarios de cabeceras válidas e inválidas. |

### 8.6 Criterios de aceptación del MVP

El MVP debería considerarse exitoso si:

- Un usuario puede configurar una cabecera por tramos antes de dibujarla.
- El dibujo queda en capas correctas.
- Los componentes principales tienen código y metadatos.
- La cabecera se genera en una posición seleccionada desde el modelo validado.
- El sistema detecta al menos errores básicos de dimensiones y compatibilidad.
- La tabla de resumen coincide con lo dibujado.
- El catálogo puede modificarse sin recompilar el plugin.
- El código permite agregar un nuevo componente de catálogo sin reescribir el flujo principal.

## 9. Roadmap propuesto

### Fase 0: Fundamentos técnicos

Objetivo: preparar la base del producto antes de automatizar mucho dibujo.

Entregables:

- Estructura de solución.
- Convenciones de nombres.
- Sistema de configuración.
- Lectura de catálogo mínimo.
- Servicio de capas.
- Servicio de bloques.
- Logging.
- Manejo de errores.
- Pruebas unitarias fuera de AutoCAD.
- Plantilla DWG base.

### Fase 1: MVP configurador de cabecera

Objetivo: configurar visualmente una cabecera 2D por postes, refuerzos, placas, tramos variables de celosía, horizontales y perfiles, y después generar el dibujo desde ese modelo.

Entregables:

- Comando de creación.
- UI inicial de configurador.
- Catálogo mínimo.
- Configuración independiente de postes, refuerzos, tramos, diagonales, horizontales y placas.
- Cotas y etiquetas básicas.
- BOM preliminar de componentes.
- Metadatos completos del modelo de cabecera.

### Fase 2: Edición y regeneración

Objetivo: permitir modificar una cabecera ya dibujada.

Entregables:

- Selección de cabecera existente.
- Lectura de metadatos.
- Pantalla con parámetros actuales.
- Regeneración controlada.
- Manejo de objetos eliminados o modificados manualmente.
- Detección de versión antigua de catálogo.

### Fase 3: Administración formal de catálogos y bloques

Objetivo: convertir los activos técnicos en una fuente confiable.

Entregables:

- Catálogo SQLite versionado.
- Migraciones de esquema.
- Validación de catálogo.
- Manifiesto de bloques.
- Control de obsolescencia.
- Herramienta interna para revisar componentes.
- Reporte de componentes sin bloque o bloque sin componente.

### Fase 4: Módulos de rack completos

Objetivo: pasar de cabeceras aisladas a módulos de rack.

Entregables:

- Modelo de bahía.
- Largueros por nivel.
- Separación entre cabeceras.
- Vista frontal.
- Vista lateral.
- Vista en planta simplificada.
- Niveles de carga.
- Etiquetas por nivel.
- BOM por módulo.

### Fase 5: Motor de cálculo y validación avanzada

Objetivo: agregar reglas de ingeniería más profundas.

Entregables:

- Validaciones de capacidad por larguero.
- Revisión de claros y niveles.
- Compatibilidad poste-larguero.
- Validación de arriostramiento.
- Reglas de placas y anclajes.
- Advertencias por componentes obsoletos.
- Reporte de validación.
- Pruebas unitarias para reglas críticas.

### Fase 6: Listas de materiales

Objetivo: generar información confiable para fabricación, compras o cotización.

Entregables:

- BOM por cabecera.
- BOM por módulo.
- BOM por proyecto.
- Agrupación por código.
- Separación pieza/kit.
- Pesos estimados.
- Exportación a Excel.
- Exportación CSV.
- Formato compatible con cotizador.

### Fase 7: Integración con Excel y cotizador

Objetivo: conectar el nuevo sistema con herramientas existentes sin dejar que Excel controle la arquitectura.

Entregables:

- Importación desde plantillas Excel controladas.
- Exportación al archivo cotizador.
- Mapeo entre códigos de ingeniería y códigos comerciales.
- Validación de datos importados.
- Reporte de diferencias.
- Control de versión de plantilla.

### Fase 8: Gestión de proyectos

Objetivo: trabajar con proyectos completos, revisiones y múltiples usuarios.

Entregables:

- Archivo de proyecto externo o base de datos.
- Revisión de diseños.
- Historial de cambios.
- Parámetros generales de obra.
- Varias zonas o sistemas de rack por proyecto.
- Actualización de dibujos desde modelo.
- Comparación entre revisión anterior y actual.

### Fase 9: Despliegue departamental

Objetivo: convertir la herramienta en plataforma interna.

Entregables:

- Instalador o paquete AutoCAD Application Bundle.
- Configuración centralizada.
- Control de permisos para catálogos.
- Ambientes: pruebas, producción.
- Documentación de usuario.
- Documentación técnica.
- Capacitación.
- Procedimiento de soporte.
- Registro de errores y telemetría interna si la política de la empresa lo permite.

### Fase 10: Automatización avanzada

Objetivo: acercarse a diseño semi-automático o automático de sistemas completos.

Entregables:

- Generación de layouts completos.
- Optimización de selección de componentes.
- Reglas por cliente o norma.
- Integración con bases de datos empresariales.
- Reportes técnicos.
- Modelos 3D o coordinación BIM si se requiere.
- Conexión con ERP/MRP si el flujo de negocio lo justifica.

## 10. Recomendaciones técnicas específicas para AutoCAD .NET

### 10.1 Manejo de documentos y transacciones

El plugin debe respetar el modelo de AutoCAD:

- Abrir documentos y bases de datos mediante los servicios adecuados.
- Usar bloqueo de documento cuando se modifique el dibujo activo.
- Agrupar cambios en transacciones controladas.
- Evitar dejar transacciones abiertas.
- Capturar errores y revertir operaciones incompletas.
- No mezclar UI larga con transacciones activas.

La regla práctica: calcular primero, dibujar después.

### 10.2 Pruebas

No todo se puede probar fácilmente dentro de AutoCAD. Por eso:

- Domain, Application, Calculation, Catalogs y Drawing abstracto deben tener pruebas unitarias.
- AutoCad Adapter debe tener pruebas manuales o pruebas de integración controladas.
- Los casos críticos deben poder ejecutarse con datos de ejemplo.
- Las validaciones deben tener casos positivos y negativos.

### 10.3 Versionado

Versionar por separado:

- Plugin.
- Catálogo.
- Biblioteca de bloques.
- Plantilla DWG.
- Archivo de proyecto.

Un dibujo generado debe saber con qué versión fue creado. Esto será muy importante cuando existan revisiones, cambios de catálogo y actualizaciones departamentales.

### 10.4 Manejo de unidades

Definir una unidad interna del sistema, por ejemplo milímetros. Todo dato externo debe convertirse al entrar y todo dato mostrado debe formatearse al salir.

No se recomienda mezclar pulgadas, milímetros y unidades de AutoCAD sin una capa explícita de conversión.

### 10.5 Errores y diagnóstico

El sistema debe registrar:

- Comando ejecutado.
- Parámetros usados.
- Versión de plugin.
- Versión de catálogo.
- Archivo DWG activo.
- Error técnico.
- Mensaje amigable para usuario.

Los mensajes al usuario deben ser claros y accionables. El log técnico puede tener más detalle.

## 11. Decisiones que conviene posponer

Para evitar sobrearquitectura en el MVP, conviene posponer:

- Motor estructural completo.
- Integración profunda con Excel.
- Interfaz de administración de catálogos.
- Base de datos central multiusuario.
- Generación de racks completos.
- Cotización automática.
- Optimización de componentes.
- 3D avanzado.
- Sincronización bidireccional compleja entre dibujo y modelo.

La arquitectura debe dejar espacio para estas capacidades, pero el primer producto debe enfocarse en configurar y dibujar una cabecera bien hecha.

## 12. Riesgos principales

| Riesgo | Mitigación |
|---|---|
| Mezclar lógica de negocio con AutoCAD API | Separar Domain/Application de AutoCad Adapter. |
| Catálogos informales o inconsistentes | Usar esquema validado y versiones de catálogo. |
| Bloques sin control de versiones | Crear manifiesto de bloques y convención de nombres. |
| Dibujos imposibles de actualizar | Guardar metadatos de instancia y parámetros. |
| MVP demasiado grande | Limitar a configurador de cabecera 2D con catálogos mínimos y pocos patrones iniciales. |
| Dependencia excesiva de Excel | Usar Excel como entrada/salida, no como núcleo interno. |
| Falta de pruebas | Probar reglas y cálculo fuera de AutoCAD. |
| Cambios manuales del usuario en geometría generada | Detectar inconsistencias y permitir regeneración controlada. |

## 13. Conclusión

El primer objetivo no debe ser construir una suite completa de diseño de racks, sino crear una base sólida: un plugin de AutoCAD capaz de configurar visualmente una cabecera desde datos estructurados, con componentes trazables, capas ordenadas, bloques controlados y metadatos persistentes.

Si el MVP se construye con separación clara entre AutoCAD, dominio, catálogos y dibujo abstracto, el sistema podrá crecer gradualmente hacia diseño completo, validaciones, listas de materiales e integración con cotización sin tener que reescribirse desde cero.

La arquitectura recomendada prioriza:

- Mantenibilidad.
- Escalabilidad.
- Trazabilidad.
- Uso controlado de bloques.
- Catálogos externos.
- Pruebas fuera de AutoCAD.
- Preparación para uso departamental.

El siguiente paso natural sería definir el estándar base del configurador `RACKCABECERA`, el modelo mínimo de excepciones y el formato inicial del catálogo de componentes, puntos de conexión y plantillas.
