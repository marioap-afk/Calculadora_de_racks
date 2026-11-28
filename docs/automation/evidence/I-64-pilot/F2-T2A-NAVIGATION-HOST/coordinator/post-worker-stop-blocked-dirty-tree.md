I-64 — F2-T2A-NAVIGATION-HOST
STOP AFTER WORKER, BEFORE CONTROLLER VERIFICATION
Disposition: COORDINATOR_F2_T2A_WORKER_BLOCKED_DECISION_REQUIRED

Controller verification: NOT RUN
nc1-nc3: NOT RUN (README section 10: only on a VERIFIED verification)
Second Worker: NOT INVOKED
Session Git writes since d9d386e9: NONE

Recovery chain:
- Replanning R20261109T174847Z-5d33: gpt-6-luna/high, normal termination, A1-A8 8 pass
- nc4 R20261109T175144Z-6a04: relative oracle MET
- Mechanical equivalence: 24 of 24 true => ACCEPTED_BY_PREAUTHORIZED_MECHANICAL_EQUIVALENCE
- Delegation SHA-256 43B0AA1AE81FDD32A7B61EF7E8384AB51D90341D9FCB533BFBD3E4147F172053
- Worker R20261109T175557Z-a639: claude-sonnet-5-5/high, 187 turns, 0 denials; prompt byte-identical (987467B0...)
- Worker authorization: CONSUMED (1 of 1). attempts: 1 / 3

Worker outcome: WorkerStatus BLOCKED / Disposition BLOCKED
BaseSha d9d386e91aa6df5db64fbb6946b7edb3542baace
RedSha = CurrentSha = HEAD = remote = 4fcea652d413414552d52f70998ce16f94f4c31b
No GREEN commit. RED: 13 paths, all inside AllowedWriteScope; Sonnet 5.5 trailer.
origin/main a140eb3a (unchanged); config.toml Snapshot D; 0 participants.

Cause (confirmed by the session in git show 4fcea652):
WorkspaceNavigationEchoTests.NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles (line 331)
asserts that WorkspaceSession.cs does not match "Echo|Navigation". That out-of-scope file contains
DrainOperation.InvalidateNavigationCaches (line 54). The test can never pass, and the GREEN must not
edit a RED test, so the Worker stopped as instructed.

Three facts block the normal verification chain:
1. Dirty tree. 8 tracked files are modified and uncommitted, all inside AllowedWriteScope: the Worker's
   full implementation. AUTOMATION_PLAN 16.4 step 1 requires a clean tree before ceding to a participant;
   step 5 forbids session Git writes until verification ends. The session did not restore or discard it.
   Saved copy: artifacts/.../1/R20261109T175557Z-a639/green-uncommitted-worktree.patch
   SHA-256 32C19A574CCF85E513309EF62116A29876EBFCDF8796579043A0B1209E44C046, byte-identical to git diff HEAD.
2. No CI evidence for the RED. Push run 42673031307 ended "failure" in 55 s with 0 jobs and 0 artifacts.
   GitHub annotation: Internal server error, Correlation ID 2c9d27d3-c894-417f-b3f2-c37a70fe3f2a.
   ci.yml blob unchanged from d9d386e9 (2012c746). Not re-run.
3. No GREEN SHA exists to verify.

Worker-reported, not verified by the session (uncommitted tree):
Core filter 755 selected, 754 passed, 1 failed (the defective test); full Core 15146 of 15147;
UI filter 64/64; Plugin Release build 0 errors, only the 2 baseline MSB3277.
RED run: Core filter 755 selected, 46 failed, 709 passed; UI filter 64/64.

Other findings:
- Full UI suite in Release: 2 failures, PendingTextFieldTests.ShowOnADirtyField_*. Same two already recorded at
  F1-T3B close: an #if DEBUG check from I-43 (07ebcd33); CI runs Debug. Unrelated to this task.
- Worker finding, design point: WorkspaceNavigationFacts cannot fill NavigationDefinitionFact.ProbedId
  (ScanEnvelopes exposes no raw envelope JSON; RackBlockData/RackEmbedStore are forbidden outside the reader),
  so this adapter never yields an UnreadableMember diagnostic.

Options:
(A) RECOMMENDED:
    1. Authorize the session to restore the 8 paths to HEAD (working tree only, no commit); the patch stays in artifacts.
    2. Authorize re-running CI run 42673031307 for 4fcea652 (no Git write).
    3. Run the normal Controller verification on 4fcea652 with a clean tree.
       Expected under 16.9: no green CI on CurrentSha => REWORK, so no nc1-nc3.
    4. Return for the correction decision.
(B) Run the Controller on the dirty tree, by explicit ruling over 16.4 step 1.
    Expected: REWORK with FailureClass CleanTree, which names the wrong cause. Not recommended.
(C) Skip verification and rule the correction directly. 16.9 has no such path; requires an explicit ruling.

Decisions requested:
Q1. Option A, B or C.
Q2. For the correction (16.8: attempts becomes 2 of 3, in a commit before the delegation; new planning; new RED
    narrowing that one regex): may the saved patch seed the GREEN, or must the Worker re-implement?
Q3. ProbedId always null: accept as a known limitation, or require a change?

F2-T2A: WORKER BLOCKED, VERIFICATION PENDING DECISION. F2-T2B: WAITING. Smoke-NAV: PENDING. attempts: 1 / 3.
No AutoCAD use. Nothing committed by the session.
