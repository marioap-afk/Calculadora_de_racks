# I-52 — CT-21D authority baseline, phase 2 delta 2 (round V4): process evidence

Implementer-side process record of round V4 (gate opened by the Coordinator in decisions section 215, applying the Architect's delta ruling of section 214). Nothing here is a ruling, a candidate or a seal; nothing here was executed on a host.

| File | Content |
|---|---|
| `coordination-sheet.md` | the binding coordination sheet given to every V4 implementer: limits, file conventions, shared identifiers (COORD section 2) and the `DEPENDS_ON` graph (COORD section 3) |
| `workflow-round-v4.js` | the orchestration script of the round: one implementer per artifact (BA-04 + BA-02b, BA-05, BA-06, BA-07, BA-08 + BA-09, BA-03, then BA-10 after the catalog), then two completeness critics (decision coverage; cross-artifact consistency and limits); the verbatim prompts are in the script |
| `round-v4-reports.json` | the structured reports of the seven implementers (per finding: status and location of the fix; open questions; cross-artifact notes; counts) and of the two critics (gaps with evidence and required fix) |
| `critic-gaps-round-v4.txt` | the 27 gaps of the two critics (8 major, 19 minor), one block per gap |
| `workflow-correction-pass.js` | the targeted correction pass over those gaps (BA-04 + BA-02b, BA-08 + BA-09, BA-06, then BA-10 and the cost memo), with the fixed ids `AQ-V4-01..AQ-V4-14` of the open Architect questions, then one re-check critic |
| `correction-pass-reports.json` | the reports of the correction pass and of the re-check critic (25 of 25 in-scope gaps closed; 8 minor residues, closed afterwards by the lead as recorded in decisions section 216) |

The reports are the Implementer's own self-check. They are not the Architect's review: the delta review of round V4 is a separate gate.
