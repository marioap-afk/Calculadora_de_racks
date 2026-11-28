# I-64 F4-T4A2-PREPARE-UI — Revisión adversarial independiente del código

**Objeto:** `git diff 4eb7e924..9333396d` (BaseSha `4eb7e9247c3ad942feea1b6a695d7ad79377a4ee`, RED `1fe36cf9cddc2bb67f1795175c2e818e6569e177`, GREEN `9333396dcd0fd7bfc899e3d5c35ad1158d5e9b7c`), rama `architecture/workspace-persistente-rackcad`.

**Autoridad de diseño:** `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md` (v3, §2 a §6 y la tabla T-A2) y `docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/coordinator/gate-contract.json` (INV-F4T4A2-01..17, F4T4A2-C01..C09). Las afirmaciones del Worker (`artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T145121Z-eba2/worker-handoff.json`) se trataron como afirmaciones a verificar.

**Método:** solo lectura. Se leyeron el diff completo, el VM final, el host, el XAML y su código subyacente, el doble, las cinco clases de prueba nuevas, los cambios de las guardas existentes, el navegador (para la interacción con el campo), el puerto y el contrato. Se revisaron los TRX del RED (tipo de fallo). No se compiló ni se ejecutó nada; la CI en verde sobre el SHA exacto se toma como dato.

---

## 1. Lo comprobado sin hallazgo

- **Lista blanca (C01).** El diff toca exactamente las 15 rutas de `AllowedWriteScope`, con los cinco archivos nuevos declarados. RED→GREEN solo cambia `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs` y `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`; ningún archivo de `tests/` cambia entre RED y GREEN. El árbol queda limpio.
- **Cambio del host (D-T4A2-04, INV-10).** `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:575-576` tiene exactamente las dos líneas del paquete, justo después de `var confirm = draft.ConfirmBase(pilot.StaleBase);`, y `:581` y `:585` usan `Confirmed(admission, hint)` y `Conflict(DescribeDifferences(confirm.Differences), admission, hint)`. Las firmas de las fábricas en `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:361-368` son las del paquete. `TryCheckPilotEdit` no cambia. Los resúmenes de M-8 (`:287-291`, `:300-303`) y de los dos campos (`:345-349`) están corregidos y los comentarios nuevos respetan H-20. No hay texto nuevo del host.
- **`WorkspacePilotPrepareTests.cs:993`.** Es la única línea que cambia en ese archivo, sin renombrar la prueba.
- **Cambios de pruebas existentes (C02).** Solo cambian las tres guardas declaradas y el doble. En las guardas solo hay adiciones, salvo dos líneas que amplían el patrón: `BaseOrReadUse` (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:647`) y el reconocimiento de asignaciones de `ErrorReason` (`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:259-270`). Ambos son estrictamente más amplios. Las autopruebas sintéticas previas de `WorkspaceCommandAdvisorGuardTests.cs` se conservan. En el doble (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`) solo se añaden cosas: `Reads()` con las tres lecturas nuevas, el resumen de `ReadHook`, `CheckNowResponder`, con la respuesta previa por defecto, y los tres parámetros opcionales de `View`. No se ha debilitado ninguna aserción.
- **Esqueleto RED (INV-14).** Los once miembros públicos nuevos del VM son `false`, `string.Empty` o cuerpos vacíos (diff 4eb7e924..1fe36cf9). El host no usa la propiedad ni los parámetros nuevos. Los fallos del RED son todos de aserción: en Core, 10 (`Assert.Equal`, `Assert.Empty` y `Assert.Contains` de las precondiciones de `Mutate`); en UI, 95 (`Assert.Equal`, `Single` y `True`). Ninguno se debe a una excepción del esqueleto.
- **Límites (INV-17, C03-C05).** No se añade `Window`, diálogo, `EnterPanelModal`, teclado, foco, `Dispatcher`, `Task`, `+=`, `event`, `delegate` ni estado estático (los `static` nuevos son métodos puros), ni `IsDefault`/`IsCancel` o un segundo `PreviewKeyDown`. No hay ninguna API de escritura ni de transacción. El navegador, el panel, la navegación, el Plugin y `Drafts/` no cambian. No aparece vocabulario prohibido en el `.cs` ni en el `.xaml`.
- **Textos T-A2.** Las 27 constantes de `WorkspacePilotFieldViewModel.cs:39-75` se compararon carácter a carácter con la tabla, junto con los prefijos de comprobación (`:921-947`) y de elección (`:950-962`). Todos coinciden. El XAML (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml:234-268`) tiene los cinco botones con su `x:Name`, `Content`, `AutomationProperties.Name`, `Click` y `Visibility` de la tabla. Sí y No están dentro de `BrowserFieldChoicePanel`, en ese orden y sin `Visibility` propio. Los manejadores de `WorkspacePanelView.xaml.cs:89-97` tienen la forma exacta.
- **Ofertas y despacho (D-T4A2-01/02, INV-03/06).**
  - `OffersCheck` (`:592-607`): Clean, Dirty, Conflict y Unknown/CommitThrew, en una superficie fijada, sin fallo y sin elección. Se ofrece aunque haya un diagnóstico persistente.
  - `OffersChoice` (`:610-624`): Descartar en Conflict y en Unknown/PostReread; Conservar solo en Conflict con `IsProtected` y `PendingText == null`. Las dos exigen que no haya diagnóstico persistente.
  - El despacho (`:409-430`) envía `TryCheckNow` en Unknown/CommitThrew, `TryConfirmBase` en Clean, Dirty y Conflict, y usa `new EditIntentRequest(session, rack, null, null)`.
- **Tablas del §3 y «sin falso limpio» (INV-04).**
  - En `ConfirmNoticeOf` (`:688-706`), A2-01 sale solo de Confirmed sin diagnóstico y con la vista releída en Clean o Dirty. A2-02 sale en Conflict, A2-35 en otro estado y A2-37 sin borrador. Con diagnóstico salen A2-03 y A2-06. En Conflict sin diagnóstico, el orden es A2-05b, A2-05 y A2-04. El resto lleva prefijo y A2-08.
  - En `CheckNoticeOf` (`:723-734`), A2-07 sale solo de Recovered.
  - La única excepción es la ruta defensiva de M-1.
- **Elección (D-T4A2-03, INV-05).** Se comprobaron:
  - la instantánea completa (`:639-647`);
  - la revalidación de los cinco campos y de la superficie, que con una diferencia da A2-34 sin llamada (`:471-486`);
  - una sola llamada según el tipo (`:489`);
  - la correspondencia del resultado con la elección (`:741-758`);
  - `CancelChoice` sin puerto (`:505-512`);
  - la retirada con `ClearNotices` (`:534-541`), en silencio con `Show` de otra superficie, `Clear` y `Fail` (`:521-531`, `:859`) y con A2-34 en `Apply` (`:797-802`), usando la oferta sin la cláusula de la elección;
  - la caja de solo lectura y los demás gestos ocultos mientras hay una elección pendiente (`:579-586`, `:806-807`);
  - que `CommitPending` sale sin hacer nada con la caja de solo lectura, de modo que perder el foco no compromete el pendiente.
- **Diagnóstico persistente (D-T4A2-06, INV-09).** Tiene campos propios (`:102-103`) que `ClearNotices` no toca. Solo lo sustituyen Confirmed o Conflict de `TryConfirmBase` (`:675-679`) y Recovered o Conflict de `TryCheckNow` (`:711-715`). Lo quitan Reloaded y Kept (`:741-750`), el cambio de superficie y `Clear` (`:526-527`), y `Apply` cuando no hay borrador (`:791-795`). Con Waiting, Rejected, Failed o una excepción se conserva. `NotEditableText` y `ClassicEditorHintText` priorizan el diagnóstico de `BeginEdit` (`:227-230`).
- **`ReconcileText` (`:766-784`).** Lleva A2-10 con «sin detalle.», ` Causa: ` solo si la causa no está en blanco y A2-14 solo con pendiente, sin diagnóstico persistente y sin elección pendiente. También A2-11 y A2-12; en cualquier otro caso queda vacío.
- **C2-M-1 y C2-M-2 (INV-11).** Las 15 mutaciones de map-2 B.3 están en `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs:132-140` (M2a-M3c) y `:220-225` (M1a-M1f), cada una con un solo `Replace` del archivo real y con `Assert.Contains` previo. Las formas exactas coinciden con H-17. Se conservan la exclusión de `Result` y el único `GetKeywords`.
- **M-2, M-4 y M-9.** Las autopruebas de M-2 (`WorkspaceEditIntentGuardTests.cs:805-855`) discriminan, y la regla nueva se apoya en la guarda existente de `.PilotRead` (`:717-728`), que cierra los alias por otra variable. Cada forma de M-4 es discriminante: sin su patrón, el texto mutado no tendría violación porque la mutación quita `ErrorReason = null;`.
- **Arrastre (5) (INV-15).** `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs:52-72` compara con ordinal y sin normalizar, y exige `\r\n` en el diseño interior y en el persistido. No es vacía en Windows.
- **Inversión de Sí/No (prueba 17).** Si se intercambian Sí y No en el VM, en el XAML (`Click`, `Name`, `Content` o `x:Name`) o en el código subyacente, falla al menos una prueba de VM, de vista o de fuente.

---

## 2. Hallazgos

### IMPORTANTE

**I-1. La positiva de la regla 5 exige que la llamada sea única en `PilotIntentReader` y fija la cuenta del archivo, contra INV-F4T4A2-12. Esto congela T4B.**

- **Evidencia:** `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs:461-462`:
  - `Assert.Equal(1, Src.Count(Src.BodyOf(host, ...PilotIntentReader...), @"\b_authorize\s*\("));`
  - `Assert.Equal(3, Src.Count(host, @"\b_authorize\s*\("));`
- **Qué dice el contrato:**
  - el paquete (`docs/initiatives/I-64-f4-t4a2-gate-contract-package.md:387`): «no se exige unicidad, para no congelar una lectura nueva de F4-T4B en esa clase»;
  - INV-F4T4A2-12: «sin exigir unicidad»;
  - la disposición I-1 de la revisión de la v1 (`:684`): «sin unicidad en el lector».
- **Por qué importa:**
  - La función de guarda (`:447-453`) sí cumple, y la autoprueba `:482-491` demuestra que tolera una segunda llamada. Pero la prueba positiva del archivo real falla en cuanto T4B añada su relectura propia del llamador o la de MUTATE (§8 del paquete, `:658`) dentro de `PilotIntentReader`, o cualquier otra llamada a `_authorize` en `WorkspaceNavigationHost.cs`.
  - Es justo la congelación que el paquete eliminó a propósito, y obligaría a declarar un cambio de guarda en T4B (C02 de T4B).
  - El handoff presenta INV-12 como cumplido (mapa por invariante, `worker-handoff.json`, «Evidence», INV-12) sin mencionar esas dos cuentas.
- **Corrección** (dentro del alcance; el archivo es nuevo de esta tarea, así que en un intento de corrección no es una «guarda existente» según C02):
  - quitar la aserción `Equal(1, …)`;
  - sustituir `Equal(3, …)` por una positiva no congelante, por ejemplo `Assert.True(Src.Count(reader, …) >= 1)` y `Assert.True(Src.Count(host, …) > Src.Count(reader, …))`, para que siga demostrando que las llamadas de `:120` y `:165` existen fuera de la clase y no disparan la regla.

### MENOR

**M-1. Ruta defensiva con falso limpio: si `GetField` lanza después de un Confirmed, sale A2-01 calculado con la vista anterior.**

- **Evidencia:** en `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:681-683`, `PresentConfirm` llama a `Render()` y luego calcula `_noticeText = ConfirmNoticeOf(outcome)`. Si el `GetField` de `Render` lanza (`:566-574`), `EnterFailure` hace `Apply(_view)` con la vista **de antes** de la comprobación (`:861`). `ConfirmNoticeOf` decide entonces A2-01, A2-02 o A2-35 sobre esa vista no releída. Con un borrador Dirty «posiblemente obsoleto», la superficie queda en «Error: …» y con la noticia «Sin cambios: el dibujo coincide con la base del borrador.».
- **Reproducción con el doble:** `ReadHook = _ => Port.Throws[GetFieldOp] = …` y `ConfirmBaseResponder = _ => Confirmed()`. Ninguna prueba lo cubre: las teorías del §5.2, fila 8, solo hacen lanzar a la operación (`WorkspacePilotReconciliationTests.cs:427-435`). Lo mismo pasa en `PresentCheck` (`:717-718`) y en `PresentChoice` (el aviso se fija antes de `Render`, `:741-761`), aunque ahí no hay falso limpio.
- **Por qué es menor y no importante:**
  - Contradice literalmente INV-04 («una excepción del puerto nunca da A2-01») y la condición «con la vista releída en Clean o Dirty».
  - Pero el contrato del puerto dice que `GetField` «never throws» (`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:56`), así que en producción la ruta solo existe si el puerto incumple su contrato.
- **Corrección:** en `PresentConfirm`, `PresentCheck` y `PresentChoice`, no presentar el resultado si `Render` dejó el campo en fallo (`if (_failure != null) return;` tras `Render()`, con `RaiseAll` y `_changed` según convenga), y añadir la prueba con el `ReadHook` indicado.

**M-2. Desviaciones de producto no declaradas: la rama A2-34 de `CheckNow` y `ReconcileText` vacío con un fallo.**

- **Evidencia:**
  - `WorkspacePilotFieldViewModel.cs:431-437`: si la vista leída en el paso 3 ya no está en un estado comprobable, `CheckNow` no llama al puerto y muestra A2-34. El paquete solo define A2-34 para la elección (D-T4A2-03) y el paso 3 no prevé esta rama.
  - `:768`: `ReconcileText` queda vacío con `_failure != null`, cuando el paquete dice que en Conflict siempre da A2-10.
  - Las dos son razonables y conservadoras, pero el handoff declara `Deviations: []`, y la primera solo se menciona dentro de «WorkCompleted» como «detalle conservador». Ninguna prueba cubre la rama `:431-437`.
- **Corrección:** declararlas en `Deviations` del handoff, o en el cierre, y añadir una prueba: vista mostrada en Clean, memoria del doble cambiada a Applying y `CheckNow()`, que debe dar cero llamadas de lectura y A2-34.

**M-3. Tras un descarte del navegador, la elección se retira, pero el A2-34 que promete el paquete no llega a verse.**

- **Evidencia:** el paquete (`:276`) dice que `Apply` retira la elección con A2-34 «por ejemplo tras un descarte del navegador». En el flujo real, `FieldDiscarded` hace `Rebuild` (→ `RenderField` → `Field.Show` → `Apply`, que pone A2-34) y después `Field.Rematerialize()` (`src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:960-961`, y lo mismo en `:483-484`). `Rematerialize` llama a `ClearNotices` (`WorkspacePilotFieldViewModel.cs:382`) y borra el A2-34. La prueba `WorkspacePilotReconciliationTests.cs:965-983` solo comprueba `HasChoice == false` y cero llamadas a `TryKeepMine`, no el aviso.
- **Por qué importa:** el resultado es seguro (no hay llamada y la elección desaparece), pero lo que ve el usuario difiere de la redacción del paquete, y nadie lo fija ni lo declara. El navegador no se puede tocar (C04).
- **Corrección:** fijar en la prueba el `NoticeText` real tras el descarte y declarar que en ese flujo la retirada es silenciosa por el `Rematerialize` del navegador.

**M-4. La regla 1 cuenta el token, no la invocación: una captura como grupo de métodos dentro del cuerpo correcto la evade.**

- **Evidencia:** `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs:70-92` exige una aparición de `\bTryKeepMine\b` dentro de `ConfirmChoice`. Un `_keep = _port.TryKeepMine;` (un campo `Func<…>`, que no es `delegate` ni `event`) dentro de `ConfirmChoice` e invocado desde `Render` pasaría la regla 1. Las guardas existentes del VM prohíben `delegate` y `+=`, pero no un campo `Func`, y el VM ya tiene un campo `Action`. La autoprueba de grupo de métodos solo existe para la regla 2 (`:182`).
- **Corrección:** exigir la forma de invocación (`_port\s*\.\s*TryKeepMine\s*\(\s*request\s*\)`, y lo mismo para las otras tres) y añadir la autoprueba `_keep = _port.TryKeepMine;` dentro de `ConfirmChoice`.

**M-5. Las autopruebas de la regla 4 no ejercen la rama que dicen probar ni siguen el método de D-T4A2-07, y la regla 3 no fija el orden del texto de la elección.**

- **Evidencia:**
  - **«La constante en el caso Failed»** (`WorkspacePilotReconciliationGuardTests.cs:387` y `:390`): renombra la etiqueta `case EditConfirmStatus.Confirmed:`, así que la guarda falla por «there is no case» (`:346`). La comprobación de sección (`:348-351`, «is not referenced inside») no tiene ninguna autoprueba que la haga fallar, por ejemplo moviendo `return UnchangedText;` al `default:` sin tocar la etiqueta.
  - **Las autopruebas de «mitades en otra parte»** (`:395-407`): concatenan texto a la carpeta en lugar de aplicar «un solo `Replace` sobre ese texto, precedido de `Assert.Contains(from, real)`» (paquete `:363`).
  - **La carpeta de la regla 4** (`:317-319`): solo incluye `WorkspacePanelView.xaml` de forma fija, no todos los `.xaml` de `src/RackCad.UI/Workspace/`. Hoy hay uno, pero un `.xaml` nuevo quedaría fuera.
  - **La regla 3** (`:274-281`): comprueba que Sí va antes que No, pero no que `BrowserFieldChoiceText` los precede, que es el orden del paquete (`:284`).
- **Corrección:** añadir una autoprueba que mueva la referencia fuera del caso Confirmed sin quitar la etiqueta; convertir las inyecciones de mitades en un `Replace` del texto real (por ejemplo, reemplazar el `Content="Comprobar ahora"` del XAML por `Content="Sin cambios"`); enumerar los `.xaml` de la carpeta; y comprobar el índice de `BrowserFieldChoiceText` frente al de Sí.

**M-6. La prueba 12 de la orden está cubierta a medias: «No» se prueba en dos de las tres elecciones.**

- **Evidencia:** el paquete §5.2, fila 12 (`:554`), pide «“No” de las tres elecciones». `WorkspacePilotReconciliationTests.cs:786-807` y `WorkspacePilotReconciliationViewTests.cs:238-261` solo prueban Descartar y Conservar sobre Conflict. «No» sobre la elección de Unknown/PostReread (A2-23) no se prueba (`:768-784` solo prueba «Sí»). El mapa del handoff da la prueba 12 por cubierta.
- **Corrección:** añadir el caso de Unknown/PostReread: «No» da cero llamadas, el borrador del doble queda igual, aparece A2-36 y A2-12 sigue en `ReconcileText`.

**M-7. Hay pruebas débiles, tautológicas o con nombres que prometen más de lo que comprueban.**

- **Evidencia:**
  - `WorkspacePilotReconciliationTests.cs:899-921` (`TheChoiceIsWithdrawnInSilence…`) no comprueba `HasChoice == true` antes de retirar la elección. Pasó en el RED inerte (no figura entre los 56 métodos fallidos) y seguiría pasando si las peticiones dejaran de crear la elección.
  - `WorkspaceDraftPilotCanonicalLineEndingTests.cs:44-50` es tautológica: comprueba la longitud, la distinción y el prefijo de su propio literal. No lo contrasta con el corpus de la caracterización vecina ni con los archivos.
  - `WorkspacePilotReconciliationTests.cs:229-242`: el nombre promete «the mark of staleness still comes from the view», pero no hay ninguna aserción sobre `StateText` ni sobre la marca, y `Clean(stale: false)` repite `Clean()`.
  - `WorkspacePilotReconciliationViewTests.cs:281-297`: el nombre dice «leaving the box», pero llama a `CommitPending()` directamente en lugar de provocar `LostFocus` en `BrowserFieldBox`, así que no ejerce el cableado del XAML que pide S2-R-09.
- **Corrección:**
  - añadir `Assert.True(…HasChoice)` tras cada `Request*`;
  - comparar el corpus con el `Canonical` de la caracterización o comprobar que existen los ocho archivos;
  - afirmar `"Modificado (posiblemente obsoleto)"` en el caso con la marca y quitar el duplicado;
  - provocar `UIElement.LostFocusEvent` sobre la caja real.

**M-8. `AssertHeld` comprueba «base, inicial, actual y pendiente intactos» solo en parte.**

- **Evidencia:** `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs:60-87` compara `InitialDraftState` solo por `.Origin` y `CurrentDraftState` solo por la escala. Una operación que sustituyera el estado inicial o el actual por otro con el mismo origen o la misma escala pasaría. La base sí se compara por referencia (`Assert.Same`). Afecta a las pruebas 5, 7, 8, 14 y 15 del Core.
- **Corrección:** guardar las referencias y comparar con `Assert.Same(before.Initial, draft.InitialDraftState)` y `Assert.Same(before.Current, draft.CurrentDraftState)`, como ya se hace con la base.

**M-9. Una cuenta exacta no declarada en M-3, y una nota de congelación del propio M-3 para T4B.**

- **Evidencia:** el paquete fija para M-3 solo `Assert.Equal(1, Src.Count(Core(), AdmissionDiagnosticPattern))` (`:407`). `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:865` añade además `Assert.Equal(2, Src.Count(core, @"\bpilot\.Read\.(?:IsAdmitted|Admission)\b"))` en todo el núcleo. Es redundante con las cuentas por cuerpo y con `BaseOrReadUse`, que ya prohíben ese uso fuera de la confirmación, y no está declarada.
- **Nota para el paquete de T4B, no atribuible al Worker:** el §8 del paquete (`:657`) asigna a T4B el diagnóstico de admisión en la vía `TryCheckNow`. Con el M-3 del paquete (una aparición en todo el núcleo) y con las cuentas en cero de `CheckBody`, T4B tendrá que declarar cambios de guarda o factorizar la expresión. Conviene preverlo en el paquete de T4B, junto con I-1.
- **Corrección:** quitar la aserción `:865` o declararla; y anotar en el paquete de T4B los cambios de guarda previstos.

---

## 3. Las 18 pruebas de la orden (§5.2) frente al código

| # | Cobertura real |
|---|---|
| 1 | Sí: `AMatchOfACleanOrDirty…` y `AMatchThatTheHostReported…` (UI), más las existentes del Core. |
| 2, 3, 4 | Sí: las tres pruebas nuevas de Confirm en `WorkspacePilotReconciliationHostTests.cs:117-170`, y `AConflictWithoutDiagnostic…` (UI). |
| 5 | Sí: el Core (`:176-224`) y la UI (`ANotAdmittedRead…`, `AKeyOnTheBox…`). La «tecla» se prueba por el *setter* y por gestos del VM, no por `PreviewKeyDown`. Es suficiente, porque la caja es de solo lectura. |
| 6 | Sí: el Core (`:228-262`) y la UI (`ADraftThatIsCleanAndPossiblyStale…` y `AnAnswerThatDidNotReread…`). |
| 7 | Sí: `:993` y las pruebas nuevas `:291-325`. |
| 8 | Casi: la lectura por defecto y la excepción dentro de `ConfirmBase` (Core), y las teorías de la UI. Falta la excepción del puerto en la relectura de memoria (M-1). |
| 9, 10, 11 | Sí: las existentes, más las de «Sí» y de la oferta de Conservar en la UI. |
| 12 | Parcial: dos de las tres elecciones (M-6). |
| 13 | Sí: el Core (`:380-428`) y la UI (comprobación byte a byte con pendiente inválido, Conservar no ofrecido y A2-20 más su sufijo). |
| 14, 15 | Sí: el Core (`:432-482`). En la UI, por las filas Rejected de las teorías. |
| 16 | Sí: las guardas existentes, la regla 1 y el doble sin escritura. |
| 17 | Sí: VM, vista y fuente; las inversiones se detectan. |
| 18 | Sí: C2-M-2 con sus mutaciones y la regla 5. Sin embargo, la positiva de la regla 5 se desvía de INV-12 (I-1). |

## 4. Afirmaciones del handoff que no se sostienen tal cual

- **INV-12, «sin exigir unicidad»:** la guarda no exige unicidad, pero la prueba positiva sí (I-1).
- **`Deviations: []`:** hay dos desviaciones de producto pequeñas sin declarar (M-2).
- **Prueba 12 cubierta:** solo se cubren dos de las tres elecciones (M-6).
- **«Las teorías de sin falso limpio cubren la excepción del puerto»:** solo cubren la excepción de la operación (M-1).
- **El resto de lo afirmado** se comprobó y es exacto: los 15 archivos, el RED inerte, el cambio exacto del host, las 15 mutaciones, los textos, la ausencia de cambios en el navegador, el panel, el Plugin y `Drafts`, y el arrastre (5).

## 5. Veredicto

**NO ACEPTABLE** (B/I/M = **0/1/9**).

- **El producto** (VM, XAML, código subyacente y el cambio del host) es correcto frente al paquete y al contrato, con los matices menores M-1 a M-3.
- **Lo único que impide aceptar es I-1:** la positiva de la regla 5 contradice el «sin exigir unicidad» de INV-F4T4A2-12 y congela una lectura nueva de T4B en `PilotIntentReader`. Es lo que el paquete descartó de forma expresa.
- **La corrección** cabe en una línea de dos aserciones, dentro de un archivo nuevo de esta tarea y sin tocar el producto. Con ella el veredicto pasaría a **ACEPTABLE CON MENORES**.
- **Los menores** se pueden aplicar en la misma corrección o dejarse declarados. Se recomienda al menos M-1 (una línea de producto más su prueba) y M-6.
