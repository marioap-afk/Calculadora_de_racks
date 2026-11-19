# I-64 F4B — Decisiones del Owner antes de los contratos de PB-T2 (Q-PBT2-1 a Q-PBT2-4)

**Fecha:** 2026-11-19. **Canal:** chat de la sesión principal, una ronda de cuatro preguntas de opciones; respuestas literales. **El paquete:** `docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md` v2.1, §8 (se custodiará con la emisión de PB-T2a). Las preguntas llegaron revisadas: revisión independiente de la v1 (NECESITA v2, 0/8/16) y su recomprobación de la v2 (LISTA PARA EMITIR TRAS REMEDICIÓN, 0/0/7, aplicada en la v2.1).

## Q-PBT2-1. «Aplicar» y «Cerrar»

**Texto, literal:** «PB-T2, el editor abierto desde el panel: «Aplicar al borrador» guarda en el borrador sin dibujar. En RACKEDITAR, «Actualizar» guarda y cierra. Avisar al cerrar de lo no aplicado sería el primer aviso sobre todo lo editado; hoy solo avisa de un módulo sin confirmar, y el código evita a propósito ese aviso general (ADR-0029 D8). ¿Qué hacen «Aplicar» y «Cerrar»?»

**Opciones:** «Aplica sin cerrar; Cerrar avisa (Recomendado)»; «Aplica y cierra, como Actualizar»; «Aplica sin cerrar; Cerrar no avisa».

**Respuesta del Owner:** «Aplica sin cerrar; Cerrar avisa (Recomendado)».
- Tras aplicar se puede seguir editando, ver el BOM o guardar en la biblioteca.
- Al cerrar con algo sin aplicar, el editor pregunta: «Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?».
- Lo tecleado sin salir de la caja se pierde sin aviso con Esc, la X o Alt+F4.
- Solo en el modo WORKSPACE; el clásico no cambia. La excepción a ADR-0029 D8 la autoriza esta respuesta.

## Q-PBT2-2. Un diseño con diagnóstico bloqueante

**Texto, literal:** «PB-T2: ¿«Aplicar al borrador» acepta un diseño con un diagnóstico bloqueante, por ejemplo una celda que no cabe? En RACKEDITAR, «Actualizar», el BOM y la biblioteca lo rechazan y muestran el motivo.»

**Opciones:** «Lo rechaza, como Actualizar (Recomendado)»; «Lo acepta».

**Respuesta del Owner:** «Lo rechaza, como Actualizar (Recomendado)». El borrador nunca guarda un diseño que no se puede dibujar: se aplica la misma compuerta (`PushBackEditorOutputGate`) en los dos modos.

## Q-PBT2-3. Una sola validación de campos

**Texto, literal:** «PB-T2: pides una sola validación de campos. La ventana usará la misma en los dos modos, pero el campo de escala del panel valida con su propia copia de esa regla (mismos textos, mayor que 0). ¿La unifico ya o al conectar «Editar todo» (PB-T4)?»

**Opciones:** «En PB-T4, con prueba ya (Recomendado)»; «Ya en PB-T2»; «No hace falta».

**Respuesta del Owner:** «En PB-T4, con prueba ya (Recomendado)».
- PB-T2 añade una prueba de concordancia entre las dos validaciones.
- PB-T4, al rehacer el campo del panel sobre el estado nuevo, deja una sola.

## Q-PBT2-4. La emisión condicional de PB-T2a, PB-T2b y PB-T2c

**Texto, literal:** «PB-T2 se divide en tres: PB-T2a (el puerto al borrador), PB-T2b (la ventana en dos modos) y PB-T2c (las pruebas heredadas en ese modo). ¿Emito cada una sin volver a ti si su revisión da 0 bloqueantes y 0 importantes sin resolver y la remedición sobre PB-T1c no cambia nada material, como con PB-TC2 y PB-T1?»

**Opciones:** «Sí, con esa condición (Recomendado)»; «No, vuelve antes de cada una».

**Respuesta del Owner:** «Sí, con esa condición (Recomendado)».
- La sesión emite cada subtarea si su comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, y la remedición sobre el cierre de PB-T1c no cambia nada material.
- Vuelve al Owner por:
  - una decisión nueva;
  - un control con una firma no prevista;
  - un intento 3;
  - un efecto material sin medir.
