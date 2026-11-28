**Estado:** v5.2. Escribe **PB-T4d** (`F4B-PBT4-RETIRE-PILOT`, la retirada del código del piloto) en forma literal, como la v5.0 y la v5.1 lo hicieron con PB-T4c, sobre el **cierre de PB-T4c** (`6c2820ba`, F4B-E55: CERRADA, intento 1 de 3, sin reparación del RED). La remedición se hizo por objetos git (`v52/remeasure_pbt4d_v52.py`) y compilando y ejecutando en copias exportadas de `6c2820ba` (`/d/r6a` en adelante; `pbt4dcc/compile/`).

**Las decisiones que la fijan:**
- **El Owner, en cuatro registros sobre PB-T4d:**
  - `owner-pbt4d-list-decisions.md` (sha256 `@@SHA_O1@@`): el alcance B, la recarga de «Editar todo» a una tarea propia, la prueba lenta acotada en PB-T4d y la emisión condicional;
  - `owner-pbt4d-substitutes-decision.md` (`@@SHA_O2@@`): «Sustitutas donde hace falta»;
  - `owner-pbt4d-run-coverage-decision.md` (`@@SHA_O3@@`): «Sustituir las 21», con la cobertura medida otra vez;
  - `owner-pbt4d-deadcode-27-decisions.md` (`@@SHA_O4@@`): «Quitarla en PB-T4d», la rama muerta de `BuildCore`, y «Sí, 27», las sustitutas de la corrida.
- **La sesión:** D-S11 a D-S15 (`session-pbt4d-design-decisions.md`, sha256 `@@SHA_S1@@`): el RED con todo lo de `tests/` y dos CT, los `Skip` con su equivalente y el prefijo `PushBackRetirePilot`, M-04 en su sitio, la prueba lenta acotada y la limpieza de los comentarios.

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

