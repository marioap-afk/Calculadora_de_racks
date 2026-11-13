Decisión del Owner sobre nc2 y nc3 de F4-T3B2 (2026-11-13), por la herramienta de preguntas de la sesión. Pregunta y respuesta literales:

Pregunta: «F4-T3B2: verificación EXECUTION_VERIFIED 14/14 y nc1 CONTROL PASS, pero nc2 y nc3 NO pasan. Causa común, error MÍO: el prompt de verificación (que los controles copian) seguía diciendo que la BaseSha es «hijo directo» de la AuthorityRevision con «tres rutas» de diff; tras la custodia del STOP son 34, todas docs de la unidad. El Controller dio Authority como primera en fail en nc2 y nc3 (en la verificación real la resolvió bien). Además, en nc2 dio Scope en PASS: comparó con las 13 rutas del contrato, no con las 12 de la delegación mutada. nc3 sí detectó FreeText. ¿Cómo sigo?»

Opciones ofrecidas:
- «Corregir y repetir (Recommended)»: Corrijo ese hecho del prompt y repito la verificación y nc1..nc3 desde cero (sin gastar intento de producto; las corridas actuales se custodian tal cual). Si nc2 vuelve a dar Scope en pass, vuelvo a ti con un sustituto.
- «Sustitutos sobre lo actual»: nc3 con el detector FreeText (la señal se detectó; Authority falló por mi error) y nc2 con un sustituto nuevo de señal de Scope determinista (el Controller no detectó la ruta quitada). Deudas nuevas.
- «Detener aquí»: Custodio el intento 1 verificado con los controles como están y paro hasta tu revisión.

Respuesta: «Corregir y repetir (Recommended)».
