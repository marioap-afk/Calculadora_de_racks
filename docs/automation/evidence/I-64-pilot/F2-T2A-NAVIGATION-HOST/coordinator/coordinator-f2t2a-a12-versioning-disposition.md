I-64 — F2-T2A / A-12
VERSIONING DISPOSITION
Decision:
OPTION A AUTHORIZED
Use a temporary side branch created from:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Suggested branch:
architecture/workspace-persistente-rackcad-a12
The main I-64 branch must remain:
architecture/workspace-persistente-rackcad
44a692a4ec386e559cc6d330b3d78b87dbad6581
until:
A-12 is activated and executed
and
nc2
and
nc3
have completed.
This route preserves the normal protocol semantics.
It avoids:

* post-custody Identity exceptions;
* prompt rewrites;
* analysis.md for nc2/nc3 merely because documentation moved HEAD;
* repeating the F2-T1 post-custody failure pattern.

The product branch remains exactly at the verified CurrentSha while the
protocol amendment is developed and reviewed separately.
Authorize creation of exactly one temporary branch:
architecture/workspace-persistente-rackcad-a12
Base:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Purpose:
A-12 protocol amendment only.
Allowed content on that branch:

* docs/initiatives/I-64-A-12.md;
* A-12 deterministic verifier;
* synthetic self-tests;
* regression demonstration;
* synthetic result artifacts;
* A-12 review / authority evidence;
* related protocol/state documentation strictly necessary for A-12.

Forbidden on the temporary branch:

* src/ product changes;
* tests/ product tests;
* GREEN/RED product changes;
* F2-T2A implementation changes;
* F2-T2B;
* any unrelated initiative work.

Until nc2/nc3 complete:
do NOT commit to:
architecture/workspace-persistente-rackcad
Keep:
HEAD = remote = 44a692a4...
This preserves ordinary Controller Identity semantics for:
A-12 real application
nc2
nc3
No post-custody mode is needed.
Because the side-branch route preserves the product branch at CurrentSha:
REMOVE the post-custody Identity mode from the A-12 verifier before
versioning.
A-12 real occurrence semantics are exact:
product branch HEAD =
remote product branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
Real handoff CurrentSha =
44a692a4ec386e559cc6d330b3d78b87dbad6581
nc1-mutated CurrentSha =
the synthetic nonexistent SHA
The bridge must prove:
HandoffIntegrity = PASS
Identity = FAIL
for the mutation.
No ancestor/descendant custody exception.
After removing post-custody mode:
re-run the complete synthetic self-test twice.
Required:
100% PASS
byte-identical outputs.
Re-run regression twice.
Required:
PASS
byte-identical outputs.
All mutants must continue failing their intended cases.
Do not reuse the old result files after verifier semantics change.
Now write:
docs/initiatives/I-64-A-12.md
It must state explicitly:

* A-12 is versioned on the temporary side branch;
* product occurrence remains on 44a692a4;
* the side branch is evidence/authority storage only;
* verifier execution against real evidence uses the product branch as the
repository occurrence;
* verifier bytes are extracted from the A-12 side-branch Git blob;
* no post-custody Identity semantics are used.

Authority remains:
Owner + Architect + Coordinator
Status:
PROPOSED / NOT ACTIVE
Push the temporary branch.
Run exact-SHA CI on its A-12 versioning commit.
CI must establish that the A-12 docs/tools package itself is valid.
Do not interpret product-test failures caused solely by checking out a
protocol-only side branch differently from the same product base.
Because the side branch starts from the GREEN product 44a692a4, normal CI
should remain green unless the A-12 tooling/docs introduce an unrelated
problem.
After A-12 is versioned and exact-SHA CI is complete:
run the formal Architect review using the already accredited:
gpt-6.1-sol / high / read-only
cell, provided:

* CLI binary unchanged;
* config.toml accepted;
* relay/process checks clean.

Review exact A-12 side-branch commit.
Architect must review:

* HandoffIntegrity vs Identity separation;
* exact mutation isolation;
* fail-closed behavior;
* exact product-branch occurrence binding;
* side-branch provenance;
* no global protocol authority;
* no real evidence before activation;
* M-04.

Verdict:
AGREED
or
CHANGES REQUIRED
If Architect = AGREED:
request the exact Owner decision for A-12.
Do not infer it.
After:
Architect AGREED
Owner SATISFIED
return to Coordinator.
Do NOT execute the real A-12 bridge before final Coordinator AGREED.
After activation, execute A-12 once.
Repository occurrence for Identity:
architecture/workspace-persistente-rackcad
HEAD/remote =
44a692a4...
Verifier:
extract exact bytes from the active A-12 side-branch Git blob.
Do NOT switch the product branch to the A-12 commit.
If A-12 PASS:
record:
nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE
Then continue ordinary:
nc2
nc3
with the product branch still at:
44a692a4...
Run nc2 and nc3 before merging A-12 documentation onto the product branch.
This preserves:
HEAD = remote = CurrentSha
and therefore requires:
no Identity exception
no prompt rewrite
no post-custody mode
Use normal protocol.
A-4 / A-6 only under their exact accepted conditions.
Only after:
A-12 real application completed
nc1 closed
nc2 closed
nc3 closed
may the A-12 side-branch documentation be brought into the main I-64 branch.
Required method:
FAST-FORWARD ONLY
provided the product branch has not moved.
Before fast-forward, prove:
product branch tip =
44a692a4...
and:
44a692a4 is the direct ancestry base of the A-12 side branch.
No merge commit.
No rebase.
No cherry-pick.
Fast-forward the I-64 branch to the A-12 side-branch tip.
Then:
push
exact-SHA CI
custody remaining control evidence/state
continue T2A close.
There is one nuance:
nc2/nc3 evidence produced after the A-12 side-branch commit cannot be
included in that already-existing side-branch tip before the controls run.
Therefore after controls:
the session MAY append the nc2/nc3 custody/state documentation to the
temporary A-12 branch while the product branch remains at 44a692a4.
Then perform one final fast-forward of the product branch to that completed
documentation tip.
This still preserves ordinary Identity during the controls.
After:

* product branch fast-forward succeeds;
* remote product branch reflects the final documentation tip;
* exact-SHA CI completes;

delete the temporary A-12 branch.
Do not delete it before the final product-branch fast-forward is verified.
If architecture/workspace-persistente-rackcad moves away from 44a692a4
before nc2/nc3 finish:
STOP.
Do not use the side-branch procedure automatically on the new state.
Return to Coordinator.
F2-T1:
CLOSED / PASS
F2-T2A Attempt 2:
EXECUTION_VERIFIED
nc1 ordinary:
EXHAUSTED
A-12:
SIDE-BRANCH VERSIONING AUTHORIZED
NOT ACTIVE
nc2:
WAITING
nc3:
WAITING
product branch:
MUST REMAIN 44a692a4
attempts:
2 / 3
F2-T2B:
WAITING
Smoke-NAV:
PENDING
