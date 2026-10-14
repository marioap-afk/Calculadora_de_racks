# Sondas de la ronda 4 de A-6: cierre del aceptante en N4 y nc1 (I-62, decisiones §85)

Banco de la supervisión para la corrección de A62-A6R3-R1: el `Closure` de la solicitud del aceptante cuando falta la entrega (N4) o cuando
un commit que nombra no existe (nc1), derivado de la tabla de A6-9 aplicada con A6-3, regla 6 (b), de la ronda 4. Aquí solo hay pruebas y
resultados; el texto de la enmienda lo produce `../text/make_r4_text.py`. No se invocó ningún modelo ni CLI de modelo (ni Codex ni claude), no
hubo red y no se leyó `auth.json`, credenciales ni `config.toml`. Todo Python corrió con `-I`, las órdenes en secuencia.

## 1. Qué se reutiliza y qué es nuevo

| Archivo | Origen |
|---|---|
| `lib_r1r4.py`, `lib_r3.py`, `reconstruct_runs.py`, `table_r3.py` | copias byte a byte de `../../r3/r1probes/` (SHA-256 en §8): forma de ejecución declarada, registro en formato Codex, reconstrucción de `Runs`, salidas sintéticas del Controller, construcción de la solicitud y tabla de aceptación de la ronda 3 |
| `derive_r4.py` | nuevo: la derivación del cierre con la regla corregida (tabla de A6-9 y A6-3, regla 6 (b) (i)-(v)), con el registro de cada término omitido y su causa |
| `table_r4.py` | nuevo: `table_r3.evaluate` sin cambio más el cotejo del `Closure` de la solicitud y de su registro de derivación con una derivación nueva desde el vector custodiado (O2 de la ronda 3, para el campo que corrige la ronda 4) |
| `lib_r4.py` | nuevo: escenario, cierres efectivos E, invocaciones I y vectores de la derivación |
| `make_readme_r4.py` | nuevo: genera las tablas de este README desde los archivos de resultados, sin copiarlas a mano |
| `run_r4.py` | nuevo: las sondas (`python -I run_r4.py all`, o un grupo: `derive`, `v`, `n4`, `nc1`, `indep`, `summary`) |
| `indep/` | la implementación independiente (`derive_indep.py`), escrita por un agente distinto solo desde `indep/SPEC.md`, con sus vectores sintéticos y sus resultados |

**Escenario.** El de la evidencia (d) de la ronda 2 (`../../r2/independence/`): la verificación de T1 sobre la entrega simulada del fixture
(BASE = TIP `95bdc29d`, RED `53436cb2`, GREEN = G `b417d36c`, EFF = MAIN `fbe25347`, AuthorityRevision `1746b404`), con su cierre efectivo
EIC-VERIFY y el `Target` de la verificación real (`{G, src/Fixture.Lib/Calculator.cs}`). Cada grupo trabaja sobre una copia propia del
repositorio de la entrega (`D:\r62-a6r2\r3\deliv`, solo lectura) en `D:\r62-a6r4\r1\c-*`, con las refs conservadas; en cada copia se hace la
escritura del objeto suelto del instrumento (P1) y la comprobación del punto de uso (P2) antes de la primera corrida.

**Entradas de cada invocación** (RAE §10: copias de las entradas que conservan sus `RunId` internos salvo el campo mutado). K y el TRX son
byte a byte los de la ronda 2; la delegación y la entrega son las de la ronda 2 con la ruta de esta copia como `ExpectedWorktree` y `Worktree`.
E es el EIC-VERIFY custodiado de la ronda 2 con las tres entradas fuera de Git sustituidas por las de la invocación: los controles usan su
propia ruta (`T1-n4`, `T1-nc1`). N4 sin entrega: E no tiene entrada de la entrega (un archivo que no existe no tiene blob); la variante STALE fija la
ruta con el blob de la entrega real, aunque el archivo no exista. nc1: entrega con `CurrentSha = deadbeef…deadbeef` (40 hexadecimales, no es un
objeto de la copia; se comprueba). I: `UnitId FX-U1`, `TaskId T1`, `Attempt 0`, el `RunId` de cada invocación y el `Target` de la verificación
real. La identificación de D, H y K entre las entradas de E la da el vector (`Designation`, las rutas que el kit da a la invocación), como en la
evidencia (d) de la ronda 2.

**Corridas reales.** Como en la ronda 3: cada orden del instrumento se ejecuta por la forma declarada con pwsh 7 (cwd = la copia), el registro
de sesión se escribe en el formato de Codex, `Runs` se reconstruye de ese registro y la solicitud se construye con `table_r3.construct`, con el
`Closure` derivado por `derive_r4.py`. El aceptante fijado corre en solo lectura sobre la misma copia, con la comprobación de cero escrituras y
de procesos. Las salidas del Controller son sintéticas (las mismas reglas y textos que la ronda 3).

## 2. Derivación sin el aceptante (`p7-derive.json`, grupo `derive`)

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| D-R2 | Vector de la ronda 2 (EIC-VERIFY, D, H y K de r2/independence (d)) con la regla corregida | cierre identico al custodiado de la ronda 2 (EIC-VERIFY.bmin-closure.json); un solo termino omitido: D.ChainRedSha (null) | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL | sí |
| D-R2-STRICT | Vector de la ronda 2 con la variante I6 estricta de E | cierre identico al custodiado de la ronda 2 (EIC-I6-STRICT) | 7 commits, 79 RevPaths, 47 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL | sí |
| D-V | Verificacion real (T1 sobre la entrega): copias de D y H con la ruta de este clon | igual al cierre de la ronda 2 salvo los SHA-256 de DELEG y H, que cambian con la ruta del clon | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL | sí |
| D-N4-ABSENT | E sin entrada de la entrega (el kit no puede fijar el blob de un archivo que no existe) | H ausente: se omiten H.BaseSha, H.CurrentSha, H.RedSha, RevList(D.BaseSha..H.CurrentSha) y C6 H con INPUT_ABSENT; RED fuera (solo lo aportaban H); G dentro por I.Target.Commit; TIP por D.BaseSha; ningun commit fuera de los de la verificacion real | 6 commits, 75 RevPaths, 47 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.BaseSha INPUT_ABSENT; H.CurrentSha INPUT_ABSENT; H.RedSha INPUT_ABSENT; RevList INPUT_ABSENT; C6.H INPUT_ABSENT | sí |
| D-N4-STALE | E fija la ruta de la entrega con el blob de la entrega real, pero el archivo no existe | H ausente: se omiten H.BaseSha, H.CurrentSha, H.RedSha, RevList(D.BaseSha..H.CurrentSha) y C6 H con INPUT_ABSENT; RED fuera (solo lo aportaban H); G dentro por I.Target.Commit; TIP por D.BaseSha; ningun commit fuera de los de la verificacion real | 6 commits, 75 RevPaths, 48 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.BaseSha INPUT_ABSENT; H.CurrentSha INPUT_ABSENT; H.RedSha INPUT_ABSENT; RevList INPUT_ABSENT; C6.H INPUT_ABSENT | sí |
| D-N4-STALE-vs-ABSENT | Diferencia entre las dos formas de E en N4 | solo la ruta de la entrega en C4 (es una entrada de E en STALE); los commits, C3, C2 y C6 iguales | {"Paths": {"Extra": ["artifacts/orchestration/FX-U1/T1-n4/0/R20261012T091000Z-a001/worker-handoff.json"], "Missing": []}} | sí |
| D-N4-BLOB | bytes de la entrega con otro blob que el que fija E | H ausente por la regla (i): las mismas omisiones y el mismo cierre que D-N4-STALE | 6 commits, 75 RevPaths, 48 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.BaseSha INPUT_ABSENT; H.CurrentSha INPUT_ABSENT; H.RedSha INPUT_ABSENT; RevList INPUT_ABSENT; C6.H INPUT_ABSENT; causa de la entrada: bytes do not have the pinned blob | sí |
| D-N4-NOTJSON | entrega truncada (no es JSON) | H ausente por la regla (i): las mismas omisiones y el mismo cierre que D-N4-STALE | 6 commits, 75 RevPaths, 48 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.BaseSha INPUT_ABSENT; H.CurrentSha INPUT_ABSENT; H.RedSha INPUT_ABSENT; RevList INPUT_ABSENT; C6.H INPUT_ABSENT; causa de la entrada: not JSON: Expecting value: line 26 column 15 (char 839) | sí |
| D-N4-DUPKEY | entrega con una clave repetida | H ausente por la regla (i): las mismas omisiones y el mismo cierre que D-N4-STALE | 6 commits, 75 RevPaths, 48 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.BaseSha INPUT_ABSENT; H.CurrentSha INPUT_ABSENT; H.RedSha INPUT_ABSENT; RevList INPUT_ABSENT; C6.H INPUT_ABSENT; causa de la entrada: repeated key TaskId | sí |
| D-N4-BOM | entrega con BOM UTF-8 | H ausente por la regla (i): las mismas omisiones y el mismo cierre que D-N4-STALE | 6 commits, 75 RevPaths, 48 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.BaseSha INPUT_ABSENT; H.CurrentSha INPUT_ABSENT; H.RedSha INPUT_ABSENT; RevList INPUT_ABSENT; C6.H INPUT_ABSENT; causa de la entrada: BOM | sí |
| D-N4-ARRAY | entrega que es un arreglo JSON, no un objeto | H ausente por la regla (i): las mismas omisiones y el mismo cierre que D-N4-STALE | 6 commits, 75 RevPaths, 48 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.BaseSha INPUT_ABSENT; H.CurrentSha INPUT_ABSENT; H.RedSha INPUT_ABSENT; RevList INPUT_ABSENT; C6.H INPUT_ABSENT; causa de la entrada: not an object | sí |
| D-NC1 | nc1: H.CurrentSha = deadbeef... (40 hex, no es un objeto del clon) | H.CurrentSha NOT_A_COMMIT y RevList PRIMITIVE_EMPTY; RED sigue dentro por H.RedSha y G por I.Target.Commit: mismos commits que la verificacion real; C6 H = SHA-256 de la entrega mutada | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha NOT_A_COMMIT; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-MISSING | H sin el campo CurrentSha | se omiten H.CurrentSha (FIELD_MISSING) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha FIELD_MISSING; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-NULL | H.CurrentSha = null | se omiten H.CurrentSha (FIELD_NULL) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha FIELD_NULL; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-EMPTY | H.CurrentSha = "" | se omiten H.CurrentSha (FIELD_EMPTY) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha FIELD_EMPTY; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-UPPER | H.CurrentSha = G en mayusculas | se omiten H.CurrentSha (NOT_A_COMMIT) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha NOT_A_COMMIT; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-ABBREV | H.CurrentSha = prefijo de 12 caracteres de G | se omiten H.CurrentSha (NOT_A_COMMIT) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha NOT_A_COMMIT; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-TREE | H.CurrentSha = el arbol de G (objeto existente, no commit) | se omiten H.CurrentSha (NOT_A_COMMIT) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha NOT_A_COMMIT; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-BLOB | H.CurrentSha = un blob existente | se omiten H.CurrentSha (NOT_A_COMMIT) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha NOT_A_COMMIT; RevList PRIMITIVE_EMPTY | sí |
| F-CUR-REF | H.CurrentSha = "HEAD" (una ref, no un nombre de commit) | se omiten H.CurrentSha (NOT_A_COMMIT) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.CurrentSha NOT_A_COMMIT; RevList PRIMITIVE_EMPTY | sí |
| F-RED-MISSING | H sin el campo RedSha | se omite H.RedSha (FIELD_MISSING); RED sigue dentro por RevList(D.BaseSha..H.CurrentSha): mismos commits que la verificacion real | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.RedSha FIELD_MISSING | sí |
| F-RED-NULL | H.RedSha = null | se omite H.RedSha (FIELD_NULL); RED sigue dentro por RevList(D.BaseSha..H.CurrentSha): mismos commits que la verificacion real | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.RedSha FIELD_NULL | sí |
| F-RED-EMPTY | H.RedSha = "" | se omite H.RedSha (FIELD_EMPTY); RED sigue dentro por RevList(D.BaseSha..H.CurrentSha): mismos commits que la verificacion real | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; H.RedSha FIELD_EMPTY | sí |
| D-D-ABSENT | Delegacion no fijada por E (generalidad de la regla (i)) | todos los terminos de D omitidos con INPUT_ABSENT, incluida la ref de C2 y C6 DELEG; sin la cadena de primer padre de D.MainSha | 5 commits, 67 RevPaths, 47 rutas, 3 archivos; omitidos: D.BaseSha INPUT_ABSENT; D.ChainBaseSha INPUT_ABSENT; D.ChainRedSha INPUT_ABSENT; D.MainSha INPUT_ABSENT; FirstParentChain(D.MainSha) INPUT_ABSENT; MergeBase INPUT_ABSENT; RevList INPUT_ABSENT; C2.origin/D.ExpectedBranch INPUT_ABSENT; C6.DELEG INPUT_ABSENT | sí |
| D-K-ABSENT | Contrato no fijado por E (generalidad de la regla (i)) | K.AuthorityRevision, K.MainSha, MergeBase, las rutas de K.Authorities y C6 CONTRACT omitidos con INPUT_ABSENT | 7 commits, 86 RevPaths, 47 rutas, 3 archivos; omitidos: D.ChainRedSha FIELD_NULL; K.AuthorityRevision INPUT_ABSENT; K.MainSha INPUT_ABSENT; MergeBase INPUT_ABSENT; C4.K.Authorities INPUT_ABSENT; C6.CONTRACT INPUT_ABSENT | sí |
| D-TARGET-NX | I.Target.Commit inexistente | I.Target.Commit NOT_A_COMMIT; G sigue dentro por H.CurrentSha | 7 commits, 89 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; I.Target.Commit NOT_A_COMMIT | sí |
| D-MB-EMPTY | K.AuthorityRevision = una raiz sin relacion con D.MainSha (MergeBase sin base comun) | MergeBase PRIMITIVE_EMPTY; la raiz entra por K.AuthorityRevision; nada sustituye la base comun ausente | 8 commits, 86 RevPaths, 48 rutas, 4 archivos; omitidos: D.ChainRedSha FIELD_NULL; MergeBase PRIMITIVE_EMPTY | sí |

## 3. Cotejo: sustituciones, insumos añadidos o inventados (`p7-derive.json`, grupo `neg`)

El cotejo de `table_r4.py` deriva de nuevo el cierre desde el vector normativo y compara, campo a campo, el cierre que fija la solicitud
(elementos de más = insumos añadidos o inventados; de menos = omitidos sin regla) y el registro de derivación custodiado (términos y causas).
El aceptante no se ejecuta en este grupo.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| S-N4-REALH | N4: la entrega ausente se sustituye por la entrega real de la verificacion (una copia valida) | el cotejo rechaza: RED, la ruta de H y C6 H de mas (el cierre no es el derivado) y el registro no declara H ausente | rechazo: CLOSURE_NOT_DERIVED, DERIVATION_RECORD_DIFFERS; diferencias del cierre: Commits +1/-0, Files +1/-0, Paths +1/-0, RevPaths +14/-0 | sí |
| S-N4-TARGET | N4: la supervision completa la entrega ausente con el Target (una entrega sintetica con CurrentSha = I.Target.Commit) | el cotejo rechaza: RED (RevList hasta el Target), la ruta de H y C6 H de mas; registro distinto | rechazo: CLOSURE_NOT_DERIVED, DERIVATION_RECORD_DIFFERS; diferencias del cierre: Commits +1/-0, Files +1/-0, Paths +1/-0, RevPaths +14/-0 | sí |
| S-NC1-G | nc1: la supervision sustituye el CurrentSha inexistente por G solo al derivar (los bytes custodiados siguen siendo los mutados) | el cierre resultante es IGUAL al normativo (RED y G ya estaban por H.RedSha y el Target); solo el registro de derivacion muestra la sustitucion, y el cotejo la rechaza por el registro | rechazo: DERIVATION_RECORD_DIFFERS; cierre igual al normativo | sí |
| S-NC1-OTHER | nc1: el CurrentSha inexistente se sustituye por otro commit existente (cabeza de origin/fx/u1-fx05) | el cotejo rechaza: commits de RevList hasta ese commit de mas, y registro distinto | rechazo: CLOSURE_NOT_DERIVED, DERIVATION_RECORD_DIFFERS; diferencias del cierre: Commits +1/-0, RevPaths +11/-0 | sí |
| S-N4-E2E | N4 con el cierre del extremo a extremo de la ronda 3 (build_closure) en lugar del derivado | el cotejo rechaza: cientos de elementos de mas (todos los commits del clon, rutas de nueve commits, C6 H) | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Commits +20/-0, Files +1/-0, Paths +1/-4, RevPaths +419/-15 | sí |
| X-N4-COMMIT | N4 + un commit existente fuera de la tabla | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Commits +1/-0 | sí |
| X-N4-NX | N4 + el commit inexistente deadbeef... (objeto inventado) | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Commits +1/-0 | sí |
| X-N4-PATH | N4 + la ruta README.md | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Paths +1/-0 | sí |
| X-N4-REVPATH | N4 + (G, README.md) | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: RevPaths +1/-0 | sí |
| X-N4-FILE | N4 + C6 H inventado | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Files +1/-0 | sí |
| X-N4-REF | N4 + una ref fuera de C2 | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Refs +1/-0 | sí |
| X-N4-DROP | N4 sin el commit TIP (D.BaseSha) | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Commits +0/-1 | sí |
| X-NC1-NX | nc1 + el CurrentSha inexistente como commit del cierre | el cotejo rechaza: el cierre no es el derivado | rechazo: CLOSURE_NOT_DERIVED; diferencias del cierre: Commits +1/-0 | sí |
| X-N4-RECORD | N4 con el cierre normativo y un registro de derivacion vacio | el cotejo rechaza: el registro no declara los terminos omitidos | rechazo: DERIVATION_RECORD_DIFFERS; cierre igual al normativo | sí |
| X-N4-OK | Control positivo: cierre normativo de N4 con su registro | el cotejo no encuentra ninguna diferencia | sin diferencias; cierre igual al normativo | sí |
| X-NC1-OK | Control positivo: cierre normativo de nc1 con su registro | el cotejo no encuentra ninguna diferencia | sin diferencias; cierre igual al normativo | sí |

## 4. Verificación real y O3 (`p8-v.json`)

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| V-r4 | Verificacion real de T1 sobre la entrega: 12 corridas reconstruidas del registro Codex; Closure derivado con la regla corregida | la regla incluye Verification; ACCEPT_FACTS rc 0; R9 MATCH (ninguna corrida fuera, clausemap dentro); tabla de la ronda 4 aceptada (construccion, aceptacion y cotejo del cierre) | ACCEPT_FACTS rc 0; R4 MATCH; R9 MATCH; tabla de la ronda 4: aceptada | sí |
| O3-STRICT | O3: las mismas 12 corridas con el cierre derivado de la variante I6 estricta de E (todo blob = el de la AuthorityRevision) | R9 MISMATCH; quedan fuera las lecturas legitimas en G: hashes, read-text y clausemap (estado en G); read-json no (el blob del contrato en G es el que fija E); la cobertura depende de la disposicion de A6-7, regla 7 | REJECT_FACTS rc 1; R4 MATCH; R9 MISMATCH; MISMATCH: R9; fuera del cierre: hashes, clausemap, read-text; tabla de la ronda 4: no aceptada | sí |

## 5. N4: entrega ausente (`p9-n4.json`)

Salida del Controller: Termination pass, Handoff fail, Authority fail, Contract pass, CleanTree pass, Identity/Remote/Scope/Ci/Tests/Trailer/
FreeText `not_run` sin entrada, Routing y Denials `not_run` del banco; `EXECUTION_BLOCKED`/`BLOCKED`. Siete órdenes que no dependen de la
entrega: `contract`, `hashes` (G), `cleantree` (con la ruta ignorada de la invocación), `read-text` (G), `read-json` (G), `trx` y `clausemap`.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| N4-c | N4 con el cierre derivado de la norma corregida (H ausente: terminos de H omitidos) y la construccion de la ronda 3 | ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R5 R6 R7 MATCH; R9 MATCH (las 7 corridas dentro, clausemap incluida); tabla de la ronda 4 aceptada | ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R9 MATCH; tabla de la ronda 4: aceptada | sí |
| N4-d | N4-c mas una ejecucion con error (identity --current inexistente) | corrida con error conservada; REJECT_FACTS con R3, R5 y R9 MISMATCH (el commit inexistente no esta en el cierre derivado); tabla no aceptada | REJECT_FACTS rc 1; R4 NOT_APPLICABLE; R9 MISMATCH; MISMATCH: R3, R5, R9; fuera del cierre: identity; tabla de la ronda 4: no aceptada | sí |
| N4-x | N4-c mas una lectura de una entrega valida sustituida por la ausente (handoff --file H=copia de la entrega real), con el cierre derivado | REJECT_FACTS: R9 MISMATCH en esa corrida (C6 no tiene H); tabla no aceptada | REJECT_FACTS rc 1; R4 NOT_APPLICABLE; R9 MISMATCH; MISMATCH: R9; fuera del cierre: handoff; tabla de la ronda 4: no aceptada | sí |
| N4-x-subst | La misma sesion con el cierre derivado de la entrega sustituida (control negativo: entrada inexistente sustituida por una valida) | el aceptante solo da ACCEPT_FACTS (R9 MATCH: el cierre inflado admite la lectura); la tabla de la ronda 4 lo rechaza por el cotejo del cierre y del registro | ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R9 MATCH; tabla de la ronda 4: no aceptada (cotejo: CLOSURE_NOT_DERIVED; DERIVATION_RECORD_DIFFERS) | sí |

## 6. nc1: `CurrentSha` inexistente (`p10-nc1.json`)

Nueve órdenes sobre entradas cuya comprobación primaria no falló: `contract`, `hashes`, `cleantree`, `read-text`, `read-json`, `trx`, `handoff`,
`freetext` y `clausemap` (sin `identity`, `diff` ni `scope`). Variantes del Controller como en la ronda 3: V1 con las cinco comprobaciones de R4
evaluadas, V2 con Scope y Trailer en `not_run` sin entrada, V3 sin `cleantree`.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| V1-r4 | V1 con el cierre derivado de la norma corregida (H.CurrentSha omitido, RevList vacia) | ACCEPT_FACTS; R4 MATCH; R9 MATCH (9 corridas dentro, clausemap incluida); tabla de la ronda 4 aceptada | ACCEPT_FACTS rc 0; R4 MATCH; R9 MATCH; tabla de la ronda 4: aceptada | sí |
| V2-r4 | V2 con el cierre derivado de la norma corregida (H.CurrentSha omitido, RevList vacia) | ACCEPT_FACTS; R4 NOT_APPLICABLE; R9 MATCH (9 corridas dentro, clausemap incluida); tabla de la ronda 4 aceptada | ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R9 MATCH; tabla de la ronda 4: aceptada | sí |
| V3-r4 | V3 (CleanTree not_run) con el cierre derivado | ACCEPT_FACTS; R4 NOT_APPLICABLE; R9 MATCH; tabla de la ronda 4 aceptada | ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R9 MATCH; tabla de la ronda 4: aceptada | sí |
| nc1-err | V1 mas una ejecucion sobre el CurrentSha inexistente (sobre de error) | REJECT_FACTS con R3, R5 y R9 MISMATCH (el commit inexistente no esta en el cierre derivado); tabla no aceptada | REJECT_FACTS rc 1; R4 MATCH; R9 MISMATCH; MISMATCH: R3, R5, R9; fuera del cierre: identity; tabla de la ronda 4: no aceptada | sí |
| nc1-x | V1 mas una lectura en un commit existente que sustituye al inexistente (13654f96:src/Fixture.Lib/Calculator.cs), con el cierre derivado | REJECT_FACTS: R9 MISMATCH en esa corrida (el commit no esta en el cierre derivado); tabla no aceptada | REJECT_FACTS rc 1; R4 MATCH; R9 MISMATCH; MISMATCH: R9; fuera del cierre: read-text; tabla de la ronda 4: no aceptada | sí |
| nc1-x-subst | La misma sesion con el cierre derivado como si CurrentSha fuera ese commit (control negativo) | el aceptante solo da ACCEPT_FACTS (R9 MATCH); la tabla de la ronda 4 lo rechaza por el cotejo del cierre y del registro | ACCEPT_FACTS rc 0; R4 MATCH; R9 MATCH; tabla de la ronda 4: no aceptada (cotejo: CLOSURE_NOT_DERIVED; DERIVATION_RECORD_DIFFERS) | sí |

## 7. Implementación independiente (`p11-indep.json`, `indep/`)

`indep/derive_indep.py` la escribió un agente distinto del que escribió `derive_r4.py`, solo desde `indep/SPEC.md` (la regla 4 de A6-9, la
tabla y la regla 6 (b) corregida, con la interfaz de vectores); no leyó la otra implementación. Sus decisiones donde el texto admitía más de
una lectura están en `indep/README.md`; las que tocan la regla coinciden con las de `derive_r4.py` (un `null` de «si no es null» se registra como
campo `null`; una `RevList` vacía es un resultado, no una primitiva sin resultado). El texto final de la regla 6 (b) solo mueve `RelayFacts`
delante de `Closure` respecto del que recibió el agente; la regla del cierre es la misma.

Resultado: 58 de 58 vectores con el mismo cierre y los mismos términos omitidos con la misma causa (33 del banco, 25 del agente: su muestra y sus vectores sintéticos sobre un repositorio propio, con dos raíces sin relación, una fusión en la cadena de primer padre y objetos que no son commits; en uno, un TRX que no verifica, las dos implementaciones fallan como error del banco). `derive_indep.py` SHA-256 `0346022c54dce18942b3b3691c6e1190b66c2cb8e79c685676193651a66ad178`.

## 8. Resultado global y archivos

Ejecución completa (`python -I run_r4.py all`) desde un área de trabajo vacía el 2026-10-14 entre las 16:37 y las 16:53 UTC.

**56 de 56 casos se cumplen** (N4 4, V 2, derive 27, indep 1, nc1 6, neg 16).

Versiones: Git git version 2.54.0.windows.1; Python 3.13.14; pwsh 7.6.7; Windows-11-10.0.26300-SP0

| SHA-256 | Archivo |
|---|---|
| `3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426` | `derive_r4.py` |
| `c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5` | `lib_r1r4.py` |
| `357a775123ec2bcb0dc9ae6323806d92e77716aaaa04a5bdc56b4fbe4701481c` | `lib_r3.py` |
| `f1d94cc0eb2849bc69eaac90db374de2ef946aff6ce139bb9b396a1dfde8ab60` | `lib_r4.py` |
| `068cd82fcb77abff0f5374418ca3bf20b886ee19851fb0b2e1a1b84844a92870` | `p10-nc1.json` |
| `864a3a182bd78b07c0dfcb2fce05b16c848d922f242f088a688b80225d9daa4e` | `p11-indep.json` |
| `099000fb80a33bee394bdfc9690398aa7f558e79a11d6fbea3ef71c829de7444` | `p7-derive.json` |
| `acb05c430ea66e57a2830f3fc00c261339e8b62268247aa1afab48c77d40d036` | `p8-v.json` |
| `b3dcf564ef67f9da0dafa21caf9d5d95a2114a8e72b422f972b5d67873efea51` | `p9-n4.json` |
| `5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da` | `reconstruct_runs.py` |
| `aabb0bb73b5b7cec9f84220dc13400e276e8664b156e50567121e6636a50f51a` | `run_r4.py` |
| `ec4d9a8ec0f665f74660bf517526b0a1c097efe630feda1f83297c8661851c9b` | `table_r3.py` |
| `e1efb54a7f9189c715780cfd130f2ea8540e70d9c5739adff3aa12a5bb5f2797` | `table_r4.py` |

El SHA-256 de `summary.json` y el de este README se dan fuera de ellos.

Lo que no queda demostrado: las salidas del Controller y los registros de sesión son sintéticos (como en la ronda 3); un solo escenario (la
entrega simulada de FX-U1), un host, una versión de git y de pwsh; la identificación de D, H y K entre las entradas de E la da el vector; la
cobertura de las lecturas legítimas por el cierre de VERIFY sigue pendiente de la disposición de A6-7, regla 7 (O3-STRICT).

Los resultados son JSON canónico ASCII saneados (`%USERPROFILE%`, `<usuario>`, `<email>`). `inputs/` contiene copias de los cierres efectivos E,
los vectores, los cierres derivados, sus registros y las solicitudes de N4 y nc1; los sobres del instrumento no se copian (llevan direcciones en
`CoAuthoredBy`) y se citan por SHA-256 en los resultados.
