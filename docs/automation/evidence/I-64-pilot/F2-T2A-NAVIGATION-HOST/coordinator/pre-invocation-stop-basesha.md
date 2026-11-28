I-64 — F2-T2A-NAVIGATION-HOST
STOP BEFORE WORKER INVOCATION
Disposition: COORDINATOR_F2_T2A_BASE_DECISION_REQUIRED
Worker: NOT INVOKED (authorization unused; attempts 1/3 unchanged)

Branch: architecture/workspace-persistente-rackcad
HEAD = origin = d9d386e91aa6df5db64fbb6946b7edb3542baace (CI 42663763540 4/4); tree clean
origin/main: a140eb3a
Accepted delegation R20261109T171357Z-e137: BaseSha 35c3cd05bb180462c869324ca832f625ad735a0b

Acceptance literal custodied (not committed, git-ignored):
artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/coordinator/coordinator-f2t2a-acceptance.md
SHA-256 01E48BC0B103DE7F319DBDB92C1219E0ACE78E5CF8A0A8DE16674A0F362DCD24 (5388 B, LF)

Defect (session-caused):
Commit d9d386e9 (planning, A1-A8 and nc4 custody for Coordinator review) sits between
planning and the Worker. That contradicts AUTOMATION_PLAN 16.4 commit order, steps 2-3
(planning, nc4 and acceptance without commit).

Measured consequences if the Worker starts now:
- 16.9 row 7 Scope: git diff --name-only BaseSha..CurrentSha includes 19 docs/ paths
  outside AllowedWriteScope -> STOP (certain).
- 16.9 row 11 Trailer: d9d386e9 carries Co-Authored-By Claude Opus 5.5, not the Worker
  model -> REWORK.
- A6: BaseSha (35c3cd05) != HEAD (d9d386e9) -> not satisfied at invocation time.
=> Attempt 1 would be consumed by a protocol artifact, not by the product.

Options:
(A) RECOMMENDED - re-plan on the current base, no other change:
    1. Optional session state commit + push (16.4 step 1): acceptance literal + this STOP.
       This becomes the new BaseSha.
    2. New Controller planning (gpt-6-luna/high) on that base, with the same T2A contract
       (AuthorityRevision 09a1499a, MainSha a140eb3a). The prompt carries the accepted
       clarifications:
       - Effects GetObject: READ only for the already-selected target, for VIEW/CENTER;
         no discovery;
       - C2 / Facade guard files out of scope (STOP P-03);
       - WorkspaceSelectionRead.cs not modified.
    3. A1-A8 + nc4, no commit.
    4. Worker, with no commit before it. To avoid a second review commit, either:
       (A1) pre-authorize the Worker if A1-A8 = 8 pass, the nc4 relative oracle is met,
            and AllowedWriteScope is identical to e137 (7 new + 8 existing); or
       (A2) review the new delegation by SHA-256 from artifacts/, without a commit.
    Cost: 1 planning + 1 nc4 Codex invocation; no Worker attempt consumed.
(B) Coordinator rule: keep e137 and evaluate rows 5, 7 and 11 and A6 from d9d386e9
    (the Worker start commit). Changes verification semantics and requires altered
    Controller inputs. Not recommended.
Rejected: history rewrite of pushed commits; revert commit (still a session commit,
and row 11 still fails).

No real-evidence bridge use; no AutoCAD; F2-T2B waiting; Smoke-NAV pending.
