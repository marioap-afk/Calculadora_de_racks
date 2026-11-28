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
