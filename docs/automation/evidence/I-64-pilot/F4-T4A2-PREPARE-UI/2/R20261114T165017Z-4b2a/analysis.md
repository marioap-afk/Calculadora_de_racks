# I-64 F4-T4A2-PREPARE-UI, intento 2: análisis de la parada de la verificación `R20261114T165017Z-4b2a`

**Objeto:**
- la entrega del intento 2: RED `f2bb4d6f`, GREEN `ae0a7b40`, sobre BaseSha `f8ccef46`;
- su verificación `R20261114T165017Z-4b2a`: **EXECUTION_BLOCKED / STOP**, FailureClass `StopCondition`, `TriggeredStopConditions` = `F4T4A2-C04`. Tests está en fail y las otras 13 comprobaciones en pass.

## 1. Lo que dice la verificación

- **Tests, evidencia literal:** los conteos de los TRX cuadran: el GREEN pasa 2923/2923 en Core y 601/601 en UI; el RED pasa 2923/2923 en Core, y en UI 598/601, con 3 fallos por aserción, exactamente las pruebas de M-1. RT y ChainRedFiles son correctos. «However, the worker records that after browser discard the pending choice is removed without A2-34 and fixes the test to expect empty NoticeText. The unit package at AuthorityRevision requires A2-34 in this Apply case (D-T4A2-02); the recorded deviation says the browser view-model would need to change, which is prohibited by F4T4A2-C04.»
- **Finding:** la misma frase. **RecommendedNextAction:** «Ask the Coordinator to resolve the A2-34 discrepancy and the F4T4A2-C04 scope limit before continuing.»

## 2. Por qué la sesión la considera una parada falsa

- **El contrato del intento 2 declara exactamente esa desviación.** La corrección de INV-F4T4A2-05 dice: «tras un descarte del navegador, la eleccion la retira el Rematerialize del navegador en silencio (sin A2-34), desviacion declarada, y la prueba fija el NoticeText real». La entrega hace lo que pide el contrato de esta delegación.
- **El análisis de la corrección** (`F4-T4A2-PREPARE-UI/1/R20261114T160452Z-872b/analysis.md`, M-3) dice lo mismo: «Se declara; la prueba fija el `NoticeText` real».
- **El navegador no cambió:** Scope está en pass, y `git diff f8ccef46..ae0a7b40` no toca `WorkspaceBrowserViewModel.cs`. C04 se dispara si haría falta cambiar el navegador, y no hizo falta: la desviación consiste precisamente en no cambiarlo.
- **La frase del Worker** «el navegador no se puede tocar (F4T4A2-C04)» explica por qué se acepta el comportamiento real. El Controller la leyó como si hubiera que cambiar el navegador.
- **La sección citada** es D-T4A2-03 del paquete (no D-T4A2-02), que es anterior a la corrección. El contrato del intento 2 la enmienda para este caso, y el contrato es la autoridad vinculante de la delegación.
- **Las revisiones:** la directa del intento 2 da el producto conforme, y la re-revisión independiente da **ACEPTABLE CON MENORES (0/0/1)**: acepta las cuatro desviaciones declaradas, entre ellas la de M-3.

## 3. Por qué la sesión no la resuelve sola

- F4-E28: la sesión no se dispensa controles a sí misma.
- La decisión del Owner para la noche (`owner-f4t4a2-overnight-decisions.md`, pregunta 2) cierra con las revisiones en 0/0 y nc1..nc3 en pass o con sustitutos autorizados. Una verificación en STOP no es 14/14, y nc1..nc3 se ejecutan sobre una verificación EXECUTION_VERIFIED.
- La pregunta 3 pide «parar esa línea y esperar» ante lo que hoy exige volver al Owner.
- La propia verificación recomienda acudir al Coordinator.

## 4. Opciones para el Owner

| Opción | Qué supone |
|---|---|
| **(a) Repetir la verificación con una nota aclaratoria (recomendada)** | Una verificación nueva, con las mismas entradas, cuyo prompt cite la corrección de INV-F4T4A2-05 y diga que C04 trata de un cambio del navegador que no se hizo (Scope en pass). La salida de `4b2a` se conserva como STOP. Si la nueva da 14/14, siguen nc1..nc3 y el cierre según la pregunta 2. No consume intento de producto |
| (b) Dispensar la parada | El Owner acepta `4b2a` como verificación con una sola desviación: Tests en fail por una lectura del Controller contraria al contrato. Siguen nc1..nc3 sobre sus entradas, con el oráculo ajustado a esa comprobación. Más rápido, pero es una dispensa de un control |
| (c) Intento 3 | Para mostrar A2-34 tras un descarte del navegador habría que cambiar el navegador, que C04 prohíbe. No resuelve nada y gasta el último intento: no recomendada |

## 5. Mientras tanto

La sesión custodia el intento 2 (planificación `f15d`, Worker `b48d`, verificación `4b2a`, las dos revisiones y este análisis) y deja el estado en espera de la decisión.

Como trabajo independiente que no depende de esta línea, prepara el paquete de F4-T1B, sin emitirlo (pregunta 3 y pregunta 4 de la noche).
