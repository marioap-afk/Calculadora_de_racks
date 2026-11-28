I-64 — F2-T2A-NAVIGATION-HOST
A-12 PREPARED, NOT VERSIONED: ONE ORDER CONFLICT
Disposition: COORDINATOR_A12_VERSIONING_DECISION_REQUIRED

Done, on synthetic fixtures only, outside the repository (nothing committed; HEAD = remote = 44a692a4; tree clean):
- verify_identity_bridge.py   SHA-256 a58df344019b61aca4f9ebdd93630e351084ebf2f5342fb8407d218ccfbe15a1
  Strict JSON, handoff schema, exact comparison and hardened Git copied verbatim from the A-11 verifier (blob f9780f2f).
  Answers separately: HandoffIntegrity (never consults repository state) and Identity.
  ControlResult PASS only with HandoffIntegrity PASS and Identity FAIL for exactly the bound synthetic, nonexistent SHA.
- selftest_identity_bridge.py SHA-256 4d857c614fa2f5a306f067f7b7d804cc0e576d4e1ac55bf9463e02ce9ae0f502
  35 of 35 PASS (your T01-T15 exactly as ordered, plus T16-T35: identity modes, mutation exactness, relay binding, parser hardening).
  Two runs byte-identical. Result SHA-256 9636a01c6b7eece68c0443fdbba445525ec9b9ab2b506bb627879ab3bec0baab.
- regression_demo_identity_bridge.py SHA-256 b605c649bfb668def2ba52e7cfe289f5fd84c0b151bf324dd4de4caa35fbf3a6
  PASS: verifier 35/35; 20 mutants, all FAIL on their target cases. Two runs byte-identical. Result SHA-256 2ad515a5a6304e9441455a7769ab30b18965aaec06292f5b122b8941bad1ecb1.
  Mutant ControllerDefect reproduces 3d86/16b6/bfc8: on T15 it gives HandoffIntegrity FAIL, Identity NOT_EVALUATED.
No real Worker handoff, nc1 mutated handoff, relay file or repository occurrence was used.
The A-12 document is not written yet: its identity clause depends on this decision.

THE CONFLICT
The order requires, at real execution: "HEAD = 44a692a4... remote branch = 44a692a4...".
It also requires, before activation: "A-12 document versioned; verifier versioned; ... exact-SHA CI complete".
A commit on the T2A branch moves HEAD and the remote off 44a692a4, so the two cannot both hold.
The same commit also breaks ordinary nc2 and nc3: 16.9 Identity requires HEAD = origin = CurrentSha, and 16.4 step 5 forbids session Git writes until the controls finish.
F2-T1 hit this: it needed POST_CUSTODY_CONTROL_IDENTITY_EXCEPTION, and 041d failed because the Controller did not read that rule from an input;
it then needed a versioned analysis.md and a prompt change.
The session stopped before the first commit because pushed history cannot be rewound.

OPTIONS
(A) RECOMMENDED: version A-12 on a temporary side branch created at 44a692a4, e.g. architecture/workspace-persistente-rackcad-a12.
    The T2A branch stays at 44a692a4. CI and the Architect review run on the side-branch SHA.
    A-12 runs in exact mode, so your literal HEAD and remote conditions hold unchanged, with the verifier taken from its Git blob.
    nc2 and nc3 run with ordinary Identity and the unchanged prompt, and 16.4 step 5 stays intact on the T2A branch.
    After the controls, the T2A branch fast-forwards to the side-branch tip (linear history) and the side branch is deleted.
    Cost: one temporary branch, which needs your authorization under WORKFLOW.
    The post-custody mode would then be removed from the verifier before versioning, and the self-test and regression re-run.
(B) Version on the T2A branch and change the A-12 identity relation to the F2-T1 rule:
    HEAD = remote = tracking; 44a692a4 is an ancestor of HEAD; 44a692a4..HEAD touches only docs/ (already implemented and tested: T16-T20).
    Needs your ruling replacing the literal HEAD condition, and a separate ruling for nc2 and nc3:
    the post-custody rule has to go in their prompt, which needs a versioned analysis.md (16.8) and reopens the F2-T1 risk.
(C) Run nc2 and nc3 first, with HEAD still at 44a692a4, then version A-12 under (B).
    Reverses your stated order (A-12 before nc2); A-12 still needs (B).

Decision requested: A, B or C.
Nothing else is blocked: on your answer the session writes the A-12 document, versions it, waits for exact-SHA CI and runs the formal review with gpt-6.1-sol/high.

F2-T2A: Attempt 2 EXECUTION_VERIFIED; nc1 ordinary path EXHAUSTED; P-04 ACTIVE. A-12: PREPARED, NOT VERSIONED, NOT ACTIVE. nc2, nc3: WAITING. F2-T2B: WAITING. Smoke-NAV: PENDING. attempts: 2 / 3.
