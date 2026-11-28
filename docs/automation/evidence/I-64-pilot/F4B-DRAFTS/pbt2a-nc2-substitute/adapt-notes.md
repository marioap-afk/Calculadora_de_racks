# Sustituto de nc2 adaptado a F4B (PREPARACION, no aplicado, no custodiado) - version corregida tras la revision independiente

Script: `nc2_exact_whitelist_substitute_f4b.py` (id `PBT2A-NC2-EXACT-WHITELIST-F4B-01`). Diff frente a `data/pbt2a-substitute-as-run.py`: `adapt-diff.txt` (29 hunks; 20 lineas quitadas, 212 anadidas, la mayoria autoprueba).
Registro si PASS: `SATISFIED_UNDER_PBT2A_EXACT_WHITELIST_SUBSTITUTE_F4B`. Ambito: solo F4B-PBT2A-DRAFT-STATE-PORT, intento 2, verificacion R20261119T074453Z-10a9.
Uso: `python -I nc2_exact_whitelist_substitute_f4b.py <real> <nc2> <BaseSha> <CurrentSha> <salida.json> [<session-inject-note.json> <planning/output.json>]`

## Resultado de la vista previa (7 argumentos): PASS
Modo F4B-E21. Autoprueba 50/50 con el resultado esperado, determinista (tras la recomprobacion §R: M-R1, M-R2, M-R3). Las 10 propiedades en true (5: salida cruda SHA `100A7695...` = relevo = nota; 8: `F4B_EXACT_EVIDENCE+BFC`).
Registrados en el resultado: `InjectNote` (ruta y SHA `AA17232A...`) y `RawControllerOutput` (ruta y SHA `100A7695...`). Salidas: `preview-run.txt`, `preview-result.json`.
Contraste: el sustituto de F4 original da FAIL en 5 y 8 (`contrast-run.txt`). El sustituto nuevo sin nota ni salida cruda (modo F4 exacto) tambien da FAIL en 5 y 8 sobre los datos F4B, como debe.

## Recomprobacion §R: M-R1, M-R2, M-R3 (segunda correccion)
- **M-R1.** (a) La igualdad salida cruda + 8 colecciones del contrato == delegation.json pasa a EXIGIDA tambien byte a byte (`json.dumps(indent=2, ensure_ascii=False) + '\n'`, UTF-8, orden de la salida cruda, como `inject_opaque.py`); sigue exigida la igualdad JSON. (b) `loads_strict` (`object_pairs_hook`) rechaza claves duplicadas al leer la nota y la salida cruda (en `main`: FAIL con motivo) y la salida cruda y la delegacion real (en la propiedad 5: false). (c) Las 8 claves deben estar en la salida cruda, con `''` exacto en Objective y `[]` exacto (tipo lista) en las otras 7 (`RawCollectionsEmptyExact`).
- **M-R2.** `RunId` cadena no vacia e igual en la nota, `bfc.PlanningRunId`, `rr0.RunId` y `delegation.json` real.
- **M-R3.** `Fidelity.startswith('PASS')`.
- Casos nuevos (cada uno falla SOLO la propiedad 5; no hay solape): AO clave duplicada en la salida cruda (la ultima coincide con la delegacion); AP `Attempt = 2.0` en la salida cruda vs `2` (iguales por JSON, distintos en bytes); AQ `AttemptsRemaining = true` vs `1` (idem); AR sin la clave AllowedWriteScope; AS `Objective = []`; AT `Invariants = ""`; AU RunId vacio en todo; AV RunId ausente en todo; AW RunId de la delegacion distinto del resto; AX Fidelity `FAIL (no PASS)`. Los 26 anteriores siguen con su resultado esperado.
- Consecuencia declarada: una salida cruda con las mismas claves en otro orden (N4 de la revision, antes aceptable) ahora da FAIL en la 5, porque la igualdad de bytes exige el orden de `inject_opaque.py`.
- Consecuencia en N9 (observacion heredada de la revision): la copia de la revision no es reproducible byte a byte desde salida cruda + contrato, asi que ahora da FAIL en la 5; la propiedad 8 sigue en true en esa corrida (la observacion sobre la 8 no cambia).

## Sondas de la recomprobacion (`probes-after-fix2.txt`, copias de `review/mut2/`, solo lectura)
R1 (datos reales, 7 args) PASS. N1, N2, N3, N5, N6, N7 y N8 dan FAIL: N1, N2, N5, N6, N7, N8 en la propiedad 5; N3 con resultado FAIL de entrada mal formada (clave duplicada) escrito por `main`, sin excepcion. Ademas N4 FAIL (5), N10 FAIL (8), X1-X4 FAIL, M6/M7 FAIL escrito; N9 FAIL (5, ver arriba).

## Sondas de la primera revision tras la correccion (`probes-after-fix.txt`, version anterior del script)
- X1 (nota incoherente): FAIL en la 5.
- X2 (relevo y nota sin atar a la salida cruda): FAIL en la 5.
- X3 (frase negada): FAIL en la 8.
- X4 (copia de nc2 fiel a F4B-E21): FAIL en la 5, por decision I-4 (no se acepta).
- M6 (nota ausente) y M7 (nota que es una lista): resultado FAIL escrito con el motivo, sin excepcion.

## Que cambio (por hallazgo)
- **I-1 (relevo atado a la salida cruda).** Modo F4B-E21 = nota Y salida cruda, ambas obligatorias (una sola da FAIL de modo incompleto). Propiedad 5 exige: SHA(salida cruda) == note.OutputSha256 == rr0.Outcome.OutputSha256; las 8 colecciones, vacias en la salida cruda (calculado con la misma regla que `inject_opaque.py`: `raw.get(k)` falsy) y `note.NonEmptyInControllerOutput` igual a lo calculado (= []); salida cruda con las 8 colecciones del contrato == delegation.json real por igualdad JSON; `note.Injected` exactamente las 8, sin repetidos ni extras. La igualdad byte a byte (serializacion de `inject_opaque.py`) paso a EXIGIDA en la segunda correccion (M-R1, abajo); en la primera solo se media.
- **I-2 (nota entera).** note.RunId == bfc.PlanningRunId == rr0.RunId; note.ContractSha256 == SHA(contrato); note.DelegationSha256 == SHA(delegacion real) == bfc.OutputSha256; Fidelity contiene PASS; Decision empieza por `F4B-E21`. Resultado registra ruta y SHA de la nota y de la salida cruda. (rr0.RunId es mi adicion a la lista: refuerza la atadura nota-relevo.)
- **I-3 (propiedad 8 exacta).** `F4B_EXACT_EVIDENCE` = literal completo de `ExpectedEvidence[0]` del contrato real (verificado `in` y en indice 0); `ExpectedEvidence` debe contenerlo como entrada exacta, mas bfc.Result PASS, AllowedWriteScopeElementwiseInOrder true y AllowedWriteScope en Fields. Se quito la subcadena normalizada (y `unicodedata`).
- **I-4 (kit de nc2).** Sin logica nueva: rr1.Outcome.OutputSha256 == SHA(delegacion mutada), en los dos modos. Declarado en el docstring y en `Notes.Nc2KitRelayRecompute`. Una copia fiel a F4B-E21 da FAIL (X4); seria otra mutacion de kit.
- **M-2.** Cada caso nuevo declara el conjunto EXACTO de propiedades que deben fallar (vacio si pasa); la autoprueba lo exige. Solo W es {1, 8}, por M-1.
- **M-3.** Casos nuevos (ver abajo).
- **M-4.** El script borra `out_path` al empezar; si la nota o la salida cruda faltan o no son un objeto JSON (o cualquier otra entrada ilegible) escribe un resultado FAIL con el motivo y sale con codigo 1, sin excepcion.
- **M-5.** Modos acoplados y registrados en `Mode`: con nota + salida cruda, F4B-E21 (5 y 8 con reglas F4B; la 8 NO acepta EXACT_CLAUSE); sin ninguna, F4 exacto (la 5 y la 8 como el original; la 8 NO acepta la evidencia F4B); solo una, FAIL.
- **M-6.** Docstring con la fuente correcta (PBT2A-NC2-EXACT-WHITELIST-01, copia de F4T1B-NC2-EXACT-WHITELIST-01); `Notes.WhyOrdinaryOracleUnreachable` depende del modo; la entrada [0] del contrato NO lleva tilde ("comparacion"). Error mio corregido; el caso sintetico que usaba tilde ya no existe (ahora usa el literal exacto).

## Casos de la autoprueba
A-N: sin tocar, resultados identicos. Nuevos (O-AN), propiedad que debe fallar entre parentesis:
O pasa. P nota.OutputSha256 distinto (5). Q DelegationSha256 distinto (5). R Injected con 7 (5). S nota dice NonEmpty no vacio con la salida cruda vacia (5). T contrato sin evidencia exacta ni EXACT_CLAUSE (8). U bfc con ElementwiseInOrder false (8). V bfc con Result FAIL (8). W bfc sin AllowedWriteScope en Fields (1 y 8). X relevo de nc2 sin recalcular (5). Y modo F4B con solo EXACT_CLAUSE (8). Z modo F4 con la evidencia F4B sin EXACT_CLAUSE (8). AA relevo distinto de la salida cruda, nota igual (5). AB relevo y nota iguales entre si pero no a la salida cruda leida, la sonda X2 (5). AC coleccion no vacia en la salida cruda, nota honesta (5). AD igual, nota que dice vacio (5). AE salida cruda + colecciones del contrato != delegacion real (5). AF Injected con un elemento de mas (5). AG Injected con un repetido (5). AH RunId de la nota incoherente (5). AI ContractSha256 incoherente (5). AJ Fidelity sin PASS (5). AK Decision que no empieza por F4B-E21 (5). AL entrada de ExpectedEvidence parecida pero negada (8). AM nota sin salida cruda (5). AN salida cruda sin nota (5).
No aislados / sin caso propio: W (solapa con la propiedad 1, M-1) y `note.DelegationSha256 == bfc.OutputSha256` (solapa con la 1, que ya compara bfc.OutputSha256 con SHA(delegacion)); no se fuerza.

## Declarados sin cambio de codigo
- **M-1:** `'AllowedWriteScope' in bfc.Fields` queda dominada por la propiedad 1 y no se ejercita sola; la 8 no mira DecodedEqual/Differences/SourceSha256 del campo.
- **M-7 (heredado del original):** el sustituto no afirma `EXECUTION_BLOCKED/STOP` ni "ninguna otra comprobacion distinta"; solo registra Classification/Disposition/FailureClass/Checks. La sesion los comprueba con su tabla de comparacion, fuera del sustituto (la revision lo comprobo a mano: solo difieren Contract y Scope).

## Dudas
- La exigencia byte a byte acopla el sustituto al orden de claves y a la serializacion de `inject_opaque.py`; si la sesion cambia ese script, el sustituto deberia revisarse.
- La nota y la salida cruda duplican claves se rechazan en `main` (FAIL sin Properties); la delegacion real y la salida cruda, ademas, en la propiedad 5.
- El texto "Contract, comprobacion 4" en `Notes` se hereda del original; no lo he reverificado para F4B.
- Los scripts `run_probes.py` y el directorio `probes-after-fix/` los anadi para ejecutar las sondas; `__pycache__` en la raiz es de la revision.
