# I-64 — F4-T2B2B-LIFECYCLE: paquete de cierre

Lo prepara la sesión responsable el 2026-11-12. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, de F4-E29 a F4-E35. El cierre lo disponen las órdenes del Owner `F4-DRAFTS/coordinator/owner-f4t2b2b-close-decision.md` y `owner-f4t2b2b-nc1-decision.md` (F4-E35).

Este paquete cierra una tarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `8f249e3a9d886348a18ca8eef35f7ab014a8b167`, CI `43294223849` push 4/4 |
| Verificación | `R20261112T201718Z-060d`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha = BaseSha `32369c7b`; RED `a2e0ec13`, GREEN `8f249e3a` |
| Contrato | `gate-contract.json` (AuthorityRevision `06ef1915`, paquete v8 e IC-08) |
| Intentos | 1 de 3; los intentos 2 y 3 no se gastan (F4-E35) |
| Config | Snapshot G (`7C1516A9…`) en todas las invocaciones |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E35; limitaciones en el §5) |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `7813` | aceptada (fidelidad PASS a la primera, A1-A8 8/8, Horizonte Medium) | primera delegación de la cadena; nc4 `fc9a` con su oráculo |
| `36c7` | entrega del intento 1 | RED `a2e0ec13` (128 fallos por aserción), GREEN `8f249e3a`; relevo de salida medido tres veces (falso P-02 de la propia sesión) |
| `060d` | `EXECUTION_VERIFIED`, 14/14 | revisión directa: producto conforme; independiente: ACEPTABLE CON MENORES (0/0/5); el Owner ordenó cerrar con los menores en F4-T3 |

## 3. Controles negativos (sobre las entradas de `060d`)

| Control | Corrida ordinaria | Resultado ordinario | Sustituto | Registro |
|---|---|---|---|---|
| nc1 (CurrentSha falso) | `R20261112T202615Z-e92e` | ORÁCULO NO CUMPLIDO / SEÑAL DEMOSTRADA: Handoff primera en fail (16.9 «salida falsa»), Identity en fail con la evidencia correcta, `EXECUTION_BLOCKED` / STOP (S-04) | F4T2B2B-NC1-IDENTITY-SIGNAL-01 (`R20261112T204312Z-f577`): PASS | SATISFIED_UNDER_F4T2B2B_NC1_IDENTITY_SIGNAL_SUBSTITUTE |
| nc2 (alcance estrechado, sin `DraftRegistry.cs`) | `R20261112T203204Z-c514` | CONTROL PASS: Scope, nombrando la ruta quitada; `EXECUTION_BLOCKED` / STOP | — | CONTROL PASS |
| nc3 (término de gate en `WorkCompleted`) | `R20261112T203605Z-97e6` | CONTROL PASS: FreeText, `EXECUTION_REWORK_REQUIRED` | — | CONTROL PASS |

## 4. Producto entregado

El ciclo de vida de los borradores del piloto acotado de Push Back en el host, en memoria y sin escritura en el DWG:
- **Asesor** `IWorkspaceLifecycleAdvisor` (`AdviseClose`, `SessionDestroyed(session, closeSequenceIntact)`).
  - `EventBridgeCore.SetLifecycleAdvisor` es su único miembro público nuevo.
  - `OnBeginDocumentClose` adopta la decisión del asesor. Su excepción da Veto, sin degradar.
  - El puente recuerda solo el último cierre permitido (un `SessionId?`) y lo olvida en las 13 entradas que mutan estado. Calcula la secuencia intacta antes de quitar el estado de la sesión y llama al asesor en cada destrucción.
  - El Plugin llama a `e.Veto()` solo con Veto. El controlador asigna el host como asesor.
- **`DraftRegistry`**, ampliado de forma aditiva; las operaciones ordinarias de F4-T1 no cambian.
  - `DiscardByUser`, `DiscardAllByUser` y `RetireKnownSafe` no llaman al comparador.
  - `RetireConservative` mira primero el texto pendiente y luego el comparador, contenido borrador a borrador. Produce `UncertainRackIds` y `UncertainReasons`.
- **Tabla de veto del Owner (F4-E29):**

  | Caso | Resultado |
  |---|---|
  | Sucio conocido | VETO |
  | Texto pendiente | VETO |
  | Sucio desconocido (excepción del comparador) | VETO |
  | Limpio conocido | Cierre normal |
  | Descartado explícitamente | Retirada y cierre, sin comparador |

  «Limpio conocido» exige la marca del host y la secuencia de cierre intacta vista por el puente.
- **`WorkspaceHostCore`:**
  - marcado de obsolescencia del drenaje (IC-08);
  - `TryReloadPilotEdit`, con `PassGate`, los pasos 1 a 8 y los estados NotFound, NotNeeded y Reloaded al final del enum;
  - `DiscardPilotEdits`, solo con `Authorize`;
  - `NotePilotEdit`;
  - la cola y el registro de `DraftLossNotice`, solo con pérdida.

  Las operaciones nuevas son código oscuro hasta F4-T3.
- **Arrastres de F4-T2B2A:** N-1 (prefijo ordenado de `EditIntentStatus`) y N-4 (las tres vías de `Rejected`).

## 5. Limitaciones y abierto

- **Desviación registrada:** M-1 contradice la letra de INV-F4T2B2B-11. La guarda del controlador exige `_bridge.SetLifecycleAdvisor(_host);` y la del puente el texto exacto del reenvío. Pasa a **F4-T3** como cambio declarado (orden del Owner, F4-E35).
- **Menores → F4-T3**, como cambios declarados:
  - M-2 (conteos y conjuntos exactos de archivo entero: `_knownClean.Remove` = 9, `.DiscardAndReload(` = 1, dos métodos en la interfaz, la cola `internal`);
  - D-1 (cinco archivos en `Drafts/`);
  - M-3 (la prueba «silent» del residual);
  - M-4 (documentación de `Waiting` y `NotAdmitted` para la recarga);
  - M-5 (`Detail` y `Witness` del NotAdmitted de la recarga; la no degradación del descarte).
- **Residuales declarados del diseño:**
  - un cierre cancelado sin entradas posteriores, seguido de una edición sin `NotePilotEdit` y de una destrucción sin su cierre, se retira como limpio conocido. Lo cubre la obligación de `NotePilotEdit` en F4-T3 y F4-T4;
  - `Stop()` destruye sesiones sin el asesor;
  - el fallo al enganchar `BeginDocumentClose` deja la sesión sin veto.
- **Controles:** nc1 se satisface con un sustituto determinista. Deuda de protocolo **PD-F4-04**: ambigüedad Handoff/Identity entre README §10 y AUTOMATION_PLAN 16.9.
- **Condición previa conocida:** la suite completa de UI en Release da 2 fallos de `PendingTextFieldTests` por `#if DEBUG`. En Debug, que es lo que corre la CI, son 0.
- **Smoke-2:** hay que medir con AutoCAD:
  - el orden real entre cierre y destrucción en CLOSE, CLOSEALL, QUIT, la pestaña y la API. Si la secuencia nunca queda intacta o hay falsos inciertos, se devuelve al Owner (D-T2B2-10);
  - el cierre cancelado;
  - un `e.Veto()` que lanza o se ignora;
  - el documento vetado no activo;
  - el coste de `AdviseClose` y `RetireConservative`;
  - la vía de retirada sin rastro.
- **Sin AutoCAD:** la lógica se prueba en Core con dobles; el Plugin, con guardas de texto y el build Release de la CI.
