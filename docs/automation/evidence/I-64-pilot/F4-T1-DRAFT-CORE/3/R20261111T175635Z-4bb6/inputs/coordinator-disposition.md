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
