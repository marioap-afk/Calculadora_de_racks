# I-65 — F4: trazabilidad de las fuentes estructurales (R-3)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «AutoCAD Smoke Pre-Release and Engineering Mapping
Decisions» §3 ([texto literal](../automation/decisions/I-65-owner-disposition-smoke-prerelease-mapping-decisions.original.txt); decisiones
[§54](../automation/decisions/I-65.md)). Completa las fuentes de las [propiedades finales](I-65-f4-final-properties-for-approval.md).

> **Estado: TRAZABILIDAD DOCUMENTAL, SIN APROBACIÓN.** Ninguna propiedad ni material queda verificado. `source`, `verifiedBy` y `verifiedOn`
> no tienen valores definitivos (R-4/R-5). Las fuentes «secundarias» son corroboración pública consultada en la web el 2026-10-14; no
> sustituyen la cita de la fuente primaria, que confirma el Owner.

## 1. Clases de fuente

| Clase | Significado |
|---|---|
| **P** | Fuente primaria normativa o del fabricante del dato (AISC, ASTM, CEN); **cita exacta pendiente de confirmar por el Owner** salvo que se indique |
| **S** | Corroboración secundaria pública (artículo, documento de agencia, documentación de software); no autoritativa |
| **D** | Derivación analítica independiente de I-65 a partir de dimensiones ([script](../automation/evidence/I-65-f4/section-props-analytic-check.py.txt), [salida](../automation/evidence/I-65-f4/section-props-analytic-check.out.txt)) |
| **R** | Base instalada de RAM (lectura pasiva y M-05c); **no** es independiente de AISC |

## 2. Trazabilidad por propiedad

| # | Propiedad | Valor propuesto | P (cita por confirmar) | S (corroboración) | D / R | Estado |
|---|---|---|---|---|---|---|
| T-01 | E (ambas filas) | 29 000 ksi | ANSI/AISC 360-22, lista de símbolos | nomenclatura de los «Properties Viewer» de AISC (E = 29 000 ksi); borrador público AISC N690-24 (29 000 ksi) | R: 29 000 000 psi | cita P pendiente |
| T-02 | ν (ambas filas; R-2 provisional) | 0.30 | EN 1993-1-1, apartado 3.2.6, o una tabla del Manual AISC | ayuda de Bentley RAM SS (ν = 0.3; es del proveedor) | R: 0.3 | cita P pendiente |
| T-03 | G (solo informativo) | 11 200 ksi | ANSI/AISC 360-22, lista de símbolos | nomenclatura de los «Properties Viewer» de AISC (G = 11 200 ksi) | E/(2(1+ν)) = 11 153.85 ksi (−0.41 %) | informativo; no se escribe |
| T-04 | Peso unitario esperado | 490 pcf | AISC Steel Construction Manual, 16.ª ed. (tabla por localizar) | Iowa DOT, instrucción 569 (490 lb/ft³ para acero estructural) | R: 486.94 pcf (DIF-01, informativa) | cita P pendiente |
| T-05 | Geometría W12X26: A, Ix, Iy, Sx, Zx | 7.65; 204; 17.3; 33.4; 37.2 | Manual AISC 16.ª ed., tabla de perfiles W (Tabla 1-1) | resumen público de terceros coincidente | D: desvíos ≤ 0.24 % con dimensiones redondeadas; R: idénticos | cita P pendiente |
| T-06 | **J de W12X26** | 0.300 in⁴ | Manual AISC (Tabla 1-1); fórmula de El Darwish y Johnston (1965), recogida por AISC en la Design Guide 9 | CISC: el J de perfiles laminados se basa en El Darwish y Johnston; la suma simple de rectángulos subestima J | **D: 0.29994 in⁴ (−0.02 %)** con la fórmula con filetes (r = kdes − tf = 0.30 in; corrección de alas 0.21·tf⁴ por ala). Con otra lectura de la corrección (0.42·tf⁴ por ala) da 0.29119 (−2.9 %): la forma exacta se confirma en la fuente P | fórmula y cita P pendientes |
| T-07 | Geometría HSS 4X4X1/4: A, Ix = Iy, J, S, Z, r, C | 3.37; 7.8; 12.8; 3.9; 4.69; 1.52; 6.56 | Manual AISC 16.ª ed., tabla de HSS cuadrados (Tabla 1-12) | — (no se obtuvo corroboración pública legible) | **D: ≤ 0.15 %** con tdes 0.233 y radio exterior 2·tdes | cita P pendiente |
| T-08 | Espesor de diseño de HSS A500 | tdes = 0.93·tnom = 0.2325 → tabulado 0.233 in | ANSI/AISC 360, apartado B4.2 (en 360-16: 0.93·tnom para HSS soldados por resistencia eléctrica; A1085 usa el nominal) | Steel Tube Institute (A500: 0.93; A1085: nominal); documentación de RISA | R: `t` 0.233 | cita P (edición 360-22) pendiente |
| T-09 | Convención de esquinas de HSS | radio exterior 2·tdes | Manual AISC, parte 1 (supuesto de cálculo de las propiedades tabuladas), por confirmar | una tabla de terceros da tolerancias reales de 1.6t a 3t para A500: el radio real varía; la convención solo fija el cálculo | D: con 2·tdes los desvíos son ≤ 0.15 %; con 1.5·tdes, hasta +2.3 % | **cita P pendiente; supuesto respaldado solo por la coherencia numérica** |
| T-10 | A992 Gr50: Fy; Fu | 50 ksi (máx. 65); 65 ksi; Fy/Fu ≤ 0.85 | ASTM A992/A992M (edición por fijar); Manual AISC, Tabla 2-4 | AISC SteelWise (2015): A992 preferida para W, Fy 50 y Fu 65; artículo de Modern Steel Construction (1999) con los requisitos de A992; Wikipedia | R: 50 000; 65 000 psi | edición P pendiente; informativo en la fila |
| T-11 | A500 Gr C conformado: Fy; Fu | 50 ksi; 62 ksi | ASTM A500/A500M (edición por fijar; A500-21 según STI) | Steel Tube Institute (Grado C 50/62 para cuadrados, rectangulares y redondos desde A500-21) | R: `A500 GrC rectangular` 50 000; 62 000 psi (`A500 GrC rounded` de RAM conserva 46 000) | edición P pendiente; informativo en la fila |

## 3. Distinciones que la aprobación debe respetar (R-1, R-2)

- **R-1 (aceptación condicional del Owner):** la coherencia entre las tablas instaladas de RAM y AISC v16.0 se usa como evidencia para el
  piloto. **No** se declara verificada la edición formal de la base de RAM.
- **R-2 (provisional):** ν = 0.30 y E = 29 000 ksi son los candidatos comunes de las dos filas. Ningún material se marca verificado hasta
  cerrar las citas T-01, T-02 y T-04.

## 4. Lo que falta para cerrar R-3

| # | Pendiente | Cómo se cierra |
|---|---|---|
| P-1 | Citas primarias de E (T-01), ν (T-02) y peso (T-04) | el Owner las confirma con edición, apartado o tabla |
| P-2 | J de W12X26 (T-06) | el Owner contrasta 0.300 en la Tabla 1-1 del Manual o confirma la forma exacta de la fórmula de El Darwish y Johnston |
| P-3 | Geometría de la HSS (T-07) y su convención (T-08, T-09) | el Owner contrasta la Tabla 1-12 del Manual y la regla de B4.2 y del radio de esquina |
| P-4 | Ediciones ASTM (T-10, T-11) | el Owner fija las ediciones de A992 y A500 |

## 5. Fuentes secundarias consultadas (web, 2026-10-14)

- AISC, «AISC Shapes Database v16.0»: https://www.aisc.org/publications/steel-construction-manual-resources/16th-ed-steel-construction-manual/aisc-shapes-database-v16.0/
- Steel Tube Institute, «Understanding HSS Material Specifications»: https://steeltubeinstitute.org/resources/understanding-hss-material-specifications-which-astm-should-i-specify-for-hss/
- Steel Tube Institute, «An Update To A Classic Material Specification; ASTM A1085»: https://steeltubeinstitute.org/resources/update-to-a-classic-material-specification-astm-a1085/
- AISC Modern Steel Construction, SteelWise (2015): https://www.aisc.org/globalassets/modern-steel/steelwise/2015/steelwise.pdf
- AISC Modern Steel Construction (1999), requisitos de A992: https://www.aisc.org/globalassets/modern-steel/archives/1999/04/1999v04_steel_industry.pdf
- Iowa DOT, instrucción 569 (peso del acero): https://ia.iowadot.gov/erl/current/IM/content/569.htm
- CISC, torsión de perfiles laminados (El Darwish y Johnston): https://www.cisc-icca.ca/?p=2172
- AISC N690-24, borrador de revisión pública: https://www.aisc.org/contentassets/29069d551d9b4836a22b1416615b8d0a/n690-24-spec_public-review_5-2-2024.pdf
- Bentley, ayuda de RAM Structural System, «Steel Material Properties»: https://docs.bentley.com/LiveContent/web/RAM%20Structural%20System%20Help%20Complete-v5/en/GUID-DDD9D196-DA44-42FC-9E87-9AE9DC359EFF.html

La Design Guide 9 de AISC no pudo leerse (PDF cifrado). La herramienta de lectura web la guardó en la carpeta de resultados de la sesión;
no se abrió ni se usó.
