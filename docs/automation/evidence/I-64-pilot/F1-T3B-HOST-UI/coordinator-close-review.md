# I-64 — Revisión del Coordinator del cierre de F1-T3B-HOST-UI

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras el cierre documental ffd111a9
(CI 38155884290 4/4).

```text
I-64 — COORDINATOR T3B CLOSE REVIEW
TARGET SHA:
ffd111a9c9984f67725ac47bba3eecb9db2c58d0
origin/main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Coordinator disposition:
PASS
Record:
F1-T3B-HOST-UI =
COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE
ProductVerifiedSha:
e0205b5a9f000bbd44aa1f7556d30f06411c3db7
ProductStatus:
EXECUTION_VERIFIED / NONE
ProtocolClose:
SATISFIED UNDER A-4 + A-6
Preserve:
37e1 =
EXECUTION_BLOCKED / STOP / Scope / C-02
7910 =
EXECUTION_VERIFIED / NONE
T3B-SCOPE-NONSEMANTIC-WHITESPACE-01 =
ACCEPTED exact exception
nc1 =
PASS
Controller nc2 =
CONTROL NOT PASSED
I64-SCOPE-BRIDGE-01 =
PASS
Controller nc3 =
NOT PASSED / P-04
I64-FREETEXT-BRIDGE-01 =
PASS
nc4 =
PASS
P-02 signal =
FALSE_POSITIVE_SESSION_OWN_PROCESS
Do not rerun or relabel any T3B control.
In the next documentary commit set:
last_evidence_commit =
ffd111a9c9984f67725ac47bba3eecb9db2c58d0
Record T3B close review = PASS.
No product changes.
Authorize:
CONTRACT / DOCUMENT PREPARATION ONLY
No product Worker.
No AutoCAD execution.
No profile mutation.
No registry mutation.
No TRUSTEDPATHS / SECURELOAD mutation.
No ACL mutation.
T3C is session-owned documentary work.
Prepare the reproducible profile-diff procedure required by F1.
The procedure must define:

1. baseline capture before Smoke-1;
2. post-run capture after Smoke-1;
3. exact comparison rules;
4. allowed differences;
5. forbidden differences;
6. evidence file format;
7. redaction rules;
8. STOP conditions;
9. restoration/rollback expectations if forbidden drift appears;
10. relationship to I-52 host ownership/release.

At minimum cover:

* active AutoCAD profile name;
* TRUSTEDPATHS;
* SECURELOAD;
* relevant RackCad profile variables/settings;
* registry locations explicitly within the approved host baseline;
* plugin/loading-related state;
* environment facts needed to prove RackCad did not mutate the host profile.

Do not broaden into unrelated Windows settings.
Freshly inspect I-52 before writing the final procedure.
Current known branch:
feature/rackmirror-espejo-semantico
Do not assume prior host baseline is still authoritative.
T3C may read and reference I-52 evidence.
T3C must NOT:

* alter I-52;
* authorize its host;
* launch AutoCAD;
* change ACL/TRUSTEDPATHS/SECURELOAD/profile/registry.

Smoke-1 still requires the applicable I-52 RELEASE.
Prefer a deterministic scriptable procedure.
Document:

* exact read-only commands;
* normalization rules;
* sorting/canonicalization;
* SHA-256 of captured evidence where practical;
* how to redact local paths/user-specific profile names if required;
* expected stable fields;
* permitted ephemeral fields, if any.

The procedure must distinguish:
BASELINE
POST
DIFF
DISPOSITION
Do not modify:
src/
tests/
project files
ROADMAP
HANDOFF
unless a later Coordinator decision explicitly authorizes it.
After:

1. versioning this T3B close decision/state correction;
2. exact CI 4/4;
3. fresh I-52/I-65/I-62 branch check;
4. drafting the complete T3C profile-diff procedure;
5. validating it is read-only and reproducible;

stop at:
COORDINATOR_T3C_REVIEW_REQUIRED
Return:

* documentary commit + CI;
* current HEAD/main;
* fresh I-52/I-65/I-62 tips;
* T3C procedure path;
* exact baseline capture scope;
* exact POST capture scope;
* normalization/diff rules;
* allowed differences;
* forbidden differences;
* STOP conditions;
* proof that no AutoCAD/profile/registry/security mutation occurred;
* what remains before F1 can be declared PASS.

Do NOT execute Smoke-1.
Do NOT declare F1 PASS.
```
