# I-52 CT-DA research instrument

Research-only implementation of the V34 host fixture contract. Nothing in this directory is referenced by a
product project or by `RackCad.sln`.

## Components

- `native/`: ObjectARX 2025 x64/v143 helper loaded only into a dedicated scratch AutoCAD process.
- `harness/`: external .NET 8 controller, contract validator, tuple collector and process fence.
- `control-plane/`: registry, fixture metadata, total-order logging, evidence, cleanup and tuple models.
- `tests/`: toolchain-independent research test runner using synthetic records only.
- `fixture-v34.json`: deterministic fixture specification; runtime materialization remains pending.
- `traceability-v34.json`: generated one-to-one NPM-V34 implementation map.
- `EXTERNAL-BUILD.md`: reproducible cross-machine native build definition.
- `build.ps1`: builds both components without copying Autodesk SDK content into the repository.

Run `validate-r1b.ps1` to build and test every toolchain-independent component. This never starts AutoCAD.

## Host smoke

```powershell
dotnet run --project eng/research/I52Ctda/harness -c Release -- smoke <repo> <output-dir> <I52CtdaNative.arx> <scratch.dwg> [profile]
```

The caller supplies a fresh scratch DWG; the harness starts one dedicated AutoCAD process on it, loads the helper,
runs `I52CTDA_SMOKE`, unloads the helper and quits answering Yes to QUIT's "discard all changes" prompt. `I52CTDA_SMOKE` materializes the seven FEC-V34
fixture identities in its own bootstrap transaction, binds them, attaches the fixture reactors and detaches them
again; it dispatches no governed ProbeId. The event log defaults to `<output-dir>/native-smoke.json.events.jsonl`.
`smoke-result.json` is `PASS` only when the native report, the ordered event log, the external PID fence and
the scratch DWG integrity agree: after the PID is gone the DWG hash must equal the pre-run hash and no new
`<scratch>.bak` may exist (`SCRATCH_DWG_MUTATED`, `SCRATCH_BAK_CREATED`). Use a fresh scratch DWG per run.
The helper does not change AutoCAD security settings: the helper folder must already be trusted, or a dedicated
profile passed as `profile`.

## V34 binding clarifications (decisions §164)

- **SM-LINK storage.** The STATE-SM-0/1 linkage bytes live in one `AcDbXrecord` owned by the scratch NOD under
  `RACKCAD_CTDA_V34_SM-LINK`, with exactly one `kDxfText` resbuf: `HFV30:LINK:A,B` at SM-0, `HFV30:LINK:A,C` at
  SM-1. The carrier is created in the bootstrap transaction, is not a fixture identity (the count stays 7) and is
  removed by cleanup (`LINK-CARRIER-REMOVED`).
- **OPEN-SM-B.** Sibling membership is the SM-LINK relation plus liveness of the named members. F-REF-B is the M
  target and stays live; it leaves the sibling set only relationally. F-REF-C does not exist at bootstrap; MUT-SM
  appends exactly one `RACKCAD_CTDA_V34_REF` insert at `(20,20,0)` on `RACKCAD_CTDA_V30_A` and writes the carrier,
  in the caller's transaction. MUT-SM never erases, opens erased, or opens F-REF-B, and never writes F-XR.
- The native helper captures raw SM facts; `SmRules` (control plane) is their only classifier: structural
  anomalies are UNKNOWN, a valid carrier with wrong post-mutation bytes is FAIL-SM, and no anomaly reaches OK.
  `SmBindingGuard` checks the MUT-SM, MUT-ALL and bootstrap source bodies in `static-native`.

The smoke additionally requires C absent after bootstrap, SM-LINK at `A,B`, one resolved
`BINDING:SM-LINK|<handle>|PRESENT|STATE-SM-0|RESOLVED` snapshot line besides the seven identity lines, and the
carrier removed by cleanup.

The instrument must not run against a user's working AutoCAD process or project DWG. Runtime results are not
product evidence and cannot close a product KindContract.
