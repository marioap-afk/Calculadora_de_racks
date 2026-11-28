# I-64 F4-T4A1-PREPARE-CORE, intento 2: revisión adversarial independiente del código

## Objeto

- **Rama:** `architecture/workspace-persistente-rackcad`.
- **Diff revisado:** `443b5b3a..bbd9a35e`, 21 rutas, en dos commits:
  - el RED `921e3a36`, con el esqueleto y las pruebas;
  - el GREEN `bbd9a35e`, con la implementación.
- **Autoridades**, leídas desde Git:
  - `docs/automation/evidence/I-64-pilot/F4-T4A1-PREPARE-CORE/coordinator/gate-contract-attempt2.json`, con INV-F4T4A1-01..15 y F4T4A1-C01..C08;
  - `docs/initiatives/I-64-f4-t4-gate-contract-package.md` v3, en sus §2, §4 (D-T4A1-01..08) y §5;
  - `docs/automation/evidence/I-64-pilot/F4-T4A1-PREPARE-CORE/1/R20261114T023526Z-9d4c/analysis.md`;
  - F4-E25, F4-E51 y F4-E52, en `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md`.
- **Entrega del Worker**, usada solo como pista: `artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/2/R20261114T033524Z-deb3/worker-handoff.json`. De ella sale la lista de pruebas que fallan en el RED, con la que se mide qué pruebas discriminan.

## Método

- **Solo lectura.** Se usaron `git diff`, `git show` y `git grep` sobre el worktree, y se leyeron archivos. No se ejecutaron builds, pruebas ni AutoCAD, y no se modificó nada.
- **Comparaciones hechas:**
  - cada guarda cambiada, con su versión en `443b5b3a`;
  - el RED con el GREEN, en `git diff 921e3a36 bbd9a35e`, también solo sobre `tests/`;
  - el código nuevo, con las primitivas de RACKEDITAR (`src/RackCad.Plugin/RackPushBackCommands.cs:198-461`) y con `RackViewCodec`, `RackViewAvailability`, `RackProjectStore.PreflightInnerSources`, `RackProject.WithSourceMetadataFrom` y `DraftStaleBase`.
- **Citas:** todas las referencias `archivo:línea` son del commit `bbd9a35e`, salvo que se diga otra cosa.
- **Lo que no se repite**, porque ya lo midió la sesión: la CI, los recuentos y el alcance de las 21 rutas.

## Veredicto

**ACEPTABLE CON MENORES.** Recuento: **B 0 / I 0 / M 11.**

El comportamiento cumple el contrato en todos los puntos que se pidió mirar:

- la tabla de `ConfirmBase`, y el Conflict con MATCH sigue en Conflict;
- el Conservar atómico;
- los pasos 1-8 de las tres operaciones del host;
- el orden de los siete resultados de `Prepare`, con Conflict antes de la admisión;
- el plan inmutable, que nunca borra una vista;
- `Name` y el sobre salen siempre del sobre releído;
- los arrastres (1), (3) y (6);
- R-3;
- ninguna escritura authored.

No hay rodeos de guarda en el producto. Los hallazgos son menores:

- huecos en guardas nuevas o extendidas que un rodeo trivial todavía atraviesa;
- una prueba que no discrimina el arrastre (1);
- textos y documentación imprecisos;
- un campo del plan no declarado;
- dos desviaciones leves del orden o de la inercia del RED.

## Hallazgos

### M-1. La prueba del arrastre (1) no discrimina la corrección

- **Referencias:** `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs:767`, frente a `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs:210-215`.
- **Problema:**
  - `CarryOver1_TheRejectionComparesTheCurrentStateAsItIsAgainstACopyOfTheInitialOne...` pasa también con el código anterior, que copiaba `current` y le neutralizaba la escala. No está entre los cinco fallos del RED de esta clase que lista la entrega: solo fallan `CarryOver2` ×3, `CarryOver3` y `CarryOver6`.
  - El motivo es que las ediciones que prueba (`PalletTolerance`, `PostPeralte`, `RearTopeSaque` y la altura de una celda) sobreviven intactas a `PushBackPilotStateComparer.Copy`. Para estados bien formados, las dos formas son observacionalmente equivalentes, porque el copiador es «igual para el comparador» por construcción.
  - Aun así, el mapa de evidencia de la entrega cita `CarryOver1` como evidencia de INV-F4T4A1-11 (1).
- **Por qué importa:** si alguien deshace el arrastre (1) y vuelve a `Copy(current)`, ninguna prueba falla. El invariante queda escrito pero no fijado.
- **Corrección sugerida:**
  - añadir una guarda de fuente sobre el cuerpo de `ComposeForWrite`: que no aplique `Copy(` a `current`, que copie `initial`, y que compare con `current` tal cual. Debe llevar su autoprueba en negativo, sobre la mutación a `Copy(current)`;
  - o, si existe, un estado que `Copy` normalice;
  - y corregir el mapa de evidencia.

### M-2. La guarda antialias deja pasar alias de `pilot` y operadores nulos

- **Referencias:** `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:645`, `:715-742`.
- **Problema:**
  - `BaseOrReadUse` reconoce la lectura solo como `\b(?:pilot|result)\.Read\b`.
  - La guarda nueva `TheResultOfThePilotIsOnlyEverNamedPilotInTheHostCoreSoNoAliasEscapesTheGuardOfTheUseOfTheBase` impide renombrar `read.PilotRead`, pero no impide:
    - un alias del propio `pilot`, como `var p = pilot; ... p.Read.Admission.Diagnostic`;
    - `pilot?.Read`;
    - `pilot!.Read`.
  - Ninguna de las tres formas coincide con `BaseOrReadUse`, y ninguna nombra `.PilotRead`.
  - La autoprueba `TheAliasGuardDetectsARenamedWayToReachTheReadOrTheBaseInTheRealTextOfTheCore` solo inyecta `var other = pilot; var s = other.StaleBase;`. Ese caso lo detecta `.StaleBase`, que no depende del nombre, no el mecanismo antialias. Da por cubierto un caso que no lo está.
- **Por qué importa:**
  - INV-F4T4A1-04 (intento 2) prohíbe eludir una guarda con «alias, variable renombrada ni patrón de nombre». Un rodeo trivial, de la misma familia que la variable `reread` del intento 1, atraviesa todavía la guarda que se escribió para cerrarlo.
  - El producto no lo usa: en `WorkspaceHostCore.cs` solo hay `var pilot = read.PilotRead;`, en las líneas 327, 408, 486, 570, 649 y 730.
  - El hueco del alias de `.Read` ya existía en `443b5b3a`. No es una regresión.
- **Corrección sugerida:**
  - prohibir en el núcleo del host toda asignación cuyo lado derecho sea `pilot` (`=\s*pilot\s*[;,)]`);
  - ampliar `BaseOrReadUse` a `\bpilot\s*[?!]?\s*\.\s*Read\b`;
  - añadir autopruebas en negativo con `var p = pilot; var d = p.Read.Admission.Diagnostic;` y con `pilot?.Read.IsAdmitted`.

### M-3. La exención del diagnóstico de admisión es global y solo se cuenta en cinco cuerpos

- **Referencias:** `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:654-659`, `:693-697`.
- **Problema:**
  - `WithoutTheAllowedConsumers` borra `AdmissionDiagnosticPattern` de todo el texto del núcleo, y también de `IntentPath` y de `InterfacePath`.
  - El recuento por operación solo comprueba que hay uno en `TryConfirmPilotBase` y cero en los otros cuatro cuerpos de lectura.
  - Si un sexto método del núcleo usara esa misma expresión (por ejemplo una operación de T4B, o un auxiliar), la guarda del uso de la base no lo vería.
- **Por qué importa:** el contrato pide una admisión del uso «explícita y con un recuento por operación». El recuento no es total.
- **Corrección sugerida:** añadir `Assert.Equal(1, Src.Count(Core(), AdmissionDiagnosticPattern))`, o aplicar la exención solo dentro de `ConfirmBody()`.

### M-4. La guarda del invariante de R-3 en el núcleo tiene huecos de sintaxis

- **Referencia:** `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:259`.
- **Problema:** la expresión `(?<![\w.])ErrorReason\s*=(?![=>])\s*([^;]+);` no reconoce:
  - `this.ErrorReason = x;`, porque la mirada atrás excluye el punto;
  - `ErrorReason ??= x;`;
  - una deconstrucción del tipo `(ErrorReason, _) = (...)`.

  En cualquiera de esos casos, una razón asignada fuera del estado Error pasaría sin que la guarda la viera.
- **Por qué importa:** es la guarda de D-T4A1-06, y el contrato prohíbe patrones que esquiven una expresión regular. El código real no usa ninguna de esas formas: las asignaciones están en `DraftSession.cs:497`, `:551` y `:683`.
- **Corrección sugerida:**
  - admitir el prefijo `this.`;
  - tratar `??=` como asignación;
  - añadir autopruebas en negativo con esas formas.

### M-5. Un descriptor que no se decodifica se notifica como «vista ya no disponible»

- **Referencias:** `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:379-381` y `:285-287`. La prueba que fija el mensaje es `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs:1758` y `:1772`.
- **Problema:**
  - Un sobre cuyo descriptor (vista y sección) no se decodifica da `HasAddress == false`, y `RackViewAvailability.Evaluate` responde `Unavailable` (`src/RackCad.Application/Systems/Shared/RackViewAvailability.cs:139-140`).
  - El plan da el mismo Invalid y el mismo texto que a un fantasma: «El piloto no borra vistas y la vista B2 ya no está disponible en el sistema saliente: usa RACKEDITAR.»
  - RACKEDITAR distingue los dos casos: con un descriptor inválido aborta con «descriptor de vista invalido (posible corrupcion)» (`src/RackCad.Plugin/RackPushBackCommands.cs:230-236`).
  - La prueba `corrupt-descriptor` fija el mensaje engañoso.
- **Por qué importa:** el resultado es correcto (Invalid, como pide D-T4A1-04 (e)), pero el motivo apunta a una causa falsa. Ese motivo llegará al panel en A2 o en T4B.
- **Corrección sugerida:**
  - comprobar `decoded.HasAddress` antes de la disponibilidad, con su propio texto (por ejemplo, «La vista B2 tiene un descriptor de vista inválido: usa RACKEDITAR.»);
  - ajustar la prueba.

### M-6. Un catálogo nulo se evalúa antes que el Conflict

- **Referencia:** `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:308-311`.
- **Problema:**
  - `if (reread == null || catalog == null) return WithInvalid(MissingInputReason);` va antes de `CompareWith`.
  - Con un catálogo nulo y una relectura que difiere, `Prepare` da Invalid en lugar de Conflict, aunque Conflict y NotAdmitted no necesitan el catálogo.
  - El orden contractual, en D-T4A1-04 e INV-F4T4A1-07, pone el Conflict en el paso 4 y el Invalid en el 6.
- **Por qué importa:** en T4B, Invalid se traduce en `NotAppliedPreflight` y Conflict en `NotAppliedConflict`. Es un caso de programación, no del usuario, pero rompe la totalidad del orden declarado.
- **Corrección sugerida:**
  - dejar en esa posición solo `reread == null`, sin el cual no se puede comparar;
  - mover `catalog == null` al principio de `Build`.

### M-7. El plan guarda un campo que el contrato no enumera, sin declararlo como desviación

- **Referencias:** `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:214`, `:222`, `:236-237`, `:252` y `:409`.
- **Problema:**
  - `ExpectedVariableRegistryText` no está en la lista cerrada de INV-F4T4A1-08: RackId, la clave del origen, ExpectedBase, la escala, los miembros y la expectativa de POST.
  - La entrega lo menciona como «Nota», pero `Deviations` está vacío.
- **Por qué importa:**
  - Es razonable, porque `CompareWith` compara el registro de variables (`src/RackCad.Application/Workspace/Drafts/DraftStaleBase.cs:165-166`), y es una cadena con igualdad ordinal.
  - Pero amplía un tipo contractual sin disposición, y T4B lo consumirá.
- **Corrección sugerida:** que la sesión lo disponga de forma explícita, aceptándolo como ampliación declarada, o que se retire.

### M-8. La documentación de Waiting no coincide con la operación, y Applying durante la lectura se trata distinto

- **Referencias:** `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:287`, frente a `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:551-553` y `:583-584`. Para comparar, `WorkspaceHostCore.cs:666-667` y `:747-748`.
- **Problema:**
  - El summary de `EditConfirmStatus.Waiting` dice que también se espera si «the draft was being applied». Pero `TryConfirmPilotBase` responde **NotApplicable** con A1-01 para un borrador en Applying, como fija D-T4A1-03.
  - Si el borrador entra en Applying o se desregistra **durante** la lectura, `ConfirmBase` da NotApplicable y el host responde NotApplicable con «El borrador no admite comprobación en su estado actual.». En el mismo caso, Conservar y Descartar responden Waiting con «El borrador se está aplicando o ya no está registrado.».
- **Por qué importa:** la documentación del tipo del puerto induce a error a A2, que despachará por estado. La asimetría tampoco está declarada.
- **Corrección sugerida:**
  - corregir el summary;
  - opcionalmente, en el `default:` del consumo, distinguir `draft.Status == DraftStatus.Applying || !draft.IsRegistered` y responder Waiting con el texto existente. Si se hace, hay que declararlo.

### M-9. El doble de la UI no cuenta las tres lecturas nuevas en `Reads()`

- **Referencia:** `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:61-63`.
- **Problema:**
  - `Reads()` sigue filtrando solo `BeginOp`, `CheckOp` y `ReloadOp`.
  - El summary de `ReadHook` sigue diciendo «Runs inside TryBeginEdit, TryCheckNow and TryReload». Sin embargo, `ReadHook` ya se invoca en `TryConfirmBase`, `TryKeepMine` y `TryDiscardAndReload` (`:193-218`).
- **Por qué importa:** cualquier prueba de la UI de A2 que demuestre «no se leyó» con `Assert.Empty(port.Reads())` no verá una llamada a las tres lecturas nuevas.
- **Corrección sugerida:** incluir `ConfirmBaseOp`, `KeepMineOp` y `DiscardAndReloadOp` en `Reads()` y actualizar el summary. Las aserciones existentes no cambian, porque hoy nadie llama a esas operaciones.

### M-10. La atomicidad de la sobrecarga de Conservar depende de que `keepOn` no mute su segundo argumento

- **Referencias:** `src/RackCad.Application/Workspace/Drafts/DraftSession.cs:460-466` y `:480`.
- **Problema:**
  - `keepOn` recibe `_current` por referencia. Así lo fija la fórmula del paquete, `_copy(keepOn(_copy(read.InitialState!), _current))`, y la prueba lo afirma con `Assert.Same` (`tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs:411`).
  - El XML promete que «if one of them throws the draft stays exactly as it was». Eso solo es cierto si `keepOn` no muta `current` antes de lanzar.
  - La función de producción, `PushBackPilotMaterializer.KeepPilotEditsOn`, es pura (`PushBackPilotMaterializer.cs:242-251`), así que hoy no hay defecto.
- **Por qué importa:** es una garantía documentada más fuerte que la real para cualquier otro `keepOn`.
- **Corrección sugerida:** documentar la precondición en el XML («keepOn must not mutate its arguments»), sin cambiar la fórmula del paquete.

### M-11. El esqueleto del RED no era inerte en todos los miembros nuevos

- **Referencias:**
  - `921e3a36:src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:58-70`, que es `From`;
  - las igualdades de `PushBackPilotMemberPlan`, `PushBackPilotPostExpectation` y `PushBackPilotUpdatePlan`;
  - `921e3a36:src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:176-183`;
  - `921e3a36:src/RackCad.Plugin/Workspace/WorkspaceController.cs:293-297`.
- **Problema:**
  - INV-F4T4A1-13 pide «los tipos y miembros nuevos con cuerpos inertes». `From`, las igualdades y los reenvíos de la compuerta y del controlador ya son los definitivos en el RED.
  - Por eso sus pruebas pasan en el RED. Por ejemplo, `TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying` no figura entre los fallos del RED.
  - No cambia el comportamiento de producción existente, porque nadie los llama, y la exigencia central de INV-13 se cumple.
- **Por qué importa:** la cadena RED→GREEN no evidencia esos miembros, y la entrega no lo declara.
- **Corrección sugerida:** declararlo en la disposición como desviación menor aceptada. En tareas futuras, dejar esos cuerpos inertes en el RED, por ejemplo con un `From` que devuelva una instantánea vacía.

## Comprobado sin defecto

### `ConfirmBase` (INV-F4T4A1-02)

- Los tipos están en `DraftSession.cs`, sin archivo nuevo: `DraftConfirmStatus` en la línea 204 y `DraftConfirmResult` en la 217.
- Una relectura nula lanza (`:643`).
- Da NotApplicable sin ningún cambio fuera de Clean, Dirty o Conflict, o si el borrador no está registrado (`:645-647`).
- MATCH solo limpia `IsPossiblyStale` cuando no hay estado explícito (`:652`). En Conflict con MATCH responde Confirmed y **sigue en Conflict**, sin tocar `ConflictDifferences` ni la marca (`:650-654`).
- Con diferencias pasa a Conflict, o en Conflict las sustituye, con las diferencias ordenadas por `CompareWith` (`:656-658`; `DraftStaleBase.cs:138-168`).
- Nunca toca la base, el inicial, el actual, el pendiente, `LastApplyCause` ni `Warnings`, no limpia Dirty y no toma la ranura.
- Las pruebas son discriminantes, por ejemplo `WorkspacePilotPrepareTests.cs:240-257`, y `WorkspaceDraftApplyTests.cs` no cambia.

### La sobrecarga de Conservar (INV-F4T4A1-03)

- Con `keepOn` nulo lanza antes que con `read` nulo (`DraftSession.cs:469-470`).
- Comprueba en el orden de la versión de dos parámetros: NotAllowed, NotConfirmed y NotAdmitted (`:471-477`). Sin admisión no llama a `keepOn`.
- Hace todas las copias y la llamada a `keepOn` antes de `Rebase` (`:479-482`) y conserva el pendiente.
- La versión de dos parámetros no cambia (`:443-458`).
- El host la llama exactamente como pide el paquete (`WorkspaceHostCore.cs:654`) y no nombra `.InitialState` ni `.InitialDraftState`. Lo fija una guarda en `WorkspaceEditIntentGuardTests.cs`.

### Las tres operaciones del host (INV-F4T4A1-04)

El orden de los pasos es el mismo en las tres:

| Paso | `TryConfirmPilotBase` (`WorkspaceHostCore.cs`) | `TryKeepPilotEdit` | `TryDiscardAndReloadPilotEdit` |
|---|---|---|---|
| Errores de la petición | `:535` y `:539` | `:606` y `:616` | `:691` y `:701` |
| `PassGate` | `:543` | `:620` | `:705` |
| NotFound | `:549` | `:627` | `:711` |
| Aplicabilidad antes de leer | `:551-553` | `:629-632` | `:713` |
| La lectura | `:556` | `:635` | `:716` |
| El consumo | `:574-584` | `:654-670` | `:735-751` |

- El mapeo coincide con D-T4A1-03.
- `_knownClean.Remove` solo se llama en el Conflict de Confirmar, en Kept y en Reloaded (`:581`, `:658` y `:739`).
- La contención es `catch` → `Report(` → Failed, sin degradar la sesión.
- M-5 se lee solo como `pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic` (`:579`), sin alias.
- Las implementaciones explícitas llevan la marca de lectura con `try`/`finally` (`:843-883`).
- La compuerta reenvía y responde Waiting dentro de un aviso (`WorkspacePilotSupport.cs:176-183`), y el controlador solo reenvía (`WorkspaceController.cs:293-297`).

### Los tipos del puerto (INV-F4T4A1-05)

- `EditConfirmStatus` y `EditConfirmOutcome`, que es un struct con valor por defecto Unavailable y `Differences` nunca nulo (`WorkspaceEditIntent.cs:282-358`).
- `Kept` está en la posición 10 (`:132`), y la longitud exacta 11 la fija `WorkspacePilotPrepareTests.cs:1352`.
- `PilotUnknownOrigin` (`IWorkspacePilotEditPort.cs:181-186`).
- `PilotFieldView` tiene un solo constructor, con los tres parámetros opcionales en orden tras `errorReason` (`:207-210`).
- El comentario del puerto dice «seis» lecturas (`:19`).
- `ViewOfPilotDraft` rellena `unknownOrigin` solo en Unknown, `conflictText` solo en Conflict y `lastCause` tal cual (`WorkspaceHostCore.cs:1203-1213`).

### `Prepare` (INV-F4T4A1-07)

- La instantánea solo la construye `From`, con su constructor privado y la consideración del estado bajo Applying (`PushBackPilotUpdatePlan.cs:21-70`).
- El orden es NotApplicable, PendingText, NothingToApply (sin mirar la relectura), Conflict (antes de la admisión), NotAdmitted y Build (`:303-316`). Toda excepción se contiene como Invalid (`:318-321`).
- Los casos Invalid son (`:328-381`):
  - (a) `ComposeForWrite`;
  - (b) cero o varios orígenes byte a byte;
  - (c) `PreflightInnerSources` de Application, en orden ordinal de clave, con PushBack y el proyecto del sobre de origen como iniciador, igual que `RackPushBackCommands.cs:221`;
  - (d) el ensamblador, el resolver y `Cortes` dentro de un `try`;
  - (e) la disponibilidad con `PushBackViewAvailabilityFacts(cortes, IsComposite)`, como en `RackPushBackCommands.cs:269-271`.
- `Prepare` nunca cambia el borrador.

### El plan (INV-F4T4A1-08 y 09)

- Es inmutable: solo propiedades de lectura y colecciones `ReadOnlyCollection` sobre copias.
- La igualdad es explícita, ordinal y en orden, y la escala se compara con `Equals`. `DraftStaleBaseEntry` y `PushBackPilotKeyedText` son `readonly record struct` de cadenas y valores (`PushBackPilotUpdatePlan.cs:137-260`).
- La vista y la sección se calculan como en RACKEDITAR (`:385-399`), y el `default` hacia Lateral es seguro, porque solo llega con una dirección disponible.
- El diseño saliente se compone con `WithSourceMetadataFrom` del proyecto resuelto alineado por índice (`:401`).
- El sobre saliente se compone sobre el sobre **releído**, con `source.Id` y `source.Name`, en el único miembro que compone (`:414-418`).
- No hay borrados. Una vista no disponible da Invalid.

### El censo de `Compose` (INV-F4T4A1-10)

- Hay una sola entrada nueva, en la posición ordinal correcta (`CustomPropertiesEnvelopeGuardTests.cs:196`), con su línea de comentario.
- TEnv12 pasa a 11 (`CustomPropertiesEnvelopeTests.cs:708`).

### Los arrastres de `ComposeForWrite` (INV-F4T4A1-11)

- (6) rechaza NaN, ±∞, 0, −0 y los negativos (`PushBackPilotMaterializer.cs:205-208`).
- (1) compara `current` sin normalizar con una copia de `initial` que lleva la escala de `current` (`:210-215`).
- (3) exige que la base rematerializada sea igual a `initial` según el comparador del piloto (`:221-224`).
- Las pruebas de (2), (3) y (6) fallan en el RED. Solo la de (1) no discrimina (M-1).
- `ComposeForWrite` solo tiene un llamador, después de la comparación (`WorkspacePilotPrepareGuardTests.cs:156-190`).

### R-3 y las guardas (INV-F4T4A1-12)

- La guarda por cuerpo de `ViewOfPilotDraft` está en `WorkspaceHostHardeningGuardTests.cs:552` y siguientes, con sus autopruebas.
- La guarda del núcleo está en `WorkspacePilotPrepareGuardTests.cs:257-317`, con los límites de M-4.
- La exención de `ConfirmBase` por nombre está en `WorkspaceDraftRegistryTests.cs:136`.
- C2-M-8 es un `Fact` (`WorkspaceCommandAdvisorTests.cs:379`).

### Las guardas cambiadas, frente a `443b5b3a`

Ninguna se debilita más allá de lo declarado:

- `WorkspaceLifecycleGuardTests.cs:449`: la cota pasa de 9 a 12, y `ChangesADraft` gana `.KeepMineOnCurrentVersion(` y `.ConfirmBase(`.
- `WorkspacePilotEditGuardTests.cs`: 15 operaciones, seis lecturas, la marca con cota 6, la compuerta, los reenvíos, `WriteViolations` sobre los tres cuerpos públicos y los nombres de tipo, todo con sus autopruebas en negativo.
- `WorkspaceEditIntentGuardTests.cs`: `AllowedConsumers` gana solo los dos consumidores declarados; la contención cubre los cinco cuerpos; los nombres prohibidos en el drenaje y en Idle se amplían.
- `WorkspaceHostHardeningGuardTests.cs`: la cola de `PilotFieldView` se exige exacta, y la tabla pasa a 30 filas con cuentas exactas.
- `WorkspaceEditIntentHostTests.cs:982` y `:1046`.
- Los únicos cambios a pruebas existentes fuera de §4 son los dos de la corrección.

### Ninguna escritura authored y los límites (INV-F4T4A1-01 y 14)

- Ninguna línea añadida en `src/` nombra una transacción, un bloqueo, `Commit`, `OpenMode`, `UpgradeOpen`, `AppendEntity`, `Erase`, `RackBlockData`, una importación, `async`, `Task`, `Dispatcher`, `Timer`, `ExecuteInCommandContextAsync` ni reflexión.
- La pureza del archivo del plan la fija `WorkspacePilotPrepareGuardTests.cs:30-79`.

### La disciplina RED/GREEN (INV-F4T4A1-13)

- `git diff 921e3a36 bbd9a35e -- tests/` está vacío. El GREEN solo toca `DraftSession.cs`, `PushBackPilotMaterializer.cs`, `PushBackPilotUpdatePlan.cs` y `WorkspaceHostCore.cs`.
- En el RED:
  - `ConfirmBase` devuelve NotApplicable y la sobrecarga NotAllowed, sin cambio;
  - los cuerpos del host devuelven `default` sin leer;
  - `Prepare` devuelve NotApplicable;
  - `ComposeForWrite` y `ViewOfPilotDraft` no cambian.
- La salvedad es M-11.

### Rodeos de guarda en el producto

No hay ninguno. No queda ninguna variable `reread` ni ningún patrón `is { }` en el host, y el resultado del piloto solo se llama `pilot`.
