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

El build de CI puede usar la excepción condicional compile-only de
[ADR-0003](../adr/0003-referencias-autocad-para-ci.md). Esa ruta excluye assets de runtime y no
autoriza incluir paquetes o DLL Autodesk en outputs, artifacts o bundle. La instalación y ejecución
siguen requiriendo AutoCAD; cualquier cambio de versiones, fuente, runner, audiencia o finalidad
exige nueva revisión.

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
  secciones.csv
  mensulas.csv
  base-plates.csv
  flow-bed-profiles.csv
  connection-points.csv
  connection-layout.csv
  views.csv
  blocks.csv
  seguridad.csv
  header-templates.json
  defaults.json
  blocks-library.dwg       ← OPCIONAL en el bundle: activo del usuario, no versionado; ver §5
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

## 4. Carga automática (Autoloader bundle — recomendado)

Para que RackCad **cargue solo al abrir AutoCAD** (sin `NETLOAD`), se usa un *bundle* del
Autoloader: una carpeta **`RackCad.bundle`** con un `PackageContents.xml` + `Contents\` (los
DLL y `catalogs\`), colocada en un `ApplicationPlugins`.

**El `dotnet publish` lo arma solo** (flujo canónico y reproducible; `deploy\build-bundle.ps1` lo
envuelve y lo verifica fail-closed). Tras publicar el plugin queda en:

```
src\RackCad.Plugin\bin\Release\net8.0-windows\publish\RackCad.bundle\
  PackageContents.xml   (generado desde deploy\RackCad.bundle\PackageContents.template.xml)
  Contents\
    RackCad.Plugin.dll  (+ Application/Domain/UI)
    catalogs\  (CSV/JSON de producto; blocks-library.dwg es opcional y pertenece al usuario)
```

Generarlo y verificarlo desde el repositorio (AutoCAD **cerrado**):

```powershell
pwsh deploy\build-bundle.ps1            # dotnet publish + deploy\verify-bundle.ps1 (fail-closed)
```

### Instalar
- **Con el script** (compila si pasas `-Build` e instala en el perfil del usuario):
  ```powershell
  pwsh deploy\install-bundle.ps1 -Build
  ```
  Instala `RackCad.bundle` en `%AppData%\Autodesk\ApplicationPlugins\RackCad.bundle` mediante
  staging + respaldo + sustitución verificada. Si ya existe
  `Contents\catalogs\blocks-library.dwg`, lo conserva byte por byte.
- **A mano:** copia esa carpeta `RackCad.bundle` a **`%AppData%\Autodesk\ApplicationPlugins\`**
  (por usuario) o a `%ProgramFiles%\Autodesk\ApplicationPlugins\` (todos los usuarios, requiere
  admin). Cierra AutoCAD antes: bloquea `RackCad.Plugin.dll`. Una copia manual no ofrece el
  respaldo/rollback del script y debe resguardar primero la biblioteca DWG.

Abre AutoCAD 2025+; los comandos (`RACKCAD`, `RACKSELECTIVO`, …) quedan disponibles al arranque.

> El `PackageContents.xml` se **genera** desde la plantilla con `SeriesMin="R25.0"` y
> `SeriesMax="R25.0"` (solo AutoCAD 2025; política en
> [ADR-0004](../adr/0004-estrategia-de-versiones-de-autocad.md)). Para actualizar, cierra AutoCAD y
> ejecuta de nuevo el script. Los DLL y catálogos CSV/JSON se sustituyen por los del bundle nuevo;
> `blocks-library.dwg` se preserva.

### Comportamiento seguro al actualizar

`install-bundle.ps1` no modifica la instalación en sitio. El flujo es:

1. valida que el bundle nuevo tenga manifiesto, los cuatro DLL y catálogos;
2. copia y verifica el bundle en una carpeta de staging única junto al destino;
3. copia al staging el `blocks-library.dwg` de la instalación existente, si lo hay, y verifica su hash;
4. renombra la instalación anterior a un respaldo recuperable;
5. activa el staging y vuelve a validar la instalación;
6. elimina el respaldo únicamente después de terminar las verificaciones.

Si falla después de crear el respaldo, el script intenta restaurar automáticamente la instalación
anterior. Si no puede, imprime la ruta exacta de la carpeta `.RackCad.bundle.backup-*` que debe
renombrarse manualmente a `RackCad.bundle`. Un bundle fallido apartado queda como
`.RackCad.bundle.failed-*` para diagnóstico. AutoCAD abierto se considera error: el script termina
con código no cero sin tocar el destino.

La política actual es deliberadamente simple:

| Contenido | Política de actualización |
|---|---|
| `PackageContents.xml`, DLLs y catálogos CSV/JSON | Producto: se usa la versión del bundle nuevo. No hay fusión. |
| `Contents\catalogs\blocks-library.dwg` | Usuario: se conserva la versión ya instalada. |
| `%AppData%\RackCad\` y biblioteca configurada en otra ruta | Fuera del bundle: el instalador no los toca. |

La separación formal entre catálogos base del producto y overrides del usuario queda como iniciativa
arquitectónica futura; hasta entonces, una edición directa de CSV/JSON dentro del bundle funciona en
runtime, pero **se reemplaza en la siguiente actualización**.

**Alternativa manual:** `APPLOAD` → *Suite de inicio (Startup Suite)* → añadir
`RackCad.Plugin.dll`. Se carga solo al abrir cada dibujo (pero sin la comodidad del bundle).

### Compartir la app y publicar versiones nuevas

El bundle **no es un artefacto que se mantenga a mano**: el target `AssembleAutoloaderBundle` del
`.csproj` elimina y **regenera completo** `RackCad.bundle` (DLLs frescos + `catalogs\`) en cada
`dotnet publish`. Esto evita que sobrevivan archivos de configuraciones anteriores. El
`PackageContents.xml` **se genera** desde `deploy\RackCad.bundle\PackageContents.template.xml`
sustituyendo la versión y las series de AutoCAD; la versión es **única** (`RackCadVersion` en
`Directory.Build.props`) y alimenta también los atributos de los ensamblados, que llevan
`InformationalVersion = <versión>+<sha>` para trazar el commit exacto. Subir de versión = cambiar
`RackCadVersion` en un solo lugar y volver a publicar. Así que "actualizar el bundle" = **publicar**.

Para pasar una versión nueva a otra persona:

1. Cierra AutoCAD (bloquea `RackCad.Plugin.dll`).
2. `pwsh deploy\install-bundle.ps1 -Build` (o `pwsh deploy\build-bundle.ps1`, o `dotnet publish -c Release`).
3. Comparte la carpeta `RackCad.bundle` (o un `.zip`); la otra persona la pega en su
   `%AppData%\Autodesk\ApplicationPlugins\`. Para una actualización segura en una máquina con el
   repositorio, debe usar `install-bundle.ps1`; si solo recibe el ZIP, debe respaldar manualmente
   `blocks-library.dwg` antes de reemplazar. Al reabrir AutoCAD carga sola.

Dos matices que evitan re-desplegar:

- **Cambios solo de datos NO requieren reiniciar AutoCAD.** Los catálogos son CSV/JSON que RackCad **lee
  en vivo** (recarga por fecha/tamaño al re-ejecutar el comando). Se pueden editar dentro de
  `Contents\catalogs\` para una prueba local, pero hoy son archivos de producto: el instalador los
  reemplaza en la siguiente actualización. Para conservar un cambio hay que aplicarlo también a la
  fuente controlada que produce el bundle.
- **Librería/catálogo central compartido:** si apuntas la ruta de la librería de bloques a un disco/red
  común (menú `RACKCAD`, ver §5), un cambio de bloques lo ven todas las máquinas sin re-desplegar.

---

## 5. Catálogo y librería de bloques (la "base de datos")

Son **dos** cosas. El catálogo sí forma parte del bundle; la biblioteca DWG es un activo externo del usuario:

1. **Catálogo (CSV/JSON):** se busca en la carpeta `catalogs` **junto al propio DLL**
   (`CatalogDirectory.Resolve()`). Sin configuración.
2. **Librería de bloques (`blocks-library.dwg`):** NO está versionada ni se genera. `BlockLibraryLocator` busca la
   ruta elegida por el usuario y, como fallback, un archivo con ese nombre junto a los catálogos. Contiene las
   definiciones de bloque que se clonan al dibujo cuando faltan.

**Apuntar a una ubicación compartida (opcional):** desde el menú `RACKCAD` se puede ver y
elegir la ruta de la librería de bloques. La ruta elegida se guarda por usuario en:

```
%AppData%\RackCad\settings.json
```

Si no se configura nada, se intenta usar `blocks-library.dwg` junto a los catálogos. La primera instalación no
garantiza que exista; en actualizaciones posteriores, el instalador conserva el archivo que ya esté en esa ruta.
Para un despliegue reproducible, configurar explícitamente la biblioteca aprobada en cada perfil o proporcionar el
activo del usuario después de la primera instalación.

> **Importante:** la columna `blockName` de `blocks.csv` debe coincidir **exactamente** con
> el nombre del bloque dentro de `blocks-library.dwg`. Si un bloque no existe en el dibujo ni
> en la librería, la pieza simplemente se omite y se reporta como faltante (no truena).

### Editar catálogos sin recompilar

En una instalación ya desplegada se puede editar cualquier CSV/JSON dentro de la carpeta
`catalogs\` junto al DLL. RackCad recarga el catálogo al re-ejecutar el comando (la caché se
invalida por fecha/tamaño de los archivos), así que **no hace falta reiniciar AutoCAD** para
un cambio de catálogo. Sí conviene cerrar el archivo en Excel antes de volver a dibujar. Esta edición
es local y temporal respecto al despliegue: una actualización instala de nuevo los catálogos de producto
y no intenta fusionar el CSV/JSON editado.
(Detalle de formatos y columnas: ver [catalogos-y-plantillas.md](catalogos-y-plantillas.md).)

### Prueba reproducible del instalador

El harness crea únicamente bundles falsos bajo `%TEMP%`; no toca la instalación real:

```powershell
pwsh deploy\test-install-bundle.ps1
```

Cubre primera instalación, catálogo sustituido, biblioteca preservada, destino parcial, archivo
bloqueado, rollback tras respaldo, segunda ejecución, rutas con espacios y origen incompleto.

### Verificar el bundle (fail-closed y reproducible)

`deploy\verify-bundle.ps1` valida un `RackCad.bundle` ya armado: estructura, nombres, rutas, versión y
series del manifiesto (contra `Directory.Build.props`) y una **allowlist recursiva fail-closed** que
prueba que solo se distribuyen los cuatro DLL de RackCad y catálogos permitidos —cero DLL Autodesk
(ADR-0003)—; además imprime el inventario con hashes SHA-256.

```powershell
pwsh deploy\verify-bundle.ps1 -BundlePath src\RackCad.Plugin\bin\Release\net8.0-windows\publish\RackCad.bundle
```

Para comprobar reproducibilidad, publica dos veces el mismo commit y compara inventarios:
`-InventoryOutPath` escribe el inventario y `-BaselineInventoryPath` lo contrasta (deben coincidir ruta
y hash). `deploy\build-bundle.ps1` encadena `dotnet publish` + verificación.

---

## 6. Comandos disponibles

Cada comando tiene un **alias corto** (columna "Atajo") que viaja con el plugin — no hay que editar
`acad.pgp`. Si un atajo choca con un alias que ya tengas en tu `acad.pgp`, el del PGP gana; usa el
comando completo o cambia el atajo en `RackFrameCommands.Aliases.cs`.

| Comando | Atajo | Qué hace |
|---------|-------|----------|
| `RACKCAD` | `RK` | Menú principal (elige el tipo de rack a diseñar). |
| `RACKAYUDA` | `RA` | Ventana con **todos los comandos y sus atajos** dentro de AutoCAD (esta tabla, viva). |
| `RACKCABECERA` | `RCB` | Configurador de cabecera (editor). |
| `QUICKCABECERA` | `QCB` | Cabecera por línea de comandos (pide poste/fondo/alto). |
| `RACKSISTEMADINAMICO` | `RSD` | Sistema dinámico (pallet flow). |
| `QUICKCAMA` | `QCM` | Cama de rodamiento. |
| `RACKSELECTIVO` | `RS` | Editor de rack selectivo (matriz frentes × niveles). |
| `RACKEDITAR` | `RED` | Seleccionar un rack ya dibujado y reabrir su editor; al confirmar redibuja todas sus vistas. |
| `RACKDUPLICAR` | `RD` | Copiar un rack como uno **independiente** (GUID nuevo; editar la copia no afecta al original), **al estilo COPY**: punto base → puntos de destino con liga elástica, **copia múltiple por defecto** (Enter/Esc termina; `Unica` cambia a una sola). Cada copia se llama "… - copia N". Duplica la vista clicada. |
| `RACKLISTA` | `RL` | Tabla de todos los racks del dibujo (nombre, tipo, vistas presentes, nº de copias) con zoom al seleccionado. |
| `RACKBOMTOTAL` | `RB` | BOM consolidado de todo el dibujo (desglose por rack × copias + gran total por componente). |
| `RACKLAYOUT` | `RLY` | Replica la vista en **planta** de un rack en una rejilla de almacén: filas × columnas + pasillos + numeración automática (A1, B2…). Copias **enlazadas** (un bloque, editar una edita todas; el BOM las cuenta) o **independientes** (GUID/nombre propio por copia). Opcional: hileras **espalda-con-espalda** (pares con flue, pasillo solo entre pares) y **verificar encaje** contra un edificio (ancho × largo) — avisa si la rejilla no cabe. La orientación se hereda del rack (rótalo antes para girar la rejilla). |
| `RACKRELLENAR` | `RR` | **Rellena automáticamente** el área disponible con un rack: dibuja el contorno de la nave como **polilínea cerrada** en la capa `RACKCAD_SITIO` (acepta formas en L; los arcos se aproximan por vértices) y las **columnas** ahí mismo (círculos, rectángulos o bloques — se libran por su caja + holgura). Calcula la rejilla máxima que cabe (prueba ambas orientaciones, opcional espalda-con-espalda), reporta cuántos racks caben y cuántas celdas se omiten, y al confirmar coloca las copias **enlazadas** + etiquetas. |

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
