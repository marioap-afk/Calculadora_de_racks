# Comprobación acotada de PB-T4e (CA-T4e) sobre el paquete v5.5

**Fecha:** 2026-11-22. **Revisor:** independiente; no participé en el paquete, la propuesta ni la precondición 6.

**El objeto:** la v5.5 del paquete del contrato de PB-T4 (`pbt4/I-64-f4b-pbt4-gate-contract-package-v55.md`, sha256 `e597790ff700a2909b77c21960b5b013b2b108cf8073f2b866a0face25f8ae83`), en lo que añade o cambia para PB-T4e (`F4B-PBT4-EDIT-ALL-RELOAD`). La base es el cierre de PB-T4d, `160e019d773ba9095e2d5858cd593a954b8eec67`.

**Las entradas, con su sha256 comprobado:**

| Archivo (relativo a `S`) | sha256 |
|---|---|
| `pbt4/I-64-f4b-pbt4-gate-contract-package-v55.md` | `e597790f…ae83` |
| `pbt4/I-64-f4b-pbt4-gate-contract-package-v54.md` | `e37843fc…fc63` |
| `pbt4e/owner-pbt4e-list-decisions.md` | `d889d56b…c973` |
| `pbt4e/list-proposal.md` | `bf2b5804…a7bf` |
| `pbt4/session-pbt4e-design-decisions.md` | `0c3bfc7b…0da2` |
| `pbt4/pbt4e-green-v55.txt` | `5d26dc48…5407` |
| `pbt4/pbt4e-red-v55.txt` | `647e1853…033b` |
| `pbt4ecc/compile/pbt4e-precondition6.json` | `ef33dca0…6d21` |
| `pbt4ecc/compile/compile-check.md` | `1fd30380…505e` |

También casan con lo que dice el paquete:
- las herramientas: `green_pbt4e_v55.py` (`888c9fb2…`), `red_pbt4e_v55.py` (`6727c6f2…`), `apply_green_pbt4e_v55.py` (`832afa9e…`), `apply_red_pbt4e_v55.py` (`50b19194…`) y `v55/make_pieces_pbt4e_v55.py` (`9ffcb6b8…`);
- los registros: `cov-p6-v55.txt` (`70f0e41b…`), `required-pbt4e.tsv` (`a66fd8d4…`) y `driver-r71-tr.txt` a `driver-r75-ct.txt`, con los sha256 de `compile-check.md`.

**El método:**
- **El código y los documentos del repositorio,** solo por objetos git desde el checkout principal (`git show`, `diff`, `grep`, `ls-tree`), nunca en disco. El worktree, ni abierto.
- **Python,** solo de lectura, con guiones escritos con Write en `pbt4/_review_t4e/` y ejecutados con `python -I <archivo>`.
- **Nada compilado ni ejecutado.** Lo que dicen las corridas lo tomo de sus registros.
- **Reproducido por mí:**
  - el GREEN, aplicado en memoria sobre los textos de `160e019d`: cada OLD está una vez, y los cuatro archivos dan los sha256 de la pieza y de `driver-r72-p6.txt`;
  - el RED: los dos NEWFILE dan sus `NewFileSha`;
  - el anexo V55-A, igual byte a byte a las piezas;
  - el diff de secciones de la v5.4 a la v5.5;
  - los `RequiredTests`, contra el contrato emitido de PB-T4d;
  - las 24 líneas, contra las clases y la sonda;
  - el mapeo de líneas de la cobertura en WHC y FVM.

En lo que sigue, WHC es `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`, FVM `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs` y DS `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`. Las líneas son de `160e019d` salvo que diga «GREEN».

## Veredicto

**EMITIBLE. B 0 / I 0 / M 5.**

- El GREEN hace lo que eligió el Owner: solo la recarga, sin aviso, con la misma función que «Editar».
- Si la recarga no recarga, no se abre la ventana, y el campo muestra lo mismo que «Editar» en los cinco resultados que se alcanzan.
- Hay como mucho una lectura por gesto, y ninguna con el editor abierto.
- D-S17 y D-S18 se sostienen sobre el código de `160e019d`.
- Las CT fijan lo que dice el paquete, caen en el RED por aserción, pasan en el GREEN, y el control prueba lo que dice.
- La lista vacía se sostiene, medida y por análisis.
- El contrato literal casa con la precondición 6, y la base local casa con la CI del cierre de PB-T4d.
- **Sin decisiones nuevas del Owner.** Las condiciones de la pregunta 4 se cumplen en lo que toca a esta comprobación: 0 bloqueantes y 0 importantes, la remedición reproducida sin cambio material y la lista elegida (vacía).

**Los cinco menores** no impiden emitir.
- **M-01 es el único de código.** Con un borrador «Sin decidir» y marcado, «Editar todo» cambia de respuesta en el puerto. No se alcanza desde el panel.
- **M-02 a M-05 son de texto o de custodia.**

Conviene declarar M-01 (o cambiarlo, a elección de la sesión) y corregir M-03 y M-04 en la custodia de la emisión.

## Hallazgos

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| **M-01** | Menor | **El GREEN:** bloque 4 de `pbt4e-green-v55.txt`, `else if (draft.NeedsLazyReload)` (GREEN WHC:1205).<br>**El código de la base:** DS:298-306; WHC:1857-1867, :1203-1205 y :2021-2025; FVM:236-237 y :463.<br>**La prueba existente:** `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostFull.cs:115-123` y `:596-612`.<br>**El paquete:** D-PBT4-22, «Lo que no hace»; INV-PBT4E-04 | **Un borrador «Sin decidir» y marcado cambia de respuesta en el puerto.**<br>**Por qué:** `NeedsLazyReload` es `_registered && IsPossiblyStale && Status == Clean` (DS:306). Sin estado explícito y sin texto pendiente, `Status` llama al comparador (`IsDirty`, DS:298), y nada lo protege.<br>**El caso está modelado y probado:**<br>• un comparador que lanza da el estado «Sin decidir» (`OpenUndecidable`);<br>• `AComparerThatThrowsMakesTheFieldUndecidable…` exige `Assert.Empty(env.Hr.Faults)`;<br>• el drenaje marca todos los borradores, sin mirar el comparador (WHC:2021-2025).<br>**Lo que pasa:**<br>• **hoy,** `ViewOfPilotDraft` protege `Status` (WHC:1860-1867), y `TryEditAll` responde `Refused(EditAllStateReason)` sin ningún `Report`;<br>• **con el GREEN,** la excepción sale del `else if` y la recoge el `catch` de `EditAllOfPilot`: `Failed(<mensaje>)`, con `Report("WorkspaceHostCore.TryEditAll")`.<br>**No se ve en el panel:**<br>• el botón exige `!_view.Exists \|\| _view.CanEdit` (FVM:236-237), y «Sin decidir» no es editable;<br>• el único llamador de `TryEditAll` en `src/` es FVM:463, detrás de ese botón (`WorkspacePanelView.xaml:227`, `.xaml.cs:86`).<br>**No se abre nada en ningún caso.**<br>**Lo que no lo nombra:** ni D-PBT4-22 («Dirty, Conflict, Unknown, Error, Applying y un borrador sin la marca siguen como hoy») ni INV-PBT4E-04. Ninguna CT ni ninguna prueba existente prueba `TryEditAll` con un borrador «Sin decidir» | **(a) Lo mínimo, solo texto:** declararlo en D-PBT4-22 («Lo que no hace») y en un R-T4-41 de §7: es una diferencia solo del puerto, inalcanzable desde el panel, y no abre en ningún caso. Así no lo trata como parada C03 la revisión del código.<br>**(b) Si se quiere «como hoy» literal en todos los estados:** mover la rama detrás de las tres comprobaciones de la vista. Primero `!view.CanEdit`, y después `if (<no se acaba de empezar> && draft.NeedsLazyReload)`.<br>• Ninguna de las 24 líneas cambia: lo comprobé receta a receta.<br>• Sí cambian el GREEN, INV-PBT4E-03 y el texto de D-PBT4-22, y hay que repetir la precondición 6: sería una v5.6.<br>**Ninguna de las dos vuelve al Owner:** no cambia nada visible |
| **M-02** | Menor | `pbt4ecc/compile/cov_p6_pbt4e_v55.py:99-131` (las uniones de la base y del GREEN, y el mapeo con difflib); `cov-p6-v55.txt`; §4.5 | **El método de la cobertura tiene dos límites, y el paquete no los declara.**<br>**(1) El GREEN incluye las cuatro CT nuevas,** así que «no baja nada» no muestra por sí solo que las pruebas existentes conservan sus caminos.<br>• Aquí sí se sostiene, por análisis: ninguna prueba existente de «Editar todo» marca un borrador (`git grep` de `MarkPossiblyStale`, `DrainWithStaleHint`, `OnSourceEvent` e `Idle()` en las cinco clases que llaman a `TryEditAll` o a `EditAll()`; la vista del doble de UI, con la marca en `false`, en `PushBackEditAllWindowTests.cs:163`).<br>• También lo sostienen los cuerpos vigilados: `EditAllOfPilot` ejecuta 30 líneas en la base y 30 en el GREEN.<br>**(2) difflib empareja mal las líneas repetidas.** La `}` que cierra el bloque de «sin borrador» (base WHC:1201) queda emparejada con la que cierra el `else if` (GREEN WHC:1209), y la verdadera (GREEN :1204) cuenta como «nueva».<br>• Aquí es inocuo: las dos se ejecutan (:1204, con 2 golpes). Pero un GREEN que rompiera un camino podría quedar tapado así | **Declarar los dos límites** en §4.5 y en la cabecera del guion.<br>**Opcional,** para la parte de «las existentes»: una corrida de cobertura del GREEN sin las dos clases nuevas (`FullyQualifiedName!~PushBackEditAllReload`), comparada con la base |
| **M-03** | Menor | `pbt4/session-pbt4e-design-decisions.md:39-48` (la evaluación de R-T4-39) | **La etiqueta.** Dice «La pregunta 3 habla del conflicto de un borrador con cambios del usuario (la opción 3, no elegida)». La pregunta 3 de la 15.ª tanda es «Orden»; el conflicto es la opción 3 de la **pregunta 1**. Las preguntas 3 y 4 no se evalúan, aunque no dependen de la medida.<br>**Las premisas que no nombra:** la premisa deducida no solo estaba en las preguntas de la 15.ª tanda. El Owner la leyó en dos sitios más:<br>• `pbt4e/list-proposal.md:31`: «"Actualizar" acaba en el conflicto y en «Conservar mi borrador»»;<br>• la pregunta de OWNER-02 de PB-T4c (`pbt4ccc/owner-pbt4c-code-decisions.md:25`): «al aplicar sale el camino de «Conservar mi borrador»».<br>**La premisa que sostiene esas decisiones sigue en pie:** «no se escribe nada sin avisar». `NotAppliedPreflight` tampoco escribe | **Corregir la etiqueta** y añadir las dos premisas, con la conclusión.<br>**Contárselo al Owner en PB-T5,** como información y no como decisión, junto al paso 6 del Smoke-PB (§5.5): con un borrador Dirty y un cambio de fuera que reescriba el sobre de origen, «Actualizar» puede dar `NotAppliedPreflight` y no «Conservar mi borrador» |
| **M-04** | Menor | `pbt4ecc/compile/post-p6-v55-out.txt` (la última línea); `pbt4/v55/frag/head.md` y `ops-d.md` (modificados a las 17:32); `pbt4/v55/make_v55-out.txt`; `pbt4/v55/handoff-pbt4e.md`, §5 | **Las dos v5.5.** El registro de `post_p6_v55.sh` (17:31) acaba en una v5.5 `a241bfbe6d2a18d5fba9d154aa0975a9c38031c1d121074bb04827d541cc103d` (1 003 244 bytes). La revisada es `e597790f…ae83` (1 003 976 bytes): la dio una segunda ejecución de `make_v55.py` después de cambiar `head.md` y `ops-d.md`.<br>**Lo que falta en el traspaso:** declara el ensayo con valores de prueba (desliz 3), pero no esta segunda ejecución ni qué cambió (732 bytes).<br>**Lo que no puedo ver:** la `a241bfbe` no se conserva.<br>**Lo que sí comprobé:** las cifras de la `e597790f` casan con el JSON y con `compile-check.md` | **Declarar en el traspaso y en la custodia** que la autoridad es `make_v55-out.txt` (`e597790f`) y qué cambió en `head.md` y `ops-d.md` después de `post-p6`.<br>**Otra opción:** repetir el último paso de `post_p6_v55.sh`, para que su registro nombre la versión que se emite |
| **M-05** | Menor | `pbt4e/owner-pbt4e-list-decisions.md:16` (la opción 1 de la pregunta 1: «Toca una función»), frente a `pbt4e/list-proposal.md:35` y al GREEN | **La premisa «Toca una función» se queda corta.** El GREEN cambia el cuerpo de `EditAllOfPilot`, el aviso de `EditAll` (FVM), la línea de `BeginEdit` con la constante y los comentarios de cuatro archivos.<br>**La propuesta lo decía bien:** «toca una función de WHC y, para que el aviso sea igual que el de «Editar», unas líneas de FVM».<br>**No cambia la elección:** la pregunta 2 («el campo muestra lo mismo que «Editar»») ya pide esas líneas | **Nada en el paquete.** Anotarlo en el registro de la emisión, junto al coste de D-S17, que ya dice que A toca dos archivos de producto |

## Lo que comprobé y está bien

### 1. El GREEN frente a las decisiones del Owner

**Solo la recarga, sin aviso, como «Editar».**
- **La misma función, sin copia:** la rama nueva (GREEN WHC:1205-1209) llama a `TryReloadPilotEdit(request)` (WHC:498-576), la que usa «Editar» por el puerto (WHC:1103-1115).
- **Con `Reloaded`, sigue y abre:** el borrador es el mismo objeto, porque `DiscardAndReload` lo cambia en su sitio (DS:422-437, `Rebase` en DS:489-500). Queda Clean, registrado y sin texto pendiente, así que las tres comprobaciones de WHC:1211-1214 (GREEN) pasan y se llega a `ShowModal`.
- **Ningún aviso de la recarga:** al cerrar sale el de siempre, «Editor cerrado sin cambios en el borrador.» (CT-03, `recargado-y-cerrado`).

**Dirty, Conflict, Unknown, Error, Applying, texto pendiente, sin marca y sin registrar: como hoy.**
- `NeedsLazyReload` es falso para todos ellos:
  - con estado explícito, por `_explicitStatus` (DS:300);
  - con texto pendiente, por `_pending.Count > 0` (DS:298, que cortocircuita);
  - sin la marca o sin registrar, por los dos primeros términos de DS:306.
- Así se sigue el camino de hoy (WHC:1211-1224, GREEN), y CT-01 lo mide con Dirty, Conflict y sin la marca.
- La única excepción es M-01.

**Si la recarga falla, no se abre y el campo muestra lo mismo que «Editar».**
- **Los resultados que se alcanzan:** `Reloaded`, `NotAdmitted` (DS:427-428), `Waiting` (lector `NoActiveDocument` o `Busy`, y `NotAllowed`), `Rejected` (lector) y `Failed` (lector, lectura incompleta o excepción contenida con `Report("WorkspaceHostCore.TryReloadPilotEdit")`).
- **Ninguno abre:** todos salvo `Reloaded` devuelven `NotBegun(<la recarga>)` (GREEN WHC:1208).
- **Lo que muestra el campo (GREEN FVM:475-487):**
  - con `NotAdmitted`, el diagnóstico, la pista y, con `view.Exists`, `NotReloadedText`;
  - con los demás, `EditAllNoticeOf` → `Describe(Begin.Status, Begin.Reason)` (FVM:1343), la misma función y los mismos datos que «Editar» (FVM:413).
- **El borrador queda Clean y marcado:** CT-02 da `obsoleto=True borrador=Clean`.

**Como mucho una lectura por gesto, y ninguna con el editor abierto.**
- O se empieza el borrador o se recarga, nunca las dos cosas (`if`/`else if`).
- `GetField` y `CommitField` solo tocan memoria (WHC:1226-1301).
- La recarga va dentro de la marca de `TryEditAll` (WHC:1163-1175) y antes de `ShowModal`.
- CT-01 lo mide: `lecturas=1 lecturas-antes-de-abrir=1 lecturas-dentro=0`.

**Ningún otro cambio visible.**
- La constante `NotReloadedText` lleva el mismo texto que el literal de FVM:410; `BeginEdit` muestra lo mismo.
- Lo demás son comentarios:
  - WEI:276 y :308;
  - en el puerto, IWorkspacePilotEditPort.cs:118-121, que conserva las dos frases de `WorkspacePilotEditGuardTests.cs:1083-1084` y no lleva las de :1085-1086 (lo comprobé con `grep` sobre los dos textos);
  - el resumen de `EditAll` (FVM:426-431).
- Ningún miembro, estado, texto ni botón nuevo. La única declaración nueva es `NotReloadedText` (PBT4E-C13). Sin BOM, y saltos de línea `\n`, como la base.

### 2. D-S17 y D-S18, sobre el código de `160e019d`

**D-S17 se sostiene.**
- **La regla está en un solo sitio:** `NeedsLazyReload` (DS:306), aplicada en WHC:527.
- **Llamar a `TryReloadPilotEdit` directamente no pierde nada:**
  - los envoltorios del puerto solo añaden la marca, que `TryEditAll` ya pone (WHC:1163-1175);
  - y la profundidad de los avisos, que `TryEditAll` ya mira: `WorkspacePilotSupport.cs:173-174` y `:209-210`, y el adaptador de `WorkspaceController.cs:300` y `:332`, que solo reenvía.
- **La compuerta repetida no tiene efecto:** `PassGate` (WHC:2040-2074) lee la sonda y autoriza, y `Authorize` es pura (`WorkspaceSessionRegistry.cs:39-46`).

**D-S18 se sostiene.**
- **`view.Exists` distingue bien la recarga.**
  - `GetField` solo da `Exists=false` cuando `FindPilotDraft` devuelve nulo (WHC:1230-1231).
  - Entre `GetField` y `TryEditAll` solo corre `CommitField`, que no crea ni quita borradores.
  - Con un borrador que ya existe, el anfitrión nunca lo empieza (`draft == null`, WHC:1195).
- **El caso de error de `GetField`.** La vista de error lleva `Exists=true` (WHC:1236).
  - **Se atribuiría mal solo si** `FindPilotDraft` lanzara en `GetField` y, justo después, en `EditAllOfPilot`, devolviera nulo. El código no lo permite de forma determinista.
  - **Si lanza las dos veces,** sale `EditAllOutcome.Failed`, sin aviso de la recarga.
  - **Si lo que lanzó fue `ViewOfPilotDraft`,** el borrador existe, y el aviso, si sale, es el correcto.
- **Un valor nuevo de `EditAllStatus`** exigiría la fábrica en `src/` para escribir la respuesta del doble de CT-03, así que el RED dejaría de ser solo de `tests/`. Ninguna prueba enumera `EditAllStatus`: `git grep` en `tests/` no da nada. La única línea con `NotBegun` es la de PushBackEditAllTests.cs:245.

**Los resultados que no se alcanzan:**
- `NotNeeded`: el anfitrión acaba de mirar `NeedsLazyReload` en la misma llamada síncrona, sin cambio de estado entre medias.
- `NotFound`: la misma clave, y `FindPilotDraft` equivale a `_drafts.Find` para un `RackId` no en blanco (WHC:1841-1842).
- `Unavailable`: la compuerta ya pasó.
- `RackId` en blanco: va por la rama de «sin borrador».
- **Si se alcanzaran:** `NotNeeded` no abriría y diría «No se pudo abrir la edición: El borrador no necesita recargarse.», a diferencia de «Editar», que sigue. Hoy es inalcanzable, así que no lo cuento como hallazgo.
- **Un testigo sin clave:** el campo nunca lo manda (FVM:463 pasa `null`). Otro llamador recibiría el mismo `Failed` que «Editar».

**El caso del borde del texto pendiente:** el análisis es correcto y no hay pérdida.
- La caja de un borrador Clean marcado está en solo lectura (FVM:807-814), así que el texto pendiente tiene que venir de antes de la marca.
- `CommitField` solo deja el borrador Clean si el valor confirmado es igual al de antes.
- Es deducido y no se ejecutó, y §9 lo declara.

### 3. Las CT y el RED

**Las 24 líneas** salen de los `Finish` de las clases: 6, 4 y 5 en Core, y 9 en UI.
- Son las de §5.7 y las de la sonda sobre el GREEN (`r71-tr/green-core-lineas.txt` y `green-ui-lineas.txt`), tal cual.
- La tabla de RR-3 (15 + 9) y las cifras de `make_v55-out.txt` casan.

**En el RED caen por aserción:** la pila empieza en `Finish`, bajo `tests/` (`r71-tr/red-*-lineas.txt` y `driver-r72-p6.txt`).
- Core: 3 `Failed` en la suite completa.
- UI: 1 `Failed`, en el filtro 19.
- `Failed == Selected` en los dos filtros con `ExpectRed`.

**En el GREEN pasan las cuatro.**

**Pillan los GREEN equivocados que pide la consigna:**

| GREEN equivocado | La línea que lo pilla |
|---|---|
| Recargar siempre | `sin-marca … lecturas=0` |
| Recargar un Dirty | `marcado-sucio … lecturas=0` |
| Abrir tras una recarga fallida | `abierto=False` en CT-02 |
| Leer dos veces, o durante el modal | `lecturas=1 … lecturas-dentro=0` |
| Recargar después de `ShowModal` | `lecturas-antes-de-abrir=1` |
| Recargar antes del anfitrión del editor | `marcado-sin-anfitrion … lecturas=0` |
| Recargar por el puerto, que daría `Waiting` por la marca | `estado=Closed` |
| No mostrar el aviso | `no-recargado editar-todo … aviso=[…]` |
| Mostrarlo siempre | `sin-borrador-no-admitido … aviso=[]` y `ocupado …` |
| Avisar al recargar | `recargado-y-cerrado aviso=[Editor cerrado…]` |
| Recargar en el campo (B) | `llamadas=[GetField,TryEditAll,GetField]` |

**Lo que no pilla, y no importa:**
- un GREEN que llamara a `TryReloadPilotEdit` sobre todo borrador y siguiera también con `NotNeeded`. Es casi equivalente, y el GREEN es literal y se comprueba por sha256 ([15]);
- una recarga puesta antes de la comprobación de la actualización en vuelo. Lo mismo.

**El control prueba lo que dice:**
- `control_pieces_v55.py` parte la pieza por prefijo: Application, el anfitrión y sus comentarios, y UI, el campo. Las dos piezas son idénticas en `r74` y en `r75`.
- **Con solo el campo:** caen las tres de Core y pasa CT-03 (`driver-r74-ct.txt`).
- **Con solo el anfitrión:** al revés (`driver-r75-ct.txt`).

**Las clases nuevas no tocan nada existente:**
- son dos archivos nuevos, con sus dobles como clases anidadas privadas;
- usan los ayudantes existentes sin editarlos (`FullReadRig`, `FullEditIntentFakeReader`, `FullEditIntentKit`, `IntentKit`, `FullPrepareKit`, `FullWriteKit`, `PilotEraseScopeDouble`, `RackEmbedStore` y `EditorDiscardPromptCollection`);
- no llevan `#nullable`. `Nullable` está desactivado en `Directory.Build.props`, según `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:9`;
- **ningún filtro heredado las selecciona:** lo comprobé como subcadena, por ejemplo `PushBackEditAllTests` frente a `PushBackEditAllReloadTests`.

### 4. La lista vacía

**Ninguna prueba existente prepara un borrador marcado para «Editar todo».**
- Las cinco clases que llaman a `TryEditAll` o a `EditAll()` no marcan, no drenan ni dan una vista marcada.
- Así, `EditAllOfPilot` no entra en la rama nueva para ninguna prueba existente.
- La línea `cola estado=NotBegun` de CT-PBT4C-03 (PushBackEditAllTests.cs:150-157 y :245) es la rama de «sin borrador», que no cambia.

**Las guardas de texto sobre `src/`, una a una:**

| Guarda | Por qué sigue en verde |
|---|---|
| `WorkspacePilotEditGuardTests.cs:541-589` | El cuerpo de cada envoltorio no cambia; el recuento de `_pilotReadInProgress = true` sigue en 6; `:574` muta la primera aparición de `return TryReloadPilotEdit(request);`, que sigue siendo la del envoltorio (WHC:1109), porque el GREEN usa `var reload = …` (y, además, `EditAllOfPilot` va después en el archivo) |
| `WorkspacePilotEditGuardTests.cs:1078-1087` | Las dos frases siguen y las prohibidas no están |
| `WorkspacePilotSurfaceGuardTests` (:56-61 y :82-120) | `EditAll` no está en ninguna de las dos listas de miembros, no nombra `TryReload` y no añade ningún setter |
| CT-PBT4C-07 (`PushBackOpenerClassicPathGuardTests.cs:20-71`) | El texto nuevo no nombra ninguno de sus ocho nombres prohibidos |
| La tabla H-07 de `WorkspaceHostHardeningGuardTests.cs:421-489` | El GREEN no añade ningún literal al anfitrión |

**Todas las guardas de texto que leen los cuatro archivos son de Core**, 31 archivos por `git grep`, y la suite completa de Core corrió en el RED y en el GREEN.

**La UI fuera de los 19 filtros:**
- **El anfitrión real:** ninguna prueba de UI lo usa salvo `PushBackEditAllWindowTests`, el filtro 17.
- **Los modelos de vista del panel y el puerto:** quien los usa está bajo `RackCad.UI.Tests.Workspace` (filtro 2), en `PushBackFullPanelFieldTests` (filtro 12) o en `PushBackEditAllWindowTests`.
- **Las guardas de UI que recorren `src/RackCad.UI`** (`UiSystemBoundaryGuardTests`: XAML y URI de recursos) **o las fuentes de pruebas de UI** (`SelectiveWindowConstructionGuardTests`) no ven nada de lo que cambia.
- No se corrió en local (INV-PBT4E-10); la CI del RED y la del GREEN lo miden.

### 5. La cobertura

`cov-p6-v55.txt` sostiene lo que dice, dentro de su universo: Domain, Application y UI; Core entera y los 19 filtros (las tres corridas juntas y cada filtro en su proceso).
- **Lo que no baja:** 69844 líneas y 28820 ramas de la base siguen ejecutadas o cubiertas en el GREEN; 0 sin pareja.
- **El texto cambiado:** 1 línea (FVM:410, de 44 a 52 golpes, ramas 2/2).
- **Lo nuevo,** con todas sus ramas: las 5 de WHC y la de FVM (GREEN FVM:482, ramas 2/2: con y sin `view.Exists`).
- **Bajan 0.**

**El método es sólido:**
- compara conjuntos de (punto, path) y no recuentos, con el punto por su rango en la línea;
- lleva por difflib solo los cuatro archivos que cambian;
- exige todas las ramas en lo nuevo y en lo cambiado;
- vigila los cuatro cuerpos (en `TryReloadPilotEdit`, de 24 a 27 ramas; en `EditAll`, de 18 a 22).

Sus dos límites, en M-02.

### 6. El contrato literal de §6.5

**El alcance.**
- **`AllowedWriteScope`:** 6 rutas, las cuatro del GREEN y las dos del RED.
- **`ForbiddenWriteScope`:**
  - lo da el generador con [PKC-C], que en `src/` prohíbe «todo `src/` salvo el `AllowedWriteScope`» (§6.0);
  - la lista de PB-T4d (335 entradas) prohíbe los prefijos `src/RackCad.Application/Workspace/` y `src/RackCad.UI/`, que el generador tiene que partir;
  - en `tests/`, la lista de PB-T4d más sus cinco clases nuevas; ninguna de ellas está en el alcance de PB-T4e;
  - el número no lo verifico (abajo).

**Los invariantes y las paradas.**
- **Invariantes:** 14 (INV-PBT4E-01 a 14).
- **Paradas propias:** C01 a C14, C16, C17 y C18, los mismos ids que PB-T4d; con las 17 estándar, 34.
- **El bloque literal** de §6.5.1 es ASCII entero (0 caracteres fuera de ASCII), con el `TaskId` de 24 caracteres.

**Los `RequiredTests`: 90 = 88 + 2.**
- **Los 88 heredados:** filtro, proyecto y `MinSelected` iguales fila a fila a los de `F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json` en `160e019d`, que es el mismo archivo que el de la corrida `R20261122T171951Z-bc04`.
- **El único cambio:** el n.º 84, `ExpectRed` de true a false, declarado, porque CT-PBT4D-01 pasa desde el cierre.
- **Ningún `MinSelected` baja** (`required-pbt4e.tsv`: base, RED y GREEN).
- **Los 2 nuevos,** con `ExpectRed` true: 3 y 1, con 0 en la base, 3 y 1 `Failed` en el RED, y 3 y 1 en el GREEN.
- **Los 19 filtros de UI** son los de `ui_filters_pbt4e_v55.txt`.

**Los totales, reproducidos de los registros:**

| Rama | Core | UI |
|---|---|---|
| **Base** | 19418 = 19015 + 0 + 403 (`driver-r73-bc.txt`) | La unión de los 19 filtros: 899 = 869 + 0 + 30 |
| **RED** | 19421 = 19015 + 3 + 403 (`driver-r72-p6.txt`) | 2465 = 2416 + 1 + 48 |
| **GREEN** | 19421 = 19018 + 0 + 403 | 2465 = 2417 + 0 + 48 |

Las de UI del RED y del GREEN son la línea base de la CI (2464 = 2416 + 0 + 48) más la diferencia de los filtros (899 → 900).

**La base local casa con la CI del cierre de PB-T4d.**
- **La CI:** `45057868915`, sobre `7db6fd0c`, da Core 19 418 = 19 015 + 0 + 403 y UI 2 464 = 2 416 + 0 + 48 (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md:2246` y `superseding-characterization.md:635`, en `160e019d`).
- **El mismo código:** `git diff --stat 7db6fd0c 160e019d -- src tests` sale vacío. Los cuatro archivos citados y DS tampoco cambian desde `7cc1076e`.

**La evidencia,** de [0] a [15], es coherente con §5.4 y §6.0:
- `ui-tests` en failure en el RED, por CT-03;
- los sha256 de los cuatro archivos de `src/` del GREEN y de su base;
- la comprobación mecánica de las piezas.

### 7. R-T4-39

**Estoy de acuerdo con la sesión:** la medida no cambia ninguna premisa de lo que el Owner eligió.
- **Pregunta 1:** describe el defecto, y la opción 3, que trataba el conflicto de un Dirty, no se eligió.
- **Pregunta 2:** la opción elegida se justifica por «abrir sobre una base que ya se sabe vieja».
- **Preguntas 3 y 4:** no dependen de la medida.
- **La premisa de seguridad de OWNER-02 de PB-T4c,** «no se escribe nada sin avisar», sigue siendo cierta: `NotAppliedPreflight` no escribe.
- **El motivo medido** («Ningún sobre editable de la relectura coincide con el origen del borrador.») es el `NoOriginReason` de `PushBackPilotUpdateRun.cs:585`, que la receta provoca al renombrar también el sobre de origen.
- **El paso 6 del Smoke-PB ya no da por hecho el conflicto:** dice «da lo de hoy».

Lo que falta en el registro de la sesión, en M-03.

### 8. Decisiones nuevas del Owner

**Ninguna.**
- M-01 no es visible desde el panel. Se resuelve declarándolo o con una reordenación que no cambia ninguna línea; las dos caben en el diseño que el Owner dejó a la sesión.
- M-03 es información para PB-T5, no una decisión.
- M-02, M-04 y M-05 son de texto o de custodia.

### Además

**Las 99 secciones sin cambio:**
- Mi propio diff de secciones da 100 iguales, contando el preámbulo, y 14 cambiadas, exactamente las que nombra la cabecera.
- Las nuevas son 11: las ocho de la cabecera y tres de más, que son el título, el registro de cambios v5.4 → v5.5 y la cabecera de la v5.4 trasladada.
- La cabecera de la v5.4 pasa entera a su sección, sin otro cambio que el título.
- PB-T4a a PB-T4d quedan enteras.

**Las citas `ruta:línea` de D-PBT4-22, D-S17 y D-S18 casan con `160e019d`,** y también la del origen: la revisión del código de PB-T4c, :240-246.

## Lo no verificado

- **No compilé ni ejecuté nada,** por regla. Los resultados de las corridas los tomo de sus registros, cuyo sha256 comprobé. No volví a calcular la cobertura desde los JSON de coverlet ni abrí las TRX.
- **El número de `ForbiddenWriteScope`:** «medido por el generador», y no existe todavía. Tampoco que el generador parta bien los prefijos `src/RackCad.Application/Workspace/` y `src/RackCad.UI/`.
- **La suite completa de UI:** solo por análisis (§4). La miden la CI del RED y la del GREEN.
- **M-01, sin ejecutar:** es lectura del código. No corrí una sonda con `OpenUndecidable`, `MarkPossiblyStale` y `TryEditAll`.
- **La diferencia entre la v5.5 `a241bfbe` y la `e597790f`** (M-04).
- **El Smoke-PB,** sin AutoCAD. Una duda para su guía: en el paso 5, «borrar fuera todas las vistas del rack» puede quitar la pestaña o el borrador del panel antes de «Editar todo». En ese caso no se vería el aviso esperado.
- **Un desliz mío, declarado:**
  - lancé una vez `python -I -` con un heredoc vacío; no ejecutó código, pero va contra la regla;
  - en la misma orden, un heredoc vacío creó `pbt4/_review_t4e/fws_prefix.py`, que después reescribí con Write.
  - No escribí nada en el repositorio, no abrí el worktree y no leí archivos en disco del checkout principal: solo sus objetos git.

## La línea BOUNDED

> EMITIBLE; B 0 / I 0 / M 5; sin importantes que corregir (M-01: con un borrador Sin decidir y marcado, el else-if evalua NeedsLazyReload sin proteger el comparador y TryEditAll pasa de Refused sin Report a Failed con Report, solo en el puerto e inalcanzable desde el panel porque el boton exige CanEdit: declararlo o mover la rama tras las comprobaciones de la vista; M-02: el metodo de cobertura incluye las CT nuevas en el GREEN y difflib empareja mal la llave de WHC:1201; M-03: la evaluacion de R-T4-39 llama pregunta 3 a la opcion 3 de la pregunta 1 y no nombra las premisas de list-proposal.md:31 ni de OWNER-02; M-04: post-p6 registra una v5.5 a241bfbe y la revisada es e597790f; M-05: la pregunta 1 dice toca una funcion); D-S17 y D-S18 se sostienen sobre 160e019d; sin decisiones nuevas del Owner; precondicion 6 sobre 160e019d: base Core 19418 = 19015 + 0 + 403 igual a la CI 45057868915, RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403, UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48, las 24 lineas de la sonda iguales a las CT, control campo y anfitrion correcto, cobertura PASA, RequiredTests 90 = 88 + 2 iguales a PB-T4d salvo el n.o 84 con ExpectRed false; GREEN y RED reproducidos por objetos git con los mismos sha256; 99 secciones sin cambio

## Recomprobación 1 (v5.6, solo el diff)

**Fecha:** 2026-11-22. **El objeto:** la v5.6 (`pbt4/I-64-f4b-pbt4-gate-contract-package-v56.md`, sha256 `6b8939ae26d39392824108fe9b73bc3741f30a45657810a2ecf534e9a40cf66a`), solo en lo que cambia frente a la v5.5. Con ella van sus piezas, la precondición 6 repetida (las copias r76 a r83) y el traspaso. **Las reglas** son las del primer encargo: objetos git en `160e019d`, nada compilado ni ejecutado, Python solo con `python -I <archivo>` (esta vez sin ningún desliz) y escrituras solo en `S`.

**Las entradas, con su sha256 comprobado:**

| Archivo (relativo a `S`) | sha256 |
|---|---|
| `pbt4/I-64-f4b-pbt4-gate-contract-package-v56.md` | `6b8939ae…f66a` |
| `pbt4/pbt4e-green-v56.txt` | `a745c273…8158` |
| `pbt4/pbt4e-red-v56.txt` | `d052f73d…c9e1` |
| `pbt4/session-pbt4e-design-decisions.md` | `b21f2af4…1e6d` |
| `pbt4ecc/compile/pbt4e-precondition6.json` | `daea5984…c169` |
| `pbt4ecc/compile/compile-check.md` | `549766eb…d426` |
| `pbt4ecc/compile/post-p6-v56-out.txt` | `36f88c52…b485` |
| `pbt4/v56/handoff-pbt4e-v56.md` | `dc7e6775…45e0` |

También casan con lo que dice el paquete:
- **las herramientas:** `red_pbt4e_v56.py` (`8c38e532…`), `apply_red_pbt4e_v56.py` (`59a0fcb3…`), `green_pbt4e_v56.py` (`b3aba680…`), `apply_green_pbt4e_v56.py` (`492aaab1…`) y `v56/make_pieces_pbt4e_v56.py` (`8a00c0d2…`); `green_` y `red_` solo cambian la versión respecto a las de la v5.5;
- **los registros:** `v56/make_v56-out.txt` (`53e884a8…`), `v56/historia/v56-38efafdb.md` (`38efafdb…`), `cov-p6-v56.txt` (`057591b7…`), `cov-p6-existentes-v56.txt` (`3887c2ef…`) y `driver-r76-tr.txt` a `driver-r83-p6.txt`, con los sha256 de `compile-check.md`.

**Reproducido por mí:**
- **el GREEN de la v5.6,** aplicado en memoria sobre `160e019d`: cada OLD está una vez, y WHC da `6eb9bf7a…`; los otros tres archivos dan lo mismo que en la v5.5. Es lo que escriben las copias r82, r83, r80 y r81;
- **los dos `NewFileSha`:** el de Core, `9e849864…`, y el de UI, `a29d7a36…`, sin cambio;
- **el anexo V56-A,** igual byte a byte a las piezas;
- **el diff de secciones** de la v5.5 a la v5.6, y el de tres vías con la v5.4;
- **PBT4E-C11,** frente a PBT4D-C11 del contrato emitido;
- **las 25 líneas,** frente a las sondas de r76 y r82;
- **la cobertura de las dos líneas VARIA,** leída en los JSON de coverlet de r73, r79, r72 y r83.

**Veredicto:** EMITIBLE, B 0 / I 0 / M 2 (M-06 y M-07, uno de texto y otro de custodia, sin decisión del Owner; M-01, M-02, M-03 y M-04, resueltos; M-05, para el registro de la emisión).

### Los hallazgos nuevos

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| **M-06** | Menor | **El registro de la sesión:** D-S17 (`pbt4/session-pbt4e-design-decisions.md:11-20`), frente a D-S19 (:58-81) y al GREEN de la v5.6 | **D-S17 sigue describiendo el sitio de la v5.5.** Dice «una rama `else if (draft.NeedsLazyReload)`, tras la de «sin borrador» y antes de las comprobaciones del estado, del texto pendiente y de `ShowModal` … Es el GREEN literal del anexo V55-A» (:13), y «la de empezar o la de recargar (`else if`)» (:19).<br>**D-S19 dice que ajusta el sitio** (:58), pero D-S17 no lleva ninguna nota que remita a ella, como sí la lleva la corrección de M-03 (:48). El título sigue siendo «sobre la v5.5» (:1).<br>**El paquete está bien:** D-PBT4-22 dice «D-S17 (confirmada; el sitio exacto, D-S19)». Pero quien lea D-S17 sola en el registro custodiado se queda con el sitio de antes | **Lo más barato:** declararlo en el registro de la emisión (F4B-E60), junto a M-05: «el sitio de D-S17 lo sustituye D-S19 (`!begunHere && draft.NeedsLazyReload`, tras las tres comprobaciones; anexo V56-A)».<br>**La otra opción:** añadir esa nota en D-S17 y en el título. Pero cambia el sha256 del registro (`b21f2af4…`), que el paquete cita en la cabecera, en §1, §8.5 y §11 y en §6.5.2. Haría falta una v5.7 solo de texto |
| **M-07** | Menor | **La v5.6:** la cabecera (:5) y §1, la fila «La comprobación acotada de la v5.5». **§11.10** | **La v5.6 cita la versión de antes de esta recomprobación.** Cita `independent-review-pbt4e-check.md` con `27636ed5…ef72`, que es el informe sin esta sección. Al añadirla, el sha256 del archivo cambia.<br>**Lo comprobé:** los primeros 30 371 bytes del archivo resultante siguen dando `27636ed5…`; lo de antes no se tocó.<br>**§11.10** custodia «la comprobación acotada CA-T4e … y su recomprobación», así que la custodia necesita el sha256 final | **En la custodia** (F4B-E60), registrar el sha256 final del archivo y que `27636ed5…` es el de sus primeros 30 371 bytes, la comprobación de la v5.5.<br>No hace falta cambiar el paquete |

### Lo que comprobé de M-01 a M-05

**M-01 → D-S19: resuelto.**

**Nada evalúa la marca antes de las tres comprobaciones de la vista.** En el GREEN, `EditAllOfPilot` va así:

| Paso | Líneas (GREEN WHC) |
|---|---|
| La compuerta, la actualización en vuelo y el anfitrión del editor | :1185-1191, sin cambio |
| La búsqueda, con `begunHere = false` | :1194-1195 |
| La rama de «sin borrador», con `begunHere = true` | :1196-1203 |
| Las tres comprobaciones, sobre `ViewOfPilotDraft` | :1205-1208 |
| **Solo después,** `if (!begunHere && draft.NeedsLazyReload)` | :1215-1219 |
| `ShowModal` | :1228 |

`NeedsLazyReload` no aparece en ningún otro sitio de `EditAllOfPilot`.

**Los estados distintos de «Clean y marcado» responden exactamente como en `160e019d`:**

| Estado | La respuesta | Dónde (GREEN, y en la base) |
|---|---|---|
| Applying o no registrado | `Waiting` | GREEN :1206, base :1204 |
| «Sin decidir», Unknown o Error | `Refused(EditAllStateReason)` | GREEN :1207, base :1205 |
| Con texto pendiente | `Refused(EditAllPendingTextReason)` | GREEN :1208, base :1206 |
| Conflict | Abre como hoy: `NeedsLazyReload` es falso por el estado explícito, sin llamar al comparador | — |
| Sin la marca | Abre como hoy: corta en `IsPossiblyStale` | — |
| Dirty | Abre como hoy: `NeedsLazyReload` es falso | — |
| Un borrador que el gesto acaba de empezar | No se recarga (`begunHere`) | — |

**Sobre el Dirty sin estado explícito:** el comparador se llama una vez más, después de `ViewOfPilotDraft`. Solo cambiaría algo con un comparador no determinista. El de producción es una igualdad pura (`PushBackFullDraftStateComparer.cs:19-25`), así que no es un hallazgo.

**Sigue habiendo como mucho una lectura por gesto, y ninguna con el editor abierto:**
- la estructura lo garantiza: se empieza el borrador o se recarga, nunca las dos cosas;
- la recarga va dentro de la marca de `TryEditAll` y antes de `ShowModal`;
- las líneas de CT-01 que lo fijan no cambian.

**La línea nueva fija lo que dice.**
- **La receta:** un borrador empezado en el registro con `PilotEditThrowingComparerFull`, que siempre lanza «the comparer threw» (`tests/RackCad.Tests/Workspace/WorkspacePilotEditHostFull.cs:16-27`, la misma que usa `OpenUndecidable` en :115-123). Después, `MarkPossiblyStale` y `TryEditAll`.
- **Lo que fija:** `Refused` con `EditAllStateReason`, sin lecturas, sin fallos y con la marca puesta.
- **Lo medido:**

| Copia | El GREEN | `marcado-sin-decidir` | CT-01 |
|---|---|---|---|
| r76 | El de la v5.5 (`apply-green.txt`: WHC `fa19d28c`) | `estado=Failed motivo=[the comparer threw] … fallos=[WorkspaceHostCore.TryEditAll]` | Cae |
| r82 | El de la v5.6 (WHC `6eb9bf7a`) | `estado=Refused …` | Pasa |
| r83 | El de la v5.6 | — | La suite de Core del GREEN da 0 `Failed` |
| El RED | — | Ya da `Refused` | Cae por `marcado-limpio` |

**Las 24 líneas que comprobé siguen iguales y en su orden:**
- las 15 de Core de la v5.5 están en la v5.6, más la nueva;
- la clase de UI es idéntica byte a byte;
- con el GREEN de la v5.5 (r76), la sonda da exactamente las otras 15 de Core y las 9 de UI; con el de la v5.6 (r82), las 25;
- §5.7 las lleva todas.

**Los textos dicen lo mismo que el código:**
- **D-PBT4-22:** el punto 1, las citas de WHC:1203-1206 y :1209-1215 de `160e019d`, «Lo que no hace» y la nueva sección D-S19;
- **§6.5.1:** INV-PBT4E-03 y 04, y PBT4E-C13, con `begunHere` y la rama como cambios que no son declaraciones;
- **R-T4-41 (§7), §8.5, §0 y §1;**
- **la única excepción es el registro de la sesión** (M-06).

**El control** (r80 y r81) se hizo con el GREEN final: `a745c273`, con las piezas de control `d46c2550` y `c0712516`. Da lo mismo que en la v5.5.

**M-02: resuelto.**

**Los dos límites están declarados:** en §4.5 y en la cabecera de `pbt4ecc/compile/cov_p6_pbt4e_v56.py` (:16-25, sha256 `cbbffa87…`). El resto del guion solo añade `MODO=existentes` (:47 y :209, que no cuenta como CAE una línea nueva sin ejecutar) y el título.

**La corrida opcional está bien hecha:**
- **Core sin las dos clases** (`FullyQualifiedName!~PushBackEditAllReload`): 19418 = 19015 + 0 + 403 (`driver-r83-p6.txt`), que es el total de la base.
- **`ui_union18_v56.txt`** es `ui_union_v55.txt` sin el filtro 19 (por `diff`). La unión da 899 = 869 + 0 + 30 las tres veces.
- **Cada filtro en su proceso:** se usan los `green-ui01` a `18` de la principal, porque ningún filtro heredado selecciona las clases nuevas; del lado de la base, `base-uif01` a `18`.

**Y sostiene que las pruebas existentes conservan sus caminos:**
- PASA, sin líneas ni ramas perdidas;
- los cuerpos vigilados quedan iguales:

| Cuerpo | Líneas (base → GREEN) | Ramas (base → GREEN) |
|---|---|---|
| `EditAllOfPilot` | 30 → 30 | 21 → 21 |
| `TryReloadPilotEdit` | — | 24 → 24 |
| `EditAll` | 34 → 34 | 18 → 18 |
| `BeginEdit` | 33 → 33 | 25 → 25 |

- el texto cambiado, FVM:410, sigue con sus 44 golpes;
- sin las CT nuevas, solo se ejecutan GREEN WHC:1195 y :1202, y :1215 tiene 3 de 4 ramas: solo las CT nuevas llegan a la recarga, como dice §4.5.

**Las 2 líneas VARIA** (`src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:37-38`, las propiedades `Available` y `Selected`) **son variación entre corridas, sin relación con el cambio.**
- PB-T4e no toca ese archivo ni su carpeta.
- Lo medí en los JSON de coverlet: el mismo código de la base da 0 de 3 uniones en r73 y 2 de 3 en r79.

| Copia | Uniones que la cubren | El filtro 04 |
|---|---|---|
| r73 (la base de la v5.5) | 0 de 3 | La cubre |
| r79 (la base de la v5.6) | 2 de 3 | La cubre |
| r72 (el GREEN de la v5.5) | 0 de 3 | La cubre |
| r83 (el GREEN de la v5.6) | 0 de 3; y 1 de 3 en la unión de 18 | La cubre |

**La regla las trata bien:**
- la línea sigue ejecutada en el GREEN, por el filtro 04 en su proceso, así que el PASA no se toca;
- la base no es estable (2 de 3), así que son VARIA y no PERDIDA (`cov_p6_pbt4e_v56.py:285-287`), como dice D-S16;
- en la v5.5 no salían porque las dos bases daban 0 de 3.

**M-03: resuelto.** La etiqueta está corregida (`session-pbt4e-design-decisions.md:44-48`). Las dos premisas, `list-proposal.md:31` y `owner-pbt4c-code-decisions.md:25`, están nombradas con su conclusión (:50-54), y la información para PB-T5 está en :56.

**M-04: resuelto, y basta como «un solo armado».**
- **`post-p6-v56-out.txt`** nombra la `6b8939ae` (:30-31).
- **Frente al intento 2** solo cambian esas dos líneas. Las del sha256 del JSON (`daea5984`) y de `compile-check.md` (`549766eb`) son iguales.
- **La `38efafdb` y la `6b8939ae`** solo difieren en la línea 9.
- **Los dos `make_v56-out`** solo difieren en la línea de las 10 secciones de más y en la de la versión.
- **La versión que se emite** sale de un solo `make_v56`, el último paso de un `post-p6` cuyo registro la nombra.
- **La anterior** se guarda en `v56/historia/` con su registro, y §11.10 custodia `v56/`.
- **El traspaso** declara la segunda ejecución de `make_v55.py` (`a241bfbe` → `e597790f`) y lo que cambió en `ops-d.md` y `head.md`.

No hace falta nada más.

**M-05:** va al registro de la emisión (F4B-E60), según §1 y el registro de la sesión (:88). Hoy no se puede comprobar.

### Los literales nuevos, «Lo que cambia» y el resto

**PBT4E-C11 es PBT4D-C11 con exactamente los reemplazos declarados.**
- **El origen:** `make_c11_v56.py` lo saca del paquete en `160e019d`. Ese texto es igual al del contrato emitido de PB-T4d (`F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json`): lo comparé.
- **El fragmento** `v56/frag/c11.txt` es la línea del paquete.
- **El diff palabra a palabra** solo da:
  - la etiqueta, PBT4D → PBT4E (el cuerpo no tiene ningún `PBT4D`);
  - el «PB-T4d» final → «PB-T4e»;
  - «version 2» → «version 3», dos veces;
  - la ruta, el SHA-256, la decisión, la revisión y la propiedad 10 de la v3;
  - la cláusula de la satisfacción de PB-T4d.
- **En `160e019d`:** la v3 está en su ruta y da `e68b6e59…28c6` (la v2, `c033e96c…`). La revisión dice «Veredicto: APTO» (`independent-review-substitute-v3.md:9`).
- **La decisión del Owner,** literal, es «Sustituto v3 y revisión (Recomendado)», con «la v3 sustituye a la v2 de aquí en adelante, PB-T5 incluida» (`owner-pbt4d-nc2-decision.md:21` y `:27`).
- **La lectura del autor (desviación 8 del traspaso) es correcta.**
  - Lo de PB-T4d no fue una aceptación puntual: el Owner no eligió «Aceptar nc2 solo esta vez» (:18). Fue la satisfacción bajo un sustituto revisado.
  - Ponerla entre lo que no se extiende tiene la misma forma que la disposición de PB-T3a, que PBT4D-C11 ya llevaba.
  - Y es cierto: PB-T4e corre su propio nc2.
- **El texto** es ASCII.

**P-01, con la Snapshot L, solo en lo de PB-T4e.**
- Va literal en §6.5.1: `docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-L/snapshot-l.json`, que en `160e019d` da `69781991…5e8e`, con `ConfigSha256` `B71650A6…EBB3` y 105 nombres.
- La decisión del Owner está en `owner-config-snapshot-l-decision.md:21` y `:25`: la L sustituye a la K.
- **§6.0 sigue con la K** para PB-T4a a PB-T4d (:2189 y :2200), y el P-01 del contrato emitido de PB-T4d es la K.
- **El bloque de §6.5.1 sigue siendo ASCII:** 18 paradas (P-01 y las 17 propias) y 14 invariantes; con las 16 estándar de §6.0, 34.

**§6.5.2** lleva las autoridades nuevas con sus rutas en `160e019d`: la v3, su revisión APTO, la decisión del Owner y F4B-E58; la Snapshot L, su decisión y F4B-E57. Lleva también la v2 y la K entre las que ya no son autoridades. F4B-E57 y F4B-E58 casan con el mensaje del commit `160e019d`.

**«Lo que cambia» sale del diff de secciones.**
- Las 14 cambiadas y el anexo nuevo son exactamente las de `make_v56-out.txt`.
- **Las 109 sin cambio son ciertas** (mi diff de tres vías): 99 sin cambio desde la v5.4, que son PB-T4a a PB-T4d, y las 10 que nombra la cabecera, con el archivo de CT-PBT4E-03 en el anexo. El preámbulo tampoco cambia.
- **No hay ningún cambio sin declarar.**
- **Un detalle que no cuento como hallazgo:** el cambio del nombre de las copias (r71 → r82, r74 → r80 y r75 → r81) también aparece en D-PBT4-22, §5.7 y §7, que la columna «Dónde» de V56-04 no nombra. «Lo que cambia» sí nombra esas secciones.

**Los totales, los `RequiredTests` y los filtros no cambian.**
- La tabla de los 90 es idéntica fila a fila a la de la v5.5.
- `required-pbt4e-v56.tsv` es idéntico byte a byte al de la v5.5 (`a66fd8d4…`).
- Los filtros son `ui_filters_pbt4e_v55.txt` (`d769e5e3…`).
- **Los totales,** los de la v5.5: Core RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403; UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48.
- **La base** (r79): Core 19418 = 19015 + 0 + 403, igual que la CI `45057868915`; la unión de los filtros, 899 = 869 + 0 + 30.
- **En r83** corrieron los 38 procesos de UI.
- **`values-v56.json`** solo cambia, frente al de la v5.5, en lo declarado: las duraciones, la cobertura, el JSON y su registro, las 21 compilaciones, el control y las entradas nuevas.

**Las desviaciones del traspaso: ninguna afecta a la medida.** El traspaso tiene 8; el encargo dice «1 a 6» y llama «desviación 6» a la lectura de C11, que en el traspaso es la 8.

| N.º | Qué | Por qué no afecta |
|---|---|---|
| 1 | r77: el GREEN con `CS8604`, sobrescrito | El final se midió en r82 y r83. La cabecera de r76 lista el sha256 de aquel GREEN como pieza, pero r76 aplicó el de la v5.5 (`apply-green.txt`) |
| 2 | r78 sin UI | r83 la repite entera |
| 3 | Las tres ejecuciones de `post-p6` | Comprobado arriba (M-04) |
| 4 | El control no se repite | Se hizo con el GREEN final |
| 5 | Los ensayos en seco | No son versiones |
| 6 | `python -I -m py_compile` y dos archivos en `/tmp` | Están fuera de las reglas, pero se borraron y no tocan nada de lo medido |
| 7 | Los guiones reutilizados | La cabecera de r79 dice «(v5.5)» porque usa `p6_basecov_v55.sh` sin cambio: es cosmético |
| 8 | La lectura de C11 | Correcta (arriba) |

**Decisiones nuevas del Owner: ninguna.**
- D-S19 no cambia nada visible ni la lista; el sitio de la recarga es lo que el Owner dejó a la sesión.
- C11 y P-01 aplican decisiones del Owner ya tomadas: la v3 «de aquí en adelante» y la Snapshot L.
- M-06 y M-07 son de texto y de custodia.

### Lo no verificado

- **No compilé ni ejecuté nada.** Lo de las corridas lo tomo de sus registros, cuyo sha256 comprobé. La cobertura no la volví a calcular: de los JSON solo leí las dos líneas VARIA.
- **Los GREEN que no se conservan:** el primero de la v5.6 (`8e3e8077`, sobrescrito) y la v5.5 `a241bfbe`. Solo cuentan como historia.
- **Lo que aún no existe:** el número de `ForbiddenWriteScope`, que da el generador, y el registro de la emisión (M-05, M-06 y M-07).
- **La suite completa de UI y el Smoke-PB:** como en la comprobación de la v5.5.

> ; recomprobacion v5.6: EMITIBLE; B 0 / I 0 / M 2 (M-06: el D-S17 del registro de la sesion sigue describiendo el sitio de la v5.5, un else if antes de las comprobaciones, sin remitir a D-S19; M-07: la v5.6 cita CA-T4e con el sha256 27636ed5, el de antes de esta recomprobacion; los dos de texto o de custodia, sin decision del Owner); M-01 resuelto por D-S19 (la recarga tras las tres comprobaciones de la vista, con !begunHere && NeedsLazyReload; nada evalua la marca antes; marcado-sin-decidir da Failed con Report con el GREEN de la v5.5 en r76 y Refused sin fallos ni lecturas con el de la v5.6 en r82 y r83; las otras 24 lineas iguales); M-02 resuelto (los dos limites declarados en 4.5 y en el guion; la corrida opcional sin las clases nuevas, Core 19418 = 19015 + 0 + 403 y la union de 18 filtros 899 = 869 + 0 + 30 tres veces, PASA con los cuerpos vigilados intactos; las 2 VARIA de RackViewBatchDialogPresenter.cs:37-38 varian entre corridas del mismo codigo, la base r73 0/3 frente a la r79 2/3, y el filtro 04 las cubre siempre); M-03 corregido en el registro; M-04 resuelto (post-p6 nombra la 6b8939ae; frente a la 38efafdb solo cambia la linea 9, y en post-p6 solo las dos lineas de la version); M-05 al registro de la emision; PBT4E-C11 es PBT4D-C11 con los reemplazos declarados y la v3 e68b6e59 (APTO; decision del Owner Sustituto v3 y revision, de aqui en adelante); P-01 con la Snapshot L 69781991 solo en PB-T4e, y 6.0 sigue con la K; 109 secciones sin cambio = 99 + 10; RequiredTests 90 y su tsv identicos a la v5.5; precondicion 6 sobre 160e019d: base Core 19418 = 19015 + 0 + 403 igual a la CI 45057868915, RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403, UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48, 21 compilaciones sin warning CS, control campo y anfitrion correcto; GREEN y RED de la v5.6 reproducidos por objetos git con los mismos sha256
