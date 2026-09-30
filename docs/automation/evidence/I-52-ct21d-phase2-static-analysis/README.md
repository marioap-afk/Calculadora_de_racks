# I-52 CT-21D — Phase-2 static analysis evidence (read-only, against `origin/main` 3375aadb)

Raw outputs of the independent static analysis performed for the **Authority Baseline Artifacts, Phase 2** gate. They are evidence for BA-06 V2 (inventory review, census), BA-08 V2 (kind baseline facts), BA-05 V2 (tolerance sources) and BA-09 V2 (warm-up residual). **Nothing was compiled, executed or observed in AutoCAD.**

| Item | Value |
|---|---|
| source analysed | an extract (`git archive origin/main src assets tests docs/adr .github eng/ci`) of commit `3375aadbbf929427a6d106b2fff275d64863b89c` |
| method | a governed multi-agent static review (workflow run `wf_9ae00c65-313`, 13 read-only agents), 2026-09-30 |
| independence (V2.2 PA-11 condition 5) | the two operation reviewers (`B1a` call-graph traversal; `B1b` API-surface sweep) and their completeness critic were **blind**: they were forbidden to open anything outside the extract, which does **not** contain the BA-06 V1 inventory; none of them compiled the inventory |
| critics | `critic_ops` adjudicated every difference between `B1a` and `B1b` in the code and swept again for missed sites; `critic_census` re-swept the three projects for static state missing from the census |

| File | Content |
|---|---|
| `B1a-blind-op-review-call-graph.json` | blind review 1: call-graph traversal from the command entry points |
| `B1b-blind-op-review-api-sweep.json` | blind review 2: sweep of every AutoCAD mutating API call site |
| `critic_ops-op-review-completeness-critic.json` | union, adjudications, contract-required operations, residual facts |
| `B2a..B2d-census-*.json` | mutable static-state census of Domain, Application and Plugin |
| `critic_census-census-completeness-critic.json` | census completeness sweep and corrections |
| `B3-blocks-csv-and-builders.json` | static derivation of the required library block names from `assets/catalogs/blocks.csv` and the builders |
| `B4-fail-closed-mapping.json` | V17 fail-closed conditions mapped to design fields and fixture constraints |
| `B5-tolerance-and-transform.json` | `GeometryTolerance` and transform facts |
| `B6-existing-readers-comparators-seams.json` | existing readers, comparators, AUTH-15 analysis, source-guard patterns |
| `B7-deps-loadset-commands.json` | project dependencies, load set, command registration, CI facts |

These files are evidence, not authority: the artifacts that cite them state the conclusions and their gate.
