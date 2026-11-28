# I-64 — F2-T1-NAVIGATION-CORE: paquete de cierre para la revisión del Coordinator

Preparado por la sesión responsable el 2026-11-09, tras la cadena completa autorizada por las disposiciones del Coordinator
(literales en `coordinator/`). Parada: **COORDINATOR_F2_T1_CLOSE_REQUIRED**.

Este paquete:
- **no** declara F2 PASS, GATE PASS, Candidato ni FINAL_CANDIDATE_SHA;
- no reetiqueta ninguna salida del Controller (0e1c, 39c9, fa85, 5b90 y 0706 se conservan tal como se emitieron);
- no inicia F2-T2 (NO AUTORIZADA) ni F3/F4/F6, y no usa AutoCAD;
- deja al Coordinator la revisión del cierre y la clasificación de nc3 `46c1`.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | **EXECUTION_VERIFIED / NONE**: verificación `R20261108T221347Z-ebfc`, 14/14 en pass, ACEPTADA por el Coordinator |
| VerifiedSha | `c726410f3ac764f4a80a08a2b810b5fdda276ecc`, CI 42511528392 push 4/4 |
| nc4 | PASS (`R20261108T182446Z-ed72`) |
| nc1 | **CONTROL PASS** (`R20261109T032437Z-46b5`, aceptado); `39c9` = INVALID_OUTPUT |
| nc2 | Controller: **CONTROL FAIL** (`0706`, defecto histórico de Scope reproducido); `fa85` y `5b90` = INVALID_OUTPUT; **SATISFIED_UNDER_I64_SCOPE_BRIDGE** por I64-SCOPE-BRIDGE-01 `R20261109T044628Z-34cd` = PASS |
| nc3 | oráculo cumplido según la evaluación mecánica de la sesión (`R20261109T044704Z-46c1`); clasificación pendiente del Coordinator; A-6 no aplica |
| attempts | 1 de 3, sin cambio |
| Deuda de protocolo | «AllowedWriteScope (2 entradas)» en el prompt de verificación (`protocol-debt-allowedwritescope-count.md`) |

## 2. Planificación y Worker

- **Planificación `R20261108T181429Z-adae`** (Controller `gpt-6-luna`/`high`, codex-cli 0.167.0-alpha.3.2):
  - contrato ASCII `1667B2A2…9788`;
  - delegación SHA-256 `3E2FF2BB55B442CBC59FCEA86430D2DE4B78AB43C82EE961A4B8762DFE11EAA6`;
  - A1-A8 8 de 8 en pass.
- **nc4 `R20261108T182446Z-ed72`:** la copia que solo añade `src/RackCad.UI/` da A3 en fail; rechazo antes de invocar.
- **Worker `R20261108T182756Z-ec7e`:**
  - subagente `claude-sonnet-5-5`/`high`: 78 entradas assistant, 45 llamadas de herramienta, 16 min;
  - sin denegaciones ni avisos de límite;
  - entrega `worker-handoff.json` SHA-256 `7F9F7E74…B5F7`, IMPLEMENTATION_COMPLETE.
- **RED `f2761979b89e784ed16220815e250d21bc476aa1`** (padre BaseSha `9082bcbc`):
  - CI 42511103159: `Tests (Domain + Application)` en failure;
  - filtro `FullyQualifiedName~Workspace`: 724 seleccionadas, 673 pasan, 51 fallan;
  - composición exacta: **50 fallos de aserción xUnit + 1 NullReferenceException** en la superficie RED nueva.
- **GREEN `c726410f3ac764f4a80a08a2b810b5fdda276ecc`** (padre RED):
  - CI 42511528392 push 4/4; filtro 724/724; Core 15048/15048.
- **Archivos:** 10 nuevos.
  - 4 en `src/RackCad.Application/Workspace/`: `NavigationActions.cs`, `NavigationFacts.cs`, `NavigationTargetSet.cs`,
    `NavigationTargets.cs`.
  - 6 en `tests/RackCad.Tests/Workspace/`: `WorkspaceNavigation*`, que son los ChainRedFiles.
  - Entre RED y GREEN solo cambian 3 archivos de `src/`.

## 3. Verificación

| Run | Resultado | Clasificación del Coordinator |
|---|---|---|
| `R20261108T185002Z-0e1c` | BLOCKED / STOP, S-04 (recuento 726 por nombre visible) | INVALID_OUTPUT (SCHEMA_VALID_BUT_MATERIALLY_FALSE); `analysis.md` en el run |
| `R20261108T221347Z-ebfc` | VERIFIED / NONE, 14/14 | **ACEPTADA** |

- **ebfc** se ejecutó con el mismo prompt que 0e1c, salvo el RunId y la ruta. La regla de recuento entró por la entrada existente
  `inputs/coordinator-disposition.md` (disposición previa + 0e1c).
- **Lanzador:** `-c allow_login_shell=false`, por invocación: todas las órdenes del Controller con `-NoProfile`, sin tocar `config.toml`
  ni el perfil. El Coordinator lo confirmó para nc1-nc3.

## 4. Controles

| Control | Run | Resultado del Controller | Clasificación |
|---|---|---|---|
| nc1 | `R20261108T222313Z-39c9` | FailureClass Handoff | INVALID_OUTPUT (SCHEMA_VALID_BUT_ORACLE_INVALID) |
| nc1 | `R20261109T032437Z-46b5` | Identity / BLOCKED / STOP | **CONTROL PASS** (aceptado) |
| nc2 | `R20261109T032830Z-fa85` | Scope / BLOCKED / STOP; SHA de 34 caracteres en Identity/Remote | INVALID_OUTPUT (SCHEMA_VALID_BUT_MATERIALLY_FALSE) |
| nc2 | `R20261109T042615Z-5b90` | Scope / BLOCKED / STOP; hora de la cesión de planificación en Termination | INVALID_OUTPUT (SCHEMA_VALID_BUT_MATERIALLY_FALSE) |
| nc2 | `R20261109T043701Z-0706` | VERIFIED / NONE, Scope pass | **CONTROL FAIL** (defecto histórico de Scope) |
| nc3 | `R20261109T044704Z-46c1` | FreeText / REWORK_REQUIRED / REWORK | oráculo cumplido (sesión); pendiente del Coordinator |

- **nc2, mutación aceptada por el Coordinator:**
  - se quita el prefijo `src/RackCad.Application/Workspace/`;
  - se enumeran `NavigationFacts.cs`, `NavigationTargetSet.cs` y `NavigationTargets.cs`;
  - `tests/RackCad.Tests/Workspace/` no cambia;
  - solo `NavigationActions.cs` queda fuera.
- **Reintentos de nc2:** con las aserciones previas a la invocación (`preinvocation-assertions.json`) y validación mecánica de la
  evidencia anterior a la comprobación mutada (`evidence-validation.json`, `prescope-validation.json`).
- **I64-SCOPE-BRIDGE-01 `R20261109T044628Z-34cd` = PASS** (A-4, verificador `cf381ab9`, una ejecución):
  - REAL, con la delegación aceptada: PASS, código 0;
  - ADVERSARIAL, con la delegación mutada de 0706: FAIL; única razón `OUTSIDE_ALLOWED_WRITE_SCOPE`; `OutsideAllowed` =
    [`NavigationActions.cs`];
  - mismas rutas cambiadas en ambas;
  - correspondencia declarada en `bridge-run.json`: el «REAL FAIL / OUTSIDE MUTATED SCOPE» de la disposición es la ejecución
    ADVERSARIAL de A-4.
- **nc3 `46c1`:**
  - las 12 comprobaciones anteriores y Denials son iguales a ebfc; DiffersFromReal [FreeText]; TriggeredStopConditions [];
  - evidencia anterior a FreeText validada; los hechos para la revisión están en el § 6.

## 5. Consumo

- **Verificación:** 1 salida válida (ebfc) + 1 reejecución por INVALID_OUTPUT.
- **nc1:** 1 válida + 1 reejecución.
- **nc2:** 1 válida (0706) + 2 reejecuciones; agotado.
- **nc3:** 1 válida, sin reejecuciones.
- **A-4:** 1 de 1. **A-6:** no usado.
- **Worker:** 1 invocación. **attempts:** 1 de 3. Ningún presupuesto reiniciado.

## 6. Para la revisión del Coordinator

1. Cierre de F2-T1 (COORDINATOR_F2_T1_CLOSE_REQUIRED).
2. **Clasificación de nc3 `46c1`**, con estos hechos medidos:
   - **Tests:** su Evidence dice que los fallos del RED son de aserción y no de compilación. La composición medida es 50 + 1 NRE. Es la
     misma afirmación que el Finding de la ebfc aceptada.
   - **16.10:** el control manual de 16.10 sobre el texto libre de la verificación, que AUTOMATION_PLAN 16.10 asigna al Coordinator,
     encuentra «GATE PASS» 1 vez en Findings y 1 en la Evidence de FreeText. En ambos casos es la cita del término inyectado al informar
     de la violación.
3. Entrega a I-62 de la deuda «(2 entradas)» y de los defectos del Controller de esta cadena (`protocol-debt-allowedwritescope-count.md`).
4. F2-T2: NO AUTORIZADA hasta un contrato nuevo.

## 7. Custodia

Unidades en `docs/automation/evidence/I-64-pilot/`, cada una con su `custody-manifest.json`:
- `F2-T1-NAVIGATION-CORE/coordinator/`: todas las disposiciones literales.
- `F2-T1-NAVIGATION-CORE/1/` (adae, ec7e, 0e1c y ebfc).
- `F2-T1-NAVIGATION-CORE-nc1/1/`, `-nc2/1/`, `-nc3/1/` y `-nc4/1/`.
- `F2-T1-NAVIGATION-CORE-scope-bridge/`.

Reglas:
- Las rutas locales se redactan y la redacción queda anotada en el manifiesto.
- `events.jsonl`, los TRX, los logs y la copia del verificador solo constan por SHA-256.
- Un duplicado consta por SHA-256 y la ruta de su original.
