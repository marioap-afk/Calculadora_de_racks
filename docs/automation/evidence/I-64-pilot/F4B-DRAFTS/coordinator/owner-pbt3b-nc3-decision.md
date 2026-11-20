# Decisión del Owner: nc3 de PB-T3b (F4B-E46)

**Fecha:** 2026-11-20.

**Contexto:** F4B-PBT3B-FULL-WRITE-RUN, intento 2 de 3, sobre el GREEN `c7582785`.
- La verificación `R20261120T091621Z-2b95` dio EXECUTION_VERIFIED, 14/14.
- **nc1** `R20261120T092052Z-0d2f` dio CONTROL NOT PASSED, con la firma exacta de PD-F4B-01. Su sustituto revisado, `R20261120T093907Z-71d6`, dio PASS: nc1 = SATISFIED_UNDER_PBT3B_NC1_CASCADE_SUBSTITUTE.
- **nc2** `R20261120T092518Z-aea7` dio CONTROL PASS.
- **nc3** `R20261120T093218Z-fd73` dio CONTROL NOT PASSED: EXECUTION_BLOCKED/STOP, con FailureClass Tests y PBT3B-C07.
  - La mutación de FreeText sí se detectó: FreeText fail, en el offset 4264 de WorkCompleted.
  - Antes falló Tests, que en la verificación real había pasado. Su evidencia dice que la TRX de Core del GREEN tiene 19 124 resultados, que ExpectedEvidence[14] pide 19 128, y que el diff no explica la diferencia.

**Lo que midió la sesión** (`F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/`, `count_rows.py` y su salida):
- Las clases nuevas suman 9 + 4 + 6 + 2 = 21 filas.
- `WorkspacePilotPrepareGuardTests` pasa de 29 filas en la línea base a 25. La Theory S-2 con `Skip` cuenta como una sola fila omitida, en lugar de sus 5 filas: −4.
- Las otras dos clases de guardas no cambian: 87 y 125.
- El total es 19 107 − 4 + 21 = 19 124, con 6 omitidas. Es la rama «+ 6» que el propio contrato admite; el 19 128 solo vale en la rama «+ 10».
- Ningún sustituto autorizado cubre nc3, así que la sesión no lo repitió ni le aplicó ningún sustituto, y volvió al Owner.

## La pregunta, literal

«PB-T3b (intento 2): la verificación real dio 14/14. nc1 quedó satisfecho por su sustituto autorizado (firma exacta PD-F4B-01) y nc2 dio CONTROL PASS. nc3 NO pasó: detectó bien la frase prohibida que se le inyecta (FreeText), pero antes marcó Tests como fallo porque el contrato dice «19 128 resultados de Core» y la CI da 19 124. La verificación real aceptó 19 124 con las mismas entradas. Medí la diferencia por clase: la Theory S-2 de F4, al llevar Skip, cuenta como 1 fila en vez de 5 (−4). 19 107 − 4 + 21 nuevas = 19 124, que es la rama «+6 omitidas» que el propio contrato admite; el 19 128 solo vale en su otra rama («+10»). No hay ningún sustituto autorizado para nc3. ¿Qué hago con nc3?»

Opciones:
1. **Aceptar por disposición (Recomendado):** nc3 = SATISFIED_BY_OWNER_DISPOSITION. Cierro PB-T3b (F4B-E46) con la medición por clase custodiada y la decisión literal. Desde PB-T3c, el paquete da el total exacto de cada rama de recuento para que no haya dos lecturas.
2. **Repetir nc3 una vez:** lanzo otro nc3 con las mismas entradas. Si da CONTROL PASS, cierro. Si vuelve a fallar, vuelvo a ti. El Controller puede leer igual de forma estricta la misma cláusula.
3. **Reemitir el contrato corregido:** corrijo la cifra de ExpectedEvidence[14] en el contrato y repito la verificación y los tres controles sobre el mismo GREEN. Es lo más estricto y lo más lento.

## La respuesta del Owner, literal

«Aceptar por disposición (Recomendado)»

## Lo que se aplica

- **PB-T3b:** nc3 = SATISFIED_BY_OWNER_DISPOSITION. La corrida ordinaria de nc3 se conserva tal cual, con su relevo, y nc3 no se repite.
- **Desde PB-T3c:** cuando una evidencia esperada dependa de cómo cuente la TRX, por ejemplo una Theory con `Skip`, el paquete y el contrato dan el total exacto de cada rama, y no un solo total con una alternativa en las omitidas.
