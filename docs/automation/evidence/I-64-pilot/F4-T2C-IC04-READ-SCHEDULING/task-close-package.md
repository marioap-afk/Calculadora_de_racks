# I-64 — F4-T2C-IC04-READ-SCHEDULING: paquete de cierre

Lo prepara la sesión responsable el 2026-11-12. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E36 y F4-E37. El cierre lo disponen las órdenes del Owner `F4-DRAFTS/coordinator/owner-f4t2c-close-decision.md` y `owner-f4t2c-nc1-decision.md`.

Este paquete cierra una tarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `33e43f9f4246d63c0b1fdcdddb58f62b5c963c53`, CI `43319785427` push 4/4 |
| Verificación | `R20261112T231128Z-7f67`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha = BaseSha `d5d94a76`; RED `703bda9d`, GREEN `33e43f9f` |
| Contrato | `gate-contract.json` (AuthorityRevision `05c7b501`, paquete v2) |
| Intentos | 1 de 3; los intentos 2 y 3 no se gastan (F4-E37) |
| Config | Snapshot G (`7C1516A9…`) en todas las invocaciones |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E37; limitaciones en el §5) |
| F4-T2 | Completa en sus subtareas: T2A, T2B1, T2B2A, T2B2B y T2C |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `027e` | aceptada (fidelidad PASS a la primera, A1-A8 8/8, Horizonte Medium) | primera delegación de la cadena; nc4 `1123` con su oráculo; sin replanificación |
| `2c89` | entrega del intento 1 | RED `703bda9d` (83 fallos por aserción), GREEN `33e43f9f` |
| `7f67` | `EXECUTION_VERIFIED`, 14/14 | revisión directa: producto conforme; independiente: ACEPTABLE CON MENORES (0/0/4); el Owner ordenó cerrar con los menores en F4-T3 |

## 3. Controles negativos (sobre las entradas de `7f67`)

| Control | Corrida ordinaria | Resultado ordinario | Sustituto | Registro |
|---|---|---|---|---|
| nc1 (CurrentSha falso) | `R20261112T231801Z-f749` | ORÁCULO NO CUMPLIDO / SEÑAL DEMOSTRADA: Handoff primera en fail (el mismo modo que en F4-T2B2B, PD-F4-04), Identity en fail con la evidencia correcta, `EXECUTION_BLOCKED` / STOP (S-04) | F4T2C-NC1-IDENTITY-SIGNAL-01 (`R20261112T234036Z-5559`): PASS | SATISFIED_UNDER_F4T2C_NC1_IDENTITY_SIGNAL_SUBSTITUTE |
| nc2 (alcance estrechado, sin `EventBridgeCore.cs`) | `R20261112T232117Z-1dbe` | CONTROL PASS: Scope, nombrando la ruta quitada; F4T2C-C01 | — | CONTROL PASS |
| nc3 (término de gate en `WorkCompleted`) | `R20261112T232753Z-93d5` | CONTROL PASS: FreeText, `EXECUTION_REWORK_REQUIRED` | — | CONTROL PASS |

## 4. Producto entregado

**IC-04: todo fuera del Idle**, por decisión del Owner. Sin escritura en el DWG y sin cambios en el controlador, el host de navegación, los lectores, `WorkspaceHostCore`, la UI ni la paleta.
- **`WorkspaceIdleScheduler`** (Application, puro):
  - `OnIdleTick` programa a lo sumo una continuación, y solo si no hay otra programada, si hay post y si la elegibilidad en memoria lo permite;
  - `Run` desmarca, vuelve a comprobar y solo entonces ejecuta;
  - todo contenido y sin reprogramarse;
  - contadores en memoria.
- **`EventBridgeCore.CanDrainNow()`:**
  - comparte el predicado de la pasada (`TryGetDrainable`), que ahora también usa `OnIdle`;
  - es falso con cualquier cierre permitido pendiente (`_lastAllowedClose.HasValue`), para no leer dentro de la pregunta de un cierre ni romper la secuencia de F4-T2B2B;
  - es una consulta pura.
- **`AutoCadEventBridge`:**
  - el manejador de Idle solo llama a `OnIdleTick`;
  - la pasada de drenaje (`OnIdle`, sin cambios) corre únicamente en la continuación, posteada con `Dispatcher.BeginInvoke(DispatcherPriority.Background)` al dispatcher del hilo principal;
  - con ella corren la lectura de selección y, después, la del resumen;
  - comprobación `[Conditional("DEBUG")]` de `TopTransaction` tras la pasada, que solo registra.
- **Guardas:** el cambio declarado (1) añade `CanDrainNow` a la lista de miembros públicos de `EventBridgeCore`. El (2) añade una excepción por archivo exacto para el `Dispatcher` en el puente.

## 5. Limitaciones y abierto

**Menores → F4-T3** (orden del Owner, cambios declarados):
1. G-1 y G-2 no cubren el método intermedio `RunIdleContinuation`;
2. G-3 y G-10 no fijan la forma exacta del post; además hay que prohibir `.Wait(`, `ExecuteInApplicationContext` y `Dispatcher.Run`;
3. tres pruebas del cierre pendiente no tienen aserción de profundidad;
4. la documentación y los contextos de registro del puente.

Hasta entonces, el riesgo es de regresión futura del cableado; el producto entregado es correcto.

**Residuales para el Owner:**
- **R-T2C-01:** una superficie Ready no se refresca tras un evento que invalida su resumen. Es un hueco preexistente de F3 y un cambio de UI.
- **R-T2C-02:** latencia tras un cierre cancelado sin comando.

**Arrastre a F4-T3:** los subdiálogos del panel activan `SetModalRackCadActive(true)` antes de `ShowModalWindow`.

**Controles:** nc1 se satisface con un sustituto determinista. **PD-F4-04** se repite por segunda vez.

**Condición previa conocida:** la suite completa de UI en Release da 2 fallos de `PendingTextFieldTests` por `#if DEBUG`. En Debug, que es lo que corre la CI, son 0.

**Smoke-2:**
- la temporización de pila en el host real (D-T2C-05);
- el bombeo del dispatcher en bucles anidados y modales;
- el cierre (CLOSE, pestaña, API, QUIT/CLOSEALL) y R-T2C-02;
- la ventana del cambio de documento;
- la latencia de Background;
- la continuación perdida;
- las aserciones Debug existentes;
- la reentrada de continuaciones.

**Smoke dirigido opcional de T2C:** no se hizo. El contrato lo deja fuera del Worker, y su observación funcional queda disponible para el Owner (§6 del paquete) o para Smoke-2.

**Sin AutoCAD:** la lógica y la temporización de pila se prueban en Core con dobles; el Plugin, con guardas de texto y el build Release de la CI.
