# Revisión independiente del código: I-64 F4B PB-T4c (`F4B-PBT4-EDIT-ALL-WINDOW`), intento 1

**Objeto:** `git diff d5026014..7cc1076e`, dos commits:
- el RED, `280c79be` (hijo directo de `d5026014`);
- el GREEN, `7cc1076e` (hijo directo del RED).

No hay commit «RED repair». WorkRunId `R20261121T205731Z-41db`.

**Autoridades,** leídas en `7cc1076e`:
- el contrato, `docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json`;
- el paquete v5.1, `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`, citado como «paq.» con su línea;
- D-S7 a D-S10 (`F4B-DRAFTS/pbt4/session-pbt4c-design-decisions.md`);
- las decisiones del Owner de `F4B-DRAFTS/coordinator/` (`owner-pbt4c-list-4-decision.md`, `owner-pbt2b-close-decisions.md`, `owner-f4b-dpb07-extensiondata-decisions.md`);
- la entrada «## PB-T4c» de `superseding-characterization.md`.

**Cómo leí:**
- El código y los documentos, solo por objetos git desde el checkout principal (`show`, `diff`, `grep`, `ls-tree`, `log`). No leí, ni listé, ni nombré el worktree de la rama.
- La entrega, en la copia `rv/`.
- No compilé ni ejecuté pruebas.
- Mis guiones de Python (`-I`, escritos con la herramienta de archivos) solo leen blobs y TRX. Ninguno ejecuta un guion del paquete:
  - **las dos piezas,** con un lector propio del formato de bloques;
  - **los `Skip`,** leyendo `SKIPS` con `ast.literal_eval`, sin importar el módulo.
- La CI, con `gh run view` y `gh api`, solo de lectura. Para comparar duraciones, bajé con `gh api` los dos artefactos de TRX de la corrida `44903818806` (PB-T4b) a una carpeta aislada del scratchpad.

---

## Veredicto

**ACEPTABLE CON MENORES, condicionado a que se disponga I-01.** I-01 no pide ningún cambio en `src/`. **B 0 / I 1 / M 7**, más **5 hallazgos OWNER**.

**Lo que está bien** (comprobado por mí, no por el resumen):
- **El RED,** 8 de 8 archivos de `src/`, y **el GREEN,** 13 de 13, son byte a byte las piezas del paquete:
  - las piezas son `pbt4c-skeleton-v51.txt` (`f8840c67…`) y `pbt4c-green-v51.txt` (`081ef77e…`);
  - las dos son iguales al anexo V51-A del paquete;
  - los blobs no tienen CRLF ni BOM;
  - las sha256 son las de `apply-red.txt` y `apply-green.txt`, que son iguales a `F4B-DRAFTS/pbt4/cc4c/r5j-rg/`.
- **El GREEN** solo toca 10 archivos de `src/` (RED..GREEN). No toca `tests/`, `docs/` ni `Pilot/`.
- **El alcance son exactamente las 26 rutas** del `AllowedWriteScope`. No hay ninguna del `ForbiddenWriteScope`.
- **Los 4 `Skip`** son los de la lista, con su motivo literal, y en esos archivos no cambia nada más.
- **Los 4 equivalentes** tienen su `EquivalentSha`, igual al texto del anexo, y no cambian después del RED.
- **Los totales de cada rama cuadran** con F4B-E46. Comparé las TRX nombre a nombre con la línea base de PB-T4b.
- **Ningún `warning CS`.** La CI del GREEN da 4/4, con el Plugin.
- **D2,** sin violación.

**Por qué no es REWORK:**
- **I-01** es una desviación **no declarada** de la receta de §5.3 en CT-PBT4C-11. Su línea esperada se cumple, y el código de `src/` no está en duda. La misma receta ya está en CT-PBT4B-10, cerrada en PB-T4b.
- Hay un indicio de que la receta literal podría no dar la línea esperada (véase I-01). Por eso pide una disposición, no una corrección a ciegas.
- **Los menores** son debilidades de discriminación (dos de la letra del paquete), un aviso engañoso de la letra del GREEN y huecos de registro.

## Recuento

| Severidad | N.º | Ids |
|---|---|---|
| Bloqueante | 0 | — |
| Importante | 1 | I-01 |
| Menor | 7 | M-01 a M-07 |
| OWNER | 5 | OWNER-01 a OWNER-05 |

---

## Tabla de hallazgos

Todas las rutas, en `7cc1076e` salvo que se diga otra cosa.

| Id | Sev. | Dónde | Qué | Qué propongo |
|---|---|---|---|---|
| **I-01** | I | `tests/RackCad.Tests/PushBackEditAllTests.cs:589-600` (CT-11, segunda receta); paq. :1613 (§5.3, fila 11) y :1237-1241 (D-PBT4-18, condición 7) | **Lo que pide §5.3:** «la condición 7 por el puerto (la línea base resuelta del estado aplicado, sustituida por la de otro fixture)». **Lo que hace la CT:** sustituye `Editor.SideA.LastModuleReconciliation`, es decir, el **informe de módulos** y no la línea base, por un objeto de `RuntimeHelpers.GetUninitializedObject`, que tampoco es «otro fixture» (:594-595). **La línea** `sustituir-referencia estado=Applied isdirty=False` se cumple, pero sobre otra referencia. **No está declarado:** ni en `Deviations` ni en `UnexpectedFindings`; la entrega dice que la CT-11 se escribió desde §5.3. **El precedente:** CT-PBT4B-10 hace lo mismo (`PushBackFullPanelTests.cs:691-695`). **Un indicio de que la receta literal no daría la línea:** el comparador lee una proyección de la línea base (`PushBackEditableStateCopyTests.cs:1318-1333`, «M01 (condition 7 (ii))»). Con dos líneas base distintas, mutar la longitud de un módulo cambia `Equals` (:1359, `iguales-tras-mutar-un-modulo=False`). Así que la línea base de un fixture de contenido distinto podría dar `isdirty=True`, contra la línea esperada y contra lo que afirma D-PBT4-18 («no compara la línea base»). No lo ejecuté | **Disposición** (OWNER-01): ratificar la receta como errata, como en CT-PBT4B-10, o sacar una versión del paquete que fije la receta y su línea. No hace falta cambiar código. Tocar la CT ahora sería tocar `tests/` tras el RED |
| **M-01** | M | `PushBackEditAllTests.cs:106` (`Port => Rig.Core`) y :425; paq. :1612 (§5.3, fila 10) | §5.3 pide para CT-10 «el puerto real (`WorkspacePilotEditGate` sobre WHC)». La CT llama a `TryEditAll` sobre el anfitrión, sin la compuerta. Las líneas no cambian, y el reenvío de la compuerta lo fija CT-12. Desviación no declarada | Registrar |
| **M-02** | M | `PushBackOpenerClassicPathGuardTests.cs:34-56` y :69; `PushBackEditAllTests.cs:272` | **CT-07:** la autoprueba inyecta el nombre en el texto del sujeto, así que «falla» aunque el sujeto esté vacío. No ve una extracción que deje de casar, y la guarda pasaría en vacío en el GREEN. Comprobé que las cinco expresiones casan en `7cc1076e` (WHC:1163 y :1179; FVM:432; WCo:337; los dos archivos de `Draft/`), así que hoy la guarda no está vacía. **CT-04,** `estado-igual`: `launch.State` es por referencia el `Editable` actual del borrador, así que la comparación da `True` por identidad. Las dos debilidades son de la letra del paquete («un sujeto que falta es su texto vacío») | Para una versión futura: una línea «sujeto … longitud>0» en el GREEN |
| **M-03** | M | `tests/RackCad.UI.Tests/PushBackEditAllWindowTests.cs:343-361` (`InWorkspace`) | `VentanaBorrador` construye la ventana y carga el borrador **fuera** del `try` (:346; el `try` empieza en :347). §5.0 pide «la carga dentro del `try` y `Cerrar` en `finally`». Si la carga lanza, la excepción escapa de la CT (no es una línea «EXCEPCION»), y una ventana ya construida no se cerraría. La rama clásica (:378-395) sí sigue la regla | Registrar |
| **M-04** | M | `WorkspaceHostCore.cs:1210-1222`; FVM:1344 | Si `ShowModal` lanza **después** de un aplicar, `EditAllOfPilot` devuelve `Failed`, con `Applied` a 0, aunque el borrador ya tiene la edición y `NotePilotEdit` ya corrió. El campo dice «No se abrió el editor: <mensaje>.», que es falso: el editor se abrió y hay cambios. Es la letra del GREEN | Para una versión futura; anotarlo en el Smoke-PB (OWNER-04) |
| **M-05** | M | paq. :1680 («CT-04 y 11: el adaptador inerte responde `NotEditable`») | En el RED, CT-04 y CT-11 caen porque **el puerto responde `NotAvailable`** y el doble nunca entra en `ShowModal` (TRX de `44956801926`). CT-04 da `Actual: "tras-aplicar estado=NotAvailable…"`; CT-11 da `Actual: ""`. Siguen cayendo por aserción, y la tabla de la entrega (`Evidence`) da la causa real. La errata es del texto del paquete | Registrar |
| **M-06** | M | `rv/` | **Faltan en la copia:** `relay-subst-check.json`, que el encargo dice que está (con él, `Firmas` y el GREEN literal en el detalle del relevo), y la carpeta `clean/` de la prueba de limpieza. **No hay `TreeSha` en las corridas locales** (null, admitido por INV-11). **Las compilaciones Debug** que usaron las corridas con `--no-build` no tienen registro; la entrega lo declara | Completar la custodia |
| **M-07** | M | `tests/RackCad.Tests/PushBackDraftLaunchGuardTests.cs:29`, :31 y :201 | La copia de CT-PBT4B-19 arrastra un separador duplicado y el encabezado «CT-19» sobre CT-PBT4C-15. Es cosmético; el resto de la clase es la copia exacta de `PushBackPanelDraftGuardTests.cs`, salvo los nombres, los archivos y la línea de `cubre` | Nada |

---

## Diseño → código

| Decisión | Lo que pide | El código (`7cc1076e`) | ¿Conforme? |
|---|---|---|---|
| **D-PBT4-12, el botón** | `BrowserFieldEditAllButton` justo tras «Editar», con sus cuatro atributos; `Visibility` = `ShowEditAllButton`; el `Click` a `OnBrowserFieldEditAllClick` = `Contained(() => _viewModel.Browser.Field.EditAll())` | `WorkspacePanelView.xaml:224-228`; `WorkspacePanelView.xaml.cs:86`; FVM:236-237: las condiciones de «Editar» salvo la de solo lectura (superficie fijada, sin vista previa, sin fallo, sin diagnóstico que se quede, sin elección, y `!Exists \|\| CanEdit`) | Sí. Véase OWNER-02 (un borrador Clean marcado como posiblemente obsoleto) |
| **D-PBT4-12, el gesto** | `GetField`; con texto pendiente, `CommitField` como frontera (`Invalid`: no abre y da el aviso; `Busy` o `Rejected`: `RefusalOf`); después `TryEditAll(session, rack, null, null)`; la superficie; `NotBegun`/`NotAdmitted`, como `BeginEdit`; `Render` y `_changed` | FVM:432-490 y `EditAllNoticeOf` FVM:1334-1346 | Sí |
| **D-PBT4-13, el puerto** | `TryEditAll` como único miembro nuevo, con su cuerpo por defecto `=> EditAllOutcome.NotAvailable(null);` | `IWorkspacePilotEditPort.cs:123` | Sí |
| **D-PBT4-13, WEI** | `EditAllStatus` (7) y `EditAllOutcome`, tras `EditIntentOutcome` | `WorkspaceEditIntent.cs:264-327` | Sí |
| **D-PBT4-13, el anfitrión** | `_fullEditHost` y `SetFullEditHost` junto a `SetPilotUpdateHost`; la operación explícita con la marca como envoltorio; `EditAllOfPilot` con `PassGate`, después el vuelo y después la contención con `Report` y `NotePilotEdit` por aplicar | WHC:88-90, :305-310, :1163-1176 y :1179-1224 | Sí |
| **D-PBT4-13, los dos reenvíos** | La compuerta (`_depth.IsInside ? Waiting(InsideNoticeReason) : _inner.TryEditAll`) y el adaptador del Plugin (`=> _gate.TryEditAll(request)`) | `WorkspacePilotSupport.cs:208-210`; WCo:332 | Sí, y nada más en el puerto |
| **D-PBT4-14, K = 1** | La marca envuelve `TryEditAll`, el inicio del borrador y `ShowModal`; la tabla de estados con el editor abierto | WHC:1165-1175: `_pilotReadInProgress` se pone antes de `EditAllOfPilot`, que llama a `TryBeginPilotEdit` (:1197) y a `ShowModal` (:1215), y se quita en `finally`. CT-05 fija las 19 líneas | Sí |
| **D-PBT4-15, el modal** | `private sealed class FullEditorModal : IWorkspaceFullEditHost`, con `using (_controller.EnterPanelModal()) _open(launch);`; cableado tras `SetPilotUpdateHost` | WCo:335-352 y :114. `EnterPanelModal()` tiene **un** llamador en `src/` (WCo:350) | Sí |
| **D-PBT4-15, el abridor** | `internal static class RackWorkspaceEditorWindow`, `Show(object)`: `ForDraft(...)` y `AcApplication.ShowModalWindow(window)`, sin `Owner`, sin `RackCad.Application.Workspace` y sin nada tras cerrar | `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs:13-20`. `ForDraft` tiene **un** llamador en `src/` (:17) | Sí |
| **D-PBT4-13/15, el lanzamiento y el adaptador de `Draft/`** | `For(draft, port)`: el `Editable` actual, el `RackId` y el proyecto del sobre del `Origin` (nulo admitido); `ApplyToDraft`: `SetCurrentDraftState(new PushBackFullDraftState(edited, CurrentDraftState.Origin))`, `applied()` una vez con `Updated` | `PushBackFullDraftLaunch.cs:14-28`; `PushBackFullDraftStatePort.cs:22-29`: el origen se conserva y la marca *known clean* acaba por `NotePilotEdit` (WHC:1213, :887). Sin `#nullable` y sin `?` (V51-01) | Sí |
| **D-PBT4-16, OB-01.3** | Los seis sujetos sin los ocho nombres | CT-07 en verde. Escaneé yo las líneas añadidas a `src/`: ningún `LockDocument`, transacción, `Commit(`, `Database`, `File.`, `MessageBox`, `Dispatcher` ni `Task` | Sí. Véase M-02 |
| **D-PBT4-17, «Cerrar»** | Solo en WORKSPACE, un `if` antes de la línea de hoy y la constante | W:3436 y :3463; `HasUnappliedDraftChanges` sigue a `false` en CLASSIC (W:3445). `RackPushBackSystemWindow.xaml` no cambia | Sí (INV-05) |
| **C13** | La lista cerrada, por nombre y por archivo | Censo propio sobre el diff, igual al de `inv/inv-green-final.txt`: WHC +8, WEI +21, FVM +7, WCo +6, el puerto +1, la compuerta +1, PV.xaml +1, PV.xaml.cs +1, W +1 y los cuatro archivos nuevos (2, 3, 5 y 2). Los cambios prescritos que no son declaraciones (`RaiseAll`, el primer `if` de `PendingWork` y la llamada del getter) son los únicos. WHC no cambia ningún cuerpo existente | Sí |
| **INV-06** | WHC conserva sus dos interfaces; ningún tipo de `Application.Workspace` lleva `Subscription\|Editor\|Draft` | `IWorkspaceFullEditHost`, `EditAllStatus` y `EditAllOutcome` cumplen; la declaración de la clase WHC no cambia | Sí |

---

## La lista, los `Skip` y los equivalentes

**La lista:** `pbt4c-substitution-list-4.tsv`, sha256 `9bd4c17a…`, igual en el árbol. Tiene 4 filas.

**Los `Skip`:**
- **Lo saltado es exactamente la lista.** Comparé el conjunto `NotExecuted` de las TRX de PB-T4c con el de la CI de PB-T4b (`44903818806`, `c065080d`):
  - Core, 458 → 461: son PEG:155, CT-PBT1B-15 y CT-PBT4B-20;
  - UI, 48 → 49: es CT-PBT2B-06;
  - ninguna otra fila cambia, ni en el RED ni en el GREEN.
- **En los 4 archivos solo cambia la línea del atributo** (`git diff -U0`):
  - PEG:155, `PushBackSharedReferenceGuardTests.cs:306`, `WorkspaceFullCopyGuardTests.cs:568` y `PushBackWorkspaceApplyCloseTests.cs:686`;
  - la línea siguiente es el método de la lista, en `d5026014`.
- **El motivo es el literal.** Con `SKIPS` leído de `pbt4c_skips_v50.py` sin ejecutarlo, cada archivo es su base con su línea `[Fact(Skip = "…")]` y nada más, en RedSha y en CurrentSha: **PASA**, 4 de 4. `check-skips-red.txt` y `check-skips-green.txt` dicen PASA (son el mismo archivo). No ejecuté `check_skips_pbt4c_v50.py`.

**Los equivalentes** (sha256 del blob, igual en RED y en GREEN, e igual al bloque del anexo V51-A):

| Equivalente | `EquivalentSha` | Lo que conserva | Lo que cambia (diff propio contra el original de `d5026014`) |
|---|---|---|---|
| `PushBackEditAllPortEquivalenceGuardTests` | `1a2607d3…` | `PortViolations`, los enums, la reflexión y `#nullable` | Solo la fila 18, `TryEditAll`, con su cuerpo por defecto en la expresión. Las 17 filas y el cuerpo de la prueba, idénticos |
| `PushBackEditAllNameWritersGuardTests` | `3744992127…` | Los ayudantes (`Clean`, `Inject`, `MethodText`, `WriterInsideDrawPushBackView`, `OrderOf` y demás), las 7 asignaciones, el orden y las 3 autopruebas | Solo `"archivos=14"` → `"archivos=15"` |
| `PushBackEditAllFullCopyGuardTests` | `4619b6d3…` | El archivo entero de CT-PBT4B-20 | La clase renombrada y `Restore`, con la línea exacta del `Skip` de PEG:155; nada más (`diff` de 2 bloques) |
| `PushBackEditAllApplyCloseTests` (UI) | `7c245826…` | `Sesion`, `Nueva`, `Cierre` y las demás ventanas | Solo la línea de la ventana 4, con la pregunta combinada |

**En las dos ramas:**
- los tres de Core pasan en el RED y en el GREEN (CI y local `d0`/`g0`);
- el de UI cae en el RED por `Assert.Equal` (`Collections differ`, primera línea del StackTrace en `PushBackEditAllApplyCloseTests.cs:160`) y pasa en el GREEN.

---

## El punto 4 (a): las dos desviaciones declaradas

**(i) ¿El contrato exige una sola corrida de la suite completa en local?** No. Lo que pide:
- **`ExpectedEvidence[5]`:** «en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED […] y sobre el GREEN, y tres corridas en verde de los filtros con ExpectRed sobre el GREEN»;
- **INV-PBT4C-10:** la suite completa de Core en local está **permitida** («la de Core, sí»), no exigida;
- **los totales exactos de cada rama** (F4B-E46, `ExpectedEvidence[14]`) los da la CI, y el relevo los toma de sus TRX;
- **el contrato no fija** ni una orden única ni el `--blame-hang-timeout`.

Las tres particiones son disjuntas y su suma da los totales exactos (lo comprobé en las TRX):
- **RED:** 3 315 (`Workspace`) + 47 (la clase sola) + 16 257 (el resto) = **19 619**. Passed 2 860 + 47 + 16 242 = **19 149**; Failed **9**; Skipped 455 + 0 + 6 = **461**.
- **GREEN:** 3 315 + 16 257 + 47 = **19 619**. Passed **19 158**; Failed 0; Skipped **461**.
- **BaseSha:** 3 315 + 16 290 = **19 605** (19 147 + 458).

Los 85 filtros salen de esas particiones (`inv/filters-*.txt`, todos con su `MinSelected`). Los de UI se corrieron uno a uno: los 19 del contrato y ninguno más.

**(ii) ¿Tenía el Worker esa latitud?** Por el contrato, sí:
- partir la suite y dar más tiempo a una clase son decisiones de **procedimiento local** que el contrato no regula, y no cambian ningún resultado;
- **no es como CT-20 (5):** aquella cambiaba el **contenido** de una CT contra §5.2, una decisión que C07 reservaba;
- **C09** («a test would hang») no se dispara, en mi lectura. La prueba no cuelga: es lenta y pasa en todas las corridas (6 min 45 s en BaseSha, 14 min 6 s y 8 min 34 s, y en las tres CI). Además es una prueba existente, a la que aplicaría C16 si fallara, y no falló.

Una salvedad que no puedo comprobar: el prompt de la delegación no está en `rv/` ni en git. Si fijaba `--blame-hang-timeout 10m`, el cambio a 20 y 30 minutos es una desviación del procedimiento de la delegación que el Coordinator registra. No es una decisión del Owner.

La primera corrida abortada coincidió con filtros de UI en paralelo, y la entrega lo declara. Sus resultados se descartaron.

**(iii) ¿La vuelve más lenta PB-T4c?** No: ya era lenta en PB-T4b. Es `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild…`, en `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs:1912-1945`. La añadió el RED de PB-T4b (`c1e4f458`). Hace un `Prepare` completo por cada número de toques del catálogo (:1928), así que su coste es cuadrático, y PB-T4c no toca ese camino.

| Corrida | SHA | Duración de la prueba | Paso «dotnet test Core» |
|---|---|---|---|
| CI de PB-T4b, `44903818806` | `c065080d` | **276,1 s** | 12:25:37 → 12:31:28 (5 min 51 s) |
| CI del RED de PB-T4c, `44956801926` | `280c79be` | **320,7 s** | 22:27:11 → 22:34:30 (7 min 19 s) |
| CI del GREEN de PB-T4c, `44959653142` | `7cc1076e` | **187,7 s** | 23:01:10 → 23:06:08 (4 min 58 s) |
| Local, BaseSha (`b1`) | `d5026014` | 405,3 s | — |
| Local, RED, la clase sola (`r2c`, 20 min) | árbol del RED | 846,0 s | — |
| Local, GREEN, la clase sola (`g2`, 30 min) | árbol del GREEN | 514,0 s | — |

En las tres CI es, con mucho, la prueba más lenta: la siguiente tarda 31-52 s. El riesgo que esto deja en la CI va a OWNER-05.

---

## D2

- **¿Una copia comparte un objeto mutable fuera de las dos referencias de cada lado?** No.
  - `ApplyToDraft` recibe una copia de la ventana (`W:3273`, `Copy(editable)`), y `SetCurrentDraftState` copia otra vez con el copiador inyectado (`DraftSession.cs:345-350`).
  - La ventana copia el lanzamiento al cargar (`W:177`).
  - `ProjectOf` deserializa un proyecto nuevo del texto del origen, que es un `string`.
  - `PushBackFullDraftLaunch.For` entrega por referencia el `Editable` actual. No es una copia: es el estado vivo, que sale por referencia por diseño (`DraftSession.cs:287`, INV-F4T1-19) y que la ventana copia.
  - CT-11 (primera parte) fija las huellas de las dos referencias antes y después de cada aplicar.
- **¿Una línea añadida muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?** No. Ninguna línea añadida a `src/` nombra `WorkingBaseline`, `LastModuleReconciliation`, `RackModuleReconciliationResult` ni `PushBackSystem`. CT-15, la copia exacta de G-L1, pasa sobre los dos archivos nuevos de `Draft/`.
- **¿Se traduce algún estado del piloto al modelo completo?** No. `FindPilotDraft` y `ViewOfPilotDraft` operan sobre `DraftSession<PushBackFullDraftState>` (WHC:1857). Los nombres «Pilot» son heredados.
- **La condición 7 por el puerto** está cubierta solo con el informe de módulos (I-01).

---

## Las CT

Hay **un `[Fact]` por CT,** ninguna `[Theory]` y un solo `Assert.Equal` por CT:
- Core: 6 + 3 + 1 + 1, con `namespace RackCad.Tests`;
- UI: 4, con `namespace RackCad.UI.Tests`.

Las líneas esperadas son, una a una, las de §5.3. Los dobles nuevos son clases anidadas privadas.

| CT | ¿Fija lo de §5.3? | En el RED (CI) | Notas |
|---|---|---|---|
| 01 | Sí | Aserción: `BrowserFieldEditAllButton=0` | — |
| 02 | Sí: los cinco casos | Aserción: `aviso=[]` | — |
| 03 | Sí: los 11 casos (sin anfitrión, sin borrador, la cola, Clean, Dirty, Conflict, Applying, Unknown, Error, texto pendiente y otra sesión) | Aserción: `NotAvailable motivo=null` | — |
| 04 | Sí | Aserción: `tras-aplicar estado=NotAvailable` | M-02 (`estado-igual`), M-05 |
| 05 | Sí: las 19 líneas; `lecturas-dentro` medidas dentro de `Inside` | Aserción | `CrossBoundary` con `TabChange`, que es bloqueante (`DraftSession.cs:91`) |
| 06 | Sí | Aserción: `estado=[Limpio] aviso=[]` | — |
| 07 | Sí | Pasa (`ExpectRed` false) | **Autoprueba:** 48 de 48 «falla», pero no depende del sujeto (M-02) |
| 08 | Sí | Aserción: `FullEditorModal=0` | — |
| 09 | Sí: `ambas`, `solo-sin-aplicar`, `ninguna` y `clasico-modulo` (D-S10), con la costura de D-S7 | Aserción: la pregunta de hoy | M-03. Cierra ventanas con trabajo pendiente bajo el aviso sustituido, como el equivalente del paquete y PB-T2b |
| 10 | Las líneas, sí; el camino, no (sin la compuerta) | Aserción: `NotAvailable` | M-01 |
| 11 | Las líneas, sí; la receta de la condición 7, no | Aserción: `Actual: ""` | **I-01**, M-05 |
| 12 | Sí: `new WorkspacePilotEditGate(core, depth)` | Aserción | — |
| 13 | Sí: `ControllerViolations`, copiada literal de PEG:659-707, con 18 operaciones | Aserción: `the adapter lacks TryEditAll` | **Autoprueba:** muta el texto real, y la línea base exige `violaciones=[]`, así que discrimina |
| 14 | Sí | Aserción: `llamadas=[]` | — |
| 15 | Sí. Es copia exacta de CT-PBT4B-19: el `diff` solo cambia el comentario, la clase, los archivos, el método y `cubre` | Pasa (`ExpectRed` false) | **Autoprueba:** las 15 de G-L1 se inyectan en cada archivo y exigen detección en todos, y la línea base exige cero violaciones, así que discrimina. Las de pureza son de añadir texto, heredadas |

**El RED y el GREEN, en las TRX de `rv/`:**
- **Los 4 filtros con `ExpectRed` caen enteros por aserción,** con la primera línea del StackTrace bajo `tests/`:
  - Core, 9 de 9 (`PushBackEditAllTests` 6 y `PushBackEditAllGuardTests` 3);
  - UI, 5 de 5 (las 4 CT y el equivalente).
- **Totales:**
  - CI del RED: Core 19 619 = 19 149 + 9 + 461 y UI 2 486 = 2 432 + 5 + 49;
  - CI del GREEN: Core 19 619 = 19 158 + 0 + 461 y UI 2 486 = 2 437 + 0 + 49.
- **Ninguna prueba falla fuera de la lista** (C16).
- **Las tres corridas en verde** (`e1` a `e3`): 6, 3, 4 y 1 Passed cada vez.
- **Ningún `warning CS`** en los registros de build; los avisos por código son iguales a la línea base.
- **La CI del RED:** `tests` y `ui-tests` en failure, `build-ui` en success y `build-plugin` omitido.
- **La CI del GREEN:** 4/4, con «Build Plugin without AutoCAD» en success.

## Contención

- **Escrituras en el DWG:** ninguna en los seis sujetos ni en las líneas añadidas. La única lectura del dibujo es la de `TryBeginPilotEdit`, cuando no hay borrador, antes del modal.
- **El abridor** no hace nada tras `ShowModalWindow`.
- **En las pruebas:**
  - ninguna ventana abierta desde el panel;
  - ningún `MessageBox` real: el aviso está sustituido en CT-09;
  - ningún configurador: la costura `HeaderConfiguratorDialog`;
  - nada de BOM, biblioteca ni «Insertar».
- **La UI en local:** solo los 19 filtros del contrato, nunca la suite completa.
- **No verificable aquí:** la prueba de limpieza (`clean/` no está en `rv/`).

---

## Hallazgos OWNER

- **OWNER-01. La disposición de I-01:**
  - o ratificar la receta de CT-11 (el informe con `GetUninitializedObject`, como CT-PBT4B-10);
  - o sacar una versión del paquete que fije la receta de la línea base con otro fixture y compruebe que su línea `isdirty=False` se cumple de verdad, dado el comparador por proyección (`PushBackEditableStateCopyTests.cs:1318-1359`).
  Si el texto de D-PBT4-18 («no compara la línea base») es inexacto, también conviene corregirlo.
- **OWNER-02. «Editar todo» sobre un borrador Clean marcado como posiblemente obsoleto.**
  - **«Editar»** recarga antes, con `TryReload` (FVM:386-388; O-01, carga perezosa).
  - **«Editar todo»** abre el editor sin recargar: `EditAllOfPilot` no mira `IsPossiblyStale` ni `NeedsLazyReload` (WHC:1194-1206), `CanEdit` tampoco (WHC:1886-1887), y el botón se ofrece (FVM:236-237).
  - **La consecuencia:** se edita sobre una base vieja y el borrador pasa a Dirty y obsoleto, con el camino de «Conservar mi borrador», en lugar de la recarga silenciosa.
  - **El paquete no lo trata** (D-PBT4-13, §5.3). Decidir si es lo querido, y mirarlo en el Smoke-PB con un cambio externo del dibujo y un borrador Clean.
- **OWNER-03. BOM y «Guardar en biblioteca» en la ventana de «Editar todo».**
  - Por D-PB-07 (`owner-f4b-dpb07-extensiondata-decisions.md:7-11`, «BOM y biblioteca») siguen visibles en WORKSPACE: `ApplyModeChrome` solo oculta las inserciones (W:3642-3649; W.xaml:755-758).
  - Con PB-T4c, la ventana es alcanzable desde el producto por primera vez.
  - «Guardar en biblioteca» escribe un `.rackcad.json` en disco y ejecuta `EnsureId` y `SetName` sobre la identidad de la ventana (W:3495-3530). El BOM puede exportar XLSX o CSV.
  - Ni el DWG ni el perfil, pero INV-PBT4C-04 dice que «la ventana solo entrega sus ediciones al puerto del lanzamiento», y §5.5 no lo lista.
  - En el Smoke-PB: mirar las dos salidas desde «Editar todo», y si el `SetName` de la biblioteca llega al borrador con «Aplicar al borrador».
- **OWNER-04. Lo que §5.5 no lista y es un cambio observable de PB-T4c** (paq. :1710-1732):
  - la pregunta combinada de «Cerrar», con el aviso real de AutoCAD;
  - el resumen y «Modificado» del campo tras cerrar;
  - el aviso de M-04, si una excepción del editor llega tras aplicar.
- **OWNER-05. La prueba lenta, frente al fusible de la CI.**
  - **El fusible:** `ci.yml:38-42` dice «El maximo observado de este job es ~2.5 min» y usa `--blame-hang-timeout 5m` (:137).
  - **La prueba** tarda 3-5,3 min en la CI. En el RED de PB-T4c pasó del umbral (320,7 s) sin abortar, porque, en mi lectura, el detector mide la inactividad y otras pruebas terminaban en paralelo.
  - **No lo causa PB-T4c,** que lo hereda de PB-T4b. Pero es frágil: si cambia el paralelismo o la máquina, la CI de PB-T4d o del cierre podría abortar.
  - Registrarlo (`ideas-futuras.md`) o decidir antes de PB-T4d.

**¿Alguna decisión que el contrato no le dejaba al Worker?** Las dos recetas no declaradas:
- la de CT-11 (I-01);
- el camino de CT-10 (M-01).

Las desviaciones de procedimiento local (4 a) entran, en mi lectura, en su latitud.

---

## Lo no verificado

- **No ejecuté pruebas ni compilé.** Que la receta literal de CT-11 dé `isdirty=True` es una inferencia desde `PushBackEditableStateCopyTests.cs:1318-1359`, no una medida.
- **`relay-subst-check.json` no está en `rv/`,** así que no revisé su detalle (`Firmas`, los `Skip` archivo a archivo según el relevo, los `EquivalentSha` y los `warning CS` del relevo). Lo comprobé todo por mi cuenta.
- **No ejecuté** `green_pbt4c_v51.py`, `make_equivalents_pbt4c_v50.py` ni `check_skips_pbt4c_v50.py`:
  - **el esqueleto y el GREEN,** con un lector propio de las piezas sobre `d5026014`;
  - **los equivalentes,** contra el texto del anexo V51-A, la descripción de §6.3/anexo y un `diff` contra sus originales;
  - **los `Skip`,** con `SKIPS` leído por `ast`.
- **El prompt de la delegación** no está en `rv/` ni en git. No sé si fijaba `--blame-hang-timeout 10m`.
- **Las corridas locales:**
  - tienen `TreeSha` null y se hicieron sobre el árbol antes de cada commit;
  - las compilaciones Debug que usaron no tienen registro;
  - la CI sobre los SHA exactos lo confirma, pero la equivalencia exacta del árbol local no la vi.
- **La semántica de `--blame-hang-timeout`** (que mide la inactividad, no el tiempo de cada prueba) es mi lectura de vstest. No la comprobé.
- **La prueba de limpieza:** `clean/` no está en `rv/`.
- **Lo que solo se ve en AutoCAD:** el modal real, la paleta deshabilitada, el foco, los eventos del puente durante el modal y el redibujo.
