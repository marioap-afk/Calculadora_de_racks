I-64 / F2-T2A-NAVIGATION-HOST - HISTORICAL CONTROL MIRROR: WORKTREE BINDING - STOP BEFORE BUILDING

STOP: COORDINATOR_F2_T2A_MIRROR_DESIGN_DECISION_REQUIRED

Nothing was built or executed under the disposition. The one authorized mirror execution of nc2 is NOT consumed. The order literal is saved.

THE PROBLEM
As ordered, the mirror is a separate repository at a different path, with the verified inputs unchanged by bytes. Those two requirements conflict with the expected oracle Identity = pass:
- delegation.json carries ExpectedWorktree = <product-worktree> (the real path), and worker-handoff.json carries Worktree = the same path. Both are verified inputs, which must stay identical by bytes.
- AUTOMATION_PLAN 16.9 row 5 defines Identity as "Rama y worktree", before Remote and Scope.
- The Controller does use it. It read ExpectedWorktree in bfc8 and b454; it ran Get-Location or rev-parse --show-toplevel in 89a6, 16b6 and bfc8; it reported "Branch and worktree match the delegation" (16b6) and "branch and worktree match" (b454).
- In a mirror at another path, the current directory cannot equal ExpectedWorktree. The prompt says "worktree = el directorio de trabajo actual", so the Controller might tolerate it, but a strict reading gives Identity = fail, pre-empting Scope exactly as Remote pre-empted it in b454.
- The 9 pre-run proofs do not cover this, because they bind the branch name and not the worktree path.

I raise this before spending the single execution, because b454 showed that an environmental pre-emption yields a non-probative result.

OPTIONS
A (session recommendation). Keep the isolated mirror as the source of Git state, but keep the Controller's working directory at the real path: GIT_DIR = <mirror git dir> and GIT_WORK_TREE = <product-worktree>, set only in the Controller process environment. The mirror holds the historical refs (product branch and origin/product branch = 44a692a4, origin/main = a140eb3a), built only from local objects and with all 9 proofs. Inputs stay identical by bytes, the prompt stays normal, ExpectedWorktree and Worktree remain true, and config.toml gets no new project path. The product repository's .git and refs are never used or written; the working-tree files are only read, under the read-only sandbox. The difference from the literal order is that only the Git data is outside the product worktree, not the files. If the variables did not reach the Controller's shell, it would see live origin/main and Remote would fail, which the disposition already treats as STOP.
B. Separate worktree as ordered, with ExpectedWorktree and Worktree rewritten to the mirror path and the derived hashes recomputed, proven byte for byte to be the only differences. This needs an explicit waiver of "same verified inputs by bytes".
C. Run exactly as ordered and accept the risk. If Identity fails on the path, STOP; the single execution is consumed.

STATE (measured now)
- Product branch: HEAD = remote = 44a692a4ec386e559cc6d330b3d78b87dbad6581, clean tree. FROZEN.
- Live origin/main: 04a7041492d760170ce3f3ba5fceeb29146107cc. No ref was changed.
- Temporary branch: 31a1c837.
- A-13 REAL APPLICATION = PASS. nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE. nc2: b454 CONTROL NOT PASSED, preserved; one mirror execution authorized and unused. nc3 waiting. A-4 not applied. Attempts 2/3. F2-T2B waiting. Smoke-NAV pending.
- No mirror directory exists. No custody commit. No AutoCAD. No product change.
