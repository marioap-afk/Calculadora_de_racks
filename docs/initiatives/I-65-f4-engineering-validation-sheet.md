# I-65 — F4: hoja de validación de ingeniería de secciones y materiales

Documento de la unidad (Coordinator). Versión 2 (2026-10-14), preparada para la **validación del Owner** como verificador provisional
(disposición «F4 Engineering Validation and Host Readiness», [texto literal](../automation/decisions/I-65-owner-disposition-f4-validation-host-readiness.original.txt);
decisiones [§49](../automation/decisions/I-65.md)). La versión 1 está en `71baf43b` (decisiones §47). Desarrolla la matriz §1 del
[paquete de validación de ingeniería](I-65-f4-engineering-validation-package.md). No cambia el Freeze, ADR-0050 ni ninguna tolerancia.

> **Estado: LISTA PARA LA VALIDACIÓN DEL OWNER. SIN FIRMA, SIN FECHA DE APROBACIÓN, SIN VALIDACIÓN FINAL.** Edición formal de la base de RAM: **no verificada** (R-1: la coherencia con AISC v16.0 se usa solo como evidencia del piloto). El Owner asume
> provisionalmente la verificación (OD-F4-02). Nada de esta hoja se le atribuye hasta su confirmación expresa. Las dos filas de
> `assets/structural-analysis/ram-elements/section-material-map.json` siguen `proposed` (MAPPING UPDATE = PENDING ENGINEERING APPROVAL).
> RAM no se abrió para preparar esta hoja.

**Leyenda de origen:**

| Marca | Significado |
|---|---|
| `[CATÁLOGO]` | Catálogo neutral del repositorio: AISC Shapes Database v16.0 (`assets/catalogs/structural-sections-w.csv` y `structural-sections-hss-rect.csv`; archivo fuente SHA-256 `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496`) |
| `[PRE-LECTURA]` | Archivos de la base instalada de RAM leídos en pasivo, sin abrir RAM ([2026-10-11](../automation/evidence/I-65-f4-prep/ram-database-preread.md); [2026-10-14, tablas completas](../automation/evidence/I-65-f4/ram-section-tables-vs-aisc-v16.md)) |
| `[P1]` | Medido en RAM Elements 26.00.01.015 en la sonda P1 ([p1-results.md](../automation/evidence/I-65-f0/p1-results.md)) |
| `[CANDIDATO]` | Valor o cita propuestos por el Coordinator; **el Owner los confirma o corrige** |
| `[TERCEROS]` | Corroboración pública de terceros (2026-10-14); **no autoritativa** |
| `[OWNER]` | Lo confirma, decide o firma el Owner |

## 1. Disposiciones vigentes (decisiones §47 y §49)

| Tema | Disposición del Owner |
|---|---|
| OD-F4-02 | El Owner asume **provisionalmente** la verificación de las dos filas |
| OD-F4-03 | **Aceptada:** la base de RAM representa las propiedades que consume el adaptador, **contrastadas con fuentes de ingeniería externas** |
| DIF-01 | Peso unitario −0.62 %: aceptada **solo** como diferencia informativa del piloto sin peso propio; la tolerancia no cambia |
| DIF-02 | **R-2 (decisiones §54): ν = 0.30 aceptado PROVISIONALMENTE** como candidato común de las dos filas, con E = 29 000 ksi, sujeto a fuentes técnicas verificables ([trazabilidad](I-65-f4-structural-sources-traceability.md) T-01, T-02); ningún material se marca verificado hasta cerrar las citas |
| DIF-03 | La base «Montilla» **no** se usa como fuente de autoridad del piloto mientras no se identifique |
| DIF-04 | **R-1 (decisiones §54), ACEPTACIÓN CONDICIONAL:** la coherencia entre las tablas instaladas de RAM y AISC v16.0 (§4) se usa como evidencia **para el piloto**; **no** se declara verificada la edición formal de la base de RAM |

Tolerancias (Freeze §4, sin cambio): A, Ix, Iy y J de RAM frente al catálogo ±0.5 %; E y ν exactas; peso unitario ±0.5 %.

## 2. Fila 1: `AISC-W-W12X26` → `W 12X26` → ASTM A992 Gr50

### 2.1 Identidad

| Campo | Valor | Origen | Confirmación `[OWNER]` |
|---|---|---|---|
| Id de RackCad | `AISC-W-W12X26` (familia `W`, designación EDI `W12X26`, etiqueta del Manual `W12X26`) | `[CATÁLOGO]` | — |
| Nombre de sección en RAM | `W 12X26` | `[P1]` S1 | ☐ |
| Tabla y país de RAM (valores que se escribirían en `ramSection.table` y `.country`) | `W`; `United States` (`Sections/United States/W.sec`, etiqueta `[W.leo]`; `IsmName` `W12X26`, `AISC`; nombre único) | `[PRE-LECTURA]` | ☐ |
| Material | `ASTM-A992-GR50` → `A992 Gr50` (`Materials/United States/Steel.mat`, nombre único) | fila `proposed`; `[PRE-LECTURA]` | ☐ |

### 2.2 Geometría (Δ = RAM frente al catálogo)

| Propiedad | Catálogo v16.0 | RAM | Δ | Criterio | ☐ `[OWNER]` |
|---|---|---|---|---|---|
| A (in²) | 7.65 | 7.65 | 0 % | ±0.5 % | ☐ |
| Ix (in⁴) | 204 | 204 (`I33loc`) | 0 % | ±0.5 % | ☐ |
| Iy (in⁴) | 17.3 | 17.3 (`I22loc`) | 0 % | ±0.5 % | ☐ |
| J (in⁴) | 0.3 | 0.3 | 0 % | ±0.5 % | ☐ |
| d; bf; tw; tf; kdes (in) | 12.2; 6.49; 0.23; 0.38; 0.68 | 12.2; 6.49; 0.23; 0.38; 0.68 | 0 | informativo | ☐ |
| Sx; Zx; Sy; Zy (in³) | 33.4; 37.2; 5.34; 8.17 | 33.4; 37.2; 5.34; 8.17 | 0 | informativo | ☐ |
| rx; ry (in) | 5.17; 1.51 | no se guardan | — | informativo | ☐ |
| Cw (in⁶) | 607 | 607 | 0 | informativo | ☐ |
| Peso nominal (lb/ft) | 26 | no se guarda | — | informativo | ☐ |

Ejes: `I33` de RAM = Ix del catálogo (flexión fuerte `M33`, `[P1]` §4).

### 2.3 Material A992 Gr50

| Propiedad | Valor esperado `[CANDIDATO]` | Fuente candidata | RAM | Δ | Criterio | Estado |
|---|---|---|---|---|---|---|
| E | 29 000 ksi | ANSI/AISC 360, lista de símbolos (E del acero) | 29 000 000 psi | 0 | exacta | por confirmar |
| ν | 0.3 | ver §5.1 (DIF-02) | 0.3 | 0 | exacta | **por confirmar** |
| G (solo si el oráculo fuera G) | 11 200 ksi | ANSI/AISC 360, lista de símbolos (G del acero) | E/(2(1+ν)) = 11 153.85 ksi | −0.41 % | el Freeze fija «E y ν» | DIF-02 |
| Peso unitario | 490 pcf | AISC Steel Construction Manual (tabla por localizar) | 486.94 pcf (0.28179435 lb/in³) | **−0.62 %** | ±0.5 % | **fuera; DIF-01 aceptada como informativa** |
| Fy; Fu (informativo) | 50; 65 ksi | ASTM A992/A992M (edición por fijar) | 50 000; 65 000 psi | 0 | — | por confirmar |
| α; Ry; Rt (informativo) | no aplica a V1 | — | 6.50E-06; 1.1; 1.1 | — | — | solo se registra |

## 3. Fila 2: `AISC-HSS-RECT-HSS4X4X_250` → `HSS_SQR 4X4X1_4` → ASTM A500 GrC

### 3.1 Identidad

| Campo | Valor | Origen | Confirmación `[OWNER]` |
|---|---|---|---|
| Id de RackCad | `AISC-HSS-RECT-HSS4X4X_250` (familia `HSS-RECT`, EDI `HSS4X4X.250`, etiqueta `HSS4X4X1/4`) | `[CATÁLOGO]` | — |
| Nombre de sección en RAM | `HSS_SQR 4X4X1_4` (tabla de tubos cuadrados, distinta de la rectangular; D-06) | `[P1]` S1 | ☐ |
| Tabla y país de RAM | `HSS_SQR`; `United States` (`Sections/United States/HSS_SQR.sec`, etiqueta `[HSS_Rect.leo]`; `IsmName` `HSS4X4X1/4`, `AISC`; nombre único) | `[PRE-LECTURA]` | ☐ |
| Material | `ASTM-A500-GRC` → `A500 GrC rectangular` (`Steel.mat`, nombre único; `CSA_A500 GrC rectangular` es otro) | fila `proposed`; `[PRE-LECTURA]` | ☐ |

### 3.2 Geometría

| Propiedad | Catálogo v16.0 | RAM | Δ | Criterio | ☐ `[OWNER]` |
|---|---|---|---|---|---|
| A (in²) | 3.37 | 3.37 | 0 % | ±0.5 % | ☐ |
| Ix = Iy (in⁴) | 7.8 | 7.8 (`I33loc` = `I22loc`) | 0 % | ±0.5 % | ☐ |
| J (in⁴) | 12.8 | 12.8 | 0 % | ±0.5 % | ☐ |
| Ht; B (in) | 4; 4 | 4; 4 (`a`; `b`) | 0 | informativo | ☐ |
| tnom; tdes (in) | 0.25; 0.233 | `t` 0.233 (espesor de **diseño**) | 0 frente a tdes | informativo | ☐ |
| S; Z (in³) | 3.9; 4.69 | 3.9; 4.69 | 0 | informativo | ☐ |
| r (in); C (in³) | 1.52; 6.56 | no se guardan | — | informativo | ☐ |
| Peso nominal (lb/ft) | 12.21 | no se guarda | — | informativo | ☐ |

### 3.3 Material A500 GrC (conformado cuadrado o rectangular)

| Propiedad | Valor esperado `[CANDIDATO]` | Fuente candidata | RAM | Δ | Criterio | Estado |
|---|---|---|---|---|---|---|
| E | 29 000 ksi | ANSI/AISC 360, símbolos | 29 000 000 psi | 0 | exacta | por confirmar |
| ν | 0.3 | ver §5.1 (DIF-02) | 0.3 | 0 | exacta | **por confirmar** |
| G (solo si el oráculo fuera G) | 11 200 ksi | ANSI/AISC 360, símbolos | 11 153.85 ksi | −0.41 % | «E y ν» | DIF-02 |
| Peso unitario | 490 pcf | AISC Manual (tabla por localizar) | 486.94 pcf | **−0.62 %** | ±0.5 % | **fuera; DIF-01 aceptada como informativa** |
| Fy; Fu (informativo) | 50; 62 ksi | ASTM A500/A500M, Grado C conformado (edición por fijar); `[TERCEROS]` Steel Tube Institute | 50 000; 62 000 psi | 0 | — | por confirmar |
| α; Ry; Rt (informativo) | no aplica a V1 | — | 6.50E-06; 1.3; 1.2 | — | — | solo se registra |

## 4. Edición del catálogo de RAM (DIF-04)

Evidencia de solo lectura del 2026-10-14 ([custodia](../automation/evidence/I-65-f4/ram-section-tables-vs-aisc-v16.md)), sin abrir RAM:

- Los archivos de la base **no declaran** la edición de AISC (sin cabecera de versión; búsqueda de marcas en la carpeta `Database` sin
  resultados; sin archivos de notas de versión en la raíz de la instalación).
- **Comparación completa** de las tablas de RAM con el catálogo v16.0 del repositorio:
  - `W.sec`: las **289** W del catálogo están en RAM con **0 diferencias** en A, Ix, Iy, J, Cw, d, bf, tw, tf, Zx, Zy, Sx y Sy; RAM tiene
    además 13 W36 que no están en v16.0 (perfiles de ediciones anteriores, p. ej. `W36X230`, `W36X800`).
  - `HSS_SQR.sec`: los **126** HSS cuadrados del catálogo están en RAM con **0 diferencias** en A, Ix, Iy, J, Ht, B, tdes, Z y S; RAM
    tiene además 5 tubos pequeños que no están en v16.0.
- **Lectura del Coordinator:** la base de RAM es un **superconjunto coherente con AISC Shapes Database v16.0** en estas dos tablas
  (todas las filas de v16.0 presentes e idénticas, más filas heredadas). **No es una confirmación formal de la edición:** no se pueden
  descartar ediciones con los mismos valores en estas filas, y las notas de versión de RAM consultadas en la web no la mencionan.
- **Para confirmarla** (decisión del Owner): (a) aceptar esta evidencia como confirmación suficiente para el piloto; o (b) verla en la
  interfaz de RAM (diálogo de la base de secciones o documentación del producto), lo que exige autorizar abrir RAM solo para esa
  lectura; o (c) documentación de Bentley que la declare.

## 5. Fuentes y valores aún por confirmar `[OWNER]`

### 5.1 DIF-02: oráculo ν o G

| Opción | Fuente candidata (cita a confirmar por el Owner) | Efecto |
|---|---|---|
| **ν = 0.3 (recomendada)** | EN 1993-1-1, apartado 3.2.6 (coeficientes del material para aceros estructurales: ν = 0.3 en régimen elástico); o una tabla de propiedades del acero del AISC Steel Construction Manual que dé 0.30, si el Owner la localiza | RAM guarda ν = 0.3 → coincide exacto → **DIF-02 desaparece**. G de AISC 360 queda informativo |
| G = 11 200 ksi | ANSI/AISC 360, lista de símbolos | RAM no guarda G; el implícito es 11 153.85 ksi (−0.41 %) → reconocimiento del Owner |

`[TERCEROS]`: una página de ayuda de Bentley (RAM Structural System) usa ν = 0.3 y G = E/2.6 para AISC; es del proveedor, no es una
fuente independiente.

### 5.2 Lista de confirmaciones

| # | Qué confirma el Owner | Estado |
|---|---|---|
| V-01 | Edición del catálogo de RAM (§4, DIF-04): opción (a), (b) o (c) | **antes de verificar las filas** |
| V-02 | Oráculo de material y su fuente (§5.1, DIF-02) | pendiente |
| V-03 | Cita de E = 29 000 ksi (ANSI/AISC 360, edición) | pendiente |
| V-04 | Cita del peso unitario 490 pcf (AISC Manual, tabla) — DIF-01 ya aceptada como informativa | pendiente |
| V-05 | Citas de Fy/Fu: ASTM A992/A992M y ASTM A500/A500M Gr C, ediciones (informativo) | pendiente |
| V-06 | Geometría de las dos secciones frente a una fuente externa (AISC Manual 16.ª ed. o fabricante). La HSS4X4X1/4 **no** pudo corroborarse en fuentes públicas; la W12X26 coincide en un resumen de terceros | pendiente |
| V-07 | Nombres, tabla y país de RAM de las dos secciones y de los dos materiales (§2.1, §3.1); «Montilla» excluida como autoridad (DIF-03) | pendiente |
| V-08 | Texto de `source` que se escribiría en cada fila (cita breve de E, ν o G y peso unitario) | pendiente |
| V-09 | `verifiedBy` (nombre que el Owner indique) y `verifiedOn` (fecha que el Owner indique, `yyyy-MM-dd`) | **solo con su confirmación expresa** |

## 6. Tarjeta de validación del Owner (sin rellenar)

| Campo | `AISC-W-W12X26` | `AISC-HSS-RECT-HSS4X4X_250` |
|---|---|---|
| Verificador (`verifiedBy`) | *(pendiente de confirmación del Owner)* | *(pendiente)* |
| Fecha (`verifiedOn`) | *(pendiente)* | *(pendiente)* |
| Edición del catálogo de RAM aceptada (V-01) | | |
| Oráculo de material y fuente (V-02) | | |
| Citas (V-03..V-05) | | |
| Geometría contrastada con (V-06) | | |
| Resultado: CONFORME / CONFORME CON DIFERENCIAS RECONOCIDAS (DIF-01) / NO CONFORME | | |
| Confirmación expresa del Owner | | |

Revisión técnica de valores y fuentes (decisiones §50): [I-65-f4-properties-technical-review.md](I-65-f4-properties-technical-review.md).

Con la tarjeta confirmada, el Coordinator presenta el contrato de la actualización a `verified`
([borrador](I-65-f4-d-mapping-verified-contract.md)); no antes.
