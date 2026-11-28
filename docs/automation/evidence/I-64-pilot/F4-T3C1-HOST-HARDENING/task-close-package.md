# I-64 — F4-T3C1-HOST-HARDENING: paquete de cierre

Lo prepara la sesión responsable el 2026-11-13. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E47 y F4-E48.

**Autoridades del cierre:**
- La disposición del Owner y del Coordinator (`F4-DRAFTS/coordinator/owner-f4t3b1-close-disposition.md`, F4-E43) hace a F4-T3C dueña:
  - del endurecimiento del fallo o desconocido de `GetRacksAtRisk`;
  - de los menores arrastrados;
  - de lo demás de F4-T3C.
- El Owner dividió F4-T3C en T3C1 y T3C2 (`owner-f4t3c-decomposition-decision.md`, F4-E47).
- El Owner decidió el cierre de T3C1: «Cerrar y arrastrar» (`owner-f4t3c1-close-decision.md`, F4-E48).

Este paquete cierra una subtarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `7bd275e42b579ef62944ff5f9dec1c5d9c58eb39`, CI `43477341374` push 4/4 |
| Verificación | `R20261113T181650Z-979b`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `c04ddfb3` (la emisión del contrato); RED `b845b9a6`, GREEN `7bd275e4` |
| Contrato | `gate-contract.json` (AuthorityRevision `3a53ee2d`, el paquete de F4-T3C v3) |
| Intentos | 1 de 3 |
| Config | Snapshot H (F4-E46) en todas las comprobaciones de la cadena |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E48; limitaciones en el §5) |
| F4-T3 | T3A, T3B1, T3B2 y T3C1 cerradas; T3C2 en espera de su paquete |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `8f54` | aceptada a la primera | fidelidad PASS en las ocho colecciones, A1-A8 8/8, Horizonte Medium |
| `95e5` (nc4) | con su oráculo | solo A3 en fail |
| `f35e` | entrega | Worker `claude-sonnet-5-5`/high, 282 turnos y 0 denegaciones. RED `b845b9a6`: fallan 29 ejecuciones de pruebas nuevas (15 de Core y 14 de UI), ninguna existente. GREEN `7bd275e4`: las 12 rutas del contrato; no toca `tests/` |
| `979b` | `EXECUTION_VERIFIED`, 14/14 | sobre `7bd275e4`, con el RED acreditado |

**CI exacta:**
- RED `43472387303`: Core y UI en failure, como piden los dos `ExpectRed` true.
- GREEN `43477341374`: los cuatro jobs en success.
  - Filtros: 2517/2517 en Core y 454/454 en UI.
  - Suites completas: Core 18055/18055; UI con 2091 pasadas, 0 fallidas y 17 condicionales ya existentes sin ejecutar.

## 3. Revisiones y controles

- **Revisión directa:** producto conforme, sin hallazgos (`direct-review.md`).
- **Revisión adversarial independiente:** ACEPTABLE CON MENORES (0/0/6) (`independent-review.md`; clasificación en `analysis.md`).
- **Controles** sobre las entradas de `979b`, con su oráculo ordinario. Para nc2 se aplicó la regla de nc2 de la orden del Owner de cierre de F4-T3A, vigente para F4-T3B y F4-T3C: con la firma exacta de PD-F4-01, el sustituto de lista blanca exacta de la subtarea, sin volver al Owner. Es el segundo uso de la regla, tras F4-T3B1.

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261113T182131Z-f473` | CONTROL PASS: Identity única en fail, `EXECUTION_BLOCKED` / STOP |
| nc2 (sin la primera ruta del diff en el alcance) | `R20261113T182403Z-95b0` | CONTROL NOT PASSED con la firma exacta de PD-F4-01: Scope en fail sobre `IWorkspacePilotEditPort.cs` con F4T3C1-C01; Contract en fail solo por la exactitud; `EXECUTION_BLOCKED` / STOP; ninguna otra distinta. Se conserva tal cual |
| sustituto de nc2 `F4T3C1-NC2-EXACT-WHITELIST-01` | `R20261113T182852Z-a04c` | **PASS**: autoprueba de 14 casos determinista y las 10 propiedades. nc2 = SATISFIED_UNDER_F4T3C1_EXACT_WHITELIST_SUBSTITUTE |
| nc3 («GATE PASS» en `WorkCompleted`) | `R20261113T182900Z-f4c5` | CONTROL PASS: FreeText única en fail, `EXECUTION_REWORK_REQUIRED` / REWORK |

## 4. Producto entregado

Endurecimiento del host del piloto y menores de F4-T3B2, **sin Plugin**. No hay escritura authored, ni lectura nueva de la intención, ni costuras. No cambian el controlador, el puente, los comandos, `BrowserSession`, el XAML, la llamada a `IsSessionAlive` ni el `Refresh` de F3.

- **D-T3C1-01 y D-T3C1-02. El desconocido explícito:**
  - `PilotRiskView.IsKnown`; `default` es el desconocido, nunca «nada en riesgo»;
  - `RequestDocumentDiscard` y `ConfirmDiscard` no piden confirmación ni descartan con un desconocido, y lo dicen con su texto exacto;
  - la confirmación de solo inciertos tiene su texto propio.
- **D-T3C1-03. Las colas:**
  - `TakeLossNotices` convierte cada aviso en su `try`;
  - `TakeCloseNotices` envuelve su cuerpo en un `try` y no vacía la cola si falla.
- **D-T3C1-04:** `DropCloseNotice` va dentro del primer `try` de `SessionDestroyed`, después de la retirada.
- **D-T3C1-05:**
  - los 28 motivos del host y del soporte (tabla H-07) están en español exacto;
  - `RefusalOf` y `WhyOf` ya no duplican el motivo.
- **D-T3C1-06:** `PilotFieldView.ErrorReason`, como último parámetro opcional. El host lo da solo en Error y el campo lo muestra.
- **D-T3C1-07. Los menores de F4-T3B2:**
  - M-1: la confirmación se quita en el `catch` del cierre limpio;
  - M-2: la retención y la confirmación «no se pudo comprobar»;
  - M-3: la contención del constructor del panel;
  - M-4: la expresión de declaración;
  - M-5: autopruebas en negativo y la forma pública de 33 firmas;
  - M-7: la guarda y las pruebas de `ContainFailure`.
  - M-6 y D-1 no cambian.

## 5. Limitaciones y abierto

**Menores de la revisión independiente** (decisión del Owner, «Cerrar y arrastrar»):
- **→ F4-T3C2:**
  - R-1: ninguna prueba discrimina la quita del prefijo duplicado en `WhyOf`; hace falta una prueba de texto exacto;
  - R-2: la prueba de `_confirmUnread` no discrimina que otra petición, sin cancelar, lo borre;
  - R-4: la documentación de `TakeLossNotices` y `TakeCloseNotices` («leaves the queue empty») y la de `RequestDocumentDiscard` y `ConfirmDiscard` no reflejan el `try` ni el desconocido.
- **→ F4-T4:** R-3, porque «`ErrorReason` solo en Error» del host no está discriminado. Es latente: F4-T4 es la que pone borradores en Error.
- **Declarados aquí:**
  - R-5: el ayudante `View(...)` del doble ganó un `errorReason` opcional. Es inocuo, dentro del alcance, y no cambia la respuesta por defecto ni ninguna aserción;
  - R-6: el desconocido no lleva motivo en el valor y va solo al registro. Además, hasta R-C14 de F4-T3C2, el campo mezcla los motivos del host en español con los del lector del Plugin en inglés.
- **Para el paquete de F4-T3C2:** `LiteralViolations` fija el número de apariciones de cada texto en español en el host y en el soporte. Una reutilización en esos archivos exige un cambio declarado de la guarda.

**Limitaciones de D-T3C1-08:**
- los motivos de F3 siguen en inglés (F7);
- `ErrorReason` es latente hasta F4-T4;
- un aviso de pérdida cuya conversión lanza se pierde y solo queda en el registro;
- los `catch` externos de `GetRacksAtRisk` y de las dos colas son inalcanzables. Solo los cubren las guardas por cuerpo y las pruebas de `default` y del constructor; las costuras están prohibidas.

**Siguen siendo de F4-T3C2** (F4-E47):
- `RACKPANELDESCARTAR` y sus mensajes en la línea de órdenes;
- el envoltorio del asesor;
- el aviso del panel con la vía del comando;
- los censos de comandos;
- M-1 de F4-T2B2B;
- los motivos del Plugin que llegan al campo (R-C14).

**Smoke-2** (riesgos de la revisión independiente):
- desplegar juntos `Application`, `UI` y `Plugin`, porque cambia la firma del constructor de `PilotFieldView`;
- recrear la paleta con un veto en cola y comprobar que «Reintentar» recupera un Error del panel;
- la mezcla de idiomas del campo hasta F4-T3C2;
- la fila del veto al cerrar el documento;
- el ajuste de los textos largos en la paleta estrecha;
- el foco y el modal, que siguen pendientes.

El desconocido, la retención con `GetField` que lanza y `ErrorReason` no se pueden ejercitar en el smoke.

**Proceso:**
- **Un hecho viejo del prompt de verificación, corregido antes de lanzar `979b`.** El prompt de verificación clonado de F4-T3B2 decía «SNAPSHOT G … F4-E13», pero el config es la Snapshot H desde F4-E46.
  - Las verificaciones `f5e9` y `4f9b` de F4-T3B2 y sus controles llevaban ese mismo hecho viejo. Sus registros de relevo midieron el hash real H, aceptado, y no causó ningún fallo.
  - No se reabre F4-T3B2 ni se altera su evidencia.
- **Lección:** al clonar la cadena de una subtarea, cada hecho fijado en un prompt (snapshot, BaseSha, conteos) se vuelve a medir en el momento de lanzar.
- **Deudas de protocolo:**
  - PD-F4-01 se repite en nc2 con su firma exacta. Es su cuarta aparición: F4-T2B1, F4-T3A, F4-T3B1 y F4-T3C1. Sigue sin resolverse de forma global dentro de I-64;
  - PD-F4-04, PD-F4-05 y PD-F4-06 no aparecieron.

**Sin AutoCAD:** pruebas del host con dobles, del VM y de la vista; el Plugin, con el build Release de la CI.
