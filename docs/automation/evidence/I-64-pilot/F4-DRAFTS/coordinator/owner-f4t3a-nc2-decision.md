Pregunta de la sesión al Owner (AskUserQuestion, 2026-11-13), tras los controles negativos de F4-T3A-EDIT-PORT:

F4-T3A: nc1 y nc3 dan CONTROL PASS por el motivo correcto (nc1: Identity primera en fail, sin necesidad del sustituto; nc3: FreeText con REWORK). nc2 da CONTROL NOT PASSED con el MISMO modo de fallo que nc2 de F4-T2B1 (deuda PD-F4-01): el Controller detecta la ruta quitada, con Scope en fail nombrando IWorkspacePilotEditPort.cs y F4T3A-C01, y da EXECUTION_BLOCKED/STOP. Pero también marca Contract en fail, que va antes en el orden de 16.9, porque con la lista blanca exacta (F4-E21) la delegación estrechada ya no coincide con el contrato. En T2B1 lo resolviste con el sustituto determinista F4T2B1-NC2-EXACT-WHITELIST-01: 10 propiedades, entre ellas que Scope tiene que fallar, que Contract falla solo por la cláusula de exactitud y que el Controller informó Scope en fail. La regla de relevo de F4-E29 me obliga a devolvértelo antes de aplicar un sustituto por analogía, y la regla de nc1 de la orden de T3 no cubre nc2. Guardo nc2 tal cual. ¿Qué hago?

Opciones ofrecidas:
- Sustituto + regla T3 (Recomendado): Copio el sustituto de T2B1 como F4T3A-NC2-EXACT-WHITELIST-01, cambiando solo el identificador y el ámbito; ejecuto su autoprueba dos veces y compruebo las 10 propiedades. nc2 queda satisfecho bajo ese sustituto y cierro T3A. Para T3B y T3C, si se reproduce exactamente esta firma (Scope en fail sobre la ruta quitada, Contract en fail solo por la exactitud, BLOCKED/STOP), reutilizo el patrón sin volver a preguntarte, como en la regla de nc1, registrando cada uso.
- Sustituto + cerrar: Lo mismo para T3A (sustituto F4T3A-NC2-EXACT-WHITELIST-01, nc2 satisfecho, cierre), pero sin regla permanente: si vuelve a pasar en T3B o T3C, te lo devuelvo otra vez.
- No cerrar todavía: T3A queda VERIFICADA sobre 18471988 con el cierre pendiente; no aplico ningún sustituto hasta que des otra disposición.

Respuesta del Owner: Sustituto + regla T3 (Recomendado)
