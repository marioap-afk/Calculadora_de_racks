> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v5.4

**Estado:** v5.4, **solo de texto**. Corrige la v5.3 de **PB-T4d** (`F4B-PBT4-RETIRE-PILOT`) tras la recomprobación 1 de CA-T4d (al final de `independent-review-pbt4d-check.md`, sha256 `76a10c8bde8a3c0532d0c7ffa1dda18d339882d22873fedcd0ecc2854b1b679f`: EMITIBLE, B 0 / I 0 / M 6, M-08 a M-13), sobre la misma base (`6c2820ba`). No se repite ninguna corrida de pruebas: los cambios del RED son solo de comentarios (el código sin comentarios es el de la v5.3) y los del GREEN, solo su cabecera; las piezas de la v5.4 se reaplicaron en una copia nueva (`/d/r70`), con 0 errores y 0 `warning CS`, y la cobertura se volvió a leer de los mismos JSON.

**Las decisiones:** sin decisiones nuevas del Owner. **La sesión:** D-S16 con su nota (`session-pbt4d-design-decisions.md`, sha256 `bd02d2103c7a671c51c1de8901bf76608e4944f98ad0fabcca1fdd253d3801c8`): cada entrada de «Bajan» nombra la corrida, no la prueba, y ESTABLE solo por la UI exige también un filtro en su proceso.

**Lo que cambia:** la cabecera (este registro) y la de la v5.3 (M-10); §4.4; §6.4.1 y §6.4.2; §7; §8.5; §11 y el anexo, que pasa a V54-A; las cifras de la lista en §0 y §8.3. **Lo que no cambia:** el resto de PB-T4d, PB-T4a, PB-T4b y PB-T4c.

**Lo que sale de la v5.4 (PB-T4d):** las piezas `pbt4d-red-v54.txt` y `pbt4d-green-v54.txt` (con los módulos y aplicadores de la v5.3, `--pieza`), `pbt4d_test_edits_v54.py`, `v54/make_red_v54.py`, `v54/make_list_v54.py` y la lista `pbt4d-retire-list-271.tsv`; `cov_p6_v54.py`, `p6_json_pbt4d_v54.py`, `post_p6_v54.sh` y `p6_reapply_v54.sh`; y `make_v54.py`, que arma esta versión. Lo de la v5.3, como historia.

---

## Registro de cambios v5.3 → v5.4

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| V54-01 | **Recomprobación 1 de CA-T4d M-08:** el JSON no fijaba por hash todas las entradas de la cobertura, y §6.4.2 no decía sus claves exactas | El JSON fija el sha256 de los JSON de UI del GREEN (`GreenUiCoverage`: las 3 corridas juntas y los 18 filtros, también en `Green.UI[n].Json`) y el completo de los 19 de la base (`BaseCoverage.UIByFilter.Runs`); la reaplicación de la v5.4 va en `Reapply`. §6.4.2 lista las claves exactas: el generador lee `RedDeleted` (comparada con los bloques DELETE) y no lee `Noise` (informativo). Sin correr pruebas | §6.4.2; `pbt4d-precondition6.json`; `compile-check.md` |
| V54-02 | **Recomprobación 1 de CA-T4d M-09:** [13] de §6.4.1 | «D-S11 a D-S15 y lo que la sesión fijó tras CA-T4d» pasa a «D-S11 a D-S16» | §6.4.1 ([13]) |
| V54-03 | **Recomprobación 1 de CA-T4d M-10:** «Lo que cambia» de la v5.3 no nombraba §0, §1, §6.0, §7 ni §9 | La cabecera de la v5.3 los nombra, y la columna «Dónde» de su registro también | La cabecera de la v5.3 y su registro |
| V54-04 | **Recomprobación 1 de CA-T4d M-11:** «Bajan» y la definición de ESTABLE | (a) Cada entrada nombra la corrida que la cubría, no la prueba (coverlet no da la prueba): se declara. (b) ESTABLE solo por la UI exige las tres corridas juntas **y** al menos un filtro en su proceso (`cov_p6_v54.py`). Vuelto a leer de los mismos JSON: entradas 16801: PERDIDA 0, VARIA 0, ESTABLE 16801; y, solo por la corrida de un filtro que no esta en el GREEN (RackCad.UI.Tests.PushBackFieldValidationConcordanceTests), 118 lineas o ramas en 3 archivos; igual que con la regla de la v5.3 | §4.4; `cov-p6-v54.txt` |
| V54-05 | **Recomprobación 1 de CA-T4d M-12:** el texto que quedaba | (a) WPUR en el RED: el resumen de la clase y los comentarios de la n.º 48 y la n.º 49, que se quedaban sin su prueba (dos ediciones con registro, solo comentarios): `pbt4d-red-v54.txt`. (b) La cabecera de la pieza del GREEN nombraba el módulo de la v5.2: `pbt4d-green-v54.txt` (solo esa cabecera; los bloques, los de la v5.3). (c) El porqué de la fila de `WriteViolations` (PEG:906), el de su edición de M-04. (d) La directiva `using` de Draft/ que el RED añade en `WorkspaceDraftPilotIntentStaleBaseTests.cs` tiene su fila (grupo `directiva`): la lista pasa a `pbt4d-retire-list-271.tsv` | §6.4; la lista; el anexo |
| V54-06 | **Recomprobación 1 de CA-T4d M-13:** el conjunto vigilado de 436 a 537 líneas | Una frase: desde la v5.3 entran también las líneas de WPUR dentro de los tramos que el GREEN borra enteros, que la v5.2 quitaba antes de contar | §4.4 |
| V54-07 | La versión y la reaplicación | El `Objective` cita la v5.4. Las piezas de la v5.4, reaplicadas en `/d/r70` (`p6_reapply_v54.sh`): 5 compilaciones con 0 errores y 0 `warning CS`; frente a la copia principal de la v5.3 (`/d/r6y`), el texto de `tests/` solo cambia en WPUR, su código sin comentarios no cambia y `src/` es igual | §6.4.1; §11 |

---

## La cabecera de la v5.3, sin cambio salvo lo que dice el registro v5.3 → v5.4

**Estado:** v5.3. Corrige la v5.2 de **PB-T4d** (`F4B-PBT4-RETIRE-PILOT`) tras su comprobación acotada (CA-T4d, `independent-review-pbt4d-check.md`, sha256 `439837f9b31d5cfae625ec8614fe33d921a16b339faf3959c04a92d5748dced3`: EMITIBLE TRAS CORRECCIONES, B 0 / I 2 / M 7), sobre la misma base, el **cierre de PB-T4c** (`6c2820ba`, F4B-E55). La precondición 6 se repitió entera en copias exportadas nuevas (`/d/r6v` en adelante; `pbt4dcc/compile/`).

**Las decisiones que la fijan, además de las de la v5.2:**
- **el Owner,** `owner-pbt4d-cat4d-decisions.md` (sha256 `fa07db3831fcf604fd6f95f3e461af7888bec2607196174c0bec49f144a03fa1`): «Sustituirlas» (la n.º 48 y la n.º 49 de WPUR pasan a sustitutas: 29) y «Sustituirlos siempre» (la cobertura de la precondición 6 mira todo `src/` que se queda; todo camino que se queda y pierde su única prueba recibe una sustituta sobre el modelo completo, sin volver al Owner, salvo que una no se pueda escribir o salgan más de 10 nuevas sobre las 29);
- **la sesión,** en su orden para la v5.3: M-01 a M-07 de la comprobación acotada, con la regla del Owner donde toca; y **D-S16** (`session-pbt4d-design-decisions.md`, sha256 `27010f8fdb5b9a51564cefad6804d498e1f143ab24458bf2f477579b34aea52e`): la cobertura de la UI como unión de corridas, y la lista de lo que baja.

**Lo que cambia:** PB-T4d: §0, §1, D-PBT4-10, §4.4, §5.6, §6.0, §6.4 (con §6.4.1 y §6.4.2), §7, §8.3, §8.5, §9, §11 y el anexo, que pasa a V53-A (§0, §1, §6.0, §7 y §9, nombrados desde la v5.4: recomprobación 1 de CA-T4d, M-10). **Lo que no cambia:** PB-T4a, PB-T4b y PB-T4c, D-S1 a D-S15 y el anexo V51-A. **Nueva:** D-S16 de la sesión (la UI de la cobertura como unión de corridas, y la lista de lo que baja).

**Lo que sale de la v5.3 (PB-T4d):**
- las dos piezas literales, `pbt4d-green-v53.txt` y `pbt4d-red-v53.txt`, y sus módulos y aplicadores (`green_pbt4d_v53.py`, `apply_green_pbt4d_v53.py`, `red_pbt4d_v53.py` y `apply_red_pbt4d_v53.py`; los de la RED leen además bloques DELETE);
- sus generadores (`v53/make_green_v53.py`, `v53/make_red_v53.py`, `v53/unused_v53.py`), las ediciones como datos (`pbt4d_test_edits_v53.py`), las 29 sustitutas (`v53/make_run_eq_v53.py`) y la lista (`v53/make_list_v53.py`); los guiones de la v5.2 que se llaman sin cambio (`v52/retire_pbt4d_v52.py`, `v52/make_fullcopy_eq_v52.py`);
- la precondición 6 (`pbt4dcc/compile/`: `p6_pbt4d_v53.sh`, `p6d_basecov_v53.sh`, `p6_control_v53.sh`, `cov_p6_v53.py`, `required_pbt4d_v53.py`, `p6_json_pbt4d_v53.py`, y los de la v5.2 que no cambian, `census_c13_v52.py` y `ui_totals_v52.py`); las salidas de la v5.2 quedan como historia, con el sufijo `-v52`;
- y `make_v53.py`, que arma esta versión desde la v5.2.

---

## Registro de cambios v5.2 → v5.3

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V53-01** | **CA-T4d I-01:** dos bajas de WPUR dejaban sin prueba código que se queda | La n.º 48 (`TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan`) y la n.º 49 (`TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope`) no prueban la corrida, sino `PushBackPilotSystemRebuild`, que se queda; eran las únicas de su línea 32 y de 6 ramas. El Owner: «Sustituirlas» (`owner-pbt4d-cat4d-decisions.md`, pregunta 1). Pasan a `PushBackRetirePilotRunTests` sobre el plan del PREPARE completo (`FullUpdateSetup`): **29 sustitutas, 31 resultados**. Sus filas, «sustituir con registro», con su porqué verdadero | §0; §1; D-PBT4-10; §4.4; §5.6; §6.4; la lista; el anexo |
| **V53-02** | **CA-T4d I-02:** el universo de la cobertura era más estrecho que lo pedido | La precondición 6 compara la cobertura de toda la suite de Core y de la unión de los filtros de UI, en la base y en el GREEN, sobre todo `src/` que se queda (Domain, Application y UI), línea a línea y rama por rama, con el JSON de coverlet (`cov_p6_v53.py`); la UI de cada rama, la unión de tres corridas de la unión de los filtros y de cada filtro en su proceso, porque un binding de WPF da golpes que dependen de lo que corrió antes (R-T4-38). La regla del Owner (pregunta 2). Reescritos INV-PBT4D-06, PBT4D-C16, el paso 7 y «Lo que la medida no ve» de §4.4 (§1: D-S16; §7: R-T4-34 y R-T4-38). **Lo medido:** archivos de src/ instrumentados 775; lineas ejecutadas en la base que se quedan 69842 (ramas cubiertas 28818); retiradas por el GREEN 419; texto cambiado 4; lineas sin ejecutar en el GREEN 0; lineas con ramas perdidas 0; **PASA** (`cov-p6-v53.txt`); conjunto vigilado de la v5.2: 537 lineas de WPUR, 433 se quedan, 0 sin cubrir en el GREEN; bajan (informativo) 16801, de ellas PERDIDA 0 | §4.4; §6.4.1 |
| V53-03 | **CA-T4d M-01:** el corpus de CT-PBT3B-06 (iii) | (iii) pasaba 15 diseños del corpus por `BuildCore`, el ensamblador y la barrera, y era la única prueba de que la barrera no bloquea diseños válidos. Con la regla del Owner, se queda en su sitio, sobre el modelo completo: los 15 diseños, como borradores completos (`FullWriteKit`), por `PrepareFull`, y dan `Ready`; sus líneas esperadas son las mismas. Sale solo (ii), del piloto | §6.4; la lista (CT-PBT3B-06); el anexo |
| V53-04 | **CA-T4d M-02:** la rama de `MirrorSelection` con el lado activo B | Medida con la cobertura de la unión de los filtros de UI, como pide la regla: Core sola pierde la rama del lado activo B de `PushBackCompositeEditorState.cs:233` (en la base la cubría el `Copy` del comparador del piloto, en pruebas que se borran), pero la UI de los filtros la cubre en la base y en el GREEN: en «Bajan», ESTABLE (GREEN: las tres corridas de todos los filtros juntos la cubren). No queda sin prueba: sin sustituta | §4.4; §6.4.1 |
| V53-05 | **CA-T4d M-03:** ayudantes sin uso tras la retirada | Medidos en el árbol del RED (IDE0051/IDE0052 en una copia, y por texto), en vueltas hasta que no sale ninguno: **32 ayudantes** «borrar con registro (ayudante sin uso tras la retirada)», y **3 clases que quedaban sin pruebas** (`PushBackFieldValidationConcordanceTests`, `PushBackDeclaredCorrectionPilotParityTests` y `WorkspaceDraftPilotCanonicalLineEndingTests`, de UI), que se borran enteras: la pieza del RED gana bloques DELETE. No queda código muerto en `tests/` | §4.4; §6.0 (RR-2); §6.4; §7 (R-T4-36); la lista (grupo `M-03`) |
| V53-06 | **CA-T4d M-04:** la lista no describía, fila a fila, lo que edita el RED | Dos columnas nuevas, `declaraciones-6c2820ba` (las sobrecargas) y `tramo-editado` (medido miembro a miembro contra el árbol del RED); **6 filas** `ayudante-editado` para los ayudantes que el RED edita sin fila propia (`E3Violations`, `E5Violations`, `HostViolations`, `WriteViolations` de WPEG:906, `AfterMatchViolations` y `Kinds`); y los porqué de C-7 dicen «borrar», no «sustituir». `make_list_v53.py` comprueba que toda línea de código que el RED cambia cae en una fila | §4.4; la lista |
| V53-07 | **CA-T4d M-05:** comentarios del GREEN y «tres ramas» | (a) El GREEN gana 11 bloques de solo comentarios, al final: el resumen de `PushBackPilotMaterializer`, el de la corrida, el de `ExecuteFull`, el de `ComparePostFull`, el del planificador y el de `PrepareFull`, y los de `ErasedKeys`, `Erasures`, `Members` y `PushBackPilotMemberPlan`, que describían el piloto como presente; el código, sin comentarios, es el mismo. (b) «la condición allowErase de sus tres ramas» pasa a «cuatro condiciones: tres en el bucle y la de `AllObsolete`» | D-PBT4-10; §6.4.1 (C13); el anexo |
| V53-08 | **CA-T4d M-06:** el número de la clase y la errata de la pregunta | El comentario de `PushBackRetirePilotRunTests` dice el número real (29 sustitutas; de las otras 20, 19 se borran y una se queda). La errata de la pregunta del Owner de `owner-pbt4d-deadcode-27-decisions.md` («se borran 22», que eran 21) se declara en §8.5 | §5.6; §8.5; el anexo |
| V53-09 | **CA-T4d M-07:** la copia principal no corrió sola ni con los 18 filtros del contrato | La copia principal corre sola (su conductor lo comprueba al empezar y lo registra al acabar) y con los 18 filtros de UI de `RequiredTests` de PB-T4d (`ui_filters_pbt4d_v53.txt`). Lo de `/d/r6r` (v5.2), declarado en `compile-check.md` | §6.4.1; §7 (R-T4-37); `compile-check.md` |
| V53-10 | La precondición 6, entera otra vez | En copias nuevas: los ensayos (`/d/r6v`, `/d/r6x`), la base con cobertura (`/d/r6w`), la principal (`/d/r6y`), el control de las CT (`/d/r6z`), el censo de C13, la cobertura ampliada, los `RequiredTests` y los totales. `Package` `v5.3` | §6.4.1; §9; §11 |
| V53-11 | La versión | El `Objective` cita la v5.3 | §6.4.1 |

---

## La cabecera de la v5.2, sin cambio salvo lo que dice el registro v5.2 → v5.3

**Estado:** v5.2. Escribe **PB-T4d** (`F4B-PBT4-RETIRE-PILOT`, la retirada del código del piloto) en forma literal, como la v5.0 y la v5.1 lo hicieron con PB-T4c, sobre el **cierre de PB-T4c** (`6c2820ba`, F4B-E55: CERRADA, intento 1 de 3, sin reparación del RED). La remedición se hizo por objetos git (`v52/remeasure_pbt4d_v52.py`) y compilando y ejecutando en copias exportadas de `6c2820ba` (`/d/r6a` en adelante; `pbt4dcc/compile/`).

**Las decisiones que la fijan:**
- **El Owner, en cuatro registros sobre PB-T4d:**
  - `owner-pbt4d-list-decisions.md` (sha256 `40a52fea22c441c436758d6e3826ef03d938827465d771eb3ab350ba67472962`): el alcance B, la recarga de «Editar todo» a una tarea propia, la prueba lenta acotada en PB-T4d y la emisión condicional;
  - `owner-pbt4d-substitutes-decision.md` (`12a41c3f509de5fc759e1733d0d0b555087c9d1d2fa8bc6436296baa09440d75`): «Sustitutas donde hace falta»;
  - `owner-pbt4d-run-coverage-decision.md` (`5c852b6a8e99eba0a000026adf25a27f718548e19bded942b9a7336ee3120c2f`): «Sustituir las 21», con la cobertura medida otra vez;
  - `owner-pbt4d-deadcode-27-decisions.md` (`61b0c77d20b8130751706e43c39542a5e849abe6bf936ab96f9363d9146fc853`): «Quitarla en PB-T4d», la rama muerta de `BuildCore`, y «Sí, 27», las sustitutas de la corrida.
- **La sesión:** D-S11 a D-S15 (`session-pbt4d-design-decisions.md`, sha256 `c5ddb9370cd73cb74540d7ecdc227dd6063ef55d91cb444fca0b9d45e12a80f2`): el RED con todo lo de `tests/` y dos CT, los `Skip` con su equivalente y el prefijo `PushBackRetirePilot`, M-04 en su sitio, la prueba lenta acotada y la limpieza de los comentarios.

**Lo que cambia:** PB-T4d entera:
- D-PBT4-10;
- §0, §1, §4.4 (nueva), §5.4 y §5.6 (nueva);
- §6.0 (sus filas de PB-T4d) y §6.4, con §6.4.1 y §6.4.2;
- §7, §8.3, §8.5, §9 y §11;
- y el anexo V52-A, con el texto literal del GREEN, de las ediciones con registro, de los `Skip` y de los archivos nuevos.

**Lo que no cambia:** PB-T4a, PB-T4b y PB-T4c (sus §5.1 a §5.3 y §6.1 a §6.3, enteros), D-S1 a D-S10 y el anexo V51-A.

**Lo que sale de la v5.2 (PB-T4d):**
- las dos piezas literales, `pbt4d-green-v52.txt` y `pbt4d-red-v52.txt`, y sus módulos y aplicadores (`green_pbt4d_v52.py`, `apply_green_pbt4d_v52.py`, `red_pbt4d_v52.py` y `apply_red_pbt4d_v52.py`);
- sus generadores (`v52/make_green_v52.py` y `v52/make_red_v52.py`), las ediciones como datos (`pbt4d_test_edits_v52.py`), las 27 sustitutas (`v52/make_run_eq_v52.py`), el equivalente de CT-PBT4B-20 (`v52/make_fullcopy_eq_v52.py`) y la lista (`v52/make_list_v52.py`);
- las medidas de la remedición, de la necesidad y de la cobertura (`v52/` y `pbt4dcc/compile/`), y el alto del relevo (`v52/sub/`);
- la precondición 6 (`pbt4dcc/compile/`: `p6_pbt4d_v52.sh`, `p6d_basecov_v52.sh`, `p6_control_v52.sh`, `cov_p6_v52.py`, `census_c13_v52.py`, `required_pbt4d_v52.py`, `ui_totals_v52.py` y `p6_json_pbt4d_v52.py`);
- y `make_v52.py`, que arma esta versión desde la v5.1.

---

## Registro de cambios v5.1 → v5.2

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V52-01** | **PB-T4d vuelve al Owner con su lista** (`owner-pbt4b-retire-list-decisions.md`, pregunta 1) | La lista, vuelta a medir sobre el cierre de PB-T4c, por texto y compilando y ejecutando: dos alcances (A y B); el Owner elige **B** y la emisión condicional (`owner-pbt4d-list-decisions.md`). La recarga de «Editar todo», fuera | D-PBT4-10; §0; §4.4; §6.4 |
| **V52-02** | **Las 138 usaban el piloto como vehículo de código que se queda** | Medido desde cero bajas en `/d/r6h`: 201 de las 235 bajas de B hacen falta y 34 no. El Owner: «Sustitutas donde hace falta» (`owner-pbt4d-substitutes-decision.md`): se quedan 7 CT de PB-T3b y sus 6 ayudantes; lo que también prueba código que se queda conserva esa parte, como edición con registro; lo demás se borra con registro | §4.4; §6.4.1; la lista |
| **V52-03** | **La corrida genérica que solo cubría WPUR** | Medida con coverlet (`/d/r6j`, `/d/r6k`): 436 líneas de la corrida que se queda, 35 solo de WPUR. El Owner: «Sustituir las 21». Al preparar las sustitutas, el relevo midió rama por rama (`v52/sub/cov-sides.txt`): **7 ramas más** solo las cubrían otras 6 pruebas. El Owner: **«Sí, 27»** (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 2): `PushBackRetirePilotRunTests`, sobre `ExecuteFull`, con `FullUpdateSetup` | §4.4; §6.4; el anexo V52-A |
| **V52-04** | **Código muerto tras la retirada** | Sin el `Build` del piloto, `BuildCore` solo lo llama `BuildFull` con `allowErase` true, y la rama de la vista no disponible no se alcanza. El Owner: **«Quitarla en PB-T4d»** (pregunta 1): el GREEN quita `allowErase` y lo que solo existe con él (la rama, sus dos constantes, el nombre de cada sobre releído). La n.º 10 de WPUR pasa a mano al `Invalid` de la frontal B de un rack sin lado B. E-3 de D-1 pierde sus reglas de `allowErase` y de `ViewUnavailable` (edición con registro); CT-01 y CT-02 cuentan lo nuevo retirado | D-PBT4-10; §5.6; §6.4.1; la pieza del GREEN |
| V52-05 | Las decisiones de la sesión | D-S11 (el RED con todo lo de `tests/`, sin esqueleto, y dos CT), D-S12 (tres `Skip` y sus equivalentes, prefijo `PushBackRetirePilot`, filtros por clase exacta), D-S13 (M-04 en su sitio), D-S14 (la prueba lenta, cada 97.º toque y el último) y D-S15 (los cuatro `cref` y el comentario) | §5.6; §6.4; §8.5 |
| V52-06 | La precondición 6 de PB-T4d | En copias nuevas (`/d/r6r` en adelante): el RED literal, el GREEN literal, la cobertura rama por rama frente a la base, el control de las CT, el censo de C13, los `RequiredTests` y los totales. Las copias anteriores, historia | §6.4.1; §9; §11 |
| V52-07 | Los textos literales | §6.4.1: `Objective`, `AllowedWriteScope` ruta a ruta, INV-PBT4D-01 a 14, PBT4D-C01 a C18, `ExpectedEvidence`, `RequiredTests` y los `Skip`; el anexo V52-A | §6.4; el anexo V52-A |
| V52-08 | La versión | El `Objective` cita la v5.2. `TaskId` `F4B-PBT4-RETIRE-PILOT` (21) | §6.4.1 |

---

## La cabecera de la v5.1, sin cambio salvo lo que dice el registro v5.1 → v5.2

**Estado:** v5.1. Corrige **PB-T4c** con la comprobación acotada **CA-T4c** (`independent-review-v1.md`, §22: EMITIBLE TRAS CORRECCIONES, **0/1/3**, sin decisiones nuevas del Owner):
- **I-01:** los dos archivos nuevos de `Draft/`, sin `#nullable enable` y sin el `?` de `ProjectOf`;
- **M-01 a M-03**;
- y **la lección:** la precondición 6 no corría las CT. Gana un paso, el 8: el ensayo de las CT de texto y el censo de C13.

La precondición 6 se repitió entera en copias nuevas (`/d/r5i` a `/d/r5l`), porque cambian el esqueleto y el GREEN.

**Lo que cambia:**
- las dos piezas (`pbt4c-green-v51.txt` y `pbt4c-skeleton-v51.txt`) y sus tres guiones (`green_pbt4c_v51.py`, `apply_red_pbt4c_v51.py` y `apply_green_pbt4c_v51.py`);
- el docstring de las CT como datos (`pbt4c_ct_table_v51.py`); las CT y sus líneas son las mismas;
- §5.3 (M-01), §6.3, §6.3.1 (M-02, el paso 8 y lo medido) y §6.3.2;
- §7, §9 y §11;
- y el anexo V51-A, que sustituye al V50-A.

**Lo que no cambia:** la lista de 4, los `Skip`, los equivalentes y su `EquivalentSha`, las CT y sus líneas, los `RequiredTests`, los totales, las autoridades (salvo la sección de las n.º 0 y 65), D-S7 a D-S10, PB-T4a, PB-T4b y PB-T4d.

**Lo que sale de la v5.1 (PB-T4c):**
- las piezas, desde las de la v5.0 (`v51/make_pieces_v51.py`), y los guiones, desde los de la v5.0 (`v51/make_tools_v51.py`);
- el ensayo de las CT de texto (`pbt4c-rehearsal-textct.cs`, que escribe `v51/make_rehearsal_v51.py` desde los objetos git de la base) y el censo de C13 (`pbt4ccc/compile/census_c13_v51.py`);
- el conductor de la precondición 6 (`p6_pbt4c_v51.sh`), sus lecturas del ensayo (`rehearsal_check_v51.py`) y de cada clase (`classes_v51.py`), el coste (`p6_isdirty_pbt4c_v51.sh`), el control del ensayo sobre la v5.0 (`p6_control50_v51.sh` y `control_check_v51.py`), el resumen (`p6_json_pbt4c_v51.py`) y su registro (`compile_check_v51.py`, que escribe `compile-check.md` con el resultado de cada clase);
- la lectura RR-3 sobre esta versión (`v51/check_rr3_v51.py`);
- y `make_v51.py`, que arma esta versión desde la v5.0.

---

## Registro de cambios v5.0 → v5.1

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V51-01** | **CA-T4c I-01** (importante): **CT-PBT4C-15 no podía pasar, ni en el RED ni en el GREEN.** `PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs` empezaban por `#nullable enable`, en el esqueleto y en el GREEN, y la pureza que CT-15 copia de CT-PBT3A-14 da `#nullable` en cuanto ve `^\s*#nullable` (`PushBackFullReadGuardTests.cs:293-296` y `PushBackDraftWriteGuardTests.cs:294-298` de `6adff90d`). La precondición 6 no corre las CT, y las sondas no cubrían la CT-15 | **Sin la directiva en los dos archivos,** en las dos piezas, **y sin el `?` de `RackProject? ProjectOf`**: sin la directiva daría CS8632, porque `Directory.Build.props:28` fija `Nullable` en `disable` (CA-T4a I-01; la regla del §5.0). Los demás archivos nuevos, repasados contra las reglas de las CT de texto que les aplican, con el ensayo del paso 8 (V51-05). No cambian CT-15, la lista, los totales ni los filtros | las dos piezas; el anexo V51-A; §6.3; §6.3.1 |
| V51-02 | CA-T4c M-01 | **De dónde sale cada línea de CT-03, 04, 05 y 12, línea a línea:** 22 tal cual de la sonda, 9 con los campos de la CT, 3 derivadas (y cómo) y 4 del texto del GREEN. **`lecturas-dentro`** son las lecturas durante `ShowModal`. Las corridas: `/d/r5f` (la válida de la v5.0) y `/d/r5i` (la de la v5.1), iguales por sha256. La cita de la vuelta parada (`/d/r5d`) sale | §5.3 |
| V51-03 | CA-T4c M-02 | **El nombre del equivalente de PEG:155-163** conserva «…SeventeenOperations…» aunque su tabla tenga dieciocho operaciones: es la copia literal; lo que fija es la tabla (como la §20 M-02) | §6.3.1 |
| V51-04 | CA-T4c M-03 | **El comentario del GREEN en `PendingWork()` de W** casa con D-S10: en WORKSPACE, un módulo escenificado deja siempre cambios sin aplicar, así que la pregunta del módulo sola es la de CLASSIC. Solo comentario | la pieza del GREEN; el anexo V51-A |
| **V51-05** | **La lección de I-01: la precondición 6 no corría las CT** | **El paso 8, nuevo:** el ensayo de las CT de texto sobre `src/` (CT-01, 07, 08, 13 y 15), en clases temporales con sus líneas literales de §5.3, sobre el RED y sobre el GREEN, con su control sobre las piezas de la v5.0 (que ve I-01), y el censo de C13 y de los nombres, por texto, con sus autopruebas. **Qué cubre y qué no,** declarado | §6.3.1; §7; §11 |
| V51-06 | La precondición 6, otra vez, pasos 1 a 9 | `/d/r5i` (el ensayo y las sondas), `/d/r5j` (el RED exacto y el GREEN literal), `/d/r5k` (el coste) y `/d/r5l` (el control del ensayo sobre la v5.0). Lo previsto, con D = 0. Las de la v5.0 (`/d/r5f`, `/d/r5g` y `/d/r5h`) pasan a historia | §5.3; §6.3.1; §9; §11 |
| V51-07 | La custodia de `compile-check.md` | **El resultado de cada clase que corre** (Passed, Failed y Skipped), también de las que la custodia solo lleva por sha256; las salidas nuevas, con nombres cortos: ninguna ruta pasa de 150 caracteres con el prefijo de `cc4c/` | §6.3.2 |
| V51-08 | La versión | El `Objective` cita la v5.1. CA-T4c: la §22 y su recomprobación (§23). Los guiones y las piezas, por su nombre de la v5.1 en §6.3.2 | §6.3.1; §6.3.2 |

---

## La cabecera de la v5.0, sin cambio salvo lo que dice el registro v5.0 → v5.1


**Estado de la v5.0:** Escribe **PB-T4c** (`F4B-PBT4-EDIT-ALL-WINDOW`, «Editar todo») en forma literal, como la v4.4 a la v4.9 lo hicieron con PB-T4b, sobre el **cierre de PB-T4b** (`6adff90d`, F4B-E53: CERRADA, intento 1 de 3; el GREEN `c065080d` es el texto literal de la v4.9). La remedición se hizo por objetos git (`v50/remeasure_pbt4c_v50.py`), y la precondición 6, en copias exportadas de `6adff90d` (`/d/r5a` en adelante; `pbt4ccc/compile/`).

**Las decisiones que la fijan:**
- **El Owner, dos veces, sobre la lista de PB-T4c:** la de 2 que vio (`owner-pbt4-decisions.md`, pregunta 4) pasó a 3 con CT-PBT4B-20 («Añadirla a la lista (Recomendado)», `owner-pbt4c-list-3-decision.md`, sha256 `a98537acd2af674de4147eb3edc94f6483fa5db38fdaf692072c4a7395d7ed13`) y a **4** con CT-PBT2B-06 (`owner-pbt4c-list-4-decision.md`, sha256 `e98470114a04d8c7bb6102ebe66e9d106c8950e8dc2604702d97caf4e286d31a`): **`pbt4c-substitution-list-4.tsv`**, sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de` (la de 3, `pbt4c-substitution-list-3.tsv`, `d83e0a817144fa99f95ed4bfd151b822a4fc1035c10f736f77e8f8e6350186c1`, queda como historia).
- **La sesión:** D-S7 a D-S10 (`session-pbt4c-design-decisions.md`, sha256 `d1912d978f03cab40fd88b93b9574170bee93841e43fdaca29b1e1512a4c3e83`): la costura de CT-09, los filtros por clase exacta, el nombre `IWorkspaceFullEditHost` y lo que cubre CT-09.

**Lo que cambia:** PB-T4c entera: D-PBT4-12 a 17, §4.3, §5.3, §6.0 (sus filas), §6.3 con §6.3.1 y §6.3.2, §8.3, §8.5, §9, §11 y el anexo V50-A, con el texto literal del GREEN, del esqueleto, de los `Skip` y de los equivalentes, y las líneas medidas de las CT.

**Lo que no cambia:** PB-T4a y PB-T4b (sus §5.1, §5.2, §6.1 y §6.2, enteros), PB-T4d (§6.4) y D-S1 a D-S6.

**Lo que salió de la v5.0 (PB-T4c):**
- las piezas literales `pbt4c-green-v50.txt` y `pbt4c-skeleton-v50.txt` y su generador, `green_pbt4c_v50.py`;
- los `Skip` (`pbt4c_skips_v50.py`), los equivalentes (`make_equivalents_pbt4c_v50.py`) y su comprobación (`check_skips_pbt4c_v50.py`);
- el RED y el GREEN en una copia (`apply_red_pbt4c_v50.py` y `apply_green_pbt4c_v50.py`);
- la lista (`make_list_pbt4c_4.py`), las CT como datos (`pbt4c_ct_table_v50.py`), el paseo de guardas (`walk_guards_pbt4c_v50.py`) y las sondas;
- y `make_v50.py`, que arma esta versión desde la v4.9.

---

## Registro de cambios v4.9 → v5.0

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V50-01** | **La remedición sobre `6adff90d`** (por objetos git; `v50/remeasure_pbt4c_v50-out.txt`): ningún anclaje de PB-T4c cambió de texto salvo los que tocó PB-T4b. **Citas de la v4.9 corregidas:** «archivos=14» está en **:384** (no :381); WHS 398-421, EIG 1011-1024 y 1155-1184, WNH 713-723, WBS 361-366, URT 1108-1109, `ControllerViolations` 659-707, EIH:1060 y :1079. **Movido por PB-T4b** (+38 en WHC): «two-phase» en :1187 (era :1149), `TryBeginEdit` 1058-1070, `RequestUpdate` 1414-1473 (la marca en :1426 y `PassGate` en :1430), `PassGate` :1959, `UpdateGate` :1023; `TryKeepPilotEdit` (652-751) y `SetPendingText` cambiaron. **La fila «PEG:380-385, verde» de §4.3 era obsoleta:** PEG:379-386 está en la lista de 478 y su copia (`WorkspacePilotEditGuardFull.cs:266-272`, fijada por `CopySha`) exige las dos interfaces de WHC | D-PBT4-12 a 17; §4.3 |
| **V50-02** | **La lista, de 2 a 4,** con dos decisiones del Owner: **CT-PBT4B-20** (el `Skip` de PEG deja el archivo distinto de su `BaseSha`; medido en `/d/r5a` con solo los dos `Skip`) y **CT-PBT2B-06** (su ventana 4 fija que la pregunta del módulo «manda primero»: es lo que A-T2b-OWNER-01, F4B-E38, mandó corregir en PB-T4; la revisión v1 decía «Ninguna prueba fija hoy la pregunta con las dos cosas pendientes», y no era cierto) | §0; §4.3; §8.3; §8.5; la lista |
| **V50-03** | **El GREEN y el esqueleto, literales** (anexo V50-A), aplicados por anclajes exactos sobre `6adff90d`: 9 archivos existentes y 4 nuevos; el RED, 4 existentes y 4 nuevos. Compilan con 0 errores y 0 `warning CS`, el Plugin incluido | D-PBT4-12 a 17; §6.3.1; anexo V50-A |
| **V50-04** | **Los cuatro `Skip` y los cuatro equivalentes,** literales, que genera el guion desde los originales, con un cambio declarado cada uno. **El de CT-PBT2B-06 falla en el RED** (la ventana del esqueleto no cambia) y pasa en el GREEN, así que su filtro va con `ExpectRed` true | §5.3; §6.3.1; anexo V50-A |
| **V50-05** | **La tabla entera de las CT,** CT-01 a 15, con su clase y sus líneas literales, para la lectura RR-3 de la cadena (la v4.9 solo traía la de la CT-15 y remitía a la v3) | §5.3 |
| **V50-06** | **Las filas de PB-T4c de la tabla de RR-3,** una por clase, cada una con su clase y sus CT, como la de CT-19 de PB-T4b; la de CT-08 nombra su clase (`PushBackEditAllGuardTests`) | §6.0 |
| **V50-07** | **Los totales, en cifras** (F4B-E46), sin fórmulas: GREEN Core 19 619 = 19 158 + 0 + 461 y UI 2 486 = 2 437 + 0 + 49; RED Core 19 619 = 19 149 + 9 + 461 y UI 2 486 = 2 432 + 5 + 49 | §5.3; §6.3.1 [14] |
| **V50-08** | **D-S7:** CT-09 escenifica el módulo con `HeaderConfiguratorDialog` (no con `HeaderConfiguratorPresenter`, que construiría un `RackFrameConfiguratorWindow` real: C05) | D-PBT4-17; CT-09 |
| **V50-09** | **D-S8:** los filtros nuevos, por clase exacta; ninguno selecciona una clase con el otro `ExpectRed`, y ningún filtro heredado selecciona una clase nueva | §6.3.1 |
| **V50-10** | **Las restricciones medidas que el GREEN cumple:** WHC con sus dos interfaces; los nombres prohibidos de PEG:1015; el comentario de PEG:1079; `EditAllOutcome` fuera del censo de portadores; los 30 textos de WHHG; los límites de WCo | D-PBT4-13; §4.3 |
| **V50-11** | **D-S9:** `IWorkspaceFullEditHost` (y `SetFullEditHost`, `_fullEditHost`), porque `WorkspaceModelBoundaryTests:101` prohíbe «Editor» en los tipos de `RackCad.Application.Workspace` y el paquete nombraba `IWorkspaceFullEditorHost` desde la v4 (medido en `/d/r5c`, y el arreglo en `/d/r5e`) | D-PBT4-13 y 15; anexo V50-A |
| **V50-12** | **El paseo de guardas** sobre el GREEN, con el método de la v4.8 (las tuplas) y las pruebas de `6adff90d`, que incluyen las de PB-T4b. **Lo que no ve, declarado:** la reflexión sobre tipos (la caída de V50-11) y el comportamiento de la UI (la de CT-PBT2B-06). Lo que vale es la suite completa de Core y los filtros de UI de la precondición 6 | §4.3; §9 |
| **V50-13** | **Las dudas del generador** (`generator-doubts-pbt4c.md`): el contrato literal, sus piezas por sha256, la precondición 6 y su resumen, y CA-T4c | §6.3.1; §6.3.2 |
| **V50-14** | **Cuándo se ofrece «Editar todo»:** con las condiciones de «Editar» salvo la de solo lectura (si no, desaparecería con el borrador abierto) | D-PBT4-12 |
| **V50-15** | **K = 1, medido** con la sonda (CT-05): las 17 operaciones con su respuesta y su motivo | D-PBT4-14; CT-05 |
| **V50-16** | **Los filtros heredados:** el n.º 12 (`RackCad.Tests.Workspace`) **sí selecciona** PEG:155-163 y CT-PBT4B-20 (2 718 → 2 716); el n.º 7, de 2 a 1; el n.º 36 (UI), de 4 a 3; **el n.º 43 queda en 0 y su fila sale**; `MinSelected` cuenta resultados (medido en las TRX) | §6.3.1 |
| **V50-17** | **CT-09 y «solo un módulo escenificado» (D-S10):** en WORKSPACE no se puede alcanzar. Escenificar un módulo deja siempre cambios sin aplicar, porque el estado editable lleva la autoridad de módulos (W:177; la sonda de UI en `/d/r5f`). CT-09 cubre `ambas`, `solo-sin-aplicar`, `ninguna` y `clasico-modulo` | D-PBT4-17; CT-09 |
| **V50-18** | **Por qué un equivalente cae en el RED:** el de CT-PBT2B-06 fija la pregunta combinada, que solo trae el GREEN (W no cambia en el esqueleto, o CT-09 pasaría en el RED). Los otros tres son de guardas que el RED ya cumple. En PB-T4b, la mayoría de las copias pasaban en el RED porque fijaban lo que PB-T4b no cambia | §5.3 |

## La cabecera de la v4.9, sin cambio salvo lo que dice el registro v4.9 → v5.0

**Estado de la v4.9:** Corrige la parte de **PB-T4b** con la comprobación acotada **CA-T4b** (`independent-review-v1.md`, §20: EMITIBLE TRAS CORRECCIONES, 0/1/8) y con la **séptima tanda del Owner**, el coste de la lectura (`owner-pbt4b-cost-decision.md`, sha256 `d0f4ca259f26fbf91676538d0895eca7acf921b76d5c56eb47885965213f5576`: «Darlo por cumplido (Recomendado)»). Los registros de la sesión van en `session-pbt4b-design-decisions.md` (sha256 `8b18add602766c705260e976e3566b151e13567ee3f2b40a0c0ff12bd193e934`, con M-03 a M-06 y la corrección de I-01 de la CA-T4b).

**Lo que cambia:**
- **CA-T4b I-01** (V49-01): el equivalente de WHHG `NoneOfThe30…` trae, tras su `Assert.Empty`, **la autoprueba de la guarda** (WHHG:501-526: sus dos variables y sus 13 `Assert.NotEmpty`, literales), sobre su propio `LiteralViolations` y su tabla con 0, 6 y 0. Es lo que hace el equivalente de G-4 con la autoprueba de IG:510-517.
  - **Por qué:** la autoprueba original pasa en vacío en el GREEN. Su tabla (2, 5 y 1) ya no casa con el anfitrión, así que `LiteralViolations` sin mutar ya da tres violaciones y sus `NotEmpty` se cumplen con cualquier mutación.
  - **Lo que cambia:** solo el `CopySha` de `WorkspaceHostHardeningGuardFull.cs`. No cambian la lista, los totales ni la partición;
- **M-01 a M-04 de la CA-T4b** (V49-02 a V49-05), de texto;
- **el coste de la lectura** (V49-06): el Owner lo da por cumplido con dos de tres corridas de la v4.8. **La v4.9 conserva esas corridas,** porque no cambia ninguna de sus entradas (comprobado por sha256 en la precondición 6).

**Lo demás no cambia:**
- la lista de 478, el mapa v5 y el GREEN;
- el esqueleto y el kit;
- D-S1 a D-S6;
- PB-T4a.

**La precondición 6 repite los pasos 1 a 6** con los guiones de la v4.9, en copias nuevas (`/d/r4u` en adelante). Sus resultados van en `pbt4bcc/compile/`.

**Lo que salió de la v4.9 (PB-T4b):**
- las ediciones (`pbt4b_hand_edits_v49.py`);
- el guion de la copia (`make_copies_pbt4b_v49.py`) y su revisión (`pbt4b-equivalents-review-v49.txt`, 114 entradas);
- el RED (`apply_red_pbt4b_v49.py`);
- la autoprueba (`selftest_pbt4b_v49.py`, `v49/selftest_pbt4b_v49-out.txt`);
- y `make_v49.py`, que arma esta versión desde la v4.8.

Los `Skip`, la partición y las comprobaciones son los de la v4.8.

---

## Registro de cambios v4.8 → v4.9

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V49-01** | **CA-T4b I-01** (importante): la autoprueba `TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier` (WHHG:501-526) **pasa en vacío en el GREEN**. La v4.8 daba otra razón («es una autoprueba que inyecta textos sobre el anfitrión y pasa», §4.2) | **Como G-4:** el equivalente de `NoneOfThe30…` (`WorkspaceHostHardeningGuardFull`) trae, tras su `Assert.Empty`, las 13 `Assert.NotEmpty` de la autoprueba, con sus dos variables (`host` y `support`), sobre su `LiteralViolations` y su tabla con 0, 6 y 0. Su texto literal va en el anexo V49-A. **La CA-T4b (§20.10) dice «las 14 `NotEmpty`»: en WHHG:501-526 de `78a7cef3` son 13, y van todas** (`selftest_pbt4b_v49.py` las compara, línea a línea, con el objeto git). §4.2 declara el vacío de la original y corrige la razón. No cambian la lista, los totales ni la partición: en el RED, el `Assert.Empty` cae antes (Red, 1 resultado). Cambia el `CopySha` de esa copia | §4.2; D-PBT4-11; el anexo V49-A |
| V49-02 | CA-T4b M-01 | Declarado en §4.2: las autopruebas de `HostViolations` de PEG que quedan fuera de la lista (PEG:401-416, :418-431, :433-451, :453-468, :470-492 y :516-526; 6 métodos, 31 resultados) **también pasan en vacío en el GREEN**, con la mitigación de la §20.10 | §4.2 |
| V49-03 | CA-T4b M-02 | **Los nombres de las dos copias de WPP de D-S1** («…NeverErasesAView…», «…MakesTheUpdateInvalid…») dicen lo contrario de lo que afirman. Se conservan por el mecanismo (CT-20 (2) y la partición), y **la regla es la del cuerpo**. Va en V48-02 y en §8.3 | V48-02; §8.3 |
| V49-04 | CA-T4b M-03 | En §4.2, WCAG: `ReasonTable` es WCAG:408-419, y las filas que cuenta el paseo son :415-419 (la v4.8 citaba :408 y :447) | §4.2 |
| V49-05 | CA-T4b M-04 | **«`AtOrigin`, NH, `ReadAt` y WHC no cambian»** quiere decir que **D-S4 no los toca** (D-PBT4-21): WHC sí cambia en el GREEN de PB-T4b, por D-PBT4-04 y D-PBT4-08. Corregido en D-PBT4-03. La fila V45-02 de la cabecera de la v4.5 queda como estaba, con esta aclaración | D-PBT4-03; V45-02 (historia) |
| V49-06 | **La séptima tanda del Owner: el coste de la lectura** | «Darlo por cumplido (Recomendado)» (`owner-pbt4b-cost-decision.md`): dos de tres corridas de la sonda de la v4.8 cumplen todo. **La v4.9 conserva el paso 7 de la v4.8** (CT-06 en `/d/r4s` y el coste en `/d/r4s` y `/d/r4t`): la precondición 6 comprueba por sha256 que no cambia ninguna de sus entradas (el GREEN de `src/`, las sondas, el kit y las copias que usan). `pbt4b-precondition6.json` lleva el campo `Cost`, que remite a esas corridas y a la decisión | §6.2.1, paso 7; §8.5; §11 |
| V49-07 | La versión | El `Objective` de PB-T4b cita la v4.8 → **v4.9** y «sus siete tandas». La cabecera cita los registros vigentes (la v4.8 citaba un sha256 viejo del de la sesión, `f8518e78…`). **Las corridas de la v4.9:** `/d/r4u` en adelante | §6.2.1; §11 |

---

## La cabecera de la v4.8, sin cambio salvo lo que dice el registro v4.8 → v4.9

**Estado de la v4.8:** Corrige la parte de **PB-T4b** con lo que encontró la precondición 6 de la v4.7. La lista pasa a 478 por la **sexta tanda del Owner** (`owner-pbt4b-list-478-decision.md`, sha256 `a18afbb4c20320ed98a12202db6311b925bb1d9e406064f45d0ea661ee2affe7`); lo demás lo decide la sesión (`session-pbt4b-design-decisions.md`, sha256 `f8518e78aaffd64ebd16aa60c4775c9a1417182d0648e61cb43110edc9a53999`).

**Qué encontró la precondición 6 de la v4.7** (`/d/r4m`, la primera vez con la suite completa de Core):
- compiló sin errores ni avisos CS;
- el RED fue exacto salvo una prueba;
- **cayeron dos pruebas fuera de la lista y dos copias** (el análisis medido, en `v47/analysis-outside-477.md`).

**Lo que cambia por eso:**
- **D-S6, el espacio de nombres de las copias** (V48-01): las 21 copias van en el espacio de nombres raíz de su ensamblado (`RackCad.Tests` o `RackCad.UI.Tests`), como exige `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`, y sus clases llevan el prefijo `FullPanelCopy`: `FullPanelCopy<Clase>ProbeTests`, `…RedTests` y `…GreenTests`. Ningún filtro heredado las selecciona. Es el mecanismo que el paquete ya usa para las CT;
- **D-S1 en dos copias de WPP** (V48-02): `InvalidE_AMutableMember…` y `ThePilotNeverErasesAViewAPhantom…` fijaban que el piloto nunca borra una vista. Sus equivalentes afirman lo que ya fijan D-PB-04, D-PBT3-09 (e) y D-PBT4-07: el lateral fantasma, `Ready` con borrados [B2]; `side-b`, `Invalid` con `FrontalBOfOneWayPrefix`/`Suffix`; los dos descriptores, como hoy. **El reparto de C-0 pasa de 358/70 a 356/72**, con el mapa de la copia v5;
- **la sexta tanda del Owner, la lista de 478** (V48-03): `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept` entra en C-5, con su `Skip` y el equivalente `WorkspaceHostHardeningGuardFull`. Su tabla es la misma con tres cuentas nuevas: 2 → 0, 5 → 6 y 1 → 0, cada una con su `// before N`;
- **el paseo de guardas corregido** (V48-04): no reconocía los campos de tipo tupla. Corregido, marca tres pruebas más, que se declaran en §4.2;
- **las cifras que dependen de la lista** (V48-05), recalculadas.

**Lo demás no cambia:** el GREEN (`green_pbt4b_v45.py`), el esqueleto, el kit de la v4.5, las CT salvo CT-20, MU, las ediciones de ayudante, D-S4, D-S5 y PB-T4a. **La precondición 6 se corre otra vez entera** con los guiones de la v4.8, en copias nuevas (`/d/r4q` en adelante); sus resultados van en `pbt4bcc/compile/`.

**Lo que salió de la v4.8 (PB-T4b):**
- la lista de 478: `pbt4b-substitution-list-478.tsv`, sha256 `8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b`, generada por `make_list_478.py` (`v48/make_list_478-out.txt`);
- el mapa de la copia v5: `pbt4b-copy-map-v5.tsv`, sha256 `bda79bb7fb6e1dfe885cdc5942b2af45dfac8861663ccc99dbb38a61b8844524`, generado por `make_copy_map_v5.py`;
- las ediciones: `pbt4b_hand_edits_v48.py`;
- el guion de la copia y su revisión: `make_copies_pbt4b_v48.py` y `pbt4b-equivalents-review-v48.txt`, con 114 entradas;
- los `Skip`, el RED y su comprobación: `pbt4b_skips_v48.py`, `apply_red_pbt4b_v48.py` y `check_skips_pbt4b_v48.py`;
- la partición: `partition_pbt4b_v48.py`;
- el paseo y su comparación: `walk_guards_pbt4b_v48.py` y `compare_walks_v48.py`, con sus salidas en `v48/`;
- la autoprueba: `selftest_pbt4b_v48.py`, `v48/selftest_pbt4b_v48-out.txt`;
- y `make_v48.py`, que arma esta versión desde la v4.7.

---

## Registro de cambios v4.7 → v4.8

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V48-01** | **D-S6: `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace` cae en el RED y en el GREEN** (`/d/r4m`). Las 20 copias declaraban `…FullPanelCopies`, y la guarda (la excepción de I-23) exige el espacio raíz del ensamblado | **Las copias, en el espacio raíz, con el prefijo `FullPanelCopy` en la clase.** Medido en `/d/r4o`: con el espacio raíz compila sin errores ni avisos, el RED es exacto y la guarda pasa. **Con los nombres de hoy**, el filtro heredado n.º 12 seleccionaría las copias Red (289 `Failed` en el RED); **con el prefijo, ningún filtro heredado las selecciona** (`v47/analysis-outside-477.md`, punto 1). Cambian los nombres completos de las copias, sus filtros (n.º 47 a 88), los del modo de medida (`…Tests.FullPanelCopy`), M-00, CT-20 y el `CopySha`. La lista no cambia por esto, y ningún filtro heredado ni prueba existente se toca | D-PBT4-11; §5.2; §6.2.1 |
| **V48-02** | **D-S1: dos copias mecánicas de WPP no pasan en el GREEN** (fallan igual en el RED): `ThePilotNeverErasesAView…` espera `Invalid` y el modelo completo responde `Ready`, porque borra la lateral obsoleta | **A mano, con su texto literal** (el anexo V48-A), según **D-PB-04**, **D-PBT3-09 (e)** y **D-PBT4-07**. Medido en `/d/r4p`: con el modelo completo, el fantasma da `Ready` con miembros A1 y borrados B2, y `side-b` da `Invalid` con `FrontalBOfOneWayPrefix`. **El mapa v5**: 356 mecánicas y 72 a mano, las dos con `R-D-S1-borra`. Con su texto, pasarían también en el RED (`PrepareFull` es código de la base), así que irían a la clase Green. **Sus nombres** («…NeverErasesAView…», «…MakesTheUpdateInvalid…») dicen lo contrario de lo que afirman: se conservan por el mecanismo (CT-20 (2) y la partición), y la regla es la del cuerpo (CA-T4b M-02; V49-03) | D-PBT4-11; §6.0 (RR-3); §8.3 |
| **V48-03** | **La sexta tanda del Owner:** `WorkspaceHostHardeningGuardTests.NoneOfThe30…` cae en el GREEN y no estaba en la lista. Cuenta tres textos que el GREEN cambia por D-PBT4-04 y D-PBT4-08 | **«Añadirla a la lista (Recomendado)»** (`owner-pbt4b-list-478-decision.md`). **La lista pasa a 478**: C-0 428, C-5 **23** y UI 27; Core 451; **21 archivos**. El equivalente `WorkspaceHostHardeningGuardFull` lleva la tabla con 0, 6 y 0, cada una con su `// before N`, y su prueba sin cambio. Sería Red: en el RED, WHC aún tiene las cuentas de la base | D-PBT4-11; §4.2; §6.2; §8.3; §8.5 |
| **V48-04** | **El paseo de guardas no veía los campos de tipo tupla** (`walk_guards_pbt4b_v45.py:91`) | **`walk_guards_pbt4b_v48.py`**, con el tipo de `FIELD` admitiendo una tupla y un genérico con espacios. **Con las entradas de la v4.5**, marca tres pruebas más fuera de la lista de 477 (`v48/compare_walks_v48-same-inputs-out.txt`). Una es la de la sexta tanda; **las otras dos no entran en la lista** y se declaran en §4.2: `WorkspaceHostHardeningGuardTests.TheGuardOfTheLiteralsTrips…`, una autoprueba que pasa, y `WorkspaceCommandAdvisorGuardTests.TheCatalogBody…`, cuyo `"Read"` es el valor de la columna `Body` de su tupla, no un literal que se cuente en RIA. **Las dos pasan en las dos suites completas del GREEN** (`/d/r4m` y `/d/r4o`). Con las copias de la v4.8 y la lista de 478, en §4.2 | §4.2 |
| V48-05 | Las cifras que dependen de la lista | Core sin las CT: **19 587 = (19 129 − R_C) + R_C + 458** en el RED y **19 587 = 19 129 + 0 + 458** en el GREEN. Con las CT: **19 605 = 19 147 + 0 + 458**. La UI no cambia. Además: el `MinSelected` del filtro n.º 12, de 2 719 a **2 718**; el `AllowedWriteScope`, de 57 a **59** rutas; las copias y los archivos de los `Skip`, de 20 a **21**; las clases de las copias, de 40 a **42**; R_C, `<medido en la precondicion 6>` (previsto: 292 + 1 − 2 = 291) | §5.2; §6.2; §6.2.1; §6.2.2 |
| V48-06 | La versión | El `Objective` de PB-T4b cita la v4.8, «sus seis tandas» y las 478; el de PB-T4a sigue citando la v4.3. **Las corridas de la precondición 6:** `/d/r4g` a `/d/r4i` (v4.5 y v4.6), `/d/r4j` a `/d/r4m` (v4.7), los diagnósticos `/d/r4n` a `/d/r4p` y las de la v4.8 (`/d/r4q` en adelante) | §6.2.1; §11 |

---

## La cabecera de la v4.7, sin cambio salvo lo que dice el registro v4.7 → v4.8

**Estado de la v4.7:** Corrige la parte de **PB-T4b** con lo que encontró la precondición 6 de la v4.6. No cambia la lista de 477, ninguna aserción, ninguna fila ni los totales, y no hay decisión nueva del Owner: es **D-S5**, una decisión de la sesión (`session-pbt4b-design-decisions.md`, sha256 `47e4bf86d426e0f3903c14ecc7d971999d610dd05c8eb595f37feeb85f3ffa5b`), con el mismo mecanismo que D-S1:
- **la precondición 6 de la v4.6 paró en el paso 4, la partición** (`/d/r4i`). Compiló sin errores y sin ningún `warning CS` en Core, UI y Plugin. De las 476 pruebas de copia, 249 pasan enteras en el RED, 223 fallan enteras, todas por aserción, y **cuatro `Theory` mezclan resultados** (30 filas: 18 fallan y 12 pasan). Ninguna prueba fuera de la lista de 477 falló;
- **D-S5: las cuatro se parten por filas** (V47-01) en dos equivalentes escritos a mano: uno en `…FullRedTests` con las filas que fallan en el RED y otro en `…FullGreenTests` con las que pasan, con el mismo nombre, el mismo cuerpo y las mismas aserciones del original. En las dos de `MemberData`, dos miembros de datos nuevos de la copia, `StatusNamesRed` y `StatusNamesGreen`, reparten las filas de `StatusNames` sin cambiar ninguna;
- **la regla de la partición se amplía solo para eso** (V47-02; §5.2, §6.2.1 paso 4, `partition_pbt4b_v47.py`): una `Theory` que mezcla se parte por filas en dos equivalentes a mano. Las demás causas siguen parando;
- **CT-PBT4B-20 admite el reparto por filas y nombra las cuatro** (V47-03);
- **el reparto de C-0 pasa de 360/68 a 358/70**, con el mapa de la copia v4 (`pbt4b-copy-map-v4.tsv`, sha256 `0c5abc6d00d9dfc9943812aae4c9fd31338c3b64f17cbff632f949378e9d9b1c`).

**Lo demás no cambia:** el GREEN (`green_pbt4b_v45.py`), el esqueleto, el kit de la v4.5, la lista de 477, las CT salvo CT-20, los totales, MU, las ediciones de ayudante de la v4.6, D-S4 y PB-T4a. **La precondición 6 se corre otra vez entera** con los guiones de la v4.7, en copias nuevas (`/d/r4j` en adelante). Sus resultados, su `pbt4b-precondition6.json` y su `compile-check.md` van en `pbt4bcc/compile/`, no en el paquete.

**Lo que salió de la v4.7 (PB-T4b):**
- el mapa de la copia v4 (`pbt4b-copy-map-v4.tsv`, `make_copy_map_v4.py`, `v47/make_copy_map_v4-out.txt`);
- las ediciones (`pbt4b_hand_edits_v47.py`: `SPLIT`, `RED_KEYS`, `SPLIT_COMMENT` y `C0_NEW_HELPERS`);
- el guion de la copia (`make_copies_pbt4b_v47.py`) y su revisión (`pbt4b-equivalents-review-v47.txt`, 102 entradas);
- los `Skip` (`pbt4b_skips_v47.py`), el RED (`apply_red_pbt4b_v47.py`) y su comprobación (`check_skips_pbt4b_v47.py`);
- la partición (`partition_pbt4b_v47.py`);
- la autoprueba (`selftest_pbt4b_v47.py`, `v47/selftest_pbt4b_v47-out.txt`), con la partición de prueba `v47/partition-check-r4i.tsv`, que es la de `partition_pbt4b_v47.py red` sobre las TRX de `/d/r4i` (un registro de trabajo, no la partición de la emisión);
- y `make_v47.py`, que arma esta versión desde la v4.6.

---

## Registro de cambios v4.6 → v4.7

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V47-01** | **D-S5: cuatro `Theory` mezclan resultados en el RED** (la precondición 6 de la v4.6, `/d/r4i`, paso 4) | **Se parten por filas, a mano, con su texto literal** (la tabla de abajo y el anexo V47-A). Dos eran copias mecánicas (WLHT y la `Confirm…` de WPP) y pasan a «a mano» con el motivo `R-D-S5`. La `Keep…` de WPP ya era a mano por D-S1 y lleva `R-D-S1,R-D-S5`. La de PEG es un equivalente de guarda (C-5) y sigue siéndolo. El guion pone la `Theory` entera en la clase Probe del modo de medida y, con la partición, sus dos mitades. **El mapa v4** (`0c5abc6d…9b1c`): 358 mecánicas y 70 a mano. Los dos `Skip` de C-0 que eran «copia» pasan a «equivalente escrito a mano». La lista no cambia | D-PBT4-11; §5.2; §6.0 (RR-3); §6.2; §8.3; §8.5 |
| **V47-02** | La regla de la partición de la v4.5 decía que una prueba que mezcla para la emisión, y que no se parte una `Theory` | **Una `Theory` que mezcla se parte por filas en dos equivalentes a mano.** La partición lo admite solo para las declaradas en el paquete (`SPLIT`), cuando sus filas que fallan son exactamente las declaradas (`RED_KEYS`, por el primer argumento que escribe xUnit en el nombre del resultado). Entonces da dos filas, Red y Green. **Siguen parando:** otra prueba que mezcla, una declarada que no mezcla o mezcla otras filas, una que no corre o se salta, y una falta sin aserción. Si la precondición 6 encuentra otra `Theory` mezclada, se le aplica D-S5 con una versión del paquete; si encuentra una copia que no pasa en el GREEN, D-S1 | §5.2; §6.2.1, pasos 4 y 6 |
| **V47-03** | CT-PBT4B-20 | Admite el reparto por filas: cada una de las cuatro tiene dos pruebas de igual nombre, una en cada clase, cuyas filas, juntas y sin repetirse, son las del original. Nombra las cuatro, con la línea `particion-por-filas <Clase>.<método> red=<n> green=<m> filas-del-original=True`. Las dos de `MemberData` se comprueban por reflexión en ejecución | §5.2 (CT-20); §6.0 (RR-3) |
| V47-04 | Los totales | No cambian: cada prueba de la lista sigue sumando un resultado, y las cuatro reparten sus filas entre sus dos mitades sin cambiar ninguna. El `[13]` y el `[14]` de `ExpectedEvidence` lo dicen | §5.2; §6.2.1 `[13]` y `[14]` |
| V47-05 | La versión | El `Objective` de PB-T4b cita la v4.7; el de PB-T4a sigue citando la v4.3. **Los `StopConditions` no cambian:** D-S5 es de la partición, que corre la sesión antes de emitir, y PBT4B-C07 y C16 siguen igual. Las corridas de la precondición 6: `/d/r4g` (v4.5, paró en la compilación), `/d/r4h` (diagnóstico), `/d/r4i` (v4.6, paró en la partición) y las de la v4.7 (`/d/r4j` en adelante) | §6.2.1; §11 |

**Las cuatro `Theory` partidas por filas** (D-S5; las filas Red son las que fallan en el RED, medidas en `/d/r4i`; el texto, en el anexo V47-A y en `pbt4b-equivalents-review-v47.txt`):

| `Theory` (archivo de origen) | Filas | Red: fallan en el RED | Green: pasan | Por qué falla en el RED | Mapa |
|---|---|---|---|---|---|
| WLHT `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` (`InlineData`) | 10 | 3: `a-check-that-finds-a-conflict`, `a-check-that-recovers` y `a-reload` | 7: `a-veto`, `a-failure-of-the-evaluation`, `an-admitted-intention`, `a-discard-of-the-host`, `a-drain`, `a-drain-with-the-stale-hint` y `a-note-of-an-edit` | `Assert.NotNull` en `Env.Pilot`: el anfitrión del RED aún busca el borrador del piloto | mecánica → `R-D-S5` |
| PEG `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` (`InlineData`, equivalente de guarda) | 4 | 1: la del adaptador estático, `private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();` | 3: el orden de las bases, `GetField` y `CommitField` | `Assert.Contains`: WHC del RED aún tiene el adaptador del piloto | guarda (C-5) |
| WPP `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` (`MemberData(StatusNames)`) | 8 | 7: `StatusNamesRed` (todos salvo `Clean`) | 1: `StatusNamesGreen` (`Clean`) | `NullReferenceException` en `Rig.Edit`, en el código de la prueba (por aserción según INV-PBT4B-02) | mecánica → `R-D-S5` |
| WPP `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` (`MemberData(StatusNames)`) | 8 | 7: `StatusNamesRed` | 1: `StatusNamesGreen` | la misma | `R-D-S1` → `R-D-S1,R-D-S5` |

**El texto de cada mitad** es el de la prueba en la copia (con M y, en la de PEG, con su edición de guarda), con dos cambios. El primero es una línea de comentario antes de `[Theory]`, `SPLIT_COMMENT`, que dice a qué mitad pertenece. El segundo son sus filas:
- en las de `InlineData`, las líneas `[InlineData(…)]` de su mitad, en el orden del original;
- en las de `MemberData`, `[MemberData(nameof(StatusNamesRed))]` o `[MemberData(nameof(StatusNamesGreen))]` en lugar de `[MemberData(nameof(StatusNames))]`.

**Los dos miembros nuevos** (`C0_NEW_HELPERS`) van solo en la copia de WPP, tras `StatusNames`, y cada clase de la partición lleva el suyo, porque solo se incluyen los ayudantes que alcanzan sus pruebas. Filtran las filas de `StatusNames` por `Clean`, sin escribir ninguna.

---

## La cabecera de la v4.6, sin cambio salvo lo que dice el registro v4.6 → v4.7

**Estado de la v4.6:** Corrige la parte de **PB-T4b** con lo que encontró la precondición 6 de la v4.5, sin cambiar la lista de 477 y sin decisión nueva del Owner (la decisión de la sesión, en desarrollo de D-S1):
- **la precondición 6 de la v4.5 paró en la compilación** (`/d/r4g`): el proyecto de pruebas de Core no compilaba con las copias del guion, por dos defectos del texto de las copias (`pbt4bcc/compile/compile-check.md`, §1); un diagnóstico (`/d/r4h`) separó los dos;
- **el guion de la copia añade la directiva `using` de `Draft/`** (MU, V46-01) a las copias que nombran un tipo de `Draft/`;
- **ocho copias «mecánicas» que no compilan sobre el modelo completo pasan a equivalentes escritos a mano** (D-S1, V46-02): seis de `WorkspacePilotEditHostTests` (cuatro por el ayudante `Env.OpenUndecidable`, editado en la copia, y dos por su cuerpo) y dos de `WorkspacePilotPrepareTests`; **el reparto de C-0 pasa de 368/60 a 360/68**, con el mapa de la copia v3;
- **la recomprobación de la v4.5** (§19 de `independent-review-v1.md`: LISTO PARA LA PRECONDICIÓN 6, 0/0/4): M-07 se corrige («cinco escaneos») y M-08 a M-10 se declaran en el paquete y en F4B-E52.

**Lo demás no cambia:** el GREEN (`green_pbt4b_v45.py`), el esqueleto, el kit de la v4.5, la lista de 477, las CT, los totales, D-S4 y PB-T4a. **La precondición 6 se corre otra vez entera** con los guiones de la v4.6, en copias nuevas (`/d/r4i` en adelante); sus resultados, su `pbt4b-precondition6.json` y su `compile-check.md` van en `pbt4bcc/compile/`, no en el paquete.

**Lo que salió de la v4.6 (PB-T4b):** el mapa de la copia v3 (`pbt4b-copy-map-v3.tsv`, sha256 `11b26ab1ecbfbf4a289c1485c14a803529549b2e066e13588668763448390743`, `make_copy_map_v3.py`); las ediciones (`pbt4b_hand_edits_v46.py`); el guion de la copia (`make_copies_pbt4b_v46.py`) y su revisión (`pbt4b-equivalents-review-v46.txt`, 90 entradas: 68 a mano, 21 de guarda y el ayudante editado); los `Skip` (`pbt4b_skips_v46.py`), el RED (`apply_red_pbt4b_v46.py`) y su comprobación (`check_skips_pbt4b_v46.py`); la autoprueba (`selftest_pbt4b_v46.py`, `v46/selftest_pbt4b_v46-out.txt`); y `make_v46.py`, que arma esta versión desde la v4.5.

---

## Registro de cambios v4.5 → v4.6

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V46-01** | **La precondición 6 de la v4.5: las copias no compilan** (`/d/r4g`, 41 errores: CS0246, CS0103 y CS0535) | **MU, una regla nueva del guion de la copia:** cada copia que nombra un tipo de `Draft/` (fuera de cadenas y comentarios) y no lleva `using RackCad.Application.Systems.PushBack.Draft;` gana esa directiva tras el último `using` de su cabecera. M-04 y M-06 cambiaban el nombre de los tipos pero no su espacio de nombres. Diez copias la llevan (nueve la ganan) | D-PBT4-11; §6.2.1 |
| **V46-02** | **D-S1: ocho copias «mecánicas» no compilan sobre el modelo completo** (el diagnóstico `/d/r4h`, con MU: 14 errores) | **A mano, con su texto literal** (`pbt4b_hand_edits_v46.py`): las seis de WPEH (el ayudante `Env.OpenUndecidable` **de la copia** y dos pruebas abren el borrador con la lectura completa, `FullRead!.Read`) y las dos de WPP (H-02, y la lectura completa de la misma base). **El guion admite ediciones de ayudante en C-0** (`C0_HELPER_EDITS`), en la copia y nunca en el original, y CT-PBT4B-20 las nombra; las fija el `CopySha`. **El mapa v3** (`11b26ab1…0743`): 360 mecánicas y 68 a mano, las ocho con el motivo `R-D-S1-compila`. Los ocho `Skip` pasan a «equivalente escrito a mano». La lista no cambia | D-PBT4-11; §5.2; §6.0 (RR-3); §6.2; §8.3 |
| V46-03 | M-07 de la §19 | «seis escaneos» → «cinco» (V45-08 y D-PBT4-11) | Registro v4.5; D-PBT4-11 |
| V46-04 | M-08 de la §19 | Declarado: D-S4 lee el piloto del `Primary` y difiere solo el modelo completo; el coste residual del piloto lo mide la sonda (`solo el piloto`) | D-PBT4-21; F4B-E52 |
| V46-05 | M-09 de la §19 | Declarado: el recuento de materializaciones de la sonda es el de su propia réplica; lo de RIA lo fija CT-PBT4B-21 | §6.2.1, paso 7; F4B-E52 |
| V46-06 | M-10 de la §19 | Declarado: la UI, solo con sus filtros; una prueba de UI de fuera se vería en la CI (C16) | §6.2.1, pasos 5 y 6 |
| V46-07 | La versión | El `Objective` de PB-T4b cita la v4.6; el de PB-T4a sigue citando la v4.3. Las corridas de la precondición 6: `/d/r4g` (paró en la compilación), `/d/r4h` (diagnóstico) y las de la v4.6 (`/d/r4i` en adelante) | §6.2.1; §11 |


## La cabecera de la v4.5, sin cambio salvo lo que dice el registro v4.5 → v4.6

**Estado de la v4.5:** Cierra la parte de **PB-T4b** sobre el código real ya cerrado de PB-T4a, con lo que llegó tras la v4.4:
- **la decisión del Owner sobre el coste de la lectura de PB-T4a,** «Cerrar y optimizar en PB-T4b» (`owner-pbt4a-cost-decision.md`), que la sesión desarrolla como **D-S4**: una sola materialización del modelo completo por lectura (D-PBT4-21);
- **la quinta tanda del Owner sobre PB-T4b,** «Añadirlas a la lista» (`owner-pbt4b-list-477-decision.md`): la lista pasa a **477**, con CT-PBT4A-03 y CT-PBT4A-05;
- **la revisión de la v4.4** (§18 de `independent-review-v1.md`: NECESITA v4.5, 0/2/6, sin decisiones para el Owner): I-01, I-02 y M-01 a M-06.

**PB-T4a está CERRADA** (F4B-E51, `78a7cef3`, sobre el GREEN `faef7020`) y no cambia: su texto de §5.1, §6.1, §6.1.1 y §6.1.2 queda byte a byte como en la v4.3 y la v4.4, con «v4.3» en su `Objective`. PB-T4c y PB-T4d, sin cambio de fondo.

**La base de PB-T4b:** el cierre de PB-T4a, **`78a7cef3`**, cuyo `src/` y cuyos `tests/` son los del GREEN `faef7020`. Se resuelve así lo que la v4.4 marcaba `<tras el cierre de PB-T4a>`: el nombre real de CT-PBT4A-06, los `BaseSha` de los 20 archivos de origen y la línea base de la CI (`44814418683`). Lo que dependa de ejecutar va marcado **`<medido en la precondicion 6>`** (ASCII, como en el bloque literal), con su regla.

**Cómo se midió:** solo por objetos git en `78a7cef3`, con Python `-I` y guiones escritos en `<carpeta-del-paquete>`. Nada se compiló ni se ejecutó, y el worktree no se leyó. El GREEN literal de PB-T4a (`green_pbt4a.py`) es, byte a byte, el de `faef7020` en sus seis archivos de `src/`.

**Lo que salió de la v4.5 (PB-T4b):**
- **la lista de 477** (`pbt4b-substitution-list-477.tsv`, sha256 `5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651`): las 475, sin cambio y en su orden, y CT-PBT4A-03 y CT-PBT4A-05 al final; las de 470 y 475 quedan como historia;
- **D-S4:** RIA difiere la lectura completa del `Primary` (`System.Lazy`), y la relectura con el origen encontrado ya no la materializa; RIA entra en el alcance como `UNIT_CHANGE`, con su lista cerrada de cambios; **CT-PBT4B-21** lo fija; **la sonda del coste** (`pbt4b-read-cost-probe.cs`) va en la precondición 6;
- **el GREEN literal de `src/`** (`green_pbt4b_v45.py`: el de la v4.4 y RIA, diez archivos) y **`apply_green_pbt4b_v45.py`**; el RED, el de la v4.4 (RIA no cambia en él), con **`apply_red_pbt4b_v45.py`**: el esqueleto, los 477 `Skip`, el kit y las 20 copias;
- **el kit de la v4.5** (`pbt4b-kit-WorkspaceFullReadKit-v45.cs`: `FullReadRig`, M-04 y M-05) y **el guion de la copia de la v4.5** (`make_copies_pbt4b_v45.py` con `pbt4b_hand_edits_v45.py`), con su revisión (`pbt4b-equivalents-review-v45.txt`, 81 equivalentes);
- **los `Skip` literales** (`pbt4b_skips_v45.py`) y **su comprobación** (`check_skips_pbt4b_v45.py`, 20 archivos y 477 `Skip`);
- **la partición con la regla de INV-PBT4B-02** (`partition_pbt4b_v45.py`, I-02) y **dos pasos más en la precondición 6:** la sonda de las líneas «presentar» de CT-06 (`pbt4b-ct06-presentar-probe.cs`) y la suite completa sobre el RED exacto, con sus totales;
- **CT-PBT4B-20 corregida** (I-01) y **§5.2, §6.2 y §8.5 al día**, con los totales exactos de cada rama.

**Los hechos,** en `pbt4-measurements.md` (R-0 a R-43; las mediciones de la v4.4 y la v4.5 van en sus salidas). **Los guiones y sus salidas** de la v4.5: `make_list_477.py` (`v45/make_list_477-out.txt`); `green_pbt4b_v45.py`; `check_ria_v45.py` (`v45/check_ria_v45-78a7-out.txt`: CT-PBT4A-07 con su código real, CT-PBT4B-21 y las cuentas de B-08, CAG, G-9 e INV-14); `walk_guards_pbt4b_v45.py` (`v45/pbt4b-guard-walk-v45-delta2.tsv` y `-out.txt`, solo lo que cambia D-S4; `v45/pbt4b-guard-walk-v45-full.tsv` y `-out.txt`, todo el GREEN sobre el cierre de PB-T4a); `ria_added_lines_v45.py` (`v45/ria_added_lines_v45-out.txt`); `compare_walks_v45.py` (`v45/compare_walks_v45-out.txt`); `make_copies_pbt4b_v45.py`, `pbt4b_hand_edits_v45.py` y el modo de medida (`v45/copies_probe_v45/` y `v45/copies_probe_v45-out.txt`); `pbt4b_skips_v45.py`, `apply_red_pbt4b_v45.py`, `apply_green_pbt4b_v45.py`, `check_skips_pbt4b_v45.py` y `partition_pbt4b_v45.py`; su autoprueba y las medidas que se citan (`selftest_pbt4b_v45.py`, `v45/selftest_pbt4b_v45-out.txt`) y la de los dos guiones de escritura sobre una copia mínima (`selftest_apply_v45.py`, `v45/selftest_apply_v45-out.txt`: el RED escribe 47 archivos, con 477 `Skip` y 20 copias, y el GREEN, 10); las dos sondas de la precondición 6 (`pbt4b-read-cost-probe.cs` y `pbt4b-ct06-presentar-probe.cs`); y `make_v45.py`, que arma esta versión desde la v4.4 y los fragmentos de `v45/`.

**Abreviaturas:**
- [P], [OD], [P2] y [P3]: el paquete de la fase v3.1, la disposición del Owner y los paquetes de PB-T2 v2.9 y PB-T3 v4.7;
- [PT1]: el paquete de PB-T1, `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`;
- [PKC2]: el paquete del contrato de F4-T4C2, del que F4B copia las paradas estándar (como [P3] §6.0);
- [DP]: las decisiones del Owner sobre el procedimiento de F4B (`owner-f4b-procedure-decisions.md`);
- DISP: `F4B-DRAFTS/f4b-dispositions.md`;
- WHC: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- NH: `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`;
- RIA: `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- FVM: `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`;
- PV: `src/RackCad.UI/Workspace/WorkspacePanelView.xaml(.cs)`;
- W: `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml(.cs)`;
- WCo: `src/RackCad.Plugin/Workspace/WorkspaceController.cs`;
- PP y UR: el plan y la corrida de `Systems/PushBack/Pilot/`;
- EIH: `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs`;
- EIG: `…/WorkspaceEditIntentGuardTests.cs`;
- WLG: `…/WorkspaceLifecycleGuardTests.cs`;
- PEG: `…/WorkspacePilotEditGuardTests.cs`;
- RG: `…/WorkspacePilotReconciliationGuardTests.cs`;
- CAG: `…/WorkspaceCommandAdvisorGuardTests.cs`;
- WBSG: `…/WorkspaceBridgeSourceGuardTests.cs`;
- WNAT: `…/WorkspaceNavigationAuthorityTests.cs`;
- WHSG: `…/WorkspaceHostSourceGuardTests.cs`;
- URT: `…/WorkspacePilotUpdateRequestTests.cs`;
- IG: `…/WorkspacePilotUpdateIntegrationGuardTests.cs`;
- WPP: `…/WorkspacePilotPrepareTests.cs`; WPRH: `…/WorkspacePilotReconciliationHostTests.cs`; WPUH: `…/WorkspacePilotUpdateHostTests.cs`; WPUG: `…/WorkspacePilotUpdateGuardTests.cs`;
- FRP, FRG y FRS: `tests/RackCad.Tests/PushBackFullReadPathTests.cs`, `…/PushBackFullReadGuardTests.cs` y `…/PushBackFullReadSupport.cs`, las clases de las CT de PB-T4a;
- F5-A: la fase de I-64 que sigue a F4B (la del optimizador del layout), donde el roadmap aplaza lo que F4B no hace.

---

## Registro de cambios v4.4 → v4.5

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V45-01** | **Dos pruebas de PB-T4a fuera de la lista caen con el diseño** (CT-PBT4A-03 y CT-PBT4A-05: sus ayudantes buscan el borrador del piloto o leen sin la lectura completa; la v4.4 solo tenía su especificación) | **La decisión del Owner, «Añadirlas a la lista»:** la lista pasa a 477 (`pbt4b-substitution-list-477.tsv`, `5975c5b6…a651`). Sus equivalentes, escritos a mano: el mismo ayudante sobre el borrador completo (`FullReadRig`, en el kit) y, en CT-05, un lector sin la interfaz que responde `FromReads` (`FullEditIntentFakeReader`). 20 archivos de los `Skip` y 20 copias | D-PBT4-11; §4.2; §5.2; §6.2; §8.3; §8.5 |
| **V45-02** | **D-S4: una sola materialización por lectura** (`owner-pbt4a-cost-decision.md`) | RIA difiere la lectura completa del `Primary` en un `System.Lazy` sobre el mismo `input`, y `Full` es `_full?.Value`; `AtOrigin`, NH, `ReadAt` y WHC no cambian. RIA entra en el alcance como `UNIT_CHANGE`; CT-PBT4B-21 lo fija en texto; la sonda del coste, en la precondición 6. Ninguna prueba cae: CT-PBT4A-07, con su código real, da sus 13 líneas; B-08, EIG:139-141, CAG y G-9 no cambian | D-PBT4-21; §4.2; §5.2; §6.2; §11 |
| **V45-03** | **I-01: CT-PBT4B-20 no podía pasar en el RED** | La copia mecánica es su original «con M-00 a M-08», con los tipos de M-00 de cada clase literales; se compara con el texto del método en `BaseSha` (sin el `Skip`); `BaseSha` y `CopySha` son el sha256 del texto con los saltos de línea a `\n` (`core.autocrlf=true`) | §5.2; §6.0 (RR-3) |
| **V45-04** | **I-02: la partición y la precondición 6** | La partición usa la regla de INV-PBT4B-02 (la primera línea del StackTrace bajo `tests/`; `partition_pbt4b_v45.py`); la precondición 6 gana la sonda de las líneas «presentar» de CT-06 y la suite completa sobre el RED exacto, con sus totales (Core 19 586 y UI 2 478, sin las CT) | §5.2; §6.2.1; §11 |
| V45-05 | M-01: la copia de EIG | `BaseOrReadUse` busca `pilot` y `full` | D-PBT4-11 |
| V45-06 | M-02: las vistas | Hasta tres líneas de vistas, dicho y medido (CT-06, `of lineas=7`), y declarado en F4B-E52 | D-PBT4-09 |
| V45-07 | M-03: las 28 copias idénticas | **Se quedan en la lista,** declaradas como copias sin cambio: la lista es la que vio el Owner, y una copia idéntica que cayera sería D-S1, no una vuelta al Owner | D-PBT4-11; §6.2.1 `[13]` |
| V45-08 | M-04 y M-05: el kit | `FullPrepareKit.Reread` rechaza un estado (`Assert.Null(state)`); `FullHandKit.FullOfMatchesTheReader`, que CT-20 corre sobre cinco escaneos (V46-03) | D-PBT4-11; §5.2 |
| V45-09 | M-06: el tamaño | El contrato y las listas nunca van en línea en un prompt (30 000 bytes; la cadena los cita por ruta y sha256) | §6.2.2 |
| V45-10 | La base | El cierre de PB-T4a, `78a7cef3`: el nombre de CT-PBT4A-06, los `BaseSha`, la CI de la línea base; el `ForbiddenWriteScope`, por la regla del generador sobre el alcance final (con el de la v4.4, 407 en `78a7cef3`) | §6.2; §6.2.1; §6.2.2 |
| V45-11 | Los totales | GREEN Core **19 604 = 19 147 + 0 + 457** y UI **2 481 = 2 433 + 0 + 48**; el RED, con R_C y R_UI; sin las CT, Core **19 586** y UI **2 478** | §5.2; §6.2.1 `[14]`; §11 |
| V45-12 | §8 y las autoridades | §8.5: las cinco tandas del Owner sobre PB-T4, la de PB-T4a sobre el coste y D-S1 a D-S4; las autoridades de PB-T4b, sin las de la cadena de PB-T4a | §1; §6.2.2; §8.5 |
| V45-13 | La versión | El `Objective` de PB-T4b cita la v4.5 y CT-PBT4B-01..21; el de PB-T4a sigue citando la v4.3 | §6.2.1 |

## Registro de cambios v4.3 → v4.4

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V44-01** | **Cinco guardas fuera de la lista caen con el diseño** (medidas sobre el GREEN literal; §17 de la revisión) | **La decisión del Owner, «Añadirlas a la lista»:** la lista pasa a 475 (`pbt4b-substitution-list-475.tsv`, `654c3b7b…0707c`), con EIG:871 y :883 (el diagnóstico de admisión de la lectura completa), WLG:627 y :642 (el marcado y la recarga del borrador completo) y WPUG:823 («Actualizar» con `ExecuteFull`); la de PB-T4d pasa a 138 (`d5467dc1…387c`); el `MinSelected` de WPUG baja a 85. Cada una con su equivalente literal sobre el modelo completo; el de G-4 trae la autoprueba que IG:510-517 ya no discrimina | D-PBT4-11; §4.2; §6.2; §6.4; §8.3; §8.5 |
| **V44-02** | **D-S2: «Conservar» con dos «Sí»** | El paso 1 de la v4.3 (`RequestKeepMine` llamaba al puerto) rompía RG:104 y RG:128, que no están en ninguna lista. `RequestKeepMine` sigue preguntando desde la memoria; el primer «Sí» llama a `TryKeepMine` sin testigo y presenta; el segundo, con el testigo vigente, hace `KeepOn`. La línea de RG:132 no cambia y la petición lleva el testigo. Medido. Cambio visible, en PBT4B-C03 | D-PBT4-08; §4.2; §6.2.1 |
| **V44-03** | **D-S1: siete copias mecánicas no pueden pasar en el GREEN** | Pasan a equivalentes escritos a mano, con su texto literal: 368/60, y el final, tras la precondición 6. Los 60 y los 19 equivalentes de las guardas, literales, por `make_copies_pbt4b.py` con `pbt4b_hand_edits.py`; el Worker los copia byte a byte | D-PBT4-11; §5.2; §6.2 |
| **V44-04** | **El kit, literal** | `pbt4b-kit-WorkspaceFullReadKit.cs`: los tres tipos con los miembros de los del kit del piloto y las lecturas del piloto (D-S3), y `FullHandKit`, el apoyo de los equivalentes; un `Reread` hecho a mano lleva su gemelo completo | D-PBT4-11; §6.2.1 |
| V44-05 | Las copias, partidas | Cada copia, en `<Clase>Full.cs` junto a su original, en `…FullPanelCopies`, con su clase partida en `FullRedTests` y `FullGreenTests` y, en cada una, solo los ayudantes que alcanzan sus pruebas (un campo privado sin uso sería un `warning CS`). La partición, medida por la sesión con `partition_pbt4b.py` | D-PBT4-11; §6.2.1 |
| **V44-06** | **D-S3: tres correcciones del paquete** | El testigo se lee con `var pilot = read.PilotRead;` en **cuatro** sentencias (EIG:743); el texto del testigo de `Presented` y la admisión de «Conservar» salen de la función pura de `Draft/` (`PushBackKeepMinePresentation.Read`, con `OriginText`, `Diagnostic` e `IsAdmitted`), y WHC solo nombra la lectura en sus consumidores; `FullPrepareKit` conserva los tipos de lectura del piloto | D-PBT4-04; D-PBT4-08; D-PBT4-09; D-PBT4-11 |
| **V44-07** | El encargo de la v4.4 | **§5.2 entero:** CT-PBT4B-01 a 20 con sus líneas, su clase y su `ExpectRed`; las líneas de CT-06 (la presentación «Por grupos»), deducidas del clasificador real y fabricadas, a confirmar en la precondición 6; **los totales exactos:** GREEN Core 19 601 = 19 146 + 0 + 455 y UI 2 481 = 2 433 + 0 + 48; el RED, con lo que mide la partición | §5.2 |
| **V44-08** | El encargo de la v4.4 | **§8.5:** las tres tandas del Owner sobre PB-T4 y la decisión de la sesión, citadas | §8 |
| **V44-09** | El encargo de la v4.4 | **§6.2 entero:** el `AllowedWriteScope` ruta a ruta (53), `UNIT_CHANGE` (WHC, WEI, FVM, PV y `NumericFieldValidation.cs`; ni WCo ni `WorkspacePilotSupport.cs`, que el GREEN no toca), el RED y el GREEN, el bloque de literales (§6.2.1: `TaskId`, `Objective`, INV-PBT4B-01 a 14, PBT4B-C01 a C20 y `ExpectedEvidence` [0] a [15]), los `RequiredTests`, los `Skip`, el kit, la partición y lo que necesita el generador (§6.2.2); y las filas de PB-T4b de la tabla de RR-3 (§6.0) | §6.0; §6.2 |
| V44-10 | La versión | El `Objective` de PB-T4b cita la v4.4; el de PB-T4a sigue citando la v4.3, con la que se emitió | §6.1.1; §6.2.1 |

## Registro de cambios v4.2 → v4.3

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V43-01** | **CA-T4a I-01** | **El texto literal de `Draft/PushBackOriginByText.cs`, sin `?` de tipo de referencia:** `public static string Choose(…)` y `pair => pair.Value`, en D-PBT4-03 y en la viñeta del esqueleto de §6.1. Con `Nullable=disable` (`Directory.Build.props:28`, que ningún `.csproj` cambia) y la pureza de `Draft/`, que prohíbe `#nullable` (CT-PBT3A-14 y CT-PBT4A-08), cada `string?` daba **CS8632**, ya en el esqueleto (registros de la compilación de la sesión), y `build-plugin` falla con cualquier `warning CS` (`eng/ci/verify-autocad-references.ps1`, `Assert-DotNetSuccess -NoWarnings`). Se corrige también la justificación del molde (v4.2:401): el archivo es «oblivious», y pasar un `Dictionary<string, string>` al `IReadOnlyDictionary<string, string?>` del constructor no da ningún aviso. **§5.0 gana la regla,** y las demás piezas de código literal de PB-T4a, PB-T4b y PB-T4c se revisaron frente a ella: **ninguna más cambia** (§5.0). Los totales no cambian | D-PBT4-03; §5.0; §6.1 |
| **V43-02** | **CA-T4a M-01** | **El recorrido no es «entero»:** §4.1, §5.0, §5.1, §9 y la frase de PBT4A-C16 lo describen como es, por marcas (`MARKERS`), que no ven las rutas compuestas con `Path.Combine` ni los barridos de todo `src/`, de todo Application o de todo el Plugin. Se añade la ampliación de CA-T4a: **146** archivos de prueba de fuera nombran `src/RackCad.(Application|Plugin)`, `"src"` o un ayudante de barrido, y **97** nombran como literal una carpeta antecesora de un archivo tocado o nuevo; sus literales, probados contra las 134 líneas que el GREEN añade o cambia y contra los dos archivos nuevos, solo casan con fichas genéricas, y los que leen de verdad esos archivos se leyeron: **ninguno cae**. `WorkspaceModelBoundaryTests` entra en la lista de reflexión. El riesgo que queda, en C16: una lectura que solo se ve al ejecutar; lo cierra la medida de la precondición 6 con `apply_green_pbt4a.py` | §4.1; §5.0; §5.1; §9; §11 |
| V43-03 | La recomendación de CA-T4a con M-01 | **`apply_green_pbt4a.py <dir-copia>`** escribe en una copia exportada de `7f0d9aa3` el `src/` del GREEN literal de PB-T4a (seis archivos, con LF; no toca `tests/`), tras comprobar que la copia es de la revisión y no es un checkout; imprime cada archivo con su sha256 y el sha256 del conjunto. Lo prueba `selftest_apply_green.py` | §11 |
| V43-04 | CA-T4a M-03 | La precondición 6 pide los 12 filtros de UI de los `RequiredTests` sobre la copia, o la declaración de que no se corrieron (los cubre `ui-tests` de la CI del RED); y cero `warning CS` | §11 |
| V43-05 | La versión | El `Objective` de las tres subtareas cita la v4.3; el nombre del archivo lleva un solo punto decimal, como pide `bounded_check` | §6 |

## Registro de cambios v4.1 → v4.2

| Id | Hallazgo o decisión | Qué cambia | Dónde |
|---|---|---|---|
| **V42-01** | **«Retirar tras PB-T4c»** (pregunta 1 de la segunda vuelta) | El código del piloto **no se retira en PB-T4b**. Hay una subtarea nueva, **PB-T4d**, después de PB-T4c, con todo lo de la retirada y la causa C-7: sustituye las 139 y borra con registro las 55 originales saltadas y los 29 miembros de apoyo, y vuelve al Owner con su lista antes de emitirse. De PB-T4d hay un esbozo con su lista (`pbt4d-retire-list.tsv`, sha256 `1c9d67e8…8b71`); su contrato viene después. PB-T4b queda sin borrar ni editar ninguna prueba: su `UNIT_CHANGE` y su alcance no tienen ningún archivo de `Pilot/` | D-PBT4-04, 06, 10 y 20; §0; §6.0, §6.2 y §6.4; §8; §11 |
| **V42-02** | **B-08** | `PushBackOriginByText.ReadAt(scan, originText, catalog)`, en `Draft/`, hace las dos relecturas puras y devuelve una tupla (el escaneo, la del piloto y la completa) o `null`; `AtOrigin` solo la llama. RIA conserva un `PushBackPilotIntentReader.Read(` y un `new PushBackPilotIntentScan(` (EIG:350-364, medido en verde). Cambian con él: PBT4A-C13 (`ReadAt`) e INV-PBT4A-04; las líneas de CT-PBT4A-04 (`readat …`) y de CT-PBT4A-07 (`ria-lectores …`, `ria-AtOrigin readat=1 piloto=0 completo=0 …` y `autoprueba-segunda-lectura-del-piloto-en-RIA=falla`); la pureza de CT-PBT4A-08, que admite los dos lectores; el esqueleto (`ReadAt` inerte, `=> null;`). **Los totales no cambian** (comprobado) | D-PBT4-03; §5.1; §6.1 |
| **V42-03** | **«Sí, esa lista»** (pregunta 2 de la segunda vuelta) | La lista de PB-T4b, **470**, separada en `pbt4b-substitution-list.tsv` y fijada por sha256 (`44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`), con la regla de filtrado del archivo completo (`f24c2dbc…`). El generador de PB-T4b comprueba las dos cosas | D-PBT4-11; §6.2; §11 |
| **V42-04** | **I-25** | **El recorrido entero,** por diferencia, de las 1 026 pruebas de las 46 clases que leen RIA, NH, WHC, WEI, `Application/Workspace/` o sus carpetas, sobre el texto literal del GREEN: 764 no las leen, 162 las leen sin ninguna diferencia, y las 100 señaladas se leyeron una a una (cae una, CT-PBT3A-12, la de la lista). «D = 0», corregido: en el RED lo confirma la precondición 6; en el GREEN, el recorrido, y lo que no ve es un riesgo con su parada (PBT4A-C16) | §4.1; §5.1; §9 |
| **V42-05** | **I-26** | Retirar después de PB-T4c **no evita el borrado** de las 55 originales y de los 29 miembros de apoyo: cuando se retire, dejarán de compilar igual. Lo que cambia es que ya no corre ninguna | D-PBT4-10; §6.4 |
| V42-06 | M-32 | De las 67 por compilación, `WorkspacePilotUpdateRunTests` da 42 y `PushBackFullWritePrepareTests`, 6; 48 y 9 son sus totales en C-7, por texto y por compilación | §6.4; §8.3 |
| V42-07 | M-33 | Las 9 de las 15 de la letra que nombran lo que se retirará caen por el cambio del anfitrión, no por la retirada: 470 es exacto con cualquier opción | D-PBT4-11; §4.2 |
| V42-08 | M-34 | La partición de las copias por su resultado en el RED (`<Clase>FullRedTests` y `<Clase>FullGreenTests`), técnica; el kit nuevo es código de prueba, literal en el contrato, y no la «implementación de referencia» | D-PBT4-11; §5.2 |
| V42-09 | M-35 | El `<summary>` de `AtOrigin` dice que una excepción de la relectura pura no se contiene en RIA, sino en NH | D-PBT4-03 |
| V42-10 | El léxico de las mediciones | `lists_core.py` tomaba `@"""…` por una cadena cruda. Corregido; las listas regeneradas son iguales byte a byte | §9; R-41 |
| V42-11 | El generador de PB-T4a | Lo que cambia respecto de la v4.1: el esqueleto (`ReadAt`), PBT4A-C13, INV-PBT4A-04, las líneas de CT-04 y CT-07 con su fila de RR-3, y «v4.2» en el `Objective` | §6.1 |

## Registro de cambios v4 → v4.1

| Id | Hallazgo o decisión | Qué cambia | Dónde |
|---|---|---|---|
| **V41-01** | **Las decisiones del Owner** (Q1 a Q4) | Las ramas no elegidas salen: «Conservarlo» y «Dos modelos» (Q1), «Por campos» y «Por campos con valores» (Q2), «Editar una línea» y «la referencia» (Q3), y las emisiones parciales (Q4). Quedan en §8.2, con la cita del registro. EIH no se edita; no hay `PushBackFieldLabels` ni `PushBackPresentationForm` | D-PBT4-09 a 11; §5; §6; §8 |
| **V41-02** | **I-24** | Una CT por subtarea con G-L1 (copiada de CT-PBT2A-04, 15 autopruebas) y la pureza (copiada de CT-PBT3A-14) sobre **sus** archivos nuevos de `Draft/`: **CT-PBT4A-08** (`PushBackOriginByText.cs`), **CT-PBT4B-19** (`PushBackFullFieldAdapter.cs`, `PushBackKeepMinePresentation.cs` y `PushBackAnnotationScaleRule.cs`, con la línea de `Apply`) y **CT-PBT4C-15** (`PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs`, con la de `ApplyToDraft`). Así la condición 10 de D2 no deja de cubrirlos en silencio. 32, 33 y 33 líneas; sus filas de RR-3 | §5.1 a 5.3; §6.0 |
| **V41-03** | **I-22, I-23** | **Las listas, guardadas y exactas,** solo de lo elegido: `pbt4-substitution-lists.md` (y `.tsv`). La UI de la elección, recontada: 27 (21 de `WorkspacePilotReconciliationTests` y 6 de `WorkspacePilotReconciliationViewTests`). Lo que añade «Retirarlo», sin las ya contadas, por texto y por compilación. **Los números, frente a la cota, en §8.3: PB-T4b la supera** | D-PBT4-11; §8.3 |
| **V41-04** | **El guion de «Saltar y duplicar»** (mensaje de la sesión, punto 3) | El mapa cerrado M (M-00 a M-08), el kit nuevo y **CT-PBT4B-20,** la CT que comprueba, clase a clase, que cada copia es el original con el mapa y nada más. 375 copias mecánicas y 53 «a mano», con su motivo | D-PBT4-11; §5.2 |
| **V41-05** | **I-08** («Por grupos») | Los nueve nombres fijos, la unión (`A`, `A y B`, `A, B y C`), el plural de las vistas y las líneas de lo estructural y de las vistas, literales | D-PBT4-09 |
| **V41-06** | **Duda 4 del generador** (`RackPilotIntentAdapter.Read` y NH:242) | **Medido:** EIG:139-141 fija la firma entera de `Read`, hasta `RackCatalog catalog)`. Un parámetro nuevo, obligatorio **o con valor por defecto,** la rompe (contrafactual en rojo, 2 de 2), y esa guarda no está en la lista de PB-T4a. **Lo que menos rompe:** la firma no cambia; RIA gana `AtOrigin`, una relectura pura sobre el escaneo ya tomado, y NH, que ya estaba en el alcance y en `UNIT_CHANGE`, la llama con el texto del origen. Ninguna guarda prohíbe parámetros opcionales en el Plugin; la que fija la firma lo decide | D-PBT4-02 y 03 |
| **V41-07** | **Duda 5** (las guardas que buscan `ReadIntent` sin anclar) | Medidas una a una sobre el texto literal del GREEN (WHC con la sentencia en los cinco cuerpos; NH y RIA con sus ediciones literales): **32 de 32 en verde,** con EIG:579, 604-617, 636, 925 y 1124, WLG:369, 442, 623, 634, 656 y 687, y PEG:288 y 297 | §4.1 |
| **V41-08** | **Duda 3** (`MinSelected`) | `PushBackDraftWriteGuardTests` baja de 3 a **2** en PB-T4a, porque el `Skip` de CT-PBT3A-12 cae en ese filtro heredado | §6.1 |
| **V41-09** | **Duda 3** (los totales con la CT de I-24) | CT-PBT4A-08 **pasa en el RED** (es una guarda sobre un archivo que el esqueleto ya crea). **Una sola rama** (F4B-E46): RED 19 137 = 19 125 + 5 `Failed` + 7 `Skipped`; GREEN 19 137 = 19 130 + 7. Un `[Fact]` por CT | §5.1; §6.1 |
| **V41-10** | **Duda 6** (el `Skip` en el parche del esqueleto) | El parche lleva `src/` y el `Skip` de CT-PBT3A-12, con su texto exacto. La suite de Core sobre la copia con el esqueleto: **19 129 = 19 122 + 7,** comprobado | §6.1 |
| **V41-11** | **Duda 1** (el TaskId) | La regla es **«como mucho 26 caracteres»** (MAX_PATH solo acota por arriba). `F4B-PBT4-FULL-READ-PATH` tiene 23; los nombres no cambian | §0; §6.0 |
| **V41-12** | **Duda 7** (sin remisiones) | §5.0, §5.1, §6.0 (con la identidad, las paradas comunes con su texto, RR y C11) y §6.1, enteros. Los literales del contrato de PB-T4a, en un bloque, para la comparación del generador | §5; §6 |
| **V41-13** | **M-28** | El texto desfasado: el encabezado del §2 (`7f0d9aa3`), el `<a medir>` de CAG en D-PBT4-02 (medido: verde, con la restricción de no añadir motivos) y la cabecera (R-0 a R-38 y todos los guiones) | Cabecera; §2; D-PBT4-02 |
| **V41-14** | **M-29** | Se declara que el esqueleto del RED de PB-T4b **no toca** `NumericFieldValidation.cs`: la delegación es del GREEN | D-PBT4-06; §6.2 |
| **V41-15** | **M-30** | CT-PBT4C-08 fija el literal de la conversión de `object` en el abridor: `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);` | §5.3 |
| V41-16 | El causa de CT-PBT3A-12 | La rompe RIA, que nombra `PushBackFullDraftReader` (uno de sus cinco nombres); no `PushBackOriginByText`, que no está entre ellos | §4.1 |
| V41-17 | Dudas 7 y 8 | La tabla de motivos de CAG (5 en `Read` de RIA, 3 en `ReadOnActiveDocument` de NH, dos `ex.Message`) y la reflexión sobre WHC siguen en verde, medidas; CT-PBT4A-07 fija `literales=0` en `AtOrigin` | §4.1; §5.1 |
| V41-18 | El generador de PB-T4a | La lista de lo que necesita: alcance, prohibido, invariantes, paradas, `RequiredTests` con `MinSelected`, evidencia con los totales exactos de la rama única y autoridades | §6.1.2 |

## Registro de cambios v3 → v4

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V4-01** | **B-07** (WCo no puede nombrar `RackCad.UI`) | **La regla de la escala pasa a Application,** sin cableado:<br>• la lógica pura de `NumericFieldValidation` (que solo usa `System.Globalization` y `LocalizedNumberParser` de `RackCad.Application.Formatting`) pasa a una clase de Application, `NumericTextRule`;<br>• `NumericFieldValidation` de la UI delega en ella, con la misma API pública y los mismos mensajes;<br>• `PushBackAnnotationScaleRule` (Application) da los parámetros de la escala; WHC la usa por defecto.<br>Hay una sola implementación y Core la tiene, así que no queda «el campo sin regla». **Medido:** ninguna guarda lee el texto de `NumericFieldValidation.cs` ni de `NumericField.cs`; las pruebas que la nombran la llaman por su API (R-30), y ninguna es de T1B por letra.<br>**El abridor, en dos piezas:**<br>• la clase anidada de WCo solo abre el modal, sin nombrar `RackCad.UI` ni `PushBack`;<br>• la ventana la abre `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`, que no nombra `RackCad.Application.Workspace`.<br>**Rompe una sola guarda: CT-PBT1B-15,** el censo de archivos del Plugin con `PushBack`, que pasa de 14 a 15. Ninguna carpeta lo evita: todo archivo nuevo que abra la ventana nombra `PushBack` (R-31) | D-PBT4-06, 13 a 16; §4.3 |
| **V4-02** | **B-06** (las firmas y los números) | **Las firmas,** por método más los ayudantes de su mismo archivo, a un nivel y por nombre; ya no «por archivo». **C-8 desaparece** con la regla en Application. **C-9 es nueva:** las lecturas `FromPilot(` y `PrepareKit.Reread(` fuera del kit. **Medido por opción de Q4** (R-29):<br>• opción 1: 242 de 428;<br>• opción 2: 428;<br>• opción 3: no se mide por texto.<br>`<n4>`: 149 métodos por texto, 99 fuera de las 9 clases | D-PBT4-11; §8.2 |
| **V4-03** | **I-19** | EIG:664-690 y EIG:692-721 cuentan `pilot.Read` por cuerpo y no se pueden evitar sin quedarse con el borrador del piloto. Van a la lista de PB-T4b (C-5) | §4.2 |
| **V4-04** | **I-20** | CT-PBT4C-10 se parte en dos:<br>• Core: puerto → compuerta → WHC → doble del editor → «Actualizar»;<br>• **CT-PBT4C-14 (UI):** la FVM real sobre un doble del puerto que registra la llamada | §5.3 |
| **V4-05** | **I-21** (Q4) | Q4 neutral, con los números. **Las dos opciones cumplen las tres condiciones,** así que se recomienda «Saltar y duplicar», que no exige una excepción a C02 ni se aparta del «como D-1» | §8.2 Q4 |
| V4-06 | Q3 | La lista como **cota superior**: se emite la que vio el Owner o una parte; si el GREEN hace fallar otra prueba, vuelve al Owner | §8.2 Q3 |
| V4-07 | M-24 | El texto desfasado, corregido: D-PBT4-01 (una sustitución), D-PBT4-17 (la costura medida), D-PBT4-19 (sin `x:Static`), §7 («la v4»), la precondición 4 (Q1 a Q4) y la cabecera (`7f0d9aa3`) | Varios |
| V4-08 | M-25 | La condición 7 sustituye la línea base resuelta con `PushBackEditorState.SetWorkingBaseline(PushBackSystem)` (`PushBackEditorState.cs:74-79`), sobre una copia | D-PBT4-18 |
| V4-09 | M-26 | Q2: «del orden de un centenar de campos del editor» | §8.2 Q2 |
| V4-10 | M-27 | OWNER-1 a OWNER-3 de F4B-E48 (espacio papel o bloques del usuario, purga de un nivel, bloques dinámicos), en el Smoke-PB y en §7. La medición de la duración de la suite de Core (límite de 20 min, `ci.yml:42`) | §5.5; §7 |

## Registro de cambios v2 → v3

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V3-01** | **B-05** («Editar todo» no llega al anfitrión) | Hay **dos reenvíos** entre el panel y el anfitrión: `PilotEditAdapter` (WCo:283-327) y `WorkspacePilotEditGate` (`WorkspacePilotSupport.cs:153-`). Con `TryEditAll` con cuerpo por defecto, los dos lo devolverían `NotAvailable` sin reenviar.<br>En PB-T4c **los dos reenvían `TryEditAll`**: los dos entran en el alcance y en la lista cerrada. Además:<br>• CT-PBT4C-12 (Core) prueba la compuerta con el anfitrión real, sin AutoCAD;<br>• CT-PBT4C-13 (guarda de texto) exige el reenvío del adaptador del Plugin, con las 18 operaciones de `ControllerViolations` (PEG:683-695), y falla en el RED;<br>• CT-PBT4C-10 recorre la cadena de punta a punta (FVM → puerto → compuerta → anfitrión → doble del editor) | D-PBT4-13 y 16; §5.3; §6.3 |
| **V3-02** | **B-04 e I-18** (la lista) | **La causa C-0,** «la lectura viene del kit del piloto», con su firma mecánica y los archivos de kit por nombre. **El procedimiento va al Owner como Q4,** con tres opciones; la recomendada es «Editar una línea del kit compartido». **C-5 se mide sobre el texto literal del GREEN** que el contrato entrega. **Medido:** 9 clases y 428 métodos (R-21) | D-PBT4-11; §8.2 Q4; §11 |
| **V3-03** | **I-12** (PB-T4a rompe más) | La elección del origen va **en línea** en cada cuerpo, sin ayudante. Siguen los seis `_editIntentReader!.ReadIntent(`, un `_editIntentReader` seguido de `.` o `!` por cuerpo, y el orden de EIG:563-575, EIG:758-770 y `WorkspaceLifecycleGuardTests:642-656`. **Solo rompe CT-PBT3A-12** | D-PBT4-03; §4.1; §5.1 |
| **V3-04** | **I-13** (la clave tras POST) | **No hay clave que guardar.** La relectura usa el **texto** del origen del borrador, `draft.CurrentDraftState.Origin`. El Plugin elige el único miembro mutable cuyo texto es, byte a byte, ese texto, que es la regla de `ChooseOrigin` de UR:640-650. Sin coincidencia, o con varias, cae en el `Primary` de hoy.<br>Tras un POST que borra el origen, el borrador queda limpio con el origen nuevo (el comparador completo compara `Origin`). No se toca ningún tipo de Application salvo la interfaz nueva, y se quitan el diccionario y sus ayudantes (M-16 y M-18) | D-PBT4-03; CT-PBT4A-03 y CT-PBT4B-17 |
| **V3-05** | **I-14** (roturas por letra) | • **Evitadas por diseño:**<br>  – EIG:670 y :720: la clasificación va en una función pura de `Draft/`, y WHC no nombra la base;<br>  – la paridad del XAML: el XAML no cambia (sin `x:Static`), y CT-PBT4B-04 compara la regla con las propiedades de la caja real;<br>  – EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723`: el abridor es una clase anidada de WCo, sin archivo nuevo en `Plugin/Workspace/`.<br>• **En la tabla:** `WorkspacePilotPrepareTests:1351-1353` (11 estados de `EditIntentStatus`, con `Kept` el último), por `Presented` (C-5, PB-T4b) | D-PBT4-06, 08 y 15; §4.2 |
| **V3-06** | **I-15** | La fila de `RequestUpdate`: `Busy` por la marca (WHC:1388), antes de `PassGate` | D-PBT4-14 |
| **V3-07** | **I-16** | La condición 7 de D2, citada literal, con una línea que discrimina: un rack resuelto de **contenido distinto** en `Current` deja `IsDirty` en `False` | D-PBT4-18; CT-PBT4B-10 |
| **V3-08** | **I-17** (Q2) | Q2 con sus costes medidos: unas 136 a 165 rutas de hoja por diseño, las hojas derivadas y los 320 px. La recomendada pasa a ser **«Por grupos»**. Se dice que la letra de D-PB-05 dice «fields» | §8.2 Q2 |
| V3-09 | M-16 | Desaparece: no hay clave que convertir, porque se pasa el texto | D-PBT4-03 |
| V3-10 | M-17 | El censo de PB-T4a admite solo RIA (más `Draft/`, PP y UR). En PB-T4b, por letra, el de WHC (C-5) | CT-PBT4A-06; §4.2 |
| V3-11 | M-18 | Desaparece: sin diccionario no hay claves que borrar | D-PBT4-03 |
| V3-12 | M-19 | CT-PBT4B-11 con una secuencia fija: el comparador da `False` entre un estado con una longitud de módulo `NaN` y su copia | §5.2 |
| V3-13 | M-20 | Se declara el límite: el testigo cubre solo el sobre del origen | D-PBT4-08 |
| V3-14 | M-21 | Q1: «retira lo que se puede retirar»; la opción (C), sin número, porque no se puede medir sin diseñarla | §8.2 Q1 |
| V3-15 | M-22 | Se cierran con su cita los `<a medir>` que la §13 da por medidos. Quedan solo K, D y E, las líneas de CT-PBT4B-06 y el coste de `IsDirty` | §9 |
| V3-16 | M-23 | PB-T4a se declara **con un cambio observable:** las relecturas pasan del `Primary` al origen del borrador | D-PBT4-01 y 03; §6.1 |

## Registro de cambios v1 → v2

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| V2-01 | **B-01** (el anfitrión no recibe la lectura completa) | **PB-T4a nueva, solo aditiva:**<br>• el adaptador del Plugin (RIA) lee también el modelo completo sobre el mismo escaneo;<br>• `EditIntentReadResult` lleva la lectura completa y el catálogo junto a la del piloto (`FromReads`); `FromPilot` no cambia;<br>• la relectura de un borrador abierto admite el origen.<br>Los archivos del Plugin entran en el alcance; el censo de PB-T3 se amplía; `EditIntentKit` no se toca y hay un kit nuevo | D-PBT4-02 y 03; §6.1 |
| V2-02 | **B-02** (la regla en el Plugin, sin prueba posible) | **La regla única vive en la UI** (`AnnotationScaleRule`, en `src/RackCad.UI/Controls/`), sobre `NumericFieldValidation`. La caja `AnnotationScaleBox` toma de ella sus parámetros (`x:Static`), y la prueba la llama desde la UI. El Plugin solo la conecta al anfitrión (`Func<string, DraftFieldValidation>`, un tipo de `Workspace.Drafts`). La copia del adaptador del piloto se trata en Q1 | D-PBT4-06 |
| V2-03 | **B-03, I-03 e I-04** (la lista) | **El procedimiento de la lista,** con causas cerradas y firmas, y cuatro opciones que la sesión elige. Las preguntas se hacen después de medir | D-PBT4-11; §8.2 |
| V2-04 | **I-01** (K = 1) | **Un solo mecanismo,** con los estados por operación:<br>• el abridor del Plugin pone el modal (`EnterPanelModal()`, el único conductor admitido de la bandera);<br>• `TryEditAll` mantiene la marca de lectura durante `ShowModal`: las operaciones del campo y `DiscardRack` dan `Busy`, y las de intención, `Waiting`, sin comprobaciones nuevas | D-PBT4-15 |
| V2-05 | **I-02** (los miembros nuevos del puerto) | **Sin `PilotBoundaryKind.EditAll`:** el gesto usa `CommitField` como frontera, y URT:1107-1109 sigue en verde.<br>**«Conservar» sin miembro nuevo:** usa `TryKeepMine` en dos tiempos, con el testigo.<br>**El único miembro nuevo es `TryEditAll`** (PB-T4c), que rompe PEG:156-163: va a la lista.<br>Medidas y citadas PEG:156-163, :280-281 y :380-385 | D-PBT4-08, 13 y 14; §4 |
| V2-06 | **I-05** (Q1) | Q1 con huecos, el coste de conservar el piloto (A-5, R-T1-17, la premisa de D-1 y la regla duplicada) y una opción de retirada realista: se retira lo que solo usaba el panel y se conserva el lector | §8.2 |
| V2-07 | **I-06** (D2) | CT-PBT4B-10 con las filas de `DiscardAndReload`, `SetPendingText` y `CommitField`, y la condición 7 (sustituir una de las dos referencias en `Current` no cambia `IsDirty`); la huella, tras cada operación | D-PBT4-18; §5.3 |
| V2-08 | **I-07** (`NaN`) | CT-PBT4B-11 pone el `NaN` en una longitud de módulo del commit (A-9, `RackModuleCommit.SameAs`) | §5.3 |
| V2-09 | **I-08** (los literales) | La presentación, parametrizada por Q2, con la unión, el plural y las vistas rechazadas fijados. Las líneas de CT-PBT4B-06 se miden antes de emitir con el clasificador real | D-PBT4-09 |
| V2-10 | **I-09** (los ayudantes y la disciplina de lectura) | La lista nominal cerrada por contrato. Las reglas de lectura de `TryEditAll`, copiadas de `TryKeepPilotEdit`. El equivalente de PEG amplía sus listas | §6.0; D-PBT4-14 |
| V2-11 | **I-10** (el Smoke-PB) | §5.5 y §10 ampliados con todo lo que solo existe en el Plugin | §5.5; §10 |
| V2-12 | **I-11** (Q2) | Q2 con «Por grupos», «Por campos» y «Por campos, con sus valores», sin «Solo recuentos» ni «no abre diálogos». Recomendada: «Por campos» | §8.2 |
| V2-13 | M-01 | 17 operaciones (PEG:44-65) | §2; R-2 |
| V2-14 | M-02 | G-4 exige que el conjunto de archivos que nombran `PushBackPilotUpdateRun.Execute(` sea solo WHC (IG:497-498) | §4 |
| V2-15 | M-03 | `NotePilotEdit(SessionId)` | D-PBT4-13 |
| V2-16 | M-04 | Las relecturas de A-T3-8 son las de un borrador abierto (WHC:409, :503, :590, :675 y :759), no `Begin` (:328); `Precheck` no lee; POST ya relee por `Origin` | D-PBT4-03 |
| V2-17 | M-05 | Los cinco archivos del camino, nombrados en CT-PBT4C-07 | D-PBT4-16 |
| V2-18 | M-06 | Las cuentas de cada rama, rehechas | §5.4 |
| V2-19 | M-07 | `PushBackDraftSessionPort` y `LaunchFor`: se conservan **con registro** en DISP. A-T3-9: registro en DISP y en `ideas-futuras.md` | D-PBT4-13; D-PBT4-20 |
| V2-20 | M-08 | [PKC2] y F5-A, definidos | Abreviaturas |
| V2-21 | M-09 | El abridor muestra la ventana con `ShowModalWindow`, **sin dueño WPF**, como el clásico (`PushBackClassicCharacterizationPluginGuardTests.cs:196-209`) | D-PBT4-16 |
| V2-22 | M-10 | Las citas corregidas: la forma del resultado (EIH:1175-1194) y el cableado (EIG:1150-1190) | §4 |
| V2-23 | M-11 | La guarda de «sin traducción» mira WHC, WEI, `WorkspacePilotSupport.cs`, RIA y NH | CT-PBT4B-12 |
| V2-24 | M-12 | CT-PBT4C-09 lee `PendingWork()` sin cerrar la ventana; la costura de escenificar se mide; la petición de «Editar todo» y los estados Unknown, Error y Undecidable, fijados | D-PBT4-14 y 17 |
| V2-25 | M-13 | Una medición del coste de `IsDirty` en la corrida de medición (precondición 6) | §7 |
| V2-26 | M-14 | `UNIT_CHANGE` completa. `IPushBackFullEditorHost` se declara en `Application/Workspace` por EIH:1088 (un solo campo de tipo de `Systems`), frente a R-PB-05 y ADR-0029 D12 | §6.0; D-PBT4-15 |
| V2-27 | M-15 | CT-PBT4C-10 lleva una línea tras un gesto compuesto (F4B-E30) | §5.4 |
| V2-28 | §8 de la revisión, lo declarado | Que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro. Y el procedimiento de la lista | D-PBT4-08; D-PBT4-11 |
| V2-29 | La sesión | **Tres subtareas** (PB-T4a, PB-T4b y PB-T4c) y sus TaskId | D-PBT4-01 |

---



## 0. En una página

**Lo que pide el Owner** ([OD], fila de PB-T4): «Add "Editar todo" … Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.»

**La base:** el cierre de PB-T3c, `7f0d9aa3`. **Cuatro subtareas secuenciales,** cada una con su 1/3. Los `TaskId` empiezan por `F4B-PBT4-` y tienen **como mucho 26 caracteres** (MAX_PATH solo acota por arriba; V41-11):

| Subtarea | TaskId | Qué | Sustituciones exactas (§8.3) | La emisión |
|---|---|---|---|---|
| **PB-T4a** | `F4B-PBT4-FULL-READ-PATH` (23) | El Plugin lee también el modelo completo sobre el mismo escaneo; el resultado lleva lo completo y el catálogo. Las relecturas de un borrador abierto van al origen del borrador por su texto, con una relectura pura en `Draft/` sobre el escaneo ya tomado (A-T3-8): es un cambio observable declarado. La firma de `RackPilotIntentAdapter.Read` no cambia | **1** (CT-PBT3A-12) | Condicional (pregunta 4) |
| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala, en Application; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con dos «Sí», la presentación «Por grupos» y `KeepOn` (D-S2); una sola materialización del modelo completo por lectura (D-S4). **El código del piloto queda sin uso, sin retirarlo** | **478** (428 de las 9 clases, «Saltar y duplicar», 356 copias y 72 a mano; 23 de la letra; 27 de la UI), la lista que el Owner tomó como la que vio, con las cinco de la tercera tanda y las dos de la quinta | Condicional, con esa lista, sobre el cierre de PB-T4a (`78a7cef3`) |
| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo», con el abridor en dos piezas | **4** (PEG:155-163, CT-PBT1B-15, CT-PBT4B-20 y CT-PBT2B-06), la lista que el Owner tomó como la que vio tras sus dos decisiones de PB-T4c (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`) | Condicional, con esa lista, sobre el cierre de PB-T4b (`6adff90d`) |
| **PB-T4d** | `F4B-PBT4-RETIRE-PILOT` (21) | Retira el código del piloto que el panel ya no usa, con el alcance B y la rama muerta de `BuildCore`; el RED lleva todo lo de `tests/` (D-S11) | **271** filas con acción de `pbt4d-retire-list-271.tsv` (borrar, reescribir, sustituir y acotar con registro), con las 29 sustitutas de WPUR y los ayudantes sin uso borrados (§4.4; v5.3) | Condicional (`owner-pbt4d-list-decisions.md`, pregunta 4), con esa lista, sobre el cierre de PB-T4c (`6c2820ba`) |

**Las decisiones del Owner,** tomadas (§8.2): Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones» (`owner-pbt4-decisions.md`); después, «Retirar tras PB-T4c» y «Sí, esa lista» (`owner-pbt4b-retire-list-decisions.md`); y, en la tercera tanda, «Añadirlas a la lista» (`owner-pbt4b-breaks-decision.md`). Después, «Cerrar y optimizar en PB-T4b» (`owner-pbt4a-cost-decision.md`) ; en la quinta tanda, «Añadirlas a la lista» (`owner-pbt4b-list-477-decision.md`), y en la sexta, «Añadirla a la lista» (`owner-pbt4b-list-478-decision.md`); y en la séptima, sobre el coste de la lectura de PB-T4b, «Darlo por cumplido» (`owner-pbt4b-cost-decision.md`). Sobre PB-T4c, dos veces «Añadirla a la lista» (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`). La sesión decidió D-S1 a D-S3, D-S5 y D-S6 (`session-pbt4b-design-decisions.md`), D-S4 (D-PBT4-21; §8.5) y, para PB-T4c, D-S7 a D-S10 (`session-pbt4c-design-decisions.md`).

**PB-T4a es la primera que se emite.** Su parte del paquete está completa y sin remisiones: el diseño (D-PBT4-02 y 03, con el texto literal del GREEN y `ReadAt`, B-08), las roturas medidas con el recorrido por marcas de sus guardas y la ampliación de CA-T4a (§4.1), la lista cerrada CT-PBT4A-01 a 08 con sus totales (§5.1), lo común (§6.0) y el contrato propuesto con lo que necesita su generador (§6.1).

## 1. Autoridades

Las de la v1 (§1), sin cambio. Además, la revisión de la v1 y las instrucciones de la sesión. Las principales:

| Autoridad | Lo que manda aquí |
|---|---|
| **[OD]**, la fila de PB-T4, D-PB-01 y D-PB-05 | Un editor completo, modal, con borrador dentro y fuera y cero escrituras en el DWG; los pasos 1 a 7; «an informed destructive choice»; sin traducción de estado |
| **[P]**: OB-01.1 a 6, OB-03.2, OB-05.1 a 5, R-PB-10, §7.3 (OB-T1B.1 a 3) y §7.4 | |
| **[P2] §10** (A2-1 a A2-13) y **[P3] §10** (A-T3-1 a A-T3-11) | |
| **[PT1]**: A-5 («La copia de la carga del piloto … PB-T4 la retira al consumir el API», :1549), A-9 (:1553), A-10 (:1554) y R-T1-17 (:1367) | |
| **F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; E15, E21, E25, E35 Q4, E37, E42, E45, E46 y E48; D-PB-10** | |
| **Las decisiones del Owner sobre PB-T4** (`owner-pbt4-decisions.md`, 2026-11-20) | Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones» (§8.2). Desde la v4.1, Q1 a Q4 siguen la numeración de ese registro (la v4 llamaba Q3 a la emisión y Q4 al procedimiento) |
| **Las decisiones del Owner sobre la retirada y la lista de PB-T4b** (`owner-pbt4b-retire-list-decisions.md`, 2026-11-20) | «Retirar tras PB-T4c» y «Sí, esa lista» (§8.2) |
| **La decisión del Owner sobre las cinco roturas** (`owner-pbt4b-breaks-decision.md`, 2026-11-20) | «Añadirlas a la lista»: 475 en PB-T4b, 138 en PB-T4d y WPUG en 85 (§8.5) |
| **La decisión del Owner sobre el coste de la lectura de PB-T4a** (`owner-pbt4a-cost-decision.md`, 2026-11-20; en `F4B-DRAFTS/coordinator/` del cierre `78a7cef3`) | «Cerrar y optimizar en PB-T4b»: una sola materialización del modelo completo por lectura (D-S4, D-PBT4-21) |
| **La decisión del Owner sobre las dos de PB-T4a** (`owner-pbt4b-list-477-decision.md`, 2026-11-20) | «Añadirlas a la lista»: 477 en PB-T4b (§8.5) |
| **La decisión del Owner sobre la guarda de los textos del anfitrión** (`owner-pbt4b-list-478-decision.md`, 2026-11-20, la sexta tanda) | «Añadirla a la lista»: **478** en PB-T4b (§8.5) |
| **La decisión del Owner sobre el coste de la lectura de PB-T4b** (`owner-pbt4b-cost-decision.md`, 2026-11-21, la séptima tanda) | «Darlo por cumplido»: el paso 7 de la precondición 6, con dos de tres corridas (§8.5) |
| **Las dos decisiones del Owner sobre la lista de PB-T4c** (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`, 2026-11-21, la octava y la novena tandas) | «Añadirla a la lista», dos veces: **4** en PB-T4c (§8.5) |
| **Las decisiones de diseño de la sesión para PB-T4c** (`session-pbt4c-design-decisions.md`, 2026-11-21) | D-S7 a D-S10 (§8.5) |
| **Las decisiones del Owner sobre la lista de PB-T4d** (`owner-pbt4d-list-decisions.md`, 2026-11-22, la 10.ª tanda) | «Alcance B», «Tarea propia después» (la recarga), «Acotarla en PB-T4d» (la prueba lenta) y «Sí, con condiciones» (§8.5) |
| **La decisión del Owner sobre las sustitutas** (`owner-pbt4d-substitutes-decision.md`, 2026-11-22, la 11.ª) | «Sustitutas donde hace falta» (§8.5) |
| **La decisión del Owner sobre la corrida genérica** (`owner-pbt4d-run-coverage-decision.md`, 2026-11-22, la 12.ª) | «Sustituir las 21» (§8.5) |
| **Las dos decisiones del Owner sobre la rama muerta y las 27** (`owner-pbt4d-deadcode-27-decisions.md`, 2026-11-22, la 13.ª) | «Quitarla en PB-T4d» y «Sí, 27» (§8.5) |
| **Las decisiones de diseño de la sesión para PB-T4d** (`session-pbt4d-design-decisions.md`, 2026-11-22) | D-S11 a D-S15 y, tras CA-T4d, D-S16 (§8.5) |
| **La comprobación acotada de la v5.2** (`independent-review-pbt4d-check.md`, 2026-11-22) | EMITIBLE TRAS CORRECCIONES, B 0 / I 2 / M 7: la v5.3 corrige los nueve (registro v5.2 → v5.3) |
| **Las dos decisiones del Owner tras CA-T4d** (`owner-pbt4d-cat4d-decisions.md`, 2026-11-22, la 14.ª) | «Sustituirlas» (la 48 y la 49) y «Sustituirlos siempre» (la regla de los huecos de la cobertura ampliada) (§8.5) |
| **El arrastre A-T2b-OWNER-01** (F4B-E38; `owner-pbt2b-close-decisions.md:36`) | «Cerrar» en WORKSPACE con una sola pregunta (D-PBT4-17) |
| **Las decisiones de diseño de la sesión** (`session-pbt4b-design-decisions.md`, 2026-11-20) y las §17 y §18 de `independent-review-v1.md` | D-S1 a D-S3, D-S5 y D-S6, y D-S4 (§8.5, D-PBT4-21); la verificación de las cinco roturas y la revisión de la v4.4 |

---

## 2. Hechos medidos en `7f0d9aa3`

Medidos en `7dfc7c21` (v1 a v3) y vueltos a medir en `7f0d9aa3`, el cierre de PB-T3c (v4 y v4.1): entre las dos, `src/` y `tests/` solo cambian en el escritor, la fachada y dos guardas de PB-T3c (R-28), así que valen en `7f0d9aa3`. Los de la v1 (R-1 a R-12), corregidos, y los nuevos (R-13 a R-38; los de la v4.1, R-34 a R-38). Lo esencial:

| Hecho | Fuente |
|---|---|
| **El resultado de la lectura solo lleva el piloto:** `EditIntentReadResult(status, reason, PushBackPilotIntentResult? pilotRead)`, con las fábricas `FromPilot`, `NoActiveDocument`, `Busy`, `Rejected` y `Failed` | WEI:35-64 |
| **El Plugin lee:** NH `ReadIntent` → `ReadOnActiveDocument` → `RackPilotIntentAdapter.LoadCatalog()` fuera del bloqueo → `LockDocument` → `ChooseOriginDefinition(revalidation.Fresh, chosenReference)` → `RackPilotIntentAdapter.Read(document, rackId, origin, witnessKey, witnessText, catalog)` → `EditIntentReadResult.FromPilot(read.Pilot)` | NH:207-251; WEI:682-692 |
| **El escaneo** es `new PushBackPilotIntentScan(rackId, originKey, scan.Facts, scan.RawEnvelopes, CompletenessEvidence, witness)`. `PushBackFullDraftReader.Read` recibe ese mismo tipo y llama primero al lector del piloto | RIA:116-121; `Draft/PushBackFullDraftReader.cs:51-59` |
| **`PushBackFullIntentResult` no lleva el testigo;** el resultado del piloto sí (`PreviewWitness`) | `Pilot/PushBackPilotIntentResult.cs:25-59`; `Draft/PushBackFullDraftReader.cs:12-41` |
| **Las seis lecturas del anfitrión:** `ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness)` en WHC:328 (`Begin`), :409, :503, :590, :675 y :759; seis `var pilot = read.PilotRead;` | R-13 |
| **EIG:733-736:** ningún `.PilotRead` fuera de esas asignaciones; exactamente 6 asignaciones y 6 `.ReadIntent(` | EIG:725-737 |
| **EIG:414-430:** NH llama una vez a `RackPilotIntentAdapter.LoadCatalog()` y una vez a `RackPilotIntentAdapter.Read(` | R-14 |
| **EIH:1088:** WHC tiene exactamente un campo cuyo tipo, o algún argumento genérico, es de `RackCad.Application.Systems` (`_updateHost`) | EIH:1075-1099 |
| **EIH:52:** el kit de lecturas de todo el anfitrión construye con `FromPilot`; 12 usos de `FromPilot` en las pruebas | R-13 |
| **Las 17 operaciones del puerto** (PEG:44-65), fijadas por reflexión (PEG:159-161); las seis de lectura (:68); `WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort` y nada más (:380-385); el adaptador del piloto es el único campo estático del anfitrión (:280) | R-15 |
| **URT:1107-1109** fija los 7 nombres de `PilotBoundaryKind` | R-15 |
| **K = 1:**<br>• `SetPendingText`, `CommitField`, `CrossBoundary`, `DiscardRack` y `DiscardDocument` miran la marca `_pilotReadInProgress` y dan `Busy`; no miran el modal;<br>• `PassGate` y `UpdateGate` miran el modal y dan `Waiting`;<br>• `PilotFieldStatus` = { Rejected, Applied, NotEditable, Busy, Invalid } | WHC:1127, :1155, :1208, :1243, :1270, :1940, :996; `IWorkspacePilotEditPort.cs:143-157` |
| **La regla de la UI de F4:** «Asking for an option calls no operation of the port and only reads its memory» | `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs:780-793` |
| **RG:116** muta `public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);` | R-15 |
| **La UI de la reconciliación:** 40 llamadas a `RequestKeepMine` o `ConfirmChoice` en `tests/RackCad.UI.Tests/Workspace/` | R-15 |
| **Las pruebas no referencian el Plugin:** `RackCad.Tests.csproj:23-31`, `RackCad.UI.Tests.csproj:25-27` | R-16 |
| **La caja de la escala:** `<controls:NumericField x:Name="AnnotationScaleBox" … Minimum="0" MinimumInclusive="False" />`; `NumericField` valida con `NumericFieldValidation.Validate` | W.xaml:68-69; `src/RackCad.UI/Controls/NumericField.cs:149` |
| **`IDraftPendingTextAdapter<TState>`:** `DraftFieldValidation Validate(string fieldKey, string text)` y `TState Apply(TState current, IReadOnlyDictionary<string, string> pendingText)`; `DraftFieldValidation(bool IsValid, string? Message)` | `Drafts/DraftSession.cs:95-110` |
| **El modelo completo usa el piloto:** las constantes de `PushBackPilotMaterializer` (en `Draft/` 15 veces) y `PushBackPilotIntentReader.Read` (el lector completo); el lector del piloto usa `PushBackPilotMaterializer.Materialize` | R-17 |
| **`NaN`:** `PalletTolerance` se compara con `double.Equals` (`NaN` es igual a `NaN`); `RackModuleCommit.SameAs` compara longitudes con `!=` | `PushBackEditableStateComparer.cs:409`; `Systems/Shared/RackModuleEditSession.cs:76-` |
| **El abridor clásico** muestra la ventana con `AcApplication.ShowModalWindow(window)` y sin asignar `Owner` | `PushBackClassicCharacterizationPluginGuardTests.cs:196-209` |
| **La suite de UI** corre en un STA compartido con un `MessageBox` modal real | `.github/workflows/ci.yml:213-222` |

---

## 3. El diseño


### D-PBT4-01. Cuatro subtareas secuenciales (V42-01)

**El orden:** PB-T4a sobre el cierre de PB-T3c (`7f0d9aa3`); PB-T4b sobre el de PB-T4a; PB-T4c sobre el de PB-T4b; y PB-T4d, la retirada del piloto, sobre el de PB-T4c, cuando el Owner apruebe su lista. Cada una con su 1/3 (OB-10.1), sin rebase.

**Por qué así:**
- PB-T4a deja el camino de la lectura completa y la relectura con el origen, con **una sola** sustitución por letra;
- PB-T4b concentra el cambio de tipo y su lista;
- PB-T4c es casi aditiva, con cuatro sustituciones con registro (la lista de 4; V50-02);
- PB-T4d retira el código del piloto cuando ya no corre ninguna prueba que lo use (D-PBT4-10).

**Los TaskId** empiezan por `F4B-PBT4-` y tienen como mucho 26 caracteres (23, 25, 24 y, el propuesto de PB-T4d, 21), para los sustitutos de nc1 y nc2 (§6.0; V41-11).

### PB-T4a

#### D-PBT4-02. La lectura completa, junto a la del piloto (B-01; V41-06; V42-02)

**En Application** (WEI), aditivo. El texto literal del GREEN:

```csharp
        private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead, PushBackFullIntentResult? fullRead, RackCatalog? catalog)
        {
            Status = status;
            Reason = reason;
            PilotRead = pilotRead;
            FullRead = fullRead;
            Catalog = catalog;
        }

        /// <summary>I-64 F4B PB-T4a: the read of the full model over the same scan; null unless the read came from <see cref="FromReads"/>.</summary>
        public PushBackFullIntentResult? FullRead { get; }

        /// <summary>I-64 F4B PB-T4a: the catalog of the read; null unless the read came from <see cref="FromReads"/>.</summary>
        public RackCatalog? Catalog { get; }

        public static EditIntentReadResult FromPilot(PushBackPilotIntentResult pilotRead)
        {
            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));
            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);
        }

        public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog)
        {
            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));
            if (fullRead == null) throw new ArgumentNullException(nameof(fullRead));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, fullRead, catalog);
        }
```

- `NoActiveDocument`, `Busy`, `Rejected` y `Failed` pasan `null, null` en los dos argumentos nuevos; su texto no cambia en lo demás.
- Los `using` nuevos de WEI: `RackCad.Application.Catalogs` y `RackCad.Application.Systems.PushBack.Draft`.
- **`FromPilot` no cambia de comportamiento:** deja `FullRead` y `Catalog` en null, así que `EditIntentKit` (EIH:51-52) y los 12 usos de `FromPilot` siguen como están.

**En el Plugin (RIA).** **La firma de `RackPilotIntentAdapter.Read` no cambia** (V41-06): EIG:139-141 la fija entera, hasta `RackCatalog catalog)`, así que añadirle un parámetro, obligatorio o con valor por defecto, rompería esa guarda, que no está en la lista que vio el Owner (contrafactual medido en `measure_pbt4_v41-out.txt`, `measure_pbt4_v42-out.txt` y `measure_pbt4_v43-out.txt`). El texto literal del GREEN:
- el `using` nuevo `RackCad.Application.Systems.PushBack.Draft`, antes del de `Pilot`;
- `RackPilotIntentReadResult` gana un segundo constructor y dos propiedades:

  ```csharp
        internal RackPilotIntentReadResult(PushBackPilotIntentResult pilot, PushBackFullIntentResult full, PushBackPilotIntentScan input)
        {
            Pilot = pilot;
            Full = full;
            Input = input;
            FailureReason = string.Empty;
        }

        internal PushBackFullIntentResult? Full { get; }

        internal PushBackPilotIntentScan? Input { get; }
  ```

  El constructor de hoy, el de las cinco salidas de fallo, no cambia, y esas cinco líneas tampoco;
- en `Read`, la única línea que cambia es la del éxito, que llama una vez al lector completo sobre el **mismo** `input`, justo después del lector del piloto:

  ```csharp
                return new RackPilotIntentReadResult(read, PushBackFullDraftReader.Read(input, catalog), input);
  ```

**Lo que no hace:**
- **Nada traduce:** lo completo sale del lector completo sobre el mismo escaneo (P3:530, A-T3-2), y no del resultado del piloto.
- **Ninguna lectura nueva del dibujo:** el escaneo es el mismo, dentro del mismo bloqueo; no hay segundo `LockDocument` ni segundo `CaptureDrawing`.
- **El anfitrión no usa todavía `FullRead`:** lo usa PB-T4b.

**Lo que necesita `Classify`:**
- los sobres de los miembros salen de `FullRead.StaleBase` (las entradas con su texto);
- el catálogo, de `Catalog`.

**El testigo** se queda en `PilotRead.PreviewWitness`, porque el lector completo lo calcula con el mismo lector del piloto.

**Las guardas del Plugin, medidas sobre el texto literal del GREEN** (§4.1: el recorrido por marcas de las clases que leen los archivos tocados, `walk_guards_pbt4a.py`, con la ampliación de CA-T4a; y las aserciones traducidas de `measure_pbt4_v43-out.txt`, 36 de 36 en verde):
- **EIG:350-364 (B-08):** un solo `new PushBackPilotIntentScan(` y un solo `PushBackPilotIntentReader.Read(` en todo RIA, los dos en `Read` y en su orden; por eso las relecturas puras van en `ReadAt`, en `Draft/`;
- EIG:414-430: NH sigue con una `LoadCatalog()` y una `RackPilotIntentAdapter.Read(`;
- G-9 sigue con sus cuentas: RIA conserva `StartTransaction` 1 y `LockDocument` 1, y NH `LockDocument` 2;
- INV-14 (`AssertNoOpenTransaction` antes y después) sigue: `Read` no cambia salvo la línea del éxito;
- **`WorkspaceCommandAdvisorGuardTests`** (CAG:21-22 y :406-464; antes `<a medir>`, M-28): fija cinco motivos en español en el cuerpo de `Read` de RIA, con dos `ex.Message`, y tres en `ReadOnActiveDocument` de NH. **Sigue en verde:** el GREEN no añade ningún literal de cadena ni ningún `ex.Message` a esos dos cuerpos, y `AtOrigin` no tiene ninguno. Es una restricción para el Worker, que CT-PBT4A-07 comprueba (`literales=0`).

#### D-PBT4-03. La relectura con el origen (A-T3-8; I-12, I-13, M-04, M-16, M-18, M-23; V41-06, V41-07; B-08, M-35)

**El problema.** Las cinco relecturas de un borrador abierto (WHC:409, :503, :590, :675 y :759) pasan `request.ChosenReference`, que el panel deja en null. El Plugin elige entonces el `Primary` (WEI:686), no el origen del borrador. `Begin` (:328) sí debe usar `ChosenReference`.

**La fuente del origen: el texto, no una clave (I-13).**
- Cada estado de borrador ya lleva el texto del sobre de su origen: `PushBackPilotDraftState.Origin` (`Pilot/PushBackPilotDraftState.cs:15-32`) y `PushBackFullDraftState.Origin`.
- La regla es la de `ChooseOrigin` de la corrida (UR:640-650): **el único miembro mutable cuyo texto es, byte a byte, el del origen.**
- **Sin coincidencia, o con varias** (por ejemplo, el origen se editó fuera), la lectura se queda como hoy: la del `Primary`. Lo releído sigue siendo el rack actual, y la detección de conflicto de F4 hace su trabajo.
- **Tras un POST `Applied` que borra el origen** (PB-T3b reancla a la primera vista redibujada, PP:322 y :633), el borrador queda en Clean con la relectura de POST como base. El comparador completo compara `Origin` (`PushBackFullDraftStateComparer.cs:19`), así que `CurrentDraftState.Origin` es el texto del origen nuevo, y la relectura siguiente lo encuentra. Lo fija CT-PBT4B-17.

**La función pura, en `src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs`** (archivo nuevo; el texto literal del GREEN):

```csharp
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the origin of an open draft, found by its text: the key of the ONLY mutable member
    /// whose raw text is, byte for byte, the origin of the draft (the rule of the run, UR:640-650), the same scan at that key, and
    /// the two pure reads over it (the pilot and the full model), as PushBackFullDraftReader does.
    /// </summary>
    public static class PushBackOriginByText
    {
        public static string Choose(DraftStaleBase staleBase, string originText)
        {
            if (staleBase == null || originText == null) return null;
            var matches = staleBase.Entries.Values
                .Where(entry => entry.Membership == DraftMembership.Mutable && string.Equals(entry.EnvelopeText, originText, StringComparison.Ordinal))
                .Select(entry => entry.Key)
                .ToList();
            return matches.Count == 1 ? matches[0] : null;
        }

        public static PushBackPilotIntentScan AtKey(PushBackPilotIntentScan scan, string originKey)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            return new PushBackPilotIntentScan(
                scan.RackId,
                originKey,
                scan.Facts,
                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                scan.CompletenessEvidence,
                scan.PreviewWitness);
        }

        public static (PushBackPilotIntentScan Scan, PushBackPilotIntentResult Pilot, PushBackFullIntentResult Full)? ReadAt(PushBackPilotIntentScan scan, string originText, RackCatalog catalog)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            var key = Choose(PushBackPilotIntentReader.BuildStaleBase(scan), originText);
            if (key == null || key == scan.OriginKey) return null;
            var input = AtKey(scan, key);
            return (input, PushBackPilotIntentReader.Read(input, catalog), PushBackFullDraftReader.Read(input, catalog));
        }
    }
}
```

**`ReadAt` (B-08):** las dos relecturas puras van en `Draft/`, no en RIA, porque EIG:350-364 exige un solo `PushBackPilotIntentReader.Read(` y un solo `new PushBackPilotIntentScan(` en **todo** RIA (`Adapter()`, :356-357), y EIG:465-470 no admite ningún `PushBackPilot` en NH. Devuelve una tupla con nombre (el escaneo, la lectura del piloto y la completa), o `null` si no hay otro origen; no hay tipo nuevo. No añade dependencias: `PushBackFullDraftReader.cs` ya llama a `PushBackPilotIntentReader.Read` desde `Draft/` (:56). Con los dos lectores, el archivo sigue sin ninguna ficha de G-L1 ni ningún término de la pureza (CT-PBT4A-08), como `PushBackFullDraftReader.cs`.

`AtKey` copia los sobres en un diccionario nuevo con el comparador ordinal, como UR (`ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)`). **Sin ningún `?` de tipo de referencia** (CA-T4a I-01; la regla de §5.0): los proyectos tienen `Nullable=disable` (`Directory.Build.props:28`) y la pureza de `Draft/` prohíbe `#nullable`, así que el archivo es «oblivious». Ahí, pasar un `Dictionary<string, string>` al `IReadOnlyDictionary<string, string?>` del constructor no da ningún aviso, y un `string?` daría CS8632, que rompe `build-plugin`. **Ningún término de la pureza de `Draft/`** (CT-PBT3A-14 y CT-PBT4A-08) aparece en el archivo, tampoco en sus comentarios: ni `Task`, ni `Database`, ni `Transaction`, ni `File.`.

**La relectura en el origen, en RIA, sobre el mismo escaneo** (el texto literal del GREEN; va antes de `ReadPreviewWitness`):

```csharp
        /// <summary>
        /// I-64 F4B PB-T4a (D-PBT4-03): the same read at the origin of an open draft, chosen by its text over the scan that
        /// <see cref="Read"/> already took, by the pure reread of the full model (PushBackOriginByText.ReadAt). It reads nothing
        /// more of the drawing. Without exactly one other mutable member with that text, the read stays as it was. An exception of
        /// the pure reread is not contained here, unlike <see cref="Read"/>: it reaches the reader of the host, which answers Failed.
        /// </summary>
        internal static RackPilotIntentReadResult AtOrigin(RackPilotIntentReadResult read, string originText, RackCatalog catalog)
        {
            if (!read.Succeeded || read.Input == null) return read;
            var at = PushBackOriginByText.ReadAt(read.Input, originText, catalog);
            return at == null ? read : new RackPilotIntentReadResult(at.Value.Pilot, at.Value.Full, at.Value.Scan);
        }
```

- **`AtOrigin` solo llama a `ReadAt`** (B-08): RIA se queda con un solo `PushBackPilotIntentReader.Read(`, un solo `PushBackFullDraftReader.Read(` y un solo `new PushBackPilotIntentScan(`, los tres en `Read` (EIG:350-364, medido).
- **No lee el dibujo:** sin `CaptureDrawing`, sin bloqueo, sin transacción, sin literal de cadena y sin `ex.Message` (CT-PBT4A-07). **Una excepción de la relectura pura no se contiene en RIA** (M-35; el `<summary>` lo dice): sube a `ReadIntentAtOrigin` de NH, que la contiene como `Failed(ex.Message)`, igual que `ReadIntent`.
- **El coste:** en el caso del origen, las dos lecturas puras se repiten una vez sobre el escaneo ya tomado; no hay acceso nuevo al dibujo. Va con el coste del Smoke-PB (§5.5).
- RIA no nombra `DraftStaleBase`, `BuildStaleBase` ni `RackCad.Application.Workspace` (EIG:384-388): la base se calcula dentro de `ReadAt`.

**La interfaz nueva,** en `src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs` (archivo nuevo, completo ya en el esqueleto):

```csharp
#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the reader of the edit intention that can read again at the origin of an open draft,
    /// found by its text. It is optional: a reader without it is read as today.
    /// </summary>
    public interface IWorkspaceEditIntentOriginReader
    {
        EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness? witness);
    }
}
```

**El lector anidado de NH** (`PilotIntentReader`, que sigue siendo la única otra clase del archivo, EIG:443). NH no tiene `#nullable`, así que el parámetro nuevo va sin `?` (con `?` daría el aviso CS8632). El texto literal del GREEN, edición a edición:
1. `private sealed class PilotIntentReader : IWorkspaceEditIntentReader, IWorkspaceEditIntentOriginReader`
2. en `ReadIntent`: `return ReadOnActiveDocument(session, rackId, chosenReference, witness, null);`
3. un método nuevo, justo antes de `ReadOnActiveDocument`:

   ```csharp
            public EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness witness)
            {
                try
                {
                    return ReadOnActiveDocument(session, rackId, null, witness, originText);
                }
                catch (System.Exception ex)
                {
                    LogFault("WorkspaceNavigationHost.ReadEditIntent", ex);
                    return EditIntentReadResult.Failed(ex.Message);
                }
            }
   ```
4. `private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness, string originText)`
5. tras la sentencia de `RackPilotIntentAdapter.Read(…)`, dentro del bloqueo:

   ```csharp
                    if (originText != null)
                        read = RackPilotIntentAdapter.AtOrigin(read, originText, catalog.Catalog);
   ```
6. `? EditIntentReadResult.FromReads(read.Pilot, read.Full, catalog.Catalog)` en lugar de `? EditIntentReadResult.FromPilot(read.Pilot)`.

Siguen una sola `LoadCatalog()`, una sola `RackPilotIntentAdapter.Read(` y un solo `_targets.Revalidate(` en NH (EIG:414-431), un `read.Pilot` y ningún `PushBackPilot` (EIG:465-467), y la forma exacta de cada `_authorize(` del lector (RG:696-712).

**En línea en cada cuerpo de WHC, sin ayudante (I-12).** En las cinco relecturas, la sentencia de la lectura pasa a este texto literal (el cuerpo ya tiene `draft` de `_drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId)` antes de leer, medido):

```csharp
                var read = _editIntentReader is IWorkspaceEditIntentOriginReader originReader
                    ? originReader.ReadIntentAtOrigin(request.Session, request.RackId, draft.CurrentDraftState.Origin, request.Witness)
                    : _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
```

Lo que mantiene en verde (medido sobre este texto, §4.1):
- **los recuentos:** siguen los seis `.ReadIntent(` (EIG:736), un `_editIntentReader` seguido de `.` o `!` por cuerpo (EIG:573-574, :767, WLG:656) y el orden (EIG:568-569, :761-763, WLG:646-654). `ReadIntentAtOrigin(` no casa con `\.ReadIntent\s*\(`, y `_editIntentReader is` no casa con `_editIntentReader\s*[.!]`;
- **la base:** `draft.CurrentDraftState.Origin` no es una lectura de la base (`BaseOrReadUse`, EIG:648), y WHC ya nombra `draft.CurrentDraftState` (WHC:1772);
- **la reflexión sobre WHC** (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503, IG:427-428): la variable de patrón `originReader` no es un miembro; WHC no declara nada nuevo;
- **los dobles existentes del lector** (`EditIntentFakeReader`, `PilotEditAuditedReader`) no implementan la interfaz nueva: siguen por la rama de hoy.

**El cambio observable (M-23).** En AutoCAD, las relecturas pasan del `Primary` al origen del borrador. Es lo que pide A-T3-8, y se declara en el `Objective` de PB-T4a y en el Smoke-PB.

**En PB-T4b** (D-S4, D-PBT4-21), la lectura completa del `Primary` se difiere en RIA, y con el origen encontrado no se materializa. **D-S4 no toca** `AtOrigin`, NH, `ReadAt` ni las cinco relecturas de WHC (CA-T4b M-04; V49-05); WHC sí cambia en el GREEN de PB-T4b, por D-PBT4-04 y D-PBT4-08.

### PB-T4b

#### D-PBT4-04. El borrador del panel es el completo (A-T3-1; D-S3, V44-06)

**El anfitrión** (el texto literal, en `green_pbt4b.py`, `WHC_SUBS`):
- **`Begin`:** `_drafts.Begin(request.Session, request.RackId, full.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy)`.
- **La admisión y su diagnóstico** salen de la lectura completa (`full.Read`, `full.Detail`, `full.ClassicEditorHint`); también el diagnóstico de admisión de «Comprobar ahora» y de la comprobación general (`full.Read.IsAdmitted ? null : full.Read.Admission.Diagnostic`), porque el del piloto diría «fuera del piloto» de un rack A/B editable (OB-03.2). Es la causa de EIG:871 y EIG:883 (§4.2).
- **Sin `FullRead`** (un lector que devuelve `FromPilot`): `Begin` no crea borrador y responde `NotAdmitted` con «No editable aquí: la lectura no trae el modelo completo.» (`FullReadMissingReason`, una constante privada nueva de WHC) y la pista del editor clásico; las cinco relecturas responden `Failed` con el mismo texto.
- **Los nueve `Find<PushBackPilotDraftState>`** pasan a `Find<PushBackFullDraftState>`; `FindPilotDraft` y `ViewOfPilotDraft` toman `DraftSession<PushBackFullDraftState>`, y la escala que se muestra es `Editable.AnnotationScale`.
- **Las seis lecturas** ganan `var full = read.FullRead;`. **El testigo de la vista previa se sigue leyendo con `var pilot = read.PilotRead;` y `pilot.PreviewWitness`, en las cuatro sentencias que lo usan:** `Begin`, la recarga, «Conservar» y «Descartar y recargar» (WHC:344, :523, :699 y :785 de la base). La v4.3 decía `read.PilotRead.PreviewWitness` «en las tres que lo usan»: son cuatro, y esa forma rompía la `Theory` EIG:743 (`TheAliasGuardDetectsARenamedWay…`, fuera de toda lista), que no admite `.PilotRead` en WHC más que en la asignación (D-S3). «Comprobar ahora» y la comprobación general no leen el testigo y no nombran `pilot`.
- **WHC solo nombra `full.Read` y `full.StaleBase` en los consumidores que admite la guarda de la base** (`Begin`, `CheckNow`, `ConfirmBase`, `DiscardAndReload`, `KeepMineOnCurrentVersion`) y en la expresión del diagnóstico de las dos comprobaciones; **no nombra `.InitialState`, `.InitialDraftState` ni `.Entries`** (medido con el recorrido, §4.2). Por eso la clasificación de «Conservar» y su admisión van en la función pura de `Draft/` (D-PBT4-08).

**Nada traduce:**
- un borrador del piloto no existe tras el GREEN, porque el tipo se fija en `Begin`;
- el registro se vacía al cerrar el documento, como hoy.

**El código del piloto queda sin uso, y lo retira PB-T4d, tras PB-T4c** (D-PBT4-10; V42-01).

#### D-PBT4-05. OB-03.2

Explícito y con su prueba:
- un rack A/B del corpus (`pb-20-ab` y los demás compuestos) abre su borrador en el panel;
- `pb-21-cola` sigue rechazado con `OutOfPilotDiagnostic` y su `ClassicEditorHint`;
- AUTH-13 sigue: el lector completo pasa primero por el del piloto.



#### D-PBT4-06. La regla única de la escala, en Application (B-02; B-07; Q-PBT2-3)

**Una sola implementación, movida y no copiada:**
- **La lógica pura** de `NumericFieldValidation.Validate` (`src/RackCad.UI/Controls/NumericFieldValidation.cs:61-`; solo usa `System.Globalization` y `RackCad.Application.Formatting.LocalizedNumberParser`) pasa a `src/RackCad.Application/Formatting/NumericTextRule.cs`, al lado del analizador que ya usa:
  - `public static NumericTextCheck Check(string text, bool isOptional, bool integerOnly, double? minimum, double? maximum, bool minimumInclusive, bool maximumInclusive)`;
  - `NumericTextCheck`, un `readonly struct` con constructor interno y `Status` (`NumericTextStatus`: los seis estados de `NumericFieldStatus`, en el mismo orden), `Value` (`double?`), `Message` e `IsValid`; la UI traduce el estado por su posición (V44-09; el texto literal, en `green_pbt4b.py`);
  - los dos mensajes como constantes, «Escribe un valor.» y «Valor no numérico.», y los de rango, tal cual.
- **`NumericFieldValidation` de la UI** conserva su API pública (`Validate`, `EmptyRequiredMessage`, `NotANumberMessage`, `NumericFieldValidationResult` y `NumericFieldStatus`) y **delega** en `NumericTextRule.Check`. Sus constantes pasan a ser `= NumericTextRule.EmptyRequiredMessage` y `= NumericTextRule.NotANumberMessage`.
- **`PushBackAnnotationScaleRule`,** en `src/RackCad.Application/Systems/PushBack/Draft/`:
  - `Minimum = 0.0` y `MinimumInclusive = false`;
  - `public static DraftFieldValidation Validate(string text)`, que es `NumericTextRule.Check(text, false, false, Minimum, null, MinimumInclusive, true)` traducido a `DraftFieldValidation`.

**Sin cableado:** WHC crea el adaptador del campo con esa regla por defecto. No hay campo nuevo en WHC ni asignación en WCo, y WCo no nombra `RackCad.UI` (`WorkspaceHostSourceGuardTests.cs:398-420`). **Desaparece la causa C-8:** Core tiene la regla.

**Las guardas, medidas (R-30):**
- ninguna prueba lee el texto de `NumericFieldValidation.cs` ni de `NumericField.cs`;
- las 27 que nombran `NumericField` o `NumericFieldValidation` los llaman por su API pública, que no cambia;
- `NumericFieldValidationTests` (UI) prueba la regla directamente: sigue en verde porque el comportamiento es el mismo, y es la prueba de que la delegación no cambia nada;
- **ninguna guarda de T1B** fija el texto de esos archivos: `PushBackClassicCharacterizationSourceGuardTests` lee `src/RackCad.UI/Systems/PushBack` (:171), no `Controls/`.

**El XAML no cambia:** `WorkspaceDraftPilotValidationParityTests.cs:80-97` sigue en verde. CT-PBT4B-04 compara `PushBackAnnotationScaleRule` (`Minimum` y `MinimumInclusive`, y la validación en los diez textos de CT-PBT2B-10) con las propiedades de `AnnotationScaleBox` en una ventana real.

**El adaptador nuevo,** `Draft/PushBackFullFieldAdapter`, llama a `PushBackAnnotationScaleRule.Validate`.

**La copia del piloto** de la regla (`PushBackPilotFieldAdapter`) se retira con él, en PB-T4d (D-PBT4-10); hasta entonces queda sin uso, y la única regla que usa el panel es `PushBackAnnotationScaleRule`.

**El esqueleto del RED de PB-T4b no toca `NumericFieldValidation.cs`** (M-29): crea `NumericTextRule.cs` y `PushBackAnnotationScaleRule.cs` inertes, y la delegación de `NumericFieldValidation.Validate` en `NumericTextRule.Check` es del GREEN.

#### D-PBT4-07. «Actualizar» con la escritura completa

- `TryUpdatePilot` conserva su nombre y su firma (G-10 y los llamadores de EIG).
- Su cuerpo pasa a `PushBackPilotUpdateRun.Precheck(draft)` y `.ExecuteFull(draft, request, port)`.
- **Desde el cierre de PB-T3c** (`7f0d9aa3`), el escritor borra vistas: `RackPilotMutationScope` implementa `IPushBackPilotViewEraser` con `RackViewBlockEraser`.
- **G-4** (IG:497-498) espera que WHC sea el único archivo que nombra `PushBackPilotUpdateRun.Execute(`. Tras el cambio, ninguno lo nombra: va a la lista por letra (§4.2).


#### D-PBT4-08. «Conservar mi borrador» con dos «Sí» (OB-05.3 a 5; A-T3-3; I-02, I-14, M-20; D-S2, V44-02)

**Lo decidido por la sesión** (`session-pbt4b-design-decisions.md`, D-S2): el flujo que deja en verde las guardas del lector de la elección, que no están en ninguna lista. `ReaderViolations` (RG:69-100) exige que FVM nombre `TryKeepMine` una sola vez, y solo como `_port.TryKeepMine(request)` dentro de `ConfirmChoice`; RG:132 muta la línea literal `var outcome = kind == ChoiceKind.KeepMine ? _port.TryKeepMine(request) : _port.TryDiscardAndReload(request);`. El paso 1 de la v4.3 (que `RequestKeepMine` llamara al puerto) rompía RG:104 y RG:128. **Rechazada:** que el núcleo conserve también sin testigo; contradice «`KeepOn` solo tras confirmar con el testigo vigente».

**Por `TryKeepMine`, sin miembro nuevo del puerto.** La petición sin testigo presenta, y con el testigo vigente conserva:
1. **«Conservar mi borrador»** (`RequestKeepMine`) **sigue preguntando desde la memoria** (`AskChoice`), como hoy: no llama al puerto.
2. **El primer «Sí»** (`ConfirmChoice`) llama a `_port.TryKeepMine(request)` **sin testigo** (la petición es `new EditIntentRequest(session, rack, null, kind == ChoiceKind.KeepMine ? keepWitness : null)`, y `keepWitness` aún es null). El anfitrión (`TryKeepPilotEdit`):
   - **paso 1:** relee con el origen (D-PBT4-03), con la detección de conflicto de F4 y las mismas comprobaciones de antes de leer;
   - **paso 2:** tras la lectura, si el borrador ya no está registrado o no está en Dirty ni en Conflict, `Waiting` con el texto de hoy; si no, clasifica con la **función pura de `Draft/`**:
     ```csharp
     var presentation = PushBackKeepMinePresentation.Read(draft, full, read.Catalog!);
     if (!presentation.IsAdmitted)
         return EditIntentOutcome.NotAdmitted(presentation.Diagnostic, full.Detail, full.ClassicEditorHint, witness);
     return EditIntentOutcome.Presented(presentation.Lines, new EditIntentWitness(full.OriginKey, presentation.OriginText), witness);
     ```
     `Read` clasifica con `PushBackConflictClassifier.Classify(draft.InitialDraftState, read.Read.InitialState, draft.CurrentDraftState, los sobres mutables de read.StaleBase, catalog)`. **WHC no nombra `.InitialDraftState`, `.StaleBase` ni `.Entries`** fuera de los consumidores, y la admisión de la relectura la decide `Read` (`Diagnostic`, null si se admite);
   - devuelve **`Presented`**, el estado nuevo, al final del enumerado, con las líneas y el testigo `EditIntentWitness(clave del origen, texto del sobre releído)`. **No cambia el borrador** ni la marca de conocido limpio.
3. **La UI** vuelve a hacer la pregunta (`AskChoice`) con las líneas y el testigo de esa lectura (`_keepLines` y `_keepWitness`), y muestra las líneas en un `TextBlock` nuevo, `BrowserFieldKeepMinePresentationText`, dentro del panel de la elección, después de la pregunta y **encima del Sí/No**. Si la situación ya no ofrece «Conservar», aviso `SituationChangedText` y sin elección.
4. **El segundo «Sí»** llama a `_port.TryKeepMine(request)` **con el testigo**. El lector lo compara con lo que hay (`PreviewWitness`):
   - **`Unchanged`** (y la petición traía testigo): `draft.KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true)`, y `Kept`;
   - **cualquier otra cosa** (`Changed`, o un lector que no evaluó el testigo): vuelve a presentar, con las líneas y el testigo nuevos (paso 2), sin conservar.
5. **«No»** no llama al puerto, como hoy.

**El cambio visible, declarado** (PBT4B-C03; F4B-E52): «Conservar mi borrador» pide **dos «Sí»**: el primero muestra los cambios por grupos encima del Sí/No y el segundo conserva. Cumple OB-05.4: la confirmación explícita va después de la presentación, y sin elección no se escribe. El número de clics no lo fija ninguna decisión del Owner.

**El texto literal** del GREEN (`green_pbt4b.py`): `WHC_SUBS` (`keep` y `keep-detalle`), `FVM_SUBS` (los campos `_keepWitness` y `_keepLines`, las propiedades `KeepMinePresentationText` y `ShowKeepMinePresentation`, la captura `var keepWitness = _keepWitness;` al empezar `ConfirmChoice`, la petición, el caso `Presented` de `PresentChoice` y los dos avisos de `RaiseAll`) y `PV_NEW` (el `TextBlock`, con `Visibility` sobre `ShowKeepMinePresentation`). **Medido** (`v44/measure_rg_pbt4b-out.txt`): RG:104 da 0 violaciones en la base y en el GREEN; las líneas que mutan RG:116, :117, :120 y :132 a :134 están una vez cada una; y las mutaciones de RG:116 y RG:132 siguen viéndose.

**Lo que rompe por letra** (en la tabla de §4.2): las 15 de la v4.3 que siguen cayendo, y **siete copias mecánicas** que fijaban que `TryKeepPilotEdit` sin testigo conserva o que `EditIntentStatus` tiene 11 estados (D-S1, D-PBT4-11): pasan a equivalentes escritos a mano.

**Lo declarado:**
- pedir «Conservar» pasa a leer el dibujo en el primer «Sí», frente a la regla de F4 de la UI :780-793 (su copia, sin cambios, sigue en verde porque `RequestKeepMine` no llama al puerto). Lleva su registro (V2-28);
- **M-20:** el testigo cubre solo el sobre del origen. Si cambia otro miembro entre la presentación y el segundo «Sí», la presentación queda vieja en ese miembro. Aun así, lo que se conserva parte de la relectura del segundo «Sí», y POST vuelve a comprobar. Se declara como límite.

#### D-PBT4-09. La presentación «Por grupos» (Q2 del Owner; I-08; V41-05)

**Lo decidido:** «Por grupos (Recomendado)» (`owner-pbt4-decisions.md`, pregunta 2). Las formas «Por campos» y «Por campos con valores» no se hacen: no hay tabla de nombres de campos (`PushBackFieldLabels`) ni forma como parámetro (`PushBackPresentationForm`).

`PushBackKeepMinePresentation.Of(PushBackConflictClassification c)` es pura, en `Draft/PushBackKeepMinePresentation.cs`. Devuelve `Lines` (`IReadOnlyList<string>`) e `IsEmpty`. `PushBackKeepMinePresentation.Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)` clasifica (D-PBT4-08) y llama a `Of`; con una relectura admitida guarda además `OriginText`, el texto del sobre del origen de la relectura, que es el del testigo, y con una que no se admite no clasifica y devuelve su `Diagnostic` (`IsAdmitted` falso), para que WHC no nombre la lectura fuera de sus consumidores (D-S3, V44-06).

**Los nueve nombres fijos de grupo,** en el orden de `PushBackChangeGroup` (`PushBackConflictClassifier.cs:14`), literales:

| `PushBackChangeGroup` | Literal |
|---|---|
| `Name` (0) | `el nombre` |
| `Rack` (1) | `el rack` |
| `Annotations` (2) | `las anotaciones` |
| `Safety` (3) | `la seguridad` |
| `Fronts` (4) | `los frentes` |
| `Modules` (5) | `los módulos` |
| `SideB` (6) | `el lado B` |
| `Composite` (7) | `la parte compuesta` |
| `OutsideEditor` (8) | `fuera del editor` |

**La enumeración de una lista de grupos:** cada grupo una sola vez, aunque tenga varias entradas, en el orden de la tabla; con su literal; unidos así:
- uno: `A`;
- dos: `A y B`;
- tres o más: `A, B y C` (comas entre todos menos los dos últimos, que van con ` y `).

Ningún literal empieza por «i» ni por «hi», así que la conjunción es siempre ` y `.

**Las líneas, en este orden y solo si tienen contenido:**

| Línea | Cuándo | Texto literal |
|---|---|---|
| 1 | `ChangedExternally` no está vacía | `Cambiado fuera: <enumeración>.` |
| 2 | `ChangedByUser` no está vacía | `Cambiado por ti: <enumeración>.` |
| 3 | `Overlapping` no está vacía | `Cambiado en los dos: <enumeración>.` |
| 4 | `Structural` no está vacía | `Cambios estructurales: <enumeración de los grupos de las entradas estructurales>.` |
| 5 | `ViewsToErase` tiene 1 | `Vistas: se borrará 1 lateral obsoleto.` |
| 5 | `ViewsToErase` tiene n > 1 | `Vistas: se borrarán <n> laterales obsoletos.` |
| 6 | `ViewsRejected` tiene 1 | `Vistas: 1 no se puede conservar.` |
| 6 | `ViewsRejected` tiene n > 1 | `Vistas: <n> no se pueden conservar.` |
| 7 | `AllViewsObsolete` | `Todas las vistas quedarían obsoletas.` |

- `<n>` va en cifras, con cultura invariante.
- `ViewsRejected` son claves y no motivos: la línea dice cuántas, no cuáles.
- **El punto final,** siempre, una sola vez por línea.
- `IsEmpty` es verdadero si no hay ninguna línea.

**Las vistas, en hasta tres líneas** (M-02 de la §18; V45-06): las líneas 5, 6 y 7 son independientes (se borran laterales, hay vistas que no se pueden conservar y todas quedarían obsoletas), así que las vistas ocupan de cero a tres líneas, y la presentación entera, hasta siete. La opción del Owner decía «más una línea de lo estructural y otra de las vistas»; **la sesión las deja en hasta tres, porque cada una dice una cosa distinta que el usuario decide por separado, y lo declara en F4B-E52.** Medido: CT-PBT4B-06 fija la clasificación fabricada con las tres (`of lineas=7`), y la sonda de la precondición 6 mide el caso del anfitrión (ninguna).

**El ejemplo de la pregunta 2,** con un cambio de fuera en el nombre y en los frentes: `Cambiado fuera: el nombre y los frentes.`, más la línea 4 si el cambio de los frentes es estructural.

**Las líneas de CT-PBT4B-06** (§5.2): las de la función pura, literales, sobre una clasificación fabricada; y las del anfitrión, con un cambio que no toca los frentes (el nombre, fuera; la escala, en el borrador), deducidas del clasificador real, porque cambiar los frentes cambia también hojas de otros grupos (P3:713). Se confirman en la precondición 6 con su sonda (`pbt4b-ct06-presentar-probe.cs`, §6.2.1 y §11); el contrato de PB-T4b las lleva literales.

#### D-PBT4-10. El código del piloto se retira en PB-T4d, tras PB-T4c (Q1 del Owner y su segunda decisión; I-26; V42-01)

**Lo decidido:**
- «Retirarlo (Recomendado)» (`owner-pbt4-decisions.md`, pregunta 1): «Se retira lo que solo usaba el panel»; del modelo paralelo «solo queda la carga, dentro del lector, porque el modelo completo la usa»;
- **«Retirar tras PB-T4c (Recomendado)»** (`owner-pbt4b-retire-list-decisions.md`, pregunta 1): «El código del piloto NO se retira en PB-T4b. Queda sin uso hasta una subtarea propia, PB-T4d, que va después de PB-T4c.» «PB-T4d vuelve al Owner con su lista antes de emitirse. La autorización condicional de `owner-pbt4-decisions.md` (pregunta 4) no la cubre.»

«Conservarlo sin uso», «Dos modelos hasta F5-A» y «Retirar en PB-T4b» no se hacen.

**En PB-T4b y PB-T4c, el código del piloto queda sin uso, y no se toca:**
- el panel deja de usarlo en PB-T4b (D-PBT4-04 a 08), pero ningún archivo de `Pilot/` cambia por la retirada, y sus pruebas siguen en verde;
- **PB-T4b no borra ni edita ninguna prueba ni ningún ayudante:** solo pone los `Skip` de su lista (D-PBT4-11), como en D-1;
- la regla de la escala queda, mientras tanto, también en `PushBackPilotFieldAdapter`, sin uso; la única que usa el panel es `PushBackAnnotationScaleRule` (D-PBT4-06).

**Lo que retira PB-T4d** (v5.3; el alcance B, `owner-pbt4d-list-decisions.md`, pregunta 1, y la rama muerta, `owner-pbt4d-deadcode-27-decisions.md`, pregunta 1):
- el comparador (`PushBackPilotStateComparer`, con su tipo anidado `StructuralEquality`) y el adaptador del campo (`PushBackPilotFieldAdapter`): sus dos archivos se borran;
- `KeepPilotEditsOn` y `ComposeForWrite`;
- **lo que queda sin uso en `src/`** (el alcance B): la composición del piloto (`PushBackPilotComposition`), su instantánea (`PushBackPilotPrepareInput`) y su `ComparePost`;
- `PushBackPilotUpdatePlanner.Prepare` y su `Build` privado (del piloto);
- `PushBackPilotUpdateRun.Precheck(DraftSession<PushBackPilotDraftState>)` y `Execute` (del piloto);
- **en `BuildCore`, el modo que no borra vistas:** el parámetro `allowErase` y lo que solo existe con él (sin el `Build` del piloto, `BuildFull` es su único llamador, con `true`): la condición `allowErase` de sus cuatro condiciones (tres en el bucle, `NoAddress`, `ObsoleteLateral` y `FrontalBOfOneWay`, y la de `AllObsolete`; CA-T4d M-05 b), la rama de la vista no disponible y sus dos constantes (`ViewUnavailablePrefix` y `ViewUnavailableSuffix`) y el nombre de cada sobre releído;
- y los cuatro `cref` y el comentario que nombran lo retirado (D-S15; solo comentarios), y los comentarios que describían como presentes la corrida, el PREPARE y la composición para escribir del piloto (CA-T4d M-05 a; solo comentarios).

El texto literal es la pieza del GREEN (`pbt4d-green-v54.txt`; el anexo V54-A). **El censo de C13** lo cuenta: 57 declaraciones quitadas, ninguna nueva (§6.4.1).

**Lo que se queda,** porque el modelo completo lo usa (R-17):
- el lector del piloto (`PushBackPilotIntentReader`) y su materialización (`PushBackPilotMaterializer.Materialize`);
- el tipo `PushBackPilotDraftState` que devuelve ese lector;
- las constantes de `PushBackPilotMaterializer`;
- `PrepareFull`, `PushBackFullPrepareInput`, `BuildFull` y `BuildCore`, y `ExecuteFull`, el `Precheck` del borrador completo, `ComparePostFull`, `ComparePostCore` y `DesiredNamesOf`.
- **La copia de la carga de A-5** (PM:150-172, dentro de `Materialize`) no se puede retirar entera, porque el lector la usa. Se declara: queda solo como la admisión del lector.

**La lista de PB-T4d** (§4.4 y §6.4; `pbt4d-retire-list-271.tsv`), medida sobre el cierre de PB-T4c compilando y ejecutando, desde cero bajas:
- **se quedan sin tocar** 7 CT de PB-T3b y sus 6 ayudantes, que pasan sobre el modelo completo (`owner-pbt4d-substitutes-decision.md`);
- **se reescriben con registro** las pruebas que también prueban código que se queda (CT-PBT3B-05, 06, con su corpus (iii) sobre el modelo completo (CA-T4d M-01), y 09, E-3, E-5 y E-6 de D-1, H-06 de WPPG, la regla del adaptador de PEG, Ownership, StaleBase, `MaterializeReproduces…`), los vacíos de M-04 (D-S13) y la prueba lenta, acotada (D-S14);
- **se sustituyen con registro** las 29 de WPUR que cubren líneas o ramas de código que se queda que nada más cubre (sobre el modelo completo, en `PushBackRetirePilotRunTests`): 27 de la corrida genérica, sobre `ExecuteFull`, y las n.º 48 y 49, de `PushBackPilotSystemRebuild` (CA-T4d I-01); y las tres que caen al ejecutar (`Skip` y equivalente, D-S12);
- **se borran con registro** las demás: lo que solo fija lo retirado o pasa en vacío, las 55 originales saltadas desde PB-T4b y sus ayudantes, la `Theory` de UI de SUS-01, lo que deja de compilar con la retirada B y, para que no quede código muerto en `tests/`, los ayudantes que la retirada deja sin uso y las tres clases que deja sin pruebas (CA-T4d M-03);
- queda una sola regla de la escala.

#### D-PBT4-11. La lista de sustituciones, «Saltar y duplicar» y el guion de la copia (B-03, B-04, B-06, I-03, I-04, I-18, I-21, I-22, I-23; V41-03 y V41-04; V42-01, V42-03, M-33, M-34; D-S1, V44-01, V44-03 a V44-05; V45-01, V45-05, V45-07, V45-08; V46-01, V46-02; V47-01; V48-01 a V48-03; V49-01)

**Lo decidido:**
- «Saltar y duplicar (Recomendado)» (`owner-pbt4-decisions.md`, pregunta 3): «Como en D-1, las 428 se saltan y se copian con un kit nuevo, y un guion comprueba que cada copia solo cambia el kit […]. No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes.» «Editar una línea» e «Implementación de referencia» no se hacen: EIH no se edita;
- «Sí, esa lista (Recomendado)» (`owner-pbt4b-retire-list-decisions.md`, pregunta 2): la lista de 470, «la que vio». «Una prueba que falle fuera de ella vuelve al Owner.»;
- **«Añadirlas a la lista (Recomendado)»** (`owner-pbt4b-breaks-decision.md`): **la lista de PB-T4b pasa de 470 a 475** con EIG:871, EIG:883, WLG:627, WLG:642 y WPUG:823, que se saltan y se sustituyen con registro por equivalentes que fijan lo mismo sobre el modelo completo; la quinta sale de la lista de PB-T4d (139 → 138), y el `MinSelected` de `WorkspacePilotUpdateGuardTests` baja de 86 a 85;
- **D-S1 de la sesión** (`session-pbt4b-design-decisions.md`): las siete «copias mecánicas» que fijan lo que PB-T4b cambia a propósito, y las que encuentre la precondición 6 al ejecutar las copias sobre el GREEN literal, pasan a **equivalentes escritos a mano, con su registro y con su texto literal en el paquete**. La lista de pruebas sustituidas no cambia por eso; cambia cómo se escribe su sustituta;
- **«Añadirlas a la lista (Recomendado)»** (`owner-pbt4b-list-477-decision.md`, la quinta tanda): **la lista pasa de 475 a 477** con **CT-PBT4A-03** y **CT-PBT4A-05**, dos CT de PB-T4a cuyo código real (el cierre `78a7cef3`) la v4.4 no tenía: su ayudante `PushBackFullReadRig` busca el borrador con `Find<PushBackPilotDraftState>` (FRS:100), y el lector de CT-05, `EditIntentFakeReader`, responde `FromPilot`, sin la lectura completa (EIH:22, :49-52); con el borrador completo, `Begin` responde `NotAdmitted` (`FullReadMissingReason`) o el borrador no se encuentra. Para que pasaran, PB-T4b tendría que abrir un borrador del piloto. Cada una, con su `Skip` y **un equivalente escrito a mano que fija lo mismo sobre el borrador completo**.

- **«Añadirla a la lista (Recomendado)»** (`owner-pbt4b-list-478-decision.md`, la sexta tanda; V48-03): **la lista pasa de 477 a 478** con `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept` (WHHG:491-499), en C-5. La guarda cuenta cuántas veces está exactamente cada texto en español en WHC y su soporte, y el GREEN cambia tres cuentas por el diseño ya decidido (D-PBT4-04 y D-PBT4-08): «La relectura no trajo resultado.» 2 → 0, «El borrador se está aplicando o ya no está registrado.» 5 → 6 y «El rack no tiene escala de anotación.» 1 → 0. Su equivalente, `WorkspaceHostHardeningGuardFull`, lleva la misma tabla con esas tres cuentas, cada una con su `// before N` (el anexo V49-A).

**La lista de PB-T4b, fijada por sha256** (V48-03): **`pbt4b-substitution-list-478.tsv`**, sha256 **`8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b`** (105 379 bytes, LF, 478 filas y la cabecera): la de 477 (`pbt4b-substitution-list-477.tsv`, `5975c5b6…a651`), sin cambio y en su orden, con la fila de WHHG al final; la genera `make_list_478.py` sobre `78a7cef3` (`v48/make_list_478-out.txt`). **La de 477** (105 044 bytes, que generó `make_list_477.py`, `v45/make_list_477-out.txt`, con las 475 y las dos de PB-T4a al final) la de 475 (`pbt4b-substitution-list-475.tsv`, `654c3b7b…0707c`), sin cambio y en su orden, con las dos filas al final, con la misma forma; la genera `make_list_477.py` sobre `78a7cef3` (`v45/make_list_477-out.txt`). **La de 477, la de 475 y la de 470** (`pbt4b-substitution-list.tsv`, `44393b19…63e4`) quedan como historia y no cambian. **La fila de CT-PBT4A-06** sigue nombrándola por su prefijo (`CT_PBT4A_06 (nueva en PB-T4a)`), porque su sha256 la fija; su nombre real, en `78a7cef3`, es `CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes` (FRG:98), y `pbt4b_skips_v45.py` la resuelve por el prefijo. **La de PB-T4d** sigue en **`pbt4d-retire-list-138.tsv`**, sha256 **`d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`**; `pbt4d-retire-list.tsv` (`1c9d67e8…8b71`) queda como historia.

| Lista | Pruebas | Su sustituta |
|---|---|---|
| **C-0**, las 9 clases | 428 | **356 copias mecánicas** y **72 equivalentes escritos a mano** (los 53 de la v4.3, los 7 de D-S1, los 8 de la v4.6, los 2 de D-S5 y los 2 de la v4.8, D-S1), por el guion de la copia |
| **C-5**, la letra de las guardas | **23** (las 15 de la v4.3, las 5 de la tercera tanda, las 2 de PB-T4a y la de la sexta tanda) | **CT-PBT4A-06:** CT-PBT4B-13. **Las otras 22:** su equivalente, una copia parcial de su clase con el texto literal del paquete |
| **UI**, la interfaz de «Conservar» | 27 | Su copia, sin cambios: con D-S2, `RequestKeepMine` no llama al puerto y su texto sigue valiendo |
| **Total** | **478** | **Core 451 y UI 27**, en 21 archivos |

**Las 28 copias sin cambio (M-03 de la §18; V45-07):** las 27 de la UI y RG:109-123 se saltan y se sustituyen por una copia idéntica, porque con D-S2 su texto sigue valiendo. **Se quedan en la lista**, declaradas en el registro como copias sin cambio (§6.2.1 `[13]`): la lista es la que el Owner vio en sus tandas, y una copia idéntica que cayera en el GREEN sería D-S1 (un equivalente a mano y una versión del paquete), mientras que un original fuera de la lista que cayera volvería al Owner. Emitir 447 también cabría en su condición («una parte»); no se hace.

**La firma de una causa,** como en la v4.3 (sobre el texto de la prueba y de los ayudantes de su archivo que nombra, a un nivel). C-7, ahora de PB-T4d, se mide además por compilación.

| Causa | Firma |
|---|---|
| **C-0. La lectura viene del kit del piloto** | El archivo es una de las 9 clases de R-21: todas sus pruebas se saltan y se duplican |
| C-1. El tipo del borrador | `Find<PushBackPilotDraftState>`, `DraftSession<PushBackPilotDraftState>`, `PushBackPilotDraftState` o `PushBackPilotStateComparer` |
| C-2. El rechazo A/B | `OutOfPilotDiagnostic` o `CompositeOutOfPilotReason` |
| C-3. «Conservar» en dos tiempos | `TryKeepMine` o `KeepPilotEditsOn`; en la UI, `RequestKeepMine`, `KeepMine` o `BrowserFieldKeepMineButton` |
| C-4. «Actualizar» por `ExecuteFull` | `PushBackPilotUpdateRun.Execute` o `Precheck`, `NonPilotContentReason`, `TryUpdatePilot` |
| C-5. La letra de una guarda | Las tablas de §4, método a método, y las cinco medidas sobre el GREEN literal (§4.2) |
| C-6. El adaptador del piloto | `PushBackPilotFieldAdapter` |
| C-7. La retirada (PB-T4d) | Nombra un miembro que se retira (por texto), o deja de compilar al retirarlo (por compilación). **No es de PB-T4b** |
| C-9. Lecturas del piloto fuera del kit | `FromPilot(` o `PrepareKit.Reread(` en la prueba o en un ayudante |

**«Saltar y duplicar», cómo se hace** (V44-03):
1. **Los originales.** Cada prueba de la lista gana un `Skip` con su motivo literal en su línea de atributo (`[Fact]` → `[Fact(Skip = "…")]`, `[Theory]` → `[Theory(Skip = "…")]`), y nada más cambia en el archivo. Los motivos, en ASCII, los fija `pbt4b_skips_v48.py` (con la lista de 478 y el mapa de la copia v5), uno por causa (§6.2.1, «Los `Skip` literales»).
2. **Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v49.py`** (el de la v4.8, `make_copies_pbt4b_v48.py`, con las piezas de `make_copies_pbt4b.py` de la v4.4, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b_v47.py`, sin tocarlos; las ediciones de `pbt4b_hand_edits_v49.py`, las dos clases de PB-T4a y la guarda de la sexta tanda, la lista de 478, el mapa v5, D-S6 (el espacio raíz y el prefijo `FullPanelCopy`), las ediciones de ayudante de C-0, MU, los miembros nuevos de C-0 y las `Theory` partidas por filas de D-S5), por objetos git del cierre de PB-T4a, con el texto literal del paquete; **el Worker no los escribe: copia, byte a byte, los archivos que el guion genera con la partición cerrada** (§6.2, «El RED»). Hay tres clases de archivo de origen:
   - **las 9 clases de C-0, enteras:** sin los tipos del kit del piloto (`EditIntentFakeReader`, `EditIntentKit` y `PrepareKit`), con el mapa M aplicado a todo el texto y, en los 60 «a mano», además sus sustituciones H y sus ediciones literales (`pbt4b_hand_edits.py`, `HAND`);
   - **las 9 clases de guarda de C-5** (`PushBackWritePathGuardTests`, IG, EIG, PEG, RG, WLG, WPUG, FRP y FRG): una copia parcial, **sin M**, con sus pruebas de la lista y sus ediciones (`GUARD_EDITS`) y los ayudantes que alcanzan, con sus ediciones de ayudante (`HELPER_EDITS`): el modelo completo donde la guarda nombraba el piloto;
   - **las 2 clases de la interfaz** (`WorkspacePilotReconciliationTests` y `WorkspacePilotReconciliationViewTests`): una copia parcial, sin ningún cambio.
3. **La forma de cada copia:** un archivo `<Clase>Full.cs` junto a su original (`<Clase>` es su nombre sin `Tests`), en el **espacio de nombres raíz de su ensamblado**, `RackCad.Tests` (`RackCad.UI.Tests` en la UI), como el resto de su proyecto (**D-S6**, V48-01: lo exige `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`, la excepción de I-23; la v4.7 las ponía en `…FullPanelCopies`); cada tipo de primer nivel `T` del archivo, salvo la clase de prueba y los del kit, pasa a `T` + `Full` (M-00); **la clase de prueba se parte en `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests`** según la partición cerrada (en el modo de medida, una sola, `FullPanelCopy<Clase>ProbeTests`; **el prefijo `FullPanelCopy`** hace que ningún filtro heredado las seleccione, porque `…Tests.FullPanelCopy…` no contiene `RackCad.Tests.Workspace` ni ningún otro; es el mecanismo de las CT; **una `Theory` partida por filas, D-S5, va en las dos**, cada mitad con sus filas, y entera en la de medida), y **cada una lleva solo los ayudantes que alcanzan sus pruebas**, por nombre y de forma transitiva (un campo privado sin uso sería un `warning CS`). Las pruebas no cambian de nombre; van en el orden del original, después de los ayudantes. La cabecera de la clase (sus comentarios y su declaración) y su cierre se copian tal cual.
4. **El kit nuevo,** literal, en `tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs` (el archivo custodiado `pbt4b-kit-WorkspaceFullReadKit-v45.cs`, §6.2.1; el de la v4.4, `pbt4b-kit-WorkspaceFullReadKit.cs`, queda como historia): `FullEditIntentFakeReader`, `FullEditIntentKit` y `FullPrepareKit`, con **los miembros de los tipos del kit que sustituyen, con sus nombres y sus parámetros**; el borrador y su comparador son los del modelo completo (M-04 y M-05), y **las lecturas conservan los tipos de las del piloto** (D-S3: `Reread`, `DraftOf(PushBackPilotIntentResult, …)` y `Prepare(…, PushBackPilotIntentResult)`, porque las copias les pasan `IntentKit.Read(…)` y un `PushBackPilotIntentResult?[]`, WPP:1471). Cada lectura que el lector o el kit dan al anfitrión es `EditIntentReadResult.FromReads` sobre el mismo escaneo (`FullEditIntentKit.ReadOf`) o sobre la misma base (`FullHandKit.FullOf`); un `Reread` hecho a mano lleva su gemelo completo, con la misma base y la misma admisión. **Un cuarto tipo, `FullHandKit`,** es el apoyo de los equivalentes a mano (`FullOf`, `ReadOfPilot`, `Prepare`, `Seen`, `ReadWithoutFullRead`, `FullState`, `FullOfText` y `Twin`). Es código de prueba, no la implementación de `src/` (M-34). El kit no depende del kit del piloto, que PB-T4d puede borrar. **En la v4.5** (V45-08):
   - **`FullReadRig`,** el quinto tipo: el ayudante `PushBackFullReadRig` de FRS (que no se edita) sobre el borrador del panel, que es el completo: el mismo anfitrión y las mismas cinco relecturas, con `Find<PushBackFullDraftState>` y `DraftSession<PushBackFullDraftState>`; lo usan los equivalentes de CT-PBT4A-03 y CT-PBT4A-05;
   - **M-04:** `FullPrepareKit.Reread` rechaza un estado del piloto (`Assert.Null(state)`): su gemelo completo solo lleva el estado de `pb-01-base`, y ninguna copia pasa uno (17 llamadas, todas sin estado);
   - **M-05:** `FullHandKit.FullOfMatchesTheReader(PushBackPilotIntentScan)`: sobre el mismo escaneo, `FullOf` de la lectura del piloto da lo que da `PushBackFullDraftReader.Read` (la admisión, el diagnóstico, la clave del origen, el detalle, la pista, la base y, admitido, el estado completo, con su comparador). CT-PBT4B-20 lo corre sobre cinco escaneos (V46-03), así que si el lector cambia, la réplica no se aparta sin que nada lo vea.

**El mapa cerrado M** (`copy_map_defs.py`), en este orden; en las copias de C-0, ninguna otra diferencia:

| Id | Del original | A la copia |
|---|---|---|
| M-00 | Cada tipo de primer nivel `T` del archivo que se copia, salvo la clase de prueba y los del kit | `T` + `Full`, en cada uso dentro de la copia; la clase de prueba `<Clase>Tests` pasa a `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests` (D-S6) |
| M-01 | `EditIntentFakeReader` | `FullEditIntentFakeReader` |
| M-02 | `EditIntentKit` | `FullEditIntentKit` |
| M-03 | `PrepareKit` | `FullPrepareKit` |
| M-04 | `PushBackPilotDraftState` | `PushBackFullDraftState` |
| M-05 | `PushBackPilotStateComparer` | `PushBackFullDraftStateComparer` |
| M-06 | `PushBackPilotFieldAdapter` | `PushBackFullFieldAdapter` |
| M-07 | `.Inputs.Annotations.AnnotationScale` y `.Inputs.Annotations!.AnnotationScale` | `.Editable.AnnotationScale` |
| M-08 | `.Inputs.PalletTolerance` | `.Editable.PalletTolerance` |
| **MU** (v4.6) | Una copia que nombra un tipo de `Draft/` (un tipo declarado en `src/RackCad.Application/Systems/PushBack/Draft/`, fuera de cadenas y comentarios) y no lleva la directiva | `using RackCad.Application.Systems.PushBack.Draft;` tras el último `using` de su cabecera (V46-01): M-04 y M-06 cambian el nombre del tipo, no su espacio de nombres |

M-01 a M-08 son de palabra completa; MU no cambia ningún nombre, solo añade la directiva (en la v4.6, diez copias la llevan: nueve la ganan y `PushBackFullReadPathFull` la hereda). M-06 exige que `PushBackFullFieldAdapter` tenga `AnnotationScaleKey`, `EmptyMessage`, `NotANumberMessage` y `NotPositiveMessage`, con el texto del piloto (las tiene, D-PBT4-06).

**Las sustituciones H de los equivalentes a mano** (`pbt4b_hand_edits.py`, `H`), cerradas, después de M y solo en las pruebas cuya fila las nombra:

| Id | Del original | Al equivalente |
|---|---|---|
| H-01 | `PushBackPilotUpdatePlanner.Prepare(` | `FullHandKit.Prepare(` (el PREPARE completo sobre la lectura completa de la misma base) |
| H-02 | `PushBackPilotPrepareInput.From(` | `PushBackFullPrepareInput.From(` |
| H-03 | `PushBackPilotMaterializer.ComposeForWrite(` | `PushBackFullComposition.ForFullWrite(` |
| H-04 | `EditIntentReadResult.FromPilot(` | `FullHandKit.ReadOfPilot(` (`FromReads` con la lectura completa de la misma base) |

**Los 72 a mano** (el mapa de la copia v5, `pbt4b-copy-map-v5.tsv`, sha256 `bda79bb7fb6e1dfe885cdc5942b2af45dfac8861663ccc99dbb38a61b8844524`, que lleva las dos de la v4.8 con `R-D-S1-borra` (V48-02) y las siete de D-S1 de la v4.4 con el motivo `R-D-S1`, las ocho de la v4.6 con `R-D-S1-compila` y las dos de D-S5 que eran mecánicas con `R-D-S5`; la `Keep…` de WPP, ya a mano por D-S1, lleva `R-D-S1,R-D-S5`; el v4, `0c5abc6d…9b1c`, el v3, `11b26ab1…0743`, el v2, `3ffadf5b…9e01`, y `pbt4-copy-map.tsv`, `1091ae50…`, quedan como historia):
- **32 con texto propio:** en la v4.8 (V48-02, D-S1), las dos de WPP que el modelo completo borra, `InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView` (el lateral fantasma sale de los `Invalid` y da `Ready` con borrados [B2]; `side-b`, `Invalid` con `FrontalBOfOneWayPrefix`/`Suffix`; los dos descriptores, como hoy) y `ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan` (con el fantasma, `Ready`, miembros [A1] y borrados [B2]; sin él, como hoy y sin borrados), según **D-PB-04** («borrar vistas obsoletas, sí; crear vistas no está autorizado»), **D-PBT3-09 (e)** y **D-PBT4-07**; en la v4.6 (V46-02), dos de WPEH (`TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull` y `TheReasonOfAFieldThatCannotBeReadIsInSpanish`, que abren el borrador con la lectura completa, `FullEditIntentKit.Admitted("R9").FullRead!.Read`, en lugar de la del piloto) y dos de WPP (`TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying`, con H-02, y `PrepareDoesNotPutTheDraftInConflictNorCleanDirtyNorTakeTheSlotOfTheUpdate`, que abre el borrador con `FullHandKit.FullOf(read)!.Read`, la lectura completa de la misma base); y, de la v4.5, 14 de WPP (cinco con H-01 y H-02, una con H-03, y ocho con ediciones literales: los dos tiempos de «Conservar» con `FullHandKit.Seen`, `KeepOn`, el estado `Presented` del enumerado, el contenido que el modelo completo sí edita y el estado completo construido con su editable), 1 de EIH (H-04) y 11 de WPRH (siete con H-04, una con `ReadWithoutFullRead` y las tres de D-S1 con los dos tiempos);
- **40 sin más que M:** en la v4.7 (V47-01), las dos que D-S5 parte por filas y eran copias mecánicas, WLHT `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` y WPP `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing`: su texto es el de M, y son a mano por el reparto de sus filas; en la v4.6, las cuatro de WPEH que usan el ayudante `Env.OpenUndecidable` (`DiscardRackNeedsNoComparerAndRemovesAnUndecidableDraftToo`, `AComparerThatThrowsMakesTheFieldUndecidableProtectedAndNotEditableAndGetFieldNeverThrows`, `TheRacksAtRiskAreOrderedOrdinallyAndAComparerThatThrowsMakesThemUncertainApart` y `TheFallbackByDraftIsKnownAndKeepsTheUncertainOnesApart`): su texto es el de M, y son a mano por la edición del ayudante; y, de la v4.5, 28 de las 29 de R-C4 (`TryUpdatePilot` conserva su nombre y su firma, y sus esperas siguen valiendo con `ExecuteFull`; la que falta, `InvalidA_ACurrentState…` de WPP, lleva texto propio), y seis que la firma marcó y cuyo texto sigue valiendo: cuatro de R-C3 (dos de WPP, `AnExceptionOfTheReader…` e `InsideANotice…`, y dos de URT, `ARequestOfASession…` y `DuringTheFlight…`: «Conservar» responde igual antes de leer y cuando el lector lanza), una de R-C2 en EIH (`TheTechnicalDetail…`: el rechazo de otro sistema sigue con su diagnóstico y su detalle) y una de R-C9 en EIH (`TheReadResultCarries…`, que prueba `FromPilot` mismo). **Su resultado lo mide la precondición 6,** como el de las copias.

**Las ediciones de ayudante en las copias de C-0** (V46-02; `C0_HELPER_EDITS` de `pbt4b_hand_edits_v46.py`): sobre el texto del ayudante ya con M, en la copia y nunca en el original, cada una las veces que dice en el conjunto de los ayudantes de su archivo; si no casa, el guion para. **Una sola:** `Env.OpenUndecidable` de `WorkspacePilotEditHostFull` abre el borrador con `FullEditIntentKit.Admitted(rack).FullRead!.Read` en lugar de `.PilotRead!.Read`, porque con el comparador completo `DraftRegistry.Begin<TState>` no infiere el tipo (CS0411). Las pruebas que la usan pasan a «a mano»; la revisión lleva el ayudante editado, y CT-PBT4B-20 lo nombra (su punto 4: lo fija el `CopySha`).

**Las `Theory` partidas por filas** (D-S5; V47-01; `SPLIT`, `RED_KEYS`, `SPLIT_COMMENT` y `C0_NEW_HELPERS` de `pbt4b_hand_edits_v47.py`): las cuatro que mezclaban resultados en el RED de la v4.6. Dos son de C-0 y eran mecánicas (WLHT `EachOperation…` y WPP `Confirm…`), una es de C-0 y ya era a mano (WPP `Keep…`, D-S1) y una es un equivalente de guarda (PEG `TheHostGuardTrips…`). **Con la partición, el guion pone su texto dos veces,** en `FullPanelCopy<Clase>RedTests` con las filas que fallan en el RED y en `FullPanelCopy<Clase>GreenTests` con las que pasan: el mismo nombre, el mismo cuerpo y las mismas aserciones, con una línea de comentario (`SPLIT_COMMENT`) antes de `[Theory]` que dice a qué mitad pertenece. En las de `InlineData`, cada mitad lleva sus líneas `[InlineData(…)]` en el orden del original; en las dos de `MemberData`, `StatusNamesRed` o `StatusNamesGreen` en lugar de `StatusNames`. **Los dos miembros nuevos** van solo en la copia de WPP, tras `StatusNames`, y filtran sus filas por `Clean` sin escribir ninguna. **El guion para** si la partición no da las dos filas, Red y Green, exactamente para estas cuatro, o si sus filas Red no son las declaradas. En el modo de medida van enteras. Su texto literal, en el anexo V49-A (el mismo de la V47-A, con los nombres de clase de D-S6); la tabla, en la cabecera de la v4.7.

**Los 22 equivalentes de las guardas** (`GUARD_EDITS` y `HELPER_EDITS`): en la v4.8 (V48-03), **la de los textos del anfitrión**, WHHG `NoneOfThe30…`, con su tabla `Table` con tres cuentas nuevas (2 → 0, 5 → 6 y 1 → 0, cada una con su `// before N`) y, **desde la v4.9** (CA-T4b I-01; V49-01), su prueba con la **autoprueba de la guarda** tras su `Assert.Empty`: las líneas WHHG:504-525, literales (sus dos variables y sus 13 `Assert.NotEmpty`), sobre su `LiteralViolations` y esa tabla, como hace el equivalente de G-4 (el anexo V49-A); y, de antes, CT-PBT3B-13 admite WHC y exige que el anfitrión solo encuentre el borrador completo; G-4 cuenta `PushBackPilotUpdateRun.ExecuteFull(` y **trae la autoprueba que IG:510-517 ya no discrimina** (§17.2 de la revisión): un llamador de la corrida o de la actualización fuera del núcleo y una segunda llamada en él; las siete de EIG cuentan la base y la lectura de `full`, el diagnóstico de admisión de la lectura completa (dos veces, en las dos comprobaciones) y `var full = read.FullRead;` seis veces; **su ayudante `BaseOrReadUse` busca las dos lecturas, `pilot` y `full`** (`\b(?:pilot|full|result)\.Read\b|\b(?:pilot|full)\s*[?!]?\s*\.\s*Read\b`; M-01 de la §18, V45-05), porque `pilot` sigue vivo en WHC en cuatro sentencias (D-S3) y WHC no nombra `pilot.Read` (medido, `v45/selftest_pbt4b_v45-out.txt`); las cinco de PEG, el adaptador estático `PushBackFullFieldAdapter`; RG:109-123, sin cambio; WLG:627 y :642, `Find<PushBackFullDraftState>` y `DiscardAndReload(full.Read)`; WPUG:823, el orden con `Find<PushBackFullDraftState>` y `PushBackPilotUpdateRun.ExecuteFull(`; **CT-PBT4A-03**, el mismo texto con `new FullReadRig(…)` en lugar de `new PushBackFullReadRig(…)` (el lector `PushBackOriginReaderDouble`, que responde `FromReads`, no cambia); y **CT-PBT4A-05**, el mismo texto con `new FullEditIntentFakeReader()` (un lector sin la interfaz nueva que responde `FromReads`) y `new FullReadRig(reader)`.

**Su texto literal:** el de `make_copies_pbt4b_v49.py` con `pbt4b_hand_edits_v49.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v49.txt` lleva el texto final de las 114 entradas (72 a mano, 22 de guarda, las nueve ediciones de ayudante de las guardas, el ayudante editado de C-0, los dos miembros nuevos y las ocho mitades de D-S5), generado en el modo de medida (§6.2.1).

**Fuera de las pruebas no queda nada que impida compilar las copias** (`copy_map_pbt4-out.txt`), y **el guion para** si una edición no casa exactamente las veces que dice, si una clase de C-0 no tiene en la lista exactamente sus pruebas, o si una guarda o una prueba de la interfaz de la lista no está en su archivo.

**El guion de la copia es también una CT, CT-PBT4B-20** (`WorkspaceFullCopyGuardTests`, Core, `ExpectRed` false; §5.2), que lo comprueba en ejecución: el original igual a su base salvo sus `Skip`, cada prueba de la lista con su copia de igual nombre, cada copia mecánica igual al texto de su original en `BaseSha` con M-00 a M-08, cada archivo de copia y el kit con el sha256 del contrato (del texto con los saltos de línea a `\n`), y `FullOf` igual al lector (M-05). Las ediciones de ayudante de C-0 las fija el `CopySha` de su archivo.

**C02 no tiene excepción en PB-T4b:** el único cambio en los 21 archivos de origen (los 18 de la v4.4, FRP, FRG y WHHG) es el `Skip`, el mecanismo de D-1; ningún ayudante existente se edita ni se borra. El borrado de las 55 originales y de los 29 miembros de apoyo es de PB-T4d, con su lista y su vuelta al Owner (D-PBT4-10).

#### D-PBT4-21. Una sola materialización del modelo completo por lectura (D-S4; OWNER-02; V45-02)

**Lo decidido:** «Cerrar y optimizar en PB-T4b (Recomendado)» (`owner-pbt4a-cost-decision.md`, en `F4B-DRAFTS/coordinator/` del cierre `78a7cef3`): PB-T4b añade a su diseño elegir la vista de origen antes de leer, para materializar el modelo completo **una sola vez** por lectura; el coste que quede se vuelve a medir con la misma prueba sobre el GREEN de PB-T4b y en el Smoke-PB; si la optimización rompe una prueba fuera de la lista, vuelve al Owner. **D-S4 es la decisión de la sesión que lo desarrolla** (se declara en F4B-E52).

**El coste de hoy** (`read-cost/read-cost.md`, sobre `faef7020`): casi todo es la materialización completa (`PushBackEditableStateSeat.LoadExisting`, de 25 a 78 ms por llamada). `RIA.Read` lee el modelo completo del `Primary` en el acto; con el origen encontrado, `AtOrigin` lee además el del origen por `ReadAt` y descarta el del `Primary`: dos materializaciones.

**Por qué en RIA, y no en WHC ni en `Draft/`:**
- la lectura completa inmediata está en la línea del éxito de `RIA.Read`, cuya firma fija EIG:139-141 hasta `RackCatalog catalog)`; no puede recibir el texto del origen;
- el texto del origen solo lo tiene NH, que no puede nombrar el piloto ni `Draft/` (EIG:465-470, B-08);
- WHC recibe la lectura ya hecha (`EditIntentReadResult`).

**El diseño, en RIA y solo en `RackPilotIntentReadResult` y en la línea del éxito de `Read`** (el texto literal, en `green_pbt4b_v45.py`):
- el constructor de PB-T4a, `(pilot, PushBackFullIntentResult full, input)`, guarda el valor ya leído: `_full = new System.Lazy<PushBackFullIntentResult>(full);` (lo usa `AtOrigin`, con la lectura del origen);
- un segundo constructor, `(pilot, System.Lazy<PushBackFullIntentResult> full, input)`, guarda la lectura diferida: `_full = full;`;
- el campo `private readonly System.Lazy<PushBackFullIntentResult>? _full;`, y la propiedad `internal PushBackFullIntentResult? Full => _full?.Value;` (antes, una propiedad automática);
- la línea del éxito de `Read`: `return new RackPilotIntentReadResult(read, new System.Lazy<PushBackFullIntentResult>(() => PushBackFullDraftReader.Read(input, catalog)), input);`.

**Lo que hace:**
- **`Begin` y la relectura sin coincidencia, o con el origen en el `Primary`:** NH pide `read.Full` una vez, en `FromReads`, dentro del bloqueo, y el modelo completo del `Primary` se materializa entonces: una vez;
- **la relectura con el origen encontrado:** `AtOrigin` trae el modelo completo del origen por `ReadAt` y no pide el del `Primary`, que no se materializa nunca: una vez.

**Lo que no hace:**
- **no traduce estado:** lo completo sigue saliendo del lector completo sobre el mismo escaneo;
- **no lee más el dibujo** ni abre otro bloqueo; la lectura diferida corre dentro del `LockDocument` de NH, antes de su `return`;
- **no cambia la excepción:** `PushBackFullDraftReader.Read` contiene las suyas y responde `NotAdmitted` (`PushBackFullDraftReader.cs:108-116`), así que diferirla no cambia ningún motivo de fallo;
- **no toca NH, `AtOrigin`, `ReadAt` ni WHC.** CT-PBT4B-17 y el paso 1 de D-PBT4-08 (la relectura con el origen) no cambian.

**El mecanismo, frente a la letra de la decisión** (M-08 de la §19; V46-04; se declara en F4B-E52): la decisión dice «elegir la vista de origen antes de leer». D-S4 lee el piloto del `Primary` (lo necesita para el testigo y para saber si la lectura vale), elige el origen por texto sobre ese escaneo y difiere solo el modelo completo hasta saberlo. El resultado es el que pide la decisión: una materialización completa por lectura. El coste residual del piloto (de 2 a 10 ms en `read-cost`) lo mide la sonda en su línea `parte: solo el piloto`.

**Las guardas, medidas** (`check_ria_v45.py`, `walk_guards_pbt4b_v45.py --delta-v44` y `ria_added_lines_v45.py`; §4.2):
- **B-08 (EIG:350-364):** en todo RIA, un `new PushBackPilotIntentScan(`, un `PushBackPilotIntentReader.Read(` y un `PushBackFullDraftReader.Read(`, los tres en `Read` y en ese orden;
- **EIG:139-141:** la firma de `Read`, sin cambio;
- **CAG:** los cinco motivos de `Read` y sus dos `ex.Message`, sin cambio; ningún literal nuevo;
- **G-9:** RIA, `StartTransaction` 1 y `LockDocument` 1; **INV-14:** los dos `AssertNoOpenTransaction` de `Read`;
- **CT-PBT4A-07,** con su código real (`PushBackFullReadPathTests.cs:257-375` de `78a7cef3`): sus 13 líneas, iguales, y sus cinco autopruebas; **CT-PBT4A-04** no lee RIA y `ReadAt` no cambia.

**CT-PBT4B-21** (`PushBackFullPanelGuardTests`, `ExpectRed` true; §5.2) lo fija en texto, con autoprueba: los tests no referencian el Plugin, así que, como CT-PBT4A-07, lee RIA, NH y `PushBackOriginByText.cs`. **El coste que queda** (el propio del modelo completo, una vez por lectura) lo mide la sonda de la precondición 6 (`pbt4b-read-cost-probe.cs`, §11), con lo esperado.

### PB-T4c

**Desde la v5.0, entero y literal** (V50-01 a V50-03, V50-08, V50-10, V50-14 y V50-15): medido por objetos git en el cierre de PB-T4b (`6adff90d`, F4B-E53). El texto del GREEN y del esqueleto es el de las piezas `pbt4c-green-v51.txt` y `pbt4c-skeleton-v51.txt` (§6.3.2), copiadas en el anexo V51-A. Las citas de líneas son de `6adff90d` (`v50/remeasure_pbt4c_v50-out.txt`).

#### D-PBT4-12. El gesto, con `CommitField` como frontera (R-PB-10; I-02)

- **El botón** (PV): `BrowserFieldEditAllButton`, `Content="Editar todo"`, `AutomationProperties.Name="Editar el rack completo en el editor"`, en el `WrapPanel` de los botones del campo (PV:218-254), justo tras «Editar» (PV:219-223). Su `Visibility` es `Browser.Field.ShowEditAllButton` y su `Click`, `OnBrowserFieldEditAllClick`, que en `WorkspacePanelView.xaml.cs` es `Contained(() => _viewModel.Browser.Field.EditAll())` (junto a `OnBrowserFieldEditClick`, :83).
- **Cuándo se ofrece** (V50-14): la v4.9 decía «en las mismas condiciones que «Editar»». «Editar» (`ShowEditButton`, FVM:1136) solo se ofrece con la caja en solo lectura, así que desaparecería en cuanto el borrador está abierto y editable. Por eso `ShowEditAllButton` tiene las condiciones de «Editar» **salvo la de solo lectura**:
  - una superficie fijada (no la vista previa: el gesto no pasa por el navegador, que fija la pestaña de «Editar»);
  - sin fallo, sin diagnóstico que se quede y sin elección pendiente;
  - y sin borrador, o con uno que el editor abre (`CanEdit`: registrado y Clean, Dirty o Conflict).
- **El gesto `EditAll()`** (FVM, junto a `BeginEdit`):
  1. `GetField`; con texto pendiente, `_port.CommitField(origin, rack)`, «the same two-phase step as a blocking boundary» (WHC:1187):
     - `Invalid`: no abre; el campo muestra el texto tal cual, con su mensaje, y el aviso «No se abrió el editor: <mensaje>.»;
     - `Busy` o `Rejected`: no abre, y el aviso es el de hoy para un texto que no se aplicó (`RefusalOf`, FVM:1225);
     - `Applied` o `NotEditable`: sigue;
  2. `_port.TryEditAll(new EditIntentRequest(session, rack, null, null))`: sin referencia (el origen lo guarda el anfitrión, D-PBT4-03) y sin testigo;
  3. si la superficie cambió entre tanto, el aviso de hoy (`SurfaceChangedText`); si el anfitrión no empezó el borrador con una lectura no admitida, su diagnóstico y su pista, como `BeginEdit`; si no, el aviso de `EditAllNoticeOf`: «Editor cerrado: sus cambios están en el borrador.» con algún aplicar, «Editor cerrado sin cambios en el borrador.» sin ninguno, y para lo demás el prefijo de hoy («En espera: », «Rechazado: », el de `Describe` de la intención) o «No se abrió el editor: <motivo>.»;
  4. `Render()` (`GetField`) y `_changed?.Invoke()`: el navegador rehace su tira (el resumen y « *» salen de la vista del campo, como hoy).
- **Sin valor nuevo en `PilotBoundaryKind`:** URT:1108-1109 sigue en verde.
- **El anfitrión no abre si el borrador aún tiene texto pendiente,** por si el gesto no lo confirmó: «Hay texto sin confirmar en el campo.».

#### D-PBT4-13. El anfitrión, los dos reenvíos y el puerto generalizado (A-T3-11, A2-1; B-05)

**`TryEditAll` es el único miembro nuevo del puerto** (`IWorkspacePilotEditPort.cs`, al final, tras `TakeUpdateProgress`), con cuerpo por defecto, `=> EditAllOutcome.NotAvailable(null);`, para que el doble `WorkspacePilotEditPortDouble` no se edite (C02). **`EditAllOutcome`** (un valor) y **`EditAllStatus`** (`NotAvailable`, `Waiting`, `Rejected`, `NotBegun`, `Refused`, `Closed` y `Failed`; el valor por defecto es `NotAvailable`) van en WEI, tras `EditIntentOutcome`. `EditAllOutcome` lleva `Status`, `Reason`, `Begin` (el `EditIntentOutcome` de la intención que no empezó el borrador) y `Applied` (los aplicar del editor).

**En WHC** (V50-03):
- el campo `_fullEditHost` (`IWorkspaceFullEditHost?`, de `Application.Workspace`: EIH:1079 sigue con un solo campo de `Systems`, `_updateHost`) y su asignación `SetFullEditHost`, junto a `SetPilotUpdateHost` (WHC:292);
- **la operación del puerto, explícita,** `EditAllOutcome IWorkspacePilotEditPort.TryEditAll(EditIntentRequest request)`, tras la de `TryDiscardAndReload` (WHC:1130-1142): **la marca de lectura como envoltorio** (`_pilotReadInProgress`, como `TryKeepMine`, WHC:1116-1128), **mantenida durante `ShowModal`**;
- **su cuerpo,** el ayudante privado `EditAllOfPilot`, con las reglas de `TryKeepPilotEdit` (WHC:652-751; I-09): `PassGate` primero (`Unavailable` → `NotAvailable`, `Waiting` y `Rejected`), `UpdateInFlightForRequest` después, la contención con `Report("WorkspaceHostCore.TryEditAll", …)`, y `NotePilotEdit(session)` por cada aplicar.

**Los resultados** (medidos con la sonda de la precondición 6, §5.3):
- sin anfitrión del editor: `NotAvailable`, «No editable aquí: el panel no tiene el editor completo.», sin leer;
- **sin borrador, lo empieza:** `TryBeginPilotEdit` (su lectura, su admisión y su diagnóstico); si no admite (la cola aparcada, AUTH-13), `NotBegun` con ese resultado y sin abrir;
- Clean, Dirty o Conflict: abre;
- Applying, o un borrador que ya no está registrado: `Waiting`, «El borrador se está aplicando o se retiró: el editor no se abre.»;
- Unknown, Error o Undecidable (`CanEdit` falso): `Refused`, «El borrador no admite el editor completo en su estado actual.»;
- con texto pendiente: `Refused`, «Hay texto sin confirmar en el campo.»;
- otra sesión: `Rejected`, el motivo de `PassGate`;
- al cerrar: `Closed`, con los aplicar.

**Los dos reenvíos entre el panel y el anfitrión** (B-05; R-22). El panel recibe `new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth))` (WCo:52). Con cuerpo por defecto, ninguno reenviaría. Los dos reenvían, de forma explícita:
- **la compuerta** (`WorkspacePilotSupport.cs:153-209`): `public EditAllOutcome TryEditAll(EditIntentRequest request) => _depth.IsInside ? EditAllOutcome.Waiting(InsideNoticeReason) : _inner.TryEditAll(request);`, tras `RequestUpdate`. Lo prueba CT-PBT4C-12;
- **el adaptador del Plugin** (`PilotEditAdapter`, WCo:283-327): `public EditAllOutcome TryEditAll(EditIntentRequest request) => _gate.TryEditAll(request);`, al final. Lo prueba la guarda de texto CT-PBT4C-13.

**El lanzamiento y el adaptador del puerto,** en `Draft/` (puros, CT-PBT4C-15):
- `PushBackFullDraftLaunch.For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)`: el `Editable` actual del borrador (la ventana lo copia al cargar), el `RackId` y el proyecto del sobre del `Origin` (`RackEmbedStore` y `RackProjectStore`, como `PushBackFullDraftMaterializer`; nulo si el texto no lo trae, que el lanzamiento admite);
- `PushBackFullDraftStatePort : IPushBackDraftStatePort` (`ctor(draft, Action applied)`): `ApplyToDraft(edited)` llama a `SetCurrentDraftState(new PushBackFullDraftState(edited, draft.CurrentDraftState.Origin))`, así que **el origen se conserva**; con `Updated`, llama una vez a `applied()` y responde `Applied`; si no, `NotEditable`.

`PushBackDraftSessionPort` y `LaunchFor` (PB-T2a) se conservan sin uso, con su registro en DISP.

**Las restricciones medidas que el GREEN cumple** (V50-10; las guardas siguen en verde en la suite completa, §4.3):
- **WHC conserva sus dos interfaces:** la copia de PEG:379-386 en `WorkspacePilotEditGuardFull.cs:266-272`, que fija `CopySha`, exige `IWorkspaceLifecycleAdvisor` e `IWorkspacePilotEditPort` y nada más. Por eso `TryEditAll` va en el puerto, y no en una interfaz nueva de WHC;
- **los nombres de los tipos del puerto y del soporte** evitan `Draft|Editor|Subscription|Tab` (PEG:1015, `NameViolations`): `EditAllOutcome` y `EditAllStatus` los evitan, e `IWorkspaceFullEditHost` va en su propio archivo;
- **ningún tipo de `RackCad.Application.Workspace` lleva `Subscription|Editor|Draft` en su nombre** (`WorkspaceModelBoundaryTests:101`, por reflexión): por eso el host del editor completo se llama `IWorkspaceFullEditHost` (D-S9; el paquete nombraba `IWorkspaceFullEditorHost` desde la v4, y la precondición 6 lo vio caer en `/d/r5c`);
- **el comentario del puerto** conserva «only the six operations of the edit intention read the drawing» y «while one of the six reads is in progress» (PEG:1079). `TryEditAll` solo lee por la intención, y su comentario lo dice;
- **`EditAllOutcome` queda fuera del censo de portadores** (`CarriersWithoutCensus`, EIH y su copia: `^Edit(?:Intent|Check|Confirm|Update)(?:Request|Witness|Outcome|Start|Progress)$`);
- **ninguno de los 30 textos de la guarda de WHHG se repite** en WHC ni en el soporte (el equivalente de `NoneOfThe30…` cuenta cada uno): `TryEditAll` usa textos propios y, donde comparte uno, su constante (`PilotReadInProgressReason`, `InsideNoticeReason`);
- `ControllerViolations` (PEG:659-707): un solo `public IWorkspacePilotEditPort`, tres `+=`, ningún `-=` y un solo `event` en WCo;
- la compuerta no gana `static` (`GateViolations`, PEG:602-631, y su prueba, :633);
- en las carpetas del Workspace, ningún diálogo fuera de `EnterPanelModal` (`ModalScopes` y `DialogViolations`, PEG:809-839, y su prueba, :841): `FullEditorModal` solo llama a `_open`.

#### D-PBT4-14. Los estados de cada operación durante el editor (I-01; I-15)

**Medido** con la sonda de la precondición 6 (CT-PBT4C-05), con la ventana abierta (dentro de `ShowModal`; el doble pone el modal):

| Operación | Responde | Por qué |
|---|---|---|
| `SetPendingText`, `CommitField`, `CrossBoundary` (una frontera bloqueante), `DiscardRack` y `DiscardDocument` | `Busy`, «Hay una lectura del piloto en curso.» | Miran la marca (WHC:1165, :1193, :1246, :1281 y :1308) |
| `TryBeginEdit`, `TryCheckNow`, `TryReload`, `TryConfirmBase`, `TryKeepMine`, `TryDiscardAndReload` y `TryEditAll` | `Waiting`, el mismo texto | La marca, en el envoltorio de cada una (WHC:1058-1142 y el de `TryEditAll`) |
| **`RequestUpdate`** | **`Busy`, el mismo texto** | **Mira la marca antes que nada** (WHC:1426), antes de `PassGate` (:1430) |
| `GetField`, `GetRacksAtRisk`, `TakeLossNotices`, `TakeCloseNotices` y `TakeUpdateProgress` | De memoria, como hoy | |

El texto de `Busy` dice «lectura en curso» mientras el editor está abierto. **Ninguna lectura del dibujo** dentro (`lecturas-dentro=0`), y al cerrar el panel vuelve a responder. Se declara: en AutoCAD la paleta está deshabilitada por el modal WPF, y eso solo se ve en el Smoke-PB.

#### D-PBT4-15. El modal y el abridor, en dos piezas (I-01; B-07; M-14)

**Por qué dos piezas** (medido en `6adff90d`). Una sola no cabe en ningún sitio sin romper una guarda:
- **en WCo** (anidada) tendría que nombrar la ventana, de `RackCad.UI.Systems.PushBack`, y `WorkspaceHostSourceGuardTests.cs:398-421` prohíbe `RackCad.UI` en todo archivo de `Plugin/Workspace/` salvo la paleta y el puente;
- **en un archivo nuevo de `Plugin/Workspace/`** rompería las dos guardas de 9 archivos (EIG:1011-1024; `WorkspaceNavigationHostTests:713-723`);
- **fuera de `Plugin/Workspace/`,** si implementara una interfaz de `RackCad.Application.Workspace`, rompería `WorkspaceBridgeSourceGuardTests.cs:361-366`.

**Las dos piezas:**
1. **El modal, anidado en WCo,** tras `PilotEditAdapter`: `private sealed class FullEditorModal : IWorkspaceFullEditHost`, con `ctor(WorkspaceController controller, Action<object> open)` y un solo método, `public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class { using (_controller.EnterPanelModal()) _open(launch); }`.
   - **No nombra `RackCad.UI`** (398-421 sigue en verde) **ni `PushBack`** (WCo no entra en el censo de CT-PBT1B-15).
   - Se asigna en el getter de la navegación de WCo, justo tras `_host.SetPilotUpdateHost(new RackPilotUpdateCommandContext());` (WCo:110): `_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));`. EIG:1155-1184 sigue en verde.
2. **La ventana, en `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`:** `internal static class RackWorkspaceEditorWindow`, con un solo método, `internal static void Show(object launch)`:
   - `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);` (W:296-300);
   - `AcApplication.ShowModalWindow(window);`, sin `Owner`, como el abridor clásico (`PushBackClassicCharacterizationPluginGuardTests.cs:195-210`);
   - **no nombra `RackCad.Application.Workspace`** (361-366 sigue en verde) ni ningún término de G-9 (IG:593-595); no lee ni escribe el dibujo, ni hace nada tras cerrar.

**`IWorkspaceFullEditHost`,** en `src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs`, con el método genérico `void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class;`. WHC lo llama con el `PushBackDraftEditorLaunch`. Se declara como excepción a R-PB-05 y a ADR-0029 D12.

**La guarda que rompe: CT-PBT1B-15** (`PushBackSharedReferenceGuardTests.cs:306-403`, «archivos=14» en **:384**; la v4.9 citaba :381). `RackWorkspaceEditorWindow.cs` nombra `PushBack` (la ventana y el lanzamiento), así que el censo pasa a 15. **Ninguna carpeta lo evita:** todo archivo nuevo que abra la ventana nombra `PushBack`, y ponerlo en uno de los 14 existentes mezclaría el abridor con el camino clásico o con el escritor (R-31). Se sustituye con registro (C-5), con su equivalente: «archivos=15 (los 14 y el abridor); asignaciones=7», lo demás igual (§5.3).

#### D-PBT4-16. La guarda de OB-01.3 (M-05, M-09)

CT-PBT4C-07 mira **seis sujetos**, en texto sin comentarios:
- `RackWorkspaceEditorWindow.cs`;
- el cuerpo de `FullEditorModal` en WCo;
- los cuerpos de `TryEditAll` y de `EditAllOfPilot` en WHC;
- el de `EditAll` en FVM;
- `Draft/PushBackFullDraftLaunch.cs`;
- `Draft/PushBackFullDraftStatePort.cs`.

Ninguno nombra `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, `DrawAndPlace`, `RedrawInPlace`, `InsertRequested` ni `InsertionRequest`. La autoprueba en negativo inyecta cada nombre en cada sujeto (48). En el RED, un sujeto que falta es su texto vacío, y la guarda pasa.

**`RackWorkspaceEditorWindow.cs` no lleva** `LockDocument`, transacción ni nada tras cerrar.

#### D-PBT4-17. «Cerrar» con las dos cosas (A-T2b-OWNER-01; M-12; D-S7 y D-S10)

`PendingWork()` de W (W:3427-3433), **solo en WORKSPACE**:
- un módulo escenificado **y** cambios sin aplicar: una sola pregunta, `WorkspaceStagedAndUnappliedQuestion` = «Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?», con un `if` antes de la línea de hoy, que no cambia;
- solo cambios sin aplicar: la de hoy, «Hay cambios sin aplicar al borrador…»;
- **en CLASSIC, nada cambia:** `HasUnappliedDraftChanges()` es `false` fuera de WORKSPACE (W:3439-3441), y un módulo escenificado pregunta lo de hoy.

**Medido (D-S10):** en WORKSPACE, **escenificar un módulo deja siempre cambios sin aplicar**, porque el estado editable de la ventana lleva la autoridad de módulos (W:177: «el editor, la cola, la autoridad de modulos y los portadores»). La sonda de UI sobre el GREEN literal (`/d/r5f`, `pbt4c-probe-close-ui.cs`) lo vio con los dos gestos que escenifican: la cabecera con `HeaderConfiguratorDialog` y `StageModuleEdit` dan la pregunta combinada; el conmutador `AdvancedModulesToggle` o la sola elección del módulo no dejan nada pendiente. **Así que en WORKSPACE «solo un módulo escenificado» no se puede alcanzar:** la rama de `PendingWork()` que pregunta solo por el módulo queda para CLASSIC. La decisión del Owner (A-T2b-OWNER-01, F4B-E38) se cumple con la pregunta combinada.

**Lo que cambia en una prueba existente:** CT-PBT2B-06, ventana 4, fijaba que con `StageModuleEdit` mandaba la pregunta del módulo. Está en la lista de 4 (`owner-pbt4c-list-4-decision.md`), con su equivalente.

**La prueba** (CT-PBT4C-09) lee `PendingWork()` sin cerrar la ventana y cubre lo que se puede alcanzar (D-S10): `ambas`, `solo-sin-aplicar`, `ninguna` y `clasico-modulo`. **Escenifica la cabecera con `HeaderConfiguratorDialog` (W:1488) y `ConfigureModuleHeaderButton`** (D-S7): `ConfigureModuleHeader` llama a la costura en lugar de `ShowHeaderConfigurator` (W:1901-1903), así que no se construye el configurador. La v4.9 nombraba `HeaderConfiguratorPresenter` (W:2373), que recibe un `RackFrameConfiguratorWindow` real ya construido, y C05 lo prohíbe. W solo cambia `PendingWork()` y su constante; `W.xaml` no cambia.

### Común



#### D-PBT4-18. D2 (A2-5, A-T3-4; I-06, I-16, M-25)

**La letra del Owner** (`owner-d1-d2-decisions.md:113-131`), con la 7: «comparing InitialDraftState vs CurrentDraftState does not depend on mutable changes inside those shared objects».

**CT-PBT4B-10,** la huella tras cada operación:
- condición 3: `Begin`, `SetCurrentDraftState` y el rebase;
- condición 5: `SetPendingText` y `CommitField`;
- condición 6: `DiscardAndReload` y `KeepMineOnCurrentVersion` con `KeepOn`;
- **condición 7:**
  - sobre una copia del borrador, se sustituye la línea base resuelta del lado A por la de otro fixture, de contenido distinto, con `PushBackEditorState.SetWorkingBaseline(PushBackSystem)` (`PushBackEditorState.cs:74-79`);
  - el comparador da `True`, porque no compara la línea base ni el informe (`PushBackEditableStateComparer.cs:20-27`), e `IsDirty` da `False`;
  - la línea es `linea-base-de-otro-contenido isdirty=False`;
  - el informe de reconciliación se sustituye por el miembro con el que CT-PBT1B-16 lo lee para su huella, con la línea `informe-de-otro-contenido isdirty=False`.

**CT-PBT4C-11,** sobre `ApplyToDraft`.

**El límite literal,** en C17. **M11** no aplica.


#### D-PBT4-19. Lo que PB-T4 no hace

- No toca la ventana clásica, RACKEDITAR, RPB ni el menú. **W solo cambia la pregunta de D-PBT4-17,** en WORKSPACE; el XAML no cambia.
- No crea vistas.
- No traduce estados.
- No usa el asiento en `Recompute` (A-T3-9).
- No abre AUTH-13 ni cambia `DormantTailOutOfPilotReason` ni el camino clásico.
- No añade botones ni campos fuera de «Editar todo», el texto de la presentación y la pregunta de «Cerrar».
- No cachea las pasadas.
- **No cambia el comportamiento de ninguna caja numérica:** `NumericFieldValidation` delega en la misma lógica (D-PBT4-06).

#### D-PBT4-20. Los arrastres y las limitaciones

| Arrastre o limitación | Dónde | Por qué |
|---|---|---|
| A-T3-1, A-5 y R-T1-17 | PB-T4d: se retira tras PB-T4c (V42-01) | D-PBT4-10 |
| A-T3-2 (OB-03.2) | PB-T4b | D-PBT4-05 |
| A-T3-3 (`KeepOn`; la presentación; las guardas «sin conectar»; el nombre `""`) | PB-T4b. El nombre `""` sale como «Cambiado por ti: el nombre.» | D-PBT4-08 y 09 |
| A-T3-4 (D2, M11, M15, F4B-E30) | D2 en PB-T4b y PB-T4c; M11 no aplica; M15 se mide (§7); F4B-E30, una línea en CT-PBT4C-10 | D-PBT4-18 |
| A-T3-8 | PB-T4a | D-PBT4-03 |
| A-T3-9 | Fuera: registro en DISP y entrada en `ideas-futuras.md` (M-07) | CT-PBT3A-01 y CT-PBT2B-02 ya fijan la igualdad; tocar `Recompute` arriesga T1B |
| A-T3-10 | Las constantes se quedan en `PushBackPilotMaterializer` (el lector las usa) | D-PBT4-10 |
| A-T3-11 y A2-1 | PB-T4c | D-PBT4-13 |
| A2-2, A2-3 y A2-9 | PB-T4c | D-PBT4-12 a 16 |
| A2-6 (la regla) | PB-T4b | D-PBT4-06 |
| A2-11 | CT-PBT4C-10, sin AutoCAD; lo físico, el Smoke-PB | §5.5 |
| A2-13 y A-9 (`NaN`) | CT-PBT4B-11 | D-PBT4-18 |
| A-T2b-OWNER-01 | PB-T4c | D-PBT4-17 |
| OWNER-01 de PB-T2c (lado B sin frentes) | CT-PBT4B-09 | D-PB-08 |
| OWNER-01 de «Restaurar»; PBT3B-IR-M01 y OWNER-2; el configurador | Fuera o regla, como en la v1. OWNER-2 de PB-T3b se anota en `ideas-futuras.md` con el cierre de PB-T3c (F4B-E48); los riesgos del borrado real (OWNER-1 a OWNER-3 de F4B-E48), al Smoke-PB (§5.5) | |

---



## 4. Las guardas, por subtarea

### 4.1 PB-T4a (V41-06, V41-07, V41-16, V41-17; B-08, I-25)

**Una sola sustitución: CT-PBT3A-12** (`PushBackDraftWriteGuardTests.cs:60-95`). En el GREEN, RIA nombra `PushBackFullDraftReader`, uno de sus cinco nombres (`ConnectedNames`, :45-48), fuera de `Draft/`, PP y UR. La sustituye CT-PBT4A-06, el mismo censo con RIA admitida. En el esqueleto RIA no cambia, así que CT-PBT3A-12 pasaría; lleva el `Skip` desde el RED, como D-1.

**B-08, arreglado.** EIG:350-364 (`TheReadBuildsTheInputFromTheScanAndTheEvidenceOnce…`) cuenta en **todo** RIA un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(` (:356-357). La v4.1 ponía la segunda lectura del piloto en `AtOrigin`, y la habría roto. Con `ReadAt` en `Draft/` (D-PBT4-03), RIA conserva una de cada, en `Read`, y esa prueba sigue en verde.

**El recorrido por marcas (I-25), por diferencia** (`walk_guards_pbt4a.py` sobre `7f0d9aa3`, con el GREEN literal de `green_pbt4a.py`; las salidas, `pbt4a-guard-walk.tsv`, `pbt4a-guard-walk-out.txt` y `pbt4a-guard-walk-flagged.txt`):
- **Qué recorre, y qué no** (CA-T4a M-01): selecciona por las marcas `MARKERS` de `walk_guards_pbt4a.py`, que son literales de ruta y de ayudante; **no ven** una ruta compuesta con `Path.Combine(…, "src", "RackCad.Application", "Workspace")`, ni los barridos de todo `src/`, de todo Application o de todo el Plugin (las marcas no incluyen `src`, `src/RackCad.Application` ni `src/RackCad.Plugin`). Con ese criterio recorre las **46 clases** de prueba que nombran por texto los archivos que toca PB-T4a (WHC, WEI, NH y RIA), sus carpetas (`Application/Workspace/`, `Plugin/Workspace/`, `Plugin/Views/`, `Systems/PushBack/Draft/` y `src`) o los ayudantes de carpeta de `WorkspaceHostSource` (`ApplicationFiles`, `PluginFiles`, `AllProduct`). Son las de la §16.2 de la revisión (EIG, CAG, IG, `WorkspaceBridgeSourceGuardTests`, `WorkspaceBrowserHostGuardTests`, `WorkspaceNavigationAuthorityTests`, `WorkspaceNavigationHostTests`, `WorkspacePaletteHostSourceGuardTests`, `WorkspacePilotReconciliationGuardTests` y `WorkspaceSourceGuardExceptionTests`) y las demás que el mismo criterio encuentra: **1 026 pruebas** sin `Skip`.
- **El método:** una prueba de texto que pasa en la base solo puede caer en el GREEN si alguna consulta que hace sobre el texto da otro resultado. Para cada prueba se toman todos los literales de cadena de su texto, de los ayudantes de su archivo que alcanza (transitivo) y de las constantes de ruta de otras clases que nombra. Cada literal se evalúa, como expresión regular y como subcadena, en la base y en el GREEN, sobre el archivo entero y miembro a miembro. Los archivos nuevos se miran aparte, para las pruebas que barren su carpeta.
- **Lo que da:**
  - **764** no leen ninguno de los archivos tocados ni sus carpetas: verdes por construcción;
  - **162** los leen y ningún literal con nombre da otro resultado ni seleccionan un cuerpo que cambie: verdes por construcción;
  - **100** quedan señaladas (67 por un literal que cambia de cuenta, 33 porque leen un cuerpo que cambia) y **se leyeron una a una:**
    - **1 cae: CT-PBT3A-12,** la de la lista;
    - **las otras 99, verdes.** Los motivos, por grupos:
      - las que miden cuerpos que el GREEN no toca (el drenaje, `PassGate`, `AdviseClose`, `SessionDestroyed`, `MarkPilotDraftsPossiblyStale`, `DiscardPilotEdits`, `UpdateGate`, `TryUpdatePilot`, los cuerpos del resumen de NH, `ReadSummary`, `ExecuteAsync`, `RunOnActiveDocument`, `ReturnFocusToDrawing`, `TakeLossNotices` y `TakeCloseNotices`), aunque la cuenta del archivo entero cambie;
      - las de los cinco cuerpos que releen, de la recarga y de las explícitas del puerto (EIG:562-580, :582-596, :692-778, :870-894, :983-1009 y :1357-1390; WLG:469-681; PEG:379-598 y :715-734): las cuentas siguen, porque `ReadIntentAtOrigin(` no casa con `\.ReadIntent\s*\(` ni `_editIntentReader is` con `_editIntentReader\s*[.!]`, y el texto de la sentencia no tiene ninguna marca, ningún texto del vuelo ni ninguna operación del ciclo de vida;
      - las de RIA (EIG:88-118, :170-204, :206-301, :303-320, :322-364): la firma de `Read`, sus motivos, sus `catch`, su `finally`, `AssertNoOpenTransaction`, un `CaptureDrawing`, un `new Handle`, un `RackBlockData.Read`, ningún `GetObject`, ningún `throw`, ninguna API de escritura de `AdapterWrites` y, con `ReadAt`, una lectura de cada lector;
      - las de NH (EIG:405-470; RG:696-712; CAG:441-465; IG:590-720; WBSG, los ocho: los enganches, la persistencia, la superficie pública y los puntos de entrada): `ReadIntentAtOrigin` no añade ningún evento, ninguna persistencia, ningún tipo de AutoCAD en una firma pública, ningún `_authorize(`, ningún motivo, ningún `RackLog.Exception(` ni ningún `LockDocument`;
      - las de carpeta (EIG:505-535 y :1011-1024; WNAT:187-196; WHSG:155-166; G-6 y G-9 de IG; WLG:732-744; PEG:916-975 y :1000-1076): la interfaz nueva no nombra el piloto, ni `WorkspaceHostCore`, ni ninguna API de escritura, y `Plugin/Workspace/` sigue con sus 9 archivos; `PushBackOriginByText.cs` nombra el piloto, pero no usa ninguna API de escritura ni ningún bloqueo (G-9 lo admite así, como a `PushBackFullDraftReader.cs`);
      - CT-PBT3A-13 y 14 leen una lista fija de 8 archivos de `Draft/`, que no cambia (de ahí CT-PBT4A-08);
      - G16, las tres de `SelectiveEditorOpenTests` y la documentación de los estados de WEI (WLG:869-885; PEG:1104-1122) leen otros archivos, o partes de WEI que no cambian.
- **La ampliación de CA-T4a, sobre lo que las marcas no ven** (`cat4a_walk_gap.py` y `cat4a_added_lines.py`, con sus salidas `cat4a_walk_gap-out.txt` y `cat4a_added_lines-all-out.txt`, a custodiar):
  - **146** archivos de prueba fuera del recorrido nombran `src/RackCad.(Application|Plugin)`, `"src"` o un ayudante de barrido; **97** nombran como literal una carpeta antecesora de un archivo tocado o nuevo;
  - cada literal de esos 97 (de 4 letras o más, o con metacaracteres) se probó contra las **134** líneas que el GREEN añade o cambia en WHC, WEI, NH y RIA, y contra las de los dos archivos nuevos: solo casan fichas genéricas (`Systems`, `RackCad.Application`, `PushBack`, `name`), en barridos acotados a otros archivos, y algún falso positivo de la herramienta (un literal que, como expresión regular, es una clase de caracteres);
  - **los que sí leen los archivos tocados se leyeron:** `PushBackSharedReferenceGuardTests` (CT-PBT1B-14, cuyas reglas de G-L1 el GREEN no toca, y CT-PBT1B-15, sin ninguna asignación `.Name =` nueva), `WorkspaceModelBoundaryTests`, `CustomPropertiesEnvelopeGuardTests` y `CustomPropertiesEdgeGuardTests` (eligen sus archivos por nombre), `RackDefinitionCreatorGuardTests`, `NamespaceFolderGuardTests` (los dos archivos nuevos tienen el espacio de nombres de su carpeta, en bloque), `StructuralSectionPluginSourceGuardTests` y `SelectiveDuplicationFailClosedTests` (archivos fijos). **Ninguno cae.**
- **La reflexión:** las guardas que la usan sobre WHC (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503 e IG:427-428) siguen, porque WHC no declara nada nuevo; **`WorkspaceModelBoundaryTests`** recorre por reflexión los tipos y miembros de `RackCad.Application.Workspace`, y ninguno nuevo (`IWorkspaceEditIntentOriginReader`, `ReadIntentAtOrigin`, `FullRead`, `Catalog` y `FromReads`) casa con `Subscription|Editor|Draft`, `Tab` ni `ProjectSummary|Metric|Provider|…`; las que enumeran tipos del ensamblado (`NoProductPlannerGuardTests`, `RackModuleEditSessionTests:497`, `CustomPropertiesProjectTests:329` y `RackViewCountInvariantTests:39`) miran otros espacios de nombres o nombres que los tipos nuevos no usan.
- **Las aserciones traducidas** (`measure_pbt4_v43.py`; sobre el mismo GREEN, con la de B-08, la regla de CA-T4a I-01 y las tres contrafactuales):

| Guarda | Qué se comprueba | Sobre el GREEN |
|---|---|---|
| EIG:568-574 | Begin: orden, 1 `.ReadIntent(` y 1 `_editIntentReader` seguido de `.`/`!` | **verde** |
| EIG:569-574 | «Comprobar ahora»: orden, 1 `.ReadIntent(` y 1 `_editIntentReader` seguido de `.`/`!` | **verde** |
| EIG:578-579 | PassGate no lee | **verde** |
| EIG:604 | el drenaje del núcleo no nombra la intención | **verde** |
| EIG:620-623 | solo WHC llama a `.ReadIntent(` (NH y RIA del GREEN no) | **verde** |
| EIG:636 | el drenaje sigue limpio (autoprueba) | **verde** |
| EIG:671-688 | la base solo llega a sus consumidores; las cuentas por operación de `pilot.Read` y `.StaleBase` | **verde** |
| EIG:695-718 | las cuentas de los tres cuerpos de la reconciliación | **verde** |
| EIG:733-736 | seis `var pilot = read.PilotRead;` y seis `.ReadIntent(` en WHC | **verde** |
| EIG:761-769 | los tres cuerpos: orden, 1 `.ReadIntent(`, 1 `_editIntentReader` con `.`/`!` y la marca después de la lectura | **verde** |
| EIG:1124-1130 | «Actualizar» no lee | **verde** |
| WLG:369 | AdviseClose | **verde** |
| WLG:442 | SessionDestroyed | **verde** |
| WLG:623 | el drenaje | **verde** |
| WLG:634 | MarkPilotDraftsPossiblyStale | **verde** |
| WLG:645-656 | la recarga: orden, 1 `.ReadIntent(`, 1 `_editIntentReader` con `.`/`!`, 1 `pilot.Read` | **verde** |
| WLG:687 | el descarte explícito del anfitrión | **verde** |
| PEG:288 y :297 | solo cambian los cinco cuerpos que ya leían (ninguna operación sin lectura ni de «Actualizar»): check, confirm, discard, keep, reload | **verde** |
| Reflexión (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503, IG:427-428) | WHC no declara nada nuevo: los mismos miembros declarados | **verde** |
| EIG:409-431 | el lector anidado: los ocho pasos en orden; 1 `LoadCatalog(`, 1 `RackPilotIntentAdapter.Read(` y 1 `_targets.Revalidate(` en NH | **verde** |
| EIG:441-453 | NH: dos clases y el lector anidado implementa la interfaz | **verde** |
| EIG:465-470 | NH: un `read.Pilot` y ningún `PushBackPilot` ni `RackCad.Application.Systems` | **verde** |
| RG:696-712 | cada `_authorize(` del lector tiene la forma exacta | **verde** |
| CAG:410-451 (NH) | ReadOnActiveDocument: los tres motivos en español y ninguno en inglés | **verde** |
| NHT:418-559 y WBHG:72-168 | las cuentas de NH (`RackLog.Exception(` 1, `ReturnFocusToDrawing(` 2, `.Focus(` 1, sin async/await; los `+=` 2 de NHT:984 y WBHG:218 son de la paleta, no de NH) | **verde** |
| IG:684-685 (G-9) | NH: `LockDocument` 2 | **verde** |
| EIG:132-147 | RIA: la firma de `Read` (:139-141), tres clases de resultado y nada público | **verde** |
| EIG:150-160 | RIA: ningún campo de un objeto del anfitrión ni campo estático | **verde** |
| EIG:384-388 | RIA no sabe nada del Workspace ni de la navegación | **verde** |
| CAG:410-464 (RIA) | Read: los cinco motivos en español y dos `ex.Message` | **verde** |
| IG:684 (G-9) | RIA: `StartTransaction` 1 y `LockDocument` 1 | **verde** |
| EIG:350-364 (B-08) | RIA: un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(` en todo el archivo, y el orden de `Read` | **verde** |
| EIG:322-336 y :338-347 | RIA: un `CaptureDrawing(` (y en `Read`), un `new Handle(`, sin `Handle.Parse` ni `GetObjectId(` | **verde** |
| CT-PBT4A-07 (nueva) | `AtOrigin` solo llama a `ReadAt`: sin lecturas propias, sin `CaptureDrawing`, bloqueo, transacción, literal de cadena ni `ex.Message`; RIA nombra una vez cada lector, en `Read` | **verde** |
| CT-PBT4A-04 y 08 (Draft) | `PushBackOriginByText`: `Choose`, `AtKey` y `ReadAt`, con un lector de cada; ningún término de la pureza de CT-PBT3A-14 | **verde** |
| CA-T4a I-01 (la regla del §5.0) | ningún `?` de tipo de referencia en lo que el GREEN añade a un archivo sin `#nullable enable` (NH y `Draft/PushBackOriginByText.cs`; WHC, WEI, RIA y la interfaz lo llevan), y ningún `#nullable` en `Draft/`: ninguno | **verde** |
| Contrafactual (CA-T4a I-01) | con el texto de la v4.2 de `PushBackOriginByText.cs`, la regla ve `string?`, `string?` | **ROJA (esperada)** |
| Contrafactual (EIG:139-141) | si `Read` ganara `originText` como parámetro obligatorio, la firma fijada por la guarda deja de casar | **ROJA (esperada)** |
| Contrafactual (EIG:139-141) | si `Read` ganara `originText` como parámetro con valor por defecto, la firma fijada por la guarda deja de casar | **ROJA (esperada)** |

**En rojo:** 0 de 36 sobre el GREEN; contrafactuales en rojo, como se espera: 3 de 3.

**Lo que queda** (D = 0 en el GREEN, §5.1): **una lectura que solo se ve al ejecutar** (una regla que no se escribe como literal, o la reflexión sobre un tipo que no se miró). La cierra la medida de la precondición 6 con `apply_green_pbt4a.py` (la suite completa de Core sobre el `src/` del GREEN literal); si una prueba de fuera de la lista cae en el GREEN, es **PBT4A-C16** y vuelve al Owner. CA-T4a leyó las 100 señaladas y las de la ampliación sobre el texto de la v4.2, igual en esto.

### 4.2 PB-T4b (V44-01, V44-06; V45-01, V45-02)

**Las roturas por letra (C-5), método a método: 23.** Las 15 de la v4.3 (`pbt4-substitution-lists.md`, «C-5 y C-6»), las cinco que se midieron sobre el GREEN literal de PB-T4b y que el Owner añadió a la lista (`owner-pbt4b-breaks-decision.md`; la verificación, en la §17 de `independent-review-v1.md`), las dos de PB-T4a, que se vieron al construir la v4.5 sobre su código real (`owner-pbt4b-list-477-decision.md`), y la de la sexta tanda, que vio la precondición 6 de la v4.7 con la suite completa de Core (`owner-pbt4b-list-478-decision.md`; V48-03):

| Guarda | Métodos | Causa | Su sustituta |
|---|---|---|---|
| CT-PBT4A-06 (el censo de PB-T4a) | 1 | Admite WHC | **CT-PBT4B-13** |
| CT-PBT3B-13 (`PushBackWritePathGuardTests:251-299`) | 1 | El anfitrión encuentra el borrador completo y nombra la escritura completa | Su equivalente en `PushBackWritePathGuardFull` |
| G-4 (IG:487-502, `G4Violations`) | 2: :504-508 y :519-532 | `PushBackPilotUpdateRun.Execute(` deja de nombrarse | Su equivalente en `WorkspacePilotUpdateIntegrationGuardFull`, con la autoprueba de IG:510-517 |
| EIG:664-690, :692-721, :727-737, :754-778 y :799-810 | 5 | `pilot.Read` en `Begin`; `KeepPilotEditsOn`; las seis `var pilot = read.PilotRead;` | Sus equivalentes en `WorkspaceEditIntentGuardFull` |
| **EIG:870-880 y :882-894** (nuevas) | **2** | **El diagnóstico de admisión de las dos comprobaciones sale de la lectura completa** (OB-03.2) | Sus equivalentes en `WorkspaceEditIntentGuardFull` |
| PEG, la regla del adaptador estático de `HostViolations` (:280) | 5: :379-386, :388-399, :495-514, :528-537 y :1033-1054 | C-6 | Sus equivalentes en `WorkspacePilotEditGuardFull` (:1033-1054, sin cambio) |
| RG:109-123 | 1 | La v4.3: `Mutate` sobre `RequestKeepMine` (RG:116). **Con D-S2 su texto sigue valiendo** | Su copia, sin cambio, en `WorkspacePilotReconciliationGuardFull` |
| **WLG:626-635 y :641-664** (nuevas) | **2** | **El marcado y la recarga de un borrador del piloto** (`Find<PushBackPilotDraftState>` y `pilot.Read`) | Sus equivalentes en `WorkspaceLifecycleGuardFull` |
| **WPUG:822-840** (nueva) | **1** | **«Actualizar» con la corrida del piloto** (`PushBackPilotUpdateRun.Execute(`; estaba en la lista de PB-T4d, pero cae en PB-T4b) | Su equivalente en `WorkspacePilotUpdateGuardFull` |
| **CT-PBT4A-03** (FRP:152-186; v4.5) | **1** | **Su ayudante busca el borrador del piloto** (`PushBackFullReadRig.Draft()`, FRS:100): con el borrador completo no lo encuentra y faltan las cinco líneas | Su equivalente en `PushBackFullReadPathFull`, con `FullReadRig` |
| **CT-PBT4A-05** (FRG:53-74; v4.5) | **1** | **Su lector responde `FromPilot`** (`EditIntentFakeReader`, EIH:22): con el borrador completo, `Begin` responde `NotAdmitted` y la línea cambia | Su equivalente en `PushBackFullReadGuardFull`, con `FullEditIntentFakeReader` y `FullReadRig` |
| **WHHG:491-499** (`NoneOfThe30LiteralsInEnglish…`; v4.8) | **1** | **Cuenta exactamente los textos en español del anfitrión**, y el GREEN cambia tres: «La relectura no trajo resultado.» 2 → 0 (WHC:426 y :611 de la base: `CheckNow` y `ConfirmBase` ya no leen el piloto y responden `FullReadMissingReason`, D-PBT4-04), «El borrador se está aplicando o ya no está registrado.» 5 → 6 (la rama nueva de «Conservar» que presenta, D-PBT4-08, paso 2) y «El rack no tiene escala de anotación.» 1 → 0 (WHC:1783: la escala del modelo completo no es anulable) | Su equivalente en `WorkspaceHostHardeningGuardFull`, con la misma tabla y esas tres cuentas, y con la autoprueba de la guarda (WHHG:504-525) tras su `Assert.Empty` (CA-T4b I-01) |

**Cómo se midieron las cinco:** con `walk_guards_pbt4b.py`, el recorrido por diferencia de las guardas que leen WHC, WEI, FVM, PV y `NumericFieldValidation.cs` (34 clases, 885 pruebas), sobre el GREEN de PB-T4a y el GREEN literal de PB-T4b (`green_pbt4b.py`). 113 pruebas fuera de la lista de 470 cambiaban en algún literal; se leyeron una a una y **108 siguen en verde**; las cinco caen. La revisión las confirmó (§17.1 a §17.3) con las aserciones traducidas. **Con el GREEN de la v4.4** (D-PBT4-08: la clasificación de «Conservar» y su admisión en `Draft/`), el recorrido se repitió con la lista de 475 (`v44/pbt4b-guard-walk-v44.tsv`): las pruebas marcadas fuera de la lista son las mismas 108 de antes, que siguen en verde. **El hueco del recorrido** (las rutas compuestas, los barridos de todo `src/` y las pruebas de UI) es el de PB-T4a: lo cubre la precondición 6, que ejecuta la suite sobre el GREEN literal.

**Las dos de PB-T4a** (V45-01) no las ve un recorrido por letra: CT-PBT4A-03 y CT-PBT4A-05 no leen texto; ejecutan el anfitrión con su ayudante y su lector. Se vieron al leer el código real de las ocho CT de PB-T4a en el cierre (`78a7cef3`, FRP, FRG y FRS): **las otras seis no caen** (CT-01, 02 y 04 son puras sobre `FromReads`, los lectores y `ReadAt`, que PB-T4b no cambia; CT-06 está en la lista; CT-07 lee RIA y NH, abajo; CT-08 solo lee `PushBackOriginByText.cs`, que no cambia).

**El recorrido de la v4.5** (`walk_guards_pbt4b_v45.py`, sobre el cierre de PB-T4a, con la lista de 477, con RIA entre los archivos tocados y sus marcas, y con las 20 copias y el kit de la v4.5 como archivos locales; `v45/compare_walks_v45-out.txt`):
- **solo lo que cambia D-S4** (`--delta-v44`, la base es el GREEN de la v4.4 con la RIA de PB-T4a): **23 pruebas marcadas fuera de la lista**, leídas una a una, **todas en verde**: las de EIG que leen RIA (las cuentas de B-08, los bloqueos y las transacciones, `AssertNoOpenTransaction`, los `catch`, las escrituras, `CaptureDrawing` y `Handle`), CAG (los motivos y los `ex.Message` de `Read`), G-9 de IG, WBSG y WHSG (barridos de `Plugin/Workspace` y de todo `src/`, sin enganches, persistencia ni inicializadores nuevos), `WorkspaceNavigationHostTests:581-595` y CT-PBT4A-07, que `check_ria_v45.py` traduce de su código real: sus 13 líneas, iguales;
- **todo el GREEN de la v4.5:** 149 marcadas fuera de la lista: 107 de las 108 de la v4.4 (la que falta, CT-PBT3A-12, ya la salta PB-T4a), las 23 del delta (cinco de ellas también entre las 107), 18 de las copias y el kit, que son las sustitutas, y **seis nuevas por las marcas del Plugin**, leídas, en verde, porque no leen ningún archivo tocado: `G16ProjectedEnvelopeTests:100-111` (`RackDefinitionCreator.cs`), `I60BatchCreationNamingTests:108-114` (`RackProjection*.cs`), tres de `SelectiveEditorOpenTests` (los comandos y la ventana del Selectivo) y CT-PBT4A-08 (solo `PushBackOriginByText.cs`);
- **el barrido de las líneas añadidas de RIA** (`ria_added_lines_v45.py`, como la ampliación de CA-T4a): 152 pruebas que leen el Plugin o todo `src/`; ningún acierto aplica a RIA (son nombres comunes, como `PushBack`, `Read` o `[CommandMethod(…)]` como clase de caracteres).
**El recorrido de la v4.5 no veía los campos de tipo tupla** (su expresión `FIELD`, `walk_guards_pbt4b_v45.py:91`, no admitía espacios en el tipo), y por eso no marcó la guarda de la sexta tanda, cuya tabla es `(string English, string Spanish, int Times)[] Table` (WHHG:421). La precondición 6 de la v4.7 la vio caer con la suite completa de Core (`v47/analysis-outside-477.md`, punto 2).

**El recorrido de la v4.8** (V48-04; `walk_guards_pbt4b_v48.py`, el de la v4.5 con esa corrección y nada más: el tipo admite una tupla y un genérico con espacios; reconoce campos nuevos en 18 archivos de guardas, `v48/walk-v48-same-inputs-as-v45-out.txt`):
- **con las entradas de la v4.5** (la lista de 477, las 20 copias y el kit de la v4.5; `v48/compare_walks_v48-same-inputs-out.txt`): las mismas 1 096 pruebas, **tres marcadas más fuera de la lista**, y dos de G-9 de IG (ya marcadas) con más diferencias:
  - **WHHG `NoneOfThe30LiteralsInEnglish…`**: las tres cuentas de arriba. **Entra en la lista** (la sexta tanda);
  - **WHHG `TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier`**: su hermana, la autoprueba de la guarda, que lee la misma tabla. **No entra en la lista.** Pasa en las dos suites completas del GREEN (`/d/r4m` y `/d/r4o`), **pero en vacío** (CA-T4b I-01; V49-01): llama a `LiteralViolations` sobre el anfitrión y el soporte reales (WHHG:504-505), con la tabla del original (2, 5 y 1), y solo exige `NotEmpty`, 13 veces, sin una línea base vacía. En el GREEN esas tres cuentas ya no casan (0, 6 y 0), así que `LiteralViolations` sin mutar ya da tres violaciones, y las 13 se cumplen con cualquier mutación. **Por eso el equivalente de `NoneOfThe30…` trae esa autoprueba sobre su tabla nueva**, como el de G-4 (la v4.8 daba otra razón: «es una autoprueba que inyecta textos sobre el anfitrión y pasa»);
  - **`WorkspaceCommandAdvisorGuardTests.TheCatalogBodyDoesNotNameTheDocumentTypeAndNoSpanishReasonUsesAWordThatTheNeighbouringGuardsForbid`** (WCAG:468-474): el paseo ve que `"Read"` pasa de 21 a 22 en RIA. **No entra en la lista:** `"Read"` es el valor de la columna `Body` de su tupla `ReasonTable` (WCAG:408-419; las filas que cuenta el paseo son :415-419; CA-T4b M-03), el nombre de un cuerpo, no un literal que se cuente en RIA; la prueba solo comprueba que `CatalogBody()` no nombra `Document` y mira la columna `Spanish`. Es un falso positivo del paseo, y **pasa en las dos suites completas del GREEN** (`/d/r4m` y `/d/r4o`);
  - G-9 de IG (`G9NoFileOfThePlugin…` y `G9TripsOnAPilotFile…`), ya marcadas desde la v4.4 y leídas: pasan en el GREEN;
- **con las copias de la v4.8 y la lista de 478** (`v48/pbt4b-guard-walk-v48.tsv`, `v48/compare_walks_v48-copies-v48-out.txt` y `v48/walk-v48-flagged-diff.txt`): las pruebas de la revisión, las mismas; **ninguna cambia de categoría**; fuera de la lista, sin contar las copias ni el kit, **133 marcadas: las 131 de la v4.5 y las dos de arriba**, declaradas. La de la sexta tanda ya está en la lista.

**Si la precondición 6 de la v4.8 hace fallar alguna de esas dos, para** y vuelve a la sesión.

**Las 23 las rompe el cambio del anfitrión de PB-T4b, no la retirada** (M-33). La v4.3 decía que 470 era exacto con cualquier opción; no miraba las que la lista de PB-T4d clasificó por texto (WPUG:822-840), sin GREEN de PB-T4b no podían medirse las otras cuatro (§17.1 de la revisión), y sin el código de PB-T4a no podían verse sus dos CT (V45-01).

Las otras pruebas de las mismas guardas que nombran lo retirado solo por su ayudante (las autopruebas de `HostViolations` que exigen `NotEmpty`, y G-4 :510-517) siguen en verde en PB-T4b y están en la lista de PB-T4d (C-7). **IG:510-517 pasa en vacío:** G4 ya tiene una violación en el GREEN, porque el núcleo deja de nombrar `Execute`; por eso el equivalente de :504-508 trae su autoprueba sobre `ExecuteFull` (§17.2 de la revisión). **Lo mismo pasa con las autopruebas de `HostViolations` de PEG que quedan fuera de la lista** (CA-T4b M-01; V49-02): PEG:401-416, :418-431, :433-451, :453-468, :470-492 y :516-526 (6 métodos, 31 resultados) **pasan en vacío en el GREEN**, porque el anfitrión del GREEN ya incumple la regla del adaptador estático del piloto (PEG:279-280), así que sus `NotEmpty` se cumplen sin la mutación. **La mitigación:** CT-20 (1) y (4) fijan el texto de `HostViolations` del original y de la copia; la copia conserva parte del lado que salta (la `Theory` partida de D-S5, la del orden del vuelo y la de la trampa); y PB-T4d (C-7) las sustituye. **La autoprueba de la guarda de los textos de WHHG** (WHHG:501-526) pasa en vacío por la misma razón, y esa sí la trae el equivalente de `NoneOfThe30…` (CA-T4b I-01; arriba).

**La UI de la elección (C-3; 27):** 21 de `WorkspacePilotReconciliationTests` (entre ellas :780-793, «Asking for an option calls no operation of the port and only reads its memory») y 6 de `WorkspacePilotReconciliationViewTests`. **Con D-S2 su texto sigue valiendo:** `RequestKeepMine` pregunta desde la memoria y el primer «Sí» llama a `TryKeepMine` como hoy, y el doble del puerto responde lo que el `KeepMineResponder` de cada prueba le dice. Están en la lista, que el Owner tomó como la que vio, y se sustituyen por su copia sin cambio (D-PBT4-11); el comportamiento nuevo lo fija CT-PBT4B-14.

**Las que siguen en verde:**

| Guarda o prueba | Por qué |
|---|---|
| `WorkspaceDraftPilotValidationParityTests:80-97` | Los parámetros de la caja no cambian |
| `NumericFieldValidationTests` y las 26 clases de UI que usan `NumericField` | Misma API y mismo comportamiento |
| EIG:670 y :720 (la base en WHC), y la `Theory` EIG:743 (el alias) | WHC no nombra `.InitialDraftState`, `.StaleBase` ni `.Entries` fuera de los consumidores, y `pilot` solo es `read.PilotRead` (D-S3) |
| RG:104, :128 y :132 a :140 | D-S2: `TryKeepMine` sigue nombrado una vez, como `_port.TryKeepMine(request)`, dentro de `ConfirmChoice` (`v44/measure_rg_pbt4b-out.txt`) |

### 4.3 PB-T4c (V50-01, V50-02, V50-10 a V50-12 y V50-16)

**Medido en el cierre de PB-T4b** (`6adff90d`): por objetos git, con el paseo de guardas sobre el GREEN literal, y por ejecución, con la suite completa de Core y los filtros de UI de §6.3.1 sobre el RED exacto y sobre el GREEN literal (la precondición 6 de la v5.1, `/d/r5j`; §6.3.1).

**La lista de 4** (`pbt4c-substitution-list-4.tsv`, sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de`), la que el Owner tomó como la que vio:

| # | Guarda o prueba | Causa | Su sustituta |
|---|---|---|---|
| 1 | **PEG:155-163** (`ThePortDeclaresTheSeventeenOperations…`), las 17 operaciones por reflexión | C-5: `TryEditAll` es la 18.ª | `PushBackEditAllPortEquivalenceGuardTests`: la prueba y los miembros de PEG que alcanza, con la fila de `TryEditAll` al final de `Operations` (18) |
| 2 | **CT-PBT1B-15** (`PushBackSharedReferenceGuardTests.cs:306-403`; «archivos=14» en :384) | C-5 (B-07): `RackWorkspaceEditorWindow.cs` nombra `PushBack` | `PushBackEditAllNameWritersGuardTests`: la prueba y sus ayudantes, con «archivos=15»; lo demás, igual |
| 3 | **CT-PBT4B-20** (`WorkspaceFullCopyGuardTests.cs:568-654`) | C-5: el `Skip` de PEG:155 deja PEG distinto de su `BaseSha` (PEG es uno de sus 21 originales y `SkipLine` solo devuelve los `Skip` de PB-T4b); medido en `/d/r5a` con solo los dos primeros `Skip`: 19 605 = 19 144 + 1 + 460 (`owner-pbt4c-list-3-decision.md`) | `PushBackEditAllFullCopyGuardTests`: el archivo entero, con la clase renombrada y `Restore`, que además devuelve a `[Fact]` la línea exacta del `Skip` de PB-T4c de PEG:155; sus líneas esperadas no cambian |
| 4 | **CT-PBT2B-06** (`PushBackWorkspaceApplyCloseTests.cs:686-779`, UI) | C-3: su ventana 4 (`StageModuleEdit`) deja pendientes un módulo y cambios sin aplicar, y fija que la pregunta del módulo «manda primero»; D-PBT4-17 (A-T2b-OWNER-01) la cambia por la combinada; medido en `/d/r5c` (`owner-pbt4c-list-4-decision.md`) | `PushBackEditAllApplyCloseTests` (UI): la prueba y lo que alcanza de su archivo, con la línea de la ventana 4 cambiada a la pregunta nueva |

**Las que siguen en verde,** medidas (los anclajes, en `v50/remeasure_pbt4c_v50-out.txt`):

| Guarda | Por qué sigue en verde |
|---|---|
| `WorkspaceHostSourceGuardTests:398-421` | WCo no nombra `RackCad.UI` (el modal está anidado y no nombra la ventana) |
| EIG:1011-1024 y `WorkspaceNavigationHostTests:713-723` (9 archivos en `Plugin/Workspace/`) | Ningún archivo nuevo en `Plugin/Workspace/` |
| `WorkspaceBridgeSourceGuardTests:361-366` y :139-142 | `RackWorkspaceEditorWindow.cs` no nombra `RackCad.Application.Workspace`; `FullEditorModal` no nombra ningún diálogo |
| G-9 (IG:593-595) | El abridor no nombra el piloto ni cuenta en las cuentas de RIA y NH |
| EIG:1155-1184 (el cableado de `SetPilotUpdateHost`) | `SetFullEditHost` va después, en el mismo getter |
| CT-PBT2B-08 (`PushBackWorkspaceModeSourceGuardTests:307-364`) | R10 cuenta los once literales uno a uno (:271); el literal nuevo no es uno de ellos |
| URT:1108-1109, EIH:1060 y EIH:1079 | Sin valor nuevo de `PilotBoundaryKind`; `TryEditAll` no lleva «Update»; `_fullEditHost` es de `Application.Workspace` |
| La copia de PEG:379-386 (`WorkspacePilotEditGuardFull.cs:266-272`) | WHC conserva sus dos interfaces |
| PEG:1015 y PEG:1079 | Los nombres de los tipos del puerto y del soporte evitan `Draft\|Editor\|Subscription\|Tab`; el comentario del puerto conserva «six» |
| `ControllerViolations` (PEG:659-707), la compuerta (PEG:602-633) y los diálogos (PEG:809-841) | Los límites de WCo y de la compuerta, sin cambio |
| El censo de portadores (EIH y su copia) | `EditAllOutcome` no casa con la familia de nombres |
| El equivalente de `NoneOfThe30…` (WHHG) | `TryEditAll` no repite ninguno de los 30 textos |
| `WorkspaceModelBoundaryTests:93-104` | **D-S9:** `IWorkspaceFullEditHost`, sin «Editor» (con `IWorkspaceFullEditorHost` caía; `/d/r5c`, y el arreglo en `/d/r5e`) |
| CT-PBT4B-12, 13, 16, 19 y 21 | Medidas con la suite completa: en verde |

**El paseo de guardas** (`walk_guards_pbt4c_v50.py`, el método de la v4.8 con sus tuplas, sobre el GREEN literal y las pruebas de `6adff90d`, que incluyen las de PB-T4b, y los cuatro equivalentes por su ruta virtual): **295 clases y 3 902 pruebas sin `Skip` de las clases que nombran un archivo tocado o su carpeta; 596 marcadas (A: un literal cambia de cuenta), 514 sin diferencia y 2 792 que no leen ninguno (`v50/pbt4c-guard-walk-v50b-out.txt`; la lectura por tipo de diferencia, `v50/walk-analysis-v50b-out.txt`)**. Casi todas las marcas salen de los literales que aparecen en los cuatro archivos nuevos o de cuentas que crecen en archivos que la guarda lee entero; las que importan, las ve la suite completa.

**Lo que el paseo no ve** (V50-12), declarado: **la reflexión sobre tipos** (la caída de D-S9: `WorkspaceModelBoundaryTests` recorre los tipos del ensamblado, sin ningún literal de archivo) y **el comportamiento de la UI** (la de CT-PBT2B-06: una prueba de ventana, sin texto de fuente). **Por eso lo que vale es la suite completa de Core y los filtros de UI** de la precondición 6, que las vieron las dos.

**Si cualquier otra prueba falla,** fuera de la lista de 4, es PBT4C-C16 y vuelve al Owner (pregunta 4).

### 4.4 PB-T4d (V52-01 a V52-04; V53-01 a V53-04; V54-01, V54-04 y V54-06)

**Medido en el cierre de PB-T4c** (`6c2820ba`): por objetos git (la remedición, `v52/remeasure_pbt4d_v52-out.txt`) y compilando y ejecutando en copias exportadas: la necesidad de cada baja desde cero bajas (`/d/r6h`), la cobertura de la corrida genérica (`/d/r6j` y `/d/r6k`, y rama por rama en `v52/sub/cov-sides.txt`), dos ensayos (`/d/r6l` y `/d/r6n`), y la precondición 6 de la v5.2 y, tras la comprobación acotada CA-T4d, la de la v5.3 (§6.4.1), con sus ensayos (`/d/r6v` y `/d/r6x`).

**La lista** (`pbt4d-retire-list-271.tsv`, sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac`), por grupo y acción:

| Grupo | Acción | Filas |
|---|---|---|
| C-7 | borrar con registro | 75 |
| C-7 | reescribir con registro | 27 |
| C-7 | se queda | 7 |
| C-7 | sustituir con registro | 29 |
| M-03 | borrar con registro | 35 |
| M-04 | reescribir con registro | 3 |
| SUS-01 | borrar con registro | 1 |
| apoyo | borrar con registro | 21 |
| apoyo | reescribir con registro | 2 |
| apoyo | se queda | 6 |
| ayudante-editado | reescribir con registro | 6 |
| compilacion | borrar con registro | 11 |
| compilacion | sustituir con registro | 1 |
| directiva | reescribir con registro | 1 |
| ejecucion | sustituir con registro | 3 |
| lenta | acotar con registro | 1 |
| original | borrar con registro | 55 |
| **Total** | | **284** (271 con acción) |

**Los grupos:** C-7, las 138 de la lista de la v4.4 (por texto o por compilación); `original`, las 55 originales saltadas desde PB-T4b; `apoyo`, sus ayudantes; `compilacion`, lo que deja de compilar con la retirada B (medido en `/d/r6e`); `ejecucion`, las tres que caen al ejecutar (D-S12); M-04 (D-S13); `lenta` (D-S14); SUS-01; `M-03`, los ayudantes que la retirada deja sin uso y las tres clases que deja sin pruebas (CA-T4d M-03); `ayudante-editado`, los ayudantes que el RED edita sin fila propia (CA-T4d M-04 b). **Las acciones:** «borrar con registro» (la baja va en el RED), «reescribir con registro» (se queda, editada en su sitio), «sustituir con registro» (`Skip` y equivalente, o, en WPUR, baja con su equivalente en `PushBackRetirePilotRunTests`), «acotar con registro» y «se queda» (sin tocar; no se emite). **Desde la v5.3** (CA-T4d M-04), dos columnas más: `declaraciones-6c2820ba`, todas las declaraciones del nombre en su archivo (las sobrecargas), y `tramo-editado`, las líneas de la base que el RED cambia dentro de ellas, medidas miembro a miembro contra el árbol final del RED; `make_list_v53.py` comprueba que toda línea de código que el RED cambia cae en alguna fila. Los porqué de C-7 dicen lo que se hace (borrar), no «sustituir».

**Lo que cambia la medida frente a la lista por texto** (V52-02 y V52-03):
- **34 de las 235 bajas de B no hacen falta** (`/d/r6h`): 7 CT de PB-T3b y sus 6 ayudantes se quedan; las otras se reescriben o se borran por lo que fijan;
- **las 48 de WPUR** recorrían 436 líneas de la corrida genérica que se queda; el resto de la suite cubría 401. **27 sustitutas** cubren lo demás, línea a línea y rama por rama (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 2); **y la 48 y la 49** no prueban la corrida, sino `PushBackPilotSystemRebuild`, que se queda (lo usan `DesiredNamesOf` y el escritor real de «Actualizar»), y son las únicas que cubren su línea 32 y 6 ramas (CA-T4d I-01): **29 sustitutas** (`owner-pbt4d-cat4d-decisions.md`, pregunta 1); las otras 19 se borran, y una se queda en WPUR;
- **la rama muerta de `BuildCore`** (V52-04): la vista no disponible solo la alcanzaba el piloto; el GREEN la quita, y E-3 de D-1 pierde las dos reglas que la nombraban.

**Las que siguen en verde,** medidas con la suite completa de Core y los filtros de UI (§6.4.1): las guardas que leen el texto del plan (`PushBackDraftWriteGuardTests`, `PushBackFullPanelCensusGuardTests`, `PushBackFullReadGuard*`, `PushBackWritePathGuardTests` y su copia, `CustomPropertiesEnvelopeGuardTests`), las 24 copias candidatas de la remedición (falsos positivos de la lectura por texto) y la copia lenta de OWNER-05, acotada.

**La cobertura, ampliada** (CA-T4d I-02; `owner-pbt4d-cat4d-decisions.md`, pregunta 2): la precondición 6 compara la cobertura de **toda la suite de Core** y de la **unión de los filtros de UI** de PB-T4c y de PB-T4d, en la base sin cambios y en el GREEN, sobre **todo el código de `src/` que se queda** (Domain, Application y UI, instrumentados con coverlet en JSON), línea a línea y rama por rama (`cov_p6_v54.py`). Lo que solo cubre la UI se mide con la cobertura de sus filtros antes de darlo por perdido. **La regla del Owner:** todo camino que se queda y pierde su única prueba recibe una sustituta sobre el modelo completo, sin volver a él; vuelve solo si una sustituta no se puede escribir o si salen más de 10 sustitutas nuevas sobre las 29. Las 436 líneas de la v5.2 y sus 9 ramas se quedan como conjunto vigilado; **desde la v5.3 se cuentan 537** (433 se quedan), porque ahora entran también las líneas de WPUR que caen dentro de los tramos que el GREEN borra enteros, que la v5.2 quitaba antes de contar (recomprobación 1 de CA-T4d, M-13). La UI de cada rama es la unión de tres corridas de la unión de los filtros y de cada filtro en su proceso: un binding de WPF da golpes que dependen de lo que corrió antes (`noise-batch/`; R-T4-38). **Lo medido en la v5.3:** archivos de src/ instrumentados 775; lineas ejecutadas en la base que se quedan 69842 (ramas cubiertas 28818); retiradas por el GREEN 419; texto cambiado 4; lineas sin ejecutar en el GREEN 0; lineas con ramas perdidas 0; **PASA** (`cov-p6-v54.txt`); conjunto vigilado de la v5.2: 537 lineas de WPUR, 433 se quedan, 0 sin cubrir en el GREEN; bajan (informativo) 16801, de ellas PERDIDA 0. **Bajan** (informativo; D-S16: lo que cubren más corridas en la base que en el GREEN, por componente emparejado; no cambia el PASA; cada entrada nombra la corrida que la cubría, no la prueba, porque coverlet no da la prueba; y ESTABLE solo por la UI exige las tres corridas juntas y al menos un filtro en su proceso: recomprobación 1 de CA-T4d, M-11): entradas 16801: PERDIDA 0, VARIA 0, ESTABLE 16801; y, solo por la corrida de un filtro que no esta en el GREEN (RackCad.UI.Tests.PushBackFieldValidationConcordanceTests), 118 lineas o ramas en 3 archivos; **ninguna PERDIDA**; VARIA, declaradas (una prueba que no cambia y una cobertura que varía entre corridas, no caminos perdidos): ninguna; las ESTABLE (16801) las sigue cubriendo el GREEN con Core, o con las tres corridas de todos los filtros juntos y al menos un filtro en su proceso (`cov-p6-v54.txt`, «Bajan (informativo)»). Las 37-38 de `RackViewBatchDialogPresenter.cs`, el ruido que motivó D-S16, no bajan en esta corrida (la unión las cubre en los dos lados); se declaran igual en `compile-check.md`: no son un camino perdido.

**Lo que la medida no ve,** declarado: la cobertura de datos (que un corpus pase entero por un camino que ya está cubierto línea a línea; por eso M-01 se resolvió por lectura, no por la medida); el Plugin, que no carga ninguna prueba; y la UI fuera de los filtros, que no cambia (el RED solo quita pruebas de UI dentro de ellos y el GREEN no toca la UI); la CI completa de UI lo cubre (C06/C16).

**Si cualquier otra prueba falla,** fuera de la lista, es PBT4D-C16 y vuelve al Owner (`owner-pbt4d-list-decisions.md`, pregunta 4).


## 5. La red de pruebas

### 5.0 Reglas comunes (enteras; V41-12)

**Las reglas de PB-T1 a PB-T3,** que se copian en cada contrato y no se heredan:
- `[Fact]` que acaban en **un** `Assert.Equal` de las líneas unidas por `\n`, con «R», cultura invariante, GUID fijo y orden ordinal;
- **un `[Fact]` por CT. Ninguna `[Theory]` nueva:** la TRX cuenta distinto una `Theory` con `Skip` (F4B-E46);
- la regla «EXCEPCION» para toda llamada al API nuevo: una excepción se escribe como la línea `EXCEPCION <Tipo>: <mensaje>` y no escapa de la CT;
- **las pruebas de UI:**
  - en `EditorDiscardPromptCollection`, con la carga dentro del `try` y `Cerrar` en `finally`;
  - sin teclado ni foco real, sin `MessageBox` real y sin ventanas que no sean la de Push Back;
  - **ninguna crea un `RackFrameConfiguratorWindow`** (F4B-E30), ninguna cierra una ventana con trabajo pendiente y ninguna escribe en el perfil;
- sin AutoCAD, sin BOM, sin «Guardar en biblioteca» y sin «Insertar»;
- ningún fixture nuevo en `Fixtures/I64`;
- **los ayudantes existentes se usan sin editarlos** (`IntentKit`, `PilotKit`, `DraftKit`, `PilotUpdateScopeDouble`, `PilotEraseScopeDouble` de `PushBackFullWriteSupport.cs`, `WorkspaceHostTestSupport`, `WorkspacePilotEditPortDouble`, `EditIntentKit`). **Lo que falte va en un archivo nuevo de la subtarea:**
  - PB-T4a: `tests/RackCad.Tests/PushBackFullReadSupport.cs`, con `PushBackFullReadKit` (las lecturas `FromReads` sobre `IntentKit`) y `PushBackOriginReaderDouble` (el lector con origen, que registra lo que recibe);
  - PB-T4b: el kit nuevo de «Saltar y duplicar» (`WorkspaceFullReadKit.cs`, D-PBT4-11, con `FullHandKit` y `FullReadRig`); las CT de UI usan `WorkspacePilotEditPortDouble`, sin editarlo;
  - PB-T4c: el doble del anfitrión del editor.

**El momento de cada lectura** (F4B-E30 y F4B-E33): cada línea se arma justo después de su último gesto. El borrador guarda copias, y el rebase de POST cambia `InitialDraftState` y `CurrentDraftState`. La huella se toma tras cada operación.

**Los gestos que no cambian nada** (la lección M-13): una edición escribe un valor distinto del que tiene el diseño, en un campo que el ensamblador conserva. Se usan los valores medidos de PB-T3: `PalletTolerance` 4.5 y `PalletDepth` 48.5. `BeamDepth` no se edita.

**Los archivos nuevos se comprueban contra las guardas existentes antes de fijar el esqueleto** (la lección de F4B-E42 y F4B-E45). Antes de emitir, la sesión:
- compila el esqueleto exacto del RED sobre una copia exportada (fuera del worktree);
- ejecuta la suite completa de Core y los filtros de UI de la subtarea;
- barre los nombres nuevos y los textos del esqueleto contra los literales de todas las guardas que leen `src/` (el barrido de nombres y el de sobrecargas);
- **mide las guardas sobre el texto literal del GREEN que prescribe el paquete,** aplicado en memoria, y **las recorre por marcas**, prueba a prueba, en las clases que nombran los archivos tocados o sus carpetas, con lo que las marcas no ven medido aparte (para PB-T4a, §4.1: `walk_guards_pbt4a.py`, la ampliación de CA-T4a y `measure_pbt4_v43-out.txt`); y, sobre la copia, **compila el `src/` del GREEN literal** con `apply_green_pbt4a.py` y corre la suite de Core sobre él.

**Ningún `?` de tipo de referencia anulable sin `#nullable enable`** (CA-T4a I-01; V43-01). `Directory.Build.props:28` fija `<Nullable>disable</Nullable>`, y ningún `.csproj` lo cambia (medido en `7f0d9aa3`): ni `src/RackCad.Application`, ni `src/RackCad.Domain`, ni `src/RackCad.Plugin`, ni `src/RackCad.UI`, ni `tests/RackCad.Tests`, ni `tests/RackCad.UI.Tests`, ni las dos herramientas de `tools/`. Por eso:
- un archivo nuevo o una línea nueva, de `src/` o de `tests/`, no escribe `string?`, `object?` ni ningún `T?` de un tipo de referencia, salvo que el archivo empiece por `#nullable enable` (como WEI, WHC, RIA o la interfaz nueva);
- **en `Draft/`, nunca:** su pureza prohíbe la directiva (CT-PBT3A-14 y CT-PBT4A-08);
- los `?` de tipos de valor (`long?`, `double?`, una tupla `(…)?`) y el operador `!` no cuentan;
- el motivo: cada uno da **CS8632**, y `build-plugin` compila la solución en Release y falla con cualquier `warning CS` (`eng/ci/verify-autocad-references.ps1`, `Assert-DotNetSuccess -NoWarnings`).

**Revisado frente a la regla** (`measure_pbt4_v43-out.txt`): el código literal de PB-T4a (WEI, WHC, RIA y la interfaz llevan `#nullable enable`; NH no lo lleva y su texto nuevo no tiene ningún `?` de referencia; `Draft/PushBackOriginByText.cs`, ya corregido) y el de PB-T4b y PB-T4c (`NumericTextRule.Check`, con `double?`; `PushBackAnnotationScaleRule.Validate`; `PushBackKeepMinePresentation.Of` y `Read`; `IWorkspaceFullEditorHost.ShowModal<TLaunch>`; `FullEditorModal.ShowModal`, con `Action<object>`; `RackWorkspaceEditorWindow.Show(object launch)`). **Solo cambia `PushBackOriginByText.cs`.** Lo que el paquete no da literal (el `NumericTextCheck` de PB-T4b, o `_fullEditorHost` en WHC) sigue la regla al escribirse: en un archivo sin `#nullable enable`, sin `?`.

**Un solo «Actualizar» lógico:** las recetas de PB-T4b no llaman a «Actualizar» desde la ventana; solo desde el panel.

**Las CT de I-24** (CT-PBT4A-08, CT-PBT4B-19 y CT-PBT4C-15): copian, sin referenciarlos, las reglas de G-L1 de CT-PBT2A-04 (`PushBackDraftPortSharedReferenceGuardTests.cs`: `Tokens`, `Rebind`, `Rule1`, `Rule1b`, `Rule1c`, `Clean`, `Collapse`, `Members`, `Aliases`, `Violations` e `Inject`, con sus 15 autopruebas) y la pureza de CT-PBT3A-14 (`ForbiddenTerms` y `Purity`). Cada autoprueba se inyecta en memoria en el texto de **cada** archivo de su lista, y falla si las reglas la ven en todos.

### 5.1 PB-T4a: la lista cerrada CT-PBT4A-01 a 08 (Core; entera)

| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |
|---|---|---|---|---|
| 01 | `FromReads`: `PilotRead`, `FullRead` y `Catalog`; un nulo da `ArgumentNullException`; `FromPilot` y los demás estados los dejan en null | `fromreads piloto=True completo=True catalogo=True estado=Read`; `fromreads-nulo piloto=ArgumentNullException completo=ArgumentNullException catalogo=ArgumentNullException`; `frompilot completo=null catalogo=null`; `otros completo=null catalogo=null` (`NoActiveDocument`, `Busy`, `Rejected` y `Failed`) | `PushBackFullReadPathTests` | true |
| 02 | El lector completo y el del piloto sobre el mismo escaneo de `IntentKit`, por `PushBackFullReadKit` (`FromReads`): `pb-01-base`, `pb-20-ab` (rechazado por el piloto y admitido por el completo), `pb-21-cola` y el caso de AUTH-13 de PB-T3a | Una por diseño, `<diseño> piloto=<Admitted|NotAdmitted> completo=<Admitted|NotAdmitted> mismo-escaneo=True`; entre ellas, literal, `pb-20-ab piloto=NotAdmitted completo=Admitted mismo-escaneo=True` | `PushBackFullReadPathTests` | true |
| 03 | **Las cinco relecturas** usan `ReadIntentAtOrigin` con `draft.CurrentDraftState.Origin` cuando el lector la ofrece (`PushBackOriginReaderDouble`); `Begin` usa `ReadIntent` con `ChosenReference` | `Begin lector=ReadIntent`; y, por cada una de `CheckNow`, `Reload`, `ConfirmBase`, `KeepMine` y `DiscardAndReload`, `<operación> lector=ReadIntentAtOrigin origen-del-borrador=True` | `PushBackFullReadPathTests` | true |
| 04 | **La elección por texto y la relectura, puras:** `PushBackOriginByText.Choose(DraftStaleBase, string)` sobre `PushBackPilotIntentReader.BuildStaleBase` del escaneo, como UR:645-649; `PushBackOriginByText.AtKey`; y `PushBackOriginByText.ReadAt` (B-08) | `choose uno-mutable=<clave>`; `choose ninguno=null`; `choose varios=null`; `choose no-mutable=null`; `atkey clave=<clave> resto-igual=True`; `readat otro-origen clave=<clave> piloto=<estado> completo=<estado> mismo-escaneo=True`; `readat mismo-origen=null`; `readat sin-coincidencia=null` | `PushBackFullReadPathTests` | true |
| 05 | Con un lector sin la interfaz nueva, las cinco relecturas usan `ReadIntent` como hoy | Por cada operación, `<operación> lector=ReadIntent` | `PushBackFullReadGuardTests` | **false** |
| 06 | El censo: solo `Draft/`, PP, UR y RIA nombran los cinco tipos de CT-PBT3A-12. **Sustituye a CT-PBT3A-12** | Las de CT-PBT3A-12, iguales: `sin-conectar archivos-revisados>0=True coincidencias=[]` y las diez `autoprueba-<nombre>-en-<destino>=falla` (los cinco nombres en `WorkspaceHostCore.cs` y en `WorkspacePilotSupport.cs`) | `PushBackFullReadGuardTests` | **false** |
| 07 | **La guarda del Plugin, en texto,** con autoprueba en negativo sobre texto inyectado en memoria | `ria-firma-de-Read-sin-cambio=True`; `ria-Read completo=1 tras-el-piloto=True`; `ria-lectores piloto=1 completo=1 escaneos=1`; `ria-AtOrigin readat=1 piloto=0 completo=0 base=0 lecturas-del-dibujo=0 literales=0`; `nh-lector interfaces=IWorkspaceEditIntentReader,IWorkspaceEditIntentOriginReader`; `nh-ReadIntentAtOrigin ReadOnActiveDocument=1`; `nh-AtOrigin=1 dentro-del-bloqueo=True tras-Read=True`; `nh-resultado FromReads=1 FromPilot=0`; `autoprueba-sin-lector-completo=falla`; `autoprueba-AtOrigin-con-CaptureDrawing=falla`; `autoprueba-segundo-AtOrigin=falla`; `autoprueba-FromPilot-en-NH=falla`; `autoprueba-segunda-lectura-del-piloto-en-RIA=falla` | `PushBackFullReadPathTests` | true |
| **08** | **I-24:** G-L1 y la pureza sobre el archivo nuevo de `Draft/` de PB-T4a, `PushBackOriginByText.cs` (§5.0) | **32:** `g-l1 archivos=1 reglas-sin-coincidencias=True`; las 15 `autoprueba-<a, b, c, e, f1, f2, f3, g1, g2, h1, h2, i, j, k1, k2>=falla`; `pureza archivos=1 violaciones=[]`; `autoprueba-#nullable=falla`; `autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla`; y las 13 `autoprueba-<término>=falla` de `ForbiddenTerms` | `PushBackFullReadGuardTests` | **false** |

- **En CT-07,** `literales=0` y `lecturas-del-dibujo=0` son la restricción de CAG (D-PBT4-02): el cuerpo de `AtOrigin` no tiene ningún literal de cadena, ni `CaptureDrawing`, `LockDocument`, `Transaction`, `AssertNoOpenTransaction` o `ex.Message`. `ria-firma-de-Read-sin-cambio` compara la firma con la expresión de EIG:139-141. **`ria-lectores` es la cuenta de EIG:356-357 en todo RIA** (B-08): un `PushBackPilotIntentReader.Read(`, un `PushBackFullDraftReader.Read(` y un `new PushBackPilotIntentScan(`; `autoprueba-segunda-lectura-del-piloto-en-RIA` inyecta una segunda lectura del piloto en el texto de RIA.
- **CT-08 pasa en el RED:** el esqueleto ya crea `PushBackOriginByText.cs` (con `Choose` que devuelve null, `AtKey` que devuelve el escaneo y `ReadAt` que devuelve null), puro y sin ninguna ficha de G-L1. En el GREEN, el archivo llama a los dos lectores, como `PushBackFullDraftReader.cs`, y sigue sin ninguna ficha ni ningún término de la pureza (medido). Sus autopruebas fallan sobre el texto inyectado.

**Las clases:** `PushBackFullReadPathTests` (01 a 04 y 07), con `ExpectRed` true; `PushBackFullReadGuardTests` (05, 06 y 08), con false. Las dos y `PushBackFullReadSupport.cs`, en `tests/RackCad.Tests/`, con `namespace RackCad.Tests`; ningún filtro heredado las selecciona por prefijo.

**Los totales** (F4B-E46: una sola rama, porque PB-T4a no quita ningún `Skip` ni añade ninguna `Theory`). La base: el cierre de PB-T3c (CI `44757675821` sobre `938a35f0`), **Core 19 129 = 19 123 `Passed` + 6 `Skipped`**, y UI 2 451 = 2 430 + 21. PB-T4a añade **8 CT, ocho `[Fact]`,** y pone **un `Skip`** (CT-PBT3A-12, un `[Fact]`):

| Rama | Core | UI |
|---|---|---|
| **RED** | **19 137 = 19 125 `Passed` + 5 `Failed` (CT-01 a 04 y 07) + 7 `Skipped`** | 2 451 = 2 430 + 21, sin cambio |
| **GREEN** | **19 137 = 19 130 `Passed` + 0 `Failed` + 7 `Skipped`** | 2 451 = 2 430 + 21 |

- 19 125 = 19 123 − 1 (el `Skip`) + 3 (CT-05, 06 y 08, que pasan en el RED).
- **La suite de Core sobre la copia con el esqueleto** (el parche de `src/` y el `Skip`, sin las clases nuevas; precondición 6): **19 129 = 19 122 + 7**. Sin el `Skip` daría 19 129 = 19 123 + 6, y CT-PBT3A-12 pasaría, porque RIA no cambia en el esqueleto.
- **D = 0** (I-25): ninguna otra prueba falla. **En el RED,** se confirma en la precondición 6, que corre la suite completa sobre la copia con el esqueleto (19 129 = 19 122 + 7). **En el GREEN,** lo sostienen el recorrido por marcas de §4.1 y la ampliación de CA-T4a sobre los 97 archivos de fuera, por texto; y **lo mide la precondición 6** con `apply_green_pbt4a.py`: el `src/` del GREEN literal sobre la copia y la suite completa de Core, que debe dar **19 129 = 19 122 + 0 + 7** con el `Skip` de CT-PBT3A-12 puesto (19 129 = 19 122 + 1 + 6 sin él, con CT-PBT3A-12 como única caída) y ningún `warning CS`. Lo que queda es una lectura que solo se ve al ejecutar las clases nuevas del Worker: si una prueba fuera de la lista cae en el GREEN, es PBT4A-C16 y vuelve al Owner.
- **El arreglo de B-08 no cambia los totales:** `ReadAt` es un método de un archivo que ya existía en el esqueleto, y CT-04 y CT-07 siguen siendo un `[Fact]` cada una. Con el texto de la v4.1, el GREEN habría dado 19 129 `Passed`, 1 `Failed` (EIG:350-364) y 7 `Skipped`.

### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 21 (entera; V44-07; V45-02, V45-03, V45-11; V47-02, V47-03; V48-01, V48-05)

**Las reglas de §5.0,** enteras: un `[Fact]` por CT, que acaba en un `Assert.Equal` de las líneas unidas por `\n`; ninguna `[Theory]` nueva; la regla «EXCEPCION»; las de UI; los ayudantes existentes sin editar. **Las CT de Core van en `tests/RackCad.Tests/`, con `namespace RackCad.Tests`; las de UI, en `tests/RackCad.UI.Tests/`, con `namespace RackCad.UI.Tests`.** Ninguna clase empieza por `Workspace` salvo `WorkspaceFullCopyGuardTests` (CT-20), que el filtro heredado `RackCad.Tests.Workspace` también selecciona y pasa en el RED.

**Los ayudantes de las CT:** el kit (`WorkspaceFullReadKit.cs`, §6.2.1), `IntentKit`, `PilotKit`, `DraftKit`, `WorkspaceHostRig`, `PilotUpdateScopeDouble` y `PilotEraseScopeDouble` (Core), y `WorkspacePilotEditPortDouble` (UI), sin editarlos. **Los escaneos realistas** son los de `FullPrepareKit.RealScan` (el frontal `A1` con sección 0, el lateral `B2` en `ExistingPost()` y la planta `C3`), salvo que la CT diga otro.

| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |
|---|---|---|---|---|
| **01** | **El borrador del panel es el completo** (D-PBT4-04): `TryBeginPilotEdit` sobre `FullEditIntentKit.Admitted` deja `Find<PushBackFullDraftState>` no nulo y `Find<PushBackPilotDraftState>` nulo; su editable es igual (comparador) al de `PushBackFullDraftReader.Read` del mismo escaneo, y su `Origin` es el texto del sobre `A1`. Con un lector que devuelve `FromPilot`, `NotAdmitted` con `FullReadMissingReason` y ningún borrador | `begin tipo=PushBackFullDraftState piloto=null editable-igual=True origen-igual=True`; `begin-sin-completo estado=NotAdmitted diagnostico=No editable aquí: la lectura no trae el modelo completo. pista=True borradores=0` | `PushBackFullPanelTests` | true |
| **02** | **OB-03.2 por el anfitrión:** `pb-20-ab` abre su borrador completo; `pb-21-cola` sigue rechazado con `OutOfPilotDiagnostic` y su pista; el caso de AUTH-13 de PB-T3a sigue rechazado | `pb-20-ab estado=Admitted borrador=PushBackFullDraftState`; `pb-21-cola estado=NotAdmitted diagnostico-fuera-del-piloto=True pista=True borrador=null`; `auth-13 estado=NotAdmitted borrador=null` | `PushBackFullPanelTests` | true |
| **03** | **El campo de escala sobre el completo:** `SetPendingText` y `CommitField` del puerto cambian solo `Editable.AnnotationScale` (comparador sobre una copia con la escala de antes) e `IsDirty` pasa a `True`; la validación es la de `PushBackAnnotationScaleRule` sin asignar nada | `campo commit=Applied escala=2.5 resto-igual=True isdirty=True`; `campo texto=[] estado=Invalid mensaje=Escribe un valor.`; `campo texto=[abc] estado=Invalid mensaje=Valor no numérico.`; `campo texto=[0] estado=Invalid mensaje=El valor debe ser > 0.`; `campo texto=[-1] estado=Invalid mensaje=El valor debe ser > 0.` | `PushBackFullPanelTests` | true |
| **04** | **La regla única, frente a la caja:** `PushBackAnnotationScaleRule` (`Minimum` y `MinimumInclusive`, y `Validate` en los diez textos de CT-PBT2B-10) frente a las propiedades de `AnnotationScaleBox` de una ventana real de Push Back y su validación | `parametros minimo=0 minimo-inclusivo=False iguales=True`; y, por cada uno de los diez textos, `texto-<n> valido-igual=True mensaje-igual=True` (n de 1 a 10) | `PushBackFullPanelFieldTests` (UI) | true |
| **05** | **«Actualizar» por `ExecuteFull`** (D-PBT4-07), con `PilotEraseScopeDouble`: un borrador cuyo estado actual (`SetCurrentDraftState`) deja obsoleto el lateral `B2` | `actualizar estado=Applied begin-mutation=1 seal=1 borrador=Clean`; `borrado EraseUnit:B2` | `PushBackFullPanelTests` | true |
| **06** | **Los pasos 1 y 2 de «Conservar»** (D-PBT4-08 y 09): `TryKeepPilotEdit` sin testigo sobre un borrador Dirty marcado como posiblemente obsoleto, con la escala 2.25 del usuario y, fuera, el nombre de las tres vistas cambiado (`IntentKit.Variant(…, name: "Otro nombre")`), da `Presented` con las líneas y el testigo, y el borrador no cambia (comparador y huella). **Y la función pura** `PushBackKeepMinePresentation.Of` sobre una clasificación fabricada (sus constructores internos): en «fuera», el nombre y dos entradas de los frentes; en «por ti», las anotaciones, la seguridad y los módulos; en «los dos», el rack; estructurales, una de los frentes y una de los módulos; dos vistas que borrar, una rechazada y todas obsoletas | **Anfitrión:** `presentar estado=Presented testigo-clave=A1 testigo-texto-igual-al-origen=True borrador-igual=True huella-igual=True`; `presentar Cambiado fuera: el nombre.`; `presentar Cambiado por ti: las anotaciones.` **Pura:** `of lineas=7`; `of Cambiado fuera: el nombre y los frentes.`; `of Cambiado por ti: las anotaciones, la seguridad y los módulos.`; `of Cambiado en los dos: el rack.`; `of Cambios estructurales: los frentes y los módulos.`; `of Vistas: se borrarán 2 laterales obsoletos.`; `of Vistas: 1 no se puede conservar.`; `of Todas las vistas quedarían obsoletas.`; `of-singular Vistas: se borrará 1 lateral obsoleto.`; `of-plural Vistas: 3 no se pueden conservar.`; `of-vacia lineas=0 isempty=True` | `PushBackFullPanelTests` | true |
| **07** | **El segundo «Sí» y OB-05.4:** con un testigo que ya no vale (el dibujo cambió tras la presentación), `Presented` de nuevo, sin conservar; con el vigente (el lector lo ve `Unchanged`), `KeepOn`: `Initial` es la relectura, `Current.Editable` el del borrador y `Current.Origin` el origen nuevo, y Dirty | `segundo-si testigo-viejo estado=Presented conservado=False borrador-igual=True`; `segundo-si testigo-vigente estado=Kept initial-igual-a-la-relectura=True editable-del-borrador=True origen-nuevo=True estado-del-borrador=Dirty` | `PushBackFullPanelTests` | true |
| **08** | **Los pasos 4 a 7 por el panel:** tras conservar y «Actualizar» (`PilotUpdateScopeDouble`), el sobre de cada miembro del plan lleva el diseño del borrador y el resto del sobre del rack actual (los campos no editables, la identidad y la versión) | `tras-conservar-y-actualizar estado=Applied diseno-del-borrador=True resto-del-rack-actual=True` | `PushBackFullPanelTests` | true |
| **09** | **OWNER-01 de PB-T2c:** un lado B declarado sin frentes escribe el diseño sin lado B (D-PB-08), y el borrador conserva la declaración hasta el rebase | `lado-b-sin-frentes diseno-escrito-sin-lado-b=True declaracion-hasta-el-rebase=True` | `PushBackFullPanelTests` | true |
| **10** | **D2, condiciones 3, 5, 6 y 7,** sobre el borrador del panel: la huella de las dos referencias compartidas, igual antes y después de cada operación | `Begin huellas-iguales=True`; `CommitField huellas-iguales=True`; `Presentar huellas-iguales=True`; `KeepOn huellas-iguales=True`; `Rebase huellas-iguales=True`; `sustituir-referencia isdirty=False` | `PushBackFullPanelTests` | true |
| **11** | **`NaN`** (A-9 y A2-13), declarado, sobre el borrador del panel: una longitud de módulo `NaN` en su estado actual, frente a su propia copia | `nan-longitud-modulo igual-a-su-copia=False` | `PushBackFullPanelTests` | true |
| **12** | **Sin traducción** (M-11), en texto: WHC, WEI, `WorkspacePilotSupport.cs`, RIA y NH no construyen un `PushBackFullDraftState` (`new PushBackFullDraftState(`); WHC no nombra `pilot.Read`, `KeepPilotEditsOn`, `PushBackPilotFieldAdapter`, `PushBackPilotStateComparer`, `Find<PushBackPilotDraftState>` ni `PushBackPilotUpdateRun.Execute(`, y nombra `PushBackPilotUpdateRun.ExecuteFull(` y `PushBackPilotUpdateRun.Precheck(` una vez cada uno, en `TryUpdatePilot`. Con autoprueba | `sin-traduccion archivos=5 coincidencias=[]`; `whc-sin-piloto nombres=[]`; `whc-actualizar ExecuteFull=1 Precheck=1`; y `autoprueba-<nombre>-en-WorkspaceHostCore.cs=falla` por cada uno de los seis nombres; `autoprueba-new PushBackFullDraftState-en-WorkspacePilotSupport.cs=falla` | `PushBackFullPanelGuardTests` | true |
| **13** | **El censo del modelo completo admite WHC; sustituye a CT-PBT4A-06:** solo `Draft/`, PP, UR, RIA y WHC nombran los cinco tipos de CT-PBT3A-12 | `sin-conectar archivos-revisados>0=True coincidencias=[]`; y las diez `autoprueba-<nombre>-en-<destino>=falla`: los cinco nombres (`PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` y `PushBackViewDispositions`) en `WorkspacePilotSupport.cs` y en `WorkspaceEditIntent.cs` | `PushBackFullPanelCensusGuardTests` | **false** |
| **14** | **La UI de «Conservar» con dos «Sí»** (D-S2), con la FVM real y `WorkspacePilotEditPortDouble`, cuyo `KeepMineResponder` responde `Presented` sin testigo y `Kept` con él; y la vista real del panel | `pedir llamadas-al-puerto=GetField`; `primer-si TryKeepMine=1 testigo=null`; `presentacion texto=[Cambiado fuera: el nombre.\nCambiado por ti: las anotaciones.] visible=True`; `xaml BrowserFieldKeepMinePresentationText dentro-del-panel-de-la-eleccion=True antes-del-si=True`; `segundo-si TryKeepMine=2 testigo=A1`; `conservado aviso=[Borrador conservado sobre la versión actual del dibujo.]`; `no llamadas-al-puerto=0` | `PushBackFullPanelFieldTests` (UI) | true |
| **15** | **La guarda de la elección,** en texto: FVM nombra `TryKeepMine` una vez, como `_port.TryKeepMine(request)`, dentro de `ConfirmChoice`; la petición de «Conservar» lleva el testigo de la presentación (`kind == ChoiceKind.KeepMine ? keepWitness : null`); la UI no compone texto (`KeepMinePresentationText` solo une `_keepLines` con `\n`). Con autoprueba | `fvm TryKeepMine=1 en=ConfirmChoice`; `fvm peticion-con-testigo=True`; `fvm presentacion-sin-componer=True`; `autoprueba-segundo-TryKeepMine=falla`; `autoprueba-texto-compuesto=falla` | `PushBackFullPanelGuardTests` | true |
| **16** | **La regla sin cableado** (D-PBT4-06): WHC tiene un solo adaptador estático, `PushBackFullFieldAdapter`, que llama a `PushBackAnnotationScaleRule.Validate`; WCo no asigna ninguna regla y no nombra `RackCad.UI` | `whc-adaptador PushBackFullFieldAdapter estatico=1 instancia=0`; `adaptador-regla PushBackAnnotationScaleRule.Validate=1`; `wco-sin-regla asignaciones=0`; `wco-sin-ui RackCad.UI=0` | `PushBackFullPanelGuardTests` | true |
| **17** | **Tras un «Actualizar» `Applied` que borra la vista de origen** (`PilotEraseScopeDouble`, POST reanclado): el borrador queda en Clean, su `CurrentDraftState.Origin` es el texto de la primera vista redibujada, y la relectura siguiente pasa a `ReadIntentAtOrigin` | `borrador estado=Clean origen-es-la-primera-redibujada=True`; `relectura lector=ReadIntentAtOrigin origen-del-borrador=True` | `PushBackFullPanelTests` | true |
| **18** | **La regla de Application y la de la UI dan lo mismo** (D-PBT4-06): `NumericFieldValidation.Validate` y `NumericTextRule.Check`, en una tabla fija de once casos (vacío requerido, vacío opcional, `abc`, `1.5` entero, `3` entero, `0` con mínimo 0 exclusivo, `0` con mínimo 0 inclusivo, `10` con máximo 10 exclusivo, `10` con máximo 10 inclusivo, `2,5` y `-1` con mínimo 0) | `casos=11`; y, por caso, `caso-<n> estado-igual=True valor-igual=True mensaje-igual=True` (n de 1 a 11) | `PushBackFullPanelFieldTests` (UI) | true |
| **19** | **I-24:** G-L1 y la pureza sobre los tres archivos nuevos de `Draft/`: `PushBackFullFieldAdapter.cs`, `PushBackKeepMinePresentation.cs` y `PushBackAnnotationScaleRule.cs` (§5.0). **Cubre `Apply`** | **33:** `g-l1 archivos=3 reglas-sin-coincidencias=True`; las 15 `autoprueba-<a, b, c, e, f1, f2, f3, g1, g2, h1, h2, i, j, k1, k2>=falla`; `pureza archivos=3 violaciones=[]`; `autoprueba-#nullable=falla`; `autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla`; las 13 `autoprueba-<término>=falla` de `ForbiddenTerms`; y `cubre Apply=PushBackFullFieldAdapter.cs` | `PushBackPanelDraftGuardTests` | **false** |
| **20** | **El guion de «Saltar y duplicar»** (D-PBT4-11), en ejecución. **Cada texto se normaliza a `\n` antes de comparar o de hashear** (el repositorio trabaja con `core.autocrlf=true`; I-01). (1) Cada uno de los 21 archivos de origen es su base salvo las líneas de atributo con su `Skip` literal: el texto de su base tiene el sha256 `BaseSha` del contrato (§6.2.1), y el archivo, con cada línea de `Skip` devuelta a `[Fact]` o `[Theory]`, es ese texto. (2) Cada prueba de la lista, salvo CT-PBT4A-06, tiene en su copia una prueba de igual nombre (las cuatro de D-S5, dos: una en `FullPanelCopy<Clase>RedTests` y otra en `FullPanelCopy<Clase>GreenTests`). (3) En las 9 clases de C-0, cada copia mecánica (las 356 del mapa de la copia v5) es **el texto de su método en la base** (de su primera línea de atributo a su llave de cierre, sin el `Skip`) **con M-00 a M-08**; M-00, con sus tipos literales en la CT, cada uno a `<tipo>Full`: `CommandAdvisorPortDouble` (`WorkspaceCommandAdvisorTests`), `LifecycleScriptedAdvisor` (`WorkspaceLifecycleHostTests`), y `PilotEditThrowingComparer`, `PilotEditThrowingProbe`, `PilotEditAuditedReader` y `PilotEditOutcomeProbe` (`WorkspacePilotEditHostTests`); las otras seis, ninguno (medido, `v45/selftest_pbt4b_v45-out.txt`). (4) Cada archivo de copia y el kit tienen el sha256 del contrato (`CopySha` y el del kit, del texto con los saltos de línea a `\n`), que fija sus equivalentes a mano, las ediciones de ayudante de C-0 (`Env.OpenUndecidable` en `WorkspacePilotEditHostFull`, V46-02) y los dos miembros nuevos de D-S5 (`StatusNamesRed` y `StatusNamesGreen` en `WorkspacePilotPrepareFull`). (5) Ninguna copia nombra `EditIntentFakeReader`, `EditIntentKit`, `PrepareKit`, `PushBackPilotDraftState`, `PushBackPilotStateComparer`, `PushBackPilotFieldAdapter` ni `PushBackFullReadRig` como palabra completa. (6) **M-05:** `FullHandKit.FullOfMatchesTheReader` da `True` en cinco escaneos: `FullEditIntentKit.TwoViews(FullEditIntentKit.RackA)`, `FullPrepareKit.RealScan()`, el de `pb-20-ab` y el de AUTH-13 de CT-PBT4A-02 (FRP:126-135) y el de `FullEditIntentKit.OriginMissing` (`IntentKit.Scan(FullEditIntentKit.RackA, "ZZ", …)`). (7) **D-S5** (V47-03): cada una de las cuatro `Theory` partidas por filas (WLHT `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively`, PEG `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` y las dos de WPP, `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` y `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading`) está en `FullPanelCopy<Clase>RedTests` y en `FullPanelCopy<Clase>GreenTests`, y sus dos mitades son iguales salvo sus filas y su línea `SPLIT_COMMENT`. En las de `InlineData`, las filas de las dos, juntas, son las del original con su M o su edición de guarda, una a una y sin repetirse. En las de `MemberData`, `StatusNamesRed` y `StatusNamesGreen` dan, juntas y sin repetirse, las filas de `StatusNames`, por reflexión en ejecución. Ninguna otra prueba de la lista está en las dos clases | Por archivo de origen, `original <Clase> igual-a-la-base=True skip=<n>` (21 líneas; los n de §6.2.1); por copia de C-0, `copia <Clase> pruebas=<n> mecanicas=<c> a-mano=<h> igual-al-mapa=True sha256=True` (9, con los números del mapa de la copia v5; `pruebas` cuenta los nombres, y una prueba partida por filas, uno); por copia de guarda o de UI, `copia <Clase> pruebas=<n> sha256=True` (12); `kit sha256=True`; por cada `Theory` partida por filas, `particion-por-filas <Clase>.<método> red=<n> green=<m> filas-del-original=True` (4 líneas: `WorkspaceLifecycleHost` 3 y 7, `WorkspacePilotEditGuard` 1 y 3, y las dos de `WorkspacePilotPrepare`, 7 y 1); `kit FullOf-igual-al-lector=True escaneos=5`; `sin-nombres-del-kit-del-piloto=True`; y `autoprueba-linea-de-mas=falla`, `autoprueba-asercion-cambiada=falla`, `autoprueba-skip-de-mas=falla`, `autoprueba-sin-copia=falla` | `WorkspaceFullCopyGuardTests` | **false** |
| **21** | **Una sola materialización del modelo completo por lectura** (D-PBT4-21, D-S4), en texto, sobre RIA, NH y `Draft/PushBackOriginByText.cs` leídos con `WorkspaceHostSource.Code` (sin comentarios), con autoprueba sobre texto inyectado en memoria: la línea del éxito de `Read` difiere la lectura completa; `RackPilotIntentReadResult` la guarda en `_full` (el valor ya leído, o la lectura diferida) y `Full` es `_full?.Value`; `AtOrigin` no pide `read.Full`; NH la pide una vez, dentro del bloqueo y tras `AtOrigin`; `ReadAt` lee el modelo completo una vez | `ria-Read completo=1 diferido=1`; `ria-resultado Full-diferido=1 campo=1 de-un-valor=1 de-una-lectura-diferida=1`; `ria-AtOrigin Full-del-Primary=0`; `nh-Full=1 dentro-del-bloqueo=True tras-AtOrigin=True`; `readat completo=1`; `autoprueba-completo-inmediato=falla`; `autoprueba-Full-del-Primary-en-AtOrigin=falla`; `autoprueba-segundo-Full-en-NH=falla`; `autoprueba-Full-sin-diferir=falla`; `autoprueba-segundo-completo-en-ReadAt=falla` | `PushBackFullPanelGuardTests` | true |

**Las expresiones de CT-PBT4B-21,** literales (`check_ria_v45.py`, `lines21` y `run21`, que las corre sobre el RED y el GREEN de la v4.5):
- los cuerpos: `Read` y `AtOrigin`, por `internal\s+static\s+RackPilotIntentReadResult\s+(Read|AtOrigin)\s*\(`; la clase, por `internal\s+sealed\s+class\s+RackPilotIntentReadResult\b`; `ReadOnActiveDocument` de NH y su `using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)`; `ReadAt`, por `public\s+static\s+\([^)]*\)\?\s+ReadAt\s*\(`; cada uno de su firma a la llave que cierra la primera que abre (como `BodyOrEmpty` de CT-PBT4A-07);
- `completo`: `PushBackFullDraftReader\.Read\s*\(`; `diferido`: `new\s+System\.Lazy<PushBackFullIntentResult>\s*\(\s*\(\s*\)\s*=>\s*PushBackFullDraftReader\.Read\s*\(\s*input\s*,\s*catalog\s*\)\s*\)`;
- `Full-diferido`: `internal\s+PushBackFullIntentResult\?\s+Full\s*=>\s*_full\s*\?\.\s*Value\s*;`; `campo`: `private\s+readonly\s+System\.Lazy<PushBackFullIntentResult>\?\s+_full\s*;`; `de-un-valor`: `_full\s*=\s*new\s+System\.Lazy<PushBackFullIntentResult>\s*\(\s*full\s*\)\s*;`; `de-una-lectura-diferida`: `_full\s*=\s*full\s*;`;
- `Full-del-Primary` y `nh-Full`: `\bread\s*\.\s*Full\b`; `tras-AtOrigin`: su primera coincidencia en `ReadOnActiveDocument` va después de `RackPilotIntentAdapter\.AtOrigin\s*\(`;
- las autopruebas: `diferido` sustituido por `PushBackFullDraftReader.Read(input, catalog)`; ` var injected = read.Full;` tras la `{` de `AtOrigin`; `\nvar injected = read.Full;\n` al final de NH; `Full-diferido` sustituido por `internal PushBackFullIntentResult? Full { get; }`; y ` var injected = PushBackFullDraftReader.Read(input, catalog);` tras `var input = AtKey(scan, key);` de `ReadAt`.

**Las clases:**
- Core, `ExpectRed` true: **`PushBackFullPanelTests`** (CT-01, 02, 03, 05, 06, 07, 08, 09, 10, 11 y 17: 11) y **`PushBackFullPanelGuardTests`** (CT-12, 15, 16 y 21: 4);
- Core, `ExpectRed` false: **`PushBackFullPanelCensusGuardTests`** (CT-13), **`PushBackPanelDraftGuardTests`** (CT-19) y **`WorkspaceFullCopyGuardTests`** (CT-20);
- UI, `ExpectRed` true: **`PushBackFullPanelFieldTests`** (CT-04, 14 y 18: 3).
- **18 CT de Core y 3 de UI: 21.**

**Lo que fija cada `ExpectRed`:**
- **En el RED,** el anfitrión sigue con el piloto (WHC sin cambio), RIA sigue leyendo el modelo completo en el acto, FVM tiene sus dos propiedades inertes y los cuatro archivos nuevos de `src/` son inertes (§6.2, «El RED»). Fallan por aserción las CT de las tres clases con `true`: un borrador del piloto, no del completo (01 a 03, 05 a 11 y 17); WHC con los nombres del piloto (12 y 16); FVM sin el testigo (15); la lectura completa sin diferir (21: `ria-Read completo=1 diferido=0`, medido); la regla y la presentación inertes (04, 06, 14 y 18).
- **Pasan en el RED:** CT-13 (WHC todavía no nombra los cinco tipos, y el censo lo admite), CT-19 (los tres archivos inertes son puros) y CT-20 (las copias, el kit y los `Skip` van en el commit RED; su base es la de `BaseSha`, sin los `Skip`).
- **En CT-06, las tres líneas «presentar»** salen del clasificador real (PB-T3a, cerrado). Se dedujeron de su texto: el nombre está solo en el sobre (`$.Name`, el grupo «el nombre»), y la escala del usuario solo en `$.Structure.AnnotationScale` («las anotaciones»); con el escaneo realista, las tres vistas se redibujan y no hay línea de vistas. **Se confirman en la precondición 6 con su sonda** (`pbt4b-ct06-presentar-probe.cs`, una clase temporal que corre el caso del anfitrión de CT-06 sobre la copia con el GREEN; §11), y lo esperado es: `presentar estado=Presented testigo-clave=A1 testigo-texto-igual-al-origen=True borrador-igual=True origen-igual=True lecturas=1 lineas=2`, `presentar Cambiado fuera: el nombre.` y `presentar Cambiado por ti: las anotaciones.`; si no son esas, el paquete se corrige antes de emitir.

**Los totales de PB-T4b** (F4B-E46), con la base del cierre de PB-T4a (F4B-E51, `78a7cef3`; la CI de su GREEN, `44814418683` sobre `faef7020`: Core **19 137 = 19 130 + 0 + 7** y UI **2 451 = 2 430 + 21**):
- **cada prueba de la lista con copia o equivalente suma exactamente un resultado:** su original pasa a uno solo, omitido (una `Theory` con `Skip` es un resultado), y su sustituta repite sus filas (una `Theory` copiada tiene los mismos datos; las cuatro partidas por filas, D-S5, las reparten entre sus dos mitades sin cambiar ninguna). Por eso **el total y el número de omitidas no dependen de las filas de las `Theory` de `MemberData`**;
- **CT-PBT4A-06** pasa a omitida, y su sustituta es una CT (CT-13);
- **Core:** 451 de la lista (428 de C-0, 15 de C-5 de la v4.3, las 5 de la tercera tanda, las 2 de PB-T4a y la de la sexta tanda), 450 con copia o equivalente, y 18 CT. **UI:** 27 de la lista, con copia, y 3 CT.

| Rama | Core | UI |
|---|---|---|
| **GREEN** | **19 605 = 19 147 `Passed` + 0 `Failed` + 458 `Skipped`** | **2 481 = 2 433 `Passed` + 0 `Failed` + 48 `Skipped`** |
| **RED** | **19 605 = (19 147 − F) `Passed` + F `Failed` + 458 `Skipped`,** con **F = 15 + R_C** | **2 481 = (2 433 − F_UI) `Passed` + F_UI `Failed` + 48 `Skipped`,** con **F_UI = 3 + R_UI** |
| **RED sin las CT** (la suite completa de la precondición 6) | **19 587 = (19 129 − R_C) `Passed` + R_C `Failed` + 458 `Skipped`** | **2 478 = (2 430 − R_UI) `Passed` + R_UI `Failed` + 48 `Skipped`** |
| **GREEN sin las CT** (la precondición 6) | **19 587 = 19 129 `Passed` + 0 `Failed` + 458 `Skipped`** | **2 478 = 2 430 `Passed` + 0 `Failed` + 48 `Skipped`** |

- 19 605 = 19 137 + 450 + 18; 19 147 = 19 130 − 1 + 18; 458 = 7 + 451. 2 481 = 2 451 + 27 + 3; 2 433 = 2 430 + 3; 48 = 21 + 27.
- Sin las CT: 19 587 = 19 137 + 450; 19 129 = 19 130 − 1; 2 478 = 2 451 + 27. **La de la sexta tanda sube el RED sin las CT de 19 586 (v4.7) a 19 587**, como las dos de PB-T4a lo subieron de 19 584 (v4.4) a 19 586 (cada una suma un resultado); la UI no cambia.
- **15** son las CT de Core con `ExpectRed` true (11 + 4), y **3** las de UI.
- **R_C y R_UI** son los resultados (las filas) de las pruebas de las clases `FullPanelCopy…RedTests` de Core y de UI: **`<medido en la precondicion 6>`**, con la partición (`partition_pbt4b_v48.py red`, que da el número de resultados de cada clase, y R_C y R_UI). **La regla:** una prueba cuyos resultados del RED son todos `Failed` por aserción va a `FullPanelCopy…RedTests`; todos `Passed`, a `FullPanelCopy…GreenTests`; **una `Theory` que mezcla se parte por filas en dos equivalentes escritos a mano** (D-S5; V47-02): uno en `FullPanelCopy…RedTests` con las filas que fallan y otro en `FullPanelCopy…GreenTests` con las que pasan, con el mismo cuerpo y las mismas aserciones; la partición da para ella dos filas, Red y Green, y lo admite solo para las declaradas en el paquete (las cuatro de D-S5, con sus filas Red: `SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v48.py`, los de la v4.7). **Para la emisión** una prueba que mezcla sin estar declarada (se le aplica D-S5 con una versión del paquete), una declarada que no mezcla o mezcla otras filas, y una que se salta o falla sin aserción (C04). **Por aserción** es la regla de INV-PBT4B-02 (I-02): **la primera línea del StackTrace del resultado está bajo `tests/`**; entra así un `NullReferenceException` en el código de la prueba (con el anfitrión del piloto, `Find<PushBackFullDraftState>` da null), y para una cuya primera línea está en `src/` o no tiene ruta.
- **D = 0** (I-25): ninguna otra prueba falla. **En el RED,** lo mide la precondición 6 con la suite completa de Core y los filtros de UI sobre la copia con el RED exacto, sin las CT: 19 587 y 2 478, con R_C y R_UI `Failed`, que tienen que ser los de la partición. **En el GREEN,** lo sostienen el recorrido de §4.2 y la revisión (§17 y §18), y **lo mide la precondición 6** con `apply_green_pbt4b_v45.py`: la suite completa de Core y los filtros de UI sobre la copia con el GREEN literal, con 0 `Failed` y ningún `warning CS`. **Una copia que no pase en el GREEN pasa a equivalente escrito a mano** (D-S1), con su texto literal, y la emisión necesita una versión del paquete con él; **una prueba que falle fuera de la lista de 478 vuelve al Owner.**

### 5.3 PB-T4c: la lista cerrada CT-PBT4C-01 a 15 (entera; V50-05 a V50-07)

**Desde la v5.0, entera y literal,** como §5.2 para PB-T4b: las CT-01 a 13 de la v3 (las 01 a 11 de la v2 con los cambios de la v3), la CT-14 de la v4 y la CT-15 de la v4.1, con lo que fijó la v4.9 y las líneas medidas sobre el GREEN literal. **De dónde sale cada línea** (V51-02; CA-T4c M-01):
- **las de CT-03, 04, 05 y 12,** de la sonda de Core sobre el GREEN literal (`pbt4c-probe-editall.cs`, una clase temporal sobre la copia con el GREEN), como dice, línea a línea, la tabla de después de la de las CT. La corrida válida de la v5.0 es `/d/r5f` (`pbt4ccc/compile/out-probes-v50b-r5f/probe-core-lines.txt`) y la de la v5.1, `/d/r5i` (`pbt4ccc/compile/r5i-en/sonda-core-lineas.txt`): iguales, sha256 `cbbd8457149afd6919dbfb763b63c47a60283bc44d01f05c0b92aae1652ea59f`;
- **las de CT-01, 07, 08, 13 y 15,** del ensayo de las CT de texto (§6.3.1, el paso 8), que las da tal cual sobre el GREEN (y, sobre el RED, las de CT-07 y CT-15, mientras CT-01, 08 y 13 caen por su `Assert.Equal`);
- **las de CT-09,** de la sonda de UI (`pbt4c-probe-close-ui.cs`; D-S10);
- **las de CT-02, 06, 10, 11 y 14,** del texto literal del GREEN (anexo V51-A), sin ejecutar.

**Las reglas de §5.0,** enteras: un `[Fact]` por CT, que acaba en un `Assert.Equal` de las líneas unidas por `\n`; ninguna `[Theory]` nueva; la regla «EXCEPCION»; las de UI; los ayudantes existentes sin editar. **Las CT de Core van en `tests/RackCad.Tests/`, con `namespace RackCad.Tests`; las de UI, en `tests/RackCad.UI.Tests/`, con `namespace RackCad.UI.Tests`.** Ninguna clase nueva empieza por `Workspace` ni por un prefijo de un filtro heredado (D-S8, §6.3.1).

**Los ayudantes de las CT:** `FullReadRig`, `FullEditIntentFakeReader`, `FullEditIntentKit` y `FullPrepareKit` (`WorkspaceFullReadKit.cs`), `IntentKit`, `PilotKit`, `DraftKit`, `WorkspaceHostRig`, `FullWriteKit` y `PilotEraseScopeDouble` (Core), y en UI la FVM real, sin editarlos. **Los dobles nuevos** (el de `IWorkspaceFullEditHost`, que pone el modal con `SetModalRackCadActive` y lee el lanzamiento; y el del puerto de la UI, que registra las llamadas y responde lo que la CT le da) **van dentro de su clase**, como clases anidadas privadas. Lo que una CT copia de otra clase (por ejemplo, `Rig`, `ThreeViews` y `LessOneFront` de `PushBackFullPanelTests` para CT-10) se copia, sin referenciarlo.

| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |
|---|---|---|---|---|
| **01** | **El botón, en texto** (D-PBT4-12): en `WorkspacePanelView.xaml`, `BrowserFieldEditAllButton` una vez, en el `WrapPanel` de los botones del campo y justo tras `BrowserFieldEditButton`, con sus cuatro atributos literales; en `WorkspacePanelView.xaml.cs`, `OnBrowserFieldEditAllClick` una vez, con su cuerpo literal | `xaml BrowserFieldEditAllButton=1 en-el-panel-de-los-botones=True tras=BrowserFieldEditButton`; `xaml Content=[Editar todo] AutomationProperties.Name=[Editar el rack completo en el editor]`; `xaml Visibility=[{Binding Browser.Field.ShowEditAllButton, Converter={StaticResource FlagToVisibility}}] Click=[OnBrowserFieldEditAllClick]`; `code-behind OnBrowserFieldEditAllClick=1 cuerpo=[Contained(() => _viewModel.Browser.Field.EditAll())]` | `PushBackEditAllGuardTests` | true |
| **02** | **El gesto y sus avisos** (D-PBT4-12; R-PB-10), con la FVM real sobre un doble nuevo del puerto, en la clase, que registra y responde `Closed` sin aplicar: con texto pendiente, `CommitField` `Invalid` (mensaje `Valor no numérico.`), `Busy` (motivo `Hay una lectura del piloto en curso.`) o `Rejected` (motivo `La petición no es del documento activo.`) no llaman a `TryEditAll`; `Applied` y `NotEditable`, sí | `invalido TryEditAll=0 mensaje=[Valor no numérico.] aviso=[No se abrió el editor: Valor no numérico.]`; `ocupado TryEditAll=0 aviso=[El texto no se aplicó: Hay una lectura del piloto en curso.]`; `rechazado TryEditAll=0 aviso=[El texto no se aplicó: La petición no es del documento activo.]`; `valido TryEditAll=1 aviso=[Editor cerrado sin cambios en el borrador.]`; `no-editable TryEditAll=1 aviso=[Editor cerrado sin cambios en el borrador.]` | `PushBackEditAllWindowTests` (UI) | true |
| **03** | **`TryEditAll` del anfitrión** (D-PBT4-13), por el puerto (`IWorkspacePilotEditPort`) sobre `FullReadRig` y el lector del kit, con un doble de `IWorkspaceFullEditHost` que registra y no aplica: sin anfitrión; sin borrador (lo empieza, con su lectura); la cola aparcada (`pb-21-cola`); Clean, Dirty y Conflict; Applying; Unknown y Error; texto pendiente; y otra sesión | `sin-anfitrion estado=NotAvailable motivo=[No editable aquí: el panel no tiene el editor completo.] abierto=False lecturas=0 borrador=False`; `sin-borrador estado=Closed abierto=True lecturas=1 borrador=Clean`; `cola estado=NotBegun inicio=NotAdmitted diagnostico-fuera-del-piloto=True pista=True abierto=False borrador=False`; `clean estado=Closed abierto=True lecturas=0`; `dirty estado=Closed abierto=True lecturas=0`; `conflict estado=Closed abierto=True lecturas=0`; `applying estado=Waiting motivo=[El borrador se está aplicando o se retiró: el editor no se abre.] abierto=False`; `unknown estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False`; `error estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False`; `texto-pendiente estado=Refused motivo=[Hay texto sin confirmar en el campo.] abierto=False`; `otra-sesion estado=Rejected motivo=[La petición no es del documento activo.] abierto=False lecturas=0` | `PushBackEditAllTests` | true |
| **04** | **El lanzamiento y el adaptador del puerto** (D-PBT4-13): dentro de `ShowModal`, el doble lee el lanzamiento (el estado del borrador, el rack y el proyecto del sobre del origen) y aplica dos veces por su puerto (escala `2.5` y `3`); el origen se conserva; tras cerrar, `Closed` con las dos y el borrador Dirty. Y el adaptador solo, sobre un borrador Applying y sobre uno Clean | `lanzamiento estado-igual=True rack=True proyecto=True proyecto-con-diseno=True`; `aplicar-1 estado=Applied isdirty=True origen-igual=True escala=2.5`; `aplicar-2 estado=Applied isdirty=True escala=3`; `tras-aplicar estado=Closed aplicadas=2 borrador=Dirty origen-igual=True`; `adaptador-no-editable estado=NotEditable llamadas=0`; `adaptador-aplica estado=Applied llamadas=1` | `PushBackEditAllTests` | true |
| **05** | **K = 1** (D-PBT4-14; R-PB-10): dentro de `ShowModal` (el doble pone el modal con `SetModalRackCadActive`), cada operación del puerto sobre el borrador abierto, con su respuesta y su motivo; ninguna lectura del dibujo dentro; y, tras cerrar, el panel vuelve a responder | `k1 SetPendingText Busy [Hay una lectura del piloto en curso.]`; `k1 CommitField Busy [Hay una lectura del piloto en curso.]`; `k1 CrossBoundary Busy [Hay una lectura del piloto en curso.]`; `k1 DiscardRack Busy [Hay una lectura del piloto en curso.]`; `k1 DiscardDocument Busy [Hay una lectura del piloto en curso.]`; `k1 TryBeginEdit Waiting [Hay una lectura del piloto en curso.]`; `k1 TryCheckNow Waiting [Hay una lectura del piloto en curso.]`; `k1 TryReload Waiting [Hay una lectura del piloto en curso.]`; `k1 TryConfirmBase Waiting [Hay una lectura del piloto en curso.]`; `k1 TryKeepMine Waiting [Hay una lectura del piloto en curso.]`; `k1 TryDiscardAndReload Waiting [Hay una lectura del piloto en curso.]`; `k1 TryEditAll Waiting [Hay una lectura del piloto en curso.]`; `k1 RequestUpdate Busy [Hay una lectura del piloto en curso.]`; `k1 GetField existe=True estado=Clean`; `k1 GetRacksAtRisk conocido=True`; `k1 TakeLossNotices 0 TakeCloseNotices 0`; `k1 TakeUpdateProgress en-vuelo=False`; `k1-cerrado estado=Closed lecturas-dentro=0`; `despues SetPendingText Applied TryBeginEdit AlreadyOpen` | `PushBackEditAllTests` | true |
| **06** | **El resumen y «Modificado» al cerrar** (A2-9), con la FVM real sobre el doble del puerto de CT-02: con un aplicar, el doble pasa su campo a Dirty y responde `Closed` con 1; sin aplicar, `Closed` con 0; el navegador recibe su aviso de cambio una vez | `con-aplicar estado=[Modificado] aviso=[Editor cerrado: sus cambios están en el borrador.] cambiado=1`; `sin-aplicar estado=[Limpio] aviso=[Editor cerrado sin cambios en el borrador.] cambiado=1` | `PushBackEditAllWindowTests` (UI) | true |
| **07** | **La guarda de OB-01.3** (D-PBT4-16), en texto sin comentarios, sobre los seis sujetos: `RackWorkspaceEditorWindow.cs`, el cuerpo de `FullEditorModal` en WCo, los de `TryEditAll` y `EditAllOfPilot` en WHC, el de `EditAll` en FVM, `Draft/PushBackFullDraftLaunch.cs` y `Draft/PushBackFullDraftStatePort.cs`. Ninguno nombra los ocho; la autoprueba inyecta cada nombre en cada sujeto. Un sujeto que falta (en el RED) es su texto vacío | `sujeto RackWorkspaceEditorWindow.cs prohibidos=[]`; `sujeto FullEditorModal prohibidos=[]`; `sujeto TryEditAll prohibidos=[]`; `sujeto EditAll prohibidos=[]`; `sujeto PushBackFullDraftLaunch.cs prohibidos=[]`; `sujeto PushBackFullDraftStatePort.cs prohibidos=[]`; `autoprueba-RackPushBackCommands-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-RackMenuCommands-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-EditorModules-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-RackViewBatchExecution-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-DrawAndPlace-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-RedrawInPlace-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-InsertRequested-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-InsertionRequest-en-RackWorkspaceEditorWindow.cs=falla`; `autoprueba-RackPushBackCommands-en-FullEditorModal=falla`; `autoprueba-RackMenuCommands-en-FullEditorModal=falla`; `autoprueba-EditorModules-en-FullEditorModal=falla`; `autoprueba-RackViewBatchExecution-en-FullEditorModal=falla`; `autoprueba-DrawAndPlace-en-FullEditorModal=falla`; `autoprueba-RedrawInPlace-en-FullEditorModal=falla`; `autoprueba-InsertRequested-en-FullEditorModal=falla`; `autoprueba-InsertionRequest-en-FullEditorModal=falla`; `autoprueba-RackPushBackCommands-en-TryEditAll=falla`; `autoprueba-RackMenuCommands-en-TryEditAll=falla`; `autoprueba-EditorModules-en-TryEditAll=falla`; `autoprueba-RackViewBatchExecution-en-TryEditAll=falla`; `autoprueba-DrawAndPlace-en-TryEditAll=falla`; `autoprueba-RedrawInPlace-en-TryEditAll=falla`; `autoprueba-InsertRequested-en-TryEditAll=falla`; `autoprueba-InsertionRequest-en-TryEditAll=falla`; `autoprueba-RackPushBackCommands-en-EditAll=falla`; `autoprueba-RackMenuCommands-en-EditAll=falla`; `autoprueba-EditorModules-en-EditAll=falla`; `autoprueba-RackViewBatchExecution-en-EditAll=falla`; `autoprueba-DrawAndPlace-en-EditAll=falla`; `autoprueba-RedrawInPlace-en-EditAll=falla`; `autoprueba-InsertRequested-en-EditAll=falla`; `autoprueba-InsertionRequest-en-EditAll=falla`; `autoprueba-RackPushBackCommands-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-RackMenuCommands-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-EditorModules-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-RackViewBatchExecution-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-DrawAndPlace-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-RedrawInPlace-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-InsertRequested-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-InsertionRequest-en-PushBackFullDraftLaunch.cs=falla`; `autoprueba-RackPushBackCommands-en-PushBackFullDraftStatePort.cs=falla`; `autoprueba-RackMenuCommands-en-PushBackFullDraftStatePort.cs=falla`; `autoprueba-EditorModules-en-PushBackFullDraftStatePort.cs=falla`; `autoprueba-RackViewBatchExecution-en-PushBackFullDraftStatePort.cs=falla`; `autoprueba-DrawAndPlace-en-PushBackFullDraftStatePort.cs=falla`; `autoprueba-RedrawInPlace-en-PushBackFullDraftStatePort.cs=falla`; `autoprueba-InsertRequested-en-PushBackFullDraftStatePort.cs=falla`; `autoprueba-InsertionRequest-en-PushBackFullDraftStatePort.cs=falla` | `PushBackOpenerClassicPathGuardTests` | **false** |
| **08** | **Las dos piezas del abridor, en texto** (D-PBT4-15; M-30): `FullEditorModal` en WCo, que implementa `IWorkspaceFullEditHost`, con un `using (_controller.EnterPanelModal())` y la llamada a `_open`, sin `RackCad.UI` ni `PushBack`; `RackWorkspaceEditorWindow.Show`, con la conversión y la llamada literales, una vez cada una, sin `Owner =` ni `RackCad.Application.Workspace`; y el cableado en WCo, una vez, tras `SetPilotUpdateHost` | `modal FullEditorModal=1 implementa=IWorkspaceFullEditHost using-EnterPanelModal=1 _open=1 RackCad.UI=0 PushBack=0`; `abridor conversion=[var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);] veces=1`; `abridor mostrar=[AcApplication.ShowModalWindow(window);] veces=1 Owner-asignado=0 RackCad.Application.Workspace=0`; `cableado [_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));] veces=1 tras-SetPilotUpdateHost=True` | `PushBackEditAllGuardTests` | true |
| **09** | **«Cerrar» con las dos cosas** (D-PBT4-17; A-T2b-OWNER-01; D-S7 y D-S10), con la ventana real: `PendingWork()` sin cerrar la ventana. En WORKSPACE (`pb-31-v2-modulos`), la cabecera escenificada con `SelectModule(w, "M3")`, `HeaderConfiguratorDialog` (una copia con `PanelClear` 40) y `ConfigureModuleHeaderButton`, sin construir el configurador, que deja también cambios sin aplicar (D-S10); en WORKSPACE (`pb-01-base`), solo cambios sin aplicar (`ToleranceBox` a 4.5) y ninguno; en CLASSIC (`pb-31-v2-modulos`), la misma cabecera | `ambas pregunta=[Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?]`; `solo-sin-aplicar pregunta=[Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?]`; `ninguna pendiente=False`; `clasico-modulo pregunta=[Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?]` | `PushBackEditAllWindowTests` (UI) | true |
| **10** | **De punta a punta sin AutoCAD** (A2-11; F4B-E30): el puerto real (`WorkspacePilotEditGate` sobre WHC) → `TryEditAll` → el doble de `IWorkspaceFullEditHost` aplica por el puerto del lanzamiento un gesto compuesto (un frente menos y la escala), una vez → «Actualizar» con `PilotEraseScopeDouble` (como CT-PBT4B-05) → `Applied` y Clean | `editar-todo estado=Closed aplicadas=1 borrador=Dirty`; `lo-aplicado-es-el-editado=True`; `actualizar estado=Applied begin-mutation=1 seal=1 borrador=Clean`; `borrado EraseUnit:B2` | `PushBackEditAllTests` | true |
| **11** | **D2 en `ApplyToDraft`** (D-PBT4-18): la huella de las dos referencias compartidas antes y después de cada aplicar, y la condición 7 por el puerto (la línea base resuelta del estado aplicado, sustituida por la de otro fixture) | `ApplyToDraft-1 estado=Applied huellas-iguales=True`; `ApplyToDraft-2 estado=Applied huellas-iguales=True`; `sustituir-referencia estado=Applied isdirty=False` | `PushBackEditAllTests` | true |
| **12** | **La compuerta reenvía `TryEditAll`** (B-05): `new WorkspacePilotEditGate(core, depth)`, por el puerto: dentro de un aviso, `Waiting` con `InsideNoticeReason` y sin abrir; fuera, llega al anfitrión y abre | `compuerta-dentro estado=Waiting motivo=[Se está entregando un aviso del panel.] abierto=False lecturas=0`; `compuerta-fuera estado=Closed abierto=True lecturas=1` | `PushBackEditAllTests` | true |
| **13** | **El adaptador del Plugin reenvía `TryEditAll`** (B-05): `ControllerViolations` (PEG:659-707) copiada con las 18 operaciones (la tabla del equivalente de PEG:155-163), con dos autopruebas: un adaptador sin el reenvío y uno que no solo delega | `adaptador operaciones=18 violaciones=[]`; `autoprueba-sin-reenvio-de-TryEditAll=falla`; `autoprueba-reenvio-que-no-solo-delega=falla` | `PushBackEditAllGuardTests` | true |
| **14** | **La FVM llama al puerto en su orden** (I-20), sobre el doble de CT-02: con texto pendiente, `CommitField` y después `TryEditAll`; sin él, solo `TryEditAll`; la petición de D-PBT4-12; y cuándo se ofrece el botón | `con-texto llamadas=[GetField,CommitField,TryEditAll,GetField] cambiado=1`; `sin-texto llamadas=[GetField,TryEditAll,GetField] cambiado=1`; `peticion sesion-del-campo=True rack-del-campo=True referencia=null testigo=null`; `boton sin-borrador=True editable=True no-editable=False vista-previa=False` | `PushBackEditAllWindowTests` (UI) | true |
| **15** | **I-24:** G-L1 y la pureza sobre los dos archivos nuevos de `Draft/` de PB-T4c: `PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs` (§5.0; como CT-PBT4B-19). **Cubre `ApplyToDraft`** | `g-l1 archivos=2 reglas-sin-coincidencias=True`; `autoprueba-a=falla`; `autoprueba-b=falla`; `autoprueba-c=falla`; `autoprueba-e=falla`; `autoprueba-f1=falla`; `autoprueba-f2=falla`; `autoprueba-f3=falla`; `autoprueba-g1=falla`; `autoprueba-g2=falla`; `autoprueba-h1=falla`; `autoprueba-h2=falla`; `autoprueba-i=falla`; `autoprueba-j=falla`; `autoprueba-k1=falla`; `autoprueba-k2=falla`; `pureza archivos=2 violaciones=[]`; `autoprueba-#nullable=falla`; `autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla`; `autoprueba-Autodesk=falla`; `autoprueba-RackCad.Plugin=falla`; `autoprueba-RackCad.UI=falla`; `autoprueba-System.Windows=falla`; `autoprueba-Transaction=falla`; `autoprueba-Database=falla`; `autoprueba-async=falla`; `autoprueba-await=falla`; `autoprueba-Task=falla`; `autoprueba-File.=falla`; `autoprueba-System.IO=falla`; `autoprueba-RackEmbedComposer=falla`; `autoprueba-new RackEmbedDocument=falla`; `cubre ApplyToDraft=PushBackFullDraftStatePort.cs` | `PushBackDraftLaunchGuardTests` | **false** |

**De dónde sale cada línea de CT-03, 04, 05 y 12** (V51-02; CA-T4c M-01): **22 tal cual** de la sonda, **9 con los campos de la CT** (la sonda imprime además campos que la CT no fija, con los mismos valores en los que sí fija), **3 derivadas** y **4 del texto** del GREEN, que la sonda no tiene.

| CT | La línea de §5.3 | De dónde sale |
|---|---|---|
| CT-03 | `sin-anfitrion estado=NotAvailable motivo=[No editable aquí: el panel no tiene el editor completo.] abierto=False lecturas=0 borrador=False` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas` |
| CT-03 | `sin-borrador estado=Closed abierto=True lecturas=1 borrador=Clean` | derivada: `borrador=Clean` es el `estado-del-borrador=Clean` de la sonda (su `borrador=True` dice que existe) |
| CT-03 | `cola estado=NotBegun inicio=NotAdmitted diagnostico-fuera-del-piloto=True pista=True abierto=False borrador=False` | del texto del GREEN: la sonda no la tiene (el `Begin` de la cola aparcada da `NotAdmitted`, como CT-PBT4B-02 en `PushBackFullPanelTests.cs:230`) |
| CT-03 | `clean estado=Closed abierto=True lecturas=0` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas`, `motivo` |
| CT-03 | `dirty estado=Closed abierto=True lecturas=0` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas`, `estado-del-borrador`, `motivo` |
| CT-03 | `conflict estado=Closed abierto=True lecturas=0` | del texto del GREEN: la sonda no la tiene. En Conflict el borrador admite cambios (`DraftSession.cs:331-332`), así que abre como Clean y Dirty |
| CT-03 | `applying estado=Waiting motivo=[El borrador se está aplicando o se retiró: el editor no se abre.] abierto=False` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas`, `lecturas` |
| CT-03 | `unknown estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas`, `estado-del-borrador`, `lecturas` |
| CT-03 | `error estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False` | del texto del GREEN: la sonda no la tiene. En Error el borrador no admite cambios (`DraftSession.cs:331-332`), así que se rechaza como Unknown |
| CT-03 | `texto-pendiente estado=Refused motivo=[Hay texto sin confirmar en el campo.] abierto=False` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas`, `lecturas` |
| CT-03 | `otra-sesion estado=Rejected motivo=[La petición no es del documento activo.] abierto=False lecturas=0` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas` |
| CT-04 | `lanzamiento estado-igual=True rack=True proyecto=True proyecto-con-diseno=True` | de la sonda, **tal cual** |
| CT-04 | `aplicar-1 estado=Applied isdirty=True origen-igual=True escala=2.5` | de la sonda, **tal cual** |
| CT-04 | `aplicar-2 estado=Applied isdirty=True escala=3` | de la sonda, **tal cual** |
| CT-04 | `tras-aplicar estado=Closed aplicadas=2 borrador=Dirty origen-igual=True` | derivada: `borrador=Dirty` es el `estado-del-borrador=Dirty` de la sonda |
| CT-04 | `adaptador-no-editable estado=NotEditable llamadas=0` | de la sonda, **tal cual** |
| CT-04 | `adaptador-aplica estado=Applied llamadas=1` | del texto del GREEN: la sonda no la tiene (el adaptador sobre un borrador que admite el cambio) |
| CT-05 | `k1 SetPendingText Busy [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 CommitField Busy [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 CrossBoundary Busy [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 DiscardRack Busy [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 DiscardDocument Busy [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 TryBeginEdit Waiting [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 TryCheckNow Waiting [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 TryReload Waiting [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 TryConfirmBase Waiting [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 TryKeepMine Waiting [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 TryDiscardAndReload Waiting [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 TryEditAll Waiting [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 RequestUpdate Busy [Hay una lectura del piloto en curso.]` | de la sonda, **tal cual** |
| CT-05 | `k1 GetField existe=True estado=Clean` | de la sonda, **tal cual** |
| CT-05 | `k1 GetRacksAtRisk conocido=True` | de la sonda, **tal cual** |
| CT-05 | `k1 TakeLossNotices 0 TakeCloseNotices 0` | de la sonda, **tal cual** |
| CT-05 | `k1 TakeUpdateProgress en-vuelo=False` | de la sonda, **tal cual** |
| CT-05 | `k1-cerrado estado=Closed lecturas-dentro=0` | derivada: `lecturas-dentro` son las lecturas del lector **durante `ShowModal`**. La sonda imprime `lecturas-dentro=1`, el total del lector tras `TryEditAll`, con la lectura de `Begin` de antes (`pbt4c-probe-editall.cs:198`); la de `Begin` es una sola (`sin-borrador … lecturas=1`), así que durante `ShowModal` son 1 − 1 = **0** |
| CT-05 | `despues SetPendingText Applied TryBeginEdit AlreadyOpen` | de la sonda, **tal cual** |
| CT-12 | `compuerta-dentro estado=Waiting motivo=[Se está entregando un aviso del panel.] abierto=False lecturas=0` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas` |
| CT-12 | `compuerta-fuera estado=Closed abierto=True lecturas=1` | de la sonda, **con los campos de la CT**: la sonda imprime además `aplicadas`, `motivo` |

**Las clases:**
- Core, `ExpectRed` true: **`PushBackEditAllTests`** (CT-03, 04, 05, 10, 11 y 12: 6) y **`PushBackEditAllGuardTests`** (CT-01, 08 y 13: 3);
- Core, `ExpectRed` false: **`PushBackOpenerClassicPathGuardTests`** (CT-07) y **`PushBackDraftLaunchGuardTests`** (CT-15);
- UI, `ExpectRed` true: **`PushBackEditAllWindowTests`** (CT-02, 06, 09 y 14: 4).
- **11 CT de Core y 4 de UI: 15.**

**Y los cuatro equivalentes de la lista de 4** (no son CT; su texto es el del guion, `EquivalentSha`, y no admiten reparación; §6.3.1):
- en Core, `ExpectRed` false: `PushBackEditAllPortEquivalenceGuardTests` (PEG:155-163, con 18 operaciones), `PushBackEditAllNameWritersGuardTests` (CT-PBT1B-15, con «archivos=15») y `PushBackEditAllFullCopyGuardTests` (CT-PBT4B-20, que además devuelve el `Skip` de PB-T4c de PEG:155);
- en UI, **`ExpectRed` true:** `PushBackEditAllApplyCloseTests` (CT-PBT2B-06, con la pregunta combinada en la ventana 4).

**Por qué esta vez un equivalente cae en el RED** (V50-04), cuando en PB-T4b la mayoría pasaban:
- **En PB-T4b,** las copias reproducían pruebas del panel con el kit completo. La mayoría fijaban un comportamiento que PB-T4b no cambia, y pasaban ya en el RED; solo caían las que fijaban lo que el GREEN cambia (las clases `FullPanelCopy…RedTests` de la partición).
- **En PB-T4c, tres de los cuatro equivalentes son de guardas de texto o de reflexión.** Sus condiciones las cumple ya el RED: el esqueleto declara `TryEditAll`, el abridor inerte nombra `PushBack` y los `Skip` van en el RED. Por eso pasan en las dos ramas.
- **El cuarto fija un comportamiento que solo trae el GREEN:** la pregunta combinada de «Cerrar» (D-PBT4-17). W no cambia en el esqueleto, porque si cambiara, CT-09 pasaría en el RED (C04). Así que el equivalente de CT-PBT2B-06 cae por aserción en el RED y pasa en el GREEN, como una clase Red de la partición de PB-T4b, y su filtro va con `ExpectRed` true.

**Lo que fija cada `ExpectRed`:**
- **En el RED,** el esqueleto exacto de §6.3.1: el puerto con `TryEditAll` y su cuerpo por defecto, WEI con `EditAllOutcome` y `EditAllStatus`, `IWorkspaceFullEditHost.cs` entero, WHC solo con `SetFullEditHost` sin efecto, FVM con `ShowEditAllButton` en `false` y `EditAll` sin efecto, los dos archivos nuevos de `Draft/` y el abridor, inertes; la compuerta, WCo, PV, `WorkspacePanelView.xaml.cs` y W, sin cambio. Fallan por aserción:
  - CT-03, 05, 10 y 12: el puerto responde `NotAvailable` sin motivo;
  - CT-04 y 11: el adaptador inerte responde `NotEditable`;
  - CT-01, 08 y 13: los textos no están;
  - CT-02, 06, 09 y 14: la FVM no llama al puerto y W pregunta lo de hoy;
  - y el equivalente de CT-PBT2B-06.
- **Pasan en el RED:**
  - CT-07: los sujetos inertes o ausentes no nombran nada;
  - CT-15: los dos archivos inertes son puros;
  - los tres equivalentes de Core.

**Los totales de PB-T4c** (F4B-E46; V50-07), **en cifras,** con la base del cierre de PB-T4b (F4B-E53, `6adff90d`; la CI de su GREEN, `44903818806` sobre `c065080d`: Core **19 605 = 19 147 + 0 + 458** y UI **2 481 = 2 433 + 0 + 48**):
- **cada una de las 4 de la lista suma exactamente un resultado:** su original pasa a uno solo, omitido, y su equivalente es un `[Fact]`;
- **Core:** 3 de la lista, 3 equivalentes y 11 CT. **UI:** 1 de la lista, 1 equivalente y 4 CT.

| Rama | Core | UI |
|---|---|---|
| **GREEN** | **19 619 = 19 158 `Passed` + 0 `Failed` + 461 `Skipped`** | **2 486 = 2 437 `Passed` + 0 `Failed` + 49 `Skipped`** |
| **RED** | **19 619 = 19 149 `Passed` + 9 `Failed` + 461 `Skipped`** | **2 486 = 2 432 `Passed` + 5 `Failed` + 49 `Skipped`** |
| **RED sin las CT** (la suite completa de Core y los filtros de UI de la precondición 6) | **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`** | los 18 filtros de UI de la precondición 6: solo cae el equivalente de CT-PBT2B-06, por aserción |
| **GREEN sin las CT** (la precondición 6) | **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`** | los 18 filtros: 0 `Failed` |

- Core: 19 619 = 19 605 + 3 + 11; 19 158 = 19 147 − 3 + 3 + 11; 461 = 458 + 3.
- UI: 2 486 = 2 481 + 1 + 4; 2 437 = 2 433 − 1 + 1 + 4; 49 = 48 + 1; en el RED, 2 432 = 2 437 − 5.
- Sin las CT: 19 608 = 19 605 + 3; 19 147 = 19 147 − 3 + 3.
- **9** son las CT de Core con `ExpectRed` true (6 + 3); en UI, las 4 CT y el equivalente de CT-PBT2B-06 (**5**).
- **K = 4, D = 0 y E = 4** (§9 y §11): los `Skip` de la lista, las pruebas que fallan fuera de ella y los equivalentes. **D = 0 medido** en la precondición 6 de la v5.1 (`/d/r5j`), con la suite completa de Core sobre el RED exacto y sobre el GREEN literal, sin las CT, y los 18 filtros de UI (`pbt4ccc/compile/pbt4c-precondition6.json`, sha256 `29179a12ef8623f4c276f943a725ece6f3ffd84464f16705e1feda757efbce1a`, y `compile-check.md`, `060c5c8b4ea1197079bcde5f5c5d8d299d5b71dde6295d2a468982e5fb1bb02c`); y, en la v5.0, en `/d/r5g`, como historia.

### 5.4 La CI del RED

`tests` en failure, `build-ui` en success y `build-plugin` omitido. `ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI, y en PB-T4d, que no añade ninguna CT de UI y cuyas bajas de UI no caen.

### 5.5 Lo que queda para el Smoke-PB (PB-T5, D-PB-09; I-10; M-27)

La lista de la v3:
- el botón abre la ventana modal real;
- el dibujo no se edita con ella abierta (OB-01.4);
- el borrado físico (PB-T3c);
- A2-11 hasta el DWG;
- el coste;
- «Restaurar»;
- OWNER-2;
- **la lectura completa en AutoCAD** (PB-T4a): un rack A/B se abre en el panel (OB-03.2);
- **la relectura con el origen tras borrar la vista de origen** (A-T3-8 con POST reanclado), y su coste: la relectura pura de `AtOrigin` repite las dos lecturas sobre el escaneo ya tomado;
- **la regla única cableada:** los mensajes del campo del panel y de la caja son iguales en AutoCAD;
- **el abridor cableado:** la ventana se abre desde la paleta, sin dueño, con el modal del panel, y nada se escribe tras cerrarla;
- **el paso 3 con un cambio real del dibujo** entre la presentación y el «Sí»;
- **K = 1 con el modal real:** la paleta deshabilitada.

Más los riesgos del borrado real de PB-T3c (F4B-E48, `f4b-dispositions.md:1609`):
- OWNER-1: las referencias de la vista en espacio papel o dentro de bloques del usuario se borran sin aviso;
- OWNER-2: la purga de las anidadas es de un nivel;
- OWNER-3: los bloques dinámicos.

Se observan con «Editar todo» → «Actualizar» sobre racks reales, y junto a la línea «Vistas: se borrará…» de la presentación. Los racks A/B fabricados se rotulan «CONSTRUCTED REPRESENTATIVE TEST CASES».

### 5.6 PB-T4d: la lista cerrada CT-PBT4D-01 y 02 (entera; V52-04; V53-01)

**Las reglas de §5.0,** enteras: un `[Fact]` por CT, que acaba en un `Assert.Equal` de las líneas unidas por `\n`; ninguna `[Theory]` nueva; la regla «EXCEPCION». Las dos CT van en `tests/RackCad.Tests/`, con `namespace RackCad.Tests` y `#nullable enable` en su cabecera. Ningún nombre de clase nueva empieza por `Workspace` ni por el prefijo de un filtro heredado: el prefijo `PushBackRetirePilot` no lo es de ninguno (D-S12).

**De dónde sale cada línea:** de su clase, literal del paquete (anexo V54-A; sin cambio desde la v5.2). El ensayo de las CT de texto (§6.4.1, el paso 8) las corre tal cual sobre el RED y sobre el GREEN.

| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |
|---|---|---|---|---|
| **01** | **La retirada, en texto** (D-PBT4-10; D-S11; `owner-pbt4d-deadcode-27-decisions.md`, pregunta 1): sobre el texto de `src/` sin comentarios, los dos archivos que se borran no existen; ninguna de las once declaraciones retiradas está en su archivo (las dos últimas, el modo de `BuildCore` que no borra vistas, `allowErase`, y su texto para una vista no disponible, `ViewUnavailablePrefix` y `ViewUnavailableSuffix`); ningún archivo de `src/` nombra lo retirado; y las siete piezas que se quedan están una vez. Con autopruebas sobre texto inyectado en memoria | `retirado-archivo PushBackPilotStateComparer.cs existe=False`; `retirado-archivo PushBackPilotFieldAdapter.cs existe=False`; `retirado PushBackPilotMaterializer.ComposeForWrite=0`; `retirado PushBackPilotMaterializer.KeepPilotEditsOn=0`; `retirado PushBackPilotComposition=0`; `retirado PushBackPilotPrepareInput=0`; `retirado PushBackPilotUpdatePlanner.Prepare=0`; `retirado PushBackPilotUpdatePlanner.Build=0`; `retirado PushBackPilotUpdateRun.Precheck(piloto)=0`; `retirado PushBackPilotUpdateRun.Execute=0`; `retirado PushBackPilotUpdateRun.ComparePost=0`; `retirado PushBackPilotUpdatePlanner.BuildCore(allowErase)=0`; `retirado PushBackPilotUpdatePlanner.ViewUnavailable=0`; `src-nombra-lo-retirado archivos=[]`; `se-queda PushBackPilotIntentReader.Read=1`; `se-queda PushBackPilotMaterializer.Materialize=1`; `se-queda PushBackPilotDraftState=1`; `se-queda PushBackPilotUpdatePlanner.PrepareFull=1`; `se-queda PushBackPilotUpdateRun.ExecuteFull=1`; `se-queda PushBackPilotUpdateRun.Precheck(completo)=1`; `se-queda PushBackPilotUpdateRun.ComparePostFull=1`; `autoprueba-archivo-PushBackPilotStateComparer.cs=falla`; `autoprueba-archivo-PushBackPilotFieldAdapter.cs=falla`; `autoprueba-PushBackPilotMaterializer.ComposeForWrite=falla`; `autoprueba-PushBackPilotMaterializer.KeepPilotEditsOn=falla`; `autoprueba-PushBackPilotComposition=falla`; `autoprueba-PushBackPilotPrepareInput=falla`; `autoprueba-PushBackPilotUpdatePlanner.Prepare=falla`; `autoprueba-PushBackPilotUpdatePlanner.Build=falla`; `autoprueba-PushBackPilotUpdateRun.Precheck(piloto)=falla`; `autoprueba-PushBackPilotUpdateRun.Execute=falla`; `autoprueba-PushBackPilotUpdateRun.ComparePost=falla`; `autoprueba-PushBackPilotUpdatePlanner.BuildCore(allowErase)=falla`; `autoprueba-PushBackPilotUpdatePlanner.ViewUnavailable=falla`; `autoprueba-uso-en-WorkspaceHostCore.cs=falla` (35) | `PushBackRetirePilotGuardTests` | true |
| **02** | **El censo de `tests/`** (D-S11): ningún archivo `.cs` de los dos proyectos de pruebas nombra lo retirado como código (sin comentarios ni literales de cadena o de carácter, también los literales verbatim, interpolados y crudos). Con autopruebas: cada nombre, inyectado como código en el texto de un archivo en memoria | `tests archivos-revisados>0=True nombran-lo-retirado=[]`; `autoprueba-PushBackPilotStateComparer-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-PushBackPilotFieldAdapter-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-ComposeForWrite-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-KeepPilotEditsOn-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-PushBackPilotPrepareInput-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-PushBackPilotComposition-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-ComparePost-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-PushBackPilotUpdatePlanner.Prepare-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-PushBackPilotUpdateRun.Execute-en-WorkspacePilotPrepareFull.cs=falla`; `autoprueba-ViewUnavailable-en-WorkspacePilotPrepareFull.cs=falla` (11) | `PushBackRetirePilotCensusGuardTests` | **false** |

- **En CT-01,** las expresiones de cada declaración son las de la clase (anexo V52-A): las de lo retirado se cuentan en su archivo (`allowErase` como palabra; las dos constantes con `\bViewUnavailable(?:Prefix|Suffix)\b`); `src-nombra-lo-retirado` busca los siete nombres como palabra y las dos llamadas (`PushBackPilotUpdatePlanner.Prepare(` y `PushBackPilotUpdateRun.Execute(`) en todo `src/` sin comentarios; un archivo que no existe es su texto vacío, y la autoprueba de un archivo le da un texto.
- **En CT-02,** los nombres son los mismos siete, las dos llamadas y las dos constantes de la vista no disponible; `ComparePost` se busca como palabra, así que `ComparePostFull` y `ComparePostCore` no casan.

**Lo que fija cada `ExpectRed`:**
- **En el RED,** `src/` sigue con el piloto: CT-01 cae por su `Assert.Equal` (los dos archivos existen, las once declaraciones están y cinco archivos de `Pilot/` nombran lo retirado). CT-02 pasa: el RED ya quitó de `tests/` todo lo que lo nombra.
- **En el GREEN,** las dos pasan.

**Las clases:** Core, `ExpectRed` true: **`PushBackRetirePilotGuardTests`** (CT-01); Core, `ExpectRed` false: **`PushBackRetirePilotCensusGuardTests`** (CT-02). **2 CT de Core y ninguna de UI.**

**Y las tres clases nuevas que no son CT** (su texto es el de la pieza del RED y no admiten reparación; §6.4.1), en Core y con `ExpectRed` false:
- **`PushBackRetirePilotFullCopyGuardTests`** (D-S12): el equivalente de PB-T4c de CT-PBT4B-20, copia literal con los `BaseSha`, los `Skip`, las `CopySha` y los recuentos que PB-T4d cambia (`make_fullcopy_eq_v52.py`, sobre los textos finales del RED de la v5.3); sus líneas esperadas son las del original, las mismas 53;
- **`PushBackRetirePilotPlanGuardTests`** (D-S12): las dos guardas de `WorkspacePilotPrepareGuardTests`, cada una con su cambio declarado;
- **`PushBackRetirePilotRunTests`** (`owner-pbt4d-run-coverage-decision.md`; `owner-pbt4d-deadcode-27-decisions.md`, pregunta 2; `owner-pbt4d-cat4d-decisions.md`, pregunta 1): las 29 sustitutas de `WorkspacePilotUpdateRunTests` sobre el modelo completo, con el ayudante `FullUpdateSetup` (`v53/make_run_eq_v53.py`): 27 de la corrida genérica, sobre `ExecuteFull`, y la n.º 48 y la n.º 49, de `PushBackPilotSystemRebuild` sobre el plan del PREPARE completo (CA-T4d I-01). 26 por el mapa de PB-T4b (la n.º 48, con una nota); a mano, la n.º 10 (el `Invalid` de PREPARE dentro de la corrida es la frontal B de un rack sin lado B), la n.º 43 (la lectura y la comparación de POST del modelo completo) y la n.º 49 (solo el comentario del lector). 31 resultados (la n.º 42 es una `Theory` de 3).

Las tres pasan en las dos ramas: el RED ya lleva los textos de `tests/` que miden, y `PrepareFull`, `PushBackFullPrepareInput`, su fábrica, `ExecuteFull` y `PushBackPilotSystemRebuild` no cambian en el GREEN.

## 6. Los contratos propuestos

### 6.0 Común (entero; V41-12)

**Identidad:**
- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B;
- `attempts`: cada subtarea empieza en 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true;
- evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Los `TaskId`:** con el prefijo **`F4B-PBT4-`** y **como mucho 26 caracteres** (V41-11; MAX_PATH solo acota por arriba): `F4B-PBT4-FULL-READ-PATH` (23), `F4B-PBT4-FULL-DRAFT-PANEL` (25) y `F4B-PBT4-EDIT-ALL-WINDOW` (24). El prefijo es el que liga `--task PBT4` en los sustitutos de nc1 y nc2 (`nc1_cascade_signal_substitute.py:205`; `nc2_exact_whitelist_substitute_f4b_v2.py:410`), así que se usan sin editarlos, con `--task PBT4` en las tres subtareas.

**Las colecciones opacas** (F4B-E21): las ocho (`Authorities`, `Objective`, `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`, `RequiredTests`, `ExpectedEvidence` y `StopConditions`) las pone la sesión byte a byte desde el contrato. El Controller hace la clase, el enrutamiento y los criterios de aceptación. Cada relevo lo declara. La comparación byte a byte, A1-A8 y nc4 van sobre la delegación final.

**`ExpectedEvidence[0]`** es, **byte a byte**, el literal de PB-T3 de la lista cerrada del sustituto de nc2 (el `ExpectedEvidence[0]` del contrato de PB-T3c):

> antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;

Si no, hay que añadir un literal al sustituto y volver a revisarlo (F4B-E43).

**El tamaño** (F4B-E21): el contrato de PB-T3c mide 54 162 bytes. Cada contrato de PB-T4 lleva solo sus CT y sus listas. La de PB-T4b va en un archivo custodiado que el contrato cita por su sha256 (`pbt4b-substitution-list-475.tsv`, `654c3b7b…0707c`; la v4.3, `pbt4b-substitution-list.tsv`, `44393b19…63e4`; D-PBT4-11), porque como tabla dentro del contrato pasaría de unos 60 KB.

**Las rutas `UNIT_CHANGE` y F4B-E25:** cada contrato declara `UNIT_CHANGE` las rutas **existentes** de producto que cambia:
- **PB-T4a:** WEI, WHC, RIA y NH;
- **PB-T4b:** WHC, WEI, FVM, PV, `src/RackCad.UI/Controls/NumericFieldValidation.cs` y RIA (D-S4, V45-02; el GREEN literal no toca `WorkspacePilotSupport.cs`, WCo ni NH). **Ningún archivo de `Pilot/`:** la retirada es de PB-T4d;
- **PB-T4c:** WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, `WorkspacePanelView.xaml` y `WorkspacePanelView.xaml.cs`, WCo y `RackPushBackSystemWindow.xaml.cs` (W; `W.xaml` no cambia);
- **PB-T4d:** los seis archivos de `Pilot/` del alcance (§6.4): `PushBackPilotMaterializer.cs`, `PushBackPilotUpdatePlan.cs`, `PushBackPilotUpdateRun.cs` y `PushBackPilotDraftState.cs` (solo comentarios, D-S15), que cambian, y `PushBackPilotStateComparer.cs` y `PushBackPilotFieldAdapter.cs`, que se borran.

**En una corrección** (F4B-E25, `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md`), una ruta `UNIT_CHANGE` que aparezca en `git diff --name-only AuthorityRevision BaseSha` no incumple 16.3 si se cumplen las tres condiciones del Owner, que el Controller comprueba con git:
1. todo commit de `AuthorityRevision..BaseSha` que la toca es un `RedSha` o un `CurrentSha` de una entrega de un Worker de la misma cadena (el mismo `TaskId`), según su `worker-handoff.json`;
2. esa ruta está en el `AllowedWriteScope` de la delegación de esa entrega;
3. ningún commit de la sesión de `AuthorityRevision..BaseSha` toca esa ruta.

Las rutas `EXTERNAL` siguen prohibidas en ese diff, sin excepción. F4B-E25 va entre las autoridades de cada contrato.

**`ForbiddenWriteScope`,** lo calcula el generador con el método de [PKC-C], como en PB-T3:
- todo `src/` salvo el `AllowedWriteScope` de la subtarea;
- en `tests/`: las listas curadas de PB-T1, las clases de PB-T1, PB-T2 y PB-T3, `tests/RackCad.Tests/Fixtures/I64/`, `tests/RackCad.UI.Tests/Workspace/` y el prefijo `tests/RackCad.Tests/Workspace/`;
- **salvo** las rutas de `tests/` del `AllowedWriteScope`, que se sacan de la lista (en PB-T4a, `PushBackDraftWriteGuardTests.cs`, una clase de PB-T3, por el `Skip`). Si una de ellas cae dentro de un prefijo prohibido, el prefijo se sustituye por la lista explícita de sus archivos menos esa, como en PB-T3b;
- las 17 entradas de la raíz.

**La comprobación antes de emitir** (precondición 6; la lección de F4B-E45): la sesión compila sobre una copia exportada el esqueleto exacto del RED, ejecuta la suite completa de Core y los filtros de UI de la subtarea, barre los nombres y las sobrecargas, y mide las guardas sobre el texto literal del GREEN del paquete. **El GREEN de referencia no existe:** el paquete prescribe el texto literal del GREEN donde una guarda lo lee, y la sesión lo aplica en memoria, solo para las guardas de texto.

**Las paradas estándar** (17), copiadas de [PKC2] §6.7 como en el contrato de PB-T3c, con P-01 en la Snapshot K (F4B-E37):

- **S-02:** `architecture outside Freeze`
- **S-03:** `material ambiguity`
- **S-04:** `contradictory evidence`
- **S-05:** `destructive or irreversible action not authorized`
- **S-07:** `conflict with another active owner`
- **S-08:** `branch or worktree ownership conflict`
- **S-12:** `Worker loses context or cannot verify AuthorityRevision`
- **S-13:** `origin/main movement invalidates exact-SHA assumptions; apply AUTOMATION_PLAN 16.7`
- **S-14:** `I-64 Freeze or contract explicitly says STOP`
- **P-01:** `config.toml no longer matches the accepted Snapshot in force (today Snapshot K, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json)`
- **P-02:** `foreign live participant, unattributable process or more than one open delegation`
- **P-03:** `delegation outside gate contract`
- **P-04:** `BLOCKED cap reached`
- **P-05:** `invocation differs from accepted delegation package`
- **P-06:** `usage or credit-limit warning`
- **P-07:** `another invocation would exceed the invocation budget`
- **P-08:** `responsible session operates the worktree during cession`

**Las de [DP], literales, en cada contrato** (x = A, B o C, con el prefijo `PBT4x`):
- **PBT4x-C08:** `a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner`
- **PBT4x-C12:** `a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)`
- **PBT4x-C11, con los sustitutos de nc2 y nc1 vigentes:** el texto de `PBT3C-C11` del contrato de PB-T3c con `PBT3C` cambiado por `PBT4` (el `--task` de nc1 y nc2 es `PBT4` en las tres subtareas, y sus registros, `SATISFIED_UNDER_PBT4_…`) y «PB-T3c» por el nombre de la subtarea. El de PB-T4a, entero, en §6.1.1. nc3 no tiene sustituto (F4B-E46).

**El límite de D2, en cada contrato:** **PBT4x-C17,** con el texto literal del Owner (el de `PBT3C-C17`):

> (F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." The Worker chooses none of the three exits: the Owner does

El Worker no elige ninguna salida.

**La reparación del RED** (F4B-E35, pregunta 4, la regla RR del Owner; como en [P3] §6.0): un error de escritura en una prueba nueva de la tarea, visto después del commit RED, se repara sin gastar intento (RR-7) si se cumple todo lo que sigue. La regla excepciona AUTOMATION_PLAN 16.8 («tests/ no se toca tras el RED») solo en estas condiciones:
- **RR-1, cuándo:** después del commit RED y su push, y antes del commit GREEN; como mucho una reparación por intento. Un segundo error de escritura en el mismo intento es la parada de siempre (C07).
- **RR-2, qué toca:** solo archivos de `tests/` que el commit RED de la tarea añadió; nunca una prueba existente, ni `src/` (tampoco el esqueleto inerte del RED), ni un fixture, ni nada fuera del `AllowedWriteScope`. **En PB-T4a,** `PushBackDraftWriteGuardTests.cs` no lo añadió el RED, sino que lo cambió (el `Skip`): una reparación no lo toca, y un error en el `Skip` o en su motivo literal es la parada de siempre. **En PB-T4b,** lo mismo para los 21 archivos con sus `Skip`; y una reparación tampoco toca el kit ni las 21 copias, cuyo texto es el del paquete. **En PB-T4d,** el RED cambia 26 archivos existentes de `tests/` (edita 23 y, desde la v5.3, borra 3) y añade cinco: una reparación solo toca las dos clases de las CT (`PushBackRetirePilotGuardTests.cs` y `PushBackRetirePilotCensusGuardTests.cs`), nunca los 26 existentes ni las tres clases nuevas que no son CT, cuyo texto es el del paquete.
- **RR-3, sin cambiar lo esperado:** ninguna línea esperada, literal ni oráculo del paquete o del contrato cambia; la reparación corrige solo el cuándo y el cómo de una lectura o de un gesto que el paquete ya prescribe. Si para que la prueba pase hace falta cambiar una línea esperada, es C07.
- **RR-4, el RED repetido entero:** sobre `RedRepairSha` se vuelve a pasar el criterio completo del RED del contrato (INV-PBT4x-02); el commit se empuja y su CI se registra como la del RED reparado.
- **RR-5, declarado:** el commit se titula «RED repair» y nombra la tarea; la entrega declara `RedRepairSha`, la prueba y la línea corregidas, el error y por qué no cambia ninguna línea esperada, con el diff; `RedSha` sigue siendo el commit RED original.
- **RR-6, la cadena:** `BaseSha..RedSha`, el RED de cada contrato; `RedSha..RedRepairSha`, solo archivos que el RED añadió; `RedRepairSha..CurrentSha`, el GREEN de cada contrato. Sin reparación, la cadena es la de siempre.
- **Las dos lecturas de PB-T4, porque los tres RED tocan `src/`** (el esqueleto) y `tests/` existentes (los `Skip`): RR-1, «antes del primer commit que toque `src/`», se lee «antes del commit GREEN»; el primer tramo de RR-6, «BaseSha..RedSha, solo `tests/`», se lee «el RED de cada contrato».

**La verificación con una reparación del RED** (F4B-E35, RR-7):
- **n.º 10 (`Tests`):** el RED se acredita con `RedSha` y con la CI del RED original, y sus TRX, con los filtros con `ExpectRed` entre los fallidos. La cláusula «`git diff --name-only RedSha CurrentSha` no toca `RT` ∪ `ChainRedFiles`» se comprueba desde `RedRepairSha`.
- **n.º 11 (`Trailer`):** el trailer va también en el commit «RED repair».
- **Lo que el Controller comprueba de RR** (si algo falla, es C18): RR-1, RR-2, RR-4, RR-5 y RR-6, como en PB-T3c. **Por subtarea, además,** el tramo `RedSha..RedRepairSha` no toca: en PB-T4a, ningún archivo de `src/` ni `PushBackDraftWriteGuardTests.cs`; en PB-T4b, ningún archivo de `src/`, ni los 21 archivos de los `Skip`, ni el kit, ni las copias; en PB-T4c, ningún archivo de `src/`, ni los cuatro equivalentes, ni los cuatro archivos de los `Skip` (PEG, `PushBackSharedReferenceGuardTests.cs`, `WorkspaceFullCopyGuardTests.cs` y `PushBackWorkspaceApplyCloseTests.cs`); en PB-T4d, ningún archivo de `src/`, ni los 26 archivos existentes de `tests/` que el RED edita, ni las tres clases nuevas que no son CT (`PushBackRetirePilotRunTests`, `PushBackRetirePilotFullCopyGuardTests` y `PushBackRetirePilotPlanGuardTests`). **Los equivalentes no admiten reparación** (RR-2): su texto es el del guion, con su `EquivalentSha` (§6.3.1), y no tienen fila en la tabla de abajo, que es la de las clases de las CT.
- **La ayuda mecánica de RR-3:** es parada C07 que un literal de la lista cerrada de la clase reparada (la tabla de abajo) cambie entre `RedSha` y `RedRepairSha`: una línea o un fragmento, si no aparece como subcadena exacta el mismo número de veces; un valor de gesto, si no aparece como token el mismo número de veces.
- **Indicios para la revisión, no parada:** una cadena `<campo>=` añadida o quitada en `git diff -U0 RedSha RedRepairSha`, o un cambio en el primer argumento de un `Assert.Equal`.

**La lista cerrada de la ayuda de RR-3, por clase.** «Línea» es una línea esperada o un fragmento (se compara como subcadena); «valor», un valor de gesto (como token):

| Subtarea | Clase | Literales fijados por el paquete |
|---|---|---|
| PB-T4a | `PushBackFullReadPathTests` (CT-01 a 04 y 07) | **Líneas:** las de CT-01 a 04 y 07 de §5.1, literales; de CT-02, los fragmentos ` piloto=`, ` completo=` y ` mismo-escaneo=True`, y la línea `pb-20-ab piloto=NotAdmitted completo=Admitted mismo-escaneo=True`; de CT-03, ` lector=ReadIntentAtOrigin origen-del-borrador=True` y `Begin lector=ReadIntent` |
| PB-T4a | `PushBackFullReadGuardTests` (CT-05, 06 y 08) | **Líneas:** de CT-05, ` lector=ReadIntent`; de CT-06, los cinco nombres (`PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` y `PushBackViewDispositions`) y los dos destinos (`WorkspaceHostCore.cs` y `WorkspacePilotSupport.cs`); de CT-08, `g-l1 archivos=1 reglas-sin-coincidencias=True`, `pureza archivos=1 violaciones=[]`, las ocho fichas de G-L1 (`WorkingBaseline`, `workingBaseline`, `LastModuleReconciliation`, `computation.System`, `lastComputation.System`, `session.System`, `lastCompositeSystem` y `SystemToInsert`) y los 15 términos de la pureza (`#nullable`, `namespace RackCad.Application.Systems.PushBack.Draft`, `Autodesk`, `RackCad.Plugin`, `RackCad.UI`, `System.Windows`, `Transaction`, `Database`, `async`, `await`, `Task`, `File.`, `System.IO`, `RackEmbedComposer` y `new RackEmbedDocument`) |
| PB-T4a | `PushBackFullReadSupport` (el kit y el doble) | **Líneas:** `ReadIntentAtOrigin` y `FromReads` |
| PB-T4b | `PushBackPanelDraftGuardTests` (CT-19) | **Líneas:** `g-l1 archivos=3 reglas-sin-coincidencias=True`, `pureza archivos=3 violaciones=[]`, `cubre Apply=PushBackFullFieldAdapter.cs`, y las fichas y los términos de la fila de CT-PBT4A-08 |
| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20) | **Líneas:** ` igual-a-la-base=True`, ` igual-al-mapa=True`, ` sha256=True`, las cuatro `particion-por-filas` con ` filas-del-original=True` (D-S5), `kit FullOf-igual-al-lector=True escaneos=5`, los 21 nombres de clase de origen, las nueve parejas de M-00 a M-08 (con los seis tipos de M-00 de §5.2), los `BaseSha` de los 21 archivos de los `Skip` (§6.2.1) y los `CopySha` de las 21 copias y del kit; los cinco escaneos de M-05 |
| PB-T4b | `PushBackFullPanelTests` (CT-01, 02, 03, 05 a 11 y 17) | **Líneas:** las de esas CT en §5.2, literales; los fragmentos ` estado=Presented`, ` huellas-iguales=True`, `borrado EraseUnit:B2`, `presentar Cambiado fuera: el nombre.`, `presentar Cambiado por ti: las anotaciones.`, las once `of…` y `No editable aquí: la lectura no trae el modelo completo.` **Valores:** `2.25`, `2.5` y `Otro nombre` |
| PB-T4b | `PushBackFullPanelGuardTests` (CT-12, 15 y 16) | **Líneas:** las de esas CT en §5.2; los seis nombres de CT-12, `PushBackPilotUpdateRun.ExecuteFull(`, `PushBackPilotUpdateRun.Precheck(`, `kind == ChoiceKind.KeepMine ? keepWitness : null`, `PushBackFullFieldAdapter` y `PushBackAnnotationScaleRule.Validate` |
| PB-T4b | `PushBackFullPanelGuardTests` (CT-21) | **Líneas:** las de CT-21 en §5.2 y sus expresiones literales; `System.Lazy<PushBackFullIntentResult>`, `_full?.Value`, `read.Full` y `var injected = read.Full;` |
| PB-T4b | `PushBackFullPanelCensusGuardTests` (CT-13) | **Líneas:** `sin-conectar archivos-revisados>0=True coincidencias=[]`, los cinco nombres (`PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` y `PushBackViewDispositions`) y los dos destinos (`WorkspacePilotSupport.cs` y `WorkspaceEditIntent.cs`) |
| PB-T4b | `PushBackFullPanelFieldTests` (UI; CT-04, 14 y 18) | **Líneas:** las de esas CT en §5.2; `BrowserFieldKeepMinePresentationText` y `Borrador conservado sobre la versión actual del dibujo.` **Valores:** los diez textos de CT-PBT2B-10 y los once casos de CT-18 |
| PB-T4b | `WorkspacePilotPrepareFull`, `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` (a mano, R-D-S1 y R-D-S5; partida por filas en las dos clases) | **El texto entero de sus dos mitades, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `EachOperationReadsOnceAndSetsTheMarkOfReadInProgressAroundTheReadAndResetsItAfterwards` (a mano, R-C3) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `AnExceptionOfTheReaderIsFailedWithTheMessageReportedAndTheMarkAndTheSessionAreNotTouched` (a mano, R-C3) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `KeepAfterAChangeOfTheDrawingStaysOnTheNewVersionWithTheScaleAndThePendingTextOfTheDraft` (a mano, R-retirado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `KeepAConflictLeavesItAndTheDraftIsDirtyOnTheNewVersion` (a mano, R-D-S1) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `AKeepThatIsKeptEndsTheMarkAndOneThatIsNotNeededOrNotAdmittedDoesNot` (a mano, R-D-S1) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `InsideANoticeTheGateAnswersWaitingForTheThreeOperationsWithoutDelegatingAndOutsideItDelegates` (a mano, R-C3) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `TheNewOutcomesAreValuesWithTheStatesOfThePackageAndTheDefaultIsUnavailable` (a mano, R-D-S1) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `PendingTextComesBeforeEverythingButNotApplicableAndBeforeTheReread` (a mano, R-retirado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `ACleanDraftHasNothingToApplyAndTheRereadIsNotLookedAt` (a mano, R-retirado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `InvalidA_ACurrentStateThatDoesNotComposeIsInvalidWithTheReasonOfTheCompositionAndOverTheCarryOversOfTheMaterializer` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `InvalidA_AnOriginThatNoLongerMaterializesIsInvalidWithTheReasonOfTheMaterializerBeforeAnythingElse` (a mano, R-estado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten` (a mano, R-retirado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `PrepareIsTotalAMissingPieceIsInvalidAndNeverAnException` (a mano, R-retirado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `AMissingCatalogIsAnsweredAfterTheConflictAndTheAdmissionAndNotBeforeThem` (a mano, R-retirado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `TheOutgoingDesignAndEnvelopeOfEachViewComeFromTheRereadEnvelopeAndNeverFromTheDraft` (a mano, R-retirado,R-estado) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentHostFull`, `TheTechnicalDetailOfTheDiagnosticTravelsApartFromItsMainText` (a mano, R-C2) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentHostFull`, `CheckNowUsesTheBaseOfARereadThatIsNotAdmittedToo` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentHostFull`, `TheUpdateOfThePilotIsAPublicMethodOfTheCoreWhoseSecondParameterIsThePortOfTheUpdateAndItsOutcomeNamesNoTypeOfTheDraftsNorOfThePilot` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentHostFull`, `ThePortHasExactlyTheTwoMethodsOfTheRequestWithUpdateInTheirNameAndRejectsAThirdOne` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentHostFull`, `TheReadResultCarriesThePilotReadOnlyWhenItIsARead` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateRequestFull`, `ARequestOfASessionThatIsNoLongerTheActiveOneIsRejectedByItsAuthorizationAndNeverWaitsForTheFlightOfThreeOperations` (a mano, R-C3) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateRequestFull`, `TheOutcomeThatT4BGivesIsKeptWithTheSameStatusReasonDifferencesAndWarningsAsThePlainCallOfTheUpdate` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateRequestFull`, `DuringTheFlightTheFiveOperationsThatCouldChangeTheDraftAnswerBusyOrWaitingAndTheDraftDoesNotChange` (a mano, R-C3) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateRequestFull`, `TheTextsOfTheGateOfTheUpdateAreTheConstantsOfTheRunWithTheirOldWording` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateRequestFull`, `AnUpdateOfTheCoreThatRunsOverASlotThatWasTakenAwayNeverAnswersAnApplied` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `TheGateRejectsAModalAWrongSessionAndNoDocumentWithoutReadingAnythingAndWithoutTheSlot` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `TheGateHasNoRestAndNoReaderSoACommandInProgressAndAMissingReaderDoNotStopTheUpdate` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `TheUpdateOfThePilotNeverUsesTheReaderOfTheIntention` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `ABlankRackAndAnUnknownRackHaveNoDraftAndNothingIsRun` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `TheCheckBeforeTheSlotAnswersNothingToApplyPendingTextAndNotApplicableWithoutTakingTheSlot` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AnotherUpdateInProgressInTheRegistryAnswersItAndRunsNothing` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `ADraftThatIsAlreadyApplyingIsRefusedWithTheNameOfTheRefusal` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AFailureBeforeTheSlotIsFailedAndLoggedAndTheSessionIsNotDegraded` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `TheSlotIsClosedExactlyOnceAndIsFreeAfterEveryAnswer` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AnAppliedUpdateRebasesTheDraftAndRunsThePortWithTheAuthorizedDocumentAndTheRack` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AConflictOfMutateIsNotAppliedConflictWithTheDifferencesAndTheDraftGoesToConflict` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `ANotAdmittedReadWithTheBaseOfTheDraftIsNotAppliedAdmissionAndTheDraftKeepsItsStatus` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `APreflightAndAFailureKeepTheDraftAsItWasAndCarryTheirCause` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `ASealThatThrewIsCommitThrewAndTheDraftIsUnknownWithItsOrigin` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AnAppliedUpdateWithWarningsKeepsThemAndTheDraftIsClean` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `APostThatFailedIsAppliedPostRereadFailedWithTheCauseAndTheWarningsAndTheDraftIsUnknown` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `ADraftThatWasDiscardedDuringTheRunIsClosedDetachedAndTheSlotIsFree` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AnErrorThatArrivesDuringTheRunIsAppliedAfterTheClassificationAndTheAnswerIsClosedInError` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AnErrorThatArrivesDuringAnUpdateWithWarningsKeepsTheWarningsAndTheClassificationOfTheClosedInErrorAnswer` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AnErrorThatArrivesDuringAConflictKeepsTheConflictAsTheClassificationAndWritesNothing` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `AnErrorThatArrivesDuringACommitThatThrewKeepsTheUnknownVisibleAsTheClassificationWithItsCause` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `TheAnswerOfAnUpdateHasTheStatusOfTheClassOfOutcomeThatClosedTheDraftForEveryClass` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateHostFull`, `TheAnswerOfAnUpdateAlwaysComesFromTheCloseAndTheOutcomeNeverNamesADraft` (a mano, R-C4) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `ConfirmWithAChangeOfTheScaleAndOfAnotherFieldIsAConflictAndKeepAfterwardsKeepsTheScaleOfTheDraftOnTheNewVersion` (a mano, R-D-S1) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `ConfirmOfARackThatIsNoLongerEditableWithTheSameBaseIsConfirmedWithTheDiagnosticAndTheHint` (a mano, R-C2,R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `ConfirmOfARackThatIsNoLongerEditableWithAnotherBaseIsAConflictWithBothThings` (a mano, R-C2,R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `AnExceptionInsideTheComparisonOfTheGeneralCheckIsFailedReportedAndChangesNothing` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `AnInvalidPendingTextDoesNotBlockTheThreeOperationsConfirmAndKeepLeaveItExactlyAsTypedAndDiscardEmptiesIt` (a mano, R-D-S1) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `TheThreeOperationsStillReadTheDrawingExactlyOnceAndTheGeneralCheckStillComparesWithTheBaseOfTheDraft` (a mano, R-D-S1) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `CheckNowWithARereadThatIsNotAdmittedAndMatchesTheBaseIsRecoveredAndCarriesTheDiagnosticAndTheHint` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `CheckNowWithARereadThatIsNotAdmittedAndDiffersFromTheBaseIsAConflictAndCarriesTheDiagnosticAndTheHint` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `TheDiagnosticNeverDecidesTheClassificationTheSameBaseGivesTheSameAnswerAndTheSameDraftWithOrWithoutAdmission` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `AFailureOfTheDiagnosticGivesNullAndNeverChangesTheClassificationNorTurnsAChangedDraftIntoFailed` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationHostFull`, `TheDiagnosticOfCheckNowReadsTheDrawingExactlyOnceAndWritesNothingAndANotAdmittedRereadStillReadsOnce` (a mano, R-C9) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `PushBackWritePathGuardFull`, `CT_PBT3B_13_NoFileOutsideThePlanTheRunAndTheModelNamesTheFullWriteAndTheHostFindsOnlyTheDraftOfThePilot` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateIntegrationGuardFull`, `G4OnlyTheExecuteOfTheRunOfTheCoreCallsTheUpdateAndOnlyTheCoreCallsTheRunOfT4B` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateIntegrationGuardFull`, `G4TripsOnAGroupOfMethodsOfTheUpdateOutsideTheCoreAndInsideItAndNotOnTheNameInsideAString` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentGuardFull`, `TheBaseOfAResultOfThePilotOnlyReachesBeginCheckNowOrDiscardAndReloadInTheHostCoreAndTheTwoOperationsCountTheirOwnUses` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentGuardFull`, `ThePerOperationCountsOfTheBaseAndOfTheReadInTheThreeBodiesOfTheReconciliationAreTheirOwnConsumer` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentGuardFull`, `TheResultOfThePilotIsOnlyEverNamedPilotInTheHostCoreSoNoAliasEscapesTheGuardOfTheUseOfTheBase` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentGuardFull`, `TheThreeBodiesOfTheReconciliationFollowTheStepsOfTheCheckAndTheReloadWithTheApplicabilityBeforeTheOneRead` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentGuardFull`, `TheBaseGuardAllowsTheConsumersOfTheObligation` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditGuardFull`, `TheHostImplementsThePortExplicitlyAndNeverReadsOutsideTheThreeReadsAndChangesOnlyThroughNotePilotEdit` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditGuardFull`, `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` (equivalente de una guarda, C-5; partido por filas en las dos clases, D-S5) | **El texto entero de sus dos mitades, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditGuardFull`, `TheFlightGuardTripsWhenTheFlightIsCheckedBeforeTheGateThatAuthorizesInTheThreeReconciliations` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditGuardFull`, `TheHostGuardAllowsWhatTheNextTaskWouldAddAnUpdateOperationThatWrites` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditGuardFull`, `TheTrapGuardTripsOnEachNameAndEachWordThatTheNeighbouringGuardsForbid` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotReconciliationGuardFull`, `TheGuardOfWhoReadsSeesAKeepInTheRequestACheckInThePresentationAnyOfTheFourInTheBrowserAndANameof` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentGuardFull`, `TheExpressionOfTheDiagnosticOfTheAdmissionIsInTheWholeHostCoreExactlyTwiceOnceInTheGeneralCheckAndOnceInCheckNow` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceEditIntentGuardFull`, `TheGuardOfTheExpressionOfTheDiagnosticSeesAThirdOneInTheCoreASecondOneInCheckNowAndOneInKeep` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceLifecycleGuardFull`, `TheMarkingOnlyTouchesTheDraftsOfThePilotOfTheSessionAndIsOnlyAHint` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceLifecycleGuardFull`, `TheReloadIsAMethodOfItsOwnWithTheGateAndTheEightStepsOfTheTableAndOneReadThatOnlyReachesDiscardAndReload` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotUpdateGuardFull`, `TheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose` (equivalente de una guarda, C-5) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `PushBackFullReadPathFull`, `CT_PBT4A_03_TheFiveRereadsOfAnOpenDraftReadAtTheOriginOfTheDraftAndBeginReadsAsToday` (equivalente de una guarda, C-5; owner-pbt4b-list-477-decision.md) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `PushBackFullReadGuardFull`, `CT_PBT4A_05_WithAReaderWithoutTheNewInterfaceTheFiveRereadsUseReadIntentAsToday` (equivalente de una guarda, C-5; owner-pbt4b-list-477-decision.md) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `PrepareDoesNotPutTheDraftInConflictNorCleanDirtyNorTakeTheSlotOfTheUpdate` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditHostFull`, `DiscardRackNeedsNoComparerAndRemovesAnUndecidableDraftToo` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditHostFull`, `AComparerThatThrowsMakesTheFieldUndecidableProtectedAndNotEditableAndGetFieldNeverThrows` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditHostFull`, `TheRacksAtRiskAreOrderedOrdinallyAndAComparerThatThrowsMakesThemUncertainApart` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditHostFull`, `TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditHostFull`, `TheFallbackByDraftIsKnownAndKeepsTheUncertainOnesApart` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditHostFull`, `TheReasonOfAFieldThatCannotBeReadIsInSpanish` (a mano, R-D-S1-compila) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotEditHostFull`, el ayudante `Env.OpenUndecidable` (editado en la copia, D-S1, V46-02) | **El texto entero del ayudante, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceLifecycleHostFull`, `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` (a mano, R-D-S5; partida por filas en las dos clases) | **El texto entero de sus dos mitades, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` (a mano, R-D-S5; partida por filas en las dos clases) | **El texto entero de sus dos mitades, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, los miembros nuevos `StatusNamesRed` y `StatusNamesGreen` (solo en la copia, D-S5, V47-01) | **El texto entero de cada miembro, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView` (a mano, R-D-S1-borra; D-S1, V48-02) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspacePilotPrepareFull`, `ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan` (a mano, R-D-S1-borra; D-S1, V48-02) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceHostHardeningGuardFull`, `NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept` (equivalente de una guarda, C-5; owner-pbt4b-list-478-decision.md) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | `WorkspaceHostHardeningGuardFull`, el ayudante `Table` (las tres cuentas nuevas, V48-03) | **El texto entero del ayudante, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |
| PB-T4b | Las 356 copias mecánicas, las 27 de la interfaz y el kit | **El texto entero, el del guion o el custodiado** (`CopySha` y el sha256 del kit): no admiten reparación (RR-2) |
| PB-T4c | `PushBackEditAllTests` (CT-03, 04, 05, 10, 11 y 12) | **Líneas:** las de esas CT en §5.3, literales **Valores:** `2.5` y `3` |
| PB-T4c | `PushBackEditAllGuardTests` (CT-01, 08 y 13) | **Líneas:** las de esas CT en §5.3, literales |
| PB-T4c | `PushBackOpenerClassicPathGuardTests` (CT-07) | **Líneas:** las de esas CT en §5.3, literales |
| PB-T4c | `PushBackDraftLaunchGuardTests` (CT-15) | **Líneas:** `g-l1 archivos=2 reglas-sin-coincidencias=True`, `pureza archivos=2 violaciones=[]`, `cubre ApplyToDraft=PushBackFullDraftStatePort.cs`, y las fichas y los términos de la fila de CT-PBT4A-08 |
| PB-T4c | `PushBackEditAllWindowTests` (UI; CT-02, 06, 09 y 14) | **Líneas:** las de esas CT en §5.3, literales **Valores:** `4.5` |
| PB-T4d | `PushBackRetirePilotGuardTests` (CT-01) | **Líneas:** las 35 de CT-01 en §5.6, literales, y las expresiones e inyecciones de su tabla `Retired` |
| PB-T4d | `PushBackRetirePilotCensusGuardTests` (CT-02) | **Líneas:** las 11 de CT-02 en §5.6, literales, y las expresiones e inyecciones de su tabla `RetiredUses` |

**Las filas de PB-T4c** (V50-06) son una por clase de las CT, con su clase y sus CT, en la forma de la de CT-19 de PB-T4b; las líneas de CT-08, entre ellas la conversión y la llamada del abridor, son de `PushBackEditAllGuardTests`.

**Las filas de PB-T4d** (V52-07) son una por clase de las CT, como las de PB-T4c. Las tres clases nuevas que no son CT no tienen fila: no admiten reparación (RR-2).

**La lectura de `Ci` en el RED:** Core falla a propósito en el RED, y `build-plugin` tiene `needs: [tests, build-ui]` (`.github/workflows/ci.yml`), así que sale **omitido**. `ExpectedEvidence` lo pide así: `tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido y `ui-tests` en `success` (PB-T4a) o en `failure` (PB-T4b y PB-T4c; en PB-T4c, con las 4 CT de UI y el equivalente de CT-PBT2B-06), y en `success` en PB-T4d (sin CT de UI; sus bajas de UI no caen), sin cuelgue. **El total exacto de cada rama** (F4B-E46). En el GREEN, 4/4 en `success`.

**El `Trailer`:** el de `worker-handoff.json` es el trailer real de los commits RED, «RED repair» (si lo hay) y GREEN, nunca `null` si los commits lo llevan.

**`EligibleCells`:** las del contrato de PB-T3c: Worker `anthropic/subagent/claude-sonnet-5-5`, effort `high`; Controller `openai/process/gpt-6-luna`, effort `high`; vencen el **2026-12-29**; nc4 en la primera delegación de cada cadena; nc1 a nc3 tras la verificación.

**Invariantes comunes** (cada contrato los numera INV-PBT4x-07 a 14):
- **07. Las redes:** §5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, salvo las de la lista cerrada de sustituciones de la subtarea, con su `Skip` de motivo literal (PB-T4a: solo CT-PBT3A-12). En `CurrentSha`, el `git diff -U0 BaseSha..CurrentSha` de los archivos de pruebas existentes solo lleva las líneas de atributo con `Skip`.
- **08. Las pruebas de UI:** las reglas de §5.0.
- **09. Determinismo:** GUID fijo, cultura invariante, «R» y orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:** solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, sí; sin copias de árboles; las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, corren dentro de `EditorDiscardPrompt.Substitute` cuando abren ventanas, no entran en ningún commit y guardan TRX y log como artefactos.
- **11. La evidencia:** rutas relativas comprobadas en `worker-handoff.json`; `TreeSha` real o null; el `Trailer`, el real; el Worker no escribe en `docs/`.
- **12.** Las reglas de F4B; sin AutoCAD; la verificación con las tres comprobaciones deterministas de F4B-E15.
- **13. Las declaraciones nuevas en `src/`:** las de la lista cerrada de la parada C13 de cada contrato, y ninguna más, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. En los archivos nuevos se admiten además métodos privados, tipos anidados privados sin estado y campos `private static readonly` de un tipo inmutable (`Regex` o `string`).
- **14. D2:** fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado.

**Las propias comunes** (ASCII, con el prefijo de cada contrato; las de PB-T4a, enteras, en §6.1.1):
- **C01:** `doing the task would require writing outside the AllowedWriteScope.`
- **C02:** editar, saltar, reactivar o borrar una prueba o un ayudante existente fuera de la lista cerrada de la subtarea; o tocar `InternalsVisibleTo`, los paquetes, un `.csproj` o `Fixtures/I64`.
- **C03:** un cambio observable de RACKEDITAR, o del panel fuera de lo declarado por la subtarea. STOP y vuelve al Owner.
- **C04:** `a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.`
- **C05:** `a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or writing to the profile or to disk.`
- **C06:** `the GREEN CI fails outside the new tests, reproducibly and not attributable to them.`
- **C07:** el texto de PB-T3c, con las recetas que se autocomprueban de la subtarea, y la excepción de RR.
- **C09:** `a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.`
- **C10:** lo que la subtarea excluye (D-PBT4-19 y su alcance).
- **C13:** la lista cerrada de declaraciones nuevas de la subtarea.
- **C14:** un diseño del corpus cambia de admisión o de bloqueo.
- **C16:** una prueba existente fuera de la lista cerrada de la subtarea falla. Vuelve al Owner (pregunta 4).
- **C17:** D2 (arriba).
- **C18:** `a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in the attempt; it touches a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope; it changes an expected line, literal or oracle, or anything other than the when or the how of a reading or a gesture the package already prescribes (RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP.`

### 6.1 PB-T4a — `F4B-PBT4-FULL-READ-PATH` (entero; V41-12)

- **Objective** (ASCII): el de §6.1.1.
- **`AllowedWriteScope`** (10):
  - `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` (WEI);
  - `src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs` (nuevo);
  - `src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs` (nuevo);
  - `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` (WHC);
  - `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs` (RIA);
  - `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs` (NH);
  - `tests/RackCad.Tests/PushBackFullReadPathTests.cs`, `tests/RackCad.Tests/PushBackFullReadGuardTests.cs` y `tests/RackCad.Tests/PushBackFullReadSupport.cs` (nuevos);
  - **solo para el `Skip`:** `tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs`.
- **`UNIT_CHANGE`:** WEI, WHC, RIA y NH (F4B-E25).
- **El RED:** las tres clases nuevas de `tests/`, el esqueleto exacto y el `Skip`. **El esqueleto exacto** (el parche, `pbt4a-skeleton.patch`, lleva `src/` y el `Skip`; la duda 6):
  - **WEI:** el constructor privado con los dos argumentos nuevos, `FullRead` y `Catalog` como en D-PBT4-02, las cuatro fábricas de fallo y `FromPilot` con `null, null`, y `FromReads` **inerte**: `public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog) => new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);` (sin comprobar nulos y con los dos campos en null);
  - **`IWorkspaceEditIntentOriginReader.cs`:** completo, el de D-PBT4-03;
  - **`Draft/PushBackOriginByText.cs`:** los `using`, el `namespace` de bloque y el comentario de D-PBT4-03, con `Choose` que devuelve `null` (`=> null;`), `AtKey` que devuelve `scan` (`=> scan;`) y `ReadAt` que devuelve `null` (`=> null;`), con sus firmas literales y su `using RackCad.Application.Catalogs;` (B-08). **La firma es `public static string Choose(DraftStaleBase staleBase, string originText)`, sin `?`** (CA-T4a I-01): con `string?`, el esqueleto ya daba CS8632;
  - **WHC, RIA y NH:** sin cambio;
  - **el `Skip`,** en `PushBackDraftWriteGuardTests.cs:60`, que pasa a ser esta línea, literal (ASCII, la única línea que cambia en el archivo):

    ```csharp
        [Fact(Skip = "I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]
    ```
- **El GREEN:** solo `src/`: WEI, `PushBackOriginByText.cs`, WHC, RIA y NH, con el texto literal de D-PBT4-02 y 03. `IWorkspaceEditIntentOriginReader.cs` no cambia desde el RED.
- **`RequiredTests`** (38; la tabla de §6.1.1).
- **Invariantes:** INV-PBT4A-01 a 06 (§6.1.1) y los comunes 07 a 14.
- **`StopConditions`:** las 17 estándar y PBT4A-C01 a C18 (§6.1.1): 34.
- **`ExpectedEvidence`** (15; §6.1.1), con la línea base y los totales exactos de la rama única de §5.1.
- **La línea base:** el cierre de PB-T3c (`7f0d9aa3`; la CI `44757675821` del GREEN `938a35f0`).

#### 6.1.1 Los textos literales del contrato de PB-T4a

Para la comparación del generador, en ASCII, campo a campo.

```text
TaskId: F4B-PBT4-FULL-READ-PATH

Objective:
Hacer que la lectura de la intencion de edicion del Workspace de Push Back traiga tambien el modelo completo de PB-T3 sobre el mismo escaneo: el adaptador del Plugin llama al lector completo tras el del piloto y el resultado de la lectura lleva la lectura completa y el catalogo junto a la del piloto (FromReads; FromPilot no cambia); y que las cinco relecturas de un borrador abierto vayan al origen del borrador por su texto (el unico miembro mutable con ese texto, elegido sobre el mismo escaneo, o el Primary de hoy si no hay uno) cuando el lector lo ofrece, con la eleccion en linea en cada cuerpo (A-T3-8). Es un cambio observable declarado: las relecturas dejan de ir al Primary. El tipo del borrador del panel no cambia, nada traduce estados, la firma de RackPilotIntentAdapter.Read no cambia y no se lee el dibujo mas de una vez por lectura, segun el paquete del contrato de PB-T4 v4.3, las decisiones del Owner sobre PB-T4 y la lista cerrada CT-PBT4A-01..08. Solo se sustituye con registro CT-PBT3A-12. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. No se ejecuta AutoCAD.

Invariants:
INV-PBT4A-01. Alcance: dentro de las 10 rutas. git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Domain vacio; en src/RackCad.Plugin solo RIA y NH; en src/RackCad.Application solo WEI, WHC, IWorkspaceEditIntentOriginReader.cs y Draft/PushBackOriginByText.cs; en tests/ solo las tres clases nuevas y la linea del Skip de PushBackDraftWriteGuardTests.cs.
INV-PBT4A-02. RED y GREEN: el RED son las tres clases nuevas de tests/, el esqueleto exacto (WEI con FromReads inerte, la interfaz nueva completa y PushBackOriginByText inerte) y el Skip de CT-PBT3A-12, con WHC, RIA y NH sin cambio; el GREEN solo toca WEI, PushBackOriginByText.cs, WHC, RIA y NH. En el RED, cada filtro con ExpectRed da todos Failed por asercion (la primera linea del StackTrace bajo tests/), con Failed == Selected; el filtro de las guardas pasa y sus autopruebas fallan sobre el texto inyectado. Con una reparacion del RED (F4B-E35, RR): RedSha..RedRepairSha solo toca clases de prueba que anadio el RED y ningun archivo de src/ ni PushBackDraftWriteGuardTests.cs; el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el GREEN de arriba.
INV-PBT4A-03. La API: exactamente la de D-PBT4-02 y D-PBT4-03, con el texto literal del GREEN del paquete donde lo prescribe; la firma de RackPilotIntentAdapter.Read no cambia (EIG:139-141).
INV-PBT4A-04. Una CaptureDrawing y una LockDocument por lectura; AtOrigin no lee el dibujo (sin CaptureDrawing, bloqueo, transaccion, literal de cadena ni ex.Message) y solo llama a PushBackOriginByText.ReadAt, que hace en Draft/ las dos relecturas puras; RIA conserva un PushBackPilotIntentReader.Read(, un PushBackFullDraftReader.Read( y un new PushBackPilotIntentScan(, los tres en Read (EIG:350-364), StartTransaction 1 y LockDocument 1, y NH LockDocument 2 (G-9).
INV-PBT4A-05. El tipo del borrador del panel no cambia: Find<PushBackPilotDraftState> en las nueve lineas; WHC solo cambia en las cinco sentencias de lectura, con el texto literal de D-PBT4-03, y no declara nada nuevo.
INV-PBT4A-06. Ningun archivo de src/ fuera de Draft/, PP, UR y RIA nombra los cinco tipos del modelo (CT-PBT4A-06).
INV-PBT4A-07. Las redes: section 5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, con una unica salvedad: el Skip de CT-PBT3A-12, con su motivo literal. En CurrentSha, el git diff -U0 BaseSha..CurrentSha de PushBackDraftWriteGuardTests.cs solo lleva la linea de atributo con Skip.
INV-PBT4A-08. Las pruebas de UI: las reglas de section 5.0.
INV-PBT4A-09. Determinismo: GUID fijo, cultura invariante, "R" y orden ordinal; ni hora ni aleatoriedad.
INV-PBT4A-10. Las corridas locales: solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, si; sin copias de arboles; las corridas temporales terminan en Assert.Fail(string.Join("\n", lineas)), corren dentro de EditorDiscardPrompt.Substitute cuando abren ventanas, no entran en ningun commit y guardan TRX y log como artefactos.
INV-PBT4A-11. La evidencia: rutas relativas comprobadas en worker-handoff.json; TreeSha real o null; el Trailer, el real; el Worker no escribe en docs/.
INV-PBT4A-12. Las reglas de F4B; sin AutoCAD; la verificacion con las tres comprobaciones deterministas de F4B-E15.
INV-PBT4A-13. Las declaraciones nuevas en src/: las de la lista cerrada de PBT4A-C13 (con ReadAt), y ninguna mas, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. No se admite ningun ayudante privado, tampoco en los dos archivos nuevos: el texto literal del paquete no lo necesita.
INV-PBT4A-14. D2: fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna linea anadida muta un PushBackSystem ni un RackModuleReconciliationResult adoptado.

StopConditions (propias):
PBT4A-C01: doing the task would require writing outside the AllowedWriteScope.
PBT4A-C02: it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a .csproj or Fixtures/I64. Only one exception: the Skip of CT-PBT3A-12 in PushBackDraftWriteGuardTests.cs, with its literal reason. Any existing test that fails, outside that list, returns to the Owner.
PBT4A-C03: an observable change of RACKEDITAR appears, or of the pilot in the panel other than the declared one (the five rereads of an open draft go to the origin of the draft by its text, A-T3-8). STOP and return to the Owner.
PBT4A-C04: a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.
PBT4A-C05: a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or writing to the profile or to disk.
PBT4A-C06: the GREEN CI fails outside the new tests, reproducibly and not attributable to them.
PBT4A-C07: a fact of section 2 does not hold in a way that changes a CT or the design; or an expected line is not met in the GREEN; or a self-checking recipe (the self-tests of CT-PBT4A-07 and CT-PBT4A-08) does not exercise its path. The Worker does not adjust what is expected; except under F4B-E35 (the RED repair rule, RR): a writing error in a new test of this task, found after the RED push and before the GREEN, is repaired without this stop and without spending an attempt by one declared RED repair commit that touches only test files the RED added and changes only the when or the how of a reading or a gesture the package already prescribes, and no expected line, literal or oracle of the package or the contract, with the full RED criterion rerun on RedRepairSha; a second writing error in the same attempt, a repair that needs an expected line changed, or a literal line the package fixes for the repaired class (an expected line or a literal) that does not stay byte for byte in it, is this stop.
PBT4A-C08: a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner
PBT4A-C09: a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.
PBT4A-C10: doing the task would require what the scope of PB-T4a excludes: changing the type of the draft of the panel; beginning a draft from the full read; the window, Editar todo or Conservar mi borrador in two steps; the rule of the scale; creating views; touching the classic editor or RACKEDITAR; opening AUTH-13; or changing the signature of RackPilotIntentAdapter.Read.
PBT4A-C11: a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner ([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, and nothing else) -> substitute PBT4-NC2-EXACT-WHITELIST-F4B-01, the version 2 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py, SHA-256 c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45; owner-pbt3a-nc2-decision.md, "Aceptar y ampliar el sustituto"), whose property 8 admits only the closed list of the exact literals of the evidence entry of the byte-by-byte comparison, one per package (PB-T2 and PB-T3), with its reviewed text unchanged and run with --task PBT4 in the F4B-E21 mode with the injection note and the raw output of this task's accepted planning, recorded as nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B; if inject_opaque.py changed since that run, the substitute is reviewed before it is reused; (2) nc1 with the signature PD-F4B-01, the false SHA cascade (F4B-E40: owner-pbt2c-nc1-decision.md, its conditions (a) to (f): the mutation only changes the CurrentSha of the handoff to a SHA that does not exist in the repository; EXECUTION_BLOCKED / STOP with S-04 among the stops; Handoff is the first check in fail; the failing checks are a subset of Handoff, Identity, Remote, Scope, Ci and Tests that always includes Handoff and Identity, and all the others pass as in the real verification; each failing evidence cites the false SHA and none attributes the failure to another cause; the Identity evidence also writes the real HEAD), of which the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265; as applied in F4, f4-dispositions.md:1657-1660) is a particular case -> substitute PBT3x-NC1-CASCADE-SIGNAL-01 (F4B-E40 and F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py, SHA-256 41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40, reviewed APTO, with its declared limits), with its reviewed text unchanged and run with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md) and, for PD-F4B-01, its conditions (a) to (f); the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or Trailer also in fail, any nc1 that does not meet PD-F4B-01 - the one-off acceptances of nc1 in PB-TC (F4B-E08), in PB-TC2 attempt 1 (F4B-E12), in PB-T2a (F4B-E34), in PB-T2b (F4B-E38) and in PB-T2c (F4B-E40) do not extend -, an nc2 that the version 2 of the substitute does not give PASS - the disposition of the Owner on the nc2 of PB-T3a (F4B-E43) does not extend -, a substitute that does not give PASS, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner before any substitute or acceptance; no one-off acceptance extends to PB-T4a
PBT4A-C12: a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)
PBT4A-C13: doing the task would require touching a file of src/ other than WEI, WHC, RIA, NH, IWorkspaceEditIntentOriginReader.cs and Draft/PushBackOriginByText.cs; or adding a declaration of any visibility (type, constructor, method, property, field, constant, enum member or nested type; an overload is one more) whose name is not in this closed list: in WorkspaceEditIntent.cs, the properties FullRead and Catalog of EditIntentReadResult and its method FromReads; in IWorkspaceEditIntentOriginReader.cs, the interface IWorkspaceEditIntentOriginReader and its method ReadIntentAtOrigin; in Draft/PushBackOriginByText.cs, the class PushBackOriginByText and its methods Choose, AtKey and ReadAt; in RackPilotIntentAdapter.cs, the second constructor of RackPilotIntentReadResult, its properties Full and Input, and the method AtOrigin of RackPilotIntentAdapter; in WorkspaceNavigationHost.cs, the method ReadIntentAtOrigin of the nested reader. Two changes of existing members are prescribed and are not new declarations: the private constructor of EditIntentReadResult gains two parameters, and ReadOnActiveDocument gains the parameter originText. In WorkspaceHostCore.cs, no new declaration: only the inline statement of D-PBT4-03 in five bodies (the pattern variable originReader is not a member). The Worker stops before adding it.
PBT4A-C14: a design of the corpus changes its admission or its lock (the pilot reader, the full reader, AUTH-13 or the parked tail).
PBT4A-C16: an existing test outside the closed list of substitutions of PB-T4a (CT-PBT3A-12) fails, including one that reads the text in a way only seen when it runs. It returns to the Owner (owner-pbt4-decisions.md, question 4).
PBT4A-C17: (F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." The Worker chooses none of the three exits: the Owner does
PBT4A-C18: a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in the attempt; it touches a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope; it changes an expected line, literal or oracle, or anything other than the when or the how of a reading or a gesture the package already prescribes (RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP.

ExpectedEvidence:
[0] antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;
[1] el commit RED y su push antes del GREEN; con una reparacion del RED (F4B-E35, RR-4; V43-04), ademas, el commit "RED repair" y su push antes del GREEN, con su CI, registrada como la del RED reparado, y los TRX de cada filtro con ExpectRed sobre RedRepairSha;
[2] la CI del RED como en section 6.0 (tests en failure, ui-tests y build-ui en success, build-plugin omitido, sin cuelgue); la del RED reparado, si lo hay, da lo mismo;
[3] la tabla "CT -> la asercion que falla en el RED" y las autopruebas de CT-PBT4A-07 y CT-PBT4A-08;
[4] el GREEN y su CI 4/4;
[5] en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED, sobre RedRepairSha si hay reparacion, y sobre el GREEN, y tres corridas en verde de los filtros con ExpectRed sobre el GREEN;
[6] la entrega con el mapa CT -> metodo, las rutas relativas comprobadas y el Trailer real de los commits RED, "RED repair" (si lo hay) y GREEN; con una reparacion del RED, ademas, RedRepairSha, la prueba y la linea corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y RedSha, que sigue siendo el commit RED original (F4B-E35, RR-5);
[7] la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3;
[8] con una reparacion del RED, la verificacion la lee como parte de la entrega (F4B-E35, RR-7; section 6.0; V43-05 y V44-04, CA-T3a M-01): n.o 10 (Tests): el RED se acredita con RedSha y con la CI del RED original, y sus TRX, con los filtros con ExpectRed entre los fallidos. La clausula "git diff --name-only RedSha CurrentSha no toca RT U ChainRedFiles" se comprueba desde RedRepairSha: git diff --name-only RedRepairSha CurrentSha. n.o 11 (Trailer): el trailer va tambien en el commit "RED repair". Lo que el Controller comprueba de RR (si algo falla, es C18): RR-1, un solo commit "RED repair", hijo directo de RedSha y anterior al GREEN; RR-2, git diff --name-only RedSha RedRepairSha dentro de las clases de prueba que anadio el RED (git diff --name-only --diff-filter=A BaseSha RedSha -- tests/); RR-4, la CI de RedRepairSha, registrada como la del RED reparado, y los TRX de los filtros con ExpectRed sobre el; RR-5, la declaracion de la entrega, con el diff, y RedSha, el commit RED original; RR-6, con el tramo RedSha..RedRepairSha. En PB-T4a, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto) ni PushBackDraftWriteGuardTests.cs (el Skip de CT-PBT3A-12). La ayuda mecanica de RR-3: es parada C07, una violacion cierta de RR-3, que un literal de la lista cerrada de la clase reparada (section 6.0 del paquete, la tabla de RR-3) cambie entre RedSha y RedRepairSha: una linea o un fragmento, si no aparece como subcadena exacta el mismo numero de veces; un valor de gesto, si no aparece como token el mismo numero de veces. Indicios para la revision, no parada: una cadena <campo>= anadida o quitada en git diff -U0 RedSha RedRepairSha, o un cambio en el primer argumento de un Assert.Equal. Pueden dar falsos positivos, como una linea que solo se mueve de sitio, asi que van a Findings y a la revision, con la declaracion de RR-5.
[9] la CI del RED con tests en failure, ui-tests y build-ui en success y build-plugin omitido;
[10] la del GREEN 4/4, con el Plugin compilado;
[11] el censo de declaraciones de WEI, WHC, RIA y NH, y de los dos archivos nuevos, en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es una declaracion mas): la diferencia es la lista cerrada de PBT4A-C13;
[12] el Trailer real;
[13] la entrada "## PB-T4a" de superseding-characterization.md, escrita por la sesion en el commit de emision, con el Skip de CT-PBT3A-12, su motivo literal y CT-PBT4A-06, que la sustituye; su tabla de ejecucion se completa al cerrar.
[14] Linea base de la CI: la del cierre de PB-T3c (F4B-E48): run 44757675821 sobre el GREEN 938a35f0, Core 19 129 resultados, con 19 123 Passed y 6 Skipped, los de D-1, y UI 2 451, con 2 430 Passed y 21 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4a anade ocho [Fact] y un Skip de un [Fact], sin ninguna Theory, asi que hay una sola rama. El RED: Core 19 137 resultados, con 19 125 Passed, 5 Failed (CT-PBT4A-01 a 04 y 07) y 7 Skipped. El GREEN: Core 19 137, con 19 130 Passed, 0 Failed y 7 Skipped. UI, en los dos: 2 451, con 2 430 Passed y 21 Skipped.
```

**`RequiredTests`** (38): las dos clases nuevas, y los 36 filtros del contrato de PB-T3c, **todos con `ExpectRed` false** (también `PushBackViewEraseGuardTests`, que era true en PB-T3c) y su `MinSelected`, salvo **`PushBackDraftWriteGuardTests`, que baja de 3 a 2** por el `Skip` de CT-PBT3A-12 (V41-08), como bajaron los de F4 con los `Skip` de D-1:

| # | Project | Filter | MinSelected | ExpectRed |
|---|---|---|---|---|
| 1 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests` | **5** | true |
| 2 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests` | **3** | false |
| 3 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackViewEraseGuardTests` | 4 | false |
| 4 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 | false |
| 5 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateGuardTests` | 86 | false |
| 6 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | 123 | false |
| 7 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackSharedReferenceGuardTests` | 2 | false |
| 8 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 | false |
| 9 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullWrite` | 13 | false |
| 10 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 | false |
| 11 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathGuardTests` | 2 | false |
| 12 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | 3261 | false |
| 13 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullDraft` | 10 | false |
| 14 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests` | **2** | false |
| 15 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 547 | false |
| 16 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditableState` | 13 | false |
| 17 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate` | 1 | false |
| 18 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 | false |
| 19 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 | false |
| 20 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 | false |
| 21 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false |
| 22 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 10 | false |
| 23 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 16 | false |
| 24 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 | false |
| 25 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet` | 14 | false |
| 26 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 14 | false |
| 27 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 | false |
| 28 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.I58F1` | 10 | false |
| 29 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 9 | false |
| 30 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.NamespaceFolderGuardTests` | 7 | false |
| 31 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftStatePortTests` | 3 | false |
| 32 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 | false |
| 33 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 | false |
| 34 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 | false |
| 35 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 | false |
| 36 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 4 | false |
| 37 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 | false |
| 38 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 | false |

- **Nota (no va al contrato).** Ningún filtro heredado selecciona las clases nuevas: `RackCad.Tests.PushBackFullDraft` y `RackCad.Tests.PushBackFullWrite` no son prefijo de `RackCad.Tests.PushBackFullRead…`, y `RackCad.Tests.Workspace` no las selecciona porque su espacio de nombres es `RackCad.Tests`. Los `MinSelected` heredados no bajan: PB-T4a no salta nada más.

#### 6.1.2 Lo que necesita el generador de PB-T4a (V41-18)

| Pieza | Dónde, en este paquete | Lo que comprueba el generador |
|---|---|---|
| **`TaskId`** | §6.1.1 | `F4B-PBT4-FULL-READ-PATH`, 23 caracteres, ≤ 26 |
| **`Objective`** | §6.1.1 | ASCII, byte a byte |
| **`Authorities`** | Abajo | Cada una con su ruta en `AuthorityRevision` |
| **`AllowedWriteScope`** | §6.1 | Las 10 rutas, en ese orden |
| **`ForbiddenWriteScope`** | §6.0 | El método de [PKC-C], con `PushBackDraftWriteGuardTests.cs` fuera de lo prohibido |
| **`UNIT_CHANGE`** | §6.0 y §6.1 | WEI, WHC, RIA y NH |
| **`Invariants`** | §6.1.1 | INV-PBT4A-01 a 06 y los comunes 07 a 14, literales |
| **`StopConditions`** | §6.0 y §6.1.1 | Las 17 estándar del contrato de PB-T3c, y PBT4A-C01 a C14, C16, C17 y C18 (sin C15): 34. **C11** es `PBT3C-C11` con `PBT3C` → `PBT4` y «PB-T3c» → «PB-T4a», y acaba en «no one-off acceptance extends to PB-T4a» |
| **`RequiredTests`** | §6.1.1 | 38, con `MinSelected` y `ExpectRed`; `PushBackDraftWriteGuardTests` en 2 |
| **`ExpectedEvidence`** | §6.1.1 | 15; `[0]` byte a byte el de PB-T3c; `[14]` con los totales de la rama única: RED 19 137 = 19 125 + 5 + 7, GREEN 19 137 = 19 130 + 0 + 7, UI 2 451 = 2 430 + 21. El generador la compara con la cuenta de CT (8), de `[Fact]` (8), de `Skip` (1) y de `ExpectRed` (5) |
| **`EligibleCells`, `RoutingEnforcement`, `CorrectionsAuthorized`** | §6.0 | Las de PB-T3c; `required`; true |
| **El esqueleto y su parche** | §6.1 | WEI, la interfaz, `PushBackOriginByText` inerte y la línea del `Skip`, literal; la copia compila y da Core **19 129 = 19 122 + 7** |
| **El registro declarado** | §6.1.1 `[13]` | La entrada «## PB-T4a» con el `Skip`, el motivo literal y CT-PBT4A-06 |
| **La lista de PB-T4a** | `pbt4-substitution-lists.md` | 1: `CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes` |

**Las autoridades de PB-T4a:**
- [OD] (la fila de PB-T4, D-PB-01 y D-PB-05) y [P] (OB-01.1 a 6, OB-03.2, OB-05.1 a 5, R-PB-10, §7.3 y §7.4);
- [P2] §10 y [P3] §10 (A-T3-1 a A-T3-11; A-T3-8 es el que aplica PB-T4a);
- [PT1] (A-5, A-9, A-10 y R-T1-17);
- **las decisiones del Owner sobre PB-T4** (`owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`, con su número F4B-E al custodiarlos);
- F4B-E48 (el cierre de PB-T3c), F4B-E46 (el total exacto de cada rama), F4B-E45 (las sobrecargas y la reflexión), F4B-E43 y F4B-E40 (los sustitutos de nc2 y nc1), F4B-E37 (la Snapshot K), F4B-E35 pregunta 4 (RR), F4B-E29 (D-1), F4B-E25 (`UNIT_CHANGE`), F4B-E21 y F4B-E22, F4B-E15, F4B-E11 (D2) y F4B-E03;
- este paquete (v4.3), con su comprobación acotada de PB-T4a (CA-T4a).

**Lo que cambia para el generador respecto de la v4.1** (V42-02): solo el texto de PB-T4a que toca B-08. El `Objective`, el alcance (10 rutas), `UNIT_CHANGE`, los `RequiredTests` (38), las paradas (34), los totales y `ExpectedEvidence` no cambian. Cambian:
- el esqueleto: `PushBackOriginByText` lleva también `ReadAt` inerte (`=> null;`) y el `using RackCad.Application.Catalogs;`;
- PBT4A-C13 añade `ReadAt` a los métodos de `PushBackOriginByText`;
- INV-PBT4A-04 dice que `AtOrigin` solo llama a `ReadAt` y que RIA conserva una lectura de cada lector y un escaneo, en `Read`;
- las líneas literales de CT-PBT4A-04 y CT-PBT4A-07 (§5.1), y la fila de RR-3 de `PushBackFullReadPathTests` con ellas;
- el `Objective` cita «el paquete del contrato de PB-T4 v4.3» (en la v4.2, v4.2; en la v4.1, v4.1); lo demás del `Objective`, igual;
- **en la v4.3** (CA-T4a I-01): la firma literal de `Choose` en el esqueleto y en el GREEN, `public static string Choose(…)`, y el molde de `AtKey`, `pair => pair.Value`; el sha256 del parche del esqueleto cambia con ellos;
- el registro de autoridades nombra también `owner-pbt4b-retire-list-decisions.md`, que no cambia PB-T4a.

### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL` (entero; V44-09; V45-01, V45-02, V45-10; V46-01, V46-02; V47-01, V47-02; V48-01 a V48-05; V49-01, V49-06)

**Se emite por la autorización condicional** (`owner-pbt4-decisions.md`, pregunta 4) **con la lista exacta de 478** que el Owner tomó como la que vio (las 470 de `owner-pbt4b-retire-list-decisions.md`, pregunta 2, las cinco de `owner-pbt4b-breaks-decision.md`, las dos de `owner-pbt4b-list-477-decision.md` y la de `owner-pbt4b-list-478-decision.md`; `pbt4b-substitution-list-478.tsv`, sha256 `8b0fcd0c…9e8b`), si se cumple el resto de la condición, **sobre el cierre de PB-T4a** (F4B-E51, `78a7cef3`). **No retira nada:** la retirada es de PB-T4d.

- **Objective** (ASCII): el de §6.2.1.
- **`AllowedWriteScope`** (59), ruta a ruta:
  - **`src/`, existentes (`UNIT_CHANGE`, 6):** `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` (WHC), `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` (WEI), `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs` (FVM), `src/RackCad.UI/Workspace/WorkspacePanelView.xaml` (PV), `src/RackCad.UI/Controls/NumericFieldValidation.cs` y `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs` (RIA; D-S4);
  - **`src/`, nuevos (4):** `src/RackCad.Application/Formatting/NumericTextRule.cs`, `src/RackCad.Application/Systems/PushBack/Draft/PushBackAnnotationScaleRule.cs`, `src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs` y `src/RackCad.Application/Systems/PushBack/Draft/PushBackKeepMinePresentation.cs`;
  - **las clases de las CT (6):** `tests/RackCad.Tests/PushBackFullPanelTests.cs`, `tests/RackCad.Tests/PushBackFullPanelGuardTests.cs`, `tests/RackCad.Tests/PushBackFullPanelCensusGuardTests.cs`, `tests/RackCad.Tests/PushBackPanelDraftGuardTests.cs`, `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs` y `tests/RackCad.UI.Tests/PushBackFullPanelFieldTests.cs`;
  - **el kit (1):** `tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs`;
  - **las copias (21), nuevas, que genera el guion:** en `tests/RackCad.Tests/Workspace/`, `WorkspaceLifecycleHostFull.cs`, `WorkspacePilotPrepareFull.cs`, `WorkspaceEditIntentHostFull.cs`, `WorkspacePilotEditHostFull.cs`, `WorkspaceCommandAdvisorFull.cs`, `WorkspacePilotUpdateRequestFull.cs`, `WorkspacePilotUpdateHostFull.cs`, `WorkspacePilotReconciliationHostFull.cs`, `WorkspaceAssociativeBookkeepingFull.cs`, `WorkspaceHostHardeningGuardFull.cs`, `WorkspacePilotUpdateIntegrationGuardFull.cs`, `WorkspaceEditIntentGuardFull.cs`, `WorkspacePilotEditGuardFull.cs`, `WorkspacePilotReconciliationGuardFull.cs`, `WorkspaceLifecycleGuardFull.cs` y `WorkspacePilotUpdateGuardFull.cs`; en `tests/RackCad.Tests/`, `PushBackWritePathGuardFull.cs`, `PushBackFullReadPathFull.cs` y `PushBackFullReadGuardFull.cs`; y en `tests/RackCad.UI.Tests/Workspace/`, `WorkspacePilotReconciliationFull.cs` y `WorkspacePilotReconciliationViewFull.cs`;
  - **solo para los `Skip` (21):** los 21 archivos de origen: las 9 clases de C-0, `PushBackWritePathGuardTests.cs`, IG, EIG, PEG, RG, WLG, WPUG, WHHG (`tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs`), los dos de la interfaz, `tests/RackCad.Tests/PushBackFullReadPathTests.cs` (CT-PBT4A-03) y `tests/RackCad.Tests/PushBackFullReadGuardTests.cs` (CT-PBT4A-05 y 06).
  - **Ningún archivo de `Pilot/`, ni WCo, ni `WorkspacePilotSupport.cs`, ni `WorkspacePanelView.xaml.cs`, ni NH, ni `Draft/PushBackOriginByText.cs`, ni `PushBackFullReadSupport.cs`:** el GREEN literal no los cambia.
- **`UNIT_CHANGE`:** WHC, WEI, FVM, PV, `NumericFieldValidation.cs` y RIA (F4B-E25).
- **`ForbiddenWriteScope`:** el método de [PKC-C] con el precedente de PB-T3c y PB-T4a en `tests/` (la lista explícita del contrato anterior, menos las rutas de `tests/` de este alcance; un prefijo prohibido que cubre una de ellas se sustituye por la lista de sus archivos), más las clases de PB-T4a tras su cierre, salvo las dos de este alcance (FRP y FRG). **El número lo da la regla del generador sobre el alcance final de la v4.5 en `78a7cef3`** (`<medido por el generador>`): con el alcance de la v4.4 le salían **407** entradas en `78a7cef3` (405 en `5aaacce3`: el GREEN de PB-T4a añadió dos archivos a `src/`, que parten dos entradas más), y RIA, FRP, FRG y las dos copias nuevas entran ahora en el alcance. La v4.4 tomaba 452, que venía del alcance provisional de la fase 1.
- **El RED** (un commit, con su push antes del GREEN):
  - **el esqueleto exacto de `src/`** (`green_pbt4b_v45.skeleton`, el de la v4.4): WEI con su API entera (el estado `Presented` y lo que lleva `EditIntentOutcome`; el anfitrión del RED no la usa); FVM con `KeepMinePresentationText` y `ShowKeepMinePresentation` inertes (`string.Empty` y `false`); `NumericTextRule.cs` con su tipo, su enumerado y sus constantes, y `Check` inerte (`NotANumber` sin mensaje); `PushBackAnnotationScaleRule.cs` con sus constantes y `Validate` inerte (`Invalid` vacío); `PushBackFullFieldAdapter.cs` con sus cinco constantes, `Validate` inerte (`Invalid(UnknownFieldMessage)`) y `Apply` que devuelve el estado; `PushBackKeepMinePresentation.cs` con sus propiedades y `Of` y `Read` inertes (sin líneas). **WHC, RIA, PV y `NumericFieldValidation.cs` no cambian** (M-29);
  - **los 478 `Skip`** literales, en los 21 archivos de origen, y nada más en esos archivos;
  - **el kit,** byte a byte el custodiado (`pbt4b-kit-WorkspaceFullReadKit-v45.cs`);
  - **las 21 copias,** byte a byte las que genera `make_copies_pbt4b_v49.py` con la partición cerrada de §6.2.1 (su sha256, `CopySha`, va en el contrato);
  - **las seis clases de las CT.**
  - **La herramienta:** `python -I apply_red_pbt4b_v49.py <dir-copia> --repo <checkout> --rev 78a7cef3 --partition <pbt4b-copy-partition.tsv>` escribe exactamente eso salvo las CT, tras comprobar que la copia es de la revisión; el Worker puede usarla sobre su worktree o copiar sus salidas. Imprime el sha256 de cada archivo.
- **El GREEN:** solo `src/`, los diez archivos del texto literal de `green_pbt4b_v45.py` (los nueve de la v4.4 y RIA; `apply_green_pbt4b_v45.py` lo escribe en una copia). `tests/` no cambia después del RED (salvo una reparación del RED, solo en las clases de las CT).
- **`RequiredTests`:** la tabla de §6.2.1.
- **Invariantes:** INV-PBT4B-01 a 06 (§6.2.1) y los comunes 07 a 14.
- **`StopConditions`:** las 17 estándar y PBT4B-C01 a C14, C16, C17, C18 y C20 (§6.2.1): 35.
- **`ExpectedEvidence`:** §6.2.1.
- **La línea base:** el cierre de PB-T4a (F4B-E51, `78a7cef3`), con la CI de su GREEN (`44814418683` sobre `faef7020`).

#### 6.2.1 Los textos literales del contrato de PB-T4b

Para la comparación del generador, en ASCII, campo a campo. Lo que dependía del cierre de PB-T4a está medido en `78a7cef3`; lo que depende de ejecutar va como `<medido en la precondicion 6>`, con su regla: el generador lo sustituye al emitir.

```text
TaskId: F4B-PBT4-FULL-DRAFT-PANEL

Objective:
Pasar el borrador del panel del Workspace de Push Back al modelo completo de PB-T3 con la lectura completa de PB-T4a, sin traducir estados: el borrador nace de la lectura completa y el anfitrion lo busca como completo, con la admision y el diagnostico de esa lectura; OB-03.2 (los racks A/B se editan en el panel; la cola y AUTH-13 siguen rechazados); la regla unica de la escala en Application (la logica de la validacion numerica pasa de la UI a NumericTextRule y la UI delega en ella sin cambiar su comportamiento; el adaptador del campo usa PushBackAnnotationScaleRule por defecto); Actualizar con Precheck y ExecuteFull; y Conservar mi borrador con dos Si: el primero llama a TryKeepMine sin testigo y presenta los cambios por grupos, clasificados por una funcion pura de Draft, con el testigo de lo releido; el segundo conserva con KeepOn solo si ese testigo sigue valiendo; y una sola materializacion del modelo completo por lectura (el adaptador del Plugin difiere la del Primary, y la relectura con el origen encontrado no la hace). El codigo del piloto queda sin uso, sin retirarlo (lo retira PB-T4d), segun el paquete del contrato de PB-T4 v4.9, las decisiones del Owner sobre PB-T4 (sus siete tandas, con las del coste de la lectura de PB-T4a y de PB-T4b), la lista cerrada CT-PBT4B-01..21 y la lista exacta de 478 sustituciones que el Owner tomo como la que vio: cada una con su Skip de motivo literal y su copia o su equivalente, literales del paquete, sin editar ni borrar ninguna prueba ni ningun ayudante. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. Ni Editar todo, ni RACKEDITAR, ni la ventana; no se ejecuta AutoCAD.

Invariants:
INV-PBT4B-01. Alcance: dentro de las 59 rutas. git diff BaseSha..CurrentSha -- src/RackCad.Domain vacio; en src/RackCad.Plugin solo Views/RackPilotIntentAdapter.cs; en src/RackCad.Application solo WHC, WEI, Formatting/NumericTextRule.cs y los tres archivos nuevos de Systems/PushBack/Draft/; en src/RackCad.UI solo FVM, Workspace/WorkspacePanelView.xaml y Controls/NumericFieldValidation.cs; ningun archivo de Pilot/; en tests/ solo las seis clases de las CT, el kit, las 21 copias y las lineas de atributo con Skip de los 21 archivos de la lista.
INV-PBT4B-02. RED y GREEN: el RED son las seis clases de las CT, el kit y las 21 copias byte a byte (su sha256, CopySha), los 478 Skip y el esqueleto exacto de src/ (WEI con su API, FVM con sus dos propiedades inertes y los cuatro archivos nuevos inertes; WHC, RIA, PV y NumericFieldValidation.cs sin cambio); el GREEN solo toca src/, con el texto literal del paquete. En el RED, cada filtro con ExpectRed da todos Failed por asercion (la primera linea del StackTrace bajo tests/), con Failed == Selected; los filtros con ExpectRed false pasan y sus autopruebas fallan sobre el texto inyectado. Con una reparacion del RED (F4B-E35, RR): RedSha..RedRepairSha solo toca las seis clases de las CT y ningun archivo de src/, ni el kit, ni las copias, ni los 21 archivos de los Skip; el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el GREEN de arriba.
INV-PBT4B-03. La API: exactamente la de D-PBT4-04 a D-PBT4-09 y D-PBT4-21, con el texto literal del GREEN del paquete (green_pbt4b_v45.py); Conservar mi borrador por TryKeepMine, sin miembro nuevo del puerto: sin testigo, o con uno que ya no vale, presenta (Presented) y no cambia el borrador; con el vigente, KeepOn; RequestKeepMine no llama al puerto y FVM nombra TryKeepMine una sola vez, dentro de ConfirmChoice.
INV-PBT4B-04. El borrador del panel es el completo: Find<PushBackFullDraftState> en las nueve lineas, Begin con la lectura completa, la admision y el diagnostico de la lectura completa, Actualizar con PushBackPilotUpdateRun.Precheck y ExecuteFull; WHC no nombra pilot.Read, KeepPilotEditsOn, PushBackPilotFieldAdapter, PushBackPilotStateComparer ni PushBackPilotUpdateRun.Execute(, y lee el testigo de la vista previa con var pilot = read.PilotRead; en las cuatro sentencias que lo usan; nada traduce un estado del piloto a uno completo. Una sola materializacion del modelo completo por lectura: Read de RIA difiere la del Primary (System.Lazy, y Full es _full?.Value) y AtOrigin no la pide; NH, AtOrigin y ReadAt no cambian (CT-PBT4B-21).
INV-PBT4B-05. La regla unica de la escala: la logica de NumericFieldValidation.Validate esta en NumericTextRule.Check, que la UI llama con la misma API publica y el mismo resultado; PushBackAnnotationScaleRule la usa con Minimum 0 exclusivo; el adaptador del campo de WHC es PushBackFullFieldAdapter, estatico, y nada asigna una regla.
INV-PBT4B-06. Ningun archivo de src/ fuera de Draft/, PP, UR, RIA y WHC nombra los cinco tipos del modelo (CT-PBT4B-13); los tres archivos nuevos de Draft/ son puros y sin fichas de G-L1 (CT-PBT4B-19); el codigo del piloto queda sin uso y no se toca (lo retira PB-T4d).
INV-PBT4B-07. Las redes: section 5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, salvo las 478 de la lista cerrada (pbt4b-substitution-list-478.tsv, sha256 8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b), cada una con su Skip de motivo literal. En CurrentSha, el git diff -U0 BaseSha..CurrentSha de los 21 archivos de pruebas existentes solo lleva las lineas de atributo con Skip.
INV-PBT4B-08. Las pruebas de UI: las reglas de section 5.0.
INV-PBT4B-09. Determinismo: GUID fijo, cultura invariante, "R" y orden ordinal; ni hora ni aleatoriedad.
INV-PBT4B-10. Las corridas locales: solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, si; sin copias de arboles; las corridas temporales terminan en Assert.Fail(string.Join("\n", lineas)), corren dentro de EditorDiscardPrompt.Substitute cuando abren ventanas, no entran en ningun commit y guardan TRX y log como artefactos.
INV-PBT4B-11. La evidencia: rutas relativas comprobadas en worker-handoff.json; TreeSha real o null; el Trailer, el real; el Worker no escribe en docs/.
INV-PBT4B-12. Las reglas de F4B; sin AutoCAD; la verificacion con las tres comprobaciones deterministas de F4B-E15.
INV-PBT4B-13. Las declaraciones nuevas en src/: las de la lista cerrada de PBT4B-C13, y ninguna mas, por nombre y acotada a los archivos que tocan, sobrecargas incluidas.
INV-PBT4B-14. D2: fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna linea anadida muta un PushBackSystem ni un RackModuleReconciliationResult adoptado.

StopConditions (propias):
PBT4B-C01: doing the task would require writing outside the AllowedWriteScope.
PBT4B-C02: it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a .csproj or Fixtures/I64. No exception outside the closed list of PB-T4b: the Skip of each of the 478 tests of pbt4b-substitution-list-478.tsv, with its literal reason, in its attribute line, and nothing else in those 21 files. Any existing test that fails, outside that list, returns to the Owner.
PBT4B-C03: an observable change of RACKEDITAR appears, or of the panel other than the declared ones (the draft of the panel is the full one; two-sided racks are edited in the panel; Actualizar writes with the full model; Conservar mi borrador asks for two Yes: the first one shows the changes by groups above Yes and No, the second one keeps). STOP and return to the Owner.
PBT4B-C04: a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.
PBT4B-C05: a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or writing to the profile or to disk.
PBT4B-C06: the GREEN CI fails outside the new tests, reproducibly and not attributable to them.
PBT4B-C07: a fact of section 2 does not hold in a way that changes a CT or the design; or an expected line is not met in the GREEN; or a self-checking recipe (the self-tests of CT-PBT4B-12, CT-PBT4B-13, CT-PBT4B-15, CT-PBT4B-19, CT-PBT4B-20 and CT-PBT4B-21) does not exercise its path. The Worker does not adjust what is expected; except under F4B-E35 (the RED repair rule, RR): a writing error in a new CT of this task (one of its six classes; never the kit nor the copies, whose text is the package one), found after the RED push and before the GREEN, is repaired without this stop and without spending an attempt by one declared RED repair commit that touches only test files the RED added and changes only the when or the how of a reading or a gesture the package already prescribes, and no expected line, literal or oracle of the package or the contract, with the full RED criterion rerun on RedRepairSha; a second writing error in the same attempt, a repair that needs an expected line changed, or a literal line the package fixes for the repaired class (an expected line or a literal) that does not stay byte for byte in it, is this stop. A copy or a hand-written equivalent of the list that fails in the GREEN is this stop too: the session issues a package version with its hand-written text (D-S1).
PBT4B-C08: a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner
PBT4B-C09: a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.
PBT4B-C10: doing the task would require what the scope of PB-T4b excludes: retiring or editing the code of the pilot (Pilot/); Editar todo or the window; creating views; touching the classic editor or RACKEDITAR; opening AUTH-13; a new member of the edit port of the panel; translating a state of the pilot into a full one; or, for the one materialization per read (D-S4), touching NH, AtOrigin or ReadAt, or reading the drawing again.
PBT4B-C11: a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner ([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, and nothing else) -> substitute PBT4-NC2-EXACT-WHITELIST-F4B-01, the version 2 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py, SHA-256 c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45; owner-pbt3a-nc2-decision.md, "Aceptar y ampliar el sustituto"), whose property 8 admits only the closed list of the exact literals of the evidence entry of the byte-by-byte comparison, one per package (PB-T2 and PB-T3), with its reviewed text unchanged and run with --task PBT4 in the F4B-E21 mode with the injection note and the raw output of this task's accepted planning, recorded as nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B; if inject_opaque.py changed since that run, the substitute is reviewed before it is reused; (2) nc1 with the signature PD-F4B-01, the false SHA cascade (F4B-E40: owner-pbt2c-nc1-decision.md, its conditions (a) to (f): the mutation only changes the CurrentSha of the handoff to a SHA that does not exist in the repository; EXECUTION_BLOCKED / STOP with S-04 among the stops; Handoff is the first check in fail; the failing checks are a subset of Handoff, Identity, Remote, Scope, Ci and Tests that always includes Handoff and Identity, and all the others pass as in the real verification; each failing evidence cites the false SHA and none attributes the failure to another cause; the Identity evidence also writes the real HEAD), of which the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265; as applied in F4, f4-dispositions.md:1657-1660) is a particular case -> substitute PBT3x-NC1-CASCADE-SIGNAL-01 (F4B-E40 and F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py, SHA-256 41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40, reviewed APTO, with its declared limits), with its reviewed text unchanged and run with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md) and, for PD-F4B-01, its conditions (a) to (f); the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or Trailer also in fail, any nc1 that does not meet PD-F4B-01 - the one-off acceptances of nc1 in PB-TC (F4B-E08), in PB-TC2 attempt 1 (F4B-E12), in PB-T2a (F4B-E34), in PB-T2b (F4B-E38) and in PB-T2c (F4B-E40) do not extend -, an nc2 that the version 2 of the substitute does not give PASS - the disposition of the Owner on the nc2 of PB-T3a (F4B-E43) does not extend -, a substitute that does not give PASS, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner before any substitute or acceptance; no one-off acceptance extends to PB-T4b
PBT4B-C12: a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)
PBT4B-C13: doing the task would require touching a file of src/ other than WHC, WEI, FVM, PV, NumericFieldValidation.cs, RIA (src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs), Formatting/NumericTextRule.cs and the three new files of Draft/; or adding a declaration of any visibility (type, constructor, method, property, field, constant, enum member, nested type or x:Name; an overload is one more) whose name is not in this closed list: in WorkspaceEditIntent.cs, the enum member Presented of EditIntentStatus and, in EditIntentOutcome, a second private constructor, the private field _presentationLines, the properties PresentationLines and KeepWitness and the method Presented; in WorkspaceHostCore.cs, the private constant FullReadMissingReason; in WorkspacePilotFieldViewModel.cs, the private fields _keepWitness and _keepLines and the properties KeepMinePresentationText and ShowKeepMinePresentation; in WorkspacePanelView.xaml, the TextBlock BrowserFieldKeepMinePresentationText; in RackPilotIntentAdapter.cs, in RackPilotIntentReadResult, a second internal constructor (PushBackPilotIntentResult pilot, System.Lazy<PushBackFullIntentResult> full, PushBackPilotIntentScan input) and the private readonly field _full; in NumericTextRule.cs, the enum NumericTextStatus with its six members (Valid, EmptyOptional, EmptyRequired, NotANumber, BelowMinimum, AboveMaximum), the struct NumericTextCheck with its internal constructor and its properties Status, Value, Message and IsValid, and the class NumericTextRule with its constants EmptyRequiredMessage and NotANumberMessage, its method Check and its private methods MinimumMessage, MaximumMessage and Format; in PushBackAnnotationScaleRule.cs, the class with its constants Minimum and MinimumInclusive and its method Validate; in PushBackFullFieldAdapter.cs, the class with its constants AnnotationScaleKey, EmptyMessage, NotANumberMessage, NotPositiveMessage and UnknownFieldMessage and its methods Validate and Apply; in PushBackKeepMinePresentation.cs, the class with its private constructor, its properties Lines, IsEmpty, OriginText, Diagnostic and IsAdmitted, its methods Of and Read and its private methods AddGroups, Join and TextOf. Prescribed changes that are not new declarations: the existing private constructor of EditIntentOutcome assigns the two new members; the existing constructor (pilot, full, input) of RackPilotIntentReadResult assigns _full = new System.Lazy<PushBackFullIntentResult>(full), its property Full becomes => _full?.Value, and the success line of Read passes new System.Lazy<PushBackFullIntentResult>(() => PushBackFullDraftReader.Read(input, catalog)); NumericFieldValidation.Validate delegates and its private methods MinimumMessage, MaximumMessage and Format move to NumericTextRule; WHC changes the bodies of the six reading operations, TryUpdatePilot, AnswerOfAClosedThatWasNotAccepted, FindPilotDraft, ViewOfPilotDraft, SetPendingText, GetRacksAtRisk and MarkPilotDraftsPossiblyStale, and the type of its static adapter; FVM changes ConfirmChoice, RemoveChoice, PresentChoice and RaiseAll. The Worker stops before adding anything else.
PBT4B-C14: a design of the corpus changes its admission or its lock (the pilot reader, the full reader, AUTH-13 or the parked tail).
PBT4B-C16: an existing test outside the closed list of substitutions of PB-T4b (the 478 of pbt4b-substitution-list-478.tsv) fails, including one that reads the text in a way only seen when it runs. It returns to the Owner (owner-pbt4-decisions.md, question 4). A copy or a hand-written equivalent of the list that fails in the GREEN is not this stop: it is PBT4B-C07, and the session issues a package version with its hand-written text (D-S1).
PBT4B-C17: (F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." The Worker chooses none of the three exits: the Owner does
PBT4B-C18: a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in the attempt; it touches a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope; it changes an expected line, literal or oracle, or anything other than the when or the how of a reading or a gesture the package already prescribes (RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP.
PBT4B-C20: a numeric box of any window changes its status, its value or its message (NumericFieldValidationTests or CT-PBT4B-18 would see it), or a numeric field of the panel validates other than PushBackAnnotationScaleRule.

ExpectedEvidence:
[0] antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;
[1] el commit RED y su push antes del GREEN; con una reparacion del RED (F4B-E35, RR-4), ademas, el commit "RED repair" y su push antes del GREEN, con su CI, registrada como la del RED reparado, y los TRX de cada filtro con ExpectRed sobre RedRepairSha;
[2] la CI del RED como en section 6.0 (tests y ui-tests en failure, build-ui en success, build-plugin omitido, sin cuelgue); la del RED reparado, si lo hay, da lo mismo;
[3] la tabla "CT -> la asercion que falla en el RED", la de las clases FullPanelCopy<Raiz>RedTests de las copias (sus pruebas fallan por asercion, como en la particion) y las autopruebas de CT-PBT4B-12, 13, 15, 19, 20 y 21;
[4] el GREEN y su CI 4/4;
[5] en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED, sobre RedRepairSha si hay reparacion, y sobre el GREEN, y tres corridas en verde de los filtros con ExpectRed sobre el GREEN;
[6] la entrega con el mapa CT -> metodo, las rutas relativas comprobadas, el sha256 del kit y de cada una de las 21 copias (iguales a CopySha) y el Trailer real de los commits RED, "RED repair" (si lo hay) y GREEN; con una reparacion del RED, ademas, RedRepairSha, la prueba y la linea corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y RedSha, que sigue siendo el commit RED original (F4B-E35, RR-5);
[7] la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3;
[8] con una reparacion del RED, la verificacion la lee como parte de la entrega (F4B-E35, RR-7; section 6.0): n.o 10 (Tests): el RED se acredita con RedSha y con la CI del RED original, y sus TRX, con los filtros con ExpectRed entre los fallidos. La clausula "git diff --name-only RedSha CurrentSha no toca RT U ChainRedFiles" se comprueba desde RedRepairSha: git diff --name-only RedRepairSha CurrentSha. n.o 11 (Trailer): el trailer va tambien en el commit "RED repair". Lo que el Controller comprueba de RR (si algo falla, es C18): RR-1, un solo commit "RED repair", hijo directo de RedSha y anterior al GREEN; RR-2, git diff --name-only RedSha RedRepairSha dentro de las seis clases de las CT; RR-4, la CI de RedRepairSha, registrada como la del RED reparado, y los TRX de los filtros con ExpectRed sobre el; RR-5, la declaracion de la entrega, con el diff, y RedSha, el commit RED original; RR-6, con el tramo RedSha..RedRepairSha. En PB-T4b, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto), ni el kit, ni las 21 copias, ni los 21 archivos de los Skip. La ayuda mecanica de RR-3: es parada C07, una violacion cierta de RR-3, que un literal de la lista cerrada de la clase reparada (section 6.0 del paquete, la tabla de RR-3) cambie entre RedSha y RedRepairSha: una linea o un fragmento, si no aparece como subcadena exacta el mismo numero de veces; un valor de gesto, si no aparece como token el mismo numero de veces. Indicios para la revision, no parada: una cadena <campo>= anadida o quitada en git diff -U0 RedSha RedRepairSha, o un cambio en el primer argumento de un Assert.Equal. Pueden dar falsos positivos, como una linea que solo se mueve de sitio, asi que van a Findings y a la revision, con la declaracion de RR-5.
[9] la CI del RED con tests y ui-tests en failure, build-ui en success y build-plugin omitido;
[10] la del GREEN 4/4, con el Plugin compilado;
[11] el censo de declaraciones de WHC, WEI, FVM, PV, NumericFieldValidation.cs y RIA, y de los cuatro archivos nuevos, en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es una declaracion mas): la diferencia es la lista cerrada de PBT4B-C13;
[12] el Trailer real;
[13] la entrada "## PB-T4b" de superseding-characterization.md, escrita por la sesion en el commit de emision: la tabla por clase y por causa de las 478 (C-0 con sus 9 clases, 356 copias y 72 a mano, con cuatro Theory partidas por filas en las dos clases de la particion, D-S5; C-5 con sus 23, las 2 de PB-T4a y la de la sexta tanda entre ellas; UI con sus 27), el sha256 de la lista, los motivos literales de los Skip, la sustituta de cada una y las 28 copias sin cambio (las 27 de la UI y RG:109-123), declaradas como tales; su tabla de ejecucion se completa al cerrar, con los recuentos por clase;
[14] Linea base de la CI: la del cierre de PB-T4a (F4B-E51, 78a7cef3): run 44814418683 sobre su GREEN faef7020, Core 19 137 resultados, con 19 130 Passed y 7 Skipped, y UI 2 451, con 2 430 Passed y 21 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4b pone 478 Skip (451 en Core y 27 en UI), cada uno sobre una prueba que pasa a un solo resultado omitido, anade 450 copias o equivalentes en Core (cuatro de ellas, Theory partidas por filas en dos metodos, uno en cada clase de la particion, que reparten sus filas sin cambiar ninguna; D-S5) y 27 en UI, con las mismas filas que sus originales, y 21 CT (18 de Core y 3 de UI), sin ninguna Theory nueva en las CT. El GREEN: Core 19 605 resultados, con 19 147 Passed, 0 Failed y 458 Skipped; UI 2 481, con 2 433 Passed, 0 Failed y 48 Skipped. El RED: los mismos totales y omitidas, con 15 + R_C Failed en Core y 3 + R_UI en UI, donde R_C y R_UI son los resultados de las clases FullPanelCopy<Raiz>RedTests de la particion de Core y de UI: <medido en la precondicion 6>.
[15] la comprobacion mecanica de los Skip, check_skips_pbt4b_v48.py, sobre RedSha y sobre CurrentSha frente a BaseSha: los 21 archivos, iguales a su base con su Skip literal y nada mas (478 Skip).
```

**Los `Skip` literales** (`pbt4b_skips_v48.py`; ASCII; `<Clase>` es la clase original sin `Tests`):

| Causa | El motivo |
|---|---|
| C-0, copia mecánica (356) | `I-64 F4B PB-T4b: sustituida por su copia en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).` |
| C-0, a mano (72) | `I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).` |
| C-5 de la v4.3 (14, sin CT-PBT4A-06) | `I-64 F4B PB-T4b: sustituida por su equivalente en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2).` |
| CT-PBT4A-06 | `I-64 F4B PB-T4b: sustituida por CT-PBT4B-13 de PushBackFullPanelCensusGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2).` |
| C-5, las cinco de la tercera tanda | `I-64 F4B PB-T4b: sustituida por su equivalente en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-breaks-decision.md).` |
| C-5, las dos de PB-T4a (CT-PBT4A-03 y 05) | `I-64 F4B PB-T4b: sustituida por su equivalente en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-list-477-decision.md).` |
| C-5, la de la sexta tanda (WHHG) | `I-64 F4B PB-T4b: sustituida por su equivalente en WorkspaceHostHardeningGuardFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-list-478-decision.md).` |
| UI (27) | `I-64 F4B PB-T4b: sustituida por su copia en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2).` |

El atributo pasa de `[Fact]` a `[Fact(Skip = "<motivo>")]` y de `[Theory]` a `[Theory(Skip = "<motivo>")]`, y nada más cambia en el archivo. **CT-PBT4A-06** se llama, en `78a7cef3`, `CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes` (FRG:98); su fila de la lista la nombra por el prefijo y `pbt4b_skips_v48.py` la resuelve. **La comprobación mecánica, `check_skips_pbt4b_v48.py <checkout> 78a7cef3 <revisión>`,** lee los 478 (los 50 de C-5 y de la interfaz, y los 428 de C-0) en los 21 archivos: cada archivo de la revisión tiene que ser, byte a byte con LF, el de la base con el `Skip` literal de cada prueba de la lista y nada más. **CT-PBT4B-20 hace lo mismo en ejecución** (su punto 1). Por archivo: C-0, los de §8.3; `PushBackWritePathGuardTests` 1, IG 2, EIG 7, PEG 5, RG 1, WLG 2, WPUG 1, `WorkspacePilotReconciliationTests` 21, `WorkspacePilotReconciliationViewTests` 6, `PushBackFullReadPathTests` 1, `PushBackFullReadGuardTests` 2 y `WorkspaceHostHardeningGuardTests` 1. **Core 451 y UI 27** (medido, `v45/selftest_pbt4b_v45-out.txt` y, la de la sexta tanda, `v48/selftest_pbt4b_v48-out.txt`).

**`BaseSha` de CT-PBT4B-20** (V45-03; el sha256 del texto de cada archivo de origen en `78a7cef3`, con los saltos de línea normalizados a `\n`; medido, `v45/selftest_pbt4b_v45-out.txt`):

| Archivo | `BaseSha` | `Skip` |
|---|---|---|
| `tests/RackCad.Tests/PushBackFullReadGuardTests.cs` | `97dff70cf57d866895eaae78ebde98e4350135743d747018c7de0a0a4758eb75` | 2 |
| `tests/RackCad.Tests/PushBackFullReadPathTests.cs` | `f6611c3ad90d0e8d481b6a2b56788b53d37f3f0f08af4f1ae5d3cb81783ab28f` | 1 |
| `tests/RackCad.Tests/PushBackWritePathGuardTests.cs` | `996adc5d4ddfc80462e7ade23ef44a8f18fabd0fdbf814b88f2505f437af65b6` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingTests.cs` | `07573661e65b0a4658796a92bf90fd33e79c667a3ccf2f4615588519899bf3f1` | 10 |
| `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs` | `27f26a28a760225bed676f8ca0d52fb1fd4b64d1d601f64bfdfb1b83f6786a66` | 54 |
| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs` | `da9d341f6b544d3d85eaafdd01cffe7796fa5367a8ead1e2f0071e2110e5cab4` | 7 |
| `tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs` | `f52072b53b819edd270e05ec38c987a54aa6772fbe09c18aee71afb4f4067b80` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs` | `df1dd9e118ce78162b304052f64f356cfac30d8c7fd9e6a8ea1eb6f680887b0b` | 62 |
| `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs` | `4044c8678c60a0e05cef7eb6fa8d6b8161651f4dbf17f8b1ced04498b3975997` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs` | `1808ac7a22c33938b575063e3b7a049450f187b493c5b09b4464ddc37acfc5f0` | 69 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs` | `1ef3b59be954177391a0a0c5df07d2daa65728bb90d6f4d20dd46e8531903d35` | 5 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs` | `43afc8f34423a9ee00bc830c3b78473cebc71f08a3f3bdb4551f1e751148400b` | 59 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs` | `d85407d0961082f01066c61a6cd6e21e95038f001572660d00f7414737390ade` | 67 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs` | `48dead3420e3ac272bb05e43732bae50d8e47eccacf09bb8f37578acc5d35494` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs` | `e6557a8032ef69f37f6b4a91f1c918510a19d13f64d5bff61912b1b91f3a0aac` | 28 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs` | `e7db8e72cb4bfcd182f686b907128c8428f59fd14ec812365fd1b10a5dc089fc` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs` | `d602daf657aae175f9972052e1594034959c0706fd2466644f740d981bfa18fe` | 35 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs` | `d5c075112c4eee1e47ebdcae641e80c78d10cb4fda4a5b70eac8833bddccafb2` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs` | `cabfb232768fe69e1e7b686722fa87d05efe97b44474c1b4b8a411db598b130e` | 44 |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs` | `1cedca91c973b0dfe8825e002bfcab4fc883d71219bf7fc4612f19dc4ea8300a` | 21 |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewTests.cs` | `e8db638a72da2a517cba2ea66a762a79b25ddd75621e9d3e95c07ef97a2bd568` | 6 |

**El kit, literal** (D-PBT4-11.4): el archivo custodiado **`pbt4b-kit-WorkspaceFullReadKit-v45.cs`**, sha256 **`b4ed29f92838e5761faf1afaba1f33e58ba82a56541900f52db8c5ecb6239b9a`**, que va byte a byte en `tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs` (el de la v4.4, `pbt4b-kit-WorkspaceFullReadKit.cs`, `52b2fb90…e9bea`, queda como historia). Sus cinco tipos: `FullEditIntentFakeReader` (`Calls`, `Answer`, `Throw` y `ReadIntent`), `FullEditIntentKit` (`RackA`, `RackB`, `TwoViews`, `Admitted`, `ReadOf`, `OriginMissing` y `MemberBecameBlocking`), `FullPrepareKit` (`Rack`, `Frontal`, `Lateral`, `Planta`, `ExistingPost`, `MissingPost`, `Scan`, `RealScan`, `BaseOf`, `Reread`, con `Assert.Null(state)` por M-04, los dos `DraftOf`, `SetScale` y `Prepare`), `FullHandKit` (`FullState`, `FullOfText`, `Twin`, `FullOf`, `FullOfMatchesTheReader` por M-05, `ReadOfPilot`, `Prepare`, `Seen` y `ReadWithoutFullRead`) y `FullReadRig` (`Host`, `Core`, `Session`, `Request`, `Draft`, `Begin` y `Run`, el ayudante de CT-PBT4A-03 y 05 sobre el borrador completo).

**El guion de la copia y los equivalentes, literales:** `make_copies_pbt4b_v49.py` (sha256 `d0bdf62e216518e90f8e9fc28e644467b2ec3333713a43fe9e340b1f91d9533f`), el de la v4.8 (`make_copies_pbt4b_v48.py`, sha256 `74f15dbd7686596ccd4d66e2989f3888f61adaa2fd8a37dc11cf92c81674317f`) con `pbt4b_hand_edits_v49.py` (sha256 `a10461f62041984a570168e01b5ab0997dabbfab21a58c810ebd979c28646190`), que carga `pbt4b_hand_edits_v48.py` (`7d98d007d647f125596088ecf29eccec7352dc9682decb520d7e4369d3dde9fa`); y, como en la v4.8, las piezas de `make_copies_pbt4b_v47.py` (`2a96b2afb6ce84824047809101f0aed052d0f42717bc64382d917815667dd247`), `make_copies_pbt4b_v46.py` (`3cc8e56cfc2609d567e69577a28f03ac5801c5b46f0bddba5c6692cc715ecdd6`) y `make_copies_pbt4b.py` de la v4.4 (`a6fd6c01547a01ba1fa2be03fbaced84caed2f4f1365082a701ff39dca33fa3c`), sin tocarlas, con `pbt4b_hand_edits_v47.py` (`d2e4429dffda21640278f2c739e7a6dad8f7db0a08d0c58c6c23496fca88c000`), `pbt4b_hand_edits_v46.py` (`46167c571245148e597af36a6efe5c8b6d5811d0b457eefb0101f035fea7e93e`), `pbt4b_hand_edits_v45.py` (`7403c43f3589d17fe5bdb120bb6016dbacf440ab52de37a168f493eb4a8ce4e5`) y `pbt4b_hand_edits.py` (`68306474ee14a351f0c8f9ae8b3ca0e791f75c06f0425304683aec9788c5808a`), `copy_map_defs.py`, `lists_core.py`, la lista de 478 y el mapa de la copia v5. En el modo de medida, sobre `78a7cef3`, da 21 archivos (`v49/copies_probe_v49`; el sha256 del conjunto de las líneas «sha256  ruta», ordenadas, `4ff5b6705a409478cbfaa69ff49b78bd0fdeb6549c64003c367df3a2ddeb595e`; `v49/copies_probe_v49-out.txt`): las de la v4.8 salvo `WorkspaceHostHardeningGuardFull.cs`. Su revisión, `pbt4b-equivalents-review-v49.txt` (sha256 `182b6aed301dc18a176da947b6eefacac2f8d7f68d88ab3f0f0c8449f99311ec`), lleva el texto final de las 114 entradas. Las de la v4.5 a la v4.8 quedan como historia. **Las autopruebas:** `selftest_pbt4b_v48.py` (`v48/selftest_pbt4b_v48-out.txt`) y `selftest_pbt4b_v49.py` (`v49/selftest_pbt4b_v49-out.txt`), que comprueba que la copia de WHHG es la de la v4.8 con un solo bloque tras el `Assert.Empty`: el comentario de I-01 y las líneas WHHG:504-525 del objeto git, literales, con sus 13 `Assert.NotEmpty`.

**La partición cerrada en `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests` y lo demás de la precondición 6** (I-02; §11.6), que corre la sesión antes de emitir, sobre copias exportadas del cierre de PB-T4a, fuera de todo commit:
1. `git archive` de `78a7cef3` a una carpeta nueva, fuera del worktree;
2. `python -I apply_red_pbt4b_v49.py <copia> --repo <checkout> --rev 78a7cef3` (sin `--partition`: el esqueleto, los `Skip`, el kit y las 21 copias en modo Probe, una clase `FullPanelCopy<Clase>ProbeTests` por archivo; sin las CT);
3. la compilación, sin ningún `warning CS`, y las TRX de `FullyQualifiedName~RackCad.Tests.FullPanelCopy` y de `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopy` (D-S6);
4. `python -I partition_pbt4b_v48.py red <core.trx> <ui.trx> pbt4b-copy-partition.tsv`: la partición (archivo, método, `Red` o `Green`, resultados), su sha256, el número de resultados de cada clase y **R_C y R_UI**. **La regla de la aserción es la de INV-PBT4B-02:** la primera línea del StackTrace del resultado está bajo `tests/`. **Una `Theory` que mezcla se parte por filas** (D-S5; V47-02) si es una de las cuatro declaradas y sus filas que fallan son las declaradas (`SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v48.py`, los de la v4.7): da dos filas de la partición, Red y Green, y la salida dice cuántas parte (`4 partidas por filas, D-S5`). **Para** si otra prueba mezcla resultados (se le aplica D-S5 con una versión del paquete, y el paso 4 se repite), si una de las cuatro no mezcla o mezcla otras filas, o si una prueba no corre, se salta o falla sin aserción (la primera línea, en `src/` o sin ruta);
5. **sobre otra copia exportada, el RED exacto:** `apply_red_pbt4b_v49.py … --partition pbt4b-copy-partition.tsv`, sin las CT; la suite completa de Core y los filtros de UI de la subtarea: **Core 19 587 = (19 129 − R_C) + R_C + 458** y **UI 2 478 = (2 430 − R_UI) + R_UI + 48**, con exactamente los R_C y R_UI de la partición `Failed` y nada más (D = 0 en el RED); `check_skips_pbt4b_v48.py` sobre la copia, por su texto (en la corrida, `check_skips_copy_v48.py`, que compara la copia con la base); **la UI, solo con sus filtros** (M-10 de la §19; V46-06): una prueba de UI de fuera de ellos que cayera solo se vería en la CI del RED o del GREEN (C16);
6. **sobre la misma copia, el GREEN literal:** `apply_green_pbt4b_v45.py …`; la suite completa de Core y los filtros de UI: **Core 19 587 = 19 129 + 0 + 458** y **UI 2 478 = 2 430 + 0 + 48**, ningún `warning CS`, y `python -I partition_pbt4b_v48.py green <core.trx> <ui.trx>`: todas las copias pasan. **Las que no pasan** pasan a equivalentes escritos a mano (D-S1; si una `Theory` pasa en unas filas y no en otras, D-S5): la emisión necesita una versión del paquete con su texto, y el paso 4 se repite. **Una prueba que falle fuera de la lista de 478** vuelve al Owner;
7. **las dos sondas, sobre la copia del GREEN,** como clases temporales en `tests/RackCad.Tests/` que no entran en ningún commit y acaban en `Assert.Fail(texto)`, corridas solo con su filtro:
   - **las líneas «presentar» de CT-06:** `pbt4b-ct06-presentar-probe.cs` (`TmpCt06PresentarProbe`), con lo esperado de §5.2; si no son esas, el paquete se corrige antes de emitir;
   - **el coste de la lectura** (D-S4): `pbt4b-read-cost-probe.cs` (`TmpReadCostProbe`), la sonda de `read-cost` con las dos órdenes, la de PB-T4a y la de PB-T4b. **Lo que mide** (M-09 de la §19; V46-05): el recuento `materializaciones` es el de su propia réplica del orden de RIA, NH y `AtOrigin`, porque las pruebas no referencian el Plugin; lo de RIA lo fija CT-PBT4B-21, por texto, y la sonda mide el coste del orden. **Lo esperado:** `materializaciones Begin=1 origen-encontrado=1 cae-al-Primary=1 origen-es-Primary=1` en los seis escenarios, sin `DISTINTA`; y, por escenario admitido, la mediana de `t4b relectura origen-encontrado` no pasa de la de `t4b Begin` más las de `solo el piloto` y `solo BuildStaleBase` más el mayor de 10 ms y el 30 % de `t4b Begin`, y es menor que el 75 % de la de `t4a relectura origen-encontrado`; `t4b Begin`, `cae-al-Primary` y `origen-es-Primary` no pasan de sus `t4a` más el mayor de 10 ms y el 30 %. Con las medidas de `read-cost`, la relectura con el origen encontrado debe bajar de +80 a +175 ms sobre la base a la de `Begin` más unos 3 a 12 ms. **Si no se cumple,** la emisión se detiene y vuelve a la sesión, que lo lleva al Owner (un efecto material, pregunta 4); las TRX y su sha256 van con la emisión. **En la v4.8, la primera de tres corridas no cumplió un caso, y el Owner lo dio por cumplido** (`owner-pbt4b-cost-decision.md`, la séptima tanda; las tres corridas, en `pbt4bcc/compile/compile-check.md`); **la v4.9 conserva ese paso 7**, porque no cambia ninguna de sus entradas, comprobado por sha256 (V49-06);
8. el contrato lleva por sha256 la partición, las 21 copias generadas con ella (`CopySha`) y el kit; el texto de la partición, prueba a prueba, va en el archivo custodiado, no en el contrato (la duda 5 del generador).

**`RequiredTests`** (38 heredados + 8 nuevos + las clases de las copias). Todos los heredados con `ExpectRed` false; sus `MinSelected`, con los `Skip` de la lista (el criterio de `count_tests_git.py`, medido por el generador en la fase 1, más las siete nuevas, todas `[Fact]`):

| # | Project | Filter | MinSelected | ExpectRed |
|---|---|---|---|---|
| 1 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests` | **4** | false |
| 2 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests` | **1** | false |
| 3 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackViewEraseGuardTests` | 4 | false |
| 4 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 | false |
| 5 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateGuardTests` | **85** | false |
| 6 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | **121** | false |
| 7 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackSharedReferenceGuardTests` | 2 | false |
| 8 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 | false |
| 9 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullWrite` | 13 | false |
| 10 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 | false |
| 11 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathGuardTests` | **1** | false |
| 12 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | **2718** | false |
| 13 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullDraft` | 10 | false |
| 14 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests` | 2 | false |
| 15 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | **521** | false |
| 16 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditableState` | 13 | false |
| 17 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate` | 1 | false |
| 18 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 | false |
| 19 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 | false |
| 20 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 | false |
| 21 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false |
| 22 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 10 | false |
| 23 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 16 | false |
| 24 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 | false |
| 25 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet` | 14 | false |
| 26 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 14 | false |
| 27 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 | false |
| 28 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.I58F1` | 10 | false |
| 29 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 9 | false |
| 30 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.NamespaceFolderGuardTests` | 7 | false |
| 31 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftStatePortTests` | 3 | false |
| 32 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 | false |
| 33 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 | false |
| 34 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 | false |
| 35 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 | false |
| 36 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 4 | false |
| 37 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 | false |
| 38 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 | false |
| 39 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelTests` | 11 | true |
| 40 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelGuardTests` | **4** | true |
| 41 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelCensusGuardTests` | 1 | false |
| 42 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPanelDraftGuardTests` | 1 | false |
| 43 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspaceFullCopyGuardTests` | 1 | false |
| 44 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullPanelFieldTests` | 3 | true |
| 45 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldValidationTests` | 13 | false |
| 46 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldTests` | 10 | false |
| 47 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostRedTests` | `<medido en la precondicion 6>` | true |
| 48 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostGreenTests` | `<medido en la precondicion 6>` | false |
| 49 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareRedTests` | `<medido en la precondicion 6>` | true |
| 50 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests` | `<medido en la precondicion 6>` | false |
| 51 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostRedTests` | `<medido en la precondicion 6>` | true |
| 52 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostGreenTests` | `<medido en la precondicion 6>` | false |
| 53 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditHostRedTests` | `<medido en la precondicion 6>` | true |
| 54 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditHostGreenTests` | `<medido en la precondicion 6>` | false |
| 55 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorRedTests` | `<medido en la precondicion 6>` | true |
| 56 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorGreenTests` | `<medido en la precondicion 6>` | false |
| 57 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestRedTests` | `<medido en la precondicion 6>` | true |
| 58 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestGreenTests` | `<medido en la precondicion 6>` | false |
| 59 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostRedTests` | `<medido en la precondicion 6>` | true |
| 60 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostGreenTests` | `<medido en la precondicion 6>` | false |
| 61 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostRedTests` | `<medido en la precondicion 6>` | true |
| 62 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostGreenTests` | `<medido en la precondicion 6>` | false |
| 63 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingRedTests` | `<medido en la precondicion 6>` | true |
| 64 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingGreenTests` | `<medido en la precondicion 6>` | false |
| 65 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackWritePathGuardRedTests` | `<medido en la precondicion 6>` | true |
| 66 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackWritePathGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 67 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateIntegrationGuardRedTests` | `<medido en la precondicion 6>` | true |
| 68 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateIntegrationGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 69 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentGuardRedTests` | `<medido en la precondicion 6>` | true |
| 70 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 71 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardRedTests` | `<medido en la precondicion 6>` | true |
| 72 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 73 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationGuardRedTests` | `<medido en la precondicion 6>` | true |
| 74 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 75 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleGuardRedTests` | `<medido en la precondicion 6>` | true |
| 76 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 77 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateGuardRedTests` | `<medido en la precondicion 6>` | true |
| 78 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 79 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationRedTests` | `<medido en la precondicion 6>` | true |
| 80 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests` | `<medido en la precondicion 6>` | false |
| 81 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewRedTests` | `<medido en la precondicion 6>` | true |
| 82 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests` | `<medido en la precondicion 6>` | false |
| 83 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadPathRedTests` | `<medido en la precondicion 6>` | true |
| 84 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadPathGreenTests` | `<medido en la precondicion 6>` | false |
| 85 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadGuardRedTests` | `<medido en la precondicion 6>` | true |
| 86 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadGuardGreenTests` | `<medido en la precondicion 6>` | false |
| 87 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardRedTests` | `<medido en la precondicion 6>` | true |
| 88 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardGreenTests` | `<medido en la precondicion 6>` | false |

- **Nota (no va al contrato).** Los filtros heredados no seleccionan las copias (D-S6): sus clases están en el espacio raíz y empiezan por `FullPanelCopy`, y `FullyQualifiedName~RackCad.Tests.FullPanelCopy…` no contiene `RackCad.Tests.Workspace`, `RackCad.Tests.PushBackFullRead…` ni ningún otro filtro de la tabla (medido por texto, `pbt4bcc/compile/out-diag-analysis/filter-overlap.txt`: 0 de 46). La guarda de los textos del anfitrión de la sexta tanda (WHHG) está en `RackCad.Tests` y el filtro n.º 12 la seleccionaba: con su `Skip`, su `MinSelected` baja de 2 719 a **2 718**. `WorkspaceFullCopyGuardTests` (CT-20) está en `RackCad.Tests` y el filtro n.º 12 también la selecciona: pasa en el RED y en el GREEN. **`NumericFieldTests` se cuenta por archivo** (la duda 3): el filtro selecciona sus 10 pruebas en ejecución, aunque `count_static` las atribuya a su clase anidada `BorderBrushSource`. **Una clase de copia sin pruebas en su partición no existe, y su fila sale de la tabla** al emitir. Los n.º 1 y 2 bajan por CT-PBT4A-03 (de 5 a 4) y por CT-PBT4A-05 y 06 (de 3 a 1).

#### 6.2.2 Lo que necesita el generador de PB-T4b (las dudas de la fase 1, `generator-doubts-pbt4b.md`)

| Pieza | Dónde, en este paquete | Lo que comprueba el generador |
|---|---|---|
| **`TaskId`** | §6.2.1 | `F4B-PBT4-FULL-DRAFT-PANEL`, 25 caracteres, ≤ 26 |
| **`Objective`** | §6.2.1 | ASCII, byte a byte; cita el paquete v4.9, CT-PBT4B-01..21, las **478** sustituciones (antes, 477), el `Skip`, la materialización única y que lo retira PB-T4d |
| **`AllowedWriteScope`** | §6.2 | Las 59 rutas, en ese orden; ninguna de `Pilot/`; de `src/RackCad.Plugin/`, solo RIA |
| **`ForbiddenWriteScope`** | §6.0 y §6.2 | Su regla, sobre el alcance final en `78a7cef3` (con el de la v4.4, 407; el de la v4.8 añade dos rutas de `tests/`, la copia y el archivo de la guarda nueva: `<medido por el generador>`) |
| **`UNIT_CHANGE`** | §6.0 y §6.2 | WHC, WEI, FVM, PV, `NumericFieldValidation.cs` y RIA |
| **`Invariants`** | §6.2.1 | INV-PBT4B-01 a 14, literales |
| **`StopConditions`** | §6.0 y §6.2.1 | Las 17 estándar del contrato de PB-T4a y PBT4B-C01 a C14, C16, C17, C18 y C20 (sin C15 ni C19): **35**. **C11** es `PBT4A-C11` con «PB-T4a» → «PB-T4b» y acaba en «no one-off acceptance extends to PB-T4b». **C02** no tiene excepción fuera de los `Skip` de la lista de 478 |
| **`RequiredTests`** | §6.2.1 | Los 38 heredados con sus `MinSelected` (n.º 1: **4**; n.º 2: **1**; n.º 5: **85**; n.º 6: 121; n.º 11: 1; n.º 12: **2 718**; n.º 15: 521), las seis clases de las CT (`PushBackFullPanelGuardTests`, **4**), `NumericFieldValidationTests` (13) y `NumericFieldTests` (10, por archivo), y las 42 filas de las clases de las copias (D-S6: `FullPanelCopy<Clase>RedTests` y `…GreenTests`) con sus `MinSelected` medidos (`<medido en la precondicion 6>`: el número de resultados de cada clase en la partición; una clase sin pruebas sale de la tabla) |
| **`ExpectedEvidence`** | §6.2.1 | 16; `[0]` byte a byte el de PB-T3c; `[14]` con los totales de §5.2; la cuenta de CT (**21**), de `[Fact]` (21), de `Skip` (**478**) y de CT en filtros con `ExpectRed` (**18**: 15 de Core y 3 de UI), más las clases `FullPanelCopy…RedTests` de la partición, también con `ExpectRed` |
| **La lista** | `pbt4b-substitution-list-478.tsv` | sha256 `8b0fcd0c…9e8b`; es la de 477 (`5975c5b6…a651`) con la fila de WHHG al final; recuentos C-0 428, C-5 **23** y UI 27 |
| **La fila de CT-PBT4A-06** | La lista | Resuelta: el método de FRG que empieza por `CT_PBT4A_06` es `CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes` (`78a7cef3`, FRG:98); la lista no cambia, porque su sha256 la fija |
| **El mapa de la copia** | `pbt4b-copy-map-v5.tsv` | sha256 `bda79bb7…4524`; **356** mecánicas y **72** a mano (los motivos de la v4.3, siete `R-D-S1`, ocho `R-D-S1-compila`, dos `R-D-S5`, uno `R-D-S1,R-D-S5` y dos `R-D-S1-borra`) |
| **La lista de PB-T4d** | `pbt4d-retire-list-138.tsv` | sha256 `d5467dc1…387c`; 138 sustituciones, 55 originales y 29 miembros |
| **El esqueleto y el GREEN** | `green_pbt4b_v45.py` (`skeleton` y `green`) | El parche del esqueleto (el de la v4.4), los diez archivos del GREEN, la pureza de `Draft/` y la regla del §5.0 sobre los `?` sobre el texto literal (RIA lleva `#nullable enable`) |
| **El kit, el guion de la copia, los equivalentes y la partición** | §6.2.1 | Por sha256: el kit de la v4.5, `make_copies_pbt4b_v49.py`, `make_copies_pbt4b_v48.py`, `make_copies_pbt4b_v47.py`, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v49.py`, `pbt4b_hand_edits_v48.py`, `pbt4b_hand_edits_v47.py`, `pbt4b_hand_edits_v46.py`, `pbt4b_hand_edits_v45.py` y `pbt4b_hand_edits.py`, la partición y las 21 copias (`CopySha`, `<medido en la precondicion 6>`), todos del texto con los saltos de línea a `\n` |
| **`BaseSha` de CT-20** | §6.2.1 | Los 21, medidos en `78a7cef3` (la tabla de arriba) |
| **El registro declarado** | §6.2.1 `[13]` | La entrada «## PB-T4b», por clase y por causa, con el sha256 de la lista y las 28 copias sin cambio (la duda 6) |
| **El tamaño** (M-06) | — | **El contrato** (hacia 75 a 80 KB, con el `ForbiddenWriteScope`) **y las listas nunca van en línea en un prompt:** la línea de órdenes de Windows admite unos 32 767 caracteres y ningún prompt de la cadena pasa de **30 000 bytes**; la cadena los cita por ruta y sha256, y `check_prompt_size.sh` aborta con 93 (`pbt4bcc/adapt-notes-pbt4b.md`; la verificación 1 de PB-T4a no arrancó por eso) |
| **La precondición 6** | `pbt4bcc/compile/pbt4b-precondition6.json` | Schema `rackcad-pbt4b-precondition6/v1`, Revision (`78a7cef3…`), PartitionSha256, CopySha (las 21), R_C, R_UI y `MinSelected` de las 42 clases de las copias, por el nombre de la clase (`FullPanelCopy<Clase>RedTests` o `…GreenTests`; 0: la fila sale); la corrida, en su `compile-check.md` |
| **La numeración** | — | F4B-E51 (el cierre de PB-T4a), F4B-E52 (la emisión) y las secciones 279, 280 y 281 |

**Las autoridades de PB-T4b:** las de PB-T4a (§6.1.2) y, además: `owner-pbt4b-breaks-decision.md` (la tercera tanda), `owner-pbt4a-cost-decision.md` (la decisión del Owner sobre el coste de la lectura, de la que sale D-S4; en `F4B-DRAFTS/coordinator/` del cierre `78a7cef3`), `owner-pbt4b-list-477-decision.md` (la quinta tanda) , `owner-pbt4b-list-478-decision.md` (la sexta) y `owner-pbt4b-cost-decision.md` (la séptima, el coste), que la emisión de PB-T4b custodiará junto a las otras tandas y a `session-pbt4b-design-decisions.md`, `session-pbt4b-design-decisions.md` (D-S1 a D-S3, D-S5 y D-S6; D-S4, en §8.5 y D-PBT4-21), las §17 y §18 de `independent-review-v1.md` y el cierre de PB-T4a (F4B-E51); este paquete (v4.9), con su comprobación acotada de PB-T4b (CA-T4b: la §20 de `independent-review-v1.md`, sobre la v4.8), y la §19 de `independent-review-v1.md`. **No son autoridades de PB-T4b** `owner-pbt4a-verify-stop-decision.md` ni `owner-pbt4a-nc1-decision.md`: son de la cadena de PB-T4a.

### 6.3 PB-T4c — `F4B-PBT4-EDIT-ALL-WINDOW` (entero; V50-03, V50-04, V50-09, V50-13 y V50-16)

**Se emite por la autorización condicional** (`owner-pbt4-decisions.md`, pregunta 4) **con la lista exacta de 4** que el Owner tomó como la que vio (las 2 de la pregunta 4, CT-PBT4B-20 de `owner-pbt4c-list-3-decision.md` y CT-PBT2B-06 de `owner-pbt4c-list-4-decision.md`; `pbt4c-substitution-list-4.tsv`, sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de`), si se cumple el resto de la condición, **sobre el cierre de PB-T4b** (F4B-E53, `6adff90d`).

- **Objective** (ASCII): el de §6.3.1.
- **`AllowedWriteScope`** (26), ruta a ruta, en este orden:
  - **src/, existentes (UNIT_CHANGE, 9):** 1. `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`, 2. `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`, 3. `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs`, 4. `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`, 5. `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`, 6. `src/RackCad.UI/Workspace/WorkspacePanelView.xaml`, 7. `src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs`, 8. `src/RackCad.Plugin/Workspace/WorkspaceController.cs`, 9. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`;
  - **src/, nuevos (4):** 10. `src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs`, 11. `src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs`, 12. `src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs`, 13. `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`;
  - **las clases de las CT (5):** 14. `tests/RackCad.Tests/PushBackEditAllTests.cs`, 15. `tests/RackCad.Tests/PushBackEditAllGuardTests.cs`, 16. `tests/RackCad.Tests/PushBackOpenerClassicPathGuardTests.cs`, 17. `tests/RackCad.Tests/PushBackDraftLaunchGuardTests.cs`, 18. `tests/RackCad.UI.Tests/PushBackEditAllWindowTests.cs`;
  - **los equivalentes (4), nuevos, que genera el guion:** 19. `tests/RackCad.Tests/PushBackEditAllPortEquivalenceGuardTests.cs`, 20. `tests/RackCad.Tests/PushBackEditAllNameWritersGuardTests.cs`, 21. `tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs`, 22. `tests/RackCad.UI.Tests/PushBackEditAllApplyCloseTests.cs`;
  - **solo para los Skip (4):** 23. `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`, 24. `tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs`, 25. `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs`, 26. `tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs`;
  - **ningún archivo de `Pilot/`, ni NH, ni RIA, ni `RackPushBackSystemWindow.xaml`:** el GREEN literal no los cambia.
- **`UNIT_CHANGE`:** las 9 existentes de `src/` (las de la primera fila; F4B-E25).
- **`ForbiddenWriteScope`:** el método de [PKC-C] con el precedente de PB-T3c, PB-T4a y PB-T4b en `tests/` (la lista explícita del contrato anterior, menos las rutas de `tests/` de este alcance; un prefijo prohibido que cubre una de ellas se sustituye por la lista de sus archivos), más las clases de PB-T4b tras su cierre (sus seis clases de las CT, el kit y las 21 copias), salvo las de este alcance (`WorkspaceFullCopyGuardTests.cs`, solo para su `Skip`). **El número lo da la regla del generador sobre el alcance final en `6adff90d`** (`<medido por el generador>`).
- **El RED** (un commit, con su push antes del GREEN):
  - **el esqueleto exacto de `src/`** (`pbt4c-skeleton-v51.txt`, sha256 `f8840c67fd41c3bbdabf125d9b80941e3f3aedf5e610681c8ce15f111e7b392e`, que aplica `green_pbt4c_v51.py`): 4 existentes (el puerto, WEI, WHC y FVM) y 4 nuevos (`IWorkspaceFullEditHost.cs`, los dos de `Draft/` y el abridor), inertes; la compuerta, WCo, PV, `WorkspacePanelView.xaml.cs` y W no cambian;
  - **los 4 `Skip`** literales, en los 4 archivos de la lista, y nada más en esos archivos;
  - **los 4 equivalentes,** byte a byte los que genera `make_equivalents_pbt4c_v50.py` (su sha256, `EquivalentSha`, va en el contrato);
  - **las cinco clases de las CT.**
  - **La herramienta:** `python -I apply_red_pbt4c_v51.py <dir-copia> --repo <checkout> --rev 6adff90d` escribe exactamente eso salvo las CT, tras comprobar que la copia es de la revisión; el Worker puede usarla sobre su worktree o copiar sus salidas. Imprime el sha256 de cada archivo.
- **El GREEN:** solo `src/`, los 13 archivos del texto literal de `pbt4c-green-v51.txt` (sha256 `081ef77e66a390cf3637f15851b5fe05a5cdf07f6e0065928fc5c841b250dc16`; `apply_green_pbt4c_v51.py` lo escribe en una copia). `tests/` no cambia después del RED (salvo una reparación del RED, solo en las clases de las CT).
- **`RequiredTests`:** la tabla de §6.3.1.
- **Invariantes:** INV-PBT4C-01 a 06 (§6.3.1) y los comunes 07 a 14.
- **`StopConditions`:** las 17 estándar y PBT4C-C01 a C14, C16, C17 y C18 (§6.3.1): 34.
- **`ExpectedEvidence`:** §6.3.1.
- **La línea base:** el cierre de PB-T4b (F4B-E53, `6adff90d`), con la CI de su GREEN (`44903818806` sobre `c065080d`).

#### 6.3.1 Los textos literales del contrato de PB-T4c

Para la comparación del generador, en ASCII, campo a campo. Todo está medido en `6adff90d`; lo que da el generador va como `<medido por el generador>`.

```text
TaskId: F4B-PBT4-EDIT-ALL-WINDOW

Objective:
Anadir Editar todo al panel del Workspace de Push Back sobre el borrador completo de PB-T4b: un boton y su gesto, que confirma el texto pendiente con CommitField antes de abrir (R-PB-10); TryEditAll en el puerto, con su cuerpo por defecto, en el anfitrion y en sus dos reenvios (la compuerta de avisos y el adaptador del panel del Plugin), que empieza el borrador si hace falta, mantiene la marca de lectura mientras la ventana esta abierta (K = 1) y abre la ventana de Push Back en modo WORKSPACE en dos piezas: el modal del panel, en una clase anidada del controlador del Plugin sobre IWorkspaceFullEditHost, y la ventana, sin dueno, en una clase nueva de Plugin/Views; el lanzamiento y el adaptador del puerto de Draft/, que conservan Origin y acaban la marca known clean; la guarda de texto de OB-01.3 sobre los seis sujetos del camino; Cerrar en WORKSPACE con una sola pregunta si hay a la vez un modulo escenificado y cambios sin aplicar (A-T2b-OWNER-01); y el resumen y Modificado al cerrar, segun el paquete del contrato de PB-T4 v5.1, las decisiones del Owner sobre PB-T4 (con las dos de la lista de PB-T4c), la lista cerrada CT-PBT4C-01..15 y la lista exacta de 4 sustituciones que el Owner tomo como la que vio: cada una con su Skip de motivo literal y su equivalente, literal del paquete. Solo se sustituyen con registro la guarda de las operaciones del puerto (PEG:155-163), el censo de archivos del Plugin con PushBack (CT-PBT1B-15), la guarda del guion de Saltar y duplicar (CT-PBT4B-20) y la pregunta de Cerrar con un modulo escenificado (CT-PBT2B-06). Ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC; cero escrituras en el DWG desde la ventana; no se ejecuta AutoCAD.

Invariants:
INV-PBT4C-01. Alcance: dentro de las 26 rutas. git diff BaseSha..CurrentSha -- src/RackCad.Domain vacio; en src/RackCad.Plugin solo Workspace/WorkspaceController.cs y Views/RackWorkspaceEditorWindow.cs; en src/RackCad.Application solo WHC, IWorkspacePilotEditPort.cs, WorkspacePilotSupport.cs, WEI, Workspace/IWorkspaceFullEditHost.cs y los dos archivos nuevos de Systems/PushBack/Draft/; en src/RackCad.UI solo FVM, Workspace/WorkspacePanelView.xaml, Workspace/WorkspacePanelView.xaml.cs y Systems/PushBack/RackPushBackSystemWindow.xaml.cs; ningun archivo de Pilot/; en tests/ solo las cinco clases de las CT, los cuatro equivalentes y las lineas de atributo con Skip de los cuatro archivos de la lista.
INV-PBT4C-02. RED y GREEN: el RED son las cinco clases de las CT, los cuatro equivalentes byte a byte (su sha256, EquivalentSha), los cuatro Skip y el esqueleto exacto de src/ (pbt4c-skeleton-v51.txt: el puerto con TryEditAll y su cuerpo por defecto, WEI con EditAllStatus y EditAllOutcome, IWorkspaceFullEditHost.cs entero, WHC solo con SetFullEditHost sin efecto, FVM con ShowEditAllButton en false y EditAll sin efecto, y los dos archivos nuevos de Draft/ y el abridor, inertes; la compuerta, WCo, WorkspacePanelView.xaml, WorkspacePanelView.xaml.cs y RackPushBackSystemWindow.xaml.cs sin cambio); el GREEN solo toca src/, con el texto literal del paquete (pbt4c-green-v51.txt). En el RED, cada filtro con ExpectRed da todos Failed por asercion (la primera linea del StackTrace bajo tests/), con Failed == Selected, tambien el del equivalente de CT-PBT2B-06; los filtros con ExpectRed false pasan y sus autopruebas fallan sobre el texto inyectado. Con una reparacion del RED (F4B-E35, RR): RedSha..RedRepairSha solo toca las cinco clases de las CT y ningun archivo de src/, ni los cuatro equivalentes, ni los cuatro archivos de los Skip; el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el GREEN de arriba.
INV-PBT4C-03. La API: exactamente la de D-PBT4-12 a D-PBT4-17, con el texto literal del GREEN del paquete; TryEditAll es el unico miembro nuevo del puerto, con su cuerpo por defecto, y sus dos reenvios explicitos, uno en la compuerta y otro en el adaptador del Plugin, y nada mas; ForDraft tiene un solo llamador en src/ (RackWorkspaceEditorWindow.Show) y EnterPanelModal() otro (FullEditorModal.ShowModal).
INV-PBT4C-04. Cero escrituras: ninguno de los seis sujetos de CT-PBT4C-07 nombra LockDocument, una transaccion, Commit( ni una API de escritura del dibujo; la ventana solo entrega sus ediciones al puerto del lanzamiento; nada despues de cerrar.
INV-PBT4C-05. La ventana: solo cambia PendingWork() y su constante WorkspaceStagedAndUnappliedQuestion, en WORKSPACE (en WORKSPACE, un modulo escenificado deja siempre cambios sin aplicar: D-S10); ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC cambian; RackPushBackSystemWindow.xaml no cambia.
INV-PBT4C-06. K = 1 y los nombres: la marca de lectura envuelve TryEditAll, con el inicio del borrador y ShowModal; con el editor abierto, la tabla de D-PBT4-14 (CT-PBT4C-05); WHC conserva sus dos interfaces; los tipos del puerto y del soporte evitan Draft, Editor, Subscription y Tab, y ningun tipo de RackCad.Application.Workspace lleva Subscription, Editor ni Draft (D-S9).
INV-PBT4C-07. Las redes: section 5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, salvo las 4 de la lista cerrada (pbt4c-substitution-list-4.tsv, sha256 9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de), cada una con su Skip de motivo literal. En CurrentSha, el git diff -U0 BaseSha..CurrentSha de los cuatro archivos de pruebas existentes solo lleva las lineas de atributo con Skip.
INV-PBT4C-08. Las pruebas de UI: las reglas de section 5.0.
INV-PBT4C-09. Determinismo: GUID fijo, cultura invariante, "R" y orden ordinal; ni hora ni aleatoriedad.
INV-PBT4C-10. Las corridas locales: solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, si; sin copias de arboles; las corridas temporales terminan en Assert.Fail(string.Join("\n", lineas)), corren dentro de EditorDiscardPrompt.Substitute cuando abren ventanas, no entran en ningun commit y guardan TRX y log como artefactos.
INV-PBT4C-11. La evidencia: rutas relativas comprobadas en worker-handoff.json; TreeSha real o null; el Trailer, el real; el Worker no escribe en docs/.
INV-PBT4C-12. Las reglas de F4B; sin AutoCAD; la verificacion con las tres comprobaciones deterministas de F4B-E15.
INV-PBT4C-13. Las declaraciones nuevas en src/: las de la lista cerrada de PBT4C-C13, y ninguna mas, por nombre y acotada a los archivos que tocan, sobrecargas incluidas.
INV-PBT4C-14. D2: fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna linea anadida muta un PushBackSystem ni un RackModuleReconciliationResult adoptado.

StopConditions (propias):
PBT4C-C01: doing the task would require writing outside the AllowedWriteScope.
PBT4C-C02: it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a .csproj or Fixtures/I64. No exception outside the closed list of PB-T4c: the Skip of each of the 4 tests of pbt4c-substitution-list-4.tsv (sha256 9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de), with its literal reason, in its attribute line, and nothing else in those 4 files. Any existing test that fails, outside that list, returns to the Owner.
PBT4C-C03: an observable change of RACKEDITAR appears, or of the panel or of the window other than the declared ones (the Editar todo button and its gesture; the full editor opened from the panel, modal, over the full draft; Cerrar in WORKSPACE with one question when a staged module and unapplied changes would be lost together; the summary and Modificado after closing). STOP and return to the Owner.
PBT4C-C04: a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.
PBT4C-C05: a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or writing to the profile or to disk.
PBT4C-C06: the GREEN CI fails outside the new tests, reproducibly and not attributable to them.
PBT4C-C07: a fact of section 2 does not hold in a way that changes a CT or the design; or an expected line is not met in the GREEN; or a self-checking recipe (the self-tests of CT-PBT4C-07, CT-PBT4C-13 and CT-PBT4C-15) does not exercise its path. The Worker does not adjust what is expected; except under F4B-E35 (the RED repair rule, RR): a writing error in a new CT of this task (one of its five classes; never the four equivalents, whose text is the package one), found after the RED push and before the GREEN, is repaired without this stop and without spending an attempt by one declared RED repair commit that touches only test files the RED added and changes only the when or the how of a reading or a gesture the package already prescribes, and no expected line, literal or oracle of the package or the contract, with the full RED criterion rerun on RedRepairSha; a second writing error in the same attempt, a repair that needs an expected line changed, or a literal line the package fixes for the repaired class (an expected line or a literal) that does not stay byte for byte in it, is this stop. An equivalent of the list that fails in the GREEN, or one of the three of Core that fails in the RED, is this stop too: the session issues a package version with its text.
PBT4C-C08: a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner
PBT4C-C09: a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.
PBT4C-C10: doing the task would require what the scope of PB-T4c excludes: retiring or editing the code of the pilot (Pilot/); touching the classic editor, RACKEDITAR, the classic path or the menu; the window in CLASSIC; creating views; translating a state; a member of the edit port other than TryEditAll; a new value of PilotBoundaryKind; reading or writing the drawing from the window or after it closes; or touching a test of the list other than by its Skip.
PBT4C-C11: a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner ([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, and nothing else) -> substitute PBT4-NC2-EXACT-WHITELIST-F4B-01, the version 2 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py, SHA-256 c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45; owner-pbt3a-nc2-decision.md, "Aceptar y ampliar el sustituto"), whose property 8 admits only the closed list of the exact literals of the evidence entry of the byte-by-byte comparison, one per package (PB-T2 and PB-T3), with its reviewed text unchanged and run with --task PBT4 in the F4B-E21 mode with the injection note and the raw output of this task's accepted planning, recorded as nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B; if inject_opaque.py changed since that run, the substitute is reviewed before it is reused; (2) nc1 with the signature PD-F4B-01, the false SHA cascade (F4B-E40: owner-pbt2c-nc1-decision.md, its conditions (a) to (f): the mutation only changes the CurrentSha of the handoff to a SHA that does not exist in the repository; EXECUTION_BLOCKED / STOP with S-04 among the stops; Handoff is the first check in fail; the failing checks are a subset of Handoff, Identity, Remote, Scope, Ci and Tests that always includes Handoff and Identity, and all the others pass as in the real verification; each failing evidence cites the false SHA and none attributes the failure to another cause; the Identity evidence also writes the real HEAD), of which the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265; as applied in F4, f4-dispositions.md:1657-1660) is a particular case -> substitute PBT3x-NC1-CASCADE-SIGNAL-01 (F4B-E40 and F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py, SHA-256 41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40, reviewed APTO, with its declared limits), with its reviewed text unchanged and run with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md) and, for PD-F4B-01, its conditions (a) to (f); the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or Trailer also in fail, any nc1 that does not meet PD-F4B-01 - the one-off acceptances of nc1 in PB-TC (F4B-E08), in PB-TC2 attempt 1 (F4B-E12), in PB-T2a (F4B-E34), in PB-T2b (F4B-E38) and in PB-T2c (F4B-E40) do not extend -, an nc2 that the version 2 of the substitute does not give PASS - the disposition of the Owner on the nc2 of PB-T3a (F4B-E43) does not extend -, a substitute that does not give PASS, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner before any substitute or acceptance; no one-off acceptance extends to PB-T4c
PBT4C-C12: a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)
PBT4C-C13: doing the task would require touching a file of src/ other than the 13 of the AllowedWriteScope; or adding a declaration of any visibility (type, constructor, method, property, field, constant, enum member, nested type or x:Name; an overload is one more) whose name is not in this closed list: in IWorkspacePilotEditPort.cs, the method TryEditAll of the interface, with its default body; in WorkspaceEditIntent.cs, the enum EditAllStatus with its seven members (NotAvailable, Waiting, Rejected, NotBegun, Refused, Closed, Failed) and the struct EditAllOutcome with its private constructor, its properties Status, Reason, Begin and Applied and its static methods NotAvailable, Waiting, Rejected, NotBegun, Refused, Closed and Failed; in WorkspaceHostCore.cs, the private constants EditAllNoEditorReason, EditAllPendingTextReason, EditAllBusyDraftReason and EditAllStateReason, the private field _fullEditHost, the public method SetFullEditHost, the explicit implementation IWorkspacePilotEditPort.TryEditAll and the private method EditAllOfPilot; in WorkspacePilotSupport.cs, in WorkspacePilotEditGate, the public method TryEditAll; in WorkspaceController.cs, in PilotEditAdapter, the public method TryEditAll, and the private nested class FullEditorModal with its private fields _controller and _open, its public constructor and its public method ShowModal; in WorkspacePilotFieldViewModel.cs, the private constants EditAllNotOpenedPrefix, EditAllAppliedText and EditAllNothingAppliedText, the property ShowEditAllButton, the method EditAll and the private methods EditAllNoticeOf and SentenceOf; in WorkspacePanelView.xaml, the Button BrowserFieldEditAllButton; in WorkspacePanelView.xaml.cs, the private method OnBrowserFieldEditAllClick; in RackPushBackSystemWindow.xaml.cs, the private constant WorkspaceStagedAndUnappliedQuestion; in IWorkspaceFullEditHost.cs, the interface IWorkspaceFullEditHost with its method ShowModal; in PushBackFullDraftLaunch.cs, the static class PushBackFullDraftLaunch with its method For and its private method ProjectOf; in PushBackFullDraftStatePort.cs, the class PushBackFullDraftStatePort with its private fields _draft and _applied, its constructor and its method ApplyToDraft; in RackWorkspaceEditorWindow.cs, the static class RackWorkspaceEditorWindow with its method Show. Prescribed changes that are not new declarations: WHC changes no existing body; FVM changes RaiseAll; PendingWork of the window gains its first if; the getter Navigation of WCo gains the call to SetFullEditHost. The Worker stops before adding anything else.
PBT4C-C14: a design of the corpus changes its admission or its lock (the pilot reader, the full reader, AUTH-13 or the parked tail).
PBT4C-C16: an existing test outside the closed list of substitutions of PB-T4c (the 4 of pbt4c-substitution-list-4.tsv) fails, including one that reads the text or the types in a way only seen when it runs. It returns to the Owner (owner-pbt4-decisions.md, question 4). An equivalent of the list that fails where the package does not expect it is not this stop: it is PBT4C-C07.
PBT4C-C17: (F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." The Worker chooses none of the three exits: the Owner does
PBT4C-C18: a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in the attempt; it touches a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope; it changes an expected line, literal or oracle, or anything other than the when or the how of a reading or a gesture the package already prescribes (RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP.

ExpectedEvidence:
[0] antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;
[1] el commit RED y su push antes del GREEN; con una reparacion del RED (F4B-E35, RR-4), ademas, el commit "RED repair" y su push antes del GREEN, con su CI, registrada como la del RED reparado, y los TRX de cada filtro con ExpectRed sobre RedRepairSha;
[2] la CI del RED como en section 6.0 (tests y ui-tests en failure, build-ui en success, build-plugin omitido, sin cuelgue); la del RED reparado, si lo hay, da lo mismo;
[3] la tabla "CT -> la asercion que falla en el RED", con el equivalente de CT-PBT2B-06 (PushBackEditAllApplyCloseTests), que falla por asercion en el RED, y las autopruebas de CT-PBT4C-07, 13 y 15;
[4] el GREEN y su CI 4/4;
[5] en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED, sobre RedRepairSha si hay reparacion, y sobre el GREEN, y tres corridas en verde de los filtros con ExpectRed sobre el GREEN;
[6] la entrega con el mapa CT -> metodo, las rutas relativas comprobadas, el sha256 de cada uno de los cuatro equivalentes (iguales a EquivalentSha) y el Trailer real de los commits RED, "RED repair" (si lo hay) y GREEN; con una reparacion del RED, ademas, RedRepairSha, la prueba y la linea corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y RedSha, que sigue siendo el commit RED original (F4B-E35, RR-5);
[7] la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3;
[8] con una reparacion del RED, la verificacion la lee como parte de la entrega (F4B-E35, RR-7; section 6.0): n.o 10 (Tests): el RED se acredita con RedSha y con la CI del RED original, y sus TRX, con los filtros con ExpectRed entre los fallidos. La clausula "git diff --name-only RedSha CurrentSha no toca RT U ChainRedFiles" se comprueba desde RedRepairSha: git diff --name-only RedRepairSha CurrentSha. n.o 11 (Trailer): el trailer va tambien en el commit "RED repair". Lo que el Controller comprueba de RR (si algo falla, es C18): RR-1, un solo commit "RED repair", hijo directo de RedSha y anterior al GREEN; RR-2, git diff --name-only RedSha RedRepairSha dentro de las cinco clases de las CT; RR-4, la CI de RedRepairSha, registrada como la del RED reparado, y los TRX de los filtros con ExpectRed sobre el; RR-5, la declaracion de la entrega, con el diff, y RedSha, el commit RED original; RR-6, con el tramo RedSha..RedRepairSha. En PB-T4c, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto), ni los cuatro equivalentes, ni los cuatro archivos de los Skip. La ayuda mecanica de RR-3: es parada C07, una violacion cierta de RR-3, que un literal de la lista cerrada de la clase reparada (section 6.0 del paquete, la tabla de RR-3) cambie entre RedSha y RedRepairSha: una linea o un fragmento, si no aparece como subcadena exacta el mismo numero de veces; un valor de gesto, si no aparece como token el mismo numero de veces. Indicios para la revision, no parada: una cadena <campo>= anadida o quitada en git diff -U0 RedSha RedRepairSha, o un cambio en el primer argumento de un Assert.Equal. Pueden dar falsos positivos, como una linea que solo se mueve de sitio, asi que van a Findings y a la revision, con la declaracion de RR-5.
[9] la CI del RED con tests y ui-tests en failure, build-ui en success y build-plugin omitido;
[10] la del GREEN 4/4, con el Plugin compilado;
[11] el censo de declaraciones de WHC, IWorkspacePilotEditPort.cs, WorkspacePilotSupport.cs, WEI, FVM, WorkspacePanelView.xaml, WorkspacePanelView.xaml.cs, WCo y RackPushBackSystemWindow.xaml.cs, y de los cuatro archivos nuevos, en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es una declaracion mas): la diferencia es la lista cerrada de PBT4C-C13;
[12] el Trailer real;
[13] la entrada "## PB-T4c" de superseding-characterization.md, escrita por la sesion en el commit de emision: la tabla de las 4 (PEG:155-163, CT-PBT1B-15 y CT-PBT4B-20, C-5, en Core; CT-PBT2B-06, C-3, en UI), el sha256 de la lista, los motivos literales de los Skip, el equivalente de cada una con su EquivalentSha y su cambio declarado, y las decisiones del Owner (owner-pbt4-decisions.md, pregunta 4; owner-pbt4c-list-3-decision.md; owner-pbt4c-list-4-decision.md); su tabla de ejecucion se completa al cerrar;
[14] Linea base de la CI: la del cierre de PB-T4b (F4B-E53, 6adff90d): run 44903818806 sobre su GREEN c065080d, Core 19 605 resultados, con 19 147 Passed y 458 Skipped, y UI 2 481, con 2 433 Passed y 48 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4c pone 4 Skip (3 en Core y 1 en UI), cada uno sobre una prueba que pasa a un solo resultado omitido, anade 4 equivalentes (3 en Core y 1 en UI), un [Fact] cada uno, y 15 CT (11 de Core y 4 de UI), sin ninguna Theory nueva. El GREEN: Core 19 619 resultados, con 19 158 Passed, 0 Failed y 461 Skipped; UI 2 486, con 2 437 Passed, 0 Failed y 49 Skipped. El RED: Core 19 619, con 19 149 Passed, 9 Failed y 461 Skipped; UI 2 486, con 2 432 Passed, 5 Failed (las 4 CT de UI y el equivalente de CT-PBT2B-06) y 49 Skipped.
[15] la comprobacion mecanica de los Skip, check_skips_pbt4c_v50.py, sobre RedSha y sobre CurrentSha frente a BaseSha: los cuatro archivos, iguales a su base con su Skip literal y nada mas (4 Skip).
```

**Los `Skip` literales** (`pbt4c_skips_v50.py`, sha256 `a59a3aedc982bb15312a04eb5ddbe92afa2a868a1177e7c8b4c3f49332970f7f`; ASCII), cada uno en la línea de atributo `[Fact]` de su prueba en `6adff90d`:

- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`:155 (`ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot`): `[Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]`
- `tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs`:306 (`CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses`): `[Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllNameWritersGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]`
- `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs`:568 (`CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList`): `[Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4c-list-3-decision.md).")]`
- `tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs`:686 (`CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied`): `[Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllApplyCloseTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4c-list-4-decision.md).")]`

**Los equivalentes, literales** (`make_equivalents_pbt4c_v50.py`, sha256 `d81fcb41bed684b8ac35949841e3a7fba2277120bdf3e882d0267f8176e4a896`, desde los originales de `6adff90d`; su texto, en el anexo V51-A; `EquivalentSha`, del texto con `\n`):

- `tests/RackCad.Tests/PushBackEditAllPortEquivalenceGuardTests.cs`, `1a2607d3a35ae8f9737fffb0eb32a4acd8100318fe83e5b0098d30428f57d250`: PEG:155-163, con la fila de `TryEditAll` al final de `Operations` (18); Core, `ExpectRed` false
- `tests/RackCad.Tests/PushBackEditAllNameWritersGuardTests.cs`, `3744992127be8b7e1d8d822a769b8eb2a6886789c73285c8436e5baec6bf7b6b`: CT-PBT1B-15, con «archivos=15»; Core, `ExpectRed` false
- `tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs`, `4619b6d310340f9c97a7d9f31bda1db852d2fcda905b91bb857510f7e6b7e6ce`: CT-PBT4B-20, con la clase renombrada y `Restore`, que además devuelve la línea exacta del `Skip` de PB-T4c de PEG:155; Core, `ExpectRed` false
- `tests/RackCad.UI.Tests/PushBackEditAllApplyCloseTests.cs`, `7c2458260ef301d7f05bfaff8e1e8c4a4a618f25fd7022705d55dee32dbe78d2`: CT-PBT2B-06, con la línea de la ventana 4 a la pregunta combinada; UI, **`ExpectRed` true** (cae en el RED, V50-18)

**El nombre del equivalente de PEG:155-163** (V51-03; CA-T4c M-02) conserva el de su original, `ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot`, aunque su tabla tenga dieciocho operaciones: es la copia literal (`owner-pbt4-decisions.md`, pregunta 4), y lo que fija es la tabla, no el nombre (como la §20 M-02 de CA-T4b con las copias de D-S1).

**La precondición 6** (I-02; §11.6), que corre la sesión antes de emitir, sobre copias exportadas del cierre de PB-T4b, fuera de todo commit (`pbt4ccc/compile/`, que la emisión custodia en `F4B-DRAFTS/pbt4/cc4c/`; el resumen, `pbt4c-precondition6.json`, sha256 `29179a12ef8623f4c276f943a725ece6f3ffd84464f16705e1feda757efbce1a`, y su registro, `compile-check.md`, `060c5c8b4ea1197079bcde5f5c5d8d299d5b71dde6295d2a468982e5fb1bb02c`):
1. `git archive` de `6adff90d` a una carpeta nueva, fuera del worktree;
2. `python -I apply_red_pbt4c_v51.py <copia> --repo <checkout> --rev 6adff90d`: el esqueleto, los 4 `Skip` y los 4 equivalentes, sin las CT;
3. la compilación de Core y de UI en Debug y del Plugin en Release con `-o`, sin ningún `warning CS`;
4. `check_skips_pbt4c_v50.py` sobre la copia: los 4 archivos, iguales a su base con su `Skip`; y los filtros (`filter_overlap_pbt4c_v50b.py`, D-S8);
5. **el RED exacto, sin las CT:** la suite completa de Core, **19 608 = 19 147 + 0 + 461**, con las 3 de Core de la lista omitidas con su motivo y los 3 equivalentes de Core en verde; y los 18 filtros de UI (los 17 heredados y el del equivalente de UI), donde **solo cae el equivalente de CT-PBT2B-06, por aserción**, y CT-PBT2B-06 está omitida con su motivo (D = 0);
6. **sobre la misma copia, el GREEN literal** (`apply_green_pbt4c_v51.py`): la suite completa de Core, **19 608 = 19 147 + 0 + 461**, y los 18 filtros de UI, **0 `Failed`**, sin ningún `warning CS`. **Una prueba que falle fuera de la lista de 4** vuelve al Owner; **un equivalente que no pase donde se espera** necesita una versión del paquete;
7. **las sondas, sobre el GREEN,** como clases temporales que no entran en ningún commit y acaban en `Assert.Fail(texto)`, cada una con su filtro: `pbt4c-probe-editall.cs` (las líneas de CT-03, 04, 05 y 12) y `pbt4c-probe-close-ui.cs` (CT-09 y D-S10); y el coste de `IsDirty` con la sonda de PB-T4b (`pbt4bcc/compile/TmpIsDirtyCostProbe.cs`), en Release;
8. **el ensayo de las CT de texto y el censo de C13** (V51-05; nuevo en la v5.1, por CA-T4c I-01):
   - **el ensayo:** `pbt4c-rehearsal-textct.cs` (sha256 `ede5d822d30e8d5dc022f124a2a42f4e7cb562f53de7862be11b2d7b0b0e13ba`), cinco clases temporales (`TmpPbt4cTextCt01`, `07`, `08`, `13` y `15`) que no entran en ningún commit, cada una con un `[Fact]` que acaba en el `Assert.Equal` de sus líneas literales de §5.3. Las escribe `v51/make_rehearsal_v51.py` (`d7db45cb78b5769196edecc737537105855d5d680850a9765e64118ea3fcf60e`): CT-15 con G-L1 y la pureza de `PushBackPanelDraftGuardTests.cs` (CT-PBT4B-19) y CT-13 con `ControllerViolations` de PEG (PEG:659-707), copiados de los objetos git de la base; CT-01, 07 y 08, desde su fila de §5.3. **Sobre el RED,** CT-07 y CT-15 pasan y CT-01, 08 y 13 caen por su `Assert.Equal`; **sobre el GREEN,** las cinco pasan (`rehearsal_check_v51.py`);
   - **su control:** el mismo ensayo sobre las piezas de la v5.0 (`p6_control50_v51.sh`) **ve I-01**: CT-15 cae en las dos ramas, con su primera diferencia en la línea de la pureza, y las otras cuatro dan lo mismo que con la v5.1 (`control_check_v51.py`);
   - **el censo** (`census_c13_v51.py`, `83a88e3895716dfe8810d360b20c8bce1751a178356cd90591e308a5844343cc`), por texto y por objetos git: la diferencia de declaraciones del GREEN frente a la base es, nombre a nombre y con su cuenta, la lista cerrada de PBT4C-C13, la del esqueleto es una parte de ella, nada se quita, y ningún nombre nuevo de `RackCad.Application.Workspace` cae en las reglas de `WorkspaceModelBoundaryTests:83`, :101 y :102; con siete autopruebas (una constante, una sobrecarga, una propiedad, un miembro de enumerado, un tipo anidado y un `x:Name` de más, y un campo de menos);
9. el contrato lleva por sha256 las piezas del GREEN y del esqueleto, los 4 equivalentes (`EquivalentSha`) y la lista.

**Qué cubre el paso 8 y qué no** (V51-05):
- **Cubre, literal y en las dos ramas,** las cinco CT de texto sobre `src/` (CT-01, 07, 08, 13 y 15: todas las de PB-T4c que leen texto), con su `ExpectRed`; y, por texto, la lista cerrada de C13 y los nombres.
- **Cubren las sondas, solo sobre el GREEN,** las líneas de CT-03, 04, 05 y 12 que dice §5.3 (22 tal cual, 9 con sus campos y 3 derivadas) y las de CT-09.
- **No cubre** CT-02, 06 y 14 (la FVM con un doble del puerto, en UI), CT-10 (de punta a punta, con `PilotEraseScopeDouble`) ni CT-11 (las huellas de D2), ni las cuatro líneas de CT-03 y 04 que salen del texto, ni el RED de las CT de comportamiento.
- **Por qué esas no se escriben literales:** §5.3 fija sus líneas y lo que hacen, no cómo se construyen (los fixtures de la cola, del conflicto y del error, el doble del borrado, las huellas). Un ensayo sería una versión de la sesión, no la CT del Worker. Lo que el ensayo no ve lo ven la CI del RED (C04) y la del GREEN (C07).

**Lo medido en la v5.1** (V51-06): `/d/r5j` para los pasos 2 a 6, `/d/r5i` para las sondas del 7 y el ensayo del 8, `/d/r5k` para el coste y `/d/r5l` para el control del ensayo. Lo previsto, con D = 0, K = 4, E = 4 y cero `warning CS`:
- **Core, sin las CT:** 19 608 = 19 147 + 0 + 461 en las dos ramas; **UI,** los 18 filtros: en el RED solo cae el equivalente de CT-PBT2B-06, por aserción, y en el GREEN, 0 `Failed`;
- **el ensayo:** PASA en las dos ramas; **su control sobre la v5.0:** ve I-01; **el censo:** PASA; **las líneas de las sondas:** las mismas que en `/d/r5f`, por sha256;
- **la duración de la suite de Core:** 736 s en el RED y 622 s en el GREEN;
- **el coste de `IsDirty`:** el de PB-T4b, dentro del ruido (pb-20-ab, mediana limpia 9.7677 ms, frente a 9.35 ms en PB-T4b y 9.64 ms en la v5.0).

**Las corridas de la v5.0** (`/d/r5f`, `/d/r5g` y `/d/r5h`) y la vuelta parada (`/d/r5a` a `/d/r5e`, con la medida de D-S9 en `/d/r5e`) son historia: las piezas que midieron no son las de la v5.1.

**`RequiredTests`** (76 heredados + 9 nuevos = 85). Los heredados son los 77 del contrato de PB-T4b (`F4B-PBT4-FULL-DRAFT-PANEL`, emitido), con su número, **todos con `ExpectRed` false** (las CT, las clases Red de la partición y sus filtros pasan desde el cierre de PB-T4b), y sus `MinSelected` bajan en una por cada prueba de la lista que seleccionan; **el n.º 43 queda en 0 y su fila sale** (V50-16). Los nuevos, del 78 al 86, por clase exacta (D-S8):

| # | Project | Filter | MinSelected | ExpectRed |
|---|---|---|---|---|
| 1 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests` | 4 | false |
| 2 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests` | 1 | false |
| 3 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackViewEraseGuardTests` | 4 | false |
| 4 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 | false |
| 5 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateGuardTests` | 85 | false |
| 6 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | 121 | false |
| 7 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackSharedReferenceGuardTests` | **1** | false |
| 8 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 | false |
| 9 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullWrite` | 13 | false |
| 10 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 | false |
| 11 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathGuardTests` | 1 | false |
| 12 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | **2716** | false |
| 13 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullDraft` | 10 | false |
| 14 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests` | 2 | false |
| 15 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 521 | false |
| 16 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditableState` | 13 | false |
| 17 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate` | 1 | false |
| 18 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 | false |
| 19 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 | false |
| 20 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 | false |
| 21 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false |
| 22 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 10 | false |
| 23 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 16 | false |
| 24 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 | false |
| 25 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet` | 14 | false |
| 26 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 14 | false |
| 27 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 | false |
| 28 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.I58F1` | 10 | false |
| 29 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 9 | false |
| 30 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.NamespaceFolderGuardTests` | 7 | false |
| 31 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftStatePortTests` | 3 | false |
| 32 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 | false |
| 33 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 | false |
| 34 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 | false |
| 35 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 | false |
| 36 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | **3** | false |
| 37 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 | false |
| 38 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 | false |
| 39 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelTests` | 11 | false |
| 40 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelGuardTests` | 4 | false |
| 41 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelCensusGuardTests` | 1 | false |
| 42 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPanelDraftGuardTests` | 1 | false |
| 44 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullPanelFieldTests` | 3 | false |
| 45 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldValidationTests` | 13 | false |
| 46 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldTests` | 10 | false |
| 47 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostRedTests` | 24 | false |
| 48 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostGreenTests` | 69 | false |
| 49 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareRedTests` | 44 | false |
| 50 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests` | 47 | false |
| 51 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostRedTests` | 24 | false |
| 52 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostGreenTests` | 57 | false |
| 53 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditHostRedTests` | 59 | false |
| 54 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditHostGreenTests` | 17 | false |
| 55 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorRedTests` | 6 | false |
| 56 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorGreenTests` | 70 | false |
| 57 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestRedTests` | 53 | false |
| 58 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestGreenTests` | 1 | false |
| 59 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostRedTests` | 29 | false |
| 60 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostGreenTests` | 18 | false |
| 61 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostRedTests` | 23 | false |
| 62 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostGreenTests` | 5 | false |
| 63 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingRedTests` | 1 | false |
| 64 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingGreenTests` | 21 | false |
| 65 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackWritePathGuardRedTests` | 1 | false |
| 66 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateIntegrationGuardRedTests` | 2 | false |
| 67 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentGuardRedTests` | 13 | false |
| 68 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardRedTests` | 6 | false |
| 69 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardGreenTests` | 17 | false |
| 70 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationGuardGreenTests` | 1 | false |
| 71 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleGuardRedTests` | 2 | false |
| 72 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateGuardRedTests` | 1 | false |
| 73 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests` | 30 | false |
| 74 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests` | 6 | false |
| 75 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadPathRedTests` | 1 | false |
| 76 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadGuardRedTests` | 1 | false |
| 77 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardRedTests` | 1 | false |
| 78 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllTests` | 6 | true |
| 79 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllGuardTests` | 3 | true |
| 80 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackOpenerClassicPathGuardTests` | 1 | false |
| 81 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftLaunchGuardTests` | 1 | false |
| 82 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllWindowTests` | 4 | true |
| 83 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests` | 1 | false |
| 84 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllNameWritersGuardTests` | 1 | false |
| 85 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllFullCopyGuardTests` | 1 | false |
| 86 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllApplyCloseTests` | 1 | true |

- **Nota (no va al contrato).** `MinSelected` cuenta resultados no omitidos, como en el contrato de PB-T4b (medido en las TRX de `/d/r5j`: n.º 5, 85 resultados de 40 métodos). **Los cuatro que bajan:** n.º 7, `PushBackSharedReferenceGuardTests` (CT-PBT1B-15), de 2 a **1**; **n.º 12, `RackCad.Tests.Workspace`, que selecciona PEG y `WorkspaceFullCopyGuardTests`** (PEG:155-163 y CT-PBT4B-20; la duda del generador decía que PEG no caía en ningún filtro heredado, y no es así: el filtro es una subcadena del nombre completo `RackCad.Tests.WorkspacePilotEditGuardTests…`), de 2 718 a **2 716**; n.º 36, `PushBackWorkspaceApplyCloseTests` (CT-PBT2B-06), de 4 a **3**; y n.º 43, `WorkspaceFullCopyGuardTests`, de 1 a 0, así que sale. **La prueba de PEG de la lista no necesita filtro nuevo:** su equivalente tiene el suyo (n.º 83) y el n.º 12 sigue cubriendo las demás de PEG. Ningún filtro heredado selecciona una clase nueva, y cada filtro nuevo solo la suya (`filter_overlap_pbt4c_v50b.py`; `pbt4ccc/compile/r5j-rg/filtros.txt`).

#### 6.3.2 Lo que necesita el generador de PB-T4c (las dudas de la fase 1, `generator-doubts-pbt4c.md`)

| Pieza | Dónde, en este paquete | Lo que comprueba el generador |
|---|---|---|
| **`TaskId`** | §6.3.1 | `F4B-PBT4-EDIT-ALL-WINDOW`, 24 caracteres, ≤ 26 |
| **`Objective`** | §6.3.1 | ASCII, byte a byte; cita el paquete v5.1, CT-PBT4C-01..15 y las **4** sustituciones |
| **`AllowedWriteScope`** | §6.3 | Las 26 rutas, en ese orden y con su número; ninguna de `Pilot/`; de `src/RackCad.Plugin/`, solo WCo y `Views/RackWorkspaceEditorWindow.cs` |
| **`ForbiddenWriteScope`** | §6.0 y §6.3 | Su regla, sobre el alcance final en `6adff90d` (`<medido por el generador>`) |
| **`UNIT_CHANGE`** | §6.0 y §6.3 | Las 9 existentes de `src/` |
| **`Invariants`** | §6.3.1 | INV-PBT4C-01 a 14, literales |
| **`StopConditions`** | §6.0 y §6.3.1 | Las 17 estándar del contrato de PB-T4b y PBT4C-C01 a C14, C16, C17 y C18: **34**. **C11** es `PBT4B-C11` con «PB-T4b» → «PB-T4c». **C02** no tiene excepción fuera de los `Skip` de la lista de 4 |
| **`RequiredTests`** | §6.3.1 | Las 85 filas: los 76 heredados con su número y sus `MinSelected` (n.º 7: **1**; n.º 12: **2 716**; n.º 36: **3**; el n.º 43 sale), todos con `ExpectRed` false, y los 9 nuevos (n.º 78 a 86), por clase exacta (D-S8): 5 de las CT y 4 de los equivalentes |
| **`ExpectedEvidence`** | §6.3.1 | 16; `[0]` byte a byte el de PB-T3c; `[13]`, la entrada «## PB-T4c» del registro declarado; `[14]`, con los totales de §5.3 y la base de PB-T4b; la cuenta de CT (**15**), de `[Fact]` (15), de `Skip` (**4**) y de filtros con `ExpectRed` true (**4**: 2 de Core y 2 de UI, con 14 resultados: 9 de Core y 5 de UI) |
| **La lista** | `pbt4c-substitution-list-4.tsv` | sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de`; 4 filas (3 C-5 en Core y 1 C-3 en UI), cada una un `[Fact]` sin `Skip` en su rango en `6adff90d`; la genera `make_list_pbt4c_4.py` (`02096c2ae404238263759cc88854205f922e7c1980fec5ce224a70761a6eb777`). La de 3 (`d83e0a817144fa99f95ed4bfd151b822a4fc1035c10f736f77e8f8e6350186c1`) y las filas «PB-T4c» de `pbt4-substitution-lists.tsv` quedan como historia |
| **El esqueleto y el GREEN** | `pbt4c-skeleton-v51.txt` y `pbt4c-green-v51.txt`, con `green_pbt4c_v51.py` | **Por su texto en el paquete**, no como parche: dos piezas de texto sin escapes, con bloques `EDIT` (un `OLD` que aparece exactamente una vez y su `NEW`) y `NEWFILE`, y `FROMGREEN` en el esqueleto; el generador las aplica sobre `6adff90d` y comprueba que cada `OLD` está una vez. sha256: `f8840c67fd41c3bbdabf125d9b80941e3f3aedf5e610681c8ce15f111e7b392e`, `081ef77e66a390cf3637f15851b5fe05a5cdf07f6e0065928fc5c841b250dc16` y `a8a3afa2a0a71674ccd7eafc1f4e52eea183fef7cbf8bc5fd319be9dfb639215`. Su texto, en el anexo V51-A |
| **Los `Skip`, los equivalentes y su comprobación** | §6.3.1 | Por sha256: `pbt4c_skips_v50.py` (`a59a3aedc982bb15312a04eb5ddbe92afa2a868a1177e7c8b4c3f49332970f7f`), `make_equivalents_pbt4c_v50.py` (`d81fcb41bed684b8ac35949841e3a7fba2277120bdf3e882d0267f8176e4a896`), `apply_red_pbt4c_v51.py` (`03dbb6a76d1e50c7c1935926727e4a5172f44c30a3cc6bc11ddac51197bd5102`), `apply_green_pbt4c_v51.py` (`8b9445455b60521598a113e14e870fe5218d3b1b4b142308fbac934016d9bd49`) y `check_skips_pbt4c_v50.py` (`71521e52abdd7f371fb4b067d2b4400bb508c2d1d6a246ca83df4bb39127ee22`); los 4 `EquivalentSha` |
| **Las CT como datos** | `pbt4c_ct_table_v51.py` | sha256 `5ee03f8ebe411b1bf689102753ff93f77c1988266432cf9ea2f3e46df116623f`: la tabla de §5.3, que lee la lectura RR-3 de la cadena. **Pasada sobre esta versión** (`pbt4ccc/pbt4c_rr3_reading.py`, `7585a87e…`, sin tocarla; `v51/check_rr3_v51.py` y su salida, sobre la v5.1): 0 errores; las cinco clases de las CT, con sus líneas iguales a las de sus CT y nada pendiente; los cuatro equivalentes, sin fila (no admiten reparación) |
| **El ensayo y el censo** (el paso 8) | `pbt4c-rehearsal-textct.cs` y `pbt4ccc/compile/census_c13_v51.py` | sha256 `ede5d822d30e8d5dc022f124a2a42f4e7cb562f53de7862be11b2d7b0b0e13ba` y `83a88e3895716dfe8810d360b20c8bce1751a178356cd90591e308a5844343cc`; el generador del ensayo, `v51/make_rehearsal_v51.py` (`d7db45cb78b5769196edecc737537105855d5d680850a9765e64118ea3fcf60e`); su lectura, `rehearsal_check_v51.py` (`772f35eebc2589bfdd3d64adef7183b6109044c4d3ac8bf7f1cadc2054b20428`); su control, `p6_control50_v51.sh` (`612b80ab7a60969c772b9f3c58ca50f7cd01c1890aa1ccfa334570142ed97dd3`) y `control_check_v51.py` (`ec74af4484ae7496fc4e8f0258a71a2f88b124dd42933b5734314d4682384262`). **No van al contrato:** son de la sesión, como las sondas |
| **La precondición 6** | `pbt4ccc/compile/pbt4c-precondition6.json` | Schema `rackcad-pbt4c-precondition6/v1`; `Revision` (`6adff90d…`), `List` (su sha256, K = 4: 3 de Core y 1 de UI), `Pieces` (el sha256 de cada pieza), `Skeleton`, `GreenFiles`, `SkipFiles` y `EquivalentSha` (el sha256 de cada archivo escrito), `Builds` y `CsWarnings` (0), `Red` y `Green` (los totales de Core y cada filtro de UI con su TRX), `D` (0), `E` (4), `SuiteDurationSeconds`, `InheritedCoreFiltersGreen` (los resultados y métodos de cada filtro heredado), `FilterOverlap`, `CheckSkips`, `Probes`, `IsDirtyCost` y `History` (las corridas de la v5.0 y la vuelta parada); **y, desde la v5.1, con el mismo Schema** (las claves nuevas no cambian las de la v1): `Rehearsal` (el ensayo en cada rama), `Control` (el ensayo sobre la v5.0), `Census` (el censo de C13) y `ClassResults` (el sha256 de las tablas de cada clase). sha256 `29179a12ef8623f4c276f943a725ece6f3ffd84464f16705e1feda757efbce1a`; la corrida, en su `compile-check.md` (`060c5c8b4ea1197079bcde5f5c5d8d299d5b71dde6295d2a468982e5fb1bb02c`), **con el resultado de cada clase que corre** (Passed, Failed y Skipped). La carpeta va a `F4B-DRAFTS/pbt4/cc4c/`; **las salidas de la v5.1 tienen nombres cortos:** ninguna ruta pasa de 150 caracteres con ese prefijo (V51-07) |
| **CA-T4c** | `independent-review-v1.md` | **`BOUNDED_SECTIONS`: la §22** (la comprobación acotada de PB-T4c sobre la v5.0: EMITIBLE TRAS CORRECCIONES, 0/1/3) **y la §23** (su recomprobación, sobre esta versión, la v5.1). **La línea `BOUNDED_RESULT`:** como en PB-T4b (`make_pbt4b_contract.py:53-60`), la línea BOUNDED literal de la §22.10, en ASCII, con la cola de la recomprobación de la §23 (`; recomprobacion v5.1: EMITIBLE, B 0 / I 0 / M <n>`); el generador la comprueba con `bounded_check` contra esas secciones. La emisión necesita 0 bloqueantes y 0 importantes sin resolver (pregunta 4) |
| **La numeración** | — | F4B-E53 (el cierre de PB-T4b), F4B-E54 (la emisión) y las secciones 283 (X) y 284 (BASE) |

**Las autoridades de PB-T4c** (la n.º y la ruta son las del contrato de PB-T4b emitido, con sus 72):
- **Siguen, sin cambio:** las n.º 1 a 5, 7 a 27 y 30 a 35, y 38 a 42.
- **Siguen, con su sección al día:**
  - la n.º 0, el paquete, en la v5.1, con sus secciones de PB-T4c (D-PBT4-12 a 17, §4.3, §5.3, §6.0 y §6.3);
  - la n.º 6, `superseding-characterization.md`, con su entrada «## PB-T4c» (`[13]`);
  - la n.º 28, `f4b-dispositions.md`, hasta F4B-E53;
  - la n.º 29, el contrato anterior: el de PB-T4b (`F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json`), en lugar del de PB-T3c;
  - la n.º 36, la base: el cierre de PB-T4b (`F4B-PBT4-FULL-DRAFT-PANEL/task-close-package.md`, F4B-E53, con la CI de la línea base, `44903818806`), en lugar del de PB-T3c;
  - la n.º 37, «este contrato»: `F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json`;
  - la n.º 65, `independent-review-v1.md`, con su §22 (CA-T4c) y su recomprobación, la §23.
- **No son autoridades de PB-T4c:** las n.º 43 a 64 (las tandas, la lista de 478, el mapa, el kit, los guiones de la copia, su revisión y la precondición 6 de PB-T4b) y las n.º 66 a 71 (los `UNIT_CHANGE` de PB-T4b).
- **Se añaden:**
  - `owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md` (la octava y la novena tandas, sobre la lista de PB-T4c);
  - `session-pbt4c-design-decisions.md` (D-S7 a D-S10);
  - `owner-pbt2b-close-decisions.md` (F4B-E38; A-T2b-OWNER-01, :36);
  - `owner-pbt4b-code-decisions.md` (F4B-E53; los vacíos de M-04 van a la lista de PB-T4d, no a esta);
  - `pbt4c-substitution-list-4.tsv`, las piezas y los guiones de §6.3.2 y `pbt4c-precondition6.json` (UNIT_DOC);
  - y los 9 `UNIT_CHANGE` de `src/` (el texto literal del GREEN de PB-T4c, `pbt4c-green-v51.txt`).

### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT` (entero; V52-01 a V52-08; V53-01 a V53-11; V54-01 a V54-07)

**Se emite por la autorización condicional de PB-T4d** (`owner-pbt4d-list-decisions.md`, pregunta 4: «Sí, con condiciones (Recomendado)»): si la comprobación acotada da 0 bloqueantes y 0 importantes, la remedición no cambia nada material y la lista es la elegida o una parte. **La lista** es la del alcance B (pregunta 1), con las sustitutas de `owner-pbt4d-substitutes-decision.md`, las 29 de WPUR (`owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md`, pregunta 2, y `owner-pbt4d-cat4d-decisions.md`, pregunta 1), la rama muerta de `BuildCore` (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 1) y lo que la regla del Owner de `owner-pbt4d-cat4d-decisions.md` (pregunta 2) y la sesión añaden tras la comprobación acotada CA-T4d (M-01 y M-03): **`pbt4d-retire-list-271.tsv`**, sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac` (284 filas, 271 con acción y 13 que se quedan sin tocar). **La base:** el cierre de PB-T4c (F4B-E55, `6c2820bab4d8b41e3e229e23f3a1638ccbb04735`).

- **Objective** (ASCII): el de §6.4.1.
- **`AllowedWriteScope`** (37), ruta a ruta, en este orden:
  - **src/, existentes que cambian (UNIT_CHANGE, 4):** 1. `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs`, 2. `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`, 3. `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs`, 4. `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs` (solo comentarios, D-S15);
  - **src/, existentes que se borran (UNIT_CHANGE, 2):** 5. `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs`, 6. `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotFieldAdapter.cs`;
  - **tests/, existentes que el RED edita (23) o borra (3, bloques DELETE; CA-T4d M-03):** 7. `tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs`, 8. `tests/RackCad.Tests/PushBackFullWritePrepareTests.cs`, 9. `tests/RackCad.Tests/PushBackFullWriteRunTests.cs`, 10. `tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`, 11. `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`, 12. `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs`, 13. `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs`, 14. `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs`, 15. `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs`, 16. `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardFull.cs`, 17. `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`, 18. `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs`, 19. `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs`, 20. `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`, 21. `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs`, 22. `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs`, 23. `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs`, 24. `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs`, 25. `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`, 26. `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs`, 27. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionPilotParityTests.cs` (se borra), 28. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`, 29. `tests/RackCad.UI.Tests/PushBackFieldValidationConcordanceTests.cs` (se borra), 30. `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs` (se borra), 31. `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs`, 32. `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`;
  - **tests/, nuevos (5):** 33. `tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs`, 34. `tests/RackCad.Tests/PushBackRetirePilotCensusGuardTests.cs`, 35. `tests/RackCad.Tests/PushBackRetirePilotRunTests.cs`, 36. `tests/RackCad.Tests/PushBackRetirePilotFullCopyGuardTests.cs`, 37. `tests/RackCad.Tests/PushBackRetirePilotPlanGuardTests.cs`;
  - **ningún otro archivo de `src/`** (ni de `Pilot/`), ni la ventana, ni el panel, ni el Plugin: el GREEN literal no los cambia.
- **`UNIT_CHANGE`:** las 6 de `src/` (4 que cambian y 2 que se borran; F4B-E25).
- **`ForbiddenWriteScope`:** el método de [PKC-C] con el precedente de PB-T3c y de PB-T4a a PB-T4c en `tests/` (la lista explícita del contrato anterior, menos las rutas de `tests/` de este alcance; un prefijo prohibido que cubre una de ellas se sustituye por la lista de sus archivos), más las clases de PB-T4c tras su cierre (sus cinco clases de las CT y sus cuatro equivalentes), salvo las de este alcance (`PushBackEditAllFullCopyGuardTests.cs`, solo para su `Skip`). **El número lo da la regla del generador sobre el alcance final en `6c2820ba`** (`<medido por el generador>`).
- **El RED** (un commit, con su push antes del GREEN; D-S11): **todo lo de `tests/`,** el texto literal de la pieza `pbt4d-red-v54.txt` (sha256 `80f8cc0c5c9f54de033648e7eec95728fa0fc0d28e31886101207e3dbec381db`): las bajas con registro, las ediciones con registro, los 3 `Skip` (D-S12), la prueba lenta acotada (D-S14), los vacíos de M-04 (D-S13), las 29 sustitutas de WPUR, el corpus de CT-PBT3B-06 sobre el modelo completo (CA-T4d M-01), los ayudantes sin uso y los 3 archivos sin pruebas que se borran (CA-T4d M-03) y los 5 archivos nuevos. **`src/` queda intacto: no hay esqueleto.** **La herramienta:** `python -I apply_red_pbt4d_v53.py <dir-copia> --repo <checkout> --rev 6c2820ba` escribe exactamente eso (y borra los 3 archivos de los bloques DELETE), tras comprobar que la copia es de la revisión; el Worker puede usarla sobre su worktree o copiar sus salidas. Imprime el sha256 de cada archivo.
- **El GREEN:** solo `src/`, el texto literal de `pbt4d-green-v54.txt` (sha256 `aaf6cab882cfb3eba6939fd8b64ec1fa15c22431ba8bdaeb9bcdf4dbb89ffbf0`; `apply_green_pbt4d_v53.py` lo escribe en una copia): 4 archivos cambiados y 2 borrados. `tests/` no cambia después del RED (salvo una reparación del RED, solo en las dos clases de las CT).
- **`RequiredTests`:** la tabla de §6.4.1 (88: 83 heredados y 5 nuevos).
- **Invariantes:** INV-PBT4D-01 a 07 y 13 (§6.4.1) y los comunes 08 a 12 y 14.
- **`StopConditions`:** las 17 estándar y PBT4D-C01 a C14, C16, C17 y C18 (§6.4.1): 34.
- **`ExpectedEvidence`:** §6.4.1.
- **La línea base:** el cierre de PB-T4c (F4B-E55, `6c2820ba`), con la CI de su GREEN (`44959653142` sobre `7cc1076e`).

#### 6.4.1 Los textos literales del contrato de PB-T4d

Para la comparación del generador, en ASCII, campo a campo. Todo está medido en `6c2820ba`; lo que da el generador va como `<medido por el generador>`.

```text
TaskId: F4B-PBT4-RETIRE-PILOT

Objective:
Retirar el codigo del piloto de Push Back que el panel ya no usa desde PB-T4b, con el alcance B que eligio el Owner (owner-pbt4d-list-decisions.md, pregunta 1): el comparador del piloto y el adaptador de su campo, enteros; ComposeForWrite, KeepPilotEditsOn y la composicion del piloto (PushBackPilotComposition); la instantanea del piloto (PushBackPilotPrepareInput), su Prepare y su Build; el Precheck, el Execute y el ComparePost del piloto; y, en BuildCore, el modo que no borra vistas (el parametro allowErase y lo que solo existe con el: la rama de la vista no disponible y sus dos constantes, ViewUnavailablePrefix y ViewUnavailableSuffix; owner-pbt4d-deadcode-27-decisions.md, pregunta 1); y limpiar los cuatro cref y el comentario que nombran lo retirado (D-S15) y los comentarios que describian como presentes la corrida, el PREPARE y la composicion para escribir del piloto (CA-T4d M-05). Se quedan el lector del piloto, su materializacion, PushBackPilotDraftState, las constantes de PushBackPilotMaterializer, PrepareFull, ExecuteFull, el Precheck del borrador completo, ComparePostFull, BuildFull y BuildCore. El RED lleva todo lo de tests/ con el texto literal del paquete (D-S11): la lista exacta pbt4d-retire-list-271.tsv, cada fila con su accion (borrar con registro, reescribir con registro, sustituir con registro, acotar con registro), los tres Skip con su equivalente (D-S12), los vacios de M-04 reescritos (D-S13), la prueba lenta acotada (D-S14), las 29 sustitutas de WorkspacePilotUpdateRunTests sobre el modelo completo (27 de la corrida generica sobre ExecuteFull, owner-pbt4d-deadcode-27-decisions.md, pregunta 2, y 2 de PushBackPilotSystemRebuild, owner-pbt4d-cat4d-decisions.md, pregunta 1), el corpus de CT-PBT3B-06 sobre PrepareFull (CA-T4d M-01), los ayudantes que la retirada deja sin uso y las tres clases que deja sin pruebas, borrados con registro (CA-T4d M-03), y las dos CT, CT-PBT4D-01 y 02; el GREEN solo toca src/, con el texto literal del paquete, segun el paquete del contrato de PB-T4 v5.4. Ningun cambio de comportamiento: ni el panel, ni la ventana, ni RACKEDITAR, ni el Plugin; la recarga de Editar todo va a una tarea propia (pregunta 2); cero escrituras en el DWG; no se ejecuta AutoCAD.

Invariants:
INV-PBT4D-01. Alcance: dentro de las 37 rutas. git diff BaseSha..CurrentSha -- src/RackCad.Domain src/RackCad.UI src/RackCad.Plugin vacio; en src/RackCad.Application solo los seis archivos de Systems/PushBack/Pilot/ del alcance (cuatro cambian y dos se borran); en tests/ solo los 26 archivos existentes (23 que el RED edita y 3 que borra) y los 5 nuevos del alcance, con el texto literal de la pieza del RED.
INV-PBT4D-02. RED y GREEN: el RED es la pieza literal de tests/ (pbt4d-red-v54.txt, sha256 80f8cc0c5c9f54de033648e7eec95728fa0fc0d28e31886101207e3dbec381db), byte a byte, y src/ sin cambio (no hay esqueleto, D-S11); el GREEN solo toca src/, con el texto literal del paquete (pbt4d-green-v54.txt, sha256 aaf6cab882cfb3eba6939fd8b64ec1fa15c22431ba8bdaeb9bcdf4dbb89ffbf0). En el RED, el filtro con ExpectRed (CT-PBT4D-01, PushBackRetirePilotGuardTests) da Failed por asercion (la primera linea del StackTrace bajo tests/), con Failed == Selected; los filtros con ExpectRed false pasan, y las autopruebas de CT-PBT4D-01 y 02 fallan sobre el texto inyectado. Con una reparacion del RED (F4B-E35, RR): RedSha..RedRepairSha solo toca las dos clases de las CT y ningun archivo de src/, ni los 26 archivos existentes de tests/, ni las tres clases nuevas que no son CT; el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el GREEN de arriba.
INV-PBT4D-03. Lo retirado y lo que se queda: exactamente D-PBT4-10 (v5.3) con el texto literal del GREEN del paquete; lo retirado no esta en src/ ni lo nombra ningun archivo de src/, y las siete piezas que se quedan (el lector del piloto, Materialize, PushBackPilotDraftState, PrepareFull, ExecuteFull, el Precheck del borrador completo y ComparePostFull) estan una vez (CT-PBT4D-01); ningun archivo de tests/ nombra lo retirado como codigo (CT-PBT4D-02).
INV-PBT4D-04. Sin cambio de comportamiento: el panel, la ventana, RACKEDITAR y el Plugin no cambian; la corrida generica, el plan y la comparacion de POST del modelo completo dan lo mismo que en BaseSha (la red de section 5, las 29 sustitutas y las ediciones con registro en verde).
INV-PBT4D-05. Las sustitutas y los equivalentes, literales: PushBackRetirePilotRunTests (las 29 de WPUR sobre el modelo completo: 27 de la corrida, sobre ExecuteFull, y 2 de PushBackPilotSystemRebuild), PushBackRetirePilotFullCopyGuardTests (el equivalente de CT-PBT4B-20) y PushBackRetirePilotPlanGuardTests (las dos guardas de WorkspacePilotPrepareGuardTests), byte a byte el texto de la pieza del RED, con su sha256 (NewFileSha); los tres originales de D-S12 con su Skip de motivo literal en su linea de atributo.
INV-PBT4D-06. La cobertura, de todo el codigo de src/ que se queda (CA-T4d I-02; owner-pbt4d-cat4d-decisions.md, pregunta 2): cada linea que la suite completa de Core o los filtros de UI de PB-T4c y de PB-T4d ejecutan en BaseSha, fuera de lo que retira el GREEN literal, se ejecuta en CurrentSha, y cada rama que cubren en BaseSha sigue cubierta en CurrentSha (medido en la precondicion 6, linea a linea y rama por rama, con coverlet en JSON sobre RackCad.Domain, RackCad.Application y RackCad.UI). Todo camino que se queda y pierda su unica prueba tiene una sustituta sobre el modelo completo; las 436 lineas y las 9 ramas de la v5.2 son un conjunto vigilado dentro de este.
INV-PBT4D-07. Las redes: section 5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, salvo las 271 filas con accion de la lista cerrada (pbt4d-retire-list-271.tsv, sha256 05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac), cada una como dice su accion y con el texto literal de la pieza del RED.
INV-PBT4D-08. Las pruebas de UI: las reglas de section 5.0.
INV-PBT4D-09. Determinismo: GUID fijo, cultura invariante, "R" y orden ordinal; ni hora ni aleatoriedad.
INV-PBT4D-10. Las corridas locales: solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, si; sin copias de arboles; las corridas temporales terminan en Assert.Fail(string.Join("\n", lineas)), corren dentro de EditorDiscardPrompt.Substitute cuando abren ventanas, no entran en ningun commit y guardan TRX y log como artefactos.
INV-PBT4D-11. La evidencia: rutas relativas comprobadas en worker-handoff.json; TreeSha real o null; el Trailer, el real; el Worker no escribe en docs/.
INV-PBT4D-12. Las reglas de F4B; sin AutoCAD; la verificacion con las tres comprobaciones deterministas de F4B-E15.
INV-PBT4D-13. Las declaraciones de src/: ninguna nueva; las quitadas, exactamente las de la lista cerrada de PBT4D-C13, por nombre y con su cuenta, sobrecargas y tipos anidados incluidos; y allowErase no aparece en src/.
INV-PBT4D-14. D2: fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna linea anadida muta un PushBackSystem ni un RackModuleReconciliationResult adoptado.

StopConditions (propias):
PBT4D-C01: doing the task would require writing outside the AllowedWriteScope.
PBT4D-C02: it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a .csproj or Fixtures/I64. No exception outside the closed list of PB-T4d: the 271 rows with an action of pbt4d-retire-list-271.tsv (sha256 05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac), each as its action says (delete with record, rewrite with record, substitute with record, bound with record), with the literal text of the RED piece pbt4d-red-v54.txt (sha256 80f8cc0c5c9f54de033648e7eec95728fa0fc0d28e31886101207e3dbec381db), and nothing else in tests/. Any existing test that fails, outside that list, returns to the Owner.
PBT4D-C03: an observable change appears, of RACKEDITAR, of the panel, of the window, of the Plugin or of the write of the drawing: PB-T4d retires code that the panel no longer uses and changes no behaviour. STOP and return to the Owner.
PBT4D-C04: a method of a filter with ExpectRed passes on the RED, or fails by an exception and not by an assertion.
PBT4D-C05: a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or writing to the profile or to disk.
PBT4D-C06: the GREEN CI fails outside the new tests, reproducibly and not attributable to them.
PBT4D-C07: a fact of section 2 does not hold in a way that changes a CT or the design; or an expected line is not met in the GREEN; or a self-checking recipe (the self-tests of CT-PBT4D-01 and CT-PBT4D-02) does not exercise its path. The Worker does not adjust what is expected; except under F4B-E35 (the RED repair rule, RR): a writing error in a new CT of this task (one of its two classes; never the three new classes that are not CTs, whose text is the package one), found after the RED push and before the GREEN, is repaired without this stop and without spending an attempt by one declared RED repair commit that touches only test files the RED added and changes only the when or the how of a reading the package already prescribes, and no expected line, literal or oracle of the package or the contract, with the full RED criterion rerun on RedRepairSha; a second writing error in the same attempt, a repair that needs an expected line changed, or a literal line the package fixes for the repaired class that does not stay byte for byte in it, is this stop. A substitute or an equivalent of the three new classes that fails in the RED or in the GREEN is this stop too: the session issues a package version with its text.
PBT4D-C08: a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner
PBT4D-C09: a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.
PBT4D-C10: doing the task would require what the scope of PB-T4d excludes: touching a file of src/ other than the six of Pilot/ of the AllowedWriteScope, or a text of those six other than the literal GREEN; retiring or changing what stays (the reader of the pilot, its materialization, PushBackPilotDraftState, the constants of PushBackPilotMaterializer, PrepareFull, PushBackFullPrepareInput, BuildFull, BuildCore, ExecuteFull, the Precheck of the full draft, ComparePostFull, ComparePostCore or DesiredNamesOf); the reload of Editar todo over a clean draft that may be stale (owner-pbt4d-list-decisions.md, question 2: its own task after PB-T4d); touching the classic editor, RACKEDITAR, the window, the panel or the Plugin; or touching tests/ other than with the literal RED piece.
PBT4D-C11: a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner ([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, and nothing else) -> substitute PBT4-NC2-EXACT-WHITELIST-F4B-01, the version 2 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py, SHA-256 c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45; owner-pbt3a-nc2-decision.md, "Aceptar y ampliar el sustituto"), whose property 8 admits only the closed list of the exact literals of the evidence entry of the byte-by-byte comparison, one per package (PB-T2 and PB-T3), with its reviewed text unchanged and run with --task PBT4 in the F4B-E21 mode with the injection note and the raw output of this task's accepted planning, recorded as nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B; if inject_opaque.py changed since that run, the substitute is reviewed before it is reused; (2) nc1 with the signature PD-F4B-01, the false SHA cascade (F4B-E40: owner-pbt2c-nc1-decision.md, its conditions (a) to (f): the mutation only changes the CurrentSha of the handoff to a SHA that does not exist in the repository; EXECUTION_BLOCKED / STOP with S-04 among the stops; Handoff is the first check in fail; the failing checks are a subset of Handoff, Identity, Remote, Scope, Ci and Tests that always includes Handoff and Identity, and all the others pass as in the real verification; each failing evidence cites the false SHA and none attributes the failure to another cause; the Identity evidence also writes the real HEAD), of which the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265; as applied in F4, f4-dispositions.md:1657-1660) is a particular case -> substitute PBT3x-NC1-CASCADE-SIGNAL-01 (F4B-E40 and F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py, SHA-256 41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40, reviewed APTO, with its declared limits), with its reviewed text unchanged and run with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md) and, for PD-F4B-01, its conditions (a) to (f); the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or Trailer also in fail, any nc1 that does not meet PD-F4B-01 - the one-off acceptances of nc1 in PB-TC (F4B-E08), in PB-TC2 attempt 1 (F4B-E12), in PB-T2a (F4B-E34), in PB-T2b (F4B-E38) and in PB-T2c (F4B-E40) do not extend -, an nc2 that the version 2 of the substitute does not give PASS - the disposition of the Owner on the nc2 of PB-T3a (F4B-E43) does not extend -, a substitute that does not give PASS, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner before any substitute or acceptance; no one-off acceptance extends to PB-T4d
PBT4D-C12: a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)
PBT4D-C13: doing the task would require touching a file of src/ other than the six of the AllowedWriteScope; or adding a declaration of any visibility in src/ (none is added); or removing a declaration that is not in this closed list (57 by the census of the package, an overload is one more): the type PushBackPilotStateComparer with its nested type StructuralEquality and all their members, and the type PushBackPilotFieldAdapter with all its members (their two files are deleted); in PushBackPilotMaterializer.cs, the methods ComposeForWrite and KeepPilotEditsOn and the type PushBackPilotComposition with all its members; in PushBackPilotUpdatePlan.cs, the type PushBackPilotPrepareInput with all its members, the methods Prepare and Build of PushBackPilotUpdatePlanner and its constants ViewUnavailablePrefix and ViewUnavailableSuffix; in PushBackPilotUpdateRun.cs, the overload Precheck(DraftSession<PushBackPilotDraftState>) and the methods Execute and ComparePost. Prescribed changes that are not declarations: BuildCore loses its parameter allowErase, the condition allowErase of its four conditions (three in the loop and the one of AllObsolete), the branch of a view that is not available and the name of each re-read envelope; BuildFull calls it without the argument true; and the four cref and the comment of D-S15 and the comments of CA-T4d M-05 change (comments only). The Worker stops before removing or adding anything else.
PBT4D-C14: a design of the corpus changes its admission or its lock (the pilot reader, the full reader, AUTH-13 or the parked tail).
PBT4D-C16: an existing test outside the closed list of PB-T4d (the 271 rows with an action of pbt4d-retire-list-271.tsv) fails, including one that reads the text or the types in a way only seen when it runs; or a line or a branch of src/ that stays (RackCad.Domain, RackCad.Application or RackCad.UI), executed or covered in BaseSha by the full Core suite or by the UI filters of PB-T4c and PB-T4d, is no longer executed or covered in CurrentSha. It returns to the Owner (owner-pbt4d-list-decisions.md, question 4; owner-pbt4d-deadcode-27-decisions.md; owner-pbt4d-cat4d-decisions.md, question 2: before the emission, every such path gets a substitute over the full model, and only a substitute that cannot be written, or more than 10 new substitutes over the 29, return to the Owner). A substitute or an equivalent of the three new classes that fails where the package does not expect it is not this stop: it is PBT4D-C07.
PBT4D-C17: (F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." The Worker chooses none of the three exits: the Owner does
PBT4D-C18: a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in the attempt; it touches a file the RED did not add, an existing test, src/, a fixture, one of the three new classes that are not CTs or anything outside the AllowedWriteScope; it changes an expected line, literal or oracle, or anything other than the when or the how of a reading the package already prescribes (RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP.

ExpectedEvidence:
[0] antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;
[1] el commit RED y su push antes del GREEN; con una reparacion del RED (F4B-E35, RR-4), ademas, el commit "RED repair" y su push antes del GREEN, con su CI, registrada como la del RED reparado, y los TRX de cada filtro con ExpectRed sobre RedRepairSha;
[2] la CI del RED como en section 6.0 (tests en failure, por CT-PBT4D-01; ui-tests en success; build-ui en success; build-plugin omitido; sin cuelgue); la del RED reparado, si lo hay, da lo mismo;
[3] la tabla "CT -> la asercion que falla en el RED": CT-PBT4D-01 cae por su Assert.Equal (src/ sigue con el piloto) y CT-PBT4D-02 pasa, con las autopruebas de las dos;
[4] el GREEN y su CI 4/4;
[5] en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED, sobre RedRepairSha si hay reparacion, y sobre el GREEN, y tres corridas en verde de los filtros con ExpectRed sobre el GREEN;
[6] la entrega con el mapa CT -> metodo, las rutas relativas comprobadas, el sha256 de cada uno de los cinco archivos nuevos (iguales a NewFileSha) y de cada uno de los 23 archivos existentes de tests/ que el RED edita en RedSha (iguales a los de la pieza del RED), y la ausencia en RedSha de los 3 que borra, y el Trailer real de los commits RED, "RED repair" (si lo hay) y GREEN; con una reparacion del RED, ademas, RedRepairSha, la prueba y la linea corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y RedSha, que sigue siendo el commit RED original (F4B-E35, RR-5);
[7] la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3;
[8] con una reparacion del RED, la verificacion la lee como parte de la entrega (F4B-E35, RR-7; section 6.0): n.o 10 (Tests): el RED se acredita con RedSha y con la CI del RED original, y sus TRX, con los filtros con ExpectRed entre los fallidos. La clausula "git diff --name-only RedSha CurrentSha no toca RT U ChainRedFiles" se comprueba desde RedRepairSha: git diff --name-only RedRepairSha CurrentSha. n.o 11 (Trailer): el trailer va tambien en el commit "RED repair". Lo que el Controller comprueba de RR (si algo falla, es C18): RR-1, un solo commit "RED repair", hijo directo de RedSha y anterior al GREEN; RR-2, git diff --name-only RedSha RedRepairSha dentro de las dos clases de las CT; RR-4, la CI de RedRepairSha, registrada como la del RED reparado, y los TRX de los filtros con ExpectRed sobre el; RR-5, la declaracion de la entrega, con el diff, y RedSha, el commit RED original; RR-6, con el tramo RedSha..RedRepairSha. En PB-T4d, ademas: RedSha..RedRepairSha no toca ningun archivo de src/, ni los 26 archivos existentes de tests/ que el RED edita o borra, ni las tres clases nuevas que no son CT. La ayuda mecanica de RR-3: es parada C07, una violacion cierta de RR-3, que un literal de la lista cerrada de la clase reparada (section 6.0 del paquete, la tabla de RR-3) cambie entre RedSha y RedRepairSha: una linea o un fragmento, si no aparece como subcadena exacta el mismo numero de veces; un valor de gesto, si no aparece como token el mismo numero de veces. Indicios para la revision, no parada: una cadena <campo>= anadida o quitada en git diff -U0 RedSha RedRepairSha, o un cambio en el primer argumento de un Assert.Equal. Pueden dar falsos positivos, como una linea que solo se mueve de sitio, asi que van a Findings y a la revision, con la declaracion de RR-5.
[9] la CI del RED con tests en failure, ui-tests en success, build-ui en success y build-plugin omitido;
[10] la del GREEN 4/4, con el Plugin compilado;
[11] el censo de declaraciones de los seis archivos de Pilot/ del alcance, en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es una declaracion mas; un tipo anidado tambien): ninguna nueva; las quitadas, la lista cerrada de PBT4D-C13 (57); y allowErase no aparece en src/;
[12] el Trailer real;
[13] la entrada "## PB-T4d" de superseding-characterization.md, escrita por la sesion en el commit de emision: la lista (pbt4d-retire-list-271.tsv, con su sha256) por grupo y accion, los motivos literales de los tres Skip, cada equivalente y sustituta con su NewFileSha y su cambio declarado, las ediciones con registro, y las decisiones del Owner (owner-pbt4d-list-decisions.md, owner-pbt4d-substitutes-decision.md, owner-pbt4d-run-coverage-decision.md, owner-pbt4d-deadcode-27-decisions.md y owner-pbt4d-cat4d-decisions.md) y de la sesion (D-S11 a D-S16); su tabla de ejecucion se completa al cerrar;
[14] Linea base de la CI: la del cierre de PB-T4c (F4B-E55, 6c2820ba): run 44959653142 sobre su GREEN 7cc1076e, Core 19 619 resultados, con 19 158 Passed y 461 Skipped, y UI 2 486, con 2 437 Passed y 49 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4d borra con registro las bajas de su lista, pone 3 Skip en Core, anade 3 clases nuevas que no son CT (las 29 sustitutas, el equivalente de CT-PBT4B-20 y las dos guardas de WPPG) y 2 CT de Core, sin ninguna CT de UI, y borra 3 archivos de UI sin pruebas. El GREEN: Core 19418 resultados, con 19015 Passed, 0 Failed y 403 Skipped; UI 2464, con 2416 Passed, 0 Failed y 48 Skipped. El RED: Core 19418 resultados, con 19014 Passed, 1 Failed (CT-PBT4D-01) y 403 Skipped; UI 2464, con 2416 Passed, 0 Failed y 48 Skipped;
[15] la comprobacion mecanica de las piezas: sobre RedSha, el texto de cada archivo de tests/ del alcance es el que da red_pbt4d_v53.red sobre BaseSha, y los 3 de los bloques DELETE no estan (red_pbt4d_v53.red_full; apply_red_pbt4d_v53.py), y src/ es el de BaseSha; sobre CurrentSha, src/ es el que da green_pbt4d_v53.green (apply_green_pbt4d_v53.py) y tests/ el de RedSha.
```

**Los `Skip` literales** (`pbt4d_test_edits_v54.py`, `SKIPS`; ASCII), cada uno en la línea de atributo `[Fact]` de su prueba en `6c2820ba`:

- `tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs`:572 (`CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList`): `[Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]`
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`:81 (`TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage`): `[Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]`
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`:194 (`TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread`): `[Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]`

**Los archivos nuevos, literales** (su texto, en el anexo V54-A; `NewFileSha`, del texto con `\n`), en Core:

- `tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs`, `0d64806a53b36ea0719bc7647dcfd3c15e8169a66a4494517097a82ce12fadb3`: CT-PBT4D-01; **`ExpectRed` true**
- `tests/RackCad.Tests/PushBackRetirePilotCensusGuardTests.cs`, `1874c47811ddda428745a1f5566f1e7290b31fa2a2e65b07c2ff52059f863f20`: CT-PBT4D-02; `ExpectRed` false
- `tests/RackCad.Tests/PushBackRetirePilotRunTests.cs`, `3554df0d960f18ef6bb2b930f4ac7757e8ca7438ebf6a9eda8934ef9acf8fe69`: las 29 sustitutas de WPUR sobre el modelo completo, con `FullUpdateSetup` (`v53/make_run_eq_v53.py`): 27 de la corrida genérica sobre `ExecuteFull` y la n.º 48 y la n.º 49 de `PushBackPilotSystemRebuild` (CA-T4d I-01); 26 por el mapa de PB-T4b; a mano la n.º 10, la n.º 43 y la n.º 49 (solo su comentario); `ExpectRed` false
- `tests/RackCad.Tests/PushBackRetirePilotFullCopyGuardTests.cs`, `ee9b6f1513f549065da391c10d6da947cc8e197644efbc7d72cf41e46443e6cf`: el equivalente de CT-PBT4B-20 de PB-T4c, copia literal con los `BaseSha`, los `Skip`, las `CopySha` y los recuentos que PB-T4d cambia (`make_fullcopy_eq_v52.py`, sobre los textos finales del RED de la v5.3); `ExpectRed` false
- `tests/RackCad.Tests/PushBackRetirePilotPlanGuardTests.cs`, `48c2f4a9619e2f3e7e5de4f1ad98bf68024635c268866fa1242f65f329d8ca43`: las dos guardas de `WorkspacePilotPrepareGuardTests` (D-S12), cada una con su cambio declarado; `ExpectRed` false

**Las ediciones con registro** (`pbt4d_test_edits_v54.py`, `EDITS`, y la llamada de StaleBase, `STALE_CALL`): las de §8.3 de la lista, con su texto literal en el anexo V54-A. **Las bajas con registro:** las filas «borrar con registro» y «sustituir con registro» de la lista, que la pieza del RED quita (`v54/make_red_v54.py` con `retire_pbt4d_v52.py` en seco: 193 bajas de las 201 necesarias de `/d/r6h`, menos las 17 que se quedan editadas, más las 9 que compilan y se quitan; y, desde la v5.3, los 32 ayudantes sin uso de M-03, que quita `v53/unused_v53.py` tras las ediciones, y los 3 archivos sin pruebas, en bloques DELETE).

**La precondición 6** (I-02; §11.6), que corre la sesión antes de emitir, sobre copias exportadas del cierre de PB-T4c, fuera de todo commit (`pbt4dcc/compile/`, que la emisión custodia en `F4B-DRAFTS/pbt4/cc4d/`; el resumen, `pbt4d-precondition6.json`, sha256 `52e03e061cd62c79597f51b8eaf144a27d68b3bf5db876406a804be2fa978989`, y su registro, `compile-check.md`, `d13afdb02216d385c32ade81b5cf9508c648a970337cf92b9d964b936933de42`):
1. `git archive` de `6c2820ba` a una carpeta nueva, fuera del worktree;
2. `python -I apply_red_pbt4d_v53.py <copia> --repo <checkout> --rev 6c2820ba`: el RED literal de `tests/`;
3. la compilación de Core y de UI en Debug, sin ningún `warning CS`;
4. **el RED exacto:** la suite completa de Core, donde **solo cae CT-PBT4D-01, por aserción** (las 2 CT, los 3 `Skip` con su motivo y las tres clases nuevas en verde), y **los 18 filtros de UI del contrato** (los de `RequiredTests`, `ui_filters_pbt4d_v53.txt`; CA-T4d M-07 b), **0 `Failed`**. **La copia principal corre sola** (CA-T4d M-07 a): el conductor comprueba al empezar que no hay otro `dotnet` ni `testhost`;
5. **sobre la misma copia, el GREEN literal** (`apply_green_pbt4d_v53.py`): la compilación de Core y de UI en Debug y del Plugin en Release con `-o`, sin ningún `warning CS`;
6. **el GREEN:** la suite completa de Core, **0 `Failed`**, y los 18 filtros de UI, **0 `Failed`**, con la duración de la suite y de la prueba lenta acotada. **Una prueba que falle fuera de la lista** vuelve al Owner (PBT4D-C16); **una sustituta o un equivalente que no pase** necesita una versión del paquete (PBT4D-C07);
7. **la cobertura ampliada de todo `src/` que se queda, línea a línea y rama por rama** (CA-T4d I-02; `owner-pbt4d-cat4d-decisions.md`, pregunta 2): la suite completa de Core y la unión de los filtros de UI de PB-T4c y de PB-T4d (`ui_union_v53.txt`), con coverlet en JSON sobre Domain, Application y UI (`cov_v53.runsettings`), en el GREEN de la copia principal y en la base sin cambios, sola, en otra copia (`p6d_basecov_v53.sh` y `p6d_uirep_v53.sh`; la referencia); la UI de cada rama, la unión de tres corridas de la unión de los filtros y de cada filtro por separado, en su proceso, como los corre el contrato (`p6d_uirep_v53.sh` y `p6d_uifilt_v53.sh`; los golpes de un binding de WPF dependen de lo que corrió antes en el mismo proceso: `noise-batch/`, R-T4-38); `cov_p6_v54.py` lleva cada línea de la base al GREEN (por texto en los archivos que el GREEN cambia) y exige que cada línea ejecutada en la base (Core o UI) siga ejecutada y cada rama cubierta siga cubierta; las de texto cambiado, ejecutadas y con todas sus ramas; las retiradas, listadas; lo que Core sola pierde y cubre la UI, listado aparte; y el conjunto vigilado de la v5.2 (las 436 líneas y las 9 ramas). **Un camino que se queda sin su única prueba** recibe una sustituta sobre el modelo completo (la regla del Owner); **vuelve al Owner** si una no se puede escribir o si salen más de 10 nuevas sobre las 29;
8. **el ensayo de las CT de texto y el censo de C13:** las dos CT son clases literales del RED, así que el ensayo es su resultado en la suite completa de cada rama (CT-01 cae en el RED y pasa en el GREEN; CT-02 pasa en las dos), con su **control** en otra copia (`p6_control_v53.sh`): la base con solo las dos CT (caen las dos) y el RED con la retirada incompleta de la variante A en `src/` (cae CT-01 y pasa CT-02); y **el censo** (`census_c13_v52.py`, por objetos git): ninguna declaración nueva en `src/`, las quitadas iguales a la lista cerrada de PBT4D-C13 y ningún `allowErase`, con cuatro autopruebas;
9. los `RequiredTests` y los totales (`required_pbt4d_v53.py`, con la base de UI en la corrida de la unión, y `ui_totals_v52.py`), y el contrato lleva por sha256 las dos piezas, los cinco archivos nuevos (`NewFileSha`) y la lista.

**Lo medido en la v5.3** (V53-06 y V54-01; la v5.2, historia): `/d/r6y` para los pasos 2 a 7 (el RED, el GREEN y la cobertura del GREEN; **sola**: 0 procesos al empezar), `/d/r6w` para la referencia de la cobertura (la base sin cambios, sola: la suite de Core, 19619 = 19158 + 0 + 461, y la unión de los filtros de UI, 921 = 890 + 0 + 31) y la base de los `MinSelected`, y `/d/r6z` para el control del ensayo; el censo, por objetos git. Lo previsto, con cero `warning CS`:
- **Core:** en el RED, 19418 = 19014 + 1 + 403, y solo cae CT-PBT4D-01, por aserción; en el GREEN, 19418 = 19015 + 0 + 403;
- **UI,** los 18 filtros del contrato: 0 `Failed` en las dos ramas;
- **la duración de la suite de Core:** 325 s en el RED y 261 s en el GREEN (355 s con la cobertura); **la prueba lenta acotada:** 13.1 s en el RED y 17.0 s en el GREEN (la de PB-T4c tardaba unos 390 s sola, `/d/r6f`);
- **la cobertura ampliada:** archivos de src/ instrumentados 775; lineas ejecutadas en la base que se quedan 69842 (ramas cubiertas 28818); retiradas por el GREEN 419; texto cambiado 4; lineas sin ejecutar en el GREEN 0; lineas con ramas perdidas 0; **PASA** (`cov-p6-v54.txt`); conjunto vigilado de la v5.2: 537 lineas de WPUR, 433 se quedan, 0 sin cubrir en el GREEN; bajan (informativo) 16801, de ellas PERDIDA 0;
- **el ensayo:** CT-01 cae en el RED y pasa en el GREEN; CT-02 pasa en las dos; **su control:** sobre la base con solo las dos CT, caen las dos; sobre el RED con la variante A en `src/`, cae CT-01 y pasa CT-02; **el censo:** PASA;
- **los totales de UI de la CI** (la línea base menos lo que quitan las bajas de UI, medido en la unión de los filtros): UI red: 2464 = 2416 + 0 + 48 (Passed + Failed + Skipped) y UI green: 2464 = 2416 + 0 + 48 (Passed + Failed + Skipped).

**Las corridas anteriores** son historia: las de la v5.2 (`/d/r6a` a `/d/r6u`; lo de `/d/r6r`, declarado en `compile-check.md`, CA-T4d M-07) y los dos ensayos de la v5.3 (`/d/r6v` y `/d/r6x`).

**`RequiredTests`** (88: 83 heredados y 5 nuevos). Los heredados son los del contrato de PB-T4c (`F4B-PBT4-EDIT-ALL-WINDOW`, emitido), con su número de allí, **todos con `ExpectRed` false**, y su `MinSelected` medido sobre el GREEN literal (resultados no omitidos); salen los que quedan en 0: el n.º 37 (`RackCad.UI.Tests.PushBackFieldValidationConcordanceTests`, de 1 a 0: 1 resultados en la base y 0 en el GREEN) y el n.º 85 (`RackCad.Tests.PushBackEditAllFullCopyGuardTests`, de 1 a 0: 1 resultados en la base y 0 en el GREEN). Los nuevos, por clase exacta (D-S12): las dos CT y las tres clases nuevas que no son CT. Ningún filtro heredado selecciona una clase nueva, y cada filtro nuevo solo la suya, en las dos ramas (`required_pbt4d_v53.py`).

| n.º | Project | Filter | MinSelected | ExpectRed | Origen |
|---|---|---|---|---|---|
| 1 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests` | 4 | false | PB-T4c n.º 1 |
| 2 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests` | 1 | false | PB-T4c n.º 2 |
| 3 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackViewEraseGuardTests` | 4 | false | PB-T4c n.º 3 |
| 4 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 | false | PB-T4c n.º 4 |
| 5 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateGuardTests` | 83 | false | PB-T4c n.º 5 |
| 6 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | 120 | false | PB-T4c n.º 6 |
| 7 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackSharedReferenceGuardTests` | 1 | false | PB-T4c n.º 7 |
| 8 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 | false | PB-T4c n.º 8 |
| 9 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullWrite` | 13 | false | PB-T4c n.º 9 |
| 10 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 | false | PB-T4c n.º 10 |
| 11 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathGuardTests` | 1 | false | PB-T4c n.º 11 |
| 12 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | 2539 | false | PB-T4c n.º 12 |
| 13 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullDraft` | 10 | false | PB-T4c n.º 13 |
| 14 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests` | 2 | false | PB-T4c n.º 14 |
| 15 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 503 | false | PB-T4c n.º 15 |
| 16 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditableState` | 13 | false | PB-T4c n.º 16 |
| 17 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate` | 1 | false | PB-T4c n.º 17 |
| 18 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 | false | PB-T4c n.º 18 |
| 19 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 | false | PB-T4c n.º 19 |
| 20 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 | false | PB-T4c n.º 20 |
| 21 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false | PB-T4c n.º 21 |
| 22 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 10 | false | PB-T4c n.º 22 |
| 23 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 16 | false | PB-T4c n.º 23 |
| 24 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 | false | PB-T4c n.º 24 |
| 25 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet` | 14 | false | PB-T4c n.º 25 |
| 26 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 12 | false | PB-T4c n.º 26 |
| 27 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 | false | PB-T4c n.º 27 |
| 28 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.I58F1` | 10 | false | PB-T4c n.º 28 |
| 29 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 9 | false | PB-T4c n.º 29 |
| 30 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.NamespaceFolderGuardTests` | 7 | false | PB-T4c n.º 30 |
| 31 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftStatePortTests` | 3 | false | PB-T4c n.º 31 |
| 32 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 | false | PB-T4c n.º 32 |
| 33 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 | false | PB-T4c n.º 33 |
| 34 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 | false | PB-T4c n.º 34 |
| 35 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 | false | PB-T4c n.º 35 |
| 36 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 3 | false | PB-T4c n.º 36 |
| 37 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 | false | PB-T4c n.º 38 |
| 38 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelTests` | 11 | false | PB-T4c n.º 39 |
| 39 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelGuardTests` | 4 | false | PB-T4c n.º 40 |
| 40 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullPanelCensusGuardTests` | 1 | false | PB-T4c n.º 41 |
| 41 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPanelDraftGuardTests` | 1 | false | PB-T4c n.º 42 |
| 42 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullPanelFieldTests` | 3 | false | PB-T4c n.º 44 |
| 43 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldValidationTests` | 13 | false | PB-T4c n.º 45 |
| 44 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldTests` | 10 | false | PB-T4c n.º 46 |
| 45 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostRedTests` | 24 | false | PB-T4c n.º 47 |
| 46 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostGreenTests` | 69 | false | PB-T4c n.º 48 |
| 47 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareRedTests` | 44 | false | PB-T4c n.º 49 |
| 48 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests` | 46 | false | PB-T4c n.º 50 |
| 49 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostRedTests` | 24 | false | PB-T4c n.º 51 |
| 50 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostGreenTests` | 57 | false | PB-T4c n.º 52 |
| 51 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditHostRedTests` | 59 | false | PB-T4c n.º 53 |
| 52 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditHostGreenTests` | 17 | false | PB-T4c n.º 54 |
| 53 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorRedTests` | 6 | false | PB-T4c n.º 55 |
| 54 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorGreenTests` | 70 | false | PB-T4c n.º 56 |
| 55 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestRedTests` | 53 | false | PB-T4c n.º 57 |
| 56 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestGreenTests` | 1 | false | PB-T4c n.º 58 |
| 57 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostRedTests` | 29 | false | PB-T4c n.º 59 |
| 58 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostGreenTests` | 18 | false | PB-T4c n.º 60 |
| 59 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostRedTests` | 23 | false | PB-T4c n.º 61 |
| 60 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostGreenTests` | 5 | false | PB-T4c n.º 62 |
| 61 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingRedTests` | 1 | false | PB-T4c n.º 63 |
| 62 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingGreenTests` | 21 | false | PB-T4c n.º 64 |
| 63 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackWritePathGuardRedTests` | 1 | false | PB-T4c n.º 65 |
| 64 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateIntegrationGuardRedTests` | 2 | false | PB-T4c n.º 66 |
| 65 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceEditIntentGuardRedTests` | 13 | false | PB-T4c n.º 67 |
| 66 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardRedTests` | 6 | false | PB-T4c n.º 68 |
| 67 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardGreenTests` | 17 | false | PB-T4c n.º 69 |
| 68 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationGuardGreenTests` | 1 | false | PB-T4c n.º 70 |
| 69 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceLifecycleGuardRedTests` | 2 | false | PB-T4c n.º 71 |
| 70 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateGuardRedTests` | 1 | false | PB-T4c n.º 72 |
| 71 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests` | 30 | false | PB-T4c n.º 73 |
| 72 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests` | 6 | false | PB-T4c n.º 74 |
| 73 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadPathRedTests` | 1 | false | PB-T4c n.º 75 |
| 74 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackFullReadGuardRedTests` | 1 | false | PB-T4c n.º 76 |
| 75 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardRedTests` | 1 | false | PB-T4c n.º 77 |
| 76 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllTests` | 6 | false | PB-T4c n.º 78 |
| 77 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllGuardTests` | 3 | false | PB-T4c n.º 79 |
| 78 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackOpenerClassicPathGuardTests` | 1 | false | PB-T4c n.º 80 |
| 79 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftLaunchGuardTests` | 1 | false | PB-T4c n.º 81 |
| 80 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllWindowTests` | 4 | false | PB-T4c n.º 82 |
| 81 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests` | 1 | false | PB-T4c n.º 83 |
| 82 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditAllNameWritersGuardTests` | 1 | false | PB-T4c n.º 84 |
| 83 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllApplyCloseTests` | 1 | false | PB-T4c n.º 86 |
| 84 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackRetirePilotGuardTests` | 1 | true | nuevo: CT-PBT4D-01 |
| 85 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackRetirePilotCensusGuardTests` | 1 | false | nuevo: CT-PBT4D-02 |
| 86 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackRetirePilotFullCopyGuardTests` | 1 | false | nuevo: equivalente de CT-PBT4B-20 (D-S12) |
| 87 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackRetirePilotPlanGuardTests` | 2 | false | nuevo: equivalente de las dos guardas de WPPG (D-S12) |
| 88 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackRetirePilotRunTests` | 31 | false | nuevo: las 29 sustitutas de WPUR (CA-T4d I-01) |

#### 6.4.2 Lo que necesita el generador de PB-T4d

| Pieza | Dónde | Lo que es |
|---|---|---|
| **La pieza del RED** | `pbt4/pbt4d-red-v54.txt` | sha256 `80f8cc0c5c9f54de033648e7eec95728fa0fc0d28e31886101207e3dbec381db`. Bloques EDIT (un OLD que está exactamente una vez en el texto que se va editando), DELETE (desde la v5.3) y NEWFILE, con `\n`; la aplican `red_pbt4d_v53.py` (`7a67652e3515083367aa12c829c1560b6742a007a30a354c205f0aa0eb7de5c2`) y `apply_red_pbt4d_v53.py` (`b99635ea0c26230c74611abc2560a05952850fd12916f06fc588fbe2a445a7dc`). La genera `v54/make_red_v54.py` (la de la v5.3 con las dos ediciones de comentarios de WPUR de M-12 a) (`7ed0074e03cbc49cc3a883416411ab96e9056815cdfb73d244a0e267147c268f`) desde los objetos git de la base, con `pbt4d_test_edits_v54.py` (`b8c78f726a32f6768c576f636d80f78a55922c7f9c475b389c4444974b360aea`), `v53/make_run_eq_v53.py` (`cd9fe688c5ce3eea136d87bf8893c6697b1c8d45f04629ab686765a3f2d1fb73`), `v53/unused_v53.py` (`b76acd7364052add873aca2bd1dbd72136e5085d86a1fecd27e6be1ea19d54c8`) y `v52/make_fullcopy_eq_v52.py` (`98065a8e8351bd32d698613a725c7128ad34e6b7c8087311fefaf0c74449f2c1`) |
| **La pieza del GREEN** | `pbt4/pbt4d-green-v54.txt` | sha256 `aaf6cab882cfb3eba6939fd8b64ec1fa15c22431ba8bdaeb9bcdf4dbb89ffbf0`. Bloques EDIT y DELETE; la aplican `green_pbt4d_v53.py` (`ea7b2086630ae827309164e7a33610b4cae694c9d3c406a2d457aa0a5858cc73`) y `apply_green_pbt4d_v53.py` (`70e7dd04a52b6975c1e228d26a07f9bb2749d206b36e32ca9172ae9d606062a0`). La genera `v53/make_green_v53.py` (`00a30017756dd6082625eb45a25b95934ea33e27474522675abb46f95b85fa92`): la pieza de la v5.2 (de `v52/make_green_v52.py`, desde la retirada B medida en `/d/r6e`, con la limpieza de D-S15 y la rama muerta de `BuildCore`), sin cambiar ninguno de sus bloques, y 11 bloques de solo comentarios de CA-T4d M-05 (a); en la v5.4, `pbt4d-green-v54.txt` es esa pieza con solo su cabecera corregida (recomprobación 1 de CA-T4d, M-12 b) |
| **La lista** | `pbt4/pbt4d-retire-list-271.tsv` | sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac`; la genera `v54/make_list_v54.py` (`5e6acc3263f2fbe3e457c117c0ed309a2616beb4ec35db76830d6dc3be8bdc05`): una fila por baja, edición o sustitución, con su grupo, archivo, miembro, líneas en `6c2820ba`, `Skip`, acción y porqué, y, desde la v5.3, sus declaraciones y su tramo editado (CA-T4d M-04) |
| **Los archivos nuevos** | en la pieza del RED (NEWFILE) | `NewFileSha` de cada uno (arriba) |
| **La precondición 6** | `pbt4dcc/compile/pbt4d-precondition6.json` | Schema `rackcad-pbt4d-precondition6/v1`; metadatos `Schema`, `Package`, `Task`; y, en este orden, `Revision`, `Pieces`, `List`, `RedFiles`, `RedDeleted` (los 3 borrados; el generador la compara con los bloques DELETE de la pieza del RED), `GreenFiles`, `GreenDeleted`, `Builds`, `CsWarnings`, `Alone`, `Red`, `Green`, `GreenWithCoverage`, `GreenUiCoverage` (las 3 corridas juntas y los 18 filtros del GREEN, con el sha256 de su TRX y de su JSON), `SuiteDurationSeconds`, `SlowTestSeconds`, `BaseCoverage`, `Reapply` (las piezas de la v5.4 reaplicadas en `/d/r70`), `Noise` (informativo: el ruido de D-S16; el generador no lo lee), `Coverage`, `Census`, `Control`, `Rehearsal`, `ClassResults`, `RequiredTests`, `UiTotals`, `History`, `Expected`, `AllAsExpected`; `Green.UI[n]` lleva también `Json`, y `BaseCoverage.UIByFilter.Runs`, el sha256 completo de la TRX y del JSON de cada filtro. sha256 `52e03e061cd62c79597f51b8eaf144a27d68b3bf5db876406a804be2fa978989`; la corrida, en su `compile-check.md` (`d13afdb02216d385c32ade81b5cf9508c648a970337cf92b9d964b936933de42`), con el resultado de cada clase que corre (Passed, Failed y Skipped) en cada rama. La carpeta va a `F4B-DRAFTS/pbt4/cc4d/`; **nombres cortos:** ninguna ruta pasa de 156 caracteres con el prefijo `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/` |

**Las autoridades de PB-T4d** (la n.º y la ruta son las del contrato de PB-T4c emitido):
- **Siguen, sin cambio:** las de PB-T4c que no son de su subtarea.
- **No son autoridades de PB-T4d:** las de las tandas, las listas, los equivalentes y la precondición 6 de PB-T4c, y sus `UNIT_CHANGE`.
- **Se añaden:** `owner-pbt4d-list-decisions.md`, `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md`, `owner-pbt4d-cat4d-decisions.md` y `session-pbt4d-design-decisions.md` (D-S11 a D-S16); la comprobación acotada de la v5.2 (`independent-review-pbt4d-check.md`); esta versión del paquete (v5.4); la lista; las dos piezas; y la precondición 6 (`pbt4d-precondition6.json` y `compile-check.md`).

## 7. Riesgos y lecciones aplicadas

Los de la v1 (R-T4-01 a 15) y la v4 (R-T4-16 a 21), con lo que hacen la v4.1 y la v4.2:

| Riesgo | Qué hace la v4.2 |
|---|---|
| R-T4-01, la lista | Las listas, guardadas método a método, con su causa y su destino; la de PB-T4b, fijada por sha256 y aprobada por el Owner (470; en la tercera tanda, 475; en la quinta, 477; en la sexta, 478) |
| R-T4-08, los miembros por defecto | Solo `TryEditAll`, con sus dos reenvíos explícitos (CT-PBT4C-12 y 13) |
| R-T4-13, el modal y el abridor | Dos piezas; CT-PBT4C-08 fija el literal de la conversión (M-30) |
| **R-T4-27, lo que el paseo no ve** (V50-12) | La reflexión sobre tipos y el comportamiento de la UI: las dos caídas de la v5.0 las vio la suite completa de Core y los filtros de UI de la precondición 6, no el paseo. Lo que vale es la suite completa |
| **R-T4-28, un nombre fijado y nunca ejecutado** (D-S9) | `IWorkspaceFullEditorHost` estaba en el paquete desde la v4 y una guarda por reflexión lo prohibía; se midió al compilar y correr el GREEN literal |
| **R-T4-29, las CT no corren en la precondición 6** (CA-T4c I-01; V51-05) | El paso 8: el ensayo de las CT de texto, sobre el RED y sobre el GREEN, con su control sobre la v5.0, y el censo de C13. Lo que no cubre, declarado (§6.3.1) |
| **R-T4-30, la lista por texto no es la necesaria** (V52-02) | Se mide desde cero bajas, compilando (`/d/r6h`): 34 de las 235 bajas de B no hacían falta, entre ellas 7 CT de PB-T3b |
| **R-T4-31, compilar no basta** | Un nombre que se queda sin su campo se resuelve como otro (`Comparer.Equals` como `System.Collections.Comparer`): la prueba compila y cae, o pasa en falso. Se corre la suite completa |
| **R-T4-32, «lo cubren otras» se mide, rama por rama** (V52-03) | Con coverlet: primero por líneas (`/d/r6j`, `/d/r6k`), y el relevo vio que el método prueba a prueba contaba ramas, no cuáles; la precondición 6 compara la base y el GREEN rama por rama, en JSON |
| **R-T4-33, código muerto tras la retirada** (V52-04) | Una rama que solo alcanzaba lo retirado no la puede cubrir ninguna sustituta: el GREEN la quita (`allowErase`), por decisión del Owner |
| **R-T4-34, el universo de la cobertura** (CA-T4d I-02; V53-02) | Medir solo lo que una clase ejecutaba no ve a quien llama desde otro archivo ni lo que pierden las demás bajas: se compara toda la suite (Core y los filtros de UI) sobre todo `src/` que se queda |
| **R-T4-35, un porqué que no se mide** (CA-T4d I-01; V53-01) | «Usa la corrida del piloto como vehículo» valía para 19 de las 21 bajas de WPUR; la 48 y la 49 probaban otra cosa. Un porqué de baja se comprueba con la cobertura ampliada |
| **R-T4-36, código muerto en `tests/`** (CA-T4d M-03; V53-05) | Una retirada deja ayudantes sin uso y clases sin pruebas: se miden con IDE0051/IDE0052 en una copia y por texto, en vueltas hasta que no sale ninguno, y se borran con registro |
| **R-T4-37, la copia principal, sola** (CA-T4d M-07; V53-09) | Una corrida solapada no cambia los resultados, pero sí las duraciones: el conductor comprueba al empezar que no hay otro `dotnet` ni `testhost` |
| **R-T4-38, el ruido de la cobertura de UI** (V53-02) | Un binding de WPF da golpes que dependen del despachador (`RackViewBatchDialogPresenter.cs:37-38`: la base da 1, 0 y 1 en tres corridas de la misma prueba, y en la unión de los filtros el GREEN da 0 tres veces y 1 tres veces con su filtro solo, `noise-batch/`): la UI de cada rama es la unión de tres corridas de la unión de los filtros y de cada filtro en su proceso, como los corre el contrato |
| R-T4-16, la doble lectura del piloto | Se mide en la precondición 6. En PB-T4a, la relectura en el origen repite las dos lecturas puras sobre el mismo escaneo, en `Draft/`, sin acceso nuevo al dibujo. En PB-T4b, D-S4 deja una sola materialización del modelo completo por lectura (D-PBT4-21), y la precondición 6 mide el coste con la sonda de `read-cost` |
| R-T4-17, el coste de `IsDirty` | Sin cambio |
| R-T4-19, la duración de la suite de Core | Se mide en la precondición 6 de PB-T4b; si pasa de 15 min, vuelve a la sesión |
| R-T4-20, mover la validación numérica | `NumericFieldValidationTests` (UI) y CT-PBT4B-18; PBT4B-C20. El esqueleto de PB-T4b no toca `NumericFieldValidation.cs` (M-29) |
| R-T4-21, los riesgos del borrado real | Al Smoke-PB (§5.5) |
| R-T4-22, una guarda que fija una firma entera | EIG:139-141 fija la de `RackPilotIntentAdapter.Read`; `AtOrigin` lo evita |
| **R-T4-23, la retirada rompe por compilación** | Un `Skip` no evita un error de compilación. **El Owner la puso después de PB-T4c** (PB-T4d): PB-T4b no borra ni edita nada, y PB-T4d borra con registro, cuando ya no corre ninguna de esas pruebas (I-26) |
| R-T4-24, las copias en el RED | Partidas por su resultado en el RED, con una partición cerrada medida (§5.2) |
| R-T4-25, el léxico de las mediciones | Corregido otra vez en la v4.2 (`@"""…`); las listas no cambian |
| **R-T4-26. Una guarda que cuenta una llamada en todo un archivo** (B-08) | EIG:357 exige un solo `PushBackPilotIntentReader.Read(` en todo RIA, y la v4.1 lo medía solo en el cuerpo de `Read`. **La lección:** las guardas de los archivos tocados se recorren enteras, prueba a prueba (§4.1), no por selección; lo que no se pueda ver se declara con su parada |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas antes de PB-T4

Las de la v1 (§8.1): [OD] con D-PB-01 y D-PB-05; F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; F4B-E15, E21, E25, E35 Q4, E37, E42, E45 y E46; D-PB-10.

### 8.2 Las decisiones sobre PB-T4 (2026-11-20)

**Los registros literales:** `owner-pbt4-decisions.md` (las cuatro primeras) y `owner-pbt4b-retire-list-decisions.md` (las dos de la vuelta de PB-T4b, redactadas en la §16.5 de la revisión). La numeración F4B-E se asigna al custodiarlos, con la emisión de PB-T4a. Las preguntas se numeran como en esos registros.

**La primera vuelta** (`owner-pbt4-decisions.md`):

| Pregunta | Respuesta del Owner, literal | Lo que aplica la v4.2 | Lo que no se hace |
|---|---|---|---|
| **1, «Piloto»** | «Retirarlo (Recomendado)» | D-PBT4-10: se retira lo que solo usaba el panel; la carga se queda dentro del lector, que usa el modelo completo para AUTH-13. **Cuándo,** lo dice la segunda vuelta: en PB-T4d | «Conservarlo sin uso» y «Dos modelos hasta F5-A» |
| **2, «Presentación»** | «Por grupos (Recomendado)» | D-PBT4-09, con los nueve nombres fijos, la unión, el plural y las líneas de lo estructural y de las vistas | «Por campos» y «Por campos con valores»: sin `PushBackFieldLabels` ni `PushBackPresentationForm` |
| **3, «Pruebas F4»** | «Saltar y duplicar (Recomendado)» | D-PBT4-11: las 428 se saltan y se duplican con un kit nuevo, como en D-1, con el mapa cerrado y el guion de CT-PBT4B-20; sin excepción a C02 en PB-T4b | «Editar una línea» (sin excepción declarada a C02 ni edición de EIH) y «Implementación de referencia» |
| **4, «Emisión»** | «Sí, con condiciones (Recomendado)» | PB-T4a, PB-T4b y PB-T4c, en ese orden, cada una tras el cierre de la anterior, sin volver al Owner si la comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición no cambia nada material y la lista emitida es la que vio el Owner o una parte de ella. **No cubre PB-T4d** | «Solo PB-T4a» y «Cada subtarea me vuelve» |

**La segunda vuelta** (`owner-pbt4b-retire-list-decisions.md`):

| Pregunta | Respuesta del Owner, literal | Lo que aplica la v4.2 | Lo que no se hace |
|---|---|---|---|
| **1, «Retirada»** | «Retirar tras PB-T4c (Recomendado)» | D-PBT4-10 y §6.4: el código del piloto queda sin uso en PB-T4b y PB-T4c, y lo retira **PB-T4d**, después de PB-T4c, que sustituye las 139 y borra con registro las 55 originales saltadas y los 29 ayudantes, y **vuelve al Owner con su lista antes de emitirse** | «Retirar en PB-T4b» y «Conservarlo sin uso» |
| **2, «Lista T4b»** | «Sí, esa lista (Recomendado)» | D-PBT4-11: la lista de PB-T4b son las **470** de `pbt4b-substitution-list.tsv` (sha256 `44393b19…63e4`), que el Owner toma como «la que vio»; PB-T4b se emite con ella por la autorización condicional, si se cumple el resto de la condición | «Vuelve antes de emitir» |

**Vuelve al Owner** (pregunta 4, literal): «por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista». Si eso pasa en mitad de una subtarea, el Worker para (C16 y las paradas de §6). **Y PB-T4d, siempre, con su lista.**

### 8.3 Las listas guardadas y lo que el Owner aprobó (I-22, I-23, I-26, M-32, M-33)

| Subtarea | Lista | Lo que vio el Owner | Exacto | Su estado |
|---|---|---|---|---|
| PB-T4a | CT-PBT3A-12 (C-5) | 1 | **1** | Autorización condicional (pregunta 4) |
| PB-T4b | Las 9 clases (C-0), «Saltar y duplicar»: **356 copias mecánicas y 72 equivalentes a mano** (D-S1 y D-S5; la v4.3, 375 y 53; la v4.5, 368 y 60; la v4.6, 360 y 68; la v4.7, 358 y 70; `pbt4b-copy-map-v5.tsv`). Los nombres de las dos de WPP de la v4.8 se conservan por el mecanismo (CT-20 (2) y la partición), y la regla es la del cuerpo (CA-T4b M-02) | 428 | **428** | |
| PB-T4b | La letra de las guardas (C-5 y C-6) | «unas 12»; en la tercera tanda, 20; en la quinta, 22; en la sexta, **23** | **23** (15, las cinco de la tercera tanda, las dos de PB-T4a y la de la sexta) | |
| PB-T4b | La UI de la elección (C-3) | «hasta 21», recontada a unas 25 | **27** | |
| PB-T4b | **Total** | **unas 465**; en la segunda vuelta, **470**; en la tercera, **475**; en la quinta, 477; en la sexta, **478** | **478** | **Aprobada como la que vio** (segunda vuelta, pregunta 2, y tercera, quinta y sexta tandas); `pbt4b-substitution-list-478.tsv`, sha256 `8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b` (las de 477, `5975c5b6…a651`, 475, `654c3b7b…0707c`, y 470, `44393b19…63e4`, quedan como historia) |
| PB-T4c | PEG:155-163, CT-PBT1B-15 y CT-PBT4B-20 (C-5, Core), y CT-PBT2B-06 (C-3, UI) | 2; tras la remedición, 3 y 4 | **4** | **Aprobada como la que vio** (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`); `pbt4c-substitution-list-4.tsv`, sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de` (la de 3, `d83e0a817144fa99f95ed4bfd151b822a4fc1035c10f736f77e8f8e6350186c1`, queda como historia) |
| PB-T4d | La lista del alcance B fila a fila (C-7, originales, apoyo, compilación, ejecución, M-04, la lenta, SUS-01, M-03 y los ayudantes editados), con las 29 sustitutas de WPUR, la rama muerta de `BuildCore` y, desde la v5.3, sus declaraciones y su tramo editado | El esbozo: 138 + 55 + 29; B medida: 235 bajas | **271** con acción (284 filas) | **La elegida** (`owner-pbt4d-list-decisions.md`, pregunta 1, con `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md` y `owner-pbt4d-cat4d-decisions.md`); `pbt4d-retire-list-271.tsv`, sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac` (la de la v5.2, `pbt4d-retire-list-229.tsv`, `1943468ba9a9f342b74bb8ea2c57e1b664c7751b52344e7a712ea7bc2e151fad`, y el esbozo, `pbt4d-retire-list-138.tsv`, `d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`, quedan como historia) |

**Las cifras que corrige la v4.2** (M-32): de las 67 pruebas que dejan de compilar al retirar, `WorkspacePilotUpdateRunTests` da **42** y `PushBackFullWritePrepareTests`, **6**; en C-7, por texto y por compilación, son **48** (de las 49 de la clase) y **9** (de 9). Las demás de compilación: `WorkspaceDraftPilotOwnershipTests` 8, `WorkspaceDraftPilotStateTests` 7, `PushBackDeclaredCorrectionPilotParityTests` 2, `PushBackFullWriteRunTests` 1 y `WorkspaceDraftPilotMaterializationTests` 1.

**Lo que cambia y lo que no al retirar después de PB-T4c** (I-26): la retirada no evita el borrado de las 55 originales saltadas y de los 29 miembros de apoyo; cuando se retire, dejarán de compilar igual. Lo que cambia es que, para entonces, ya no corre ninguna, porque sus copias y sus equivalentes están en verde desde PB-T4b, y que PB-T4b conserva la premisa de la pregunta 3 de la primera vuelta.

**Las 478 no dependen de la retirada** (M-33): las de la letra que nombran lo que se retirará caen por el cambio del anfitrión (§4.2), también WPUG:822-840, que la v4.3 ponía en PB-T4d.

**Lo declarado, sin preguntar** (como en la v4):
- que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro;
- el límite del testigo (M-20);
- el cambio observable de PB-T4a (M-23);
- que la validación numérica pasa de la UI a Application, sin cambiar su comportamiento (D-PBT4-06);
- **que la firma de `RackPilotIntentAdapter.Read` no cambia** y la relectura en el origen es pura, en `Draft/`, sobre el mismo escaneo (D-PBT4-03; V41-06; B-08);
- **que, entre PB-T4b y PB-T4d, la regla de la escala queda también en `PushBackPilotFieldAdapter`,** sin uso (D-PBT4-10).

### 8.4 Lo que no es decisión

Lo de la v3 (§8.3): el orden de las subtareas, los nombres, las CT y las guardas medidas. Además:
- **el abridor en dos piezas;**
- **la validación numérica movida a Application,** que no cambia el comportamiento. Si la comprobación acotada o la precondición 6 muestran una guarda de T1B que lo impide, vuelve al Owner por OB-T1B.3;
- **`AtOrigin` y `ReadAt` en lugar de un parámetro nuevo de `Read`:** lo imponen dos guardas medidas (EIG:139-141 y EIG:350-364), sin cambio de comportamiento;
- **la partición de las copias por su resultado en el RED** (§5.2; la T-1 de la §16.4), técnica.


---

### 8.5 Las tandas del Owner sobre PB-T4 y las decisiones de la sesión (V44-08; V45-12)

| Tanda | Registro | Las preguntas y la respuesta literal | Lo que fija para PB-T4b |
|---|---|---|---|
| **1.ª** (2026-11-20) | `owner-pbt4-decisions.md` | Pregunta 1, «Piloto»: «Retirarlo (Recomendado)». Pregunta 2, «Presentación»: «Por grupos (Recomendado)». Pregunta 3, «Pruebas F4»: «Saltar y duplicar (Recomendado)». Pregunta 4, «Emisión»: «Sí, con condiciones (Recomendado)» | El modelo completo en el panel sin traducir; la presentación por grupos; las 428 del panel, saltadas y duplicadas con un kit nuevo y un guion, sin editar pruebas ni ayudantes; la emisión condicional, tras el cierre de la anterior, con la lista que vio el Owner o una parte, y la vuelta por una decisión nueva o una prueba que falle fuera de la lista |
| **2.ª** (2026-11-20) | `owner-pbt4b-retire-list-decisions.md` | Pregunta 1: «Retirar tras PB-T4c (Recomendado)». Pregunta 2, «Lista T4b»: «Sí, esa lista (Recomendado)» | El código del piloto queda sin uso hasta PB-T4d, que vuelve al Owner con su lista; la lista de PB-T4b, 470, la que vio |
| **3.ª** (2026-11-20) | `owner-pbt4b-breaks-decision.md` | «Roturas T4b»: «Añadirlas a la lista (Recomendado)». La otra opción, «Mantener código muerto», no se hace | **La lista de PB-T4b pasa de 470 a 475** (EIG:871, EIG:883, WLG:627, WLG:642 y WPUG:823), cada una con su equivalente sobre el modelo completo; **la de PB-T4d, de 139 a 138**; **el `MinSelected` de WPUG, de 86 a 85** |
| **4.ª, la de PB-T4a sobre el coste** (2026-11-20) | `owner-pbt4a-cost-decision.md` (en `F4B-DRAFTS/coordinator/` del cierre `78a7cef3`) | «Coste T4a»: «Cerrar y optimizar en PB-T4b (Recomendado)». «Cerrar y medir en Smoke-PB» y «Corregir PB-T4a ahora» no se hacen | PB-T4a cierra con el coste registrado como limitación; **PB-T4b elige la vista de origen antes de leer, para materializar el modelo completo una sola vez por lectura** (D-S4, D-PBT4-21); el coste que quede se vuelve a medir con la misma prueba sobre el GREEN de PB-T4b (la precondición 6) y en el Smoke-PB; si la optimización rompe una prueba fuera de la lista, vuelve al Owner |
| **5.ª** (2026-11-20) | `owner-pbt4b-list-477-decision.md` | «Lista T4b»: «Añadirlas a la lista (Recomendado)». La otra opción, «No emitir PB-T4b», no se hace | **La lista de PB-T4b pasa de 475 a 477** (CT-PBT4A-03 y CT-PBT4A-05), cada una con su `Skip` y un equivalente escrito a mano sobre el borrador completo; es la lista que vio el Owner, y si aparece otra prueba que falle fuera de ella, vuelve al Owner |
| **6.ª** (2026-11-20) | `owner-pbt4b-list-478-decision.md` | «Guarda T4b»: «Añadirla a la lista (Recomendado)». «Mantener las cuentas» y «No emitir PB-T4b» no se hacen | **La lista de PB-T4b pasa de 477 a 478** (WHHG `NoneOfThe30…`), con su `Skip` y su equivalente `WorkspaceHostHardeningGuardFull`, la misma tabla con las cuentas 0, 6 y 0, cada una con su `// before N`; es la lista que vio el Owner, y si aparece otra prueba que falle fuera de ella, vuelve al Owner |
| **7.ª** (2026-11-21) | `owner-pbt4b-cost-decision.md` | «Coste T4b»: «Darlo por cumplido (Recomendado)». «Sonda dedicada antes» y «No emitir PB-T4b» no se hacen | **El coste de la lectura de la precondición 6 se da por cumplido** con dos de tres corridas que cumplen todo (la primera incumple un solo caso, `pb-20-ab x4 vistas +200 racks ajenos`, `origen-es-Primary`); las tres corridas y sus TRX, por sha256, van con la emisión; el coste real en AutoCAD queda para el Smoke-PB; **no cambia ningún criterio del paquete** |

| **8.ª** (2026-11-21) | `owner-pbt4c-list-3-decision.md` | «Lista T4c»: «Añadirla a la lista (Recomendado)». «Otro diseño sin tocarla» y «No emitir PB-T4c» no se hacen | **La lista de PB-T4c pasa de 2 a 3** (CT-PBT4B-20), con su `Skip` y su equivalente, copia literal que además reconoce el `Skip` de PB-T4c (lo que fija para PB-T4c) |
| **9.ª** (2026-11-21) | `owner-pbt4c-list-4-decision.md` | «Lista T4c»: «Añadirla a la lista (Recomendado)». «Preguntar distinto» y «No cambiar «Cerrar» ahora» no se hacen | **La lista de PB-T4c pasa de 3 a 4** (CT-PBT2B-06), con su `Skip` y su equivalente, copia literal con la línea de la ventana 4 a la pregunta nueva (lo que fija para PB-T4c) |
| **10.ª** (2026-11-22) | `owner-pbt4d-list-decisions.md` | «Lista T4d»: «Alcance B (Recomendado)». «Recarga»: «Tarea propia después (Recomendado)». «Prueba lenta»: «Acotarla en PB-T4d (Recomendado)». «Emisión T4d»: «Sí, con condiciones (Recomendado)» | PB-T4d retira el alcance B; la recarga de «Editar todo» va a una tarea propia; la prueba lenta se acota (D-S14); la emisión es condicional (lo que fija para PB-T4d) |
| **11.ª** (2026-11-22) | `owner-pbt4d-substitutes-decision.md` | «Las 138»: «Sustitutas donde hace falta (Recomendado)» | Se quedan 7 CT de PB-T3b y sus 6 ayudantes; lo que también prueba código que se queda conserva esa parte; lo demás se borra con registro |
| **12.ª** (2026-11-22) | `owner-pbt4d-run-coverage-decision.md` | «Corrida»: «Sustituir las 21 (Recomendado)» | Las 21 de WPUR, equivalentes sobre `ExecuteFull` en `PushBackRetirePilotRunTests`; la precondición 6 mide otra vez la cobertura |
| **13.ª** (2026-11-22) | `owner-pbt4d-deadcode-27-decisions.md` | «Rama muerta»: «Quitarla en PB-T4d (Recomendado)». «27 en vez de 21»: «Sí, 27 (Recomendado)» | El GREEN quita `allowErase` y lo que solo existe con él; las sustitutas pasan a 27 (y la 06, 07, 16, 19, 29 y 42) y se borran 21; la cobertura, rama por rama |
| **14.ª** (2026-11-22) | `owner-pbt4d-cat4d-decisions.md` | «48 y 49»: «Sustituirlas (Recomendado)». «Huecos nuevos»: «Sustituirlos siempre (Recomendado)» | La 48 y la 49 de WPUR pasan a sustitutas sobre el modelo completo (29); la cobertura de la precondición 6 mira todo `src/` que se queda; todo camino que se queda y pierde su única prueba recibe una sustituta sin volver al Owner, salvo que una no se pueda escribir o salgan más de 10 nuevas sobre las 29; lo que cubra la UI se mide con sus filtros |

**Las decisiones de la sesión,** que no son del Owner:
- **D-S7 a D-S10** (`session-pbt4c-design-decisions.md`, 2026-11-21; para PB-T4c): **D-S7**, la costura de CT-09 es `HeaderConfiguratorDialog`; **D-S8**, los filtros nuevos, por clase exacta; **D-S9**, `IWorkspaceFullEditHost` (y `SetFullEditHost`, `_fullEditHost`), por `WorkspaceModelBoundaryTests:101`; **D-S10**, CT-09 cubre lo que se puede alcanzar (en WORKSPACE, un módulo escenificado deja siempre cambios sin aplicar). Ninguna cambia la lista ni el comportamiento visible decidido por el Owner;
- **D-S11 a D-S15** (`session-pbt4d-design-decisions.md`, 2026-11-22; para PB-T4d): **D-S11**, el RED lleva todo lo de `tests/`, sin esqueleto, y dos CT (CT-PBT4D-01 con `ExpectRed` true y CT-PBT4D-02 con false); **D-S12**, las tres que caen al ejecutar, con `Skip` y equivalente, prefijo `PushBackRetirePilot` y filtros por clase exacta; **D-S13**, los vacíos de M-04, en su sitio; **D-S14**, la prueba lenta, cada 97.º toque y el último; **D-S15**, los cuatro `cref` y el comentario, y `PushBackPilotDraftState.cs` en `UNIT_CHANGE`.
- **Tras CA-T4d** (la sesión, en su orden para la v5.3; no son decisiones del Owner): M-01, el corpus de CT-PBT3B-06 (iii) pasa al modelo completo en su sitio, con la regla del Owner; M-02, la rama de `MirrorSelection`, medida con la cobertura de los filtros de UI; M-03, los ayudantes sin uso y las clases sin pruebas, borrados con registro; M-04, la lista fila a fila con sus declaraciones y su tramo editado; M-05, los comentarios del GREEN que describían el piloto como presente, y «cuatro condiciones»; M-06, el número de la clase de las sustitutas; M-07, la copia principal sola y con los 18 filtros del contrato.
- **D-S16** (`session-pbt4d-design-decisions.md`, 2026-11-22; para PB-T4d, tras CA-T4d): la cobertura de la UI de cada lado es la unión de tres corridas de todos los filtros juntos y de una de cada filtro en su proceso (un binding de WPF da golpes que dependen de lo que corrió antes; R-T4-38), y el informe lista lo que baja (informativo); **con su nota tras la recomprobación 1 de CA-T4d** (M-11): cada entrada nombra la corrida, no la prueba, y ESTABLE solo por la UI exige también un filtro en su proceso.
- **Una errata, declarada** (CA-T4d M-06): la pregunta 2 de `owner-pbt4d-deadcode-27-decisions.md` dice «se borran 22 en vez de 27»; eran **21** pruebas (48 − 27): con `StatusAfter`, 22 filas de WPUR salían del archivo, pero `StatusAfter` va copiado en la clase nueva. La v5.2 la aplicó como 21 sin declararla. En la v5.3, con la 48 y la 49 sustituidas, se borran 19 y una se queda en WPUR.
- **D-S1 a D-S3, D-S5 y D-S6** (`session-pbt4b-design-decisions.md`, 2026-11-20): **D-S1**, las siete copias mecánicas que no pueden pasar en el GREEN, y las que encuentre la precondición 6, pasan a equivalentes escritos a mano, con su registro y su texto literal, y la lista no cambia por eso; **en la v4.6**, la sesión aplica D-S1 a ocho más, que no compilaban sobre el modelo completo (V46-02), y decide que el guion añada la directiva de `Draft/` (MU, V46-01) y admita ediciones de ayudante en las copias de C-0, sin volver al Owner; **D-S5** (tras la v4.6; V47-01 y V47-02), las cuatro `Theory` que mezclan resultados en el RED (WLHT, PEG y las dos de WPP) se parten por filas en dos equivalentes escritos a mano, con el mismo cuerpo y las mismas aserciones (las dos de `MemberData`, con dos miembros de datos nuevos en la copia), y la regla de la partición se amplía solo para eso, con el mecanismo de D-S1 y sin volver al Owner, porque la lista, las aserciones, las filas y los totales no cambian (358/70 en C-0); **D-S6** (tras la v4.7; V48-01), las copias en el espacio de nombres raíz de su ensamblado, con el prefijo `FullPanelCopy` en sus clases, para que `NamespaceFolderGuardTests` siga en verde sin tocarla ni tocar ningún filtro heredado (el precedente de D-S2); y **D-S1 en la v4.8** (V48-02), las dos copias de WPP que fijaban que el piloto nunca borra una vista pasan a equivalentes que afirman lo que ya fijan D-PB-04, D-PBT3-09 (e) y D-PBT4-07 (356/72 en C-0); **D-S2**, «Conservar» con dos «Sí», rechazada la variante en la que el núcleo conserva sin testigo; **D-S3**, el alias, el testigo desde `Draft/` y `FullPrepareKit`. El Owner fue informado de D-S1 y D-S2 antes de la tercera tanda, con el texto exacto que copia `owner-pbt4b-breaks-decision.md`;
- **D-S4** (en desarrollo de la 4.ª tanda; D-PBT4-21): la lectura completa del `Primary`, diferida en RIA (`System.Lazy`), y `Full` como `_full?.Value`; `AtOrigin`, NH y `ReadAt` sin cambio; RIA en el alcance como `UNIT_CHANGE`; CT-PBT4B-21; y la sonda del coste en la precondición 6, con una materialización en los cuatro casos.

Se declaran en la emisión de PB-T4b (F4B-E52), que custodia los siete registros del Owner sobre PB-T4 y la decisión de la sesión. **Las decisiones de la cadena de PB-T4a** (`owner-pbt4a-verify-stop-decision.md` y `owner-pbt4a-nc1-decision.md`, F4B-E51) no son autoridades de PB-T4b.

**Lo que el paquete no decide:** ninguna tanda se reinterpreta. La v4.5 aplica la 4.ª y la 5.ª como dicen (la materialización única, sin romper ninguna prueba fuera de la lista, medido en §4.2; las dos de PB-T4a, sus equivalentes y su `Skip`) y D-S1 a D-S6 como las escribió la sesión; la v4.8 aplica la 6.ª como dice (la guarda de los textos del anfitrión, en la lista con su equivalente), y la v4.9, la 7.ª (el coste, dado por cumplido, sin cambiar ningún criterio). **M-02 de la revisión** (las vistas en hasta tres líneas, frente a «otra de las vistas» de la opción del Owner) se declara en F4B-E52 (D-PBT4-09).

## 9. Límites

- Nada se compiló ni se ejecutó, y el worktree no se leyó: todo es por objetos git en `7f0d9aa3`, en `5aaacce3` (la v4.4, con el GREEN literal de PB-T4a en memoria) y, para la v4.5, en el cierre de PB-T4a, `78a7cef3` (su `src/` y sus `tests/` son los de `faef7020`, y su `src/` es el GREEN literal).
- **Las listas son recuentos por texto** (el método y los ayudantes de su archivo, a un nivel) **y, para la retirada, por referencia de código transitiva.** Pueden contar de más (un nombre común que casa en otro sentido) o de menos (una dependencia por reflexión o por un nombre que no se ve en el texto). La de PB-T4b es la que aprobó el Owner (470, 475 en la tercera tanda, 477 en la quinta y 478 en la sexta); la de PB-T4d (138) es un esbozo que se vuelve a medir.
- **Las guardas de PB-T4a se recorrieron por marcas, por diferencia,** sobre el texto literal del GREEN que prescribe este paquete (§4.1), y las 100 señaladas se leyeron una a una. **No es un recorrido entero** (CA-T4a M-01): las marcas no ven las rutas compuestas con `Path.Combine` ni los barridos de todo `src/`, de todo Application o de todo el Plugin; CA-T4a midió aparte esos 97 archivos, y ninguno cae. Lo que queda es una lectura que solo se ve al ejecutar: la mide la precondición 6 con `apply_green_pbt4a.py`, y si cae una prueba fuera de la lista es PBT4A-C16.
- **El léxico de C# de las mediciones** (`lists_core.py`) se corrigió en la v4.2 para las cadenas verbatim que empiezan por una comilla escapada (`@"""…`), que tomaba por cadenas crudas: tres archivos de las listas las tienen (EIG, RG y `WorkspacePilotUpdateGuardTests`). **Las listas y sus sha256 no cambian** (comprobado: `pbt4-substitution-lists.tsv` sigue en `f24c2dbc…`).
- **Medidos en la copia exportada, para PB-T4c** (V50-12; V51-06): K = 4, D = 0 y E = 4 (`/d/r5j`), el coste de `IsDirty` (`/d/r5k`), las líneas de las sondas y el ensayo de las CT de texto (`/d/r5i`), y el control del ensayo sobre la v5.0 (`/d/r5l`); los de la v5.0 (`/d/r5f`, `/d/r5g` y `/d/r5h`), como historia. **Lo que el ensayo no cubre:** las CT de comportamiento salvo por las sondas (§6.3.1). **Lo que el paseo de guardas no ve:** la reflexión sobre tipos y el comportamiento de la UI; lo ve la suite completa de Core y los filtros de UI.
- **Medidos en copias exportadas, para PB-T4d** (V52-06; V53-10): la necesidad de cada baja (`/d/r6h`), la cobertura (`/d/r6j`, `/d/r6k`), los ensayos (`/d/r6l`, `/d/r6n`, `/d/r6v` y `/d/r6x`) y la precondición 6 de la v5.3 (`/d/r6w`, `/d/r6y` y `/d/r6z`); las demás, historia. **Lo que la cobertura no ve:** la cobertura de datos, el Plugin y la UI fuera de los filtros (§4.4).
- **Quedaban para la copia exportada:** K, D y E de PB-T4c; en PB-T4b, la partición de las copias en el RED (R_C y R_UI), el RED exacto entero, que las copias pasen en el GREEN, las lecturas de las guardas que solo se ven al ejecutar (el recorrido por diferencia, §4.2, no las ve), las líneas «presentar» de CT-PBT4B-06, el coste de la lectura con D-S4, el coste de `IsDirty` y la duración de la suite de Core.
- **El miembro del informe de reconciliación** para la condición 7 se nombra como el que lee CT-PBT1B-16. La línea base va por `SetWorkingBaseline`.

## 10. Lo que queda para PB-T5

- El Smoke-PB, con la lista de §5.5.
- La CI exacta del Candidato.
- El gate-close de F4B.

## 11. Las precondiciones de emisión

1. **PB-T3c CERRADA:** cumplida (`7f0d9aa3`, F4B-E48).
2. **Las respuestas del Owner:** cumplidas (`owner-pbt4-decisions.md`, `owner-pbt4b-retire-list-decisions.md` y, para PB-T4b, `owner-pbt4b-breaks-decision.md`, `owner-pbt4a-cost-decision.md`, `owner-pbt4b-list-477-decision.md`, `owner-pbt4b-list-478-decision.md` y `owner-pbt4b-cost-decision.md`; y, para PB-T4c, `owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`), con su número F4B-E al custodiarlas; y las decisiones de la sesión, `session-pbt4b-design-decisions.md` (D-S1 a D-S3, D-S5 y D-S6) y D-S4 (§8.5), declaradas en F4B-E52. **Para PB-T4d:** `owner-pbt4d-list-decisions.md`, `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md` y `owner-pbt4d-cat4d-decisions.md`, y `session-pbt4d-design-decisions.md` (D-S11 a D-S15), con su número F4B-E al custodiarlas.
3. **La comprobación acotada** de cada subtarea: 0 bloqueantes y 0 importantes sin resolver (pregunta 4).
4. **Las celdas, la Snapshot y el binario,** vigentes.
5. **La remedición sobre el cierre de la subtarea anterior** no cambia nada material (pregunta 4).
6. **La comprobación sobre una copia exportada:**
   - el esqueleto exacto de la subtarea compila;
   - la suite completa de Core y los filtros de UI dan lo previsto (PB-T4a: **19 129 = 19 122 + 7** con el esqueleto y el `Skip`);
   - el recorrido de las guardas por marcas sobre el texto literal del GREEN (PB-T4a: §4.1, 1 026 pruebas, una que cae, la de la lista), la ampliación de CA-T4a (97 archivos, ninguno cae) y las aserciones traducidas (`measure_pbt4_v43-out.txt`, 36 de 36);
   - **PB-T4a, el GREEN literal compilado** (V43-03): `python -I apply_green_pbt4a.py <dir-copia> --repo <checkout-principal> --rev 7f0d9aa3` sobre una copia exportada de `7f0d9aa3` (sin `.git`); con el `Skip` de CT-PBT3A-12 puesto, la suite completa de Core da **19 129 = 19 122 + 0 + 7**, y ningún `warning CS` en los registros del Plugin, de Core y de UI (los `MSB3277` son de las referencias locales de AutoCAD);
   - **cero `warning CS`** también sobre el esqueleto (CA-T4a I-01);
   - **los 12 filtros de UI** de los `RequiredTests` de PB-T4a (n.º 8, 15, 20, 21, 23, 25 a 27 y 35 a 38) sobre la copia, o la declaración, en `compile-check.md` y en `COMPILE_RESULT`, de que no se corrieron; en ese caso los cubre el trabajo `ui-tests` de la CI del RED (CA-T4a M-03);
   - el barrido de nombres y de sobrecargas;
   - **PB-T4b** (V45-04, V46-07, V47-05, V48-06 y V49-06; los ocho pasos de §6.2.1, con los guiones de la v4.9, y el paso 7, el de la v4.8 (V49-06); la corrida, en `pbt4bcc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4a (`78a7cef3`): `apply_red_pbt4b_v49.py` en modo de medida y `partition_pbt4b_v48.py red` (con la regla de INV-PBT4B-02 y el reparto por filas de D-S5): la partición, su sha256 y R_C y R_UI; **el RED exacto** con la partición, sin las CT: la suite completa de Core y los filtros de UI, **Core 19 587 = (19 129 − R_C) + R_C + 458** y **UI 2 478 = (2 430 − R_UI) + R_UI + 48**; **el GREEN literal** (`apply_green_pbt4b_v45.py`): **Core 19 587 = 19 129 + 0 + 458** y **UI 2 478 = 2 430 + 0 + 48** (con las CT, 19 605 = 19 147 + 0 + 458 y 2 481 = 2 433 + 0 + 48), cero `warning CS` y `partition_pbt4b_v48.py green`: todas las copias pasan; **las dos sondas** sobre la copia del GREEN, en clases temporales fuera de todo commit: las líneas «presentar» de CT-06 (`pbt4b-ct06-presentar-probe.cs`) y **el coste de la lectura** (`pbt4b-read-cost-probe.cs`: una materialización en los cuatro casos, y lo esperado de §6.2.1); `check_skips_pbt4b_v48.py`; el coste de `IsDirty` y la duración de la suite. **Una copia que no pase** pasa a equivalente escrito a mano (D-S1), **una `Theory` que mezcle** se parte por filas (D-S5) y la emisión necesita una versión del paquete con su texto; **una prueba que falle fuera de la lista de 478** vuelve al Owner; **un coste que no cumpla lo esperado** vuelve a la sesión, que lo lleva al Owner (el de la v4.8 lo dio por cumplido el Owner, `owner-pbt4b-cost-decision.md`);
   - **PB-T4c** (los nueve pasos de §6.3.1; `pbt4ccc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4b (`6adff90d`): `apply_red_pbt4c_v51.py`, la compilación sin ningún `warning CS`, `check_skips_pbt4c_v50.py` y los filtros; **el RED exacto,** sin las CT: **Core 19 608 = 19 147 + 0 + 461** y los 18 filtros de UI, donde solo cae el equivalente de CT-PBT2B-06, por aserción; **el GREEN literal:** **Core 19 608 = 19 147 + 0 + 461** y los 18 filtros, 0 `Failed`; las sondas de las líneas de las CT y el coste de `IsDirty`; **el ensayo de las CT de texto** (PASA en las dos ramas, y su control sobre la v5.0 ve I-01) **y el censo de C13** (PASA); **K = 4, D = 0 y E = 4**, y la duración de la suite. Medido en la v5.1 (`pbt4c-precondition6.json`, `29179a12ef8623f4c276f943a725ece6f3ffd84464f16705e1feda757efbce1a`). **Una prueba que falle fuera de la lista de 4** vuelve al Owner.
   - **PB-T4d** (los nueve pasos de §6.4.1; `pbt4dcc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4c (`6c2820ba`): el RED literal y el GREEN literal, sin ningún `warning CS`; **Core:** en el RED, 19418 = 19014 + 1 + 403, y solo cae CT-PBT4D-01, por aserción; en el GREEN, 19418 = 19015 + 0 + 403; los 18 filtros de UI del contrato, 0 `Failed`; la copia principal, sola; **la cobertura ampliada de todo `src/` que se queda, PASA**; el ensayo de las CT y su control, y el censo de C13 (PASA). Medido en la v5.3 (`pbt4d-precondition6.json`, `52e03e061cd62c79597f51b8eaf144a27d68b3bf5db876406a804be2fa978989`). **Una prueba que falle fuera de la lista** vuelve al Owner.
7. **La lista que se emite es la aprobada, o una parte:** PB-T4a, 1; **PB-T4b, las 478 de `pbt4b-substitution-list-478.tsv`** (sha256 `8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b`), que el generador comprueba; **PB-T4c, las 4 de `pbt4c-substitution-list-4.tsv`** (sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de`). **PB-T4d, las 271 filas con acción de `pbt4d-retire-list-271.tsv`** (sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac`), la elegida (`owner-pbt4d-list-decisions.md`, pregunta 1, con las sustitutas y la rama muerta de sus decisiones siguientes), que el generador comprueba; se emite por su autorización condicional (pregunta 4).
8. **La custodia,** en el commit de emisión de PB-T4a:
   - este paquete (como `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`), las versiones v1 a v4.1, el registro de mediciones, la revisión y su encargo, las instrucciones de cada versión, los guiones y sus salidas, las listas (`pbt4-substitution-lists.md` y `.tsv`, `pbt4b-substitution-list.tsv`, `pbt4d-retire-list.tsv`, `pbt4-copy-map.tsv` y `pbt4-retire-compile.tsv`), el recorrido de las guardas (`pbt4a-guard-walk.tsv`, `pbt4a-guard-walk-out.txt` y `pbt4a-guard-walk-flagged.txt`) y los fragmentos de `v41/` y `v42/`;
   - los dos registros del Owner (`F4B-DRAFTS/coordinator/owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`);
   - la entrada «## PB-T4a» de `superseding-characterization.md`.
9. **Para PB-T4b y PB-T4c:** la anterior CERRADA y su remedición (**PB-T4a, CERRADA: F4B-E51, `78a7cef3`**). **En el commit de emisión de PB-T4b,** además: esta versión (v4.9), la v4.8, la v4.7, la v4.6, la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/`, `v46/`, `v47/`, `v48/` y `v49/`, las listas (`pbt4b-substitution-list-478.tsv`, `pbt4b-substitution-list-477.tsv`, `pbt4b-substitution-list-475.tsv` y `pbt4d-retire-list-138.tsv`), los mapas de la copia v2 a v5, el kit de la v4.5, los guiones de la v4.4 a la v4.9 y sus salidas, la partición y las 21 copias generadas con ella (por sha256), `pbt4b-equivalents-review-v45.txt`, `pbt4b-equivalents-review-v46.txt`, `pbt4b-equivalents-review-v47.txt`, `pbt4b-equivalents-review-v48.txt` y `pbt4b-equivalents-review-v49.txt`, el análisis de la v4.7 (`v47/analysis-outside-477.md`), la regla de la tercera corrida del coste (`v48/session-cost-rerun-rule.md`), la corrida de la precondición 6 (`pbt4bcc/compile/`: `compile-check.md`, `pbt4b-precondition6.json`, sus guiones, sus registros y el sha256 de sus TRX y binarios), las dos sondas y sus TRX con su sha256, los registros del Owner de la tercera, la quinta, la sexta y la séptima tandas (`owner-pbt4b-breaks-decision.md`, `owner-pbt4b-list-477-decision.md`, `owner-pbt4b-list-478-decision.md` y, la séptima, `owner-pbt4b-cost-decision.md`; `owner-pbt4a-cost-decision.md` ya está en el cierre de PB-T4a), la decisión de la sesión y la entrada «## PB-T4b» de `superseding-characterization.md`. **En el commit de emisión de PB-T4c,** además: esta versión (v5.1) y la v5.0, los fragmentos y los guiones de `v50/` y de `v51/`, las listas (`pbt4c-substitution-list-4.tsv` y la de 3), las piezas y los guiones de §6.3.2 y sus salidas, las sondas, la remedición (`v50/remeasure_pbt4c_v50.py` y su salida), el paseo de guardas, la corrida de la precondición 6 (`pbt4ccc/compile/`: `compile-check.md`, `pbt4c-precondition6.json`, sus guiones, sus registros y el sha256 de sus TRX y binarios, las de la v5.1 en `r5i-en/`, `r5j-rg/`, `r5k-id/` y `r5l-cc/`, y como historia las de la v5.0, la vuelta parada y la medida de D-S9 de `v50/variant-rename/`), el ensayo de las CT de texto y el censo de C13 con sus salidas, los registros del Owner de la octava y la novena tandas, `session-pbt4c-design-decisions.md` y la entrada «## PB-T4c» de `superseding-characterization.md`. **Para PB-T4d:** el cierre de PB-T4c (CERRADA: F4B-E55, `6c2820ba`), su lista vuelta a medir y las respuestas del Owner (cumplidas); **en el commit de emisión de PB-T4d,** además: esta versión (v5.4), los fragmentos y los guiones de `v52/` (con `v52/sub/`, el alto del relevo), de `v53/` y de `v54/`, la comprobación acotada de la v5.2 (`independent-review-pbt4d-check.md`), la lista (`pbt4d-retire-list-271.tsv`; la de la v5.2, `pbt4d-retire-list-229.tsv`, como historia), las dos piezas y sus módulos y aplicadores, la remedición, las medidas de la necesidad y de la cobertura, la corrida de la precondición 6 (`pbt4dcc/compile/`, en `F4B-DRAFTS/pbt4/cc4d/`: `compile-check.md`, `pbt4d-precondition6.json`, sus guiones, sus registros y el sha256 de sus TRX y de sus coberturas, y como historia las copias anteriores), los cinco registros del Owner de las tandas 10.ª a 14.ª, `session-pbt4d-design-decisions.md` (D-S11 a D-S16) y la entrada «## PB-T4d» de `superseding-characterization.md`.


## Anexo V49-A. El texto literal de lo escrito a mano de la v4.8 y la v4.9, y de las cuatro `Theory` partidas por filas

Copiado por `make_v49.py` de la revisión del guion, `pbt4b-equivalents-review-v49.txt` (sha256 `182b6aed301dc18a176da947b6eefacac2f8d7f68d88ab3f0f0c8449f99311ec`, 114 entradas). **Sustituye al anexo V48-A:** es el mismo salvo el equivalente de WHHG `NoneOfThe30…`, que trae la autoprueba de la guarda (CA-T4b I-01; V49-01). Cada prueba va en su clase de la copia, en el lugar de la prueba original y con la indentación de la clase; lo que manda es el `CopySha` de su archivo.

### D-S1 en dos copias de WPP (V48-02)

**`FullPanelCopyWorkspacePilotPrepare<Red|Green>Tests.InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView`** (la clase, la de la partición):

```csharp
    [Fact]
    public void InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView()
    {
        var origin = PilotKit.Text("pb-01-base");
        var originFrontal = FullPrepareKit.Frontal();
        var cases = new Dictionary<string, string>
        {
            // The frontal view of side B in a rack that has only side A.
            ["side-b"] = FullPrepareKit.Frontal(section: 2),
            // A frontal view with the section of the fixtures (-1) is a corrupt descriptor: it is never coerced into another view.
            ["corrupt-descriptor"] = PilotKit.Text("pb-01-base"),
            ["section-out-of-range"] = FullPrepareKit.Frontal(section: 9),
        };

        foreach (var (name, text) in cases)
        {
            var members = FullPrepareKit.BaseOf(("A1", originFrontal), ("B2", text));
            var draft = FullPrepareKit.DraftOf(members, originFrontal);
            FullPrepareKit.SetScale(draft, 2.25);

            var result = FullPrepareKit.Prepare(draft, FullPrepareKit.Reread(members));

            Assert.True(result.Kind == PushBackPilotPrepareKind.Invalid, name);
            Assert.Null(result.Plan);
            // M-5 (I-64 F4-T4B): a descriptor that does not decode has no address to ask about, so it has its own text and is never reported as a
            // view that is "no longer available".
            var corrupt = name is "corrupt-descriptor" or "section-out-of-range";
            // I-64 F4B PB-T4b (D-S1; D-PBT3-09 (e)): with the full model, the frontal of side B of a rack without side B has its own text.
            Assert.StartsWith((corrupt ? PushBackPilotUpdatePlanner.ViewAddressPrefix : PushBackPilotUpdatePlanner.FrontalBOfOneWayPrefix) + "B2", result.Reason, StringComparison.Ordinal);
            Assert.EndsWith(corrupt ? PushBackPilotUpdatePlanner.ViewAddressSuffix : PushBackPilotUpdatePlanner.FrontalBOfOneWaySuffix, result.Reason, StringComparison.Ordinal);
        }

        // I-64 F4B PB-T4b (D-S1; D-PB-04, D-PBT3-09 (e) and D-PBT4-07): the full model erases the obsolete lateral, so the phantom is not
        // a view that is no longer available: the update is Ready, the phantom is erased and it is never in the plan of the redrawn views.
        var phantomMembers = FullPrepareKit.BaseOf(("A1", originFrontal), ("B2", FullPrepareKit.Lateral(FullPrepareKit.MissingPost)));
        var phantomDraft = FullPrepareKit.DraftOf(phantomMembers, originFrontal);
        FullPrepareKit.SetScale(phantomDraft, 2.25);
        var phantom = FullPrepareKit.Prepare(phantomDraft, FullPrepareKit.Reread(phantomMembers));
        Assert.Equal(PushBackPilotPrepareKind.Ready, phantom.Kind);
        Assert.Equal(new[] { "A1" }, phantom.Plan!.Members.Select(m => m.Key).ToArray());
        Assert.Equal(new[] { "B2" }, phantom.Plan!.Erasures.ToArray());

        Assert.NotEqual(origin, originFrontal);
    }
```

**`FullPanelCopyWorkspacePilotPrepare<Red|Green>Tests.ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan`**:

```csharp
    [Fact]
    public void ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan()
    {
        var origin = FullPrepareKit.Frontal();
        var withPhantom = FullPrepareKit.BaseOf(("A1", origin), ("B2", FullPrepareKit.Lateral(FullPrepareKit.MissingPost)));
        var draft = FullPrepareKit.DraftOf(withPhantom, origin);
        FullPrepareKit.SetScale(draft, 2.25);

        var result = FullPrepareKit.Prepare(draft, FullPrepareKit.Reread(withPhantom));

        // I-64 F4B PB-T4b (D-S1; D-PB-04, D-PBT3-09 (e) and D-PBT4-07): the full model erases the obsolete lateral, so the phantom is not
        // a view that is no longer available: the update is Ready, the phantom is erased and it is never in the plan of the redrawn views.
        Assert.Equal(PushBackPilotPrepareKind.Ready, result.Kind);
        Assert.Equal(new[] { "A1" }, result.Plan!.Members.Select(m => m.Key).ToArray());
        Assert.Equal(new[] { "B2" }, result.Plan!.Erasures.ToArray());

        // The same rack without the phantom is Ready, and the plan redraws only the views that exist.
        var without = FullPrepareKit.BaseOf(("A1", origin), ("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost())));
        var clean = FullPrepareKit.DraftOf(without, origin);
        FullPrepareKit.SetScale(clean, 2.25);
        var ready = FullPrepareKit.Prepare(clean, FullPrepareKit.Reread(without));
        Assert.Equal(PushBackPilotPrepareKind.Ready, ready.Kind);
        Assert.Equal(new[] { "A1", "B2" }, ready.Plan!.Members.Select(m => m.Key).ToArray());
        Assert.Empty(ready.Plan!.Erasures);
    }
```

### La guarda de la sexta tanda (V48-03; C-5), con su autoprueba (CA-T4b I-01; V49-01)

**`FullPanelCopyWorkspaceHostHardeningGuard<Red|Green>Tests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept`** (la prueba del original y, tras su `Assert.Empty`, la autoprueba WHHG:504-525, con sus 13 `Assert.NotEmpty`):

```csharp
    [Fact]
    public void NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept()
    {
        Assert.Equal(30, Table.Length);
        Assert.Equal(30, Table.Select(t => t.English).Distinct().Count());
        Assert.Equal(30, Table.Select(t => t.Spanish).Distinct().Count());

        Assert.Empty(LiteralViolations(Src.Code(HostPath), Src.Code(SupportPath)));

        // I-64 F4B PB-T4b (CA-T4b I-01; owner-pbt4b-list-478-decision.md): the self-test of the guard (WHHG:501-526), over the table of this
        // copy. In the GREEN the original self-test passes vacuously: its table (2, 5 and 1) no longer matches the host (0, 6 and 0), so
        // LiteralViolations already answers three violations without any mutation. Here the host is clean for this table (the Assert.Empty
        // above), so each of the 13 mutations has to make the guard trip.
        var host = Src.Code(HostPath);
        var support = Src.Code(SupportPath);

        // An English literal that comes back, in the host and in the support.
        Assert.NotEmpty(LiteralViolations(host + " \"The reread failed.\"", support));
        Assert.NotEmpty(LiteralViolations(host, support + " \"A notice of the panel is being delivered.\""));
        // A Spanish text that goes away (here, replaced by another), and one that is repeated.
        Assert.NotEmpty(LiteralViolations(host.Replace("\"Falló la relectura.\"", "\"Otra cosa.\"", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host + " \"Falló la relectura.\"", support));
        // A lost identifier.
        Assert.NotEmpty(LiteralViolations(host.Replace("NoDraftReason", "NoBorradorReason", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host, support.Replace("InsideNoticeReason", "DentroDelAvisoReason", StringComparison.Ordinal)));
        // The two new texts: an English original that comes back, and a Spanish text that goes away.
        Assert.NotEmpty(LiteralViolations(host + " \"The draft cannot be checked in its current status.\"", support));
        Assert.NotEmpty(LiteralViolations(host + " \"Only a draft in conflict or with edits marked as possibly stale is kept.\"", support));
        Assert.NotEmpty(LiteralViolations(host.Replace("El borrador no admite comprobación en su estado actual.", "Otra cosa.", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host.Replace("Solo se conserva un borrador en conflicto", "Se conserva un borrador en conflicto", StringComparison.Ordinal), support));
        // A reused text that one operation of the reconciliation stops using changes its exact count.
        Assert.NotEmpty(LiteralViolations(host.Replace("\"El testigo de la vista previa no tiene clave.\"", "\"Otro.\"", StringComparison.Ordinal), support));
        // The text of the record or of the differences changes.
        Assert.NotEmpty(LiteralViolations(host.Replace("(variable registry)", "(registro de variables)", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host.Replace("Uncertain racks", "Racks inciertos", StringComparison.Ordinal), support));
    }
```

**`WorkspaceHostHardeningGuardFull`, el ayudante `Table`** (la tabla, con tres cuentas nuevas):

```csharp
    // ------------------------------------------------------------------ H-07: the 30 literals of the port of the pilot are in Spanish

    // The table of the package (H-07, and the two rows A1-01 and A1-02 of F4-T4A1): the literal in English, its Spanish text and how many times it
    // stands in the host and the support. Since F4-T4A1 the three operations of the reconciliation reuse the texts of the others, and the count of each
    // one is the EXACT number of its occurrences now (the value before the task is in the comment of each row that changed).
    private static readonly (string English, string Spanish, int Times)[] Table =
    {
        ("A read of the pilot is in progress.", "Hay una lectura del piloto en curso.", 1),
        ("The request does not belong to the active document.", "La petición no es del documento activo.", 6),
        ("There is no draft for the rack.", "No hay borrador para el rack.", 3),
        ("The preview witness has no key.", "El testigo de la vista previa no tiene clave.", 6), // before 3
        ("The drawing is not ready to be read.", "El dibujo no está listo para leerse.", 6), // before 3
        ("The read of the edit intention failed.", "Falló la lectura de la intención de edición.", 1),
        ("The read of the edit intention carries no result.", "La lectura de la intención de edición no trajo resultado.", 1),
        ("The session was retired.", "La sesión se retiró.", 1),
        ("A blank rack has no draft.", "Un rack sin identificador no tiene borrador.", 2), // before 1
        ("The draft is not unknown after a commit that threw.", "El borrador no quedó desconocido tras una aplicación fallida.", 2),
        ("The reread failed.", "Falló la relectura.", 2), // before 1
        ("The reread carries no result.", "La relectura no trajo resultado.", 0), // before 2 (I-64 F4B PB-T4b, owner-pbt4b-list-478-decision.md)
        ("The draft does not need to be reloaded.", "El borrador no necesita recargarse.", 1),
        ("The read for the reload failed.", "Falló la lectura para la recarga.", 3), // before 1
        ("The read for the reload carries no result.", "La lectura para la recarga no trajo resultado.", 3), // before 1
        ("The draft is being applied or is no longer registered.", "El borrador se está aplicando o ya no está registrado.", 6), // before 5 (I-64 F4B PB-T4b, owner-pbt4b-list-478-decision.md)
        ("The draft was not reloaded.", "El borrador no se recargó.", 3), // before 1
        ("The state of the field could not be read.", "No se pudo leer el estado del campo.", 1),
        ("The text is required.", "El texto es obligatorio.", 1),
        ("The field cannot be edited now.", "El campo no se puede editar ahora.", 4),
        ("The boundary is not known.", "La frontera no se conoce.", 1),
        ("The rack has no annotation scale.", "El rack no tiene escala de anotación.", 0), // before 1 (I-64 F4B PB-T4b, owner-pbt4b-list-478-decision.md)
        ("No reader of the edit intention is assigned.", "No hay un lector de la intención de edición asignado.", 1),
        ("There is no active document.", "No hay documento activo.", 1),
        ("A RackCad window is open.", "Hay una ventana de RackCad abierta.", 1),
        ("A command is in progress.", "Hay un comando en curso.", 1),
        ("The value is not a finite number greater than zero.", "El valor no es un número finito mayor que cero.", 1),
        ("A notice of the panel is being delivered.", "Se está entregando un aviso del panel.", 1),
        // The two rows that F4-T4A1 adds (table T-A1 of the package), each one declared as a private constant of the host.
        ("The draft cannot be checked in its current status.", "El borrador no admite comprobación en su estado actual.", 1),
        ("Only a draft in conflict or with edits marked as possibly stale is kept.", "Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.", 1),
    };
```

### Las cuatro `Theory` partidas por filas (D-S5) y los dos miembros nuevos (con los nombres de D-S6)

**`FullPanelCopyWorkspaceLifecycleHostRedTests.EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively`** (la mitad Red):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that fail there, and in the other class of the partition, the rest.
    [Theory]
    [InlineData("a-check-that-finds-a-conflict")]
    [InlineData("a-check-that-recovers")]
    [InlineData("a-reload")]
    public void EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively(string operation)
    {
        Action<Env>? before = operation switch
        {
            "a-check-that-finds-a-conflict" or "a-check-that-recovers" => env => env.UnknownPilot(),
            "a-reload" => env => env.StalePilot(),
            _ => null,
        };

        Action<Env> between = operation switch
        {
            "a-veto" => env =>
            {
                var dirty = env.Seed("R1");
                dirty.SetCurrentDraftState(dirty.CurrentDraftState with { Name = "Edited" });
                Assert.Equal(CloseDecision.Veto, env.Advisor.AdviseClose(env.Session));
                env.Core.OpenDrafts.DiscardByUser(env.Session, "R1");
            },
            "a-failure-of-the-evaluation" => env =>
            {
                var comparer = new DraftThrowingComparer();
                env.Seed("R1", comparer);
                comparer.Throw = true;
                Assert.Equal(CloseDecision.Veto, env.Advisor.AdviseClose(env.Session));
                env.Core.OpenDrafts.DiscardByUser(env.Session, "R1");
            },
            "an-admitted-intention" => env => Assert.Equal(EditIntentStatus.Admitted, env.Core.TryBeginPilotEdit(env.Request()).Status),
            "a-check-that-finds-a-conflict" => env =>
            {
                env.Reader.Answer = FullEditIntentKit.MemberBecameBlocking;
                Assert.Equal(EditCheckStatus.Conflict, env.Core.TryCheckPilotEdit(env.Request()).Status);
            },
            "a-check-that-recovers" => env => Assert.Equal(EditCheckStatus.Recovered, env.Core.TryCheckPilotEdit(env.Request()).Status),
            "a-reload" => env => Assert.Equal(EditIntentStatus.Reloaded, env.Core.TryReloadPilotEdit(env.Request()).Status),
            "a-discard-of-the-host" => env => Assert.Equal(EditDiscardStatus.Discarded, env.Core.DiscardPilotEdits(env.Session).Status),
            "a-drain" => env => env.Core.OnDrained(Outcome(env.Session)),
            "a-drain-with-the-stale-hint" => env => env.Core.OnDrained(Outcome(env.Session, DrainOperation.MarkDraftsPossiblyStale)),
            _ => env => env.Core.NotePilotEdit(env.Session),
        };

        Assert.Equal("conservative", PathAfter(before, between));
    }
```

**`FullPanelCopyWorkspaceLifecycleHostGreenTests.EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively`** (la mitad Green):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that pass there, and in the other class of the partition, the rest.
    [Theory]
    [InlineData("a-veto")]
    [InlineData("a-failure-of-the-evaluation")]
    [InlineData("an-admitted-intention")]
    [InlineData("a-discard-of-the-host")]
    [InlineData("a-drain")]
    [InlineData("a-drain-with-the-stale-hint")]
    [InlineData("a-note-of-an-edit")]
    public void EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively(string operation)
    {
        Action<Env>? before = operation switch
        {
            "a-check-that-finds-a-conflict" or "a-check-that-recovers" => env => env.UnknownPilot(),
            "a-reload" => env => env.StalePilot(),
            _ => null,
        };

        Action<Env> between = operation switch
        {
            "a-veto" => env =>
            {
                var dirty = env.Seed("R1");
                dirty.SetCurrentDraftState(dirty.CurrentDraftState with { Name = "Edited" });
                Assert.Equal(CloseDecision.Veto, env.Advisor.AdviseClose(env.Session));
                env.Core.OpenDrafts.DiscardByUser(env.Session, "R1");
            },
            "a-failure-of-the-evaluation" => env =>
            {
                var comparer = new DraftThrowingComparer();
                env.Seed("R1", comparer);
                comparer.Throw = true;
                Assert.Equal(CloseDecision.Veto, env.Advisor.AdviseClose(env.Session));
                env.Core.OpenDrafts.DiscardByUser(env.Session, "R1");
            },
            "an-admitted-intention" => env => Assert.Equal(EditIntentStatus.Admitted, env.Core.TryBeginPilotEdit(env.Request()).Status),
            "a-check-that-finds-a-conflict" => env =>
            {
                env.Reader.Answer = FullEditIntentKit.MemberBecameBlocking;
                Assert.Equal(EditCheckStatus.Conflict, env.Core.TryCheckPilotEdit(env.Request()).Status);
            },
            "a-check-that-recovers" => env => Assert.Equal(EditCheckStatus.Recovered, env.Core.TryCheckPilotEdit(env.Request()).Status),
            "a-reload" => env => Assert.Equal(EditIntentStatus.Reloaded, env.Core.TryReloadPilotEdit(env.Request()).Status),
            "a-discard-of-the-host" => env => Assert.Equal(EditDiscardStatus.Discarded, env.Core.DiscardPilotEdits(env.Session).Status),
            "a-drain" => env => env.Core.OnDrained(Outcome(env.Session)),
            "a-drain-with-the-stale-hint" => env => env.Core.OnDrained(Outcome(env.Session, DrainOperation.MarkDraftsPossiblyStale)),
            _ => env => env.Core.NotePilotEdit(env.Session),
        };

        Assert.Equal("conservative", PathAfter(before, between));
    }
```

**`FullPanelCopyWorkspacePilotEditGuardRedTests.TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance`** (la mitad Red):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that fail there, and in the other class of the partition, the rest.
    [Theory]
    [InlineData("private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();", "private readonly PushBackFullFieldAdapter _adapter = new();")]
    public void TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance(string from, string to)
    {
        var host = Host();
        Assert.Contains(from, host);

        Assert.NotEmpty(HostViolations(host.Replace(from, to, StringComparison.Ordinal)));
    }
```

**`FullPanelCopyWorkspacePilotEditGuardGreenTests.TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance`** (la mitad Green):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that pass there, and in the other class of the partition, the rest.
    [Theory]
    [InlineData("public sealed class WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort", "public sealed class WorkspaceHostCore : IWorkspacePilotEditPort, IWorkspaceLifecycleAdvisor")]
    [InlineData("PilotFieldView IWorkspacePilotEditPort.GetField(", "public PilotFieldView GetField(")]
    [InlineData("PilotFieldResult IWorkspacePilotEditPort.CommitField(", "public PilotFieldResult CommitField(")]
    public void TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance(string from, string to)
    {
        var host = Host();
        Assert.Contains(from, host);

        Assert.NotEmpty(HostViolations(host.Replace(from, to, StringComparison.Ordinal)));
    }
```

**`FullPanelCopyWorkspacePilotPrepareRedTests.ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing`** (la mitad Red):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that fail there, and in the other class of the partition, the rest.
    [Theory]
    [MemberData(nameof(StatusNamesRed))]
    public void ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing(string name)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Statuses().Single(s => s.Name == name).Put(rig, draft);
        var applies = name is "Clean" or "Dirty" or "DirtyStale" or "Conflict";

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        if (applies)
        {
            Assert.Equal(EditConfirmStatus.Confirmed, outcome.Status);
            Assert.Single(rig.Reader.Calls);
        }
        else
        {
            Assert.Equal(EditConfirmStatus.NotApplicable, outcome.Status);
            Assert.Equal("El borrador no admite comprobación en su estado actual.", outcome.Reason);
            Assert.Empty(rig.Reader.Calls);
        }
    }
```

**`FullPanelCopyWorkspacePilotPrepareGreenTests.ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing`** (la mitad Green):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that pass there, and in the other class of the partition, the rest.
    [Theory]
    [MemberData(nameof(StatusNamesGreen))]
    public void ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing(string name)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Statuses().Single(s => s.Name == name).Put(rig, draft);
        var applies = name is "Clean" or "Dirty" or "DirtyStale" or "Conflict";

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        if (applies)
        {
            Assert.Equal(EditConfirmStatus.Confirmed, outcome.Status);
            Assert.Single(rig.Reader.Calls);
        }
        else
        {
            Assert.Equal(EditConfirmStatus.NotApplicable, outcome.Status);
            Assert.Equal("El borrador no admite comprobación en su estado actual.", outcome.Reason);
            Assert.Empty(rig.Reader.Calls);
        }
    }
```

**`FullPanelCopyWorkspacePilotPrepareRedTests.KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading`** (la mitad Red):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that fail there, and in the other class of the partition, the rest.
    [Theory]
    [MemberData(nameof(StatusNamesRed))]
    public void KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading(string name)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Statuses().Single(s => s.Name == name).Put(rig, draft);
        rig.Reader.Answer = Changed;

        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());

        switch (name)
        {
            case "DirtyStale":
            case "Conflict":
                // I-64 F4B PB-T4b (D-S1, D-PBT4-08): the first call presents; the second, with the witness of that read, keeps.
                Assert.Equal(EditIntentStatus.Presented, outcome.Status);
                Assert.Single(rig.Reader.Calls);
                rig.Reader.Answer = rack => FullHandKit.Seen(Changed(rack), outcome.KeepWitness!);
                Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request(witness: outcome.KeepWitness)).Status);
                Assert.Equal(2, rig.Reader.Calls.Count);
                break;
            case "Applying":
                Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
                Assert.Equal("El borrador se está aplicando o ya no está registrado.", outcome.Reason);
                Assert.Empty(rig.Reader.Calls);
                break;
            default:
                Assert.Equal(EditIntentStatus.NotNeeded, outcome.Status);
                Assert.Equal("Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.", outcome.Reason);
                Assert.Empty(rig.Reader.Calls);
                break;
        }
    }
```

**`FullPanelCopyWorkspacePilotPrepareGreenTests.KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading`** (la mitad Green):

```csharp
    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that pass there, and in the other class of the partition, the rest.
    [Theory]
    [MemberData(nameof(StatusNamesGreen))]
    public void KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading(string name)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Statuses().Single(s => s.Name == name).Put(rig, draft);
        rig.Reader.Answer = Changed;

        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());

        switch (name)
        {
            case "DirtyStale":
            case "Conflict":
                // I-64 F4B PB-T4b (D-S1, D-PBT4-08): the first call presents; the second, with the witness of that read, keeps.
                Assert.Equal(EditIntentStatus.Presented, outcome.Status);
                Assert.Single(rig.Reader.Calls);
                rig.Reader.Answer = rack => FullHandKit.Seen(Changed(rack), outcome.KeepWitness!);
                Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request(witness: outcome.KeepWitness)).Status);
                Assert.Equal(2, rig.Reader.Calls.Count);
                break;
            case "Applying":
                Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
                Assert.Equal("El borrador se está aplicando o ya no está registrado.", outcome.Reason);
                Assert.Empty(rig.Reader.Calls);
                break;
            default:
                Assert.Equal(EditIntentStatus.NotNeeded, outcome.Status);
                Assert.Equal("Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.", outcome.Reason);
                Assert.Empty(rig.Reader.Calls);
                break;
        }
    }
```

**`WorkspacePilotPrepareFull`, el miembro nuevo `StatusNamesRed`** (solo en la copia; en la clase de la partición cuyas pruebas lo alcanzan):

```csharp
    // I-64 F4B PB-T4b (D-S5): the rows of StatusNames that fail in the RED (every status but Clean, whose draft the rig edits), for the half
    // of the Theories split by rows that goes to the Red class. With the member of the Green class they are, row by row, the rows of
    // StatusNames.
    public static TheoryData<string> StatusNamesRed()
    {
        var data = new TheoryData<string>();
        foreach (string name in StatusNames())
        {
            if (!string.Equals(name, "Clean", StringComparison.Ordinal)) data.Add(name);
        }

        return data;
    }
```

**`WorkspacePilotPrepareFull`, el miembro nuevo `StatusNamesGreen`** (solo en la copia; en la clase de la partición cuyas pruebas lo alcanzan):

```csharp
    // I-64 F4B PB-T4b (D-S5): the row of StatusNames that passes in the RED (Clean), for the half of the Theories split by rows that goes to
    // the Green class.
    public static TheoryData<string> StatusNamesGreen()
    {
        var data = new TheoryData<string>();
        foreach (string name in StatusNames())
        {
            if (string.Equals(name, "Clean", StringComparison.Ordinal)) data.Add(name);
        }

        return data;
    }
```

## Anexo V51-A. El texto literal de PB-T4c: el GREEN, el esqueleto y los equivalentes (v5.1)

Copiado por `make_v51.py`; sustituye al anexo V50-A de la v5.0. **Lo que manda es el sha256 de cada pieza** (§6.3.2); este anexo es para leerlo sin generar nada. Frente a la v5.0 (V51-01 y V51-04): sin `#nullable enable` en los dos archivos de `Draft/` (las dos piezas), sin el `?` de `ProjectOf` y con el comentario nuevo de `PendingWork()` (el GREEN), y la cabecera de cada pieza.

### La pieza del GREEN, `pbt4c-green-v51.txt` (sha256 `081ef77e66a390cf3637f15851b5fe05a5cdf07f6e0065928fc5c841b250dc16`)

```text
@@@@ PB-T4c (paquete v5.1): el texto literal del GREEN de src/. Cada bloque EDIT sustituye su OLD (exactamente una vez) por su NEW en el
@@@@ archivo de 6adff90d; cada bloque NEWFILE es un archivo nuevo entero. Saltos de linea \n; green_pbt4c_v51.py lo aplica y lo comprueba.
@@@@ EDIT src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs
@@@@ OLD
        EditUpdateProgress TakeUpdateProgress(WorkspaceRequest origin, string rackId);
    }
@@@@ NEW
        EditUpdateProgress TakeUpdateProgress(WorkspaceRequest origin, string rackId);

        /// <summary>
        /// I-64 F4B PB-T4c (D-PBT4-13): "Editar todo" for one rack. Without work for the rack it begins it through the edit intention, so its
        /// one read is the one of the intention; then it opens the full editor over the work and keeps the mark of the reads until the editor
        /// closes, so while it is open every operation of the panel answers Busy or Waiting (K = 1). The editor only hands its edits to the
        /// work: nothing here writes the drawing. A port that does not offer it answers NotAvailable.
        /// </summary>
        EditAllOutcome TryEditAll(EditIntentRequest request) => EditAllOutcome.NotAvailable(null);
    }
@@@@ END
@@@@ EDIT src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
@@@@ OLD
        private static EditIntentOutcome Without(EditIntentStatus status, string? reason) =>
            new(status, EditWitnessStatus.NotEvaluated, reason, null, null, null);
    }
@@@@ NEW
        private static EditIntentOutcome Without(EditIntentStatus status, string? reason) =>
            new(status, EditWitnessStatus.NotEvaluated, reason, null, null, null);
    }

    /// <summary>How "Editar todo" ended (I-64 F4B PB-T4c, D-PBT4-13). The default value is NotAvailable.</summary>
    public enum EditAllStatus
    {
        /// <summary>The port does not offer the full editor, no reader is assigned or there is no full editor here: nothing was read or opened.</summary>
        NotAvailable,

        /// <summary>It waits: the gate, a read in progress, a notice of the panel, an update in flight or a work that is being applied.</summary>
        Waiting,

        /// <summary>The request is not of the active document: nothing was read or opened.</summary>
        Rejected,

        /// <summary>There was no work and the edit intention did not begin it: the outcome carries the one of the intention.</summary>
        NotBegun,

        /// <summary>The work is in a state that the editor does not open (unknown, in error or undecidable), or it still has pending text.</summary>
        Refused,

        /// <summary>The full editor was opened over the work and is closed again; Applied counts the edits that it handed to the work.</summary>
        Closed,

        /// <summary>A failure of the host during the operation: contained, and the session is not degraded.</summary>
        Failed,
    }

    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-13) — the answer of the host to "Editar todo". A value: it names no type of the work and no type of the
    /// systems, and the outcome of the intention that it may carry is the value of the edit intention.
    /// </summary>
    public readonly struct EditAllOutcome
    {
        private EditAllOutcome(EditAllStatus status, string? reason, EditIntentOutcome begin, int applied)
        {
            Status = status;
            Reason = reason;
            Begin = begin;
            Applied = applied;
        }

        public EditAllStatus Status { get; }

        /// <summary>Why the editor did not open; null for Closed and NotBegun.</summary>
        public string? Reason { get; }

        /// <summary>The outcome of the edit intention that did not begin the work (NotBegun); the default value otherwise.</summary>
        public EditIntentOutcome Begin { get; }

        /// <summary>How many times the editor handed its edits to the work (Closed); 0 otherwise.</summary>
        public int Applied { get; }

        public static EditAllOutcome NotAvailable(string? reason) => new(EditAllStatus.NotAvailable, reason, default, 0);

        public static EditAllOutcome Waiting(string reason) => new(EditAllStatus.Waiting, reason, default, 0);

        public static EditAllOutcome Rejected(string reason) => new(EditAllStatus.Rejected, reason, default, 0);

        public static EditAllOutcome NotBegun(EditIntentOutcome begin) => new(EditAllStatus.NotBegun, null, begin, 0);

        public static EditAllOutcome Refused(string reason) => new(EditAllStatus.Refused, reason, default, 0);

        public static EditAllOutcome Closed(int applied) => new(EditAllStatus.Closed, null, default, applied);

        public static EditAllOutcome Failed(string reason) => new(EditAllStatus.Failed, reason, default, 0);
    }
@@@@ END
@@@@ EDIT src/RackCad.Application/Workspace/WorkspaceHostCore.cs
@@@@ OLD
        private const string NotKeepableReason ="Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.";
@@@@ NEW
        private const string NotKeepableReason ="Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.";

        // I-64 F4B PB-T4c (D-PBT4-13): the reasons of "Editar todo".
        private const string EditAllNoEditorReason = "No editable aquí: el panel no tiene el editor completo.";
        private const string EditAllPendingTextReason = "Hay texto sin confirmar en el campo.";
        private const string EditAllBusyDraftReason = "El borrador se está aplicando o se retiró: el editor no se abre.";
        private const string EditAllStateReason = "El borrador no admite el editor completo en su estado actual.";
@@@@ END
@@@@ EDIT src/RackCad.Application/Workspace/WorkspaceHostCore.cs
@@@@ OLD
        private IPushBackPilotUpdateHost? _updateHost;
@@@@ NEW
        private IPushBackPilotUpdateHost? _updateHost;

        // I-64 F4B PB-T4c (D-PBT4-15): the host of the full editor (assigned once, it keeps nothing): it enters the modal of the panel and
        // shows the editor over the launch.
        private IWorkspaceFullEditHost? _fullEditHost;
@@@@ END
@@@@ EDIT src/RackCad.Application/Workspace/WorkspaceHostCore.cs
@@@@ OLD
        public void SetPilotUpdateHost(IPushBackPilotUpdateHost host) =>
            _updateHost = host ?? throw new ArgumentNullException(nameof(host));
@@@@ NEW
        public void SetPilotUpdateHost(IPushBackPilotUpdateHost host) =>
            _updateHost = host ?? throw new ArgumentNullException(nameof(host));

        /// <summary>
        /// Assigns the host of the full editor of "Editar todo" (I-64 F4B PB-T4c, D-PBT4-15), by assignment like the executor of the update and
        /// never by constructor. It has no state. Without it "Editar todo" answers NotAvailable.
        /// </summary>
        public void SetFullEditHost(IWorkspaceFullEditHost host) =>
            _fullEditHost = host ?? throw new ArgumentNullException(nameof(host));
@@@@ END
@@@@ EDIT src/RackCad.Application/Workspace/WorkspaceHostCore.cs
@@@@ OLD
        EditIntentOutcome IWorkspacePilotEditPort.TryDiscardAndReload(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditIntentOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryDiscardAndReloadPilotEdit(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }
@@@@ NEW
        EditIntentOutcome IWorkspacePilotEditPort.TryDiscardAndReload(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditIntentOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryDiscardAndReloadPilotEdit(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        // I-64 F4B PB-T4c (D-PBT4-13, D-PBT4-14): "Editar todo". The mark of the reads wraps the whole operation, the begin of the work and the
        // editor included, so while the full editor is open every operation of the panel answers Busy or Waiting (K = 1).
        EditAllOutcome IWorkspacePilotEditPort.TryEditAll(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditAllOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return EditAllOfPilot(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        // The rules of the keep of the pilot (I-09): the gate first, the update in flight after it, the exceptions contained with Report, and
        // NotePilotEdit for every edit that the editor hands to the work. Nothing here writes the drawing.
        private EditAllOutcome EditAllOfPilot(EditIntentRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditAllOutcome.NotAvailable(reason);
                if (gate == EditGate.Waiting) return EditAllOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditAllOutcome.Rejected(reason);

                if (UpdateInFlightForRequest(request)) return EditAllOutcome.Waiting(PushBackPilotHostTexts.UpdateInFlightReason);
                if (_fullEditHost == null) return EditAllOutcome.NotAvailable(EditAllNoEditorReason);

                // Without work for the rack the edit intention begins it, with its one read, its admission and its diagnostic.
                var draft = FindPilotDraft(request.Session, request.RackId);
                if (draft == null)
                {
                    var begun = TryBeginPilotEdit(request);
                    if (begun.Status != EditIntentStatus.Admitted) return EditAllOutcome.NotBegun(begun);
                    draft = FindPilotDraft(request.Session, request.RackId);
                    if (draft == null) return EditAllOutcome.Failed(EditAllBusyDraftReason);
                }

                var view = ViewOfPilotDraft(draft);
                if (view.State == PilotFieldState.Applying || !draft.IsRegistered) return EditAllOutcome.Waiting(EditAllBusyDraftReason);
                if (!view.CanEdit) return EditAllOutcome.Refused(EditAllStateReason);
                if (draft.HasPendingText) return EditAllOutcome.Refused(EditAllPendingTextReason);

                // The port keeps the origin of the work, and every edit that it hands to the work ends the "known clean" mark of the session.
                var applied = 0;
                var port = new PushBackFullDraftStatePort(draft, () =>
                {
                    applied++;
                    NotePilotEdit(request.Session);
                });
                _fullEditHost.ShowModal(PushBackFullDraftLaunch.For(draft, port));
                return EditAllOutcome.Closed(applied);
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryEditAll", ex);
                return EditAllOutcome.Failed(ex.Message);
            }
        }
@@@@ END
@@@@ EDIT src/RackCad.Application/Workspace/WorkspacePilotSupport.cs
@@@@ OLD
        public EditUpdateStart RequestUpdate(EditIntentRequest request) =>
            _depth.IsInside ? EditUpdateStart.Waiting(InsideNoticeReason) : _inner.RequestUpdate(request);
@@@@ NEW
        public EditUpdateStart RequestUpdate(EditIntentRequest request) =>
            _depth.IsInside ? EditUpdateStart.Waiting(InsideNoticeReason) : _inner.RequestUpdate(request);

        // I-64 F4B PB-T4c (D-PBT4-13): "Editar todo" may begin the work, so it waits inside a notice like the reads.
        public EditAllOutcome TryEditAll(EditIntentRequest request) =>
            _depth.IsInside ? EditAllOutcome.Waiting(InsideNoticeReason) : _inner.TryEditAll(request);
@@@@ END
@@@@ EDIT src/RackCad.Plugin/Workspace/WorkspaceController.cs
@@@@ OLD
                    _host.SetPilotUpdateHost(new RackPilotUpdateCommandContext());
@@@@ NEW
                    _host.SetPilotUpdateHost(new RackPilotUpdateCommandContext());

                    // The host of the full editor of "Editar todo" (I-64 F4B PB-T4c, D-PBT4-15) is wired the same way, after the executor: the
                    // modal of the panel, nested here, over the opener of the window, in the views of the Plugin.
                    _host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));
@@@@ END
@@@@ EDIT src/RackCad.Plugin/Workspace/WorkspaceController.cs
@@@@ OLD
            public EditUpdateProgress TakeUpdateProgress(WorkspaceRequest origin, string rackId) => _gate.TakeUpdateProgress(origin, rackId);
        }
@@@@ NEW
            public EditUpdateProgress TakeUpdateProgress(WorkspaceRequest origin, string rackId) => _gate.TakeUpdateProgress(origin, rackId);

            public EditAllOutcome TryEditAll(EditIntentRequest request) => _gate.TryEditAll(request);
        }

        // I-64 F4B PB-T4c (D-PBT4-15): the modal of the panel for the full editor. It names neither the interface nor the system: it enters the
        // modal of the panel and hands the launch to the opener, which shows the window without an owner and returns when it closes.
        private sealed class FullEditorModal : IWorkspaceFullEditHost
        {
            private readonly WorkspaceController _controller;
            private readonly Action<object> _open;

            public FullEditorModal(WorkspaceController controller, Action<object> open)
            {
                _controller = controller;
                _open = open;
            }

            public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class
            {
                using (_controller.EnterPanelModal()) _open(launch);
            }
        }
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
@@@@ OLD
        private const string ListSeparator = "; ";
@@@@ NEW
        private const string ListSeparator = "; ";

        // I-64 F4B PB-T4c (D-PBT4-12): the notices of "Editar todo".
        private const string EditAllNotOpenedPrefix = "No se abrió el editor: ";
        private const string EditAllAppliedText = "Editor cerrado: sus cambios están en el borrador.";
        private const string EditAllNothingAppliedText = "Editor cerrado sin cambios en el borrador.";
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
@@@@ OLD
        public bool ShowEditButton => _showEdit;
@@@@ NEW
        public bool ShowEditButton => _showEdit;

        /// <summary>
        /// I-64 F4B PB-T4c (D-PBT4-12): the "Editar todo" button. It has the conditions of the Edit button but the read-only one: a pinned
        /// surface, no failure, no diagnostic that stays and no choice waiting, and no work yet or a work that the editor opens.
        /// </summary>
        public bool ShowEditAllButton =>
            _failure == null && _session.HasValue && !_isPreview && _persistDiagnostic == null && !HasChoice && (!_view.Exists || _view.CanEdit);
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
@@@@ OLD
        /// <summary>Enter or leaving the box: commits the pending text, only with pending text and an editable box.</summary>
@@@@ NEW
        /// <summary>
        /// I-64 F4B PB-T4c (D-PBT4-12): "Editar todo". The pending text is committed first, the same two-phase step as a blocking boundary: an
        /// invalid text does not open the editor and shows its message, and a field that is busy or refused does not open it either. Then the
        /// port is asked to open the full editor over the work of the rack, with no reference and no witness: the port begins the work if there
        /// is none and holds the panel while the editor is open. When it closes, the field and the strip are shown again from the port.
        /// </summary>
        public void EditAll()
        {
            if (_port == null || !_session.HasValue || _rack == null) return;

            var session = _session.Value;
            var rack = _rack;
            try
            {
                ClearNotices();
                var view = _port.GetField(new WorkspaceRequest(session), rack);
                if (view.PendingText != null)
                {
                    var committed = _port.CommitField(new WorkspaceRequest(session), rack);
                    if (committed.Status == PilotFieldStatus.Invalid)
                    {
                        Apply(committed.View);
                        _messageText = committed.Message ?? committed.View.PendingMessage ?? string.Empty;
                        _noticeText = EditAllNotOpenedPrefix + SentenceOf(_messageText);
                        RaiseAll();
                        return;
                    }

                    if (committed.Status != PilotFieldStatus.Applied && committed.Status != PilotFieldStatus.NotEditable)
                    {
                        Render();
                        _noticeText = RefusalOf(committed);
                        RaiseAll();
                        return;
                    }
                }

                var outcome = _port.TryEditAll(new EditIntentRequest(session, rack, null, null));
                if (!IsMine(session, rack))
                {
                    // The editor closed over another surface: its result is not shown here.
                    Render();
                    _noticeText = SurfaceChangedText;
                    RaiseAll();
                    return;
                }

                if (outcome.Status == EditAllStatus.NotBegun && outcome.Begin.Status == EditIntentStatus.NotAdmitted)
                {
                    _diagnostic = outcome.Begin.Diagnostic ?? string.Empty;
                    _hint = outcome.Begin.ClassicEditorHint ?? string.Empty;
                }
                else
                {
                    _noticeText = EditAllNoticeOf(outcome);
                }

                Render();
                _changed?.Invoke();
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
            }
        }

        /// <summary>Enter or leaving the box: commits the pending text, only with pending text and an editable box.</summary>
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
@@@@ OLD
            OnPropertyChanged(nameof(ShowEditButton));
@@@@ NEW
            OnPropertyChanged(nameof(ShowEditButton));
            OnPropertyChanged(nameof(ShowEditAllButton));
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
@@@@ OLD
        // The prefix of a check that did not reread the drawing: the ones of Describe, the one of a draft that does not apply, and a default.
@@@@ NEW
        // I-64 F4B PB-T4c (D-PBT4-12): the notice of "Editar todo" once the editor closed, or when it did not open.
        private static string EditAllNoticeOf(EditAllOutcome outcome)
        {
            switch (outcome.Status)
            {
                case EditAllStatus.Closed: return outcome.Applied > 0 ? EditAllAppliedText : EditAllNothingAppliedText;
                case EditAllStatus.NotAvailable: return SentenceOf(outcome.Reason);
                case EditAllStatus.Waiting: return "En espera: " + SentenceOf(outcome.Reason);
                case EditAllStatus.Rejected: return "Rechazado: " + SentenceOf(outcome.Reason);
                case EditAllStatus.NotBegun: return Describe(outcome.Begin.Status, outcome.Begin.Reason);
                default: return EditAllNotOpenedPrefix + SentenceOf(outcome.Reason);
            }
        }

        // A text as the end of a sentence: one final period, and "sin detalle." for a missing one.
        private static string SentenceOf(string text) => string.IsNullOrWhiteSpace(text) ? "sin detalle." : text.TrimEnd('.') + ".";

        // The prefix of a check that did not reread the drawing: the ones of Describe, the one of a draft that does not apply, and a default.
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePanelView.xaml
@@@@ OLD
                                    Click="OnBrowserFieldEditClick" />
@@@@ NEW
                                    Click="OnBrowserFieldEditClick" />
                            <Button x:Name="BrowserFieldEditAllButton" Content="Editar todo" Margin="0,2,6,0" Padding="10,3"
                                    Style="{StaticResource SecondaryButtonStyle}"
                                    AutomationProperties.Name="Editar el rack completo en el editor"
                                    Visibility="{Binding Browser.Field.ShowEditAllButton, Converter={StaticResource FlagToVisibility}}"
                                    Click="OnBrowserFieldEditAllClick" />
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs
@@@@ OLD
        private void OnBrowserFieldEditClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.BeginFieldEdit());
@@@@ NEW
        private void OnBrowserFieldEditClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.BeginFieldEdit());

        // I-64 F4B PB-T4c (D-PBT4-12): "Editar todo" goes straight to the gesture of the field, which commits the pending text first.
        private void OnBrowserFieldEditAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.Field.EditAll());
@@@@ END
@@@@ EDIT src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs
@@@@ OLD
                "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?");
            return modules.HasPendingWork ? modules : EditorPendingWork.When(HasUnappliedDraftChanges(), WorkspaceUnappliedQuestion);
        }
@@@@ NEW
                "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?");

            // I-64 F4B PB-T4c (D-PBT4-17; A-T2b-OWNER-01): a staged module AND unapplied changes ask one question that names both. In
            // WORKSPACE a staged module always leaves unapplied changes (D-S10), so the module question alone is the one of CLASSIC, where
            // there are never unapplied changes; unapplied changes alone ask the question of today.
            if (modules.HasPendingWork && HasUnappliedDraftChanges()) return EditorPendingWork.When(true, WorkspaceStagedAndUnappliedQuestion);
            return modules.HasPendingWork ? modules : EditorPendingWork.When(HasUnappliedDraftChanges(), WorkspaceUnappliedQuestion);
        }
@@@@ END
@@@@ EDIT src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs
@@@@ OLD
        private const string WorkspaceUnappliedQuestion = "Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?";
@@@@ NEW
        private const string WorkspaceUnappliedQuestion = "Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?";

        // I-64 F4B PB-T4c (D-PBT4-17): the one question when a staged module and unapplied changes would be lost together.
        private const string WorkspaceStagedAndUnappliedQuestion = "Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?";
@@@@ END
@@@@ NEWFILE src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs
#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-15) — the host of the full editor of "Editar todo": it shows the editor modally over the launch that the host
    /// core hands it, inside the modal of the panel, and returns when the editor closes. It is generic so that the workspace names no type of
    /// the systems; it keeps nothing, and the editor only hands its edits to the port of the launch. It is a declared exception to R-PB-05
    /// and to ADR-0029 D12: it lives with the workspace and not with the system.
    /// </summary>
    public interface IWorkspaceFullEditHost
    {
        void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class;
    }
}
@@@@ END
@@@@ NEWFILE src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs
using System;
using RackCad.Application.Persistence;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-13) — the launch of the full Push Back editor over the full draft of the panel: the editable state of the draft
    /// (the window copies it when it loads), the identity of the rack, the project of the envelope of its origin and the port through which the
    /// edits go back to the draft. It reads no drawing: the project comes from the text of the origin that the draft already holds.
    /// </summary>
    public static class PushBackFullDraftLaunch
    {
        public static PushBackDraftEditorLaunch For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (port == null) throw new ArgumentNullException(nameof(port));
            var current = draft.CurrentDraftState;
            return new PushBackDraftEditorLaunch(current.Editable, draft.RackId, ProjectOf(current.Origin), port);
        }

        // The project of the envelope of the origin, with its metadata, or null when the text does not hold one: the launch admits a null one.
        private static RackProject ProjectOf(string origin)
        {
            var envelope = new RackEmbedStore().Deserialize(origin);
            if (envelope == null) return null;
            return new RackProjectStore().TryDeserialize(envelope.Design, out var project, out _) ? project : null;
        }
    }
}
@@@@ END
@@@@ NEWFILE src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs
using System;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-13) — the port of the full editor over the full draft of the panel. It hands the edited state to the draft with
    /// the origin that the draft holds, so the origin is kept, and the draft keeps a COPY of it. Every edit that the draft takes calls back
    /// once, so the host ends the "known clean" mark of the session. It copies nothing and calls no other operation of the draft.
    /// </summary>
    public sealed class PushBackFullDraftStatePort : IPushBackDraftStatePort
    {
        private readonly DraftSession<PushBackFullDraftState> _draft;
        private readonly Action _applied;

        public PushBackFullDraftStatePort(DraftSession<PushBackFullDraftState> draft, Action applied)
        {
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            _applied = applied ?? throw new ArgumentNullException(nameof(applied));
        }

        public PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited)
        {
            if (edited == null) throw new ArgumentNullException(nameof(edited));
            var result = _draft.SetCurrentDraftState(new PushBackFullDraftState(edited, _draft.CurrentDraftState.Origin));
            if (result.Status != DraftEditStatus.Updated) return new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, result.IsDirty);
            _applied();
            return new PushBackDraftApplyResult(PushBackDraftApplyStatus.Applied, result.IsDirty);
        }
    }
}
@@@@ END
@@@@ NEWFILE src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs
#nullable enable
using RackCad.Application.Systems.PushBack;
using RackCad.UI.Systems.PushBack;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-15) — the opener of the full Push Back editor for "Editar todo". It builds the window over the launch and shows
    /// it modally, without an owner, like the classic opener; the modal of the panel is its caller's. It reads and writes nothing of the
    /// drawing and does nothing after the window closes: the window only hands its edits to the port of the launch.
    /// </summary>
    internal static class RackWorkspaceEditorWindow
    {
        internal static void Show(object launch)
        {
            var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);
            AcApplication.ShowModalWindow(window);
        }
    }
}
@@@@ END
```

### La pieza del esqueleto, `pbt4c-skeleton-v51.txt` (sha256 `f8840c67fd41c3bbdabf125d9b80941e3f3aedf5e610681c8ce15f111e7b392e`)

```text
@@@@ PB-T4c (paquete v5.1): el esqueleto exacto de src/ del RED. FROMGREEN toma enteros los bloques del GREEN de esa ruta (pbt4c-green-v51.txt);
@@@@ los demas bloques son los del esqueleto. Gate, WCo, PV, PV.xaml.cs y W no cambian en el RED. Saltos de linea \n.
@@@@ FROMGREEN src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs
@@@@ FROMGREEN src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
@@@@ FROMGREEN src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs
@@@@ EDIT src/RackCad.Application/Workspace/WorkspaceHostCore.cs
@@@@ OLD
        public void SetPilotUpdateHost(IPushBackPilotUpdateHost host) =>
            _updateHost = host ?? throw new ArgumentNullException(nameof(host));
@@@@ NEW
        public void SetPilotUpdateHost(IPushBackPilotUpdateHost host) =>
            _updateHost = host ?? throw new ArgumentNullException(nameof(host));

        /// <summary>I-64 F4B PB-T4c (RED): the setter of the host of the full editor of "Editar todo", still without effect.</summary>
        public void SetFullEditHost(IWorkspaceFullEditHost host)
        {
        }
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
@@@@ OLD
        public bool ShowEditButton => _showEdit;
@@@@ NEW
        public bool ShowEditButton => _showEdit;

        /// <summary>I-64 F4B PB-T4c (RED): the "Editar todo" button, still never offered.</summary>
        public bool ShowEditAllButton => false;
@@@@ END
@@@@ EDIT src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
@@@@ OLD
        /// <summary>Enter or leaving the box: commits the pending text, only with pending text and an editable box.</summary>
@@@@ NEW
        /// <summary>I-64 F4B PB-T4c (RED): "Editar todo", still without effect: it calls no operation of the port.</summary>
        public void EditAll()
        {
        }

        /// <summary>Enter or leaving the box: commits the pending text, only with pending text and an editable box.</summary>
@@@@ END
@@@@ NEWFILE src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs
using System;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>I-64 F4B PB-T4c (RED): the launch of the full Push Back editor over the full draft of the panel, still without the project of its origin.</summary>
    public static class PushBackFullDraftLaunch
    {
        public static PushBackDraftEditorLaunch For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (port == null) throw new ArgumentNullException(nameof(port));
            return new PushBackDraftEditorLaunch(draft.CurrentDraftState.Editable, draft.RackId, null, port);
        }
    }
}
@@@@ END
@@@@ NEWFILE src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs
using System;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>I-64 F4B PB-T4c (RED): the port of the full editor over the full draft of the panel, still without effect: it hands nothing to the draft.</summary>
    public sealed class PushBackFullDraftStatePort : IPushBackDraftStatePort
    {
        private readonly DraftSession<PushBackFullDraftState> _draft;
        private readonly Action _applied;

        public PushBackFullDraftStatePort(DraftSession<PushBackFullDraftState> draft, Action applied)
        {
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            _applied = applied ?? throw new ArgumentNullException(nameof(applied));
        }

        public PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited)
        {
            if (edited == null) throw new ArgumentNullException(nameof(edited));
            return new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, _draft.IsDirty && _applied != null);
        }
    }
}
@@@@ END
@@@@ NEWFILE src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs
#nullable enable
using RackCad.Application.Systems.PushBack;

namespace RackCad.Plugin.Views
{
    /// <summary>I-64 F4B PB-T4c (RED): the opener of the full Push Back editor for "Editar todo", still without effect.</summary>
    internal static class RackWorkspaceEditorWindow
    {
        internal static void Show(object launch)
        {
            _ = launch as PushBackDraftEditorLaunch;
        }
    }
}
@@@@ END
```

### El equivalente de PEG:155-163, `tests/RackCad.Tests/PushBackEditAllPortEquivalenceGuardTests.cs` (`EquivalentSha` `1a2607d3a35ae8f9737fffb0eb32a4acd8100318fe83e5b0098d30428f57d250`)

```csharp
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4B PB-T4c (owner-pbt4-decisions.md, pregunta 4; D-PBT4-13): the equivalent of WorkspacePilotEditGuardTests:155-163, whose Skip is in the
// closed list of PB-T4c. It is that test with the members of its file that it reaches, literal, and ONE more row in the table of the operations:
// TryEditAll, with its default body, at the end of the port, so the port declares eighteen operations and the reflection sees the eighteen.
public class PushBackEditAllPortEquivalenceGuardTests
{
    private const string PortPath = "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs";

    private static string Port() => Src.Code(PortPath);

    private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

    private static readonly (string Name, string Interface)[] Operations =
    {
        ("TryBeginEdit", @"EditIntentOutcome\s+TryBeginEdit\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryCheckNow", @"EditCheckOutcome\s+TryCheckNow\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryReload", @"EditIntentOutcome\s+TryReload\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryConfirmBase", @"EditConfirmOutcome\s+TryConfirmBase\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryKeepMine", @"EditIntentOutcome\s+TryKeepMine\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryDiscardAndReload", @"EditIntentOutcome\s+TryDiscardAndReload\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("GetField", @"PilotFieldView\s+GetField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("SetPendingText", @"PilotFieldResult\s+SetPendingText\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CommitField", @"PilotFieldResult\s+CommitField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CrossBoundary", @"PilotBoundaryResult\s+CrossBoundary\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*PilotBoundaryKind\s+\w+\s*\)\s*;"),
        ("DiscardRack", @"EditDiscardOutcome\s+DiscardRack\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("DiscardDocument", @"EditDiscardOutcome\s+DiscardDocument\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("GetRacksAtRisk", @"PilotRiskView\s+GetRacksAtRisk\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("TakeLossNotices", @"IReadOnlyList<PilotLossView>\s+TakeLossNotices\s*\(\s*\)\s*;"),
        ("TakeCloseNotices", @"IReadOnlyList<PilotCloseView>\s+TakeCloseNotices\s*\(\s*\)\s*;"),

        // I-64 F4-T4C1-HOST (D-T4C-20, row 5): the two operations of the update, at the end of the port.
        ("RequestUpdate", @"EditUpdateStart\s+RequestUpdate\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TakeUpdateProgress", @"EditUpdateProgress\s+TakeUpdateProgress\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),

        // I-64 F4B PB-T4c (D-PBT4-13; owner-pbt4-decisions.md, pregunta 4): "Editar todo", with its default body, at the end of the port.
        ("TryEditAll", @"EditAllOutcome\s+TryEditAll\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*=>\s*EditAllOutcome\.NotAvailable\s*\(\s*null\s*\)\s*;"),
    };

    private static readonly Regex DraftOrPilotNames = new(@"\bDraft\w*|\bPushBack\w*|RackCad\.Application\.Systems|Workspace\.Drafts");

    private static readonly (string Enum, string[] Prefix)[] OrderedEnums =
    {
        ("PilotFieldState", new[] { "NotOpen", "Clean", "Dirty", "Conflict", "Applying", "Unknown", "Error", "Undecidable" }),
        ("PilotBoundaryDecision", new[] { "Proceeded", "Aborted", "Kept", "Busy" }),
        ("PilotBoundaryKind", new[] { "TabChange", "RackChange", "DocumentChange", "PanelHide", "Dematerialize", "DocumentClose", "Update" }),
    };

    private static IReadOnlyList<string>? EnumMembers(string code, string name)
    {
        var body = TryBody(code, @"public\s+enum\s+" + name + @"\b");
        if (body == null) return null;
        var inner = body.Substring(body.IndexOf('{') + 1).TrimEnd('}');
        return inner.Split(',').Select(m => m.Trim()).Where(m => m.Length > 0).ToArray();
    }

    private static IEnumerable<string> PortViolations(string port)
    {
        if (!Regex.IsMatch(port, @"\bpublic\s+interface\s+IWorkspacePilotEditPort\b")) yield return "the interface is missing";
        if (Src.Count(port, @"\bnamespace\s+RackCad\.Application\.Workspace\s*\{") != 1) yield return "the namespace is not the one of the workspace";

        // The seventeen operations (the two of the update are the last) are a lower bound: a later task may add its own to this port.
        foreach (var (name, signature) in Operations)
            if (Src.Count(port, signature) != 1) yield return "the port lacks " + name;

        foreach (Match match in DraftOrPilotNames.Matches(port)) yield return "the port names " + match.Value;

        foreach (var (name, prefix) in OrderedEnums)
        {
            var members = EnumMembers(port, name);
            if (members == null) yield return "the enum " + name + " is missing";
            else if (!members.Take(prefix.Length).SequenceEqual(prefix)) yield return "the enum " + name + " lost its ordered prefix";
        }

        var status = EnumMembers(port, "PilotFieldStatus");
        if (status == null) yield return "the enum PilotFieldStatus is missing";
        else
        {
            foreach (var name in new[] { "Applied", "Rejected", "NotEditable", "Busy", "Invalid" })
                if (!status.Contains(name)) yield return "PilotFieldStatus lacks " + name;
            if (status.Count == 0 || status[0] != "Rejected") yield return "the default of PilotFieldStatus is not the inert Rejected";
        }
    }

    [Fact]
    public void ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot()
    {
        Assert.Empty(PortViolations(Port()));
        Assert.Equal(
            Operations.Select(o => o.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray(),
            typeof(IWorkspacePilotEditPort).GetMethods().Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        Assert.StartsWith("#nullable enable", Src.Text(PortPath).TrimStart((char)0xFEFF));
    }
}
```

### El equivalente de CT-PBT1B-15, `tests/RackCad.Tests/PushBackEditAllNameWritersGuardTests.cs` (`EquivalentSha` `3744992127be8b7e1d8d822a769b8eb2a6886789c73285c8436e5baec6bf7b6b`)

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c (owner-pbt4-decisions.md, pregunta 4; D-PBT4-15): the equivalent of CT-PBT1B-15 of PushBackSharedReferenceGuardTests, whose
    /// Skip is in the closed list of PB-T4c. It is that test with the members of its file that it reaches, literal, with one change: the census of
    /// the files of the Plugin that name PushBack has 15, the 14 of before and the opener of the full editor (RackWorkspaceEditorWindow.cs),
    /// which writes no Name; the seven assignments, the order and the self-tests do not change.
    /// </summary>
    public sealed class PushBackEditAllNameWritersGuardTests
    {
        /// <summary>The text without comments, with the string and character literals emptied (as the source guards do).</summary>
        private static string Clean(string source)
        {
            source = Regex.Replace(source, "@\"(?:[^\"]|\"\")*\"", "\"\"");
            source = Regex.Replace(source, "\\$?\"(?:\\\\.|[^\"\\\\\\n])*\"", "\"\"");
            source = Regex.Replace(source, "'(?:\\\\.|[^'\\\\\\n])'", "''");
            source = Regex.Replace(source, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            source = Regex.Replace(source, @"//[^\n]*", string.Empty);
            return source;
        }

        private static string Collapse(string text)
            => string.Join(" ", text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

        private static string Root => PilotKit.RepoRoot().FullName;

        /// <summary>An injected line, inside a member appended to the real text of a file (never written to disk).</summary>
        private static string Inject(string source, string line)
        {
            var classEnd = source.LastIndexOf('}', source.LastIndexOf('}') - 1);
            return source.Substring(0, classEnd) + "void Inyectado() { " + line + " }\n" + source.Substring(classEnd);
        }

        private const string NameWriter = @"(?<![\w.])\w+(?:\s*\.\s*\w+)*\s*\.\s*Name\s*=(?![=>])";

        private static IEnumerable<string> SourceFiles(string folder)
            => Directory.GetFiles(Path.Combine(Root, folder), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                               && !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .OrderBy(path => path, StringComparer.Ordinal);

        /// <summary>M07: the WHOLE statement of each assignment (up to its semicolon), so a change of its right-hand side shows.</summary>
        private const string NameStatement = NameWriter + @"[^;]*;";

        private static List<(string File, string Text)> Assignments(IReadOnlyDictionary<string, string> cleanedFiles)
            => cleanedFiles
                .SelectMany(file => Regex.Matches(file.Value, NameStatement).Cast<Match>().Select(match => (file.Key, Collapse(match.Value))))
                .ToList();

        /// <summary>The text of a method, from its signature to its closing brace (the text has no comments and its literals are emptied).</summary>
        private static string MethodText(string text, string signature)
        {
            var start = text.IndexOf(signature, StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            var open = text.IndexOf('{', start);
            if (open < 0) return string.Empty;
            var depth = 0;
            for (var index = open; index < text.Length; index++)
            {
                if (text[index] == '{') depth++;
                else if (text[index] == '}' && --depth == 0) return text.Substring(start, index - start + 1);
            }

            return string.Empty;
        }

        /// <summary>M07: the first writer must be INSIDE <c>DrawPushBackView</c>, not merely after its declaration.</summary>
        private static bool WriterInsideDrawPushBackView(string commands)
            => MethodText(commands, "static void DrawPushBackView(").Contains("system.Name = rackName;");

        private static bool OrderOf(string text, string method, string modal, string writer)
        {
            var start = text.IndexOf(method, StringComparison.Ordinal);
            if (start < 0) return false;
            var shown = text.IndexOf(modal, start, StringComparison.Ordinal);
            var written = text.IndexOf(writer, start, StringComparison.Ordinal);
            return shown >= 0 && written >= 0 && shown < written;
        }

        [Fact]
        public void CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses()
        {
            var log = new List<string>();
            var cleaned = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var path in SourceFiles("src/RackCad.Plugin"))
            {
                var text = Clean(File.ReadAllText(path));
                if (text.Contains("PushBack")) cleaned[Path.GetFileName(path)] = text;
            }

            var assignments = Assignments(cleaned);
            log.Add("archivos=" + cleaned.Count);
            log.Add("asignaciones=" + assignments.Count);
            foreach (var assignment in assignments) log.Add(assignment.File + ": " + assignment.Text);

            var pushBack = assignments.Where(a => a.File == "RackPushBackCommands.cs" || a.Text.StartsWith("pushBack", StringComparison.Ordinal)).ToList();
            log.Add("escritores-pushback=" + pushBack.Count);

            var commands = cleaned.ContainsKey("RackPushBackCommands.cs") ? cleaned["RackPushBackCommands.cs"] : string.Empty;
            var menu = cleaned.ContainsKey("RackMenuCommands.cs") ? cleaned["RackMenuCommands.cs"] : string.Empty;

            // The call to DrawPushBackView is the first occurrence (the declaration comes later); the writer is inside it (M07).
            var modalFirst = commands.IndexOf("AcApplication.ShowModalWindow(window);", StringComparison.Ordinal);
            var call = commands.IndexOf("DrawPushBackView(", StringComparison.Ordinal);
            var declaration = commands.IndexOf("static void DrawPushBackView(", StringComparison.Ordinal);
            log.Add("orden=" + (modalFirst >= 0 && modalFirst < call && call < declaration && WriterInsideDrawPushBackView(commands)));
            log.Add("orden=" + OrderOf(commands, "static void EditPushBack(", "AcApplication.ShowModalWindow(window);", "system.Name = name;"));
            log.Add("orden=" + OrderOf(menu, "public void RackCad()", "AcApplication.ShowModalWindow(menu);", "pushBack.System.Name = pushBackName;"));

            var calls = 0;
            foreach (var path in SourceFiles("src"))
            {
                var text = Clean(File.ReadAllText(path));
                calls += Regex.Matches(text, @"\bDrawPushBackView\s*\(").Cast<Match>()
                    .Count(match => !Regex.IsMatch(text.Substring(Math.Max(0, match.Index - 12), Math.Min(12, match.Index)), @"void\s+$"));
            }

            log.Add("DrawPushBackView-llamadas=" + calls);

            // Self-tests, over the real text with an injected change: an eighth assignment, and the writer of EditPushBack moved
            // before its ShowModalWindow.
            var injected = new Dictionary<string, string>(cleaned, StringComparer.Ordinal)
            {
                ["RackPushBackCommands.cs"] = Inject(commands, "state.Editor.SideA.WorkingBaseline.Name = \"x\";"),
            };
            log.Add("autoprueba-octava=" + (Assignments(injected).Count == 7 ? "pasa" : "falla"));

            var editStart = commands.IndexOf("static void EditPushBack(", StringComparison.Ordinal);
            var moved = commands;
            if (editStart >= 0)
            {
                var writer = moved.IndexOf("system.Name = name;", editStart, StringComparison.Ordinal);
                var modal = moved.IndexOf("AcApplication.ShowModalWindow(window);", editStart, StringComparison.Ordinal);
                if (writer >= 0 && modal >= 0)
                {
                    moved = moved.Remove(writer, "system.Name = name;".Length).Insert(modal, "system.Name = name; ");
                }
            }

            log.Add("autoprueba-orden=" + (OrderOf(moved, "static void EditPushBack(", "AcApplication.ShowModalWindow(window);", "system.Name = name;") ? "pasa" : "falla"));

            // M07: the writer of DrawPushBackView moved out of the method (to just before its declaration) no longer counts as inside it.
            var outside = commands;
            if (declaration >= 0)
            {
                var inside = outside.IndexOf("system.Name = rackName;", declaration, StringComparison.Ordinal);
                if (inside >= 0)
                {
                    outside = outside.Remove(inside, "system.Name = rackName;".Length).Insert(declaration, "system.Name = rackName; ");
                }
            }

            log.Add("autoprueba-escritor-fuera-de-DrawPushBackView=" + (WriterInsideDrawPushBackView(outside) ? "pasa" : "falla"));

            Assert.Equal(
                new[]
                {
                    "archivos=15",
                    "asignaciones=7",
                    "RackMenuCommands.cs: header.Configuration.Name = Systems.Shared.RackNewRackName.Resolve( document, global::RackCad.Domain.Systems.Shared.RackSystemKind.Selective, header.Configuration.Name);",
                    "RackMenuCommands.cs: dynamic.System.Name = dynamicName;",
                    "RackMenuCommands.cs: selective.System.Name = selectiveName;",
                    "RackMenuCommands.cs: cantilever.Design.Name = cantileverName;",
                    "RackMenuCommands.cs: pushBack.System.Name = pushBackName;",
                    "RackPushBackCommands.cs: system.Name = rackName;",
                    "RackPushBackCommands.cs: system.Name = name;",
                    "escritores-pushback=3",
                    "orden=True",
                    "orden=True",
                    "orden=True",
                    "DrawPushBackView-llamadas=1",
                    "autoprueba-octava=falla",
                    "autoprueba-orden=falla",
                    "autoprueba-escritor-fuera-de-DrawPushBackView=falla",
                },
                log);
        }
    }
}
```

### El equivalente de CT-PBT2B-06, `tests/RackCad.UI.Tests/PushBackEditAllApplyCloseTests.cs` (`EquivalentSha` `7c2458260ef301d7f05bfaff8e1e8c4a4a618f25fd7022705d55dee32dbe78d2`)

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackWorkspaceSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c (owner-pbt4c-list-4-decision.md; D-PBT4-17, A-T2b-OWNER-01): the equivalent of CT-PBT2B-06 of
    /// PushBackWorkspaceApplyCloseTests, whose Skip is in the closed list of PB-T4c. It is that test with the members of its file that it
    /// reaches (Sesion, Nueva and Cierre), literal, with one change: the expected line of window 4, where a staged module and unapplied changes
    /// are pending together, is the one question that names both (WorkspaceStagedAndUnappliedQuestion of the window).
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackEditAllApplyCloseTests
    {
        /// <summary>Una ventana de WORKSPACE con su borrador, su puerto contado y el estado del borrador de antes de abrirla.</summary>
        private sealed class Sesion
        {
            public RackPushBackSystemWindow W;
            public DraftSession<PushBackEditableState> Draft;
            public PuertoContado Puerto;
            public PushBackEditableState Antes;
        }

        private static Sesion Nueva(List<string> log, PushBackEditableState estado, RackProject proyecto = null)
        {
            var (_, draft) = Borrador(estado);
            var puerto = new PuertoContado(draft);
            var antes = Copy(draft.CurrentDraftState);
            var w = Paso(log, () => VentanaBorrador(draft, proyecto, puerto));
            return w == null ? null : new Sesion { W = w, Draft = draft, Puerto = puerto, Antes = antes };
        }

        /// <summary>Una ventana de WORKSPACE sobre su propio borrador: sonda, gestos y, en el <c>finally</c>, «Si» y <c>Cerrar</c>.</summary>
        private static bool Cierre(List<string> log, PromptProbe avisos, Action<Sesion> gestos)
        {
            var s = Nueva(log, Estado(Fixture(DisenoBase)));
            if (s == null) return false;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, s.W)) return false;
                gestos(s);
                return true;
            }
            finally
            {
                // `CloseQuietly` de T-CL no cierra una ventana sin mostrar: se contesta «Si» y se cierra con `Cerrar`.
                avisos.Answer = true;
                CerrarVentana(s.W);
            }
        }

        [Fact]
        public void CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            // El primer Sta del proceso crea el hilo STA e instala en el su aviso de descarte permanente: se arranca ANTES de sustituir
            // el aviso de esta prueba, o el primero que corra en el proceso la dejaria sin su sustituto.
            Sta(() => { });
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    // Ventana 1: sin cambios, «Cerrar» cierra sin preguntar.
                    if (!Cierre(log, avisos, s =>
                    {
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-sin-cambios avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W)
                            + " borrador-igual=" + Igual(s.Draft.CurrentDraftState, s.Antes));
                    }))
                    {
                        return;
                    }

                    // Ventana 2: una edicion sin aplicar; «No» la deja abierta, «Si» la cierra y el borrador queda como estaba.
                    if (!Cierre(log, avisos, s =>
                    {
                        SetNumberR(s.W, "ToleranceBox", 4.5);
                        avisos.Answer = false;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-cambios-no avisos=" + avisos.Asked + " pregunta=" + avisos.Question + " cerrada=" + EstaCerradaVentana(s.W));
                        avisos.Answer = true;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-cambios-si avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W)
                            + " borrador-igual=" + Igual(s.Draft.CurrentDraftState, s.Antes) + " llamadas=" + s.Puerto.Llamadas);
                    }))
                    {
                        return;
                    }

                    // Ventana 3: lo editado se aplico, y «Cerrar» no pregunta.
                    if (!Cierre(log, avisos, s =>
                    {
                        SetNumberR(s.W, "ToleranceBox", 4.5);
                        Click(s.W, "UpdateButton");
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-tras-aplicar avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W) + " llamadas=" + s.Puerto.Llamadas);
                    }))
                    {
                        return;
                    }

                    // Ventana 4: un modulo escenificado sin confirmar manda primero, con su texto de siempre.
                    if (!Cierre(log, avisos, s =>
                    {
                        StageModuleEdit(s.W);
                        avisos.Answer = true;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-modulo avisos=" + avisos.Asked + " pregunta=" + avisos.Question + " cerrada=" + EstaCerradaVentana(s.W));
                    }))
                    {
                        return;
                    }

                    // CLASSIC: «Cerrar» no pregunta por lo editado, como siempre (ADR-0029 D8).
                    var clasica = Ventana();
                    try
                    {
                        avisos.Reiniciar();
                        Cargar(clasica, Fixture(DisenoBase));
                        SetNumberR(clasica, "ToleranceBox", 4.5);
                        Click(clasica, "CloseButton");
                        log.Add("clasico-con-cambios avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(clasica)
                            + " peticion=" + clasica.InsertRequested);
                    }
                    finally
                    {
                        avisos.Answer = true;
                        CerrarVentana(clasica);
                    }
                });
            }

            var esperadas = new List<string>
            {
                "cerrar-sin-cambios avisos=0 cerrada=True borrador-igual=True",
                "cerrar-con-cambios-no avisos=1 pregunta=" + PreguntaSinAplicar + " cerrada=False",
                "cerrar-con-cambios-si avisos=2 cerrada=True borrador-igual=True llamadas=0",
                "cerrar-tras-aplicar avisos=0 cerrada=True llamadas=1",
                "cerrar-con-modulo avisos=1 pregunta=Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar? cerrada=True",
                "clasico-con-cambios avisos=0 cerrada=True peticion=False",
            };

            Assert.Equal(esperadas, log);
        }
    }
}
```

### El equivalente de CT-PBT4B-20, `tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs` (`EquivalentSha` `4619b6d310340f9c97a7d9f31bda1db852d2fcda905b91bb857510f7e6b7e6ce`)

Es `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs` de `6adff90d` entero (656 líneas), con dos cambios y nada más:

1. la clase: `public sealed class WorkspaceFullCopyGuardTests` → `public sealed class PushBackEditAllFullCopyGuardTests`;

2. `Restore`:

```csharp
        private static string Restore(string text) => SkipLine.Replace(text, "$1[$2]");
```

pasa a:

```csharp
        // I-64 F4B PB-T4c (owner-pbt4c-list-3-decision.md): the one Skip of PB-T4c in an original, the one of WorkspacePilotEditGuardTests:155,
        // goes back to its [Fact] too, by its exact line; the count of the Skip of PB-T4b and the expected lines do not change.
        private const string EditAllSkipLine = "    [Fact(Skip = \"I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).\")]";

        private static string Restore(string text) => SkipLine.Replace(text, "$1[$2]").Replace(EditAllSkipLine + "\n", "    [Fact]\n");
```

## Anexo V54-A. El texto literal de PB-T4d: el GREEN, las ediciones con registro, los ayudantes sin uso, los `Skip` y los archivos nuevos (v5.4)

Las bajas con registro no van aquí: son las filas «borrar con registro» y «sustituir con registro» de la lista, y su texto es el de la pieza del RED (`pbt4d-red-v54.txt`, sha256 `80f8cc0c5c9f54de033648e7eec95728fa0fc0d28e31886101207e3dbec381db`), que las quita. Los anexos V52-A y V53-A de la v5.2 y la v5.3 quedan como historia.

### La pieza del GREEN, `pbt4d-green-v54.txt` (sha256 `aaf6cab882cfb3eba6939fd8b64ec1fa15c22431ba8bdaeb9bcdf4dbb89ffbf0`)

```text
@@@@ # PB-T4d (I-64 F4B), paquete v5.4: el GREEN literal de src/, la retirada del piloto con el alcance B (owner-pbt4d-list-decisions.md, pregunta 1),
@@@@ # la limpieza de cuatro cref y un comentario (D-S15, solo comentarios), la rama muerta de BuildCore (el parametro allowErase y lo que
@@@@ # solo existe con el; owner-pbt4d-deadcode-27-decisions.md, pregunta 1) y, al final, los comentarios que describian el piloto como
@@@@ # presente (CA-T4d M-05 (a), solo comentarios). Los bloques de la v5.2 no cambian.
@@@@ # Bloques: EDIT <ruta> / OLD / NEW / END (el OLD esta exactamente una vez en el texto que se va editando; un NEW sin lineas es el texto vacio)
@@@@ # y DELETE <ruta> (el archivo se borra). Los textos, con saltos de linea \n. Lo aplica green_pbt4d_v53.py (apply_green_pbt4d_v53.py) sobre 6c2820ba; desde la v5.3, solo cambia esta cabecera.

@@@@ DELETE src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs

@@@@ DELETE src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotFieldAdapter.cs

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs
@@@@ OLD
    /// <summary>
    /// The pair <see cref="PushBackCompositeEditorAssembler.BuildDesign"/> consumes, or the reason the base cannot be
    /// composed. The state and the inputs of a composition belong to the caller: they are fresh objects, never one of the
    /// states of a draft.
    /// </summary>
    public sealed class PushBackPilotComposition
    {
        private PushBackPilotComposition(PushBackCompositeEditorState? editor, PushBackEditorInputs? inputs, string? reason)
        {
            Editor = editor;
            Inputs = inputs;
            Reason = reason;
        }

        public bool IsComposed => Editor != null && Inputs != null;

        /// <summary>The composite state to hand to the assembler; null when the composition was rejected.</summary>
        public PushBackCompositeEditorState? Editor { get; }

        /// <summary>The rack-wide inputs to hand to the assembler; null when the composition was rejected.</summary>
        public PushBackEditorInputs? Inputs { get; }

        /// <summary>Why the composition was rejected; null when it was composed.</summary>
        public string? Reason { get; }

        public static PushBackPilotComposition Composed(PushBackCompositeEditorState editor, PushBackEditorInputs inputs)
            => new PushBackPilotComposition(
                editor ?? throw new ArgumentNullException(nameof(editor)),
                inputs ?? throw new ArgumentNullException(nameof(inputs)),
                null);

        public static PushBackPilotComposition Rejected(string reason)
            => new PushBackPilotComposition(null, null, reason);
    }

@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs
@@@@ OLD
        /// <summary>
        /// The composition to write (IC-07 point 2, D-12): what the pilot does not edit comes from the CURRENT base, never from
        /// the draft. (1) If the editable content of <paramref name="current"/> differs from the one of
        /// <paramref name="initial"/> in anything other than <c>Inputs.Annotations.AnnotationScale</c> (the pilot comparator
        /// with that one field neutralized), it is a typed rejection: nothing that the pilot does not edit is ever reverted or
        /// written in silence. (2) Otherwise it materializes AGAIN from <c>initial.Origin</c> — the current base, which gives
        /// a fresh module authority — and applies to it ONLY the scale of <paramref name="current"/>. It returns the pair
        /// <see cref="PushBackCompositeEditorAssembler.BuildDesign"/> consumes. Pure and total: it mutates neither state, it
        /// ignores <c>current.Origin</c> and it never throws; a base that no longer materializes gives a typed rejection.
        /// Since F4-T4A1 (D-T4A1-05, the carry-overs of F4-E25) the comparison of (1) is made on the current state as it is, a scale that
        /// is not finite or not greater than zero is rejected (6), and so is a base that, materialized again, no longer equals the
        /// initial state (3). It is only safe AFTER a match of the base with a re-read (H-06): the plan of an update calls it then.
        /// </summary>
        public static PushBackPilotComposition ComposeForWrite(
            PushBackPilotDraftState initial, PushBackPilotDraftState current, RackCatalog catalog)
        {
            try
            {
                if (initial == null || current == null) return PushBackPilotComposition.Rejected("Falta el estado inicial o el actual.");
                if (initial.Inputs.Annotations == null || current.Inputs.Annotations == null)
                    return PushBackPilotComposition.Rejected(NonPilotContentReason);

                // (6) An invalid scale that arrived by an edit in place is never written: not a number, infinite, zero or negative.
                var scale = current.Inputs.Annotations.AnnotationScale;
                if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0.0)
                    return PushBackPilotComposition.Rejected(InvalidScaleReason);

                // (1) The current state is compared as it is, never normalized: it is the copy of the INITIAL state that has its scale
                // neutralized (it takes the scale of the current one), so nothing of the current state is hidden by a copy.
                var neutralInitial = PushBackPilotStateComparer.Copy(initial);
                neutralInitial.Inputs.Annotations.AnnotationScale = scale;
                if (!PushBackPilotStateComparer.Instance.Equals(neutralInitial, current))
                    return PushBackPilotComposition.Rejected(NonPilotContentReason);

                var fresh = Materialize(initial.Origin, catalog);
                if (!fresh.IsAdmitted || fresh.State == null)
                    return PushBackPilotComposition.Rejected(fresh.Reason ?? UnreadableOrFutureDiagnostic);

                // (3) The base that is materialized AGAIN must still be the initial state: if the catalog changed in the meantime, the
                // base is no longer the one the user saw and nothing is composed over it.
                if (!PushBackPilotStateComparer.Instance.Equals(initial, fresh.State))
                    return PushBackPilotComposition.Rejected(BaseChangedReason);

                if (fresh.State.Inputs.Annotations == null) return PushBackPilotComposition.Rejected(NonPilotContentReason);
                fresh.State.Inputs.Annotations.AnnotationScale = current.Inputs.Annotations.AnnotationScale;
                return PushBackPilotComposition.Composed(fresh.State.Editor, fresh.State.Inputs);
            }
            catch (Exception ex)
            {
                return PushBackPilotComposition.Rejected(ComposeFailurePrefix + ex.Message);
            }
        }

        /// <summary>
        /// After the user keeps the draft on a newer version (KeepMineOnCurrentVersion) the surface needs a current state that
        /// is the NEW base plus the one thing the pilot edits: a copy of <paramref name="newInitial"/> carrying the
        /// <c>AnnotationScale</c> of <paramref name="current"/>. Pure: it mutates neither argument and shares no mutable
        /// object with them; the surface sets the result as the current state of the draft.
        /// </summary>
        public static PushBackPilotDraftState KeepPilotEditsOn(PushBackPilotDraftState newInitial, PushBackPilotDraftState current)
        {
            if (newInitial == null) throw new ArgumentNullException(nameof(newInitial));
            if (current == null) throw new ArgumentNullException(nameof(current));

            var kept = PushBackPilotStateComparer.Copy(newInitial);
            if (kept.Inputs.Annotations != null && current.Inputs.Annotations != null)
                kept.Inputs.Annotations.AnnotationScale = current.Inputs.Annotations.AnnotationScale;
            return kept;
        }
@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
    /// <summary>
    /// I-64 F4-T4A1 (D-T4A1-04, N-7) — the explicit snapshot of a draft that <see cref="PushBackPilotUpdatePlanner.Prepare"/> reads. It
    /// is built by ONE factory, <see cref="From"/>, so no other code names the base or the states of a draft to prepare an update.
    /// The two states are copies, and <see cref="ConsideredStatus"/> is the status the draft had before an attempt that is in flight.
    /// </summary>
    public sealed class PushBackPilotPrepareInput
    {
        private PushBackPilotPrepareInput(
            string rackId,
            DraftStaleBase staleBase,
            PushBackPilotDraftState initialDraftState,
            PushBackPilotDraftState currentDraftState,
            bool hasPendingText,
            bool isDirty,
            DraftStatus consideredStatus)
        {
            RackId = rackId;
            StaleBase = staleBase;
            InitialDraftState = initialDraftState;
            CurrentDraftState = currentDraftState;
            HasPendingText = hasPendingText;
            IsDirty = isDirty;
            ConsideredStatus = consideredStatus;
        }

        public string RackId { get; }

        /// <summary>The base of the draft: what the last read of the drawing saw.</summary>
        public DraftStaleBase StaleBase { get; }

        /// <summary>A COPY of the initial state of the draft.</summary>
        public PushBackPilotDraftState InitialDraftState { get; }

        /// <summary>A COPY of the current state of the draft.</summary>
        public PushBackPilotDraftState CurrentDraftState { get; }

        public bool HasPendingText { get; }

        public bool IsDirty { get; }

        /// <summary>The status of the draft, and under Applying the status it had before the attempt began.</summary>
        public DraftStatus ConsideredStatus { get; }

        /// <summary>The one construction of the snapshot of a draft.</summary>
        public static PushBackPilotPrepareInput From(DraftSession<PushBackPilotDraftState> draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            var status = draft.Status;
            return new PushBackPilotPrepareInput(
                draft.RackId,
                draft.StaleBase,
                draft.InitialDraftState,
                PushBackPilotStateComparer.Copy(draft.CurrentDraftState),
                draft.HasPendingText,
                draft.IsDirty,
                status == DraftStatus.Applying ? draft.StatusBeforeAttempt : status);
        }
    }
@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        /// The pure PREPARE of an update. It evaluates, in this order and returning at the first that holds: NotApplicable (the draft is
        /// not Clean or Dirty), PendingText, NothingToApply (Clean; the re-read is not looked at), Conflict (the raw comparison of the
        /// base with the re-read, BEFORE the admission), NotAdmitted (only with a match), Invalid (a to e) and Ready. It is total: any
        /// exception is an Invalid with its reason. It changes nothing, reads nothing and writes nothing.
        /// </summary>
        public static PushBackPilotPrepareResult Prepare(PushBackPilotPrepareInput input, PushBackPilotIntentResult reread, RackCatalog catalog)
        {
            try
            {
                if (input == null) return PushBackPilotPrepareResult.WithInvalid(MissingInputReason);

                if (input.ConsideredStatus != DraftStatus.Clean && input.ConsideredStatus != DraftStatus.Dirty)
                    return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.NotApplicable);
                if (input.HasPendingText) return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.PendingText);
                if (input.ConsideredStatus == DraftStatus.Clean) return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.NothingToApply);

                if (reread == null) return PushBackPilotPrepareResult.WithInvalid(MissingInputReason);

                var comparison = input.StaleBase.CompareWith(reread.StaleBase);
                if (!comparison.IsMatch) return PushBackPilotPrepareResult.WithConflict(comparison.Differences);

                if (!reread.Read.IsAdmitted)
                    return PushBackPilotPrepareResult.WithNotAdmitted(reread.Read.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic);

                return Build(input, reread, catalog);
            }
            catch (Exception ex)
            {
                return PushBackPilotPrepareResult.WithInvalid(PrepareFailurePrefix + ex.Message);
            }
        }

        /// <summary>
@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        private static PushBackPilotPrepareResult Build(PushBackPilotPrepareInput input, PushBackPilotIntentResult reread, RackCatalog catalog)
        {
            // A missing catalog is answered here, after the comparison and the admission (M-6): a difference of the base or a read that is not
            // admitted is told as such even when the catalog is missing.
            if (catalog == null) return PushBackPilotPrepareResult.WithInvalid(MissingInputReason);

            // The current state is composed over the base that matches the re-read (H-06), with the carry-overs of D-T4A1-05. The pilot never erases a view.
            var composition = PushBackPilotMaterializer.ComposeForWrite(input.InitialDraftState, input.CurrentDraftState, catalog);
            return BuildCore(
                catalog,
                input.RackId,
                input.StaleBase,
                reread.StaleBase,
                input.InitialDraftState.Origin,
                input.CurrentDraftState.Inputs.Annotations.AnnotationScale,
                false,
                null,
                composition.Editor,
                composition.Inputs,
                composition.IsComposed ? null : composition.Reason ?? PushBackPilotMaterializer.NonPilotContentReason);
        }
@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
@@@@ OLD
        /// <summary>
        /// The check before the slot (step 1): the pure PREPARE without a re-read and without a catalog. It only stops for the three kinds
        /// that need neither; anything else goes on.
        /// </summary>
        public static PushBackPilotPrecheck Precheck(DraftSession<PushBackPilotDraftState> draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));

            var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), null!, null!);
            switch (result.Kind)
            {
                case PushBackPilotPrepareKind.NotApplicable: return PushBackPilotPrecheck.Stop(result.Kind, NotApplicableReason);
                case PushBackPilotPrepareKind.PendingText: return PushBackPilotPrecheck.Stop(result.Kind, PendingTextReason);
                case PushBackPilotPrepareKind.NothingToApply: return PushBackPilotPrecheck.Stop(result.Kind, NothingToApplyReason);
                default: return PushBackPilotPrecheck.Go();
            }
        }

@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
@@@@ OLD
        /// <summary>
        /// The whole update under the slot. TOTAL: it never throws. The outcome is the one that the draft must be closed with.
        /// </summary>
        public static DraftApplyOutcome<PushBackPilotDraftState> Execute(
            DraftSession<PushBackPilotDraftState> draft,
            PushBackPilotUpdateRequest request,
            IPushBackPilotUpdatePort port)
        {
            if (draft == null || request == null || port == null)
                return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(MissingArgumentsReason);

            // M-01: the state of the run lives outside the try, so what escapes is classified with what the run already did.
            PushBackPilotPrepareInput? input = null;
            var execution = new Execution<PushBackPilotDraftState>(
                draft,
                request,
                port,
                snapshotted =>
                {
                    input = PushBackPilotPrepareInput.From(snapshotted);
                    return input.InitialDraftState.Origin;
                },
                (_, scan, catalog) => PushBackPilotUpdatePlanner.Prepare(input!, PushBackPilotIntentReader.Read(scan, catalog), catalog),
                (scan, catalog, plan) =>
                {
                    var read = PushBackPilotIntentReader.Read(scan, catalog);
                    return (read.Read, ComparePost(plan, read));
                });
            try
            {
                return execution.Run();
            }
            catch (Exception ex)
            {
                return execution.OutcomeOfAnEscape(ex);
            }
        }

@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
@@@@ OLD
        /// <summary>
        /// The comparison of POST, without normalizing anything (D-T4B-05): the same keys as the plan, the envelope of each key equal
        /// ordinally, and the same scale. Null when everything matches; otherwise the cause.
        /// </summary>
        public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackPilotIntentResult? postRead)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (postRead == null) return PostRereadMissingReason;

            return ComparePostCore(plan, postRead.Read.Admission, postRead.StaleBase, postRead.Read.InitialState?.Inputs.Annotations.AnnotationScale);
        }

@@@@ NEW
@@@@ END

@@@@ # D-S15 (la sesion): solo comentarios, sobre el texto anterior: los cuatro cref a lo retirado y el comentario que se queda sin su Build.

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs
@@@@ OLD
    /// operation that assembles a design: a design is only ever assembled from a composition made by
    /// <see cref="PushBackPilotMaterializer.ComposeForWrite"/>, never directly over the state of a draft.
@@@@ NEW
    /// operation that assembles a design: a design is never assembled directly over the state of a draft.
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
    /// I-64 F4B PB-T3b (D-PBT3-09) — the explicit snapshot of a draft of the FULL model that the full PREPARE reads. It is the twin of
    /// <see cref="PushBackPilotPrepareInput"/>, which does not change, and it is built by ONE factory, <see cref="From"/>.
@@@@ NEW
    /// I-64 F4B PB-T3b (D-PBT3-09) — the explicit snapshot of a draft of the FULL model that the full PREPARE reads. It is built by ONE
    /// factory, <see cref="From"/>.
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
    /// <summary>The seven outcomes of <see cref="PushBackPilotUpdatePlanner.Prepare"/>, in the order in which they are evaluated.</summary>
@@@@ NEW
    /// <summary>The seven outcomes of <see cref="PushBackPilotUpdatePlanner.PrepareFull"/>, in the order in which they are evaluated.</summary>
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
    /// <summary>The result of <see cref="PushBackPilotUpdatePlanner.Prepare"/>. Immutable.</summary>
@@@@ NEW
    /// <summary>The result of <see cref="PushBackPilotUpdatePlanner.PrepareFull"/>. Immutable.</summary>
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        // The base of the draft and the re-read match here, and the re-read is admitted: the plan can be built, or it is Invalid.

        // The same, for the full model: the composition is the one of the model of the draft, and the plan may erase the laterals that the edition leaves obsolete.
@@@@ NEW
        // The base of the draft and the re-read match here, and the re-read is admitted: the plan can be built, or it is Invalid. The composition
        // is the one of the model of the draft, and the plan may erase the laterals that the edition leaves obsolete.
@@@@ END

@@@@ # La rama muerta de BuildCore (owner-pbt4d-deadcode-27-decisions.md, pregunta 1): el parametro allowErase y lo que solo existe con el,

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        public const string ViewUnavailablePrefix = "El piloto no borra vistas y la vista ";

        public const string ViewUnavailableSuffix = " ya no está disponible en el sistema saliente: usa RACKEDITAR.";

@@@@ NEW
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
                input.CurrentDraftState.Editable.AnnotationScale,
                true,
@@@@ NEW
                input.CurrentDraftState.Editable.AnnotationScale,
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        // The building of the plan, common to the two models: the composition (or why it was rejected), the base of the draft, the base of the re-read
        // and the origin of the draft, the scale, whether the model may erase views and the name of the draft.
@@@@ NEW
        // The building of the plan: the composition (or why it was rejected), the base of the draft, the base of the re-read and the origin of the
        // draft, the scale and the name of the draft. The plan may erase the laterals that the edition leaves obsolete.
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
            double scale,
            bool allowErase,
@@@@ NEW
            double scale,
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
            // (e) What the edition does to each view, by key (ordinal). The pilot never erases a view: a member that is not available in the outgoing
            // system makes its update Invalid. The full model erases an obsolete lateral and rejects the frontal of side B of a rack without it.
@@@@ NEW
            // (e) What the edition does to each view, by key (ordinal): an obsolete lateral is erased, and the frontal of side B of a rack without it
            // is rejected.
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
                if (!decoded.HasAddress || (allowErase && disposition == PushBackViewDispositionKind.NoAddress))
@@@@ NEW
                if (!decoded.HasAddress || disposition == PushBackViewDispositionKind.NoAddress)
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
                if (disposition == PushBackViewDispositionKind.ObsoleteLateral && allowErase)
@@@@ NEW
                if (disposition == PushBackViewDispositionKind.ObsoleteLateral)
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
                if (disposition == PushBackViewDispositionKind.FrontalBOfOneWay && allowErase)
                    return PushBackPilotPrepareResult.WithInvalid(FrontalBOfOneWayPrefix + entry.Key + FrontalBOfOneWaySuffix);
                if (disposition != PushBackViewDispositionKind.Redraw)
                    return PushBackPilotPrepareResult.WithInvalid(ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix);
@@@@ NEW
                if (disposition == PushBackViewDispositionKind.FrontalBOfOneWay)
                    return PushBackPilotPrepareResult.WithInvalid(FrontalBOfOneWayPrefix + entry.Key + FrontalBOfOneWaySuffix);
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
                // (f) The name: the pilot keeps the one of each envelope that was re-read; the full model writes the one of the draft, or, when it is
                // blank, the one of the envelope that was re-read for the origin, in every view.
                var memberName = allowErase ? (string.IsNullOrWhiteSpace(draftName) ? origins[0].Embed.Name : draftName) : embed.Name;
@@@@ NEW
                // (f) The name: the one of the draft, or, when it is blank, the one of the envelope that was re-read for the origin, in every view.
                var memberName = string.IsNullOrWhiteSpace(draftName) ? origins[0].Embed.Name : draftName;
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
            if (allowErase && dispositions.AllObsolete) return PushBackPilotPrepareResult.WithInvalid(AllViewsObsoleteReason);
@@@@ NEW
            if (dispositions.AllObsolete) return PushBackPilotPrepareResult.WithInvalid(AllViewsObsoleteReason);
@@@@ END

@@@@ # CA-T4d M-05 (a) (la sesion): solo comentarios, sobre el texto anterior: los que describian como presentes la corrida, el PREPARE y
@@@@ # la composicion para escribir del piloto, que se retiran.

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs
@@@@ OLD
    /// I-64 F4-T2A (D-T2-03, D-T2-04) — materializes the pilot state from the authored envelope text and composes the
    /// state to write. It owns NO parser: it reads through the existing RACKEDITAR path (<see cref="RackEmbedStore"/>, the
@@@@ NEW
    /// I-64 F4-T2A (D-T2-03, D-T2-04) — materializes the pilot state from the authored envelope text; since I-64 F4B PB-T4d it no
    /// longer composes a state to write (that went with the run of the pilot). It owns NO parser: it reads through the existing
    /// RACKEDITAR path (<see cref="RackEmbedStore"/>, the
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
@@@@ OLD
    /// I-64 F4-T4B (D-T4B-02) — the pure and TOTAL run of the update of the bounded Push Back pilot: it decides, classifies and
@@@@ NEW
    /// I-64 F4-T4B (D-T4B-02) — the pure and TOTAL run of the update, born for the bounded Push Back pilot and, since I-64 F4B
    /// PB-T4d, the one of the full model only (the run of the pilot is retired): it decides, classifies and
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
@@@@ OLD
    /// I-64 F4B PB-T3b (D-PBT3-10) — the same run serves the two models of the draft: the one of the pilot and the full one. The views that the
    /// update leaves obsolete are taken away between the start of MUTATE and its seal, by the capacity that the scope of MUTATE may implement.
@@@@ NEW
    /// I-64 F4B PB-T3b (D-PBT3-10) — the same run served the two models of the draft, the one of the pilot and the full one; since I-64 F4B
    /// PB-T4d it serves only the full one (ExecuteFull), because the run of the pilot is retired. The views that the update leaves obsolete
    /// are taken away between the start of MUTATE and its seal, by the capacity that the scope of MUTATE may implement.
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
@@@@ OLD
        /// The whole update under the slot for a draft of the FULL model (D-PBT3-10). TOTAL: it never throws. It is the same run as the one of the
        /// pilot, with the plan, the re-read and the comparison of the full model.
@@@@ NEW
        /// The whole update under the slot for a draft of the FULL model (D-PBT3-10). TOTAL: it never throws. It is the generic run, with the plan,
        /// the re-read and the comparison of the full model (the run of the pilot that shared it is retired in I-64 F4B PB-T4d).
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
@@@@ OLD
        /// <summary>The comparison of POST for the FULL model (D-PBT3-10): the erased views are gone, and the rest as for the pilot.</summary>
@@@@ NEW
        /// <summary>The comparison of POST for the FULL model (D-PBT3-10): the erased views are gone, and the keys, the envelopes and the
        /// scale are the ones of the plan.</summary>
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
    /// What the plan redraws for one member that the pilot may change: the view and the section as RACKEDITAR writes them, the text
@@@@ NEW
    /// What the plan redraws for one member that the update may change: the view and the section as RACKEDITAR writes them, the text
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        public IReadOnlyList<PushBackPilotKeyedText> Envelopes { get; }

        /// <summary>The keys of the views that the update erases, ordered by key (ordinal); empty in the pilot.</summary>
@@@@ NEW
        public IReadOnlyList<PushBackPilotKeyedText> Envelopes { get; }

        /// <summary>The keys of the views that the update erases, ordered by key (ordinal).</summary>
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        public string PostOriginKey { get; }

        /// <summary>The keys of the views that the update erases, ordered by key (ordinal); empty in the pilot.</summary>
@@@@ NEW
        public string PostOriginKey { get; }

        /// <summary>The keys of the views that the update erases, ordered by key (ordinal).</summary>
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        /// <summary>The members that the pilot may change, ordered by key (ordinal).</summary>
@@@@ NEW
        /// <summary>The members that the update may change, ordered by key (ordinal).</summary>
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
    /// I-64 F4-T4A1 (D-T4A1-04) — the pure PREPARE of an update of the bounded Push Back pilot. It reads a snapshot of the draft and
@@@@ NEW
    /// I-64 F4-T4A1 (D-T4A1-04) — the pure PREPARE of an update, born for the bounded Push Back pilot and, since I-64 F4B PB-T4d, the
    /// one of the full model only (the PREPARE of the pilot is retired). It reads a snapshot of the draft and
@@@@ END

@@@@ EDIT src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
@@@@ OLD
        /// The pure PREPARE of an update of the FULL model (D-PBT3-09), with the same order as the one of the pilot: NotApplicable,
@@@@ NEW
        /// The pure PREPARE of an update of the FULL model (D-PBT3-09), in the order that the PREPARE of the pilot had (D-T4A1-04; retired
        /// in I-64 F4B PB-T4d): NotApplicable,
@@@@ END
```

### Las ediciones con registro (`pbt4d_test_edits_v54.py`, `EDITS`, en su orden, sobre el texto ya sin las bajas)

**`tests/RackCad.Tests/PushBackFullWritePrepareTests.cs`:** CT-PBT3B-05 fija el nombre del modelo completo; la linea del piloto nombraba su Prepare, que se retira.

Desde la línea que contiene el texto de abajo hasta la anterior a la que contiene `Finish(`:

```text
            // The pilot keeps the name of each envelope that was re-read.
```

Pasa a:

```text
            // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the part of the pilot (its Prepare, retired) goes; what the full model writes stays.
```

**`tests/RackCad.Tests/PushBackFullWritePrepareTests.cs`:** CT-PBT3B-05: sale la linea esperada del piloto.

El texto, una vez en el archivo:

```text
                    "nombre-en-blanco prepare=Ready A1=Origen B2=Origen C3=Origen",
                    "piloto prepare=Ready A1=Origen B2=Otro-B C3=Otro-C",
```

Pasa a:

```text
                    "nombre-en-blanco prepare=Ready A1=Origen B2=Origen C3=Origen",
```

**`tests/RackCad.Tests/PushBackFullWritePrepareTests.cs`:** CT-PBT3B-06: (ii) es del piloto (ComposeForWrite y su Prepare); (i) fija la barrera del modelo completo; (iii), el corpus por el ensamblador y la barrera, pasa a borradores del modelo completo por PrepareFull (CA-T4d M-01).

Desde la línea que contiene el texto de abajo hasta la anterior a la que contiene `const string blockedPrefix = "El diseño tiene un error que impide escribirlo: ";`:

```text
            // (ii) a draft of the PILOT over pb-01-base with the manual structure of A at 1: the pilot admits it, and now the barrier stops it.
```

Pasa a:

```text
            // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): (ii) was a draft of the PILOT (its Prepare and ComposeForWrite, retired)
            // and goes; the barrier over the full model, (i), stays. (owner-pbt4d-cat4d-decisions.md, CA-T4d M-01): (iii), the designs of the
            // corpus, was a draft of the pilot too, and passes now as drafts of the FULL model through PrepareFull, the assembler and the barrier.

            // (iii) the designs of the corpus that the full model admits are not blocked: the same edit gives Ready.
            var fixtures = new[]
            {
                "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",
                "pb-10-sin-datum", "pb-11-sin-i41", "pb-12-poste-nulo", "pb-13-sideb-no-presente", "pb-14-peralte-heredado", "pb-15-sin-composite",
                "pb-31-v2-modulos",
            };
            foreach (var fixture in fixtures)
            {
                Paso(log, () =>
                {
                    var read = FullWriteKit.Read(IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal(fixture))));
                    var draft = FullWriteKit.DraftOf(read);
                    var scale = fixture == "pb-02-modulos" ? 2.5 : 2.0;
                    FullWriteKit.Edit(draft, state => state.Editable.AnnotationScale = scale);
                    var result = PrepareFull(log, draft, read);
                    log.Add(fixture + " admitido=" + read.Read.IsAdmitted + " prepare=" + Describe(result));
                });
            }
```

**`tests/RackCad.Tests/PushBackFullWritePrepareTests.cs`:** CT-PBT3B-06: sale la linea esperada de (ii), del piloto; las de (iii), el corpus, se quedan (CA-T4d M-01).

Desde la línea que contiene el texto de abajo hasta la anterior a la que contiene `Finish(log, expected);`:

```text
                "celda-no-cabe prepare=Invalid estado-igual-al-primer-bloqueante-de-Evaluate=True",
```

Pasa a:

```text
                "celda-no-cabe prepare=Invalid estado-igual-al-primer-bloqueante-de-Evaluate=True",
            };
            expected.AddRange(fixtures.Select(fixture => fixture + " admitido=True prepare=Ready"));
```

**`tests/RackCad.Tests/PushBackFullWriteRunTests.cs`:** CT-PBT3B-09 fija ComparePostFull; las dos lineas del piloto nombraban ComparePost, que se retira.

Desde la línea que contiene el texto de abajo hasta la anterior a la que contiene `Finish(`:

```text
            // The pilot, with a plan without erasures, gives what it gives today in the same cases.
```

Pasa a:

```text
            // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the comparison of the pilot (ComparePost, retired) goes; ComparePostFull stays.
```

**`tests/RackCad.Tests/PushBackFullWriteRunTests.cs`:** CT-PBT3B-09: salen las dos lineas esperadas del piloto.

El texto, una vez en el archivo:

```text
                    "erased-keys=[B2]",
                    "piloto-todo-igual <null>",
                    "piloto-escala-distinta True",
```

Pasa a:

```text
                    "erased-keys=[B2]",
```

**`tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`:** E-3: el Build del piloto se retira; lo de BuildCore y del modelo completo se queda.

El texto, una vez en el archivo:

```text
            var build = Src.BodyOf(plan, @"private\s+static\s+PushBackPilotPrepareResult\s+Build\s*\(");
```

Pasa a:

```text
            // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the Build of the pilot is retired; what E-3 protected of BuildCore and of
            // the full model stays.
```

**`tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`:** E-3: sale la regla del Build del piloto.

El texto, una vez en el archivo:

```text
            if (!Regex.IsMatch(build, @"\bBuildCore\s*\([^;]*\bfalse\b")) yield return "the pilot does not call BuildCore without the capacity to erase";
```

Pasa a:

```text
(nada)
```

**`tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`:** E-3: sale la regla de la escala del piloto.

El texto, una vez en el archivo:

```text
            if (!build.Contains("input.CurrentDraftState.Inputs.Annotations.AnnotationScale", StringComparison.Ordinal)) yield return "the pilot does not take the scale of its current state";
```

Pasa a:

```text
(nada)
```

**`tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`:** E-5 y E-6: el Execute del piloto se retira; las reglas de M-01 sobre ExecuteFull y Execution<TState> se quedan.

El texto, una vez en el archivo:

```text
                ("Execute", @"public\s+static\s+DraftApplyOutcome<PushBackPilotDraftState>\s+Execute\s*\("),
```

Pasa a:

```text
                // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the Execute of the pilot is retired; ExecuteFull stays.
```

**`tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`:** E-3 y la rama muerta de BuildCore: salen las reglas de allowErase y de ViewUnavailable; los borrados siguen condicionados a una lateral obsoleta.

El texto, una vez en el archivo:

```text
            if (!core.Contains("PushBackViewDispositionKind.ObsoleteLateral && allowErase", StringComparison.Ordinal)) yield return "the erasures are not conditioned to an obsolete lateral and to a model that admits erasing";
            // The pilot keeps its text for a view that is not available.
            if (!core.Contains("ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix", StringComparison.Ordinal)) yield return "the pilot does not keep ViewUnavailablePrefix";
            if (!Regex.IsMatch(full, @"\bBuildCore\s*\([^;]*\btrue\b")) yield return "the full model does not call BuildCore with the capacity to erase";
```

Pasa a:

```text
            // I-64 F4B PB-T4d (owner-pbt4d-deadcode-27-decisions.md, pregunta 1): the mode of the pilot that never erases (allowErase) is retired
            // with the text of BuildCore for a view that is not available; the erasures stay conditioned to an obsolete lateral.
            if (!core.Contains("disposition == PushBackViewDispositionKind.ObsoleteLateral", StringComparison.Ordinal)) yield return "the erasures are not conditioned to an obsolete lateral";
```

**`tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs`:** E-3 y la rama muerta de BuildCore: las dos comprobaciones directas del texto, con el de los borrados que se queda.

El texto, una vez en el archivo:

```text
            Assert.Contains("PushBackViewDispositionKind.ObsoleteLateral && allowErase", plan);
            Assert.Contains("ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix", plan);
```

Pasa a:

```text
            Assert.Contains("disposition == PushBackViewDispositionKind.ObsoleteLateral", plan);
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`:** H-06 (WPPG): ComposeForWrite se retira; la propiedad sigue sobre ForFullWrite, la composicion del modelo completo.

El texto, una vez en el archivo:

```text
        var callers = product
            .Where(f => !f.Path.EndsWith("/PushBackPilotMaterializer.cs", StringComparison.Ordinal)
                        && Regex.IsMatch(f.Code, @"\bPushBackPilotMaterializer\s*\.\s*ComposeForWrite\s*\("))
            .ToArray();
        if (callers.Length != 1 || callers[0].Path != PlanPath) yield return "ComposeForWrite has callers other than the planner: " + string.Join(", ", callers.Select(c => c.Path));
        foreach (var (_, code) in callers.Where(c => c.Path == PlanPath))
        {
            var compare = code.IndexOf(".CompareWith(", StringComparison.Ordinal);
            var compose = code.IndexOf("PushBackPilotMaterializer.ComposeForWrite(", StringComparison.Ordinal);
            if (compare < 0 || compose < compare) yield return "ComposeForWrite is called before the comparison of the base";
            if (Regex.Matches(code, @"\bPushBackPilotMaterializer\s*\.\s*ComposeForWrite\s*\(").Count != 1) yield return "ComposeForWrite is called more than once";
        }
```

Pasa a:

```text
        // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): ComposeForWrite is retired; H-06 holds for the composition that stays,
        // PushBackFullComposition.ForFullWrite.
        var callers = product
            .Where(f => !f.Path.EndsWith("/PushBackFullComposition.cs", StringComparison.Ordinal)
                        && Regex.IsMatch(f.Code, @"\bPushBackFullComposition\s*\.\s*ForFullWrite\s*\("))
            .ToArray();
        if (callers.Length != 1 || callers[0].Path != PlanPath) yield return "ForFullWrite has callers other than the planner: " + string.Join(", ", callers.Select(c => c.Path));
        foreach (var (_, code) in callers.Where(c => c.Path == PlanPath))
        {
            var compare = code.IndexOf(".CompareWith(", StringComparison.Ordinal);
            var compose = code.IndexOf("PushBackFullComposition.ForFullWrite(", StringComparison.Ordinal);
            if (compare < 0 || compose < compare) yield return "ForFullWrite is called before the comparison of the base";
            if (Regex.Matches(code, @"\bPushBackFullComposition\s*\.\s*ForFullWrite\s*\(").Count != 1) yield return "ForFullWrite is called more than once";
        }
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`:** H-06 (WPPG), su autoprueba: el llamador inyectado es de ForFullWrite.

El texto, una vez en el archivo:

```text
        Assert.NotEmpty(AfterMatchViolations(product.Append(("src/RackCad.Application/Workspace/Other.cs", "var c = PushBackPilotMaterializer.ComposeForWrite(a, b, catalog);"))));
```

Pasa a:

```text
        Assert.NotEmpty(AfterMatchViolations(product.Append(("src/RackCad.Application/Workspace/Other.cs", "var c = PushBackFullComposition.ForFullWrite(a, b, catalog);"))));
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`:** H-06 (WPPG), su autoprueba: la composicion inyectada antes de la comparacion es ForFullWrite.

El texto, una vez en el archivo:

```text
"var early = PushBackPilotMaterializer.ComposeForWrite(i, c, catalog); var comparison = input.StaleBase.CompareWith(reread.StaleBase);"
```

Pasa a:

```text
"var early = PushBackFullComposition.ForFullWrite(i, c, catalog); var comparison = input.StaleBase.CompareWith(reread.StaleBase);"
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`:** PEG, HostViolations: la regla del adaptador estatico pasa a la del adaptador completo, como en su copia (GUARD_EDITS de PB-T4b); las seis autopruebas de la lista vuelven a discriminar (paquete, seccion 4.2: «PB-T4d (C-7) las sustituye»).

El texto, una vez en el archivo:

```text
        if (Regex.IsMatch(host, @"private\s+(?:readonly\s+)?PushBackPilotFieldAdapter\s+_\w+")) yield return "the adapter of the pilot is a field of instance";
        if (Src.Count(host, @"private\s+static\s+readonly\s+PushBackPilotFieldAdapter\s+\w+") != 1) yield return "the adapter of the pilot is not the one static field";
```

Pasa a:

```text
        if (Regex.IsMatch(host, @"private\s+(?:readonly\s+)?PushBackFullFieldAdapter\s+_\w+")) yield return "the adapter of the field is a field of instance";
        if (Src.Count(host, @"private\s+static\s+readonly\s+PushBackFullFieldAdapter\s+\w+") != 1) yield return "the adapter of the field is not the one static field";
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`:** M-04 (PEG:591-598): el Replace busca FindPilotDraft con la firma del borrador completo.

El texto, una vez en el archivo:

```text
        var host = Host().Replace("private DraftSession<PushBackPilotDraftState>? FindPilotDraft", extra + "private DraftSession<PushBackPilotDraftState>? FindPilotDraft", StringComparison.Ordinal);
```

Pasa a:

```text
        var host = Host().Replace("private DraftSession<PushBackFullDraftState>? FindPilotDraft", extra + "private DraftSession<PushBackFullDraftState>? FindPilotDraft", StringComparison.Ordinal);
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`:** M-04 (PEG:916-920, su ayudante WriteViolations, PEG:906): FindPilotDraft con la firma del borrador completo.

El texto, una vez en el archivo:

```text
        foreach (var helper in new[] { @"private\s+DraftSession<PushBackPilotDraftState>\?\s+FindPilotDraft\b",
```

Pasa a:

```text
        foreach (var helper in new[] { @"private\s+DraftSession<PushBackFullDraftState>\?\s+FindPilotDraft\b",
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardFull.cs`:** M-04 (WorkspacePilotEditGuardFull.cs:255-258): el ayudante de la copia, con la firma del borrador completo.

El texto, una vez en el archivo:

```text
        foreach (var helper in new[] { @"private\s+DraftSession<PushBackPilotDraftState>\?\s+FindPilotDraft\b",
```

Pasa a:

```text
        foreach (var helper in new[] { @"private\s+DraftSession<PushBackFullDraftState>\?\s+FindPilotDraft\b",
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs`:** la prueba lenta (OWNER-05): su bucle se acota a cada 97.o toque del catalogo y al ultimo; las aserciones no cambian.

El texto, una vez en el archivo:

```text
        var failures = new List<PushBackPilotPrepareResult>();
        for (var allowed = 0; allowed < total; allowed++)
        {
```

Pasa a:

```text
        // I-64 F4B PB-T4d (OWNER-05; owner-pbt4d-list-decisions.md, pregunta 3; D-S14): the loop is bounded to every 97th touch and the
        // last one, instead of every touch (4 936 PREPARE, about 6.5 minutes); the failures of the composition (a) and of the builder (d)
        // are among them, and the assertions do not change.
        var failures = new List<PushBackPilotPrepareResult>();
        foreach (var allowed in Enumerable.Range(0, total).Where(touch => touch % 97 == 0 || touch == total - 1))
        {
```

**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs`:** Ownership: el nucleo del borrador; se quedan las filas box de las ocho Theory.

El texto, una vez en el archivo:

```text
    public static IEnumerable<object[]> Kinds() => new[] { new object[] { "box" }, new object[] { "pilot" } };
```

Pasa a:

```text
    // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the rows of the pilot go (its comparer is retired); the ones of the
    // mutable test state, which test the ownership boundary of the draft core, stay.
    public static IEnumerable<object[]> Kinds() => new[] { new object[] { "box" } };
```

**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs`:** Ownership: Both solo con el estado de prueba.

El texto, una vez en el archivo:

```text
        if (kind == "box") scenario(new Ownership<DraftBox>(OwnershipKits.Box()));
        else scenario(new Ownership<PushBackPilotDraftState>(OwnershipKits.Pilot()));
```

Pasa a:

```text
        Assert.Equal("box", kind);
        scenario(new Ownership<DraftBox>(OwnershipKits.Box()));
```

**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs`:** Ownership: salen las dos filas del piloto de la Theory de POST.

El texto, una vez en el archivo:

```text
    [InlineData("box", false)]
    [InlineData("box", true)]
    [InlineData("pilot", false)]
    [InlineData("pilot", true)]
```

Pasa a:

```text
    [InlineData("box", false)]
    [InlineData("box", true)]
```

**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`:** StaleBase: el nucleo del borrador (Check now de un borrador Unknown); el borrador nace de la lectura completa.

El texto, una vez en el archivo:

```text
    private static DraftSession<PushBackPilotDraftState> UnknownAfterACommitThatThrew(PushBackPilotIntentResult first)
    {
        Assert.True(first.Read.IsAdmitted);
        var begun = new DraftRegistry().Begin(DraftKit.S1, "R1", first.Read, PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        var draft = begun.Draft!;
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"));
```

Pasa a:

```text
    // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the draft is born from the read of the FULL model of the same scan (the
    // comparer of the pilot is retired); the base that Check now compares is the same, and the rereads are still the ones of the reader.
    private static DraftSession<PushBackFullDraftState> UnknownAfterACommitThatThrew(PushBackPilotIntentScan scan)
    {
        var first = PushBackFullDraftReader.Read(scan, PilotKit.Catalog);
        Assert.True(first.Read.IsAdmitted);
        var begun = new DraftRegistry().Begin(DraftKit.S1, "R1", first.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        var draft = begun.Draft!;
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(DraftApplyOutcome<PushBackFullDraftState>.CommitThrew("the commit threw"));
```

**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`:** StaleBase: la directiva del modelo completo.

El texto, una vez en el archivo:

```text
using RackCad.Application.Systems.PushBack.Pilot;
```

Pasa a:

```text
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
```

**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs`:** Materialize se queda (lo usa el lector del modelo completo); su comparacion con la carga de la ventana pasa del comparador del piloto al diseno armado.

El texto, una vez en el archivo:

```text
        Assert.True(Comparer.Equals(byHand, materialized));
```

Pasa a:

```text
        // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the comparer of the pilot is retired; the two states are compared by
        // the design that the product assembles from them.
        Assert.Equal(
            PilotKit.Persist(new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(byHand.Editor, byHand.Inputs)),
            PilotKit.Persist(new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(materialized.Editor, materialized.Inputs)));
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`:** WPUR: HappyCalls, Boom, Cause y NamedScan quedan sin uso, y los titulos de sus secciones y el comentario de la n.o 42 sin prueba (CA-T4d M-03).

Desde la línea que contiene el texto de abajo hasta la anterior a la que contiene `// M-06 (d): the names of the views of a composite rack carry the side of each cut. The pilot refuses a composite rack (its reader says it is`:

```text
    private static readonly string[] HappyCalls =
```

Pasa a:

```text
    // I-64 F4B PB-T4d (CA-T4d M-03): the tests of the run of the pilot go (29 of them have their substitute over the full model in
    // PushBackRetirePilotRunTests), and with them their helpers and the titles of their sections; what stays is the rule of the names of a
    // composite rack, over a plan built by hand.
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`:** WPUR: el resumen de la clase describia la corrida del piloto, que se retira (recomprobacion 1 de CA-T4d, M-12 a; solo comentarios).

El texto, una vez en el archivo:

```text
// I-64 F4-T4B-MUTATE-POST (INV-F4T4B-01, 02, 03, 05, 06, 10, 16): the pure and TOTAL run of the update of the pilot, with the real draft
// session, the real planner and the real reader, and a double of the port that simulates the drawing. The classification is the table of
// D-13 by frontier, with the three rows of the abort, the row of the outer release, the precedence and its two declared exceptions.
```

Pasa a:

```text
// I-64 F4-T4B-MUTATE-POST (INV-F4T4B-01, 02, 03, 05, 06, 10, 16) tested here the run of the update of the pilot, which PB-T4d retires.
// I-64 F4B PB-T4d (recomprobacion 1 de CA-T4d, M-12): what stays is the rule of the names of the views of a composite rack (DesiredNamesOf), over a plan
// built by hand; the run itself is tested over the full model in PushBackRetirePilotRunTests.
```

**`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`:** WPUR: los comentarios de la n.o 48 y la n.o 49, que se quedaban sin su prueba (las dos van sustituidas en PushBackRetirePilotRunTests; recomprobacion 1 de CA-T4d, M-12 a; solo comentarios).

El texto, una vez en el archivo:

```text
    }

    // The system of the outgoing design, rebuilt apart from the text of a member of the plan with the same resolver as PREPARE.

    // I-64 F4-T4-HOST-C1 (D-C1-01, INV-C1-01, test 1 of D-C1-09): the system that the writer redraws carries the rack name of the envelope of ITS
    // member, raw, because the label of the name of the views is drawn only from it. Everything runs over the text kits of the plan: the members
    // are only deserialized, nothing is drawn.
}
```

Pasa a:

```text
    }
}
```

**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`:** las 4 llamadas `UnknownAfterACommitThatThrew(IntentKit.Read(IntentKit.TwoViews()))` pasan a `UnknownAfterACommitThatThrew(IntentKit.TwoViews())`.

### Los ayudantes sin uso tras la retirada (CA-T4d M-03; `pbt4d_test_edits_v54.py`, `UNUSED` y `DELETE_FILES`)

Después de las ediciones, `v53/unused_v53.py` quita cada uno (su declaración, sus `///`, sus atributos y el `//` pegado encima, y una línea en blanco de las dos que la rodean); los de `edicion` los quita la última edición de WPUR. Los tres archivos, en bloques DELETE.

- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`: `HappyCalls` (edicion)
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`: `Boom` (edicion)
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`: `Cause` (edicion)
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`: `NamedScan` (edicion)
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs`: `AssertHeld` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs`: `Held` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs`: `YetAnotherBase` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs`: `WithExtra` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs`: `DraftHeld` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs`: `Hold` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs`: `ComposeForWriteViolations` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs`: `MaterializerPath` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs`: `ScalePath` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs`: `Variant` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs`: `CanonicalCorpus` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs`: `LegacyCorpus` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs`: `TextCorpus` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs`: `IsContainer`, la declaración n.º 2 del archivo (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs`: `ScaleCases` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs`: `Cultures` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs`: `PilotRich` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs`: `InCulture` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs`: `Texts` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs`: `Cultures` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`: `DesignNumber` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`: `Diff` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`: `Fixture` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`: `LoadInWindow` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs`: `Mutator` (quitar)
- `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs`: `Held` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`: `WindowResult` (quitar)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`: `Persist` (quitar)
- `tests/RackCad.UI.Tests/PushBackFieldValidationConcordanceTests.cs`: **se borra el archivo** (su unica prueba (CT-PBT2B-10) se borra: la clase queda vacia)
- `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionPilotParityTests.cs`: **se borra el archivo** (sus dos pruebas (CT-PBTC-06 y CT-PBTC-10) se borran: la clase queda vacia)
- `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs`: **se borra el archivo** (sus pruebas se borran: la clase queda con solo ayudantes sin uso (Canonical, Designs, RepoRoot y Fixture))

### Los `Skip`

- `tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs`:572 (`CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList`): `[Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]`
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`:81 (`TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage`): `[Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]`
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`:194 (`TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread`): `[Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]`

### `tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs` (`NewFileSha` `0d64806a53b36ea0719bc7647dcfd3c15e8169a66a4494517097a82ce12fadb3`)

```csharp
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4B PB-T4d — CT-PBT4D-01 (D-PBT4-10; D-S11): the code of the pilot that the panel no longer uses is retired, and what the full
/// model uses stays. Over the text of src/ without comments: the two files that are deleted do not exist, none of the eleven retired
/// declarations is declared in its file (the last two, the mode of BuildCore that never erases and its text for a view that is not
/// available, by owner-pbt4d-deadcode-27-decisions.md, pregunta 1), no file of src/ names what is retired, and the seven pieces that stay
/// are there once. It fails
/// on the RED (src/ still has the pilot) and passes on the GREEN. Its self-tests run the same reading over text injected in memory.
/// </summary>
public sealed class PushBackRetirePilotGuardTests
{
    private const string Pilot = "src/RackCad.Application/Systems/PushBack/Pilot/";

    private static readonly (string Name, string File)[] RetiredFiles =
    {
        ("PushBackPilotStateComparer.cs", Pilot + "PushBackPilotStateComparer.cs"),
        ("PushBackPilotFieldAdapter.cs", Pilot + "PushBackPilotFieldAdapter.cs"),
    };

    // The retired declarations: a name, its file and the expression of its declaration.
    private static readonly (string Name, string File, string Pattern, string Injected)[] Retired =
    {
        ("PushBackPilotMaterializer.ComposeForWrite", Pilot + "PushBackPilotMaterializer.cs", @"\bComposeForWrite\s*\(",
            "public static object ComposeForWrite(object initial, object current, object catalog) => null;"),
        ("PushBackPilotMaterializer.KeepPilotEditsOn", Pilot + "PushBackPilotMaterializer.cs", @"\bKeepPilotEditsOn\s*\(",
            "public static object KeepPilotEditsOn(object newInitial, object current) => null;"),
        ("PushBackPilotComposition", Pilot + "PushBackPilotMaterializer.cs", @"\bclass\s+PushBackPilotComposition\b",
            "public sealed class PushBackPilotComposition { }"),
        ("PushBackPilotPrepareInput", Pilot + "PushBackPilotUpdatePlan.cs", @"\bclass\s+PushBackPilotPrepareInput\b",
            "public sealed class PushBackPilotPrepareInput { }"),
        ("PushBackPilotUpdatePlanner.Prepare", Pilot + "PushBackPilotUpdatePlan.cs", @"\bpublic\s+static\s+PushBackPilotPrepareResult\s+Prepare\s*\(",
            "public static PushBackPilotPrepareResult Prepare(object input, object reread, object catalog) => null;"),
        ("PushBackPilotUpdatePlanner.Build", Pilot + "PushBackPilotUpdatePlan.cs", @"\bprivate\s+static\s+PushBackPilotPrepareResult\s+Build\s*\(",
            "private static PushBackPilotPrepareResult Build(object input, object reread, object catalog) => null;"),
        ("PushBackPilotUpdateRun.Precheck(piloto)", Pilot + "PushBackPilotUpdateRun.cs", @"\bPrecheck\s*\(\s*DraftSession\s*<\s*PushBackPilotDraftState\s*>",
            "public static PushBackPilotPrecheck Precheck(DraftSession<PushBackPilotDraftState> draft) => null;"),
        ("PushBackPilotUpdateRun.Execute", Pilot + "PushBackPilotUpdateRun.cs", @"\bDraftApplyOutcome\s*<\s*PushBackPilotDraftState\s*>\s+Execute\s*\(",
            "public static DraftApplyOutcome<PushBackPilotDraftState> Execute(object draft, object request, object port) => null;"),
        ("PushBackPilotUpdateRun.ComparePost", Pilot + "PushBackPilotUpdateRun.cs", @"\bComparePost\s*\(",
            "public static string ComparePost(object plan, object postRead) => null;"),
        ("PushBackPilotUpdatePlanner.BuildCore(allowErase)", Pilot + "PushBackPilotUpdatePlan.cs", @"\ballowErase\b",
            "private static readonly bool allowErase = false;"),
        ("PushBackPilotUpdatePlanner.ViewUnavailable", Pilot + "PushBackPilotUpdatePlan.cs", @"\bViewUnavailable(?:Prefix|Suffix)\b",
            "public const string ViewUnavailablePrefix = \"\";"),
    };

    // A use of what is retired, as code, anywhere in src/.
    private static readonly Regex Uses = new Regex(
        @"\b(?:PushBackPilotStateComparer|PushBackPilotFieldAdapter|ComposeForWrite|KeepPilotEditsOn|PushBackPilotPrepareInput|PushBackPilotComposition|ComparePost)\b"
        + @"|\bPushBackPilotUpdatePlanner\s*\.\s*Prepare\s*\(|\bPushBackPilotUpdateRun\s*\.\s*Execute\s*\(");

    // What stays, because the full model uses it (D-PBT4-10, R-17): once each.
    private static readonly (string Name, string File, string Pattern)[] Kept =
    {
        ("PushBackPilotIntentReader.Read", Pilot + "PushBackPilotIntentReader.cs", @"\bpublic\s+static\s+PushBackPilotIntentResult\s+Read\s*\("),
        ("PushBackPilotMaterializer.Materialize", Pilot + "PushBackPilotMaterializer.cs", @"\bpublic\s+static\s+PushBackPilotMaterialization\s+Materialize\s*\("),
        ("PushBackPilotDraftState", Pilot + "PushBackPilotDraftState.cs", @"\bclass\s+PushBackPilotDraftState\b"),
        ("PushBackPilotUpdatePlanner.PrepareFull", Pilot + "PushBackPilotUpdatePlan.cs", @"\bpublic\s+static\s+PushBackPilotPrepareResult\s+PrepareFull\s*\("),
        ("PushBackPilotUpdateRun.ExecuteFull", Pilot + "PushBackPilotUpdateRun.cs", @"\bDraftApplyOutcome\s*<\s*PushBackFullDraftState\s*>\s+ExecuteFull\s*\("),
        ("PushBackPilotUpdateRun.Precheck(completo)", Pilot + "PushBackPilotUpdateRun.cs", @"\bPrecheck\s*\(\s*DraftSession\s*<\s*PushBackFullDraftState\s*>"),
        ("PushBackPilotUpdateRun.ComparePostFull", Pilot + "PushBackPilotUpdateRun.cs", @"\bpublic\s+static\s+string\?\s+ComparePostFull\s*\("),
    };

    /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
    private static T Paso<T>(List<string> log, Func<T> step)
    {
        try
        {
            return step();
        }
        catch (Exception error)
        {
            log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
            return default!;
        }
    }

    /// <summary>The lines of the reading, over a source given as a function: the code of a file without comments, or null when it does not exist.</summary>
    private static List<string> Lines(Func<string, string?> code, IReadOnlyList<(string Path, string Code)> product)
    {
        var lines = new List<string>();
        foreach (var (name, file) in RetiredFiles) lines.Add("retirado-archivo " + name + " existe=" + (code(file) != null));
        foreach (var (name, file, pattern, _) in Retired) lines.Add("retirado " + name + "=" + Src.Count(code(file) ?? string.Empty, pattern));
        var naming = product.Where(f => Uses.IsMatch(f.Code)).Select(f => f.Path.Substring(f.Path.LastIndexOf('/') + 1)).ToArray();
        lines.Add("src-nombra-lo-retirado archivos=[" + string.Join(",", naming) + "]");
        foreach (var (name, file, pattern) in Kept) lines.Add("se-queda " + name + "=" + Src.Count(code(file) ?? string.Empty, pattern));
        return lines;
    }

    private static string? Read(string file) => Src.Exists(file) ? Src.Code(file) : null;

    [Fact]
    public void CT_PBT4D_01_TheCodeOfThePilotThatThePanelNoLongerUsesIsRetiredAndWhatTheFullModelUsesStays()
    {
        var log = new List<string>();
        var product = Paso(log, () => Src.AllProduct()) ?? Array.Empty<(string, string)>();
        var real = Paso(log, () => Lines(Read, product)) ?? new List<string>();
        log.AddRange(real);

        // The self-tests: each one makes the reading see what is retired again, in memory, and the lines must differ from the real ones.
        foreach (var (name, file) in RetiredFiles)
        {
            var injected = Paso(log, () => Lines(f => f == file ? "public sealed class Injected { }" : Read(f), product));
            log.Add("autoprueba-archivo-" + name + "=" + (injected != null && !injected.SequenceEqual(real) ? "falla" : "pasa"));
        }

        foreach (var (name, file, _, text) in Retired)
        {
            var injected = Paso(log, () => Lines(f => f == file ? (Read(f) ?? string.Empty) + "\n" + text + "\n" : Read(f), product));
            log.Add("autoprueba-" + name + "=" + (injected != null && !injected.SequenceEqual(real) ? "falla" : "pasa"));
        }

        var withUse = product.Select(f => f.Path.EndsWith("/WorkspaceHostCore.cs", StringComparison.Ordinal) ? (f.Path, f.Code + "\nvar c = new PushBackPilotStateComparer();\n") : f).ToList();
        var used = Paso(log, () => Lines(Read, withUse));
        log.Add("autoprueba-uso-en-WorkspaceHostCore.cs=" + (used != null && !used.SequenceEqual(real) ? "falla" : "pasa"));

        var expected = new List<string>
        {
            "retirado-archivo PushBackPilotStateComparer.cs existe=False",
            "retirado-archivo PushBackPilotFieldAdapter.cs existe=False",
            "retirado PushBackPilotMaterializer.ComposeForWrite=0",
            "retirado PushBackPilotMaterializer.KeepPilotEditsOn=0",
            "retirado PushBackPilotComposition=0",
            "retirado PushBackPilotPrepareInput=0",
            "retirado PushBackPilotUpdatePlanner.Prepare=0",
            "retirado PushBackPilotUpdatePlanner.Build=0",
            "retirado PushBackPilotUpdateRun.Precheck(piloto)=0",
            "retirado PushBackPilotUpdateRun.Execute=0",
            "retirado PushBackPilotUpdateRun.ComparePost=0",
            "retirado PushBackPilotUpdatePlanner.BuildCore(allowErase)=0",
            "retirado PushBackPilotUpdatePlanner.ViewUnavailable=0",
            "src-nombra-lo-retirado archivos=[]",
            "se-queda PushBackPilotIntentReader.Read=1",
            "se-queda PushBackPilotMaterializer.Materialize=1",
            "se-queda PushBackPilotDraftState=1",
            "se-queda PushBackPilotUpdatePlanner.PrepareFull=1",
            "se-queda PushBackPilotUpdateRun.ExecuteFull=1",
            "se-queda PushBackPilotUpdateRun.Precheck(completo)=1",
            "se-queda PushBackPilotUpdateRun.ComparePostFull=1",
            "autoprueba-archivo-PushBackPilotStateComparer.cs=falla",
            "autoprueba-archivo-PushBackPilotFieldAdapter.cs=falla",
            "autoprueba-PushBackPilotMaterializer.ComposeForWrite=falla",
            "autoprueba-PushBackPilotMaterializer.KeepPilotEditsOn=falla",
            "autoprueba-PushBackPilotComposition=falla",
            "autoprueba-PushBackPilotPrepareInput=falla",
            "autoprueba-PushBackPilotUpdatePlanner.Prepare=falla",
            "autoprueba-PushBackPilotUpdatePlanner.Build=falla",
            "autoprueba-PushBackPilotUpdateRun.Precheck(piloto)=falla",
            "autoprueba-PushBackPilotUpdateRun.Execute=falla",
            "autoprueba-PushBackPilotUpdateRun.ComparePost=falla",
            "autoprueba-PushBackPilotUpdatePlanner.BuildCore(allowErase)=falla",
            "autoprueba-PushBackPilotUpdatePlanner.ViewUnavailable=falla",
            "autoprueba-uso-en-WorkspaceHostCore.cs=falla",
        };
        Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
    }
}
```

### `tests/RackCad.Tests/PushBackRetirePilotCensusGuardTests.cs` (`NewFileSha` `1874c47811ddda428745a1f5566f1e7290b31fa2a2e65b07c2ff52059f863f20`)

```csharp
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4B PB-T4d — CT-PBT4D-02 (D-S11): no file of tests/ names what the retirement of the pilot removes, as code. The text of every
/// .cs file of the two test projects is read without comments, without string and character literals; a use of a retired name there would
/// stop compiling on the GREEN, and the GREEN can no longer touch tests/ (AUTOMATION_PLAN 16.8). It passes on the RED and on the GREEN; its
/// self-tests inject each name, as code, in the text of one file in memory.
/// </summary>
public sealed class PushBackRetirePilotCensusGuardTests
{
    private const string Sample = "tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs";

    private static readonly (string Name, string Pattern, string Injected)[] RetiredUses =
    {
        ("PushBackPilotStateComparer", @"\bPushBackPilotStateComparer\b", "var x = new PushBackPilotStateComparer();"),
        ("PushBackPilotFieldAdapter", @"\bPushBackPilotFieldAdapter\b", "var x = new PushBackPilotFieldAdapter();"),
        ("ComposeForWrite", @"\bComposeForWrite\b", "var x = PushBackPilotMaterializer.ComposeForWrite(a, b, c);"),
        ("KeepPilotEditsOn", @"\bKeepPilotEditsOn\b", "var x = PushBackPilotMaterializer.KeepPilotEditsOn(a, b);"),
        ("PushBackPilotPrepareInput", @"\bPushBackPilotPrepareInput\b", "var x = PushBackPilotPrepareInput.From(d);"),
        ("PushBackPilotComposition", @"\bPushBackPilotComposition\b", "PushBackPilotComposition x = null;"),
        ("ComparePost", @"\bComparePost\b", "var x = PushBackPilotUpdateRun.ComparePost(p, r);"),
        ("PushBackPilotUpdatePlanner.Prepare", @"\bPushBackPilotUpdatePlanner\s*\.\s*Prepare\s*\(", "var x = PushBackPilotUpdatePlanner.Prepare(i, r, c);"),
        ("PushBackPilotUpdateRun.Execute", @"\bPushBackPilotUpdateRun\s*\.\s*Execute\s*\(", "var x = PushBackPilotUpdateRun.Execute(d, r, p);"),
        // owner-pbt4d-deadcode-27-decisions.md, pregunta 1: the text of BuildCore for a view that is not available goes with the mode that never erases.
        ("ViewUnavailable", @"\bViewUnavailable(?:Prefix|Suffix)\b", "var x = PushBackPilotUpdatePlanner.ViewUnavailablePrefix;"),
    };

    /// <summary>The code of a C# text without comments and without the text of string and character literals (regular, verbatim,
    /// interpolated and raw), each replaced by a space.</summary>
    private static string Mask(string text)
    {
        var s = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
            {
                while (i < text.Length && text[i] != '\n') i++;
                continue;
            }

            if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                var end = text.IndexOf("*/", i + 2, StringComparison.Ordinal);
                i = end < 0 ? text.Length : end + 2;
                s.Append(' ');
                continue;
            }

            if (c == '"' && i + 2 < text.Length && text[i + 1] == '"' && text[i + 2] == '"')
            {
                var run = 0;
                while (i + run < text.Length && text[i + run] == '"') run++;
                var close = text.IndexOf(new string('"', run), i + run, StringComparison.Ordinal);
                i = close < 0 ? text.Length : close + run;
                s.Append(' ');
                continue;
            }

            var verbatim = (c == '@' || c == '$') && i + 1 < text.Length
                && (text[i + 1] == '"' || ((text[i + 1] == '@' || text[i + 1] == '$') && i + 2 < text.Length && text[i + 2] == '"'))
                && (c == '@' || text[i + 1] == '@');
            if (verbatim)
            {
                i = text.IndexOf('"', i) + 1;
                while (i < text.Length)
                {
                    if (text[i] == '"' && i + 1 < text.Length && text[i + 1] == '"') { i += 2; continue; }
                    if (text[i] == '"') { i++; break; }
                    i++;
                }

                s.Append(' ');
                continue;
            }

            if (c == '"' || c == '\'')
            {
                i++;
                while (i < text.Length && text[i] != c && text[i] != '\n')
                {
                    if (text[i] == '\\') i++;
                    i++;
                }

                i++;
                s.Append(' ');
                continue;
            }

            s.Append(c);
            i++;
        }

        return s.ToString();
    }

    private static IReadOnlyList<(string Path, string Code)> TestFiles()
    {
        var root = Src.RepoRoot();
        return new[] { "tests/RackCad.Tests", "tests/RackCad.UI.Tests" }
            .SelectMany(folder => Directory.GetFiles(Path.Combine(root, folder.Replace('/', Path.DirectorySeparatorChar)), "*.cs", SearchOption.AllDirectories))
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .Select(f => (Path.GetRelativePath(root, f).Replace(Path.DirectorySeparatorChar, '/'), Mask(File.ReadAllText(f))))
            .OrderBy(f => f.Item1, StringComparer.Ordinal)
            .ToList();
    }

    private static string Naming(IEnumerable<(string Path, string Code)> files)
    {
        var hits = files.SelectMany(f => RetiredUses.Where(r => Regex.IsMatch(f.Code, r.Pattern)).Select(r => f.Path.Substring(f.Path.LastIndexOf('/') + 1) + ":" + r.Name));
        return "[" + string.Join(",", hits) + "]";
    }

    [Fact]
    public void CT_PBT4D_02_NoFileOfTheTestsNamesWhatTheRetirementOfThePilotRemoves()
    {
        var log = new List<string>();
        var files = TestFiles();
        log.Add("tests archivos-revisados>0=" + (files.Count > 0) + " nombran-lo-retirado=" + Naming(files));

        var real = Naming(files);
        foreach (var (name, _, injected) in RetiredUses)
        {
            var mutated = files.Select(f => f.Path == Sample ? (f.Path, f.Code + "\n" + Mask(injected) + "\n") : f).ToList();
            log.Add("autoprueba-" + name + "-en-WorkspacePilotPrepareFull.cs=" + (Naming(mutated) != real ? "falla" : "pasa"));
        }

        var expected = new List<string>
        {
            "tests archivos-revisados>0=True nombran-lo-retirado=[]",
            "autoprueba-PushBackPilotStateComparer-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotFieldAdapter-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-ComposeForWrite-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-KeepPilotEditsOn-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotPrepareInput-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotComposition-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-ComparePost-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotUpdatePlanner.Prepare-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotUpdateRun.Execute-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-ViewUnavailable-en-WorkspacePilotPrepareFull.cs=falla",
        };
        Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
    }
}
```

### `tests/RackCad.Tests/PushBackRetirePilotRunTests.cs` (`NewFileSha` `3554df0d960f18ef6bb2b930f4ac7757e8ca7438ebf6a9eda8934ef9acf8fe69`)

```csharp
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md; owner-pbt4d-deadcode-27-decisions.md, pregunta 2; owner-pbt4d-cat4d-decisions.md,
/// pregunta 1): the substitutes, over the full model, of the 29 tests of WorkspacePilotUpdateRunTests that exercise lines or branches of code
/// that stays and that nothing else exercises, measured line by line and branch by branch: 27 of the generic run (Execution&lt;TState&gt;) and
/// of the plan, over ExecuteFull, and 2 of the rebuild of the system of a member (PushBackPilotSystemRebuild), over the plan of the full
/// PREPARE. The run of the pilot that they used as a vehicle is retired; the run, the planner and the reader are the real ones of the full
/// model, with the double of the port that simulates the drawing. Each test keeps the name and the assertions of its original, with the map
/// of PB-T4b (the kit of the copies, the full draft and ExecuteFull); the ones that do not fit the map say so. Of the other 20 tests of the
/// original class, 19 are deleted with their record and one stays in it.
/// </summary>
public class PushBackRetirePilotRunTests
{
    private static InvalidOperationException Boom(string message) => new(message);

    private static string Cause(DraftApplyOutcome<PushBackFullDraftState> outcome) => outcome.Cause ?? string.Empty;

    [Fact]
    public void ThereIsNoOriginWhenNoMutableMemberHasTheTextOfTheOriginOfTheDraft()
    {
        var setup = new FullUpdateSetup();
        // The origin of the draft is the text of A1; the drawing no longer has it in any member.
        var others = setup.Members.Select(m => m.Fact.DefinitionKey == "A1" ? IntentKit.Member("A1", FullPrepareKit.Frontal("pb-02-modulos")) : m).ToArray();
        setup.Port.PrepareDrawing = others;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.NoOriginReason, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    [Fact]
    public void SeveralMembersWithTheTextOfTheOriginAreAPreflightWithTheirOwnReasonAndNotTheMisleadingOneOfTheReader()
    {
        var setup = new FullUpdateSetup();
        var twin = IntentKit.Member("D4", setup.Scan.RawEnvelopes["A1"]);
        setup.Port.PrepareDrawing = setup.Members.Append(twin).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.SeveralOriginsReason, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
    }

    [Fact]
    public void ADifferenceOfTheBaseInPrepareIsAConflictWithItsDifferencesAndNothingIsImported()
    {
        var setup = new FullUpdateSetup();
        setup.Port.PrepareDrawing = setup.Members.Select(m => m.Fact.DefinitionKey == "B2" ? IntentKit.Member("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost(), "pb-02-modulos")) : m).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Contains(new DraftDifference("B2", DraftDifferenceKind.EnvelopeText), outcome.Differences);
        Assert.Null(outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    // I-64 F4B PB-T4d (owner-pbt4d-deadcode-27-decisions.md, pregunta 1): by hand. The full model erases a lateral that is not available, and
    // BuildCore no longer has a view that is not available (the mode of the pilot that never erases is retired with it); the Invalid of PREPARE
    // inside the run is the frontal of side B of a rack without side B, prepared as in CT-PBT3B-04 (ii), and nothing is imported.
    [Fact]
    public void AViewThatIsNotAvailableInTheOutgoingSystemIsAPreflightAndNothingIsImported()
    {
        var scan = IntentKit.Scan(FullPrepareKit.Rack, "A1", IntentKit.Member("A1", FullPrepareKit.Frontal("pb-20-ab")), IntentKit.Member("FB", FullPrepareKit.Frontal("pb-20-ab", 2)));
        var draft = FullWriteKit.DraftOf(FullWriteKit.Read(scan));
        FullWriteKit.Edit(draft, state => state.Editable.Editor.SetSideBPresent(false));
        var port = new PilotUpdateScopeDouble(FullWriteKit.ItemsOf(scan));
        Assert.True(draft.TryBeginApply().IsStarted);

        var outcome = PushBackPilotUpdateRun.ExecuteFull(draft, new PushBackPilotUpdateRequest(100, FullPrepareKit.Rack), port);

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.FrontalBOfOneWayPrefix + "FB" + PushBackPilotUpdatePlanner.FrontalBOfOneWaySuffix, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", port.Calls);
    }

    [Fact]
    public void ACleanDraftIsNothingToApplyAndAConflictDraftIsNotApplicableAndAPendingTextIsPendingTextEvenWithoutTheSlot()
    {
        var clean = new FullUpdateSetup();
        clean.Draft.CurrentDraftState.Editable.AnnotationScale = clean.Draft.InitialDraftState.Editable.AnnotationScale;
        Assert.Equal(DraftStatus.Clean, clean.Draft.Status);
        var nothing = PushBackPilotUpdateRun.ExecuteFull(clean.Draft, clean.Request, clean.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, nothing.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NothingToApplyReason, nothing.Cause);

        var pending = new FullUpdateSetup();
        pending.Draft.SetPendingText("AnnotationScale", "3");
        var typed = PushBackPilotUpdateRun.ExecuteFull(pending.Draft, pending.Request, pending.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, typed.Kind);
        Assert.Equal(PushBackPilotUpdateRun.PendingTextReason, typed.Cause);

        var conflict = new FullUpdateSetup();
        conflict.Draft.ConfirmBase(FullPrepareKit.BaseOf(("A1", FullPrepareKit.Frontal("pb-02-modulos"))));
        var notApplicable = PushBackPilotUpdateRun.ExecuteFull(conflict.Draft, conflict.Request, conflict.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, notApplicable.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NotApplicableReason, notApplicable.Cause);
    }

    [Fact]
    public void WithoutSurvivorsNothingIsImportedAndTheRunStops()
    {
        var setup = new FullUpdateSetup();
        setup.Port.PrepareDrawing = setup.Members.Select(m => new IntentItem(PilotUpdateScopeDouble.FactOf(m.Fact.DefinitionKey, m.Text!, layoutReferences: 0), m.Text)).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoSurvivorsReason, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
    }

    [Fact]
    public void AnImportThatFailsIsAPreflightWithItsCauseAndNoScopeOfMutationIsStarted()
    {
        var setup = new FullUpdateSetup();
        setup.Port.Import = PushBackPilotImportResult.Failed("no block library");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal("no block library", outcome.Cause);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    [Fact]
    public void AForeignScopeOpenIsRejectedAsAValueAndNothingIsRereadWrittenOrReleased()
    {
        var setup = new FullUpdateSetup();
        setup.Port.Rejection = PushBackPilotUpdateRun.ForeignScopeOpenCause;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal("TOP_TRANSACTION_OPEN", outcome.Cause);
        Assert.Equal(new[] { "Begin", "ReadForPrepare", "PrepareUnits", "BeginMutation", "Dispose:scope" }, setup.Port.Calls.ToArray());
    }

    [Fact]
    public void ADocumentThatChangedBetweenPrepareAndMutateIsAPreflightWithoutWrites()
    {
        var setup = new FullUpdateSetup();
        setup.Port.DocumentMatchesAtReread = false;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.DocumentMismatchReason, outcome.Cause);
        Assert.Empty(setup.Port.Written);
    }

    [Fact]
    public void ABlockThatIsNotTheOneThatWasPreparedIsAPreflightAndNothingIsWritten()
    {
        var setup = new FullUpdateSetup();
        setup.Port.PreparedBlockMatches = key => key != "B2";

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.DestinationChangedReason, outcome.Cause);
        Assert.Empty(setup.Port.Written);
        Assert.DoesNotContain("Seal", setup.Port.Calls);
    }

    [Fact]
    public void TheSurvivorsAndTheLockedLayersAreCheckedAgainInMutateAndNothingIsWritten()
    {
        var noSurvivors = new FullUpdateSetup();
        noSurvivors.Port.RereadDrawing = noSurvivors.Members.Select(m => new IntentItem(PilotUpdateScopeDouble.FactOf(m.Fact.DefinitionKey, m.Text!, layoutReferences: 0), m.Text)).ToArray();
        var first = noSurvivors.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, first.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoSurvivorsReason, first.Cause);
        Assert.Empty(noSurvivors.Port.Written);
        Assert.Contains("PrepareUnits", noSurvivors.Port.Calls);

        var locked = new FullUpdateSetup();
        locked.Port.LockedAtReread = new[] { "C3" };
        var second = locked.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, second.Kind);
        Assert.Equal(PushBackPilotUpdateRun.LockedLayerPrefix + "C3", second.Cause);
        Assert.Empty(locked.Port.Written);
        Assert.DoesNotContain("Seal", locked.Port.Calls);
    }

    [Fact]
    public void AbortThatFailsAfterAUnitStartedIsCommitThrewWithAbortFailedAndNoRollbackIsClaimed()
    {
        var setup = new FullUpdateSetup();
        setup.Port.ThrowAt["WriteUnit:B2"] = Boom("unit boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.AbortFailedPrefix, Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
        Assert.DoesNotContain("Seal", setup.Port.Calls);
        Assert.DoesNotContain("Post", setup.Port.Calls);

        var closed = setup.Draft.CompleteApply(outcome);
        Assert.Equal(DraftStatus.Unknown, setup.Draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, setup.Draft.UnknownOrigin);
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, closed.Kind);
    }

    [Fact]
    public void AReleaseThatFailsWithoutSealAndWithoutAUnitStartedIsNotAppliedFailureAndNotAnUncertainOutcome()
    {
        var setup = new FullUpdateSetup();
        setup.Port.ThrowAt["Reread"] = Boom("reread boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Contains("reread boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Empty(setup.Port.Written);
    }

    [Fact]
    public void ASealThatReturnsAndAReleaseThatThrowsGoesToPostAndTheReleaseIsAWarning()
    {
        var setup = new FullUpdateSetup();
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Contains(outcome.Warnings, w => w.Contains("release boom", StringComparison.Ordinal));
        Assert.Contains("Post", setup.Port.Calls);
        Assert.True(outcome.PostRead!.IsAdmitted);
    }

    [Fact]
    public void APostWithoutValuesOrWithoutItsRereadIsAppliedPostRereadFailed()
    {
        var withoutValues = new FullUpdateSetup();
        withoutValues.Port.NullPostValues = true;
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, withoutValues.Run().Kind);

        var withoutScan = new FullUpdateSetup();
        withoutScan.Port.NullPostScan = true;
        var outcome = withoutScan.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.PostRereadMissingReason, outcome.Cause);
    }

    [Fact]
    public void AForeignScopeOpenAtPostIsAppliedPostRereadFailedWithItsCauseAndTheWarnings()
    {
        var setup = new FullUpdateSetup();
        setup.Port.MissingPieces["A1"] = new[] { "PLACA_Y" };
        setup.Port.ForeignAtPost = true;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.ForeignScopeOpenCause, outcome.Cause);
        Assert.Single(outcome.Warnings);
        var closed = setup.Draft.CompleteApply(outcome);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, closed.Kind);
        Assert.Equal(DraftStatus.Unknown, setup.Draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, setup.Draft.UnknownOrigin);
        Assert.Single(setup.Draft.Warnings);
    }

    [Fact]
    public void AComparisonThatThrowsAfterTheSealIsAppliedPostRereadFailed()
    {
        var setup = new FullUpdateSetup();
        // The scan of POST carries a null fact: building the read of POST throws after the seal.
        setup.Port.PostOverride = request => new PushBackPilotPostValues(
            false,
            new PushBackPilotScanValues(true, new RackSiblingScanFact[] { null! }, new Dictionary<string, string>()),
            new Dictionary<string, string>(),
            null);

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.PostFailurePrefix, Cause(outcome), StringComparison.Ordinal);
    }

    [Fact]
    public void AnOuterReleaseThatThrowsAfterPostTurnsAppliedIntoAppliedWithWarningsAndKeepsTheOtherWarnings()
    {
        var plain = new FullUpdateSetup();
        plain.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var outcome = plain.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Contains(outcome.Warnings, w => w.Contains("outer boom", StringComparison.Ordinal));

        var warned = new FullUpdateSetup();
        warned.Port.MissingPieces["C3"] = new[] { "PIEZA_Z" };
        warned.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var second = warned.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, second.Kind);
        Assert.Equal(2, second.Warnings.Count);
        Assert.True(second.PostRead!.IsAdmitted);

        // Both end Clean: the warnings do not change the status of the draft.
        Assert.Equal(DraftStatus.Clean, StatusAfter(plain, outcome));
    }

    private static DraftStatus StatusAfter(FullUpdateSetup setup, DraftApplyOutcome<PushBackFullDraftState> outcome)
    {
        setup.Draft.CompleteApply(outcome);
        return setup.Draft.Status;
    }

    [Fact]
    public void AnOuterReleaseThatThrowsOutsidePostAddsItsCauseToTheClassificationThatWasAlreadyDecided()
    {
        var preflight = new FullUpdateSetup();
        preflight.Port.DocumentMatches = false;
        preflight.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var first = preflight.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, first.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.DocumentMismatchReason, Cause(first), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(first), StringComparison.Ordinal);

        var failure = new FullUpdateSetup();
        failure.Port.ThrowAt["PrepareUnits"] = Boom("import boom");
        failure.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var second = failure.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, second.Kind);
        Assert.Contains("import boom", Cause(second), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(second), StringComparison.Ordinal);

        var committed = new FullUpdateSetup();
        committed.Port.ThrowAt["Seal"] = Boom("seal boom");
        committed.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var third = committed.Run();
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, third.Kind);
        Assert.Contains("outer boom", Cause(third), StringComparison.Ordinal);

        var abort = new FullUpdateSetup();
        abort.Port.ThrowAt["WriteUnit:A1"] = Boom("unit boom");
        abort.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");
        abort.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var fourth = abort.Run();
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, fourth.Kind);
        Assert.Contains("abort boom", Cause(fourth), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(fourth), StringComparison.Ordinal);
    }

    [Fact]
    public void AnOuterReleaseThatThrowsOnAConflictIsDroppedBecauseAConflictCarriesNoCause()
    {
        var setup = new FullUpdateSetup();
        setup.Port.RereadDrawing = setup.Members.Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", FullPrepareKit.Planta("pb-02-modulos")) : m).ToArray();
        setup.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");

        var outcome = setup.Run();

        // The declared limitation of the package: the exception of the release is lost, the classification is not.
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Null(outcome.Cause);
    }

    [Fact]
    public void AConflictOrAPreflightOfMutateStaysWhatItIsEvenWhenTheReleaseOfTheScopeOfMutateThrows()
    {
        var conflict = new FullUpdateSetup();
        conflict.Port.RereadDrawing = conflict.Members.Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", FullPrepareKit.Planta("pb-02-modulos")) : m).ToArray();
        conflict.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");
        var first = conflict.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, first.Kind);
        Assert.NotEmpty(first.Differences);

        var preflight = new FullUpdateSetup();
        preflight.Port.LockedAtReread = new[] { "B2" };
        preflight.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");
        var second = preflight.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, second.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.LockedLayerPrefix + "B2", Cause(second), StringComparison.Ordinal);
        Assert.Contains("release boom", Cause(second), StringComparison.Ordinal);
        Assert.Empty(preflight.Port.Written);
    }

    [Fact]
    public void ASealThatThrewKeepsBeingCommitThrewBecauseOfTheSealEvenIfTheAbortThrowsToo()
    {
        var setup = new FullUpdateSetup();
        setup.Port.ThrowAt["Seal"] = Boom("seal boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.SealFailedPrefix, Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRunNeverThrowsForMissingArgumentsOrForAPortThatAnswersNothing()
    {
        var setup = new FullUpdateSetup();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.ExecuteFull(null!, setup.Request, setup.Port).Kind);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.ExecuteFull(setup.Draft, null!, setup.Port).Kind);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.ExecuteFull(setup.Draft, setup.Request, null!).Kind);

        var noScope = new FullUpdateSetup();
        noScope.Port.ReturnNullScope = true;
        var outcome = noScope.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoScopeReason, outcome.Cause);

        var noStart = new FullUpdateSetup();
        noStart.Port.NullMutationStart = true;
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, noStart.Run().Kind);
        Assert.Contains("Dispose:scope", noStart.Port.Calls);
    }

    private static PushBackPilotIntentScan NamedScan(string name) => FullPrepareKit.Scan(
        IntentKit.Member("A1", FullPrepareKit.Frontal(name: name)),
        IntentKit.Member("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost(), name: name)),
        IntentKit.Member("C3", FullPrepareKit.Planta(name: name)));

    [Fact]
    public void ANameThatDiffersAfterTheRenameIsAWarningAndACaseOnlyDifferenceIsNot()
    {
        var differs = new FullUpdateSetup(() => NamedScan("Mi rack"));
        differs.Port.NameAfterPost = (key, desired) => key == "C3" ? "Ocupado" : desired;
        var outcome = differs.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        var warning = Assert.Single(outcome.Warnings);
        Assert.Contains("C3", warning, StringComparison.Ordinal);

        var sameCase = new FullUpdateSetup(() => NamedScan("Mi rack"));
        sameCase.Port.NameAfterPost = (key, desired) => desired.ToUpperInvariant();
        Assert.Equal(DraftApplyOutcomeKind.Applied, sameCase.Run().Kind);
    }

    [Fact]
    public void TheMissingPiecesAndAFailedRegenerationAreWarningsAndTheStateStaysClean()
    {
        var setup = new FullUpdateSetup();
        setup.Port.MissingPieces["A1"] = new[] { "POSTE_Q", "PLACA_R" };
        setup.Port.RegenFailure = "InvalidOperationException: regen";

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Equal(3, outcome.Warnings.Count);
        Assert.Contains(outcome.Warnings, w => w.Contains("POSTE_Q", StringComparison.Ordinal) && w.Contains("A1", StringComparison.Ordinal));
        Assert.Contains(outcome.Warnings, w => w.Contains("regen", StringComparison.Ordinal));
        var closed = setup.Draft.CompleteApply(outcome);
        Assert.True(closed.Rebased);
        Assert.Equal(DraftStatus.Clean, setup.Draft.Status);
        Assert.Equal(outcome.Warnings.ToArray(), setup.Draft.Warnings.ToArray());
    }

    // The comparison of POST is byte for byte: nothing is normalized, so each of these differences fails it.
    [Theory]
    [InlineData("trailing")]
    [InlineData("crlf")]
    [InlineData("case")]
    public void ThePostComparisonDoesNotNormalizeAnything(string change)
    {
        var setup = new FullUpdateSetup();
        setup.Port.TextAfterSeal = (key, text) =>
        {
            if (key != "B2") return text;
            return change switch
            {
                "trailing" => text + " ",
                "crlf" => text.Replace("\n", "\r\n", StringComparison.Ordinal) + "\r\n",
                _ => text.Replace("\"lateral\"", "\"Lateral\"", StringComparison.Ordinal),
            };
        };

        var outcome = setup.Run();

        Assert.NotEqual(DraftApplyOutcomeKind.Applied, outcome.Kind);
        Assert.NotEqual(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);

        // M-06 (b): the cause names the envelope that differs (B2), not just any cause.
        Assert.StartsWith(PushBackPilotUpdateRun.PostEnvelopeDiffersPrefix + "B2", outcome.Cause, StringComparison.Ordinal);
    }

    // I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md): by hand over the map. The comparison of POST of the full model, ComparePostFull,
    // over the full read of each re-read (the comparison of the pilot, ComparePost, is retired); the cases and the causes are the same.
    [Fact]
    public void ThePostComparisonFailsForAnotherSetOfKeysAnotherEnvelopeAndAnotherScale()
    {
        var plan = new FullUpdateSetup().ExpectedPlan();
        var good = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));
        Assert.Null(PushBackPilotUpdateRun.ComparePostFull(plan, good));

        var fewer = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Take(2).Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));
        Assert.Equal(PushBackPilotUpdateRun.PostKeysDifferReason, PushBackPilotUpdateRun.ComparePostFull(plan, fewer));

        var more = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).Append(IntentKit.Member("D4", plan.Envelope("C3")!)).ToArray()));
        Assert.Equal(PushBackPilotUpdateRun.PostKeysDifferReason, PushBackPilotUpdateRun.ComparePostFull(plan, more));

        var edited = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.Key == "C3" ? m.EnvelopeText + " " : m.EnvelopeText)).ToArray()));
        Assert.StartsWith(PushBackPilotUpdateRun.PostEnvelopeDiffersPrefix + "C3", PushBackPilotUpdateRun.ComparePostFull(plan, edited), StringComparison.Ordinal);

        Assert.Equal(PushBackPilotUpdateRun.PostRereadMissingReason, PushBackPilotUpdateRun.ComparePostFull(plan, null));
        var notAdmitted = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "ZZ", IntentKit.Member("A1", plan.Envelope("A1")!)));
        Assert.StartsWith(PushBackPilotUpdateRun.PostNotAdmittedPrefix, PushBackPilotUpdateRun.ComparePostFull(plan, notAdmitted), StringComparison.Ordinal);
    }

    // I-64 F4B PB-T4d (owner-pbt4d-cat4d-decisions.md, pregunta 1): the map; the plan is the one of the full PREPARE (FullUpdateSetup) and
    // the rebuild, PushBackPilotSystemRebuild, stays.
    // The system of the outgoing design, rebuilt apart from the text of a member of the plan with the same resolver as PREPARE.
    [Fact]
    public void TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan()
    {
        var plan = new FullUpdateSetup().ExpectedPlan();

        var system = PushBackPilotSystemRebuild.Build(plan.Members[0].DesignText, PilotKit.Catalog);

        Assert.NotNull(system);
        Assert.False(PushBackPilotSystemRebuild.IsComposite(system));
        Assert.Throws<InvalidOperationException>(() => PushBackPilotSystemRebuild.Build("not a design", PilotKit.Catalog));
        Assert.Throws<ArgumentNullException>(() => PushBackPilotSystemRebuild.Build(null!, PilotKit.Catalog));
        Assert.Throws<ArgumentNullException>(() => PushBackPilotSystemRebuild.IsComposite(null!));
    }

    // I-64 F4B PB-T4d (owner-pbt4d-cat4d-decisions.md, pregunta 1): the map, and by hand only the comment of the reader; the plan is the one
    // of the full PREPARE (FullUpdateSetup) and the rebuild, PushBackPilotSystemRebuild, stays.
    // I-64 F4-T4-HOST-C1 (D-C1-01, INV-C1-01, test 1 of D-C1-09): the system that the writer redraws carries the rack name of the envelope of ITS
    // member, raw, because the label of the name of the views is drawn only from it. Everything runs over the text kits of the plan: the members
    // are only deserialized, nothing is drawn.
    [Fact]
    public void TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope()
    {
        var store = new RackEmbedStore();

        // Every member of the plan of a rack carries the name of its envelope, and nothing else of the system changes. (The full reader
        // admits a rack whose views share one name, so the three views carry the same one here; different names come below, by hand.)
        var scan = FullPrepareKit.Scan(
            IntentKit.Member("A1", FullPrepareKit.Frontal(name: "PB G")),
            IntentKit.Member("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost(), name: "PB G")),
            IntentKit.Member("C3", FullPrepareKit.Planta(name: "PB G")));
        var plan = new FullUpdateSetup(() => scan).ExpectedPlan();
        Assert.Equal(new[] { "A1", "B2", "C3" }, plan.Members.Select(m => m.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray());
        foreach (var member in plan.Members)
        {
            var system = PushBackPilotSystemRebuild.BuildForMember(member, PilotKit.Catalog);
            var plain = PushBackPilotSystemRebuild.Build(member.DesignText, PilotKit.Catalog);

            Assert.Equal(store.Deserialize(member.EnvelopeText)!.Name, system.Name);
            Assert.Equal("PB G", system.Name);
            Assert.Equal(plain.IsComposite, system.IsComposite);
            Assert.Equal(plain.TotalLength, system.TotalLength);
            Assert.Equal(plain.Fronts!.Count, system.Fronts!.Count);
            Assert.Equal(plain.InOutBeamCatalogId, system.InOutBeamCatalogId);
            Assert.Equal(plain.HighEndBeamCatalogId, system.HighEndBeamCatalogId);
            Assert.Equal(plain.DefensePieceId, system.DefensePieceId);
        }

        // The name goes as it is written: with spaces around it, identical; a blank one, or none, stays blank and draws no label.
        var design = store.Deserialize(FullPrepareKit.Frontal())!.Design;
        PushBackPilotMemberPlan Member(string text) => new("K0", RackEmbedDocument.ViewFrontal, 0, design!, text);
        string? NameOf(string? written) =>
            PushBackPilotSystemRebuild.BuildForMember(Member(IntentKit.Rewrite(FullPrepareKit.Frontal(), e => e.Name = written)), PilotKit.Catalog).Name;

        // Each member carries the name of its OWN envelope, also when the views of a drawing were named differently.
        Assert.Equal("Rack uno", NameOf("Rack uno"));
        Assert.Equal("Rack dos", NameOf("Rack dos"));
        Assert.Equal("  Mi rack  ", NameOf("  Mi rack  "));
        Assert.Equal("   ", NameOf("   "));
        Assert.True(string.IsNullOrWhiteSpace(NameOf("   ")));
        Assert.True(string.IsNullOrWhiteSpace(NameOf(string.Empty)));
        Assert.True(string.IsNullOrWhiteSpace(NameOf(null)));

        // The arguments and the envelope that cannot be read: the same exceptions as the help that it extends, and one reason of its own.
        var noMember = Record.Exception(() => PushBackPilotSystemRebuild.BuildForMember(null!, PilotKit.Catalog));
        Assert.IsType<ArgumentNullException>(noMember);
        var noCatalog = Record.Exception(() => PushBackPilotSystemRebuild.BuildForMember(Member(FullPrepareKit.Frontal()), null!));
        Assert.IsType<ArgumentNullException>(noCatalog);
        var notADesign = Record.Exception(() => PushBackPilotSystemRebuild.BuildForMember(new PushBackPilotMemberPlan("K0", RackEmbedDocument.ViewFrontal, 0, "not a design", FullPrepareKit.Frontal()), PilotKit.Catalog));
        Assert.IsType<InvalidOperationException>(notADesign);
        Assert.Equal(PushBackPilotSystemRebuild.UnreadableDesignReason, notADesign!.Message);
        var unreadable = Record.Exception(() => PushBackPilotSystemRebuild.BuildForMember(Member("{ not an envelope"), PilotKit.Catalog));
        Assert.IsType<InvalidOperationException>(unreadable);
        Assert.Equal(PushBackPilotSystemRebuild.UnreadableEnvelopeReason, unreadable!.Message);
    }
}

/// <summary>
/// I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md): the copy of PilotUpdateSetup for the FULL model, the helper of the substitutes of
/// the run: the standard rack, a full draft born from the full read with an edited scale, and the double of the port; the plan is the one of
/// the full PREPARE and the run is ExecuteFull.
/// </summary>
internal sealed class FullUpdateSetup
{
    public const double NewScale = 2.25;

    public FullUpdateSetup(Func<PushBackPilotIntentScan>? scan = null, double scale = NewScale)
    {
        Scan = (scan ?? (() => FullPrepareKit.RealScan()))();
        var read = FullWriteKit.Read(Scan);
        Assert.True(read.Read.IsAdmitted);
        Draft = FullWriteKit.DraftOf(read);
        FullWriteKit.Edit(Draft, s => s.Editable.AnnotationScale = scale);
        Members = Scan.Facts.Select(f => new IntentItem(f, Scan.RawEnvelopes[f.DefinitionKey])).ToArray();
        Port = new PilotUpdateScopeDouble(Members);
        Request = new PushBackPilotUpdateRequest(100, FullPrepareKit.Rack);
    }

    public PushBackPilotIntentScan Scan { get; }

    public DraftSession<PushBackFullDraftState> Draft { get; }

    public IntentItem[] Members { get; }

    public PilotUpdateScopeDouble Port { get; }

    public PushBackPilotUpdateRequest Request { get; }

    /// <summary>The plan that the run builds for the standard rack, computed apart with the pure full PREPARE.</summary>
    public PushBackPilotUpdatePlan ExpectedPlan()
    {
        var result = PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(Draft), FullWriteKit.Read(Scan), PilotKit.Catalog);
        Assert.True(result.Kind == PushBackPilotPrepareKind.Ready, result.Reason);
        return result.Plan!;
    }

    /// <summary>Takes the slot like the host does, and runs.</summary>
    public DraftApplyOutcome<PushBackFullDraftState> Run()
    {
        Assert.True(Draft.TryBeginApply().IsStarted);
        return PushBackPilotUpdateRun.ExecuteFull(Draft, Request, Port);
    }
}
```

### `tests/RackCad.Tests/PushBackRetirePilotFullCopyGuardTests.cs` (`NewFileSha` `ee9b6f1513f549065da391c10d6da947cc8e197644efbc7d72cf41e46443e6cf`)

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4d: the equivalent of the equivalent of PB-T4c (PushBackEditAllFullCopyGuardTests), with the BaseSha, the Skip, the CopySha and the
    /// counts of the originals and the copies that the retirement of the pilot edits; nothing else changes.
    /// I-64 F4B PB-T4b — the script of «Saltar y duplicar» (CT-PBT4B-20; D-PBT4-11), in execution. It is a GUARD: it passes on the skeleton
    /// and on the implementation, because the copies, the kit and the Skip travel in the RED, and it discriminates through its negative
    /// self-tests, run over text injected in memory (never written). EVERY text is normalized to <c>\n</c> before it is compared or hashed
    /// (the repository works with <c>core.autocrlf=true</c>; I-01).
    ///
    /// <para>
    /// (1) Each of the 21 original files is its base but for the lines of attribute that carry their Skip. (2) Each test of the list, but
    /// CT-PBT4A-06, has a test of the same name in its copy. (3) In the nine classes of C-0, each mechanical copy is the text of its method in
    /// the base with the closed map M. (4) Each copy and the kit have the sha256 of the contract. (5) No copy names the kit of the pilot.
    /// (6) The full read that the kit derives is the one that the reader gives. (7) The four Theory split by rows are, together, the original.
    /// </para>
    /// </summary>
    public sealed class PushBackRetirePilotFullCopyGuardTests
    {
        private static string Root => PilotKit.RepoRoot().FullName;

        // The base of each original: the sha256 of its text in BaseSha with the line ends as \n (section 6.2.1 of the package), and its number of Skip.
        private static readonly (string Path, string BaseSha, int Skips)[] Origins =
        {
            ("tests/RackCad.Tests/PushBackFullReadGuardTests.cs",
             "97dff70cf57d866895eaae78ebde98e4350135743d747018c7de0a0a4758eb75", 2),
            ("tests/RackCad.Tests/PushBackFullReadPathTests.cs",
             "f6611c3ad90d0e8d481b6a2b56788b53d37f3f0f08af4f1ae5d3cb81783ab28f", 1),
            ("tests/RackCad.Tests/PushBackWritePathGuardTests.cs",
             "996adc5d4ddfc80462e7ade23ef44a8f18fabd0fdbf814b88f2505f437af65b6", 1),
            ("tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingTests.cs",
             "07573661e65b0a4658796a92bf90fd33e79c667a3ccf2f4615588519899bf3f1", 10),
            ("tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs",
             "27f26a28a760225bed676f8ca0d52fb1fd4b64d1d601f64bfdfb1b83f6786a66", 54),
            ("tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs",
             "da9d341f6b544d3d85eaafdd01cffe7796fa5367a8ead1e2f0071e2110e5cab4", 7),
            ("tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs",
             "f52072b53b819edd270e05ec38c987a54aa6772fbe09c18aee71afb4f4067b80", 1),
            ("tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs",
             "0279187354a9bbacc1f0c487b6741d6286a7ab3db7a5fb47aba175ec1cdfde50", 60),
            ("tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs",
             "4044c8678c60a0e05cef7eb6fa8d6b8161651f4dbf17f8b1ced04498b3975997", 2),
            ("tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs",
             "1808ac7a22c33938b575063e3b7a049450f187b493c5b09b4464ddc37acfc5f0", 69),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs",
             "60cb0f3d2412fb7fd630fa97be708f3c449f0a3c6ee786951732c1bf98e92c49", 5),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs",
             "235fc42968106cb9915e6e392ef095edddf46d192f42efd3e0a0ea8343e43289", 41),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs",
             "6f5844735f7568bb94b3a295e64420b36d7718e71b72437cab15a2f3521af40f", 33),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs",
             "48dead3420e3ac272bb05e43732bae50d8e47eccacf09bb8f37578acc5d35494", 1),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs",
             "a4f0e37fbac6db89d19969b14ef1b1a8bd7e7727e5526fcae133ed83c85554e9", 21),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs",
             "73a491cf46d161186d02aba330136fc1b08dc67cd1b71d53db99b3a8946fc8df", 1),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs",
             "d602daf657aae175f9972052e1594034959c0706fd2466644f740d981bfa18fe", 35),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs",
             "df9f8ef808307b71a925cf97a38863abcc50247dddc5df6b4c839d8488b94103", 2),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs",
             "cabfb232768fe69e1e7b686722fa87d05efe97b44474c1b4b8a411db598b130e", 44),
            ("tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs",
             "1cedca91c973b0dfe8825e002bfcab4fc883d71219bf7fc4612f19dc4ea8300a", 21),
            ("tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewTests.cs",
             "e8db638a72da2a517cba2ea66a762a79b25ddd75621e9d3e95c07ef97a2bd568", 6),
        };

        // The sha256 of each copy: its text with the line ends as LF (CopySha of the contract).
        private static readonly Dictionary<string, string> CopySha = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["tests/RackCad.Tests/PushBackFullReadGuardFull.cs"] =
                "ee09002b03cc3231c76ccfe4900919ee1241ffc85ea5d42d8d83278cbba3aa35",
            ["tests/RackCad.Tests/PushBackFullReadPathFull.cs"] =
                "8d4cfb2eebdbcfc0615243368feca221a4f4834bef704abb4e097528d9039bc7",
            ["tests/RackCad.Tests/PushBackWritePathGuardFull.cs"] =
                "38cb158d8464ff303eaadddf9e1722e49057618d39bb83b9ee32150fd3ea5a3c",
            ["tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingFull.cs"] =
                "5d54905f0a3f76ba786dce1d0adaa89eee3fe50c867197e575d50b07cf958a01",
            ["tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorFull.cs"] =
                "af0929f95082d0bd896ed11afbdc2f1cdc6f10749e34ab678193955fcf924522",
            ["tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardFull.cs"] =
                "a6c90caec73f6f440b2228e60279f478372409cce0bdce8504c7cc29d5da76ed",
            ["tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostFull.cs"] =
                "c269eac4774b6f5bf5ee20c7a9a2a02097b15060c6e6c3b8d5dd32d9f316ad9e",
            ["tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardFull.cs"] =
                "f82806835c6da858abda885bbfc8296b28c7952d64038e72eebbabd5cb27d9ea",
            ["tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardFull.cs"] =
                "f4523b11a73c223ece1bbd930a578f9be637cb38af8a2066b110a060bfd1dac7",
            ["tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostFull.cs"] =
                "91aa5c15a095ddcdafcba74d73c2e0a78d013d8bfe53a4d4f3af6708cf075c16",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardFull.cs"] =
                "70d4f004c432c23c96e939f0cc1916b864db27f90e1ed30d704bef152349e331",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotEditHostFull.cs"] =
                "7fed50bac4d5bc4bfe440f75c6eac37647142c0538f42278cb9612c5f9fdc95c",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs"] =
                "ca88ddf8d16af66548081c290b038e9f4b017eceabab3c9d77ac1fef3d8dfc8d",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardFull.cs"] =
                "b07cf90219fa182786e2a731c6a5e6fbc06cabe4480bd811418e4b8ab78f7bfc",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostFull.cs"] =
                "83980964cfd66ae88dc0d93f2f6e9e6e9d3bac5a60ce99698039230d96f0c97d",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardFull.cs"] =
                "30a47920ea337b6620472a2614a3cad2c8a699e3238259fb0741b348535911c4",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostFull.cs"] =
                "4ba5042576e0ca28771120ad322c18f7021b870db49999b5ce66a430f398b4f0",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardFull.cs"] =
                "889f5d86a7097db0cecd9e6bb216e5c587d7c5e934eea8d37f048f667b0cd383",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestFull.cs"] =
                "e81cc9d319544140c31850bb8f876d884f19a3b0f77d2ade3184b25209ca0fbb",
            ["tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationFull.cs"] =
                "985f9b1ff90da56e7093a6675196f7586436842eedea643991bc1c9d6d3c502d",
            ["tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewFull.cs"] =
                "54953f62130a7dc3f4f57b43039bbffd291ae628957766e90e427d1f9f792bdf",
        };

        private const string KitPath = "tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs";
        private const string KitSha = "b4ed29f92838e5761faf1afaba1f33e58ba82a56541900f52db8c5ecb6239b9a";


        // The nine classes of C-0 (the mechanical copies and the equivalents written by hand), and the numbers of the map of the copy v5.
        private static readonly Dictionary<string, (int Tests, int Mechanical, int Hand)> NineCounts = new Dictionary<string, (int, int, int)>(StringComparer.Ordinal)
        {
            ["WorkspaceAssociativeBookkeeping"] = (10, 10, 0),
            ["WorkspaceCommandAdvisor"] = (54, 54, 0),
            ["WorkspaceEditIntentHost"] = (60, 55, 5),
            ["WorkspaceLifecycleHost"] = (69, 68, 1),
            ["WorkspacePilotEditHost"] = (41, 41, 0),
            ["WorkspacePilotPrepare"] = (33, 25, 8),
            ["WorkspacePilotReconciliationHost"] = (21, 12, 9),
            ["WorkspacePilotUpdateHost"] = (35, 12, 23),
            ["WorkspacePilotUpdateRequest"] = (44, 39, 5),
        };

        // The tests of the other twelve originals that have a copy (the guards and the two of the interface); the CT of the census has none.
        private static readonly Dictionary<string, int> OtherCounts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["PushBackFullReadGuard"] = 1,
            ["PushBackFullReadPath"] = 1,
            ["PushBackWritePathGuard"] = 1,
            ["WorkspaceEditIntentGuard"] = 7,
            ["WorkspaceHostHardeningGuard"] = 1,
            ["WorkspaceLifecycleGuard"] = 2,
            ["WorkspacePilotEditGuard"] = 5,
            ["WorkspacePilotReconciliationGuard"] = 1,
            ["WorkspacePilotUpdateGuard"] = 1,
            ["WorkspacePilotUpdateIntegrationGuard"] = 2,
            ["WorkspacePilotReconciliation"] = 21,
            ["WorkspacePilotReconciliationView"] = 6,
        };

        // M-00: the types of the nine classes that are copied with the suffix Full (the others of the nine have none).
        private static readonly Dictionary<string, string[]> TypesOfTheCopy = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["WorkspaceCommandAdvisor"] = new[] { "CommandAdvisorPortDouble" },
            ["WorkspaceLifecycleHost"] = new[] { "LifecycleScriptedAdvisor" },
            ["WorkspacePilotEditHost"] = new[] { "PilotEditThrowingComparer", "PilotEditThrowingProbe", "PilotEditAuditedReader", "PilotEditOutcomeProbe" },
        };

        // M-01 to M-08, closed and in this order.
        private static readonly (string Pattern, string Replacement)[] MapM =
        {
            (@"\bEditIntentFakeReader\b", "FullEditIntentFakeReader"),
            (@"\bEditIntentKit\b", "FullEditIntentKit"),
            (@"\bPrepareKit\b", "FullPrepareKit"),
            (@"\bPushBackPilotDraftState\b", "PushBackFullDraftState"),
            (@"\bPushBackPilotStateComparer\b", "PushBackFullDraftStateComparer"),
            (@"\bPushBackPilotFieldAdapter\b", "PushBackFullFieldAdapter"),
            (@"\.Inputs\.Annotations!?\.AnnotationScale\b", ".Editable.AnnotationScale"),
            (@"\.Inputs\.PalletTolerance\b", ".Editable.PalletTolerance"),
        };

        // The four Theory that the partition of the RED splits by rows (D-S5): the class, the method and whether their rows are a MemberData.
        private static readonly (string Stem, string Method, bool Member)[] Splits =
        {
            ("WorkspaceLifecycleHost", "EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively", false),
            ("WorkspacePilotEditGuard", "TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance", false),
            ("WorkspacePilotPrepare", "ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing", true),
            ("WorkspacePilotPrepare", "KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading", true),
        };

        private static readonly Dictionary<string, (int Red, int Green)> SplitRows = new Dictionary<string, (int, int)>(StringComparer.Ordinal)
        {
            ["WorkspaceLifecycleHost.EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively"] = (3, 7),
            ["WorkspacePilotEditGuard.TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance"] = (1, 3),
            ["WorkspacePilotPrepare.ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing"] = (7, 1),
            ["WorkspacePilotPrepare.KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading"] = (7, 1),
        };

        private static readonly Regex SkipLine = new Regex(
            @"^([ \t]*)\[(Fact|Theory)\(Skip = ""I-64 F4B PB-T4b: [^""]*""\)\]", RegexOptions.Multiline);

        private static readonly Regex KitOfThePilot = new Regex(
            @"\b(?:EditIntentFakeReader|EditIntentKit|PrepareKit|PushBackPilotDraftState|PushBackPilotStateComparer|PushBackPilotFieldAdapter|PushBackFullReadRig)\b");

        // ---------------------------------------------------------------------------------------------- helpers

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
        private static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        private static string Normalize(string text) => text.Replace("\r\n", "\n");

        private static string ReadText(string relative) => Normalize(File.ReadAllText(Path.Combine(Root, relative)));

        private static string Sha(string text)
        {
            using (var sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(new UTF8Encoding(false).GetBytes(text)).Select(b => b.ToString("x2")));
            }
        }

        // I-64 F4B PB-T4c (owner-pbt4c-list-3-decision.md): the one Skip of PB-T4c in an original, the one of WorkspacePilotEditGuardTests:155,
        // goes back to its [Fact] too, by its exact line; the count of the Skip of PB-T4b and the expected lines do not change.
        private const string EditAllSkipLine = "    [Fact(Skip = \"I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).\")]";

        private static string Restore(string text) => SkipLine.Replace(text, "$1[$2]").Replace(EditAllSkipLine + "\n", "    [Fact]\n");

        private static string Stem(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return name.EndsWith("Tests", StringComparison.Ordinal) ? name.Substring(0, name.Length - "Tests".Length) : name;
        }

        private static string CopyPathOf(string path) => path.Substring(0, path.Length - "Tests.cs".Length) + "Full.cs";

        private static string ApplyMap(string stem, string text)
        {
            foreach (var (pattern, replacement) in MapM) text = Regex.Replace(text, pattern, replacement);
            if (TypesOfTheCopy.TryGetValue(stem, out var types))
            {
                foreach (var type in types) text = Regex.Replace(text, @"\b" + Regex.Escape(type) + @"\b", type + "Full");
            }

            return text;
        }

        // ---------------------------------------------------------------------------------------------- the lexicon and the members

        private static void Blank(StringBuilder builder, string code, int from, int to)
        {
            for (var x = from; x < to; x++) builder.Append(code[x] == '\n' ? '\n' : ' ');
        }

        /// <summary>The text with the comments and the inside of the strings and of the characters blanked, without moving a character.</summary>
        private static string Mask(string code)
        {
            var builder = new StringBuilder(code.Length);
            var i = 0;
            var n = code.Length;
            while (i < n)
            {
                var c = code[i];
                if (c == '/' && i + 1 < n && code[i + 1] == '/')
                {
                    var end = code.IndexOf('\n', i);
                    if (end < 0) end = n;
                    Blank(builder, code, i, end);
                    i = end;
                    continue;
                }

                if (c == '/' && i + 1 < n && code[i + 1] == '*')
                {
                    var end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    end = end < 0 ? n : end + 2;
                    Blank(builder, code, i, end);
                    i = end;
                    continue;
                }

                var k = i;
                var verbatim = false;
                var interpolated = false;
                while (k < n && (code[k] == '$' || code[k] == '@'))
                {
                    if (code[k] == '@') verbatim = true;
                    else interpolated = true;
                    k++;
                }

                if (k < n && code[k] == '"' && (k > i || i == 0 || !(char.IsLetterOrDigit(code[i - 1]) || code[i - 1] == '_')))
                {
                    var j = k + 1;
                    var depth = 0;
                    var closed = false;
                    while (j < n)
                    {
                        var ch = code[j];
                        if (interpolated && ch == '{')
                        {
                            if (depth == 0 && j + 1 < n && code[j + 1] == '{')
                            {
                                j += 2;
                                continue;
                            }

                            depth++;
                        }
                        else if (interpolated && ch == '}' && depth > 0)
                        {
                            depth--;
                        }
                        else if (depth == 0)
                        {
                            if (verbatim && ch == '"')
                            {
                                if (j + 1 < n && code[j + 1] == '"')
                                {
                                    j += 2;
                                    continue;
                                }

                                closed = true;
                                break;
                            }

                            if (!verbatim && ch == '\\')
                            {
                                j += 2;
                                continue;
                            }

                            if (!verbatim && ch == '"')
                            {
                                closed = true;
                                break;
                            }

                            if (!verbatim && ch == '\n') break;
                        }

                        j++;
                    }

                    j = Math.Min(j, n);
                    builder.Append(code, i, k + 1 - i);
                    Blank(builder, code, k + 1, j);
                    if (closed && j < n)
                    {
                        builder.Append('"');
                        j++;
                    }

                    i = j;
                    continue;
                }

                if (c == '\'')
                {
                    var j = i + 1;
                    if (j < n && code[j] == '\\')
                    {
                        j += 2;
                        while (j < n && code[j] != '\'' && code[j] != '\n') j++;
                    }
                    else
                    {
                        j++;
                    }

                    if (j < n && code[j] == '\'')
                    {
                        builder.Append('\'');
                        Blank(builder, code, i + 1, j);
                        builder.Append('\'');
                        i = j + 1;
                        continue;
                    }
                }

                builder.Append(c);
                i++;
            }

            return builder.ToString();
        }

        private sealed class Method
        {
            public string Name { get; set; }

            public int Start { get; set; }

            /// <summary>The attribute lines above the declaration.</summary>
            public string Header { get; set; }

            /// <summary>From the first line of attribute to the closing brace, without the line end.</summary>
            public string Text { get; set; }
        }

        private static readonly Regex Declaration = new Regex(@"\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(");

        /// <summary>The tests of a text ([Fact] and [Theory]) with their attribute lines and their text, from the first attribute to the closing brace.</summary>
        private static List<Method> Methods(string text)
        {
            var masked = Mask(text);
            var lines = masked.Split('\n');
            var starts = new int[lines.Length];
            for (var line = 1; line < lines.Length; line++) starts[line] = starts[line - 1] + lines[line - 1].Length + 1;

            var result = new List<Method>();
            foreach (Match match in Declaration.Matches(masked))
            {
                var at = Array.BinarySearch(starts, match.Index);
                if (at < 0) at = ~at - 1;
                var first = at - 1;
                while (first >= 0 && lines[first].TrimStart().StartsWith("[", StringComparison.Ordinal)) first--;
                first++;
                if (first == at) continue;
                var attributes = string.Join("\n", lines.Skip(first).Take(at - first));
                if (!Regex.IsMatch(attributes, @"\[(?:Fact|Theory)\b")) continue;

                var depth = 1;
                var p = match.Index + match.Length;
                while (p < masked.Length && depth > 0)
                {
                    if (masked[p] == '(') depth++;
                    else if (masked[p] == ')') depth--;
                    p++;
                }

                while (p < masked.Length && char.IsWhiteSpace(masked[p])) p++;
                int close;
                if (p < masked.Length && masked[p] == '{')
                {
                    var braces = 0;
                    close = p;
                    for (; close < masked.Length; close++)
                    {
                        if (masked[close] == '{') braces++;
                        else if (masked[close] == '}' && --braces == 0) break;
                    }
                }
                else
                {
                    close = masked.IndexOf(';', p);
                }

                var lineEnd = text.IndexOf('\n', Math.Min(close, text.Length - 1));
                if (lineEnd < 0) lineEnd = text.Length;
                var start = starts[first];
                result.Add(new Method
                {
                    Name = match.Groups[1].Value,
                    Start = start,
                    Header = text.Substring(start, starts[at] - start),
                    Text = text.Substring(start, lineEnd - start),
                });
            }

            return result;
        }

        /// <summary>The tests of an original that carry the Skip of PB-T4b, with its reason.</summary>
        private static List<(string Name, string Reason)> Listed(string original)
            => Methods(original)
                .Select(method => (method.Name, Match: Regex.Match(method.Header, @"Skip = ""(I-64 F4B PB-T4b: [^""]*)""")))
                .Where(item => item.Match.Success)
                .Select(item => (item.Name, item.Match.Groups[1].Value))
                .ToList();

        /// <summary>CT-PBT4A-06 is replaced by CT-PBT4B-13 and has no copy; every other test of the list has one.</summary>
        private static bool HasCopy(string reason) => !reason.Contains("CT-PBT4B-13");

        // ---------------------------------------------------------------------------------------------- the lines

        private static string OriginalLine((string Path, string BaseSha, int Skips) origin, string text)
            => "original " + Stem(origin.Path) + " igual-a-la-base=" + (Sha(Restore(text)) == origin.BaseSha) + " skip=" + SkipLine.Matches(text).Count;

        private static string CopyLine((string Path, string BaseSha, int Skips) origin, string original, string copy)
        {
            var stem = Stem(origin.Path);
            var copySha = CopySha[CopyPathOf(origin.Path)];
            var listed = Listed(original).Where(item => HasCopy(item.Reason)).ToList();
            var copyMethods = Methods(copy);
            if (!NineCounts.ContainsKey(stem))
            {
                var present = listed.Count(item => copyMethods.Any(method => method.Name == item.Name));
                return "copia " + stem + " pruebas=" + present + " sha256=" + (Sha(copy) == copySha);
            }

            var baseMethods = Methods(Restore(original)).GroupBy(method => method.Name).ToDictionary(group => group.Key, group => group.First());
            var tests = 0;
            var mechanical = 0;
            var hand = 0;
            var equal = true;
            foreach (var item in listed)
            {
                var found = copyMethods.Where(method => method.Name == item.Name).ToList();
                if (found.Count == 0) continue;
                tests++;
                if (item.Reason.Contains("su equivalente escrito a mano en"))
                {
                    hand++;
                    continue;
                }

                mechanical++;
                equal = equal && found.Count == 1 && baseMethods.ContainsKey(item.Name) && found[0].Text == ApplyMap(stem, baseMethods[item.Name].Text);
            }

            return "copia " + stem + " pruebas=" + tests + " mecanicas=" + mechanical + " a-mano=" + hand + " igual-al-mapa=" + equal + " sha256=" + (Sha(copy) == copySha);
        }

        private static List<string> RowsOf(Method method)
            => method.Header.Split('\n').Where(line => line.TrimStart().StartsWith("[InlineData(", StringComparison.Ordinal)).ToList();

        private static List<string> MemberRows(string typeName, string member)
        {
            var type = typeof(WorkspaceFullCopyGuardTests).Assembly.GetType(typeName);
            var method = type == null ? null : type.GetMethod(member, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var rows = method == null ? null : method.Invoke(null, null) as System.Collections.IEnumerable;
            return rows == null ? new List<string> { "<sin filas>" } : rows.Cast<object[]>().Select(row => Convert.ToString(row[0])).ToList();
        }

        private static string SplitLine((string Stem, string Method, bool Member) split, string original, string copy)
        {
            var label = split.Stem + "." + split.Method;
            var redAt = copy.IndexOf("class FullPanelCopy" + split.Stem + "RedTests", StringComparison.Ordinal);
            var greenAt = copy.IndexOf("class FullPanelCopy" + split.Stem + "GreenTests", StringComparison.Ordinal);
            var methods = Methods(copy).Where(method => method.Name == split.Method).ToList();
            var inRed = methods.Where(method => redAt >= 0 && method.Start > redAt && (greenAt < 0 || method.Start < greenAt)).ToList();
            var inGreen = methods.Where(method => greenAt >= 0 && method.Start > greenAt).ToList();
            if (redAt < 0 || greenAt < redAt || inRed.Count != 1 || inGreen.Count != 1) return "particion-por-filas " + label + " sin-las-dos-mitades";

            List<string> redRows;
            List<string> greenRows;
            List<string> originalRows;
            if (split.Member)
            {
                redRows = MemberRows("RackCad.Tests.FullPanelCopy" + split.Stem + "RedTests", "StatusNamesRed");
                greenRows = MemberRows("RackCad.Tests.FullPanelCopy" + split.Stem + "GreenTests", "StatusNamesGreen");
                originalRows = MemberRows("RackCad.Tests." + split.Stem + "Tests", "StatusNames");
            }
            else
            {
                redRows = RowsOf(inRed[0]);
                greenRows = RowsOf(inGreen[0]);
                var source = Methods(Restore(original)).First(method => method.Name == split.Method);
                originalRows = RowsOf(source).Select(row => ApplyMap(split.Stem, row)).ToList();
            }

            var together = redRows.Concat(greenRows).OrderBy(row => row, StringComparer.Ordinal).ToList();
            var same = together.SequenceEqual(originalRows.OrderBy(row => row, StringComparer.Ordinal), StringComparer.Ordinal) && together.Distinct().Count() == together.Count;
            return "particion-por-filas " + label + " red=" + redRows.Count + " green=" + greenRows.Count + " filas-del-original=" + same;
        }

        private static (string Path, string BaseSha, int Skips) OriginOf(string stem) => Origins.First(origin => Stem(origin.Path) == stem);

        // ------------------------------------------------------------------------------------------------- CT-20

        [Fact]
        public void CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var originals = new Dictionary<string, string>(StringComparer.Ordinal);
            var copies = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var origin in Origins)
            {
                originals[origin.Path] = Paso(log, () => ReadText(origin.Path)) ?? string.Empty;
                copies[origin.Path] = Paso(log, () => ReadText(CopyPathOf(origin.Path))) ?? string.Empty;
            }

            // (1) The 21 originals.
            foreach (var origin in Origins)
            {
                log.Add(OriginalLine(origin, originals[origin.Path]));
                expected.Add("original " + Stem(origin.Path) + " igual-a-la-base=True skip=" + origin.Skips);
            }

            // (2), (3) and (4): the nine classes of C-0 and then the twelve copies of the guards and of the interface.
            foreach (var origin in Origins.Where(origin => NineCounts.ContainsKey(Stem(origin.Path))))
            {
                log.Add(Paso(log, () => CopyLine(origin, originals[origin.Path], copies[origin.Path])));
                var counts = NineCounts[Stem(origin.Path)];
                expected.Add("copia " + Stem(origin.Path) + " pruebas=" + counts.Tests + " mecanicas=" + counts.Mechanical + " a-mano=" + counts.Hand + " igual-al-mapa=True sha256=True");
            }

            foreach (var origin in Origins.Where(origin => !NineCounts.ContainsKey(Stem(origin.Path))))
            {
                log.Add(Paso(log, () => CopyLine(origin, originals[origin.Path], copies[origin.Path])));
                expected.Add("copia " + Stem(origin.Path) + " pruebas=" + OtherCounts[Stem(origin.Path)] + " sha256=True");
            }

            var kit = Paso(log, () => ReadText(KitPath)) ?? string.Empty;
            log.Add("kit sha256=" + (Sha(kit) == KitSha));
            expected.Add("kit sha256=True");

            // (7) The four Theory split by rows.
            foreach (var split in Splits)
            {
                var origin = OriginOf(split.Stem);
                log.Add(Paso(log, () => SplitLine(split, originals[origin.Path], copies[origin.Path])));
                var rows = SplitRows[split.Stem + "." + split.Method];
                expected.Add("particion-por-filas " + split.Stem + "." + split.Method + " red=" + rows.Red + " green=" + rows.Green + " filas-del-original=True");
            }

            // (6) M-05: the full read that the kit derives is the one that the reader gives, over five scans.
            var scans = Paso(log, () => new[]
            {
                FullEditIntentKit.TwoViews(FullEditIntentKit.RackA),
                FullPrepareKit.RealScan(),
                IntentKit.Scan(new RackEmbedStore().Deserialize(PilotKit.Text("pb-20-ab")).Id, "H1", IntentKit.Member("H1", PilotKit.Text("pb-20-ab"))),
                IntentKit.Scan(
                    IntentKit.Rack,
                    "A1",
                    IntentKit.Member("A1", IntentKit.Rewrite(IntentKit.BaseText(), e => e.Design = I58F1Fixtures.Set(I58F1Fixtures.Rich("pushback"), "PushBack.Future", "1")))),
                IntentKit.Scan(FullEditIntentKit.RackA, "ZZ", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", FullEditIntentKit.RackA))),
            });
            log.Add("kit FullOf-igual-al-lector=" + (scans != null && scans.All(scan => FullHandKit.FullOfMatchesTheReader(scan))) + " escaneos=" + (scans == null ? 0 : scans.Length));
            expected.Add("kit FullOf-igual-al-lector=True escaneos=5");

            // (5) No copy of the nine classes of C-0 names the kit of the pilot (the guards name the state of the pilot in the text they inject).
            log.Add("sin-nombres-del-kit-del-piloto=" + !Origins.Where(origin => NineCounts.ContainsKey(Stem(origin.Path))).Any(origin => KitOfThePilot.IsMatch(copies[origin.Path])));
            expected.Add("sin-nombres-del-kit-del-piloto=True");

            // The self-tests, over text injected in memory: each makes the guard see another line than the real one.
            var sample = OriginOf("WorkspaceAssociativeBookkeeping");
            var original = originals[sample.Path];
            var copy = copies[sample.Path];
            var realOriginal = OriginalLine(sample, original);
            var realCopy = Paso(log, () => CopyLine(sample, original, copy));
            log.Add("autoprueba-linea-de-mas=" + (OriginalLine(sample, original + "\n// una linea de mas\n") == realOriginal ? "pasa" : "falla"));
            var changedAssertion = new Regex(@"Assert\.Equal\(").Replace(copy, "Assert.NotEqual(", 1);
            log.Add("autoprueba-asercion-cambiada=" + (Paso(log, () => CopyLine(sample, original, changedAssertion)) == realCopy ? "pasa" : "falla"));
            var extraSkip = original + "\n    [Fact(Skip = \"I-64 F4B PB-T4b: otra\")]\n    public void Otra() { }\n";
            log.Add("autoprueba-skip-de-mas=" + (OriginalLine(sample, extraSkip) == realOriginal ? "pasa" : "falla"));
            var firstName = Listed(original).First().Name;
            var withoutCopy = new Regex(@"\bpublic void " + Regex.Escape(firstName) + @"\(").Replace(copy, "public void Renombrada(", 1);
            log.Add("autoprueba-sin-copia=" + (Paso(log, () => CopyLine(sample, original, withoutCopy)) == realCopy ? "pasa" : "falla"));
            expected.Add("autoprueba-linea-de-mas=falla");
            expected.Add("autoprueba-asercion-cambiada=falla");
            expected.Add("autoprueba-skip-de-mas=falla");
            expected.Add("autoprueba-sin-copia=falla");

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
```

### `tests/RackCad.Tests/PushBackRetirePilotPlanGuardTests.cs` (`NewFileSha` `48c2f4a9619e2f3e7e5de4f1ad98bf68024635c268866fa1242f65f329d8ca43`)

```csharp
#nullable enable
using System;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4B PB-T4d (owner-pbt4d-list-decisions.md, pregunta 1): the equivalents of two guards of WorkspacePilotPrepareGuardTests (F4-T4A1)
// that name what the retirement of the pilot removes. Each one is its original with one declared change and nothing else:
// (1) the types of the package lose the snapshot of the pilot (PushBackPilotPrepareInput) and keep the one of the full model
//     (PushBackFullPrepareInput), with its private constructor, its one factory From over a draft of the full model and its one construction;
// (2) the order of PREPARE is read in PrepareFull, the PREPARE that stays, whose last step is BuildFull.
public class PushBackRetirePilotPlanGuardTests
{
    private const string PlanPath = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs";
    private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";

    private static string Plan() => Src.Code(PlanPath);

    [Fact]
    public void TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage()
    {
        var text = Src.Text(PlanPath);

        Assert.StartsWith("#nullable enable", text.TrimStart((char)0xFEFF));
        var namespaces = Regex.Matches(Src.CodeOnly(text), @"^\s*namespace\s+([\w\.]+)\s*(\{|;)", RegexOptions.Multiline);
        var declared = Assert.Single(namespaces);
        Assert.Equal("RackCad.Application.Systems.PushBack.Pilot", declared.Groups[1].Value);
        Assert.Equal("{", declared.Groups[2].Value);
        Assert.Equal(PilotFolder + "PushBackPilotUpdatePlan.cs", PlanPath);
        foreach (var type in new[] { "PushBackFullPrepareInput", "PushBackPilotUpdatePlanner", "PushBackPilotUpdatePlan", "PushBackPilotPrepareResult", "PushBackPilotPrepareKind" })
            Assert.Matches(@"\b(?:class|enum)\s+" + type + @"\b", Plan());
        // The one construction of the snapshot is its factory: the constructor is private.
        Assert.Matches(@"private\s+PushBackFullPrepareInput\s*\(", Plan());
        Assert.Matches(@"public\s+static\s+PushBackFullPrepareInput\s+From\s*\(\s*DraftSession<PushBackFullDraftState>\s+draft\s*\)", Plan());
        Assert.Equal(1, Src.Count(Plan(), @"\bnew\s+PushBackFullPrepareInput\s*\("));
    }

    [Fact]
    public void TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread()
    {
        var plan = Plan();
        var prepare = Src.BodyOf(plan, @"public\s+static\s+PushBackPilotPrepareResult\s+PrepareFull\s*\(");

        var order = new[]
        {
            "PushBackPilotPrepareKind.NotApplicable",
            "PushBackPilotPrepareKind.PendingText",
            "PushBackPilotPrepareKind.NothingToApply",
            ".CompareWith(",
            "WithConflict(",
            "reread.Read.IsAdmitted",
            "WithNotAdmitted(",
            "BuildFull(",
        };
        var previous = -1;
        foreach (var needle in order)
        {
            var at = prepare.IndexOf(needle, StringComparison.Ordinal);
            Assert.True(at > previous, "out of order or missing: " + needle);
            previous = at;
        }

        // The re-read is not named before NothingToApply: it is not looked at for a clean draft.
        var nothing = prepare.IndexOf("PushBackPilotPrepareKind.NothingToApply", StringComparison.Ordinal);
        Assert.DoesNotContain("reread.", prepare.Substring(0, nothing), StringComparison.Ordinal);
        // The conflict comes before the admission.
        Assert.True(prepare.IndexOf("WithConflict(", StringComparison.Ordinal) < prepare.IndexOf("reread.Read.IsAdmitted", StringComparison.Ordinal));
        // Any exception is an Invalid with its reason, never an exception to the caller.
        Assert.Matches(@"catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*return\s+PushBackPilotPrepareResult\.WithInvalid\s*\(", prepare);
        Assert.DoesNotMatch(new Regex(@"\bthrow\b"), prepare);
    }
}
```
