# I-62 — Kit de la revisión formal de la enmienda A-3 por `claude-cli` (R20261009T040327Z-58a1, intento 2)

```text
LogicalReviewRequestId: L20261009T040327Z-58a1   InvocationId: I20261009T052403Z-58a1   AttemptSeq: 2   RunId: R20261009T040327Z-58a1
Intentos:       1 = INVALID_LAUNCH (TRANSPORT_AUTH_REFRESH_CONFLICT, antes de cualquier turno del modelo; attempt-1/accreditation.json);
                2 = este kit, el único intento que queda; sin reintento automático
Autorización:   disposición del Coordinator §58, punto 3 (cuerpo pegado, 2 767 bytes, SHA-256 3ed7e135…): revisión formal independiente por
                claude-cli del objeto exacto; preparar y custodiar el paquete, el cierre de insumos, el auditor y la preflight sobre la versión
                exacta; invocar automáticamente un Architect elegible, de solo lectura, con Actor, Session y Context independientes; no modificar A-3
                tras lanzar la revisión; no declarar A-3 AGREED anticipadamente. Decisiones §57, punto 3 (claude-cli 2.1.293 aceptado como
                transporte de ARCHITECT y REVIEWER; nunca PRINCIPAL_COORDINATOR, WORKER ni escritura) y la línea del Owner CLAUDE-CLI-I62 = A;
                UNA revisión, sin reintento automático
Consumo:        cubierto por la línea del Owner CLAUDE-CLI-I62 = A (uso read-only de claude-cli para el rol ARCHITECT de I-62), la misma base que
                la re-revisión de A-2; decisiones §60, punto 0: completar la revisión formal de A-3 y comunicar su veredicto
Objeto:         recibo de publicación 492885254b58203f0bc0099345db2998b25d2a2b (añade a3-guards-result.json; sus demás cambios
                son registros: solicitud de recuperación, evidencia, paquetes del Owner y estado)
                commit de publicación 91e29886a4870dd673372c66453d76ea503a22b9 (padre 76b48e78 = A3_BASE; preparada sobre b0884216)
                docs/initiatives/I-62-A-3.md                                blob ea6721f78cb63fbc4f9d99563f3262eb36a32c5d   <A3_BLOB>
                docs/initiatives/I-62-A-3-annex-maf-codex.md                blob f410f7fced25440be3866473974f417d2f6a9b50   <ANNEX_BLOB>
                docs/initiatives/I-62-architect-package-A-3.md              blob d289329d31f163739ea8e55f5a3ad3227a5cc62e   (sin blob propio)
                docs/automation/decisions/I-62.md                           blob 27591f161aabb2de30610b0d17341597fd2a4079   <DEC_BLOB>
                docs/automation/evidence/I-62-evidence.md                   blob 6855110a61b70a5ceb7e3928b5de4671cb6af7d6   <EV_BLOB>
                docs/automation/evidence/I-62-A3/a3-guards.py               blob 215b2e43e40c67bc8d47cbfc5ccc3814b80451ea   <GUARDS_BLOB>
                docs/automation/evidence/I-62-A3/a3-selftest.json           blob 38a79ca082617781e1332daa3a4d127cb433dacc   <SELFTEST_BLOB>
                docs/automation/evidence/I-62-A3/a3-guards-result.json      blob 70b23c0dbcae830cd86394a204d94ee39d764a0b   (Result PASS; Head 91e29886)
                sin objeto formal anterior y sin hallazgos formales anteriores (las rondas adversariales de A-3 §12 son internas)
Freeze:         b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; V14 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                (los dos commits son anteriores al rebase y no están en el clon; V14 se identifica por su blob en el commit)
Clon:           D:\r62-arch-a3 (git clone --no-local -c core.autocrlf=false --single-branch desde el worktree; main = el recibo; sin remoto; sin
                enlaces; 3ef58efa, decisiones §58, posterior al recibo, eliminado del clon; kit/clone-verification.json, AllChecks)
Run:            D:\r62-arch-a3-run, con cuatro archivos y nada más antes del lanzamiento:
                order.txt (cuerpo de la disposición §58, 2 767 bytes, 3ed7e135…), prompt.md (= kit/prompt.md, 23 417 bytes, 42fe64ca…),
                order-s56.txt (texto fijo de la orden §56, 8 562 bytes, e8b00328…) y order-s57.txt (texto fijo de la orden §57, 3 654 bytes, 00a13ae5…)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC» (re-medidos sin ejecutar el binario); claude-opus-5-5 con effort xhigh; herramientas Read,
                Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; session id 2e266dfe-a1ca-4372-acd9-924c980bf50a (uuid4 nuevo del intento 2);
                cwd = el clon; prompt por stdin; resultado por --json-schema; tope de 7 200 s; espera de 120 s entre `auth status` y el
                lanzamiento (mitigación del intento 1)
Compuerta D-01: kit/transport-gate.json en MEASURED (vía a) sobre el registro de caracterización existente R20261008T183600Z-char
                (claude-cli-characterization.json, SHA-256 20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c, fijado; la copia del
                clon, idéntica a la del worktree): Probes.C3 PASS sobre la plantilla exacta, STALE = false, transport_gate.evaluate sin problemas
Estado:         kit del intento 2 custodiado ANTES del lanzamiento (commit de custodia: pendiente; el intento 1 se custodió en 524b293e); NO
                lanzado. Lo lanza la sesión principal con kit/launch.py, una vez
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1-a3 sobre la transcripción, la salida stream-json y launch/)
```

**Recibo de publicación.** El paquete fija varios blobs con marcadores (`<A3_BLOB>`, `<ANNEX_BLOB>`, `<DEC_BLOB>`, `<EV_BLOB>`, `<GUARDS_BLOB>`,
`<SELFTEST_BLOB>`) que se fijan «al publicar». Este README es el recibo: el commit es `49288525` y los blobs son los de la tabla de arriba, calculados
con git en ese commit (`make_closure.py` los comprueba y falla si cambian). El paquete y A-3 no se editan.

## Historial de intentos

- **Intento 1** (InvocationId `I20261009T040327Z-58a1`, session id `ae3590eb-b508-4949-84da-8aa7340df993`; kit custodiado en `524b293e`,
  `docs/automation/evidence/I-62-architect-A-3/R20261009T040327Z-58a1/`, idéntico byte a byte): la sesión principal lo lanzó una vez con
  `python -B launch.py` a las 05:18:49Z. La preflight pasó (las diez comprobaciones en Ok). El proceso corrió de 05:18:56Z a 05:19:06Z: salida 1, sin
  terminación ni tope. `stdout.jsonl` trae un `init` correcto (session id, cwd `D:\r62-arch-a3`, `claude-opus-5-5`, `dontAsk`, herramientas Glob,
  Grep, Read y StructuredOutput, 2.1.293), un único mensaje sintético del asistente (modelo `<synthetic>`, 0 tokens) y un `result` con
  `is_error` = true, `terminal_reason` = `api_error`, coste 0 y el texto «Failed to refresh OAuth token: another Claude Code process is
  refreshing it or exited mid-refresh…». Ningún turno del modelo, ninguna lectura, ningún resultado, ningún consumo. Causa probable (hecho de la
  sesión principal, no medido por el invocador): la comprobación 5 (`claude.exe auth status --json`) inició una renovación del token OAuth y terminó
  a mitad, justo antes del lanzamiento; en la corrida de A-2 el token no necesitó renovarse. Después, `auth status` informa `loggedIn` = true.
  Registro del invocador (`attempt-1/accreditation.json`): **INVALID_LAUNCH**, causa **TRANSPORT_AUTH_REFRESH_CONFLICT**; los cuatro archivos del run
  y el clon, sin cambio (comprobados antes y después). Custodia:
  - `attempt-1/launch-raw/`: `D:\r62-arch-a3-run\launch\` movido intacto (sus SHA-256 son los que registra `run.json`); contiene rutas del perfil
    del usuario: **no versionar**;
  - `attempt-1/launch/`: copia saneada (rutas del perfil → `%USERPROFILE%`; solo cambia `stdout.jsonl`);
  - `attempt-1/project-dir/`: `%USERPROFILE%\.claude\projects\D--r62-arch-a3\` movido intacto (solo la transcripción `ae3590eb….jsonl`,
    SHA-256 `9d1da64f…` = el de `run.json`); contiene el correo, el uuid de la organización y rutas del perfil: **no versionar**;
  - `attempt-1/project-dir.sanitized/`: copia saneada de esa transcripción (rutas → `%USERPROFILE%`, correo → `<user-email>`, uuid de la
    organización → `<organization-uuid>`, nombre de usuario → `%USERNAME%`).
  Con eso el run vuelve a tener solo sus cuatro archivos y no queda directorio de proyecto `D--r62-arch-a3*` (preflight 7 y 8).
- **Intento 2** (este kit; InvocationId `I20261009T052403Z-58a1`, AttemptSeq 2, session id nuevo `2e266dfe-a1ca-4372-acd9-924c980bf50a`): el
  único intento que queda. Mismo objeto, mismo cierre canónico, mismo clon y mismos archivos del run salvo `prompt.md` (identidades del intento 2 y
  una frase neutral sobre el intento 1). Sin reintento automático: si el intento 2 vuelve a fallar, `launch.py` lo registra y la decisión es de la
  sesión principal y del Coordinator.

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon):
  - canónicos: la tabla de insumos del paquete §2 en el commit del recibo y el propio paquete (27 archivos). Cada uno lleva `Blob`, `Bytes`,
    `Lines`, `Sha256`, codificación, `PackageDeclaredBlob` (lo que fija el paquete, comprobado contra el commit) y `LineRanges` calculados
    mecánicamente desde los encabezados, las filas y las líneas que cita el paquete (título exacto, fila única o contenido comprobado);
  - crecimiento solo por añadido (`AppendOnly`): la evidencia (blob de preparación `6a2b1569…` → `6855110a…`, §88 y §89 añadidas) y
    `owner-decision-packets.md` (el paquete nombra `cdb86114…`; en el recibo es `f39c0eff…`, con las líneas 340-346 añadidas): el texto del blob
    nombrado es prefijo exacto del texto del commit, y las líneas que citan A-3 y el paquete no cambian;
  - transitivos: ninguno (con `--safe-mode` el runtime no inyecta CLAUDE.md, memoria ni ganchos);
  - run: los cuatro archivos, clase RUN, con `RunFileHashes`. `order.txt`, `order-s56.txt` y `order-s57.txt` pueden ser premisa; `prompt.md` nunca;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate` (idéntica a la del kit de A-2: la plantilla exacta del lanzamiento, con `<add-dir>`,
    `<session-id>` y `<json-schema>` como marcadores), `AddDirStatus`, `TimeoutSeconds` y `Gate` (D-01).
- **Herramientas y lectura.** El revisor solo tiene Read, Grep y Glob, y StructuredOutput para el resultado. Los rangos orientan, no limitan. Es una
  violación:
  - cualquier Read o Grep fuera de los archivos del cierre y del run;
  - un Grep sin `path`, sobre un directorio o con `glob` o `type`;
  - un Glob cuyo alcance (base y patrón, evaluados sobre el disco) o cuyas rutas devueltas contengan un archivo fuera del cierre;
  - cualquier otra herramienta, aunque el runtime la rechace;
  - toda denegación de permiso.
- **Prompt** (`kit/prompt.md` = `D:\r62-arch-a3-run\prompt.md`, byte a byte; 23 417 bytes, 256 líneas, SHA-256
  `42fe64cad219506625e6cf8c39adce6fc6cf9690cc2bf9c730c68b42eea6756f`). Es neutral: no trae veredicto esperado ni contexto privado de la sesión autora,
  y no tiene salto de línea final. Pide:
  - el Paso 0 con lo que el revisor puede comprobar con sus herramientas: `order.txt` entera (líneas 25-26: objeto y blob), la cabecera del paquete
    (1-40), `a3-guards-result.json` (`Head`, G3 y G5b) y `a3-selftest.json` (`A3Text` y `A3TextBlob`);
  - el veredicto del paquete §1: `Focus` 1-8 (paquete §4; la pregunta 9 son las Q-A3), Q-A3-01..Q-A3-22 de A-3 §9 (también Q-A3-19 y Q-A3-22, que
    A-3 marca como retiradas: el paquete pide disponer las 22), la decisión del Owner, `NoChangeConfirmation`, `ScopeConfirmation`, `GuardsVerification`
    de solo lectura, la materialidad, `IfAgreed`, el modo y la identidad (commit, ruta y blob del objeto, y blob del anexo);
  - dice que no hay hallazgos formales anteriores: las rondas adversariales de A-3 §12 son internas y forman parte del objeto;
  - en `InjectedContextDeclaration`, los identificadores de cuenta que inyecte el runtime (correo, uuid de organización) solo por su tipo, sin su
    valor: `output.json` se versiona como evidencia.
- **Esquema** (`kit/result.schema.json`; se pasa compacto a `--json-schema`, 10 891 caracteres ASCII). Lleva el veredicto (AGREED | CHANGES
  REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED) y estos campos:
  - `RequiredFindings` (`A62-A3-NN`) y `OptionalFindings` (`A62-A3-ON`) con `PremiseRefs`; `QuestionDispositions` con los ids reales Q-A3-01..22;
  - `Focus` (1-8), `NoChangeConfirmation` (P-01/P-11/OD-2, topes de D.3, B.1-B.11, `state/v2`, texto de §20, A-1, A-2 y F4), `ScopeConfirmation`
    (lecturas de B.2, B.4, B.5 y §20.5.1 exactamente las declaradas, ningún verificador, ninguna clase de cambios aceptables, ninguna lectura de
    valores, materialización fuera del registro), `GuardsVerification` (G1-G5 y G5b, solo lectura), `Materiality`, `OwnerAuthorityCheck` (con
    `FingerprintAcceptance` y `ConfigurationValueReading`) e `IfAgreed`;
  - `IdentityCheck` (base: el clon verificado por el invocador; seis booleanas del Paso 0), `ReviewedAnnexBlob`, `ReviewerDeclaredIdentity`,
    `InjectedContextDeclaration` y `KnownLimitations`.
  `NOT_ASSESSED` y `null` solo valen con NOT_ACCREDITED, y lo exige el auditor. Sin `$schema`, `$defs`, `$ref`, `anyOf` ni `const`; la regla
  «invariante o contraejemplo» del REQUIRED está en la coherencia del auditor.
- **Compuerta de transporte D-01** (`kit/transport_gate.py` y `kit/transport-gate.json`; la comparten `launch.py` y el auditor; predicados sin
  cambio frente al kit de A-2):
  - `ArgsTemplate` = la plantilla de la lista que lanza `launch.py`; C1 midió esa plantilla sin `--add-dir` y C3, con `--add-dir`;
  - estado entregado: `MEASURED` (vía a) sobre el registro de caracterización existente, con su SHA-256 fijado en la compuerta
    (`20d0de2c…`): `Probes.C3` en PASS sobre la plantilla exacta, `Eligibility.MeasuredArgsTemplate` = la plantilla, `STALE` = false y
    `ARCHITECT` ELIGIBLE. Ruta fijada: la copia del registro en el clon (`D:\r62-arch-a3\docs\automation\evidence\I-62-claude-cli\R20261008T183600Z-char\claude-cli-characterization.json`,
    inmutable mientras el clon esté limpio en el recibo), idéntica byte a byte a la del worktree;
  - además, sin ejecutar el binario: su SHA-256 sigue siendo `8693c4a0…` y su firma, válida; los blobs de `model-catalog.md`, `routing.md` y del
    descriptor de `claude-cli` no cambiaron desde la caracterización (`d2d74c14`);
  - `launch.py` copia el registro en `launch/` y el auditor vuelve a evaluar la compuerta sobre esa copia.
- **Auditor v5.1-a3** (`kit/post-review.py`). Las reglas de v5.1, adaptadas al objeto y al cierre de A-3. Cada fallo es un motivo, y ACCREDITED
  exige cero motivos:
  - lanzamiento: `stdout.jsonl` dentro de `<run>\launch`; `run.json` con código de salida 0, sin terminación, el session id, el ejecutable MEDIDO
    (ruta resuelta y SHA-256 tomados justo antes de lanzarlo) igual al binario del cierre, los argumentos exactos, cwd = el clon, y `PromptSha256`,
    `StdoutSha256` y `Transcript.Sha256` iguales a `prompt.md`, al stdout y a la transcripción auditados;
  - preflight: `preflight.json` con las diez comprobaciones presentes y en Ok, el session id, el binario medido (ruta y SHA-256) igual al del
    cierre, la versión `2.1.293 (Claude Code)`, `loggedIn` = true y la compuerta igual a `kit/transport-gate.json`;
  - compuerta: `transport_gate.evaluate` sin problemas sobre las copias de `launch/`;
  - stream-json: `init` (session id, cwd, modelo, `permissionMode`, herramientas {Glob, Grep, Read, StructuredOutput}, sin MCP, versión) y `result`
    (`success`, session id, `structured_output` presente, `permission_denials` vacío, `modelUsage` solo del modelo pedido, sin subagentes);
  - transcripción: una sola sesión, la fijada, que es también el nombre del archivo; cwd = el clon; versión 2.1.293; sin cadena lateral; el primer
    mensaje de usuario idéntico a `prompt.md` y ningún otro (un resumen de compactación cuenta como otro); `claude-opus-5-5` y `xhigh` en cada
    mensaje del asistente;
  - herramientas, rutas y Glob como arriba (rutas efectivas con `realpath`, seguras frente a uniones); fidelidad de cada Read; premisas encontradas
    en las líneas citadas y entregadas fielmente por Read, solo de archivos del cierre o de `order.txt`, `order-s56.txt` y `order-s57.txt`;
  - el `structured_output` coincide con la última llamada a StructuredOutput y su adjunto, valida contra el esquema y es coherente: cobertura
    (Q-A3-01..22 y Focus 1-8), reglas de AGREED (cero REQUIRED, sin decisión del Owner, `NoChangeConfirmation` todo NO_CHANGE, `ScopeConfirmation`
    todo CONFIRMED e `IfAgreed` todo en true), CHANGES REQUIRED, BLOCKED y NOT_ACCREDITED, un REQUIRED con invariante o contraejemplo, la
    identidad revisada (commit, ruta, blob del objeto y blob del anexo) y los ids de la invocación;
  - custodia del run: archivo del run = copia del kit = `RunFileHashes`; el directorio del run solo admite los cuatro archivos y `launch/`, y
    `launch/` solo los archivos que escribe `launch.py`;
  - clon después de la corrida: HEAD, rama única, sin remoto, limpio con ignorados, blobs designados y sin enlaces;
  - manejo defensivo, sin cambio de comportamiento sobre los formatos medidos: una entrada de stream-json o de la transcripción cuyo `message` no
    es un objeto (como una entrada `system/permission_denied` con `message` en texto), una línea JSON que no es un objeto o una denegación que no es
    un objeto nunca rompen la auditoría.
  Uso: `python -B kit/post-review.py <transcripción> <run>\launch\stdout.jsonl D:\r62-arch-a3 D:\r62-arch-a3-run kit kit/result.schema.json
  <audit.json> [<output.json>]`. `run.json`, `preflight.json` y las copias de la compuerta se leen de `<run>\launch`.
- **Lanzador** (`kit/launch.py`; no lo ejecuta quien prepara el kit, salvo `--dry-run`). Si falla cualquiera de estas comprobaciones, se niega a
  lanzar (código 2) y no escribe nada:
  1. integridad del kit frente a `kit-manifest.json`, sin archivos distintos ni sin listar;
  2. transporte del cierre, incluidas `ArgsTemplate`, `TimeoutSeconds` y `AuthSettleSeconds`;
  3. binario: ruta resuelta, SHA-256 y Authenticode;
  4. `--version` = 2.1.293;
  5. `auth status` con `loggedIn` (solo se conservan `loggedIn`, `authMethod` y `apiProvider`);
  6. clon con HEAD, limpio, blobs y sin enlaces;
  7. run con exactamente los cuatro archivos y sus hashes;
  8. sin transcripción previa con el session id ni directorio de proyecto para el clon o el run (`D--r62-arch-a3*`);
  9. línea de órdenes de menos de 32 000 caracteres (mide 13 013);
  10. compuerta D-01 en MEASURED o ACCEPTED, con sus registros y sus SHA-256, y la plantilla de los argumentos = la de la compuerta.
  Después escribe `preflight.json` y la copia del registro en `D:\r62-arch-a3-run\launch\`, **espera 120 s contados desde el final de `auth
  status`** (intento 2), vuelve a medir el binario justo antes de lanzarlo (si
  cambió, aborta sin lanzar) y lanza la ruta resuelta con la lista exacta de argumentos, stdin = `prompt.md`, cwd = el clon y un tope de 7 200 s con
  terminación (`terminate` y, 60 s después, `kill`). Escribe `stdout.jsonl`, `stderr.txt` y `run.json` (con `AuthSettle`: inicio, fin y segundos de la
  espera; y `TransportFailure`: `TRANSPORT_AUTH_REFRESH_CONFLICT` si el `result` contiene «Failed to refresh OAuth token» con 0 tokens, o null).
  No reintenta en ningún caso. Hereda el entorno de la sesión principal,
  como en la caracterización (C1). `--dry-run` hace solo la preflight, **sin ejecutar el binario** (4 y 5 quedan sin ejecutar), sin escribir y sin
  lanzar.
  La lista que lanza es, literalmente: `<%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe resuelto> -p --model claude-opus-5-5 --effort
  xhigh --output-format stream-json --verbose --safe-mode --strict-mcp-config --no-chrome --tools Read,Grep,Glob --permission-mode dontAsk
  --permission-prompts none --add-dir D:\r62-arch-a3-run --session-id 2e266dfe-a1ca-4372-acd9-924c980bf50a --json-schema <kit/result.schema.json
  compacto, ASCII>`, con stdin = `D:\r62-arch-a3-run\prompt.md` y cwd = `D:\r62-arch-a3`.
- **Selftest previo** (`kit/selftest-result.json`, repetido para el intento 2): **107/107 PASS** sobre el clon y el run reales, que quedan limpios e idénticos (66 casos de
  auditoría y 41 de la compuerta, los mismos del kit de A-2, con las particularidades de A-2 sustituidas por las de A-3: Paso 0 de A-3, ids Q-A3 y
  A62-A3, archivos del run de A-3, el anexo y `ScopeConfirmation`). Cada caso audita contra una compuerta sintética (MEASURED construida con
  `c3_analysis.analyze` sobre una sonda PASS sintética; ACCEPTED en el caso 41). Los casos 57 y 58, que en A-2 probaban reglas de hallazgos
  anteriores que aquí no existen, prueban ahora AGREED con un apartado de `ScopeConfirmation` sin CONFIRMED y otro blob del anexo.
- **Mutación** (`kit/mutation-post-review.py` → `kit/mutation-result.json`): cada sitio de regla de `post-review.py` (91: los cinco sitios de
  hallazgos anteriores del kit de A-2 ya no existen) y de `transport_gate.py` (36) se desactiva por turno en una copia y se ejecutan los 107 casos:
  **127/127 mutantes muertos** (cada regla desactivada hace fallar al menos un caso), con la base sin mutar en 107/107. Corrida acotada (tope de
  3 600 s; duró unos 25 minutos) sobre los archivos finales del kit (SHA-256 de `post-review.py` y `transport_gate.py` registrados en el resultado). No se repite para el intento 2:
  ninguno de los dos módulos mutados cambió (mismos SHA-256 `46c1aec4…` y `83e2a917…`).
- **Fidelidad previa** (`kit/preflight-read-fidelity.json`): 11 lecturas en el clon, sin filtro: 10 fieles y 1 fallida por el tamaño de su salida
  (A-3 1201-1486; sin contenido, no acredita nada), 0 degradadas. Son el paquete entero, A-3 entera salvo la línea 1487 (de más de 2 000
  caracteres; la evita a propósito) y `a3-guards-result.json` 1-80 y 3636-3656. Salen de la transcripción del subagente que preparó el kit; su
  SHA-256 es el de la instantánea medida, porque la transcripción sigue creciendo. La lectura fallida motivó la recomendación del prompt de leer
  el objeto por tramos de 150 líneas.
- **Clon y run** (`kit/clone-verification.json`, generado por `kit/verify_clone.py`), todo comprobado:
  - HEAD = el recibo; solo `main`; sin remotos; limpio, también sin ignorados; ninguna etiqueta fuera de la historia de HEAD;
  - 29 blobs designados (los 27 del cierre, el paquete de A-2 y el registro de caracterización); `91e29886`, `76b48e78`, `b0884216`, `322cf8a8`,
    `bb0d5522`, `d2d74c14` y `3bbaabef` presentes; `4c617e82`, `b64a3b64` y `3ef58efa` ausentes;
  - 0 enlaces o uniones y 0 entradas 120000; `core.autocrlf` = false;
  - run con exactamente los cuatro archivos, idénticos al kit y al cierre; `order-s56.txt` y `order-s57.txt` con el SHA-256 y los bytes que
    declaran decisiones §56 y §57 en el commit;
  - el registro de caracterización del clon con el SHA-256 de la compuerta;
  - sin directorio de proyecto de `claude-cli` para el clon o el run ni transcripción con el session id;
  - preflight de las guardas, evidencia del invocador que el revisor no ejecuta: `a3-guards.py self-test --a3 docs/initiatives/I-62-A-3.md` PASS,
    116/116 vectores y 105/105 mutantes, con una salida idéntica entera a `a3-selftest.json` custodiado; el clon queda limpio después.
- **Preflight en seco** (`launch-dry-run.json`, junto a este README, fuera de `kit/`): `launch.py --dry-run` sobre el kit del intento 2: ninguna comprobación
  bloqueante; 1-3 y 6-10 en Ok; la 4 (`--version`) y la 5 (`auth status`) no se ejecutan en seco, porque quien prepara el kit no ejecuta el binario.
  La línea de órdenes mide 13 013 caracteres.
- **Manifiesto:** `kit/kit-manifest.json` (generado por `kit/make_manifest.py`), con el SHA-256 de cada archivo del kit (21) y de los cuatro
  archivos del run (SHA-256 del manifiesto `defda283ba13003cefb76d144f12bf8fecdd190082304350aa5779b5ac77d7c6`, intento 2, con la compuerta en
  MEASURED). El manifiesto del intento 1 era `7d5dd52e…`.

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar. Este kit no cambia el contrato de ninguna revisión anterior.

## Desviaciones del intento 2 frente al kit del intento 1, con su motivo

- **Espera de 120 s entre `auth status` y el lanzamiento** (`AUTH_SETTLE_S`, también en `Transport.AuthSettleSeconds` del cierre y en la
  comprobación 2). Motivo: el intento 1 falló con «Failed to refresh OAuth token» justo después de la comprobación 5; la espera deja terminar una
  renovación que esa llamada haya iniciado. La medición del binario sigue haciéndose justo antes de lanzarlo, después de la espera.
- **Registro de `TRANSPORT_AUTH_REFRESH_CONFLICT` en `run.json`** (`TransportFailure`): solo clasifica, no reintenta. Comprobado con la salida real
  del intento 1 (da `TRANSPORT_AUTH_REFRESH_CONFLICT`, 0 tokens) y con tres casos sintéticos (un result correcto, el mismo texto con tokens y sin
  result: los tres dan null). El auditor no cambia: una corrida así sigue sin acreditar.
- **Identidades:** `InvocationId` nuevo con el mismo esquema y el mismo sufijo (`I20261009T052403Z-58a1`, así el patrón del esquema no cambia),
  `AttemptSeq` = 2 (el esquema del resultado pasa a `enum [2]` y el selftest toma el valor del cierre) y un session id nuevo; `RunId` y
  `LogicalReviewRequestId` se conservan. El cierre registra el intento 1 en `PreviousAttempts`, y el manifiesto lo copia en su cabecera.
- **Prompt:** cambian el `InvocationId`, el `AttemptSeq` y la primera línea, que dice de forma neutral que el intento 1 terminó antes de cualquier
  turno del modelo, sin lecturas ni resultado. Nada más.
- **`launch/` del intento 1 movido, no borrado:** la orden era quitarlo del run tras copiarlo; se mueve intacto a `attempt-1/launch-raw/` (además de
  la copia saneada en `attempt-1/launch/`), para no perder los bytes cuyos SHA-256 registra `run.json`. El run queda igual: solo sus cuatro
  archivos.
- **Regenerados:** `closure.json`, `transport-gate.json` (embebe el session id), `clone-verification.json` (AllChecks), `selftest-result.json`
  (107/107), `kit-manifest.json` y `launch-dry-run.json`. Sin cambio: `post-review.py`, `transport_gate.py`, `mutation-result.json`,
  `preflight-read-fidelity.json`, `c3_analysis.py`, `read_fidelity.py`, `mutation-post-review.py` y los tres textos de las órdenes.
- **Posible factor no medido:** el proceso hereda el entorno de la sesión principal (como en C1 y en la corrida de A-2), incluidas variables de la
  integración con el host como `CLAUDE_CODE_SDK_HAS_HOST_AUTH_REFRESH` (solo se registran los nombres). El kit no lo cambia: cambiar el entorno
  sería otra desviación del transporte medido.

## Desviaciones frente al kit de A-2 (R20261008T184326Z-f1e2), con su motivo

- **Objeto nuevo, sin revisión formal anterior.** No hay delta ni objeto anterior, y no hay hallazgos formales anteriores: desaparecen
  `PriorFindingDispositions`, `IfAgreed.PriorRequiredClosed` y las cinco reglas de coherencia sobre hallazgos anteriores (cobertura de
  `PriorFindingDispositions`, id reutilizado sin STILL_OPEN, `LinkedFindingIds` inexistentes, STILL_OPEN sin hallazgo vinculado y A62-A2-01 sin
  REQUIRED). Las rondas adversariales de A-3 §12 son internas y
  el prompt lo dice.
- **Archivos del run.** En lugar de `delta.diff` y del objeto anterior, los textos fijos de las órdenes §56 y §57 (`order-s56.txt` y
  `order-s57.txt`). Motivo: el paquete §5 exige «el texto fijo de las órdenes (§56 y §57) … custodiados antes del lanzamiento», y A-3 y el paquete
  citan «orden, punto n» con la numeración del texto pegado de §56 (SHA-256 `e8b00328…`), que el archivo de decisiones resume en una sola fila
  (11-17); sin ese texto el revisor no puede comprobar esas citas (p. ej., Q-A3-03 sobre los puntos 13 y 15). Su identidad la comprueban
  `make_closure.py` y `verify_clone.py` contra el SHA-256 y los bytes que declaran decisiones §56 y §57 en el commit. `order.txt` es el cuerpo de la
  disposición §58 (la autoridad de esta revisión), que no está en el clon porque decisiones §58 se registró después del recibo. El resultado de las
  guardas no necesita copia en el run: está en el clon (`a3-guards-result.json`, en el recibo).
- **Compuerta D-01 entregada en MEASURED.** El kit de A-2 se entregó en PENDING y la sesión principal ejecutó después la sonda C3. Aquí la vía (a)
  ya está medida: la compuerta se fija sobre el registro existente (`20d0de2c…`) con `transport_gate.py set MEASURED`, que valida con los mismos
  predicados que `launch.py` y el auditor. `probe-c3.py` no se incluye (en el kit de A-3 no se ejecuta ninguna sonda); su analizador puro, que
  produjo `Probes.C3`, se extrae sin cambios a `c3_analysis.py` (`norm`, `text_of`, `analyze` y `DENIAL_RX` literales) y solo lo usa el selftest.
- **Esquema.** Campos nuevos: `ReviewedAnnexBlob` (el paquete designa el objeto y el anexo por blob), `ScopeConfirmation` (las confirmaciones del
  paquete §1 que no son de «sin cambio»). `NoChangeConfirmation` pasa a los ocho apartados del paquete §1; `IdentityCheck`, a las seis
  comprobaciones del Paso 0 de A-3; `OwnerAuthorityCheck` añade `FingerprintAcceptance` y `ConfigurationValueReading` (A-3 §8); `Focus` llega a 8;
  `GuardsVerification` admite G5b; `IfAgreed` tiene seis campos propios de A-3. AGREED exige además `ScopeConfirmation` todo CONFIRMED.
- **Tope de 7 200 s** (antes 3 600 s). Motivo: el objeto pesa 200 993 bytes (A-2: 29 957) y el cierre es mucho mayor; la corrida de A-2 duró unos
  15 minutos. Un corte por tope consume la única invocación sin resultado. Volver a 3 600 s exige cambiar `TIMEOUT_S` en `launch.py` y en
  `make_closure.py`, regenerar el cierre y el manifiesto.
- **`launch.py --dry-run` no ejecuta el binario.** En el kit de A-2 la preflight en seco ejecutaba `claude.exe --version`. Quien prepara este kit no
  ejecuta `claude.exe`, así que en seco las comprobaciones 4 y 5 quedan sin ejecutar (`Ok` = null); el lanzamiento real las ejecuta como antes.
- **Preflight más estricta.** La 2 compara también `TimeoutSeconds`; la 8 exige que no exista ningún directorio de proyecto `D--r62-arch-a3*` (clon
  o run).
- **Clon.** Desde el worktree (el de A-2 se clonó desde el repositorio de trabajo, mismo almacén de objetos). La rama ya estaba en `3ef58efa`
  (decisiones §58, posterior al recibo): tras `checkout -B main 49288525`, `branch -D` y `remote remove`, `reflog expire --expire=now --all` y
  `gc --prune=now` dejan ese commit fuera del clon, y `verify_clone.py` lo comprueba ausente.
- **Verificación del clon.** La preflight de las guardas compara además la salida regenerada con `a3-selftest.json` custodiado (idéntica entera).
- **Blobs que cambiaron solo por añadido.** `owner-decision-packets.md`: el paquete §2 fija `cdb86114…`, que es el blob del commit de publicación;
  en el recibo es `f39c0eff…` porque `49288525` añadió la sección OD-2-MAT (líneas 340-346). El cierre usa el blob del recibo y comprueba el
  prefijo exacto; las líneas citadas (93, 182, 199 y 200) no cambian. La evidencia es un marcador del paquete y se resuelve en el recibo.
- **Prompt.** El Paso 0 se adapta a lo que el revisor puede leer en A-3; se añaden el recibo de publicación, la recomendación de leer el objeto por
  tramos de 150 líneas y el aviso de que una compactación del contexto deja la corrida sin acreditar.
- **Temporales.** En `.tmp/st` y `.tmp/ex1`, dentro de la carpeta del kit, y se borran al final (antes `.st` y `.ex1`). La unión de prueba del caso
  03 apunta a `D:\r62-arch-a2r`, que es solo destino de lectura.

## Supuestos no medidos (declarados antes del lanzamiento)

- La duración de la corrida frente al tope de 7 200 s, con un objeto siete veces mayor que el de A-2.
- La compactación automática del contexto en una corrida larga: ni C1, ni C2, ni la corrida de A-2 la observaron. Si ocurre, el auditor la cuenta
  como un segundo mensaje de usuario y la corrida queda NOT_ACCREDITED; el prompt lo avisa.
- El límite de salida de Read en `claude-cli` 2.1.293: en la preparación, una lectura de 286 líneas de A-3 §12 superó el límite de la herramienta
  de la sesión que preparó el kit; no está medido para `claude-cli`, y el prompt recomienda tramos de 150 líneas en el objeto.
- El esquema compacto mide 10 891 caracteres y el prompt 23 417 bytes; la corrida de A-2 validó un esquema del mismo tipo y un prompt de 18 642
  bytes con fidelidad exacta.
- `modelUsage` con un solo modelo en una corrida larga: se observó en la corrida de A-2 (unos 15 minutos); el auditor lo sigue tratando como motivo
  duro.

## Antes del lanzamiento (procedimiento de la sesión principal)

1. Confirmar la autoridad de consumo: el paquete §5 pide, para cada uso, la autorización explícita de consumo del Owner (decisiones §56, punto 6)
   y, si no consta para esta invocación, pedirla en la solicitud única. Las líneas del Owner que acompañan a §58 cubren B3, el bloque de Codex y
   OD-2-MAT, no esta revisión; la caracterización registra el consumo como cubierto por CLAUDE-CLI-I62 = A, que es como se lanzó la re-revisión
   de A-2. La decisión es de la sesión principal; el kit no la presume.
2. Opcional: `python -B kit/transport_gate.py check` (código 0) y `python -B kit/launch.py --dry-run` (todas las comprobaciones en Ok salvo la 4 y la
   5, que solo se ejecutan en el lanzamiento real).
3. Custodiar el kit del intento 2 (commit de custodia, sin versionar `attempt-1/launch-raw/` ni `attempt-1/project-dir/`) y lanzar una vez:
   `python -B kit/launch.py`. La espera de 120 s forma parte del lanzamiento. Si cambia cualquier archivo del kit, regenerar el manifiesto
   con `python -B kit/make_manifest.py` (o, solo para la compuerta, `--gate`) y volver a custodiar.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada: se corrige la causa y se custodia otra vez el kit.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json`, y los cuatro archivos del run frente a `RunFiles` del manifiesto (el auditor
  repite la custodia en `RunCustody`).
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a3\2e266dfe-a1ca-4372-acd9-924c980bf50a.jsonl" D:\r62-arch-a3-run\launch\stdout.jsonl D:\r62-arch-a3 D:\r62-arch-a3-run kit kit/result.schema.json audit.json output.json`.
- `output.json`: el `structured_output` del mensaje `result`, en UTF-8 con sangría 1. `audit.json`: la salida literal del auditor v5.1-a3.
- `runtime-evidence.json` recoge: de `launch/`, `run.json`, `preflight.json`, la copia de la compuerta y los SHA-256 de `stdout.jsonl` y
  `stderr.txt`; la transcripción, no versionada (ruta y SHA-256); el clon después de la corrida; y la integridad del kit y de los archivos del run.
- A-3 no se modifica después de lanzar la revisión, y ningún resultado se declara AGREED antes del veredicto del Coordinator (disposición §58,
  punto 3).
