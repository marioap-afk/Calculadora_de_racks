# Autoridad del Owner para la segunda reverificacion de F4-T1 (dos textos literales)

## 1. POST-CUSTODY IDENTITY SUBSTITUTION

I-64 — F4-T1-DRAFT-CORE
POST-CUSTODY IDENTITY SUBSTITUTION
Current branch:
61d83788e7fa8decd861626b7e3ca89c8f23b9b4
Product CurrentSha:
ce73ceb7
RED:
5a27ac8b
Attempt:
3 / 3
Handoff reissue:
F4T1-HANDOFF-REISSUE-01
YES.
Authorize a LIMITED post-custody substitution for:
Identity
only for:

1. the single reverification of the reissued Attempt-3 handoff;
2. nc1;
3. nc2;
4. nc3

for F4-T1.
This does NOT modify AUTOMATION_PLAN globally.
Normal 16.9 Identity requires:
HEAD
remote branch
CurrentSha
That was true when the product delivery was originally verified.
After the REWORK classification, 16.4 correctly caused the session to
version:

* Attempt-3 custody/evidence;
* Coordinator disposition / handoff-reissue authority;

as docs-only commits.
Therefore HEAD is now intentionally above CurrentSha.
Requiring literal HEAD = CurrentSha during the authorized reverification
would turn correct protocol custody into an unavoidable Identity failure.
The product itself has not changed.
For this reverification only, replace the exact:
HEAD = remote = CurrentSha
portion of Identity with:
A. HEAD = remote product branch;
B. CurrentSha is an ancestor of HEAD;
C. every path in:
git diff --name-only CurrentSha..HEAD
is under:
docs/
D. there are ZERO paths under:
src/
tests/
E. there are no other product/build/source changes between CurrentSha and
HEAD;
F. BaseSha ancestry remains valid;
G. RedSha remains on the expected BaseSha..CurrentSha chain;
H. branch identity remains correct;
I. worktree remains correct and clean.
If all hold:
the post-custody product identity is accepted.
This substitution recognizes only:
verified product
+
later session-owned custody / authority documentation.
It does NOT permit:

* product source above CurrentSha;
* tests above CurrentSha;
* build changes above CurrentSha;
* Worker product commits above CurrentSha;
* unrelated initiative code above CurrentSha.

Any such path:
Identity = FAIL
STOP
Before invoking the Controller, prove explicitly:
CurrentSha:
ce73ceb7
is ancestor of:
61d83788...
and:
git diff --name-only ce73ceb7..61d83788
contains only docs/ paths.
Record the exact path list.
Do not rely only on commit messages.
The reissued handoff remains bound to:
CurrentSha = ce73ceb7
Do NOT change CurrentSha to 61d83788.
61d83788 is custody/authority HEAD.
ce73ceb7 remains the product delivery being verified.
This distinction must appear explicitly in the verification input.
Remote continues to require:
HEAD = remote product branch
for the current custody tip.
The product occurrence itself remains:
CurrentSha = ce73ceb7
Do not fabricate an old remote state.
nc1 must remain meaningful under this substitution.
Its deliberately nonexistent CurrentSha must still fail Identity.
For nc1, the substitute ancestry condition becomes:
mutated CurrentSha must be an ancestor of HEAD
which must FAIL.
Therefore the post-custody substitution does NOT weaken nc1.
Expected nc1 oracle remains:
Identity = fail
FailureClass = Identity
EXECUTION_BLOCKED / STOP
subject to the known Handoff/Identity Controller defect and already accepted
A-13 procedure if it reproduces exactly.
For nc2:
use the same post-custody Identity substitution.
The Scope mutation remains the only intended mutation.
Expected:
post-custody Identity = pass
Remote = pass
Scope = fail
If the exact known Scope Controller defect reproduces:
use A-4 only under its existing terms.
For nc3:
use the same post-custody Identity substitution.
Expected:
post-custody Identity = pass
Remote = pass
Scope = pass
FreeText = fail
If the exact known FreeText Controller defect reproduces:
use A-6 only under its existing terms.
This does NOT:

* increment attempts;
* create Attempt 4;
* create a Worker invocation;
* create RED/GREEN;
* modify ce73ceb7.

F4-T1 remains:
Attempt 3 / 3
Now run ONE reverification with:

* corrected handoff;
* CurrentSha = ce73ceb7;
* HEAD/remote = 61d83788;
* this post-custody Identity authority;
* the already authorized narrow Tests oracle for N-1;
* all other 16.9 checks unchanged.

Expected:
14 / 14 PASS
EXECUTION_VERIFIED / NONE
Run:
nc1
nc2
nc3
using this same post-custody Identity rule.
If all controls close:
F4-T1 = CLOSED / PASS
Then begin:
F4-T2
with:
attempts = 1 / 3
and mandatory N-2 anti-aliasing obligation.
If Identity fails because:

* ce73ceb7 is not ancestor of HEAD;
* any non-docs path exists above ce73ceb7;
* local/remote custody tips differ;
* branch/worktree mismatch exists;

STOP.
Do not relax the rule further.
If another check fails for an unrelated reason:
STOP and return exact evidence.
Do not create another handoff sanitation automatically.
F4-T1 product:
ce73ceb7
Attempt:
3 / 3
N-1:
CORRECTED
Handoff reissue:
PRECHECK PASS
Post-custody Identity:
AUTHORIZED
Reverification:
AUTHORIZED ONCE
nc1-nc3:
WAITING
F4-T2:
WAITING
future attempts 1 / 3

## 2. SECOND REVERIFICATION DISPOSITION (sustituye solo la parte de red de la condicion A)

I-64 — F4-T1-DRAFT-CORE
SECOND REVERIFICATION DISPOSITION
Current branch:
61d83788e7fa8decd861626b7e3ca89c8f23b9b4
Product CurrentSha:
ce73ceb7
RED:
5a27ac8b
Attempt:
3 / 3
Snapshot:
G
ACCEPTED
Prior reverification:
4bb6
EXECUTION_BLOCKED / STOP
FailureClass = Identity
4bb6 is preserved as historical evidence.
Its Identity failure is classified as:
REVERIFICATION INPUT / PROMPT DEFECT
not:

* product failure;
* handoff failure;
* RED/GREEN failure;
* actual repository identity failure.

Reason:
the post-custody Identity disposition required remote equality to be
established, but the prompt incorrectly instructed the read-only Controller
to perform:
git ls-remote
itself.
The accredited Controller cell has restricted network.
Under the normal protocol, live remote facts are supplied by the session.
Therefore the network failure is not probative of product Identity.
Authorize exactly ONE additional reverification.
This is the FINAL reverification authorized for the handoff reissue.
No Worker.
No product change.
No new attempt.
No RED/GREEN.
No HEAD movement before invocation.
Replace only the faulty network portion of condition A.
Controller-local requirement:
HEAD
refs/remotes/origin/architecture/workspace-persistente-rackcad
and both equal the current custody tip:
61d83788e7fa8decd861626b7e3ca89c8f23b9b4
The Controller must NOT execute:
git ls-remote
or any other network Git operation.
The actual remote equality is supplied by the SESSION.
Immediately before the Controller invocation:
session performs read-only:
git ls-remote
for the I-64 product branch.
Required:
remote =
61d83788...
Record that fact in the verification input.
After Controller completion:
remeasure it once.
Required:
remote remains =
61d83788...
If remote differs before invocation:
STOP.
Do not invoke Controller.
If remote changes during the cession:
STOP after the Controller result and return to Coordinator.
Conditions B through I remain EXACTLY unchanged.
In particular:
B.
CurrentSha ce73ceb7 is ancestor of HEAD.
C.
ce73ceb7..HEAD contains only docs/ paths.
D.
zero src/ paths.
E.
zero tests/ paths.
F.
BaseSha ancestry valid.
G.
RedSha remains on the expected chain.
H.
branch identity correct.
I.
worktree correct and clean.
No other relaxation.
Continue to distinguish:
Product CurrentSha:
ce73ceb7
Custody / authority HEAD:
61d83788...
Do NOT rewrite the handoff CurrentSha to HEAD.
ce73ceb7 remains the product being verified.
61d83788 remains the docs-only branch tip.
Snapshot G is ACCEPTED.
Probe:
32f7 = PASS
Use G as the config baseline for this reverification and the subsequent
controls.
If config.toml changes again:
STOP.
Do not create Snapshot H automatically.
Use the already reissued handoff.
Do not sanitize or rewrite it again.
FreeText must remain clean.
The handoff structured-diff proof remains part of the evidence.
Use the already authorized narrow Attempt-3 Tests oracle.
Required facts remain:

* RED regression block fails in RedSha;
* block identical RED vs GREEN;
* GREEN modifies only the authorized BOM-rule region;
* exact GREEN CI passes;
* no test/assertion mutation in GREEN.

No change to that exception.
Expected real verification:
14 / 14 PASS
Classification:
EXECUTION_VERIFIED
Disposition:
NONE
Verified product:
ce73ceb7
HEAD/custody tip:
61d83788
Run:
nc1
nc2
nc3
using the SAME post-custody Identity substitution.
Live remote facts continue to be supplied by the session, not queried over
network by the Controller.
Expected:
mutated CurrentSha is not an ancestor of HEAD
Identity = fail
FailureClass = Identity
EXECUTION_BLOCKED / STOP
If the exact historical Handoff/Identity defect reproduces:
A-13 may be used only under its existing accepted terms.
Expected:
post-custody Identity = pass
Remote = pass
Scope = fail
FailureClass = Scope
If the exact historical Scope defect reproduces:
A-4 may be used only under its existing accepted terms.
Expected:
post-custody Identity = pass
Remote = pass
Scope = pass
FreeText = fail
FailureClass = FreeText
Use normal rules / existing accepted bridge only if the exact historical
defect reproduces.
If this second reverification fails any check:
STOP.
Do NOT:

* rewrite the prompt again;
* issue another handoff;
* invoke another Controller reverification;
* create Attempt 4;
* weaken another oracle.

Return exact evidence to Coordinator.
If reverification = VERIFIED
and nc1/nc2/nc3 close:
F4-T1 = CLOSED / PASS
Then:

1. custody Snapshot G evidence;
2. custody handoff reissue;
3. custody both reverifications;
4. custody controls;
5. preserve 4bb6 historical STOP;
6. preserve ce94 historical FreeText REWORK;
7. record final F4-T1 close;
8. start F4-T2 with attempts = 1 / 3.

Mandatory inherited obligation:
N-2 anti-aliasing.
Host-owned/read objects must not be capable of mutating:
InitialDraftState
after DraftSession creation.
The T2 contract must include the adversarial alias-mutation test already
ordered.
F4-T1:
Attempt 3 / 3
Product:
ce73ceb7
N-1:
CORRECTED
Snapshot G:
ACCEPTED
Handoff reissue:
READY
4bb6:
HISTORICAL STOP / PROMPT DEFECT
Second reverification:
AUTHORIZED ONCE
nc1-nc3:
WAITING
F4-T2:
WAITING
future attempts 1 / 3
