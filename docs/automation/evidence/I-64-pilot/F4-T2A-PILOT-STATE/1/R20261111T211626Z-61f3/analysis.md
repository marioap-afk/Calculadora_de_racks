# Analysis - F4-T2A-PILOT-STATE replan R20261111T211626Z-61f3 (A4 FAIL, STOP P-03)

Written by the I-64 main session under the Coordinator order "I-64 - F4-T2A CONTRACT COMPACTION / REISSUE DISPOSITION"
(docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t2a-contract-compaction.md), which classifies the defect.

## Facts

- Single replan authorized by F4-E19 after planning R20261111T204805Z-13b0 (A5 FAIL). Codex gpt-6-luna / high (effective,
  from turn_context), read-only, binary 5449c1f0, config.toml Snapshot G at exit and entry; exit code 0; terminal event
  turn.completed; no participants. BaseSha 0a563cfb (docs-only custody of 13b0), AuthorityRevision 4dc111b5, MainSha 812baf87.
- The strengthened prompt (explicit ban on grammar, number, punctuation and accent normalization, the 13b0 example, and the
  announced byte comparison) worked for what it targeted: Invariants (15, including "lo leen"), the 18 AcceptanceCriteria
  of 13b0, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective and AllowedWriteScope are byte-identical
  to their sources.
- The deterministic byte-fidelity comparison (byte-fidelity-check.json), run by the session before acceptance, REJECTED the
  output: ForbiddenWriteScope has 138 entries instead of 139. Entry 55 of the contract,
  src/RackCad.Application/Systems/PushBack/PushBackHighEndBeamGeometry.cs, is missing; every other entry is present in the
  contract order.
- A1-A8 without short-circuit: A1-A3 and A5-A8 pass; A4 FAIL (one forbidden entry of the contract not covered).
- nc4 R20261111T211955Z-e9cb: A3 fail and the rest equal to the real evaluation (relative oracle met); it enables nothing.

## Classification

- Controller planning-output fidelity defect of the same class as 13b0 (copy of the structured output), with a different
  mechanism: omission of one element of a long verbatim list, instead of a rewrite inside a string.
- Not a contract, scope, product or implementation defect. STOP (P-03), REJECTED_BEFORE_INVOCATION; the Worker was not
  invoked; attempts stays 1 of 3. delegation.json is preserved as produced and is not repaired.

## Contributing factor

- ForbiddenWriteScope of the contract has 139 literal entries. The allowed scope lives inside two mixed directories
  (src/RackCad.Application/Systems/PushBack/ and src/RackCad.Application/Workspace/), so those directories could not be
  forbidden by prefix and their existing files were enumerated one by one (52 + 27 + 3). The longer the verbatim list,
  the higher the copy pressure on the model.
