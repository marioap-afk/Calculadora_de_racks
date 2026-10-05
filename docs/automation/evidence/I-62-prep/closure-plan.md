# I-62 — Plan exacto de cierre documental e integración (preparación; nada de esto se escribe ahora)

> **Preparación.** Fuente: WORKFLOW §2, §4, §7 y §11.3-§11.6; LIFECYCLE §8; Proposal V14 §14, §17 y Anexo E.7. Ningún archivo de cierre se modifica hasta
> la orden del cierre documental, después de `FINAL_CANDIDATE_SHA`. Complementa `f7-ready-closure.md` (borrador factual de FOUNDATIONS). MEASURED al
> 2026-10-04: `origin/main` = `819955d6`.

## 1. Contenido de cierre, con texto exacto

| Superficie | Cambio | Texto o regla |
|---|---|---|
| `docs/FOUNDATIONS.md` | entrada nueva (descriptiva, subordinada a ADR/Freeze) | el borrador de `f7-ready-closure.md` §1.1, reescrito con los hechos finales: SHAs de cierre de F1-F7, `I62_EFFECTIVE_SHA`, A-n acordadas, OD resueltas y limitaciones decididas. Sin normas nuevas |
| `docs/adr/README.md` | fila nueva, en orden numérico (formato de las filas 0045 y 0046) | número `0048` enlazado a `0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md`; título «Ejecución delegada portable: roles, binding por capacidad y autoverificación del Principal (sucesor parcial de ADR-0046)»; estado tras OD-1 («aceptado» o «propuesto») |
| `docs/adr/0048-…md` | campo `Estado` | «aceptado» solo si OD-1 lo acepta; si no, sigue «propuesto» |
| `docs/HANDOFF.md` | §1 (resumen) y §4 (siguiente acción) | «I-62 integrada (merge `<MERGE_SHA>`, tag `integration/I-62`): I62 rige desde `I62_EFFECTIVE_SHA` para las unidades I62_DELEGATED; I-61 sigue para las unidades I61». Sin copiar normas |
| `docs/ROADMAP.md` | fila de I-62 (línea ~500 en `main`) | estado «integrada <fecha>» y referencia al tag, en la ventana de WORKFLOW §2 |
| contrato `docs/initiatives/I-62-portabilidad-coordinador-principal.md` | `status` y §8 | `integrated`; `ov_assignment_ref` |
| estado `docs/automation/state/I-62.yml` | `/v1` | `state: completed`; `current_phase: integrated`; `last_evidence_commit` |
| `docs/ideas-futuras.md` | disposición de `f7-ready-closure.md` §1.2 | solo lo diferido con autoridad |

## 2. Orden de la integración (WORKFLOW §11.4-§11.6; E.7)

1. **READY-01..09** en orden (`ready-candidate.md`) → `FINAL_CANDIDATE_SHA`; OV-I62-01..06 sobre ese SHA.
2. **Cierre documental** en la rama (WORKFLOW §4.5.4: el cierre escribe `integrada` antes del merge), en un commit `CLOSURE_SHA` que solo toca las superficies de §1.
3. **Rebase final** sobre `origin/main` si avanzó; si cambia el SHA, se repiten READY-02..09 (LIFECYCLE §8).
4. **Merge local** `--no-ff` en una copia de `main` (no publicada):
   - comprobar que la rama contiene **un único** commit con el trailer `Agent-Protocol-Normative: I-62` (el de MaterializationClose de F4) y que su primer padre no lo alcanza;
   - **regenerar el mapa de cláusulas** sobre `merge^1 → merge` y compararlo con el materializado (C-20b; `f4/compat-proto` es el prototipo); una diferencia
     es corrección (vuelve a F4) o A-n;
   - Core Full sobre el merge local.
5. **PRE** (WORKFLOW §11.3; precedente `8a021fb6`), en el cuerpo del merge: comando `git ls-remote --heads origin` con su salida cruda, fecha UTC, `main`
   observado y la tabla «Derived formal claim table» (`Initiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition`). Las
   unidades activas de PRE son ANTERIORES (I61 de por vida). **El `Claim-Id` de I-62 no figura en PRE.**
6. **Push** de `main` (merge efectivo = `I62_EFFECTIVE_SHA`, que rige al aceptarse el push; E.7: punto de entrada, §16.13, punteros, mapa y esquema en el
   **mismo** merge, sin activación parcial).
7. **POST** y **tag** `integration/I-62`: POST (`git ls-remote --heads origin` tras el push), ruta y blob del mapa en EFF, `FINAL_CANDIDATE_SHA`, `CLOSURE_SHA`,
   `MERGE_SHA` y la CI posterior al merge (precedentes `integration/I-56` e `integration/I-61`).
8. **Recibo** en la evidencia de I-62 y estado final.

## 3. Conflictos previsibles con las hermanas (MEASURED al 2026-10-04)

| Rama | Punta | Superficies de cierre que toca | Serialización |
|---|---|---|---|
| I-52 `feature/rackmirror-espejo-semantico` | `fb6b5648` (merge-base `95690c28`) | `docs/adr/README.md` (fila 0036), `docs/HANDOFF.md`, `docs/ROADMAP.md` | si integra antes, I-62 rebasa y añade la fila 0048 detrás de la 0036; si después, I-52 rebasa |
| I-63 `architecture/parametros-calculados-resumen-proyecto` | `fdd4b651` | `docs/ROADMAP.md` (su fila) | acuse de ventana; sin solape de líneas |
| I-64 `architecture/workspace-persistente-rackcad` | `39b45f36` | `docs/ROADMAP.md` (su fila); en su cierre, la fila 0047 del índice ADR | la 0047 precede a la 0048: quien integre después rebasa |
| I-62 | `bf7b0d9c` | AUTOMATION_PLAN, LIFECYCLE, agent-execution, ROADMAP (fila del bootstrap) | AUTOMATION_PLAN y LIFECYCLE son autoridades calientes (WORKFLOW §7): ninguna hermana las toca hoy; DC-07 antes de cada escritura |

**Regla de la evidencia por SHA:** una integración de una hermana antes que I-62 cambia `origin/main` → READY-04 de I-62 rebasa → SHA nuevo → READY-02..09 y la
conformidad de READY-06 se repiten (LIFECYCLE §9). Previsión: si I-52, I-63 o I-64 integran antes, el mapa regenerado en el merge local debe seguir igual
(sus cambios no tocan las superficies del mapa; si las tocaran, C-20b lo detectaría).
