I-64 — A-8 / I64-TESTFILTER-BRIDGE-01
COORDINATOR ARCHITECT ACCREDITATION + REVIEW ORDER
Current A-8 revision:
d4a97950c2757d09f971a66621e7262e385ad818
Exact-SHA CI:
42571951654
4 / 4 SUCCESS
A-8 status:
PROPOSED / NOT ACTIVE
F2-T1 status:
BLOCKED_PROTOCOL_DEPENDENCY
No real F2-T1 evidence may be processed by the A-8 bridge before activation.
The versioned preparation package is ACCEPTED FOR ARCHITECT REVIEW.
Accepted preparation facts:

* docs/initiatives/I-64-A-8.md exists at d4a97950;
* M-04 = YES;
* authority = Owner + Architect + Coordinator;
* verifier is deterministic and I-64-only;
* synthetic self-test = 35 / 35 PASS;
* repeated self-test output is byte-identical;
* four deliberately incorrect counting strategies fail their signature
regression cases;
* no real F2-T1 TRX or handoff was processed by the bridge;
* product src/, tests/ and project files were not modified.

This acceptance authorizes review only.
It does NOT activate A-8.
The historical gpt-6.1-sol/high/read-only Architect measurement does not
transfer from the deleted CLI.
One accreditation probe is AUTHORIZED.
Exact requested cell:
Provider:
OpenAI
Model:
gpt-6.1-sol
Effort:
high
Transport:
Codex CLI / process
Capability:
read / tool-use / effort-applied
Sandbox:
read-only
CLI:
codex-cli 0.167.0-alpha.3.2
binary directory:
bin/f71a721e0e6193a9
binary SHA-256:
4B3286E35D7FE866C6B0EB3F07D7A4CDB5E96489576170DFD6B3A15AA1C67382
Use:
the same minimal read-only accreditation oracle and relay discipline used by
the previous Architect probe a198 and the current-CLI Controller probe 3b8b.
The probe must establish:

1. requested model = effective model = gpt-6.1-sol;
2. requested effort = effective effort = high;
3. read-only Git oracle succeeds;
4. structured output is valid;
5. no write to the worktree;
6. no Git mutation;
7. no AutoCAD;
8. no usage or credit warning;
9. config.toml matches an accepted snapshot immediately before and after;
10. participant termination and relay entry are clean.

Current accepted config set includes Snapshot D.
Probe budget:
ONE invocation maximum.
No automatic retry.
The probe does not consume I-64 product attempts.
If the probe does not PASS:
STOP and return to Coordinator.
Do not run the Architect review.
If and only if the accreditation probe PASSES:
one formal Architect review is AUTHORIZED.
Review mode:
read-only
Model / effort:
the newly accredited gpt-6.1-sol / high cell
Review object:
EXACT d4a97950 A-8 package.
The Architect must review at minimum:

1. A-8 materiality:
M-04 = YES;
all other M flags = NO.
2. Scope:
I-64 only;
substitute protocol evidence only;
no global AUTOMATION_PLAN authority;
no change to product semantics.
3. FullyQualifiedName semantics:
case-insensitive substring over
className + "." + method name.
4. TRX parser:
fail closed;
no ambiguity accepted;
counters and test definitions/results coherent.
5. RequiredTests:
MinSelected handling;
GREEN all selected tests pass;
RED selected failures are preserved;
expected 724 / 724 / 0 and 724 / 673 / 51 are bindings for the later
real occurrence, not synthetic assumptions used to make the verifier pass.
6. ChainRedFiles:
derived independently with hardened Git;
same six accredited files required;
RED -> GREEN must not alter those test files.
7. Mutation exactness:
real and mutated handoffs schema-valid;
only authorized WorkCompleted mutation;
exactly one prohibited 16.10 term introduced;
no compensating change.
8. Relationship with A-6:
reuse of accepted parsing/term semantics is allowed;
A-6 itself is not executed;
A-8 remains a distinct bridge.
9. Synthetic adversarial coverage:
display-name;
class-only;
case-sensitive;
method-only;
Theory argument false-positive;
method-only location true-positive;
malformed/unreadable inputs;
ChainRedFiles failures;
unauthorized additional mutation.
10. Pre-activation boundary:
no real F2-T1 evidence may be processed by the bridge before
Owner + Architect + Coordinator activation.
11. Fail-closed behavior:
any unverifiable required input must produce FAIL, never PASS.

Architect output must be one of:
AGREED
or
CHANGES REQUIRED
with REQUIRED and OPTIONAL findings separated.
Do not activate A-8 merely because the Architect agrees.
Owner SATISFIED is still REQUIRED independently.
Do not infer it from:

* creation of A-8;
* prior A-4 mandate;
* prior A-5/A-6 decision;
* this Coordinator order;
* the Owner relaying messages.

The exact Owner decision must be recorded explicitly.
If Architect = CHANGES REQUIRED:
STOP.
A-8 remains NOT ACTIVE.
Return the exact findings to Coordinator.
If Architect = AGREED but Owner decision is still absent:
STOP at:
OWNER_A8_DECISION_REQUIRED
Do not run the real bridge.
If:
Architect = AGREED
and
Owner = SATISFIED
return to Coordinator for the final A-8 activation decision.
Coordinator AGREED is NOT granted in advance by this order.
Until all three authorities are present:
Owner SATISFIED
Architect AGREED
Coordinator AGREED
do NOT execute I64-TESTFILTER-BRIDGE-01 against:

* real GREEN TRX;
* real RED TRX;
* real Worker handoff;
* real nc3 mutated handoff;
* any other real F2-T1 occurrence evidence.

Synthetic fixtures remain allowed.
F2-T1 product:
COMPLETE
Real verification:
EXECUTION_VERIFIED
nc1:
PASS
nc2:
SATISFIED_UNDER_I64_SCOPE_BRIDGE
nc3:
NORMAL PATH EXHAUSTED
A-8:
PROPOSED / NOT ACTIVE
Architect probe:
AUTHORIZED ONCE
Architect review:
CONDITIONALLY AUTHORIZED AFTER PROBE PASS
Owner:
PENDING EXPLICIT DECISION
Coordinator activation:
PENDING
attempts:
1 / 3
F2-T2:
NOT AUTHORIZED
