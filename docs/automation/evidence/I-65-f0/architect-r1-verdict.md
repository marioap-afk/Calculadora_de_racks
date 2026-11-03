# I-65 — Veredicto del Architect, ronda 1 (custodia)

- Revisión de la Proposal V1 sobre `0f4bc8849ddc0e491231d841cf43b56badba1e98`.
- Participante: subagente de la sesión Coordinator, contexto independiente, modelo `claude-opus-5-5`; último mensaje 2026-10-10T21:35:35.848Z UTC.
- Texto íntegro del veredicto final del revisor, extraído de su transcripción local sin edición. SHA-256 del cuerpo (UTF-8): `8844b8603652be46e2f58d8808ef25ccc6b35f6ddeaf7086cf3a48bc857cc42d`.

---

```text
ARCHITECT_VERDICT: CHANGES REQUIRED
REVIEW_MODE: SAME-SESSION ROLE. Subagent of the Coordinator's desktop session, with its own context. It did not
             write the documents under review. Reviewer and author are different roles in the same session,
             not different people.
REVIEWED_COMMIT: 0f4bc8849ddc0e491231d841cf43b56badba1e98
```

Owner decisions Q-O1 to Q-O5 and Q-O7 are also still open. Q-O4 (authorising probe P1) has to come before any Freeze; see A65-V1-01.

## FINDINGS

**A65-V1-01 | REQUIRED | TARGET: §6 F0, D-03 (RAM unit pair), D-04, INV-07, D-08 (definition of an empty model), Q-O4**
- **ISSUE:** The Proposal would freeze guesses that only probe P1 can settle:
  - whether every magnitude follows the N-mm pair;
  - which axis is vertical in RAM;
  - RAM's local-axis and rotation rules;
  - what a new, empty RAM model contains;
  - how the script finds its own package folder.

  F0 lists P1 only as "con autorización". D-03's fallback (the adapter "excludes that magnitude until an A-n") does not work for length or force: excluding either one kills the slice. Discovery §5 itself says the Freeze cannot fix units or orientation without P1.
- **WHY:** §5: freezable elements that cannot be verified. §6: invariant→test obligations must be executable. Mandate: "Do not guess RAM capabilities. Verify them before freezing the adapter architecture."
- **FIX:**
  - Make this a hard precondition of the Freeze, and re-review that version: P1 has run, its report is versioned under `evidence/I-65-f0/`, and D-03, D-04, INV-07 and the empty-model rule in D-08 are rewritten with MEASURED values and their REDs.
  - If the Owner refuses Q-O4, the result is `BLOCKED — OWNER DECISION`, not a conditional Freeze.
  - Replace the "exclude the magnitude" fallback with STOP for every magnitude V1 needs: length, force, moment and point-load position.

**A65-V1-02 | REQUIRED | TARGET: `ram-probe-p1.py`, F0**
- **ISSUE:** The probe changes whatever model is open, and nothing in its code checks that the model is empty first.
  - It adds nodes, members and load cases, runs `analyze()` twice, and in step S9 calls `save_as()` on the whole open model into `OUT_DIR`.
  - Only a header comment tells the operator to do File > New first.
  - The API cannot delete anything: `nodes.delete` and `members.delete` raise `NotImplementedError` (checked in the installed package). A run on a user's model therefore cannot be undone in memory, and S9 rebinds it to the probe file.
  - The probe breaks the same empty-model rule that D-09 and Discovery §4 set for the product script.
- **WHY:** Mandate "SECURITY / FILE SAFETY" and CD-I65-G0-06. As designed, the F0 deliverable cannot be run safely (§5, "not executable").
- **FIX:**
  - Before the first change, abort with no writes and no save unless all of these hold: `len(m.nodes) == 0`, `len(m.members) == 0`, the number of load cases equals the File > New baseline, and `OUT_DIR` is an absolute, existing directory that is no longer the placeholder.
  - Run S9 only when all of those checks passed.

**A65-V1-03 | REQUIRED | TARGET: `ram-probe-p1.py`, D-03, D-04, D-09**
- **ISSUE:**
  - **(a) Analysis steps are contaminated.** Steps N1 and N2 call `m.analyze()` on the whole model. By then the model already holds unstable parts from earlier steps:
    - in A1, the base node is free;
    - in S1, five members span the same two free nodes;
    - in R1, a member has M22/M33 released at both ends and sits on a free node;
    - in U2, the nodes are free;
    - the U1 node and A1's `endX` node are both at (1000, 0, 0) mm.

    A singular or unstable stiffness matrix can void the only scale checks that do not depend on reading values back through the API.
  - **(b) Orientation coverage misses the slice's members.** Under D-04's mapping (`X_RAM = X`, `Y_RAM = Z`, `Z_RAM = −Y`), the base and arms run along RAM ±Z, sloped arms are inclined in RAM's Y-Z plane, and NegativeY arms run in reverse. Step A1 only measures vertical members and one horizontal member along RAM X. All five official example scripts build 2D frames in the X-Y plane with gravity along −Y.
  - **(c) Nodal moments and local loads are not measured.** D-02 includes nodal moments, but no step measures their units or sign, or member loads in LOCAL directions.
  - **(d) Script assumptions are not measured.** No step measures how a script run with "Run File…" finds its own folder (`__file__` or the working directory; the help page is silent). Nor does it measure whether a decoder for the "encoded" payload is available (`base64`/`binascii` are not on the import allowlist).
  - **(e) The active database is not recorded.** Name resolution depends on which country's section/material database the new model uses.
- **WHY:** §5. D-03, D-04 and D-09 would be frozen from an instrument that cannot produce the MEASURED values they need.
- **FIX:**
  - Run N1/N2 first on a fresh model, or make every auxiliary node fully fixed and avoid coincident or duplicate members.
  - Add members along RAM ±Z, inclined in RAM Y-Z, and reversed, at rotations 0 and 90, and read `get_local_axes()` on each. Alternative: change D-04 so the station plane maps onto RAM X-Y, and measure that.
  - Add an `add_force` with mx/my/mz and read back a LOCAL-direction point load.
  - Record `__file__`, `os.getcwd()`, decoder availability and the active database.
  - Anything V1 uses that P1 does not measure becomes Unsupported in V1.

**A65-V1-04 | REQUIRED | TARGET: D-05 (IDL-02/04/05), D-04, D-10 (SUP-01/02), LOD-01, INV-06, §4**
- **ISSUE:** Only nodes at axis intersections are defined. These rules are missing:
  - **Column foot elevation.** Floor (z = 0) or `ColumnStartZ`, the top of the bottom plate. `CantileverColumnBaseDatum` puts the column on its plate and the base on the floor, so the two differ.
  - **Column top and arm tip.** For the arm tip: the member end, or the end plate/stop.
  - **Column–base node.** Whether it exists when SUP-02 drops the base; alternative B is still open.
  - **Origin and axis mapping for the manual reference.** Station members are in station-local coordinates; the line adds `CantileverLineStationPlacement.Offset`.
  - **IDL-04 tolerance.** IDL-05 only gives a merge tolerance and an angular tolerance.
  - **Admitted section families.** A single C or L arm is centred on its bounding box, not its centroid (`Placements(Single)` uses `−Bounds.Center.X`), so its centroid sits off the station plane. L sections also have rotated principal axes.
  - **LOD-01 load position.** A fraction of the analytical member (which includes half the column, per IDL-03) or of the physical arm.

  Several of these differ by a plate thickness or half a column depth, far more than the 0.5 mm equivalence tolerance.
- **WHY:** §5: the INV-06 golden test and the §4 matrix have no determinable expected values. Mandate "MEMBER IDEALIZATION", "SUPPORTS" and "LOADS".
- **FIX:**
  - Write one explicit rule for each node class and for the load-position reference.
  - Choose SUP-02 option A or B, or give the Owner two complete rule sets under Q-O3.
  - Freeze the origin (station-local datum) and the global mapping the manual model must use.
  - State the IDL-04 tolerance.
  - Restrict V1 to doubly symmetric families (W, HSS-RECT, S); C and L become Unsupported with a diagnostic.
  - Name the reference station design.
  - Derive the INV-06 golden values by hand from these rules, not by running the builder.

**A65-V1-05 | REQUIRED | TARGET: D-03, guard G4, INV-02/03**
- **ISSUE:**
  - D-03 says the constant lb = 0.45359237 kg "does not exist" and will be defined once. It already exists: `StructuralSectionUnits.PoundsPerFootToKilogramsPerMeter = 0.45359237d / 0.3048d`.
  - Converting catalog data has no declared owner: area (in²), Ix/Iy/J (in⁴), Cw (in⁶). ADR-0021 §10 and `StructuralSectionUnits` are "the ONE place where a unit is converted in the neutral catalog", and they own 645.16.
  - G4 would flag `LengthUnits` and `StructuralSectionUnits` unless it is scoped. It must also coexist with `LengthUnitsAuthorityGuardTests`, which forbids `25.4` and role names such as `InchesToMillimeters` or `MillimetersPerInch` outside `LengthUnits`.
  - Mass and density have no consumer in V1: self-weight is excluded and RAM uses its own material database.
- **WHY:** M-01 (a second authority for the same value) on a freezable element. Discovery DC-09 says M-01 is not triggered only if constants are reused, and the text does not achieve that.
- **FIX:**
  - Add a table that names one owner per dimension:
    - Lengths: derived from `LengthUnits.MillimetersPerInch`.
    - Catalog area, inertia and warping: through `StructuralSectionUnits`, declared as Extends of ADR-0021 with the existing API bit-identical, or derived only from `LengthUnits`.
    - Pound-mass: drop it from V1, or give it a single shared owner.
    - lbf: derive it as lb × gₙ (gₙ = 9.80665) instead of adding a third literal.
  - Scope G4 to `src/RackCad.Application/StructuralAnalysis/**` and state how it relates to `LengthUnitsAuthorityGuardTests`.

**A65-V1-06 | REQUIRED | TARGET: D-03 ("conversión de ida y vuelta exacta"), INV-03**
- **ISSUE:** With SI values stored as doubles, exact round-trips cannot be achieved:
  - in→m→in was not bit-exact for 241 of 2000 sampled values (measured);
  - in→m→mm is not equal to in→mm: 0.1625 in gives 4.1275 one way and 4.1274999999999995 mm the other.

  A test written as stated will either fail or be weakened ad hoc.
- **WHY:** §5: a frozen test obligation that cannot be executed as written.
- **FIX:**
  - Require exact equality only where the arithmetic makes it exact, for example one multiplication by `MillimetersPerInch` on the path to RAM.
  - Use a frozen relative tolerance (for example ≤ 1e-12) for round-trips.
  - Keep the ×25.4, ×4.448… and ×1000 scale checks exact.

**A65-V1-07 | REQUIRED | TARGET: D-03, D-15, DC-09**
- **ISSUE:** D-03 makes SI the canonical storage of a RackCad Application model that holds geometry.
  - ADR-0005 D1 (accepted) says all geometry is expressed in inches and no other internal unit is introduced.
  - ADR-0040 D3 makes `LengthUnits` the single neutral authority for generic length conversion.
  - ADR-0021 §10 owns catalog conversions.

  DC-09 checks M-08 only against ADR-0017. The package has no ADR-0050 draft, and D-15 does not mention ADR-0005, 0021 or 0040.
- **WHY:** M-08 is at least UNKNOWN, which counts as triggered until resolved (§3). WORKFLOW §10: an accepted ADR can only be displaced by an ADR that declares the exception in its scope.
- **FIX:**
  - Record the M-08 evaluation in DC-09.
  - Put the ADR-0050 draft in the next package with an explicit compatibility clause: the analysis model is derived and not persisted; the inch stays canonical for product, DWG, catalogs and persistence; conversions use `LengthUnits` and `StructuralSectionUnits`. Alternatively, store lengths in inches.
  - The Owner accepts ADR-0050 before F1.

**A65-V1-08 | REQUIRED | TARGET: D-06, D-07, §4, OV-I65-01, D-09**
- **ISSUE:** RAM takes sections and materials by name and analyses with its own database values.
  - Checked in the installed files: `Steel.mat` stores E, ν ("U") and unit weight. For example, A36 has ν = 0.26, and "A500 GrC rectangular" has W = 0.28179 lb/in³. `W.sec` stores A, I33, I22, J and Cw.
  - D-07 creates a RackCad material profile (E, G or ν, weight) whose values RAM never receives. That gives two unchecked authorities for the analysed values.
  - Example: under D-07's rule C/L/S → A36, RAM's ν = 0.26 gives G ≈ 11,508 ksi, against the usual 11,200 ksi.
  - D-06 promises an OV coherence check "with a frozen tolerance", but gives no tolerance and no OV ID.
  - §4 compares sections and materials by name only, so it cannot see property drift.
  - These member facts that change behaviour are not compared: cardinal point, rigid offsets, stiffness factors, axial behaviour, rotational springs.
- **WHY:** M-01 plus a blind oracle (§5). Mandate challenge "provider file becoming RackCad authority".
- **FIX:** Choose one approach for property values:
  - **(a) RackCad is authoritative.** Keep the RackCad profile and catalog as the authority, and add frozen rows to §4 and OV-I65-01:
    - per section: A, Ix, Iy, J (and Cw where used) against RAM's entry, within about ±0.5 %;
    - per material: E and ν exact, unit weight within ±0.5 %;
    - the manifest lists the expected values.
  - **(b) RAM's database is the source.** Under Q-O2 the Owner declares RAM's database as the property source, recorded as an AnalysisAssumption, and the V1 profile becomes identity + mapping only.

  In both cases, add exact-equality rows for cardinal point = CENTROID, offsets = 0, stiffness factors = 1, axial NORMAL and springs = 0, and have the script set and read back `cardinal_point`.

**A65-V1-09 | REQUIRED | TARGET: D-01 (G1/G2), D-13, INV-01**
- **ISSUE:** G1 says no file outside the provider subfolder names RAM. That conflicts with D-13:
  - `CreateRamBuildPackage` is listed among the neutral Application capabilities;
  - the Plugin command has to call it;
  - the comparator tool reads RAM dumps.

  The detection rule is not specified, and a naive one is useless: 2,485 lines in Application and Domain contain "ram" in any case (Frame, Param, Program…), while none contain the word RAM.
- **WHY:** §5: the main guard for challenge 1 cannot be executed as written. It either fails on legitimate code or misses violations.
- **FIX:**
  - Freeze the scope: `Domain/**` and `Application/**` except `StructuralAnalysis/Providers/RamElements/**`, plus an exact-path allowlist for the one Plugin command file. Tools and tests are out of scope.
  - Freeze the token rule: case-sensitive `RamElements`, `ramelements`, "RAM Elements", and `\bRam[A-Z]`.
  - Move `CreateRamBuildPackage` into the provider folder.

**A65-V1-10 | REQUIRED | TARGET: D-10 (LOD-01), D-13, D-12, F5, OV-I65-02**
- **ISSUE:** "Values entered explicitly" has no input channel:
  - the D-13 command asks for no loads;
  - nothing is persisted;
  - the input unit and its validation are undefined;
  - loads are not among the listed inputs of the input fingerprint.

  F5 cannot be executed as specified, and the entry point is exactly where the lb / N / kgf confusion the mandate warns about would happen.
- **WHY:** §5. Mandate "fail closed on unknown units" and "no hidden load assumptions".
- **FIX:**
  - Freeze a channel, for example a versioned JSON load file chosen at export, where each load has: an explicit unit, an explicit position basis, a direction, and the node or member it applies to.
  - A missing or unknown unit is Invalid.
  - The file is part of the input fingerprint and is listed in the manifest.
  - Pure tests with a RED for lbf vs N vs kgf and for a missing unit.

**A65-V1-11 | REQUIRED | TARGET: §3 INV-04/05/07/08/09/10/11/12/14, D-08, D-09, D-12, D-13**
- **ISSUE:**
  - **No REDs.** Most rows describe a test idea but give no expected RED.
  - **INV-04 is a blind oracle.** It checks stability only, so a fingerprint that never changes would pass, which makes stale-result detection untested.
  - **INV-08 clashes with D-04.** It says supports and releases map "one to one", but under D-04's mapping neutral rotation restraints swap axes in RAM (neutral ry becomes RAM rz and vice versa). A naive index-for-index mapping would pass the test as written.
  - **Missing obligations:**
    - V1 staleness: the comparator must reject a readback whose input fingerprint differs from the package's;
    - atomic writing of the package (D-08);
    - the D-13 limits: no AutoCAD event hookups, no implied-selection writes, no WPF or PaletteSet, no menu edits;
    - the D-09 template rules: the import allowlist; no `save`, `save_as`, `new`, `analyze` or `os.*`; the readback file created exclusively; load categories passed as text; one `add_force` per node and case; at most 24 point loads per member and case.
- **WHY:** §5 and §6: invariants without a test obligation that has an expected RED.
- **FIX:**
  - Add a RED to every INV row.
  - Fingerprint sensitivity RED: changing geometry, section, material, an assumption, a mapping or the adapter version must change the fingerprint.
  - A STALE-readback RED in the comparator.
  - An atomic-writer RED: an injected failure leaves no folder.
  - A Plugin source guard for D-13.
  - A static guard over the template plus payload contract tests: two loads on one node become one summed entry; 25 point loads give Unsupported before anything is generated.

**A65-V1-12 | REQUIRED | TARGET: D-08, D-09, OV matrix**
- **ISSUE:**
  - The script's failure paths have no test (Python is excluded from CI by INV-14) and no OV:
    - a section or material name rejected mid-build, giving `FAILED — DISCARD MODEL`;
    - a readback mismatch;
    - the capability gate.

    OV-01 only exercises the success path and OV-04 only the non-empty model, so the script's self-check is a blind oracle.
  - "Version not supported" cannot be detected as written: `Model.version` raises `NotImplementedError` in the installed `model.py`. Only `ramelements.__version__` and the set of bridge functions are observable.
- **WHY:** §5: failure semantics without a test obligation, and a diagnostic that cannot be produced.
- **FIX:**
  - Add OV scenarios with fault-injection packages generated by the tool: an unknown RAM section name, and a payload whose readback cannot match. Both expect FAILED in the console and in the readback.
  - The comparator refuses FAILED readbacks (pure test).
  - Define the gate as the package version plus the required bridge functions measured in P1-E1. Record the RAM product version as declared, not verified.

**A65-V1-13 | REQUIRED | TARGET: §5 OV matrix, §6 F4/F5/READY, D-14**
- **ISSUE:**
  - OV-I65-01 to 04 are assigned only to F4 and F5. Lifecycle §8 requires every applicable scenario to run on `FINAL_CANDIDATE_SHA` (an intermediate milestone is extra), and an intermediate Owner milestone needs its own complete Candidate.
  - OV-I65-02 and the §4 row "Comportamiento (F5, si aplica)" depend on "if analysed", while analysis is out of V1 unless Q-O5 says otherwise. Their content cannot be verified.
- **WHY:** §5: an inconsistent gate and OV plan. READY-08.
- **FIX:**
  - State that OV-I65-01 to 04 are re-run on the final SHA, or get the Owner's approval for any reduction.
  - Define how the F4 milestone build is delivered, per the manual guide.
  - Fix OV-I65-02's content according to Q-O5: drop the behaviour rows if F6 is out, or specify who runs `analyze()` and how results are captured for both models.

**A65-V1-14 | OPTIONAL | TARGET: D-12, D-02 metadata**
- **ISSUE:** For Cantilever, the envelope Id and `Line.Id` are different values (FOUNDATIONS), and D-12 doesn't say which one "RackId" means. The canonical form of the authored design behind SourceFingerprint is also undefined; since I-11 the DTO keeps unknown fields.
- **FIX:** RackId = `RackEmbedDocument.Id` (ADR-0009). Define the canonical form, or mark SourceFingerprint as informational only.

**A65-V1-15 | OPTIONAL | TARGET: D-02 orientation, INV-07**
- **ISSUE:** The depth vector has to account for `PrismaticSectionInstance.RotationRadians` and `Mirrored`.
- **FIX:** Derive the depth vector from both, and add a RED with a non-zero rotation.

**A65-V1-16 | OPTIONAL | TARGET: IDL-05, D-11**
- **ISSUE:** A double station with different arms on each side can produce column nodes a few hundredths of an inch apart. That is more than the 0.001 in merge tolerance but less than the 0.5 mm matching tolerance.
- **FIX:** Define a minimum segment length or a diagnostic, and make the comparator fail on ambiguous matches.

**A65-V1-17 | OPTIONAL | TARGET: D-02**
- **ISSUE:** D-02 carries things V1 never uses: nodal moments, local-direction and distributed loads, Cw, mass. It also adds a load-translation extension point with no implementation beyond explicit input.
- **FIX:** Trim them, or mark each one Unsupported until it is exercised. Drop the empty extension point.

**A65-V1-18 | OPTIONAL | TARGET: D-09 determinism**
- **ISSUE:** "Byte-identical package" contradicts "no timestamps inside what is signed", which implies timestamps elsewhere.
- **FIX:** State that the package contains no timestamps at all.

**A65-V1-19 | OPTIONAL | TARGET: contract front matter**
- **FIX:** Set `requires_plugin_build`, `requires_autocad` and `requires_owner_validation` at the Freeze; D-13 and OV-I65-01 need them.

## ANSWERS

- **AQ-01:** Folder plus guards is enough for V1. The adapter uses only the base class library, so a separate assembly would fence nothing and would add a project to the solution, CI and bundle. Two conditions:
  - G1 gets a precise scope and token rule (A65-V1-09);
  - G2 also states that `Systems/*` never references `Providers/*`.

  Revisit a separate assembly only when a provider brings a binary dependency.
- **AQ-02:** Typed SI storage is defensible, since forces, moments and stresses have no RackCad unit, but not as written. It needs the ADR-0005/0040/0021 compatibility clause (A65-V1-07), the ownership table (A65-V1-05) and a defined round-trip oracle (A65-V1-06). Reusing `LengthUnits` alone is not enough, because pound-mass and in², in⁴, in⁶ still need owners. Storing lengths in inches and converting once to the RAM pair is the lower-risk option for lengths. Either choice is acceptable once recorded.
- **AQ-03:** Yes, for the transfer equivalence, because both models apply the same rule. Conditions:
  - it is a named AnalysisAssumption in the manifest;
  - the length basis for load positions is fixed (A65-V1-04);
  - no claim about the real rack's behaviour is drawn from F5.

  Rigid offsets later need their unit group measured by P1 (RC-01).
- **AQ-04:** No; see A65-V1-02 and A65-V1-03. Otherwise its side effects are contained:
  - it writes only to `OUT_DIR`;
  - if `OUT_DIR` is missing, `open()` and `listdir` fail before `save_as` runs;
  - it consumes one licensed session.

  All material and section names it uses exist in the installed US database.
- **AQ-05:** Compatible. A command-line command that prompts for a pick inside the command is not "selection/event architecture". ID30's guard on its unintegrated branch (`WorkspaceBridgeSourceGuardTests`) forbids AutoCAD event hookups outside `src/RackCad.Plugin/Workspace/`, and restricts `GetEntity`/`GetSelection` only inside that folder, so a standalone new command file passes. The Plugin has no `CommandClass` assembly attribute, so a new file is all that is needed. Conditions, enforced by the guard in A65-V1-11:
  - no event subscriptions;
  - no `SetImpliedSelection`;
  - no PaletteSet or WPF;
  - no menu edits;
  - a new file only;
  - no consumption of ID30 code.
- **AQ-06:** Yes. A gate covering only the neutral model would be a layer gate (§7), and a foundation-only unit merges with its first consumer (§2). F3 is acceptable as its own gate because the RAM package is a reviewable contract with its own observable output.
- **AQ-07:** Yes. The failure paths and the script's self-check have no obligation (A65-V1-12), nor do the template rules and payload contract (A65-V1-11) or the way the script finds its folder (A65-V1-03 d).
- **AQ-08:** The choice is well founded.
  - Against Selectivo: `secciones.csv` has empty `Ix`/`Iy` in every row, rack posts have no A/I/J/Cw, and there is no material anywhere. RAM's US database has no rack posts and the API cannot create sections, so a Selectivo frame would need made-up properties or a new authority for post sections.
  - For Cantilever: its members use AISC v16.0 ids, and their frames are placed at the tabulated centroid (ADR-0022 §2; `CantileverColumnBaseFrameResolver`). Column and base are W by product policy, and every member bends about its strong axis in the station's Y-Z plane.

  Still missing: the named reference station and the section-family restriction (A65-V1-04). Q-O1 still needs the Owner, because it departs from the candidate the mandate named first.
- **AQ-09:** ADR-0017 is handled correctly: it is consumed, INV-11 applies (it needs a RED), and M-08 is triggered only if ratios or verdicts appear in RackCad. F6 should explicitly exclude `member_design` and ratios.

  ADR-0050:
  - Its draft belongs in the Freeze package, including the ADR-0005/0021/0040 clause from A65-V1-07, so that the text reviewed is the text frozen.
  - The Owner must accept it before F1 (WORKFLOW §8), not necessarily before the Freeze.
  - Number 0050 is free: 0047 belongs to I-64, 0048 to I-62, 0049 is on main.
- **AQ-10:** It covers the mandate's error list except "RAM version unsupported", which cannot be detected (A65-V1-12).
  - RackCad side: there is no partial export only if the writer is atomic, which needs its RED (A65-V1-11).
  - RAM side: a partial model cannot be avoided, because nothing can be deleted and the section is assigned after the member is created. That is acceptable only with all of these:
    - a FAILED status in the readback;
    - the comparator refusing it;
    - OV coverage of the failure path;
    - the readback file created exclusively.

## MANDATORY_CHALLENGES

1. GAP(A65-V1-09)
2. GAP(A65-V1-04)
3. GAP(A65-V1-05, A65-V1-06, A65-V1-07, A65-V1-10)
4. COVERED
5. GAP(A65-V1-04, A65-V1-08, A65-V1-10)
6. GAP(A65-V1-08)
7. GAP(A65-V1-01, A65-V1-03, A65-V1-12)
8. COVERED
9. GAP(A65-V1-11)
10. COVERED (see A65-V1-17)
11. COVERED

## VERIFICATION LOG

All reads were from git objects at 0f4bc884 and bb0d5522, and read-only files under the RAM Elements install folder. Nothing was executed from that folder.

- **Branch content and mandate:** the branch adds only `docs/` (10 files). The mandate blob's SHA-256 is AE1A0412…6E56, 25,549 bytes, matching the evidence.
- **Cantilever datum and frames:** the column starts at `ColumnStartZ` (top of its bottom plate) and the base rests on the floor. Frame origins are at the tabulated centroid (ADR-0022 §2). Column, arm and base all bend about their strong axis in the station's Y-Z plane. A single arm is centred on its bounding box (`−Bounds.Center.X`). Each station is in local coordinates, and the line applies an Offset. A blocked station produces no geometry. Result: confirmed, and the source of A65-V1-04.
- **Cantilever section policies:** column and base are W only; arms admit any family; paired arms must be channels. Result: confirmed.
- **Unit conversions:**
  - `LengthUnits` converts only towards inches.
  - `StructuralSectionUnits` holds 645.16 and an inline 0.45359237 (contradicting D-03's "does not exist").
  - `LengthUnitsAuthorityGuardTests` forbids `25.4` and conversion role names outside `LengthUnits`.

  Result: confirmed; source of A65-V1-05.
- **Round-trip precision:** a PowerShell test of double arithmetic gave 241 of 2000 in→m→in round-trips non-exact, and in→m→mm ≠ in→mm. Result: source of A65-V1-06.
- **ADRs:** read ADR-0005, 0017, 0021 and ADR-0040 D3. ADR-0050 is not present; 0047 and 0048 are on parallel branches. Result: confirmed; source of A65-V1-07.
- **Selectivo data:** `secciones.csv` `Ix`/`Iy` are empty in all rows. The default SingleDiagonal bracing in the BOM model has no troquel setbacks; DoubleDiagonal does. Result: Discovery §6.2–6.3 is substantially confirmed.
- **RAM API (installed package):**
  - `members.add` sets the section after creating the member;
  - node and member delete raise `NotImplementedError`;
  - `add_force` replaces existing values;
  - point loads are limited to 24 and positions are 0–100 percentages;
  - `str(category)` is sent to the bridge;
  - `Model.version`, `open` and `close` raise `NotImplementedError`;
  - `new()` may open a save dialog;
  - the unit pair is a single force–length code from 0 to 11;
  - the default cardinal point is not documented.

  Result: Discovery §3.1 is confirmed. The version-gate gap is new (A65-V1-12).
- **RAM databases:** `Steel.mat` holds E, ν, unit weight, Fy and Fu, and includes "A36 (weightless)", A992 Gr50 and A500 GrC. `W.sec`/`HSS_RECT.sec` W10X49 rows match RackCad's catalog (A 14.4, Ix 272, Iy 93.4, J 1.39, Cw 2070) and carry an `IsmName`. Every name the probe uses exists. Result: confirmed; source of A65-V1-08.
- **RAM runtime and examples:** the embedded Python 3.13 has no `.pyd` modules. The five example scripts build X-Y frames with gravity along −Y. The help page documents "Open Script…" and "Run File…" but says nothing about the working directory. Result: source of A65-V1-03.
- **Probe P1:** read every step. There is no emptiness check before changes; it creates coincident and unrestrained parts before the analysis steps; and it does not cover RAM Z, inclined or reversed members, or nodal moments. Result: A65-V1-02 and A65-V1-03.
- **ID30 boundary:** the I-64 branch guard scopes event hookups and pick calls to `Plugin/Workspace/`. The Plugin has no `CommandClass` attribute. Result: AQ-05 answered "compatible".
- **G1 matching:** 2,485 Application and Domain lines contain "ram" in any case; none contain the word "RAM". Result: source of A65-V1-09.
- **Workflow V2:** the effective merge `8a021fb6` is an ancestor of the base. The header of `INITIATIVE_LIFECYCLE.md` still says "not effective", which is outside I-65's scope and not raised.
- **Not verified:** the strings inside the RAM binary (security policy, MCP server), the OpenRE manual content, and the external Bentley URLs.
