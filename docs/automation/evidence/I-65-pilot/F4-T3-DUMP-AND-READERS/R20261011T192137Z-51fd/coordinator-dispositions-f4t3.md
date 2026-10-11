# I-65 F4-T3-DUMP-AND-READERS — decisión del Coordinator previa a la verificación (intento 2, WorkRunId R20261011T183618Z-05a9)

Texto literal entregado al Controller como entrada de la verificación, junto con `analysis-f4t3.md` (causa raíz). Se registrará en
`docs/automation/decisions/I-65.md` con la custodia, después de la verificación y de los controles, para no mover `HEAD` de `CurrentSha`.

- **CD-I65-F4-14 (resolución del STOP del Worker, §16.11).** El Worker declaró `BLOCKED/STOP` por C-08 tras UNA invocación
  `python --version` con la salida descartada, posterior a sus dos commits y a su push (análisis adjunto). La sesión aplicó la parada y la
  devolvió al Coordinator; el Coordinator, con la causa raíz en `analysis-f4t3.md`, decide que la resolución **no cambia el trabajo**: la
  entrega `76db7642d1bb9494c7f70155d3d3cabe45152bb3` se somete tal cual a la verificación de las 14 comprobaciones de §16.9; no hay
  delegación de corrección y `attempts` sigue en 2 (§16.8 y §16.11). La decisión no es OWNER-RESERVED.
- **DEV-I65-F4-06 (desviación de proceso registrada).** Cuarta invocación de un intérprete Python por un Worker en la unidad, esta vez
  contra una parada expresa del contrato. No afecta al producto, a las pruebas ni a la evidencia: comprueba en la transcripción, en el diff
  y en el registro del trabajo que solo hubo esa invocación, que fue posterior al commit GREEN y que el árbol quedó limpio en `76db7642`.
- `WorkerStatus` BLOCKED y `Disposition` STOP de la entrega reflejan esa parada, ya resuelta por esta decisión; no son, por sí mismos,
  una comprobación de §16.9. Evalúa las 14 comprobaciones sobre la entrega y los hechos del relevo.
- Nota de método declarada por el Worker (no es desviación): antes del commit RED probó la implementación en un directorio ignorado
  (`work/green`) para asegurar que las pruebas del RED eran correctas; el commit RED contiene solo el esqueleto y ninguna prueba del RED
  cambió después (comprobable con `git diff --name-only a283e0cc 76db7642 -- tests/`).
