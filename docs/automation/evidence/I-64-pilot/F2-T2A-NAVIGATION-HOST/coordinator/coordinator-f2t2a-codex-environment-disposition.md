I-64 — F2-T2A-NAVIGATION-HOST
CODEX ENVIRONMENT CHANGE DISPOSITION
Product branch:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Temporary A-13 branch:
31a1c837a3c63f2e8886ef38c63b68bd52354078
Current main:
04a7041492d760170ce3f3ba5fceeb29146107cc
nc2 historical-environment execution:
AUTHORIZED BUT NOT CONSUMED
Current config SHA-256:
2EEDDAAB28C1147A5D927B4EF1E4C69229197E04CB6B114161FFFF04A16588C8
Decision:
NOT ACCEPTED YET
Do NOT designate it Snapshot E yet.
Reason:
preserving the same 104 key names is insufficient.
The values changed, and those values may affect:

* model selection;
* effort;
* routing;
* sandbox;
* approval policy;
* permissions;
* tool availability;
* limits;
* execution behavior.

Owner acceptance requires semantic evidence of what changed.
Authorize a READ-ONLY comparison of:
Snapshot D
vs
current config.toml
No model invocation required.
Produce a deterministic report containing:

1. total keys in D;
2. total keys in current config;
3. added keys;
4. removed keys;
5. changed-value keys;
6. unchanged-value keys.

For each changed-value key, report:

* exact key path;
* old semantic value;
* new semantic value;

except secrets/tokens/passwords/credentials, for which report only:
CHANGED / UNCHANGED
plus safe type/shape information.
Do not print secret material.
Classify every changed key into one of:
A. cosmetic / UI only
B. telemetry / logging only
C. model / effort / routing
D. sandbox / permission / approval
E. tool / connector capability
F. execution / timeout / resource limit
G. unknown / potentially material
Snapshot E may be accepted only if there are:

* no unexplained changes;
* no secret exposure;
* no incompatible routing changes;
* no permission widening not explicitly approved;
* no sandbox weakening not explicitly approved.

After the comparison:
STOP and return the report.
Do NOT infer Snapshot E acceptance.
A separate Owner decision is required.
Until then:
pre-flight item 10 = FAIL
and nc2 remains blocked.
Old accredited binary:
SHA-256
4B3286E35D7FE866C6B0EB3F07D7A4CDB5E96489576170DFD6B3A15AA1C67382
New binary:
SHA-256
4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17
Version string:
codex-cli 0.167.0-alpha.3.2
Decision:
TREAT AS A NEW CLI BINARY
Same version string does NOT preserve accreditation.
Authorize ONE re-accreditation probe of the Controller cell using the new
binary.
Cell:
gpt-6-luna / high / read-only
Purpose:
prove that the new binary preserves the execution characteristics required
by the Controller protocol.
This is NOT nc2.
It does NOT consume the nc2 control execution.
Measure and record:

* exact binary path;
* SHA-256;
* version string;
* effective model;
* effective effort;
* read-only/sandbox behavior;
* normal termination;
* schema-valid structured output;
* process/relay lifecycle;
* no unexpected Git writes;
* no unexpected participant process;
* no usage/credit warning;
* no routing change.

Use a neutral probe task.
Do not use the real nc2 mutation.
Do not use the real Controller occurrence as the probe subject.
Also authorize ONE neutral Codex-mediated child test under:
GIT_DIR=<historical mirror git dir>
GIT_WORK_TREE=<real product worktree>
using the NEW binary.
It must return only the five already-approved Git facts:

1. HEAD
2. origin/main
3. origin/product branch
4. branch name
5. show-toplevel

Expected historical values:
HEAD =
44a692a4...
origin/main =
a140eb3a...
origin/product branch =
44a692a4...
branch =
architecture/workspace-persistente-rackcad
show-toplevel =
real ExpectedWorktree path
This probe is not nc2.
STOP if the new binary:

* drops GIT_DIR;
* drops GIT_WORK_TREE;
* substitutes live origin/main;
* changes working directory semantics;
* writes repository state;
* changes effective model/effort;
* violates read-only behavior;
* produces invalid structured output;
* introduces an unexplained process/relay behavior.

Do not attempt to compensate automatically.
Do NOT reaccredit:
gpt-6.1-sol / high
yet.
No Architect review is currently pending.
Only reaccredit that cell if a later formal Architect review becomes
necessary.
Execute now only:

1. config D vs current semantic comparison;
2. new-binary Controller neutral probe;
3. Codex-mediated GIT_DIR/GIT_WORK_TREE propagation probe.

No nc2 invocation.
No nc3 invocation.
No product write.
No custody commit to product branch.
Return to:
OWNER_COORDINATOR_CODEX_ENVIRONMENT_ACCEPTANCE_REQUIRED
with:
A. config semantic diff;
B. proposed Snapshot E disposition;
C. new binary probe result;
D. Git environment propagation result.
Only after:
Owner accepts Snapshot E
and
Coordinator accredits the new binary
may pre-flight item 10 be changed from FAIL to PASS.
Then repeat the entire historical-control pre-flight once more before nc2.
F2-T2A product:
EXECUTION_VERIFIED
A-13:
REAL APPLICATION PASS
nc1:
SATISFIED_UNDER_I64_IDENTITY_BRIDGE
nc2:
b454 CONTROL NOT PASSED
historical-environment execution UNUSED
nc3:
WAITING
config.toml:
NEW / NOT ACCEPTED
new Codex binary:
PROBE AUTHORIZED / NOT ACCREDITED
attempts:
2 / 3
F2-T2B:
WAITING
Smoke-NAV:
PENDING
