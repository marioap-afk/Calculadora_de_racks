# FX-06 — auditor del transporte: `OWNER_AS_MESSAGE_BUS` y AUTONOMY_GAP (SOLO SUPERVISIÓN)

```text
Programa:    owner_as_message_bus_audit.py v3.1.0 (Python 3, sin dependencias; salida determinista). Autoprueba: 45 casos sintéticos de auditoría + 19
             rechazos de entrada = 64/64 PASS (selftest-result.json; ningún dato de una corrida real). v3 = revisión R2 del kit (README §9); v3.1 =
             verificación de R2 (README §9.1).
Autoridades: V14 D.8 (PASS: OWNER_AS_MESSAGE_BUS = false en 1-7, sin relevo del Owner ni decisión intermedia del Coordinator; paso 1; fila FAIL), C-32,
             C-37, C-39 («OWNER_AS_MESSAGE_BUS = false en los pasos 1-7»), §20.1 (RELAY frente a ESCALATION y COORDINATOR_DECISION), §20.2 AUT-I1, §20.3.2
             (identidad observada en el relay-record/v2, referenciada como RuntimeEvidenceRef), §20.9 (AUTONOMY_GAP ≠ PASS), B.8.8 (launch_evidence,
             runtime_evidence, autonomy_gaps[]; I-P13), B.9 (último párrafo); AUTOMATION_PLAN 16.20 (P-20), 16.29 (P-21); README §18.7; F4:
             AutonomyGaps.OwnerAsMessageBus y AutonomyGaps.Missing (tests/RackCad.Tests/I62/MaterializationClose.cs), F4-OBS-22; decisiones del
             Coordinator §49 (fila FX-06), §50 (fila «Mensajes de la supervisión a A») y §54 («Bus de mensajes»).
Entrada:     la construye la supervisión DESPUÉS de la corrida (custodia del fixture + eventos humanos de la transcripción). La entrada contiene
             tokens solo de supervisión (p. ej., el SHA del oráculo): nunca se entrega a una sesión del fixture.
```

## 1. Qué decide y qué no

- Calcula `OWNER_AS_MESSAGE_BUS` ∈ {TRUE, FALSE, UNDETERMINED} para la ventana de los pasos 1-7, las entradas AUTONOMY_GAP (custodiadas y derivadas), los
  relevos manuales sin registro (P-21), las decisiones intermedias del Coordinator y `PassEligibleOnTransport`.
- **No** declara PASS: el PASS de FX-06 exige además los pasos 1-8 completos y lo decide el Coordinator (D.8; decisiones §46 «F6 GATE PASS nunca se
  autodeclara»). Tampoco afirma `OWNER_AS_MESSAGE_BUS` = false sin la corrida real (decisiones §54).
- Coincide con F4 en la custodia, aplicada a la secuencia de la ventana: TRUE si un AUTONOMY_GAP que cuenta en 1-7 (§4) tiene `RelayedBy` OWNER; un
  AUTONOMY_GAP del Coordinator es hueco de autonomía (C-37, criterio 15 no PASS) pero no hace al Owner el bus (F4-OBS-22). El valor de F4 sobre todos
  los puntos suministrados sale aparte, como información (`OwnerAsMessageBusF4RuleAllSuppliedPoints`).
- Añade, con fallo cerrado, lo que la custodia del estado sola no ve: los mensajes humanos a la sesión del Principal, las aperturas de otras sesiones,
  los commits de `fx/u1` o de `main` del fixture de un autor distinto del Principal dentro de la ventana y los marcadores del protocolo que un autor
  distinto del Coordinator escribe en el archivo de decisiones desde el commit de la RLA (§2, «Ámbito del archivo de decisiones»).

## 2. Ventana (por instantes)

- **Apertura:** el instante (`Utc`) del evento humano de arranque (`Window.KickoffEventSeq`; el `Seq` solo lo identifica). El arranque es un evento de
  la sesión del Principal (`Window.PrincipalSession`) y **anterior al push del paso 1** (`Window.Step1Commit`, el commit de X v1; D.8-1; README §2.1:
  «El último mensaje humano antes del paso 1 es el arranque»); si no, la entrada se rechaza. El push del paso 1 no puede ser posterior al primer punto
  con bucle activo. Su contenido debe ser un MESSAGE con el literal `continúa` o con la plantilla neutra de README §5 (por su SHA-256 en
  `Window.KickoffTemplateSha256`); si no, KICKOFF_NONSTANDARD con disposición (§3).
- **Cierre:** el `PushedUtc` del punto que custodia la ingestión de C (`Window.Step7RecordVersion`), incluido. `null` = el paso 7 no se alcanzó: la ventana
  queda abierta hasta el último evento y la corrida no es elegible.
- **Fase de cada evento humano y de cada punto**: solo por su instante. Antes de la apertura: PRE_WINDOW; en (apertura, cierre]: IN_WINDOW; después:
  POST_WINDOW. Un evento distinto del arranque, un punto o un commit de decisiones con el mismo instante que el arranque cuentan dentro (fallo cerrado);
  la RLA, en cambio, tiene que ser estrictamente anterior. Ningún punto con bucle activo puede ser anterior o simultáneo al arranque (§6).
- Antes del arranque: precondiciones (apertura de la sesión, designación del Coordinator, mensaje inicial). Después del paso 7: paso 8 (el Owner solo
  decide, si procede).
- **Ámbito del archivo de decisiones** (v3.1): desde el commit de la RLA vigente (el último commit del Coordinator con `I62-REVIEW-LOOP-AUTHORIZATION:`
  anterior al arranque; P5) o desde el primer evento de la sesión del Principal si es anterior; sin RLA, desde ese primer evento. Es más amplio que la
  ventana porque el Principal ya actúa antes del arranque (P8: preflight, propuesta de binding y QR) y el lector de F4 aceptaría un bloque suyo igual que
  uno de la ventana. El auditor lo publica en el resultado, en `Window.DecisionsFileScopeUtc` y `Window.DecisionsFileScopeBasis` (RLA_COMMIT o
  PRINCIPAL_SESSION_FIRST_EVENT).

## 3. Clasificación de los eventos humanos

| Evento | Fase | Clase | Efecto en `OWNER_AS_MESSAGE_BUS` | Disposición | Base |
|---|---|---|---|---|---|
| apertura o reapertura de la sesión del Principal (`Session` = `Window.PrincipalSession`) por el Owner | cualquiera | OWNER_OPENED_SESSION | ninguno | no (salvo tope superado: más de 2 aperturas de esa sesión, D.8) | §49 FX-06; §54 |
| apertura o reapertura de otra sesión por el Owner | antes o después de la ventana | OWNER_OPENED_OTHER_SESSION | ninguno; no cuenta para el tope del Principal | no | §54 («abrir una sesión asignada al Owner no es AUTONOMY_GAP») |
| apertura o reapertura de otra sesión por el Owner | ventana | OWNER_OPENED_OTHER_SESSION_IN_LOOP | UNDETERMINED | sí | [architect-invocation-contract.md](architect-invocation-contract.md) §2 (sesión nueva de `claude-desktop-session`: «la abre el Owner: un clic humano en 1-7 es un relevo manual»); D.8 paso 2 («lanza **automáticamente**») |
| «continúa» literal de la supervisión | antes de la ventana o arranque | FIXTURE_CONTROL_CONTINUE_PRE_WINDOW | ninguno | sí, **sin** bandera, hasta que OQ-20 se decida | §50 («la sesión de supervisión puede enviar a A el mensaje literal mínimo «continúa» para G0, QU y QH»); práctica vigente: la supervisión no envía mensajes a sesiones del fixture (FX-U1-chain/chain.json: «el mensaje de la supervisión llevaría el título de la sesión real», condicional no medido); OQ-20 |
| «continúa» literal de la supervisión | ventana | SUPERVISION_CONTINUE_IN_LOOP | ninguno | sí | §50 («no hacen verdadero `OWNER_AS_MESSAGE_BUS`»); su alcance en 1-7: OQ-04, OQ-20 |
| otro mensaje de la supervisión | antes / ventana / después | SUPERVISION_MESSAGE_PRE_WINDOW / SUPERVISION_MESSAGE_IN_LOOP / SUPERVISION_MESSAGE_POST_WINDOW | ninguno / UNDETERMINED / ninguno | sí | §50 (solo el literal mínimo, «sin información sustantiva del protocolo»); fallo cerrado |
| etiqueta de una sesión del plano real observada en la sesión del fixture (`ObservedSenderLabel` del evento, medida en la transcripción) | cualquiera | (bandera SUPERVISION_LABEL_LEAK) | — | sí | P-16; D.6. Solo con evidencia medida: sin el campo, no hay bandera |
| mensaje con contenido solo de supervisión (oráculo, esperados) | cualquiera | (bandera SUPERVISION_ONLY_CONTENT_IN_HUMAN_EVENT) | — | sí | D.6; kit FX-06 (oráculo fuera de los hosts) |
| mensaje de arranque: MESSAGE cuyo `Text` (obligatorio en el arranque; sin él, rechazo, §6) es el literal `continúa` o la plantilla neutra (SHA-256 de `Text` en `Window.KickoffTemplateSha256`) | arranque | KICKOFF | ninguno | no | D.8 («El Owner puede autorizar antes la apertura y el consumo (OD-5), pero no actúa como transporte en tiempo de ejecución»); README §2.1 y §5 |
| arranque con marcadores del protocolo o líneas de artefactos | arranque | KICKOFF_WITH_PROTOCOL_CONTENT | ninguno | sí | fallo cerrado |
| arranque con otro contenido u otro `Kind` (instrucciones libres, respuesta a una pregunta del Principal, apertura de sesión) | arranque | KICKOFF_NONSTANDARD | ninguno | sí | README §5 (el estímulo posterior a la designación es el literal `continúa`); §20.1 (una exención de opción B es COORDINATOR_DECISION) |
| «continúa» literal escrito por el Owner | antes de la ventana | FIXTURE_CONTROL_CONTINUE_PRE_WINDOW | ninguno | no | práctica vigente (las continuaciones las escribe el Owner; decisiones §54 «Bus de mensajes»: abrir una sesión asignada al Owner no es AUTONOMY_GAP, el transporte manual entre roles sí; precedente: R recibió el mensaje inicial y un «Continúa», evidencia §71). §50 no cubre literalmente este caso (OQ-20) |
| líneas de un artefacto de rol custodiado (resultado, invocación, prompt, cierre) | ventana | MANUAL_RELAY → AUTONOMY_GAP derivado con `RelayedBy` OWNER | **TRUE** | — | C-32; C-37; §20.9; §54 («el transporte manual entre roles sí») |
| «continúa» literal del Owner | ventana | CONTINUE_STIMULUS_IN_LOOP | ninguno | **sí**: su alcance en 1-7 no está fijado (OQ-04); con D.8, un mensaje para reanudar a un Principal que terminó el turno impide el PASS (README §6 R-08) | D.8 PASS; kit FX-06 «Autonomía» |
| marcadores del protocolo sin líneas de artefactos (ids L/I/R/B/P, nombres de esquema, veredictos, estados de disposición, SHAs, objetos JSON) | ventana | PROTOCOL_CONTENT_IN_LOOP | UNDETERMINED | sí | fallo cerrado (posible relevo resumido) |
| respuesta del Owner a una pregunta del Principal | ventana | HUMAN_DECISION_IN_LOOP | UNDETERMINED | sí | §20.1 (solo una frontera de ESCALATION_OWNER la justifica); precedente FX-U1 (respuesta a AskUserQuestion atribuida al «Coordinator», evidencia §69) |
| aprobación humana de una herramienta (permiso) | ventana | OWNER_CLICK_IN_LOOP (candidato AUTONOMY_GAP) | UNDETERMINED | sí | recipes.md FX-06 («un clic humano aquí es un relevo manual»; preparación, no texto congelado); OV-I62-06 en ov-scripts.md («no pulsar lanzamientos de otras sesiones, no aprobar pasos intermedios»); modo de permisos: lo elige el Owner y la supervisión lo registra (README P7; OQ-25) |
| cambio de effort | ventana | RUNTIME_CONTROL_IN_LOOP | ninguno | sí | §50 solo lo admite en FX-01 |
| detención de la sesión | ventana | SESSION_STOPPED_IN_LOOP | ninguno | sí | — |
| cualquier otro texto del Owner | ventana | OWNER_INTERVENTION_UNCLASSIFIED | UNDETERMINED | sí | fallo cerrado |
| evento no atribuible | ventana o arranque | UNATTRIBUTED_HUMAN_EVENT | UNDETERMINED | sí | fallo cerrado |
| cualquier evento del Owner | después del paso 7 | POST_WINDOW_HUMAN_EVENT | ninguno | no | D.8-8 («sí, solo para decidir», recipes.md) |

«continúa» literal: tras NFC, recorte, casefold y sin «¡», «.» ni «!» finales, igual a `continúa` o `continua`.

## 4. Comprobaciones sobre la custodia

| Comprobación | Regla | Resultado |
|---|---|---|
| registros AUTONOMY_GAP de `orchestration.autonomy_gaps[]` | deduplicados por `(Path, Blob)` y clasificados por el primer punto que los custodia. **Cuentan** si aparecen por primera vez en un punto de la ventana o si nombran una solicitud lógica de la ventana. **Heredados** (primer punto anterior al arranque y sin solicitud de la ventana): se enumeran en `PreexistingAutonomyGaps` y no cuentan en 1-7. **Posteriores** (primer punto después del paso 7 y sin solicitud de la ventana): `PostWindowAutonomyGaps`, con disposición | contados con `RelayedBy` OWNER → TRUE (regla de F4 sobre la ventana); contado con `RelayedBy` COORDINATOR → hueco registrado (no PASS); contado con `RelayedBy` ausente u otro valor → bandera AUTONOMY_GAP_RELAYER_UNKNOWN y UNDETERMINED (README §18.7: el registro dice quién relevó; C-39 «causa exacta»). Lectura de la supervisión de D.8 y C-39 («en los pasos 1-7»), no texto de F4: OQ-24 |
| referencia AUTONOMY_GAP que no resuelve en el árbol del punto, o que no es un AUTONOMY_GAP | I-S13; README §18.7; F4 `Missing` | en un punto de la ventana o posterior: bandera (no PASS); en un punto anterior: se enumera como heredada |
| lanzamiento de cada intento en LAUNCHED o posterior | un registro con `Transport` = `external-process`, rol ARCHITECT y el mismo `RunId`, **custodiado por el propio intento**: el `StateRef` de `launch_evidence` o el de `runtime_evidence` (B.8.8; §20.3.2). Con la variante L de la receta (sin QU LAUNCHED) o si el intento pasó de LAUNCHING a RESULT_RECEIVED, la prueba solo puede venir de `runtime_evidence` | AUTOMATIC; si falta, UNPROVEN → UNDETERMINED; LAUNCH_UNCERTAIN se acepta como incierto por protocolo |
| resultado custodiado de cada intento | su SHA-256 = el `OutputSha256` de ese registro (salida `-o` del proceso lanzado por el Principal) | AUTOMATIC; si no, UNPROVEN → UNDETERMINED (posible resultado transportado a mano) |
| `ReviewLoopAuthorization` | un commit del archivo de decisiones con `Issuer` COORDINATOR y `I62-REVIEW-LOOP-AUTHORIZATION:`, publicado antes del arranque. `Issuer` = identidad del **autor del commit** (igual a su `AuthorRole` en `BranchCommits[]`), nunca el autor que declara el bloque: el lector de F4 (`Orchestration.DecisionBlock`) acepta cualquier bloque cercado con el marcador. El commit de la RLA vigente abre el ámbito del archivo de decisiones (§2) y por eso tiene que figurar en `BranchCommits[]`: su `Issuer` siempre se contrasta con el autor del commit | si falta: bandera RLA_NOT_CUSTODIED_BEFORE_WINDOW; commit de la RLA vigente ausente de `BranchCommits[]` o con otro `AuthorRole`: rechazo |
| commits del archivo de decisiones desde el ámbito (§2: commit de la RLA vigente o primer evento de la sesión del Principal) | dentro de la ventana, todo commit exige disposición: solo los de `Issuer` COORDINATOR son decisiones del Coordinator (D.8); el archivo no debe cambiar entre el arranque y el paso 7 ([rla.template.md](rla.template.md) §3). Antes del arranque (desde la RLA) y después del paso 7, todo commit de un autor distinto del Coordinator exige disposición (el archivo lo escribe el Coordinator del fixture, [evidence-schema.md](evidence-schema.md) §2). Los commits anteriores al ámbito (p. ej., los de FX-02) no se auditan aquí | ventana: COORDINATOR → INTERMEDIATE_COORDINATOR_DECISION (no PASS); PRINCIPAL → se listan, con disposición; OWNER u OTHER → bandera DECISIONS_FILE_COMMIT_BY_NON_COORDINATOR_IN_WINDOW con disposición. Fuera de la ventana, autor distinto del Coordinator → disposición `DECISIONS_FILE_COMMIT_BY_<autor>_PRE_WINDOW` o `…_POST_WINDOW`. En todo el ámbito, un marcador `I62-*` o `FIXTURE-ORDER` de un autor que no es el Coordinator: bandera DECISIONS_FILE_MARKER_BY_NON_COORDINATOR, con su fase (V14 §20.2 AUT-I1 «La autonomía cambia el transporte y la orquestación, nunca la autoridad»; §20.5.1 «Nunca simula una decisión individual del Coordinator que no ocurrió»; 16.20 P-20; D.8 FAIL «materializa un binding fuera de los criterios») |
| commits de `fx/u1` y de `main` del fixture dentro de la ventana (`BranchCommits[]`, con la identidad del autor) | los pasos 1-7 son todos del Principal (D.8); un commit ajeno (una orden fuera del archivo de decisiones, un cambio en `main` que fuerza un rebase) es invisible para la custodia del estado | autor distinto del Principal: bandera NON_PRINCIPAL_COMMIT_IN_WINDOW con disposición |
| relevos manuales derivados de mensajes | cada uno con un AUTONOMY_GAP custodiado (`RelayedBy` OWNER, misma solicitud si el mensaje la nombra) | si falta: P-21 (`MissingAutonomyGapRecords`) |

Sobre OQ-11: hoy un `relay-record/v2` válido para una revisión no puede construirse sin decidir `TaskId` (obligatorio, cadena), `Phase` (sin valor de
revisión) y `WindowSeq` (≥ 1). El auditor solo exige del registro de prueba los campos de su entrada (§6); la forma custodiada (un `relay-record/v2` con
la decisión de OQ-11, o el `runtime-evidence.json` de [evidence-schema.md](evidence-schema.md) §3) la decide el Coordinator antes de la ventana.

## 5. Agregados

- `OWNER_AS_MESSAGE_BUS` = TRUE si hay un relevo del Owner cierto (regla de F4 sobre los huecos que cuentan, o MANUAL_RELAY); si no, UNDETERMINED si queda
  algún motivo de duda (`UndeterminedReasons`); si no, FALSE.
- `PassEligibleOnTransport` = true solo si: paso 7 alcanzado; bus FALSE; ningún AUTONOMY_GAP que cuente; ningún P-21; ninguna bandera; ningún elemento
  con disposición requerida. Si es false, `ReasonsNotEligible` dice por qué.
- Cada AUTONOMY_GAP sale con los dos juegos de campos: README §18.7 / F4 (`Kind`, `LogicalReviewRequestId`, `Round`, `RelayedBy`, `Medium`, `Artifact`,
  `Cause`) y V14 §20.9 (`RequiredRole`, `RequiredAction`, `MissingCapabilityOrAuthority`, `AttemptedTransport`, `WhyAutomaticRelayUnavailable`,
  `ManualFallbackUsed`), con `UNKNOWN` donde la fuente no lo establece (OQ-17).

## 6. Entrada `fx06-transport-audit-input/v3` (la construye la supervisión)

| Campo | Contenido | Cómo se obtiene |
|---|---|---|
| `Unit` | unidad anfitriona | — |
| `SupervisionOnlyTokens[]` | cadenas que nunca deben aparecer en un mensaje humano: SHA-256 del oráculo y fragmentos distintivos de los archivos solo de supervisión | la supervisión, de sus propios archivos (esta línea no lee el oráculo) |
| `Window` | `KickoffEventSeq` (entero); `Step7RecordVersion` (entero, o `null` si el paso 7 no se alcanzó); `Step1Commit` (`{Commit, PushedUtc}` del commit de X v1, o `null` si el paso 1 no se alcanzó; obligatorio si hay paso 7); `PrincipalSession` (id de la sesión del Principal); `KickoffTemplateSha256[]` (SHA-256 de la plantilla neutra de README §5 tal como se rellenó; puede ir vacía) | la supervisión, tras la corrida |
| `BranchCommits[]` | todos los commits de `fx/u1` y de `main` del fixture, al menos desde el ámbito de §2 (el commit de la RLA de P5, o el primer evento de la sesión del Principal si es anterior) hasta el paso 7, más los commits de decisiones posteriores: `Commit`, `Ref`, `PushedUtc`, `AuthorRole` (PRINCIPAL, COORDINATOR, OWNER, OTHER: identidad sintética del autor del commit; la del Principal la fija P6) | `git --git-dir=<origen del fixture> log --format=…` de ambas ramas y el registro de push; la supervisión declara la cobertura |
| `Points[]` | todos los puntos durables desde el último anterior al arranque hasta el del paso 7 (y los posteriores que existan): `RecordVersion`, `Commit`, `Point`, `PushedUtc`, `LoopPhase`, `AutonomyGaps[]` (`Path`, `Blob`, `ResolvesInTree`, `Record`), `Attempts[]` (`LogicalReviewRequestId`, `AttemptSeq`, `State`, `RunId`, `LaunchEvidence`, `RuntimeEvidence`, `Result` con `Sha256`) | `git --git-dir=<origen del fixture> show <commit>:docs/automation/state/<UNIDAD>.yml` y los blobs citados; `PushedUtc` del registro de la supervisión o del push observado |
| `Decisions[]` | commits que tocan el archivo de decisiones: `Commit`, `PushedUtc`, `Markers[]`, `Issuer` (COORDINATOR, PRINCIPAL, OWNER, OTHER) | `git log` del archivo de decisiones; `Markers[]` = la línea de marcador de cada bloque cercado que el commit **añade o modifica** respecto de su padre (una enmienda en sitio de un bloque existente también lo lista); `Issuer` = la identidad del autor del commit (la misma que su `AuthorRole` en `BranchCommits[]`), nunca la autoría que declare el bloque |
| `RelayRecords[]` | registros de prueba del lanzamiento, cada uno citado por `launch_evidence` o `runtime_evidence` de un intento: `Path`, `Blob`, `Source` (RELAY_RECORD_V2 o RUNTIME_EVIDENCE), `RunId`, `Role`, `AdapterId`, `Transport`, `OutcomeKind`, `OutputSha256` | blobs custodiados |
| `ArtifactLineDigests[]` | por artefacto de rol custodiado (resultados, invocaciones, prompts, cierres): SHA-256 de cada línea con NFC y recorte, de 24 caracteres o más | blobs custodiados |
| `HumanEvents[]` | `Seq`, `Utc`, `Session` (obligatoria), `Actor` (OWNER, SUPERVISION, OTHER), `Kind`, `Text`, `TextSha256`, `ObservedSenderLabel` (etiqueta o título de remitente que la transcripción de la sesión del fixture muestra para el mensaje, o `null`) | transcripción de la sesión del Principal (mensajes del lado del usuario, respuestas a preguntas, aprobaciones de herramientas) y metadatos de la app (apertura y reapertura de **cualquier** sesión, effort); lo hace la supervisión después de la corrida |
| `Caps.PrincipalSessions` | 2 (D.8: 1 sesión + 1 reapertura); solo cuentan las aperturas de `Window.PrincipalSession` | D.8 |

**Rechazo de la entrada** (fallo cerrado, código 2): falta un campo obligatorio; `KickoffEventSeq` o `Step7RecordVersion` no existen; `Seq` o
`RecordVersion` duplicados; instante que no es ISO-8601 con Z; `TextSha256` distinto de `Text`; `Issuer`, `AuthorRole` o `Source` fuera de su
enumeración; **orden de `Seq` distinto del orden de los `Utc`**; **orden de `RecordVersion` distinto del orden de los `PushedUtc`**; **cierre no posterior a
la apertura**; punto del paso 7 que **no** tiene `LoopPhase` ARCHITECT_SATISFIED, que no es el primero con esa fase en la ventana o que no ingiere
(RESULT_INGESTED) un intento de una solicitud distinta de la primera de la ventana (con al menos dos solicitudes en la ventana); arranque que no es anterior
al primer punto con bucle activo; **arranque fuera de la sesión del Principal**; **arranque no anterior al push del paso 1**; **arranque de `Kind` MESSAGE o ANSWER_TO_PRINCIPAL_QUESTION
sin `Text`** (su `TextSha256` no se puede verificar; v3.1); paso 7 sin paso 1; `Step1Commit` ausente de `BranchCommits[]` o posterior al primer punto con
bucle activo; **un punto desde el ámbito de §2 hasta el paso 7, o un commit de decisiones desde ese ámbito (incluido el de la RLA vigente), que no figura
en `BranchCommits[]`**; `Issuer` de un commit de decisiones distinto de su `AuthorRole`; `Markers` que no es una lista de cadenas; `(Commit, Ref)`
duplicado en `BranchCommits[]`.

## 7. Uso

```text
python owner_as_message_bus_audit.py self-test --output selftest-result.json
python owner_as_message_bus_audit.py audit --input <R:>/transport-audit-input.json --output <R:>/transport-audit-result.json
```

El resultado lleva `InputSha256` (bytes exactos de la entrada). Dos ejecuciones con la misma entrada dan los mismos bytes (comprobado en la autoprueba y
con `example-input.synthetic.json` → `example-result.synthetic.json`).

## 8. Límites declarados

- La cobertura depende de la transcripción y de los metadatos de la app: una acción humana que no deja rastro en ellos no se ve (misma cobertura
  declarada que D.6). La supervisión declara la cobertura en la entrada de evidencia.
- La coincidencia por líneas detecta copias literales; un resumen parafraseado solo se detecta por marcadores (UNDETERMINED), nunca como relevo cierto.
- La clasificación de clics, respuestas y «continúa» dentro de la ventana es fallo cerrado, no una regla congelada: la decide el Coordinator (OQ-04;
  para el modo de permisos y las aprobaciones de herramientas, OQ-25; para los mensajes de la supervisión, OQ-20).
- El ámbito de los AUTONOMY_GAP heredados (no cuentan en 1-7) es una lectura de la supervisión de D.8 y C-39 (OQ-24). Mientras el Coordinator no la
  confirme, la supervisión presenta `OwnerAsMessageBusF4RuleAllSuppliedPoints` junto al valor de la ventana.
- El auditor no lee artefactos fuera de su entrada: los artefactos que la supervisión publica en el fixture se comprueban antes con
  `prepublish_scan.py` (README §2.1, P5), no con este programa.
- `BranchCommits[]` solo es tan completo como lo construya la supervisión: el auditor exige que figuren el commit del paso 1, el commit de la RLA
  vigente, los puntos desde el ámbito de §2 hasta el paso 7 y los commits de decisiones desde ese ámbito; no puede probar que no falte otro. Que el
  `AuthorizationRef` de cada binding apunte a la RLA publicada por el Coordinator lo comprueba `materialization-reproduction.json`
  (`AuthorizationMatchesPublishedRla`; README §2.4, paso 5), no este programa. `ObservedSenderLabel` depende de que la transcripción muestre el remitente: su ausencia no
  prueba que no hubo fuga.
- El auditor solo evalúa el transporte. No re-deriva la materialización, ni las lecturas del revisor, ni el aislamiento del Principal: esas
  re-auditorías son pasos aparte de la supervisión (README §2.4, paso 5; [evidence-schema.md](evidence-schema.md) §4).
