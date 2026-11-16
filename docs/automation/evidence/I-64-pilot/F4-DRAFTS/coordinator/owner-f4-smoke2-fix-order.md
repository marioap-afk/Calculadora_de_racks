# I-64 F4 — Orden del Owner: corregir los errores de Smoke-2 antes de seguir (texto literal)

Mensajes del Owner en el chat de la sesión principal, durante Smoke-2 (2026-11-16), tras el hallazgo F4-S2-03 (las vistas no se refrescan tras `U` hasta `REGEN`):

> Voy a cerrar autocad completamente, arregla todos los errores que encontramos y después continuamos con el SMOKE, arregla todos los errores posibles ya que no estaré en mi casa

> Listo, ya cerré AutoCAD, empieza

## Lectura de la sesión (declarada)

**Sustituye** el orden de F4-E73, que era «Smoke-2 primero y la corrección después». Ahora se corrige antes de seguir con Smoke-2, que queda **suspendido** antes del paso 16 repetido.

**Alcance:** los errores encontrados en Smoke-2 hasta ahora.
- F4-E72: el nombre del rack en el «Actualizar» del piloto.
- F4-S2-01: las lecturas del panel marcan su propio borrador como «posiblemente obsoleto». Con un borrador limpio, la caja solo deja editar menos de 1 s.
- F4-S2-02: borrar no inicia la edición; probablemente es consecuencia de S2-01.
- F4-S2-03: tras `U`, las vistas no se refrescan hasta `REGEN`.
- La observación de rendimiento: la regeneración tras «Actualizar» tarda bastante.

RACKPROYECTAR queda fuera, en su propia iniciativa, por la decisión de F4-E73.

**Cómo:**
- **Diagnóstico.** La sesión opera AutoCAD solo para diagnosticar, con una build de diagnóstico de un worktree desechable y sobre copias, como en H0. No ejecuta ningún paso de Smoke-2. Smoke-2 lo sigue operando el Owner presente: sigue en vigor la regla de la decisión 4.
- **Corrección.** Es una tarea contratada de F4, con su paquete revisado de forma independiente, RED y GREEN, CI exacta, las dos revisiones, la verificación y los controles.
- **Reglas que se mantienen:** las del Owner que ya rigen en F4. Sin main, sin force-push y sin tocar la evidencia custodiada. Las decisiones materiales que no cubran esas reglas se anotan para el Owner, sin parar el resto.
