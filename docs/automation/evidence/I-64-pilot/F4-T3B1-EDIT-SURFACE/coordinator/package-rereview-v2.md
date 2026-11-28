# Re-revisión adversarial independiente — Paquete del contrato de F4-T3B1 (v2)

- **Paquete revisado:** el paquete `I-64-f4-t3b-gate-contract-package.md` (carpeta de trabajo de la sesión) (v2), con su §7 (puntos) y su §8 (disposición). La v1 sigue en `package-v1.md`, y mi revisión de la v1 en `package-review.md`.
- **Repositorio:** worktree de I-64, rama `architecture/workspace-persistente-rackcad`, HEAD `eb55606c`.
- **Método:** solo lectura (`git grep` y lectura de archivos). Sin build ni pruebas. Lo que digo de las guardas sale de leer sus regex contra el texto real.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** 0 BLOQUEANTE, 2 IMPORTANTE, 6 MENOR.

La v2 cierra los 10 IMPORTANTE y los 11 MENOR de la v1, con las salvedades de N-1 (el primer gesto por tecla) y N-2 (R-B2-5). No encontré ninguna guarda no declarada que rompa con lo que introduce la v2:
- `PreviewTextInput`, `PreviewKeyDown`, `PreviewMouseLeftButtonDown`, `LostFocus` y `MouseDoubleClick` en el code-behind;
- `e.Handled`;
- el `TextBox` y los botones segundo y tercero de la superficie;
- los nombres de automatización;
- el atributo nuevo en el `Button` de la plantilla de pestaña.

Los dos IMPORTANTE se corrigen en el texto del paquete, sin volver al Owner (en N-2, opción a).

## Hallazgos

### N-1. IMPORTANTE — Teclado: la tecla del primer gesto se aplica si no se marca, y D-01 queda a medias

**Dónde.** D-T3B1-03 (pulsación en `PreviewTextInput`, «la tecla no se aplica») y D-T3B1-04 (solo Enter y Esc con `e.Handled`).

**(a) La tecla del gesto se aplica.**
- El gesto corre dentro de `PreviewTextInput`. Si responde `Admitted` o `AlreadyOpen`, el VM notifica la editabilidad, y el binding de `IsReadOnly` actualiza el `TextBox` en el acto.
- Si el handler no marca `e.Handled = true`, el `TextInput` que sigue llega a una caja **ya editable**: el editor inserta el carácter y el setter llama a `SetPendingText`.
- Contradice D-T3-04 («el gesto que disparó la intención no se aplica») y D-10 (`proposal-v5.md:430`).
- La prueba prevista no lo detecta: `RaiseEvent` de `PreviewTextInputEvent` no encadena el `TextInput`.

**(b) D-01 pide más que Enter y Esc.** D-01 (`proposal-v5.md:131-132`) nombra espacio, Enter, Esc, Supr, Ctrl+Z, Ctrl+C y Ctrl+V. La v2 solo marca Enter y Esc.
- En el estado por defecto del campo, la caja en solo lectura, todos los comandos de edición están inhabilitados. Supr, Ctrl+Z, Ctrl+X y Ctrl+V no los maneja nadie en WPF.
- Con la caja editable pasa lo mismo con Ctrl+Z sin nada que deshacer o Ctrl+V sin texto en el portapapeles (`CanExecute` falso).
- Si salen hacia AutoCAD, son ERASE de la selección implícita, UNDO o PASTECLIP. Sería un cambio del DWG nacido de una tecla del panel, contra el oráculo crítico.
- La mecánica no está congelada (`proposal-v5.md:137`), pero la obligación sí.

**Trampa.** `e.KeyboardDevice` contiene «Keyboard» y cae en G-08 (`WorkspaceNavigationHostTests.cs:919`). No se pueden leer los modificadores desde el `.cs`.

**Corrección (compatible con G-08).**
- **Caja en solo lectura:** el primer gesto por tecla va en `PreviewKeyDown`: cualquier tecla que no sea un modificador ni Tab lanza el gesto y se marca `Handled`. Al marcar la tecla, no se genera `TextInput`, así que la tecla no se aplica ni sale del panel. Si se conserva `PreviewTextInput`, se marca siempre `Handled` con la caja en solo lectura.
- **Caja editable:** un `KeyDown` de burbuja marca `Handled` toda tecla que el editor no haya consumido, salvo Tab (que se deja a la navegación). Las dos reglas se combinan sin conflicto. Ojo: marcar el `KeyDown` suprime el `TextInput`, así que un `KeyDown` que lo marque todo anularía un gesto colocado en `PreviewTextInput`.
- **Pruebas de vista:**
  - en solo lectura, `PreviewKeyDown` con `D5` → gesto, `Handled` y `box.Text` sin cambio;
  - si se usa `PreviewTextInput`, la prueba lanza el `TextInput` cuando no queda marcado, como hace el sistema de entrada;
  - `KeyDown` de `Delete` y de `Z` sin consumir → `Handled`.
- **Smoke-2:** la lista completa de teclas de D-01.

### N-2. IMPORTANTE — R-B2-5 promete lo que T3B2 no puede cumplir con `GetRacksAtRisk`

**Dónde.** §6 R-B2-5 («su VM contiene esas llamadas: un fallo de `GetRacksAtRisk` no se presenta como «sin racks en riesgo»»).

**Problema.**
- El fallo de `GetRacksAtRisk` no llega al VM como una excepción. El `catch` externo del host devuelve `default`, con las dos listas vacías (`WorkspaceHostCore.cs:802-806`), y es indistinguible de «nada en riesgo». Un `try/catch` en el VM no lo ve.
- Cumplir la regla exige el endurecimiento del menor 4: un indicador de fallo en `PilotRiskView` o un incierto. Eso es un cambio del host, prohibido para T3B2 por el alcance que este mismo paquete prevé (C04), y el Owner lo asignó a T3C.
- Para `TakeLossNotices` y `TakeCloseNotices` la regla sí se cumple: sus excepciones llegan al VM, aunque los avisos ya sacados se pierden en el host.

**Corrección.** Una de dos:
- (a) redactar R-B2-5 a lo alcanzable:
  - el fallo de `GetRacksAtRisk` es hoy inalcanzable y su dirección es segura para la TOCTOU: el conjunto «cambió», así que no descarta;
  - el mensaje «sin racks en riesgo» puede ser falso, y queda como limitación declarada hasta T3C;
  - opcional: contrastar con `GetField(...).IsProtected` de las pestañas conocidas y, si discrepan, avisar «no se pudo decidir»;
- (b) pedir al Owner que la parte de `GetRacksAtRisk` del menor 4 pase a T3B2, con su cambio de host declarado.

Lo coherente con la orden de cierre de T3A es (a), dejando constancia al Owner (ya previsto en §8, M-7).

### N-3. MENOR — `isPreview` queda desfasado tras fijar

**Dónde.** D-T3B1-02 (`Show(session, rack, isPreview)` y el renderizado en cada presentación) y D-T3B1-03 (fijación automática al reconstruir la tira).

**Problema.**
- En `Rebuild`, `SyncSurface` (con su `Show`) va antes que `RebuildTabs`, que fija (`WorkspaceBrowserViewModel.cs:204-209`). Una preview con borrador protegido queda fijada, pero el campo sigue creyendo que es preview.
- `PinTab` (por `Interact`) no es una «presentación del panel», así que no vuelve a renderizar.

El efecto va en la dirección segura (solo lectura y «Editar» en una pestaña ya fijada), pero es incoherente.

**Corrección.** El navegador vuelve a renderizar el campo al **final** de cada `Rebuild` (`Present`, `Interact` y el `Action` del campo), después de `RebuildTabs`, con el `IsPreview` vigente.

### N-4. MENOR — El renderizado en cada presentación no debe borrar el aviso ni el «No editable aquí»

**Dónde.** D-T3B1-02 («cada presentación vuelve a renderizar el estado del campo desde `GetField`»).

**Problema.** `NotAdmitted` no deja borrador: `GetField` responde `NotOpen`. Y el motivo de un `Waiting` vive solo en el VM. Si el renderizado lo recalcula todo, el siguiente `StateChanged` (normalmente el fin del comando) borra «No editable aquí», la indicación de RACKEDITAR o el motivo, y vuelve a ofrecer «Editar».

**Corrección.** El renderizado de presentación actualiza el estado, la editabilidad y el texto. Conserva `NoticeText` y el diagnóstico `NotAdmitted` de la misma sesión y rack hasta un gesto nuevo o un cambio de superficie. Prueba: `NotAdmitted` → `Present` → el diagnóstico sigue.

### N-5. MENOR — El setter como entrada única necesita su propia contención y su propia guarda de editabilidad

**Dónde.** D-T3B1-04 y D-T3B1-08 («los gestos del campo contienen sus fallos»).

**Problemas.**
- **(a)** Una excepción en el setter de origen la captura el motor de binding de WPF, y sin `ValidatesOnExceptions` solo la registra en la traza: no llega al `Contained` de la vista. Si el setter lanzara (por ejemplo, el `Action` del navegador), la caja quedaría con un texto que el borrador no tiene, sin aviso. El setter tiene que contener sus fallos: Error del campo y restauración.
- **(b)** El setter no hace nada, y restaura, si el campo no es editable. La caja en solo lectura ya lo impide en la vista, pero la prueba 10 en el VM («teclear» en una preview) depende de ello.
- **(c)** `CommitField` en `LostFocus`/Enter solo con pendiente **y la caja editable**. Un Unknown o Applying con pendiente daría `NotEditable` (`WorkspaceHostCore.cs:648`) y un aviso espurio.

### N-6. MENOR — Precisiones de las guardas nuevas y de G-04

- **(a)** «El code-behind no captura `TextChanged`» debe mirar también el XAML (`TextChanged\s*=`). Un handler con `RoutedEventArgs` vale para `TextChangedEventHandler` por contravarianza y no nombraría `TextChanged` en el `.cs`. Conviene añadir `SourceUpdated` y `TargetUpdated`. Para `.Text =`, usar `\.Text\s*=(?!=)`.
- **(b)** Equilibrio de G-04: `<Border\b` casa con `<Border.Style>` y con `<Border … />`, y desequilibra el recuento. La apertura debe ser `<Border(?=[\s>])`, sin `/>`, y el cierre `</Border>`, con una autoprueba de cada caso.
- **(c)** En G-15, «nadie toca la plantilla de pestaña» contradice el `MouseDoubleClick` del botón de la plantilla. La guarda `:283-296` solo pide que el `Border` no lleve atributos de borde: redactarlo así.

### N-7. MENOR — Huecos en las pruebas

- **(a)** Prueba 9, «orden de llamadas»: `Pin` no es una llamada del puerto. Para probar que se fija antes de la intención, el gancho del doble mira `Browser.Tabs` dentro de `TryBeginEdit` (`IsPreview == false`). Decirlo.
- **(b)** Las pruebas de `PreviewKeyDown` tienen que lanzar `Keyboard.PreviewKeyDownEvent`. El patrón de H-09 (`SelectiveCommitCoverageTests.cs:34-42`) lanza `KeyDownEvent`, y `RaiseEvent` no encadena el túnel con la burbuja.
- **(c)** Con la opción (a) de I-9 desapareció la única prueba de `Dematerialize`. Falta una que sobreviva a R-B6: selección con la pestaña **solo dirty** (sin pendiente) → R se activa y la anterior pasa por `Dematerialize`.
- **(d)** D-T3-03: `CommittedText` puede ser null con su motivo (`WorkspaceHostCore.cs:876-880`). El paquete no dice qué muestra la caja (vacía y editable) ni dónde aparece `CommittedTextReason`. Fijarlo y probarlo.

### N-8. MENOR — Horizonte

**Problema.** La v2 añade trabajo: pruebas STA con `TextComposition`, `HwndSource`, gancho de reentrada, Error por excepción, mecánica de teclado (N-1) y renderizado en cada `Rebuild`. El alcance sigue en 19 archivos, pero el volumen de pruebas crece claramente respecto de la v1. La mecánica fijada reduce la exploración, que era el objetivo de M-10. La salida C07 → Owner, con el bloque separable de los arrastres de Core, es razonable.

**Recomendación.** Que el prompt de planificación cite D-T3B1-04 (con N-1) como receta. Así ni el Worker ni el planificador tienen que reconstruir la mecánica de WPF.

## Lo que comprobé y encontré correcto

**Cierre de los hallazgos de la v1, frente al código.**
- **I-1:** la editabilidad exige «no preview» y la prueba 10 reproduce el camino de cerrar y reseleccionar. Una pestaña fijada nunca vuelve a preview (`BrowserSession.cs:127-134`), así que el desfase de N-3 solo va en la dirección segura.
- **I-2:** el Limpio obsoleto en solo lectura cumple O-01. `Reloaded` limpia la marca (`DraftSession.cs:446-450`), así que no hay un bucle de recargas.
- **I-3:** «Recargar» sin `TryReload`, y fallos contenidos en el campo.
- **I-4:**
  - el binding TwoWay con `PropertyChanged` llama al setter solo al pasar del destino al origen;
  - `box.Text = "x"` en una prueba conserva el binding TwoWay y empuja al origen, que es el patrón de `SelectiveCommitCoverageTests.cs:25`;
  - la restauración dentro del setter es viable en .NET 8, porque el binding relee el origen;
  - las pruebas (a)-(c) discriminan `LostFocus` por defecto, la captura en `TextChanged` y la restauración.
- **I-5:** las precondiciones evitan el `Aborted` espurio de `WorkspaceHostCore.cs:692-698` y el compromiso en un re-clic. Un `Kept` sobre un no editable con pendiente es probable con el doble.
- **I-6:** la comprobación tras la lectura y su prueba con el gancho.
- **I-7:** «deben pasar», con el esqueleto completo. Las guardas nuevas, en negativo, pasan sobre un VM inerte, y las de G-01..G-04 pasan si el esqueleto ya está cableado.
- **I-8:** los textos nuevos de G-01..G-03 son coherentes con los 3 sitios de G-01 y los 2 de G-02 (`WorkspacePaletteHostSourceGuardTests.cs:192`, `:539`; `WorkspaceBrowserHostGuardTests.cs:213-215`, `:225-229`; `WorkspaceNavigationHostTests.cs:947`). La cota de G-04 cumple I-1, con la salvedad de N-6(b).
- **I-9:** las pruebas 5 y 6 van sobre `DocumentChange` y sobre `Kept`, que R-B6 no cambia, y lo interino queda declarado sin prueba.
- **I-10:** se añade la pulsación, con la salvedad de N-1.
- **M-1..M-11:** cerrados. G-15 y G-16 están bien medidos: el primer `        Waiting,` de `WorkspaceEditIntent.cs` es el de `EditIntentStatus` (`:88`) y `EditCheckStatus` va en `:195`.

**Guardas frente a lo nuevo de la v2.** Busqué en `tests/RackCad.Tests/Workspace` y en `tests/RackCad.UI.Tests`:
- no hay ninguna sobre `PreviewMouse`, `PreviewTextInput`, `PreviewKeyDown`, `MouseDoubleClick`, `Handled`, `TextBox`, `KeyBinding` ni nombres de automatización de la sección (la única es `AutomationProperties.Name="Workspace"`, `WorkspacePaletteHostSourceGuardTests.cs:617`);
- G-08 admite `KeyEventArgs`, `Key.Enter`, `TextCompositionEventArgs` y `MouseButtonEventArgs` (salvo `e.KeyboardDevice`, N-1);
- G-09 no afecta;
- el atributo nuevo del `Button` de la plantilla no toca el `Border` de `:283-296`;
- las pruebas de UI existentes enumeran la tira o elementos por nombre, nunca todos los botones o cajas de la vista (`WorkspaceBrowserViewTests.cs:249`, `:304-319`, `:366-400`).

Trampa sin declarar: si el Worker llama `Commit()` a la entrada de Enter del VM, el code-behind contendría `.Commit(` y caería en `WorkspaceNavigationHostTests.cs:920`. G-11 lo cubre.

**Implementabilidad de las pruebas.**
- **Patrones de eventos:** `LostFocusEvent` (`BoundedEditorCharacterizationTests.cs:458`), `KeyEventArgs` con `HwndSource` (`SelectiveCommitCoverageTests.cs:34-42`), `MouseButtonEventArgs` (`DynamicShellMigrationTests.cs:201`); `TextCompositionEventArgs` se construye con `InputManager.Current`. Las pruebas no están sujetas a G-08.
- **El doble:** los valores del puerto se construyen con fábricas y constructores públicos.

**I-1 en las guardas nuevas.** Las dos guardas negativas por cuerpo no prohíben un «Comprobar ahora» de T4 ni los botones de T3B2. Retirar la guarda del modal es correcto, porque la cubre G-13 (`WorkspacePilotEditGuardTests.cs:639-660`).

**T3B2.**
- `keepActive` sigue siendo implementable sin otra ronda de guardas: miembros por nombre (`WorkspaceBrowserBoundaryTests.cs:176-197`), vocabulario (`:54-67`, `:104-113`), tipos de parámetro (`WorkspaceBrowserSessionTests.cs:264-268`), y la secuencia larga con el valor por defecto.
- «Nunca se reemplaza la pestaña activa» cierra el riesgo de `BrowserSession.cs:95-107`.
- La relectura de R-B9, decidida ya, es aceptable.

**Contrato.** Los 19 archivos siguen bastando. La regla RED es coherente con `AUTOMATION_PLAN.md:599` y `:620`.

## Riesgos que solo resuelve un smoke con AutoCAD (además de los de la revisión de la v1)

1. **Qué teclas no manejadas de una paleta WPF llegan a AutoCAD con `KeepFocus = true`:** la lista completa de D-01, en solo lectura y en edición.
2. **Si marcar el `KeyDown` suprime de verdad el `WM_CHAR`** dentro del bucle de mensajes de AutoCAD (N-1), y por tanto si la tecla del gesto no se aplica.
3. **La relectura del binding** tras una restauración, y la posición del cursor, en el host real.
