> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktrees-de-Codex>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo de la comprobación acotada de PB-T3c (CA-T3c) (enviado por la sesión al revisor de PB-T3 como mensaje; texto exacto)

## 1. CA-T3c, sobre la v4.7 (2026-11-20T12:49:24.454Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T3c (CA-T3c)** antes de emitirla.

**La autorización:** F4B-E35, pregunta 3 (el Owner, «Sí, con esa condición»). La condición: 0 bloqueantes, 0 importantes sin resolver y que la remedición sobre el cierre de PB-T3b no cambie nada material.

**El estado:**
- **PB-T3b está CERRADA** (F4B-E46), con la custodia `fe1c67f4efa23f2abb9b6ab9d636df54ae15f343`, GREEN `c7582785`, RED `9fda277e`, intento 2 de 3, y la CI del GREEN `44707482956` (Core 19 124 = 19 118 Passed + 6 Skipped, con la Theory S-2 como una fila; UI 2 451 = 2 430 + 21).
- **nc3 de PB-T3b:** el contrato decía «19128 … (+6 o +10 Skipped)» y salió 19 124, la rama «+6». El Owner lo aceptó por disposición y fijó la lección: desde PB-T3c, una evidencia esperada que dependa de cómo cuente la TRX lleva el total exacto de cada rama (`F4B-DRAFTS/coordinator/owner-pbt3b-nc3-decision.md` y la medición por clase en `F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/`, en `fe1c67f4`).
- **El worktree está parado**, sin delegación activa. Aun así, el código se lee solo por objetos git desde el checkout principal, en `fe1c67f4`.

**Las entradas** (en `hs65\pbt3\`, salvo que se diga otra cosa):
- **el paquete v4.7:** `I-64-f4b-pbt3-gate-contract-package-v47.md` (sha `976361c0…`), con `v47-delta.md` (V47-01 a V47-07), `make_v47.py`, `check_v47.py` y `check-v47-out.txt`. La v4.6 que custodió `2bb3b759` ya la comprobaste en tu §13 de CA-T3b. Ahora tocan el delta y todo lo que se refiere a PB-T3c: §4.1, §5.0, §5.4, §6.0, §6.3, las precondiciones 1, 9, 12 y 13, y las filas de PB-T3c de la tabla de RR-3;
- **el generador:** `make_pbt3c_contract.py` (`46E77EDD…`), junto con `issue_pbt3c.py`, `issue_pbt3c-selftest.txt` e `issue_pbt3c-dry-objects.txt`;
- **el seco:** `dry-pbt3c.txt` y `contract-dry-pbt3c.json` (sha `2A1FF346…`), con `neg-pbt3c.txt` (77 de 77);
- **la remedición sobre el cierre:** `measure_pbt3c.py`, `pbt3c-anchors-out.txt`, `sweep-pbt3c-close.txt`, `overload-sweep-pbt3c-fe1c67f4.txt`, `name-check-pbt3c-fe1c67f4.txt` y la R-23 de `pbt3-measurements.md`;
- **el esqueleto:** `pbt3c-skeleton.patch` (`a81de435…`) y `make_pbt3c_skeleton.py`;
- **los borradores:** `f4b-e47.md`, `evidence-274.md`, `evidence-275.md` y `superseding-pbt3c.md`;
- **las dudas del generador:** `generator-doubts-pbt3c.md`, con su lectura (21 dudas);
- **los sustitutos:** `hs65\nc1-cascade\limits-declared.md` (PD-F4B-01, `41440edc…`) y `hs65\nc2-substitute-f4b-v2\limits-declared.md` (v2, `C033E96C…`), con `--task PBT3C` en su lista cerrada;
- **lo ya aceptado en PB-T3b:** el contrato reemitido, `git show fe1c67f4:docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json`; tu informe `independent-review-pbt3b-check.md`; y, de la revisión independiente del código de PB-T3b, los menores PBT3B-IR-M01, M02 y M07 en `docs/ideas-futuras.md` de `fe1c67f4` (V47-06).

**Lo que la sesión ya decidió** (no lo decides tú, pero dime si ves un problema):
- **Duda 17, el registro declarado de PB-T3b:** el cierre dejó su tabla de ejecución sin rellenar. El commit X de PB-T3c la completa con cinco sustituciones exactas, y F4B-E47 lo declara. El texto exacto y sus comprobaciones están en `superseding-pbt3b-close-fill.md`. **Revísalo:** que cada dato tenga su fuente en `fe1c67f4` y que no cambie nada más.
- **Dudas 12 y 20:** antes de X se vuelven a correr el seco con `--bounded-file`, `measure_pbt3c.py` y `check_v47.py`, para que las salidas custodiadas nombren el generador final.
- **Duda 16:** la comunicación al Owner la hace la sesión. **Dudas 14, 18 y 21:** de acuerdo con la lectura del redactor.

**Mientras revisas:**
- el redactor añadirá a `issue_pbt3c.py` solo las cinco sustituciones del registro. Lo que lees ahora no cambia en lo demás; cuando esté, te pediré que compruebes el delta;
- la sesión hace la compilación de comprobación (precondición 13) sobre una copia exportada nueva de `fe1c67f4`: el esqueleto, Core-tests y UI-tests en Debug, el Plugin en Release con `-o`, un control positivo (el tipo y el método del esqueleto en el ensamblado) y la suite completa de Core. Va en `hs65\pbt3ccc\compile\`. Cuando acabe, te mandaré el resultado para tu informe.

**Qué comprobar:**
1. **La fidelidad del contrato en seco frente a la v4.7 para PB-T3c.** Revisa:
   - el alcance: las 4 rutas, y WR como UNIT_CHANGE;
   - el ForbiddenWriteScope (181);
   - las 12 invariantes, con el hueco 05-06 (duda 13);
   - las 33 paradas, con C04, C13, C16, C17 y la C11 literal; y el texto común de C02 que nombra PB-T3b (duda 15);
   - los 34 RequiredTests (dudas 1 y 4);
   - las 15 entradas de evidencia (duda 2); la primera tiene que ser uno de los literales de `F4B_EXACT_EVIDENCES` del script v2 de nc2;
   - las 50 autoridades y las 28 anclas;
   - los marcadores.
2. **La lección de F4B-E46 aplicada:** un `[Fact]` por CT (duda 6), el RED y el GREEN previstos con una sola rama (duda 7) y lo esperado en la precondición 13 (duda 8). ¿Cuadran con la TRX del GREEN de PB-T3b y con lo que añade PB-T3c? ¿Queda algún recuento que dependa de cómo cuente la TRX y no lleve su total exacto?
3. **RR en PB-T3c:** la regla con la lectura de PB-T3c y las dos filas de PB-T3c de la tabla de RR-3 (duda 3).
4. **El esqueleto y los barridos:** la firma de D-PBT3-11, el esqueleto sin comentario (duda 9), y los barridos de sobrecargas, reflexión y nombres (T3C-1; dudas 10 y 11). Recuerda la lección de PB-T3b: la reflexión por nombre (`Assert.Single`) y los escaneos de ensamblado no los ve un barrido por nombre. ¿Cubren T3C-1 y la precondición 13 ese hueco para la fachada nueva y para el cambio del constructor de `RackPilotMutationScope`?
5. **La remedición:** ¿cambia algo material frente a lo que mide la v4.6? Incluye los marcadores rellenos (cinco grupos y seis sitios, duda 5) y sus fuentes.
6. **V47-06:** que M01, M02 y M07 de PB-T3b estén bien llevados a la nota del §5.4.
7. **Las CT de PB-T3c** frente al momento de cada lectura y a los gestos que no cambian nada, con los diseños de PB-T3b que llegan al borrado (`EraseUnit` en WR y el constructor de `RackPilotMutationScope`).
8. **Decisiones nuevas para el Owner,** si las hay.

Las dudas 1 a 4, 6, 7, 13 y 15 las decides tú. Las demás, confírmalas o corrígelas.

**Formato:**
- El informe va en `hs65\pbt3\independent-review-pbt3c-check.md`, con secciones numeradas (§1, §2, …).
- El veredicto va en una línea propia: «**Veredicto: EMITIBLE …**» o «**Veredicto: EMITIBLE TRAS CORRECCIONES …**», con B / I / M.
- Propón el literal ASCII de `BOUNDED_RESULT`, que debe empezar por EMITIBLE. Mira cómo lo comprueba `bounded_check` en el generador.

**Reglas:**
- nada en `<worktrees-de-Codex>\...`;
- Python con `-I` y solo en scripts escritos con Write, sin `-c`, sin `python -` y sin heredoc;
- escribe solo en `hs65\pbt3\`, en ese informe y en guiones con el prefijo `cat3c_`.

**Responde con:**
- el veredicto;
- los hallazgos, uno por línea;
- el literal.

### 1.1. Complemento enviado durante esta ronda (2026-11-20T12:56:13.548Z)

Mensaje de la sesión, para tu § del esqueleto y los barridos (punto 4 del encargo): la compilación de comprobación de la precondición 13 ya está hecha. Inclúyela en tu informe.

**Dónde:** `hs65\pbt3ccc\compile\`. El informe es `compile-check.md`. Están también el guion `compile_check.sh`, la salida `out-run1.txt`, el resumen de la TRX `trx-outcomes.txt` (con `trx_outcomes.py`) y los registros de MSBuild y VSTest en `out\`.

**El resultado:**
- la copia sale de `git archive fe1c67f4`; el esqueleto `a81de435…` aplica limpio, y el archivo no existía en la base;
- Core-tests Debug, UI-tests Debug y el Plugin Release (`-o`): rc 0 y 0 errores en los tres, sin ningún aviso que nombre el esqueleto;
- el control positivo: `RackViewBlockEraser` y `EraseInTransaction` están en `RackCad.Plugin.dll`;
- **la suite completa de Core: 19 124 = 19 118 pasadas + 0 fallidas + 6 omitidas.** Las omitidas son S-1 a S-6 de D-1, con su motivo literal; la Theory S-2 cuenta como una fila. Es lo esperado en la precondición 13 y coincide con la CI del GREEN de PB-T3b;
- `build-server shutdown` hecho; el checkout principal, sin cambios.

**Lo que no tiene:** control negativo de choque, porque T3C-1 no encuentra ninguno que reproducir; el control es el positivo. El informe dice por qué y qué queda sin cubrir.

**Qué te pido:**
- que digas si esto cumple la precondición 13 tal como la escribe la v4.7;
- si la falta de control negativo es aceptable o hace falta uno;
- y si `compile-check.md` dice algo que no cuadre con los registros.

**El literal que pondré en `COMPILE_RESULT`**, salvo que lo corrijas:
«Core-tests y UI-tests en Debug y el Plugin en Release (con -o), 0 errores; el tipo y el método del esqueleto en el ensamblado del Plugin; la suite completa de Core sobre la copia exportada de fe1c67f4, 19 124 = 19 118 pasadas + 0 fallidas + 6 omitidas (S-1 a S-6 de D-1)».

## 2. Recomprobación 1 (2026-11-20T13:28:50.958Z)

Recomprobación de CA-T3c, sobre la v4.7 (que no cambia): comprueba los tres deltas que salieron de tu informe y del registro de PB-T3b.

**1. El delta de `issue_pbt3c.py`.** Tú lo dejaste pendiente en la duda 17.
- Lo describe `hs65\pbt3\issue-pbt3c-delta-r2.txt`. La versión anterior está en `_ref\issue-r1\`.
- El sha actual es `88e8a2a7…`, y lleva tres cambios:
  - **las cinco sustituciones** del registro de PB-T3b, leídas de `superseding-pbt3b-close-fill.md`, con sus siete negativas (N-R1 a N-R7, en el selftest). La fila nueva «| **Estado** | EJECUTADA |» ya existe dos veces en el archivo (PB-TC y PB-TC2), así que la comprobación es posicional;
  - **`COMPILE_RESULT`**, con el literal que aprobaste;
  - **`REQ_N`**, de 34 a 36, por tu M-01.
- Las viñetas nuevas de `f4b-e47.md` y `evidence-274.md`.

**2. El delta del generador** (`delta-r3-pbt3c.txt`; lo de antes está en `_ref\r3-before\`):
- **M-01:** el generador pasa a `8D1804CF…` y el seco a `682F9986…`. Las pruebas exigidas pasan de 34 a 36, con `PushBackFullWrite` 13 y `PushBackPilotUpdateEquivalenceGuardTests` 6, `ExpectRed` false y el orden de PB-T3b. Lleva cinco negativas nuevas (G17 a G21), 82 de 82 en total. La duda 1 queda como «decidida por CA-T3c: incluir»;
- **M-03:** `overload_sweep_t3c.py` corregido, con sus autopruebas. La salida cambia en una línea (5 → 4 parámetros) y ninguna conclusión cambia;
- `measure_pbt3c.py` y `check_v47.py` vueltos a correr, y `--selftest` y `--dry-objects` con rc 0.

**3. `compile-check.md` y el control negativo** (`hs65\pbt3ccc\compile\`):
- **M-04:** «Lo que no cubre» nombra ahora como hueco real el cuerpo del GREEN de la fachada frente a las guardas de texto que leen todo `src/` o el Plugin;
- **el control negativo opcional que propusiste,** hecho con `compile_check_ctrl.sh` (`out-run-ctrl.txt`, `out-ctrl\`). En la misma copia y sin recompilar, metí `PushBack` como identificador en el código de la fachada. CT-PBT1B-15 falló 1 de 1 (espera `archivos=14` y la copia da 15). Después restauré el archivo byte a byte (mismo sha256) y volvió a pasar 1 de 1.

**M-02 no se aplica a la v4.7** (no va al contrato). Lo llevaré a `docs/ideas-futuras.md` en el cierre de PB-T3c. Dime si no estás de acuerdo.

**Qué te pido:**
- que los tres deltas sean fieles a tus hallazgos y no cambien nada más;
- que el seco `682F9986…` sea fiel a la v4.7 con tu lectura de M-01;
- y si queda algo antes de X.

**Formato:**
- Añade al informe una sección con este encabezado exacto: `## §N. Recomprobación sobre la v4.7`, donde N es el siguiente número. `issue_pbt3c.py` cuenta las rondas por ese encabezado.
- Pon tu veredicto en una línea propia.
- Propón el `BOUNDED_RESULT` final: tu literal con un sufijo para la recomprobación (en PB-T3b fue «; recomprobacion v4.6: EMITIBLE, B 0 / I 0 / M 2»). Compruébalo con `bounded_check` de `make_pbt3c_contract.py` (`8D1804CF…`).

**Las reglas de siempre**, con guiones `cat3c_`.

**Responde con:** el veredicto, los hallazgos y el literal final.
