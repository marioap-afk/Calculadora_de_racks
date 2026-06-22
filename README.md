# RackCad - Configurador de cabeceras de rack

Proyecto MVP para una aplicacion profesional de AutoCAD .NET en C# enfocada en configurar cabeceras de racks industriales.

El comando `RACKCABECERA` abre una ventana WPF modal dentro de AutoCAD para configurar una cabecera estandar, modificar excepciones y validar visualmente el modelo. Como primer paso de generacion CAD ya existe el dibujo **block-based de la cabecera en vista lateral**: el boton `Insertar en AutoCAD` del configurador (o el comando `RACKCABECERALATERAL`) inserta los bloques anclados a sus puntos de conexion. Requiere que los bloques de AutoCAD esten definidos en el dibujo; los que falten se informan y se omiten. Las vistas frontal/planta y el motor CAD completo siguen pendientes.

## Estado actual

- Solucion Visual Studio / .NET 8: `RackCad.sln`.
- Plugin AutoCAD: `src/RackCad.Plugin`.
- UI WPF: `src/RackCad.UI`.
- Logica de aplicacion: `src/RackCad.Application`.
- Modelo de dominio: `src/RackCad.Domain`.
- Comandos disponibles: `RACKCAD` (menu), `RACKCABECERA` (configurador) y `RACKCABECERALATERAL` (dibuja la cabecera estandar en vista lateral).
- AutoCAD objetivo actual: AutoCAD 2025 completo, no LT.
- Estandar temporal: cabecera de 132 in de alto, 42 in de fondo, horizontales en 0/44/88/132 in y paneles derivados.

## Compilar

```powershell
dotnet build RackCad.sln -v:minimal
```

Advertencias conocidas: `MSB3277` por conflictos de versiones entre referencias AutoCAD 2025 y ensamblados .NET. Actualmente no bloquean la compilacion.

## Modos de configuracion

El configurador abre en **Configuracion rapida**: solo se elige tipo de cabecera (plantilla), tipo de poste, alto y fondo, y se pulsa `Generar cabecera`. Un check **Editor avanzado** en la barra superior muestra el editor detallado actual (horizontales, paneles, perfiles, excepciones). La vista previa se mantiene en ambos modos.

Las plantillas viven en `assets/catalogs/header-templates.json` (se cargan con `RackFrameTemplateProvider`, con respaldo interno en `RackFrameTemplateCatalog` si el archivo falta). La generacion la hace `RackFrameConfigurationFactory`. Las elevaciones de cada plantilla estan en pulgadas y escalan con el alto elegido. Para editarlas ver `docs/catalogos-y-plantillas.md`.

## Pruebas

Pruebas unitarias en `tests/RackCad.Tests` (xUnit). Cubren `RackCad.Domain` y `RackCad.Application`, que son `net8.0` puro y por tanto corren en cualquier OS (incluido el codespace Linux, sin AutoCAD ni Windows):

```bash
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
```

## Catalogos externos

Los perfiles, placas, puntos, vistas, bloques y layout de conexion viven como CSV versionado en `assets/catalogs/` (las plantillas y los defaults siguen en JSON). Tras la unificacion: horizontales y diagonales comparten una sola lista de celosia (`truss-profiles.csv`) y los refuerzos son postes (`post-profiles.csv`); ya no existen `diagonal-profiles` ni `reinforcement-profiles`.

- `post-profiles.csv`
- `truss-profiles.csv` (horizontales y diagonales de celosia)
- `base-plates.csv`
- `connection-points.csv`
- `views.csv`
- `connection-layout.csv` (posicion 2D de cada punto por pieza y vista)
- `blocks.csv` (nombre de bloque de AutoCAD por pieza y vista)
- `defaults.json`
- `header-templates.json`

Como editar estos archivos y como se relacionan entre si: `docs/catalogos-y-plantillas.md` y `docs/modelo-de-datos.md`.

Se cargan con `RackCad.Application.Catalogs.JsonRackCatalogProvider` (piezas; lee el `.csv` y, si falta, el `.json`) y `RackCad.Application.RackFrames.RackFrameTemplateProvider` (plantillas). El plugin y el proyecto de pruebas copian estos archivos a una carpeta `catalogs/` junto al ensamblado, de modo que `JsonRackCatalogProvider.FromBaseDirectory()` los resuelve en runtime. Una prueba (`CatalogStandardConsistencyTests`) garantiza que todo id usado por la cabecera estandar exista en los catalogos antes de migrar los valores hardcodeados.

## Probar en AutoCAD

1. Abrir AutoCAD 2025.
2. Ejecutar `NETLOAD`.
3. Cargar:

```text
src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

4. Ejecutar uno de los comandos:

```text
RACKCAD                 (menu principal)
RACKCABECERA            (configurador; el boton "Insertar en AutoCAD" dibuja lo configurado)
RACKCABECERALATERAL     (dibuja la cabecera estandar en vista lateral, sin dialogo)
```

Para que el dibujo lateral inserte piezas, los bloques de AutoCAD referidos en `blocks.csv` (vista `LATERAL`) deben existir en el dibujo, con los parametros dinamicos `LONGITUD` (poste) y `Distancia1` (travesano). Ver `docs/generacion-cabecera-lateral.md`.

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

- Dibujo de las vistas frontal y planta (solo existe la cabecera lateral).
- Definicion automatica de los bloques dinamicos en el DWG (deben existir previamente).
- SQLite.
- BOM formal.
- Exportacion Excel.
- Metadatos persistidos en DWG.

