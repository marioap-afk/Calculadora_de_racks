I-64 — F2-T2A-NAVIGATION-HOST
ATTEMPT 2 VERIFIED; nc1 ORACLE NOT MET
Disposition: COORDINATOR_F2_T2A_NC1_DECISION_REQUIRED

ATTEMPT 2 PRODUCT
Worker R20261109T194127Z-31c0 (claude-sonnet-5-5/high, 110 turns, 0 denials; prompt byte-identical): IMPLEMENTATION_COMPLETE.
BaseSha 53302b85518a8d0eaaa07cf1af5affc97581aa95
RED 2da0e60d0bd250791d8eb8751e05c3f274756de2 (2 test files only)
GREEN = CurrentSha = HEAD = remote 44a692a4ec386e559cc6d330b3d78b87dbad6581 (8 product files, 0 test files; Sonnet 5.5 trailer on both)
Scope: 10 paths, all inside AllowedWriteScope. origin/main a140eb3a. Tree clean. config.toml Snapshot D.
CI GREEN 42687835409: 4/4 success, including Build Plugin without AutoCAD.
  Core filter 756 selected, 756 passed; full Core 15148/15148. UI filter 64/64; full UI 1701 passed, 17 skipped, 0 failed.
CI RED 42687231210: Tests failure, Build UI success, UI Tests success, Build Plugin skipped.
  Core filter 756 selected, 709 passed, 47 failed (by assertion).

Real verification R20261109T201422Z-89a6 (gpt-6-luna/high, schema-valid; output SHA-256 881DB5E6A40CB8D0...):
EXECUTION_VERIFIED / NONE. FailureClass NONE. VerifiedSha 44a692a4ec386e559cc6d330b3d78b87dbad6581.
14 of 14 checks pass; RedPart pass in Ci and Tests. ChainRedSha 2da0e60d; ChainRedFiles = the 7 tests/ paths.
Facts on that output: 8 non-ASCII bytes in free text despite the prompt's request (section sign, n-tilde); schema-valid, no 16.9 check depends on it.
stderr ends with "failed to record rollout items (thread not found)" after turn.completed; effective model and effort were read from the session log.
16.10 scan of the output: no matches.

nc1 R20261109T202056Z-3d86 (same verified inputs, CurrentSha -> deadbeef...; pre-invocation assertions all pass):
Expected: Identity fail, FailureClass Identity, EXECUTION_BLOCKED / STOP, earlier checks equal to the real verification.
Observed: EXECUTION_BLOCKED / STOP, FailureClass Handoff, TriggeredStopConditions S-04.
Handoff = fail, Identity = fail; every other check = pass (no cascade into Ci or Tests).
Handoff evidence: the mutated CurrentSha is "inconsistent with Git HEAD and the recorded CI SHA".
Schema-valid and coherent. Relative oracle: NOT MET. Output SHA-256 BEDE765850C5AAA0...
Entry: 0 participants; 1 unattributable process (powershell.exe, pid 41196) gone on re-read (CLEARED_BY_REREAD rule on P-02).

Same failure mode as 3ddb and 78b0 (F1-T2-BRIDGE) and 39c9 (F2-T1), all ruled INVALID_OUTPUT by the Coordinator, with a retry authorized.
Those rulings bind their own units, so the session did not classify this output and did not retry by analogy.

Not run: nc2, nc3, any retry. No second Worker. No Git write by the session since 53302b85.
Custody of planning bafe, Worker 31c0, verification 89a6 and nc1 3d86 waits in ignored artifacts: 16.4 step 5 allows no session commit until the controls finish.

Decision requested:
Q1. Classification of nc1 3d86. Recommended, matching 39c9: SCHEMA_VALID_BUT_ORACLE_INVALID / INVALID_OUTPUT.
Q2. If so, authorize one nc1 retry (same verified inputs, same mutation, same prompt except the RunId; no attempts consumed).
    README section 10 allows up to two re-runs per control.
Q3. Confirm that nc2 and nc3 then proceed normally, each stopping at any result that misses its oracle
    (A-4 and A-6 only under their accepted conditions; A-11 not available).

F2-T1: CLOSED / PASS. F2-T2A: ATTEMPT 2 EXECUTION_VERIFIED, CONTROLS PENDING. F2-T2B: WAITING. Smoke-NAV: PENDING. attempts: 2 / 3.
