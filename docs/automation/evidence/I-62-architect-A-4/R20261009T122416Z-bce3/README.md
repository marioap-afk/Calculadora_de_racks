# I-62 — Kit de la revisión formal de la enmienda A-4 por `claude-cli` (R20261009T122416Z-bce3)

```text
LogicalReviewRequestId: L20261009T122416Z-bce3   InvocationId: I20261009T122416Z-bce3   AttemptSeq: 1   RunId: R20261009T122416Z-bce3
Autorización:   disposición del Coordinator §61, punto 2 (cuerpo pegado, 3 196 bytes, SHA-256 c8245687…): preparar el kit, el cierre de insumos, el
                auditor y la independencia de Actor/Session/Context; lanzar automáticamente por claude-cli una revisión formal independiente
                cuando las guardas estén conformes; la revisión examina expresamente H-1, H-2 y H-3, la ruta alternativa cmd.exe y la sustitución
                del Architect de FX-02 por claude-cli; no declarar A-4 AGREED sin revisión acreditada y veredicto del Coordinator. Origen de A-4:
                disposición §60, punto 2 (2 047 bytes, 3a7257e1…). Decisiones §57, punto 3 (claude-cli 2.1.293 aceptado como transporte de
                ARCHITECT y REVIEWER; nunca PRINCIPAL_COORDINATOR, WORKER ni escritura) y la línea del Owner CLAUDE-CLI-I62 = A. UNA revisión, sin
                reintento automático
Consumo:        cubierto por la línea del Owner CLAUDE-CLI-I62 = A (uso read-only de claude-cli para el rol ARCHITECT de I-62), la misma base que
                las revisiones de A-2 y A-3; decisiones §61, punto 2: lanzar automáticamente cuando las guardas estén conformes (PASS sobre 7f065a3a)
Objeto:         recibo de publicación 02be0c34efc6125627ad4b31978fb349acc724c8 (añade el resultado de las guardas en modo CORRECTION y la
                clasificación de FX-02)
                corrección 7f065a3a56102b8034247c7c64514e3f256f7b74 (A-4 corregida antes de la revisión; guardas y self-test)
                primera publicación 4710084b7589e27c511572d409b8742b75cfcbf4 (candidata 27ffa26b35ecacfae083bf9d360fe8460b8d5564, la que nombra la orden)
                docs/initiatives/I-62-A-4.md                                blob 7d863219a271b5ec427ef8669435826e19be8f11   (el «del recibo» del paquete)
                docs/initiatives/I-62-architect-package-A-4.md              blob 085f30f602532a619734b47b6b8d60035d8fb278
                docs/automation/evidence/I-62-A4/a4-guards.py               blob 2489e35f4a5f0bb43696966b669ae7dd09b5004b
                docs/automation/evidence/I-62-A4/a4-selftest.json           blob 166157f6032698e3cbde467a1cbf462f32177958
                docs/automation/evidence/I-62-A4/a4-guards-result.json      blob d011d8efe2fb7a757de6e3b3444673733ec1df14   (PASS; Mode CORRECTION; Head 7f065a3a)
                sin objeto formal anterior y sin hallazgos formales anteriores
Freeze:         b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; V14 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                (los dos commits son anteriores al rebase y no están en el clon; V14 se identifica por su blob en el commit)
Clon:           D:\r62-arch-a4 (git clone --no-local -c core.autocrlf=false --single-branch desde el worktree; main = el recibo; sin remoto; sin
                enlaces; nada posterior al recibo; kit/clone-verification.json, AllChecks)
Run:            D:\r62-arch-a4-run, con tres archivos y nada más antes del lanzamiento:
                order.txt (cuerpo de la disposición §61, 3 196 bytes, c8245687…), order-s60.txt (cuerpo de la disposición §60, 2 047 bytes,
                3a7257e1…) y prompt.md (= kit/prompt.md, 23 471 bytes, fc649df5…)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC» (re-medidos sin ejecutar el binario); claude-opus-5-5 con effort xhigh; herramientas Read,
                Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; session id 91b59aa4-cdcb-492b-94e9-715045bc7367 (uuid4);
                cwd = el clon; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s entre `auth status` y el lanzamiento
Compuerta D-01: kit/transport-gate.json en MEASURED (vía a) sobre el registro de caracterización existente R20261008T183600Z-char
                (claude-cli-characterization.json, SHA-256 20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c, fijado; la copia del clon
                de A-4): Probes.C3 PASS sobre la plantilla exacta, STALE = false, transport_gate.evaluate sin problemas
Estado:         kit custodiado ANTES del lanzamiento (commit de custodia: pendiente); NO lanzado. Lo lanza la sesión principal con kit/launch.py, una vez
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1-a4 sobre la transcripción, la salida stream-json y launch/)
```

**Recibo de publicación.** El paquete designa el objeto como «blob <el del commit de publicación> (borrador de esta pasada: 7d863219…)» y nombra
sus insumos con blobs de `524b293e`. Este README es el recibo: el commit es `02be0c34`, el objeto `7d863219…` y el paquete `085f30f6…`, calculados con
git en ese commit (`make_closure.py` los comprueba). Todos los insumos del paquete §2 conservan el blob que nombra salvo dos:
- decisiones: `625a7075…` → `133614dd…`, solo por añadido (§61); el texto nombrado es prefijo exacto;
- evidencia: `d132626e…` → `bcdbcecc…`: añade §96-§102 y, en §96, la línea 3029 (que contenía un retorno de carro suelto) pasó a ser tres líneas
  (commit `e6fe2e08`); §84 y §93-§95, las que cita el paquete, son idénticas (`AppendOnly.NonAppendChanges` del cierre).
El paquete y A-4 no se editan.

**Objeto y orden.** La orden (§61, punto 2) nombra la candidata `27ffa26b` (primera publicación, `4710084b`). Las guardas sobre esa publicación dieron
FAIL solo en G1 (una mayúscula en una cita de Q-A4-10); A-4 se corrigió antes de la revisión en `7f065a3a` (blob `7d863219`; cambian sus líneas 18 y
341, y el paquete en sus líneas 16 y 124) y las guardas en modo CORRECTION dan PASS. G2C del resultado custodiado registra el diff y que los cinco
párrafos normativos son idénticos a los de `27ffa26b`. El objeto de esta revisión es `7d863219`, como fija la orden del Coordinator del kit; el prompt
pide comprobar el vínculo en el Paso 0.

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon): 27 archivos canónicos (1 826 929 bytes):
  - la tabla de insumos del paquete §2 (19 filas, 21 archivos) y el propio paquete, con `PackageDeclaredBlob` comprobado contra el commit y
    `LineRanges` calculados desde los encabezados y las filas, y desde las líneas que cita el paquete (con su primera línea comprobada);
  - las guardas de A-4 (`a4-guards.py`, `a4-selftest.json`, `a4-guards-result.json`), que el paquete §2 no lista pero que el Paso 0 necesita;
  - por orden del Coordinator del kit, la solicitud de desbloqueo de FX-02 y su clasificación (`requests/FX-02-unblock-request-2026-10-09.md` y
    `requests/FX-02-decision-classification-2026-10-09.md`) para H-1..H-3 y U-09 (e); la evaluación de la ruta `cmd.exe` y el `result.json` del bloque
    A2-P2 ya estaban en el paquete §2;
  - run: los tres archivos, clase RUN, con `RunFileHashes`; `order.txt` y `order-s60.txt` pueden ser premisa; `prompt.md` nunca;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate` (idéntica a la de los kits de A-2 y A-3), `TimeoutSeconds`, `AuthSettleSeconds` y `Gate`.
- **Herramientas y lectura.** Solo Read, Grep y Glob, y StructuredOutput para el resultado. Los rangos orientan, no limitan. Es una violación: cualquier
  Read o Grep fuera del cierre y del run; un Grep sin `path`, sobre un directorio o con `glob` o `type`; un Glob cuyo alcance o cuyas rutas devueltas
  contengan un archivo fuera del cierre; cualquier otra herramienta; toda denegación de permiso.
- **Prompt** (`kit/prompt.md` = `D:\r62-arch-a4-run\prompt.md`, byte a byte; 23 471 bytes, 253 líneas, SHA-256
  `fc649df50d093d701c44153259a4a8e937d76dbf7d5b62ec0062bdeda22a02a9`; sin salto de línea final). Neutral: sin veredicto esperado ni contexto privado.
  Pide:
  - el Paso 0: `order.txt` entera (línea 13: la candidata), la cabecera del paquete (1-26), `a4-guards-result.json` (1-8, G2C 531-616 y 1255-1274) y
    `a4-selftest.json` (1-35);
  - el veredicto del paquete §1 con disposiciones explícitas (REQUIRED, OPTIONAL o NO_FINDING, ligadas a hallazgos) de seis temas en `Focus`:
    `A4-1_CMD_ROUTE` (la ruta `cmd.exe`), `A4-2_ARCHITECT_SUBSTITUTION` (el Architect de FX-02 por `claude-cli`), `A4-3_H1`, `A4-4_H2`, `A4-5_H3` y
    `U-09e_Q-A4-11` (quién decide A1'-A8' dentro de la ventana); cada tema contesta además las preguntas del paquete §4 que le corresponden;
  - un veredicto separado de A4-5 (`A45Verdict`: AGREED, CHANGES REQUIRED, EXCLUDE o BLOCKED — OWNER DECISION);
  - Q-A4-01..Q-A4-14; la decisión del Owner; si las dos líneas del Owner de A-4 §7 (`A4-SONDA-CONSUMO` y `A4-CLAUDE-FX02-CONSUMO`) solo autorizan
    consumo (`OwnerConsumptionLines`); `NoChangeConfirmation` (A-1, A-2, A-3, la fila de topes de D.3, P-01, OD-2, la receta 16.4, los descriptores,
    el texto de AUTOMATION_PLAN 16.20-16.21, el de B1-B10 y producción); `GuardsVerification` de solo lectura; materialidad; modo e identidad;
  - los identificadores de cuenta inyectados, solo por su tipo.
- **Esquema** (`kit/result.schema.json`; compacto, 11 042 caracteres ASCII): veredicto (AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION |
  NOT_ACCREDITED); `RequiredFindings` (`A62-A4-NN`) y `OptionalFindings` (`A62-A4-ON`) con `PremiseRefs`; `Focus` (seis temas, con disposición,
  `FindingIds` y `PremiseRefs`); `A45Verdict`; `QuestionDispositions` (Q-A4-01..14); `NoChangeConfirmation` (11); `OwnerConsumptionLines`;
  `GuardsVerification` (G1, G2C, G2D, G3, G4, G5, G5b); `Materiality`; `OwnerAuthorityCheck`; `IfAgreed` (7); `IdentityCheck` (cinco booleanas del
  Paso 0); `ReviewerDeclaredIdentity`, `InjectedContextDeclaration` y `KnownLimitations`. `NOT_ASSESSED` y `null` solo valen con NOT_ACCREDITED.
- **Compuerta de transporte D-01** (`kit/transport_gate.py`, predicados sin cambio): entregada en MEASURED (vía a) sobre el registro existente
  `20d0de2c…` (copia del clon de A-4, idéntica a la del worktree y a la del clon de A-3). Sin ejecutar el binario: SHA-256 `8693c4a0…` y firma
  válida; los blobs del catálogo, de `routing.md` y del descriptor de `claude-cli` no cambiaron desde la caracterización.
- **Auditor v5.1-a4** (`kit/post-review.py`): las reglas de v5.1 y v5.1-a3, adaptadas al objeto y al cierre de A-4. Cada fallo es un motivo, y
  ACCREDITED exige cero motivos: lanzamiento (`run.json`), preflight (diez comprobaciones), compuerta, stream-json (`init` y `result`), transcripción
  (sesión, cwd, versión, primer mensaje = `prompt.md` y ningún otro, modelo y effort en cada mensaje), herramientas, rutas y Glob, fidelidad de cada
  Read, premisas (también en `Focus`), `structured_output` coherente con la transcripción y con el esquema, custodia del run y clon después de la
  corrida. Coherencia de A-4:
  - cobertura de Q-A4-01..14 y de los seis temas de `Focus`, con las mismas reglas de enlace (REQUIRED u OPTIONAL con su hallazgo; NO_FINDING sin
    hallazgos);
  - `A45Verdict`: NOT_ASSESSED solo con NOT_ACCREDITED; CHANGES REQUIRED con un REQUIRED vinculado; BLOCKED con `OwnerDecisionRequired`; ids
    existentes;
  - AGREED: cero REQUIRED (también en `Focus` y en las preguntas), `A45Verdict` en AGREED o EXCLUDE, sin decisión del Owner,
    `OwnerConsumptionLines` = ONLY_CONSUMPTION, `NoChangeConfirmation` todo NO_CHANGE e `IfAgreed` todo en true;
  - CHANGES REQUIRED, BLOCKED y NOT_ACCREDITED como antes; identidad revisada (commit, ruta, blob) e ids de la invocación.
  Manejo defensivo, sin cambio sobre los formatos medidos: un `message` que no es un objeto, una línea JSON que no es un objeto o una denegación que
  no es un objeto nunca rompen la auditoría.
  Uso: `python -B kit/post-review.py <transcripción> D:\r62-arch-a4-run\launch\stdout.jsonl D:\r62-arch-a4 D:\r62-arch-a4-run kit
  kit/result.schema.json <audit.json> [<output.json>]`.
- **Lanzador** (`kit/launch.py`; no lo ejecuta quien prepara el kit, salvo `--dry-run`). Se niega a lanzar (código 2, sin escribir nada) si falla:
  1. integridad del kit frente a `kit-manifest.json`; 2. transporte del cierre (incluidas `ArgsTemplate`, `TimeoutSeconds` y `AuthSettleSeconds`);
  3. binario (ruta, SHA-256, Authenticode); 4. `--version` = 2.1.293; 5. `auth status` con `loggedIn`; 6. clon (HEAD, limpio, blobs, sin enlaces);
  7. run con exactamente los tres archivos y sus hashes; 8. sin transcripción previa con el session id ni directorio de proyecto `D--r62-arch-a4*`;
  9. línea de órdenes de menos de 32 000 caracteres (mide 13 224); 10. compuerta en MEASURED o ACCEPTED.
  Después escribe `preflight.json` y la copia del registro en `D:\r62-arch-a4-run\launch\`, **espera 120 s desde el final de `auth status`**, vuelve a
  medir el binario y lanza la ruta resuelta con la lista exacta, stdin = `prompt.md`, cwd = el clon y un tope de 3 600 s con terminación. Escribe
  `stdout.jsonl`, `stderr.txt` y `run.json` (con `AuthSettle` y `TransportFailure`: TRANSPORT_AUTH_REFRESH_CONFLICT si el `result` contiene
  «Failed to refresh OAuth token» con 0 tokens). No reintenta en ningún caso. `--dry-run` no ejecuta el binario (4 y 5 quedan sin ejecutar).
  La lista que lanza es, literalmente: `<%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe resuelto> -p --model claude-opus-5-5 --effort
  xhigh --output-format stream-json --verbose --safe-mode --strict-mcp-config --no-chrome --tools Read,Grep,Glob --permission-mode dontAsk
  --permission-prompts none --add-dir D:\r62-arch-a4-run --session-id 91b59aa4-cdcb-492b-94e9-715045bc7367 --json-schema <kit/result.schema.json
  compacto, ASCII>`, con stdin = `D:\r62-arch-a4-run\prompt.md` y cwd = `D:\r62-arch-a4`.
- **Selftest previo** (`kit/selftest-result.json`): **112/112 PASS** sobre el clon y el run reales, que quedan limpios e idénticos: 71 casos de
  auditoría (los 66 del kit de A-3, adaptados a A-4, y 5 nuevos, 65-69, para las reglas de `Focus` y de `A45Verdict`, uno de ellos positivo: AGREED
  con A4-5 excluida) y 41 de la compuerta.
- **Mutación** (`kit/mutation-post-review.py` → `kit/mutation-result.json`): cada sitio de regla de `post-review.py` (95) y de `transport_gate.py`
  (36) se desactiva por turno en una copia y se ejecutan los 112 casos: **131/131 mutantes muertos**, con la base sin mutar en 112/112. Corrida
  acotada (tope de 3 600 s; duró unos 22 minutos) sobre los archivos finales del kit.
- **Fidelidad previa** (`kit/preflight-read-fidelity.json`): 3 lecturas en el clon, sin filtro, las tres fieles: el paquete entero y A-4 entera (1-200
  y 201-379). Salen de la transcripción del subagente que preparó el kit; su SHA-256 es el de la instantánea medida.
- **Clon y run** (`kit/clone-verification.json`, generado por `kit/verify_clone.py`), todo comprobado: HEAD = el recibo, solo `main`, sin remotos,
  limpio con ignorados, ninguna etiqueta ni commit fuera de la historia de HEAD; 29 blobs designados; la candidata en `4710084b` y el objeto en
  `7f065a3a`; `7f065a3a`, `4710084b`, `524b293e`, `f504f6a9`, `c8d69fcb` y `d2d74c14` presentes, `4c617e82` y `b64a3b64` ausentes; 0 enlaces y 0
  entradas 120000; `core.autocrlf` = false; run con los tres archivos, idénticos al kit y al cierre, y los dos textos de las órdenes con el SHA-256 y
  los bytes que declaran decisiones §61 y §60; el registro de caracterización del clon con el SHA-256 de la compuerta; sin directorio de proyecto ni
  transcripción previa. Preflight de las guardas: `a4-guards.py self-test` PASS (66/66 mutantes), con una salida idéntica entera a `a4-selftest.json`
  custodiado (ver desviaciones).
- **Preflight en seco** (`launch-dry-run.json`, junto a este README): `launch.py --dry-run` sobre el kit entregado: ninguna comprobación bloqueante; 1-3 y 6-10 en
  Ok; la 4 (`--version`) y la 5 (`auth status`) no se ejecutan en seco, porque quien prepara el kit no ejecuta el binario. La línea de órdenes mide
  13 224 caracteres.
- **Manifiesto:** `kit/kit-manifest.json` (20 archivos del kit y los tres del run; SHA-256 `2aa5e927582b1f618b44de93b60f4ad7a5e3bf3380b1e32dcbe3b31e3cdd4acb`).

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar.

## Desviaciones frente al kit de A-3 (R20261009T040327Z-58a1, intento 2), con su motivo

- **Insumos añadidos al paquete §2.** La solicitud de desbloqueo de FX-02 y su clasificación, por orden expresa del Coordinator del kit (el revisor
  las necesita para H-1..H-3 y U-09 (e)); el paquete §2 dice que la solicitud no es insumo canónico, y el prompt lo declara y recuerda que, según la
  orden §61, punto 3, es un registro de propuestas, no de autorizaciones. También las guardas de A-4, que el paquete no lista y el Paso 0 necesita.
- **Objeto distinto del que nombra la orden.** La orden nombra la candidata `27ffa26b`; el kit revisa la corrección `7d863219` (orden del
  Coordinator del kit y recibo `02be0c34`). El vínculo se comprueba mecánicamente (`make_closure.py` y `verify_clone.py`) y el revisor lo comprueba en
  el Paso 0 con G2C del resultado custodiado.
- **Evidencia que no creció solo por añadido.** En `e6fe2e08` la línea 3029 de §96 (con un retorno de carro suelto) se reescribió en tres líneas. El
  cierre lo registra (`NonAppendChanges`) y comprueba que §84 y §93-§95, las que cita el paquete, son idénticas.
- **Archivos del run.** Tres: `order.txt` (§61) y `order-s60.txt` (§60, el origen de A-4), los dos con su identidad contrastada con decisiones §61 y
  §60 en el commit, y `prompt.md`.
- **Esquema y coherencia.** `Focus` pasa de ítems numerados a seis temas con disposición REQUIRED, OPTIONAL o NO_FINDING ligada a hallazgos (como las
  preguntas), con `PremiseRefs` auditadas; `A45Verdict` nuevo; `OwnerConsumptionLines` en lugar de `ScopeConfirmation`; sin `ReviewedAnnexBlob`;
  `NoChangeConfirmation`, `IdentityCheck`, `GuardsVerification` e `IfAgreed` adaptados al paquete §1 de A-4. Reglas nuevas del auditor: cobertura y
  enlaces de `Focus`; las cuatro de `A45Verdict`; AGREED exige además `A45Verdict` en AGREED o EXCLUDE y `OwnerConsumptionLines` = ONLY_CONSUMPTION.
- **Tope de 3 600 s** (A-3: 7 200 s). A-4 pesa 40 944 bytes y el cierre 1 826 929; la corrida de A-3 (objeto de 200 993 bytes) duró 13 minutos
  (05:40:38Z-05:53:43Z) y la de A-2, unos 15: el tope deja más de cuatro veces ese margen. La espera de 120 s queda fuera del tope.
- **Preflight de las guardas contra el worktree, en solo lectura.** `a4-guards.py` necesita `origin/main` y los commits del Freeze y de V14,
  anteriores al rebase, que un clon de una sola rama sin remoto no tiene (en el clon da FAIL en G1 por eso). El a4-guards.py del clon (blob
  `2489e35f`) se ejecuta con `--repo` = el worktree (rev-parse, diff entre commits, merge-base, cat-file y config; ninguna escritura) y con `--a4` y
  `--pkg` = los archivos del clon, como se generó el resultado custodiado; su salida es idéntica entera a `a4-selftest.json`.
- **Sin cambios** frente al kit de A-3: la compuerta y sus predicados, la plantilla de argumentos, el binario, la mitigación de 120 s y la
  clasificación TRANSPORT_AUTH_REFRESH_CONFLICT, el analizador C3, el lector de fidelidad y las reglas de v5.1.

## Supuestos no medidos (declarados antes del lanzamiento)

- La duración frente al tope de 3 600 s (estimada por las corridas de A-2 y A-3).
- La compactación automática del contexto (no observada en C1, C2 ni en las corridas de A-2 y A-3); el prompt avisa.
- Que la espera de 120 s evite el conflicto de renovación del token del intento 1 de A-3 (en el intento 2 de A-3 funcionó, una vez).

## Antes del lanzamiento (procedimiento de la sesión principal)

1. Autoridad de consumo: el paquete §5 dice que el consumo de `claude-cli` para revisiones reales de I-62 lo cubre CLAUDE-CLI-I62 = A (DEC L1085). La
   decisión es de la sesión principal; el kit no la presume.
2. Opcional: `python -B kit/transport_gate.py check` (código 0) y `python -B kit/launch.py --dry-run` (todo en Ok salvo la 4 y la 5).
3. Custodiar el kit (commit de custodia) y lanzar una vez: `python -B kit/launch.py`. Si cambia un archivo del kit, `python -B kit/make_manifest.py` y
   volver a custodiar.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json` y los tres archivos del run frente a `RunFiles`.
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a4\91b59aa4-cdcb-492b-94e9-715045bc7367.jsonl" D:\r62-arch-a4-run\launch\stdout.jsonl D:\r62-arch-a4 D:\r62-arch-a4-run kit kit/result.schema.json audit.json output.json`.
- `output.json`: el `structured_output` del `result`, en UTF-8 con sangría 1. `audit.json`: la salida literal del auditor v5.1-a4.
- `runtime-evidence.json`: de `launch/`, `run.json`, `preflight.json`, la copia de la compuerta y los SHA-256 de `stdout.jsonl` y `stderr.txt`; la
  transcripción (ruta y SHA-256, no versionada; sanear antes de versionar cualquier copia); el clon después; la integridad del kit y del run.
- A-4 no se modifica después de lanzar la revisión y no se declara AGREED sin la revisión acreditada y el veredicto del Coordinator.
