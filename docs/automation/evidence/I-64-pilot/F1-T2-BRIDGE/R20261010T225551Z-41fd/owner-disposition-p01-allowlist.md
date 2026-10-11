# I-64 F1-T2-BRIDGE — Disposición del Owner/Coordinator: lista cerrada de dos instantáneas de config.toml

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-10, tras el segundo STOP P-01 antes de invocar la
verificación R20261010T225551Z-41fd. La disposición pide no versionarla todavía; se versiona en la custodia del cierre de T2.
SNAPSHOT A («73890CA3…» en el texto) es la línea base aceptada antes por el Owner:
73890CA3319206B85B68A42E23E8D323C45361CFC29E06B3E116EFFF5DAD0FBF (estado I-64, `relay_baseline`).

```text
OWNER / COORDINATOR DISPOSITION — I-64 F1-T2-BRIDGE
TARGET_SHA:
e2f642b86197441eea0838c8055c1db666606f11
origin/main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
For the remainder of the CURRENT F1-T2-BRIDGE verification/control chain,
accept exactly either of these two config.toml hashes:
SNAPSHOT A:
73890CA3…
SNAPSHOT B:
64BEEBC7401225A910A80EC166BFD852A8AC9E9E06EC1CCEFB6663DC72AF397A
Both have:

* 105 section/key names;
* the same ordered section/key-name inventory.

Do NOT read or persist config values.
This is a CLOSED two-snapshot allowlist.
It is NOT:

* acceptance of arbitrary config changes;
* a wildcard;
* a permanent protocol change;
* evidence that the differing values are semantically equivalent.

If config.toml has ANY hash other than A or B:
STOP P-01.
If the key-name inventory differs from the known 105-name inventory:
STOP P-01 even if a hash is somehow otherwise expected.
Both exact snapshots were previously observed and individually accepted
during this same emergency continuation.
The Owner's Codex app has been observed alternating between these exact
snapshots while preserving the key-name inventory.
Repeated STOPs caused solely by A <-> B alternation would no longer add
useful safety information.
A third state remains untrusted.
Do NOT version this decision yet.
Preserve the current protocol ordering:
verification
-> nc1
-> nc2
-> ScopeBridge
-> nc3
-> custody/state/evidence commit
No session Git write before those controls finish.
Re-read immediately before invoking Codex:

1. config.toml hash;
2. ordered key-name inventory;
3. HEAD;
4. origin branch;
5. origin/main;
6. process ownership.

Accept config only if:
hash == SNAPSHOT A
OR
hash == SNAPSHOT B
AND key-name inventory == known 105-name inventory.
Require:
HEAD =
origin/architecture/workspace-persistente-rackcad =
e2f642b86197441eea0838c8055c1db666606f11
origin/main =
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Otherwise STOP.
The prepared verification:
R20261010T225551Z-41fd
remains authorized if its invocation has not started.
Invoke the single valid:
CONTROLLER_VERIFICATION
gpt-6-luna
high
read-only
against:
DelegationRunId:
R20261010T082206Z-21c1
WorkRunId:
R20261010T202008Z-3df4
BaseSha:
d5bdcb7aff31ba54901a3a0acdbfd81be92ab516
RedSha:
31e3c57a66025ba339acb2c796298e71153126cc
CurrentSha:
e2f642b86197441eea0838c8055c1db666606f11
Expected successful outcome:
EXECUTION_VERIFIED / NONE
If valid verification = REWORK:
STOP S-11.
No correction Worker.
Without Git writes:

1. nc1;
2. nc2;
3. I64-SCOPE-BRIDGE-01 REAL + ADVERSARIAL under A-4;
4. nc3.

nc4 remains PASS and is not repeated.
Then custody and documentary close.
Stop at:
COORDINATOR_T2_CLOSE_REVIEW_REQUIRED
```
