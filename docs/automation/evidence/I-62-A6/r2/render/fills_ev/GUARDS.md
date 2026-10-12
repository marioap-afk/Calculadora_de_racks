`docs/automation/evidence/I-62-A6/r2/a6r2-guards.py` (solo biblioteca estándar; modos `self-test`, `preview` y `run`), adaptadas de las de la
ronda 1, que no cambian:

- **G1:** cabecera, literales, citas, identificadores, referencias a reglas y privacidad.
- **G2:** rutas del commit: solo `I-62-A-6.md` modificado y archivos añadidos bajo `r2/`.
- **G3:** superficies prohibidas, objetos congelados y C-20b, incluidas la revisión y la evidencia de la ronda 1.
- **G4:** correspondencia con el dictamen y con los hallazgos de la ronda 1 (§3.16), coherente con la línea `Cubre:` de cada delta.
- **G5:** pureza del delta; A6-2 y A6-6, byte a byte como en la ronda 1.
- **G6:** líneas del Owner, byte a byte, y la línea (a) en decisiones §78.
- **G7:** identidad por blob de las piezas.
- **G8:** estado y declaraciones.
- **G9, nueva:** alcance de R5. Solo VERIFY; PLAN excluido; fuentes de VERIFY y de los controles negativos.
- **G10, nueva:** el resultado del aceptante de A6-5 frente a los bytes del aceptante fijado (comprobaciones, dispensables, fórmula de la
  `Decision`, códigos de salida, datos de cada comprobación y entradas de la solicitud de A6-3) y los estados normativos citados en §2.
- **G11, nueva:** lista de todas las fuentes y rangos de líneas que el texto cita, para el cierre de la revisión siguiente (decisiones §79,
  punto 5).
- **G5b:** autoprueba custodiada.

La autoprueba mata todos sus mutantes sobre este texto, y los controles negativos de extremo a extremo dan el resultado esperado. Los recuentos y
los resultados de `preview` y de `run` sobre el commit que publique la ronda 2 están en `r2/` y en `r2/preflight.md`. Ninguna revisión formal si no
pasan.
