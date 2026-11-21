# PB-T4b: las dudas del generador y de la emisión (fase 1, sobre la emisión de PB-T4a y la v4.3)

Este documento transcribe las dudas del redactor de `make_pbt4b_contract.py` y de `issue_pbt4b.py` (2026-11-20). La fase 1 trabaja por objetos git sobre la emisión de PB-T4a (`5aaacce3`, F4B-E50), antes de su cierre. No contiene decisiones de la sesión. Cada duda lleva la lectura del redactor, para que la versión siguiente del paquete o CA-T4b la confirmen o la corrijan.

## Lo que hay

- **`make_pbt4b_contract.py`** (fase 1; se arma con `_t4b/build_t4b.py`). Es ASCII puro y no escribe ningún contrato: sin `--dry`, aborta. El seco sobre `5aaacce3` da rc 0 (`dry-pbt4b-fase1.txt`), con 15 PENDIENTES y 5 AVISOS. Comprueba:
  - el anclaje en la emisión de PB-T4a (X `27c13fab` y BASE `5aaacce3`), con `src/` y `tests/` iguales a ella;
  - la numeración prevista, que va en parámetros: F4B-E51 (el cierre de PB-T4a), F4B-E52 (la emisión) y las secciones 279, 280 y 281;
  - el contrato de PB-T4a: las 17 paradas estándar, PBT4B-C11 sacada de PBT4A-C11 (cambiando PB-T4a por PB-T4b), la primera evidencia y sus 38 filtros;
  - nc1 y nc2 v2 con `--task PBT4`, y la Snapshot K;
  - la lista de 470 en la revisión: su sha256 `44393b19…` y que es, byte a byte, el filtrado de `pbt4-substitution-lists.tsv` (D-PBT4-11);
  - sus recuentos: 428 (375 mecánicas y 53 a mano, con el desglose de la v4.3), 15 y 27;
  - cada prueba de la lista, una sola vez en su archivo, sin `Skip` y con su línea en su rango;
  - las 9 clases, enteras, frente al mapa de la copia, con el sha256 de cada original;
  - el alcance que nombra el §6.2: lo que existe, lo nuevo que no existe y que ninguna ruta es de `Pilot/`;
  - los filtros heredados con los `MinSelected` que dejan los `Skip` (abajo);
  - el §6.2 de la v4.3: su `Objective`, en ASCII.
- **`issue_pbt4b.py`** (fase 1): `--selftest` y `--dry-objects` dan rc 0, y `--dry` y `--commit` abortan.
  - La numeración del generador es la de la revisión: F4B-E50 y la 278 son las últimas.
  - Muestra las 8 líneas del estado de la emisión de PB-T4a.
  - De lo que hay en `hs65/pbt4`, 150 archivos son iguales a lo que custodió X de PB-T4a, y ninguno ha cambiado.
  - Hay uno sin clasificar, `_msg.txt` (de otro autor; no lo he leído).
  - La redacción de la copia `<unidad>:\r4b` como `<copia>` funciona.
- **Las negativas** (`_neg/run_neg_pbt4b.py`): 17 de 17.

## Las dudas

1. **La fila de CT-PBT4A-06 de la lista** dice «`CT_PBT4A_06 (nueva en PB-T4a)`» en `(PB-T4a) tests/RackCad.Tests/PushBackFullReadGuardTests.cs`, con las líneas «-». La lista se midió en `7f0d9aa3`, cuando esa prueba no existía.
   - **Propuesta:** el generador lee el nombre real en el cierre de PB-T4a: exactamente un método que empieza por `CT_PBT4A_06`, sin `Skip`.
   - La lista guardada no cambia, porque su sha256 la fija.
   - El contrato nombra el método real. Su `Skip` y su sustituta, CT-PBT4B-13, van en el registro declarado.
   - La versión siguiente del paquete lo dice así, o fija otra regla.
2. **11 pruebas de la lista son `Theory` de `MemberData`.** El criterio de `count_tests_git.py` no las cuenta por texto: son 2 de `WorkspaceCommandAdvisorTests`, 2 de EIH, 3 de `WorkspacePilotPrepareTests`, 1 de la UI de «Conservar», 1 de `WorkspacePilotUpdateHostTests` y 2 de `WorkspacePilotUpdateRequestTests`.
   - Los recuentos estáticos de los filtros no las restan.
   - **El total exacto de cada rama** (F4B-E46) tiene que medirlo la precondición 6 en ejecución, con sus filas de datos.
3. **Los `MinSelected` heredados, con los `Skip` de la lista,** medidos en `5aaacce3`:
   - Core `RackCad.Tests.Workspace`: 3 261 → **2 724** (537 resultados saltados).
   - UI `RackCad.UI.Tests.Workspace`: 547 → **521** (26).
   - IG: 123 → **121**.
   - `PushBackWritePathGuardTests`: 2 → **1**.
   - `PushBackFullReadGuardTests` (de PB-T4a): 3 → **2**, por CT-PBT4A-06; esa clase existe tras el cierre de PB-T4a.
   - Los demás no cambian, y ningún filtro heredado se queda sin pruebas.
   - Las copias van en `RackCad.Tests.FullPanelCopies`, así que no entran en el filtro `RackCad.Tests.Workspace` (§5.2).
4. **`NumericFieldTests`:** `count_static` atribuye sus 10 pruebas a su clase anidada `BorderBrushSource`, que se declara antes que ellas.
   - Por clase, el filtro `RackCad.UI.Tests.NumericFieldTests` cuenta 0 por texto, aunque en ejecución selecciona las 10. El generador las cuenta por archivo.
   - Si el contrato lleva ese filtro con `MinSelected` 10, su comprobación estática tiene que contar por archivo, o el filtro tiene que ser el de la clase anidada.
5. **El tamaño del contrato (F4B-E21).** Con el método de [PKC-C] aplicado a todo `tests/`, el `ForbiddenWriteScope` provisional sale con 1 081 entradas (63 KB en JSON): las rutas del alcance parten la raíz de `RackCad.Tests` y `Workspace/`.
   - **Con el precedente de PB-T3c y PB-T4a** (la lista explícita de `tests/` del contrato anterior, menos las rutas del alcance), son **452 entradas y 27 KB**, frente a 316 y 18,5 KB en PB-T4a.
     - Salen 26 rutas, y los prefijos `tests/RackCad.Tests/Workspace/` y `tests/RackCad.UI.Tests/Workspace/` se parten en 66 y 22 entradas.
     - Entran las clases de PB-T4a tras su cierre.
   - **El contrato de PB-T4a pesa 64,5 KB.** El de PB-T4b, sin el kit ni la partición en el contrato, saldría hacia 75 a 80 KB.
     - El kit literal (D-PBT4-11.3: «va literal en el contrato») y la partición, prueba a prueba (375 nombres en 18 clases), pueden añadir de 30 a 60 KB.
     - **Propuesta:** el contrato lleva la lista de 470, el kit y la partición por archivo custodiado y sha256, como ya hace con la lista, y no como texto literal en una colección.
     - Con F4B-E21 la sesión pone las colecciones opacas, así que no hay límite de salida del Controller. El tamaño solo cuenta para la delegación y el Worker.
6. **La entrada «## PB-T4b» del registro declarado** tiene 470 sustituciones. Una tabla prueba a prueba, como en PB-T4a, pasaría de 100 KB.
   - **Propuesta:** el registro lleva la tabla por clase y por causa (C-0 con sus 9 clases, C-5 y UI), con el sha256 de la lista, y la tabla de ejecución al cerrar con los recuentos por clase.
   - La versión siguiente del paquete lo fija.
7. **Los `Skip` literales.** D-PBT4-11.1 da la forma de C-0 con «`<Clase>Full`», y la de las 53 a mano con «su equivalente `<CT>` de `<clase>`». Las 15 de C-5 y las 27 de la UI no tienen forma escrita.
   - Para PB-T4a, el `Skip` era una sola línea literal en el §6.1.
   - Para PB-T4b, el contrato necesita la regla exacta de cada motivo y una comprobación mecánica que la lea: ¿CT-20 para las 9 clases, y otra para las otras 42?
8. **La partición de las copias** (`<Clase>FullRedTests` y `<Clase>FullGreenTests`) la mide la precondición 6 con el esqueleto, las copias y el kit, antes de emitir.
   - El contrato la lleva por nombre.
   - Esa medición no se puede hacer por objetos git: es de la sesión, en una copia exportada, como la compilación de PB-T4a.
9. **El orden de las líneas del estado:** la emisión de PB-T4b cambia las 8 líneas que deje el cierre de PB-T4a, no las de su emisión. En la fase 2, `issue_pbt4b.py` lee las del cierre de PB-T4a (como `issue_pbt4a.py` leía `_close_state_lines.py.txt` del cierre de PB-T3c).
10. **`_msg.txt`** en `hs65/pbt4`: no es mío, y la custodia lo da sin clasificar. La sesión dice si se excluye.

## Lo que falta en la v4.3 para PB-T4b

Lo lista el seco, uno a uno, como PENDIENTE:
- el bloque ASCII de literales, como el 6.1.1: TaskId, `Objective`, INV-PBT4B-*, PBT4B-C01 a C20 (con C02 sin excepción fuera de sus `Skip`, C11 y C20) y la `ExpectedEvidence`;
- la tabla de `RequiredTests`, con proyecto, filtro, `MinSelected` y `ExpectRed`;
- el `AllowedWriteScope` exacto, ruta a ruta: hoy es prosa;
- el §5.2 entero: CT-PBT4B-01 a 18 con su clase y su `ExpectRed` (la v4.3 remite a la v3 y a la v4);
- el total exacto de cada rama;
- las filas de PB-T4b de la tabla de RR-3: una por cada uno de los 53 equivalentes a mano;
- los `Skip` literales;
- el kit literal;
- la partición medida;
- las líneas de CT-PBT4B-06;
- el esqueleto exacto del RED y, si lo prescribe, el GREEN literal, con la regla del §5.0 sobre los `?` (CA-T4a I-01).
