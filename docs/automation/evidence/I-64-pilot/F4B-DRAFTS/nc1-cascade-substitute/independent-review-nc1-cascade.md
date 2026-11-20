# Revisión independiente: `PBT3x-NC1-CASCADE-SIGNAL-01` (firma PD-F4B-01)

**Objeto:** `nc1-cascade\nc1_cascade_signal_substitute.py`, SHA-256 `bf30fe7f68ec14ad9005588b0d0c6f281d4576e8b1209807cfb4995fc7ced2e2`.
**Definición vinculante:** `pbt2ccc\owner-pbt2c-nc1-decision.md`, «Lo que implica», punto 2, condiciones (a) a (f), y punto 3.
**Base:** `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`git show 05beabfd:…/nc1_identity_signal_substitute.py`).
**Método:**
- análisis del código antes de leer `notes.md`, para no anclarme en la lectura del autor;
- ejecuciones con `python -I`, con el cwd en la raíz del repositorio;
- sondas sobre copias de los datos reales;
- prueba de mutación de la autoprueba;
- barrido de solo lectura de las 68 salidas históricas de nc1 de la rama `origin/architecture/workspace-persistente-rackcad`.

No he aplicado ni custodiado nada. Solo he escrito en `nc1-cascade\review\`.

## Veredicto

**APTO TRAS CORRECCIONES.**

**B / I / M = 0 / 6 / 10.**

**Lo que está bien:**
- Los tres casos reales dan lo esperado: PB-T2a `0302` da PASS, PB-T2b `bfda` da FAIL y PB-T2c `ea01` da PASS.
- Con las salidas reales sin tocar no encontré ningún falso PASS.
- (b), (c), (d), (e1) y (f) están comprobadas mecánicamente y sin aflojar.

**Lo que hay que corregir antes de los controles de PB-T3a** son los seis «I»:
- **(a) no está cerrada del todo** (I-1, I-2 e I-3): la comparación de la entrega no distingue `1` de `true` ni ve contenedores vacíos; no se revisa el prompt; y la inexistencia del SHA falso en el repositorio es opcional.
- **La autoprueba** (I-4) no detecta que se quiten 11 de las 14 condiciones que probé.
- **La lista ALIEN** (I-5 e I-6): una de las variantes que añadió el autor (`omitido`) choca con el propio vocabulario del protocolo. Además, la lista solo cubre causas ajenas redactadas como ausencia; ese residuo tiene que quedar declarado.

## Hallazgos

### Importantes (I)

**I-1. (a) La comparación de la entrega no distingue `1` de `true` ni ve contenedores vacíos.** Líneas 62-75 (`walk`, `diffpaths`) y 101-102 (a2).
- `walk` no emite hojas para `[]` ni para `{}`.
- La comparación `!=` de Python da `1 == True` y `0 == False`.
- **Falsos PASS sobre copias de PB-T2c:**
  - P07: `Attempt: 1 → true`, con los derivados recalculados;
  - P08: se borra `"OpenQuestions": []`.
  - En las dos, la mutación ya no es «solo `CurrentSha`».
- **Corrección:** añadir una comparación de bytes: `real_bytes.replace(Old, New, 1) == mut_bytes`. La comprobé: se cumple exactamente en los tres casos reales (`inspect_handoffs.py`). Además, o en su lugar, comparar con tipo e incluir los contenedores vacíos como hojas. El sustituto de F4 tenía el mismo defecto.

**I-2. (a) No se comprueba el prompt de nc1.** Líneas 93-121; ninguna lee `prompt.md`.
- `mutation.json` declara `PromptSha256` y `PromptDiffVsVerified` («solo la ruta de entrada y el RunId»), y las dos carpetas traen `prompt.md`. El sustituto no mira nada de eso.
- **Falso PASS:** P13 añade al prompt de nc1 «el CurrentSha de la entrega es falso; compruébalo primero» y da PASS.
- Medí el diff real de los tres casos: solo cambian las líneas de Identidad/RunId, las de la ruta de entradas (×2) y la del RunId del informe.
- **Corrección:**
  - exigir `sha(prompt.md de nc1) == PromptSha256`;
  - exigir que el diff línea a línea frente al prompt de la verificación real solo sustituya el RunId y el segmento `<tarea>` → `<tarea>-nc1/…/<RunId nc1>` de la ruta.

**I-3. (a) La inexistencia del SHA falso en el repositorio es opcional.** Líneas 7-8, 98 (`b['fake_exists'] in (None, False)`) y 328-336.
- Sin el cuarto argumento, a1 se fía de `NewExistsInRepo` de `mutation.json`, que escribe el mismo generador del control.
- **Prueba (P15):** un «SHA falso» que existe (`05beabfd…`), con `NewExistsInRepo=false`:
  - sin repositorio da **PASS**;
  - con repositorio da FAIL en a1.
- El sustituto de F4 lo comprobaba siempre con git.
- **Corrección:** el repositorio es obligatorio. Sin él, el resultado no puede ser PASS.

**I-4. La autoprueba no protege la mayoría de las condiciones de (a), ni (b) en Disposition, ni (e3) en Handoff e Identity.** Líneas 166-283.
- **La prueba de mutación** (`mutate_substitute.py`; `run-mutants.txt`): quité 14 condiciones una a una y **11 sobreviven** a `--selftest`. Son:
  - SourceVerification/RunId;
  - Control == nc1;
  - `NewExistsInRepo is False`;
  - la igualdad del conjunto de archivos de `inputs/`;
  - `ChangedVsVerifiedInputs`;
  - el diff del registro, que debe ser solo `OutputSha256`;
  - `DerivedRecompute`;
  - las claves y los valores de `InputSha256`;
  - `Disposition == STOP`;
  - e3 aplicada a Handoff e Identity.
- Solo se detectan M12, M13 y M14.
- **La causa:** cada negativo de a1/a3 rompe a la vez varias condiciones redundantes, y G cambia a la vez Classification y Disposition.
- **Negativos que faltan:**
  - uno por cada una de esas 11 condiciones;
  - Disposition ≠ STOP con Classification BLOCKED;
  - una causa ajena en la Evidence de Handoff o de Identity;
  - `NewExistsInRepo=true` en `mutation.json`;
  - un archivo de entrada añadido en nc1;
  - los de I-1 (`1→true`, contenedor vacío), I-2 (prompt), M-1 (FailureClass incoherente) y M-2 (comprobación desconocida).

**I-5. La variante `omitid[oa]s?` choca con el vocabulario del propio protocolo y da falsos FAIL previsibles.** Línea 53.
- **El prompt del Controller** (línea 102 del `prompt.md` de PB-T2c) describe el RED esperado como «build-plugin omitido por su needs».
- **F01:** en `ea01`, cambio «Build Plugin skipped» por esas mismas palabras. El resultado es FAIL en e3, aunque la cascada es idéntica.
- **En el histórico de F4B,** el nc1 de PB-T1b (`a4e8`) escribe en Ci «build plugin omitido» y en Tests «2 omitidas de 18», «sin omitidos». Las tres frases son legítimas y caen en ALIEN (`run-alien-context.txt`).
- **No es uno de los ejemplos del Owner** («no declarado», «null», «ausente»). Es una variante que añadió el autor.
- **Corrección:** quitarla, o restringirla a que el sujeto sea un campo de la entrega.

**I-6. ALIEN solo detecta causas ajenas redactadas como ausencia. Los falsos PASS por otra redacción son un residuo del mecanismo que hay que declarar.** Líneas 43-53 y 154-160.
- **Sondas con falso PASS** (copias de PB-T2c; cada evidencia cita el SHA falso y atribuye además el fallo a otra causa):
  - P01: «falta RedRepairSha en la entrega»;
  - P02: inglés, «the handoff is missing RedRepairSha»;
  - P03: «Worker.Trailer de la entrega está vacío»;
  - P04: «OutputWrittenUtc precede Cession.StartUtc». Es la mala lectura de PB-T2b, que allí quedó en Findings;
  - P05: «incluye src/…/Foo.cs fuera de AllowedWriteScope».
- **No se arregla ampliando la lista sin más.** Medí las palabras candidatas (`candidate_alien.py`; `run-candidate-alien.txt`):
  - `vacio` está en la Evidence de Tests de `ea01` («ExistentesQueLaEntregaNoNombra esta vacio», «git diff RedSha..HEAD -- tests esta vacio»). Añadirla rompería un caso que debe pasar. La exclusión del autor es correcta;
  - incluso con ancla («entrega … vacío») también casa en `ea01`;
  - `empty` y `none` aparecen en decenas de evidencias legítimas («git diff … is empty», «none intersects ForbiddenWriteScope»);
  - `missing`, `absent` y `falta` aparecen en descripciones legítimas;
  - `not declared`, `undeclared` y `not present` tienen cero coincidencias en las 68 salidas históricas y en `0302`/`ea01`.
- **Corrección:**
  1. Añadir solo las formas inglesas de colisión cero: `not declared`, `undeclared`, `not present`, `is null` ya entra por `null`. El propio autor anota en la nota 9 que el Controller escribe a veces en inglés.
  2. Declarar el residuo en `SCOPE` y en el resultado, por ejemplo: «e3 detecta causas ajenas redactadas como ausencia, nulidad o falta de declaración de un campo; no detecta otras atribuciones (fechas, rutas, conteos, trailers)». Así el Owner o el Coordinator saben qué cubre la firma. La decisión dice «Lo comprueba una lista cerrada», así que el mecanismo es fiel; lo que falta es dejar el límite por escrito.

### Menores (M)

**M-1. `FailureClass` sin restricción.** Línea 133, solo informativa.
- **P12:** `FailureClass=Authority`, con Authority en pass, da PASS.
- **Lo que dice el protocolo:** la regla de coherencia del prompt (README §8, línea 121 del prompt de PB-T2c) fija «FailureClass = la primera que no está en pass (o StopCondition)». Si la primera en fail es Handoff, una salida coherente solo puede llevar `Handoff` o `StopCondition`.
- **Recomendación:** exigir `FailureClass ∈ {Handoff, StopCondition}`.
  - No endurece: solo rechaza salidas incoherentes, que encajan en «otra primera en fail».
  - Exigir `== Handoff`, como en F4, sí endurecería: el histórico tiene `StopCondition` con Handoff primera (`2376`, `f93c`).

**M-2. Las comprobaciones fuera de las 14 se ignoran.** Líneas 130, 141 y 307-311.
- **P14:** una decimoquinta comprobación (`Workspace`) en fail da PASS.
- **Recomendación:** exigir `set(Checks de nc1) == set(Checks de la real) == set(ORDER)`.

**M-3. Las palabras sueltas `null`, `ausente`, `ausencia`, `no consta` y `no presente` dan falsos FAIL** (líneas 47-52). Ocurre cuando el sujeto es el SHA falso o el vocabulario de una prueba.
- **Sondas:**
  - F02: «el CurrentSha declarado no consta en el repositorio»;
  - F03: «esperado W, actual null» al describir el RED;
  - F04: «ausente del repositorio».
- **En el histórico:** `b2ea` Identity escribe «RedSha=null coincide con la corrección autorizada», y varios Tests en pass escriben «actual null» o «ausencia de escritura».
- Es un fallo conservador, porque el caso vuelve al Owner, y `null` y `ausente` son ejemplos del propio Owner. Basta documentarlo.

**M-4. Abreviaturas del SHA falso en las demás comprobaciones.** Línea 82.
- **Lo que hace el script:** acepta en Remote, Scope, Ci y Tests el prefijo de exactamente 8 caracteres o más. **F05:** con 7 (`deadbee`, la abreviatura por defecto de git) da FAIL.
- **Es una interpretación obligada por el Owner:** sin ella, Tests de `ea01` («…al CurrentSha deadbeef») no pasa, y el Owner exige que `ea01` pase.
- **Recomendación:** registrar la interpretación en el sustituto: «literal = los 40 hex o su prefijo de 8 o más en las demás; los 40 en Handoff e Identity».

**M-5. La comprobación con git ahora pela a commit.** Línea 336, `cat-file -e <sha>^{commit}`.
- El sustituto de F4 usaba `cat-file -e <sha>`.
- Con el peel, un SHA que exista como árbol o blob cuenta como «no existe», y (a) dice «un SHA que no existe en el repositorio».
- **Recomendación:** volver a `cat-file -e <sha>`, sin `^{commit}`.

**M-6. El «HEAD real» se deduce del `CurrentSha` real sin cruzarlo con nada.** Líneas 88-89 y 162.
- **Recomendación:** comprobar además que el `VerifiedSha` de la salida real coincide con el `CurrentSha` real. Las tres salidas reales lo cumplen. Así lo que da por hecho el docstring (líneas 8-9) queda comprobado mecánicamente.

**M-7. Dos descripciones de la autoprueba son inexactas.**
- **Caso A** (línea 227): la llama «cascada completa de PB-T2a (Handoff, Identity, Remote, Ci, Tests)», pero `0302` también tiene Scope en fail.
- **Caso H2** (línea 253):
  - dice «fuera del conjunto», pero Scope está dentro del conjunto;
  - usa `skipped`, cuando el protocolo usa `not_run` (línea 33 del prompt).
  - **Recomendación:** usar `not_run` en una comprobación de fuera del conjunto, por ejemplo Trailer.

**M-8. Con una entrada malformada, el script rompe con traceback y no escribe el JSON del resultado, en vez de dar FAIL.**
- Ocurre con una Evidence que no es cadena (líneas 79 y 58), un `Result` ausente (línea 310), una entrada sin `worker-handoff.json` (líneas 327 y 340) o un `CurrentSha` que no es cadena cuando hay repositorio (línea 336).
- En todos esos casos no hay PASS. Pero falta el resultado que se registra.

**M-9. «La x se cambia por subtarea» obligaría a editar el artefacto revisado.** Líneas 1-2 y 31-32.
- El Owner nombra un único sustituto, `PBT3x-NC1-CASCADE-SIGNAL-01`, y un único registro, `SATISFIED_UNDER_PBT3x_NC1_CASCADE_SUBSTITUTE`, para PB-T3a, b, c, PB-T4 y PB-T5.
- Si se edita el ID o el SCOPE en cada subtarea, cambia el hash del script y ya no es el que se revisó.
- `SCOPE` dice además «solo F4B PB-T3x» y no menciona PB-T4 ni PB-T5.
- **Recomendación:** no editar nada. Pasar la subtarea como argumento o tomarla de las salidas (`TaskId`), y que el SCOPE nombre las cinco subtareas.

**M-10. Los tres casos reales no forman parte de la autoprueba que se ejecuta en cada uso.** Punto 3 de la decisión: «Entre sus casos de prueba están las salidas reales de nc1…».
- Hoy los tres casos solo se comprueban fuera de la autoprueba, en `runs/`. Los sintéticos A y T2b los imitan, pero no son esos datos.
- **Recomendación:** un modo dorado (`--golden <data>`) que exija PASS, FAIL y PASS con las propiedades exactas, o por lo menos fijar en el resultado los hashes de `runs/*.json`.

## Fidelidad a (a)-(f)

**(a)**
- **Comprobado:**
  - a1: `mutation.json`, 40 hex, distinto del real, `Old` y `New`, RunIds;
  - a2: diff de JSON;
  - a3: solo cambian la entrega y `relay-record-work.json`, este solo en `Outcome.OutputSha256`, y los hashes son coherentes.
- **Abierto:** I-1, I-2, I-3 y M-5.

**(b)** Fiel: `EXECUTION_BLOCKED`, `STOP` y `'S-04' in TriggeredStopConditions` (líneas 127-128).

**(c)** Fiel: la primera `fail` en el orden canónico es Handoff (líneas 130-134).
- Queda implicada por d1 y d2, porque Termination no está en el conjunto.
- `FailureClass`: ver M-1.

**(d)** Fiel:
- d1: subconjunto de las 6;
- d2: con Handoff e Identity;
- d3: todo lo demás en `pass` en nc1 y en la verificación real. Un `not_run` cae, como debe.
- **La exigencia de 14/14 y `EXECUTION_VERIFIED` en la verificación real** no endurece nada. El prompt fija «EXECUTION_VERIFIED si y solo si las 14 están en pass», y los controles solo se lanzan tras una verificación VERIFIED.
- **Abierto:** M-2.

**(e)**
- e1: Handoff e Identity con los 40 hex. Fiel a «lo escriben literal», y a la regla (3) del prompt para Identity.
- e2: los 40 hex, el prefijo de 8 o la frase cerrada. La frase se busca sobre texto normalizado y con todas las variantes de género y número. Fiel, con M-4.
- e3: ver I-5, I-6 y M-3.

**(f)** Fiel: los 40 hex del HEAD real en Identity (línea 162). Abierto: M-6.

## Dudas del autor (`notes.md`)

1. **El prefijo de 8 como literal en Remote, Scope, Ci y Tests:** es fiel y lo impone el Owner, porque `ea01` debe pasar. Exigir los 40 en Handoff e Identity sigue la letra de (e) y la regla (3) del prompt. Falta registrar la interpretación (M-4).
2. **`FailureClass` no exigido:** es fiel a la letra de (c), pero admite salidas incoherentes. Exigir `FailureClass ∈ {Handoff, StopCondition}` no endurece, porque son los dos únicos valores que el protocolo admite con Handoff como primera (M-1). Igualarlo a Handoff, como en F4, sí endurecería.
3. **La lista de causas ajenas:**
   - los ejemplos del Owner están cubiertos;
   - excluir `vacio` y `falta` está justificado: lo medí, `vacio` casa en `ea01` y `falta` en evidencias legítimas;
   - `omitido` hay que retirarlo (I-5);
   - el residuo hay que declararlo (I-6).
4. **La verificación real exigida 14/14:** es fiel. Equivale a `EXECUTION_VERIFIED` según la regla de coherencia del prompt.

El resto de notas no cambia nada:
- la nota 5 (Identity sola no cumple la firma) es coherente con «siempre Handoff e Identity»;
- la nota 6 queda cubierta por M-6.

**Sobre PD-F4-04 como caso particular:** el sustituto de F4 aceptaba prefijos de 8 en las evidencias de Identity y Handoff; PD-F4B-01 exige los 40. Por eso el caso particular vale para la forma del conjunto en fail, no para el texto. El nc1 de F4-T3C2 (`e8d4`) sí cumple las propiedades de evidencia nuevas (`run-history-cascades.txt`).

## Ejecuciones

Todas con `python -I`, con el cwd en `<checkout-principal>`. Las salidas están en `review\`.

| Ejecución | Resultado |
|---|---|
| `nc1_cascade_signal_substitute.py --selftest` | `SELFTEST_PASS=True DETERMINISTIC=True CASES=35`. Idéntica byte a byte a `selftest.txt` (`run-selftest.txt`). |
| PB-T2a `t2a-real` / `t2a-nc1`, sin repositorio y con él | **PASS** (exit 0). Fail = Handoff, Identity, Remote, Scope, Ci, Tests. `FakeExistsInRepoChecked`: null / false. |
| PB-T2b `t2b-real` / `t2b-nc1`, sin repositorio y con él | **FAIL** (exit 1) en c, d1, d2, e2 y e3. Primera en fail: Identity. Trailer en fail. ALIEN casa en Tests («no esta declarado») y en Trailer («null»). |
| PB-T2c `t2c-real` / `t2c-nc1`, sin repositorio y con él | **PASS** (exit 0). Fail = las 6. |
| Comparación con `runs/*.json` | Idénticos, salvo las rutas. |
| Integridad de `data/` | Ningún archivo de `data/` es más nuevo que mis corridas, y una segunda corrida de PB-T2c vuelve a dar PASS (`run-t2c-recheck.json`). |
| `probes.py` (`run-probes.txt`, `probes\summary.json`) | 21 sondas, en la tabla de abajo. |
| `mutate_substitute.py` (`run-mutants.txt`) | 14 mutantes: 11 sobreviven y 3 se detectan. |
| `scan_history.py` y `alien_context.py` | En 68 salidas históricas de nc1 (solo `git show`): 7 de 159 evidencias en fail casan con ALIEN. Las de `a4e8` (`omitido`) y `b2ea` (`null`) son legítimas. |
| `history_cascades.py` | De 22 salidas históricas con forma de cascada, 12 cumplen las propiedades de evidencia, `e8d4` y `ea01` entre ellas. Las demás caen por e1, e2 (inglés), b, d1 o e3 (`b2ea`, `null`). Es solo informativo. |
| `candidate_alien.py` | Coincidencias de las palabras candidatas: `vacio` casa en `ea01`; `empty`, `none`, `missing` y `absent` son ubicuos; `not declared`, `undeclared` y `not present` tienen cero coincidencias. |

**Sondas** (copias en `review\probes\<id>\`; los `.trx` son enlaces duros que no se modificaron; los derivados se recalcularon cuando cambiaba la entrega):

| Sonda | Caso | Esperado | Sustituto | Resultado |
|---|---|---|---|---|
| P01 «falta RedRepairSha» en Remote | T2c | FAIL | PASS | **falso PASS** (I-6) |
| P02 «missing», en inglés, en Tests | T2c | FAIL | PASS | **falso PASS** (I-6) |
| P03 «Worker.Trailer … vacío» en Ci | T2c | FAIL | PASS | **falso PASS** (I-6) |
| P04 fecha (OutputWrittenUtc precede) en Handoff | T2c | FAIL | PASS | **falso PASS** (I-6) |
| P05 ruta fuera de AllowedWriteScope en Scope | T2c | FAIL | PASS | **falso PASS** (I-6) |
| P06 Authority en fail | T2c | FAIL | FAIL d1 | correcto |
| P07 entrega `Attempt 1→true` | T2c | FAIL | PASS | **falso PASS** (I-1) |
| P08 entrega sin `OpenQuestions: []` | T2c | FAIL | PASS | **falso PASS** (I-1) |
| P09 entrega con `WorkCompleted` cambiado | T2c | FAIL | FAIL a2 | correcto |
| P10 Identity sin el HEAD (solo prefijo) | T2c | FAIL | FAIL f | correcto |
| P11 Handoff en pass, `FailureClass=Handoff` | T2c | FAIL | FAIL c, d2 | correcto |
| P11b Termination en fail, `FailureClass=Handoff` | T2c | FAIL | FAIL c, d1 | correcto |
| P12 `FailureClass=Authority` | T2c | FAIL | PASS | **falso PASS** (M-1) |
| P13 prompt de nc1 con una pista | T2c | FAIL | PASS | **falso PASS** (I-2) |
| P14 decimoquinta comprobación en fail | T2c | FAIL | PASS | **falso PASS** (M-2) |
| P15 SHA existente con `NewExistsInRepo=false` | T2c | FAIL | FAIL a1 con repositorio; **PASS sin él** | I-3 |
| F01 «build-plugin omitido por su needs» en Ci | T2c | PASS | FAIL e3 | **falso FAIL** (I-5) |
| F02 «no consta en el repositorio» en Remote | T2c | PASS | FAIL e3 | falso FAIL (M-3) |
| F03 «actual null» en Tests | T2c | PASS | FAIL e3 | falso FAIL (M-3) |
| F04 «ausente del repositorio» en Scope | T2a | PASS | FAIL e3 | falso FAIL (M-3) |
| F05 `CurrentSha deadbee` (7) en Tests | T2c | PASS? | FAIL e2 | conservador (M-4) |

## Recomprobación (2026-11-19)

**Objeto:**
- `nc1_cascade_signal_substitute.py`, SHA-256 `87F13FE1698FB8BDA9B899AEBAD0E534DEEEDCC7BB3AF5305791E2FA16196BB6`, 567 líneas;
- `nc1_cascade_golden.json`, SHA-256 `8FC80F76…91DAC`, igual al `GOLDEN_SHA256` de la línea 58;
- `build_golden.py`.

**Decisiones de la sesión que doy por fijadas:**
- I-6: solo `not declared`, `undeclared` y `not present`, con el límite declarado;
- M-3 y M-4: solo se documentan;
- el registro va en mayúsculas.

**Método:**
- volví a ejecutar mis 21 sondas, en `review\v2\probes_v2.py`, con `--task VALIDATION`;
- adapté mis 14 mutantes al código por átomos y añadí 19 más (`review\v2\mutate_v2.py`);
- comprobé el dorado contra `data/` con un script mío, sin usar `build_golden.py` (`review\v2\golden_check.py`);
- leí los generadores de nc1;
- añadí 6 sondas nuevas y 10 entradas malformadas.

No he usado los resultados de `work\` del autor.

### Veredicto de la recomprobación

**APTO TRAS CORRECCIONES. B / I / M nuevos = 0 / 2 / 6.**

**Lo que ya está bien:**
- Los 16 hallazgos anteriores están corregidos o documentados según lo decidido.
- Los tres casos reales siguen dando PASS, FAIL y PASS.
- Mis 16 mutantes adaptados se detectan todos.
- El dorado es fiel byte a byte.

**Lo que queda antes de PB-T3a** son dos «I» nuevos:
- **I-7:** `--task` no está ligado a la tarea de los datos. Con `--task PBT3A` sobre PB-T2c sale el registro de PB-T3a.
- **I-8:** el repositorio obligatorio no está anclado. El directorio personal del usuario es un repositorio git sin commits, así que cualquier ruta bajo él, el scratchpad incluido, se acepta como «el repositorio», y en él ningún SHA existe.

### 1. Estado de cada hallazgo anterior

| Hallazgo | Estado | Dónde | Prueba |
|---|---|---|---|
| I-1 (`1→true`, contenedores vacíos) | **Corregido** | Líneas 69-83 (hojas con tipo; contenedores vacíos como hoja); 121-124 (`rb.replace(Old, New, 1) == mb`) | P07 y P08 dan FAIL en los dos átomos de a2. Ver M-14 (nuevo). |
| I-2 (prompt) | **Corregido** | Líneas 144-151 (a4: `PromptSha256`, prompt real con solo la ruta y el RunId cambiados, texto exacto de `PromptDiffVsVerified`) | P13 da FAIL en a4. Ver M-11 (nuevo). |
| I-3 (repositorio opcional) | **Corregido en parte** | Líneas 515-516 (repositorio obligatorio); 523-530 | P15 da FAIL en a1 con repositorio; P15b da FAIL con `Reason` sin él. **Ver I-8 (nuevo).** |
| I-4 (autoprueba) | **Corregido** en el nivel de átomo | Líneas 98-191 (44 átomos); 444-458 (cobertura: cada átomo falla en algún caso) | Mis 16 mutantes adaptados (M01a-M14) se detectan todos. Quedan huecos dentro de los átomos: M-15 (nuevo). |
| I-5 (`omitido`) | **Corregido** | Líneas 53-56 (sin `omitid`); caso A7 (línea 314) | F01 da PASS. |
| I-6 (residuo de ALIEN) | **Corregido según lo decidido** | Línea 56 (los tres patrones en inglés); límite declarado en el docstring (27-30), `SCOPE_TEXT` (36-38) y `LIMITS[0]` (40), que salen en el resultado (509) | P01-P05 siguen en falso PASS, como límite declarado. |
| M-1 (`FailureClass`) | **Corregido** | Línea 164 (Handoff o StopCondition) | P12 da FAIL en `c.FailureClassIsHandoffOrStopCondition`. Hueco: M30 sobrevive (M-15). |
| M-2 (comprobaciones desconocidas) | **Corregido** | Línea 171 (`set(nc) == set(ORDER) == set(real)`) | P14 da FAIL. Ver M-12 (nuevo, claves duplicadas). |
| M-3 | **Documentado** | `LIMITS[1]` (41); `SCOPE_TEXT` | F02, F03 y F04 siguen en falso FAIL conservador. |
| M-4 | **Documentado** | `LIMITS[2]` (42); docstring (29-30) | F05 sigue en falso FAIL conservador. |
| M-5 (`^{commit}`) | **Corregido** | Línea 530 (`cat-file -e <sha>`) | — |
| M-6 (`VerifiedSha`) | **Corregido** | Línea 190 (`f.HeadEqualsRealVerifiedSha`) | Caso `f-Verified`. |
| M-7 (descripciones) | **Corregido** | Línea 308 (A, «sin Scope»); 366 (`d3-NotRun`, `not_run` en Trailer) | — |
| M-8 (entradas malformadas) | **Corregido en lo esencial** | Líneas 86-90 (`text()`); 202-207; 543-546 | X01-X07 y X10 dan FAIL con `Reason` y escriben la salida. Resto en M-16 (nuevo). |
| M-9 (ID y SCOPE por subtarea) | **Corregido** | Líneas 2-3, 35-36 y 495-514 (`--task` obligatorio; el texto no se edita); 541 (registro `SATISFIED_UNDER_PBT3A_NC1_CASCADE_SUBSTITUTE`, mayúsculas, como decidió la sesión) | **Ver I-7 (nuevo).** |
| M-10 (casos reales en la autoprueba) | **Corregido** | Líneas 57-60 y 421-441 (dorado con SHA fijado); 452-458 | `golden_check.py`: fiel byte a byte. Límite declarado en `LIMITS[3]`: el dorado usa `NewExistsInRepo` y no git. |

### 2. Las 21 sondas y los mutantes contra el código nuevo

**Sondas** (`review\v2\run-probes-v2.txt` y `review\v2\probes\summary.json`):

| Grupo | Sondas | Lo esperado | Obtenido |
|---|---|---|---|
| Límite declarado I-6 | P01, P02, P03, P04, P05 | Falso PASS | **Falso PASS en las cinco**, como se declaró |
| Ahora corregidas | P06, P07, P08, P09, P10, P11, P11b, P12, P13, P14, P15 y P15b (sin repositorio) | FAIL | FAIL en todas, cada una con el átomo correspondiente |
| I-5 | F01 | PASS | PASS |
| Documentadas M-3 y M-4 | F02, F03, F04, F05 | Falso FAIL conservador | Falso FAIL conservador (e3, e3, e3 y e2) |

**Mutantes** (`review\v2\run-mutants-v2.txt`): 35 en total; 24 se detectan y 11 sobreviven.
- **Mis 14 originales, adaptados a átomos (16 al desdoblar M01 y M12):** se detectan los 16.
- **Los 19 nuevos, dentro de los átomos y de relajación:** sobreviven 11. Los detalla M-15 (nuevo).

### 3. Lo nuevo

**Los casos dorados.**
- El SHA del archivo coincide con `GOLDEN_SHA256`.
- `golden_check.py` comprueba cada caso con 6 o 7 comprobaciones por lado: el SHA de todas las entradas, el prompt, la entrega y el registro byte a byte, `output.json` y `mutation.json` parseados, y los RunId. **Todo coincide** (`GOLDEN_FIDELITY=True`).
- Los átomos que se esperan de `t2b` (líneas 59-60) son los que da la corrida real con `data/`.
- Si el dorado falta o cambia, la autoprueba falla (líneas 426-427) y el uso real da FAIL (521-522).
- M29 (quitar la comprobación del SHA) sobrevive, como era de esperar: la autoprueba no puede ejercitar la manipulación de su propio dorado. La custodia de `ScriptSha256`, que va en el resultado (línea 509), la cubre.

**El prompt de nc1 (I-2) y el texto de `PromptDiffVsVerified`.**
- **El texto es el que escribe el generador.** Los tres `mutation.json` reales dicen exactamente «solo la ruta de entrada y el RunId de la invocacion (README 10)», que es `PROMPT_DIFF_TEXT` (línea 50). Es una constante en todos los generadores de nc1 que encontré:
  - `pbt2acc\make_pbt2a_control.py:107`;
  - `pbt2bcc\make_pbt2b_control.py:107`;
  - `pbt2ccc\make_pbt2c_control.py:107`;
  - **`pbt3acc\make_pbt3a_control.py:107`**, el de PB-T3a;
  - y los de F2, F3 y F4.
- **El `PromptSha256` de los tres coincide** con el SHA del `prompt.md` de nc1.
- **La transformación también coincide.** El generador hace `.replace('<tarea>/1/<VER>', '<tarea>-nc1/1/<RID>').replace('RunId <VER>', 'RunId <RID>')` y además comprueba `VER not in pr2`. Así que la sustitución completa de la línea 150 da el mismo resultado.
- El intento lo toma de la salida real (línea 146) y no de un `1` fijo; es correcto.

**`--task`.**
- Es obligatorio (línea 511); sin él, o con `PBT9`, sale FAIL con `Reason` (X07).
- `VALIDATION` no genera registro.
- El Id y el registro se derivan de la etiqueta (líneas 508 y 541).
- **Falta ligarlo a los datos: I-7.**

**Las 44 condiciones elementales.**
- Las conté en el código: a1 11, a2 2, a3 9, a4 3, b 3, c 2, d1 1, d2 2, d3 4, e1 2, e2 1, e3 2, f 2. Son 44.
- `ATOMS_NOT_COVERED []`.
- No todas son elementales: `a3.DerivedRecomputeMatches` reúne 6 condiciones, `a4.PromptIsReal…` 5, y `a2.Bytes…` y `f.IdentityEvidenceHasRealHead` 3 cada una. Por eso sobreviven M15 y M17 (M-15).

**Las entradas malformadas** (X01-X10): todas dan FAIL con `Reason` y escriben la salida, salvo X09 (M-16):
- falta `prompt.md`;
- `output.json` no es JSON;
- Evidence `null`;
- falta `Result`;
- falta `mutation.json`;
- el directorio real no existe;
- `--task` inválido;
- `Checks` en forma de lista.

**Repositorio.** X08 (el cuarto argumento no es un repositorio) da **PASS**: es I-8.

### 4. Sondas propias sobre el código nuevo

| Sonda | Esperado | Sustituto | Resultado |
|---|---|---|---|
| N01 `--task PBT3A` sobre PB-T2c sin tocar | FAIL o sin registro | PASS con `SATISFIED_UNDER_PBT3A_NC1_CASCADE_SUBSTITUTE` | **Falso PASS** (I-7). `work\task3a.json` del autor muestra lo mismo. |
| N02 SHA existente (05beabfd) como falso, contra un repositorio ajeno vacío (`git init` en `review\v2`) | FAIL | PASS | **Falso PASS** (I-8) |
| X08 el cuarto argumento es una carpeta de sondas del scratchpad | FAIL | PASS | **Falso PASS** (I-8). `rev-parse` sube hasta el repositorio vacío del directorio personal. |
| N03 la misma pista en el prompt real y en el de nc1, con `PromptSha256` actualizado | FAIL | PASS | **Falso PASS** (M-11) |
| N04 `output.json` de nc1 con `"Authority"` dos veces: fail y luego pass | FAIL | PASS | **Falso PASS** (M-12) |
| N05 la salida de nc1 declara otro TaskId y otro Attempt | FAIL | PASS | **Falso PASS** (M-13) |
| F06 la entrega real cita su CurrentSha en un campo anterior; la copia cambia solo el campo, como hace el generador de PB-T3a | PASS | FAIL en `a2.Bytes…` | falso FAIL conservador (M-14) |

### Hallazgos nuevos

#### Importantes (I)

**I-7. `--task` no está ligado a la tarea de los datos.** Líneas 507-508 y 541-542.
- **N01:** `--task PBT3A` sobre las salidas de PB-T2c (TaskId `F4B-PBT2C-WORKSPACE-CARRYOVER`) da PASS y el registro `SATISFIED_UNDER_PBT3A_NC1_CASCADE_SUBSTITUTE`.
- **El riesgo:** un nc1 de otra subtarea, o una validación sobre una corrida anterior lanzada con la etiqueta equivocada, deja un registro falso.
- **Corrección:**
  - con `--task` distinto de `VALIDATION`, exigir que el `TaskId` de la salida real y el de la de nc1 sean iguales y empiecen por `F4B-<TAREA>-`. PB-T3a es `F4B-PBT3A-FULL-WRITE-MODEL`, según `pbt3acc\make_pbt3a_control.py:12`;
  - exigir que `SourceInputs` de `mutation.json` contenga ese TaskId;
  - con `VALIDATION`, no exigirlo.

**I-8. El repositorio obligatorio no está anclado al proyecto.** Líneas 523-530.
- **Lo que se comprueba hoy:** solo `rev-parse --git-dir` y que el SHA falso no exista.
- **El problema en esta máquina:** el directorio personal del usuario es un repositorio git sin commits. Cualquier ruta bajo él, el scratchpad incluido, se resuelve a ese repositorio, y en él no existe ningún SHA.
- **Pruebas:**
  - X08 (repositorio = una carpeta de sondas) da PASS;
  - N02 (el SHA existente `05beabfd` como «falso», contra un repositorio vacío) da PASS. Es decir, P15 vuelve a pasar con solo equivocarse de ruta.
- **Corrección:** exigir que `git -C <repo> cat-file -e` encuentre el `CurrentSha` real y el `BaseSha` de la entrega real. Si no los encuentra, el repositorio no es el del proyecto y el resultado es FAIL.

#### Menores (M)

**M-11. El prompt de la verificación real no está anclado.** Líneas 144-150.
- **N03:** la misma pista en los dos prompts, con `PromptSha256` actualizado, da PASS.
- **Hay un ancla disponible:** el `relay-record.json` de la verificación real guarda el SHA del prompt («prompt SHA-256 1DD08635…»), aunque en texto libre.
- **Recomendación:** exigir que el SHA de `prompt.md` real aparezca ahí.
- Manipular la copia real queda fuera del modelo de amenaza habitual; por eso es menor.

**M-12. Las claves JSON duplicadas se aceptan en silencio.** Líneas 476-481 (`json.load`).
- **N04:** una salida con `Authority` en fail y después en pass da PASS.
- **Recomendación:** `object_pairs_hook` que rechace duplicados en `output.json`, `mutation.json` y las entregas. Hoy `checks_of` también colapsaría Ids repetidos en la forma de lista.

**M-13. No se comparan la identidad de la salida de nc1 con la de la real.** Línea 146, que solo usa la salida real.
- **N05:** `TaskId` y `Attempt` distintos en la salida de nc1 dan PASS.
- **Recomendación:** exigir que `TaskId`, `Attempt`, `DelegationRunId` y `WorkRunId` coincidan con los de la verificación real del mismo intento. Se puede hacer junto con I-7.

**M-14. El átomo de bytes de a2 usa la primera aparición de `Old`, y el generador de PB-T3a sustituye el campo.** Línea 122.
- `pbt3acc\make_pbt3a_control.py:48` sustituye el campo con `re.search(rb'("CurrentSha"\s*:\s*")<CUR>"')`.
- **F06:** si la entrega real cita su CurrentSha antes de ese campo, por ejemplo en un texto, sale un falso FAIL.
- Es conservador, y en los tres casos reales el campo es la primera aparición.
- **Recomendación:** sustituir con la misma expresión del campo que usa el generador.

**M-15. Huecos de la autoprueba dentro de los átomos.** Casos en las líneas 307-397. Sobreviven:
- M15: `DerivedRecompute` sin `Field`;
- M17: a4 sin `seg_r in rt`;
- M19-M24: seis patrones de ALIEN sin caso propio (`nulo`, `ausencia`, `no consta`, `sin declarar`, `no (se) declara(n)`, `no presente`);
- M30: `FailureClass` que admite también Identity;
- M31: S-04 relajado a cualquier `S-0x`;
- M29: es por construcción.

**Negativos que faltan:**
- un caso por patrón de ALIEN;
- `fc='Identity'` con Handoff primera;
- `stops=('S-07',)`;
- `DerivedRecompute` con otro `Field`.

La afirmación de `notes.md` de que «0 sobreviven» vale para forzar átomos y las 4 relajaciones del autor. No vale dentro de los átomos.

**M-16. Dos restos de M-8.**
- **X09:** si la ruta de salida está en un directorio que no existe, hay traceback y no se escribe resultado, porque `write_out` está fuera del `try` (línea 547).
- **X10:** `checks_of` admite `Checks` en forma de lista (líneas 223), pero `main` indexa como diccionario (línea 538), así que una salida en lista siempre da FAIL. Es conservador; habría que elegir una sola forma.

**Una observación, no un hallazgo.** `pbt3acc\make_pbt3a_control.py` define `recompute_reissue()` para un `reissue-record.json`, pero no lo llama en nc1.
- Si las entradas de PB-T3a llevaran ese archivo y hubiera que recalcularlo, a3, que exige exactamente dos archivos cambiados y un solo `DerivedRecompute`, daría FAIL: es conservador.
- Conviene confirmar antes de PB-T3a que su verificación no trae `reissue-record.json`.

### Ejecuciones de la recomprobación

Todas con `python -I`, con el cwd en `<checkout-principal>`. Salidas en `review\v2\`.

| Ejecución | Resultado |
|---|---|
| `--selftest` | `SELFTEST_PASS=True DETERMINISTIC=True SYNTHETIC=69 GOLDEN=3`, `ATOMS_NOT_COVERED []`. Idéntica a `selftest.txt` (`run-selftest.txt`). |
| PB-T2a `--task VALIDATION`, con repositorio | **PASS**, fail = las 6, `FakeShaFoundByGit=false`, `NOT_RECORDED` (`run-t2a.json`) |
| PB-T2b ídem | **FAIL** en c.FirstFailIsHandoff, d1, d2.HandoffFails, e2 y e3.NoAlienInOthers (`run-t2b.json`) |
| PB-T2c ídem | **PASS**, fail = las 6 (`run-t2c.json`) |
| `golden_check.py` | `GOLDEN_FIDELITY=True`, con el SHA igual al fijado (`run-golden-check.txt`) |
| `probes_v2.py` | 21 sondas originales, 6 nuevas (N01-N05 y F06) y 10 malformadas (`run-probes-v2.txt`) |
| `mutate_v2.py` | 35 mutantes: 24 se detectan y 11 sobreviven (`run-mutants-v2.txt`) |
| Integridad de `data/` | Ningún archivo es más nuevo que el inicio de la recomprobación. |

Los `.trx` de las sondas son enlaces duros que no se modificaron. `review\v2\probes\_emptyrepo` es un `git init` vacío para N02.

## Recomprobación 2 (2026-11-19)

**Objeto:**
- `nc1_cascade_signal_substitute.py`, SHA-256 `41440EDC4A963BEF890E4336DCCB885837424008C027CDB172D2B926E0F49B40`, 740 líneas;
- `nc1_cascade_golden.json`, SHA-256 `8EB3A991371777AB218E434F4ECF43DC1FD42403C64DBF5C9A949084EFA88C24`, igual al `GOLDEN_SHA256` de la línea 66;
- `build_golden.py`.

**Lo que ejecuté** (todo mío, en `review\v3\`; no usé `work\` del autor):
- todas las sondas anteriores y 18 nuevas (`probes_v3.py`);
- los 35 mutantes anteriores y 13 nuevos (`mutate_v3.py`);
- la fidelidad del dorado, ahora con el `relay-record.json` (`golden_check_v3.py`).

### Veredicto

**APTO. B / I / M nuevos = 0 / 0 / 2.**
- I-7, I-8 y M-11 a M-16 están corregidos.
- Las dos menores nuevas (M-17 y M-18) no bloquean los controles de PB-T3a: la primera exigiría manipular la custodia de la verificación real, y la segunda es de cobertura de la autoprueba, no un error del código revisado.

**Antes de usarlo:** el `apply_nc1_cascade_pbt3a.sh` de PB-T3a tiene el marcador `NC1_SHA_REVIEWED=POR_RELLENAR`. Hay que poner este SHA, `41440EDC…`, y copiar el dorado `8EB3A991…` junto al script, como ya hace ese mismo script.

### 1. Estado de los hallazgos de la recomprobación

| Hallazgo | Estado | Dónde | Prueba |
|---|---|---|---|
| I-7 (`--task` sin ligar) | **Corregido** | Líneas 7-8; 204-210 (propiedad g: el TaskId real empieza por `F4B-<TAREA>-`, el de nc1 es igual y `SourceInputs` lo contiene); 692-693 (VALIDATION solo con los pares dorados); 710 (registro solo con etiqueta) | N01 da FAIL en `g.RealTaskIdMatchesTask`. V1 (par cruzado) da FAIL con `Reason`. V4 (`PBT3`) da FAIL. V3 (`validation` en minúsculas) da PASS sin registro. |
| I-8 (repositorio sin anclar) | **Corregido** | Líneas 9-10; 159-160 (`a1.RepoHasRealCurrentSha` y `a1.RepoHasRealBaseSha`); 648-652 (`git_has`); 696-698 | N02 y X08 dan FAIL en los dos átomos; el repositorio vacío del perfil queda rechazado. G1 (subcarpeta `docs` del checkout) y G2 (repositorio vacío con `alternates` al del proyecto) dan PASS. G3 (P15 contra el repositorio bueno) da FAIL en `a1.GitConfirmsNotExists`. |
| M-11 (prompt real sin anclar) | **Corregido** | Líneas 115-131 (`prompt_shas`); 198-202 (`a4.RealPromptShaInRelayRecord`; sin candidatos, error) | N03 da FAIL en el átomo. R1 (solo la clave `PromptSha256`) da PASS. R3 (falta `relay-record.json`) da FAIL con `Reason`. Ver M-17 (nuevo). |
| M-12 (claves duplicadas) | **Corregido** | Líneas 101-112 (`strict_pairs`, `jl`); 607-609; 280-286 (Ids repetidos en lista) | N04 (`output.json`), D2 (`mutation.json`) y D3 (`relay-record.json` real) dan FAIL con `Reason`. |
| M-13 (identidad de nc1) | **Corregido** | Líneas 207-209 (TaskId igual; Attempt igual y del mismo tipo) | N05 da FAIL en los dos átomos de g. T1 (`Attempt "1"`) da FAIL. |
| M-14 (primera aparición) | **Corregido** | Líneas 165-168 (sustitución del campo `"CurrentSha": "<Old>"`, una vez, como el generador) | F06 da PASS. M44 (sin `count=1`) se detecta por A10. |
| M-15 (huecos dentro de los átomos) | **Corregido** | Casos en las líneas 474-488 | M15, M17, M19-M24, M30 y M31 se detectan ahora. |
| M-16 (salida y `Checks` en lista) | **Corregido** | Líneas 635-645 (crea la carpeta; si no puede, exit 2); 280-286 y 705 (`checks_of` también en `main`) | X09 da PASS y escribe el resultado en una carpeta nueva. X10 (`Checks` en lista) da PASS. |

### 2. Las ejecuciones

**Autoprueba** (`run-selftest.txt`):
- `SELFTEST_PASS=True DETERMINISTIC=True SYNTHETIC=97 GOLDEN=3`;
- 7 pruebas unitarias en OK;
- `ATOMS_NOT_COVERED []`;
- idéntica a `selftest.txt`.

**Los tres casos reales**, con `--task VALIDATION` y el checkout principal (`run-t2a/b/c.json`):
- PASS, FAIL y PASS;
- PB-T2b falla en c, d1, d2, e2 y e3;
- `RepoHasRealCurrentSha` y `RepoHasRealBaseSha` dan true;
- hay un solo candidato de SHA de prompt por caso.

**El dorado** (`run-golden-check.txt`):
- `GOLDEN_FIDELITY=True`;
- el lado real lleva el `relay-record.json` exacto de `data/` y no tiene claves de más;
- el SHA del prompt real está en su `relay-record.json`.

**N01-N05, X08 y X09** (`run-probes-v3.txt`):
- N01-N05 y X08 dan FAIL, como se espera;
- X09 da PASS escribiendo la salida.

**El resto de las sondas anteriores:**
- P01-P05 siguen en falso PASS: es el límite declarado;
- F02-F05 siguen en falso FAIL conservador;
- P06-P15b dan FAIL;
- F01, F06 y X10 dan PASS.

**Mutantes** (`run-mutants-v3.txt`): 48 en total; 41 se detectan, 6 sobreviven y 1 no aplica.
- **Los 19 de la recomprobación (M15-M33):**
  - 17 se detectan;
  - M29 sobrevive;
  - M33 no aplica.
- **De acuerdo con el autor:**
  - **M29** sobrevive por construcción: la autoprueba no puede ejercitar la manipulación de su propio dorado, y `LIMITS[4]` lo declara (custodia de `ScriptSha256` y `GoldenSha256`);
  - **M33** ya no aplica: a2 sustituye el campo, no los bytes. Su equivalente es mi M44 (quitar `count=1` en la sustitución del campo), y se detecta por A10.
- **Los 13 nuevos (M34-M46):** 8 se detectan y 5 sobreviven. Los que sobreviven son M35, M36, M45, M46 y M42 (M-18).

### 3. Sondas nuevas sobre lo nuevo

| Sonda | Esperado | Sustituto | Resultado |
|---|---|---|---|
| V1 VALIDATION, real de T2c con nc1 de T2a (par no dorado) | FAIL | FAIL, `Reason`: solo pares dorados | correcto |
| V2 ídem, con el RunId de la salida de nc1 cambiado a `ea01` para formar un par dorado | FAIL | FAIL en 15 átomos (a1, a2, a3, a4, f, g) | correcto: pasar la puerta de los pares no basta |
| V3 `--task validation` en minúsculas | PASS sin registro | PASS, `NOT_RECORDED` | correcto |
| V4 `--task PBT3` | FAIL | FAIL, `Reason` | correcto |
| R1 SHA del prompt solo en la clave `PromptSha256` | PASS | PASS | correcto |
| R2 «SHA-256 del prompt <hex>», otra redacción | PASS? | FAIL, `Reason`: sin candidatos | conservador. El script de relevo de PB-T3a escribe «prompt SHA-256» (`pbt3acc\relay_pbt3a_verify.py:77`), así que no le afecta. |
| R3 falta `relay-record.json` real | FAIL | FAIL, `Reason` | correcto |
| R4 pista en los dos prompts y un **segundo** «prompt SHA-256» en el relay real | FAIL? | **PASS** | M-17: requiere manipular la custodia |
| R5 pista en los dos prompts y el SHA manipulado en una clave `PromptShaPlanning` del relay real | FAIL | **PASS** | M-17: requiere manipular la custodia |
| G1 repositorio = subcarpeta `docs` del checkout | PASS | PASS | correcto |
| G2 repositorio vacío con `alternates` al del proyecto | PASS | PASS | correcto. Un primer intento dio FAIL por un CR que escribió mi sonda en `alternates`; corregido en la sonda, no en el sustituto. |
| G3 P15 contra el repositorio bueno | FAIL | FAIL en `a1.GitConfirmsNotExists` | correcto |
| D2 y D3 claves duplicadas en `mutation.json` y en el relay real | FAIL | FAIL, `Reason` | correcto |
| T1 `Attempt "1"` en nc1 | FAIL | FAIL en `g.Nc1AttemptEqualsReal` | correcto |
| W1 otros `DelegationRunId` y `WorkRunId` en nc1 | PASS? | PASS | aceptado (duda 2) |

### 4. Las dudas del autor

**Duda 1 (VALIDATION solo con los pares dorados): de acuerdo.**
- `VALIDATION` nunca genera registro (líneas 710-711), así que no puede producir un registro falso. La lista de pares solo añade un tope.
- V2 muestra que falsificar el par en `output.json` no abre nada: el resto de condiciones cae.
- La consecuencia es que, para PB-T3a, solo vale `--task PBT3A`. Es lo que hace `apply_nc1_cascade_pbt3a.sh`.

**Duda 2 (`DelegationRunId` y `WorkRunId` no añadidos): de acuerdo, y retiro esa parte de mi M-13.**
- La identidad del intento ya la fijan:
  - `SourceVerification` = RunId real (a1);
  - las entradas idénticas, porque la delegación y el registro de trabajo llevan esos RunId (a3);
  - el prompt, que nombra `DelegationRunId` y `WorkRunId` y es el real salvo RunId y ruta (a4);
  - TaskId y Attempt (g).
- Si la salida de nc1 escribiera otros, sería una mala copia del Controller de un dato que se le dio. No toca (a)-(f).
- W1 da PASS, y es aceptable.

### Hallazgos nuevos (menores)

**M-17. El ancla del prompt real admite cualquiera de varios candidatos y cualquier clave que contenga `prompt` y `sha`.** Líneas 118-131 y 198-202.
- **Lo que ya cubre:** detecta que se cambie solo `prompt.md` (N03).
- **Lo que no cubre:** que se añada un segundo SHA al relay real, en texto (R4) o en una clave ajena (R5). En los dos casos hay que manipular la copia custodiada de la verificación real, que está fuera del modelo de amenaza habitual.
- **Recomendación:**
  - exigir exactamente un candidato: los tres casos reales tienen uno y el script de relevo de PB-T3a escribe uno;
  - limitar la lectura por clave a un nombre exacto (`PromptSha256`).

**M-18. La autoprueba no ejercita las guardas de `main()` ni la estrictez del texto libre.** Sobreviven:
- M35: VALIDATION sin la lista de pares (línea 692);
- M36 y M45: BaseSha o CurrentSha dados por buenos (línea 698);
- M46: `write_out` sin crear la carpeta (línea 639);
- M42: el texto libre relajado a «cualquier 64 hex» (línea 115).

Hoy estas guardas solo las comprueban mis sondas (V1, N02, X08, G1, G2 y X09).

**Recomendación:**
- un caso sintético con el SHA del prompt real bajo una clave sin `prompt`, y sin el texto «prompt SHA-256». Se espera ERROR y detectaría M42;
- declarar en `LIMITS` que las guardas de `main()` (repositorio, pares de VALIDATION, carpeta de salida) las verifica la revisión, no la autoprueba;
- o bien una autoprueba de `main()` sobre una copia temporal de los dorados con el checkout como repositorio.

**Observación, no hallazgo:** `TASKS` fija PBT3A, PBT3B, PBT3C, PBT4 y PBT5. Si PB-T4 o PB-T5 se dividieran (`F4B-PBT4A-…`), el prefijo `F4B-PBT4-` no casaría y daría FAIL, que es conservador. Ampliarlo obligaría a editar el script y a revisarlo de nuevo. Sigue pendiente confirmar lo de `reissue-record.json` de PB-T3a (nota 8 de `notes.md`).

### Ejecuciones y salvedad

| Ejecución | Resultado |
|---|---|
| `--selftest` | `SELFTEST_PASS=True DETERMINISTIC=True SYNTHETIC=97 GOLDEN=3`, `ATOMS_NOT_COVERED []` |
| PB-T2a, T2b y T2c con `--task VALIDATION` y el checkout | PASS, FAIL y PASS |
| `golden_check_v3.py` | `GOLDEN_FIDELITY=True`; SHA igual al fijado |
| `probes_v3.py` | 33 sondas anteriores y 18 nuevas, en la tabla de arriba |
| `mutate_v3.py` | 48 mutantes: 41 se detectan, 6 sobreviven (M29, M35, M36, M42, M45 y M46) y 1 no aplica (M33) |
| Integridad de `data/` | Ningún archivo es más nuevo que el inicio de esta recomprobación. |

**Salvedad de método:** para leer un campo de `run-t2b.json` usé una vez `python -I -c`, contra la regla de usar solo scripts escritos con Write. Fue una lectura de un JSON sin escritura y no afecta a ningún resultado; el mismo dato aparece en `run-t2b.json`.

`review\v3\probes\_emptyrepo` y `_altrepo` son repositorios `git init` de prueba; `_altrepo` solo tiene un `alternates` que apunta al del proyecto.
