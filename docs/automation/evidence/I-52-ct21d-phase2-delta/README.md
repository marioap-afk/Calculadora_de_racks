# I-52 — CT-21D baseline artifacts, phase-2 delta: independent static analyses (evidence)

Read-only static analyses made for the phase-2 delta (decisions sections 211 and 212). Nothing was built, run or
tested; no host program was started. Code facts are bound to `3375aadbbf929427a6d106b2fff275d64863b89c` (the bound
historical SHA); each analysis read the code with `git show` / `git grep` at that commit.

| File | Content | Used by |
|---|---|---|
| `sa1-contract-fragments.md` | the **only** input of the independent compiler: verbatim contract fragments (V2.1 4.2, 7, 7.1, 15.2..15.7, 19.1, 27.2; V2.2 PA-6, PA-10; V2.4 section 1) | BA-06 V3 section 3.2 (AR2-37) |
| `sa1-compiled-operations.json` | the contract-required operations compiled from the fragments alone (203 rows, 25 ambiguities), with exact clause quotes | BA-06 V3 section 3.2 |
| `sa1-comparison-with-t3-and-r-list.json` | a separate comparator's clause-by-clause comparison of the compiled list with the critic's list T3 and the BA-06 V2 R-list: mapping, rows seen only by T3 or the R-list, 51 differences with resolutions | BA-06 V3 sections 3.2.1..3.2.3 |
| `sa2-product-entity-types.json` | the entity and symbol-record types RackCad itself creates, per kind, with an adversarial verification | BA-05 V3 section 2.5; BA-08 V3 section 14 |
| `sa3a-selective-writer-facts.json` | Selective frontal/planta writer constants, layer reachability (SL-Y), NOD and project variables, with an adversarial verification | BA-08 V3 sections 3.1, 4, 11 |
| `sa3b-selective-misc-facts.json` | missing pieces, command count, dynamic parameters, plan mutability, nested naming, planta selection, required block names with catalog hashes, static caches, with an adversarial verification | BA-08 V3; BA-06 V3 sections 4.3, 11 |
| `sa4-static-state-census.json` | mutable static state of `RackCad.UI` and the warm-up-relevant static state of Application, Plugin and Domain, with an adversarial verification | BA-06 V3 section 6; BA-09 V3 section 6 |
| `prompts.json` | the exact prompts of every task (compiler, comparator, analyses, verifier template) | condition 5 of V2.2 PA-11 (AR2-37: the task inputs are published) |

**Independence of the compiler (condition 5).** The compiler's prompt gave it one file (`sa1-contract-fragments.md`)
and forbade reading any other file or running git; it never received BA-06, the critic's list or the static-analysis
evidence. The comparator received the compiled list, the critic's JSON, BA-06 V2 and the fragments, and did not read
source code. Each fact analysis was checked by a separate adversarial verifier that re-read the code; the verifiers'
verdicts (`CONFIRMED`, `CORRECTED`, `UNCERTAIN`) are stored with the analyses, and the artifacts use the corrected
statement where a verifier corrected one.
