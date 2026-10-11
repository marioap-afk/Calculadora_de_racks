# I-65 F4-T2-SOURCE-ENTRY — disposición del Coordinator previa a la verificación (intento 2, WorkRunId R20261011T173811Z-0167)

Texto literal entregado al Controller como entrada de la verificación. Se registrará en `docs/automation/decisions/I-65.md` con la custodia,
después de la verificación y de los controles, para no mover `HEAD` de `CurrentSha`.

- **DEV-I65-F4-05 (desviación de proceso, no bloqueante).** El Worker declaró en `Deviations`, y la transcripción lo confirma, UNA invocación
  `python3 --version` con la salida descartada, incluida por error en una orden `grep`. El prompt del Worker prohibía invocar un intérprete
  Python por cualquier motivo; el C-08 del contrato de esta tarea cubre la necesidad de Python para compilar o probar, que no existe. No se
  ejecutó la plantilla, el programa de análisis ni ningún script; nada del producto ni de las pruebas depende de Python. El Coordinator la
  clasifica como desviación de proceso **no bloqueante** (C-08 no se activa) y, por ser la tercera vez en la iniciativa (CD-I65-F3-12,
  DEV-I65-F4-01), endurece los contratos siguientes: su C-08 incluirá la ejecución de un intérprete Python por cualquier motivo.
- El archivo temporal `TempProbeScratch.cs` que el Worker creó y borró antes del RED no está en ningún commit (comprobable con `git log`).
