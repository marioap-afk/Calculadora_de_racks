# I-65 — F4: paquete de aprobación de ingeniería del mapeo (R-3, R-4, R-5)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «AutoCAD Smoke Acceptance / Final Engineering Mapping
Review» ([texto literal](../automation/decisions/I-65-owner-disposition-smoke-acceptance-final-mapping-review.original.txt); decisiones
[§60](../automation/decisions/I-65.md)). Cierra la parte documental de R-3 y prepara R-4 y R-5 sobre la
[trazabilidad](I-65-f4-structural-sources-traceability.md) y las [propiedades finales](I-65-f4-final-properties-for-approval.md).

> **ENGINEERING DOCUMENTATION = HECHA. MAPPING UPDATE = NO.** La tabla `section-material-map.json` no cambia (SHA-256
> `5229231e0d924e48a839ef77bf0b63a2b631214a670783e01d148b9ac6cb4048`; las dos filas siguen `proposed`). `verifiedBy` y `verifiedOn`
> quedan vacíos y **no** se atribuye firma de ingeniería. Sin código, sin Workers, sin AutoCAD, sin RAM. F4 GATE PASS = NO.

## 1. Criterios

| Clasificación | Significado en este paquete |
|---|---|
| **VERIFIED** | Valor leído en una fuente primaria publicada, accesible en R-3 y con integridad comprobada (hash), o metadato bibliográfico leído en la página oficial del emisor |
| **DERIVED** | Valor o convención reproducidos por cálculo independiente de I-65 a partir de datos primarios publicados. Complementa la cita; no la sustituye |
| **PENDING** | La fuente primaria no pudo leerse (PDF que exige descarga, norma de pago, página que bloquea la lectura) o no permite verificar el valor |

| Naturaleza | Significado |
|---|---|
| **P** | Propiedad publicada por el emisor |
| **D** | Propiedad derivada matemáticamente |
| **C** | Convención de modelado o normativa (no es una medida del perfil) |

RAM **nunca** es prueba: la columna «Diferencia frente a RAM» solo informa. Ningún valor se clasifica VERIFIED por coincidir con RAM.

## 2. Fuentes primarias y su acceso en R-3

| # | Fuente | Acceso | Resultado |
|---|---|---|---|
| FP-1 | **AISC Shapes Database v16.0** (libro oficial de AISC, hoja `Database v16.0`) | Copia descargada en I-36A del enlace oficial de AISC (`https://cloud.aisc.org/biggie_bin/aisc-shapes-database-v160-2.xlsx`, botón de la página oficial; procedencia en la [evidencia de I-36A](../automation/evidence/I-36A-catalogo-secciones-estructurales.md) §2). Leída en solo lectura en `tools/sources/` del checkout principal (ignorada por Git; el checkout sigue limpio). SHA-256 `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496` = el del [manifiesto del catálogo](../../assets/catalogs/structural-sections-manifest.json). Su hoja `Readme` declara la versión v16.0 de agosto de 2023 y su coherencia con las tablas del Manual AISC, 16.ª edición, 1.ª impresión | **Disponible**: base de las propiedades geométricas VERIFIED |
| FP-2 | ANSI/AISC 360-22, *Specification for Structural Steel Buildings* | Gratuita, pero solo en PDF tras la página de descarga de AISC; las páginas HTML de AISC respondieron 403 a la herramienta de lectura. **No se descargó** (descargar exige permiso expreso) | PENDING |
| FP-3 | AISC *Steel Construction Manual*, 16.ª ed. | Obra de pago; no disponible | PENDING (FP-1 declara coherencia con sus tablas) |
| FP-4 | ASTM A992/A992M | Ficha oficial (HTML) en `store.astm.org/a0992_a0992m-22.html`: **A992/A992M-22, activa**, actualizada el 2022-09-21; el texto de la norma es de pago | Edición vigente VERIFIED; requisitos PENDING |
| FP-5 | ASTM A500/A500M | Ficha oficial en `store.astm.org/a0500_a0500m-23.html`: **A500/A500M-23, activa**, actualizada el 2023-11-21; texto de pago | Edición vigente VERIFIED; requisitos PENDING |
| FP-6 | EN 1993-1-1, apartado 3.2.6 (ν del acero) | Norma de pago; no leída | PENDING |

Las fichas de ASTM muestran un aviso que prohíbe usar IA sobre las normas de ASTM. De esas páginas solo se tomaron metadatos
bibliográficos (designación, estado y fecha); no se procesó contenido de ninguna norma.

## 3. Comprobaciones derivadas sobre la población de FP-1

Script [aisc-v16-convention-check.py.txt](../automation/evidence/I-65-f4/aisc-v16-convention-check.py.txt) (biblioteca estándar,
solo lectura), salida [aisc-v16-convention-check.out.txt](../automation/evidence/I-65-f4/aisc-v16-convention-check.out.txt). Complementa
la derivación de la fila anterior ([script](../automation/evidence/I-65-f4/section-props-analytic-check.py.txt)).

| # | Comprobación | Resultado |
|---|---|---|
| C-0 | Filas del piloto tal como se publican | W12X26 y HSS4X4X1/4 extraídas literalmente (todas sus columnas imperiales) |
| C-1 | `tdes` = 0.93·`tnom` (a 0.001 in) en los 525 HSS rectangulares y cuadrados | **525/525** (524 con el `tnom` tabulado; la restante es `HSS1-1/2X1-1/2X3/16`, cuyo `tnom` 0.188 es 3/16 = 0.1875 redondeado) |
| C-2 | Ancho plano `h` = `Ht` − 3·`tdes`, `b` = `B` − 3·`tdes` | **525/525** |
| C-3 | A e Ix de los 525 HSS con radio exterior 2.0·`tdes` frente a 1.5·`tdes` | 2.0·`tdes`: \|ΔA\| mediana 0.074 % (máx. 0.39 %), \|ΔIx\| mediana 0.075 % (máx. 0.93 % en `HSS1-1/2X1-1/2X1/4`, Ix tabulado con dos cifras). 1.5·`tdes`: 0.88 % y 1.68 % de mediana; 510 filas con \|ΔIx\| > 0.5 % |
| C-4 | J de los 289 perfiles W con la fórmula con filetes de El Darwish y Johnston (r = `kdes` − `tf`) | Con **0.21·tf⁴ por ala** (total 0.42·tf⁴): **286/289** dentro del 1 %, mediana +0.05 %, máx. 1.14 %. Con 0.42·tf⁴ por ala: 0/289. Sin filetes: 36/289 |
| C-5 | Densidad implícita de los pesos nominales publicados | 525 HSS (área con `tnom` y radio 2·`tnom`): mediana **490.00 pcf** (488.79 a 490.56). 289 W (`W`·144/A, con el peso de la designación redondeado): mediana 489.3 pcf |
| C-6 | Filas del piloto | W12X26: J derivada 0.29994 (−0.02 %); 489.41 pcf. HSS4X4X1/4: 0.93·0.25 = 0.2325 → 0.233; A 3.3710 y Ix 7.8008 con 2·`tdes` (3.4176 y 7.9826 con 1.5·`tdes`); 489.89 pcf |
| C-7 | ν implícito en E = 29 000 ksi y G = 11 200 ksi | E/(2G) − 1 = **0.2946**; con ν = 0.30, G = 11 153.85 ksi (−0.41 %) |
| C-8 | Peso de RAM | 0.28179435 lb/in³ = 486.94 pcf (−0.62 %) = **7 800.0 kg/m³**; 490 pcf = 7 849.0 kg/m³ |

C-4 resuelve por derivación la forma de la fórmula que la trazabilidad (T-06) dejaba abierta: 0.21·tf⁴ por ala.

## 4. Tabla final

### 4.1 Fila `AISC-W-W12X26`

| # | Propiedad | Valor propuesto | Fuente técnica específica | Evidencia verificable | Diferencia frente a RAM | Clasificación | Decisión |
|---|---|---|---|---|---|---|---|
| W-01 | Identidad | AISC `W12X26` (EDI `W12X26`) → RAM United States / `W` / `W 12X26` | FP-1, columnas `AISC_Manual_Label` y `EDI_Std_Nomenclature` | C-0 | idéntica | VERIFIED (P); identidad en RAM reconocida en M-05c | — |
| W-02 | A | 7.65 in² | FP-1, columna `A` | C-0; derivación −0.12 % | 0 % | VERIFIED (P) + DERIVED | D-01 |
| W-03 | Ix | 204 in⁴ | FP-1, `Ix` | C-0; −0.24 % | 0 % | VERIFIED (P) + DERIVED | D-01 |
| W-04 | Iy | 17.3 in⁴ | FP-1, `Iy` | C-0; +0.16 % | 0 % | VERIFIED (P) + DERIVED | D-01 |
| W-05 | **J** | **0.300 in⁴** (tabulado `0.3`) | FP-1, `J` | C-0; C-4: 0.29994 (−0.02 %) con la forma fijada en la población (286/289) | 0 % | **VERIFIED (P) + DERIVED**. La cita de la fórmula en la Design Guide 9 de AISC sigue PENDING; no hace falta para el valor | D-01 |
| W-06 | d; bf; tw; tf; kdes. Sx; Zx; Sy; Zy. Cw | 12.2; 6.49; 0.23; 0.38; 0.68 in. 33.4; 37.2; 5.34; 8.17 in³. 607 in⁶ | FP-1 | C-0; Sx −0.12 %, Zx −0.24 % | 0 % | VERIFIED (P); informativas, no se escriben | — |
| W-07 | Peso nominal | 26 lb/ft | FP-1, `W` | C-5: 489.41 pcf implícitos | — | VERIFIED (P); informativo | — |

### 4.2 Fila `AISC-HSS-RECT-HSS4X4X_250`

| # | Propiedad | Valor propuesto | Fuente técnica específica | Evidencia verificable | Diferencia frente a RAM | Clasificación | Decisión |
|---|---|---|---|---|---|---|---|
| H-01 | Identidad | AISC `HSS4X4X1/4` (EDI `HSS4X4X.250`) → RAM United States / `HSS_SQR` / `HSS_SQR 4X4X1_4` | FP-1 | C-0 | idéntica | VERIFIED (P); identidad reconocida en M-05c | — |
| H-02 | tnom; **tdes** | 0.25; **0.233 in** | FP-1, `tnom` y `tdes` (el `Readme` define `tdes` como espesor de diseño de la pared) | C-0 | `t` 0.233: 0 % | **VERIFIED (P)** | D-01 |
| H-03 | Regla tdes = 0.93·tnom (A500) | 0.93 | ANSI/AISC 360-22, apartado B4.2 (FP-2, no leída) | C-1: 525/525 filas de FP-1; corroboración secundaria (Steel Tube Institute, RISA, artículos de AISC) en la trazabilidad | — | C: **DERIVED**; cita primaria PENDING | D-02 |
| H-04 | **Radio exterior de esquina** para las propiedades | 2.0·tdes (interior 1.0·tdes) | Manual AISC 16.ª ed., parte 1 (FP-3, no disponible); el `Readme` de FP-1 no lo declara | C-3: 2.0·tdes reproduce la población (mediana 0.07 %); 1.5·tdes no (mediana 0.9 a 1.7 %) | no aplica: RAM usa valores tabulados | C: **DERIVED**; cita primaria PENDING; **no se escribe en la fila** | D-02 |
| H-05 | Ancho plano h = b | 3.3 in (= B − 3·tdes) | FP-1, `h` y `b` | C-2: 525/525 | — | VERIFIED (P); es la convención de esbeltez, no la de propiedades | — |
| H-06 | A | 3.37 in² | FP-1, `A` | C-0; +0.03 % | 0 % | VERIFIED (P) + DERIVED | D-01 |
| H-07 | Ix = Iy | 7.80 in⁴ | FP-1, `Ix`, `Iy` | C-0; +0.01 % | 0 % | VERIFIED (P) + DERIVED | D-01 |
| H-08 | J | 12.8 in⁴ | FP-1, `J` | C-0; −0.15 % | 0 % | VERIFIED (P) + DERIVED | D-01 |
| H-09 | S; Z; r; C | 3.90; 4.69 in³; 1.52 in; 6.56 in³ | FP-1 | C-0; desvíos ≤ 0.08 % | S, Z: 0 %; r y C no se guardan en RAM | VERIFIED (P) + DERIVED; informativas | — |
| H-10 | Peso nominal | 12.21 lb/ft | FP-1, `W` | C-5: 489.89 pcf implícitos | — | VERIFIED (P); informativo | — |

### 4.3 Material (las dos filas)

| # | Propiedad | Valor propuesto | Fuente técnica específica | Evidencia verificable | Diferencia frente a RAM | Clasificación | Decisión |
|---|---|---|---|---|---|---|---|
| M-01 | **E** | 29 000 ksi → `{ "value": 29000, "unit": "ksi" }` | ANSI/AISC 360-22, lista de símbolos (FP-2) | FP-2 no leída. Corroboración secundaria: publicaciones de AISC (Steel Bridge Design Handbook, cap. 1; Engineering Journal) y borrador AISC N690-24 | 0 % (RAM 29 000 000 psi) | **PENDING** (P no leída) | D-03 |
| M-02 | **ν** | 0.30 → `poissonRatio: 0.3`, `shearModulus: null` | Convención de modelado; citas candidatas: EN 1993-1-1, 3.2.6 (FP-6) o AISC 360 (FP-2) | C-7: el G de AISC (11 200 ksi) implica ν = 0.2946; ν = 0.30 da G −0.41 % | 0 % (RAM 0.3) | **C: PENDING** (cita) | D-04 |
| M-03 | **Peso unitario esperado** | 490 pcf → `{ "value": 490, "unit": "pcf" }` | Manual AISC 16.ª ed. (FP-3, no disponible) | C-5: los pesos publicados en FP-1 implican 490.00 pcf de mediana en 525 HSS. Corroboración secundaria: Iowa DOT, IM 569 | **−0.62 %** (RAM 486.94 pcf = 7 800.0 kg/m³), DIF-01 | **DERIVED**; cita primaria PENDING | D-05 |
| M-04 | Designación y edición, fila W | ASTM A992/A992M, Grado 50; edición activa A992/A992M-22 | FP-4 | ficha oficial leída el 2026-10-14 | RAM `A992 Gr50` no declara edición | Edición vigente **VERIFIED**; edición que referencia AISC 360-22 (A3.1): PENDING | D-06 |
| M-05 | Fy; Fu (A992) | 50 ksi (máx. 65); 65 ksi. Informativos, no se escriben | ASTM A992 (FP-4, de pago) | corroboración secundaria: AISC SteelWise 2015 y Modern Steel Construction 1999 | 0 % (RAM 50 000 y 65 000 psi) | PENDING (informativo) | D-06 |
| M-06 | Designación y edición, fila HSS | ASTM A500/A500M, Grado C, cuadrado y rectangular; edición activa A500/A500M-23 | FP-5 | ficha oficial leída el 2026-10-14 | RAM `A500 GrC rectangular` no declara edición | Edición vigente **VERIFIED**; edición referenciada: PENDING | D-06 |
| M-07 | Fy; Fu (A500 Gr C) | 50; 62 ksi. Informativos | ASTM A500 (FP-5, de pago) | corroboración secundaria: Steel Tube Institute | 0 % frente a `A500 GrC rectangular` (no `A500 GrC rounded`, Fy 46) | PENDING (informativo) | D-06 |
| M-08 | Edición formal del catálogo de RAM | no declarada por Bentley | — | DIF-04; R-1: 289 W y 126 HSS de v16.0 idénticos en RAM | — | **Limitación registrada**; no se declara verificada | — (R-1 vigente) |

### 4.4 Resumen

| Clasificación | Propiedades |
|---|---|
| VERIFIED | Toda la geometría de las dos filas (W-01..W-07, H-01, H-02, H-05..H-10) y las ediciones vigentes de ASTM (M-04, M-06) |
| DERIVED | Regla 0.93·tnom (H-03), radio exterior 2·tdes (H-04), forma de la fórmula de J (en W-05) y peso unitario 490 pcf (M-03) |
| PENDING | E (M-01), ν (M-02), las citas primarias de H-03, H-04 y M-03, las ediciones referenciadas por AISC 360-22 y Fy/Fu (M-05, M-07) |

Los tres valores de material que se **escriben** en las filas (E, ν, peso) no tienen cita primaria leída en R-3: E y ν son PENDING y el peso
es DERIVED. La geometría, que la tabla no escribe y RAM toma de su catálogo, queda VERIFIED.

## 5. Diferencias documentadas

| # | Diferencia | Disposición |
|---|---|---|
| DIF-01 | Peso unitario de RAM 486.94 pcf (7 800 kg/m³) frente a 490 pcf: −0.62 % | Informativa y aceptada **solo** para la prueba de transferencia sin peso propio. La tolerancia no cambia. `unitWeight` lleva el valor esperado (490), no el de RAM. Los valores de RAM **no** son equivalentes para análisis posteriores |
| DIF-02 | ν = 0.30 frente al ν = 0.2946 implícito en el G de AISC | El oráculo es ν (R-2); G no se escribe; con ν = 0.30, G queda 0.41 % por debajo de 11 200 ksi |
| DIF-04 | Edición formal del catálogo de RAM | Sin verificar; coherencia con v16.0 como evidencia del piloto (R-1). La limitación entra en el texto `source` |

## 6. Textos `source` propuestos (R-4)

Restricciones del lector (`RamSectionMapReader`): una línea, caracteres imprimibles y el espacio como único blanco. El texto entra en el
manifiesto del paquete y en `AnalysisInputFingerprint`. Se proponen en ASCII.

**Variante B (recomendada si no se cierran las citas primarias; todo su contenido está verificado o derivado en este paquete):**

Fila `AISC-W-W12X26`:

```text
Geometry: AISC Shapes Database v16.0 (Aug 2023; file SHA-256 82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496), row W12X26 as published: A 7.65 in2, Ix 204 in4, Iy 17.3 in4, J 0.300 in4; independent derivation within 0.24 %. Material: ASTM A992/A992M Grade 50 (current ASTM edition A992/A992M-22). E 29000 ksi and nu 0.30: modeling conventions for structural steel accepted at verification; primary citation not read in I-65. Unit weight 490 pcf: steel density implied by the AISC v16.0 HSS nominal weights (median 490.00 pcf). RAM: United States/W/W 12X26, material A992 Gr50 (E 29000 ksi, nu 0.3, 486.94 pcf). DIF-01: RAM unit weight -0.62 %, informative only for the pilot transfer test without self weight; RAM values are not equivalent for later analyses. RAM catalog formal edition not verified. Ref: I-65 F4 engineering approval package.
```

Fila `AISC-HSS-RECT-HSS4X4X_250`:

```text
Geometry: AISC Shapes Database v16.0 (Aug 2023; file SHA-256 82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496), row HSS4X4X1/4 as published: tnom 0.25 in, tdes 0.233 in (0.93 tnom), A 3.37 in2, Ix = Iy 7.80 in4, J 12.8 in4; independent derivation with outside corner radius 2 tdes within 0.15 %. Material: ASTM A500/A500M Grade C, square and rectangular (current ASTM edition A500/A500M-23). E 29000 ksi and nu 0.30: modeling conventions for structural steel accepted at verification; primary citation not read in I-65. Unit weight 490 pcf: steel density implied by the AISC v16.0 HSS nominal weights (median 490.00 pcf). RAM: United States/HSS_SQR/HSS_SQR 4X4X1_4, material A500 GrC rectangular, not A500 GrC rounded (E 29000 ksi, nu 0.3, 486.94 pcf). DIF-01: RAM unit weight -0.62 %, informative only for the pilot transfer test without self weight; RAM values are not equivalent for later analyses. RAM catalog formal edition not verified. Ref: I-65 F4 engineering approval package.
```

**Variante A (si el Owner confirma las citas primarias de D-03 a D-05):** igual que B, pero la frase de E y ν y la del peso se sustituyen
por:

```text
E 29000 ksi: ANSI/AISC 360-22, Symbols. nu 0.30: <cita confirmada por el Owner>. Unit weight 490 pcf: <tabla del AISC Steel Construction Manual 16th ed. confirmada por el Owner>; consistent with the density implied by the AISC v16.0 HSS nominal weights.
```

Los marcadores `<…>` no se rellenan por analogía: los fija el Owner con la edición, el apartado o la tabla que consulte.

## 7. `verifiedBy` y `verifiedOn` (R-5)

Preparados y **vacíos**:

```json
"status": "proposed",
"verifiedBy": null,
"verifiedOn": null
```

- `verifiedBy`: texto imprimible de una línea con la identidad del verificador que el Owner apruebe **expresamente**. El Coordinator no lo
  propone ni lo atribuye (OD-F4-02 sigue provisional y la disposición prohíbe atribuir firma).
- `verifiedOn`: fecha `yyyy-MM-dd` de esa aprobación expresa.
- El lector exige ambos, más `ramSection.table`, `ramSection.country`, `elasticModulus`, `unitWeight`, `source` y ν o G, para aceptar
  una fila `verified`; con cualquiera vacío la fila es `Invalid`.

## 8. Decisiones que requieren aprobación del Owner

| # | Decisión | Opciones | Recomendación |
|---|---|---|---|
| D-01 | Clasificar la geometría de las dos filas como VERIFIED con FP-1 (archivo oficial con hash comprobado) y la derivación como corroboración | aprobar / exigir además FP-3 | Aprobar |
| D-02 | Regla 0.93·tnom y radio 2·tdes: aceptar la evidencia DERIVED de la población o exigir la cita primaria (AISC 360-22 B4.2; Manual, parte 1) | aceptar DERIVED / exigir cita | Aceptar DERIVED: ninguna de las dos se escribe en la fila |
| D-03 | E = 29 000 ksi: confirmar la cita de AISC 360-22 (gratuita: el Owner la consulta o autoriza su descarga con nombre, origen y tamaño) o aceptarla como convención de ingeniería | cita / convención | Confirmar la cita (es el único valor normativo que se escribe y su fuente es gratuita) |
| D-04 | ν = 0.30 como convención de modelado (cierre definitivo de R-2) con su cita (EN 1993-1-1, 3.2.6, u otra) o sin cita primaria | con cita / convención | Aceptar como convención y nombrar la cita si el Owner la consulta |
| D-05 | Peso unitario 490 pcf: confirmar la tabla del Manual o aceptar la base DERIVED (C-5) | cita / DERIVED | Cualquiera de las dos; DIF-01 no cambia |
| D-06 | Ediciones de ASTM a citar: las vigentes (A992/A992M-22; A500/A500M-23) o las que referencia AISC 360-22; Fy/Fu siguen informativos | vigentes / referenciadas | Vigentes, salvo que el Owner fije las de AISC 360-22 |
| D-07 | Oráculo definitivo: `poissonRatio` 0.3 y `shearModulus` null | ν / G | ν |
| D-08 | Texto `source` de cada fila: variante A o B de §6, literal | A / B / texto propio | B si D-03..D-05 no se cierran con cita; A si se cierran |
| D-09 | `verifiedBy` y `verifiedOn` literales | — | Solo el Owner |
| D-10 | Orden de F4-D y habilitación de la reserva (Codex tope 48, `max_attempts` 9, hasta 3 Workers, 1 revisión del Architect), sin reiniciar contadores | autorizar / diferir | Tras D-01..D-09 |

## 9. Condiciones exactas para autorizar F4-D

Todas, con el texto literal del Owner custodiado:

| # | Condición |
|---|---|
| C-F4D-01 | D-01 a D-07 aprobadas expresamente |
| C-F4D-02 | Cada PENDING de los valores que se escriben (E, ν, peso) cerrado con una cita primaria confirmada por el Owner **o** aceptado expresamente como convención o como base DERIVED; ningún valor se apoya solo en RAM |
| C-F4D-03 | Texto `source` literal aprobado para cada fila (D-08), de una línea e imprimible |
| C-F4D-04 | `verifiedBy` y `verifiedOn` literales dados por el Owner (D-09) |
| C-F4D-05 | Valores exactos a escribir en las dos filas: `ramSection.table` `W` / `HSS_SQR`; `ramSection.country` `United States`; `elasticModulus` `{ "value": 29000, "unit": "ksi" }`; `poissonRatio` `0.3`; `shearModulus` `null`; `unitWeight` `{ "value": 490, "unit": "pcf" }`; `status` `verified`. Ningún otro campo cambia |
| C-F4D-06 | DIF-01 sigue informativa; ninguna tolerancia cambia; `unitWeight` no toma el valor de RAM (D-INV-03 del [contrato](I-65-f4-d-mapping-verified-contract.md)) |
| C-F4D-07 | La limitación DIF-04 figura en `source` |
| C-F4D-08 | Orden del Owner que emita el [contrato F4-D](I-65-f4-d-mapping-verified-contract.md) (rutas §3, invariantes §4, cadena §5) y habilite la reserva (D-10) |
| C-F4D-09 | Siguen vigentes: controles K1-K6; sin RAM; sin AutoCAD (F4-D no lo necesita); sin código de I-52, I-62, I-64 ni ID30; sin cambios del Freeze; sin F5, F6 ni F7 |

**Consecuencia que el Owner debe conocer antes de ordenar F4-D:** la tabla se copia a la salida de compilación, así que F4-D cambia el
binario. El smoke aceptado cubre el DLL `2e04a4bb…e41d` con filas `proposed` (rechazo seguro) y **no** cubre un DLL con filas
`verified`. Ese segundo smoke (camino `Valid`, OV-I65-03) y la primera corrida en RAM serán decisiones aparte.

## 10. Lo que no se hizo

- No cambió ninguna fila ni ningún archivo de producción; no hubo Codex (41 de 46), Workers ni intentos (6 de 6).
- No se abrió AutoCAD ni RAM; no se descargó ningún archivo; no se declaró Candidato de hito ni GATE PASS de F4.
- El checkout principal solo se leyó (FP-1) y sigue limpio.
