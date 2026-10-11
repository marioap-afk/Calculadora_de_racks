# B-min acceptor — notes (I-62, §75; experimental, not normative)

Follows SPEC **v0.2** (§5 errata E1-E7; see "Errata v0.2" below). Written only from `SPEC.md` (§1.5, §2, §3, §4,
§5) and the normative text (AUTOMATION_PLAN blob `f525cb1e`: 16.5,
16.9, 16.10, 16.13, 16.14; schemas `controller-verification.v2`, `delegation.v2`, `worker-handoff`,
`clause-map`), read-only from the architecture worktree under `%USERPROFILE%\.codex\worktrees\`. Not read: the
sibling `tool\` folder, `probe_tools.py`, `mv_check.py`, `clause_map.py`, and the excluded evidence folders
(`I-62-F6/OD-2`, `MAP-INVALID`, `FX-02/s70|s72|s74`, `kits`). No model CLI was invoked; no network.

## Files

| File | Role |
|---|---|
| `bmin_accept.py` | the acceptor (stdlib only, zero writes, run with `python -I`) |
| `tests/test_bmin_accept.py` | unittest suite; synthetic repos under `D:\r62-bmin-tmp\accept\work` (never under `D:\r62-fixture`) |
| `tests/fake_tool.py` | TEST SCAFFOLDING only: minimal SPEC-conformant envelope emitter with mutation switches |

`fake_tool.py` uses the git primitives the SPEC prescribes for the tool (`rev-parse`, `merge-base --is-ancestor`,
`git diff --name-only`, `%(trailers)`), so for `identity`/`diff`/`scope`/`hashes`/`contract` it also cross-checks
the acceptor's independent primitives against git. For `clausemap` it imports the acceptor: there it is not
independent; it serves the plumbing and mutant tests, while the acceptor's clause-map facts are checked against
hand-derived expectations on a map with known defects.

## How to run

```powershell
# acceptor (all inputs read-only)
python -I bmin_accept.py --request REQUEST.json [--request-sha256 HEX]

# flags (may be mixed with a request; a conflicting value is USAGE, exit 64) and the single-run shortcut
python -I bmin_accept.py --repo R --tool bmin_facts.py --tool-blob <40hex> `
  --python-version 3.13.14 --python-exe-sha256 <64hex> --closure C.json --closure-sha256 <64hex> `
  --relay-facts F.json --relay-sha256 <64hex> --na R4=reason --na R6=reason --na R7=reason `
  --envelope ENVELOPE.json -- diff --repo R --from <A> --to <B>
# other flags: --tool-git-dir D --tool-git-blob <40hex> --run-from path|object

# tests (from this folder). BMIN_KEEP=1 keeps the temporary tree; BMIN_ACCEPT_TMP moves its root
python -I tests/test_bmin_accept.py
```

Exit code: 0 = `ACCEPT_FACTS`, 1 = `REJECT_FACTS`, 64 = usage error (stdout still carries a `REJECT_FACTS`
object with `Error`). Stdout: one canonical ASCII JSON object + `\n`.

### Request (`rackcad-bmin-accept-request/0-experimental`)

```json
{
  "Schema": "rackcad-bmin-accept-request/0-experimental",
  "Repo": "<clone the tool ran on>",
  "Tool": {"Path": "<bmin_facts.py>", "PinnedBlob": "<40hex>", "RunFrom": "path|object",
           "GitObject": {"GitDir": "<repo holding the tool>", "Blob": "<40hex>"}},
  "Interpreter": {"Python": "<platform.python_version()>", "ExecutableSha256": "<64hex>"},
  "Runs": [{"Name": "diff-1", "Envelope": "<saved tool stdout>",
            "Argv": ["diff", "--repo", "...", "--from", "<40hex>", "--to", "<40hex>"]}],
  "Verification": {"Controller": {"Path": "...", "Sha256": "..."}, "Delegation": {"Path": "...", "Sha256": "..."},
                   "Handoff": {"Path": "...", "Sha256": "..."}, "IgnorePath": "artifacts/...",
                   "TermsRev": "<40hex>", "MainSha": "<40hex>", "DeclaredCoAuthoredBy": "<optional>"},
  "Closure": {"Path": "...", "Sha256": "<optional pin>"},
  "RelayFacts": {"Path": "...", "CitedSha256": "<64hex>"},
  "NotApplicable": {"R6": "reason"}
}
```

### Closure (`rackcad-bmin-closure/0-experimental`, R9)

`Commits` (40-hex), `RevPaths` (`{"Rev","Path"}` or `"rev:path"`), `Refs` (symbolic revisions allowed, e.g.
`HEAD`, `refs/remotes/origin/<branch>`), `Paths` (pathspecs, check-ignore paths), `Objects` (extra object ids),
`Files` (`{"Name","Sha256"}`). Trees of allowed commits and blobs of declared `RevPaths` are implicitly allowed.

## Checks and decision

`Decision` = `ACCEPT_FACTS` only if every check is `MATCH`, `NOT_APPLICABLE` or `MANUAL`. A check without data is
`MISMATCH` unless declared in `NotApplicable`; R1, R2, R3 and R9 cannot be waived; declaring a check not applicable
while supplying its data is `MISMATCH`; any exception inside a check is `MISMATCH`. The acceptor never emits
`EXECUTION_*`; a final hygiene pass redacts that string and `X:\Users\<name>` path components in `Detail`.

| R | What the acceptor does | Independent primitive |
|---|---|---|
| R1 | own SHA-1 of `blob <len>\0` + bytes of the tool file and/or of `git cat-file blob <Blob>`; each must equal `PinnedBlob` | own blob hash |
| R2 | `sys.flags.isolated == 1`, `platform.python_version()` and SHA-256 of the interpreter image equal the declared values; every envelope `Runtime` (received and re-run) has `Isolated: true` and the declared `Python` | hashlib |
| R3 | received envelope: bytes < 0x80, canonical bytes, exact SPEC 1.5 key set, `Error` null, `Facts` an object, `FactsSha256` recomputed over canonical `Facts`, no verdict vocabulary; re-run with `python -I` (tool path, or the verified object bytes via stdin) and every key except `Runtime` compared. Runs only after R1 matched; in path mode the tool file is re-hashed afterwards | — |
| R4 | recomputes `Identity`, `Scope`, `CleanTree`, `Trailer`, `FreeText` and the map part of `Authority` from its own facts and compares with `Checks.<Name>.Result` (`not_run` = not pass) | own walk, ls-tree diff, own coverage, own trailer parser, own 16.10 terms, own Derive/Validate |
| R5 | field comparison of `identity`, `diff`, `scope`, `hashes` envelopes | `ls-tree -r -z` (mode + id), own parent walk, own trailer parser, `for-each-ref`, own blob SHA-1 |
| R6 | `clausemap`: Derive, Entry, MV-1..MV-7; plus `AuthorityResolution[].EffectiveSha/ClauseMapBlob` vs own Derive at `Verification.MainSha` | own first-parent walk + exact-line trailer scan, own JSON-schema subset, own Markdown parser, own sentence splitter |
| R7 | normative-reading rules: `PerPath` coverage, `MalformedEntries`, `U19Normalized` (scope); `A3`/`A4` (contract); the four MV-4 lists (clausemap) | own coverage / containment / U-19 |
| R8 | always `MANUAL` | — |
| R9 | every `GitCalls` argv and `Inputs` entry fits the closure; argv `--file` files hash to their `--sha256` and appear in `Inputs` | own argv classifier and revision resolver |
| R10 | SHA-256 of the relay facts file equals the cited value | hashlib |

Git: one executor, closed list (`rev-parse cat-file ls-tree rev-list diff log show merge-base status check-ignore
for-each-ref`), `-c core.quotepath=false -c core.fsmonitor=false`, env without inherited `GIT_*` plus
`GIT_OPTIONAL_LOCKS=0 GIT_TERMINAL_PROMPT=0 LC_ALL=C GIT_NO_REPLACE_OBJECTS=1`, `shell=False`. Used:
`rev-parse` (object format; toplevel + HEAD + abbrev in one call), `cat-file --batch-all-objects --batch-check`,
`cat-file --batch`, `cat-file blob` (tool object), `ls-tree -r -z --full-tree`, `for-each-ref`,
`status --porcelain -z --untracked-files=normal`, `check-ignore -q --no-index`. Every commit and blob read is
re-hashed and must match its id; object format must be sha1. Processes: `git`, and `sys.executable -I` only for
the R3 re-run.

## Results (Windows 11, Python 3.13.14 with `-I`, git 2.54.0.windows.1)

`python -I tests/test_bmin_accept.py`: **52 tests, all pass** (about 330 s; process start-up dominates).
(v0.1 run: 48 tests, all pass.)

| Class | Tests | Content |
|---|---|---|
| `TestPrimitives` | 9 | blob SHA-1 = `git hash-object`; sentence splitter (16.13 not a boundary); pointer-only removal; Markdown sections with CRLF and code fences, `### 16.3` vs `### 16.30`, preamble; scope rules (malformed, U-19, coverage, containment); trailer parser cases; 16.10 terms (16, field names excluded); free-text whole-word/casefold; JSON-schema subset |
| `TestIndependentFacts` | 7 | ls-tree paths = `git diff --no-renames --name-only`; own ancestry/ranges = `merge-base`/`rev-list`; own trailers = git `%(trailers)` on 13 messages (mixed paragraph, non-final paragraph, title-only, Signed-off-by 25% rule, spaced separator, upper/lower key, continuation, CRLF, trailing blanks, non-ASCII); Derive on valid / duplicate trailer / trailer on main / no trailer; valid map = no defect; defective map = every known defect in its own list |
| `TestEndToEnd` | 29 | full request ACCEPT; each R check MATCH and MISMATCH (below); E4 and E6 cases |
| `TestErrataV02` | 3 | E1 vocabulary exemptions (only the listed echoes, per command); E2 strict `{Entry, List, Prefix}` objects (order, legacy strings rejected, R7 end-to-end); E3 `:(literal)` normalized in R9, other magic rejected (unit + end-to-end) |
| `TestHygiene` | 4 | zero writes; AST scan; determinism + ASCII; closed git list |

Required paths: wrong tool blob → R1 MISMATCH (and the tool is not executed); tool from a git object, re-run
from stdin → R1/R3 MATCH; declared version / exe hash wrong, acceptor without `-I`, envelope
`Runtime.Isolated=false` → R2 MISMATCH; tool without `-I` (`NOT_ISOLATED`), `BAD_REVISION` envelope,
non-canonical bytes, verdict word in `Facts` → R3 MISMATCH; tampered `Facts` without and with recomputed
`FactsSha256` → R3 (and R5) MISMATCH, REJECT; each of Identity/Scope/CleanTree/Trailer/FreeText flipped, real
failures (out-of-scope path, malformed forbidden entry, foreign trailer, wrong branch, RedSha outside the range,
untracked file, path not ignored, `gate  pass` in Evidence) → R4 MATCH/MISMATCH as expected; `Authority: pass`
with a defective map or a duplicate trailer → R4 MISMATCH, with a valid map → MATCH; closure without a commit,
without a ref, with a wrong file hash, extra `config` call, `cat-file --batch` reads, `:(glob)` pathspec → R9
MISMATCH; rename with `--no-renames` → R5 MATCH (both paths), without it → R5 MISMATCH; duplicated trailer or
trailer without qualifying merge, and two distinct `claim_id` values → AMBIGUOUS envelope rejected (R3, R6), and a
tool that emits facts anyway → R6 MISMATCH (E4) with R3 MATCH; no trailer → R6 MATCH; repeated `claim_id` → R6
MATCH with `ClaimIdLines` compared; R10
match/mismatch/missing/`NotApplicable`; waiver of R1 refused; usage errors exit 64.

Mutants of the (fake) tool, each killed by the acceptor with R3 still MATCH (the mutant reproduces itself):

| SPEC §4 mutant | Fake switch | Killed by |
|---|---|---|
| coverage rule inverted | `coverage-inverted` | R7 (`AllowedCoveredBy`) |
| sentence boundary | `sentence-boundary` | R6 (`FirstSentenceS16`) |
| ancestry argument order | `ancestry-swap` | R5 (`Ancestry`) |
| path omitted | `drop-path` | R5 (`Paths`) |
| MV-4 lists merged | `mv4-merged` | R6 and R7 (`AddedPresentInEff1`, `LiteralViolations`) |
| pointer removed with the whole line | `pointer-line` | R6 (`S163EffMinusPointer`) |
| (extra) U-19 off / first trailer only / extra git call / verdict in Facts | `u19-off`, `trailer-first`, `extra-gitcall`, `verdict` | R7 / R5 / R9 / R3 |
| (v0.2) legacy U-19 strings / renames on / glob pathspec / facts despite E4 | `u19-legacy`, `renames-on`, `glob-pathspec`, `e4-off` | R7 / R5 / R9 / R6 |

Zero writes (acceptor, including its re-runs of the tool): before and after, equal SHA-256 of `.git/index`,
`git for-each-ref`, `git status --porcelain --ignored` and the full file list with sizes of the clone, of this
folder and of the inputs folder; also for an in-process clause-map computation. AST: imports are exactly
`hashlib json os platform re subprocess sys`; `open` only with `"rb"`; no `os` mutators, no path writers, no
`eval/exec`, `write` only on `sys.stdout.buffer`, `subprocess.run` only in `git_run` and `rerun_tool`, always
with list argv and `shell=False`. Determinism: two runs give identical stdout bytes, all < 0x80, canonical.

### Sanity run on a fresh fixture clone (independent facts only, no oracle comparison)

Clone `git clone --no-local -c core.autocrlf=false -b fx/u1 D:/r62-fixture/fixture-origin.git ...`
(main `fbe25347`, HEAD `95bdc29d`), computed in about 9 s:

- Derive at main: `TrailerCommits` = [`a9f6c929`], `FirstParentMerges` = [`fbe25347`], `Candidates` = [`fbe25347`],
  `Eff` = `fbe25347` (`EffParent1` `930288c5`, `EffParent2` `a9f6c929`).
- Entry at main: `ApBlob` `f525cb1e`, `WorkflowBlob` `446c32ba`, `EntryMatches` [614], `R163Matches` [717],
  `EntryNamesR163` true. 16.10 terms at main: the 16 terms.
- MV-1: map `4d49d3e1` present, parses, schema `6b54dda9` present, 0 schema violations. MV-2: Surfaces = closed list.
- MV-3: `ChangedUnderSurfaces` empty, `FilesPaths` 31, `OnlyInFiles` 31 (EFF^1 already holds the same surface
  blobs as EFF: AP, WORKFLOW and even the map have identical ids at `930288c5` and `fbe25347`).
- MV-4: `LiteralViolations` 7, `AddedPresentInEff1` 23, `EntryFilePresentInEff1` 1 (the map schema),
  `EntryFilesOutsideSchema` 0. MV-5: no duplicates, no entry on a non-MODIFIED path.
- MV-6: 6 MODIFIED files; derived sets empty for all (identical text), map sets non-empty;
  `docs/initiatives/PROMPT_TEMPLATES.md` is absent at both EFF^1 and EFF; ENTRY pair = the fixed pair.
- MV-7: entry once and names 16.13; pointer is the first sentence of `## 16.` and the last of `### 16.3`;
  `S163EqualAfterPointerRemoval` false (EFF^1's 16.3 already contains the pointer).
- Identity (branch `fx/u1`, base `fbe25347`, current `95bdc29d`): HEAD = origin branch = current, base ancestor of
  current. Range main..HEAD: 18 commits, 8 with `Claude Opus 5.5 <noreply@anthropic.com>`; 31 changed paths, none
  under the excluded evidence prefixes.

Temporary trees (`D:\r62-bmin-tmp\accept\work`, the fixture clone, logs) were removed at the end.
`D:\r62-fixture` was only read by `git clone --no-local`.

## SPEC ambiguities resolved (any disagreement with the tool fails closed: MISMATCH → REJECT)

1. **Controller `Checks`** is an object keyed by check name in `controller-verification/v2` (not an array); R4 reads
   `Checks.<Name>.Result` and treats `not_run` like `fail`.
2. **16.10 terms** (now E5). The backticked field names are not terms; the terms are the list after the first
   `` `: `` → 16 terms. Free text is whitespace-collapsed and casefolded before whole-word matching; each array
   element is matched separately; `Gate` is never scanned.
3. **Renames** (now E6). Diff paths include both the source and the destination of a rename (ls-tree comparison;
   the tool uses `--no-renames`). A tool listing only the destination is an R5 MISMATCH.
4. **Orders not fixed by the SPEC** are compared as sets/multisets: `Commits` (set + uniqueness),
   `TrailerCommits`, MV-3 lists (except `FilesPaths`, map order), the four MV-4 lists, MV-5/MV-6 lists.
   Compared in order: `Paths` (byte order = tree order), `FirstParentMerges`, `Candidates`, `MV4.Rows`, `PerPath`, `A3`/`A4` entries.
5. **Underspecified element formats** are accepted in several representations, with strict content
   (`U19Normalized` is no longer one of them: E2 fixes it and it is compared exactly): `MalformedEntries` (string or
   `{"Entry"}`); `AllowedCoveredBy`/`ForbiddenCoveredBy`/`ContainedBy` compared after U-19; MV-5 pairs
   (`{"Path","Section"}`, `[p, s]`, `"p|s"`, or paths only); MV-6 per-file records as a list or under
   `PerFile`/`Files`/...; `EntryKindEntries`/`ExpectedEntryKind` under `MV6` or `Validate`.
6. **`SchemaViolations`**: only emptiness is compared (pointer/keyword granularity is implementation-dependent).
7. **Malformed entries**: `..` means a path segment equal to `..`; a drive prefix (`C:`) counts as absolute. For the
   R4 Scope rule any malformed or empty entry in Allowed or Forbidden makes the expected result «not pass»
   (otherwise a malformed forbidden entry would silently cover nothing).
8. **Markdown**: heading = normalized line starting with 1-6 `#` followed by a space or end; fences toggle on lines
   starting with three backticks; preamble = text before the first level-2 heading; a section includes its heading
   line; "body" = the lines after the heading up to the next heading of the same or higher level.
9. **`## 16.` / `### 16.3`** = the unique heading whose normalized text equals the label or starts with label +
   space (so `### 16.30` is not 16.3); not unique → null and a defect.
10. **Pointer removal**: sentence level, the last occurrence that is a whole sentence, once (rejoining with single
    spaces equals removing the sentence with its preceding space on normalized text).
11. **`EntryNamesR163`**: literal substring of the 16.13 heading in the entry section text (CRLF → LF).
12. **Map parse**: duplicate keys, BOM or non-UTF-8 → `MapParses` false. MV-3 uses the map's own `Surfaces`; MV-3
    to MV-6 are computed only when the map parses and has the basic shape (else MV-1 is already a defect and those
    fields are not compared). MV-5 keys use the normalized `Section`.
13. **MV-6** runs on every MODIFIED file of the map (a non-Markdown file has only `preámbulo`); a MODIFIED file
    absent in a revision has null derived sets and is a defect.
14. **Authority map part** (R4): Evaluate E2-E3 (duplicate trailer, trailer without derivable EFF, entry point
    without EFF, entry/16.13 count ≠ 1, entry not naming 16.13) plus every MV-1..MV-7 defect forbid `pass`;
    PRE_ACTIVATION (no trailer, no entry) leaves it unconstrained. 16.13 count at EFF is an extra internal check.
15. **R2 executable**: when `sys.executable` cannot be opened (Windows App Execution Alias, as on this machine),
    the image with the same name under `sys.base_prefix` is hashed; `Detail.ExecutableSource` says which.
16. **R3 vocabulary** (§1.8 + E1): keys must not contain the listed words; string values must not be exactly one
    of them (or start with `EXECUTION_`). Document text that merely contains e.g. `STOP` (16.3) is allowed. E1
    exemptions are applied per command and by exact Facts path (see below); `Classify.ClaimIdLines` and
    `Tests[].TestName` are echoes too but are not in the E1 list, so they stay checked.
17. **R5 scope extended** to `identity` and `hashes` envelopes, because ancestry and blob ids are critical
    independent primitives; R7 = the normative-reading rules listed above.
18. **R9**: the closure format is the acceptor's own. Stdin-driven reads (`--batch*`, `--stdin`) are violations
    because they are not declared in `GitCalls`; `-c` only with the two SPEC values; `-C` only the request repo;
    only `-n`/`--max-count` take a separate value. Pathspecs: `:(literal)` is stripped (E3); any other token that
    starts with `:` in a pathspec position (or with `:(` in a revision position) is a violation. Blobs are
    authorized only through `RevPaths` (E7); `Paths` only authorize pathspecs.
19. **Identity**: worktree paths compared with `normcase(abspath())`; ancestry is reflexive (as `merge-base
    --is-ancestor`); the rebase / `patch-id` branch of 16.9 #5 is not covered.
20. **Trailer**: declared value from `Verification.DeclaredCoAuthoredBy` or `handoff.Worker.Trailer` (with or
    without the `Co-Authored-By:` key); key ASCII case-insensitive; git trailer-block rules mirrored (title
    paragraph excluded, final paragraph, all trailers or ≥ 25 % with a git-generated prefix, continuation lines,
    whitespace before `:`); «coherente con el modelo efectivo» is not checked (needs routing facts); an empty
    range passes vacuously.
21. **`NotApplicable`** must be explicit; `MANUAL` (R8) does not block the decision.

## Errata v0.2 (SPEC §5) as applied by the acceptor

| Id | Change in the acceptor |
|---|---|
| E1 | `vocabulary_violations(command, facts)`: exempt, by exact Facts path, `trx` `Counters/**` and `Tests/*/Outcome`; `freetext` `Terms/*` and `Matches/*/Term`; `read-text` `Lines/*/Text`; `read-json` `Values/**`; `handoff` `Fields/**`; `clausemap` `Validate/MV7/{FirstSentenceS16, LastSentenceS163, S163EffMinusPointer, S163Eff1}` and `Pre/{HeaderCells, Rows, MalformedRows, ClaimIdCounts}/**`. `/**` exempts keys and values inside the node; a leaf path exempts only its value (the key, chosen by the tool, is still checked). Everything else the tool produces is still checked. |
| E2 | own `U19Normalized` = `[{"Entry", "List", "Prefix"}]`, Allowed then Forbidden, delegation order, `Prefix` = entry without `**`; R7 compares the list exactly (the earlier tolerant normalizer is removed). |
| E3 | R9 `pathspec_path`: `:(literal)<path>` → `<path>`; other magic (`:(glob)`, `:(literal,icase)`, `:!`, `:/`, ...) → violation. |
| E4 | R6: if the acceptor's own Derive has ≥ 2 trailer commits, or one trailer and no qualifying merge, an envelope with Facts is a MISMATCH (the tool must have failed with `AMBIGUOUS`); an error envelope is already rejected by R3/R6. With `--state-rev/--state-path` the acceptor reads the `claim_id` lines itself (per line: optional whitespace, `claim_id:`, optional whitespace, first non-space run), compares `Classify.ClaimIdLines`, and two or more distinct values with Facts present is a MISMATCH; repeats of one value are fine. No trailer stays a fact (`Eff` null). R4's map part already treated these cases as defects. |
| E5, E6 | already the acceptor's behaviour (terms after the colon; both rename paths). The fake tool now uses `--no-renames`. |
| E7 | already the acceptor's behaviour (blobs only via `RevPaths`). |
