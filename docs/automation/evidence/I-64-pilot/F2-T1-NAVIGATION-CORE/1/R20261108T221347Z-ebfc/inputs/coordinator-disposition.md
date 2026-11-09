# I-64 — Disposición del Coordinator sobre el STOP previo a la planificación de F2-T1 (literal)

Recibida el 2026-11-08 por el chat de la sesión responsable, que esperaba con `pre-planning-stop.json` (C-F2T1-01..05). La pegó el
Owner.

```text
I-64 — COORDINATOR F2-T1 PRE-PLANNING STOP DISPOSITION
Task:
F2-T1-NAVIGATION-CORE
Current state:
STOPPED BEFORE CONTROLLER PLANNING
No model invocation occurred.
No Worker was launched.
No attempt was consumed.
No product work occurred.
The originally issued F2-T1 gate-contract is:
SUPERSEDED_BEFORE_FIRST_DELEGATION
Do not use it for planning.
Finding ACCEPTED.
Previously measured Controller CLI:
codex-cli 0.162.0-alpha.17.2
bin/2e5e00daee91c61d
is no longer present.
Current available CLI:
codex-cli 0.167.0-alpha.3.2
binary directory:
bin/f71a721e0e6193a9
binary SHA-256:
4B3286E35D7FE866C6B0EB3F07D7A4CDB5E96489576170DFD6B3A15AA1C67382
The old gpt-6-luna/high/read-only measurement does NOT transfer automatically
to this CLI.
One accreditation probe is AUTHORIZED.
Probe:

* Model: gpt-6-luna
* Effort: high
* Transport: Codex CLI / process
* Sandbox: read-only
* CLI: exact 0.167.0-alpha.3.2 binary above
* Use the same minimal Git/read-only oracle used by probe 515d.
* Structured output required.
* No product write.
* No Git write.
* No AutoCAD.
* No Worker.
* Snapshot D must match immediately before and after the invocation.
* Full effective model and effective effort must be recorded.

Probe budget:
MAXIMUM ONE MODEL INVOCATION.
If the probe:

* completes with the requested effective model/effort;
* satisfies the read-only oracle;
* has no usage/credit warning;
* and Snapshot D remains exact;

then:
CONTROLLER CELL = ACCREDITED FOR F2-T1
for this CLI identity.
If the probe fails, is interrupted, produces invalid output, reports another
model/effort, changes config.toml, or raises a usage/credit warning:
STOP and return to Coordinator.
Do not retry the probe automatically.
This accreditation probe does NOT consume F2-T1 attempts.
Finding ACCEPTED.
The descriptive transport string:
"subagente de la sesion de escritorio"
must not be used as the delegation transport identifier.
The reissued contract must use the canonical machine value:
Worker Cell:
anthropic/subagent/claude-sonnet-5-5
Worker Transport:
subagent
Delegation:
Owner.Kind = subagent
Executor.Transport = subagent
Those values are coherent for A8.
Worker model:
claude-sonnet-5-5
Worker effort:
high
The existing measured write-commit-push cell remains eligible; no Worker
probe is required.
For the Controller accreditation record use:
Controller Cell:
openai/process/gpt-6-luna
Controller Transport:
process
The Controller remains read-only and is not the Worker Owner in the
delegation package.
Finding ACCEPTED.
Reissue the F2-T1 contract using ASCII-only free text.
The ASCII contract is semantically equivalent to the original Coordinator
contract and replaces it before any planning invocation.
Required transformations include:

* accented letters -> unaccented equivalents;
* en/em dash -> "-";
* section sign -> "section";
* typographic quotes -> ASCII quotes when needed.

Paths, SHA values, TaskId, invariant IDs and schema values remain unchanged.
Do not paraphrase or weaken an invariant while transliterating it.
Record:
REISSUE_GATE_CONTRACT_ASCII
and preserve:

* SHA-256 of the original superseded contract;
* SHA-256 of the ASCII reissue;
* a mechanical equivalence record showing that only the approved
transliteration/canonical transport changes plus the dispositions in this
review occurred.

The ASCII reissue must validate against:
rackcad-gate-contract/v1
before planning.
After the Controller probe PASSES, the reissued contract must contain:
Worker:
Cell = anthropic/subagent/claude-sonnet-5-5
Provider = Anthropic
Model = claude-sonnet-5-5
Transport = subagent
Capabilities =
read
tool-use
write-commit-push
effort-applied
TestedEfforts =
high
Controller:
Cell = openai/process/gpt-6-luna
Provider = OpenAI
Model = gpt-6-luna
Transport = process
Capabilities =
read
tool-use
effort-applied
TestedEfforts =
high
The Controller Source field must cite the NEW F2-T1 probe evidence and the
exact CLI identity.
Do not claim the old 515d measurement is the current measurement.
RoutingEnforcement remains:
required
Finding ACCEPTED.
The F2-T1 invocation budget is fixed as follows.
Accreditation probe:

* 1 invocation maximum.
* no automatic retry.

Normal chain:
Controller planning:

* 1 valid planning output for the initial delegation.
* up to 2 reruns only for BLOCKED/transport/INVALID_OUTPUT as permitted by
AUTOMATION_PLAN 16.8.
* a third such failure => STOP P-04.

Worker:

* 1 work invocation per accepted delegation.
* no duplicate Worker for the same delegation.
* a correction requires a new accepted delegation.

Controller verification:

* 1 valid verification output per delivered WorkRunId.
* up to 2 reruns only for BLOCKED/transport/INVALID_OUTPUT under 16.8.
* a third such failure => STOP P-04.

Corrections:

* each authorized correction uses its own:
planning
   * Worker
   * verification
sequence.
* corrections consume attempts according to 16.8.

Negative controls after EXECUTION_VERIFIED:
nc1:

* 1 valid Controller output.
* up to 2 transport/invalid-output reruns.

nc2:

* 1 valid Controller output.
* up to 2 transport/invalid-output reruns.

nc3:

* 1 valid Controller output.
* up to 2 transport/invalid-output reruns.

nc4:

* mechanical acceptance control.
* NO model invocation.
* run once on the first accepted delegation of the chain, as required by
agent-execution/README section 10.

Any invocation beyond these caps:
STOP P-07.
F1 attempts do not carry into F2.
Before issuing the first F2-T1 delegation, version the canonical state for
this new task:
attempts = 1
max_attempts = 3
Therefore the initial delegation must carry:
Attempt = 1
AttemptsRemaining = 2
MaxReworkLoops = 3
ChainBaseSha:
the BaseSha of the first accepted F2-T1 delegation.
Initial:
ChainRedSha = null
ChainRedFiles = []
The CLI accreditation probe does not increment attempts.
A BLOCKED/transport rerun does not increment attempts.
A correction after REWORK, or a STOP whose resolution changes the work,
increments attempts exactly as AUTOMATION_PLAN 16.8 requires.
No resets.
The previous authorized sequence was incomplete.
For F2-T1:
nc4 = REQUIRED before the first real Worker invocation.
After a real:
EXECUTION_VERIFIED
the following are REQUIRED before the delegated task can be returned for
Coordinator close:
nc1
nc2
nc3
They are global controls from agent-execution/README section 10 and are not
F1-only.
They run once against the final EXECUTION_VERIFIED inputs of the chain.
Expected oracles remain exactly those of the README:
nc1 -> Identity -> STOP
nc2 -> Scope -> STOP
nc3 -> FreeText -> REWORK
Do not reinterpret a failed control as PASS.
A-4 / A-6
A-4 and A-6 remain ACTIVE I-64 substitute mechanisms, but they are NOT
mandatory by default for every F2-T1 execution.
Do NOT run them if nc2 and nc3 produce their required valid oracle outputs.
Conditional authorization:
If nc2 reproduces the known Controller protocol defect that historically
prevented the real Scope control from being credited, then:
STOP the normal control classification
and invoke I64-SCOPE-BRIDGE-01 under A-4 once.
If nc3 reproduces the known Controller protocol defect that historically
prevented the real FreeText control from being credited, then:
STOP the normal control classification
and invoke I64-FREETEXT-BRIDGE-01 under A-6 once.
For either bridge:

* preserve the failed nc result as historical evidence;
* do not relabel it PASS;
* one bridge execution maximum for that control in F2-T1;
* no attempt reset;
* no additional bridge rerun without Coordinator disposition.

If nc2/nc3 fail for ANY DIFFERENT reason:
do not apply A-4/A-6 by analogy.
STOP to Coordinator.
Because the current state still carries the completed F1 attempt count, a
state transition commit is REQUIRED before Controller planning.
Order:

0. Run the authorized Controller accreditation probe.
1. If probe PASS, prepare the ASCII reissued gate-contract with the new
measured Controller cell.
2. Update I-64 state to F2-T1:
attempts = 1
max_attempts = 3
next task = F2-T1-NAVIGATION-CORE
gate = F2
3. Commit/push the state transition and Coordinator contract custody as
allowed by the session/documentation rules.
4. Re-run the 16.4 exit checks:
HEAD = origin branch
clean tree
origin/main fresh
Snapshot D exact
process check
no open delegation.
5. Controller planning.
6. Build delegation.
7. Run A1-A8 without short-circuit.
8. Run nc4.
9. Only if the real delegation passes A1-A8 and nc4 produces its required
rejection, invoke the Worker.
10. Worker RED -> push.
11. Worker GREEN -> push.
12. Exact-SHA CI.
13. Controller verification.
14. If EXECUTION_VERIFIED, run nc1, nc2, nc3.
15. Apply A-4/A-6 only under the conditional rules above.
16. Custody/state.
17. Return at COORDINATOR_F2_T1_CLOSE_REQUIRED.

All semantic scope from the original F2-T1 contract remains unchanged.
Allowed product write scope remains only:
src/RackCad.Application/Workspace/
tests/RackCad.Tests/Workspace/
No Plugin.
No UI.
No Domain.
No StructuralAnalysis.
No F3 tabs.
No F4 drafts/editing.
No F6 document-wide browser/inventory.
No AutoCAD.
F2-T2 remains NOT AUTHORIZED.
First action now:
run the ONE authorized gpt-6-luna/high/read-only accreditation probe on
codex-cli 0.167.0-alpha.3.2.
If PASS:
continue without another Coordinator round using this disposition to reissue
the ASCII F2-T1 contract and proceed through the sequence above.
If not PASS:
STOP and return with the probe evidence.
F2-T1 remains open, but no Controller planning is authorized until the probe
passes.
```

=====

I-64 — F2-T1 VERIFICATION 0e1c COORDINATOR DISPOSITION
Task:
F2-T1-NAVIGATION-CORE
Verification RunId:
R20261108T185002Z-0e1c
Reported Controller disposition:
EXECUTION_BLOCKED / STOP
TriggeredStopCondition: S-04
Coordinator disposition:
0e1c = INVALID_OUTPUT
The reported S-04 is NOT accepted as a real STOP.
The Controller's Tests check used the wrong counting semantics.
It counted:
display testName containing "Workspace"
case-sensitive
and obtained:
726
That is not the semantics of the contract filter:
FullyQualifiedName~Workspace
The applicable VSTest semantics are:
className.methodName contains "Workspace"
case-insensitive
Using the same CI TRX files with that semantics gives:
GREEN:
724 selected
724 passed
0 failed
RED:
724 selected
673 passed
51 failed
Those values match the worker handoff.
The Controller's 726 is explained by:
+3 unrelated Theory cases whose DISPLAY names contain "Workspace" only in
their argument text;
-1 legitimate selected test whose fully qualified class/method contains
uppercase "WORKSPACE".
Therefore there is no contradictory evidence.
S-04 = NOT ESTABLISHED.
Do not preserve 0e1c as a valid STOP classification.
Preserve the original output as historical evidence, labeled:
SCHEMA_VALID_BUT_MATERIALLY_FALSE
INVALID_OUTPUT
Do not rewrite its JSON.
Worker delivery remains intact.
RED:
f2761979b89e784ed16220815e250d21bc476aa1
CI:
42511103159
Expected Core failure observed.
GREEN:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
CI:
42511528392
4/4 SUCCESS
Current remote branch:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
No Worker correction is authorized.
No new Worker invocation is authorized.
No attempt is consumed by 0e1c.
attempts remains:
1 / 3
The RED remains acceptable.
Observed:
51 failures in the new F2-T1 test surface.
Of those:
50 = xUnit assertion failures
1 = NullReferenceException inside a new RED test because the skeleton returns
null where the implemented behavior later returns the primary target.
Disposition:
RED_PART = ACCEPTABLE
The NullReferenceException does not make the RED a compilation failure and
does not invalidate the RED because:

* it occurs inside the new F2-T1 RED test surface;
* the RED product is intentionally an incomplete compilable skeleton;
* the failure disappears through the GREEN implementation;
* no production/runtime exception is being accepted as GREEN behavior.

Do not rewrite the historical RED result as "all assertion failures".
Preserve the exact 50 + 1 split.
The two Controller commands returning exit code 1 because user profile.ps1
fails under ConstrainedLanguage are recorded as tooling/environment facts.
They do not establish a denial, product failure or S-04 by themselves because
the Controller invocation completed and produced the structured output.
However, for the verification rerun:
invoke pwsh in the Controller environment with profile loading disabled where
the launcher supports doing so.
Preferred:
pwsh -NoProfile
Do not change system/user PowerShell policy or profile files.
If the Codex CLI itself forces profile loading and this cannot be isolated
without changing persistent host configuration:
record it and continue only if the verification can still obtain all required
read-only evidence deterministically.
No host/profile mutation is authorized.
One verification rerun is AUTHORIZED.
This is:
BLOCKED/INVALID_OUTPUT recovery
not:
REWORK
not:
Worker correction
not:
new attempt
Budget after authorization:
planning valid outputs: 1 / 1
Worker invocations: 1 / 1
verification:
valid usable outputs: 0 / 1
invalid/blocked reruns used: 1 / 2 after the rerun is launched
attempts:
1 / 3
Do NOT create a Worker correction.
Do NOT change the gate contract.
Do NOT change the delegation.
Do NOT change the handoff.
Do NOT change RED or GREEN commits.
The verification rerun must evaluate the SAME delivery.
A narrow verification-analysis record is REQUIRED before rerun.
Record:
analysis.md
with:
Failure:
0e1c counted display testName rather than VSTest FullyQualifiedName filter
semantics.
Correct semantic rule:
FullyQualifiedName~Workspace means case-insensitive substring matching on the
fully qualified class/method identity represented by UnitTest/TestMethod,
not display-name argument text.
Measured result from the same TRX inputs:
GREEN 724/724/0
RED 724/673/51.
Classification:
previous S-04 was a Controller counting error; no contradictory product
evidence exists.
The analysis record must NOT modify product code, tests, delegation or
contract.
Because AUTOMATION_PLAN 16.8 prohibits rewriting a prompt without a
versioned analysis.md:
version this analysis as documentation/evidence only before using any
modified verification prompt.
If the session's current 16.4 write embargo prevents that documentation
commit under its local implementation of the procedure, then do NOT modify
the prompt.
Instead rerun the SAME verification prompt and supply the corrected
test-count semantics through the existing verification input/relay evidence
mechanism without changing the normative prompt.
The important invariant is:
no semantic change to the verification criteria;
only correction of the factual counting method.
If neither route can be done without violating 16.4/16.8:
STOP to Coordinator rather than improvising.
The rerun's Tests check must evaluate RequiredTests using the actual filter
semantics.
Required filter:
FullyQualifiedName~Workspace
Required minimum:
645
Actual delivered selection:
724
Expected GREEN:
selected = 724
passed = 724
failed = 0
Expected RED:
selected = 724
passed = 673
failed = 51
The Controller must not derive selection by searching display names.
It may verify the count from:

* TRX UnitTest/TestMethod fully qualified identity;
* or an equivalent VSTest-compatible reconstruction.

The 51 RED failures must remain attributable to the new F2-T1 tests.
The Coordinator does NOT predeclare EXECUTION_VERIFIED.
The rerun must independently execute all fourteen checks under
AUTOMATION_PLAN 16.9.
If all fourteen pass:
EXECUTION_VERIFIED is eligible.
Then execute:
nc1
nc2
nc3
as already required.
If another check genuinely fails:
preserve that result and follow its normal disposition.
If the rerun again reports the same false 726 contradiction:
STOP to Coordinator.
Do not spend the second remaining transport/INVALID_OUTPUT rerun
automatically on the same defect.
nc1..nc3 remain NOT AUTHORIZED UNTIL an EXECUTION_VERIFIED result exists.
After VERIFIED:
nc1 = required
nc2 = required
nc3 = required
A-4 / A-6 remain conditional exactly as previously disposed.
F2-T1 product delivery:
PENDING VERIFICATION RERUN
Worker delivery:
UNCHANGED
RED:
f2761979b89e784ed16220815e250d21bc476aa1
GREEN:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
F2-T2:
NOT AUTHORIZED
No AutoCAD.
No F3/F4/F6 work.
If verification rerun becomes EXECUTION_VERIFIED:
run nc1..nc3
then return according to the existing F2-T1 close sequence.
If verification rerun is not VERIFIED:
return immediately with its exact result.
Do not launch another Worker.
Do not increment attempts for this INVALID_OUTPUT recovery.
