> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Revisión independiente y adversarial: `nc2_exact_whitelist_substitute_f4b.py`

Objeto: `PBT2A-NC2-EXACT-WHITELIST-F4B-01` frente al original `data/pbt2a-substitute-as-run.py`. Revisión sin aplicar ni custodiar nada. Leí `adapt-notes.md` y `preview-*` solo al terminar mi análisis, para contrastar.

## Veredicto: APTO TRAS CORRECCIONES

| B | I | M |
|---|---|---|
| 0 | 4 | 7 |

Sobre los datos actuales, el PASS es correcto. Lo comprobé fuera del script con mis propias comprobaciones:
- sha(`planning/output.json`) = `note.OutputSha256` = `rr0.OutputSha256` = `100A7695…`.
- La salida cruda tiene vacías las 8 colecciones.
- Si se le ponen las 8 colecciones del contrato, la salida cruda reproduce `delegation.json` byte a byte (`indent=2` y `\n`).
- `RunId` y `ContractSha256` coinciden en la nota, el bfc, el relevo y la delegación.
- Solo `ExpectedEvidence[0]` contiene la frase, y es la evidencia previa al Worker.
- En la salida del Controller de nc2, solo `Contract` y `Scope` difieren de la verificación real. El resultado es `EXECUTION_BLOCKED` / `STOP` / `FailureClass = Contract`.

El problema es otro: el script no demuestra mecánicamente parte de lo que certifica. Tres sondas mías dan PASS cuando deberían dar FAIL (X1, X2 y X3).

## 1. El diff
`diff` original → adaptado. Solo cambian:
- el docstring: id, descripción y línea «Uso»;
- `import unicodedata`, `ID` y las constantes `INJECTED` y `BYTE_COMPARISON`;
- la función nueva `mentions_byte_comparison`;
- la propiedad 5 (l. 121-129 y 133) y la propiedad 8 (l. 145-148);
- el sintético (l. 179-214 y 229-269);
- `TARGET_PROPERTY` y la condición `hit` de la autoprueba (l. 273-284);
- el 6.º argumento (l. 298 y 314-315), el registro `_F4B` (l. 328) y `argv[1:7]` (l. 337).

Las propiedades 1-4, 6, 7, 9 y 10, los auxiliares, `load_inputs` y `git diff` quedan idénticos. La cadena `Scope` del resultado no cambia, porque el original ya tenía este ámbito. Los casos A-N dan los mismos paquetes y los mismos vectores de propiedades en los dos scripts (`run-selftest-matrix.txt`: `A_N_SameBundles` y `A_N_SameVectors` en true).

## 2. La propiedad 5
- Están atados: nota → delegación real → bfc (`DelegationSha256 == sha(real_deleg) == bfc.OutputSha256`) y relevo mutado → delegación mutada (`rr1 == sha(mut_deleg)`, con `diffpaths` solo en `OutputSha256`, regla F4 intacta).
- **No** está atado el relevo crudo → salida cruda: el script nunca lee `planning/output.json` (I-1).
- No comprueba la coherencia ni registra la identidad de la nota (I-2).
- Si la nota falta o está mal formada, el script lanza una excepción y no escribe salida (M-4). Si la nota es `null` o no se pasa, se aplica la regla de F4 y el resultado es FAIL.

## 3. La propiedad 8
- Las condiciones del bfc se leen del bfc **real** (l. 97). Como la propiedad 5 exige `real_bfc == mut_bfc` byte a byte, no es posible atestiguar un bfc distinto en la copia.
- Siguen exigidos: A3, A4 y A5 llanos, `ma != ca` y las 7 opacas iguales (l. 148-149).
- `AllowedWriteScope` exacto: la propiedad 8 solo exige su presencia en `Fields`. La igualdad la dan la propiedad 1 (`Utf8BytesEqual`, l. 102) y `ra == ca` (M-1).
- Basta una subcadena en cualquier entrada, aunque esté negada (I-3). Hoy hay una sola coincidencia y es la correcta.

## 4. La autoprueba
- 25/25 y determinista.
- P, Q, R, S, T, U, V y X rompen exactamente la propiedad declarada y nada más. W rompe además la 1 (M-1, ya reconocido en las notas).
- Faltan casos negativos (M-3).

## Hallazgos

| Id | Sev. | Línea | Hallazgo |
|---|---|---|---|
| I-1 | I | 125-129, 298, 315 | **El relevo crudo no se ata a la salida cruda.** `rr0.OutputSha256` solo se compara con `note.OutputSha256`, y las dos son afirmaciones de la sesión. `Injected` y `NonEmptyInControllerOutput` se aceptan por lo que dice la nota. Nunca se lee `planning/output.json`, ni se comprueba que la salida cruda más las 8 colecciones del contrato sea igual a `delegation.json`. En F4, el relevo quedaba atado a los bytes de la delegación, así que esto afloja la comprobación. **X2: relevo y nota con un SHA que no corresponde a ninguna salida, y `output.json` crudo = `{}` → PASS.** Corrección: leer la salida cruda (7.º argumento o junto a la nota); exigir sha(cruda) = nota = relevo; calcular las vacías desde la cruda; exigir cruda + 8 del contrato = delegación real. En los datos actuales se cumple. |
| I-2 | I | 126-129, 315, 318-328 | **La nota no se valida ni queda registrada.** No se comprueban `RunId`, `ContractSha256`, `Fidelity` ni `Decision`. **X1: valores inventados → PASS.** El resultado no registra la ruta ni el SHA de la nota (solo `Property5Rule`), así que el PASS depende de un archivo externo que no queda trazado. Corrección: exigir `note.RunId == rr0.RunId == bfc.PlanningRunId == rd.RunId` y `note.ContractSha256 == sha(contrato)`, y registrar `NotePath`, `NoteSha256` y `RawOutputSha256`. |
| I-3 | I | 13, 17-20, 146 | **Afloja la propiedad 8.** El original exigía pertenencia exacta (`EXACT_CLAUSE in ExpectedEvidence`). El adaptado acepta, en cualquier entrada, la subcadena normalizada (NFKD y minúsculas), aunque esté negada o hable de otra cosa. **X3: contrato coherente que dice «no se exige la comparación byte a byte de AllowedWriteScope; basta el subconjunto A3» → PASS.** Corrección: fijar como constante el texto exacto de `ExpectedEvidence[0]` (sin tilde en los datos reales) y exigir pertenencia exacta, conservando `EXACT_CLAUSE` como alternativa. |
| I-4 | I | 126 frente a 134 | **Semántica asimétrica del relevo.** El relevo real se valida con F4B-E21: `OutputSha256` es el SHA de la salida cruda. El relevo de nc2 se valida con F4: `OutputSha256` es sha(delegación mutada). Con F4B-E21, `OutputSha256` no es un hash derivado de `delegation.json`. La copia de nc2 cambió un campo no derivado, contradice sus propias `Notes` («el Controller dejó vacías las ocho colecciones…») y el motivo que da `mutation.json` («hash derivado de delegation.json»). El script no lo detecta ni lo documenta. **X4: una copia de nc2 fiel a F4B-E21 (relevo sin cambiar, solo `delegation.json`) da FAIL en la propiedad 5.** Hoy no tiene efecto observable, porque ninguna otra comprobación del Controller difiere. Pide decisión del Owner o del Coordinator, y una nota explícita en `Notes`, o bien aceptar las dos formas. |
| M-1 | M | 146 | `'AllowedWriteScope' in bfc_fields` queda dominada por la propiedad 1 y nunca se ejercita sola (W rompe también la 1). La propiedad 8 no mira `DecodedEqual`, `Differences` ni `SourceSha256 == OutputSha256` del campo. |
| M-2 | M | 283-284 | `hit` solo exige que falle la propiedad objetivo, no que las demás sigan en true. Por eso no se detecta que W rompe también la 1. |
| M-3 | M | 244-270 | Faltan casos negativos: nota con `RunId` o `ContractSha256` incoherentes; nota que coincide con el relevo pero no con la salida cruda; frase negada o en otra entrada; `Injected` con un elemento de más, duplicados o que no es una lista; nota ausente o mal formada. |
| M-4 | M | 298, 315, 329 | Si la nota falta (`FileNotFoundError`) o es una lista (`AttributeError`), el script sale con código 1 y no escribe salida. Falla en cerrado, pero no deja registro, y no borra un `out_path` anterior: podría quedar un PASS viejo. |
| M-5 | M | 122-124, 147 | El régimen de la propiedad 5 lo elige el argumento y el de la 8 lo eligen los datos: no están acoplados. Sin la nota, la propiedad 8 pasa por `BYTE_COMPARISON` (R2). |
| M-6 | M | 1, 4, 323-326 | El docstring dice «copia de F4T1B-…», cuando la fuente directa es `PBT2A-NC2-EXACT-WHITELIST-01`. `Notes.WhyOrdinaryOracleUnreachable` sigue hablando de la «cláusula de exactitud del contrato», que F4B no tiene. `adapt-notes.md` dice que la entrada [0] lleva tilde: es falso, el contrato dice `comparacion` sin tilde. El caso O sí la usa con tilde. |
| M-7 | M | 150-153 (ya estaba en el original) | El sustituto no afirma dos puntos de la firma: `EXECUTION_BLOCKED/STOP` y «ninguna otra comprobación distinta»; solo los registra. Comprobado a mano: solo difieren `Contract` y `Scope` (`firma-check.txt`). |

## Ejecuciones
Todas con cwd = `<checkout-principal>`, `python -I` y scripts escritos con Write en `review/`. BaseSha = `17872be8179475a3c080a9df158488a4a7b4f870` y CurrentSha = `5ddeb3b4…`. Además, en una línea de Bash se coló por error un `python -I -c "pass"`, que no hace nada y no leyó ni escribió nada.

| Ejecución | Resultado |
|---|---|
| `selftest_matrix.py` | 25/25, determinista; A-N idénticos al original; W rompe 1 y 8 (`run-selftest-matrix.txt`) |
| R1: adaptado con nota | **PASS**, 10/10, `Property5Rule` = F4B-E21, `ExactWhitelistBy` = BYTE_COMPARISON (`runs/R1-*`) |
| R2: adaptado sin nota | FAIL en la 5 (la 8 pasa) |
| R3: original | FAIL en la 5 y la 8 (reproduce el contraste) |
| M1: nota con otro SHA crudo | FAIL en la 5 |
| M2: bfc real y nc2 con `Result` = FAIL (`mutation.json` recalculado) | FAIL en la 8 |
| M3: bfc con `ElementwiseInOrder` = false | FAIL en la 8 |
| M4: contrato sin la frase (todo lo derivado recalculado de forma coherente) | FAIL en la 8 |
| M5: `Injected` sin `AllowedWriteScope` | FAIL en la 5 |
| M6 y M7: nota ausente / nota que es una lista | excepción, código 1, sin salida |
| **X1**: nota incoherente | **PASS** (I-2) |
| **X2**: relevo sin atar a la salida cruda | **PASS** (I-1) |
| **X3**: frase negada | **PASS** (I-3) |
| X4: nc2 fiel a F4B-E21 | FAIL en la 5 (I-4) |
| `inspect_data.py` y `firma_check.py` | cadena actual coherente; firma comprobada a mano |

Archivos: `review/{selftest_matrix.py, mutations.py, inspect_data.py, firma_check.py, run-selftest-matrix.txt}`, `review/runs/*` (incluido `mutations-summary.json`) y las copias mutadas en `review/mut/*`.

## Contraste con `adapt-notes.md`
Las notas ya reconocían tres puntos: la subcadena (I-3), que no se validan `RunId` ni `ContractSha256` (parte de I-2) y que W no está aislado (M-1). No mencionan:
- que la salida cruda nunca se lee (I-1);
- la asimetría del relevo de nc2 (I-4);
- que la nota no queda registrada en el resultado;
- el error de la tilde.

## §R. Recomprobación (script corregido, SHA-256 `4cd4c01f…a196`, 7 argumentos)

### Veredicto: APTO

| B | I | M |
|---|---|---|
| 0 | 0 | 3 |

Los tres M son nuevos y no bloquean. Los datos (`data/**`) no cambian respecto a la primera revisión.

### R.1 Estado de los hallazgos anteriores

| Id | Estado | Línea |
|---|---|---|
| I-1 | **Corregido.** Lee la salida cruda (l. 375). Exige que sha(cruda) = nota = relevo (l. 126 y 137). Calcula las colecciones vacías (l. 127 y 138). Exige que la cruda más las 8 colecciones del contrato sea igual a la delegación real (l. 128-133 y 139). X2 → FAIL. Lo que queda está en M-R1. | 117-139, 375-376 |
| I-2 | **Corregido.** Comprueba `RunId`, `ContractSha256`, `DelegationSha256`, `Fidelity` y `Decision` (l. 140-144), y registra la nota y la salida cruda con su ruta y su SHA (l. 374 y 376). X1 → FAIL. Lo que queda está en M-R2 y M-R3. | 140-144, 374, 376 |
| I-3 | **Corregido.** Exige la entrada exacta `F4B_EXACT_EVIDENCE` (l. 12 y 164). X3 → FAIL, y la misma frase con un espacio final (N10) → FAIL. | 12, 164 |
| I-4 | **Declarado** por decisión de la sesión (l. 3 y 400, `Nc2KitRelayRecompute`). X4 → FAIL, como se espera. | 3, 400 |
| M-1 | **Declarado** (descripción de W en l. 301; `adapt-notes.md` l. 36). | 301 |
| M-2 | **Corregido.** `EXPECT_FAILED` da el conjunto exacto de propiedades y `hit` lo compara por igualdad. | 322-334 |
| M-3 | **Corregido.** Casos Z y AA-AN. | 303-318 |
| M-4 | **Corregido.** Borra el resultado anterior (l. 357-358), usa `read_json_object` (l. 348-353) y captura la excepción (l. 368 y 403-404). M6 y M7 escriben un FAIL, salen con código 1 y sin stderr. | 348-358, 403-404 |
| M-5 | **Corregido.** Los modos se acoplan por los argumentos (l. 360-366). La regla de la propiedad 8 depende del modo (l. 161-166). Ver K1-K5. | 118-123, 161-166, 360-366 |
| M-6 | **Corregido.** Docstring (l. 1), `WhyOrdinaryOracleUnreachable` según el modo (l. 397-399) y error de la tilde (`adapt-notes.md` l. 28). | 1, 397-399 |
| M-7 | **Declarado** (`adapt-notes.md` l. 37). | — |

### R.2 El diff (original → corregido)
Cambian solo estas zonas:
- docstring;
- `ID`, `F4B_EXACT_EVIDENCE`, `INJECTED` y `up`;
- propiedad 5 (l. 117-151) y propiedad 8 (l. 160-168);
- sintético (l. 198-319);
- `EXPECT_FAILED` y `selftest` (l. 322-335);
- `read_json_object` y `main` (borrado del resultado anterior, modo, `try`, registro de la nota y la salida cruda, `Notes`);
- `argv[1:8]`.

Las propiedades 1-4, 6, 7, 9 y 10, los auxiliares, `load_inputs` y `git diff` son idénticos al original. El `import` vuelve a ser el del original. En el modo F4, las propiedades 5 y 8 son literalmente las del original.

### R.3 La autoprueba (`review/selftest_matrix2.py`)
- 40/40, determinista y sin desajustes.
- O-AN: el conjunto real de propiedades que fallan es igual a `EXPECT_FAILED` en los 26 casos, y cada conjunto corresponde a su descripción. W da {1, 8}; los demás negativos dan {5} o {8}; O da {}.
- A-N: los paquetes (con `note` y `raw_output` = None) y los vectores son idénticos a los del original. No tienen conjunto exacto porque se juzgan por el resultado global, igual que en el original.

### R.4 Ejecuciones (`review/mutations2.py`, copias en `review/mut2/`, salidas en `review/recheck/`)

| Caso | Esperado | Obtenido |
|---|---|---|
| R0: `data/*` con 7 argumentos | PASS | **PASS**; las propiedades 1-10 en true; igualdad JSON y de bytes en true |
| M1 (nota con otro SHA crudo), M5 (`Injected` incompleto) | FAIL | FAIL {5} |
| M2 (bfc con `Result` = FAIL), M3 (`ElementwiseInOrder` = false), M4 (contrato sin la frase) | FAIL | FAIL {8} |
| M6 (nota ausente), M7 (nota que es una lista) | FAIL escrito | FAIL escrito (`FileNotFoundError` / `ValueError`), código 1, sin excepción |
| X1 (nota incoherente), X2 (relevo sin atar a la salida cruda) | FAIL | FAIL {5} |
| X3 (frase negada) | FAIL | FAIL {8} |
| X4 (nc2 fiel a F4B-E21) | FAIL (I-4) | FAIL {5} |
| N10: frase exacta más un espacio | FAIL | FAIL {8} |
| K1: 5 argumentos (modo F4) | FAIL | FAIL {5, 8} |
| K2: solo la nota; K3: nota `''` y salida cruda | FAIL | FAIL (modo incompleto) |
| K4: salida cruda = `delegation.json`; K5: nota y salida cruda intercambiadas | FAIL | FAIL {5} |
| **N1**: en la salida cruda, `"Attempt": 2.0` (en la delegación, 2) | FAIL | **PASS** (M-R1) |
| **N2**: en la salida cruda, `"AttemptsRemaining": true` (en la delegación, 1) | FAIL | **PASS** (M-R1) |
| **N3**: `"Model"` duplicado en la salida cruda, con el primer valor distinto | FAIL | **PASS** (M-R1; también da true la igualdad de bytes que solo se mide) |
| N4: claves reordenadas en la salida cruda | PASS | PASS (aceptable: mismo contenido) |
| **N5**: la salida cruda sin la clave `AllowedWriteScope` y con `Objective` = `[]` | FAIL | **PASS** (M-R1) |
| **N6**: `RunId` ausente en la nota, el bfc y los dos relevos | FAIL | **PASS** (M-R2) |
| **N7**: `RunId` de la delegación y de la salida cruda distinto del del relevo, el bfc y la nota (todo recalculado) | FAIL | **PASS** (M-R2) |
| **N8**: `Fidelity` = `"FAIL (no PASS)"` | FAIL | **PASS** (M-R3) |
| N9: `F4B_EXACT_EVIDENCE` más otra entrada que releva la exactitud (coherente) | — | PASS (observación: igual que el original con `EXACT_CLAUSE`; no es una regresión y no cuenta) |

### R.5 Hallazgos nuevos

| Id | Sev. | Línea | Hallazgo |
|---|---|---|---|
| M-R1 | M | 125-134, 139 | **La igualdad JSON entre la cruda más las 8 colecciones y la delegación es laxa.** El `==` de Python da por iguales `2.0` y `2`, y `true` y `1` (N1, N2). Acepta una clave ausente o una colección de otro tipo vacía (N5). Además, `json.loads` se queda con la última de dos claves duplicadas, así que la salida cruda puede decir otra cosa en un campo no inyectado (N3), y ni siquiera la igualdad de bytes medida lo detecta. En los datos reales, `RawPlusContractCollectionsEqualsDelegationBytes` da true, pero solo se mide. Corrección sugerida: exigir esa igualdad de bytes; leer la salida cruda con un `object_pairs_hook` que rechace claves duplicadas; exigir que las 8 claves estén presentes con `''` y `[]` exactos. |
| M-R2 | M | 141 | **`RunId`.** `note.RunId == bfc.PlanningRunId == rr0.RunId` también da true si falta en los tres (`None == None == None`, N6). Además no se compara con `rd['RunId']`, que es el de la salida cruda (N7). F4 tampoco lo comparaba, así que no es una regresión. Corrección sugerida: exigir una cadena no vacía igual a `rd['RunId']`. |
| M-R3 | M | 144 | **`'PASS' in Fidelity` es una subcadena:** `"FAIL (no PASS)"` pasa (N8). El bfc `Result == 'PASS'` sigue exigiéndose en la propiedad 8, así que el efecto es menor. Corrección sugerida: `Fidelity.startswith('PASS')`. |

Ninguno de los tres permite un falso PASS sobre los datos actuales. En R0, todas las igualdades se cumplen de forma estricta: bytes iguales, sin claves duplicadas en ningún JSON de la cadena (`review/dupkeys_check.py`, `recheck/dupkeys-check.txt`) y `RunId` presente e igual en la nota, el bfc, el relevo y la delegación.

## Recomprobación v2 (`nc2_exact_whitelist_substitute_f4b_v2.py`, SHA-256 `C033E96C…8E45`)

Revisé la v2 frente a su base `base-6ccaf12d-original.py`, que es idéntica byte a byte al v1 custodiado `6ccaf12d…`. Material: `hs65\nc2-substitute-f4b-v2\`, las decisiones del Owner (`owner-pbt3a-nc2-decision.md`) y `git show` de solo lectura. Todo lo que generé está en `hs65\nc2-substitute-f4b-v2\review\`.

### Veredicto: APTO

| B | I | M |
|---|---|---|
| 0 | 0 | 4 |

Ninguno de los cuatro M da un falso PASS por un uso accidental. Para colar un PASS indebido hay que reetiquetar a mano los datos y recalcular sus hashes.

### V.0 Trazabilidad (observación, no cuenta)
Mi §R aprobó `4cd4c01f…`. El v1 custodiado, `6ccaf12d…`, incorporó después las correcciones M-R1, M-R2 y M-R3 (l. 147-177, casos AO-AX), que yo no había revisado. Las reviso ahora y son correctas:
- AO-AX rompen solo la propiedad 5.
- Mis sondas N1, N2, N3, N5, N6 y N8 de la §R, repetidas con la v2 sobre copias coherentes de PB-T3a, dan todas FAIL (`review/probes-mr/`). N3, la clave duplicada, falla al leer la salida cruda y escribe un FAIL con el motivo.

### V.1 La propiedad 8: lista cerrada (l. 17-22 y 197-201)
- Es una tupla de 2 literales `str`, distintos y en ASCII.
- **PB-T2** = `ExpectedEvidence[0]` en los contratos de PB-T2a (las copias de la v1 y de la v2, y `git show c40eea4a`), de PB-T2b y de PB-T2c.
- **PB-T3** = `ExpectedEvidence[0]` del contrato de `485b5651` (`git show`, SHA `B37FF5E9…`, igual a la copia y a `c40eea4a`), igual en la cadena decodificada y presente en los bytes. Es la única entrada de PB-T3a que contiene «byte a byte» (`review/verify-literals-own.txt`).
- La igualdad es entera y exacta: `lit in ee` sobre una lista compara cadenas completas con `==`.
- **No se cuela ninguna variante.** Dan FAIL {8}, en la autoprueba o en mis sondas:
  - sin «ocho»;
  - con un espacio doble, o con un espacio final;
  - con «.» en vez de «;»;
  - con la inicial en mayúscula;
  - mezcla de los literales de T2 y T3;
  - con un NBSP en lugar de un espacio (S4);
  - con una «а» cirílica (S5);
  - con «comparación» con tilde (S6).
- El resto de la propiedad 8 (bfc, A3-A5, `ma != ca` y las opacas) es idéntico al v1.

### V.2 `--task` y VALIDATION (l. 14, 409-415, 452-466, 490-494, 508, 519-528)
- `check_task` exige que el `TaskId` empiece por `F4B-<TAREA>-`. `PBT3` no casa con `PBT3A` (T3).
- La ligadura se comprueba sobre las dos delegaciones (`ok = ok and bind`).
- Dan FAIL escrito: sin `--task`; con una tarea equivocada (S11, y `checks/` del autor); con `--task` al final sin valor (S12).
- En minúsculas se acepta (S10).
- VALIDATION: cualquier `TaskId` que empiece por `F4B-`, sin registro.
- La propiedad 5 y el modo están intactos.

### V.3 El diff (v1 → v2, 19 bloques)
Solo cambian:
- el docstring;
- `ID` → `ID_GENERIC` y `TASKS`;
- la tupla de literales;
- las l. 197-201 de la propiedad 8;
- el sintético: el renombrado `EV_T2`/`EV_T3` y los casos AY-BE;
- `EXPECT_FAILED`, `check_task` y T1-T6;
- `main`: `task`, `Id`, `Scope` tomado de los datos, `ScriptSha256`, `TaskBinding` y `Record`;
- el análisis de argumentos.

Las propiedades 1-7, 9 y 10, la regla de la 8 fuera de la tupla y los auxiliares son idénticos al v1.

### V.4 La autoprueba (`review/selftest_matrix_v2.py`)
- **63/63** (57 casos sintéticos y T1-T6), determinista y sin desajustes.
- A-AX (50 casos): mismos paquetes y mismos vectores que el v1.
- AY da PASS, con el literal PB-T3.
- AZ, BA, BB, BC, BD y BE fallan exactamente {8}. En cada uno, la entrada difiere del literal de PB-T3 solo en lo que dice su descripción, comprobado con difflib (BE es T2 + «ocho»).
- T1-T6 dan el resultado declarado.

### V.5 Los casos reales (`review/runs/`)

| Caso | Resultado |
|---|---|
| v2, `--task PBT2A`, PB-T2a | **PASS**, 10/10, PB-T2, bytes iguales, ligadura correcta, `SATISFIED_UNDER_PBT2A_…`, autoprueba 63/63 |
| v2, `--task PBT3A`, PB-T3a | **PASS**, 10/10, PB-T3, bytes iguales, ligadura correcta, `SATISFIED_UNDER_PBT3A_…`, autoprueba 63/63 |
| Contraste v1 `6ccaf12d` | PB-T2a PASS; PB-T3a FAIL solo en la 8 (lo que se reportó) |
| Firma PD-F4-01 | En los dos nc2 solo difieren `Contract` y `Scope` respecto de la verificación real, con `EXECUTION_BLOCKED` / `STOP` / `Contract`. Las condiciones de parada: S-04 y PBT2A-C01 en PB-T2a; S-04, P-03 y PBT3A-C01 en PB-T3a. |

### V.6 Sondas propias (`review/probes_v2.py` → `review/probes/`)

| Sonda | Resultado |
|---|---|
| S1: el literal pasa al final de `ExpectedEvidence` | PASS (observación O-1) |
| S2: contrato de PB-T3a con el literal de PB-T2 y `--task PBT3A` | PASS (observación O-2) |
| S3: el literal escrito con el escape `a` en los bytes | PASS (observación O-3; mismo texto) |
| S4, S5, S6: variantes con NBSP, homoglifo o tilde | FAIL {8} |
| **S7**: el contrato dice `TaskId` PB-T3b y la delegación PB-T3a | **PASS** (M-V1) |
| **S8**: delegaciones y salida cruda reetiquetadas a PB-T3b; contrato, relevos, bfc y salidas siguen en PB-T3a; `--task PBT3B` | **PASS**, con `SATISFIED_UNDER_PBT3B_…` sobre datos de PB-T3a (M-V1) |
| **S9**: `--task VALIDATION` sobre PB-T3a | **PASS**, código 0, `Record` = `VALIDATION_ONLY_NO_RECORD` (M-V2) |
| S10: `--task pbt3a` | PASS |
| S11: `--task PBT3A` sobre PB-T2a | FAIL (ligadura) |
| S12: `--task` sin valor | FAIL |
| S13: `--task` repetido | PASS con el último valor (observación O-4) |
| S14: control PB-T2a | PASS |

### V.7 Hallazgos

| Id | Sev. | Línea | Hallazgo |
|---|---|---|---|
| M-V1 | M | 409-415, 490 | **La ligadura de `--task` mira solo el `TaskId` de las dos delegaciones, y por prefijo.** No lo compara con el del contrato, la salida cruda, los dos relevos, el bfc, `worker-handoff.json` ni las dos `output.json`, aunque todos lo llevan. Con S7 y S8, coherentes, se obtiene PASS, y S8 registra `SATISFIED_UNDER_PBT3B` sobre datos de PB-T3a. El uso accidental de una tarea equivocada sí se detecta (S11). Corrección sugerida: exigir el mismo `TaskId` en todos esos archivos, además del prefijo. |
| M-V2 | M | 464, 499, 508, 515 | **VALIDATION devuelve `SubstituteResult` = `PASS` y código 0** sobre datos reales de una tarea (S9). Solo lo distinguen `Id` = `PBTx-…` y `Record`. Corrección sugerida: `PASS (VALIDATION, sin registro)` o un código de salida propio. |
| M-V3 | M | 8 | El docstring tiene una segunda línea «Uso» desfasada: el nombre de archivo del v1 y sin `--task`. Contradice la l. 3. |
| M-V4 | M | 424-430 | T1-T6 prueban solo el predicado `check_task`. La integración en `main` (la ligadura de las dos delegaciones, `ok = ok and bind`, `Id` y `Record` según la tarea) no está en la autoprueba; la cubren solo corridas externas (`checks/`, S9-S13). |

Observaciones que no cuentan:
- **O-1:** el literal se acepta en cualquier posición, no solo en [0] como dice el comentario de la l. 16. Es igual que en el v1.
- **O-2:** un literal de la lista vale para cualquier `--task`. No es una variante libre y encaja con «un paquete nuevo con otra redacción exige añadir su literal». Si se quiere «uno por paquete» en sentido estricto, hay que ligar cada `TASKS` a su literal.
- **O-3:** la igualdad es sobre la cadena decodificada, no sobre los bytes del contrato.
- **O-4:** si `--task` se repite, gana el último; los argumentos posicionales de más se ignoran.
- **O-5:** los contratos de PB-T3b y PB-T3c aún no están en git (`c40eea4a`). El literal de PB-T3 procede de `make_pbt3a_contract.py` (l. 1151). Si su generador lo reproduce byte a byte, pasarán; si no, darán FAIL, como se pretende.

Avisos sobre mis propias ejecuciones:
- En las rondas anteriores, `importlib` dejó cachés de bytecode fuera de `review/`: `pbt2acc2\nc2-substitute-f4b\__pycache__` y `data\__pycache__`. No los borré.
- En esta ronda usé `-I -B` y no escribí nada fuera de `nc2-substitute-f4b-v2\review\` ni de este informe. `git status` del checkout principal está limpio.
- En una línea de Bash se coló otra vez un `python -I -B -c "pass"`, que no hace nada.
