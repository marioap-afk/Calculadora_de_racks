# I-62 — Paquete de revisión del Architect (Proposal V10)

```text
PROPOSAL V10 — NOT REVIEWED
Architect      = REVIEW REQUIRED: UNA revisión formal limpia (V6 y V7 CHANGES REQUIRED; V8 no enviada; la revisión de V9 NO se acredita como formal limpia)
Coordinator    = A62-V9-01..06 ACCEPTED REQUIRED; Proposal V10 autorizada (registro I-62-architect-review-v9-disposition.md)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V10 §11.4); sin decisión no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v10.md                       blob 58f88fc602d0d4eaf3900a3514259da6b94faba2
  docs/initiatives/I-62-architect-review-v9-disposition.md    blob 865cf0d822d8184e2156e3f5e9196146917561d9
Versiones anteriores:
  V9: commit b0725114e60319079c3abacf10542743ebb9303c, blob 831e3a6a87a242c872cfa0755f73e282f6c043c8 (Architect: BLOCKED — OWNER DECISION; no acreditada como revisión formal limpia)
  V8: commit d66463a5…, blob 667b59d7… (histórica; no revisada por el Architect)
  V7: commit 3d7ec77f…, blob 9f68c950… (Architect: CHANGES REQUIRED)
  V6: commit 3888c5c8…, blob 19672958… (Architect: CHANGES REQUIRED)
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v10.md` = `58f88fc6…`. Si no coincide, revisa la versión designada o rechaza la discordancia. Este
> paquete no lleva su propio blob.

## 0. Condiciones de la revisión limpia (disposición del Owner y del Coordinator, «NEXT FORMAL REVIEW»)

V10 tendrá **una** revisión formal limpia del Architect. La revisión de V9 no se repite. La invocación debe:
- usar una sesión o un proceso independiente de la sesión autora, en SEPARATE SESSION, y declarar si revisor y autor son la misma persona (LIFECYCLE §5);
- usar un clon limpio y solo lectura;
- usar el **cierre efectivo de insumos** calculado antes de lanzar (Proposal V10 §20.3.1), con todas las lecturas transitivas obligatorias de `AGENTS.md`;
- no usar la transcripción ni la memoria del autor;
- declarar el contexto inyectado;
- registrar el `RuntimeEvidenceRef` desde el invocador (modelo, effort, hilo o sesión, sandbox, versión y huella, observados en el runtime; §20.3.2).

**Cierre efectivo previsto** para esta revisión. Se calculó sobre los blobs de esta rama, que en esas rutas son iguales a `main`. El invocador lo **recalcula
sobre el commit exacto** antes de lanzar y custodia el resultado:

| Clase | Contenido |
|---|---|
| `CanonicalInputs` | los de §2 |
| `AllowedTransitiveInputs` (opción A) | de `AGENTS.md` «Leer primero»: `docs/HANDOFF.md`, `README.md`, `docs/ARCHITECTURE.md` y los Context Packs del contrato (`documentation-governance`, con el índice `docs/context-packs/README.md`). Del índice, «Base obligatoria»: `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/AUTOMATION_PLAN.md` **completo** y el contrato `docs/initiatives/I-62-portabilidad-coordinador-principal.md`. Del pack, `required_docs`: `docs/INITIATIVE_LIFECYCLE.md`, `docs/FOUNDATIONS.md`, `docs/initiatives/README.md` y `docs/initiatives/PROMPT_TEMPLATES.md` |
| no incluidas (registradas) | `optional_docs` del pack (`I-06-auditoria-documental.md`): opcional, no obliga. `code_globs` del pack: orientan la inspección y no son una obligación de lectura (el índice lo dice así). Las instrucciones dentro de las plantillas de PROMPT_TEMPLATES se dirigen a otros prompts, no a quien las lee. Las referencias de HANDOFF («se consulta en …») son punteros, no obligaciones |
| `AllowedActions` | `git log --oneline -10`, `git rev-parse`, `git status` y `git show` (lectura de Git; ACTION_COMPATIBLE) |
| ACTION_INCOMPATIBLE | `dotnet test` (paso 4 de «Leer primero»): **quien autorice la invocación** decide antes de lanzar entre (a) una exención explícita para esta acción de solo lectura, con la CI exacta del commit como evidencia equivalente (opción B), y (b) permitir la acción en un clon desechable con escritura local y sin publicación. Sin esa decisión no hay lanzamiento |
| dependiente del adapter | `codex-cli` carga `AGENTS.md`. Un adapter de Claude carga además `CLAUDE.md`, cuya «Lectura inicial» no añade rutas fuera de la lista anterior; su guía de validación de dibujo queda como CONDITIONAL_NOT_TRIGGERED |
| `ForbiddenInputs` | transcripción y memoria de la sesión autora; sesiones ajenas; worktrees reales de otras unidades; `D:\IDs`; artefactos transitorios |

**Forma del resultado:** en lo posible, `rackcad-architect-review-result/v1` (Proposal V10 B.10.1), como representación experimental y no normativa, con:
- `OpenFindings` = A62-V9-01..06 (REQUIRED, aceptados por el Coordinator) y A62-V9-O01, A62-V9-O02 (OPTIONAL);
- una `FindingDisposition` por cada uno.

Si no es posible, es un AUTONOMY_GAP (§20.9).

**Alcance:** V10 completa. El foco mínimo son las correcciones de A62-V9-01..06, GAP-07, GAP-08, O01 y O02 (§3 y §4), y su reflejo en B.2, B.5, B.7,
B.8.8, B.9, B.10, C-29..C-41, D.5, D.8, F.8 y G. Las trazas son análisis del diseño, no ensayos.

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, fuente o contraejemplo, por qué importa, corrección precisa; disposición de cada hallazgo abierto.
Modo:      SEPARATE SESSION (declarado); si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

Con cero REQUIRED: el Owner decide OD-6 y después viene el Consensus Freeze (disposición del Owner y del Coordinator).

## 2. Lectura (insumos canónicos)

1. [Proposal V10](I-62-proposal-v10.md) completa (anexos A-G).
2. [Disposición del Owner y del Coordinator sobre la revisión de V9](I-62-architect-review-v9-disposition.md); [registro de la revisión de
   V9](I-62-architect-review-v9.md) y su resultado literal
   ([`output.json`](../automation/evidence/I-62-architect-v9/R20261002T000511Z-ba10/output.json)).
3. [Requisito R62-AUTO](I-62-coordinator-requirement-auto.md); registros del Architect [V6](I-62-architect-review-v6.md) y [V7](I-62-architect-review-v7.md);
   registros del Coordinator [V1](I-62-coordinator-review-v1.md) … [V5](I-62-coordinator-review-v5.md).
4. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt).
5. Autoridades: LIFECYCLE §§2-9; WORKFLOW §§3-4 y 10-11; AGENTS; AUTOMATION_PLAN §§8 y 16; ADR-0046; el Freeze de I-61
   ([Proposal V9 de I-61](I-61-proposal-v9.md)); `routing.md`, `model-catalog.md`, el README y los esquemas de agent-execution.

## 3. Delta V9 → V10

| Área | V9 | V10 |
|---|---|---|
| Aceptación de Architects en el bucle | `ReviewLoopAuthorization` con «política de binding»; un binding nuevo exigía la aceptación A7' individual | **§20.5.1:** autorización de **materialización** con diez campos cerrados; el Principal observa, comprueba, materializa, custodia e invoca solo con todos los criterios en SATISFIED; `Acceptance.Basis` = AUTHORIZED_MATERIALIZATION con `AuthorizationRef` y `DecisionRef` = `null` (B.5, B.2); sin candidato, COORDINATOR_DECISION u OWNER |
| Ciclo de una invocación | `pending_invocation` = lanzada y no ingerida | **§20.6:** siete estados durables (INVOCATION_PLANNED, BUDGET_RESERVED, LAUNCHING, LAUNCHED, LAUNCH_UNCERTAIN, RESULT_RECEIVED, RESULT_INGESTED) más CANCELLED_BEFORE_LAUNCH; write-ahead; recuperación A-D; ingestión idempotente |
| Identidades | `InvocationId` y `RunId` | **`LogicalReviewRequestId`** estable durante las reejecuciones; `InvocationId` por intento; `RunId` por lanzamiento (B.2, B.9) |
| Presupuestos | cinco contadores; `architect_invocations` = `review_rounds` + `transport_reruns` (ambiguo) | cuatro constantes (3, 3, 2 por solicitud, 2 por linaje) y dos topes derivados con fórmula cerrada (correcciones ≤ 2; lanzamientos ≤ 9); todo crece en la reserva; la tercera reejecución se rechaza antes de lanzar |
| Cierre de hallazgos | `closed_by` → un resultado del Architect; sin disposiciones explícitas | **§20.5.2:** `FindingDispositions[]` obligatorias; la omisión deja el hallazgo abierto; AGREED con un abierto omitido = INVALID; cierre con autoridad; rebaja solo por el emisor (LIFECYCLE §5); SUPERSEDED en el mismo linaje |
| Contratos de salida | un único `review-result/v1` para Architect y Reviewer | **§20.7, B.10:** `architect-review-result/v1` y `reviewer-result/v1` sobre un sobre común; correspondencia cerrada rol + acción → contrato; un REVIEWER nunca emite AGREED ni ARCHITECT_SATISFIED |
| Cambio de objeto | I-P13 lo admitía en CORRECTING → PUBLISHED, pero F.8 lo cambiaba al crear la segunda invocación | **§20.5:** `loop.object` cambia solo en el QU de PUBLISHED; F.8 reescrita en 17 pasos, con las caídas en cada frontera; I-P13 y F.8 coinciden |
| Insumos de la invocación | `CanonicalInputs` y `ForbiddenInputs` | **§20.3.1:** + `AllowedTransitiveInputs`, `AllowedActions`, `DeclaredRuntimeContext` y `EffectiveInputClosure` (`input-closure/v1`), calculado hasta el punto fijo; opción A por defecto; opción B con autoridad; lectura fuera del cierre = INVALID_REVIEW_CONTEXT (P-22); regla de solo lectura propuesta para §16 bajo OD-1 |
| Identidad del revisor | `ReviewerIdentity` autodeclarada en el resultado | **§20.3.2:** `ReviewerDeclaredIdentity` (informativa) frente a `InvokerObservedRuntimeIdentity` (`RuntimeEvidenceRef`); contradicción = S-04 (P-23) |
| Matriz | C-29..C-39 | C-29, C-30, C-31, C-34, C-35, C-36 y C-38 ampliados; **C-40** (materialización) y **C-41** (cierre e identidad) nuevos |
| Riesgos (O01) | §19 apuntaba al §5 del paquete V9, que ya no tenía los 12 retos | §19 con la tabla de los 12 retos del mandato, su tratamiento y su riesgo residual |
| OV y D.5 (O02) | FINAL_CANDIDATE con OV-I62-01..05; D.5 sin FX-06 | OV-I62-01..06; FX-06 en D.5 |

## 4. Disposición de la sesión por hallazgo (la sesión declara; el cierre lo decide quien revisa)

| Hallazgo | Corrección en V10 | Obligación de verificación |
|---|---|---|
| A62-V9-01 | §5; §20.5 (autorización); §20.5.1; B.2 (`AuthorizationRef`); B.5 (`Acceptance`); B.7 (REVIEWER por contrato de gate); §20.13 | C-40 (nuevo Architect tras CHANGES REQUIRED; binding no elegible rechazado; binding elegible materializado sin relevo humano); D.8 pasos 2 y 6 |
| A62-V9-02 | §20.4; §20.6 (estados, regla de lanzamiento, casos A-D); B.8.8 (`review_requests[]`, `attempts[]`, I-P13); F.8 (caídas) | C-29 (caídas en cada frontera y sucesor en otro host), C-34 |
| A62-V9-03 | §20.5.2; B.9 (`OpenFindings`); B.10.1 (`FindingDispositions`, `Downgrades`, coherencia); B.8.8 (`findings[]`, I-S18, I-P13) | C-35, C-38 (revisión que mantiene el hallazgo, omisión, cierre de otro id, resultado de otra versión, rebaja por otra autoridad, REVIEWER que cierra) |
| A62-V9-04 | §20.6 (tres identidades, constantes y topes derivados, comprobación antes de reservar); B.2; B.9 | C-34 (cuarto intento rechazado antes de lanzar), C-36 |
| A62-V9-05 | §2; §20.7 (correspondencia cerrada); B.1; B.10.0-B.10.2; §13 (P-20) | C-30 (e), C-38 |
| A62-V9-06 | §20.5 (puntos durables por fase); B.8.8 (I-P13); F.8 pasos 10-14 | C-31 (positivo completo), C-38 (negativo con cambio tardío) |
| GAP-07 | §20.3.1; B.1; B.7 (`ReadAudit`); B.9; §11.1; §11.4; §13 (P-22) | C-41 (a)-(c), (f) |
| GAP-08 | §20.3.2; B.8.8 (`runtime_evidence`); B.10.0; §13 (P-23) | C-41 (d), (e) |
| A62-V9-O01 | §19 | — |
| A62-V9-O02 | §17 (FINAL_CANDIDATE_SHA); D.5 | — |

## 5. Riesgos y preguntas para la revisión

1. **Materialización.** ¿Bastan los diez campos de §20.5.1 para que la materialización no cree autoridad, o falta alguno? ¿Es correcto que el validador
   **reproduzca** la comprobación desde el preflight custodiado?
2. **Write-ahead.** El orden BUDGET_RESERVED → LAUNCHING → lanzamiento cuesta dos QU antes de cada intento. ¿Es aceptable a cambio de distinguir el caso A
   del caso B sin observación externa?
3. **Cierre conservador.** En roles de solo lectura, LAUNCH_UNCERTAIN permite el intento siguiente sin acreditar la terminación del anterior. ¿Es aceptable,
   dado que un resultado tardío nunca se ingiere?
4. **Constantes que coinciden.** Con los valores propuestos, `MAX_REVIEW_ROUNDS` = `MAX_ARCHITECT_LOGICAL_REQUESTS` y la corrección por linaje coincide con
   el tope derivado de correcciones. Se conservan para que la autorización pueda bajarlas por separado. ¿Basta esa justificación?
5. **Rebaja.** «Solo el emisor» rebaja (LIFECYCLE §5), y el emisor es la revisión concreta (`opened_in`). En un bucle con Architects nuevos por ronda, la
   rebaja casi nunca será posible y el camino normal es el cierre por corrección comprobada. ¿Es la lectura correcta de LIFECYCLE?
6. **Opción B para `dotnet test`.** La regla de solo lectura de §20.3.1 se propone para §16 bajo OD-1. Hasta su vigencia, cada invocación necesita una
   exención explícita. ¿Es la autoridad correcta, o la exención debe ser otra?
7. **REVIEWER por contrato de gate.** V10 permite materializar bindings de REVIEWER si el contrato de gate lo autoriza (B.7). ¿Está dentro del alcance de
   A62-V9-01 y de R62-AUTO-10, o debería quedar fuera?

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor: incluye la orquestación autónoma (§20), con la materialización autorizada, los contratos por rol, el cierre de insumos y su regla de solo lectura | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas (A, B, FX-04b, FX-06 con `codex-cli`) |
| OD-3 | autenticar Claude CLI | B; FX-06 si el Architect es `claude-cli` |
| OD-4 | sandbox de Codex para escritura | B y FX-04b |
| OD-5 | permiso de ensayo con la semántica única de §15, incluida la apertura y el consumo de FX-06 | F6 |
| OD-7 | remoto del fixture con CI | cierre de F6, FX-04b y FX-06 (paso 5) |

La resolución de `dotnet test` para la invocación de esta revisión (§0) no es una OD del diseño: es un parámetro de la autorización de la invocación.

## 7. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.
