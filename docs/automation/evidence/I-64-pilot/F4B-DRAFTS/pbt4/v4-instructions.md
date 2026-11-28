# Instrucciones de la sesión para el paquete v4 de PB-T4

La recomprobación de la v3 está en la §14 de `independent-review-v1.md` y da **NECESITA v4, B/I/M = 2 / 3 / 4**. Escribe la v4 en `I-64-f4b-pbt4-gate-contract-package-v4.md`. Las versiones anteriores no se tocan.

**La base es ahora el cierre de PB-T3c:** `7f0d9aa3` (F4B-E48, GREEN `938a35f0`). Lee por objetos git en `7f0d9aa3` y quita los marcadores `<tras el cierre de PB-T3c>` que ya se puedan rellenar. La base de la CI: Core 19 129 = 19 123 + 6 omitidas; UI 2 451 = 2 430 + 21.

**Las reglas de siempre:**
- nada en el worktree;
- no compilar;
- Python con `-I`, en guiones escritos con Write;
- escribe solo en `hs65\pbt4\`.

## Lo que decide la sesión (aplícalo)

1. **B-07: los dos cableados del Plugin frente a `WorkspaceHostSourceGuardTests.cs:398-420`.**
   - **El abridor:** en dos piezas, como propone la §14. La clase anidada del controlador solo pone el modal. La ventana la abre una clase del Plugin fuera de `Plugin/Workspace`. Mide qué guardas mira cada carpeta candidata (`WorkspaceBridgeSourceGuardTests:364-367` para `Plugin/Views/` y las demás) y elige una donde todas sigan en verde. Si no hay ninguna, el paquete lo dice con la lista de roturas.
   - **La regla de la escala:** evalúa la propuesta de la §14, que pasa la lógica pura de `NumericFieldValidation` a Application para que haya una sola implementación y Core la tenga por defecto. Mide antes, por objetos git, las guardas de T1B y de F4 sobre `NumericField`, `NumericFieldValidation` y la caja de escala. Si moverla rompe guardas de T1B (la línea roja del Owner: «Do not silently weaken the old guard»), no la muevas. Busca otra salida que cumpla B-02 y B-07 a la vez, o deja las dos opciones con sus roturas para que la sesión elija. No inventes una tercera copia de la regla.
2. **B-06: las firmas y los números de Q4.**
   - Rehaz las firmas de causa contando los ayudantes del mismo archivo, a un nivel (`Open`, `Pilot`, `Draft`, `DraftOf`…, con `Find<PushBackPilotDraftState>`).
   - Añade las dos causas que faltan: «el campo sin regla», si sigue existiendo tras B-07, y las lecturas del piloto fuera del kit.
   - Quita la firma «por archivo», que mete las 428 en cualquier opción.
   - Da, para cada opción de Q4 y medido por texto en `7f0d9aa3`:
     - PB-T4a: 1;
     - PB-T4b: el número de la causa, más las roturas por letra y las de la UI de la elección;
     - PB-T4c: 1, más lo que deje B-07;
     - `<n4>`, las pruebas propias del piloto si se retira: unas 57 fuera del anfitrión y 130 en total. Mídelo y da el desglose.
   - La opción 3 no se mide por texto: dilo.
3. **I-19:** EIG:664-690 cuenta `pilot.Read` por cuerpo. Evítalo por diseño si puedes; si no, a la lista con su causa.
4. **I-20:** parte CT-PBT4C-10 en una prueba de UI (con la FVM real) y otra de Core, sin la UI.
5. **I-21 y la Q4:** reescríbela con los números reales y de forma neutral. Las opciones 1 y 2 se presentan con lo que cumplen y lo que no, sin «la única opción». La recomendación la decides tú con los números, y la justificas en una frase. Si las dos cumplen lo mismo, recomienda la que no rompe C02 ni el «como D-1»: «Saltar y duplicar».
6. **Q1 y Q3,** con los números concretos por opción de Q4. El Owner contesta las cuatro en una sola vuelta. Q3 formula «la lista que viste» como **cota superior**: se emite esa lista o una parte, y si el GREEN del Worker hace fallar otra prueba, vuelve al Owner.
7. **Q2:** matiza la cifra de unas 150 rutas, como dice la §14.
8. **M-24 a M-27:** corrige el texto desfasado.

## Lo que entregas

- `I-64-f4b-pbt4-gate-contract-package-v4.md`, con una tabla V4-xx al principio;
- `pbt4-measurements.md` ampliado;
- los guiones con sus salidas;
- `owner-questions-pbt4.md`: las cuatro preguntas en su texto final, ya con los números. Cada una es autosuficiente (el Owner solo ve el diálogo), tiene de 2 a 4 opciones, la recomendada va primero y cada etiqueta de opción tiene como mucho 5 palabras. Debe estar listo para pegarlo en AskUserQuestion.

**Responde con:**
- la solución de B-07 y sus guardas;
- los números por opción;
- el texto final de las cuatro preguntas;
- lo que quede abierto.
