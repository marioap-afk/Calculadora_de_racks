# I-64 F4B PB-T3a — Revisión adversarial independiente del código (intento 2, corrección F4B-E42)

**Objeto:** `git diff 485b5651bcec132761347b40ed38b60cfec0db04 6919e17af9fdd5c702d454a97a3c5b6cc6be1739`, con el RED `5dbc9661438112c7be283a79ff5d980e78607067`. WorkRunId `R20261120T025240Z-f006`.
**Método:** solo lectura, por objetos git desde el checkout principal (`git show`, `diff`, `log`, `grep`, `ls-tree`, `rev-parse`), y la entrega copiada en `rv/` (handoff, relevo, TRX, logs e `inv/`). No se leyó, listó ni ejecutó nada en el worktree del Worker. No se ejecutó dotnet ni pruebas. Python solo de lectura, escrito con la herramienta de archivos, para resumir TRX, el handoff y tres fixtures leídos por `git show`.

## 1. Veredicto

**ACEPTABLE CON MENORES.**

**B 0 / I 0 / M 9.**

No hay bloqueantes, importantes ni hallazgos para el Owner. El alcance, el RED, el GREEN, la API cerrada, C-1 y D2 se cumplen. Los nueve menores son de fidelidad de las pruebas a la letra del paquete (receta, tautología, regla «EXCEPCION», momento de una lectura, el literal de RR-3), de cobertura de una guarda y de dos ramas, y una divergencia latente entre el producto y el oráculo prescrito. Ninguno cambia un resultado de hoy.

## 2. Alcance y forma

| Comprobación | Evidencia | Resultado |
|---|---|---|
| La cadena | `5dbc9661` tiene por padre `485b5651`; `6919e17a` tiene por padre `5dbc9661`. Dos commits; ningún «RED repair» | Bien |
| BaseSha..CurrentSha | `git diff --name-status`: 14 rutas, todas `A`, exactamente el `AllowedWriteScope`. Fuera de `src/` y `tests/`: vacío | Bien (INV-PBT3A-01) |
| RedSha..CurrentSha | 7 archivos `M`, todos de `Draft/` (`PushBackFullDraftState.cs` no cambia: el esqueleto ya es el contenedor); `git diff --quiet ... -- tests`: sin cambios | Bien (INV-PBT3A-02) |
| Ningún archivo existente cambia | Todo el diff es `A`; G-9 (`WorkspacePilotUpdateIntegrationGuardTests`) y las demás guardas son las de `485b5651` byte a byte | Bien (INV-PBT3A-07) |
| La base | `git diff 18e4b783 485b5651 -- src tests` y `git diff f273eae1 485b5651 -- src`: vacíos | Bien |
| C-1, el token | `git grep -w ForWrite` en `Draft/`, en `5dbc9661` y en `6919e17a`: vacío | Bien |
| C-1, la firma | `PushBackFullComposition.cs:46` (`5dbc9661`) y `:60` (`6919e17a`): `public static PushBackFullWriteComposition ForFullWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog)` | Bien (D-PBT3-05) |
| G-9 y su autoprueba | `rv/red-ci-core/core.trx` y `rv/ci-core/core.trx`: `G9NoFileOfThePluginOrOfApplicationThatNamesThePilot…` y `G9TripsOnAPilotFileThat…` en `Passed` | Bien |
| El esqueleto exacto (§6.1) | Comparado archivo a archivo en `5dbc9661`: el estado, completo; el comparador con su constructor privado, `Instance`, `ReferenceEquals`, `0` y `new PushBackFullDraftState(new PushBackEditableState(), state.Origin)`; `Pass` → `null` y `LoadExisting` → `new PushBackEditableState()`; `Materialize` → `Failed(UnreadableOrFutureDiagnostic)`; `Read` → `NotAdmitted(MaterializeDiagnostic)` con `BuildStaleBase(scan)`; `ForFullWrite` → `Rejected(NonPilotContentReason)` y `KeepOn` → `current`; `Of` todo `Redraw`; `Classify` vacío. Tipos, enumerados, constructores y propiedades completos | Bien |
| La API cerrada (D-PBT3-02 a 07) | Leída declaración a declaración en `6919e17a`; coincide con la salida de `rv/inv/inv-green-final.txt`. Públicas e internas: exactamente las de los bloques de código, con sus constructores (privados, internos y el público del estado) y los valores de los enumerados. `IsComposed` e `IsAdmitted` son propiedades calculadas de solo lectura, como sus gemelas del piloto (`PushBackPilotMaterializer.cs:37` y `:69` en `485b5651`) | Bien (INV-PBT3A-03, C13) |
| Los privados (INV-PBT3A-13) | Métodos privados (`OfClass`, `KeysOf`, `DocumentsOf`, `ParseObject`, `AddLeaves`, `DiffPaths`, `LeafPaths`, `SameValue`, `GroupOf`, `Covers`) y un `private static readonly string Absent` (`PushBackConflictClassifier.cs:107`). Ningún tipo anidado ni otro campo | Bien |
| Un namespace de bloque, sin términos prohibidos, sin componer sobres | CT-14 en verde en las dos CI; lectura de los ocho archivos | Bien (INV-PBT3A-04, D-PBT3-08) |
| Una llamada | `git grep -c` en `6919e17a`: `PushBackPilotIntentReader.Read(` una vez en el lector (`PushBackFullDraftReader.cs:56`); `PushBackEditableStateSeat.LoadExisting` una vez en el materializador (`PushBackFullDraftMaterializer.cs:70`) | Bien (INV-PBT3A-05) |
| Sin conectar | `git grep -E` de los **17** nombres de tipo nuevos y de `PushBack.Draft` en `src/` fuera de `Draft/`, en `6919e17a`: vacío | Bien (INV-PBT3A-06) |
| TreeSha | `rev-parse <sha>^{tree}`: `aa83f9b2` (base), `8634d69f` (RED) y `0b5ab775` (GREEN), los del handoff | Bien |
| Trailer | RED y GREEN: `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`, igual al `Trailer` del handoff | Bien |

## 3. Tabla de hallazgos

| Id | Sev. | Dónde (`6919e17a`; las pruebas son iguales en `5dbc9661`) | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-01 | M | `tests/RackCad.Tests/PushBackFullDraftCompositionTests.cs:284-296` | **El caso «modulo» de CT-06 no es la receta F-01.** El §5.0 (fila CT-PBT3A-06 de la tabla de gestos) dice «una longitud de módulo de F-01», y F-01 ([PKT1] §5.0) alarga el primer separador de A y después el primero de B, cada uno con `CommitModuleEdits` y `Pasada`. La prueba solo alarga el separador de A (+2) y confirma, sin `Pasada` y sin el de B, sobre `pb-20-ab`. La receta F-01 está en la clase (`F01Design`, `:92-108`), pero solo la usa CT-11. El oráculo (`diseno-igual` frente a `Pass` sobre una copia) se sostiene igual, y el camino de `ForFullWrite` es el mismo para A y para B; lo que no se ejerce es un módulo `B:` compuesto | Registrarlo en el cierre. Si hubiera reparación en otra subtarea, no aplica: es de PB-T3a |
| M-02 | M | `PushBackFullDraftCompositionTests.cs:296` | **`cambio-llego` es tautológico en «modulo»:** `composition.Editor.SideA.ModuleSession != null`, y el getter de `ModuleSession` abre la sesión si es nula (`PushBackEditorState.cs`, `ModuleSession`, en `485b5651`), así que nunca es nula. Lo que prueba que el gesto llegó al diseño es `distinto-del-inicial=True` (`:254`), que sí discrimina | Ninguna; anotarlo |
| M-03 | M | `PushBackFullDraftCompositionTests.cs:87`, `:104`, `:134` (`Pasada` = `PushBackEditableStateSeat.Pass`), `:278` y `:579` (constructor de `PushBackFullDraftState`), `:371` (`PushBackFullDraftStateComparer.Copy`); `PushBackFullDraftLoadTests.cs:676` (constructor) | **La regla «EXCEPCION» (§5.0) no envuelve todas las llamadas al API nuevo.** Las demás pasan por `Paso`. Sin efecto hoy: en el RED el esqueleto no lanza, y en el GREEN `Pass` convierte la excepción del armado en `null`. Si una lanzara, la prueba fallaría por excepción y no por la línea esperada | Ninguna en PB-T3a; que PB-T3b no lo copie |
| M-04 | M | `tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs:42-58` | **CT-12 cubre menos que INV-PBT3A-06.** El invariante habla de «los tipos nuevos»; la guarda busca los cinco nombres del §5.2 con `\b`, así que no ve `PushBackFullDraftStateComparer` (`\bPushBackFullDraftState\b` no casa dentro de él), `PushBackFullDraftMaterializer`, `PushBackEditableStateSeat`, `PushBackFullWriteComposition` ni los demás. Es fiel a la lista cerrada del paquete. Lo comprobé aparte: en `6919e17a`, ningún archivo de `src/` fuera de `Draft/` nombra ninguno de los 17 tipos. Un enganche con `var` y `DraftRegistry.Begin(…, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy)` pasaría la guarda | En la verificación de PB-T3b y PB-T3c, medir INV-PBT3x-06 con los 17 nombres por `git grep`, no solo con CT-12 |
| M-05 | M | `PushBackDraftWriteGuardTests.cs:255` y `:278`; paquete §6.0, tabla de RR-3, fila CT-12 a 14 | **El fragmento de RR-3 ` reglas-sin-coincidencias=True` (con el espacio delante) no aparece en la clase:** la línea es `"reglas-sin-coincidencias=" + …`, sin prefijo. Sin reparación no tiene efecto; con una, la ayuda mecánica contaría 0 = 0 y no protegería ese literal. El Worker lo declaró (`UnexpectedFindings[1]`) | Ninguna en PB-T3a |
| M-06 | M | `src/RackCad.Application/Systems/PushBack/Draft/PushBackConflictClassifier.cs:142` y `:149-155`, con `DiffPaths` `:276-277`; oráculo, `PushBackFullDraftClassifierTests.cs:159-160` | **Divergencia latente entre el producto y el oráculo prescrito en «estructural» de `Fronts`.** Cuando un escalar nulo bajo un frente pasa a contenedor (en `pb-01-base` hay 18 hojas nulas bajo `Fronts`: `BeamLengthOverride`, `DrawPallets` y `PalletsDeepOverrides`), el producto da a la hoja nueva `BaseValue=<ausente>` y `estructural=True`. `PilotKit.JsonDiff` da `null -> …` y el oráculo, `estructural=False`. Ningún caso de CT-09 ni de CT-10 lo ejerce. La letra de D-PBT3-07 («toda entrada en la que algún valor es `<ausente>`») respalda al producto; la intención («un frente que aparece o desaparece») no: un `PalletsDeepOverrides` que aparece no es un frente nuevo. Va en el sentido conservador (marca de más) y el clasificador no está conectado | Arrastre para PB-T4 (presentación, OB-05.3 y OB-05.4) y para el paquete: precisar la regla de «estructural» de `Fronts` y la lectura de `<absent>` del oráculo antes de usarlo con sobrescrituras por frente |
| M-07 | M | `src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftReader.cs:53` | **`Read` no es total con un escaneo nulo:** lanza `ArgumentNullException`, mientras D-PBT3-04 dice «`Read`, total». Es lo mismo que hace el lector del piloto (`PushBackPilotIntentReader.cs:77` en `485b5651`), y el paso 4 no podría construir la base con `BuildStaleBase(null)` | Aceptar; nota del cierre |
| M-08 | M | `PushBackFullDraftCompositionTests.cs:240-256` | **El momento de la lectura «después» en CT-06:** la huella `printAfter` (`:251`) y `current-intacto` (`:255`) se toman tras tres operaciones más (el `BuildDesign` de la composición y dos `Pass` sobre copias), no justo tras `ForFullWrite` (§5.0, «cada línea se arma justo después de su último gesto y antes del siguiente»). Es la dirección conservadora: un falso «sin cambios» no puede salir de ahí | Ninguna |
| M-09 | M | `PushBackFullDraftReader.cs:81-106`; `PushBackFullDraftLoadTests.cs:223-270` | **Las ramas del paso 2 de D-PBT3-04 en las que el lector completo difiere del piloto sin admitir no las ejerce ningún CT.** Son el materializador completo en `OutOfPilot`, en `Failed` ilegible y en otro `Failed`, tras un piloto que admite o que rechaza por compuesto. CT-03 cubre el paso directo (las 14 reglas) y el compuesto admitido. Se alcanzan, por ejemplo, con un diseño compuesto que el resolutor rechaza: el piloto da «fuera del piloto (F5)» y el completo, «no se pudo preparar el borrador» con el motivo. La lista cerrada CT-PBT3A-01 a 14 no lo pide | Llevarlo a la red de PB-T3b o de PB-T4, que usan el lector completo |

## 4. Diseño → código

| Decisión | Código (`6919e17a`) | Lectura |
|---|---|---|
| **D-PBT3-02**, el estado | `PushBackFullDraftState.cs:12-21` | Constructor con `ArgumentNullException` por los dos argumentos; `Editable` y `Origin` de solo lectura. Bien |
| D-PBT3-02, el comparador | `PushBackFullDraftStateComparer.cs:19-39` | `Equals`: `ReferenceEquals`, nulos, `Origin` ordinal y el comparador de PB-T1b. `GetHashCode`: `HashCode.Combine` de lo prescrito. `Copy`: `PushBackEditableStateComparer.Copy` + `Origin`. Bien |
| **D-PBT3-03**, `Pass` | `PushBackEditableStateSeat.cs:20-35` | Sentencia a sentencia igual al ayudante `Pasada` de [PKT1] (`PushBackDraftStatePortTests.cs:68-81` en `485b5651`), que reproduce la adopción de `Recompute` en W (`RackPushBackSystemWindow.xaml.cs:625-669` en `485b5651`: armado en `try`, `AcceptComputation`, `CommitDormantTailConsumption`, la marca de restauración, `ClearModuleCommit`, `RestoreAdvancedRackParameters`; `RefreshModuleSelector` se queda en `_ = ModuleSession`). Excepción del armado → `null`. Bien |
| D-PBT3-03, `LoadExisting` | `PushBackEditableStateSeat.cs:41-54` | Nulo antes de crear nada; estado y dos ensambladores; `LoadNew`, `Pass`, `SeedDefaultSafety`, `Pass`, `LoadDesign`, `Pass`; siempre el lado A. Bien; CT-01 lo iguala con la ventana en los 16 |
| D-PBT3-03, `Materialize` | `PushBackFullDraftMaterializer.cs:48-76` | El orden y los textos del paso 1 al 6, con las constantes del piloto; el compuesto se admite y la cola aparcada no. Línea a línea igual al del piloto (`PushBackPilotMaterializer.cs:126-181` en `485b5651`) hasta la admisión. Bien |
| **D-PBT3-04**, `Read` | `PushBackFullDraftReader.cs:51-117` | Una llamada al lector del piloto; admitido o rechazo exacto por compuesto (diagnóstico y detalle en ordinal) → materialización completa con el texto crudo del origen; las cuatro salidas con sus diagnósticos, detalles y pistas; si no, lo del piloto tal cual; base y clave siempre las del piloto; excepción → `MaterializeDiagnostic` con `BuildStaleBase(scan)`. AUTH-13 decide antes (reglas 10 y 11 del piloto). Bien, salvo M-07 |
| **D-PBT3-05**, `ForFullWrite` | `PushBackFullComposition.cs:60-84` | Nulos (el texto de PM:201) y catálogo; escala; rematerialización y comparación con el comparador completo; copia de `current.Editable` y `Composed(copy.Editor, copy.BuildInputs(…), copy.Name)`; excepción con el prefijo. No compone ni construye un `RackEmbedDocument`. Bien |
| D-PBT3-05, `KeepOn` | `PushBackFullComposition.cs:90-95` | Nulos → `ArgumentNullException`; copia del `Editable` de `current` con el `Origin` de `newInitial`. Pura. Bien |
| **D-PBT3-06**, `Of` | `PushBackViewDisposition.cs:54-81` | Los hechos una vez (cortes de `outgoing` e `IsComposite`); por miembro, `Decode`; sin dirección → `NoAddress`; disponible → `Redraw`; lateral → `ObsoleteLateral`; frontal `PushBackCut` del lado B → `FrontalBOfOneWay`; lo demás → `NoAddress`. El plan ordena por clave y deriva las listas; `AllObsolete` con miembros y todos obsoletos. Bien |
| **D-PBT3-07**, `Classify` | `PushBackConflictClassifier.cs:109-165` | Los tres documentos de cada estado sobre una copia (`DocumentsOf`, `:172-206`): el sobre de `Origin` sin `Design` y con `Editable.Name`; el proyecto sin `PushBack`; el `$.PushBack` del diseño armado y serializado. Diff hoja a hoja con rutas `$.A.B[3].C` e índices; clase por los cambios de c y de d frente a b; grupos por el prefijo más largo, con la fila por defecto por el mapa (`:312-336`); estructural según la tabla; vistas con `Of` sobre el sistema del diseño del borrador (`:161-164`); rechazadas = `FrontalBOfOneWay` ∪ `NoAddress`. Bien, salvo M-06 |
| D-PBT3-08 | Los ocho archivos | Ningún archivo existente de `src/`; ninguno nombra Autodesk, Plugin, UI, `System.Windows`, `Transaction`, `Database`, `async`, `await` ni `Task`. Bien |
| **D-PBT3-12**, D2 | Abajo, §5 | Bien |
| **D-PBT3-13**, AUTH-13 | Lector del piloto sin cambio; CT-04 | Las 28 rutas anidadas y las tres capas, rechazadas por los dos lectores con el texto y la pista literales. Bien |
| **D-PBT3-14** | El diff | No crea vistas; no toca el clásico, la ventana, el anfitrión, el puerto ni el Plugin; no conecta; no abre AUTH-13; la cola sigue fuera. Bien |

## 5. D2 y su límite

**Conclusión: se cumple.** Ninguna copia comparte un objeto mutable fuera de las dos referencias declaradas (la línea base resuelta y el informe de módulos de cada lado). Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado. No hace falta reabrir L-1 (PBT3A-C17).

| Operación | Qué comparte o toca | Evidencia |
|---|---|---|
| `Copy` | Delega en el copiador de PB-T1b; `Origin` es una cadena | `PushBackFullDraftStateComparer.cs:35-39`. CT-05 mide `compartidos-exactos=True` y `cola-comision-sesion … compartidos=0` en cinco escenarios (con F-01 y X-01) |
| `ForFullWrite` | Copia `current.Editable`; devuelve el editor y los portadores de la copia, que comparten con `current` solo las dos referencias de D2; la base se rematerializa aparte | `PushBackFullComposition.cs:71-78`; CT-06 `current-intacto=True huella-igual=True`; CT-11 `ForFullWrite huellas-iguales=True` |
| `KeepOn` | Copia; ni `newInitial` ni `current` cambian | `:90-95`; CT-07 `compartidos-con-current-solo-d2=True compartidos-con-newInitial=0` |
| `Pass` y `LoadExisting` | Sobre un estado propio. `AcceptComputation` sustituye la línea base por `computation.System`, un objeto recién resuelto (`SetWorkingBaseline`): es adopción, no mutación | `PushBackEditableStateSeat.cs:28`; `PushBackEditorDesignAssembler.cs:101-107` en `485b5651` |
| `Of` | Solo lee `outgoing` (`Cortes` e `IsComposite`) | CT-11 le pasa la línea base compartida de `rich` y de X-01: `Of huellas-iguales=True` |
| `Classify` | Arma los tres diseños sobre copias; resuelve un sistema nuevo para el borrador; los nodos del sobre se copian con `DeepClone` | `PushBackConflictClassifier.cs:161`, `:174-175`, `:188` y `:196`; CT-11 `Classify huellas-iguales=True` |
| `Equals` y `GetHashCode` | Solo leen | CT-11 `Equals huellas-iguales=True` |
| Los textos de G-L1 | Ningún token de G-L1 (`WorkingBaseline`, `LastModuleReconciliation`, `computation.System`…) en `Draft/` | CT-13 `reglas-sin-coincidencias=True` en las dos CI, con sus ocho autopruebas en `falla` |

**Para PB-T3b (no es hallazgo):** la composición de `ForFullWrite` entrega un editor que comparte las dos referencias de D2 con `current`. Si el plan llama a `BuildDesign` sobre él, CT-PBT3B-12 tiene que medirlo.

## 6. Los CT

Todas las clases terminan en un `Assert.Equal` de la lista esperada con la lista de líneas, igual que las clases de PB-T1b (`PushBackEditableStateCopyTests.cs`). Las pruebas no cambian entre `5dbc9661` y `6919e17a`.

| CT | Método | ¿Fija lo del §5.2, con su oráculo? | En el RED (TRX de la CI) |
|---|---|---|---|
| 01 (UI) | `PushBackFullDraftWindowParityTests.cs:45-100` | Sí. Los 16 con `estado-igual` y `diseno-igual`, por `EditableStateForTest` y `Persist(LastComputation.Design)`; `pb-21-cola OutOfPilot motivo=<DormantTailOutOfPilotReason>`. Sin gestos, leído tras `LoadExisting`. `Ventana()` sin mostrar, la carga en el `try` y `Cerrar` en el `finally`, dentro de `Substitute`. Añade `descartes=0` | Failed por aserción, `:99` |
| 02 | `PushBackFullDraftLoadTests.cs:126-163` | Sí. Los seis `Failed` con su texto, `pb-21-cola`, `pb-20-ab` (completo `Admitted`, piloto `OutOfPilot`) y c-13 con el prefijo; el texto de la excepción ya lo fijan pruebas existentes | Failed por aserción, `:144` |
| 03 | `:223-270` | Sí. Las 14 reglas, `admitido` y el compuesto, con base, diagnóstico, detalle, pista y clave. Ver M-09 | Failed por aserción, `:269` |
| 04 | `:304-341` | Sí. Las 28 rutas, copiadas iguales a `I58F1Matrix.cs:97-102` (con `Structure("pushback")` = `PushBack.Structure`); las tres capas; la línea literal. **Se comprueba a sí misma:** `control completo=Admitted`. Fija el piloto del control en «fuera del piloto (F5)» (el paquete admitía también `Admitted`): más estricta, y es un hecho, porque `Rich` trae el lado B | Failed por aserción, `:340` |
| 05 | `:653-752` | Sí. Copia igual y distinta instancia, `Origin` cambiado, dos materializaciones iguales (con `GetHashCode`), los compartidos exactos (el recorrido y `IsImmutable` copiados de CT-PBT1B-07 sin cambios) y el `Mutator` ranura a ranura (copiado) | Failed por aserción, `:751` |
| 06 | `CompositionTests.cs:260-327` | Sí en las cuatro escalas, la base cambiada, el origen ilegible y los nulos; los compuestos con `diseno-igual`, `nombre-igual` y `current` sin cambios por el comparador y la huella. Ver M-01, M-02 y M-08 | Failed por aserción, `:310` |
| 07 | `:332-395` | Sí. `KeepOn` y la composición de `DraftSession.KeepMineOnCurrentVersion` en Dirty con `PalletTolerance` 4.5 y `IsDirty` leído antes de la operación (entre medias solo hay una lectura del escaneo, que no toca el borrador). La base nueva se comprueba por referencia con la segunda lectura del mismo escaneo | Failed por aserción, `:386` |
| 08 | `ViewTests.cs:72-142` | Sí. Los ocho momentos de CT-PB0-10 y sus obsoletas `[], [4], [2], [], [], [2], []` (iguales a `PushBackFullEditorCharacterizationLateralCutTests.cs:127-133`); el frontal B de `pb-20-ab` sin B; el lateral con sección −1; el único lateral obsoleto; planta y frontal de A | Failed por aserción, `:141` |
| 09 | `ClassifierTests.cs:210-316` | Sí. Los seis casos con los valores 4.5, 48.5, 3.25 y 4.25 (distintos de los del fixture, medidos: 4, 48, nulo); sus anclas literales con `presente=True`; las 27 líneas de la parte (3), en el orden y con los dueños del paquete. **El oráculo es independiente en el diff y en las reglas:** `PilotKit.JsonDiff` y una tabla de prefijos escrita otra vez y elegida por la longitud. Comparte con el producto, por prescripción, la construcción de los tres textos (ensamblador y almacén) y la fila por defecto (`Ownership.Of`). Ver M-06 | Failed por aserción, `:315` |
| 10 | `:324-375` | Sí. «+ frente» con `<ausente>` en b; el módulo cambiado fuera (`pb-02-modulos`, M2 de 30 a 32: distinto); el lado B; «- frente» con `ViewsToErase=[L3]`, sin rechazadas y `AllViewsObsolete=False`, con una planta entre los miembros para que el `False` discrimine | Failed por aserción, `:374` |
| 11 | `CompositionTests.cs:573-635` | Sí. El estado rico de CT-PBT1B-06 y X-01; la huella de CT-PBT1B-16, copiada igual; las referencias se capturan justo antes de cada operación | Failed por aserción, `:624` |
| 12 | `DraftWriteGuardTests.cs:61-95` | Sí, con la lista del paquete. Las diez autopruebas inyectadas en memoria en WHC y en `WorkspacePilotSupport.cs` saltan. Ver M-04 | Passed |
| 13 | `:250-283` | Sí. Las reglas y las fichas son una copia literal de CT-PBT1B-14 (diff limpio, R2b incluida); las ocho autopruebas del paquete saltan. Ver M-05 | Passed |
| 14 | `:306-334` | Sí. Los términos del §5.2 sobre el texto crudo; las quince autopruebas saltan | Passed |

**Lecciones del §5.0:**
- **Cada línea en su momento:** sí, salvo M-08 (conservadora).
- **Ningún gesto escribe el valor del diseño:** medido en los fixtures por `git show` (`pb-01-base`: tolerancia 4, fondo 48, `PostPeralte` nulo, nombre PB-01, sin `CustomProperties`; `pb-20-ab`: hueco 54; `pb-02-modulos`: M2 = 30).
- **Ningún gesto toca un campo que el ensamblador ajusta:** `BeamDepth` no se edita en la red. Solo se asigna 5.5 en el estado rico de CT-11, que mide huellas y no un efecto en el diseño.
- **Lo que no es literal en las líneas esperadas:** `entradas=<n>` de CT-09 y CT-10 sale del oráculo, como permite el paquete.

## 7. El RED, el GREEN y la estabilidad

**El RED:**
- `rv/red-ci-core/core.trx` (sha256 `7FAF7A66…`, el del relevo): 19 107 resultados, 19 097 `Passed` y 10 `Failed`. Los 10 son exactamente el filtro `RackCad.Tests.PushBackFullDraft` (`Failed == Selected`). Mensaje `Assert.Equal() Failure: Collections differ` y la primera línea de la pila en el método de prueba, bajo `tests/`. Las tres guardas y G-9, en `Passed`.
- `rv/red-ci-ui/ui.trx`: 2 451 resultados, 2 429 `Passed`, 21 omitidos y 1 `Failed`, CT-01, por aserción.
- **Ninguna prueba existente falla.** Lo mismo en los TRX locales del RED (`rv/tests/r1/red-red-1.trx` y `red-red-2.trx`).
- **Sin reparación del RED:** RR-1 a RR-6, la tabla de RR-3 y C18 no aplican.

**El GREEN:**
- `rv/ci-core/core.trx`: 19 107 `Passed`.
- `rv/ci-ui/ui.trx`: 2 451 resultados, 2 430 `Passed` y 21 omitidos.
- Es lo previsto: Core + 13 y UI + 1 sobre la línea base de PB-T2c. Los cuatro jobs en `success` y el RED con `build-plugin` omitido son lo que registra el relevo (no consultado en GitHub: §9).
- En local, tres corridas en verde de los dos filtros con `ExpectRed` (g1 a g3).
- Avisos únicos por código iguales en la base y en el GREEN (`rv/tests/b0/warn-baseline.txt` y `rv/tests/g4/warn-green.txt`).

**Orden, cultura y estado estático:**
- **El producto:** sin estado estático mutable (solo `Instance` y `Absent`). Índices con `CultureInfo.InvariantCulture`; órdenes ordinales.
- **Las pruebas:** usan ensambladores `static readonly` por clase y `PilotKit.Catalog` compartido, el mismo patrón que PB-T1b. GUID fijos (`IntentKit.Rack`, `FixedId`). Algún `int` y algún `bool` se concatenan sin cultura (`post.ToString()` en `ViewTests.cs:106`), sin efecto con enteros no negativos.
- No vi dependencias de orden.

## 8. Contención

- **Sin ventana mostrada, sin modal real y sin `MessageBox`:** ni configurador, ni «Insertar», ni BOM, ni «Guardar en biblioteca». `git grep` de `Show(`, `ShowDialog`, `MessageBox`, `RackFrameConfiguratorWindow`, `File.Write`, `Directory.Create`, `NewGuid`, `DateTime` y `Random` en los 14 archivos: vacío.
- **Lecturas de disco:** las guardas leen `src/` y los fixtures; no escriben. La prueba de limpieza del Worker (antes, tras el RED y tras el GREEN) no ve cambios en el perfil.
- **UI en local:** solo los filtros del contrato, uno a uno, sin filtro de método y sin la suite completa (`TestsExecuted` y `rv/inv/filters.txt`).
- **Fuera del alcance:** no se tocó el Plugin, Domain, la ventana ni AutoCAD. El Release del Plugin se compiló a la carpeta de artefactos de la corrida.

## 9. Hallazgos para el Owner

**Ninguno.** El modelo no está conectado, así que no hay cambio observable en el panel ni en RACKEDITAR. Las diferencias con el piloto que fija están declaradas en el paquete:
- el compuesto admitido;
- el mapeo del lector completo;
- la composición desde `current`;
- el `Origin` de la base nueva en `KeepOn`;
- el clasificador sobre diseños rearmados.

La regla de «estructural» de `Fronts` (M-06) es una precisión del paquete para PB-T4, no una decisión del Owner.

## 10. Lo no verificado

- **No ejecuté dotnet ni pruebas:** los resultados salen de los TRX copiados. Sus sha256 coinciden con los del relevo.
- **No consulté GitHub:** las conclusiones de los jobs de las CI `44666242222` y `44668584248`, y que sus TRX sean de esos SHA, son las que registra el relevo.
- **No leí el worktree del Worker:** los logs, los parches del intento 1 y sus sha256 los tomo de la entrega copiada.
- **La paridad de CT-01 con la ventana:** solo comparé `Pass` con `Pasada` y con la adopción de W en `485b5651`. Que el asiento iguale a la ventana en los 16 lo da CT-01 en verde, no una derivación mía de `LoadFromModel`.
- **La ausencia de cambios en el perfil** es la medición del Worker.
- **Una nota de método:** en un paso ejecuté por error `python -I -c "print(1)"`, que no leyó ni escribió nada. Va contra la regla de no usar `-c`; lo declaro. El resto del Python se escribió con la herramienta de archivos.
