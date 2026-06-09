# Guia de desarrollo y validacion

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
docs/
assets/
```

## Proyectos

`RackCad.Domain`

- Entidades y enums del modelo de racks.
- No debe depender de AutoCAD ni WPF.

`RackCad.Application`

- Servicios de aplicacion.
- Estandar hardcodeado temporal.
- Builder de miembros fisicos.

`RackCad.UI`

- Ventana WPF.
- ViewModel.
- Tablas, arbol, panel de propiedades, vista previa.

`RackCad.Plugin`

- Entrada AutoCAD .NET API.
- Comando `RACKCABECERA`.
- Referencias a `AcCoreMgd`, `AcDbMgd`, `AcMgd`.

## Compilar

```powershell
dotnet build RackCad.sln -v:minimal
```

Si se necesita indicar una ruta distinta de AutoCAD:

```powershell
dotnet build RackCad.sln -p:AutoCADInstallDir="C:\Program Files\Autodesk\AutoCAD 2025"
```

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

4. Ejecutar:

```text
RACKCABECERA
```

## Checklist funcional del configurador

Antes de continuar con dibujo AutoCAD, validar:

- La ventana abre dentro de AutoCAD.
- La cabecera estandar carga con 4 horizontales y 3 paneles.
- Dividir panel crea nueva horizontal.
- Combinar panel elimina horizontal intermedia.
- Al cambiar elevaciones, los IDs vuelven a `H1`, `H2`, `H3` por orden fisico.
- Paneles siempre quedan consecutivos.
- No aparecen referencias viejas como `H2-H5`.
- La vista previa se actualiza.
- Excepciones aparecen al modificar.
- `Restaurar cabecera estandar` limpia excepciones y vuelve al estado inicial.
- `Restaurar layout predeterminado` solo cambia tamanos de UI.

## Archivos generados que no deben versionarse

Ya estan en `.gitignore`:

- `bin/`
- `obj/`
- `.vs/`
- `.appdata/`
- `.localappdata/`
- `.dotnet_home/`
- `.nuget_packages/`

