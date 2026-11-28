# I-64 F4B PB-T1c, intento 2 (la corrección): revisión adversarial independiente del código

**Objeto:** la cadena `833ea6bf..161a0dad` (RED del intento 1 `e5a21989`, preparación `d8ccbcb8`, RED de la corrección `2faae9da`, GREEN `161a0dad`). WorkRunId `R20261119T041505Z-11b9`.

**Cómo se leyó:**
- el código y los documentos, solo por objetos git desde el checkout principal (`git show/diff/log/ls-tree/grep <sha>`);
- la entrega, desde su copia (`rv/`: `worker-handoff.json`, `inv/`, `tests/`, `ci-*`, `red-ci-*`);
- dos guiones Python propios, de solo lectura y escritos con la herramienta de archivos: uno analiza los TRX y otro hace el censo y las comprobaciones sobre el diff.

No se ejecutó dotnet, ninguna prueba ni AutoCAD. No se leyó el worktree.

**Abreviaturas:** W = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`; WC = `….Composite.cs`. Sin otra indicación, las líneas «base» son de `833ea6bf` y las «verde», de `161a0dad`.

## Veredicto

**ACEPTABLE CON MENORES.** Recuento: **B 0 / I 0 / M 5**, y un hallazgo OWNER informativo.

La corrección es exactamente C-1. C-2 se cumple, y el GREEN es D-PBT1-13 (1) a (9) solo en W y WC. Todos los hunks caen en su rango. El censo de declaraciones y las costuras salen como exige el contrato. Ninguna línea añadida muta el rack resuelto ni el informe. La CI que hay en la copia cuadra con la línea base +2/+2.

No he encontrado ningún cambio observable de RACKEDITAR. Los menores son de documentación y de fuerza de las pruebas, y ninguno afecta al producto.

## Tabla de hallazgos

| Id | Sev. | Dónde (sha) | Hallazgo | Consecuencia |
|---|---|---|---|---|
| PBT1CC2-IR-M01 | M | W verde:3207-3210 (`161a0dad`); base W:3323-3327 | En `Restore_Click` queda el comentario «I-42 (A7 / A6V-1…) restaurar actúa sobre el sistema EFECTIVO…». Describía `LoadCompositeFromDesign(design, preserveDormantIntent: true)`, que ya no existe, y ahora queda encima de `LoadFromModel`. D-PBT1-13 (3) mapea W:3320-3327, comentario incluido, a dos líneas (`RestoreEditor` y `UpdateCompositeEnabled();`). | Solo documentación. No incumple INV-PBT1C-03, porque está dentro del rango y el comentario no es nuevo. |
| PBT1CC2-IR-M02 | M | W verde:49-57 | Antes, el bloque `<summary>` del estado compuesto («I-42 — el estado COMPUESTO…») documentaba `composite`. Ahora documenta `editable` (W:56), el miembro que le sigue, y no el reenvío `composite` (W:57), y la frase añadida dice «… y este es su editor». Además, los comentarios de criterio de `ReadInputs` (base W:848-861 y W:883) salen con el código; `BuildInputs` de Application conserva solo una parte. | Solo documentación. El comentario nuevo está dentro de W:49-57, como permite INV-PBT1C-03. |
| PBT1CC2-IR-M03 | M | `tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs:442`, `:464` (`161a0dad`); `assets/catalogs/secciones.csv` | El gesto «PostBox=otro» de CT-02 es inerte con el catálogo entregado. Solo hay una fila `POSTE`, y `Otro` pone `SelectedIndex = 0` en un combo de un elemento que ya está seleccionado. CT-02 tampoco comprueba que cada gesto haya cambiado algo. La fila PostBox del censo del §5.5 no ejerce la escritura `editable.PostCatalogId = PostBox.SelectedId ?? …` (W verde:773). | Hueco de cobertura, no un defecto: por lectura, el código es correcto. La regla vive en CT-PBT1B-03 y CT-PB0-17. Con un catálogo editado de varios postes, ninguna prueba de la ventana fija el traspaso al estado. |
| PBT1CC2-IR-M04 | M | `PushBackWindowOnStateTests.cs:267`, `:285`; `PushBackFullEditorCharacterizationSupport.cs:101-105` | CT-01 deriva sus líneas esperadas de la misma enumeración del directorio (`CorpusNames()`) que las reales, así que no fija por sí misma los 17 diseños. Con el directorio vacío pasaría con dos líneas. | Lo mitigan la guarda del corpus de WLC (§4) y las clases que fijan el corpus. Los «58 de 58» no se ven en un TRX en verde. |
| PBT1CC2-IR-M05 | M | `PushBackWindowOnStateTests.cs:577-587`, `:391-409` | La parte 2 de CT-02 captura las dos referencias tras cada paso del gesto, no «tras cada pasada» (§5.4). Las líneas base que se adoptan y se sustituyen dentro de un paso de varias pasadas nunca se fichan. Ejemplos: la pasada reentrante de `LoadExisting` y las dos pasadas de «Restaurar». | Riesgo práctico bajo. El recálculo neutro vuelve a ejercer la pasada sobre una línea base ya fichada, y no se puede tomar una copia del estado entre dos pasadas de un mismo paso. |

## 0. La corrección

| Comprobación | Resultado | Evidencia |
|---|---|---|
| C-1 exacta | **Sí.** Solo se mueve el bloque `if (!EstaCerrada(w)) Recalcular(w); vigia.Capturar(log, w);` antes de la línea de la parte 1, con un comentario. Las líneas esperadas (`:619-624`) no cambian, ni la parte 2 (`:598`). | `git diff e5a21989 2faae9da -- tests`: un hunk, `PushBackWindowOnStateTests.cs:582-597`, +5/−3 |
| Sin debilitar | El propósito se conserva. Un control que fuera fuente de verdad lo vuelve a leer la pasada neutra, y la copia no lo lleva. La costura inerte sigue sin pasar: en el TRX del RED, todas las líneas son `iguales=False` y `huellas-iguales=False`. | `rv/red-ci-ui/ui.trx`, el mensaje de CT-02 |
| C-2 | **Sí.** `git diff e5a21989 2faae9da -- tests` y `git diff e5a21989 161a0dad -- tests` tocan solo esa clase. CT-01, CT-03 y CT-04 quedan idénticas a `e5a21989`. | diffstat |
| El desfase en la base | **Coherente con el código; no ejecutado.** En la base, `PushBackCompositeEditorAssembler.BuildDesign` arma la secuencia desde `state.SideA`, cuya estructura modular «vive en `WorkingBaseline`». `AcceptComputation` adopta el sistema como línea base «que preservará la SIGUIENTE pasada». Lo que la intención compuesta (hueco, separador, estructura) aplica al resolver entra, por tanto, en el diseño persistido una pasada después. | `PushBackEditorDesignAssembler.cs:25`, `:101-107` y `:198` (`833ea6bf`); `PushBackCompositeEditorAssembler.cs:38-89` |
| Ya existía | Por construcción. Recompute, CA y DA no cambian, y `ReadInputs` da unos inputs iguales campo a campo (§2). El GREEN no puede haber creado el desfase. | `git diff 833ea6bf 161a0dad -- src/RackCad.Application`: vacío |
| ¿Inocuo para «Actualizar», el BOM y la biblioteca? | **Sí.** Las cuatro salidas llaman a `RequestRecompute()`, que corre síncrono fuera de un `Defer`, antes de leer `lastComputation`: la pasada extra es la que reconverge. Pero **«Restaurar» no recalcula antes** (OWNER-01). | base W:3337, :3391, :3465 y :3483; `RecomputeGate.cs:36-45`; W:3309-3328 |

## 1. Alcance y forma

- **El RED.**
  - `833ea6bf..e5a21989 -- src`: una línea, `internal PushBackEditableState EditableStateForTest => new PushBackEditableState();`, en W:168, y las tres clases.
  - `d8ccbcb8` solo toca `docs/`.
  - `2faae9da` solo toca la clase de UI, y su `src` es el de `e5a21989`.
- **El GREEN.** `2faae9da..161a0dad`: solo W y WC.
- **F4B-E25.** Entre `af46fcd5` y `d8ccbcb8`, solo `e5a21989` toca W.
- **INV-PBT1C-03.** Hay 44 hunks en `-U0` y todos caen en su rango.
  - Las retiradas son `-326,49`, `-1289,10` y WC `-811,124`, y solo quitan líneas.
  - Los cuatro métodos solo ganan `SyncName();`: `-1237,0`, `-3352,0`, `-3405,0` y `-3504,0`.
  - Hay dos inserciones entre miembros: `-167,0` (`EditableStateForTest`) y `-1239,0` (`SyncName` y una línea en blanco).
  - Hay un único comentario nuevo, en W:54, dentro de W:49-57.
- **El censo de declaraciones** (heurístico y propio):
  - salen `SideAOnly`, `SelectedDimensionViews` y los dos `LoadCompositeFromDesign`;
  - entran solo `editable`, `SyncName` y `EditableStateForTest`;
  - los doce reenvíos conservan su nombre.

  En valor absoluto, el Worker cuenta 327 → 326 y el paquete 325 → 324: el método de conteo es distinto y el delta (−1) es el mismo.
- **INV-PBT1C-04.** Siguen los siete `SyncName();`, inmediatamente después de cada `Adopt(` o `SetName(`: W verde:278, :302, :318, :1130, :3236, :3290 y :3390. Los literales de T-SG están en miembros que solo ganan `SyncName();`, y `PushBackClassicCharacterizationSourceGuardTests` pasa en la CI del GREEN.
- **INV-PBT1C-05:**
  - una sola `.RequestUpdate(`;
  - ningún token prohibido en las líneas añadidas;
  - `SetNumber(`: 0 añadidas y 0 quitadas;
  - las siembras y `var pallet` y `var options` no cambian (base W:392 y :413 fuera de los hunks);
  - `AdoptCarriers`, como primera sentencia del `try` (W verde:337);
  - `UpdateCompositeEnabled();`, en la línea siguiente en los tres sitios;
  - las cuatro líneas testigo: 1/1/1/1 en la base y 0/0/0/0 en el verde;
  - los literales de I-55 y Fact3 siguen presentes (W verde:3020 y :3109; `forceRebuild` solo en documentación).
- **INV-PBT1C-06.** 39 líneas `internal` en W más 1 en WC en la base. El verde solo añade `EditableStateForTest => editable;`.

## Tabla diseño → código (D-PBT1-13)

| Punto | Código (`161a0dad`) | Conforme | Nota |
|---|---|---|---|
| (1) El estado | W:56 `editable`; W:57 `composite => editable.Editor` | Sí | M02 (la documentación queda sobre `editable`) |
| (2) Los reenvíos | W:84, :101, :106-109, :114, :327, :1144, :1151 y :1155, con el mismo nombre y en el sitio del campo | Sí | Los valores iniciales del estado son los de los campos de la base |
| (3) Las cargas, con el orden de hoy | `LoadNew` W:274 y :285; `LoadDesignForNew` W:297-298; `LoadExisting` W:313-314; `Restore_Click` W:3205-3206 | Sí | Mismo orden que la base, también el de la pasada reentrante de HT1-10b. M01 |
| (4) `AdoptCarriers` primero | W:337. Las asignaciones de base W:391, :393, :398, :401, :403-411, :414, :419 y :423-427 se retiran, con el comentario de W:405 | Sí | Ninguna siembra lee un portador. Los manejadores salen por `suppressSync`. `ShowDimensionViews` reescribe los mismos valores. La pasada reentrante escribe portadores que `AdoptCarriers` sobrescribe todos |
| (5) `ReadInputs` | W:767 (la unidad de W:852); W:772-781 con las expresiones de W:853, :868 (+respaldo), :874-878 y :1295-1297; `return editable.BuildInputs(assembler)` | Sí | `BuildInputs` es igual campo a campo a base W:846-889. La unidad de `generalPallet` solo se lee en W:840 |
| (6) Lo retirado | `SideAOnly`, `SelectedDimensionViews` y los dos `LoadCompositeFromDesign`, con su documentación | Sí | Líneas testigo, 0 |
| (7) El criterio de bloqueo | WC:783-794, que delega en `PushBackEditorOutputGate` | Sí | La puerta es la expresión de la base, literal |
| (8) `SyncName` | W:1133 y siete llamadas | Sí | `Name_Changed` sigue saliendo por `suppressSync` antes de `SetName` |
| (9) La costura | W:168 | Sí | — |

**Equivalencias comprobadas por lectura:**
- **c-02.** `PostBox.SelectedId` es nulo solo si el id pedido no se ofrece. Entonces `ResolvePostCatalogId` da `Defaults.Post`, y DA:169-171, con un valor nulo, da el mismo.
- **c-09.** La salida siempre lleva la unidad de la caja, porque W:767 la escribe antes de `BuildInputs`.
- **La cola.** El aparcado copia los ocho campos (`DynamicRackModuleDesign` no tiene más) y el despertar vuelve a copiar con `DeepCopy`.
- **El orden de las cargas y la excepción de c-13.** Los mismos.
- **A/B.** Sin cambios: `state => composite.Active`.
- **El foco y el +frente.** No se tocan.

No veo ningún cambio observable.

## D2 y su límite

- **Las líneas añadidas.** Ninguna línea añadida de W ni de WC asigna ni muta `lastComputation.System`, `session.System`, `lastCompositeSystem`, la línea base ni el informe de un lado.
  - La única asignación nueva de `.Name` es `editable.Name` en `SyncName`.
  - `lastCompositeSystem` solo se pasa como argumento a la puerta, que hace la misma llamada que la base (regla 3).
  - Las cargas vuelven a apuntar la línea base por el camino de la base.
  - Cumplen INV-PBT1C-07c y 14.
- **CT-PBT1C-04.** Aplica exactamente D-PBT1-15:
  - las secciones de reglas son textualmente idénticas a las de la CT-PBT1B-14 corregida (`PushBackWindowSharedReferenceGuardTests.cs:39-178` = `PushBackSharedReferenceGuardTests.cs:33-176`): las 8 fichas, `Rebind`, las reglas 1, 1b (las tres formas) y 1c, y la regla 2 aplicada a la 1 y a la 1b, no a la 1c, con las mismas exclusiones;
  - las dieciséis autopruebas, (k1) y (k2) incluidas, más la (d), tienen que fallar en W **y** en WC (`:234`), lo que es más estricto que lo exigido.
- **CT-PBT1C-02, parte 2:**
  - la huella profunda es copia literal de la de CT-PBT1B-16 (`PushBackEditableStateCopyTests.cs:1080-1137`), con `Name` de la raíz;
  - las raíces son los no nulos de los cuatro miembros;
  - cubre los gestos del censo y los cinco de D2 (campo en error, cálculo inválido de 100 000, caja escrita sin salir, «Restaurar» y cerrar sin pedir);
  - `Cuenta > 0` evita que pase en vacío;
  - lo único que no es literal es «tras cada pasada» (M05).

## Los CT

- **CT-01:**
  - compara de verdad la ventana con el estado solo, con el comparador de PB-T1b, que incluye `Name`, la unidad y todos los portadores;
  - recorre la ventana nueva y, sobre los 17 diseños, X-01 y F-01, los caminos `LoadExisting`, `LoadDesignForNew` y «Restaurar»;
  - las recetas X-01 y F-01 son las del §5.0;
  - no es tautológica: la ventana recorre controles, `ReadInputs` y la pasada reentrante, y el estado solo no;
  - le falta fijar el tamaño del corpus (M04).
- **CT-02:**
  - el censo del §5.5 está completo, con las seis avanzadas y los módulos, y lleva las huellas;
  - en el verde, la parte 1 comprueba sobre todo la fidelidad de `Copy` y el punto fijo tras la pasada neutra; el efecto en la salida lo fijan las redes existentes, como reparte el §5.5;
  - el gesto de PostBox es inerte (M03).
- **CT-03:** fija lo que pide el §5.4: ningún campo con almacenamiento para los 12 nombres, ni `SideAOnly`, ni `LoadCompositeFromDesign`, ni `ParkDormantTail(`; `IsBlocked(`; `AdoptCarriers` primero; `BuildInputs` y la costura. En el RED falla por las ocho líneas (`campos-con-almacenamiento=12`…).
- **CT-04:** ver D2.
- **El RED** (en la copia, `rv/red-ci-*`):
  - CT-01, CT-02 y CT-03 dan `Failed == Selected` (2 y 1): fallan por `Assert.Equal() Failure`, sin prefijo de tipo, con la primera línea de la pila en `tests/` (`:296`, `:626` y `:97`), sin `EXCEPCION` y sin NotExecuted;
  - CT-04, CT-PBT1B-14 y CT-PBT1B-15 pasan;
  - el filtro UI PushBack da 562: 557 P, 3 NotExecuted previos y 2 F, que son solo los dos métodos nuevos;
  - UI 2 416 P es igual a la base, y Core 19 087 P = 19 086 + CT-04: ninguna prueba existente falla.
- **El GREEN y la estabilidad:**
  - Core da 19 088 P, la línea base +2;
  - UI da 2 439: 2 418 P, 21 NotExecuted y 0 F, la línea base +2;
  - el conjunto de nombres es idéntico entre el RED y el GREEN, y solo cambian de resultado las tres pruebas nuevas;
  - los mismos 21 omitidos;
  - en local, g1 a g3 dan 2/2 y 1/1 tres veces, y h1 a h3 (CT-PBT1A-14) dan 1/1 tres veces;
  - las pruebas usan la cultura invariante (`Sta`), el GUID fijo, el orden ordinal y `[Collection]`, y cierran en `finally`; no veo ninguna dependencia de orden ni de estado estático.
- **Contención:**
  - el producto no escribe a disco y las pruebas nuevas no escriben archivos;
  - no se toca el Plugin ni Application;
  - el perfil: solo cambia la fecha de `RackFrameConfigurator.layout`, con el mismo SHA-256, que el análisis §4 admite.

## Hallazgos OWNER

- **OWNER-01 (informativo; no es de PB-T1c).** El análisis §4 dice que «Actualizar», el BOM y la biblioteca recalculan antes de usar el diseño, y es cierto: W:3337, :3391, :3465 y :3483. **«Restaurar» no lo hace**: lee `lastComputation.Design` sin una pasada previa (base W:3309-3328, igual en el verde).
  - Un «Restaurar» justo después de GapBox, del separador central o de la estructura propia restaura desde el diseño que va una pasada por detrás.
  - Por el mecanismo, la intención compuesta, que va en `design.Composite`, debería volver a ganar en la pasada siguiente, pero ninguna CT lo ejerce sobre un compuesto: el gesto «Restaurar» de CT-02 corre sobre `pb-01-base`.
  - La base y el GREEN se comportan igual, así que no es un cambio sin declarar.
  - Propuesta: añadirlo a la nota del desfase para la revisión de PB-T2.
- **Diferencias observables sin declarar:** ninguna.

## Lo no verificado

- No ejecuté nada. Los resultados vienen de los TRX copiados, no de una corrida propia.
- **El estado de los jobs de la CI.** No lo puedo confirmar:
  - en el RED, que `build-ui` acabara en success y `build-plugin` en skipped: `rv/tests/redci-watch.txt` se corta antes del final;
  - en el GREEN, que los cuatro jobs estuvieran en success;
  - tampoco que los TRX pertenezcan a los runs `44459636816` y `44463212627`.
- **El desfase de una pasada en la base.** Verificado solo por lectura, no medido. Tampoco puedo verificar «58 de 58 ventanas iguales» ni el diagnóstico `t3`, porque los TRX en verde no traen el log.
- **Las corridas locales.** No se pueden atar a un árbol (`TreeSha` null).
- **La prueba de limpieza.** No cubrió `settings.json`; el Worker lo declara.
- **Los avisos.** En los logs copiados, los avisos del Plugin (MSB3277) son iguales en b0, b1 y b2. Los avisos xUnit que cita el Worker no aparecen en esos logs.
- **El handoff en bruto.** Lleva rutas absolutas en `Worktree` y `Command`. Las rutas de evidencia son relativas. El precedente custodiado las sustituye por marcadores; queda para la custodia de la sesión.
- **El comportamiento físico** en AutoCAD es de Smoke-PB.
