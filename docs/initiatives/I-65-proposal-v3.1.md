# I-65 — Proposal V3.1 (Structural Analysis Interop Foundation + RAM Elements Adapter)

```text
Estado: PROPUESTA CONGELABLE — pendiente de la revisión de confirmación del Architect
Frozen: no
Coordinator: AGREED (sujeto a la confirmación del Architect)        Architect: cierre por diff de A65-V3-01/02 pendiente
IMPLEMENTATION AUTHORIZATION = NO (aun con AGREED: F1 exige una autorización posterior del Owner y la aceptación de ADR-0050)
Sustituye a: Proposal V3 (ronda 4 = CHANGES REQUIRED: A65-V3-01 y A65-V3-02 REQUIRED, 03..06 OPTIONAL; A65-V1-01 CLOSED)
Historia: V1 → V2 → V2.1 → V3 (valores medidos por P1 v3.2 y decisiones del Owner) → V3.1 (correcciones de la ronda 4)
```

Fuentes: [mandato](../automation/decisions/I-65-owner-mandate.original.txt), [Discovery F0](I-65-discovery.md) (con su adenda §13),
[contrato](I-65-interop-analisis-estructural.md), borrador [ADR-0050](../adr/0050-interoperabilidad-analisis-estructural.md). Los
nombres de tipos son hipótesis; se congelan responsabilidades, contratos observables y semántica de fallo. Las marcas **[P1]** señalan
valores que la sonda P1 debía medir; en esta V3 están sustituidos por valores `MEASURED` de la corrida real (P1 v3.2,
[resultados](../automation/evidence/I-65-f0/p1-results.md)) y marcados **[P1✓]**.

## 0. Precondiciones del Freeze (A65-V1-01)

| ID | Precondición | Estado |
|---|---|---|
| FP-01 | Sonda P1 ejecutada en RAM Elements 26.00.01.015 con autorización del Owner y su informe versionado | **satisfecha**: P1 v3.2 (S9 retirado por condición del Owner; revisado por el Architect), [p1-results.md](../automation/evidence/I-65-f0/p1-results.md) |
| FP-02 | D-03, D-04 (mapeo y regla de giro), D-08 (modelo vacío), tolerancias de lectura de vuelta (D-09) e INV-07 con valores `MEASURED` | **satisfecha en esta V3** (revisión de confirmación pendiente) |
| FP-03 | Decisiones del Owner Q-O1, Q-O2, Q-O3, Q-O5 y Q-O7 registradas | **satisfecha** ([decisiones](../automation/decisions/I-65.md) §5): Q-O1 y Q-O7 **provisionales** (condición en §10), Q-O2 sin regla por familia, Q-O3 hipótesis solo para equivalencia, Q-O5 F6 fuera |
| FP-04 | Borrador de ADR-0050 revisado junto con la versión congelable | este paquete; aceptación del Owner (Q-O6) **antes de F1** |
| FP-05 | Ninguna falla de clase STOP en longitud, fuerza o posición de carga | **satisfecha**: escala verificada (Timoshenko Δ ≤ 7·10⁻⁵), momento en lb·in, posiciones en % exactas |

Si el Owner rechaza Q-O4, el resultado es `BLOCKED — OWNER DECISION`; no hay Freeze condicional. **Regla STOP:** si P1 muestra que la
longitud, la fuerza o la posición de carga puntual no se comportan como el adaptador requiere, la iniciativa se detiene y vuelve al
Coordinator y al Owner; ninguna de esas magnitudes se «excluye» (V1 lo permitía; retirado).

## 1. Principio rector

```text
RackCad / DWG                  = autoridad de producto y diseño (sin cambios de persistencia)
Structural Analysis Model      = representación analítica NEUTRAL, DERIVADA, no persistida y regenerable
RAM Elements                   = consumidor externo de análisis (nunca autoridad de RackCad)
```

Flujo V1: `DWG → diseño authored → CantileverLineResolver (existente) → fuente estructural Cantilever (nueva) → StructuralModel
(nuevo) → adaptador RAM (nuevo, carpeta de proveedor) → paquete en carpeta nueva → script ejecutado por el ingeniero en la Python
Console de RAM Elements → lectura de vuelta → comparador`.

## 2. Decisiones

### D-01 — Capas y frontera de proveedor (A65-V1-09; AQ-01)

- Neutral (modelo, unidades de análisis, diagnósticos, huellas, comparador): `RackCad.Application`, carpeta raíz nueva (hipótesis
  `StructuralAnalysis/`). Domain no cambia.
- Fuente Cantilever: `Application/Systems/Cantilever/`, solo lectura de `CantileverLineResolver` / `CantileverStationAssembly`.
- Adaptador RAM: `Application/StructuralAnalysis/Providers/RamElements/`, solo BCL. **Toda** capacidad con RAM en su contrato vive ahí,
  incluida la creación del paquete y la lectura de los volcados de RAM. Devuelve contenido; escribir es del llamador.
- Guardas de fuente congeladas:
  - **G1 (proveedor):** alcance `src/RackCad.Domain/**`, `src/RackCad.Application/**`, `src/RackCad.UI/**` y `src/RackCad.Plugin/**`
    salvo `src/RackCad.Application/StructuralAnalysis/Providers/RamElements/**`; lista de permiso de ruta exacta: el único archivo de
    comando del Plugin de D-13. Fuera de alcance: `tests/**`, `tools/**`. Tokens prohibidos (sensibles a mayúsculas): `RamElements`,
    `ramelements`, `RAM Elements`, y la expresión `\bRam[A-Z]`.
  - **G2 (neutralidad):** `StructuralAnalysis/**` fuera de `Providers/**` no referencia `Systems.*` ni `Providers.*`; `Systems/*` no
    referencia `Providers.*`.
  - **G3:** Domain no referencia `StructuralAnalysis`.
  - **G4 (unidades):** §D-03.
- Ensamblado propio del adaptador: no en V1 (solo BCL); se reabre si un proveedor trae una dependencia binaria.

### D-02 — StructuralModel V1 (recortado al uso real; A65-V1-17)

| Parte | Contenido V1 | Identidad |
|---|---|---|
| Metadata | `SourceRef` (RackId = `RackEmbedDocument.Id` del **sobre**, Kind, índice de estación, `SourceFingerprint`), versión del generador, hashes del archivo de cargas, de las filas de mapeo verificadas (sección y material) y del conjunto de supuestos | neutral |
| Nodos | posición (Length×3) | id neutral determinista derivado de la regla que lo creó (D-05) |
| Miembros | nudo I, nudo J, `SectionRef`, `MaterialRef`, vector del eje de peralte en global, liberaciones por extremo, rol (Column/Arm), pieza de origen | id neutral determinista |
| Secciones | `StructuralSectionId` + A, Ix, Iy, J del catálogo (**informativas**: ver D-06) | `StructuralSectionId` |
| Materiales | `StructuralMaterialId` (identidad; ver D-07) | neutral |
| Apoyos | 6 banderas por nudo (globales neutrales) + id de supuesto | — |
| Liberaciones | 10 banderas por miembro (locales) + id de supuesto; V1 = todas falsas | — |
| Casos de carga | un caso `REF1`, categoría neutral «otra», sin peso propio | neutral |
| Cargas | **solo** cargas puntuales en miembro, dirección global −Z neutral, magnitud (Force), posición como distancia absoluta desde el nudo I del miembro analítico | — |
| Supuestos | lista `AnalysisAssumption(id, enunciado)` | id |
| Diagnósticos | D-08 | — |

Fuera de V1 (pedir cualquiera → `Unsupported`): momentos nodales, cargas en direcciones locales, cargas distribuidas, Cw y propiedades de
alabeo, masa y densidad, peso propio, combinaciones, resortes, offsets rígidos. **No** se crea un punto de extensión de traducción de
cargas: la única entrada es el archivo explícito de D-10b.

### D-03 — Unidades: almacenamiento, propietarios y frontera (A65-V1-05/06/07; AQ-02)

- **Almacenamiento canónico de V1 = sistema consuetudinario de EE. UU. coherente: longitud en pulgadas (ADR-0005 D1, sin unidad
  interna nueva para geometría), fuerza en libras-fuerza**; derivadas: lbf·in, psi, in², in⁴. Las cantidades son tipos por dimensión sin
  constructor público desde `double` sin unidad.
- **Tabla de propietarios (una autoridad por dimensión):**

| Dimensión | Propietario | Regla |
|---|---|---|
| Longitud in↔mm / in↔m | `LengthUnits.MillimetersPerInch` (ADR-0040 D3) | derivación por multiplicación; sin literal nuevo |
| Área, inercia (catálogo) | catálogo neutral (filas `US_CUSTOMARY`: ya en in², in⁴; sin conversión) | una fila con otro sistema nativo → conversión solo por `StructuralSectionUnits` (ADR-0021 §10); si la API no la ofrece → `Unsupported` |
| Masa de libra (0.45359237 kg) | **propietario único nuevo** en `Application/Units/` (hipótesis `MassUnits.KilogramsPerPound`); `StructuralSectionUnits` lo consume con resultado **idéntico bit a bit** (Extends ADR-0021, API sin cambio, prueba de igualdad de bits) | — |
| Gravedad estándar gₙ = 9.80665 m/s² | propietario único nuevo (hipótesis `ForceUnits.StandardGravity`) | — |
| lbf, kgf, kip, kN | derivados: lbf = lb·gₙ; kgf = 1 kg·gₙ; kip = 1000 lbf; kN = 1000 N | sin literales adicionales |

- **G4:** en `src/RackCad.Application/StructuralAnalysis/**` ningún literal de la lista cerrada `25.4`, `645.16`, `0.45359237`,
  `9.80665`, `4.4482216152605`, `0.3048`, ni nombres de rol de conversión de longitud; complementa, no sustituye, a
  `LengthUnitsAuthorityGuardTests` (que sigue vigente en todo el repositorio). Los propietarios nuevos (`Application/Units/`) quedan
  fuera de G4 y bajo su propia prueba de igualdad.
- **Frontera de entrada:** el archivo de cargas (D-10b) declara su unidad por carga; la conversión a lbf ocurre en un solo punto.
  Unidad ausente o desconocida → `Invalid`.
- **Frontera de salida [P1✓]:** el adaptador emite en el par **`LB_IN`**, sin conversión de longitud ni de fuerza. Medido: en `LB_IN`
  coordenadas, magnitudes, posiciones y giros no redondos se leen **idénticos**; la escala de longitud, fuerza y momento es correcta
  (flechas de voladizo frente a Timoshenko con Δ ≤ 7·10⁻⁵; reacciones `P·L` en lb·in). Las lecturas en pares métricos de RAM llevan error
  propio de RAM (−3.2·10⁻⁸ en longitud, −5.22·10⁻⁶ en fuerza; g implícita 9.8066): ni el script ni el volcado leen en un par métrico.
- **Cambio de par por la API [P1✓]:** conserva los valores físicos (U1, P2). Que no cambie las unidades de la interfaz es `INFERENCE`
  (captura y `Config.ini`), no medición.
- **Exactitud (A65-V1-06):** igualdad exacta solo donde la aritmética es exacta (una multiplicación por un factor propietario en el camino
  a RAM; escalas ×25.4, ×1000 y lbf↔N comprobadas con el mismo operador); idas y vueltas con tolerancia relativa congelada ≤ 1e-12.
- Pruebas que detectan: in↔mm, lbf↔N y kgf↔N, kip↔lbf, unidad ausente, unidad desconocida, y escala en la lectura de vuelta.

### D-04 — Ejes y orientación (A65-V1-03b, A65-V1-11 INV-08, A65-V1-15)

- Neutral: dextrógiro, Z arriba, **origen = datum local de la estación** (`CantileverColumnBaseDatum`; antes del `Offset` de la línea):
  X a lo largo de la línea, Y a lo largo de la base y de los brazos de la cara positiva, Z vertical, z = 0 en el piso. V1 admite **solo
  estaciones de una cara `PositiveY`** (A65-V2-02).
- RAM **[P1✓]**: **Y arriba** (peso propio `(0, −1, 0)`). Mapeo congelado `X_RAM = X`, `Y_RAM = Z`, `Z_RAM = −Y` (rotación propia); el
  plano de la estación (Y-Z neutral) es el plano Y-Z de RAM y los brazos `PositiveY` van en `(0, +subida, −avance)`, dirección medida. La
  misma transformación aplica a apoyos globales: `Tx→Tx`, `Ty→−Tz`, `Tz→Ty`, `Rx→Rx`, `Ry→−Rz`, `Rz→Ry` (las banderas no tienen signo).
  Las liberaciones son locales y no se transforman.
- **Regla de ejes locales de RAM [P1✓]** (12 direcciones × 3 giros, error ≤ 3.3·10⁻¹⁵): miembro no vertical `l3 = unit(l1 × Ŷ)`,
  `l2 = l3 × l1`; vertical (`l1 = ±Ŷ`) `l3 = +Ẑ`, `l2 = l3 × l1`; `rotation = θ` gira `l2` y `l3` alrededor de `l1` en sentido dextrógiro.
  El peralte va a lo largo de `l2` y la flexión fuerte es `M33`.
- **Giro emitido:** `θ = atan2(d · l3₀, d · l2₀)` en grados, con `d` = vector de peralte neutral mapeado a RAM y `(l2₀, l3₀)` de la regla.
- Orientación: el vector del eje de peralte se deriva del marco de `PrismaticSectionInstance` **incluidos `RotationRadians` y
  `Mirrored`**; el adaptador calcula el giro con la regla medida y el script verifica con `get_local_axes()` (D-09 regla 9).
- P1 midió los miembros reales de la rebanada de una cara: verticales, horizontales a lo largo de ±Z de RAM, inclinados en el plano Y-Z
  de RAM (sentido de los brazos `PositiveY`), invertidos, con giro 0°, 90° y 12.3456°.

### D-05 — Idealización analítica (A65-V1-04, A65-V1-16)

| ID | Regla |
|---|---|
| IDL-01 | Eje analítico = eje centroidal del miembro (origen del marco de `PrismaticSectionInstance` + eje longitudinal), nunca la geometría de bloque |
| IDL-02 | **Nudo pie de columna** `COL.FOOT`: eje de la columna en su inicio físico, z = `ColumnStartZ` (cara superior de la placa inferior) |
| IDL-03 | **Nudo cabeza de columna** `COL.TOP`: eje de la columna en su fin físico |
| IDL-04 | **Nudo brazo–columna** `ARM.<nivel>.<lado>.J`: intersección, en el plano de la estación (Y-Z), del eje del brazo prolongado hacia la columna con el eje de la columna |
| IDL-05 | **Nudo punta de brazo** `ARM.<nivel>.<lado>.TIP`: eje del brazo en el fin físico del miembro brazo (no la placa de tope) |
| IDL-06 | Sin offsets rígidos: el brazo analítico va de `ARM…J` a `ARM…TIP` e incluye media columna y la placa (supuesto `IDL-NO-OFFSETS`, en el manifiesto) |
| IDL-07 | Base: **no se modela** en V1 (SUP-02 opción A) |
| IDL-08 | Tolerancias: fusión de nudos 0.001 in; separación mínima entre nudos de la columna 0.05 in (menor → diagnóstico `Invalid`, no fusión silenciosa); desviación de un eje respecto del plano de la estación 0.01 in (mayor → `Unsupported`); angular 0.01°; un nudo brazo–columna fuera del tramo `COL.FOOT`–`COL.TOP` → `Invalid` |
| IDL-09 | Familias admitidas en V1: **doblemente simétricas** W, HSS rectangular/cuadrada y S. C y L (centrado por caja, ejes principales girados) → `Unsupported` |
| IDL-10 | Fuera de V1 → `Unsupported`: brazos con inclinación mayor que 10° respecto de la horizontal (el umbral en que RAM pasa a la regla de miembro vertical no está medido; los brazos del producto rondan 2°; A65-V3-06), estaciones de doble cara y brazos `NegativeY`, brazos de canales pareados, arriostramientos, barras redondas, separadores, placas y cartelas, estaciones bloqueadas, más de 24 cargas por miembro |

**Estación de referencia `REF-CANT-01`** (propuesta; el Owner puede sustituirla por una real de una cara en Q-O7): una cara
(`PositiveY`), columna `AISC-W-W12X26`, base `AISC-W-W12X26` (requerida por el diseño aunque no se modele), brazos de cuerpo único
`AISC-HSS-RECT-HSS4X4X_250`, tres niveles, pendiente por defecto (`CantileverDefaults.ArmSlopeRisePer12` = 7/16). Su diseño se versiona
como fixture en F1. **Golden de INV-06 (A65-V2-06):** entradas = colocaciones resueltas por el resolver existente (orígenes, ejes y
longitudes de brazos, `ColumnStartZ`, longitud de columna), autoridad probada por separado; los nudos analíticos se **derivan a mano** con
IDL-01..08 y se escriben como literales (p. ej. eje de columna W12X26 en x = 0, y = −6.1 in; nudo brazo–columna del nivel k en
z = z_a,k − (y_a,k + 6.1)·tan θ, tan θ = (7/16)/12).

### D-06 — Secciones (A65-V1-08)

```text
StructuralSectionId (catálogo neutral, AISC v16.0) → StructuralSection → nombre de sección de RAM
```

- Identidad = `StructuralSectionId`; nunca `displayName` ni etiqueta de bloque. Mapeo a RAM explícito y versionado (tabla de datos nueva;
  solo filas revisadas; sin inferencia de cadenas en tiempo de ejecución). Fila ausente → `Unsupported`. La familia HSS-RECT de RackCad
  incluye tubos cuadrados, que RAM guarda en otra tabla (`HSS_SQR 4X4X1_4`); la fila de mapeo lo refleja.
- **Espacio de nombres de RAM [P1✓]:** RAM acepta nombres de cualquier base de país sin configurarla (`IPE 200`); cada fila de mapeo
  registra la tabla y el país de RAM y la confirma un ingeniero; un nombre duplicado entre bases es UNKNOWN y lo detecta la fila de
  propiedades de OV.
- **Fuente de propiedades para el análisis [Q-O2]** — recomendación **(b)**: la base de datos de RAM es la fuente (supuesto
  `ASM-PROP-01`); las propiedades del catálogo viajan como **informativas** en el manifiesto y la OV compara A, Ix, Iy, J de RAM contra
  ellas con **±0.5 %** (fila de la matriz §4); los deltas numéricos se registran en la evidencia de OV y todo delta mayor a ±0.5 % exige
  el reconocimiento expreso del Owner (A65-V2-08). Alternativa (a): RackCad como autoridad con la misma fila convertida en criterio de
  PASS.

### D-07 — Materiales (A65-V1-08)

- **Sin regla por familia (decisión del Owner Q-O2).** El material se asigna en la **misma fila explícita** del mapeo de cada sección:
  `StructuralSectionId → (nombre de sección de RAM, StructuralMaterialId, nombre de material de RAM)`, cada fila **verificada por un
  ingeniero** (quién y cuándo, en la tabla). Fila ausente o sin verificar → `Unsupported`. Nunca se infiere de nombres de producto ni de
  la familia geométrica. Para `REF-CANT-01` las dos filas se proponen al ingeniero (p. ej. `W 12X26`/`A992 Gr50`,
  `HSS_SQR 4X4X1_4`/`A500 GrC rectangular`, ambas aceptadas por RAM en P1) y solo valen tras su verificación.
- **Oráculo de material (A65-V3-02):** cada fila verificada registra los valores **esperados** de E, ν (o G) y peso unitario del material
  de RAM elegido, **con su fuente de ingeniería citada** (p. ej. norma ASTM o Manual AISC), nunca copiados de la base de RAM. La fila de la
  matriz §4 compara los valores de la base de RAM contra esos valores registrados; una diferencia exige el reconocimiento expreso del
  Owner (como los deltas de sección) o el cambio de la fila. Ejemplo conocido: el `A36` de RAM tiene ν = 0.26 frente a 0.3 habitual.
- Con la recomendación (b), los valores de material que usa el análisis los aporta **solo** la base de RAM; RackCad no los transfiere.
  Los valores esperados de cada fila verificada (oráculo de material) no son una segunda autoridad del análisis: solo sirven para la fila
  §4 de la OV.

### D-08 — Errores, versión y escritura atómica (A65-V1-11, A65-V1-12; AQ-10)

- `StructuralExportOutcome = Valid | Unsupported | Invalid | Unavailable`; cualquier diagnóstico bloqueante impide el paquete.
- **Escritura atómica del paquete:** se escribe en una carpeta temporal hermana y se publica con un único renombrado a la carpeta nueva;
  si algo falla, no queda carpeta de destino.
- **Puerta de capacidad del script [P1✓]:** `ramelements.__version__ == "0.1.0"` **y** presencia de las funciones del puente que el
  script usa (subconjunto de las 207 medidas en P1-E1). La versión del producto RAM se registra como **declarada, no verificada**
  (`Model.version` no está expuesto).
- **Modelo vacío [P1✓]:** `len(nodes) == 0`, `len(members) == 0` y como máximo **un** caso de carga sin cargas (File > New crea uno
  por defecto cuyo id depende del idioma de la interfaz: `CM` en español). El script no usa ni borra ese caso.
- Dentro de RAM, un fallo a mitad (nombre rechazado, lectura de vuelta distinta, capacidad ausente) detiene el script con estado
  `FAILED — DISCARD MODEL` en consola y en la lectura de vuelta; el procedimiento exige descartar el modelo sin guardar (**[P1✓]** un
  nombre rechazado lanza `ValueError` y deja un miembro huérfano).

### D-09 — Transporte: paquete de construcción para RAM Elements (A65-V1-03d, A65-V1-11, A65-V1-18)

- API Python oficial, ejecutada por el ingeniero con «Run File…» sobre un modelo nuevo y vacío. No es automatización de interfaz.
- Contenido de la carpeta nueva: `manifest.json`, `rackcad-ram-build.py`, una copia del archivo de cargas usado; tras ejecutarlo,
  `ram-readback.json`. **Ningún archivo del paquete contiene marcas de tiempo**: mismo modelo y mismas entradas → bytes idénticos.
- Reglas congeladas de la plantilla (verificadas por guarda estática sobre la plantilla y pruebas de contrato de la carga útil):
  1. imports solo `ramelements`, `json`, `math`;
  2. la carga útil es un literal de cadena JSON **solo ASCII** decodificado con `json.loads` (sin base64);
  3. la ruta absoluta de la carpeta del paquete va **embebida** por RackCad (**[P1✓]** `__file__` no existe con «Ejecutar archivo…» y el
     directorio actual es la carpeta de instalación de RAM);
  4. sin `save`, `save_as`, `new`, `analyze`, `open` de archivos del usuario ni `os.*`; el único archivo que escribe es
     `ram-readback.json`, creado **en modo exclusivo** dentro de la carpeta embebida;
  5. categorías de caso como texto (`"OTHER"`), nunca miembros del enum (**[P1✓]** el enum guarda `"LoadCategory.DEAD"` sin error);
  6. una sola llamada `add_force` por nudo y caso (V1 no usa cargas nodales) y como máximo 24 cargas puntuales por miembro y caso
     (exceso → `Unsupported` en RackCad antes de generar);
  7. fija y lee de vuelta `cardinal_point = CENTROID`; **lee** offsets, factores de rigidez, comportamiento axial y resortes de cada
     miembro y falla si difieren de los valores por defecto **[P1✓]** (CENTROID, `(0,0,0)`, `(1,1,1,1)`, `NORMAL`, 0); no usa esos setters;
  8. puerta de capacidad y modelo vacío (D-08) antes de la primera escritura;
  9. la autocomprobación por lectura de vuelta, en `LB_IN` (lectura medida idéntica), usa: coordenadas y magnitudes con
     `|Δ| ≤ 1e-9 · max(1, |v|)`, posiciones de carga en % con `|Δ| ≤ 1e-9`, componentes de ejes locales con `|Δ| ≤ 1e-9` (ruido medido
     3.3·10⁻¹⁵); sección, material, liberaciones y apoyos exactos;
  10. **no** llama `analyze()`: tras un análisis, cualquier escritura abre un diálogo modal que bloquea el script hasta una respuesta
      humana (**[P1✓]**); la puerta de modelo vacío garantiza que no hay resultados previos.
- OpenRE, STAAD, SAP2000, DXF, ISM y el servidor MCP del binario: no se usan.

### D-10 — Supuestos de V1 (Q-O3)

| ID | Supuesto | Limitación declarada |
|---|---|---|
| SUP-01 | `COL.FOOT` empotrado (6 GDL) | sin rigidez de anclaje, placa ni losa |
| SUP-02 | Base no modelada (opción A). La opción B (base como miembro) queda para una A-n posterior a OV-I65-01 | la base real apoya en el piso |
| SUP-03 | Análisis plano: todos los nudos con Tx, Ry y Rz neutrales restringidos (mapeados a RAM según D-04) | sin pandeo fuera del plano ni arriostramientos |
| CON-01 | Brazo–columna continuo (sin liberaciones) | la conexión real es semirrígida |
| IDL-NO-OFFSETS | D-05 IDL-06 | sobreestima la flexibilidad del brazo; ninguna conclusión de comportamiento real se extrae de V1 |
| ASM-PROP-01 | Propiedades de sección y material = base de datos de RAM (Q-O2: en estudio para la prueba controlada) | ediciones distintas de AISC y valores de material de RAM distintos de los habituales; comparación en OV contra el catálogo (sección) y contra los valores esperados con fuente de cada fila (material) |
| LOD-01 | Un caso `REF1` con cargas del archivo D-10b; sin peso propio ni combinaciones | no deriva de tarimas, impacto, sismo ni tributarias |

### D-10b — Canal de entrada de cargas (A65-V1-10)

- Archivo JSON versionado (hipótesis de esquema `rackcad-structural-loads/v1`) elegido en la exportación. Cada carga declara: destino
  (brazo por nivel y lado), base de posición (`physicalArmFraction` medida desde el inicio físico del brazo, lado columna), dirección
  (`-Z`), magnitud y **unidad explícita** (`N`, `kN`, `lbf`, `kip`, `kgf`). Unidad ausente/desconocida, destino inexistente, fracción
  fuera de [0, 1] o dirección no admitida → `Invalid`.
- La fuente convierte la posición a distancia absoluta desde `ARM…J` (distancia rígida de IDL-06 + fracción × longitud física) y el
  adaptador la emite como **porcentaje** del miembro analítico (sin unidades).
- El archivo forma parte de la huella de entrada, se copia al paquete y se lista en el manifiesto con su SHA-256.

### D-11 — Equivalencia (A65-V1-11, A65-V1-12, A65-V1-16)

- Referencia: el ingeniero modela a mano en RAM la estación de referencia con las mismas reglas (D-04, D-05, D-10).
- Volcado: script **fijo de solo lectura** (en la carpeta del proveedor) que vuelca el modelo abierto a JSON canónico, con el mismo
  mecanismo de ruta embebida y archivo exclusivo. **Contrato de unidades del volcado (A65-V2-03; A65-V3-01):** registra el par inicial
  (`m.units.current`); **solo si** es distinto de `LB_IN`, fija `LB_IN` por la API y lo restaura en un `try/finally`; si ya es `LB_IN`, no
  llama `units.set`. **[P1✓]** el cambio de par por la API conserva los valores físicos de un modelo **no analizado**; su efecto sobre un
  modelo con resultados no está medido (en P1 el diálogo modal apareció tras `analyze()` en la primera escritura, sin distinguir si la
  disparó `units.set` o `nodes.add`). Por eso el volcado se ejecuta **antes** de analizar la referencia (o sobre una copia sin analizar).
  Si aun así aparece el diálogo «los resultados se perderán», se responde **No** y se detiene (STOP): se vuelca un modelo no analizado. El
  lector del proveedor acepta solo volcados leídos en `LB_IN`; otro par → `Invalid("unit pair")`.
- Casos de carga sin cargas (p. ej. el caso por defecto `CM`) se ignoran en ambos volcados; solo se comparan los casos con cargas.
- El ingeniero aplica las cargas del modelo manual como **cargas puntuales en miembro** (la API no lee cargas nodales; A65-V2-09).
- Comparador puro (Application) + herramienta de consola fuera del producto: empareja nudos por coordenadas y miembros por extremos;
  **falla** si un emparejamiento es ambiguo; **rechaza** volcados con estado `FAILED` y lecturas de vuelta cuya huella de entrada no
  coincide con la del paquete (`STALE`).

### D-12 — Provenance y resultados obsoletos (A65-V1-14)

- `RackId` = `RackEmbedDocument.Id` del sobre (ADR-0009), no el `Line.Id` interior de Cantilever.
- `SourceFingerprint` = SHA-256 de los bytes del JSON `Design` tal como está persistido en el sobre (conservador: cualquier cambio de
  bytes → STALE; un STALE espurio es seguro), + RackId + Kind + índice de estación.
- `AnalysisInputFingerprint` = SHA-256 del modelo neutral canónico + hashes del archivo de cargas, de las filas de mapeo verificadas,
  de los supuestos y de la versión del adaptador. La lectura de vuelta repite la huella de entrada.
- Regla: un resultado o volcado se acepta como vigente solo si su huella coincide con la regenerada; si no, `STALE`. Sin persistencia
  nueva en el DWG.

### D-13 — Capacidad para ID30 y comando mínimo (A65-V1-09, A65-V1-11; AQ-05)

- Neutral (Application): `BuildStructuralModel(fuente, alcance, cargas, perfiles) → Outcome`, `CompareReadbacks(a, b, contrato) →
  informe`. Proveedor (`Providers/RamElements/`): `CreateRamBuildPackage(modelo, opciones) → Outcome<contenido>` y el lector de
  volcados. `Analyze` y `ReadResults`: no existen (NOT EXPOSED).
- Comando de línea del Plugin, **un archivo nuevo** (lista de permiso de G1): seleccionar un rack Cantilever, pedir estación, archivo de
  cargas y carpeta nueva; escribir el paquete de forma atómica; listar diagnósticos. Prohibido (guarda de fuente del Plugin): suscribirse a
  eventos de AutoCAD, `SetImpliedSelection`, WPF, PaletteSet, edición de menús, consumir código de ID30.

### D-14 — Análisis y resultados (Q-O5; AQ-09)

**Fuera de V1 (A65-V2-04; Owner Q-O5 = APPROVED)**; solo se añade por decisión del Owner más una A-n. Hechos medidos para una futura
iniciativa (**[P1✓]**, solo caracterización): `analyze()` es síncrono y funciona; se leen desplazamientos, reacciones y fuerzas de
extremo; RAM incluye deformación por cortante; los textos de avisos/errores no se exponen (solo conteos); **escribir después de analizar
bloquea el script con un diálogo modal**; el solver crea `NoName.fem` en `ProgramData`. Si se incluyera (F6): bandera del script que corre `analyze()` y escribe desplazamientos, reacciones y
fuerzas de extremo con la huella de entrada; **excluye** `member_design` y cualquier ratio o veredicto (ADR-0017; M-08). RackCad no
importa ni presenta resultados en esta iniciativa.

### D-15 — ADR-0050 (A65-V1-07; AQ-09)

Borrador versionado en este paquete ([ADR-0050](../adr/0050-interoperabilidad-analisis-estructural.md), estado «propuesto»), con la
cláusula de compatibilidad: el modelo de análisis es derivado y no persistido; la pulgada sigue canónica para producto, DWG, catálogos y
persistencia (ADR-0005); longitudes por `LengthUnits` (ADR-0040 D3); catálogo por `StructuralSectionUnits` (ADR-0021 §10); consume
ADR-0017 sin reabrirlo; declara que `StructuralSectionUnits` consume el propietario nuevo de la masa de libra con resultado idéntico bit a
bit (A65-V2-07). El texto revisado es el que se congela; el Owner lo acepta **antes de F1**.

## 3. Invariantes → pruebas (todas con RED)

| ID | Invariante | Prueba | RED esperado |
|---|---|---|---|
| INV-01 | Sin identidades ni convenciones de proveedor fuera de su carpeta | G1, G2 | un archivo neutral con `RamElements` o `RamX…` hace fallar G1 |
| INV-02 | Cantidades tipadas; sin literales de conversión en el análisis | tipos + G4 | un `25.4` en `StructuralAnalysis/**` hace fallar G4 |
| INV-03 | Conversiones de entrada correctas y exactas donde corresponde | lbf↔N, kgf↔N, kip, in↔mm, idas y vueltas ≤ 1e-12 | antes de la frontera, la prueba no compila o falla |
| INV-04 | Determinismo **y sensibilidad** de huellas | mismo diseño → mismo JSON y huella; cambiar geometría, sección, material, supuesto, mapeo, archivo de cargas o versión del adaptador → huella distinta | una huella constante pasa la estabilidad y falla la sensibilidad |
| INV-05 | Fallo cerrado sin paquete parcial | mapeo ausente, familia C/L, canales pareados, doble cara o `NegativeY`, brazo con inclinación > 10°, 25 cargas, carga sin unidad, nudo brazo–columna fuera de la columna → `Unsupported`/`Invalid`; fallo inyectado en la escritura → no queda carpeta | un escritor no atómico deja carpeta |
| INV-06 | Geometría analítica (IDL-01..08) | golden de `REF-CANT-01` derivado a mano | nudos en puntos de inserción de bloque o en caras → falla |
| INV-07 | Orientación **[P1✓]** | regla de ejes de RAM (D-04) aplicada a las 12 direcciones y 3 giros de P1 reproduce sus vectores (tabla de P1 como fixture); vector de peralte con rotación y espejo; `θ` emitido reproduce el peralte | una rotación no nula ignorada, o `l3 = +Ẑ` aplicado a un miembro no vertical → falla |
| INV-08 | Apoyos y liberaciones con la transformación de D-04 | tabla de mapeo de banderas | un mapeo índice a índice (Ry→Ry) → falla |
| INV-09 | Cargas solo desde el archivo; posición y magnitud exactas | contrato de la carga útil | una carga derivada de producto o una posición sobre la longitud física sin la parte rígida → falla |
| INV-10 | Plantilla del script | guarda estática (imports, llamadas prohibidas, modo exclusivo, categoría como texto, puerta de capacidad antes de escribir) | quitar la puerta o añadir `save_as` → falla |
| INV-11 | ADR-0017 | guarda: sin `member_design`, ratios ni veredictos en RackCad | una llamada o tipo de ratio → falla |
| INV-12 | Capas y comando | G3 + guarda del Plugin (D-13) | un `+=` a evento de AutoCAD o `SetImpliedSelection` en el archivo de comando → falla |
| INV-13 | Comparador | volcados iguales → PASS; +1 mm, sección cambiada, eje local invertido, carga ×1000, emparejamiento ambiguo, volcado `FAILED`, huella distinta, volcado en un par distinto de `LB_IN` → FAIL/rechazo/`Invalid` | comparador permisivo → falla |
| INV-14 | Core sin RAM ni Python | guarda sobre `tests/RackCad.Tests/**`: sin referencias a `ramelements`, a un intérprete Python ni a la carpeta de instalación de RAM | una prueba Core que invoque `python` o lea `ramelements` → falla (A65-V2-08) |
| INV-15 | Unidad de entrada obligatoria | carga sin unidad o con `"lb"` ambigua → `Invalid` | aceptar un número sin unidad → falla |
| INV-16 | Propietario único de lb y gₙ; `StructuralSectionUnits` idéntico bit a bit | prueba de igualdad de bits + guarda de todo `src/**` que prohíbe `0.45359237` y `9.80665` fuera del archivo propietario, análoga a `LengthUnitsAuthorityGuardTests` (A65-V2-07) | un segundo literal en cualquier archivo de `src/**` → falla |

## 4. Matriz de equivalencia (generado frente a referencia manual)

| Hecho | Criterio | Tolerancia |
|---|---|---|
| Coordenadas de nudos | emparejamiento por posición (ambiguo → FAIL) | 0.02 in (0.5 mm) |
| Conectividad | mismos pares de nudos | exacta |
| Ejes locales 1/2/3 | vectores | 0.1° |
| Sección / material | mismo nombre de RAM | exacta |
| Propiedades de sección (A, Ix, Iy, J) de RAM frente al catálogo | informativa con (b); criterio con (a) | ±0.5 % |
| Propiedades de material (E, ν o G, peso unitario) | base de RAM contra los valores esperados con fuente de cada fila verificada (D-07); diferencia → reconocimiento del Owner o cambio de fila | E y ν exactas; peso ±0.5 % |
| Punto cardinal, offsets, factores de rigidez, axial, resorte | CENTROID, 0, 1, normal, 0 | exacta |
| Liberaciones / apoyos | banderas tras D-04 | exacta |
| Cargas: valor / posición / dirección | por miembro y caso | 0.1 % / 0.02 in / exacta |
| Combinaciones | ninguna en ambos | exacta |
| Casos sin cargas (p. ej. `CM` por defecto) | ignorados en ambos | — |

Sin filas de comportamiento (reacciones, desplazamientos) mientras F6 esté fuera.

## 5. Validación del Owner / ingeniería (A65-V1-12, A65-V1-13)

| ID | Gate | Escenario |
|---|---|---|
| OV-I65-01 | F4 (hito temprano) y READY | Exportar `REF-CANT-01` desde un DWG, correr el script sobre un modelo vacío, sin errores; volcar generado y manual **antes de analizarlos** (D-11); matriz §4 PASS salvo filas informativas |
| OV-I65-02 | F5 y READY | Cargas de `REF1`: valor, posición y dirección equivalentes (sin comportamiento si F6 está fuera) |
| OV-I65-03 | F4 y READY | Estación no soportada (C/L, canales pareados, doble cara, brazo `NegativeY` o con inclinación > 10°) o mapeo ausente o sin verificar → sin paquete, diagnóstico visible |
| OV-I65-04 | F4 y READY | El script se niega a correr sobre un modelo no vacío y no lo modifica |
| OV-I65-05 | F4 y READY | **Inyección de fallo**: paquete de prueba con un nombre de sección inexistente → `FAILED — DISCARD MODEL` en consola y en la lectura de vuelta; el comparador lo rechaza |
| OV-I65-06 | F4 y READY | **Inyección de fallo**: paquete cuya lectura de vuelta no puede coincidir (ejes esperados alterados) → `FAILED` |
| OV-I65-07 | F4 y READY | **Inyección de fallo de capacidad** (A65-V1-12): paquete cuya `__version__` esperada está alterada, o que exige una función del puente inexistente → el script se niega **antes de cualquier escritura**; después, nudos = miembros = 0 |
| OV-I65-08 | READY | Lista de la guía manual sobre el Candidato final |

- OV-I65-01..07 se **repiten sobre `FINAL_CANDIDATE_SHA`** ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §8); reducirlo exige al
  Owner. El hito de F4 es un hito intermedio del Owner: exige **su propio Candidato completo** y la entrega del DLL según
  [la guía manual](../guias/validacion-manual-autocad.md).
- Los paquetes de inyección de fallo los genera la herramienta de pruebas, no el producto.
- **Equivalencia ≠ validación de ingeniería (Owner Q-O3):** OV-I65-01..07 validan solo que el modelo transferido es equivalente a la
  referencia bajo las hipótesis declaradas. Ninguna OV valida que esas hipótesis representen el comportamiento real del rack, y ningún
  documento de I-65 lo afirmará.

## 6. Gates funcionales

| Gate | Resultado verificable |
|---|---|
| F0 | P1 ejecutada (hecho), Proposal V3 con valores medidos (esta), ADR-0050 en borrador revisado, `AGREED` Coordinator + Architect, Freeze |
| F1 (incluye F2 del mandato) | De `REF-CANT-01` y del archivo de cargas se obtiene un `StructuralModel` válido, determinista, con golden derivado a mano; fallo cerrado; unidades. **Empieza solo con ADR-0050 aceptado y autorización expresa del Owner** (IMPLEMENTATION AUTHORIZATION) |
| F3 | Paquete RAM determinista y atómico; guardas G1-G4 y de plantilla; contrato de la carga útil |
| F4 | Comando mínimo + volcado + comparador; Candidato de hito; corrida real; OV-I65-01, 03, 04, 05, 06, 07 |
| F5 | OV-I65-02 |
| F6 | **Fuera de V1** (D-14); solo por decisión del Owner antes del Freeze o por decisión del Owner + A-n |
| F7 | Conformidad, regresión, entrada de FOUNDATIONS redactada, guía de uso, ADR-0050 |
| READY | READY-01..09; OV completa sobre el Candidato final |

## 7. Fundaciones

```text
Consumes:   Rack Identity (ADR-0009); catálogo neutral de secciones (ADR-0020..0023); colocación Cantilever
            (PrismaticSectionInstance, ADR-0022/0024/0025); LengthUnits (ADR-0005, ADR-0040 D3); ADR-0017
Extends:    StructuralSectionUnits (ADR-0021 §10): consume el propietario único de la masa de libra, API y valores idénticos bit a bit
Introduces: Structural Analysis Model neutral; frontera de unidades de análisis (fuerza y propietarios de lb y gₙ); frontera de
            proveedores de análisis; supuestos explícitos; canal de cargas explícitas; huellas de fuente y de entrada
```

## 8. Propiedad y paralelismo

ID30/I-64: sin cambios respecto de V1, con la guarda de D-13. I-62: sin cambios. I-38: verificación y dimensionado siguen ahí. Autoridad
compartida → STOP y Master Orchestrator.

## 9. No-objetivos

Los del mandato y del contrato §4, más: Selectivo, Dinámico, Push Back y mezzanine en V1; estaciones de doble cara y brazos `NegativeY`;
secciones C, L y de rack perforadas; base modelada; offsets, resortes y conexiones semirrígidas; momentos nodales, cargas locales y distribuidas; peso propio; combinaciones;
importar o mostrar resultados o ratios; guardar modelos de RAM; OpenRE/STAAD/ISM/MCP.

## 10. Decisiones pedidas al Owner

| ID | Decisión | Recomendación |
|---|---|---|
| Q-O1 | Rebanada V1 | **provisional: estación Cantilever de una cara** (Owner). Condición: si OV-I65-01 muestra que la transferencia no es verificable sin hipótesis incorrectas → STOP y nueva decisión del Owner |
| Q-O2 | Fuente de propiedades | **(b) base de RAM en estudio** para la prueba controlada; **sin regla por familia**; mapeo explícito verificado por fila (D-07) |
| Q-O3 | Supuestos de D-10 | **solo hipótesis explícitas para la prueba de equivalencia** (Owner) |
| Q-O4 | Autorizar P1 | **ejecutada** (v3.2, Computer Use, 2026-10-11) |
| Q-O5 | F6 | **APPROVED: fuera de V1** |
| Q-O6 | Aceptar ADR-0050 | **PENDING**: se presenta la versión final tras la confirmación del Architect; antes de F1 |
| Q-O7 | Estación de referencia | **`REF-CANT-01` provisional** (Owner); especificación para el ingeniero en §15; la validación del Owner no se declara sin evidencia real. Sustituirla después del Freeze = decisión del Owner + A-n, con IDL sin cambios y un golden nuevo derivado a mano (A65-V3-04) |
| Q-O8 | Autorizar F1 (implementación) | **no pedida todavía**; requiere AGREED + ADR-0050 aceptado + autorización expresa |

## 11. Ejecución con I-61 (tras el Freeze)

Sin cambios respecto de V1 §11. El contrato se fija en el Freeze: `requires_plugin_build: true`, `requires_autocad: true` (D-13,
OV-I65-01) y `requires_owner_validation: true` (A65-V1-19).

## 12. Riesgos

Los de V1 §12, más: el hito de F4 exige un Candidato completo (costo de tiempo del Owner); el número 0050 puede chocar con otra
iniciativa antes de integrar (se renumera, no se reserva). Medidos en P1: validación de licencia lenta con estado «Desconocido» (cada
corrida puede requerir al Owner); un nombre de sección podría existir en dos bases de país (UNKNOWN; lo detecta la fila de propiedades);
error gRPC benigno al cerrar RAM; la API 0.1.0 puede cambiar (puerta de capacidad); la base de usuario «Montilla» no localizada.

## 13. Disposición de la ronda 1

| Hallazgo | Disposición |
|---|---|
| A65-V1-01 | §0 FP-01/FP-02 y regla STOP; marcas [P1]; Freeze imposible sin P1 |
| A65-V1-02 | P1 v2: puerta de modelo vacío y de `OUT_DIR` antes de la primera escritura; S9 condicionada |
| A65-V1-03 | P1 v2: N1/N2 primero con nudos fijos y sin coincidencias; miembros en ±Z de RAM, inclinados, invertidos; `__file__`, directorio, decodificadores y base activa. D-02 recortado: momentos nodales y cargas locales fuera de V1 |
| A65-V1-04 | D-05 IDL-02..10, origen y familias; SUP-02 = A; `REF-CANT-01`; golden a mano |
| A65-V1-05 | D-03 tabla de propietarios; lb y gₙ con propietario único; `StructuralSectionUnits` Extends bit a bit; G4 acotada |
| A65-V1-06 | D-03 exactitud; tolerancia relativa ≤ 1e-12 |
| A65-V1-07 | D-03 en pulgadas (ADR-0005); D-15 y borrador de ADR-0050 con cláusula; M-08 evaluado en la adenda de Discovery |
| A65-V1-08 | D-06/D-07 fuente de propiedades (Q-O2); filas §4 con tolerancias; script fija y lee punto cardinal y demás |
| A65-V1-09 | D-01 G1 con alcance y tokens; `CreateRamBuildPackage` en la carpeta del proveedor |
| A65-V1-10 | D-10b canal de cargas con unidad explícita; en la huella |
| A65-V1-11 | §3 con RED en todas las filas; INV-04 sensibilidad; INV-08 con transformación; INV-05 atómico; INV-10 plantilla; INV-12 Plugin |
| A65-V1-12 | D-08 puerta de capacidad medible; OV-I65-05/06; comparador rechaza `FAILED` |
| A65-V1-13 | §5 repetición sobre el Candidato final; Candidato de hito en F4; OV-I65-02 sin comportamiento |
| A65-V1-14..19 (OPTIONAL) | incorporados: D-12, D-04, IDL-08, D-02, D-09, §11 |

## 14. Disposición de la ronda 2 (sobre V2, `8f9ac48c`)

| Hallazgo | Disposición en V2.1 |
|---|---|
| A65-V1-01 | `OPEN — BLOCKED BY P1` (aceptado por el Architect como correcto por diseño) |
| A65-V1-12 (residual) | OV-I65-07: inyección de fallo de capacidad, rechazo antes de escribir, nudos = miembros = 0 |
| A65-V2-01 | P1 v3: (a) todos los setters de la regla 7 se ejercitan y se leen; la regla 7 pasa a leer los valores por defecto y fijar solo `cardinal_point`; (b) valores no redondos leídos en `LB_IN` y `N_MM` → tolerancias del script en V3; (c) innecesario con una sola cara (V2-02); P1 v3 mide el sentido real del brazo inclinado `PositiveY`; (d) la puerta aborta si hay artefactos de una corrida anterior |
| A65-V2-02 | V1 solo de una cara `PositiveY`; doble cara y `NegativeY` → `Unsupported` (IDL-10, INV-05, OV-I65-03) |
| A65-V2-03 | D-11: el volcado registra el par activo y no llama `units.set`; el lector acepta solo `LB_IN` (otro → `Invalid`), con el cambio de unidades en la interfaz condicionado a P1-U1; RED en INV-13 |
| A65-V2-04 | D-14: F6 congelado fuera de V1; Q-O5 en FP-03 |
| A65-V2-05..09 (OPTIONAL) | incorporados: G1 con Plugin y UI; entradas del golden y nudo fuera de la columna; ADR-0050 y guarda de todo `src/**` para lb y gₙ; RED de INV-14 y reconocimiento del Owner de deltas > ±0.5 %; cargas manuales como cargas en miembro |

## 15. Especificación de la referencia manual `REF-CANT-01` (provisional; para el ingeniero)

Propósito: construir a mano en RAM Elements el modelo con el que se compara el generado. No es una validación de ingeniería (Q-O3).

1. **Modelo:** RAM Elements 26.00.01.015, File > New; no partir de otro modelo.
2. **Ejes:** Y vertical hacia arriba; la estación vive en el plano Y-Z de RAM; brazos hacia −Z de RAM (`X_RAM = X`, `Y_RAM = Z`,
   `Z_RAM = −Y` del datum local de la estación; D-04).
3. **Nudos:** solo los de IDL-02..05 (pie de columna, cabeza de columna, intersección brazo–columna por nivel, punta de brazo), con las
   coordenadas de la **tabla golden** de INV-06, que se deriva a mano de las colocaciones resueltas de `REF-CANT-01` en F1 (no existe
   todavía; no se inventa).
4. **Miembros:** columna partida en los nudos brazo–columna; un miembro por brazo, de su nudo de intersección a su punta; sin base
   (SUP-02 = A); sin offsets rígidos ni resortes; punto cardinal centroide.
5. **Secciones y materiales:** los de las filas verificadas del mapeo (D-06/D-07), por nombre exacto de RAM.
6. **Orientación:** peralte de la columna y de los brazos en el plano de la estación (flexión fuerte `M33` por cargas verticales).
7. **Apoyos:** pie empotrado (SUP-01); todos los nudos con `Tx`, `Ry` y `Rz` de RAM restringidos (SUP-03 mapeado).
8. **Cargas:** un caso con las cargas del archivo de cargas de la prueba, aplicadas como **cargas puntuales en miembro** sobre cada brazo,
   dirección global −Y de RAM, posición en % del miembro analítico (D-10b); sin peso propio ni combinaciones.
9. **Volcado:** ejecutar el script fijo de volcado (F4) sobre el modelo abierto **antes de analizarlo**; si el modelo se construyó en
   lb-in no hay cambio de unidades; si aparece el diálogo «los resultados se perderán», responder **No** y detenerse (D-11). No guardar a
   menos que el ingeniero lo decida por su cuenta.
10. **Unidades recomendadas:** construir la referencia en lb-in evita cualquier cambio de par durante el volcado.

## 16. Disposición de la ronda 3 y de P1

| Elemento | Disposición en V3 |
|---|---|
| A65-V1-01 | P1 ejecutada (v3.2); valores medidos incorporados (marcas **[P1✓]**); sin falla de clase STOP; RAM resultó Y arriba (no Z) |
| A65-V21-01 | cerrado por el Architect en `acaefc1d` |
| A65-V21-02 (OPTIONAL) | OV-I65-03 incluye doble cara y `NegativeY` |
| Cambios materiales derivados de P1 o del Owner (para confirmación) | (1) D-11: el volcado fija `LB_IN` por la API y restaura el par (antes: nunca `units.set`); (2) D-07: sin regla por familia, material en la fila verificada (Owner Q-O2); (3) D-08: caso por defecto admitido; (4) D-09 reglas 9 y 10; (5) §15 especificación de la referencia; (6) FP-05 |

## 17. Disposición de la ronda 4 (sobre V3, `d077a1b4`)

| Hallazgo | Disposición en V3.1 |
|---|---|
| A65-V1-01 | CLOSED por el Architect |
| A65-V21-02 | CLOSED por el Architect |
| A65-V3-01 | D-11: `units.set` solo si el par no es `LB_IN`, con restauración en `try/finally`; volcado antes de analizar; diálogo → «No» y STOP; §15 pasos 9-10; OV-I65-01; afirmación sobre la interfaz reclasificada como `INFERENCE` en D-03 |
| A65-V3-02 | D-07: valores esperados de E, ν (o G) y peso con fuente de ingeniería en cada fila verificada, nunca copiados de RAM; §4 y ASM-PROP-01 comparan contra ellos; diferencia → reconocimiento del Owner o cambio de fila |
| A65-V3-03 (OPTIONAL) | «perfil de material» sustituido por «filas de mapeo verificadas» en D-02 y D-12 |
| A65-V3-04 (OPTIONAL) | Q-O7: sustitución después del Freeze = Owner + A-n, IDL sin cambios, golden nuevo a mano |
| A65-V3-05 (OPTIONAL) | `p1-results.md` registra los dos hashes del informe (archivo original CRLF y blob LF de Git) |
| A65-V3-06 (OPTIONAL) | IDL-10: brazos con más de 10° respecto de la horizontal → `Unsupported` |
| A65-V3-02 (residual, cierre por diff) | frase residual de D-07 sustituida: los valores esperados vienen solo de cada fila verificada y no son autoridad del análisis |
| A65-V31-01 | INV-05 (y OV-I65-03) incluyen «brazo con inclinación > 10°» |
