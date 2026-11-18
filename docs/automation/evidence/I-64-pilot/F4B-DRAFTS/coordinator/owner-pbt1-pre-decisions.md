# I-64 F4B — Decisiones del Owner antes del contrato de PB-T1 (OB-06.5) y autorización condicional de emisión (F4B-E09)

**Fecha:** 2026-11-18. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación.

**Contexto:**
- **La vuelta:** antes del contrato de PB-T1, OB-06.5 manda volver al Owner, en una sola vuelta, con los casos alcanzables de pérdida (H-31, OWNER-1, c-02, `pb-21-cola` y T-2).
- **La base:** el expediente del analista independiente, que se custodia como `F4B-DRAFTS/pbt1/ob065-dossier.md`.
- **Lo que comprobó la sesión antes de preguntar:** los inventarios crudos de los 22 sobres reales del bloque M, sin AutoCAD. Los únicos módulos `GAP` y `B:` son de un rack A/B real (`SideB.IsPresent` verdadero), así que ningún rack real es de un sentido con cola guardada.
- **Antes del mensaje:** el Owner había escrito: «Avanza todo lo que puedas, me refiero a que no contestaré lo que no sea necesario, no que no trabajes».

Las preguntas y las respuestas se transcriben **literales**.

## Pregunta 1 (H-31 y OWNER-1)

**Pregunta:**

> Cajas que todavía redondean. Hoy el hueco entre lados, la longitud de un módulo y cuatro parámetros avanzados se guardan redondeados a 3 decimales solo con pasar por la caja. Además, pasar por la longitud de un módulo cuyo largo trae más decimales lo convierte de «calculado» en «manual», y ese módulo ya no sigue los cambios del fondo. A eso se suma el caso «Ambos lados» con la caja vacía, que dejaste anotado. La tarea siguiente (PB-T1, sacar el estado fuera de la ventana) no puede imitar ese redondeo sin fingirlo. Tu regla dice que ninguna caja manda. ¿Qué hago?

**Opciones:**
- **«Corregir en PB-TC2 (Recomendado)»:** Una tarea corta y propia antes de PB-T1, con la misma regla de PB-TC y solo en la ventana Push Back. Cubre las seis cajas y «Ambos lados»: unas 5 pruebas nuevas y ninguna sustituida. Con 3 decimales o menos no cambia nada; con más, menos de 0,0005" y el BOM en diezmilésimas.
- **«Dejarlas como están»:** PB-T1 deja esas seis cajas como hoy, como excepción a tu regla de que ninguna caja manda. «Ambos lados» también se queda como limitación.

**Respuesta del Owner:** «Corregir en PB-TC2 (Recomendado)».

## Pregunta 2 (c-02)

**Pregunta:**

> Poste que el catálogo no tiene. Si un rack guardado trae un poste que no existe en el catálogo (solo pasa con un archivo editado o con otro catálogo), RACKEDITAR lo cambia sin avisar por el poste por defecto. El panel del Workspace lo conserva, pero entonces la retícula de troqueles, los topes y el BOM salen incompletos. ¿Qué regla quieres?

**Opciones:**
- **«Como hoy, como regla explícita (Recomendado)»:** RACKEDITAR no cambia: un poste que no está en el catálogo pasa al de por defecto, igual que ya se hace con las vigas. Queda escrito como regla del estado, y el Workspace la heredará. Cuesta una prueba dentro de PB-T1.
- **«Avisar y pedir otro poste»:** Al abrir un rack así, aparece un aviso y no se puede dibujar hasta elegir un poste del catálogo. Es un cambio visible; iría en PB-TC2.
- **«Conservar el poste inexistente»:** Cambia el dibujo y el BOM (troquel, topes, descripción del poste). Necesita tarea propia y otra vuelta contigo con el efecto medido. No recomendado.

**Respuesta del Owner:** «Como hoy, como regla explícita (Recomendado)».

## Pregunta 3 (`pb-21-cola`, OWNER-3 de PB-T0)

**Pregunta:**

> Rack de un sentido con una mitad B «dormida». Es un rack de un solo sentido que guarda dentro una mitad B desactivada, como el caso de prueba pb-21-cola. Al pulsar Actualizar, RACKEDITAR pierde esa mitad guardada, aunque el dibujo no cambia. El programa actual ya no crea racks así (está medido en AutoCAD), y ninguno de tus 22 racks reales medidos es así; el único con mitad B es un rack A/B normal. El panel del Workspace ya los rechaza. ¿Qué hago?

**Opciones:**
- **«Dejarlo fuera de la garantía (Recomendado)»:** PB-T1 garantiza «sin pérdidas» para los racks que el programa crea hoy, y este caso queda declarado como no soportado. RACKEDITAR sigue como hoy y el Workspace lo sigue rechazando. Coste: cero.
- **«Lo mismo, más un aviso»:** Igual, pero RACKEDITAR muestra un aviso al abrir un rack así. Es pequeño e iría en PB-TC2.
- **«Corregirlo»:** Guardar la mitad dormida en el documento. Cambia el formato guardado y una decisión anterior (I-42). Es una tarea grande.

**Respuesta del Owner:** «Dejarlo fuera de la garantía (Recomendado)».

## Pregunta 4 (emisión)

**Pregunta:**

> Para no interrumpirte en cada paso: ¿emito PB-TC2, si la eliges, y después PB-T1 sin volver a preguntarte, siempre que sus revisiones independientes salgan sin fallos bloqueantes ni importantes y no aparezca ninguna decisión nueva? Si aparece algo que es tuyo, me paro y te lo traigo.

**Opciones:**
- **«Sí, emite con esa condición (Recomendado)»:** Avanzo PB-TC2 y PB-T1 de principio a fin. Solo vuelvo a ti por una decisión nueva, un control con firma no prevista, un intento 3 o un efecto material sin medir.
- **«No, pregúntame antes de cada emisión»:** Preparo y reviso cada plan, pero espero tu orden antes de emitirlo.

**Respuesta del Owner:** «Sí, emite con esa condición (Recomendado)».

## Alcance

- **H-31 y OWNER-1 = (A).** Una tarea propia, **PB-TC2**, va antes de PB-T1. Contiene:
  - la regla `Keep` en `GapBox`, en `ModuleLengthBox` y en las cuatro cajas avanzadas anulables (el diseño del anexo A de PB-TC v2, ya revisado);
  - la caja mixta de «Ambos lados», que aplica el valor escrito a los dos lados.

  Es un cambio declarado de RACKEDITAR por D-PB-06 («rounding behavior/state»), con presupuesto propio de 1/3 (D-PB-10).
- **c-02 = (B).** PB-T1 reproduce la sustitución de hoy como **regla explícita del estado**: un poste fuera del catálogo pasa al de por defecto, como `ResolveBeamId` para las vigas. Lleva una prueba y RACKEDITAR no cambia. Los demás casos de T-2 (normalizaciones de Application y los rechazos c-12 y c-13) se quedan como hoy, y PB-T1 los reproduce.
- **`pb-21-cola` = (C).** El contrato de PB-T1 declara que un rack de un sentido con cola compuesta guardada **no es un «supported classic rack»** para la garantía sin pérdidas. RACKEDITAR sigue como hoy. El Workspace conserva `DormantTailOutOfPilotReason` en todo F4B. Guardar el lado dormido sigue siendo la deuda H14.
- **Emisión: autorización condicional.** PB-TC2 y después PB-T1 se emiten sin volver al Owner si se cumplen tres condiciones:
  - sus revisiones independientes acaban con 0 bloqueantes y 0 importantes sin resolver;
  - no aparece ninguna decisión material nueva;
  - se cumplen las demás reglas de F4B ([DP], D-PB-10 y las paradas que vuelven al Owner).

  La sesión vuelve al Owner por:
  - una decisión nueva;
  - un control con una firma no prevista;
  - un intento 3;
  - un efecto material sin medir.
- **Informativo:** uno de los dos dibujos reales medidos tiene un rack A/B. Puede servir para Smoke-PB (D-PB-09).
