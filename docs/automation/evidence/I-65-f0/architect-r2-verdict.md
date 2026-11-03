# I-65 — Veredicto del Architect, ronda 2 (custodia)

- Revisión de la Proposal V2 sobre `8f9ac48ce135ef7cff08feb25f0ec19cc4c6aa7a`; mismo revisor que la ronda 1.
- Participante: subagente de la sesión Coordinator, contexto independiente, modelo `claude-opus-5-5`; último mensaje 2026-10-10T21:52:37.143Z UTC.
- Texto íntegro del veredicto final, extraído de su transcripción local sin edición. SHA-256 del cuerpo (UTF-8): `db76253436c5fdb61b4b504a32d8bafe9f8c7a6b4b0f8c44cc716e9799ebd24d`.

---

```text
ARCHITECT_VERDICT: CHANGES REQUIRED
REVIEW_MODE: SAME-SESSION ROLE. The same Architect subagent as round 1, with its own context, not the
             author of the documents. Reviewer and author are different roles in one session, not
             different people.
REVIEWED_COMMIT: 8f9ac48ce135ef7cff08feb25f0ec19cc4c6aa7a
```

A65-V1-01 is correctly blocked until P1 runs. V2 closes most of round 1. Five REQUIRED items remain, and all have small fixes: A65-V1-12 (residual), A65-V2-01, A65-V2-02, A65-V2-03 and A65-V2-04. A65-V2-01 has to be fixed before the Owner authorises P1, because P1 is a single licensed run.

## PRIOR_FINDINGS

- **A65-V1-01 → OPEN — BLOCKED BY P1.** This is correct by design. §0 makes FP-01 and FP-02 hard preconditions, the STOP rule replaces "exclude the magnitude", and every value P1 must supply is marked [P1].
- **A65-V1-02 → CLOSED.** In P1 v2, steps only register themselves. The gate runs before any write: it checks node and member counts and that `OUT_DIR` is a real, absolute, non-placeholder path, and it never calls `SystemExit`. S9 runs only after the gate passes. Hardening for re-runs is in A65-V2-01(d).
- **A65-V1-03 → CLOSED.**
  - The analysis steps now run first, on a stable model, with no coincident nodes.
  - Nodal moments and local loads are out of V1.
  - `__file__`, the working directory, decoders and the ASCII JSON literal are now measured.
  - The active database is detected indirectly through country-specific names.

  One residual orientation gap is covered by A65-V2-01(c) and A65-V2-02. It comes from my own imprecise wording in round 1: I called NegativeY arms "reversed", but a sloped NegativeY arm is not the reverse of a PositiveY arm.
- **A65-V1-04 → CLOSED.** IDL-01..10 now define every node class, the origin, the tolerances and the admitted families; SUP-02 is option A; REF-CANT-01 is defined; the golden values are derived by hand. Optional precision is in A65-V2-06.
- **A65-V1-05 → CLOSED.**
  - Each dimension has one owner.
  - lb-mass and gₙ get a single new owner, and `StructuralSectionUnits` stays bit-identical.
  - G4 is scoped to `StructuralAnalysis/**` with a closed list of literals.
  - Mass is out of V1.

  At the base commit, the only existing copy of 0.45359237 in `src` is in `StructuralSectionUnits`. A repo-wide guard is recommended in A65-V2-07.
- **A65-V1-06 → CLOSED.** Exact equality is required only where the arithmetic is exact, with a relative tolerance of ≤ 1e-12 elsewhere. With the LB_IN pair, nothing is converted on the way to RAM.
- **A65-V1-07 → CLOSED.** Storage is now inches and lbf, ADR-0050 has a compatibility clause, and Discovery §13 evaluates M-08.
- **A65-V1-08 → CLOSED.**
  - The property source is decided by Q-O2 and declared as ASM-PROP-01.
  - Under option (b) RackCad stores no material values, so there is no second authority.
  - §4 has gating rows for material values and exact rows for member defaults.

  This closure depends on Q-O2 being recorded (FP-03). The setters that template rule 7 relies on are in A65-V2-01(a).
- **A65-V1-09 → CLOSED.** G1 now has a scope and case-sensitive tokens, and `CreateRamBuildPackage` and the dump reader live in the provider folder. Optional scope fix in A65-V2-05.
- **A65-V1-10 → CLOSED.** D-10b defines the load channel: an explicit unit per load and a defined position basis. The file is part of the input fingerprint, and INV-15 covers it.
- **A65-V1-11 → CLOSED.** Every row now has a RED except INV-14 (optional, A65-V2-08). The fingerprint-sensitivity, STALE, atomic-writer, Plugin-guard and template-guard obligations are all present.
- **A65-V1-12 → OPEN.**
  - **Residual:** the capability gate (`__version__` plus bridge functions) is protected only by a static guard (INV-10). Its behaviour inside RAM has no OV, and a static guard is not behavioural evidence (AGENTS.md, "Guardia de fuente y prueba de comportamiento").
  - **Fix:** add a tool-generated fault-injection package whose expected `__version__` is altered, or which requires a bridge function that doesn't exist. Expect the script to refuse before any write, with nodes = members = 0 afterwards. It can be a new OV row or part of OV-I65-05.

  Everything else in this finding is closed: OV-05/06 cover the failure paths, INV-13 makes the comparator refuse FAILED readbacks, and the version gate is now defined in a measurable way.
- **A65-V1-13 → CLOSED.** OV is repeated on `FINAL_CANDIDATE_SHA`, F4 has its own milestone Candidate, and OV-02 no longer has behaviour rows.
- **A65-V1-14 to A65-V1-19 (OPTIONAL) → CLOSED.** All incorporated.

## FINDINGS

**A65-V2-01 | REQUIRED | TARGET: `ram-probe-p1.py` v2, D-09 rule 7, script readback tolerances in D-08/D-09**
- **ISSUE:** P1 v2 still leaves four gaps.
  - **(a) Setters are not measured.** Template rule 7 sets offsets (0,0,0), stiffness factors 1, axial NORMAL and spring 0 on every member. P1 only exercises the `cardinal_point` setter (step R1); the others are only read. Discovery already found documented API calls that are missing from the bridge (C16).
  - **(b) Readback precision is not measured.** Every probe value is an exactly representable round number (100.0, 4.375, −1000.0, 37.5, −1234.5). Nothing shows whether RAM stores values in another internal unit and rounds them when read back in LB_IN. Yet V3 must freeze the script's self-check tolerances for coordinates and load values; only the 0.1° for axes is stated. A tolerance that is too tight fails every run at F4; one that is too loose is a blind oracle.
  - **(c) One arm direction is missing.** In RAM coordinates, a sloped NegativeY arm runs along (0, +rise, +run). A1 measures only (0, +rise, −run) and its reverse (0, −rise, +run). This only matters if V1 keeps double-face stations (A65-V2-02).
  - **(d) Re-runs give misleading results.** If `OUT_DIR` holds files from an earlier run, the exclusive-create test in E2 fails with `FileExistsError` and records a false "exclusive write not allowed", and `save_as` may overwrite an older `probe-model.retx`.
- **WHY:** Rule 7 and the script's self-check would be frozen on API behaviour nobody has observed (§5: not verifiable; mandate: "Do not guess"). P1 is meant to be the one definitive licensed run.
- **FIX:**
  - **(a)** Exercise every setter in rule 7 and read each one back, or change rule 7 to read the defaults, fail if any differs, and set only `cardinal_point`.
  - **(b)** Add non-round values (for example a node at 123.456789012 in, a load of −1234.56789 lbf at 37.123456 %, a rotation of 12.3456°) and read them back in LB_IN and in one metric pair. V3 then freezes the script's tolerances from these results.
  - **(c)** Add the direction (0, 4.375, +100) at rotations 0 and 90, unless V1 drops double-face stations.
  - **(d)** Have the gate abort if `write-test.txt`, `p1-report.json` or `probe-model.retx` already exist in `OUT_DIR`.
  - Optional, not blocking: read the self-weight vector after `enable_self_weight(True)` on a dedicated case, then disable it. Discovery RC-03/RC-08 lists this, but V1 has no self-weight.

**A65-V2-02 | REQUIRED | TARGET: D-05 (IDL-04/08/10), REF-CANT-01, INV-06, §5 OV**
- **ISSUE:** V1 admits double-face stations:
  - node ids include `<lado>`;
  - IDL-08 handles merging and minimum separation between the two sides;
  - D-10b targets an arm "por nivel y lado";
  - IDL-10 does not exclude them.

  But the only golden test (INV-06) and every OV use REF-CANT-01, which is single-face PositiveY. The NegativeY path has no golden, no OV and no P1 orientation coverage: arms mounted on the far face, their frames, the arm–column node on that side, and the merge or `Invalid` rules between sides.
- **WHY:** §5: an invariant with no test obligation.
- **FIX:** Either:
  - declare V1 single-face only, with NegativeY or double-face stations → `Unsupported`, added to IDL-10 and OV-I65-03 (A65-V2-01(c) then goes away); or
  - add a hand-derived double-face golden (REF-CANT-02) with a RED for the minimum-separation `Invalid`, plus P1 coverage.

**A65-V2-03 | REQUIRED | TARGET: D-11 (dump of the manual model, comparator), D-03 units boundary**
- **ISSUE:** The read-only dump of the manual reference has no unit contract.
  - The engineer's model can be in any of RAM's 12 unit pairs; for this Owner a metric pair is likely.
  - V2 doesn't say whether the dump calls `units.set`, which would change the reference model and contradict "read-only".
  - Nor does it say whether the dump records the active pair, or whether the reader rejects pairs other than LB_IN.
  - D-03 names no owner for converting tonf, kgf, kN, m or cm in the comparator.

  This is a second units boundary outside D-03, sitting in the equivalence oracle.
- **WHY:** Mandate UNITS ("one boundary; fail closed on unknown units"); §5: an oracle that cannot be determined.
- **FIX:**
  - The dump records `m.units.current` and the raw values, and never calls `units.set`.
  - In V1 the provider reader accepts only LB_IN and returns `Invalid("unit pair")` for any other pair. The procedure tells the engineer to switch the manual model's display units to lb-in in RAM's UI before dumping; that switch is safe only if P1-U1 shows it preserves physical values.
  - Alternatively, define conversions for all 12 pairs through the D-03 owners.
  - RED: a dump in KN_M is `Invalid`, or is converted correctly.

**A65-V2-04 | REQUIRED | TARGET: §0 FP-03, D-14, §6 F6**
- **ISSUE:** F6 stays conditional ("only if Q-O5 includes it"), but FP-03 lists only Q-O1, Q-O2, Q-O3 and Q-O7. A Freeze that contains a conditional gate does not freeze its scope (§6, READY-01).
- **WHY:** §5: an inconsistent gate plan.
- **FIX:** Add Q-O5 to FP-03, or freeze "F6 out of V1" so that it can only be added later by an Owner decision plus an A-n.

**A65-V2-05 | OPTIONAL | TARGET: D-01 G1**
- **ISSUE:** G1's scope is only Domain and Application, so the allowlist entry for the Plugin command file means nothing, and other Plugin or UI files could still use RAM types.
- **FIX:** Bring `src/RackCad.Plugin/**` and `src/RackCad.UI/**` into scope, keeping the single-file allowlist.

**A65-V2-06 | OPTIONAL | TARGET: INV-06, REF-CANT-01**
- **ISSUE:** The inputs to the hand-derived golden aren't stated.
- **FIX:**
  - The golden takes REF-CANT-01's resolved placements from the existing resolver as fixture inputs: arm origins, axes, lengths, `ColumnStartZ` and column length. The resolver is a separately tested authority.
  - It hand-derives the analytical nodes from those inputs and writes them as literals.
  - Add: an arm–column node outside the range from column foot to column top → `Invalid`.
  - Name REF-CANT-01's base section: the design needs one even though the base is not modelled.

**A65-V2-07 | OPTIONAL | TARGET: ADR-0050 Decision 3, INV-16**
- **ISSUE and FIX:**
  - ADR-0050 should say explicitly that `StructuralSectionUnits` (ADR-0021 §10) consumes the new lb owner with bit-identical results, and list ADR-0021 and ADR-0040 under References.
  - INV-16's RED depends on G4, which only covers `StructuralAnalysis/**`. A repo-wide guard for `0.45359237` and `9.80665` outside the owner file, like `LengthUnitsAuthorityGuardTests`, would give it a real RED.

**A65-V2-08 | OPTIONAL | TARGET: §3 INV-14, §4 property row under option (b)**
- **FIX:**
  - Give INV-14 a RED, for example a Core test that references `ramelements` or a Python interpreter.
  - Under option (b), record the numeric property deltas in the OV evidence, and require the Owner to acknowledge any delta above ±0.5 %.

**A65-V2-09 | OPTIONAL | TARGET: D-11, manual-reference instructions**
- **ISSUE:** `Node` has no getter for nodal forces (`node.py` only has `add_force`). If the engineer enters loads as nodal forces, the dump cannot see them and the result is a false FAIL.
- **FIX:** Tell the engineer to apply loads as member point loads.

## ANSWERS

- **AQ2-01: Yes, it closes A65-V1-07.**
  - Inches are ADR-0005's own unit.
  - lbf introduces a canonical unit only for force, which had no prior authority, so neither M-01 nor M-08 is triggered.
  - ADR-0050 Decision 3 states the scope.
  - Converting N, kN or kgf into lbf is a single rounding, covered by the 1e-12 tolerance.
- **AQ2-02: Yes, it closes A65-V1-05 without triggering M-08 on ADR-0021.** ADR-0021 §10 fixes the exact factor (0.45359237 / 0.3048) and the single conversion function, not where the constant is declared. ADR-0040 D3 already set the same precedent with its aliases. Conditions:
  - a bit-identity test for `PoundsPerFootToKilogramsPerMeter`;
  - no change to the public API or to ADR-0021's Decision text;
  - the change declared as Extends (§7, and A65-V2-07).

  It does not interact with `LengthUnitsAuthorityGuardTests`, which has no mass or force role names.
- **AQ2-03: Yes, for the probe.**
  - The only irreversible harm is adding entities to a model that holds user content. Requiring nodes = 0 and members = 0 rules out any model with geometry.
  - Load cases are additive and are recorded.
  - Everything is written only inside `OUT_DIR`.
  - Recording the File > New load-case baseline instead of gating on it is right, because that baseline is what P1 measures.

  Add the re-run check from A65-V2-01(d). For the product template, the D-08 rule that a model is empty must include the measured baseline, and it does.
- **AQ2-04: Yes, if the golden's inputs are the resolved placements (A65-V2-06), and for single-face only (A65-V2-02).**
  - For W12X26 (d = 12.2 in, so `Bounds.MaxY` = 6.1 in), the column axis is at x = 0, y = −6.1 in.
  - Column foot: z = `ColumnStartZ`. Column top: z = `ColumnStartZ` + the column cut length.
  - Arm–column node for level k: z = z_a,k − (y_a,k + 6.1)·tan θ, where (y_a,k, z_a,k) is the arm's frame origin and tan θ = (7/16)/12 from `CantileverDefaults.ArmSlopeRisePer12`.
  - Arm tip: the end of the resolved arm member.

  Every value is determinable by hand.
- **AQ2-05:**
  - **A65-V1-08:** yes. Option (b) is one declared source; the member-default rows are exact; materials are checked against the values the Owner declares. Option (a) is also fully specified.
  - **A65-V1-12:** partly. OV-05 and OV-06 remove the blind oracle for name rejection and readback mismatch. Whatever RAM does with an unknown name (P1-S1), raising or substituting, the readback name check catches it. The capability-gate path is still only statically guarded, so the finding stays open.
- **AQ2-06: Yes, five items, all small.**
  - A65-V1-12 residual;
  - A65-V2-01, which must be fixed before P1 runs;
  - A65-V2-02;
  - A65-V2-03;
  - A65-V2-04, which adds Q-O5 to the preconditions.

  Once these are fixed, P1 has run and the Owner has answered Q-O1, Q-O2, Q-O3, Q-O5 and Q-O7, I see no other blocker. V3 must then also freeze the D-04 mapping and rotation rule from A1, and the script's readback tolerances from A65-V2-01(b).

## MANDATORY_CHALLENGES

1. COVERED (optional A65-V2-05)
2. GAP(A65-V2-02)
3. GAP(A65-V2-03, A65-V2-01)
4. COVERED
5. COVERED
6. COVERED
7. GAP(A65-V1-01 blocked by P1, A65-V2-01, A65-V1-12)
8. COVERED
9. COVERED
10. COVERED
11. COVERED

## VERIFICATION LOG

All reads were from git objects and read-only files under the RAM install folder. Nothing was executed from that folder.

- **Delta 0f4bc884..8f9ac48c:** 9 files, all docs. The custody file is present (47 references to A65-V1).
- **P1 v2, read in full:**
  - The gate runs before any write and uses no `SystemExit`.
  - Step order is E1, E2, C5, N2, N1, then the remaining steps. N2's base and N1's foot are fixed, and no analysis runs after N1.
  - No coincident nodes (regions at x = 0 / 1000 / 2000 / 3000 / 4000 / 5000).
  - `LoadCaseCollection` is iterable and `LoadCase.id` exists.
  - `_module` is one of `_CoreWrapper`'s slots.
  - The setters for offsets, stiffness, axial behaviour and springs exist in the package, but only `cardinal_point` is exercised.
  - All values are exactly representable.
  - A1's directions do not include (0, +rise, +run).
- **RAM database names:** every name P1 v2 uses exists in the US database: `W 12X26`, `HSS_SQR 4X4X1_4`, `S 10X35`, A992 Gr50, A500 GrC rectangular, A36, A36 (weightless). `IPE 200` is in `Europe/IPE.sec`.
- **REF-CANT-01 sections:**
  - `HSS_SQR 4X4X1_4` (A 3.37, I 7.8/7.8, J 12.8) and `W 12X26` (A 7.65, I33 204, I22 17.3, J 0.3) match RackCad's catalog exactly.
  - RackCad's HSS-RECT family includes square tubes, which RAM keeps in `HSS_SQR.sec`; the mapping table must reflect that.
- **Analysis on an unsaved model:** the official examples 01 and 04 analyse unsaved new models. `ModelNotSavedError` only concerns `save()`.
- **Nodal forces:** `Node` has `add_force` only, with no getter.
- **Base code (bb0d5522):**
  - `RackEmbedDocument.Design` is a string, so a fingerprint over the persisted bytes is feasible.
  - `CantileverDefaults.ArmSlopeRisePer12` = 7/16.
  - The only existing copy of 0.45359237 in `src` is in `StructuralSectionUnits`.
- **Documents read in full:** ADR-0050 draft, Discovery §13, the decisions file (F0 section), the state file, and Proposal V2 §0–§13.
