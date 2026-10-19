# I-62 A-7 — Preflight dirigido de la segunda candidata (ronda 2)

> Supervisión, plano a. Preflight de la candidata `docs/initiatives/I-62-A-7.md` ronda 2 (blob `8a528133c7cccd83356e31f8fd79cd2ebb0cec15`, PROPUESTA; generada desde la ronda 1, blob `6c29747d61c5f689a195095ad3712e6d9dba62aa`, por `make_a7r2.py`, primera versión `76eaba8a81bf55a458749ff112a7c440bbd3de53`; corregida en el mismo archivo por `fix_a7r2_b.py`, blob `897d7fca742e1f9f60376be764a05c098f3afe55`, con las observaciones 1-3 del primer preflight de la ronda 2, y por `fix_a7r2_c.py` con las dos precisiones del segundo: la cita de AP 16.8 y la exigencia de RED indeterminada) sobre la base `b9334f22204c95872e7defe41bbdaaf849097585`, según decisiones §94, puntos 2, 3 y 5, con validación incremental (§88). La coherencia con el texto de `fix_a7r2_b.py` obligó a cambiar el constructor y el validador (exigencia de RED según AP 16.8; medición sin hechos del relevo) y se repitieron q1-q6; `fix_a7r2_c.py` ya no cambia las herramientas, así que en la última corrida se repite lo que depende del texto (q0, q1-q2 con RED-UNDETERMINED, q5, q6 y las guardas) y se reutilizan por SHA-256, de la corrida anterior de este directorio, `q3-n4-nc1.json` (`a4d04a25335825e7…`), `q4-b1.json` (`a1291d85c3d45091…`) (piezas del procedimiento salvo el blob de A-7 y designaciones iguales byte a byte: q0, REUSE-PHASES). La evidencia de la ronda 1 (`docs/automation/evidence/I-62-A7/`) se reutiliza por SHA-256 y este preflight no la escribe. Aquí solo hay pruebas, resultados y hallazgos; ninguna regla se decide. Sin Codex, claude ni ningún CLI de modelo; sin red ni `gh`; sin credenciales; sin crear B1 ni ningún repositorio; nada publicado. Instrumento (`9a829b0e…`) y aceptante (`434ef1fc…`) solo desde sus objetos Git. Todo Python con `-I`. Punto de entrada único: `python -I run_a7r2.py all` (con `--reuse q3,q4` en la última corrida).

**Resultado global: 119 de 119 casos se cumplen.** Todos se cumplen. Guardas sobre el commit hipotético de publicación: **PASS** (G1-G7); autoprueba de las condiciones nuevas o cambiadas: **PASS**, 46 de 46 mutantes muertos.

## 1. Qué cambia en la ronda 2 y qué se repite

| Cambio | Pruebas repetidas | Reutilizado por identidad (ronda 1) |
|---|---|---|
| R1: categoría (c) con seis papeles cerrados (contrato, delegación, entrega, TRX de `CurrentSha`, TRX de `RedSha` si la entrega exige RED, hechos del relevo), un artefacto por papel, ruta y blob registrados, ausencia con la clasificación de 16.9, comprobación de ausencia que no es P-22, rechazo de lo no designado; `RelayFacts` = la entrada RELAY | q1 (TRX y BOM), q2 (RelayFacts), q3 (N4 y nc1), q4 (tipo B1) | negativos de (a), (b) y (d) de la ronda 1 (`p3-neg.json`), sin cambio de regla |
| R2: invalidador = A7-1 en el blob acordado + procedimiento de construcción y validación nombrado | q5 | — |
| R3: precisión de A6-2, regla 1, dentro del ámbito | q6 (COMPAT-A6-RULES) | líneas del Owner y presupuestos de V14 D.3 (`p5-compat.json`) |
| O1, O7, O2 (descriptivos) | q7 (guardas G4) | — |
| `fix_a7r2_b.py`: punto 1 (c) «si la entrega exige RED (16.9, `Ci` y `Tests`)» y «la invocación de medición … no tiene hechos del relevo»; punto 7 con «la implementación de la derivación del cierre del aceptante»; fila (c) de §3.3 | q0 (CAND-DIFF), q1 (RED-*), q2 (RELAY-MEASUREMENT*), q5 (INV-PIECE, INV-DERIVATION), q7 (G4 rule1c-roles, rule1c-measurement, rule7-invalidator, table-c-roles, superseded-r2b) | — |
| `fix_a7r2_c.py`: fila L16 de §2 con AP 16.8 L616-L617 y la cita de «**exige RED**»; punto 1 (c) «(16.8; 16.9, `Ci` y `Tests`)»; regla 5 (c) «si la exigencia de RED de 16.8 no se puede determinar porque falta la entrega, el TRX de la corrida de `RedSha` ni se admite ni se exige, y se registra la causa» | q0 (CAND-DIFF-C), q1 (RED-UNDETERMINED), q5 (blob de A-7), q6, q7 (G3 L16, L16-168-lines, L16-168-quote, L16-168-required; G4 rule1c-roles, rule5c-169, rule5c-undetermined, rule5c-absence-check, superseded-r2c) | q3, q4 (sin dependencia del texto; REUSE-PHASES) |
| — | — | reproducción sobre R7-A (`p1-repro.json`), reconstrucción estática (`p6-recon.json`, `reconstruct_runs*.py` sin cambio), condiciones de las guardas sin cambio (`p4-guards-selftest.json`) |

## 2. Procedimiento de la ronda 2

- **Formato de designación `a7-designation/2`** (`a7-designation-2.schema.json`): `Artifacts` con `Role` ∈ {CONTRACT, DELEG, H, TRX_CURRENT, TRX_RED, RELAY}, `Path`, `Blob` registrado (obligatorio) y `Custody`; `Invocation.Kind` ∈ {VERIFY, MEASUREMENT} (ausente = VERIFY), añadido en esta corrida porque sin esa marca la medición no se distingue de una verificación (q2, RELAY-MEASUREMENT-DESIGNATED). El formato de la ronda 1 (`a6m7-designation/1`) solo se lee como evidencia negativa (`--legacy-designation`).
- **Constructor `make_eic_a7.py` (ronda 2)**: (a), (b) y (d) como en la ronda 1; (c) con los seis papeles, en el orden contrato, …, delegación, entrega, TRX de `CurrentSha`, TRX de `RedSha`, hechos del relevo, instrumento. **La entrega exige RED** según AP 16.8 («RED de la cadena», L616-L618, la condición que usan las filas 9 `Ci` y 10 `Tests` de 16.9): `ChainRedSha` nulo, o `git diff --name-only BaseSha..CurrentSha` toca `ChainRedFiles` (`BaseSha`, `ChainRedSha` y `ChainRedFiles` de la delegación, `CurrentSha` de la entrega); `ExpectRed` (siempre el del contrato) nombra las pruebas que deben fallar y no decide la exigencia. Sin `CurrentSha` utilizable y con `ChainRedSha` no nulo la condición es indeterminada: el TRX de `RedSha` no se admite ni se exige y la causa se registra. En la medición (`Kind` MEASUREMENT) no hay papel RELAY: designado, no se admite; sin designar, no es una ausencia. Sin contrato o delegación utilizables no hay cierre; la entrega, un TRX o el relevo ausentes, sin blob registrado, con otro blob o ilegibles no aportan entrada y se registran. La regla anterior de la herramienta (RED = algún `RequiredTests[].ExpectRed` de la delegación) queda solo como evidencia negativa (`--red-rule=expectred`).
- **Validador `check_a7_closure.py` (ronda 2)**: escrito desde el texto, sin importar el constructor (su propia implementación de la condición de 16.8); comprueba los seis papeles, un artefacto por papel, el blob registrado frente a los bytes custodiados (un BOM forma parte de los bytes) y rechaza como FOREIGN lo no designado, de otro papel (un TRX de `RedSha` cuando la entrega no exige RED o no se puede determinar; hechos del relevo en la medición) o con otro blob.
- **`designate_a7.py`** (lado del aceptante): comprobación de la designación y vector de `derive_r4.py` (sin cambio) con los seis papeles; C6 nombra TRX a los dos TRX designados y no nombra los hechos del relevo. Las dos son «la implementación de la derivación del cierre del aceptante» del punto 7. `designate_m7.py` (ronda 1) sigue sin cambio para R7-A.
- **`check_citation_blobs.py`**: solo cambia para leer `a7-designation/2`; el criterio de cobertura de §3.5 es el mismo.

## 3. Entorno, identidades y reutilización (`q0-env.json`)

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| ENV-W | HEAD del worktree de I-62 (solo lectura) | b9334f22204c95872e7defe41bbdaaf849097585 | b9334f22204c95872e7defe41bbdaaf849097585 | sí |
| CAND | candidata A-7 ronda 2 (tras fix_a7r2_b.py y fix_a7r2_c.py) leída por completo | blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15 | blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15, 37205 bytes, 346 líneas, LF | sí |
| CAND-DIFF-B | candidata 897d7fca… frente a 76eaba8a… (fix_a7r2_b.py) | solo las tres frases de fix_a7r2_b.py: punto 1 (c), punto 7, fila (c) de §3.3 | 3 tramos: L137-L138 → L137-L139 (punto 1 (c)); L175-L175 → L176-L177 (punto 7); L189-L189 → L191-L191 (fila (c) de §3.3) | sí |
| CAND-DIFF-C | candidata 8a528133… frente a 897d7fca… (fix_a7r2_c.py) | solo las tres frases de fix_a7r2_c.py: fila L16 de §2, punto 1 (c), punto 5 (c) | 3 tramos: L99-L99 → L99-L99 (fila L16 de §2); L137-L137 → L137-L137 (punto 1 (c)); L166-L166 → L166-L167 (punto 5 (c)) | sí |
| PREV | ronda 1 en la base (la que la ronda 2 corrige en el mismo archivo) | 6c29747d61c5f689a195095ad3712e6d9dba62aa | 6c29747d61c5f689a195095ad3712e6d9dba62aa | sí |
| R1-EVID | evidencia de la ronda 1 publicada (docs/automation/evidence/I-62-A7/, 35 archivos) frente a la local reutilizada | idéntica byte a byte (la ronda 2 no la toca; el resultado de la guarda run lo añadió el Coordinator) | iguales 34; solo en la publicación: a7-guards-result-run.json; ningún archivo de r2/ publicado todavía: sí | sí |
| REUSE-derive_r4.py | copia byte a byte reutilizada por identidad | SHA-256 3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426 | SHA-256 3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426 | sí |
| REUSE-designate_m7.py | copia byte a byte reutilizada por identidad | SHA-256 491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6 | SHA-256 491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6 | sí |
| REUSE-lib_r1r4.py | copia byte a byte reutilizada por identidad | SHA-256 c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5 | SHA-256 c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5 | sí |
| REUSE-make_eic_verify.py | copia byte a byte reutilizada por identidad | SHA-256 58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2 | SHA-256 58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2 | sí |
| PIN-instrumento | instrumento desde su objeto Git, sin modificar | blob 9a829b0eeb1210238052b1feac06cfa8f7964aed | blob recalculado 9a829b0eeb1210238052b1feac06cfa8f7964aed | sí |
| PIN-aceptante | aceptante desde su objeto Git, sin modificar | blob 434ef1fccb0d15762b30244b4a60928120698411 | blob recalculado 434ef1fccb0d15762b30244b4a60928120698411 | sí |
| INPUTS | entradas de la ronda 2: designaciones a7-designation/2 (esquema a7-designation-2.schema.json) y hechos del relevo sintéticos (relay-record/v2 del repositorio de I-62) | todas válidas contra su esquema (validador del subconjunto de lib_r1r4.py) | 11 archivos; con violaciones: relay-facts-NC2.json (solo el patrón de RunId de relay-record/v2 frente a los RunId de los controles del banco, ya conocido: decisiones §92, punto 6) | sí |
| REUSE-PHASES | fases reutilizadas de la corrida anterior de este directorio (sin dependencia del texto): q3, q4 | las piezas del procedimiento salvo el blob de A-7 y las designaciones, iguales byte a byte a las de esa corrida (A-7 897d7fca…) | piezas iguales: sí; designaciones iguales: sí; archivos reutilizados: q3-n4-nc1.json a4d04a25335825e7…, q4-b1.json a1291d85c3d45091… | sí |

## 4. TRX de `CurrentSha` y de `RedSha`, exigencia de RED y fidelidad del BOM (`q1-trx.json`)

Invocación V sobre una copia del repositorio de la entrega (objeto suelto del instrumento escrito en la copia). TRX de `RedSha` sintético (`inputs/fixture-tests-red.trx`): el TRX del kit con un resultado fallido de la prueba con `ExpectRed`, con el mismo BOM y fin de línea. Las corridas del instrumento son reales (forma declarada en pwsh 7) y el aceptante fijado recibe una solicitud mínima de datos (sin `Verification`, R4 no aplicable por A6-3, regla 6 (c)); no es la aceptación de una salida del Controller. Los casos RED-* comparan, sobre delegaciones sintéticas (copias de la del banco con `ChainRedSha`, `ChainRedFiles` o `ExpectRed` cambiados y su blob registrado), la regla de 16.8 con la regla anterior de la herramienta.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| TRX-POS | la entrega exige RED (16.8: ChainRedSha nulo en la delegación del banco): TRX de la corrida de CurrentSha y TRX de la de RedSha designados con su papel, ruta y blob | dos entradas TRX en el cierre; validador acepta; designate_a7 válida; C6 con (TRX, sha) de cada una | RedRequired True (16.8: ChainRedSha is null); entradas TRX: artifacts/ci/R37949808974/fixture-tests.trx 40295387, artifacts/ci/R37949800001/fixture-tests.trx 5fd8860b; validador: acepta; C6: [('CONTRACT', '8f34367053cf'), ('DELEG', 'f380200c402f'), ('H', '5a6f76ba4fa3'), ('TRX', '4ccdf2cecba5'), ('TRX', '5e388d8774fd')] | sí |
| TRX-POS-R9 | aceptante fijado sobre dos corridas `trx --file TRX=` (los dos TRX designados), con el cierre derivado | R9 MATCH (el cierre fija los dos TRX en C6); R10 MATCH (RelayFacts = la entrada RELAY de (c)); ACCEPT_FACTS | ACCEPT_FACTS rc 0; R9 MATCH; R10 MATCH; cero escrituras: sí | sí |
| TRX-RED-FACTS | hechos del instrumento sobre el TRX de RedSha | la prueba con ExpectRed entre las fallidas (lo que lee Tests RedPart de 16.9) | rc 0; FailedTests ['Fixture.Tests.CalculatorTests.SubtractReturnsDifference']; Sha256 = el de C6: sí | sí |
| TRX-RED-AUSENTE | RED exigido y el TRX de RedSha sin bytes custodiados | sin entrada, causa registrada, nada sustituido (ni el TRX de CurrentSha ni otra copia); el validador acepta el cierre | entradas TRX: ['artifacts/ci/R37949808974/fixture-tests.trx']; ausencia: ABSENT: the custodied file does not exist; validador: acepta | sí |
| TRX-RED-ABSENT-169 | clasificación de 16.9 con el TRX de RedSha ausente: la lectura de ese TRX con el cierre derivado | R9 MISMATCH sobre la corrida del TRX de RedSha (fuera del cierre, P-22): Tests RedPart no puede ser pass (AP 16.9 fila 10, «las fallidas del TRX del RedSha»), así que Tests no pasa (REWORK) y nada lo sustituye | REJECT_FACTS; R9 MISMATCH (trx-red: Inputs[0]: file outside the closure); C6 del cierre: [('CONTRACT', '8f34367053cf'), ('DELEG', 'f380200c402f'), ('H', '5a6f76ba4fa3'), ('TRX', '4ccdf2cecba5')] | sí |
| TRX-RED-NO-DESIGNADO | RED exigido y el TRX de RedSha sin designar | sin entrada, causa registrada, nada sustituido (ni el TRX de CurrentSha ni otra copia); el validador acepta el cierre | entradas TRX: ['artifacts/ci/R37949808974/fixture-tests.trx']; ausencia: NOT_DESIGNATED: the delivery requires RED (16.8: ChainRedSha is null) and the designation names no TRX of the RedSha run (16.9: Tests and Ci RedPart cannot be pass); no entry, nothing substituted; validador: acepta | sí |
| TRX-RED-NOT-REQUIRED | la entrega no exige RED según 16.8 (ChainRedSha 53436cb2… no nulo; git diff --name-only BaseSha..CurrentSha no toca ChainRedFiles = [tests/Fixture.Tests/Fixture.Tests.csproj]); ExpectRed del contrato sin cambio (true) y la designación nombra un TRX de RedSha | RedRequired false en constructor y validador; el constructor no admite el TRX de RedSha (causa registrada); un cierre que lo incluya es rechazado (FOREIGN: no hay ese papel); el cierre sin él se acepta | RedRequired False / False (16.8: ChainRedSha 53436cb20adf1fe87dc91b0382dd3123a1fac44f is not null and git diff --name-only BaseSha..CurrentSha does not touch ChainRedFiles: RED is not required (16.9 Ci RedPart not_applicable, citing the ChainRedSha run)); constructor: NOT_ADMITTED: the delivery does not require RED (16.8: ChainRedSha 53436cb20adf1fe87dc91b0382dd3123a1fac44f is not null and git diff --name-only BaseSha..CurrentSha does not touch ChainRedFiles: RED is not required (16.9 Ci RedPart not_applicable, citing the ChainRedSha run)), so rule 1 (c) has no TRX of the RedSha run; validador: acepta; con la entrada del TRX de RedSha: rechaza: FOREIGN ['artifacts/ci/R37949800001/fixture-tests.trx'] | sí |
| RED-DIV-EXPECTRED | divergencia de la regla anterior de las herramientas (RED si algún RequiredTests[].ExpectRed de la delegación es true) frente a 16.8, con las mismas entradas de TRX-RED-NOT-REQUIRED (una delegación de una cadena que ya acreditó un RED: A5 obliga a ExpectRed = el del contrato, y 16.8 dice que ExpectRed «es siempre el del contrato») | la regla anterior exige RED y admite (y, sin designar, exige) el TRX de RedSha donde 16.8 no lo exige: las dos reglas divergen, y la regla 16.8 (la de las herramientas ahora) da la respuesta de 16.8 | regla anterior: RedRequired True, entrada del TRX de RedSha sí, validador con ella: acepta, sin designar: NOT_DESIGNATED: the delivery requires RED (superseded tool r; regla 16.8: RedRequired False, sin designar: sin ausencia del TRX de RedSha (no se exige) | sí |
| RED-169-TOUCH | ChainRedSha no nulo y git diff --name-only BaseSha..CurrentSha toca ChainRedFiles = [tests/Fixture.Tests/CalculatorTests.cs] | la entrega exige RED (16.8): el TRX de RedSha entra; validador acepta; las dos reglas coinciden aquí | RedRequired True / True (16.8: ChainRedSha 53436cb20adf1fe87dc91b0382dd3123a1fac44f is not null and git diff --name-only BaseSha..CurrentSha touches ChainRedFiles: tests/Fixture.Tests/CalculatorTests.cs); entrada del TRX de RedSha: sí; validador: acepta; regla anterior: RedRequired True | sí |
| RED-DIV-NULL | ChainRedSha nulo y ningún RequiredTests con ExpectRed true (divergencia de principio) | 16.8: la entrega exige RED (ChainRedSha nulo): el TRX de RedSha entra y el validador acepta; la regla anterior diría lo contrario | regla 16.8: RedRequired True / True, entrada sí, validador acepta; regla anterior: RedRequired False, constructor: NOT_ADMITTED: the delivery does not requ, validador con la entrada: rechaza (FOREIGN) | sí |
| RED-UNDETERMINED | ChainRedSha no nulo y la entrega ausente (sin CurrentSha): la exigencia de RED de 16.8 no se puede determinar; A7-1, regla 5 (c), tras fix_a7r2_c.py: «el TRX de la corrida de `RedSha` ni se admite ni se exige, y se registra la causa» | coincide con el texto: RedRequired indeterminado en constructor y validador; designado, el TRX de RedSha no se admite (causa registrada) y un cierre que lo incluya es FOREIGN; sin designar, no se exige (ninguna ausencia por él) y la causa queda registrada (RedBasis); el resto del cierre se acepta | RedRequired None / None; designado: NOT_ADMITTED: UNDETERMINED: ChainRedSha 53436cb20adf1fe87dc91b0382dd31, validador con la entrada: rechaza: FOREIGN; sin designar: ausencias del TRX de RedSha 0, RedBasis UNDETERMINED: ChainRedSha 53436cb20adf1f; validador: acepta | sí |
| TRX-THIRD | un tercer TRX no designado: en el cierre y como lectura | validador: FOREIGN; aceptante: R9 MISMATCH sobre su lectura (P-22) | validador: rechaza FOREIGN; aceptante: REJECT_FACTS, R9 MISMATCH (trx-third: Inputs[0]: file outside the closure) | sí |
| TRX-REPLACED | el TRX de RedSha sustituido por otros bytes (el blob registrado no cambia) | sin entrada (BLOB_MISMATCH), nada sustituido | causa: BLOB_MISMATCH: the custodied bytes do not have the registered blob; entradas TRX: ['artifacts/ci/R37949808974/fixture-tests.trx'] | sí |
| BOM-POS | el TRX del banco empieza con un BOM UTF-8: su blob se calcula sobre los bytes con el BOM | entrada con el blob de los bytes con BOM (= el registrado, 40295387…); el blob sin el BOM es otro; los hechos del instrumento citan el SHA-256 de los bytes con BOM | BOM: sí; blob con BOM 40295387 (registrado 40295387); sin BOM 5582335a; Sha256 de los hechos = SHA-256 con BOM: sí | sí |
| BOM-STRIPPED | una copia del TRX sin el BOM (bytes distintos) en la ruta designada; el blob registrado es el de los bytes con BOM | sin entrada (BLOB_MISMATCH: quitar el BOM no es fiel); su lectura queda fuera del cierre (R9 MISMATCH) | causa: BLOB_MISMATCH: the custodied bytes do not have the registered blob; aceptante: REJECT_FACTS, R9 MISMATCH | sí |
| ABSENCE-CHECK | comprobar que la entrega designada falta (handoff --allow-absent sobre su ruta de custodia) con el cierre sin entrega (como N4) | no es una lectura fuera del cierre ni aporta contenido (A7-1, punto 5 (c)): R9 MATCH; los hechos dicen Present false | hechos: {'Present': False}; aceptante: ACCEPT_FACTS, R9 MATCH; C6 del cierre sin H: sí | sí |

## 5. `RelayFacts` (`q2-relay.json`)

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| RELAY-POS | hechos del relevo sintéticos (relay-record/v2: Outcome COMPLETED y operación 7, RemoteFacts con las corridas de CurrentSha y RedSha y sus TRX, Routing, Denials) designados con su papel, ruta y blob | una entrada RELAY; C6 no le da nombre (el instrumento no la lee); la solicitud lleva RelayFacts con su SHA-256 y R10 MATCH | entrada: artifacts/orchestration/FX-U1/T1/0/R20261012T092000Z-c001/relay-facts.json 5ad9a3e7; Name(p) de C6 para la ruta: ninguno; R10 MATCH ({'Cited': 'ab36aa0fda2c54f87f3c461f778daea5f67a4388f9898c5a0f62659b6edb5597', 'Computed': 'ab36aa0fda2c54f87f3c461f778daea5f67a4388f9898c5a0f62659b6edb5597'}) | sí |
| RELAY-ABSENT | hechos del relevo ausentes | sin entrada, causa registrada; la solicitud no lleva RelayFacts (A7-1, punto 6: los de (c)) y R10 no puede dar MATCH: la salida no quedaría aceptada (A6-5, regla 2, exige R10 en MATCH); nada lo sustituye | causa: ABSENT: the custodied file does not exist; aceptante: REJECT_FACTS, R10 MISMATCH ({'Missing': 'no data for this check and no NotApplicable declaration'}) | sí |
| RELAY-SUBST | hechos del relevo sustituidos (otros bytes en la ruta designada; blob registrado sin cambio) | sin entrada (BLOB_MISMATCH); si la solicitud citara el SHA-256 registrado con esos bytes, R10 MISMATCH | causa: BLOB_MISMATCH: the custodied bytes do not have the registered blob; aceptante con el SHA-256 citado y los bytes sustituidos: R10 MISMATCH ({'Cited': 'ab36aa0fda2c54f87f3c461f778daea5f67a4388f9898c5a0f62659b6edb5597', 'Computed': '23d3f58d4bcba57246789077de99ea53754c40daa718628d1a1a55a2b1d932ee'}) | sí |
| RELAY-FOREIGN | hechos del relevo de otra corrida, en una ruta no designada | FOREIGN (no admitidos; su lectura sería P-22) | validador: rechaza: FOREIGN artifacts/orchestration/FX-U1/T1/0/R20261011T081000Z-a000/relay-facts.json | sí |
| RELAY-MEASUREMENT | invocación de medición M (a7-designation/2 con Invocation.Kind MEASUREMENT), sin hechos del relevo | sin entrada RELAY; no es una ausencia (la medición no los tiene: nada que clasificar por 16.9), solo una nota; validador acepta | entradas RELAY: 0; ausencias RELAY: 0; nota: RELAY: not applicable to the measurement invocation (A7-1, point 1 (c); validador: acepta | sí |
| RELAY-MEASUREMENT-DESIGNATED | una medición cuya designación nombra hechos del relevo (papel inexistente para ella) | el constructor no los admite (causa registrada); un cierre que los incluya es FOREIGN; sin la marca Kind la medición no se distinguiría de una verificación (los admitiría): la marca es necesaria en el formato | constructor: NOT_ADMITTED: the measurement invocation produces no verific; validador con la entrada: rechaza: FOREIGN ['relay-facts.json']; sin Kind: entrada RELAY sí | sí |

## 6. N4 y nc1 con la designación y el constructor de la ronda 2 (`q3-n4-nc1.json`)

Las solicitudes custodiadas del banco (N4-A; nc1 V1, V2 y V3) se repiten tal cual, cambiando solo el `Closure` por el derivado del E de la ronda 2; los resultados del banco son `p3-n4.json` y `p4-nc1.json` del banco (por identidad).

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| N4-E | N4: E de A7-1 r2 (a7-designation/2 con TRX_RED y RELAY) frente al E de la ronda 1 / R7-A custodiado en el banco | el E r2 es el de la ronda 1 más las entradas TRX_RED y RELAY (en el mismo orden); validador acepta; ausencias de la invocación registradas (N4: la entrega) | quitando 2 entradas (fixture-tests.trx, relay-facts.json), bytes iguales al E del banco: sí; validador: acepta; ausencias (c): H ABSENT | sí |
| N4-CLOSURE | N4: cierre del aceptante derivado (designate_a7 + derive_r4 sin cambio) frente al del banco | superconjunto del cierre de la ronda 1 (más C4 de las dos rutas nuevas y C6 del segundo TRX), con las mismas omisiones | superconjunto: sí; omisiones iguales: sí; C6: [('CONTRACT', '8f34367053cf'), ('DELEG', 'e7a9114e20d2'), ('TRX', '4ccdf2cecba5'), ('TRX', '5e388d8774fd')] | sí |
| N4-A-R2 | N4: la misma solicitud del banco (N4-A) con el cierre derivado r2 | mismo resultado del aceptante que en el banco (las comprobaciones no dependen del cierre salvo R9, monótona: un cierre mayor no crea violaciones); cero escrituras en el repositorio del banco | ACCEPT_FACTS rc 0; comprobaciones iguales a las del banco: sí; RelayFacts del banco = entrada RELAY de (c): sí; cero escrituras: sí | sí |
| NC1-E | NC1: E de A7-1 r2 (a7-designation/2 con TRX_RED y RELAY) frente al E de la ronda 1 / R7-A custodiado en el banco | el E r2 es el de la ronda 1 más las entradas TRX_RED y RELAY (en el mismo orden); validador acepta; ausencias de la invocación registradas (N4: la entrega) | quitando 2 entradas (fixture-tests.trx, relay-facts.json), bytes iguales al E del banco: sí; validador: acepta; ausencias (c): ninguna | sí |
| NC1-CLOSURE | NC1: cierre del aceptante derivado (designate_a7 + derive_r4 sin cambio) frente al del banco | superconjunto del cierre de la ronda 1 (más C4 de las dos rutas nuevas y C6 del segundo TRX), con las mismas omisiones | superconjunto: sí; omisiones iguales: sí; C6: [('CONTRACT', '8f34367053cf'), ('DELEG', '195930e9e4f6'), ('H', 'a7bd1b78c9a6'), ('TRX', '4ccdf2cecba5'), ('TRX', '5e388d8774fd')] | sí |
| NC1-V1-R2 | NC1: la misma solicitud del banco (NC1-V1-A) con el cierre derivado r2 | mismo resultado del aceptante que en el banco (las comprobaciones no dependen del cierre salvo R9, monótona: un cierre mayor no crea violaciones); cero escrituras en el repositorio del banco | ACCEPT_FACTS rc 0; comprobaciones iguales a las del banco: sí; RelayFacts del banco = entrada RELAY de (c): sí; cero escrituras: sí | sí |
| NC1-V2-R2 | NC1: la misma solicitud del banco (NC1-V2-A) con el cierre derivado r2 | mismo resultado del aceptante que en el banco (las comprobaciones no dependen del cierre salvo R9, monótona: un cierre mayor no crea violaciones); cero escrituras en el repositorio del banco | ACCEPT_FACTS rc 0; comprobaciones iguales a las del banco: sí; cero escrituras: sí | sí |
| NC1-V3-R2 | NC1: la misma solicitud del banco (NC1-V3-A) con el cierre derivado r2 | mismo resultado del aceptante que en el banco (las comprobaciones no dependen del cierre salvo R9, monótona: un cierre mayor no crea violaciones); cero escrituras en el repositorio del banco | ACCEPT_FACTS rc 0; comprobaciones iguales a las del banco: sí; cero escrituras: sí | sí |

## 7. Tipo B1 con EFF^1 distinto (`q4-b1.json`, `b1-like/`)

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| B1R2-E | B1-like (candidate-b, EFF^1 con los blobs anteriores a la activación) con a7-designation/2 y el constructor r2 (mismos commits sintéticos que en la ronda 1) | las mismas fijaciones en EFF^1 que en la ronda 1 (6, blob distinto del de la AR) más TRX_RED y RELAY; validador acepta | commits iguales a la ronda 1: sí; entradas en EFF^1 con blob distinto de la AR: 6; (c): CONTRACT, DELEG, H, TRX_CURRENT, TRX_RED, RELAY; validador: acepta | sí |
| B1R2-R9 | clausemap por la forma declarada y el aceptante fijado, con el cierre derivado del E r2 y con el de R7-A | R9 MATCH con A7-1 r2; R9 MISMATCH con R7-A (lecturas en EFF^1 fuera del cierre); R10 MATCH con los hechos del relevo de (c) | clausemap rc 0; A7 r2: ACCEPT_FACTS, R9 MATCH, R10 MATCH; R7-A: REJECT_FACTS, R9 MISMATCH (12 violaciones); cero escrituras: sí | sí |
| B1R2-UC | residuo declarado (§3.5): Calculator.cs cambió antes de la AR; clausemap lo compara en EFF^1, EFF y MainSha_eval | sin cambio respecto de la ronda 1: R9 MISMATCH con el cierre de A7-1 r2 (rechazo falso, nunca aceptación falsa) y la precondición de cobertura bloquea el lanzamiento (S-03) | aceptante: REJECT_FACTS, R9 MISMATCH; precondición: NO_LAUNCH_S-03 (Calculator.cs) | sí |
| B1R2-CIT | precondición de cobertura en el caso B1-like principal | se puede lanzar (AUTOMATION_PLAN en EFF^1 está fijado por el cierre) | LAUNCH_MAY_PROCEED | sí |

## 8. Identidad del invalidador de A6-7 (`q5-invalidator.json`)

| Pieza (A7-1, punto 7) | Identidad |
|---|---|
| A-7 (candidata ronda 2) | blob `8a528133c7cccd83356e31f8fd79cd2ebb0cec15` |
| constructor `make_eic_a7.py` | SHA-256 `0e5f9305525f8d0e91d09127dc1a0d2238883a37b28d29b08cfaf98803584aad` |
| validador `check_a7_closure.py` | SHA-256 `59e26e118697bc8371e0496d8b44490a5105e74536c5e67ca56343c854bc96b1` |
| formato `a7-designation-2.schema.json` | SHA-256 `ae33ad31dde5f38094d766b6a576b3f354ff2e27066fa791718a751c9bf306c0` |
| derivación del cierre del aceptante: `designate_a7.py` | SHA-256 `759df4f0332b1e4557ab3460973323e3016e485d6cb92cb1793df56f01842ab9` |
| derivación del cierre del aceptante: `derive_r4.py` (sin cambio desde la ronda 1) | SHA-256 `3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426` |

| Invocación | SHA-256 del cierre concreto (se custodia con la invocación) | ¿Válido? |
|---|---|---|
| V | `1c12a669efd809c1f4d983c743d98449e2315cd7e40c7c90c855b9ed5f8bd585` | sí |
| N4 | `62d06da54d87cc322f3f7885fef0aa05f474b26bb9cd1f3ed9df43c6f6b1300e` | sí |
| NC1 | `278d927bb6841b170cc2ab9d5658b496dbfa9ce80926c0d1e49e216008211056` | sí |
| NC2 | `a4c9e8d65b0a0d1bb5bf39214eaea4e5194633a7af494a25768c7eee927d4a1e` | sí |
| N5 | `dd71b1467acf0b393d3a82647548e27d811d476c7050ebe97e34e8d6f581b04d` | sí |
| N6 | `3a4045c3e160db7b9193e47a4619398857ef32b3be1776d2e94770f0a8694c98` | sí |
| N7A | `075c4980eaaa3a2e7d2a19a6bb14d65c5b920086b570902e846e2ad3de5ac247` | sí |
| N7B | `baa8ea47a84a17e201641eb41eb21966cf1ff42cf7db2e44225a99f787545677` | sí |
| M | `0c4f308ec46ea29e0081f6058df7de89e35a931712182a7c7bcc90ac5abeebf3` | sí |

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| INV-DISTINCT | cierres de V, N4, nc1, nc2, N5, N6, N7a, N7b y M con a7-designation/2 y el constructor r2 | SHA-256 distintos por construcción, todos válidos bajo A7-1 con el mismo procedimiento (constructor, validador, formato de designación e implementación de la derivación) | 9 cierres, 9 SHA-256 distintos; aceptados 9; procedimiento: A-7 8a528133, constructor 0e5f9305525f…, validador 59e26e118697…, formato ae33ad31dde5…, designate_a7 759df4f0332b…, derive_r4 3de12c54e389… | sí |
| INV-INPUT-ONLY | V con otros hechos del relevo (otra sesión): cambian solo las entradas de la invocación | otro cierre, válido, y el mismo invalidador: no anula la medición | SHA-256 del cierre 36484200e7c6dc94 (antes 1c12a669efd809c1); validador: acepta; procedimiento igual: sí | sí |
| INV-PIECE | un byte más en cada pieza del procedimiento nombrado (A-7; el constructor, el validador, el formato de la designación y la implementación de la derivación del cierre del aceptante: designate_a7.py y derive_r4.py), una a una | cada cambio es un cambio del invalidador (anula la medición y la elegibilidad, A7-1, punto 7) y solo cambia esa pieza | A7Blob → cambia A7Blob; BuilderSha256 → cambia BuilderSha256; ValidatorSha256 → cambia ValidatorSha256; DesignationFormatSha256 → cambia DesignationFormatSha256; DerivationDesignateSha256 → cambia DerivationDesignateSha256; DerivationDeriveSha256 → cambia DerivationDeriveSha256 | sí |
| INV-DERIVATION | cambio de designate_a7.py y, aparte, de derive_r4.py (la implementación de la derivación del cierre del aceptante, pieza nueva del punto 7 tras fix_a7r2_b.py) | cada uno es un cambio del invalidador aunque el E de la invocación (y su SHA-256) no cambie; con la lista de piezas de la primera versión de la ronda 2 (sin la derivación) no se habría notado | designate_a7.py: E 1c12a669efd8 (igual al de V: sí), cambia DerivationDesignateSha256; con la lista anterior cambia nada; derive_r4.py: E 1c12a669efd8 (igual al de V: sí), cambia DerivationDeriveSha256; con la lista anterior cambia nada | sí |

## 9. Compatibilidad con A-5 y A-6 (`q6-compat.json`)

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| PIN-AUTOMATION_PLAN.md | blob de docs/AUTOMATION_PLAN.md en la base b9334f22 | f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5 | f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5 | sí |
| PIN-README.md | blob de docs/automation/agent-execution/README.md en la base b9334f22 | 592dcfd45e8743fd83b3423249fbd3c73e3d3055 | 592dcfd45e8743fd83b3423249fbd3c73e3d3055 | sí |
| PIN-I-62-A-1.md | blob de docs/initiatives/I-62-A-1.md en la base b9334f22 | c01899a72b940503bb85a0fab42bc085c603fd0f | c01899a72b940503bb85a0fab42bc085c603fd0f | sí |
| PIN-I-62-A-2.md | blob de docs/initiatives/I-62-A-2.md en la base b9334f22 | f1e1d6f08d3cf677500794c7d0019433a4dc7d4e | f1e1d6f08d3cf677500794c7d0019433a4dc7d4e | sí |
| PIN-I-62-A-3.md | blob de docs/initiatives/I-62-A-3.md en la base b9334f22 | ea6721f78cb63fbc4f9d99563f3262eb36a32c5d | ea6721f78cb63fbc4f9d99563f3262eb36a32c5d | sí |
| PIN-I-62-A-4.md | blob de docs/initiatives/I-62-A-4.md en la base b9334f22 | 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f | 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f | sí |
| PIN-I-62-A-5.md | blob de docs/initiatives/I-62-A-5.md en la base b9334f22 | ad94ceedacbf3dbf70f0217a583978d0e32592fc | ad94ceedacbf3dbf70f0217a583978d0e32592fc | sí |
| PIN-I-62-A-6.md | blob de docs/initiatives/I-62-A-6.md en la base b9334f22 | 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 | 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 | sí |
| PIN-I-62-consensus-freeze.md | blob de docs/initiatives/I-62-consensus-freeze.md en la base b9334f22 | 0f6b860837e2478db8525e1fb30a485bc4646816 | 0f6b860837e2478db8525e1fb30a485bc4646816 | sí |
| PIN-I-62-proposal-v14.md | blob de docs/initiatives/I-62-proposal-v14.md en la base b9334f22 | 34ad80ea1bfff144bfc5169f62920a4c904c1bfa | 34ad80ea1bfff144bfc5169f62920a4c904c1bfa | sí |
| OWNER-A-6-S7-L1324 | línea del Owner: A-6 §7 L1324 | sin cambio (mismo SHA-256 que en la ronda 1); A-7 r2 no la reescribe (tramo común ≤ 60 caracteres) | SHA-256 83152eae4f451961; igual al del preflight de la ronda 1: sí; tramo común más largo con A-7 r2: 42 | sí |
| OWNER-A-6-S7-L1325 | línea del Owner: A-6 §7 L1325 | sin cambio (mismo SHA-256 que en la ronda 1); A-7 r2 no la reescribe (tramo común ≤ 60 caracteres) | SHA-256 b52a46e34aea2926; igual al del preflight de la ronda 1: sí; tramo común más largo con A-7 r2: 40 | sí |
| OWNER-decisiones-S78 | línea del Owner: decisiones §78 | sin cambio (mismo SHA-256 que en la ronda 1); A-7 r2 no la reescribe (tramo común ≤ 60 caracteres) | SHA-256 83152eae4f451961; igual al del preflight de la ronda 1: sí; tramo común más largo con A-7 r2: 42 | sí |
| OWNER-decisiones-L890 | línea del Owner: decisiones L890 | sin cambio (mismo SHA-256 que en la ronda 1); A-7 r2 no la reescribe (tramo común ≤ 60 caracteres) | SHA-256 76fc967585300073; igual al del preflight de la ronda 1: sí; tramo común más largo con A-7 r2: 20 | sí |
| OWNER-decisiones-L931 | línea del Owner: decisiones L931 | sin cambio (mismo SHA-256 que en la ronda 1); A-7 r2 no la reescribe (tramo común ≤ 60 caracteres) | SHA-256 7dcb195327e300bb; igual al del preflight de la ronda 1: sí; tramo común más largo con A-7 r2: 15 | sí |
| COMPAT-A6-RULES | reglas de A-6 que toca A7-1 en la ronda 2 | A6-2, reglas 1 y 3, precisadas en su ámbito y declaradas en cabecera, §1, §3.4 y §6; ninguna otra (punto 8) | cabecera: sí; §1: sí; §3.4: sí; §6: sí; punto 8: sí | sí |
| COMPAT-A5 | A-5 (A5-1, A5-2) | blob ad94ceed sin cambio; A-7 r2 declara A5-1 y A5-2 sin cambio y no crea B1 | blob ad94ceedacbf3dbf70f0217a583978d0e32592fc; «A5-1 y A5-2 sin cambio»: sí; «no crea B1»: sí | sí |

| Regla de A-6 | Cómo la toca A7-1 (ronda 2) |
|---|---|
| A6-1 | no tocada |
| A6-2, regla 1 | precisada dentro del ámbito de A-7 (ronda 2, R3): además del instrumento, las entradas de (c) que no son artefactos versionados son excepciones a la clase de §20.3.1, solo por el blob de sus bytes custodiados |
| A6-2, regla 2 | sin cambio (la categoría (d) remite a ella) |
| A6-2, regla 3 | precisada dentro del ámbito de A-7: referencia de comparación (I6) por categoría; la AR para las autoridades de la unidad |
| A6-3, reglas 1-7 | sin cambio; la regla 6 (b) deriva del cierre de A7-1 tal como es y su RelayFacts son los de (c) |
| A6-4 a A6-6, A6-8 | no tocadas |
| A6-7, reglas 5 y 7 | sin cambio de texto: el «cierre de VERIFY» es A7-1 con el procedimiento nombrado (A7-1, punto 7) |
| A6-9, regla 4 y tabla | sin cambio (C6 no nombra los hechos del relevo) |

## 10. Guardas de la ronda 2 (`a7-guards.py`; `q7-guards-preview.json`, `q7-guards-selftest.json`)

`preview` construye el commit hipotético de publicación sobre `b9334f22`: modifica `docs/initiatives/I-62-A-7.md` (de la ronda 1, `6c29747d`, a la candidata, blob `8a528133c7cccd83356e31f8fd79cd2ebb0cec15`) y añade este directorio bajo `docs/automation/evidence/I-62-A7/r2/` (sin `summary.json`, `preflight.md`, `MANIFEST.sha256`, `q7-guards.json` ni su propia salida, generados después; G7 se repite al final sobre el directorio completo). Commit hipotético: `cad7178cd7c704f62ab0504714688070ae58deb7`. Para el commit de publicación: `python -I a7-guards.py run --base <base> --head <commit> --out <json>`. La autoprueba mata un mutante por cada condición nueva o cambiada de la ronda 2 (incluidas las cinco de `fix_a7r2_b.py` y las nueve de `fix_a7r2_c.py`; el mutante de `rule7-instance` cambió de ancla porque `fix_a7r2_b.py` movió un salto de línea); las condiciones sin cambio conservan el código y la autoprueba de la ronda 1 (G3 literal/lines/blob/anchor/extra para L1-L15; G4 scope, nochange, quote, categories, location, table, rule1, rule1a, rule2a, rule2-repeat, rule5a, residual, residual-coverage, superseded-35, q06; G5 classification, owner, no-agreed; G6 values, motive, header; G7 host-identity, users-path, e-mail; G2 pinned, header-claims, protected (surface, kit, I-64/I-65)).

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| GUARD-PREVIEW | guardas G1-G7 sobre el commit hipotético (base b9334f22 + candidata r2 como modificación + este directorio bajo I-62-A7/r2/) | PASS en las siete | G1 PASS; G2 PASS; G3 PASS; G4 PASS; G5 PASS; G6 PASS; G7 PASS | sí |
| GUARD-SELFTEST | autoprueba: un mutante por condición nueva o cambiada en la ronda 2 | candidata PASS y todos los mutantes muertos | candidata PASS; mutantes muertos 46 de 46; condiciones sin cambio reutilizadas de la autoprueba de la ronda 1 | sí |
| GUARD-R2B | condiciones de G4 nuevas o cambiadas por las tres frases de fix_a7r2_b.py (punto 1 (c) RED y medición, punto 7, fila (c) de §3.3, frases sustituidas como superadas) | un mutante por condición, todos muertos | 5 de 5 muertos: rule1c-roles, rule1c-measurement, rule7-invalidator, superseded-r2b, table-c-roles | sí |
| GUARD-R2C | condiciones de G3 y G4 nuevas o cambiadas por fix_a7r2_c.py (L16 con AP 16.8 L616-617; punto 1 (c) «(16.8; 16.9, …)»; regla 5 (c) con la exigencia de RED indeterminada; la cita sin 16.8 como superada) | un mutante por condición, todos muertos | 9 de 9 muertos: G3 L16, G3 L16-168-lines, G3 L16-168-quote, G3 L16-168-required, G4 rule1c-roles, G4 rule5c-169, G4 rule5c-undetermined, G4 rule5c-absence-check, G4 superseded-r2c | sí |
| MUT-G1-only-allowed | añade un archivo fuera de r2/ (docs/automation/evidence/I-62-A7/extra.txt) | G1 FAIL (G1-only-allowed) | G1 FAIL | sí |
| MUT-G1-amendment-modified | el commit no modifica la enmienda (la ronda 1 queda como está) | G1 FAIL (G1-amendment-modified) | G1 FAIL | sí |
| MUT-G2-r1-evidence | cambia un archivo de la evidencia de la ronda 1 (I-62-A7/preflight.md) | G2 FAIL (G2-protected(r1-evidence)) | G2 FAIL | sí |
| MUT-G2-previous-round | la cabecera declara otra ronda anterior | G2 FAIL (G2-previous-round) | G2 FAIL | sí |
| MUT-G3-rows | falta la fila L17 | G3 FAIL (G3-rows) | G3 FAIL | sí |
| MUT-G3-L16 | L16 con otro conjunto de líneas de 16.9 (sin la 638) | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G3-L16-168-lines | L16 sin las líneas 616-617 de 16.8 (la cita de «exige RED» queda fuera de las líneas citadas) | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G3-L16-168-quote | L16 con la cita de 16.8 alterada («o si» → «y si») | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G3-L16-168-required | L16 vuelve a la fila anterior a fix_a7r2_c.py (solo AP 16.9, sin la definición de 16.8) | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G3-L17 | L17 con una palabra cambiada en la cita | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G4-points | el punto 7 (invalidador) deja de estar numerado | G4 FAIL (G4-points) | G4 FAIL | sí |
| MUT-G4-rule1c-roles | (c): la condición del TRX de RedSha sin la referencia a 16.8 (vuelve a «(16.9, `Ci` y `Tests`)») | G4 FAIL (G4-rule1c-roles) | G4 FAIL | sí |
| MUT-G4-rule1c-measurement | (c) sin «la invocación de medición … no tiene hechos del relevo» | G4 FAIL (G4-rule1c-measurement) | G4 FAIL | sí |
| MUT-G4-rule1-a62r1 | sin la precisión de A6-2, regla 1, en el punto 1 | G4 FAIL (G4-rule1-a62r1) | G4 FAIL | sí |
| MUT-G4-rule2c | regla 2 (c) sin «un artefacto como máximo por papel» | G4 FAIL (G4-rule2c) | G4 FAIL | sí |
| MUT-G4-rule5c | regla 5 (c) sin los hechos del relevo | G4 FAIL (G4-rule5c) | G4 FAIL | sí |
| MUT-G4-rule5c-169 | regla 5 (c) sin la clasificación de 16.9 | G4 FAIL (G4-rule5c-169) | G4 FAIL | sí |
| MUT-G4-rule5c-undetermined | regla 5 (c): con la exigencia de RED indeterminada, el TRX de RedSha se admite | G4 FAIL (G4-rule5c-undetermined) | G4 FAIL | sí |
| MUT-G4-rule5c-absence-check | regla 5 (c): comprobar la ausencia vuelve a ser una lectura fuera del cierre | G4 FAIL (G4-rule5c-absence-check) | G4 FAIL | sí |
| MUT-G4-rule5c-p22 | regla 5 (c): la lectura de lo no designado deja de ser P-22 | G4 FAIL (G4-rule5c-p22) | G4 FAIL | sí |
| MUT-G4-rule6-relay | regla 6 sin los hechos del relevo de la solicitud | G4 FAIL (G4-rule6-relay) | G4 FAIL | sí |
| MUT-G4-rule7-invalidator | regla 7 sin la implementación de la derivación del cierre del aceptante entre las piezas | G4 FAIL (G4-rule7-invalidator) | G4 FAIL | sí |
| MUT-G4-rule7-instance | regla 7: el invalidador vuelve a ser el SHA-256 de un cierre | G4 FAIL (G4-rule7-instance) | G4 FAIL | sí |
| MUT-G4-rule7-annul | regla 7 sin lo que no anula la medición | G4 FAIL (G4-rule7-annul) | G4 FAIL | sí |
| MUT-G4-superseded | reinsertada la frase de la ronda 1 «el resultado de pruebas (TRX) que la invocación verifica» | G4 FAIL (G4-superseded) | G4 FAIL | sí |
| MUT-G4-superseded-r2b | reinsertada la frase superada por fix_a7r2_b.py «si la delegación exige RED» | G4 FAIL (G4-superseded) | G4 FAIL | sí |
| MUT-G4-superseded-r2c | reinsertada la cita superada por fix_a7r2_c.py «exige RED (16.9, `Ci` y `Tests`)» (sin 16.8) | G4 FAIL (G4-superseded) | G4 FAIL | sí |
| MUT-G4-s34-a62r1 | §3.4 sin la viñeta de A6-2, regla 1 | G4 FAIL (G4-s34-a62r1) | G4 FAIL | sí |
| MUT-G4-s34-a67 | §3.4 sin «no el SHA-256 de una instancia» | G4 FAIL (G4-s34-a67) | G4 FAIL | sí |
| MUT-G4-superseded-34 | §3.4 reinsertado «nombra ese cierre (su SHA-256)» | G4 FAIL (G4-superseded-34) | G4 FAIL | sí |
| MUT-G4-s31-o7 | §3.1 sin «las lecturas de contenido de MV-6 y MV-7» | G4 FAIL (G4-s31-o7) | G4 FAIL | sí |
| MUT-G4-superseded-31 | §3.1 vuelve a «las lecturas de MV-3, MV-4, MV-6 y MV-7» | G4 FAIL (G4-superseded-31) | G4 FAIL | sí |
| MUT-G4-s35-o2 | §3.5 sin la reverificación de 16.7 | G4 FAIL (G4-s35-o2) | G4 FAIL | sí |
| MUT-G4-s36 | sin §3.6 | G4 FAIL (G4-s36) | G4 FAIL | sí |
| MUT-G4-s5-m04 | motivo de M-04 sin la regla 5 vigente para (a) | G4 FAIL (G4-s5-m04) | G4 FAIL | sí |
| MUT-G4-superseded-5 | motivo de M-04 vuelve a «sin lanzamiento en (a) y en el objeto de (b)» | G4 FAIL (G4-superseded-5) | G4 FAIL | sí |
| MUT-G4-q-r2 | sin la pregunta Q-A7R2-03 | G4 FAIL (G4-q-r2) | G4 FAIL | sí |
| MUT-G4-a62r1-declared | §6 sin la precisión de A6-2, reglas 1 y 3 | G4 FAIL (G4-a62r1-declared) | G4 FAIL | sí |
| MUT-G4-table-absence | la columna de ausencia de (c) en §3.3 sin la clasificación de 16.9 | G4 FAIL (G4-table-absence) | G4 FAIL | sí |
| MUT-G4-table-c-roles | la fila (c) de §3.3 vuelve a «con RED» y sin «la medición no los tiene» | G4 FAIL (G4-table-c-roles) | G4 FAIL | sí |
| MUT-G5-amendment | Amendment ronda 1 | G5 FAIL (G5-amendment) | G5 FAIL | sí |
| MUT-G5-state | State PROPUESTA (ronda 1) | G5 FAIL (G5-state) | G5 FAIL | sí |
| MUT-G5-architect | Architect review sin la ronda 1 | G5 FAIL (G5-architect) | G5 FAIL | sí |
| MUT-G5-previous-round | sin el bloque Previous round | G5 FAIL (G5-previous-round) | G5 FAIL | sí |
| MUT-G7-amendment-scanned | una dirección de correo en la enmienda modificada (en la ronda 2 la enmienda no es un archivo añadido) | G7 FAIL (e-mail address) | G7 FAIL | sí |
| MUT-G5-state-table | §11: Architect ronda 2 LAUNCHED | G5 FAIL (G5-state-table) | G5 FAIL | sí |

## 11. Hallazgos y observaciones

**Casos que no se cumplen:** ninguno.

**Hallazgo sobre las herramientas (corregido en la corrida sobre `897d7fca`, no en el texto).** Con el texto corregido («si la entrega exige RED»), la regla del primer preflight de la ronda 2 (RED = algún `RequiredTests[].ExpectRed` de la delegación) **no era coherente** con AP: la exigencia de RED la define 16.8 (L616-L617: `ChainRedSha` nulo o `git diff --name-only BaseSha..CurrentSha` que toca `ChainRedFiles`) y `ExpectRed` «es siempre el del contrato» (L618), así que no la decide. Las dos divergen cuando la cadena ya acreditó un RED y la entrega no toca `ChainRedFiles` (RED-DIV-EXPECTRED: la regla anterior admitía, y sin designar exigía, un TRX de `RedSha` que 16.8 no pide; caso realista en cualquier delegación posterior a la acreditación) y, en principio, con `ChainRedSha` nulo sin ninguna prueba `ExpectRed` (RED-DIV-NULL). El constructor y el validador implementan ahora la condición de 16.8; en el banco (FX-U1) y en el tipo B1 `ChainRedSha` es nulo, así que sus resultados no cambian (q1 TRX-*, q3, q4).

Observaciones (no se reetiquetan; ninguna cambia un resultado):

1. **Cita de la definición de RED (aplicada por `fix_a7r2_c.py`).** El punto 1 (c) cita ahora «(16.8; 16.9, `Ci` y `Tests`)» y la fila L16 de §2 cita AP 16.8 L616-L617 con el literal de «**exige RED**»; G3 lo comprueba sobre esas líneas del blob fijado de AP (filas con varias fuentes separadas por «;»: un grupo de líneas por fuente; para las filas de una sola fuente el análisis es el mismo de antes).
2. **Exigencia de RED indeterminada (aplicada por `fix_a7r2_c.py`; RED-UNDETERMINED).** La regla 5 (c) dice ahora que, si la exigencia de RED de 16.8 no se puede determinar porque falta la entrega, el TRX de `RedSha` ni se admite ni se exige y se registra la causa: es lo que ya hacían las herramientas (designado: NOT_ADMITTED con la causa UNDETERMINED; sin designar: ninguna ausencia por él y la causa en `RedBasis`), sin cambio de código.
3. **Medición sin hechos del relevo (aplicada).** Para distinguir la medición de una verificación, `a7-designation/2` lleva ahora `Invocation.Kind`; sin esa marca, unos hechos del relevo designados por error en la medición entrarían en el cierre (RELAY-MEASUREMENT-DESIGNATED). Punto del kit: la designación del kit tiene que marcar la invocación de medición.
4. **Piezas del invalidador (aplicada).** Un cambio de `designate_a7.py` o de `derive_r4.py` es un cambio del invalidador aunque el E de la invocación no cambie (INV-DERIVATION); con la lista de la primera versión de la ronda 2 no se habría notado. La disposición que nombre el procedimiento tiene que identificar los dos archivos (o los que implementen la derivación en el kit).
5. **`Ci` sin RED exigido.** Cuando la entrega no exige RED, `Ci` `RedPart` es `not_applicable` «citando la corrida del `ChainRedSha`»: cita la corrida (hechos remotos del relevo), no lee su TRX, y `Tests` `RedPart` es `not_applicable`; por eso el cierre no lleva TRX de esa corrida. Coherente con el punto 1 (c).
6. **RunId de los controles del banco** (punto del kit, sin cambio). El patrón de `RunId` de relay-record/v2 no admite los `RunId` de los controles del banco (p. ej. `…-m7c2`); ya conocido (decisiones §92, punto 6) y sin efecto en R10, que solo compara el SHA-256.
7. **Formato de designación del kit** (punto del kit, sin cambio). El kit de la medición (`a6m7-measure`) usa `a6m7-designation/1`, que no registra los blobs de la delegación, la entrega y el TRX ni los papeles TRX de `RedSha` y RELAY: la próxima preparación del kit tiene que adoptar `a7-designation/2` (o equivalente, con la marca de la medición) y designar el TRX de `RedSha` cuando la entrega exija RED y los hechos del relevo.

## 12. Archivos

Publicables en `docs/automation/evidence/I-62-A7/r2/` (este directorio). `MANIFEST.sha256` lista todos, incluido este documento.

| Archivo | SHA-256 |
|---|---|
| `a7-designation-2.schema.json` | `ae33ad31dde5f38094d766b6a576b3f354ff2e27066fa791718a751c9bf306c0` |
| `a7-guards.py` | `57238da1a6939d09e18936dbc014950dac50d80f2680bcf91101e310d0e445d8` |
| `check_a7_closure.py` | `59e26e118697bc8371e0496d8b44490a5105e74536c5e67ca56343c854bc96b1` |
| `check_citation_blobs.py` | `e99ea7105d82a4262e6998ff767f08341bdae92c24fb31560393f8283e69f737` |
| `derive_r4.py` | `3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426` |
| `designate_a7.py` | `759df4f0332b1e4557ab3460973323e3016e485d6cb92cb1793df56f01842ab9` |
| `designate_m7.py` | `491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6` |
| `lib_r1r4.py` | `c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5` |
| `make_eic_a7.py` | `0e5f9305525f8d0e91d09127dc1a0d2238883a37b28d29b08cfaf98803584aad` |
| `make_eic_verify.py` | `58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2` |
| `preflight_text_r2.py` | `1d85a559e13c3664056d0561e1a49bc600340300aec8c5f5148eff96e3631a06` |
| `q0-env.json` | `4e535d5e7dd860099ae0c510de1551666b357e16ba33b74f00d42a6fcb346bb1` |
| `q1-trx.json` | `21bafeaa1171ade5800a5a63490f18c99bed9d1ebd8159853b70b11dbb056bc4` |
| `q2-relay.json` | `d06e9c38b4278658a8858f533391cd7f5114e9543816a2a6c23f39d829ffc018` |
| `q3-n4-nc1.json` | `a4d04a25335825e73ad6bd2adea038ad5dad84efeea0b2f97f2aaba18d3e7549` |
| `q4-b1.json` | `a1291d85c3d45091ab17db2796888e097725f82bc195ed1d74bd607bebbc618a` |
| `q5-invalidator.json` | `9d18788a567b30a6c1ab45cc53f913c7c0b00a22f1634280dced6ac6f2991f1e` |
| `q6-compat.json` | `7992df2c0327cb6d289c9a52a966ec1b67df8a7e3e260260a68b56b9e109c5f5` |
| `q7-guards-preview.json` | `2a6d4019e0d9aa40b9540a355b1be2babd6ece079321ed44daaeabe78bfed29a` |
| `q7-guards-selftest.json` | `951a9483b265c4475c3802ce53333e5d0ef130301af86fa2938f87fb62634d8d` |
| `q7-guards.json` | `5cbbbd600fa1a889f5c2835e8f685c4671dca9900eb2f02fe2dfa77d58666b9b` |
| `run_a7r2.py` | `10ba82bb29be3381054468ef3483417c29bc140707dd8336cd396d83f64e167f` |
| `summary.json` | `5968133abb6e8975b8046a676a5e49a2a0074db1110d25fb7dc547629d940fa3` |
| `b1-like/main/FX-U2-T1.gate-contract.json` | `b759226ed289425ff972908c16f243dbbb6494eec510a3698e3897da5cae2239` |
| `b1-like/main/deleg.json` | `c524c5905cafba16a2755ec279649c6c8e01d18a4b0cca86a9fa6a82149e547b` |
| `b1-like/main/designation.json` | `1d477205ecb0e07f5fee98950c0d00b2260018ff2d3570d19da27ee54f656947` |
| `b1-like/main/fixture-tests.trx` | `4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513` |
| `b1-like/main/handoff.json` | `e9302caa80b6f87966171bb04ef7c8c33e3effc6731f0b667ba383003db38b4c` |
| `b1-like/main/relay-facts.json` | `a67ff3c7f8797acbdbf1c37d2d47f4ea0c4de238a6d783d6cccf65203e121fab` |
| `b1-like/uc/FX-U2-T1.gate-contract.json` | `7ddd714a92a7065a66dc59d99f804cc776476fd01e425747030f3a7fe414e932` |
| `b1-like/uc/deleg.json` | `d9be6e0d9938a3f8e718fd95857372894960a74bdb343da120c355bd35bdd178` |
| `b1-like/uc/designation.json` | `490206d5e5b0ed55a5893b548da180566acd3508f4e0a0cc80afd59639c19c63` |
| `b1-like/uc/fixture-tests.trx` | `4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513` |
| `b1-like/uc/handoff.json` | `4fd79967851578a3391a9da13b70e696d6470b1e6e2c877220bf92dbedecc267` |
| `b1-like/uc/relay-facts.json` | `56770ad1b616cc1d18233b7e047086542e591ac69d844d7a75a79e13cfb1ba5a` |
| `designations/M.json` | `0b3b1f7eb488362f0816beefd9ede78630794fab467bdfc762d5f0306496c111` |
| `designations/N4.json` | `84e3cc5a23761d02a080440204875c1e17ea0dfd67e95d4d39e6136e4e757c02` |
| `designations/N5.json` | `b7462c8c59ef19a7c17310d704d9ccebeebdd443794bae8ff0586953e545ade2` |
| `designations/N6.json` | `2094a8c61e45c78620395697b8f8584611755fbb0d184cebbbdfda771724430d` |
| `designations/N7A.json` | `0ff1446bf462345be6adb4577b743afc3a56f721090c2ebed73a0a6df90254a7` |
| `designations/N7B.json` | `ce5b569d62806a96eac6620db06cbe8eafe4675f07e0595f913f1cb446430c82` |
| `designations/NC1.json` | `eecab5dedab55ebc2c872d713c57d5849b6e7f5c1294088a1c1c0e11b2e36536` |
| `designations/NC2.json` | `b89bfff037ac7142d67a331a02d9ff361c3e3f7b9ce0d13a698d66baecdc66eb` |
| `designations/V.json` | `72b2b4c4b4210e49bd8ef63d6db9a217e20e34bdbc0055b5f41394a4c22cd0da` |
| `inputs/fixture-tests-red.trx` | `5e388d8774fd0f3cca15d7899a76b2155d2f98532b18b7f6ba0d0a033bde74f0` |
| `inputs/relay-facts-NC2.json` | `cf0467197a01ee076040824780847a13b414de086ed3b1a78be3a8b07bda30c8` |
| `inputs/relay-facts-V.json` | `ab36aa0fda2c54f87f3c461f778daea5f67a4388f9898c5a0f62659b6edb5597` |

