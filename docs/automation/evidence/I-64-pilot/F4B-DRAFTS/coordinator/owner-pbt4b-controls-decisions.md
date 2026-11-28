# Decisiones del Owner sobre los controles nc1 y nc3 de PB-T4b (I-64 F4B), intento 1

**Fecha:** 2026-11-21. Después de la verificación `R20261121T133638Z-1b82` (EXECUTION_VERIFIED 14/14) y de sus tres controles:
- **nc1** `R20261121T134602Z-5688`: CONTROL NOT PASSED. Difieren de la verificación real Handoff, Identity, Remote, Scope, Ci, Tests y Trailer, con las paradas S-04 y PBT4B-C18. El sustituto revisado de PD-F4B-01 (`apply_nc1_cascade_pbt4b.sh`, `--task PBT4`, ScriptSha256 `41440edc…`) dio FAIL en la corrida `R20261121T140108Z-d3ca`, por `d1_FailSetWithinAllowed` (Trailer está entre los fallos) y `e2_OtherFailCitesFakeSha` (la evidencia de Trailer no cita el SHA falso literalmente).
- **nc2** `R20261121T135034Z-464a`: CONTROL PASS ordinario.
- **nc3** `R20261121T135520Z-2bac`: CONTROL NOT PASSED. FreeText detecta la frase inyectada, pero antes difiere Tests, con PBT4B-C04: según el control, 171 de los 306 fallos del RED son excepciones y no aserciones.

La sesión preguntó con AskUserQuestion, en una sola vuelta de dos preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «nc1»)

**Pregunta, literal:**

> PB-T4b, control nc1 (pone un SHA falso en la entrega y la verificación tiene que pararlo): la verificación del control sí lo paró, y todos sus fallos salen de ese SHA falso. Pero esta vez también marcó la comprobación del pie de los commits (Trailer), con la misma causa dicha con otras palabras («no existe el objeto CurrentSha»). El sustituto revisado no admite ese fallo extra y da FAIL. Es la sexta variación de nc1 en F4B (antes: PB-TC, PB-TC2, PB-T2a, PB-T2b y PB-T4a), y un sustituto solo vale con la firma exacta. ¿Qué hago?

**Opciones:**
1. **Aceptarlo solo esta vez (Recomendado):** nc1 queda satisfecho por tu decisión, solo en este caso y sin ampliar la firma del sustituto. Igual que en PB-TC y PB-T4a.
2. **Aceptar y revisar el sustituto:** Lo mismo para PB-T4b, y además, antes de PB-T4c, una revisión del sustituto para que admita también el fallo en cascada de Trailer. La firma nueva te volvería con esa revisión.
3. **Repetir nc1; si falla, aceptarlo:** Una corrida más de nc1, unos 5 minutos. Si vuelve a salir corta, queda satisfecho por tu decisión (como en PB-TC2).

**Respuesta del Owner, literal:** «Aceptarlo solo esta vez (Recomendado)».

## 2. Pregunta (encabezado «nc3»)

**Pregunta, literal:**

> PB-T4b, control nc3 (mete en la entrega una frase prohibida y la verificación tiene que detectarla): la detectó, que es para lo que existe. Pero además marcó las pruebas: dijo que 171 de los 306 fallos del RED son excepciones (NullReferenceException) y no aserciones. La regla del contrato (INV-PBT4B-02) dice que cuenta como aserción un fallo cuya primera línea está en el código de pruebas, y entonces los 306 son por aserción. Así lo vieron la verificación real (14/14) y la revisión independiente. Lo que probablemente confundió al control es una etiqueta de nuestro registro, «PorAsercion», que solo cuenta los Assert (la revisión ya la señaló). No hay sustituto para nc3. ¿Qué hago?

**Opciones:**
1. **Aceptarlo solo esta vez (Recomendado):** nc3 queda satisfecho por tu decisión: detectó la frase y el fallo de pruebas es una mala lectura de la regla. La etiqueta del registro se corrige en la cadena de PB-T4c, para que no vuelva a confundir.
2. **Corregir y repetir todo:** Se corrige ya la etiqueta del registro y se repiten la verificación y los tres controles. No gasta intento, pero cuesta alrededor de una hora y puede volver a variar nc1.
3. **Repetir solo nc3:** Una corrida más de nc3, igual que esta, unos 5 minutos. Si vuelve a salir corta, te vuelve.

**Respuesta del Owner, literal:** «Aceptarlo solo esta vez (Recomendado)».

## Lo que queda decidido

- **nc1 = SATISFIED_BY_OWNER_DISPOSITION,** solo para esta corrida. El sustituto no cambia y no se amplía ninguna firma. nc1 no se repite.
- **nc3 = SATISFIED_BY_OWNER_DISPOSITION,** solo para esta corrida. nc3 no se repite.
  - La etiqueta «PorAsercion» de las Notes del relevo del trabajo (M-07 de la revisión independiente) se corrige en la cadena de PB-T4c: cuenta por la regla de INV-PBT4B-02, o se renombra.
- **nc2 = CONTROL PASS ordinario.**
- Con eso, los controles de PB-T4b quedan satisfechos, y sigue el cierre.
