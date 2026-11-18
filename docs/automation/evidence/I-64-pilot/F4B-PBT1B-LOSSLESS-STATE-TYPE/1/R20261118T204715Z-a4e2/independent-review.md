# Revisión independiente y adversarial del código — I-64 F4B-PBT1B-LOSSLESS-STATE-TYPE, intento 1

**Objeto:** `git diff 4b0498e3..a0a5f699` en la rama `architecture/workspace-persistente-rackcad` (worktree de I-64), con el RED intermedio `2d9febc6`. Entrega: `artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/`.
**Autoridades leídas en el GREEN:** el contrato (`docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json`), el paquete v2.7 (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`, en adelante **PKG**) y las cuatro decisiones del Owner del encargo.
**Método:** solo lectura con `git show`/`git diff` y lectura de archivos; Python de solo lectura (escrito con la herramienta de archivos) para resumir los TRX, contar selecciones de seguridad de dos fixtures y comparar el parche de `fb01` con el GREEN. No se ejecutó dotnet ni ninguna prueba; no se escribió en el worktree (`git status` limpio, HEAD `a0a5f699`).

Alias de este informe: **ES** `src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs`; **EL** `…/PushBackEditorState.Load.cs`; **RMES** `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs`; **PES** `…/PushBack/PushBackEditableState.cs`; **PESC** `…/PushBackEditableStateComparer.cs`; **PESO** `…/PushBackEditableStateOwnership.cs`; **GATE** `…/PushBackEditorOutputGate.cs`; **W**/**WC** la ventana y su parcial `Composite` en la BaseSha; **CPY** `tests/RackCad.Tests/PushBackEditableStateCopyTests.cs`; **LOAD** `tests/RackCad.Tests/PushBackEditableStateLoadTests.cs`; **GRD** `tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs`; **PAR** `tests/RackCad.UI.Tests/PushBackEditableStateParityTests.cs`.

---

## 1. Veredicto

**REWORK — B/I/M = 0/2/15.**

- **El producto (`src/`) está bien.** Las cuatro clases nuevas y los métodos nuevos de ES y RMES reproducen D-PBT1-02 a 11 y 09b. La API es exacta y no hay ningún campo nuevo. La copia solo comparte los no nulos de `workingBaseline` y `LastModuleReconciliation` de cada lado, y ninguna línea añadida muta un rack resuelto ni un informe adoptado. No aparece ninguna diferencia ni pérdida nueva frente a la ventana, salvo la normalización conocida de `pb-15-sin-composite`.
- **El rework es solo de pruebas, en dos clases que están dentro del AllowedWriteScope:**
  - la guarda G-L1 (CT-PBT1B-14) no aplica la regla 2 de D-PBT1-15 tal como está escrita (I-01);
  - un caso de CT-PBT1B-10 no puede fallar (I-02).

  Las dos taras vienen del borrador heredado de `fb01`.

## 2. Recuento

| Severidad | N | Ids |
|---|---|---|
| Bloqueante | 0 | — |
| Importante | 2 | PBT1B-IR-I01, PBT1B-IR-I02 |
| Menor | 15 | PBT1B-IR-M01 a M15 |
| OWNER | 0 | — (ver §8) |

## 3. Tabla de hallazgos

| Id | Sev. | Dónde | Qué | Qué hacer |
|---|---|---|---|---|
| **PBT1B-IR-I01** | **I** | GRD:163 (frente a PKG:692-696) | **G-L1 deja fuera media regla 2 de D-PBT1-15.** El paquete dice: «Un `<id>` cuenta como ficha para las reglas **1 y 1b** en el resto del miembro». `Violations` solo aplica a los alias la regla 1: `result.AddRange(Rule1(member, aliases.Keys)…)`, y nunca `Rule1b`. Así pasan sin verse las formas «alias + molde», justo las que V23C-I01 quiso cubrir. Por ejemplo, `var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add("x");`, o `var p = side.LastModuleReconciliation.Preserved; ((List<string>)p).Add("x");`. Las trece autopruebas pasan porque ninguna ejerce un molde sobre un alias. Hoy hay 0 coincidencias de todos modos, así que no esconde ninguna mutación actual; pero la condición 10 de D2 trata de las futuras. | Añadir `Rule1b(member, aliases.Keys)` en GRD:163 y, como defensa, una autoprueba «alias + molde». No hace falta tocar producto |
| **PBT1B-IR-I02** | **I** | CPY:924-929 (frente a PKG:912, N-M-02) | **El caso N-M-02 de CT-PBT1B-10 no puede fallar.** Los dos estados que compara tienen nombres distintos: `anterior` se carga con el nombre del sobre de `pb-31-v2-modulos`, que es `"PB-01"`, y `fresh` con `LoadState(log, rebuilt.Design, "pb-31")`. `CarriersEqual` compara `Name` (PESC:429), así que la línea `…-distintos=True` sale siempre, aunque el comparador tratase como «sin cambios» una sesión que conserva la lista confirmada de una línea base anterior. Ninguna otra prueba ejerce esa regla. Por lectura, la regla sí está implementada (PESC:236-243, ES:192-202); lo que falta es la prueba. | Cargar `fresh` con el mismo nombre (`NameOfText` del sobre) para que la única diferencia sea la sesión |
| PBT1B-IR-M01 | M | CPY:1256-1268 | **«comparar-invariante» de CT-16 no puede fallar, por cómo está planteado.** Los dos estados comparten la misma línea base y copias idénticas de la sesión, así que mutar el objeto compartido no puede cambiar el resultado de `Equals`. Es la formulación literal de la condición 7 (iii) (PKG:628, PKG:918). La (ii), que el comparador solo lea la proyección declarada, se sostiene solo por lectura (PESC:250-280). | Para la sesión: añadir un caso con dos líneas base **distintas** e iguales en la proyección, mutar `Name` en una y comprobar que `Equals` no cambia. Así la (ii) queda probada |
| PBT1B-IR-M02 | M | CPY:1151-1160 (frente a PKG:918, «y cada portador») | El bloque «editar» de CT-16 solo toca `Pallet`, `PalletsDeep`, `PostCatalogId`, `AnnotationScale`, `SeparatorCountOverride`, `Name` y `SafetySelections`. Le faltan `PostPeralte`, `PalletTolerance`, `BeamDepth`, `FirstLevelDatum`, las casillas, `Dimensions`, `DimensionStyle`, `LoadedDimensionViews`, `DimensionViewsTouched`, las tres casillas de cotas y cinco de los seis avanzados. CT-08 sí los muta, pero sin huella profunda | Completar la lista de portadores en `Edit` |
| PBT1B-IR-M03 | M | CPY:1108-1125 | `Watch` mide cada referencia nueva **al terminar el paso** que la adopta. Una mutación posterior a la adopción dentro de la misma `Pasada` (tras `AcceptComputation`) no se vería. Es un límite inherente a «desde su adopción», pero la entrega no lo declara | Declararlo como límite |
| PBT1B-IR-M04 | M | CPY:608-625 (frente a PKG:908, CT-06 (2)) | **CT-06 se aparta del texto en tres puntos:** <br>- corre sobre un solo estado rico (F-01 con una cola sintética), sin el «`pb-20-ab` con B retirado» que nombra el paquete; <br>- toma B = `Copy(R)` una vez, no por ranura; <br>- no comprueba que `undo()` deshace la mutación, cosa que el piloto sí comprueba (`WorkspaceDraftPilotStateTests.cs:1001-1002`). <br>La cobertura de ranuras E es completa igual (`campos-E-sin-ejercer=[]`) | Añadir el segundo estado y la comprobación de `undo` |
| PBT1B-IR-M05 | M | CPY:745-753, CPY:807, CPY:885-888 | **Comprobaciones más débiles que el texto en CT-08 y CT-09:** <br>- CT-08 no compara el diseño de una `Pasada` tras **cada** mutación de ranura, solo por grupo (PKG:668); <br>- «otro elemento de seguridad cambiado» solo se ejerce en `pb-06-seguridad`, porque `pb-20-ab` tiene 0 selecciones; <br>- en CT-09, `cola-de-B-intacta` solo mira que el diseño tenga algún módulo `B:`, no que la cola vuelva igual. El oráculo real es copia = original | Ninguna urgente; reforzar si se reabre |
| PBT1B-IR-M06 | M | CPY:711-713 | La parte 3 de CT-07 resta del grafo del estado **todo** lo que alcanza la línea base antes de cruzarlo con el diseño. Un alias que también alcanzara la línea base quedaría oculto. Además, tras `LoadDesign` la sesión y el commit son nulos (ES:99-106), así que la parte 3 no los ejerce | Medir desde contenido, cola, sesión y commit, no desde «todo menos la línea base» |
| PBT1B-IR-M07 | M | GRD:251, GRD:261, GRD:341-347, GRD:296-300 | **CT-15 es algo más débil que el texto:** <br>- las siete líneas del censo se cortan en `=` y no son las sentencias literales que da el paquete (PKG:646-647), así que un cambio del lado derecho no se ve; <br>- la primera comprobación de orden pide que el escritor vaya después de la declaración de `DrawPushBackView`, pero no que esté **dentro** de ella | Registrar la sentencia completa; acotar el método |
| PBT1B-IR-M08 | M | PAR:183-185, PAR:505-511, PAR:549 | CT-01 solo corre «Restaurar tras retirar B» y «tras SetGap(60)» en `pb-20-ab`, X-01 y F-01, no en los 19 diseños que lista el paquete (PKG:903). En un rack de un sentido «retirar B» degenera, pero `SetGap(60)` no. Es una reducción no declarada | Declararla o extender los caminos |
| PBT1B-IR-M09 | M | PESO:109-112 (frente a PKG:476) | **`Of` decide «propiedad declarada» mirando la tabla, no el tipo del documento.** El paquete pide `Envelope` («ExtensionData») si la primera propiedad «no es una propiedad declarada del documento de su nivel». Hoy da lo mismo, porque todas las propiedades de los tres documentos están en la tabla. Pero una propiedad nueva de `PushBackDesignDocument` saldría `Envelope` en vez de `Unclassified`, y CT-11 no lo vería | Declarar la equivalencia o comparar con las propiedades del tipo |
| PBT1B-IR-M10 | M | PESC:157,164,167,188,202,211,242,245,250,403; PESO:68,115,121,133 | **La lista cerrada de privados de INV-PBT1B-03 no se cumple al pie de la letra.** El comparador tiene `TailsEqual`, `ModulesEqual`, `LinesEqual`, `AuthorityEqual`, `Unchanged`, `OpenOver`, `BaselineIntentEqual`, `CarriersEqual`, `Persisted` y `CopyPallet`, que no salen de PC:64-83 ni de PC:134-350. El mapa tiene `Covers`, `FirstProperty`, `BuildTable` y el campo estático `IndexPattern`. Son necesarios para D-PBT1-06 y 08, pero no se declaran: la comprobación de la entrega solo miró los públicos de ES y RMES (`tests/gr/invariants-green.txt`) | Para la sesión: aclarar INV-03 en el contrato; para el Worker: declararlo |
| PBT1B-IR-M11 | M | PES:111-113, PES:121-123; EL:59 (frente a W:296, W:313, W:3309-3313) | **Con un diseño nulo, `LoadDesignEditor` y `RestoreEditor` cambian el estado y después lanzan.** Llaman a `SetActiveSide(A)` y `SetSideBPresent(false)` y luego lanzan `ArgumentNullException` desde EL. La ventana, con nulo, no hace nada. No se observa en PB-T1b (la ventana conserva sus guardas), pero un anfitrión que no sea la ventana (PB-T2/T4) heredaría esa mutación parcial | Declararlo para PB-T2/T4 |
| PBT1B-IR-M12 | M | RMES:102, RMES:136 (frente a PKG:432) | `RackModuleCommit.SameAs` compara los `double` con `!=`, así que `NaN` no es igual a sí mismo, y el comparador lo usa (PESC:228). D-PBT1-06 dice que `NaN` es igual a sí mismo. Es coherente con el `AreEquivalent` que ya existía (RMES:996) y solo afecta a una longitud o una altura `NaN` dentro de un commit | Declararlo o usar `Equals` |
| PBT1B-IR-M13 | M | CPY:1018; LOAD:282, LOAD:345; `PushBackEditorOutputGateTests.cs:48-49`; LOAD:426-444 | **La regla «EXCEPCION» (PKG:829) se aplica a medias.** Varias llamadas al API nuevo no pasan por `Paso`. En el esqueleto no lanzan y el RED falló por aserción, así que no tuvo efecto. Aparte, CT-05 rotula `RECHAZADO` igual si rechaza el almacén o la carga del estado, así que por sí solo no prueba que c-13 lance «al cargar el estado». CT-01 lo deja claro con `rechazo-igual=True` | Envolver las llamadas; distinguir los dos rótulos |
| PBT1B-IR-M14 | M | `.github/workflows/ci.yml` (`build-plugin: needs: tests`); `worker-handoff.json` | **Evidencia:** <br>- en la CI del RED, `build-plugin` sale omitido porque depende de `tests`, mientras ExpectedEvidence pide `success`. Es incoherente en el propio contrato, y la entrega lo dice solo dentro de `WorkCompleted`, no en `Deviations`; <br>- `"Trailer": null`, aunque los commits RED y GREEN llevan `Co-Authored-By: Claude Sonnet 5.5` | Para la verificación y la sesión |
| PBT1B-IR-M15 | M | PESC:211-240 | **Coste:** cada `Equals` hace dos `CopyAuthorityFrom` por lado, que copian en profundidad sesión y commit (con ida y vuelta por JSON de cada cabecera). En la CI, CT-06 tarda 27,7 s y CT-08 34,9 s. Es correcto, pero será el camino caliente del `IsDirty` de PB-T4 | Anotarlo como arrastre de PB-T4 |

## 4. Diseño → código

| Decisión | Código | Valoración |
|---|---|---|
| **D-PBT1-02** (el tipo, sus miembros y sus valores iniciales) | PES:38-95; constructor en PES:40-52 | **Conforme.** Cada miembro y cada valor inicial coinciden con la tabla de PKG:346-362. No hay ningún público de más y no contiene GUID, `sourceProject` ni derivados |
| **D-PBT1-03** (las cargas) | PES:100-126 (las mitades del editor), PES:130-169 (`AdoptCarriers`), PES:173-203, PES:211-232 (`SideAOnly`), PES:238-359 (`LoadComposite`) | **Conforme**, con el orden de W:273-276, W:299-302 y W:3320-3327. `AdoptCarriers` reproduce W:391-427 y W:1268-1287: <br>- pallet nuevo con `ResolveWeightUnit(Pallet?.WeightUnit, …)` (PES:133-135); <br>- `Math.Max(2, …)` y los «>0 ? : defecto»; <br>- `ResolvePostCatalogId` (PES:138); <br>- los seis avanzados; <br>- anotaciones y `Dimensions` recortado a `[0, Detailed]`; <br>- las cotas con `EffectiveDetail`; <br>- `DeepCopy` de la seguridad. <br>`LoadComposite` es WC:829-931 literal, sin `UpdateCompositeEnabled` y con la cola copiada campo a campo con un `store` local (PES:243-261, PES:291-299). `SideAOnly` es W:342-373 literal. Única nota: M11 (diseño nulo) |
| **D-PBT1-04** (`BuildInputs`) | PES:367-405 | **Conforme** campo a campo con W:846-889: objeto nuevo en cada llamada, unidad del estado (con `?? "kg"`), `FromEditor`, `AuthorizedSafety`. Lo confirma CT-01 (`Persist` idéntico en los 19 diseños y todos los caminos) |
| **D-PBT1-05** (el copiador) | PESC:63-136, PESC:138-155 | **Conforme**, en el orden del paquete: <br>1. `Snapshot`/`Restore` con la reposición de la selección del piloto, literal de PC:64-83; <br>2. la cola copiada y `MarkDormantTailConsumed` si estaba pendiente; <br>3. `CopyAuthorityFrom` por lado; <br>4. los portadores, con el pallet nuevo, los seis avanzados, `DeepCopy` y `Name` |
| **D-PBT1-06** (el comparador) | PESC:33-57, PESC:167-280, PESC:284-399, PESC:403-500 | **Conforme:** <br>- el contenido del editor y `StructuralEquality` son copia literal de PC:134-249 y PC:280-289/309-350; <br>- la cola, por forma persistida; <br>- la autoridad: intención de la línea base, como DA:57-81, más líneas y postes derivados; el commit con `SameAs`; la sesión con la regla «sin cambios» (null, o sin pendientes y equivalente a `Begin(baseline)`), leída en estados desechables para no abrir sesiones en el original; <br>- los portadores. <br>No compara selección, informe ni marca. Notas: M12 (`NaN` en el commit) y M15 (coste) |
| **D-PBT1-07** (ES y RMES) | ES:173-181; RMES:50-156 (commit), RMES:848-892 (sesión) | **Conforme:** <br>- `CopyAuthorityFrom` comparte solo `workingBaseline` y `LastModuleReconciliation`, y copia sesión y commit; <br>- `Copy`/`SameEditAs` de la sesión con `CopyLiteral` (ocho campos, sin `ResolveProvenance`), `CopyLines`, `AreEquivalent*`, `restored` en orden y la bandera; <br>- `Copy`/`SameAs` del commit con `CopyModule` y un `store` local (N-M-06). <br>Ningún campo nuevo (`git diff`: solo métodos) |
| **D-PBT1-08** (el mapa) | PESO:9-175 | **API exacta y tabla conforme** a PKG:479-488. Una desviación de semántica en la regla de reserva (M09) |
| **D-PBT1-09** (c-02, rama ordinal) | PES:415-428 | **Conforme** con la rama que fijó el paso 1: fila (b) `PostBox.SelectedId=<null>`, en `tests/p1/step1-lines.txt`. Las opciones son las de `UiSupport.ToOptions` (agrupadas por `Id.Trim()` sin mayúsculas, la primera, recortada). En blanco va al valor por defecto; si casa ordinalmente, devuelve `requested`; si no, el valor por defecto. Equivale a la ventana porque DA:169-171 aplica el mismo valor por defecto al nulo de `PostBox` |
| **D-PBT1-09b** (c-09, rama «conserva») | PES:435-450 y PES:133-135 | **Conforme** con la fila (f): `f carga=[kg] WeightUnitBox.SelectedItem=lb antes=ton salida=lb`. En blanco da «kg»; «kg» y «lb» ordinales se conservan; cualquier otra unidad deja `current` si es «kg» o «lb», y si no «kg» |
| **D-PBT1-10** (`pb-21-cola`) | PES:16-24, PES:454-465 | **Conforme** con la condición del piloto (PM:150-170): un sentido y módulos o líneas con id de cola |
| **D-PBT1-11** (criterio de bloqueo) | GATE:17-40 | **Literal** de WC:783-807. `RackBomOutputGate` no cambia |
| **D-PBT1-15** (D2) | ES:173-181; PESC:63-136; GRD | Producto conforme (§5). Guardas: I-01 y M07 |

**¿Cambia algún comportamiento sin declararlo?** En RACKEDITAR, no: la ventana, el piloto, el Plugin y CS no cambian, y el diff de esas rutas está vacío. En el tipo nuevo, solo M11 (diseño nulo) y M09 (regla de reserva del mapa), y ninguno de los dos se observa hoy. La copia de la cola al cargar sí está declarada (PKG:386).

## 5. Los CT

| CT | Clase y método | ¿Fija lo que promete? | Valoración |
|---|---|---|---|
| 01 (UI) | PAR, `CT_PBT1B_01_TheLoadsOfTheStateMatchTheWindowOfToday` | **Sí, compara de verdad la ventana con el estado:** `Persist` de `LastComputation.Design` (o de `DesignToInsert` con «Actualizar») frente a `Persist` del diseño de la `Pasada` del estado, más `OutputIsBlockedForTest` frente a `IsBlocked`. Cubre: <br>- c-01 a c-16; <br>- los cuatro bloqueantes con `StatusText` frente a `StatusOr`; <br>- las dos recetas X-01 y F-01, en ventana y en Core. <br>`Ventana()`, la carga dentro del `try`, `Cerrar` en `finally`, el aviso sustituido y `descartes=0` | Bien; M08 (dos caminos solo en 3 diseños) |
| 02 | LOAD, `CT_PBT1B_02_…` | **Sí.** Los tres pasos de la v2.7: <br>- (i) `JsonDiff(D, D1)` con `LegacyDeltas`, copia literal de `WorkspaceDraftPilotMaterializationTests.cs:706-735`; <br>- (ii) S1 frente a S2, con la línea de `pb-15-sin-composite` exactamente como PKG:904 (`iguales=False SideB.Fronts[0].IsActive=True->False normalizada-iguales=True`), donde `normalizada-iguales` se mide tras `SetSlotPresent(B,0,false)`; <br>- (iii) S2 frente a S3. <br>La excepción de pb-15 solo cubre esa línea: cualquier otra diferencia rompe la aserción | Bien. Las 18 entradas son las 16 fixtures sin `pb-21-cola` más X-01 y F-01 |
| 03 | LOAD, `CT_PBT1B_03_…` | **Sí**, con las líneas de la rama ordinal (PKG:905) y c-01 y c-02 en la salida | Bien |
| 04 | LOAD, `CT_PBT1B_04_…` | **Sí:** `SupportOf` sobre el corpus, `delta-filtrado=667 sha256=0b54e8ff…` (el mismo hash de CT-PB0-01) y el piloto `OutOfPilot` | Bien |
| 05 | LOAD, `CT_PBT1B_05_…` | **Sí:** las líneas son las de CT-PB0-17 (`PushBackFullEditorCharacterizationCatalogTests.cs:128-151`) sin c-02 | Bien; M13 (rótulo ambiguo de c-13) |
| 06 | CPY, `CT_PBT1B_06_…` | **Sí en lo esencial:** <br>- censo cerrado (`sin-clasificar=[]`, `obsoletas=[]`); <br>- clase D exacta, con sus tipos; <br>- el `Mutator` del piloto, copia literal de `WorkspaceDraftPilotStateTests.cs:1154-1245` con sus `ContainerKeys` ampliadas; <br>- solo las exclusiones ancladas (5)(a) y (5)(b) (CPY:426-430); <br>- solo los seis de `Advanced`; <br>- igualdad con el comparador, no por reflexión. <br>No hay ninguna exclusión fuera de las ancladas | M04 |
| 07 | CPY, `CT_PBT1B_07_…` | **Sí:** <br>- parte 1: compartidos = los no nulos de D2 y lo que alcanzan; es una comprobación real, porque falla si se comparte algo más; <br>- parte 2, sin exención, contra todo el original; <br>- `AllObjects` entra en diccionarios | M06 |
| 08 | CPY, `CT_PBT1B_08_…` | **Sí:** en las dos direcciones, por ranuras E (salvo `SafetyFacePiece`), autoridad, intención y portadores; tras cada grupo comprueba igualdad, conjunto de objetos y diseño | M05 |
| 09 | CPY, `CT_PBT1B_09_…` | Sí, con copia = original como oráculo | M05 |
| 10 | CPY, `CT_PBT1B_10_…` | **En parte.** Casi todo discrimina (`NaN`, `null`, línea base con el mismo commit, selección). **El caso N-M-02 no** | **I02** |
| 11 | `PushBackEditableStateOwnershipTests.cs`, `CT_PBT1B_11_…` | Sí, con las líneas de PKG:913 | M09 |
| 12 | `PushBackEditorOutputGateTests.cs`, `CT_PBT1B_12_…` | **Sí:** líneas literales de PKG:914, `Valid()` y `Blocking()` copiados de `PushBackOutputGateTests.cs:28-46` y cultura invariante | Bien |
| 13 | CPY, `CT_PBT1B_13_…` | Sí: ningún objeto compartido entre dos llamadas, «Restaurar estándar» no toca `Advanced`, y llegan los seis avanzados, la unidad y las casillas | Bien (no mira `DimensionViews`) |
| 14 | GRD, `CT_PBT1B_14_…` | **No del todo:** las fichas, los archivos, las reglas 1, 1b, 1c y 3, y las trece autopruebas (a…j) son las de D-PBT1-15, pero **la regla 2 no alimenta la 1b** | **I01** |
| 15 | GRD, `CT_PBT1B_15_…` | Sí en lo que fija: `archivos=14`, `asignaciones=7`, `escritores-pushback=3`, tres veces `orden=True`, `DrawPushBackView-llamadas=1` y las dos autopruebas | M07 |
| 16 | CPY, `CT_PBT1B_16_…` | **Sí** en «editar», «validar», «descartar y recargar» y en las huellas de «comparar»: huellas profundas, `Name` en claro, y referencias capturadas al empezar y al adoptarse. **No** en «comparar-invariante», que no puede fallar | M01, M02, M03 |
| 17 | LOAD, `CT_PBT1B_17_…` | **Sí:** las nueve líneas de la regla (rama «conserva») y las cinco de CT-PBT1A-02 más la sexta, `restaurar ton lb caja=[lb] … "lb"`, con la secuencia exacta de PKG:919 | Bien |

**Clases y archivos:** coinciden con PKG:899. Todas las clases de Core son `public sealed`, y cada `[Fact]` acaba en un solo `Assert.Equal`. La clase de UI está en `EditorDiscardPromptCollection`.

## 6. D2 y su límite

1. **¿La copia comparte algo más que los no nulos de `workingBaseline` y `LastModuleReconciliation`?** **No.** `CopyAuthorityFrom` (ES:173-181) reasigna esos dos campos y copia sesión y commit (RMES:848-860, RMES:50-72). Lo demás se copia en profundidad:
   - el editor, por `Snapshot`/`Restore`;
   - la cola, campo a campo con `DeepCopy`;
   - el pallet, nuevo;
   - la seguridad, con `DeepCopy` (solo comparte `SafetyFacePiece`, que es inmutable).

   La firma es una cadena. CT-07 lo comprueba de verdad (compartidos = raíces D2 y su alcance, con `raices>0=True`).
2. **¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?** **No** en `src/`. Todo es lectura:
   - el comparador lee la línea base con `Begin(baseline.Structure)` (RMES:226-254), que copia en profundidad, y con las listas de líneas y postes;
   - la puerta llama a `Evaluate`.

   Las huellas de CT-16 confirman que esos caminos no mutan nada. Las únicas escrituras sobre un rack resuelto están en la prueba (CPY:1262-1264, exigido por PKG:918) y en un diseño, no en un sistema (CPY:952).
3. **¿Las guardas aplican las reglas y las autopruebas exactas?**
   - **CT-14:** autopruebas exactas, pero **le falta la regla 2 sobre la 1b** (I01).
   - **CT-15:** censo, orden y autopruebas conformes, con las debilidades de M07.
4. **¿CT-07, 08 y 16 prueban lo que dicen?**
   - **CT-07 y CT-08:** sí; tienen debilidades menores (M05, M06).
   - **CT-16:** sí en tres bloques y en las huellas de «comparar». «comparar-invariante» es tautológico por cómo está formulado (M01).

**El límite (PBT1B-C17):** no se activó. Ni la entrega ni el código necesitan mutar el rack ni el informe.

## 7. El borrador heredado (`R20261118T193423Z-fb01`)

He comparado `uncommitted-work-at-stop.patch` con el GREEN.

- **Producto:** los cuatro archivos nuevos de `src/` y las líneas añadidas a ES y RMES son **idénticos byte a byte** al borrador escrito con la v2.6. La v2.7 no cambia nada de producto (PKG:60), así que es coherente.
- **Pruebas:**
  - `PushBackEditableStateOwnershipTests`, `PushBackEditorOutputGateTests` y `PushBackSharedReferenceGuardTests` son idénticas al borrador;
  - `PushBackEditableStateCopyTests` solo cambia en la v2.7: `NotMutated` con las exclusiones (5)(a) y (b) de CT-06, la exclusión de `SafetyFacePiece` en CT-08 (PBT1BC27-M02) y `Paso` en `Cambio` de CT-10;
  - `PushBackEditableStateLoadTests` solo cambia en los tres pasos de CT-02 (v2.7, PBT1BC27-M04);
  - CT-01 (PAR) es nuevo.
- **¿Arrastra algo de la v2.6?** En el texto de los CT, no. La «lectura por reflexión» de CT-06 que dio los falsos fallos de `fb01` (`tests/dev/ct06-failures.txt`) es justo la que la v2.7 aclaró: mutador del piloto, igualdad con el comparador y dos exclusiones ancladas. El GREEN aplica esa definición. **Sí arrastra dos defectos de prueba:** I01 y I02 ya estaban en el borrador.
- **¿El RED tenía solo el esqueleto?** **Sí.** `git diff 4b0498e3 2d9febc6 -- src` es exactamente el esqueleto de PKG:893-897, miembro a miembro, y el GREEN no toca `tests/` (diff vacío). Que la implementación no estuviera en el árbol antes del commit RED no es verificable (§9). Hay un matiz de proceso: las pruebas se escribieron y ajustaron con una implementación ya existente (la de `fb01`), y la línea (ii) de CT-02 se midió sobre ella. La v2.7 lo asume (PKG:904); la entrega lo declara.

## 8. RED, GREEN, estabilidad y contención

- **RED** (resumen de los TRX descargados):
  - `red-ci-core/core.trx`: 19 086 resultados, **14 Failed** (los 13 de `PushBackEditableState` y el de `PushBackEditorOutputGate`), todos con `Assert.Equal() Failure: Collections differ`, sin prefijo de tipo y con la primera línea del seguimiento bajo `tests/`. Las dos guardas, **Passed**. Ninguna prueba existente falla.
  - `red-ci-ui/ui.trx`: 2 437 resultados, 1 Failed (CT-01, en PAR:545), 2 415 Passed y 21 NotExecuted.
  - La tabla `tests/r1/ct-red-assertions.txt` es coherente: los fallos vienen de líneas `EXCEPCION` o de valores del esqueleto.
- **GREEN:**
  - `ci-core/core.trx`: 19 086 Passed;
  - `ci-ui/ui.trx`: 2 437 resultados (2 416 Passed y 21 NotExecuted), que son los conteos de PKG:1177;
  - tres corridas locales en verde de los filtros con ExpectRed (`tests/g1` a `g3`: 13, 1 y 1);
  - el alcance son las 12 rutas, y el diff de UI, Plugin, Domain, Pilot, Workspace y CS está vacío.
- **Determinismo:**
  - sin reloj ni aleatoriedad, con el GUID fijo;
  - cultura invariante en la UI (`Sta`) y en CT-12; las huellas usan «R» invariante;
  - las rutas salen de `RepoRoot` y la inyección de las guardas es en memoria;
  - estado estático: el sustituto del aviso es global, pero está protegido por la colección. El `Mutator` solo toca objetos de cada estado; la pieza `SafetyFacePiece`, que es compartida (`None`), está excluida en CT-06 y CT-08, así que no se muta ningún singleton;
  - el ensamblador estático de CPY se comparte dentro de una clase secuencial; riesgo bajo.
- **Ventanas:** ninguna queda viva ni se muestra (`Cerrar` es idempotente).
- **Contención:** las pruebas solo leen archivos; `cleanliness-before` y `cleanliness-after` coinciden; no se toca el Plugin ni AutoCAD.

**Hallazgos OWNER: ninguno.** No aparece ninguna diferencia ni pérdida nueva distinta de la normalización conocida de `pb-15-sin-composite`: CT-01 iguala ventana y estado en todo el corpus, y la (ii) de CT-02 da `iguales=True` en los otros 17. Tres puntos son para la sesión, no para el Owner:
- M01: la formulación de la condición 7 (iii);
- M10: la lista cerrada de INV-PBT1B-03;
- M14: `build-plugin` en la CI del RED.

## 9. Lo no verificado

- **El estado de los cuatro jobs de la CI** (runs 44400123417 y 44405531606). Sin GitHub, por la regla de solo lectura con git. Solo tengo los TRX descargados de `tests` y `ui-tests`, más `build-plugin` omitido según la entrega.
- **Que la implementación no estuviera en el árbol antes del commit RED,** y que la corrida temporal `red-probe-window-side` se revirtiera. Solo lo dice la entrega; el contenido del commit RED sí está verificado.
- **Que el paso 1 corriera sobre la BaseSha.** El TRX (`tests/p1/p1.trx`, a las 14:51, antes del RED de las 15:18) y el texto del código temporal coinciden con INV-PBT1B-06.
- **El comportamiento en ejecución:** no ejecuté pruebas. En particular, no sé si la regla N-M-02 da `distintos=True` cuando se corrija I02; por lectura de PESC:236-243 y ES:192-202 debería darlo.
- **Los procesos y el perfil durante las corridas,** más allá de las dos pruebas de limpieza.
- **Las verificaciones del Controller** (F4B-E15) y nc1 a nc3, que están fuera de este encargo.
