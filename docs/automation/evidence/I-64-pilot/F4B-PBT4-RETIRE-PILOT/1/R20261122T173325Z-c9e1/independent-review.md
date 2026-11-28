# Revisión adversarial independiente del código — I-64 F4B PB-T4d (`F4B-PBT4-RETIRE-PILOT`), intento 1

**Objeto:** `git diff 399c9bec6d4778aa50096ab6204859c0bd015419 7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`. Son dos commits: el RED `aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a` y el GREEN `7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`. WorkRunId `R20261122T173325Z-c9e1`.

**Cómo se leyó:**
- El código y los documentos, solo por objetos git (`git show`, `diff`, `grep`, `ls-tree`, `rev-parse`) desde el checkout principal. No se leyó, listó ni ejecutó nada en el worktree de la rama.
- La entrega, por la copia de `rv/`.
- La CI, con `gh run view` (estado, jobs y log), solo de lectura.
- Todas las comprobaciones propias son guiones Python de solo lectura, escritos con la herramienta de archivos en una carpeta aparte de la entrega: las piezas contra los blobs, C02 línea a línea, las TRX, las sustitutas contra sus originales, y las dos CT reimplementadas sobre los árboles git.

---

## 1. Veredicto

**ACEPTABLE CON MENORES.**

**Recuento: B 0 / I 0 / M 3.** Además, **3 hallazgos OWNER**, informativos: ninguno pide una decisión antes de cerrar.

Resumen:
- El RED y el GREEN son, archivo a archivo, el texto literal de las piezas v5.4. Se comprobó reaplicando los generadores custodiados sobre BaseSha y comparando con los blobs.
- Las declaraciones quitadas de `src/` son exactamente las 57 de C13, y no se añade ninguna. `allowErase` no aparece en `src/`.
- Todo cambio de los 26 archivos existentes cae en una fila con acción de la lista.
- A nivel de resultados de prueba, solo desaparecen pruebas de la lista, solo aparecen las de las cinco clases nuevas y solo cambian de estado los tres `Skip`.
- Los totales de la CI son los exactos del contrato en las dos ramas, sin ningún `warning CS`.
- La retirada no cambia comportamiento: `BuildCore` sin `allowErase` es equivalente para su único llamador, `BuildFull`. La razón está en §5.

Los tres menores son de custodia, de exactitud de una afirmación de la entrega y de nombres o comentarios que quedan desfasados. Ninguno toca código de `src/` ni lo que fija el contrato.

---

## 2. Tabla de hallazgos

| Id | Sev. | Dónde | Qué | Por qué importa | Qué hacer |
|---|---|---|---|---|---|
| **M-01** | M | `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/pbt4d-precondition6.json` (`RedFiles`, WPUR) frente a `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs` @ `aa65ecfd` | En `RedSha`, WPUR tiene sha256 `7ea6170422f48334f18ff2cd3b1cb1b32c79264861dd52a6e6b56570daf7ba13`, que es lo que da `red_full` con `pbt4d-red-v54.txt`. `RedFiles` de la precondición 6 dice `62c6ff807daa62724ac1e09a0effb89a5ecd52ff988df523dd7e44bad92f5489`. Lo medí: es lo que da la pieza **v5.3** (`pbt4d-red-v53.txt`, `de0d3a0e…`). La diferencia v5.3 → v5.4 en WPUR son solo comentarios: 3 líneas de cabecera cambiadas y 6 de comentario o en blanco quitadas, las que iban sobre la n.º 48 y la n.º 49. Los otros 27 archivos coinciden con `RedFiles`. Es M-16 de la recomprobación de CA-T4d, que la v5.4 no corrigió en el JSON. Ni la entrega ni el registro del relevo lo declaran: el relevo dice «la forma» de `RedFiles`, no el sha256. | El brief y cualquier comprobación que compare el RED contra `RedFiles` darían un falso «no coincide» en 1 de 28 archivos. El contrato manda la pieza (INV-PBT4D-02; ExpectedEvidence: «iguales a los de la pieza del RED»), y contra la pieza el RED es exacto. | En la verificación y en el cierre, comparar WPUR contra la pieza v5.4 (`7ea61704…`), no contra `RedFiles`. Declarar M-16 como abierto en el registro de la cadena; si se corrige el JSON, que sea en una tarea de documentos. |
| **M-02** | M | `tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs:111-115` @ `7db6fd0c` (y `aa65ecfd`); `rv/worker-handoff.json`, `Evidence` («Tabla CT …») | En el RED, las dos autopruebas de archivo de CT-01 no discriminan: dan `pasa` y no `falla`. Inyectan un texto para un archivo que en el RED ya existe, así que `existe=True` no cambia. Las otras 12 autopruebas sí discriminan en el RED. En el GREEN discriminan las 14. Lo confirmé reimplementando la lectura sobre el árbol del RED (§8). La entrega dice que las autopruebas de las dos CT «pasan en RED y GREEN». Para esas dos líneas de CT-01 en el RED no es así, y la TRX no lo deja ver, porque `Assert.Equal` solo muestra la primera diferencia. | INV-PBT4D-02 dice que, en el RED, «las autopruebas de CT-PBT4D-01 y 02 fallan sobre el texto inyectado»; al pie de la letra, no se cumple para esas dos. No afecta al criterio del RED (CT-01 cae por aserción, Failed == Selected) ni al GREEN. Es el diseño literal de la clase del paquete: el Worker no podía cambiarlo. | Declararlo en la verificación como límite del diseño de la CT: las autopruebas de archivo solo discriminan cuando el archivo no existe, es decir, en el GREEN. Corregir la frase de la entrega. Sin cambio de código. |
| **M-03** | M | Tests @ `7db6fd0c`: `PushBackFullWritePrepareTests.cs:549` (`CT_PBT3B_06_…OfTheFullModelAndOfThePilot`), `PushBackFullWriteRunTests.cs:246` (`CT_PBT3B_09_…AndThePilotKeepsItsComparison`), `PushBackPilotUpdateEquivalenceGuardTests.cs:104-106` y `:183`, `PushBackRetirePilotRunTests.cs:79` (`AViewThatIsNotAvailable…`, que ahora prueba la frontal B, a mano y declarado), la clase `WorkspacePilotUpdateRunTests`. Fuera del alcance, en `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`: `:1783` (la etiqueta `"WorkspaceHostCore.PilotUpdateRun.Execute"`) y `:1240` y `:1268` («the adapter of the pilot», que es el campo `PilotFieldAdapter` de tipo `PushBackFullFieldAdapter`) | Quedan nombres de prueba y comentarios que describen como presente lo retirado. Dos ejemplos: el resumen de `E3Violations` dice «a view that is not available is never erased by the pilot … when the model admits them», y el de `E5Violations`, «the two runs that must exist, Execute (the pilot) and ExecuteFull». Es texto literal de la pieza del RED o de un archivo prohibido (C10). | Inducen a error a quien lea el nombre de la prueba o la guarda; no afectan a lo que se prueba. Renombrar cambia la identidad de la prueba y los filtros `FullyQualifiedName~`. | Una tarea propia de limpieza posterior, si el Owner la quiere (OWNER-03). Nada en PB-T4d. |

No encontré ningún bloqueante ni ningún importante.

---

## 3. Alcance y forma

| Comprobación | Resultado | Evidencia |
|---|---|---|
| El RED solo toca `tests/` | **Sí.** 31 rutas: 23 M, 5 A, 3 D; `git diff --name-only BaseSha RedSha -- src` vacío | `git diff --name-status 399c9bec aa65ecfd` |
| Cada archivo del RED, el texto de la pieza | **Sí, los 28.** Reaplicando `red_pbt4d_v53.red_full` (custodiado, `7a67652e…`) con `pbt4d-red-v54.txt` (`80f8cc0c…`) sobre BaseSha, el texto coincide con el blob de `RedSha` y con el de `CurrentSha`. El blob ya está en LF y sin BOM, así que coincide también en bytes. Contra `RedFiles` de la precondición 6, coinciden 27 de 28: WPUR no (M-01) | El guion de verificación, salida «RED mismatches» (solo `P6!=piece` en WPUR) |
| Los tres que borra, ausentes | **Sí,** en `RedSha` y en `CurrentSha`: `PushBackDeclaredCorrectionPilotParityTests.cs`, `PushBackFieldValidationConcordanceTests.cs` y `Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs`; iguales a `RedDeleted` | `git show` falla en las dos revisiones |
| El GREEN, literal y solo `Pilot/` | **Sí.** `RedSha..CurrentSha`: 4 M y 2 D, solo en `src/RackCad.Application/Systems/PushBack/Pilot/`; `tests/` sin cambio. `green_pbt4d_v53.green` con `pbt4d-green-v54.txt` (`aaf6cab8…`) da el mismo texto sobre BaseSha y sobre RedSha, igual al blob de `CurrentSha` y a `GreenFiles` (`a545dbff`, `7b7f2fb4`, `f70febaa`, `5c936519`). Los dos borrados están en `GreenDeleted` | Las mismas comprobaciones, para el GREEN |
| Las 37 rutas | `BaseSha..CurrentSha` son exactamente las 37 del AllowedWriteScope; nada fuera de `src/` y `tests/`. `src/RackCad.Domain`, `src/RackCad.UI` y `src/RackCad.Plugin` sin cambio | Ídem |
| C13: las declaraciones quitadas | **Exactamente las 57, y ninguna nueva.** Lo comprobé en el diff y lo crucé con el censo de la entrega (`inv/census-base.txt`, 247, contra `census-green.txt`, 190: solo líneas quitadas): comparador con `StructuralEquality` (1 + `Instance` + 16 métodos + el tipo anidado + `MaxDepth` + 2 sobrecargas de `Equal` = 22); adaptador (8); `PushBackPilotComposition` con sus 7 miembros, más `ComposeForWrite` y `KeepPilotEditsOn` (10); `PushBackPilotPrepareInput` con sus 9 (10); `Prepare`, `Build`, `ViewUnavailablePrefix` y `ViewUnavailableSuffix` (4); `Precheck(DraftSession<PushBackPilotDraftState>)`, `Execute` y `ComparePost` (3). Los dos archivos borrados no declaran otros tipos de primer nivel | `git show 399c9bec:…/PushBackPilotStateComparer.cs` y `…/PushBackPilotFieldAdapter.cs` |
| `allowErase` en `src/` | **No aparece** en `7db6fd0c`. En BaseSha estaba en `PushBackPilotUpdatePlan.cs:524`, `:590`, `:592`, `:598`, `:623` y `:629`, todo dentro del diff | `git grep allowErase` |
| Lo retirado, nombrado en `src/` | **Nada:** ni como código ni en comentarios, ninguno de los once nombres en `src/`. Solo quedan la etiqueta de texto de `WorkspaceHostCore.cs:1783` y el campo `PilotFieldAdapter`, de otro tipo; ninguno casa con las expresiones de CT-01 (M-03) | `git grep` sobre `7db6fd0c -- src` |

---

## 4. La lista, C02 y las tres clases nuevas que no son CT

### La lista

`pbt4d-retire-list-271.tsv` @ BaseSha:
- sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac`;
- 284 filas: 271 con acción y 13 que se quedan.

### C02, línea a línea

Un guion propio compara el diff de línea BaseSha → RedSha de los 23 archivos editados con los tramos `declaraciones-6c2820ba` y `tramo-editado` de cada fila. Antes comprobé que `tests/` de `6c2820ba` y de `399c9bec` son iguales.
- **Toda línea de código cambiada cae dentro de una fila con acción.** Lo que el alineador de difflib atribuía a otro miembro eran intercambios de líneas idénticas (`}`, el `[Fact(Skip …)]` heredado de PB-T4b). Se comprobó que, en cada archivo, el multiconjunto de esas líneas es el mismo que el de las líneas «todo» que figuraban sin cambiar.
- **Ninguna fila «se queda» cambia:** las 13 de `PushBackFullWritePrepareTests.cs`.
- **Ninguna fila «reescribir» o «acotar» cambia fuera de su `tramo-editado`.**
- **La única inserción fuera de un miembro** es el `using RackCad.Application.Systems.PushBack.Draft;` de `WorkspaceDraftPilotIntentStaleBaseTests.cs:7`. Es la fila 284, del grupo `directiva`.
- **Fuera de las filas solo cambian comentarios y líneas en blanco.** Entre ellos, la cabecera de WPUR, que es la edición de solo comentarios de V54-05 (a).
- **Los tres archivos borrados:** todas sus filas son «borrar con registro».

### C02 y C16, a nivel de resultados

Comparé las TRX por nombre en la misma máquina: la base local (`tests/b0/core-full.trx`) contra el RED local (`tests/r1/core-full.trx`).
- **Core, desaparecen** 237 resultados de 166 métodos, todos en las 11 clases de los archivos editados. Cada método tiene su fila «borrar» o «sustituir», salvo 8 `Theory` de Ownership que pierden filas de datos por la edición del ayudante `Kinds` (fila 280, «reescribir»).
- **Core, aparecen** solo 36: las cinco clases nuevas.
- **Core, cambian de estado** solo 3, de Passed a NotExecuted: los tres `Skip`.
- **De RED a GREEN** solo cambia CT-01, de Failed a Passed.
- **UI, en los 18 filtros:** desaparecen 21 resultados de 4 clases, todos de la lista; no aparece ni cambia nada.
- **UI, en la CI:** 2486 → 2464 cuadra con esos 21 más CT-PBT2B-10, que está fuera de los filtros, en el archivo borrado. No hay ninguna prueba que caiga fuera de la lista (C16).

### Los ayudantes borrados (CA-T4d M-03)

Los 32 de `UNUSED` y las 3 clases de `DELETE_FILES` son los que nombra el anexo V54-A. El RED y el GREEN compilan, con 0 errores en los 15 registros de build y la CI en verde. Por tanto, ninguno de los borrados seguía en uso por código.

No comprobé de forma exhaustiva que no queden ayudantes `internal` o `public` sin uso. IDE0051 solo ve los privados.

### Los tres `Skip` (D-S12)

Literales del anexo V54-A, cada uno en la línea de atributo de su prueba, y cada uno nombra su equivalente:
- `PushBackEditAllFullCopyGuardTests.cs:572`, que nombra `PushBackRetirePilotFullCopyGuardTests`;
- `WorkspacePilotPrepareGuardTests.cs:81` y `:196` (`:194` en la base), que nombran `PushBackRetirePilotPlanGuardTests`.

En la CI, las tres salen NotExecuted con ese motivo, en las dos ramas.

### Las tres clases nuevas que no son CT

**Byte a byte las de la pieza.** El sha256 del blob en `RedSha` y en `CurrentSha` coincide con `NewFileSha`:
- `PushBackRetirePilotRunTests.cs`: `3554df0d960f18ef6bb2b930f4ac7757e8ca7438ebf6a9eda8934ef9acf8fe69`;
- `PushBackRetirePilotFullCopyGuardTests.cs`: `ee9b6f1513f549065da391c10d6da947cc8e197644efbc7d72cf41e46443e6cf`;
- `PushBackRetirePilotPlanGuardTests.cs`: `48c2f4a9619e2f3e7e5de4f1ad98bf68024635c268866fa1242f65f329d8ca43`.

**Las 29 sustitutas** (29 métodos, 31 resultados; la n.º 42 es una `Theory` de 3):
- Cada una tiene su original en WPUR @ BaseSha, con el mismo nombre.
- Comparé las líneas de aserción normalizando solo el mapa de PB-T4b (`PilotUpdateSetup`/`FullUpdateSetup`, `Execute`/`ExecuteFull`, `ComparePost`/`ComparePostFull`, `PrepareKit`/`FullPrepareKit`). **28 son idénticas.**
- La que no lo es: la n.º 10, `AViewThatIsNotAvailable…` (`PushBackRetirePilotRunTests.cs:75-92`). Pasa de `ViewUnavailablePrefix + "B2"` a `FrontalBOfOneWayPrefix + "FB" + FrontalBOfOneWaySuffix`. Es el cambio a mano que declara §5.6, porque el modelo completo borra el lateral obsoleto en lugar de rechazarlo.
- Todas van sobre `ExecuteFull`, `PrepareFull` (`FullUpdateSetup.ExpectedPlan`), `ComparePostFull` y `PushBackPilotSystemRebuild`. **Ninguna usa lo retirado.**
- La n.º 48 y la n.º 49 prueban `PushBackPilotSystemRebuild.Build` y `BuildForMember` sobre el plan del PREPARE completo, con las mismas 5 y 22 aserciones que sus originales.

**`PushBackRetirePilotPlanGuardTests`:** cada guarda es su original con el cambio declarado y nada más:
- `PushBackFullPrepareInput` en lugar de `PushBackPilotPrepareInput`, con `From(DraftSession<PushBackFullDraftState>)`;
- `PrepareFull` y `BuildFull(` en lugar de `Prepare` y `Build(`.

**`PushBackRetirePilotFullCopyGuardTests`:** el equivalente de PB-T4c con la clase renombrada, una nota y los `BaseSha`, `CopySha` y recuentos nuevos. Las dos `CopySha` nuevas son los sha256 del RED de `WorkspacePilotEditGuardFull.cs` (`70d4f004…`) y `WorkspacePilotPrepareFull.cs` (`ca88ddf8…`).

**Pasan en las dos ramas:**
- en la CI del RED y del GREEN: 31/31, 1/1 y 2/2;
- en local: `r1c` y `g1c`, del core68 al core70.

---

## 5. El diseño frente al código (D-PBT4-10)

| Diseño | Código @ `7db6fd0c` | Estado |
|---|---|---|
| El comparador (con `StructuralEquality`) y el adaptador del campo, enteros | Los dos archivos se borran. Nadie fuera de `Pilot/` los usaba en BaseSha (`git grep` vacío) | OK |
| `ComposeForWrite`, `KeepPilotEditsOn` y `PushBackPilotComposition` | Fuera de `PushBackPilotMaterializer.cs`. Las constantes que se quedan siguen en uso: `InvalidScaleReason`, `BaseChangedReason` y `ComposeFailurePrefix` en `Draft/PushBackFullComposition.cs:69`, `:75` y `:82`; `NonPilotContentReason` en `PushBackPilotUpdatePlan.cs:396` | OK |
| `PushBackPilotPrepareInput`, `Prepare` y `Build` | Fuera de `PushBackPilotUpdatePlan.cs` | OK |
| El `Precheck`, el `Execute` y el `ComparePost` del piloto | Fuera de `PushBackPilotUpdateRun.cs`. El llamador del host (`WorkspaceHostCore.cs:919`) ya pasaba un `DraftSession<PushBackFullDraftState>` en BaseSha: la sobrecarga que resuelve es la misma | OK |
| `BuildCore` sin `allowErase` | Sin el parámetro (`:401`). Las cuatro condiciones quedan sin `allowErase`: `:473` `NoAddress`, `:475` `ObsoleteLateral`, `:481` `FrontalBOfOneWay` y `:509` `AllObsolete`. Sale la rama `disposition != Redraw` con `ViewUnavailable…`. El nombre es siempre el del borrador, o el del sobre de origen si el del borrador está en blanco (`:503`): sale `: embed.Name` | OK |
| `BuildFull` sin el argumento `true` | `:386-398`, sin `true` | OK |
| **Sin cambio de comportamiento en `BuildCore`** | `PushBackViewDispositionKind` tiene exactamente cuatro valores (`Draft/PushBackViewDisposition.cs:13`): `Redraw`, `ObsoleteLateral`, `FrontalBOfOneWay` y `NoAddress`. En BaseSha, con `allowErase = true` (el único llamador vivo), los tres no-`Redraw` ya volvían o seguían antes de la rama `!= Redraw`, que era inalcanzable. Las otras tres condiciones y el nombre dan lo mismo con `true` | **Equivalente** |
| Los cuatro `cref` y el comentario de D-S15 | `PushBackPilotDraftState.cs:10`; `PushBackPilotUpdatePlan.cs:17-18`, `:75` y `:100` (pasan a `PrepareFull`); el comentario huérfano se une al de `BuildFull` | OK |
| Los comentarios de CA-T4d M-05 | El resumen del materializador, el de la corrida (`UpdateRun:40-52`), el de `ExecuteFull`, el de `ComparePostFull`, el del planificador, el de `PrepareFull`, y los de `ErasedKeys`, `Erasures`, `Members` y `PushBackPilotMemberPlan`. Solo comentarios | OK |
| Lo que se queda, una vez | El lector, `Materialize`, `PushBackPilotDraftState`, las constantes, `PrepareFull`, `PushBackFullPrepareInput`, `BuildFull`, `BuildCore`, `ExecuteFull`, el `Precheck` completo, `ComparePostFull` y `ComparePostCore`. `ExecuteFull` y `ComparePostCore` solo cambian en comentarios. El lector lo sigue usando el modelo completo: `Draft/PushBackFullDraftReader.cs:56` y `:111`, `Draft/PushBackOriginByText.cs:41` y `:44`, y el Plugin en `Views/RackPilotIntentAdapter.cs:149`, que no cambia | OK (INV-PBT4D-03 e INV-PBT4D-04) |

En las 37 líneas añadidas a `src/`, solo 5 no son comentario: las cuatro condiciones y el nombre. Ninguna crea un objeto, lo copia ni lo muta.

---

## 6. El punto 4 (a): las desviaciones y los hallazgos inesperados de la entrega

- **`Deviations` está vacío, sin reparación del RED:** hay dos commits, sin «RED repair». **`TriggeredStopConditions` también está vacío.** Lo confirmo: no veo ninguna desviación.
- **`UnexpectedFindings`:**
  - la suite de Core tarda unos 12 minutos en la base y unos 5 en el RED y el GREEN;
  - los 3 archivos borrados no dejan referencias.

  Ninguno de los dos cambia lo que fija el contrato o el paquete. El primero es el efecto de D-S14: la prueba lenta pasa de 6,5 minutos a 7,6 s en la CI del RED y a 9,2 s en la del GREEN.
- **Afirmaciones de la entrega que matizo:**
  - **M-02:** las autopruebas de CT-01 no «pasan» en el RED en sus dos líneas de archivo.
  - **La prueba de limpieza:** «antes y después idénticas». El contenido es idéntico (mismo sha256 y misma fecha del configurador; mismo listado de la biblioteca, 11 entradas), aunque los dos archivos difieren en los finales de línea.
  - **`C02PorArchivo` del relevo** cuenta las 29 de WPUR como «Cambiadas» (138 borradas, 45 cambiadas), porque las casa por nombre con su sustituta. En WPUR se borran: queda una sola prueba (`WorkspacePilotUpdateRunTests.cs:25-48` @ `aa65ecfd`). Es una etiqueta del informe y no un defecto: sus filas dicen «sustituir con registro», que en WPUR es baja con equivalente.

---

## 7. D2 y su límite

**¿Alguna copia comparte un objeto mutable fuera de las dos referencias declaradas de cada lado?** No.
- El GREEN no añade ninguna copia: solo borra la del piloto (`PushBackPilotStateComparer.Copy` y su uso en `PushBackPilotPrepareInput.From`).
- La copia que se queda, la del modelo completo (`PushBackFullPrepareInput.From`), no cambia: solo cambia su comentario.
- El RED no toca `src/`.

**¿Alguna línea añadida muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?** No.
- Las 5 líneas de código añadidas son condiciones y una asignación de cadena; ninguna nombra esos tipos.
- Las sustitutas solo editan el estado editable de sus borradores de prueba, nunca el rack resuelto. Por ejemplo, `state.Editable.Editor.SetSideBPresent(false)` en `PushBackRetirePilotRunTests.cs:83`.

**PBT4D-C17:** no se da.

---

## 8. Las CT

**Un `[Fact]` por CT, sin `Theory` nueva.** Las dos clases están en `tests/RackCad.Tests/`, con `namespace RackCad.Tests` y `#nullable enable`, y terminan en un `Assert.Equal` de las líneas unidas por `\n`, con la regla «EXCEPCION» (`Paso`).

### CT-PBT4D-01 (`PushBackRetirePilotGuardTests.cs`)

**Fija lo que dice §5.6, línea a línea:**
- las 35 líneas esperadas (`:127-164`) son, en orden y literales, las de la tabla de §5.6;
- las expresiones (`:31-72`) son las que describe §5.6: `allowErase` como palabra y `\bViewUnavailable(?:Prefix|Suffix)\b`;
- `Uses` busca los siete nombres y las dos llamadas, y no casa `ComparePostFull` ni `ComparePostCore`;
- un archivo que no existe es texto vacío.

**Reimplementé la lectura sobre los árboles git** (`CodeOnly`, `AllProduct` y los recuentos):
- **sobre `7db6fd0c`,** el log son, exactamente, las 35 líneas esperadas;
- **sobre `aa65ecfd`,** difieren 16 líneas:
  - los dos archivos existen;
  - las once declaraciones están: `ComparePost` cuenta 2 y `allowErase` 6;
  - `src-nombra-lo-retirado` da los cinco archivos de `Pilot/`, como dice §5.6;
  - y las dos autopruebas de archivo dan `pasa` (M-02).

  La primera diferencia es la de la TRX: la posición 54, `existe=True`.

**Las autopruebas discriminan:**
- en el GREEN, las 14;
- en el RED, 12 de 14 (M-02).

**Una fragilidad menor del diseño, sin severidad:** `Prepare` y `Build` se buscan con su visibilidad (`public static` y `private static`). Un `Build` reintroducido como `internal` no lo vería ni la línea `retirado` ni `Uses`, que solo busca la llamada a `Prepare(`. La compilación del GREEN y la guarda E-3 no lo cubren tampoco. Es texto literal del paquete y no afecta a esta entrega.

### CT-PBT4D-02 (`PushBackRetirePilotCensusGuardTests.cs`)

**Fija lo que dice §5.6:**
- las 11 líneas (`:140-153`) son las de §5.6;
- `Mask` quita los comentarios y los literales regulares, verbatim, interpolados y crudos;
- los nombres son los siete, las dos llamadas y las dos constantes.

**La reimplementación** (`Mask` y la búsqueda sobre los `.cs` de los dos proyectos):
- **sobre `7db6fd0c` y sobre `aa65ecfd`,** el log son exactamente las 11 líneas esperadas;
- **sobre `399c9bec`,** difiere, porque `tests/` todavía nombra lo retirado, como en el control de la precondición 6.

**Las autopruebas** inyectan cada nombre como código en `WorkspacePilotPrepareFull.cs` y discriminan en las dos ramas.

**La dirección contraria la cubre el dato real:** en `tests/` hay 71 líneas que nombran lo retirado en cadenas y comentarios, en 16 archivos (`git grep` @ `7db6fd0c`). Si `Mask` no las quitara, `nombran-lo-retirado` no estaría vacío.

En la práctica, CT-02 es redundante con la compilación del GREEN, como dice su propia cabecera.

---

## 9. El RED y el GREEN (las TRX de `rv/` y la CI)

| Comprobación | RED | GREEN |
|---|---|---|
| Run de la CI (`gh run view`) | `45054587789` sobre `aa65ecfd`: `tests` failure, `ui-tests` success, `build-ui` success, `build-plugin` skipped | `45057868915` sobre `7db6fd0c`: los 4 jobs en success, con el Plugin compilado |
| Core (`core.trx` de la CI) | 19 418 = 19 014 + 1 + 403 | 19 418 = 19 015 + 0 + 403 |
| UI (`ui.trx` de la CI) | 2 464 = 2 416 + 0 + 48 | 2 464 = 2 416 + 0 + 48 |
| El filtro con `ExpectRed` | CT-01, Failed 1 == Selected 1, por `Assert.Equal() Failure: Strings differ` («existe=False» esperado, «existe=True» real). La primera línea del StackTrace es `tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs:line 165`, en la CI y en local (`tests/r1c/core66.trx`) | Passed, y tres veces seguidas en local (`g1r`, `g2r` y `g3r`) |
| Otros fallos | Ninguno fuera de CT-01: ni de la lista (C02), ni de las tres clases nuevas (C07), ni existentes (C16) | Ninguno |
| Las 7 filas `Skipped` de la base | Siguen, una vez cada una: E-1 a E-6 de D-1 (PB-T3b) y CT-PBT3A-12 (sustituida por CT-PBT4A-06) | Ídem |
| Los 3 `Skip` nuevos | NotExecuted con su motivo | Ídem |
| `warning CS` | 0, en los 15 registros de build locales y en los dos logs de la CI | 0 |
| Cuelgue | No. Core 3 min 6 s; la prueba acotada, 7,6 s | No. Core 2 min 56 s; la prueba acotada, 9,2 s |

**La CI de BaseSha:** `45043090710` sobre `399c9bec` falló. El log dice «The specified inactivity time of 5 minutes has elapsed», y la prueba en curso era `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_…`, la que acota D-S14, con Core abortado en 19 548. Así lo confirma la sesión. El RED ya lleva la cota (`WorkspacePilotPrepareFull.cs:1927-1931` @ `aa65ecfd`), y ni la CI del RED ni la del GREEN muestran el cuelgue. Lo que fija la prueba acotada se queda:
- `Assert.All` Invalid, sin plan y con motivo;
- el fallo del constructor (`SystemFailurePrefix`) y `catalog-boom`, entre los 52 PREPARE que recorre.

**La cobertura (INV-PBT4D-06):** no la medí. Pedía ejecutar con coverlet, y el brief lo prohíbe. La precondición 6 dice:
- veredicto **PASA**;
- 0 líneas sin ejecutar y 0 líneas con ramas perdidas en el GREEN, sobre 69 842 líneas y 28 818 ramas que se quedan;
- 537 líneas vigiladas de WPUR, de las que 433 se quedan, con 0 sin cubrir;
- «Bajan», 16 801, todas ESTABLE y ninguna PERDIDA.

Es aplicable a esta entrega porque lo comprobado coincide con lo medido:
- el `src/` del GREEN es byte a byte `GreenFiles`;
- el `tests/` del RED es la pieza v5.4, cuyo código sin comentarios es el de la v5.3 (`Reapply.TestsCodeChanged` = `[]`, lo que confirmo con el diff de WPUR de M-01);
- BaseSha tiene el `src/` y los `tests/` de `6c2820ba`.

---

## 10. Contención

- **Ningún cambio observable** del panel, la ventana, RACKEDITAR, el Plugin ni la escritura del dibujo (C03):
  - `src/RackCad.UI` y `src/RackCad.Plugin` no cambian;
  - en `src/RackCad.Application` solo cambian los seis de `Pilot/`;
  - lo borrado no tenía ningún llamador fuera de `Pilot/`;
  - el único camino vivo que cambia de texto, `BuildCore`, es equivalente (§5).
- **Ninguna escritura en el DWG:** AutoCAD no se ejecutó (`KnownLimitations`).
- **Ninguna escritura en el perfil:** la sonda de antes y de después es igual en contenido.
- **En disco**, solo los artefactos de la corrida. El Plugin se compiló con `-o` dentro de `artifacts/`.
- **La UI, solo con los filtros del contrato.** De los 209 `TestsExecuted`, las 54 corridas de UI son los 18 filtros de `RequiredTests` en la base, en el RED y en el GREEN, uno a uno. No se corrió la suite completa de UI en local. Las únicas corridas sin filtro son las tres de la suite completa de Core.
- **El `TreeSha` de cada fase** coincide con el árbol real de su commit (`9c3e4d41`, `7d934138` y `9dbb148b`).
- **El trailer** de los dos commits es `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`, el de la celda del Worker.

---

## 11. Hallazgos para el Owner

- **¿Alguna decisión que el contrato no le dejaba al Worker?** No. Aplicó las dos piezas literales con sus herramientas, sin desviaciones, sin reparación del RED y sin tocar nada fuera del alcance.
- **OWNER-01 (informativo):** la CI del commit de emisión (`399c9bec`, run `45043090710`) está en rojo por el cuelgue de la prueba lenta que PB-T4d acota. Conviene que el cierre lo registre como causa conocida, resuelta en esta cadena; no pide decisión.
- **OWNER-02 (Smoke-PB, informativo):** el paquete no lista en §5.5 los caminos de `BuildCore` cuyo texto cambia. La equivalencia es segura por construcción, porque el enumerado tiene 4 valores, y Core cubre esos caminos: CT-PBT3B-02, 04 y 05, y la sustituta n.º 10. Aun así, una pasada de «Editar todo» → «Actualizar» en AutoCAD los vería en el dibujo real, y puede sumarse a la del borrado físico que ya está prevista:
  - un plan que borra un lateral obsoleto;
  - un borrador con el nombre en blanco, que escribe el del sobre de origen en todas las vistas;
  - el rechazo de la frontal B de un rack sin lado B.
- **OWNER-03 (informativo):** los nombres y comentarios desfasados de M-03 piden, si se quiere, una limpieza en una tarea propia. Renombrar pruebas cambia su identidad y los filtros.

---

## 12. Lo no verificado

- **La cobertura línea a línea y rama a rama (INV-PBT4D-06):** pide ejecutar con coverlet. Me remito a la precondición 6 (§9).
- **Que no queden en `tests/` ayudantes `internal` o `public` sin uso:** IDE0051 y la compilación no lo ven. Solo comprobé que lo borrado no seguía en uso.
- **Las corridas locales:** no las repetí, porque el brief prohíbe ejecutar dotnet. Las juzgo por sus TRX y logs de `rv/`.
- **`inputs/red-files.txt` (`4065207e`):** la entrega lo cita, pero no está en `rv/`. Sus valores sí aparecen en `gen/apply-red.txt`, y los comprobé contra los blobs.
- **La suite completa de UI de la línea base:** la CI `44959653142` no la descargué. Cuadré el total de UI por diferencia con los filtros y con el archivo borrado fuera de ellos.
- **El comportamiento en AutoCAD:** queda para el Smoke-PB.
- **Dos incumplimientos míos de las reglas del brief, sin efecto:** usé dos veces `python -c`, para ver la versión y para recortar una salida de texto, y una vez un heredoc vacío (`print`). Todo fue de solo lectura y en la carpeta de trabajo de la revisión, fuera del repositorio y del worktree. Las comprobaciones que sostienen el informe son guiones escritos con la herramienta de archivos y lanzados con `-I`.
