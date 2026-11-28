# I-64 F4B — Decisión del Owner sobre la parada de PB-T3b (intento 1): renombrar los métodos nuevos, dentro de F4B-E45

**Fecha:** 2026-11-20. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación. La pregunta y la respuesta se copian literales.

## La parada

El Worker `R20261120T055235Z-a45b`, sobre la delegación `R20261120T054837Z-814c`, paró con **PBT3B-C02 y PBT3B-C07 antes del commit RED**. No hizo commits ni push. HEAD sigue en `eab27a9e` y el árbol está limpio.

**Lo que falla:** con el esqueleto exacto del §6.2 del paquete v4.5, `RackCad.Tests` no compila. Da dos errores CS0121, de llamada ambigua, en pruebas existentes de F4 que el contrato prohíbe editar:
- **`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs:669`:** `PushBackPilotUpdateRun.Execute(null!, setup.Request, setup.Port)` es ambigua entre `Execute(DraftSession<PushBackFullDraftState>, …)`, que es nueva (D-PBT3-10), y `Execute(DraftSession<PushBackPilotDraftState>, …)`, que es la existente.
- **`…/WorkspacePilotUpdateRunTests.cs:799`:** `PushBackPilotUpdateRun.ComparePost(plan, null)` es ambigua entre `ComparePost(PushBackPilotUpdatePlan, PushBackFullIntentResult?)`, que es nueva, y `ComparePost(PushBackPilotUpdatePlan, PushBackPilotIntentResult?)`, que es la existente.

**La comprobación de la sesión:** la sesión comprobó los dos errores en `tests/s0/build-core-debug.log` del Worker y las dos líneas en `eab27a9e`.

**La causa:** es un defecto del paquete, no del Worker. El paquete afirma que ninguna firma existente cambia y que las pruebas de F4 compilan contra ellas. Las firmas no cambian, pero añadir una sobrecarga con el mismo nombre vuelve ambiguo el literal `null`. El barrido de guardas del §4.1 buscaba colisiones de nombres en el texto y no comprobaba la resolución de sobrecargas del compilador.

**Lo que dejó el Worker:** no eligió ninguna salida. Dejó `blocked-skeleton.patch`, el esqueleto probado sin aplicar, y las líneas base de `BaseSha`: Core, UI y Plugin en Release con 0 errores, y el filtro Workspace con 3 482 de 3 482.

## Pregunta

> PB-T3b, intento 1 de 3: el Worker se detuvo antes del RED, sin commits. El paquete v4.5 manda añadir al plan dos métodos nuevos para el modelo completo con el mismo nombre que dos existentes (Execute y ComparePost), y eso vuelve ambiguo un `null` en dos líneas de pruebas existentes de F4 (WorkspacePilotUpdateRunTests.cs:669 y :799). No compila. Lo verifiqué con el log del compilador (CS0121). ¿Cómo lo corrijo?

**Opciones:**
- **«Renombrar los métodos nuevos (Recomendado)»:** Como con ForWrite en PB-T3a: los dos métodos nuevos pasan a tener nombres propios del modelo completo. Las firmas existentes y las pruebas de F4 no se tocan. Corrijo el paquete (con lo ya redactado para la v4.6), reemito PB-T3b como intento 2 de 3 con su análisis y su comprobación acotada, y antes compilo el esqueleto corregido en una copia aparte para no repetir esto.
- **«Editar las dos líneas de F4»:** Autorizas cambiar solo esas dos líneas de pruebas de F4 (añadir el tipo al `null`) como edición declarada. El API del paquete queda como está. Entra en el alcance un archivo de pruebas de F4 que hoy está prohibido.
- **«Parar PB-T3b aquí»:** No corrijo nada todavía y PB-T3b queda bloqueada hasta que lo revises tú.

**Respuesta del Owner:** «Renombrar los métodos nuevos (Recomendado)».

## Lo que implica

**El paquete de PB-T3 corregido:** la v4.6, que incorpora lo ya redactado para PB-T3c.
- **Los nombres nuevos:** las dos sobrecargas nuevas de `PushBackPilotUpdateRun` para el modelo completo se renombran. Los nombres que fija la sesión son `ExecuteFull` y `ComparePostFull`, con la misma firma. Están libres en `eab27a9e`.
- **Lo que cambia con ellos:** todo lo que el paquete hace depender de esas dos sobrecargas en PB-T3b y en PB-T3c: D-PBT3-09, D-PBT3-10, la lista cerrada de PBT3B-C13, el esqueleto del §6.2 y las CT que las nombran.
- **Lo que no se toca:** ni las firmas existentes ni ninguna prueba existente.
- **La comprobación nueva antes de reemitir:** la sesión compila el esqueleto corregido sobre una copia exportada de la base, fuera del worktree, con Core, las pruebas de Core, UI y las pruebas de UI. El barrido de guardas se amplía con la resolución de sobrecargas: hay que buscar las llamadas existentes con `null` o `default` a los métodos que reciben una sobrecarga nueva.

**La comprobación acotada de la v4.6:** antes de reemitir.

**La reemisión del contrato** de F4B-PBT3B-FULL-WRITE-RUN con la v4.6, y **la corrección en el intento 2 de 3**, con el análisis de la sesión. La delegación nueva puede partir del `blocked-skeleton.patch` del intento 1, con los nombres cambiados.

**Lo que no cambia:** el diseño, D-1 y la reparación del RED (RR, F4B-E35).

## Nota posterior de la sesión: un tercer método con el mismo remedio

La pregunta nombró dos métodos, los de los dos errores de compilación. El barrido ampliado de la v4.6 revisa también la resolución de sobrecargas y la reflexión, y encontró un tercer choque. El compilador no lo detecta y solo saldría al ejecutar.

**El choque:** la prueba existente de F4 `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs:2155-2160` comprueba por reflexión (`Assert.Single(members, m => m.Name == "Prepare")`) que `PushBackPilotUpdatePlanner` tiene un único método público estático `Prepare`, con la firma del piloto. La `Prepare(PushBackFullPrepareInput, PushBackFullIntentResult, RackCatalog)` nueva del esqueleto haría fallar esa prueba en el RED.

**Lo que hace la sesión:** aplica el mismo remedio que eligió el Owner. El método nuevo pasa a llamarse **`PrepareFull`**, con la misma firma, y ninguna prueba existente se toca. No es una decisión nueva: es la misma regla, renombrar el método nuevo que choca, aplicada a un tercero que la pregunta no conocía. La sesión lo declara aquí, en el análisis y en F4B-E45, y se lo comunica al Owner.

**La comprobación antes de reemitir:** la copia exportada se compila y además ejecuta la suite completa de Core. Así se detecta un choque que solo aparece al ejecutar.
