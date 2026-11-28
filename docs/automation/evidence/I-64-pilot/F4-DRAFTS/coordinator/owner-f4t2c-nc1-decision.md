Pregunta de la sesión al Owner (AskUserQuestion, 2026-11-12), tras los controles de F4-T2C:

Controles de F4-T2C sobre 33e43f9f (verificación 7f67 14/14). nc2 y nc3 dan CONTROL PASS por el motivo correcto: nc2 nombra la ruta quitada (EventBridgeCore.cs) y nc3 detecta el término en WorkCompleted. nc1 REPITE el modo de fallo de T2B2B (PD-F4-04). El Controller detectó la mutación y dio BLOCKED/STOP con S-04: la evidencia de Identity nombra el SHA falso deadbeef… frente al HEAD real 33e43f9f. Pero la primera comprobación en fail vuelve a ser Handoff, no Identity, y fallan en cascada Remote y Ci. Tu orden dice no reutilizar automáticamente un sustituto, y el de T2B2B (F4T2B2B-NC1-IDENTITY-SIGNAL-01) era solo para esa tarea. Tengo preparada, sin ejecutar, una copia idéntica para T2C (F4T2C-NC1-IDENTITY-SIGNAL-01): las mismas 7 propiedades y la misma autoprueba de 12 casos, con solo el identificador y el ámbito cambiados. ¿Cómo trato nc1?

Opciones ofrecidas:
- Sustituto T2C + cerrar (Recomendado): Ejecuto F4T2C-NC1-IDENTITY-SIGNAL-01 sobre las entradas de esta tarea. Si pasa, registro nc1 como satisfecho por sustituto y la recurrencia en PD-F4-04, y cierro T2C como PASS WITH LIMITATIONS, con los menores ya decididos en T3. F4-T2 quedaría completa por subtareas. Si el sustituto falla, vuelvo a ti.
- Fijar regla para F4: Lo mismo que la opción anterior, y además registro una regla de F4: mientras siga PD-F4-04, un nc1 con Handoff como primera comprobación, Identity en fail con evidencia correcta y BLOCKED/STOP se resuelve con el sustituto de identidad de la tarea, sin volver a ti cada vez. Si cambia algo más, sí vuelvo.
- No cerrar todavía: Dejo T2C VERIFICADA con el cierre pendiente y nc1 como CONTROL NOT PASSED, hasta que decidas cómo resolver PD-F4-04 en el protocolo.

Respuesta del Owner: Sustituto T2C + cerrar (Recomendado)
