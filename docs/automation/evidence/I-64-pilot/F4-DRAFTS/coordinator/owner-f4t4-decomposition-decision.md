Decisiones del Owner sobre la descomposición de F4-T4 y dos puntos de diseño (2026-11-13), tomadas tras la revisión independiente del paquete de F4-T4 v1 y por la herramienta de preguntas de la sesión. Preguntas, opciones y respuestas literales:

1. Pregunta: «F4-T4A (PREPARE/STALE/CONFLICT, solo lectura), medido sobre el código real: unos 22-25 archivos en cuatro capas (núcleo de borradores, piloto, host y puerto con sus reenvíos, VM del campo y XAML, y 8-10 pruebas y guardas). ¿Cómo la estructuro? (Una división duplica la capacidad de intentos de T4A, de 3 a 6; cada subtarea empieza en 1/3.)»
   Opciones:
   - «Dos subtareas (Recommended)»: T4A1-PREPARE-CORE: comprobación general de la base, «Conservar mi borrador» y «Descartar y recargar» en host y puerto, PREPARE puro con plan inmutable, arrastres de ComposeForWrite y R-3; todo probado en Core. Luego T4A2-PREPARE-UI: «Comprobar ahora» y la reconciliación del Conflicto en el panel, más las guardas Sí/No y Authorize de T3C2.
   - «Una sola tarea»: F4-T4A entera en un contrato (unos 22-25 archivos), attempts 1/3. Más riesgo de copia en la planificación y de una revisión con muchos hallazgos.
   Respuesta: «Dos subtareas (Recommended)».

2. Pregunta: «F4-T4B (MUTATE/POST/UNDO): el mapeo mide que es la tarea más amplia de F4. Lleva el contexto de comando (ExecuteInCommandContextAsync) y el puente del Task, excepciones en unas 15 guardas, la lectura caller-owned, la transacción con la frontera de Commit y la inyección Debug, las fachadas de Push Back, las unidades por vista, la importación de biblioteca, POST con relectura, el ámbito de mutación propia, la UI de Actualizar y la medición en host de Undo. ¿La divido ya?»
   Opciones:
   - «T4B + T4C ya (Recommended)»: T4B-MUTATE-POST: transacción atómica, lectura caller-owned, fachadas, unidades, importación y POST, probadas con puertos simulados y guardas. T4C-HOST-UNDO: contexto de comando, ámbito de mutación propia, UI de Actualizar y la medición en host de Undo como primer paso (con A-n al Owner si Undo no es un paso o el callback es diferido). Sus paquetes se fijan tras cerrar T4A.
   - «T4B única; T4C solo si hace falta»: Un solo contrato de T4B; T4C solo si la medición en host lo exige. Riesgo: T4B excede a un Worker y la medición de Undo no tiene vehículo propio.
   Respuesta: «T4B + T4C ya (Recommended)».

3. Pregunta: «Canonicalización de dibujos antiguos (LegacyDeltas): al actualizar, el piloto reescribe el diseño entero desde la base autoritativa recién releída (no desde copias del borrador), como la ruta de Application. En dibujos antiguos eso normaliza campos que el piloto no edita (HeaderPostCatalogId, LegacyHighEndBeamPeralte, SideB.IsPresent…). ¿Lo acepto?»
   Opciones:
   - «Aceptar como Application (Recommended)»: El piloto escribe desde la base autoritativa, con la misma canonicalización que la ruta de Application; se declara en T4B y se observa en Smoke-2 con un dibujo antiguo.
   - «Antiguos: No editable aquí»: Un rack que requiera canonicalización se muestra como «No editable aquí» en el panel y se edita con RACKEDITAR; el piloto solo escribe dibujos ya canónicos.
   Respuesta: «Aceptar como Application (Recommended)».

4. Pregunta: «La «relectura autoritativa antes de actualizar» que asignaste a T4A: el Freeze (D-12) pone la autoridad en una relectura dentro de la transacción de MUTATE, con una variante caller-owned de la lectura que aún no existe. T4A puede dar la comprobación previa de solo lectura (O-04, que no sustituye a esa relectura). ¿Dónde pongo la variante caller-owned de solo lectura?»
   Opciones:
   - «En T4B, usada en MUTATE (Recommended)»: T4A entrega O-04, la comparación, el plan y la clasificación; la lectura caller-owned (solo lectura, con su guarda) va en T4B, donde se usa dentro de la transacción. La autoridad de D-12 sigue en MUTATE.
   - «En T4A (solo lectura)»: La variante caller-owned de solo lectura, con su guarda, entra en T4A (Plugin/Views); T4B solo la llama dentro de MUTATE. T4A crece con archivos del Plugin.
   Respuesta: «En T4B, usada en MUTATE (Recommended)».
