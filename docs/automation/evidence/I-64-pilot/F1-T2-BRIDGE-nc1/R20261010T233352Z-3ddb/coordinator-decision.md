# I-64 F1-T2-BRIDGE — Decisión del Coordinator sobre nc1 (R20261010T233352Z-3ddb)

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-10, tras la parada por el nc1
R20261010T233352Z-3ddb. Se versiona en la custodia del cierre de T2.

```text
I-64 — COORDINATOR DECISION
F1-T2-BRIDGE / nc1
CurrentSha:
e2f642b86197441eea0838c8055c1db666606f11
Verified execution:
R20261010T225551Z-41fd
EXECUTION_VERIFIED / NONE
The nc1 output is NOT accepted as a valid negative-control execution.
Classify:
Outcome:
INVALID_OUTPUT
Transport disposition:
BLOCKED
Valid nc1 outputs consumed:
0
Reason:
The output contains materially false verification evidence.
Specifically:

1. Handoff failed because the Controller evaluated the mutated CurrentSha.
AUTOMATION_PLAN 16.9 Handoff does NOT evaluate CurrentSha.
Handoff checks only the delivery presence/validity and the required
TaskId, Attempt, DelegationRunId, RunId and invocation chronology.
CurrentSha belongs to Identity.
2. The Controller stated that FilesChanged contained 4 paths.
The actual copied worker handoff contains 10 paths.
The input itself was not truncated or mutated there.
The apparent 4-path set came from PowerShell display truncation.

Therefore this is a schema-conforming but factually false Controller output.
Under the execution protocol, a conforming but false output is
INVALID_OUTPUT / transport BLOCKED and is not counted as the single valid
negative-control execution.
Do NOT record:
nc1 = FAIL final
for this RunId.
Record instead:
nc1 attempt R20261010T233352Z-3ddb =
INVALID_OUTPUT / NOT A VALID CONTROL OUTPUT
Do not write Git.
Do not custody yet.
Do not create an A-n.
Do not create an IdentityBridge.
HEAD must remain:
e2f642b86197441eea0838c8055c1db666606f11
The accepted closed config set remains:
SNAPSHOT A:
73890CA3319206B85B68A42E23E8D323C45361CFC29E06B3E116EFFF5DAD0FBF
SNAPSHOT B:
64BEEBC7401225A910A80EC166BFD852A8AC9E9E06EC1CCEFB6663DC72AF397A
with the known identical 105-name inventory.
Any third hash or inventory change:
STOP P-01.
Authorize one transport/INVALID_OUTPUT retry of nc1.
Use a NEW RunId.
Use:

* the exact same verified WorkRun inputs;
* the exact same nc1 semantic mutation:
only worker-handoff.CurrentSha -> nonexistent valid 40-hex SHA;
* the same Controller cell:
gpt-6-luna / high / read-only;
* the same CONTROLLER_VERIFICATION prompt.

README §10 still applies:
the prompt may differ only in the input route and invocation RunId.
Do NOT add coaching to the Controller.
Do NOT rewrite the prompt to explain the prior error.
Before counting the retry as a valid control output, mechanically compare
the Controller evidence with the actual input.
At minimum require:
Handoff evidence must NOT:

* use CurrentSha validity as a Handoff criterion;
* claim a FilesChanged cardinality different from the input;
* rely on a truncated display as complete evidence.

The actual mutated handoff must still contain all 10 FilesChanged entries.
Expected nc1 oracle:
Termination = same Result as real verification
Handoff = same Result as real verification
Authority = same Result as real verification
Contract = same Result as real verification
Identity = FAIL
FailureClass:
Identity
Classification:
EXECUTION_BLOCKED
Disposition:
STOP
All later checks follow the relative-control rules.
If retry 1 satisfies the oracle:
nc1 = PASS
Then proceed immediately, without Git writes, to:

1. nc2;
2. I64-SCOPE-BRIDGE-01 REAL + ADVERSARIAL;
3. nc3.

If retry 1 is again schema-conforming but contains demonstrably false
evidence:
classify it INVALID_OUTPUT.
One further transport/INVALID_OUTPUT retry remains under the control limit.
If retry 2 also yields INVALID_OUTPUT:
STOP P-04 for nc1.
If either retry produces a factually sound valid output but genuinely misses
the nc1 oracle:
nc1 = FAIL final
and STOP to Coordinator.
Do NOT prepare an IdentityBridge or A-5 at this stage.
That path is considered only if nc1 produces a factually sound valid output
that fails its oracle and therefore cannot be retried.
The current problem is a false Controller output, not yet a demonstrated
defect in the Identity oracle itself.
Proceed with nc1 retry 1.
```
