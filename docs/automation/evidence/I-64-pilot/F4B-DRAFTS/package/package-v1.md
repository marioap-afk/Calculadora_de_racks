# I-64 — Push Back completo en el panel: paquete de la fase (v1, borrador para el Owner)

La sesión lo prepara por orden del Owner («Sí, prepara el paquete de Push Back completo», 2026-11-17), después del cierre de F4 (`5ccaa770`, F4-E81).
- Todos los hallazgos se midieron en la punta `5ccaa770`, en solo lectura.
- **No emite contratos.** Esta v1 sirve para que el Owner decida el §3 y el §4. Después vendrán una revisión adversarial independiente (v2), como en F4, y la emisión de la primera tarea.

## 0. Qué es «Push Back completo»

Hoy el panel edita **un solo campo** de Push Back: la escala de anotación (D-T2-01). Rechaza los racks A/B y los que aparcan una cola compuesta («fuera del piloto (F5)»). Todo lo demás se hace en RACKEDITAR. IC-06 lo dejó escrito: el piloto «No es la migración completa del editor de Push Back».

**Objetivo de la fase:** que todo lo que hoy se edita de un rack Push Back con RACKEDITAR pueda editarse desde el panel como borrador, sin tocar el dibujo hasta «Actualizar». Ese Actualizar es el del panel: todo o nada, un solo paso de Deshacer, con detección de conflicto y veto al cerrar. Entra todo lo de RACKEDITAR, incluidas las ediciones estructurales y los racks A/B. RACKEDITAR sigue igual.

## 1. Autoridades vinculantes

| Autoridad | Qué fija |
|---|---|
| Proposal V5 D-15 (`I-64-proposal-v5.md:557-582`) | El contrato de adaptador por sistema: (1) superficie alojable; (2) intención separada del cierre; (3) ida y vuelta del estado sin la ventana; (4) diálogos con el dueño que decide el host; (5) escritor caller-owned. Antes, la caracterización. «No se reescribe lógica de dominio» |
| D-16 (`:586-593`) | RACKEDITAR sin cambio observable (INV-19). Si la extracción cambiara algo observable de un comando clásico: M-03 y **STOP** |
| D-08 (`:393-397`), D-10 (`:410-438`) y D-13 (`:500-546`) | K = 1 superficie viva; no hay modelo paralelo; los gestos de campo se comparten con la ventana clásica; Actualizar todo o nada y un solo paso de Deshacer |
| INV-09, 10, 15, 18, 19, 25, 28 y 29 (`:747-767`) | Los criterios por sistema de F5 (§11: «caracterización, ida y vuelta, INV-10, 18, 19, 28 y 29») |
| §11 (`:783`, `:803-805`, `:812`) | «no hay micro-gates»; A64-PV1-21: sin cierre ni integración parcial por resecuenciación, salvo decisión del Owner; caracterización antes de migrar |
| No-objetivos (`:75`, `:80`) | Insertar vistas y `BindingIntent` desde el panel siguen en el clásico; no se cambia RACKEDITAR |
| IC-03, IC-06 e IC-07 (`I-64-implementation-clarifications.md:57-61`, `:95-111`, `:118-129`) | «Abrir en editor clásico» pendiente; el piloto no es la migración; A/B, cola aparcada y autoridad de módulos como residual de F5 |
| ADR-0029 D7 a D13 (`docs/adr/0029-…md:119-180`) | Una política de cierre; dirty por ámbito; `Owner` y `CenterOwner`; foco determinista; «Crear un segundo modelo paralelo está prohibido»; caracterización primero, sin editarla para que pase; un cambio de comportamiento exige decisión del Owner |
| ADR-0019 (shell por composición) y ADR-0047 (propuesto) | El shell es un `Control` sin conocimiento del sistema; las superficies alojadas heredan D7, D8, D9, D11 y D13 |
| Cierre de F4 (`I-64-pilot/F4-DRAFTS/gate-close.md`) | Lo que ya funciona y sus limitaciones (R-1 a R-5 de C2, O-18, F4-S2-03 en Cantilever) |

## 2. Hallazgos del repositorio (medidos en `5ccaa770`)

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-PB-01 | **El núcleo de borradores ya es genérico:** `DraftSession<TState>` con comparador y copiador inyectados, texto pendiente por campo (diccionario), máquina de estados de Actualizar, Conservar o Descartar, `DraftRegistry` y veto (`Workspace/Drafts/*`) | Se reutiliza tal cual |
| H-PB-02 | **El host, el puerto y la UI están atados al campo único:** `WorkspaceHostCore` siempre usa la clave `AnnotationScale` (`:1136`) y `Find<PushBackPilotDraftState>` en diez sitios; `IWorkspacePilotEditPort.SetPendingText` y `CommitField` no llevan clave de campo (`:64`, `:71`); `WorkspacePilotFieldViewModel` (1266 líneas) une el ciclo de vida con un solo `Text` | Separar el ciclo de vida del campo y añadir al puerto «reemplazar el estado actual» |
| H-PB-03 | **El copiador del borrador pierde el estado de módulos y de la cola aparcada** (`PushBackPilotStateComparer.Copy :58-83`), y `BuildDesign` depende de ese estado (`PushBackEditorDesignAssembler.cs:198-213`, `PushBackCompositeEditorAssembler.cs:64-202`). La carga de un compuesto siempre aparca su cola (`…Composite.cs:835-843`) | **El bloqueo principal.** Las ediciones estructurales y los A/B no funcionan sin resolverlo. A/B y cola aparcada van **juntos** |
| H-PB-04 | **La composición para escribir solo admite la escala:** `ComposeForWrite` rechaza cualquier otro cambio (`:210-215`), y `KeepPilotEditsOn` es «base nueva + mi escala» (`:242-251`) | Generalizar a «estado completo» |
| H-PB-05 | **La escritura del panel ya redibuja todas las vistas desde el diseño completo,** también los cortes del lado B: `RackPilotUpdateWriter.cs:118`, `:129-135`; los servicios de planta, lateral y frontal; `RedefineSystemBlock`. Es una sola transacción y un solo paso de Deshacer, y no tiene nada propio de la escala | Se reutiliza. Que una edición estructural que conserva las vistas se redibuja bien es **hipótesis** sin prueba física |
| H-PB-06 | **A la escritura del panel le falta:** (a) **borrar** vistas que sobran (el clásico borra cortes laterales con `EraseViewBlocks`, que abre su propio bloqueo y transacción y no es caller-owned: `RackCommandSupport.cs:218-231`, `RackPushBackCommands.cs:322-356`); (b) **crear** vistas nuevas (el clásico usa `RackViewBatchExecution`, `:246-260`, `:369-373`; existe la primitiva caller-owned `RackDefinitionCreator.CreateInTransaction`); (c) **renombrar** el rack (el sobre se compone con el Id y el nombre de origen, `PushBackPilotUpdatePlan.cs:436`); (d) la **barrera de diagnósticos bloqueantes** que tiene el clásico (`RackPushBackSystemWindow.xaml.cs:688-691`); (e) `ComparePost` comprueba la escala (`PushBackPilotUpdateRun.cs:181-182`) | Trabajo de escritura nuevo (§5, PB-T3) |
| H-PB-07 | **La ventana clásica** es un `Window` cuyo contenido es `RackEditorVisualShell`, con 3630 + 911 líneas de code-behind, 792 de XAML, 139 `x:Name` y ningún ViewModel. Parte del estado authored vive **en campos de la ventana**: `safetySelections`, `generalPallet`, `advanced`, `firstLevelDatum`, `loadedDimensionViews`, `frontalSide`. Otros valores solo existen en los controles y los lee `ReadInputs` en cada recálculo (`:812-857`). `Recompute` está en el code-behind (`:603-702`) | El punto 3 de D-15 (ida y vuelta sin la ventana) **no es posible hoy**: hay que llevar ese estado a Application, sin crear un modelo paralelo |
| H-PB-08 | **Actualizar va unido al cierre:** `RequestDraw` pide `RequestUpdate` o `RequestInsert` y luego `Close()` (`:3323-3379`). La escritura la hace quien llama (`RackPushBackCommands.cs:147-384`). Excepción: «Guardar en biblioteca» escribe un archivo desde la propia ventana (`:3441-3475`) | Separar la intención del cierre (punto 2 de D-15) |
| H-PB-09 | **Hay ocho subdiálogos con `Owner = this`** (configurador de cabecera, seguridad, botas, defensas, topes traseros, BOM, lote de vistas, guardar archivo). El aviso de cierre no tiene dueño. La ventana se abre con `ShowModalWindow` sin dueño | Punto 4 de D-15: decide el host |
| H-PB-10 | **El panel no puede alojar un editor:** su superficie es un `StackPanel` fijo, sin hueco para un control; K = 1; la paleta mide 320 de inicio y 240 de mínimo, sin desplazamiento horizontal. El editor clásico pide como mínimo **1120 × 672**. `EnterPanelModal` existe pero nadie lo llama, y Actualizar se rechaza mientras hay un modal de RackCad abierto (`WorkspaceHostCore.cs:1594-1597`) | Decisión D-PB-01 (§3) |
| H-PB-11 | **La identidad de la ventana está fijada por pruebas:** el contenido es el shell; `FindName` busca 26 nombres; hay propiedades `*ForTest`. La guarda G-10 exige **una sola** `.RequestUpdate(` propia en `RackPushBackSystemWindow.xaml.cs`. Las guardas de texto de T1B buscan literales como `{ Owner = this }` en `Systems/PushBack`. La caracterización T1B son 55 pruebas de UI y 10 de Core | Se sustituyen con el patrón de I-39: la guarda vieja con `Skip` y un contrato equivalente que sí se ejecuta. **Nunca se editan para que pasen** (ADR-0029 D13) |
| H-PB-12 | **Siguen las pérdidas de la carga clásica** de F4-T2A (`ideas-futuras.md:1722-1736`): (a) redondeo a «0.###»; (b) `ReadInputs` pierde `DimensionStyle`; (c) los ajustes de `LoadFromModel`, más dos no listados (el peralte trasero fuera de 3..6 y los combos de peralte); (d) `CommitCurrentCell` reescribe la celda desde los controles en cada recálculo. `WorkspaceDraftPilotWindowLoadCharacterizationTests` solo admite (a) y (b) | Decisión D-PB-06 |
| H-PB-13 | **Paso 64 de Smoke-2 (la casilla A/B que «no se guardó»): es diseño, no un fallo de guardado.** Marcar «Rack de dos sentidos (lado B)» solo declara la *capacidad*: todos los frentes del lado B nacen en blanco (`PushBackCompositeEditorState.cs:378-385`). `BuildSideB` devuelve null sin ningún frente activo (`:1031-1047`), así que no se serializa ningún lado B y al reabrir la casilla sale desmarcada. Lo fija `PushBackBlankFrontAuthorityTests.cs:88-102`. El sobre cambió por `DefaultTopology` y `AbsentSlotsB`. **Nada avisa** de «lado B declarado sin frentes» | Ni defecto de guardado ni A/B probado. Hueco de uso: D-PB-08 |
| H-PB-14 | **No hay ningún rack A/B real en los dibujos disponibles** (S2-12 de Smoke-2: NOT AVAILABLE) | La prueba física de A/B necesita un rack fabricado: D-PB-09 |
| H-PB-15 | **La clasificación de lo estructural vive en el ensamblador, no en la ventana:** `PushBackEditorDesignAssembler.cs:204-213` y `DynamicEditorDesignAssembler.MustRebuild`. Estructural: tarima, fondos por frente o celda, número de frentes y niveles activos, frentes en blanco, «Restaurar estándar» | La vía del panel puede reutilizarla tal cual |

## 3. La decisión de fondo: dónde vive el editor completo (D-PB-01)

El editor clásico necesita como mínimo 1120 px y el panel mide 320 (H-PB-10). Por eso «editar todo Push Back desde el panel» tiene tres formas posibles:

| Opción | Qué es | A favor | En contra |
|---|---|---|---|
| **H1 — El editor completo, abierto desde el panel en su ventana** | En la pestaña del rack, el panel ofrece «Editar todo». Abre **la misma superficie del editor de Push Back** (el mismo shell y los mismos controles), cargada desde el **borrador** del panel y no desde el dibujo. Su «Actualizar» no escribe por la vía clásica: devuelve el estado al borrador y el panel lo aplica con su Actualizar (todo o nada, un paso de Deshacer, conflicto y veto). «Cerrar» devuelve el estado al borrador sin escribir nada | El usuario usa el editor que ya conoce; el panel sigue estrecho; cumple D-15 (2 a 5) y D-08 (K = 1: la ventana es la superficie viva); el cambio en la ventana se limita al estado (H-PB-07) y a separar la intención del cierre (H-PB-08) | Sigue siendo una ventana grande encima del dibujo; si es **modal**, mientras está abierta no se puede tocar el dibujo, igual que con RACKEDITAR |
| H2 — La superficie dentro del panel | El editor completo, alojado en la paleta | Es la lectura literal de D-15 (1) | La paleta tendría que medir al menos 1120 px o haría falta un editor nuevo con diseño adaptable: un cambio grande de uso, y choca con el panel acoplado y estrecho |
| H3 — Un formulario compacto propio del panel | Una segunda interfaz para los campos más usados | Encaja en 320 px | Una segunda interfaz del mismo estado: el doble de mantenimiento, riesgo de divergencia, y va contra el espíritu de D-15 («no hay seis reescrituras») y ADR-0029 D11 |

**Recomendación: H1, modal en esta fase.**
- Modal, porque encaja con la regla vigente («Actualizar se rechaza con un modal de RackCad abierto») y con las sesiones por documento (D-03): mientras está abierta no se puede cambiar de documento.
- La versión no modal queda como mejora posterior.
- H1 cubre de paso IC-03 («Abrir en editor clásico») para Push Back.

## 4. Decisiones que se piden al Owner

| Id | Decisión | Recomendación |
|---|---|---|
| D-PB-01 | La forma del editor completo (§3) | **H1, modal** |
| D-PB-02 | **El lugar en el plan.** La fase **F4B — Push Back completo** va entre F4 y F5-A, y F5-A entra con F4B PASS. Se registra como disposición del Owner y como una aclaración IC-08, sin editar la Proposal congelada. Se queda en la rama: no hay integración parcial en `main` (A64-PV1-21) | **Sí** |
| D-PB-03 | **Los racks A/B y la cola aparcada, dentro del alcance.** Van unidos (H-PB-03) | **Sí** |
| D-PB-04 | **Cambios en el conjunto de vistas** al Actualizar desde el panel: borrar las vistas que sobran y crear las que hagan falta | **Borrar, sí:** una primitiva caller-owned dentro de la transacción única, con su guarda de fachada. **Crear:** PB-T0 mide primero qué ediciones lo necesitan. Mientras tanto, el panel lo rechaza con un diagnóstico claro («Usa RACKEDITAR…»). Insertar vistas a petición del usuario sigue fuera (Proposal `:75`) |
| D-PB-05 | **Qué significa «Conservar mi borrador»** con el estado completo: hoy es «base nueva + mi escala» | **Gana mi borrador completo** sobre la base nueva, tras una confirmación que lista lo que cambió fuera del panel. Sin mezclar campo a campo, porque sería impredecible |
| D-PB-06 | **Las pérdidas de la carga clásica** (H-PB-12). Con H1 el panel y RACKEDITAR comparten la superficie, así que una corrección vale para los dos | **Corregir (b)** (`DimensionStyle`: se pierde un dato) y **(a)** (el redondeo pierde precisión en silencio): cambio funcional declarado, ADR-0029 D13. (c) y (d), medirlas primero en PB-T0 |
| D-PB-07 | **«Guardar en biblioteca», BOM y lote de vistas** dentro del editor abierto desde el panel | BOM y «Guardar en biblioteca», **sí** (no escriben en el dibujo). Lote de vistas e Insertar, **no**: siguen siendo del clásico |
| D-PB-08 | **Aviso de «lado B declarado sin ningún frente activo»** (H-PB-13), no bloqueante, en los dos caminos | **Sí** (cambio pequeño y declarado) |
| D-PB-09 | **Validación física:** un Smoke-PB dirigido, no las 88 comprobaciones. Los racks A/B se fabrican con RACKEDITAR sobre copias de trabajo, porque no hay ninguno real (H-PB-14) | **Sí** |
| D-PB-10 | **Presupuesto:** por tarea, 3 intentos por tarea, como en F4 (F4-E09 y F4-E29) | **Sí** |

## 5. Descomposición propuesta

| Tarea | Contenido | Depende de |
|---|---|---|
| **PB-T0 — Medición y caracterización** (sin producto) | (1) Medir en el clásico qué vistas borra, crea o renombra cada clase de edición estructural, incluida la conversión a A/B, y fijarlo con pruebas de caracterización. (2) Caracterizar la ida y vuelta de un compuesto A/B y de la cola aparcada. (3) Medir las pérdidas (c) y (d). (4) Preparar la sustitución de las guardas de texto de T1B con el patrón de I-39. (5) Reproducir en el host la conversión a A/B con un frente activo del lado B (H-PB-13) | — |
| **PB-T1 — El estado completo en Application** | Llevar a un estado de Application el que hoy vive en la ventana: seguridad, tarima general, avanzados, datum del primer nivel, vistas de cotas, lado frontal y los valores que solo existen en los controles (H-PB-07). La ventana pasa a usarlo: es el mismo modelo, no uno paralelo. El copiador lleva los módulos y la cola aparcada (H-PB-03); la materialización iguala a `LoadExisting`, A/B incluidos; el comparador cubre todo. T1B sigue en verde (INV-19) | PB-T0 |
| **PB-T2 — Separar la intención del cierre, y el host** | `RequestDraw` emite la intención hacia el host; el host clásico hace exactamente lo de hoy. El dueño de los subdiálogos lo decide el host. Se aplican la sustitución de I-39 y la excepción declarada de la guarda G-10 | PB-T1 |
| **PB-T3 — Escritura completa** | Generalizar el plan y la corrida (no solo la escala); borrar vistas con una primitiva caller-owned y su guarda; renombrar; barrera de diagnósticos bloqueantes; `ComparePost` genérico; las inyecciones UNIT y SEAL siguen valiendo; la creación de vistas, según D-PB-04 | PB-T1 (en paralelo con PB-T2) |
| **PB-T4 — El panel** | «Editar todo» abre el editor (H1) sobre el borrador. El puerto se generaliza: reemplazar el estado actual y varios campos. Se separan el ciclo de vida y el campo en el modelo de vista. Textos; el resumen y «Modificado» en la pestaña. Las lecturas siguen IC-04 | PB-T2 y PB-T3 |
| **PB-T5 — Cierre** | CI exacta; Smoke-PB, con A/B fabricados (D-PB-09); gate-close de F4B | PB-T4 |

Cada tarea pasa por la cadena de F4: contrato, planificación, Worker (RED y GREEN), CI exacta, verificación, revisiones directa e independiente, controles nc1 a nc3 y cierre por la regla D2.

## 6. Obligaciones e invariantes de la fase

- **INV-19 y D-16:**
  - RACKEDITAR, RPB y los demás comandos clásicos sin cambio observable, salvo los cambios funcionales que el Owner autorice expresamente (D-PB-06 y D-PB-08);
  - T1B en verde, o sustituida con equivalentes según I-39;
  - cualquier otro cambio observable es M-03 y **STOP**.
- **Sin modelo paralelo** (ADR-0029 D11): la ventana y el panel usan el mismo estado de Application.
- **D-13:** Actualizar todo o nada, un solo paso de Deshacer, conflicto antes de escribir, veto al cerrar y descarte explícito; las inyecciones Debug siguen válidas.
- **INV-09, 10, 15 (K = 1), 18, 25, 28 y 29** para Push Back completo, cada una con su prueba automatizada.
- **Escritores caller-owned:** las primitivas nuevas (borrar y, si se decide, crear) entran en `CallerOwnedFacadeGuardTests`.
- **IC-06:** no se dice «editor de Push Back migrado» hasta el cierre de F4B.

## 7. Riesgos

| Id | Riesgo | Tratamiento |
|---|---|---|
| R-PB-01 | Mover el estado de una ventana de 4500 líneas (H-PB-07) sin cambiar su comportamiento | Caracterización primero (PB-T0); pasos pequeños; T1B como red |
| R-PB-02 | La semántica de compuesto y cola aparcada (la deuda H14: «dormir no persiste») | PB-T0 la fija antes de tocar nada; en el panel, la cola aparcada vive solo en el borrador, como en el clásico |
| R-PB-03 | Borrar o crear vistas dentro de la transacción única es nuevo en el panel | Primitivas caller-owned con guarda; inyecciones UNIT y SEAL; Smoke-PB |
| R-PB-04 | No hay racks A/B reales (H-PB-14) | Racks fabricados (D-PB-09) y declarados como tales |
| R-PB-05 | El adaptador queda hecho a la medida de Push Back y no sirve en F5-A | El ciclo de vida del panel y del puerto, genérico en el tipo de estado (H-PB-01); lo específico de Push Back, en `Systems/PushBack` (ADR-0029 D12) |
| R-PB-06 | Un Actualizar grande tarda segundos (F4) | Sin cambio en esta fase; se mide en el Smoke-PB |

## 8. Requisitos de cierre de F4B

- Todas las tareas cerradas.
- T1B en verde o sustituida.
- INV del §6 cubiertas.
- CI exacta del Candidato.
- Smoke-PB PASS.
- Firma del Owner.
- Sin integración parcial en `main` (A64-PV1-21).

## 9. Siguiente

1. El Owner decide D-PB-01 a D-PB-10 (§4).
2. La sesión incorpora las decisiones (v2), pasa una revisión adversarial independiente y la incorpora.
3. Se emite el contrato de PB-T0.

Lo que queda **fuera de F4B:** los demás sistemas (F5-A y F5-B); O-18 (las lecturas del panel vacían Rehacer); F4-S2-03 en Cantilever; el rendimiento del Actualizar; Insertar vistas y `BindingIntent` desde el panel; la mejora de uso de la fila de navegación.
