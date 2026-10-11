# I-64 — Revisión final del cierre del gate F1 por el Coordinator: F1 = PASS

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras la versión 3 del paquete de cierre
(`0f49f691`, CI 38173373735 push 4/4) y la parada en COORDINATOR_F1_GATE_CLOSE_REQUIRED. La revisión anterior, CHANGES REQUIRED
sobre `b35548fd` (`coordinator-gate-close-review-1.md`), es histórica y se conserva.

```text
I-64 — COORDINATOR FINAL F1 GATE REVIEW
Reviewed SHA:
0f49f69160cdffc1d2933d6165d5f472b5793d84
MainSha:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Exact CI:
38173373735
push
4/4 SUCCESS
Product tree:
src/ and tests/ =
e0205b5a9f000bbd44aa1f7556d30f06411c3db7
F1 — WORKSPACE FOUNDATION
GATE PASS
F1 = PASS
The previous CHANGES REQUIRED disposition on b35548fd is historical and
remains preserved.
Its only material blocker, D-T3D-01 / INV-24, was resolved by:
I-64-A-7 =
AGREED / ACTIVE
Architect:
R20261011T202537Z-1990
AGREED
0 REQUIRED
0 OPTIONAL
Coordinator:
AGREED
M-04:
YES
Owner approval:
NOT REQUIRED
Owner INFORMED
F1-T1-MODEL =
COMPLETE_UNDER_I64_SCOPE_BRIDGE
F1-T2-BRIDGE =
COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE
F1-T3A-HOST-CORE =
COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE
F1-T3B-HOST-UI =
COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE
F1-T3C-PROFILE-DOC =
COMPLETE
F1-T3D-SMOKE-PROCEDURE =
COMPLETE / ACCEPTED
Record:
INV-24 F1 portion =
SATISFIED UNDER I-64-A-7
For Smoke-1:
S1-06a..e =
NOT_APPLICABLE_BY_DESIGN
NEVER PASS
SM-14a..e =
SUPPLEMENTARY_EVIDENCE
S1-06f/g/h =
REMAIN MANDATORY
For F4 / Smoke-2:
S2-16..S2-22 =
MANDATORY / BINDING
Lifecycle-wide INV-24 =
OPEN
It remains open until S2-16..S2-22 and the applicable final OV assignment
are executed.
This does not block F1 PASS.
R-01:
ACCEPT evidence by exact T1/T3A tests.
No retrospective contract rewrite.
R-02:
ACCEPT global UI/AutoCAD boundary guards as actual INV-13 protection.
R-03:
ACCEPT the frozen Core simulated-source verification for the modal branch
of INV-23.
Smoke-1 S1-07 observes real RACKEDITAR coexistence through command-active
blocking.
Do not reopen classic commands in F1.
R-04:
DEFERRED BY DESIGN to F3.
R-05:
ACCEPT automated containment evidence; Smoke-1 remains supplementary host
observation.
R-06:
host observations remain part of Smoke-1 after F1 PASS.
R-07:
DISPOSED.
R-08:
protocol debt remains external in I-62.
Does not block I-64.
R-09:
DISPOSED.
R-10:
ADR 0047 remaining PROPOSED is acceptable for F1.
Owner acceptance is outside this gate close.
R-11:
ACCEPT §40 as the effective T1 Coordinator close disposition.
R-12:
historical Controller failures remain preserved.
A-4/A-6 protocol substitutions are accepted for I-64.
R-13:
DISPOSED by active A-7.
R-14:
DISPOSED by corrected Smoke-1 fixture requirements.
Final F1 product identity remains:
e0205b5a9f000bbd44aa1f7556d30f06411c3db7
No new product verification is required for the gate-close documentation.
Do not rewrite any historical RED, verification, nc2 or nc3 result.
Version this Coordinator F1 PASS decision.
In the same documentary commit:

1. record F1 = PASS;
2. record this gate review verbatim;
3. append the appropriate evidence section;
4. set:

last_evidence_commit =
0f49f69160cdffc1d2933d6165d5f472b5793d84

5. set the next action to Smoke-1 preparation;
6. preserve lifecycle-wide INV-24 as OPEN;
7. preserve protocol_debt for I-62;
8. do not touch src/, tests/, projects, ROADMAP or HANDOFF.

Push and require exact CI 4/4.
Smoke-1 itself is NOT yet authorized to run.
The next phase is preparation of a concrete Smoke-1 Candidate.
Required before execution:
A. CANDIDATE
Establish exact Candidate SHA according to the frozen/manual-validation
rules, including:

* clean tree;
* full local UI suite PASS;
* required Debug builds/tests;
* exact-SHA CI SUCCESS;
* exact RackCad DLL SHA-256.

A later documentary F1 PASS commit does not by itself change product bytes;
Candidate identity must nevertheless follow the applicable Candidate rule.
B. FIXTURES
Prepare/identify BOTH:
PR-12 DIAGNOSTICO
and
PR-13 SIN IDENTIDAD
with:

* source file;
* SHA-256;
* provenance/purpose;
* exact expected panel title/text;
* never-save requirement.

Do not use a generic block as SIN IDENTIDAD.
C. I-52
Only after the F1 PASS decision is versioned and green:
request the applicable I-52 host RELEASE / acknowledgement for Smoke-1.
The acknowledgement must provide the reference values required by T3C and
resolve the one-time DLL-load route and PaletteKeyAcceptance.
Do not launch AutoCAD before that RELEASE.
D. PROFILE
Prepare the T3C BASELINE command and approved tool hashes.
Do not capture the actual Smoke-1 BASELINE until the I-52 RELEASE and
acknowledgement are present and all prerequisites pass.
Do NOT execute Smoke-1 merely because F1 is PASS.
Return to Coordinator after Candidate + fixtures + I-52 RELEASE are ready.
Required next stop:
COORDINATOR_SMOKE1_AUTHORIZATION_REQUIRED
Return:

* F1 PASS documentary commit and exact CI;
* Candidate SHA;
* Candidate DLL SHA-256;
* local Full/UI results;
* exact Candidate CI;
* PR-12 fixture identity;
* PR-13 fixture identity;
* I-52 RELEASE / acknowledgement;
* PaletteKeyAcceptance;
* one-time load disposition;
* T3C tool hashes;
* confirmation acad.exe = 0;
* confirmation no BASELINE/Smoke execution occurred yet.

Do not yet:

* launch AutoCAD;
* execute Smoke-1;
* take the Smoke-1 BASELINE;
* mutate profile/security;
* start F2;
* start F3/F4;
* declare Smoke-1 PASS.

F1 itself is now PASS.
```
