
## D-09. Cierre de F2-T2-HOST-C1 y disposición de Smoke-NAV

- **Problema:** cerrar la corrección y disponer el hito, que correspondían al Coordinator.
- **F2-T2-HOST-C1 = CLOSED / PASS** sobre `003be859`. RED `ce48067e`, `85affa68` y `8050f512`; GREEN `cdef2d44` y `003be859`; CI 42777768225 4/4; Core 16320/16320; UI 1792 más 17 omitidas. Una revisión independiente del diff no encontró defectos bloqueantes, y sus dos observaciones se corrigieron antes del candidato.
- **Smoke-NAV = PASS sobre `003be859`** (ocurrencia `R20261110T073451Z-e221`, operada por la sesión): SN-01..SN-07 y SN-10 en PASS, SN-09 medido, SN-06c y la vía de pestañas de SN-10 `NOT_APPLICABLE_UNTIL_F3`.
- **SN-08 = LIMITADA, no cuenta como PASS:** la ejecución síncrona no deja intervalo en el que una petición quede pendiente, así que la carrera no se puede provocar; no se observó efecto cruzado entre documentos. La comprobación de sesión sigue en el host y la cubren las pruebas de T2A.
- **F2 = PASS** con esa limitación y con la desviación de D-08 pendiente de ratificación del Owner. No es Owner Validation.
- **Fundamento:** evidencia real en AutoCAD con el candidato exacto, guarda de procesos sin STOP y perfil `CLEAN`.
- **Riesgo:** el piloto usó dibujos sintéticos con la geometría controlada; la Owner Validation debe repetirlo con racks reales.
- **Alcance:** F2. No integra en `main`.
