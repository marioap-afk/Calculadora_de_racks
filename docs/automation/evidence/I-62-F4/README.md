# I-62 — Evidencia de F4 (implementación de producción; Freeze V14 + A-1 AGREED)

```text
Autoridad:  decisiones §43 (veredicto del Coordinator sobre A-1 y orden de apertura de F4)
A-1:        AGREED sobre ca09ade8 / docs/initiatives/I-62-A-1.md / blob c01899a7 (no se edita)
Nivel:      A (V14 §20.10): textos normativos, esquemas, validadores deterministas en tests/RackCad.Tests y controles reproducibles; sin servicio
Estado:     en curso (cortes F4-A..F4-H); este archivo se completa con el paquete del gate (F4-H)
```

## Superficies de producción

- `tests/RackCad.Tests/I62/` (namespace `RackCad.Tests`, exigido por la guarda de I-23): la implementación. Sus guardas viven en
  `tests/RackCad.Tests/I62F4*Tests.cs`, y sus datos sintéticos en `tests/RackCad.Tests/I62Fixtures/`.
  - `YamlSubset.cs`: lector con fallo cerrado y escritor canónico del subconjunto YAML de `state/v2` (puerto de `I-62-prep/f4/yaml-subset`).
  - `StateV2Shape.cs`: forma de B.8.1 y B.8.8 con los campos de A-1.
  - `StateTree.cs`: árbol del commit de un punto (lo que un `StateRef` puede citar).
  - `StateV2Validator*.cs`: invariantes de archivo (I-S01..I-S18), de pares (I-P01..I-P13) y, en F4-E, con historia.
  - `Orchestration.cs`: constantes, aristas y lectores de la orquestación (topes congelados, entradas de decisión, autoridad REVIEWER, D1-17).
  - `RebaseChain.cs`: cadena de mapas, ResolveBranchRef (D2-10), EquivalentReviewedObject (D2-12) y validez de un mapa en su publicación (B.8.7 y
    A62-A1U-O1).
  - `StateV2Validator.History.cs`: invariantes con historia (I-H01, I-H02 con D2-3, I-P03, I-P08) y las comprobaciones de A-1 que necesitan Git
    (D2-2 cont. con A62-A1T-01, D2-6 (2), D2-11). `IGitHistory` tiene dos implementaciones: `GitProcessHistory` (Git real) y, en los datos de
    prueba, `SyntheticGitHistory` (grafo sintético); `GitScratch` crea repositorios Git desechables para los negativos E y F.

## RED → GREEN por corte (`red-green/`)

| Corte | Guarda | RED (método sin implementar) | GREEN |
|---|---|---|---|
| F4-A | `I62F4YamlSubsetTests` | 9/9 en error (`Read` y `Write`) | 9/9 |
| F4-B | `I62F4StateV2FileInvariantTests` | 42/43 en error (`ValidateFile`; el barrido de los estados v2 reales no lo llama) | 43/43 |
| F4-C | `I62F4StateV2PairInvariantTests` | 30/30 en error (`ValidatePair`) | 30/30 |
| F4-D | `I62F4OrchestrationValidatorTests` | 51/51 en error (`FileOrchestration` y `PairOrchestration`) | 51/51 |
| F4-E1 | `I62F4RebaseChainTests` (negativos A–H de la cadena, A62-A1U-O1, D2-12) | 8/8 en error (`Resolve`, `PublicationProblems` y `Equivalent`) | 8/8 |
| F4-E2 | `I62F4HistoryInvariantTests` | 9/10 en error (`ValidateHistory`, `ValidatePairHistory` y `ValidateB1History`; la admisión por los validadores de archivo y de pares no los llama) | 10/10 |

El RED se captura sustituyendo solo los cuerpos de los métodos nombrados por `throw new NotImplementedException` y restaurando el archivo byte a byte
(`redgreen.py` en el scratchpad de la sesión; la salida queda aquí). La selección es mayor que cero en todos los cortes.

## Decisiones de implementación (sin cambio de semántica congelada; para revisión del Coordinator)

- **F4-OBS-01 — `next_action` en la reconciliación.** I-P05 enumera lo que puede cambiar un QU REBASE_RECONCILIATION, y no nombra `next_action`. Pero
  I-S18 exige que `NextAction` sea derivable de forma única del estado: si su `target` copia `loop.object`, tras la reconciliación (D2-4) la copia
  guardada debe seguir a su objeto. El validador admite solo que `next_action.target.commit` pase a la imagen por el mapa, con el mismo `path` y
  `blob` (regla de D2-4); cualquier otro cambio de `next_action` en la reconciliación sigue siendo I-P05.
- **F4-OBS-02 — referencias al archivo de decisiones.** Los archivos de decisiones solo crecen por entradas añadidas, y I-S13 obliga a cada punto a
  citar el blob de su propio árbol. Así, una `StateRef` a un archivo de decisiones cambia de blob sin cambiar de sentido. En las reglas de pares que
  comparan registros inmutables (D1-6, D1-8, D1-9, D1-18, D1-19, D2-8), esa referencia se compara como igual **solo** si el archivo de `n` es una
  extensión por adición del de `p` (los dos blobs están en los árboles del par). Una reescritura de una entrada anterior sigue detectándose (caso
  negativo propio en C-38).
- **F4-OBS-03 — forma de una entrada de decisión.** Los marcadores congelados (§8.6, §20.5 y los de A-1) se leen en el bloque cercado de la entrada
  que contiene la línea exacta del marcador; sus campos son líneas `Clave: valor`. Para la `ReviewLoopAuthorization`: `ContinuesLoopInstanceId` (D1-12)
  y `Budget.<tope>` con los nombres de `architect_budgets[].caps` (D1-2); un tope ausente no rebaja el congelado. El texto literal se materializa en la
  subsección I62 de AUTOMATION_PLAN §16 (F4-H), como dispone §8.6 para las plantillas de los marcadores.
- **F4-OBS-04 — sustitución de una autoridad REVIEWER.** D1-20 impide abrir un bucle REVIEWER con una autoridad SUPERSEDED por una decisión custodiada
  del Coordinator. Para que sea comprobable mecánicamente, la decisión de sustitución lleva el marcador
  `I62-REVIEWER-AUTHORITY-SUPERSEDED: <authorization_id>` (identidad de D1-20). Se materializa con las demás plantillas.
- **F4-OBS-05 — nombres canónicos de `StateFields[].Field`.** El campo es texto libre en `relay-record/v2`. El validador exige una entrada única por campo
  SHA con estos nombres: `chains[<task_id>].chain_base_sha`, `chains[<task_id>].chain_red_sha`, `last_window.verified_sha`, `unverified_commits[<i>].sha`,
  `last_evidence_commit` y, por A-1 D2-1, `orchestration.loop.object.commit`, `orchestration.review_requests[<id>].object.commit` y
  `orchestration.review_requests[<id>].attempts[<seq>].Target.commit`.
- **F4-OBS-06 — aristas de fase.** Además del diagrama de §20.5 (el QU de ingestión publica la fase siguiente directamente) y de las dos de SM-05, el
  validador admite la reejecución de transporte tras un resultado INVALID (RESULT_INGESTED → (RE)REVIEW_PENDING) y la segunda solicitud tras una
  decisión del Owner (ESCALATE_OWNER → REREVIEW_PENDING, §20.6), como el oráculo de la preparación.
- **F4-OBS-07 — commits de la ventana (I-P03, I-P08).** V14 dice «entre p y n solo hay commits del Worker o imágenes del rebase de 16.7» (W-2)
  y «ningún commit del Worker modifica el archivo de estado» (W-3), sin nombrar cómo se reconoce un commit del Worker. El validador usa los hechos
  custodiados por n: un commit de la ventana es del Worker si es ancestro de uno de sus resultados (`last_window.verified_sha`,
  `unverified_commits[].sha` o el `chain_red_sha` de la tarea de la ventana). Esto equivale a la cadena `BaseSha..CurrentSha` de I-61. La ventana
  empieza en el Q0, o en su imagen cuando el rebase de la propia ventana lo reescribió (I-P12). Las dos reglas aplican solo con p = Q0: fuera de una
  ventana no hay commits del Worker, y un par de reconciliación no puede contar la imagen de p como escritura del Worker.
