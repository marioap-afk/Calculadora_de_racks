I-64 / F2-T2A-NAVIGATION-HOST - A-12 ARCHITECT REVIEW: CHANGES REQUIRED - STOP

STOP: COORDINATOR_A12_ARCHITECT_CHANGES_REQUIRED

VERSIONING (executed as ordered)
- Temporary branch: architecture/workspace-persistente-rackcad-a12
- A-12 versioning commit: 4206b2da3a9985f429b77480cd5b5d26377f3215
  Single parent 44a692a4ec386e559cc6d330b3d78b87dbad6581. 12 files, all under docs/. Created by plumbing with a temporary index, no checkout.
- Exact-SHA CI: 42720753124, 4/4 success.
- Post-custody mode removed before versioning. All earlier self-test and regression results are void.
- Self-test: 31/31 PASS, two runs byte-identical (SHA-256 b50a75eac5ea566e01a3f39ae669fe17011a3877abe9e514d152d3948abea1ac).
- Regression: PASS, two runs byte-identical (SHA-256 3c94bc6f0755cb3fd1ef41bb604ee7f1a29ee508fdc8b3b2387748ce30333a0e). Verifier 31/31; all 18 mutants FAIL, each on its intended cases.
- Verifier blob: 13605f1a1c0e0f11c9d248f9def29559b6a4369b.

ARCHITECT REVIEW
- RunId: R20261109T233319Z-e044. Reviewed object: exact commit 4206b2da.
- Cell: gpt-6.1-sol / high / read-only (effective values measured); CLI 0.167.0-alpha.3.2.
- Binary SHA-256 unchanged (4B3286E35D7FE866C6B0EB3F07D7A4CDB5E96489576170DFD6B3A15AA1C67382). config.toml Snapshot D (377D0C3C2DA32B0F...) accepted before and after. Relay and process checks clean before and after (0 participants, 0 unattributable).
- Cession 2026-11-09T23:33:54Z to 23:38:53Z. Exit 0, turn.completed, schema valid, coherent, subject blobs match, 0 non-ASCII bytes. 29 commands, all with -NoProfile. No Git writes. No 16.10 terms.
- Output SHA-256: 815276716451F40EEA879899EFC3B8EF98F198963B8364E0F09B863422FDA4FA.
- Prompt SHA-256: 09011773A032922DAACDD739B5AEBA6C3A21A05E6EA76B1F819D8777BC347100.
- Record SHA-256: f1f10c5ebbd010b6544c099b80bb44a2f4f1fa5be981e63dfbf6f6d10765e828.
- Verdict: CHANGES REQUIRED. REQUIRED = 5. OPTIONAL = 0.
- Conforming: Q01 (M-04), Q02 (scope), Q04 (Identity, no post-custody semantics), Q06 (mutation isolation), Q08 (occurrence binding), Q09 (side-branch provenance), Q10 (relationships), Q11 (no expansion into nc2/nc3), Q13 (pre-activation boundary), Q15 (no new authority).
- Not conforming: Q03, Q05, Q07, Q12, Q14.

FINDINGS (two root causes; the Architect declares Q07 and Q14 as consequences of them, and Q12 is the test gap)
1. I64-A12-RELAY-SCHEMA (Q03). Only the two handoffs are schema-validated. The relay record receives partial checks, so missing required fields and forbidden additional properties are accepted. The self-test relay fixture is itself invalid against the official schema, yet reaches HandoffIntegrity PASS and exit 0.
2. I64-A12-NONEXISTENCE-ERROR (Q05). verify_identity_bridge.py:398-399 treats any non-zero git cat-file result as proof of nonexistence. Simulated return codes 128 and 129 reached ControlResult PASS and exit 0.
3. I64-A12-FAIL-CLOSED-GUARANTEE (Q07). The general fail-closed guarantee is false because of 1 and 2.
4. I64-A12-ADVERSARIAL-GAPS (Q12). No case or mutant detects 1 or 2.
5. I64-A12-UNVERIFIABLE-PASS (Q14). Observable PASS path caused by 1 and 2.
All five: RequiresGlobalI61Change = false, RequiresOwnerDecision = false.

SESSION CHECKS OF THE REVIEW
- Findings 1 and 2 confirmed against the code at 4206b2da: verifier line 304 validates only the handoffs; lines 398-399 use rc != 0; the official relay-record schema requires 14 top-level fields with additionalProperties = false, and the fixture omits Participant, Exit, Entry, RemoteFacts, RebaseMap, Disposition and Notes.
- Full-text audit of the 29 reviewer commands: 4 executions of bridge tools, all synthetic and in memory, none referencing artifacts/. Real files were only hashed or read as data, as the prompt allowed.

LIMITATIONS DECLARED BY THE ARCHITECT
- Full self-test and regression not re-run (they need temporary repositories; read-only sandbox). Sources, hashes and versioned results inspected instead.
- git ls-remote unavailable from the sandbox. Live remote equality must be verified before any authorized application.
- Remote CI of the side-branch commit not verified by the Architect. The session measured it: run 42720753124, 4/4.

STATE
- Product branch: HEAD = remote = tracking = 44a692a4ec386e559cc6d330b3d78b87dbad6581, clean tree. Not moved.
- Temporary branch: 4206b2da, local and remote. Not deleted.
- A-12: PROPOSED / NOT ACTIVE.
- Owner text NOT requested: the condition (Architect AGREED) is not met.
- Real A-12 bridge not executed. nc2 not run. nc3 not run. Attempts 2/3. P-04 history unchanged.
- Review artifacts are ignored and uncommitted: artifacts/orchestration/I-64/A-12-review/R20261109T233319Z-e044/.
- No AutoCAD. No product change.

DECISION REQUIRED FROM THE COORDINATOR
No correction has been started: A-12 is append-only and the single authorized review is consumed.

Session recommendation: order the next append-only amendment on the same temporary branch, with A-12 kept immutable as CHANGES REQUIRED / NOT ACTIVE. Minimal delta:
(a) validate the complete relay record against the official relay-record schema, pinned by blob at a140eb3a, before HandoffIntegrity can pass, still without consulting the repository;
(b) prove nonexistence only from a successful object query whose response is exactly "missing" for the candidate, with any error, unexpected output or incomplete query giving FAIL and a non-zero exit;
(c) make the synthetic relay fixture schema-valid and add cases for a missing required field, an additional property, a wrong type and Git errors in the final query, with mutants that disable each new protection;
(d) self-test twice, regression twice, exact-SHA CI, and one new formal Architect review on the new exact commit.

Open point for that order: full schema validation will also apply to the real relay input (SHA-256 b687bdd82f286757ed90e50b0f01b977ec4fcad6eb0e00b7e017ba05e3a1852f). This session has not validated that file against the schema, because the real occurrence is only bound by SHA-256 before activation. If it were invalid, the single real execution would FAIL with no retry. Please state whether a schema-only pre-check of that one file is authorized before activation, or whether it stays inside the single execution.
