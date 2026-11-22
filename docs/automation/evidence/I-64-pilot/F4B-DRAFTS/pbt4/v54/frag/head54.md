**Estado:** v5.4, **solo de texto**. Corrige la v5.3 de **PB-T4d** (`F4B-PBT4-RETIRE-PILOT`) tras la recomprobación 1 de CA-T4d (al final de `independent-review-pbt4d-check.md`, sha256 `@@SHA_REVIEW@@`: EMITIBLE, B 0 / I 0 / M 6, M-08 a M-13), sobre la misma base (`6c2820ba`). No se repite ninguna corrida de pruebas: los cambios del RED son solo de comentarios (el código sin comentarios es el de la v5.3) y los del GREEN, solo su cabecera; las piezas de la v5.4 se reaplicaron en una copia nueva (`/d/r70`), con 0 errores y 0 `warning CS`, y la cobertura se volvió a leer de los mismos JSON.

**Las decisiones:** sin decisiones nuevas del Owner. **La sesión:** D-S16 con su nota (`session-pbt4d-design-decisions.md`, sha256 `@@SHA_S1@@`): cada entrada de «Bajan» nombra la corrida, no la prueba, y ESTABLE solo por la UI exige también un filtro en su proceso.

**Lo que cambia:** la cabecera (este registro) y la de la v5.3 (M-10); §4.4; §6.4.1 y §6.4.2; §7; §8.5; §11 y el anexo, que pasa a V54-A; las cifras de la lista en §0 y §8.3. **Lo que no cambia:** el resto de PB-T4d, PB-T4a, PB-T4b y PB-T4c.

**Lo que sale de la v5.4 (PB-T4d):** las piezas `pbt4d-red-v54.txt` y `pbt4d-green-v54.txt` (con los módulos y aplicadores de la v5.3, `--pieza`), `pbt4d_test_edits_v54.py`, `v54/make_red_v54.py`, `v54/make_list_v54.py` y la lista `@@LIST_FILE@@`; `cov_p6_v54.py`, `p6_json_pbt4d_v54.py`, `post_p6_v54.sh` y `p6_reapply_v54.sh`; y `make_v54.py`, que arma esta versión. Lo de la v5.3, como historia.

---

## Registro de cambios v5.3 → v5.4

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| V54-01 | **Recomprobación 1 de CA-T4d M-08:** el JSON no fijaba por hash todas las entradas de la cobertura, y §6.4.2 no decía sus claves exactas | El JSON fija el sha256 de los JSON de UI del GREEN (`GreenUiCoverage`: las 3 corridas juntas y los 18 filtros, también en `Green.UI[n].Json`) y el completo de los 19 de la base (`BaseCoverage.UIByFilter.Runs`); la reaplicación de la v5.4 va en `Reapply`. §6.4.2 lista las claves exactas: el generador lee `RedDeleted` (comparada con los bloques DELETE) y no lee `Noise` (informativo). Sin correr pruebas | §6.4.2; `pbt4d-precondition6.json`; `compile-check.md` |
| V54-02 | **Recomprobación 1 de CA-T4d M-09:** [13] de §6.4.1 | «D-S11 a D-S15 y lo que la sesión fijó tras CA-T4d» pasa a «D-S11 a D-S16» | §6.4.1 ([13]) |
| V54-03 | **Recomprobación 1 de CA-T4d M-10:** «Lo que cambia» de la v5.3 no nombraba §0, §1, §6.0, §7 ni §9 | La cabecera de la v5.3 los nombra, y la columna «Dónde» de su registro también | La cabecera de la v5.3 y su registro |
| V54-04 | **Recomprobación 1 de CA-T4d M-11:** «Bajan» y la definición de ESTABLE | (a) Cada entrada nombra la corrida que la cubría, no la prueba (coverlet no da la prueba): se declara. (b) ESTABLE solo por la UI exige las tres corridas juntas **y** al menos un filtro en su proceso (`cov_p6_v54.py`). Vuelto a leer de los mismos JSON: @@BAJAN_SHORT@@ | §4.4; `cov-p6-v54.txt` |
| V54-05 | **Recomprobación 1 de CA-T4d M-12:** el texto que quedaba | (a) WPUR en el RED: el resumen de la clase y los comentarios de la n.º 48 y la n.º 49, que se quedaban sin su prueba (dos ediciones con registro, solo comentarios): `pbt4d-red-v54.txt`. (b) La cabecera de la pieza del GREEN nombraba el módulo de la v5.2: `pbt4d-green-v54.txt` (solo esa cabecera; los bloques, los de la v5.3). (c) El porqué de la fila de `WriteViolations` (PEG:906), el de su edición de M-04. (d) La directiva `using` de Draft/ que el RED añade en `WorkspaceDraftPilotIntentStaleBaseTests.cs` tiene su fila (grupo `directiva`): la lista pasa a `@@LIST_FILE@@` | §6.4; la lista; el anexo |
| V54-06 | **Recomprobación 1 de CA-T4d M-13:** el conjunto vigilado de 436 a 537 líneas | Una frase: desde la v5.3 entran también las líneas de WPUR dentro de los tramos que el GREEN borra enteros, que la v5.2 quitaba antes de contar | §4.4 |
| V54-07 | La versión y la reaplicación | El `Objective` cita la v5.4. Las piezas de la v5.4, reaplicadas en `/d/r70` (`p6_reapply_v54.sh`): 5 compilaciones con 0 errores y 0 `warning CS`; frente a la copia principal de la v5.3 (`/d/r6y`), el texto de `tests/` solo cambia en WPUR, su código sin comentarios no cambia y `src/` es igual | §6.4.1; §11 |

---

## La cabecera de la v5.3, sin cambio salvo lo que dice el registro v5.3 → v5.4

