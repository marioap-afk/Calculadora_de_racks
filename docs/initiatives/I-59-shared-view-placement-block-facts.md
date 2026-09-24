# I-59 — Shared View Foundation Placement & Block Requirement Facts

Status: D/F0 — F1 CLOSED / ACCEPTED; Consensus V3 reached; Freeze materialization pending

Workflow: V2

Archetype: FOUNDATION EVOLUTION

Branch: `architecture/shared-view-placement-block-facts`

BASE_SHA: `c75e7434a909d396c05e55c39a140ba53be9e98c`

CLAIM_SHA: `ec6ccc459532c71e0ed85e0b410f6a0c6ab7f25f`

Claim-Id: `9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a`

Trigger: I-55 G14 / ID19 Multi-Rack Projection.

## Goal

Evolve the integrated Shared View Foundation so existing consumers can obtain the neutral facts already required by I-55 ID19 without reconstructing a second authority locally.

The unit closes only after AUTH-08 and AUTH-12 deltas are integrated into `main`, verified post-merge, and exposed through an annotated `integration/I-59` receipt.

## Consumes / Extends / Introduces

Consumes:
- Shared View Foundation from I-57/I-58;
- View Identity and Rack Identity;
- Unknown-field Preservation;
- existing `Transform2D` authority;
- existing library requirement/query/import seams.

Extends:
- AUTH-08 placement/source-transform facts;
- AUTH-12 structural block requirements and availability facts.

Introduces:
- no product policy;
- no competing transform authority;
- no persisted authority;
- only additional neutral facts required by existing consumers.

## AUTH-08 delta

Freeze a neutral carrier/classification boundary capable of preserving the source-placement facts required by consumers, including:
- XYZ position when captured at the AutoCAD boundary;
- canonical rotation;
- per-axis scale facts and signs when semantically required;
- determinant/reflection;
- uniform/non-uniform and unit/non-unit classification;
- half-turn and negative-Z classification;
- normal and whether it is world +Z;
- tolerance inputs;
- origin facts only when a demonstrated consumer requires them.

`Transform2D` remains the existing 2D mathematical authority. AutoCAD extraction remains in Plugin; Application receives pure snapshots/facts.

Foundation does not decide Rigid, Orthographic, Relative Frame Window, accept/reject/remedy, anchor or placement policy.

## AUTH-12 delta

Freeze a neutral requirement carrier that can preserve, without collapsing:
- PieceId;
- ViewAddress or equivalent neutral view fact;
- Role = Required | OptionalVisual | NotApplicable;
- LibraryKey, including a blank key as an observable structural fact when Role = Required.

Freeze neutral availability sufficient to distinguish at least:
- Ok;
- FileMissing;
- Unknown;

and, if the existing boundary requires it, distinguish key absent, block absent and library unavailable.

Foundation supplies facts. Consumer policy remains outside Foundation.

## Hard limits

Do not:
- modify I-55 product code or implement ID19;
- implement CommonTransform2D, Rigid or Orthographic policy;
- modify RACKMIRROR policy or AUTH-15;
- change geometry, BOM, builders, authored authority, Resolve, Prepare or naming;
- change persisted DTO/schema;
- reopen I-57 or I-58.

Target:
- Schema diff = NONE
- Persistence migration = NONE

If persisted schema appears necessary, STOP and escalate.

## Existing-consumer rule

Inspect current I-55 G8/G9/G12, I-52, Shared View Foundation tests and every consumer discovered on current `main`. Prefer compatible additive evolution where it avoids needless breakage, but do not preserve an inadequate contract merely to avoid a diff.

## Design and authorization

Architect review is required before implementation and final Architect conformance is required before Candidate.

Implementation authorization requires, on the same Freeze:
- Coordinator = AGREED
- Architect = AGREED

Until both exist:

`IMPLEMENTATION AUTHORIZATION = NO`

## Functional gates

- D/F0 — claim, bootstrap, focused Discovery, proposed Freeze and Architect package.
- F1 — characterization / RED for AUTH-08 and AUTH-12.
- F2 — neutral model + extraction/classification.
- F3 — consumer-compatible Foundation integration.
- F4 — cross-consumer conformance + I-55 readiness proof.
- READY — Candidate under Workflow V2.
- Integration — merge, post-merge verification, receipt/tag and cleanup.

Do not create micro-gates for individual classes.

## Consumer unlock

I-55 G14 may be declared UNBLOCKED by this unit only after I-59 is integrated, post-merge CI is verified and `integration/I-59` is a valid annotated receipt reachable from `origin/main`.

I-55 then reconciles against new main and resumes G14 without cherry-pick.

## Review artifacts

- Complete Proposal V3 / Freeze DRAFT: [I-59-proposal-v3.md](I-59-proposal-v3.md)
- Architect review package V3: [I-59-architect-review-package-v3.md](I-59-architect-review-package-v3.md)
- F1 evidence: [I-59-f1-characterization.md](../automation/evidence/I-59-f1-characterization.md)

```text
F1 RED = ESTABLISHED
F1 CLOSURE = ACCEPTED
Proposal V3 = AGREED / FREEZE MATERIALIZATION PENDING
AR59-V2-24 = RESOLVED
AR59-V2-25 = RESOLVED
Freeze = DRAFT / MATERIALIZATION PENDING
Architect = AGREED
Coordinator = AGREED
F2 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO
```
