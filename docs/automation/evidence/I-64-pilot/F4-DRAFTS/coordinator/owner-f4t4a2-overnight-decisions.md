Decisiones del Owner para la noche del 2026-11-14 (F4-T4A2 y lo siguiente), por la herramienta de preguntas de la sesión, a petición del propio Owner («Solicita ya todas las aprobaciones que crees que vas a requerir porque ya dormiré»). Preguntas, opciones y respuestas literales:

Pregunta 1 (Diseño T4A2, selección múltiple): «F4-T4A2 (paquete v1, en revisión independiente): ¿qué decisiones de diseño declaradas apruebas desde ya? Marca todas las que aceptes; las no marcadas las dejo pendientes y no emito el contrato sin ellas.»

Opciones:
- «Conservar solo con trabajo propio»: «Conservar mi borrador» se ofrece solo si el borrador tiene cambios o texto pendiente. Sin ellos solo podría devolver la escala anterior sobre un cambio externo (reversión en silencio), así que solo se ofrece «Descartar y recargar», que no pierde nada.
- «Prueba 13 según el Freeze»: El Freeze (D-10) no clasifica ninguna acción de reconciliación como bloqueable. «Comprobar ahora» y «Conservar» conservan el pendiente byte a byte; «Descartar y recargar» lo pierde solo tras una confirmación que lo dice. Nada se descarta ni se normaliza en silencio.
- «Conflicto con diagnóstico»: Cambio declarado del host de T4A1: el Conflicto de «Comprobar ahora» lleva el diagnóstico de admisión, para que un rack borrado, ya no editable o ilegible no se confunda con un cambio externo. Cambia una sola prueba existente, declarada.
- «Las que salgan de la revisión»: Las decisiones de diseño que pida la revisión independiente: las tomo con la opción recomendada y las registro como delegadas. Incluye las menores ya declaradas: Desconocido tras la relectura de POST usa «Descartar y recargar» con confirmación, y «Comprobar ahora» solo en pestaña fijada.

Respuesta: «Lo que recomiendes».

Pregunta 2 (Autonomía): «Cadena de F4-T4A2 esta noche (revisión, emisión, planificación, Worker, CI, revisiones, verificación, controles y custodia): ¿hasta dónde avanzo sin ti?»

Opciones:
- «Hasta el cierre (Recommended)»: Si las revisiones del código dan 0 bloqueantes y 0 importantes, y nc1..nc3 pasan o se satisfacen con sustitutos de firma EXACTA ya autorizados, cierro CLOSED / PASS WITH LIMITATIONS y arrastro los menores. Si el Worker se detiene por un hueco del paquete o la revisión da un importante, corrijo en el intento 2. Nunca gasto el intento 3 sin ti.
- «Hasta VERIFICADA»: Ejecuto la cadena completa, incluidos los controles y la custodia, pero el cierre de T4A2 queda pendiente de tu decisión por la mañana.
- «Hasta el Worker»: Emito el contrato y ejecuto la planificación y el Worker; me detengo antes de la verificación del Controller.

Respuesta: «Hasta el cierre (Recommended)».

Pregunta 3 (Imprevistos): «Si aparece algo que hoy exige volver a ti (un modo de fallo NUEVO de nc1..nc3 sin firma exacta, una segunda falla de fidelidad de la planificación, o un hash nuevo de config.toml escrito por la app de Codex), ¿qué hago?»

Opciones:
- «Parar esa línea y esperar (Recommended)»: Detengo solo esa línea y dejo escrito el análisis con mi propuesta. Mientras tanto sigo con el trabajo independiente que no dependa de ella, como preparar paquetes. Respeta tu regla de no inferir sustitutos.
- «Decide tú y regístralo»: Tomo la opción recomendada y la registro como delegada: un sustituto determinista nuevo para un modo de fallo nuevo, y una snapshot nueva de config.toml solo si tiene las mismas claves que H.
- «Sustitutos sí, config no»: Puedo aplicar un sustituto determinista nuevo, registrado como delegado, pero un hash nuevo de config.toml detiene toda invocación de modelos hasta tu decisión.

Respuesta: «Parar esa línea y esperar (Recommended)».

Pregunta 4 (T1B y T4B): «Cuando F4-T4A2 cierre (PREPARE completo), ¿hasta dónde sigo con F4-T1B (va antes de T4B) y F4-T4B (la primera y única tarea de F4 con escrituras authored en el DWG)?»

Opciones:
- «Solo paquetes (Recommended)»: Preparo los paquetes de T1B y T4B sobre el código real, con revisión independiente, pero no emito ningún contrato hasta que lo autorices.
- «T1B completa; T4B paquete»: Emito y ejecuto la cadena de T1B (sin escrituras authored) y preparo el paquete de T4B con revisión, sin emitirlo.
- «T1B y T4B completas»: Emito y ejecuto también T4B con su cadena completa. Las escrituras se prueban con dobles y guardas; nada toca AutoCAD, y Smoke-2 sigue siendo tuya.

Respuesta: «Solo paquetes (Recommended)».
