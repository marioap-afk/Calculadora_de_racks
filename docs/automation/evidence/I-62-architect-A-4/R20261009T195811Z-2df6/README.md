# I-62 — Kit de la segunda re-revisión formal de la enmienda A-4 por `claude-cli` (R20261009T195811Z-2df6)

```text
LogicalReviewRequestId: L20261009T195811Z-2df6   InvocationId: I20261009T195811Z-2df6   AttemptSeq: 1   RunId: R20261009T195811Z-2df6
Autorización:   disposición del Coordinator §63, punto 6 (cuerpo pegado, 3 345 bytes, SHA-256 00418a54…, el que declara decisiones §63): integrar
                la corrección de A62-A4-02, la autorización presupuestaria propuesta por U-14 y las disposiciones ordinarias de FX-02; actualizar
                materialidad, delta, guardas, pruebas y paquete; publicar la versión exacta con trazabilidad; lanzar una re-revisión formal por
                claude-cli si las guardas están conformes; no declarar AGREED anticipadamente. Punto 1: revisión 2 ACCREDITED; A62-A4-01 CLOSED;
                A62-A4-02 ACCEPTED REQUIRED con la opción B; O9 y O10 no bloqueantes. Decisiones §57, punto 3, y CLAUDE-CLI-I62 = A. UNA re-revisión,
                sin reintento automático
Consumo:        cubierto por CLAUDE-CLI-I62 = A (uso read-only de claude-cli para el rol ARCHITECT de I-62), la misma base que las revisiones 1 y 2
Objeto:         recibo 24284a4a86f3f4b9552f8de3905f08fa0c81e84a (guardas CORRECTION3 PASS sobre c2dbc225)
                corrección 3 c2dbc22566cff9319f12e521d841b5bfcbb40e0c (A-4 5e2ba68e, paquete 57c060e1, guardas ampliadas), base del delta b25dde6b
                historia: 27ffa26b (4710084b) → 7d863219 (7f065a3a; revisión 1) → 0d954376 (a3332495; revisión 2) → 5e2ba68e (c2dbc225)
                docs/initiatives/I-62-A-4.md                                 blob 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f   (el «del recibo» del paquete)
                docs/initiatives/I-62-architect-package-A-4.md               blob 57c060e15f14ade6aefeda246adf26b2adaf2594   (§0: partes A, B y C)
                docs/automation/evidence/I-62-A4/a4-guards.py                blob 27748730b64dc9df3c30db71cb521ec0806e5d29
                docs/automation/evidence/I-62-A4/a4-selftest.json            blob 94dbe94b5a87b5cf0474524adc312d39b9f44666   (A4TextBlob 5e2ba68e)
                docs/automation/evidence/I-62-A4/a4-guards-result-c3.json    blob 4d939646208f6769d83c7ec565e0fc6f66a9214e   (PASS; Mode CORRECTION3; Head c2dbc225)
Anterior:       objeto 0d9543761e3e3a45a94338776f7c1ba763224ab3 (revisión 2, R20261009T150948Z-fdc2: CHANGES REQUIRED, A62-A4-02 y A62-A4-O9..O10;
                A62-A4-01 CLOSED; auditor ACCREDITED); salida output.json blob 54986dab…; registro I-62-architect-review-A-4-r2.md blob d19d849d…
Freeze:         b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; V14 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                (los dos commits son anteriores al rebase y no están en el clon; V14 se identifica por su blob en el commit)
Clon:           D:\r62-arch-a4r2 (git clone --no-local -c core.autocrlf=false --single-branch desde el worktree; main = el recibo; sin remoto; sin
                enlaces; nada posterior al recibo; kit/clone-verification.json, AllChecks)
Run:            D:\r62-arch-a4r2-run, con siete archivos y nada más antes del lanzamiento:
                order.txt (cuerpo §63, 3 345 bytes, 00418a54…), order-s62.txt (cuerpo §62, 3 035 bytes, 560e043d…), order-s61.txt (cuerpo §61,
                3 196 bytes, c8245687…), delta.diff (git diff b25dde6b c2dbc225 -- docs/initiatives/I-62-A-4.md; 48 403 bytes, 118d1660…),
                A-4.0d954376.md (git show b25dde6b:…; 48 685 bytes, 9ae90ad7…), A-4.7d863219.md (git show a5b50c68:…; 40 944 bytes, 39dd78ab…) y
                prompt.md (= kit/prompt.md, 33 455 bytes, e7a807c0…)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC» (re-medidos sin ejecutar el binario); claude-opus-5-5 con effort xhigh; herramientas Read,
                Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; session id 2efd6f27-8d7c-4826-b3df-a189443b3950 (uuid4);
                cwd = el clon; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s entre `auth status` y el lanzamiento
Compuerta D-01: kit/transport-gate.json en MEASURED (vía a) sobre el registro de caracterización existente R20261008T183600Z-char
                (claude-cli-characterization.json, SHA-256 20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c, fijado; la copia del clon
                D:\r62-arch-a4r2, blob 541956eb): Probes.C3 PASS sobre la plantilla exacta, STALE = false, transport_gate.evaluate sin problemas
Estado:         kit preparado ANTES del lanzamiento (commit de custodia: pendiente); NO lanzado. Lo lanza la sesión principal con kit/launch.py, una vez
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1-a4r2 sobre la transcripción, la salida stream-json y launch/)
```

**Recibo de publicación.** El paquete designa el objeto como «blob <el del commit de publicación> (borrador de esta pasada: 5e2ba68e…)». Este README
es el recibo: el commit es `24284a4a`, el objeto `5e2ba68e…` y el paquete `57c060e1…`, calculados con git en ese commit (`make_closure.py` los
comprueba). Todos los insumos del paquete §2 conservan el blob que nombra salvo tres:
- decisiones: el paquete la nombra dos veces, `625a7075…` (en `524b293e`) y `01905d16…` («en `d5b454fd`»); el commit tiene `01905d16…`, idéntico al
  segundo, y `625a7075…` es prefijo exacto;
- evidencia: `d132626e…` → `1b1431dc…`: añade §96-§112 y, en §96, la línea 3029 (con un retorno de carro suelto) pasó a ser tres líneas (`e6fe2e08`);
  §84 y §93-§95, las que cita el paquete, son idénticas (`AppendOnly.NonAppendChanges`);
- README del kit de FX-02: `fd7ef02d…` → `2439837a…`: cambia la línea 8 (`d358d31e`, resello de N8) y añade la línea 356 (`57877a7d`); las líneas
  que cita el paquete (44, 140, 144, 167, 180 y 243-249) son idénticas y están en las mismas posiciones (comprobación por líneas citadas, nueva en
  `make_closure.py`; A-4 §6 lo declara).
El paquete y A-4 no se editan.

**Versiones anteriores y delta.** El paquete §2 pide A-4 «en `d5b454fd`» (`0d954376`, para el delta) y «en `a5b50c68`» (`7d863219`, la revisada en la
revisión 1). El revisor no ejecuta git, así que el run trae las dos versiones literales y `delta.diff`. `make_closure.py` y `verify_clone.py`
comprueban que son la salida literal de git en el clon, sus blobs, que `0d954376` está en `b25dde6b`, `d5b454fd`, `155b7771` y `a3332495`, y que las
líneas de cambio de `delta.diff` son las del diff blob a blob `0d954376..5e2ba68e` (misma línea `index`).

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon; SHA-256 `12d6a878…`): 32 archivos canónicos (2 198 483 bytes) y
  siete archivos del run (181 063 bytes):
  - la tabla de insumos del paquete §2 (27 filas: 26 archivos distintos del clon, porque decisiones aparece en dos filas, entre ellos las salidas de
    las revisiones 1 y 2, el registro de la revisión 2, la conciliación de U-14 y el validador de producción de F4, y dos filas que son archivos del
    run: A-4 «en `d5b454fd`» y «en `a5b50c68`») y el
    propio paquete, con `PackageDeclaredBlob` comprobado contra el commit y `LineRanges` calculados desde los encabezados, las filas y las líneas que
    cita el paquete (con su primera línea comprobada); se añaden las líneas que cita el paquete §0 (V14 L2184-L2185, L2437 y L2523; decisiones
    L982-L985);
  - las guardas (`a4-guards.py`, `a4-selftest.json`, `a4-guards-result-c3.json`), que el paquete §2 no lista y el Paso 0 necesita; la solicitud de
    desbloqueo de FX-02 y su clasificación, que el paquete §2 deja en el cierre como contexto; decisiones §51 y §55-§63; evidencia §84, §93-§95 y
    §101-§112;
  - `PriorFindingIds` = A62-A4-01, A62-A4-02, A62-A4-O9 y A62-A4-O10 (leídos de las dos salidas, que se comprueban como revisiones de `7d863219` y de
    `0d954376` con CHANGES REQUIRED, y con A62-A4-01 CLOSED en la revisión 2);
  - run: los siete archivos, clase RUN, con `RunFileHashes`; todos menos `prompt.md` pueden ser premisa;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate` (idéntica a la de los kits de A-2, A-3 y A-4), `TimeoutSeconds`, `AuthSettleSeconds` y
    `Gate`.
- **Herramientas y lectura.** Solo Read, Grep y Glob, y StructuredOutput para el resultado. Los rangos orientan, no limitan. Es una violación: cualquier
  Read o Grep fuera del cierre y del run; un Grep sin `path`, sobre un directorio o con `glob` o `type`; un Glob cuyo alcance o cuyas rutas devueltas
  contengan un archivo fuera del cierre; cualquier otra herramienta; toda denegación de permiso. El objeto pesa ahora 62 415 bytes: también se lee por
  tramos de 300 líneas como máximo.
- **Prompt** (`kit/prompt.md` = `D:\r62-arch-a4r2-run\prompt.md`, byte a byte; 33 455 bytes, 332 líneas, SHA-256
  `e7a807c02188bbbf27f5d3845bf9f239aa74417b2e734041e30c03ed5785ff65`; sin salto de línea final). Neutral: sin veredicto esperado ni contexto privado.
  Pide:
  - el Paso 0: `order.txt` entera (punto 6, líneas 33-43), `delta.diff` 1-4 (línea `index` `0d954376..5e2ba68e` y rutas), el paquete 1-71 (cabecera
    y §0), `a4-guards-result-c3.json` 1-7 y 1723-1739 (nota de G5 «frente a 0d954376» y G5b) y `a4-selftest.json` 48-65 (`A4TextBlob`);
  - `PriorFindingDispositions`: A62-A4-02 `CLOSED` o `STILL_OPEN`; A62-A4-01 `CLOSED` (confirmar que sigue cerrado) o `STILL_OPEN`; los dos con al
    menos una premisa; A62-A4-O9 y A62-A4-O10 `APPLIED`, `NOT_APPLIED` o `N/A`;
  - disposiciones explícitas de los ocho temas de la orden en `Focus`: `OPTION_B_A4-4_ARCHITECT`, `LITERAL_IS18_B5_RAE143_F4` (con la frase explícita
    sobre I-S18 y la ausencia de UNKNOWN en una materialización), `A4-6_BUDGET_DELTA` (+1 sesión de Principal solo para A2, R contada, F6-OBS-01,
    ningún otro tope, P-07, `A4-PRINCIPAL-A2-CONSUMO`, separabilidad, M-03/M-04 y por qué ni M-02 ni M-05), `H1_H2_H3_STILL_SOUND`,
    `A4-1_A4-2_UNCHANGED`, `MATERIALITY_OVERALL`, `SEPARABILITY_A4-5_A4-6` y `FX-02_DISPOSITIONS_CONTEXT_ONLY`; cada tema contesta además las
    preguntas del paquete §4 que le corresponden;
  - `A45Verdict` y `A46Verdict` separados; Q-A4-01..Q-A4-22; `DeltaConfirmation` (A4-1, A4-2, A4-3 y A4-5 idénticos a `0d954376`; A4-4, introducción
    y reglas 2-4); `OwnerConsumptionLines` para las tres líneas del Owner (solo consumo y solo con efecto tras el acuerdo);
    `NoChangeConfirmation` (12, con la fila «Codex, total» y cualquier otro tope de D.3 distinto de los dos de A4-6); `GuardsVerification` (G1,
    G2C3, G2D3, G3, G4, G5, G5b); materialidad; modo e identidad;
  - **números de línea exactos** en cada premisa;
  - los identificadores de cuenta inyectados, solo por su tipo.
- **Esquema** (`kit/result.schema.json`; compacto, 14 310 caracteres ASCII; SHA-256 `ae1a171a…`): el de la re-revisión 1 con
  `PriorFindingDispositions` (4 ids), `Focus` (8 temas nuevos), Q-A4-01..22, `A46Verdict` (misma forma que `A45Verdict`), `DeltaConfirmation` (5
  claves frente a `0d954376`), `NoChangeConfirmation` (12 claves: `D3CodexTotalRow` y `D3OtherCapsBeyondA46` en lugar de `D3CapsRow`),
  `OwnerConsumptionLines.EffectiveOnlyAfterAgreement` y `GuardsVerification` con G2C3/G2D3.
- **Compuerta de transporte D-01** (`kit/transport_gate.py`, byte a byte igual al del kit de la re-revisión 1, predicados sin cambio): entregada en
  MEASURED (vía a) sobre el registro existente `20d0de2c…` (copia del clon). Sin ejecutar el binario: SHA-256 `8693c4a0…` y firma válida
  (Get-FileHash y Get-AuthenticodeSignature).
- **Auditor v5.1-a4r2** (`kit/post-review.py`; SHA-256 `ca8cc11e…`): las reglas de v5.1-a4r, adaptadas al objeto y al cierre de la corrección 3.
  Cada fallo es un motivo y ACCREDITED exige cero motivos. Coherencia:
  - hallazgos anteriores con dos REQUIRED (A62-A4-01 y A62-A4-02: solo `CLOSED` o `STILL_OPEN`, con premisa; un `STILL_OPEN` enlazado a un REQUIRED)
    y dos OPTIONAL (A62-A4-O9 y A62-A4-O10: `APPLIED`, `NOT_APPLIED` o `N/A`; un `NOT_APPLIED` enlazado a un hallazgo); ids anteriores solo para
    reformular; enlaces a ids existentes; cobertura de los cuatro;
  - cobertura de Q-A4-01..22 y de los ocho temas de `Focus`; `DeltaConfirmation` en CONFIRMED o NOT_CONFIRMED con un veredicto;
  - `A45Verdict` y `A46Verdict` con las mismas reglas (NOT_ASSESSED solo con NOT_ACCREDITED; CHANGES REQUIRED con un REQUIRED; BLOCKED con
    `OwnerDecisionRequired`; ids existentes);
  - AGREED: cero REQUIRED (también en `Focus` y en las preguntas), A62-A4-01 y A62-A4-02 `CLOSED`, `A45Verdict` y `A46Verdict` en AGREED o EXCLUDE,
    sin decisión del Owner, `OwnerConsumptionLines` = ONLY_CONSUMPTION con `EffectiveOnlyAfterAgreement` = YES, `NoChangeConfirmation` todo
    NO_CHANGE, `DeltaConfirmation` todo CONFIRMED e `IfAgreed` todo en true;
  - CHANGES REQUIRED, BLOCKED y NOT_ACCREDITED como antes; identidad revisada (commit, ruta, blob) e ids de la invocación.
  Uso: `python -B kit/post-review.py <transcripción> D:\r62-arch-a4r2-run\launch\stdout.jsonl D:\r62-arch-a4r2 D:\r62-arch-a4r2-run kit
  kit/result.schema.json <audit.json> [<output.json>]`.
- **Lanzador** (`kit/launch.py`; SHA-256 `bf266956…`; no lo ejecuta quien prepara el kit, salvo `--dry-run`). Se niega a lanzar (código 2, sin
  escribir nada) si falla: 1. integridad del kit frente a `kit-manifest.json`; 2. transporte del cierre; 3. binario (ruta, SHA-256, Authenticode);
  4. `--version` = 2.1.293; 5. `auth status` con `loggedIn`; 6. clon (HEAD, limpio, blobs, sin enlaces); 7. run con exactamente los siete archivos y
  sus hashes; 8. sin transcripción previa con el session id ni directorio de proyecto `D--r62-arch-a4r2*`; 9. línea de órdenes de menos de 32 000
  caracteres (mide 16 968); 10. compuerta en MEASURED o ACCEPTED.
  Después escribe `preflight.json` y la copia del registro en `D:\r62-arch-a4r2-run\launch\`, **espera 120 s desde el final de `auth status`**,
  vuelve a medir el binario y lanza la ruta resuelta con la lista exacta, stdin = `prompt.md`, cwd = el clon y un tope de 3 600 s con terminación.
  Escribe `stdout.jsonl`, `stderr.txt` y `run.json` (con `AuthSettle` y `TransportFailure`: TRANSPORT_AUTH_REFRESH_CONFLICT si el `result` contiene
  «Failed to refresh OAuth token» con 0 tokens). No reintenta en ningún caso. `--dry-run` no ejecuta el binario (4 y 5 quedan sin ejecutar).
  La lista que lanza es, literalmente: `<%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe resuelto> -p --model claude-opus-5-5 --effort
  xhigh --output-format stream-json --verbose --safe-mode --strict-mcp-config --no-chrome --tools Read,Grep,Glob --permission-mode dontAsk
  --permission-prompts none --add-dir D:\r62-arch-a4r2-run --session-id 2efd6f27-8d7c-4826-b3df-a189443b3950 --json-schema <kit/result.schema.json
  compacto, ASCII>`, con stdin = `D:\r62-arch-a4r2-run\prompt.md` y cwd = `D:\r62-arch-a4r2`.
- **Selftest previo** (`kit/selftest-result.json`): **127/127 PASS** sobre el clon y el run reales, que quedan limpios e idénticos: 86 casos de
  auditoría (los 81 de la re-revisión 1, adaptados a los cuatro hallazgos anteriores, a los ocho temas y a los siete archivos del run, y 5
  nuevos, 80-84: líneas del Owner con efecto antes del acuerdo, `A46Verdict` NOT_ASSESSED, CHANGES REQUIRED sin REQUIRED y con un id
  inexistente, BLOCKED sin `OwnerDecisionRequired`, y un positivo: AGREED con A4-6 excluida) y 41 de la compuerta. El caso 25 acepta
  premisas de los seis archivos de premisa del run; el 15 rechaza una premisa con la línea desplazada en uno. Los directorios temporales de
  cada caso se nombran solo por su número: con el nombre completo, la ruta del caso 66 pasaba de 260 caracteres (MAX_PATH) y el primer
  intento se detuvo en ese caso, sin efecto en el clon ni en el run.
- **Mutación** (`kit/mutation-post-review.py` → `kit/mutation-result.json`): cada sitio de regla de `post-review.py` (104) y de
  `transport_gate.py` (36) se desactiva por turno en una copia y se ejecutan los 127 casos: **140/140 mutantes muertos**, con la base sin
  mutar en 127/127. Corrida acotada (tope de 6 000 s; duró unos 40 minutos, 20:26:28Z-21:06:44Z) sobre los archivos finales del kit. Como
  `post-review.py` cambió, no se reutiliza ningún resultado anterior (tampoco el de `transport_gate.py`, idéntico al probado, que se vuelve a
  probar con los casos nuevos).
- **Fidelidad previa** (`kit/preflight-read-fidelity.json`): 4 lecturas en el clon, sin filtro, las cuatro fieles: el paquete entero y A-4 entera
  (1-180, 181-350 y 351-516). Salen de la transcripción del subagente que preparó el kit; su SHA-256 es el de la instantánea medida.
- **Clon y run** (`kit/clone-verification.json`, generado por `kit/verify_clone.py`), todo comprobado: HEAD = el recibo, solo `main`, sin remotos,
  limpio con ignorados, ninguna etiqueta ni commit fuera de la historia de HEAD (1 856); 33 blobs designados; la historia de la candidata y de los
  insumos anteriores (`27ffa26b` en `4710084b`; `7d863219` en `7f065a3a` y `a5b50c68`; `0d954376` en `a3332495`, `d5b454fd`, `155b7771` y
  `b25dde6b`; `5e2ba68e` y el paquete `57c060e1` en `c2dbc225`; decisiones `01905d16` en `d5b454fd`; las salidas `84773744` y `54986dab` en sus
  custodias); los commits de las tres correcciones, de las dos custodias y recibos, `524b293e`, `f504f6a9`, `c8d69fcb` y `d2d74c14` presentes,
  `4c617e82` y `b64a3b64` ausentes; 0 enlaces y 0 entradas 120000; `core.autocrlf` = false; run con los siete archivos, idénticos al kit y al cierre,
  los tres textos de las órdenes con el SHA-256 y los bytes que declaran decisiones §63, §62 y §61, y los tres archivos de git iguales a su salida;
  el registro de caracterización del clon con el SHA-256 de la compuerta; sin directorio de proyecto `D--r62-arch-a4r2*` ni transcripción previa.
  Preflight de las guardas: `a4-guards.py self-test` PASS (112/112 mutantes), con una salida idéntica entera a `a4-selftest.json` custodiado (worktree
  en el recibo, solo lectura).
- **Preflight en seco** (`launch-dry-run.json`, junto a este README): `launch.py --dry-run` sobre el kit entregado: ninguna comprobación bloqueante; 1-3 y 6-10 en Ok;
  la 4 (`--version`) y la 5 (`auth status`) no se ejecutan en seco, porque quien prepara el kit no ejecuta el binario. La línea de órdenes mide
  16 968 caracteres. Código 2, como siempre en seco.
- **Manifiesto:** `kit/kit-manifest.json` (24 archivos del kit y los siete del run; SHA-256 `d684e16802144cf1faa99f66ab5b0dba975694e6cc292daefc58a37af7425607`).

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar.

## Desviaciones frente al kit de la re-revisión 1 (R20261009T150948Z-fdc2) y a la orden, con su motivo

- **Séptimo archivo del run, `A-4.7d863219.md`.** La orden enumera seis archivos del run; el paquete §2 lista también A-4 «en `a5b50c68`»
  (`7d863219`, la versión de la revisión 1). Para que el cierre cubra entera la tabla del paquete, el kit la trae como archivo del run, premisa
  permitida, salida literal de git comprobada.
- **`delta.diff` por ruta, no blob a blob**, como en los kits de las re-revisiones de A-2 y de A-4: `git diff b25dde6b c2dbc225 -- <objeto>`, con la
  misma línea `index 0d954376..5e2ba68e` y las mismas líneas de cambio que el diff blob a blob (comprobado mecánicamente).
- **A62-A4-01 en `PriorFindingDispositions`.** La orden pide confirmar que sigue cerrado; el kit lo trata como un REQUIRED anterior más (`CLOSED` o
  `STILL_OPEN`, con premisa), y AGREED exige los dos REQUIRED anteriores en `CLOSED`.
- **Insumos fuera de la tabla del paquete §2:** las guardas (Paso 0), decisiones §51 y §61-§63 (orden) y evidencia §101-§112 (recibos). La solicitud
  de desbloqueo y su clasificación siguen en el cierre porque el paquete §2 lo dice. Quedan fuera, por no nombrarlos ni la orden ni el paquete, el
  registro de la revisión 1, `a4-guards-result.json`, `-c2.json` y `-preview.json` (`NotTriggered`).
- **README del kit de FX-02 con un cambio no añadido (línea 8).** Se comprueban las líneas citadas por el paquete, idénticas y en su sitio; el
  cambio queda en `AppendOnly.NonAppendChanges`.
- **Esquema y coherencia** ampliados como pide la orden: `A46Verdict` con las reglas de `A45Verdict`; `OwnerConsumptionLines.EffectiveOnlyAfterAgreement`
  (las tres líneas solo tienen efecto tras el acuerdo); `NoChangeConfirmation` separa la fila «Codex, total» (`D3CodexTotalRow`) de cualquier otro tope
  distinto de los dos de A4-6 (`D3OtherCapsBeyondA46`), porque A4-6 sí sube la fila Principal A y el total de la ronda A; `DeltaConfirmation` añade
  A4-1 y pasa a «introducción y reglas 2-4» de A4-4.
- **`transport_gate.py` sin tocar** (byte a byte el de la re-revisión 1; su cabecera nombra todavía esa re-revisión): se conserva igual para que sus
  predicados sean los ya probados.
- **Unión del caso 03 del selftest** hacia el clon de la re-revisión 1 (`D:\r62-arch-a4r`, solo como destino de lectura).
- **Tope de 3 600 s**, igual que en los kits de A-4: las revisiones 1 y 2 duraron unos 21 minutos cada una; este cierre pesa 2 198 483 bytes más
  181 063 del run. La espera de 120 s queda fuera del tope.
- **Sin cambios** frente a la re-revisión 1: la compuerta y sus predicados, la plantilla de argumentos, el binario, la mitigación de 120 s y la
  clasificación TRANSPORT_AUTH_REFRESH_CONFLICT, el analizador C3, el lector de fidelidad y las reglas de v5.1-a4r no afectadas.

## Supuestos no medidos (declarados antes del lanzamiento)

- La duración frente al tope de 3 600 s (estimada por las revisiones 1 y 2).
- La compactación automática del contexto (no observada en C1, C2 ni en las corridas anteriores); el prompt avisa.
- Que la espera de 120 s evite el conflicto de renovación del token del intento 1 de A-3.

## Antes del lanzamiento (procedimiento de la sesión principal)

1. Autoridad: decisiones §63, punto 6 (re-revisión si las guardas están conformes: CORRECTION3 PASS sobre `c2dbc225`); consumo cubierto por
   CLAUDE-CLI-I62 = A (paquete §5). La decisión es de la sesión principal; el kit no la presume.
2. Opcional: `python -B kit/transport_gate.py check` (código 0) y `python -B kit/launch.py --dry-run` (todo en Ok salvo la 4 y la 5).
3. Custodiar el kit (commit de custodia) y lanzar una vez: `python -B kit/launch.py`. Si cambia un archivo del kit, `python -B kit/make_manifest.py` y
   volver a custodiar.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json` y los siete archivos del run frente a `RunFiles`.
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a4r2\2efd6f27-8d7c-4826-b3df-a189443b3950.jsonl" D:\r62-arch-a4r2-run\launch\stdout.jsonl D:\r62-arch-a4r2 D:\r62-arch-a4r2-run kit kit/result.schema.json audit.json output.json`.
- `output.json`: el `structured_output` del `result`, en UTF-8 con sangría 1. `audit.json`: la salida literal del auditor v5.1-a4r2.
- `runtime-evidence.json`: de `launch/`, `run.json`, `preflight.json`, la copia de la compuerta y los SHA-256 de `stdout.jsonl` y `stderr.txt`; la
  transcripción (ruta y SHA-256, no versionada; sanear antes de versionar cualquier copia); el clon después; la integridad del kit y del run.
- A-4 no se modifica después de lanzar la re-revisión y no se declara AGREED sin la revisión acreditada y la decisión posterior del Coordinator.
