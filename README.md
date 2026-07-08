# RackCad - Diseno y dibujo de racks industriales

Plugin de AutoCAD .NET 8 en C#/WPF para **disenar y dibujar** racks industriales. Ya no es solo un configurador de cabeceras: maneja **cuatro tipos de rack**, cada uno con su ventana editora, su dibujo en AutoCAD y round-trip de edicion.

Cada rack dibujado es una **definicion de bloque** de AutoCAD que se coloca con el mouse (jig); sus copias son referencias a esa definicion. Los bloques de pieza que falten en el dibujo activo se **importan automaticamente** desde una biblioteca DWG (configurable desde el menu principal), de modo que los comandos funcionan incluso en un dibujo en blanco.

## Estado actual

- Solucion Visual Studio / .NET 8: `RackCad.sln`.
- Plugin AutoCAD: `src/RackCad.Plugin` (adaptador delgado; unico proyecto que toca la API de AutoCAD).
- UI WPF: `src/RackCad.UI` (no referencia AutoCAD).
- Logica de aplicacion (geometria/calculo puro, testeable): `src/RackCad.Application`.
- Modelo de dominio: `src/RackCad.Domain`.
- AutoCAD objetivo actual: AutoCAD 2025 completo, no LT.

## Modulos (tipos de rack)

Cada tipo tiene su ventana editora WPF, su servicio de dibujo en AutoCAD y su round-trip de edicion.

| Modulo | Ventana | Que hace |
|---|---|---|
| **Cabecera (marco)** | `RackFrameConfiguratorWindow` | Un marco = 2 postes + placas base + celosia. Horizontales = fuente de verdad; paneles derivados. Configuracion rapida ("Insertar" en un clic) + editor avanzado (horizontales, paneles, perfiles, refuerzo de poste, excepciones). El peralte de la placa base es editable por placa (`BasePlatePlacement.PeralteOverride`; null = derivado con `StandardPeralte`). |
| **Sistema dinamico (pallet flow)** | `RackDynamicSystemWindow` | Vista lateral del sistema completo: cabeceras a lo largo del tramo (celosia espejeada) como bloques anidados compartidos, separadores por nivel, postes derivados con refuerzo opcional, altura de cabecera automatica desde la carga, presets por modulo, BOM. |
| **Cama de rodamiento (flow bed)** | `RackFlowBedWindow` | Riel (LONGITUD parametrica), tope, rodillos al paso minimo por diametro y frenos segun fondo de tarima; tipo dinamica o pushback (sin frenos). Ventana con vista previa o comando rapido. |
| **Selectivo (editor avanzado pallet-driven)** | `RackSelectiveWindow` | Vista frontal. Matriz **frentes x niveles**; cada celda = tarima (frente/alto) + tarimas por nivel + larguero + peralte de larguero. Geometria en `SelectiveGeometryResolver` (largueros por troquel, claro por tarima+holgura, altura por frente desde el escalon, datum de piso en Y=0, larguero a piso por frente). Overrides manuales opcionales por celda (vacio = auto). Cada poste (N frentes -> N+1 postes) puede referenciar una cabecera embebida de la que sale su placa/peralte. BOM (postes, placas, largueros, mensulas) con `SelectiveBomBuilder` + `RackBomWindow` (grid + export CSV). |

## Comandos

```text
RACKCAD                 (menu principal: cabecera, sistema dinamico, cama, selectivo, biblioteca de bloques)
RACKCABECERA            (configurador de cabeceras; "Insertar en AutoCAD" dibuja lo configurado)
RACKCABECERALATERAL     (dibuja la cabecera estandar en vista lateral, sin dialogo)
QUICKCABECERA           (cabecera sin interfaz: poste, fondo y alto por linea de comandos)
RACKSISTEMADINAMICO     (dibuja el sistema dinamico por defecto, sin dialogo)
QUICKCAMA               (cama sin interfaz: tipo, rodillo, fondo del carril y de tarima)
RACKSELECTIVO           (abre el editor selectivo; dibuja la vista frontal al confirmar)
RACKEDITAR              (selecciona un rack dibujado y reabre su editor precargado; ver Round-trip)
```

## Identidad y round-trip (RACKEDITAR)

Los cuatro tipos comparten la misma logica reutilizable de identidad y edicion en sitio:

- Cada rack dibujado es **una** definicion de bloque; las copias son referencias a ella.
- En la **definicion** del bloque se embebe (diccionario de extension, `Xrecord` troceado en fragmentos <=255 chars) un sobre unificado `RackEmbedDocument { Kind, Id (GUID), Name, Design (JSON del diseno) }`. `Kind` es uno de `selective`, `dynamic`, `cabecera`, `cama`.
- `RACKEDITAR` lee el sobre del bloque seleccionado, **despacha por `Kind`** y reabre el editor correcto precargado (`LoadExisting`). Al confirmar, **redefine la definicion en sitio** (`RedefineSystemBlock` + `Regen`): todas las copias se actualizan a la vez y ninguna se mueve.
- El nombre "Rack A" (campo en cada editor) es el nombre del bloque; el GUID va en el sobre para evitar colisiones.
- Stores del diseno por tipo: `SelectivePalletDesignStore` (selectivo), `RackProjectStore` (dinamico/cabecera), `FlowBedConfigurationStore` (cama).

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

- `post-profiles.csv` (postes; los refuerzos son postes)
- `truss-profiles.csv` (una sola lista de celosia: horizontales y diagonales)
- `beam-profiles.csv` (largueros; columna `peraltes` = valores permitidos, FK a mensula)
- `mensulas.csv` (mensulas del selectivo)
- `base-plates.csv` (con `peralteBase` / `peraltePorPeraltePoste` -> `StandardPeralte`)
- `spacers-profiles.csv` (separadores de cabecera del sistema dinamico)
- `flow-bed-profiles.csv` (cama de rodamiento: riel/rodillo/freno/tope, columna `role`)
- `connection-points.csv`
- `views.csv`
- `connection-layout.csv` (posicion 2D de cada punto por pieza y vista; X = localX + slope*param)
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

## Vistas

- **Frontal**: selectivo.
- **Lateral**: cabecera, sistema dinamico y cama de rodamiento.

## Fuera de alcance actualmente

- Vista **lateral del selectivo** (Fase 5 pendiente): cada poste desplegado como su cabecera completa, enlazado por el mismo GUID.
- Dibujo de la vista de planta.
- Calculo de rodillos/frenos por capacidad (hoy paso minimo por diametro + freno por fondo de tarima; las reglas de capacidad estan definidas para una fase futura).
- Integracion de la cama de rodamiento dentro del dibujo del sistema dinamico.
- SQLite.
- Exportacion Excel (el BOM se exporta a CSV).
