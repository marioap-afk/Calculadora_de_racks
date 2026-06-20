# Pendientes de instalacion local

Este documento lista lo que debe instalarse o configurarse fuera del Codespace para compilar y probar el plugin completo de AutoCAD.

## Maquina objetivo

- Windows.
- Visual Studio 2022.
- AutoCAD 2025 completo.
- .NET SDK compatible con `net8.0-windows`.

## Visual Studio 2022

Instalar Visual Studio 2022 con el workload:

- `.NET desktop development`.

Este workload es necesario para trabajar comodamente con:

- C#.
- WPF.
- proyectos `net8.0-windows`.

## AutoCAD 2025

Instalar AutoCAD 2025 completo.

No usar AutoCAD LT, porque el plugin depende de AutoCAD .NET API y carga mediante `NETLOAD`.

El proyecto espera por defecto esta ruta:

```text
C:\Program Files\Autodesk\AutoCAD 2025
```

Desde esa instalacion deben estar disponibles:

- `AcCoreMgd.dll`
- `AcDbMgd.dll`
- `AcMgd.dll`

Si AutoCAD esta instalado en otra ruta, compilar indicando `AutoCADInstallDir`:

```powershell
dotnet build RackCad.sln -p:AutoCADInstallDir="RUTA_DE_AUTOCAD"
```

## ObjectARX SDK 2025

Instalar ObjectARX SDK 2025 es recomendado para desarrollo CAD avanzado.

No es estrictamente necesario para el build actual si AutoCAD ya aporta las DLL requeridas, pero ayuda con:

- documentacion de API;
- ejemplos oficiales;
- tipos y patrones de AutoCAD .NET;
- futuras operaciones con entidades, transacciones, capas, XData y extension dictionaries.

## Git LFS

Instalar Git LFS antes de versionar archivos pesados como:

- bloques `.dwg`;
- plantillas `.dwt`;
- assets CAD grandes.

Comando recomendado:

```powershell
git lfs install
```

## Assets pendientes

Todavia falta crear y poblar assets reales del proyecto.

Carpetas previstas:

- `assets/blocks`
- `assets/templates`
- `assets/catalogs`

Contenido pendiente:

- libreria real de bloques AutoCAD;
- plantillas DWG/DWT;
- catalogos reales de perfiles;
- catalogos de placas base;
- catalogos de puntos de conexion;
- plantillas de cabecera;
- reglas/versiones de catalogo.

## Validacion local

En Windows, despues de instalar AutoCAD y dependencias:

```powershell
dotnet restore RackCad.sln
dotnet build RackCad.sln -v:minimal
```

Para probar en AutoCAD:

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

## Nota sobre Codespace

El Codespace ya tiene instalado y configurado lo posible sin AutoCAD:

- paquetes NuGet;
- analizadores;
- proyecto de pruebas;
- CI;
- filtro `RackCad.Codespace.slnf`;
- build de core, catalogos, UI y tests.

El build completo de `RackCad.sln` en Codespace no puede compilar `RackCad.Plugin` porque las DLL propietarias de AutoCAD no estan disponibles en Linux.
