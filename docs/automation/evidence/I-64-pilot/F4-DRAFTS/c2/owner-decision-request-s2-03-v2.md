# Decisión del Owner: F4-S2-03, segunda versión (sustituye a la primera)

## Qué medí mientras no estabas (2026-11-16, AutoCAD, copias desechables cerradas sin guardar, ningún paso de Smoke-2)

1. **El DLL de C1 funciona en AutoCAD:**
   - «Editar» queda «Listo para editar» y «Comprobar ahora» queda «Limpio», sin la marca;
   - Retroceso borra a la primera;
   - tras «Actualizar», el rótulo «PB G» sigue en cada vista y el estado queda «Limpio».
2. **S2-03 ya existía antes de I-64.** `RACKEDITAR` clásico hace lo mismo: tras `U`, la pantalla no se refresca hasta `REGEN`. El panel hereda el redibujo clásico.
3. **Encontré la causa y una corrección de UNA línea, probada en AutoCAD con un build desechable:**
   - **la causa:** el redibujo abre las inserciones de las vistas para escritura, pero no les cambia nada, así que el Deshacer no las registra y `U` no las vuelve a dibujar;
   - **la corrección:** reasignarles su misma posición (`reference.Position = reference.Position;`). Con eso:
     - en el panel, `U` refresca sola, sin `REGEN`, y `REDO` funciona y refresca; tras «Actualizar» sigue «Limpio»;
     - en `RACKEDITAR` clásico, `U` refresca sola, y `REDO` funcionó 3 de 4: una vez dijo «Nothing to redo» y no se repitió al intentarlo de nuevo.
   - **No toca el Freeze:** ni el drenaje ni los eventos, sin `Regen` nuevo y con un solo paso de Deshacer. La opción (a) de la primera versión, con su enmienda del Freeze, ya no hace falta.

## Por qué no la apliqué sin ti

- La línea va en `Drawing/LateralHeaderDrawer.cs`, el redibujo que comparten Selectivo, Dinámico, Cabecera, Cama, Push Back, el ejecutor de variables y el Insertar de Selectivo. Todas las tareas de F4 han tenido prohibida esa carpeta.
- Cambia lo que se ve tras `U` y `REDO` en esos sistemas, y solo lo medí en Push Back. Es tu categoría «Undo» de D5.
- **Cantilever no se arreglaría:** tiene su propio bucle, igual y sin la línea.
- La alternativa de ponerla solo en el panel rompe guardas ratificadas del escritor (T4B/T4C1), otro caso de D5.

El paquete de la tarea (F4-T4-HOST-C2, v2) está escrito y con revisión independiente: EMITIBLE TRAS CORRECCIONES 0/4/12, todo aplicado. Está listo para emitir.

## Lo que necesito que decidas

1. **¿Corregimos S2-03 en I-64?**
   - **(W) [recomendada]:** sí, con la línea en el redibujo compartido. Corrige el panel y cambia, sin haberlo observado, el `U`/`REDO` de los demás sistemas que lo usan.
   - **(b):** no. Smoke-2 lo anota como limitación preexistente, y se arregla en otra iniciativa o en un hotfix.
   - **(W-p):** solo en el panel. No la recomiendo: rompe guardas ratificadas y deja el panel y el clásico distintos.
2. **Si (W), ¿Cantilever entra?**
   - **Fuera [recomendado]:** se queda como hoy y se declara.
   - **Dentro:** la misma línea en su bucle, sin medir.
3. **Si (W), ¿basta el riesgo declarado de los demás sistemas?**
   - **Basta [recomendado]:** para F4, y se anota para la OV final de I-64.
   - **No:** quieres un punto de OV por sistema.
4. **Si (W), ¿antes o después de Smoke-2?**
   - **Antes [recomendado]:** unas 3 o 4 horas de cadena, sin ti. Smoke-2 lo comprueba en los pasos 14 y 16.
   - **Después:** Smoke-2 sigue con el DLL de C1, que ya está listo (`<carpeta-del-DLL-de-C1>`), y S2-03 se anota como limitación.

Basta con contestar, por ejemplo: «W, Cantilever fuera, basta, antes».
