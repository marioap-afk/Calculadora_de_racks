# Guia de desarrollo y validacion

RackCad es un plugin de AutoCAD (.NET, `net8.0-windows`, WPF) para disenar y dibujar
racks. Ya no es solo un configurador de cabeceras: maneja CUATRO tipos de rack, cada
uno con su ventana editora, su dibujo en AutoCAD e identidad + round-trip de edicion.

## Requisitos

- Windows.
- AutoCAD 2025 completo.
- .NET SDK con soporte para `net8.0-windows`.
- Visual Studio recomendado.

El entorno reportado tiene:

- .NET SDK 10 instalado.
- .NET 8 SDK disponible.

## Estructura de solucion

```text
RackCad.sln
src/
  RackCad.Domain/
  RackCad.Application/
  RackCad.UI/
  RackCad.Plugin/
tests/
  RackCad.Tests/
docs/
assets/
```

## Proyectos

`RackCad.Domain`

- Entidades y enums del modelo de racks (cabecera, sistema dinamico, cama, selectivo).
- No debe depender de AutoCAD ni WPF.

`RackCad.Application`

- Servicios de aplicacion, catalogos y persistencia.
- `JsonRackCatalogProvider` carga los CSV de `assets/catalogs/` en un `RackCatalog`.
- Geometria del selectivo en `SelectiveGeometryResolver`; BOM en `SelectiveBomBuilder`.
- Sobre unificado de identidad `RackEmbedDocument` y stores de diseno.

`RackCad.UI`

- Ventanas WPF, una por tipo de rack, mas el menu principal y el BOM:
  - `RackMainMenuWindow` (menu de que disenar).
  - `RackFrameConfiguratorWindow` (cabecera / marco).
  - `RackDynamicSystemWindow` (sistema dinamico / pallet flow).
  - `RackFlowBedWindow` (cama de rodamiento).
  - `RackSelectiveWindow` (selectivo, matriz frentes x niveles).
  - `RackBomWindow` (grid + export CSV del BOM del selectivo).
- ViewModels, tablas, arbol, panel de propiedades y vista previa.

`RackCad.Plugin`

- Entrada AutoCAD .NET API. Comandos en `RackFrameCommands`.
- Servicios de dibujo: `LateralHeaderDrawService`, `PlantaHeaderDrawService`,
  `DynamicSystemDrawService`, `FlowBedDrawService`, `SelectiveFrontalDrawService`,
  `SelectivePlantaDrawService`.
- Identidad embebida en la definicion del bloque via `RackBlockData` (Xrecord troceado).
- Referencias a `AcCoreMgd`, `AcDbMgd`, `AcMgd`.

`RackCad.Tests`

- Suite de pruebas (`net8.0`, xUnit), 232 tests verdes en `release/claude-review`.

## Compilar

```powershell
dotnet build RackCad.sln -v:minimal
```

Si se necesita indicar una ruta distinta de AutoCAD:

```powershell
dotnet build RackCad.sln -p:AutoCADInstallDir="C:\Program Files\Autodesk\AutoCAD 2025"
```

Nota: AutoCAD bloquea `RackCad.Plugin.dll` mientras esta cargado. Para reconstruir el
plugin, cierra AutoCAD primero; para revisar solo la UI/logica sin AutoCAD, compila
contra un directorio de salida temporal en vez de `-t:Compile` sobre el DLL en uso.

## Probar

```powershell
dotnet test RackCad.sln -v:minimal
```

Los tests no dependen de AutoCAD (targetean `net8.0`) y cubren geometria del selectivo,
catalogos, persistencia/round-trip del sobre y builders.

## Advertencias conocidas

Durante build aparecen advertencias `MSB3277` por conflictos de versiones en:

- `Microsoft.VisualBasic`.
- `System.Drawing`.

Vienen de referencias AutoCAD 2025 contra referencias .NET. Actualmente no bloquean el build.

## Cargar en AutoCAD

1. Abrir AutoCAD 2025.
2. Ejecutar `NETLOAD`.
3. Cargar:

```text
src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

4. Ejecutar `RACKCAD` (menu principal) o directamente el comando del tipo deseado.

## Comandos disponibles

Todos estan registrados con `[CommandMethod]` en `RackFrameCommands`.

| Comando               | Que hace |
| --------------------- | -------- |
| `RACKCAD`             | Menu principal (`RackMainMenuWindow`): elige que disenar e inserta. |
| `RACKCABECERA`        | Abre el configurador de cabecera (`RackFrameConfiguratorWindow`). |
| `RACKCABECERALATERAL` | Dibuja la cabecera lateral estandar directo, sin configurador. |
| `QUICKCABECERA`       | Cabecera lateral desde la linea de comandos (pide poste, fondo, alto). |
| `RACKSISTEMADINAMICO` | Dibuja un sistema dinamico (pallet flow) preliminar. |
| `QUICKCAMA`           | Cama de rodamiento (dinamica o pushback) desde la linea de comandos. |
| `RACKSELECTIVO`       | Editor selectivo avanzado (`RackSelectiveWindow`), matriz frentes x niveles. |
| `RACKEDITAR`          | Selecciona un rack dibujado, lo reabre en su editor y lo redefine en sitio. |

## Los cuatro tipos de rack

1. CABECERA (marco): 2 postes + placas base + celosia. Horizontales = fuente de verdad;
   paneles derivados. El peralte de la placa base es editable por placa
   (`BasePlatePlacement.PeralteOverride`; null = derivado con `StandardPeralte`).
2. SISTEMA DINAMICO (pallet flow): cabeceras a lo largo del tramo + separadores por nivel.
3. CAMA DE RODAMIENTO (flow bed): riel + rodillos + frenos + tope; dinamica o pushback.
4. SELECTIVO: matriz FRENTES x niveles (el termino es "frente", no "bahia"). Cada celda =
   tarima + larguero + peralte de larguero; geometria en `SelectiveGeometryResolver`,
   BOM en `SelectiveBomBuilder`. Cada poste puede referenciar una cabecera por poste.

## Identidad y round-trip

- Cada rack dibujado es UNA definicion de bloque; las copias son referencias a ella.
- En la definicion del bloque se embebe un sobre `RackEmbedDocument { SchemaVersion,
  Kind, View, Section, Id (GUID), Name, Design (JSON) }`. Kinds: `selective`, `dynamic`,
  `cabecera`, `cama`. Views: `frontal`, `lateral`, `planta`; `Section` es el indice de
  corte lateral del selectivo (-1 = vista no seccionada).
- Un rack puede tener varias vistas ligadas por el mismo GUID: el selectivo tiene
  frontal + lateral (un bloque por poste/corte) + planta; la cabecera tiene lateral +
  planta. Las vistas lateral/planta solo se insertan desde `RACKEDITAR` de una vista
  existente (nunca quedan huerfanas).
- `RACKEDITAR` sobre cualquier vista lee el sobre, despacha por `Kind`, reabre el editor
  del sistema completo precargado (`LoadExisting`) y al confirmar redibuja TODAS las
  vistas ligadas (encontradas por GUID escaneando las definiciones de bloque) en sitio
  + Regen: todas las copias se actualizan a la vez, ninguna se mueve.
- El nombre "Rack A" de cada editor es el nombre del bloque; el GUID va en el sobre.

## Catalogos

`assets/catalogs/*.csv`, cargados por `JsonRackCatalogProvider` a `RackCatalog`:
`post-profiles`, `truss-profiles`, `beam-profiles` (columna `peraltes` = FK a mensula),
`mensulas`, `base-plates` (`peralteBase`/`peraltePorPeraltePoste` -> `StandardPeralte`),
`connection-points` + `connection-layout` (por vista: X = localX + localXPorParam*paramX;
Y = localY + localYPorParam*paramY), `blocks`, `views`,
`flow-bed-profiles`, `spacers-profiles`. Excel-first: el `.csv` gana sobre el `.json`;
acepta UTF-8 y ANSI/Windows-1252 de Excel; cache con invalidacion por firma de archivos
(editar el CSV y relanzar el comando recarga). Persistencia de proyecto: `RackProjectStore`
-> `.rackcad.json`.

## Checklist de validacion

Configurador de cabecera (`RACKCABECERA`):

- La ventana abre dentro de AutoCAD.
- La cabecera estandar carga con 4 horizontales y 3 paneles.
- Dividir panel crea nueva horizontal; combinar panel elimina la horizontal intermedia.
- Al cambiar elevaciones, los IDs vuelven a `H1`, `H2`, `H3` por orden fisico.
- Paneles siempre consecutivos; no aparecen referencias viejas como `H2-H5`.
- La vista previa se actualiza y las excepciones aparecen al modificar.
- El peralte de placa base es editable por placa (override) y respeta el derivado si se deja vacio.
- `Restaurar cabecera estandar` limpia excepciones y vuelve al estado inicial.
- `Restaurar layout predeterminado` solo cambia tamanos de UI.

Dibujo y round-trip (los cuatro tipos):

- Cada comando dibuja su bloque y permite colocarlo con el mouse.
- Reabrir con `RACKEDITAR` carga los datos correctos segun el `Kind`, desde CUALQUIER
  vista del rack (frontal, lateral o planta).
- Al confirmar la edicion, todas las copias del rack reflejan el cambio sin recolocarse.
- Round-trip multi-vista: con frontal + lateral + planta del mismo selectivo insertadas
  (o lateral + planta de la misma cabecera), editar cualquiera de ellas redibuja TODAS
  las vistas ligadas por GUID.
- Insertar lateral del selectivo pregunta QUE corte (numero de poste) y se coloca con jig.
- Los botones de insertar lateral/planta solo se habilitan desde `RACKEDITAR` de una
  vista existente (deshabilitados con tooltip si no aplica).
- `RACKSELECTIVO` abre el BOM (`RackBomWindow`) y exporta CSV.

## Archivos generados que no deben versionarse

Ya estan en `.gitignore`:

- `bin/`
- `obj/`
- `.vs/`
- `.appdata/`
- `.localappdata/`
- `.dotnet_home/`
- `.nuget_packages/`
