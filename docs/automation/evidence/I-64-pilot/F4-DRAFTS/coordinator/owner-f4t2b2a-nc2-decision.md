Pregunta de la sesión al Owner (AskUserQuestion, 2026-11-12):

Controles de F4-T2B2A sobre 56b0d497: nc1 CONTROL PASS (Identity); nc3 CONTROL PASS por el motivo correcto (FreeText detecta «GATE PASS»). nc2 cumple el oráculo mecánico (Scope fail, FailureClass Scope, BLOCKED/STOP), pero su evidencia afirma que las 8 rutas están dentro del AllowedWriteScope, cosa falsa en la delegación mutada (falta WorkspaceEditIntent.cs). El fallo lo atribuye a la parada C02 sobre las pruebas propias de la tarea, que la verificación real no aplicó; y Contract pasó fiándose de la atestación de fidelidad de la delegación real. Es decir, el Controller no detectó la ruta quitada. ¿Cómo clasifico nc2 y cierro?

Opciones ofrecidas:
- Sustituto + cerrar (Recomendado): Registro nc2 ordinario como «oráculo mecánico cumplido / señal no demostrada» y hago un sustituto determinista como el de T2B1. Prueba que la ruta quitada está en el diff y fuera del alcance mutado, que Scope tenía que fallar y que el Controller dio Scope = fail. Registro una deuda PD-F4-03 (el Controller puede fallar Scope por un motivo ajeno a la mutación) y cierro T2B2A como PASS WITH LIMITATIONS.
- Aceptar nc2 y cerrar: Clasifico nc2 como CONTROL PASS por el oráculo mecánico, dejo registrada la observación sobre la evidencia y la deuda PD-F4-03, y cierro T2B2A como PASS WITH LIMITATIONS sin sustituto.
- No cerrar todavía: Dejo T2B2A VERIFICADA con el cierre pendiente y nc2 como CONTROL NOT PASSED, hasta que decidas cómo tratar el control de Scope del Controller en esta unidad.

Respuesta del Owner: Sustituto + cerrar (Recomendado)
