I-64 / F2-T2A-NAVIGATION-HOST - HISTORICAL CONTROL ENVIRONMENT: PRE-FLIGHT ITEM 10 FAILED - STOP

STOP: OWNER_COORDINATOR_CODEX_ENVIRONMENT_DECISION_REQUIRED

The Controller was NOT invoked. The one authorized nc2 historical-environment execution is NOT consumed.

WHAT CHANGED (outside the session, after nc2 b454 ended at 03:08:43Z)
1. Codex CLI binary replaced. The accredited binary (Codex/bin/f71a721e0e6193a9/codex.exe, SHA-256 4B3286E35D7FE866C6B0EB3F07D7A4CDB5E96489576170DFD6B3A15AA1C67382) no longer exists. The only codex.exe is now Codex/bin/5449c1f0f26f1f98/codex.exe, SHA-256 4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17, directory created 2026-11-10T03:08:58Z. The version string is unchanged (codex-cli 0.167.0-alpha.3.2), but the binary is different.
2. config.toml changed. Now SHA-256 2EEDDAAB28C1147A5D927B4EF1E4C69229197E04CB6B114161FFFF04A16588C8, modified 2026-11-10T03:14:05Z, not in the accepted set A/B/C/D. The 104 key names are identical to Snapshot D; only values differ.
b454 is not affected: it ran on the accredited binary, and its entry check read Snapshot D.

PRE-FLIGHT (read-only, exact Controller environment, from the real worktree; no network after the live facts)
Mirror: built only from local objects (26647 objects, fsck exit 0), at <directorio-historico>\gitdir. Build record SHA-256 f80a492292865327dd997de2f50f5ccec157398197323eb12591f2b2b32b4129.
- 1 HEAD = 44a692a4: PASS
- 2 origin/main = a140eb3a: PASS
- 3 origin/product branch = 44a692a4: PASS
- 4 branch --show-current: PASS
- 5 show-toplevel = ExpectedWorktree: PASS
- 6 status --porcelain clean: PASS
- 7 BaseSha ancestry: PASS
- 8 RedSha ancestry: PASS
- 9 tree and files identical: PASS (5851 files compared, 0 differing)
- 10 config.toml accepted Snapshot D: FAIL
- 11 real repository refs, HEAD, packed-refs, index and config unchanged before and after: PASS
- Earlier proofs: CurrentSha, BaseSha and RedSha object bytes identical; branch expected by the Controller identical; historical origin/main equals the value recorded by 89a6; live repository clean: all PASS.
- Gate record SHA-256 bf5ec76ca3c55b7e62a871056d302254890a50ba19f5c69149cb20bd35a98ced.

CHILD-PROCESS PROOF: PASS for the part that does not involve Codex
A neutral read-only child through the launcher chain (timeout + env -C + runtime pwsh) received GIT_DIR and GIT_WORK_TREE and returned the five key facts; stderr empty. A Codex-mediated child (codex sandbox, available in the new binary) was NOT run, because the new binary is not accepted. That is the only way to prove the variables survive Codex's own shell layer.

FACTS RECORDED
LiveRemoteFacts: origin/main = 04a7041492d760170ce3f3ba5fceeb29146107cc; product branch = 44a692a4; main advanced 19 commits after the verified occurrence; authority blobs identical; movement already accepted.
HistoricalControlRemoteFacts: origin/main = a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60; product branch = 44a692a4. The historical environment exists only to evaluate a negative control relative to verification 89a6. It does not claim that a140eb3a is current main.

STATE
- Product branch: HEAD = remote = 44a692a4, clean. FROZEN. No real ref or index touched.
- Mirror retained, unmodified since the gate, for reuse. Not fetched, not pushed.
- A-13 REAL APPLICATION = PASS. nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE. nc2: b454 CONTROL NOT PASSED; one historical-environment execution unused. nc3 waiting. Attempts 2/3. F2-T2B waiting. Smoke-NAV pending. No custody commit. No AutoCAD. No product change.

DECISIONS REQUIRED
1. Owner: accept or reject config.toml 2EEDDAAB28C1147A5D927B4EF1E4C69229197E04CB6B114161FFFF04A16588C8 as an accepted snapshot (for example Snapshot E). Pre-flight item 10 must then name that snapshot instead of D.
2. Coordinator: rule on the new binary. Session recommendation: treat it as a changed CLI despite the identical version string, and authorize one probe of the Controller cell gpt-6-luna / high / read-only on it, plus one neutral codex sandbox child that returns the five Git facts under GIT_DIR and GIT_WORK_TREE. The nc2 launcher would then point at the new binary path. The Architect cell gpt-6.1-sol would need the same treatment only if another formal review is ordered.
After both decisions the session repeats the read-only gate and the child proof, and only then invokes the single nc2.
