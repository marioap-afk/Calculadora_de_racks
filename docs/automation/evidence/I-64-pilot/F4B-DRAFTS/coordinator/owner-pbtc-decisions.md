# I-64 F4B — Decisiones del Owner antes de emitir PB-TC (F4B-E06)

**Fecha:** 2026-11-17 (noche). **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación.

**Contexto:** la vuelta al Owner que prevén [P]:766, [OD]:153-155 y F4B-E05 («Siguiente»), con el paquete v2 del contrato de PB-TC.
- La v2 se revisó: revisión independiente v1 = NO EMITIBLE 1/4/10; comprobación acotada de la v2 = EMITIBLE TRAS CORRECCIONES 0/1/9.
- La comprobación acotada no encontró ninguna decisión material nueva, además de las cinco del §8.

Las preguntas y las respuestas se transcriben **literales**.

## Pregunta 1 (OD-PBTC-1; sub-decisión R-03)

**Pregunta:**

> Decimales: hoy, al abrir un rack con RACKEDITAR, diez números se guardan redondeados a 3 decimales aunque no los toques. Son el fondo y la tolerancia de la tarima, el peralte del poste, la escala de anotación y la altura del primer nivel, y en la celda el frente, el alto, el peso, el claro y el largo manual. El peralte IN/OUT no está entre ellos: siempre se guarda como 6, el del catálogo. Con valores de prueba, corregirlo mueve textos y cotas hasta 0,018" y piezas hasta 0,0013". El BOM cambia solo si el frente de la tarima o el largo manual traen decimales. No cambian ni el número ni el tipo de piezas, ni los niveles. Falta medir qué hace hoy con los módulos si el fondo trae decimales: se medirá antes de corregir, y si cambian piezas o el BOM vuelvo a ti. ¿Lo corrijo?

**Opciones:**
- **«Corregir, regla Cantilever (Recomendado)»:** Se guarda el valor exacto. Vale al abrir, al cambiar de celda, con «Ambos lados», al aplicar a otras celdas y al cargar de la biblioteca. Si escribes en la caja algo que se ve igual (61.2350 sobre 61.235), se conserva el exacto; si escribes algo distinto, manda lo tuyo. Es la regla que ya usa Cantilever.
- **«Corregir, toda edición manda»:** Igual, pero cualquier cosa que escribas en la caja manda aunque se vea igual. Necesita algo más de código y una prueba más.
- **«No corregir»:** Se deja el redondeo. La tarea siguiente, la que separa el estado, tendría que imitarlo, y habría que replantear el plan de la fase.

**Respuesta del Owner:** «Corregir, regla Cantilever (Recomendado)».

**Alcance:**
- OD-PBTC-1 (i), con la extensión declarada más allá de la ruta de carga: selección de celda, «Ambos lados», alcance y datos del frente, y biblioteca.
- R-03 (i), la regla `Keep` de Cantilever.
- La vuelta por PBTC-C14 se mantiene: si el delta de la base de CT-PBTC-10 es material, vuelve al Owner antes del GREEN.

## Pregunta 2 (OD-PBTC-2)

**Pregunta:**

> Estilo de cotas: hoy RACKEDITAR olvida el estilo de cotas guardado en el rack y lo borra al pulsar Actualizar. Dibuja con el estilo actual del dibujo y fuerza el tamaño de texto, las flechas y 2 decimales. Si lo corrijo, cuando el dibujo tenga un estilo con ese nombre las cotas saldrán con él tal cual; si no lo tiene, todo queda como hoy. Lo mismo al insertar una vista o al guardar en la biblioteca. En tus 22 racks medidos no cambia nada: ninguno tiene estilo ni dibuja cotas. ¿Lo corrijo?

**Opciones:**
- **«Corregir (Recomendado)»:** RACKEDITAR conserva el estilo guardado, igual que ya hace el panel del Workspace. No cambian los puntos medidos, la capa, el número de cotas ni el BOM.
- **«No corregir»:** Se deja como hoy: el estilo se pierde al editar.

**Respuesta del Owner:** «Corregir (Recomendado)».

**Alcance:** OD-PBTC-2 (i).

## Pregunta 3 (OD-PBTC-3)

**Pregunta:**

> Aviso del lado B vacío: si marcas «Rack de dos sentidos (lado B)» sin activar ningún frente del lado B, ese lado no se guarda. Se mostrará un texto que lo explica; solo informa, no bloquea nada y no se puede enfocar. ¿Qué texto y en qué lugar?

**Opciones:**
- **«Texto corto, franja de estado (Recomendado)»:** En la franja de estado de abajo, siempre visible, con la misma letra que la línea de estado. Respeta la regla actual de no poner mensajes de estado en el panel lateral. Vista previa:

  ```text
  Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.
  ```

- **«Texto corto, bajo la casilla»:** Justo debajo de la casilla del lado B, en el panel lateral. Queda junto al gesto que lo provoca, pero se desplaza con el panel y rompe la regla de no poner mensajes de estado ahí. Vista previa: el mismo texto corto.
- **«Texto largo, franja de estado»:** Añade qué pasa al reabrir el rack. Solo es exacto en la ventana clásica; en el futuro modo Workspace no está comprobado. Vista previa:

  ```text
  Lado B declarado sin ningún frente activo: así no se guardará, y al reabrir el rack la casilla aparecerá desmarcada. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.
  ```

**Respuesta del Owner:** «Texto corto, franja de estado (Recomendado)». La vista previa seleccionada es la siguiente:

```text
Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.
```

**Alcance:**
- el lugar L2, un `TextBlock` en `StatusContent`, detrás de `StatusText`;
- el literal de arriba, byte a byte.

## Pregunta 4 (OD-PBTC-4 y OD-PBTC-5; selección múltiple)

**Pregunta:**

> Dos arreglos más pueden entrar en esta misma tarea, ya especificados y revisados. El primero: hoy, tras «Restaurar estándar», pulsar «Actualizar» vuelve a escribir los valores anteriores de altura de cabecera, poste derivado y separadores. El segundo: el hueco entre lados, la longitud de un módulo y cuatro parámetros avanzados también se guardan redondeados con solo pasar por la caja. Si no marcas uno, «Restaurar estándar» se hace como tarea aparte y las otras cajas te las vuelvo a preguntar, ambas antes de la tarea que separa el estado. ¿Cuáles entran?

**Opciones:**
- **«Restaurar estándar (Recomendado)»:** Al Actualizar se escribe lo que pediste al restaurar. Cuesta una prueba nueva, sustituir una prueba actual y una comprobación corta. Cambia lo que se dibuja en ese caso.
- **«Otras cajas que redondean (Recomendado)»:** La misma regla y los mismos archivos. Cuesta tres pruebas más, sin chocar con ninguna actual, y una comprobación corta. Además, escribir una longitud de módulo que se ve igual a la actual deja de marcarse como cambio pendiente.

**Respuesta del Owner:** «Restaurar estándar (Recomendado)». Solo esa opción.

**Alcance:**
- **OD-PBTC-4 = (A).** El anexo B se incorpora en la v2.1 de PB-TC como corrección declarada nueva, D-PB-11: H-PB-28, «Restaurar estándar» llega al Actualizar. Eso trae consigo:
  - CT-PBTC-14;
  - la sustitución S-04 de CT-PB0-05;
  - las rutas añadidas.
- **OD-PBTC-5 = (B).** El anexo A **no** entra en PB-TC: `GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables. Por el texto de la pregunta, el caso vuelve al Owner **antes del contrato de PB-T1**, la tarea que separa el estado, por OB-06.5. El registro declarado lo anota como pendiente.

## Informativo dado en el §8 de la v2

- El paso 4 de D-PB-06 en el Workspace y el aviso de D-PB-08 en el modo WORKSPACE se prueban en PB-T2. [P] lo asigna así, y [OD]:144-147 pide los dos modos.
- La sesión lo anota en `f4b-dispositions.md` como obligación de PB-T2 al emitir.
