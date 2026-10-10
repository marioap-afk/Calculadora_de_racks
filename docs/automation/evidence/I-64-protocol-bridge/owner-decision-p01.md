# I-64 — Decisión del Owner: P-01 del puente de protocolo de emergencia

Transcripción literal de la decisión del Owner recibida por el chat de la sesión responsable el 2026-10-10, tras el STOP P-01 de la
fase A (evidencia §33).

```text
OWNER DECISION — I-64 EMERGENCY PROTOCOL BRIDGE / P-01
Acepto como nueva línea base observada de Codex para esta continuación de I-64:
config.toml SHA-256:
73890CA3…
Key-name count:
105
Cambio estructural observado respecto de la línea base anterior:
nueva sección [projects.<redactado>] con la clave trust_level.
Esta aceptación:

* resuelve P-01 para esta línea base;
* NO autoriza modificar config.toml;
* NO autoriza leer ni registrar valores sensibles;
* NO implica que cualquier cambio futuro de config.toml quede aceptado;
* cualquier cambio posterior de hash vuelve a activar P-01.

Sobre codex-cli 0.162.0-alpha.2:
NO lo declaro automáticamente equivalente a la medición anterior.
El Coordinator queda autorizado a ejecutar UNA sonda read-only mínima para
re-medir la celda gpt-6-luna/high bajo el runtime actual.
La sonda:

* no escribe Git;
* no cambia config.toml;
* no toca producto;
* no consume presupuesto de F1-T1-MODEL;
* no activa A-1;
* no ejecuta el ScopeBridge contra T1.

Si la sonda pasa, puede lanzarse la revisión independiente ya preparada de
A-1.
Si la sonda falla o aparece P-01/P-06/P-02:
STOP y regreso al Owner/Coordinator según corresponda.
```
