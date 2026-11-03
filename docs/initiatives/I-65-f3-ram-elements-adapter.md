# I-65 — F3: adaptador de RAM Elements (concreción del Coordinator)

Documento de la unidad (Coordinator, 2026-10-11). Concreta, **dentro del Freeze** (Proposal V3.1: D-01, D-03, D-04, D-06, D-07, D-08,
D-09, D-10, D-12, D-13; §3 INV-04, 05, 07, 08, 10, 11, 14; §6 fila F3) y de ADR-0050, lo que F3 implementa por orden del Owner
([I-65-owner-authorization-f3.original.txt](../automation/decisions/I-65-owner-authorization-f3.original.txt)). Las decisiones que lo
adoptan están en [decisiones](../automation/decisions/I-65.md) §17. Nada aquí cambia una cláusula congelada.

## 1. Alcance de F3 y exclusiones

F3 produce, a partir de un `StructuralModel` `Valid` (F1), el **paquete de construcción para RAM Elements**: traducción determinista al
sistema y la convención de RAM, tabla de mapeo de proveedor verificable, carga útil, plantilla Python fija, manifiesto de procedencia y
escritura atómica en una carpeta nueva. Fuera de F3 (F4 o posterior): el comando del Plugin y la ruta de la lista de permiso de G1, la lectura
única del sobre (A65-F1-08), el script de volcado, el lector de volcados, el comparador, la distribución de la tabla en el paquete del
producto, toda corrida en RAM y toda OV. Nunca: `analyze()`, lectura de resultados, guardado o sobrescritura de modelos de RAM.

**Exclusiones de V1** (se listan en el manifiesto): no se modelan como elementos la base, las placas, las cartelas, los separadores, los
arriostramientos ni las barras redondas (SUP-02, IDL-06, CD-I65-F1-14); sin peso propio, combinaciones, cargas nodales, distribuidas ni
locales, resortes, offsets ni liberaciones (D-02, D-09).

## 2. Tabla de mapeo de proveedor (D-06, D-07; A65-F1-01, A65-F1-02)

Una **única** tabla verificable: `assets/structural-analysis/ram-elements/section-material-map.json`, esquema
`rackcad-ram-elements-section-map/v1`. Cada fila:

| Campo | Contenido |
|---|---|
| `sectionId` | `StructuralSectionId` del catálogo neutral (único en la tabla) |
| `ramSection` | `{ "name", "table", "country" }`: nombre exacto en la base de RAM, tabla y país de esa base |
| `materialId` | `StructuralMaterialId` neutral asignado explícitamente |
| `ramMaterial` | nombre exacto del material en la base de RAM |
| `expectedMaterial` | oráculo de la OV (A65-V3-02): `elasticModulus` y `unitWeight` como `{ "value", "unit" }`, `poissonRatio` o `shearModulus` (al menos uno), y `source` (cita de ingeniería, nunca copiada de la base de RAM) |
| `status` | `proposed` o `verified` |
| `verifiedBy`, `verifiedOn` | verificador y fecha `yyyy-MM-dd` |

- Unidades de `expectedMaterial`, tabla cerrada y declarada (no se convierten en F3; las compara el comparador de F4): módulos en `psi` o
  `ksi`; peso unitario en `pcf` o `lbf/in3`. Cualquier otra → fila `Invalid`.
- Validación estricta: propiedades desconocidas o duplicadas, tipos erróneos, `sectionId` repetido o vacío, nombres vacíos o con caracteres
  no imprimibles → tabla `Invalid` (sin proyección). Una fila `verified` exige **todos** los campos (tabla y país de RAM, `expectedMaterial`
  completo con `source`, verificador y fecha); si falta uno, la tabla es `Invalid`. Una fila `proposed` puede tener vacíos y **no asigna
  nada**: el modelo que la necesite es `Unsupported`.
- **Proyección neutral:** la tabla implementa `IStructuralMaterialProfile`; sus filas neutrales llevan como texto canónico la **fila completa
  del proveedor** con prefijos de longitud (A65-F1-01); el texto canónico neutral por defecto también pasa a prefijos de longitud
  (A65-F1-02). Ninguna otra implementación de producto del perfil asigna materiales.
- **Contenido inicial** (F3): las dos filas de `REF-CANT-01` como `proposed`, con los nombres que RAM aceptó en la sonda P1 (S1): `AISC-W-W12X26`
  → `W 12X26` / `ASTM-A992-GR50` / `A992 Gr50`, y `AISC-HSS-RECT-HSS4X4X_250` → `HSS_SQR 4X4X1_4` / `ASTM-A500-GRC` /
  `A500 GrC rectangular`; tabla, país y `expectedMaterial` vacíos. El Coordinator **no** rellena valores de ingeniería: los completa y
  verifica un ingeniero antes de la OV (D-07). Con la tabla tal como se entrega, toda exportación es `Unsupported` (fallo cerrado).

## 3. Unidades y frontera (D-03)

- Par de RAM: `LB_IN`. El modelo neutral ya está en pulgadas y libras-fuerza: **no hay conversión** de longitud ni de fuerza en el adaptador.
- Posición de carga: porcentaje del miembro analítico, `100 · d / L` (adimensional).
- Ángulos: grados, por `atan2` (§5). Sin literales de conversión en `Providers/` (G4 cubre `StructuralAnalysis/**`, `Providers/` incluido).

## 4. Coordenadas y apoyos (D-04; INV-08; CD-I65-F1-19)

- Posiciones: `X_RAM = X`, `Y_RAM = Z`, `Z_RAM = −Y`.
- Banderas de apoyo: orden neutral fijo `[Tx, Ty, Tz, Rx, Ry, Rz]` (CD-I65-F1-19). La transformación de D-04 es una permutación (las banderas
  no tienen signo):

| RAM (`Restraints`) | `dx` | `dy` | `dz` | `rx` | `ry` | `rz` |
|---|---|---|---|---|---|---|
| bandera neutral | `Tx` | `Tz` | `Ty` | `Rx` | `Rz` | `Ry` |

  Pie de columna (SUP-01): las seis → `Restraints(dx, dy, dz, rx, ry, rz)`. Demás nudos (SUP-03, `[T, F, F, F, T, T]`) → `dx`, `ry`, `rz`.
  Un mapeo índice a índice (`Ry → ry`) falla INV-08.
- Liberaciones: locales, sin transformación; en V1 todas falsas (D-02), la plantilla no las escribe y las comprueba por lectura de vuelta.
- Prioridad de fusión de nudos de la columna (CD-I65-F1-19 b): **documentada, sin cambio** (solo actúa dentro de 0.001 in); el adaptador
  exporta los nudos que resultan, sin volver a fusionar.

## 5. Ejes locales y giro (D-04; INV-07)

Regla medida en P1 (§4 de `p1-results.md`): con `l1` = unitario de I a J en RAM e `Ŷ = (0, 1, 0)`:

- miembro vertical (componente horizontal de `l1` ≤ 1e-12): `l3₀ = +Ẑ`, `l2₀ = l3₀ × l1`;
- miembro no vertical con inclinación respecto de la horizontal ≤ 10°: `l3₀ = unit(l1 × Ŷ)`, `l2₀ = l3₀ × l1`;
- cualquier otra orientación → `Unsupported` (el umbral de RAM no está medido; A65-V3-06).

Giro emitido: `θ = atan2(d · l3₀, d · l2₀)` en grados, con `d` el vector de peralte neutral llevado a RAM `(dx, dz, −dy)`; tras girar `θ`
alrededor de `l1` (dextrógiro), `l2 = d`. El oráculo de la prueba son las 12 direcciones × 3 giros de P1 (`p1-report.json`, paso A1).

## 6. Cargas (D-02, D-09)

Un caso `REF1` con categoría de texto `"OTHER"` (nunca el miembro del enum), sin peso propio. Cada carga: `add_point_load` en el miembro, dirección
`GLOBAL_Y`, valor `−magnitud` (lbf), posición en porcentaje (`is_percentage=True`). Máximo 24 por miembro (ya fallo cerrado en F1).

## 7. Paquete (D-08, D-09, D-12)

Carpeta nueva con exactamente: `manifest.json`, `rackcad-ram-build.py` y la copia byte a byte del archivo de cargas; tras la ejecución en
RAM, el script añade `ram-readback.json`. Sin marcas de tiempo; UTF-8 sin BOM; LF. Mismo modelo, mismas entradas y misma carpeta de destino
→ bytes idénticos.

- **Carga útil:** JSON solo ASCII (`rackcad-ram-build-payload/v1`) con la versión del adaptador, el par `LB_IN`, la ruta absoluta de la
  carpeta del paquete, la huella de entrada (`AnalysisInputFingerprint`), el caso, los nudos (id neutral, coordenadas de RAM, banderas), los
  miembros (id neutral, nudos, sección y material de RAM, giro, ejes locales esperados), las cargas (miembro, dirección, valor, porcentaje),
  las tolerancias de D-09 regla 9 y la lista de capacidades (versión `0.1.0` y funciones del puente). Se incrusta en el script como literal de
  cadena Python que `json.loads` decodifica (sin base64).
- **Plantilla:** texto fijo del adaptador, con las reglas 1-10 de D-09. Llamadas admitidas: las ejecutadas en P1 (`model()`, `units.set`,
  `nodes.add` con `restraints` y `description`, `members.add` con `section`, `material` **siempre explícito**, `rotation` y `description`,
  `cardinal_point`, lectura de offsets, factores de rigidez, comportamiento axial, resortes y liberaciones, `get_local_axes`,
  `get_coordinates`, `restraints`, `load_condition.add_load_case` con categoría de texto, `add_point_load` con porcentaje,
  `point_load_count`, `get_point_load`, `__version__` y la lista de funciones del núcleo) y **`Restraints(...)` con banderas parciales**
  (clase del paquete instalado; P1 solo ejecutó `FIXED` y `PINNED`), cubierta por la puerta de capacidad y la lectura de vuelta exacta. La
  lista de funciones del núcleo que la puerta exige está contenida en las 207 medidas en P1.
- **Manifiesto** (`rackcad-ram-build-manifest/v1`): adaptador y versión; destino (versión del producto **declarada, no verificada**,
  `ramelements` `0.1.0`, `LB_IN`); `SourceRef` y `SourceFingerprint`; `AnalysisInputFingerprint`; archivo de cargas (nombre y SHA-256); filas
  de mapeo usadas completas y su hash; supuestos con su limitación declarada (D-10) y la advertencia **«hipótesis solo para la equivalencia
  de transferencia; no constituyen validación estructural»** (Q-O3, ADR-0050 §5); exclusiones de V1 (§1); archivos del paquete con SHA-256
  (salvo el propio manifiesto).
- **Escritura atómica** (neutral, `StructuralAnalysis/`): destino inexistente con padre existente; escritura en carpeta temporal hermana y un
  único renombrado; destino existente → rechazo sin tocar nada (nunca sobrescribir); cualquier fallo → sin carpeta de destino y sin restos
  temporales. El adaptador **no escribe** archivos: devuelve el contenido (D-01).

## 8. Golden de RAM de `REF-CANT-01` (derivado a mano)

Del modelo de [I-65-f1-ref-cant-01.md](I-65-f1-ref-cant-01.md) §3 con §4-§6 de este documento. Tolerancias: 1e-9 in, 1e-12 en vectores,
1e-9 en grados y en porcentajes.

| Nudo | X_RAM | Y_RAM | Z_RAM | `Restraints` |
|---|---|---|---|---|
| `COL.FOOT` | 0 | 0.25 | 6.1 | `dx dy dz rx ry rz` |
| `ARM.1.PositiveY.J` | 0 | 20.26716169689737 | 6.1 | `dx ry rz` |
| `ARM.2.PositiveY.J` | 0 | 48.267161696897375 | 6.1 | `dx ry rz` |
| `ARM.3.PositiveY.J` | 0 | 76.26716169689738 | 6.1 | `dx ry rz` |
| `COL.TOP` | 0 | 90.75 | 6.1 | `dx ry rz` |
| `ARM.1.PositiveY.TIP` | 0 | 21.810300688090436 | −36.22609804415268 | `dx ry rz` |
| `ARM.2.PositiveY.TIP` | 0 | 49.81030068809044 | −36.22609804415268 | `dx ry rz` |
| `ARM.3.PositiveY.TIP` | 0 | 77.81030068809044 | −36.22609804415268 | `dx ry rz` |

| Miembros | Regla | `l1` | giro θ | `l2` esperado | `l3` esperado |
|---|---|---|---|---|---|
| `COL.1`..`COL.4` | vertical | (0, 1, 0) | −90° | (0, 0, −1) | (−1, 0, 0) |
| `ARM.k.PositiveY` | no vertical | (0, 0.036434127070177715, −0.9993360567820188) | 0° | (0, 0.9993360567820188, 0.036434127070177715) | (1, 0, 0) |

| Carga | Miembro | Dirección | Valor (lbf) | Posición (%) |
|---|---|---|---|---|
| `P1` | `ARM.1.PositiveY` | `GLOBAL_Y` | −1000 | 57.50128206169155 |
| `P2` | `ARM.2.PositiveY` | `GLOBAL_Y` | −2000 | 36.25192309253731 |
| `P3` | `ARM.3.PositiveY` | `GLOBAL_Y` | −`ForceUnits.ToPoundsForce(500, kgf)` | 78.75064103084577 |
