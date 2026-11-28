# Traspaso de PB-T4d (paquete v5.2), fase 4: las 27 sustitutas, la rama muerta y la precondición 6

**Fecha:** 2026-11-22. Sigue al de la fase 3 (`v52/handoff-pbt4d-v52.md`, `4bc2c391…bdd6`), cuyas reglas y carpetas valen igual. **Base:** `6c2820ba`. **No se emite nada todavía.**

## 1. Lo nuevo decidido

- **`owner-pbt4d-deadcode-27-decisions.md`** (`61b0c77d…c853`), tras el alto del relevo (`v52/sub/cov-sides.txt` y `cov-otras.txt`, con sus guiones):
  - **(a) «Quitarla en PB-T4d»:** el GREEN quita también el parámetro `allowErase` de `BuildCore` y lo que solo existe con él. Si eso hace caer algo fuera de la lista, PARA.
  - **(b) «Sí, 27»:** las sustitutas de WPUR son las 21 más la 06, 07, 16, 19, 29 y 42. La precondición 6 mide la cobertura rama por rama (JSON de coverlet). Si una rama de la corrida que se queda sale sin cubrir, PARA.
  - **Una errata de la pregunta:** dice «se borran 22»; son 21 pruebas (48 − 27). Con `StatusAfter`, 22 filas de WPUR salen del archivo, pero `StatusAfter` va copiado en la clase nueva.

## 2. Las piezas de esta fase

| Pieza | Dónde | sha256 | Cómo |
|---|---|---|---|
| **GREEN** con la rama muerta | `pbt4/pbt4d-green-v52.txt` | `4059860f…4932` | `v52/make_green_v52.py` (`49a3db9e…ab9c`), mismo uso; bloques `DEADCODE` al final (10 EDIT en el plan). La anterior, `v52/sub/hist/pbt4d-green-v52-966f17a5.txt` |
| **Las 27** | `v52/run/PushBackRetirePilotRunTests.cs` | `d5fd4119…6147` | `python -I make_run_eq_v52.py <repo> run/PushBackRetirePilotRunTests.cs run/make_run_eq-out.txt` (desde `v52/`): 25 por el mapa de PB-T4b, la 10 a mano (FrontalBOfOneWay), la 43 con `FullWriteKit.Read` y `ComparePostFull`, y `FullUpdateSetup` |
| Ediciones | `pbt4/pbt4d_test_edits_v52.py` | ver `sha256sum` | + 2 EDITS de E-3 (PEQ: fuera las reglas de `allowErase` y `ViewUnavailable`; queda `disposition == …ObsoleteLateral`, que vale en las dos ramas) y `WPUR_SUBST` |
| CT-01 | `v52/ct/PushBackRetirePilotGuardTests.cs` | `0d64806a…adb3` | + 2 retirados: `BuildCore(allowErase)` y `ViewUnavailable` → 35 líneas |
| CT-02 | `v52/ct/PushBackRetirePilotCensusGuardTests.cs` | `1874c478…3f20` | + `ViewUnavailable` → 11 líneas |
| **RED** borrador 2 | `v52/try/red-draft-2.txt` | `00e311b6…eebe` | `make_red_v52.py` (con el archivo nuevo de `run/`), árbol `try/rt2` |
| Lista | `v52/try/list2/pbt4d-retire-list-229.tsv` | `1943468b…1fad` | `make_list_v52.py`: 242 filas, 229 con acción; WPUR: 28 «sustituir» (27 y `StatusAfter`) y 21 «borrar» |
| §5.6 | `v52/frag_56.md` | ver `sha256sum` | CT-01 35, CT-02 11, y la clase de las 27 |

Las versiones anteriores de todo lo que cambió, en `v52/sub/hist/`.

## 3. Medidas

- `/d/r6m`: ensayo rápido (`pbt4dcc/compile/p6d_quick2_v52.sh`, que suma al filtro las guardas que leen el texto del plan) con `red-draft-2.txt`. **Historia:** faltaba el ayudante `NamedScan` (4 CS0103 en el archivo nuevo); corregido en `make_run_eq_v52.py`.
- **`/d/r6n`:** el mismo ensayo con `red-draft-3.txt` (`26734e73…cf42`) y el GREEN `4059860f…4932`. **RED:** Core y UI, 0 errores y 0 `warning CS`; 747 filas filtradas, solo cae CT-01. **GREEN:** Core, UI y Plugin, 0 y 0; 747 filas, 0 `Failed`. Las 27 (29 resultados: la 42 es una `Theory` de 3) pasan en las dos ramas. Registro `driver-r6n-qk.txt`.

## 4. Lo hecho y lo que falta

- **Paso 1 cerrado:** `v52/run/PushBackRetirePilotRunTests.cs` `e8b93c2e…c5c8` (con `NamedScan`), de `make_run_eq_v52.py` `a3f44e99…1d`.
- **Paso 2 cerrado:** `pbt4/pbt4d-red-v52.txt` `26734e73…cf42` (= `v52/try/red-draft-3.txt`; registro `v52/try/make_red-3.txt`) y `pbt4/pbt4d-retire-list-229.tsv` `1943468b…1fad` (242 filas, 229 con acción).
- **El censo de C13** (`pbt4dcc/compile/census_c13_v52.py` `60de77d1…aab3`, salida `census-c13-v52.txt` `5d649c7c…34cc`): 0 nuevas, 57 quitadas = la lista cerrada, 0 `allowErase`, las 4 autopruebas caen. PASA.
- **Cortadas por el cierre de la sesión (2026-11-21, noche), historia:** `/d/r6q` (el control de las CT: su registro llegó al final, con lo previsto: (a) base con solo las dos CT, caen las dos; (b) RED con la retirada A en `src/`, cae CT-01 por `retirado PushBackPilotComposition=1` y pasa CT-02) y `/d/r6p` (la cobertura de la base: solo la compilación). Se repiten en copias nuevas por orden de la sesión.
- **La corrida principal, `/d/r6r`, HECHA** (`driver-r6r-p6.txt`): las cinco compilaciones, 0 errores y 0 `warning CS`; RED Core 19 416 = 19 012 + 1 + 403 (331 s), solo cae CT-01; GREEN Core 19 416 = 19 013 + 0 + 403 (283 s); con cobertura, 0 `Failed` (593 s); UI, 18 filtros, 0 `Failed` en las dos ramas. **Los totales de UI de la CI** (`ui-totals-red.txt` y `ui-totals-green.txt`, frente a `/d/r5j`): 2 464 = 2 416 + 0 + 48 en las dos ramas (quitan 22: 21 Passed y 1 Skipped).
- **La precondición 6, CERRADA, todo lo previsto** (`pbt4dcc/compile/pbt4d-precondition6.json` `5bec3aca…f7e8`, `compile-check.md` `3cb03c7e…2316`):
  - `/d/r6s` (la base, JSON): 19 619 = 19 158 + 0 + 461, igual que la CI del cierre de PB-T4c;
  - **la cobertura** (`cov-p6-v52.txt` `f705b72b…102b`, `cov_p6_v52.py` `196eebe9…e619`): PASA. De las 436 líneas, 433 llegan al GREEN (4 con texto cambiado) y 3 son retiradas (600, 601 y el `memberName` viejo); 0 sin ejecutar y 0 ramas perdidas. Las 7 del alto, la 525 y la 594, iguales rama por rama; la línea nueva de `memberName` corre 407 veces, con sus 2 ramas;
  - `/d/r6t` (el control): base con solo las dos CT, caen las dos; RED con la variante A, cae CT-01 (`retirado PushBackPilotComposition=1`) y pasa CT-02;
  - `/d/r6u` (`p6d_ui19_v52.sh`): los 19 filtros de UI en la base, y `PushBackEditAllWindowTests` con el RED y con el GREEN (4 cada vez); 0 `Failed`, 0 avisos;
  - **los `RequiredTests`** (`required-pbt4d.tsv` `67fc6e73…7e76`): 83 heredados (salen el n.º 37, `PushBackFieldValidationConcordanceTests`, y el n.º 85, `PushBackEditAllFullCopyGuardTests`) + 5 nuevos = **88**; sin solapamientos. Bajan los n.º 5, 6, 12, 15, 26 y 50 de PB-T4c;
  - **los totales de la CI:** Core RED 19 416 = 19 012 + 1 + 403 y GREEN 19 416 = 19 013 + 0 + 403; UI 2 464 = 2 416 + 0 + 48 en las dos ramas;
  - la prueba lenta acotada: 16,8 s en el RED y 8,1 s en el GREEN.
- **`required_pbt4d_v52.py` corregido:** el `MinSelected` heredado es un suelo; baja en (cuenta en la base − cuenta en el GREEN). La base de Core, `r6s-bc/base.trx`; la de UI, `r6u-u9/base-ui*.trx`. La numeración de PB-T4c salta el n.º 43.
- **La v5.2, HECHA:** `pbt4/I-64-f4b-pbt4-gate-contract-package-v52.md` `466ab3db…f13e` (821 086 bytes), de `pbt4/make_v52.py` `f64d54c0…ef66` (registro `v52/make_v52-out.txt`, 27 reemplazos). Los `RequiredTests` finales (`required-pbt4d.tsv` `583108c2…889a`): 88; solo CT-PBT4D-01 con `ExpectRed` true (los heredados, todos false). El JSON final, `56bcba77…70b5`; `compile-check.md`, `9c4adedab…8934`. Sin rutas locales en los textos custodiados (este traspaso y los anteriores sí las llevan: la custodia los redacta).
- **Escritos para la v5.2:** `pbt4/make_v52.py`; los fragmentos `v52/frag/head.md`, `d10.md`, `s44.md` y `s64.md`, y `v52/frag_56.md`; y, en `pbt4dcc/compile/`, `p6_json_pbt4d_v52.py` (escribe el JSON y `compile-check.md`), `required_pbt4d_v52.py`, `ui_totals_v52.py`, `cov_p6_v52.py` y `cov_v52b.runsettings`.
- **Las órdenes que faltan, en orden** (desde `pbt4dcc/compile/`):
  1. `bash p6d_basecov_v52.sh /d/r6s` y `bash p6_control_v52.sh /d/r6t` (con `| tr -d '\r' > driver-<copia>.txt`);
  2. `python -I cov_p6_v52.py "<repo>" r6j-cv r6s-bc/base.coverage.json r6r-p6/core-green-cov.coverage.json ../../pbt4/pbt4d-green-v52.txt cov-p6-v52.txt`;
  3. `python -I required_pbt4d_v52.py ../../pbt4/v52/pbt4c-required-6c2820ba.tsv required-pbt4d.tsv required-pbt4d.txt RED_CORE=r6r-p6/core-red.trx RED_UI=… GREEN_CORE=r6r-p6/core-green.trx GREEN_UI=…` (los 18 `*-ui*.trx` de cada rama);
  4. `python -I ui_totals_v52.py ui-totals-red.txt red BASE=/d/r5j/_trx/g-ui-NN.trx… RAMA=r6r-p6/red-uiNN.trx…`, y lo mismo para `green`;
  5. `python -I p6_json_pbt4d_v52.py . r6r-p6 r6s-bc r6t-ct`;
  6. `python -I make_v52.py "<repo>"` (desde `pbt4/`).
- **Falta:**
  1. La corrida principal en `/d/r6r` (`p6_pbt4d_v52.sh`), sola: RED (suite de Core y 18 filtros de UI), GREEN (lo mismo y el Plugin) y la cobertura del GREEN con JSON.
  2. `cov_p6_v52.py` (r6j, `r6s-bc/base.coverage.json`, `r6r-p6/core-green-cov.coverage.json`): si una línea o una rama queda sin cubrir, PARA.
  3. `required_pbt4d_v52.py` sobre las TRX de `r6r-p6`: los `RequiredTests` y los totales. La línea base de la CI del cierre de PB-T4c (F4B-E55, GREEN `44959653142`): Core 19 619 = 19 158 + 0 + 461; UI 2 486 = 2 437 + 0 + 49. El total de UI de PB-T4d sale de la base menos lo que quitan las bajas de UI (por las TRX de los filtros, frente a `/d/r5j` `g-ui-*`).
  4. `pbt4d-precondition6.json` y `compile-check.md` en `pbt4dcc/compile/`.
  5. La v5.2 con `make_v52.py`.
