# I-64 F4-T4C2-PANEL — Comprobación independiente del borrador del contrato de gate

**Objeto:** `contract-draft.json` (AuthorityRevision provisional en ceros) y su generador `make_t4c2_contract.py`.
**Base:** rama `architecture/workspace-persistente-rackcad`, HEAD `0a57211d` (cierre de F4-T4C1-HOST); `origin/main` = `812baf87` = `MainSha` del borrador.
**Autoridades:** paquete `docs/initiatives/I-64-f4-t4c-gate-contract-package.md` v5 (§4, §6, §7, D-T4C-19, D-T4C-20), decisión D5 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-decisions-d1-d6.md`), contrato y cierre de T4C1 (`.../F4-T4C1-HOST/coordinator/gate-contract-attempt2.json`, `.../F4-T4C1-HOST/task-close-package.md`) y paquete de T4A2 (§8, H-19, D-T4A2-06).
**Método:** solo lectura, con `git -C`, lectura de archivos, `git ls-tree` del HEAD para la cobertura de las listas, y los TRX de la CI verde de T4C1. No se ejecutó dotnet ni se escribió en el worktree.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.**

**Recuento B/I/M: 1 / 5 / 13.** No hay ningún hallazgo OWNER.

El bloqueante es una parada copiada de T4C1 sin adaptar, y su corrección es textual. Los cinco importantes se corrigen dentro del contrato y del alcance, sin decisión nueva. D5 autoriza a incorporar los hallazgos necesarios.

## Comprobaciones conformes

- **`AllowedWriteScope` (13):**
  - son exactamente las rutas [2] y [1, 2] del §6;
  - las 13 existen en el HEAD, y ninguna cae bajo un prohibido;
  - no sobra ninguna. Cada una la necesita el botón, el gesto, la presentación, el diagnóstico, una de las filas 12, 18 o 21, o el doble de UI;
  - `TryCheckPilotEdit` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:382-445`) y `EditCheckOutcome` (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:238-280`) viven en archivos permitidos;
  - el puerto no cambia, así que los dobles de Core (`TW/WorkspaceCommandAdvisorTests.cs`, `TW/WorkspaceHostTestSupport.cs`) no hacen falta.
- **`ForbiddenWriteScope` (130):**
  - las 130 entradas existen, sin duplicados ni redundancias;
  - todo `src/RackCad.UI/` fuera de lo permitido queda cubierto. Las 10 subraíces se comprobaron con `git ls-files`, y no queda ningún archivo de `src/RackCad.UI/` sin cubrir;
  - la clausura de T1B queda cubierta: G1 a G3 y G6 por las subraíces y los archivos de primer nivel de la UI, G4 por `src/RackCad.Application/Systems/` y G5 por `src/RackCad.Plugin/`;
  - F28 y los `*Commands*.cs` caen bajo `src/RackCad.Plugin/`, y `tools/` cubre `RamToolCommands.cs`;
  - la costura de I-55 queda protegida por `src/RackCad.Plugin/`, `src/RackCad.Application/Views/Redraw/` y las pruebas F45 a F58;
  - lo que creó T4C1 queda prohibido: el corredor, el adaptador, el escritor, el controlador y el puente por los prefijos, y el puerto, `WorkspacePilotSupport.cs`, `IWorkspaceBridgeLink.cs`, `EventBridgeCore.cs`, `WorkspaceSession.cs` y las pruebas HR, RQ, OM, IG y el doble del host como `C1_ONLY`;
  - las 13 guardas de D-T4C-20 (a) están por archivo exacto, y las tres de (b) de T4C1 pasan ahora al prohibido;
  - todo lo que prohibía T4C1 sigue cubierto, salvo la raíz `src/RackCad.UI/`, que se parte a propósito.
- **`RequiredTests`:** las 26 cuentas coinciden exactamente con los TRX de `artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T165352Z-3e8e/` (`ci-core/core.trx`: 19002 resultados; `ci-ui/ui.trx`: 2310):
  - Core Workspace 3421;
  - las 23 clases, cada una con su cuenta, por ejemplo `G16FirstViewCorrectiveTests` 61;
  - UI Workspace 601;
  - `PushBackClassicCharacterization` 55.

  `ExpectRed` es coherente con el RED de T4C2. En Core fallan las guardas cambiadas de las filas 12, 18 y 21, y en UI fallan las pruebas nuevas contra cuerpos inertes, como pide la tabla del §6 («true en [2]»). Las 23 clases y los 55 de T1B no se tocan.
- **Paradas:**
  - las 17 del protocolo son idénticas a las de T4C1;
  - C15 recoge fielmente D4: una replanificación reforzada por cada delegación nueva, sin gastar intento, y STOP al Owner si falla una segunda vez;
  - C16 recoge los seis temas de D5 (salvo M-03).
- **Nada se rompe sin aviso en:**
  - G-10, que ya admite el VM del campo (`TW/WorkspacePilotUpdateIntegrationGuardTests.cs:733-743`);
  - el censo de `Task` y `async`, con el VM del campo en 0/0;
  - las pruebas de forma de `EditCheckOutcome` (`TW/WorkspaceEditIntentHostTests.cs:1126-1173`), mientras no se añadan valores a `EditCheckStatus`;
  - las guardas de XAML de la superficie, que fijan una cota inferior de botones y `Content` únicos que «Actualizar» no repite.

  Los riesgos reales están en I-04 y M-09.

## Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| **B-01** | B | **C07 está copiada de T4C1 sin adaptar:** «a guard change outside the **T4C1** rows of D-T4C-20». En T4C2, las filas exigidas (12, 18 y 21) no son de T4C1, así que la parada salta literalmente con el trabajo mandado y contradice INV-F4T4C-17. Al revés, deja sin parada un cambio de las filas [1] que viven en un archivo permitido: las filas 9, 10 y 11 de `TW/WorkspaceEditIntentGuardTests.cs` | `StopConditions` F4T4C-C07 del borrador; el generador (l. 92) copia C01 a C14 sin cambios; paquete §6, C07 (l. 1645), y la columna «Parte» de D-T4C-20 (l. 1382-1405) | Reescribir C07: «a guard change outside the T4C2 rows of D-T4C-20 (12, 18 and 21), the H-19 table of the F4-T4A2 package and the UI rule of D-T4A2-06 that D-T4C-12 declares; in particular any change of a [1] row, also inside an allowed file». Añadir al generador una aserción que impida emitir C07 y C14 con su texto de T4C1 |
| **I-01** | I | **C14 está copiada de T4C1, y el contrato no acota por miembro los dos archivos de Application que permite.** C14 admite literalmente cambiar `TryUpdatePilot` y `UpdateGate` «beyond I-01, M-01 and M-03», y `RUN` dentro de I-01 y M-01, pero esos arrastres los cerró T4C1. `WorkspaceHostCore.cs` y `WorkspaceEditIntent.cs`, ambos permitidos, contienen casi entera la integración ratificada de T4B y T4C1: `RequestUpdate`, `TakeUpdateProgress`, `ExpireStaleUpdate`, la corrida anidada, `UpdateGate`, `TryUpdatePilot`, la traducción de `PilotBoundaryKind.Update` en `CrossBoundary` y los tipos `EditUpdateOutcome`, `EditUpdateStart` y `EditUpdateProgress`. Solo C16 los protege, en términos generales | C14 del borrador; `WorkspaceHostCore.cs:382-445` es el único miembro que cambia D-T4C-12; `WorkspaceEditIntent.cs:238-280` (`EditCheckOutcome`) frente a `:382-590` (`EditUpdate*`); D-T4C-19, parte [2] (l. 1359-1361) | Reescribir C14 para T4C2: «a change would be needed in WorkspaceHostCore.cs outside the body of TryCheckPilotEdit (D-T4C-12), or in WorkspaceEditIntent.cs outside EditCheckOutcome (its two optional properties, its constructor and its factories), or any change of RequestUpdate, TakeUpdateProgress, ExpireStaleUpdate, the nested run, UpdateGate, TryUpdatePilot, CrossBoundary or the EditUpdate* types». Llevar la misma frontera a un invariante |
| **I-02** | I | **La parte de núcleo de D-T4C-12 no tiene en el permitido ningún archivo de pruebas de comportamiento.** `TryCheckPilotEdit` se prueba con el núcleo real en `TW/WorkspaceEditIntentHostTests.cs`, que es de T4C1 y está prohibido. El diagnóstico de la comprobación general se prueba en `TW/WorkspacePilotReconciliationHostTests.cs`, que no está en ninguna lista. Las pruebas de UI usan el doble del puerto. Por eso, «su fallo da null», «antes de CheckNow» y que el diagnóstico y la pista lleguen de verdad al `EditCheckOutcome` de Recovered y de Conflict solo los cubre la cuenta textual de la fila 12. Un cableado roto, que calcula la expresión y no la pasa, pasaría. `ExpectedEvidence` dice que el diagnóstico «se prueba en Core» | §6 (pruebas de Core [2]: tres guardas más `TW/WorkspaceEditIntentGuardTests.cs`); D-T4C-17, l. 1302 (solo UI); `TW/WorkspaceEditIntentHostTests.cs:730-830` y su `Rig` privado (`:73`); `TW/WorkspacePilotReconciliationHostTests.cs:15-40` (núcleo real con un lector doble y sin guardas) | Añadir `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs` al permitido. No tiene guardas, no es de T4C1 y entra en el filtro de Workspace. Sus pruebas para D-T4C-12: <ul><li>con una relectura no admitida, el diagnóstico y la pista en Recovered y en Conflict;</li><li>null con una relectura admitida;</li><li>un fallo del diagnóstico da null y no cambia la clasificación;</li><li>una sola lectura.</li></ul>Si no se amplía el alcance, declararlo como limitación y corregir `ExpectedEvidence`: en Core, solo por guarda de fuente |
| **I-03** | I | **INV-F4T4C-14 es falso en el HEAD:** dice que «`.RequestUpdate(` solo aparece en Update() del VM del campo y en los dos reenvíos». G-10 admite, por la I-01 de T4C1, cuatro ventanas de `src/RackCad.UI/Systems/` con su propio `RequestUpdate`, con cuenta 1 cada una, y deja fuera entero el VM del campo. Una guarda nueva escrita según el invariante tropezaría con esas cuatro ventanas, que la tarea no puede tocar. Por ejemplo, añadir `RequestUpdate` a la regla 1 de la reconciliación, que recorre todo `src/RackCad.UI/` | IG `:722-748` (`OwnRequestUpdateWindows`, `FieldViewModelPath`, `withoutGesture`); INV-F4T4C-21 de T4C1 (I-01); `TW/WorkspacePilotReconciliationGuardTests.cs:51-52` (`UiFiles` = `src/RackCad.UI/`) y `:72-80` | Reescribir: «G-1 a G-7, G-9 y G-10 siguen en verde. El `.RequestUpdate(` del puerto del panel solo aparece en los dos reenvíos y, desde T4C2, exactamente una vez en el VM del campo, dentro de `Update()` (fila 21). Las cuatro ventanas de `src/RackCad.UI/Systems/` con su `RequestUpdate` propio siguen como excepciones por archivo exacto y no cambian» |
| **I-04** | I | **Riesgo de rotura fuera del alcance, sin aviso.** D-T4C-03 (F-1) exige `TakeUpdateProgress` en cada repintado, y el repintado del campo ya pasa por `GetField`, que el doble registra. Si el doble de UI registra `TakeUpdateProgress` en `Calls` o en `Operations()`, fallan las aserciones de conjunto exacto de `UW/WorkspacePilotCloseSyncTests.cs`: <ul><li>`AssertOnly`, en `:119`, `:149`, `:285` y `:307`;</li><li>`IsSubsetOf(allowed)`, en `:1464-1473`.</li></ul>Ese archivo no está en el permitido. El contrato no avisa del riesgo ni dice cómo evitarlo | `UW/WorkspacePilotEditPortDouble.cs:52-59`, `:233-235` (registra GetField) y `:346-348` (las dos operaciones inertes); `UW/WorkspacePilotCloseSyncTests.cs:70-71`; FVM `:238-251` y `:557-577`; BVM `:801-813` (`RenderField` llama a `Field.Show` en cada repintado) | Fijar una restricción en el contrato: el doble registra `RequestUpdate`, `TakeUpdateProgress` y su orden con `CrossBoundary` en un registro propio, sin cambiar `Calls`, `Operations()` ni `Reads()`. La alternativa es añadir `UW/WorkspacePilotCloseSyncTests.cs` al permitido, con su cambio declarado. Se recomienda la primera, porque no amplía el alcance |
| **I-05** | I | **INV-F4T4C-02 e INV-F4T4C-11 resumen D-T4C-11 y D-T4C-03 dejando fuera reglas de la parte [2]** que una lectura literal permitiría violar: <ul><li>(a) si `TakeUpdateProgress` encuentra un resultado antes de pedir, lo presenta y **no pide** (paso 1, regla 5, RR N-15). El borrador solo dice «recoge antes lo pendiente»;</li><li>(b) el botón **se ofrece también con un diagnóstico persistente**, y la corrida decide (H-M3-42), al contrario que las opciones de T4A2 (D-T4A2-06, `OffersChoice`). Nunca se ofrece en la vista previa;</li><li>(c) `InFlight` falso con Unavailable no toca lo ya presentado (regla 2);</li><li>(d) solo consume el resultado la superficie viva, para su propio rack y después de `IsMine`. Si la superficie cambió: `PresentSurfaceChange`, y el resultado sigue en el almacén (regla 3);</li><li>(e) todo valor de inicio que no sea `Accepted` se presenta en el gesto con su causa, también `Failed(HostNotRunReason)` (paso 5, regla 4);</li><li>(f) los textos nuevos son constantes del VM, con su prueba de tabla.</li></ul> | Paquete D-T4C-11 (l. 947-989) y D-T4C-03 (l. 538-547); FVM `:610-625` (`OffersChoice` oculta las opciones con `_persistDiagnostic`) | Ampliar INV-02 con (a) e INV-11 con (b) a (f), citando el paso o la regla de cada una |
| M-01 | M | **Al prohibido le falta «el resto de `tests/RackCad.UI.Tests/` fuera de lo permitido»,** que pide la partición de F41 en el §6. Quedan 174 archivos de pruebas de UI fuera de las dos listas, y 20 de ellos están en UW, junto al trabajo: `WorkspacePilotCloseSyncTests.cs`, `WorkspacePilotReconciliationViewTests.cs`, `WorkspacePilotCloseSyncViewTests.cs`, `WorkspacePanelViewTests.cs` y otros. El generador dice «con la UI partida como pide el section 6». T4C1 tuvo la misma omisión, y en T4B F4-E60 evitó las coberturas grandes por el riesgo de copia | §6, l. 1532-1536; la cobertura medida contra `git ls-tree` de `0a57211d`; `f4-dispositions.md:2139` | Añadir al menos los 20 archivos de UW que no están permitidos, enumerados con `git ls-files` como `UI_WS_REST`. Declarar que los 154 archivos de primer nivel quedan protegidos solo por la lista blanca (F4-E60), o enumerarlos |
| M-02 | M | C09 solo cubre que el host traduzca Unknown o reclasifique. En T4C2 el riesgo es la UI | C09; D-T4C-11, «sin reclasificarlo» y «Unknown nunca…» | «the host **or the panel** translates Unknown, presents it as a failure or a success, or reclassifies an outcome» |
| M-03 | M | C16 no dice a quién se vuelve. D5 manda volver al Owner, y C15 sí lo dice | C16; D5, l. 113-120 | Añadir «STOP to the Owner (decision D5)» |
| M-04 | M | A INV-12 le faltan cuatro cosas de D-T4C-12: <ul><li>«antes de CheckNow»;</li><li>«nunca convierte en Failed una respuesta cuyo CheckNow ya cambió el borrador»;</li><li>la forma de `EditCheckOutcome`: dos opcionales y ningún valor nuevo de `EditCheckStatus` (`TW/WorkspaceEditIntentHostTests.cs:1136`);</li><li>la regla nueva de `PresentCheck`: «retira salvo que la respuesta traiga diagnóstico».</li></ul> | D-T4C-12, l. 999-1020 | Añadir las cuatro |
| M-05 | M | A INV-13 le falta que ClosedInError **nunca se presenta como éxito**, aunque su clasificación sea Applied | D-T4C-13, fila M-03 (l. 1034); documentación de `EditUpdateStatus.ClosedInError` (`WorkspaceEditIntent.cs:420-424`) | Añadir «con su clasificación y nunca como éxito» |
| M-06 | M | **Dos redacciones de invariante no son fieles:** <ul><li>INV-19 añade «la UI solo usa el puerto del panel». Es inventado y, leído literalmente, falso: el VM del panel usa cuatro puertos;</li><li>INV-03 prohíbe «ningún… event… nuevo en la vista», y D-T4C-11 manda un manejador `Click` nuevo.</li></ul> | INV-19 del paquete (l. 1621); `WorkspacePanelViewModel.cs` (constructor); D-T4C-11 (l. 948) | INV-19: «el gesto y la presentación solo usan `IWorkspacePilotEditPort`». INV-03: «la palabra clave `event`; el manejador `Click` del botón, dentro de `Contained`, es el declarado» |
| M-07 | M | **Se omiten INV-04 a INV-10** sin decir que siguen vigentes. Tampoco se dice que los menores de T4C1 (N-01 a N-05 y D2-2) no se arrastran, porque sus archivos están prohibidos | §6, l. 1596-1607; cierre de T4C1, §5 | Una línea: «INV-F4T4C-04 a 10 de F4-T4C1-HOST siguen vigentes, con sus guardas en verde; los menores de T4C1 no se arrastran» |
| M-08 | M | `ExpectedEvidence` omite el build Debug del Plugin con salida temporal, que piden el §6 y T4C1 | §6, l. 1627; `ExpectedEvidence` de T4C1 | Añadirlo, o declarar que no hace falta porque el Plugin no cambia y el build Release lo compila |
| M-09 | M | **No se citan al Worker las guardas fuera de la parte [2] que acotan el cambio:** <ul><li>`TW/WorkspaceLifecycleGuardTests.cs:481-484`: dos `_knownClean` en `CheckBody` y la forma literal de los case Recovered y Conflict, con `return EditCheckOutcome.Recovered(` y `Conflict(`;</li><li>`TW/WorkspaceEditIntentGuardTests.cs:1003-1016`: las palabras prohibidas, incluso en comentarios, en CORE y en `WorkspaceEditIntent.cs`;</li><li>los nombres de H-20 (`TW/WorkspaceModelBoundaryTests.cs`);</li><li>`TW/WorkspaceBrowserHostGuardTests.cs:323-347` y `:383-399`: `Content` únicos en la superficie, y ningún Width ni Height;</li><li>`TW/WorkspacePilotReconciliationGuardTests.cs:414-477`: «Sin cambios…» y «El cambio no llegó a aplicarse…» una sola vez en la carpeta;</li><li>el censo de IG (0/0);</li><li>`TW/WorkspaceHostHardeningGuardTests.cs:758-783`.</li></ul> | Los archivos citados | Añadirlas a la lista de restricciones que la emisión cita al Worker (como pide IR M-16 en el §6) |
| M-10 | M | La sección de la autoridad del paquete dice «D-T4C-01 a D-T4C-20, solo lo marcado [2] o [1, 2]», pero las D-T4C-NN no llevan marca. D-T4C-03 y D-T4C-17 son necesarias y podrían leerse como excluidas | `Authorities[0]` | Nombrarlas: D-T4C-03, -11, -12 y -13 (M-03 en la UI); -17 (UI y contención); -19 ([2]); y -20 (filas 12, 18 y 21, y (a)) |
| M-11 | M | El `Objective` dice «sin cambios del host…», y `WorkspaceHostCore.cs`, el núcleo del host, sí cambia por D-T4C-12 | `Objective` | «Sin cambios de la integración en el host de F4-T4C1 ni de T4B», con la frontera de I-01 |
| M-12 | M | El §6 pide la clausura de T1B «por entrada explícita». El borrador la cubre por raíces: la cobertura es igual o mayor, pero no se declara | §6, l. 1527; map-4 §3.4 | Declarar la equivalencia en el contrato o en el generador: G1 a G3 y G6 por las subraíces y la raíz de la UI, G4 por `Application/Systems/` y G5 por `Plugin/` |
| M-13 | M | Los textos nuevos de la UI no los fija el paquete. Con el precedente de D1, conviene que la entrega los liste para revisarlos | D-T4C-11 (l. 989); D1 | En `ExpectedEvidence`: la entrega lista cada constante nueva con su valor y su caso. Por D1, no se gasta una corrección solo por redacción |

## Hallazgos OWNER

**Ninguno.** La parte [2] se puede hacer con las decisiones ya tomadas en v5:
- la UI de Actualizar (D-T4C-11, O-14 y O-15);
- el canal F-1 (D-T4C-03);
- el diagnóstico (D-T4C-12, con D-1 sin cambios);
- M-03 en la UI.

No exige decidir nada nuevo sobre la semántica de Actualizar, la autoridad de escritura, Undo, el conflicto o stale, ni T1B, y no cambia el comportamiento ratificado de T4B ni de T4C1.

Las correcciones anteriores son de fidelidad del contrato o de alcance de pruebas, dentro del «incorporar los hallazgos necesarios» de D5:
- C07 y C14;
- la ruta de I-02;
- la restricción del doble de I-04.

**Durante la ejecución serían C16, con vuelta al Owner:**
- que F-1 necesitara leer `InFlight` sin consumir el resultado, lo que cambiaría el puerto;
- que D-T4C-12 necesitara valores nuevos de `EditCheckStatus` o tocar la clasificación de T4A2;
- que la presentación de ClosedInError con la clasificación Applied obligara a redefinir qué es un éxito.
