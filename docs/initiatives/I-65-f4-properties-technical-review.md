# I-65 — F4: revisión técnica de las propiedades del piloto (pendientes de aprobación)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Host Coordination and Combination Validation» §4
([texto literal](../automation/decisions/I-65-owner-disposition-f4-host-coordination-combination-validation.original.txt); decisiones
[§50](../automation/decisions/I-65.md)). Complementa la [hoja de validación](I-65-f4-engineering-validation-sheet.md) v2.

> **V-01..V-09 siguen pendientes de tu validación.** Esta revisión **no** declara verificada ninguna propiedad. La coincidencia entre el
> catálogo AISC y la base de RAM no cuenta como verificación: RAM también deriva de AISC. DIF-01 sigue siendo una diferencia informativa
> exclusiva del piloto sin peso propio.

## 1. HSS 4X4X1/4 (prioridad 1)

**Derivación analítica independiente** (custodia: [script](../automation/evidence/I-65-f4/section-props-analytic-check.py.txt),
[salida](../automation/evidence/I-65-f4/section-props-analytic-check.out.txt)). Calcula las propiedades a partir de las dimensiones, sin
leer el catálogo ni RAM. Usa un polígono de 4000 segmentos por esquina; J se obtiene con Bredt-Batho sobre la línea media y C con la
fórmula de AISC para HSS rectangulares.

| Propiedad | Catálogo v16.0 | Derivado (B = 4, tdes = 0.233, radio exterior 2·tdes) | Δ | Con tdes = 0.93·0.25 = 0.2325 | Δ |
|---|---|---|---|---|---|
| A (in²) | 3.37 | 3.3710 | +0.03 % | 3.3646 | −0.16 % |
| Ix = Iy (in⁴) | 7.8 | 7.8008 | +0.01 % | 7.7883 | −0.15 % |
| S (in³) | 3.9 | 3.9004 | +0.01 % | 3.8941 | −0.15 % |
| r (in) | 1.52 | 1.5212 | +0.08 % | 1.5214 | +0.09 % |
| Z (in³) | 4.69 | 4.6923 | +0.05 % | 4.6841 | −0.13 % |
| J (in⁴) | 12.8 | 12.7805 | −0.15 % | 12.7578 | −0.33 % |
| C (in³) | 6.56 | 6.5638 | +0.06 % | 6.5517 | −0.13 % |

- Con radio exterior 2·tdes, todo queda dentro de ±0.5 %. Es una comprobación **independiente de los valores tabulados**, pero **no** de
  las convenciones geométricas de AISC: espesor de diseño 0.93·tnom y radio exterior 2·tdes.
- **Sensibilidad:** con radio exterior 1.5·tdes, Ix sube +2.3 %. La conformidad depende de esa convención, que debes confirmar en el
  Manual AISC o en el fabricante.
- Corroboración pública: no se obtuvo (el PDF del fabricante no fue legible y la página de terceros respondió 403).

## 2. W12X26 (contraste de plausibilidad)

Con las dimensiones tabuladas, que están redondeadas (d 12.2, bf 6.49, tw 0.23, tf 0.38, filete kdes − tf = 0.30), y sin leer el
catálogo ni RAM:

| Propiedad | A | Ix | Iy | Sx | Zx | J |
|---|---|---|---|---|---|---|
| Δ derivado frente al catálogo | −0.12 % | −0.24 % | +0.16 % | −0.12 % | −0.24 % | −5.4 % (fórmula simple de paredes delgadas, sin el término del filete) |

Coherente con el catálogo salvo J, que necesita la fórmula completa con filetes o una fuente externa (Manual AISC). Un resumen público de
terceros coincide en A, Ix, Iy, J y Cw.

## 3. E y ν o G (prioridad 2)

| Valor | RAM | Fuente candidata | Observación |
|---|---|---|---|
| E = 29 000 ksi | 29 000 000 psi | ANSI/AISC 360, lista de símbolos | por citar (edición) |
| ν = 0.3 | 0.3 | EN 1993-1-1, apartado 3.2.6 (fuente ajena a AISC y a RAM); o una tabla del Manual AISC que la dé | **oráculo recomendado**: coincide exacto; elimina DIF-02 |
| G = 11 200 ksi | no se guarda (11 153.85 ksi implícito) | ANSI/AISC 360, lista de símbolos | solo si eliges G: −0.41 % → reconocimiento |

En RAM, E, ν y el peso unitario son **iguales en casi todos los aceros** de la base (A992, A500, A1085, A53). Son valores genéricos de la
base, no del grado. Las excepciones son A36 y A500 GrA para tubería (ν 0.26, peso 0.284 lb/in³).

## 4. Edición del catálogo de RAM (prioridad 3)

Las tablas W y HSS_SQR de RAM contienen todas las filas de AISC v16.0 de esas familias con valores idénticos, más filas heredadas
([custodia](../automation/evidence/I-65-f4/ram-section-tables-vs-aisc-v16.md)), y los archivos no declaran la edición. **Pendiente
(V-01):**

- aceptar esa evidencia;
- verla en la interfaz de RAM: puede hacerse en la misma sesión que M-05c si autorizas abrir RAM;
- o documentación de Bentley.

## 5. Correspondencia exacta de materiales (prioridad 4)

Lectura de solo lectura de `Materials/United States/Steel.mat` (mismos bytes que la pre-lectura, SHA-256 `2a8b4239…39ce1`):

| RackCad | Nombre exacto en RAM | Fy; Fu en RAM | Especificación candidata | Entradas cercanas que **no** deben usarse |
|---|---|---|---|---|
| `ASTM-A992-GR50` (W12X26) | `A992 Gr50` | 50; 65 ksi | ASTM A992/A992M: Fy ≥ 50, Fu ≥ 65 ksi | — |
| `ASTM-A500-GRC` (HSS cuadrado = conformado) | `A500 GrC rectangular` | 50; 62 ksi | ASTM A500/A500M Grado C para perfiles conformados (cuadrados y rectangulares): 50; 62 ksi | `A500 GrC rounded` (46; 62 ksi en RAM), `A500 GrB rectangular` (46; 58), `A1085` (50; 65), `CSA_A500 GrC rectangular` (Canadá) |

- El nombre exacto importa: la variante `rectangular` es la que corresponde a un tubo cuadrado.
- Observación informativa: el `A500 GrC rounded` de RAM con 46 ksi refleja ediciones de A500 anteriores a la que, según el Steel Tube
  Institute (A500-21), igualó redondos y conformados a 50 ksi. No afecta al piloto, que no usa tubos redondos, pero indica que la base de
  materiales de RAM no sigue necesariamente la última edición de ASTM.
- Ry y Rt de RAM (1.1 y 1.1; 1.3 y 1.2) son informativos: V1 no diseña.

## 6. Tabla de propiedades pendientes de aprobación

| # | Elemento | Valor propuesto | Evidencia disponible | Falta para aprobar | Recomendación |
|---|---|---|---|---|---|
| V-01 | Edición del catálogo de RAM | AISC v16.0 (superconjunto) | comparación completa de tablas, 0 diferencias | declaración formal | aceptar la evidencia o leer la interfaz en la sesión de M-05c |
| V-02 | Oráculo de material | ν = 0.3 | RAM 0.3; EN 1993-1-1 §3.2.6 (por confirmar) | tu cita | ν = 0.3 |
| V-03 | E | 29 000 ksi | RAM; AISC 360 | cita y edición | aprobar con la cita |
| V-04 | Peso unitario esperado | 490 pcf (DIF-01 informativa) | AISC Manual (tabla por localizar) | cita | aprobar con la cita; DIF-01 sin cambios |
| V-05 | Fy / Fu | A992 50/65; A500 GrC conformado 50/62 | RAM; ASTM (ediciones por fijar); STI | ediciones | informativo |
| V-06a | Geometría HSS 4X4X1/4 | A 3.37; Ix 7.8; J 12.8 | derivación analítica dentro de ±0.16 % (convención 2·tdes) | confirmar la convención y una fuente externa (Manual, Tabla de HSS cuadrados) | aprobar si confirmas la convención |
| V-06b | Geometría W12X26 | A 7.65; Ix 204; Iy 17.3; J 0.3 | derivación de plausibilidad (≤ 0.24 % salvo J); resumen de terceros | J de una fuente externa | aprobar con el Manual |
| V-07 | Nombres, tabla y país de RAM; materiales exactos | §5 y hoja §2.1 / §3.1 | pre-lectura; nombres únicos | confirmación (interfaz opcional) | aprobar; «Montilla» excluida |
| V-08 | Texto de `source` | por redactar con tus citas | — | tus citas | tras V-02..V-05 |
| V-09 | `verifiedBy` / `verifiedOn` | — | — | tu confirmación expresa | solo tú |
