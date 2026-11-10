I-64 / F2-T2A-NAVIGATION-HOST - CODEX ENVIRONMENT: COMPARISON AND PROBES COMPLETE - STOP

STOP: OWNER_COORDINATOR_CODEX_ENVIRONMENT_ACCEPTANCE_REQUIRED

nc2 and nc3 were not invoked. The one nc2 historical-environment execution is unused. Pre-flight item 10 remains FAIL.

A. CONFIG SEMANTIC DIFF
Limitation, first: a per-key value diff against Snapshot D cannot be proven. Snapshot D was recorded only as a SHA-256 and an ordered list of key names, deliberately without values, and its values no longer exist on this machine. I attempted a hash-verified reconstruction over 12 combinations of locally available prior values (binary directory, app version, runtime directory, client hash); none reproduced the SHA-256 of D.
1. Total keys in D: 104 (name extractor).
2. Total keys in current: 104. Flattened value keys: 68.
3. Added keys: none.
4. Removed keys: none. Ordered-name hash identical to D.
5. Changed-value keys: NOT DETERMINABLE.
6. Unchanged-value keys: NOT DETERMINABLE.
Inferred, not proven: mcp_servers.node_repl.env.CODEX_CLI_PATH must differ if D pointed at the replaced binary directory.
Classification of the 68 current keys: A cosmetic/UI 9; B telemetry 0; C model/effort/routing 3; D sandbox/permission/approval 14; E tool/connector 40; F execution/limits 2; G unknown 0. 20 keys are maintained by the app install (runtime paths, app version, CLI path, trusted client hash) and are the likely changes.
Current material values:
- C: model = gpt-5.6-sol; model_reasoning_effort = medium; service_tier = priority. The Controller launcher overrides model and effort. It does not override service_tier.
- D: windows.sandbox = unelevated; 12 projects.<path>.trust_level = trusted; shell_environment_policy.set.NODE_REPL_TRUSTED_BROWSER_CLIENT_SHA256S (a hash).
- F: mcp_servers.node_repl.startup_timeout_sec = 120; NODE_REPL_NATIVE_PIPE_CONNECT_TIMEOUT_MS = 1000.
Secrets: no secret-shaped value found; one opaque id (SKY_CUA_NATIVE_PIPE_DIRECTORY) reported by shape only. User and project paths are redacted.
Report: config-semantic-report.json, SHA-256 e8282ba2fedeb90f4cad918417bdbb1b5217cc5df6588ab3efdc605d2f6ec2d2. Full inventory included.
Independent evidence of effective behavior: the turn_context of the probe under the current config and new binary was compared with that of nc2 b454 (Snapshot D, old binary), over 13 fields: approval_policy, approvals_reviewer, collaboration_mode, comp_hash, cyber_access_program, disabled_plugin_ids, effort, model, permission_profile, realtime_active, root_turn_id, sandbox_policy, summary. The only difference is root_turn_id, a per-turn id.

B. PROPOSED SNAPSHOT E DISPOSITION
- No unexplained changes: NOT DEMONSTRABLE, because D's values do not exist.
- No secret exposure: MET.
- No incompatible routing change: effective model, effort and routing equal to b454 (MET on effective evidence).
- No permission widening: effective permission_profile identical to b454. Per-key comparison of trust levels is not possible.
- No sandbox weakening: effective sandbox_policy identical to b454 (read-only). Per-key comparison of windows.sandbox is not possible.
Session proposal: the criterion "no unexplained changes" cannot be satisfied as written. Snapshot E can only be accepted through an explicit Owner waiver of that criterion, resting on the effective evidence above. If accepted, the current inventory becomes a value-level baseline, so later changes can be diffed. The session does not infer acceptance.

C. NEW BINARY PROBE: R20261110T034758Z-6a68 = PASS
- Binary: Codex/bin/5449c1f0f26f1f98/codex.exe, SHA-256 4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17 (same before and after; single codex.exe). Version codex-cli 0.167.0-alpha.3.2.
- Effective: gpt-6-luna / high. Sandbox read-only; file system read only; network restricted; approval never.
- Exit 0, turn.completed, cession 03:48:07Z to 03:48:24Z. Output schema-valid; Git oracle exact (HEAD, origin/main, state blob). Output SHA-256 3C2C09BD07F6425581E9BF59DFF16515BBABA72E8095B88D208638643DDF2E96.
- Relay record valid, SHA-256 D5121B7A7410EEF298E35970BA17FCFF8859EDDB05373F817D1AAF672194CC5A. No Git writes: real repository and mirror unchanged. No participant or unattributable process. No usage or credit warning. No routing change. Stderr: only the stdin notice.
- Observed change: commands are now launched with -NoProfile; none of the b454 commands were. It removes the profile.ps1 noise seen before. Whether it comes from the binary or from config.toml is not determinable.

D. GIT ENVIRONMENT PROPAGATION (Codex-mediated, new binary): PASS
- Mechanism: timeout + env -C <product-worktree> + codex sandbox with sandbox_mode read-only, no model, running only the five approved commands, under GIT_DIR = historical mirror and GIT_WORK_TREE = <product-worktree>.
- Result: exactly five lines. HEAD = 44a692a4; origin/main = a140eb3a; origin/product branch = 44a692a4; branch correct; show-toplevel = real ExpectedWorktree. Exit 0, stderr empty, live origin/main not substituted. Real repository, mirror, binary and config.toml unchanged; no participants before or after.
- Result SHA-256 1832c4720af1c8d13f467e2381e2f7dac76199e069d21dd7c11b6ff7e5815223.
- Limitation: codex sandbox does not pass through the model's shell tool. If that path behaved differently in the real nc2, Remote would see live origin/main, which the disposition already treats as STOP.

STATE
- Product branch: HEAD = local tracking = 44a692a4, clean. FROZEN. No Git network operation while the mirror exists; live facts were measured immediately before construction.
- Live origin/main (last measured): 04a70414. Historical control origin/main: a140eb3a.
- A-13 REAL APPLICATION = PASS. nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE. nc2: b454 CONTROL NOT PASSED; historical-environment execution unused. nc3 waiting. Attempts 2/3. F2-T2B waiting. Smoke-NAV pending. No custody commit. No AutoCAD. No product change.

DECISIONS REQUIRED
1. Owner: decide on config.toml 2EEDDAAB28C1147A5D927B4EF1E4C69229197E04CB6B114161FFFF04A16588C8, knowing that the value diff against D cannot be demonstrated.
2. Coordinator: accredit or reject the new binary as the Controller cell, considering the -NoProfile change.
After both, the session repeats the entire historical-control pre-flight once more before nc2.
