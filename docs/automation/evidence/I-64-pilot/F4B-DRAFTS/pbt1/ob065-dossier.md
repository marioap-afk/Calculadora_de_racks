# I-64 F4B — Expediente de la vuelta al Owner antes del contrato de PB-T1 (OB-06.5 y arrastres)

**Analista independiente, solo lectura.** Rama `architecture/workspace-persistente-rackcad`, HEAD `5597c05b`. El producto es el GREEN de PB-TC, `4440f67d` (`git diff 4440f67d 5597c05b -- src tests` vacío). Sin compilar, sin ejecutar pruebas, sin AutoCAD y sin escribir en el worktree. Todo `ruta:línea` está medido en HEAD.

**Abreviaturas:**
- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3690 líneas); **WC** = `….Composite.cs` (936); **WX** = `….xaml`.
- **DA** = `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs`; **PBCEA** = `…/PushBackCompositeEditorAssembler.cs`; **CS** = `…/PushBackCompositeEditorState.cs`; **LOAD** = `…/PushBackEditorState.Load.cs`.
- **DRSR** = `src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs`; **PM** = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs`.
- **[P]** = el paquete de la fase v3.1; **[OD]** = `owner-f4b-disposition.md`; **[OPD]** = `owner-pbtc-decisions.md`; **[SC]** = `superseding-characterization.md`; **EV** = `docs/automation/evidence/I-64-pilot/`.
- Las líneas de W y WC que citaban PB-T0 (en `f2eedd9a`) se movieron con el GREEN de PB-TC: W:390 → W:399, W:836 → W:868, WC:838-842 → WC:860-868, WC:173 → WC:198, W:1865 → W:1905.

---

## 1. Resumen para el Owner

Antes del contrato de PB-T1 (sacar el estado del editor Push Back fuera de la ventana, sin cambiar RACKEDITAR) quedan **tres decisiones tuyas**. Lo demás lo resuelven tus reglas; está en los apartados 3 y 4.

**Pregunta 1. Las cajas que aún redondean (H-31) y «Ambos lados» con la caja vacía (OWNER-1).**
Hoy el hueco entre lados, la longitud de un módulo y cuatro parámetros avanzados se guardan redondeados a 3 decimales con solo pasar por la caja. Además, pasar por la longitud de un módulo cuyo largo trae más decimales lo convierte de «calculado» en «manual». PB-T1 no puede imitar ese redondeo sin fingirlo, y tu regla dice que ningún control manda. En «Ambos lados», con la caja vacía porque los lados difieren, escribir el número que se ve no iguala los dos lados, aunque la ayuda de la caja dice que sí.
- **(A) Corregir las dos cosas en una tarea propia, PB-TC2, antes de PB-T1 (recomendada).** Es la regla de PB-TC, solo en la ventana Push Back: unas 5 pruebas nuevas, ninguna sustituida, cadena propia con 1/3. Con 3 decimales o menos no cambia nada; con más, menos de 0,0005" y el BOM en diezmilésimas.
- (C) No corregir y autorizar que PB-T1 deje esas seis cajas como están. Sería una excepción a tu requisito «ningún control es la fuente de verdad».
- Solo «Ambos lados» podría quedarse como limitación, porque PB-T1 sí puede reproducirlo sin fingir. Pero arreglarlo junto a lo anterior cuesta una prueba.

**Pregunta 2. Un poste que el catálogo no tiene (c-02).**
RACKEDITAR lo cambia sin avisar por el poste por defecto; el panel del Workspace lo conserva. Conservarlo tampoco sirve: sin ese poste en el catálogo, la retícula de troqueles, los topes y el BOM salen distintos o incompletos (lectura de código).
- **(B) Reproducir lo de hoy como regla explícita del estado, como ya se hace con las vigas (recomendada).** RACKEDITAR no cambia, y el Workspace heredará la misma regla con «Editar todo». Cuesta una prueba dentro de PB-T1.
- (A) Conservar el poste inexistente: cambia el dibujo y el BOM; tarea propia y otra vuelta contigo con el efecto medido.
- (C) Bloquear la salida con un aviso «elige un poste»: cambio visible; tarea propia.
- Las demás sustituciones al abrir (viga o peralte fuera de catálogo, peralte posterior fuera de lista, peralte de larguero 0, poste en blanco) y los dos rechazos (fondos = 1, holgura = 0) ya ocurren fuera de la ventana. Se quedan como hoy, salvo que quieras corregir alguno.

**Pregunta 3. El rack de un sentido que guarda una mitad B dormida (`pb-21-cola`).**
Al «Actualizar», RACKEDITAR pierde esa mitad guardada (el hueco y 8 módulos de B); lo dibujado no cambia en el caso medido. El producto actual ya no crea documentos así: está medido en AutoCAD que quitar el lado B no deja cola. Solo saldrían de una versión de desarrollo de I-42 (23 ago - 1 sep 2026) o de un archivo editado. El panel del Workspace ya los rechaza.
- **(C) Dejarlo fuera de la garantía «sin pérdidas» de PB-T1 (recomendada).** RACKEDITAR igual que hoy, el Workspace lo sigue rechazando en todo F4B, y guardar el lado dormido queda como deuda (H14).
- (A) Corregirlo: guardar el lado dormido en el documento. Cambia el formato guardado y una decisión de I-42; tarea grande.
- (C + aviso) Lo mismo que (C), más un aviso al abrir un rack así en RACKEDITAR. Es pequeño e iría en PB-TC2.
- Antes de preguntarte, la sesión comprueba sin AutoCAD si alguno de los 22 racks reales es así. Si alguno lo es, recomiendo (C + aviso).

**Lo que no te pregunto:** los huecos de pruebas que PB-T1 tiene que cubrir, los arrastres menores de PB-T0 y PB-TC, y las guardas de T1B. Ninguna guarda se rompe por necesidad (apartado 4).

**Dato nuevo:** uno de los dos dibujos reales medidos tiene un rack A/B. Por los nombres parece de pruebas, pero Smoke-PB podría usarlo.

---

## 2. Los casos 1 a 5

### 2.0 Cuadro resumen

| # | Caso | ¿Ventana o Application? | ¿Choca con un requisito duro de PB-T1? | Recomendación | ¿Del Owner? |
|---|---|---|---|---|---|
| 1 | H-31: `GapBox`, `ModuleLengthBox` y 4 avanzados | **Ventana** (el control reescribe el estado en un gesto sin edición) | **Sí**: «no WPF control becomes source of truth», «module state preserved», «A/B preserved» | (A) PB-TC2 antes de PB-T1 | **Sí** (Q1) |
| 2 | OWNER-1: «Ambos lados» con caja mixta | Ventana, pero la regla `Keep` es una función pura y reproducible | No | (A) en PB-TC2, junto con H-31 | **Sí** (Q1) |
| 3 | c-02: poste inexistente → defecto | **Ventana** (`PostBox`, sin respaldo) | **Sí**, si se deja como está: «no WPF control becomes source of truth» | (B) regla explícita en el estado | **Sí** (Q2) |
| 4 | `pb-21-cola`: cola de B en un rack de un sentido | **Application** (aparcado al cargar; la salida de un sentido la retira) | Sí, **si** se considera un «supported classic rack» | (C) fuera de la garantía; se reproduce; el Workspace sigue rechazándolo | **Sí** (Q3) |
| 5 | c-01, c-03, c-04/c-06, c-05/c-07, c-14 | **Application** (normalización de carga, compartida con el piloto) | No | (B) se reproducen sin trabajo | No: [OD] §7.4 y OB-RE.1 (se informa en Q2) |
| 5 | c-12 y c-13 | Rechazo antes de la ventana (almacén y resolver) | No | (B) se reproducen | No: no hay pérdida, nada se escribe |

**Cómo leo OB-06.5 en el caso 5.** OB-06.5 manda volver al Owner por «ninguna clase nueva de pérdida que PB-T0 fije tal cual». Su motivo, escrito en la propia OB, es que «PB-T1 no puede dejar que un control sea la fuente de verdad».
- En las normalizaciones de Application ningún control interviene.
- Corregirlas sería un cambio observable de RACKEDITAR, que [OD] (§7.4: «Observable behavior must remain unchanged except for explicitly authorized corrections») y OB-RE.1 solo admiten con autorización expresa.
- Reproducirlas es, por tanto, lo que ya manda la regla. La vuelta se cumple informando, y eso va dentro de Q2.
- Si la sesión prefiere la lectura literal, la misma Q2 lo cubre: su respuesta recomendada es «se quedan como hoy».

### 2.1 Caso 1 — H-31 (`GapBox`, `ModuleLengthBox` y las cuatro avanzadas anulables)

| Campo | Contenido |
|---|---|
| **Hecho medido** | **`GapBox`:** se siembra con «0.###» en cada pasada válida (WC:746, `GapBox.SetNumber(composite.Gap)`). `Gap_Changed`, en `LostFocus` (WX:148), hace `composite.SetGap(GapBox.Value.Value)` sin comparar (WC:184-200, `:198`).<br>**`ModuleLengthBox`:** se siembra al elegir un módulo (W:1833). `ModuleLength_Changed` (W:1892-1913; WX:554) escenifica `Val(ModuleLengthBox, module.Length)` (W:1905) si difiere en 0,0001 o más (W:1906). `SetLength` (W:1911) **marca el módulo como manual** (`IsManualOverride = true`, `IsCalculated = false`, `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs:185-203`), y «Actualizar» lo confirma (W:3386-3389).<br>**Avanzadas:** se siembran con `SetOptional` = `SetNumber` «0.###» (W:1595-1626). Cualquier `LostFocus` de sus cinco cajas o el clic de su casilla (WX:492, :499, :507, :510, :524 y :531; W:1656-1665) relee **las seis** en `ReadAdvancedRackParameters` (W:1632-1648). Las anulables con decimales son `ManualHeaderHeightOverride`, `DerivedPostReinforcementHeight`, `DerivedPostHeight` y `SeparatorSpacingOverride` (W:1639, :1641, :1642 y :1646).<br>**Lo registrado:** [SC]:175-183; paquete de PB-TC §10 (`docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md:1341-1349`) y D-PBTC-07 (`:475-483`) |
| **Alcance** | **Por la interfaz:** basta con pasar el foco, sin editar. Las longitudes calculadas salen del fondo de la tarima (`length = depth + …`, `src/RackCad.Application/Systems/Dynamic/DynamicRackSystemBuilder.cs:87`, `:102`), y PB-TC hace que un fondo exacto sobreviva al reabrir. Por eso **PB-TC amplió el alcance del caso del módulo**:<br>• con un fondo de 47,99213" (1219 mm), el módulo mide 47,99213, la caja muestra 47,992 y la diferencia es 0,00013 ≥ 0,0001;<br>• pasar por la caja lo deja **manual** de 47,992;<br>• es aritmética sobre el código, sin medir.<br>**Corpus:** ningún diseño trae más de 3 decimales en estos campos (gap 54, módulos 48/54, avanzados 150, 3, 18 y 140; `pbtc-measurements` E-7 y K-3).<br>**22 sobres reales:** los inventarios no recogen estos campos |
| **Efecto hoy en RACKEDITAR** | **Pérdida de datos** en el sobre al «Actualizar»: los decimales del hueco y de los avanzados, y la procedencia del módulo, que pasa de calculado a manual. **Dibujo:** < 0,0005" en el hueco y en las longitudes. **BOM:** puede cambiar en diezmilésimas (compara longitudes exactas, como R7 y R11 de T-3). Además, un módulo que se vuelve manual deja de seguir los cambios posteriores del fondo |
| **Piloto o panel** | El piloto no pasa por la ventana (H-PB-30) y conserva los valores exactos. El panel solo edita la escala |
| **Choque con PB-T1** | **Sí, con tres requisitos:**<br>• «no WPF control becomes source of truth»: es la clasificación de H-PB-44;<br>• «module state preserved»: un gesto sin edición cambia `IsManualOverride` y la longitud;<br>• «A/B preserved»: el hueco es estado A/B |
| **(A) Corregir en PB-TC2** | Es el diseño del anexo A de PB-TC v2 (`EV/F4B-DRAFTS/pbtc/package-v2.md:1217-1299`), ya revisado (ICT2 I-03 RESUELTO):<br>• `Keep(GapBox, composite.Gap)` en WC:198;<br>• `Keep(ModuleLengthBox, module.Length)` en W:1905;<br>• `KeepOptional` en W:1639, :1641, :1642 y :1646;<br>• `Val` (W:3634) se queda sin usos.<br>**Coste:** cadena completa con 1/3; 3 CT (los CT-PBTC-11 a 13 del anexo); **0 sustituciones** según el anexo A.4 y el complemento N-07 de ICT2 (`EV/F4B-DRAFTS/pbtc/independent-review-v2-check.md:69`), que hay que volver a medir en el paquete; producto en W y WC; un archivo de prueba nuevo |
| **(B) Reproducir en el estado** | **No es posible sin fingir.**<br>• Reproducirlo exige una regla «al perder el foco sin editar, redondea a 3 decimales», que es fingir el redondeo ([P] §5: «Eso es imposible sin fingir el redondeo»).<br>• La otra vía es dejar el manejador leyendo la caja, que es justo lo que H-PB-44 llama control fuente de verdad.<br>• He considerado la lectura laxa (el estado ya es la autoridad y la caja solo edita) y la descarto: contradice H-PB-44 y [P] §5, y el Owner aceptó esa clasificación en [OPD] P4 |
| **(C) Otra** | **(C1)** El Owner exime esas seis cajas del requisito «ningún control es la fuente de verdad». PB-T1 conserva los manejadores y su prueba de ida y vuelta excluye esos gestos. Coste de código 0, pero rebaja un requisito duro.<br>**(C2)** Corregirlas dentro de PB-T1 como cambio declarado: lo prohíbe D-PB-06 («Do not hide these corrections inside PB-T1 state extraction») y OB-06.2 |
| **Recomendación** | **(A).** Es la única opción que cumple a la vez [OD] PB-T1 y D-PB-06. El redondeo ya está autorizado en principio («rounding behavior/state», D-PB-06), y PB-TC solo lo aplazó (OD-PBTC-5 = (B)). El diseño y su revisión ya existen. El hallazgo de que el módulo se vuelve manual hace que el caso no sea cosmético |
| **¿Del Owner?** | **Sí.** La sesión se comprometió a volver a preguntar ([OPD] P4: «las otras cajas te las vuelvo a preguntar… antes de la tarea que separa el estado»). OB-06.5 lo exige, y el efecto en el BOM es «material» por la letra de OB-06.4 |

### 2.2 Caso 2 — OWNER-1 («Ambos lados» con una caja mixta)

| Campo | Contenido |
|---|---|
| **Hecho medido** | `ReadCellValues(side)` (W:802-830) se ejecuta una vez por lado (`ForEachEditedSide`, W:774-788, desde `CommitCurrentCell`, W:758-762). En cada lado, `Keep(caja, valor del lado)` (W:818-822) devuelve el exacto del lado si `SameShown` (W:3674-3678, W:3685-3686). `OptionalVal` (W:3665-3668) hace lo mismo con el largo manual.<br>Con A = 61,23456 y B = 60, la caja está mixta y vacía (`SetMixed`, WC:565-589; `IsMixed`, WC:432). Si se escribe 61.235, A conserva 61,23456 y B pasa a 61,235, así que los lados siguen distintos.<br>**La ayuda de la caja promete lo contrario:** «Escribe uno para aplicarlo a A y a B» (WC:583-584) |
| **Alcance** | Por la interfaz, en un rack A/B: modo «Ambos lados», una celda con valores distintos en A y B, y un número que se ve igual al exacto de uno de los lados. Ni el corpus ni los racks reales lo ejercen (solo hay un rack A/B real, en D2, apartado 6) |
| **Efecto hoy en RACKEDITAR** | No hay pérdida de datos: los dos valores son exactos y la diferencia es < 0,0005. Es un **defecto de gesto**: la caja no hace lo que dice. Antes de PB-TC los dos lados quedaban en 61,235 |
| **Piloto o panel** | No aplica: el piloto no tiene «Ambos lados» |
| **Choque con PB-T1** | No. `Keep` y `SameShown` son funciones puras de (valor escrito, valor del lado), y el estado extraído puede aplicarlas igual. No hay ningún control como fuente de verdad |
| **(A) Corregir en PB-TC2** | En `ReadCellValues`, una caja que estaba mixta y recibe un valor lo aplica a los dos lados sin `Keep`: `IsMixed(caja) && caja.Value.HasValue ? caja.Value.Value : Keep(…)`, y lo mismo en `OptionalVal`. **Coste:** 1 CT (UI-STA, compuesto con A ≠ B); unas líneas en W; 0 sustituciones (ninguna prueba de PB-TC fija «Ambos lados»: M-03 de PB-TC y `grep` sobre `PushBackDeclaredCorrection*.cs`) |
| **(B) Reproducir en PB-T1** | Posible sin fingir; coste 0. El defecto de gesto se queda |
| **(C) Otra** | — |
| **Recomendación** | **(A), dentro de PB-TC2.** Usa la misma regla, los mismos archivos y la misma cadena que H-31, cuesta una prueba y restaura la promesa de la ayuda. Si el Owner no quiere PB-TC2 por H-31, entonces (B) |
| **¿Del Owner?** | **Sí**, por su propia orden: «La propuesta de arreglo vuelve al Owner antes del contrato de PB-T1» (`owner-pbtc-mixed-decision.md`) |

### 2.3 Caso 3 — c-02 (`HeaderPostCatalogId` inexistente → el poste por defecto)

| Campo | Contenido |
|---|---|
| **Hecho medido** | **Siembra:** `PostBox.SelectedId = string.IsNullOrWhiteSpace(inputs.PostCatalogId) ? catalog?.Defaults?.Post : inputs.PostCatalogId;` (W:399). `SelectedId` es `SelectedValue` (`src/RackCad.UI/Controls/CatalogCombo.cs:24-28`), y un id ausente de la lista no queda seleccionado.<br>**Lectura:** `PostCatalogId = PostBox.SelectedId` (W:868), **sin respaldo**. Con nulo, el ensamblador pone `catalog.Defaults.Post` (DA:169-171).<br>**Las vigas, en cambio, sí tienen respaldo:** `SelectedValue as string ?? cell.InOutBeamCatalogId` (W:823) y lo mismo para el intermedio (W:826).<br>**Lo fija:** CT-PB0-17 (`tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationCatalogTests.cs:46`, `:130-131`).<br>**El catálogo entregado tiene un solo poste** (`assets/catalogs/secciones.csv:2`; `defaults.json:2`), y el otro histórico (`POSTE_OMEGA_3X3`) salió del catálogo el 2026-06-22, antes de que existiera Push Back |
| **Alcance** | **Solo por documento:** un JSON editado, un catálogo editado por el usuario (CSV) o un dibujo hecho con otro catálogo. Con el producto y el catálogo entregados no se alcanza. No está en el corpus (CT-17 lo fabrica sobre `pb-01-base`). En los 22 sobres reales no se sabe, porque el inventario no recoge el poste |
| **Efecto hoy en RACKEDITAR** | Al abrir, el poste guardado se sustituye en silencio por el poste por defecto; al «Actualizar», el sobre y el dibujo pasan a ese poste (BOM, retícula de troquel del poste por defecto). Es un cambio silencioso de pieza |
| **Piloto o panel** | **Conserva el id:** LOAD:197 copia `HeaderPostCatalogId` y DRSR:64-66 solo sustituye el blanco (medición M8 de PB-T0). Pero con ese id el resolver no encuentra la conexión del troquel, y por lectura de código:<br>• la base de la retícula sale (0,0) (DRSR:67-77; `src/RackCad.Application/Systems/Selective/SelectivePostGeometry.cs:173-178`);<br>• el ancla de los topes sale nula (`src/RackCad.Application/Systems/PushBack/PushBackRearTopeBuilder.cs:141`, fijado por `tests/RackCad.Tests/PushBackTopeAnchorPerViewTests.cs:81-92`);<br>• el BOM describe el poste con el id crudo (`src/RackCad.Application/Systems/Dynamic/SystemBomBuilder.cs:206`, que Push Back reutiliza).<br>Hoy el panel y RACKEDITAR ya divergen en este caso |
| **Choque con PB-T1** | **Sí**, si se deja como está: el combo decide el valor («no WPF control becomes source of truth»). No choca con «round-trip lossless» si la sustitución pasa a ser una regla de carga explícita, como ya son las de las vigas (caso 5) |
| **(A) Corregir (conservar el id)** | PB-TC2, o tarea propia: `PostBox.SelectedId ?? <id cargado>` y mostrar el id desconocido. **Coste:**<br>• 2-3 CT;<br>• **sustituir CT-PB0-17** por el patrón I-39, porque su línea de c-02 fija la sustitución;<br>• medir el efecto en dibujo, topes y BOM, que por lectura es material por OB-06.4, y volver al Owner con él |
| **(B) Reproducir en el estado** | **Posible sin fingir:** una regla de Application del tipo «id de poste que no está en el catálogo → nulo», que DA:169-171 ya convierte en el poste por defecto. Es la misma forma que `ResolveBeamId` para las vigas (`src/RackCad.Application/Systems/Dynamic/DynamicRackLevelGeometry.cs:200-204`).<br>**Coste:** una función pura y 1 CT de Core dentro de PB-T1; CT-PB0-17 sigue en verde; RACKEDITAR no cambia.<br>**Dónde va:** en la carga clásica del estado extraído. El piloto no se toca en PB-T1, y la divergencia del panel queda anotada para PB-T4, que la hereda al consumir la API del estado |
| **(C) Otra** | Conservar el id y bloquear la salida con un diagnóstico («el poste X no existe en el catálogo: elige uno»). Es un cambio visible y un texto nuevo para el Owner; tarea propia, 2 CT |
| **Recomendación** | **(B).**<br>• **Refutación de la opción fácil (A),** «corregir porque el piloto lo conserva y las vigas tienen respaldo»: el id conservado produce un rack incoherente (troquel en 0, topes sin ancla, BOM con un id crudo). Sería un cambio material de RACKEDITAR para dejarlo peor.<br>• **Refutación de la otra opción fácil,** «reproducir no necesita al Owner»: OB-06.5 nombra la clase (c) como no autorizada, y es él quien autoriza reproducirla |
| **¿Del Owner?** | **Sí** (OB-06.5: clase (c) de la ventana). Su respuesta fija también qué hará el Workspace cuando llegue «Editar todo» |

### 2.4 Caso 4 — `pb-21-cola` (OWNER-3 de PB-T0)

| Campo | Contenido |
|---|---|
| **Hecho medido** | **El diseño:** un rack de un sentido (`SideB = {"IsPresent": false}`, que da `IsComposite` falso, `src/RackCad.Domain/Systems/PushBack/PushBackDesign.cs:70`) que guarda la secuencia del rack compuesto: `M1..M8`, `GAP` y `B:M8..B:M1`, con PalletsDeep 8.<br>**Al cargar:** `LoadExisting` → `LoadCompositeFromDesign` aparca la cola en el estado (WC:860-868; `ParkDormantTail`, CS:74-96).<br>**Al recalcular:**<br>• `BuildSideB` devuelve nulo (CS:1026-1029), así que el rack no es efectivamente compuesto;<br>• PBCEA:52-57 vuelve a aparcar y entrega la cabeza;<br>• DA:279-287 retira la cola de la copia;<br>• PBCEA:81-87 declara `AbsentSlotsB = [0]`.<br>**Resultado:** la salida pierde 667 hojas (los 9 módulos de la cola, `SideB` y el `AbsentSlotsB` añadido), fijado por CT-PB0-01 (`tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationStateTests.cs:146-159`). CT-PB0-02 (`:237`) prueba que «Actualizar» escribe esa misma salida.<br>**En la misma sesión la cola no se pierde:** al declarar B y activar una ranura vuelve entera (CT-PB0-06, `PushBackFullEditorCharacterizationCompositeTests.cs:66-96`).<br>**Es deuda conocida:** «dormir no persiste, esa es la deuda H14» (CS:54); [P] R-PB-02 (`:729`): «la cola aparcada vive en el borrador igual que en el clásico» |
| **Alcance** | **Por la interfaz actual: no.**<br>• Quitar B, o dejar B sin ranuras, aparca la cola y la retira de la salida (CT-PB0-07, `…CompositeTests.cs:98-127`).<br>• **Medido en AutoCAD** (bloque M, M-4): tras la ida y vuelta A/B el sobre vuelve a 6 módulos con `SideB` ausente (`EV/F4B-PBT0-CHARACTERIZATION/block-m/inventories/M4-ab3.summary.txt:13-15`; `envdiffs.txt`, solo `AbsentSlotsB`).<br>• **H-077 no lo crea:** la vista insertada tras una edición lleva el sobre previo (M-6b), y eso da hermanas con diseños distintos, no una cola en un rack de un sentido.<br>**Origen probable:** versiones de desarrollo de I-42 entre `45d6e63f` (2026-08-23) y `4f52bc57` (2026-09-01). El mensaje de `4f52bc57` (R-1) dice que «la secuencia compuesta seguía viajando» en el diseño de un sentido. No está medido.<br>**Corpus:** `pb-21-cola`, fabricado en F4-T2A (`19123dfe`).<br>**22 sobres reales: no determinable con lo custodiado** (el resumen no lista los ids de módulo):<br>• D1 tiene 14 sobres de un sentido con 4 a 6 módulos;<br>• D2 tiene 3 racks de un sentido con 15 módulos y 8 o 1 frentes, junto a un rack compuesto de 15 módulos (`M7-D2-pre.summary.txt:2-9`). Es compatible con una cola, pero también con racks de 15 fondos |
| **Efecto hoy en RACKEDITAR** | **Pérdida de datos** en el sobre al «Actualizar»: la mitad B dormida y el hueco manual (54). En el caso medido el dibujo y el BOM no cambian, porque la cola no se dibuja en un rack de un sentido.<br>**Riesgo por lectura, sin medir:** con una cabeza personalizada, la carga resuelve 17 módulos contra 8 posiciones y DRSR:135-144 reconstruye la receta estándar, así que la cabeza también se perdería. `pb-21-cola` tiene la cabeza estándar y no lo muestra. Los documentos de aquellas versiones ya tenían la cabeza estándar (R-1 de `4f52bc57`) |
| **Piloto o panel** | **Lo rechaza, sin pérdida:** «El diseño aparca una cola compuesta: fuera del piloto.» (PM:108, :160-170), con la pista «Usa RACKEDITAR…» (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:56`). Lo fijan:<br>• `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs:119-127`, `:334-350` y `:434`;<br>• `WorkspaceDraftPilotIntentReadTests.cs:610-619`.<br>**El copiador del piloto no lleva la cola** (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs:53-60`): es trabajo de PB-T1 (H-PB-27) |
| **Choque con PB-T1** | • **«parked queue preserved»:** no, si PB-T1 lleva la cola en el estado y en sus copias (CS:58-61, `dormantTailConsumed`).<br>• **«A/B preserved»:** no. El objeto `SideB` no presente es inerte (`IsComposite` falso antes y después).<br>• **«state round-trip lossless for supported classic racks»:** **sí, si `pb-21-cola` es un rack soportado**, porque documento → estado → documento pierde 667 hojas. La forma no la escribe el producto actual |
| **(A) Corregir (persistir la cola dormida)** | Producto en Application:<br>• PBCEA y DA dejan de retirar la cola;<br>• la carga de un sentido con cola resuelve solo la cabeza (hoy `SideAOnly` solo lo hace para compuestos, W:342-373);<br>• el resolver de un sentido no puede ver la cola, sin tocar `DynamicRackSystemResolver`, que comparte con el Dinámico;<br>• hay que admitirlo en el piloto.<br>Revierte la decisión H14 de I-42 y cambia lo que significa el formato guardado. **Coste:** ≥ 4 archivos de `src`; ≥ 4 CT; **sustituir CT-PB0-01 y CT-PB0-06** con registro; cadena propia; probable vuelta por OB-06.4 |
| **(B) Reproducir en el estado** | Sale sola, porque la mecánica ya está en Application: la cola se aparca en el estado, vive en la sesión y no viaja a la salida. PB-T1 solo tiene que llevarla en el copiador y el comparador. Coste 0 adicional |
| **(C) Otra** | **(C)** Lo mismo que (B), más dos declaraciones en el contrato de PB-T1:<br>• «un rack de un sentido con cola compuesta guardada no es un supported classic rack para la garantía sin pérdidas»;<br>• el Workspace conserva `DormantTailOutOfPilotReason` en todo F4B (OB-03.1 se cumple porque nunca se admite).<br>**Coste:** 0 de código; una prueba que fije la exclusión (CT-PB0-01 ya fija la salida).<br>**(C + aviso)** Lo mismo, más un aviso no bloqueante en RACKEDITAR al cargar un rack así, con el patrón del aviso de D-PB-08 (`TextBlock` en `StatusContent`). Va en PB-TC2: WX, WC, 1-2 CT y un texto que propone el paquete |
| **Recomendación** | **(C)**, condicionada.<br>• **Refutación de la opción fácil:** «es Application, no hay que preguntar» es falso. Que el rack esté soportado o no es una lectura del requisito duro del Owner, que solo él define. Y RACKEDITAR abre el rack sin avisar y pierde datos en silencio, contra el espíritu de D-PB-08 («Do not silently imply…»).<br>• **Por qué no (A):** es un cambio de formato y de una decisión de I-42 para un documento que el producto actual no escribe.<br>• **Condición:** antes de preguntar, la sesión mira en los inventarios crudos de D2 (`<carpeta-de-copias-de-PB-T0>/inv/M7-D2-pre.txt`, sin AutoCAD) si algún sobre de un sentido trae ids `GAP` o `B:`. Si alguno lo trae, recomiendo **(C + aviso)** |
| **¿Del Owner?** | **Sí:** define el alcance de «supported classic racks» ([OD] PB-T1) y de «parked queue state» (D-PB-03). OB-06.5 lo exige además (OWNER-3) |

### 2.5 Caso 5 — los demás casos de T-2

| Caso | Hecho medido | Alcance | Efecto hoy en RACKEDITAR | Piloto o panel | ¿Ventana? ¿Choque? | Opción | ¿Owner? |
|---|---|---|---|---|---|---|---|
| **c-01** poste en blanco → defecto | DRSR:64-66 y DA:169-171. La ventana lo sustituye también, y antes (W:399). CT-17 (`…CatalogTests.cs:45`, `:128-129`) | Solo por documento (también `pb-12-poste-nulo` en WLC) | El sobre pasa de «» al id del poste. El dibujo no cambia (el resolver ya usaba el poste por defecto) | Igual (Application) | Application. Sin choque | (B), sin trabajo | No |
| **c-03** peralte posterior 7,25 → 3,5 | `state.NormalizePeraltes(...)` (DA:155; ES:259-269; `PushBackResolver.cs:146`). La lista fija de la ventana (W:44; W:480-481) es inalcanzable: coincide con el catálogo (`assets/catalogs/secciones.csv:10`) | Solo por documento, o con otro catálogo (H-PB-31) | El sobre se normaliza. El dibujo ya salía así | Igual | Application. Sin choque. Si PB-T1 sustituye `RearPeraltes`, la lista visible no cambia | (B) | No |
| **c-04 / c-06** viga inexistente → defecto | `ResolveBeamId` (`DynamicRackLevelGeometry.cs:32`, `:41`, `:46-48` y `:200-204`). Los combos de la ventana tienen respaldo (W:823, :826) | Por documento o por un catálogo editado | El sobre pasa al larguero por defecto. El dibujo ya salía así | Igual | Application. Sin choque | (B) | No |
| **c-05 / c-07** `InOutBeamDepth` 6,5 → 6 | `DynamicRackLevelGeometry.cs:42-45` → `DynamicLoadBeamGeometry.cs:52-71`. La ventana tiene respaldo (`SelectedPeralte`, W:824). **La clase (d) no se alcanza** | Por documento o por catálogo | Igual que arriba | Igual | Application. Sin choque | (B) | No |
| **c-12** `PalletsDeep = 1` | `RackProjectStore.ValidateProject` lanza `InvalidOperationException` (`src/RackCad.Application/Persistence/RackProjectStore.cs:343-353`). `Math.Max(2, …)` de LOAD:196 y W:396 es código muerto en esta vía | Solo por documento (el producto nunca escribe 1) | **Error:** RACKEDITAR informa por `RackCommandSupport.Report` (`src/RackCad.Plugin/RackMenuCommands.cs:156`, `:184`, `:186-188`) y no abre | Failed, «Ilegible o de versión futura.» (PM:141-142). El texto es engañoso, pero no es de PB-T1 | Rechazo antes de la ventana. No hay pérdida. Sin choque | (B) | No |
| **c-13** `PalletTolerance = 0` (D-3) | `Validate` lanza «La holgura transversal debe ser mayor que cero.» (DRSR:473-474, desde `:45`), dentro de `LoadExisting` y sin `try` (`src/RackCad.Plugin/RackPushBackCommands.cs:168-169`). Las normalizaciones `> 0 ? : defecto` de W:397-398 y LOAD:199 son inalcanzables | Solo por documento (el resolver nunca deja escribir 0) | **Error informado, el rack no se reabre.** No hay cierre inesperado (RackMenuCommands.cs:186-188) | Failed con el mensaje (PM:177-180) | Application. Sin choque. PB-T1 tiene que mantener el mismo tipo, el mismo mensaje y la misma vía de informe (red, §3) | (B). Corregirlo (abrir con la holgura por defecto) sería un cambio nuevo y no se propone | No |
| **c-14** `BeamDepth = 0` → 6 | LOAD:200. W:402-403; DA:165-168 (`ResolveBeamDepth`; el catálogo declara solo 6, `secciones.csv:8`) | Solo por documento | El sobre pasa de 0 a 6. El dibujo ya usaba 6 (H-29) | Igual | Application. Sin choque | (B) | No |

**Nota.** CT-17 compara la salida con el sobre **vuelto a deserializar**, así que las normalizaciones del almacén (`Dimensions` fuera de rango → 0, unidad en blanco → kg, escala ≤ 0 → 1; `DynamicRackSystemDocument.cs:205`, `:226-227` y `:429-432`) no aparecen como delta. Viven en Application, se reproducen solas y no son decisión de nadie.

---

## 3. Los huecos de red (caso 6): no son decisiones del Owner

Ninguno es una pérdida ni un cambio de comportamiento. La regla que los resuelve es [P] §6.2 («Un control sin prueba queda listado como hueco, para que PB-T1 conozca su red»), el cierre de PB-T0 (D-5 → «Paquete de PB-T1, que decide si completa la red») y el cierre de PB-TC §4. Ninguno de los huecos de D-5 esconde una clase nueva de pérdida: son casillas, enteros o cajas sin siembra desde el modelo. El único con decimales, el manejador de `GapBox`, ya está en el caso 1.

### 3.1 D-5: los controles sin una prueba que los cambie (T-1 de PB-T0)

He contado las pruebas de UI existentes que nombran cada control (`git grep "\"<nombre>\""`). Que una prueba lo nombre **no** prueba que fije su efecto en la salida, y el paquete de PB-T1 debe comprobarlo.

| Control (grupo de MB) | Pruebas de UI que lo nombran, fuera de PB-T0 | Lo que debe añadir PB-T1 |
|---|---|---|
| `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck` (H-029) | Solo T-KB (tabulación) y las del shell del Selectivo y del Dinámico | **Hueco real:** cambiar cada casilla y fijar `Annotations.*` en la salida |
| `DimensionsFrontalCheck`, `DimensionsLateralCheck`, `DimensionsPlantaCheck` | `PushBackDimensionViewsWindowTests.cs`; `PushBackDeclaredCorrectionDimensionStyleTests.cs` (lateral) | Comprobar que fijan `DimensionViews` en la salida y el par cargado/tocado de I-50 (W:1249-1303) |
| `WeightUnitBox` | `PushBackPalletScopeTests.cs` | Hueco de escritura (c-09: el desplegable muestra kg y el diseño conserva ton). Fijar los dos |
| `PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` (H-030) | 7, 5, 19 y 5 archivos | Comprobar que alguna fija la salida por celda y por frente; si no, CT nuevo |
| `CellIntermediatePeralteBox`, `CellFondoOverrideBox`, `CellDrawPalletCheck` | 4, 5 y 2 (`PushBackCellConfigurationWindowTests.cs`) | Igual |
| Manejador de `GapBox`, `CentralSeparatorCheck`, `StructureOverrideBox`, `CellTopologyBox`, `RunDirectionBox`, `TopologyScopeBox` (H-031) | `PushBackCompositeWindowTests.cs` (todos) | Comprobar que fijan `Composite.*` en la salida |
| `FrontCountBox` | 5 | Igual |
| `DerivedPostLineHeightBox` (sin siembra desde el modelo; solo lo lee su botón, W:2127-2133) | `PushBackHeaderCartesianTests.cs` | Igual |

### 3.2 Arrastres de PB-T0 y de PB-TC

| Id | Qué (medido) | Lo que debe hacer el paquete de PB-T1 |
|---|---|---|
| PB-T0 M-01 | `Abrir` crea la ventana y llama a `LoadExisting` sin `try` (`tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs:126-133`). En c-13 queda una ventana inerte | Las pruebas nuevas usan el patrón D-PBTC-08 (`var w = Ventana(); try { carga } finally { Cerrar(w); }`). Si cambia el ayudante de PB-T0, es un cambio declarado (cerrar y volver a lanzar) |
| PB-T0 M-04 | CT-08 y CT-09 leen el estado del editor, no `Structure.Fronts` ni `Composite.AbsentSlots*` de la salida | Después de PB-T1, el estado es el mismo objeto que se extrae. Hace falta una aserción sobre la salida, en una prueba nueva |
| PB-T0 D-3 | c-13: `LoadExisting` lanza (caso 5) | Una prueba que fije que la carga desde el estado extraído lanza el mismo tipo y el mismo mensaje, sin mostrar ventana. Prioridad baja |
| PB-T0, observación de CT-16 | Aceptar Seguridad sin cambios sobre `pb-05-topes` añade el portador de la familia de botas (`ApplyBootSections`, W:2782-2814) | Fijar si ese portador llega al BOM o al dibujo (pasa por `AuthorizedSafety`, W:884-887). Si cambia el BOM, se anota como comportamiento existente y se reproduce |
| PB-TC M-02 | CT-PBTC-08 es en parte tautológica | Comparar `IsEnabled` de todos los botones de acción con el aviso visible y sin él (PB-T1 o PB-T2) |
| PB-TC M-03 | Ningún CT fija (a) en «Ambos lados», en el alcance (W:1346), en los datos del frente (W:1510), en «Restaurar» (W:3307) ni en los portadores (W:833-844). D-PB-11 no tiene caso negativo | Red de PB-T1 **antes** de mover el estado. Si se hace PB-TC2, su CT de OWNER-1 cubre la parte de «Ambos lados» |
| PB-TC M-04 / D-1 | Higiene de la evidencia de la entrega | No es red: es una lección para la plantilla del prompt del Worker |
| PB-TC M-05 | Si el segundo `Click` de CT-PBTC-14 lanzara, el `finally` cerraría con cambios de módulos pendientes y saldría un `MessageBox` real | En las pruebas nuevas, confirmar o cancelar en un `finally` interno. CT-PBTC-14 no se edita, salvo como cambio declarado |

### 3.3 Red que exigen los casos 1 a 5 (trabajo de PB-T1, no decisión)

- **El copiador y el comparador completos** (H-PB-27). Hoy `PushBackPilotStateComparer.Copy` no lleva baseline, sesión, commit ni cola (`…/Pilot/PushBackPilotStateComparer.cs:53-60`). «parked queue preserved» y «no aliasing» se prueban con `pb-20-ab`, `pb-21-cola`, X-01 y un compuesto con cola despierta.
- **La ida y vuelta** sobre los 17 diseños y X-01. Si la respuesta a Q3 es (C), se declara la exclusión de la cola en un rack de un sentido.
- **Si la respuesta a Q2 es (B),** un CT de Core de la regla del poste, y CT-17 en verde.

---

## 4. Las guardas de T1B y las de texto (caso 7)

**Conclusión: OB-T1B.3 no obliga a preguntar ya.** Ninguna guarda queda roja por necesidad al extraer el estado. Tres guardas dependen de decisiones de diseño del paquete de PB-T1: dos de T-SG sobre el nombre y una de T-PG sobre la entrada de la carga. Si el paquete elige un diseño que no las mantiene en verde, OB-T1B.3 obliga a volver al Owner antes del contrato **con esta lista**.

**Recomendación al paquete de PB-T1:** fijar como invariantes los puntos de la columna «Condición».

| Guarda | `ruta:línea` | Qué fija en W o WC | Riesgo con PB-T1 | Condición para seguir en verde |
|---|---|---|---|---|
| **T-SG S-9** | `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs:293-298` | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` (W:3498) | **MEDIO** | `SaveLibrary_Click` no cambia. Leer `NameBox.Text` equivale al estado: `Name_Changed` (W:1234-1238) lo copia en cada cambio y `LoadFromModel` siembra los dos (W:390 y W:322). Además, el texto sin recortar es la sugerencia del diálogo, y cambiarlo por el nombre recortado sería un cambio observable |
| **T-SG CT-22** | mismo archivo, `:323-341` | El orden `PromptSaveToLibrary` → `session.Identity.EnsureId();` → `session.Identity.SetName(NameBox.Text?.Trim());` → `RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject)` → `Save` (W:3498-3510) | **MEDIO** | Siguen en W los miembros `session`, `lastComputation`, `sourceProject` y la lectura de `NameBox`, aunque sean reenvíos al estado. **Rompe** si el nombre o `sourceProject` pasan al estado como autoridad y W deja de leerlos con esos nombres |
| **T-PG CT-25** | `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs:235-272` | En `EditPushBack`, en este orden: `window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);` → `if (!window.InsertRequested)` → `var design = window.DesignToInsert;` → `var system = window.SystemToInsert;`. Además `window.RackId`, `window.RackName`, `window.UpdateOnly` y `window.InsertionRequest.Views` | **MEDIO-BAJO** | La entrada sigue siendo `LoadExisting`, que construye el estado dentro (OB-01.1b: «esos constructores y sus llamadas no cambian»). **Rompe** si «classic RACKEDITAR still works through the extracted state» se interpreta como que el Plugin construya el estado |
| T-SG CT-20 S-7 | `…SourceGuardTests.cs:280-284` | `new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();` (W:3478) | Bajo | `lastComputation` sigue en W (es derivado, no estado editable) |
| T-SG CT-23 | `:356-379` | `if (moduleOwner.ModuleSession.HasPendingChanges) moduleOwner.CommitModuleEdits();` (W:3336) y los argumentos del lote | Bajo | `moduleOwner` sigue como miembro de W |
| T-SG CT-21 | `:302-319` | `new RackPushBackSystemWindow(context.CanInsertInAutoCad) { Owner = context.Owner };` ×2 en `EditorModules` | Bajo | OB-01.1b |
| T-SG CT-16, S-5, S-6, S-8 y el `OnClosed` del configurador | `:216-231`, `:268-278`, `:286-291` y `:343-352` | Dueños y literales de los subdiálogos (W:2466, :2486) | Bajo | No tocar los subdiálogos |
| T-SG CT-19 y `InsertRequestedRaised` | `:235-264` | Ningún `.cs` ni `.xaml` de `Systems/PushBack` contiene `KeyDown`, `TabIndex`, `.Focus(`, `DialogResult`… | Bajo | Un archivo nuevo de UI bajo `Systems/PushBack` tampoco puede contenerlos |
| T-PG CT-24, CT-26, CT-27 y las fachadas | `…PluginGuardTests.cs:196-231` y `:275-390` | Literales del Plugin | Bajo | PB-T1 no toca el Plugin |
| **G-10** | `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:723-774` (pruebas en `:770` y `:776`) | W tiene **exactamente una** `.RequestUpdate(` (W:3409), y ningún otro archivo de `src` la tiene | Bajo en PB-T1; **alto en PB-T2** | Ningún miembro ni llamada nueva se llama `RequestUpdate` |
| I-55 | `tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs:8-15` | `PhysicalPostIndex(LateralSectionBox.SelectedItem)` (W:3135) y `corte.PostIndex == section` (W:3224), **en W** | Bajo | No mover la selección de vista de W |
| Módulos | `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:381-400` | Sobre W **en crudo, con comentarios**: contiene `RequestStandardRestore()` (W:2539) y no contiene `forceRebuild:` ni `forceRebuild)` (hoy solo hay `<c>forceRebuild</c>` en W:2535) | Bajo-medio | No mover `RestoreAllModules_Click` (W:2537) de W, y no escribir esas dos formas ni en un comentario |
| Desviador | `tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs:311-318` | `desviadorLevelsPerPost: DesviadorLevelsPerPost()` (W:3044) y `showDesviadorSide: false` (W:3048), **en W** | Bajo | No tocar la vista previa |
| T-KB, T-OW, T-RS, foco, `AnnotationScaleBox` | `…KeyboardTests.cs:126-282`; `…OwnerTests.cs:49-74`; `…ResourceTests.cs:24-48`; `RichEditorContractTests.cs:36-53`; `Workspace/WorkspaceDraftPilotValidationParityTests.cs:82-99` | WX | Bajo | PB-T1 no toca WX |
| Censo de ventanas | `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:98-160` | Ningún `Window` nuevo | Bajo | — |
| Corpus de WLC | `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:324-333` | Un `pb-*.json` nuevo en `Fixtures/I64` la hace fallar | Restricción de la red | Fixtures nuevos fuera de `Fixtures/I64` |
| RAM Elements | `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs:85-93` | Literales de las pruebas de Core | Restricción | — |
| Núcleo de borradores | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs:232-247` | `src/RackCad.Application/Workspace/Drafts/` no nombra `PushBack`, `Pilot` ni `RackCad.Application.Systems` | Restricción | El estado extraído vive en `Systems/PushBack` (R-PB-05) |
| **Costuras internas** (no son guardas de texto, pero atan miembros de W) | W:165-193 (`Session`, `State`, `Assembler`, `LastComputation`, `CurrentInputsAreValid`, `SafetySelections`…), WC:43 (`CompositeState`), W:383, W:1569, W:3527 y las de T-7 de PB-T0 | Las usan T1B, PB-T0 y PB-TC | **Alto si se quitan** (la suite de UI no compila) | PB-T1 las conserva como reenvíos. No necesitan `InternalsVisibleTo` nuevo |

**Para PB-T2, informativo:** al separar aplicar, cerrar y escribir en `RequestDraw` (W:3363-3418), las que con probabilidad saltan son G-10, T-SG CT-23 y T-PG CT-25. Esa vuelta es de PB-T2, antes de su contrato.

---

## 5. La PB-TC2 que se recomienda

| Punto | Propuesta |
|---|---|
| **Qué entra** | **(1)** H-31: el diseño del anexo A de PB-TC v2 (§2.1, opción A).<br>**(2)** OWNER-1: una caja mixta que recibe un valor lo aplica a los dos lados (§2.2).<br>**(3) Solo si el Owner lo elige en Q3:** el aviso de rack de un sentido con cola guardada.<br>**(4) Solo si el Owner elige (A) en Q2:** conservar el poste inexistente. No se recomienda; obligaría a medir y a volver al Owner |
| **Qué no entra** | `NumericField` (OB-06.6) y Cantilever, que no cambia y cuyas 67 pruebas de UI van en `RequiredTests`. Tampoco los casos de Application del caso 5, ni nada de PB-T1 |
| **Pruebas** | 3 CT de H-31 (los CT-PBTC-11 a 13 del anexo, renumerados), 1 de OWNER-1 y, si entra (3), 1-2 del aviso. **Sustituciones previstas: 0** (anexo A.4, N-07 de ICT2; ninguna prueba de PB-TC fija «Ambos lados»). El paquete debe volver a medirlo con `git grep` |
| **Producto** | W y WC. Con (3), además WX |
| **Efecto declarado (OB-06.4)** | • Con 3 decimales o menos, nada.<br>• Con más: el hueco y los avanzados exactos, longitudes con diferencias < 0,0005, y el BOM puede cambiar en diezmilésimas.<br>• Pasar por `ModuleLengthBox` deja de convertir un módulo calculado en manual, y escribir un valor que se ve igual deja de escenificarse.<br>• «Ambos lados» vuelve a igualar los lados.<br>Como en PBTC-C14, la cadena mide el delta de la base y para si el efecto es material y no se ha medido |
| **Autoridad que necesita** | • [OD] D-PB-06 («rounding behavior/state», los seis pasos y «Do not hide these corrections inside PB-T1»);<br>• la respuesta del Owner a Q1, y a Q3 si entra (3);<br>• OB-06.2 a OB-06.6 y OB-T1B.2, si apareciera una sustitución;<br>• D-PB-10 (un presupuesto propio de 1/3, con el intento 3 siempre por el Owner);<br>• las decisiones de procedimiento [DP] F4B-E03 (firmas EXACTAS, una replanificación);<br>• el registro en [SC], con su sección nueva y el «Pendiente» de PB-T1 cerrado, y su anotación en `f4b-dispositions.md`;<br>• el paso 4 de D-PB-06 en el Workspace sigue pendiente de PB-T2 (OB-06.3) |
| **Orden** | Cierre de PB-TC (hecho) → **PB-TC2** → contrato de PB-T1.<br>El paquete de PB-T1 se puede redactar en paralelo (solo documentos), pero sus hechos se miden en el cierre de PB-TC2, que es su `BaseSha`.<br>Delegaciones sucesivas (R-PB-09) |
| **Coste aproximado** | La cadena completa de F4: paquete con revisión independiente, contrato, planificación, A1-A8, nc4, Worker con RED y GREEN, CI, verificación, revisiones directa e independiente, nc1 a nc3 y cierre. Como referencia, PB-TC se emitió y se cerró en una noche (F4B-E07 a F4B-E08), con 13 rutas y 10 métodos de prueba. PB-TC2 sería más pequeña: unas 4 a 6 rutas y 4 a 6 métodos |

---

## 6. Lo que no pude medir

- **Nada se ejecutó.** Los efectos son lectura de código y de la evidencia custodiada: las entregas, los TRX citados en los cierres y los inventarios resumidos del bloque M.
- **Los 22 sobres reales.** Los resúmenes custodiados no recogen los ids de módulo, el poste, las vigas, el hueco, las longitudes de módulo ni los avanzados. Por eso no sé:
  - si algún rack real es como `pb-21-cola`;
  - si alguno tiene un poste fuera del catálogo;
  - si alguno trae más de 3 decimales en los campos de H-31.

  La sesión lo puede medir sin AutoCAD sobre los inventarios crudos de su carpeta local de copias (`<carpeta-de-copias-de-PB-T0>/inv/`).
- **D2 tiene un rack con `SideB` presente** (`comp=True`, id corto `f6fa1734`, 8 frentes, 15 módulos, `Name` vacío; `EV/F4B-PBT0-CHARACTERIZATION/block-m/inventories/M7-D2-pre.summary.txt:4-6`; `comp` = «`SideB` con `IsPresent`», `…/coordinator/pbt0-block-m-procedure.md:46`). Contradice la premisa de H-PB-14 y de D-PB-09 («no representative real A/B rack currently exists»). El análisis del bloque M no lo comenta. No sé si es un rack de producción o de pruebas: los nombres de D2 («Dinámico bloque 1», «Rack - copia») sugieren pruebas.
- **El mecanismo exacto de WPF en c-02** (selección vacía o selección anterior): el resultado, el poste por defecto, lo mide CT-17; el mecanismo es inferido. No medí si un id válido con otras mayúsculas también se pierde, aunque `SelectedValue` compara de forma ordinal y el catálogo con `OrdinalIgnoreCase`.
- **El dibujo con un poste inexistente conservado** (troquel en 0, topes sin ancla, BOM con el id crudo): por lectura. Se mide con AutoCAD.
- **La cabeza personalizada en un rack de un sentido con cola,** que se reconstruiría estándar al cargar (DRSR:135-144): por lectura. `pb-21-cola` tiene la cabeza estándar.
- **El módulo que pasa de calculado a manual en `ModuleLengthBox`:** por lectura y aritmética. No sé cuántos racks reales tienen un fondo con residuo ≥ 0,0001 al redondear a 3 decimales.
- **Que las versiones del 23 de agosto al 1 de septiembre de 2026 guardaran colas en racks de un sentido:** se deduce del mensaje de `4f52bc57`; no está medido.
- **La cobertura real de los controles de D-5 por las suites anteriores a PB-T0:** solo conté los archivos que los nombran.
- **Si un diseño concreto de PB-T1 mantiene en verde S-9, CT-22 y CT-25:** depende del diseño, que todavía no existe.
- **El coste en horas de PB-TC2:** estimado por analogía con PB-TC.
