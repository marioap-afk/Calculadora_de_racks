# RackCad - Configurador de cabeceras de rack

Proyecto MVP para una aplicacion profesional de AutoCAD .NET en C# enfocada en configurar cabeceras de racks industriales.

El estado actual no dibuja entidades AutoCAD todavia. El comando `RACKCABECERA` abre una ventana WPF modal dentro de AutoCAD para configurar una cabecera estandar, modificar excepciones y validar visualmente el modelo antes de pasar a generacion CAD.

## Estado actual

- Solucion Visual Studio / .NET 8: `RackCad.sln`.
- Plugin AutoCAD: `src/RackCad.Plugin`.
- UI WPF: `src/RackCad.UI`.
- Logica de aplicacion: `src/RackCad.Application`.
- Modelo de dominio: `src/RackCad.Domain`.
- Comando disponible: `RACKCABECERA`.
- AutoCAD objetivo actual: AutoCAD 2025 completo, no LT.
- Estandar temporal: cabecera de 132 in de alto, 42 in de fondo, horizontales en 0/44/88/132 in y paneles derivados.

## Compilar

```powershell
dotnet build RackCad.sln -v:minimal
```

Advertencias conocidas: `MSB3277` por conflictos de versiones entre referencias AutoCAD 2025 y ensamblados .NET. Actualmente no bloquean la compilacion.

## Probar en AutoCAD

1. Abrir AutoCAD 2025.
2. Ejecutar `NETLOAD`.
3. Cargar:

```text
src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

4. Ejecutar:

```text
RACKCABECERA
```

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

- Dibujo real en AutoCAD.
- Bloques dinamicos.
- SQLite.
- BOM formal.
- Exportacion Excel.
- Metadatos persistidos en DWG.

