# I-64 — Orden del Coordinator de revisión formal de I-64-A-14 (PREPARADA, SIN EMITIR)

Preparada por la sesión el 2026-11-23 a partir de la orden de A-7
(`docs/automation/evidence/I-64-pilot/F1-GATE/A-7/coordinator-a7-review-order.md`) y del lanzamiento de A-13
(`docs/automation/evidence/I-64-pilot/A-13-review/R20261110T022847Z-bc2b/`). **La emite el Coordinator**: el Owner, como en Q-C1 y
Q-C2. Hasta que la emita no autoriza nada.

Los campos marcados `[COORDINATOR]` los decide el Coordinator. La sesión solo escribe su propuesta, que es la de A-14 §9 y §12.

## Lo que se midió al prepararla (2026-11-23, sin lanzar nada)

| Comprobación | Medida | Resultado |
|---|---|---|
| Binario de la CLI | `Codex/bin/5449c1f0f26f1f98/codex.exe`, SHA-256 `4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17`; es el único `codex.exe` en `Codex/bin` | OK, el acreditado |
| `config.toml` | SHA-256 `3E9BC35B0E72641A81241EC3E32BF45010B8CC3CC93FDD4E3C5E8188E6D19FFC`, escrito `2026-11-23T19:21:15Z`. Tiene 105 nombres, con los mismos nombres ordenados que la L (`3ECF8E07…`): cambian valores, no nombres | **NO es la Snapshot L** (`B71650A6…`). `cfg_check_f4.py` da `Accepted = false` (código 7) |
| Rama lateral | `architecture/workspace-persistente-rackcad-a14` = `ab0170b76c34249b4d772ac18cc571c1b5a587f1`, en local y en `origin`; árbol limpio | OK |
| Rama de I-64 | `fa59bd82acff7ce276a1605c60d426c45a3e3b8d`, en local y en `origin` | OK, sin moverse |

**Consecuencia:** hoy la precondición E-2 falla, y la orden de abajo para antes de invocar. Hace falta una decisión del Owner sobre el
`config.toml` nuevo. Los precedentes son la J, la K y la L, donde el Owner eligió «Sonda neutra, luego …»
(`F4B-DRAFTS/coordinator/owner-config-snapshot-l-decision.md`). La sesión no lanzó ninguna sonda.

## Ajuste a la Snapshot M (2026-11-23, después de la medida de arriba)

**Autoridad:** la decisión del Owner de la quinta ronda (`owner-a14-decisions.md`), literal: «Sonda neutra, luego M
(Recomendado)», con la descripción «Como con la L: una sonda neutra con gpt-6-luna en solo lectura. Si da PASS, esta configuración
queda como Snapshot M y se lanza la revisión del Architect.»

**La sonda:** `R20261123T211742Z-282c`, desde el worktree lateral, da PASS. Binario `4F91AE86…`, gpt-6-luna / high, solo lectura,
política `never` y CLI 0.167.0-alpha.3.2. Frente a la sonda `2c14` de la L, el contexto efectivo solo difiere en `root_turn_id`. El
repositorio y `config.toml` no cambian durante la sonda. El único cambio de `config.toml` frente a la L es el GUID de la canalización
de `SKY_CUA_NATIVE_PIPE_DIRECTORY`: con el GUID de la instancia de la app de la L se reproduce byte a byte el SHA-256 de la L.

**El cambio en el texto de la orden:** solo se reemplazan las cuatro menciones de la Snapshot L:
- el bloque `config:` (nombre, SHA-256, fuente y sonda);
- E-2;
- las dos líneas de INVALID_OUTPUT.

Nada más cambia. Los campos `[COORDINATOR]` y el resto del texto quedan como estaban. Arriba, la medida de la preparación se conserva
como historia.

## Texto de la orden

```text
I-64 — COORDINATOR A-14 REVIEW ORDER

Amendment:
I-64-A-14 (Replaneacion hacia la ventana RackCad sin bloqueo)

Exact object:
commit:
ab0170b76c34249b4d772ac18cc571c1b5a587f1
branch:
architecture/workspace-persistente-rackcad-a14 (lateral, Q-C1)
parent:
fa59bd82acff7ce276a1605c60d426c45a3e3b8d (rama de I-64, F4B-E62)
path:
docs/initiatives/I-64-A-14.md
blob:
427ce3039516c1254f9ae96888b2e9649ce8b200
exact-SHA CI:
run 45263171189 = success, 4/4 jobs

Freeze authority:
commit:
9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311 (imagen en la rama: 55e7ca6337fef6dcc14f5c40bdd55daf88d78cbf)
path:
docs/initiatives/I-64-proposal-v5.md
blob:
dc1924ff5a73a4e529fbc5f995621d29351a091b

Status:
PROPOSED / NOT ACTIVE

Coordinator decisions in this order (A-14 Sec.10 step 4; Sec.12):

Q-C3, materiality:
[COORDINATOR: confirm or correct]
M-01..M-07 = YES
M-08 = YES (por prudencia)
archetype = NEW ARCHITECTURE
(A-14 Sec.9)

Q-C3, authority:
[COORDINATOR: confirm or correct]
Owner + Architect + Coordinator
Owner for the OWNER-RESERVED matters (exact choice in A-14 Sec.11);
Architect + Coordinator for the material M flags.

Sec.12 row 2:
[COORDINATOR: confirm or correct]
tramo 1 = the I-64 unit; later tramos = new units, named at their bootstrap (Q-C4).

Sec.12 row 3:
[COORDINATOR: position; the Architect reviews it, Q05]
Applies-to: all; scope extension of A-7 only; bridges A-1..A-6 and A-8..A-13 not extended;
Owner informed (A-14 Sec.11 point 15).

Owner:
REQUIRED for the OWNER-RESERVED matters, by the exact choice in Sec.11, only after Architect AGREED and
Coordinator AGREED. Not requested by this order. Never inferred.

Invoke exactly ONE formal Architect review.

Cell (Q-C2, decided 2026-11-23):
model:
gpt-6-luna
effort:
high
sandbox:
read-only
CLI:
codex-cli 0.167.0-alpha.3.2,
Codex/bin/5449c1f0f26f1f98/codex.exe,
SHA-256 4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17
config:
config.toml = Snapshot M
(3E9BC35B0E72641A81241EC3E32BF45010B8CC3CC93FDD4E3C5E8188E6D19FFC, 105 names,
OrderedNamesSha256 3ECF8E072CC52AE60A6E6D508DB54DCBFFFAFB04EAF9B52905A2CEAB0E0812E3; Owner decision 2026-11-23, fifth round of A-14;
neutral probe R20261123T211742Z-282c)
launcher:
the accredited gpt-6-luna line of the F4B runs:
-s read-only -m gpt-6-luna -c model_reasoning_effort="high"
plus --output-schema for the review, -o output.json and --json.
No other flag.

Working tree:
the lateral worktree, on the exact commit, clean.
NEVER the I-64 worktree: no read, no write, no git operation there.
No checkout, no commit, no push during the review.

STOP Coordinator BEFORE invocation if any of these does not hold:
E-1  the binary SHA-256 is exactly 4F91AE86...EFC17, and it is the only codex.exe under Codex/bin;
E-2  config.toml is exactly Snapshot M (sha256, 105 names, ordered-names sha256);
E-3  processes: 0 participants and 0 persistent unattributable, against the lateral worktree;
E-4  git: lateral branch local = remote = ab0170b7; lateral worktree clean; no git operation in
     progress; main checkout clean; exact-SHA CI of ab0170b7 = success;
E-5  the prompt and the schema are built from the exact files prepared for this order, and their
     SHA-256 values are recorded before launch.

After the run, the result is INVALID_OUTPUT (it does not count as the valid review) if:
- the effective model, effort, sandbox or approval policy differ from the accredited ones
  (gpt-6-luna, high, read-only, never; Snapshot M probe 282c);
- config.toml at entry is not Snapshot M;
- the lateral branch or worktree changed;
- the output does not validate against the schema.

If the I-64 branch moves away from fa59bd82 before the review (for example, at the F4B close):
[COORDINATOR: confirm]
the review still targets ab0170b7; record the new tip; do not rebase, merge or cherry-pick.

The Architect must read at minimum (all at ab0170b7, with git show):
1. docs/initiatives/I-64-A-14.md
2. the Freeze V5, blob dc1924ff...
3. docs/initiatives/I-64-A-1.md .. I-64-A-13.md (A-7 entire)
4. docs/initiatives/I-64-implementation-clarifications.md (IC-01..IC-11)
5. docs/initiatives/I-64-f4b-push-back-completo-package.md, and
   docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md (OD)
6. docs/initiatives/I-64-owner-brief.txt
7. docs/INITIATIVE_LIFECYCLE.md (Sec.2, Sec.3, Sec.5..Sec.8) and docs/WORKFLOW.md (Sec.2, Sec.4.5, Sec.10, Sec.11)
8. the custodied sources: docs/automation/evidence/I-64-pilot/A-14/ (Owner decisions, replaneacion,
   prototype, v1..v2.3, independent reviews and dispositions, briefs, notes, sha256.txt)
9. this order, literal.
No product execution is needed.

Questions:
Q01..Q15 and the materiality assessment M-01..M-08, exactly as in the prepared prompt
(a14-architect-prompt.md):
Q01 materiality; Q02 authority; Q03 composition of "Actualizar todo"; Q04 window rectangle route;
Q05 Applies-to and A-7; Q06 reading rule of "pestana"; Q07 Clean draft life; Q08 D-PB-01;
Q09 RACKEDITAR and creation commands; Q10 tramos; Q11 clause deltas and test obligations;
Q12 non-reopening; Q13 versioning fidelity; Q14 new Owner decision; Q15 open or unverifiable path.

Return:
Verdict:
AGREED
or
CHANGES REQUIRED
or
BLOCKED - OWNER DECISION
Then:
* M-01..M-08 assessment (Q-C3);
* Owner-required assessment;
* REQUIRED findings, each with a stable ID;
* OPTIONAL findings;
* explicit statement whether the composition of "Actualizar todo" and the C-10 route are acceptable;
* explicit statement whether A-14 is suitable for Coordinator AGREED.
No implementation recommendations outside A-14's scope.

Budget:
One valid Architect review.
Up to two additional invocations only for transport failure or INVALID_OUTPUT.
A valid CHANGES REQUIRED consumes the review and returns to the Coordinator.
A-14 is append-only: the correction is A-15 (precedent A-12 -> A-13). Do not self-correct without new
Coordinator authority.

Custody:
the review goes to docs/automation/evidence/I-64-pilot/A-14-review/<RunId>/ on the lateral branch only
(the A-13-review pattern: prompt.md, architect-review.schema.json, output.json, review-record.json,
custody-manifest.json, the entry and exit snapshots); events, transcripts and stderr only by SHA-256;
redact local paths. Commit on the lateral branch; push without force; exact-SHA CI.

After the review:
do not request the Owner signature; do not activate A-14.
Stop at:
COORDINATOR_A14_ARCHITECT_REVIEW_REQUIRED
Return:
* Architect RunId;
* exact model and effort, effective;
* verdict;
* REQUIRED findings;
* OPTIONAL findings;
* M-01..M-08 result;
* Owner-required result;
* whether the Architect says A-14 is ready for Coordinator AGREED.

Do NOT touch the I-64 branch, its worktree or main.
Do NOT run AutoCAD.
Do NOT start any phase of A-14.
```

## Notas para el Coordinator

- **Es la primera vez que esta celda corre fuera del worktree de I-64.** Las corridas de F4B con gpt-6-luna y las revisiones de A-8..A-13
  corrieron todas en el worktree de I-64, que ahora no se toca. En `config.toml`, ese worktree no figura con su ruta exacta, pero sí
  una carpeta que lo contiene (la del perfil del usuario). El worktree lateral no figura de ninguna de las dos formas. No se sabe si
  Codex aplica la confianza de un proyecto a sus subcarpetas, así que el contexto efectivo puede cambiar con el directorio.
  - Por eso la orden convierte en `INVALID_OUTPUT` cualquier diferencia de modelo, effort, sandbox o política de aprobación efectivos,
    y cualquier cambio de `config.toml` durante la corrida.
  - Si el Coordinator prefiere una sonda neutra en el worktree lateral antes de la revisión, puede decidirlo junto con el
    `config.toml` nuevo.
- **El lanzador:** se copia la línea acreditada de gpt-6-luna de F4B, sin `-c allow_login_shell=false`. Las revisiones de A-8..A-13
  lo llevaban, pero con otro binario y otra config. No se midió con este binario ni con la L, y añadirlo sería una variación sin sonda.
- **El presupuesto, las dos invocaciones extra por fallo de transporte y la parada final** copian la orden de A-7.
