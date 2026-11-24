# Independent review of the Smoke-PB evidence (Q-PBT5-8), before custody

You are an independent reviewer. You did not run this smoke test. Your job is to find what is wrong, overstated, missing, or unsafe to publish in the evidence below **before** it is committed to a PUBLIC git repository. Be adversarial: assume the operator (an AI session) made mistakes and look for them. Read-only: do not modify any file listed below, do not run AutoCAD, do not use git, do not use screen control. Write only your report.

## What was tested
A contracted physical smoke test ("Smoke-PB") of the Push Back workspace panel of RackCad, an AutoCAD 2025 plugin, on the Candidate commit `ebd304a3`. 65 steps (SPB-01 … SPB-65), each with a criterion written in the contract guide. The operator ran it on copies of a drawing (`cNN.dwg`), recorded literal UI texts, took screenshots, and dumped read-only inventories of the drawing (`RCDUMP`) before/after steps.

## Material (all local, read-only)
- **The contract guide** (criteria and procedure): `<carpeta-del-Smoke-PB>\sesion-respaldo\pbt5\smoke-run-v2.md` — especially §4 (procedure, blocks), §4.11 (injections), §4.12 (constructed contexts), §7 (log) and §8 (screenshots: crop, opaque masking, never-custody list, checklist, derivation record).
- **The operator guide** (per-step origin, what is done, what is measured, proposed criterion, UI literals with `src/` line references): `<carpeta-del-Smoke-PB>\operator-guide.md`.
- **The package** (decisions and contract context): `<carpeta-de-la-sesion>\hs65\pbt5\I-64-f4b-pbt5-package-v2.4.md`.
- **What would be custodied** (staging): `<carpeta-del-Smoke-PB>\custodia-staging\run\` — `LEEME.md` explains each file: `run-log.md` (redacted), `owner-summary.md`, `integrity.md`, `environment-pre.md`, `environment-post.md`, `inventories\` (summaries + `INDICE.txt`), `inv-texts\`.
- **The screenshot crops** that would be custodied: `<carpeta-del-Smoke-PB>\screens\crop\` (PNG), with `derivation-record.txt` and `no-custodiadas.txt` there.
- **For cross-checking only (never custodied):** raw screenshots `<carpeta-del-Smoke-PB>\screens\raw\`, raw inventories `<carpeta-del-Smoke-PB>\inv\`, the unredacted log `<carpeta-del-Smoke-PB>\run-log.md`.
- The Candidate source, if you need to check a UI literal: the git object store of the worktree `<worktree>` at commit `ebd304a3` (read files with `git -C <wt> show ebd304a3:<path>`; this is the only git use allowed, and it is read-only).

## What to check
1. **Privacy of everything that would be custodied** (the repository is public): any drive letter or local path, any Windows user name or account name, any client/project/sheet/title-block name, any other application, taskbar or desktop in a crop. Open every crop PNG (they are the publication). Check that every masked zone is opaque and actually covers the sensitive text, and that the command-line history in crops does not show the echo of `NETLOAD`, `vla-open` or the `defun` lines (they carry paths). Check the texts too (run-log, inv-texts, summaries).
2. **Each step's proposed verdict against the guide's criterion.** For every SPB step, does the evidence recorded in the log (literal texts, inventory hashes, captures) support the proposed result (PASS / FAIL / OBSERVADO / LIMITATION / NOT AVAILABLE / NO VERIFICABLE)? Flag any PASS that is not supported, any criterion silently reinterpreted, any FAIL or OBSERVADO that should be something else, and any step missing a verdict.
3. **Fidelity of the record:** sample-check that hashes cited in the log match the files (inventories in `<carpeta-del-Smoke-PB>\inv\`, by full or 8-hex-prefix SHA-256; captures in the raw folder). Check that claims such as "`post` idéntico a `ins`" hold.
4. **Crops vs the derivation record:** sample-check that each crop's recorded raw SHA-256 and crop SHA-256 match the files, and that the rectangle plausibly shows the step's result.
5. **Declared deviations and incidents:** are all deviations from the guide declared (operator repetitions at most 2, extra steps, things not done)? Anything done that the guide forbids (for example "Always Load", "Always ignore", touching settings, saving a copy, closing an AutoCAD the session did not open)?
6. **Completeness of the custody set** against §7 of the guide (header items of the log, inventories summaries, integrity, environment pre/post, owner summary, screenshots + derivation record).

## Output
Write your report to `<carpeta-de-la-sesion>\hs65\pbt5\run-review\independent-review-evidence.md` (Spanish or English), with:
- a verdict line: `RESULTADO: CUSTODIABLE | CUSTODIABLE TRAS CORRECCIONES | NO CUSTODIABLE` and the counts `B <n> / I <n> / M <n>` (B = blocking: a privacy leak or a verdict that is wrong; I = important; M = minor);
- each finding with its id, class, file and line or crop name, what is wrong, and what would fix it;
- a short list of things you checked and found correct (so the coverage is visible).
Never quote a sensitive string in your report: describe where it is (file, line/crop, region) instead.
