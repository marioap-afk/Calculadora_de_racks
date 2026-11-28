# I-64 F4-T4B: decisión del Owner sobre el movimiento de `Traverse` tras el cierre de F4-T1B (2026-11-14)

**Contexto.** La sesión preguntó en el chat (AskUserQuestion) después de medir `map-4-traverse-t1b-coverage.md` sobre `7c29a5e5`.
- La disposición `owner-f4t4-disposition-reverify-t1b-t4b.md` autoriza el movimiento «if and only if», entre otras cosas, «characterization T1B protects the relevant existing behavior».
- El mapa mide que esa condición **no se cumple**:
  - T1B no lee `RackSiblingScan.cs` ni el cableado del barrido; su contrato los puso en el prohibido;
  - ninguna prueba ejecuta `Traverse`, porque el proyecto de pruebas no carga el Plugin y la CI no tiene AutoCAD;
  - nada en T1B revela una diferencia de comportamiento.

Se transcriben la pregunta y la respuesta literales.

**Pregunta:** «F4-T4B: autorizaste mover el núcleo de Traverse (RackSiblingScan.cs, el barrido que comparten Insertar, el insertar de RACKEDITAR, RACKPROYECTAR y el lector del piloto) «si y solo si» T1B protege el comportamiento relevante. Medido: T1B no lo cubre (no lee ese archivo ni su cableado), y ninguna prueba puede ejecutar Traverse sin AutoCAD. ¿Cómo sigo con la v4 de T4B?»

**Respuesta:** «Guardas exactas en T4B (Recommended)».

La descripción de la opción: «Se mantiene el movimiento y la condición (3) se sustituye, declarado, por protección propia de T4B: una guarda de texto exacto del núcleo contra el literal de c3ae1c97 (con autoprueba negativa), una guarda exacta del envoltorio y de Capture/CaptureDrawing, un censo de llamadas al núcleo, 5 clases más en RequiredTests y la cobertura de Smoke-2 de los cuatro llamadores.»

Las alternativas, que no se eligieron:
- «Tarea T1C previa»: una caracterización propia del barrido antes de T4B.
- «Sin mover Traverse»: rediseñar la relectura del piloto.
