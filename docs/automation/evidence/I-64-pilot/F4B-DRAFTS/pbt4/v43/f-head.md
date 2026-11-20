> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.3

**Estado:** v4.3. Aplica la comprobación acotada de PB-T4a (`independent-review-pbt4a-check.md`, CA-T4a): **EMITIBLE TRAS CORRECCIONES, B/I/M = 0 / 1 / 4.** Corrige el importante (I-01) y M-01; M-02 (las autoridades del registro de mediciones) y M-04 (el texto de F4B-E50) son del generador y de los borradores, y M-03 (los filtros de UI de la precondición 6), de la sesión, que este paquete recoge en la precondición 6. Lo demás, como en la v4.2: la §16 de la revisión, las decisiones del Owner (`owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`) y las dudas del generador de PB-T4a.

**La base:** el cierre de PB-T3c, `7f0d9aa3` (F4B-E48; GREEN `938a35f0`, CI `44757675821`): Core 19 129 = 19 123 + 6 omitidas; UI 2 451 = 2 430 + 21.

**Cómo se midió:** solo por objetos git en `7f0d9aa3` (`git show`, `git ls-tree` y `git cat-file --batch`), con Python `-I` y guiones escritos en `<carpeta-del-paquete>`. Nada se compiló ni se ejecutó, y el worktree no se leyó.

**Lo que sale de esta versión:**
- **PB-T4a:** el texto literal de `Draft/PushBackOriginByText.cs` ya no usa ningún `?` de tipo de referencia (`string Choose` y `pair => pair.Value`), y el §5.0 gana la regla; ninguna otra pieza de código literal del paquete la incumple (medido). **Los totales no cambian:** RED 19 137 = 19 125 + 5 + 7; GREEN 19 137 = 19 130 + 7; esqueleto 19 129 = 19 122 + 7.
- **El recorrido de las guardas, descrito como es** (por marcas, no entero), con la ampliación de CA-T4a sobre los 97 archivos de fuera; y **un guion nuevo, `apply_green_pbt4a.py`,** que escribe en una copia exportada el `src/` del GREEN literal, para que la sesión corra la suite completa de Core sobre él y cuente los avisos (§11, precondición 6).
- PB-T4b, PB-T4c y PB-T4d, sin cambio de fondo.

**Los hechos,** en `pbt4-measurements.md` (R-0 a R-43; R-42 y R-43 son de la v4.3). **Los guiones y sus salidas:** los de la v4.2 (en su cabecera, `I-64-f4b-pbt4-gate-contract-package-v42.md`) y, **en la v4.3:**
- `green_pbt4a.py`, regenerado (I-01);
- `make_measure_v43.py` → `measure_pbt4_v43.py` → `measure_pbt4_v43-out.txt` (36 de 36 en verde y 3 de 3 contrafactuales en rojo, con la regla nueva y su contrafactual sobre el texto de la v4.2);
- `walk_guards_pbt4a.py`, vuelto a correr sobre el GREEN de la v4.3 (`pbt4a-guard-walk.tsv`, `pbt4a-guard-walk-out.txt` y `pbt4a-guard-walk-flagged-v43.txt`);
- **`apply_green_pbt4a.py`** y su autoprueba, `selftest_apply_green.py` (`selftest_apply_green-out.txt`);
- `make_v43.py`, que arma esta versión desde la v4.2 y los fragmentos de `v43/`;
- y, de CA-T4a, `cat4a_walk_gap.py`, `cat4a_added_lines.py` y sus salidas, a custodiar.

---

## Registro de cambios v4.2 → v4.3

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V43-01** | **CA-T4a I-01** | **El texto literal de `Draft/PushBackOriginByText.cs`, sin `?` de tipo de referencia:** `public static string Choose(…)` y `pair => pair.Value`, en D-PBT4-03 y en la viñeta del esqueleto de §6.1. Con `Nullable=disable` (`Directory.Build.props:28`, que ningún `.csproj` cambia) y la pureza de `Draft/`, que prohíbe `#nullable` (CT-PBT3A-14 y CT-PBT4A-08), cada `string?` daba **CS8632**, ya en el esqueleto (registros de la compilación de la sesión), y `build-plugin` falla con cualquier `warning CS` (`eng/ci/verify-autocad-references.ps1`, `Assert-DotNetSuccess -NoWarnings`). Se corrige también la justificación del molde (v4.2:401): el archivo es «oblivious», y pasar un `Dictionary<string, string>` al `IReadOnlyDictionary<string, string?>` del constructor no da ningún aviso. **§5.0 gana la regla,** y las demás piezas de código literal de PB-T4a, PB-T4b y PB-T4c se revisaron frente a ella: **ninguna más cambia** (§5.0). Los totales no cambian | D-PBT4-03; §5.0; §6.1 |
| **V43-02** | **CA-T4a M-01** | **El recorrido no es «entero»:** §4.1, §5.0, §5.1, §9 y la frase de PBT4A-C16 lo describen como es, por marcas (`MARKERS`), que no ven las rutas compuestas con `Path.Combine` ni los barridos de todo `src/`, de todo Application o de todo el Plugin. Se añade la ampliación de CA-T4a: **146** archivos de prueba de fuera nombran `src/RackCad.(Application|Plugin)`, `"src"` o un ayudante de barrido, y **97** nombran como literal una carpeta antecesora de un archivo tocado o nuevo; sus literales, probados contra las 134 líneas que el GREEN añade o cambia y contra los dos archivos nuevos, solo casan con fichas genéricas, y los que leen de verdad esos archivos se leyeron: **ninguno cae**. `WorkspaceModelBoundaryTests` entra en la lista de reflexión. El riesgo que queda, en C16: una lectura que solo se ve al ejecutar; lo cierra la medida de la precondición 6 con `apply_green_pbt4a.py` | §4.1; §5.0; §5.1; §9; §11 |
| V43-03 | La recomendación de CA-T4a con M-01 | **`apply_green_pbt4a.py <dir-copia>`** escribe en una copia exportada de `7f0d9aa3` el `src/` del GREEN literal de PB-T4a (seis archivos, con LF; no toca `tests/`), tras comprobar que la copia es de la revisión y no es un checkout; imprime cada archivo con su sha256 y el sha256 del conjunto. Lo prueba `selftest_apply_green.py` | §11 |
| V43-04 | CA-T4a M-03 | La precondición 6 pide los 12 filtros de UI de los `RequiredTests` sobre la copia, o la declaración de que no se corrieron (los cubre `ui-tests` de la CI del RED); y cero `warning CS` | §11 |
| V43-05 | La versión | El `Objective` de las tres subtareas cita la v4.3; el nombre del archivo lleva un solo punto decimal, como pide `bounded_check` | §6 |

