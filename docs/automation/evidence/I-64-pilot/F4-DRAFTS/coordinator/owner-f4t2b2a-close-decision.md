Pregunta de la sesión al Owner (AskUserQuestion, 2026-11-12):

F4-T2B2A, intento 2 (corrección): verificado 14/14 sobre 56b0d497, CI 4/4, y el producto correcto según las dos revisiones. Pero la re-revisión independiente da NO ACEPTABLE por N-1 (IMPORTANTE, solo pruebas): la prueba de WorkspaceEditIntentHostTests.cs:1018 exige que EditIntentStatus tenga exactamente 7 estados, y T2B2B tiene que añadir NotFound, NotNeeded y Reloaded para la recarga (D-T2B2-12); ese archivo no está en el alcance de T2B2B. Hay además 4 menores (N-2: el finally Debug puede lanzar con un documento nulo; N-3 y N-5: guardas; N-4: un comentario). Queda 1 intento (3 de 3). ¿Cómo sigo?

Opciones ofrecidas:
- Cerrar y N-1 a T2B2B (Recomendado): Corro nc1..nc3 sobre 56b0d497 y, si pasan, cierro T2B2A como CLOSED / PASS WITH LIMITATIONS sin gastar el intento 3. El contrato de T2B2B declara el cambio de esa prueba: el enum pasa a fijarse como prefijo ordenado, que es una enmienda del alcance de T2B2B (§3 y §6 del paquete). Los menores se arrastran a T2B2B y T3.
- Intento 3: N-1 y menores: Corrección con el último intento: N-1, N-3 y N-5 en pruebas, y N-2 y N-4 en producto. Si la verificación o la nueva revisión fallaran otra vez, el presupuesto se agota (S-11) y F4-T2 vuelve a ti.
- Intento 3: solo N-1: Corrección mínima con el último intento: solo cambia la aserción del enum (prefijo ordenado). Los menores se arrastran a T2B2B y T3. Hay menos superficie para hallazgos nuevos, pero sigue siendo el último intento.

Respuesta del Owner: Cerrar y N-1 a T2B2B (Recomendado)
