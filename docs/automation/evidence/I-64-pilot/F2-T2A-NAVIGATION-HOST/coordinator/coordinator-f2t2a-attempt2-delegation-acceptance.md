I-64 — F2-T2A-NAVIGATION-HOST
ATTEMPT 2 DELEGATION ACCEPTANCE
Planning:
R20261109T193332Z-bafe
BaseSha:
53302b85518a8d0eaaa07cf1af5affc97581aa95
Current branch:
53302b85518a8d0eaaa07cf1af5affc97581aa95
origin/main:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
A1-A8:
8 / 8 PASS
nc4:
NOT REPEATED
CORRECT FOR THIS CHAIN
attempts:
2 / 3
Delegation:
R20261109T193332Z-bafe
is:
ACCEPTED
Worker Attempt 2:
AUTHORIZED ONCE
No new planning is required.
No commit is required before Worker invocation.
Accepted difference:
Dimensions.Horizon
previous:
Low
new:
Medium
This does NOT change effective routing.
The task remains:
Deep
and:
NOT Long-horizon
because only:
Horizon = High
forces Long-horizon under the current routing authority.
Therefore:
F2T2-C07 = NOT TRIGGERED
The Horizon reevaluation is accepted as a planning judgment difference.
It does not require a new contract or delegation.
Condition 16 is overridden narrowly for this occurrence.
The condition was intended to prevent a material expansion of the correction.
The only observed difference outside the prepared expected-difference list is:
Dimensions.Horizon: Low -> Medium
That field does not change:

* Objective;
* AllowedWriteScope;
* ForbiddenWriteScope;
* Invariants;
* RequiredTests;
* ExpectedEvidence;
* StopConditions;
* Worker model;
* effort;
* routing level;
* implementation scope;
* correction content.

Therefore:
Condition 16 = SATISFIED BY COORDINATOR DISPOSITION
for bafe only.
Do not generalize this override.
Criterion 22 is ACCEPTED.
The added RED test proving:
ProbedId = null
does NOT make an unreadable member navigable
is required evidence for:
F2T2A-KL-01
and follows directly from the prior Coordinator disposition.
It is NOT scope expansion.
Required behavior:

* unreadable/probe-only member remains non-navigable;
* null ProbedId does not promote it into NavigationTargets;
* target attribution/safety do not depend on ProbedId;
* no new read authority is introduced merely to populate it.

The test belongs within the accepted T2A Core test scope.
Worker must create a NEW RED.
The RED must include the corrected version of:
NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles
so it tests the actual architectural invariant without rejecting the
pre-existing:
InvalidateNavigationCaches
symbol.
The RED must also include Criterion 22 coverage.
RED must compile.
Expected failures must be assertions/behavioral guards, not compilation
failure.
The Attempt-1 saved patch MAY be provided to the Worker as seed/reference.
The Worker owns all decisions about reuse.
The session must not apply it.
The Worker must independently:

* inspect;
* reuse/discard;
* correct;
* test;
* commit RED;
* commit GREEN;
* push.

Immediately before invoking the Worker, re-check:

1. HEAD = remote = 53302b85...
2. origin/main = a140eb3a...
3. tree clean;
4. config.toml = accepted A/B/C/D;
5. 0 conflicting participants;
6. no new session commit since planning;
7. AllowedWriteScope remains bafe exact scope.

If any fails:
STOP.
Do not invoke Worker.
Worker:
claude-sonnet-5-5
Effort:
high
Attempt:
2
One invocation.
BaseSha:
53302b85518a8d0eaaa07cf1af5affc97581aa95
Required sequence:
BaseSha
-> corrected RED commit
-> GREEN commit
-> push
No session-authored commit between planning and Worker.
STOP after the single Attempt-2 Worker invocation.
Do not invoke Attempt 3 automatically.
Then run the normal Controller verification chain.
If real verification = EXECUTION_VERIFIED:
proceed with normal controls.
If REWORK/BLOCKED/STOP:
return to Coordinator.
F2-T1:
CLOSED / PASS
F2-T2A:
ATTEMPT 1 = REWORK
ATTEMPT 2 = DELEGATION ACCEPTED
WORKER AUTHORIZED ONCE
attempts:
2 / 3
nc4:
already satisfied for T2A
F2-T2B:
WAITING
Smoke-NAV:
PENDING
