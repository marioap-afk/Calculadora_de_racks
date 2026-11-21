# PB-T4b: decisiones de diseño de la sesión antes de la v4.4 (I-64 F4B)

**Fecha:** 2026-11-20.

**Contexto:** al preparar la v4.4 del paquete de PB-T4, el redactor midió sobre `5aaacce3`, con el GREEN literal de PB-T4a aplicado en memoria, que el diseño de PB-T4b de la v4.3 rompe dos cosas que no estaban previstas. Se detuvo antes de escribir, como se le había pedido para cualquier cosa que cambiara la lista o exigiera una decisión nueva. La sesión decide lo siguiente. No son decisiones del Owner: la lista de 470 pruebas sustituidas, que es lo que fija la condición de emisión del Owner, no cambia. Se declaran en la emisión de PB-T4b y se informa al Owner.

## D-S1. Las copias mecánicas que no pueden pasar en el GREEN pasan a equivalentes escritos a mano

**El hecho:** siete de las 375 «copias mecánicas» de la v4.3 (`pbt4-copy-map.tsv`) fijan el comportamiento que PB-T4b cambia a propósito, y no pueden pasar en el GREEN:
- `WorkspacePilotPrepareTests.TheNewOutcomesAreValuesWithTheStatesOfThePackageAndTheDefaultIsUnavailable` (:1319-1356) exige los 11 nombres de `EditIntentStatus`, y D-PBT4-08 añade `Presented`;
- seis llaman una vez a `TryKeepPilotEdit` sin testigo y esperan `Kept`, y con «sin testigo, presenta» reciben `Presented`:
  - en `WorkspacePilotPrepareTests`, :727-756, :1041-1055 y :1179-1200;
  - en `WorkspacePilotReconciliationHostTests`, :160-179, :389-418 y :536-557.

**La decisión:** esas siete, y las que encuentre la precondición 6 al ejecutar las copias sobre el GREEN literal, pasan a ser **equivalentes escritos a mano, con su registro**, en lugar de copias mecánicas. Cada equivalente conserva todo lo que el original protegía que no dependa del cambio, y afirma el comportamiento nuevo donde el original afirmaba el viejo.

**Por qué no vuelve al Owner:**
- la lista de pruebas sustituidas, con su `Skip`, sigue siendo la de 470 (`pbt4b-substitution-list.tsv`, `44393b19…`);
- cambia solo cómo se escribe el equivalente de unas pocas, de 375/53 a 368/60, o algo más con lo que mida la precondición 6, siempre dentro de las 470.

**Lo que la emisión tiene que hacer:**
- declararlo en F4B-E52;
- custodiar el mapa de la copia nuevo con su sha256;
- dar el reparto final medido;
- que los equivalentes a mano tengan su texto en el paquete, para que el Worker no tenga latitud.

## D-S2. «Conservar mi borrador»: el flujo compatible con las guardas del lector (dos «Sí»)

**El hecho:** el paso 1 de D-PBT4-08 hace que `RequestKeepMine` llame al puerto. Eso rompe dos guardas que no están en ninguna lista:
- `WorkspacePilotReconciliationGuardTests` :104 (`TheFourOperations…`);
- `WorkspacePilotReconciliationGuardTests` :128 (`TheGuardOfWhoReadsSeesAnOperationCaptured…`).

`ReaderViolations` (RG:69-100) exige que la ventana nombre `TryKeepMine` una sola vez, y solo como `_port.TryKeepMine(request)` dentro de `ConfirmChoice`. RG:132 muta esa línea literal.

**La decisión:** el flujo que deja esas guardas en verde.
1. «Conservar mi borrador» sigue preguntando desde la memoria, como hoy.
2. El primer «Sí» llama a `TryKeepMine` sin testigo. El anfitrión presenta las líneas, por grupos, encima del Sí/No, y devuelve el testigo.
3. El segundo «Sí» llama con el testigo vigente y conserva con `KeepOn`.
4. La línea literal de RG:132 no cambia, y la petición lleva `_keepWitness`.

**Por qué no vuelve al Owner:**
- la otra salida, el diseño de la v4.3 con las dos guardas añadidas a la lista, haría crecer la lista por encima de la que vio el Owner;
- este flujo cumple OB-05.4: es una confirmación explícita después de la presentación, y sin elección no se escribe;
- el número de clics no lo fija ninguna decisión del Owner;
- su nota sobre la interfaz deja los detalles de forma para después de la primera etapa.

Es un cambio visible y se declara en PBT4B-C03 y en F4B-E52.

**Rechazada:** la variante en la que el núcleo conserva también sin testigo. Contradice «`KeepOn` solo tras confirmar con el testigo vigente».

## D-S3. Lo que se corrige en el paquete, sin decisión

- **El alias de EIG:743:** se conserva `var pilot = read.PilotRead;` y se usa `pilot.PreviewWitness`, en las cuatro sentencias, incluida la de `TryDiscardAndReloadPilotEdit` (WHC:785).
- **El texto del testigo de `Presented`** sale de la función pura de `Draft/`, no de WHC, porque la guarda de la base prohíbe `.StaleBase`, `.InitialState` y `.Entries` fuera de los consumidores admitidos.
- **`FullPrepareKit`** conserva los tipos de lectura del piloto en `Reread`, `DraftOf` y `Prepare`, porque las copias le pasan `IntentKit.Read(…)` y un `PushBackPilotIntentResult?[]` (WPP:1471).

## D-S4. Una sola materialización del modelo completo por lectura

**De dónde sale:** de la decisión del Owner sobre el coste de la lectura de PB-T4a, «Cerrar y optimizar en PB-T4b» (`owner-pbt4a-cost-decision.md`), que la sesión desarrolla así. Su texto normativo está en el paquete, desde la v4.5: **D-PBT4-21**, V45-02 y V46-04, con CT-PBT4B-21. Aquí solo se resume para que las decisiones D-S1 a D-S6 queden en un mismo registro:
- RIA difiere la lectura completa del `Primary` en un `System.Lazy` sobre el mismo `input`, y `Full` es `_full?.Value`;
- D-S4 no toca NH, `AtOrigin`, `ReadAt` ni WHC (WHC sí cambia en el GREEN de PB-T4b, por D-PBT4-04 y D-PBT4-08, pero no por D-S4);
- RIA entra en el alcance como `UNIT_CHANGE`, con su lista cerrada de cambios;
- el piloto del `Primary` se sigue leyendo primero y solo se difiere el modelo completo (M-08 de la §19);
- la sonda del coste (`pbt4b-read-cost-probe.cs`) va en la precondición 6.

**Por qué no vuelve al Owner:** desarrolla su propia decisión, y no cambia la lista: la v4.5 midió que ninguna prueba cae por ella.

## D-S5. Las cuatro `Theory` que mezclan resultados en el RED se parten por filas

**Fecha:** 2026-11-20, después de la v4.6 (`e58df774…`).

**El hecho:** la precondición 6 de la v4.6, sobre una copia exportada de `78a7cef3`, para en el paso 4, la partición. De las 476 pruebas de copia, 249 pasan enteras en el RED, 223 fallan enteras y **cuatro `Theory` mezclan resultados** (30 filas: 18 fallan y 12 pasan):
- **WLHT** `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` (`InlineData`): 3 de 10 filas fallan en `Env.Pilot` (`Assert.NotNull`);
- **PEG** `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` (`InlineData`): falla 1 de 4 filas, la del adaptador estático, porque WHC del RED aún tiene el del piloto;
- **WPP** `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` y `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` (`MemberData`): en cada una fallan 7 de 8 filas, por un `NullReferenceException` en `Rig.Edit`, en el código de la prueba. Solo pasa `Clean`.

Todas las filas que fallan lo hacen por aserción según la regla de INV-PBT4B-02. Ninguna prueba fuera de la lista de 477 falla, y no hay avisos CS. La regla de la partición de la v4.5 (§5.2) dice que una prueba que mezcla resultados para la emisión, y que una `Theory` no se parte.

**La decisión:** cada una de esas cuatro se parte por filas, con el mecanismo de D-S1. Pasa a equivalentes escritos a mano, con su registro y su texto literal en el paquete:
- **un método en `…FullRedTests`** con las filas que fallan en el RED;
- **otro en `…FullGreenTests`** con las filas que pasan;
- **el mismo cuerpo** y las mismas aserciones del original;
- **en las dos de `MemberData`,** dos miembros de datos nuevos dentro de la copia, que reparten las filas del original sin cambiar ninguna.

La regla de la partición se amplía solo para eso: **una `Theory` que mezcla se parte por filas en dos equivalentes a mano, y la partición lo admite solo para las declaradas en el paquete**, con sus filas Red (`SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v4n.py`). La condición exacta es la del paquete (§5.2, la regla de R_C y R_UI). Para la emisión, en cambio:
- una que mezcla sin estar declarada (se le aplica D-S5 con una versión nueva del paquete);
- una declarada que no mezcla, o que mezcla otras filas;
- y una que no corre, se salta o falla sin aserción (C04).

**Por qué no vuelve al Owner:**
- la lista de pruebas sustituidas, con su `Skip`, sigue siendo la de 477 (`pbt4b-substitution-list-477.tsv`, `5975c5b6…`), la que vio el Owner;
- no cambia ninguna aserción, ninguna fila ni los totales de la CI; solo cómo se reparten las filas entre las dos clases de la partición, de 360/68 a 358/70 en C-0;
- es el mismo mecanismo que D-S1, que la sesión ya decidió y que la emisión declara.

**Lo que la emisión tiene que hacer:**
- declararlo en F4B-E52, junto a D-S1;
- que CT-20 y la partición admitan el reparto por filas, nombrando las cuatro;
- custodiar el mapa de la copia nuevo con su sha256 y dar el reparto final medido.

## D-S6. Las copias, en el espacio de nombres raíz con el prefijo `FullPanelCopy`

**Fecha:** 2026-11-20, después de la v4.7 (`b86ef9cb…`). El análisis medido está en `v47/analysis-outside-477.md`, punto 1.

**El hecho:** la precondición 6 de la v4.7 (pasos 5 y 6, con la suite completa de Core) hace caer `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace` en el RED y en el GREEN. La guarda no está en la lista. Las 20 copias declaran `RackCad.Tests.FullPanelCopies` (D-PBT4-11.3), y la guarda exige que cada `.cs` de los proyectos de pruebas declare solo el espacio raíz de su ensamblado. Ese espacio se eligió para que los filtros heredados n.º 12 y n.º 15 (`…Tests.Workspace` y `…UI.Tests.Workspace`, con `ExpectRed` false) no seleccionen las copias, no para evitar una colisión de tipos.

**La decisión:** las 20 copias van en el espacio de nombres raíz de su ensamblado, y sus clases llevan el prefijo `FullPanelCopy` (por ejemplo, `FullPanelCopyWorkspaceLifecycleHostRedTests`). Es el mismo mecanismo que el paquete ya usa para las CT. Medido en una copia exportada: con el espacio raíz, compila sin errores ni avisos y la guarda pasa. El prefijo evita que los filtros 12 y 15 seleccionen las copias, y ninguna clase del repositorio empieza hoy por `FullPanelCopy`.

**Por qué no vuelve al Owner:**
- la guarda queda en verde sin tocarla, y no se toca ninguna prueba existente ni ningún filtro heredado;
- la lista sigue en 477;
- cambian solo los nombres completos de las copias y sus filtros (las filas n.º 47 a 88), el `CopySha` y los guiones.

Es el precedente de D-S2: un diseño que deja en verde una guarda que no está en la lista.

**Rechazadas:**
- añadir la guarda a la lista, porque la debilitaría y su comentario lo pide expresamente;
- editarla (PBT4B-C02);
- cambiar los filtros heredados.

## D-S1, aplicada a dos copias de WPP

**El hecho:** `WorkspacePilotPrepareTests.ThePilotNeverErasesAViewAPhantom…` (:1996-2016) e `InvalidE_AMutableMember…` (:1746-1780) son copias mecánicas que fallan igual en el RED y en el GREEN. Fijan que el piloto nunca borra una vista, y el modelo completo borra la lateral obsoleta. El análisis está en `v47/analysis-outside-477.md`, punto 3.

**La decisión:** pasan a equivalentes escritos a mano (D-S1). Afirman lo que ya fijan **D-PB-04** («borrar vistas obsoletas, sí; crear vistas no está autorizado»), **D-PBT3-09 (e)** (`FrontalBOfOneWay` → `Invalid`; `ObsoleteLateral` → a `Erasures`) y **D-PBT4-07**:
- con el fantasma lateral, `Ready` con borrados [B2];
- `side-b`, `Invalid` con `FrontalBOfOneWayPrefix`/`Suffix`;
- los dos descriptores, como hoy.

No es una decisión nueva: es el comportamiento ya decidido y custodiado desde PB-T3. El reparto de C-0 queda en 356/72 o lo que mida la precondición 6.

## Lectura de la sesión: dos pruebas que marca el paseo de guardas corregido y que no entran en la lista

**Fecha:** 2026-11-20, al corregir el paseo para la v4.8 (V48-04). El paquete las declara en §4.2.

**El hecho:** con los campos de tipo tupla reconocidos, el paseo (`walk_guards_pbt4b_v48.py`) marca, fuera de la lista de 477, tres pruebas más:
- la de la sexta tanda del Owner (`owner-pbt4b-list-478-decision.md`);
- y las dos de abajo.

Las dos de abajo **pasan en las dos suites completas de Core del GREEN** (`/d/r4m` y `/d/r4o`) y en la precondición 6 de la v4.8.

**La lectura:**
- **`WorkspaceCommandAdvisorGuardTests.TheCatalogBodyDoesNotNameTheDocumentTypeAndNoSpanishReasonUsesAWordThatTheNeighbouringGuardsForbid`**, `WorkspaceCommandAdvisorGuardTests.cs:468-474`, comprobado por la sesión por objetos git en `78a7cef3`:
  - solo hace `DoesNotMatch` sobre `CatalogBody()` (sin `Document`) y sobre la columna `Spanish` de `ReasonTable`;
  - el `"Read"` que vio el paseo es el valor de la columna `Body` de la tupla `ReasonTable` (WCAG:408-419; las filas que cuenta el paseo son :415-419), el nombre de un cuerpo, no un literal que la guarda cuente en RIA.

  **Es un falso positivo del paseo.**
- **`WorkspaceHostHardeningGuardTests.TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier`** (WHHG:501-526): es la autoprueba de la guarda de los textos. Pasa en el GREEN, **pero en vacío** (CA-T4b, §20, I-01).
  - **Cómo trabaja:** llama a `LiteralViolations` sobre el anfitrión y el soporte reales, con la tabla del original (2, 5 y 1), y solo exige `NotEmpty` (14 veces).
  - **Por qué pasa en vacío:** en el GREEN, esas tres cuentas ya no casan, así que el texto sin mutar ya da violaciones. Pasa, pero ya no discrimina.
  - **La corrección de la v4.9:** como el equivalente de G-4, el de `NoneOfThe30…` (`WorkspaceHostHardeningGuardFull`) trae tras su `Assert.Empty` las 14 `NotEmpty` de la autoprueba, sobre su propio `LiteralViolations` y su tabla nueva. La lista no cambia.

  _Corrección de la sesión, 2026-11-21:_ la primera redacción decía «trabaja sobre su propia entrada y pasa en el GREEN», y no era cierto.

**Por qué no vuelve al Owner:** su regla (`owner-pbt4-decisions.md`, pregunta 4) es que una prueba **falle** fuera de la lista, y estas no fallan. Si la CI o una corrida posterior hiciera fallar alguna de las dos, volvería al Owner.
