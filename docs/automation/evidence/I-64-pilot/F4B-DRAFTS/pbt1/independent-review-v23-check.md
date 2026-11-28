> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. El worktree se cita como `<worktree>` y las rutas del repo son relativas.

# I-64 F4B — Comprobación acotada de la v2.3 de PB-T1 (remedición) y del generador del contrato de PB-T1a

**Veredicto:** **EMITIBLE TRAS CORRECCIONES — B/I/M = 0 / 1 / 3. Decisiones nuevas para el Owner: ninguna.**

- **El importante (V23C-I01) no toca PB-T1a:** ni su contrato, ni su generador, ni sus CT.
  - Es un hueco de la guarda G-L1, la condición 10 de D2, que se usa en PB-T1b y PB-T1c.
  - Está en el texto del paquete que se custodia con PB-T1a como autoridad de las tres subtareas.
  - Se corrige en ese texto antes de custodiarlo, sin volver al Owner, y lo verifica la comprobación acotada de PB-T1b.
- **Después de esa corrección, PB-T1a se emite «normally»** (F4B-E09 y F4B-E11). Los tres menores se aplican en el mismo commit de emisión.

## 1. Método y límites

**Las entradas:** la v2.3 (citada `T23:n`), los registros de cambios v2.1 → v2.2 y v2.2 → v2.3, el apartado N de `pbt1-measurements.md`, `make_pbt1a_contract.py`, `remeasure-instructions.md`, `v22-instructions.md`, `owner-d1-d2-decisions.md` (F4B-E11) y mis dos informes anteriores.

**Contrastadas en `<worktree>`,** HEAD `cd47523b`, con `src` y `tests` iguales a `b5c09f0b`. Lectura de código y pruebas con `git -C`, `sed` y `grep`.

**El generador:** lo ejecuté solo en seco, con la orden literal del encargo (`… --dry --print`).
- Salida: `DRY allowed 9 forbidden 131 (src 59 tests 55 root 17) inv 14 stops 30 required 19 evidence 9 authorities 38 mutants-ok 15 bytes 39158 sha256 CAA2210A…7B21`.
- Es idéntica, byte a byte, a la `dry-out.txt` de la sesión; solo difieren los finales de línea.
- El worktree quedó limpio.

**Un script mío de solo lectura,** `review-tools/check_v23.py`, escrito con la herramienta de archivos y ejecutado con `python -I`:
- vuelve a contar el censo de `Name` del Plugin;
- prueba las reglas 1 y 1b de G-L1, tal como las escribe T23:546-547, frente a formas de mutación que no están en sus autopruebas;
- busca en los 49 archivos de G-L1 una ficha seguida de un indexador o de `as`/`is`.

**Incidencia de procedimiento, declarada.** Una de mis órdenes de Bash llevaba, por error, `python -I -c "pass"` delante de un `git show`. No leyó ni escribió nada (`pass`), pero la regla prohíbe `-c`. No hubo ninguna otra ejecución de Python fuera de los dos scripts de archivo.

**Límites:**
- Nada se compiló ni se ejecutó.
- Que F-03 recorra en la base la pasada reentrante (`pasadas-restaurar=2`) es lectura; lo fija el Worker con PBT1A-C07.
- La validación del contrato contra su esquema solo la hace el camino que escribe (`Test-Json`, `make_pbt1a_contract.py:374-375`), no el `--dry`.

## 2. La incorporación de D2 (v2.2)

**Las diez condiciones y el límite están cubiertos con fidelidad y de forma comprobable** (D-PBT1-15, T23:488-501), salvo el hueco de la condición 10 (V23C-I01).

| Condición | Cobertura | Juicio |
|---|---|---|
| 1. Los dos tipos | `PushBackSystem` y `RackModuleReconciliationResult` en los cuatro miembros (T23:503-508); CT-06 clase D y CT-07 parte 1 | Fiel. Detalle de nulos (V23C-M01) |
| 2. Nada más comparte | CT-07, partes 1 a 3, con `IsImmutable` y los diccionarios | Fiel |
| 3, 5. Edición y texto pendiente | CT-PBT1B-16 «editar» y CT-PBT1C-02 parte 2; el texto pendiente se arrastra a PB-T4 (no existe en PB-T1) | Fiel. Arrastre correcto (`IDraftPendingTextAdapter` es de F4/PB-T4) |
| 4. Validación | CT-16 «validar» (puerta, diagnósticos, `Pasada` inválida, regla c-02) y la ventana; arrastre a PB-T2 y PB-T4 | Fiel |
| 6. Descartar y recargar | CT-16 «descartar y recargar»; arrastre a PB-T4 | Fiel |
| 7. Comparar inicial y actual | CT-16 «comparar» con línea base compartida, y la prohibición de mutar (L-4) | **Lectura legítima** (abajo) |
| 8. El nombre tras la ventana | CT-PBT1B-15 (censo y orden) y CT-16 / CT-PBT1C-02 (`Name` no cambia); arrastre a PB-T3 y PB-T4 | Fiel y **medido**: §3 |
| 9. Mutar todo lo editable de la copia | CT-PBT1B-08 reescrito (T23:530-537) | Fiel |
| 10. Guardas de mutación futura | G-L1 (CT-PBT1B-14 y CT-PBT1C-04) y su ampliación por tarea | **Hueco no declarado** (V23C-I01) |
| El límite | PBT1x-C17 en los tres contratos, con el texto literal, y A-2 (T23:880, T23:1206-1243) | Fiel |

**Las lecturas declaradas:**
- **L-4 (la condición 7).** Con una línea base compartida, el resultado del comparador es invariante y se prueba. Con líneas base distintas, lo que se hace es prohibir la mutación (G-L1 y C17), no probar la invariancia. Es la única forma de cumplir la condición sin dejar de comparar la intención de módulos (M-15). Es una lectura de cómo se prueba, no una excepción a la condición. **No es una decisión nueva.**
- **L-5 (el alcance del límite).**
  - **PB-T4:** el límite del Owner va ligado a «inside the Workspace draft lifetime», y PB-T4 es la tarea que vive en ese tiempo de vida. Incluirla es leer el criterio del Owner, no añadirle algo.
  - **La edición clásica dentro de PB-T1:** es más estricta que la letra. Solo puede producir más paradas, nunca autorizar algo, y coincide con lo que ya prueban las condiciones 3 a 6 en la ventana. Hoy no hay ninguna mutación en ese camino (N-9), así que no bloquea nada.
  - **Es una restricción adicional legítima, no una decisión nueva.** El Owner puede estrecharla.
- **L-6 (qué es una premisa material).** Es razonable y la uso en el §3.

## 3. La remedición (v2.3)

**Las diferencias V23-01 a V23-17 son no materiales**: anclas, recuentos o texto de guardas que la v2.2 dejó «A MEDIR». Lo comprobé así:
- **Las anclas y los mutantes:** el `--dry` comprueba con `assert` los quince textos en su línea de `b5c09f0b`, incluidos W:3684 (07a) y W:3666 (10), y la unicidad de los [TC2].
- **Los objetivos se conservan:**
  - `KeepCell` llama a `Keep` (D-PBTC2-05), así que MU-07a sigue alcanzando la celda de CT-07;
  - la rama vacía de `OptionalVal` no cambió, así que MU-10 sigue alcanzando M-07;
  - con `ReadCellValues()`, una caja mixta sin escribir lleva a B el valor de A, así que MU-08 sigue discriminando.
- **D1 y CT-11:** `pb-05-topes` no tiene valores con más precisión de la que se muestra (N-8).

**Lo que la v2.3 cambió en las guardas de D2:**
- **El censo de `Name` (CT-PBT1B-15, V23-13).** **Correcto.** Mi recuento independiente da lo mismo:
  - 14 archivos del Plugin contienen `PushBack`, y en ellos hay 7 asignaciones, literales, tres de Push Back (`RackMenuCommands.cs:135`; `RackPushBackCommands.cs:98` y `:190`);
  - las anclas del orden se sostienen: `:48` → `:57`, con `DrawPushBackView` llamada una sola vez en `src/` (`:57`; su declaración en `:75`); `:170` → `:190`; `:34` → `:135`.
  - **No hay cuarto escritor.** `ProjectVariableMutationExecutor.cs:169` resuelve un sistema Selectivo propio.
- **La huella profunda (CT-PBT1B-16, V23-12).** **Correcta y necesaria:** `Snapshot` no ve lo derivado ni `Composite`.
  - Comprobé que no hay caché perezosa: `git grep -i -E "\?\?=|Lazy<|cache" -- src/RackCad.Domain` no da nada.
  - Comprobé que los consumidores de lectura (`Cortes`, `Describe`, la puerta) no llaman a `RefreshPhysicalModel`: solo lo llaman los almacenes y los constructores, sobre objetos nuevos (`RackFrameProjectStore.cs:67`, `DynamicRackSystemBuilder.cs:147-192`, `SystemRegistry.Default.cs:51-184`).
  - `Serialize` solo lee (`RackFrameProjectStore.cs:23-30`).
  - Queda una ambigüedad sobre qué referencias se miden (V23C-M02).
- **La regla 1b de G-L1 y sus autopruebas (c) y (e) (V23-11).** **Correctas en lo que prueban:**
  - mi script confirma que la (c) con molde de estilo C solo la ve la 1b;
  - la (e) solo la ve la regla 2 con el `foreach`;
  - hoy no hay ninguna coincidencia en los 49 archivos.

  **Pero las reglas no ven formas comunes del mismo archivo** (V23C-I01), y el paquete no las declara como límite.

**¿Una premisa material sin detectar?** No.
- PB-TC2 cambió solo las zonas [TC2], todas fuera de lo que toca PB-T1c («Fuera, por la letra», T23:378 de la v2).
- `Keep` tiene ahora dos usuarios más, el hueco y la longitud del módulo. Está previsto en la tabla T-8.
- La aceptación del nc1 de PB-TC2 vale solo para su intento 1 (F4B-E12). Como el sustituto de PD-F4-04 falló por su propiedad 5, **el nc1 de PB-T1a volverá casi con seguridad al Owner**. C11 y R-T1-08 ya lo prevén, así que es procedimiento y no premisa (V23-15).

## 4. El generador de PB-T1a

**El JSON que produciría coincide con el §6.1 de la v2.3:**
- **Las autoridades (38):**
  - 4 de custodia (el paquete, el registro, IRT1-1 y la comprobación de la v2);
  - 26 existentes, incluidas F4B-E11 y `owner-pbtc2-nc1-decisions.md`, todas con `in_head`;
  - el contrato;
  - 2 `UNIT_CHANGE` (W y WC, solo los mutantes);
  - 5 externas.
  - **Falta la comprobación acotada de la remedición**, que el §6.0 manda custodiar (V23C-M03).
- **`AllowedWriteScope` (9) y `ForbiddenWriteScope` (131).**
  - `src/`, 59: idéntico al de PB-TC2, comprobado con `assert`.
  - `tests/`, 55: `tests/RackCad.Tests/`, los 51 archivos curados, las dos clases de PB-TC2 y `Workspace/`.
  - La raíz, 17: igual a la de PB-TC2.
  - Sin intersección y con todo `src/` cubierto.
- **Los invariantes (14):** INV-PBT1A-01 a 06; los comunes 07 a 12 y 14; y el 13, que es la antigua C15 (N-M-07). El contenido coincide con T23:929-935 y el §6.0.
- **Los `RequiredTests` (19):** filtro a filtro y con sus `MinSelected`, los de la tabla de T23:914-925. El único con `ExpectRed` es `PushBackStateNet` (14). Las cinco clases de PB-TC se filtran por clase, sin colisionar con `PushBackDeclaredCorrection2…` (`assert`).
- **Las paradas (30):** las 17 estándar, iguales a las de PB-TC2 con P-01 de la Snapshot I; C01 a C12 con C04; y C17.
  - C11 transcribe PD-F4-01 y PD-F4-04 en la misma forma ASCII que PBTC2-C11, y añade que **ninguna** de las dos aceptaciones puntuales (F4B-E08 y F4B-E12) se extiende.
  - C17 lleva el texto literal del límite.
  - C15 ya no está entre las paradas.
- **Las celdas:** iguales a las de PB-TC2 (`assert`).
- **`IssuedBy`:** F4B-E09, la pregunta 4, F4B-E11 (D2 y «PB-T1 PLAN»), F4B-E03 y las revisiones.
- **Los 15 mutantes:** texto y línea comprobados sobre `b5c09f0b` (`mutants-ok 15`). MU-T1A-13 se ancla por línea porque su texto se repite en W:296.

## 5. PB-T1a por sí misma

**Se puede emitir y cerrar:**
- **Los mutantes:** en su línea, con su texto y con su objetivo (§3).
- **El RED:** son pruebas nuevas más los mutantes, y el GREEN se limita a `git restore`.
  - El RED deja Core en `success`: ningún texto de mutante lo lee una prueba (N-2).
  - La lectura de `Ci` `RedPart` tiene precedente (F4-E56 y [PKC]:466).
  - El régimen de corrección por §16.8 está escrito (T23:882).
- **Los CT:** los catorce de PB-T1a no cambiaron con la remedición, salvo F-03.
  - F-03 se comprueba a sí mismo (`pasadas-restaurar=2`, `frontal=A`, PBT1A-C07).
  - Por lectura, el refuerzo de 100000 lanza en `ValidateReinforcementAgainstPost`, porque `DerivedPostReinforcementHeightBox` no tiene máximo (WX:510-511). El cálculo inválido de la receta se alcanza.
- **La red para PB-T1b y PB-T1c:** las huellas de CT-14 (con F-03) y los CT de los huecos de D-5, M-03, M-04, M-07, CT-16 y M-02. La autoridad entre subtareas (INV-PBT1A-13 y la precondición 5) protege OB-06.5.
- **Un riesgo previsto, no un defecto:** el nc1 de esta cadena volverá probablemente al Owner (§3).

## 6. Los hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| V23C-I01 | **IMPORTANTE** (PB-T1b y PB-T1c; no toca PB-T1a) | **G-L1 no ve formas comunes de mutación en el mismo archivo, y el paquete no las declara como límite.** Su límite declarado es «una mutación dentro de un método llamado en otro archivo» (T23:558; R-T1-12). Mi script aplica las reglas 1 y 1b tal como están escritas, y dan **0** en todas estas formas: <br>• `state.WorkingBaseline.Structure.Modules[0].Length = 1.0;` <br>• `lastComputation.System.Structure.Modules[0] = null;` <br>• `workingBaseline.Structure.Fronts[0].Levels.Clear();` <br>• `(side.LastModuleReconciliation.Preserved as List<string>).Add("x");` <br>• `if (… .Preserved is List<string> l) { l.Add("x"); }` <br>Tampoco ve un lambda sobre una ficha (`….Modules.ToList().ForEach(m => m.Length = 1)`). La causa: la cola de la regla 1 (`\.\s*\w+(\(…\))?`) no admite `[…]`; la 1b solo admite el molde `((T)…)`; y la regla 2 no cuenta los parámetros de un lambda. La condición 10 de D2 pide que las guardas identifiquen una mutación futura, y el indexador es la forma natural de mutar un módulo. Hoy no hay ninguna de estas formas en los 49 archivos, así que la premisa de D2 se sostiene | T23:546-558; `review-tools/check_v23.py` (sección 2: regla1=0 y regla1b=0 en las seis formas; los controles (a) y (c) dan 1) | En D-PBT1-15, CT-PBT1B-14 y CT-PBT1C-04: <br>(1) la cola de la regla 1 admite también `\s*\[[^\]]*\]` en cada eslabón; <br>(2) la 1b se amplía a `<ficha>…\s+as\s+(List\|IList\|ICollection\|Collection\|HashSet\|Dictionary\|IDictionary)\s*<` y a `<ficha>…\s+is\s+(…)\s*<[^>]*>\s+\w+`, y la variable del `is` cuenta como alias; <br>(3) la regla 2 cuenta el parámetro de un lambda cuyo receptor contiene una ficha; <br>(4) autopruebas nuevas (f) indexador, (g) `as` y (h) lambda; <br>(5) el límite declarado nombra los métodos de Domain que mutan (`RecalculatePositions` y los `…InPlace`, que ya caen en «otro archivo»). <br>Solo es texto. La comprobación acotada de PB-T1b verifica que estas reglas dan 0 hoy y que sus autopruebas saltan |
| V23C-M01 | MENOR | «Exactamente esos cuatro objetos (dos por lado)» (T23:492) no admite nulos. El lado B de un rack de un sentido no tiene línea base, y el informe de B puede ser nulo (lo pone `AdoptLoadedBaseline`, ES:99-106). Además, la línea base de B no avanza con `AcceptComputation`, que solo toca el lado A | T23:492, :506; ES:99-106; DA:101-107 | «Los **no nulos** de esos cuatro miembros» en la condición 1 y en CT-07, parte 1 |
| V23C-M02 | MENOR | CT-PBT1B-16 mide «antes y después de cada bloque, para las cuatro referencias». `Pasada`, `LoadDesign` y `Restore` **sustituyen** esas referencias: si se vuelven a leer tras el bloque, la huella cambia sin mutación | T23:748; CT-PBT1C-02, parte 2 (que sí lo precisa) | «Las referencias capturadas al empezar el bloque y las que el estado adopta durante él, cada una desde su adopción», como CT-PBT1C-02 |
| V23C-M03 | MENOR | La custodia y las autoridades no cuadran. El §6.0 manda custodiar en la emisión de PB-T1a la comprobación acotada de la remedición (este informe) y los cinco scripts, pero `CUSTODY` del generador solo comprueba cuatro rutas y no lista este informe entre las autoridades. Además, el §6.0 nombra los resultados `bounded-check-v2.md`, `bounded-check-pbt1b.md` y `bounded-check-pbt1c.md`, y los archivos reales son `independent-review-v2-check.md` e `independent-review-v23-check.md` | T23:822-833; `make_pbt1a_contract.py:308-313`, `:348` | Añadir `F4B-DRAFTS/pbt1/independent-review-v23-check.md` a `CUSTODY` (autoridades 39, con su `assert`) y alinear los nombres del §6.0 con los reales |

## 7. Veredicto

**EMITIBLE TRAS CORRECCIONES — B/I/M = 0 / 1 / 3.**

- **D2:** las diez condiciones y el límite están incorporados con fidelidad. L-4 y L-5 son lecturas legítimas, no decisiones nuevas.
- **La remedición:** no cambia ninguna premisa material (L-6). Las tres guardas reescritas de D2 son correctas en lo que prueban, con el hueco de V23C-I01.
- **El generador:** coincide con el §6.1 en todo lo que pide el encargo. Solo le falta la autoridad de este informe (V23C-M03).
- **PB-T1a:** se puede emitir y cerrar por sí misma. V23C-I01 no la toca.
- **Decisiones nuevas para el Owner: ninguna.** L-1 está cerrada por D2.

**Para emitir PB-T1a sin volver al Owner** (F4B-E09 y F4B-E11):
1. Aplicar en el texto del paquete que se custodia la corrección de V23C-I01, que es mecánica y no cambia el contrato de PB-T1a, y la de los tres menores. V23C-M03 cambia el generador: autoridades 39.
2. Volver a ejecutar el `--dry`.
3. Emitir. La comprobación acotada de PB-T1b verifica las reglas nuevas de G-L1.
