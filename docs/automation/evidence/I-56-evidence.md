# I-56 — Canonical evidence

## Identity and transition

| Field | Value |
|---|---|
| Unit | `I-56` |
| Initiative | I-56 — Initiative Workflow V2 |
| Workflow | V1 grandfathered |
| Claim-Id | `82946e97-508a-4e0e-9b9c-09b9936be121` |
| Transition | T1 / pre-effective claim / remains V1 |
| Original claim base | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| Original claim commit | `50ba035b53450c9a601024f3925fbb316234eb93` |
| Original claim time | `2026-09-14T09:39:43-06:00` |
| Effective publication | `2026-09-17T20:26:34Z` |

The claim commit and its first accepted remote claim predate both the activation-pause START and the
effective publication. The complete PRE embedded in the normative merge confirms the same Claim-Id and
branch before effectiveness. I-56 therefore remains governed by Workflow V1.

## Design, decision and conformance identities

| Evidence | Result |
|---|---|
| Proposal | V4, commit `94a4e446b76ff28d8a434252fb859c071cf11db5`, blob `bfc719bb11c86f8851f9e5b0a1991a31f1156d6e` |
| Freeze / amendments | Proposal V4 is the exact consensus design; no Freeze delta or A-n applies to I-56 |
| Owner | **APPROVED — PROPOSAL V4**, durable decision commit `a6f5b3daf2ef8024061c128db755d8a9464faf5f` |
| ADR | ADR-0045 **ACCEPTED**; materialization `5da6feef2b528aaa44ed5ce581f976ea63251609`, accepted blob `0a74817ae1d48bb8a7f7ada8dfd6abeb173878af`, acceptance record `010ffb3c22572914023e150effee44ff073f656f` |
| Historical dry-run | **PASS**, commit `2654646bf1743bb0d53106c090a90cb29a168b91` |
| Global conformance | **PASS**, report commit `f1b60391cf22fd3371c50e9cfba787812b13c14b`, reviewed materialized tip `10b27465ff3b247b8b8342d6029a575119f42a67` |
| Architect | **CONFORMING**, complete independent review on `10b27465ff3b247b8b8342d6029a575119f42a67` |
| Coordinator | **CONFORMING**, complete follow-up review on the same exact tip |

## Activation controls

| Control | Evidence |
|---|---|
| Normative marker | `afd1077cba90ace890ac253bbe34a234b6691540` |
| Exact marker trailer | `Workflow-V2-Normative: I-56` |
| Tag-protection ruleset | `23505961`; target `tag`; `refs/tags/integration/*`; active update and deletion protection; no bypass |
| Activation-pause START | `2026-09-17T17:47:01Z` |
| Workflow V2 effective SHA | `8a021fb67c16dfccd6afc18448ea7e6a71a32364` |

The effective SHA is the first normative merge accepted by `origin/main`. This evidence correction does
not redefine it.

## Original Candidate round

| Field | Evidence |
|---|---|
| Candidate base / parent | `afd1077cba90ace890ac253bbe34a234b6691540` |
| `FINAL_CANDIDATE_SHA` | `039f0446b4546f489a1b6f7206a06d1af4389fea` |
| Worktree / SDK | Clean; resolved SDK `8.0.423` |
| Core Full local | **7240 PASS / 0 FAIL / 0 SKIP** |
| UI Full local | **1568 PASS / 0 FAIL / 17 SKIP / 1585 total** |
| Debug UI build | **PASS**, 0 errors and 0 warnings |
| Debug Plugin build | **PASS**, 0 errors; 2 documented `MSB3277` warnings |
| Exact branch push CI | Run `35254836487`; `push`; branch `docs/initiative-workflow-v2`; exact Candidate SHA; four required jobs **success** |
| Architect conformance | **CONFORMING** on the exact Candidate SHA |
| Coordinator conformance | **CONFORMING** on the exact Candidate SHA |
| Owner Validation | **NOT APPLICABLE**: the complete change is documentation and process only; it changes no product behavior, drawing, AutoCAD command, block, catalog, persistence path or product DLL behavior |

The Candidate push CI remains the branch-CI authority for this round. Later coverage dispatches do not
replace it.

## Original closure and normative publication

| Field | Evidence |
|---|---|
| Original closure | `3b9b7b703868ce6e8453ee84891abeb34b68c9d4` |
| Closure local evidence | Core Full, UI Full, Debug UI and Debug Plugin **PASS** on the clean closure SHA |
| Closure push CI | Run `35267387810`; exact closure SHA; four required jobs **success** |
| Normative merge | `8a021fb67c16dfccd6afc18448ea7e6a71a32364` |
| Effective publication | GitHub accepted the `main` push at `2026-09-17T20:26:34Z` |
| PRE | The complete authoritative PRE snapshot and derived claim table are embedded in merge `8a021fb67c16dfccd6afc18448ea7e6a71a32364` |
| POST | The complete verified POST snapshot/table is reserved for the final annotated `integration/I-56` tag |

## Post-merge verification under V1

### V1 4.5.6

**PASS.** Post-merge run `35270828410` has `event=push`, `head_branch=main` and
`head_sha=8a021fb67c16dfccd6afc18448ea7e6a71a32364`. Core, UI tests, Debug UI and Plugin build all concluded
`success`.

Coverage artifact:

| Field | Value |
|---|---|
| Artifact ID | `10518004993` |
| Name | `rackcad-coverage-cobertura` |
| Digest | `sha256:6b3fef3d2d7590d9c052debdefac97003da6ebd7c0aa18094a44510bcaad1030` |
| Size | `584218` bytes |
| Created | `2026-09-17T20:28:43Z` |
| Exact association | Run `35270828410`, merge SHA `8a021fb67c16dfccd6afc18448ea7e6a71a32364` |

### V1 4.5.7

**PASS.** Dispatch `35271447976` requested and checked out Candidate
`039f0446b4546f489a1b6f7206a06d1af4389fea`. Its `measured-sha.txt` records the same `measured_sha`,
`event=workflow_dispatch`, `run_head_sha=8a021fb67c16dfccd6afc18448ea7e6a71a32364` and
`run_id=35271447976`. The workflow concluded `success`.

Coverage artifact:

| Field | Value |
|---|---|
| Artifact ID | `10519140764` |
| Name | `rackcad-coverage-cobertura` |
| Digest | `sha256:bf2e01cf363f8198efdacf93171d9ab87cba14544d0df7ddf926049c183ed3df` |
| Size | `584252` bytes |
| Created | `2026-09-17T20:34:11Z` |
| Exact association | Dispatch `35271447976`, measured Candidate `039f0446b4546f489a1b6f7206a06d1af4389fea` |

## Metrics

| Metric | Value |
|---|---|
| Original Candidate Core Full | 7240 passed; 0 failed; 0 skipped |
| Original Candidate UI Full | 1568 passed; 0 failed; 17 skipped; 1585 total |
| Original Candidate branch CI | 4/4 required jobs succeeded |
| Original closure branch CI | 4/4 required jobs succeeded |
| Post-merge CI | 4/4 required jobs succeeded |
| Candidate coverage dispatch | 4/4 required jobs succeeded |
| Active Owner-validation duration | UNKNOWN — Owner Validation was not applicable |
| Original local-suite and build durations | UNKNOWN — no durable measured values are available |

## Corrective materialization and final record

This file was omitted from the original closure and is being materialized through the required retained-
branch corrective round. It is factual evidence, introduces no policy, does not modify Proposal V4 or
ADR-0045, and does not redefine the effective merge.

Post-merge facts may be referenced here, but this file does not replace the final annotated tag. The
final `integration/I-56` record remains authoritative for the complete POST snapshot/table, corrective
merge identity, cleanup results, and durable activation-pause END.

```text
Integration tag: integration/I-56 = PENDING CORRECTIVE ROUND FINALIZATION
Activation pause: ACTIVE
Cleanup: NOT YET
```
