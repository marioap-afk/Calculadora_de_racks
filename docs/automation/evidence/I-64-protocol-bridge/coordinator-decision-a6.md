# I-64 — Decisión del Coordinator: A-6 / corrección de exactitud numérica de I64-FREETEXT-BRIDGE-01

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras la revisión del Architect
R20261011T022328Z-2809 (CHANGES REQUIRED, 2 REQUIRED) de A-5.

```text
I-64 — COORDINATOR DECISION
A-6 / I64-FREETEXT-BRIDGE-01 NUMERIC EXACTNESS CORRECTION
TARGET_SHA:
6a215c9cd0092c840defc0fcdab1491cde77a801
Current main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Architect review:
R20261011T022328Z-2809
Verdict:
CHANGES REQUIRED
REQUIRED:
I64-A5-NUMERIC-OVERFLOW-FAIL-OPEN
I64-A5-LOSSY-NUMERIC-COMPARISON
Both REQUIRED are ACCEPTED.
They are deterministic defects in the A-5 verifier.
They do NOT require:

* a new Owner decision;
* a global I-61 change;
* I-62 authority;
* a product change;
* a Worker rerun.

Create:
docs/initiatives/I-64-A-6.md
Append-only.
Do NOT edit A-1..A-5.
A-6 corrects A-5 for activation.
A-5 remains historical and immutable.
Applies-to:
I-64
Authority:
Owner + Architect + Coordinator
Owner:
SATISFIED by the existing explicit A-5 Owner decision.
Architect:
REQUIRED on the exact A-6 object.
Coordinator:
PENDING.
Materiality remains:
M-01 = NO
M-02 = NO
M-03 = NO
M-04 = YES
M-05 = NO
M-06 = NO
M-07 = NO
M-08 = NO
No new materiality is introduced.
The verifier must compare JSON evidence without numeric loss.
Do NOT parse evidence numbers into binary floating point.
Use an exact representation.
Preferred approach:

* parse integers as exact Python int;
* parse JSON decimals/exponents into decimal.Decimal using parse_float;
* reject non-finite results explicitly;
* preserve type distinctions where the worker-handoff schema requires them;
* compare values and types exactly.

The implementation must fail closed.
At minimum:

1. `1e999` must NOT become an accepted Infinity-like value;
2. `-1e999` must NOT become accepted non-finite numeric evidence;
3. `1.0` must NOT compare equal to `1.00000000000000001`;
4. `1e-999` must NOT collapse to numeric zero;
5. integer `1` must remain distinct from boolean `true`;
6. integer and decimal representations must respect the exact field types
required by rackcad-worker-handoff/v1;
7. NaN / Infinity / -Infinity tokens remain rejected;
8. any Decimal overflow/invalid numeric condition fails closed;
9. no normalization may hide a second mutation in the handoff.

Do not solve this by rejecting every numeric JSON representation unless the
schema itself forbids it.
The verifier must accept all valid rackcad-worker-handoff/v1 evidence that
A-5 intended to accept.
The fix must be schema-aware.
For fields whose schema requires integer:

* require an actual JSON integer;
* reject boolean;
* reject decimal/exponential representation if the schema does not allow it.

For nullable integer fields:
allow null only where the schema permits it.
For strings / arrays / objects:
retain strict type and structural equality.
For any unconstrained numeric field encountered in nested evidence:

* preserve exact JSON numeric value;
* never round through float.

If the verifier cannot prove equivalence exactly:
FAIL.
Extend the synthetic self-test.
Keep all existing 36 cases.
Add at minimum:
T37:
same structure with `1e999` in an otherwise uninspected numeric location
=> FAIL.
T38:
`-1e999`
=> FAIL.
T39:
REAL `1.0`, MUTATED `1.00000000000000001`
with no authorized relation
=> FAIL.
T40:
REAL `0`, MUTATED `1e-999`
=> FAIL.
T41:
integer `1` versus boolean `true`
=> FAIL.
T42:
integer-required field represented as decimal/exponent form
=> FAIL when schema disallows it.
T43:
a valid large integer inside the permitted schema range/shape
=> handled exactly, not rounded.
T44:
a valid finite decimal in a field where the verifier permits arbitrary JSON
numeric content
=> parsed deterministically and compared exactly.
Also create a regression demonstration against the A-5 verifier blob:
48739ba6328cbee4c425d26ea94a7f471c021bdf
The new tests must expose the A-5 defects.
Then show:
A-5 verifier:
regression suite FAILS.
A-6 verifier:
all tests PASS.
Run the complete self-test twice and require byte-identical output.
Do not change:

* §16.10 term parsing semantics;
* governed fields;
* the exact mutation " GATE PASS.";
* Owner authority;
* nc3 historical disposition;
* applicability only to I-64;
* product state;
* any Scope/A-4 behavior.

A-6 is only the exactness/fail-closed correction required by the Architect.
Preserve the already-declared deviation:
the A-5 verifier was accidentally executed once against the real T2 inputs
before Architect/Coordinator approval.
Do not erase it.
Do not treat its discarded result as evidence.
Do not repeat real-T2 execution during A-6 development or review.
All A-6 testing before agreement must use synthetic data only.
Version:

* A-6;
* corrected verifier;
* updated self-test;
* A-5 regression demonstration;
* reproducible A-6 self-test result;
* evidence/status updates.

Update amendment_refs to include A-6.
Push.
Require exact CI:
4/4 SUCCESS.
After CI success, run ONE formal independent Architect review of A-6.
Profile:
ARCHITECTURE_REVIEW
Semantic effort:
Deep
Use:
gpt-6.1-sol / high / independent read-only CLI
if the cell is still fresh and available.
The review must include:

* Freeze;
* A-5;
* A-6;
* old verifier blob;
* new verifier blob;
* A-5 Architect review R20261011T022328Z-2809;
* exact dispositions of both REQUIRED;
* complete self-test;
* regression demonstration;
* Owner decision;
* Coordinator decision;
* historical T2/nc3 facts.

Architect must explicitly dispose:
I64-A5-NUMERIC-OVERFLOW-FAIL-OPEN
I64-A5-LOSSY-NUMERIC-COMPARISON
Required questions:

1. Are both prior REQUIRED CLOSED?
2. Are JSON numbers now parsed without binary-float loss?
3. Can exponent overflow or underflow create an accepted false equality?
4. Are numeric values compared exactly?
5. Are bool/int and schema-required numeric types kept distinct?
6. Does malformed/non-finite numeric evidence fail closed?
7. Did the fix preserve all nonnumeric A-5 invariants?
8. Is the verifier still limited to the exact FreeText relation?
9. Is the complete §16.10 authority still derived mechanically?
10. Does the verifier contain any new fail-open path?
11. Are M-01..08 unchanged with only M-04 YES?
12. Is any new Owner or I-62 authority required?

Successful result:
AGREED
REQUIRED = 0
If any REQUIRED remains:
STOP Coordinator.
Do not autonomously create A-7.
Even after Architect AGREED:
do NOT execute I64-FREETEXT-BRIDGE-01 against T2.
Stop at:
COORDINATOR_A6_AGREEMENT_REQUIRED
Return:

* A-6 commit/blob;
* corrected verifier blob/SHA-256;
* total self-test count/result;
* regression result against A-5;
* exact CI;
* Architect RunId;
* model/effort;
* verdict;
* REQUIRED count;
* dispositions of both previous REQUIRED;
* M-01..M-08;
* confirmation that real T2 inputs were not used again.

Proceed with A-6.
```
