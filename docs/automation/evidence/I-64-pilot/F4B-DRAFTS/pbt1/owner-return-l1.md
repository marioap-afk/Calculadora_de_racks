# I-64 F4B PB-T1 — Vuelta al Owner por L-1 («no aliasing»), preparada para la misma vuelta que C14 de PB-TC2

**Fecha:** 2026-11-18, madrugada. La preparó la sesión principal en modo autónomo y la corrigió según N-I-01 de la comprobación acotada de la v2 (`independent-review-v2-check.md`, §3.2), para que sea fiel y no tenga sesgo.

## Qué pasó

- **El paquete de PB-T1** se divide en tres subtareas: PB-T1a, la red de pruebas; PB-T1b, el estado en Application; y PB-T1c, la ventana sobre el estado.
- **Las revisiones:** la independiente de la v1 dio 0/8/15, y la comprobación acotada de la v2 dio 0/1/10. Ese importante abierto era esta misma pregunta, ya corregida aquí.
- **Una sola decisión es del Owner: L-1.** Sin su respuesta, PB-T1 no se emite por la autorización condicional (F4B-E09, pregunta 4).

## L-1, en lenguaje llano

**El requisito.** Uno de los requisitos duros del Owner para PB-T1 es «no aliasing»: la copia del estado de un rack no puede compartir objetos con el original, para que cambiar uno no cambie el otro.

**Las dos piezas que hoy se comparten.** Son el rack ya resuelto (la «línea base») y el informe de reconciliación de los módulos.
- **La línea base es calculada,** pero guarda las personalizaciones de módulos ya confirmadas entre un recálculo y otro.
- **Hoy el producto ya la comparte por referencia** entre la ventana, la sesión y el estado.
- **Hoy solo se le cambia el nombre al dibujar,** cuando la ventana ya se cerró. Lo hace el Plugin al dibujar o actualizar, y el lote del menú.

**Las opciones:**
- **(a) Aceptar la excepción, declarada y limitada a esas dos piezas.**
  - Todo lo demás de la copia sí queda separado: la cola aparcada, la edición de módulos y la sesión, con una prueba que lo comprueba aparte.
  - Sin cambio de comportamiento.
  - **El riesgo:** un cambio futuro en ese objeto afectaría a las dos copias. Las tareas siguientes quedan obligadas a no hacerlo (arrastre A-2), y las pruebas vigilan a los consumidores de hoy.
- **(b) Una tarea previa de Domain** que haga copias profundas del rack resuelto. Es cara: toca las configuraciones de cabecera, cuyas anulaciones manuales solo viven en memoria y hay que copiar a mano.
- **(c) Volver a calcular la línea base en la copia** (rehacer el rack resuelto a partir del diseño), con una prueba de equivalencia.
  - Cumple la letra sin tocar Domain, pero la copia necesita el catálogo.
  - Si la prueba con racks A/B falla, se vuelve al Owner.
  - El anexo L1-C del paquete ya trae el cambio.

## La pregunta propuesta

> Para la tarea que saca el estado de la ventana (PB-T1): tu requisito «no aliasing» pide que la copia del estado no comparta objetos con el original. Hay dos piezas que hoy el producto comparte: el rack ya resuelto (calculado, pero guarda las personalizaciones de módulos confirmadas) y el informe de módulos. Hoy solo se les cambia el nombre al dibujar, con la ventana ya cerrada. ¿Qué hago?

**Opciones:**
- **«Excepción declarada (Recomendado)»:** se mantienen compartidas esas dos piezas; todo lo demás va separado y probado. El riesgo: un cambio futuro en ese objeto afectaría a las dos copias, y las tareas siguientes quedan obligadas a no hacerlo.
- **«Recalcularlas en la copia»:** la copia rehace el rack resuelto con el catálogo, con una prueba de equivalencia. Si la prueba con racks A/B falla, vuelvo a ti.
- **«Tarea previa de Domain»:** copias profundas del rack resuelto. Es cara: toca las anulaciones manuales de cabecera, que solo viven en memoria.
