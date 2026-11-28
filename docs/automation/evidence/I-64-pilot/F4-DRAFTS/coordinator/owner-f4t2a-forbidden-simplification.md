I-64 — F4-T2A
FORBIDDEN SCOPE SIMPLIFICATION DISPOSITION
Current branch:
26c80eb4a13e9fdbc8a93074500de2606fde3800
Current main:
812baf879612320653094a8aaf46aca0bac81383
Prior planning failures:
13b0:
A5 FAIL
REJECTED_BEFORE_INVOCATION
61f3:
A4 FAIL
REJECTED_BEFORE_INVOCATION
Worker:
NOT INVOKED
Attempts:
1 / 3
UNCHANGED
Authorize OPTION 1.
Reissue the F4-T2A contract with a REDUCED ForbiddenWriteScope.
The reduction is allowed ONLY for individually enumerated files located
inside mixed directories where:

* some descendants are explicitly allowed by AllowedWriteScope;
* therefore the directory itself cannot safely be forbidden by prefix.

The effective write boundary remains the exact AllowedWriteScope whitelist.
The previous requirement:
"every original forbidden path is still explicitly forbidden"
is SUPERSEDED for this contract revision.
New rule:
Every path removed from ForbiddenWriteScope must be proven to remain
UNWRITABLE because it is outside the exact AllowedWriteScope.
This is an intentional reduction of redundant defense-in-depth.
It is NOT claimed to be byte- or representation-equivalent to the prior
ForbiddenWriteScope.
For F4-T2A the write-scope rule is:
git diff --name-only BaseSha..CurrentSha
MUST satisfy:
every changed path ∈ AllowedWriteScope
AND
no changed path intersects retained ForbiddenWriteScope.
Therefore a path outside AllowedWriteScope remains a Scope violation even if
it is no longer separately enumerated in ForbiddenWriteScope.
AllowedWriteScope is the PRIMARY exhaustive whitelist.
ForbiddenWriteScope remains a SECONDARY explicit barrier for paths/subtrees
that can be expressed compactly without overlapping allowed candidates.
Based on the deterministic report:
original ForbiddenWriteScope:
139 entries
retain:
57 entries
remove:
82 individually enumerated files
ONLY where those files are inside the mixed:
Systems/PushBack/
and:
Workspace/
areas described by the report and are NOT in AllowedWriteScope.
Do not remove any other entry merely to reduce list length.
The exact removed set must come from the deterministic report, not manual
selection.
For every one of the 82 removed entries, prove mechanically:

1. path existed in original ForbiddenWriteScope;
2. path is NOT in AllowedWriteScope;
3. path is NOT covered by an allowed prefix;
4. if Worker changes that path, Scope fails because:
changed path ∉ AllowedWriteScope;
5. no candidate/allowed file is accidentally removed from the whitelist.

Produce a machine-readable removed-path report.
Every retained prefix/file must still be semantically correct.
Prove:

* no retained forbidden prefix contains an allowed path;
* no AllowedWriteScope path intersects ForbiddenWriteScope;
* the specific path omitted by 61f3 remains protected either:
a) explicitly by retained ForbiddenWriteScope; or
b) by being outside AllowedWriteScope.

Record which mechanism protects it.
Because more protection now depends on the whitelist:
AllowedWriteScope must be copied EXACTLY from the contract.
Its fidelity becomes especially critical.
Before accepting planning, mechanically prove:
delegation.AllowedWriteScope
contract.AllowedWriteScope
byte-for-byte / element-for-element in order.
No missing entry.
No additional entry.
No prefix broadening.
No normalization.
If this check fails:
STOP.
Before contract issuance, run a deterministic contract-level test:
For each removed forbidden path P:
simulate changed-path set = {P}
Expected:
Scope = FAIL
because P is outside AllowedWriteScope.
Also test:
for each allowed path A:
simulate changed-path set = {A}
Expected:
Scope is not rejected merely for being outside AllowedWriteScope.
This establishes that removing the redundant ForbiddenWriteScope entry did
not make the file writable.
Issue a NEW F4-T2A contract revision.
Record explicitly:
Product semantics:
UNCHANGED
AllowedWriteScope:
UNCHANGED
ForbiddenWriteScope:
REDUCED 139 -> 57
Security effect:
redundant explicit deny removed for 82 paths;
exact allow-list enforcement remains authoritative.
Reason:
reduce planning-output copy fragility after two rejected delegations while
preserving the effective write boundary.
The 61f3 custody already exists at:
26c80eb4...
Do not create another custody commit merely to restate 61f3.
Version:

* this disposition;
* reduced-scope report;
* removed-path proof;
* new contract revision;

in the normal contract-reissue documentation commit.
After the new contract is issued:
authorize ONE planning invocation.
New RunId.
Same Controller:
gpt-6-luna / high
Keep the strengthened literal-copy instructions.
Run deterministic collection comparison BEFORE acceptance.
Required:
A1-A8 = 8 PASS
In particular:
A4:
new reduced ForbiddenWriteScope copied exactly
A5:
all invariants copied exactly
No semantic-equivalence waiver.
Run nc4 again against the new contract/delegation pair.
Required normal relative oracle.
If:

* reduction proof PASS;
* contract issuance valid;
* planning normal;
* A1-A8 = 8 PASS;
* nc4 PASS;
* config accepted;
* BaseSha/MainSha valid;
* tree/process checks clean;

then:
F4-T2A Worker is AUTHORIZED ONCE.
No further Owner approval required.
If planning against the reduced 57-entry ForbiddenWriteScope again fails
because the Controller cannot faithfully emit the contract collections:
STOP.
Do NOT:

* reduce ForbiddenWriteScope further;
* retry planning again;
* switch models automatically;
* manually patch delegation.json.

Return to Coordinator.
At that point the evidence is sufficient to justify reconsidering the
protocol-level approach where mechanical contract collections are separated
from Controller judgment fields.
F4-T2A remains:
attempts = 1 / 3
No Worker has run.
Planning/contract recovery does not consume a product attempt.
F4-T1:
CLOSED / PASS
F4-T2A:
NEW CONTRACT REVISION AUTHORIZED
ForbiddenWriteScope:
139 -> 57
AllowedWriteScope:
UNCHANGED / PRIMARY WHITELIST
attempts:
1 / 3
Worker:
NOT INVOKED
F4-T2B:
WAITING
F4-T2C:
WAITING
F4-T3/T4:
WAITING
Smoke-2:
PENDING
