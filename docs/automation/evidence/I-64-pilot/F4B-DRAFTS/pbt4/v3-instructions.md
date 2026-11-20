# Instrucciones de la sesión para el paquete v3 de PB-T4

La comprobación acotada de la v2 está en la §13 de `independent-review-v1.md` y da **NECESITA v3, B/I/M = 2 / 7 / 8**. Escribe la v3 en `I-64-f4b-pbt4-gate-contract-package-v3.md`. La v1 y la v2 no se tocan.

**Las reglas son las de siempre:**
- solo objetos git en `7dfc7c21`;
- nada en el worktree;
- no compilar;
- Python con `-I`, en guiones escritos con Write;
- escribe solo en `hs65\pbt4\`.

## Lo que decide la sesión (aplícalo)

1. **B-05: el reenvío del Plugin.** El `PilotEditAdapter` del Plugin (WCo:52 y :283-332) reenvía hoy las 17 operaciones. En PB-T4c, el reenvío de `TryEditAll` entra explícitamente en el `AllowedWriteScope` y en la lista cerrada de miembros y cambios de cuerpo, con su CT: una guarda de texto o una prueba que falle en el RED si el adaptador no reenvía. Mira si hay otros reenvíos o envoltorios entre el panel y el anfitrión que tengan el mismo problema, y trátalos igual. «Editar todo» tiene que llegar de punta a punta, y una prueba tiene que demostrarlo sin AutoCAD.
2. **B-04 e I-18: el procedimiento de la lista va al Owner como Q4, aparte.** Añade la causa **C-0**, «la lectura viene del kit del piloto», con su firma mecánica. Define «su kit» por archivo, sin nada transitivo. Mide ya, por objetos git, todo lo que se pueda medir: las 9 clases y los 428 métodos que usan `EditIntentFakeReader` y `EditIntentKit.ReadOf` (EIH:17-52), por clase.
   - La Q4 tiene tres opciones. Usa como base el texto de la §13:
     - **(1) «Editar una línea del kit compartido» (Recomendado):** en PB-T4b, `EditIntentKit.ReadOf` (EIH:51-52) pasa a `FromReads`. Las 9 clases conservan sus aserciones, ahora sobre el camino real del producto, y solo se sustituyen con registro las que fallen por C-1 a C-7. Di qué regla rompe (C02 y el «como D-1» de A-T3-1) y por qué la sesión la recomienda: cumple a la vez (a) un criterio cerrado, (b) ninguna implementación oculta y (c) ninguna guarda debilitada en silencio. Di también cuántas pruebas quedarían en la lista con esta opción, si se puede estimar ya por objetos git.
     - **(2) «Saltar y duplicar, como D-1»:** equivalentes mecánicos, con un mapa cerrado de sustituciones y un guion que compare cada original con su copia, más C-0. Da el número que vería el Owner, que es mucho mayor.
     - **(3) «Una implementación de referencia de la sesión».**
   - Con cada opción, lo que exige al contrato de PB-T4b: el alcance, las paradas y la excepción a C02 si hace falta.
   - **C-5 (la letra de las guardas de texto):** mídelo sobre las líneas literales del GREEN que el contrato entrega al Worker, aplicadas a una copia solo para ejecutar esas guardas. Escribe ese procedimiento en el §5 o en las precondiciones.
3. **I-12:** elige el diseño que menos guardas rompa con el mismo efecto. Si elegir el origen en línea en cada cuerpo deja rota solo CT-PBT3A-12, hazlo así y dilo. Si no, pon en la tabla de PB-T4a todas las roturas: EIG:563-575, EIG:758-770 y `WorkspaceLifecycleGuardTests:642-656`.
4. **I-13: la clave del origen tras POST.** Diseña de dónde sale con una fuente real. `DraftApplyOutcome` no la expone, `PostOriginKey` está en el plan, y `StaleBase` en el cuerpo rompe EIG:1124. Si hace falta tocar un tipo de Application, dilo con su alcance y su CT. Es el caso central de A-T3-8.
5. **I-14:** añade a las tablas de rotura, con su causa C-x y su subtarea:
   - EIG:670 y :720;
   - `WorkspacePilotPrepareTests:1352` (11 estados de `EditIntentStatus`);
   - `WorkspaceDraftPilotValidationParityTests:80-97` (el texto del XAML con `x:Static`);
   - EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723` (los 9 archivos de `Plugin/Workspace`).

   Para cada una, si la evitas por diseño (por ejemplo, el abridor fuera de `Plugin/Workspace`, o sin `x:Static`), mejor: dilo.
6. **I-15:** corrige la fila de `RequestUpdate` en la tabla de K = 1 (`Busy` por la marca de lectura, WHC:1388, que pasa por `PassGate`).
7. **I-16:** CT de la condición 7 de D2 con una línea que discrimine. La letra de la condición está en `owner-d1-d2-decisions.md`: cítala y quita ese `<a medir>`.
8. **I-17, la Q2:** presenta las tres opciones con sus costes reales:
   - la tabla de hojas anidadas que exige «Por campos», con cuántas rutas;
   - el ruido de las hojas derivadas que informa el clasificador;
   - la longitud en un panel de 320 px.

   La recomendada es **«Por grupos»**, salvo que tus mediciones muestren que «Por campos» cuesta poco y no mete ruido. En ese caso, recomiéndala con los números. Di siempre que la letra de D-PB-05 dice «fields».
9. **Q1:** di que `<N>` es una selección por texto y que es probable volver a mitad de PB-T4b. Quita «cumple la letra de A-5». Para la opción (C), o das un número diseñado o dices que no se puede medir sin diseñarla.
10. **Los `<a medir>` que la §13 da por medidos** (la costura de `HeaderConfiguratorPresenter`, EIG:1150-1190, CT-PBT2B-08 y `WorkspaceCommandAdvisorGuardTests`): ciérralos con su cita. Quedan para la copia exportada solo K, D y E, las líneas de CT-PBT4B-06 y el coste de `IsDirty`.
11. **Los menores M-16 a M-23:** resuélvelos o explica por qué no.

## Lo que entregas

- `I-64-f4b-pbt4-gate-contract-package-v3.md`, con una tabla V3-xx al principio (hallazgo → cambio → sección);
- `pbt4-measurements.md` ampliado;
- los guiones nuevos con sus salidas.

**Responde con:**
- los cambios de diseño (B-05, I-12 e I-13);
- las cuatro preguntas al Owner, en su texto final con huecos;
- los números que ya puedas dar para la Q4;
- lo que no resuelvas.
