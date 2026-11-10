I-64 — F2-T2A-NAVIGATION-HOST
HISTORICAL CONTROL MIRROR
WORKTREE BINDING DISPOSITION
Decision:
OPTION A AUTHORIZED
The previously authorized nc2 mirror execution remains:
UNUSED
It is not consumed by this design correction.
Use an isolated historical Git directory as the source of repository state.
Run the Controller with:
GIT_DIR = <isolated historical mirror git directory>
GIT_WORK_TREE = <real product worktree path>
The Controller process working directory remains:
<real product worktree path>
Therefore the control sees:

* the real expected worktree path;
* the historical Git refs;
* the verified product files;
* unchanged verification inputs.

The real product repository .git directory and its refs must NOT be modified.
Live facts remain:
product branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
live origin/main =
04a7041492d760170ce3f3ba5fceeb29146107cc
Do not:

* update-index in the real repository;
* reset any real ref;
* rewrite origin/main;
* fetch into the historical mirror during controls;
* push from the mirror.

The isolated Git directory must represent the recorded verification frame.
Required historical refs:
HEAD / product branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
origin/product branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
origin/main =
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Construct it only from already available local Git objects.
No network operation is allowed while constructing or using the mirror.
Before spending the single Controller execution, run a read-only pre-flight
under the EXACT environment intended for the Controller:
GIT_DIR=<mirror>
GIT_WORK_TREE=<real worktree>
and from the real worktree directory.
This pre-flight does NOT consume the nc2 Controller execution.
It must prove all of the following using ordinary Git commands:

1. 
git rev-parse HEAD
44a692a4...
2. 
git rev-parse origin/main
a140eb3a...
3. 
git rev-parse origin/architecture/workspace-persistente-rackcad
44a692a4...
4. 
git branch --show-current
architecture/workspace-persistente-rackcad
5. 
git rev-parse --show-toplevel
the real product worktree path expected by delegation.json
6. 
git status --porcelain
clean
7. BaseSha ancestry =
PASS
8. RedSha ancestry =
PASS
9. CurrentSha tree/files used by the Controller are byte-identical to the
real product worktree
10. config.toml remains accepted Snapshot D
11. real product repository refs remain byte-for-byte / value-for-value
unchanged before and after the pre-flight.

If ANY item fails:
STOP.
Do not invoke the Controller.
Before Controller invocation, prove that a child process launched using the
same process-launch mechanism receives:
GIT_DIR
GIT_WORK_TREE
and returns the same five key Git facts:
HEAD
origin/main
origin/product branch
branch name
show-toplevel
This must be a neutral read-only child shell.
It is NOT a Controller invocation.
If the variables are dropped, rewritten or not inherited:
STOP.
The evidence record must preserve BOTH sets of facts:
LiveRemoteFacts:
origin/main = 04a70414...
HistoricalControlRemoteFacts:
origin/main = a140eb3a...
State explicitly:
the historical environment exists only to evaluate a negative control
relative to verification 89a6.
It does not claim that a140eb3a is current main.
Controller inputs remain byte-identical to the accepted verification/control
inputs except for the already-defined nc2 mutation and derived hashes that
belong to nc2 itself.
Do NOT rewrite:
ExpectedWorktree
Worktree
branch
MainSha
or other occurrence identity fields to accommodate the mirror.
No prompt change.
If and only if all pre-flight gates pass:
invoke ONE nc2 Controller run.
Working directory:
real product worktree
Process environment:
GIT_DIR=<historical mirror git dir>
GIT_WORK_TREE=<real product worktree>
Normal Controller prompt.
Same nc2 mutation.
New RunId.
No hints about the expected Scope result.
Required earlier checks:
Termination = pass
Handoff = pass
Authority = pass
Contract = pass
Identity = pass
Remote = pass
Then:
Scope = fail
FailureClass = Scope
Classification = EXECUTION_BLOCKED
Disposition = STOP
Later checks follow the normal relative-oracle rules.
If the Controller instead reproduces the exact accepted historical A-4
signature:
all checks before Scope equal the real verification;
Remote = pass;
Scope = pass despite the excluded path;
and there is no unrelated failure,
then:
STOP
and A-4 may be applied under its existing accepted terms.
Do not broaden A-4.
b454 itself remains:
CONTROL NOT PASSED
and is not retroactively converted into A-4 evidence.
If Identity fails because of:

* worktree path;
* branch;
* GIT_DIR/GIT_WORK_TREE handling;
* any mirror/environment issue;

STOP.
The nc2 execution is consumed.
Do not alter inputs or environment and retry automatically.
Return to Coordinator.
If Remote still sees:
04a70414...
or otherwise fails despite the passed pre-flight:
STOP.
That proves the Controller escaped or ignored the historical Git
environment.
Do not retry.
Only after nc2 closes successfully:
reuse the SAME validated historical-control environment for nc3.
Before nc3, repeat the read-only environment gate.
Do not rebuild or mutate the mirror between controls.
Run normal nc3 with unchanged prompt and its established mutation.
A-6 remains available only under its exact existing conditions.
After nc2 and nc3 are both closed:
delete the ephemeral mirror only after all evidence/hashes required for
custody are recorded.
Deletion of the mirror must not touch the real product repository.
Then continue the already authorized side-branch custody / fast-forward
procedure.
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
ONE HISTORICAL-ENVIRONMENT EXECUTION AUTHORIZED
NOT YET CONSUMED
nc3:
WAITING
product branch:
FROZEN AT 44a692a4
live origin/main:
04a70414
historical control origin/main:
a140eb3a
attempts:
2 / 3
F2-T2B:
WAITING
Smoke-NAV:
PENDING
