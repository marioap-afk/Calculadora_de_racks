# I-62 — Kit de la re-revisión formal de la enmienda A-4 por `claude-cli` (R20261009T150948Z-fdc2)

```text
LogicalReviewRequestId: L20261009T150948Z-fdc2   InvocationId: I20261009T150948Z-fdc2   AttemptSeq: 1   RunId: R20261009T150948Z-fdc2
Autorización:   disposición del Coordinator §62, punto 2 (cuerpo pegado, 3 035 bytes, SHA-256 560e043d…, el que declara decisiones §62): publicar
                la corrección conservando la trazabilidad con el blob anterior, ejecutar las guardas sobre el commit exacto y, con PASS, custodiar
                el paquete nuevo y lanzar automáticamente una re-revisión independiente por claude-cli, que examina en especial H-1, H-2, H-3,
                F6-OBS-03 y la ruta cmd.exe, el Architect alternativo por claude-cli, la materialidad M-02/M-05, U-09 (e) y la separabilidad de
                A4-5; sin AGREED hasta una revisión acreditada y la decisión posterior del Coordinator. Punto 1: revisión 1 NOT_ACCREDITED;
                A62-A4-01 = ACCEPTED REQUIRED; O1..O8 = ACCEPTED NON-BLOCKING. Decisiones §57, punto 3 (claude-cli 2.1.293 aceptado como
                transporte de ARCHITECT y REVIEWER) y la línea del Owner CLAUDE-CLI-I62 = A. UNA re-revisión, sin reintento automático
Consumo:        cubierto por CLAUDE-CLI-I62 = A (uso read-only de claude-cli para el rol ARCHITECT de I-62), la misma base que las revisiones
                de A-2, A-3 y A-4 (revisión 1); decisiones §62, punto 2: lanzar automáticamente con las guardas en PASS (CORRECTION2 sobre a3332495)
Objeto:         recibo cca8c60e4acfea498844c5d5f8ea14b807fbd4f3 (decisiones §62: corrección 2 con guardas PASS)
                corrección 2 a33324957f8778dd286a5a871881d403fd5a2d14 (A-4 0d954376, paquete 59052b84, guardas ampliadas)
                historia de la candidata: 27ffa26b (4710084b) → 7d863219 (7f065a3a; revisada en la revisión 1, custodiada en a5b50c68) → 0d954376
                docs/initiatives/I-62-A-4.md                                 blob 0d9543761e3e3a45a94338776f7c1ba763224ab3   (el «del recibo» del paquete)
                docs/initiatives/I-62-architect-package-A-4.md               blob 59052b847ccc13e8be539189ae1b77dc7f9d3623   (§0: hallazgo y delta)
                docs/automation/evidence/I-62-A4/a4-guards.py                blob 6595d8730456d7a06fc3edbd833dfa1eef11a1bc
                docs/automation/evidence/I-62-A4/a4-selftest.json            blob ddaf18f6b6df3692836761685630c62417a2d171   (A4TextBlob 0d954376)
                docs/automation/evidence/I-62-A4/a4-guards-result-c2.json    blob 2fce7b58f9da20a17fa19f4955a1bc984e8f0022   (PASS; Mode CORRECTION2; Head a3332495)
Anterior:       objeto 7d863219a271b5ec427ef8669435826e19be8f11 (revisión 1, R20261009T122416Z-bce3: CHANGES REQUIRED, A62-A4-01 y A62-A4-O1..O8;
                auditor NOT_ACCREDITED); salida output.json blob 84773744…; registro docs/initiatives/I-62-architect-review-A-4.md blob b464f2c5…
Freeze:         b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; V14 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                (los dos commits son anteriores al rebase y no están en el clon; V14 se identifica por su blob en el commit)
Clon:           D:\r62-arch-a4r (git clone --no-local -c core.autocrlf=false --single-branch desde el worktree; main = el recibo; sin remoto; sin
                enlaces; los commits posteriores al recibo, podados; kit/clone-verification.json, AllChecks)
Run:            D:\r62-arch-a4r-run, con seis archivos y nada más antes del lanzamiento:
                order.txt (cuerpo §62, 3 035 bytes, 560e043d…), order-s61.txt (cuerpo §61, 3 196 bytes, c8245687…), order-s60.txt (cuerpo §60,
                2 047 bytes, 3a7257e1…), delta.diff (git diff a5b50c68 a3332495 -- docs/initiatives/I-62-A-4.md; 25 767 bytes, 6105684f…),
                A-4.7d863219.md (git show a5b50c68:docs/initiatives/I-62-A-4.md; 40 944 bytes, 39dd78ab…) y prompt.md (= kit/prompt.md,
                30 382 bytes, e1eea6b2…)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC» (re-medidos sin ejecutar el binario); claude-opus-5-5 con effort xhigh; herramientas Read,
                Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; session id 9eb98aef-1bc0-44b9-9dc8-7e963f22bdf2 (uuid4);
                cwd = el clon; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s entre `auth status` y el lanzamiento
Compuerta D-01: kit/transport-gate.json en MEASURED (vía a) sobre el registro de caracterización existente R20261008T183600Z-char
                (claude-cli-characterization.json, SHA-256 20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c, fijado; la copia del clon
                D:\r62-arch-a4r, blob 541956eb): Probes.C3 PASS sobre la plantilla exacta, STALE = false, transport_gate.evaluate sin problemas
Estado:         kit preparado ANTES del lanzamiento (commit de custodia: pendiente); NO lanzado. Lo lanza la sesión principal con kit/launch.py, una vez
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1-a4r sobre la transcripción, la salida stream-json y launch/)
```

**Recibo de publicación.** El paquete designa el objeto como «blob <el del commit de publicación> (borrador de esta pasada: 0d954376…)». Este README
es el recibo: el commit es `cca8c60e`, el objeto `0d954376…` y el paquete `59052b84…`, calculados con git en ese commit (`make_closure.py` los
comprueba). Todos los insumos del paquete §2 conservan el blob que nombra salvo tres, que crecieron después de `524b293e`:
- decisiones: `625a7075…` → `83269536…`, solo por añadido (§61 y §62); el texto nombrado es prefijo exacto;
- README del kit de FX-02: `fd7ef02d…` → `ceb38284…`, solo por añadido (una línea, la 356, en `57877a7d`); prefijo exacto; las líneas que cita el
  paquete no cambian (A-4 §6 lo declara);
- evidencia: `d132626e…` → `a50b1dcb…`: añade §96-§105 y, en §96, la línea 3029 (con un retorno de carro suelto) pasó a ser tres líneas (commit
  `e6fe2e08`); §84 y §93-§95, las que cita el paquete, son idénticas (`AppendOnly.NonAppendChanges` del cierre).
El paquete y A-4 no se editan.

**Objeto anterior y delta.** El paquete §2 pide la A-4 «en `a5b50c68`» (`7d863219`) para el delta. El revisor no ejecuta git, así que el run trae
el objeto anterior literal (`A-4.7d863219.md`) y el diff (`delta.diff`), como hizo el kit de la re-revisión de A-2. `make_closure.py` y
`verify_clone.py` comprueban que los dos son la salida literal de git en el clon, que el blob del objeto anterior es `7d863219`, y que las líneas de
cambio de `delta.diff` son las del diff blob a blob `7d863219..0d954376` (misma línea `index`).

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon; SHA-256 `cdb35c12…`): 30 archivos canónicos (2 022 717 bytes):
  - la tabla de insumos del paquete §2 (22 filas: 23 archivos del clon, entre ellos la salida de la revisión 1, `output.json`, y el validador de
    producción de F4, L119-L124, y la fila «A-4 en `a5b50c68`», que es el archivo del run `A-4.7d863219.md`) y el propio paquete, con
    `PackageDeclaredBlob` comprobado contra el commit y `LineRanges` calculados desde los encabezados, las filas y las líneas que cita el paquete
    (con su primera línea comprobada);
  - por orden del Coordinator: las guardas (`a4-guards.py`, `a4-selftest.json`, `a4-guards-result-c2.json`), el registro de la revisión 1
    (`I-62-architect-review-A-4.md`) y la solicitud de desbloqueo de FX-02 con su clasificación; decisiones §55-§62 y evidencia §84, §93-§95 y
    §101-§105;
  - `PriorFindingIds` = A62-A4-01 y A62-A4-O1..O8 (leídos de `output.json`, que se comprueba como revisión de `7d863219` con CHANGES REQUIRED);
  - run: los seis archivos, clase RUN, con `RunFileHashes`; todos menos `prompt.md` pueden ser premisa;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate` (idéntica a la de los kits de A-2, A-3 y A-4), `TimeoutSeconds`, `AuthSettleSeconds` y
    `Gate`.
- **Herramientas y lectura.** Solo Read, Grep y Glob, y StructuredOutput para el resultado. Los rangos orientan, no limitan. Es una violación: cualquier
  Read o Grep fuera del cierre y del run; un Grep sin `path`, sobre un directorio o con `glob` o `type`; un Glob cuyo alcance o cuyas rutas devueltas
  contengan un archivo fuera del cierre; cualquier otra herramienta; toda denegación de permiso.
- **Prompt** (`kit/prompt.md` = `D:\r62-arch-a4r-run\prompt.md`, byte a byte; 30 382 bytes, 309 líneas, SHA-256
  `e1eea6b2f8a5cc2c8cf7ac33273e2e154b6053b1ebf5c8e4d601aef7f485d7b0`; sin salto de línea final). Neutral: sin veredicto esperado ni contexto privado.
  Pide:
  - el Paso 0: `order.txt` entera (punto 2, líneas 15-30), `delta.diff` 1-4 (línea `index` `7d863219..0d954376` y rutas), el paquete 1-52 (cabecera
    y §0), `a4-guards-result-c2.json` 1-7 y 1421-1437 (nota de G5 «frente a 7d863219» y G5b) y `a4-selftest.json` 30-50 (`A4TextBlob`);
  - `PriorFindingDispositions`: A62-A4-01 `CLOSED` o `STILL_OPEN` con al menos una premisa; A62-A4-O1..O8 `APPLIED`, `NOT_APPLIED` o `N/A`; enlaces a
    los hallazgos de la re-revisión;
  - disposiciones explícitas (REQUIRED, OPTIONAL o NO_FINDING, ligadas a hallazgos) de los ocho temas de §62, punto 2, en `Focus`:
    `H1_A4-3_NO_FABRICATED_ACCEPTANCE`, `H2_A4-4_REAL_INDEPENDENCE` (con el discriminador y los consumidores de la regla 5), `H3_A4-5_NO_BUDGET_RESET`,
    `F6-OBS-03_A4-1_CMD_ROUTE` (con las inserciones de O1 y O2), `A4-2_ALT_ARCHITECT_CLAUDE_CLI`, `MATERIALITY_M02_M05`, `U-09e` y
    `A4-5_SEPARABILITY`; cada tema contesta además las preguntas del paquete §4 que le corresponden;
  - un veredicto separado de A4-5 (`A45Verdict`); Q-A4-01..Q-A4-17; `DeltaConfirmation` (A4-2, A4-3 y A4-5 idénticos a `7d863219`; A4-4,
    introducción y reglas 1-4, idénticas); la decisión del Owner; `OwnerConsumptionLines`; `NoChangeConfirmation` (incluida la fila de topes de D.3);
    `GuardsVerification` de solo lectura; materialidad; modo e identidad;
  - **números de línea exactos** en cada premisa, con el recordatorio de que la revisión 1 quedó NOT_ACCREDITED por una línea desplazada en uno;
  - los identificadores de cuenta inyectados, solo por su tipo.
- **Esquema** (`kit/result.schema.json`; compacto, 13 341 caracteres ASCII; SHA-256 `3c273ec9…`): el de A-4 (revisión 1) con `PriorFindingDispositions`
  (9 ids; `CLOSED | STILL_OPEN | APPLIED | NOT_APPLIED | N/A`; `LinkedFindingIds`, `PremiseRefs`), `Focus` con los ocho temas, Q-A4-01..17,
  `DeltaConfirmation` (4), `GuardsVerification` (G1, G2C2, G2D2, G3, G4, G5, G5b), `IfAgreed` (8, con `PriorRequiredClosed`) e `IdentityCheck` (seis
  booleanas del Paso 0: `DeltaIndexMatchesBlobs`, `DeltaPathIsObject`, `PackageNamesObjectBlob`, `GuardsResultA4BlobMatchesObject`,
  `GuardsResultComparesWithPreviousBlob`, `SelftestA4TextBlobMatchesObject`).
- **Compuerta de transporte D-01** (`kit/transport_gate.py`, predicados sin cambio): entregada en MEASURED (vía a) sobre el registro existente
  `20d0de2c…` (copia del clon). Sin ejecutar el binario: SHA-256 `8693c4a0…` y firma válida (Get-FileHash y Get-AuthenticodeSignature).
- **Auditor v5.1-a4r** (`kit/post-review.py`; SHA-256 `cf70aea4…`): las reglas de v5.1-a4, adaptadas al objeto corregido y al cierre de la
  re-revisión. Cada fallo es un motivo y ACCREDITED exige cero motivos. Coherencia de la re-revisión:
  - reglas de hallazgos anteriores restituidas del auditor de la re-revisión de A-2: cobertura de los nueve ids; A62-A4-01 solo `CLOSED` o
    `STILL_OPEN` y con premisa; A62-A4-On solo `APPLIED`, `NOT_APPLIED` o `N/A`; un id anterior solo reaparece en los hallazgos para reformular un
    `STILL_OPEN` (A62-A4-01) o un `NOT_APPLIED` (On); todo `STILL_OPEN` o `NOT_APPLIED` enlazado a un hallazgo de la re-revisión (A62-A4-01 a un
    REQUIRED); enlaces a ids existentes; las premisas de las disposiciones se auditan como las demás;
  - cobertura de Q-A4-01..17 y de los ocho temas de `Focus`; `DeltaConfirmation` en CONFIRMED o NOT_CONFIRMED con un veredicto;
  - `A45Verdict` como en v5.1-a4;
  - AGREED: además de lo de v5.1-a4, A62-A4-01 `CLOSED` y `DeltaConfirmation` todo CONFIRMED;
  - CHANGES REQUIRED, BLOCKED y NOT_ACCREDITED como antes; identidad revisada (commit, ruta, blob) e ids de la invocación.
  Uso: `python -B kit/post-review.py <transcripción> D:\r62-arch-a4r-run\launch\stdout.jsonl D:\r62-arch-a4r D:\r62-arch-a4r-run kit
  kit/result.schema.json <audit.json> [<output.json>]`.
- **Lanzador** (`kit/launch.py`; SHA-256 `cf2b29e5…`; no lo ejecuta quien prepara el kit, salvo `--dry-run`). Se niega a lanzar (código 2, sin
  escribir nada) si falla: 1. integridad del kit frente a `kit-manifest.json`; 2. transporte del cierre; 3. binario (ruta, SHA-256, Authenticode);
  4. `--version` = 2.1.293; 5. `auth status` con `loggedIn`; 6. clon (HEAD, limpio, blobs, sin enlaces); 7. run con exactamente los seis archivos y
  sus hashes; 8. sin transcripción previa con el session id ni directorio de proyecto `D--r62-arch-a4r*`; 9. línea de órdenes de menos de 32 000
  caracteres (mide 15 860); 10. compuerta en MEASURED o ACCEPTED.
  Después escribe `preflight.json` y la copia del registro en `D:\r62-arch-a4r-run\launch\`, **espera 120 s desde el final de `auth status`**, vuelve
  a medir el binario y lanza la ruta resuelta con la lista exacta, stdin = `prompt.md`, cwd = el clon y un tope de 3 600 s con terminación. Escribe
  `stdout.jsonl`, `stderr.txt` y `run.json` (con `AuthSettle` y `TransportFailure`: TRANSPORT_AUTH_REFRESH_CONFLICT si el `result` contiene
  «Failed to refresh OAuth token» con 0 tokens). No reintenta en ningún caso. `--dry-run` no ejecuta el binario (4 y 5 quedan sin ejecutar).
  La lista que lanza es, literalmente: `<%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe resuelto> -p --model claude-opus-5-5 --effort
  xhigh --output-format stream-json --verbose --safe-mode --strict-mcp-config --no-chrome --tools Read,Grep,Glob --permission-mode dontAsk
  --permission-prompts none --add-dir D:\r62-arch-a4r-run --session-id 9eb98aef-1bc0-44b9-9dc8-7e963f22bdf2 --json-schema <kit/result.schema.json
  compacto, ASCII>`, con stdin = `D:\r62-arch-a4r-run\prompt.md` y cwd = `D:\r62-arch-a4r`.
- **Selftest previo** (`kit/selftest-result.json`): **122/122 PASS** sobre el clon y el run reales, que quedan limpios e idénticos: 81 casos de
  auditoría (los 71 del kit de A-4, adaptados, y 10 nuevos, 70-79, para las disposiciones anteriores y `DeltaConfirmation`, uno de ellos positivo: una
  re-revisión coherente con A62-A4-01 `STILL_OPEN` y un On `NOT_APPLIED`) y 41 de la compuerta. El caso 25 acepta premisas de los cinco archivos de
  premisa del run (también `delta.diff` con su marca y `A-4.7d863219.md`); el 15 rechaza una premisa con la línea desplazada en uno.
- **Mutación** (`kit/mutation-post-review.py` → `kit/mutation-result.json`): cada sitio de regla de `post-review.py` (104) y de
  `transport_gate.py` (36) se desactiva por turno en una copia y se ejecutan los 122 casos: **140/140 mutantes muertos**, con la base sin
  mutar en 122/122. Corrida acotada (tope de 6 000 s; duró unos 75 minutos, 16:24:39Z-17:39:33Z) sobre los archivos finales del kit. Un primer
  intento con la salida en búfer y tope de 3 600 s se detuvo a mano en el mutante 42 porque no iba a caber en el tope; dejó la unión de
  prueba del caso 03 en el clon, que se retiró (solo el enlace; el destino `D:62-arch-a4` intacto y los dos clones limpios) antes de repetir.
- **Fidelidad previa** (`kit/preflight-read-fidelity.json`): 4 lecturas en el clon, sin filtro, las cuatro fieles: el paquete entero y A-4 entera
  (1-150, 151-300 y 301-423). Salen de la transcripción del subagente que preparó el kit; su SHA-256 es el de la instantánea medida.
- **Clon y run** (`kit/clone-verification.json`, generado por `kit/verify_clone.py`), todo comprobado: HEAD = el recibo, solo `main`, sin remotos,
  limpio con ignorados, ninguna etiqueta ni commit fuera de la historia de HEAD (1 848); 31 blobs designados; la historia de la candidata (`27ffa26b`
  en `4710084b`, `7d863219` en `7f065a3a` y en `a5b50c68`, `0d954376` en `a3332495`), el paquete anterior `085f30f6` y la salida `84773744` en
  `a5b50c68`; `a3332495`, `a5b50c68`, `02be0c34`, `7f065a3a`, `4710084b`, `524b293e`, `f504f6a9`, `c8d69fcb` y `d2d74c14` presentes, `4c617e82` y
  `b64a3b64` ausentes; 0 enlaces y 0 entradas 120000; `core.autocrlf` = false; run con los seis archivos, idénticos al kit y al cierre, los tres
  textos de las órdenes con el SHA-256 y los bytes que declaran decisiones §62, §61 y §60, y `delta.diff` y `A-4.7d863219.md` iguales a la salida de
  git; el registro de caracterización del clon con el SHA-256 de la compuerta; sin directorio de proyecto `D--r62-arch-a4r*` ni transcripción previa.
  Preflight de las guardas: `a4-guards.py self-test` PASS (86/86 mutantes), con una salida idéntica entera a `a4-selftest.json` custodiado.
- **Preflight en seco** (`launch-dry-run.json`, junto a este README): `launch.py --dry-run` sobre el kit entregado: ninguna comprobación bloqueante; 1-3 y 6-10 en Ok;
  la 4 (`--version`) y la 5 (`auth status`) no se ejecutan en seco, porque quien prepara el kit no ejecuta el binario. La línea de órdenes mide
  15 860 caracteres. Código 2, como siempre en seco.
- **Manifiesto:** `kit/kit-manifest.json` (23 archivos del kit y los seis del run; SHA-256 `bec6d9db45576910440c8310a946c2fff3580f471ce0b947eded5f55eac96580`).

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar.

## Desviaciones frente al kit de A-4 (R20261009T122416Z-bce3) y a la orden, con su motivo

- **`delta.diff` por ruta, no blob a blob.** La orden pide el diff `7d863219..0d954376` del objeto. `git diff 7d863219 0d954376` nombra los blobs como
  rutas (`a/7d863219…`); el kit usa `git diff a5b50c68 a3332495 -- docs/initiatives/I-62-A-4.md`, con la misma línea `index 7d863219..0d954376` y las
  mismas líneas de cambio (comprobado mecánicamente), y con la ruta del objeto en `a/` y `b/`, como el kit de la re-revisión de A-2.
- **Insumos añadidos al paquete §2**, por orden del Coordinator: el registro de la revisión 1, la solicitud de desbloqueo y su clasificación, las
  guardas, decisiones §61-§62 y evidencia §101-§105. Quedan fuera `a4-guards-result.json` (modo CORRECTION de la revisión 1) y
  `a4-guards-result-preview.json`, que la orden no nombra (`NotTriggered`).
- **README del kit de FX-02 por añadido.** Su blob cambió después del paquete (`fd7ef02d` → `ceb38284`, una línea añadida); se trata como
  crecimiento por añadido con prefijo exacto, igual que decisiones.
- **Esquema y coherencia.** `PriorFindingDispositions` restituidas (con los valores de la orden: CLOSED | STILL_OPEN para A62-A4-01 y APPLIED |
  NOT_APPLIED | N/A para O1..O8); `Focus` con los ocho temas de §62; `DeltaConfirmation` nuevo; `IdentityCheck`, `GuardsVerification` e `IfAgreed`
  adaptados. Dos reglas son decisiones del kit, no de la orden: un `NOT_APPLIED` exige un hallazgo enlazado (como el `STILL_OPEN` de un opcional en
  el auditor de A-2) y AGREED exige `DeltaConfirmation` todo CONFIRMED (el paquete §0 afirma esa identidad).
- **Preflight de las guardas contra el worktree, en solo lectura**, como en el kit de A-4. El worktree está ahora en `a39a1ada` (posterior al recibo),
  con A-4 todavía en `0d954376`; la salida del self-test es idéntica entera a la custodiada.
- **Unión del caso 03 del selftest** hacia el clon de la revisión 1 (`D:\r62-arch-a4`, solo como destino de lectura), en lugar del de A-3.
- **Tope de 3 600 s**, igual que en el kit de A-4: la revisión 1 (cierre de 1 826 929 bytes) duró unos 21 minutos (13:24:44Z-13:45:40Z); este cierre
  pesa 2 022 717 bytes más 105 371 del run. La espera de 120 s queda fuera del tope.
- **Sin cambios** frente al kit de A-4: la compuerta y sus predicados, la plantilla de argumentos, el binario, la mitigación de 120 s y la
  clasificación TRANSPORT_AUTH_REFRESH_CONFLICT, el analizador C3, el lector de fidelidad y las reglas de v5.1-a4.

## Supuestos no medidos (declarados antes del lanzamiento)

- La duración frente al tope de 3 600 s (estimada por la revisión 1).
- La compactación automática del contexto (no observada en C1, C2 ni en las corridas de A-2, A-3 y A-4); el prompt avisa.
- Que la espera de 120 s evite el conflicto de renovación del token del intento 1 de A-3.

## Antes del lanzamiento (procedimiento de la sesión principal)

1. Autoridad: decisiones §62, punto 2 (lanzar automáticamente con las guardas en PASS); consumo cubierto por CLAUDE-CLI-I62 = A (paquete §5). La
   decisión es de la sesión principal; el kit no la presume.
2. Opcional: `python -B kit/transport_gate.py check` (código 0) y `python -B kit/launch.py --dry-run` (todo en Ok salvo la 4 y la 5).
3. Custodiar el kit (commit de custodia) y lanzar una vez: `python -B kit/launch.py`. Si cambia un archivo del kit, `python -B kit/make_manifest.py` y
   volver a custodiar.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json` y los seis archivos del run frente a `RunFiles`.
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a4r\9eb98aef-1bc0-44b9-9dc8-7e963f22bdf2.jsonl" D:\r62-arch-a4r-run\launch\stdout.jsonl D:\r62-arch-a4r D:\r62-arch-a4r-run kit kit/result.schema.json audit.json output.json`.
- `output.json`: el `structured_output` del `result`, en UTF-8 con sangría 1. `audit.json`: la salida literal del auditor v5.1-a4r.
- `runtime-evidence.json`: de `launch/`, `run.json`, `preflight.json`, la copia de la compuerta y los SHA-256 de `stdout.jsonl` y `stderr.txt`; la
  transcripción (ruta y SHA-256, no versionada; sanear antes de versionar cualquier copia); el clon después; la integridad del kit y del run.
- A-4 no se modifica después de lanzar la re-revisión y no se declara AGREED sin la revisión acreditada y la decisión posterior del Coordinator.
