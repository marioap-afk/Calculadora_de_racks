# Paquete de cierre — F4B-PBT2A-DRAFT-STATE-PORT (PB-T2a)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `5ddeb3b4ed99cb17d05142c67d47a26e89e238f9`, con attempts **2 de 3**. Quedan en Application:
- el puerto por el que el editor completo de Push Back, en modo WORKSPACE, devuelve su estado editado al borrador: `IPushBackDraftStatePort`, `PushBackDraftApplyResult` y el adaptador `PushBackDraftSessionPort`, sobre `DraftSession<PushBackEditableState>`;
- el lanzamiento tipado: `PushBackDraftEditorLaunch`.

Ni la ventana, ni el piloto, ni el host, ni el Plugin cambian.

**Autoridades:**
- el contrato `F4B-PBT2A-DRAFT-STATE-PORT/coordinator/gate-contract.json` (paquete de PB-T2 v2.2; AuthorityRevision `1ee10d3f`; ChainBaseSha `0dc623ca`);
- el análisis de la corrección, en `.../1/R20261119T060524Z-55f2/analysis.md`;
- las decisiones del Owner F4B-E28, D2 y, sobre los controles, F4B-E34 (`F4B-DRAFTS/coordinator/owner-pbt2a-controls-decisions.md`);
- las disposiciones F4B-E32 y F4B-E33.

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `1ee10d3f` y BASE `0dc623ca` | F4B-E32, por la autorización condicional de F4B-E28 (pregunta 4); comprobación acotada 0/0/6, aplicada |
| Planificación | `R20261119T060327Z-ad3e` | Aceptada al primer lanzamiento: fidelidad 8/8 y A1-A8 8/8. nc4 `R20261119T060503Z-d5f7` |
| Worker, intento 1 | `R20261119T060524Z-55f2` | RED `c33ac6ac`: el esqueleto y las dos clases. **Paró con PBT2A-C07**, porque CT-PBT2A-02 leía el estado del lanzamiento después de aplicar |
| Análisis y preparación | `17872be8` | F4B-E33: corrección en el intento 2 de 3, con C-1 |
| Planificación de la corrección | `R20261119T065628Z-7de7` | Aceptada al primer lanzamiento: fidelidad 8/8 y A1-A8 8/8, con CorrectionOf. nc4 `R20261119T065912Z-4284` |
| Worker de la corrección | `R20261119T065932Z-3b39` | IMPLEMENTATION_COMPLETE. RED `fb90c823`, solo con C-1, y GREEN `5ddeb3b4`, con los dos archivos de Application |
| CI del RED | `44478438623` | tests en failure (CT-PBT2A-01 a 03, por aserción); ui-tests y build-ui en success; build-plugin omitido |
| CI del GREEN | `44480898605` | 4/4. Core 19 092; UI 2 439 (2 418 pasadas y 21 omitidas). Es la línea base del cierre de PB-T1c más 4 en Core, como se preveía |
| Revisión directa | — | Sin hallazgos |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/5** |
| Verificación | `R20261119T074453Z-10a9` | **EXECUTION_VERIFIED** / NONE, 14/14 |
| nc1 | `R20261119T074938Z-0302` | CONTROL NOT PASSED, una variación de PD-F4-04: Handoff, Identity, Remote, Scope, Ci y Tests, todas por el SHA falso; STOP S-04. **El Owner: «Aceptarlo solo esta vez»** → `nc1 = SATISFIED_BY_OWNER_DISPOSITION` |
| nc2 | `R20261119T075153Z-a799` | CONTROL NOT PASSED, la firma exacta de PD-F4-01: Contract y Scope; STOP S-04 y PBT2A-C01 |
| Sustituto de F4 sobre nc2 | `R20261119T081504Z-9d60` | `PBT2A-NC2-EXACT-WHITELIST-01`: **FAIL** en las propiedades 5 y 8, por el formato de F4B-E21 |
| Sustituto adaptado sobre nc2 | `R20261119T125505Z-c2d3` | **El Owner: «Adaptar el sustituto»** → `PBT2A-NC2-EXACT-WHITELIST-F4B-01`, revisado (APTO): **PASS**, 10/10 y autoprueba 50/50 → `nc2 = SATISFIED_UNDER_PBT2A_EXACT_WHITELIST_SUBSTITUTE_F4B` |
| nc3 | `R20261119T075831Z-6b19` | **CONTROL PASS** |

Las tablas de comparación, comprobación por comprobación, están en F4B-E34. Las decisiones literales del Owner están en `F4B-DRAFTS/coordinator/owner-pbt2a-controls-decisions.md`.

**Relevos:**
- 0 participantes en toda la cadena;
- la Snapshot J y el binario acreditado;
- `main` = `812baf87`, sin tocar.

**El relevo del trabajo de la corrección:** su script medía la cadena en todo el diff, que incluye el commit de documentación de la sesión. Se corrigió para mirar solo `src` y `tests`, la misma lección que el control de cadena de la verificación.

## 2. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| PBT2A-C07 (intento 1) | CT-PBT2A-02 leía `estado-es-el-actual` después de aplicar la edición | Corregido en el intento 2 (F4B-E33, C-1) |
| PBT2A-IR2-M01 a M05 | Cobertura de CT-02 y CT-03, dos comentarios XML, la lectura de INV-PBT2A-01 en una corrección y las corridas del Worker en segundo plano | `docs/ideas-futuras.md` |
| CT-PBT2A-04 | Las autopruebas se inyectan en los dos archivos nuevos, más estricto que la letra | Declarado; la revisión no lo marca |
| ChainRedSha | El RED del intento 1 no se acreditó: null en la corrección | Sin efecto |
| nc1 y nc2 | Los controles no salían por las dos vías de PBT2A-C11 | El Owner (F4B-E34): nc1 aceptado solo esta vez; nc2, el sustituto adaptado a F4B-E21 |

**Siguiente:**
- **PB-T2b:** se emite con el paquete de PB-T2 v2.7 (la v2.6 más la regla RR de F4B-E35) cuando su recomprobación termine limpia, por la autorización condicional de F4B-E28.
- **PB-T3:** tiene la autorización condicional de F4B-E35 (pregunta 3) y se emite tras el cierre de PB-T2c.
