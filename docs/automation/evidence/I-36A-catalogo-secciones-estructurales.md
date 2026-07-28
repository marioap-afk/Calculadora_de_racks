# Evidencia — I-36A, núcleo y catálogo de secciones estructurales

Todo lo de este documento es **reproducible**: cada número sale de un comando o de una celda del libro
oficial, y cada afirmación se puede comprobar sin confiar en este archivo.

| Campo | Valor |
|---|---|
| Iniciativa | I-36A — Núcleo y catálogo de secciones estructurales |
| Rama | `architecture/catalogo-secciones-estructurales` |
| Claim-Id | `c72f4d42-a201-40c1-b9d9-2f03d9556681` |
| Worktree | `.claude/worktrees/architecture-catalogo-secciones-estructurales` |
| Base | `a35374f99dde91077a2e05bfc69358fb3e3b3ad9` (`Merge I-23: namespaces finales por sistema`) |
| CI de la base | run **30307095708**, `completed / success`, sobre el SHA exacto de `origin/main` |

---

## 1. Preflight real

Ejecutado **antes** de escribir nada y antes del reclamo.

| Comprobación | Resultado |
|---|---|
| `git fetch origin --prune` | OK |
| `main` == `origin/main` | sí, `a35374f99dde91077a2e05bfc69358fb3e3b3ad9` en ambos |
| `git status` | limpio |
| Merge / rebase / cherry-pick / bisect en curso | ninguno |
| Worktrees | uno solo, el principal del dueño |
| Stashes | cero |
| Ramas remotas | **solo** `origin/main` |
| `architecture/catalogo-secciones-estructurales` | libre |
| `architecture/geometria-secciones-estructurales` | libre y **reservada** para I-36B (no se creó) |
| Otra iniciativa sobre `Catalogs`, `assets/catalogs`, `tests`, `.sln` o los índices documentales | ninguna |

Documentos leídos: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md`,
`docs/HANDOFF.md`, `docs/adr/README.md`, ADR-0005, ADR-0007, ADR-0008, ADR-0012,
`docs/guias/catalogos-y-plantillas.md`, `docs/guias/modelo-de-datos.md`,
`docs/initiatives/TEMPLATE.md`, `docs/initiatives/README.md`, los Context Packs
`architecture-kernel`, `catalogs-data`, `delivery-validation` y `documentation-governance`, los
contratos de I-19, I-26 e I-23, `src/RackCad.Application/Catalogs/` completo (incluido `Validation/`),
las pruebas de catálogo de `tests/RackCad.Tests` y las reglas de copiado/bundle de `assets/catalogs`
(los cuatro `.csproj` y `deploy/verify-bundle.ps1`).

`OWNER-DECISIONS.md` **no existe** en el repositorio; las decisiones del dueño de esta iniciativa
están versionadas en [`../decisions/I-36A.md`](../decisions/I-36A.md).

---

## 2. La fuente y su procedencia

| Campo | Valor |
|---|---|
| Nombre | AISC Shapes Database v16.0 |
| Publicador | American Institute of Steel Construction |
| Página oficial (la del encargo) | <https://www.aisc.org/aisc/publications/steel-construction-manual/aisc-shapes-database-v160/> — responde **200** |
| URL del encargo | `https://www.aisc.org/globalassets/product-files-not-searched/manuals/aisc-shapes-database-v16.0.xlsx` — **404** con `curl`, **403** con `Invoke-WebRequest` |
| URL utilizada | `https://cloud.aisc.org/biggie_bin/aisc-shapes-database-v160-2.xlsx` |
| Por qué esa | Es el destino del botón **«DOWNLOAD SHAPES DATABASE V16.0»** que publica la propia página oficial. Dominio del propio AISC: **mismo publicador**, no un mirror ni un tercero |
| Descarga | `HTTP 200`, `content-type application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` |
| Tamaño | **2 028 540** bytes |
| **SHA-256** | **`82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496`** |
| Ubicación | scratchpad de la sesión, **fuera** del repositorio. El libro **no se versiona** |
| No utilizado | `https://cloud.aisc.org/biggie_bin/aisc-shapes-database-v160h.xlsx` — es la **Historic Shapes Database v16.0H**, que la misma página publica aparte. La base suplementaria **ASTM A1085** tampoco se usó |

### Verificación del contenido, no solo del nombre

| Comprobación | Resultado |
|---|---|
| Partes OOXML | `xl/workbook.xml`, `xl/_rels/workbook.xml.rels`, `xl/sharedStrings.xml` (211 KB), `xl/worksheets/sheet1.xml` (55 KB) y `xl/worksheets/sheet2.xml` (**14.7 MB**) |
| Hojas declaradas | `Readme` y `Database v16.0` — **dos**, las que el encargo describe |
| Versión declarada | La hoja `Readme` dice literalmente «AISC Shapes Database v16.0», fecha **agosto 2023**, y «consistent with … AISC Steel Construction Manual, 16th Edition, 1st Printing» |
| Readme legible | Sí: se leyó completo, incluido el glosario de las 84 variables y la lista de formas nuevas de la v16.0 |
| Convención EDI | Documentada en el propio Readme: la columna B es «the shape designation according to the AISC Naming Convention for Structural Steel Products for Use in Electronic Data Interchange (EDI), June 25, 2001» |
| Fila de encabezados | La **1**, con **166** columnas |
| Bloque estadounidense | `A`–`CF` (índices 0–83) |
| Espejo métrico | `CG`–`FJ` (índices 84–165), empezando por su propio `EDI_Std_Nomenclature` |
| Marcador de «no aplica» | **EN DASH** `–` (U+2013), 250 455 apariciones; no es celda vacía |

---

## 3. Reclamo

| Campo | Valor |
|---|---|
| Rama creada desde | `origin/main` (`a35374f`) |
| Commit de reclamo | `c9d53d209f71ff2cf019e8cc121dbe0096f84a2b` |
| Primer push | **aceptado**, sin `--force` — el reclamo es válido |
| Trailers | `Initiative-Id: I-36A`, `Claim-Id: c72f4d42-…`, `Co-Authored-By: Claude Opus 5` |

---

## 4. ADRs

| ADR | Título | Estado | Efecto |
|---|---|---|---|
| **0020** | Catálogo neutral de secciones estructurales | aceptado | **Reemplaza a ADR-0008** en **autoridad conceptual**, no en comportamiento |
| **0021** | Identidad, unidades y presentación de secciones estructurales | aceptado | Nuevo. **No** reemplaza a ADR-0005 |
| 0008 | Perfiles estructurales unificados en secciones.csv por rol | **reemplazado por ADR-0020** | Sección *Decisión* **intacta**; solo estado, enlaces y una nota posterior fechada |
| 0005 | Estrategia de unidades | **aceptado, sin cambio** | Solo una nota posterior con el enlace: la pulgada sigue siendo la unidad interna y no se implementa conversión del DWG |

El índice `docs/adr/README.md` refleja los dos estados y explica el alcance exacto del reemplazo.

---

## 5. Esquema final

Namespace **`RackCad.Application.StructuralSections`**, en
`src/RackCad.Application/StructuralSections/` (cumple la regla namespace = carpeta de I-23).

**Modelo**: `StructuralSectionFamily` (+ tokens), `StructuralSectionId`,
`StructuralSectionDesignationNormalizer`, `StructuralSectionIdentity`, `StructuralSectionSource`
(+ `StructuralSectionUnitSystem`), `StructuralSectionProperties`, `IStructuralSectionDimensions` con
`WSectionDimensions` / `HssRectangularSectionDimensions` / `ChannelSectionDimensions` /
`AngleSectionDimensions`, `StructuralSectionDefinition`, `StructuralSectionStatusOverride`.

**Datos**: `StructuralSectionCsvSchema` (única autoridad de nombres y orden de columnas),
`StructuralSectionCsvSerializer`, `StructuralSectionCsvWriter`, `StrictCsvTable`,
`StructuralSectionCsvException`, `StructuralSectionsManifest`.

**Servicios**: `StructuralSectionCatalog`, `IStructuralSectionCatalogProvider`,
`CsvStructuralSectionCatalogProvider`, `StructuralSectionCatalogValidator`, `StructuralSectionUnits`,
`StructuralSectionLabelFormatter`.

**Compartido**: `RackCad.Application.Catalogs.CsvLexer` — el parser léxico RFC-4180 extraído
**verbatim** de `CsvCatalogReader`, que ahora delega. Comportamiento del lector tolerante **sin
cambio**, fijado por `CsvLexerTests`.

Columnas por archivo: identidad (8) + propiedades comunes (10) + específicas de familia —W 27,
HSS-RECT 7, C 31, L 33—. La matriz completa de las 84 columnas de la fuente, con su significado según
el Readme, unidades US y métricas, familias aplicables, campo de RackCad y razón de omisión cuando
aplica, vive en [`docs/guias/secciones-estructurales.md`](../../guias/secciones-estructurales.md) §11.

**Omitidas deliberadamente**: las seis razones de esbeltez (`bf/2tf`, `b/t`, `b/tdes`, `h/tw`,
`h/tdes`, `D/t`) por ser cocientes derivables de valores ya presentes y pertenecer al dominio de diseño
que ADR-0017 difiere; y `OD` e `ID`, que ninguna de las cuatro familias importadas tabula (`OD` sí se
**lee** para clasificar). El resto de columnas del bloque estadounidense se importa.

---

## 6. Política final de IDs

```
AISC-{FAMILIA}-{EDI_NORMALIZADO}     FAMILIA ∈ { W, HSS-RECT, C, L }
```

Normalización: mayúsculas invariantes, sin espacios, `.` `/` `-` → `_`. Resultado ASCII, mayúsculas,
sin espacios, sin barra, sin punto decimal. Sin revisión, sin grado de material, sin magnitudes.
`DisplayName` nunca es clave. Ninguna dimensión ni peso se deriva del texto de una designación.

| Etiqueta del manual | EDI | Id |
|---|---|---|
| `W12X26` | `W12X26` | `AISC-W-W12X26` |
| `C10X15.3` | `C10X15.3` | `AISC-C-C10X15_3` |
| `L4X4X1/4` | `L4X4X1/4` | `AISC-L-L4X4X1_4` |
| `HSS4X4X1/4` | `HSS4X4X.250` | `AISC-HSS-RECT-HSS4X4X_250` |

**Colisiones: 0.** Las 983 designaciones EDI producen 983 ids distintos, todos dentro de `[A-Z0-9_-]`.
También son 983 las designaciones EDI normalizadas distintas y 983 las etiquetas de manual distintas.

### Discrepancia con el ejemplo del encargo, resuelta y sujeta a validación

El encargo pide una política «basada en la designación EDI oficial» y da como ejemplo
`AISC-HSS-RECT-HSS4X4X1_4`. En el libro real el EDI de esa sección es **`HSS4X4X.250`** y
`HSS4X4X1/4` es su **AISC Manual Label**: para las 525 HSS rectangulares el EDI escribe el espesor en
decimal y el manual en fracción, y **513 de las 983 filas difieren** entre ambas designaciones. Los
otros tres ejemplos del encargo (`AISC-W-W12X28`, `AISC-C-C10X15_3`, `AISC-L-L4X4X1_4`) **sí**
coinciden con el EDI.

Se aplicó la **regla**, no el ejemplo: EDI en las cuatro familias sin excepción, porque una regla que
cambia de fuente según la familia deja de ser determinista. El id real es
**`AISC-HSS-RECT-HSS4X4X_250`**; el Manual Label se conserva íntegro, es único y es con el que se busca
y se muestra. Registrado en **ADR-0021 §6** y es el **punto 5** de la validación del dueño.

### Segundo hallazgo: `W12X28` no existe

La serie W12 de la v16.0 va `W12X14, 16, 19, 22, 26, 30, 35, 40, 45, 50, 53, 58, 65, 72, 79, 87, 96,
106, 120, 136, 152, 170, 190, 210, 230, 252, 279, 305, 336` — **no hay `W12X28`**. El ejemplo del dueño
fija el **formato** de la etiqueta de peso, no una sección real. Por eso la prueba que reproduce
literalmente `W12X28 — 28 lb/ft (41.7 kg/m)` usa una sección **sintética**, y las pruebas del catálogo
distribuido usan `W12X26 — 26 lb/ft (38.7 kg/m)`.

---

## 7. Conteos exactos

Salida literal del importador sobre el libro oficial.

| Concepto | Valor |
|---|---|
| Filas de datos de la hoja | **2 299** (más 2 filas finales vacías, ignoradas) |
| Filas **seleccionadas** | **983** |
| Secciones importadas | **983** |
| **Filas seleccionadas rechazadas** | **0** |

| Familia | Secciones |
|---|---|
| W | **289** |
| HSS rectangular y cuadrado | **525** (de ellas **126** cuadradas) |
| C | **32** |
| L | **137** |
| **Total** | **983** |

| Tipo excluido | Filas |
|---|---|
| `2L` | 639 |
| `WT` | 289 |
| `HSS-ROUND` | 189 |
| `PIPE` | 51 |
| `MC` | 40 |
| `S` | 28 |
| `ST` | 28 |
| `HP` | 22 |
| `M` | 16 |
| `MT` | 14 |
| **Total** | **1 316** |

`983 + 1 316 = 2 299`. La partición cierra exactamente y no queda ninguna fila sin clasificar.

**Criterio del HSS, por campos oficiales y no por expresión regular**: con `OD` y sin paredes →
redondo, excluido (189); con `Ht`/`B`/`h`/`b` y sin `OD` → importado (525). `525 + 189 = 714`, el total
de filas `Type = HSS`, sin solape ni sobrantes; ninguna fila resultó ambigua.

---

## 8. Contraste con el bloque métrico oficial

El bloque métrico **no genera filas**: se usa como testigo independiente de que cada magnitud se leyó
de la columna que su encabezado dice. Una desviación fuera de tolerancia detiene la importación.

| Magnitud | Comparadas | Desviación máxima | Tolerancia | Peor caso |
|---|---|---|---|---|
| `W` (peso) | 983 | **4.128 %** | 5 % | `C5X6.7` |
| `A` | 983 | 0.461 % | 1 % | `W24X55` |
| `d` | 458 | 0.435 % | 1 % | `W36X925` |
| `bf` | 321 | 0.412 % | 1 % | `W10X12` |
| `tw` | 321 | 0.402 % | 1 % | `C7X14.75` |
| `tf` | 321 | 0.445 % | 1 % | `W10X33` |
| `Ht` | 525 | 0.392 % | 1 % | `HSS4X4X1/2` |
| `B` | 525 | 0.392 % | 1 % | `HSS20X4X1/2` |
| `tnom` | 525 | 0.366 % | 1 % | `HSS16X6X3/16` |
| `tdes` | 525 | 0.288 % | 1 % | `HSS34X10X5/8` |
| `t` | 137 | 0.357 % | 1 % | `L8X8X1-1/8` |
| `b` | 662 | 0.392 % | 1 % | `L4X4X3/4` |

La tolerancia del peso es amplia **por una razón concreta y no por imprecisión**: la columna métrica de
peso es un **valor nominal de designación redondeado por separado**, no una conversión. `C5X6.7` se
publica como `10.4 kg/m` cuando la conversión exacta da `9.97`. La geometría, que sí es conversión con
redondeo a tres cifras significativas, se queda por debajo del **0.47 %**.

---

## 9. Archivos generados y sus hashes

SHA-256 tal como los declara `structural-sections-manifest.json`. Verificados **tres veces**: en el
archivo del worktree, en el objeto que Git almacena (`git show HEAD:…`) y dentro del bundle canónico.

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `structural-sections-w.csv` | 71 118 | `9259F672CDDC6855E321E0483F819F5875967145C6F218571F3D8E1FDCE78F1E` |
| `structural-sections-hss-rect.csv` | 93 588 | `FDC8E3E436DFA33421D0ED8A06F8CAC7B82C232F997802B6A95CC25498443F0D` |
| `structural-sections-c.csv` | 9 052 | `E42871A455AD2F78E9C9550E6B9D65431B678BB6CC867C841EC4E6BBEF66F63E` |
| `structural-sections-l.csv` | 38 455 | `6B5077003388735502FEBAC99281266B37C3688FAEEA2D2AB1BDB399E44BF2FC` |
| `structural-section-sources.csv` | 284 | `96FB1590D0FB2E904F4ECEC0D8560A6B7F6080CCE7F6E8302FFFA3A83B5FA694` |
| `structural-section-status.csv` | 26 | `7B4CB158AF88769BD90AB9CE2CE3D21010EDB7F7FB334E591917B6F5342E6D7D` |
| `structural-sections-manifest.json` | 1 519 | *(no se hashea a sí mismo: sería circular)* |

**Reproducibilidad demostrada, no afirmada.** Dos importaciones independientes sobre el mismo libro, a
directorios distintos, produjeron los **siete archivos byte-idénticos**. El manifiesto no lleva ninguna
marca de tiempo.

`.gitattributes` marca los siete como `-text`, con alcance limitado a ellos. El repositorio usa
`core.autocrlf=true`, así que sin esa regla el checkout convertiría los saltos de línea y el hash del
archivo **en disco** dejaría de coincidir con el del manifiesto en un clon nuevo —incluido el runner
Linux de CI—. Comprobado: los hashes del objeto almacenado en Git coinciden exactamente con los seis
de la tabla.

---

## 10. Sentinelas

Dos por familia, leídas **a mano** de la hoja `Database v16.0` del libro cuyo SHA-256 está en §2. Cada
una está fijada en `ShippedStructuralSectionCatalogTests` con su número de fila.

### W — fila 2, `W44X408`

`T_F=T` · `W=408` · `A=120` · `d=44.8` · `ddet=44.75` · `bf=16.1` · `bfdet=16.125` · `tw=1.22` ·
`twdet=1.25` · `tf=2.17` · `tfdet=2.1875` · `kdes=2.96` · `kdet=3.375` · `k1=1.8125` · `Ix=38700` ·
`Zx=2000` · `Sx=1730` · `rx=18` · `Iy=1520` · `Cw=691000` · `T=38` · `WGi=5.5` · `WGo=3`.
Métrico de contraste: `W=607`, `A=77400`, `d=1140`. `T_F=T` porque `tf = 2.17 in > 2 in`.

### W — fila 245, `W12X26`

`T_F=F` · `W=26` · `A=7.65` · `d=12.2` · `bf=6.49` · `tw=0.23` · `tf=0.38` · `kdes=0.68` ·
`kdet=1.0625` · `Ix=204` · `Iy=17.3` · `Cw=607` · `T=10.125`. Métrico: `W=38.7`, `A=4940`, `d=310`.

### HSS-RECT — fila 1536, `HSS34X10X1`

`W=277.07` · `A=76.2` · `Ht=34` · `h=31.2` · `B=10` · `b=7.21` · `tnom=1` · `tdes=0.93` · `Ix=9600` ·
`Zx=750` · `J=4040` · `C=555`. Métrico: `W=412`, `A=49200`. No es cuadrado.

### HSS-RECT — fila 1983, `HSS4X4X1/4` (EDI `HSS4X4X.250`)

`W=12.21` · `A=3.37` · `Ht=4` · `h=3.3` · `B=4` · `b=3.3` · `tnom=0.25` · `tdes=0.233` · `Ix=7.8` ·
`J=12.8` · `C=6.56`. Métrico: `W=18.1`, `A=2170`. **Es cuadrado** y se cuenta dentro de `HSS-RECT`.

### C — fila 357, `C15X50`

`W=50` · `A=14.7` · `d=15` · `bf=3.72` · `tw=0.716` · `tf=0.65` · `kdes=1.44` · `x=0.799` ·
`eo=0.583` · `xp=0.49` · `Ix=404` · `Cw=492` · `ro=5.49` · `H=0.937`. Métrico: `W=74`, `A=9480`,
`d=381`.

### C — fila 366, `C10X15.3`

`W=15.3` · `A=4.48` · `d=10` · `bf=2.6` · `tw=0.24` · `tf=0.436` · `x=0.634` · `eo=0.796` ·
`Ix=67.3` · `H=0.884`. Métrico: `W=22.8`, `A=2890`, `d=254`.

### L — fila 429, `L12X12X1-3/8`

`W=105` · `A=31.1` · `d=12` · `b=12` · `t=1.38` · `kdes=2.09` · `x=3.5` · `y=3.5` · `Ix=413` ·
`Iz=165` · `rz=2.3` · `H=0.627` · `tan(α)=1` · `Iw=661`. Métrico: `W=156`, `A=20100`, `d=305`.
Alas iguales.

### L — fila 509, `L4X4X1/4`

`W=6.6` · `A=1.93` · `d=4` · `b=4` · `t=0.25` · `kdes=0.625` · `x=1.08` · `y=1.08` · `Ix=3` ·
`Iz=1.19` · `J=4.38e-2` · `Cw=5.05e-2` · `zB=0`. Métrico: `W=9.8`, `A=1250`, `d=102`.

### Sentinela extra de nomenclatura — `L8X6X1`

AISC etiqueta el ángulo por su ala **larga** primero, mientras que las columnas son `d` = ala **corta**
y `b` = ala **larga**: `d=6`, `b=8`. Se fija con una prueba propia porque invertirlo espejearía en
silencio cada ángulo desigual que dibuje I-36B.

### `zB = 0` es un valor real

`zB` vale exactamente **0** en los **61** ángulos de alas iguales, porque el punto B está sobre el eje
z. Es el único cero legítimo de todo el cuerpo de datos (barrido sobre las 983 filas: ninguna otra
columna contiene un cero). La regla «un cero significa un valor perdido» se restringió por eso a las
**magnitudes**, dejando fuera las **posiciones**; hay regresión para ambos casos.

---

## 11. Pruebas y builds

| Suite / gate | Resultado |
|---|---|
| `RackCad.Tests` | **1762 / 1762**, 0 fallos, 0 omitidas (base `origin/main`: 1619 → **+143**) |
| `RackCad.UI.Tests` | **494 / 494**, 0 fallos, 0 omitidas (sin cambio: I-36A no toca UI) |
| Build `tools/RackCad.StructuralSections.Import` Debug | **0 errores, 0 advertencias** |
| Build `src/RackCad.Application` Debug | **0 errores, 0 advertencias** |
| Build `src/RackCad.UI` Debug | **0 errores, 0 advertencias** |
| Build `src/RackCad.Plugin` Debug | **0 errores** (2 `MSB3277` conocidos de las referencias AutoCAD) |
| `deploy/build-bundle.ps1` + `verify-bundle.ps1` | **OK, 147 comprobaciones**; DLL idénticos al publish, catálogos idénticos a `assets/catalogs`, cero DLL de Autodesk |
| Los 7 archivos nuevos dentro del bundle | **sí**, con hashes idénticos a los del manifiesto |

Reparto de las 143 pruebas nuevas:

- **`CsvLexerTests` (10)** — regresiones del parser léxico extraído: rarezas históricas (CR suelto
  descartado, última fila sin salto emitida, salto final sin fila fantasma, comilla doblada, salto
  dentro de comillas, línea en blanco) **más** el lector tolerante conducido de punta a punta.
- **`StructuralSectionModelTests` (37)** — normalización, política de ids, ausencia de revisión en el
  id, lookups por id/EDI/designación, habilitado/deshabilitado, filtro por familia, ambigüedad
  rechazada, duplicado rechazado, propiedades opcionales, `tnom` ≠ `tdes`, cuadrado como caso de
  rectangular, factor exacto de conversión, peso por longitud (incluida la prueba de que usa
  `WeightPerLength` y no el texto de la designación) y el formateador en sus tres formas, con el
  ejemplo literal del dueño.
- **`StructuralSectionStrictReaderTests` (25)** — encabezado faltante, duplicado, desconocido y vacío;
  fila con número de celdas incorrecto; id vacío; número ilegible con archivo, línea, columna e id;
  `NaN`/infinito; separador de miles rechazado; requerido ausente; opcional vacío que queda `null`;
  familia inválida; familia que contradice su archivo; sistema de unidades inválido; booleano
  inválido; archivo vacío; líneas en blanco ignoradas; y el overlay de estado completo (deshabilita
  sin borrar, id desconocido, id duplicado, overlay vacío, round-trip con coma dentro del texto).
- **`StructuralSectionValidatorTests` (24)** — fuente desconocida, revisión ausente, id que no
  corresponde a su designación, cero donde se espera magnitud, **cero aceptado donde se espera
  posición**, peso y área no positivos, área ausente, dimensión de familia faltante, dimensiones que no
  corresponden a la familia, `tdes > tnom`, pared plana que alcanza la total, alas del ángulo
  invertidas, grado de material como advertencia, designación ambigua, y el manifiesto (conteo,
  hash, filas rechazadas, auto-inclusión, hash del libro ausente, round-trip y **JSON sin `\r`**).
- **`StructuralSectionImporterTests` (19)** — enumeradas en §12.
- **`ShippedStructuralSectionCatalogTests` (28)** — el catálogo realmente distribuido, incluidas las
  sentinelas de §10.

Regresiones vigentes que siguen verdes y no se tocaron: `CsvCatalogReaderTests`,
`SeccionesCatalogTests`, `CatalogValidatorTests` (I-19), `CatalogCanonicalIdsTests` y
`CatalogManifestGuardTests` (I-26), `ShippedCatalogIntegrityTests`, `CatalogBlockManifestTests`,
`NamespaceFolderGuardTests` (I-23) y `UiSystemBoundaryGuardTests`.

---

## 12. Pruebas del importador, sobre un XLSX generado en runtime

No se versiona ningún fixture binario: `SyntheticAiscWorkbook` construye un `.xlsx` **real** —
container, `workbook.xml`, relaciones, `sharedStrings.xml` y celdas con referencia `r`— incluido el
**espejo métrico**, así que las pruebas recorren el mismo camino que el libro de 2 MB y no un stub.

1. Solo entran las **cuatro familias autorizadas** (W, HSS rect., HSS cuadrado, C, L conviviendo con
   HSS redondo, MC y 2L).
2. Los tipos excluidos se **reportan por tipo** y no son error.
3. Redondo y rectangular se separan **por los campos oficiales**, con dos designaciones que empiezan
   igual y que ninguna expresión regular podría separar con fiabilidad.
4. Un HSS que no sea exactamente una de las dos formas es **ambiguo** y detiene la importación.
5. **Ninguna fila seleccionada se descarta**: filas seleccionadas == secciones producidas.
6. Un encabezado **renombrado** detiene la importación nombrándolo.
7. Un **espejo métrico ausente** detiene la importación.
8. Un **número ilegible** detiene la importación.
9. Una **dimensión obligatoria ausente** rechaza la fila con su número y su razón.
10. Un `T_F` inválido rechaza la fila.
11. Un **peso no positivo** rechaza la fila.
12. Una **colisión de ids** detiene la importación nombrando las dos designaciones.
13. Un espejo métrico **contradictorio** detiene la importación.
14. Dos corridas producen archivos **byte-idénticos**.
15. El manifiesto **no lleva marca de tiempo** y **no se hashea a sí mismo**.
16. El manifiesto declara el hash del libro y los conteos que produjo.
17. `Publish` escribe los siete archivos y **no deja staging**.
18. Lo publicado se lee con el **proveedor estricto** y **valida limpio**.
19. Una reimportación **preserva el overlay** de las secciones que siguen existiendo.

---

## 13. Guardas de alcance, verificadas con `git diff`

| Guarda | Resultado |
|---|---|
| `assets/catalogs/secciones.csv` | **byte-idéntico** a `origin/main` (`git diff --exit-code` limpio) |
| Los otros 10 catálogos vigentes | **cero líneas cambiadas** |
| `blocks-library.dwg` | no se tocó; **no se creó ningún bloque** |
| `src/RackCad.Domain` | **cero archivos** |
| `src/RackCad.UI` | **cero archivos** |
| `src/RackCad.Plugin` | **cero archivos** |
| `deploy/` | **cero archivos** |
| `.github/` | **cero archivos** |
| `docs/HANDOFF.md` | **no tocado** (corresponde a la sesión de integración) |
| Columna Estado del ROADMAP | las filas de la Fase 6 nacen en `pendiente`; **nada marcado como integrada** |
| `architecture/geometria-secciones-estructurales` | **no creada**; reservada para I-36B |
| Geometría, contornos, radios, proyecciones, bloques, AutoCAD, WPF | **cero** |

Archivos **modificados** (todos los demás son nuevos): `.gitignore` (ruta ignorada del libro),
`RackCad.sln` (alta del proyecto de herramienta), `tests/RackCad.Tests/RackCad.Tests.csproj`
(referencia a la herramienta) y `src/RackCad.Application/Catalogs/CsvCatalogReader.cs` (**solo** delega
su parser léxico; comportamiento idéntico y fijado por regresiones), más los documentos del §14.

---

## 14. Documentación tocada

`docs/ARCHITECTURE.md` (§4.4.1 nueva y §7.6 ampliada), `docs/guias/catalogos-y-plantillas.md`,
`docs/guias/modelo-de-datos.md`, `docs/guias/secciones-estructurales.md` (**nueva**, con la matriz
completa de columnas), `docs/adr/README.md`, `docs/adr/0005-*.md` (nota posterior),
`docs/adr/0008-*.md` (estado + nota posterior), `docs/adr/0020-*.md` y `docs/adr/0021-*.md` (nuevos),
`docs/ROADMAP.md` (Fase 6), `docs/initiatives/README.md`,
`docs/initiatives/I-36A-catalogo-secciones-estructurales.md` (nuevo),
`docs/automation/decisions/I-36A.md` (nuevo), `docs/automation/state/I-36A.yml` (nuevo) y esta
evidencia.

---

## 15. Validación pendiente del dueño

**AutoCAD: no aplica** — I-36A no cambia dibujo, bloques, comandos ni DWG.

Gate `owner-validation`, siete puntos:

1. **Fuente y SHA-256** — §2, incluida la nota de que la URL `globalassets` del encargo ya no existe y
   de que se usó el enlace que publica la propia página oficial.
2. **Conteos por familia** — §7: 289 / 525 / 32 / 137 = 983; excluidos 1 316; rechazadas 0.
3. **Sentinelas** — §10, dos por familia con su fila del libro.
4. **Etiquetas de peso** — nativo primero: `W12X26 — 26 lb/ft (38.7 kg/m)`.
5. **Política de IDs** — §6, **incluido el caso HSS** (`AISC-HSS-RECT-HSS4X4X_250` para `HSS4X4X1/4`) y
   el hecho de que `W12X28` no existe en la v16.0.
6. **Estado habilitado/deshabilitado** — overlay de excepciones, hoy vacío; deshabilitar retira de las
   selecciones nuevas y `GetById` sigue resolviendo.
7. **`secciones.csv` y los sistemas existentes no cambiaron** — §13.

---

## 16. Confirmación expresa

- **No** se implementó nada de **I-36B**: cero geometría, contornos, radios, filetes, polilíneas,
  regiones, sólidos, vistas, longitudes prismáticas, orientación, proyección ni definiciones internas
  de AutoCAD. Su rama **no se creó**.
- **No** se tocó **AutoCAD**: ni la API, ni un bloque, ni `blocks-library.dwg`, ni `blocks.csv`.
- **No** se tocó la **UI**: `src/RackCad.UI` con cero archivos cambiados.
- **No** se migró ningún **miembro**: postes, largueros, celosías, separadores y `secciones.csv`
  intactos.
- **Ningún cambio funcional** en los sistemas existentes: Domain, UI y Plugin sin un archivo tocado, y
  el único cambio en Application fuera del namespace nuevo es la delegación del parser léxico.
- **`main` intacta**: todo el trabajo vive en `architecture/catalogo-secciones-estructurales`; no hubo
  merge, ni push a `main`, ni borrado de rama o worktree.
