# I-64 — F4-T2B2A-INTENT-WIRING: paquete de cierre

Lo prepara la sesión responsable el 2026-11-12. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, de F4-E29 a F4-E33. El cierre lo disponen las órdenes del Owner `F4-DRAFTS/coordinator/owner-f4t2b2a-close-decision.md` y `owner-f4t2b2a-nc2-decision.md` (F4-E33).

Este paquete cierra una tarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `56b0d4977c6c9dae1eef78d1bcff5842ca45dcc5`, CI `43259514731` push 4/4 |
| Verificación | `R20261112T170518Z-ed09`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `e277dd42`; intento 1: RED `db37e30d`, GREEN `2d743e46`; intento 2: BaseSha `36b497ad`, RED `568f845d`, GREEN `56b0d497` |
| Contratos | `gate-contract.json` (intento 1) y `gate-contract-attempt2.json` (corrección, F4-E32) |
| Intentos | 2 de 3; el intento 3 no se gasta (F4-E33) |
| Config | Snapshot G (`7C1516A9…`) en todas las invocaciones |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E33; limitaciones en el §5) |

## 2. Planificaciones y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `a409` | REJECTED_BEFORE_INVOCATION (A4) | `ForbiddenWriteScope[43]` sin «Source»; se devolvió al Owner (F4-E31) |
| `4456` | aceptada (fidelidad PASS, A1-A8 8/8) | replanificación única autorizada por el Owner; nc4 `7099` con su oráculo |
| `4331` | entrega del intento 1 | RED `db37e30d`, GREEN `2d743e46` |
| `46e1` | `EXECUTION_VERIFIED`, 14/14 | la revisión independiente dio NO ACEPTABLE (I-1) (F4-E32) |
| `519b` | aceptada (fidelidad PASS, A1-A8 8/8) | planificación de la corrección |
| `3d42` | entrega del intento 2 | RED `568f845d`, GREEN `56b0d497` |
| `ed09` | `EXECUTION_VERIFIED`, 14/14 | sobre `56b0d497`; la re-revisión dio NO ACEPTABLE (N-1), y el Owner ordenó cerrar con N-1 en T2B2B |

## 3. Controles negativos (sobre las entradas de `ed09`)

| Control | Corrida ordinaria | Resultado ordinario | Sustituto | Registro |
|---|---|---|---|---|
| nc1 (CurrentSha falso) | `R20261112T171355Z-447b` | CONTROL PASS: Identity, `EXECUTION_BLOCKED` / STOP | — | CONTROL PASS |
| nc2 (alcance estrechado, sin `WorkspaceEditIntent.cs`) | `R20261112T171913Z-7a93` | ORÁCULO MECÁNICO CUMPLIDO / SEÑAL NO DEMOSTRADA: Scope fail atribuido a C02; la ruta quitada no se detectó | F4T2B2A-NC2-SCOPE-SIGNAL-01 (`R20261112T173209Z-c91c`): PASS | SATISFIED_UNDER_F4T2B2A_SCOPE_SIGNAL_SUBSTITUTE |
| nc3 (término de gate en `WorkCompleted`) | `R20261112T172443Z-06a8` | CONTROL PASS: FreeText, `EXECUTION_REWORK_REQUIRED` | — | CONTROL PASS |

## 4. Producto entregado

El cableado de la intención de edición del piloto acotado de Push Back en el host, sin escritura en el DWG:
- **Proyección aditiva:** `RackSiblingDrawingScan.RawEnvelopes` (`init`, ordinal, de solo lectura), con el mismo `json` que lee `Traverse`. El constructor conserva su firma.
- **Adaptador** `Plugin/Views/RackPilotIntentAdapter.cs`:
  - `LoadCatalog`, fuera del bloqueo;
  - `Read`, con una sola `CaptureDrawing`, el helper único `ToHandle`, `TopTransaction` comprobada en todos los caminos y fallos tipados;
  - `ReadPreviewWitness`, para F4-T3.
- **Costura:** `IWorkspaceEditIntentReader`, implementada por una clase anidada de `WorkspaceNavigationHost`, en este orden: documento activo; reposo; catálogo; bloqueo; autorización (INV-22); `Revalidate` y elección del origen; adaptador. Se asigna con `SetEditIntentReader` desde el getter `Navigation`.
- **Elección del origen:** `WorkspaceEditIntent.ChooseOriginDefinition`, una función pura (D-11).
- **`WorkspaceHostCore`:**
  - un solo `DraftRegistry` con su `DraftClosePolicy`;
  - `TryBeginPilotEdit` y `TryCheckPilotEdit`, que siguen los pasos 1 a 8 de D-T2B2-06, con una sola lectura y contención como Failed sin degradar la sesión;
  - un testigo sin clave da Failed sin leer;
  - los resultados son valores (`EditIntentOutcome` y `EditCheckOutcome`, con `Differences` nunca nulo).
  Las dos operaciones son código oscuro hasta F4-T3.
- **Arrastres de F4-T2B1:** de T2B1/N-1 a T2B1/N-4; entre ellos, `Detail` nulo con un Kind en blanco y la prueba del par de reglas (9, 10).

## 5. Limitaciones y abierto

- **N-1** (re-revisión, IMPORTANTE, solo pruebas): `WorkspaceEditIntentHostTests.cs` fija el conjunto exacto de `EditIntentStatus`. Pasa a **F4-T2B2B** como cambio declarado, a un prefijo ordenado con `default` igual a `Unavailable`. Exige enmendar el §3 y el §6 del paquete (orden del Owner, F4-E33).
- **Menores:**
  - N-2 (`finally` en Debug con un documento nulo) → F4-T3;
  - N-3 (la guarda de la obligación 10 reconoce consumidores por nombre) → limitación aceptada;
  - N-4 (el comentario de `Rejected` omite la vía del lector) → F4-T2B2B;
  - N-5 (guardas que deciden de antemano la exposición de T3) → F4-T3, con sus propios cambios de guarda (H-03).
- **Residuos aceptados:**
  - la diferencia entre clave nula y en blanco en el adaptador, inalcanzable desde el core;
  - la documentación de `Failed`;
  - el nombre «…OfAnyFile»;
  - el cero de `EditIntentReadStatus`, que el host defiende.
- **Controles:** nc2 se satisface con un sustituto determinista. Deuda de protocolo **PD-F4-03**: el Controller puede fallar Scope por un motivo ajeno a la mutación.
- **Condición previa conocida:** la suite completa de UI en Release da 2 fallos de `PendingTextFieldTests` por `#if DEBUG`. En Debug, que es lo que corre la CI, son 0.
- **Smoke-2:** hay que medir con AutoCAD:
  - los bloqueos anidados;
  - DBMOD y UNDO tras el `Commit` de lectura del barrido;
  - el coste de dos recorridos de la BlockTable;
  - la dependencia del catálogo en «Comprobar ahora»;
  - los fallos del adaptador fuera de RackLog;
  - el origen limitado a referencias de Model Space.
- **Sin AutoCAD:** la lógica se prueba en Core con dobles; el Plugin, con guardas de texto y el build Release de la CI.
