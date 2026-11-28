# PB-T4b: la verificación `R20261121T132429Z-db16` agotó el tiempo (decisión de la sesión)

**Fecha:** 2026-11-21.

**El hecho:**
- La verificación del Controller `R20261121T132429Z-db16` (prompt de 28 963 bytes, con la disposición de CT-20 como entrada) se lanzó a las 13:25:13Z.
- El lanzador la cortó a las 13:35:14Z con el código 124: es el `timeout --kill-after=10 600` de `launch_pbt4b_verify.sh`, el mismo de toda la cadena de F4B.
- Hasta entonces el Controller estuvo trabajando: 50 órdenes completadas, la última la lista de los filtros de la UI del GREEN.
- No hay `output.json` ni `turn.completed`.
- Los procesos de entrada: 0 participantes y 0 no atribuibles.
- El árbol, limpio, en `c065080d`.

**Por qué ahora y no antes:** la entrega de PB-T4b es mucho mayor que las anteriores. Tiene 77 filtros de `RequiredTests` y TRX de Core de unos 28 MB por rama (tres ramas, con la reparación del RED). En PB-T4a la verificación cabía en esos 600 s.

**La decisión:** la sesión la toma, como hizo con `611f` en PB-T4a.
- **`db16` es un lanzamiento no productivo:**
  - el modelo se invocó, pero no llegó a dar resultado;
  - se custodia como unidad propia, con su nota;
  - no cuenta como verificación ni como parada;
  - no gasta intento.
- **El tope de tiempo** de `launch_pbt4b_verify.sh` y de `launch_pbt4b_nc.sh` pasa de 600 a 1800 s.
  - No cambia nada más: ni el prompt, ni el transporte, ni el modelo, ni el esfuerzo.
  - El contrato no fija ese tope.
- **La verificación se relanza** con un RunId nuevo, el mismo prompt (regenerado) y la misma disposición.
