I-64 — F2-T2A-NAVIGATION-HOST
A-13 PASS / nc2 REMOTE PREEMPTION DISPOSITION
Verified product:
44a692a4ec386e559cc6d330b3d78b87dbad6581
A-13:
REAL APPLICATION PASS
nc1:
SATISFIED_UNDER_I64_IDENTITY_BRIDGE
nc2 run:
R20261110T030457Z-b454
b454 is classified as:
CONTROL NOT PASSED
Reason:
the nc2 relative oracle was pre-empted by:
Remote = fail
FailureClass = Remote
S-13
because the Controller consulted the current live:
origin/main = 04a70414...
instead of remaining confined to the remote-state facts recorded by the
verified execution.
Therefore b454 is NOT probative of the nc2 Scope oracle.
Its:
Scope = pass
does reproduce the known symptom, but it does NOT satisfy the exact A-4
signature because Remote already differs from the real verification.
Therefore:
A-4 IS NOT APPLIED TO b454.
Preserve b454 exactly.
Option A:
REJECTED
Do not rewrite or temporarily reset:
refs/remotes/origin/main
in the real I-64 repository.
A real remote-tracking ref is evidence.
Do not falsify it for a control.
Option B:
REJECTED
A-4 requires its exact accepted signature.
b454 does not have it.
Option C:
REJECTED
Do not rebase the verified product merely because main advanced after the
verified occurrence.
AUTHORIZED.
Create an EPHEMERAL CONTROL MIRROR outside the product worktree.
Purpose:
execute nc2 and nc3 against the exact repository-state frame to which the
real verification 89a6 and its relative controls refer.
This is NOT:

* the product repository;
* a product branch;
* a rewrite of any real ref;
* a replacement for current live remote evidence.

The live product repository must remain untouched.
Build the mirror only from local Git object data already present in the
repository.
Do not fetch into it during a control.
The mirror must contain the same commit graph needed by:
BaseSha
RedSha
CurrentSha
recorded origin/main
and the exact verified product tree.
Required historical refs inside the mirror:
HEAD / product branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
origin/product branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
origin/main =
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
These are the repository facts recorded for the verified occurrence.
Before creating the mirror, record separately:
live origin/main =
04a7041492d760170ce3f3ba5fceeb29146107cc
and prove:

* main advanced after the verified occurrence;
* the movement is already known;
* relevant authority blobs are unchanged;
* product branch did not move.

This prevents the mirror from concealing current state.
The control record must explicitly contain BOTH:
LiveRemoteFacts
and:
HistoricalControlRemoteFacts
The mirror must be temporary and isolated.
Do not:

* modify the source repository refs;
* push from the mirror;
* fetch during control execution;
* update remote-tracking refs from network;
* write product commits;
* copy ignored control outputs back as Git objects.

The mirror may be deleted after nc2/nc3 evidence is captured.
Before running nc2:
prove all of the following:

1. CurrentSha object bytes identical;
2. BaseSha object bytes identical;
3. RedSha object bytes identical;
4. relevant tree blobs identical;
5. delegation / contract / verification input bytes identical;
6. config.toml Snapshot D identical;
7. branch name expected by Controller identical;
8. historical origin/main exactly equals the value recorded by 89a6;
9. live product repository remains unchanged and clean.

Record hashes.
If any fails:
STOP.
nc1/nc2/nc3 are synthetic negative controls.
Their purpose is to test the Controller against deliberate mutations
relative to the already accepted real verification.
The current live advancement of main occurred AFTER that verification and is
not itself the mutation under test.
The isolated mirror preserves the historical verification frame without
altering or hiding the live repository.
Authorize ONE fresh nc2 invocation in the isolated historical control mirror.
This is not a retry of b454 caused by Controller-output randomness.
It is a new execution after removing an unrelated environmental preemption.
Use:

* normal Controller prompt;
* same nc2 mutation;
* same verified inputs by bytes;
* new RunId;
* no special hint telling the Controller to fail Scope.

Expected relative oracle:
Termination = pass
Handoff = pass
Authority = pass
Contract = pass
Identity = pass
Remote = pass
Scope = fail
FailureClass = Scope
Classification = EXECUTION_BLOCKED
Disposition = STOP
later checks according to the normal relative-oracle rule.
If the new nc2 instead returns the exact previously accepted historical
defect signature for A-4:
Scope = pass
with all earlier checks equal to the real verification and no unrelated
failure,
STOP and apply A-4 only under its already accepted terms.
Do not broaden A-4.
Record:
nc2 = CONTROL PASS
Then proceed to nc3 in the SAME isolated mirror and same historical
repository-state frame.
Run nc3 normally.
Expected mutation:
FreeText gate term only.
Expected oracle remains the established nc3 relative oracle.
If it reproduces the exact accepted A-6 defect:
A-6 may be used only under its existing terms.
Otherwise follow the ordinary result.
If Remote still fails despite the mirror having:
origin/main = a140eb3a...
STOP.
Do not manipulate more state.
Return exact Controller evidence.
That would demonstrate a different Controller defect.
Throughout nc2/nc3:
real product repository remains:
HEAD =
remote product branch =
44a692a4...
live origin/main remains:
04a70414...
Do not mutate either fact.
When nc2 and nc3 are both closed:
continue the previously authorized side-branch custody procedure.
Do not start T2B until F2-T2A is formally closed.
F2-T1:
CLOSED / PASS
F2-T2A product:
EXECUTION_VERIFIED
A-13:
REAL APPLICATION PASS
nc1:
SATISFIED_UNDER_I64_IDENTITY_BRIDGE
nc2:
b454 CONTROL NOT PASSED
ONE MIRROR EXECUTION AUTHORIZED
nc3:
WAITING
attempts:
2 / 3
product branch:
FROZEN AT 44a692a4
live main:
04a70414
F2-T2B:
WAITING
Smoke-NAV:
PENDING
