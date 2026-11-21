# PB-T4b: lo que cae en la precondición 6 de la v4.7 — análisis medido

**Autor:** el redactor del paquete. La sesión copia aquí su informe tal cual, porque el entorno no le dejó escribir el archivo. Paquete v4.7 `b86ef9cbd29eea5fa1629af37db105dff8ef2feaa3a6999b2504ec49751d40c9`, sin cambios; sin `pbt4b-precondition6.json`.

**Las medidas, en copias nuevas:**
- `/d/r4n` no midió nada: la reescritura solo cubría la forma de archivo del `namespace`, se quedó en 15 de 20 y paró antes de compilar.
- `/d/r4o`: las 20 copias en el espacio de nombres raíz. Compilación, suite completa de Core y los 15 filtros de UI, en el RED y en el GREEN.
- `/d/r4p`: una sonda temporal del PREPARE del piloto frente al del modelo completo.

**Comprobación de la sesión** (por objetos git, en `78a7cef3`): WHC tiene los tres textos 2, 5 y 1 veces, y `WorkspacePilotSupport.cs` ninguna. La tabla de la guarda los exige en `WorkspaceHostHardeningGuardTests.cs:434` (2, `// before 1`), :438 (5, `// before 1`) y :444 (1).

| Punto | Qué cae | Recomendación | Lista | ¿Owner? |
|---|---|---|---|---|
| 1 | `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace` (RED y GREEN) | **1a:** las copias en el espacio de nombres raíz, con las clases `FullPanelCopy<Raíz>{Probe,Red,Green}Tests` | 477 | no |
| 2 | `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglish…` (GREEN) | **2a:** añadirla a la lista (478, en C-5) con un equivalente que lleve las cuentas 0, 6 y 0 | **478** | **sí** |
| 3 | `ThePilotNeverErasesAViewAPhantom…` e `InvalidE_AMutableMember…` (dentro de las 477) | D-S1, con equivalentes según D-PBT3-09 (e); mapa 356/72 | 477 | no |

## 1. `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`

**Por qué D-PBT4-11.3 fija `…FullPanelCopies`:**
- **El texto** (v4.7:833): «en el espacio de nombres `RackCad.Tests.FullPanelCopies` […]; cada tipo de primer nivel `T` del archivo, salvo la clase de prueba y los del kit, pasa a `T` + `Full` (M-00)».
- **El motivo**, de cuando se eligió (`v42/f-s5.md:83`): «para que el filtro heredado `RackCad.Tests.Workspace` (que el RED tiene que pasar, INV-PBT3A-02) no las seleccione». La nota de §6.2.1 lo repite (v4.7:2029).
- **No evita ninguna colisión de tipos.** M-00 ya renombra cada tipo de primer nivel, las clases de prueba son nuevas, y cada copia nombra `FullPanelCopies` una sola vez, en su línea `namespace`.
- **Lo que evita es una colisión de filtros:** que el n.º 12 (`FullyQualifiedName~RackCad.Tests.Workspace`) y el n.º 15 (`FullyQualifiedName~RackCad.UI.Tests.Workspace`) seleccionen las copias. Los dos son heredados y tienen `ExpectRed` false.

**Medido en `/d/r4o`, con el espacio raíz y los nombres de clase de hoy:**
- **La reescritura:** cambian solo las 20 líneas `namespace`. Son 15 de archivo y 5 de bloque: las tres de `tests/RackCad.Tests/` y las dos de la UI.
- **Compila** en Core y en UI: 0 errores y 0 `warning CS`. No colisiona ningún tipo, ni `using static`, ni métodos de extensión.
- **El RED, Core completo:** 19 586 = 18 837 + 292 + 457. Las 292 son exactamente las Red de la partición, y no falla nada fuera de las copias: **la guarda pasa.** En la UI, 0 Failed.
- **El GREEN, Core completo:** 19 586 = 19 126 + 3 + 457. Las tres son la guarda del punto 2 y las dos copias del punto 3. La guarda de espacios de nombres pasa. En la UI, 0 Failed.
- **Pero choca con los dos filtros heredados:**

| Filtro (`ExpectRed` false) | RED de hoy (`/d/r4m`) | Espacio raíz con los nombres de hoy (`/d/r4o`) |
|---|---|---|
| n.º 12 `RackCad.Tests.Workspace` | 3 314 filas, 0 Failed, ninguna de las copias | 3 924 filas, **289 Failed** (610 de copias) |
| n.º 15 `RackCad.UI.Tests.Workspace` | 672 filas, 0 Failed | 708 filas, 0 Failed (entran las copias de UI) |

Con los nombres de hoy, el filtro 12 fallaría en el RED.

**La forma limpia (1a): espacio raíz y el prefijo `FullPanelCopy` en el nombre de la clase.**
- **Por texto,** sobre los 46 filtros de §6.2.1 que no son de las copias, ninguno las selecciona. Hoy tampoco ninguno; con el espacio raíz y los nombres de hoy, dos.
- **Ninguna clase** del repositorio empieza hoy por `FullPanelCopy`.
- **Es el mismo mecanismo que el paquete ya usa para las CT** (v4.7:1259): «con `namespace RackCad.Tests` […]. Ninguna clase empieza por `Workspace` salvo `WorkspaceFullCopyGuardTests`».

**Lo que cambia con 1a:**
- **Los nombres completos de las pruebas:** `RackCad.Tests.FullPanelCopies.WorkspaceLifecycleHostFullRedTests.<m>` pasa a `RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostRedTests.<m>`. Los métodos, los archivos y los tipos `TFull` no cambian.
- **Los filtros:**
  - las 38 filas de las copias (n.º 47 a 84) y los filtros del modo de medida, que pasan a `…FullPanelCopy`;
  - los heredados no cambian, ni su `MinSelected`.
- **CT-20:**
  - M-00 y su punto (3) nombran la clase nueva;
  - cambia el `CopySha`;
  - sus líneas esperadas no cambian.
- **Los guiones:** el generador y las expresiones `CLASS` de la partición y de `p6_check`.
- **Coste:** una versión y la precondición 6 otra vez. La lista no cambia y no se toca ninguna prueba existente.
- **Riesgo, bajo.** El espacio raíz está medido; el prefijo en sí no, pero solo renombra 40 clases.

**Las otras opciones:**
- **1b, añadir la guarda a la lista (478):** necesita al Owner. Además debilita la guarda, cuyo comentario (`NamespaceFolderGuardTests.cs:41-51`) dice: «la excepción no es una exención: esta prueba la vigila» y «`FullyQualifiedName~` es la interfaz operativa de verificación del repo».
- **1c, editar la guarda:** prohibido (PBT4B-C02).
- **1d, cambiar los filtros 12 y 15** para que excluyan las copias: toca filtros heredados, justo lo que el comentario de la guarda pide no hacer.

## 2. `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglish…`

**Qué protege** (`WorkspaceHostHardeningGuardTests.cs:416-420`): «H-07: the 30 literals of the port of the pilot are in Spanish». Su tabla lleva, por cada texto, el literal en inglés, su texto en español y cuántas veces está en el anfitrión y su soporte: «the count of each one is the EXACT number of its occurrences now (the value before the task is in the comment of each row that changed)».
- **Qué cuenta** (`LiteralViolations`, :470-489): en WHC y en `WorkspacePilotSupport.cs`, sin comentarios, que el inglés no vuelva, que el español esté exactamente N veces y que no lleve subcadenas prohibidas.
- **Por qué exacto:** para que todo cambio de los textos del anfitrión sea deliberado. Las tareas anteriores actualizaron su fila en la misma tarea, con un comentario `// before N` (:434 y :438).

**Los tres textos:**

| Texto | Exige | Base `78a7cef3` | GREEN | Por qué |
|---|---|---|---|---|
| «La relectura no trajo resultado.» | 2 | WHC:426 (`CheckNow`) y :611 (`ConfirmBase`): `if (pilot == null) …Failed("…")` | **0**: WHC:433-434 y :620-621, `if (full == null) …Failed(FullReadMissingReason)` (`green_pbt4b.py:528-532` y :564-572) | **Desaparece un camino del piloto:** esas dos relecturas ya no leen el piloto. Lo fija D-PBT4-04 (v4.7:618): «las cinco relecturas responden `Failed` con el mismo texto». También INV-PBT4B-04: `var pilot` solo en cuatro sentencias |
| «El borrador se está aplicando o ya no está registrado.» | 5 | WHC:539, :676, :715, :764 y :801 | **6**: la nueva está en WHC:718 (`green_pbt4b.py:597`) | **Se duplica a propósito:** la rama de «Conservar» que presenta. D-PBT4-08, paso 2 (v4.7:682): «`Waiting` con el texto de hoy» |
| «El rack no tiene escala de anotación.» | 1 | WHC:1783: `if (annotations == null) reason = …` | **0**: WHC:1808, `Show(draft.CurrentDraftState.Editable.AnnotationScale, …)` (`green_pbt4b.py:649-651`) | **Desaparece el camino:** en el modelo completo la escala es `double`, no anulable (`PushBackEditableState.cs:66`), como dice D-PBT4-04. **El texto no se mueve:** no está en ningún archivo de `src/` del GREEN |

**¿Hay un GREEN que mantenga las cuentas sin artificio? No:**
- **El texto 3** pediría una rama `null` sobre un `double`, es decir, código muerto.
- **El texto 2** pediría otro texto para la misma situación, contra D-PBT4-08. Quitar esa comprobación tampoco sirve: la rama que presenta no pasa por `KeepMineOnCurrentVersion`, y presentaría sobre un borrador que se está aplicando.
- **El texto 1** solo se mantiene cambiando D-PBT4-04, y arregla una de las tres filas.

**Las opciones:**
- **2a (recomendada):** añadirla a la lista (478, en C-5) con un `Skip` y un equivalente `WorkspaceHostHardeningGuardFull`. Su tabla sería igual salvo tres cuentas (2 → 0, 5 → 6 y 1 → 0), con su `// before`.
  - En la partición sería Red.
  - Su hermana `TheGuardOfTheLiteralsTrips…` pasa en el GREEN.
- **2b:** cambiar D-PBT4-04 por el texto 1. No evita la decisión del Owner por los textos 2 y 3.
- **2c:** código muerto o textos distintos solo por la guarda. Rechazada.

**La pregunta para el Owner:**

> El GREEN de PB-T4b cambia tres cuentas exactas de la guarda de los textos del anfitrión, por D-PBT4-04 y D-PBT4-08: de 2, 5 y 1 en la base a 0, 6 y 0. La guarda no está en la lista. Opciones:
> - **(A)** añadirla a la lista (478) con un equivalente que lleve las cuentas nuevas (recomendado);
> - **(B)** mantener las cuentas de la base, con código muerto y textos cambiados solo para la guarda;
> - **(C)** parar PB-T4b.

**Por qué no la marcó el paseo de guardas de la v4.5:** su expresión `FIELD` (`walk_guards_pbt4b_v45.py:91`) no reconoce el tipo tupla del campo `Table`, `(string English, string Spanish, int Times)[]` (:421). Por eso sus 30 textos nunca entraron en la comparación, y la prueba salió con «-».

## 3. Las dos copias de WPP

**Qué afirma cada original:**
- **`InvalidE_AMutableMember…`** (`WorkspacePilotPrepareTests.cs:1745-1779`) tiene cuatro casos con B2:
  - `phantom-lateral`, en :1752-1753, con el comentario «a phantom that RACKEDITAR would erase and the pilot never does»;
  - `side-b`;
  - `corrupt-descriptor`;
  - `section-out-of-range`.

  En todos exige `Invalid` y `Plan` null, con el motivo `ViewUnavailablePrefix` o, en los dos descriptores, `ViewAddressPrefix`.
- **`ThePilotNeverErasesAViewAPhantom…`** (:1996-2016): con el fantasma, `Invalid` y `Plan` null. Sin él, `Ready` con A1 y B2.

**Qué hace el modelo completo:**
- **El camino:** `FullPrepareKit.Prepare` (kit :176-177) llama a `PrepareFull` (`PushBackPilotUpdatePlan.cs:445`), que pasa por `BuildFull` (`allowErase: true`, :488-505) y por el paso (e) de `BuildCore` (:578-600).
- **El GREEN no toca ese archivo:** las dos copias fallan igual en el RED y en el GREEN.
- **Medido en `/d/r4p`:**

| Caso | Piloto | Completo |
|---|---|---|
| `phantom-lateral` | `Invalid`, «El piloto no borra vistas…» | **`Ready`, miembros A1, borrados B2** |
| `side-b` | `Invalid`, «El piloto no borra vistas…» | **`Invalid`, «La vista B2 es un frontal del lado B en un rack sin lado B…»** |
| los dos descriptores | `Invalid`, `ViewAddress` | igual |
| lateral existente | `Ready`, A1 y B2 | igual |

**¿Lo fija ya alguna decisión? Sí:**
- **D-PB-04** (`f4b-dispositions.md:13`): «borrar vistas obsoletas, sí; crear vistas no está autorizado».
- **OB-04.1** (`package-v2.md:236`).
- **El paquete de PB-T3:** :357 y :367, y **D-PBT3-09 (e)** (:765-768): «completo: `FrontalBOfOneWay` → `Invalid(…)`; `ObsoleteLateral` → a `Erasures`».
- **Sus CT:** `CT_PBT3B_02` y `CT_PBT3B_04` (`PushBackFullWritePrepareTests.cs:295` y :384).
- **D-PBT4-07** (v4.7:670): «Desde el cierre de PB-T3c, el escritor borra vistas».

**Los equivalentes (D-S1, sin el Owner):**
- **`ThePilotNeverErases…`:** con el fantasma, `Ready` con miembros [A1] y borrados [B2]; sin él, igual que hoy.
- **`InvalidE_…`:**
  - `side-b` pasa a `Invalid` con `FrontalBOfOneWayPrefix`/`Suffix`;
  - los dos descriptores, como hoy;
  - `phantom-lateral` sale del bucle de los `Invalid`: `Ready` con borrados [B2].

**Efecto:**
- **el mapa** pasa de 358/70 a 356/72;
- **la partición:** pasarían también en el RED (la sonda corrió sobre el RED), así que irían a las clases Green y **R_C bajaría de 292 a 290**. Lo mide la precondición 6.

## 4. ¿Puede caer algo más?

- **Core: la lista es completa.** Los pasos 5 y 6 corrieron la suite entera, 19 586 filas cada uno. Con el espacio raíz (`/d/r4o`), el RED tiene 0 fallos fuera de las copias, y el GREEN solo los puntos 2 y 3.
- **UI: solo está medido lo que cubren los 15 filtros** (913 filas, 0 Failed). Por texto, fuera de esos filtros, no veo otra causa:
  - `UiSystemBoundaryGuardTests`: el panel conserva su `x:Class` y sus URIs;
  - `SelectiveWindowConstructionGuardTests`: las copias de UI no nombran la ventana del selectivo;
  - las ventanas que usan `NumericField`: las cubren los filtros n.º 45 y n.º 37, que pasan.

  El resto solo lo vería la CI.
- **Las CT** no estaban en la precondición 6. Por texto:
  - van en el espacio raíz (v4.7:1259), así que cumplen la guarda del punto 1;
  - la guarda del punto 2 no las cuenta;
  - solo CT-20 entra en el filtro 12, como prevé el paquete.

## Los archivos de las medidas

En `pbt4bcc/compile/`:
- `p6_diag_root_ns.sh` y `diag_root_ns.py`: el diagnóstico del espacio raíz;
- `p6_diag_phantom.sh` y `TmpPhantomProbe.cs`: la sonda del PREPARE;
- `filter_overlap_p6.py`, `filter_select_p6.py` y `count_literals_p6.py`: las cuentas de filtros y de textos;
- `out-run-diag-rootns-r4n.txt`, `out-run-diag-rootns-r4o.txt` y `out-run-diag-phantom-r4p.txt`: las corridas;
- `out-diag-rootns-r4o/`, `out-diag-phantom-r4p/`, `out-diag-analysis/` y `out-redgreen-v47-r4m-extra/`: sus salidas.
