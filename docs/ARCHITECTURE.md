# Arquitectura de RackCad

Este documento describe la arquitectura **vigente** y el horizonte **objetivo** de RackCad. Cuando
una sección objetivo todavía no está implementada se marca expresamente; no debe interpretarse como
estado actual.

Fuentes de apoyo:

- las convenciones obligatorias viven en [AGENTS.md](../AGENTS.md);
- el estado vivo y la última evidencia están en [HANDOFF.md](HANDOFF.md);
- las iniciativas y dependencias están en [ROADMAP.md](ROADMAP.md);
- las decisiones aceptadas están en [adr/](adr/README.md);
- los términos del dominio están en [guias/glosario.md](guias/glosario.md).

## 1. Principios vigentes

1. **Dependencias hacia el dominio.** Domain no depende de ninguna otra capa; Application depende de
   Domain; UI depende de Application; Plugin adapta UI/Application a AutoCAD.
2. **AutoCAD solo en Plugin.** Geometría, resolución, BOM, persistencia y layout deben poder probarse
   sin AutoCAD.
3. **Una regla en un sitio.** Si dibujo, BOM y UI comparten una cantidad o coordenada, consumen el
   mismo helper puro de Application.
4. **Diseño separado de geometría resuelta.** La intención editable se persiste; resolvers y builders
   reconstruyen el resultado físico.
5. **Persistencia versionada y compatible.** Los DTO aceptan campos nullable para documentos legacy,
   definen fallbacks y tienen pruebas de round-trip.
6. **Catálogos Excel-first.** Los datos de producto siguen en CSV/JSON versionados; los nombres de
   bloque provienen de catálogo y deben coincidir con el DWG real.
7. **El dibujo se deriva.** Las vistas de un rack comparten identidad y se redibujan desde el diseño,
   no se editan como geometría independiente.
8. **Producto sin paquetes NuGet.** Las dependencias externas del código de producto requieren una
   decisión explícita.

## 2. Solución y dirección de dependencias

```text
RackCad.Domain (net8.0, puro)
        ^
RackCad.Application (net8.0, puro y testeable)
        ^
RackCad.UI (net8.0-windows, WPF; sin AutoCAD)
        ^
RackCad.Plugin (net8.0-windows; único adaptador AutoCAD)
```

| Proyecto | Responsabilidad vigente | No debe contener |
|---|---|---|
| `RackCad.Domain` | Diseños, entidades, enums, selecciones y defaults de dominio | WPF, AutoCAD, IO de plataforma |
| `RackCad.Application` | Resolución, geometría, planes de dibujo, BOM, catálogos, persistencia y layout | API de AutoCAD, controles WPF |
| `RackCad.UI` | Ventanas, ViewModels, captura y preview | Referencias AutoCAD, reglas geométricas duplicadas |
| `RackCad.Plugin` | Comandos, transacciones, jigs, Xrecords y materialización de bloques | Reglas de negocio que puedan vivir en Application |

La ruta normal de una operación rica es:

```text
diseño editable
  -> resolver puro
  -> sistema/plan resuelto
  -> builder de vista o BOM
  -> adapter de Plugin
  -> bloque y metadatos en AutoCAD
```

## 3. Sistemas vigentes

### 3.1 Cabecera o marco

Un marco tiene dos postes, placas base, horizontales y celosía. Las **horizontales son la fuente de
verdad**; los paneles son intervalos derivados entre horizontales consecutivas.

```text
FrameHorizontal ordenadas
  -> BracingPanel consecutivos
  -> FrameMember físicos
  -> preview, dibujo y BOM
```

Invariantes:

- después de ordenar por elevación, las horizontales se renumeran `H1`, `H2`, ...;
- `P1` solo puede unir `H1-H2`, `P2` solo `H2-H3`, y así sucesivamente;
- agregar, eliminar, mover, dividir o combinar reconstruye todos los paneles;
- `DoubleDiagonal` y `XBracing` son arreglos distintos;
- `AutoAlternating` alterna la dirección de las diagonales por panel;
- `BasePlatePlacement.PeralteOverride = null` conserva el peralte derivado del poste.

`BracingPanelMemberBuilder` materializa los miembros que consumen vista previa, dibujo y BOM. La
restauración estándar parte de un snapshot limpio y elimina excepciones, no parchea el modelo viejo.

### 3.2 Selectivo

El selectivo es pallet-driven y sigue esta tubería:

```text
SelectivePalletDesign
  -> SelectiveGeometryResolver
  -> SelectiveRackSystem
  -> SelectiveFrontalBuilder / SelectiveLateralBuilder / SelectivePlantaBuilder
  -> DrawServices del Plugin
```

El diseño contiene la matriz frentes × niveles, tarimas, largueros, holguras, overrides opcionales,
cabeceras por poste y selección de seguridad. El resolver deriva claros, longitudes, elevaciones y
alturas; los builders no vuelven a inventar esas reglas.

Reglas geométricas centrales:

- largo de larguero = frente × cantidad + tolerancia × (cantidad + 1), salvo override;
- separaciones verticales se redondean hacia arriba a la rejilla de troquel;
- el nivel cero es piso y solo lleva larguero cuando `FloorBeam` lo solicita;
- la altura comercial redondea al pie desde la superficie de carga y la tarima superior;
- el frente más alto gobierna un poste compartido.

La doble profundidad admite varios **fondos**, cada uno con niveles, conteo de frentes y fondo de
tarima propios. `SelectiveDepthLayout` define una rejilla maestra con el fondo más largo; los demás
son prefijos alineados. El fondo de cabecera se deriva del fondo de tarima menos la tolerancia
canónica, salvo override. Separadores, lateral, planta y preview consumen el mismo layout.

El “medio frente” se representa como N tramos y N-1 postes intermedios. El último tramo es el resto
calculado y `SelectiveMedioFrente.Resolve` es la fuente única para frontal, planta y BOM. Si los
tramos no caben, se conserva el frente completo.

#### Seguridad selectiva

`SelectiveSafetySelection` es la selección persistida. Todo campo nuevo debe copiarse en `DeepCopy`
y mapearse explícitamente en `SelectivePalletDesignDocument`, con fallback legacy y prueba de
round-trip. La colocación y el conteo viven en helpers de Application:

- botas y laterales comparten reglas de sustitución física;
- topes, desviadores, parrillas y tarimas usan planes/resolvers compartidos;
- la parrilla se cuenta desde `SelectiveFrontalBuilder.ParrillaRow`, consumido por UI, vistas y BOM;
- tarimas pueden ser referencia visual sin entrar al BOM;
- la misma pieza física no se cuenta otra vez por aparecer proyectada en otra vista.

#### Cotas y anotaciones selectivas

`SelectiveDimensions` y `SelectiveAnnotations` producen planes puros. El detalle de cota forma parte
del diseño y los adapters de AutoCAD solo materializan el plan. Las cotas deben usar la geometría
resuelta y los puntos catalogados, nunca coordenadas duplicadas en UI o Plugin.

### 3.3 Sistema dinámico

El dinámico separa `DynamicRackDesign` de `DynamicRackSystem`. `DynamicRackSystemResolver` valida y
materializa frentes, niveles, fondos, alturas y perfiles. Sus vistas ligadas son laterales por poste,
frontal de salida, frontal de entrada y planta.

Componentes principales:

- `DynamicFrontGeometry`: retícula transversal, longitudes IN/OUT y alturas adyacentes;
- `DynamicDepthGeometry`: rangos longitudinales y fondos por frente;
- `DynamicRackLevelGeometry`: celda frente × nivel;
- `DynamicLoadBeamGeometry`: largueros de entrada/salida y mates al troquel;
- `DynamicIntermediateBeamGeometry`: apoyos interiores;
- `DynamicFlowBedLateralBuilder`: cama compuesta por posición y nivel;
- `DynamicSystem*Builder`: planes lateral, frontal y planta.

Los frentes cortos definen el patrón estructural común y los largos lo prolongan. La altura de cada
poste es el máximo de sus frentes adyacentes. Cada corte lateral persiste su `Section`; payloads
legacy sin sección caen al poste cero.

La cama dinámica reutiliza el builder de cama existente. Su longitud comercial se resuelve una vez;
los mates y offsets de catálogo no alteran ese corte. Largueros IN/OUT e intermedios se colocan a
partir de las mismas líneas de troquel y origen del riel.

#### Seguridad, cotas y anotaciones dinámicas

`DynamicSafetyLateralBuilder` y `DynamicSafetyMultiViewBuilder` proyectan una selección común sobre
las vistas. Botas, laterales, desviadores, defensas y guías de entrada se contabilizan por pieza
física, no por proyección. `DynamicViewDecorations` centraliza nombre, numeración y cotas; detalle,
escala y estilo son parte del diseño versionado.

### 3.4 Cama de rodamiento

`FlowBedConfiguration` representa la intención; `FlowBedLateralBuilder` crea el plan lateral y
`FlowBedBomBuilder` produce el BOM. La cama standalone es un componente completo. El dinámico puede
componer el mismo builder sin copiar su aritmética.

## 4. Capacidades compartidas vigentes

### 4.1 Identidad, vistas y round-trip

Cada rack lógico tiene un GUID y una o más definiciones de bloque por vista. En la definición se
embebe:

```text
RackEmbedDocument
  SchemaVersion
  Kind
  View
  Section
  Id
  Name
  Design
```

`RackEmbedStore` serializa el sobre; `RackBlockData` lo guarda en Xrecords troceados. `RACKEDITAR`
lee el sobre, despacha por `Kind`, carga el editor correcto y redefine en sitio todas las vistas con
el mismo GUID. Las copias de una definición se actualizan sin moverse. `RACKDUPLICAR` crea identidad
independiente; COPY de AutoCAD conserva la misma definición.

Las vistas adicionales solo se insertan desde un rack existente. Esto evita laterales o plantas
huérfanas sin diseño fuente.

### 4.2 Persistencia

Los stores persisten **diseños**, no geometría resuelta. Los DTO `*Document` son contratos de
compatibilidad:

- llevan `SchemaVersion` cuando corresponde;
- los campos nuevos que leen documentos antiguos son nullable;
- cada nullable tiene fallback conservador explícito;
- `SchemaGuard` rechaza versiones futuras incompatibles;
- todo cambio de contrato exige prueba de round-trip y escenario legacy.

La identidad embebida en DWG viaja con el dibujo. Los archivos `.rackcad.json` y la biblioteca de
diseños son persistencia auxiliar, no sustituyen el payload del bloque.

### 4.3 BOM

Los builders de BOM reejecutan o consumen los planes puros que gobiernan el dibujo. Un BOM puede ser
plano o por componentes con receta de piezas. El consolidado escanea racks por GUID, multiplica por
copias y evita duplicar vistas ligadas. CSV y XLSX son adaptadores de salida; XLSX se escribe como
OOXML sin dependencia de producto externa.

### 4.4 Catálogos y bloques

`JsonRackCatalogProvider` carga `assets/catalogs/*.csv` y JSON auxiliares. Los CSV prevalecen, aceptan
UTF-8 o Windows-1252 y la caché se invalida por firma de archivos. Las claves y FKs se documentan en
[modelo-de-datos.md](modelo-de-datos.md); la edición se documenta en
[catalogos-y-plantillas.md](catalogos-y-plantillas.md).

Cada pieza y vista se resuelve por `blocks.csv`; `blockName` coincide exactamente con la definición
del DWG. Los parámetros dinámicos se buscan case-insensitive. Si un stretch funciona manualmente y
falla solo por API, primero se revisa la autoría del bloque y la dirección del grip.

### 4.5 Layout de almacén

`WarehouseGridPlanner`, `WarehouseFitChecker` y `WarehouseAutoFill` viven en Application. Los
comandos `RACKLAYOUT` y `RACKRELLENAR` son adapters del Plugin. La versión vigente coloca y rellena de
forma determinista; no es el optimizador IA futuro. El layout consume huellas resueltas de planta y
no debe recrear dimensiones del rack.

## 5. UI y adaptación AutoCAD

La UI vigente tiene una ventana por sistema y varios diálogos especializados. Puede coordinar
selección y captura, pero las reglas compartidas viven en Domain/Application. Los previews deben
consumir la misma geometría resuelta que el dibujo.

El Plugin mantiene transacciones, selección, jigs, imports de bloques y Xrecords. Los `*DrawService`
reciben planes ya calculados. Para rendimiento:

- se reutilizan definiciones anidadas con patrón ARRAY;
- no se fijan parámetros dinámicos repetidamente por cada referencia si una definición compartida
  puede resolverlo;
- una edición multivista termina con un solo `Regen`;
- no se reconstruyen controles WPF completos por cada clic.

## 6. Build y validación

Los comandos canónicos y la definición de terminado viven en AGENTS. El detalle de instalación está
en [despliegue.md](despliegue.md). Restricciones que condicionan arquitectura:

- Domain, Application y sus pruebas no requieren AutoCAD ni Windows;
- UI y Plugin usan `net8.0-windows`;
- AutoCAD abierto puede bloquear los DLL del directorio Debug;
- `AutoCADInstallDir` permite apuntar el build a otra instalación;
- los avisos MSB3277 de las referencias de AutoCAD son conocidos; errores propios no se aceptan;
- la validación real del dibujo y de los bloques DWG corresponde a AutoCAD y al dueño.

El checklist manual detallado se extraerá de la guía 03 a
`docs/guias/validacion-manual-autocad.md` durante la fase de migración aprobada de I-06.

## 7. Arquitectura objetivo

Esta sección es horizonte, no implementación actual. Su ejecución está repartida en ROADMAP y debe
respetar los ADRs aceptados.

### 7.1 Kernel y módulos de sistema

Cada sistema futuro aporta un módulo con:

```text
Descriptor
Documento versionado
Resolver diseño -> sistema
Builders por vista -> SystemPlan
BOM builder
Estado de editor puro + vista WPF
Draw adapter
```

El costo de añadir el sistema N+1 debe concentrarse en su módulo. La prueba de fuego es Push Back:
si obliga a editar stores o switches de otros sistemas, los contratos compartidos están incompletos.

### 7.2 Registros

- `SystemRegistry` en Application: persistencia, validación y biblioteca de diseños.
- `KindHandlerRegistry` en Plugin: edición, BOM, layout y restamp por `Kind`.
- `EditorModuleRegistry` en UI: menú y biblioteca sin propiedades/handlers por cada sistema.

Un `Kind` desconocido debe producir un error visible, no una omisión silenciosa.

### 7.3 Editor Shell

El objetivo es una `RackEditorSession<TDesign,TSystem>` compartida para catálogo, identidad,
recompute coalescido e inserción. El estado del editor se extrae a Application. Controles comunes:

- `NumericField`;
- `CatalogCombo`;
- `SelectionMatrix`;
- `PreviewCanvas`;
- una base `RackDialogWindow`.

Los grids realmente dinámicos pueden construirse en código; los diálogos estáticos permanecen en
XAML. La clonación de `RackFrameConfiguration` debe existir en un solo servicio probado.

### 7.4 Application y persistencia objetivo

- servicios de colocación por familia de accesorio y vista;
- configuraciones de seguridad por subtipo, con DTO propio;
- persistencia uniforme: Document + SchemaVersion + SchemaGuard + round-trip;
- escritura atómica y distinción entre archivo ausente e ilegible;
- namespaces `Systems.Selective`, `Systems.Dynamic`, `Systems.FlowBed`, `Systems.Shared`;
- stores/providers nuevos detrás de interfaz cuando llegue hosting fuera de AutoCAD.

### 7.5 Solución futura

Los cuatro proyectos actuales permanecen. Solo cuando exista necesidad real se agregan:

```text
RackCad.Data
RackCad.Api
RackCad.Reports
RackCad.UI.Tests
```

`Directory.Build.props` centralizará versión y opciones comunes; el bundle tenderá a generarse con
`dotnet publish`. La política cero NuGet de producto solo cambia mediante ADR.

### 7.6 Datos objetivo

Los CSV Excel-first continúan. Se agregan validación con severidades, diagnóstico de filas
descartadas, reglas de encoding/EOL y un manifest de compatibilidad de `blocks-library.dwg`. Costos y
precios requieren ADR antes de elegir catálogo versionado, fuente separada o base de datos.

## 8. Apéndice temporal: agregar un tipo hoy

Este patrón refleja el estado actual y se conserva por decisión DOC-02. **No es la arquitectura
final.** I-18 lo sustituirá por `docs/guias/agregar-un-sistema.md` después de validar el patrón de
módulos con Push Back.

1. Definir un `Kind` estable en el sobre persistido.
2. Agregar el despacho `Edit<Kind>` en `RackFrameCommands`.
3. Implementar `LoadExisting` en la ventana editora.
4. Implementar embed y redibujo en sitio en el DrawService.

Mientras existan los switches actuales, un tipo nuevo también debe revisar persistencia, biblioteca,
BOM consolidado, ayuda, layout y pruebas de round-trip. El objetivo de los registros es eliminar esa
lista de ediciones compartidas.

## 9. Cobertura de las fuentes absorbidas

| Fuente anterior | Contenido absorbido aquí |
|---|---|
| `02-modelo-tecnico-vigente.md` visión general | §§1-3 |
| 02: cabecera y sus invariantes | §3.1 |
| 02: motor selectivo, doble profundidad y medio frente | §3.2 |
| 02: dinámico modular y cama | §§3.3-3.4 |
| 02: identidad, round-trip, BOM y catálogos | §4 |
| Auditoría arquitectónica §4 | §7 |
| Guía 03: entorno, build y bloqueo de DLL | §6 |
| Guía 04: patrón “agregar un tipo” | §8 |
| Seguridad vigente | §§3.2 y 3.3 |
| Layout de almacén | §4.5 |
| Cotas y anotaciones | §§3.2 y 3.3 |

Esta tabla permite verificar la absorción antes de archivar 02. El archivo fuente no se mueve en la
fase 3 de I-06.
