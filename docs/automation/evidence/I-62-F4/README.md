# I-62 — Evidencia de F4 (implementación de producción; Freeze V14 + A-1 AGREED)

```text
Autoridad:  decisiones §43 (veredicto del Coordinator sobre A-1 y orden de apertura de F4)
A-1:        AGREED sobre ca09ade8 / docs/initiatives/I-62-A-1.md / blob c01899a7 (no se edita)
Nivel:      A (V14 §20.10): textos normativos, esquemas, validadores deterministas en tests/RackCad.Tests y controles reproducibles; sin servicio
Estado:     paquete del gate (F4-H) para la revisión del Coordinator; F4 GATE PASS no se autodeclara
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
  - `MarkdownSections.cs`, `MiniJsonSchema.cs` y `CompatibilityGuard.cs`: secciones de 16.3/E.4, validación del subconjunto de JSON Schema
    del mapa (con fallo cerrado ante una palabra clave no soportada; sin dependencias nuevas) y la guarda C-20a sin historia.
  - `VerificationFacts.cs` (P1 identidad, 16.9 #5; P2 alcance, 16.9 #7; P3 hechos sin veredicto), `ConfigurationFacts.cs` (P5: invalidación
    por los `Invalidators` de `preflight/v1`, STOP por huella cambiada en una cesión, huellas sin valores ni secretos; sin operación de aceptar) y
    `TrxReading.cs` (P6: lectura normalizada del TRX con estados inesperados y cruce con las declaraciones `Skip =`). Son helpers deterministas dentro de
    la frontera de verificación del Controller (decisiones §43), puertos de `I-62-prep/night-2026-10-05/helpers/`; ni rol, ni servicio, ni autoridad.
  - `NextActionDerivation.cs`: `NextAction` derivable de forma única (V14 §20.4; I-S18; P-17), integrada en el validador de archivo.
- Textos normativos de F4 (plano b, inactivos hasta `I62_EFFECTIVE_SHA`): `docs/AUTOMATION_PLAN.md` §8 (formato `/v2` de las unidades I62), 16.25
  (custodia, ventana, CAS, rebase con las filas de la orquestación de A-1, `RebaseMap`, ResolveBranchRef, EquivalentReviewedObject y toma con rebase), 16.26
  (recuperación y transiciones T0..T22, reconstrucción, commits sin verificar, P-12 y P-13), 16.27 (conteo), 16.28 (arranque, adopción y plantillas
  literales de los marcadores), 16.29 (orquestación: siguiente acción, bucles del Architect y del REVIEWER, intentos, presupuestos, AUTONOMY_GAP, P-17,
  P-18 y P-21) y 16.30 (planos, MaterializationClose y P-16). Enmiendas de A-1 a textos F3: 16.20 (paso 3 con ResolveBranchRef, `ContinuesLoopInstanceId`
  y vigencia del REVIEWER) y README §14.3 (`VALIDITY` y «Reproducción») y §14.4 (regla 3). README de `agent-execution` §17 y §18 (procedimientos).
- `DelegationJournal.cs`: diario encadenado (`PrevRelaySha256`, `WindowSeq`), `DS(L, J)` de B.8.2 con la coherencia del `Exit`, los contadores del Q7
  desde el diario (S-04), el mínimo conservador de B.8.5, la clasificación R-1/R-2/R-3 de los commits posteriores a un Q0 sin diario y el conteo por
  clase que hereda una continuación (`ContinuesTaskId`, C-16).
  `GitCommitStateTree` lee el árbol del commit de cada punto (los blobs con un solo `ls-tree`); `GitProcessHistory` memoriza los hechos de ids completos.
- `Adoption.cs` (T20/T21), `ProcessFacts.cs` (README §3.2, C-17), `InputClosure.cs` (16.24, C-41) e `InputFidelity.cs` (§20.3.3, C-42: preflight,
  representación entregada, envoltorios, unidades canónicas de B.11 con la segmentación de `gen-manifest.py`, clausura, resolución e independencia).
- `MaterializationClose.cs`: la invalidación del cierre de materialización (C-21) y los registros AUTONOMY_GAP (C-37) con la auditoría del transporte
  (`OWNER_AS_MESSAGE_BUS`, C-32).
- `docs/automation/agent-execution/schemas/automation-state.v2.schema.json`: la emisión exacta de `StateV2Shape.ToJsonSchema()` (guarda C-18 en
  `I62F4StateSchemaTests`).
- Textos de la adopción de compatibilidad (Anexo E), inactivos hasta `I62_EFFECTIVE_SHA`: `docs/WORKFLOW.md` §12 (punto de entrada, texto
  congelado de E.3.0), §4 paso 2 (referencia «al abrir») y §10 (fila ampliada); `docs/AUTOMATION_PLAN.md` §16.13 (resolver) y los punteros de §16 y
  16.3; `docs/automation/agent-execution/compatibility/clause-map.schema.json` e `I62-clause-map.json`, derivado con `compat/clause_map.py`.

## RED → GREEN por corte (`red-green/`)

| Corte | Guarda | RED (método sin implementar) | GREEN |
|---|---|---|---|
| F4-A | `I62F4YamlSubsetTests` | 9/9 en error (`Read` y `Write`) | 9/9 |
| F4-B | `I62F4StateV2FileInvariantTests` | 42/43 en error (`ValidateFile`; el barrido de los estados v2 reales no lo llama) | 43/43 |
| F4-C | `I62F4StateV2PairInvariantTests` | 30/30 en error (`ValidatePair`) | 30/30 |
| F4-D | `I62F4OrchestrationValidatorTests` | 51/51 en error (`FileOrchestration` y `PairOrchestration`) | 51/51 |
| F4-E1 | `I62F4RebaseChainTests` (negativos A–H de la cadena, A62-A1U-O1, D2-12) | 8/8 en error (`Resolve`, `PublicationProblems` y `Equivalent`) | 8/8 |
| F4-E2 | `I62F4HistoryInvariantTests` | 9/10 en error (`ValidateHistory`, `ValidatePairHistory` y `ValidateB1History`; la admisión por los validadores de archivo y de pares no los llama) | 10/10 |
| F4-F | `I62F4CompatibilityGuardTests` (C-20a) | 7/10 en error **sobre el árbol sin materializar** (sin punto de entrada, 16.13, punteros ni mapa); pasan la procedencia de 16.3, el subconjunto de esquema y un control que también detecta el puntero ausente | 10/10 |
| F4-G1 | `I62F4VerificationHelperTests` P1-P3 (Git real) | 4/4 en error (`Identity`, `Scope` y `VerdictProblems`) | 4/4 |
| F4-G2 | `I62F4VerificationHelperTests` P5 | 3/3 en error (`Compare`, `CessionGate` y `FingerprintProblems`) | 3/3 |
| F4-G3 | `I62F4VerificationHelperTests` P6 | 3/3 en error (`Read` y `DeclaredSkipMethods`) | 3/3 |
| F4-G4 | `I62F4NextActionTests` | 5/5 en error (`Derive` y `Check`) | 5/5 |
| F4-H1 | `I62F4StateSchemaTests` (C-18, esquema `/v2`) | 10/10 en error **sin el archivo del esquema** | 10/10 |
| F4-G5 | `I62F4JournalCounterTests` (C-15 DS, C-16) | 5/5 en error (`ChainProblems`, `Derive`, `ExitProblems`, `Counts`, `Q7CounterProblems`, `ConservativeMinimum`, `ReconstructionProblems`) | 5/5 |
| F4-G6 | `I62F4ProcessFactsTests` (C-17) | 3/3 en error (`Classify`, `WorkerClasses`) | 3/3 |
| F4-G7 | `I62F4InputClosureTests` (C-41) | 4/5 en error (`ObligationsOf`, `AuditReads`; la comparación de identidad no los llama) | 5/5 |
| F4-G8 | `I62F4InputFidelityTests` (C-42) | 6/6 en error (`Preflight`, `Compare`, `Units`, `Independence`, `Ingest`, `Resolve`) | 6/6 |
| F4-G9 | `I62F4CloseAndGapTests` (C-21, C-37) | 2/2 en error (`Invalidating`, `Missing`) | 2/2 |
| F4-G10 | adopción en `I62F4CustodyRebaseMcTests` (T20/T21) | 1/1 en error (`T20Problems`, `T21Problems`) | 1/1 |
| F4-H2 | `I62F4A1RegressionTests` (regresión de A-1) y C-40 (F4) | 103/110 en error (`FileOrchestration` y `PairOrchestration`; pasan el control de cobertura y seis trazas solo de historia) | 110/110 |
| F4-H3 | `I62F4CustodyTransferMcTests` y `I62F4CustodyReconstructionMcTests` (C-15: T16, T17, F.3, T12a, T12b) | 5/5 en error (`PairPrincipal`) | 5/5 |
| F4-H4 | `I62F4JournalCounterTests` (C-16: correcciones frente a verificaciones, `ContinuesTaskId`) | 1/6 en error (`ClassLaunches`, `ContinuationLineage`, `ContinuationProblems`; los demás no los llaman) | 6/6 |
| F4-H5 | `I62F4LoopRecoveryMcTests` (C-29: caídas en cada frontera de F.8) | 1/1 en error (`PairOrchestration`) | 1/1 |
| F4-H6 | `I62F4CloseAndGapTests` (C-32: auditoría del transporte) | 1/3 en error (`OwnerAsMessageBus`) | 3/3 |

El RED se captura sustituyendo solo los cuerpos de los métodos nombrados por `throw new NotImplementedException` y restaurando el archivo byte a byte
(`redgreen.py` en el scratchpad de la sesión; la salida queda aquí). La selección es mayor que cero en todos los cortes. F4-H2, F4-H3 y F4-H5
son controles nuevos sobre reglas ya implementadas en F4-B..F4-E: su RED quita el método del validador que las aplica.

## Regresión de A-1 (`I62F4A1RegressionTests`)

Cada traza del arnés de A-1 (`docs/automation/evidence/I-62-A1/a1-counterexamples-result.json`) de los hallazgos que nombra la orden se reproduce sobre
el validador de producción con su nombre y su veredicto. Una traza VALID pasa todos los puntos, pares e invariantes con historia. Una INVALID falla
en su paso defectuoso por la invariante y la cláusula de A-1 que la guardan, y por nada fuera del conjunto declarado. Las trazas que ya eran negativos
con nombre de C-38 se reutilizan por nombre. Un control comprueba que ninguna traza de esos hallazgos queda sin reproducir ni sin cubrir.

| Hallazgo | Trazas del arnés | Reproducidas: VALID / INVALID / negativo C-38 | Cubiertas por otra prueba |
|---|---|---|---|
| A62-A1-01 | 7 | 1 / 2 / 4 | — |
| A62-A1-02 | 12 | 6 / 2 / 4 | — |
| A62-A1-03 | 4 | 1 / 1 / 2 | — |
| A62-A1-04 | 6 | 3 / 3 / 0 | — |
| A62-A1-05 | 4 | 2 / 2 / 0 | — |
| A62-A1-06 | 10 | 3 / 5 / 0 | `a62-a1-06-sucesor-en-clon-limpio` (Git real: `I62F4RebaseChainTests` E_F y `I62F4CustodyMcTests`); `fc06-literal-referencias-tras-rebase` (contraejemplo del I-S18 literal de V14; con A-1 es `a62-a1-06-referencias-tras-un-rebase`, VALID) |
| OBS-A1-01 | 21 | 8 / 7 / 6 | — |
| A62-A1R-01 | 4 | 1 / 3 / 0 | — |
| A62-A1R-02 | 5 | 3 / 2 / 0 | — |
| A62-A1R-03 | 4 | 1 / 2 / 1 | — |
| A62-A1S-01 | 2 | 0 / 2 / 0 | — |
| A62-A1S-02 | 2 | 1 / 1 / 0 | — |
| A62-A1T-01 | 16 | 4 / 12 / 0 | — |
| A62-A1U-O1 | obligación de F4 | 0 / 1 / 0 (entrada compuesta fabricada) | `I62F4RebaseChainTests` C y `I62F4HistoryInvariantTests` (A62-A1T-01) |

Son 94 trazas distintas: 92 reproducidas y 2 cubiertas por otra prueba (algunas trazas pertenecen a dos hallazgos). Además, 14 mutaciones en memoria:
desactivar la cláusula que guarda un negativo de cada hallazgo lo deja pasar (D1-5, D1-8, D1-9, D1-13, D1-17, D1-18, D1-20, D2-2, D2-3, D2-8, D2-11,
D2-12 y A62-A1S-02).

## Controles por obligación (C-15..C-42; solo la parte de F4)

Las clases de prueba se citan sin el prefijo `I62F4`. «Mutación» se exige para la evidencia (i) Core RG + mutation; en las obligaciones (ii) MC el
negativo de cada control hace ese papel. «Verde» significa que los controles pasan en el SHA de implementación. No es un GATE PASS.

| C | Parte de F4 | Superficie | Positivo | Negativo | Mutación | Evidencia | Resultado |
|---|---|---|---|---|---|---|---|
| C-15 | completa | `RebaseChain.cs`, `StateV2Validator.History.cs`, `DelegationJournal.cs`, `StateTree.cs`, `Adoption.cs`; 16.25, 16.26; README §17 | BOOTSTRAP → G0 → QU, F.1 y QU entre ventanas, rebase entre ventanas (F.6) con la Q0 siguiente desde un clon limpio `--no-local` (`CustodyMc`); rebase dentro de una ventana, toma con rebase (F.7) y adopción T20/T21 (`CustodyRebaseMc`); T17, T16 y F.3 con T12b (`CustodyTransferMc`); T12b desde Q0 con R-1 y R-2 y T12a con TRANSFER (`CustodyReconstructionMc`); `DS` en cada paso (`JournalCounter`); positivos de `HistoryInvariant` y `RebaseChain` | las doce mutaciones sembradas de la fila (`CustodyMutationMc`); push rechazado y fallo de transporte (`CustodyCasMc`); un segundo recuperador sin designación; negativos A–H de la cadena y A62-A1U-O1; trazas INVALID de A62-A1-04/05/06, A62-A1R-02 y A62-A1T-01 | desactivar I-H01, I-H02, D2-3, D2-11, D2-2, D2-6 o I-P08 deja pasar su negativo | F4-E1, E2, G5, G10, H2, H3 | verde |
| C-16 | completa | `DelegationJournal.cs` (conteo, Q7, mínimo conservador, R-1/R-2/R-3, `ClassLaunches`, `ContinuationProblems`); 16.27 | contadores del Q7 = diario; incierto contado; mínimo conservador; dos verificaciones no son dos correcciones; una corrección caída antes de verificar cuenta; `ContinuesTaskId` hereda la clase; R-1/R-2 en Git real | contador ausente o contradictorio → S-04; reconstrucción bajo el mínimo; continuación de sí misma, de una tarea sin cadena o en ciclo → S-04; R-3 sin reconstrucción | — | F4-G5, H3, H4 | verde |
| C-17 | completa | `ProcessFacts.cs`; 16.4; README §3.2 | clases sobre instantáneas sintéticas; `c17/c17-processes.ps1` en el host (positivo `pwsh` con la ruta) | efímero no vivo tras relectura; participante ajeno o no atribuible → P-02 | — | F4-G6; `c17/c17-result.json` | verde |
| C-18 | completa | `YamlSubset.cs`, `StateV2Shape.cs`, `StateV2Validator*.cs`; esquema `/v2` | estados `/v2` reales e historia sintética, cada punto y cada par | 35 negativos de archivo y 22 de pares, uno por invariante | desactivar la invariante deja pasar su negativo (11 casos) | F4-A, B, C, H1 | verde |
| C-19 | — (F2) | — | — | — | — | guarda de F2 en el Full | no se reclama |
| C-20a | completa | `CompatibilityGuard.cs`, `MiniJsonSchema.cs`, `MarkdownSections.cs`; WORKFLOW §12, 16.13, mapa y esquema | árbol materializado | 19 comprobaciones con su negativo | sobre el árbol sin materializar, 7/10 en error | F4-F | verde |
| C-20b | F4 (la repetición sobre el merge es de la integración) | `compat/clause_map.py` | mapa custodiado = derivación (EQUAL) | negativos de mapa dentro de C-20c (MAP_INVALID) | — | `compat/c20b-result.json` | verde; la tabla E.5/derivado espera la revisión del Coordinator |
| C-20c | completa | `compat/c20c.py` | C-20c-1 (a)(b), C-20c-2 y lecturas compuestas | N-a..N-s y negativos de mapa | — | `compat/c20c-result.json` | verde (32/32 con C-28) |
| C-21 | completa | `MaterializationClose.cs`; 16.30 | sin cambio normativo tras el cierre | cambio normativo posterior → invalidación (Git real) | — | F4-G9 | verde; revisión del Coordinator |
| C-22..C-27 | — (F6) | — | — | — | — | — | no se reclama |
| C-28 | completa | 16.13, 16.28 y el clasificador | (a), (c), (e) trabajo directo sin maquinaria delegada; (d) contrato tras el QU con aceptaciones | (b) contrato en DIRECT_ONLY → P-15; (d) contrato antes del QU → PENDING_G0 | — | `compat/c20c-result.json` (C-28) | verde |
| C-29 | completa | `NextActionDerivation.cs`, validador; `LoopMc` | sucesor en clon limpio reconstruye X, resultado, REQUIRED abiertos, presupuesto y CORRECT_AND_REREVIEW (`LoopReconstructionMc`); caídas A–D con la evidencia de fidelidad y la acreditación histórica desde la custodia (`LoopRecoveryMc`) | segunda reserva, relanzar un incierto, segunda ingestión con otro blob, recuento | — | F4-G4, H5 | verde |
| C-30 | F4: (e) en el estado | F3: contratos (`f3-mc.py`); F4: ingestión (I-P13) | resultado del rol correcto (F.8) | resultado con el contrato de otro rol, nunca VALID (`OrchestrationMc`) | — | F3: `f3-mc-result.json` (12 casos); F4: `OrchestrationMc` | verde (parte de F4) |
| C-31 | completa | validador; `LoopMc` | F.8 en Git real, todas las invariantes en cada punto y sin escalada | negativos de C-38 sobre F.8 | las de C-38 | F4-D | verde |
| C-32 | F4: auditoría de la secuencia de C-31 | `AutonomyGaps.OwnerAsMessageBus` | `OWNER_AS_MESSAGE_BUS` = false (sintético y Git real) | un relevo del Owner registrado → true | — | F4-H6 | verde (parte de F4); FX-06 es de F6 |
| C-33 | completa | validador (P-19, escalada) | BLOCKED — OWNER DECISION → ESCALATE_OWNER con la decisión exacta | corrección o invocación tras la escalada | — | `OrchestrationMc` | verde |
| C-34 | completa | validador (topes, P-18) | topes congelados y rebajados | (a)–(e): cada tope para antes de reservar | — | `OrchestrationMc` | verde |
| C-35 | completa | validador (P-19, D2-12, contrato de salida) | VALID CHANGES REQUIRED abre sus linajes | INVALID no abre ni avanza; blob distinto, AGREED con REQUIRED, omisión, otra versión, otro rol | desactivar P-19 deja pasar | `OrchestrationMc`, C-38 | verde |
| C-36 | completa | validador | cambio de proveedor, modelo, Principal y etiquetas continúa contadores y linaje | reinicio de un contador | — | `OrchestrationMc` | verde |
| C-37 | F4: registro y P-21 | `AutonomyGaps.Missing` y `Record`; README §18.7 | relevo manual con registro custodiado | relevo sin registro; registro que no resuelve (I-S13) | — | F4-G9 | verde (parte de F4); el FX es de F6 |
| C-38 | completa | `StateV2Validator.Orchestration.cs`, `NextActionDerivation.cs` | F.8 verde en cada punto y par; positivos de A-1 | 39 negativos de C-38 y 93 casos de la regresión de A-1 | 7 + 14 cláusulas desactivadas | F4-D, G4, H2 | verde |
| C-39 | — (F6) | — | — | — | — | — | no se reclama |
| C-40 | F4: vigencia de acción | F3: materialización (`f3-mc.py`); F4: validador e I-P13 | (a) binding materializado en F.8; (e) acreditación tras AGREED; (g) revisión antigua acreditada; (i) LAUNCHED se completa tras revocar; (j) arrancó antes del fin | (f) LAUNCHING tras revocar; (k) cancelación sin prueba; (l) reintento del incierto; (m) resultado de un cancelado; (n) UNAUTHORIZED_LAUNCH no abre linajes ni avanza | — | `OrchestrationMc` | verde (parte de F4); F4-OBS-20 |
| C-41 | completa | `InputClosure.cs`; 16.24 | (a) cierre sobre el `AGENTS.md` real | (b)–(f) | — | F4-G7 | verde |
| C-42 | F4: corpus y capturas sintéticas | `InputFidelity.cs`; §20.3.3 | (1)–(3), (a), (f), (i), (k), (m), (r), (t), (v), (w), (z), (6), (7) | (4), (5), (b)–(e), (g), (h), (j), (l), (n), (o), (p), (q), (s), (u), (x), (y), (8) | — | F4-G8 | verde (parte de F4); las capturas reales de runtime son de F6 |

## Full y CI del SHA de implementación

```text
SHA:   6f0187cb30852971b49153c2763c8ba6caeaa64d (árbol limpio antes y después del Full; HEAD igual al empezar y al terminar)
Core:  dotnet test tests/RackCad.Tests/RackCad.Tests.csproj      12962 total, 12962 correctas, 0 con error, 0 omitidas
UI:    dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj  1654 total, 1637 correctas, 0 con error, 17 omitidas
CI:    corrida 37391012002, event push, ref refs/heads/architecture/portabilidad-coordinador-principal, head_sha exacto;
       Tests (Domain + Application), Build Plugin without AutoCAD, Build UI y UI Tests en success
```

- **P6 sobre los TRX reales** (`p6/p6-trx.ps1`, que carga `TrxReading` del ensamblado de pruebas; resultado en `p6/p6-result.json`, sin copiar los
  TRX): Core PASS (SHA-256 `2af7eea2…`), 12962 correctas y ninguna omitida. UI PASS (SHA-256 `527c50d4…`): 1637 correctas y 17 omitidas, cada una
  cruzada con un método declarado con `Skip =` en las fuentes de UI. El número de omisiones no está fijado en el helper. Nota de Core: «Again» y
  «Later» aparecen como declarados y no omitidos porque son el texto de una fuente sintética dentro de `I62F4VerificationHelperTests`; es una nota,
  no un fallo.
- **C-20b sobre el SHA:** `compat/clause_map.py check bb0d5522 6f0187cb` = EQUAL (31 archivos, 70 entradas).
- **C-20c sobre la punta publicada:** `compat/c20c.py` = 32/32, mapa VALID (MV-1..MV-7), EFF local `f1600fac`. La ruta del repositorio de origen
  debe ser absoluta: el arnés hace `git -C <clon> fetch <origen>`, y con `.` leería el propio clon.

## Compatibilidad: C-20b y C-20c (MC; se repiten sobre el SHA final de F4 y sobre el merge local de la integración)

- **C-20b** (`compat/clause_map.py check <base> <tip>`): el mapa custodiado es igual a la derivación MV-2..MV-6 entre `origin/main`
  (`bb0d5522`) y la punta. Resultado en `compat/c20b-result.json`. Los negativos de mapa (MV-3 sin un archivo modificado, MV-4 `BaseBlob` distinto,
  MV-5/N-c entrada duplicada, MV-6/N-d sección cambiada no listada, N-e sección igual listada como MODIFIED y MV-6 sin la ENTRY de WORKFLOW) corren
  dentro de C-20c y dan MAP_INVALID.
- **C-20c** (`compat/c20c.py <repo> <out>`, port del prototipo `I-62-prep/f4/compat-proto` sobre los **textos reales**). Usa un clon desechable con
  la historia real; EFF es el merge local de la punta más un commit vacío **local** con el trailer normativo, que nunca se publica, y M2 = EFF + X1 + X2 + X3.
  Cada caso ejecuta `Evaluate` desde E1, y el arnés obtiene 16.13 del punto de entrada y el mapa, su esquema y las superficies del texto de 16.13.
  Resultado en `compat/c20c-result.json`.
  C-20c-1 (a) PRE_ACTIVATION y (b) I61 con la tabla de E.6 cita por cita. C-20c-2 (I-64, contrato real con `Path` y `Section` idénticos) da I61, más el
  positivo de WORKFLOW «documento completo» con X3. N-a..N-s y los tres negativos de mapa de C-20b: **27/27**.
- **Lecturas compuestas, unidad por unidad** (C-20c-2, filas 10 y 11): en README y `routing.md`, todo `##` de EFF^1 se lee en `MainSha_eval` y las
  secciones solo I62 se omiten. Hay 8 diferencias con el literal de E.6 (README §§1, 3, 5, 6 y 11; routing §§4, 5 y 7, que E.6 lee en EFF^1). Todas
  derivan de la diferencia E.5/derivación de la tabla siguiente: esas secciones no están MODIFIED en el mapa derivado.

**Mapa previsto (E.5) frente al derivado** (C-20b; la clasificación es una propuesta para la revisión del Coordinator):

| Archivo | Previsto (E.5) | Derivado | Propuesta |
|---|---|---|---|
| `docs/AUTOMATION_PLAN.md` | MODIFIED `## 8.` (`/v2`), `## 16.` (ancestro), 16.1, 16.3 (puntero), 16.4-16.9, 16.11 y 16.12 «donde cambian»; ADDED subsecciones I62; ENTRY 16.13 | MODIFIED `## 16.`, 16.1, 16.3 y el `#` del documento; ADDED 16.14-16.24; ENTRY 16.13 | `## 8.` llega con F4-H (formato `/v2`) y se compara de nuevo. 16.4-16.9, 16.11 y 16.12 no cambian: la previsión era condicional («donde cambian») |
| `docs/WORKFLOW.md` | MODIFIED `## 4.` y `## 10.`; ENTRY `## 12.` | igual (más el `#` del documento) | igual |
| `docs/INITIATIVE_LIFECYCLE.md` | §5/§9 solo con OD-6 alternativa 1 | MODIFIED §5 y §9 | igual |
| `docs/initiatives/PROMPT_TEMPLATES.md` | `## G.` si cambia; `## 2.` | MODIFIED `## 2.`; `## G.` sin cambio | igual |
| `agent-execution/README.md` | MODIFIED §§1, 3, 5, 6 y 11; ADDED secciones I62 | solo el `#` del documento; ADDED §§12-16 y sus subsecciones | **diferencia**: F1 aplazó esas modificaciones sin contenido (decisiones §32.5), y ningún texto congelado fija un cambio en ellas; el contenido I62 está en secciones nuevas (E.7). Propuesta: la previsión no se materializa. No es una corrección (la materialización no se aparta de un texto congelado) ni una A-n (el Freeze no queda incompleto). Decide el Coordinator |
| `agent-execution/routing.md` | MODIFIED §§4-5 y §7; clase nueva en sección nueva | solo el `#`; ADDED §8 y §9 (F2, con GATE PASS) | **diferencia**: misma propuesta que README |
| `agent-execution/model-catalog.md` | secciones nuevas (E.7) | sin cambio | **diferencia**: los requisitos por perfil y acción están en `routing.md` §8 (F2). Misma propuesta |
| adapters, esquemas `/v2` y nuevos | ADDED | ADDED (19 archivos) | igual |
| `compatibility/clause-map.schema.json` | ENTRY | ENTRY | igual |
| esquemas `/v1`; `AGENTS.md`, `CLAUDE.md` | ninguna | ninguna | igual |
| `docs/adr/`, `docs/FOUNDATIONS.md` | índice y entrada en el cierre documental; ADR sucesor ADDED | ADR-0048 ADDED | el índice y FOUNDATIONS llegan con el cierre documental |

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
  `unverified_commits[].sha` o el `chain_red_sha` de la tarea de la ventana), lo que equivale a la cadena `BaseSha..CurrentSha` de I-61, y si no es
  un commit de la sesión. Para eso se usa la definición congelada de 16.1 y B.8.6: un commit de la sesión solo toca `docs/automation/` o los documentos
  de la unidad. El MC de C-15 mostró que el primer criterio solo no basta: un commit de la sesión anterior a los del Worker es ancestro del GREEN. La ventana
  empieza en el Q0, o en su imagen cuando el rebase de la propia ventana lo reescribió (I-P12). Las dos reglas aplican solo con p = Q0: fuera de una
  ventana no hay commits del Worker, y un par de reconciliación no puede contar la imagen de p como escritura del Worker.
- **F4-OBS-08 — posición de 16.13.** E.1 dice «al final de §16» con la numeración anterior a F1. Las decisiones §32 (punto 4, aceptadas con el GATE PASS
  de F1) reservaron 16.13 y numeraron las partes I62 desde 16.14. Por eso 16.13 va entre 16.12 y 16.14. Su identidad es la línea de encabezado exacta
  (E.3.0, E3), no la posición.
- **F4-OBS-09 — redacción de WORKFLOW §4 y §10.** V14 §3 fija el contenido («solo referencias: paso “al abrir” → autoverificación de §16 para unidades
  I62_DELEGATED»; «texto de la fila ampliado» con el delta «obligaciones de la sesión principal fuera de una delegación»), pero no el literal. La
  redacción es mínima e inactiva («desde `I62_EFFECTIVE_SHA`»). Sus dueños son WORKFLOW y el Owner (OD-1, antes de READY-03).
- **F4-OBS-10 — P-15 en 16.13.** 16.13 define P-15 con el texto exacto de la fila de V14 §13, igual que las subsecciones F3 definen sus códigos P.
- **F4-OBS-11 — alcance de la derivación de `NextAction`.** V14 fija la regla de unicidad (§20.4, I-S18, P-17) y algunos pares rol/acción, pero no un
  vocabulario completo de acciones. La derivación solo determina lo congelado: la escalada nombra el rol que decide; CORRECTING es
  PRINCIPAL / CORRECT_AND_REREVIEW sobre `loop.object`; una fase pendiente con un intento reservable invoca al revisor del bucle sobre el objeto de su
  solicitud, con la autorización vigente como `invocation_permission` y el contrato de salida de su rol (16.23). Dos determinaciones con roles
  distintos son P-17. Los demás estados dejan libre el `next_action` guardado dentro de las otras reglas de I-S18.
- **F4-OBS-12 — P6 y los estados inesperados.** El prototipo solo marcaba Failed y NotExecuted sin motivo. La orden exige distinguir Passed,
  Failed, omitidas y estados inesperados: cualquier otro `outcome` (Timeout, Aborted, Inconclusive, Error…) falla y se informa aparte.
- **F4-OBS-13 — plantillas literales de los marcadores (16.28).** V14 §8.6 manda materializar en F4 el texto literal de los marcadores. Siguiendo
  F4-OBS-03, cada decisión va en un bloque cercado con la línea exacta del marcador y campos `Clave: valor`. Para la entrada de G0 se usan `Claim-Id` y
  `BootstrapRecordVersion`, porque V14 exige el `Claim-Id` y la `record_version` del BOOTSTRAP. Los demás nombres de campo vienen de V14 §20.5 y de A-1.
- **F4-OBS-14 — numeración de los textos de F4.** El dossier de F4 preveía 16.25-16.29. Se añade 16.30 (planos y MaterializationClose, V14 §15), porque
  MaterializationClose es una regla I62 que generaliza el cierre de G2 sin tocar 16.3 (C-21). Las previsiones de E.5 para README §§1, 3, 5, 6 y 11 siguen
  sin materializarse: el contenido I62 está en secciones nuevas (§17 y §18), clasificado en C-20b.
- **F4-OBS-15 — esquema `/v2` emitido desde el validador.** El esquema no se escribe a mano. Es la emisión del árbol declarativo con el que el validador
  comprueba la forma, en el subconjunto que valida `MiniJsonSchema`. La guarda exige igualdad exacta, así que el contrato y el validador no pueden
  divergir. Al escribir la guarda apareció un defecto real de `MiniJsonSchema`: un entero creado como `long` no contaba como número y `minimum` no se
  comprobaba. Se corrigió.
- **F4-OBS-16 — lectura del diario para `DS`.** B.8.2 nombra la aceptación de `d`, la verificación válida y el cierre declarado sin fijar su forma en
  `relay-record/v2`. Se leen así: aceptación = registro PLANNING cuyo `Outcome.Acceptance` tiene A1..A8 en `pass` (donde README §4 anota la aceptación);
  verificación válida = registro VERIFICATION con `Outcome.Kind` COMPLETED; cierre declarado = registro posterior a la aceptación con `Disposition`
  BLOCKED o STOP (la sesión detiene la ventana y la devuelve al Coordinator). Para el Q7, un lanzamiento cuenta salvo `REJECTED_BEFORE_INVOCATION`, y
  `LAUNCH_UNCERTAIN` cuenta como incierto (§9.3).
- **F4-OBS-17 — P-19 y §20.5 en la ingestión.** Al escribir el MC de C-35 apareció un hueco del validador de F4-D. Un resultado INVALID podía abrir
  linajes, y la ingestión podía publicar una fase distinta de la que da el veredicto. El texto congelado lo prohíbe: P-19 («no avanza»), el diagrama de
  §20.5 (AGREED → ARCHITECT_SATISFIED; CHANGES REQUIRED → CORRECTING; BLOCKED — OWNER DECISION → ESCALATE_OWNER; INVALID → reejecución) y §20.5.2 (un
  linaje nace de un hallazgo de un resultado con autoridad). Se añadieron como cláusula P-19 de I-P13, con positivos y negativos en
  `I62F4OrchestrationMcTests`. Es un defecto localizado de la implementación, corregido dentro de F4; no cambia ninguna semántica congelada.
- **F4-OBS-18 — fidelidad por intento y hallazgos UNACCREDITED.** Al escribir C-42 (5) y (8) aparecieron dos huecos de la misma clase que F4-OBS-17. Uno:
  un hallazgo UNACCREDITED podía abrir un linaje. V14 §20.3.3 dice que «no abren, cierran ni sustituyen linajes» (cláusula P-25 de I-P13). Otro: un
  intento nuevo podía reutilizar la evidencia de fidelidad del anterior, y «cada intento lleva la suya» (I-S18: el `InvocationId` del registro es el de la
  invocación del intento). Ambos se corrigieron dentro de F4.
- **F4-OBS-19 — obligaciones del cierre de insumos (C-41).** El cálculo lee las obligaciones solo de las secciones normativas de forma fija: AGENTS.md
  «Leer primero», CLAUDE.md «Lectura inicial» y «Comandos esenciales», y `required_docs` / `optional_docs` de un Context Pack. Un enlace es READ; un
  comando `git` de solo lectura es ACTION_COMPATIBLE; otro comando, con permisos READ_ONLY, es ACTION_INCOMPATIBLE; `optional_docs` es
  CONDITIONAL_NOT_TRIGGERED. El runtime no infiere obligaciones de otra prosa.
- **F4-OBS-20 — C-40 (n): el instante de arranque observado.** V14 §20.5.1 decide el destino de un intento en LAUNCHING al terminar la vigencia por
  la evidencia del invocador frente a `ended_utc` (operación 7, PID y `CreationDateUtc`). Ningún campo congelado de `relay-record/v2` ni de B.8.8 fija
  ese instante. Por eso la clasificación (completar o UNAUTHORIZED_LAUNCH) es del procedimiento del invocador (README §18.4). El validador hace
  cumplir sus consecuencias: un UNAUTHORIZED_LAUNCH no abre ni cierra linajes, no satisface y no avanza la fase (P-19). No se reclama una comprobación
  mecánica de la clasificación.
- **F4-OBS-21 — README §18.4.** El procedimiento cubría el no arranque con la vigencia terminada, pero no los otros tres destinos de V14 §20.5.1. Se
  completó con el texto congelado (arrancó antes, no arrancó, indeterminado, arrancó después → UNAUTHORIZED_LAUNCH y STOP P-20) y se regeneró el
  mapa de cláusulas (EffBlob de README). Es una corrección localizada de F4; DC-07 previo sin cambios en los hermanos.
- **F4-OBS-22 — lectura de C-32.** `OWNER_AS_MESSAGE_BUS` se lee de la custodia: es true si algún punto custodia un AUTONOMY_GAP con `RelayedBy`
  OWNER. Todo relevo manual tiene su registro (P-21); sin él la evidencia ya es inválida. Un relevo del Coordinator es AUTONOMY_GAP (C-37, el
  criterio 15 no es PASS), pero no convierte al Owner en el bus. La exigencia de FX-06 de no tener decisiones intermedias del Coordinator es de F6.
- **F4-OBS-23 — lectura de «hereda los contadores» (C-16).** §9.3 y 16.27 no fijan la forma. El conteo por clase sigue la cadena de continuación
  declarada en `task_intent` y `chains[]` (`ContinuesTaskId`, transitiva). Una continuación de sí misma, de una tarea sin cadena o en ciclo no puede
  heredar y es S-04 (contador ausente).
- **F4-OBS-24 — fase de un bucle EXECUTION.** B.8.8 dice «la fase de §8 (ejecución)» sin enumerarla. El validador no restringe ese valor, y las
  trazas de A-1 usan un valor simbólico como el arnés. Sí aplica las reglas de A-1 al tipo: sin `instance_id`, sin LOOP_CLOSED (D1-13) y con la
  reconciliación de `loop.object` en un rebase (D2-4).
