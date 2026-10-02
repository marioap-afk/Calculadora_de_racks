# I-62 — Dossier de ejecución de F3 (preparación; F3 no está abierto)

> **Preparación, no materialización.** Orden del Coordinator de [decisiones](../../decisions/I-62.md) §33 (clase B). F3 **no** está autorizado: este archivo
> no crea esquemas, pruebas, textos normativos ni manifiestos. Las referencias «V14 §…» son de la Proposal V14 congelada (`4c617e82`, blob `34ad80ea`). Los
> blobs «actuales» son los de `1eddbf48` (F2).

## 1. Resultado congelado de F3 (V14 §17)

Binding, independencia, `binding/v1`, `gate-contract/v2`, `delegation/v2`, A1'-A8' y las 14 comprobaciones (Anexo G); contratos `role-invocation/v1`,
`input-closure/v1`, `input-fidelity/v1`, `architect-review-result/v1` y `reviewer-result/v1` (B.9, B.10). Obligaciones: **C-11..C-14, C-30 y C-40** (C-30 y
C-40 se completan en F4). El manifiesto B.11 de las superficies congeladas es un entregable de F3 «junto con los contratos», y se valida en F4 (§20.3.3,
punto 5).

## 2. Mapa de archivos

Rutas relativas a la raíz. `S/` = `docs/automation/agent-execution/schemas/`. Todas las rutas nuevas siguen el patrón versionado de F2 (`*.v<n>.schema.json`).

| Path | CurrentBlob | FrozenSourceClause | ExpectedAction | OwningObligation | PossibleSiblingConflict |
|---|---|---|---|---|---|
| `S/binding.v1.schema.json` | — | B.5, B.2 (`BindingRef`, `AuthorizationRef`) | ADD | C-11, C-12, C-40 | ninguno |
| `S/gate-contract.v2.schema.json` | — | B.7 (gate-contract/v2), §20.5.1 (`Materialization` del REVIEWER), B.8.6 | ADD | C-11, C-13, C-14 | ninguno |
| `S/delegation.v2.schema.json` | — | B.7 (delegation/v2) | ADD | C-12, C-14, C-30 | ninguno |
| `S/role-invocation.v1.schema.json` | — | B.9, §20.3 | ADD | C-30, C-40 | ninguno |
| `S/input-closure.v1.schema.json` | — | §20.3.1, B.9 (`EffectiveInputClosure`) | ADD | C-30 (f); C-41 en F4 | ninguno |
| `S/input-fidelity.v1.schema.json` | — | §20.3.3 (`CanonicalInputFidelity`), B.9 | ADD | C-42 en F4 | ninguno |
| `S/architect-review-result.v1.schema.json` | — | B.10.0, B.10.1, §20.5.2 | ADD | C-30 (e); C-35 en F4 | ninguno |
| `S/reviewer-result.v1.schema.json` | — | B.10.0, B.10.2 | ADD | C-30 (e) | ninguno |
| `S/normative-dependency-manifest.v1.schema.json` | — | B.11 | ADD | C-42 (q)-(z) en F4 | ninguno |
| `docs/initiatives/I-62-normative-dependency-manifest.json` | — | §20.3.3 punto 5; B.11 («la ruta exacta se fija en F3») | ADD | C-42 en F4 | ninguno |
| `docs/AUTOMATION_PLAN.md` | `f0e0fd31` | §3.1, fila «§16 subsecciones I62: binding y aceptación»; §5; §11.1-11.3; G.1 (deltas de A1-A8 y de las 14 comprobaciones); §13 (P-10 ya está; P-19 y P-20 si las usan los contratos) | MODIFY: subsecciones I62 **nuevas** (16.20 binding y aceptación; 16.21 independencia; 16.22 aceptación A1'-A8' y comprobaciones I62), sin tocar 16.5 ni 16.9 (E.7: secciones nuevas cuando no hace falta cambiar una existente) | C-11..C-14 | ninguno hoy |
| `docs/INITIATIVE_LIFECYCLE.md` | `6415c33b` | §11.4, alternativa 1 (OD-6); §3.1, fila LIFECYCLE | MODIFY §5/§9 (predicado de independencia de las revisiones mayores), plano (b) inactivo. **Gate no explícito en §17**; se propone F3 porque C-13 (6)-(11) es su control | C-13 | ninguno hoy; autoridad caliente (WORKFLOW §7) |
| `docs/automation/agent-execution/README.md` | `94efcb3d` | B.5, §5, §20.3.1, §20.3.3, §20.7 | MODIFY: secciones I62 nuevas (§14 binding y aceptación; §15 invocación de rol, cierre de insumos y preflight de fidelidad; §16 validación de resultados por rol) | C-11..C-14, C-30, C-40 | ninguno |
| `docs/automation/agent-execution/routing.md` | `9dd94dfe` | §5 (algoritmo de binding), E.5 («§§4-5 y §7»), E.7 | MODIFY: sección nueva «§9 Binding por capacidad (I62)» (E.7). Se aparta de la previsión de E.5 (MODIFIED en §§4-5 y 7) sin cambiar el contrato; C-20b lo registra como diferencia justificada por E.7 | C-11 | ninguno |
| `docs/automation/agent-execution/model-catalog.md` | `166d978d` | B.5 (`Cell` = `{CellId, CatalogBlob, AdapterId, Model, EffortSemantic, EffortProvider}`), E.7 | MODIFY: sección nueva con las celdas por adapter (`CellId`) | C-11 | ninguno |
| `tests/RackCad.Tests/PrincipalPortabilityProtocolTests.cs` | `bd9bd2c2` | B.1 (estrictos, neutrales) | MODIFY: la lista `F2CoreSchemas` pasa a cubrir los nueve esquemas de F3 (C-05 y C-01 ampliados a todo el núcleo) | B.1; refuerzo de C-05 | ninguno |
| `docs/automation/evidence/I-62-F3/` | — | Anexo C | ADD: MC de C-11..C-14, C-30 y C-40 | — | ninguno |
| `S/preflight.v1`, `S/relay-record.v2`, `S/controller-verification.v2` | `6a054079`, `2b368f9c`, `bf4fb76d` | B.4, B.7 | VALIDATE (los consumen los contratos de F3) | C-11, C-30 | ninguno |
| `S/*.schema.json` (cinco `/v1`) | blobs de C-19 | C-19 | VALIDATE: sin cambios | C-19 | ninguno |

**AGENTS.md, CLAUDE.md, FOUNDATIONS, HANDOFF, ROADMAP e índice ADR:** sin cambios en F3.

## 3. Planes de esquema

Reglas comunes de B.1:
- JSON Schema 2020-12 y `additionalProperties: false` en todos los objetos; el único punto abierto es `AdapterFacts.Facts` (que no aparece en F3);
- todo campo declarado es obligatorio, y `null` solo significa «no aplica»;
- lo desconocido se expresa con un estado explícito.

Como en F2, sin `$ref`, `oneOf` ni condicionales: las reglas entre campos van en el README (procedimiento de coherencia). IMPLEMENTATION_CHOICE = forma mínima
cuando V14 deja la representación abierta.

### 3.1 `rackcad-binding/v1` (B.5)

| Campo | Forma | Notas |
|---|---|---|
| `Schema`, `BindingId` (`^B…`), `UnitId`, `Scope` (UNIT \| TASK), `TaskId` (nullable solo con UNIT), `Role` (5 roles), `ProtocolSet` | escalares | — |
| `Actor`, `Session` | `ActorRef`, `SessionRef` de F2 | «de la observación» |
| `Cell` | `{CellId, CatalogBlob, AdapterId, Model (nullable), EffortSemantic (5 valores), EffortProvider (nullable)}` | `Model` y `EffortProvider` nullables si el adapter no los controla |
| `PreflightRef` | `{PreflightId, Location {Kind, Path, CommitSha, Blob}, Sha256}` | `Location` como en `BindingRef` (IMPLEMENTATION_CHOICE) |
| `Eligibility` | `{MeasuredInvocation {State (MEASURED \| NOT_MEASURED), RunRef (nullable)}, ConsumptionCovered (OFFICIAL \| MEASURED \| UNKNOWN), CatalogVerifiedOn, Stale (bool)}` | elegible ⇔ MEASURED ∧ ≠ UNKNOWN ∧ `Stale` false (regla de coherencia) |
| `Independence` | `{Requirements[] {ReferenceRole, ReferenceActor, Actor, Session, Context, Provider}, Satisfaction[] {ReferenceRole, Dimension, Result (SATISFIED \| NOT_SATISFIED \| UNKNOWN), Evidence}}` | único por (`ReferenceRole`, `Dimension`); dimensiones = REQUIRED \| PREFERRED \| NOT_REQUIRED |
| `RejectedAlternatives[]`, `RoutingReason`, `EscalationConditions` | listas y texto | — |
| `CountersSnapshot` | `{Attempts, CorrectionLaunches[] {TaskId, FailureClass, Count}, BlockedReruns[] {TaskId, Phase, Count}, RebaseRecoveries[] {TaskId, Count}, Invocations[] {Scope, Launched, Uncertain, Cap}}` | «copia de §9.3» (IMPLEMENTATION_CHOICE de forma) |
| `Custody` | `{FromBindingId (nullable), CessionRunId (nullable)}` | nullables en el primer binding |
| `Acceptance` | `{State (PENDING \| ACCEPTED \| REJECTED), Basis (INDIVIDUAL_DECISION \| AUTHORIZED_MATERIALIZATION \| null), DecisionRef ({Path, Marker} \| null), AuthorizationRef (B.2 \| null), MaterializationCheck ({Criteria[] {CriterionId, Required, Observed, Result, Evidence}} \| null), Utc (nullable)}` | **combinaciones válidas** (coherencia): PENDING ⇒ todo null; INDIVIDUAL_DECISION ⇒ `DecisionRef` ≠ null y `AuthorizationRef` = `MaterializationCheck` = null; AUTHORIZED_MATERIALIZATION ⇒ `DecisionRef` = null, `AuthorizationRef` y `MaterializationCheck` ≠ null con todos los criterios en SATISFIED, solo para ARCHITECT (o REVIEWER con contrato que lo autorice), sin PENDING previo |

Campos con autoridad: `Acceptance` (decisión del Coordinator o autorización), `Independence.Satisfaction`, `Eligibility`. Compatibilidad: esquema nuevo, sin `/v1`
previo.

### 3.2 `rackcad-gate-contract/v2` (B.7)

Campos: todos los de `/v1` (`Schema`, `Unit`, `Gate`, `TaskId`, `Objective`, `AuthorityRevision`, `MainSha`, `Authorities[]`, `AllowedWriteScope`,
`ForbiddenWriteScope`, `Invariants`, `RequiredTests[]`, `ExpectedEvidence`, `StopConditions[]`, `RoutingEnforcement`, `CorrectionsAuthorized`, `IssuedBy`,
`IssuedUtc`) **salvo** `EligibleCells`, que pasa a:
- `RoleRequirements[]`: `{Role, Profile, Action, Mandatory[] (RequirementId), Independence[] {ReferenceRole, Actor, Session, Context, Provider},
  EligibleCells[] (forma de celda de `/v1`), Materialization (nullable)}`, único por `Role`. `Materialization` solo para REVIEWER, con los campos de §20.5.1
  (`Role`, `AuthorizedActions[]`, `MinimumCapabilities[]`, `RequiredIndependence`, `EligibleCells`, `ModelEffortBounds` {MinLevel, MinEffort, MaxLevel
  nullable, MaxEffort nullable}, `Permissions` = READ_ONLY, `Budget`, `ObjectFamily`, `Validity` {FromRecordVersion, Until});
- añadidos: `ProtocolSet`, `MaterializationCloseSha`, `SupersededCommits[] {Sha, WindowSeq}` y `SupersededBaseSha` (nullable ⇔ la lista está vacía).

Combinaciones inválidas: `Materialization` ≠ null con `Role` ≠ REVIEWER; `SupersededBaseSha` null con `SupersededCommits` no vacío (y al revés); dos
`RoleRequirements` con el mismo `Role`. Compatibilidad: `/v1` intacto (C-19); un contrato `/v1` nunca se reemite como `/v2` (E.3).

### 3.3 `rackcad-delegation/v2` (B.7)

Todos los campos de `/v1`. Cambia `Owner.Kind`: `subagent | process` → `session-internal | external-process`. Se añaden `Executor.BindingRef`,
`RoleRequirements[]` (copia del contrato, **nunca** menor: A5' compara `Mandatory` ⊇ y cada dimensión ≥) y `SupersededCommits[]` (copia exacta).
`Executor.Provider` se conserva como texto libre: no es un enum, así que C-05 no lo restringe; el proveedor sale del descriptor del binding (B.6).

### 3.4 `rackcad-role-invocation/v1` (B.9)

Campos de la tabla de B.9:
- `Schema`, `InvocationId`, `LogicalReviewRequestId` (nullable) y `AttemptSeq` (nullable);
- `UnitId`, `Gate`, `TaskId` (nullable) y `ProtocolSet`;
- `RequestedRole` y `Action`, con la pareja cerrada de §20.7;
- `Target` = `{commit, path, blob}`, nullable solo en PLAN;
- `AuthorityRevision`;
- `CanonicalInputs[] {Path, Blob}`, `AllowedTransitiveInputs[] {Path, Blob, RequiredBy {Path, Section, Blob}}` y `AllowedActions[] {Action, RequiredBy, Class
  (ACTION_COMPATIBLE \| EXEMPTED), ExemptionRef, ExemptionScope}`;
- `HealthSignals[] {Kind (PUBLICATION_CI), RunRef, Sha}`;
- `DeclaredRuntimeContext[] {Kind, Source, SizeOrSha256}` y `ForbiddenInputs[]`;
- `EffectiveInputClosure` e `InputFidelityPreflight`, como `StateRef`;
- `OpenFindings[] {FindingId, LineageId, Severity, Issuer}`;
- `RequiredCapabilities[]` e `IndependenceRequirements`;
- `Permissions` (READ_ONLY para ARCHITECT, EXECUTION_CONTROLLER y REVIEWER);
- `Binding` (`BindingRef`) y `OutputContract`, con un valor de la correspondencia cerrada;
- `BudgetSnapshot`, `StopConditions[]`, `EscalationConditions[]` y `Authorization` (`StateRef`).

Combinaciones inválidas (P-19 o rechazo):
- `OutputContract` ≠ el de la pareja;
- `Target` null fuera de PLAN;
- `Permissions` de escritura en un rol de solo lectura;
- EXEMPTED sin `ExemptionRef`.

No hay campo de texto de prompt (16.17).

### 3.5 `rackcad-input-closure/v1` (§20.3.1)

`{Schema, AuthorityRevision, CanonicalInputs[], AutomaticInstructions[] {Path, Blob, Adapter}, Obligations[] {SourcePath, SourceSection, SourceBlob, Text,
Class (READ | ACTION_COMPATIBLE | ACTION_INCOMPATIBLE | CONDITIONAL_NOT_TRIGGERED), Resolution (INCLUDED | ALLOWED | EXEMPTED | NOT_TRIGGERED), Target
(nullable), Authority (nullable), Reason (nullable)}, AllowedTransitiveInputs[], AllowedActions[], ForbiddenInputs[], FixedPointIterations}`. Las formas de
`Obligations` y de `AutomaticInstructions` son IMPLEMENTATION_CHOICE sobre el texto de §20.3.1.

Reglas de coherencia:
- ACTION_INCOMPATIBLE ⇒ EXEMPTED con `Authority`, y sin exención no hay lanzamiento;
- CONDITIONAL_NOT_TRIGGERED ⇒ `Reason`;
- una condición ambigua se trata como READ.

### 3.6 `rackcad-input-fidelity/v1` (§20.3.3)

`{Schema, Phase (PREFLIGHT | POSTRUN), Records[] CanonicalInputFidelity, CorpusCharacters[], DeliveredRepresentationSource}`, con `CanonicalInputFidelity` =
`{InputRef, GitBlob, CanonicalSha256, Encoding {Charset, Bom, LineEnding}, TransportRepresentation, TransportSha256 (nullable), VerificationMethod
(nullable), CharacterClassesChecked[] {Character, CountCanonical, CountTransport}, FidelityStatus (FAITHFUL | FAITHFUL_NORMALIZED | DEGRADED_BOUNDED |
DEGRADED_UNBOUNDED | UNVERIFIED), DegradedSpans (StateRef | null)}`. Exactamente uno de `TransportSha256` y `VerificationMethod` es no nulo (coherencia).
`DegradedSpans` es null con FAITHFUL o FAITHFUL_NORMALIZED.

### 3.7 `rackcad-architect-review-result/v1` (B.10.0 + B.10.1)

**Sobre común:**
- `Schema`, `ResultId`, `LogicalReviewRequestId`, `InvocationId` y `AttemptSeq`;
- `RequestedRole` = ARCHITECT y `Action` = REVIEW_DESIGN;
- `ReviewedUnit`, `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob`;
- `ReviewerBinding`, `ReviewerDeclaredIdentity` (informativa, sin MATCH) y `ReviewerMode`;
- `InjectedContextDeclaration` (obligatoria), `InputsRead[]` e `InputFidelityEvidenceRef`;
- `IndependenceEvidence`, `KnownLimitations[]` y `RecommendedNextAction`.

**Propios:**
- `Verdict` (AGREED \| CHANGES REQUIRED \| BLOCKED — OWNER DECISION);
- `RequiredFindings[]` y `OptionalFindings[]`, con la forma `{FindingId, LineageId (nullable), Source, AffectedSection, Evidence, PremiseRefs[] {Path,
  Section, LineStart, LineEnd, Quote}, WhyItMatters, CorrectionRequired}`;
- `FindingDispositions[] {FindingId, LineageId, State, SupersededBy[], Rationale, EvaluatedObject {commit, path, blob}, PremiseRefs[]}`;
- `Downgrades[]` y `OwnerDecisions[]`.

Coherencia (B.10.1):
- AGREED ⇔ ningún REQUIRED nuevo y ningún REQUIRED abierto sin CLOSED o SUPERSEDED;
- CHANGES REQUIRED ⇒ algún REQUIRED abierto;
- BLOCKED ⇒ `OwnerDecisions` no vacío;
- `EvaluatedObject` = objeto revisado.

### 3.8 `rackcad-reviewer-result/v1` (B.10.0 + B.10.2)

Sobre común con `RequestedRole` = REVIEWER y `Action` = REVIEW_CHANGE. Campos propios:
- `Disposition` (NO_FINDINGS \| FINDINGS);
- `Findings[] {FindingId, LineageId, Severity (BLOCKING \| ADVISORY), AffectedPath, Evidence, Recommendation}`;
- `FindingDispositions[]` (solo linajes REVIEWER);
- `Recommendations[]`.

`additionalProperties: false` impide `Verdict`, AGREED y ARCHITECT_SATISFIED.

### 3.9 `rackcad-normative-dependency-manifest/v1` (B.11): ver §5.

### 3.10 Comprobación local de los planes (borradores fuera del repositorio)

Los nueve esquemas de §3 y §5 se redactaron como borradores locales, en el scratchpad de la sesión y sin commit: F3 no está autorizado. Se comprobaron
con la misma lógica de las guardas de F2. MEASURED:
- los nueve, estrictos (cero objetos abiertos), sin `$ref`, `oneOf` ni condicionales y sin marcas de proveedor en sus enums;
- los nueve compilan con `Test-Json` 2020-12 (PowerShell 7.6.6) y rechazan un objeto vacío por las propiedades requeridas;
- `delegation/v2` se construye mecánicamente sobre el `/v1` real con el delta de B.7;
- **un detalle de patrón:** el campo congelado `DeclaredRuntimeContext[].SizeOrSha256` (B.9) termina en `Sha256`, así que la guarda de patrones de F2
  exigirá un patrón de 64 hex. Al materializarlo se usa `^([0-9]+|[0-9a-f]{64})$` (tamaño o hash), que la satisface sin renombrar el campo.

Conclusión: los planes de campos de F3 se pueden expresar de forma estricta y validable sin cambiar el Freeze.

## 4. Plan de pruebas (RED → GREEN)

Las seis obligaciones de F3 son de clase **(ii) MC** (Anexo C). Se ejecutan con un arnés reproducible en `docs/automation/evidence/I-62-F3/` (como `f2-mc.py`),
con `Test-Json` y las reglas de coherencia del README. Además, una ampliación **(i) Core RG** de C-05/C-01 a los esquemas de F3 (B.1). Un MC no tiene RED de
CI; su «RED» es la ejecución del arnés antes de materializar, que falla por artefacto ausente (registrado), como en F1 y F2.

| Obligación | Prueba focal (MC) | Positivo exacto | Negativos exactos (Anexo C) | Mutaciones de control | Oráculo | RED antes de F3 | GREEN | Archivos |
|---|---|---|---|---|---|---|---|---|
| C-11 | `c11-binding-sin-obligatorio` | binding con todos los obligatorios en MATCH, MEASURED, OFFICIAL y no `Stale` → ACCEPTED (INDIVIDUAL_DECISION) | obligatorio UNKNOWN; `MeasuredInvocation` NOT_MEASURED; cobertura UNKNOWN; requisito omitido (E11) → REJECTED / P-10 | positivo con un criterio cambiado a UNKNOWN → debe pasar a REJECTED (el arnés lo detecta) | B.5 `Eligibility`; §4.2; §5 | `binding.v1.schema.json` ausente | 4 negativos en REJECTED/P-10, positivo ACCEPTED | `S/binding.v1`, README §14, routing §9 |
| C-12 | `c12-referencias` | `BindingRef` CUSTODIED de la misma unidad, tarea y revisión, en un ancestro → A2' pass | otra unidad; otra tarea; otra revisión; obsoleto por rebinding; TRANSIENT presentado como CUSTODIED → rechazo A2' | se quita el «obsoleto»: el caso debe pasar a pass | B.2 (`BindingRef`); G.1 A2 | esquemas F3 ausentes | 5 rechazos A2', 1 pass | `S/binding.v1`, `S/delegation.v2`, README §14 |
| C-13 | `c13-independencia` | casos (1)-(11) del Anexo C con sus esperados exactos; (6)-(11) con OD-6 alternativa 1 | (4) misma instancia con dos `BindingId` → rechazo; (7) autor de la sucesión anterior incluido; (8) UNKNOWN; (9) Controller = Worker de `SupersededCommits` → rechazo; (11) mismo humano operador → rechazo | (2) con el Contexto compartido → Contexto no; (10) con el commit dentro de la unidad → descalifica | §11.1-11.4; B.2 (`ActorRef`, `ReviewSubject`, `AuthorRef`) | `Independence` y texto LIFECYCLE ausentes | los 11 resultados exactos | `S/binding.v1`, `S/gate-contract.v2`, AUTOMATION_PLAN 16.21, LIFECYCLE §5/§9 |
| C-14 | `c14-casos-de-cierre` | filas 1-7 de G.2 (verificación y conteo con 16.9/16.8) con sus esperados exactos. Las filas 8-9 (R-1, R-2) necesitan `state/v2` y van a C-15 (F4); la 10-11 (resolver, PENDING_G0) a C-20c (F4); las 12-27 (bucle y fidelidad), a C-29..C-42 (F4) | según G.2 | se intercambia STOP > REWORK: el arnés debe fallar | G.2; 16.8; 16.9 | `controller-verification/v2` sin reglas I62 de A1'-A8' | 7 filas exactas | `S/delegation.v2`, `S/gate-contract.v2`, AUTOMATION_PLAN 16.22 |
| C-30 | `c30-invocacion-architect` | (d) binding válido con la alternativa 1 y con la 2; (i) PLAN con `delegation/v2` y VERIFY con `controller-verification/v2` → aceptadas | (a) misma sesión que el autor; (b) insumos con la transcripción del autor; (c) sin declaración de contexto; (e) `OutputContract` cruzado; (f) sin `EffectiveInputClosure` → rechazo P-10/P-19; (g) PLAN con `controller-verification/v2`; (h) VERIFY con `delegation/v2` → INVALID (P-19) | se intercambia la tabla de §20.7: (i) debe fallar | §20.3, §20.7, B.9, B.10 | esquemas de invocación y resultados ausentes | resultados exactos de (a)-(i) | `S/role-invocation.v1`, `S/input-closure.v1`, los dos `S/*-result.v1`, README §15-16 |
| C-40 (parte F3) | `c40-materializacion` | (a) Architect nuevo, materializado bajo la autorización vigente → ACCEPTED con `Basis` AUTHORIZED_MATERIALIZATION | (b) siete candidatos no elegibles → rechazo; (c) ninguno satisface → STOP + COORDINATOR_DECISION u OWNER; (d) `DecisionRef` fabricado o sin `AuthorizationRef` → P-20; (h) autorización caducada reutilizada → rechazo | un criterio en UNKNOWN debe contar como NOT_SATISFIED | §20.5.1; B.5; B.2 (`AuthorizationRef`) | `binding.v1` ausente | (a)-(d) y (h) exactos; (e)-(g) e (i)-(n) en F4 (vigencia, intentos) | `S/binding.v1`, `S/role-invocation.v1`, README §14 |
| B.1 en F3 | `I62_C05_*` ampliadas | los nueve esquemas de F3, estrictos y neutrales | mutaciones de C-05 sobre un esquema de F3 | — | B.1 | esquemas ausentes | GREEN | prueba Core |

**Deuda de protocolo de I-64, incorporada como control adicional de C-14** (§7): `c14-scope-sin-pertenencia`. Una verificación con `Scope` en `pass` cuya
`Evidence` no trae la pertenencia reproducible por entrada de `AllowedWriteScope`, o cuyo diff cae fuera del alcance, debe ser rechazada por la regla de
coherencia del README y por la comprobación del Coordinator. Es un control de procedimiento: no cambia el Freeze.

## 5. Plan de materialización de B.11

- **Ruta** (B.11 la deja para F3; IMPLEMENTATION_CHOICE): `docs/initiatives/I-62-normative-dependency-manifest.json` para las superficies congeladas
  (Proposal V14 y los destinos de §3.1), con identidad por blob. Tras la materialización de F4, un segundo manifiesto junto a los documentos materializados:
  `docs/automation/agent-execution/compatibility/I62-normative-dependency-manifest.json`.
- **Esquema:**
  - `{Schema, Unit, AuthorityRevision, Scope[], Entries[]}`;
  - `Entries[]` = `{Source: NormativeUnitRef, DependsOn[] {Target, EdgeKind}, Complete}`;
  - `NormativeUnitRef` = `{Document, UnitId, Anchor, RevisionCommit, RevisionBlob}`;
  - `Target` como **objeto discriminado** (sin `oneOf`): `{Kind (UNIT \| PROPOSED \| WHOLE_DOCUMENT), Unit (NormativeUnitRef \| null), Proposed
    ({FutureDocument, FutureAnchor, DesignSource, State, MaterializedAnchor} \| null), WholeDocument ({Document, Class (BOUNDED_ENTRY_SET \|
    COMPOSITE_RULE), EntrySet[], CompositeRuleRef (nullable)} \| null)}`, con exactamente uno no nulo según `Kind`;
  - `EdgeKind` ∈ ONLY_IF \| EXCEPT \| SUBJECT_TO \| DEFINITION \| ENUM \| INVARIANT \| ALGORITHM \| STATE_TRANSITION \| AUTHORITY \| PRECEDENCE.
- **Identidad canónica** (§20.3.3, punto 1): `UnitId` = el identificador propio de la cláusula (C-n, P-n, T-n, I-S/I-P/I-H n, OD-n, nombre de campo o de estado);
  si no lo tiene, `<ancla>#<tipo><ordinal>` (p. ej., `§20.6#table1.row3`). La proyección de líneas es para comprobar la visibilidad.
- **Validación determinista (con fallo cerrado):** resolución en el orden de los seis pasos de §20.3.3 punto 2; cierre bajo `DependsOn`; ciclos admitidos;
  `Target` informativo, histórico o de ejemplo → inválido; `Complete` false o ausente → INCOMPLETE_METADATA; un documento entero sin conjunto de entrada ni
  regla compuesta → WHOLE_DOCUMENT_UNBOUNDED (destino inválido); inmutable tras el Freeze sin enmienda.
- **Nodo terminal:** cero dependencias de control sin resolver (punto 7), nunca por tamaño ni por ser externo.
- **Destinos propuestos:** las filas de §3.1 se resuelven a su `DesignSource`. Las ya materializadas pasan a MATERIALIZED con su ancla real. A la fecha de F2,
  medido en la rama:
  - MATERIALIZED: el bloque de roles de AUTOMATION_PLAN §16.1 y las subsecciones 16.14 a 16.19; el ADR sucesor (ADR-0048); `routing.md` §8;
  - PROPOSED: §16.13, el puntero de §16.3, las subsecciones de binding, custodia, recuperación, conteo y adopción, la orquestación, §8 (`state/v2`), las
    secciones de WORKFLOW y la de LIFECYCLE.
- **Pruebas relevantes de C-42** (F4 y F6): (q)-(z). El validador de B.11 se usa en (q), (r), (s), (u), (w), (x), (y) y (z), y la resolución de destinos
  propuestos en (t). En F3 se entregan el esquema, el manifiesto y el procedimiento del validador (README); su ejecución exhaustiva es de F4.
- **Riesgo medido:** la clausura literal de V13 abarcó ~8 660 líneas de 19 archivos. El manifiesto acotado a las superficies de I-62 es el remedio de V14, y su
  tamaño se estima al redactarlo: una entrada por unidad de control usada por las premisas.

## 6. DC-07 para F3

MEASURED a las 22:57:25Z y de nuevo antes del commit de preparación. Ramas activas: I-52 `fb6b5648`, I-63 `138bc3d4`, I-64 `39b45f36`; ninguna toca
`agent-execution/`, `AUTOMATION_PLAN`, `INITIATIVE_LIFECYCLE`, `WORKFLOW`, las clases de prueba de los protocolos, `routing.md` ni `model-catalog.md`.

**Acción de coordinación** al abrir F3: repetir `git fetch --prune` y el diff por rama sobre el mapa de §2. Si una hermana empieza a tocar LIFECYCLE (el único
archivo de F3 fuera del área del protocolo), se aplica WORKFLOW §7 (archivos calientes) antes de escribir. **Conflicto previsible en el cierre documental, no en
F3:** el índice ADR (`docs/adr/README.md`) lo modifica ya I-52 (fila 0036), e I-64 añadirá la 0047; la fila 0048 de I-62 se serializa en su cierre.

## 7. Deuda de protocolo de I-64 (control nc2 de Scope): disposición

**Fuente:** `docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/R20261002T184050Z-8415/protocol-debt-handoff.md` (rama de I-64, `39b45f36`), leído sin
modificarlo. **Hecho:** con un `AllowedWriteScope` mutado que excluía el único archivo cambiado, el Controller emitió `Scope` = pass. Su comparación
estructurada había fallado («Only core types are supported in this language mode», ConstrainedLanguage) y no marcó `not_run`. I-63 registró el mismo control
en G2 sin discriminar (DEV-G2-01) y discriminando en G3 (evidencia de I-63 §46).

**Análisis:**
1. **Semántica.** El significado de `Scope` (16.9 n.º 7) es «`git diff --name-only BaseSha..CurrentSha` ⊆ `AllowedWriteScope`». Que un `not_run` cuenta como
   `fail` y que ni stdout ni un JSON conforme bastan (ADR-0046 #6, conservado por ADR-0048) ya está escrito. V14 (G.1) conserva `Scope` y solo lo amplía con
   `SupersededCommits`. **El Controller incumplió una regla existente:** no falta semántica.
2. **Detección.** El control negativo nc2 (README §10) cumplió su función: detectó el defecto y la unidad se detuvo. El protocolo falló cerrado.
3. **Lo que falta es procedimiento.** Ningún texto exige que la `Evidence` de `Scope` contenga la pertenencia por entrada de forma reproducible, ni una
   comparación por el Coordinator antes de aceptar un VERIFIED. V14 asigna al Coordinator las «comprobaciones entre artefactos» de la verificación (B.1), y
   16.1 ya le permite rechazar un VERIFIED.

**Clasificación:**
- **Para las unidades I62: B** (requisito de implementación que F3 satisface sin cambiar el Freeze). Al materializar las 14 comprobaciones y el README de F3:
  - el procedimiento de verificación pide, para `Scope`, la salida reproducible de `git diff --name-only` y la pertenencia de cada ruta a una entrada del
    alcance; si la comparación no se puede ejecutar, `not_run`;
  - la regla de coherencia del relevo rechaza un `Scope` = pass sin esa evidencia;
  - la comparación mecánica del Coordinator (B.1) se documenta como paso de aceptación.

  Control de prueba: `c14-scope-sin-pertenencia` (§4).
- **Para I-64: D** (deuda de otra unidad). I-64 es una unidad **ANTERIOR**: su `Claim-Id` `614371d5-…` es anterior a cualquier `I62_EFFECTIVE_SHA`, así que
  sigue en I61 toda su vida (V14 §14.1). Además, las reglas de I-62 **no son retroactivas** («Sin efecto retroactivo», §14.1). Ninguna entrega de I-62 puede
  desbloquear a I-64 por la vía A de su registro. Lo que I-64 necesita es la vía B, una enmienda autoritativa sobre el protocolo I61 que defina un control
  sustituto aplicable a I-64. Lo deciden el Master Coordinator y los Coordinators de I-64 y de I-62.
- **No es C:** no hay hueco del Freeze. Un control mecánico obligatorio de la sesión **como STOP** sería una regla nueva (A-n), pero no hace falta: basta la
  regla de evidencia del procedimiento más la comprobación del Coordinator, que ya existe como autoridad.
