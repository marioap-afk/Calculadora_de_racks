# I-65 — F1: estación de referencia `REF-CANT-01` y golden de INV-06 (derivado a mano)

Documento de la unidad (Coordinator, 2026-10-11). Concreta, **dentro del Freeze** (Proposal V3.1 D-05 «Estación de referencia» e
IDL-01..10, D-10, D-10b, §15; Owner Q-O7 = `REF-CANT-01` provisional), los valores del fixture que la Proposal deja abiertos, las
entradas resueltas por el resolver existente y el golden de los nudos analíticos derivado a mano. Lo consume la tarea
F1-T2-CANTILEVER-SOURCE (decisiones I-65 §13). No es una validación de ingeniería (Q-O3): es la referencia de la equivalencia de
transferencia.

## 1. Diseño del fixture (CD-I65-F1-13)

Lo fijado por el Freeze: una cara `PositiveY`; columna `AISC-W-W12X26`; base `AISC-W-W12X26`; brazos de cuerpo único
`AISC-HSS-RECT-HSS4X4X_250`; tres niveles; pendiente por defecto `CantileverDefaults.ArmSlopeRisePer12` = 7/16.

Lo que el Freeze deja abierto y fija el Coordinator (valores habituales del producto; ninguno cambia una regla IDL):

| Campo | Valor | Motivo |
|---|---|---|
| `StationCount` | 2 | mínimo del producto (`CANT_LINE_NEEDS_TWO_STATIONS`); se exporta la estación 0 |
| `ColumnCentreSpacing` | 48 in | valor por defecto |
| `FirstLevelPunchIndex` | 0 | primer nivel en la primera perforación regular |
| `RequestedClearHeight` | 24 in | claro habitual; sin valor por defecto en el producto |
| `Base.Length` | 48 in | sin valor por defecto en el producto |
| `Body.CutLength` | 36 in | sin valor por defecto en el producto |
| Placas, perforaciones, arriostramiento, separador | valores por defecto del dominio | no se modelan (§4) |

JSON persistido del diseño (forma de `RackProjectStore`; `Line.Id` fijo, distinto del `RackId` del sobre):
[`ref-cant-01.design.json`](../automation/evidence/I-65-f1/ref-cant-01.design.json). Sus bytes en el fixture de pruebas pueden diferir por
fin de línea; ninguna prueba depende de un hash literal de esos bytes.

## 2. Entradas resueltas (autoridad: resolver existente)

Resolución con `CantileverLineResolver.Resolve` y las políticas de producción `CantileverCataloguePolicies` sobre el catálogo versionado,
en el sistema local de la estación 0 (datum `CantileverColumnBaseDatum`, antes del desplazamiento de línea; `OriginX` = 0). Corrida:
[`ref-cant-01-resolver-run.txt`](../automation/evidence/I-65-f1/ref-cant-01-resolver-run.txt) (sonda de solo lectura
[`ref-cant-01-resolver-probe.cs.txt`](../automation/evidence/I-65-f1/ref-cant-01-resolver-probe.cs.txt), fuera del producto). Pulgadas.

| Pieza | Origen del marco | Eje longitudinal (`AxisZ`) | `AxisY` (peralte) | Longitud | Giro / espejo |
|---|---|---|---|---|---|
| Columna `CANT-STN-COL` | (0, −6.1, 0.25) | (0, 0, 1) | (0, 1, 0) | 90.5 | 0 / no |
| Base `CANT-STN-PY-BAS` (no se modela) | (0, 0.25, 6.1) | (0, 1, 0) | (0, 0, 1) | 48 | 0 / no |
| Brazo nivel 1 `CANT-STN-L1-PY-BODY-1` | (0, 0.25, 20.498672113564037) | (0, 0.9993360567820188, 0.03643412707017777) | (0, −0.03643412707017777, 0.9993360567820188) | 36 | 0 / no |
| Brazo nivel 2 `CANT-STN-L2-PY-BODY-1` | (0, 0.25, 48.49867211356404) | ídem | ídem | 36 | 0 / no |
| Brazo nivel 3 `CANT-STN-L3-PY-BODY-1` | (0, 0.25, 76.49867211356404) | ídem | ídem | 36 | 0 / no |

`ColumnStartZ` = 0.25 (placa inferior de 0.25); fin físico de la columna z = 90.75. θ = atan(0.4375/12) = 0.036442192609432884 rad;
tan θ = 7/192.

## 3. Golden derivado a mano (IDL-01..08)

Sistema neutral = sistema local de la estación (D-04: X a lo largo de la línea, Y a lo largo de la base y de los brazos `PositiveY`, Z
vertical). Fórmulas: eje de columna y = −6.1, x = 0 (IDL-01); `COL.FOOT` en z = `ColumnStartZ` (IDL-02); `COL.TOP` en el fin físico
(IDL-03); `ARM.k.PositiveY.J` = intersección del eje del brazo prolongado con el eje de la columna, z = z_o − (0.25 − (−6.1))·tan θ =
z_o − 6.35·tan θ (IDL-04); `ARM.k.PositiveY.TIP` = origen + 36·eje (IDL-05); sin offsets: el brazo analítico va de J a TIP (IDL-06).
Tolerancia de comparación: 1e-9 in en coordenadas y 1e-12 en componentes de vectores unitarios.

| Nudo | x | y | z |
|---|---|---|---|
| `COL.FOOT` | 0 | −6.1 | 0.25 |
| `ARM.1.PositiveY.J` | 0 | −6.1 | 20.26716169689737 |
| `ARM.2.PositiveY.J` | 0 | −6.1 | 48.267161696897375 |
| `ARM.3.PositiveY.J` | 0 | −6.1 | 76.26716169689738 |
| `COL.TOP` | 0 | −6.1 | 90.75 |
| `ARM.1.PositiveY.TIP` | 0 | 36.22609804415268 | 21.810300688090436 |
| `ARM.2.PositiveY.TIP` | 0 | 36.22609804415268 | 49.81030068809044 |
| `ARM.3.PositiveY.TIP` | 0 | 36.22609804415268 | 77.81030068809044 |

| Miembro | Nudo I → nudo J | Sección | Rol | Pieza de origen | Vector de peralte |
|---|---|---|---|---|---|
| `COL.1` | `COL.FOOT` → `ARM.1.PositiveY.J` | `AISC-W-W12X26` | Column | `CANT-S0-STN-COL` | (0, 1, 0) |
| `COL.2` | `ARM.1.PositiveY.J` → `ARM.2.PositiveY.J` | `AISC-W-W12X26` | Column | `CANT-S0-STN-COL` | (0, 1, 0) |
| `COL.3` | `ARM.2.PositiveY.J` → `ARM.3.PositiveY.J` | `AISC-W-W12X26` | Column | `CANT-S0-STN-COL` | (0, 1, 0) |
| `COL.4` | `ARM.3.PositiveY.J` → `COL.TOP` | `AISC-W-W12X26` | Column | `CANT-S0-STN-COL` | (0, 1, 0) |
| `ARM.1.PositiveY` | `ARM.1.PositiveY.J` → `ARM.1.PositiveY.TIP` | `AISC-HSS-RECT-HSS4X4X_250` | Arm | `CANT-S0-STN-L1-PY-BODY-1` | (0, −0.03643412707017777, 0.9993360567820188) |
| `ARM.2.PositiveY` | `ARM.2.PositiveY.J` → `ARM.2.PositiveY.TIP` | `AISC-HSS-RECT-HSS4X4X_250` | Arm | `CANT-S0-STN-L2-PY-BODY-1` | ídem |
| `ARM.3.PositiveY` | `ARM.3.PositiveY.J` → `ARM.3.PositiveY.TIP` | `AISC-HSS-RECT-HSS4X4X_250` | Arm | `CANT-S0-STN-L3-PY-BODY-1` | ídem |

Distancia rígida de IDL-06 (de J al inicio físico del brazo, a lo largo del eje): 6.35 / cos θ = 6.354218840504721 in; longitud del
brazo analítico 42.35421884050472 in. Columna partida en los nudos J (§15 paso 4). Ningún nudo J fuera de `COL.FOOT`–`COL.TOP` y ninguna
separación menor que 0.05 in (IDL-08).

**Apoyos y liberaciones (D-10):** `COL.FOOT` con las seis banderas restringidas (supuesto `SUP-01`); cada uno de los otros siete nudos con
Tx, Ry y Rz restringidos y Ty, Tz y Rx libres (supuesto `SUP-03`); ninguna liberación (supuesto `CON-01`). Supuestos del modelo,
exactamente: `SUP-01`, `SUP-02`, `SUP-03`, `CON-01`, `IDL-NO-OFFSETS`, `ASM-PROP-01`, `LOD-01`, con enunciados neutrales que no afirman
validación estructural.

## 4. Lectura de IDL-10 (CD-I65-F1-14)

Arriostramientos, barras redondas, separadores, placas y cartelas **no se modelan como elementos** en V1: es la redacción de la Proposal V1
(IDL-06, «… separadores, placas y cartelas como elementos») que las versiones siguientes abreviaron, y la que exigen SUP-02 (base no
modelada), SUP-03 («sin pandeo fuera del plano ni arriostramientos»), IDL-06 (el brazo analítico incluye media columna y la placa) y la
propia fila F1 de §6 (un `StructuralModel` **válido** de `REF-CANT-01`, cuya línea tiene siempre placas, separadores y arriostramientos).
Su presencia en la línea no hace `Unsupported` la estación. Sí son `Unsupported` las características de la estación que IDL-10 enumera:
doble cara, brazos `NegativeY`, brazos de canales pareados, inclinación mayor que 10°, estación bloqueada y más de 24 cargas por miembro.

## 5. Archivo de cargas del fixture (`REF1`, solo para pruebas)

`rackcad-structural-loads/v1` con tres cargas (D-10b; CD-I65-F1-05), una por brazo:

| id | destino | fracción | magnitud | posición desde J | magnitud en lbf |
|---|---|---|---|---|---|
| `P1` | nivel 1, `PositiveY` | 0.5 | 1000 `lbf` | 6.354218840504721 + 0.5·36 = 24.35421884050472 | 1000 |
| `P2` | nivel 2, `PositiveY` | 0.25 | 2 `kip` | 15.354218840504721 | 2000 |
| `P3` | nivel 3, `PositiveY` | 0.75 | 500 `kgf` | 33.35421884050472 | `ForceUnits` (500 kgf ≈ 1102.311310924388) |

Dirección global −Z en todas. No es el caso de la OV-I65-02, cuyo archivo se elige en F5.
