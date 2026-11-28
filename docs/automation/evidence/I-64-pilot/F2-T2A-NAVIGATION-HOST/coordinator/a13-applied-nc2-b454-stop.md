I-64 / F2-T2A-NAVIGATION-HOST - A-13 APPLIED: PASS; nc2 NOT CLOSED - STOP

STOP: COORDINATOR_F2_T2A_NC2_DECISION_REQUIRED

A-13 REAL APPLICATION = PASS (executed once)
- All 9 pre-execution conditions true, authority blobs remeasured identical.
- Verifier bytes extracted from blob 1c2510f71160804c6d42ae4da580641523c72a07. Repository occurrence: product branch at 44a692a4.
- Exit 0. HandoffIntegrity = PASS. Identity = FAIL. ControlResult = PASS. FailClosed = false. No reasons. 52 checks, 0 failing. ChangedFields = [CurrentSha]. CandidateObject = MISSING. Stderr empty.
- HEAD = remote = tracking = 44a692a4 before and after.
- Bridge output SHA-256: f8624114b06d754f20e7fe31bffb20ae76d417171c571b11cbf6039560486111. Application record SHA-256: dca866bbc679d0b288030226b9ca61aced954766797204eb60ab4224c8a35df5.
- Recorded: nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE. Ordinary history preserved: 3d86 = INVALID_OUTPUT, 16b6 = INVALID_OUTPUT, bfc8 = CONTROL NOT PASSED / P-04 exhausted.

nc2 ORDINARY: R20261110T030457Z-b454 (one run; not retried)
- Normal Controller prompt, same inputs as verification 89a6 except the mutated delegation and its derived hash. gpt-6-luna / high / read-only. Binary, config.toml Snapshot D and process checks clean before and after. Cession 2026-11-10T03:05:34Z to 03:08:43Z. Exit 0. Pre-invocation assertions all true.
- Mutation: AllowedWriteScope excludes src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs.
- Output: schema valid, coherent. SHA-256 D6F12CC2B2FD6441FD17EC950212C2C925239CF0553FD8771012A8B7177745C4.
- Result: EXECUTION_BLOCKED / STOP, FailureClass Remote, TriggeredStopConditions [S-13]. Only Remote differs from the real verification.
- Expected oracle: Scope = fail, FailureClass Scope, EXECUTION_BLOCKED / STOP, earlier checks equal to the real run. NOT MET. CONTROL NOT PASSED.
- Remote evidence (Controller): "Current git rev-parse origin/main returns 04a7041492d760170ce3f3ba5fceeb29146107cc, whose parent is a140eb3a, showing main advanced after the recorded run; S-13 applies."
- Scope evidence (Controller): "git diff --name-only BaseSha..CurrentSha returned 10 paths; all are in the delegated scope and contract scope, with no forbidden-path intersection." Scope = pass, although the delegation excludes one of those 10 paths.

WHY THE SESSION STOPPED
- nc2 did not close, so nc3 was not started.
- A-4 was not applied. Its exact historical signature is Scope = pass with EXECUTION_VERIFIED and no other check differing. Here the Scope check repeats the defect, but Classification is BLOCKED and Remote differs. Whether that is exact enough is the Coordinator's decision.
- The disposition accepted the movement of origin/main for the A-13 review/activation chain. It does not bind the Controller, which applied S-13 from the live tracking ref. AUTOMATION_PLAN 16.9 row 6 places the Remote signal "en el registro", and the record itself shows OriginMainSha = MainSha = a140eb3a.
- nc3 would meet the same S-13, because Remote precedes FreeText. Any decision should cover both controls.

STATE
- Product branch: HEAD = remote = 44a692a4ec386e559cc6d330b3d78b87dbad6581, clean tree. FROZEN.
- Temporary branch: 31a1c837, local and remote. No commit since.
- nc1: SATISFIED_UNDER_I64_IDENTITY_BRIDGE. nc2: NOT CLOSED (b454). nc3: NOT RUN. Attempts 2/3. F2-T2B waiting. Smoke-NAV pending.
- origin/main = 04a70414. Local refs/remotes/origin/main = 04a70414, updated by the session's routine fetch in pre-checks; main really advanced.
- All new evidence is in ignored artifacts. No custody commit. No AutoCAD. No product change.

OPTIONS FOR THE COORDINATOR
A (session recommendation). Classify b454 as not probative of Scope, because S-13 pre-empted it, and authorize one new nc2 and then nc3, with the normal prompt and the same inputs, with the local refs/remotes/origin/main restored to MainSha a140eb3a for the duration of the controls and no fetch during them. That reproduces the state of the verification each control is relative to. It moves no branch and changes no history, and a fetch afterwards restores the real value. It is a session Git write inside the control window, so it needs explicit authorization, and the Coordinator should state that it does not conceal S-13 because the main advance is already accepted and the authority blobs are identical. If the new nc2 then shows the exact historical signature, A-4 applies on its existing terms.
B. Rule that b454 Scope = pass is the accepted A-4 defect despite the Remote result, authorize the A-4 bridge for nc2 without another Controller run, and rule on nc3, which will hit the same S-13 before FreeText.
C. Follow 16.7 (rebase on the new main). Not recommended: it moves the product branch away from the verified SHA and voids the current verification and A-13 binding.
