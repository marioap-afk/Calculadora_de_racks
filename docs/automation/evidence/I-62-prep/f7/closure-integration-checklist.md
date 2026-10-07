# I-62 — Lista de cierre documental e integración serializada (staging; nada de esto se escribe ahora)

> **Preparación de staging.** Orden nocturna, decisiones §54 (fila «F7 y READY»: «listas de READY, cierre e integración … sin cierre de HANDOFF/ROADMAP,
> sin FINAL_CANDIDATE ni integración»). Autoridades: WORKFLOW §2 (tres momentos de ROADMAP), §4 paso 5 (sub-pasos 1-7, citados por §11.6 como «4.5.6/4.5.7»),
> §7 (archivos calientes: de lo que toca el cierre, solo `docs/HANDOFF.md` y `docs/ROADMAP.md`), §11.3-§11.6; LIFECYCLE §8; AGENTS (tabla Full: «Cierre documental de iniciativa»); Proposal V14 §14.1, §17 (filas «Cierre
> documental» e «Integración»), Anexo E.1, E.4, E.5 y E.7; AUTOMATION_PLAN 16.13, 16.14 y 16.30. Revisa y corrige `I-62-prep/closure-plan.md` (2026-10-04/05;
> correcciones en §9). Precedentes de formato: cierre de I-63 `3c5019bf` y de I-61 `e78d2332`; activación de I-56 `8a021fb6` (PRE) y su marcador `afd1077c`.

## 1. Precondiciones (todas antes de escribir el cierre)

- [ ] READY-01..09 en orden y `FINAL_CANDIDATE_SHA` declarado (`ready-dry-run.md`); evidencia exact-SHA del Candidato (Core Full, UI Full, builds Debug,
  CI push 4/4, cobertura del Candidato) y OV-I62-01..06 sobre ese SHA (`ov-packets.md`).
- [ ] OD-1 decidida (V14 §18) y momento de la edición de ADR-0048 fijado (Q2).
- [ ] Disposición del Coordinator sobre Q1 (cierre documental y mapa frente a `MC_I62`), Q3 (forma de la entrada de FOUNDATIONS) y Q15 (commit único con
  el trailer).
- [ ] Acuses de ventana de `docs/ROADMAP.md` de las hermanas activas (estrategia de coordinación del contrato; precedente: cierre de I-63 con acuses de
  I-62, I-64 e I-52) y DC-07 inmediatamente antes de escribir.

## 2. Contenido del commit de cierre documental (`CLOSURE_SHA`; WORKFLOW §4 paso 5.4 y §11.4)

| Superficie | Cambio | Texto o regla exacta | ¿Superficie de 16.13? |
|---|---|---|---|
| `docs/FOUNDATIONS.md` | entrada factual según Q3: Forma R (reescribir «Agent Execution Protocol» conservando los hechos I61 vigentes) o Forma N (entrada nueva; la I61 queda `STABLE` o `SUPERSEDED -> <entrada>`) | `foundations-draft.md` §2 (R) o §2.1 (N) con los `[PENDIENTE]` resueltos; una línea por campo; `Last changed by: I-62`; sin normas nuevas (FOUNDATIONS «Estado normativo») | **sí** (E.5 lo prevé: «entrada (cierre documental; descriptiva)») |
| `docs/adr/README.md` | fila nueva | `\| [0048](0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md) \| Ejecución delegada portable: roles independientes del proveedor, binding por capacidad acreditada y autoverificación del Principal (sucesor parcial de ADR-0046) \| <aceptado \| propuesto> \|` (título = H1 del ADR; sufijo como la fila 0049 de I-63). **Posición:** hoy el índice termina `0045, 0044, 0046, 0049`; la 0048 va entre la 0046 y la 0049; si I-64 integra antes, detrás de su 0047. La fila de ADR-0046 no cambia (precedente: la 0043 sigue «aceptado» con la 0049 como sucesor parcial) | **sí** (E.5: «índice (cierre documental)») |
| `docs/adr/0048-…md` | `Estado` | «aceptado» solo si OD-1 lo acepta; si no, «propuesto». Si se hace aquí y no en READY-03, ver Q2 | **sí**; cambia el blob `e1bd8d91` listado en el mapa (C-20a lo detecta) |
| `docs/automation/agent-execution/compatibility/I62-clause-map.json` | regenerar | `python docs/automation/evidence/I-62-F4/compat/clause_map.py write <EFF^1 previsto>` tras preparar el árbol; añade `docs/FOUNDATIONS.md` y `docs/adr/README.md` como MODIFIED (MV-3) y, si cambia, el `EffBlob` de ADR-0048 (MV-4). **No está en `closure-plan.md`**. Sin regenerar, falla la C-20b (MV-3/MV-4) del cierre y del merge local. La C-20a (comprobación EFFBLOB de `tests/RackCad.Tests/I62/CompatibilityGuard.cs`, que recorre solo los `Files` ya listados en el mapa; guarda `I62_C20a_ASurfaceChangedWithoutRegeneratingTheMapIsDetectedByItsEffBlob`) falla además solo si cambia ADR-0048, única de estas superficies que ya es fila del mapa; FOUNDATIONS y el índice ADR no lo son | **sí** (`agent-execution/`; además `COPIED` en el manifiesto del fixture, blob `4d49d3e1`) → Q1 |
| `docs/HANDOFF.md` | resumen y siguiente acción | «I-62 integrada (<fecha>): desde `I62_EFFECTIVE_SHA`, el resolver de compatibilidad (AUTOMATION_PLAN §16.13, punto de entrada WORKFLOW §12) rige la lectura de autoridades de toda unidad y las partes I62 rigen para las unidades I62_DELEGATED; las unidades I61 siguen en I61; `MERGE_SHA`, CI posterior, cobertura, limpieza, `I62_EFFECTIVE_SHA`, PRE/POST y el blob del mapa en el tag `integration/I-62`». **Sin `MERGE_SHA` en el texto** (no existe al escribir el cierre; WORKFLOW §11.4: hashes en cuerpos de commit, evidencia y tag; HANDOFF enlaza). Secciones: Q8 | no |
| `docs/ROADMAP.md` | marca de cierre de la fila de I-62 (momento 3, WORKFLOW §2) | última columna: «**integrada (<fecha>)** — READY PASS (…); ADR-0048 <estado>; Owner Validation <resultado por escenario>; `MERGE_SHA`, CI posterior al merge, cobertura y limpieza en el tag `integration/I-62` ([evidencia](automation/evidence/I-62-evidence.md))» (formato de las filas de I-61 e I-63). La descripción de la fila conserva hechos obsoletos («Arquetipo NEW ARCHITECTURE provisional», «DC-07 pendiente», «Discovery (F0), Architect y Freeze pendientes»); WORKFLOW §2 solo permite la marca de cierre en el momento 3 | no |
| contrato `docs/initiatives/I-62-portabilidad-coordinador-principal.md` | `status` y enlaces | `status: integrated` (precedente I-63); `ov_assignment_ref` → V14 §18 si no se hizo en READY-08; `amendment_refs` → A-1 si no se hizo en READY-01 (Q11). Sin tips, corridas ni hashes cambiantes (WORKFLOW §11.4) | no |
| `docs/automation/state/I-62.yml` (`/v1`) | estado final | `current_phase: integrated`; `state: completed`; `next_action` sin trabajo pendiente; `last_evidence_commit` = `FINAL_CANDIDATE_SHA` (precedente I-63) | no |
| `docs/ideas-futuras.md` | sección «I-62 — hallazgos fuera de alcance y seguimientos» | solo lo diferido con autoridad (`ideas-futuras-disposition.md`) | no |
| evidencia y decisiones | READY, conformidad, OV, receta de integración | archivo de evidencia con el esqueleto de WORKFLOW §11.4 | no |

**Evidencia del commit de cierre** (SHA nuevo; no hereda nada del Candidato):
- [ ] `git diff --name-only <FINAL_CANDIDATE_SHA>..HEAD` solo lista `docs/` (WORKFLOW §4 paso 5.4); el mapa regenerado está bajo `docs/`.
- [ ] **Core Full y UI Full locales** sobre `CLOSURE_SHA` (AGENTS, fila «Cierre documental de iniciativa»: Core y UI obligatorias; una corrida anterior solo
  se reutiliza si es literalmente el mismo SHA).
- [ ] CI push 4/4 sobre `CLOSURE_SHA` (no sustituye la del Candidato: WORKFLOW §11.5).
- [ ] `python docs/automation/evidence/I-62-F4/compat/clause_map.py check $(git merge-base HEAD origin/main) HEAD <out>` = EQUAL.

## 3. Integración serializada (WORKFLOW §11.3 y §11.5; V14 §17 «Integración»; E.7)

**Regla de orden:** la C-20b de integración se ejecuta sobre el SHA exacto del merge que se publica. Ese merge lleva PRE en el cuerpo (WORKFLOW §11.3), así
que PRE se captura **antes** de construir el merge y la C-20b va después, sobre ese SHA. Si PRE se repite («Si la preparación se retrasa o cambia, se repite
PRE y se regenera el merge», §11.3), el merge regenerado tiene otro SHA y la C-20b se repite sobre él: la evidencia de un merge no publicado no se reutiliza
(V14 §17: «No hay reutilización de evidencia por igualdad de árbol. Cada SHA tiene la suya»; E.7: «Se regenera y se revisa de nuevo sobre el merge local
antes del push»).

| # | Paso | Comprobación o comando | Evidencia |
|---|---|---|---|
| 1 | `git fetch` inmediatamente antes del merge; DC-07 | `git fetch origin`; `git ls-remote --heads origin`; si `main` avanzó tras el cierre → **ruta R** (WORKFLOW §11.5: conservar la ronda, retirar el cierre del tip, rebasar, publicar el Candidato rebasado solo, repetir READY/conformidad/Full/CI/OV que correspondan, cierre nuevo) | salida cruda y fecha UTC |
| 2 | commit único con el trailer | `git log --format='%H %(trailers:key=Agent-Protocol-Normative,valueonly)' origin/main..HEAD` → exactamente un commit con `I-62`; `git log --grep='Agent-Protocol-Normative' origin/main` → 0. **Hoy no existe ninguno** (MEASURED: 0 en `bb0d5522..e9473425` y 0 en `origin/main`): Q15 | registro |
| 3 | **captura de PRE** (16.13 «Tabla PRE»; E.1; WORKFLOW §11.3; precedente `8a021fb6`) | `git ls-remote --heads origin` con fecha UTC, `main` observado y tabla `ref / tip / commit de reclamo / Claim-Id` (plantilla de §5) | texto de PRE (§5) |
| 4 | merge local `--no-ff` **con PRE en el cuerpo**, no publicado (WORKFLOW §4 paso 5.5). Técnica que fije la sesión de integración: `git merge --no-ff <rama>` o, como en el precedente de I-63 (tag `integration/I-63`, «Merge method»), `git merge-tree --write-tree` + `git commit-tree`, sin checkout de `main` | primer padre = `origin/main` observado en PRE; segundo padre = `CLOSURE_SHA`; el primer padre no alcanza el commit del trailer; cuerpo = §5 | `MERGE_LOCAL_SHA` |
| 5 | punto de entrada, §16.13, punteros, mapa y esquema en ese merge (E.7; 16.14 «No hay activación parcial») | `git show <MERGE_LOCAL_SHA>:docs/WORKFLOW.md` contiene una vez `## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)`; `git show <MERGE_LOCAL_SHA>:docs/AUTOMATION_PLAN.md` contiene una vez `### 16.13 Compatibilidad de protocolos de ejecución delegada`; mapa y esquema presentes | salida |
| 6 | **C-20b sobre ese merge exacto** (Anexo C; E.7) | `clause_map.py check <MERGE_LOCAL_SHA>^1 <MERGE_LOCAL_SHA> <out>` = EQUAL; toda diferencia se clasifica como corrección (vuelve por la rama) o A-n | `c20b-merge.json` con `MergeSha` = `MERGE_LOCAL_SHA` y `MapBlob` |
| 7 | Core Full sobre el mismo `MERGE_LOCAL_SHA` (prep. anterior; recomendado, no exigido literalmente por el Freeze) | SDK de usuario, como READY-05. Si su duración retrasa la publicación, se aplica la fila 8 | TRX + SHA-256, con el SHA del merge |
| 8 | **comprobación de PRE justo antes del push** (operación de §11.3: PRE «inmediatamente antes de publicar»; no es regla nueva) | `git ls-remote --heads origin` igual al snapshot de PRE del cuerpo. Si difiere, o la preparación se retrasó o cambió: repetir PRE (fila 3) → regenerar el merge (fila 4) → repetir las filas 5-7 sobre el SHA nuevo. Nunca se publica un merge cuyo SHA no tenga su propia C-20b | registro de la comparación |
| 9 | push a `main` del **mismo** `MERGE_LOCAL_SHA`, sin cambios | se acepta sin force; `I62_EFFECTIVE_SHA` = ese merge, vigente al aceptarse (16.14). No se escribe su SHA dentro de su propio commit (WORKFLOW §11.2, por analogía expresa de V14 §14.1) | registro ordenado del push; `MERGE_SHA` = `MergeSha` de `c20b-merge.json` |
| 10 | **POST** | `git ls-remote --heads origin` inmediatamente después | §6 de este archivo |
| 11 | CI posterior al merge (compuerta, WORKFLOW §4 paso 5.6) | `event=push`, `ref=refs/heads/main`, `head_sha=MERGE_SHA`, 4 jobs `success`, artefacto `rackcad-coverage-cobertura` presente | corrida |
| 12 | cobertura diferida del Candidato (WORKFLOW §4 paso 5.7) | `gh workflow run ci.yml --ref main -f candidate_sha=<FINAL_CANDIDATE_SHA>`; `candidate_sha` = `HEAD` del checkout = `measured-sha.txt` | corrida |
| 13 | derivación de EFF desde `origin/main` | `Derive`: primer merge first-parent cuyo segundo padre alcanza el commit del trailer y cuyo primer padre no; único (16.14) | registro |
| 14 | limpieza, solo tras 11 y 12 (WORKFLOW §4 paso 6: «solo después de que los pasos 5.6 y 5.7 hayan pasado») | rama local, remota y worktree según WORKFLOW §3 | resultado por elemento |
| 15 | tag anotado `integration/I-62` (WORKFLOW §11.6; E.7; 16.13) | contenido de §7; tipo `tag`, destino en first-parent de `main` | `git cat-file -p integration/I-62` |
| 16 | recibo en la evidencia | hechos posteriores al merge: solo en el tag (WORKFLOW §11.4) | — |

## 4. Archivos calientes y regla de la evidencia por SHA

AUTOMATION_PLAN, LIFECYCLE y WORKFLOW no figuran en la tabla de archivos calientes de WORKFLOW §7, y el contrato declara solo `hot_files: [docs/ROADMAP.md]`:
son superficies compartidas que I-62 modifica y se coordinan por DC-07. Hoy ninguna hermana las toca (DC-07 de §44, §60, §62).
Si una hermana integra antes y cambia una superficie que I-62 también modifica, el `BaseBlob` de esa fila del mapa cambia (MV-4) y el mapa debe
regenerarse (Q1). Toda integración de una hermana antes que I-62 mueve `origin/main` → READY-04 rebasa → SHA nuevo → READY-02..09 y READY-06 completos
(LIFECYCLE §8 y §9).

## 5. Plantilla de PRE (cuerpo del merge efectivo)

```text
Merge I-62: <título>

Initiative: I-62
Workflow governing integration: V2 (contrato: V2, T4)
FINAL_CANDIDATE_SHA: <40 hex>
CLOSURE_SHA: <40 hex>
Normative marker SHA: <40 hex del único commit con Agent-Protocol-Normative: I-62>
PRE UTC: <fecha UTC>
PRE main SHA: <40 hex>
PRE complete head count: <n>

Complete PRE raw git ls-remote --heads origin snapshot:
<salida cruda, una línea por ref>

Derived formal claim table:
Initiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition
I-52 | refs/heads/feature/rackmirror-espejo-semantico | <tip> | 150fd8e616209690abfbf521b3a1417e2a48e85c | 232b55c5-3d0c-47c7-90ae-a07caab06431 | <evidencia> | PRE: ANTERIOR_DEMOSTRADA → I61 de por vida
I-64 | refs/heads/architecture/workspace-persistente-rackcad | <tip> | 6ea9b7e8002eaa69e17fbcf677e7855f40315ec4 | 614371d5-441f-4f14-bac9-f97017105610 | <evidencia> | PRE: ANTERIOR_DEMOSTRADA → I61 de por vida
I-62 | refs/heads/architecture/portabilidad-coordinador-principal | <tip = CLOSURE_SHA> | a851841ae40d63f59ac18ff64f792efd07ca036c (imagen tras el rebase del 2026-10-05: 288649b682eb8cc5aba20b22f58983f23ac43121) | 5b661a17-8c18-4183-8554-3866059cba2b | <evidencia> | <incluir o no: Q7>
<toda otra rama presente en el snapshot>

I62_EFFECTIVE_SHA is not known until this merge is accepted by origin/main.
```

Los valores conocidos hoy (Claim-Id e identidades de reclamo) salen de: I-52, PRE de `8a021fb6`; I-64, `docs/automation/state/I-64.yml` y su primer commit
`6ea9b7e8` en su rama; I-62, `docs/automation/state/I-62.yml` y `rebase-map.json` (`a851841a → 288649b6`). Se vuelven a leer en el momento; nunca se
inventan (WORKFLOW §11.3: «PRE incompleto, Claim-Id ausente/duplicado o identidad contradictoria nunca recibe un valor inventado»). Las unidades de PRE
son ANTERIOR_DEMOSTRADA y siguen en I61 toda su vida (V14 §14.1).

## 6. Plantilla de POST

```text
POST UTC: <fecha UTC>
POST raw git ls-remote --heads origin snapshot:
<salida cruda>
Accepted push record: <ref> <old>..<new> (orden del push aceptado)
Claim table (misma forma que PRE; Claim-Id como clave de unión; una rama solo en POST = orden ambiguo → DESCONOCIDA, sin default)
```

## 7. Contenido del tag anotado `integration/I-62`

Bloque obligatorio de WORKFLOW §11.6 (una vez cada clave) más lo que exigen V14 §17 («POST y blob del mapa en el tag»), E.7 («El tag repite la ruta y el
blob del mapa, y POST») y 16.13/16.14 («su identidad se registra en el tag»). El formato de las claves adicionales es una propuesta, no texto congelado.

```text
Initiative: I-62
Unit: I-62
Workflow: V2
Claim-Id: 5b661a17-8c18-4183-8554-3866059cba2b
FINAL_CANDIDATE_SHA: <40 hex>
CLOSURE_SHA: <40 hex>
MERGE_SHA: <40 hex de la ronda verificada>
Unverified merges: none | <merge / candidate / closure / motivo>
Post-merge CI: run=<id> event=push ref=refs/heads/main head_sha=<MERGE_SHA> jobs=<resultados> coverage-artifact=<identidad>
Candidate coverage: run=<id> event=workflow_dispatch candidate_sha=<FINAL_CANDIDATE_SHA> measured-sha=<mismo SHA> artifact=<identidad>
Cleanup: local-branch=<resultado> remote-branch=<resultado> worktree=<resultado> date=<fecha>
Evidence: docs/automation/evidence/I-62-evidence.md
Corrects: none
Correction reason: none
I62_EFFECTIVE_SHA: <merge derivado según 16.14; = MERGE_SHA solo si la ronda verificada es la del merge efectivo>
Normative marker SHA: <40 hex>
Clause map: path=docs/automation/agent-execution/compatibility/I62-clause-map.json blob=<git rev-parse I62_EFFECTIVE_SHA:<ruta>>
PRE: <referencia al cuerpo del merge efectivo>
POST: <snapshot y tabla completos>
```

Precedentes: `integration/I-56` (bloque V1 + claves de activación) e `integration/I-61`. Un tag publicado nunca se mueve; una corrección usa
`integration/I-62-corr<N>` (WORKFLOW §11.6). La protección de `integration/*` es una acción administrativa aparte.

## 8. Conflictos con las hermanas (MEASURED 2026-10-07 con refs locales; repetir DC-07 en el momento)

| Rama / unidad | Punta (base) | Superficies de cierre que toca | Efecto sobre I-62 | Serialización |
|---|---|---|---|---|
| I-63 `architecture/parametros-calculados-resumen-proyecto` | **integrada**: merge `bb0d5522`, tag `integration/I-63` | añadió la fila **0049** al índice ADR (detrás de la 0046) y su fila de ROADMAP | ya en la base de I-62: la fila 0048 se inserta entre la 0046 y la 0049; `ideas-futuras.md` cambió de blob (`e5641f0e` → `570507d1`), la disposición de §1.2 de `f7-ready-closure.md` se rehace sobre el blob actual | sin acción |
| I-52 `feature/rackmirror-espejo-semantico` | `fb6b5648` (base `95690c28`, anterior a `bb0d5522`) | `docs/adr/README.md` (fila **0036**), `docs/adr/0036-…md`, `docs/HANDOFF.md`, `docs/ROADMAP.md` | si integra antes: I-62 rebasa (READY-04, repetición de READY-02..09); `docs/adr/README.md` es superficie de 16.13 → cambia el `BaseBlob` de la fila del índice en el mapa del cierre (se regenera en el cierre en todo caso); HANDOFF y ROADMAP: conflicto textual posible, se resuelve en el rebase | quien integre después rebasa; acuse de ventana de ROADMAP |
| I-64 `architecture/workspace-persistente-rackcad` | `39b45f36` (base `819955d6`); estado: `F1 BLOCKED_PROTOCOL_DEPENDENCY` | `docs/ROADMAP.md` (su fila); `docs/adr/0047-…md` (archivo, «propuesto»); en su cierre, la fila **0047** | si integra antes: la 0047 precede a la 0048 en el índice; `docs/adr/0047` entra en la base (no en el mapa de I-62). **Conflicto de coordinación:** su `next_action` espera que I-62 lleve el hallazgo nc2 a la evolución del protocolo; I-62 declara que la regla `Scope` de 16.22 rige solo unidades I62 y no repara la deuda nc2 de I-64, unidad I61 (evidencia §37.5; V14 §14.1). El cierre de I-62 no debe afirmar que desbloquea I-64 (Q13) | quien integre después rebasa; decisión del Master Coordinator sobre la deuda |
| I-62 | `e9473425` (base `bb0d5522`) | AUTOMATION_PLAN, LIFECYCLE, WORKFLOW, `agent-execution/`, PROMPT_TEMPLATES, `docs/adr/0048`, ROADMAP (fila del bootstrap); en el cierre: FOUNDATIONS, índice ADR, HANDOFF, ROADMAP, mapa | — | DC-07 antes de cada escritura de superficies compartidas |

## 9. Correcciones a `closure-plan.md` encontradas en esta preparación

1. **Trailer:** `closure-plan.md` §2.4 supone que la rama ya contiene el único commit con `Agent-Protocol-Normative: I-62` («el de MaterializationClose de
   F4»). **MEASURED:** `6f0187cb` no lo lleva y ningún commit de `bb0d5522..e9473425` ni de `origin/main` lo lleva. Hay que crearlo (Q15).
2. **Mapa en el cierre:** el plan no incluye la regeneración del mapa, que el cierre necesita porque modifica superficies de 16.13 (E.5 filas `docs/adr/` y
   `docs/FOUNDATIONS.md`): sin ella falla la C-20b (MV-3/MV-4); la guarda C-20a falla además solo si cambia ADR-0048, ya listado en el mapa.
   Interacción con `MC_I62`: Q1.
3. **HANDOFF:** el texto propuesto cita `<MERGE_SHA>`, que no existe al escribir el cierre; WORKFLOW §11.4 deja los hashes en el tag.
4. **FOUNDATIONS:** «entrada nueva» frente a `extends: [Agent Execution Protocol]` del contrato (Q3).
5. **Índice ADR:** «en orden numérico» ya no describe el índice real (`0045, 0044, 0046, 0049`); la fila va entre la 0046 y la 0049.
6. **PRE:** «El `Claim-Id` de I-62 no figura en PRE» contradice el precedente `8a021fb6`, donde I-56 figura en su propia tabla (Q7).
7. **Evidencia del cierre:** falta la Core Full y la UI Full locales sobre `CLOSURE_SHA` que exige AGENTS para el cierre documental.

## 10. Preguntas abiertas (lista canónica de este carril; las citan los demás archivos)

| Id | Cláusulas en tensión (exactas) | Pregunta | Decide |
|---|---|---|---|
| Q1 | V14 §15 («un cambio posterior a esas superficies lo invalida: se cierra de nuevo, se resiembra el fixture y se repiten los pilotos afectados»); AUTOMATION_PLAN 16.30, con sus propias palabras y sin resiembra («Un cambio posterior a esas superficies invalida el cierre: se cierra de nuevo y se repiten las pruebas afectadas»); C-21; frente a E.5 (filas `docs/adr/` «índice (cierre documental)» y `docs/FOUNDATIONS.md` «entrada (cierre documental; descriptiva)») y E.7 (mapa regenerado sobre el merge local) | El cierre documental modifica superficies de 16.13 y obliga a regenerar el mapa (también superficie, y `COPIED` en el fixture). ¿Ese cambio previsto invalida `MC_I62`? Si sí, ¿qué pilotos son «afectados» y cuándo se resiembra? Si no, ¿dónde se registra la excepción? | Coordinator (interpretación, LIFECYCLE §10) o A-n si el Freeze está incompleto |
| Q2 | V14 §18 (OD-1 «antes de READY-03»); LIFECYCLE §8 READY-02 («documentos de producto listos y versionados»); decisiones §47 («NO CHANGE BEFORE F6 PILOTS»); `closure-plan.md` §1 (Estado en el cierre); precedentes I-61 `b69c3463` (ADR-0046 aceptado en READY-03) e I-63 `13c6bee0` (ADR-0049 aceptado antes del Candidato) | ¿En qué commit cambia `Estado` de ADR-0048 tras OD-1: antes de READY-03 (nuevo blob en el mapa y Q1 antes de la OV) o en el cierre documental? | Coordinator |
| Q3 | contrato `extends: [Agent Execution Protocol]` e `introduces: []`; FOUNDATIONS «Esquema de entrada» (campos; `Status` = `STABLE` o `SUPERSEDED -> <entry>`) y LIFECYCLE §4.1 (entrada factual antes de READY-04), que no fijan si una extensión reescribe o crea; 16.14 (unidades I61 en I61 toda su vida); `closure-plan.md` §1 («entrada nueva») frente a `f7-ready-closure.md` §1.1 («extiende») | ¿Forma R (reescribir la entrada existente conservando, rotulados «(I61)», los hechos I61 vigentes) o Forma N (entrada I62 nueva, con la entrada I61 conservada `STABLE` o marcada `SUPERSEDED -> <entrada nueva>` con el motivo en la evidencia)? Con N, ¿cambian `extends`/`introduces` del contrato? Este carril redacta R (`foundations-draft.md` §2) y esboza N (§2.1) | Coordinator |
| Q4 | V14 §18 OV-I62-01 («con effort inferior y después correcto»); D.3 fila Principal A («FX-01 (3 preflights dentro de esta sesión…)»); D.5 («FX-01»); decisiones §51 («la secuencia compacta congelada exacta») | ¿La secuencia exacta de OV-I62-01 son dos observaciones (inferior → requerido) o los tres preflights de D.3? ¿Con qué effort el tercero? Además: `ov-packets.md` registra el **supuesto** de que OV-I62-01 y OV-I62-03 corren en la misma sesión del Principal A (D.3: Principal A «toda la ronda», «1 sesión», FX-01 «dentro de esta sesión», «+1 reapertura», tope 2; D.5: FX-02 «Principal 1»). ¿Se confirma? Si no, la segunda sesión consume la reapertura y no queda reserva | Coordinator |
| Q5 | D.5 («Se resiembra el fixture desde el Candidato»); D.1 (fixture con su `main` y F_eff); 16.14 (trailer único en first-parent de `main`: una segunda TEST-ACTIVATION en el mismo `main` del fixture sería ACTIVATION_INVALID); OD-7 (decisiones §46/§48: «un repositorio»; «Qué NO autoriza: … borrar el repositorio … cualquier otro repositorio»); regla del Owner §48 (repositorios de CI públicos por defecto); OD-5 (2) y OD-4 (`D:\r62-fixture`) | ¿Cómo se resiembra para la OV: origen local y repositorio público nuevos (exigen autorización del Owner que OD-7 no da), otra técnica dentro del repositorio actual, u otra? | Owner (infraestructura) + Coordinator |
| Q6 | D.3 FX-04a paso 4 (B = `codex-desktop-session`; Claude nueva = variante registrada aparte); decisiones §52 («en esta corrida»); catálogo sin celda Codex Frontera | ¿La selección de §52 (variante de mismo proveedor) vale también para OV-I62-05 (a) sobre el Candidato? | Coordinator |
| Q7 | 16.13/E.2 paso 4 («cid aparece exactamente una vez en PRE → I61»); precedente `8a021fb6` (I-56 en su propia tabla); `closure-plan.md` §2.5 | ¿La tabla PRE incluye el Claim-Id de I-62? | Coordinator |
| Q8 | WORKFLOW §4 paso 5.4, §5 y §8 («`docs/HANDOFF.md` §8-12»); HANDOFF actual con §1-§7; precedente I-63 `3c5019bf` (§1/§2/§4/§5) | ¿Qué secciones de HANDOFF edita el cierre? | Coordinator (errata de su dominio, LIFECYCLE §10) |
| Q9 | V14 §17 fila F6 (obligaciones C-22..C-27, C-39, **C-42 (7, 8)**); D.4 (FX-06 admite limitación decidida por el Owner); Anexo C C-32/C-37 («F4, F6») | Si FX-06 queda UNVERIFIED/UNSUPPORTED, ¿puede haber F6 GATE PASS con C-42 (7, 8) y las partes F6 de C-32/C-37 sin ejercer? | Coordinator |
| Q10 | D.3 fila Principal A («1 sesión … +1 reapertura … 2 sesiones»); decisiones §51 (Principal R temporal) | ¿R cuenta en el tope de sesiones de Principal de la ronda A? El Principal nuevo de FX-02 sería la 2.ª o la 3.ª | Coordinator |
| Q11 | decisiones §43 («el acuerdo se registra fuera (decisiones, evidencia, estado y registros de proceso)»); evidencia §58 («… en el estado y en el contrato»); cuerpo del contrato (línea 194: «**A-1 AGREED** sobre `ca09ade8` / blob `c01899a7`»); frontmatter `amendment_refs: []`; LIFECYCLE §6 y READY-09 («visibilidad de todas las A-n») | El cuerpo del contrato ya registra A-1. ¿Debe además el campo `amendment_refs` del frontmatter listar A-1 antes de READY-01? | Coordinator |
| Q12 | LIFECYCLE §6 («El acuerdo identifica commit, ruta y blob»; «inmutable desde su commit de Freeze»); FREEZE_SHA `b64a3b64` no es ancestro de la rama (imagen `fb49fceb`, evidencia §44.1); un rebase de READY-04 volverá a reescribirlo | ¿READY-09 acredita la identidad por blob + cadena de `RebaseMap` (patch-id), como hicieron los registros de A-1? | Coordinator |
| Q13 | estado de I-64 (`next_action`: llevar nc2 a la evolución del protocolo, I-62); evidencia de I-62 §37.5; V14 §14.1 (unidades I61 sin cambio) | ¿Cómo se registra en el cierre que I-62 no desbloquea I-64? | Master Coordinator / Coordinator |
| Q14 | LIFECYCLE §8 READY-06; V14 §20.11 (I-62 gobernada por I-61; relevo manual = AUTONOMY_GAP); OD-2 (paquete: «para las invocaciones del fixture»); OD-3 rechazada | ¿Qué sesión y transporte usa el Architect de la conformidad final de I-62 (plano a)? ¿Aplica la huella de OD-2 a invocaciones de `codex-cli` fuera del fixture? | Coordinator + Owner |
| Q15 | V14 §14.1 y 16.14 («el **único** commit con el trailer `Agent-Protocol-Normative: I-62`»); WORKFLOW §11.2 (precedente: marcador dedicado `afd1077c`, anterior al Candidato de I-56) | ¿Qué commit lleva el trailer y cuándo: un commit marcador vacío antes de READY-04/Candidato (no cambia superficies) o el commit de cierre? | Coordinator |
| Q16 | V14 D.3 FX-04b paso 3 («Worker: B no puede lanzar subagentes de Claude, así que se hace rebinding a `codex-cli` con escritura (OD-4) o a otro adapter que B pueda lanzar con escritura acreditada»), cuya premisa vale para la B `codex-desktop-session` de D.3 paso 4; D.4 («UNSUPPORTED» = «capacidad medida ausente»; «una limitación de escritura de Codex afecta a FX-04b y a FX-03»); decisiones §52 (B = `claude-desktop-session` «en esta corrida»); §50 («FX-04b / FX-03: se conservan UNSUPPORTED y UNVERIFIED … sin repetir la sonda de escritura»); §51-§54 (C-25b UNSUPPORTED); sonda OD-4 `R20261006T072306Z-od41` | Con la B de mismo proveedor de §52, ¿FX-04b / OV-I62-05 (b) sigue UNSUPPORTED, o pasa a UNVERIFIED (Controller `codex-cli` pendiente de OD-2d) con un Worker `claude-subagent` lanzable por B según D.3 paso 3? | Coordinator |
| Q17 | V14 D.3 FX-04a paso 8 (N11: segunda sesión B2 con un hecho retirado) y su tabla de topes (fila «N11: B2», 1 sesión); D.5 («**FX-04a**: Principal B 1 sesión; 0 invocaciones (OV-I62-05 a)»); decisiones §51-§53 (N11 = NOT_APPLICABLE para el QH2 de F6, sin `correction_launches`) | ¿N11 forma parte de «FX-04a compacto» de D.5 cuando el QH de la unidad resembrada tenga `correction_launches`? Si sí, ¿con qué tope de sesiones? | Coordinator |
