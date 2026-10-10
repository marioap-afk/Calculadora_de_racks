# I-64 — Acuerdo del Coordinator sobre A-4 (I64-SCOPE-BRIDGE-01)

Registro versionado de la disposición condicional del Coordinator contenida en la orden NIGHT RUN (`coordinator-order-night-run.md`,
blob `e4ace3242fa57b8e94396782557c1b0f1e8abf17`). La sesión responsable comprobó cada predicado de forma mecánica antes de ejecutar el
puente.

**Cláusula de la orden** (literal):

```text
This prompt carries the Coordinator disposition:
IF AND ONLY IF the exact A-2 or its authorized append-only correcting A-n
receives:
Architect = AGREED
REQUIRED = 0
and all of the following remain true:

* Owner emergency mandate unchanged;
* Applies-to = I-64 only;
* historical nc2 remains FAIL;
* verifier identity is exact;
* M-04 = YES;
* no other M is YES;
* no I-61 text modified;
* no I-62 branch modified;
* CI of the amendment/evidence commit = 4/4 SUCCESS;

THEN the Coordinator decision is:
COORDINATOR = AGREED
for that exact amendment object.
Version this decision before executing the bridge.
If any predicate is false:
Coordinator agreement is NOT granted.
```

**Objeto exacto:** `docs/initiatives/I-64-A-4.md`, blob `d587403e40243beda5db61779ef8e9687c480619`, en el commit
`0a61ee3a50ea4bb01a5dce0d791a77fa4b0b3446`. Es la A-n correctora autorizada de A-2, a través de A-3. El verificador autorizado es el blob
`cf381ab96a3d945a9569d56bb3aaf546e3f2fce1` (SHA-256 `a3d4ed8a25e99ea6b8bc0af945e6e688c13f472a7e0745752c79e04ee79f7d65`).

| Predicado | Valor | Evidencia |
|---|---|---|
| Architect = AGREED, REQUIRED = 0 | true | `architect-review/R20261010T031016Z-cca9/` (`gpt-6.1-sol` / `high`, ARCHITECTURE_REVIEW, Deep, SEPARATE SESSION): AGREED, 0 REQUIRED, 0 OPTIONAL; I64-A2-RUNID-FAIL-OPEN e I64-A3-JSON-CONSTANT-FAIL-OPEN = CLOSED |
| Mandato de emergencia del Owner sin cambios | true | `owner-mandate-emergency.md`, blob `4271dd7e128b001e5aaf8d1bbedcafbfc40faadd` en HEAD |
| Applies-to = I-64 solamente | true | una sola línea `Applies-to: I-64` en A-4 |
| nc2 histórico sigue en FAIL | true | `F1-T1-MODEL-nc2/R20261002T184050Z-8415/oracle-result.json`: CONTROL NOT PASSED, OracleRelative false; sus archivos de evidencia no cambian desde `14685b45` |
| Identidad exacta del verificador | true | blob `cf381ab9…` en `0a61ee3a`, en HEAD y citado en A-4 |
| M-04 = YES | true | bloque M de A-4 |
| Ningún otro M = YES | true | bloque M de A-4 |
| Sin cambios del texto de I-61 | true | `git diff --name-only 819955d6 HEAD` vacío para AUTOMATION_PLAN, `agent-execution/`, LIFECYCLE, WORKFLOW y AGENTS |
| Sin cambios de la rama de I-62 | true | ningún archivo de I-62 en el diff de la rama de I-64; la sesión no publica en `architecture/portabilidad-coordinador-principal` (punta observada `362bec126c688e3a12e8ceb97cdf1124960ad47f`) |
| CI del commit de la enmienda = 4/4 SUCCESS | true | corrida 38019583881 (push, `headSha` `0a61ee3a…`), los cuatro jobs en success |

**Decisión:**

```text
COORDINATOR = AGREED
for the exact amendment object docs/initiatives/I-64-A-4.md (blob d587403e40243beda5db61779ef8e9687c480619, commit 0a61ee3a)
Owner = SATISFIED (emergency mandate)
Architect = AGREED (R20261010T031016Z-cca9)
Active amendment for I64-SCOPE-BRIDGE-01 = A-4 (A-1, A-2 and A-3 remain historical and immutable)
```

El puente se ejecuta contra F1-T1-MODEL solo después de este registro (A-4 §4 y A-2 §3).
