# I-65 — F4: implementación (concreción del Coordinator)

Documento de la unidad (Coordinator, 2026-10-11). Concreta, **dentro del Freeze** (Proposal V3.1: D-01, D-02, D-03, D-04, D-05, D-06,
D-07, D-08, D-09, D-11, D-12, D-13; §3 INV-01..16; §4; §5; §6 fila F4) y de ADR-0050, lo que F4 implementa por orden del Owner
([I-65-owner-authorization-f4.original.txt](../automation/decisions/I-65-owner-authorization-f4.original.txt)), a partir del paquete de
preparación revisado por el Architect ([I-65-f4-preparation.md](I-65-f4-preparation.md) y
[I-65-f4-engineering-validation-package.md](I-65-f4-engineering-validation-package.md)). Las decisiones que lo adoptan están en
[decisiones](../automation/decisions/I-65.md) §26. Nada aquí cambia una cláusula congelada.

## 0. Autorización y fronteras

- **F4 IMPLEMENTATION AUTHORIZATION = YES, CONDITIONAL; F4 ONLY.** Se implementan y prueban los componentes de integración de F4.
- **No autorizado:** ejecutar el modelo generado (ni ningún script) en RAM Elements; declarar el Candidato de hito; declarar GATE PASS de F4
  (exige la comparación real contra la referencia manual); F5, F6, F7; capacidades fuera de V1; cambios del Freeze; infraestructura de I-62;
  interfaces de ID30. La unidad se **detiene antes de la primera corrida real** y pide autorización específica.
- **Decisiones de ingeniería del Owner** (§7): OD-F4-03 provisional; OD-F4-04 reconocida, no aprobada; OD-F4-05; OD-F4-06.

## 1. Tareas, orden y presupuesto

Cinco tareas delegadas (reparto de T4 y T5 ajustado por CD-I65-F4-09), en serie (una delegación abierta a la vez), cada una con una cadena RED → GREEN, Controller Codex `gpt-6-luna`/`high`
(planificación y verificación) y Worker `claude-sonnet-5-5`/`high`:

| Tarea | Resultado |
|---|---|
| F4-T1-PACKAGE-HARDENING | correcciones de la plantilla y del paquete heredadas de F3 (A65-F3-01, 02, 04, 05, 06, 07, 08, 09, 11; prueba T4-21) |
| F4-T2-SOURCE-ENTRY | entrada neutral de una sola lectura del diseño persistido y endurecimiento de la fuente de F1 (A65-F1-03, 08, 09, 10, 11, 12) |
| F4-T3-DUMP-AND-READERS | script fijo de volcado de solo lectura y lectores del proveedor (volcado, lectura de vuelta, huella del manifiesto) hacia tipos neutrales |
| F4-T4-COMPARATOR-AND-TOOL | comparador neutral puro con informe determinista (D-11, INV-13, matriz §4 en lo que leen los volcados; A65-F3-10) y herramienta de consola fuera del producto (script de volcado, comparación, inyección de fallos) |
| F4-T5-PLUGIN-COMMAND | comando de línea del Plugin (D-13), con la tabla en la salida de compilación y la ruta de G1 (A65-F3-12) |

Presupuesto de Codex del Owner: **28**. Por tarea: planificación 1, verificación 1, nc1-nc3 3 (nc4 no invoca) = 5; cinco tareas = 25; reserva
3 (una planificación rechazada o una reverificación). Una invocación que excediera el tope no se lanza (P-07).

## 2. F4-T1-PACKAGE-HARDENING

**Plantilla de construcción** (`RamBuildScriptTemplate`), sin cambiar D-09:

- **A65-F3-11:** todas las comprobaciones de la carga útil (par `LB_IN`; sección y material no vacíos en cada miembro; nudos de cada
  miembro y miembro de cada carga existentes; dirección `GLOBAL_Y` en cada carga; seis banderas booleanas por nudo; ids únicos) forman una
  **tercera puerta**, evaluada junto con las de capacidad y modelo vacío **antes** de abrir la lectura de vuelta y de la primera escritura.
- **T4-21 (A65-F3-03):** todo rechazo de puerta queda en `differences` con el prefijo `gate: `; el estado sigue siendo `FAILED — DISCARD MODEL`
  (D-08).
- **A65-F3-01:** el giro leído se compara **módulo 360** (diferencia envuelta a (−180, 180]) con la tolerancia de la regla 9; el valor leído
  se registra tal cual.
- **A65-F3-02:** la descripción del miembro **no** se compara; se escribe (id neutral) y se registra el valor leído.
- **A65-F3-04:** la serialización y la escritura de `ram-readback.json` van dentro del manejo de errores; si fallan, se escribe un respaldo
  mínimo con estado `FAILED — DISCARD MODEL`, la huella y el error; el archivo siempre se cierra; la consola imprime el estado.

**Paquete:**

- **A65-F3-07:** `RamBuildPackage` expone la carpeta embebida (la de las opciones, idéntica a `packageFolder` de la carga útil); quien
  publica el paquete usa esa misma cadena como destino (el comando de T5 lo hace y su guarda lo comprueba).
- **A65-F3-08:** la regla de carpeta rechaza (`Invalid`, código existente) segmentos que terminan en punto o espacio y nombres reservados
  de Windows (`CON`, `PRN`, `AUX`, `NUL`, `COM1`-`COM9`, `LPT1`-`LPT9`, sin distinguir mayúsculas y con o sin extensión).
- **A65-F3-09:** prueba de la carrera del destino con el sistema de archivos falso (el destino aparece entre la comprobación y el renombrado):
  fallo sin tocar el destino y sin restos temporales; el escritor solo cambia si la prueba revela un defecto.

**Deudas de guardas:**

- **A65-F3-05:** los dos literales partidos para esquivar INV-14 (`StructuralAnalysisRamElementsGuardTests.cs` y
  `StructuralAnalysisRamElementsAssumptionsTests.cs`) se escriben enteros y esos dos archivos se eximen **por ruta exacta** en la lista de
  INV-14 (que pasa de 2 a 4 rutas exactas, con autoprueba). Ninguna otra exención.
- **A65-F3-06:** la prueba de F1 que buscaba tipos con «Writer» pasa a afirmar que, en el espacio de nombres neutral de `StructuralAnalysis`,
  la E/S de archivos solo aparece en `StructuralPackageWriter.cs` (y siguen sin existir `Analyze` ni `ReadResults`).

## 3. F4-T2-SOURCE-ENTRY

- **A65-F1-08:** una entrada neutral (Application, `Systems/Cantilever`) recibe el `RackId`, el `Kind`, el índice de estación, los **bytes**
  del JSON `Design` tal como están persistidos en el sobre, una **función de decodificación** de esos bytes que aporta quien llama (el comando
  del Plugin, con el almacén de proyecto) y los bytes del archivo de cargas; aplica esa función a **esos** bytes y los hashea intactos (D-12).
  La estación no depende de la persistencia (guarda `CantileverSourceGuardTests`, I-37C; CD-I65-F4-11). Mismos bytes → misma
  `SourceFingerprint`; un cambio solo de espacios → huella distinta.
- **A65-F1-09:** excepciones de la deserialización, de la fuente o del resolver → `Unavailable` con diagnóstico, sin modelo; el constructor
  rechaza (`Invalid`) un modelo vacío, apoyos duplicados por nudo e ids de supuesto ausentes.
- **A65-F1-11:** un bloqueo de la línea completa tiene su propio código de diagnóstico, distinto del de una estación bloqueada.
- **A65-F1-03:** IDL-09 como lista de permiso: solo W, HSS-RECT y S; cualquier otra familia → `Unsupported`.
- **A65-F1-10:** la re-ortogonalización del vector de peralte se acota a la tolerancia angular de IDL-08 (0.01°); una componente mayor →
  `Unsupported`.
- **A65-F1-12:** una pieza estructural del plano de la estación que la fuente no reconoce → fallo cerrado; la línea completa de
  `REF-CANT-01` (base, placas, separadores, arriostramientos y cartelas, no modelados por CD-I65-F1-14) sigue `Valid` con el golden de INV-06
  sin cambio.

## 4. F4-T3-DUMP-AND-READERS

- **Script de volcado** (proveedor, plantilla fija, D-11): imports solo del módulo del proveedor (y su `get_core`), `json` y `math`; puerta de
  capacidad (versión `0.1.0` y funciones del núcleo, todas dentro de las 207 de P1); **solo lectura** del modelo abierto (sin `add`, sin
  setters, sin `analyze`, `save`, `save_as`, `new`, `os`, `sys`); par inicial registrado; **solo si** no es `LB_IN`, `units.set(LB_IN)` y
  restauración en `try/finally`; un único archivo `ram-dump.json` abierto en modo exclusivo dentro de la carpeta embebida; vuelca nudos (id,
  descripción, coordenadas, banderas), miembros (id, descripción, nudos I y J, sección, material, giro, ejes locales, punto cardinal, offsets,
  factores, comportamiento axial, resortes, liberaciones), casos (id, categoría como texto, peso propio, cargas puntuales de cada miembro con
  dirección, valor, distancia y bandera de porcentaje) y combinaciones (id y términos). Toda llamada no ejecutada en P1 se toma de la fuente
  instalada del paquete (leída, no ejecutada) y la cubre la puerta de capacidad.
- **Lectores del proveedor:** volcado (`rackcad-ram-dump/v1`), lectura de vuelta (`rackcad-ram-readback/v1`) y huella del manifiesto; estrictos;
  convierten al **marco neutral** (inversa de D-04: banderas y vectores) y a cantidades tipadas; la posición de cada carga se normaliza a
  distancia absoluta desde el nudo I (A65-F4P-02). Volcado no leído en `LB_IN` → `Invalid("unit pair")`; volcado o lectura de vuelta con
  estado de fallo → rechazo.
- **Tipos neutrales** de lectura (sin identidad de proveedor, G1/G2) que consume el comparador.
- **INV-14** (CD-I65-F4-13): ninguna exención nueva ni edición de pruebas existentes; las pruebas de la plantilla del volcado
  obtienen el nombre del módulo del texto de la plantilla de construcción (precedente F4-T1) y no parten literales.

## 5. F4-T4-COMPARATOR-AND-TOOL

Comparador neutral puro (`CompareReadbacks`, D-13) del volcado del generado contra el del manual, con la lectura de vuelta y la huella del
paquete: empareja nudos por posición (0.02 in; ambiguo → FAIL) y miembros por extremos; evalúa las filas de la matriz §4 que leen los
volcados (paquete de ingeniería §3 F): coordenadas, conectividad, ejes locales (0.1°), sección y material por nombre, punto cardinal,
offsets, factores, axial y resortes, liberaciones y apoyos, cargas (valor 0.1 %, posición 0.02 in, dirección exacta), combinaciones
(ninguna en ambos); ignora los casos sin cargas; exige categoría `"OTHER"` en el generado y deja la del manual como informativa; marca
`NOT_EVALUATED` las filas de propiedades de sección y material (las evalúa la hoja firmada). Rechaza (`STALE`, `FAILED`, `Invalid`) según
D-11 e INV-13. Informe JSON determinista. Las tolerancias son las del Freeze §4; **no se alteran** (OD-F4-04).

- **Herramienta de consola fuera del producto** (D-11; precedente `tools/RackCad.StructuralSections.Import`): proyecto nuevo bajo `tools/`,
  en la solución y referenciado por el proyecto de pruebas Core (que así lo compila y prueba en la CI); no entra al paquete del Plugin.
  Subórdenes: generar un script de volcado en una carpeta **nueva** (publicación atómica, nunca sobrescribe); comparar (paquete, volcado
  generado, volcado manual → informe y código de salida); generar los **paquetes de inyección de fallo** de OV-I65-05/06/07 a partir de un
  paquete válido, con una sola mutación semántica cada uno (sección inexistente; ejes esperados alterados; versión o función del núcleo
  alteradas) y la carpeta embebida nueva, marcados en su manifiesto como inyección; el producto no puede emitirlos. Rutas: el proyecto nuevo
  bajo `tools/`, una entrada en `RackCad.sln` y una `ProjectReference` en `tests/RackCad.Tests/RackCad.Tests.csproj` (CD-I65-F4-09).

## 6. F4-T5-PLUGIN-COMMAND

- **Comando de línea del Plugin** (D-13), un archivo nuevo en la ruta exacta de la lista de permiso de G1: selecciona un rack Cantilever,
  pide estación, archivo de cargas y carpeta nueva; lee el sobre **una vez** y usa la entrada de T2; lee la tabla distribuida con su lector
  (A65-F3-12: nunca otro perfil de material); publica en la carpeta embebida del paquete (A65-F3-07); captura toda excepción como
  `Unavailable`; lista diagnósticos. Prohibido: eventos de AutoCAD, `SetImpliedSelection`, WPF, `PaletteSet`, menús, código de ID30 o de
  `Plugin/Workspace`.
- **Distribución de la tabla** (CD-I65-F4-10): `src/RackCad.Plugin/RackCad.Plugin.csproj`, solo para añadir
  `assets/structural-analysis/ram-elements/section-material-map.json` a la **salida de compilación** (el DLL Debug que usa la validación
  manual). El paquete del autoloader exigiría cambiar `deploy/verify-bundle.ps1`, fuera de las rutas autorizadas: queda diferido; sin la
  tabla, el comando responde `Unavailable` con un diagnóstico visible.

## 7. Decisiones de ingeniería del Owner (orden de F4 §3-§4)

| ID | Decisión del Owner | Efecto en F4 |
|---|---|---|
| OD-F4-03 | Base de RAM como fuente de las propiedades, **provisional**, condicionada a verificar las filas contra una referencia técnica independiente | la fila §4 de propiedades sigue informativa (±0.5 %) y la evalúa la hoja firmada; ninguna fila pasa a `verified` sin esa verificación |
| OD-F4-04 | Diferencia de peso unitario −0.62 % **reconocida y registrada, no aprobada** como equivalencia dentro de ±0.5 % | no bloquea las pruebas geométricas y de transferencia (V1 sin peso propio); tolerancias sin cambio; nada la presenta como validada; un análisis futuro con peso propio exige resolverla |
| OD-F4-05 | θ = −90° en las columnas de `REF-CANT-01`, como convención de transferencia | especificación manual §2.3 confirmada |
| OD-F4-06 | Archivo de cargas `P1`-`P3` del piloto F1 | archivo de OV-I65-01; las filas de cargas de §4 cuentan |

Estos valores son referencias de equivalencia; no constituyen validación del diseño estructural real.

## 8. Condiciones previas a la primera prueba real (orden de F4 §5)

1. Correcciones obligatorias de F4 implementadas y verificadas. 2. Dos filas verificadas por ingeniería con responsable, fecha y fuentes.
3. Referencia manual `REF-CANT-01` preparada. 4. Paquete generado correctamente y validado por pruebas automatizadas. 5. Plan de ejecución
y condiciones de parada confirmadas. Con ellas cumplidas, el Coordinator pide al Owner autorización específica para ejecutar en RAM.

## 9. Unidad de cierre de F4: autorización, orden y presupuesto (2026-10-13)

Autorización del Owner (decisiones §41, OD-RET y OD-GO): **F4-T6 + F4-T5 ONLY**, en el entorno original con el riesgo residual aceptado; sin
RAM, sin F5-F7, sin GATE PASS de F4 antes de la validación real. Dos cadenas en serie (una delegación abierta a la vez), cada una con
RED → GREEN, Controller Codex `gpt-6-luna`/`high` (planificación y verificación), Worker `claude-sonnet-5-5`/`high` y controles negativos:
primero **F4-T6-COMPARATOR-HARDENING** (§10) y después **F4-T5-PLUGIN-COMMAND** re-emitida como corrección (§11). Codex: tope 46 (28
consumidas); `attempts` 3 y `max_attempts` 6 sin reinicios. Controles operativos K1-K6 de CD-I65-F4-32 y condición C-12 en ambos contratos.
Al terminar: revisión del Architect de T6 y T5 y cierre de la parte implementable con la suite Core completa y la CI del SHA exacto.

## 10. F4-T6-COMPARATOR-HARDENING

Concreta las observaciones A65-F4-01..06 de la revisión del Architect con las decisiones del Owner (§40-§41), sin cambiar el Freeze ni sus
tolerancias. Rutas: `StructuralReadbackComparison.cs`, `StructuralReadback.cs` (solo añadidos), `Providers/RamElements/RamTransferReaders.cs`,
`Providers/RamElements/RamDumpReader.cs`, `tools/RackCad.StructuralAnalysis.RamTool/` (sin su `.csproj`) y pruebas nuevas; ediciones
nombradas de `StructuralAnalysisComparisonRowTests.cs`, `StructuralAnalysisComparisonToolTests.cs` y `StructuralAnalysisComparisonSupport.cs`.

- **A65-F4-01:** nudos emparejados por distancia **euclídea** ≤ 0.02 in (diagonal (0.015, 0.015) → FAIL; (0.010, 0.010) → PASS); el valor
  de carga se compara con la diferencia relativa respecto del valor **manual** (m = 999, g = 1000 → FAIL) y, si el manual vale 0, en valor
  absoluto con 1e-6 lbf (CD-I65-F4-31).
- **A65-F4-02:** con combinaciones no leídas el resultado global nunca es PASS: resultado `Incomplete` (`INCOMPLETE`), opción
  `--combinations-decision <texto>` obligatoria y registrada en el informe, código de salida propio.
- **A65-F4-03:** la lectura de vuelta se lee con sus `values` hacia un tipo neutral; el volcado del generado debe coincidir con ellos
  (si no, REJECTED); la herramienta rechaza la misma ruta o los mismos bytes como generado y manual; el informe lleva, cuando quien llama
  la aporta, una sección `inputs` con huellas y SHA-256 de las entradas.
- **A65-F4-04:** la herramienta reconoce la marca de inyección (solo en `tools/`), evalúa la lectura de vuelta y nunca aprueba (REJECTED).
- **A65-F4-05:** categoría vacía exacta solo en casos sin cargas; ausente, null, solo espacios o vacía con cargas → Invalid; semántica NO
  MEDIDA (CD-I65-F4-34).
- **A65-F4-06:** ensayo general con fixtures sintéticos del paquete de validación §2 (4 decimales, −90 y 270, porcentajes de 3 decimales,
  otros ids y orden, caso `CM`): PASS; FAIL con +90, con porcentaje de 1 decimal y con un miembro invertido.

## 11. F4-T5-PLUGIN-COMMAND: re-emisión como corrección

Mismo alcance e invariantes que §6 y que el contrato de `R20261011T215537Z-f944`, re-emitidos contra la `AuthorityRevision` vigente y
como corrección (CD-I65-F4-22, CD-I65-F4-30). Añade la regla de censos de CD-I65-F4-33 (I-64 no integrada: de 37 a 38 solo con
`RACKRAMPAQUETE`), los controles K1-K6 y la condición C-12. La lista de rutas prohibidas se recalcula al emitir para incluir las pruebas
que haya creado F4-T6.
