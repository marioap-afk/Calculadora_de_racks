# Revisión independiente del código: I-64 F4B PB-T4a (`F4B-PBT4-FULL-READ-PATH`), intento 1

**Objeto:** `git diff 5aaacce3..faef7020`: RED `214f8c75`, GREEN `faef7020` y BaseSha `5aaacce3`. WorkRunId `R20261120T193653Z-de54`.
**Autoridades:** el contrato `docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-READ-PATH/coordinator/gate-contract.json` y el paquete v4.3 `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`, con sha256 `4f482279…`, que coincide con el que cita el contrato. Se citan como «paquete», con su número de línea en `faef7020`.

**Cómo leí:**
- el código y los documentos, solo por objetos git (`show`, `diff`, `grep`, `ls-tree`) desde el checkout principal;
- la entrega, en la copia `rv/` del scratchpad.

No leí ni nombré el worktree de la rama. No compilé ni ejecuté pruebas. Los guiones de Python (`-I`, escritos con la herramienta de archivos) solo leyeron blobs exportados y TRX.

---

## Veredicto

**ACEPTABLE CON MENORES. B 0 / I 0 / M 7, más 3 hallazgos OWNER.**

El RED es exactamente el esqueleto del parche custodiado aplicado a la base, más las tres clases nuevas y la línea del `Skip`. El GREEN es, carácter a carácter, el texto literal de D-PBT4-02 y D-PBT4-03. La CI y los totales cuadran con F4B-E46 en su única rama, y no hay ninguna violación de D2.

Los menores son debilidades de discriminación de algunas CT (casi todas heredadas de la letra del paquete) y notas de proceso. Los OWNER son riesgos de comportamiento y de coste para el Smoke-PB.

## Recuento

| Severidad | N.º | Ids |
|---|---|---|
| Bloqueante | 0 | — |
| Importante | 0 | — |
| Menor | 7 | M-01 a M-07 |
| OWNER | 3 | OWNER-01 a OWNER-03 |

---

## Tabla de hallazgos

| Id | Sev. | Dónde (`sha`:ruta:línea) | Qué | Recomendación |
|---|---|---|---|---|
| M-01 | M | `faef7020`:`tests/RackCad.Tests/PushBackFullReadPathTests.cs:109-148`; `PushBackFullReadSupport.cs:24-28` | **CT-02 casi tautológica respecto del Plugin.** El kit hace él mismo las dos lecturas sobre un escaneo y las une con `FromReads`. `mismo-escaneo` compara `OriginKey` y la base de dos lecturas que el kit tomó del mismo objeto. En el RED falla solo porque `FromReads` es inerte. La propiedad real de RIA (el lector completo sobre el **mismo** `input`) solo la fija CT-07, por regex sobre `PushBackFullDraftReader.Read(input, catalog)`. Sigue la letra de §5.1 («por `PushBackFullReadKit` (`FromReads`)»), así que no es defecto del Worker | Anotarlo en el registro; en PB-T4b, si se quiere, una CT por comportamiento sobre RIA (hoy imposible sin AutoCAD) |
| M-02 | M | `PathTests.cs:171-182`; `Support.cs:53,68-73` | **CT-03 no distingue `CurrentDraftState.Origin` de `InitialDraftState.Origin`.** El doble responde siempre `EditIntentKit.TwoViews`, con el mismo texto de origen, así que un WHC que pasara `InitialDraftState.Origin` también daría `origen-del-borrador=True`. Lo que sí discrimina es un texto de otro miembro o `null`. El literal de WHC lo sostienen solo la igualdad de texto (INV-05; comprobada aquí, véase la tabla de diseño) y la verificación | Ninguna en PB-T4a. En PB-T4b, una CT con base y estado actual de textos de origen distintos (tras un rebase de «Conservar») |
| M-03 | M | `PathTests.cs:207-216`; `faef7020`:`src/…/Draft/PushBackOriginByText.cs:20` | **CT-04 no fija la comparación «byte a byte».** No hay un caso casi igual (mayúsculas o espacios), así que `OrdinalIgnoreCase` o un recorte pasarían igual. Las líneas de §5.1 no lo piden, y el código es el literal del paquete | Registro; si se reabre el paquete, añadir `choose casi-igual=null` |
| M-04 | M | `rv/worker-handoff.json` (UnexpectedFindings[0]; RunRef `r0-tmp1`/`r0-tmp2`/`r0-tmp3`); `rv/tests/r0/r0-tmp*.trx` | **El oráculo de `<clave>` y `<estado>` salió de clases temporales.** `TmpMeasurePathTests`/`TmpMeasureGuardTests` llevaban una copia en memoria del GREEN literal, se ejecutaron (dos de ellas con filtro de método en Core: `TmpMeasurePathTests.CT_PBT4A_0` y `…CT_PBT4A_07`) y se borraron antes del RED. Lo permite INV-PBT4A-10: acaban en `Assert.Fail(string.Join(…))`, guardan TRX y log, no entran en ningún commit, y la restricción de filtros es solo de UI. Se declara en la entrega. Los valores coinciden con autoridades independientes de PB-T3a (`PushBackFullDraftLoadTests.cs:157-159` y `:268`: pb-21-cola fuera, pb-20-ab completo admitido). Nota de proceso, no defecto | Ninguna; dejar constancia en el cierre |
| M-05 | M | `PathTests.cs:165-166`; `GuardTests.cs:61-62`; `Support.cs:82-90,106,116` | **Pasos fuera de `Paso`.** En CT-03 y CT-05, `new PushBackFullReadRig(reader)` corre fuera de `Paso`, y el rig lleva `Assert.Equal`/`Assert.True` internos (estos sí, dentro de `Paso`). Si el constructor del rig lanzara, la CT caería por excepción y no por su aserción. La regla «EXCEPCION» de §5.0 cubre las llamadas al API nuevo, y el rig solo usa API existente, así que no hay incumplimiento. En el RED las cinco cayeron por aserción | Ninguna |
| M-06 | M | `rv/worker-handoff.json`, Evidence[7] | Dice «the three commits», pero son dos (RED y GREEN, sin RR). Lo corrige en la misma frase («two commits, no RED repair»). Cosmético | Ninguna |
| M-07 | M | `faef7020`:`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:224` | **La relectura con origen descarta `ChosenReference`** (pasa `null`). Hoy no se nota: los cuatro gestos del panel construyen la petición con `null, null` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:364,473,553,622` en `faef7020`), y el único lector de producción es el de NH (`WorkspaceController.cs:106`). Un llamador futuro que pasara una referencia en una de las cinco relecturas la vería ignorada en silencio. Es texto literal del paquete (D-PBT4-03, paso 3) | Anotarlo para PB-T4c, cuyo gesto lleva `ChosenReference` nulo (D-PBT4-12) |

**No encontré ningún defecto de código** en el alcance de PB-T4a: el GREEN es el literal prescrito y no añade nada más.

---

## Alcance y forma (punto 1)

**El RED es el esqueleto exacto.**
- Exporté los blobs de `5aaacce3` y apliqué `pbt4a-skeleton.patch`, con sha256 `e99c87c6…de19`, igual que el contrato, fuera de cualquier repositorio. El resultado es igual a los blobs de `214f8c75` (sin contar CRLF/LF) en WEI, la interfaz, `PushBackOriginByText.cs` y DWG.
- `git diff --name-status 5aaacce3 214f8c75`: A de la interfaz, A de OBT y A de las tres clases de prueba; M de WEI y M de DWG. RIA, NH y WHC no cambian.
- DWG cambia solo en la línea 60, la del `Skip`, con su motivo literal (§6.1:1291).

**El GREEN** (`git diff --name-status 214f8c75 faef7020`) toca solo `PushBackOriginByText.cs`, WEI, WHC, RIA y NH, con numstat 28/3, 7/1, 15/5, 27/1 y 19/4.
- No toca nada de `tests/`, y la interfaz no cambia desde el RED.
- GREEN es hijo directo del RED, y no hay ningún commit «RED repair».
- El trailer es `Co-Authored-By: Claude Sonnet 5.5`, en los dos commits.

**Nada fuera de las 10 rutas.** No cambian UI, Domain, `RackPushBackCommands.cs`, `RackCommandSupport.cs` ni `docs/` (`git diff --stat 5aaacce3 faef7020 -- docs/` vacío).

**El censo de declaraciones, por diferencia:** son 14, la lista cerrada de PBT4A-C13.
- **WEI:** `FullRead`, `Catalog` y `FromReads`.
- **La interfaz:** el tipo y `ReadIntentAtOrigin`.
- **OBT:** la clase, `Choose`, `AtKey` y `ReadAt`.
- **RIA:** el 2.º constructor (de 1 a 2), `Full`, `Input` y `AtOrigin`.
- **NH:** `ReadIntentAtOrigin`.
- **WHC:** ninguna. Solo la variable de patrón `originReader`, local.
- Cambian de firma, como se prescribe, el constructor privado de WEI y `ReadOnActiveDocument` (`NH:233`).
- No hay ningún ayudante privado (INV-PBT4A-13).
- La diferencia cuadra con el censo de la entrega. Los absolutos (192→195, 127→127, …) no los reproduje (véase «No verificado»).

## Tabla diseño → código (punto 2)

Todo en `faef7020`. «OK lit.» significa que comprobé por guion la igualdad exacta con el bloque del paquete.

| Diseño (paquete) | Código | Resultado |
|---|---|---|
| D-PBT4-02, WEI: ctor con 2 parámetros, `FullRead`, `Catalog`, `FromPilot` con `null, null`, `FromReads` con 3 `ArgumentNullException` (paq. 263-290), las 4 fábricas de fallo con `null, null` y los `using` | `WorkspaceEditIntent.cs:5-6,39-45,56,59,61-73,75-81` | OK lit. |
| D-PBT4-02, RIA: 2.º ctor y propiedades (paq. 302-312) y el `using` de Draft antes del de Pilot | `RackPilotIntentAdapter.cs:7-8,39-45,51,53` | OK lit. |
| RIA `Read`: la única línea que cambia es la del éxito, con el lector completo sobre el mismo `input` y tras el piloto (paq. 319) | `RackPilotIntentAdapter.cs:133-136` | OK lit. Siguen 1 `CaptureDrawing` (:129), 1 `new PushBackPilotIntentScan(` (:133), 1 `PushBackPilotIntentReader.Read(` (:134) y 1 `PushBackFullDraftReader.Read(` (:136). `LockDocument` y `StartTransaction` siguen en 1 (diff sin cambios en esas líneas). La firma de `Read` no cambia (:107-113), y la regex de CT-07 es la de `WorkspaceEditIntentGuardTests.cs:141` |
| D-PBT4-03, `AtOrigin` (paq. 409-420), antes de `ReadPreviewWitness` | `RackPilotIntentAdapter.cs:157-168` | OK lit. Solo llama a `ReadAt`; no tiene lectura del dibujo, ni literales, ni `ex.Message`. Cae al resultado del `Primary` (`return read`) si `ReadAt` da `null` |
| OBT entero (paq. 353-399) | `Draft/PushBackOriginByText.cs:1-47` | OK lit. (archivo idéntico). `Choose` (:16-24): solo `Mutable`, `Ordinal`, y `null` con 0 o con varias. Es pura: LINQ sobre entradas inmutables (`DraftStaleBaseEntry` es un `readonly record struct`, `DraftStaleBase.cs:25`) |
| `Choose` sobre `BuildStaleBase` del escaneo, «como UR:645-649» | `OBT:41`; UR `PushBackPilotUpdateRun.cs:642-655` | Equivalente: UR construye la base con origen vacío y `ReadAt` con el del `Primary`, pero `Analyze` clasifica sin mirar el origen (`PushBackPilotIntentReader.cs:113-119`). `Choose` aparece una vez |
| La interfaz (paq. 431-442) | `IWorkspaceEditIntentOriginReader.cs:1-12` | OK lit.; sin cambios desde el RED |
| NH, ediciones 1 a 6 (paq. 446-471) | `WorkspaceNavigationHost.cs:196, 211, 220-231, 233, 262-263, 264-266` | OK lit. Siguen 1 `LoadCatalog()` (:241), 2 `LockDocument` (:245 y el del resumen), 1 `RackPilotIntentAdapter.Read(` (:255), `AtOrigin` dentro del bloqueo y tras `Read`, y `FromReads` sin `FromPilot`. `ReadIntentAtOrigin` usa el mismo `ReadOnActiveDocument` |
| WHC: las 5 sentencias con el bloque literal (paq. 478-480); `Begin` intacto | `WorkspaceHostCore.cs:409-411, 505-507, 594-596, 681-683, 767-769`; `Begin` en :328 | **El texto del GREEN es igual al de la base con las 5 sentencias sustituidas** (igualdad exacta del archivo entero), en `TryCheckPilotEdit`, `TryReloadPilotEdit`, `TryConfirmPilotBase`, `TryKeepPilotEdit` y `TryDiscardAndReloadPilotEdit`, cada una con su `var draft = _drafts.Find<PushBackPilotDraftState>(…)` antes. Siguen 9 `Find<PushBackPilotDraftState>`, 6 `.ReadIntent(` y el orden |
| El lector de producción implementa la interfaz | `WorkspaceController.cs:106` → `WorkspaceNavigationHost.cs:57,64` | El `is` de WHC ve el `PilotIntentReader` real, sin ningún envoltorio, así que el cambio observable M-23 sí ocurre en producción |
| `FromReads` no puede lanzar en producción | `RackPilotIntentAdapter.cs:125-148` (las cinco salidas de fallo con `Pilot = null`); `PushBackFullDraftReader.cs:51-117` (nunca devuelve null y contiene sus excepciones); `NH:242-243` (catálogo comprobado) | `Succeeded` implica el ctor de 3 argumentos, así que `Full` y el catálogo no son nulos |

## D2 y su límite (punto 3)

- **Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult`.**
  - Las 133 líneas añadidas a `src/` solo asignan a locales o a propiedades propias en constructores: `FullRead`, `Catalog`, `Pilot`, `Full`, `Input` y `FailureReason`.
  - No aparece ninguna ficha de G-L1 (`WorkingBaseline`, `LastModuleReconciliation`, `.System`, `.Structure`, `.Modules`…).
  - CT-08 lo confirma sobre `PushBackOriginByText.cs`: `g-l1 … reglas-sin-coincidencias=True`.
- **Ninguna copia comparte un objeto mutable.**
  - `AtKey` (`OBT:26-36`) crea un escaneo nuevo con los sobres en un diccionario nuevo y comparte los `Facts`, la evidencia y el testigo. No es una copia del estado del borrador, y además todo es inmutable:
    - el constructor de `PushBackPilotIntentScan` copia hechos y sobres a vistas de solo lectura (`PushBackPilotIntentScan.cs:40-77`);
    - `RackSiblingScanFact` solo tiene getters (`RackSiblingMembership.cs:16-52`);
    - `PushBackPilotPreviewWitness` es un `record`.
  - `EditIntentReadResult` guarda referencias a las dos lecturas y al catálogo, pero no es una copia del estado, y el anfitrión todavía no usa `FullRead` ni `Catalog`.
  - La lectura del piloto y la completa no comparten objetos: el lector completo hace su propia lectura del piloto (`PushBackFullDraftReader.cs:56`).
- **Nada traduce un estado del piloto al completo.**
  - En el producto, la lectura completa sale de `PushBackFullDraftReader.Read(input, catalog)` (`RIA:136`, `OBT:44`) y NH pasa `read.Full` (`NH:265`).
  - En las pruebas, el kit usa el lector completo (`Support.cs:27`).
- **No se reabre L-1** (C17).

## Los CT (punto 4)

| CT | Clase / `[Fact]` | ¿Fija lo de §5.1? | Discriminación | RED (CI `214f8c75`) | GREEN |
|---|---|---|---|---|---|
| 01 | Path / 1 | Sí, las 4 líneas literales | Buena: identidad por `ReferenceEquals`, nulos por tipo de excepción | Falla por `Assert.Equal` (`PathTests.cs:94`): `completo=False catalogo=False` | Pasa |
| 02 | Path / 1 | Sí. `pb-20-ab piloto=NotAdmitted completo=Admitted mismo-escaneo=True` es literal; `pb-21-cola` y `auth-13` dan `NotAdmitted` en los dos lectores, como fija PB-T3a | Débil para el Plugin (M-01) | Falla por aserción (:137): `completo=NotAdmitted` | Pasa |
| 03 | Path / 1 | Sí: `Begin lector=ReadIntent` y las 5 líneas `… lector=ReadIntentAtOrigin origen-del-borrador=True` | Media (M-02) | Falla por aserción (:185): `CheckNow lector=ReadIntent` | Pasa |
| 04 | Path / 1 | Sí, las 8 líneas (`B2` y `A1` como `<clave>`, `Admitted` como `<estado>`) | Buena en mutable/no mutable: el `xref` da `ReadOnly`, así que quitar el filtro `Mutable` daría `C3`. Débil en «byte a byte» (M-03) | Falla por aserción (:240): `choose uno-mutable=null` | Pasa |
| 05 | Guard / 1 | Sí: `<op> lector=ReadIntent` × 5, con el `EditIntentFakeReader` existente | Guarda por construcción | Pasa | Pasa |
| 06 | Guard / 1 | Sí, las líneas de CT-PBT3A-12 iguales. **Admite RIA y nada más**: `MayNameTheModel` = la de `PushBackDraftWriteGuardTests.cs:48-51` en `5aaacce3` más `RackPilotIntentAdapter.cs` (`GuardTests.cs:84-88`) | Las 10 autopruebas, heredadas (WHC y `WorkspacePilotSupport.cs`); ninguna sobre otro archivo del Plugin, como en el original | Pasa | Pasa |
| 07 | Path / 1 | Sí, las 13 líneas literales | **Las autopruebas en negativo discriminan, no son tautológicas.** Cada una cambia el texto en memoria y exige que `Lines(...)` dé otra cosa: sin el lector completo, `completo=0`; con `CaptureDrawing` dentro de `AtOrigin`, `lecturas-del-dibujo=1` en el cuerpo; un 2.º `AtOrigin`, `nh-AtOrigin=2`; `FromPilot` en NH, `FromPilot=1`; una 2.ª lectura del piloto en RIA, `piloto=2`. En el texto del RED, dos de ellas darían `pasa` (no hay nada que quitar), y en el GREEN, las cinco `falla` (`r0-tmp3`) | Falla por aserción (:355): `ria-Read completo=0` | Pasa |
| 08 | Guard / 1 | Sí, las 32 líneas | Las reglas de G-L1 (Tokens…Inject y las 15 autopruebas) son **idénticas** a las de `PushBackDraftPortSharedReferenceGuardTests.cs` en `5aaacce3`, y `ForbiddenTerms`/`Purity`, idénticos a CT-PBT3A-14 (comparado miembro a miembro) | Pasa | Pasa |

- **Forma:**
  - hay 8 `[Fact]` (5 + 3), ninguna `[Theory]` ni `InlineData`, y un solo `Assert.Equal` final de las líneas unidas por `\n` en cada CT;
  - los kits nuevos van en el archivo nuevo `PushBackFullReadSupport.cs`;
  - `EditIntentKit`, `IntentKit`, `PilotKit` y `WorkspaceHostRig` se usan sin editarlos (el diff de `tests/` es solo DWG:60 y los 3 archivos nuevos).
- **Determinismo:**
  - no hay ni hora, ni azar, ni formato dependiente de la cultura (solo concatenación de `bool` y `enum`), ni escrituras;
  - los recorridos de archivos van ordenados con `Ordinal`;
  - hubo tres corridas verdes del filtro con `ExpectRed` (`g-1`, `g2-1` y `g3-1`).

## El RED y el GREEN (puntos 5 y 6)

| | Core (total = Passed + Failed + Skipped) | UI | Jobs |
|---|---|---|---|
| Previsto (contrato, `ExpectedEvidence[14]`) | RED 19 137 = 19 125 + 5 + 7; GREEN 19 137 = 19 130 + 0 + 7 | 2 451 = 2 430 + 0 + 21 | RED: tests en failure, ui-tests y build-ui en success, build-plugin omitido; GREEN: 4/4 |
| CI del RED (`rv/red-ci-core/core.trx`, sha256 `6C702CB9…`; `rv/red-ci-ui/ui.trx`, `753D27CB…`) | **19 137 = 19 125 + 5 + 7** | **2 451 = 2 430 + 0 + 21** | Según el registro del relevo (run 44810371492) |
| CI del GREEN (`rv/ci-core/core.trx`, `28930709…`; `rv/ci-ui/ui.trx`, `616416A1…`) | **19 137 = 19 130 + 0 + 7** | **2 451 = 2 430 + 0 + 21** | 4/4, según el registro del relevo (run 44814418683) |

- **Los fallos del RED** son exactamente CT-01, 02, 03, 04 y 07, todos por `Assert.Equal() Failure: Strings differ`, con el primer frame en `tests/RackCad.Tests/PushBackFullReadPathTests.cs` y sin ninguna línea `EXCEPCION`. Así, Failed = Selected = 5 en el filtro con `ExpectRed`, y no se cumple C04.
- **Los saltados**, en el RED y en el GREEN, son los seis de D-1 y CT-PBT3A-12.
  - Frente a la base local (`b0full`, 19 129 = 19 123 + 6), el único salto nuevo es CT-PBT3A-12.
  - El RED y el GREEN tienen el mismo conjunto de nombres, también en UI.
  - Ninguna otra prueba existente falla ni se salta (C16).
- **Las copias de los TRX** en `rv/tests/redci` y `rv/tests/gci` tienen el mismo hash que `rv/red-ci-*` y `rv/ci-*`.
- **En local:**
  - los 38 filtros sobre el RED y sobre el GREEN tienen Selected ≥ MinSelected, y el RED solo falla en el filtro 1 (5/5);
  - la suite local de Core da los mismos totales que la CI;
  - los builds Release de base, esqueleto, RED y GREEN tienen 0 errores y los mismos avisos (66/14/2, de xUnit y MSB3277), sin ningún `warning CS`.
- **Sin reparación del RED:** `rv/rr/` está vacío y no hay commit intermedio. RR-1 a RR-6 y la tabla de RR-3 no aplican.

## Contención (punto 7)

- **Sin AutoCAD real:** no hay ventana, modal, `MessageBox`, configurador, «Insertar», BOM ni «Guardar en biblioteca». Las 8 CT son de Core, sobre `WorkspaceHostRig` (un puente simulado) y texto.
- **Sin escrituras:** no hay ninguna en disco ni en el perfil. CT-06 y CT-08 solo leen archivos, y la sonda de limpieza (`rv/inv/clean-before.txt` y `clean-after.txt`) da lo mismo antes y después.
- **UI en local:** solo los 12 filtros de UI del contrato, dos veces cada uno; nunca la suite completa.
- **Filtros de método:** solo en las corridas temporales de Core (M-04), que INV-PBT4A-10 no restringe.
- **Sin cambios** en UI, Domain ni el Plugin, fuera de RIA y NH.

---

## Hallazgos OWNER (punto 8)

### OWNER-01. El origen «deriva» al `Primary` y se queda allí tras una relectura sin coincidencia

**El mecanismo.** La regla es por texto (`OBT:16-24`). Si el texto del origen del borrador ya no está, byte a byte, en un miembro mutable, la relectura cae al `Primary`, como declara D-PBT4-03. Eso pasa cuando:
- el rack se editó fuera, porque RACKEDITAR reescribe los sobres de todas las vistas;
- tras «Comprobar ahora» en `Unknown`, si el apply sí aterrizó o si el POST falló (`AppliedPostRereadFailed`).

Lo que el paquete no dice es la consecuencia: `Reload`, `DiscardAndReload` y `KeepMine` reconstruyen el borrador con la lectura del `Primary`, y a partir de ahí `CurrentDraftState.Origin` es el texto del `Primary`:
- `draft.DiscardAndReload(pilot.Read)` en `WHC:526`;
- `KeepPilotEditsOn` copia el `Origin` del nuevo inicial (`PushBackPilotMaterializer.cs:242-251`).

Las relecturas siguientes encuentran el `Primary` por texto y no vuelven nunca a la vista de la que el usuario abrió el borrador.

**El alcance.** A-T3-8 ayuda solo mientras el texto del origen está intacto, y los flujos de conflicto son justo aquellos en los que más a menudo no lo estará. No es una regresión frente a hoy: hoy siempre era el `Primary`.

**Qué mirar en el Smoke-PB:**
1. Abrir el borrador desde una vista que no sea el `Primary`.
2. Editar el rack en RACKEDITAR.
3. «Conservar mi borrador», «Descartar y recargar» o «Comprobar ahora».
4. Ver qué vista refleja después el panel y de qué vista son las relecturas siguientes.

### OWNER-02. El coste de la doble lectura (R-T4-16), mayor de lo que dice el paquete

**Cada una de las seis lecturas** (`Begin` y las recargas perezosas incluidas) hace ahora:
- el piloto 2 veces: la directa (`RIA:134`) y la interna del lector completo (`PushBackFullDraftReader.cs:56`);
- una materialización completa.

**En el caso del origen**, `AtOrigin` añade (`OBT:41-44`):
- una tercera `BuildStaleBase` (`Analyze`, con `RackSiblingMembership.Classify` sobre todos los hechos del dibujo);
- otro piloto;
- un lector completo, que hace otro piloto y otra materialización.

**En total:** unas 5 clasificaciones, 4 evaluaciones del piloto y 2 materializaciones completas, frente a 1 y 1 de hoy. El trabajo hecho en el `Primary` se descarta, y todo ocurre dentro de `LockDocument` (`NH:245-267`) y bajo K = 1.

**La letra del paquete se queda corta.** El paquete dice «las dos lecturas puras se repiten una vez» (paq. 425 y 1503). R-T4-16 dice «se mide en la precondición 6», pero no encontré ningún registro de esa medida: `git grep R-T4-16` en `docs/` solo da textos del paquete, y el registro de mediciones no la tiene.

Si no se midió, el Owner decide si cae en la condición de su respuesta Q4 («un efecto material sin medir»). En el Smoke-PB: medir el tiempo de una relectura con origen en un dibujo grande.

**Medido después, a petición de la sesión** (`read-cost/read-cost.md`, sobre copias exportadas de `5aaacce3` y `faef7020`, sin AutoCAD, con dos corridas):
- el coste lo pone la **materialización completa** (`PushBackFullDraftMaterializer` → `PushBackEditableStateSeat.LoadExisting`), de 25-78 ms por llamada, y no la doble lectura del piloto, que añade 2-10 ms;
- en `pb-07-combinado` y `pb-20-ab`: `Begin` sube de +40 a +80 ms (de 7× a 16×), la relectura con origen encontrado de +80 a +175 ms (de 16× a 31×), y la que cae al `Primary` o tiene el origen en el `Primary`, de +35 a +86 ms.

**Es material** (por encima de ~50 ms por gesto en los fixtures mayores).

### OWNER-03. Dos subcasos de la relectura con origen que ninguna CT de PB-T4a fija

**(a) Sin destinos frescos de navegación.** Si no los hay, `ChooseOriginDefinition` da 0 (`WEI:700-709`) y la lectura del `Primary` dice «falta el origen». Sigue siendo `Succeeded`, así que `AtOrigin` busca el origen por texto en todo el escaneo y la relectura **se admite** donde hoy no se admitía. Es coherente con M-23, pero amplía lo que se observa.

**(b) Tras un POST `Applied` que reancla.** El POST del piloto relee en `plan.PostOriginKey`, que es la primera vista redibujada si se borró el origen (`PushBackPilotUpdateRun.cs:601-608`). El nuevo `CurrentDraftState.Origin` debería encontrarse por texto en la relectura siguiente, pero esto solo lo fija CT-PBT4B-17, en PB-T4b y sobre el borrador completo.

**Dos casos ya declarados por el paquete,** para el Smoke-PB:
- un origen borrado fuera, que cae al `Primary`;
- dos vistas mutables con el mismo texto (por ejemplo, una copia enlazada), que dan «varios» y caen al `Primary`.

---

## No verificado

- **La CI remota:** ni el estado de los jobs (4/4 y la omisión de build-plugin en el RED), ni que los TRX procedan de esos runs y de esos SHA. Lo tomé del registro del relevo (`rv/relay-record-work.json`, RemoteFacts); los TRX no llevan el SHA. No consulté GitHub.
- **Los censos absolutos de declaraciones** (192→195, 22→26…): solo verifiqué la diferencia, con un recuento heurístico y a mano.
- **Que las clases `TmpMeasure*` se borraran del worktree,** que no leí. Consta que no entraron en ningún commit (los árboles del RED y del GREEN) y que el relevo midió el árbol limpio en la entrada.
- **El comportamiento en AutoCAD** de la relectura en el origen (M-23) y el coste real (OWNER-02).
- **Que el texto de `PushBackPilotDraftState.Origin` tras el rebase de un POST** sea el de `PostOriginKey` en todos los caminos (OWNER-03b): solo lo leí en `PushBackPilotUpdateRun.cs:601-608`.
- **Nota de proceso:** al aplicar el parche del esqueleto para compararlo, el primer `git apply` corrió en el scratchpad, dentro de un repositorio git del perfil del usuario. Las rutas del parche quedaban fuera del directorio, así que git las ignoró y no escribió nada (lo comprobé). Repetí la aplicación con `GIT_CEILING_DIRECTORIES`, fuera de cualquier repositorio. No toqué el repositorio del proyecto.
