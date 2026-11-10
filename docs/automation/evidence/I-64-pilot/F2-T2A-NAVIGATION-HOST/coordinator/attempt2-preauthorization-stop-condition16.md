I-64 — F2-T2A-NAVIGATION-HOST
ATTEMPT 2: PRE-AUTHORIZATION NOT MET, CONDITION 16
Disposition: COORDINATOR_F2_T2A_ACCEPTANCE_REQUIRED

Worker: NOT INVOKED (Attempt 2 authorization unused). attempts: 2 / 3.
Session Git writes since 53302b85: NONE. Planning, A1-A8 and this record stay in ignored artifacts.

BaseSha established: 53302b85518a8d0eaaa07cf1af5affc97581aa95
- docs/evidence/state only (0 paths outside docs/)
- CI 42683654231: Tests failure, Build UI success, UI Tests success, Build Plugin skipped
- Core failure set identical to the Attempt 1 RED set (46 of 15147); 0 UI failures
- HEAD = remote; tree clean; origin/main a140eb3a; attempts = 2 in HEAD

Planning R20261109T193332Z-bafe: gpt-6-luna/high, normal termination, schema-valid, ASCII.
Delegation SHA-256 175F26C11F6BB837DD3999A6BB7C503E9CCF8DDA8EFC41A640ABA6DBD4514CDB
A1-A8: 8 PASS. nc4: not repeated.

20 conditions: 19 true, 1 false.
Condition 16 (correction limited): FALSE under the mechanical check fixed before the planning ran.

EXACT DIFFERENCE
Delegation field Dimensions.Horizon: 5d33 = Low; bafe = Medium.
It is the only key outside the expected difference list. Controller's reason:
"Horizon=Medium porque F2-T2 se divide en T2A y T2B y esta tarea excluye T2B".
Routing effect measured: none. routing.md: only Horizon High fixes Long-horizon. Effort Deep/high, cell
anthropic/subagent/claude-sonnet-5-5 and TaskClass are unchanged. Condition 4 (Deep, not Long-horizon) is true.
Everything else in condition 16 holds: AcceptanceCriteria are byte-identical to the prepared list; Objective,
StopConditions, ExpectedEvidence and Authorities are identical to the contract; AllowedWriteScope is identical to 5d33.

The session did not widen its own check after seeing the result, and did not interpret the condition.

ONE POINT TO CONFIRM IN THE SAME RULING
Criterion 22 adds a test for F2T2A-KL-01 (ProbedId = null must not make an unreadable member navigable).
It goes beyond the literal "impossible RED assertion". It comes from your earlier disposition ("Tests must establish"),
and no existing test covers it: every unreadable fact in WorkspaceNavigationTargetsTests.cs has a non-null ProbedId.
Location: WorkspaceNavigationHostTests.cs, inside the allowed scope. The session included it.

Decision requested:
Accept delegation R20261109T193332Z-bafe as is (Horizon Medium and criterion 22) and authorize the Worker, or state a change.
If accepted: no new planning and no commit. The session re-checks BaseSha = HEAD = remote, origin/main, config.toml,
clean tree and participants, then invokes the single Attempt 2 Worker.

F2-T2A: ATTEMPT 2 ACCEPTANCE PENDING. F2-T2B: WAITING. Smoke-NAV: PENDING. attempts: 2 / 3.
