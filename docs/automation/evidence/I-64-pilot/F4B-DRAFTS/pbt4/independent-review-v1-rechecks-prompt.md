# Encargos de las recomprobaciones de la revisión independiente del paquete de PB-T4 (enviados por la sesión al revisor como mensajes; texto exacto)

El encargo de la revisión de la v1 es `independent-review-v1-prompt.md`. Cada sección de aquí es el encargo de la ronda del mismo número en `independent-review-v1.md`.

## 1. §13. Comprobación acotada de la v2 (2026-11-20T15:10:23.588Z)

Mensaje de la sesión. Haz la **comprobación acotada de la v2 del paquete de PB-T4**, sobre tu revisión de la v1. Las reglas de lectura son las mismas: solo objetos git en `7dfc7c21`, nada en el worktree, sin compilar, Python con `-I` y guiones `ir4_`.

**Las entradas** (en `hs65\pbt4\`):
- `I-64-f4b-pbt4-gate-contract-package-v2.md`, con la tabla V2-01 a V2-29 al principio;
- `pbt4-measurements.md` (R-13 a R-20 nuevas);
- `measure_pbt4_v2.py` y su salida;
- mis instrucciones al redactor, `v2-instructions.md`.

**La división nueva:** tres subtareas.
- **PB-T4a, solo aditiva:** el lector completo en el mismo escaneo; `EditIntentReadResult` gana `FullRead`, `Catalog` y `FromReads`; la relectura usa el origen, con `IWorkspaceEditIntentOriginReader`.
- **PB-T4b:** el tipo del borrador, OB-03.2, la regla de la escala en la UI (`AnnotationScaleRule`) y «Conservar» en dos tiempos por `TryKeepMine`.
- **PB-T4c:** «Editar todo» con `CommitField` como frontera, y `TryEditAll` con cuerpo por defecto.

**Qué comprobar:**
1. **Los 29 hallazgos:** ¿la v2 resuelve cada uno, o explica bien por qué no? Mira sobre todo B-01, B-02, I-01, I-02, I-06, I-07, I-08, I-09 e I-10. Comprueba por muestreo los hechos nuevos (ruta:línea).
2. **PB-T4a:**
   - ¿es de verdad aditiva?;
   - ¿rompe solo CT-PBT3A-12 y EIG:736, como dice?;
   - ¿algo de PB-T4a es «traducción de estado»?
3. **El procedimiento de la lista de sustituciones:** el §, sus siete causas cerradas y sus cuatro opciones. Quiero tu juicio sobre cuál cumple mejor tres cosas: (a) un criterio cerrado y verificable antes de emitir; (b) no fijar la lista contra una implementación que el Worker no recibe; (c) no debilitar en silencio ninguna guarda.
   - La opción 1 puede saltar pruebas que no fallarían: ¿eso debilita algo, o los equivalentes lo cubren?
   - ¿Las opciones 3 y 4 son decisiones del Owner, como dice el paquete?
   - Si ninguna sirve, dilo.
4. **Las preguntas al Owner con huecos** (Q1, Q2 y Q3): ¿neutrales, autosuficientes y completas? ¿Falta alguna, por ejemplo la elección del procedimiento si cae en la 3 o la 4? ¿La recomendación de Q2 («Por campos») está bien justificada?
5. **Los nueve `<a medir>` que quedan para la copia exportada** (precondición 6): ¿alguno debería medirse ya, por objetos git, porque cambia el diseño?
6. **Decisiones nuevas para el Owner,** si las hay.

**El informe:**
- Añade una sección a `independent-review-v1.md` con el encabezado `## §N. Comprobación acotada de la v2`, donde N es el siguiente número.
- Pon tu veredicto en una línea propia, con la forma «**Veredicto: …** B/I/M = **n / n / n**». Los valores posibles son: NECESITA v3, EMITIBLE TRAS CORRECCIONES, o LISTO PARA LA MEDICIÓN (lo que falta solo se puede cerrar midiendo sobre el cierre de PB-T3c).

**Responde con:**
- el veredicto;
- los hallazgos bloqueantes e importantes;
- tu juicio sobre el procedimiento de la lista;
- lo que digas de las preguntas.

## 2. §14. Recomprobación de la v3 (2026-11-20T15:38:16.701Z)

Mensaje de la sesión. Haz la **recomprobación de la v3 del paquete de PB-T4**, sobre tu §13.

**Las reglas:**
- las de siempre, con un cambio en la revisión: PB-T3c ya está CERRADA (F4B-E48, custodia `7f0d9aa3`, GREEN `938a35f0`), así que lee por objetos git en `7f0d9aa3`. Lo que la v3 midió en `7dfc7c21` vale si `src/` y `tests/` no cambian; compruébalo;
- no hay ningún Worker activo, pero sigue sin tocar el worktree;
- sin compilar;
- Python con `-I`, en guiones `ir4_`.

**Las entradas** (en `hs65\pbt4\`):
- `I-64-f4b-pbt4-gate-contract-package-v3.md`, con su tabla V3-01 a V3-16;
- `pbt4-measurements.md` (R-21 a R-27);
- `measure_pbt4_v3.py` y su salida, y `make_v3.py`;
- mis instrucciones, `v3-instructions.md`.

**Qué comprobar:**
1. **B-04 y B-05.**
   - B-05: los dos reenvíos (`WorkspacePilotEditGate` y `PilotEditAdapter`); CT-PBT4C-12 y 13; y el abridor como clase anidada del controlador, para que sigan en verde las guardas de «9 archivos» y `WorkspaceBridgeSourceGuardTests:364-367`. ¿Queda algún camino por el que «Editar todo» no llegue y las pruebas sigan en verde?
   - B-04: la causa C-0 y la Q4.
2. **I-12 a I-17,** con los diseños nuevos:
   - el origen en línea en cada cuerpo, y PB-T4a rompiendo solo CT-PBT3A-12;
   - I-13 sin clave guardada (el texto del origen del borrador y la regla de `ChooseOrigin`): ¿es correcto tras un POST que borra el origen?, ¿es traducción de estado?;
   - la clasificación como función pura de `Draft/`;
   - la tabla de K = 1;
   - D2, condición 7;
   - Q2.
3. **Los números de la Q4,** medidos por texto: 428 métodos en 9 clases, y unos 44 con firma de causa. ¿Son correctos?
4. **Las cuatro preguntas al Owner,** en su texto final.
   - Quiero hacerlas **en una sola vuelta, con números concretos**. Hoy Q1 y Q3 llevan huecos que dependen de la respuesta a Q4. Dime si los huecos se pueden rellenar ya, por texto y sobre `7f0d9aa3`, para cada opción de Q4. Por ejemplo, «con la opción 1 de Q4, N = …; con la 2, N = …», de modo que el Owner conteste las cuatro sin una segunda vuelta.
   - Dime si eso cumple la condición de Q3, «la lista que viste», o si la lista tiene que ser la medida al compilar.
   - Revisa además neutralidad, autosuficiencia y completitud.
5. **Si queda algo antes de preguntar al Owner,** que no sea la medición sobre la copia exportada.

**El informe:**
- una sección nueva en `independent-review-v1.md`, con el encabezado `## §N. Recomprobación de la v3`;
- tu veredicto en una línea propia, con la forma «**Veredicto: …** B/I/M = **n / n / n**», con uno de estos: NECESITA v4, LISTO PARA PREGUNTAR AL OWNER (con lo que falte, si es solo texto) o LISTO PARA LA MEDICIÓN.

**Responde con:**
- el veredicto;
- los hallazgos bloqueantes e importantes;
- lo que digas de los números y de la vuelta única.

## 3. §15. Recomprobación de la v4 (2026-11-20T15:59:45.733Z)

Mensaje de la sesión. Haz la **recomprobación corta de la v4 del paquete de PB-T4**, sobre tu §14. Es la última antes de preguntar al Owner.

**Las reglas de siempre:**
- lee por objetos git en `7f0d9aa3`;
- nada en el worktree;
- sin compilar;
- Python con `-I`, en guiones `ir4_`.

**Las entradas** (en `hs65\pbt4\`):
- `I-64-f4b-pbt4-gate-contract-package-v4.md`, con su tabla V4-01 a V4-10;
- `owner-questions-pbt4.md`;
- `pbt4-measurements.md` (R-28 a R-33);
- `measure_pbt4_v4.py` y su salida;
- mis instrucciones, `v4-instructions.md`.

**Qué comprobar:**
1. **B-07, la regla.** La lógica pura de `NumericFieldValidation` pasa a `Application/Formatting/NumericTextRule.cs`, movida y no copiada. La UI delega en ella, y `PushBackAnnotationScaleRule` da los parámetros de la escala. Comprueba tres cosas:
   - que ninguna guarda de T1B, de F4 ni de PB-T1 a PB-T3 lo impida o se rompa. El paquete mide que 27 archivos de prueba la usan por su API pública y que ninguna guarda de T1B mira `Controls/`. Busca también las guardas de capas, por ejemplo qué puede nombrar Application, y las de espacios de nombres;
   - que no cambie el comportamiento de ninguna caja numérica;
   - que no sea una decisión del Owner.
2. **B-07, el abridor en dos piezas:**
   - `FullEditorModal`, anidado en el controlador;
   - `Plugin/Views/RackWorkspaceEditorWindow.cs`.

   ¿Siguen en verde `WorkspaceHostSourceGuardTests:398-420`, `WorkspaceBridgeSourceGuardTests:364-367`, las de «9 archivos» y G-9? ¿Es CT-PBT1B-15 (14 → 15) la única rotura? ¿Hay alguna guarda sobre `Plugin/Views/` (las de PB-T3c, por ejemplo CT-PBT3C-04) que la nueva clase rompa?
3. **B-06:** las firmas con los ayudantes a un nivel, la causa C-9 y los números por opción: PB-T4b ≈ 275 con la opción 1 y ≈ 461 con la 2; 242 y 186 en la Q4; 99 o 149 si se retira el piloto. ¿Son razonables como cota superior? ¿Se explican bien las diferencias con tus 256 y 130/57?
4. **I-19, I-20, I-21 y M-24 a M-27:** resueltos o explicados.
5. **Las cuatro preguntas de `owner-questions-pbt4.md`:**
   - ¿neutrales, autosuficientes y completas, con números coherentes entre ellas?
   - ¿la recomendación de Q4 («Saltar y duplicar») está bien justificada?
   - ¿Q3 formula bien la cota superior?
   - ¿cada etiqueta tiene como mucho 5 palabras?
   - ¿falta o sobra alguna decisión?

**El informe:**
- una sección nueva en `independent-review-v1.md`, con el encabezado `## §N. Recomprobación de la v4`;
- tu veredicto en una línea propia, con la forma «**Veredicto: …** B/I/M = **n / n / n**», con uno de estos: LISTO PARA PREGUNTAR AL OWNER, LISTO TRAS CORRECCIONES DE TEXTO (y cuáles) o NECESITA v5.

**Responde con:**
- el veredicto;
- los hallazgos bloqueantes e importantes;
- las correcciones de texto que pidas para las preguntas, con el texto exacto.
