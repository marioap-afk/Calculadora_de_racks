I-64 — F2-T2A / IDENTITY BRIDGE
COORDINATOR ORDER
Create:
I-64-A-13
A-13 corrects / supersedes A-12 FOR ACTIVATION ONLY.
A-12 remains:
HISTORICAL
CHANGES REQUIRED
NOT ACTIVE
IMMUTABLE
Temporary branch remains:
architecture/workspace-persistente-rackcad-a12
Current temporary-branch tip:
4206b2da3a9985f429b77480cd5b5d26377f3215
Product branch must remain:
architecture/workspace-persistente-rackcad
44a692a4ec386e559cc6d330b3d78b87dbad6581
ACCEPT all five REQUIRED findings.
Treat them as two root causes:

1. RELAY SCHEMA VALIDATION
2. GIT NONEXISTENCE FAIL-OPEN

The other findings are consequences/test gaps of those two.
No product change.
No global I-61 change.
No Owner decision required for the correction itself.
Before HandoffIntegrity can PASS:
the real/mutated relay record used by the bridge must be validated against
the official relay-record schema.
Pin the schema by exact Git blob from:
origin/main a140eb3a
Use the official rackcad-relay-record/v1 schema.
Validation must enforce:

* all required fields;
* exact types;
* enums/patterns;
* nested object constraints;
* additionalProperties = false wherever defined;
* no unsupported JSON constants;
* strict fail-closed parsing.

Do NOT implement a partial ad-hoc relay validator.
The verifier may perform additional semantic checks after schema validation,
but never instead of schema validation.
Replace the current invalid relay fixture.
The baseline synthetic relay fixture must itself be fully schema-valid.
Add at minimum:

* valid relay -> PASS;
* missing required field -> FAIL;
* forbidden additional property -> FAIL;
* wrong scalar type -> FAIL;
* wrong nested type -> FAIL;
* malformed JSON -> FAIL;
* invalid enum/pattern where applicable -> FAIL.

Add mutants that disable each protection.
Do NOT treat:
rc != 0
from git cat-file as proof that an object does not exist.
A-13 must distinguish:
OBJECT MISSING
from:
GIT OPERATION ERROR
A PASS-worthy nc1 nonexistent-object determination requires a successful,
unambiguous query whose semantic result is exactly:
missing
for the candidate object.
Any of the following must FAIL CLOSED:

* Git process error;
* unsupported return code;
* stderr indicating repository failure;
* malformed output;
* empty/partial response;
* unexpected extra output;
* repository unavailable;
* object query command not executed successfully.

Do not infer "missing" from process failure.
Use a Git query with explicit batch/check semantics that can distinguish
"missing" in stdout from command failure.
For example, a hardened:
git cat-file --batch-check
or equivalent exact object-query mechanism.
Required principle:
command success + exact missing result
=> object nonexistent
anything else
=> FAIL CLOSED
Preserve the hardened Git environment already inherited from A-11/A-4 where
applicable.
Add synthetic/adversarial cases at minimum:

* nonexistent object, successful query -> expected Identity failure;
* existing object -> control FAIL;
* Git return code 128 -> FAIL CLOSED;
* Git return code 129 -> FAIL CLOSED;
* Git executable failure -> FAIL CLOSED;
* malformed stdout -> FAIL CLOSED;
* unexpected stderr with otherwise plausible output -> FAIL CLOSED;
* incomplete batch response -> FAIL CLOSED;
* extra output -> FAIL CLOSED.

Add mutants for:

* rc != 0 means missing;
* stderr ignored;
* malformed output accepted;
* incomplete response accepted.

A-13 must restore the guarantee:
No ControlResult PASS is possible when:

* relay schema validity is unknown;
* repository query success is unknown;
* object existence is ambiguous;
* a required input is unreadable;
* any second mutation exists.

AUTHORIZED:
ONE schema-only pre-check of the bound real relay file:
SHA-256
b687bdd82f286757ed90e50b0f01b977ec4fcad6eb0e00b7e017ba05e3a1852f
Purpose:
determine only whether those bytes satisfy the official
rackcad-relay-record/v1 schema.
This pre-check may occur BEFORE A-13 activation.
It must:

* verify the file SHA-256 first;
* use the exact official schema blob;
* parse/validate only;
* perform no Git repository query;
* perform no HandoffIntegrity semantic evaluation;
* perform no Identity evaluation;
* perform no bridge execution;
* produce only SCHEMA_VALID or SCHEMA_INVALID plus validation diagnostics.

This is NOT counted as the single real A-13 bridge application.
If SCHEMA_INVALID:
STOP.
Do not activate or execute A-13 against the real occurrence.
Return to Coordinator because the occurrence binding itself is incompatible
with the substitute path.
If SCHEMA_VALID:
record that fact and continue normal A-13 preparation/review.
Do not inspect unrelated real occurrence inputs under this authorization.
After corrections:
run complete synthetic self-test twice.
Required:
100% PASS
byte-identical outputs
Run regression twice.
Required:
PASS
byte-identical outputs
All historical mutants plus the new relay/Git mutants must fail their target
cases.
Create:
docs/initiatives/I-64-A-13.md
State:
Corrects / supersedes for activation:
I-64-A-12
A-12 remains immutable.
Cite Architect review:
R20261109T233319Z-e044
Map:
I64-A12-RELAY-SCHEMA
I64-A12-NONEXISTENCE-ERROR
I64-A12-FAIL-CLOSED-GUARANTEE
I64-A12-ADVERSARIAL-GAPS
I64-A12-UNVERIFIABLE-PASS
to the two root-cause corrections and exact regressions.
Materiality remains:
M-01 NO
M-02 NO
M-03 NO
M-04 YES
M-05 NO
M-06 NO
M-07 NO
M-08 NO
Version A-13 on the SAME temporary side branch.
Do not touch the product branch.
After versioning:
push
exact-SHA CI
Product branch must still be:
44a692a4...
After:

* A-13 versioned;
* schema-only pre-check result recorded;
* synthetic suite PASS twice;
* regression PASS twice;
* exact-SHA CI complete;

run ONE formal Architect review with the existing accredited:
gpt-6.1-sol / high / read-only
cell.
No new probe required if CLI/config unchanged.
Architect must specifically close or retain:

* RELAY-SCHEMA
* NONEXISTENCE-ERROR
* FAIL-CLOSED-GUARANTEE
* ADVERSARIAL-GAPS
* UNVERIFIABLE-PASS

and re-check:

* HandoffIntegrity vs Identity separation;
* exact mutation isolation;
* side-branch provenance;
* no real bridge execution before activation;
* no expansion into nc2/nc3;
* M-04.

If Architect = CHANGES REQUIRED:
STOP.
Do not create A-14 automatically.
If Architect = AGREED:
request Owner decision for A-13.
No real bridge execution yet.
If Architect agrees, request exactly:
SATISFIED — authorize I-64-A-13 / I64-IDENTITY-BRIDGE-01 as the corrected
I-64-only substitute evidence path for the exhausted F2-T2A nc1
Handoff/Identity Controller defect, superseding A-12 for activation, without
changing product semantics or global protocol.
Product branch:
44a692a4
FROZEN FOR CONTROLS
A-12:
HISTORICAL / CHANGES REQUIRED / NOT ACTIVE
A-13:
AUTHORIZED TO PREPARE
NOT ACTIVE
nc1:
ORDINARY PATH EXHAUSTED
nc2:
WAITING
nc3:
WAITING
attempts:
2 / 3
F2-T2B:
WAITING
Smoke-NAV:
PENDING
