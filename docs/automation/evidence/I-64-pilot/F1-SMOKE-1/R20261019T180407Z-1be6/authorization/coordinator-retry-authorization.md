# I-64 — Autorización vigente del reintento de Smoke-1 (literal)

Recibida por el chat de la sesión responsable el 2026-10-18/19, mientras la sesión esperaba en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED.
El RunId `R20261019T180407Z-1be6` lo asignó la sesión al cumplirse las condiciones: `acad.exe` = 0 y refresco de I-52 posterior a I-65.

Nota de la sesión: el texto dice que la autorización se dio «en el turno anterior». En el chat de la sesión de I-64 no consta ningún texto
anterior de autorización del reintento. Por eso este mensaje se registra como el texto literal de la autorización vigente.

````text
La autorización del reintento ya fue concedida en el turno anterior para el Candidate:

```
09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8
```

La rama sigue exactamente en ese SHA; no hubo movimiento de I-64.
Lo que sí cambió desde entonces es el estado temporal del host, porque I-65 abrió y cerró una ventana después. Por eso no debes usar referencias de host anteriores a I-65 para la nueva BASELINE.
Disposición actual

```
COORDINATOR_SMOKE1_RETRY_AUTHORIZATION = STILL VALID

Candidate = 09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8
PaletteKeyAcceptance = ACCEPTED
One-time non-persistent NETLOAD = AUTHORIZED
SECURELOAD rule = fresh read-only value immediately before BASELINE
```

Pero la ejecución queda condicionada a un refresh final de host posterior a I-65:

1. `acad.exe = 0`.
2. Pedir a I-52 referencias frescas después del cierre de I-65.
3. Confirmar:
   * perfil activo;
   * `acad.exe` SHA-256;
   * TRUSTEDPATHS 19 entradas + SHA completo;
   * SECURELOAD actual o la regla de lectura inmediata por Owner/CAD manager;
   * no-drift relevante de registro/ApplicationPlugins/ACL.
4. Confirmar que I-52 mantiene su propia ventana/H10 retenida durante la ocurrencia.
5. Entonces asignar RunId nuevo y tomar una BASELINE nueva.

Los cambios reportados por I-65 —historial/carpeta inicial de NETLOAD y posición de la ventana— no son un problema por sí mismos porque ocurrieron antes de nuestra nueva BASELINE. Esa BASELINE los absorberá como estado inicial.
Así que no necesitas otra decisión mía sobre `PaletteKeyAcceptance`, la carga única ni SECURELOAD. Solo falta el acuse fresco de I-52 posterior a I-65; con eso puedes proceder sin volver a pedir autorización adicional.
````

## Condiciones cumplidas antes de asignar el RunId

| Condición | Evidencia |
|---|---|
| 1. `acad.exe` = 0 | 2026-10-19T18:03:15Z (sesión); I-52 a las 18:03:39Z |
| 2. Referencias frescas de I-52 posteriores a I-65 | W3 de I-65 cerró a las 18:01:09Z; refresco de I-52 a las 18:03:39Z (`i52-fresh-refs-post-i65.txt`) |
| 3. Perfil, `acad.exe`, TRUSTEDPATHS, SECURELOAD, ausencia de deriva | `<<Unnamed Profile>>`; `2a75996f…6422`; 19 entradas, `a9ba0fd1…2498`; SECURELOAD 1, solo referencia (regla: lectura inmediata del Owner); sin deriva en registro, ApplicationPlugins ni ACL |
| 4. I-52 retiene su ventana y P2 H10 | confirmado por I-52 desde nuestra BASELINE hasta nuestro POST y su recomprobación |
| 5. RunId nuevo | `R20261019T180407Z-1be6` |
