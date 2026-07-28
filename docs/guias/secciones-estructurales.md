# Secciones estructurales (catálogo neutral)

Esta guía explica el catálogo **neutral** de secciones estructurales que introdujo la iniciativa
**I-36A**: qué es, de dónde salen sus datos, cómo se regenera y qué se puede editar a mano.

Decisiones que la gobiernan:
[ADR-0020](../adr/0020-catalogo-neutral-de-secciones-estructurales.md) (autoridad neutral, reemplaza a
ADR-0008 solo en autoridad conceptual) y
[ADR-0021](../adr/0021-identidad-unidades-y-presentacion-de-secciones.md) (identidad, unidades y
presentación; **no** reemplaza a [ADR-0005](../adr/0005-estrategia-de-unidades.md)).

---

## 1. Una sección no es un miembro

Es la distinción que justifica todo lo demás.

- Una **sección estructural** describe una **sección transversal**: qué forma tiene el material.
  Peralte, ancho, espesor, área, inercias, peso por unidad de longitud.
- Un **miembro** es el uso que un sistema hace de una sección: un poste, un larguero, una celosía, un
  separador, un brazo de cantilever, una columna.
- Una **pieza comercial** es lo que se compra: un número de parte, un fabricante, un costo.

`assets/catalogs/secciones.csv` —el catálogo **legado**— mezcla las tres cosas en una fila, con su
columna `rol` (`POSTE`, `CELOSIA`, `LARGUERO`, `SEPARADOR`). Funcionó mientras RackCad describía
perfiles propios de rack, donde cada fila era a la vez las tres cosas.

Cantilever rompe esa coincidencia: se arma con perfil **estándar** y la misma `W12X26` puede ser
columna, brazo o base. Por eso el catálogo neutral **no tiene rol de miembro**: los roles no aparecen
en su esquema ni en sus tipos. Lo que la sección no dice —troqueles, conectores, ménsulas,
perforaciones, soldaduras, placas terminales, reglas de fabricación— lo aportarán los
**configuradores de miembro** de las iniciativas siguientes.

> **`secciones.csv` no se migra todavía.** Sigue siendo la fuente vigente de todos los sistemas vigentes
> actuales, sin un solo cambio funcional, hasta que migraciones futuras —una por configurador, en modo
> strangler— la retiren. I-36A no migró, no borró y no modificó ninguna fila.

---

## 2. La fuente: AISC Shapes Database v16.0

| Dato | Valor |
|---|---|
| `SourceId` | `AISC-SHAPES` |
| `SourceRevision` | `16.0` |
| Publicador | American Institute of Steel Construction |
| Tipo | base de datos técnica oficial |
| Unidades nativas | `US_CUSTOMARY` (pulgadas, in.², lb/ft) |
| Página oficial | <https://www.aisc.org/aisc/publications/steel-construction-manual/aisc-shapes-database-v160/> |
| Archivo | `aisc-shapes-database-v16.0.xlsx`, 2 028 540 bytes |
| SHA-256 | `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496` |
| Hoja de datos | `Database v16.0` (la otra hoja, `Readme`, es el glosario de variables) |

**Dónde conseguirlo.** Desde la página oficial de arriba, con el botón «DOWNLOAD SHAPES DATABASE
V16.0». Hoy ese botón apunta a `https://cloud.aisc.org/biggie_bin/aisc-shapes-database-v160-2.xlsx`
(dominio del propio AISC). La ruta `www.aisc.org/globalassets/…` que circula en documentación antigua
**ya no existe**. El segundo enlace de esa misma página, `…v160h.xlsx`, es la **Historic Shapes
Database v16.0H** y **no** es la fuente de este catálogo. La base suplementaria **ASTM A1085** tampoco
se usa.

**Dónde ponerlo.** En `tools/sources/` (ruta ignorada por Git). El libro **no se versiona**: es una
fuente externa de ~2 MB y su huella ya vive en el manifiesto. RackCad **nunca** lo descarga en tiempo
de ejecución ni de compilación.

### Familias importadas

| Familia | Token | Tipo AISC | Secciones |
|---|---|---|---|
| Perfiles de ala ancha | `W` | `W` | 289 |
| HSS rectangular y cuadrado | `HSS-RECT` | `HSS` con paredes | 525 (126 cuadrados) |
| Canales americanos | `C` | `C` | 32 |
| Ángulos simples | `L` | `L` | 137 |
| **Total** | | | **983** |

### Tipos deliberadamente NO importados

`2L` (639), `WT` (289), **HSS redondo** (189), `PIPE` (51), `MC` (40), `S` (28), `ST` (28), `HP` (22),
`M` (16), `MT` (14). Total **1 316**. Se **reportan** por tipo; no son un error.

`2L` merece una nota: una fila de doble ángulo describe un **ensamble** de dos ángulos a una
separación dada, que es una decisión de miembro, no una sección transversal.

**El HSS se separa por los campos oficiales, no por el texto de la designación.** El Readme define
`OD` como el diámetro exterior del HSS redondo y `Ht`/`B`/`h`/`b` como las paredes del cuadrado o
rectangular. Con `OD` y sin paredes: redondo, excluido. Con paredes y sin `OD`: importado. Sobre v16.0
la partición es exhaustiva y limpia (525 + 189 = 714). Una fila que no cumpla exactamente una de las
dos formas es **ambigua** y detiene la importación.

---

## 3. Los archivos

Todos viven **directamente** bajo `assets/catalogs/`, junto a los catálogos vigentes, para reutilizar
sin cambios el copiado a la carpeta `catalogs/` del plugin y el empaquetado del bundle.

| Archivo | Qué es | ¿Editable a mano? |
|---|---|---|
| `structural-sections-w.csv` | 289 secciones W | **No** — salida generada |
| `structural-sections-hss-rect.csv` | 525 HSS rectangulares y cuadrados | **No** — salida generada |
| `structural-sections-c.csv` | 32 canales C | **No** — salida generada |
| `structural-sections-l.csv` | 137 ángulos L | **No** — salida generada |
| `structural-section-sources.csv` | La fuente lógica y su procedencia | **No** — salida generada |
| `structural-section-status.csv` | Overlay de habilitado/deshabilitado | **SÍ**, es el único |
| `structural-sections-manifest.json` | Conteos y SHA-256 de los archivos **inmutables** | **No** — salida generada |

### Datos reproducibles frente a overlay mutable

Es la separación que gobierna todo lo demás:

- Los **seis primeros** archivos son **datos reproducibles**: función del libro y de nada más. El
  manifiesto declara el SHA-256 de los **cinco** que son datos (los cuatro de familia más las
  fuentes); no se hashea a sí mismo, porque sería circular.
- `structural-section-status.csv` es un **overlay local**: una decisión del operador, no un dato de la
  fuente. **NO entra en los hashes.** Si entrara, deshabilitar una sección —una edición perfectamente
  legítima— parecería corrupción de los datos AISC.
- El overlay **sí** se valida, con su propio esquema, sin duplicados y comprobando que cada `sectionId`
  exista. Simplemente se valida **aparte**.
- El importador **nunca reescribe** un overlay existente. Sólo lo **siembra vacío** cuando la carpeta
  todavía no tiene uno.

`--check` refleja esa separación: informa por un lado si los **datos generados** siguen siendo
exactamente los que produce el libro, y por otro si el **overlay local** es válido.

Los siete están marcados como `-text` en `.gitattributes`. El repositorio trabaja con
`core.autocrlf=true`, así que sin esa regla Git convertiría los saltos de línea al hacer checkout y el
SHA-256 del archivo **en disco** dejaría de coincidir con el que declara el manifiesto en un clon
nuevo.

> No edites los CSV generados: la próxima reimportación los sobrescribe y el manifiesto dejará de
> cuadrar. Si necesitas cambiar un dato, la pregunta correcta es qué dice la fuente.

---

## 4. Identidad: cómo se forma un id

```
{ID_NAMESPACE}-{FAMILIA}-{EDI_NORMALIZADO}
```

- `ID_NAMESPACE` es la **autoridad** que nombra la sección; la declara la fuente en su columna
  `idNamespace`. `AISC-SHAPES` declara **`AISC`**, así que los 983 identificadores son exactamente los
  de siempre. No es una constante del código: si mañana entrara otra fuente, declararía la suya y sus
  secciones no colisionarían con las de AISC aunque publicaran la misma designación. Debe ser no
  vacío, de `A-Z0-9` (el guion queda fuera: es el separador del propio id) y único en el catálogo.
- `FAMILIA` es el token: `W`, `HSS-RECT`, `C` o `L`.
- `EDI_NORMALIZADO` sale de la **designación EDI oficial** —la del *AISC Naming Convention for
  Structural Steel Products for Use in Electronic Data Interchange*— por una normalización
  determinista: **mayúsculas invariantes**, se eliminan los espacios y `.`, `/` y `-` pasan a `_`.

El resultado es ASCII, en mayúsculas, sin espacios, sin barra y sin punto decimal.

| Etiqueta del manual | Designación EDI | Id |
|---|---|---|
| `W12X26` | `W12X26` | `AISC-W-W12X26` |
| `C10X15.3` | `C10X15.3` | `AISC-C-C10X15_3` |
| `L4X4X1/4` | `L4X4X1/4` | `AISC-L-L4X4X1_4` |
| `HSS4X4X1/4` | `HSS4X4X.250` | `AISC-HSS-RECT-HSS4X4X_250` |

**La última fila es la que sorprende y es deliberada.** Para las 525 HSS rectangulares el EDI escribe
el espesor de pared en **decimal** y el manual en **fracción** (513 de las 983 filas difieren). El id
sigue al **EDI en las cuatro familias sin excepción**, porque una regla que cambie de fuente según la
familia deja de ser determinista. La etiqueta del manual se conserva íntegra, es única y es **con la
que se busca y se muestra**: `TryGetByDesignation("HSS4X4X1/4")` resuelve.

Reglas que el validador comprueba:

- una **colisión de normalización es un error fatal**, nunca una desambiguación automática (sobre
  v16.0 no existe ninguna: 983 designaciones producen 983 ids);
- el id **no contiene la revisión** de la fuente: `SourceRevision` vive en su propio campo, así que una
  sección que sobreviva a la v17 conserva su id;
- el id **no contiene** el grado de material ni ninguna magnitud;
- `DisplayName` **nunca** es una clave;
- nada deriva dimensiones ni peso **interpretando el texto** de una designación.

---

## 5. Unidades

La **pulgada sigue siendo la unidad geométrica interna de RackCad** (ADR-0005, intacto). Este catálogo
no altera `INSUNITS`, no convierte el DWG y no reescala nada.

- El bloque **estadounidense** de la fuente es el valor **nativo y canónico**: longitudes en pulgadas,
  área en in.², peso lineal en `lb/ft`. `NativeUnitSystem = US_CUSTOMARY`.
- Las columnas **métricas** oficiales se usan como **contraste**, nunca como filas duplicadas ni como
  sustituto. El importador compara cada magnitud contra su espejo y **detiene la importación** si se
  sale de tolerancia: 1 % para geometría y área (desviación real máxima medida: **0.461 %**) y 5 % para
  el peso nominal (**4.128 %**), porque el peso métrico de la fuente es un **valor nominal de
  designación redondeado aparte**, no una conversión — `C5X6.7` se publica como `10.4 kg/m` cuando la
  conversión exacta da `9.97`.
- La equivalencia se **calcula** con factores exactos: `0.45359237 / 0.3048` para `lb/ft → kg/m`, y
  `25.4` y `645.16` para longitud y área.
- `tnom` (espesor **nominal** de pared) y `tdes` (espesor de **diseño**) son datos **distintos** y ambos
  se conservan. **La geometría futura (I-36B) usará el nominal.**

### Cómo se muestra el peso

Primero la unidad **nativa** —la que designa y compra el usuario— y después la equivalente:

```
W12X26 — 26 lb/ft (38.7 kg/m)
```

El número sale **siempre** de `WeightPerLength`. Leer el «26» dentro de `W12X26` funcionaría para una W
y sería un disparate para un HSS.

`StructuralSectionLabelFormatter` es puro: sin WPF, sin AutoCAD, en cultura invariante.

---

## 6. Habilitar y deshabilitar secciones

`structural-section-status.csv` es un **overlay de excepciones**:

```csv
sectionId,isEnabled,notes
AISC-W-W12X26,false,sin existencias con el proveedor habitual
```

| Columna | Obligatoria | Qué hace |
|---|---|---|
| `sectionId` | sí | El id exacto de la sección |
| `isEnabled` | sí | `true` o `false`, literal y en minúsculas |
| `notes` | no | Motivo, para quien lea el archivo dentro de un año |

Reglas:

- **toda sección está habilitada por defecto**; el overlay solo contiene las excepciones, y hoy está
  vacío;
- un `sectionId` **desconocido** es error, y uno **duplicado** también: un overlay que apuntara a una
  sección inexistente parecería funcionar y no deshabilitaría nada;
- **deshabilitar no elimina**: `GetById` sigue resolviendo la sección, así que un diseño guardado antes
  de retirarla se sigue abriendo;
- las consultas de **selección nueva** (`ByFamily`, `Enabled`, `Search`) la excluyen salvo que se pida
  explícitamente lo contrario.

**Es el único override que el catálogo neutral admite.** No hay dimensión, peso ni designación
sobrescribibles: eso bifurcaría la fuente en silencio.

Es el único archivo del conjunto que se puede editar a mano, y sobrevive a una reimportación: el
importador conserva las entradas cuyas secciones sigan existiendo y **avisa** de las que no.

---

## 7. Reimportar (actualizar a una revisión futura)

```bash
dotnet run --project tools/RackCad.StructuralSections.Import -- --workbook tools/sources/aisc-shapes-database-v16.0.xlsx --output assets/catalogs
```

Para **comprobar sin escribir** que el catálogo distribuido sigue siendo exactamente el que produce ese
libro:

```bash
dotnet run --project tools/RackCad.StructuralSections.Import -- --workbook tools/sources/aisc-shapes-database-v16.0.xlsx --output assets/catalogs --check
```

La herramienta vive **fuera del producto** (`tools/`), es .NET 8 con **BCL pura** —cero NuGet, cero
Office Interop, sin Excel instalado— y **no entra al bundle**. Lee el OOXML como ZIP + XML y resuelve
la hoja y las columnas **desde el propio libro**, no por posiciones supuestas.

### Antes de importar, comprueba que el libro ES el que dice ser

No basta con que los encabezados encajen: un fork, una copia editada, otra revisión o la exportación
de otro proveedor tendrían encabezados compatibles y quedarían catalogados como la fuente oficial. La
verificación exige, todo publicado por el propio documento:

1. que exista y se pueda leer la hoja **`Readme`**;
2. que ese Readme declare **«AISC Shapes Database v16.0»**;
3. que mencione la **«16th Edition»** del Steel Construction Manual y la convención **EDI**;
4. que exista la hoja de datos que esa revisión implica, **`Database v16.0`**;
5. que revisión, hoja de datos y metadata generada cuenten **la misma historia**.

Por eso la CLI **no tiene** `--worksheet`: la hoja sale de lo que el libro acredita, nunca de un
argumento, y no hay forma de etiquetar otra hoja o revisión como v16.0.

La identidad se verifica por **contenido y estructura**, no por el SHA-256: fijar el hash actual como
único libro admisible haría imposible importar cualquier revisión futura legítima. El hash se sigue
**registrando** en el manifiesto como procedencia; no es la compuerta.

### Publicación: qué garantiza exactamente

Si algo falla durante la publicación, **todo archivo ya reemplazado se restaura byte por byte** desde
un respaldo, los que no existían antes se eliminan, y las carpetas de trabajo (staging y respaldo) se
borran: el directorio queda exactamente como estaba.

Lo que **no** se promete, porque no se puede demostrar con una prueba: atomicidad frente a un corte de
energía o a un proceso liquidado. Contra ese escenario protege otro mecanismo —el **manifiesto se
publica el último**, así que una publicación interrumpida deja datos nuevos junto a un manifiesto
viejo, y la carga validada se niega a abrir esa carpeta.

Su salida es **determinista**: encabezados en orden de esquema, filas ordenadas por `sectionId` con
comparación ordinal, números en cultura invariante con precisión de ida y vuelta, terminador `\n`,
UTF-8 sin BOM y **cero marcas de tiempo**. Dos ejecuciones sobre el mismo libro producen archivos
**byte-idénticos**.

Al actualizar a una revisión futura, lo que hay que revisar es:

1. que el importador **no** se detenga (si lo hace, dirá exactamente por qué: encabezado renombrado,
   fila ambigua, valor ilegible, colisión de ids o discrepancia US/métrico);
2. los **conteos por familia** que imprime;
3. que **ninguna entrada del overlay quede huérfana**: si el overlay nombra una sección que el libro
   nuevo ya no produce, la importación **se detiene con error**. Retirar esa decisión es del operador,
   y la forma de hacerlo es editar el overlay primero. Un aviso habría dejado que la decisión se
   evaporara;
4. que `dotnet test` siga en verde — las pruebas del catálogo distribuido fijan conteos y sentinelas,
   así que un cambio real de la fuente se verá ahí y hay que actualizarlas **conscientemente**.

---

## 7.b Cargar el catálogo: una sola puerta, y falla cerrada

`CsvStructuralSectionCatalogProvider.Load()` es la **única** forma pública de obtener el catálogo, y
**valida antes de entregarlo**. No existe una vía pública para obtener uno sin validar.

Comprueba, en una pasada: las invariantes semánticas de cada sección; el manifiesto completo
(`catalogId`, `sourceId`, `sourceRevision`, `sourceWorksheet`, `mapperVersion`, `idNamespace` y un
SHA-256 del libro con exactamente 64 hexadecimales); el **conjunto exacto** de archivos declarados
—ninguno faltante, ninguno inesperado, ninguno repetido, y nunca el propio manifiesto ni el overlay—;
el SHA-256 de cada archivo inmutable; y la correspondencia **fuente ↔ filas ↔ manifiesto**. El overlay
se valida aparte.

Si algo falla lanza `StructuralSectionCatalogException` con el diagnóstico completo. Es deliberado:
una carpeta que sólo *parsea* no es un catálogo. Puede reemplazarse en una instalación desplegada, y
una publicación interrumpida deja CSV nuevos junto a un manifiesto viejo — archivos individualmente
correctos y colectivamente mentira.

## 8. Por qué no hay un bloque por designación

`blocks.csv` relaciona **pieza + vista → bloque de AutoCAD**, y `blockName` debe coincidir exacto con
la biblioteca DWG del dueño. Crear 983 filas ahí exigiría 983 bloques dibujados a mano.

No hace falta y sería el diseño equivocado. Un perfil estándar **no necesita un bloque por
designación**: su contorno es *paramétrico*, derivable de las dimensiones que este catálogo ya guarda.
Construir esa geometría —contornos, radios y filetes, centroide como origen, vistas transversales y
longitudinales, longitud arbitraria, orientación, proyección y las definiciones internas de AutoCAD
que se deriven— es exactamente el alcance de **I-36B**.

Por eso I-36A **no toca** `blocks.csv`, **no modifica** `blocks-library.dwg` y **no hace** que
`CatalogBlockManifest` espere un bloque por sección.

---

## 9. Qué queda para I-36B y para después

**I-36B — geometría y representación prismática.** Contornos detallados por familia, radios y filetes,
el centroide como origen documentado, vistas transversales y longitudinales, longitud arbitraria,
orientación, proyección y definiciones AutoCAD internas derivadas. Podrá **derivar** una geometría
paramétrica documentada a partir de los valores tabulados; lo que no podrá es inventar un radio que la
fuente no publique. I-36A **solo conserva los datos y su fidelidad**.

**I-37 — Cantilever MVP.** El primer sistema sobre perfil estándar, y donde nacen los configuradores de
miembro.

**I-38 — ingeniería estructural de Cantilever.** No reabre
[ADR-0017](../adr/0017-validacion-cargas-diferida-ram-elements.md) sin un ADR nuevo.

---

## 10. Dónde mirar en el código

| Tema | Ruta |
|---|---|
| Modelo neutral completo | `src/RackCad.Application/StructuralSections/` |
| Identidad y normalización | `StructuralSectionId.cs`, `StructuralSectionDesignationNormalizer.cs` |
| Dimensiones por familia | `WSectionDimensions.cs`, `HssRectangularSectionDimensions.cs`, `ChannelSectionDimensions.cs`, `AngleSectionDimensions.cs` |
| Esquema de columnas (una autoridad) | `StructuralSectionCsvSchema.cs` |
| Lector CSV estricto | `StrictCsvTable.cs`, `StructuralSectionCsvException.cs` |
| Proveedor y caché | `CsvStructuralSectionCatalogProvider.cs` |
| Búsquedas | `StructuralSectionCatalog.cs` |
| Validador | `StructuralSectionCatalogValidator.cs` |
| Unidades y peso | `StructuralSectionUnits.cs` |
| Etiqueta de peso | `StructuralSectionLabelFormatter.cs` |
| Importador | `tools/RackCad.StructuralSections.Import/` |
| Parser léxico CSV compartido | `src/RackCad.Application/Catalogs/CsvLexer.cs` |

`CsvCatalogReader` **no cambió de comportamiento**: solo delega su parser léxico en `CsvLexer`, y
`CsvLexerTests` fija las rarezas históricas para que esa delegación no pueda regresar en silencio.

---

## 11. Matriz de columnas de la fuente

La hoja `Database v16.0` tiene 166 columnas repartidas en **cuatro tramos**:

| Tramo | Columnas | Qué es |
|---|---|---|
| 1 | `A`–`D` | **Metadata e identidad** del bloque estadounidense: `Type`, designación EDI, etiqueta del manual y el indicador `T_F` |
| 2 | `E`–`CF` | **Valores estadounidenses** (in., in.², lb/ft…) |
| 3 | `CG`–`CH` | **Designaciones métricas**: EDI y etiqueta del manual en su forma métrica |
| 4 | `CI`–`FJ` | **Valores métricos** (mm, mm², kg/m…), espejo del tramo 2 |

La tabla siguiente recorre las **84 columnas** de los tramos 1 y 2 —las que se importan— con su
espejo métrico, lo que significa cada una según el Readme, en qué familias importadas aparece, a qué
campo de RackCad va y —si no se importa— por qué.

Notación de familias: `W`, `H` = HSS rectangular/cuadrado, `C`, `L`. «—» = no aparece en ninguna
familia importada. Tres nombres se reescriben porque un encabezado CSV no puede llevarlos: `twdet/2` →
`twdet_2`, `tan(α)` → `tanAlpha`, y la `W` de peso → `weightPerLength` (colisionaría con el token de la
familia W).

| AISC | Métrica | Significado (Readme) | Unidad US | Unidad métrica | Familias | Campo RackCad | Estado |
|---|---|---|---|---|---|---|---|
| `Type` (A) | — | Tipo de forma | — | — | todas | *(clasificación)* | **Importada** como familia |
| `EDI_Std_Nomenclature` (B) | CG | Designación EDI | — | — | todas | `Identity.EdiDesignation` | **Importada** (base del id) |
| `AISC_Manual_Label` (C) | CH | Designación del Manual | — | — | todas | `Identity.ManualLabel` | **Importada** (nombre visible) |
| `T_F` (D) | — | Booleano de nota especial (`tf > 2 in.` en W) | — | — | W | `SourceSpecialNote` | **Importada** |
| `W` (E) | CI | Peso nominal | lb/ft | kg/m | todas | `WeightPerLength` | **Importada** |
| `A` (F) | CJ | Área de la sección | in.² | mm² | todas | `Properties.Area` | **Importada** |
| `d` (G) | CK | Peralte total; ala **corta** en ángulos | in. | mm | W C L | `Depth` / `ShortLeg` | **Importada** |
| `ddet` (H) | CL | Valor de detallado del peralte | in. | mm | W C | `DetailingDepth` | **Importada** |
| `Ht` (I) | CM | Peralte total del HSS (pared larga) | in. | mm | H | `OverallDepth` | **Importada** |
| `h` (J) | CN | Pared **plana** larga del HSS | in. | mm | H | `FlatDepth` | **Importada** |
| `OD` (K) | CO | Diámetro exterior de HSS redondo o PIPE | in. | mm | — | — | **Omitida**: solo la usa la clasificación; ninguna familia importada la tabula |
| `bf` (L) | CP | Ancho del patín | in. | mm | W C | `FlangeWidth` | **Importada** |
| `bfdet` (M) | CQ | Valor de detallado del ancho de patín | in. | mm | W C | `DetailingFlangeWidth` | **Importada** |
| `B` (N) | CR | Ancho total del HSS (pared corta) | in. | mm | H | `OverallWidth` | **Importada** |
| `b` (O) | CS | Pared plana corta del HSS; ala **larga** en ángulos | in. | mm | H L | `FlatWidth` / `LongLeg` | **Importada** |
| `ID` (P) | CT | Diámetro interior de PIPE | in. | mm | — | — | **Omitida**: exclusiva de PIPE, tipo no importado |
| `tw` (Q) | CU | Espesor del alma | in. | mm | W C | `WebThickness` | **Importada** |
| `twdet` (R) | CV | Valor de detallado del espesor de alma | in. | mm | W C | `DetailingWebThickness` | **Importada** |
| `twdet/2` (S) | CW | Valor de detallado de `tw/2` | in. | mm | W C | `HalfDetailingWebThickness` (`twdet_2`) | **Importada** |
| `tf` (T) | CX | Espesor del patín | in. | mm | W C | `FlangeThickness` | **Importada** |
| `tfdet` (U) | CY | Valor de detallado del espesor de patín | in. | mm | W C | `DetailingFlangeThickness` | **Importada** |
| `t` (V) | CZ | Espesor del ala del ángulo | in. | mm | L | `Thickness` | **Importada** |
| `tnom` (W) | DA | Espesor **nominal** de pared | in. | mm | H | `NominalThickness` | **Importada** |
| `tdes` (X) | DB | Espesor de **diseño** de pared | in. | mm | H | `DesignThickness` | **Importada** |
| `kdes` (Y) | DC | Cara exterior del patín a la punta del filete, diseño | in. | mm | W C L | `KDesign` | **Importada** |
| `kdet` (Z) | DD | Ídem, valor de detallado | in. | mm | W C L | `KDetailing` | **Importada** |
| `k1` (AA) | DE | Eje del alma a la punta del filete, detallado | in. | mm | W | `K1` | **Importada** |
| `x` (AB) | DF | Borde designado al **centro de gravedad**, horizontal | in. | mm | C L | `CentroidX` | **Importada** |
| `y` (AC) | DG | Ídem, vertical | in. | mm | L | `CentroidY` | **Importada** |
| `eo` (AD) | DH | Borde designado al **centro de cortante** | in. | mm | C | `ShearCenterX` | **Importada** |
| `xp` (AE) | DI | Borde designado al **eje neutro plástico**, horizontal | in. | mm | C L | `PlasticNeutralAxisX` | **Importada** |
| `yp` (AF) | DJ | Ídem, vertical | in. | mm | L | `PlasticNeutralAxisY` | **Importada** |
| `bf/2tf` (AG) | DK | Razón de esbeltez del patín | — | — | W | — | **Omitida**: cociente derivable de `bf` y `tf`; dominio de diseño diferido por ADR-0017 |
| `b/t` (AH) | DL | Razón de esbeltez de ángulo y patín de canal | — | — | C L | — | **Omitida**: ídem |
| `b/tdes` (AI) | DM | Razón de esbeltez de la pared corta del HSS | — | — | H | — | **Omitida**: ídem |
| `h/tw` (AJ) | DN | Razón de esbeltez del alma | — | — | W C | — | **Omitida**: ídem |
| `h/tdes` (AK) | DO | Razón de esbeltez de la pared larga del HSS | — | — | H | — | **Omitida**: ídem |
| `D/t` (AL) | DP | Razón de esbeltez de HSS redondo, PIPE o tees | — | — | — | — | **Omitida**: cociente y además de tipos no importados |
| `Ix` (AM) | DQ | Momento de inercia respecto de x | in.⁴ | ×10⁶ mm⁴ | todas | `Properties.Ix` | **Importada** |
| `Zx` (AN) | DR | Módulo plástico respecto de x | in.³ | ×10³ mm³ | todas | `Properties.Zx` | **Importada** |
| `Sx` (AO) | DS | Módulo elástico respecto de x | in.³ | ×10³ mm³ | todas | `Properties.Sx` | **Importada** |
| `rx` (AP) | DT | Radio de giro respecto de x | in. | mm | todas | `Properties.Rx` | **Importada** |
| `Iy` (AQ) | DU | Momento de inercia respecto de y | in.⁴ | ×10⁶ mm⁴ | todas | `Properties.Iy` | **Importada** |
| `Zy` (AR) | DV | Módulo plástico respecto de y | in.³ | ×10³ mm³ | todas | `Properties.Zy` | **Importada** |
| `Sy` (AS) | DW | Módulo elástico respecto de y | in.³ | ×10³ mm³ | todas | `Properties.Sy` | **Importada** |
| `ry` (AT) | DX | Radio de giro respecto de y | in. | mm | todas | `Properties.Ry` | **Importada** |
| `Iz` (AU) | DY | Momento de inercia respecto de z (principal menor) | in.⁴ | ×10⁶ mm⁴ | L | `Properties.Iz` | **Importada** |
| `rz` (AV) | DZ | Radio de giro respecto de z | in. | mm | L | `Properties.Rz` | **Importada** |
| `Sz` (AW) | EA | Módulo elástico respecto de z | in.³ | ×10³ mm³ | L | `Properties.Sz` | **Importada** |
| `J` (AX) | EB | Constante torsional | in.⁴ | ×10³ mm⁴ | todas | `Properties.J` | **Importada** |
| `Cw` (AY) | EC | Constante de alabeo | in.⁶ | ×10⁹ mm⁶ | W C L | `Properties.Cw` | **Importada** |
| `C` (AZ) | ED | Constante torsional del HSS | in.³ | ×10³ mm³ | H | `Properties.HssTorsionalConstant` | **Importada** |
| `Wno` (BA) | EE | Función de alabeo normalizada (DG 9) | in.² | mm² | W C | `Properties.Wno` | **Importada** |
| `Sw1` (BB) | EF | Momento estático de alabeo en el punto 1 | in.⁴ | ×10⁶ mm⁴ | W C | `Properties.Sw1` | **Importada** |
| `Sw2` (BC) | EG | Ídem en el punto 2 | in.⁴ | ×10⁶ mm⁴ | C | `Properties.Sw2` | **Importada** |
| `Sw3` (BD) | EH | Ídem en el punto 3 | in.⁴ | ×10⁶ mm⁴ | C | `Properties.Sw3` | **Importada** |
| `Qf` (BE) | EI | Momento estático en el patín sobre el borde del alma | in.³ | ×10³ mm³ | W C | `Properties.Qf` | **Importada** |
| `Qw` (BF) | EJ | Momento estático a media altura | in.³ | ×10³ mm³ | W C | `Properties.Qw` | **Importada** |
| `ro` (BG) | EK | Radio de giro polar respecto del centro de cortante | in. | mm | C L | `Properties.Ro` | **Importada** |
| `H` (BH) | EL | Constante flexural | — | — | C, L (solo alas iguales) | `Properties.FlexuralConstantH` | **Importada** |
| `tan(α)` (BI) | EM | Tangente del ángulo entre los ejes y-y y z-z | — | — | L | `Properties.TanAlpha` (`tanAlpha`) | **Importada** |
| `Iw` (BJ) | EN | Momento de inercia respecto del eje w | in.⁴ | ×10⁶ mm⁴ | L | `Properties.Iw` | **Importada** |
| `zA` (BK) | EO | Punto A al centro de gravedad sobre z | in. | mm | L | `Properties.ZA` | **Importada** |
| `zB` (BL) | EP | Punto B al centro de gravedad sobre z | in. | mm | L | `Properties.ZB` | **Importada** (vale 0 en alas iguales: es real) |
| `zC` (BM) | EQ | Punto C al centro de gravedad sobre z | in. | mm | L | `Properties.ZC` | **Importada** |
| `wA` (BN) | ER | Punto A al centro de gravedad sobre w | in. | mm | L | `Properties.WA` | **Importada** |
| `wB` (BO) | ES | Punto B al centro de gravedad sobre w | in. | mm | L | `Properties.WB` | **Importada** |
| `wC` (BP) | ET | Punto C al centro de gravedad sobre w | in. | mm | L | `Properties.WC` | **Importada** |
| `SwA` (BQ) | EU | Módulo elástico respecto de w en el punto A | in.³ | ×10³ mm³ | L | `Properties.SwA` | **Importada** |
| `SwB` (BR) | EV | Ídem en el punto B | in.³ | ×10³ mm³ | L (76/137) | `Properties.SwB` | **Importada** |
| `SwC` (BS) | EW | Ídem en el punto C | in.³ | ×10³ mm³ | L | `Properties.SwC` | **Importada** |
| `SzA` (BT) | EX | Módulo elástico respecto de z en el punto A | in.³ | ×10³ mm³ | L | `Properties.SzA` | **Importada** |
| `SzB` (BU) | EY | Ídem en el punto B | in.³ | ×10³ mm³ | L | `Properties.SzB` | **Importada** |
| `SzC` (BV) | EZ | Ídem en el punto C | in.³ | ×10³ mm³ | L | `Properties.SzC` | **Importada** |
| `rts` (BW) | FA | Radio de giro efectivo | in. | mm | W C | `Properties.Rts` | **Importada** |
| `ho` (BX) | FB | Distancia entre los centroides de los patines | in. | mm | W C | `Properties.Ho` | **Importada** |
| `PA` (BY) | FC | Perímetro menos una superficie de patín (o de ala corta) | in. | mm | W C L | `Properties.PA` | **Importada** |
| `PA2` (BZ) | FD | Perímetro del ángulo menos la superficie del ala larga | in. | mm | L | `Properties.PA2` | **Importada** |
| `PB` (CA) | FE | Perímetro de la forma | in. | mm | W C L | `Properties.PB` | **Importada** |
| `PC` (CB) | FF | Perímetro de caja menos una superficie de patín | in. | mm | W C | `Properties.PC` | **Importada** |
| `PD` (CC) | FG | Perímetro de caja | in. | mm | W C | `Properties.PD` | **Importada** |
| `T` (CD) | FH | Distancia entre las puntas de filete del alma | in. | mm | W C | `DistanceBetweenFilletToes` | **Importada** |
| `WGi` (CE) | FI | Gramil útil de los barrenos interiores del patín | in. | mm | W, C (24/32) | `WorkableGageInner` | **Importada** |
| `WGo` (CF) | FJ | Separación entre barrenos interior y exterior | in. | mm | W (73/289) | `WorkableGageOuter` | **Importada** |

Los tramos 3 y 4 (`CG`–`FJ`) son el espejo métrico. **Ninguna de sus columnas se importa como dato**:
se usan como contraste en la importación (§5) y no generan filas. `SourceId`, `SourceRevision`,
`IdNamespace` y `NativeUnitSystem` son campos de RackCad, no columnas de la fuente.

Un valor que la fuente marca como no aplicable —una **raya EN DASH** `–`, no una celda vacía— se
importa como **`null`**, nunca como cero.
