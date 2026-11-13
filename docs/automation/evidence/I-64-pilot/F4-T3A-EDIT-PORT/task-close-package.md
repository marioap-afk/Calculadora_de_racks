# I-64 — F4-T3A-EDIT-PORT: paquete de cierre

Lo prepara la sesión responsable el 2026-11-13. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E38 y F4-E39. El cierre lo disponen las órdenes del Owner `F4-DRAFTS/coordinator/owner-f4t3a-close-decision.md` y `owner-f4t3a-nc2-decision.md`.

Este paquete cierra una subtarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `18471988fc59dbfdeee1ffc409127c4f7226004b`, CI `43355611879` push 4/4 |
| Verificación | `R20261113T032218Z-d960`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha = BaseSha `ea34bf7d`; RED `2522e894`, GREEN `18471988` |
| Contrato | `gate-contract.json` (AuthorityRevision `7aea8cec`, paquete v2 de F4-T3) |
| Intentos | 1 de 3; los intentos 2 y 3 no se gastan (F4-E39) |
| Config | Snapshot G (`7C1516A9…`) en todas las invocaciones |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E39; limitaciones en el §5) |
| F4-T3 | T3A cerrada; T3B-EDIT-UI y T3C-CLOSE-DISCARD pendientes, en ese orden |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `4a5e` | rechazada antes del Worker (P-03) | comparación byte a byte REJECTED en Invariants[6], de indicativo a subjuntivo; sin consumir intentos |
| `cad1` | aceptada (fidelidad PASS, A1-A8 8/8, Horizonte Low) | replanificación reforzada única (F4-E31, F4T3A-C08); nc4 `5eab` con su oráculo |
| `f0c9` | entrega del intento 1 | RED `2522e894` (110 fallos por aserción), GREEN `18471988` |
| `d960` | `EXECUTION_VERIFIED`, 14/14 | revisión directa: producto conforme; independiente: ACEPTABLE CON MENORES (0/0/7); el Owner ordenó cerrar con los menores en T3B y T3C |

## 3. Controles negativos (sobre las entradas de `d960`)

| Control | Corrida ordinaria | Resultado ordinario | Sustituto | Registro |
|---|---|---|---|---|
| nc1 (CurrentSha falso) | `R20261113T032839Z-c072` | CONTROL PASS: Identity, `EXECUTION_BLOCKED` / STOP | — | CONTROL PASS |
| nc2 (alcance estrechado, sin `IWorkspacePilotEditPort.cs`) | `R20261113T033215Z-65d0` | CONTROL NOT PASSED: FailureClass Contract (el mismo modo que en F4-T2B1, PD-F4-01), con Scope en fail sobre la ruta quitada y F4T3A-C01 | F4T3A-NC2-EXACT-WHITELIST-01 (`R20261113T034349Z-8f61`): PASS | SATISFIED_UNDER_F4T3A_EXACT_WHITELIST_SUBSTITUTE |
| nc3 (término de gate en `WorkCompleted`) | `R20261113T033645Z-2c64` | CONTROL PASS: FreeText, `EXECUTION_REWORK_REQUIRED` | — | CONTROL PASS |

## 4. Producto entregado

**El puerto de edición del piloto acotado de Push Back** (campo `AnnotationScale`), en Application. Sin UI, sin comando, sin escritura authored, sin tocar `Drafts/` y sin `+=` nuevos.
- **`IWorkspacePilotEditPort`** (archivo nuevo):
  - las 12 operaciones de D-T3-02;
  - valores inmutables con listas nunca nulas;
  - los enums `PilotFieldState`, `PilotFieldStatus`, `PilotBoundaryDecision` y `PilotBoundaryKind`, este último como prefijo ordenado al que T4 puede añadir `Update`;
  - ningún tipo de `Drafts` ni del piloto.
- **`WorkspaceHostCore`:** lo implementa explícitamente, sin crecer su superficie pública.
  - **Compuerta de reentrada:** una marca en `try/finally` alrededor de las tres lecturas. Mientras está puesta, las mutaciones dan `Busy` y las lecturas anidadas dan `Waiting`.
  - **Lecturas de memoria:** `GetField` y `GetRacksAtRisk`, sin autorización y sin lanzar.
  - **Edición y fronteras:** `SetPendingText` y `CommitField` sobre la mecánica de la frontera bloqueante. Las fronteras no bloqueantes conservan el pendiente tal cual.
  - **Descartes:** `DiscardRack`, sin comparador, y `DiscardDocument`.
  - **Colas:** de pérdidas y de avisos de veto (a lo sumo uno por sesión), encolados en `AdviseClose` y caducados al descartar, en el Allow y al retirar.
  - **Marca «limpio conocido»:** `NotePilotEdit` en todo camino que muta.
- **`WorkspacePilotSupport.cs`** (archivo nuevo):
  - el texto mostrado sin exponente, que reanaliza al mismo `double` en cualquier cultura;
  - el ámbito del modal del panel, anidado e idempotente;
  - la profundidad de avisos;
  - el decorador que da `Waiting` en las lecturas dentro de un aviso.
- **`WorkspaceController`:**
  - la profundidad de avisos en `NotifyStateChanged`;
  - `PilotEdit` como adaptador anidado sobre el decorador;
  - `EnterPanelModal()`.
- **`AutoCadEventBridge`:** la documentación de la clase y los dos contextos de registro (T2C 4).
- **Cambios declarados en pruebas existentes:**
  - M-2, D-1, M-3 y M-5 de F4-T2B2B;
  - el cuerpo de `AdviseClose`;
  - T2C 1-4.

## 5. Limitaciones y abierto

**Menores → F4-T3B** (orden del Owner, cambios declarados):
1. el escritor simulado del oráculo crítico es nominal: un espía real, o retirarlo y citar la guarda por cuerpo;
2. cotas en lugar de los conteos exactos duplicados de `ThisTaskAddsNoFile...` (contra D-1);
3. las pruebas que faltan:
   - la frontera bloqueante con un borrador no editable;
   - la marca conservada por las operaciones que no mutan;
   - los `catch` externos;
   - la aserción entre literales;
5. documentar o extender `Undecidable` sobre un estado explícito;
7. la documentación del puerto: `Kept` procede en una bloqueante, el `default` nunca es un permiso y `EditCheckStatus.Waiting`.

**Menores → F4-T3C:**
4. `TakeLossNotices` y `TakeCloseNotices` contenidos, y el fallo de `GetRacksAtRisk` como incierto;
6. `DropCloseNotice` dentro de la contención de `SessionDestroyed`.

Hasta entonces son inalcanzables o de evidencia; el producto entregado es correcto.

**Oráculo crítico:** cero escrituras authored. Se sostiene por la guarda por cuerpo de las API de escritura sobre las 12 operaciones y el decorador, con autopruebas, y por la ausencia de dependencia de escritura de Application. El espía de las pruebas de host no está conectado (menor 1).

**Controles:**
- nc2 se satisface con un sustituto determinista, y **PD-F4-01** se repite por segunda vez;
- por orden del Owner, en T3B y T3C la firma exacta reutiliza el patrón sin volver a él;
- nc1 no reproduce PD-F4-04.

**Condición previa conocida:** la suite completa de UI en Release da 2 fallos de `PendingTextFieldTests` por `#if DEBUG`. En Debug, que es lo que corre la CI, son 0.

**Siguen abiertos:**
- **R-T2C-01:** DEFERRED a F6/F7 por el Owner; no se pierde.
- N-2 y la compuerta de `ReadPreviewWitness`, latentes hasta F5.
- `Stop()` sin asesor, para F7.

**Smoke-2:**
- `Authorize` a cada pulsación en CLOSE, CLOSEALL o QUIT y en un cambio de documento;
- drenajes o retiradas durante una lectura;
- el trabajo diferido fuera de la compuerta de avisos;
- los avisos de veto sin `StateChanged`;
- la edición con un comando activo o en modo degradado, y un `BeginDocumentClose` sin enganchar;
- el orden de la marca modal frente a la ventana;
- la cultura con ajustes de usuario de Windows.

**Sin AutoCAD:** la lógica se prueba en Core con dobles; el Plugin, con guardas de texto y el build Release de la CI.
