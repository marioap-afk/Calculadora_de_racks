# Independent derivation (I-62, A-6 round 4)

`derive_indep.py` implements SPEC.md §1-§2 using only the standard library:
`python -I derive_indep.py <vector.json> <out.json>`. It exits 0 when it writes the closure and 2 on a bench error, in which case it writes nothing.
Every git call has the form `git --no-replace-objects -C <Repo> <read-only command>`. The script scrubs `GIT_*` from the environment and sets `GIT_OPTIONAL_LOCKS=0`.
The only input I used was SPEC.md. I did not read `a69.txt` or `table.txt`.

## Files

| Path | What it is |
|---|---|
| `derive_indep.py` | the implementation |
| `vectors/sample.json` | the SPEC §3 vector; its output is in `out/sample.json` |
| `make_synth.py` | builds `synrepo/` (git init plus plumbing; fixed dates, so the SHAs are reproducible), `synvec/` (E/D/H/K/TRX files plus `expect.json`) and `vectors/syn-*.json` |
| `run_synth.py` | runs every synthetic vector, writes `out/syn-*.json` and checks it against `synvec/expect.json` (exit code, Omitted, Commits, Refs). It also cross-checks RevPaths with a second git query (`cat-file --batch-check <c>:<p>`) and checks Paths and Files of `all_present` exactly. Log: `out/run_synth.log`. 24/24 PASS |

## Synthetic repository

- Main line: `c0` (root) has two children, `c1` and `s1`. `m` = merge(`c1`, `s1`) with first parent `c1`. `c2` (child of `m`) is the tip of `main`.
- Feature line: `c2` -> `r1` -> `x1` -> `g1` (`fx/syn`).
- An unrelated second root history: `u0` -> `u1` (`other`).
- `ALL` = {c0, c1, s1, m, c2, r1, x1, g1}. In it, `x1` comes **only** from RevList(c2..g1), and `s1` comes only from Parents(m).

Base inputs:
- D = {BaseSha c2, MainSha c2, ChainBaseSha c2, ChainRedSha r1, ExpectedBranch fx/syn, RunId RD001}, from custody.
- H = {BaseSha c2, CurrentSha g1, RedSha r1, RunId RH001}, from custody.
- K = {AuthorityRevision c1, MainSha c2, Authorities [auth-a, auth-b]}. Its bytes come from the `-C` (committed in `g1`).
- E.AuthorityRevision = c1. I.Target.Commit = g1.
- The TRX file comes from custody. M (`docs/map.json`) is committed.

## Expected results per synthetic vector (Omitted / Commits)

| Vector | Omitted | Commits |
|---|---|---|
| all_present | none | ALL |
| h_absent_unpinned (no E entry at the H path) | C1.H.BaseSha, C1.H.CurrentSha, C1.H.RedSha, C1.RevList, C6.H: INPUT_ABSENT | ALL - x1 |
| h_custody_missing / h_blob_mismatch / h_invalid_json / h_dupkey (nested) / h_bom / h_not_object | same as h_absent_unpinned | ALL - x1 |
| h_current_missing / _null / _empty | C1.H.CurrentSha FIELD_MISSING / FIELD_NULL / FIELD_EMPTY, plus C1.RevList PRIMITIVE_EMPTY | ALL - x1 |
| h_current_upper / _39hex / _tree / _nonexistent | C1.H.CurrentSha NOT_A_COMMIT, plus C1.RevList PRIMITIVE_EMPTY | ALL - x1 |
| h_red_null | C1.H.RedSha FIELD_NULL | ALL |
| d_absent_unpinned | C1.D.BaseSha, C1.D.MainSha, C1.D.ChainBaseSha, C1.D.ChainRedSha, C1.RevList, C1.FirstParentChain, C1.MergeBase, C2.origin/D.ExpectedBranch, C6.DELEG: INPUT_ABSENT | {c1, c2, r1, g1}; Refs = HEAD and origin/main only |
| k_absent_unpinned / k_invalid_json | C1.K.AuthorityRevision, C1.K.MainSha, C1.MergeBase, C4.K.Authorities, C6.CONTRACT: INPUT_ABSENT | ALL |
| k_authorities_empty | none (`[]` is not recorded) | ALL |
| d_branch_null | C2.origin/D.ExpectedBranch FIELD_NULL | ALL; Refs = HEAD and origin/main only |
| mergebase_none (K.AuthorityRevision = u1) | C1.MergeBase PRIMITIVE_EMPTY | ALL + u1 |
| target_nonexistent | C1.I.Target.Commit NOT_A_COMMIT | ALL (g1 still comes from H) |
| trx_unverifiable | none: exit 2 (bench error, SPEC §2.1) | none |

## Places where I had to choose

1. **"D.ChainRedSha si no es null" / "H.RedSha si no es null"**: I record a null as `FIELD_NULL`. §2.3 exempts only a list that is "legitimately empty" from being recorded. The other reading is that the table itself makes null legitimate, in which case nothing is recorded. This choice decides the only Omitted record of the sample.
2. **"sin resultado (`MergeBase` sin base común)"**: only MergeBase can have no result. A RevList that returns an empty set is a result and is not recorded. FirstParentChain is never empty.
3. **"Parents(m) ... con >= 2 padres"**: I add all parents of each merge on the chain, but not their ancestry. Parents come from `rev-list --no-walk --parents`.
4. **"`.../<H.RunId>/worker-handoff.json`"**: I read "..." as `artifacts/orchestration/<u>/<t>/<a>`, the same prefix as DELEG. Each of `<u>`, `<t>`, `<a>` is one non-empty path segment. A DELEG/H name can match a CanonicalInputs path other than the designated one.
5. **"`*.trx`"**: a path that ends in `.trx` (case-sensitive, any depth). When several names match, precedence is CONTRACT > DELEG > H > TRX.
6. **C6.CONTRACT when K is absent**: I omit it with `INPUT_ABSENT`, even when the contract bytes exist and verify (K absent only because they are not valid JSON). I read "todo término de la tabla que lee la entrada que falta" by analogy with "la entrada H de C6". C6.DELEG, C6.H and C6.CONTRACT are recorded as INPUT_ABSENT even when no CanonicalInputs entry would have carried that name.
7. **A present input whose named entry is missing from CanonicalInputs** (K pinned only in AllowedTransitiveInputs, or no path matches D.RunId): nothing is contributed and nothing is recorded, because no listed cause applies.
8. **A DELEG/H/CONTRACT-named entry whose input is present but whose bytes cannot be verified** (possible only for a non-designated path that matches the pattern): bench error, as for TRX. §2.1 specifies this only for TRX.
9. **"{p de cada entrada de E}"**: the `Path` of each element of CanonicalInputs, AllowedTransitiveInputs and AutomaticInstructions. It does not include Obligations[].SourcePath or RequiredBy.Path.
10. **READ obligation with Target `""`**: skipped without a record (rule ii; no label exists for it).
11. **K.Authorities**:
    - `[]` is not recorded.
    - Missing, null or `""` is recorded as FIELD_MISSING, FIELD_NULL or FIELD_EMPTY.
    - An element whose `Path` is missing, null or `""` is skipped. The term gets **one** record with the cause of the first defective element, even when other elements contribute. Partial omission is not addressed by the text.
12. **Empty values**: I treat `""`, `[]` and `{}` at the leaf as FIELD_EMPTY.
    - Intermediate `I.Target`: missing gives FIELD_MISSING, null gives FIELD_NULL, `""` or `[]` gives FIELD_EMPTY. `{}` gives FIELD_MISSING, because Commit is absent.
    - A commit field of the wrong JSON type gives NOT_A_COMMIT ("solo vale si es una cadena de 40 ...").
13. **Wrong JSON type in a non-commit field** is a bench error, because no cause is defined for it. This covers:
    - ExpectedBranch, D/H RunId, I.UnitId, I.TaskId and I.RunId when not a string;
    - Attempt when not an integer or string, or when boolean;
    - Authorities when not a list;
    - I.Target when not an object.
14. **Designated path with several E entries carrying different blobs**: bench error ("its Blob is the pinned blob" presumes one blob). The same path with the same blob in several lists counts as one pin.
15. **Designation key missing, null or `""`**: treated as "E no fija su blob", so INPUT_ABSENT. A K path that is not designated leaves CONTRACT undefined.
16. **M**: any of these is a bench error, because M has no term labels and §2.1 says every vector pins it:
    - E pins MapPath zero or several times;
    - the bytes are missing or unverified;
    - the bytes are not a strict JSON object;
    - Surfaces or Files is missing or of the wrong type.
17. **E**: parsed with the same strict parser; a failure is a bench error. A missing or null list counts as empty. A malformed entry (not an object, or Path/Blob not strings) is a bench error.
18. **Relative `E` / `Custody` / `Repo` paths** are resolved against the vector's directory. All my vectors use absolute paths.
    - An unreadable custody file (missing, a directory, no permission) counts as "no existen".
    - When an entry is not in Custody and its Blob is not 40 lowercase hex, its bytes "do not exist", and git is not asked.
19. **Refs** are names only; I do not check that they exist. ExpectedBranch `main` collapses into `refs/remotes/origin/main`.
20. **NOT_A_COMMIT** uses `git cat-file -t <value>` literally, once per value. A tag that points to a commit is still NOT_A_COMMIT, because nothing is peeled.
21. **Blob(c:p)** needs an exact path in the `ls-tree -r -z --full-tree` output of the tree, and that entry must have type blob. Symlinks are blobs and count; gitlinks do not. Paths are not normalized, so `./a` never matches.
22. **Strict JSON**:
    - Whitespace around the single value is allowed.
    - NaN, Infinity and -Infinity are rejected.
    - Duplicate keys are compared after unescaping, so `"a"` and `"a"` are duplicates.
    - Lone-surrogate escapes are accepted.
    - Python's integer-digit limit is lifted.
