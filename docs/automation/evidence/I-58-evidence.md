# I-58 — Evidencia canonica

Vigencia: la seccion C1/V2 al final actualiza el estado; registros D/F0 V1 anteriores son historia, no acuerdo vigente.

Unit / Initiative: I-58 / I-58
Workflow: V2, T4; FOUNDATION EVOLUTION propuesto
Claim-Id: a17726b2-dee1-44b6-a3d7-3366dc4a21b4
BASE_SHA: 7097057cf8685bf5ecc09083cba37379d4a4aae8
CLAIM_SHA: e59bfda6c7e6696f0eafdc2e99797f9957553338
Branch: architecture/shared-view-authored-comparators
Worktree: C:/Users/alejandra-mendoza/.codex/worktrees/architecture-shared-view-authored-comparators
Primer push sin force aceptado: 2026-09-23T03:00:50Z (registro de salida de la sesion).

## Preflight previo al claim — MEASURED

Fetch --all --prune ejecutado y repetido inmediatamente antes de crear worktree.
origin/main = main local = BASE_SHA. Main limpio. Stashes: ninguno.
Worktrees previos: principal/main; feature-creacion-de-vistas; feature-rackmirror-espejo-semantico.
Los tres limpios; sin MERGE_HEAD, REBASE_HEAD, CHERRY_PICK_HEAD, REVERT_HEAD, BISECT_LOG,
rebase-merge, rebase-apply ni sequencer. Ninguna rama/tag/commit de I-58 previo localizado.
Inventario remoto completo: main=BASE_SHA; feature/creacion-de-vistas=08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d;
feature/rackmirror-espejo-semantico=46a2e15f529d30d65f9524f0ade98d72da980237.
Inventario local identico, mas origin/HEAD -> origin/main. I-55: G11 publicado; G12 fuera del gate,
ver commit de punta y documento I-55-g11-id18-batch-contract.md. Contrato antiguo con G7 no sustituye Git.

integration/I-57 local y remoto: tipo tag (anotado), objeto a5bc02210f740839e2fac37e63fc00512e5ccee6;
target 7097057cf8685bf5ecc09083cba37379d4a4aae8, ancestro de origin/main (exit 0).
R3 blob exacto cd42db03becff42f98b047e61c46689c17a69670; EFFECTIVE por ADR-0044 Aceptacion,
decisiones I-57 (triple registro) y recibo integration/I-57; encabezado historico no se reescribe.

Workflow V2 efectivo: 8a021fb67c16dfccd6afc18448ea7e6a71a32364, en first-parent de origin/main.
Marcador unico afd1077cba90ace890ac253bbe34a234b6691540: alcanzable por segundo padre (exit 0),
no por primero (exit 1). integration/I-56 registra END de pausa 2026-09-17T22:30:00Z.
Claim posterior, base incluye SHA efectivo; T4. Encabezados historicos no desactivan el merge normativo.
SDK resuelto: 8.0.423. Core local de base iniciado, resultado pendiente; no se afirma Full.

## Identidades y rondas

Discovery/Freeze draft: pendientes; ningun Freeze aprobado ni A-n.
FINAL_CANDIDATE_SHA: NOT DECLARED. Builds, CI de Candidate, cobertura: NOT APPLICABLE a este bootstrap.
Conformance: NOT REQUESTED. Owner Validation: asignacion en draft, no ejecutada.
Metrics: duracion Owner UNKNOWN; sin mediciones inferidas.
Integration tag I-58: inexistente / no solicitado.

IMPLEMENTATION AUTHORIZATION = NO

## Baseline y alcance de pruebas

Core local BASE_SHA, main limpio al arrancar, SDK 8.0.423: 8236 PASS, 0 FAIL, 0 SKIP;
wall-clock 213.0769571 s; runner 2 m 26 s; exit 0. Log I-58-base-core.txt.
No se atribuye esta corrida a hijos documentales. UI Full no ejecutada en este preflight; no se declara
Full combinado ni Candidato. Warnings xUnit preexistentes quedan registrados en el log, no corregidos.

BOOTSTRAP_SHA: fc633365523e1907fd50482107d6d286f67cc6e8; bootstrap versionado antes de Discovery.

## Paquete D/F0 / preparacion F1

- Discovery: docs/initiatives/I-58-discovery.md (DC-01..09, matriz authored/schema, M/EXP).
- Freeze completo: docs/initiatives/I-58-freeze-draft.md, DRAFT V1, Frozen NO.
- Anexo congelable: docs/initiatives/I-58-characterization.md.
- Architect: docs/initiatives/I-58-architect-review-package.md; veredicto NO emitido.
- Inventario: I-58-source-inventory.txt (busqueda textual de lectores y arboles, no equality).
- Probe: I-58-probe/Program.cs + I58.Probe.csproj + README.md, fuera de la solucion.

Primer probe de trabajo: fixture Dynamic sin Modules fallo en setup; descartado como RED.
Fixture corregido: 68 escenarios seleccionados, 32 expectativas incumplidas (Single/Divergent),
36 Unreadable/null observados; exit 1, 5.8915302 s. Arbol con artefactos sin commit: diagnostico de
trabajo, NO evidencia local exact-SHA reutilizable. Se repetira tras commit limpio para identidad publicada.
No hubo fix productivo; no hay GREEN de cuatro comparadores. PASS Unreadable no demuestra reader.

Lifecycle: Owner autoriza characterization cuando proceda; probe diagnostico D/F0 sin implementacion,
no se declara F1 abierto ni gate funcional cerrado. Coordinator pre-review y EXP positivas pendientes;
Architect review pendiente. Agreement requerido sobre Freeze + anexos exactos antes de implementacion.

Conformance: NOT REQUESTED / no Candidate. Owner Validation: plan/asignacion en Freeze §8; no ejecutada.
Metrics: Owner active time UNKNOWN; gate F1 time UNKNOWN; defects escaped UNKNOWN. No ahorro inferido.
Integration tag I-58: NOT CREATED. Consumer I-55 G12: NOT UNLOCKED BY THIS DELIVERY.

IMPLEMENTATION AUTHORIZATION = NO

## Evidencia publicada de D/F0 (no Candidate ni cierre de iniciativa)

PACKAGE_SHA / PROBE_VALIDATION_SHA: 33c553d96a70c5edad3c683fe9e2aaf4e78c0a03.
Antes y despues del probe: `git status --porcelain` vacio. SDK resuelto 8.0.423.
Comando: `dotnet run --project docs/automation/evidence/I-58-probe/I58.Probe.csproj`.
Resultado: exit 1 esperado, 68 seleccionados, 32 RED de Single/Divergent y 36 Unreadable/null;
17.6117919 segundos de wall-clock. No fallo de build/setup. Log: I-58-probe-observed.txt.
El log repetido es identico al ya publicado en PACKAGE_SHA; se verifico por hash de archivo.

| Kind | Seleccionados | RED | Unreadable/null | Root payload unknown preservado por store | Nested unknown preservado |
|---|---:|---:|---:|---|---|
| Dynamic | 17 | 8 | 9 | NO | NO |
| PushBack | 17 | 8 | 9 | SI | NO |
| Cantilever | 17 | 8 | 9 | SI | NO |
| Cabecera | 17 | 8 | 9 | NO | NO |

Cantilever IntervalCountSerialized=True observado. Estos hechos son de stores ACTUALES, no del reader futuro.
La preservacion root de PushBack/Cantilever no implica preservacion del arbol. No hubo GREEN funcional ni
implementacion de comparator. El probe diagnostico no acredita cierre F1 ni sustituye RED->GREEN definitivo.

## Ultimo preflight y publicacion

Fetch --all --prune repetido 2026-09-23T03:16:21Z: origin/main sigue BASE_SHA; main local limpio e identico.
I-55 sigue 08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d. I-52 avanzo independientemente a
e4e506303f81370db75e8e7e8bf1000c0e7920c3; diff del comparator y Persistence contra main sigue vacio.
No se modifico esa rama. integration/I-57 sigue objeto/target esperados; R3 mismo blob exacto.
Diff acumulado contra base en src/tests/assets/eng/.github/deploy = VACIO. Main no se modifica.
`git diff --check BASE HEAD` = PASS. Docs propias y fila propia ROADMAP son el unico delta, mas probe documental.

CI del CLAIM_SHA: run 35812632578, event=push, ref=refs/heads/architecture/shared-view-authored-comparators,
head_sha=e59bfda6c7e6696f0eafdc2e99797f9957553338, jobs requeridos 4/4 success (consultados).
Esta evidencia pertenece SOLO al claim. Bootstrap/package no heredan esa CI. El push agrupado siguiente
correra solo sobre su punta: no se atribuye retrospectivamente verde a PACKAGE_SHA.
CI de la punta de entrega: pendiente al preparar este registro; no se declara gate cerrado ni Candidate.

## Identidades exactas de revision

Version completa para revision: PACKAGE_SHA, rutas y blobs:

| Artefacto | Blob |
|---|---|
| docs/initiatives/I-58-freeze-draft.md | 5b528fcca0b5f41e35acbc0a32f4b595179ae978 |
| docs/initiatives/I-58-discovery.md | 18bfec73e1eaf53cb179d26bdbd6e8b2251753a6 |
| docs/initiatives/I-58-characterization.md | aa94e5688b312a10ce9a1a5f3425f0da56622c8f |

Los dos acuerdos deben identificar este Freeze exacto y sus anexos, o pedir una version completa nueva.
No se emitio acuerdo ni se cambio Frozen. Los blobs de TODOS los artefactos de entrega (incluido este archivo)
se registran en el cuerpo del commit de publicacion calculados desde el index; asi se evita auto-hash imposible.
`git show <DELIVERY_SHA>` contiene el manifiesto; `git ls-tree -r <DELIVERY_SHA>` verifica cada entrada.
La evidencia local anterior sigue atribuida a BASE_SHA o PACKAGE_SHA, nunca a este hijo documental.

Coordinator = PENDING
Architect = PENDING
F0 = PREPARED FOR REVIEW (no consenso declarado)
F1 = PREPARED / NOT OPEN AS FUNCTIONAL GATE
I-55 G12 CONSUMER UNLOCK = NO
FINAL_CANDIDATE_SHA = NOT DECLARED
IMPLEMENTATION AUTHORIZATION = NO

## C1 / V2 — continuacion correctiva

HEAD_INICIAL: 49fa607cde5635fc58f4a5e78b22fd6e1a46b7ea.
Mismos Claim-Id/BASE/CLAIM/rama/worktree; no claim ni worktree nuevos. Fetch --all --prune realizado;
origin/main y main local permanecen 7097057cf8685bf5ecc09083cba37379d4a4aae8, limpios. I-58 inicial
igual a upstream, limpio; sin stashes ni operaciones merge/rebase/cherry-pick/revert/bisect/sequencer.
I-55 observado 08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d; I-52 e4e506303f81370db75e8e7e8bf1000c0e7920c3.
integration/I-57: mismo objeto tag a5bc02210f740839e2fac37e63fc00512e5ccee6 y target BASE_SHA.
R3 mismo blob cd42db03becff42f98b047e61c46689c17a69670. No se modifican.

V1 revisada en HEAD_INICIAL: Freeze blob 5b528fcca0b5f41e35acbc0a32f4b595179ae978;
characterization aa94e5688b312a10ce9a1a5f3425f0da56622c8f; Discovery 18bfec73e1eaf53cb179d26bdbd6e8b2251753a6.
CI HISTORICA VERIFICADA con gh run view: run 35813721629, event=push,
ref=refs/heads/architecture/shared-view-authored-comparators (headBranch exacta),
head_sha=49fa607cde5635fc58f4a5e78b22fd6e1a46b7ea, completed/success, requeridos 4/4 SUCCESS:
Tests (Domain + Application); UI Tests (WPF controls, net8.0-windows);
Build UI (WPF, valida API de Application); Build Plugin without AutoCAD.
No se atribuye esa CI a ningun SHA nuevo ni a los padres de la publicacion V1.

C1 diagnostico: [I-58-c1-identity.md](I-58-c1-identity.md) y [fuentes exactas](I-58-c1-source-identities.txt).
PROBE_VALIDATION_SHA: 5d69a0670a6576fe344a983c45c6187a5bb280ec; arbol limpio antes/despues,
SDK 8.0.423. Probe 4 escenarios, exit 0; Core focal 66/66 y UI focal 28/28, todos sin omitidos.
Comandos (desde worktree):

```powershell
dotnet run --project docs/automation/evidence/I-58-c1-probe/I58.C1.Probe.csproj
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter 'FullyQualifiedName~SharedViewFoundationF5Tests|FullyQualifiedName~SharedViewFoundationF6Tests|FullyQualifiedName~CantileverPersistenceAndViewTests' --logger 'trx;LogFileName=I58-c1-core.trx' --results-directory "$env:TEMP/I58-c1-results"
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj --filter 'FullyQualifiedName~CantileverEditorWindowTests|FullyQualifiedName~RackEditorSessionTests' --logger 'trx;LogFileName=I58-c1-ui.trx' --results-directory "$env:TEMP/I58-c1-results"
```

Duraciones/limites/fallos setup descartados en diagnostico. Log bruto de cada corrida exitosa versionado.
No Full ni Candidate; sin validacion Owner/AutoCAD. No hay GREEN de comparadores ni se abrio F1.
La evidencia permanece en PROBE_VALIDATION_SHA; hijos documentales no la heredan.

CR58-01: CORRECTION PROPOSED / emisor debe revisar; EXP-01 A OPEN STOP efectivo con B propuesto SOLO
para I-58 PENDING COORDINATOR + ARCHITECT CONFIRMATION. D58-CANT-ID se registra fuera de alcance.
CR58-02: CORRECTION PROPOSED / emisor debe revisar; TAuthored dominio separado de canonical interno,
materializacion fiel y seam AUTH13->AUTH09 explicita. DC-09 M-02/04/05/06/08 activos, M-01/03/07 no activos
bajo limites V2. EXP-01..09 y razones individuales en Discovery. Ningun REQUIRED auto-cerrado.

Paquete completo V2: Freeze draft, Characterization, Discovery corregido y paquete Architect actualizado.
Identidades exactas del commit de contenido V2 se registran en la siguiente publicacion y en su mensaje;
no existe Freeze aprobado, A-n, Candidate, conformidad, cierre de iniciativa, merge ni tag integration/I-58.

Coordinator = CHANGES REQUIRED ON V1 / REVIEW REQUIRED ON V2
Architect = PENDING
F1 = NOT OPEN
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = NO
