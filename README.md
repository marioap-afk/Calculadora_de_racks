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
| **Sistema dinamico (pallet flow)** | `RackDynamicSystemWindow` | Vistas laterales por poste, frontal de salida, frontal de entrada y planta ligadas por GUID. La cantidad total de frentes se escribe como entero; al crecer, los nuevos copian el frente seleccionado. El editor usa una matriz frente x nivel: cada frente puede tener distinta cantidad de posiciones, niveles, fondos y posicion inicial dentro de la estructura compartida. El panel `Celda: Frente N - Nivel N` concentra los datos estructurales, el inicio del primer larguero y el peralte intermedio; este ultimo se aplica a celda, nivel, frente o todas como en el selectivo. El frente con menos fondos manda los dos espacios `+6"` y el patron cabecera/separador; los frentes mayores solo lo prolongan, por lo que pueden empezar en separador (con poste limite independiente). `BFR = frente de tarima + 2"`, el largo IN/OUT automatico es `suma(BFR) + 6"` (con override manual) y `LONGITUD` de la cama es el tramo longitudinal propio del frente menos `4"`. Un peralte global de poste se aplica a todas las cabeceras. Cada lateral conserva cabeceras, separadores, IN/OUT C6, camas inclinadas y apoyos intermedios, pero toma solo la profundidad/altura/niveles de los frentes adyacentes al poste elegido. Las preliminares consumen ese mismo corte: la lateral limita su profundidad y los frontales muestran la altura adyacente de cada poste y resaltan una celda, no una columna completa. Los cortes frontales muestran postes/placas + IN/OUT. La planta omite las camas. Botas, laterales, desviadores, defensas de montacargas y guias de entrada se proyectan en las vistas aplicables; la defensa tiene activacion y `LONGITUD` independientes para salida/entrada en cada poste, y la guia se habilita por frente/nivel en la entrada. Numeracion de frentes/niveles, nombre y cotas comparten configuracion. En planta el refuerzo continua despues del poste principal sobre el eje de flujo, en la misma linea transversal y sin espejo. El BOM incluye cabeceras/postes/placas, apoyos reforzados o limites sencillos, separadores, IN/OUT por frente y nivel, intermedios por longitud/peralte, camas sin despiece con longitud/BFR y toda la seguridad fisica sin duplicarla por vista. Internamente usa `DynamicRackDesign -> DynamicRackSystemResolver` y builders puros por vista. |
| **Cama de rodamiento (flow bed)** | `RackFlowBedWindow` | Riel (LONGITUD parametrica), tope, rodillos al paso minimo por diametro y frenos segun fondo de tarima; tipo dinamica o pushback (sin frenos). Ventana con vista previa o comando rapido. |
| **Selectivo (editor avanzado pallet-driven)** | `RackSelectiveWindow` | Vistas frontal, lateral (cortes por poste) y planta, ligadas por GUID. Matriz **frentes x niveles**; cada celda = tarima (frente/alto) + tarimas por nivel + larguero + peralte de larguero. Geometria en `SelectiveGeometryResolver` (largueros por troquel, claro por tarima+holgura, altura por frente desde el escalon, datum de piso en Y=0, larguero a piso por frente). Overrides manuales opcionales por celda (vacio = auto). Cada poste (N frentes -> N+1 postes) puede referenciar una cabecera embebida de la que sale su placa/peralte. BOM (postes, placas, largueros, mensulas) con `SelectiveBomBuilder` + `RackBomWindow` (arbol + export CSV/XLSX). Elementos de seguridad catalog-driven (bota H/C, protector lateral H/C, separador, larguero/poste tope, desviadores A/L y parrilla) desde el dialogo "Seguridad", con una variante exclusiva por familia, dibujo y BOM. |

## Comandos

```text
RACKCAD                 (menu principal: cabecera, sistema dinamico, cama, selectivo, biblioteca de disenos, biblioteca de bloques)
RACKCABECERA            (configurador de cabeceras; "Insertar en AutoCAD" dibuja lo configurado)
QUICKCABECERA           (cabecera sin interfaz: poste, fondo y alto por linea de comandos)
RACKSISTEMADINAMICO     (dibuja el sistema dinamico por defecto, sin dialogo)
QUICKCAMA               (cama sin interfaz: tipo, rodillo, fondo del carril y de tarima)
RACKSELECTIVO           (abre el editor selectivo; dibuja la vista frontal al confirmar)
RACKEDITAR              (selecciona un rack dibujado y reabre su editor precargado; ver Round-trip)
RACKDUPLICAR            (copia un rack como uno INDEPENDIENTE, con GUID nuevo; ver Round-trip)
RACKLISTA               (tabla de todos los racks del dibujo: nombre, tipo, vistas, copias; zoom al elegido)
RACKBOMTOTAL            (BOM consolidado de TODO el dibujo: desglose por rack x copias + gran total)
RACKLAYOUT              (layout de almacen v1: rejilla de racks + pasillos + numeracion)
RACKRELLENAR            (lee el sitio de una capa y auto-rellena con racks)
RACKAYUDA               (ventana in-app con todos los comandos y sus atajos cortos: RS, RED, RD, RL, ...)
```

## Identidad y round-trip (RACKEDITAR)

Los cuatro tipos comparten la misma logica reutilizable de identidad y edicion en sitio:

- Cada rack dibujado es **una** definicion de bloque; las copias son referencias a ella.
- En la **definicion** del bloque se embebe (diccionario de extension, `Xrecord` troceado en fragmentos <=255 chars) un sobre unificado `RackEmbedDocument { SchemaVersion, Kind, View, Section, Id (GUID), Name, Design (JSON del diseno) }`. `Kind` es uno de `selective`, `dynamic`, `cabecera`, `cama`; `View` es `frontal`, `lateral` o `planta`; `Section` identifica el corte (en frontal dinamico: 0 = salida, 1 = entrada; en lateral dinamico/selectivo: indice de poste; -1 = no seccionada/legacy).
- `RACKEDITAR` lee el sobre del bloque seleccionado, **despacha por `Kind`** y reabre el editor correcto precargado (`LoadExisting`). Al confirmar, **redefine la definicion en sitio** (`RedefineSystemBlock` + `Regen`): todas las copias se actualizan a la vez y ninguna se mueve.
- **Botones Actualizar / Insertar (convencion permanente en las cuatro ventanas):** **Actualizar** redibuja en sitio las vistas existentes (solo edicion); **Insertar {vista}** agrega una vista nueva **enlazada** (mismo GUID) y refresca las demas. Dinamico expone lateral, frontal salida, frontal entrada y planta; cama conserva una sola vista.
- **`RACKDUPLICAR`** hace una copia **independiente** (GUID nuevo, nombre "- copia"): editar la copia no afecta al original. Distinto del `COPY` de AutoCAD, que comparte la definicion y por ende el GUID (esas copias se editan juntas).
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

Los perfiles, placas, puntos, vistas, bloques y layout de conexion viven como CSV versionado en `assets/catalogs/` (las plantillas y los defaults siguen en JSON). Todos los perfiles estructurales (postes, celosia, largueros y separadores) viven en UN solo `secciones.csv` con columna `rol`; horizontales y diagonales comparten la celosia y los refuerzos son postes.

- `secciones.csv` (TODOS los perfiles estructurales en una hoja, columna `rol` = POSTE | CELOSIA | LARGUERO | SEPARADOR; los refuerzos son postes, horizontales y diagonales comparten la celosia, y los largueros llevan `peraltes` = valores permitidos y `mensula` = FK a mensulas)
- `mensulas.csv` (mensulas del selectivo)
- `base-plates.csv` (con `peralteBase` / `peraltePorPeraltePoste` -> `StandardPeralte`)
- `flow-bed-profiles.csv` (cama de rodamiento: riel/rodillo/freno/tope, columna `role`)
- `connection-points.csv`
- `views.csv`
- `connection-layout.csv` (posicion 2D de cada punto por pieza y vista; X = localX + localXPorParam*valor(paramX), Y = localY + localYPorParam*valor(paramY))
- `blocks.csv` (nombre de bloque de AutoCAD por pieza y vista)
- `defaults.json`
- `header-templates.json`
- `seguridad.csv` (elementos de seguridad: bota, protector lateral, tope, desviador, defensa de montacargas, guia de entrada y parrilla; con costo/moneda/unidad para cotizacion)
- `blocks-library.dwg` (definiciones de bloque; **NO versionado** — es el DWG del usuario, ver seccion Biblioteca de bloques)

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

Para que RackCad **cargue solo al abrir AutoCAD** (sin `NETLOAD` por sesion), instala el bundle del Autoloader con `pwsh deploy\install-bundle.ps1 -Build` (AutoCAD cerrado). Detalle de despliegue, carga automatica y como compartir la app: `docs/despliegue.md`.

## Documentos de contexto

Leer primero:

- `docs/HANDOFF.md` (estado actual, trabajo reciente y siguientes tareas — el documento de continuidad)
- `AGENTS.md` (convenciones obligatorias para agentes y desarrolladores) y `CLAUDE.md` (indice para Claude)
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

- **Selectivo**: frontal, lateral y planta, ligadas por el mismo GUID. La frontal es un bloque (postes + placas + largueros por nivel). La lateral son cortes: un bloque por poste (la cabecera del poste en perfil + las secciones de largueros frente/atras por nivel); al insertar se pregunta que corte por numero de poste y se coloca con jig. La planta es un bloque (una cabecera-planta por frente apilada en Y + largueros frente/atras por bahia; X = fondo, Y = frente).
- **Cabecera**: lateral y planta, ligadas por GUID (planta = 2 huellas de poste + placas + celosia colapsada a un miembro con longitud = A-corte del travesano).
- **Sistema dinamico**: cortes laterales por poste, dos cortes frontales (salida/entrada) y planta, ligados por GUID. Al insertar lateral se pide el numero de poste; cada corte usa la profundidad, altura y niveles de sus frentes adyacentes. Posiciones, niveles, fondos, inicio longitudinal e inicio del primer larguero se editan por frente; pueden copiarse al frente actual, a los frentes seleccionados o a todos. Tarima, claro y ambos largueros se editan por celda frente x nivel con alcances Celda/Seleccionadas/Nivel/Frente/Todas; Ctrl + clic mantiene la seleccion multiple. La estructura longitudinal compartida la gobierna el frente mas corto. La planta no dibuja camas; los frontales son cortes y solo incluyen largueros IN/OUT. Las vistas comparten seguridad, numeracion, nombre y configuracion de cotas.
- **Cama de rodamiento**: lateral.

Las vistas lateral/planta solo se insertan desde `RACKEDITAR` de una vista frontal/lateral existente (los botones se deshabilitan con tooltip si no aplica), para que nunca queden huerfanas. `RACKEDITAR` sobre cualquier vista reabre el editor del sistema completo y al confirmar redibuja todas las vistas (encontradas por GUID escaneando las definiciones de bloque).

## Fuera de alcance actualmente

- Calculo de rodillos/frenos por capacidad (hoy paso minimo por diametro + freno por fondo de tarima; las reglas de capacidad estan definidas para una fase futura).
- SQLite.
