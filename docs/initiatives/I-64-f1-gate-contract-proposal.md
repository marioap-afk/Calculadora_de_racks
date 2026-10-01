# I-64 — Propuesta de contratos de gate para F1 (Workspace Foundation)

```text
Estado: PROPUESTA — NO EMITIDA
Emisor previsto: Coordinator (AUTOMATION_PLAN 16.5; agent-execution/README §1: el Coordinator redacta gate-contract.json,
                 acepta o rechaza el paquete A1-A8 y declara GATE PASS)
Preparada por: sesión principal responsable de I-64 (Claude), como preparación de F1
Freeze: docs/initiatives/I-64-proposal-v5.md (Frozen: YES; Consensus I64-CONSENSUS-V5-01; evidencia §18)
AuthorityRevision propuesta: el commit que publica este documento (se toma con `git log -1 -- docs/initiatives/I-64-f1-gate-contract-proposal.md`);
                 su diff desde el Freeze solo toca documentos de la unidad y `docs/automation/` (AUTOMATION_PLAN 16.3)
MainSha esperado: 819955d61a6da4c811a11fbd11b5dca13f634b7c (se revalida en A6 al emitir)
IMPLEMENTATION: solo tras la emisión del contrato por el Coordinator, la planificación del Controller y la aceptación A1-A8
```

Este documento **no** es un `gate-contract.json`, no emite delegaciones y no autoriza trabajo. Recoge, para que el Coordinator lo emita o lo
corrija, el contenido mínimo que exige AUTOMATION_PLAN 16.5 por cada tarea de F1, derivado del Freeze sin añadir alcance. El contrato emitido
se valida contra `docs/automation/agent-execution/schemas/gate-contract.schema.json` (`rackcad-gate-contract/v1`, uno por `TaskId`).

## 1. Resultado del gate (Freeze §11, F1)

`RACKPANEL`; contexto del documento activo; selección fina (Ninguno, Uno, Selección (N) con recuento, Sin identidad, Diagnóstico) sin
enumeración global; puente único con encolado y drenaje; identidad de sesión; política de foco; contención; instrumentación; ADR
`propuesto`; procedimiento de diff de perfil. **Cero escrituras authored.**

Cierre (Freeze §11): INV-01, 02, 03 (lado de eventos), 07, 13, 14, 16, 17, 21, 22, 23, 25 (panel) y 33; CI exacta; revisión del
Coordinator. Smoke-1 ocurre **tras F1 PASS** y tras el RELEASE explícito de la ventana de host de I-52 (Freeze §12.1); no forma parte de
ninguna tarea delegada.

## 2. Tareas propuestas

| TaskId | Objetivo | Clase / effort semántico (`routing.md`) | Invariantes del Freeze | Pruebas (filtro propuesto; `ExpectRed`) |
|---|---|---|---|---|
| `F1-T0-ADR` | Crear el ADR propio en estado `propuesto` desde el anexo A del Freeze, con número asignado contra `origin/main` y ramas activas | Documentación / Routine | — | ninguna (documental) |
| `F1-T1-MODEL` | Modelo puro en Application: sesiones de documento con identidad de instancia, contexto de selección, pistas, cola y drenaje, políticas | Implementación de pruebas / Balanced | INV-01, INV-07, INV-16, INV-17, INV-22, INV-23, INV-33 | `FullyQualifiedName~RackCad.Tests.Workspace`; `ExpectRed: true` |
| `F1-T2-BRIDGE` | Puente central en el Plugin con fuente de eventos simulable: una suscripción por fuente, ciclo de vida, encolado, `BeginDocumentClose` de vida de sesión; guardas de fuente | Implementación transversal a capas (corta) / Deep | INV-02, INV-03 (eventos), INV-14, INV-21 | `FullyQualifiedName~RackCad.Tests.Workspace` y guardas de fuente; `ExpectRed: true` |
| `F1-T3-HOST` | Host `PaletteSet`, raíz WPF, comando `RACKPANEL`, selección fina hacia el contexto, política de foco, contención de excepciones e instrumentación | Implementación transversal a capas / Deep (puede resultar Long-horizon) | INV-13, INV-25; INV-24 solo host (Smoke-1) | `FullyQualifiedName~RackCad.UI.Tests.Workspace` y guardas; `ExpectRed: true` |

El orden propuesto es T0 → T1 → T2 → T3; T1 y T2 no comparten archivos de producción.

## 3. Alcance de escritura propuesto

Rutas nuevas; ninguna existente de producto, salvo las de registro estrictamente necesarias que el Coordinator enumere al emitir.

| TaskId | `AllowedWriteScope` |
|---|---|
| `F1-T0-ADR` | `docs/adr/NNNN-workspace-persistente-modeless.md` (archivo exacto, número fijado al emitir) |
| `F1-T1-MODEL` | `src/RackCad.Application/Workspace/`, `tests/RackCad.Tests/Workspace/` |
| `F1-T2-BRIDGE` | `src/RackCad.Plugin/Workspace/`, `tests/RackCad.Tests/Workspace/` |
| `F1-T3-HOST` | `src/RackCad.Plugin/Workspace/`, `src/RackCad.UI/Workspace/`, `tests/RackCad.UI.Tests/Workspace/` |

`ForbiddenWriteScope` común: `src/RackCad.UI/Systems/`, `src/RackCad.UI/RackFrames/`, los `src/RackCad.Plugin/*Commands*.cs` existentes,
`src/RackCad.Plugin/Systems/`, `src/RackCad.Plugin/Views/`, `src/RackCad.Plugin/Drawing/`, `assets/`, `eng/`, `deploy/`, `.github/`,
`docs/` (salvo el ADR de T0), `global.json`, `Directory.Build.props`, `Directory.Build.targets`, los archivos de solución y
`docs/automation/` (lo escribe la sesión). Ninguna escritura en el DWG, el registro de Windows ni el perfil de AutoCAD.

## 4. Autoridades propuestas (16.3)

| Clase | Rutas | Se leen en |
|---|---|---|
| `UNIT_DOC` | `docs/initiatives/I-64-proposal-v5.md` (Freeze), `docs/initiatives/I-64-workspace-persistente-rackcad.md`, `docs/initiatives/I-64-discovery.md`, este documento, `docs/automation/evidence/I-64-evidence.md` | `AuthorityRevision` |
| `EXTERNAL` | `AGENTS.md`, `docs/WORKFLOW.md`, `docs/INITIATIVE_LIFECYCLE.md`, `docs/AUTOMATION_PLAN.md` §16, `docs/automation/agent-execution/`, `docs/adr/0006-*.md`, `docs/adr/0019-*.md`, `docs/adr/0029-*.md`, `docs/adr/0044-*.md`, `docs/initiatives/I-55-proposal-v5.md` §4.4-§4.10 | `MainSha` |

No hay secciones `UNIT_CHANGE` (I-64 no cambia normas).

## 5. Celdas, enrutamiento y presupuesto propuestos

| Rol | Celda propuesta | Estado en el catálogo | Uso |
|---|---|---|---|
| Controller (planificación y verificación) | Codex CLI × `gpt-6-luna` × effort `high` | medida (PR-1 y G3 de I-61) | todas las tareas |
| Worker Balanced / Deep | subagente × `claude-sonnet-5-5` × effort `medium` / `high`, con `write-commit-push` | medida (U-04 y G3 de I-61) | T1 (`medium`), T2 y T3 (`high`) |
| Worker Routine | ninguna celda Eficiente con escritura elegible (`claude-haiku-4-5` en `STALE` desde 2026-10-01) | — | T0: o escalado de nivel con `ModelEscalationReason` (sin celda Eficiente) a `claude-sonnet-5-5`/`medium`, o trabajo directo de la sesión si el Coordinator lo autoriza |
| Worker Long-horizon | ninguna celda Frontera con `write-commit-push` medida | — | si T3 se clasifica Long-horizon: `routing.md` §4 «sin celda elegible» (decisión del Coordinator) |

- `RoutingEnforcement` propuesto: `required`.
- `CorrectionsAuthorized` propuesto: `true`, dentro del mismo alcance y con los topes de 16.8 (`max_attempts` = 3 del contrato de la iniciativa;
  `MaxReworkLoops` = 3).
- **Tope de invocaciones (16.8, P-07):** el Freeze de I-64 **no** lo fija. Propuesta para que el Coordinator lo fije al emitir: por `TaskId`,
  como máximo 1 planificación, 4 trabajos (1 + 3 correcciones), 4 verificaciones y los controles negativos que exija el README §10. Sin tope
  fijado, P-07 no es evaluable y la delegación no debe emitirse.

## 6. Condiciones STOP propuestas (además de S-01..S-14 y P-01..P-08)

- Cualquier necesidad de AutoCAD, de cambiar perfil, `TRUSTEDPATHS`, `SECURELOAD` o ACL, o de cargar el DLL: frontera (S-10) y vuelta a la
  sesión; Smoke-1 no es una tarea delegada.
- Cualquier cambio observable en comandos clásicos o en editores existentes (Freeze D-16, M-03): STOP.
- Cualquier escritura authored, de NOD o de XData desde el panel (Freeze D-22, INV-21): STOP.
- Cualquier uso de `Autodesk.AutoCAD.Internal` fuera del adaptador de host (Freeze D-01, O-10): STOP.
- Avance de `origin/main` respecto de `819955d6` antes de escribir: S-13 y 16.7.

## 7. Evidencia esperada por tarea

Handoff `rackcad-worker-handoff/v1`; CI `push` del `CurrentSha` exacto con los cuatro jobs requeridos en `success`; RED acreditado en la
primera delegación de cada cadena (16.8: `ChainRedSha` es `null`); TRX con selección ≥ el mínimo que fije el contrato; verificación
`rackcad-controller-verification/v1` del Controller; custodia en `docs/automation/evidence/I-64-pilot/<task>/<RunId>/` (16.12).

## 8. Precondiciones de la primera delegación (Freeze §13)

- Freeze AGREED: **cumplida** (evidencia §18).
- `docs/automation/state/I-64.yml`: **creado** en el mismo commit que este documento.
- Autoridades leídas en `MainSha`: en la planificación.
- Línea base de `config.toml` (SHA-256 y nombres de secciones y claves, sin valores): se toma en la salida del primer relevo (16.4 paso 1).
- Contrato de gate emitido por el Coordinator: **pendiente**.
