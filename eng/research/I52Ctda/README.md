# I-52 CT-DA research instrument

Research-only implementation of the frozen V35 execution contract (decision §172, `V35_FREEZE_PACKAGE_HASH`
`43DCE809…B6DF`). Nothing in this directory is referenced by a product project, by `RackCad.sln` or by `deploy/`.

## Components (R3)

- `native/`: R-NATIVE-ARX (`I52CtdaNative.arx`, ObjectARX 2025 x64/v143). A plan interpreter: `V35Ids.inc`,
  `V35PlanTable.inc` and `V35Authority.inc` are generated from the frozen catalog and NPM-V35 by
  `harness generate-v35` and must never be edited by hand. It owns LOG-SEQ-01 (the single log and sequencer, C ABI
  `I52Ctda_LogAppend`, `I52Ctda_TokenSet`, `I52Ctda_QueryC15Arm`, `int32_t I52Ctda_FinishFenceIsSet(void)`; the
  FINISH-FENCE-01 setter is internal), the completion-token table, FIN-GATE-01, the CommandResource commands
  (`I52CTDA_BOOT`, `I52CTDA_PROBE`, `I52CTDA_FIXTURE`, `HFV34_CANCEL`, `I52CTDA_QUEUED`, `I52CTDA_FINISH`) and the
  zero-ProbeId `I52CTDA_SMOKE`.
- `payload/`: R-PAYLOAD-ARX (`I52CtdaPayload.arx`), loaded only by `acedArxLoad(NL-ARX-PATH)`; PAYLOAD-DB-BINDING,
  C15 handshake, RR-PAYLOAD-DB with the fence read first, removal export `I52CtdaPayload_RemoveReactor`.
- `managed-observer/`: R-MANAGED-OBSERVER (`I52Ctda.ManagedObserver.dll`, net8.0-windows x64), RR-MANAGED-CMD
  (`Document.CommandEnded`), bound to LOG-SEQ-01 by `[DllImport("I52CtdaNative.arx")]` against the loaded module.
- `control-plane/V35/`: freeze guard, plan compiler, CONTROL-PLANE-RESULT-01 (`V35ResultEngine`, RESULT-RULE-V35),
  single-ProbeId launch plan and R3 smoke evaluator, static conformance.
- `harness/`: `freeze-v35`, `generate-v35`, `check-generated-v35`, `plan`, `conformance-v35`, `probe`, `smoke-v35`,
  `result-v35`, `tuple-v35` (plus the V34 commands, historical).
- `tests/`: research tests (V34 synthetic tests and R3 tests); no AutoCAD.
- `build-r3.ps1`: canonical build from an exact committed SHA into a versioned package outside the repository.

`validate-r1b.ps1` builds and runs every toolchain-independent check. It never starts AutoCAD.

## Single ProbeId (never run on the build machine)

```powershell
dotnet run --project eng/research/I52Ctda/harness -c Release -- probe <repo> <out> <run>\I52CtdaNative.arx <fresh-scratch.dwg> <ProbeId> [profile]
```

An unknown ProbeId is rejected before launch. One dedicated AutoCAD process per ProbeId runs DRIVER-SCRIPT-01
(BOOT, PROBE and the script-issued trigger only; FIN-GATE-01 issues FINISH and CMD-FINISH the exit). The control
plane enforces the external deadlines, verifies the exact PID exit and the scratch DWG, and only then classifies.

## R3 host smoke (next gate; zero ProbeIds)

```powershell
dotnet run --project eng/research/I52Ctda/harness -c Release -- smoke-v35 <repo> <out> <run>\I52CtdaNative.arx <fresh-scratch.dwg> [profile]
```

The run directory must hold the three modules together and be trusted by the dedicated profile (TRUSTEDPATHS).

## V34 binding clarifications (decisions §164, retained by V35)

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

The bootstrap snapshot holds the eight V35 identity lines (F-TRIGGER-XR added) plus one
`BINDING:SM-LINK|<handle>|PRESENT|STATE-SM-0|RESOLVED` line; the carrier is removed by CLN-BASE.

The instrument must not run against a user's working AutoCAD process or project DWG. Runtime results are not
product evidence and cannot close a product KindContract.
