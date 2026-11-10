# I-68 — Evidencia de la unidad (BOM Excel Native Analysis, ID72)

Unit / Initiative / Workflow: `I-68` / I-68 / V2. Claim-Id `cfd9f7bb-7388-42ba-9c7c-0285ea0f3893` (reclamo `3180201d`, base `origin/main` `04a7041492d760170ce3f3ba5fceeb29146107cc`; clasificacion T4).
Especificacion: [I-68-especificacion-id72.md](../../initiatives/I-68-especificacion-id72.md). Propuesta vigente: [I-68-proposal-v2.md](../../initiatives/I-68-proposal-v2.md). Decisiones: [decisions/I-68.md](../decisions/I-68.md).

Estado: Discovery completo. Freeze NO aceptado. **IMPLEMENTATION AUTHORIZATION = NO.**
Clases de afirmacion (LIFECYCLE §4): MEASURED, RECONSTRUCTED, INFERENCE, UNKNOWN. Sin marca = MEASURED en el codigo de la base.

## 1. Preflight

| Hecho | Valor |
|---|---|
| `origin/main` | `04a7041492d760170ce3f3ba5fceeb29146107cc` (Merge I-67) |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364`, ancestro de la base; sin pausa activa |
| Siguiente libre | I-68: la fila y el tag mas altos son I-67; I-62, I-64 e I-66 viven en ramas; ninguna referencia reserva I-68 |
| Reclamo | `feature/bom-excel-analisis-nativo`, primer push aceptado sin force |
| Worktree | `C:\Users\alejandra-mendoza\.codex\worktrees\feature-bom-excel-analisis-nativo` |
| Sesion principal | `claude-opus-5-5` / `xhigh`, MATCH con el perfil requerido (verificado al abrir la corrida) |
| Excel local | 16.0 build 20527, interfaz es-ES; LibreOffice ausente; CI sin Excel |

## 2. Discovery Core

### DC-01 Comportamiento observable actual

| Exportacion | Ventana | Hojas y encabezados |
|---|---|---|
| Consolidada (`ConsolidatedBomXlsxExporter.ToXlsx`) | `RackConsolidatedBomWindow` (RACKBOMTOTAL) | «Por rack»: Rack, Tipo, Copias, Nivel, Categoria, Perfil, Descripcion, Longitud (in), Cantidad. «Total del dibujo»: Nivel, Categoria, Perfil, Descripcion, Longitud (in), Cantidad |
| De un rack (`BomXlsxExporter.ToXlsx`) | `RackBomWindow` | «Lista de materiales»: con componentes Nivel, Categoria, Perfil, Descripcion, Longitud (in), Cantidad; plana Categoria, Perfil, Descripcion, Longitud (in), Cantidad |

Una fila por componente y una por pieza. Encabezados sin acento (`Descripcion`, `Categoria`). Sin Table, PivotTable, formulas ni formato numerico.

**Cantidad.** «Por rack» y la exportacion de un rack: fila Componente = `component.Quantity`; fila Pieza = `piece.Quantity * component.Quantity`; plana = `line.Quantity` (`ConsolidatedBomXlsxExporter.cs:41-52`). Siempre para **una** copia del rack. `int`.

**Copias.** Colocaciones fisicas: el maximo de referencias directas entre los bloques de vista del rack (`RackInventarioCommands.BomTotal.cs:110-113`), mismo criterio que RACKLISTA (`RackInventarioCommands.cs:68-72`). No es cantidad de vistas; limite: una misma vista insertada dos veces cuenta 2 (`RackBlockFinder`, referencias directas). El exportador normaliza `< 1` a 1 (`ConsolidatedBomXlsxExporter.cs:36`).

**Multiplicacion existente.** Exactamente un sitio multiplica por copias: `ConsolidatedBomBuilder.Build` (`total.Quantity += component.Quantity * copies`, `ConsolidatedBom.cs:80`), que alimenta «Total del dibujo». Ningun constructor ni handler conoce las copias (barrido de `Systems/`, `Bom/BomBuilder.cs`, `KindHandlers/`). Por tanto «Por rack» **no** esta multiplicada y «Total del dibujo» **si**.

**Nivel.** Literales `"Componente"` o `"Pieza"`: el peldaño del arbol BOM. `BomComponent` y `BomLine` solo tienen Category, ProfileId, Description, Length, Quantity y Pieces (`BillOfMaterials.cs:8-32`); **ningun productor conserva el nivel fisico de carga**. Se pierde a proposito (p. ej. `CantileverStationBomBuilder.cs:103-108`). No existen filas «sin nivel»: toda fila de «Por rack» lleva uno de los dos literales; las planas (Cama, Cabecera suelta) solo llevan `Pieza`.

**Rack.** `embed.Name.Trim()` o `"(sin nombre)"` (`RackInventarioCommands.BomTotal.cs:219`): nombre logico visible, no el GUID. Dos racks con el mismo nombre comparten etiqueta.

**Longitud.** `double` en pulgadas (encabezado `Longitud (in)`), celda numerica con formato de texto `0.######` y cultura invariante (`XlsxWriter.cs:180`). Redondeos de origen: 2 decimales en BomBuilder, Larguero, FlowBed, Selectivo; 4 en Dinamico, Push Back, Cantilever. Sin longitud significativa = `0` (placas, tope, rodillo, freno, botas). Cantilever comparte encabezados y sus descripciones marcan pulgadas (`CantileverStationBomBuilder.cs:357`).

**BOM vacio.** La hoja sale con solo el encabezado. RACKBOMTOTAL aborta si no hay racks (`:116-129`, `:226`), asi que el caso alcanzable es un rack aprobado con un BOM sin filas: «Por rack» queda sin datos y «Total del dibujo» muestra su fila sintetica, porque `ConsolidatedBomBuilder.ComponentsOf` envuelve todo BOM no basado en componentes (`ConsolidatedBom.cs:91-107`). Alcanzabilidad real por sistema: UNKNOWN.

### DC-02 Autoridades

| Valor o regla | Dueño | Gobierno |
|---|---|---|
| Cantidades por rack | Constructor BOM de cada sistema | Freeze de cada sistema |
| Copias | `RackBomTotal` (Plugin) | I-47 G13 |
| Total ×copias | `ConsolidatedBomBuilder.Build` | codigo y `ConsolidatedBomBuilderTests` |
| Forma del XLSX | `*XlsxExporter` + `XlsxWriter` | ADR-0012 cita el escritor OOXML como consecuencia de la politica |

### DC-03 Persistencia y artefactos

Sin persistencia. El XLSX se escribe y ningun codigo de producto lo relee. Sin DTO, Xrecord ni fallback.

### DC-04 Camino

`RACKBOMTOTAL` → `ScanEnvelopes` → `BomAuthoredAuthority` → `IRackKindHandler.BuildBom` → `ConsolidatedBomBuilder.Build` → `RackConsolidatedBomWindow` → `ConsolidatedBomXlsxExporter.ToXlsx` → `XlsxWriter.Build` → `File.WriteAllBytes`, con error visible por `MessageBox`.

### DC-05 Productores y consumidores

| Sistema | Productor | Forma | Filas con componente y pieza iguales | Componentes sin piezas | RACKBOMTOTAL |
|---|---|---|---|---|---|
| Selectivo | `SelectiveBomBuilder` | arbol | **Larguero** (con su Perfil), Seguridad, Separador, Tope, Parrilla | no | si |
| Dinamico | `SystemBomBuilder` | arbol | Separador, Larguero IN/OUT, intermedio, Seguridad | **Cama** | si |
| Push Back | `PushBackBomBuilder` | arbol | Larguero IN/OUT, troquel redondo, intermedio, Tope posterior, Separador, Seguridad | **Cama** | si |
| Cantilever | `CantileverLineBomBuilder` + `CantileverStationBomBuilder` | arbol | no observado | no | si |
| Cabecera | `BomBuilder.Build` | plana | n/a | n/a | si |
| Cama | `FlowBedBomBuilder` | plana | n/a | n/a | si |
| Larguero | `LargueroBomBuilder` | arbol, 1 componente | componente y pieza Perfil comparten Descripcion | no | no (sin handler) |

`RackBomWindow` (XLSX y CSV sin condiciones) se abre desde `RackFrameConfiguratorWindow:269`, `RackSelectiveWindow:2478`, `RackDynamicSystemWindow:3206`, `RackPushBackSystemWindow:3438`, `RackCantileverWindow:1367`, `RackFlowBedWindow:122`, `RackLargueroWindow:118` y `RackConsolidatedBomWindow:34`. `RackConsolidatedBomWindow` solo desde `RackInventarioCommands.BomTotal.cs:233`. El Dinamico tiene ademas un CSV directo (`RackDynamicSystemWindow:3185-3201`).

**Compartidos.** Cabeceras y postes compartidos se cuentan una vez dentro del constructor de cada sistema; no se encontro emision doble de una partida como dos componentes. Hechos relevantes para la tabla dinamica: dos filas distintas pueden tener igual Descripcion y Longitud (componente y su unica pieza; Larguero y su Perfil), y dos recetas distintas pueden compartir columnas visibles (`BomBuilder.cs:42,49`; `CantileverStationBomBuilder.cs:177-200`). Dinamico: un poste reforzado puede emitir dos piezas identicas dentro de un componente (`SystemBomBuilder.cs:270-277`). Cantidades 0 son posibles en Dinamico (intermedios), INFERENCE.

### DC-06 Pruebas y huecos

`XlsxWriterTests` cubre partes, celdas, escape, nombres y `BomXlsxExporter`. **`ConsolidatedBomXlsxExporter` no tiene prueba directa.** `ConsolidatedBomBuilderTests` cubre el total y el CSV. Productores: `SelectiveBomBuilderTests`, `SystemBomBuilderTests`, `PushBack*Tests`, `CantileverStation*Tests`, `CantileverLineTests`, `FlowBedBomBuilderTests`, `BomBuilderTests`, `LargueroBomBuilderTests`.

### DC-07 Interseccion activa

Ninguna rama remota activa toca `Application/Bom`, las dos ventanas BOM, `RackInventarioCommands.BomTotal.cs` ni sus pruebas: portabilidad `a08afecc`, workspace `44a692a4` y `-a12` `4206b2da`, rackmirror `fb6b5648`, placedclone `5d52e5d8` (0 archivos cada una). Archivo caliente propio: solo la fila de `docs/ROADMAP.md`.

### DC-08 Fundaciones

`docs/FOUNDATIONS.md` no tiene entrada de exportacion XLSX. `XlsxWriter` no cumple §4.1 como fundacion declarada; es codigo interno con dos consumidores, citado por ADR-0012. Sin contradiccion. La capacidad nueva se declara como `Introduces`, no como extension de una fundacion.

### DC-09 Materialidad

| ID | Resultado | Evidencia |
|---|---|---|
| M-01 | no activado | Cantidad total se deriva en un solo sitio con el mismo valor de Copias que se muestra; invariante I-04 lo enlaza con `ConsolidatedBomBuilder` |
| M-02 | no activado | sin persistencia |
| M-03 | no activado | todo cambio observable es lo pedido; CSV y exportacion de un rack intactos (D-2) |
| **M-04** | **activado** | el escritor puede generar sin error un archivo que Excel no abre (P-07, P-09), y sin refresco la hoja parece completa con filtros vacios (P-12, P-14). Proposal §6 lo congela |
| M-05 | no activado | `XlsxWriter.Build` conserva su contrato; los seis sistemas consumen la misma salida y se regresionan |
| **M-06** | **activado** | añade al escritor una capacidad reutilizable de Table y PivotTable; la especificacion la nombra como «new reusable Excel infrastructure» |
| M-07 | no activado | sin registro ni mecanismo transversal; una capa, un consumidor |
| M-08 | no activado en la opcion A; **activado en B y C2** | ADR-0012 admite la capacidad sobre la plataforma «cuando el costo sea aceptado»; un paquete exige acuerdo y ADR |

Arquetipo propuesto: **FOUNDATION EVOLUTION** por M-04, M-06 y por la regla de escalado de la especificacion («manipular OOXML directamente»). Ante duda, el superior (LIFECYCLE §3).

## 3. Expansiones

| EXP | Evaluacion |
|---|---|
| EXP-01 | Negativo: FOUNDATIONS, ADR-0012 y codigo coinciden. |
| EXP-02 | Negativo como autoridad. La diferencia entre el «Nivel» que supone la especificacion y el del producto es decision de alcance (D-1), no expansion. |
| EXP-03 | Negativo: sin legacy. |
| **EXP-04** | Activado: la salida la consumen seis sistemas. Cerrado con DC-05 y la matriz por sistema. |
| **EXP-05** | Activado: `ConsolidatedBomXlsxExporter` sin prueba directa. Cerrado con las obligaciones de la propuesta. |
| **EXP-06** | Activado: el escritor puede generar sin error un archivo que Excel no abre. Cerrado con §4 (P-07, P-09, P-10) e I-11, I-12, I-18. |
| EXP-07 | Negativo: DC-07. |
| **EXP-08** | Activado: con Nivel en «(Todas)» la suma por fila mezcla componente y pieza iguales. Defecto de lectura preexistente que la tabla dinamica haria visible. Cerrado como D-3. Ademas, `Total del dibujo` une por categoria, longitud y receta, ignorando Perfil y Descripcion (`ConsolidatedBom.cs:62-64`), diferencia que el resumen hara visible; registrada en Proposal §7 y OV-7, fijada por I-05. |
| EXP-09 | Activado para M-06 (crear/modificar). Se resuelve hacia el arquetipo superior. |

Omision revisada por el Coordinator: ninguna expansion adicional. Verificacion puntual del barrido delegado (subagente de solo lectura, perfil CHARACTERIZATION) contra el codigo: `BillOfMaterials.cs:8-32`, `SystemBomBuilder.cs:398-406`, `SelectiveBomBuilder.cs:243-256`.

## 4. Pruebas en Excel real

Excel 16.0 build 20527, interfaz es-ES, COM, instancia oculta propia, solo lectura, nada guardado. Datos sinteticos de 2 a 12 filas. Scripts en [I-68-probes/](I-68-probes/).

| ID | Pregunta | Resultado |
|---|---|---|
| P-01 | ¿Table + PivotTable con la forma de `XlsxWriter`? | Abre sin reparacion; ListObject y PivotTable reconocidos; fuente = nombre de la tabla |
| P-02 | Filtros Rack y Nivel, filas Descripcion → Longitud, suma | Campos y xlSum confirmados. Total 142 = esperado; Rack R-A 78; R-A + Pieza 72 |
| P-03 | ¿Se actualiza sola al abrir? | Si: celdas previas falsas (×1000) corregidas al abrir |
| P-04 | ¿Visible en Vista protegida? | Con celdas precalculadas, si (A1:B16 = esperado). Sin ellas, vacia |
| P-05 | ¿Enlazada a la Table? | Medido con columna **calculada** y definicion con listas: fila nueva amplia la Table; tras RefreshAll 142 → 276. No es el diseño recomendado; lo sustituye P-13 |
| P-06 | Cantidad total como valor | Editar el valor y RefreshAll: 64 → 134 |
| P-07 | BOM vacio | Tabla con solo encabezado: **Excel no abre el archivo**. Tabla con una fila en blanco: abre, con y sin dinamica |
| P-08 | Partida sin Nivel ni Descripcion | Aparece como item vacio, se cuenta (10) y entra al total (74) |
| P-09 | ¿Es fragil listar valores en la definicion? | **Si. Excel no abre el archivo** si Longitud es toda entera y falta `containsInteger`, si una fila tiene una variante de mayusculas listada aparte (t3) y si Rack la tiene (t5) |
| P-10 | Definicion minima sin listas de valores | Evita las tres formas de P-09: abre, Excel reconstruye, ordena segun su idioma y funde mayusculas (Poste 9). Celdas sobrantes desaparecen. Longitudes decimales separadas. Vista protegida muestra las celdas previas |
| P-11 | ¿Refresca en cada forma de abrir? | Si en 5 escenarios de instancia limpia: nueva, con libro en blanco, tras cerrar otro, con otra dinamica abierta, lectura-escritura |
| P-12 | Limitacion observada | En un bucle encadenado de automatizacion dos libros no refrescaron; en instancia limpia si. Causa UNKNOWN; no se reprodujo al intentarlo (P-14) |
| **P-13** | **Diseño recomendado** (minima + valor), longitudes 9, 10, 100, 144 y 40.5 | Sin reparacion. Campos y xlSum. Total 120 = SUM de la columna. R-A 66; Pieza 96; Componente 24; R-A + Pieza 42; varios racks 90. Misma Descripcion y Longitud suman 22; orden 9, 10, 100, 144. Editar Cantidad total: 120 → 190. Editar solo Copias o Cantidad: no cambia. Fila nueva: Cantidad total vacia; con valor, 193; Table A1:J11; fuente sigue siendo la tabla |
| **P-14** | Vista protegida, habilitar edicion y falta de refresco | Vista protegida muestra las celdas. Tras habilitar edicion: refresca, 120. Excel ya abierto y otro libro con objetos COM vivos: refresco. Con `refreshOnLoad=0`: celdas visibles, filtros sin valores; Actualizar recupera 120 |
| **P-15** | Estado inicial de Nivel y nota | «(Todas)»: Larguero 48, total 90. Pieza inicial (solo Nivel lista sus literales): Larguero 24, total 66, abre sin reparacion; con BOM vacio tambien. La nota en D1 se conserva al actualizar |

Salidas crudas de P-09..P-15 en `I-68-probes/results-*.txt`. P-01..P-08 anteriores solo tienen el resultado aqui registrado.

Limites: una sola version de Excel, una maquina, datos sinteticos; CI no tiene Excel. No se midio Excel 2016/2019 ni otras interfaces. La deteccion de reparacion usa titulo y registros `error*.xml`, con alertas desactivadas.

Rendimiento: sin medir, se caracteriza en G3 (BOM tipico y grande, generacion y apertura).

## 5. Biblioteca y opciones

**Biblioteca actual:** no hay biblioteca. `XlsxWriter` (`src/RackCad.Application/Bom/XlsxWriter.cs`, blob `9dfcd80eebd2f568daeb06a719ad5b6fbfc001a4`, codigo propio sin version de paquete ni licencia de tercero) escribe OOXML a mano con `System.IO.Compression`. Produce el archivo sin Excel instalado.

| Capacidad | En `XlsxWriter` hoy | Viable extendiendolo (medido) |
|---|---|---|
| Excel Table | **no** | si (P-01) |
| PivotTable nativa, cache | **no** | si (P-01, P-02, P-10) |
| Filtros, datos numericos | **no** | si (P-02) |
| Refresh | **no** | si (P-03, P-05, P-11) |

La especificacion ordena detenerse antes de «editar OOXML manualmente» y presentar opciones.

| Criterio | **A. Extender `XlsxWriter`** | B. ClosedXML | C2. Open XML SDK | D. Excel COM/Interop |
|---|---|---|---|---|
| Licencia | ninguna nueva | MIT (INFERENCE) | MIT (INFERENCE) | exige Office licenciado (INFERENCE) |
| Runtime / distribucion | ninguna; sin DLL nuevas | varias DLL nuevas | 1-2 DLL | Excel en cada equipo |
| AutoCAD | sin riesgo | **riesgo** (INFERENCE sobre la carga): la carpeta de AutoCAD 2025 trae ClosedXML 0.102.3 y OpenXml 2.16 y le faltan ExcelNumberFormat, XLParser e Irony (MEASURED) | **riesgo** (INFERENCE): AutoCAD trae 2.16; 3.x es salto mayor | proceso externo |
| .NET 8 | si | si | si | COM |
| ADR-0012 | conforme si se acepta el costo | excepcion + ADR (M-08) | excepcion + ADR (M-08) | excepcion de arquitectura |
| PivotTable nativa | **medido en Excel real** | no medido; requiere descarga | no medido; autoria igual de manual, con tipos | nativa |
| Visible en Vista protegida | medido (celdas previas) | no medido | manual, igual que A | si |
| Determinismo | alcanzable, se prueba en I-15 | no medido | alcanzable | no |
| Pruebas | sin Excel + validador + humo local | API propia | validador incluido | exige Excel en pruebas |
| Mantenimiento | RackCad posee el XML de la dinamica, definicion minima y casi estatica (tamaño: INFERENCE) | menos XML; hereda limites y versiones | XML tipado | fragil |

**Recomendacion: A**, con definicion minima y Cantidad total como valor (P-13..P-15). Es la unica via medida de extremo a extremo; B y C2 no se midieron porque exigen descargar paquetes y una excepcion a ADR-0012. Costo: RackCad posee ese XML y solo una prueba local en Excel real lo valida del todo.

## 6. Revision del Coordinator

Discovery DC-01..09 completo. EXP revisadas, incluidos negativos. Sin clase A abierta. Agrupacion: iniciativa y unidad unicas. Arquetipo propuesto FOUNDATION EVOLUTION. Tras la ronda 1 del Architect se corrigieron DC-01, DC-05, DC-08, DC-09 y EXP-08, y se midio el diseño recomendado. El STOP de la especificacion aplica: la decision D-0 es del Owner.

## 7. Metricas

| Campo | Valor |
|---|---|
| Arquetipo inicial / propuesto | EXTENSION / FOUNDATION EVOLUTION (M-04, M-06) |
| Rondas Discovery | 1 |
| EXP activadas | 04, 05, 06, 08, 09 |
| Rondas Architect | 2, en decisiones §6 |
| Candidato | no existe |
