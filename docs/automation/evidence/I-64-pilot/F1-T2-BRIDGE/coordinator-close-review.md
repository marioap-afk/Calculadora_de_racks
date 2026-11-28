# I-64 — Revisión del Coordinator del cierre de F1-T2-BRIDGE

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras el cierre documental 32d38688
(CI 38109192296 4/4).

```text
I-64 — COORDINATOR T2 CLOSE REVIEW
TARGET SHA:
32d38688e121b6e48d7a26b63b6fb614c046ca6e
origin/main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Coordinator disposition:
PASS
Record:
F1-T2-BRIDGE =
COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE
ProductStatus =
EXECUTION_VERIFIED / NONE
ProductVerifiedSha =
e2f642b86197441eea0838c8055c1db666606f11
ProtocolClose =
SATISFIED UNDER A-4 + A-6
Preserve historical controls exactly:
nc1 = PASS
Controller nc2 =
CONTROL NOT PASSED
I64-SCOPE-BRIDGE-01 =
PASS
Controller nc3 =
NOT PASSED / P-04
I64-FREETEXT-BRIDGE-01 =
PASS
nc4 =
PASS
Do not rerun any T2 control.
Do not reopen T2 product.
In the next authorized documentation/state commit:

1. set last_evidence_commit to the T2 close commit:

32d38688e121b6e48d7a26b63b6fb614c046ca6e

2. update protocol_debt.STATUS so it no longer states that the Scope defect
blocks I-64.

Preserve the debt itself.
The new meaning must be:

* protocol debt remains active for global/I-62 evolution;
* it no longer blocks I-64 because A-4/A-6 provide the authorized
unit-scoped substitute mechanisms.

No history rewrite.
F1 remains:
IN PROGRESS
Do NOT declare F1 PASS.
Next task:
F1-T3-HOST
The previous gpt-6-luna/high measurement used codex-cli
0.162.0-alpha.2, which no longer exists.
Authorize exactly ONE minimal read-only probe of:
gpt-6-luna
high
Codex CLI 0.162.0-alpha.17.2
Purpose:
re-measure eligibility for T3 planning / verification.
Conditions:

* read-only sandbox;
* structured output;
* deterministic Git oracle;
* no Git write;
* no product mutation;
* no credentials/API purchase;
* no P-06;
* config.toml must remain in the accepted A/B/C set with the known
ordered 105-name inventory.

If PASS:
the cell becomes eligible for T3 planning.
If FAIL:
STOP Coordinator.
After a PASS probe, prepare but do NOT yet execute the T3 gate contract.
Derive it from:

* Freeze V5;
* active A-4 and A-6 only where applicable to protocol controls;
* current I-64 state/evidence;
* current code;
* current main authorities;
* the existing F1 gate-contract proposal.

Conceptual T3 scope includes:

* PaletteSet host;
* WPF root;
* RACKPANEL command;
* active-document binding;
* fine selection context toward the Workspace model;
* focus policy;
* UI/host exception containment;
* host instrumentation.

Do not invent exact file scope from memory.
Derive exact AllowedWriteScope / ForbiddenWriteScope from the authorities
and current repository.
Canonical unit state remains:
attempts = 3
max_attempts = 3
As with T2:
this does NOT prevent the first delegation of a new TaskId.
However:
AttemptsRemaining = 0
Therefore any valid T3 result requiring correction would STOP S-11 unless a
new authority changes max_attempts.
For contract preparation, assume:
CorrectionsAuthorized = false
Do NOT increase max_attempts.
Do NOT reset attempts.
Propose an explicit invocation budget for T3 and return it to Coordinator
for issuance.
Re-evaluate T3 against routing before fixing the Worker.
Freeze/proposal classifies it as Deep but notes it may become Long-horizon.
Do not automatically reuse claude-sonnet-5-5/high if the actual derived T3
task crosses the Long-horizon threshold.
If Deep:
use an eligible measured Deep write cell.
If Long-horizon:
report the missing eligible Frontera write cell and STOP at the appropriate
authority boundary.
Do not run Smoke-1.
Smoke-1 remains after F1 PASS.
Do not touch:

* AutoCAD host campaign state;
* ACL;
* TRUSTEDPATHS;
* SECURELOAD;
* profile/registry.

Freshly re-check I-52 ownership before T3 contract issuance.
I-65 is active on:
architecture/interop-analisis-estructural
Do not assume its previously reported SHA is current.
Freshly read its remote tip and product/hot-file overlap when preparing T3.
Its previous ROADMAP write window is closed.
After:

1. documentary state correction;
2. gpt-6-luna/high probe;
3. T3 contract derivation;
4. routing classification;
5. hot-file/ownership check;

stop at:
COORDINATOR_T3_GATE_CONTRACT_REQUIRED
Return:

* documentary correction commit + CI;
* probe RunId/result/effective runtime;
* T3 objective;
* exact AllowedWriteScope;
* exact ForbiddenWriteScope;
* invariants;
* RequiredTests / expected RED;
* proposed invocation budget;
* attempts / AttemptsRemaining;
* task class / semantic effort;
* proposed Worker cell and eligibility;
* Controller cell eligibility;
* I-52/I-65 conflicts;
* exact AuthorityRevision/MainSha proposal.

Do NOT invoke T3 planning yet.
```
