Decisión de la sesión, delegada por la orden del Owner de autonomía nocturna (`owner-overnight-autonomy-2026-11-13.md`), tomada el 2026-11-13 tras las revisiones y los controles del intento 2 (corrección) de F4-T3B1-EDIT-SURFACE. No es una respuesta del Owner: el Owner no la vio.

**Revisiones del intento 2:**
- La revisión directa de la sesión da el producto conforme.
- La re-revisión adversarial independiente da ACEPTABLE CON MENORES (0/0/3): el bloqueante de teclado del intento 1 queda resuelto, y sus menores, cerrados. Los tres menores nuevos son N-1 (el doble no reproduce el `Kept` de un no editable en `TabChange`), N-2 (Space, desviación de letra) y N-3 (las repeticiones de una tecla mantenida tras abrir la caja).
- Para las revisiones, el criterio de F4-T2B2B, F4-T2C y F4-T3A sería cerrar y arrastrar N-1 y N-3 a F4-T3B2, con N-2 declarada.
- La verificación del Controller dio `EXECUTION_VERIFIED` 14/14.

**Controles del intento 2** (sobre las entradas de esa verificación):
- **nc1: CONTROL NOT PASSED, con un modo NUEVO.** El Controller no verificó la entrega mutada (REWORK, `VerifiedSha` nulo), con Handoff y Ci en fail, pero dio Identity en pass ante el CurrentSha falso. No es la firma exacta de PD-F4-04 (Identity en fail, BLOCKED/STOP), así que la regla de nc1 de la orden de F4-T3 no aplica. Además, el sustituto establecido de identidad rechaza expresamente este caso: sus autopruebas F («Identity = pass») y G («REWORK en lugar de BLOCKED/STOP») dan FAIL.
- **nc2:** la firma de PD-F4-01 (Scope en fail sobre la ruta quitada, Contract en fail por la exactitud, BLOCKED/STOP), con Ci y Trailer en fail además por variación del Controller. Por la regla de nc2 de la orden de cierre de F4-T3A, se reutiliza el sustituto F4T3B1-NC2-EXACT-WHITELIST-01: PASS.
- **nc3:** FreeText en fail y REWORK, la señal esperada. Pero la misma variación de Ci (el Controller exige que el job de Core falle en el RED, contra `ExpectRed` false) y de Trailer (busca un Trailer de nivel superior que está en `Worker.Trailer`) pone Ci como primera en fail. Ninguna regla del Owner cubre este caso.

**Decisión:** la sesión **no se dispensa a sí misma** dos controles no superados, sin regla del Owner que los cubra (precedente F4-E28: «la sesión no se dispensa a sí misma ni reescribe los resultados»). Fabricar un sustituto más débil que el ya aceptado por el Owner sería rebajar el listón del control.
- F4-T3B1 queda **VERIFICADA sobre `1708e764`, con el cierre PENDIENTE de la disposición del Owner sobre nc1 y nc3**. Se conservan los resultados ordinarios y nc2 queda satisfecho por su sustituto.
- **Deudas nuevas:**
  - PD-F4-05: Identity en pass ante un CurrentSha falso en nc1, con la entrega rechazada por Handoff;
  - PD-F4-06: variación del Controller en Ci y Trailer en los controles, con las mismas entradas que la verificación real dio en pass.
- **Propuesta al Owner:**
  - (a) aceptar nc1 con un sustituto de identidad adaptado (Identity determinista por la sesión: el CurrentSha falso no es HEAD ni existe; y el Controller no verificó la entrega mutada);
  - (b) aceptar nc3 con el detector determinista de FreeText (patrón F4T2B1-NC3-FREETEXT-DETECTOR-01);
  - (c) cerrar F4-T3B1 como PASS WITH LIMITATIONS, con N-1 y N-3 a F4-T3B2 y N-2 declarada.
- Mientras tanto, la sesión prepara y somete a revisión el paquete de F4-T3B2, sin emitir su contrato hasta el cierre de F4-T3B1.
