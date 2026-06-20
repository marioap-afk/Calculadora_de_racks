# Mejoras aplicadas con complementos

Este documento resume las mejoras ya aplicadas a partir de los complementos instalados en el Codespace.

## Logging y diagnostico

Se agrego un logger central:

- `src/RackCad.Application/Diagnostics/RackCadLogger.cs`

Usa:

- `Serilog`
- `Serilog.Sinks.File`

Ubicacion del log:

```text
%LOCALAPPDATA%\RackCad\logs
```

En entornos sin `LocalApplicationData`, usa la carpeta temporal del sistema.

Se integro en:

- `RackCad.Plugin.PluginInitializer`
- `RackCad.Plugin.RackFrameCommands`
- `RackCad.UI.RackFrameConfiguratorWindow`

Eventos registrados:

- inicializacion del plugin;
- cierre del plugin;
- ejecucion de `RACKCABECERA`;
- cierre de la ventana configuradora;
- errores no bloqueantes de UI;
- excepciones no controladas del dispatcher WPF.

Cuando `RACKCABECERA` falla dentro de AutoCAD, tambien se escribe en el editor la ruta del log.

## Catalogos externos

Se agrego el proyecto:

- `src/RackCad.Catalogs`

Incluye un servicio inicial:

- `JsonRackFrameCatalogService`

Soporta:

- cargar catalogos JSON;
- guardar catalogos JSON;
- validar catalogos contra JSON Schema.

Paquetes usados:

- `NJsonSchema`
- `CsvHelper`
- `Microsoft.Data.Sqlite`
- `Serilog`

Archivos semilla:

- `assets/catalogs/rack-frame-catalog.schema.json`
- `assets/catalogs/rack-frame-catalog.sample.json`

El catalogo de ejemplo contiene:

- perfiles de poste;
- perfiles de horizontal;
- perfiles de diagonal;
- placas base;
- puntos de conexion.

## Pruebas automatizadas

Se agrego:

- `tests/RackCad.Tests`

Paquetes usados:

- `xunit`
- `Shouldly`
- `coverlet.collector`
- `Microsoft.NET.Test.Sdk`

Pruebas actuales:

- validar que la cabecera estandar temporal se crea con 4 horizontales y 3 paneles;
- validar que el modelo fisico genera horizontales y diagonales esperadas;
- validar que el catalogo JSON semilla cumple el schema y se carga correctamente.

Comando:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -v:minimal
```

## Build en Codespace

El build recomendado en Codespace es:

```powershell
dotnet build RackCad.Codespace.slnf -v:minimal
```

Este filtro compila:

- `RackCad.Domain`
- `RackCad.Application`
- `RackCad.Catalogs`
- `RackCad.UI`
- `RackCad.Tests`

No compila `RackCad.Plugin` porque requiere las DLL propietarias de AutoCAD.

## Build completo en Windows

En Windows con AutoCAD 2025 instalado:

```powershell
dotnet build RackCad.sln -v:minimal
```

Si AutoCAD esta en otra ruta:

```powershell
dotnet build RackCad.sln -p:AutoCADInstallDir="RUTA_DE_AUTOCAD"
```
