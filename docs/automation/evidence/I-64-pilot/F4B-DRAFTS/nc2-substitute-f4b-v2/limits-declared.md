> **Redacción para el repo público:** la ruta de la revisión en la carpeta de la sesión se sustituye por la de su custodia, en esta carpeta. El resto no cambia.

# PBTx-NC2-EXACT-WHITELIST-F4B-01 (v2) — la versión revisada y sus límites declarados

**La versión que se usa:** `nc2_exact_whitelist_substitute_f4b_v2.py`, SHA-256 `C033E96CE147963FEB9C04733E9BFBEC7493FEA33D8FF79B1C2E8AB860798E45`, de PB-T3b en adelante. La autoriza la decisión del Owner «Aceptar y ampliar el sustituto» (F4B-E43, `owner-pbt3a-nc2-decision.md`).

**La revisión:** independiente, en la sección «Recomprobación v2» de `review/independent-review-substitute-f4b-v2.md`. Su veredicto: **APTO**, B 0 / I 0 / M 4.
- **La propiedad 8:** la igualdad es entera y exacta contra la lista cerrada de dos literales, el de PB-T2 y el de PB-T3. El de PB-T3 es igual a `ExpectedEvidence[0]` de `485b5651`. Diez variantes dan FAIL: espacio, puntuación, mayúscula, NBSP, cirílica, tilde y la mezcla de T2 y T3.
- **El diff frente al v1:** solo la tupla de la propiedad 8 y `--task`.
- **La autoprueba:** 63 de 63, determinista.
- **Los casos reales:** PB-T2a y PB-T3a dan PASS 10/10.

**Los límites declarados.** Son menores y no se corrigen, porque corregirlos cambiaría el SHA y obligaría a otra revisión. La sesión usa la versión revisada tal cual.
- **M-V1:** `--task` solo se liga, por prefijo, al `TaskId` de las dos delegaciones. No se contrasta con el contrato, la salida cruda, los relevos, el bfc, el handoff ni las dos `output.json`. Una tarea equivocada por accidente sí da FAIL (S11). Un PASS indebido exige reetiquetar los datos a mano y recalcular sus hashes, y la custodia fija esas copias por SHA-256.
- **M-V2:** con `VALIDATION` sobre datos reales sale `SubstituteResult = PASS` y código 0. Lo distinguen solo el `Id` genérico y el `Record`. La cadena llama siempre con la tarea concreta (`--task PBT3B` y siguientes), nunca con `VALIDATION`.
- **M-V3:** la segunda línea «Uso» del docstring está desfasada: lleva el nombre del v1 y no lleva `--task`. La válida es la primera, y los guiones de la cadena siguen esa.
- **M-V4:** T1-T6 prueban la función de ligadura `check_task`, no cómo la usa `main()`. Esa parte la cubren las corridas reales y las sondas de la revisión (S7-S11).

**Las observaciones de la revisión,** que no cuentan como hallazgos:
- El literal se acepta en cualquier posición de `ExpectedEvidence`, igual que en el v1.
- Cualquier literal de la lista vale para cualquier `--task`. La revisión lo considera conforme a la decisión del Owner: la lista lleva un literal por paquete.
- La igualdad se hace sobre el texto decodificado.
- Si `--task` se repite, gana el último.
- Los contratos de PB-T3b y PB-T3c no estaban en git cuando se revisó. La emisión de cada uno debe repetir la comprobación de su `ExpectedEvidence`: el literal de la entrada de la comparación byte a byte tiene que estar en la lista. Si no está, hay que añadirlo y volver a revisar el sustituto, como manda la decisión del Owner.

**La trazabilidad:** la revisión anterior aprobó `4cd4c01f`. El v1 custodiado, `6ccaf12d`, añadió después M-R1 a M-R3, que esta ronda da por correctas (N1, N2, N3, N5, N6 y N8 dan FAIL con la v2 sobre PB-T3a).
