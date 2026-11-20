> **Redacción para el repo público:** la carpeta de worktrees de Codex se sustituye por `<worktrees-de-Codex>`. El resto no cambia.

# I-64 F4B — Revisión independiente adversarial del paquete v1 de PB-T4

> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

**Veredicto: NECESITA v2** B/I/M = **3 / 11 / 15**

## §1. Alcance y método

- **Revisado:** `I-64-f4b-pbt4-gate-contract-package.md` (v1), `pbt4-measurements.md`, `measure_pbt4.py`, `measure_pbt4-out.txt` y `drafting-brief.md`, en `<carpeta-del-paquete>`.
- **Cómo:** el repositorio, solo por objetos git en `7dfc7c21` desde `<checkout-principal>` (`git show`, `git grep`, `git ls-tree`, `git log`). No se leyó ningún archivo del checkout en disco ni el worktree de `<worktrees-de-Codex>`, y ninguna orden lleva esa ruta. Sin compilar, sin pruebas, sin `fetch`, sin órdenes git que escriban.
- **Python:** un solo guion, `ir4_measure.py`, escrito con la herramienta de archivos y ejecutado con `python -I`. Solo llama a `git show|grep|ls-tree` y escribe `ir4_measure-out.txt`.
- **Abreviaturas:** las del paquete (WHC, FVM, PV, W, PP, UR, WCo, [OD], [P], [P2], [P3], DISP). Además: EIH = `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs`; PEG = `…/WorkspacePilotEditGuardTests.cs`; PT1 = `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`.

## §2. El veredicto, en breve

El diseño no es emitible tal como está, por tres razones:

1. **Le falta la pieza que lo hace posible:** el anfitrión no puede obtener la lectura completa (B-01).
2. **La validación única no se puede probar donde el paquete la pone** (B-02).
3. **La pregunta sobre el piloto da al Owner una magnitud falsa** y le deja sin ver la lista que él mismo pidió (B-03).

Además, varias afirmaciones medidas no se sostienen: que K = 1 sale gratis del modal, que los miembros por defecto no tocan las guardas y que PB-T4b no tendrá sustituciones.

En lo de fondo, la fidelidad a [OD] es buena: no hay traducción de estado en el diseño escrito, el modo WORKSPACE no llega al camino clásico y D-PB-05 está bien ordenada. El riesgo de traducción aparece por el hueco de B-01.

## §3. Tabla de hallazgos

| Id | Gravedad | Dónde | Qué | Por qué | Qué propongo |
|---|---|---|---|---|---|
| **B-01** | Bloqueante | D-PBT4-02, 03 y 06; §6.1 `AllowedWriteScope`; CT-PBT4A-01, 06, 07 y 14 | **El anfitrión no recibe la lectura completa, y el paquete no diseña cómo llega.**<br>• `EditIntentReadResult` solo lleva `PilotRead` (`WorkspaceEditIntent.cs:32-55`).<br>• La lectura se hace en el Plugin: `WorkspaceNavigationHost.cs:207-252` → `RackPilotIntentAdapter.cs:120-121` → `PushBackPilotIntentReader.Read`.<br>• `PushBackFullDraftReader.Read(scan, catalog)` necesita el escaneo y el catálogo (`PushBackFullDraftReader.cs:53`), que solo tiene el Plugin.<br>• `Classify` necesita los sobres de los miembros y el catálogo (`PushBackConflictClassifier.cs:109-112`).<br>• `PushBackFullIntentResult` no lleva `PreviewWitness` (`PushBackFullDraftReader.cs:12-41`), y el anfitrión lo usa en `Begin` y en «Conservar» (WHC:344, :691).<br>• A-T3-8: las seis lecturas pasan `request.ChosenReference` (WHC:328, :409, :503, :590, :675, :759), y el origen lo elige el Plugin con `ChooseOriginDefinition(…, chosenReference)` (`WorkspaceEditIntent.cs:682-692`). La firma `ReadIntent(session, rackId, long? chosenReference, witness)` (`IWorkspaceEditIntentReader.cs:20`) no admite la clave del origen.<br>• Ninguno de esos archivos del Plugin, ni `IWorkspaceEditIntentReader.cs`, está en el alcance de PB-T4a.<br>• CT-14 prohíbe que el Plugin nombre los tipos del modelo completo, y la solución natural los nombra.<br>• El kit de pruebas de todo el anfitrión, `EditIntentKit.ReadOf` (EIH:49-52), construye lecturas con `FromPilot`, y los ayudantes no se editan. | Sin esta pieza, D-PBT4-02/03/06 no se pueden implementar dentro del alcance: el Worker para (C01 o C13) o improvisa. Además, la improvisación más corta es rematerializar un estado completo a partir del resultado del piloto, que es justo la «state translation logic» que prohíbe [OD]. «Application, la UI del panel y una línea del Plugin» (§0) es falso. | Diseñar en la v2, con anclas:<br>• el camino de lectura: el Plugin lee el completo sobre el mismo escaneo, y el resultado lleva la lectura completa, el estado del testigo y lo que `Classify` necesita (o el escaneo y el catálogo);<br>• la clave del origen en las relecturas (A-T3-8);<br>• meter esos archivos en el alcance y cambiar CT-14;<br>• decidir qué pasa con `EditIntentKit` (un kit nuevo en un archivo nuevo);<br>• medir las guardas del Plugin que pisa (G-9 con `LockDocument` en `NavigationHostPath`, INV-14, `WorkspaceCommandAdvisorGuardTests`);<br>• llevar al Smoke-PB lo que solo se ve en AutoCAD.<br>Valorar una subtarea previa, solo aditiva, con ese camino. |
| **B-02** | Bloqueante | D-PBT4-04; CT-PBT4A-04; §8.3 | **La regla única vive en el Plugin, y ninguna prueba puede llamarla.**<br>• `RackCad.UI.Tests.csproj:25-27` y `RackCad.Tests.csproj:23-31` no referencian `RackCad.Plugin`.<br>• CT-PBT4A-04 (UI) compara `WorkspacePushBackFieldRule` con la caja: no compila.<br>• La regla del Plugin copia «los parámetros de la caja», así que quedan dos fuentes.<br>• Con Q1-A, `PushBackPilotFieldAdapter` conserva su propia copia de la regla. | Q-PBT2-3 («deja una sola», `owner-pbt2-decisions.md:27-33`) se cumple solo de palabra, y la prueba que la protege no se puede escribir. §8.3 la presenta como «no es decisión». | Poner la regla en `RackCad.UI` (que referencian la UI, sus pruebas y el Plugin), con los parámetros declarados una sola vez y usados por la caja; el Plugin solo la asigna. CT-04 la llama desde la UI. Decir en Q1 que, con (A), la copia del piloto sigue en el código. |
| **B-03** | Bloqueante | §0, D-PBT4-08, R-8, §5.5, §8.2 Q1 y Q3 | **La «cota superior» de 644 no es cota, y el Owner no ve la lista.** Lo que queda fuera de la cota:<br>• `WorkspacePilotPrepareTests` (67 métodos) pasa por el anfitrión real: `new WorkspaceHostRig()`, `Core.OpenDrafts.Find<PushBackPilotDraftState>` y `rig.Port.TryBeginEdit` (`WorkspacePilotPrepareTests.cs:491-517`, :803). El §4 dice que «no pasa por el anfitrión» y que sigue en verde.<br>• La UI: `WorkspacePilotReconciliationTests` tiene 21 métodos con `RequestKeepMine`, 9 de ellos con `ConfirmChoice`, sobre el doble que no se edita. Con `PreviewKeepMine` por defecto en `NotAvailable`, CT-16 obliga a no ofrecer el Sí. Por ejemplo, :780-793 exige `HasChoice` y ninguna operación del puerto. La previsión K_A^UI = 0 es falsa.<br>• `WorkspacePilotReconciliationGuardTests.cs:116`: `Mutate` anclado en `public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);`.<br>• Con B-01, todas las pruebas que alimentan al anfitrión con `EditIntentKit`.<br>Además, A-T3-1 dice «PB-T4 vuelve al Owner con su lista, como D-1» (P3:1861), y OB-T1B.3 manda llevar «la lista exacta» antes del contrato (P:708-710). Q1 pregunta con «como mucho 644», y Q3-(A) deja emitir con una lista medida que el Owner no ve. | El Owner decidiría a ciegas sobre el mayor riesgo del paquete: debilitar en silencio cientos de pruebas. Eso es lo que prohíbe el bloque T1B de [OD]: «Do not silently weaken the old guard». | Medir antes de preguntar: el esqueleto y la referencia sobre una copia exportada. Llevar a Q1 la lista exacta por clase y por causa, con el mapa prueba vieja → equivalente → lo que conserva. Q3 no puede cubrir una lista que el Owner no vio (§8). |
| **I-01** | Importante | D-PBT4-09; CT-PBT4B-03 y 05; R-T4-13 | **K = 1 no sale gratis del modal.**<br>• `SetPendingText`, `CommitField`, `CrossBoundary` y `DiscardRack` autorizan con `Authorize(origin)` y no miran `_modalRackCadActive` (WHC:1122-1180, :253-254). Solo `PassGate` (WHC:1940) y `UpdateGate` (:996) lo miran.<br>• `PilotFieldStatus` no tiene `Waiting` (`IWorkspacePilotEditPort.cs:143-157`), así que CT-05 («`SetPendingText` y `CommitField` dan `Waiting`») no se puede cumplir.<br>• Quién pone el modal es incoherente: D-PBT4-09 y CT-03 dicen el anfitrión; D-PBT4-11 y CT-08 dicen el abridor del Plugin con `EnterPanelModal()`. En Core, con un doble del abridor, nadie lo pondría.<br>• No se dice si `TryEditAll` abre la ventana dentro de la marca `_pilotReadInProgress` (WHC:1018-1031), lo que daría `Busy`. | El Worker tendría que elegir el mecanismo y los estados (latitud), o pararía por C07. En la práctica, el modal WPF deshabilita la paleta, pero esa propiedad solo se puede comprobar en el Smoke-PB. | Fijar un solo mecanismo y su dueño (por ejemplo, el anfitrión pone el modal y el abridor no), los estados esperados por operación (`Busy` en las del campo, `Waiting` en las de intención) y en qué operaciones se añade la comprobación. Medir las guardas que eso pisa. |
| **I-02** | Importante | D-PBT4-06 y 09; §4; §5.3; R-T4-08 | **Los miembros por defecto y el valor nuevo del enumerado rompen guardas medibles hoy:**<br>• PEG:156-163 exige igualdad exacta entre los nombres de `typeof(IWorkspacePilotEditPort).GetMethods()` y las 17 operaciones (son 17, no 16). Un miembro por defecto aparece en `GetMethods()`, así que `PreviewKeepMine` lo rompe ya en el RED de PB-T4a, y `TryEditAll` otra vez en PB-T4b.<br>• PEG:380-385 exige exactamente dos interfaces en WHC, y PEG:280-281 exige `PushBackPilotFieldAdapter` como único campo estático. Quitar el adaptador del piloto rompe la guarda general del anfitrión.<br>• `WorkspacePilotUpdateRequestTests.cs:1107-1109` fija `Enum.GetNames<PilotBoundaryKind>()` con 7 nombres, así que `EditAll` lo rompe. «PB-T4b sin sustituciones previstas» es falso. | Ninguna de esas causas está en (i)-(iii): el diseño produce paradas deterministas (C02/C15) que el paquete deja «a medir». | Medirlas ya y llevarlas a la lista con su causa. Para R-PB-10, valorar reutilizar `CommitField`, que ya es «the same two-phase step as a blocking boundary» (WHC:1149), en lugar de un valor nuevo del enumerado. |
| **I-03** | Importante | D-PBT4-08; PBT4A-C15; §4; §5.2 | **El criterio de la lista no es cerrado ni coherente:**<br>• «si y solo si» las causas (i)-(iii) deja fuera fallos que el propio diseño provoca: G-4 por `ExecuteFull`, CT-PBT3A-12/13 por conectar el modelo, la guarda del adaptador, el resultado de lectura (B-01), los miembros nuevos (I-02) y A-T3-8.<br>• Atribuir la causa de un fallo es un juicio, no un mecanismo.<br>• Saltar una `[Theory]` entera quita también las filas que pasan.<br>• No se exige que el equivalente «cubra todo lo que cubría lo retirado» (OB-T1B.2, P:702-707). Por ejemplo, la guarda de PEG:265-350 lleva más de diez reglas y solo una falla.<br>• Los equivalentes de autopruebas pasan en el RED, pero van a clases con `ExpectRed` true (filtro `PushBackFullPanel`), lo que da C04. | Es el canal por el que se debilita una guarda en silencio. | Un criterio por causas enumeradas, que incluya todos los cambios del diseño. Por cada método, una fila: vieja → equivalente → aserciones que conserva (todas las que no dependen del cambio). Guardas equivalentes en una clase con `ExpectRed` false, fuera del filtro. |
| **I-04** | Importante | §6.0 («GREEN de referencia»); precondición 6 | **Un procedimiento nuevo y no declarado:** la sesión escribe una implementación entera para medir la lista, no la entrega, y fija contra ella la lista cerrada. | F4B-E45 pide compilar el esqueleto y correr la suite, no implementar. Si el GREEN del Worker difiere en un detalle, fallan otras pruebas y salta C15/C16 sin culpa de nadie. | Declararlo como lectura de [DP] o preguntarlo. Custodiarlo para la revisión. Fijar en el contrato lo que la referencia decide (o derivar la lista de una regla mecánica independiente de la implementación). |
| **I-05** | Importante | §8.2 Q1 | **Q1 no es neutral ni completa.** La opción (A) omite:<br>• A-5 y R-T1-17 de PB-T1 («PB-T4 retira la [copia] del piloto», un modelo paralelo interino contra ADR-0029 D12; PT1:1367, :1549);<br>• la «retirada del materializador y del copiador del piloto» de A-T3-1 (P3:1861);<br>• la premisa de D-1, «al quitar el piloto» (`owner-pbt3-d1-decision.md:9-25`);<br>• que deja dos copias de la regla de la escala (Q-PBT2-3).<br>Las otras opciones:<br>• (B), «borrar el código del piloto», no es factible así redactada: el lector completo usa el del piloto para AUTH-13 (`PushBackFullDraftReader.cs:56`), y PP y UR son compartidos («Un solo código»).<br>• (C) subestima su coste: necesita igual la lectura de B-01 y dos tipos de borrador por rack.<br>• «F5» no se explica. | El Owner no ve el coste de la recomendada, y una de las alternativas es irreal. | El texto propuesto del §8. |
| **I-06** | Importante | D-PBT4-14; CT-PBT4A-11 | **D2:** el §2 dice que las condiciones 3, 5, 6 y 7 son de PB-T4, pero CT-11 solo cubre `Begin`, `SetCurrentDraftState`, `KeepOn` y el rebase. Faltan:<br>• la condición 6 (`DiscardAndReload`, WHC:759-);<br>• la condición 7: que comparar `Initial` con `Current` no dependa de cambios dentro de las dos referencias. Una huella igual no lo prueba.<br>La condición 5 queda implícita. | D2 se arrastra desde PB-T1 (L-5, P2:755-758), y aquí se declaran cubiertas condiciones que no se prueban. | Añadir a CT-11 las filas de `DiscardAndReload`, de `SetPendingText` y `CommitField`, y de `IsDirty` tras mutar una copia de las dos referencias, o declarar por qué no aplican. |
| **I-07** | Importante | D-PBT4-14; CT-PBT4A-12 | **CT-12 (`NaN`) prueba el campo equivocado.** `PalletTolerance` se compara con `double.Equals` (`PushBackEditableStateComparer.cs:409`), que da `NaN` igual a `NaN`. M12/A2-13 trata de `RackModuleCommit.SameAs` con `!=` (`RackModuleEditSession.cs:76-136`). | Tal como está escrita, la línea es tautológica (`NaN` ≠ 4.5) o falsa («aunque se vuelva a escribir el valor de la base» deja `IsDirty` en `false`): sería C07. | Poner el `NaN` en una longitud de módulo del commit, la de A-9 en PT1:1553, con la línea de A2-13. |
| **I-08** | Importante | D-PBT4-07; CT-PBT4A-07 y 15; Q2 | **Los literales de la presentación tienen huecos:**<br>• «Vistas: <motivo>.»: `ViewsRejected` son claves de miembros, no motivos (`PushBackConflictClassifier.cs:162-164`, :74).<br>• Sin unión fija entre grupos (Q2 pone «los frentes y el nombre», que además contradice el orden del enumerado, con `Name` primero).<br>• «se borrarán 1 laterales» en singular.<br>• Las líneas esperadas de CT-07 son conjeturas: cambiar los frentes cambia también hojas derivadas de otros grupos (P3:713). | Literales cerrados sin fuente: latitud del Worker o C07. | Fijar la unión, el plural y qué se dice de una vista rechazada (por ejemplo, «Vistas: <n> no se pueden conservar»). Medir las líneas de CT-07 en una corrida antes del contrato. |
| **I-09** | Importante | §6.0 (lista cerrada); D-PBT4-06 y 10 | **La lista de ayudantes no está cerrada:** dice «por ejemplo, `OriginKeyOf` y `PresentKeep`…», y la lección de PBT1C-C13 pide nombres cerrados. Tampoco se fija la disciplina de las dos operaciones nuevas que leen (`PreviewKeepMine` y `TryEditAll`): `PassGate`, la marca de lectura, el vuelo de la actualización (`UpdateInFlightForRequest`), la contención de excepciones y `NotePilotEdit`. Las guardas existentes no las verán, porque recorren listas fijas (PEG:68-91). | Latitud del Worker, y operaciones de lectura nuevas fuera de las guardas de F4: un debilitamiento silencioso por omisión. | La lista nominal completa. Las reglas de las dos operaciones, copiadas de `TryKeepPilotEdit`. El equivalente de la guarda del anfitrión amplía sus listas con ellas. |
| **I-10** | Importante | §5.4 | **Al Smoke-PB le falta lo que solo existe en el Plugin,** porque ninguna prueba lo ejecuta (B-01, B-02):<br>• OB-03.2 en AutoCAD (un A/B se abre en el panel);<br>• la relectura con el origen tras borrar la vista de origen (A-T3-8);<br>• la regla única cableada en el campo;<br>• el abridor cableado;<br>• el paso 3 con un cambio real del dibujo entre la presentación y el Sí;<br>• K = 1 con el modal real (la paleta deshabilitada). | D-PB-09: lo que no se prueba sin AutoCAD tiene que quedar nombrado para el Smoke-PB. | Ampliar la lista del §5.4 y el §10. |
| **I-11** | Importante | §8.2 Q2 | **Q2 empuja hacia (A) y ofrece una opción que no cumple.**<br>• El Owner escribió «fields changed externally / by the user», y (A) muestra grupos, no campos, sin decirlo.<br>• «Solo recuentos» no deja distinguir qué campos cambiaron (D-PB-05).<br>• Falta la intermedia: nombres de campo sin valores.<br>• «El panel mide unos 320 px y no abre diálogos» solo enmarca a favor de (A). Que no abra diálogos es una guarda de F4 sobre `UI/Workspace` (`WorkspacePilotSurfaceGuardTests.cs:341`), no un hecho del producto: el abridor del Plugin de PB-T4b abre una ventana. | Pregunta no neutral, y con una opción irreal. | El texto propuesto del §8. |
| M-01 | Menor | R-2; D-PBT4-06 | «16 operaciones»: son 17 (PEG:44-65). | Dato. | Corregir. |
| M-02 | Menor | §4; R-9 | G-4 no exige «exactamente una vez»: exige que el conjunto de archivos que nombran `PushBackPilotUpdateRun.Execute(` sea solo WHC (IntegrationGuard:497-498). | Dato; el equivalente propuesto dice más que la guarda. | Precisarlo. |
| M-03 | Menor | D-PBT4-10 | `NotePilotEdit(SessionId)` tiene un solo argumento (WHC:832), no `(session, rackId)`. | Firma. | Corregir. |
| M-04 | Menor | D-PBT4-03 | `Precheck` no lee (UR:130-158), y POST ya relee por `Origin` (UR:214-219). A-T3-8 toca las relecturas de un borrador abierto (WHC:409, :503, :590, :675, :759 y la nueva de `PreviewKeepMine`), no `Begin` (:328), donde `ChosenReference` es lo correcto. | Mezcla los sitios. | Reescribirlo con la lista exacta (va con B-01). |
| M-05 | Menor | D-PBT4-11; CT-PBT4B-07 | «Los cuatro archivos del camino» son cinco: el abridor, WHC, FVM, el lanzamiento y el adaptador. | Recuento. | Corregir y fijar la lista en la CT. |
| M-06 | Menor | §5.5, PB-T4b | El RED de PB-T4b: «(base `Passed`) + 7 `Failed`» olvida el +1 de CT-07, que pasa. Con I-02, la base cambia. | Aritmética (F4B-E46). | Rehacerlo. |
| M-07 | Menor | D-PBT4-10; D-PBT4-16 | A-T3-11 pide conservar o retirar `PushBackDraftSessionPort` y `LaunchFor` «con su registro» (P3:1870), y el paquete dice «sin registro». A-T3-9 se saca a `ideas-futuras.md` sin entrada en DISP. | Trazabilidad. | Registrar los dos. |
| M-08 | Menor | §6.0; Q1 | «[PKC2]» no está entre las abreviaturas, y «F5» de Q1-(C) no se explica. | Autosuficiencia. | Definirlos. |
| M-09 | Menor | D-PBT4-11 paso 3 | El abridor clásico muestra con `ShowModalWindow` sin dueño WPF (`PushBackClassicCharacterizationPluginGuardTests.cs:201-206`). «Como el resto de ventanas modales del Plugin» no está medido. | Dato pendiente. | Medirlo y decidir. |
| M-10 | Menor | R-T4-14 | «EditIntent:1174-1184» señala la prueba de la forma del resultado de lectura (EIH:1174-1195), que B-01 rompe. La guarda del cableado está en `WorkspaceEditIntentGuardTests.cs:1150-1190`. | Cita. | Corregir; las dos van a la lista. |
| M-11 | Menor | INV-PBT4A-03; CT-13 | La guarda de «sin traducción» solo mira WHC. | Con B-01, la traducción podría estar en el lector del Plugin, en `WorkspacePilotSupport.cs` o en `WorkspaceEditIntent.cs`. | Extenderla a esos archivos. |
| M-12 | Menor | CT-PBT4B-09; D-PBT4-10 | Faltan detalles:<br>• CT-09 tiene que leer `PendingWork()` y no cerrar la ventana, porque la suite de UI corre en un STA con un `MessageBox` real (`ci.yml:218-221`);<br>• falta la costura para escenificar un módulo sin el configurador;<br>• no se dice qué `ChosenReference` y qué testigo lleva la petición de «Editar todo»;<br>• no se dice qué pasa en los estados Unknown, Error y Undecidable. | Latitud. | Fijarlos. |
| M-13 | Menor | D-PBT4-16 (A-T3-4) | El coste de M15 (A-10: CT-PBT1B-06 tarda 27,7 s) pasa a ser el camino caliente de cada `GetField` y de cada pestaña tras PB-T4a (WHC:1758; `WorkspaceBrowserViewModel.cs:1297`). Dejarlo todo al Smoke-PB lo descubre al final. | Riesgo de rendimiento. | Una medición barata en la corrida de referencia. |
| M-14 | Menor | §6.0 (UNIT_CHANGE); R-PB-05 | `WorkspaceEditIntent.cs`, y con B-01 los lectores del Plugin, faltan en la lista UNIT_CHANGE. `IPushBackFieldRule` e `IPushBackFullEditorHost`, de Push Back, van en `Application/Workspace` para esquivar la guarda de campos de Systems (EIH:1086-1097), frente a R-PB-05 y ADR-0029 D12 (P:732), y no se declara. | Trazabilidad y diseño. | Declararlo. |
| M-15 | Menor | A-T3-4 (F4B-E30) | El desfase de `LastComputation.Design` «cuenta cuando el estado vuelva de la ventana al borrador». Va al Smoke-PB sin decir por qué no lo cubre una CT barata. «Aplicar» manda `editable`, no el diseño calculado. | Arrastre sin motivo explícito. | Una línea en CT-PBT4B-10 tras un gesto compuesto, o el motivo. |

## §4. Los hechos medidos, por muestreo

| Hecho del paquete | Medido en `7dfc7c21` | Resultado |
|---|---|---|
| **«K = 1 por el modal del panel, que ya existe y nadie usa»** | `EnterPanelModal()` existe y no tiene llamadores (WCo:141-145). `SetModalRackCadActive` solo lo llaman las pruebas. `PassGate` (WHC:1940) y `UpdateGate` (:996) devuelven `Waiting`. **Las operaciones del campo y `DiscardRack` no miran el modal** (WHC:1122-1180), y `PilotFieldStatus` no tiene `Waiting` (`IWorkspacePilotEditPort.cs:143-157`) | Existe y nadie lo usa: **cierto**. «Responden `Waiting` a todo el panel»: **falso** (I-01) |
| **La UI del Workspace no puede abrir diálogos** | `WorkspacePilotSurfaceGuardTests.cs:341`: sobre `src/RackCad.UI/Workspace/`, prohíbe `EnterPanelModal`, `ShowModalWindow`, `ShowDialog` y `MessageBox` | **Cierto para esa carpeta.** No impide que el Plugin abra una ventana, como hace el abridor de PB-T4b (I-11) |
| **644 métodos candidatos en 17 clases** | `ir4_measure-out.txt` §1: 644 métodos, 91 de ellos `Theory`. Cuadra con R-8 | **El recuento es exacto, pero no es una cota superior** (B-03). Fuera de las 17: `WorkspacePilotPrepareTests` (67, pasa por el anfitrión); `WorkspacePilotReconciliationTests` de UI (21 con `RequestKeepMine`); `WorkspacePilotReconciliationGuardTests:116`. En total, 26 archivos fuera de las 17 nombran lo que PB-T4a cambia (739 métodos; §2 de la salida) |
| **Las guardas que no pueden seguir en verde** | CT-PBT3A-12 (`PushBackDraftWriteGuardTests.cs:61-94`: WHC nombraría `PushBackFullDraftState`); CT-PBT3B-13 (`PushBackWritePathGuardTests.cs:235-296`: `Find<PushBackFullDraftState>` y `ExecuteFull`); G-4 (IntegrationGuard:497-498: `runCallers` vacío); `WorkspaceEditIntentGuardTests:711` | **Las cuatro, ciertas.** **Faltan:**<br>• PEG:156-163 y :380-385;<br>• `WorkspaceEditIntentGuardTests:733-737` (seis `var pilot = read.PilotRead;` y seis `ReadIntent`, que suben a siete con `PreviewKeepMine`);<br>• EIH:1174-1195;<br>• `WorkspacePilotUpdateRequestTests:1107-1109`;<br>• `WorkspacePilotReconciliationGuardTests:116` |
| **Los miembros por defecto de `IWorkspacePilotEditPort`** | PEG:161 compara `typeof(IWorkspacePilotEditPort).GetMethods()` con la lista exacta. Un miembro por defecto aparece ahí | **Rompen la guarda,** y se puede medir sin compilar (I-02). El doble de la UI no se toca, pero el comportamiento de la UI sí cambia (B-03) |
| **OB-03.2** | P3:402, :530, :958 y :1862 (A-T3-2), y el lector completo admite A/B (`PushBackFullDraftReader.cs:56-71`) | **Cierto:** PB-T3 no lo quitó, y es de PB-T4 |
| **El literal de `ExpectedEvidence[0]`** | `ir4_measure-out.txt` §5: igual byte a byte al segundo literal de `F4B_EXACT_EVIDENCES` y al de PB-T3c | **Cierto** |
| **nc1 y nc2 con `--task PBT4`** | nc1: `TASKS` incluye `PBT4` (`nc1_cascade_signal_substitute.py:41`), y el prefijo `F4B-PBT4-` se liga en :204-206. nc2 v2: `TASKS` (:14) y `check_task` (:410-415). La decisión de nc2 v2 admite reutilizar el literal de otro paquete si es el mismo («un paquete nuevo con otra redacción exige añadir su literal», `owner-pbt3a-nc2-decision.md`) | **Cierto.** Un matiz: `PBT4` liga a las dos subtareas por prefijo (M-V1 de nc2), y no las distingue entre sí. Basta declararlo |

## §5. Fidelidad al Owner

- **[OD], «Do not make PB-T4 invent state translation logic»:**
  - el diseño escrito no traduce: el borrador nace del lector completo, y el puerto de la ventana conserva `Origin` (A-T3-11);
  - **el riesgo está en B-01:** con el hueco del camino de lectura, la salida más corta es materializar un `PushBackFullDraftState` desde `PushBackPilotIntentResult.StaleBase`, y eso sería traducción;
  - la v2 tiene que cerrar el camino y extender la guarda de INV-PBT4A-03 (M-11).
- **D-PB-01:**
  - es modal, con el borrador dentro y fuera, y cero escrituras: el abridor no hace nada tras cerrar;
  - WORKSPACE nunca llega a lo clásico: la guarda de OB-01.3 tiene su autoprueba;
  - «the drawing cannot be edited while the modal editor is open» depende del modal real: va al Smoke-PB, pero sin mecanismo fijo en Core (I-01, I-10).
- **D-PB-05 y OB-05.4:** el orden 1 → 2 → 3, el testigo y «sin Sí no se escribe» están bien. Dos problemas:
  - «Por grupos» no es «fields» (I-11);
  - el texto de las vistas rechazadas no tiene fuente (I-08).
- **D2:** el límite literal va en C17, pero faltan las condiciones 6 y 7 (I-06).
- **AUTH-13 («Basta así»):** se mantiene, porque el lector completo pasa primero por el del piloto.
- **La nota del Owner sobre la interfaz:** solo un botón, un texto y una pregunta. Está bien. La nota también admite que la presentación no sea perfecta («los arreglamos después»), así que Q2 debe ceñirse a la funcionalidad.
- **El bloque T1B («Do not silently weaken the old guard») y OB-T1B.2:** es el punto más débil del paquete (B-03, I-03, I-09).

## §6. La división y el orden

- **¿Es necesaria?** Sí. «Editar todo» depende del borrador completo, y cambiar el tipo del borrador arrastra la mayor parte de las sustituciones.
- **¿Cabe cada una en un Worker sin latitud?**
  - **PB-T4a: no,** tal como está:
    - le falta el camino de lectura (B-01);
    - su lista es de tamaño desconocido y probablemente de cientos de métodos, con un equivalente escrito por el Worker para cada uno;
    - los equivalentes no están especificados;
    - las CT de UI no compilan (B-02).
  - **PB-T4b:** cabe, una vez arreglados I-01 y I-02 y medidas sus propias sustituciones.
- **Un camino más pequeño:**
  - no veo uno que cumpla [OD] y evite cambiar el tipo del borrador: (C) sigue necesitando la lectura completa y dos tipos de borrador por rack;
  - sí se puede encoger PB-T4a con una subtarea previa, solo aditiva: el lector del Plugin lee también el modelo completo, y la relectura admite el origen, sin cambiar todavía el tipo del borrador;
  - en PB-T4b, reutilizar `CommitField` como frontera, en lugar de `PilotBoundaryKind.EditAll`, evita una sustitución (I-02).
- **El orden:** PB-T4a sobre el cierre de PB-T3c, y PB-T4b sobre el de PB-T4a, es correcto. Que «Conservar» no se separe del cambio de tipo (OB-05.2) está bien razonado.

## §7. Las sustituciones con registro

- **El criterio no está cerrado ni se puede verificar antes de emitir** (I-03). Depende de una implementación de referencia que no se entrega (I-04).
- **Riesgo de debilitar en silencio:**
  - las guardas con muchas reglas: por ejemplo, la del anfitrión (PEG:265-350) se saltaría entera por una sola regla;
  - las `[Theory]` saltadas enteras;
  - las operaciones nuevas fuera de las listas fijas de las guardas (I-09).
- **Las guardas de D-1 y E-3, E-5 y E-6:**
  - con Q1-(A), siguen en verde, porque `Execute` y `ExecuteFull` existen y el piloto conserva `ViewUnavailablePrefix` (P3:1228-1236);
  - pero el Owner eligió D-1 con la premisa «al quitar el piloto», y A-T3-1 (P3:1861) y A-5 (PT1:1549) asignan a PB-T4 la retirada de la copia del piloto. Conservarlo es una decisión que Q1 tiene que mostrar con su coste (I-05);
  - con (B), la sustitución de E-3, E-5 y E-6 está bien prevista, pero «borrar el código del piloto» tiene que nombrar qué se borra: el lector del piloto se queda, porque el lector completo lo usa.

## §8. Las preguntas al Owner

**Q1, el piloto: se mantiene, reformulada, y solo después de medir la lista** (B-03, I-05). Texto propuesto:

> PB-T4 pasa el borrador del panel de Push Back al modelo completo de PB-T3, sin convertir borradores, como pediste. Al hacerlo, <N> pruebas existentes dejan de describir el panel: <n1> de comportamiento, <n2> guardas de F4 y de PB-T3, y <n3> de la interfaz del panel. Se midieron sobre una implementación de referencia, y la lista va en el paquete. Cada una se sustituye con registro por otra que conserva lo que protegía, como en D-1. Queda además el código del piloto, que el panel ya no usará: la copia de la carga (que PB-T1 dejó como paralela e interina hasta PB-T4), su comparador, su adaptador de la escala (con su propia copia de la regla, que pediste dejar en una sola), y su Prepare y su Execute. El lector del piloto se queda en los dos casos, porque el completo lo usa para el rechazo AUTH-13. ¿Qué hace PB-T4 con ese código?

- **«Retirarlo»** (o la que la sesión recomiende, en primer lugar):
  - se retira lo que solo usaba el panel;
  - se sustituyen además sus <n4> pruebas propias y tres equivalentes de D-1 (E-3, E-5 y E-6);
  - queda una sola regla de la escala y ningún modelo paralelo.
- **«Conservarlo sin uso»:**
  - no se toca, y sus pruebas siguen en verde;
  - quedan, anotados en ideas futuras, el modelo paralelo de PB-T1 y la segunda copia de la regla de la escala.
- **«Dos modelos hasta F5-A»:**
  - el campo de escala del panel sigue con el piloto;
  - «Editar todo» descarta o exige actualizar el borrador del piloto antes de abrir uno completo;
  - el panel tiene dos códigos de borrador y dos «Conservar mi borrador», y necesita igual la lectura completa.

**Q2, la presentación: se mantiene, reformulada** (I-11). Texto propuesto:

> Antes de aceptar «Conservar mi borrador», pediste mostrar lo bastante para distinguir los campos cambiados fuera, los cambiados por el usuario, los cambiados en los dos sitios, los cambios estructurales (módulos, frentes, lado B) y lo que pasa con las vistas, sin un diff JSON. El clasificador de PB-T3 da cada valor cambiado con su grupo (nombre, rack, anotaciones, seguridad, frentes, módulos, lado B, parte compuesta, fuera del editor). ¿Qué se muestra encima del Sí/No del panel?

- **«Por grupos»:**
  - una línea por clase con los grupos afectados, por ejemplo «Cambiado fuera: el nombre y los frentes.», más una línea de lo estructural y otra de las vistas;
  - dice qué zonas cambiaron, no qué campo.
- **«Por campos»:** lo mismo, con el nombre en español de cada campo cambiado. Es más largo y necesita una tabla de nombres.
- **«Por campos, con sus valores»:** además, el valor de antes, el de fuera y el tuyo.

Hay que quitar «Solo recuentos», porque no deja distinguir qué campos, y quitar «no abre diálogos» del enunciado. Si la sesión prefiere no preguntar, la opción que cumple la letra de D-PB-05 con el menor cambio es «Por campos», declarada como lectura.

**Q3, la emisión: se mantiene, reformulada** (B-03). Texto propuesto:

> PB-T4 va en <k> subtareas: <lista>. Ninguna autorización tuya cubre PB-T4. La lista de pruebas sustituidas de PB-T4a va en Q1. ¿Emito cada subtarea sin volver a ti si su comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición sobre el cierre anterior no cambia nada material y la lista medida al emitir es la que viste, sin ninguna prueba añadida?

- **«Sí, con esas condiciones»:** vuelve a ti por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba fuera de la lista.
- **«Solo la primera; las demás me vuelven».**
- **«No; cada una me vuelve con su lista».**

**Las que faltan:** ninguna decisión de producto nueva, si Q1 lleva la lista y sus costes. Dos cosas que la sesión decide pero tiene que declarar en el paquete, sin preguntarlas:
- que pedir «Conservar» pasará a leer el dibujo, frente a la regla de F4 de que la elección «calls nothing of the port but the memory» (UI :780-793);
- que el GREEN de referencia es un procedimiento nuevo (I-04).

**Las que sobran:** ninguna, si Q2 se reformula. Si no, Q2 se puede quitar con la lectura «Por campos».

## §9. Los arrastres

| Arrastre | Clasificación del paquete | Juicio |
|---|---|---|
| A-T2b-OWNER-01 | PB-T4b, D-PBT4-12 | **Bien.** Ninguna prueba fija hoy la pregunta con las dos cosas pendientes |
| Q-PBT2-3, la validación única | PB-T4a | **Mal diseñada** (B-02). Con Q1-(A) no se cumple «deja una sola» |
| D-1 y E-3, E-5 y E-6 | Con (A) siguen; con (B) se sustituyen | **Bien citado,** pero falta la premisa de D-1 y A-5 (I-05) |
| A-2 y L-5 (D2) | CT-11 y C17 | **Incompleto** (I-06) |
| OWNER-01 de «Restaurar» | Fuera; Smoke-PB | **Bien** |
| PBT3B-IR-M01 y OWNER-2 | Fuera; Smoke-PB | **Bien.** La fuente dice «sin OWNER-2 hasta el cierre de PB-T3c»: hay que remedirlo |
| El layout del configurador | Regla del §5.0 y C10 | **Bien.** La ventana de Push Back no escribe el perfil: no hay `LayoutStore` en `UI/Systems/PushBack` |
| A-T3-9 | Fuera, a `ideas-futuras.md` | **Aceptable,** con registro en DISP (M-07) |
| A-T3-10 | Fuera con (A) | **Bien,** condicionado a Q1 |
| A-T3-11 | PB-T4b | **Bien,** salvo el «sin registro» (M-07) |
| A-5 y R-T1-17 de PB-T1 | **No aparecen** | **Falta** (I-05) |

## §10. El contrato propuesto, frente a las lecciones de F4B

| Lección | PB-T4a | PB-T4b |
|---|---|---|
| La lista cerrada de miembros y ayudantes | Abierta («por ejemplo»; I-09) | Abierta |
| La CI del RED, con `build-plugin` omitido | Bien | Bien |
| El esqueleto compilado y la suite completa de Core antes de emitir | Prevista. Faltan la UI de CT-04 (B-02) y lo que ya se puede medir sin compilar (I-02) | Prevista |
| El total exacto de cada rama (F4B-E46) | Fórmula con K_A, D_A y E_A. K_A^UI no es 0 (B-03). `(13 + E_A) Failed` supone que todo equivalente falla en el RED (I-03) | Aritmética errónea (M-06) y sustituciones no previstas (I-02) |
| El tamaño (F4B-E21) | Sin estimar, con una lista que puede ser de cientos | Bien |
| Nada en el esqueleto que lean las guardas | El esqueleto añade un miembro al puerto que PEG:161 lee por reflexión (I-02) | Lo mismo con `TryEditAll` y `EditAll` |
| UNIT_CHANGE | Incompleta (M-14) | Bien, con W |
| La regla RR | Bien | Bien |
| nc1 y nc2 con `--task PBT4` | Bien (§4) | Bien |
| Las pruebas de UI con ventanas reales o que escriben el perfil | Bien; CT-04 crea la de Push Back | CT-09: leer `PendingWork()` y no cerrar (M-12) |

## §11. Las CT

- **Discriminan bien:** CT-PBT4A-01, 02, 03, 05, 08, 09, 10, 13 y 15; CT-PBT4B-01, 04, 06, 07 (guarda, con autoprueba), 08, 10 y 11.
- **No se pueden cumplir, o están mal fijadas:**
  - CT-PBT4A-04 no compila (B-02);
  - CT-PBT4A-06 no se puede probar en Core como está descrita, porque el origen lo elige el Plugin (B-01, M-04);
  - CT-PBT4A-07 tiene líneas conjeturadas (I-08);
  - CT-PBT4A-11 es incompleta (I-06);
  - CT-PBT4A-12 es tautológica o falsa (I-07);
  - CT-PBT4A-14 contradice la solución de B-01;
  - CT-PBT4A-16 y el doble viejo: B-03;
  - CT-PBT4B-03 y 05 son incoherentes con el mecanismo y con `PilotFieldStatus` (I-01).
- **Las lecturas en el momento justo:** la regla del §5.0 es correcta. CT-11 tiene que tomar la huella después de cada operación, y no solo al final.
- **Lo que no se prueba sin AutoCAD:** la lista del §5.4, ampliada con lo de I-10.

## §12. Límites de esta revisión

- Nada se compiló ni se ejecutó. Las roturas de I-02 y B-03 se deducen del texto de las guardas y de las pruebas en `7dfc7c21`. Que el diseño las provoque es seguro para PEG:161, `WorkspacePilotUpdateRequestTests:1109` y `WorkspacePilotReconciliationGuardTests:116`. Para la UI se deduce de CT-16 tal como está redactada.
- Sobre el GREEN de PB-T3c solo hay lo que dice DISP. No se midió lo que cambia ese cierre.
- La cifra de 739 métodos fuera de las 17 clases (`ir4_measure-out.txt` §2) cuenta los archivos que nombran algo que PB-T4a cambia. No es una previsión de fallos, sino una prueba de que 644 no acota.

## §13. Comprobación acotada de la v2

**Veredicto: NECESITA v3** B/I/M = **2 / 7 / 8**

**Qué se revisó:**
- `I-64-f4b-pbt4-gate-contract-package-v2.md`, con su tabla V2-01 a V2-29;
- R-13 a R-20 de `pbt4-measurements.md`;
- `measure_pbt4_v2.py` y su salida;
- `v2-instructions.md`.

**Cómo:** con las mismas reglas que en la v1, es decir, solo objetos git en `7dfc7c21`, nada del worktree, sin compilar y sin Python nuevo; todo se leyó con `git show` y `git grep`. Lo marcado como **nuevo** no existía en la v1.

### §13.1 Los 29 cambios, uno a uno

| V2 | Hallazgo | Estado | Nota |
|---|---|---|---|
| V2-01 | B-01 | **Resuelto en lo esencial** | El camino de la lectura está diseñado: el lector completo trabaja sobre el mismo `input` (RIA:121), `FromReads` deja intacto `FromPilot`, el kit es nuevo, y la relectura con el origen va en una interfaz aparte. Así no toca EIG:1048-1055, que fija el archivo de `IWorkspaceEditIntentReader`. No hay traducción. Quedan dos problemas: PB-T4a rompe más de lo que dice (I-12) y la clave del origen después de POST no tiene fuente (I-13) |
| V2-02 | B-02 | **Resuelto** | `AnnotationScaleRule` vive en `RackCad.UI` y la UI y sus pruebas pueden llamarla. El campo `Func<string, DraftFieldValidation>` respeta EIH:1088 y EIH:1102. Pero el `x:Static` rompe una prueba de F4 de la UI que se puede medir hoy (I-14) |
| V2-03 | B-03, I-03, I-04 | **No resuelto** | El orden es correcto: preguntar después de medir. Pero el procedimiento recomendado no cierra la lista (B-04) |
| V2-04 | I-01 | **Resuelto, con una fila mal** | Pone el modal el abridor del Plugin, y eso respeta la guarda de F4 (PEG:778-781). La marca se mantiene durante `ShowModal`. La fila de `RequestUpdate` está mal (I-15) |
| V2-05 | I-02 | **Resuelto en el anfitrión; roto en el Plugin** | `CommitField` como frontera mantiene URT:1107-1109 en verde, y «Conservar» va por `TryKeepMine`. Pero `TryEditAll`, con cuerpo por defecto, no llega al anfitrión en el producto (B-05). El abridor nuevo, además, rompe dos guardas de «nueve archivos» (I-14) |
| V2-06 | I-05 | **Resuelto** | Hay un matiz menor (M-21) |
| V2-07 | I-06 | **Parcial** | La línea de la condición 7 no discrimina (I-16) |
| V2-08 | I-07 | **Parcial** | La línea con `NaN` sigue ambigua (M-19) |
| V2-09 | I-08 | **Resuelto** | Para «Por campos», ver I-17 |
| V2-10 | I-09 | **Parcial** | La lista nominal no incluye el reenvío de `PilotEditAdapter` (B-05) ni la conversión de la clave (M-16) |
| V2-11 | I-10 | **Resuelto** | |
| V2-12 | I-11 | **Opciones bien; recomendación mal justificada** | Ver I-17 |
| V2-13 a V2-27 | M-01 a M-15 | **Resueltos** | Comprobados por muestreo: PEG:44-65; IG:497-498; WHC:832; PushBackClassicCharacterizationPluginGuardTests:196-209; EIH:1175-1194 y EIG:1150-1190 |
| V2-28 | Lo declarado | **Resuelto** | |
| V2-29 | Las tres subtareas | **La división se sostiene** | Pero PB-T4a no es solo aditiva (M-23) |

### §13.2 Hallazgos nuevos

| Id | Gravedad | Dónde | Qué | Por qué | Qué propongo |
|---|---|---|---|---|---|
| **B-04** | Bloqueante | D-PBT4-11, opción 1; §4.2; Q1 | **Las siete causas cerradas no recogen el fallo más común de PB-T4b, y C-5 no se puede medir sobre su esqueleto.**<br>• El lector doble de todas las pruebas del anfitrión, `EditIntentFakeReader`, responde por defecto `EditIntentKit.Admitted`, es decir, `FromPilot` (EIH:17-52). Lo usan 9 clases, con 428 métodos (EIH, PilotEditHost, ReconciliationHost, UpdateHost, UpdateRequest, LifecycleHost, AssociativeBookkeeping, CommandAdvisor y `WorkspacePilotPrepareTests`).<br>• Con D-PBT4-04, sin `FullRead` no nace ningún borrador (`FullReadMissingReason`). Toda prueba que empiece un borrador así falla, aunque no nombre ningún tipo del piloto.<br>• La firma de C-1 exige nombrar `PushBackPilotDraftState` o su comparador y un fallo `NotNull`, `NullReferenceException` o igualdad sobre ese estado. No recoge un `Assert.Equal(EditIntentStatus.Admitted, …)` que falla.<br>• Tampoco se define «su kit» de forma mecánica.<br>• C-5 se mide «sobre el esqueleto», pero el esqueleto de PB-T4b deja WHC sin cambio. Las guardas de texto leen el WHC, la FVM y el W.xaml del GREEN, que no existen antes de emitir. Ejemplos: EIG:670 y :720, EIG:733-735, G-4, PEG:280 y los demás de I-14. | La lista que vería el Owner se quedaría corta por cientos de métodos. La vuelta de C15/C16 a mitad de PB-T4b es casi segura. La lista no cumple (a) ni la condición «la lista que viste» de Q3. | Ver §13.4: una causa C-0 con su firma mecánica; el kit definido por archivo, no de forma transitiva; C-5 medida sobre el texto literal del GREEN que el contrato entrega; equivalentes mecánicos. Además, una opción 5 para el Owner |
| **B-05** | Bloqueante | D-PBT4-13; §6.0, la lista de PB-T4c; §6.3 | **«Editar todo» no llega al anfitrión en AutoCAD.**<br>• El panel recibe `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth))` (WCo:52).<br>• `PilotEditAdapter` (WCo:283-332) reenvía solo las 17 operaciones.<br>• Con `TryEditAll` con cuerpo por defecto, la llamada de la FVM llega al adaptador, que no la sobrescribe, y devuelve `EditAllOutcome.NotAvailable`.<br>• La lista cerrada solo admite la implementación en `WorkspacePilotEditGate`. Añadir el reenvío sería C13.<br>• Ninguna prueba lo ve, porque el Plugin no se ejecuta en las pruebas y la UI usa un doble.<br>• La guarda del adaptador (PEG:686-694) recorre las 17 operaciones como cota inferior: no lo exige ni lo prohíbe. | La función queda muerta en el producto con todo en verde, y solo el Smoke-PB lo descubriría, al final. | Añadir a la lista y al alcance de PB-T4c `PilotEditAdapter.TryEditAll => _gate.TryEditAll(request)`. Que el equivalente de PEG:156-163 copie también `ControllerViolations` (PEG:683-694) con 18 operaciones, para que la falta del reenvío falle. Que CT-PBT4C-08 lo compruebe en texto |
| **I-12** | Importante | D-PBT4-03; §4.1; §5.1; §6.1 (Objective) | **PB-T4a no rompe solo dos pruebas.** Al pasar las cinco relecturas al ayudante `ReadForOpenDraft`, sus cuerpos dejan de contener `_editIntentReader!.ReadIntent(`. Eso rompe además:<br>• EIG:563-575 (el cuerpo de «comprobar ahora»: el orden y un `_editIntentReader` por cuerpo);<br>• EIG:758-770 (los tres cuerpos de la reconciliación);<br>• `WorkspaceLifecycleGuardTests:642-656` (el cuerpo de la recarga). | El Objective dice «Solo se sustituyen … CT-PBT3A-12 y la cuenta de ReadIntent de EIG:736», y los totales del §5.1 están mal. El resultado sería C16 en el intento 1. | Poner la elección **en línea** en cada cuerpo: `var read = _originKeys.TryGetValue(…, out var key) && _editIntentReader is IWorkspaceEditIntentOriginReader o ? o.ReadIntentAtOrigin(…) : _editIntentReader!.ReadIntent(…);`. Así siguen los seis `.ReadIntent(`, `_editIntentReader\s*[.!]` sigue en 1 por cuerpo y se mantiene el orden. Solo rompería CT-PBT3A-12. Si se mantiene el ayudante, hay que listar las cinco pruebas |
| **I-13** | Importante | D-PBT4-03; CT-PBT4A-03 | **La clave del origen después de POST no tiene fuente.**<br>• `DraftApplyOutcome` expone `PostRead` (`DraftIntentRead`) sin clave (`Drafts/DraftApplyOutcome.cs:56-67`).<br>• `PostOriginKey` vive en el plan (PP:322) y solo se usa dentro de UR (:603).<br>• Sacarla de `PostRead.StaleBase.Entries`, comparando con el `Origin`, en el cuerpo de la actualización rompe EIG:1124, `UpdateNeverReads` (`\.StaleBase\b|\.Entries\b|\.InitialDraftState\b`). | Es el caso central de A-T3-8: POST borra la vista de origen. Sin la clave nueva, las relecturas apuntarían a una definición borrada y dirían «falta el origen», peor que el `Primary` de hoy. | Fijar la fuente: exponer la clave de POST en el resultado de la corrida (pero UR y `Drafts/` no están en el alcance), o resolver el origen por el texto de `Origin` en el Plugin, como hace `ChooseOrigin` (UR:640-642), con una regla para cuando no coincide. Hasta entonces, CT-PBT4A-03 no se puede escribir sin latitud |
| **I-14** | Importante | §4.2; §4.3; D-PBT4-06 y 16 | **Roturas por letra que ya se pueden medir y no están en las tablas:**<br>• EIG:670 y EIG:720: «Conservar» en WHC con `.StaleBase`, `.Entries`, `.InitialState` o `.InitialDraftState`, fuera de los consumidores admitidos (`Classify(draft.InitialDraftState, …, FullRead.StaleBase …)`);<br>• `WorkspacePilotPrepareTests.cs:1351-1352`: `Assert.Equal(11, intent.Length)` sobre `EditIntentStatus`; `Presented` lo sube a 12;<br>• `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs:80-97`: fija `Minimum="0"` y `MinimumInclusive="False"` en el texto del XAML, y el `x:Static` lo rompe;<br>• EIG:1011-1022 y `WorkspaceNavigationHostTests.cs:714-723`: `src/RackCad.Plugin/Workspace/` tiene exactamente 9 archivos, y `WorkspaceFullEditorHost.cs` sería el décimo. | Son C-5 deterministas, y la opción 1 no los ve (B-04). | Evitarlas por diseño:<br>• la clasificación, en una función pura de `Draft/` que reciba `draft` y `read.FullRead`, de modo que WHC no nombre la base;<br>• el abridor, fuera de `Plugin/Workspace/` (por ejemplo, `Plugin/Views/`);<br>• el XAML con sus literales, y CT-PBT4B-04 comparando la regla con las propiedades de la caja real.<br>Si no se evitan, a la tabla con su causa |
| **I-15** | Importante | D-PBT4-14; CT-PBT4C-05 | **La fila de `RequestUpdate` está mal.** Mira la marca antes que nada y da `EditUpdateStart.Busy(PilotReadInProgressReason)` (WHC:1388). Además pasa por `PassGate`, no por `UpdateGate`, que es la puerta de `TryUpdatePilot`. | CT-05 compara la tabla literal: daría C07. | `RequestUpdate` → `Busy` (la marca) |
| **I-16** | Importante | D-PBT4-18; CT-PBT4B-10 | **La condición 7 no discrimina.** Sustituir la referencia por otra «del mismo contenido» deja `IsDirty` en `false` también con un comparador que compare por contenido. Además, la letra de las condiciones 5 a 7 ya está en `owner-d1-d2-decisions.md` (el bloque «L-1 SAFETY CONDITIONS»), así que ese `<a medir>` sobra. | La línea pasaría igual si el comparador dependiera de esas referencias. | Sustituirla por un objeto de **contenido distinto** y esperar `isdirty=False`, o declarar por qué no aplica |
| **I-17** | Importante | §8.2 Q2; D-PBT4-09 | **La recomendación «Por campos» no está bien justificada.**<br>• La tabla de nombres sale de `PushBackEditableStateOwnership.Table`, que solo tiene rutas de primer nivel, unas 25 (`$.Fronts`, `$.Structure`, `$.SideB`, `$.Composite`…; `PushBackEditableStateOwnership.cs:133-175`). Con eso, «Por campos» casi coincide con «Por grupos». Y «el módulo 3: longitud» necesita una tabla de hojas anidadas que no está especificada (latitud y tamaño).<br>• El clasificador informa también de las hojas derivadas (P3:713), así que «Por campos» listaría campos que nadie editó, en 320 px.<br>• «Con el menor cambio» no es cierto: es la forma más cara. | La recomendación empuja con un argumento falso. | Llevar los costes reales a Q2, o recomendar con ellos a la vista (§13.5) |
| **I-18** | Importante | §8.2 | **El procedimiento no tiene pregunta propia.** Si la sesión elige la opción 3, la 4 o la 5 de §13.4, eso no cabe como «condición» de Q3. | Son decisiones del Owner, cada una con opciones. | Una Q4 (§13.5) |
| M-16 | Menor | D-PBT4-03 | NH tiene que convertir la clave (el texto hexadecimal que escribe RIA con `ToHandle(…).ToString()`) al `long` que recibe `RackPilotIntentAdapter.Read`, o RIA necesita una sobrecarga. No está en la lista cerrada | Latitud | Fijarlo |
| M-17 | Menor | CT-PBT4A-06 | El censo admite «desde ya» WHC, y además NH y WEI, que no nombran los cinco tipos | Afloja la guarda antes de hacer falta | En PB-T4a, admitir solo RIA; WHC, en PB-T4b |
| M-18 | Menor | D-PBT4-03 | «Se borra con el borrador»: un borrador desaparece por varios caminos (`DiscardRack`, `DiscardDocument`, el cierre, la retirada de la sesión, `DiscardAndReload`) | Latitud | Nombrarlos, o tolerar claves huérfanas en la consulta |
| M-19 | Menor | CT-PBT4B-11 | Un `NaN` no puede entrar en `Initial` desde un fixture JSON, y `IsDirty` compara `Initial` con `Current` | La línea es ambigua | Fijar la secuencia, o comprobar `Comparer.Equals(s, Copy(s)) == false` |
| M-20 | Menor | D-PBT4-08 | El testigo solo cubre el sobre del origen. Si otro miembro cambia entre la presentación y el «Sí», lo presentado puede quedar viejo, aunque lo que se conserva sí use la relectura | Límite sin declarar | Declararlo |
| M-21 | Menor | Q1 | «Cumple la letra de A-5» exagera: la copia sigue dentro del lector (D-PBT4-10 lo dice). Y `<n6>` de la opción (C) exige diseñar (C) para poder medirlo | Precisión | Decir «retira lo que se puede retirar», y dar (C) sin número |
| M-22 | Menor | §9 | Varios `<a medir>` se pueden cerrar ya (§13.6) | | |
| M-23 | Menor | D-PBT4-01 y 03 | PB-T4a cambia el comportamiento: las relecturas pasan del miembro `Primary` (`ChooseOriginDefinition` con `chosenReference` nulo, WEI:686) al origen de `Begin` | Es correcto por A-T3-8, pero no es «solo aditivo» | Declararlo como cambio observable |

### §13.3 PB-T4a

- **¿Es aditiva?** En las pruebas, casi: los dobles existentes siguen por el camino de reserva. En el producto, no: las relecturas cambian de miembro (M-23).
- **¿Rompe solo CT-PBT3A-12 y EIG:736?** No. Rompe además EIG:563-575, EIG:758-770 y `WorkspaceLifecycleGuardTests:642-656` (I-12). Con la elección en línea, solo rompería CT-PBT3A-12.
- **Lo que comprobé que sigue en verde:**
  - EIG:414-430 (un `LoadCatalog` y un `Read` en NH);
  - EIG:441-470: `class PilotIntentReader : IWorkspaceEditIntentReader` sigue casando con la segunda interfaz añadida, y `read.Pilot` sigue apareciendo una sola vez;
  - G-9 (los términos de RIA y NH no cambian);
  - `WorkspaceCommandAdvisorGuardTests:406-450` (la tabla de motivos solo exige que estén los textos en español);
  - EIH:1102-1121 (el diccionario no es de `Systems`);
  - EIH:1129-1134 (los estados como prefijo ordenado).
- **¿Algo es «traducción de estado»?** No. El estado completo sale del lector completo de PB-T3 sobre el mismo escaneo, y el testigo, del lector del piloto sobre ese mismo escaneo. Con la v2 desaparece el riesgo que señalaba B-01.

### §13.4 El procedimiento de la lista: mi juicio

**Ninguna de las cuatro opciones cumple, tal como está escrita, las tres condiciones a la vez.**

| Opción | (a) Cerrado antes de emitir | (b) Sin implementación oculta | (c) Sin debilitar |
|---|---|---|---|
| 1. Por causas, en dos tiempos | **No:** falta la causa dominante, «su kit» no es mecánico y C-5 no se mide sobre el esqueleto (B-04) | Sí | Depende de los equivalentes (ver abajo) |
| 2. Esqueleto tipado ejecutable | **No:** los cuerpos inertes hacen fallar lo que el GREEN aprobaría, y no ven las guardas de texto del GREEN | Sí | Igual que la 1 |
| 3. Referencia custodiada | Sí | **No** | Sí |
| 4. Referencia entregada | Sí | Sí, pero invierte los papeles | Sí |

**La opción 1 se puede arreglar, y es la mejor base:**
- **Una causa C-0, «la lectura viene del kit del piloto»,** con una firma mecánica: la clase construye `EditIntentFakeReader` o `PilotEditAuditedReader`, o usa `EditIntentKit` o `FromPilot`, y el método empieza un borrador o pasa por un `Rig` que lo empieza.
- **«Su kit»:** el texto del mismo archivo, más una lista cerrada de archivos de kit con nombre. Nada transitivo.
- **C-5 sobre texto entregado:**
  - el contrato fija literalmente las líneas del GREEN que leen las guardas, y ya fija muchas (`var full = read.FullRead;`, el `Begin`, la línea de `KeepOn`);
  - la sesión aplica ese texto a una copia exportada **solo para ejecutar las guardas de texto**;
  - como el Worker recibe esas mismas líneas, (b) se cumple.
- **Equivalentes mecánicos:**
  - un mapa cerrado de sustituciones de texto (`EditIntentKit` → el kit nuevo, `EditIntentFakeReader` → el doble nuevo, `Find<PushBackPilotDraftState>` → `Find<PushBackFullDraftState>`, el comparador…);
  - un guion que compara cada original con su equivalente, módulo el mapa;
  - solo C-2, C-3 y C-5 llevan equivalentes escritos a mano, con su tabla de aserciones.

**¿Debilita algo saltar pruebas que no fallarían?** No, si el equivalente es una copia mecánica: conserva todas sus aserciones, ahora sobre el modelo completo. Sí, si es una adaptación a mano. Y en los dos casos infla el número que ve el Owner. Con el arreglo de arriba no debilita. Hay que decirle al Owner que `<N>` es una selección por texto.

**¿La 3 y la 4 son decisiones del Owner?** Sí:
- la 3 es un procedimiento que no está en [DP] (F4B-E03), con la sesión escribiendo producto;
- la 4 cambia los papeles de la sesión y del Worker.

**Hay una quinta opción, también del Owner, y es la que mejor cumple (a), (b) y (c):**
- se edita en PB-T4b la única línea del kit compartido, `EditIntentKit.ReadOf` (EIH:51-52), para que use `FromReads(PushBackPilotIntentReader.Read(scan, PilotKit.Catalog), PushBackFullDraftReader.Read(scan, PilotKit.Catalog), PilotKit.Catalog)`;
- las 9 clases (428 métodos) siguen ejecutándose, con sus mismas aserciones, sobre el camino real del producto;
- se saltan y se sustituyen solo las que fallan por C-1 a C-7;
- la lista es pequeña y se mide sin implementación;
- no quedan cientos de originales saltados y duplicados.

Va contra C02 y contra el «como D-1» de A-T3-1, por eso es del Owner, con su registro declarado. Si no la quiere, queda la opción 1 arreglada.

### §13.5 Las preguntas al Owner

- **Q1:** neutral y autosuficiente, y con los costes de las dos opciones de retirada. Hay que añadir dos cosas:
  - con la opción 1, que `<N>` es una selección por texto (puede incluir pruebas que no fallarían) y que es probable volver a mitad de PB-T4b;
  - si se elige la opción 5, el texto cambia: «se edita la lectura del kit compartido; se sustituyen solo <n>».
  
  También hay que corregir M-21.
- **Q2:** las opciones están bien, pero la recomendación no (I-17). Propongo que cada opción lleve su coste:
  - «Por campos»: necesita una tabla de nombres para cada hoja anidada del diseño y enseña también los valores derivados (por ejemplo, posiciones que cambian al añadir un frente);
  - «Por grupos»: 9 literales; dice qué zonas cambiaron.
  
  «Por campos» es más fiel a la letra («fields»). «Por grupos» es más legible y más barata. Si se recomienda «Por campos», hay que decir esos costes y cerrar la tabla de nombres en el paquete.
- **Q3:** está bien con la condición «la lista que viste». Si se queda la opción 1 sin arreglar, que diga claramente que es probable volver a mitad de PB-T4b.
- **Falta una Q4,** el procedimiento, si la sesión no se queda con la opción 1 arreglada. Texto propuesto:

  > Para pasar el panel al modelo completo, las pruebas del panel de F4 empiezan sus borradores con una lectura del piloto de un kit compartido. Cambiar el tipo del borrador las hace fallar: unas <n> en 9 clases. ¿Cómo las trato?
  
  - **«Editar una línea del kit»:** la lectura del kit pasa a traer también el modelo completo. Las pruebas siguen con sus mismas aserciones, y solo se sustituyen con registro las <m> que fallan por el cambio.
  - **«Saltar y duplicar, como D-1»:** cada una se salta y se copia con el kit nuevo. Son <n> copias, con equivalentes mecánicos y un guion que comprueba que solo cambia el kit.
  - **«Una implementación de referencia»:** la sesión escribe una para medir, se custodia, y el Worker la recibe (o no) como objetivo.
- **Sobra:** ninguna. Serían cuatro, el máximo del encargo.
- **Decisiones nuevas para el Owner:** la Q4, y la opción 5 si la sesión la propone. Nada más.

### §13.6 Los `<a medir>` que quedan

Se pueden medir ya, y algunos cambian el diseño:

| `<a medir>` | Medido ahora | Efecto |
|---|---|---|
| `WorkspaceCommandAdvisorGuardTests` frente a RIA y NH | La tabla de motivos solo exige los textos en español en los tres cuerpos (:406-450); `ex.Message` sigue dos veces en `Read` (:458-460) | **Verde,** si la rama del origen no añade texto en inglés |
| EIG:1150-1190, el cableado | Exige un `SetPilotUpdateHost` después del lector y antes del `return` | **Verde** con las asignaciones nuevas después |
| CT-PBT2B-08, frente al literal y `PendingWork()` | R10 cuenta cada uno de los once literales (`PushBackWorkspaceModeSourceGuardTests.cs:271-274`), y ninguna regla mira `PendingWork` | **Verde** |
| Las guardas sobre el `x:Static` | No es una guarda de T1B: es `WorkspaceDraftPilotValidationParityTests.cs:80-97`, de F4 y de UI | **Rompe** (I-14) |
| La costura para escenificar un módulo | `HeaderConfiguratorPresenter` y `ConfigureModuleHeaderButton` (`PushBackHeaderAuthorityWindowTests.cs:99-106`), sin configurador real | **Existe** |
| La letra de las condiciones 5 a 7 de D2 | `owner-d1-d2-decisions.md`, «L-1 SAFETY CONDITIONS» | Cambia la línea de la condición 7 (I-16) |
| Los nombres de `EditIntentStatus` | `WorkspacePilotPrepareTests.cs:1351-1352` (exactamente 11) | **Rompe** (I-14) |
| Un miembro de interfaz con cuerpo por defecto | PEG:161 (ya en la lista), y el reenvío del Plugin | B-05 |

Solo se pueden medir con la copia exportada:
- la lista, con K, D y E;
- las líneas de CT-PBT4B-06 (no dependen de PB-T3c: el clasificador es de PB-T3a, ya cerrada);
- el coste de `IsDirty`.

### §13.7 Límites

- Nada se compiló. Las roturas nuevas se deducen del texto de las guardas en `7dfc7c21` y del diseño de la v2.
- La cifra de 428 métodos es la población de las 9 clases que usan el lector doble, no un recuento de fallos.

## §14. Recomprobación de la v3

**Veredicto: NECESITA v4** B/I/M = **2 / 3 / 4**

**Qué se revisó:**
- `I-64-f4b-pbt4-gate-contract-package-v3.md` (V3-01 a V3-16);
- R-21 a R-27 de `pbt4-measurements.md`;
- `measure_pbt4_v3.py` y su salida;
- `v3-instructions.md`.

**Cómo:** por objetos git en `7f0d9aa3`, sin tocar el worktree y sin compilar. Usé un guion nuevo, `ir4_measure_v3.py` (con `-I`, solo `git show|grep|ls-tree|diff`), cuya salida es `ir4_measure_v3-out.txt`.

**Lo medido en `7dfc7c21` sigue valiendo.** Entre `7dfc7c21` y `7f0d9aa3`, `src/` y `tests/` solo cambian en cuatro archivos: `RackPilotUpdateWriter.cs`, `RackViewBlockEraser.cs` y dos guardas nuevas de PB-T3c (`PushBackViewEraseGuardTests` y `PushBackWritePathPluginGuardTests`), que solo miran el escritor y la fachada. PB-T4 no toca nada de eso. La base de la CI coincide con la prevista: Core 19 129 = 19 123 + 6 y UI 2 451 = 2 430 + 21 (DISP, F4B-E48).

**Una corrección a mi §13.4.** Allí dije que, editando una línea del kit, las 428 «siguen ejecutándose con sus mismas aserciones» y que «la lista es pequeña». Era falso, porque no miré los ayudantes de cada clase: están tipados sobre el borrador del piloto (B-06). Lo que la v3 recomienda en Q4 viene de ese error mío.

### §14.1 Estado de V3-01 a V3-16

| V3 | Hallazgo | Estado |
|---|---|---|
| V3-01 | B-05 | **Resuelto en los reenvíos:** la compuerta (`WorkspacePilotSupport.cs:153-201`, con el mismo patrón `_depth.IsInside ? … : _inner.…`) y el adaptador (WCo:283-327), con CT-12 y CT-13. No hay más envoltorios: en `src/` solo implementan el puerto WHC, la compuerta y `PilotEditAdapter`. Lo que **no** se sostiene es el abridor anidado en WCo (B-07), y CT-10 no se puede compilar tal como está (I-20) |
| V3-02 | B-04, I-18 | **Parcial:** C-0, la Q4 y C-5 sobre el texto literal están bien planteados. Los números y las firmas no (B-06) |
| V3-03 | I-12 | **Resuelto.** Con la elección en línea, comprobé que siguen en verde EIG:563-575, EIG:758-770, EIG:733-736 y `WorkspaceLifecycleGuardTests:642-656` (recuentos, orden y un `_editIntentReader` seguido de `.` o `!` por cuerpo); `ReadIntentAtOrigin(` no casa con `\.ReadIntent\s*\(`. También siguen en verde EIG:441-470, EIG:414-430, G-9, `WorkspaceCommandAdvisorGuardTests:406-460` y `WorkspaceBridgeSourceGuardTests:364-367`, porque RIA puede usar `PushBackOriginByText` sin nombrar `RackCad.Application.Workspace`. **PB-T4a solo rompe CT-PBT3A-12** |
| V3-04 | I-13 | **Resuelto y correcto.** Tras un `Applied`, `CompleteApply` pone `Current = Copy(PostRead.InitialState)` (`Drafts/DraftSession.cs:592-600`, :692-695). Así `Current.Origin` es el texto del miembro `PostOriginKey`, el reanclado, y la relectura siguiente lo encuentra byte a byte. Si no coincide o hay varios, cae en el `Primary`, como hoy. **No es traducción de estado:** solo elige qué miembro releer y no construye ningún estado |
| V3-05 | I-14 | **Parcial.** El XAML sin `x:Static` está bien, y la clasificación en `Draft/` evita nombrar la base. Pero el método de EIG:664-690 cuenta `pilot.Read` por cuerpo (`Assert.Equal(1, Src.Count(intent, @"\bpilot\.Read\b"))`, :677, y :686-688). Con `var full = read.FullRead;` falla igualmente, así que no está «verde por diseño» (I-19). Además, el abridor anidado choca con otra guarda (B-07) |
| V3-06 | I-15 | Resuelto (WHC:1388, :1392) |
| V3-07 | I-16 | **Resuelto:** la línea discrimina, y el comparador no compara la base ni el informe (`PushBackEditableStateComparer.cs:20-27`). Falta nombrar cómo se sustituye la referencia (M-25) |
| V3-08 | I-17 | Resuelto. Hay un matiz en la cifra (M-26) |
| V3-09 a V3-16 | M-16 a M-23 | Resueltos. Queda texto desfasado (M-24) |

### §14.2 Hallazgos nuevos

| Id | Gravedad | Dónde | Qué | Por qué | Qué propongo |
|---|---|---|---|---|---|
| **B-06** | Bloqueante | D-PBT4-11; §8.2 Q4 y Q1 | **Con la opción 1 no fallarían 44 pruebas, sino unas 256 de las 428, y las firmas no lo distinguen.**<br>• Los ayudantes de las clases están tipados sobre el piloto: `Open(…)` devuelve `DraftSession<PushBackPilotDraftState>` con `Find<PushBackPilotDraftState>(…)!` (`WorkspacePilotUpdateRequestTests.cs:63-68`); también `Pilot(…)` (`WorkspaceLifecycleHostTests.cs:90-97`), `Draft(…)` (EIH:97-98), `DraftOf`, `Held`, etc.<br>• Con el kit editado, el borrador nace completo, `Find<PushBackPilotDraftState>` da null y cae toda prueba que use esos ayudantes, aunque su cuerpo no nombre nada del piloto.<br>• Contando el texto del método y los ayudantes de su archivo (un nivel), **256 de los 428 métodos** tienen alguna señal (`ir4_measure_v3-out.txt` §2): 235 por el tipo, a través de ayudantes. El «44» de la v3 solo cuenta el cuerpo del método.<br>• Faltan dos causas:<br>  – **«el campo sin regla»:** las pruebas de Core no pueden asignar `AnnotationScaleRule`, que vive en la UI, y D-PBT4-06 dice que sin regla el campo responde «No editable aquí». Afecta a unos 70 métodos con `SetPendingText`, `CommitField` o una frontera bloqueante, 43 de ellos en `WorkspacePilotEditHostTests`;<br>  – **las lecturas `FromPilot(` o `PrepareKit.Reread(` fuera del kit**, que la edición de una línea no arregla: 9 en `WorkspacePilotReconciliationHostTests`, EIH:770 y varias en `WorkspacePilotPrepareTests` (§3).<br>• C-0 a C-7 dicen «texto de la prueba, de su archivo…». Leído así, el archivo entero cumple la firma y entran las 428 en cualquier opción. | La Q4 se recomienda por «solo unas <m> fallarían» con m = 44, y dice que es la «única» opción sin debilitar. Con los números reales, la opción 1 ahorra unas 170 copias, no 384. El Owner decidiría con datos falsos. | Firmas por **método más los ayudantes de su mismo archivo, un nivel, por nombre**: mecánico y sin nada transitivo. Añadir las causas C-8 (el campo sin regla) y C-9 (lecturas del piloto fuera del kit), o evitar C-8 por diseño (B-07). Rehacer los números de Q4 y Q1. Valorar una opción 1′: editar también los ayudantes de las 9 clases, con una lista cerrada de líneas. Habría que medir antes cuántos cuerpos usan miembros del estado del piloto, como `PrepareKit.SetScale(draft, …)` |
| **B-07** | Bloqueante | D-PBT4-06 (el cableado de la regla, PB-T4b); D-PBT4-15 y 16 (el abridor anidado, PB-T4c) | **`WorkspaceController.cs` no puede nombrar `RackCad.UI`.** `WorkspaceHostSourceGuardTests.cs:398-420` (`NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType`) prohíbe `System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|…` en todo archivo de `src/RackCad.Plugin/Workspace/` y de `src/RackCad.Application/Workspace/`, salvo la paleta y el puente, por archivo exacto. Por eso:<br>• `_host.SetFieldRule(AnnotationScaleRule.Validate)` en WCo necesita `RackCad.UI.Controls`;<br>• el `FullEditorHost` anidado en WCo necesita `RackPushBackSystemWindow` (`RackCad.UI.Systems.PushBack`).<br>Las alternativas de la v3 también chocan:<br>• un archivo nuevo en `Plugin/Workspace/` rompe las guardas de 9 archivos;<br>• un archivo en `Plugin/Views/` que implemente una interfaz de `Application.Workspace` rompe `WorkspaceBridgeSourceGuardTests:364-367`. | Los dos cableados del Plugin de la v3 fallan una guarda que se puede medir hoy. El Worker pararía (C02/C16) o improvisaría. | **El abridor, en dos piezas:** la clase anidada de WCo solo abre el modal (`EnterPanelModal`) y delega en una clase del Plugin fuera de `Plugin/Workspace/` (por ejemplo, `Plugin/Systems/PushBack/`). Esa clase nombra la ventana y `ShowModalWindow`, recibe `PushBackDraftEditorLaunch` (de `Systems.PushBack`, no de `Workspace`) y entra en la guarda de OB-01.3.<br>**La regla, preferiblemente en Application:** pasar la lógica pura de `NumericFieldValidation` (solo `System.Globalization` y `LocalizedNumberParser`) a Application, con `NumericFieldValidation` de la UI delegando en ella. Así WHC la usa por defecto, sin cableado; Core la tiene (desaparece C-8) y sigue habiendo una sola implementación. Hay que medir antes las guardas de T1B sobre `NumericField`. La alternativa es cablearla desde la paleta (que puede nombrar `RackCad.UI`, con nombre completo para no tocar los `using` fijados en `WorkspaceNavigationHostTests:982-984`), pero entonces sigue C-8 |
| **I-19** | Importante | §4.2 | **EIG:664-690 no queda en verde.** El método cuenta `pilot.Read` en el cuerpo de la intención (:677-678) y en el de «comprobar ahora», con el diagnóstico de la admisión (:686-688). Cambiar a `var full = read.FullRead;` lo rompe por letra. EIG:692-721 también rompe (:701-720). | La tabla da «verde por diseño». Eso cambia K y la lista. | Añadirlos a §4.2 como C-5, o medirlos con el texto literal del GREEN, como dice el procedimiento |
| **I-20** | Importante | §5.3 CT-PBT4C-10 | **«Una FVM real sobre el puerto real (la compuerta sobre WHC)», en una clase de Core.** `RackCad.Tests` no referencia la UI (`RackCad.Tests.csproj:23-31`), y la UI no tiene los dobles del anfitrión (puente, sonda y lector), que viven en `RackCad.Tests`. | No compila en ninguno de los dos proyectos. | Partir CT-10: FVM → puerto con un doble que registra, en la UI; y puerto → compuerta → WHC → doble del editor → «Actualizar», en Core. O un kit nuevo de UI con esos dobles, nombrado en la lista |
| **I-21** | Importante | §8.2 Q4 | **Q4 no es neutral.**<br>• «Las hace fallar a todas: 428» no es cierto: con la opción 2 se saltan 428, pero no todas fallarían.<br>• «La única opción que deja la lista cerrada, no esconde ninguna implementación y no debilita ninguna prueba» tampoco: la opción 2, con copias mecánicas y su guion de comparación, también cumple las tres.<br>• La recomendación se apoya en m = 44 (B-06). | Empuja con argumentos que no se sostienen. | Reescribirla con los números de B-06 y con los pros y contras de cada opción: la 1 ahorra unas 170 copias pero exige una excepción a C02; la 2 no exige ninguna excepción, con 428 copias y su guion |
| M-24 | Menor | Varios | Hay texto desfasado:<br>• D-PBT4-19 («los dos atributos `x:Static`»);<br>• D-PBT4-17 sigue con `<a medir>` en la costura, que el §9 ya da por cerrada;<br>• D-PBT4-01 dice «dos sustituciones» (es una);<br>• §11, precondición 4, nombra solo Q1 a Q3;<br>• la cabecera dice «PB-T3c en curso» y «medido en `7dfc7c21`»;<br>• el §7 dice «Qué hace la v2» | | Actualizarlo |
| M-25 | Menor | CT-PBT4B-10, condición 7 | No dice cómo se sustituye el rack resuelto o el informe en `Current`: hacen falta miembros internos (hay `InternalsVisibleTo RackCad.Tests` en Application) | Latitud del Worker | Nombrar el miembro |
| M-26 | Menor | Q2; R-23 | «Unas 150 rutas» cuenta las hojas de todo el proyecto (con `Cantilever`, `DynamicSystem` y los demás de primer nivel), no solo las del Push Back del editor | Matiz | «Del orden de un centenar» |
| M-27 | Menor | §5.5; §7 | Faltan dos cosas:<br>• los riesgos de borrado de PB-T3c (OWNER-1 a OWNER-3 de F4B-E48: referencias en espacio papel o en bloques, purga de un nivel, bloques dinámicos), en el Smoke-PB y junto a la línea «Vistas: se borrarán…»;<br>• medir cuánto alarga la suite de Core pasar cientos de pruebas al comparador completo (A-10; trabajo `tests` con un límite de 20 min en `ci.yml:42`) | | Añadirlas |

### §14.3 Los números de Q4, Q1 y Q3, y la vuelta única

- **Las 428 en 9 clases:** **correcto.** Lo reproduje delimitando los métodos por sus llaves. Los 117 de los otros seis archivos de `WorkspaceHostRig` quedan bien fuera.
- **El «unos 44»:** **no vale como `<m>`.** Solo cuenta lo que nombra el cuerpo del método (con mi guion, 67 al incluir `TryKeepPilotEdit` y `TryUpdatePilot`). Con un nivel de ayudantes del mismo archivo y las dos causas que faltan, son **256 de 428**. Por clase:

  | Clase | Con alguna señal |
  |---|---|
  | `WorkspaceLifecycleHostTests` | 24 |
  | `WorkspacePilotPrepareTests` | 59 |
  | `WorkspaceEditIntentHostTests` | 29 |
  | `WorkspacePilotEditHostTests` | 48 |
  | `WorkspaceCommandAdvisorTests` | 6 |
  | `WorkspacePilotUpdateRequestTests` | 43 |
  | `WorkspacePilotUpdateHostTests` | 23 |
  | `WorkspacePilotReconciliationHostTests` | 23 |
  | `WorkspaceAssociativeBookkeepingTests` | 1 |

  Si B-07 se resuelve con la regla en Application, sobran las que solo dependen del campo sin regla. El número baja, pero no hasta 44.
- **`<n4>`, «Retirar»:** se puede medir ya. Hay 130 métodos en 20 archivos que nombran lo que se retira (§4). Fuera del anfitrión, unos 57: `WorkspaceDraftPilotMaterializationTests` 22, `WorkspaceDraftPilotStateTests` 15, `WorkspacePilotUpdateRunTests` 6, `WorkspaceDraftPilotIntentStaleBaseTests` 4, `PushBackFullWritePrepareTests` 3 y 7 de la UI.

**¿Se pueden rellenar ya los huecos de Q1 y Q3, por texto y para cada opción de Q4?** Sí, pero solo cuando la v4 haya arreglado B-06 y B-07, porque cambian los números. Lo que se podría dar entonces:

| Hueco | Valor |
|---|---|
| PB-T4a (`<a>`) | **1** (CT-PBT3A-12) |
| PB-T4c (`<c>`) | **1** (PEG:156-163), más lo que deje B-07 |
| PB-T4b (`<b>`), opción 1 | El número de B-06 (hoy, 256 en las 9 clases), más las roturas de C-5 sobre el texto literal (las de §4.2 e I-19, unas 12 a 15), más las de la UI de la elección (hasta 21) |
| PB-T4b (`<b>`), opción 2 | 428, más lo mismo de C-5 y de la UI |
| PB-T4b (`<b>`), opción 3 | No se mide por texto. A lo sumo, «no más que la opción 1» |
| `<n4>` y `<n5>` | Por texto (ver arriba) |

**¿Cumple eso la condición de Q3, «la lista que viste»?** Sí, si Q3 la formula como **cota superior**: «la lista que se emite es la que viste o una parte de ella; si el GREEN del Worker hace fallar una que no está, te vuelve».
- La medición compilada de la precondición 6 no puede dar «la lista exacta» de PB-T4b. El esqueleto deja WHC sin cambio, así que no ve los fallos de comportamiento, que solo aparecen en el GREEN del Worker. Sirve para K, D y E, para las roturas de C-5 sobre el texto literal y para confirmar que nada inesperado rompe.
- Con la opción 1, las pruebas de la lista que pasen en el GREEN **no se saltan**: siguen corriendo y no se debilita nada.
- Con la opción 2, la lista es exacta por construcción.

Así, una sola vuelta con números concretos es posible, con dos condiciones: que los números lleguen tras la v4, y que Q3 diga «cota superior; si sale una más, vuelvo».

### §14.4 Las cuatro preguntas, en su texto final

- **Q1:** neutral y autosuficiente. Le faltan los números. Su «probable vuelta a mitad de PB-T4b» es correcta.
- **Q2:** bien. La recomendación «Por grupos» está justificada y se dice la letra («fields»). Solo el matiz de M-26.
- **Q3:** cambiar la condición a cota superior (§14.3) y dar `<a>` = 1 y `<c>` = 1. Lo demás está bien.
- **Q4:** reescribirla (B-06, I-21). Texto propuesto:

  > Para pasar el panel al modelo completo, 428 pruebas del panel de F4 (9 clases) empiezan sus borradores con una lectura del piloto que les da un kit de pruebas compartido. Unas <m> fallarían de verdad, porque sus ayudantes buscan el borrador del piloto; las demás seguirían bien si el kit trajera también el modelo completo. ¿Cómo las trato?
  
  - **«Editar una línea del kit compartido»:** se edita esa línea, una excepción declarada a la regla de no editar ayudantes. Unas 428 − <m> siguen tal cual sobre el camino real. Las <m> se saltan y se sustituyen con registro.
  - **«Saltar y duplicar, como D-1»:** las 428 se saltan y se copian con el kit nuevo. Un guion comprueba que solo cambia el kit. No hay excepción, pero son 428 copias.
  - **«Una implementación de referencia»:** la sesión la escribe para medir la lista exacta. Si el Worker no la recibe, cualquier diferencia suya para el trabajo; si la recibe, cambian los papeles.
  
  La recomendación tiene que salir de esos números, no de «la única opción».
- **Falta alguna:** ninguna, salvo que la v4 elija pasar la regla de la escala a Application y la medición muestre que toca una guarda de T1B. Eso volvería al Owner por OB-T1B.3.

### §14.5 Lo que queda antes de preguntar al Owner (sin contar la medición sobre la copia exportada)

1. **B-07:** rediseñar el cableado de la regla y el abridor, y medir por objetos git las guardas de la solución elegida: `NumericField` y T1B si la regla pasa a Application; `Plugin/Systems/PushBack/` para el abridor.
2. **B-06:** redefinir las firmas (método más ayudantes del mismo archivo, un nivel), añadir C-8 y C-9 y rehacer por texto los números de Q1, Q3 y Q4 para cada opción.
3. **I-19, I-20 e I-21,** y el texto de Q3 como cota superior.
4. Los menores M-24 a M-27.

Todo eso es diseño y texto, medible por objetos git. Después, el paquete estaría **listo para preguntar al Owner**, en una vuelta con números, y la medición de la precondición 6 quedaría para el contrato: K, D y E, C-5 sobre el texto literal, las líneas de CT-PBT4B-06 y el coste de `IsDirty`.

### §14.6 Límites

- Nada se compiló.
- Los 256 son señales por texto (el cuerpo del método y un nivel de ayudantes del mismo archivo). Son una cota aproximada, no un recuento de fallos: pueden contar de más (un nombre de ayudante común) o de menos (ayudantes en otro archivo).
- B-07 se deduce del texto de `WorkspaceHostSourceGuardTests.cs:398-420` en `7f0d9aa3`, que se aplica a cualquier texto que nombre `RackCad.UI` en WCo.

## §15. Recomprobación de la v4

**Veredicto: LISTO TRAS CORRECCIONES DE TEXTO** B/I/M = **0 / 3 / 4**

**Qué falta corregir antes de preguntar:**
- en Q3: los números de cada combinación con Q1, el cálculo de la UI y la cota superior (I-22 e I-23);
- en Q1: la frase «ningún modelo paralelo» (M-31);
- en Q4: la causa de los 242 (M-31).

I-24 y M-28 a M-30 son correcciones del paquete y no cambian las preguntas: deben quedar hechas antes de emitir, no antes de preguntar.

**Qué se revisó:**
- `I-64-f4b-pbt4-gate-contract-package-v4.md` (V4-01 a V4-10);
- `owner-questions-pbt4.md`;
- R-28 a R-33;
- `measure_pbt4_v4.py` y su salida;
- `v4-instructions.md`.

**Cómo:** solo objetos git en `7f0d9aa3`, sin worktree y sin compilar. No hizo falta ningún guion nuevo; lo leí con `git show` y `git grep`.

### §15.1 B-07: la regla en Application

**No lo impide ninguna guarda, y no cambia el comportamiento.**
- **Es pura:** `NumericFieldValidation.cs` solo usa `System.Globalization` y `LocalizedNumberParser`, que ya está en Application (`NumericFieldValidation.cs:1-2`, :61-138). En `src/` solo la llama `NumericField.cs:149`.
- **No hay guardas que la miren por texto o por reflexión:**
  - ninguna prueba lee el texto de `NumericFieldValidation.cs` ni de `NumericField.cs`, ni hace reflexión sobre ellos;
  - ninguna mira `src/RackCad.Application/Formatting/`;
  - la guarda de T1B de la UI lee `Systems/PushBack`.
- **La guarda de capas y espacios de nombres** (`NamespaceFolderGuardTests`: el espacio de nombres igual a la carpeta, en bloque y uno por archivo) se cumple con `RackCad.Application.Formatting` y `RackCad.Application.Systems.PushBack.Draft`.
- **Application no nombra la UI.** La UI ya depende de Application.
- **El comportamiento** es el mismo:
  - los mensajes y el formato con la cultura actual se mueven tal cual;
  - lo fija `NumericFieldValidationTests` (UI), que es la red real;
  - lo para PBT4B-C20.
- **No es una decisión del Owner.** Ningún comportamiento observable cambia, así que T1B sigue en verde por OB-T1B.3. Cumple la Q-PBT2-3 («deja una sola»). Está bien que vaya entre lo declarado.
- **Un matiz (M-29).** El esqueleto del RED de PB-T4b debe dejar `NumericFieldValidation.cs` sin tocar; si no, caen todas las cajas en el RED. En el GREEN, CT-18 solo prueba la conversión entre los dos tipos de resultado, porque los dos lados ejecutan el mismo código. La red real es `NumericFieldValidationTests`.

### §15.2 B-07: el abridor en dos piezas

**Siguen en verde:**
- `WorkspaceHostSourceGuardTests:398-420`: WCo no nombra `RackCad.UI`. La interfaz genérica y el `Action<object>` evitan nombrar la ventana y el lanzamiento;
- `WorkspaceBridgeSourceGuardTests:364-367`: el archivo nuevo no nombra `RackCad.Application.Workspace`, porque `PushBackDraftEditorLaunch` es de `Systems.PushBack`;
- EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723`: no hay archivo nuevo en `Plugin/Workspace/`;
- G-9: `RackPushBackSystemWindow` no casa con `RackPilot\w*`;
- la guarda del modal (PEG:778-781): el abridor solo usa `EnterPanelModal`.

**Las guardas de `Plugin/Views/` y del Plugin entero** tampoco se rompen:
- las de PB-T3c (`PushBackViewEraseGuardTests:362` y :381) solo cuentan las llamadas a la primitiva de borrado;
- `PushBackWritePathPluginGuardTests` solo mira el escritor y la fachada;
- EIG:1023 solo mira RIA;
- los censos de `MainMenuStructuralSectionAccessGuardTests`, `CustomPropertiesCommandGuardTests` y `DialogWindowCharacterizationTests:561-583` miran otros símbolos y otras ventanas.

**CT-PBT1B-15 (14 → 15) es la única rotura.** Es una guarda de D2, una decisión del Owner (condición 8). Su equivalente conserva todas las líneas salvo el recuento, así que no debilita nada, pero conviene nombrarla en Q3 (§15.5).

**Un matiz (M-30).** `ShowModal<TLaunch>` y `Action<object>`, con la conversión en `RackWorkspaceEditorWindow.Show(object)`, pierden el tipo: un lanzamiento equivocado solo fallaría en AutoCAD. CT-08 debe fijar el texto literal de la conversión (`(PushBackDraftEditorLaunch)launch`), y el Smoke-PB lo cubre.

### §15.3 B-06: firmas y números

- **Las firmas,** por método más un nivel de ayudantes, con C-9 y sin C-8: bien. Explican bien la diferencia con mis 256 (cómo se cortan los métodos y C-8).
- **242 / 186 (Q4) y 88 solo por el cuerpo:** son cotas razonables.
- **«Retirar», 149 / 99:** son razonables. La diferencia con mis 130 / 57 está explicada: el paquete cuenta 35 guardas de texto (EIG, PEG, IG y otras) como «fuera de las 9 clases», y yo no las contaba.
- **El «unas 461 / 275» no es cota superior, por dos motivos:**
  - la UI no son «hasta 21». `WorkspacePilotReconciliationViewTests` pulsa los botones de verdad («Conservar mi borrador» y después «Sí») en al menos 3 métodos (:175-183, :199-213, :264-276), y eso pasa por `RequestKeepMine` y `ConfirmChoice`. Son unas 25 (I-23);
  - C-5, «unas 12», es una estimación que solo se cierra con la precondición 6 (I-23).
- **Q3 no suma lo que añade «Retirar» en PB-T4b** (I-22).

### §15.4 Los hallazgos de la §14

| Hallazgo | Estado |
|---|---|
| B-06 | Resuelto (firmas, C-9 y números), salvo la cota y las combinaciones de Q3 (I-22 e I-23) |
| B-07 | Resuelto (§15.1 y §15.2) |
| I-19 | Resuelto: EIG:664-690 y :692-721, en §4.2 por C-5 |
| I-20 | Resuelto: CT-10 en Core y CT-14 en la UI |
| I-21 | Resuelto: Q4 neutral, con la recomendación justificada en una frase |
| M-24 a M-27 | Resueltos, salvo restos (M-28) |

### §15.5 Hallazgos nuevos

| Id | Gravedad | Dónde | Qué | Por qué | Qué propongo |
|---|---|---|---|---|---|
| **I-22** | Importante | Q3 y Q1 en `owner-questions-pbt4.md` | **Los números de Q3 no incluyen lo que añade Q1.** Q3 da para PB-T4b «unas 461 o unas 275», pero si el Owner elige «Retirarlo» (la recomendada), PB-T4b retira el código y sustituye además unas 99 pruebas propias y 3 de D-1. | Con las respuestas recomendadas, la lista emitida pasaría de lo que el Owner vio, y la condición «la que viste o una parte» haría volver en el acto. | Dar el número de cada combinación (texto exacto en §15.6) |
| **I-23** | Importante | Q3; §4.2; D-PBT4-11 | **«Como mucho» con números que no lo son.** «Hasta 21» de la UI se queda corto (ver §15.3), y «unas 12» de C-5 es una estimación. | Una cota que se queda corta convierte la vuelta única en dos. | Contar en la UI también los clics de los botones (unas 25). En Q3, decir que es un recuento por texto, que la lista que se emite es esa o una parte y que cualquier prueba de más vuelve al Owner. Así lo hace ya la opción 1 de Q3, y basta con no prometer más que eso (§15.6) |
| **I-24** | Importante | §5 (las CT); D2, condición 10 | **Los archivos nuevos de `Draft/` quedan fuera de las guardas de D2 y de pureza.** CT-PBT3A-13 (G-L1 de D2) y CT-PBT3A-14 (pureza de la carpeta) recorren una lista **fija** de 8 archivos (`PushBackDraftWriteGuardTests.cs:27-32`, :253 y :309), y G-L1 de CT-PBT1B-14 no mira `Draft/` (`PushBackSharedReferenceGuardTests.cs:178-198`). Quedan fuera los seis archivos nuevos: `PushBackOriginByText`, `PushBackFullFieldAdapter`, `PushBackKeepMinePresentation`, `PushBackAnnotationScaleRule`, `PushBackFullDraftLaunch` y `PushBackFullDraftStatePort`. Dos de ellos están en el camino de edición (`Apply` y `ApplyToDraft`). | La condición 10 del Owner («source guards/tests identify any future added mutation … inside the draft/editing path») dejaría de cubrir código nuevo del camino, sin que nada falle. Es debilitar en silencio. | En cada subtarea, una CT copia de G-L1 y de la pureza sobre sus archivos nuevos de `Draft/`, con autoprueba, como hizo PB-T2a con CT-PBT2A-04. No cambia ninguna pregunta |
| M-28 | Menor | Paquete | Quedan restos desfasados:<br>• el encabezado «## 2. Hechos medidos en `7dfc7c21`»;<br>• el `<a medir en la comprobación del §6.0>` de `WorkspaceCommandAdvisorGuardTests` (línea 218), ya cerrado;<br>• «Los hechos … (R-0 a R-20)» y los guiones de la cabecera | | Actualizar |
| M-29 | Menor | §6.2 | El esqueleto del RED de PB-T4b deja `NumericFieldValidation.cs` sin tocar, y CT-18 solo prueba la conversión de tipos (§15.1) | | Declararlo |
| M-30 | Menor | D-PBT4-15; CT-08 | La conversión desde `object` pierde el tipo (§15.2) | | Fijar el texto literal en CT-08 |
| M-31 | Menor | Q1 y Q4 | Dos frases imprecisas:<br>• Q1-«Retirarlo» dice «ningún modelo paralelo; la carga sigue dentro del lector», y es contradictorio: la copia de la carga es el modelo paralelo de A-5;<br>• Q4 atribuye los 242 a «sus propios ayudantes», pero solo una parte cae por ellos (88 se ven en el cuerpo) | | Texto exacto en §15.6 |

### §15.6 Las preguntas: correcciones de texto exactas

**Q1, el primer párrafo.** Sustituir «Según lo que elijas en la pregunta del procedimiento, son unas 461 (si se saltan y se duplican) o unas 275 (si se edita una línea del kit).» por:

> Contadas por texto, y según lo que elijas en la pregunta del procedimiento, son unas 465 (si se saltan y se duplican) o unas 280 (si se edita una línea del kit).

**Q1, la opción «Retirarlo (Recomendado)».** Sustituir «Queda una sola regla de la escala y ningún modelo paralelo; la carga sigue dentro del lector.» por:

> Queda una sola regla de la escala; del modelo paralelo solo queda la carga, dentro del lector, porque el modelo completo la usa.

**Q3, el segundo párrafo.** Sustituir «Las pruebas que se sustituyen con registro son, como mucho: 1 en PB-T4a; unas 461 o unas 275 en PB-T4b, según la pregunta del procedimiento; y 2 en PB-T4c. ¿Emito cada subtarea tras el cierre de la anterior, sin volver a ti, si su comprobación acotada no deja nada bloqueante ni importante, la remedición no cambia nada material y la lista emitida es la que viste o una parte de ella?» por:

> Ninguna autorización tuya cubre PB-T4. Las pruebas que se sustituyen con registro, contadas por texto, son: 1 en PB-T4a; en PB-T4b, unas 465 si se saltan y se duplican o unas 280 si se edita una línea del kit, más unas 100 si retiras el código del piloto; y 2 en PB-T4c (la lista de operaciones del puerto y el censo de archivos de tu decisión D2 sobre el nombre, que pasa de 14 a 15 sin cambiar nada más). La lista de cada una queda guardada en el paquete. ¿Emito cada subtarea tras el cierre de la anterior, sin volver a ti, si su comprobación acotada no deja nada bloqueante ni importante, la remedición no cambia nada material y la lista que se emite es esa o una parte de ella?

(Las cifras de «unas 465», «unas 280» y «unas 100» son las de la v4 con la UI recontada: unas 25 en lugar de 21, y Retirar, 99 + 3 sin las que ya estén en la lista. La sesión debe sustituirlas por su recuento exacto por texto antes de pegar las preguntas.)

**Q3, la opción 1.** Está bien: ya dice que una prueba que falle fuera de la lista vuelve al Owner.

**Q4, la pregunta.** Sustituir «Unas 242 fallarían de verdad, porque sus propios ayudantes buscan el borrador del piloto; las otras 186 seguirían bien si el kit trajera también el modelo completo.» por:

> Unas 242 fallarían de verdad, porque ellas o sus ayudantes usan el borrador o la lectura del piloto; las otras 186 seguirían bien si el kit trajera también el modelo completo.

**Q2 y el resto de Q4:** bien.

**Lo demás que se pidió comprobar:**
- **Las etiquetas:** todas tienen 5 palabras o menos (sin contar «(Recomendado)»).
- **La recomendación de Q4** («Saltar y duplicar») está bien justificada: las dos opciones cumplen las tres condiciones, y esta no exige excepción a C02 ni se aparta de D-1.
- **Las cuatro preguntas** son neutrales y autosuficientes.
- **Decisiones que falten o sobren:** ninguna. Mover la validación numérica y el abridor en dos piezas son declaraciones, no decisiones. Si la medición de la precondición 6 mostrara una guarda de T1B afectada, eso volvería al Owner por OB-T1B.3.

### §15.7 Límites

- Nada se compiló.
- La cifra de unas 25 pruebas de la UI cuenta los métodos que llaman a `RequestKeepMine` o que pulsan el botón de «Conservar» (por texto). La sesión debe confirmarla con su guion.
- I-24 se deduce de las listas fijas de las guardas en `7f0d9aa3`.

## §16. Comprobación de la v4.1 y vuelta de PB-T4b

**Veredicto: NECESITA v4.2** B/I/M = **1 / 2 / 4**

**En resumen:**
- **PB-T4a no está listo para CA-T4a por una sola cosa.** `AtOrigin` pone en RIA una segunda llamada a `PushBackPilotIntentReader.Read(`, y una guarda que la v4.1 no midió exige que haya una sola (B-08). El arreglo es pequeño y técnico: llevar las dos relecturas puras a `Draft/`. No cambia el comportamiento ni pide ninguna decisión.
- **Lo demás de PB-T4a está bien:** el diseño, los totales, el esqueleto, §6.1.1, §6.1.2 y la lista de 1.
- **PB-T4b tiene que volver al Owner,** y el paquete lo plantea bien:
  - cambian dos premisas (P-1 y P-2);
  - el RED mixto es técnico;
  - el texto de la vuelta está en §16.5.
- **PB-T4a no depende de la vuelta,** así que la v4.2 y la vuelta pueden ir a la vez.

**Qué se revisó:**
- la v4.1 (`6089b8e9…b644`, comprobado): V41-01 a V41-18, D-PBT4-02, 03, 10 y 11, §4.1 a §4.3, §5.0 a §5.2, §6.1 a §6.1.2 y §8.2 a §8.4;
- las listas guardadas: `pbt4-substitution-lists.tsv` (`f24c2dbc…`) y `.md` (`0f4c9bad…`), `pbt4-copy-map.tsv` y `pbt4-retire-compile.tsv`;
- las salidas de `lists_pbt4`, `retire_compile_pbt4`, `probe_overlap`, `copy_map_pbt4` y `measure_pbt4_v41`;
- R-34 a R-38;
- `owner-pbt4-decisions.md`.

**Cómo:** objetos git en `7f0d9aa3`, con `git show` y `git grep`. No hizo falta ningún guion nuevo.

**La numeración de las preguntas** es la del registro:
- pregunta 1, «Piloto»;
- pregunta 3, «Pruebas F4» («Saltar y duplicar»);
- pregunta 4, «Emisión» (la cota).

En el encargo, «Q4» era «Saltar y duplicar» y «Q3» era la cota, que es la numeración del borrador de §15.

### §16.1 Hallazgos

| Id | Grav. | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| **B-08** | B | PB-T4a: D-PBT4-03 (v4.1:377-385) y CT-PBT4A-07 | `TheReadBuildsTheInputFromTheScanAndTheEvidenceOnceAndAWitnessWithoutKeyFailsBeforeAnyInputIsBuilt` (EIG:350-364) cuenta `PushBackPilotIntentReader\.Read\s*\(` en **todo** RIA (`Adapter()`, no `AdapterRead()`), con `Assert.Equal(1, …)` (:357). `AtOrigin` añade la segunda (v4.1:383), y CT-07 la fija (`piloto=1`). **Con el GREEN literal, esa prueba cae:** D = 1, una prueba falla fuera de la lista (vuelta al Owner por la pregunta 4) y la lista de PB-T4a pasa de 1 a 2. No está entre las 32 medidas (`measure_pbt4_v41-out.txt` mide de RIA EIG:132-147, :150-160 y :384-388) | §16.2 |
| **I-25** | I | PB-T4a: §4.1, R-37 y «D = 0» de §5.1 | **Las 32 aserciones son una selección:** no recorren todas las de las guardas que leen los archivos tocados, y por eso se escapó B-08. Además, «D = 0: ninguna otra prueba falla en el RED ni en el GREEN […] Se confirma en la precondición 6» no vale para el GREEN, porque la precondición 6 corre la suite sobre el esqueleto | La v4.2 enumera, una a una, las pruebas de las clases de guarda que leen RIA, NH, WHC, WEI o `Application/Workspace/` (§16.2) y las mide sobre el texto literal del GREEN. Lo que no mida, lo declara como riesgo con su parada |
| **I-26** | I | PB-T4b, §8.3 punto 2 | **La «salida» de retirar después de PB-T4c no evita el borrado.** Cuando se retire, las 55 originales saltadas y los 29 miembros de apoyo dejarán de compilar igual. Lo que cambia es que ya no correrá ninguna, porque sus copias estarán en verde. «El `Skip` de las 428 no chocaría con la retirada» lo calla | Lo dice la vuelta (§16.5) y lo corrige la v4.2 |
| M-32 | M | §8.3 punto 1 | «67 pruebas que dejan de compilar […], entre ellas 48 de las 49 de `WorkspacePilotUpdateRunTests` […], las 9 de `PushBackFullWritePrepareTests`»: de las 67 por compilación, WPURT da 42 y FWP 6. Las cifras 48 y 9 son sus totales en C-7, por texto y por compilación. Que WPURT tiene 49 pruebas es correcto en `7f0d9aa3` | Corregir las cifras |
| M-33 | M | §8.3 y D-PBT4-11 | Las 9 C-5 que nombran lo retirado (`probe_overlap-out.txt`) están entre las 15. Su causa es el cambio del anfitrión, no la retirada: G-4 (el anfitrión deja de nombrar `Execute`), PEG:280 y `KeepPilotEditsOn` en EIG | Decirlo en una frase: así 470 es exacto con cualquier opción de la retirada |
| M-34 | M | §5.2, el punto abierto | «Si lo escribe la sesión, se acerca a la implementación de referencia»: **no.** La opción 3 de la pregunta 3 era escribir la implementación de `src/` para medir la lista. El kit nuevo es código de prueba que D-PBT4-11.3 ya fija (los mismos miembros y firmas, y `FromReads` sobre el mismo escaneo), y va literal en el contrato, como cualquier CT. Medir sobre el esqueleto qué copias fallan en el RED es técnico | Reescribir la frase |
| M-35 | M | PB-T4a, el comentario de `AtOrigin` | Las excepciones de las dos lecturas puras suben a NH. La v4.1:387 lo dice, pero el `<summary>` del método no, y RIA promete un fallo tipado en `Read` | Una línea en el `<summary>` |

### §16.2 PB-T4a: `AtOrigin`

**Lo que está bien:**
- **La firma de `Read` no cambia** (EIG:139-141), y el contrafactual lo mide: 2 de 2 en rojo.
- **No es traducción de estado.** `AtOrigin` no convierte un borrador en otro: elige otra clave de origen por su texto, sobre el escaneo ya tomado, y repite sobre ese escaneo las dos lecturas puras ya probadas. Es «consumir» la API, como pidió el Owner.
- **No lee el dibujo:** no tiene `CaptureDrawing`, ni bloqueo, ni transacción. Siguen G-9 (en RIA, `StartTransaction` 1 y `LockDocument` 1; IG:684 y :715) y el censo de `Task` (IG:798-835, con RIA en (0, 0)).
- **Las demás guardas que leen RIA siguen en verde con `AtOrigin`:**
  - en EIG, :132-147, :150-160 y :176-201;
  - :268-286, sin `throw`;
  - :320-336, con un `CaptureDrawing` y un `new Handle(`;
  - :378-400;
  - :896-922, donde `.BuildStaleBase(` no casa con `\.StaleBase\b`;
  - CAG:433-439 solo lee los cuerpos de `Read` y de `LoadCatalog`.
- **NH.** Busqué en las guardas de `Workspace/` recuentos sobre lo que añade `ReadIntentAtOrigin`: `try`, `catch (System.Exception`, `LogFault(`, `EditIntentReadResult.Failed(` y `ReadOnActiveDocument`.
  - Los que hay miran cuerpos fijos (el de `ReadOnActiveDocument`, en CAG:433 y EIG:41) u otros archivos.
  - No encontré ninguno roto, pero no recorrí todas las guardas de NH (I-25).

**Lo que lo rompe (B-08): EIG:357 y :362 juntas.**
- Las dos dejan un solo sitio de RIA para la lectura del piloto: el cuerpo de `Read`, después de `new PushBackPilotIntentScan(`.
- NH tampoco puede llevarla: EIG:465-470 no admite ningún `PushBackPilot` en NH.
- Solo queda `Draft/`.

**El arreglo que propongo** es técnico y no cambia el comportamiento.
- **`PushBackOriginByText` gana `ReadAt`.** Su firma: `ReadAt(PushBackPilotIntentScan scan, string originText, RackCatalog catalog)`.
  - Hace lo que hacen hoy las líneas 2 a 5 de `AtOrigin`.
  - Devuelve una tupla (el escaneo, la lectura del piloto y la completa) o `null`, sin ningún tipo nuevo.
  - No añade ninguna dependencia: `PushBackFullDraftReader.cs` ya llama a `PushBackPilotIntentReader.Read` desde `Draft/` (:56).
- **`AtOrigin` solo llama a `ReadAt`** y construye `RackPilotIntentReadResult`. RIA se queda con un solo `PushBackPilotIntentReader.Read(`.
- **Cambia con él:**
  - PBT4A-C13 e INV-PBT4A-13 añaden `ReadAt`;
  - CT-07 pasa a `ria-AtOrigin readat=1 piloto=0 completo=0 …` y gana la autoprueba `autoprueba-segunda-lectura-del-piloto-en-RIA=falla`;
  - en CT-08, la pureza y G-L1 de `PushBackOriginByText.cs` admiten los dos lectores, como en `PushBackFullDraftReader.cs`;
  - el esqueleto lleva `ReadAt` inerte, que devuelve `null`;
  - la tabla de §4.1 añade la medida de EIG:350-364.
- **Los totales no cambian.**

**Lo que la v4.2 debe recorrer entero (I-25).** Son las clases que leen los archivos tocados, según `git grep` en `7f0d9aa3`:
- **RIA:** EIG, CAG e IG (`WorkspacePilotUpdateIntegrationGuardTests`);
- **NH:** EIG, CAG, IG, `WorkspaceBridgeSourceGuardTests`, `WorkspaceBrowserHostGuardTests`, `WorkspaceNavigationAuthorityTests`, `WorkspaceNavigationHostTests`, `WorkspacePaletteHostSourceGuardTests`, `WorkspacePilotReconciliationGuardTests` y `WorkspaceSourceGuardExceptionTests`;
- **WHC, WEI y la interfaz nueva:** las de §4.1, las que las lean por ruta (con el mismo `git grep`) y el censo de `Task` de IG, que incluye `src/RackCad.Application/Workspace/`.

### §16.3 PB-T4a: lo demás que se pidió

- **CT-PBT4A-08 está bien.** Pasa en el RED, porque el esqueleto crea el archivo puro. Sus autopruebas fallan sobre el texto inyectado, y va con `ExpectRed` false en `PushBackFullReadGuardTests`. Con B-08 hay que ajustar su pureza (§16.2).
- **Los totales cuadran:**
  - RED: 19 137 = 19 125 + 5 + 7, con 19 125 = 19 123 − 1 + 3;
  - GREEN: 19 137 = 19 130 + 7;
  - esqueleto: 19 129 = 19 122 + 7.

  El arreglo de B-08 no los cambia. Con la v4.1 tal cual, el GREEN daría 19 129 `Passed`, 1 `Failed` y 7 `Skipped`.
- **§6.1.1 y §6.1.2 coinciden con el contrato de PB-T3c (R-38):**
  - los sha256 de nc1 (`41440edc…`) y de nc2 v2 (`c033e96c…`) en `7f0d9aa3` son los del bloque;
  - las 17 paradas estándar y C11 a C18 siguen el texto de PB-T3c con PBT3C → PBT4A.
- **La lista de PB-T4a,** que es 1 (CT-PBT3A-12), está dentro de la cota del Owner una vez arreglado B-08. Sin el arreglo, el GREEN tendría una segunda prueba fuera de la lista.

### §16.4 PB-T4b: los números y las premisas

**Los números, por muestreo, cuadran:**
- **La tabla guardada tiene 612 filas:**
  - PB-T4a: 1;
  - PB-T4b: 609 = C-0 428 + C-5 y C-6 15 + UI 27 + C-7 139;
  - PB-T4c: 2.

  Sin C-7, PB-T4b queda en 470. No hay ningún par archivo-método repetido.
- **C-7: 139 = 72 por texto + 67 por compilación.** En `retire_compile`, 179 = 67 nuevas + 57 ya en las listas + 55 de las 9 clases.
- **Las 55: EIH 2 + WPEH 16 + WPP 30 + WPRH 7.** Lo comprobé en el texto:
  - `WorkspacePilotEditHostTests.cs:65`, `private const string Key = PushBackPilotFieldAdapter.AnnotationScaleKey;`, un miembro de la clase original;
  - las llamadas directas a `PushBackPilotUpdatePlanner.Prepare` en WPP (:96, :1458, :1469, :1644 y :1807-1844).
- **Los miembros de apoyo** suman 29 en la salida.
- **Las 53 «a mano».** Las pruebas de WPP que llaman al `Prepare` del piloto en su cuerpo salen como `R-retirado` en `pbt4-copy-map.tsv`, no como mecánicas.
- **El RED mixto** se sigue de D-PBT4-11: en el RED, el anfitrión todavía es del piloto.

**Lo que cambia las premisas del Owner:**
- **P-1. Las preguntas 1 y 3 no caben juntas en la misma subtarea.**
  - Retirar en PB-T4b hace que 55 de las 428 originales saltadas y 29 miembros de apoyo dejen de compilar. Habría que borrarlos o editarlos.
  - La pregunta 3 decía: «No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes».
  - Además dejarían de cumplirse el punto 1 de CT-20 (el original es igual a la base salvo los `Skip`) y «C02 no tiene excepción» (D-PBT4-11).
- **P-2. Lo que cuesta retirar (preguntas 1 y 4).**
  - La retirada sustituye 139 pruebas, no unas 100 (+39 %).
  - 67 de ellas dejan de compilar, sobre todo las que ejecutan el `Execute` o el `Prepare` del piloto.
  - El total de PB-T4b, 609 frente a unas 565, queda fuera de la cota de la pregunta 4 por la propia regla del Owner.

**Lo que es solo técnico y no necesita pregunta:**
- **T-1. El RED mixto.**
  - Las copias se parten por `ExpectRed` en `<Clase>FullRedTests` y `<Clase>FullGreenTests`, con una partición cerrada medida sobre el esqueleto.
  - M-00 nombra las dos clases, y CT-20 compara su unión con el mapa.
  - El kit va literal en el contrato (M-34).
  - No toca ninguna premisa: siguen siendo copias que solo cambian el kit.
- **T-2. 237 y 191 frente a 242 y 186.** No cambia nada: la opción elegida copia las 428 igual.

**Lo que toca a la cota, no a las premisas:**
- **T-3. 470 frente a «unas 465»: no pasa la cota por sí solo.**
  - La regla de la pregunta 4 es de conjunto («la lista que se emite es esa o una parte de ella»), no una tolerancia sobre la cifra.
  - Hay 3 de más en C-5, 15 frente a «unas 12» (que la v4 no enumeraba), y 2 de más en C-3, 27 frente a unas 25. No estaban en lo que vio el Owner.
  - Es poco, y como la vuelta ya existe se confirma en la misma ronda (pregunta 2 de §16.5).
- **T-4. Las 53 «a mano».**
  - La pregunta 3 decía «Son 428 copias». En realidad son 375 copias que comprueba el guion y 53 equivalentes escritos a mano, cada uno con su fila de RR-3.
  - 37 de las 53 son R-C2, R-C3 o R-C4: describen lo que PB-T4b cambia a propósito y necesitarían su equivalente con cualquier opción.
  - No cambia la elección, pero el Owner debe verlo al confirmar la lista. Va en la pregunta 2.

### §16.5 La vuelta al Owner: el texto exacto

Son dos preguntas, en una sola ronda de AskUserQuestion. La segunda solo depende de la primera en la cifra (470 o 609).

**Pregunta 1.** Encabezado: «Retirada».

> Al preparar PB-T4b con las listas exactas, dos de tus respuestas chocan en la misma subtarea: «Retirarlo» (el código del piloto) y «Saltar y duplicar» (las 428 pruebas del panel). Si el código del piloto se retira en PB-T4b, 55 de esas 428 dejan de compilar aunque estén saltadas, porque ellas o sus ayudantes usan lo retirado (por ejemplo, la clave de la escala del adaptador del piloto o el Prepare del piloto); y con ellas, 29 ayudantes. Habría que borrarlos con registro, y eso es una excepción a la regla de no editar pruebas ni ayudantes, que con «Saltar y duplicar» no hacía falta. Además, la retirada sustituye 139 pruebas, no unas 100: 67 de ellas porque dejan de compilar, sobre todo las que ejecutan el «Actualizar» del piloto. ¿Cuándo retiro el código del piloto?

Las opciones:
1. **«Retirar tras PB-T4c» (Recomendado):** PB-T4b sustituye 470 pruebas, sin borrar ni editar ninguna. El código del piloto queda sin uso hasta una subtarea propia, PB-T4d, después de PB-T4c, cuando las copias ya estén en verde. PB-T4d sustituye las 139 y borra con registro las 55 originales saltadas y los 29 ayudantes, que para entonces ya no corren, y te vuelve con su lista antes de emitirse.
2. **«Retirar en PB-T4b»:** PB-T4b sustituye 609 pruebas (470 + 139) y borra con registro 55 de las 428 originales y 29 ayudantes, como excepción declarada a la regla de no editar pruebas ni ayudantes. No hace falta otra subtarea.
3. **«Conservarlo sin uso»:** No se retira, y sus pruebas siguen en verde. PB-T4b sustituye 470. Quedan anotados en ideas futuras el modelo paralelo, la segunda copia de la regla de la escala y la premisa de D-1 sin cumplir.

**Pregunta 2.** Encabezado: «Lista T4b».

> Con el recuento exacto, PB-T4b sustituye 470 pruebas (609 si eliges «Retirar en PB-T4b»), frente a las unas 465 que viste. Son las 428 del panel (375 copias que comprueba el guion y 53 equivalentes escritos a mano, con su registro, porque describen lo que PB-T4b cambia a propósito o dependen del piloto de una forma que la copia no puede cambiar), 15 guardas de texto (se dijeron unas 12) y 27 pruebas de la interfaz de «Conservar» (se dijeron unas 25). Tu condición de emisión pide que la lista emitida sea la que viste o una parte de ella, y 5 de estas no estaban contadas. ¿La tomo como la lista que viste?

Las opciones:
1. **«Sí, esa lista» (Recomendado):** PB-T4b se emite con esa lista exacta, la guardada en el paquete, si se cumple el resto de tu condición de emisión. Una prueba que falle fuera de ella te sigue volviendo.
2. **«Vuelve antes de emitir»:** PB-T4b te vuelve con su contrato y su lista antes de emitirse.

**Comprobaciones del formato:**
- **Encabezados:** «Retirada» tiene 8 caracteres y «Lista T4b», 9.
- **Etiquetas:** entre 3 y 4 palabras, sin contar «(Recomendado)».
- **Neutralidad:** cada opción dice su coste:
  - una subtarea más que vuelve al Owner;
  - la excepción a la regla;
  - el modelo paralelo.

**Por qué se recomienda «Retirar tras PB-T4c»:**
- PB-T4b mantiene la premisa de la pregunta 3.
- El borrado solo ocurre cuando las originales ya no corren.
- El coste es una subtarea más, que vuelve al Owner.

**Para custodiar:** la lista que se confirma es `pbt4-substitution-lists.md` (sha256 `0f4c9bad40bcb8c0dc4590256bf042f8838d9319f5c1e60558a7755926cfbdac`):
- sin las filas de C-7, si la respuesta es la opción 1 o la 3;
- entera, si es la 2.

**Lo que cambia en la v4.2 según la respuesta a la pregunta 1:**
- **Con la opción 1,** se reescriben D-PBT4-10 y §8.3, y PB-T4d entra en el orden de la pregunta 4 con su vuelta.
- **Con la opción 2,** el punto 1 de CT-20 y la frase «C02 no tiene excepción» admiten los borrados, con su registro.

### §16.6 Límites

- **Nada se compiló.** B-08 se deduce del texto de EIG:350-364 y de la v4.1:383. La cuenta es una expresión regular sobre `Src.Code(AdapterPath)`, y la llamada de `AtOrigin` es código.
- **No recorrí todas las guardas de NH** (I-25).
- **Los números de PB-T4b** los comprobé por muestreo y sobre las salidas de los guiones del generador. No rehíce los guiones.
