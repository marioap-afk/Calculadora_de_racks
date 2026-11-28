# I-64 F4B PB-T3c — Comprobación acotada (CA-T3c) del contrato en seco y de la v4.7

**Veredicto: EMITIBLE.** B/I/M = **0 / 0 / 4**

**Qué se comprueba:** el contrato en seco de `F4B-PBT3C-FULL-WRITE-ERASE` (`contract-dry-pbt3c.json`, sha256 `2A1FF346…`, 53 549 bytes), generado sobre el cierre de PB-T3b (`fe1c67f4`, F4B-E46) con la v4.7 (`976361c0…`) y el generador `make_pbt3c_contract.py` (`46E77EDD…`). Se mira lo que la v4.7 cambia y todo lo que toca a PB-T3c. Es la condición de F4B-E35, pregunta 3: 0 bloqueantes, 0 importantes sin resolver y una remedición que no cambie nada material.

**En breve:**
- **El seco es fiel a la v4.7.** Cuadran el alcance (4 rutas, WR como `UNIT_CHANGE`), el prohibido (181), los 12 invariantes, las 33 paradas, las 15 evidencias y las 50 autoridades. Lo común es, byte a byte, lo del contrato reemitido de PB-T3b con el prefijo cambiado. La primera evidencia está en `F4B_EXACT_EVIDENCES` del sustituto de nc2 v2, que admite `--task PBT3C`.
- **La lección de F4B-E46 está bien aplicada.** El RED y el GREEN previstos tienen una sola rama y cuadran con la TRX del GREEN de PB-T3b más cinco `[Fact]`. Ningún recuento esperado depende de cómo cuente la TRX.
- **La compilación de comprobación cumple la precondición 13.** La suite completa de Core da 19 124 = 19 118 + 0 + 6. No hace falta un control negativo. El literal de `COMPILE_RESULT` vale tal cual.
- **El hueco propio de PB-T3c no es la reflexión, sino las guardas de texto que leen todo `src/` o todo el Plugin.** Ningún proyecto de pruebas referencia el Plugin, así que la fachada y el constructor no se pueden leer por reflexión ni por un escaneo de ensamblado. Leí las guardas que enumeran `src/` o el Plugin, y ninguna prohíbe fuera de archivos con nombre lo que el GREEN pone en la fachada.
- **La remedición no cambia nada material.** WR no ha cambiado desde `833ea6bf`, ni el Plugin desde `eab27a9e`, y cada hecho del cierre está en su fuente.
- **El registro declarado de PB-T3b (duda 17):** cada dato de las cinco sustituciones tiene su fuente en `fe1c67f4`, y cada línea vieja aparece una sola vez.
- **Cuatro menores:**
  - **M-01:** decido la duda 1 por la lectura que reañade, con `ExpectRed` false, los dos filtros de PB-T3b que tenían `ExpectRed`;
  - **M-02:** la nota de V47-06 da como hecho una afirmación del Worker que la revisión no verificó;
  - **M-03:** el barrido de sobrecargas cuenta mal los parámetros del constructor;
  - **M-04:** a `compile-check.md` le falta nombrar el hueco que de verdad queda.
- **No hay decisiones nuevas para el Owner.**

**`BOUNDED_RESULT` propuesto (ASCII):**

`EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; remedicion sobre fe1c67f4 sin cambios materiales; precondicion 13 cumplida: Core 19124 = 19118 + 0 + 6`

## §1. Método y límites

**Qué se leyó.** El código, por objetos git de `fe1c67f4` desde el checkout principal. Lo demás, en `hs65`:
- **El paquete:** la v4.7 y su registro de cambios, `v47-delta.md` y `check-v47-out.txt`. De la v4.7: §4.1, §5.0, §5.4, §6.0 (la tabla de RR-3), §6.3, D-PBT3-11 y las precondiciones 1, 9, 12 y 13.
- **El seco y sus pruebas:** `dry-pbt3c.txt`, `contract-dry-pbt3c.json`, `neg-pbt3c.txt` (77 de 77) y `generator-doubts-pbt3c.md`.
- **La remedición:** `sweep-pbt3c-close.txt`, `overload-sweep-pbt3c-fe1c67f4.txt`, `name-check-pbt3c-fe1c67f4.txt` y R-23 de `pbt3-measurements.md`.
- **El esqueleto:** `pbt3c-skeleton.patch`.
- **Los borradores:** `f4b-e47.md`, `superseding-pbt3c.md` y `superseding-pbt3b-close-fill.md`.
- **La compilación de la sesión,** en `pbt3ccc/compile/`: `compile-check.md`, `out-run1.txt`, `trx-outcomes.txt` y los registros de `out/`.
- **En `fe1c67f4`:**
  - el contrato reemitido de PB-T3b;
  - `count-rows-out.txt`;
  - F4B-E46 en `f4b-dispositions.md`;
  - la entrega `R20261120T075251Z-a9e0`: `worker-handoff.json`, `direct-review.md` e `independent-review.md`;
  - `task-close-package.md`;
  - `docs/ideas-futuras.md`;
  - `superseding-characterization.md`;
  - WR, `RackCommandSupport.cs:214-276` (`EraseViewBlocks`) y las guardas que cito.

**Los guiones de esta revisión** (en `hs65/pbt3/`, todos con `python -I`, cada uno con su salida `cat3c_*_out.txt`):
- **`cat3c_dry.py`:** el seco, campo a campo, frente al contrato reemitido de PB-T3b. Compara lo común con el prefijo cambiado.
- **`cat3c_scope.py`:** el prohibido de PB-T3c y el de PB-T3b frente al árbol de `fe1c67f4`. Comprueba que cada entrada existe, que ninguna cubre una ruta permitida y qué queda sin cubrir.
- **`cat3c_enum.py`:** las guardas que leen todos los `.cs` de `src/` o del Plugin, frente al cuerpo previsto de la fachada. El cuerpo es el de `EraseViewBlocks` para una definición, como manda D-PBT3-11.
- **`cat3c_evid.py`:** la primera evidencia dentro de `F4B_EXACT_EVIDENCES` del sustituto de nc2 v2, leído con `ast`; las evidencias [3], [8] y [14]; y C11.
- **`cat3c_c11.py`:** el diff de PBT3C-C11 frente a PBT3B-C11.
- **`cat3c_auth.py`:** cada autoridad, si existe ya en `fe1c67f4` o la trae la emisión.
- **`cat3c_bounded.py`:** el `bounded_check` del generador, con `reviewer='CA-T3c'`.

Además usé `git grep`, `git diff` y `git merge-base` sobre `fe1c67f4`, y `cat3a_ctx.py`, de CA-T3a, para leer líneas por objetos git.

**Una incidencia, declarada.** Un comando de Bash escribió por error una copia de la función `bounded_check` del generador (5 840 bytes) en el directorio temporal del sistema, fuera de `hs65/pbt3/`. La borré enseguida. No contenía nada del usuario ni del worktree, pero se aparta de la regla «escribe solo en `hs65\pbt3\`».

**Lo que no se hizo:**
- no ejecuté dotnet;
- no leí nada del worktree;
- no reproduje las pruebas en negativo ni la autoprueba del emisor: me fío de sus salidas.

**Pendiente:** el delta de `issue_pbt3c.py` con las cinco sustituciones del registro (§9). La sesión dirá cuándo está.

## §2. Fidelidad del contrato en seco frente a la v4.7 (punto 1)

| Campo | Lo que hay en el seco | Comprobación |
|---|---|---|
| Claves | Las 19 del contrato de PB-T3b, en el mismo orden | `cat3c_dry.py` |
| `Objective` | El de §6.3, en ASCII, con «v4.7» | Igual al de la v4.7 (`check-v47-out.txt`: solo cambia «v4.6» → «v4.7») |
| `AllowedWriteScope` (4) | WR (`src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`), la fachada y las dos clases de prueba | Las rutas del §6.3. WR es la autoridad [44], `UNIT_CHANGE` (F4B-E25) |
| `ForbiddenWriteScope` (181) | `src/` 56: Application, Domain y UI enteras, y el Plugin con 53 entradas (las 13 hermanas de WR en `Views/`). `tests/` 108: Core 39 (con `Workspace/` entera y los cinco archivos de PB-T3b) y UI 69. Raíz 17 | Ninguna entrada falta en el árbol y ninguna cubre una ruta permitida (`cat3c_scope.py`). Quedan sin cubrir 659 archivos, los mismos que en el contrato de PB-T3b (es el algoritmo de [PKC-C]; los protege C01). Respecto de PB-T3b se pliegan Application, porque PB-T3c no la toca (INV-PBT3C-01), y `Workspace/`, porque los tres archivos con los `Skip` de D-1 ya no se escriben |
| `Invariants` (12) | Propios 01 a 04; comunes 07 a 14 | Los comunes son iguales a los de PB-T3b con el prefijo cambiado. INV-PBT3C-03 se mide en WR: tres clases `RackPilot*`, `StartTransaction` (:155), `Commit` (:305), una purga (:174) y dos `[Conditional("DEBUG")]` (:328 y :335). El hueco 05-06 es correcto (duda 13, §10) |
| `StopConditions` (33) | 17 estándar y 16 propias | 15 comunes iguales. C04, C13, C16 y C17 son las del §6.3. **C11:** solo cambia «no one-off acceptance extends to PB-T3b» → «PB-T3c» (`cat3c_c11.py`). **C02:** el texto común, literal (duda 15, §10) |
| `RequiredTests` (34) | Los 2 nuevos, las guardas del escritor (86 y 123), D2, la paridad de UI de PB-T3a y los de §6.2 con `ExpectRed` false | Los mínimos cuadran con el seco y con la TRX (§3). **Faltan `PushBackFullWrite` y `PushBackPilotUpdateEquivalenceGuardTests`, por la lectura de la duda 1:** M-01 |
| `ExpectedEvidence` (15) | [0]-[2] y [4]-[7], las de PB-T3b; [3] y [8], adaptadas; [9]-[13], las del §6.3; [14], la línea de la CI | **[0]** está en `F4B_EXACT_EVIDENCES` (2 entradas) del sustituto de nc2 v2 en `fe1c67f4`, y la lista de tareas lleva `PBT3C` (`cat3c_evid.py`). **[3]** dice «las autopruebas de CT-PBT3C-01, 04 y 05», las tres que tienen autopruebas en el §5.4. **[8]** lleva «En PB-T3c, además: `RedSha..RedRepairSha` no toca WR ni la fachada». **[2]** es la de PB-T3b, y vale para PB-T3c (duda 2, §10) |
| `Authorities` (50) | 47 existen en `fe1c67f4`. Las tres que faltan las trae la emisión: `pbt3c-anchors-out.txt`, este informe y el propio contrato | `cat3c_auth.py`. El seco dice «F4B-E01 a F4B-E46» porque lo lee de la revisión (`DISP_LAST`). Sobre X dirá E47, como hizo PB-T3b («E01 a E44» con su X `05b6f04e`) |
| Anclas (28) | `anchors-ok 28` | Comprobé por objetos git las del cierre: `fe1c67f4^` = `c7582785`; `git diff c7582785 fe1c67f4 -- src tests` vacío; el RED `9fda277e` es ancestro del GREEN; el Plugin no cambia desde `eab27a9e`, ni WR desde `833ea6bf` |
| Marcadores | `<resultado por rellenar>` (en `IssuedBy`, el de `BOUNDED_RESULT`) y `<campo>` | `<campo>` es texto literal de [8] («una cadena `<campo>=`»), no un marcador |

## §3. La lección de F4B-E46 (punto 2)

**La línea base es la TRX del GREEN de PB-T3b** (`count-rows-out.txt`): 19 124 = 19 118 `Passed` + 6 `NotExecuted`. Cuadra:
- la línea base de PB-T3a eran 19 107;
- PB-T3b añadió 21: 9 de `PushBackFullWritePrepareTests`, 4 de `RunTests`, 6 de la equivalencia y 2 de `WritePathGuard`;
- y la `Theory` S-2 con `Skip` quita 4 filas en `WorkspacePilotPrepareGuardTests` (29 → 22 + 3).

**Lo previsto en PB-T3c:**
- **RED:** 19 129 = 19 119 + 4 `Failed` + 6.
- **GREEN:** 19 129 = 19 123 + 0 + 6.
- **UI, en los dos:** 2 451 = 2 430 + 21.

**Por qué cada parte se sostiene:**
- **Un `[Fact]` por CT (duda 6)** es lo que da una sola fila por CT. Es aplicar F4B-E46, no cambiar ninguna CT: ninguna CT pide una `Theory`, y las autopruebas van dentro del `[Fact]`, con un registro y una aserción al final, como los E-n y CT-13 de PB-T3b.
- **CT-05 pasa en el RED (duda 7).** Sus reglas (`Rule1`, `Rule1b` y `Rule1c` de `PushBackSharedReferenceGuardTests.cs:57-69`) solo casan a partir de una de las ocho fichas, y WR no tiene ninguna en `fe1c67f4` (0 coincidencias). El esqueleto tampoco.
- **CT-01 a 04 fallan por aserción, una fila cada una.** `Src.BodyOf` (`WorkspaceHostSourceSupport.cs:67-80`) falla con `Assert.True` cuando no encuentra el miembro, así que un `EraseUnit` que no existe da una aserción y no una excepción (C04).
- **Ningún `Skip` de D-1 cambia,** y PB-T3c no toca ni la UI ni sus pruebas.
- **La precondición 13 (duda 8)** lo confirma sobre la copia: 19 124 = 19 118 + 0 + 6, con las seis omitidas, de D-1 y con su motivo literal.

**Ningún recuento esperado depende de cómo cuente la TRX sin llevar su total exacto:**
- **[14]** da los tres totales de su única rama.
- **[11]** (el censo de declaraciones, «la diferencia son las cinco de C13») cuenta declaraciones, no filas.
- **Los `MinSelected` son mínimos,** no totales esperados:
  - **86 y 123** son los `Passed` de la TRX y lo estático, sin `MemberData`. Con las omitidas, la TRX selecciona 87 y 125. Las dos lecturas cumplen.
  - **`Workspace` de Core, 3 261**, es la cuenta estática de `count_tests_git.py`: 3 271 − 10, sin las filas de las 33 `Theory` con `MemberData`. La revisión independiente de PB-T3b midió en el GREEN 3 478 seleccionados, 3 472 `Passed` y 6 omitidos. El mínimo se cumple con cualquier forma de contar.

  Esto corrige lo que dije en la §13.9 de CA-T3b sobre la nota del §6.3. «3 271 − 10» no es un error, es el criterio estático, y la nota lo dice.

## §4. RR en PB-T3c (punto 3)

**La regla, con la lectura de PB-T3c, está bien:**
- INV-PBT3C-02: `RedSha..RedRepairSha` solo toca las dos clases de prueba, y `RedRepairSha..CurrentSha` es el GREEN;
- [8] lleva la frase propia de PB-T3c;
- la lista de «Por subtarea, además» del §6.0 dice «en PB-T3c, WR ni la fachada»;
- C18 es la común.

**Las dos filas de PB-T3c de la tabla de RR-3 (duda 3):**
- **`PushBackViewEraseGuardTests` (CT-01 a 04)** no cambia desde la v4.6. Lleva 16 líneas, todas literales del §5.4: la firma, los ocho términos prohibidos, el patrón `NamesThePilot`, las cuatro sentencias de CT-02 y la llamada de CT-04.
- **`PushBackWritePathPluginGuardTests` (CT-05)** lleva las ocho fichas, iguales a `PushBackSharedReferenceGuardTests.cs:35-36` en `fe1c67f4`, y los tres nombres de reglas de las líneas 57, 60 y 68. Lo comprobé.

**Decido la duda 3 como la lee el redactor.** Las fichas y los nombres de las reglas son lo que CT-05 copia y lo que la ayuda mecánica puede contar como subcadena. Las expresiones de dentro de las reglas (`Link`, `Assign`, `Mutators`, `Call`, `Coll`, `CastC` y `Named`, de `:44-50`) no se pueden enumerar como un literal de RR-3 sin escapar `|` en cada celda. Dejarlas fuera tampoco las desprotege: RR-3, como regla, prohíbe cambiar el oráculo («las reglas de CT-PBT1B-14»). Y CT-05 no tiene `ExpectRed`, así que una reparación sobre ella es improbable.

**Dos observaciones, sin hallazgo:**
- **«`PushBack`», «`Purge`» y «`Regen`» se cuentan como subcadena en la clase,** y el nombre de la clase ya contiene `PushBack`. Una reparación que mueva ese texto puede dar C07. Es un falso positivo hacia el lado seguro.
- **Que un literal no aparezca en `RedSha`** (por ejemplo, si el Worker escribe la firma como expresión regular) da 0 = 0. No es una parada, y es la forma en que se diseñó la ayuda.

## §5. El esqueleto, los barridos y la compilación de comprobación (punto 4)

**El esqueleto (duda 9).** `pbt3c-skeleton.patch` (643 bytes, `a81de435…`) crea `src/RackCad.Plugin/Views/RackViewBlockEraser.cs` con:
- `internal static class RackViewBlockEraser`;
- la firma exacta de D-PBT3-11, `internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)`;
- solo `throw new NotSupportedException();`;
- el espacio de nombres `RackCad.Plugin.Views`, el de su carpeta (`NamespaceFolderGuardTests`).

No lleva la cadena `PushBack` ni ningún comentario. Confirmo que vaya sin comentario, pero con una precisión: el censo de CT-PBT1B-15 cuenta `PushBack` **después** de `Clean`, que quita los comentarios y vacía los literales (`PushBackSharedReferenceGuardTests.cs:313-314`). La razón buena es otra: INV-PBT3C-04 y CT-01 prohíben la cadena en todo el archivo, y otras guardas leen el texto crudo, como `I55G15Auth15ConsumerTests` o el recuento de `[CommandMethod(` de `SelectiveEditorOpenTests:566-571`.

**La reflexión y los escaneos de ensamblado no alcanzan a PB-T3c.** `tests/RackCad.Tests.csproj` referencia Domain, Application y dos herramientas, y `RackCad.UI.Tests.csproj`, Domain, Application y UI. Ninguno referencia el Plugin. Así que ninguna prueba puede leer por reflexión ni por escaneo de ensamblado la fachada ni `RackPilotMutationScope`: el hueco de la lección de PB-T3b no existe aquí.

**El hueco propio de PB-T3c son las guardas de texto que leen todo `src/` o todo el Plugin.** Una de ellas ve la fachada sin nombrarla, y el esqueleto solo prueba la firma, no el cuerpo del GREEN: `GetObject`, `OpenMode.ForWrite`, `GetBlockReferenceIds`, `Erase()`, `UpgradeOpen()` e `InvalidOperationException`.
- **El barrido (`cat3c_enum.py`):** 43 archivos de prueba enumeran archivos, y unos veinte leen todo `src/` o todo el Plugin.
- **Las leí una a una.** Ninguna prohíbe esas operaciones fuera de archivos con nombre:
  - censos de `[CommandMethod(`: `CustomPropertiesCommandGuardTests:182`, `SelectiveEditorOpenTests:571` (40), `PushBackRoundTripSourceGuardTests:70` y `CustomPropertiesHelpCensusTests`;
  - consumidores de un nombre concreto: `CantileverLineResolver`, `Insunits`, `RackDefinitionCreator.CreateInTransaction(` y el puente del Workspace;
  - los nombres del modelo completo: CT-PBT3A-12 y CT-PBT3B-13, cuyo `FullWriteNames` no casa con `IPushBackPilotViewEraser`;
  - `Autodesk.` fuera del Plugin;
  - el espacio de nombres por carpeta.
- **Las prohibiciones de esos términos van sobre archivos con nombre:**
  - `UpdateGuard:182-212` y los recuentos de G-9 (`IntegrationGuard:600-611`), sobre WR;
  - `I55G15`, sobre `RackProjectionWriteScope.cs`;
  - `CustomPropertiesEdgeGuardTests:480`, sobre los cuerpos de Application.
- **WR en el GREEN:** `EraseUnit(` y `.EraseInTransaction(` no casan con `\bErase\s*\(` ni con `\.Erase\s*\(`. La clase `RackPilotMutationScope : IPushBackPilotMutationScope, IPushBackPilotViewEraser` sigue casando con `UpdateGuard:264` (`…:\s*IPushBackPilotMutationScope\b`).

**T3C-1 (dudas 10 y 11) y la precondición 13 cubren el resto:**
- **El constructor:** tiene un solo uso (WR:156), en su archivo, y ninguna guarda lee su texto.
- **Los nombres nuevos:** libres en sus archivos.
- **Los literales que coinciden:** genéricos y de otro alcance.

**La compilación de comprobación (precondición 13).** La pidió la sesión, y cumple la v4.7 tal como está escrita:
- **Lo que pide la precondición 13:** «Core, UI y el Plugin (con `-o`), sin errores, y la suite completa de Core ejecutada sobre esa copia, en verde».
- **Lo que se hizo:**
  - la copia sale de `git archive fe1c67f4`;
  - el esqueleto aplica limpio y el archivo no existía;
  - Core-tests y UI-tests (Debug) y el Plugin (Release, `-o`) dan rc 0, «Compilación correcta.» en los tres registros y ninguna advertencia que nombre el esqueleto;
  - en la suite de Core, el registro de VSTest dice «Con error: 0, Superado: 19118, Omitido: 6, Total: 19124», y la TRX lleva 19 124 `UnitTestResult`: 19 118 `Passed` y 6 `NotExecuted`, S-1 a S-6, con su motivo literal.
- **`compile-check.md` cuadra con los registros.** Una observación sin efecto: en `out-run1.txt`, la línea de `counters` de la TRX dice `notExecuted 0` (así resume VSTest las omitidas). El «+ 6 omitidas» del informe sale de las filas y del registro, que es lo correcto.
- **El control negativo no hace falta:**
  - la precondición no lo pide;
  - no hay ningún choque conocido que reproducir;
  - la copia la leen de verdad las guardas de texto: `RepoRoot()` sube desde la carpeta de salida hasta el `RackCad.sln` de la copia (`WorkspaceHostSourceSupport.cs:20-26`);
  - el control positivo prueba que el Plugin compiló el archivo nuevo.

  Si se quiere, uno barato lo demostraría: con `PushBack` inyectado en la fachada de la copia, CT-PBT1B-15 daría 15 archivos y no 14. Es opcional.
- **El literal de `COMPILE_RESULT`** de la sesión es correcto tal cual.
- **Lo que no dice `compile-check.md`:** el hueco de las guardas de texto ante el cuerpo del GREEN. Es la M-04.

## §6. La remedición (punto 5)

**R-23 no cambia nada material.** Cada hecho está en su fuente de `fe1c67f4`:
- la base, el GREEN, la custodia y las CI: F4B-E46 y `count-rows-out.txt`;
- 86 y 123: `count-rows-out.txt`, y lo estático, en el seco;
- WR, igual desde la emisión de PB-T3b y desde R-18: `git diff` vacío;
- el barrido de las guardas: de 92 a 94 clases, con 9 coincidencias nuevas de `PushBackWritePathGuardTests`, todas expresiones genéricas que no prohíben nada de PB-T3c (§5);
- la fila de CT-05.

**Los marcadores rellenos (duda 5).** La v4.6 tenía ocho apariciones: seis sitios por rellenar y dos menciones. Los cinco grupos del encargo cuentan como uno la fila y la nota de los `MinSelected` del §6.3. `check_v47.py` comprueba los seis, cada uno con su fuente, y en la v4.7 quedan tres menciones y ningún sitio. Confirmo la lectura del redactor.

**Lo único que cambia frente a la v4.6** es lo previsto de la CI. La v4.6 decía «19 133, o 19 129 si…», y la medición en el cierre elige la rama de 19 129. Es la rama que ya se preveía, no un cambio material.

## §7. V47-06 (punto 6)

**PBT3B-IR-M02 y M07 están bien llevados:**
- M02: la cultura del proceso, y fijarla con `InvariantCulture`, como CT-PBT1B-12;
- M07: `IsDirty` cierto justo antes de la operación.

**PBT3B-IR-M01 y OWNER-2, a medias (M-02).** La nota dice «`pb-20-ab` no da un lateral que exista solo por un hueco B». Pero eso es la afirmación del Worker que la revisión independiente dice expresamente no haber verificado (`independent-review.md:130`: «no verifiqué la afirmación del Worker»). Y faltan dos cosas:
- **el segundo hecho de OWNER-2:** el lector rechaza los sobres con nombres distintos (`DivergentDiagnostic`), y por eso CT-PBT3B-05 fabrica la relectura;
- **la lección de método de M01:** las recetas que se apartan del paquete se declaran como desviación, no solo en `UnexpectedFindings`.

**No afecta a PB-T3c.** La nota no va al contrato, y sus CT son guardas de texto sin recetas con borradores. La conclusión de la nota es correcta.

## §8. Las CT de PB-T3c, el momento de cada lectura y los gestos (punto 7)

**Las reglas del §5.0 no se aplican a PB-T3c.** El momento de cada lectura y los gestos que no cambian nada son reglas de recetas con borradores y ventanas. Las CT de PB-T3c leen texto fijo del árbol, sin gestos.

**El riesgo análogo está en las autopruebas.** Una inyección inerte haría pasar una autoprueba en vacío, pero las del §5.4 están del lado seguro:
- las inyecciones que añaden texto (una segunda llamada en WR, una en RPC o en un archivo nuevo, o un término prohibido en la fachada) siempre cambian el texto;
- una sustitución que no encuentra su ancla deja el texto real, que no tiene violaciones, así que la autoprueba que exige `NotEmpty` falla. No pasa en vacío.

**Con los diseños de PB-T3b que llegan al borrado, las CT fijan lo que hace falta:**
- **`IPushBackPilotViewEraser`** se llama entre el inicio de MUTATE y su sello, una vez por clave, y la corrida rechaza un plan con borrados si el ámbito no lo implementa. CT-02 fija que `RackPilotMutationScope` lo implementa, y el orden de `EraseUnit`: `EnsureOwned`, la búsqueda que lanza, el contador compartido con `WriteUnit` y la inyección de Debug. Así la escritura real lo admite, y la excepción de la fachada (`IsLayout` o nula) cae en el escape de la corrida, con todo o nada.
- **La relectura de MUTATE se hace antes de escribir** (WR:250-258: `matches` compara la definición con el `ObjectId` preparado). Con `_erasures` dentro, CT-03 asegura que una vista a borrar sigue siendo la misma que se resolvió en PREPARE.
- **POST no cambia:** la purga es la unión de `_staleDefinitions`, y la fachada devuelve las anidadas para ella.
- **El constructor** gana un parámetro, que no es una declaración nueva (C13), y tiene un solo uso.

## §9. El registro declarado de PB-T3b (duda 17)

**Revisé `superseding-pbt3b-close-fill.md` frente a `fe1c67f4`:**
- **Cada línea vieja aparece exactamente una vez** en `superseding-characterization.md` (`grep -c -x -F`), en la sección «## PB-T3b», y la tabla es la única con «(al cerrar)».
- **El estado:** los SHA completos del RED y del GREEN existen, y el RED es ancestro del GREEN. «Emitida en F4B-E44 y reemitida en F4B-E45» es la historia de F4B-E46.
- **El RED:**
  - la CI `44704503614`, con «tests en failure; ui-tests y build-ui en success; build-plugin omitido», sale de F4B-E46;
  - los 13 y los 6 fallidos, y los seis de D-1 en `NotExecuted`, salen de `count-rows-out.txt`;
  - «por aserción» lo dicen `task-close-package.md:42`, `direct-review.md:19` y `worker-handoff.json:1250`. La revisión independiente (`:132`) solo dice que la CI trunca los mensajes, no que lo contradiga.
- **El GREEN:** 4/4, 19 124 = 19 118 + 0 + 6 y UI 2 451 = 2 430 + 21 salen de F4B-E46 y de `count-rows-out.txt`.
- **La medición:** sale de `independent-review.md:99` de la entrega `a9e0`, y `tests/m1/m1.trx` está en su manifiesto (`custody-manifest.json:1370`). Lo que dice coincide con lo esperado de la sección: S-1, S-3, S-4 y S-5 fallan; S-2 falla en las filas 1 y 3; S-6 pasa en vacío. Que los `Skip` se restauraron lo confirman el diff `-U0` de las tres guardas y `git diff 9fda277e c7582785 -- tests` vacío.
- **El estado «EJECUTADA»** es el de los cierres de PB-TC y PB-TC2.
- **No cambia nada más.**

No es una decisión: el registro es de la sesión («Lo escribe la sesión principal»), y F4B-E47 lo declara. Cuando llegue el delta de `issue_pbt3c.py`, comprobaré que admite exactamente estas cinco sustituciones y nada más.

## §10. Las dudas del generador, una a una

| Duda | Lectura | Resultado |
|---|---|---|
| 1 | «Y los de §6.2 sin `ExpectRed`» | **Decido: entran todos los de §6.2, con `ExpectRed` false**, también `PushBackFullWrite` (13) y `PushBackPilotUpdateEquivalenceGuardTests` (6). Es el precedente del §6.2, que reañade con false el filtro de Core de PB-T3a que tenía `ExpectRed` (`PushBackFullDraft`), y es la M-03 de CA-T3b, que la v4.7 corrige para la UI de paridad. Con la lectura literal, las 19 pruebas de PB-T3b salen de la red explícita de todo contrato posterior. No se pierde ninguna red, porque la CI corre la suite entera con el total exacto en [14] y PB-T3c no toca Application, así que es menor (M-01) |
| 2 | Lo de §6.1 en la evidencia | **Correcto.** [2] vale tal cual para PB-T3c; [3] y [8] están adaptadas; [0] es la de PB-T3a y está en la lista cerrada del sustituto |
| 3 | La fila de CT-05 de RR-3 | **Correcto** (§4) |
| 4 | 86 y 123 | **Correcto:** los `Passed` de la TRX, iguales a lo estático (§3) |
| 5 | Cinco grupos, seis sitios | **Confirmada** (§6) |
| 6 | Un `[Fact]` por CT | **Aplicación de F4B-E46, no un cambio de diseño** (§3) |
| 7 | El RED previsto | **Correcto** (§3) |
| 8 | La precondición 13 | **Confirmada** por la compilación de la sesión (§5). La regla de redacción `<copia>` cubre la carpeta que usó la sesión, que se ve en `out/core-test.log` |
| 9 | El esqueleto sin comentario | **Confirmada, con la precisión del §5** |
| 10 | `Transaction`, `Erase` y `lock` | **Confirmada.** Lo amplía mi lectura de las guardas que leen todo `src/` o el Plugin (§5) |
| 11 | Las etiquetas de los barridos | **Confirmada.** Aparte, el barrido cuenta mal los parámetros del constructor (M-03) |
| 12 y 20 | Volver a correr antes de X | **De acuerdo.** Hay que hacerlo también tras aplicar M-01 |
| 13 | El hueco 05-06 | **Decido: correcto.** Los comunes tienen número fijo en las tres subtareas (§6.0), y renumerar los propios rompería la referencia común. No hace falta nota en el contrato |
| 14 | El nombre de este informe | **Confirmada** |
| 15 | C02 nombra PB-T3b | **Decido: se queda literal.** Dice que las dos salvedades son de PB-T3b, y eso le dice al Worker de PB-T3c que no tiene ninguna. Es lo correcto, y reescribir lo común rompería la comparación byte a byte |
| 16 | La comunicación al Owner | **De acuerdo:** es de la sesión |
| 17 | El registro de PB-T3b | **Revisado** (§9) |
| 18 | La línea en blanco | **Cosmético. De acuerdo** |
| 19 | La fuente de la precondición 12 | **Correcta:** 19 107 de 19 107, de `compile-check.md` y `out-run3.txt` de la custodia de PB-T3b |
| 21 | `CUSTODY_EXTRA` 16 | **De acuerdo.** El parche sin su guion no se podría rehacer |

## §11. Decisiones del Owner (punto 8)

**Ninguna nueva:**
- las dudas que decido son de redacción del contrato y caen dentro de la comprobación acotada;
- completar el registro de PB-T3b es de la sesión;
- la remedición no cambia nada material;
- no hay ningún control con una firma no prevista ni ningún intento 3.

D-2 sigue siendo un registro declarado sin `Skip`, no una decisión (§8.2). No lo leo como un debilitamiento: la guarda `UpdateGuard:182-212` sigue en verde por su letra, porque el escritor no borra por sí mismo (§5).

## §12. Hallazgos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-01 | M | v4.7 §6.3, `RequiredTests`; el generador | La duda 1, decidida por la lectura que reañade los filtros de PB-T3b: entran `PushBackFullWrite` y `PushBackPilotUpdateEquivalenceGuardTests` con `ExpectRed` false y sus mínimos, 13 y 6, que son los del contrato de PB-T3b y los `Passed` de la TRX del GREEN (9 + 4 y 6). Así lo hizo el §6.2 con `PushBackFullDraft`, y así lo pidió CA-T3b M-03 para la UI de paridad. No se pierde ninguna red (CI completa y [14]; INV-PBT3C-01) | Antes de X: una fila explícita en el §6.3 («Core \| `PushBackFullWrite`, `PushBackPilotUpdateEquivalenceGuardTests` (PB-T3b) \| 13, 6 \| false»), el generador con los dos filtros (34 → 36) y volver a correr el seco, las negativas, `measure_pbt3c.py` y `check_v47.py` |
| M-02 | M | v4.7 §5.4, la nota de V47-06 | Da como hecho que `pb-20-ab` no tiene un lateral que exista solo por B, cuando la revisión independiente de PB-T3b dice no haberlo verificado (`:130`). Omite el rechazo de nombres distintos (`DivergentDiagnostic`, OWNER-2) y la lección de declarar las recetas desviadas como desviación | Redactar la nota con la atribución («según el Worker, sin verificar») y los dos hechos de OWNER-2. No va al contrato |
| M-03 | M | `overload-sweep-pbt3c-fe1c67f4.txt` (`overload_sweep_t3c.py`) | Dice «line 239, 5 parameters» para el constructor de `RackPilotMutationScope`, que tiene 4. Cuenta la coma del tipo de tupla `(PreparedViewRedraw Prepared, DimensionViewKind Kind)`. La conclusión no cambia: no se añade ninguna sobrecarga, el uso es uno y está en su archivo, y ninguna prueba ve el Plugin por reflexión. Pero el mismo corte contaría de más en `Dictionary<string, int>` y en cualquier comparación de aridad | Contar las comas solo a profundidad 0 de `()` y `<>`, o anotar el error junto a la salida custodiada |
| M-04 | M | `pbt3ccc/compile/compile-check.md`, «Lo que no cubre» | No nombra lo que de verdad queda sin cubrir: el cuerpo del GREEN de la fachada (`GetBlockReferenceIds`, `Erase()`, `UpgradeOpen()`, `ForWrite`) frente a las guardas que leen todo `src/` o el Plugin. El esqueleto solo prueba la firma | Añadir una viñeta: lo cubren el barrido de las guardas (`sweep-pbt3c-close.txt`, con el corpus de D-PBT3-11) y la §5 de CA-T3c; la reflexión y los escaneos de ensamblado no aplican, porque ninguna prueba referencia el Plugin |

**Observaciones (no son hallazgos):**
- la razón de la duda 9 (§5);
- los falsos positivos hacia el lado seguro de la fila de CT-01 a 04 en RR-3 (§4);
- el `notExecuted 0` del resumen de `counters` (§5);
- el control negativo opcional (§5);
- «F4B-E01 a F4B-E46», que sobre X será E47 (§2).

## §13. El `BOUNDED_RESULT`, comprobado

**Cómo se comprobó.** `cat3c_bounded.py` aplica la `bounded_check` de `make_pbt3c_contract.py` a este informe, al literal y a las líneas de la v4.7. Es la misma función de PB-T3b y PB-T3a, línea a línea, y la llamo con `reviewer='CA-T3c'`, como en su llamada (`:1338`).

**Lo que da:** 0 errores, con el literal propuesto arriba.
- **El veredicto:** está en la forma de PB-T3, es de la familia EMITIBLE y no tiene importantes, así que no hace falta ninguna fila de registro.
- **El literal:** es ASCII, empieza por «EMITIBLE;» y lleva «B 0 / I 0 / M 4».
- **Este informe** no tiene recomprobaciones ni rutas locales.

## §14. Recomprobación sobre la v4.7

**Veredicto: EMITIBLE.** B/I/M = **0 / 0 / 1**

**Qué se recomprueba:** los tres deltas que salieron de las §1 a §13 y del registro de PB-T3b, sobre la misma v4.7 (`976361c0…`, sin cambios):
- el emisor, `issue_pbt3c.py` (`88e8a2a7…`);
- el generador (`8D1804CF…`), con el seco nuevo (`682F9986…`) y el barrido de sobrecargas corregido;
- `compile-check.md`, con el control negativo opcional.

**En breve:**
- **Los tres deltas son fieles a M-01, M-03 y M-04 y no cambian nada más.**
  - El generador solo cambia en el bloque de las pruebas exigidas.
  - Del seco solo cambian las pruebas exigidas: entran las dos del M-01 y no sale ninguna.
  - El emisor gana las cinco sustituciones, `COMPILE_RESULT` y `REQ_N` 36.
- **El seco `682F9986…` es fiel a la v4.7 con la lectura de M-01.** Las 31 pruebas exigidas del contrato reemitido de PB-T3b están todas, con su mínimo, `ExpectRed` false y su orden.
- **El registro de PB-T3b:** lo reproduje sin el emisor y da el mismo archivo, de 32 382 bytes, con exactamente las cinco líneas cambiadas, la 274 y de la 306 a la 309.
- **El control negativo es correcto:** falla solo por `archivos=15` y pasa al restaurar, con el mismo sha256.
- **Una menor nueva (M-05):** el corte corregido aún toma por genérica una comparación sin espacios, como `(a<b, c>d)`. No cambia nada en PB-T3c.
- **De acuerdo con llevar M-02 a `docs/ideas-futuras.md` en el cierre.**
- **Antes de X quedan** el `BOUNDED_RESULT`, las salidas que lo nombran, el encargo y volver a correr la custodia con los guiones nuevos de esta ronda (§14.6).

**`BOUNDED_RESULT` final (ASCII):**

`EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; remedicion sobre fe1c67f4 sin cambios materiales; precondicion 13 cumplida: Core 19124 = 19118 + 0 + 6; recomprobacion v4.7: EMITIBLE, B 0 / I 0 / M 1`

### 14.1 Método y límites

**Qué se leyó:**
- **Las descripciones de los deltas:** `issue-pbt3c-delta-r2.txt` y `delta-r3-pbt3c.txt`.
- **Las versiones de antes:** `_ref/issue-r1/`, con el emisor r1, el r2 sin `COMPILE_RESULT` y los borradores; y `_ref/r3-before/`, con el generador, su fragmento `c12_req.py`, el seco, las negativas, el barrido y sus salidas.
- **Las versiones de ahora:** `_t3c/c12_req.py`, `generator-doubts-pbt3c.md`, `neg-pbt3c.txt`, `issue_pbt3c-selftest.txt` e `issue_pbt3c-dry-objects.txt`.
- **La compilación:** en `pbt3ccc/compile/`, `compile-check.md`, `compile_check_ctrl.sh`, `out-run-ctrl.txt` y `out-ctrl/`.
- **El registro del cierre,** por objetos git de `fe1c67f4`.

**Los guiones nuevos** (en `hs65/pbt3/`, con `python -I`):
- **`cat3c_r3.py`:** el seco r3 frente al de antes y frente al contrato reemitido de PB-T3b (`cat3c_r3_out.txt`).
- **`cat3c_args.py`:** el corte de argumentos de `overload_sweep_t3c.py` con casos propios (`cat3c_args_out.txt`). Ejecuta solo sus dos funciones, copiadas en memoria de su texto, como `cat3c_bounded.py` con `bounded_check`; no ejecuta el barrido.
- **`cat3c_fill.py`:** el registro declarado que escribiría X, rehecho sin el emisor, con las sustituciones leídas con una expresión propia (`cat3c_fill_out.txt`).
- **`cat3c_args2.py`:** una depuración de `cat3c_args.py`, sin salida guardada.

**Una incidencia mía, declarada.** La primera versión de `cat3c_args.py` copiaba `args_at` solo hasta su primer `return None, i`, el de dentro del bucle, y la copia truncada se quedó en un bucle infinito. La paré con TaskStop. Dos corridas más se cortaron con `timeout`, y `cat3c_args2.py` mostró el corte. La versión buena copia hasta el `return None, i` de nivel de función. No tuvo ningún efecto fuera de mi proceso.

**Lo que no se hizo:**
- no ejecuté dotnet;
- no leí el worktree;
- no ejecuté el emisor, el generador, las negativas ni el barrido: comparé sus textos y sus salidas, y reproduje dos de sus resultados con guiones propios.

### 14.2 El delta del generador y el seco (M-01)

**El generador:**
- **Solo cambia el bloque de las pruebas exigidas.** El diff de `make_pbt3c_contract.py` entre `_ref/r3-before/` y el de ahora tiene 4 tramos, y son, línea a línea, los del fragmento `_t3c/c12_req.py`: `f1f879b9…` → `8d863286…`.
- **El cambio:**
  - el bucle sobre las pruebas exigidas de PB-T3b ya no salta las que tienen `ExpectRed`: las añade con `ExpectRed` false y el mínimo de ese contrato;
  - un filtro con `ExpectRed` que no sea de `M01_FILTERS` (`PushBackFullWrite` 13 y `PushBackPilotUpdateEquivalenceGuardTests` 6), o que tenga otro mínimo, aborta;
  - cada filtro de `M01_FILTERS` tiene que salir una sola vez, con `ExpectRed` false, su mínimo y la clase `t3b`.
- **Es la lectura que decidí en la duda 1.** El comentario cita CA-T3c M-01, y la v4.7 no cambia.
- **Las negativas:** 82 de 82 en `neg-pbt3c.txt`, contra el generador `8d1804cf`. Sobre las 77 de antes hay cinco nuevas, G17 a G21, y los identificadores de las demás no cambian. Las cinco abortan con la aserción de M-01, incluida G21, la lectura literal de antes. El control positivo C01 da el contrato `682f9986`.

**El seco (`cat3c_r3.py`):**
- **Lo que cambia:** de `2A1FF346…` (53 549 bytes) a `682F9986…` (53 959 bytes), ASCII y sin CR. Tiene las mismas claves en el mismo orden, y **solo cambia `RequiredTests`**, de 34 a 36:
  - las dos entradas nuevas son exactamente las de M-01, con `ExpectRed` false y mínimos 13 y 6, en las posiciones 7 y 8, tras la UI de paridad;
  - no sale ninguna, y las 34 de antes siguen en su orden.
- **Frente al contrato reemitido de PB-T3b:**
  - sus 31 pruebas exigidas están todas en PB-T3c, con el mismo mínimo y `ExpectRed` false;
  - una, `PushBackSharedReferenceGuardTests`, va en las filas propias del §6.3;
  - las otras 30, desde la séptima, siguen el orden de PB-T3b;
  - la única con `ExpectRed` en PB-T3c es `PushBackViewEraseGuardTests`.
- **Los mínimos:** lo estático del seco (13 y 6 ejecutables, sin `Skip` ni `MemberData`) coincide con los `Passed` de la TRX del GREEN de PB-T3b, 9 + 4 y 6 (`count-rows-out.txt`).
- **`dry-pbt3c.txt`** solo cambia en la línea DRY (`required 36` y el sha) y en las dos filas nuevas.
- **Lo previsto de la CI no cambia.** Son pruebas que ya cuentan en los 19 129, y no cambia el número de filas ni de fallos.

**Fiel a la v4.7.** La celda «Y los de §6.2 sin `ExpectRed`» se lee «con `ExpectRed` false», y es la única que cambia de lectura. La lectura queda escrita en el generador custodiado (su comentario y `M01_FILTERS`), en `generator-doubts-pbt3c.md` y en este informe, que es una autoridad del contrato ([02]).

Una observación, sin hallazgo: F4B-E47 no la nombra. Una frase en su párrafo de la comprobación acotada («la duda 1 de la celda de §6.2, decidida por CA-T3c: con `ExpectRed` false; 36 pruebas exigidas») ahorraría buscarla, porque la v4.7 custodiada mantiene la celda ambigua.

### 14.3 El barrido de sobrecargas (M-03)

**El arreglo es fiel a M-03:**
- **El diff de `overload_sweep_t3c.py`** (17 493 → 19 622 bytes) solo añade `generic_close`, cambia el corte de `<` en `args_at` (con su docstring) y añade las autopruebas del corte a nivel de módulo.
- **La salida** (`ffbe262f…` → `5da25ac7…`) cambia en una sola línea: «line 239, 5 parameters» pasa a «4 parameters», que es lo que tiene el constructor (WR:239-243).
- **Ninguna conclusión cambia:** el constructor tiene un uso, WR:156; no hay grupos de métodos ni reflexión; y el control de la v4.5 sigue dando :669 y :799.

**Mis casos (`cat3c_args.py`): 10 de 11 cuadran con C#.**
- **Cuadran:** el constructor da 4; también salen bien `Dictionary<string, int[]>` con `Func<int, string>`, un parámetro con valor por defecto, `global::`, `List<int?>` con una tupla anulable, el ternario `x < y ? a : b`, `F<A, B>(x)`, `(n<m, k)`, `new Dictionary<string, int>()` con `null!` y `(plan, null)`.
- **No cuadra:** `(a<b, c>d)` da 1, y en C# son dos comparaciones. C# solo toma `<…>` por genérico si al `>` le sigue uno de `( ) ] } : ; , . ? == != | ^ && || & [`, y aquí le sigue un identificador. Es la M-05.
- **No cambia nada en PB-T3c.** Ninguna llamada a los objetivos de T3C-1 lleva una comparación así, y en la salida ninguna conclusión depende de ello.
- **La salida custodiada de PB-T3b** (`overload-sweep-eab27a9e.txt:54`) tiene el mismo «5». Estoy de acuerdo en no reescribirla: allí el constructor no era objetivo. Conviene anotarlo junto a M-02 en `docs/ideas-futuras.md`.

### 14.4 El delta del emisor (la duda 17 pendiente, `COMPILE_RESULT` y `REQ_N`)

**De r1 a r2, las cinco sustituciones** (10 tramos de diff, los seis puntos que describe `issue-pbt3c-delta-r2.txt` y nada más):
- **`fill_pairs`** lee las cinco del archivo de la sesión, que es la única fuente. Exige cinco «Vieja» y cinco «Nueva:». La vieja tiene que ser la esperada (la del estado, por su comienzo; las otras cuatro, exactas), y cada nueva, distinta, no vacía y sin controles.
- **`pbt3b_fill`** exige cada vieja una sola vez, como línea y como subcadena, y dentro de la sección «## PB-T3b — …». La sustituye en su sitio, sin cambiar el número de líneas.
- **`fill_result_check` es posicional,** que es lo correcto, porque «| **Estado** | EJECUTADA |» ya está en las líneas 208 y 258 (PB-TC y PB-TC2). Exige que solo cambien cinco índices, cada uno de su vieja a su nueva, y que la entrada de PB-T3c vaya al final, tras una línea en blanco.
- **`m1_check`** busca `tests/m1/m1.trx` en el manifiesto de la unidad `a9e0`, una vez y con su SHA-256.
- **El resto:** la redacción de las cinco nuevas (`clean`), la custodia del archivo de la sesión, la carpeta de la compilación sin su informe (pendiente en las autopruebas, error al escribir), las comprobaciones de F4B-E47 y la 274, y el mensaje del commit X.
- **Las siete negativas** están en el selftest, y todas abortan con el mensaje esperado: N-R1, la vieja ausente; N-R2, repetida; N-R3, fuera de la sección; N-R4, la TRX fuera del manifiesto; N-R5, una sexta línea; N-R6, una vieja distinta; N-R7, cuatro sustituciones.

**Lo comprobé sin el emisor (`cat3c_fill.py`).** Con mi propia lectura del archivo (sha `01c288ac…`, sin CR, cinco pares de una línea) sobre el registro de `fe1c67f4`:
- cada vieja está una vez y dentro de la sección (líneas 272 a 310);
- cambian exactamente las líneas 274, 306, 307, 308 y 309;
- de las nuevas, solo la del estado de la tabla existía ya en el cierre (dos veces);
- con la entrada de PB-T3c al final, el archivo da 32 382 bytes. Es lo que dice `--dry-objects`.

El contenido es el que revisé en el §9.

**Los borradores:**
- **`f4b-e47.md`** gana la viñeta «El registro declarado de PB-T3b», con `{CLOSE8}` y la ruta del archivo de la sesión.
- **La 274** gana la frase de las cinco líneas.
- **Son las versiones que ya leí en el §9.** Ninguna marca nueva.

**De r2 a r3-before: `COMPILE_RESULT`.** Es, carácter a carácter, el literal que di por bueno en el §5, con «método» escrito con `chr(0xE9)`. El selftest comprueba que un `fill` con `COMPILE_RESULT` None aborta.

**De r3-before al de ahora: `REQ_N`,** de 34 a 36 (CA-T3c M-01). Es el único cambio.

**Las autopruebas:**
- **`--selftest`** (`80a734fb…`) da rc 0 y sitúa las cinco líneas en 274 y de 306 a 309.
- **`--dry-objects`** (`5f5b4a67…`) da rc 0, con 56 textos. Ya entran, redactadas, la carpeta de la compilación (`compile_check_ctrl.sh` y `out-ctrl/` incluidos) y este informe en su versión de las §1 a §13.

### 14.5 `compile-check.md` y el control negativo (M-04)

**M-04 está resuelto.** «Lo que no cubre» pone primero, como hueco real, el cuerpo del GREEN de la fachada frente a las guardas de texto que leen todo `src/` o el Plugin, con las operaciones (`GetBlockReferenceIds`, `Erase()`, `UpgradeOpen()` y `OpenMode.ForWrite`), lo que leyó CA-T3c y dónde se prueba de verdad: la suite completa sobre el GREEN, en la CI y en la verificación. El resto del informe no cambia, salvo la sección nueva del control y su referencia en «Cómo se hizo».

**El control negativo es correcto,** y lo que dice cuadra con sus registros:
- **La inyección:** `compile_check_ctrl.sh` mete `private const int PushBackNegativeControl = 0;` en el código de la clase, en una línea que `Clean` no quita, y no recompila. La guarda lee el texto al ejecutar.
- **Con la inyección:**
  - `ctrl-injected.log` da «Con error: 1, Superado: 0, Total: 1»;
  - la TRX da 1 `Failed` (`CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses`, `Assert.Equal()`, diferencia en la posición 0);
  - en el registro salen `archivos=14` (lo esperado) y `archivos=15` (lo real), y `asignaciones=7` en los dos. Falla solo por el archivo nuevo.
- **Al restaurar:**
  - el sha256 es el mismo antes y después (`598769d8…`);
  - «Con error: 0, Superado: 1»;
  - el original, copiado en `out-ctrl/`, se borra después;
  - el checkout principal sigue limpio.
- **Con esto queda probado** que las guardas de texto de la suite completa leen la copia, y en concreto el archivo nuevo.

### 14.6 Lo que queda antes de X

- **El `BOUNDED_RESULT`:** el literal de arriba, en `make_pbt3c_contract.py`. Eso cambia el sha del generador, así que hay que volver a correr el seco con `--bounded-file`, `measure_pbt3c.py` y `check_v47.py` (dudas 12 y 20), y comprobar que `pbt3c-anchors-out.txt` y `check-v47-out.txt` nombran el generador final.
- **El encargo de CA-T3c,** `independent-review-pbt3c-check-prompt.md`, que `--dry-objects` lista como pendiente.
- **Volver a correr `--dry-objects` y `--selftest` con este informe y los guiones nuevos de esta ronda:** `cat3c_r3.py`, `cat3c_args.py`, `cat3c_args2.py` y `cat3c_fill.py`, con sus salidas. `CAT_RX` los recoge como guiones del revisor.
- **M-02 y el «5» de `overload-sweep-eab27a9e.txt`,** a `docs/ideas-futuras.md` en el cierre de PB-T3c. Estoy de acuerdo: M-02 es de una nota que no va al contrato, y la corrección queda escrita en este informe, que se custodia.
- **M-05,** si se quiere, como límite en el docstring del barrido. No bloquea.

No veo nada más. No hay decisiones nuevas para el Owner.

### 14.7 Hallazgo nuevo

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-05 | M | `overload_sweep_t3c.py`, `generic_close` (el arreglo de M-03) | Una comparación sin espacios cuyo `>` va seguido de un identificador, como `(a<b, c>d)`, se toma por una lista genérica, y sus dos argumentos cuentan como uno. C# la lee como dos comparaciones: solo es genérica si tras el `>` viene `( ) ] } : ; , . ? == != \| ^ && \|\| & [`. Las autopruebas del guion prueban comparaciones con espacios o sin `>`, y no este caso. No afecta a PB-T3c, porque ninguna llamada a los objetivos de T3C-1 lo tiene | Mirar el carácter tras el `>` que cierra, con la regla de C#, y añadir el caso a las autopruebas; o anotarlo como límite en el docstring |

### 14.8 El `BOUNDED_RESULT` final, comprobado

**Cómo se comprobó.** `cat3c_bounded.py` aplica la `bounded_check` de `make_pbt3c_contract.py` (`8D1804CF…`, la misma función de antes) con `reviewer='CA-T3c'` a este informe entero, con la §14, a la v4.7 y al literal final.

**La línea del §13 que decía que este informe «no tiene recomprobaciones»** se refería al informe antes de esta sección. No la reescribo.

El resultado de la comprobación va en el §14.9.

### 14.9 El resultado de `bounded_check`

- **Con el literal final:** 0 errores. La función reconoce el primer veredicto (B 0 / I 0 / M 4) y la recomprobación de la §14 (EMITIBLE, B 0 / I 0 / M 1). El literal es ASCII, empieza por «EMITIBLE;» y lleva los dos recuentos. El informe no tiene rutas locales.
- **Con el literal de la primera ronda,** el que no lleva el sufijo: da 1 error, el esperado, «BOUNDED_RESULT no nombra la ultima recomprobacion: falta 'recomprobacion v4.7: EMITIBLE, B 0 / I 0 / M 1'».
