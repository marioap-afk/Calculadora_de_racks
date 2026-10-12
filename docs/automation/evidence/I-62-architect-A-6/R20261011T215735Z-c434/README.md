# I-62 — Kit de la primera revisión formal de A-6 por un Architect independiente con `claude-cli` (R20261011T215735Z-c434)

```text
LogicalReviewRequestId: L20261011T215735Z-c434   InvocationId: I20261011T215735Z-c434   AttemptSeq: 1   RunId: R20261011T215735Z-c434
Autorización:   disposición del Coordinator §77, punto 4 (cuerpo pegado, 2 918 bytes, SHA-256 e23faecbacc7ae33976f211ee381020b88dd08f38067eaa938c5b54ec36c4537,
                el que declara decisiones §77 en 7b5ffac9): preparar el kit de la revisión independiente de A-6 y sus comprobaciones estáticas; la
                revisión formal solo se lanza cuando conste una disposición afirmativa del Owner sobre ADR-0046 #9 y se hayan verificado las
                autorizaciones de revisión vigentes. Owner CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura). UNA revisión, sin
                reintento automático
Consumo:        cubierto por CLAUDE-CLI-I62 = A, la misma base que las revisiones de A-2..A-5 y el dictamen de B-min
Objeto:         docs/initiatives/I-62-A-6.md, blob eac473c0d2e58cf1c4594f48a8816b3833af6509 (69 226 bytes, 748 líneas), PROPUESTA; publicado en
                7b7aba46 (padre 108d4ca1, sin A-6); guardas `run` PASS custodiadas en 0c817e1d (a6-guards-result-run.json). Primera revisión formal:
                sin hallazgos anteriores
Commit del clon: 7b5ffac9d9c04f90b9708a867089d32e885dc815 = HEAD de W al preparar (decisiones §77 registradas; contiene A-6 y la custodia de las
                guardas). Todo archivo del cierre queda fijado por su blob en ese commit (los directorios, por su árbol)
Paquete:        ninguno (el prompt hace de paquete)
Clon:           D:\r62-arch-a6 (git clone --no-local -c core.autocrlf=false --single-branch desde W; HEAD separado en 7b5ffac9; sin ramas locales; sin
                remoto; sin enlaces). Creado en el paso final
Run:            D:\r62-arch-a6-run, con exactamente dos archivos antes del lanzamiento: order.txt (= day3\s77_body.txt, 2 918 bytes, e23faecb…) y
                prompt.md (= kit/prompt.md, renderizado desde kit/prompt.template.md)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC» (re-medidos SIN ejecutar el binario el 2026-10-11T22:25:10Z: prep/binary-measurement-2026-10-11.json);
                claude-opus-5-5 con effort xhigh; herramientas Read, Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; session id
                35cd6b11-5a2b-415a-8e87-a3b68fb0817d (uuid4, nuevo); cwd = el clon; prompt por stdin; resultado por --json-schema; tope de 3 600 s;
                espera de 120 s entre `auth status` y el lanzamiento; ArgsTemplate idéntica a la de los kits de A-2..A-5 y de B-min
Compuerta D-01: MEASURED (vía a) sobre el registro de caracterización R20261008T183600Z-char (claude-cli-characterization.json, SHA-256
                20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c, blob 541956eb), fijada sobre la copia del clon D:\r62-arch-a6;
                transport_gate.evaluate sin problemas
Compuerta Owner: 11-OwnerGate (kit/owner_gate.py), NUEVA y BLOQUEANTE: hoy = false (la línea del Owner A6-ADR0046-9-FX02 no está registrada), como se
                espera. Ver «Compuerta del Owner» abajo
Estado:         preparado y terminado, NO lanzado. launch --dry-run: única comprobación bloqueante 11-OwnerGate (4 y 5 no se ejecutan en seco)
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1-a6 sobre la transcripción, la salida stream-json y launch/)
```

**Qué se pide (order.txt, punto 4, líneas 19-24).** La revisión formal de la A-6 exacta, con veredictos explícitos sobre los cuatro temas del punto 2
(líneas 10-14: Q-A6-01; Q-A6-02, Q-A6-06/07 y Q-A6-12, estos tres REQUIRED REVIEW TOPIC) y sobre los nueve deltas A6-1..A6-9; un REQUIRED impide el
acuerdo. La sesión principal añade: separabilidad, confirmación de M-01..M-08 frente a A-6 §5 (que coincide con el dictamen acreditado),
compatibilidad con A-1..A-5, superficies protegidas, ubicación de los nueve párrafos, efectos sobre D.5 y la OV, alcance de las dos líneas del Owner
(§7 a/b), comprobación de autoridad del Owner y `IfAgreed` según el modelo de A-5; declaración del contexto inyectado y de la independencia.

**Independencia del preparador.** Quien preparó este kit es un subagente de la sesión principal (la sesión que preparó A-6); leyó A-6 entera,
decisiones §73-§77 y la interfaz de las guardas de A-6 para reproducirlas, y no leyó el código de `bmin/tool/` ni de `bmin/accept/`. El prompt no
contiene veredicto esperado ni valoraciones de la preparación; la evidencia de I-62 (`I-62-evidence.md`), que sí las contiene, queda fuera del cierre.
El kit no ejecuta el paquete B-min.

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon; 65 archivos, 2 231 552 bytes; cada archivo con su blob fijado en
  7b5ffac9 y comprobado; rangos calculados desde los encabezados y las filas, y las líneas citadas comprobadas por su primera línea):
  - A-6 entera (objeto; mapa de secciones, tabla de los ocho puntos, tabla de identidad, M-01..M-08 de §5, líneas del Owner L679-L680, Q-A6-01,
    02, 06, 07 y 12 de §8);
  - `docs/automation/evidence/I-62-A6/` entero (12 archivos: preflight.md, a6-guards.py, a6-selftest.json, los dos resultados, los scripts de
    apoyo y `out/`); en a6-guards-result-run.json, Mode/Base/Head (L2-L4) y Result (L128); en a6-selftest.json, L872-L894;
  - el paquete B-min entero (30 archivos, árbol 3caa0dfa; para Q-A6-12 el revisor compara `tool/bmin_facts.py` y `accept/bmin_accept.py`);
  - el dictamen de materialidad: `output.json` (96c3efbc) y `README.md` (8ce105e0) de R20261011T162328Z-5c5e;
  - V14: §18 (orientación: OV-I62-03 L960, OD-5 L974), §20.2 (AUT-I1 1039-1047), §20.3, §20.3.1 (CanonicalInputs 1079; pasos 2-4 1088-1099;
    verificación 1126-1130), Anexo C (2330-2382; 2333-2334; C-24 2363), D.1, D.3 (Topología A 2433-2447; totales 2523), D.4 (resultados
    2534-2537; FX-02 PASS 2544), D.5 (2558-2560) y D.6 (2572-2573; ancla 2586-2587);
  - AUTOMATION_PLAN 16.4 (receta 541-543), 16.9 (tabla; precedencia 623-624; L646; INVALID_OUTPUT L653), 16.10, 16.11 (L676), 16.13-16.14
    (MV 902-908; defectos 910-918; punto efectivo 945-946) y 16.18-16.24 (16.19 1045-1052; P-10 L1093; 16.22 1204-1207; 16.23 L1221, L1240;
    16.24 1264, 1265-1267, 1271-1276, 1279-1280, 1282, 1287-1288, P-22..P-25);
  - ADR-0046 y ADR-0048 enteros (puntos 1-9 de cada Decisión);
  - LIFECYCLE §3 (M-01..M-08 83-90; UNKNOWN 78-79; OWNER-RESERVED 94), §5 (REQUIRED/OPTIONAL L166; veredictos L171) y §6;
  - A-1, A-2 y A-3 (cabecera como orientación; archivos enteros en el cierre), A-4 entera (L169, L192, L203-L207, L393) y A-5 entera (A5-1
    175-199; L216; L221);
  - agent-execution README §5, §8 (L184), §10, §14 y §15 (I2 L561; I6 565-568); `routing.md` y `adapters/codex-cli.md` enteros;
  - los esquemas input-closure.v1, role-invocation.v1, controller-verification.v2, delegation.v2 y worker-handoff (v1), enteros;
  - `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md`, entero;
  - decisiones §73-§77 enteras (1479-1562; §77 1548-1562: cuerpo pegado 1551, puntos 1-8 1555-1562);
  - run: `order.txt` (clase RUN; único archivo del run que puede ser referencia) y `prompt.md`, con `RunFileHashes`;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate`, `TimeoutSeconds`, `AuthSettleSeconds`, `Gate` y la sección `OwnerGate`.
  **Excluida a propósito: `docs/automation/evidence/I-62-evidence.md`** (la evidencia §140-§150, citada por A-6 §3.1 y §10): contiene los
  resúmenes y valoraciones de la sesión que preparó A-6 y este kit; el revisor juzga sin ese anclaje. Excluidos también: transcripciones y
  memoria; `probe_tools.py` y las referencias de las sondas (`reference.json`, `comparison.json`); la tarjeta y el paquete OWNER-RESERVED de
  ADR-0046 #9 y la solicitud de medición (`I-62-F6/requests/`); el resto del directorio del dictamen (`audit.json`, `kit/`, `launch/`); el mapa de
  cláusulas, WORKFLOW, `state/I-62.yml`, otros worktrees y clones y el fixture.
- **Herramientas y lectura** (como en B-min). Solo Read, Grep y Glob, y StructuredOutput. Es una violación: cualquier Read o Grep fuera del cierre y
  del run; un Grep sin `path`, sobre un directorio o con `glob` o `type`; un Glob cuyo alcance o cuyas rutas devueltas contengan un archivo fuera del
  cierre; otra herramienta; una denegación de permiso. Archivos de más de 60 000 bytes (13, A-6 incluida) por tramos de 300 líneas como máximo.
- **Prompt** (`kit/prompt.template.md` → `kit/prompt.md` = `D:\r62-arch-a6-run\prompt.md`, sin salto final; 43 655 bytes, 344 líneas). Neutral:
  sin veredicto esperado ni contexto privado. Pide:
  - el Paso 0: `order.txt` entera (puntos 1, 2 y 4); decisiones §77 (bytes y SHA-256 del cuerpo pegado; el blob en el punto 1; los temas y la
    revisión en los puntos 2 y 4); `a6-guards-result-run.json` entero (Mode RUN, Base 108d4ca1, Head 7b7aba46, Result PASS); `a6-selftest.json`
    L872-L894 (Result PASS, A6TextBlob = el blob); si algo no coincide, NOT_ACCREDITED;
  - la condición de lanzamiento, explicada al revisor: la línea del Owner es posterior al clon y no está en él; su ausencia no es un defecto de A-6;
  - los cuatro temas con el texto de §77, punto 2, y lo que cada uno exige; los nueve deltas; separabilidad; M-01..M-08; compatibilidad;
    superficies protegidas; ubicación; D.5 y OV; alcance de las dos líneas del Owner; autoridad del Owner; `IfAgreed`;
  - referencias exactas `ruta:línea` / `ruta:inicio-fin`, solo de líneas leídas con Read;
  - el modo de revisión, la relación con el autor y el contexto inyectado (los identificadores de cuenta, solo por su tipo).
- **Esquema** (`kit/result.schema.json`, `rackcad-a6-architect-review/0`; 13 264 caracteres ASCII en forma compacta; generado por
  `prep/gen_schema.py`): solo las palabras clave ya medidas con claude-cli 2.1.293 en los kits de A-5 y B-min (type, additionalProperties, required,
  properties, enum, pattern, minLength, minItems, items). Campos: `Schema`, ids, `RequestedRole` (ARCHITECT), `Action` (REVIEW_DESIGN),
  `ReviewedUnit`, `ReviewedCommit`/`ReviewedPath`/`ReviewedBlob`, `ReviewerMode`, `SamePersonAsAuthor`, `ReviewerDeclaredIdentity`,
  `InjectedContextDeclaration`, `Step0` (siete booleanas + `Basis` INVOKER_VERIFIED_CLONE + `Note`), `Reads`, `Verdict` (AGREED | CHANGES REQUIRED |
  BLOCKED — OWNER DECISION | NOT_ACCREDITED), `RequiredFindings` (`A62-A6-R<n>`, `Section`, `ViolatedInvariant`, `Counterexample`, `Evidence`
  ≥ 1 referencia, `WhyItMatters`, `CorrectionRequired`), `OptionalFindings` (`A62-A6-O<n>`, `Section`, `Note`, `Evidence` ≥ 1, `SuggestedCorrection`),
  `TopicVerdicts` (Q-A6-01 | Q-A6-02 | Q-A6-06/07 | Q-A6-12; SATISFIED | NOT_SATISFIED | NEEDS_OWNER; `Rationale`, `ClauseRefs`, `FindingIds`),
  `DeltaVerdicts` (A6-1..A6-9; AGREED | CHANGES REQUIRED; `Rationale`, `ClauseRefs`, `FindingIds`), `Separability` (tres afirmaciones de A-6 §9),
  `MaterialityConfirmation` (M-01..M-08: `A6Declared`, `ReviewerAssessment`, `Agreement`), `CompatibilityA1toA5`, `NoChangeConfirmation` (ocho
  superficies), `ScopeConfirmation` (cuatro afirmaciones de ubicación), `D5AndOwnerValidation`, `OwnerDecisionsScope` (líneas a y b),
  `OwnerAuthorityCheck`, `IfAgreed` (ocho campos), `KnownLimitations`, `RecommendedNextAction`.
- **Compuerta de transporte D-01** (`kit/transport_gate.py` y `kit/c3_analysis.py`, byte a byte los de los kits de A-4, A-5 y B-min).
- **Compuerta del Owner** (`kit/owner_gate.py`, comprobación 11 de `launch.py`, BLOQUEANTE; se evalúa también en seco porque solo lee git). En la HEAD
  del worktree de I-62 en el momento del lanzamiento (rev-parse, show, hash-object sin -w y merge-base, con GIT_OPTIONAL_LOCKS=0; nunca escribe):
  (a) decisiones contiene como **línea completa**, byte a byte, la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de A-6 §7 (a) (A-6 L679 = tarjeta del
  Owner L22; SHA-256 de la línea, UTF-8 sin salto, `83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14`, verificado), en una línea
  posterior al encabezado de §77, y ninguna otra línea empieza por «A6-ADR0046-9-FX02 =»; (b) la línea del Owner `CLAUDE-CLI-I62 = A (…)` (decisiones
  L1085, SHA-256 `fbeade41f74d6694a621fe6294d9568d7b76b124b1fd78e415514582c5d6ee79`) existe como línea completa, ninguna otra línea empieza por
  «CLAUDE-CLI-I62 =», y el SHA-256 medido del binario = el del cierre; además, decisiones de la HEAD = el del árbol de trabajo (sin cambios sin
  commit), la HEAD desciende de 7b5ffac9 y el texto de decisiones de 7b5ffac9 es prefijo exacto del de la HEAD (§77 y lo anterior, intactos). No
  vale una paráfrasis, un fragmento citado (tabla, comillas, «> »), espacios o CR añadidos, un puntero a la tarjeta, ni la línea de la tarjeta: la
  compuerta solo lee el archivo de decisiones. Si no pasa, `launch.py` no ejecuta el binario (ni `--version` ni `auth status`).
- **Auditor v5.1-a6** (`kit/post-review.py`, derivado de v5.1-bmin con `prep/adapt_post_review.py`: las reglas de lanzamiento, compuerta, stream-json,
  transcripción, herramientas, lectura, fidelidad, custodia del run y clon quedan byte a byte). Cada fallo es un motivo y ACCREDITED exige cero
  motivos. Nuevas:
  - preflight: 11-OwnerGate presente, en Ok, con las dos líneas fijadas encontradas y sin problemas;
  - referencias: cada `Evidence` y cada `ClauseRefs` resuelve a líneas de un archivo del cierre (ruta relativa al clon) o de `order.txt`, todas
    entregadas fielmente por Read; `prompt.md` nunca (la rama de extractos del auditor de B-min se retira: esta revisión no tiene `Excerpt`);
  - siempre: ids de hallazgo únicos; un REQUIRED con `ViolatedInvariant` o `Counterexample`; todo `FindingIds` existe; tema NOT_SATISFIED ⇒ un
    hallazgo vinculado (un REQUIRED en Q-A6-02, Q-A6-06/07 y Q-A6-12); tema SATISFIED ⇒ ningún REQUIRED vinculado; NEEDS_OWNER ⇒
    `OwnerDecisionRequired` = true; delta CHANGES REQUIRED ⇒ un REQUIRED vinculado, AGREED ⇒ ninguno; `Separability` NOT_CONFIRMED ⇒ un hallazgo;
    M-01..M-08: `A6Declared` = A-6 §5, `Agreement` coherente, DISAGREE ⇒ un hallazgo; D.5 COVER_IN_A6 ⇒ un REQUIRED, OWNER_DECISION_REQUIRED ⇒
    `OwnerDecisionRequired`; `OwnerDecisionRequired` = true ⇒ consecuencias nombradas; ids de la invocación = los del cierre;
  - con un veredicto: las siete booleanas de `Step0` en true; objeto = commit, ruta y blob designados; los cuatro temas, los nueve deltas y M-01..M-08
    una vez cada uno; `ClauseRefs` en cada tema, delta y fila de materialidad, en `Separability`, `D5AndOwnerValidation` y `OwnerDecisionsScope`;
    ningún NOT_DETERMINED; `OwnerDecisionRequired` no nulo; todo REQUIRED citado por un delta o un tema;
  - AGREED ⇒ cero REQUIRED, los nueve deltas AGREED, los cuatro temas SATISFIED, `OwnerDecisionRequired` = false, D.5 EXCLUSION_CORRECT, A-1..A-5
    COMPATIBLE, `NoChangeConfirmation` todo NO_CHANGE, `ScopeConfirmation` todo CONFIRMED, las dos líneas del Owner LIMITED_TO_STATED_SCOPE con
    los otros dos campos CONFIRMED, `IfAgreed` todo true; `IfAgreed` nulo en los demás casos; CHANGES REQUIRED ⇒ un REQUIRED; BLOCKED — OWNER
    DECISION ⇒ `OwnerDecisionRequired` = true; NOT_ACCREDITED ⇒ `KnownLimitations` no vacía.
  Uso: `python -B kit/post-review.py <transcripción> D:\r62-arch-a6-run\launch\stdout.jsonl D:\r62-arch-a6 D:\r62-arch-a6-run kit
  kit/result.schema.json <audit.json> [<output.json>]`.
- **Lanzador** (`kit/launch.py`, derivado del de B-min con `prep/adapt_launch.py`: constantes, la comprobación 11 y no ejecutar el binario si la 11
  falla). Las diez comprobaciones de B-min sin cambio de predicados; exige `kit-params.json` con Provisional = false y `closure.ObjectBlob` = eac473c0.
  No reintenta. `--dry-run` no ejecuta el binario.
- **Comprobaciones estáticas de A-6 reproducidas** (`kit/verify_clone.py` → `kit/clone-verification.json`, AllChecks): el `a6-guards.py` del clon
  (blob c5cc6168), con `--repo` = W en solo lectura: `run --base 108d4ca1 --head 7b7aba46` da una salida idéntica entera a
  `a6-guards-result-run.json` (Result PASS, G1-G8 y G5b PASS) y `self-test --base 108d4ca1 --a6 <A-6 del clon>` una idéntica entera a
  `a6-selftest.json` (83/83 mutantes muertos, ToolBlob c5cc6168, A6TextBlob eac473c0). W sin cambios (HEAD igual antes y después).
- **Selftest definitivo** (`kit/selftest-result.json`, sobre D:\r62-arch-a6 y D:\r62-arch-a6-run): **169/169 PASS** (110 casos de auditoría, 41 de la compuerta de transporte y 18 de la compuerta del Owner), con el clon y el run limpios e idénticos al terminar; 681 s (2026-10-11T22:41:13Z-22:52:34Z). Casos 00-55: los del kit de B-min adaptados (base = un AGREED coherente; referencias en hallazgos y `ClauseRefs` en lugar de `ApplicableClauses`; el Paso 0 de A-6); 56-105: temas (REQUIRED REVIEW TOPIC sin REQUIRED, Q-A6-01 sin hallazgo, SATISFIED con REQUIRED, NEEDS_OWNER, ausente, repetido), deltas (ausente, repetido, CHANGES REQUIRED sin REQUIRED, AGREED con REQUIRED), hallazgos (REQUIRED sin disposición, id inexistente, repetido, sin invariante ni contraejemplo, sin evidencia, evidencia fuera del cierre o no leída), materialidad (fila ausente, A6Declared distinto de A-6 §5, acuerdo incoherente, DISAGREE sin hallazgo, sin referencias), separabilidad, D.5, decisión del Owner, BLOCKED, IfAgreed, AGREED con cada confirmación en negativo, NOT_DETERMINED y null con veredicto, Paso 0, objeto, ids, referencias ausentes, mal formadas o absolutas, id mal formado (esquema); tres positivos (CHANGES REQUIRED, BLOCKED y AGREED con un OPTIONAL coherentes); 106-108: registro de 11-OwnerGate en preflight.json (ausente, otra línea, fallida); O00-O17: la compuerta del Owner sobre textos sintéticos (la línea exacta tras §77 aceptada; sin la línea, paráfrasis, fragmento en tabla, «> », espacio o CR final, antes de §77, puntero a la tarjeta, revocación, sin CLAUDE-CLI-I62 u otra línea CLAUDE-CLI-I62, §77 cambiada, cambios sin commit, HEAD que no desciende, otro binario, línea fijada distinta de A-6 L679, dos encabezados §77, rechazados). SHA-256 `aacd4d5f…`.
- **Mutación definitiva** (`kit/mutation-result.json`): cada sitio de regla de `post-review.py` (107), de `transport_gate.py` (36) y de `owner_gate.py` (11) desactivado por turno y los 169 casos contra cada mutante: **154/154 mutantes muertos**, base 169/169; 3 052 s (22:52:34Z-23:43:26Z). Sobre los archivos finales (`post-review.py` `3cd059df…`, `transport_gate.py` `b05fedd1…`, `owner_gate.py` `ca4b2ebe…`). SHA-256 `073a3f1d…`.

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar.

## Lanzamiento (cuando el Owner haya dado la línea)

1. **Registro de la línea del Owner** (sesión principal; solo si el Owner da la alternativa A): copiar la línea **byte a byte desde la tarjeta**
   (`docs/automation/evidence/I-62-F6/requests/A6-ADR0046-9-FX02-owner-card-2026-10-11.md`, línea 22) a `docs/automation/decisions/I-62.md`,
   como **línea completa** (sin sangría, sin «> », sin comillas, sin espacio final; dentro de un bloque ```text vale, como §56), **después de §77**
   y solo por añadido; y hacer commit en la rama. Nada del kit cambia por ello.
2. **Rehacer la preflight en seco** (la compuerta se evalúa en vivo):
   `python -B "%TEMP%\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad\i62\day3\kit-a6-review\kit\launch.py" --dry-run`
   → `BlockingChecks` = [] (11-OwnerGate en Ok, con `A6LineNumbers` posterior a §77) y `NotRunInDryRun` = 4 y 5. Si W ya no desciende de 7b5ffac9,
   si decisiones cambió algo anterior al añadido, si hay cambios sin commit en decisiones, o si el binario cambió, la 11 (o la 3/10) lo dice y no se
   lanza.
3. **Custodia del kit** (procedimiento de la sesión principal, como en B-min) antes del lanzamiento. Si cambia cualquier archivo de `kit/`,
   `python -B kit/make_manifest.py` (o `--gate` tras un `transport_gate.py set`) y volver a custodiar.
4. **Lanzar una vez**:
   `python -B "%TEMP%\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad\i62\day3\kit-a6-review\kit\launch.py"`
   `launch.py` vuelve a evaluar las once comprobaciones (incluidas la 10 y la 11) inmediatamente antes, espera 120 s tras `auth status`, vuelve a
   medir el binario justo antes de lanzarlo y no reintenta.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json` y los dos archivos del run frente a `RunFiles`.
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a6\35cd6b11-5a2b-415a-8e87-a3b68fb0817d.jsonl" D:\r62-arch-a6-run\launch\stdout.jsonl D:\r62-arch-a6 D:\r62-arch-a6-run kit kit/result.schema.json audit.json output.json`.
- El veredicto no es el acuerdo: el acuerdo es del Architect y del Coordinator (LIFECYCLE §6) sobre el blob exacto, con un REQUIRED abierto ⇒ sin
  acuerdo (§77, punto 4). Ni A-6 AGREED, ni FX-02 PASS, ni F6 GATE PASS se declaran aquí.

## Desviaciones frente a los kits de B-min (R20261011T162328Z-5c5e) y de A-5 (R20261011T065226Z-dbb0), y frente a la orden, con su motivo

- **Compuerta del Owner (comprobación 11) nueva**, exigida por §77, punto 4, y por la orden de la sesión principal; evaluada antes de las
  comprobaciones 4 y 5, para que sin la línea del Owner el binario no se ejecute. En seco es la única bloqueante, como se espera hoy.
- **Clon en 7b5ffac9 (HEAD de W), no en un commit posterior a la línea del Owner:** el objeto, la custodia de las guardas y §77 están en él; la línea
  del Owner se registrará después y la compuerta la busca en el worktree en el momento del lanzamiento. El prompt se lo explica al revisor.
- **Objeto = un archivo; revisión formal de una enmienda** (forma de A-5), con referencias `ruta:línea` (forma de B-min) en lugar de `PremiseRefs`
  con cita; sin `Excerpt`, así que el auditor retira la rama del extracto no hallado (si no, sería código muerto que la mutación no mataría).
- **Hallazgos con `Section`, `Evidence` y corrección** (orden de la sesión principal); ids `A62-A6-R<n>` y `A62-A6-O<n>`.
- **Verdict con NOT_ACCREDITED** además de los tres valores de LIFECYCLE §5: solo la vía de parada del Paso 0, para no fabricar un veredicto.
- **Reglas de coherencia añadidas** a las de la orden, todas declaradas en el prompt: tema SATISFIED sin REQUIRED vinculado; NOT_SATISFIED de
  Q-A6-01 con al menos un hallazgo; NEEDS_OWNER y D.5 OWNER_DECISION_REQUIRED ⇒ `OwnerDecisionRequired`; delta AGREED sin REQUIRED vinculado; todo
  REQUIRED citado por un delta o un tema; DISAGREE de materialidad y NOT_CONFIRMED de separabilidad con un hallazgo; con AGREED, también las
  confirmaciones de compatibilidad, superficies, ubicación y líneas del Owner en positivo (modelo de A-5).
- **Rangos de orientación añadidos en archivos del cierre** (no cambian el conjunto de archivos): V14 §18 (matriz OV y OD-5, que A-6 §2 y §3.15
  citan; necesarios para el juicio sobre D.5/OV) y LIFECYCLE §5 (define REQUIRED/OPTIONAL y los veredictos).
- **`I-62-A6/` entero**, incluidos los scripts de apoyo y `out/` (la orden nombra preflight, guardas, autoprueba y resultados; el resto es su
  soporte custodiado, sin valoraciones).
- **Comprobaciones estáticas de A-6 reproducidas por el invocador** (patrón del kit de A-5; el de B-min no tenía guardas): con `--base 108d4ca1` en
  la autoprueba, la salida es idéntica entera a la custodiada (también `Repo`).
- **Sin `preflight-read-fidelity.json`** (como en B-min): no se leen transcripciones; la fidelidad la mide el auditor sobre cada Read del revisor.
- **Esquema sin `gate-contract.v2`** en el cierre (B-min lo tenía porque su SPEC lo leía; la orden de esta revisión no lo nombra).
- **`finish_kit.py` no se ejecutó de una vez:** el paso final se hizo con sus mismos comandos, uno a uno (`finish-kit.log`), porque el selftest se depuró entre el paso 6 y el 7 (dos expectativas del propio selftest; ni el auditor, ni el cierre, ni el run cambiaron). Después de la mutación solo cambió `finish_kit.py`: captura la salida con `-X utf8` (la consola cp1252 de este host introducía U+FFFD en la preflight capturada); el manifiesto se rehízo y la preflight en seco se repitió.

## Supuestos no medidos (declarados antes del lanzamiento)

- La duración frente al tope de 3 600 s (el cierre pesa ≈ 2,23 MB; las revisiones de A-4, A-5 y B-min duraron unos 11-21 minutos). Para Q-A6-12 el
  revisor puede necesitar leer `bmin_accept.py` (2 816 líneas) y `bmin_facts.py` (1 614) por tramos de 300 líneas.
- La compactación automática del contexto (no observada en las corridas anteriores); el prompt avisa.
- Que el esquema (13 264 caracteres compactos, con la raya de «BLOCKED — OWNER DECISION» escapada como \u2014) se comporte con claude-cli como los
  de A-5 y B-min (mismas palabras clave).
- Que la espera de 120 s evite el conflicto de renovación del token.

## Puntos abiertos

1. **La línea del Owner no está registrada.** Hoy `launch --dry-run` da 11-OwnerGate = false (BLOQUEANTE), como se espera: la revisión no puede
   lanzarse hasta que el Owner dé `A6-ADR0046-9-FX02 = A` y la sesión principal la registre (arriba). Si el Owner elige B o C, la revisión no se
   lanza (§77, punto 4; tarjeta del Owner).
2. **Lo que A-6 cita fuera del cierre** no lo puede comprobar el revisor: la evidencia §140-§150 (excluida por anclaje), decisiones anteriores a §73
   (AGREED de A-1..A-4 en §43, §57, §61, §64), el registro del Freeze, el mapa de cláusulas y WORKFLOW. El prompt le pide declararlo en
   `KnownLimitations` si su juicio depende de ello.
3. **Fidelidad previa al lanzamiento** (AP 16.24, P-24): como en B-min, no hay archivo previo; si la sesión principal la quiere, el kit trae
   `read_fidelity.py` (tres Read en D:\r62-arch-a6 y el script sobre la transcripción de esa sesión).
4. **El kit no ejecuta el paquete B-min** ni sus pruebas; la independencia efectiva del aceptante (Q-A6-12) la juzga el revisor leyendo el código, las
   notas, las pruebas y los resultados custodiados.
5. **Movimiento de W.** La compuerta exige que W siga descendiendo de 7b5ffac9 y que decisiones crezca solo por añadido; un commit posterior de
   custodia o de registro no la rompe. Si decisiones se corrige de otra forma, la compuerta se niega y hace falta decidir (y, si procede, rehacer el
   kit sobre un clon nuevo).
6. **Etiquetas.** Si en el momento del lanzamiento existe una etiqueta en el clon sobre un commit posterior al del clon, `verify_clone.py`
   (comprobación previa) fallaría; el clon actual no tiene ninguna fuera de HEAD.

## Archivos

```text
SHA-256                                                           bytes  archivo
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7    6070  kit/c3_analysis.py               (= kits de A-5 y B-min)
51968cd89def85ba5af88640db64975eb0a8ec6e0ed915503a08f3a1e0acbaf3   21691  kit/clone-verification.json
ab01e50e4ef5074ab479f3d850e8bf1f806a1bd99b97486309bc24423c834358   91278  kit/closure.json
e5c3905dcc27167bea81cd81987d86efda9066f530243d27edc4edbb1015e058    5258  kit/finish_kit.py
629fa928c13d1bc9fafdb35bf6627e06fda20b2e9909c95d679461849b3c680c    5278  kit/kit-manifest.json            (manifiesto: manda)
3fc9bf67779309c09e23535694b4a395be424ee0113f6fb464ee95a3b8229ee0    1657  kit/kit-params.json
a2bde5ac597bf7a2dd254470e5bb70b51d311617c4e9b90ba49f570b448006cf   19313  kit/launch.py
f3a48f445ecead76262580af033f190b3813a1b8b98ccb86533816c3c88517c2   51702  kit/make_closure.py
760e18986e8fe225241dfe66630efb3bd88c8d09f71af060b69fd54b5952f0f0    5913  kit/make_manifest.py
b76f7d4e33df7c8d8f6de24669e2342ffd8e55de91706b8d14f0ba75a089956b    6311  kit/mutation-post-review.py
073a3f1db16c1e505e2ccb3cfa3136db30bdf217c542cf70f46de2326bf5b578   41184  kit/mutation-result.json
e23faecbacc7ae33976f211ee381020b88dd08f38067eaa938c5b54ec36c4537    2918  kit/order.txt                    (= day3\s77_body.txt)
ca4b2ebe84dd46e208e1356974d09073c5986e24ca7a7ab4dea5eb8314ec65df    8535  kit/owner_gate.py
3cd059df5f193a6f1f2cc450663fc3c370cc00bdcc71bde6d369a5e6bdcbcf42   56247  kit/post-review.py
3c645b1c55a021d2a4febe843ec464fae78ea218d6f9371eb957def7ae5547a0    5979  kit/prepare_clone.py
341468cc7c1f716e935843c01491f1b9a98d5e12623849afa7ad6746bb0e4487   43655  kit/prompt.md                    (= D:\r62-arch-a6-run\prompt.md)
4037d5eb1f9827eb46c7649fde5784e7bec0cc1985e1430e3324c9715f4e128d   27965  kit/prompt.template.md
7b51116e5e4876c1416066994e282ad077661547ee5fb1dcb0fbcbd59e413cce    4925  kit/read_fidelity.py
c54894b8eecb7c914d3d67b7493c2ceeb1227b9d514213846c029b6f2d98cd7d   20502  kit/result.schema.json
0763891caa95fbf68f4977ea61eb5018b7d733490530319ca125b7504587b916   79570  kit/selftest-post-review.py
aacd4d5fe7d0200a048fb4f7b28fe461c2b6fe1307a5c43d4bd78047c527da32   71262  kit/selftest-result.json
72dd91a6f4e0cc0c7eb3db90547baf525e12a5c38ed3af8719a926a1d30990d7    1600  kit/transport-gate.json
b05fedd17b9b6856d618d2ffcde88efc1ceafaa3e6c2d5373667919a718129c6   18828  kit/transport_gate.py            (= kits de A-4, A-5 y B-min)
92a23f9d327cd99a606a3fe5a7411c98cf696998a5b9920121f60a61f3a3c2e7   17596  kit/verify_clone.py
545f1e997472f7209c030dfa4567d8dd57358a79f088b049f30f7f2e004e5bed    2735  launch-dry-run.json              (junto a este README)
8d91950cb818cd96f9bd5f2cfca93c5f36b90f3288a55f4c7a91eec0ede1c8bb    6108  finish-kit.log                   (junto a este README)
```

Preparación (fuera de `kit/`, no se custodian como kit): `prep/gen_schema.py` (genera el esquema), `prep/adapt_launch.py`, `prep/adapt_post_review.py`
(+ `pr_head.txt`, `pr_coherence.txt`) y `prep/adapt_selftest.py` (+ `st_head.txt`, `st_reads.txt`, `st_cases.txt`, `st_tail.txt`), que derivan el
lanzador, el auditor y el selftest de los del kit de B-min por sustituciones exactas; `prep/binary-measurement-2026-10-11.json`; `prep/ids.txt`;
`prep/prompt-preview.md` (vista previa renderizada desde W antes del clon, igual al `prompt.md` final); salidas de los pasos (`prep/step*.out`).
`finish-kit.log` (traza de los pasos 1-10, ejecutados uno a uno con los comandos de `finish_kit.py`) y `launch-dry-run.json` (salida literal de `launch.py --dry-run`, capturada con `-X utf8`), junto a este README.

## Resultado (añadido tras la corrida, sesión principal)

```text
Sesión:          35cd6b11-5a2b-415a-8e87-a3b68fb0817d, de 2026-10-12T07:16:53Z a 2026-10-12T07:38:51Z, salida 0, fallo de transporte: None
Uso:             65 turnos; herramientas {"Read": 54, "Grep": 9, "StructuredOutput": 1}
Veredicto:       CHANGES REQUIRED
REQUIRED:        5 (A62-A6-R1, A62-A6-R2, A62-A6-R3, A62-A6-R4, A62-A6-R5)
OPTIONAL:        3
Temas:           Q-A6-01 SATISFIED; Q-A6-02 NOT_SATISFIED; Q-A6-06/07 NOT_SATISFIED; Q-A6-12 NOT_SATISFIED
Deltas:          A6-1 CHANGES REQUIRED; A6-2 AGREED; A6-3 CHANGES REQUIRED; A6-4 CHANGES REQUIRED; A6-5 CHANGES REQUIRED; A6-6 AGREED; A6-7 CHANGES REQUIRED; A6-8 CHANGES REQUIRED; A6-9 CHANGES REQUIRED
Acreditación:    NOT_ACCREDITED por el auditor v5.1-a6 (1 motivo); la acreditación final es del Coordinator
Clon tras corrida: HEAD 7b5ffac9, estado limpio
```

El veredicto no es el acuerdo: el acuerdo de A-6 es del Architect y del Coordinator sobre el blob exacto, y un REQUIRED abierto lo impide
(decisiones §77, punto 4). Ni A-6 AGREED, ni FX-02 PASS, ni F6 GATE PASS.

Archivos añadidos: `output.json`, `audit.json`, `runtime-evidence.json`, `launch/` (saneado), `launch-dry-run-s78.json` (la prueba en seco sobre
`519b3ddc`), `launch-run.log` y `post-review.log`. La transcripción y `stdout.jsonl` no se versionan; sus SHA-256 están en `runtime-evidence.json`.
