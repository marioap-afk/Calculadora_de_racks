# I-64 F4B — Orden del Owner de emitir PB-TC (F4B-E07)

**Fecha:** 2026-11-17 (noche). **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación.

**Contexto:**
- **F4B-E05, «Siguiente», punto 4:** «PB-TC no se emite sin la orden del Owner».
- **Decisiones del Owner:** ya tomadas (F4B-E06, `owner-pbtc-decisions.md`).
- **Revisiones del paquete:**
  - IRT1, NO EMITIBLE 1/4/10;
  - ICT2, EMITIBLE TRAS CORRECCIONES 0/1/9;
  - ICT21, EMITIBLE 0/0/5. Sus cinco menores se aplican en el texto de emisión v2.2, sin otra comprobación, como recomienda ICT21.

La pregunta y la respuesta se transcriben **literales**.

## La pregunta

> El plan de PB-TC pasó sus revisiones y la última lo dio por emitible, sin fallos bloqueantes ni importantes. Recoge tus decisiones: decimales exactos con la regla de Cantilever, estilo de cotas conservado, aviso corto en la franja de estado y «Restaurar estándar» incluido. Emitir significa: guardar el plan y el contrato en la rama de trabajo (solo documentos; main no se toca) y lanzar la cadena automática. En la cadena, primero se escriben pruebas que fallan con el programa actual, luego la corrección solo en la ventana de Push Back, y después vienen la CI, las revisiones, la verificación y los controles. No usa AutoCAD. Tiene hasta 3 intentos, y el tercero siempre pasa antes por ti. Antes de corregir, la cadena mide qué hace hoy RACKEDITAR con los módulos de un rack cuyo fondo trae decimales; si cambian piezas, niveles o el BOM, se para y vuelve a ti. ¿Emito PB-TC?

## Las opciones

- **«Sí, emite PB-TC (Recomendado)»:** Custodio el plan, tus decisiones y el registro de pruebas sustituidas, emito el contrato con el intento 1 de 3 y arranco la cadena. Te aviso al cerrarla o si se para en un punto que vuelve a ti.
- **«Todavía no»:** Dejo todo preparado sin escribir nada en la rama y espero tu orden.

## La respuesta del Owner

«Sí, emite PB-TC (Recomendado)».

## Alcance

- La orden autoriza:
  - la custodia del paquete v2.2, del registro de mediciones, de las revisiones, de las decisiones F4B-E06, de esta orden y del registro declarado `superseding-characterization.md`;
  - la emisión del contrato de `F4B-PBTC-DECLARED-CORRECTIONS` con attempts 1 de 3;
  - la cadena ordinaria: planificación, A1-A8, nc4, Worker con RED y GREEN, CI, revisiones, verificación y nc1 a nc3.
- Quedan como vueltas al Owner:
  - PBTC-C14, si el delta de la base de CT-PBTC-10 es material;
  - PBTC-C12, el intento 3;
  - las paradas que vuelven al Owner según el contrato.
- No se toca `main`, no hay push forzado y no se modifica evidencia ya custodiada.
