# I-65 — F4: propiedades finales de las dos filas para la aprobación del Owner

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «M-05c Acceptance / OD-F4-14 Resolution / Engineering
Next Steps» §5 A ([texto literal](../automation/decisions/I-65-owner-disposition-m05c-acceptance-od-f4-14.original.txt); decisiones
[§53](../automation/decisions/I-65.md)). Consolida la [hoja de validación](I-65-f4-engineering-validation-sheet.md) v2, la
[revisión técnica](I-65-f4-properties-technical-review.md) y lo observado en RAM en M-05c
([custodia](../automation/evidence/I-65-f4/m05c/m05c-report.md)).

> **Estado: LISTA PARA LA APROBACIÓN FINAL DEL OWNER. NINGUNA FILA VERIFICADA.** `verifiedBy` y `verifiedOn` **no** se asignan.
> MAPPING UPDATE = NO: la tabla `section-material-map.json` no cambia. Ninguna propiedad se da por validada porque coincidan AISC y RAM.

## 1. Lo que ya está dispuesto

| Tema | Estado |
|---|---|
| Identidades en RAM | **Reconocidas** por el Owner: United States / `W` / `W 12X26`; United States / `HSS_SQR` / `HSS_SQR 4X4X1_4`; United States / `Steel` / `A992 Gr50` y `A500 GrC rectangular` |
| «Montilla» | no es autoridad; no existe como grupo en la interfaz (M-05c) |
| OD-F4-03 | aceptada: base de RAM como representación, contrastada con fuentes externas |
| DIF-01 | aceptada **solo** como informativa en el piloto sin peso propio; tolerancia intacta |
| Edición formal del catálogo de RAM | **sin verificar** (DIF-04) |

## 2. Fila `AISC-W-W12X26`

### 2.1 Campos que se escribirían en la fila (si el Owner aprueba)

| Campo | Valor propuesto | Base |
|---|---|---|
| `sectionId` | `AISC-W-W12X26` | sin cambio |
| `ramSection.name` / `.table` / `.country` | `W 12X26` / `W` / `United States` | identidad reconocida (M-05c) |
| `materialId` / `ramMaterial` | `ASTM-A992-GR50` / `A992 Gr50` | identidad reconocida; V-07 provisional |
| `expectedMaterial.elasticModulus` | `{ "value": 29000, "unit": "ksi" }` | E del acero, ANSI/AISC 360 |
| `expectedMaterial.poissonRatio` | `0.3` (si el oráculo es ν) | §4, F-06 |
| `expectedMaterial.shearModulus` | `null` (si el oráculo es ν) | — |
| `expectedMaterial.unitWeight` | `{ "value": 490, "unit": "pcf" }` | peso del acero, Manual AISC; DIF-01 informativa |
| `expectedMaterial.source` | texto de §5 (borrador) | citas que confirme el Owner |
| `status` | `verified` **solo tras la aprobación** | — |
| `verifiedBy` / `verifiedOn` | **no asignados** | confirmación expresa del Owner |

### 2.2 Geometría (no se escribe en la fila; es la base de la aprobación)

| Propiedad | Catálogo AISC v16.0 | RAM (archivo instalado) | Derivación independiente | Δ RAM | Criterio | Fuente independiente pendiente |
|---|---|---|---|---|---|---|
| A (in²) | 7.65 | 7.65 | 7.641 (−0.12 %) | 0 % | ±0.5 % | Manual AISC, tabla de perfiles W |
| Ix (in⁴) | 204 | 204 | 203.5 (−0.24 %) | 0 % | ±0.5 % | ídem |
| Iy (in⁴) | 17.3 | 17.3 | 17.33 (+0.16 %) | 0 % | ±0.5 % | ídem |
| J (in⁴) | 0.3 | 0.3 | 0.29994 (−0.02 %) con la fórmula con filetes de El Darwish y Johnston (forma exacta por confirmar; [trazabilidad](I-65-f4-structural-sources-traceability.md) T-06); 0.284 con la suma simple de rectángulos | 0 % | ±0.5 % | Manual AISC, Tabla 1-1 |
| d; bf; tw; tf (in) | 12.2; 6.49; 0.23; 0.38 | idénticos | — | 0 | informativo | ídem |
| Sx; Zx; Sy; Zy (in³) | 33.4; 37.2; 5.34; 8.17 | idénticos | Sx −0.12 %; Zx −0.24 % | 0 | informativo | ídem |
| Cw (in⁶) | 607 | 607 | — | 0 | informativo | ídem |

La derivación usa las dimensiones tabuladas, que están redondeadas, más el filete kdes − tf.

### 2.3 Material A992 Gr50 en RAM

E 29 000 000 psi; ν 0.3; peso 0.28179435 lb/in³ = 486.94 pcf; Fy 50 000 psi; Fu 65 000 psi (informativos, no se escriben en la fila).

## 3. Fila `AISC-HSS-RECT-HSS4X4X_250`

### 3.1 Campos que se escribirían en la fila (si el Owner aprueba)

| Campo | Valor propuesto | Base |
|---|---|---|
| `sectionId` | `AISC-HSS-RECT-HSS4X4X_250` | sin cambio |
| `ramSection.name` / `.table` / `.country` | `HSS_SQR 4X4X1_4` / `HSS_SQR` / `United States` | identidad reconocida (M-05c) |
| `materialId` / `ramMaterial` | `ASTM-A500-GRC` / `A500 GrC rectangular` | identidad reconocida; V-07 provisional; **no** `A500 GrC rounded` |
| `expectedMaterial.elasticModulus` | `{ "value": 29000, "unit": "ksi" }` | ANSI/AISC 360 |
| `expectedMaterial.poissonRatio` / `.shearModulus` | `0.3` / `null` (si el oráculo es ν) | §4, F-06 |
| `expectedMaterial.unitWeight` | `{ "value": 490, "unit": "pcf" }` | Manual AISC; DIF-01 informativa |
| `expectedMaterial.source` | texto de §5 (borrador) | citas que confirme el Owner |
| `status` / `verifiedBy` / `verifiedOn` | `verified` solo tras la aprobación / **no asignados** | — |

### 3.2 Geometría

| Propiedad | Catálogo AISC v16.0 | RAM (archivo instalado) | Derivación independiente (B = 4, tdes = 0.233, radio exterior 2·tdes) | Δ RAM | Criterio | Fuente independiente pendiente |
|---|---|---|---|---|---|---|
| A (in²) | 3.37 | 3.37 | 3.3710 (+0.03 %) | 0 % | ±0.5 % | Manual AISC, tabla de HSS cuadrados, o fabricante |
| Ix = Iy (in⁴) | 7.8 | 7.8 | 7.8008 (+0.01 %) | 0 % | ±0.5 % | ídem |
| J (in⁴) | 12.8 | 12.8 | 12.7805 (−0.15 %) | 0 % | ±0.5 % | ídem |
| S; Z (in³) | 3.9; 4.69 | 3.9; 4.69 | 3.9004; 4.6923 | 0 | informativo | ídem |
| r (in); C (in³) | 1.52; 6.56 | no se guardan | 1.5212; 6.5638 | — | informativo | ídem |
| Ht; B; tnom; tdes (in) | 4; 4; 0.25; 0.233 | 4; 4; —; 0.233 | entradas de la derivación | 0 | informativo | convención tdes = 0.93·tnom y radio 2·tdes por confirmar |

### 3.3 Material A500 GrC rectangular en RAM

E 29 000 000 psi; ν 0.3; peso 486.94 pcf; Fy 50 000 psi; Fu 62 000 psi (informativos). `A500 GrC rounded` (Fy 46 000 psi) es otra
entrada y no se usa.

## 4. Fuentes independientes

| # | Fuente | Para | Estado |
|---|---|---|---|
| F-01 | AISC Steel Construction Manual, 16.ª ed. (tablas de perfiles W y de HSS cuadrados) | geometría de las dos filas; **J de la W12X26**; convención de esquinas de HSS | **cita y página pendientes** |
| F-02 | AISC Shapes Database v16.0 (catálogo del repositorio) | geometría | disponible; no independiente de RAM |
| F-03 | ANSI/AISC 360-22, lista de símbolos | E = 29 000 ksi; G = 11 200 ksi (solo si el oráculo fuera G) | **cita pendiente** |
| F-04 | ASTM A992/A992M | Fy, Fu (informativo) | edición pendiente |
| F-05 | ASTM A500/A500M, Grado C conformado | Fy, Fu (informativo) | edición pendiente |
| F-06 | EN 1993-1-1, apartado 3.2.6, o una tabla del Manual AISC con ν = 0.30 | ν = 0.3 (oráculo recomendado) | **cita pendiente** |
| F-07 | Manual AISC: peso del acero 490 lb/ft³ | peso unitario esperado | **tabla pendiente** |
| F-08 | Derivación analítica independiente ([script](../automation/evidence/I-65-f4/section-props-analytic-check.py.txt)) | geometría de la HSS (concluyente con su convención); plausibilidad de la W | disponible |
| F-09 | Tablas del fabricante (Atlas Tube) o del Steel Tube Institute | contraste de la HSS | opcional |

## 5. Borrador del texto `source` (para corregir; no se escribe sin aprobación)

```text
E 29000 ksi: ANSI/AISC 360-22, Symbols. nu 0.3: <F-06 confirmada>. Unit weight 490 pcf: AISC Steel Construction Manual 16th ed., <tabla F-07>. DIF-01: RAM 486.94 pcf (-0.62 %), informative for the pilot without self weight. Geometry checked against <F-01 / F-08>; RAM United States/<tabla>.
```

## 6. Diferencias pendientes

| # | Diferencia | Estado | Qué falta |
|---|---|---|---|
| DIF-01 | Peso unitario −0.62 % | aceptada como informativa (piloto sin peso propio) | — |
| DIF-02 | ν o G | **ν = 0.30 aceptado provisionalmente** (R-2) | cita F-06 |
| DIF-03 | «Montilla» | resuelta: no es autoridad y no existe en la interfaz | — |
| DIF-04 | Edición formal del catálogo de RAM | **R-1 aceptación condicional**: coherencia con v16.0 como evidencia del piloto; edición formal **no verificada** | — (no declarar verificada la edición) |
| G-J | J de la W12X26 sin contraste independiente concluyente | pendiente | F-01 (Manual) |
| G-HSS | Convención de esquinas de la HSS | pendiente | confirmar radio exterior 2·tdes y tdes = 0.93·tnom en F-01 o F-09 |
| C-ED | Ediciones de ASTM y citas de E y del peso | pendientes | F-03, F-04, F-05, F-07 |
