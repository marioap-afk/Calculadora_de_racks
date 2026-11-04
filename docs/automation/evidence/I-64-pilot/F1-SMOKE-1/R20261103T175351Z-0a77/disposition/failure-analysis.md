# I-64 Smoke-1 R20261103T175351Z-0a77 — Análisis de fallos (COORDINATOR_SMOKE1_FAILURE_ANALYSIS_REQUIRED)

Candidato 09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8. Disposición del Coordinator: NOT PASS; ocurrencia válida; desviación de rol
ACEPTADA COMO DESVIACIÓN DE PROCEDIMIENTO (operó la sesión, no el Owner; T3D §1 no se reescribe). C1 sigue PASS.
Este análisis no cambia producto ni ningún resultado SM. Se basa en: registro del operador, bitácora de la sesión, capturas de computer
use (horas tomadas de la marca de tiempo de cada captura, ±1 s), RackLog del día y lectura del código del Candidato. Sin rutas locales.
Registro del operador firmado por el Owner («Firmo el registro.», 2026-11-04T15:06:53Z): la firma confirma el registro, no un PASS.

## SM-13 — FAIL (S1-06f)

| Elemento | Dato |
|---|---|
| Acción exacta | Dibujo A activo («Sesión 1»), paleta flotante y visible, instrumentación activa (42 puntos). Clic en la línea de órdenes; `LINE` + Enter (~20:14:55Z). No se marcó ningún punto. Espera >10 s con movimientos del ratón sobre el dibujo (capturas 20:14:59Z, 20:15:04Z, 20:15:13Z). `Esc` (~20:16:01Z). |
| Esperado (T3D línea 200) | «Leer el dibujo» y «Escribir en el dibujo» desmarcados; «Hay un comando en curso.»; después `Esc`: vuelven a marcarse y el texto desaparece. |
| Observado en la paleta | Durante la orden: «Workspace» / «Sesión activa» / «El panel sigue el dibujo activo.» / «Sesión 1»; «Leer el dibujo» MARCADO; «Escribir en el dibujo» MARCADO; «Selección» = «Ninguno» / «No hay objetos seleccionados.»; NINGÚN «Hay un comando en curso.». Tras `Esc`: igual (las casillas nunca se desmarcaron). |
| Disponibilidad lectura/escritura mostrada | CanReadDrawing = true, CanWriteDrawing = true (casillas marcadas), sin texto de bloqueo, en todo momento. |
| Línea de órdenes | «Command: LINE» / «LINE Specify first point:»; tras `Esc`: «Specify first point: *Cancel*». |
| Instrumentación | Antes de la orden: 42 puntos (últimas filas 37 panel.hide, 38 panel.open, 39 panel.show, 40 cache.inva… 0.1 ms, 41 selection.read 0.1 ms, 42 drain 0.2 ms 4, de SM-12). Durante la orden: 42 (ninguna fila). Tras `Esc`: 44 → 43 cache.invalidate 0.0 ms 1956.8 MB 0; 44 drain 0.0 ms 1956.8 MB 2. |
| DBMOD | No se midió en SM-13; SM-14h (20:17Z) → 16 (bit 1 = 0, igual que SM-02; el 16 es vista, de la rueda en SM-06). |
| RackLog en el intervalo | Ninguna entrada entre SM-00a (20:00:36Z) y 20:24:21Z (el archivo del día empieza a las 20:24:21Z). |
| Repetición | SM-15 (20:18:29Z, `LINE` desde el dibujo) y SM-33 (RACKAYUDA abierto) mostraron lo mismo: casillas marcadas, sin el texto. |

**Ruta de código responsable (lectura, sin cambios):**
1. `AutoCadEventBridge.Hook` engancha `Document.CommandWillStart` → `EventBridgeCore.OnSourceEvent(session, CommandWillStart)`.
2. `EventBridgeCore.OnSourceEvent` (EventBridgeCore.cs:255-257): `case BridgeSource.CommandWillStart: state.Quiescent = false; return;`
   — sin pista, sin `SyncIdle` y sin ningún aviso hacia el controlador. Lo mismo `EnteringQuiescentState` (258-260).
3. La paleta solo se repinta con `WorkspaceController.NotifyStateChanged` → `WorkspacePaletteHost.OnControllerStateChanged` →
   `_viewModel.Refresh()` (WorkspacePaletteHost.cs:121-131). `NotifyStateChanged` se llama solo desde `OpenPanel`, `ShowPanel`,
   `HidePanel`, `OnActiveSessionChanged` y `OnDrained` (WorkspaceController.cs:63-117).
4. El cálculo es correcto cuando se pide: `WorkspaceHostCore.GetSnapshot` → `WorkspaceHostPolicy.Actions(conditions)` con
   `Quiescent = _probe.IsActiveDocumentQuiescent` (= `Editor.IsQuiescent`) → `WorkspaceActionBlock.CommandActive` → «Hay un comando en
   curso.» (WorkspacePanelViewModel.cs:287). La prueba `WorkspaceHostLifecycleTests.AnActiveCommandDisablesTheActionsThatReadOrWriteTheDrawing`
   lo cubre por EXTRACCIÓN (pull) del snapshot; ninguna prueba exige que el inicio de una orden PROVOQUE el repintado.
5. Restricción vigente del contrato T2: `WorkspaceBridgeHintAndDrainTests.SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle`
   fija que `CommandWillStart` y `EnteringQuiescentState` no encolan pistas ni enganchan `Idle`. Una corrección debe avisar a la vista
   sin romper eso (aviso de presentación, sin pista, sin drenaje y sin leer el dibujo).

**Lo que no falló:** el drenaje queda inhibido con una orden en curso (`MayDrain` exige `Quiescent`; durante LINE no hubo filas), y
tras la orden la pista `CommandEnded` produjo `cache.invalidate` + `drain`. Lo que falla es la PRESENTACIÓN de la disponibilidad.

**Recomendación: PRODUCT_CORRECTION** — la expectativa de T3D coincide con S1-06f de la propuesta («Con un comando de AutoCAD
activo, las acciones del panel que leen o escriben el DWG están deshabilitadas») y con la propia política del producto (bloqueo
`CommandActive`); el producto calcula bien pero no se repinta. Requiere decisión de diseño del Architect por la restricción del
contrato T2 (punto 5). No se propone PROCEDURE_CHANGE.

## SM-34 — FAIL (S1-07, OV-17; D-T3D-06)

| Elemento | Dato |
|---|---|
| Secuencia exacta de RACKEDITAR | Pestaña «Dibujo B» (≈20:27:25Z) → panel «Sesión 8», «Ninguno», 140 puntos. Clic en la línea de órdenes; `RACKEDITAR` + Enter → «Command: RACKEDITAR» / «RACKEDITAR Selecciona un rack para editar:» (captura 20:27:30Z). Clic en el rack superior de B → ventana «RackCad – Rack selectivo: editor avanzado (vista frontal)», «Nombre (Rack A)» = «Selectivo 1», «2 frentes · 3 cabeceras · 6 largueros», «Vista actualizada.» (captura 20:27:39Z). Espera 4 s. Botón «Cerrar» (nunca Actualizar/Insertar) (~20:27:48Z). |
| Esperado (T3D línea 236) | El panel no interfiere con el editor; mientras dura la orden: «Hay un comando en curso.». No se exige «Hay un cuadro de diálogo de RackCad abierto.». |
| Observado en la paleta | En el aviso de selección y con el editor abierto: «Sesión activa» / «El panel sigue el dibujo activo.» / «Sesión 8»; «Leer el dibujo» y «Escribir en el dibujo» MARCADOS; «Ninguno» / «No hay objetos seleccionados.»; 140 puntos; NINGÚN «Hay un comando en curso.». |
| ¿Interfirió la paleta con RACKEDITAR? | No. El editor abrió, mostró el rack y se cerró con normalidad; la orden terminó («Selecciona un rack para editar:» y vuelta a «Command:»). |
| ¿Se detectó la orden activa? | No es observable desde fuera: por código, `CommandWillStart` pone `Quiescent = false` en el núcleo (detección interna), pero nunca llega a la vista (misma ruta que SM-13). Sin filas durante la orden (140 → 140), coherente con el drenaje inhibido. |
| Estado tras «Cerrar» | «Sesión activa», «Sesión 8», casillas marcadas, «Ninguno»; 142 puntos (2 filas nuevas tras el fin de la orden; su detalle no se capturó — por analogía con SM-13, `cache.invalidate` + `drain`, sin verificar). `(getvar "DBMOD")` → 0 (20:28:09Z). |
| Contexto antes / después | Antes: B, «Sesión 8», «Ninguno». Después: B, «Sesión 8», «Ninguno». Sin cambio de documento ni de selección. |
| Instrumentación / RackLog | 138 al final de SM-33 (C) → 140 al activar B → 140 durante la orden → 142 tras «Cerrar». RackLog: ninguna entrada entre 20:24:40.789Z y el cierre de AutoCAD. |
| Nota D-T3D-06 | `SetModalRackCadActive` sigue sin llamador (conocido y aceptado): la modal no se señaliza; T3D lo resuelve exigiendo solo «Hay un comando en curso.». |

**Ruta de código responsable:** la misma que SM-13 (puntos 1-5): `EventBridgeCore.OnSourceEvent` ante `CommandWillStart`
(EventBridgeCore.cs:255-257) no avisa a la vista; `WorkspaceController.NotifyStateChanged` no se dispara; la paleta conserva el último
snapshot. RACKEDITAR no aporta una ruta distinta.

**Recomendación: PRODUCT_CORRECTION** — la misma corrección de SM-13 debe hacer pasar SM-34; verificar en host con RACKEDITAR
después. No hace falta PROCEDURE_CHANGE (T3D ya aceptó D-T3D-06 con este texto).

## SM-39 — FAIL (RackLog)

Archivo del día: 1720 bytes, 8 líneas, SHA-256 1F9B1E6FF4466C98AB425EF5AB5590A79891F33D712F165E3D5C4733D82D8865. Entradas nuevas desde
SM-00a: exactamente 2 (rutas de compilación sustituidas por `<worktree>`):

```text
[2026-11-03T20:24:21.278Z] ERROR AutoCadEventBridge.DocumentActivated: System.NullReferenceException: Object reference not set to an instance of an object.
   at RackCad.Plugin.Workspace.AutoCadEventBridge.NativeId(Document document) in <worktree>\src\RackCad.Plugin\Workspace\AutoCadEventBridge.cs:line 251
   at RackCad.Plugin.Workspace.AutoCadEventBridge.<>c__DisplayClass20_4.<Hook>b__5() in <worktree>\src\RackCad.Plugin\Workspace\AutoCadEventBridge.cs:line 123
   at RackCad.Plugin.Workspace.AutoCadEventBridge.Contain(String context, Action body) in <worktree>\src\RackCad.Plugin\Workspace\AutoCadEventBridge.cs:line 279
[2026-11-03T20:24:40.789Z] ERROR AutoCadEventBridge.DocumentActivated: System.NullReferenceException: Object reference not set to an instance of an object.
   (misma traza: líneas 251, 123, 279)
```

| Línea | Severidad / contexto | Paso | Correlación (capturas) |
|---|---|---|---|
| 20:24:21.278Z | ERROR, `AutoCadEventBridge.DocumentActivated`, NullReferenceException contenida | **SM-26** (cerrar C, el último dibujo) | 20:24:17Z: aviso «Save changes to …Dibujo C.dwg?»; clic «No»; 20:24:23Z: solo la pestaña Start, panel «Sin documento». |
| 20:24:40.789Z | ERROR, mismo contexto | **SM-27** (clic en la pestaña Start, con C ya reabierto, para abrir B) | 20:24:36Z: C reabierto («Sesión 7»); clic en Start; 20:24:43Z: página Start, el panel sigue mostrando «Sesión activa» / «Sesión 7». |

Ninguna corresponde a SM-13 ni a SM-34. (Corrección: el registro decía que ambas eran de SM-26; la segunda es de SM-27. Fe de erratas en
la bitácora y en las notas del registro; ningún resultado cambia.)

- Diagnóstico de fuga de transacción: NINGUNO («A workspace drain left a transaction open.» y «The selection read left a transaction
  open.» ausentes; ninguna aserción Debug visible).
- Excepción no controlada: NINGUNA (las dos las atrapa `Contain`, AutoCadEventBridge.cs:275-285; no llegaron a AutoCAD; ningún
  cuadro de excepción; ninguna sesión degradada: `Contain` solo registra).
- Causa: el manejador de `DocumentActivated` (AutoCadEventBridge.cs:121-125) evalúa `NativeId(e.Document)` =
  `e.Document.UnmanagedObject.ToInt64()` (línea 251) sin comprobar nulo. AutoCAD entrega `DocumentActivated` con `e.Document == null`
  cuando pasa a la pestaña Start (sin documento): al cerrar el último dibujo (SM-26) y al pulsar Start con dibujos abiertos (SM-27).
- Efecto: en SM-26, ninguno visible (`DocumentToBeDestroyed` ya había retirado la sesión y avisado: «Sin documento» correcto). En SM-27,
  como la excepción corta el manejador antes de `_core.OnDocumentActivated` y `ActiveSessionChanged`, el panel siguió mostrando la sesión
  de C («Sesión 7») mientras se veía la página Start; se corrigió solo al abrir B («Sesión 8»). T3D no tiene paso para ese estado.

**Clasificación: PRODUCT_DEFECT** (contenido, sin daño). **Recomendación: PRODUCT_CORRECTION** — tratar `e.Document == null` como
«sin documento activo» (sin excepción y con aviso a la vista), con prueba del adaptador/guarda. No es EXPECTED_DIAGNOSTIC (el log
es una excepción no prevista) ni PROCEDURAL_ARTIFACT (cerrar el último dibujo y pulsar Start son acciones normales; la de SM-27 la
hizo el operador para abrir B desde Start, acción que T3D no prescribe pero que un usuario hace a diario).

## Relación causal entre los tres

- **SM-13 y SM-34: misma causa candidata** — la vista no recibe aviso cuando empieza una orden (`CommandWillStart` sin notificación).
  Son dos síntomas con la misma ruta; se mantienen como dos hallazgos, como pide el Coordinator.
- **SM-39: independiente** — otro evento (`DocumentActivated`), otra línea, otro momento (SM-26/SM-27, no durante una orden) y otra
  causa (falta de comprobación de nulo). No causa ni es causado por SM-13/SM-34. Solo comparten capa (adaptador de eventos → vista).

## Resumen de recomendaciones

| Paso | Clasificación / causa | Recomendación |
|---|---|---|
| SM-13 | falta de repintado al empezar una orden | PRODUCT_CORRECTION (con decisión de diseño por el contrato T2 de `CommandWillStart`) |
| SM-34 | misma ruta que SM-13 | PRODUCT_CORRECTION (la misma corrección; verificar en host) |
| SM-39 | PRODUCT_DEFECT: `DocumentActivated` con documento nulo | PRODUCT_CORRECTION (guarda de nulo + aviso; prueba) |

Ningún cambio de producto hecho. F2 no empieza. Sin merge ni rebase de main.
