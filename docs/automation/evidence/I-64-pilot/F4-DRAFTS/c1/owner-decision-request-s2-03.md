# Decisión pendiente del Owner: F4-S2-03 (la vista no se refresca tras `U` hasta `REGEN`)

## Qué pasa

Tras un «Actualizar» del panel, `U` deshace bien el cambio en el dibujo. Pero la pantalla sigue mostrando el cambio hasta un `REGEN`. Y si se escribe `REGEN` entre `U` y `REDO`, AutoCAD responde «Nothing to redo». Smoke-2 lo encontró en el paso 16.

## Qué se probó en AutoCAD (build de diagnóstico, copia desechable)

Al terminar `U`, `UNDO`, `REDO` o `MREDO`, si esa orden tocó una definición de bloque, el panel hace **una regeneración por código, en reposo y sin lanzar una orden**. Resultado:
- `U` refrescó la pantalla, con «Regenerating model.»;
- `REDO` siguió funcionando, con «Everything has been redone» y otro refresco;
- después, «Comprobar ahora» dio «Limpio».

## Por qué no lo corregí sin ti

Toca decisiones congeladas:
- **la lista del drenaje:** el Freeze dice que las operaciones del drenaje son estas cuatro «y solo estas» (D-04), y añadir la regeneración es una quinta;
- **las guardas de F1:** clasifican `Regen` como una escritura de la vista, que hoy está prohibida para los eventos (INV-03);
- **tu regla D5:** cambia lo que pasa tras Undo y Redo, que es exactamente su categoría «Undo»;
- **un caso sin medir:** hay que decidir cuándo dispara. Solo por el tipo «definición de bloque» queda sin medir el caso «`U` de una LINE», que podría regenerar sin necesidad. Restringirlo a las definiciones de bloque nombradas, excluyendo el espacio modelo, exige leer propiedades del objeto en el manejador, y eso necesita una aclaración propia (IC).

## Opciones

- **(a) [recomendada] Corregirlo en una tarea propia**, con el diseño probado:
  - la decisión va en Application y se puede probar sin AutoCAD;
  - la regeneración se hace en el drenaje programado y nunca en un manejador;
  - un puerto en `Plugin/Views` llama a `Editor.Regen()`;
  - va con su enmienda declarada del Freeze (D-04 e INV-03) y su IC.

  Mientras tanto, Smoke-2 declara la limitación.
- **(b) Dejarlo como limitación declarada:** tras `U`, el usuario escribe `REGEN`, y en ese caso no habrá Rehacer.
- **(c) Corregirlo solo en el Plugin** (como la build de diagnóstico): rompe 10 guardas existentes y no se puede probar sin AutoCAD. No recomendada.

**Nota:** con (a), el refresco tras `U` también ayudaría al editor clásico RACKEDITAR, que redibuja igual, pero solo si el panel está abierto.
