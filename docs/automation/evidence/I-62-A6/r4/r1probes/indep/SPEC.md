# Independent implementation of the acceptor-closure derivation (I-62, A-6 round 4)

You implement, **only from the normative text quoted below**, the mechanical derivation of the acceptor closure
(`rackcad-bmin-closure/0-experimental`) from an effective input closure E (`rackcad-input-closure/v1`) and the inputs it pins.
A second implementation exists elsewhere; the two will be compared. Do not look for it, and do not read any file under
`docs/automation/evidence/I-62-A6/`, any `*.py` under `D:\r62-a6r2`, `D:\r62-a6r3` or `D:\r62-a6r4\r1`, or any session scratchpad.

## 1. Normative text (Spanish; literal)

### 1.1 A6-9, rule 4 (unchanged)

> 4. el cierre de lecturas del aceptante se deriva mecánicamente del cierre efectivo de insumos de la invocación y de las entradas que ese cierre
>    fija por blob (la delegación, la entrega, el contrato y la invocación del rol), con la tabla de A-6 y con operaciones de grafo de Git fijadas,
>    sin ser una segunda definición de las lecturas permitidas, y se custodia con la solicitud;

### 1.2 The derivation table (A6-9, rule 4; unchanged)

```text
DERIVACIÓN  rackcad-input-closure/v1 (E) -> rackcad-bmin-closure/0-experimental
Entradas: E; la delegación D, la entrega H y el contrato K cuyos blobs fija E; la invocación del rol I (UnitId, TaskId, Attempt, RunId,
Target); el repositorio del -C (solo primitivas de grafo).

C1 Commits  = {E.AuthorityRevision} U {D.BaseSha, D.MainSha, D.ChainBaseSha, D.ChainRedSha si no es null}
              U {H.BaseSha, H.CurrentSha, H.RedSha si no es null} U {I.Target.Commit} U {K.AuthorityRevision, K.MainSha}
              U RevList(D.BaseSha..H.CurrentSha)
              U FirstParentChain(D.MainSha) U Parents(m) para cada m de FirstParentChain(D.MainSha) con >= 2 padres
              U {MergeBase(D.MainSha, K.AuthorityRevision)}
C2 Refs     = {HEAD, refs/remotes/origin/<D.ExpectedBranch>, refs/remotes/origin/main}
C3 RevPaths = {(c, p) : c en Commits, (p, b) en E.CanonicalInputs U E.AllowedTransitiveInputs U E.AutomaticInstructions,
              Blob(c:p) = b}                                   (identidad por blob)
C4 Paths    = {p de cada entrada de E} U {o.Target : o en E.Obligations, o.Class = READ, o.Target no null}
              U Surfaces(M) U {f.Path : f en Files(M)} U {a.Path : a en K.Authorities} U {IgnorePath(I)}
              M = el mapa de cláusulas cuyo blob fija E en su ruta fija;
              IgnorePath(I) = artifacts/orchestration/<I.UnitId>/<I.TaskId>/<I.Attempt>/<I.RunId>/output.json
C5 Objects  = {}                                               (árboles de Commits y blobs de RevPaths, implícitos)
C6 Files    = {(Name(p), SHA256(bytes(p, b))) : (p, b) en E.CanonicalInputs, Name(p) definido}
              Name: ruta del contrato -> CONTRACT; artifacts/orchestration/<u>/<t>/<a>/<D.RunId>/output.json -> DELEG;
              .../<H.RunId>/worker-handoff.json -> H; *.trx -> TRX
              bytes(p, b) = los bytes custodiados cuyo blob Git es b (verificados)
```

### 1.3 A6-3, rule 6 (b) — the corrected rule you must apply (round 4)

> (b) `Repo` = el `-C`; `Tool` = el blob fijado, ejecutado desde el objeto del `-C`; `Interpreter` = el intérprete declarado; `Closure` = el
> cierre derivado por la regla de A-6 sobre la independencia, con su tabla aplicada así cuando una de sus entradas no da un valor:
> (i) la delegación D, la entrega H o el contrato K faltan si E no fija su blob, o si sus bytes custodiados no existen, no tienen ese blob o no
> son un único objeto JSON en UTF-8 sin claves repetidas. Se omite entonces todo término de la tabla que lee la entrada que falta; con la
> entrega ausente (N4), H.BaseSha, H.CurrentSha y H.RedSha en C1, RevList(D.BaseSha..H.CurrentSha) y la entrada H de C6;
> (ii) un campo que la tabla lee y que no está, es `null` o está vacío no aporta ningún término. Cada valor de commit que la tabla toma de un
> campo de E, D, H, K o I, como término de C1 o como extremo de una primitiva de grafo (incluido I.Target.Commit), solo vale si es una cadena
> de 40 caracteres hexadecimales en minúscula que `git --no-replace-objects cat-file -t` da como `commit` en el `-C`; si no (en nc1,
> H.CurrentSha), se omite;
> (iii) una primitiva de grafo con un extremo omitido, o sin resultado (`MergeBase` sin base común), da el conjunto vacío; C3 y los demás
> términos se calculan sin cambio sobre lo que queda;
> (iv) un término omitido no se sustituye por ningún otro valor: ni por otra entrega o una copia suya, ni por otro commit, una ref o el
> `Target`, ni por el cierre de una prueba general;
> (v) la derivación registra cada término omitido con su causa (entrada ausente; campo ausente, `null` o vacío; valor que no nombra un commit
> existente), y ese registro se custodia con la solicitud y el cierre.

## 2. Interface (fixed by the bench, so that the two implementations can be compared)

`python -I derive_indep.py <vector.json> <out.json>` — standard library only; git via `subprocess` with `--no-replace-objects` on every call.

### 2.1 Vector (input)

```json
{
  "Repo": "<absolute path of the -C repository>",
  "E": "<path of the rackcad-input-closure/v1 JSON file>",
  "I": {"UnitId": "...", "TaskId": "...", "Attempt": 0, "RunId": "...", "Target": {"Commit": "...", "Path": "...", "Blob": "..."}},
  "Designation": {"D": "<E-namespace path of the delegation>", "H": "<... of the delivery>", "K": "<... of the contract>"},
  "Custody": {"<E-namespace path>": "<local file holding the custodied bytes>"},
  "MapPath": "docs/automation/agent-execution/compatibility/I62-clause-map.json"
}
```

- **Which entry is D, H or K.** `Designation` names the path that the kit gives to each of them in the invocation (the table presupposes this
  identification; it is not part of the rule). "E fija su blob" means: E has an entry (in `CanonicalInputs`, `AllowedTransitiveInputs` or
  `AutomaticInstructions`) whose `Path` equals that designated path; its `Blob` is the pinned blob. A designated path may have no E entry.
- **Bytes of an E entry.** If the path is a key of `Custody`, the bytes are that local file (it may not exist). Otherwise the bytes are
  `git --no-replace-objects -C <Repo> cat-file blob <Blob>`. "Verified" = the Git blob id of the bytes (SHA-1 of `blob <len>\0<bytes>`) equals
  the pinned blob.
- `D`, `H` and `K` are JSON objects (when present); field names as in the table (`BaseSha`, `MainSha`, `ChainBaseSha`, `ChainRedSha`,
  `ExpectedBranch`, `RunId`; `BaseSha`, `CurrentSha`, `RedSha`, `RunId`; `AuthorityRevision`, `MainSha`, `Authorities[].Path`).
- `I` is given inline; its fields follow the same rule (ii) (a missing, null or empty field gives no term).
- `M` is the JSON object at the bytes of the E entry whose path is `MapPath` (every vector pins it); `Surfaces(M)` = `M["Surfaces"]`,
  `Files(M)` = `M["Files"]`.
- "Ruta del contrato" in the `Name` rule = the designated K path. `Name` for DELEG and H uses `D.RunId` / `H.RunId`: if D or H is absent, its
  name is undefined. For an entry named TRX, unverifiable bytes are a bench error (exit non-zero), not an omission.

### 2.2 Graph primitives (semantics; choose your own git commands)

- `RevList(a..b)`: the set of commits reachable from b and not from a.
- `FirstParentChain(x)`: x and every first-parent ancestor down to a root.
- `Parents(m)`: the parents of m.
- `MergeBase(x, y)`: the single commit printed by `git merge-base x y`; no common ancestor = no result.
- `Blob(c:p)`: the blob id of path p in the tree of commit c; undefined if p is absent or is not a blob.

### 2.3 Output

```json
{
  "Closure": {"Schema": "rackcad-bmin-closure/0-experimental", "Commits": ["..."], "RevPaths": [{"Rev": "...", "Path": "..."}],
              "Refs": ["..."], "Paths": ["..."], "Objects": [], "Files": [{"Name": "...", "Sha256": "..."}]},
  "Omitted": [{"Term": "<label>", "Cause": "<cause>"}]
}
```

Lists may come in any order (the comparison sorts them); no duplicates.

**Term labels** (use exactly these):
`C1.E.AuthorityRevision`, `C1.D.BaseSha`, `C1.D.MainSha`, `C1.D.ChainBaseSha`, `C1.D.ChainRedSha`, `C1.H.BaseSha`, `C1.H.CurrentSha`,
`C1.H.RedSha`, `C1.I.Target.Commit`, `C1.K.AuthorityRevision`, `C1.K.MainSha`, `C1.RevList(D.BaseSha..H.CurrentSha)`,
`C1.FirstParentChain(D.MainSha)` (it also covers its `Parents(m)`), `C1.MergeBase(D.MainSha,K.AuthorityRevision)`,
`C2.origin/D.ExpectedBranch`, `C4.K.Authorities`, `C4.IgnorePath(I)`, `C6.CONTRACT`, `C6.DELEG`, `C6.H`.

**Causes** (use exactly these):
- `INPUT_ABSENT`: the term reads D, H or K and that input is absent by (i) (this cause wins over any other for that term);
- `FIELD_MISSING`, `FIELD_NULL`, `FIELD_EMPTY`: (ii), the field the term reads is not there / is `null` / is `""` (or an empty list where a
  value is required);
- `NOT_A_COMMIT`: (ii), a commit value that is not 40 lowercase hex or that `cat-file -t` does not report as `commit`;
- `PRIMITIVE_EMPTY`: (iii), a primitive with an omitted end or with no result (its inputs being present).

One record per omitted term. A term that contributes nothing only because a list is legitimately empty (e.g. `K.Authorities = []` when
present) is not recorded. `C2` keeps `HEAD` and `refs/remotes/origin/main` always.

## 3. Sample vector (read-only data from a previous round; do not write there)

```json
{
  "Repo": "D:/r62-a6r2/r3/deliv",
  "E": "D:/r62-a6r2/r3/runs/d/EIC-VERIFY.input-closure.json",
  "I": {"UnitId": "FX-U1", "TaskId": "T1", "Attempt": 0, "RunId": "R20261012T092000Z-c001",
        "Target": {"Commit": "b417d36cb820a27c78d6c7d50c234460469c89f8", "Path": "src/Fixture.Lib/Calculator.cs",
                   "Blob": "380892ee869194aa4bd56dcf32dfd76ef0203ab9"}},
  "Designation": {"D": "artifacts/orchestration/FX-U1/T1/0/R20261012T090000Z-d001/output.json",
                  "H": "artifacts/orchestration/FX-U1/T1/0/R20261012T091000Z-a001/worker-handoff.json",
                  "K": "docs/automation/decisions/FX-U1-T1.gate-contract.json"},
  "Custody": {"artifacts/orchestration/FX-U1/T1/0/R20261012T090000Z-d001/output.json": "D:/r62-a6r2/r3/runs/d/inputs/deleg-deliv.json",
              "artifacts/orchestration/FX-U1/T1/0/R20261012T091000Z-a001/worker-handoff.json": "D:/r62-a6r2/r3/runs/d/inputs/handoff-deliv.json",
              "artifacts/ci/R37949808974/fixture-tests.trx": "D:/r62-a6r2/r3/runs/d/inputs/fixture-tests.trx"},
  "MapPath": "docs/automation/agent-execution/compatibility/I62-clause-map.json"
}
```
