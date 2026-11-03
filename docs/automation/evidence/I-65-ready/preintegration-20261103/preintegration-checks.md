# I-65: comprobaciones de preintegración (solo lectura)

- **Fecha:** 2026-11-03, a las 15:2xZ, con `git fetch origin` fresco.
- **Disposición:** «W-R2 Acceptance / Final Owner Validation Preparation» (decisiones §124).
- **Alcance:** no se hizo ningún merge, rebase ni push a `main`, ni cambio de ramas o del checkout principal. `git merge-tree --write-tree` solo escribe objetos de árbol sin referencia en la base de objetos: no mueve ninguna rama.

## 1. `origin/main` y la rama I-65

| Comprobación | Resultado |
|---|---|
| `origin/main` | `bb0d5522e8411f66a51fdfb3f1f0d0514b737453` (2026-10-04T22:21:07-06:00, «Merge I-63»); no ha avanzado desde la declaración del Candidato |
| Rama remota de I-65 | `fad6f937b2db8291bf07b08b2069330137b438ed` = `HEAD` local |
| merge-base(`origin/main`, I-65) | `bb0d5522` = `origin/main`: **0 commits detrás**, 174 por delante |
| Archivos distintos de `main` | 1599: `docs/automation` 1431; `tests/RackCad.Tests` 64; `docs/initiatives` 49; `src/RackCad.Application` 38; `tools/RackCad.StructuralAnalysis.RamTool` 6; `src/RackCad.Plugin` 2; `docs/guias` 2; `docs/adr` 1 (ADR-0050); `docs/ROADMAP.md`; `deploy/verify-bundle.ps1`; `deploy/test-verify-bundle.ps1`; `assets/structural-analysis` 1; `RackCad.sln`; `README.md` |
| Merge simulado `origin/main` + I-65 | Sin conflictos. Árbol resultante `d46b798d…` = árbol de `HEAD`: la integración no mezcla nada nuevo de `main` |
| Cambios después del Candidato | `git diff --name-only 4a03b65b..HEAD`: 325 archivos, todos en `docs/automation`; **0 fuera de `docs/`** |
| Checkout principal local | `main` local en `95690c28` (detrás de `origin/main`) y limpio. La integración por plomería no lo usa ni lo toca |

## 2. Dependencias con otras iniciativas

| Iniciativa | Rama (punta) | Estado declarado | Base | Archivos en común con I-65 | Merge simulado con I-65 |
|---|---|---|---|---|---|
| I-62 | `architecture/portabilidad-coordinador-principal` (`a08afecc`, 2026-10-30) | E1, `review-ready`, puerta `owner-decision` | `bb0d5522` (267 commits por delante) | `docs/ROADMAP.md` | **limpio** |
| I-64 | `architecture/workspace-persistente-rackcad` (`09f2f1c3`, 2026-10-13) | F1, `waiting` | `bb0d5522` (79) | `docs/ROADMAP.md`, `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs`, `tests/RackCad.Tests/SelectiveEditorOpenTests.cs` | **conflicto en los dos censos de comandos** |
| I-66 | `fix/placedclone-copia-anotaciones` (`5d52e5d8`, 2026-10-29) | (rama de I-66) | `bb0d5522` (16) | `docs/ROADMAP.md` | **limpio** |
| I-52 | `feature/rackmirror-espejo-semantico` (`fb6b5648`, 2026-10-02) | fuera del alcance de esta comprobación | `95690c28` (161 por delante, 90 detrás) | — | — |

- **Conflicto con I-64.** Las dos ramas suben el censo de 37 a 38 y añaden un nombre distinto: `RACKRAMPAQUETE` en I-65 y `RACKPANEL` en I-64. Es el conflicto conocido (CD-I65-F5-60). Con R-D1, I-65 integra antes e **I-64 reconcilia después a 39** al integrar.
- **Orden de integración.** Si cualquier rama se integra en `main` antes que I-65, I-65 necesita rebase. Eso produce un SHA nuevo, deja sin valor la identidad de la evidencia (AGENTS, «Reutilización de evidencia») y obliga a otro Candidato con READY desde 02 y nuevas ventanas W-R1 y W-R2.

## 3. Números de ADR

| Rama | ADR 0046-0059 presentes |
|---|---|
| `main` | 0046, 0049 |
| I-62 | 0046, 0048, 0049 |
| I-64 | 0046, 0047, 0049 |
| I-66 | 0046, 0049 |
| I-65 | 0046, 0049, **0050** |

**0050 sigue libre fuera de I-65** (A65-FC-09).

## 4. FOUNDATIONS y ADR-0050

- **ADR-0050:** «Estado: aceptado» (Owner, 2026-10-11). Blob `c18def42` igual en el Candidato y en `HEAD`, sin commits desde su aceptación.
- **`docs/FOUNDATIONS.md`:** blob `748c74ba`, igual en `main`, en el Candidato y en `HEAD`. Las entradas de I-65 siguen **redactadas y sin publicar** en `docs/initiatives/I-65-foundations-entry.md` (blob `75276227`, sin cambios desde el Candidato). Se publican con `Status: STABLE` en el commit de cierre de la integración (LIFECYCLE §4.1; WORKFLOW §11.4; A65-FC-08).
- **Freeze y A-n:** Freeze `docs/initiatives/I-65-proposal-v3.1.md`, blob `4e6e1feb` sin cambios; `freeze_delta_ref` null; `amendment_refs` [].
- **Tabla:** blob `d088e2a9`, sin cambios.

## 5. SDK

`dotnet --version` del SDK del usuario en el worktree: **8.0.423**, el mismo de READY-05.
