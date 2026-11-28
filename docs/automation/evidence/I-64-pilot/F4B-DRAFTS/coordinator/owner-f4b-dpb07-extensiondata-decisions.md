# I-64 F4B — Decisiones del Owner: D-PB-07 (antes de PB-T2) y la ExtensionData anidada (antes de PB-T3)

**Fecha:** 2026-11-19. **Canal:** chat de la sesión principal, una ronda de dos preguntas de opciones; respuestas literales. **Las vueltas:** `F4B-DRAFTS/pbfull/owner-return-dpb07.md` y `F4B-DRAFTS/pbfull/owner-return-extensiondata.md` (expedientes medidos por lectura de objetos git en `2f5d3dec`).

## Pregunta 1 — D-PB-07 (OB-07.1)

**Texto, literal:** «D-PB-07 (antes de PB-T2): «Editar todo» abrirá el editor Push Back desde el panel, sobre el borrador y sin escribir en el dibujo. Insertar e «Insertar varias…» ya quedan fuera porque dibujan (D-PB-01). Faltan dos botones que no tocan el dibujo: «BOM» (ver y exportar a Excel o CSV) y «Guardar en biblioteca» (un archivo .rackcad.json). ¿Cuáles ofrece el editor en ese modo?»

**Opciones ofrecidas:** «BOM y biblioteca (Recomendado)»; «Solo el BOM»; «Ninguno de los dos».

**Respuesta del Owner:** «BOM y biblioteca (Recomendado)».

**Lo que implica (opción A del expediente):**
- En el modo WORKSPACE, el editor ofrece «BOM» (con su exportación) y «Guardar en biblioteca». Insertar y el lote de vistas siguen ocultos (D-PB-01, OB-01.6). RACKEDITAR no cambia.
- **Obligaciones para el contrato de PB-T2:**
  - dos costuras de prueba nuevas, para la ventana del BOM y para el diálogo de guardado, sin romper los literales de S-7, S-9 y CT-22. Si no pueden quedar en verde, se vuelve al Owner antes del contrato (OB-T1B.3);
  - la prueba UI-STA de OB-01.3 pulsa BOM y biblioteca en modo WORKSPACE y comprueba:
    - ninguna petición clásica (`InsertRequested` false, `InsertionRequest` null);
    - el borrador sin cambios hasta «Aplicar al borrador»;
    - el GUID del borrador sin cambios (el dato de H-PB-33, §2.1 del expediente);
    - el archivo de biblioteca con los campos desconocidos del rack (I-11);
  - el dueño de esas dos ventanas hijas en modo WORKSPACE lo diseña PB-T2 (D-15, punto 4).
- **Se aceptan,** declaradas, dos salidas a disco fuera del borrador (la exportación del BOM y el archivo de biblioteca), que pueden reflejar valores no aplicados ni dibujados.

## Pregunta 2 — La ExtensionData anidada y el paso 6 de D-PB-05 (OB-05.2, IR2 R-I-10)

**Texto, literal:** «Antes de PB-T3 (paso 6 de «Conservar mi borrador»): un campo nuevo dentro de un módulo o de un lado (de una versión más nueva del plugin o editado a mano) no se conserva al reescribir. Hoy el panel no escribe esos racks: los rechaza con aviso y remite a RACKEDITAR, que sí borra ese campo sin avisar (pérdida antigua). ¿Basta así para seguir con PB-T3?»

**Opciones ofrecidas:** «Basta así (Recomendado)»; «Basta, y que RACKEDITAR avise»; «No basta: conservarlos».

**Respuesta del Owner:** «Basta así (Recomendado)».

**Lo que implica (opción A del expediente):**
- PB-T3 sigue. El panel mantiene el rechazo con aviso de AUTH-13 (I-58) para todo campo desconocido a cualquier nivel, y el contrato de PB-T3 lo fija con una prueba.
- La pérdida silenciosa de RACKEDITAR («Actualizar» y «Guardar en biblioteca» con el lector tolerante) queda declarada como **limitación conocida anterior a F4B**, sin tocar el editor clásico (D-16, INV-19).
- **Nota para el paquete de la fase** (la sesión, no el Owner):
  - la línea ~314 («…se pierden en cada reescritura, clásica o del panel») se precisa: el escritor pierde lo anidado, pero el panel no llega a escribir esos racks;
  - la promesa de OB-05.2 de conservar los campos desconocidos de nivel superior en el panel no se ejercita hoy, porque AUTH-13 también los rechaza.
