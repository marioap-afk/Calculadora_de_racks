# FX-06 — auditor del transporte: `OWNER_AS_MESSAGE_BUS` y AUTONOMY_GAP (SOLO SUPERVISIÓN)

```text
Programa:    owner_as_message_bus_audit.py v2.0.0 (Python 3, sin dependencias; salida determinista). Autoprueba: 30 casos sintéticos de auditoría + 10
             rechazos de entrada = 40/40 PASS (selftest-result.json; ningún dato de una corrida real).
Autoridades: V14 D.8 (PASS: OWNER_AS_MESSAGE_BUS = false en 1-7, sin relevo del Owner ni decisión intermedia del Coordinator), C-32, C-37, C-39
             («OWNER_AS_MESSAGE_BUS = false en los pasos 1-7»), §20.1 (RELAY frente a ESCALATION y COORDINATOR_DECISION), §20.3.2 (identidad observada
             en el relay-record/v2, referenciada como RuntimeEvidenceRef), §20.9 (AUTONOMY_GAP ≠ PASS), B.8.8 (launch_evidence, runtime_evidence,
             autonomy_gaps[]; I-P13), B.9 (último párrafo); AUTOMATION_PLAN 16.29 (P-21); README §18.7; F4: AutonomyGaps.OwnerAsMessageBus y
             AutonomyGaps.Missing (tests/RackCad.Tests/I62/MaterializationClose.cs), F4-OBS-22; decisiones del Coordinator §49 (fila FX-06) y §54
             («Bus de mensajes»).
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
- Añade, con fallo cerrado, lo que la custodia sola no ve: los mensajes humanos a la sesión del Principal.

## 2. Ventana (por instantes)

- **Apertura:** el instante (`Utc`) del evento humano de arranque (`Window.KickoffEventSeq`; el `Seq` solo lo identifica).
- **Cierre:** el `PushedUtc` del punto que custodia la ingestión de C (`Window.Step7RecordVersion`), incluido. `null` = el paso 7 no se alcanzó: la ventana
  queda abierta hasta el último evento y la corrida no es elegible.
- **Fase de cada evento humano y de cada punto**: solo por su instante. Antes de la apertura: PRE_WINDOW; en (apertura, cierre]: IN_WINDOW; después:
  POST_WINDOW. Un evento distinto del arranque, un punto o un commit de decisiones con el mismo instante que el arranque cuentan dentro (fallo cerrado);
  la RLA, en cambio, tiene que ser estrictamente anterior. Ningún punto con bucle activo puede ser anterior o simultáneo al arranque (§6).
- Antes del arranque: precondiciones (apertura de la sesión, designación del Coordinator, mensaje inicial). Después del paso 7: paso 8 (el Owner solo
  decide, si procede).

## 3. Clasificación de los eventos humanos

| Evento | Fase | Clase | Efecto en `OWNER_AS_MESSAGE_BUS` | Disposición | Base |
|---|---|---|---|---|---|
| apertura o reapertura de sesión por el Owner | cualquiera | OWNER_OPENED_SESSION | ninguno | no (salvo tope superado: más de 2 sesiones, D.8) | §49 FX-06; §54 |
| mensaje de la supervisión a una sesión del fixture | cualquiera | SUPERVISION_MESSAGE_FORBIDDEN | ninguno | sí (bandera SUPERVISION_LABEL_LEAK) | P-16; D.6; hecho vigente: el mensaje llevaría el título de la sesión real, así que los mensajes al fixture los escribe el Owner. §50 autorizó esos mensajes de la supervisión «para G0, QU y QH»: conflicto registrado en OQ-20 |
| mensaje con contenido solo de supervisión (oráculo, esperados) | cualquiera | (bandera SUPERVISION_ONLY_CONTENT_IN_HUMAN_EVENT) | — | sí | D.6; kit FX-06 (oráculo fuera de los hosts) |
| mensaje de arranque | arranque | KICKOFF (o KICKOFF_WITH_PROTOCOL_CONTENT si trae marcadores o líneas de artefactos) | ninguno | solo en la segunda variante | D.8 («el Owner puede autorizar antes la apertura y el consumo») |
| «continúa» literal escrito por el Owner | antes de la ventana | FIXTURE_CONTROL_CONTINUE_PRE_WINDOW | ninguno | no | práctica vigente (las continuaciones las escribe el Owner; decisiones §54 «Bus de mensajes»: abrir una sesión asignada al Owner no es AUTONOMY_GAP, el transporte manual entre roles sí; precedente: R recibió el mensaje inicial y un «Continúa», evidencia §71). §50 no cubre literalmente este caso (OQ-20) |
| líneas de un artefacto de rol custodiado (resultado, invocación, prompt, cierre) | ventana | MANUAL_RELAY → AUTONOMY_GAP derivado con `RelayedBy` OWNER | **TRUE** | — | C-32; C-37; §20.9; §54 («el transporte manual entre roles sí») |
| «continúa» literal | ventana | CONTINUE_STIMULUS_IN_LOOP | ninguno | **sí**: su alcance en 1-7 no está fijado (OQ-04); con D.8, un mensaje para reanudar a un Principal que terminó el turno impide el PASS (README §6 R-08) | D.8 PASS; kit FX-06 «Autonomía» |
| marcadores del protocolo sin líneas de artefactos (ids L/I/R/B/P, nombres de esquema, veredictos, estados de disposición, SHAs, objetos JSON) | ventana | PROTOCOL_CONTENT_IN_LOOP | UNDETERMINED | sí | fallo cerrado (posible relevo resumido) |
| respuesta del Owner a una pregunta del Principal | ventana | HUMAN_DECISION_IN_LOOP | UNDETERMINED | sí | §20.1 (solo una frontera de ESCALATION_OWNER la justifica); precedente FX-U1 (respuesta a AskUserQuestion atribuida al «Coordinator», evidencia §69) |
| aprobación humana de una herramienta (permiso) | ventana | OWNER_CLICK_IN_LOOP | UNDETERMINED | sí | recipes.md FX-06 («un clic humano aquí es un relevo manual»; preparación, no texto congelado); OV-I62-06 («no pulsar lanzamientos, no aprobar pasos») |
| cambio de effort | ventana | RUNTIME_CONTROL_IN_LOOP | ninguno | sí | §50 solo lo admite en FX-01 |
| detención de la sesión | ventana | SESSION_STOPPED_IN_LOOP | ninguno | sí | — |
| cualquier otro texto del Owner | ventana | OWNER_INTERVENTION_UNCLASSIFIED | UNDETERMINED | sí | fallo cerrado |
| evento no atribuible | ventana o arranque | UNATTRIBUTED_HUMAN_EVENT | UNDETERMINED | sí | fallo cerrado |
| cualquier evento | después del paso 7 | POST_WINDOW_HUMAN_EVENT | ninguno | no | D.8-8 («sí, solo para decidir», recipes.md) |

«continúa» literal: tras NFC, recorte, casefold y sin «¡», «.» ni «!» finales, igual a `continúa` o `continua`.

## 4. Comprobaciones sobre la custodia

| Comprobación | Regla | Resultado |
|---|---|---|
| registros AUTONOMY_GAP de `orchestration.autonomy_gaps[]` | deduplicados por `(Path, Blob)` y clasificados por el primer punto que los custodia. **Cuentan** si aparecen por primera vez en un punto de la ventana o si nombran una solicitud lógica de la ventana. **Heredados** (primer punto anterior al arranque y sin solicitud de la ventana): se enumeran en `PreexistingAutonomyGaps` y no cuentan en 1-7. **Posteriores** (primer punto después del paso 7 y sin solicitud de la ventana): `PostWindowAutonomyGaps`, con disposición | contados con `RelayedBy` OWNER → TRUE (regla de F4 sobre la ventana); contado de otro actor → hueco registrado (no PASS). Lectura de la supervisión de D.8 y C-39 («en los pasos 1-7»), no texto de F4: OQ-24 |
| referencia AUTONOMY_GAP que no resuelve en el árbol del punto, o que no es un AUTONOMY_GAP | I-S13; README §18.7; F4 `Missing` | en un punto de la ventana o posterior: bandera (no PASS); en un punto anterior: se enumera como heredada |
| lanzamiento de cada intento en LAUNCHED o posterior | un registro con `Transport` = `external-process`, rol ARCHITECT y el mismo `RunId`, **custodiado por el propio intento**: el `StateRef` de `launch_evidence` o el de `runtime_evidence` (B.8.8; §20.3.2). Con la variante L de la receta (sin QU LAUNCHED) o si el intento pasó de LAUNCHING a RESULT_RECEIVED, la prueba solo puede venir de `runtime_evidence` | AUTOMATIC; si falta, UNPROVEN → UNDETERMINED; LAUNCH_UNCERTAIN se acepta como incierto por protocolo |
| resultado custodiado de cada intento | su SHA-256 = el `OutputSha256` de ese registro (salida `-o` del proceso lanzado por el Principal) | AUTOMATIC; si no, UNPROVEN → UNDETERMINED (posible resultado transportado a mano) |
| `ReviewLoopAuthorization` | un commit del archivo de decisiones con `Issuer` COORDINATOR y `I62-REVIEW-LOOP-AUTHORIZATION:`, publicado antes del arranque | si falta: bandera RLA_NOT_CUSTODIED_BEFORE_WINDOW |
| commits del archivo de decisiones dentro de la ventana | solo los de `Issuer` COORDINATOR son decisiones del Coordinator (D.8) | COORDINATOR: INTERMEDIATE_COORDINATOR_DECISION (no PASS); PRINCIPAL: se listan, sin efecto; OWNER u OTHER: bandera con disposición |
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

## 6. Entrada `fx06-transport-audit-input/v2` (la construye la supervisión)

| Campo | Contenido | Cómo se obtiene |
|---|---|---|
| `Unit` | unidad anfitriona | — |
| `SupervisionOnlyTokens[]` | cadenas que nunca deben aparecer en un mensaje humano: SHA-256 del oráculo y fragmentos distintivos de los archivos solo de supervisión | la supervisión, de sus propios archivos (esta línea no lee el oráculo) |
| `Window` | `KickoffEventSeq` (entero); `Step7RecordVersion` (entero, o `null` si el paso 7 no se alcanzó) | la supervisión, tras la corrida |
| `Points[]` | todos los puntos durables desde el último anterior al arranque hasta el del paso 7 (y los posteriores que existan): `RecordVersion`, `Commit`, `Point`, `PushedUtc`, `LoopPhase`, `AutonomyGaps[]` (`Path`, `Blob`, `ResolvesInTree`, `Record`), `Attempts[]` (`LogicalReviewRequestId`, `AttemptSeq`, `State`, `RunId`, `LaunchEvidence`, `RuntimeEvidence`, `Result` con `Sha256`) | `git --git-dir=<origen del fixture> show <commit>:docs/automation/state/<UNIDAD>.yml` y los blobs citados; `PushedUtc` del registro de la supervisión o del push observado |
| `Decisions[]` | commits que tocan el archivo de decisiones: `Commit`, `PushedUtc`, `Markers[]`, `Issuer` (COORDINATOR, PRINCIPAL, OWNER, OTHER) | `git log` del archivo de decisiones, los marcadores de sus bloques y el autor del bloque |
| `RelayRecords[]` | registros de prueba del lanzamiento, cada uno citado por `launch_evidence` o `runtime_evidence` de un intento: `Path`, `Blob`, `Source` (RELAY_RECORD_V2 o RUNTIME_EVIDENCE), `RunId`, `Role`, `AdapterId`, `Transport`, `OutcomeKind`, `OutputSha256` | blobs custodiados |
| `ArtifactLineDigests[]` | por artefacto de rol custodiado (resultados, invocaciones, prompts, cierres): SHA-256 de cada línea con NFC y recorte, de 24 caracteres o más | blobs custodiados |
| `HumanEvents[]` | `Seq`, `Utc`, `Session`, `Actor` (OWNER, SUPERVISION, OTHER), `Kind`, `Text`, `TextSha256` | transcripción de la sesión del Principal (mensajes del lado del usuario, respuestas a preguntas, aprobaciones de herramientas) y metadatos de la app (apertura, reapertura, effort); lo hace la supervisión después de la corrida |
| `Caps.PrincipalSessions` | 2 (D.8: 1 sesión + 1 reapertura) | D.8 |

**Rechazo de la entrada** (fallo cerrado, código 2): falta un campo obligatorio; `KickoffEventSeq` o `Step7RecordVersion` no existen; `Seq` o
`RecordVersion` duplicados; instante que no es ISO-8601 con Z; `TextSha256` distinto de `Text`; `Issuer` o `Source` fuera de su enumeración; **orden de
`Seq` distinto del orden de los `Utc`**; **orden de `RecordVersion` distinto del orden de los `PushedUtc`**; **cierre no posterior a la apertura**; punto del
paso 7 que **no** tiene `LoopPhase` ARCHITECT_SATISFIED, que no es el primero con esa fase en la ventana o que no ingiere (RESULT_INGESTED) un intento de
una solicitud distinta de la primera de la ventana (con al menos dos solicitudes en la ventana); arranque que no es anterior al primer punto con bucle
activo.

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
- La clasificación de clics, respuestas y «continúa» dentro de la ventana es fallo cerrado, no una regla congelada: la decide el Coordinator (OQ-04).
- El ámbito de los AUTONOMY_GAP heredados (no cuentan en 1-7) es una lectura de la supervisión de D.8 y C-39 (OQ-24). Mientras el Coordinator no la
  confirme, la supervisión presenta `OwnerAsMessageBusF4RuleAllSuppliedPoints` junto al valor de la ventana.
- El auditor no lee artefactos fuera de su entrada: los artefactos que la supervisión publica en el fixture se comprueban antes con
  `prepublish_scan.py` (README §2.1, P5), no con este programa.
