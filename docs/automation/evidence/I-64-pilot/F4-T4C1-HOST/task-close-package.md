# I-64 — F4-T4C1-HOST: paquete de cierre

**Resultado: CLOSED / PASS WITH LIMITATIONS**, en el intento 2 de 3.

**Autoridades del cierre:**
- la decisión D2 del Owner (`F4-DRAFTS/coordinator/owner-f4t4c-decisions-d1-d6.md`). La sesión cierra sin volver al Owner porque se cumplen sus condiciones: las dos revisiones con 0 bloqueantes y 0 importantes; la CI y la verificación en verde; y nc1 a nc3 por su oráculo ordinario, sin ningún sustituto;
- la decisión D3 del Owner, que autorizó la corrección del intento 2.

## 1. Estado resultante

- **El producto:** `b2631084`, el GREEN del intento 2. Integra en el host la escritura de T4B, sin una segunda política de mutación:
  - la operación del puerto del panel, `RequestUpdate` y `TakeUpdateProgress`;
  - el billete en vuelo con la cota «Completar o comando ajeno», sin contar las entradas en reposo;
  - el almacén del resultado por (sesión, rack);
  - el ejecutor sin estado, `_updateHost`;
  - el corredor puro `PushBackPilotHostRun`, con su cerrojo terminal;
  - el adaptador `RackPilotUpdateCommandContext`, la única llamada a `ExecuteInCommandContextAsync`, con `ExecutionResult` y una sola `OnCompleted`;
  - el bloqueo del documento exacto y el escritor de un solo uso (D-1);
  - el ámbito de mutación propia por la vía A con IC-09;
  - R-T4-07 en el host;
  - la frontera `PilotBoundaryKind.Update` en el núcleo;
  - los arrastres I-01, M-01, M-03 y M-06 de T4B;
  - las guardas G-1 a G-7, G-9 y G-10.
- **Sin UI:** `RequestUpdate` no tiene gesto de producto hasta F4-T4C2 (D-T4C-19).
- **`attempts`:** 2 de 3. El intento 3 no se gastó.
- **main** sigue en `812baf87`.

## 2. Corridas

**Intento 1** (custodia `cd902cd9`, F4-E66):
- la planificación `R20261115T143542Z-0100`, rechazada por fidelidad, y la replanificación reforzada `R20261115T144358Z-b5f7`, aceptada;
- nc4 `R20261115T144759Z-886c`;
- el Worker `R20261115T145157Z-efec`: RED `8e57061e`, GREEN `271a045c`;
- la verificación `R20261115T163412Z-eca1`: EXECUTION_VERIFIED 14/14;
- la revisión independiente: **REWORK, 1/1/10** (B-01 e I-01).

**Intento 2, la corrección** (contrato `gate-contract-attempt2.json`, BaseSha `12c248b0`):
- **la planificación** `R20261115T164833Z-353a`: fidelidad PASS y A1-A8 8/8, a la primera. La replanificación de D4 no se usó. Dos preparaciones se descartaron antes de invocar nada (`eb79` y `80c7`), por errores de los generadores de la sesión;
- **el Worker** `R20261115T165352Z-3e8e`: `claude-sonnet-5-5`/high, 284 turnos y 0 denegaciones. RED `1268bb9d`, que solo cambia `tests/`, y GREEN `b2631084`, que solo cambia `src/`, en cuatro archivos. 15 rutas, todas del alcance;
- **la CI exacta:**
  - GREEN `43741634851`, 4/4: Core Workspace 3421/3421, Core completo 19002/19002 y UI Workspace 601/601;
  - RED `43740130830`: Core en failure, con 60 fallos, todos dentro del filtro de Workspace; UI en success;
- **la verificación** `R20261115T175454Z-b1f8`: **EXECUTION_VERIFIED, 14/14**, `VerifiedSha` `b2631084`, con el RED acreditado.

## 3. Revisiones y controles

**Revisiones del intento 2:**
- **la directa de la sesión** (`direct-review.md`): **0/0/2**;
- **la re-revisión adversarial independiente** (`independent-rereview.md`): **ACEPTABLE CON MENORES, 0/0/5**. B-01 e I-01 están corregidos, y las dos pruebas de B-01 fallan contra `271a045c`.

**Controles** sobre las entradas de `b1f8`, conservando cada salida ordinaria:

| Control | Corrida | Oráculo | Resultado |
|---|---|---|---|
| nc1 | `R20261115T180033Z-7ec6` | Identity, STOP | **CONTROL PASS** (ordinario) |
| nc2 | `R20261115T180601Z-a952` | Scope, STOP | **CONTROL PASS** (ordinario): esta vez el Controller detectó la ruta fuera de la lista recortada, y PD-F4-07 no hizo falta |
| nc3 | `R20261115T181057Z-49eb` | FreeText, REWORK | **CONTROL PASS** (ordinario) |

La cadena «limit reached» que cuentan los registros de nc1 y nc3 es el texto de la parada S-11 («repeated rework limit reached») en un documento que leyó el Controller. No hubo ningún aviso de límite.

## 4. Lo que entrega F4-T4C1-HOST

- **Application:** `PushBackPilotHostRun.cs`, nuevo, más `WorkspaceHostCore.cs`, `WorkspaceEditIntent.cs`, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, `IWorkspaceBridgeLink.cs`, `EventBridgeCore.cs`, `WorkspaceSession.cs` y `PushBackPilotUpdateRun.cs`, este solo con I-01 y M-01.
- **Plugin:** `RackPilotUpdateCommandContext.cs`, nuevo, más el escritor (solo D-1), `WorkspaceController.cs` y `AutoCadEventBridge.cs`.
- **Pruebas:** cinco archivos nuevos de Core (HR, RQ, OM, IG y el doble del host), los modificados del alcance y el doble de UI, con las dos operaciones nuevas inertes.
- **Los seis textos nuevos** de `PushBackPilotHostTexts`, con la redacción que fijó la sesión y ratificó el Owner (D1).

## 5. Limitaciones y arrastres

**Los menores de la re-revisión (intento 2):**
- **N-01:** la guarda del vuelo de la fila 5 mira los cuerpos `Try*Pilot*` y ya no prohíbe el vuelo en los tres reenvíos del puerto. El producto es correcto.
- **N-02:** con un `Lock()` nulo, inalcanzable con el adaptador del producto, la causa visible es `LockFailedPrefix` más un mensaje de excepción en inglés. Coincide con D2-1 de la revisión directa.
- **N-03:** la rama de M-06 (b) con el billete empezado o terminado y todavía presente no tiene prueba propia.
- **N-04:** M-11 queda a medias. Sigue un aviso xUnit2012 en `TW/WorkspaceLifecycleGuardTests.cs:557`, introducido en el intento 1.
- **N-05:** cuatro filas nuevas de M-03 pasan en el RED sin estar declaradas.
- **D2-2, de la revisión directa:** con un ejecutor que no cumpla el contrato, `RequestUpdate` podría dar `Accepted` sin nada en el almacén.

**Tratamiento:** ninguno cambia el comportamiento del producto. N-01, N-03, N-04 y N-05 son de pruebas de esta tarea, cuyos archivos no están en el alcance de F4-T4C2: quedan como limitaciones declaradas, candidatas a una corrección de guardas. N-02 y D2-2 se declaran.

**KnownLimitations de la entrega:** el adaptador y el escritor solo los cubren las guardas de fuente y la compilación del Plugin. Ninguna prueba carga AutoCAD.

**Smoke-2** (§7 del paquete, después de F4-T4C2), con el producto, un dibujo real del Owner y el Owner como operador. Confirma:
- `ExecuteInCommandContextAsync`, el `DocumentLock`, el orden de los eventos y la cota del billete en AutoCAD real;
- Undo y Redo;
- CLOSE, CLOSEALL y QUIT;
- la mutación propia contra eventos reales;
- IC-04.

**Deudas que siguen:** H-1 (la suite de UI escribe en el perfil) y PD-F4-07.

## 6. Siguiente

Por la decisión D5 del Owner:
1. emitir **F4-T4C2-PANEL** (attempts 1 de 3), con el contrato preparado desde el paquete v5 y su comprobación independiente;
2. ejecutar su cadena;
3. después, **Smoke-2**.

T4C2 vuelve al Owner si su revisión descubre una decisión material nueva.
