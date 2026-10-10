# I-62 F6 — Caracterización de MAP_INVALID en el fixture (decisiones §66, punto 3)

Análisis de solo lectura. Fuentes: el origen del fixture `D:\r62-fixture\fixture-origin.git` (sin escritura) y el worktree de I-62
(`%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal`, sin escritura). Las órdenes y sus salidas literales están en
[work/characterization_git.txt](work/characterization_git.txt) y [work/provenance.txt](work/provenance.txt). La evaluación mecánica la hace
[mv_check.py](mv_check.py); su salida está en [mv_current_main.txt](mv_current_main.txt) y [mv_current_main.json](mv_current_main.json).

## 1. Causa exacta

El fixture se construyó con `build_fixture.py` (`I-62-F6/fixture/build_fixture.py`). Ese script:

1. sembró **F_seed** (`930288c5`) con las autoridades de **MC_I62** (`6f0187cb`, la punta de F4), es decir, con los `EffBlob` del mapa. Debía usar
   la base I-61 del mapa: los `BaseBlob` de `bb0d5522`, el `origin/main` de RackCad en F4;
2. hizo que **F_norm** (`a9f6c929`, el único commit con el trailer) cambiara solo `FIXTURE-MANIFEST.json`;
3. dejó fuera de `SEED_PATHS` dos archivos del mapa: `docs/adr/0048-…md` (ADDED) y `docs/initiatives/PROMPT_TEMPLATES.md` (MODIFIED). Ninguno de los dos
   existe en el fixture.

Por eso `git diff EFF^1 EFF` no contiene ninguna superficie, y el mapa (blob `4d49d3e1`, copiado byte a byte de RackCad) describe un cambio que el
fixture no reproduce. **El mapa no tiene defecto.** Es la derivación exacta de RackCad `bb0d5522 → 6f0187cb` (C-20b EQUAL; C-20c VALID). El defecto
está en el par EFF^1/EFF del fixture.

## 2. Objetos de la activación (salidas de git, resumidas)

| Objeto | SHA | Padres | Asunto / cuerpo |
|---|---|---|---|
| EFF = `main` | `fbe25347799e5b801ef708454212335537448bd1` | `930288c5…`, `a9f6c929…` | «Merge fixture/i62-norm: TEST-ACTIVATION de I-62 en el fixture». El cuerpo termina con `Derived formal claim table:` y la cabecera `Initiative \| ref \| tip \| original claim commit \| Claim-Id \| temporal evidence \| transition`, sin filas |
| EFF^1 | `930288c51ab1b5dbd370c8ebf9206d78670b89a0` | (raíz) | «F_seed: autoridades de MC_I62 (byte a byte) y archivos FIXTURE_LOCAL»; 47 archivos |
| EFF^2 = `fixture/i62-norm` | `a9f6c929e74ef92edfee557c3572b090f48bc318` | `930288c5…` | «F_norm: …(TEST-ACTIVATION)», con la única línea de trailer `Agent-Protocol-Normative: I-62` |
| tag | `refs/tags/test-activation/I-62` (objeto `f3876237`) → EFF | | |

Otras refs, que no se tocan: `fx/u1` = `95bdc29d`, `fx/u1-fx05` = `13654f96`, `fx/u1-fx05-r2` = `6b51b378` y `ci/smoke` = `7cf79ffc` (en el origen
local).

| Orden | Resultado |
|---|---|
| `git diff --name-status 930288c fbe2534` | `M FIXTURE-MANIFEST.json` (único; `--stat`: +6/−1, `"Activation": null` → objeto TEST-ACTIVATION) |
| `git diff --name-status 930288c a9f6c92` | `M FIXTURE-MANIFEST.json` |
| `git diff --name-status a9f6c92 fbe2534` | vacío (árbol de EFF = árbol de EFF^2) |
| `git log --all --grep='^Agent-Protocol-Normative: I-62$'` | solo `a9f6c92` (trailer único) |
| `git ls-tree -r fbe2534 -- docs/adr docs/initiatives docs/FOUNDATIONS.md` | 0 archivos |

## 3. El mapa (en EFF, blob `4d49d3e1a63d2b6128c9d9eeac473156a421571a`; esquema `6b54dda9…`)

- **`Surfaces`** (9, en el orden de 16.13): `AGENTS.md`, `CLAUDE.md`, `docs/AUTOMATION_PLAN.md`, `docs/FOUNDATIONS.md`, `docs/INITIATIVE_LIFECYCLE.md`,
  `docs/WORKFLOW.md`, `docs/adr/`, `docs/automation/agent-execution/`, `docs/initiatives/PROMPT_TEMPLATES.md`.
- **`Files`** (31): 6 MODIFIED, 24 ADDED y 1 ENTRY (el esquema).
- **`Entries`** (70): 15 MODIFIED, 53 ADDED, 2 ENTRY y 0 REMOVED.
  - Por archivo: AUTOMATION_PLAN 23, agent-execution/README 35, WORKFLOW 4, INITIATIVE_LIFECYCLE 3, routing 3 y PROMPT_TEMPLATES 2.
  - Las ENTRY son `### 16.13 Compatibilidad de protocolos de ejecución delegada` y `## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)`.

Blobs de cada entrada de `Files` (`ae/` = `docs/automation/agent-execution/`; «—» = ausente o `null`). La última columna es el candidato de la
reconstrucción (sección 4 de [reconstruction-plan.md](reconstruction-plan.md)):

| # | Ruta | Clase | BaseBlob | EffBlob | fixture EFF^1 `930288c5` | fixture EFF `fbe25347` | MV-4 literal | candidato EFF'^1 / EFF' |
|---|---|---|---|---|---|---|---|---|
| 1 | `docs/AUTOMATION_PLAN.md` | MODIFIED | 07b97bd3 | f525cb1e | f525cb1e (=Eff) | f525cb1e | FAIL | 07b97bd3 / f525cb1e ✓ |
| 2 | `docs/INITIATIVE_LIFECYCLE.md` | MODIFIED | 6415c33b | f19896a8 | f19896a8 (=Eff) | f19896a8 | FAIL | 6415c33b / f19896a8 ✓ |
| 3 | `docs/WORKFLOW.md` | MODIFIED | 884ed305 | 446c32ba | 446c32ba (=Eff) | 446c32ba | FAIL | 884ed305 / 446c32ba ✓ |
| 4 | `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md` | ADDED | — | e1bd8d91 | — | — | FAIL | — / e1bd8d91 ✓ |
| 5 | `ae/README.md` | MODIFIED | 64ae952b | 592dcfd4 | 592dcfd4 (=Eff) | 592dcfd4 | FAIL | 64ae952b / 592dcfd4 ✓ |
| 6 | `ae/adapters/claude-cli.md` | ADDED | — | ae570380 | ae570380 (=Eff) | ae570380 | pass | — / ae570380 ✓ |
| 7 | `ae/adapters/claude-desktop-session.md` | ADDED | — | 7f459235 | 7f459235 (=Eff) | 7f459235 | pass | — / 7f459235 ✓ |
| 8 | `ae/adapters/claude-subagent.md` | ADDED | — | e10926c0 | e10926c0 (=Eff) | e10926c0 | pass | — / e10926c0 ✓ |
| 9 | `ae/adapters/codex-cli.md` | ADDED | — | 155f3469 | 155f3469 (=Eff) | 155f3469 | pass | — / 155f3469 ✓ |
| 10 | `ae/adapters/codex-desktop-session.md` | ADDED | — | 02d6c33d | 02d6c33d (=Eff) | 02d6c33d | pass | — / 02d6c33d ✓ |
| 11 | `ae/compatibility/clause-map.schema.json` | ENTRY | — | 6b54dda9 | 6b54dda9 (=Eff) | 6b54dda9 | pass | — / 6b54dda9 ✓ |
| 12 | `ae/routing.md` | MODIFIED | 82ca9b38 | bba08fc4 | bba08fc4 (=Eff) | bba08fc4 | FAIL | 82ca9b38 / bba08fc4 ✓ |
| 13 | `ae/schemas/adapters/claude-cli.facts.v1.schema.json` | ADDED | — | 2298c376 | 2298c376 (=Eff) | 2298c376 | pass | — / 2298c376 ✓ |
| 14 | `ae/schemas/adapters/claude-desktop-session.facts.v1.schema.json` | ADDED | — | f246717f | f246717f (=Eff) | f246717f | pass | — / f246717f ✓ |
| 15 | `ae/schemas/adapters/claude-subagent.facts.v1.schema.json` | ADDED | — | 3706be15 | 3706be15 (=Eff) | 3706be15 | pass | — / 3706be15 ✓ |
| 16 | `ae/schemas/adapters/codex-cli.facts.v1.schema.json` | ADDED | — | 3d1b7478 | 3d1b7478 (=Eff) | 3d1b7478 | pass | — / 3d1b7478 ✓ |
| 17 | `ae/schemas/adapters/codex-desktop-session.facts.v1.schema.json` | ADDED | — | bdf5c491 | bdf5c491 (=Eff) | bdf5c491 | pass | — / bdf5c491 ✓ |
| 18 | `ae/schemas/architect-review-result.v1.schema.json` | ADDED | — | e7f5747b | e7f5747b (=Eff) | e7f5747b | pass | — / e7f5747b ✓ |
| 19 | `ae/schemas/automation-state.v2.schema.json` | ADDED | — | b1105dc1 | b1105dc1 (=Eff) | b1105dc1 | pass | — / b1105dc1 ✓ |
| 20 | `ae/schemas/binding.v1.schema.json` | ADDED | — | 13501476 | 13501476 (=Eff) | 13501476 | pass | — / 13501476 ✓ |
| 21 | `ae/schemas/controller-verification.v2.schema.json` | ADDED | — | e1faa9ec | e1faa9ec (=Eff) | e1faa9ec | pass | — / e1faa9ec ✓ |
| 22 | `ae/schemas/delegation.v2.schema.json` | ADDED | — | dfdbe461 | dfdbe461 (=Eff) | dfdbe461 | pass | — / dfdbe461 ✓ |
| 23 | `ae/schemas/gate-contract.v2.schema.json` | ADDED | — | f644bb19 | f644bb19 (=Eff) | f644bb19 | pass | — / f644bb19 ✓ |
| 24 | `ae/schemas/input-closure.v1.schema.json` | ADDED | — | 6bdf7184 | 6bdf7184 (=Eff) | 6bdf7184 | pass | — / 6bdf7184 ✓ |
| 25 | `ae/schemas/input-fidelity.v1.schema.json` | ADDED | — | e54990fc | e54990fc (=Eff) | e54990fc | pass | — / e54990fc ✓ |
| 26 | `ae/schemas/normative-dependency-manifest.v1.schema.json` | ADDED | — | 7ff2127d | 7ff2127d (=Eff) | 7ff2127d | pass | — / 7ff2127d ✓ |
| 27 | `ae/schemas/preflight.v1.schema.json` | ADDED | — | 6a054079 | 6a054079 (=Eff) | 6a054079 | pass | — / 6a054079 ✓ |
| 28 | `ae/schemas/relay-record.v2.schema.json` | ADDED | — | dca5b29c | dca5b29c (=Eff) | dca5b29c | pass | — / dca5b29c ✓ |
| 29 | `ae/schemas/reviewer-result.v1.schema.json` | ADDED | — | a74288eb | a74288eb (=Eff) | a74288eb | pass | — / a74288eb ✓ |
| 30 | `ae/schemas/role-invocation.v1.schema.json` | ADDED | — | d54a7ae7 | d54a7ae7 (=Eff) | d54a7ae7 | pass | — / d54a7ae7 ✓ |
| 31 | `docs/initiatives/PROMPT_TEMPLATES.md` | MODIFIED | 0e3ed262 | 384d0b0c | — | — | FAIL | 0e3ed262 / 384d0b0c ✓ |

**Presencia en el fixture actual:**

- 29 de las 31 rutas existen ya en EFF^1 con su `EffBlob`.
- `docs/adr/0048-…md` y `PROMPT_TEMPLATES.md` no existen en ninguna revisión del fixture.
- El mapa también está ya en EFF^1.
- Fuera de `Files`, también están bajo superficies `AGENTS.md` y `CLAUDE.md` (FIXTURE_LOCAL, iguales en EFF^1 y EFF), `model-catalog.md` (`166d978d`),
  `prompting-guide.md` (`79754c72`) y los cinco esquemas `/v1`. Todos son iguales en EFF^1 y EFF, y también en RackCad `bb0d5522` y `6f0187cb`.

## 4. Procedencia de `BaseBlob` y `EffBlob` en RackCad ([work/provenance.txt](work/provenance.txt))

| Hecho | Resultado |
|---|---|
| Cada `BaseBlob` MODIFIED es el contenido de su ruta en | **`bb0d5522`** («Merge I-63…», `origin/main` de RackCad durante F4; `merge-base bb0d5522 6f0187cb` = `bb0d5522`). 6/6 |
| Cada `EffBlob` es el contenido de su ruta en | **`6f0187cb`** («I-62 F4-H: regresión de A-1…», punta de F4 = MC_I62, `SourceCommit` del manifiesto del fixture). 31/31; también en la punta actual del worktree `299bd327` |
| ADDED/ENTRY en `bb0d5522` | ausentes (25/25) |
| Commits de introducción (`git log --find-object`) | `BaseBlob` de AP y README: `6513777f` (I-61 READY-06). WORKFLOW: `4da82667` (I-61 G2). routing y PROMPT_TEMPLATES: `7b8662c5` (I-61 G2). INITIATIVE_LIFECYCLE: `7e9073e7`. `EffBlob`: commits de F1-F4 de I-62 (`02a81831`, `ffb509e8`, `42115503`, `3078538b`, `1566341a`, `6f0187cb`) |
| Validación en F4 | **C-20b** `compat/clause_map.py check bb0d5522 6f0187cb` = EQUAL (MV-2..MV-6; 31 archivos, 70 entradas; `c20b-result.json`, `MapBlob` `4d49d3e1`). **C-20c** `compat/c20c.py`: clon desechable; EFF local `f1600fac` = merge `--no-ff` en `bb0d5522` (M0) de `6f0187cb` más un commit vacío local con el trailer; `MapValidation` = «VALID (MV-1..MV-7)» (`c20c-result.json`, `Scenario`) |

**Conclusión.** El mapa se derivó y se validó contra el par **EFF^1 = `bb0d5522`** y **árbol de EFF = árbol de `6f0187cb`** (más un commit vacío con el
trailer). El fixture tenía que reproducir ese par: base I-61 en el primer padre y las autoridades de MC_I62 solo por el segundo padre.

## 5. MV-1..MV-7 sobre el fixture actual (MainSha_eval = `main` = `fbe25347`)

Previos, en orden de `Evaluate`:

- `Derive` da EFF = `fbe25347` (EFF^1 `930288c5`, EFF^2 `a9f6c929`), con un único trailer alcanzable.
- E2 |X| = 1 y E3 |R| = 1. Las rutas del mapa, del esquema y de `Surfaces` descubiertas del texto de 16.13 coinciden con los literales.
- La tabla PRE está bien formada, con 0 filas.

| Id | Resultado | Elementos exactos que fallan |
|---|---|---|
| MV-1 | pass | el mapa existe en EFF, es JSON y valida contra el esquema en EFF (validador del subconjunto 2020-12 con fallo cerrado) |
| MV-2 | pass | `Surfaces` = la lista cerrada |
| MV-3 | **FAIL (31)** | el conjunto de rutas cambiadas bajo las superficies es **vacío** (`FIXTURE-MANIFEST.json` no es superficie); las 31 rutas de `Files` están «listadas sin cambiar entre EFF^1 y EFF» |
| MV-4 | **FAIL (7, literal)** | 6 MODIFIED con `BaseBlob` ≠ blob en EFF^1: AP (`07b97bd3` frente a `f525cb1e`), INITIATIVE_LIFECYCLE (`6415c33b` / `f19896a8`), WORKFLOW (`884ed305` / `446c32ba`), README (`64ae952b` / `592dcfd4`), routing (`82ca9b38` / `bba08fc4`) y PROMPT_TEMPLATES (`0e3ed262` / ausente; además, `EffBlob` `384d0b0c` frente a ausente en EFF). A esos 6 se suma ADR-0048 ADDED, con `EffBlob` `e1bd8d91` y ausente en EFF. Además (informativo, no entra en el recuento literal): los otros 24 ADDED/ENTRY ya existen en EFF^1, lo que contradice la derivación de FileKind (R61 b, «no existía en EFF^1») |
| MV-5 | pass | `Entries` únicas por (`Path`, `Section`), todas de archivos MODIFIED |
| MV-6 | **FAIL (69)** | los 5 Markdown MODIFIED presentes son idénticos en EFF^1 y EFF: derivación vacía. Hay 68 entradas «listadas y no derivadas» (AP 23, README 35, WORKFLOW 4, IL 3, routing 3), incluidas las dos ENTRY, y PROMPT_TEMPLATES «ausente en EFF^1» |
| MV-7 | **FAIL (2)** | «16.3 en EFF sin el puntero ≠ 16.3 en EFF^1» y «16.3 en EFF^1 ya lleva el puntero». Sí se cumplen el punto de entrada único que nombra 16.13 por su línea exacta, 16.13 única y los punteros en §16 y en 16.3 en EFF |

**Veredicto: MAP_INVALID** (MV-3, MV-4, MV-6, MV-7). Coincide con el hallazgo de la evidencia §119 y del kit A4-1 (§8 F-MAP-1). El MV-4 literal da 7,
el mismo número del kit.

**Contraste independiente:** el comprobador de F4 (`compat/clause_map.py check 930288c5 fbe25347`, solo lectura) da
`["MV-3", "MV-4" ×31 (comparación por clase+blobs), "MV-6"]`, con 0 archivos y 0 entradas derivadas ([f4_clause_map_crosscheck.txt](f4_clause_map_crosscheck.txt)).

## 6. Consecuencias y alcance

- **Classify no lee el mapa.** `Classify(FX-U1)` en `fx/u1` (`95bdc29d`) frente a EFF `fbe25347` = **I62**: el estado `/v2` tiene `effective_sha`
  `fbe25347`, el blob de la decisión `b7cff537` está presente y tiene los marcadores.
- **El fallo está en Resolve.** Toda evaluación de un contrato de ejecución delegada después de EFF en el fixture (emisión, aceptación A1-A8,
  `Authority` de la verificación, decisión sobre un VERIFIED) llega a Resolve, paso 2, y da **MAP_INVALID → `Authority` fail → STOP (S-12; P-15)**.
  Por eso FX-02 no puede llegar a un Q7 VERIFIED en este linaje, aunque se resolviera la elegibilidad del Controller.
- **El linaje r1 no tiene reparación hacia delante.** Para toda `main` que descienda de `fbe25347`:
  - `Derive` sigue dando `fbe25347`;
  - si se añade otro commit con el trailer, da ACTIVATION_INVALID por duplicado (control negativo N-4);
  - el mapa, EFF^1 y EFF se leen en EFF, que es inmutable.

  Solo un linaje cuya historia first-parent no alcance `a9f6c929`/`fbe25347` puede tener un EFF válido. Cambiar el resolver o las autoridades está
  prohibido (§66.3).
- **Ninguna medición anterior ejecutó la cadena del mapa.** C-22, el contrato de T1, FX-01, FX-04a, FX-05 y las sondas no aplicaron
  Validate/Evaluate/Resolve. El contrato de T1 se validó con `Test-Json` y la coincidencia de blobs (`FX-U1-chain/chain.json`). La sonda A4-1 dejó la
  cadena del mapa «no ejecutada». El único cálculo es la referencia del kit (§119). Ver [effects.md](effects.md).
