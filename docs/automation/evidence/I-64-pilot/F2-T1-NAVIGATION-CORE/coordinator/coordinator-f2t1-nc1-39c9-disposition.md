I-64 — F2-T1 NEGATIVE CONTROL nc1 DISPOSITION
Task:
F2-T1-NAVIGATION-CORE
Verified delivery:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Real verification:
R20261108T221347Z-ebfc
Disposition:
EXECUTION_VERIFIED / NONE
14 / 14 checks PASS.
The real verification is ACCEPTED.
Negative-control RunId:
R20261108T222313Z-39c9
Expected oracle:
Identity = fail
FailureClass = Identity
Classification = EXECUTION_BLOCKED
Disposition = STOP
Observed:
FailureClass = Handoff
additional cascade failures in Identity / Ci / Tests
TriggeredStopConditions = S-04
Coordinator disposition:
nc1 39c9 = INVALID_OUTPUT
CONTROL ORACLE NOT MET
The output is schema-valid but materially invalid as a negative-control
result.
Reason:
The nc1 mutation changes only the handoff CurrentSha to a nonexistent SHA.
Under AUTOMATION_PLAN 16.9 and agent-execution/README section 10:

* Handoff validates handoff identity fields and chronology;
* the existence/ancestry/current-repository meaning of CurrentSha belongs to
Identity.

The Controller used the mutated CurrentSha while evaluating Handoff and
therefore shifted the intended failure one check too early.
That violates the relative oracle.
The later Ci / Tests failures are cascade effects of the same artificial
CurrentSha mutation and do not satisfy the nc1 oracle.
The display-name test counting in Tests is also incorrect, but it is
secondary to the Handoff/Identity error.
Do not relabel 39c9 as a passed negative control.
Preserve it as:
SCHEMA_VALID_BUT_ORACLE_INVALID
INVALID_OUTPUT
ONE nc1 retry is AUTHORIZED.
This is a CONTROL-phase INVALID_OUTPUT recovery.
It:

* does not consume attempts;
* does not reopen the Worker;
* does not change the verified product;
* does not change the delegation;
* does not change RED or GREEN;
* does not change the real verification ebfc.

Use:
same verified inputs
same nc1 mutation
new RunId
The retry must apply the exact oracle:
Checks before Identity:
Termination = same Result as ebfc
Handoff = same Result as ebfc
Authority = same Result as ebfc
Contract = same Result as ebfc
Identity:
fail
FailureClass:
Identity
Classification:
EXECUTION_BLOCKED
Disposition:
STOP
Later checks may be not_run or may reflect consequences, but they must not
move FailureClass earlier than Identity.
If the retry again fails this oracle:
STOP to Coordinator.
Do NOT automatically spend another retry on the same defect.
For nc1, nc2 and nc3 use the same corrected test-selection semantics as the
accepted real verification:
FullyQualifiedName~Workspace
means:
case-insensitive substring match against fully qualified class/method
identity,
not display-name argument text.
Reference counts for the unchanged real delivery remain:
GREEN:
724 selected
724 passed
0 failed
RED:
724 selected
673 passed
51 failed
The exact RED composition remains:
50 xUnit assertion failures
1 NullReferenceException in the new RED test surface
Do not rewrite that as 51 assertions.
CONFIRMED:
nc1, nc2 and nc3 may use the same per-invocation launcher override used by
the accepted ebfc verification:
-c allow_login_shell=false
Purpose:
make Controller shell commands use pwsh -NoProfile and avoid the known user
profile / ConstrainedLanguage failure.
Classification:
TOOLING / INVOCATION ENVIRONMENT CONTROL
This is allowed because:

* it is per invocation;
* it does not modify config.toml;
* it does not modify the PowerShell profile;
* it does not modify execution policy;
* it does not modify product or host state;
* ebfc proved the resulting Controller execution is deterministic.

Use the same setting consistently for nc1, nc2 and nc3.
After nc1 PASSES its oracle, nc2 is authorized.
The first lexicographic file in:
git diff --name-only BaseSha..CurrentSha
that nc2 must exclude is:
src/RackCad.Application/Workspace/NavigationActions.cs
The real delegation covers it through the prefix:
src/RackCad.Application/Workspace/
README section 10 therefore requires replacing that prefix with enumeration
of the OTHER changed files previously covered by that prefix.
For this delivery, use the mutation proposed by the session:
remove:
src/RackCad.Application/Workspace/
and retain coverage for the other changed Application/Workspace files with
exact entries:
src/RackCad.Application/Workspace/NavigationFacts.cs
src/RackCad.Application/Workspace/NavigationTargetSet.cs
src/RackCad.Application/Workspace/NavigationTargets.cs
Keep:
tests/RackCad.Tests/Workspace/
unchanged.
Result:
NavigationActions.cs is the only changed file intentionally outside the
mutated AllowedWriteScope.
Expected nc2 oracle:
Scope = fail
FailureClass = Scope
Classification = EXECUTION_BLOCKED
Disposition = STOP
All checks before Scope must retain the same Result as the accepted real
verification.
If the actual diff contains an additional changed file under the removed
prefix not listed above:
STOP before nc2 and return to Coordinator.
Do not silently construct a different mutation.
nc3 remains authorized only after:
nc1 oracle = PASS
and
nc2 oracle = PASS
Use the README mutation only:
insert one gate term into WorkCompleted.
Expected oracle:
FreeText = fail
FailureClass = FreeText
Classification = EXECUTION_REWORK_REQUIRED
Disposition = REWORK
Checks before FreeText must retain the same Result as ebfc.
No bridge is authorized for nc1.
A-4 / I64-SCOPE-BRIDGE-01 remains conditional only on the previously defined
nc2 protocol defect.
A-6 / I64-FREETEXT-BRIDGE-01 remains conditional only on the previously
defined nc3 protocol defect.
Do not invoke either bridge merely because nc1 failed.
Product attempts:
1 / 3
unchanged.
Real verification:
ebfc = accepted VERIFIED
nc1:
39c9 = INVALID_OUTPUT
valid oracle outputs consumed = 0 / 1
Authorized now:
one nc1 retry
If that retry succeeds:
proceed to nc2.
If nc2 succeeds:
proceed to nc3.
No additional Worker invocation.
No product correction.
F2-T1 implementation:
COMPLETE
F2-T1 real verification:
EXECUTION_VERIFIED
Negative controls:
nc4 = PASS
nc1 = pending valid oracle
nc2 = not yet run
nc3 = not yet run
F2-T1 close:
PENDING NEGATIVE CONTROLS
F2-T2:
NOT AUTHORIZED
If nc1 retry, nc2 and nc3 all satisfy their required oracles:
complete custody/state and return at:
COORDINATOR_F2_T1_CLOSE_REQUIRED
If any control does not satisfy its oracle:
STOP immediately and return its exact output.
Do not run the next control after a failed oracle.
