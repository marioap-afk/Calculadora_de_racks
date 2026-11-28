I-64 — F2-T2A-NAVIGATION-HOST
OWNER + COORDINATOR CODEX ENVIRONMENT ACCEPTANCE
Product branch:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Temporary A-13 branch:
31a1c837a3c63f2e8886ef38c63b68bd52354078
Current main:
04a7041492d760170ce3f3ba5fceeb29146107cc
Current config.toml:
SHA-256
2EEDDAAB28C1147A5D927B4EF1E4C69229197E04CB6B114161FFFF04A16588C8
is:
ACCEPTED
Designation:
SNAPSHOT E
The prior criterion:
"no unexplained changes"
is WAIVED narrowly for the transition:
Snapshot D -> Snapshot E
because Snapshot D did not preserve value-level evidence and those historical
values are no longer reconstructible.
This waiver means only:
the exact per-key historical delta from D is UNKNOWN.
It does NOT waive:

* structural validation;
* effective model validation;
* effective effort validation;
* sandbox validation;
* permission validation;
* approval-policy validation;
* routing validation;
* secret-protection requirements.

Acceptance is based on the combination of:

1. same 104 ordered key names as Snapshot D;
2. no added keys;
3. no removed keys;
4. no unknown-risk current keys;
5. no secret-shaped values exposed by the semantic inventory;
6. effective Controller behavior matching the accredited b454 environment
on the relevant measured fields;
7. effective model unchanged for the Controller launch:
gpt-6-luna
8. effective reasoning effort unchanged:
high
9. effective sandbox:
read-only
10. approval behavior unchanged;
11. effective permission profile unchanged;
12. Git environment propagation proven under the current configuration;
13. no observed routing change.

The inability to reconstruct Snapshot D values is recorded as a historical
evidence limitation, not silently interpreted as equality.
From this point forward:
Snapshot E is the active accepted configuration baseline.
Record at minimum:

* full config SHA-256;
* ordered key-name list/hash;
* safe semantic inventory;
* classifications A-F/G;
* effective Controller turn-context baseline;
* binary SHA-256 associated with the probe;
* probe evidence hashes.

Do NOT store secrets merely to make future comparisons easier.
Future configuration changes must be compared against Snapshot E rather than
trying to reconstruct Snapshot D.
New binary:
Codex/bin/5449c1f0f26f1f98/codex.exe
SHA-256:
4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17
Version:
codex-cli 0.167.0-alpha.3.2
is:
ACCREDITED
for:
Controller cell
gpt-6-luna / high / read-only
based on neutral probe:
R20261110T034758Z-6a68
Accepted evidence:

* normal termination;
* schema-valid output;
* effective model correct;
* effective effort correct;
* read-only sandbox correct;
* approval behavior correct;
* no unexpected Git write;
* no participant leak;
* no unattributable process;
* no usage/credit warning;
* no routing change;
* relay lifecycle valid;
* binary stable before/after probe;
* GIT_DIR/GIT_WORK_TREE propagation PASS.

Observed new behavior:
shell commands use:
-NoProfile
Decision:
ACCEPTED / NON-MATERIAL FOR THIS CELL
Reason:
the effective Controller semantics remained equivalent in the neutral probe,
and -NoProfile removes dependence on user profile scripts rather than adding
a new authority or permission.
Record it as an environmental behavioral difference between the old and new
binary/config state.
Do not assume causation between:

* the binary update;
* config update;
* -NoProfile behavior.

The source is not proven and does not need to be inferred.
This accreditation applies only to:
gpt-6-luna / high / read-only
for the Controller chain.
It does NOT automatically accredit:
gpt-6.1-sol / high
for a future Architect review.
If another Architect review becomes necessary, probe that cell separately.
Historical-control pre-flight item 10 is now changed from:
FAIL
to:
PASS
provided the re-check immediately before nc2 confirms:
config.toml SHA-256 =
2EEDDAAB28C1147A5D927B4EF1E4C69229197E04CB6B114161FFFF04A16588C8
and:
Codex binary SHA-256 =
4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17
Any further change:
STOP.
The single historical-environment nc2 execution remains:
AUTHORIZED
UNUSED
Before invoking it:
repeat the COMPLETE historical-control pre-flight.
Required again:

1. historical mirror HEAD = 44a692a4;
2. historical origin/main = a140eb3a;
3. historical origin/product branch = 44a692a4;
4. branch name correct;
5. show-toplevel = real ExpectedWorktree;
6. status clean;
7. BaseSha ancestry PASS;
8. RedSha ancestry PASS;
9. product tree byte-equivalent;
10. Snapshot E exact hash PASS;
11. real repository unchanged;
12. new Codex binary exact hash PASS;
13. Codex-mediated GIT_DIR/GIT_WORK_TREE propagation still PASS;
14. no participant/process conflict.

If all pass:
invoke the ONE authorized nc2 Controller execution.
Controller:
new accredited binary
gpt-6-luna / high / read-only
Working directory:
real product worktree
Environment:
GIT_DIR = historical mirror git directory
GIT_WORK_TREE = real product worktree
Prompt:
normal unchanged nc2 Controller prompt
Mutation:
same accepted nc2 Scope mutation
No hint about expected result.
Required:
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
If exact historical A-4 defect instead occurs:
all earlier checks equal real verification
Remote = pass
Scope = pass despite excluded path
no unrelated failure
then:
STOP
A-4 may be applied under its existing terms.
Do not broaden A-4.
If nc2 closes:
repeat the historical-environment read-only gate.
Then proceed to nc3 in the same historical frame.
If nc2 is pre-empted by any new environmental condition:
STOP.
Do not retry automatically.
F2-T2A product:
EXECUTION_VERIFIED
A-13:
REAL APPLICATION PASS
nc1:
SATISFIED_UNDER_I64_IDENTITY_BRIDGE
Snapshot E:
ACCEPTED
new Controller binary:
ACCREDITED
nc2 historical execution:
AUTHORIZED / UNUSED
nc3:
WAITING
product branch:
FROZEN AT 44a692a4
attempts:
2 / 3
F2-T2B:
WAITING
Smoke-NAV:
PENDING
