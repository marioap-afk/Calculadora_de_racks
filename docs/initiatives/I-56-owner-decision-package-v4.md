# I-56 G9 — Owner Decision Package for Workflow V2 Proposal V4

> **DECISION INTERFACE — NOT POLICY.** This package summarizes the 17 policy decisions reserved to the
> Owner by the exact consensus Proposal V4. It does not modify, activate or implement Workflow V2.
>
> Proposal V4: `94a4e446b76ff28d8a434252fb859c071cf11db5`
> Independent Architect agreement: `e9740119ad029483deb73b7f8dd3451a55768919`
> Historical dry-run PASS: `2654646bf1743bb0d53106c090a90cb29a168b91`
>
> Coordinator = AGREED
> Architect = AGREED
> Consensus V4 = REACHED
> Owner decision = NOT YET RECORDED
> Workflow V2 = NOT EFFECTIVE
> WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST

## How to use this package

Select one option for each `OWN-*` item and then select the overall Proposal V4 response at the end.
Options labeled **REJECT / REQUEST CHANGE** do not select an alternative policy: they return the package
for revision. Notes may clarify a selected option, but a condition that changes Proposal V4 semantics is
a request for changes.

> ## PARTIAL APPROVAL DOES NOT ACTIVATE WORKFLOW V2
>
> If the Owner rejects, modifies or conditions any decision so that Proposal V4 semantics change:
>
> 1. record the Owner's decisions;
> 2. prepare Proposal V5;
> 3. obtain Coordinator + Architect agreement on the exact V5;
> 4. assess the dry-run impact;
> 5. return the new exact version to the Owner.
>
> No subset of V4 becomes effective independently.

## OWN-A — Three initiative archetypes

### Decision

Approve three observable archetypes that change the depth of Discovery and pre-Freeze design review:
**Extension**, **Foundation Evolution** and **New Architecture**.

### Why this is Owner-level

This sets the governance applied to future initiatives. It determines how much design work is required
before implementation while preserving one safety baseline for all work.

### Options

A. Approve the three archetypes and their limited effect.

B. Do not adopt the archetype model; keep V1 in force while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** The dry-run classified I-53S, I-51 and I-48 without losing a material capture.

### If approved

Future normative materialization will define Extension as consuming accepted foundations without a
material trigger; Foundation Evolution as materially changing an existing shared contract; and New
Architecture as introducing a transverse authority, persistence model, framework or registry.

### If rejected

Proposal V5 and new consensus are required before any normative materialization.

### Safety / evidence consequences

Archetypes affect only Discovery scope, Proposal depth and design review before Freeze. They never reduce
Candidate evidence, CI, exact-SHA identity, final Architect + Coordinator conformance, Owner Validation,
coverage or post-merge checks. UNKNOWN materiality activates the higher path until resolved.

Exact Proposal V4 references: P-03, P-04, P-05, P-08; §6 OWN-A.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-B — Architect participation, anti-churn and conformance

### Decision

Approve proportional design review, controlled delta re-review and complete final conformance by the
Architect and Coordinator.

### Why this is Owner-level

This decides the governance boundary between reducing review churn and retaining independent technical
challenge before a change can become a Candidate.

### Options

A. Approve the V4 review and conformance model.

B. Do not adopt this review model; keep V1 review practice while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** The dry-run preserved every required late I-48 finding, including AR4–AR7.

### If approved

The first review receives full Discovery, sources, code access and the complete Proposal/Freeze. Later
reviews receive the complete current version plus an explicit delta and disposition of prior findings.
Only an open REQUIRED finding creates another round. Final conformance remains complete for every
archetype.

### If rejected

V5 must redefine the review lifecycle and repeat consensus and dry-run impact assessment.

### Safety / evidence consequences

The delta focuses review but never limits its scope. A material finding in an unchanged section remains
valid. Only the issuer may downgrade REQUIRED with recorded evidence. Final conformance compares the
implemented SHA with Freeze + applicable amendments.

Exact Proposal V4 references: P-08, P-09, P-19; §6 OWN-B.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-C — Core Full cadence inside a gate

### Decision

Approve running Core Full locally at each functional gate close instead of before every internal push.

### Why this is Owner-level

This changes a current repository-wide validation cadence and accepts where Windows-local regression
risk is detected during iteration.

### Options

A. Core Full local at gate close, Candidate and closure; focal/relevant tests and full CI on internal pushes.

B. Keep the V1 requirement for Core Full local before every internal push.

### Recommended option

**A — consensus V4.** V4 deliberately rejected the stronger idea of waiting until Candidate only.

### If approved

AGENTS will be updated in a later authorized normative gate. Core Full remains required on the clean gate
close SHA and on the final Candidate, with exact-SHA reuse only when all existing reuse conditions hold.

### If rejected

The V1 cadence remains; V5 is required because P-11 and the expected efficiency change would differ.

### Safety / evidence consequences

Full CI still runs on every push. A regression visible only to local Windows Core may be found at gate
close rather than an earlier internal push, increasing retrabalho inside that gate. It can never be
deferred beyond gate close or omitted from Candidate Full.

Exact Proposal V4 references: P-10, P-11, P-12; §6 OWN-C; §8 change 1.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-D — READY before FINAL_CANDIDATE_SHA

### Decision

Approve the READY threshold before any SHA is declared `FINAL_CANDIDATE_SHA`.

### Why this is Owner-level

This defines when a deliverable may be presented for final evidence and Owner Validation.

### Options

A. Require READY-01..09 before declaring the final Candidate.

B. Do not adopt READY; retain the V1 Candidate regime while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** In the dry-run it would have prevented I-48's CI-red SHA from being named final.

### If approved

Future lifecycle rules will require closed gates, versioned decisions, final rebase/preflight, exact push
CI, complete conformance, a clean tree, complete OV assignment and Freeze integrity before fixing the
final SHA.

### If rejected

V5 must provide another threshold without weakening mandatory final evidence.

### Safety / evidence consequences

Intermediate Owner milestones are allowed only as intermediate Candidates with full V1 evidence for
their scope. They never replace the final Candidate or final OV. Any new SHA invalidates prior exact-SHA
evidence as today.

Exact Proposal V4 references: P-12, P-14, P-19; §6 OWN-D.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-E — T8 grandfathering for I-49, I-52 and I-55

### Decision

Choose how grandfathering applies to new delivery units in the three named initiative lines.

### Why this is Owner-level

Both readings are compatible with the original transition sentence but have different effects on new
claims. Only the Owner may choose the intended scope of that exception.

### Options

A. **T8-A:** grandfather only the existing formal claims/branches/contracts. A new later unit is V2,
with its own delta, evidence and Git unit.

B. **T8-B:** keep each named initiative line under V1 until its explicitly defined closure. A new unit
executing that line's existing scope stays V1 until that closure; later claims use the general V2 rule.

### Recommended option

**NO RECOMMENDATION.** Proposal V4 intentionally gives neither option preference or default value.

### If approved

The selected interpretation and, for T8-B, each line's exact scope and durable closure act will be
recorded before activation. Existing contracts are not rewritten and evidence never transfers between
units.

### If rejected

A different interpretation requires V5, new consensus and a new Owner decision. No T8 unit may proceed
while this remains unresolved.

### Safety / evidence consequences

Neither option permits retroactive reclassification by rebase or inheritance of evidence. T8-A moves
new units to V2 sooner; T8-B reduces transition mixing inside a named line but creates a bounded explicit
V1 exception. The line cannot be extended indefinitely to obtain V1.

Exact Proposal V4 references: P-02 “Version de workflow por reclamo propio”; P-25 T8 and “OWN-E ofrece
ambas opciones”; §6 OWN-E.

### Owner selection

- [ ] A — T8-A
- [ ] B — T8-B
- [ ] REJECT / REQUEST CHANGE

Owner notes, including line scope/closure if B:
________________

## OWN-F — Precise references instead of copied policy

### Decision

Approve prompts that cite every applicable normative rule precisely instead of copying large policy
blocks.

### Why this is Owner-level

This changes how operational orders communicate repository policy and assigns authority to versioned
sources rather than prompt copies.

### Options

A. Approve precise, complete references and the required gate-order fields.

B. Do not adopt this reference model; retain V1 order practice while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** It reduces contradictory copies while naming every rule a gate exercises.

### If approved

Orders will cite route + section + clause and state objective, scope, Freeze invariants, hotspots,
evidence, no-touch boundaries, uncovered stop conditions and expected report. Subordinate templates will
not create policy.

### If rejected

V5 must define another reliable order format and resolve authority duplication.

### Safety / evidence consequences

“Reference” never means “stay silent.” Missing or contradictory authority causes STOP. Undocumented
recurring safety rules must first be written in their normative owner before prompts may merely cite them.

Exact Proposal V4 references: P-16, P-18, P-24; §6 OWN-F.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-G — Remove gate-close ceremony and use conditional Discovery

### Decision

Approve removing mandatory `-CLOSE` commits per gate and expanding Discovery only when a defined trigger
requires it.

### Why this is Owner-level

This changes recurring workflow ceremony while preserving the controls that historically found defects.

### Options

A. Approve Discovery Core + triggered expansions and no separate ceremonial `-CLOSE` commit per gate.

B. Keep full expanded Discovery and/or separate `-CLOSE` commits as mandatory ceremony.

### Recommended option

**A — consensus V4.** The historical dry-run preserved all material captures.

### If approved

Every initiative still completes DC-1..DC-9. EXP-01..09 open only from recorded triggers. A gate may have
internal commits, but its clean close SHA must be pushed, tested and reviewed without an extra close-only
commit.

### If rejected

V5 is required to restore or redesign the ceremony and its documentation cadence.

### Safety / evidence consequences

Focal RED→GREEN, expected-test count, relevant tests, gate-close Core Full, exact push CI read before the
next gate and Coordinator review all remain. OWN-G does not re-vote Core cadence (OWN-C) or Architect
participation (OWN-B).

Exact Proposal V4 references: P-05, P-10, P-15; §6 OWN-G.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-H — Integration recovery, durable tags and tag protection

### Decision

Approve the integration recovery path, durable annotated `integration/*` records and authorization to
create and verify GitHub protection for that tag namespace before the normative merge.

### Why this is Owner-level

This changes repository integration governance and authorizes an external GitHub administrative action.

### Options

A. Approve P-20/P-25, including ruleset creation and verification before the normative merge.

B. Do not adopt the tag/ruleset mechanism; keep V1 integration records while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** It closes historical `MERGE_SHA = PENDING` gaps without commits directly on main.

### If approved

Integration will re-fetch before merge. If main moved after closure, route R preserves the old round,
rebases the product without its close commit, produces a new exact Candidate and evidence, recreates the
close and resumes. After verified post-merge CI, coverage and cleanup, an annotated `integration/<unit>`
tag records the complete verified round. The Owner authorizes creation/verification of protection against
tag update/deletion before I-56's normative merge.

### If rejected

V5 must define a different durable record/protection design; no partial activation may omit this piece.

### Safety / evidence consequences

Tags do not replace CI, coverage, cleanup or evidence files. A CI run triggered by `refs/tags/*` is not
branch, Candidate or post-merge evidence. Published tags are never moved or force-updated; corrections
append a complete numbered record.

Exact Proposal V4 references: P-12 route R, P-20, P-25 “Pausa y tag”; §6 OWN-H.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-J — Functional grouping and delivery units

### Decision

Approve grouping IDs only when they share both the same user problem and a material design authority or
foundation, while retaining separate Git delivery units where execution boundaries differ.

### Why this is Owner-level

This controls the size and identity of future initiatives and when shared foundation work may ship alone.

### Options

A. Approve the conjunctive grouping rule and delivery-unit model.

B. Do not adopt this grouping model; keep V1 unit handling while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** The I-53S dry-run removed foundation-only ceremony while keeping unit evidence.

### If approved

Shared infrastructure or hot files alone will not group unrelated user goals. Units split for observable
results, rollback, subsystems or forced sequencing. A foundation is folded into its first visible
consumer by default unless a P-10 division criterion requires earlier independent integration.

### If rejected

V5 must redefine grouping and how foundation-only deliveries avoid unverifiable integration.

### Safety / evidence consequences

Every delivery unit retains its own claim, branch, Candidate, evidence, merge, post-merge CI and cleanup.
No unit inherits evidence from a conceptual sibling.

Exact Proposal V4 references: P-02, P-10; §6 OWN-J.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-K — Documentation surfaces and descriptive FOUNDATIONS registry

### Decision

Approve the three-surface documentation model and choose the location of the descriptive FOUNDATIONS
registry.

### Why this is Owner-level

This changes repository-wide documentation authority, where evidence lives and whether a shared
foundation index exists. The location is a durable project-organization choice.

### Options

A. Approve contract + immutable Freeze + evidence file, with `docs/FOUNDATIONS.md` as the descriptive registry.

B. Approve the same model with `docs/architecture/foundations.md` as the registry location.

### Recommended option

**A — consensus V4 recommendation.** Repository-level normative documents already live at `docs/` root.

### If approved

Contracts hold mutable status/links, Freeze holds immutable design + OV assignments, and evidence files
hold execution facts. Post-merge facts live in the annotated integration tag. FOUNDATIONS will contain
brief verified metadata and links, subordinate to code facts and accepted ADR/Freeze authority.

### If rejected

Rejecting the surface model or registry requires V5. Choosing B is already an explicit V4 location
alternative and does not by itself change the model.

### Safety / evidence consequences

Coordinator + Architect, not the Owner, build and conform each entry against source, code and tests.
The Owner approves existence, location, purpose and subordinate status; the Owner does **not** approve
symbols, DTOs, tests or factual contents entry by entry. A consumed contradiction is EXP-01 class A and
STOP.

Exact Proposal V4 references: P-06, P-15, P-24, U-02; §6 OWN-K.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-L — One authority for each policy domain

### Decision

Approve the authority-by-domain model and its conflict rules.

### Why this is Owner-level

This changes which repository document wins when process, evidence, architecture or initiative-scope
instructions conflict.

### Options

A. Approve the V4 authority map and fail-closed conflict handling.

B. Do not adopt this map; retain V1 precedence while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** It prevents prompts and templates from silently becoming parallel policy.

### If approved

`WORKFLOW` owns Git, integration, documentation cadence and transition; `AGENTS` owns tests/evidence;
`INITIATIVE_LIFECYCLE` owns design/review; the validation guide owns manual procedure; Freeze + A-n own
initiative design; accepted ADRs own their explicit scope; recorded Owner decisions own their stated
scope. Templates remain subordinate.

### If rejected

V5 must resolve the affected precedence rules before materialization.

### Safety / evidence consequences

H.1 protections cannot be waived locally by Owner, Coordinator or Architect. A decision without written
scope is local, not precedent. Non-comparable conflicts STOP for Owner resolution; “more strict” applies
only when full containment is demonstrated.

Exact Proposal V4 references: P-17, P-24; §2 H.1; §6 OWN-L.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-M — Additive Owner Validation matrix

### Decision

Approve freezing an additive OV matrix, assigning every scenario to delivery units and running all
applicable scenarios on `FINAL_CANDIDATE_SHA`.

### Why this is Owner-level

This governs what the Owner will validate, how conceptual scenarios remain covered across units and who
may remove them.

### Options

A. Approve the additive matrix, frozen assignment and final-SHA execution rule.

B. Do not adopt the frozen matrix; retain the current validation guide while a revised proposal is prepared.

### Recommended option

**A — consensus V4.** The dry-run preserved all 14 I-53S Owner scenarios on the final SHA.

### If approved

Freeze/delta/A-n will record scenario, trigger, data, expected result, assigned units and any additional
intermediate milestone. READY checks complete assignment before Candidate.

### If rejected

V5 must redesign OV planning without eliminating Owner Validation.

### Safety / evidence consequences

The matrix adds to the existing guide; it never narrows it. Every applicable scenario runs on the final
Candidate. Intermediate validation is extra and requires full intermediate-Candidate evidence. The Owner
alone may remove/substitute a scenario or its last assignment.

Exact Proposal V4 references: P-09, P-12 READY-08, P-14; §6 OWN-M.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-N — Dedicated Workflow V2 ADR

### Decision

Decide whether Workflow V2 must have a dedicated accepted ADR before normative implementation begins.

### Why this is Owner-level

Only the Owner may accept or reject an ADR. Workflow V2 is a material repository-wide architecture and
governance decision.

### Options

A. Create the dedicated ADR during materialization and require Owner acceptance before normative gates.

B. Do not create/accept a dedicated Workflow V2 ADR.

### Recommended option

**A — consensus V4 recommendation**, consistent with the current WORKFLOW rule for decisions of substance.

### If approved

A later authorized docs-only gate will create the ADR and present it for explicit Owner acceptance before
implementing the remaining normative documents.

### If rejected

V5 is required to reconcile P-24 and the current ADR rule before implementation.

### Safety / evidence consequences

No ADR is accepted tacitly. This vote does not accept a file that does not yet exist; it selects the
required path, and the actual ADR still needs the Owner's explicit acceptance.

Exact Proposal V4 references: P-24; U-06; §6 OWN-N; §7 item 10.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-O — Authorization to edit AGENTS.md

### Decision

Authorize I-56's future normative materialization to edit `AGENTS.md` for the approved evidence and Core
cadence changes.

### Why this is Owner-level

The I-56 contract limits its current work to `docs/**` and explicitly requires a new order before editing
a file outside that boundary.

### Options

A. Authorize the later I-56 normative gate to edit `AGENTS.md` only as required by approved V4.

B. Do not authorize I-56 to edit `AGENTS.md`.

### Recommended option

**A — consensus V4 materialization requires it.**

### If approved

A later explicit gate may make the narrowly scoped `AGENTS.md` edits after all prior decisions and ADR
requirements are satisfied. This package itself makes no edit.

### If rejected

V5 is required because the approved cadence/evidence design cannot be materialized as proposed.

### Safety / evidence consequences

Authorization is limited to V4's normative materialization. It does not authorize product, tests, CI,
scripts or unrelated policy changes.

Exact Proposal V4 references: P-24 “Modificaciones existentes futuras”; §6 OWN-O;
I-56 contract §12.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-P — Pause new claims during activation

### Decision

Choose whether the activation window has a mandatory claim pause and define its exceptions and durable
end.

### Why this is Owner-level

The pause crosses initiative boundaries while I-56 is still governed by V1. Current V1 authority
requires an explicit Owner decision for that operational restriction.

### Options

A. **Mandatory pause:** stop all new claims from the recorded start before integration step 4.5.1 until
the durable end recorded after V1 steps 4.5.6/4.5.7; only explicitly recorded emergency/fix exceptions
may proceed, and prolonged blockage returns to the Owner.

B. **No mandatory global pause:** record `Claim pause: none`; any exceptional pause or emergency/fix
restriction still requires an explicit scoped Owner decision and durable record.

### Recommended option

**NO RECOMMENDATION between A and B.** V4 recommends that any pause end only after verified V1
4.5.6/4.5.7 and durable recording; it intentionally leaves obligation and exceptions to the Owner.

### If approved

The selection, start, allowed emergency/fix exceptions, prolonged-block path and durable end condition
will be recorded under V1 before 4.5.1. If A, the pause remains active until its end is durably recorded.

### If rejected

A different pause design requires V5. Activation cannot infer a pause or its end from branch state or an
ephemeral message.

### Safety / evidence consequences

A reduces races during the activation window but blocks ordinary claims. B avoids that global block but
requires classification by the transition evidence while integration proceeds. Neither option changes a
claim's workflow merely because it violates a pause.

Exact Proposal V4 references: P-25 “Pausa y tag de activacion”; §6 OWN-P.

### Owner selection

- [ ] A — mandatory pause
- [ ] B — no mandatory global pause
- [ ] REJECT / REQUEST CHANGE

Owner notes, including emergency/fix exceptions and prolonged-block instruction:
________________

## OWN-Q — Legacy claims with insufficient order evidence

### Decision

Approve fail-closed, evidence-based Owner resolution for legacy claims whose Claim-Id or temporal order
cannot be established consistently.

### Why this is Owner-level

Classifying an ambiguous claim as V1 or V2 changes its governing policy. Technical actors may collect
evidence but cannot choose the result by convenience.

### Options

A. Use T6: STOP, collect available Git/CI/decision evidence and ask the Owner to classify the specific
claim; if evidence remains insufficient, keep it stopped until an explicit decision.

B. Keep unresolved claims stopped and require V5 to define a general resolution policy before activation.

### Recommended option

**A — consensus V4.** It is the only encoded handling and never silently defaults to V1.

### If approved

Each unresolved legacy case will be decided with recorded scope and evidence. A confirmed later claim
uses T5/V2; a proven earlier claim is recorded V1 by stable identity.

### If rejected

V5 must define the alternative and test it against non-retroactivity and exact claim identity.

### Safety / evidence consequences

Missing data is UNKNOWN, not proof of an earlier claim. No foreign contract is rewritten; rebase and
current ancestry do not reclassify a stable claim.

Exact Proposal V4 references: P-25 temporal ladder, T5/T6 and OWN-Q; §6 OWN-Q.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## OWN-S — Future rollback or deactivation

### Decision

Approve the V4 boundary for future Workflow V2 deactivation: no automatic rollback, and any actual
deactivation policy must return through an explicit Owner-approved proposal.

### Why this is Owner-level

Deactivation changes repository-wide governance and may affect claims opened between activation and the
later rollback event.

### Options

A. Approve the V4 boundary: V2 remains effective until a future exact deactivation proposal is approved;
that proposal must define treatment of intervening claims, and no claim is retroactively reclassified
merely by changed ancestry.

B. Require a concrete rollback/deactivation policy in V5 before any activation.

### Recommended option

**A — consensus V4 boundary.** V4 intentionally authorizes no automatic rollback and preserves historical
effective identity.

### If approved

Normative materialization will state that rollback is never inferred from red CI, revert ancestry or
deletion of a tag. A future deactivation requires its own explicit Owner policy, technical consensus and
durable event; claims retain their recorded classification until that policy says otherwise.

### If rejected

V5 must add the requested concrete policy, repeat consensus and assess the dry-run impact before V2 can
be activated.

### Safety / evidence consequences

The historical `WORKFLOW_V2_EFFECTIVE_SHA` is never erased. There is no retroactive reclassification
unless the Owner later chooses an explicit compatible policy. This option does not pre-approve the
contents of that future policy.

Exact Proposal V4 references: P-25 “Verificacion roja / reversion”; P-26; §6 OWN-S.

### Owner selection

- [ ] A
- [ ] B
- [ ] REJECT / REQUEST CHANGE

Owner notes:
________________

## INFORMATIONAL — NO OWNER VOTE REQUIRED

These are preserved constraints or technical facts in consensus V4. They do not count among the 17
decisions and do not require a separate response.

- **Coverage policy: NO CHANGE.** Same population, triggers and cadence; coverage remains `[KEEP]`.
- **Full Candidate validation is preserved:** Core Full + UI Full local, Debug UI + Plugin builds and
  exact push CI.
- **Owner Validation is preserved** whenever its current trigger applies; metadata remains monotonic.
- **Exact-SHA identity is preserved.** Same tree is not evidence identity.
- **No T0–T4** test tiers or renamed equivalent.
- **No R0–R4** risk model or renamed equivalent.
- **No Quick CI** or separate reduced lane.
- **No automated merges.** Integration remains manual and serialized.
- **Workflow V2 is not retroactive**, except only the explicit bounded T8-B interpretation if the Owner
  selects OWN-E option B.
- **FOUNDATIONS entries are technically verified metadata.** Coordinator + Architect conform them; they
  are not facts approved entry by entry by the Owner.
- **Tag-triggered CI is not evidence** for branch, Candidate or post-merge requirements.
- **No product, tests, CI or scripts are changed by I-56.** P-21 scripts remain future design only.

Exact Proposal V4 references: P-13, P-20, P-26, §2 H.1, §6 coverage information, §7.

## Consistency and response rules

Individual selections and the overall response must be mutually coherent. Choosing an option already
encoded by V4 can form part of exact-V4 approval. Adding a condition, hybrid or alternative that changes
its semantics means **REQUEST CHANGES**, even if the overall approval box was also marked.

Both A/B selections are encoded alternatives for OWN-E, OWN-K and OWN-P. For the other decisions, each
`If rejected` paragraph states whether B can coexist with exact-V4 approval; where it requires V5, the
overall response must be **REJECT / REQUEST CHANGES**. The general reject checkbox is for an alternative
or condition not represented by A/B.

Rejecting tags while approving lifecycle, omitting the required authority model, declining necessary
AGENTS authorization while selecting the Core cadence change, or leaving T8 unresolved cannot activate a
subset. The response is recorded, V5 is prepared and the full sequence at the top of this package runs.

## Owner response summary

| ID | Selected option | Approved? | Notes |
|---|---|---|---|
| OWN-A |  |  |  |
| OWN-B |  |  |  |
| OWN-C |  |  |  |
| OWN-D |  |  |  |
| OWN-E |  |  |  |
| OWN-F |  |  |  |
| OWN-G |  |  |  |
| OWN-H |  |  |  |
| OWN-J |  |  |  |
| OWN-K |  |  |  |
| OWN-L |  |  |  |
| OWN-M |  |  |  |
| OWN-N |  |  |  |
| OWN-O |  |  |  |
| OWN-P |  |  |  |
| OWN-Q |  |  |  |
| OWN-S |  |  |  |

### Overall Proposal V4

- [ ] **APPROVE exact V4**, subject to the selections above being exactly options already defined in V4.
- [ ] **REJECT / REQUEST CHANGES**.

Owner name: ____________________
Decision date: __________________
Overall notes:
________________

## Package consistency check

```text
Check mode = SAME-SESSION READ-ONLY
Decision count = 17
Decision IDs = OWN-A/B/C/D/E/F/G/H/J/K/L/M/N/O/P/Q/S
Retired decision IDs = ABSENT
Coverage vote = ABSENT; informational only
Proposal source = exact V4 @ 94a4e446b76ff28d8a434252fb859c071cf11db5
Architect agreement = exact review @ e9740119ad029483deb73b7f8dd3451a55768919
Dry-run = PASS @ 2654646bf1743bb0d53106c090a90cb29a168b91
Policy implemented = NO
Owner response recorded = NO
```

Read-only comparison with Proposal V4 and the dry-run verifies:

- every §6 Owner ID appears once as a decision heading;
- T8-A/T8-B preserve their exact scope and have no recommended default;
- OWN-P preserves V4's lack of preference on mandatory pause while retaining its durable-end recommendation;
- OWN-Q never defaults an ambiguous claim to V1;
- OWN-S authorizes no automatic rollback or retroactive reclassification;
- coverage and every H.1 protection remain outside the vote set;
- technical mechanics stay with Coordinator/Architect and future normative owners;
- partial approval cannot activate any subset;
- no normative file, product, tests, CI, script, ADR, registry, ruleset or tag is created here.

```text
OWNER PACKAGE = READY FOR OWNER
OWNER DECISION = NOT YET RECORDED
CONSENSUS V4 = PRESERVED
DRY-RUN = PASS
WORKFLOW V2 = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
```
