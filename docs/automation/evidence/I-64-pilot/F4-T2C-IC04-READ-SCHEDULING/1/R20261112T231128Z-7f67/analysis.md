# F4-T2C-IC04-READ-SCHEDULING, intento 1: análisis de las revisiones y disposición

**Entrega:**
- Worker `R20261112T214822Z-2c89`: RED `703bda9d`, GREEN `33e43f9f`.
- CI `43319785427` 4/4.
- RED `43316625609`: Core en rojo con 83 fallos, todos dentro de `RackCad.Tests.Workspace`.

**Verificación del Controller** `R20261112T231128Z-7f67`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `33e43f9f`. Sin términos de 16.10.

## Revisión directa de la sesión

**Producto conforme.** Sin defectos de corrección frente a INV-F4T2C-01..11 ni frente a los puntos 1-13 del Owner. El informe completo está en `direct-review.md`, junto a este archivo.

## Revisión adversarial independiente

**Veredicto:** ACEPTABLE CON MENORES (0 bloqueantes, 0 importantes, 4 menores). El código de producción es correcto. El informe completo está en `independent-review.md`.

Lo que confirma:
- ninguna lectura del DWG queda en un callback de AutoCAD;
- ningún camino de código deja `IsScheduled` en true;
- no hay bucle Idle → post → Idle ni doble lectura;
- la secuencia de cierre de F4-T2B2B no se rompe;
- las pruebas negativas discriminan implementaciones incorrectas plausibles.

| Id | Severidad | Hallazgo | Tipo |
|---|---|---|---|
| 1 | MENOR (el más relevante) | G-1 y G-2 no cubren el método intermedio `RunIdleContinuation`. Un `OnIdleTick` que drenara a través de él, o un manejador de documento que lo llamara, pasaría todas las guardas. Las pruebas de Core no ejercen el cableado del Plugin | Guardas |
| 2 | MENOR | G-3 y G-10 no fijan la forma exacta del post. `BeginInvoke(...).Wait()` (que hace `PushFrame`), un post que además llama a la acción, `ExecuteInApplicationContext` y `Dispatcher.Run()` no los caza ninguna guarda | Guardas |
| 3 | MENOR | Tres pruebas del cierre pendiente no afirman la profundidad ni el recuento de lecturas (`WorkspaceIdleSchedulerTests.cs:1027-1067`, `:1091-1105`) | Pruebas |
| 4 | MENOR | La documentación de la clase del puente no menciona el planificador ni el post, y el fallo de la pasada y el aviso Debug comparten el contexto de registro `AutoCadEventBridge.IdleContinuation` | Documentación |

**Riesgos para el smoke** (no son defectos):
- el bombeo del dispatcher en bucles anidados: `DoEvents` de otros suscriptores, diálogos de otros complementos, modales sin comando, avisos de LISP;
- el cierre (CLOSE, pestaña, API, QUIT/CLOSEALL) y R-T2C-02;
- la ventana entre el cambio real de documento y `DocumentActivated`;
- la latencia de Background bajo entrada continua;
- la continuación perdida;
- las aserciones Debug existentes;
- la reentrada de una segunda continuación dentro de la primera, que podría cerrar antes de tiempo la ventana de aviso;
- la temporización de pila en el host (D-T2C-05).

## Disposición

**Pregunta al Owner.** Antes de los controles, la sesión preguntó al Owner si cerrar o corregir (`owner-f4t2c-close-decision.md`). Una corrección solo de guardas no tiene un RED que falle contra un producto correcto.

**Decisión del Owner:** «Cerrar y arrastrar a T3 (Recomendado)». En consecuencia:
- se completan nc1..nc3;
- F4-T2C se cierra sobre `33e43f9f` como PASS WITH LIMITATIONS, sin gastar el intento 2;
- los 4 menores pasan a cambios declarados del contrato de F4-T3:
  - reforzar G-1, G-2, G-3 y G-10 sobre el método intermedio y sobre la forma exacta del post (`action=>_dispatcher.BeginInvoke(DispatcherPriority.Background,action)`);
  - prohibir `.Wait(`, `ExecuteInApplicationContext` y `Dispatcher.Run` en el puente;
  - añadir las aserciones de profundidad y recuento a las tres pruebas;
  - corregir la documentación y separar los contextos de registro del puente.

Hasta entonces, el riesgo es solo de regresión futura del cableado.
