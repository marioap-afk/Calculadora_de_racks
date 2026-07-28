# Evidencia I-36D — auditoría reproducible de las 28 filas AISC `S`

> Iniciativa: **I-36D** `feature/perfiles-aisc-s` · claim `b0ff23bc3f1483971e6c3f6280a54427eab9948a`
> · Claim-Id `964effe9-9e1a-4861-ac34-594b04da48c7` · base `202e456795ec65334212db03110d7149c6ca4dc9`.
> Fase 1 (documental y analítica). **No contiene implementación de producto.**

Todo lo que sigue está medido sobre el libro acreditado, no citado de la documentación. El
procesamiento se hizo en un scratchpad **fuera del repositorio**, con `openpyxl` **ya instalado** (no
se instaló ningún paquete) y en modo lectura. No quedó ningún script dentro del worktree.

## 1. Procedencia de la fuente

| Campo | Valor |
|---|---|
| Página oficial | <https://www.aisc.org/aisc/publications/steel-construction-manual/aisc-shapes-database-v160/> — responde `200` con `User-Agent` de navegador, `403` con `curl` desnudo |
| Anchor usado | `aria-Label="DOWNLOAD SHAPES DATABASE V16.0"` apunta a `https://cloud.aisc.org/biggie_bin/aisc-shapes-database-v160-2.xlsx` |
| Enlace **no** usado | `…v160h.xlsx` (`DOWNLOAD SHAPES DATABASE V16.0H`, Historic Shapes Database 1873-2016) |
| Nombre físico | `aisc-shapes-database-v16.0.xlsx` |
| Tamaño | 2 028 540 bytes |
| **SHA-256** | `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496` |
| Ubicación | `tools/sources/aisc-shapes-database-v16.0.xlsx` — **ignorada** por `.gitignore:17`, nunca versionada |
| Hojas existentes | **2**: `Readme` (B1:O161) y `Database v16.0` (A1:FJ2302, 166 columnas) |
| **Hoja usada** | **`Database v16.0`** |

Marcadores de identidad que `AiscWorkbookVerifier` exige del `Readme`, comprobados con su misma
normalización (mayúsculas, espacios colapsados):

| Marcador | Resultado | Dónde |
|---|---|---|
| `AISC SHAPES DATABASE V16.0` | **PRESENTE** | R002, título |
| `16TH EDITION` | **PRESENTE** | R021, «consistent with … 16th Edition, 1st Printing» |
| `ELECTRONIC DATA INTERCHANGE` | **PRESENTE** | R030, convención EDI del 25-jun-2001 |

**El Readme corresponde a v16.0**: se titula *AISC Shapes Database v16.0 / Readme File*, fechado
**agosto 2023**, y declara ser actualización de v15.0 consistente con el Manual 16.ª edición, 1.ª
impresión.

Dos hechos estructurales que condicionan toda lectura del libro:

1. **Las 166 columnas son dos bloques de unidades.** `A:CF` (1-84) es **US customary** y `CG:FJ`
   (85-166) repite las mismas cabeceras en métrico; `Type` y `T_F` aparecen una sola vez. Toda esta
   auditoría usa el bloque **US customary**, que es el que declara `structural-section-sources.csv`.
2. **El vacío no es celda vacía: es `–` (en dash, U+2013).** Contar `None` daría 100 % de completitud
   en todas las columnas y sería falso.

Filas bajo el encabezado: **2 301**, de las cuales **2 299 publican `Type`**; las dos últimas (2301 y
2302) están completamente vacías. Coincide con las «2 299 filas publicadas» que documenta
`AiscFamilyClassifier`. Distribución: `HSS` 714, `2L` 639, `W` 289, `WT` 289, `L` 137, `PIPE` 51,
`MC` 40, `C` 32, **`S` 28**, `ST` 28, `HP` 22, `M` 16, `MT` 14.

## 2. Tabla obligatoria de auditoría AISC `S`

| Campo | Resultado |
|---|---|
| **Cantidad de perfiles S** | **28**, filas físicas **307-334**, contiguas; coincide con `excludedTypeCounts.S = 28` del manifiesto |
| **Designaciones** | S24X121, S24X106, S24X100, S24X90, S24X80, S20X96, S20X86, S20X75, S20X66, S18X70, S18X54.7, S15X50, S15X42.9, S12X50, S12X40.8, S12X35, S12X31.8, S10X35, S10X25.4, S8X23, S8X18.4, S6X17.25, S6X12.5, S5X10, S4X9.5, S4X7.7, S3X7.5, S3X5.7 (orden del libro; EDI = etiqueta del Manual en las 28) |
| **Columnas dimensionales disponibles** | 12 de 15 completas 28/28: `d, ddet, bf, bfdet, tw, twdet, twdet/2, tf, tfdet, kdes, kdet, T`. Parcial: `WGi` 21/28. Ausentes: `k1` 0/28 y `WGo` 0/28 |
| **Propiedades resistentes disponibles** | Las 21 completas 28/28: `A, Ix, Zx, Sx, rx, Iy, Zy, Sy, ry, J, Cw, Wno, Sw1, Qf, Qw, rts, ho, PA, PB, PC, PD`. Además `bf/2tf` y `h/tw` 28/28 |
| **Datos `k`, `k1`, `T`, etc.** | `kdes` 28/28 (0.625-2), `kdet` 28/28 (0.625-2), `T` 28/28 (1.75-20.5), `WGi` 21/28 (2.25-4). `k1` y `WGo` **vacíos en las 28**. Los cuatro primeros son **distancias al pie del filete** por definición del Readme, no radios |
| **Información de pendiente de patín** | **Ninguna.** Cero columnas de pendiente, conicidad, paso o inclinación en las 166. `tan(α)` es el ángulo de ejes principales **de ángulos simples** y está vacío en S. «Sloped flanges» aparece sólo en las notas de `T_F` para **M** y **MT**; S no figura en ninguna nota especial y su `T_F` está vacío. El Readme tampoco fija dónde se mide `tf` |
| **Radios explícitos** | **Ninguno.** Cero radios de acuerdo, de raíz o de punta. Las 5 menciones de «radius» son radios de **giro** (`rx, ry, rz, ro, rts`). `radii`, `root`, `fillet radius`, `corner`, `chamfer`, `taper`, `pitch`, `inclination` sin columna y sin definición |
| **Datos faltantes para reconstrucción exacta** | Cuatro, todos geométricos: (1) **pendiente del patín**; (2) **radio de acuerdo alma-patín**; (3) **radio o chaflán de punta**; (4) el **punto de medición de `tf`** en un patín inclinado. Ninguno es derivable: `kdes`/`kdet`/`k1`/`T` localizan el **pie** del filete, no su curvatura. Secundariamente faltan `k1` y `WGo` para detallado de conexiones |
| **Datos suficientes para representación visual** | **Sí, en el grado `TabulatedDerived` ya vigente, y con una advertencia que no aplicaba a los canales C.** `d`, `bf`, `tw` y `tf` completos bastan para un contorno cerrado y correcto en envolvente y espesores. Pero el rasgo que **define** visualmente una S es la inclinación del patín, y un contorno sin ella se lee como una **W**: en C la aproximación daba un canal reconocible; en S da otra familia. Los datos alcanzan para representar, no para representar *como S* — de ahí la convención declarada de ADR-0023 |

## 3. Barrido exhaustivo de pendiente y radios

Términos buscados en los **166 encabezados** y en el `Readme` completo: `slope, taper, pitch,
inclination, angle, tan(α), radius, radii, fillet, toe, root, corner, bevel, chamfer, flare`.

**Encabezados: un único acierto en 166 — `tan(α)`.**

> R112 — `tan(α)`: *«Tangent of the angle between the y-y and z-z axes **for single angles**»*

Vacío en las 28 filas S, y el mapper sólo lo lee cuando la familia es `Angle`.

| Categoría | Qué existe realmente |
|---|---|
| **Dato numérico publicado por fila** | **Ninguno** de pendiente, conicidad o radio. Cero columnas |
| **Definición textual general de la familia** | `slope` aparece 2 veces, ambas en las notas de `T_F` (R037): *«M-shapes: a value of T indicates that the shape has sloped flanges»* y la equivalente de **MT**. Para W y WT, `T` significa `tf > 2 in`. **S no figura en ninguna nota**, y su `T_F` está vacío en las 28. S sólo aparece en R029 (lista de `Type`) y R149 (Fig. 1, agrupada con W, M y HP para el momento estático de alabeo) |
| **Distancia (no radio)** | `kdes` = «Distance from outer face of flange to web **toe of fillet** used for design»; `kdet` = ídem para detallado; `k1` = «Distance from web center line to flange **toe of fillet** used for detailing»; `T` = «Distance between web **toes of fillets** at top and bottom of web». El Readme las llama «Distance» literalmente |
| **Radio explícito** | **Inexistente.** `rx`, `ry`, `rz` son radios de **giro**; `ro` polar; `rts` efectivo. `radii`, `root`, `corner`, `bevel`, `chamfer`, `flare`, `taper`, `pitch`, `inclination` dan **0 ocurrencias** |

## 4. Viabilidad de la convención visual candidata

Convención evaluada — **no atribuida a AISC, ASTM ni a ningún fabricante**:

```
s = 1/6                         (convención visual constante de RackCad)
a = (bf - tw) / 2

tRaiz   = tf + s*a/2
tPunta  = tf - s*a/2
delta   = kdes - tRaiz
rVisual = delta * ( sqrt(1 + s^2) + s )
```

Cara exterior horizontal; cara interior con pendiente exactamente `s` de la raíz a la punta; `kdes`
fija la ordenada del pie del filete sobre el alma; `rVisual` es el radio tangente al alma y a la cara
inclinada; punta vertical y aguda; **ningún ajuste por perfil**.

**Coherencia con ADR-0022, comprobada:** con `s = 0` se obtiene `tRaiz = tf` y
`rVisual = kdes - tf`, que es exactamente el radio derivado que ADR-0022 ya usa para W. La regla no
bifurca el modelo: le añade el término de pendiente.

**Método.** Contorno cerrado de 16 segmentos rectos y 4 arcos, construido por simetría doble. Área y
momentos por **integral de contorno exacta** (media de la circulación de `x dy - y dx`, con la
primitiva analítica de cada arco), no por discretización. Verificado de forma **independiente** contra
la descomposición `tw*d + 4*a*tf + 4*area_de_filete`: coinciden a **2.8e-14 in²** en las 28 filas. Los
cuatro arcos barren **1.4056 rad** (80.54°) en las 28.

### 4.1 Tabla por designación (pulgadas)

| # | Fila | Designacion | d | bf | tw | tf | kdes | a | tRaiz | tPunta | delta | rVisual | tang. x | Area geom. | A | err % |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | 307 | S24X121 | 24.500 | 8.050 | 0.800 | 1.090 | 2.0000 | 3.625 | 1.3921 | 0.7879 | 0.6079 | 0.7176 | 0.5996 | 35.7023 | 35.50 | +0.57 |
| 2 | 308 | S24X106 | 24.500 | 7.870 | 0.620 | 1.090 | 2.0000 | 3.625 | 1.3921 | 0.7879 | 0.6079 | 0.7176 | 0.5996 | 31.2923 | 31.10 | +0.62 |
| 3 | 309 | S24X100 | 24.000 | 7.250 | 0.745 | 0.870 | 1.7500 | 3.252 | 1.1410 | 0.5990 | 0.6090 | 0.7189 | 0.6007 | 29.4970 | 29.30 | +0.67 |
| 4 | 310 | S24X90 | 24.000 | 7.130 | 0.625 | 0.870 | 1.7500 | 3.252 | 1.1410 | 0.5990 | 0.6090 | 0.7189 | 0.6007 | 26.6170 | 26.50 | +0.44 |
| 5 | 311 | S24X80 | 24.000 | 7.000 | 0.500 | 0.870 | 1.7500 | 3.250 | 1.1408 | 0.5992 | 0.6092 | 0.7191 | 0.6009 | 23.6085 | 23.50 | +0.46 |
| 6 | 312 | S20X96 | 20.300 | 7.200 | 0.800 | 0.920 | 1.7500 | 3.200 | 1.1867 | 0.6533 | 0.5633 | 0.6650 | 0.5557 | 28.2713 | 28.20 | +0.25 |
| 7 | 313 | S20X86 | 20.300 | 7.060 | 0.660 | 0.920 | 1.7500 | 3.200 | 1.1867 | 0.6533 | 0.5633 | 0.6650 | 0.5557 | 25.4293 | 25.30 | +0.51 |
| 8 | 314 | S20X75 | 20.000 | 6.390 | 0.635 | 0.795 | 1.6300 | 2.877 | 1.0348 | 0.5552 | 0.5952 | 0.7026 | 0.5871 | 22.1354 | 22.00 | +0.62 |
| 9 | 315 | S20X66 | 20.000 | 6.260 | 0.505 | 0.795 | 1.6300 | 2.877 | 1.0348 | 0.5552 | 0.5952 | 0.7026 | 0.5871 | 19.5354 | 19.40 | +0.70 |
| 10 | 316 | S18X70 | 18.000 | 6.250 | 0.711 | 0.691 | 1.5000 | 2.769 | 0.9218 | 0.4602 | 0.5782 | 0.6826 | 0.5703 | 20.7218 | 20.50 | +1.08 |
| 11 | 317 | S18X54.7 | 18.000 | 6.000 | 0.461 | 0.691 | 1.5000 | 2.769 | 0.9218 | 0.4602 | 0.5782 | 0.6826 | 0.5703 | 16.2218 | 16.00 | +1.39 |
| 12 | 318 | S15X50 | 15.000 | 5.640 | 0.550 | 0.622 | 1.3800 | 2.545 | 0.8341 | 0.4099 | 0.5459 | 0.6444 | 0.5385 | 14.8217 | 14.70 | +0.83 |
| 13 | 319 | S15X42.9 | 15.000 | 5.500 | 0.411 | 0.622 | 1.3800 | 2.545 | 0.8340 | 0.4100 | 0.5460 | 0.6445 | 0.5385 | 12.7355 | 12.60 | +1.08 |
| 14 | 320 | S12X50 | 12.000 | 5.480 | 0.687 | 0.659 | 1.4400 | 2.397 | 0.8587 | 0.4593 | 0.5813 | 0.6862 | 0.5734 | 14.8330 | 14.70 | +0.90 |
| 15 | 321 | S12X40.8 | 12.000 | 5.250 | 0.462 | 0.659 | 1.4400 | 2.394 | 0.8585 | 0.4595 | 0.5815 | 0.6864 | 0.5736 | 12.1266 | 11.90 | +1.90 |
| 16 | 322 | S12X35 | 12.000 | 5.080 | 0.428 | 0.544 | 1.1900 | 2.326 | 0.7378 | 0.3502 | 0.4522 | 0.5338 | 0.4460 | 10.3618 | 10.20 | +1.59 |
| 17 | 323 | S12X31.8 | 12.000 | 5.000 | 0.350 | 0.544 | 1.1900 | 2.325 | 0.7378 | 0.3503 | 0.4522 | 0.5339 | 0.4461 | 9.4237 | 9.31 | +1.22 |
| 18 | 324 | S10X35 | 10.000 | 4.940 | 0.594 | 0.491 | 1.1300 | 2.173 | 0.6721 | 0.3099 | 0.4579 | 0.5406 | 0.4517 | 10.3764 | 10.30 | +0.74 |
| 19 | 325 | S10X25.4 | 10.000 | 4.660 | 0.311 | 0.491 | 1.1300 | 2.175 | 0.6722 | 0.3098 | 0.4578 | 0.5404 | 0.4516 | 7.5493 | 7.45 | +1.33 |
| 20 | 326 | S8X23 | 8.000 | 4.170 | 0.441 | 0.425 | 1.0000 | 1.865 | 0.5804 | 0.2696 | 0.4196 | 0.4954 | 0.4139 | 6.8393 | 6.76 | +1.17 |
| 21 | 327 | S8X18.4 | 8.000 | 4.000 | 0.271 | 0.425 | 1.0000 | 1.865 | 0.5804 | 0.2696 | 0.4196 | 0.4954 | 0.4139 | 5.4793 | 5.40 | +1.47 |
| 22 | 328 | S6X17.25 | 6.000 | 3.570 | 0.465 | 0.359 | 0.8130 | 1.552 | 0.4884 | 0.2296 | 0.3246 | 0.3832 | 0.3202 | 5.1042 | 5.05 | +1.07 |
| 23 | 329 | S6X12.5 | 6.000 | 3.330 | 0.232 | 0.359 | 0.8130 | 1.549 | 0.4881 | 0.2299 | 0.3249 | 0.3836 | 0.3205 | 3.7013 | 3.66 | +1.13 |
| 24 | 330 | S5X10 | 5.000 | 3.000 | 0.214 | 0.326 | 0.7500 | 1.393 | 0.4421 | 0.2099 | 0.3079 | 0.3635 | 0.3037 | 2.9627 | 2.93 | +1.12 |
| 25 | 331 | S4X9.5 | 4.000 | 2.800 | 0.326 | 0.293 | 0.7500 | 1.237 | 0.3961 | 0.1899 | 0.3539 | 0.4178 | 0.3491 | 2.8545 | 2.79 | +2.31 |
| 26 | 332 | S4X7.7 | 4.000 | 2.660 | 0.193 | 0.293 | 0.7500 | 1.234 | 0.3958 | 0.1902 | 0.3542 | 0.4181 | 0.3494 | 2.3186 | 2.26 | +2.59 |
| 27 | 333 | S3X7.5 | 3.000 | 2.510 | 0.349 | 0.260 | 0.6250 | 1.080 | 0.3500 | 0.1700 | 0.2750 | 0.3246 | 0.2712 | 2.2315 | 2.20 | +1.43 |
| 28 | 334 | S3X5.7 | 3.000 | 2.330 | 0.170 | 0.260 | 0.6250 | 1.080 | 0.3500 | 0.1700 | 0.2750 | 0.3246 | 0.2713 | 1.6940 | 1.66 | +2.05 |

### 4.2 Verificaciones

| # | Verificación | Resultado |
|---|---|---|
| 1 | `a > 0` | **PASS 28/28** |
| 2 | `tPunta > 0` | **PASS 28/28** (mín. 0.1700 in en S3X7.5 y S3X5.7) |
| 3 | `tRaiz > tPunta` | **PASS 28/28** |
| 4 | `2*tRaiz < d` | **PASS 28/28** |
| 5 | `delta > 0` | **PASS 28/28** (mín. 0.2750 in) |
| 6 | `rVisual > 0` | **PASS 28/28** (0.3246 a 0.7191 in) |
| 7 | tangencia dentro de `0 < x < a` | **PASS 28/28** (máx. relación tangencia/`a` = 0.283) |
| 8a | el arco no invade la punta | **PASS 28/28** |
| 8b | el arco no cruza el eje del alma | **PASS 28/28** (su x mínima es `tw/2`, por tangencia) |
| 8c | tramo de alma de longitud > 0 (`d > 2*kdes`) | **PASS 28/28** |
| 8d | el arco no baja del pie del filete | **PASS 28/28** |
| 9a | tramo inclinado de longitud > 0 | **PASS 28/28** |
| 9b | punta de longitud > 0 | **PASS 28/28** |
| 9c | barrido de arco válido | **PASS 28/28** (1.4056 rad en los 4 arcos) |
| 10a | ancho = `bf` | **PASS 28/28** (exacto) |
| 10b | alto = `d` | **PASS 28/28** (exacto) |
| 11a | módulo de Cx < 1e-9 | **PASS 28/28** — máx. **1.204e-16** |
| 11b | módulo de Cy < 1e-9 | **PASS 28/28** — máx. **2.725e-15** |

Contorno **cerrado y simple**: sin segmentos de longitud cero (9a-9c) y sin autocruce — el arco vive
en `x >= tw/2` y por encima del pie del filete, la cara inclinada es monótona en x entre la tangencia
y la punta, y los filetes superior e inferior no se tocan porque `d - 2*kdes > 0`.

### 4.3 Error de área frente a `A` — **diagnóstico, no objetivo**

| Métrica | Valor |
|---|---|
| Error firmado | mín. **+0.253 %** · máx. **+2.592 %** (todos positivos) |
| Error absoluto | mín. 0.253 % · máx. **2.592 %** · media **1.116 %** · mediana **1.079 %** |
| Filas con error > 1 % | **16** |
| Filas con error > 2 % | **3** — S4X9.5 (+2.31 %), S4X7.7 (+2.59 %), S3X5.7 (+2.05 %) |
| Filas con error > 3 % | **0** |
| Filas con error > 5 % | **0** |
| Peor fila | **S4X7.7**, +2.592 % |

El signo, siempre positivo, es coherente: el contorno **añade** los cuatro filetes y **no** modela los
redondeos de punta, que restarían material. El error crece al bajar el peralte, donde `kdes` y `bf`
están redondeados a una fracción mayor en proporción al tamaño. **No se ajustó `s`, `tf`, el radio ni
ningún punto por fila.**

### 4.4 Coherencia de los pies del filete: `T` frente a `d - 2k`

| Designacion | T tabulado | d - 2*kdes | dif | d - 2*kdet | dif |
|---|---|---|---|---|---|
| S24X121 | 20.500 | 20.5000 | +0.0000 | 20.5000 | +0.0000 |
| S24X106 | 20.500 | 20.5000 | +0.0000 | 20.5000 | +0.0000 |
| S24X100 | 20.500 | 20.5000 | +0.0000 | 20.5000 | +0.0000 |
| S24X90 | 20.500 | 20.5000 | +0.0000 | 20.5000 | +0.0000 |
| S24X80 | 20.500 | 20.5000 | +0.0000 | 20.5000 | +0.0000 |
| S20X96 | 16.750 | 16.8000 | +0.0500 | 16.8000 | +0.0500 |
| S20X86 | 16.750 | 16.8000 | +0.0500 | 16.8000 | +0.0500 |
| S20X75 | 16.750 | 16.7400 | -0.0100 | 16.7500 | +0.0000 |
| S20X66 | 16.750 | 16.7400 | -0.0100 | 16.7500 | +0.0000 |
| S18X70 | 15.000 | 15.0000 | +0.0000 | 15.0000 | +0.0000 |
| S18X54.7 | 15.000 | 15.0000 | +0.0000 | 15.0000 | +0.0000 |
| S15X50 | 12.250 | 12.2400 | -0.0100 | 12.2500 | +0.0000 |
| S15X42.9 | 12.250 | 12.2400 | -0.0100 | 12.2500 | +0.0000 |
| S12X50 | 9.125 | 9.1200 | -0.0050 | 9.1250 | +0.0000 |
| S12X40.8 | 9.125 | 9.1200 | -0.0050 | 9.1250 | +0.0000 |
| S12X35 | 9.625 | 9.6200 | -0.0050 | 9.6250 | +0.0000 |
| S12X31.8 | 9.625 | 9.6200 | -0.0050 | 9.6250 | +0.0000 |
| S10X35 | 7.750 | 7.7400 | -0.0100 | 7.7500 | +0.0000 |
| S10X25.4 | 7.750 | 7.7400 | -0.0100 | 7.7500 | +0.0000 |
| S8X23 | 6.000 | 6.0000 | +0.0000 | 6.0000 | +0.0000 |
| S8X18.4 | 6.000 | 6.0000 | +0.0000 | 6.0000 | +0.0000 |
| S6X17.25 | 4.375 | 4.3740 | -0.0010 | 4.3750 | +0.0000 |
| S6X12.5 | 4.375 | 4.3740 | -0.0010 | 4.3750 | +0.0000 |
| S5X10 | 3.500 | 3.5000 | +0.0000 | 3.5000 | +0.0000 |
| S4X9.5 | 2.500 | 2.5000 | +0.0000 | 2.5000 | +0.0000 |
| S4X7.7 | 2.500 | 2.5000 | +0.0000 | 2.5000 | +0.0000 |
| S3X7.5 | 1.750 | 1.7500 | +0.0000 | 1.7500 | +0.0000 |
| S3X5.7 | 1.750 | 1.7500 | +0.0000 | 1.7500 | +0.0000 |

**Resultado, sin elegir en silencio la expresión que mejor se vea:**

- `d - 2*kdet` iguala a `T` **exactamente en 26 de 28** filas;
- `d - 2*kdes` lo hace en **14 de 28**;
- máxima diferencia absoluta = **0.0500 in** en ambas expresiones;
- las dos filas que no cuadran con ninguna son **S20X96** y **S20X86** (`d = 20.3`,
  `kdes = kdet = 1.75`, `T = 16.750` frente a 16.800): diferencia de **0.050 in**, atribuible al
  redondeo del propio `d` publicado.

Lectura: **`T` es coherente con el valor de detallado `kdet`, no con `kdes`.** La convención visual
ancla el dibujo en **`kdes`** —el valor de diseño, que es el que ADR-0022 ya usa para el radio
derivado de W— y emplea `T` únicamente como **verificación**. Queda escrito que ambas expresiones
difieren; ADR-0023 registra la elección y su motivo. **`T` no es un radio.**

## 5. Conclusión de la fase

Ningún criterio de bloqueo se disparó: `tPunta > 0`, `delta > 0`, tangencia dentro del vuelo libre,
sin autocruce, bounds exactos, centroide en el origen a 1e-16, **ninguna fila requiere excepción
particular** y el error absoluto máximo (**2.592 %**) queda **por debajo del 3 %**.

La convención candidata es **geométricamente viable para las 28 filas**. Su aceptación no es
geométrica sino del Owner, sobre el dibujo real, mediante **ADR-0023** (`propuesto`).

---

# Evidencia de implementación (fases 2-4)

## 6. Catálogo

| Comprobación | Resultado |
|---|---|
| Filas `Type = S` importadas | **28**, cero descartes silenciosos |
| Total del catálogo | **1 011** (983 + 28) |
| `countsByFamily.S` | **28**; `S` **retirado** de `excludedTypeCounts` |
| Archivo nuevo | `structural-sections-s.csv`, SHA-256 `C081CD93F95A1BBD5F701C6975D18803AA6CC2E798D94B5038544383E8CF3707` |
| **Los cuatro CSV previos** | **byte-idénticos** — `git diff` vacío y los cuatro SHA-256 sin cambio (`9259F672…`, `FDC8E3E4…`, `E42871A4…`, `6B507700…`) |
| `structural-section-sources.csv` | sin cambio (`AD2AC230…`) |
| `secciones.csv` | **intacto** |
| `mapperVersion` | `I-36A.2` → **`I-36D.1`** — el mapeo ganó una familia, así que un catálogo del mapper anterior debe fallar **ruidosamente**, no cargar con una familia ausente |
| Reproducibilidad | dos ejecuciones del importador produjeron el **mismo** SHA-256 en los seis archivos |

**Corrección del id sentinela.** El contrato anunciaba `AISC-S-S10X25.4`. El id real es
**`AISC-S-S10X25_4`**: `StructuralSectionDesignationNormalizer` convierte el punto en `_`, y esa regla
la fija **ADR-0021, ya aceptado**, con el ejemplo `AISC-HSS-RECT-HSS4X4X_250`. Cambiarla habría roto los
**525** ids de HSS ya presentes en diseños guardados. Se corrigió el **documento**, no el normalizador.
La designación EDI publicada conserva su punto (`S10X25.4`) en su propio campo.

**`T_F` se omite del esquema de S**, en vez de escribirse vacío: AISC reserva sus notas especiales a W,
M, WT y MT, y las 28 filas lo dejan en blanco. `SourceSpecialNote` queda `null`.

## 7. Geometría

| Comprobación | Resultado |
|---|---|
| Builder | `SSectionGeometryBuilder`, hermano de los cuatro existentes |
| Autoridad | eje **nuevo y ortogonal** `SectionGeometryAuthority`; `SectionFidelity` **no cambia** |
| S | `VisualDerived` en **los dos** niveles de detalle |
| W, HSS, C, L | `TabulatedConstrained` — comprobado sobre las 983, en ambos niveles |
| Fidelidad de S | `TabulatedDerived` en `Tabulated`, `Simplified` en `Simplified`, **cero degradadas** |
| Recuento global | `TabulatedComplete` 289 · `TabulatedDerived` **722** (694 + 28) |
| Bounds | ancho = `bf`, alto = `d`, **exactos** en las 28 × 2 niveles |
| Centroide | residuo < 1e-9 por simetría doble |
| Excepciones por designación | **ninguna**: las 28 derivan el filete sin degradar |
| Área | +0,25 % a +2,59 %, **diagnóstica**, con prueba de **banda** que falla si alguien la cierra ajustando la regla |

**La advertencia vive en el tipo.** `StructuralSectionGeometry.Create` **lanza** si la autoridad es
`VisualDerived` y falta el diagnóstico `SG_VISUAL_CONVENTION_APPLIED`. No es una convención de
llamada: ninguna ruta futura puede perderla.

**Una sola tubería.** `StructuralSectionRepresentationPlan` transporta la autoridad y la incluye en su
`Signature()`; el preview y AutoCAD lo consumen igual. Dos guardas de fuente comprueban que el
adaptador **no** reimplementa la pendiente, el filete ni la familia.

## 8. UI y Plugin

- Filtro de familia: **«S / IPS — viga estándar americana»**, poblado desde `StructuralSectionFamilies.All`.
- Inspector: línea de autoridad **destacada** (negrita, color), visible **sólo** para `VisualDerived`.
- `RACKSECCION` y el botón del menú: advertencia **incondicional** bajo `plan.IsVisualDerived` —no
  depende del detalle, ni de la fidelidad, ni de opción alguna—. Una guarda de fuente comprueba además
  que no existe ninguna bandera que la apague.
- **Cero** inspectores, previews, comandos o adaptadores nuevos.

## 9. Pruebas y builds

| Gate | Resultado |
|---|---|
| `RackCad.Tests` | **2 093 / 2 093** (base 2 071, +22) |
| `RackCad.UI.Tests` | **538 / 538** (base 534, +4) |
| Builds Debug | Application, UI y Plugin — 0 errores propios (2 `MSB3277` conocidas del Plugin) |
| Bundle | **153 comprobaciones** (147 + 6 por el CSV nuevo); DLL idénticos al publish, catálogos idénticos a `assets/catalogs`, cero DLL Autodesk |
| Harness del bundle | **10 / 10** (1 válido + 9 negativos) |

Suites nuevas: `StructuralSectionSFamilyTests` (20), dos guardas de fuente del Plugin, tres pruebas de
inspector y el caso `S = 28` del filtro por familia.
