# I-52-AUTH15-C1 host-validation harness (TEMPORARY, non-product)

Research instrument for the AutoCAD 2025 host validation of the corrective unit `I-52-AUTH15-C1`
(contract: `docs/initiatives/I-52-auth15-c1-unnamed-envelope.md`, Freeze `6e667d02`, section 7). It is an adaptation of the
`I-52-AUTH15` harness (last state `fcca6e6c`, package `D:\I52-AUTH15-HV\fcca6e6c`) and is **removed by a commit before the
Candidate**. Nothing here is product API, a command of the product, UI or a ribbon item; nothing in `src/`, `tests/`,
`RackCad.sln` or `deploy/` references it.

## Binding rule (unchanged from the previous harness)

The harness commit leaves `src/` and `tests/` **byte-identical** to the implementation SHA. `build-hostval.ps1` refuses to
build otherwise (`git diff --quiet`, equal `src`/`tests` tree hashes, only `eng/research/I52Auth15C1Host/` and `docs/` touched,
no ignored source/config file outside `bin/obj`). The evidence records `IMPLEMENTATION_SHA`, both trees of the implementation and
of the harness commit, and `treesEqual`.

## What changed with respect to the I-52-AUTH15 harness

- **Scope: DOCUMENT authority only.** The SIDE-DB characterization re-runs are gone, and so is the side-database control set. The
  expected controls are exactly 7 records, all `DOCUMENT-AUTHORITY`: RB-01V, RB-01D, RB-02a, RB-02b, RB-02c, RB-03, RB-05. The
  SIDE-DB / CT-DA characterization is not reopened and never decides this verdict.
- **HV-09 (InvalidEnvelope)** no longer lists a blank `Name`: a null envelope and a null/blank `Id` or `Kind` are the invalid cases.
- **HV-15 (new)** is the case of this unit: for HeaderRun and Cantilever, with `Name` = null, `""` and `"   "`, the call succeeds; the
  definition exists; the raw envelope on the definition equals `RackEmbedStore.Serialize(envelope)`; `RackBlockData.Read` returns the
  same JSON; the Name read back is exactly the one supplied (no fallback, no trim); the caller's envelope is unchanged; no reference is
  placed; the caller's transaction stays the live top transaction; the caller's abort leaves nothing (DOCUMENT database).
- The launcher expects cases HV-00..HV-15 (all PASS), the 7 controls, and HV-15 among the rollback-sensitive cases on the DOCUMENT database.
- The offline characterization rig is not carried over.

## Contract matrix (H-1..H-10) to case mapping

| Row | Evidence |
|---|---|
| H-1, H-2, H-3 | HV-15: null / empty / whitespace `Name`, both families (DOCUMENT) |
| H-4 | HV-15: raw envelope == serialized envelope, read-back equal, Name read back exactly the supplied one |
| H-5, H-6, H-7 | HV-09: blank `Id`, blank `Kind`, null envelope -> `InvalidEnvelope`, no write (a database-kind-independent check, side database as in the previous harness; no rollback claim) |
| H-8 | HV-08: blank `requestedBlockName` -> `InvalidBlockName` (DOCUMENT) |
| H-9 | HV-06: a transaction that is not the top one of the database (foreign, outer-while-nested, disposed, null) -> `TransactionMismatch`; the `OpenCloseTransaction` characterization is recorded, never counted (side database, no rollback claim) |
| H-10 | HV-15 and HV-14: the transaction stays caller-owned and live, no internal commit, no reference placement, the caller's abort discards the definition (DOCUMENT) |

HV-00..HV-14 are kept as regression of the unchanged AUTH-15 behavior (they passed on the previous implementation under DOCUMENT authority).

## Build and run

`build-hostval.ps1 -HarnessSha <40 hex> -ImplementationSha <40 hex>` writes `D:\I52-AUTH15C1-HV\<HARNESS_SHA8>\{run,out,launcher,logs}`,
`SHA256SUMS`, `TRANSFER-METADATA.json` and a zip. It never starts AutoCAD. `launcher\run-hostval.ps1 -Package <folder> -ScratchDrawing <blank .dwg>
-OwnerConfirmsNoTouch` launches ONE AutoCAD process; the Owner must have trusted `<folder>\run` in `TRUSTEDPATHS` beforehand (the scripts never
change a security setting) and must not touch the computer until it exits. A run is never repeated or overwritten without authorization.
