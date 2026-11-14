# I-64 — Re-revisión adversarial independiente del paquete del contrato de F4-T4A2 (v2)

**Objeto:**
- comprobar que cada disposición del §10 de la v2 resuelve su hallazgo de la revisión de la v1 (`independent-review.md`, 1/5/16);
- revisar de forma adversarial lo que la v2 añade.

**Base medida:** HEAD `b9e1ed5a` (producto idéntico a `bbd9a35e`). Solo lectura: no se modificó nada en el repositorio, no se compiló y no se ejecutó ninguna prueba. Solo se leyó y se buscó con grep en `src/` y en `tests/` (sin `bin/` ni `obj/`). Todas las rutas son relativas al repositorio.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Cuentas: **B = 1, I = 1, M = 11.**

- **Lo que sí resuelve la v2:** las 22 disposiciones resuelven sus hallazgos, salvo dos.
  - La corrección de I-1 abre un hallazgo nuevo del mismo tipo que el B-1 de la v1, R-B1: una regla de guarda que no se mide contra el texto real.
  - La corrección de I-4 deja una contradicción interna que puede volver menos conservador el estado tras una comprobación fallida (R-I1).
- **Las dos correcciones son de una frase y se verifican de forma mecánica** (§4).

---

## 1. Verificación de las disposiciones del §10

| Id de la v1 | ¿Resuelto? | Comprobación |
|---|---|---|
| B-1 | **Sí** | H-19 se midió de nuevo en `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` y es exacto: 32 apariciones, en las siete formas y por cuerpo (intención `:327-349`, comprobación `:408-412`, recarga `:486-501`, confirmación `:570-579`, Conservar `:649-664`, descartar `:730-745`). Ninguna fuera de los seis cuerpos. Se conserva la alternativa `result` de `BaseOrReadUse` (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:645`, autoprueba `:773`). Detalle menor en R-M5 |
| I-1 | **Parcial** | La forma de `PassGate` ya es la real, con el literal (`WorkspaceHostCore.cs:1364-1367`), y es la única `Authorize(` de su cuerpo. Se añade la mutación del documento. Pero la cláusula del Plugin falla sobre el código real: **R-B1** |
| I-2 | Sí | Las reglas 1 y 2 van por token en todos los `.cs` de `src/RackCad.UI/`. Hoy ningún archivo de la UI nombra `TryConfirmBase`, `TryKeepMine` ni `TryDiscardAndReload`. `\bCheckNow\b` no coincide con `TryCheckNow`. Se piden las autopruebas de la llamada sin calificar, `this.`, el grupo de métodos, el navegador y el intercambio |
| I-3 | Sí | «Conservar» solo en Conflict con `IsProtected` y `PendingText == null`. Sin pendiente, `IsDirty` (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:298`) es «estado actual distinto del inicial»; como el piloto solo edita la escala, significa «la escala fue editada». Confirmado: el núcleo admite editar y comprometer en Conflict (`DraftSession.cs:331-332`, `CanEdit` incluye Conflict; `CommitField`, `WorkspaceHostCore.cs:922-950`), así que el «aplícalo» de A2-05b se puede hacer. A2-22 ya es veraz, salvo el caso declarado de `IsDirty` que lanza |
| I-4 | **Parcial** | Están el diagnóstico persistente, la caja de solo lectura y la pista que lleva el host. Pero D-T4A2-01 y D-T4A2-06 se contradicen, y el borrado al principio de «Comprobar ahora» deja un estado más limpio si la comprobación no lee: **R-I1** |
| I-5 | Sí | La tabla de `TryCheckNow` (§3), A2-09 neutro, la limitación declarada (H-21; guardas `WorkspaceEditIntentGuardTests.cs:681` y `:696-697`) y la asignación a F4-T4B (§8) |
| M-01 | Sí | `UnreadableDiagnostic`, por la regla 7 (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:192-194`) |
| M-02 | Sí, con matiz | Ver R-M6 |
| M-03 | Sí | A2-01 exige que la vista releída esté en Clean o Dirty. Ver R-M8 sobre el texto del resto de los estados |
| M-04 | Sí | A2-23 y el rótulo declarado, con la gemela `tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:738` |
| M-05 | Sí | A2-09 |
| M-06 | Sí | Con una elección pendiente, la caja es de solo lectura. `CommitPending` sale antes de `ClearNotices` si `!EditableOf(view)` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:248-251`), así que `LostFocus` ya no retira la elección |
| M-07 | Sí | La instantánea, la correspondencia del resultado con la elección, cero llamadas a la otra operación y la retirada en `Apply`. Ver R-M2 |
| M-08 | Sí | Ver R-M8 |
| M-09 | Sí | Ver R-B1 |
| M-10 | Sí | La regla 3 ata `Content`, `AutomationProperties.Name` y `Click`. Ver R-M7 sobre los otros tres botones |
| M-11 | Sí | El enumerado privado y C06 limitado |
| M-12 | Sí | Los ocho nombres y C09 |
| M-13 | Sí, con matiz | Ver R-M4: H-20 nombra un tercer archivo equivocado |
| M-14 | Sí | |
| M-15 | Sí | D-T4A2-11 coincide con el precedente `WorkspacePilotCloseSyncGuardTests.cs:964-984` |
| M-16 | Sí | |

---

## 2. Lo nuevo de la v2: comprobaciones sin hallazgo

**D-T4A2-04** (forma de `EditConfirmOutcome` y las dos líneas subidas a variables locales): en todo `tests/` no hay nada que lo rompa.
- **Reflexión:**
  - `WorkspaceEditIntentHostTests.cs:982-998` solo mira los espacios de nombres de los tipos;
  - `WorkspacePilotEditGuardTests.cs:180-218` exige propiedades de solo lectura en los tipos de valor del puerto, y `{ get; }` lo cumple;
  - `WorkspacePilotPrepareTests.cs:1320-1356` sigue en verde: `Confirmed("why")` y `Conflict(new[] {...})` siguen siendo válidas, y nadie lee `ClassicEditorHint` del resultado de la confirmación;
  - no hay grupos de métodos sobre las fábricas.
- **Cuentas:** ninguna prueba cuenta `ClassicEditorHint` ni el identificador `admission` en el núcleo. Las apariciones de `ClassicEditorHint` en las pruebas son de `EditIntentOutcome`: `WorkspaceEditIntentHostTests.cs:126`, `:221`, `:307` y `:326`, y `WorkspaceLifecycleHostTests.cs:1208`.
- **Las dos líneas exactas conservan las cuentas existentes:**
  - `WorkspaceEditIntentGuardTests.cs:693-695`: 1 del patrón y 2 de `pilot.Read`, y cero tras quitar los consumidores;
  - la exención (`:658-659`) reduce la primera línea a `var admission = ;`;
  - `pilot.ClassicEditorHint` está en la lista de M-2.
- **El orden:** `AssertInOrder` (`:751`) termina en `.ConfirmBase(` y las líneas van después.
- **La pista no puede faltar:** con el diagnóstico de admisión no nulo, la pista no es nula, porque una lectura no admitida siempre trae diagnóstico (`WorkspaceDraftPilotIntentReadTests.cs:988`).
- **H-20:** las prohibiciones con comentarios incluidos se aplican a `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs` (`WorkspaceEditIntentGuardTests.cs:16-18` y `:935-945`). Los resúmenes nuevos propuestos no contienen ninguna palabra prohibida. Hay una errata en la lista de archivos (R-M4).

**D-T4A2-03 y D-T4A2-06** (caja de solo lectura con una elección o un diagnóstico persistente): ninguna prueba existente cambia de comportamiento, porque los dos estados solo los crean operaciones nuevas.
- **Las pruebas de Conflict editable siguen igual:**
  - `WorkspacePilotFieldViewModelTests.cs:1002-1014` (Conflict editable);
  - los `LostFocus` de `WorkspacePilotFieldViewTests.cs:415` y `:419`;
  - `WorkspacePilotFieldViewModelTests.cs:954`.
- **Las de NotAdmitted de `BeginEdit`** (`WorkspacePilotFieldViewModelTests.cs:569`, `:648` y `:1220`; `WorkspacePilotFieldViewTests.cs:302` y `:631`) no tocan el campo persistente, y `NotEditableText` prefiere el diagnóstico de `BeginEdit`.
- **Hay que excluir explícitamente «Editar» y la apertura por gesto** (`CanOpenByGesture`, `_showEdit`, `:113` y `:356-357`). Con la caja de solo lectura, `openable` pasaría a verdadero. El paquete lo dice para los dos estados, y es correcto: `BeginFieldEditFromBox` y `BeginFieldEditFromTab` del navegador se filtran por `CanOpenByGesture` (`WorkspaceBrowserViewModel.cs:577-593`).

**Las reglas 1 a 4 y M-2** no congelan T4B ni T4C:
- la regla de `pilot` no fija cuentas;
- «Comprobar ahora» y «Descartar y recargar» tras Actualizar reutilizan los mismos `CheckNow` y `ConfirmChoice`;
- la regla 5 no exige unicidad (pero ver R-B1).

**§5 y contrato §6:** la trazabilidad de los diez puntos es correcta, INV-01 a INV-17 son coherentes con las decisiones, C01 a C09 están completos y `RequiredTests` no cambia.

---

## 3. Hallazgos

### BLOQUEANTE

**R-B1. La cláusula del Plugin de la regla 5 no se cumple en el código real.**
- **Dónde:** el paquete, D-T4A2-07, regla 5, segunda viñeta: «en `WorkspaceNavigationHost.cs`, **toda** aparición de `_authorize(` sigue la forma `var decision = _authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64());`, seguida de `if (decision != RequestDecision.Accepted)` y del Rejected».
- **Evidencia:** `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs` tiene **tres** llamadas a `_authorize(`, y dos no siguen esa forma:
  - `:120`, en la lectura del resumen: `var decision = _authorize(origin, document.UnmanagedObject.ToInt64());`;
  - `:165`, en la ejecución de la navegación: `var decision = _authorize(request.Origin, document.UnmanagedObject.ToInt64());`, sin `if`, porque la decisión pasa a `_runner.Run(...)`;
  - solo `:234`, en `PilotIntentReader.ReadOnActiveDocument`, la sigue.
- **Por qué importa:** la guarda falla en el RED y en el GREEN. Cumplirla exige cambiar el Plugin, que el contrato prohíbe («Sin cambio de producto en el Plugin», C04). Es STOP o una desviación, el mismo tipo de defecto que el B-1 de la v1, introducido al corregir I-1.
- **Corrección:**
  - Limitar el alcance a la clase anidada `PilotIntentReader` («toda aparición de `_authorize(` dentro de la clase `PilotIntentReader`»), donde vive la lectura del piloto. Así se conserva la ausencia de unicidad para una lectura nueva de T4B en esa clase.
  - Las autopruebas inyectan la mutación dentro de `ReadOnActiveDocument`.
  - Añadir una autoprueba positiva: el archivo real pasa aunque contenga `:120` y `:165`.

### IMPORTANTE

**R-I1. El diagnóstico persistente: el contrato se contradice, y una comprobación que no lee deja el estado más limpio.**
- **Dónde:**
  - D-T4A2-01, `CheckNow()`, paso 2: «`ClearNotices()`, que también retira el diagnóstico persistente de la superficie (D-T4A2-06), porque la comprobación nueva lo sustituye»;
  - D-T4A2-06: «**no lo borra `ClearNotices`** … Solo lo quitan una comprobación nueva (que pone el suyo o ninguno), …».
- **Evidencia:**
  1. Las dos frases se contradicen sobre quién lo borra. Leída al pie de la letra, D-T4A2-01 pondría el borrado dentro de `ClearNotices`, y entonces una tecla lo borraría, que es justo lo que D-T4A2-06 prohíbe.
  2. Aunque se interprete como «`CheckNow` lo borra además de llamar a `ClearNotices`», el borrado ocurre **antes** de leer. Si la comprobación nueva no lee, el diagnóstico desaparece:
     - responde `Waiting`, por un comando en curso o un modal;
     - responde `Rejected`, `Unavailable`, `Failed` o `NotFound`;
     - o la lectura lanza (`EnterFailure`).

     Entonces la caja vuelve a ser editable y se ofrecen otra vez las opciones, sin haber releído nada.
- **Por qué importa:** contradice el propio §3, «Sin falso limpio: … ni un estado más limpio que el anterior». También contradice la orden: «Unknown/error remains conservative». Y deja abierto qué implementa el Worker: un texto contradictorio es un riesgo de STOP o de desviación.
- **Corrección:**
  - Escribir que `ClearNotices` **nunca** toca el diagnóstico persistente.
  - `CheckNow` lo **sustituye** solo con un resultado que releyó: `Confirmed` o `Conflict`, que ponen el suyo o ninguno, y `Recovered` o `Conflict` en la vía `TryCheckNow`.
  - Con cualquier otro estado o con una excepción, el anterior se conserva y la caja sigue de solo lectura.
  - Añadir una prueba de UI: diagnóstico persistente, luego «Comprobar ahora» responde `Waiting` (y otra con una excepción del puerto); el diagnóstico sigue, la caja sigue de solo lectura y no se ofrecen las opciones.

### MENORES

- **R-M1. El diagnóstico persistente sobrevive a la desaparición del borrador.** Ocurre tras «Descartar borradores de este documento» del navegador (`Field.Rematerialize()`, `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:484`) o tras RACKPANELDESCARTAR con la misma superficie. Como D-T4A2-06 dice que «mientras existe» no se ofrece «Editar» ni la apertura por gesto, el rack queda sin borrador y sin «Editar» hasta cambiar de pestaña. Además, `NotEditableText` muestra el diagnóstico mientras `StateOf` dice «Sin borrador.» (`WorkspacePilotFieldViewModel.cs:370`). Corrección: retirarlo en `Apply` cuando la vista no tiene borrador.
- **R-M2. La retirada en `Apply` con A2-34 necesita dos precisiones.**
  - La oferta que se comprueba **no** incluye la cláusula «sin una elección pendiente» de D-T4A2-02. Si no, toda elección se retiraría sola en el siguiente `Apply`.
  - Con `Fail` (`EnterFailure` llama a `Apply`, `:405-416`) y con `Clear` (`ResetSurfaceState` antes de `Apply(default)`), la retirada es silenciosa y no pone A2-34.
- **R-M3. `RequestDiscardAndReload` y `RequestKeepMine` «no llaman al puerto» pero «leen la vista de memoria».** En el resto del paquete, «leer la vista de memoria» es `GetField`, que es una operación del puerto. Hay que precisar si usan `_view`, la última vista presentada, o si admiten `GetField` (que no lee el dibujo), y escribir la prueba en consecuencia.
- **R-M4. H-20 nombra mal un archivo.** Cita `IWorkspacePilotEditPort.cs`, pero la guarda (`WorkspaceEditIntentGuardTests.cs:17` y `:937`) cubre `IWorkspaceEditIntentReader.cs`. No afecta a la lista blanca; solo hay que corregir el hecho.
- **R-M5. Regla de `pilot` de M-2.** Escribir el `\b` tras el miembro (`pilot\.(?:StaleBase|Read|PreviewWitness|Detail|ClassicEditorHint)\b`), para que un `pilot.ReadX` hipotético no se quite a medias.
- **R-M6. Regla 4.** Una guarda de texto solo detecta la concatenación `"Sin " + "cambios"` con una expresión específica para ella, por ejemplo `"Sin\s*"\s*\+\s*"\s*cambios`, y no detecta cualquier partición. Conviene:
  - declarar la expresión concreta;
  - añadir a la prohibición la segunda mitad de A2-01 («el dibujo coincide con la base del borrador»);
  - decir que la teoría del §5.2, fila 8, es la defensa de fondo.
- **R-M7. Los tres botones nuevos de la reconciliación** («Comprobar ahora», «Descartar y recargar» y «Conservar mi borrador») no tienen fijados los nombres de sus manejadores ni de sus `Click=`. La regla 2 exige «su manejador exacto» sin decir cuál. Además, ninguna guarda de fuente ata su `Content`, `Click` y `Visibility`; la regla 3 solo cubre Sí y No. Hay que fijar los nombres, por ejemplo `OnBrowserFieldCheckClick`, `OnBrowserFieldDiscardAndReloadClick` y `OnBrowserFieldKeepMineClick`, y decir si los cubre la regla 3 o solo las pruebas de vista.
- **R-M8. «`Confirmed` sin diagnóstico, con la vista en otro estado: el prefijo por defecto y A2-08»** da «No se pudo comprobar: sin detalle. No se comprobó.». Es falso, porque sí se releyó y se confirmó. Hace falta un texto propio, por ejemplo A2-34 o «Comprobado, pero el borrador cambió durante la comprobación».
- **R-M9. La fila 6 del §5.2 dice «amplía `:963`», sin la precisión «en una prueba nueva» de la fila 15.** `WorkspacePilotPrepareTests.cs` solo admite el cambio de `:993`, así que editar `:963` sería C02.
- **R-M10. Renombrar la prueba de `:993`.** D-T4A2-04 permite cambiar su nombre (la declaración está en `:978`), pero el alcance dice «solo en `:993`», y cambiar el nombre complica comparar la población de pruebas entre el RED y el GREEN. Hay que fijar el nombre nuevo exacto o no renombrarla.
- **R-M11. Al teclear un pendiente tras A2-05, el botón «Conservar» desaparece sin explicación.** `ClearNotices` borra A2-05 y nada pone A2-05b. Conviene que `ReconcileText` lleve la indicación de A2-05b en Conflict con pendiente.

---

## 4. ¿Se puede emitir sin una tercera revisión?

**R-B1 y R-I1 siguen siendo un bloqueante y un importante**, así que la regla del Owner («incorporar todos los bloqueantes e importantes») exige corregirlos antes de emitir. Las dos correcciones son estrechas y se pueden verificar de forma mecánica:
- **R-B1:** limitar la regla 5 a la clase `PilotIntentReader`. Se verifica con un grep de `_authorize(` en `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`: solo `:234` queda dentro de la clase.
- **R-I1:** volver a escribir el paso 2 de D-T4A2-01 y la viñeta de D-T4A2-06 con la regla «solo un resultado que releyó sustituye el diagnóstico; ningún otro lo borra», y añadir la prueba.

**Recomendación:** basta una comprobación focalizada de esos dos puntos (leer las dos frases corregidas y repetir el grep), sin una tercera revisión completa. Los menores R-M1 a R-M11 los puede aplicar la sesión sin otra revisión.

---

## 5. Comprobación focalizada de la v3

**Alcance:** solo lo pedido.
- R-B1: la regla 5 de D-T4A2-07, más el grep de `_authorize(`.
- R-I1: D-T4A2-01, pasos 2 y 5, y las viñetas de D-T4A2-06.
- Un repaso del §10.2, de los textos nuevos A2-14 y A2-35, y de la tabla de botones (nombres de manejadores y enlaces de `Visibility`), contra las guardas y pruebas existentes.

**Condiciones:** HEAD `b9e1ed5a`, solo lectura, sin compilar ni ejecutar.

### 5.1 R-B1: resuelto

- **El grep, repetido** en `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`, da tres `_authorize(`: `:120`, `:165` y `:234`.
- **La clase anidada `PilotIntentReader`** va de `:196` a `:254` (el archivo tiene 256 líneas). Dentro solo está `:234`, y sigue exactamente la forma exigida, con `if (decision != RequestDecision.Accepted)` y el Rejected en `:235-236`.
- **La regla 5 de la v3** limita la cláusula a esa clase, deja fuera de forma explícita `:120` y `:165`, inyecta las mutaciones dentro de `ReadOnActiveDocument` y añade la autoprueba positiva sobre el archivo real. Ya se puede cumplir sin tocar el Plugin.
- **La cláusula de `PassGate`** no cambia y sigue siendo correcta: `WorkspaceHostCore.cs:1364-1367`, única `Authorize(` del cuerpo.

### 5.2 R-I1: resuelto

- **La contradicción desaparece:**
  - el paso 2 de D-T4A2-01 dice «`ClearNotices()`. **No** toca el diagnóstico persistente»;
  - D-T4A2-06 dice «`ClearNotices` nunca lo toca».
- **Solo un resultado que releyó lo sustituye**, en los dos textos y con la misma lista: `Confirmed` o `Conflict` de `TryConfirmBase`, y `Recovered` o `Conflict` de `TryCheckNow` (paso 5 y la viñeta de D-T4A2-06).
- **Los demás resultados lo conservan, con la caja de solo lectura:** `Waiting`, `Rejected`, `Unavailable`, `NotFound`, `NotApplicable`, `Failed` y una excepción. Coincide con el §3 («un error nunca deja la superficie más limpia»).
- **La prueba pedida está:** §5.2, fila 6, con «una comprobación que responde Waiting o lanza conserva el diagnóstico».
- **R-M1 queda incorporado sin contradicción:** `Apply` lo retira cuando la vista no tiene borrador.

### 5.3 Repaso de lo nuevo: sin bloqueantes ni importantes nuevos

**Los cinco botones** (tabla de T-A2 y regla 3) no chocan con ninguna guarda existente:
- **`Content`:** la gemela de G-04 (`tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:737-740`) y G-04 (`tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:339-341`) cuentan el texto con la comilla de cierre, y ninguno de los cinco coincide con «Editar», «Recargar», «Volver a leer» ni «Descartar».
- **`Click`:**
  - `Click="OnBrowserFieldDiscardAndReloadClick"` no contiene `Click="OnBrowserFieldDiscardClick"` ni `Click="OnBrowserFieldReloadClick"`;
  - en `CheckHandlers` (`WorkspacePilotCloseSyncGuardTests.cs:781-795`), `OnBrowserFieldDiscardClick\s*\(` no coincide con `OnBrowserFieldDiscardAndReloadClick(`;
  - las autopruebas por `Replace` de `:666-692`, `:744-756` y `:766-779` siguen fallando donde deben.
- **`x:Name`:** `CheckNewXamlElements` (`:614-663`) busca `x:Name="BrowserFieldDiscardButton"` con la comilla, así que `BrowserFieldDiscardAndReloadButton` no colisiona.
- **El token de la regla 2:** `\bCheckNow\b` no coincide con el nombre del manejador `OnBrowserFieldCheckClick`.
- **`Visibility`:** los enlaces declarados (`Browser.Field.ShowCheckButton`, `ShowDiscardAndReloadButton`, `ShowKeepMineButton`, y `HasChoice` en el panel) usan el convertidor `FlagToVisibility` de sus vecinos.

**Los textos A2-14 y A2-35:**
- no llevan el vocabulario prohibido (`WorkspacePaletteHostSourceGuardTests.cs:583-601`);
- no contienen «Sin cambios» ni la segunda mitad de A2-01, así que no tropiezan con la regla 4.

**§10.2:** las 13 disposiciones están reflejadas en el texto de la v3:
- D-T4A2-03: R-M2 y R-M3;
- D-T4A2-06: R-M1 y R-M11;
- H-20: R-M4, ya con `IWorkspaceEditIntentReader.cs` y las líneas correctas `:30-32`;
- M-2: R-M5;
- regla 4: R-M6;
- reglas 2 y 3, y la tabla: R-M7;
- A2-35: R-M8;
- §5.2, fila 6: R-M9;
- D-T4A2-04 y el contrato: R-M10.

### 5.4 Menores nuevos (no bloquean la emisión)

- **F-M1. La regla 4 debe ser ordinal y distinguir mayúsculas.** `src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:797` y `:811` contienen «Cabecera sin cambios.», en minúscula. Una implementación que no distinga mayúsculas fallaría sobre un archivo fuera del alcance. El texto del paquete («Sin cambios») ya pasa con la comparación ordinal por defecto, pero conviene escribirlo, o limitar la regla a `src/RackCad.UI/Workspace/`.
- **F-M2. Las condiciones de A2-14.** El texto («… aplícalo o corrígelo antes») se añade en Conflict con pendiente aunque la caja sea de solo lectura por un diagnóstico persistente o por una elección pendiente. En esos casos el usuario no puede corregirlo. Conviene omitirlo en esos dos casos.
- **F-M3. A2-35 y la vista sin borrador.** A2-35 («… vuelve a comprobarlo») puede salir con la vista releída sin borrador, y entonces «Comprobar ahora» no se ofrece. Es una imprecisión de redacción: basta con «… el borrador cambió durante la comprobación.» sin la invitación, o decirla solo si el borrador sigue existiendo.
- **F-M4. La regla 3 y la `Visibility` de Sí y No.** La regla pide la `Visibility` de «cada botón», pero para Sí y No la tabla da la del panel `BrowserFieldChoicePanel`. Conviene precisar que en esos dos se comprueba el panel.
- **Errata de esta re-revisión:** en los §2 y §3 (R-M4) cité `WorkspaceEditIntentGuardTests.cs:16-18` y `:17` para las constantes de las rutas. Las líneas correctas son `:30-32`, como dice la v3. No cambia ningún hallazgo.

### 5.5 Veredicto sobre la v3

**R-B1: resuelto. R-I1: resuelto. No hay bloqueantes ni importantes nuevos.**

**Veredicto: EMITIBLE**, con cuatro menores nuevos (F-M1 a F-M4). La sesión puede aplicarlos al emitir, sin otra revisión.
