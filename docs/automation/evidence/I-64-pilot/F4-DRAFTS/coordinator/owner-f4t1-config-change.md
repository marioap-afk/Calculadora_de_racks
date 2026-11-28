I-64 — F4-T1-DRAFT-CORE
CONFIG CHANGE DISPOSITION
Current branch:
61d83788e7fa8decd861626b7e3ca89c8f23b9b4
Prepared reverification:
4bb6
Current config SHA-256:
7C1516A9...
Previous accepted baseline:
Snapshot F
AAEB58D8...
OPTION 1 AUTHORIZED.
The current config may become:
SNAPSHOT G
only after ONE neutral Controller probe passes.
Do NOT launch reverification 4bb6 before that probe closes successfully.
Measured static facts are acceptable as the basis for the probe:

* same 104 keys;
* same key order;
* no added keys;
* no removed keys;
* 8 changed values;
* changed values are application-managed runtime/install values;
* model unchanged;
* reasoning effort unchanged;
* service tier unchanged;
* sandbox unchanged;
* accredited relay binary unchanged.

The change from Codex app version 26.1103 to 26.1106 is NOT itself grounds
for automatic acceptance.
Run ONE neutral probe using:

* the same accredited relay binary:
SHA-256 4F91AE86...
* current config:
SHA-256 7C1516A9...
* Controller cell:
gpt-6-luna / high / read-only

Probe must verify effective:

* model;
* effort;
* sandbox;
* approval policy;
* permission profile;
* routing;
* tool availability needed by Controller;
* normal structured output;
* no unexpected Git write;
* no participant leak;
* no usage/credit warning.

Also compare the effective turn-context fields against the accepted Snapshot
F probe baseline.
If the neutral probe shows no material behavioral change:
accept:
Snapshot G
with SHA-256:
7C1516A9...
Record:

* config SHA;
* ordered key-name hash;
* safe semantic inventory;
* exact 8 changed keys;
* app version transition 26.1103 -> 26.1106;
* effective Controller comparison against F;
* probe hashes.

Then Snapshot G replaces F as the active accepted baseline for subsequent
I-64 Controller work.
STOP if the probe shows any unexplained change in:

* effective model;
* effort;
* sandbox;
* approval/permissions;
* routing;
* required Controller tooling;
* execution behavior.

Do not accept G merely because the static diff looks installation-related.
If Snapshot G probe = PASS:
launch the already prepared reverification:
4bb6
using:

* reissued handoff;
* CurrentSha = ce73ceb7;
* HEAD/remote = 61d83788;
* authorized post-custody Identity substitution;
* authorized narrow Tests oracle;
* Snapshot G as accepted config baseline.

Do NOT change the prepared product identity or handoff facts merely because
the config baseline changed.
If 4bb6 = EXECUTION_VERIFIED:
run nc1
then nc2
then nc3
under the same post-custody Identity rule.
If all controls close:
F4-T1 = CLOSED / PASS
Then start:
F4-T2
with:
attempts = 1 / 3
and mandatory N-2 anti-aliasing requirement.
If config.toml changes again before or during 4bb6 / controls:
STOP.
Do not chain Snapshot H automatically.
Return with the measured diff.
F4-T1:
Attempt 3 product complete
N-1:
CORRECTED
Handoff reissue:
READY
Post-custody Identity:
AUTHORIZED
Snapshot G:
PROBE AUTHORIZED
NOT YET ACCEPTED
Reverification 4bb6:
PREPARED / NOT YET LAUNCHED
nc1-nc3:
WAITING
F4-T2:
WAITING
future attempts 1 / 3
