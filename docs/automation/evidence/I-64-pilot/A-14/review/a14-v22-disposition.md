# Disposición de la comprobación acotada de la v2.1 de I-64 A-14

**Comprobación:** `independent-review-a14-v21-check.md` (SHA-256 `be9d965987ebd2e26b49cf74f08d97de25b048578c4e63a18dce86f4c9996913`), sobre la
v2.1 (SHA-256 `e4758ae9132dd89d62239307ef8e45da81352faac339ce35858dcda362a25925`). Veredicto: EMITIBLE TRAS CORRECCIONES. De los 20 hallazgos de
la v2, 19 quedaron resueltos y 1 parcial (N-01). Hallazgos nuevos: 0 bloqueantes, 1 importante y 5 menores.

**Objeto corregido:** `I-64-A-14-draft-v2.2.md`. **Recuento:** 7 filas (el residuo de N-01, N2-01 y N2-02 a N2-06). **Aplicados: 7.
Rechazados: 0.** Además se aplican los cuatro ajustes de la firma (§5 de la comprobación) y los añadidos que pide la orden del Coordinator.

| Id | Severidad | Disposición | Dónde (v2.2) | Motivo |
|---|---|---|---|---|
| N-01 (residuo) | Parcial → resuelto | Aplicado con N2-01 | Cabecera | — |
| N2-01 | Importante | Aplicado | Cabecera («Corrects / supersedes»); C-12 (unidades; autoridad); C-13; §8; §11 (P-1); §12 (filas 3 y 3b; pendientes) | Solo se amplía el alcance de A-7, que es lo que pide C-13 (INV-24, S2-16..S2-22). A-1..A-6 y A-8..A-13 siguen siendo de la unidad I-64: el Owner los autorizó «only for this I-64 unit-scoped exception» (A-2, A-3, A-4 `:33`; A-5 `:35`; A-6 `:33`), y A-1 `:73` y A-5 `:208` los atan a I-64 hasta que los sustituya una autoridad de protocolo integrada. La cabecera deja de decir «Ninguna A-n anterior» y nombra el cambio exacto: el `Applies-to` de A-7, solo su alcance. **No** se amplía ningún puente: la posible ampliación de A-4 (Scope) o A-6 (FreeText) queda como pregunta P-1 de la firma, con valor por defecto «no» y, si el Owner la quiere, la A-n siguiente con el dueño del protocolo |
| N2-02 | Menor | Aplicado | C-12 (Smoke-PERF; autoridad); §12 (fila 16; pendientes); §11 (materia 11; punto 12) | Una sola redacción en todos los sitios: «el Owner, a propuesta del Coordinator». «Sobre lo que esté integrado» pasa a «sobre el Candidato del tramo» (§12.5 `:902`: «Sobre un Candidato completo»). En lenguaje llano: «la prueba de rendimiento completa, con tus dibujos reales», una sesión del Owner por tramo, cinco con la propuesta |
| N2-03 | Menor | Aplicado | C-12 (tabla de pros y contras); §6 (fase 4); §11 (punto 12) | Se dan los pros y contras de las dos opciones. La propuesta 0 a 4 retrasa la primera integración, que espera a la fase de más riesgo (posible STOP frente a la costura de I-55), y envejece más la rama (R-11, contra Q-09). En §6, la fase 4 «es la última fase del tramo 1, que cierran su READY y su Owner Validation» |
| N2-04 | Menor | Aplicado | C-12; §11 (materia 11) | En los dos sitios: «antes del READY-01 del primer tramo afectado». Si el afectado es el tramo 1, que ya está abierto, sus fases siguen y lo que espera a la A-n es su READY-01 |
| N2-05 | Menor | Aplicado | §10 (pasos 1 y 3); §13 | «Todas las revisiones y comprobaciones independientes de A-14 hechas hasta ese commit … y sus tablas de disposición», con LIFECYCLE §5 `:162-163` |
| N2-06 | Menor | Aplicado | §5, tramo 2 | «Si no, el tramo y los OV de Variables los fija su A-n (C-11)» |

## La firma (§5 de la comprobación y orden del Coordinator)

| Punto | Disposición | Dónde |
|---|---|---|
| «Un solo Deshacer» y el progreso | Aplicado | Lenguaje llano, punto 5 |
| «En qué lado» | Aplicado | Materia (9) y punto 10 |
| Marcar los puntos 9 y 10 como lecturas que confirma el Owner | Aplicado: «(Confirmas una lectura nuestra.)» en los dos | Puntos 9 y 10 |
| Nombrar la ruta de la biblioteca de bloques desde la fase 1 | Aplicado | Materia (9) y punto 10 |
| Smoke-PERF completo y su carga para el Owner; «si cambias los tramos, hace falta una enmienda más» | Aplicado | Materia (11) y punto 12 |
| La extensión de puentes, si se quisiera | Fuera de la frase, como pregunta P-1, con valor por defecto «no» y una línea en lenguaje llano | §11 |
