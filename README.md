# RackCad - Configurador de racks industriales

Aplicacion profesional de AutoCAD .NET 8 en C#/WPF para disenar racks industriales: cabeceras, sistemas dinamicos (pallet flow) y camas de rodamiento, con dibujo block-based en AutoCAD.

Todos los modulos generan un **bloque de AutoCAD** que se coloca con el mouse (jig). Los bloques de pieza que falten en el dibujo activo se **importan automaticamente** desde una biblioteca DWG (configurable desde el menu principal), de modo que los comandos funcionan incluso en un dibujo en blanco.

## Estado actual

- Solucion Visual Studio / .NET 8: `RackCad.sln`.
- Plugin AutoCAD: `src/RackCad.Plugin` (adaptador delgado; unico proyecto que toca la API de AutoCAD).
- UI WPF: `src/RackCad.UI` (no referencia AutoCAD).
- Logica de aplicacion (geometria/calculo puro, testeable): `src/RackCad.Application`.
- Modelo de dominio: `src/RackCad.Domain`.
- AutoCAD objetivo actual: AutoCAD 2025 completo, no LT.

## Modulos

| Modulo | Que hace |
|---|---|
| **Cabecera** | Configurador con vista previa, configuracion rapida + editor avanzado (horizontales, paneles, perfiles, refuerzo de poste, excepciones). |
| **Sistema dinamico (pallet flow)** | Vista lateral del sistema completo: cabeceras alternadas (celosia espejeada) como bloques anidados compartidos, separadores por nivel, postes derivados con refuerzo opcional, altura de cabecera automatica desde la carga, presets de configuracion por modulo, BOM. |
| **Cama de rodamiento** | Riel (LONGITUD parametrica), tope, rodillos al paso minimo por diametro y frenos segun fondo de tarima; tipo dinamica o pushback (sin frenos). Ventana con vista previa o comando rapido. |

## Comandos

```text
RACKCAD                 (menu principal: cabecera, sistema dinamico, cama, biblioteca de bloques)
RACKCABECERA            (configurador de cabeceras; "Insertar en AutoCAD" dibuja lo configurado)
RACKCABECERALATERAL     (dibuja la cabecera estandar en vista lateral, sin dialogo)
RACKSISTEMADINAMICO     (dibuja el sistema dinamico por defecto, sin dialogo)
QUICKCABECERA           (cabecera sin interfaz: poste, fondo y alto por linea de comandos)
QUICKCAMA               (cama sin interfaz: tipo, rodillo, fondo del carril y de tarima)
```

## Biblioteca de bloques

Las definiciones de bloque viven en un solo DWG (`blocks-library.dwg`). Antes de dibujar, el plugin importa las que falten en el dibujo activo (se lee de disco, sin abrir el archivo). La ruta se configura desde el menu `RACKCAD` (seccion "Biblioteca de bloques": Examinar/Restablecer) y se persiste en `%APPDATA%\RackCad\settings.json`; por defecto se busca junto a los catalogos. Los nombres de bloque deben coincidir con la columna `blockName` de `blocks.csv`. Si el archivo o un bloque no existe, la pieza se reporta como faltante y se omite (no aborta).

## Compilar

```powershell
dotnet build RackCad.sln -v:minimal
```

Advertencias conocidas: `MSB3277` por conflictos de versiones entre referencias AutoCAD 2025 y ensamblados .NET. No bloquean la compilacion.

Nota: con AutoCAD abierto y el plugin cargado (NETLOAD), los DLL del bin del plugin quedan bloqueados y el build falla solo en el paso de copia (MSB3021/MSB3027). Cerrar AutoCAD para reconstruir; para validar solo el codigo, compilar la UI a una carpeta temporal (`dotnet build src/RackCad.UI/RackCad.UI.csproj -o <temp>`) y correr las pruebas.

## Pruebas

Pruebas unitarias en `tests/RackCad.Tests` (xUnit). Cubren `RackCad.Domain` y `RackCad.Application`, que son `net8.0` puro y por tanto corren en cualquier OS (sin AutoCAD ni Windows):

```bash
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
```

## Catalogos externos

Los perfiles, placas, puntos, vistas, bloques y layout de conexion viven como CSV versionado en `assets/catalogs/` (las plantillas y los defaults siguen en JSON). Horizontales y diagonales comparten una sola lista de celosia (`truss-profiles.csv`) y los refuerzos son postes (`post-profiles.csv`).

- `post-profiles.csv`
- `truss-profiles.csv` (horizontales y diagonales de celosia)
- `base-plates.csv`
- `spacers-profiles.csv` (separadores de cabecera del sistema dinamico)
- `flow-bed-profiles.csv` (cama de rodamiento: riel/rodillo/freno/tope, columna `role`)
- `connection-points.csv`
- `views.csv`
- `connection-layout.csv` (posicion 2D de cada punto por pieza y vista)
- `blocks.csv` (nombre de bloque de AutoCAD por pieza y vista)
- `defaults.json`
- `header-templates.json`
- `blocks-library.dwg` (definiciones de bloque; ver seccion Biblioteca de bloques)

Como editar estos archivos y como se relacionan entre si: `docs/catalogos-y-plantillas.md` y `docs/modelo-de-datos.md`.

Se cargan con `RackCad.Application.Catalogs.JsonRackCatalogProvider` (piezas; lee el `.csv` y, si falta, el `.json`) y `RackFrameTemplateProvider` (plantillas). El plugin y las pruebas copian estos archivos a una carpeta `catalogs/` junto al ensamblado; `JsonRackCatalogProvider.FromBaseDirectory()` los resuelve en runtime relativo al ensamblado (no al proceso de AutoCAD).

## Probar en AutoCAD

1. Abrir AutoCAD 2025.
2. Ejecutar `NETLOAD`.
3. Cargar:

```text
src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

4. Ejecutar cualquiera de los comandos de la seccion Comandos. Los bloques que falten se importan de la biblioteca; los parametros dinamicos (`LONGITUD`, `Distancia1`) se asignan al insertar. Ver `docs/generacion-cabecera-lateral.md`.

## Documentos de contexto

Leer primero:

- `docs/00-indice-contexto.md`
- `docs/01-estado-actual-mvp.md`
- `docs/02-modelo-tecnico-vigente.md`
- `docs/03-guia-desarrollo-y-validacion.md`
- `docs/04-roadmap-operativo.md`

Documentos historicos/especificacion amplia:

- `docs/arquitectura-autocad-racks.md`
- `docs/mvp-configurador-cabeceras.md`
- `docs/modelo-datos-cabecera-rack-selectivo.md`
- `docs/plan-implementacion-mvp-csharp-autocad.md`
- `docs/analisis-macro-vba-cabeceras.md`

## Fuera de alcance actualmente

- Dibujo de las vistas frontal y planta (los modulos dibujan la vista lateral).
- Calculo de rodillos/frenos por capacidad (hoy paso minimo por diametro + freno por fondo de tarima; las reglas de capacidad estan definidas para una fase futura).
- Integracion de la cama de rodamiento dentro del dibujo del sistema dinamico.
- SQLite.
- Exportacion Excel (el BOM se exporta a CSV).
- Metadatos persistidos en DWG.
