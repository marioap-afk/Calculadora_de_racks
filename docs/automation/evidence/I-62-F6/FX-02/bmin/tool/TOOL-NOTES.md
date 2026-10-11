# B-min facts tool: notes

Experimental, non-normative (I-62, decisions 75). Built only from `SPEC.md` and
`docs/AUTOMATION_PLAN.md` blob `f525cb1e` (sections 16.3, 16.5, 16.9, 16.10, 16.13, 16.14,
16.22, 16.24) plus the schemas `clause-map.schema.json`, `gate-contract.v2`,
`delegation.v2` and `worker-handoff`, read with `git show` from the architecture
worktree under `%USERPROFILE%\.codex\worktrees\`. No probe, `mv_check`, `clause_map`, evidence
folder, kit or `accept/` content was opened. No model CLI and no network command was run.

## Files

| File | Role |
|---|---|
| `bmin_facts.py` | the tool (Python 3 stdlib, ASCII-only source; non-ASCII constants use `\N{...}`) |
| `tests/test_bmin_facts.py` | unittest suite (unit, negative fail-closed, zero-write, determinism, vocabulary, static AST) |
| `tests/mutate.py` | mutation runner (fixed list of textual mutants applied to a copy of the tool) |
| `tests/zero_write_check.py` | standalone zero-write checker for one command on any clone (used for the fixture sanity run) |

## How to run

```powershell
# the tool (refuses to run without -I: NOT_ISOLATED, exit 2)
python -I bmin_facts.py <command> --repo <clone> ...
git cat-file blob <blob> | python -I - <command> --repo <clone> ...

# the suite (temp repos under D:\r62-bmin-tmp\tool\suite-*, removed at the end)
python -m unittest discover -s tests -p test_bmin_facts.py -v
#   BMIN_TOOL=<path>  test another copy of the tool;  BMIN_TMP=<dir>  temp root

# mutation testing (copies under D:\r62-bmin-tmp\tool\mut-*, removed per mutant)
python tests\mutate.py --workers 3            # [--only M01,M02] [--no-baseline]

# zero-write check of one command on a clone
python tests\zero_write_check.py --repo <clone> -- clausemap --repo <clone> --main <sha>
```

From Git Bash, export `MSYS_NO_PATHCONV=1` before passing JSON pointers such as `/Schema`;
otherwise MSYS rewrites them into Windows paths (the tool then answers USAGE).

## What the suite covers

Synthetic repositories are built by the suite with one deterministic `git fast-import`
stream each (fixed author/committer, increasing timestamps, `--no-ff` merges, trailers,
non-ASCII paths and text such as `«`, `—`, `ó`, `ñ`), plus one small repo with a dirty
worktree built with plain git commands. All git used by the suite runs with
`GIT_CONFIG_NOSYSTEM=1` and an empty global config.

| Test class | Covers |
|---|---|
| `TestEnvelopeAndRuntime` | NOT_ISOLATED without `-I`; `Runtime.Isolated`; USAGE (exit 64) cases; execution from the Git object via stdin (same bytes as from the file); byte-identical stdout over two runs for clausemap (good and defective), read-text and diff |
| `TestIdentity` | Head, AbbrevRef, Toplevel with `/`, origin refs present/absent (null), the three ancestry keys including false cases |
| `TestDiff` | Paths in Git order with a non-ASCII path; rename reported as both paths; Commits order; Co-Authored-By values (two trailers, non-ASCII name) |
| `TestScope` | PerPath coverage, `docs/**` (U-19) coverage, MalformedEntries (absolute, `..`, backslash, wildcard), U19Normalized, entries tal cual, case sensitivity |
| `TestContract` | A3, A4, A5Missing (five keys), RequiredTestsPairs, SupersededEqual true/false; missing required field (MISSING_INPUT); repeated delegation Filter (AMBIGUOUS) |
| `TestHashesAndCleanTree` | raw blob SHA-1 and SHA-256 of the dirty worktree vs the revision; absent path and directory give nulls; status lists, check-ignore rc 0 and rc 1 |
| `TestReadText` | Lines without EOL (CRLF file), `--lines`, NonAsciiCount/NonAsciiChars, missing path and out-of-range lines (MISSING_INPUT) |
| `TestReadJson` | RFC 6901 pointers (`~0`, `~1`, indexes, leading zero, absent) from a revision and from a file; trailing comma, duplicate key and NaN (MALFORMED_JSON) |
| `TestTrx` | Counters, Tests, FailedTests, filter lists and counts; namespaced and plain TRX give the same facts |
| `TestHandoff` | MissingRequiredKeys against the schema at the revision, Fields with present-null vs absent, invalid JSON as a fact, `--allow-absent`, schema absent at the revision |
| `TestFreeText` | Terms of 16.10 (field names and `Gate` excluded), casefold whole-word matching per element, sorted Matches; AP without 16.10 (CONTRADICTORY) |
| `TestClauseMapGood` | Derive (decoy commits that only contain the trailer as a substring), Pre, Classify on a `/v2`-like state, Entry, MV-1..MV-7 with every defect list empty, pointer sharing its line with another sentence, Resolve (5 citations, ExtDiffMbAr, ArBaseDiff), declared Inputs |
| `TestClauseMapDefect` | map with known defects: MODIFIED with wrong BaseBlob, ADDED already present in Eff^1, ENTRY file that is not the schema, ENTRY file present in Eff^1, duplicated Files path, unchanged MODIFIED file, phantom ADDED file, unlisted and deleted files, duplicated entry key, entry on a non-MODIFIED path, extra ENTRY entry, duplicated headings, missing pointer in §16, 16.3 changed besides the pointer, repeated entry point, PRE with malformed rows and a duplicated Claim-Id |
| `TestClauseMapOtherStates` | pre-activation (no trailer: Eff null, Validate null), schema violations (10 pointer/keyword pairs, including `$` against a trailing newline), map absent |
| `TestNegativeFailClosed` | SPEC 4 list: nonexistent SHA, absent file, SHA-256 mismatch, unreadable JSON, unreadable XML (and DOCTYPE), duplicated trailer, ambiguous EFF, unsupported schema keyword, forbidden git subcommand through the CLI (6 forms, call never issued), no `-I`, state with two distinct claim_id. Each: Facts null, a code, exit 2 |
| `TestRunnerUnit` | the single runner with a fake `subprocess`: forced `-c` flags, `--no-ext-diff --no-textconv`, `--porcelain`, env (`GIT_OPTIONAL_LOCKS=0`, `GIT_TERMINAL_PROMPT=0`, `LC_ALL=C`, inherited `GIT_DIR` dropped), `shell=False`; 14 forbidden forms rejected before any process starts |
| `TestStaticSource` | AST: imports allowlist, no write/network/dynamic APIs (`open` only `"rb"`, `.write` only on `sys.stdout.buffer`, `os` limited to `environ`/`path`), `subprocess.run` only inside `git_exec` with `["git"] + argv` and `shell=False`; ASCII source; no forbidden word in any string literal |

Checks applied to every tool run inside the suite: zero writes (sha256 of `.git/index`,
`git for-each-ref`, `git status --porcelain --ignored`, every file of the clone with size and
mtime, before and after), envelope keys and canonical serialization, exactly one line, every
stdout byte < 0x80, `FactsSha256` recomputed, exit code vs Error, every `GitCalls` entry starts
with the forced flags and uses an allowed subcommand, `Inputs` shapes, and the vocabulary check
(a forbidden word may appear in Facts only inside a string that is a verbatim echo of input data).

## Results

Tool under test: `bmin_facts.py`, SHA-256 `b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200`
(git blob `9a829b0eeb1210238052b1feac06cfa8f7964aed`), Python 3.13.14, Git 2.54.0.windows.1.

- **Suite:** `Ran 66 tests ... OK` (66 ok, 0 failures, 0 errors), both as the mutation baseline
  (222 s) and in the final verbose run (143 s). Wall time is dominated by process start-up on
  this machine (about 0.15 s per git call and 0.2 to 1.8 s per Python start).
- **Zero writes:** no repository change in any tool run of the suite (checked before and after
  each run); the AST scan passes.
- **Determinism / ASCII:** byte-identical stdout over two runs for 4 command lines; every stdout
  byte < 0x80 in every run.
- **Mutation (`tests/mutate.py --workers 3`): 27 killed / 27, 0 survived, 0 invalid.** Every
  mutant was killed by its hint tests, so the full-suite fallback was never needed.

| Id | Mutant | Killed by |
|---|---|---|
| M01 | coverage rule inverted | `TestScope.test_scope_coverage` |
| M02 | sentence boundary off | `TestClauseMapGood.test_validate_mv7_pointer_shares_line` |
| M03 | ancestry argument order swapped | `TestIdentity.test_identity_facts` |
| M04 | one diff path dropped | `TestDiff.test_paths_commits_coauthors` |
| M05 | MV-4 lists merged | `TestClauseMapDefect.test_mv4_four_lists` |
| M06 | pointer removed with its whole line | `TestClauseMapGood.test_validate_mv7_pointer_shares_line` |
| M07 | ENTRY treated as ADDED | `TestClauseMapDefect.test_mv4_four_lists` |
| M08 | first-parent order reversed | `TestClauseMapGood.test_derive` |
| M09 | trailer matched by substring | `TestClauseMapGood.test_derive` |
| M10 | `ensure_ascii=False` | `TestReadText.test_non_ascii_fidelity` |
| M11 | missing `-I` check removed | `TestEnvelopeAndRuntime.test_not_isolated_is_refused` |
| M12 | U-19 normalization removed | `TestScope.test_scope_coverage` |
| M13 | raw blob header without NUL | `TestHashesAndCleanTree.test_hashes` |
| M14 | free-text word boundary removed | `TestFreeText.test_terms_and_matches` |
| M15 | closed list of subcommands removed | `TestRunnerUnit.test_closed_list` |
| M16 | duplicated trailer accepted | `TestNegativeFailClosed.test_duplicate_trailer` |
| M17 | unsupported schema keyword accepted | `TestNegativeFailClosed.test_unsupported_schema_keyword` |
| M18 | MV-3 deletions dropped | `TestClauseMapDefect.test_mv2_mv3` |
| M19 | two distinct claim_id accepted | `TestNegativeFailClosed.test_state_with_two_claim_ids` |
| M20 | last sentence of 16.3 taken as first | `TestClauseMapGood.test_validate_mv7_pointer_shares_line` |
| M21 | EFF candidate without first-parent condition | `TestClauseMapGood.test_derive` |
| M22 | preamble omitted from H1/H2 | `TestClauseMapDefect.test_mv6` |
| M23 | `status --porcelain` not forced | `TestRunnerUnit.test_forced_flags_and_env` |
| M24 | code fences ignored | `TestClauseMapGood.test_validate_mv6` |
| M25 | duplicate JSON keys accepted | `TestReadJson.test_malformed_and_duplicate_keys` |
| M26 | `--no-ext-diff --no-textconv` not forced | `TestRunnerUnit.test_forced_flags_and_env` |
| M27 | claim_id lines counted instead of distinct values | `TestClauseMapGood.test_pre_and_classify` |

The mutants were killed by this suite alone; the acceptor was not used (its author works
independently).

## SPEC ambiguities and how they were resolved

1. **Trailer duplicated, EFF ambiguous, two claim_id (2.12 vs 4).** 2.12 says "empty list/null"
   for these, 4 requires fail-closed errors. Resolved: no trailer is the normative absence fact
   (`Candidates` = [], `Eff` = null); two or more trailer commits, or a unique trailer with no
   qualifying first-parent merge (with first-parent order at most one can qualify), give
   `AMBIGUOUS`. For the state, `ClaimIdLines` lists every matching line; 0 values gives
   `ClaimIdInPre` = null; **two or more distinct values** give `AMBIGUOUS`. Two lines with the
   same value are not ambiguous: a real `/v2` state carries `automation_state.claim_id` and
   `protocol.basis.claim_id` (found on the fixture clone; covered by a test and mutant M27).
2. **16.10 terms.** The paragraph also puts the seven field names in backticks. Terms = backtick
   spans before "No se aplica", minus those field names (so `Gate`, after the cut, is excluded too).
3. **`git diff --name-only`.** Run as `diff --name-only --no-renames -z`: same list and Git order,
   but a rename yields both paths regardless of `diff.renames`, and names are NUL-separated.
   Path arguments use `:(literal)` (diff would glob `*`). `GIT_LITERAL_PATHSPECS` was not used
   because it breaks `check-ignore`.
4. **Environment.** Besides the three required variables, inherited `GIT_*` variables are dropped
   (no `GIT_DIR`/`GIT_INDEX_FILE` redirection). Extra forbidden tokens: `--output*`, `--ext-diff`,
   `--textconv`. Forbidden tokens are checked in the option part (before `--`). `log`/`show` add
   `--no-show-signature`.
5. **Vocabulary (1.8).** The tool never originates those words (static check of every string
   literal). Echoed data stays verbatim for fidelity: on the real AP, `Terms` contains `GATE PASS`;
   TRX `Counters` keys are `passed`/`failed`. The dynamic check accepts a hit only if the string is
   present verbatim in the input data. The match is case-sensitive (SPEC key `FailedTests`).
6. **Commits.** Lowercase 40-hex only; the SHA must peel to itself as a commit (a tag object SHA
   is BAD_REVISION).
7. **JSON.** Strict: UTF-8, no BOM, unique keys, no NaN/Infinity; otherwise MALFORMED_JSON
   (handoff: `ValidJson` = false). `read-json` adds `AbsentPointers` and `handoff` adds
   `AbsentFields`, because a real value `{"Absent": true}` would be indistinguishable.
8. **read-text.** `NonAscii*` count the selected lines; lines beyond the end and non-UTF-8 blobs
   give MISSING_INPUT.
9. **trx.** `FilterSelected`/`FilterFailed` are name lists, plus `FilterSelectedCount`/
   `FilterFailedCount` (null without the filter). All `UnitTestResult` elements in document order.
   A DOCTYPE/ENTITY or UTF-16 TRX is MALFORMED_XML; missing ResultSummary/Counters is
   MISSING_INPUT, repeated is AMBIGUOUS.
10. **hashes.** A directory or any non-blob gives null values.
11. **scope.** `..` means a path segment; `MalformedEntries` and `U19Normalized` are objects
    (`List`, `Entry`, `Reasons`/`Prefix`); covered-by lists hold the original entries. Malformed
    entries still take part in the literal coverage.
12. **contract.** U-19 is not applied (SPEC names it only for scope); containment is the AP L553
    rule. The required field list of `gate-contract/v2` is embedded (the command has no revision
    for the schema). A delegation with a repeated `Filter` is AMBIGUOUS.
13. **Markdown.** Heading = line starting with 1-6 `#` then space/tab/end, no leading spaces;
    fences = lines starting with three backticks. `preámbulo` is always an element of H1/H2 when
    the file exists; duplicated headings compare as ordered lists of section texts. MV-5 keys use
    the normalized Section.
14. **Pre.** Literal: a Markdown separator line `|---|` is a row (counted as `---`); at most one
    empty cell is removed at each end; `MalformedRows` = `{RowIndex, CellCount}`; two `Claim-Id`
    header cells is AMBIGUOUS.
15. **MV-1.** Unparseable schema in Eff is MALFORMED_JSON; map-dependent facts (MapSurfaces, MV-3,
    MV-4, MV-5, MV-6 per file) are computed only when `SchemaViolations` = [], otherwise null;
    a schema-valid map without the 16.13 shape is CONTRADICTORY. A `required` violation points to
    the missing key; `pattern` uses ECMA `$` semantics (`\Z`) with ASCII classes.
16. **MV-7.** Extra counts `S16Count`, `S163CountEff`, `S163CountEff1`. The pointer removed is the
    last whole-sentence occurrence, with its preceding space.
17. **Resolve.** `MainSha` = `--main`; extra `ContractMainSha`; no merge base is GIT_ERROR; a path
    listed twice in `Files` is AMBIGUOUS; with Eff null the Eff-dependent fields are null; Match of
    `preámbulo` is 1 when the file exists; other sections (also "documento completo") are counted
    literally; `--base` is an optional argument of `clausemap` (for `ArBaseDiff`).
18. **freetext.** Whitespace is collapsed before matching; each array element is matched on its own;
    a missing field is MISSING_INPUT; `Fields` is the list of the seven names.
19. **Other.** `FactsSha256` hashes the canonical Facts without the final newline. `GitCalls` holds
    the full argv after `git` (forced `-c` flags included). An unforeseen internal exception is
    reported fail-closed as `CONTRADICTORY` ("unexpected internal condition"). With
    `--allow-absent` and no file, no `Inputs` entry is added.

## Sanity run on the real fixture clone

Fresh clone `git clone --no-local -c core.autocrlf=false -b fx/u1 D:/r62-fixture/fixture-origin.git`
into `D:\r62-bmin-tmp\tool\fixture-clone` (main `fbe25347`, HEAD `95bdc29d`), used read-only;
each command ran through `tests/zero_write_check.py`. Inputs outside Git were extracted with
`git show` into a temporary folder: the T1 contract (`docs/automation/decisions/FX-U1-T1.gate-contract.json`,
also used as DELEG because the fixture has no delegation yet) and a small synthetic free-text
handoff. This is only a check that the commands run on real data. It is not a comparison with any oracle.

| Command | Outcome |
|---|---|
| `identity --branch fx/u1 --base fbe25347 --current 95bdc29d` | facts |
| `diff --from fbe25347 --to 95bdc29d` | facts |
| `scope` (same range, DELEG = T1 contract) | facts (31 paths, 4 U-19 entries) |
| `contract` (CONTRACT = DELEG = T1 contract) | facts |
| `hashes --rev 95bdc29d` (2 paths + 1 absent) | facts |
| `cleantree --ignore-path artifacts/orchestration/x.json` | facts |
| `read-text --rev fbe25347 --path docs/AUTOMATION_PLAN.md --lines 717-722` | facts |
| `read-json --rev 95bdc29d` (contract, 3 pointers) | facts (USAGE in a first attempt only because Git Bash rewrote `/Schema`; with `MSYS_NO_PATHCONV=1` it gives facts) |
| `handoff --allow-absent` (no handoff in the fixture) | facts (`Present` = false) |
| `freetext --terms-rev fbe25347` (synthetic handoff) | facts (16 terms read from the real 16.10) |
| `clausemap --main fbe25347 --state-rev 95bdc29d --state-path docs/automation/state/FX-U1.yml --base 95bdc29d` + CONTRACT | facts (all six blocks; 42 git calls) |
| `trx` | not run: the fixture has no TRX file locally |

Every run: zero writes (snapshots equal) and ASCII-only stdout. Preparing the clausemap run
exposed the two equal `claim_id` lines of a real `/v2` state (ambiguity 1 above). The tool was
fixed before the final suite and mutation runs.
