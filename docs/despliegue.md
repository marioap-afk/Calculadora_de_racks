# Despliegue e instalación de RackCad

Cómo entregar el plugin a otra persona **sin Visual Studio**: copiar una carpeta, hacer
`NETLOAD` y listo. El catálogo y la librería de bloques se encuentran solos junto al DLL;
solo hace falta "apuntar" una ruta si quieres una ubicación compartida.

---

## 1. Requisitos de la máquina destino

| Requisito | Detalle |
|-----------|---------|
| **AutoCAD 2025 o superior** | El plugin es `net8.0-windows`. AutoCAD 2025 es la primera versión sobre **.NET 8**. En **AutoCAD 2024 o anterior NO carga** (usan .NET Framework 4.x). Este es el punto que más suele fallar. |
| **Windows** | La interfaz es WPF. |
| **.NET 8 Desktop Runtime** | Lo trae AutoCAD 2025+. Normalmente no hay que instalar nada aparte. |

No hace falta instalar las DLL de AutoCAD: las pone AutoCAD en tiempo de ejecución (por eso
`AcCoreMgd/AcDbMgd/AcMgd` **no** se copian a la carpeta de salida).

---

## 2. Qué copiar

**La carpeta de salida completa del build**, no solo el DLL. Tras compilar en Release queda en:

```
src\RackCad.Plugin\bin\Release\net8.0-windows\
```

Debe contener (como mínimo):

```
RackCad.Plugin.dll        ← el que se hace NETLOAD
RackCad.Application.dll    ┐
RackCad.Domain.dll         ├─ hermanos: AutoCAD los resuelve desde la misma carpeta
RackCad.UI.dll             ┘
catalogs\                  ← datos maestros + geometría
  post-profiles.csv
  truss-profiles.csv
  beam-profiles.csv
  mensulas.csv
  base-plates.csv
  flow-bed-profiles.csv
  spacers-profiles.csv
  connection-points.csv
  connection-layout.csv
  views.csv
  blocks.csv
  header-templates.json
  defaults.json
  blocks-library.dwg       ← la geometría real de cada pieza (imprescindible para dibujar)
```

> Copia la carpeta **entera**. Si copias solo `RackCad.Plugin.dll`, AutoCAD no encontrará
> los DLL hermanos ni la carpeta `catalogs\`, y todo lookup (bloques, nombres) saldrá vacío.

Para generar esa carpeta desde el repositorio (con AutoCAD **cerrado**, o el DLL queda bloqueado):

```powershell
dotnet build src\RackCad.Plugin\RackCad.Plugin.csproj -c Release
```

Si AutoCAD 2025 no está en la ruta por defecto, pásala al compilar:

```powershell
dotnet build src\RackCad.Plugin\RackCad.Plugin.csproj -c Release -p:AutoCADInstallDir="C:\Program Files\Autodesk\AutoCAD 2026"
```

---

## 3. Instalación (carga manual)

1. Copiar la carpeta a la máquina destino (p. ej. `C:\RackCad\`).
2. Abrir AutoCAD.
3. Comando `NETLOAD` → seleccionar `RackCad.Plugin.dll`.
   - La primera vez AutoCAD puede preguntar si confía en el ensamblado: **Cargar / Load**.
4. Ejecutar `RACKCAD` para abrir el menú.

`NETLOAD` es **por sesión**: hay que repetirlo cada vez que se abre AutoCAD (salvo carga
automática, abajo).

---

## 4. Carga automática (opcional, recomendado para el usuario final)

Para no hacer `NETLOAD` cada vez, cualquiera de estas:

- **APPLOAD → Suite de inicio (Startup Suite):** comando `APPLOAD`, botón *Contents…* de la
  Startup Suite, añadir `RackCad.Plugin.dll`. Se carga solo al abrir cada dibujo.
- **Bundle en `ApplicationPlugins`:** empaquetar como `RackCad.bundle` (carpeta con
  `PackageContents.xml` + los DLL/catálogos) dentro de
  `%AppData%\Autodesk\ApplicationPlugins\`. AutoCAD lo autocarga al iniciar.

---

## 5. Catálogo y librería de bloques (la "base de datos")

Son **dos** cosas, y **ambas se encuentran solas** si copiaste la carpeta completa:

1. **Catálogo (CSV/JSON):** se busca en la carpeta `catalogs` **junto al propio DLL**
   (`CatalogDirectory.Resolve()`). Sin configuración.
2. **Librería de bloques (`blocks-library.dwg`):** por defecto también **junto a los
   catálogos** (`BlockLibraryLocator`). Contiene las definiciones de bloque que se clonan al
   dibujo cuando faltan.

**Apuntar a una ubicación compartida (opcional):** desde el menú `RACKCAD` se puede ver y
elegir la ruta de la librería de bloques. La ruta elegida se guarda por usuario en:

```
%AppData%\RackCad\settings.json
```

Si no se configura nada, se usa el `blocks-library.dwg` que está junto a los catálogos.

> **Importante:** la columna `blockName` de `blocks.csv` debe coincidir **exactamente** con
> el nombre del bloque dentro de `blocks-library.dwg`. Si un bloque no existe en el dibujo ni
> en la librería, la pieza simplemente se omite y se reporta como faltante (no truena).

### Editar catálogos sin recompilar

En una instalación ya desplegada se puede editar cualquier CSV/JSON dentro de la carpeta
`catalogs\` junto al DLL. RackCad recarga el catálogo al re-ejecutar el comando (la caché se
invalida por fecha/tamaño de los archivos), así que **no hace falta reiniciar AutoCAD** para
un cambio de catálogo. Sí conviene cerrar el archivo en Excel antes de volver a dibujar.
(Detalle de formatos y columnas: ver [catalogos-y-plantillas.md](catalogos-y-plantillas.md).)

---

## 6. Comandos disponibles

| Comando | Qué hace |
|---------|----------|
| `RACKCAD` | Menú principal (elige el tipo de rack a diseñar). |
| `RACKCABECERA` | Configurador de cabecera (editor). |
| `RACKCABECERALATERAL` | Cabecera estándar directa al dibujo. |
| `QUICKCABECERA` | Cabecera por línea de comandos (pide poste/fondo/alto). |
| `RACKSISTEMADINAMICO` | Sistema dinámico (pallet flow). |
| `QUICKCAMA` | Cama de rodamiento. |
| `RACKSELECTIVO` | Editor de rack selectivo (matriz frentes × niveles). |
| `RACKEDITAR` | Seleccionar un rack ya dibujado y reabrir su editor; al confirmar redibuja todas sus vistas. |

---

## 7. Solución de problemas

| Síntoma | Causa probable / solución |
|---------|---------------------------|
| `NETLOAD` da error de carga / "no se pudo cargar el ensamblado" | AutoCAD **2024 o anterior** (no soporta .NET 8). Se requiere **2025+**. |
| Los desplegables salen vacíos o los nombres se ven como `escal�n` | La carpeta `catalogs\` no está junto al DLL, o el CSV se guardó con un encoding raro. Copiar la carpeta completa; el lector acepta UTF-8 y ANSI de Excel. |
| El rack se dibuja pero **faltan piezas** | Falta `blocks-library.dwg` junto a los catálogos, o un `blockName` de `blocks.csv` no coincide con el bloque en la librería. Se listan las piezas omitidas en la línea de comandos. |
| Cambié un CSV y no se refleja | Re-ejecuta el comando (no hace falta reiniciar); asegúrate de editar el CSV **dentro de la carpeta `catalogs\` junto al DLL**, no el del repositorio. |
| No puedo recompilar: "el archivo está siendo usado por otro proceso" | AutoCAD tiene bloqueado `RackCad.Plugin.dll`. **Cierra AutoCAD** y vuelve a compilar. |

---

## 8. Resumen para el usuario final

1. Tener **AutoCAD 2025+** en **Windows**.
2. Copiar la carpeta de salida completa (DLLs + `catalogs\` con el `.dwg`).
3. `NETLOAD` → `RackCad.Plugin.dll` (o dejarlo en la Startup Suite).
4. Ejecutar `RACKCAD`.
5. *(Opcional)* Apuntar a una librería/catálogo central desde el menú `RACKCAD`.
