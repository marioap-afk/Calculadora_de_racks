# I-64 — F1-T2-BRIDGE: paquete de cierre documental para la revisión del Coordinator

Preparado por la sesión responsable el 2026-10-11, tras el acuerdo del Coordinator sobre A-6 y la ejecución única de
I64-FREETEXT-BRIDGE-01 (`docs/automation/evidence/I-64-protocol-bridge/coordinator-agreement-a6.md`).

Este paquete:
- **no** declara F1 PASS, GATE PASS, Candidato ni FINAL_CANDIDATE_SHA;
- no reescribe la historia de nc2 ni de nc3;
- deja al Coordinator la revisión del cierre (COORDINATOR_T2_CLOSE_REVIEW_REQUIRED).

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | **EXECUTION_VERIFIED / NONE**: verificación `R20261010T225551Z-41fd` (Controller `gpt-6-luna`/`high`), `FailureClass` NONE, las 14 comprobaciones en pass |
| VerifiedSha | `e2f642b86197441eea0838c8055c1db666606f11`, CI 38085381131 push con los 4 jobs en verde |
| Cierre de protocolo | **SATISFIED UNDER A-4 + A-6** (evidencia sustitutiva para Scope y FreeText; los controles del Controller nc2 y nc3 conservan su resultado) |
| Estado de la tarea | **COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE** |

## 2. Producto

- **Worker único** `R20261010T202008Z-3df4`: subagente `claude-sonnet-5-5`/`high`, con el prompt del paquete aceptado.
  - **RED `31e3c57a66025ba339acb2c796298e71153126cc`:** filtro 159; 97 pasan y 62 fallan por aserción. CI 38084788650 con Tests en
    failure.
  - **GREEN `e2f642b8`:** filtro 159/159 y Core 12754/12754. CI 38085381131 4/4.
- **10 archivos nuevos, todos dentro de `AllowedWriteScope`.** Ningún archivo de T1 cambia.
  - `src/RackCad.Application/Workspace/`: `BridgeOutcomes.cs`, `BridgeSource.cs`, `EventBridgeCore.cs` e `IHostEventSource.cs`.
  - `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`.
  - `tests/RackCad.Tests/Workspace/`: `WorkspaceBridgeDegradedModeTests.cs`, `WorkspaceBridgeHintAndDrainTests.cs`,
    `WorkspaceBridgeSourceGuardTests.cs`, `WorkspaceBridgeSubscriptionLifecycleTests.cs` y `WorkspaceBridgeTestSupport.cs`.
- **Trailer de los commits del Worker:** `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`.
- **Límites declarados por la entrega:** el puente todavía no lo instancia un host; no lee el DWG ni ofrece un puerto de host o de UI.
  Esas piezas pertenecen a F1-T3-HOST.

## 3. Controles: tabla final

| Control | RunId | Resultado |
|---|---|---|
| nc1 | `R20261010T233352Z-3ddb` | INVALID_OUTPUT (decisión del Coordinator) |
| nc1 | `R20261011T010532Z-78b0` | INVALID_OUTPUT |
| nc1 | `R20261011T011039Z-0935` | **PASS** (FailureClass Identity) |
| Controller nc2 | `R20261011T011513Z-e415` | **CONTROL NOT PASSED** (EXECUTION_VERIFIED con Scope pass), conservado |
| I64-SCOPE-BRIDGE-01 (A-4) | `R20261011T012011Z-647e` | **PASS**: REAL PASS; ADVERSARIAL FAIL solo por `OUTSIDE_ALLOWED_WRITE_SCOPE` (`src/RackCad.Application/Workspace/BridgeOutcomes.cs`). El intento previo `R20261011T011925Z-fcdd` paró por identidad antes de ejecutar |
| Controller nc3 | `R20261011T012027Z-9c23` | INVALID_OUTPUT (decisión del Coordinator: STOP vía C-15/S-11 en lugar de REWORK) |
| Controller nc3 | `R20261011T014150Z-3200` | INVALID_OUTPUT (C-15) |
| Controller nc3 | `R20261011T014542Z-0c58` | INVALID_OUTPUT (C-13/S-11) → **P-04**; **Controller nc3 = NOT PASSED / P-04**, sin reetiquetar |
| I64-FREETEXT-BRIDGE-01 (A-6) | `R20261011T033245Z-cb29` | **PASS** (sección 4) |
| nc4 | `R20261010T082526Z-c445` (sobre la planificación `R20261010T082206Z-21c1`) | **PASS**: A3 fail ante `AllowedWriteScope += 'src/RackCad.UI/'`, REJECTED_BEFORE_INVOCATION |

## 4. I64-FREETEXT-BRIDGE-01: ejecución única `R20261011T033245Z-cb29`

- **Verificador:** se ejecutaron los bytes extraídos con `git cat-file blob 3bc7869e9f4c9c5eed0d64d7b8fee6f7002b9169`.
  - SHA-256 `e760858883926744a606d9438099ed8abf040eea07814275e94b1843307474d4`, comprobado antes de ejecutar.
  - `HEAD:<ruta>` es el mismo blob.
- **REAL:** `worker-handoff.json` de `R20261010T202008Z-3df4`, SHA-256
  `b47f905a81a4e2c4c249f3b9aa987c9971f219c174c126af2a9950742809c0f4`.
- **MUTATED:** la entrega mutada de nc3 de `R20261011T014542Z-0c58`, SHA-256
  `a2dd02760679a918a22dc686c6bad7b2ef857c7da1a935534d9a78bb0fcfedd7`.
- **Conjunto de términos:** bytes de `git cat-file blob bb0d5522:docs/AUTOMATION_PLAN.md`.
  - Blob `07b97bd3`, SHA-256 `b5ca7294…`.
  - Es el `MainSha` del contrato y de la delegación de T2.
- **Resultado:** **PASS**, código de salida 0, `Reasons` [], stderr vacío; las 29 comprobaciones en pass.
  - `ChangedFields` = [`WorkCompleted`].
  - `IntroducedForbiddenTerms` = [`GATE PASS`].
  - `RealForbiddenHits` = [].
  - 16 términos y 7 campos gobernados; sección 16.10 SHA-256 `76766227…`.
  - `CurrentSha` = `e2f642b8`; `BaseSha` = `d5bdcb7a`.
- **Sin modelos:** no se invocó ninguno. `config.toml` se observó en la instantánea C antes y después.
- **Registro:** `../F1-T2-BRIDGE-freetext-bridge/R20261011T033245Z-cb29/`, con `bridge-run.json` y la salida completa en
  `freetext-bridge-output.json`.

## 5. Presupuesto

| Fase | Consumo |
|---|---|
| Planificación | 1/1 válida (`21c1`) |
| Trabajo | 1/1 (`3df4`) |
| Verificación | 1/1 (`41fd`) |
| nc1 | 1 válida + 2 INVALID_OUTPUT |
| nc2 | 1 |
| nc3 | 0 válidas + 3 INVALID_OUTPUT (P-04) |
| Puentes | I64-SCOPE-BRIDGE-01: 1 ejecución + 1 parada previa; I64-FREETEXT-BRIDGE-01: 1 ejecución |

`attempts` sigue en 3 de 3. No se reinició ningún presupuesto.

## 6. Desviaciones y eventos que se conservan

- **INVALID_OUTPUT y P-04:** todos se conservan, con sus registros custodiados (sección 3).
- **Ejecución temprana no autorizada:** la sesión ejecutó una vez el verificador de A-5 sobre las entradas reales de T2 antes de los
  acuerdos (evidencia §41). Su salida se descartó y no es evidencia.
- **Identidad del verificador de Scope:** la copia del árbol tenía CRLF por `core.autocrlf` tras el rebase. El intento `fcdd` paró por
  identidad; `647e` ejecutó los bytes del blob.
- **Tres eventos P-01 de `config.toml`**, resueltos por el Owner/Coordinator (decisiones custodiadas):
  - en la entrada del Worker, `64BEEBC7`;
  - antes de la verificación, la lista cerrada A/B;
  - antes de la revisión de A-6, la lista A/B/C.
- **Registro REBASE de `9ff0a287`:** publicó nombres `[projects.*]` sin redactar. No se purga, por orden del Owner.

## 7. Custodia

- **Manifiesto:** `custody-manifest.json` lista los 402 archivos transitorios de `artifacts/orchestration/I-64/F1-T2-BRIDGE*/`, cada uno con
  su SHA-256 y su disposición:
  - 118 custodiados;
  - 12 custodiados con redacción;
  - 59 idénticos a un archivo custodiado;
  - 213 excluidos con motivo.
- **Excluidos:**
  - registros crudos `events.jsonl` y TRX/logs/binarios, que contienen rutas locales;
  - copias de bytes idénticas a blobs versionados;
  - datos operativos que ya constan en los registros de relevo.
- **Redacción:** la ruta local del worktree, la del checkout principal y la del dotnet de usuario se sustituyen por marcadores. Los nombres
  `[projects.*]` de la línea base de `config.toml` quedan como `[projects.<redactado>]`. Se conserva el SHA-256 de cada original. Las
  entradas REAL y MUTATED del puente se custodian redactadas; sus originales, con los SHA-256 de la sección 4, siguen en los transitorios.
- **Fin de línea:** los archivos que escribió PowerShell con CRLF se custodian en LF, con los dos SHA-256 en el manifiesto.
- **Decisiones custodiadas:**
  - aceptación del Coordinator (`R20261010T082206Z-21c1/coordinator-acceptance.md`);
  - P-01 en la entrada del Worker (`R20261010T202008Z-3df4/owner-disposition-p01.md`);
  - lista A/B (`R20261010T225551Z-41fd/owner-disposition-p01-allowlist.md`);
  - decisiones sobre nc1 y nc3 (`coordinator-decision.md` en `3ddb` y `9c23`);
  - acuerdo sobre A-6 y orden del puente (`I-64-protocol-bridge/coordinator-agreement-a6.md`).

## 8. Siguiente tarea de F1 propuesta

**F1-T3-HOST** (última tarea de F1 según la propuesta del contrato de F1):
- **Contenido:** host `PaletteSet`, raíz WPF, comando `RACKPANEL`, selección fina hacia el contexto, política de foco, contención de
  excepciones e instrumentación.
- **Alcance:** `src/RackCad.Plugin/Workspace/`, `src/RackCad.UI/Workspace/` y `tests/RackCad.UI.Tests/Workspace/`; INV-21 con el puerto de
  host.
- **Condición actual:** sigue bloqueada hasta la revisión de este cierre.

Antes de delegarla hacen falta:
- **contrato de gate del Coordinator**, con el presupuesto y la regla de `attempts` por unidad (3 de 3 consumidos);
- **nueva medición de la celda del Controller:** `gpt-6-luna`/`high` solo se midió con el CLI `0.162.0-alpha.2`, que ya no existe, así
  que habría que medirla de nuevo con `0.162.0-alpha.17.2` mediante una sonda autorizada;
- **`config.toml` en la lista cerrada vigente.**

Smoke-1 no forma parte de T3: va tras F1 PASS y el RELEASE de la ventana de host de I-52.
