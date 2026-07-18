# RackCad — diseño y dibujo de racks industriales

RackCad es un plugin de AutoCAD 2025 desarrollado en .NET 8, C# y WPF. Permite diseñar, dibujar,
editar y obtener el BOM de cabeceras, racks selectivos, sistemas dinámicos y camas de rodamiento.
Cada rack conserva su diseño e identidad dentro del DWG para soportar edición round-trip y vistas
ligadas.

## Inicio rápido

Requisitos: Windows, AutoCAD 2025 completo y .NET 8 SDK. Los bloques reales viven en el
`blocks-library.dwg` del dueño y no se versionan.

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Con AutoCAD cerrado durante el build, carga mediante `NETLOAD`:

```text
src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

Después ejecuta `RACKCAD`. El procedimiento completo y el formato de evidencia están en la
[guía de validación manual](docs/guias/validacion-manual-autocad.md); instalación y Autoloader, en
[despliegue](docs/guias/despliegue.md).

## Comandos

| Comando | Uso |
|---|---|
| `RACKCAD` | Menú principal y bibliotecas. |
| `RACKCABECERA` / `QUICKCABECERA` | Diseñar una cabecera. |
| `RACKSISTEMADINAMICO` | Dibujar el sistema dinámico predeterminado. |
| `QUICKCAMA` | Dibujar una cama de rodamiento. |
| `RACKSELECTIVO` | Diseñar un rack selectivo. |
| `RACKEDITAR` | Reabrir y actualizar un rack existente. |
| `RACKDUPLICAR` | Crear una copia independiente con GUID nuevo. |
| `RACKLISTA` | Listar racks, vistas y copias del dibujo. |
| `RACKBOMTOTAL` | Generar el BOM consolidado. |
| `RACKLAYOUT` / `RACKRELLENAR` | Colocar racks en una rejilla o sitio. |
| `RACKAYUDA` | Consultar comandos y alias dentro de AutoCAD. |

## Documentación

- [HANDOFF](docs/HANDOFF.md): estado vivo, última evidencia, riesgos y siguiente acción.
- [ARCHITECTURE](docs/ARCHITECTURE.md): arquitectura vigente y objetivo.
- [AGENTS](AGENTS.md): convenciones obligatorias y definición de terminado.
- [WORKFLOW](docs/WORKFLOW.md): ramas, worktrees, revisión e integración.
- [ROADMAP](docs/ROADMAP.md): iniciativas y dependencias.
- [Guías](docs/guias/): catálogos, datos, despliegue, generación y validación.
- [Context Packs](docs/context-packs/README.md): selección ligera de contexto por iniciativa.
- [ADRs](docs/adr/README.md): decisiones aceptadas.
- [Archivo](docs/archivo/): transición e historia; no es fuente vigente.

Los catálogos versionados están en `assets/catalogs/`. Consulta
[catálogos y plantillas](docs/guias/catalogos-y-plantillas.md) y el
[modelo de datos](docs/guias/modelo-de-datos.md) antes de editarlos.
