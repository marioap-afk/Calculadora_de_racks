# I-64 F4B PB-TC — Comprobación acotada de la v2.1 del paquete del contrato (la que se emite)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. Las rutas son relativas a la raíz del repositorio. «V21:n» es la línea *n* de `I-64-f4b-pbtc-gate-contract-package-v21.md`; «[OPD]» es `owner-pbtc-decisions.md`; «ME L-n» es la fila L-n de `pbtc-measurements.md`; «ICT2» es mi comprobación de la v2. Se usan los alias del paquete: W, WC, WX, NF, DA, DEA, CS, ES, WLC y S0.

**Veredicto: EMITIBLE. Quedan abiertos 0 bloqueantes, 0 importantes y 5 menores (0/0/5).**
- N-01 a N-10 de ICT2 quedan **RESUELTOS**, incluido el importante N-01.
- Las cinco decisiones del Owner están aplicadas con fidelidad. El literal del aviso es idéntico byte a byte al de [OPD].
- D-PB-11, ya en el cuerpo, se sostiene por el código.
- Los cinco menores son de redacción, de método de evidencia y de completitud. Conviene aplicarlos en la emisión, pero ninguno impide emitir.
- No hay ninguna decisión nueva para el Owner.

## 1. Método y límites

**Leído:**
- la v2.1 completa, con sus dos registros de cambios;
- [OPD];
- `v21-instructions.md`;
- el apartado L del registro de mediciones;
- ICT2, como referencia.

**Comprobado en la rama, en solo lectura** (HEAD `f9f2afbb`, árbol limpio):
- W: las siembras del rack (W:384-406), `ReadInputs`, la rama válida de `Recompute` (W:658-692, con `ClearModuleCommit` en W:682), `RestoreAllModules_Click` (W:2497-2501), `ConfirmModule_Click` (W:2455-2463), `RestoreRackParameters_Click` (W:1628-1634) y `LoadAdvancedRackParameters` (W:1555-1579);
- `PushBackEditorState.cs:139` y `:160-161`, `RackModuleEditSession.cs:40` y `PushBackAdvancedRackParameters.cs:148-161`;
- `PalletSpecification.cs:15-28`;
- WX: los atributos `IsOptional`;
- la guarda Fact3 (`tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:375-401`);
- CT-PB0-05 (`PushBackFullEditorCharacterizationModuleTests.cs:143-208`);
- `PushBackModuleEditorWindowTests.cs:279-302`;
- `RoundingEffectTests.cs:136-139`, `:164` y `:210-213`;
- `git grep` en `tests/` de `StandardRestoreRequested`, `RequestStandardRestore`, `RestoreRackParameters`, `RestoreAdvancedRackParameters`, `LoadAdvancedRackParameters`, `ClearModuleCommit`, `ModuleCommit` y `RestoreAllModulesButton`;
- los recuentos de los tres filtros nuevos;
- con `python -I` de solo lectura, la comparación del literal del aviso entre la v2.1 y [OPD].

**Límites:**
- nada se compiló ni se ejecutó;
- que, tras «Confirmar», la pasada que consume «Restaurar estándar» ocurra antes de que la prueba lea los controles lo deduzco del código: `ConfirmModule_Click` llama a `RequestRecompute()`, que es síncrono fuera de `Defer` (`RecomputeGate.cs:35-44`). No está ejecutado;
- `Assert.Fail(string)` en xUnit 2.9.2 se toma de la API (desde 2.5), no de una compilación.

## 2. El estado de N-01 a N-10

| Id | Estado | Evidencia |
|---|---|---|
| N-01 | **RESUELTO** | Las llamadas `SetNumber` de W:386-406 no cambian, ni su argumento (V21:349). Cada portador nuevo se asigna en su propia línea, con la misma expresión que pinta su caja (V21:350-354; comprobado frente a W:389, :391, :392 y :406). El fondo usa `generalPallet.Depth` (V21:355). Así, INV-PBTC-03 (1) se cumple tal cual (V21:898). Que `generalPallet` se sustituya por un objeto nuevo en `ReadInputs` (V21:357) es correcto: es la única lectura (W:819-820), el constructor de cinco argumentos existe (`PalletSpecification.cs:15`), y los inputs siguen tomando la unidad del desplegable (W:820) |
| N-02 | **RESUELTO** | `CATALOGO` se aplica solo a la fila F4 (V21:541-545). El §5.1 queda cierto: diez filas `REDONDEA` en la base (V21:715). El método se renombra a `…TheLoadedFields…` (V21:565 y V21:698) |
| N-03 | **RESUELTO** | «diez llamadas `Keep(` sobre esas diez cajas… y cambia `OptionalVal`» (V21:899). Cuadra con el diseño: cinco llamadas en `ReadInputs` y cinco en `ReadCellValues` (V21:361). Queda un matiz de redacción (N2-05) |
| N-04 | **RESUELTO** | K4 entra en PBTC-C14, con la fórmula completa de `RoundingEffectTests.cs:164` (V21:1095; comprobado). Se escribe «los cambios temporales» (V21:479-481) y se usa `Assert.Fail` (V21:480) |
| N-05 | **RESUELTO** | «al menos una línea», y F6 da dos (V21:658). La v2.1 cita `DynamicRackSystemResolver.cs:309`, que es la línea correcta; la cita `:306` de ICT2 era mía y estaba desplazada |
| N-06 | **RESUELTO** | Las seis líneas sin cambio son `:199-201` y `:204-206`, y las cuatro que cambian, `:202`, `:203`, `:207` y `:208` (V21:668-669, V21:745 y V21:762). Comprobado línea a línea |
| N-07 | **RESUELTO** (sin objeto) | No aplica sin el anexo A. Se conserva lo medido y queda para la vuelta previa a PB-T1 (V21:48; ME L.1; V21:1294) |
| N-08 | **RESUELTO** (decae) | CT-PBTC-12 no entra (V21:49 y V21:513) |
| N-09 | **RESUELTO** | Cita `EditorWindowTestSupport.cs:89-95` (V21:470; comprobado: `box.Text = text`) |
| N-10 | **RESUELTO** | El §8.0 resume lo decidido en lenguaje llano. No usa identificadores de tarea («la tarea que separa el estado de la ventana», «la tarea que lo añade»), y dice «la escala de textos y cotas» (V21:1156-1194) |

## 3. La fidelidad a las decisiones del Owner

- **OD-PBTC-1 = (i), con R-03 (i)** ([OPD]:22-27):
  - La regla es la de `Keep` de Cantilever (V21:345 y V21:373), con la extensión declarada: selección, «Ambos lados», alcance, datos del frente y biblioteca (V21:380 y V21:1198). R-03 (ii) queda descartada (V21:390 y V21:1233).
  - **PBTC-C14 se conserva y se extiende a la fila F1** (V21:1093-1100). Queda dentro de la autoridad, porque es una lectura fiel de la pregunta 1 de [OPD]: «Falta medir qué hace hoy con los módulos si el fondo trae decimales… si cambian piezas o el BOM vuelvo a ti». F1 y M-02 son exactamente las dos filas con fondo exacto, donde la ventana reconstruye. Las demás filas tienen el efecto medido en T-3 que el Owner aceptó (V21:483). Aplicar la fórmula a todas las filas pararía siempre en F2, F5, F7 y F11, que ya se sabe que son materiales.
  - La extensión es más conservadora que la letra («piezas o el BOM»): añade las líneas de `Modules` y de `HeaderLineOverrides`, y K4. No deja latitud: el criterio es mecánico.
- **OD-PBTC-2 = (i)** ([OPD]:39-41): aplicada (D-PBTC-02, V21:392-400). Están el borrado del sobre y las rutas de biblioteca e «Insertar».
- **OD-PBTC-3 = el texto corto en L2** ([OPD]:63-71): el `TextBlock` va en `StatusContent`, detrás de `StatusText` (V21:404-408; INV-PBTC-05, V21:907). El literal de V21:412 y de V21:1181 es **idéntico byte a byte** al de [OPD]:66: 141 caracteres, comparados con Python, con «ú», «í», «á», «, » y la NFC iguales. L1 y el texto largo se retiran (V21:422 y V21:1249).
- **OD-PBTC-4 = (A), D-PB-11** ([OPD]:83-89): **incorporado entero**. Están:
  - el diseño (D-PBTC-11, V21:487-509);
  - CT-PBTC-14 (V21:665-675);
  - S-04, con su literal de `Skip` (V21:735-738) y sus aserciones vieja y nueva (V21:745-756);
  - las rutas (V21:830 y V21:835);
  - los filtros (V21:865, :868 y :871-876);
  - INV-PBTC-19 (V21:974-980);
  - la evidencia (V21:1001), las paradas (PBTC-C02, C03 y C10), el registro (V21:792 y :807), el §6.9 y el `Objective` (V21:816).

  No queda ningún «H-PB-28 fuera» (búsqueda en la v2.1: D-PBTC-07, V21:452).
- **OD-PBTC-5 = (B)** ([OPD]:90):
  - el anexo A se retira, y solo queda el §10 (V21:1286-1294);
  - las cajas de H-31 están prohibidas **por la letra** en INV-PBTC-15, en su lista «Fuera» (V21:961-965), y en PBTC-C10 (V21:1046);
  - el registro las lista como «pendiente: decisión del Owner antes de PB-T1» (V21:797-800);
  - la precondición 3 manda anotarlo en `f4b-dispositions.md` (V21:34).
- **¿Más allá o corto?** Nada va más allá de lo elegido. Los tres filtros nuevos de `RequiredTests` son guardas, no pruebas nuevas, así que el coste declarado al Owner («una prueba nueva, sustituir una prueba actual») sigue siendo cierto. Nada se queda corto.

**D-PB-11, atacado:**
- **El diseño por línea es viable.**
  - `ModuleCommit` es público (ES:139) y `StandardRestoreRequested` existe (`RackModuleEditSession.cs:40`). El único que lo pide es `RestoreAllModules_Click` (W:2499).
  - El orden es: «Confirmar» (`CommitModuleEdits` y `RequestRecompute`, W:2462-2463), la pasada síncrona, que consume el reinicio en DA:204 y DA:300-303, y la rama válida. Allí la línea nueva lee la intención antes de W:682 y, después, hace el reinicio y el repintado.
  - `LoadAdvancedRackParameters` repinta bajo `suppressSync`. La casilla se asigna por `IsChecked`, que no dispara su `Click` (WX:507), así que no hay reentrada ni otra pasada. No pedir `RequestRecompute()` es correcto, porque la computación adoptada ya trae el reinicio.
  - En el flujo de «Actualizar» con un reinicio pendiente (W:3346-3349), el resultado es el mismo.
- **CT-PBTC-14:**
  - en la base falla por aserción en el índice 3 («pb-06 A1 controles», `:202`), y la precondición `Assert.True(w.InsertRequested)` pasa;
  - en el GREEN, `Reset` deja los seis en nulo, con `DerivedPostReinforced = true`. `SetOptional(null)` vacía las cajas. `Six` usa `GetRawText`, que da `null` y `true`. Las cuatro líneas nuevas (V21:670-673) son las que salen.
- **S-04:** las cuatro aserciones viejas transcritas (V21:749-753) coinciden literalmente con `ModuleTests.cs:202`, `:203`, `:207` y `:208`. La cobertura es la misma: mismos diseños, mismos pasos y diez líneas.
- **Guardas:**
  - Fact3 exige `RequestStandardRestore()` y prohíbe `forceRebuild:` y `forceRebuild)` en W (`:388-400`), y INV-PBTC-19 (3) lo recoge. El comentario existente de W:2492-2495 usa `<c>forceRebuild</c>`, que no casa con ninguna de las dos formas.
  - No choca ninguna otra prueba:
    - CT-PB0-04 deja `pb-31` con los avanzados ya por defecto, como muestra el recorrido del corpus, y no hay otra pasada que cambie su informe;
    - `PushBackModuleEditorWindowTests.cs:295` solo mira `IsManualOverride`;
    - `PushBackEditorSideOwnershipTests.cs:97-113` mira `ModuleCommit` sin pasar por «Restaurar estándar»;
    - en UI, ninguna prueba llama directamente a `RequestStandardRestore`;
    - las de Core son de Application.
- **Los tres filtros nuevos:** `PushBackAdvancedRackParametersWindowTests` tiene 9 `[Fact]`, `PushBackModuleEditorWindowTests` 13 y `PushBackModuleEditorCharacterizationTests` (Core) 15. Ninguna tiene `Theory`, `InlineData` ni `Skip`, y cada nombre es de una sola clase. Correcto.

**Recuentos y alcance:**
- **§6.9:** 2 421 + 10 − 13 = 2 418; Skipped 17 + 4 = 21; en el RED, 10 Failed y 2 387 Passed; en el GREEN, 2 397 Passed. Cuadra.
- **§6.4:** el filtro con `ExpectRed` en 10, PB-T0 en UI en 16 y el criterio del RED `≥ 10`. Coherentes.
- **`AllowedWriteScope`:** 13 rutas (V21:818-835), con las dos de D-PB-11.
- **`ForbiddenWriteScope`:** 130 = 58 + 55 + 17. La composición de `src/` y de la raíz es la que comprobé en ICT2. Sale `PushBackFullEditorCharacterizationModuleTests.cs` y quedan las otras cuatro clases de PB-T0 (V21:852). Ninguna ruta permitida queda bajo un prefijo. Falta una guarda nueva (N2-02).

## 4. Hallazgos nuevos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| N2-01 | MENOR | **La comprobación temporal del §5.2 no se puede evidenciar con el TRX tal como está escrita.** Pide demostrar que, sin `Skip`, S-03 «falla en diez filas» y F4 «sigue pasando», y que S-04 falla en `:202`, `:203`, `:207` y `:208` y «las otras seis pasan». Pero el mensaje de `Assert.Equal` sobre listas trunca y solo enseña la primera diferencia, con unas pocas posiciones alrededor (H-PB-45, comprobado en ICT2). D-PBTC-10 resuelve esto con `Assert.Fail` para CT-PBTC-10, pero el §5.2 no lo dice para S-03 y S-04. S-01, que es un `Theory` por filas, sí se evidencia | V21:758-764; V21:1014; H-PB-45 (V21:331) | Para S-03 y S-04, escribir que el cambio temporal es también sustituir el `Assert.Equal` final por `Assert.Fail(string.Join("\n", log))`, como en D-PBTC-10, con su TRX como artefacto. O rebajar lo que se exige a «falla por aserción en la primera línea declarada» |
| N2-02 | MENOR | **Falta una guarda en el `ForbiddenWriteScope`.** `PushBackAdvancedRackParametersWindowTests.cs` entra en `RequiredTests` como guarda de D-PB-11, pero no está en la lista prohibida. Con el método de PB-T0 entraría, como entra `PushBackModuleEditorWindowTests.cs`. No abre ningún hueco, porque la lista blanca es exacta | V21:871; V21:854 | Añadirla: serían 131 entradas. Ajustar la cifra del §6.3 y de ME L-7 |
| N2-03 | MENOR | **INV-PBTC-15 deja dudosas las inserciones.** Su lista «Fuera» dice que ningún hunk «toca» `Val` (W:3594), y D-PBTC-01 manda declarar el comparador «junto a `Val`». Un hunk de inserción anclado en W:3594 (`@@ -3594,0 …`) puede leerse como que toca `Val`, aunque es la declaración de un miembro nuevo, que la propia regla permite | V21:342; V21:950; V21:965 | Definir «tocar» como cambiar o quitar una línea del rango del miembro, y escribir que las inserciones entre miembros son declaraciones nuevas. O pedir que el comparador vaya tras `OptionalVal` (W:3626) |
| N2-04 | MENOR | **Colisión de identificadores.** Las sustituciones se llaman S-01 a S-04, y las paradas estándar del mismo contrato se llaman S-02, S-03, S-04… En la transcripción de PD-F4-04, «STOP con S-04» es la parada. Con S-04 dentro, la ambigüedad ya alcanza a tres identificadores, en un contrato que lee un Controller automático. Viene de la v1 y no la señalé entonces | V21:1028; V21:1073; V21:738; V21:913 | Renombrar las sustituciones en el contrato y en el registro (por ejemplo, SUS-01 a SUS-04), o calificarlas siempre como «sustitución S-0n» |
| N2-05 | MENOR | **Dos conjuntos de «diez» en el mismo paquete.** INV-PBTC-03 (2) habla de «diez cajas» con `Keep(`: incluye `BeamDepthBox` y deja fuera `CellBeamLengthOverrideBox`. En cambio, los «diez campos» que se pierden (H-PB-42, OD-PBTC-1) excluyen `BeamDepthBox` e incluyen el largo manual. Las líneas que cita el invariante lo resuelven, pero un lector puede confundirlos | V21:899; V21:328; V21:1201 | Enumerar en INV-PBTC-03 (2) las diez cajas: Depth, Tolerance, PostPeralte, BeamDepth, AnnotationScale, FirstLevelHeight y las cuatro de la celda |

**Lo añadido por el redactor, sin hallazgo:**
- PBTC-C14 extendida a F1 es fiel y está dentro de la autoridad (§3).
- La sustitución de `generalPallet` y los portadores actualizados con `Keep` en cada pasada siguen la forma de Cantilever (`design.X = Keep(Box, design.X)`, `RackCantileverWindow.xaml.cs:333-361`). Son estables: tras una edición, el portador iguala lo que muestra la caja. No dejan latitud: los nombres, el lugar, el orden y las expresiones están fijados (V21:350-361), e INV-PBTC-15 los lista (V21:957).
- H-34 (V21:173-178) es correcto: solo `CellBeamLengthOverrideBox`, de las once, es `IsOptional` (WX:354-355).

**El §8.0 y una muestra de anclas:**
- **§8.0:** es fiel a [OPD], está en lenguaje llano y no usa identificadores de tarea (V21:1156-1194).
- **Anclas nuevas, comprobadas:**
  - W:82, :1555-1579, :1630-1631, :1766-1767 y :2497-2501;
  - ES:139 y :160-161; `RackModuleEditSession.cs:40`; `PushBackAdvancedRackParameters.cs:148-161`;
  - `RackDynamicSystemWindow.xaml.cs:446-464`;
  - `ModuleTests.cs:143-158`, :160-168, :170, :176-193 y :199-208;
  - Fact3 `:381-401` y `:399-400`;
  - `RoundingEffectTests.cs:138`, :164 y :212;
  - `RoundingLossTests.cs:89-93`;
  - `PalletSpecification.cs:25`;
  - `DynamicRackSystemResolver.cs:309`.

## 5. Veredicto

**EMITIBLE. Abiertos: 0 bloqueantes, 0 importantes y 5 menores (0/0/5).**

- La precondición 2 de la v2.1 (V21:26) queda cumplida: ICT2, con N-01 a N-10 resueltos, más esta comprobación, con 0 bloqueantes y 0 importantes sin resolver.
- Recomiendo aplicar en el mismo commit de emisión N2-01 y N2-04, que afectan a la evidencia que se pedirá al Worker y a la lectura del Controller, y N2-02, que es una línea. N2-03 y N2-05 son de redacción.
- Ninguno exige otra comprobación.

**Al Owner no va nada nuevo.** Las cinco decisiones están tomadas y aplicadas tal como constan en [OPD]. Las vueltas que quedan previstas son estas:
- PBTC-C14, si el delta de la base de F1 o de M-02 resulta material;
- el caso de H-31, antes del contrato de PB-T1.
