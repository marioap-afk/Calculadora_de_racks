# I-65 — F4: hoja de validación de ingeniería de secciones y materiales

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Engineering Validation Preparation»
([texto literal](../automation/decisions/I-65-owner-disposition-f4-engineering-validation-preparation.original.txt); decisiones
[§47](../automation/decisions/I-65.md)). Desarrolla la matriz §1 del
[paquete de validación de ingeniería](I-65-f4-engineering-validation-package.md) como **hoja por fila lista para firmar**. No cambia el
Freeze, ADR-0050 ni ninguna tolerancia.

> **Estado: PREPARADA, SIN FIRMA.** Ninguna fila es `verified` y esta hoja no la cambia: las dos filas de
> `assets/structural-analysis/ram-elements/section-material-map.json` siguen `proposed` hasta la **validación expresa de ingeniería**
> (disposición §2) y un cambio autorizado de la tabla (§7). Los valores marcados `[CANDIDATO]` y `[TERCEROS]` no son valores de
> ingeniería: los confirma o corrige el ingeniero con su cita. RAM no se abrió para preparar esta hoja (RAM EXECUTION = NO).

**Leyenda de origen** (la del paquete, más `[TERCEROS]`):

| Marca | Significado |
|---|---|
| `[CATÁLOGO]` | Catálogo neutral del repositorio: AISC Shapes Database v16.0 (`assets/catalogs/structural-sections-w.csv` y `structural-sections-hss-rect.csv`; archivo fuente SHA-256 `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496`) |
| `[PRE-LECTURA]` | Archivos de la base instalada de RAM leídos en pasivo el 2026-10-11, sin abrir RAM ([ram-database-preread.md](../automation/evidence/I-65-f4-prep/ram-database-preread.md)); **no** es lo que muestra la interfaz de RAM en una corrida |
| `[P1]` | Medido en RAM Elements 26.00.01.015 en la sonda P1 ([p1-results.md](../automation/evidence/I-65-f0/p1-results.md)) |
| `[CANDIDATO]` | Valor esperado propuesto por el Coordinator con una fuente candidata; **requiere confirmación y cita del ingeniero** |
| `[TERCEROS]` | Corroboración preliminar del Coordinator en fuentes públicas de terceros (2026-10-14, §4.2); **no es fuente autoritativa** ni sustituye la cita del ingeniero |
| `[INGENIERÍA]` | Lo anota, decide o firma el ingeniero verificador |
| `[OWNER]` | Decisión del Owner |

## 1. Reglas de la hoja

1. **Tolerancias (Freeze §4, sin cambio):** propiedades de sección A, Ix, Iy y J frente al catálogo ±0.5 % (fila informativa mientras
   rija OD-F4-03, provisional); material: E y ν exactas; peso unitario ±0.5 %. Las demás propiedades de esta hoja son **informativas**.
2. **DIF-01** (peso unitario −0.62 %) queda **reconocida** por el Owner como diferencia informativa aceptada **solo** para la primera
   validación de equivalencia sin peso propio (disposición §3). La fila sigue fuera de ±0.5 %: la tolerancia no cambia y la aceptación no
   vale para ningún análisis que dependa del peso propio.
3. Los valores **esperados** llevan fuente de ingeniería citada y **nunca** se copian de la base de RAM (D-07). La columna «RAM» viene de
   la pre-lectura; el ingeniero la confirma en la interfaz de RAM solo si el Owner autoriza abrir RAM para ello (OD-F4-02, paso B del
   paquete §3).
4. La coincidencia entre RAM y el catálogo es **consistencia**, no independencia: la base de RAM también deriva de AISC (`IsmName` con
   organización `AISC`). La independencia la dan las fuentes de §4.1 que cita el ingeniero.
5. Cada columna «Fuente independiente» y «Verificado» la llena el ingeniero; la sesión del Coordinator mide hashes y fechas de las
   capturas que se custodien (no se le piden al ingeniero datos de máquina).

## 2. Fila 1: `AISC-W-W12X26` → `W 12X26` → ASTM A992 Gr50

### 2.1 Identidad

| Campo | Valor | Origen | Confirmación `[INGENIERÍA]` |
|---|---|---|---|
| Id de RackCad | `AISC-W-W12X26` (familia `W`, designación EDI `W12X26`, etiqueta del Manual `W12X26`) | `[CATÁLOGO]` | — |
| Nombre de sección en RAM | `W 12X26` | `[P1]` S1 | ☐ |
| Tabla, archivo y país de RAM | tabla `W` (`Sections/United States/W.sec`, etiqueta `[W.leo]`), `United States`; `IsmName` `W12X26`, `AISC`; nombre único en todas las bases instaladas | `[PRE-LECTURA]` | ☐ |
| Material de la fila | `ASTM-A992-GR50` → `A992 Gr50` (`Materials/United States/Steel.mat`, nombre único) | fila `proposed`; `[PRE-LECTURA]` | ☐ |
| Edición de AISC de la base de RAM | no declarada en sus archivos (DIF-04) | `[PRE-LECTURA]` | anotar: ________ |
| Bases de usuario con el mismo nombre (p. ej. «Montilla») | desconocido (DIF-03) | `[PRE-LECTURA]` | anotar: ________ |

### 2.2 Propiedades geométricas

Unidades: in, in², in³, in⁴, in⁶ y lb/ft. Δ = (RAM − catálogo) / catálogo.

| Propiedad | Catálogo AISC v16.0 `[CATÁLOGO]` | Fuente independiente `[INGENIERÍA]` | RAM `[PRE-LECTURA]` | Δ RAM | Criterio | Verificado |
|---|---|---|---|---|---|---|
| Peso nominal (lb/ft) | 26 | | no está en el archivo | — | informativo | ☐ |
| A (in²) | 7.65 | | `area` 7.65 | 0 % | ±0.5 % (§4) | ☐ |
| d (in) | 12.2 | | 12.2 | 0 | informativo | ☐ |
| bf (in) | 6.49 | | 6.49 | 0 | informativo | ☐ |
| tw (in) | 0.23 | | 0.23 | 0 | informativo | ☐ |
| tf (in) | 0.38 | | 0.38 | 0 | informativo | ☐ |
| kdes (in) | 0.68 | | `k` 0.68 | 0 | informativo | ☐ |
| Ix (in⁴) | 204 | | `I33loc` 204 | 0 % | ±0.5 % (§4) | ☐ |
| Sx (in³) | 33.4 | | `w33top` = `w33bot` 33.4 | 0 | informativo | ☐ |
| Zx (in³) | 37.2 | | `Z33loc` 37.2 | 0 | informativo | ☐ |
| rx (in) | 5.17 | | no está en el archivo | — | informativo | ☐ |
| Iy (in⁴) | 17.3 | | `I22loc` 17.3 | 0 % | ±0.5 % (§4) | ☐ |
| Sy (in³) | 5.34 | | `w22top` = `w22bot` 5.34 | 0 | informativo | ☐ |
| Zy (in³) | 8.17 | | `Z22loc` 8.17 | 0 | informativo | ☐ |
| ry (in) | 1.51 | | no está en el archivo | — | informativo | ☐ |
| J (in⁴) | 0.3 | | `J` 0.3 | 0 % | ±0.5 % (§4) | ☐ |
| Cw (in⁶) | 607 | | `cw` 607 | 0 | informativo | ☐ |
| Áreas de corte de RAM (in²) | sin columna en el catálogo | | `Aw2` 2.81; `Aw3` 4.93 | — | informativo | ☐ |

Ejes: en RAM el peralte va a lo largo de `l2` y la flexión fuerte es `M33` (`[P1]` §4): `I33` de RAM corresponde a Ix del catálogo e
`I22` a Iy.

### 2.3 Propiedades de material (A992 Gr50)

RAM guarda el material en `Lb-In` (psi y lb/in³). 1 pcf = 1/1728 lb/in³.

| Propiedad | Esperado `[CANDIDATO]` | Fuente candidata (cita exacta `[INGENIERÍA]`) | RAM `[PRE-LECTURA]` | Δ | Criterio §4 | Resultado previsto | Verificado |
|---|---|---|---|---|---|---|---|
| E | 29 000 ksi | ANSI/AISC 360, símbolos (módulo de elasticidad del acero) | 29 000 000 psi | 0 | exacta | coincide | ☐ |
| ν | 0.3 | **sin fuente todavía: la aporta el ingeniero** | 0.3 | 0 | exacta | coincide si su fuente da 0.3 | ☐ |
| G (solo si el oráculo es G) | 11 200 ksi | ANSI/AISC 360, símbolos (módulo de corte del acero) | no se guarda; E/(2(1+ν)) = 11 153.85 ksi | −0.41 % | el Freeze fija «E y ν exactas» | no exacto → DIF-02 | ☐ |
| Peso unitario | 490 pcf (0.2835648 lb/in³) | AISC Steel Construction Manual (tabla a citar) | 0.28179435 lb/in³ = 486.94 pcf (≈ 7800 kg/m³) | **−0.62 %** | ±0.5 % | fuera de tolerancia; **DIF-01 aceptada como informativa** solo para la primera validación sin peso propio | ☐ |
| Fy; Fu (informativo; no es oráculo de §4) | 50 ksi; 65 ksi | ASTM A992/A992M (edición a citar); AISC Manual, tabla de especificaciones ASTM aplicables | 50 000 psi; 65 000 psi | 0 | — | coincide | ☐ |
| α, Ry, Rt (informativo) | no aplica a V1 (sin cargas térmicas ni diseño) | — | α 6.50E-06; Ry 1.1; Rt 1.1 | — | — | solo se registra | ☐ |

## 3. Fila 2: `AISC-HSS-RECT-HSS4X4X_250` → `HSS_SQR 4X4X1_4` → ASTM A500 GrC

### 3.1 Identidad

| Campo | Valor | Origen | Confirmación `[INGENIERÍA]` |
|---|---|---|---|
| Id de RackCad | `AISC-HSS-RECT-HSS4X4X_250` (familia `HSS-RECT`, designación EDI `HSS4X4X.250`, etiqueta `HSS4X4X1/4`) | `[CATÁLOGO]` | — |
| Nombre de sección en RAM | `HSS_SQR 4X4X1_4` (RAM guarda los tubos cuadrados en una tabla distinta de los rectangulares; D-06) | `[P1]` S1 | ☐ |
| Tabla, archivo y país de RAM | tabla `HSS_SQR` (`Sections/United States/HSS_SQR.sec`, etiqueta `[HSS_Rect.leo]`), `United States`; `IsmName` `HSS4X4X1/4`, `AISC`; nombre único | `[PRE-LECTURA]` | ☐ |
| Material de la fila | `ASTM-A500-GRC` → `A500 GrC rectangular` (`Steel.mat`, nombre único; `CSA_A500 GrC rectangular` de Canadá es otro nombre) | fila `proposed`; `[PRE-LECTURA]` | ☐ |
| Edición de AISC de la base de RAM | no declarada (DIF-04) | `[PRE-LECTURA]` | anotar: ________ |
| Bases de usuario con el mismo nombre | desconocido (DIF-03) | `[PRE-LECTURA]` | anotar: ________ |

### 3.2 Propiedades geométricas

| Propiedad | Catálogo AISC v16.0 `[CATÁLOGO]` | Fuente independiente `[INGENIERÍA]` | RAM `[PRE-LECTURA]` | Δ RAM | Criterio | Verificado |
|---|---|---|---|---|---|---|
| Peso nominal (lb/ft) | 12.21 | | no está en el archivo | — | informativo | ☐ |
| Ht; B (in) | 4; 4 | | `a` 4; `b` 4 | 0 | informativo | ☐ |
| tnom (in) | 0.25 | | — | — | informativo | ☐ |
| tdes (in) | 0.233 | | `t` 0.233 | 0 | informativo; RAM usa el espesor de **diseño**, como el catálogo | ☐ |
| h; b planos (in) | 3.3; 3.3 | | — | — | informativo | ☐ |
| A (in²) | 3.37 | | `area` 3.37 | 0 % | ±0.5 % (§4) | ☐ |
| Ix = Iy (in⁴) | 7.8 | | `I33loc` = `I22loc` 7.8 | 0 % | ±0.5 % (§4) | ☐ |
| Sx = Sy (in³) | 3.9 | | `w33top` = `w22top` 3.9 | 0 | informativo | ☐ |
| Zx = Zy (in³) | 4.69 | | `Z33loc` = `Z22loc` 4.69 | 0 | informativo | ☐ |
| rx = ry (in) | 1.52 | | no está en el archivo | — | informativo | ☐ |
| J (in⁴) | 12.8 | | `J` 12.8 | 0 % | ±0.5 % (§4) | ☐ |
| C (in³) | 6.56 | | sin columna | — | informativo | ☐ |
| Cw (in⁶) | sin columna en el catálogo | | `cw` 0 | — | informativo | ☐ |

### 3.3 Propiedades de material (A500 GrC, conformado cuadrado o rectangular)

| Propiedad | Esperado `[CANDIDATO]` | Fuente candidata (cita exacta `[INGENIERÍA]`) | RAM `[PRE-LECTURA]` | Δ | Criterio §4 | Resultado previsto | Verificado |
|---|---|---|---|---|---|---|---|
| E | 29 000 ksi | ANSI/AISC 360, símbolos | 29 000 000 psi | 0 | exacta | coincide | ☐ |
| ν | 0.3 | **sin fuente todavía: la aporta el ingeniero** | 0.3 | 0 | exacta | coincide si su fuente da 0.3 | ☐ |
| G (solo si el oráculo es G) | 11 200 ksi | ANSI/AISC 360, símbolos | E/(2(1+ν)) = 11 153.85 ksi | −0.41 % | «E y ν exactas» | no exacto → DIF-02 | ☐ |
| Peso unitario | 490 pcf | AISC Steel Construction Manual (tabla a citar) | 0.28179435 lb/in³ = 486.94 pcf | **−0.62 %** | ±0.5 % | DIF-01 aceptada como informativa (mismo alcance limitado) | ☐ |
| Fy; Fu (informativo) | 50 ksi; 62 ksi | ASTM A500/A500M, Grado C, perfiles conformados (edición a citar); AISC Manual, tabla de especificaciones ASTM aplicables | 50 000 psi; 62 000 psi | 0 | — | coincide | ☐ |
| α, Ry, Rt (informativo) | no aplica a V1 | — | α 6.50E-06; Ry 1.3; Rt 1.2 | — | — | solo se registra | ☐ |

## 4. Fuentes técnicas

### 4.1 Fuentes independientes que cita el ingeniero

| # | Fuente | Para qué | Estado |
|---|---|---|---|
| F-01 | AISC Steel Construction Manual, 16.ª ed.: tablas de dimensiones y propiedades de perfiles W y de HSS cuadrados | geometría de las dos filas | cita y página `[INGENIERÍA]` |
| F-02 | AISC Shapes Database v16.0 (archivo original de AISC; el catálogo del repositorio es su importación, SHA-256 arriba) | geometría; trazabilidad del catálogo | `[CATÁLOGO]`; el ingeniero puede contrastarlo con F-01 |
| F-03 | ANSI/AISC 360 (edición vigente que cite el ingeniero), símbolos E y G | E; G si el oráculo es G | cita `[INGENIERÍA]` |
| F-04 | ASTM A992/A992M (edición a citar) | Fy, Fu de A992 (informativo) | cita `[INGENIERÍA]` |
| F-05 | ASTM A500/A500M (edición a citar), Grado C conformado | Fy, Fu de A500 Gr C (informativo) | cita `[INGENIERÍA]` |
| F-06 | Fuente del coeficiente de Poisson ν = 0.3 | oráculo ν | **falta**; la aporta el ingeniero (si su fuente solo da G, aplica DIF-02) |
| F-07 | Fuente del peso unitario 490 pcf (AISC Manual) | DIF-01 | cita `[INGENIERÍA]` |
| F-08 | Tablas de fabricante o del Steel Tube Institute para HSS A500 (p. ej. el PDF de Atlas Tube «Atlas HSS Section Properties A500 Squares») | contraste independiente de la HSS | opcional `[INGENIERÍA]` |

### 4.2 Corroboración preliminar del Coordinator `[TERCEROS]` (2026-10-14; no autoritativa)

- **W12X26:** el resumen del buscador de una base pública de terceros (beamdimensions.com, página W12x26) coincide con el catálogo en A
  7.65, Ix 204, Iy 17.3, J 0.3, Cw 607, Zx 37.2, Zy 8.17, d 12.2 y tw 0.23. La página no se leyó directamente (solo el resumen del
  buscador).
- **HSS4X4X1/4:** **sin corroborar.** El PDF de Atlas Tube no fue legible (flujos comprimidos) y la página de terceros respondió 403. El
  ingeniero la contrasta con F-01 o F-08.
- **A500 Gr C conformado, Fy 50 / Fu 62 ksi:** coincide con las páginas del Steel Tube Institute citadas por el buscador (referencia a
  A500-21). La cita de la norma la fija el ingeniero (F-05).

## 5. Diferencias y tolerancias (resumen)

| Fila | Propiedad | Δ | Tolerancia (Freeze §4) | Estado |
|---|---|---|---|---|
| W12X26 y HSS4X4X1/4 | A, Ix, Iy, J (RAM frente al catálogo) | 0 % | ±0.5 % (informativa con OD-F4-03) | dentro; confirmar en la interfaz `[INGENIERÍA]` |
| ambas | E | 0 | exacta | dentro, si la fuente da 29 000 ksi |
| ambas | ν | 0 | exacta | dentro, si la fuente da 0.3 (DIF-02 abierta) |
| ambas | G (solo si el oráculo es G) | −0.41 % | el Freeze no fija tolerancia para G | DIF-02 → `[OWNER]` |
| ambas | Peso unitario | −0.62 % | ±0.5 % | fuera; **DIF-01 aceptada como informativa** solo para la primera validación sin peso propio |
| ambas | Edición de AISC de la base de RAM | — | — | DIF-04 abierta |
| ambas | Bases de usuario («Montilla») | — | — | DIF-03 abierta |

## 6. Firma por fila `[INGENIERÍA]`

Una tarjeta por fila. «Resultado» es uno de: **CONFORME**, **CONFORME CON DIFERENCIAS RECONOCIDAS** (lista de DIF y su reconocimiento
del Owner) o **NO CONFORME** (motivo).

| Campo | `AISC-W-W12X26` | `AISC-HSS-RECT-HSS4X4X_250` |
|---|---|---|
| Verificador (`verifiedBy`) | | |
| Fecha (`verifiedOn`, `yyyy-MM-dd`) | | |
| Cómo se leyó RAM (interfaz con autorización del Owner, o solo la pre-lectura) | | |
| Tabla y país de RAM confirmados | | |
| Edición de AISC de la base de RAM (DIF-04) | | |
| Bases de usuario revisadas y resultado (DIF-03) | | |
| Oráculo de material elegido (ν o G) y su fuente | | |
| Citas: geometría; E; ν o G; peso unitario; Fy y Fu | | |
| Capturas custodiadas (nombre de archivo; la sesión mide el SHA-256) | | |
| Resultado | | |
| Firma | | |

## 7. Después de la firma (fuera de esta preparación)

1. La firma **no** cambia la tabla por sí sola. Escribir las filas `verified` (forma del paquete §1.6) es un **cambio de datos de F4
   (F4-D)** que exige una autorización de implementación nueva del Owner con presupuesto propio: `attempts` está en 6 de 6.
2. Ese cambio debe pasar T4-18 (filas `verified` completas → `Valid`; un campo alterado → otra `AnalysisInputFingerprint`) y producir un
   DLL nuevo (la tabla se copia a la salida de compilación, CD-I65-F4-10). Solo entonces `RACKRAMPAQUETE` deja de responder
   `Unsupported` por `MATERIAL-ROW-UNVERIFIED`.
3. Mientras tanto toda exportación real es `Unsupported` (fallo cerrado, D-07).
