# I-62 — Kit de la re-revisión formal de la enmienda A-2 corregida por `claude-cli` (R20261008T184326Z-f1e2)

```text
LogicalReviewRequestId: L20261008T184326Z-f1e2   InvocationId: I20261008T184326Z-f1e2   AttemptSeq: 1   RunId: R20261008T184326Z-f1e2
Autorización:   decisiones §56, puntos 6-9 (celda claude-cli medida y elegible como transporte del ARCHITECT de I-62, caracterización acotada del
                punto 7, elegibilidad del punto 8, preferencia por la CLI del punto 9) y la línea del Owner CLAUDE-CLI-I62 = A;
                UNA re-revisión (§56, punto 3), sin reintento automático
Objeto:         commit de publicación fd411b136f888ecf301f9edaa7fcc068da8dbf22 (solo añade el resultado de las guardas)
                commit de la corrección 3bbaabef0346d4146585dbf17ac49a1d3a495b0d
                docs/initiatives/I-62-A-2.md                              blob f1e1d6f08d3cf677500794c7d0019433a4dc7d4e
                docs/initiatives/I-62-architect-package-A-2.md            blob 5bd0fa609722d9c94aeab38b0d8b486b1d4f1606
                docs/automation/evidence/I-62-A2/a2-guards.py              blob b72d26ea6b30b5cfceaa8140a536a08086562b24
                docs/automation/evidence/I-62-A2/a2-guards-result.json     blob d8f4845f853fcba8fe79e468312d4845b1c2e76e
                objeto anterior d47f71b66ba86c31f857f0d8c2d2437636947af0 (commit b280f707); delta corregido b553608c..fd411b13
Freeze:         b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; V14 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                (los dos commits son anteriores al rebase y no están en el clon; V14 se identifica por su blob en el commit)
Clon:           D:\r62-arch-a2r (git clone --no-local -c core.autocrlf=false --single-branch; main = el commit; sin remoto; sin enlaces;
                kit/clone-verification.json, AllChecks)
Run:            D:\r62-arch-a2r-run, con cuatro archivos y nada más antes del lanzamiento:
                order.txt (cuerpo de decisiones §56, 8 562 bytes, e8b00328…), prompt.md (= kit/prompt.md, 18 642 bytes, 0d1d5a4d…),
                delta.diff (17 177 bytes, abc6db95…) y A-2.d47f71b6.md (25 567 bytes, 9dca403d…)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC»; claude-opus-5-5 con effort xhigh; herramientas Read, Grep y Glob (+ StructuredOutput);
                --safe-mode, sin MCP; --add-dir del run (D-01); session id 26a89860-a107-4135-9981-a46f95816d16; cwd = el clon; prompt por stdin;
                resultado por --json-schema
Compuerta D-01: kit/transport-gate.json en PENDING: launch.py SE NIEGA a lanzar hasta que la sesión principal ate la medición C3 de la lista
                exacta (vía a) o una aceptación explícita registrada (vía b); sin una de las dos, HUMAN_LAUNCH_REQUIRED (paquete §5)
Estado:         kit custodiado ANTES del lanzamiento (commit de custodia: pendiente); NO lanzado. Lo lanza la sesión principal con kit/launch.py, una vez
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1 sobre la transcripción, la salida stream-json y launch/)
```

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon):
  - canónicos: la tabla de insumos del paquete §2 en el commit de publicación (15 archivos). Cada uno lleva `Blob`, `Bytes`, `Lines`, `Sha256`,
    codificación y `LineRanges` calculados mecánicamente desde los encabezados y las filas (título exacto y fila única comprobados). Entran el registro
    y el `output.json` de la revisión anterior, decisiones §51-§56, evidencia §78-§83 y AUTOMATION_PLAN §16.8 y §16.11, como pide el paquete nuevo;
  - transitivos: ninguno (ver desviaciones);
  - run: los cuatro archivos, clase RUN, con `RunFileHashes`. `order.txt`, `delta.diff` y `A-2.d47f71b6.md` pueden ser premisa; `prompt.md` nunca;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate` (la plantilla exacta del lanzamiento, con `<add-dir>`, `<session-id>` y
    `<json-schema>` como marcadores), `AddDirStatus` y `Gate` (D-01).
- **Herramientas y lectura.** El revisor solo tiene Read, Grep y Glob, y StructuredOutput para el resultado. Los rangos orientan, no limitan: un
  archivo del cierre puede leerse entero o por otros rangos. Es una violación:
  - cualquier Read o Grep fuera de los archivos del cierre y del run;
  - un Grep sin `path`, sobre un directorio o con `glob` o `type`;
  - un Glob cuyo alcance (base y patrón, evaluados sobre el disco) o cuyas rutas devueltas contengan un archivo fuera del cierre;
  - cualquier otra herramienta, aunque el runtime la rechace;
  - toda denegación de permiso.
- **Prompt** (`kit/prompt.md` = `D:\r62-arch-a2r-run\prompt.md`, byte a byte; 18 642 bytes, 220 líneas, SHA-256
  `0d1d5a4d4522af2d97336f299afd9bfc1833980fee87a7ae65a5d7bb33739a21`). Es neutral: no trae veredicto esperado ni contexto privado de la sesión autora.
  No tiene salto de línea final, para que ningún recorte de stdin lo altere. Pide:
  - el Paso 0 con lo que el revisor puede comprobar con sus herramientas: `order.txt` entera, la línea `index` de `delta.diff` frente a los blobs
    declarados y la cabecera del paquete;
  - el veredicto del paquete §1, con la disposición de A62-A2-01 y A62-A2-O1..O3 (CLOSED | STILL_OPEN), `Focus` 1-5 (paquete §4), Q-A2-01..Q-A2-07,
    la decisión del Owner, `NoChangeConfirmation`, el modo y la identidad;
  - en `InjectedContextDeclaration`, los identificadores de cuenta que inyecte el runtime (correo, uuid de organización) solo por su tipo, sin su
    valor: `output.json` se versiona como evidencia.
- **Esquema** (`kit/result.schema.json`; se pasa compacto a `--json-schema`). Lleva el veredicto (AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
  | NOT_ACCREDITED) y estos campos:
  - `PriorFindingDispositions`, `RequiredFindings` y `OptionalFindings` con `PremiseRefs`;
  - `QuestionDispositions`, `Focus`, `NoChangeConfirmation`, `GuardsVerification` (solo lectura), `Materiality`, `OwnerAuthorityCheck` e `IfAgreed`;
  - `IdentityCheck` (base: el clon verificado por el invocador), `ReviewerDeclaredIdentity`, `InjectedContextDeclaration` y `KnownLimitations`.
  `NOT_ASSESSED` y `null` solo valen con NOT_ACCREDITED, y lo exige el auditor.
- **Compuerta de transporte D-01** (`kit/transport_gate.py` y `kit/transport-gate.json`; la comparten `launch.py`, el auditor y `probe-c3.py`):
  - `ArgsTemplate` = la plantilla de la lista que lanza `launch.py`; C1 midió esa plantilla sin `--add-dir`;
  - `PENDING` (estado entregado): `launch.py` se niega (preflight 10);
  - `MEASURED` (vía a): el registro de caracterización, con su SHA-256 fijado en la compuerta, tiene `Probes.C3` en PASS sobre la plantilla exacta
    (lectura dentro del directorio añadido entregada fiel y sin denegación; lectura exterior denegada y no entregada; binario, versión, `init`,
    modelo y effort), `Eligibility.MeasuredArgsTemplate` = la plantilla y `STALE` = false; su C1 midió la plantilla menos `--add-dir`;
  - `ACCEPTED` (vía b): además, un registro `rackcad-transport-acceptance/v1` de la sesión principal o del Coordinator (con su SHA-256 fijado) que
    acepta exactamente `--add-dir` sobre ese registro de caracterización, con `Authority`, `Rationale` y `At`;
  - `launch.py` copia los registros en `launch/` y el auditor vuelve a evaluar la compuerta sobre esas copias.
- **Auditor v5.1** (`kit/post-review.py`). Cada fallo es un motivo, y ACCREDITED exige cero motivos:
  - lanzamiento: `stdout.jsonl` dentro de `<run>\launch`; `run.json` con código de salida 0, sin terminación, el session id, el ejecutable MEDIDO
    (ruta resuelta y SHA-256 tomados justo antes de lanzarlo) igual al binario del cierre, los argumentos exactos, cwd = el clon, y `PromptSha256`,
    `StdoutSha256` y `Transcript.Sha256` iguales a `prompt.md`, al stdout y a la transcripción auditados;
  - preflight: `preflight.json` con las diez comprobaciones presentes y en Ok, el session id, el binario medido (ruta y SHA-256) igual al del
    cierre, la versión `2.1.293 (Claude Code)`, `loggedIn` = true y la compuerta igual a `kit/transport-gate.json`;
  - compuerta: `transport_gate.evaluate` sin problemas sobre las copias de `launch/`;
  - stream-json: `init` (session id, cwd, modelo, `permissionMode`, herramientas {Glob, Grep, Read, StructuredOutput}, sin MCP, versión) y `result`
    (`success`, session id, `structured_output` presente, `permission_denials` vacío, `modelUsage` solo del modelo pedido, sin subagentes);
  - transcripción:
    - una sola sesión, la fijada, que es también el nombre del archivo; cwd = el clon; versión 2.1.293; sin cadena lateral;
    - el primer mensaje de usuario es idéntico a `prompt.md` y no hay otro;
    - `claude-opus-5-5` y `xhigh` en cada mensaje del asistente;
  - herramientas, rutas y Glob como arriba (rutas efectivas con `realpath`, seguras frente a uniones);
  - fidelidad de cada Read (como v4); premisas encontradas en las líneas citadas y entregadas fielmente por Read;
  - el `structured_output` coincide con la última llamada a StructuredOutput y su adjunto, valida contra el esquema y es coherente: cobertura,
    reglas de AGREED, CHANGES REQUIRED, BLOCKED y NOT_ACCREDITED, un REQUIRED con invariante o contraejemplo, y ids anteriores solo para STILL_OPEN;
  - custodia del run: archivo del run = copia del kit = `RunFileHashes`; el directorio del run solo admite los cuatro archivos y `launch/`, y
    `launch/` solo los archivos que escribe `launch.py`;
  - clon después de la corrida: HEAD, rama única, sin remoto, limpio con ignorados, blobs designados y sin enlaces.
  Uso: `python -B kit/post-review.py <transcripción> <run>\launch\stdout.jsonl D:\r62-arch-a2r D:\r62-arch-a2r-run kit kit/result.schema.json
  <audit.json> [<output.json>]`. `run.json`, `preflight.json` y las copias de la compuerta se leen de `<run>\launch`.
- **Lanzador** (`kit/launch.py`; no lo ejecuta quien prepara el kit). Si falla cualquiera de estas comprobaciones, se niega a lanzar (código 2) y no
  escribe nada:
  1. integridad del kit frente a `kit-manifest.json`, sin archivos distintos ni sin listar;
  2. transporte del cierre, incluida `ArgsTemplate`;
  3. binario: ruta resuelta, SHA-256 y Authenticode;
  4. `--version` = 2.1.293;
  5. `auth status` con `loggedIn` (solo se conservan `loggedIn`, `authMethod` y `apiProvider`);
  6. clon con HEAD, limpio, blobs y sin enlaces;
  7. run con exactamente los cuatro archivos y sus hashes;
  8. sin transcripción previa con el session id ni directorio de proyecto para el clon;
  9. línea de órdenes de menos de 32 000 caracteres (mide 11 257);
  10. compuerta D-01 en MEASURED o ACCEPTED, con sus registros y sus SHA-256, y la plantilla de los argumentos = la de la compuerta.
  Después escribe `preflight.json` y las copias de la compuerta en `D:\r62-arch-a2r-run\launch\`, vuelve a medir el binario justo antes de lanzarlo
  (si cambió, aborta sin lanzar) y lanza la ruta resuelta con la lista exacta de argumentos, stdin = `prompt.md`, cwd = el clon y un tope de 3 600 s
  con terminación (`terminate` y, 60 s después, `kill`). Escribe `stdout.jsonl`, `stderr.txt` y `run.json` (ruta y SHA-256 medidos del
  ejecutable, PID, inicio y fin, código de salida, terminado o no, SHA-256 de stdout y de la transcripción, clon después). Hereda el entorno de la
  sesión principal, como en la caracterización (C1). `--dry-run` hace solo la preflight, sin `auth status`, sin escribir y sin lanzar.
- **Sonda C3** (`kit/probe-c3.py`; no la ejecuta quien prepara el kit). Es la vía (a): un proceso `claude-cli` con la plantilla exacta (flags de C1
  + `--add-dir D:\r62-c3-probe\add` + session id nuevo + un esquema propio), cwd = `D:\r62-cli-char` (nunca el clon de la re-revisión), un canario
  dentro del directorio añadido y otro fuera (`D:\r62-c3-probe\out`), tope de 300 s. Comprueba antes binario, versión y `loggedIn`, y se niega si
  el registro ya tiene C3 o si ya existen `D:\r62-c3-probe` o la carpeta `C3\` junto al registro. Registra `Probes.C3` en
  `claude-cli-characterization.json` y, solo si es PASS, `Eligibility.MeasuredArgsTemplate`. Su análisis (`analyze`) está probado con datos
  sintéticos en el selftest; el proceso en sí no se ha ejecutado.
- **Selftest previo** (`kit/selftest-result.json`): **107/107 PASS** sobre el clon y el run reales, que quedan limpios e idénticos.
  - 66 casos de auditoría: transcripciones sintéticas en el formato medido en C1, con la salida stream-json y los archivos de `launch/`. Cada caso
    audita contra una compuerta SINTÉTICA (MEASURED, construida con `probe-c3.analyze` sobre una sonda PASS sintética; ACCEPTED en el caso 41),
    porque la del kit está en PENDING. Son los 28 casos del kit verificado (00-27, con la base acreditada) y 38 nuevos (28-64 y 36b), un caso
    negativo para cada regla que no lo tenía: `run.json` ausente o con session id, ruta o SHA-256 del ejecutable, cwd y hashes de prompt, stdout
    y transcripción distintos; `preflight.json` ausente, con comprobaciones ausentes o sin Ok, binario, versión, `loggedIn`, compuerta o session id distintos; la compuerta
    en PENDING, con la copia alterada o ausente, y una aceptación inválida; `init` ausente o con cwd, modelo, versión o MCP distintos; `result` sin
    success, con otro session id o sin `structured_output`; transcripción sin mensajes del asistente o sin primer mensaje de usuario; Glob con `..`
    después de un comodín; una entrada ajena en el run y un archivo ajeno en `launch/`; stdout fuera de `launch/`; un blob designado cambiado; un
    resultado que no valida el esquema (campo adicional, Test-Json); y la coherencia: ids repetidos, `LinkedFindingIds` inexistentes, A62-A2-01
    STILL_OPEN ligado solo a un OPTIONAL, `Focus` incompleto, Q REQUIRED u OPTIONAL sin su hallazgo, NO_FINDING con ids, otro objeto revisado,
    BLOCKED sin `OwnerDecisionRequired` y otros ids de invocación;
  - 41 casos de la compuerta: MEASURED y ACCEPTED válidos, un caso por problema de `transport_gate.evaluate` y tres de `probe-c3.analyze` (PASS,
    lectura añadida denegada y lectura exterior entregada).
- **Mutación** (`kit/mutation-post-review.py` → `kit/mutation-result.json`): cada sitio de regla de `post-review.py` (96) y de `transport_gate.py`
  (36) se desactiva por turno en una copia y se ejecutan los 107 casos: **132/132 mutantes muertos** (cada regla desactivada hace fallar al menos un
  caso), con la base sin mutar en 107/107.
- **Fidelidad previa** (`kit/preflight-read-fidelity.json`): 5/5 lecturas fieles en el clon, sin filtro. Son el paquete y el objeto enteros, A-2 §2.3
  (142-170), A-2 §3.3 (212-239) y V14 D.3, totales y ancla (2519-2528). Salen de la transcripción del subagente que preparó el kit; su SHA-256 es el de
  la instantánea medida, porque la transcripción sigue creciendo.
- **Clon y run** (`kit/clone-verification.json`, generado por `kit/verify_clone.py`), todo comprobado:
  - HEAD = el commit; solo `main`; sin remotos; limpio, también sin ignorados;
  - los ocho blobs designados; `3bbaabef`, `b553608c`, `b280f707` y `c9f9419d` presentes, y `4c617e82` y `b64a3b64` ausentes;
  - 0 enlaces o uniones y 0 entradas 120000; `core.autocrlf` = false;
  - run con exactamente los cuatro archivos, idénticos al kit y al cierre; `delta.diff` y `A-2.d47f71b6.md` re-derivados del clon
    (`hash-object` = `d47f71b6`);
  - sin directorio de proyecto de `claude-cli` para el clon ni transcripción con el session id;
  - preflight de las guardas, evidencia del invocador que el revisor no ejecuta: `a2-guards.py self-test` PASS, 50/50 vectores y 11/11 mutantes; el
    clon queda limpio después.
- **Preflight en seco** (`launch-dry-run.json`, junto a este README, fuera de `kit/`): `launch.py --dry-run` sobre el kit entregado. Todas las
  comprobaciones pasan salvo la 10 (compuerta en PENDING), y la 5 (`auth status`) no se ejecuta en seco. La línea de órdenes mide 11 257
  caracteres.
- **Ensayo del desbloqueo** (en una copia desechable del kit, ya borrada, con evidencia sintética del selftest): `set MEASURED` y
  `set ACCEPTED`, cada uno seguido de `make_manifest.py --gate`, dejan `launch.py --dry-run` sin comprobaciones bloqueantes; `--gate` se niega si
  cambió otro archivo del kit, y `set` se niega con una aceptación que no corresponde al registro de caracterización.
- **Manifiesto:** `kit/kit-manifest.json` (generado por `kit/make_manifest.py`), con el SHA-256 de cada archivo del kit (21) y de los cuatro
  archivos del run (SHA-256 del manifiesto `bfb3481236994025dfe5d638cf911a185113b9a746db05c1e834ad7597e9d57e`, con la compuerta en PENDING;
  cambia con `--gate`).

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar. Este kit no cambia el contrato de ninguna revisión anterior.

## Desviaciones frente al kit anterior (R20261008T014941Z-68fe), con su motivo

- **D-01. `--add-dir D:\r62-arch-a2r-run`: un flag que C1 no midió, ahora atado por una compuerta mecánica.** Los archivos del run están fuera del
  cwd (el clon), y con `--permission-mode dontAsk` y `--permission-prompts none` todo lo que pediría permiso se deniega; sin el flag, las lecturas
  de `order.txt`, `delta.diff` y `A-2.d47f71b6.md` podrían acabar en `permission_denials`. La caracterización lista los flags del adapter entre
  los invalidadores, la orden §7-§8 exige STALE = false para la elegibilidad y el paquete §5 solo permite lanzar con una celda elegible. Por eso
  `launch.py` se niega a lanzar mientras `kit/transport-gate.json` esté en PENDING, y el kit se entrega así. La decisión sigue siendo de la sesión
  principal:
  - vía (a): ejecutar `kit/probe-c3.py` (la sonda acotada C3 de la lista exacta) y, si es PASS, fijar la compuerta en MEASURED;
  - vía (b): registrar una aceptación explícita (`rackcad-transport-acceptance/v1`, de la sesión principal o del Coordinator) y fijar la compuerta
    en ACCEPTED;
  - si no elige ninguna, HUMAN_LAUNCH_REQUIRED (paquete §5).
  Quitar el flag exigiría cambiar `launch.py` y `make_closure.py` (`ADD_DIR`), regenerar el cierre y la compuerta y volver a ejecutar el selftest,
  la mutación y el manifiesto.
- **Transporte.** Antes era `claude-desktop-session`, abierta por el Owner desde una tarjeta de tarea. Ahora es `claude-cli`, lanzada por la sesión
  principal con `launch.py` (decisiones §56, punto 9: no se pide otra sesión de escritorio).
- **Herramientas.** Solo Read, Grep y Glob. Desaparecen la tabla `AllowedActions` (ID-*, MD-1, NAV-1, RD-*, EX-*, OWN-*), las salidas propias y todo
  el analizador de órdenes del auditor. Con ellos desaparecen la clase de los dos falsos positivos (`python - <arg>` y `python -X utf8 -`) y la de la
  desviación real (scripts en línea que leían archivos del cierre) de decisiones §56, punto 1.
- **Identidad.** La verifican el invocador (`verify_clone.py`, `launch.py`) y el auditor después; el revisor ya no ejecuta git. En su Paso 0 contrasta
  lo que sí puede leer: la línea `index` de `delta.diff` y la cabecera del paquete. Desaparecen `PromptHashSeen`, `OrderHashSeen` y las comprobaciones
  de HEAD antes de leer.
- **Fidelidad del prompt.** El prompt llega por stdin. El auditor compara el primer mensaje de usuario de la transcripción con `prompt.md`, en lugar de
  exigir que el revisor lo lea (`PromptReadFaithful`). `OrderReadComplete` y la lectura completa del objeto y del paquete pasan a ser cobertura
  informativa.
- **Run.** Son cuatro archivos en lugar de dos: se añaden `delta.diff` (el delta que pide el paquete §2, que el revisor ya no puede calcular) y el
  objeto anterior, y los dos pueden ser premisa.
- **Cierre.**
  - Es la tabla del paquete nuevo §2 (15 canónicos): entran el registro y el `output.json` de la revisión anterior y decisiones §56; la evidencia
    llega hasta §83; AUTOMATION_PLAN pasa a canónico por secciones.
  - Sin transitivos: con `--safe-mode` el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1), así que la opción A de GAP-07 no se
    activa.
  - Ya no se escribe `corpus.json`; el recuento queda en `CorpusDistinctNonAscii`.
- **Esquema.**
  - Campos nuevos: `PriorFindingDispositions` y `IfAgreed.PriorRequiredClosed`.
  - Rediseñados:
    - `IdentityCheck`: base del invocador, más la línea `index`, la ruta del delta y la cabecera del paquete;
    - `GuardsVerification`: solo lectura, sin `SelfTestRun` ni vectores;
    - `Focus`: 1-5, porque el paquete nuevo §4 tiene cinco preguntas más las Q-A2.
  - Se retira `LegacyApplicability`, que el paquete nuevo §1 no pide.
  - Por compatibilidad con el validador de `--json-schema` y con la API, el esquema no usa `$schema`, `$defs`, `$ref`, `anyOf` ni `const`. La regla
    «invariante o contraejemplo» del REQUIRED pasa a la coherencia del auditor. El esquema compacto va en ASCII (`ensure_ascii`), así que la línea de
    órdenes es ASCII.
- **Auditor.**
  - Comprobaciones nuevas: lanzamiento, `preflight.json`, compuerta D-01, `init` y `result` de stream-json, sesión, cwd y versión de la
    transcripción, modelo y effort por mensaje, conjunto de herramientas, alcance de Glob, denegaciones, coherencia del `structured_output` con la
    transcripción, contenido del directorio del run y de `launch/`, y re-verificación del clon (blobs y uniones).
  - El ejecutable de `run.json` es el medido por `launch.py` justo antes de lanzarlo, no una constante del lanzador, y los SHA-256 de stdout,
    transcripción y prompt de `run.json` se contrastan con los archivos auditados.
  - El auditor recibe además la salida stream-json. Tiene costuras explícitas (`list_dir`, `clone_git`, `clone_reparse`, `validate_schema`, `GATE`,
    `LAUNCH_DIR`) para que el selftest simule un run, un `launch/` o un clon alterados sin tocarlos; la auditoría real no las cambia.
- **Selftest y mutación.**
  - Hay 107 casos (66 de auditoría y 41 de la compuerta), en formato CLI, y una prueba de mutación de cada sitio de regla.
  - Los temporales viven en `.st`, junto a `kit/` (y los de `verify_clone.py`, en `.ex1`), y se borran al final; `.st`, con una guarda contra
    enlaces.
  - La unión de prueba del clon apunta a `D:\r62-arch-a2`, que es solo destino de lectura.
- **Verificación del clon.** Re-deriva los archivos del run y comprueba que no hay sesión ni proyecto previos. La preflight de las guardas sigue como
  evidencia del invocador.

## Supuestos no medidos (declarados antes del lanzamiento)

- `--add-dir` (D-01): la lista con el flag no está medida mientras no exista C3, y la compuerta impide lanzarla sin C3 o sin una aceptación
  registrada. Tampoco está medido que, sin el flag, `dontAsk` deniegue las lecturas fuera del cwd; C3 lo mide para la lectura exterior.
- El esquema del resultado usa `enum`, `pattern`, `minLength`, `minimum`/`maximum`, `null` y objetos y arrays anidados; C1 midió un esquema plano.
- El prompt tiene 18 642 bytes y 220 líneas; C1 midió 493 bytes en una sola línea, con fidelidad exacta.
- La duración de la corrida frente al tope de 3 600 s. La corrida anterior duró unos 15 minutos.
- `modelUsage` con un solo modelo en una corrida larga. Solo C1 lo observó: un único modelo en una corrida de unos 7 segundos y 3 turnos. C2 se
  terminó sin mensaje `result`, así que su `stdout.jsonl` no trae `modelUsage`. El auditor lo trata como motivo duro: si una corrida de unos 15
  minutos añade otro modelo a `modelUsage`, la corrida queda NOT_ACCREDITED aunque todos los mensajes del asistente sean de `claude-opus-5-5`.
- `effort` en cada mensaje del asistente sí se observó en C1 (3/3) y en C2 (4/4), las dos de segundos.

## Antes del lanzamiento (procedimiento de la sesión principal)

1. Elegir la vía de D-01:
   - (a) `python -B kit/probe-c3.py <claude-cli-characterization.json>` y, si es PASS,
     `python -B kit/transport_gate.py set MEASURED <claude-cli-characterization.json>`;
   - (b) escribir el registro de aceptación y `python -B kit/transport_gate.py set ACCEPTED <caracterización> <aceptación>`.
   `set` valida la evidencia con los mismos predicados que `launch.py` y el auditor, y no escribe nada si no valida.
2. `python -B kit/make_manifest.py --gate`: actualiza solo la entrada de `transport-gate.json` y se niega si cambió cualquier otro archivo.
3. Opcional: `python -B kit/launch.py --dry-run` (todas las comprobaciones en Ok salvo la 5, que no se ejecuta en seco).
4. Custodiar el kit (commit de custodia) y lanzar una vez: `python -B kit/launch.py`.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada: se corrige la causa y se custodia otra vez el kit.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json`, y los cuatro archivos del run frente a `RunFiles` del manifiesto (el auditor
  repite la custodia en `RunCustody`).
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a2r\26a89860-a107-4135-9981-a46f95816d16.jsonl" D:\r62-arch-a2r-run\launch\stdout.jsonl D:\r62-arch-a2r D:\r62-arch-a2r-run kit kit/result.schema.json audit.json output.json`.
- `output.json`: el `structured_output` del mensaje `result`, en UTF-8 con sangría 1. `audit.json`: la salida literal del auditor v5.1.
- `runtime-evidence.json` recoge:
  - de `launch/`: `run.json`, `preflight.json`, las copias de la compuerta y los SHA-256 de `stdout.jsonl` y `stderr.txt`;
  - la transcripción, no versionada: ruta y SHA-256;
  - el clon después de la corrida;
  - la integridad del kit y de los archivos del run.

## Antes del lanzamiento (sesión principal, 2026-10-08)

- **Compuerta D-01 (`--add-dir`): MEASURED.** La sonda C3 de la caracterización da PASS con la plantilla exacta (registro `20d0de2c…`;
  [caracterización](../../I-62-claude-cli/R20261008T183600Z-char/README.md)). La primera ejecución de C3 se ejecutó, pero el analizador falló con una
  entrada `system/permission_denied` de stream-json (`message` en texto). El analizador de `probe-c3.py` se hizo defensivo y C3 se repitió una vez.
- **Auditor v5.1:** la línea que lee `message` de la transcripción se hizo defensiva (sin cambio de comportamiento: las transcripciones medidas no
  tienen `message` en texto). Autoprueba repetida después del cambio: **107/107**. La prueba de mutación es la de la construcción del kit (132/132,
  sobre el auditor sin esa guarda). Su repetición completa (132 mutantes × la autoprueba entera) se detuvo por duración y no altera
  `mutation-result.json`.
- **`launch.py --dry-run`:** 9 de las 10 comprobaciones previas en Ok (integridad, transporte del cierre, binario y firma, versión, clon, run, sesión
  nueva, línea de órdenes, compuerta); la 5 (autenticación) solo se ejecuta en el lanzamiento real; [launch-dry-run.json](launch-dry-run.json).
- **Lanzamiento:** lo hace la sesión principal con `launch.py`, una vez, después del commit que custodia este kit.

## Resultado custodiado (después de la corrida)

- `launch.py` lanzó la sesión `26a89860-a107-4135-9981-a46f95816d16` (PID 31200) de 2026-10-08T20:56:00.185611Z a 2026-10-08T21:10:32.828742Z. Salida 0, con mensaje `result`, sin
  terminación; 33 turnos; modelo `claude-opus-5-5`; ninguna denegación de permisos.
- `output.json`: el `structured_output` literal (SHA-256 `15694b657f9286882890dd596729d9ddb7469f00ba73b55d45c8dfaa8364d237`). **Veredicto: AGREED.**
  - A62-A2-01 y A62-A2-O1..O3: **CLOSED**.
  - Ningún REQUIRED; OPTIONAL A62-A2-O4..O6 (precisiones del modelo G5, de la declaración y de la trazabilidad de §6; ninguna exige corregir A-2 antes
    del acuerdo).
  - Q-A2-01..07 sin hallazgo; materialidad M-03 y M-04; ninguna decisión del Owner.
- `audit.json`: auditor v5.1 literal. **ACCREDITED**, 0 motivos en 32 llamadas (identidad, transporte, prompt fiel, custodia del run, herramientas y
  rutas, fidelidad, premisas, modelo y effort, clon limpio después).
- `runtime-evidence.json` y `launch/` (`run.json`, `preflight.json`, `transport-characterization.json`). El `stdout.jsonl` y la transcripción no se
  versionan; quedan su ruta y su SHA-256.

