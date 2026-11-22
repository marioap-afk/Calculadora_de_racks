> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Traspaso de PB-T4d (paquete v5.2), fase 3: las piezas literales

**Fecha:** 2026-11-22. **Base:** el cierre de PB-T4c, `6c2820bab4d8b41e3e229e23f3a1638ccbb04735` (F4B-E55). **Rama:** `architecture/workspace-persistente-rackcad`. **No se emite nada todavía:** falta la v5.2 y su precondición 6. Este traspaso sigue al de la fase 2 (`v52/handoff-pbt4d.md`, `2d9e6c0c…`), que conviene leer antes.

**Carpetas:** `S = <scratchpad>\hs65`.
- Las piezas del paquete van en `S\pbt4\`.
- Los registros van en `S\pbt4\v52\`.
- Los conductores y las corridas van en `S\pbt4dcc\compile\`.

**Las reglas de siempre:**
- Nada en el worktree: ni leerlo ni escribir su ruta.
- Ningún commit. En el checkout principal, solo `git show`, `git archive`, `git ls-tree` y `git grep`.
- Python solo con `-I`, desde guiones escritos con Write. Nunca `-c`, `python -` ni heredoc.
- Copias exportadas nuevas en cada medida. **La siguiente libre es `/d/r6m`.**
- En UI, solo los filtros, nunca la suite completa.
- dotnet, el del usuario (`$<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe`), con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false` y `build-server shutdown` al final.
- Nombres cortos; salidas en UTF-8 con LF.
- **Nada de `/tmp`.**

## 1. Las decisiones que fijan PB-T4d

**Del Owner:**
- **`owner-pbt4d-list-decisions.md`** (`40a52fea…2962`):
  - el alcance B;
  - la recarga de «Editar todo», a una tarea propia;
  - la prueba lenta, acotada en PB-T4d;
  - la emisión condicional: comprobación acotada 0/0, remedición sin cambio material y la lista elegida o una parte.
- **`owner-pbt4d-substitutes-decision.md`** (`12a41c3f…0d75`): «Sustitutas donde hace falta (Recomendado)».
  - Se quedan las 7 CT de PB-T3b que pasan y sus 6 ayudantes.
  - Lo que también prueba código que se queda conserva esa parte, como equivalente o como edición con registro.
  - Lo que solo fija lo retirado, o pasa en vacío, se borra con registro.
- **`owner-pbt4d-run-coverage-decision.md`** (`5c852b6a…120c2f`): «Sustituir las 21 (Recomendado)».
  - Las 21 de WPUR de `v52/cov-tests-r6k.txt` pasan a equivalentes sobre `ExecuteFull`, en `PushBackRetirePilotRunTests` (`ExpectRed` false, filtro por clase exacta), con un ayudante del modelo completo, copiadas con el mapa de PB-T4b y a mano donde no case.
  - Las otras 27 se borran con registro.
  - **La sesión añade:** la precondición 6 mide otra vez la cobertura con coverlet (como en `/d/r6j`), con la retirada aplicada. Ninguna de las 436 líneas ni de los 9 caminos puede quedar sin cubrir. **Si alguno queda sin cubrir, PARA y vuelve a la sesión.**

**De la sesión,** en `session-pbt4d-design-decisions.md` (`c5ddb937…80f2`):
- **D-S11:** el RED lleva todo lo de `tests/`, `src/` queda intacto y no hay esqueleto. Hay dos CT:
  - CT-PBT4D-01, `PushBackRetirePilotGuardTests`, con `ExpectRed` true;
  - CT-PBT4D-02, `PushBackRetirePilotCensusGuardTests`, con `ExpectRed` false.
- **D-S12:** las 3 que caen al ejecutar llevan `Skip` y equivalente, con el prefijo `PushBackRetirePilot` y filtros por clase exacta.
- **D-S13:** M-04 se reescribe en su sitio.
- **D-S14:** la prueba lenta recorre cada 97.º toque y el último.
- **D-S15:** se limpian los cuatro `cref` y el comentario huérfano. Por eso `PushBackPilotDraftState.cs` pasa a `UNIT_CHANGE`.

## 2. Las piezas, con su sha256 y cómo regenerarlas

| Pieza | Dónde | sha256 | Cómo |
|---|---|---|---|
| **GREEN** de `src/`: B tal cual se midió (8 EDIT y 2 DELETE) más la limpieza de D-S15 (5 EDIT) | `pbt4/pbt4d-green-v52.txt` | `966f17a5b0bc66de12fc5a9b5e6aa6cbfb0d7b2f50af281f9341a6b670c0cf33` | `python -I v52/make_green_v52.py <repo> /d/r6e <S>/pbt4dcc/compile/r6e-B/bucle/retire-4.txt pbt4d-green-v52.txt v52/make_green_v52-out.txt` (desde `pbt4/`). Comprueba los sha256 de B frente al registro de `/d/r6e` y la limpieza (`b943f78c…`) |
| El módulo del GREEN | `pbt4/green_pbt4d_v52.py` | `efeeffe8…7692` | Aplica la pieza por objetos git (bloques EDIT y DELETE; un NEW sin líneas es el texto vacío) |
| El aplicador del GREEN | `pbt4/apply_green_pbt4d_v52.py` | `e0963560…6732` | `python -I apply_green_pbt4d_v52.py <copia> --repo <repo> --rev 6c2820ba` |
| **RED** de `tests/` (borrador sin las 21) | `v52/try/red-draft-1.txt` | `270f8a29…2527` | `python -I make_red_v52.py <repo> <S>/pbt4dcc/compile/r6h-nd/bucle/necesidad.tsv <árbol> <pieza> <registro>` (desde `v52/`; `make_red_v52.py` es `b4b0a58f…`). **Hay que regenerarlo:** CT-01 cambió después (abajo), y faltan las 21. La pieza final va a `pbt4/pbt4d-red-v52.txt` |
| El módulo y el aplicador del RED | `pbt4/red_pbt4d_v52.py` y `pbt4/apply_red_pbt4d_v52.py` | `47ee55e0…bdcbde` y `f229e3e9…9c00` | Aplica EDIT y NEWFILE, solo `tests/`. `apply_red_pbt4d_v52.py <copia> --repo <repo> --rev 6c2820ba [--pieza <pieza>]` |
| Las ediciones con registro, los `Skip` y lo que se queda | `pbt4/pbt4d_test_edits_v52.py` | `abf07a26…f7e8` | Son datos: `EDITS` (ruta, desde, hasta, nuevo y porqué), `STALE_CALL`, `SKIPS` y `KEPT`. Las lee `make_red_v52.py` |
| CT-PBT4D-01 | `v52/ct/PushBackRetirePilotGuardTests.cs` | `0605a7f0…e668a` (con el arreglo de `ComparePostFull`) | 31 líneas literales (§5.6 en `v52/frag_56.md`) |
| CT-PBT4D-02 | `v52/ct/PushBackRetirePilotCensusGuardTests.cs` | `5f9f0e82…edc0` | 10 líneas literales |
| El equivalente de las dos guardas de WPPG | `v52/try/PushBackRetirePilotPlanGuardTests.cs` | `48c2f4a9…ca43` | El tipo y la fábrica de `PushBackFullPrepareInput`, y el orden de `PrepareFull` con `BuildFull(` |
| El equivalente nuevo de CT-PBT4B-20 | lo genera `make_red_v52.py`: `PushBackRetirePilotFullCopyGuardTests.cs` | `633c1ec2…` en el borrador | `v52/make_fullcopy_eq_v52.py` (`98065a8e…`), una copia literal del de PB-T4c con los `BaseSha`, los `Skip`, las `CopySha` y los recuentos que se calculan sobre los textos finales del RED. Sobre B reprodujo la sonda: 7 originales, la `CopySha` de WPP, y EIH 60/55/5, WPEH 41/41/0, WPP 33/25/8 y WPRH 21/12/9 |
| La lista fila a fila | `v52/make_list_v52.py` (`a5d20914…`) | hoy da 242 filas, 229 con acción (`pbt4d-retire-list-229.tsv`, en `v52/try/list1/`) | `python -I make_list_v52.py <repo> <S>/pbt4dcc/compile <necesidad.tsv> <dir>`. **Hay que rehacerla con las 21** (`WPUR_SUBST` en `pbt4d_test_edits_v52.py`, más la acción «sustituir con registro»). La final va a `pbt4/pbt4d-retire-list-<n>.tsv` |
| §5.6 (las CT) | `v52/frag_56.md` | `06890b26…d3fb` | Fragmento para `make_v52.py` |

**Lo que hace el RED** (`make_red_v52.py`):
1. **Las bajas.** Son las necesarias de `/d/r6h` (201), menos las 17 que se quedan editadas (`EDIT_KEEP`), más las 9 que compilan y se quitan (`EXTRA_DELETE`): las dos guardas de composición de WPUG, las 6 del comparador del piloto de State y G4 de IG :510-517. Salen **193**. Las quita `retire_pbt4d_v52.py` sin tocarlo, con `--dry`.
2. **Las ediciones** (pueden ser tramos de línea a línea), la llamada de StaleBase (4 veces) y los tres `Skip`.
3. **Los cuatro archivos nuevos.**
4. **La pieza:** hunks de difflib con un OLD único y deslizamiento hacia los `///`. Dos detalles:
   - si `retire_pbt4d_v52.py` deja un archivo sin el salto de línea final, se le devuelve;
   - un borrado hasta el final del archivo se desliza una línea hacia arriba.

**Las ediciones con registro** (lo que también prueba código que se queda):
- CT-PBT3B-05, 06 y 09, sin sus partes del piloto;
- E-3, E-5 y E-6, sin el lado del piloto (`Build` y `Execute`);
- H-06 de WPPG, sobre `PushBackFullComposition.ForFullWrite`, con sus dos pruebas y su ayudante `AfterMatchViolations`;
- PEG `HostViolations`: la regla del adaptador estático pasa a `PushBackFullFieldAdapter`, como su copia, y así sus 6 autopruebas de la lista dejan de pasar en vacío;
- M-04, las tres;
- la prueba lenta;
- Ownership: `Kinds` y `Both` solo con «box», y fuera las dos filas del piloto de la `Theory` de POST;
- StaleBase: `UnknownAfterACommitThatThrew(PushBackPilotIntentScan)` sobre `PushBackFullDraftReader.Read`, con la directiva de `Draft/`;
- `MaterializeReproduces…`: compara por el diseño armado (`PilotKit.Persist(BuildDesign(...))`).

## 3. Las medidas de esta fase

| Copia | Qué | Resultado |
|---|---|---|
| `/d/r6g` | Necesidad: bucle desde cero bajas | **PARA:** un error en una línea `[InlineData]` no se llevaba a su método. Corregido en `measure_need_v52.py`. Queda como historia |
| **`/d/r6h`** | Necesidad (`p6d_need_v52.sh`) | **201 de las 235 bajas de B hacen falta y 34 no; ninguna fuera de B.** Compila con 0 errores y 0 `warning CS`. Core: 19 375 = 18 960 + 15 + 400. UI: los 18 filtros, 0 `Failed`. El informe, en `v52/need-report-r6h.txt`. De las 34: 16 pasan (7 CT de PB-T3b, 9 en vacío o por accidente) y 12 caen al ejecutar |
| `/d/r6i` | Cobertura | **PARA:** el colector no acepta la configuración por línea de órdenes. Se pasó a `pbt4dcc/compile/cov_v52.runsettings`. Queda como historia |
| **`/d/r6j`** | Cobertura (`p6d_cov_v52.sh`): WPUR sola, el resto de Core sin WPUR y corridas por clase | WPUR ejercita **436** líneas que se quedan; el resto cubre 401; **35 líneas y 22 con ramas solo WPUR, en 9 miembros** (`v52/cov-r6j.txt`) |
| **`/d/r6k`** | Cobertura prueba a prueba de WPUR (`p6d_covtest_v52.sh`) | **21 de las 49 tienen algo propio**; el mínimo son 17 (`v52/cov-tests-r6k.txt`, `0ed03cc9…69c1`). Las 21 son estas, por número: 08, 10, 11, 12, 15, 20, 21, 23, 24, 26, 28, 30, 31, 32, 33, 34, 35, 37, 40, 41 y 43 |
| **`/d/r6l`** | Ensayo rápido (`p6d_quick_v52.sh`) con `red-draft-1.txt` y el GREEN, sobre las clases editadas o nuevas | RED: Core y UI compilan con 0 y 0. En las 686 filas filtradas solo cae CT-PBT4D-01 (lo previsto). GREEN: Core, UI y Plugin con 0 y 0. **CT-PBT4D-01 también cae, por `ComparePostFull=2`:** su expresión contaba la llamada dentro de `ExecuteFull`. **Arreglado:** ahora busca la declaración (`\bpublic\s+static\s+string\?\s+ComparePostFull\s*\(`). Todo lo demás pasa en las dos ramas (los 3 equivalentes, CT-02 y todas las ediciones). Falta volver a correrlo |

## 4. Lo que falta

1. **Las 21 sustitutas de WPUR**, en `tests/RackCad.Tests/PushBackRetirePilotRunTests.cs`.
   - **El ayudante `FullUpdateSetup`:** una copia de `PilotUpdateSetup` (`WorkspacePilotUpdateScopeDouble.cs:238-279`) con estas piezas:
     - `FullWriteKit.Read(scan)` y `FullWriteKit.DraftOf(read)`;
     - la escala por `FullWriteKit.Edit(d, s => s.Editable.AnnotationScale = scale)`;
     - `ExpectedPlan()` con `PrepareFull(PushBackFullPrepareInput.From(...))`;
     - `Run()` con `ExecuteFull`.
   - **Las pruebas:** las de WPUR, con el mapa de PB-T4b: `PushBackPilotDraftState` → `PushBackFullDraftState`, `.Inputs.Annotations.AnnotationScale` → `.Editable.AnnotationScale`, `PilotUpdateSetup` → `FullUpdateSetup`, `PushBackPilotUpdateRun.Execute(` → `ExecuteFull(`. Donde no case, a mano. Llevan también los ayudantes de WPUR que usan (`HappyCalls`, `Boom`, `Cause` y otros).
   - **Después:** se añaden a `make_red_v52.py` como archivo nuevo, se quitan de `EDIT_KEEP`, siguen como bajas en WPUR, y la lista gana sus filas.
2. **La precondición 6 entera,** desde `/d/r6m`, con los pasos de la v5.1 adaptados:
   - **RED exacto** (sin las CT): suite completa de Core y filtros de UI. Lo previsto es 0 `Failed`.
   - **GREEN literal:** lo mismo, y 0 `warning CS`.
   - **Las sondas.**
   - **El ensayo de CT-01 y CT-02** en las dos ramas, con su control. Por ejemplo, sobre la variante A, donde CT-01 tiene que caer.
   - **El censo de C13:** ninguna declaración nueva en `src/`, y quitadas exactamente las de la lista cerrada.
   - **La cobertura con coverlet sobre el GREEN,** contra las 436 líneas y los 9 caminos de `/d/r6j`. Las líneas del GREEN cambian de número: hay que casarlas por texto o por miembro. **Si alguna queda sin cubrir, PARA.**
   - **La duración de la prueba lenta** y de la suite.
   - **Los `RequiredTests`:** los heredados (`v52/pbt4c-required-6c2820ba.tsv`) bajan sus `MinSelected` en las bajas que seleccionan, con las cuentas de base de `/d/r5j/_trx/core-green.trx` y la UI de `g-ui-*`, y se lee con `v52/required_v52.py`. El n.º 37 (`PushBackFieldValidationConcordanceTests`) y el n.º 85 (`PushBackEditAllFullCopyGuardTests`, ya saltado) quedan en 0 y salen. Se añaden los nuevos por clase exacta: las 2 CT y los 4 equivalentes (`FullCopy`, `Plan` y `Run`). **Los totales de cada rama, en cifras.**
   - **Las salidas** van a `pbt4dcc/compile/`: `pbt4d-precondition6.json` y `compile-check.md` (con los resultados por clase). Nombres cortos: ninguna ruta pasa de 156 caracteres con el prefijo `…/F4B-DRAFTS/pbt4/`.
3. **La v5.2,** desde la v5.1 (`34ca0026…94c3`), con un `make_v52.py`. Cambian o se añaden:
   - la cabecera y el registro v5.1 → v5.2;
   - §0, §1, D-PBT4-10, §4.4 (las guardas de PB-T4d) y §5.4;
   - §5.6 (`frag_56.md`);
   - §6.0: `UNIT_CHANGE` de PB-T4d, RR-2, lo que comprueba el Controller y las filas de RR-3 (una por clase de las CT: CT-01 y CT-02);
   - **§6.4 entera,** con §6.4.1 (los textos literales: `Objective` en ASCII, `AllowedWriteScope` ruta a ruta, INV-PBT4D-01 a 14, las paradas PBT4D-C01 a C18, `ExpectedEvidence`, `RequiredTests` y los `Skip` literales) y §6.4.2 (lo que necesita el generador);
   - §7, §8.3 y §8.5 (las tandas 10.ª a 12.ª y D-S11 a D-S15), §9 y §11;
   - el anexo V52-A: el GREEN literal, las ediciones con registro, los `Skip` y los archivos nuevos (las bajas, por la lista y el sha256 de la pieza del RED).
   - **Su `TaskId`:** `F4B-PBT4-RETIRE-PILOT` (21).
   - **La CI del RED:** `tests` en failure (CT-01), `ui-tests` en success, `build-ui` en success y `build-plugin` omitido.

## 5. Las lecciones

- **La lista por texto no es la necesaria.** El bucle de B empezaba con las 222 filas ya quitadas y nunca midió si hacían falta. Partiendo de cero bajas, 34 sobraban, entre ellas 7 CT de PB-T3b del modelo completo. **Se mide desde cero.**
- **Compilar no basta.** Al quitar un campo `Comparer`, `Comparer.Equals` se resuelve como `System.Collections.Comparer` (`object.Equals`): la prueba compila y cae, o pasa en falso.
- **«Lo cubren otras» se mide con cobertura** (coverlet, líneas y ramas, prueba a prueba), no se afirma: la premisa sobre las 48 de WPUR valía para 401 de 436 líneas.
- **La configuración de coverlet,** en un `.runsettings`: la de la línea de órdenes no la acepta esta versión de VSTest.
- **Un mapeo de errores que no cubre las líneas de atributo** (`[InlineData]`) deja el bucle sin miembro.
- **`retire_pbt4d_v52.py` puede dejar un archivo sin su salto de línea final,** y una pieza EDIT con OLD/NEW con `\n` no lo expresa.
- **Una expresión de «se queda» tiene que casar con la declaración,** no con las llamadas.
- **Deslices declarados:** un `python -I -c` que solo leía el contrato de PB-T4c por la entrada estándar, y un archivo de prueba en `/tmp`, borrado al momento. El checkout principal, limpio en cada medida.
