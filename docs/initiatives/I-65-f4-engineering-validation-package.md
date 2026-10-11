# I-65 — F4: paquete de validación de ingeniería (preparación)

Documento de la unidad (Coordinator, 2026-10-11), preparado por orden del Owner
([I-65-owner-authorization-f4-preparation.original.txt](../automation/decisions/I-65-owner-authorization-f4-preparation.original.txt)).
Concreta, **dentro del Freeze** (Proposal V3.1 D-04, D-05, D-06, D-07, D-09, D-10, D-10b, D-11, §4, §5 y §15) y de ADR-0050, lo que
el Owner o un ingeniero necesitan para verificar las filas de mapeo, construir a mano `REF-CANT-01` en RAM Elements y ejecutar las OV de
F4 cuando el Owner autorice F4. Las decisiones que lo adoptan están en [decisiones](../automation/decisions/I-65.md) §23.

> **Estado: PREPARACIÓN.** Nada de este documento lo ha verificado un ingeniero. Ninguna fila de mapeo es `verified`. La validación de
> ingeniería **no** está completada. No se ha ejecutado el adaptador en RAM Elements. F4 IMPLEMENTATION AUTHORIZATION = NO.
> Equivalencia ≠ validación estructural (Owner Q-O3): las hipótesis de §2.7 sirven solo para la equivalencia de transferencia.

**Leyenda de origen** (se usa en todas las tablas):

| Marca | Significado |
|---|---|
| `[CÓDIGO]` | Derivado del código y del golden probado: [I-65-f1-ref-cant-01.md](I-65-f1-ref-cant-01.md) §3 y [I-65-f3-ram-elements-adapter.md](I-65-f3-ram-elements-adapter.md) §8, con pruebas en verde en CI (F1 y F3) |
| `[CATÁLOGO]` | Catálogo neutral del repositorio: AISC Shapes Database v16.0 (`assets/catalogs/structural-sections-*.csv`; archivo fuente SHA-256 `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496`) |
| `[P1]` | Medido en RAM Elements 26.00.01.015 en la sonda P1 ([p1-results.md](../automation/evidence/I-65-f0/p1-results.md)) |
| `[PRE-LECTURA]` | Archivos de la base instalada de RAM leídos en pasivo, sin abrir RAM ([ram-database-preread.md](../automation/evidence/I-65-f4-prep/ram-database-preread.md)); **no es** la lectura de la OV |
| `[CANDIDATO]` | Valor propuesto por el Coordinator con una fuente candidata; **requiere confirmación del ingeniero** |
| `[INGENIERÍA]` | Lo decide, confirma o firma el ingeniero |
| `[OWNER]` | Decisión del Owner (lista en [I-65-f4-preparation.md](I-65-f4-preparation.md) §8) |

## 1. Matriz de verificación de secciones y materiales

### 1.1 Estado y reglas

- Las dos filas de `assets/structural-analysis/ram-elements/section-material-map.json` siguen **`proposed`**; esta preparación no las
  modifica. Con la tabla tal como está, toda exportación real es `Unsupported` (fallo cerrado, D-07).
- Una fila pasa a `verified` solo cuando el ingeniero completa la hoja de §1.6 con evidencia (capturas de la interfaz de RAM y fuentes
  citadas), y el cambio del recurso se hace dentro de F4 autorizado.
- Los valores **esperados** de material llevan fuente de ingeniería citada y **nunca** se copian de la base de RAM (D-07; A65-V3-02). La
  columna «RAM» de estas tablas viene de la pre-lectura pasiva y el ingeniero la confirma en la interfaz.
- Criterios de la matriz §4 del Freeze: propiedades de sección A, Ix, Iy, J frente al catálogo ±0.5 % (informativa con Q-O2 = b; todo
  delta mayor exige reconocimiento del Owner); material: E y ν exactas, peso unitario ±0.5 % (diferencia → reconocimiento del Owner o
  cambio de fila).

### 1.2 Sección `AISC-W-W12X26`

| Campo | Esperado | Origen | RAM (pre-lectura) | Δ | Estado |
|---|---|---|---|---|---|
| Identidad en RackCad | `AISC-W-W12X26` (familia `W`, designación EDI `W12X26`, etiqueta del Manual `W12X26`) | `[CATÁLOGO]` | — | — | fijo |
| Nombre exacto en RAM | `W 12X26` | `[P1]` S1 (aceptado y leído igual) | existe una sola vez: `Sections/United States/W.sec` | — | confirmar `[INGENIERÍA]` |
| Base, tabla y país de RAM | tabla `W` (archivo `W.sec`, etiqueta `[W.leo]`), país `United States` | `[PRE-LECTURA]` | ídem; `IsmName` `W12X26`, organización `AISC` | — | confirmar en la interfaz `[INGENIERÍA]` |
| Catálogo seleccionado en RackCad | AISC Shapes Database v16.0 | `[CATÁLOGO]` | edición de AISC no declarada en el archivo de RAM | — | edición de la base de RAM: registrar `[INGENIERÍA]` |
| d, bf, tw, tf (in) | 12.2; 6.49; 0.23; 0.38 | `[CATÁLOGO]` | 12.2; 6.49; 0.23; 0.38 | 0 | informativo |
| A (in²) | 7.65 | `[CATÁLOGO]` | `area` 7.65 | 0 % | fila §4 ±0.5 % |
| Ix (in⁴) | 204 | `[CATÁLOGO]` | `I33loc` 204 | 0 % | fila §4 ±0.5 % |
| Iy (in⁴) | 17.3 | `[CATÁLOGO]` | `I22loc` 17.3 | 0 % | fila §4 ±0.5 % |
| J (in⁴) | 0.3 | `[CATÁLOGO]` | `J` 0.3 | 0 % | fila §4 ±0.5 % |
| Sx, Zx, Sy, Zy (in³); Cw (in⁶) | 33.4; 37.2; 5.34; 8.17; 607 | `[CATÁLOGO]` | 33.4; 37.2; 5.34; 8.17; 607 | 0 | informativo |
| Peso nominal | 26 lb/ft | `[CATÁLOGO]` | no está en el archivo | — | informativo |
| Material asignado | `ASTM-A992-GR50` → `A992 Gr50` | fila `proposed` (D-07; `[P1]` S1) | existe una sola vez: `Materials/United States/Steel.mat` | — | confirmar `[INGENIERÍA]` |

Ejes: en RAM el peralte va a lo largo de `l2` y la flexión fuerte es `M33` `[P1]` §4; `I33` de RAM corresponde a Ix del catálogo.

### 1.3 Sección `AISC-HSS-RECT-HSS4X4X_250`

| Campo | Esperado | Origen | RAM (pre-lectura) | Δ | Estado |
|---|---|---|---|---|---|
| Identidad en RackCad | `AISC-HSS-RECT-HSS4X4X_250` (familia `HSS-RECT`, designación EDI `HSS4X4X.250`, etiqueta `HSS4X4X1/4`) | `[CATÁLOGO]` | — | — | fijo |
| Nombre exacto en RAM | `HSS_SQR 4X4X1_4` (RAM guarda los tubos cuadrados en otra tabla que los rectangulares; D-06) | `[P1]` S1 | existe una sola vez: `Sections/United States/HSS_SQR.sec` | — | confirmar `[INGENIERÍA]` |
| Base, tabla y país de RAM | tabla `HSS_SQR` (archivo `HSS_SQR.sec`, etiqueta `[HSS_Rect.leo]`), país `United States` | `[PRE-LECTURA]` | ídem; `IsmName` `HSS4X4X1/4`, `AISC` | — | confirmar en la interfaz `[INGENIERÍA]` |
| Ht, B (in) | 4; 4 | `[CATÁLOGO]` | `a` 4; `b` 4 | 0 | informativo |
| Espesor nominal y de diseño (in) | tnom 0.25; tdes 0.233 | `[CATÁLOGO]` | `t` 0.233 | 0 frente a tdes | RAM usa el espesor de diseño, como el catálogo `[INGENIERÍA]` |
| A (in²) | 3.37 | `[CATÁLOGO]` | `area` 3.37 | 0 % | fila §4 ±0.5 % |
| Ix = Iy (in⁴) | 7.8 | `[CATÁLOGO]` | `I33loc` = `I22loc` 7.8 | 0 % | fila §4 ±0.5 % |
| J (in⁴) | 12.8 | `[CATÁLOGO]` | `J` 12.8 | 0 % | fila §4 ±0.5 % |
| S, Z (in³); C (in³) | 3.9; 4.69; 6.56 | `[CATÁLOGO]` | 3.9; 4.69; sin columna C | 0 | informativo |
| Peso nominal | 12.21 lb/ft | `[CATÁLOGO]` | no está en el archivo | — | informativo |
| Material asignado | `ASTM-A500-GRC` → `A500 GrC rectangular` | fila `proposed` (D-07; `[P1]` S1) | existe una sola vez: `Materials/United States/Steel.mat` | — | confirmar `[INGENIERÍA]` |

### 1.4 Materiales

Valores esperados **candidatos**: el ingeniero confirma el valor y fija la cita exacta (norma, edición, sección o tabla). La pre-lectura
de RAM está en unidades `Lb-In` (psi y lb/in³).

| Propiedad | Esperado `[CANDIDATO]` | Fuente candidata (cita exacta `[INGENIERÍA]`) | `A992 Gr50` en RAM | `A500 GrC rectangular` en RAM | Δ | Criterio §4 | Resultado previsto |
|---|---|---|---|---|---|---|---|
| E | 29 000 ksi | AISC 360, nomenclatura (módulo de elasticidad del acero) | 29 000 000 psi | 29 000 000 psi | 0 | exacta | coincide |
| ν | 0.3 | valor habitual del acero estructural; **fuente a fijar por el ingeniero** (la nomenclatura de AISC 360 no lo define) | 0.3 | 0.3 | 0 | exacta | coincide si el oráculo es ν |
| G (alternativa a ν) | 11 200 ksi | AISC 360, nomenclatura (módulo de corte del acero) | 11 153.85 ksi derivado de E/(2(1+ν)); RAM no lo guarda | ídem | −0.41 % | el Freeze fija «E y ν exactas» | **no** coincide exacto → `[OWNER]` |
| Peso unitario | 490 pcf (0.2835648 lb/in³) | AISC Steel Construction Manual, peso del acero 490 lb/ft³ (tabla a fijar) | 0.28179435 lb/in³ = 486.94 pcf | ídem | **−0.62 %** | ±0.5 % | **fuera de tolerancia** → `[OWNER]` |
| Grado y resistencias (informativo; no es oráculo de §4) | A992: Fy 50, Fu 65 ksi; A500 Gr C conformado: Fy 50, Fu 62 ksi | ASTM A992; ASTM A500 Grado C, perfiles conformados (edición a fijar) | Fy 50 000; Fu 65 000 psi | Fy 50 000; Fu 62 000 psi | 0 | — | coincide |

Notas:

- El peso unitario de RAM para ambos materiales equivale a ≈ 7800 kg/m³. V1 no usa peso propio (LOD-01), así que el delta no afecta la
  transferencia; aun así la fila §4 lo evalúa y su resultado previsto es FAIL salvo reconocimiento del Owner o cambio de la fila
  (decisión OD-F4-04).
- Si el oráculo usara G = 11 200 ksi, la fila no coincidiría exactamente con la de RAM (que guarda ν). Recomendación: oráculo por **ν**
  con fuente citada (decisión OD-F4-04).
- El `A36` de RAM (ν = 0.26, 0.284 lb/in³) es el ejemplo conocido del Freeze: los materiales de la base no comparten valores; cada fila se
  verifica por separado.

### 1.5 Diferencias abiertas (resumen)

| # | Diferencia | Magnitud | Consecuencia | Quién |
|---|---|---|---|---|
| DIF-01 | Peso unitario esperado (490 pcf) frente a RAM (486.94 pcf), en ambos materiales | −0.62 % | fila §4 de material fuera de tolerancia | `[OWNER]` reconocer o cambiar la fila |
| DIF-02 | G esperado (11 200 ksi) frente a G implícito de RAM (11 153.85 ksi), solo si se elige G como oráculo | −0.41 % | fila no exacta | `[OWNER]` + `[INGENIERÍA]`: elegir ν |
| DIF-03 | Base de usuario «Montilla» registrada en la configuración de RAM, no localizada | UNKNOWN | un nombre duplicado podría cambiar qué fila usa RAM | `[INGENIERÍA]` revisar (§3 paso B4) |
| DIF-04 | Edición de AISC de la base de RAM no declarada en sus archivos | sin delta en A, Ix, Iy, J | ninguna en la pre-lectura | `[INGENIERÍA]` registrar |

### 1.6 Hoja de verificación por fila (la firma el ingeniero; plantilla)

Para cada fila (`AISC-W-W12X26` y `AISC-HSS-RECT-HSS4X4X_250`):

1. En RAM (File > New, sin analizar ni guardar), abrir la base de secciones de la interfaz y localizar el nombre exacto; anotar **tabla y
   país** y capturar A, Ix (I33), Iy (I22) y J.
2. Comprobar que ninguna otra base (incluida cualquier base de usuario, p. ej. «Montilla») define el mismo nombre; si existe, anotar cuál
   usa RAM.
3. En la base de materiales, localizar el material de la fila y capturar E, ν y peso unitario.
4. Registrar los **valores esperados** de E, ν (o G) y peso unitario con su **fuente citada** (norma o Manual, edición, sección o tabla),
   sin copiarlos de RAM.
5. Calcular los deltas frente al catálogo (sección) y frente a los esperados (material); todo delta fuera de tolerancia se presenta al Owner.
6. Firmar: nombre del verificador (`verifiedBy`) y fecha `yyyy-MM-dd` (`verifiedOn`).

Forma de la fila que F4 escribiría en la tabla tras la firma (esquema `rackcad-ram-elements-section-map/v1`; los `<…>` los llena el
ingeniero; no es un valor propuesto):

```json
{
  "sectionId": "AISC-W-W12X26",
  "ramSection": { "name": "W 12X26", "table": "<tabla de RAM>", "country": "<país de RAM>" },
  "materialId": "ASTM-A992-GR50",
  "ramMaterial": "A992 Gr50",
  "expectedMaterial": {
    "elasticModulus": { "value": "<número>", "unit": "<psi | ksi>" },
    "poissonRatio": "<número, o null si se usa shearModulus>",
    "shearModulus": "<{ value, unit } o null>",
    "unitWeight": { "value": "<número>", "unit": "<pcf | lbf/in3>" },
    "source": "<cita de ingeniería>"
  },
  "status": "verified",
  "verifiedBy": "<verificador>",
  "verifiedOn": "<yyyy-MM-dd>"
}
```

## 2. Especificación manual de `REF-CANT-01` (concreción de §15 del Freeze)

Propósito: construir **a mano** en RAM Elements el modelo con el que el comparador de F4 contrasta el generado (D-11). Todas las
coordenadas están ya en el sistema de RAM (`X_RAM = X`, `Y_RAM = Z`, `Z_RAM = −Y`; Y vertical hacia arriba; la estación vive en el plano
Y-Z de RAM; los brazos van hacia −Z de RAM).

### 2.1 Configuración del modelo

| Paso | Valor | Origen |
|---|---|---|
| Producto | RAM Elements 26.00.01.015 (`RAMElements.exe` SHA-256 `7dcb4855…7970`) | `[P1]` |
| Partida | File > New; no partir de otro modelo; no analizar | Freeze §15 pasos 1 y 9 |
| Unidades de la interfaz | lb-in (pulgadas y libras-fuerza): así el volcado no cambia de par | Freeze §15 paso 10 |
| Caso por defecto | el que crea File > New (`CM` en español): se deja sin cargas; el comparador lo ignora | Freeze D-08, D-11 |

### 2.2 Nudos

Coordenadas en pulgadas, en el sistema de RAM `[CÓDIGO]`. Basta con introducirlas con 4 decimales: la tolerancia de la matriz §4 es
0.02 in. La descripción sugerida es el id neutral (el comparador empareja por posición, no por descripción).

| Nudo (descripción) | X_RAM | Y_RAM | Z_RAM | Y_RAM (4 dec.) | Z_RAM (4 dec.) |
|---|---|---|---|---|---|
| `COL.FOOT` | 0 | 0.25 | 6.1 | 0.2500 | 6.1000 |
| `ARM.1.PositiveY.J` | 0 | 20.26716169689737 | 6.1 | 20.2672 | 6.1000 |
| `ARM.2.PositiveY.J` | 0 | 48.267161696897375 | 6.1 | 48.2672 | 6.1000 |
| `ARM.3.PositiveY.J` | 0 | 76.26716169689738 | 6.1 | 76.2672 | 6.1000 |
| `COL.TOP` | 0 | 90.75 | 6.1 | 90.7500 | 6.1000 |
| `ARM.1.PositiveY.TIP` | 0 | 21.810300688090436 | −36.22609804415268 | 21.8103 | −36.2261 |
| `ARM.2.PositiveY.TIP` | 0 | 49.81030068809044 | −36.22609804415268 | 49.8103 | −36.2261 |
| `ARM.3.PositiveY.TIP` | 0 | 77.81030068809044 | −36.22609804415268 | 77.8103 | −36.2261 |

Derivación (IDL-01..08, F1 §3): eje de la columna W12X26 en neutral x = 0, y = −6.1; pie en la cara superior de la placa inferior
(z = 0.25); cabeza en el fin físico (z = 90.75); nudo brazo–columna = intersección del eje del brazo prolongado con el eje de la columna;
punta = fin físico del brazo. Pendiente del brazo 7/16 por 12 (tan θ = 7/192).

### 2.3 Miembros, perfiles, materiales y orientación

**El sentido I → J importa:** un miembro dibujado al revés invierte `l1` y cambia `l2` y `l3`, y la fila de ejes locales de la matriz §4
falla aunque los nudos coincidan. Columnas de abajo arriba; brazos del nudo de la columna a la punta.

| Miembro (descripción) | Nudo I → nudo J | Longitud (in) | Sección de RAM | Material de RAM | Giro θ | Origen |
|---|---|---|---|---|---|---|
| `COL.1` | `COL.FOOT` → `ARM.1.PositiveY.J` | 20.01716169689737 | `W 12X26` | `A992 Gr50` | **−90°** | `[CÓDIGO]`; sección y material `[INGENIERÍA]` |
| `COL.2` | `ARM.1.PositiveY.J` → `ARM.2.PositiveY.J` | 28 | `W 12X26` | `A992 Gr50` | **−90°** | ídem |
| `COL.3` | `ARM.2.PositiveY.J` → `ARM.3.PositiveY.J` | 28 | `W 12X26` | `A992 Gr50` | **−90°** | ídem |
| `COL.4` | `ARM.3.PositiveY.J` → `COL.TOP` | 14.48283830310262 | `W 12X26` | `A992 Gr50` | **−90°** | ídem |
| `ARM.1.PositiveY` | `ARM.1.PositiveY.J` → `ARM.1.PositiveY.TIP` | 42.35421884050472 | `HSS_SQR 4X4X1_4` | `A500 GrC rectangular` | 0° | ídem |
| `ARM.2.PositiveY` | `ARM.2.PositiveY.J` → `ARM.2.PositiveY.TIP` | 42.35421884050472 | `HSS_SQR 4X4X1_4` | `A500 GrC rectangular` | 0° | ídem |
| `ARM.3.PositiveY` | `ARM.3.PositiveY.J` → `ARM.3.PositiveY.TIP` | 42.35421884050472 | `HSS_SQR 4X4X1_4` | `A500 GrC rectangular` | 0° | ídem |

Ejes locales esperados (regla de RAM medida en P1 §4; tolerancia de la matriz §4: 0.1°) `[CÓDIGO]`:

| Miembros | `l1` | `l2` (peralte) | `l3` |
|---|---|---|---|
| `COL.1`..`COL.4` con θ = −90° | (0, 1, 0) | (0, 0, −1) | (−1, 0, 0) |
| `ARM.k.PositiveY` con θ = 0° | (0, 0.036434127070177715, −0.9993360567820188) | (0, 0.9993360567820188, 0.036434127070177715) | (1, 0, 0) |

- **Por qué −90° en las columnas:** con θ = 0 RAM pone `l2 = (−1, 0, 0)`, es decir, el peralte fuera del plano de la estación (flexión
  débil). Tanto −90° como +90° dejan el peralte en el plano, pero +90° da `l2 = (0, 0, 1)` y `l3 = (1, 0, 0)`: vectores opuestos a los del
  generado, que la fila de ejes rechaza. La especificación **fija −90°** (decisión OD-F4-05). Si la interfaz de RAM muestra el giro como
  270°, es el mismo giro; lo que compara la matriz son los vectores.
- Brazos: θ = 0 deja el peralte casi vertical (flexión fuerte `M33` por cargas verticales); el tubo es cuadrado.
- El ingeniero comprueba en la interfaz que el eje 2 de las columnas apunta a −Z de RAM (hacia los brazos) y el de los brazos hacia arriba.

### 2.4 Apoyos y liberaciones

Banderas en el sistema **global de RAM** (API `dx`, `dy`, `dz`, `rx`, `ry`, `rz`; el registro de análisis de RAM usa nombres del tipo
`Tx Ty Rz`, P1 N1; el ingeniero confirma la etiqueta de la interfaz). Son el resultado de la permutación de D-04 sobre las banderas
neutrales (`dx = Tx`, `dy = Tz`, `dz = Ty`, `rx = Rx`, `ry = Rz`, `rz = Ry`); en la interfaz se marcan directamente las de esta tabla
`[CÓDIGO]`:

| Nudo | Restringidos | Libres | Supuesto |
|---|---|---|---|
| `COL.FOOT` | `dx dy dz rx ry rz` | ninguno | SUP-01 |
| Los otros siete | `dx ry rz` | `dy dz rx` | SUP-03 (análisis plano en Y-Z de RAM) |

- Liberaciones: **ninguna**, en los dos extremos de los siete miembros (CON-01).
- Propiedades de miembro con sus valores por defecto `[P1]` R1: punto cardinal CENTROID, offsets rígidos 0, factores de rigidez 1,
  comportamiento axial normal, resortes 0.

### 2.5 Cargas (alcance congelado: D-02, D-09, D-10b)

Un caso con cargas: id `REF1`, categoría «Other» (`"OTHER"` en el generado) `[CÓDIGO]`. Sin peso propio, sin combinaciones, sin cargas
nodales ni distribuidas. Cada carga es una **carga puntual en miembro**, dirección **global −Y de RAM** (hacia abajo), posición en % del
miembro analítico medida desde el nudo I (`ARM.k.PositiveY.J`).

| Carga | Miembro | Valor (lbf) | Posición (%) | Distancia desde J (in) | Entrada en el archivo de cargas |
|---|---|---|---|---|---|
| `P1` | `ARM.1.PositiveY` | −1000 | 57.50128206169155 | 24.35421884050472 | 1000 `lbf`, fracción 0.5 |
| `P2` | `ARM.2.PositiveY` | −2000 | 36.25192309253731 | 15.354218840504721 | 2 `kip`, fracción 0.25 |
| `P3` | `ARM.3.PositiveY` | −1102.311310924388 | 78.75064103084577 | 33.35421884050472 | 500 `kgf`, fracción 0.75 |

- Origen `[CÓDIGO]`: el archivo de cargas del fixture de F1 (F1 §5), que es el que el Coordinator propone para OV-I65-01 (decisión
  OD-F4-06). La posición = distancia rígida de IDL-06 (6.354218840504721 in) + fracción × 36 in de brazo físico; % = 100 · d / 42.35421884050472.
- Tolerancias de la matriz §4: valor 0.1 %, posición 0.02 in, dirección exacta. `P3` con 1102.31 lbf ya cumple.

### 2.6 Unidades

Interfaz y volcado en lb-in. El generado se escribe en `LB_IN` por la API (`units.set`) sin cambiar las unidades de la interfaz
(`[P1]`: la interfaz siguió en kip-ft). RackCad no convierte longitudes ni fuerzas en el adaptador (D-03); la única conversión de esta
referencia es la de la carga `P3` (kgf → lbf) y la hace la fuente de F1 con `ForceUnits` (gₙ = 9.80665 m/s², lb = 0.45359237 kg).

### 2.7 Idealizaciones e hipótesis (solo para la equivalencia de transferencia)

| ID | Regla o supuesto | Limitación declarada |
|---|---|---|
| IDL-01 | eje analítico = eje centroidal | — |
| IDL-02..05 | nudos de pie, cabeza, brazo–columna y punta | — |
| IDL-06 / IDL-NO-OFFSETS | sin offsets rígidos: el brazo analítico va de J a la punta e incluye media columna y la placa | sobreestima la flexibilidad del brazo; ninguna conclusión de comportamiento real se extrae de V1 |
| IDL-07 / SUP-02 | base no modelada | la base real apoya en el piso |
| IDL-08 | fusión 0.001 in; separación mínima 0.05 in | — |
| SUP-01 | pie de columna empotrado (6 GDL) | sin rigidez de anclaje, placa ni losa |
| SUP-03 | análisis plano: `dx ry rz` de RAM restringidos en todos los nudos | sin pandeo fuera del plano ni arriostramientos |
| CON-01 | brazo–columna continuo | la conexión real es semirrígida |
| ASM-PROP-01 | propiedades de sección y material = base de RAM | ediciones distintas de AISC y valores de material de RAM distintos de los habituales; se comparan en la OV (§1) |
| LOD-01 | un caso `REF1` con las cargas del archivo; sin peso propio ni combinaciones | no deriva de tarimas, impacto, sismo ni tributarias |

Advertencia del manifiesto, literal: «hipótesis solo para la equivalencia de transferencia; no constituyen validación estructural».

### 2.8 Exclusiones explícitas (V1)

No se modelan: base, placas, cartelas, separadores, arriostramientos, barras redondas; brazos `NegativeY`, doble cara, canales pareados,
brazos con más de 10°. No se usan: offsets rígidos, resortes, liberaciones, peso propio, combinaciones, cargas nodales, distribuidas o
locales, más de 24 cargas por miembro. No se hace: análisis, lectura o importación de resultados, diseño (`member_design`), guardar ni
sobrescribir modelos de RAM.

### 2.9 Qué viene del código y qué requiere confirmación de ingeniería

| Elemento | Origen | Requiere confirmación |
|---|---|---|
| Coordenadas, conectividad, sentido I → J, longitudes | `[CÓDIGO]` (golden de F1 y F3) | no |
| Giro θ y ejes locales | `[CÓDIGO]` + regla `[P1]` | no; el sentido de θ de las columnas es OD-F4-05 |
| Banderas de apoyo (permutación D-04) | `[CÓDIGO]` | no |
| Valores y posiciones de carga | `[CÓDIGO]` a partir del archivo de cargas | el archivo para la OV es OD-F4-06 |
| Identidad de sección en RackCad y propiedades del catálogo | `[CATÁLOGO]` | no |
| Nombres de sección y material de RAM | `[P1]` (aceptados) | sí, firma de la fila `[INGENIERÍA]` |
| Tabla y país de RAM | `[PRE-LECTURA]` | sí `[INGENIERÍA]` |
| Asignación de material (A992 Gr50 a la W, A500 Gr C a la HSS) | fila `proposed` | sí `[INGENIERÍA]` |
| E, ν o G y peso unitario esperados con fuente | `[CANDIDATO]` | sí `[INGENIERÍA]`; diferencias `[OWNER]` |
| Hipótesis SUP, CON, IDL, ASM, LOD | Freeze D-10 (Owner Q-O3: solo hipótesis) | no se validan aquí; no constituyen validación estructural |

## 3. Procedimiento de validación en RAM (para cuando el Owner autorice F4)

Nada de esta sección se ejecuta en la preparación. Cada corrida deja su custodia en `docs/automation/evidence/I-65-f4/` (paquetes,
lecturas de vuelta, volcados, informe del comparador, capturas, hash de `RAMElements.exe` y estado de la licencia).

**A. Precondiciones** (todas): F4 autorizada por el Owner; Candidato de hito completo con el DLL según
[la guía manual](../guias/validacion-manual-autocad.md); las dos filas `verified` en el Candidato; DIF-01..03 resueltas; A65-F3-01, 02,
04 y 11 corregidos en el Candidato; RAM 26.00.01.015 con licencia activa (el Owner puede tener que atenderla, P1).

**B. Verificación de las filas** (ingeniero; no ejecuta el adaptador): B1 abrir RAM y File > New; B2 secciones (§1.6 paso 1);
B3 materiales (paso 3); B4 bases de usuario, en particular «Montilla» (paso 2); B5 valores esperados con fuente y deltas (pasos 4-5);
B6 cerrar RAM **sin guardar**; firmar la hoja (paso 6). Puede hacerse antes del Candidato.

**C. Paquete** (Candidato): C1 abrir un DWG con un rack Cantilever con los parámetros de `REF-CANT-01` (F1 §1); C2 ejecutar el comando de
F4: estación 0, archivo de cargas de OD-F4-06, carpeta **nueva** con ruta corta; C3 comprobar diagnósticos vacíos y los tres archivos;
anotar el SHA-256 de cada uno y `AnalysisInputFingerprint`.

**D. Script generado** (OV-I65-01, parte 1): D1 RAM, File > New; comprobar «Elementos: 0» y un solo caso sin cargas; D2 Python Console →
«Ejecutar archivo…» → `rackcad-ram-build.py` de la carpeta del paquete; D3 esperado: `READBACK OK` en consola y `ram-readback.json` con la
misma huella de entrada; D4 ante `FAILED — DISCARD MODEL`: si el detalle empieza por `gate:`, el modelo no se tocó; si no, descartar sin
guardar; en ambos casos STOP y registro, y un intento nuevo exige un **paquete nuevo**; D5 inspección visual: 8 nudos, 7 miembros, columna
vertical, brazos hacia −Z de RAM, cargas hacia abajo, eje 2 de las columnas hacia −Z; D6 ejecutar el **volcado** de F4 antes de analizar;
D7 cerrar sin guardar (salvo decisión del ingeniero, D-11).

**E. Modelo manual** (OV-I65-01, parte 2): E1 RAM, File > New; unidades de la interfaz lb-in; E2 construir §2.1-§2.5; E3 volcado antes de
analizar; si aparece «los resultados se perderán», responder **No** y STOP.

**F. Comparación**: F1 comparador de consola sobre los dos volcados, el manifiesto y la lectura de vuelta; F2 matriz §4 PASS salvo filas
informativas; deltas de §1 registrados; todo delta fuera de tolerancia → reconocimiento del Owner.

**G. Inyecciones de fallo** (paquetes de la herramienta de pruebas, no del producto):

| OV | Acción | Resultado esperado |
|---|---|---|
| OV-I65-03 | En RackCad: estación de doble cara, brazo `NegativeY`, brazo con más de 10°, canales pareados o familia C/L; y una tabla con una fila `proposed` | sin paquete; diagnóstico visible (`Unsupported`/`Invalid`) |
| OV-I65-04 | Ejecutar un paquete válido sobre un modelo **no vacío** (p. ej. el manual) | rechazo `gate:` antes de escribir; conteos de nudos, miembros y casos sin cambio |
| OV-I65-05 | Paquete con un nombre de sección inexistente | `FAILED — DISCARD MODEL` en consola y lectura de vuelta; el comparador lo rechaza |
| OV-I65-06 | Paquete con ejes esperados alterados | `FAILED` |
| OV-I65-07 | Paquete con `__version__` esperada alterada, o que exige una función del puente inexistente | rechazo antes de cualquier escritura; nudos = miembros = 0 |

**H. Paradas:** licencia no disponible; diálogo modal de resultados (respuesta **No**); un `FAILED` fuera de una inyección; un delta fuera
de tolerancia sin reconocimiento; la condición de Q-O1 (transferencia no verificable sin hipótesis incorrectas) → STOP y decisión del Owner.
