# I-64 F4B — Revisión adversarial independiente del paquete v2 de «Push Back completo»

## 1. Objeto, método y límites

**Objeto.** El paquete `I-64-push-back-completo-package-v2.md` (citado como `P2:n`, la línea n), frente a:
- la orden del Owner y del Coordinator `owner-f4b-disposition.md` (citada como `OD:n`, con la numeración del archivo);
- la v1 (`P1:n`);
- los mapas de hechos MA (`map-a-classic-write-path.md`) y MB (`map-b-editor-state-testability.md`), citados por su `H-nnn`;
- el código y los documentos de la rama `architecture/workspace-persistente-rackcad` en `5ccaa770`.

**Método.** Todo en solo lectura:
- `git -C` con `rev-parse`, `log`, `show`, `diff`, `grep` y `ls-files`, y lectura de archivos;
- un script de Python de solo lectura en esta misma carpeta (`check_literal.py`). Compara carácter a carácter cada bloque «Texto del Owner (literal)» y cada cita de una línea de la tabla del §5 con la OD.

Se comprobó que HEAD es `5ccaa770577a…` y que el árbol de trabajo sigue limpio. No se compiló, no se ejecutó ninguna prueba y no se abrió AutoCAD.

**Qué no se pudo verificar:**
- el efecto real en AutoCAD: pasos de Deshacer, aspecto de las cotas y capas;
- si los dibujos reales llevan un `DimensionStyle` guardado en sus racks Push Back. Determina si el hallazgo R-I-04 es material, y solo lo puede medir PB-T0;
- que la transcripción de la OD sea completa respecto al chat del Owner. Se toma como dice su cabecera (`OD:3`);
- los mapas enteros. Se comprobó una muestra amplia (unas 110 anclas, §2.8), no cada línea;
- el recuento de T1B, que se comprobó solo contando atributos `[Fact]`, `[Theory]` e `[InlineData]` en los siete archivos.

## 2. Puntos de ataque

### 2.1 Fidelidad a la orden

**Textos literales.** El script encuentra los 12 bloques «Texto del Owner (literal)» del paquete. Los 12 coinciden carácter a carácter con la OD, con 0 discrepancias:

| Bloque | Paquete | OD |
|---|---|---|
| D-PB-01 | P2:125-152 | OD:22-49 |
| D-PB-02 | P2:171-179 | OD:13-21 |
| D-PB-03 | P2:190-201 | OD:50-61 |
| D-PB-04 | P2:211-231 | OD:75-95 |
| D-PB-05 | P2:242-275 | OD:96-129 |
| D-PB-06 | P2:300-325 | OD:130-155 |
| D-PB-08 | P2:351-363 | OD:62-74 |
| D-PB-09 | P2:375-383 | OD:224-232 |
| D-PB-10 | P2:393-396 | OD:233-236 |
| PB-T0 | P2:450-471 | OD:156-177 |
| T1B | P2:602-608 | OD:237-243 |
| RACKEDITAR | P2:622-626 | OD:244-248 |

No hay espacios finales ni caracteres raros. La raya de «F4B — PUSH BACK COMPLETO» es la misma U+2014 en los dos archivos.

Las citas de una línea del §5 (P2:430-435) también coinciden con la OD, con la normalización que declara P2:426. Son las de PB-T0, PB-T1, PB-T2, PB-T3 y PB-T4. Las citas del §0 (P2:32-34) y del §3 D-PB-07 (P2:340-341) también coinciden.

**Lo que queda fuera de los bloques literales** es solo la cabecera (`OD:7-12`), las instrucciones (`OD:249-268`) y el estado de las fases (`OD:269-297`). El paquete recoge el estado en P2:36-45 y las instrucciones en P2:653-658.

**El reparto de los bloques entre D-PB-01 y D-PB-10 es una lectura de la sesión.** La OD solo numera D-PB-06 (`OD:241`, `OD:246`) y D-PB-08 (`OD:72`). El reparto es plausible, pero el paquete no lo declara como lectura (R-M-01).

**Obligaciones que amplían o debilitan el texto:**
- **OB-05.3** (P2:293) dice «Nunca se presenta un diff JSON genérico». La OD solo prohíbe exigirlo como experiencia de usuario (`OD:129`). Es una ampliación (R-M-02).
- **OB-01.5** (P2:159) dice que CLASSIC no cambia, sin las excepciones D-PB-06 y D-PB-08 que el mismo paquete autoriza (P2:628) (R-M-03).
- **OB-01.6** (P2:160-165) presenta la ocultación de Insertar «como permite [OD]». La OD solo permite **cambiar el rótulo** (`OD:47-49`). Ocultar es una consecuencia defendible de «ZERO authored DWG writes» (`OD:31`, `OD:45-46`), pero hay que atribuirla a eso (R-M-03).
- **OB-T1B.2** (P2:612) incluye los rótulos de OB-01.6 entre los cambios que pueden sustituir T1B. Eso **debilita** la condición de la OD «RACKEDITAR mode remains unchanged» (`OD:48-49`) y contradice OB-01.6, que en ese caso manda volver al Owner (P2:165) (R-I-08).
- **OB-05.2** (P2:282) afirma que «`ExtensionData` desconocida se conserva (I-11)». Eso no es cierto en los niveles anidados (R-I-10).

**Apartados de la orden.** Ninguno se omite del todo:
- el modo explícito está en el §4, pero su calidad es insuficiente (R-I-07);
- «ZERO authored DWG writes» está en OB-01.2;
- «Do not admit A/B…» está en OB-03.1 y OB-03.2;
- los siete pasos de la reconciliación están en OB-05, con un reparto defectuoso (R-I-09);
- el «generic JSON diff» está en OB-05.3;
- los seis pasos de D-PB-06 están en OB-06, con el paso 4 partido (R-M-06);
- la lista de PB-T0 está en el §6.2, con 15 filas;
- el aviso de T1B está en el §7.3;
- RACKEDITAR está en el §7.4;
- la autorización condicional está en el §10.

**Nota sobre el encargo.** El encargo habla de «catorce puntos» de PB-T0. La OD lista **quince** (`OD:161-175`), y el paquete dice quince correctamente (P2:483).

### 2.2 D-PB-07

**¿Es correcto dejarla abierta?** Sí. La OD no trae texto, y el paquete lo declara (P2:337).

**¿Ya la decide algo de la OD?** En parte:
- «ZERO authored DWG writes» y «Do not let a Workspace invocation accidentally execute the classic drawing-write path» (`OD:31`, `OD:45-46`) deciden **Insertar y el lote de vistas**, y el paquete lo ve (P2:340);
- nada decide **BOM ni «Guardar en biblioteca»**. Ninguno de los dos escribe en el DWG (MB H-098 y H-099).

«Opens the existing Push Back editor» (`OD:25`) es compatible con la recomendación de la v1 (P1:73), pero no la ratifica. No decidir por analogía (OB-07.1) es correcto.

**¿Bloquea PB-T0?** No. PB-T0 no fija ninguna superficie del modo WORKSPACE.

**¿Hay que volver al Owner antes de PB-T0?** No. Sí hay que hacerlo antes del contrato de PB-T2, como dice P2:344.

**Lo que le falta al paquete:**
- la orden pide «the exact text of ALL ten D-PB decisions» (`OD:252`) y solo hay nueve textos. El paquete debe transcribir literal la D-PB-07 de la v1 (P1:73) y registrar esa diferencia de recuento;
- a la pregunta al Owner le falta un dato: «Guardar en biblioteca» ejecuta `EnsureId` y `SetName` sobre la identidad de la sesión (`src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3463-3464`). En modo WORKSPACE, esa identidad es la del borrador.

Todo esto va en R-M-01.

### 2.3 Los dos modos (§4)

**¿Garantiza el diseño que el modo WORKSPACE no llega al camino clásico?** No tal como está escrito. Hay tres razones.

**Primera: la frase de P2:422 no protege nada.** P2:422 dice que la ventana en WORKSPACE «no recibe el comando clásico ni sus servicios de dibujo». Eso también es cierto en CLASSIC: la ventana nunca escribe en el DWG, solo produce una petición (MB H-136; `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3323-3379`). El vector real es otro:
- la ventana anota una petición clásica: `InsertRequested`, `InsertionRequest` y `UpdateOnly`, por `RequestDraw`, `InsertBatch_Click`, `RequestUpdate` y `RequestInsert`;
- un abridor clásico la consume. Por ejemplo, `PushBackEditorModule` devuelve `window.InsertRequested ? window.InsertionRequest : null` a `RackMenuCommands`, que escribe. Lo fija `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs:302-319`.

Ocultar los botones no hace inalcanzable `RequestDraw`. Hace falta una prueba **ejecutable**: en modo WORKSPACE ningún gesto deja `InsertRequested` en `true` ni una `InsertionRequest`.

**Segunda: el paquete no declara su lectura de «explícito».** El modo CLASSIC tiene que ser el valor por defecto. Las guardas de texto de T1B fijan el literal de la construcción:
- `new RackPushBackSystemWindow(canInsertInAutoCad: true);` (`tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs:200`);
- `new RackPushBackSystemWindow(context.CanInsertInAutoCad) { Owner = context.Owner };` (`tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs:311`).

Por eso P2:421 dice «sin tocar sus llamadas». La redacción de P2:420 («un valor explícito que no tiene valor por defecto en el lado WORKSPACE») es ambigua. El paquete debe declarar su lectura:
- CLASSIC es el valor por defecto por T1B;
- a WORKSPACE solo se entra con un argumento o una factoría tipados que solo puede dar el abridor del Workspace;
- el modo es inmutable.

**Tercera: la guarda está en la tarea equivocada.** La guarda de OB-01.3 se pone en PB-T2 (P2:157), pero el abridor del Workspace no existe hasta PB-T4. Falta la guarda del lado del abridor, con su negativa. Tiene que prohibir:
- `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution` y `DrawPushBackView`;
- la lectura de `InsertionRequest`.

Las tres razones van en R-I-07.

**Los rótulos y la ocultación frente a T1B en CLASSIC:**
- son compatibles si se implementan solo en WORKSPACE. T1B no fija el `Content` de `UpdateButton`: no hay ninguna aserción sobre él, y `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml:762` lo declara como texto estático;
- hay que evitar los tokens que prohíbe `PushBackClassicCharacterizationSourceGuardTests.cs:240-247` (`.Focus(`, `DialogResult`, `TabIndex`…).

El botón «Insertar» se llama en realidad «Insertar vista actual» (`…/RackPushBackSystemWindow.xaml:784`). La declaración existe, pero mal atribuida (R-M-03).

### 2.4 La descomposición (§5)

**¿Es coherente PB-TC antes de PB-T1?** Sí. La OD prohíbe esconder las correcciones en PB-T1 (`OD:152`) y no dice dónde van.

**¿Es una decisión material?** No. Es de procedimiento. Pero «por orden de D-PB-06» (P2:45) lo exagera: la tarea separada es una elección del paquete. Además, PB-TC no está en la lista de estados de la OD (`OD:280-291`) y añade un presupuesto de 1/3. Hay que declararlo en F4B-E01 (R-M-03).

**¿Es cierta la razón del redondeo?** Sí:
- `NumericField.SetNumber` escribe con «0.###» y `Value` se vuelve a analizar del texto (`src/RackCad.UI/Controls/NumericField.cs:131-136`, `:147-152`);
- `ReadInputs` lee los controles en cada pasada (`…/RackPushBackSystemWindow.xaml.cs:812-849`).

Con «no WPF control becomes source of truth» (`OD:186`), la pérdida desaparece sola, y PB-T1 la estaría corrigiendo sin declararlo.

**El mismo argumento vale para (c) y (d), y el paquete no lo ve.** Las sustituciones de los controles y la reescritura de la celda desde ellos dependen de que el control sea la fuente. OB-06.5 las declara no autorizadas (P2:333). P2:500 además admite fijar «tal cual» cualquier clase nueva de pérdida. PB-T1 tendría que fingirlas, que es lo que el paquete rechaza para (a), o cambiar RACKEDITAR sin autorización. Falta la ruta (R-I-06).

**¿Puede PB-TC probar el punto 4 de D-PB-06 en el Workspace?** Solo de forma vacía. El piloto no pasa por la ventana (MB H-049; `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs:150-175`) y ya conserva los dos valores. La prueba con sentido es la de PB-T2. El registro de PB-TC tiene que decir que el paso 4 del lado Workspace se cierra en PB-T2 (R-M-06).

**Estados intermedios inseguros:**
- **PB-T3 frente a PB-T4.** PB-T3 generaliza `KeepPilotEditsOn` a «gana el diseño completo» (P2:279-285). La presentación informada y la elección explícita (pasos 2 y 3, OB-05.4) llegan en PB-T4. Al cerrar PB-T3, la rama aplicaría «Conservar mi borrador» con la semántica nueva detrás de la interfaz de conflicto del piloto. Eso choca con «after explicit conflict presentation» (`OD:100-101`) y con «informed destructive choice» (`OD:120`) (R-I-09).
- **El rechazo de A/B.** Lo mantiene correctamente OB-03.1 (P2:204).
- **La escritura sin barrera.** La barrera de diagnósticos va en PB-T3. Su criterio, `CompositeHasBlocking`, vive hoy en la ventana (`src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs:778-782`). Falta asignar su extracción y la dependencia (R-M-07).

**Dependencias que faltan:**
- PB-T3 necesita el criterio de bloqueo extraído;
- PB-T4 necesita la guarda del abridor (R-I-07);
- PB-T2 y PB-T3 van en paralelo sobre la misma rama, sin regla de orden de commits (R-M-14).

### 2.5 D-PB-05

**¿Se puede derivar del código el mapa de propiedad?** En lo esencial, sí.

**Identidad.** Es el GUID, no el nombre. El código lo dice: `src/RackCad.UI/Editor/RackEditorIdentity.cs:27` («the name is not stable identity — the GUID is»). `Name` es del editor (`NameBox`; «rename» en PB-T3, `OD:209`). La consecuencia hay que escribirla: con «Conservar mi borrador», un renombrado externo se revierte, después de verse en la presentación. Es la lectura literal de `OD:100-101` (R-M-04).

**Esquema y metadata de origen.** Hoy salen de la relectura:
- `WithSourceMetadataFrom(preflight.ResolvedSources[index])` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:421`; `src/RackCad.Application/Persistence/RackProject.cs:75-79`);
- `SchemaVersionPolicy.ResolveWriteVersion(source?.SchemaVersion, …)` (`src/RackCad.Application/Persistence/PushBackDesignDocument.cs:92-98`).

**Sobre.** Se compone sobre el sobre releído, con `source.Id` y `source.Name` (`PushBackPilotUpdatePlan.cs:434-438`). Para renombrar, PB-T3 tendrá que tomar `Name` del borrador y conservar el resto.

**`ExtensionData` desconocida.** Solo existe en estos niveles:
- `RackProjectDocument` (`src/RackCad.Application/Persistence/RackProjectDocument.cs:51`);
- `PushBackDesignDocument` (`…/PushBackDesignDocument.cs:75`);
- `RackEmbedDocument` (`src/RackCad.Application/Persistence/RackEmbedDocument.cs:74`).

`DynamicRackSystemDocument`, `PushBackSideDocument`, `PushBackCompositeDocument` y `PushBackFrontDocument` no tienen `[JsonExtensionData]`: el recuento de `ExtensionData` en `DynamicRackSystemDocument.cs` da 0. Así que hoy toda reescritura, clásica o del panel, **destruye en silencio** los campos desconocidos anidados. El paso 6 (`OD:110`) no se cumple en esos niveles, y P2:282 afirma lo contrario (R-I-10).

**Sesión.** `ActiveSide`, `ActiveSelection`, `frontalSide` y la cola dormida son contexto que no se persiste (MB H-012, H-021; «dormir no persiste»). No vuelven atrás porque no se escriben.

**Lo que hacen hoy `ComposeForWrite` y `KeepPilotEditsOn`:**
- `ComposeForWrite` rechaza todo lo que no sea la escala y vuelve a materializar desde el origen (`PushBackPilotMaterializer.cs:212-227`);
- `KeepPilotEditsOn` es «base nueva + mi escala» (`:242-251`).

**¿Queda algún paso sin dueño?**
- el paso 3 (OB-05.4) no tiene tarea asignada;
- OB-05.2 pide en PB-T3 «una prueba por cada uno de los siete pasos» (P2:285), pero los pasos 2 y 3 son interfaz de PB-T4;
- el paso 1 existe ya: es la relectura de PREPARE (`PushBackPilotUpdatePlan.cs:320-326`), pero no se nombra.

Todo esto va en R-I-09.

### 2.6 PB-T0 (§6)

**Cobertura.** Las 15 filas del §6.2 (P2:500-514) cubren los 15 puntos de la OD. Los nueve de vistas los cubren M-1, M-4 y M-6 (P2:539-544).

**¿Puede medir cada método lo que promete?**
- **«Borrado de vistas»** (P2:507) parte de un hecho falso: H-PB-17 dice que los cortes laterales no dependen del lado B (P2:94; MA H-061). En un rack compuesto, una ranura está activa si lo está en cualquiera de los dos lados (`src/RackCad.Application/Systems/PushBack/PushBackCompositeStructure.cs:397-400`). Los cortes se calculan sobre esa estructura compuesta (`src/RackCad.Application/Systems/PushBack/PushBackResolver.cs:63-71` y `src/RackCad.Application/Systems/PushBack/PushBackSystemLateralBuilder.cs:100-110`). Por tanto, quitar el lado B o dejar en blanco ranuras de B puede volver obsoletas vistas laterales, que el clásico borra. Ni el bloque C ni M-2 ni M-4 lo cubren (R-I-03).
- **«Pérdida del estilo de cotas»** dice «Nada nuevo» (P2:512). Pero corregirla cambia el dibujo:
  - `design.DimensionStyle` pasa a `system.DimensionStyle` (`src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:246`);
  - de ahí a `DimensionStyleName` (`src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs:117`, `:402`, usado por el corte lateral en `PushBackSystemLateralBuilder.cs:213`);
  - y `LateralHeaderDrawer` respeta el estilo nombrado «as-is» en lugar del estilo actual con las sobrescrituras escaladas (`src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:353-375`, `:381-392`).
  
  Es exactamente el disparador de `OD:153-155`. PB-T0 debe medirlo antes de PB-TC: una prueba Core del plan y M sobre una copia con el estilo presente (R-I-04).
- **Las mediciones M-1, M-5 y M-6** prometen ver qué definiciones cambian, si cada `U` repone el contenido y el Xrecord de la vista nueva tras una edición. El inventario declarado (P2:560: GUID, `Name`, vista, sección y referencias) no ve el contenido del sobre ni el de la definición. Smoke-2 ya leyó longitudes y campos del sobre (`docs/automation/evidence/I-64-pilot/F4-SMOKE-2/run/run-log-2.md:154`), así que se puede hacer con un LISP de solo lectura (R-I-11).

**¿Es segura y está bien definida la parte física?**
- **Quién opera.** El paquete decide que opera la sesión (P2:553). En F4, eso fue una orden puntual del Owner, desde el paso 30 de la corrida 2 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-smoke2-session-operator.md:5-9`). La regla vigente era que opera el Owner presente (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:2561`), y la pregunta P-03 era del Owner (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4c/map-4-t1b-seams-baseline.md:514`). «Un error del operador se repite sin gastar intento, como en Smoke-2» (P2:577) también es analogía. Es una decisión sin autoridad (R-I-02).
- **El DLL.** Existe. El diff de `src/` y de `assets/` entre `9abc1320` (el producto del DLL, `docs/automation/evidence/I-64-pilot/F4-SMOKE-2/prep/dll-c2.md`) y `5ccaa770` está vacío. Pero el paquete no exige comprobar los SHA-256 registrados de los cuatro DLL ni la carpeta `catalogs`, y cita una ruta absoluta local, que el registro del repo redacta con marcadores (R-M-09).
- **Lo que está bien:** las copias, los originales sin abrir, guardar solo la copia, las reglas de tecleo y las capturas abiertas y revisadas.
- **Lo que falta** (R-M-09):
  - nombrar los dibujos originales y la selección;
  - el procedimiento de carga del DLL sin tocar SECURELOAD;
  - en M-4, insertar antes las vistas frontales del lado B;
  - en M-2, un dibujo concreto para la regla del último vínculo.

**Qué se fija con pruebas y qué solo se describe:**

| Fijado con pruebas | Solo descrito o físico |
|---|---|
| Deltas de carga y de la segunda pasada | Creación (T1B y M-6) |
| Módulos | Actualizar frente a cerrar (T1B) |
| Cola | Estilo de cotas (lo ya existente) |
| A/B | Escrituras (guardas de texto y M) |
| Frentes y sus cortes | Catálogos distintos del entregado |
| Módulos por reconstrucción y H-PB-28 | BOM, lote y guardar (solo texto) |
| Renombrar | Deshacer (M-5) |
| La parte pura del borrado | |
| Lo que devuelven los subdiálogos con costura | |
| Las sustituciones (c) con el catálogo entregado | |
| El redondeo, campo a campo | |

**¿Basta para PB-T1, PB-T2 y PB-T3?** No del todo:
- faltan los cortes por el lado B (R-I-03);
- falta el efecto de (b) aguas abajo (R-I-04);
- falta la ruta de (c) y (d) (R-I-06);
- falta un censo control → prueba de los gestos de MB H-029 a H-031 (R-M-13);
- las pruebas de PB-T0 que fijen (a) y (b) las tendrá que sustituir PB-TC enseguida. Hay que aislarlas para no retirar la red de PB-T1 (R-M-12).

**¿Es viable el RED que propone?** Sí. Los mutantes revertidos son el precedente de T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md:523-539`, INV-F4T1B-02). El contrato tiene que fijar el `MutantScope` y las reglas de las guardas ajenas al filtro.

**¿Choca con T1B o con alguna guarda?** No:
- no se tocan fixtures en `Fixtures/I64` (corpus de `WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:323-332`);
- `EditorDiscardPromptCollection` está prevista.

El corpus de PB-T0 omite `pb-31-v2-modulos.json`, que existe en esa carpeta (R-M-11).

### 2.7 Reglas de proceso (§7)

**F4-E31** (`f4-dispositions.md:649-660`) y la regla de F4 que repite F4-E43 (`:1303`). La lectura del paquete es exacta: hay una replanificación reforzada única sin consumir intento, y el segundo rechazo vuelve al Owner. Pero la orden es «Regla para las siguientes tareas de F4» (`:659`). El propio F4-E31 registra que la sesión **no** aplicó la regla de T2B1 por analogía y preguntó al Owner (`:655`).

**PD-F4-01 y PD-F4-04.** Las lecturas son correctas (`:562`, `:860`). Pero sus reglas eran locales:
- «La regla vale solo para I-64/F4» (`:1008`);
- «para F4-T3B y F4-T3C, local a la subtarea» (`:1103-1107`).

Además existen PD-F4-05, PD-F4-06 y PD-F4-07, con regímenes propios (`:1361`, `:2183`). La «regla permanente» citada es literal (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-decisions-d1-d6.md:171`), pero es una «nota libre» de la decisión de F4-T4C.

**Conclusión:** P2:593-595 hereda por analogía justo lo que P2:592 dice que no se hereda (R-I-01).

**El patrón de I-39 existe tal como se describe:**
- la prueba vieja queda con `Skip`, con la constante `BaseEvidence` (`tests/RackCad.UI.Tests/BoundedEditorCharacterizationTests.cs:51`, `:141`);
- el contrato nuevo va en otra clase;
- el registro es `docs/automation/evidence/I-39C-caracterizacion-base-vs-contrato.md:9-15`.

T-SG lo anuncia incluso para la extracción, en `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs:23-25` y `:31-32`.

**¿Respeta OB-T1B.2 «Do not silently weaken the old guard»?** En la forma sí: no es silencioso. En el fondo no, por dos motivos:
- incluye los rótulos de OB-01.6;
- con H-PB-32 (P2:114) y OB-T1B.3 (P2:616), abre la sustitución de T1B para refactorizaciones que conservan el comportamiento. La OD reserva el registro sustitutorio a D-PB-06 o a otro cambio autorizado, y para lo conservado exige «T1B must remain green» (`OD:239-242`).

El anuncio de T-SG es anterior y de menor rango que la OD. Hay que reconciliarlo, no aplicarlo. Además, OB-T1B.1 («no se edita») choca en la letra con añadir `Skip`. Y un `Skip` sobre la Theory de WLC retira 14 filas, de las que solo 3 cambian (R-I-08, R-M-12).

### 2.8 Hallazgos (§2) y mapas: anclas comprobadas

Se comprobaron unas 110 anclas en estos archivos:
- `RackPushBackCommands.cs:147-384`, con todas las de H-PB-16 a H-PB-20;
- `SystemBlockWriter.cs:67-138`;
- `RackBlockRenamer.cs:21-58`;
- `PushBackSystemLateralBuilder.cs:91-114`;
- `RackCommandSupport.cs:117`, `:146`, `:183`, `:218`, `:258`, `:277`;
- `RackPilotUpdateWriter.cs:152-301`;
- `PushBackPilotUpdateRun.cs:163`, `:181`, `:470`, `:536`;
- `PushBackPilotUpdatePlan.cs:293-438`;
- `PushBackPilotMaterializer.cs:100-251`;
- `PushBackPilotStateComparer.cs:53-149`;
- `NumericField.cs:131-163`;
- unas 60 líneas de la ventana, entre `:44` y `:3527`;
- `RackPushBackSystemWindow.Composite.cs:802-845`;
- `PushBackCompositeEditorState.cs`, `PushBackEditorDesignAssembler.cs`, `PushBackCompositeEditorAssembler.cs` y `PushBackEditorState.cs`, en las líneas que cita MB;
- las pruebas `PushBackBlankFrontAuthorityTests.cs:86-103`, `PushBackModuleEditorWindowTests.cs:279-302`, `PushBackDimensionViewsWindowTests.cs:307-322`, la WLC `:183-252` y `:323-332`, `WorkspacePilotUpdateIntegrationGuardTests.cs:721-786` y `PushBackViewTests.cs:152`;
- la Proposal V5 `:75`, `:80`, `:393-397`, `:410`, `:438`, `:500`, `:557-593`, `:747`, `:767`, `:783`, `:803-805` y `:812`;
- ADR-0029 D7 a D13 (`:119-180`);
- IC-03 e IC-06 a IC-10.

Todas son exactas. El recuento de T1B (55 casos de UI y 10 de Core) es coherente con los atributos, y ninguna lleva `Skip`.

**Hechos falsos que fundan una decisión:**
- **MA H-061 y H-PB-17.** «No de niveles, módulos ni lado B» es falso para los racks compuestos (R-I-03).
- **P2:282.** La conservación de `ExtensionData` no es cierta en los niveles anidados (R-I-10).

**Exageraciones:**
- H-PB-25 (P2:107) dice que lo de A/B y la cola «es lo primero que PB-T1 rompería sin que se viera». Ya hay idas y vueltas compuestas por la ventana (`tests/RackCad.UI.Tests/PushBackCompositeWindowTests.cs:1606`; `tests/RackCad.UI.Tests/PushBackCompositeLoadExistingModulesTests.cs:105-142`) (R-M-13).
- La v1 sugería que el Actualizar clásico creaba vistas (P1:37). La v2 lo corrige con H-PB-16, que es cierto (`RackPushBackCommands.cs:369-374`). Pero la D-PB-04 del Owner se decidió con el marco de la v1, y el cambio no se declara (R-M-05).

### 2.9 Riesgos (§8) y cierre (§9)

**Riesgos materiales que faltan:**
- (b) cambia las cotas dibujadas (R-I-04);
- corregir el redondeo en el control compartido cambia Cantilever. `NumericField.SetNumber` se usa 33 veces en las ventanas de Cantilever (R-I-05);
- el lado B y los cortes laterales (R-I-03);
- la paridad del frontal B: el panel lo rechaza por disponibilidad (`src/RackCad.Application/Systems/Shared/RackViewAvailability.cs:112-113`; `PushBackPilotUpdatePlan.cs:400-401`) y el clásico lo redibuja (`RackPushBackCommands.cs:286-319`) (R-M-08);
- la `ExtensionData` anidada (R-I-10);
- la semántica intermedia de «Conservar mi borrador» (R-I-09);
- PB-T2 y PB-T3 en paralelo sobre una rama;
- K = 1 y el texto pendiente del panel al abrir «Editar todo».

Los dos últimos van en R-M-14.

**Lo que falta en el cierre** (R-M-14):
- «T1B … sustituida con registro» (P2:646) debe limitarse a los cambios autorizados;
- falta que Smoke-PB verifique en RACKEDITAR las correcciones D-PB-06 y D-PB-08;
- se perdió la obligación de la v1 sobre la redacción de IC-06 (P1:101).

### 2.10 Vuelta al Owner

**Antes de PB-T0: nada de producto.** Las dos cuestiones de procedimiento (R-I-01 y R-I-02) se resuelven sin Owner si la v3 adopta el valor por defecto conservador:
- en F4B no hay replanificación gratuita ni sustitutos sin orden del Owner;
- el bloque M lo opera el Owner presente, salvo orden expresa suya.

Si la sesión quiere otra cosa, es una pregunta de procedimiento.

**Decisiones de producto que la revisión hace aparecer, todas posteriores a PB-T0:**
1. **Antes de emitir PB-TC:** si PB-T0 confirma que corregir (b) cambia las cotas en RACKEDITAR, se aplica el disparador de `OD:153-155`. Lo mismo para (a), con un criterio de «materially» definido.
2. **Antes de emitir PB-T1:** si PB-T0 encuentra un caso alcanzable de (c), de (d) o de una clase nueva, decidir si se autoriza su corrección o su reproducción en el estado (R-I-06).
3. **Antes de emitir PB-T3:** si basta para el paso 6 la limitación declarada de la `ExtensionData` anidada, que es preexistente y la misma del clásico. Si no basta, sería un cambio de persistencia que afecta a todos los sistemas (R-I-10).
4. **Antes de PB-T2:** D-PB-07, ya prevista.

**Solo informativo:**
- la corrección del marco de D-PB-04 (R-M-05);
- la tarea PB-TC añadida (R-M-03).

## 3. Tabla de hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| R-I-01 | IMPORTANTE | El §7.2 hereda para F4B, por analogía, la replanificación sin intento (F4-E31) y los sustitutos PD-F4-01 y PD-F4-04. Eran reglas de F4 o de subtarea. Contradice el propio P2:592 y el precedente de F4-E31. La lista de firmas «autorizadas» está incompleta (PD-F4-05 a 07) | P2:592-596; `f4-dispositions.md:655`, `:659`, `:1008`, `:1103-1107`, `:1361`, `:2183`; `owner-f4t4c-decisions-d1-d6.md:171` | En F4B no hay replanificación gratuita ni sustitutos: un rechazo de planificación o un control fallido vuelve al Owner, salvo que el Owner autorice expresamente extender esas reglas. Quitar «regla permanente» o citarla como nota de F4-T4C |
| R-I-02 | IMPORTANTE | El bloque M lo opera «la sesión, por control de pantalla, como en Smoke-2», y repite los errores del operador sin gastar intento. En F4 el operador era decisión del Owner. La sesión operó solo por una orden puntual | P2:553, P2:577; `owner-smoke2-session-operator.md:5-9`; `f4-dispositions.md:2561`; `map-4-t1b-seams-baseline.md:514` | El bloque M lo opera el Owner presente, salvo orden expresa del Owner a la sesión. Fijar un tope de repeticiones y qué es «error del operador» |
| R-I-03 | IMPORTANTE | **Hecho falso:** los cortes laterales sí dependen del lado B en un rack compuesto (ranura activa en A o en B). Quitar B o dejar en blanco ranuras de B puede borrar vistas laterales en el clásico. El C de «Borrado de vistas», M-2 y M-4 no lo cubren, y PB-T3 copiaría un criterio mal descrito | P2:94, P2:507, P2:540-542; MA H-061; `PushBackCompositeStructure.cs:397-400`; `PushBackResolver.cs:63-71`; `PushBackSystemLateralBuilder.cs:100-110` | Corregir MA H-061 y H-PB-17. Añadir a C una prueba Core de los cortes antes y después de declarar, activar, dejar en blanco y quitar B. Añadir a M-4 las vistas laterales que se vuelven obsoletas |
| R-I-04 | IMPORTANTE | Corregir D-PB-06 (b) cambia de forma predecible las cotas dibujadas por RACKEDITAR: estilo nombrado «as-is» frente a estilo actual con sobrescrituras. Es el disparador de vuelta al Owner de la OD, pero el paquete lo trata solo como riesgo y PB-T0 «no añade nada». No hay criterio de «materially» para (a) ni para (b) | P2:512, P2:640; `OD:153-155`; `DynamicRackSystemResolver.cs:246`; `DynamicViewDecorations.cs:117`, `:402`; `LateralHeaderDrawer.cs:353-375`, `:381-392` | PB-T0 fija con una prueba Core el `DimensionStyleName` del plan con estilo nulo y con estilo nombrado. Cuenta en los dibujos reales los racks con estilo guardado. Mide en M una copia con el estilo presente. Definir el criterio de «materially». Con el resultado, volver al Owner antes de PB-TC si procede. Smoke-PB verifica (a) y (b) en RACKEDITAR |
| R-I-05 | IMPORTANTE | El lugar de la corrección del redondeo no está acotado. `NumericField.SetNumber` es un control compartido que usan las ventanas de Cantilever (33 llamadas). Corregirlo ahí cambia otros editores sin autorización | P2:331, P2:431; `NumericField.cs:131-136`; recuento de `SetNumber` en `src/RackCad.UI/Systems/Cantilever/*` | PB-TC corrige solo en la ruta de carga de Push Back y prueba que Cantilever no cambia. Tocar el control compartido es una decisión del Owner |
| R-I-06 | IMPORTANTE | La razón de PB-TC (PB-T1 no puede reproducir una pérdida que causa el control sin fingirla) vale igual para (c), para (d) y para cualquier clase nueva «fijada tal cual». Esas no están autorizadas, y PB-T1 («no WPF control becomes source of truth», «lossless») las cambiaría. No hay ruta | P2:333, P2:438, P2:500; `OD:182-188`, `OD:245-246`; MB H-083 a H-091 | PB-T0 enumera los casos alcanzables de (c), de (d) y de clases nuevas con el catálogo entregado. Regla del paquete: si hay alguno, volver al Owner antes del contrato de PB-T1, para que autorice corregirlo o reproducirlo en el estado |
| R-I-07 | IMPORTANTE | La garantía de «sin camino clásico» no apunta al vector real: la petición clásica de la ventana (`InsertRequested` y `InsertionRequest`), consumida por un abridor clásico como `EditorModules` hacia `RackMenuCommands`. No se declara que CLASSIC sea el valor por defecto por las guardas de T1B sobre el literal del constructor. La guarda del abridor falta en PB-T4 | P2:157, P2:418-422; `RackPushBackSystemWindow.xaml.cs:3323-3379`; `PushBackClassicCharacterizationSourceGuardTests.cs:302-319`; `PushBackClassicCharacterizationPluginGuardTests.cs:200` | Declarar la lectura de «explícito». WORKSPACE solo con un argumento o factoría tipados y un modo inmutable. Prueba UI-STA ejecutable: en WORKSPACE ningún gesto anota una petición clásica. Guarda de texto en PB-T4, con su negativa, contra las rutas clásicas y la lectura de `InsertionRequest` |
| R-I-08 | IMPORTANTE | La sustitución de T1B va más allá de la OD. OB-T1B.2 admite los rótulos de OB-01.6, contra `OD:48-49` y contra P2:165. H-PB-32 y OB-T1B.3 admiten sustituir guardas de T1B por refactorizaciones que conservan el comportamiento, contra `OD:239-242`. El anuncio de T-SG no se reconcilia con la OD. OB-T1B.1 choca con `Skip` | P2:114, P2:611-616; `PushBackClassicCharacterizationSourceGuardTests.cs:23-25`, `:31-32` | Quitar OB-01.6 de OB-T1B.2. Para lo conservado, T1B en verde sin sustituir nada. Si PB-T1 o PB-T2 no pueden mantener verde una guarda de T1B, volver al Owner. Redactar OB-T1B.1 como «solo se añade `Skip` con su motivo» |
| R-I-09 | IMPORTANTE | Reparto de D-PB-05: PB-T3 cambia lo que escribe «Conservar mi borrador» antes de que PB-T4 dé la presentación y la elección informadas. OB-05.4 no tiene dueño. OB-05.2 pide en PB-T3 pruebas de los pasos 2 y 3, que son de PB-T4. El paso 1 no se nombra | P2:279-294, P2:434-435; `OD:100-101`, `OD:104-106`, `OD:120`; `PushBackPilotMaterializer.cs:242-251` | PB-T3 construye la composición y el clasificador, pero deja conectada la semántica actual de «Conservar mi borrador» hasta que PB-T4 la cambie junto con la presentación. Asignar los pasos 1 a 3 y OB-05.4 a PB-T4, con su prueba |
| R-I-10 | IMPORTANTE | **Hecho falso:** «`ExtensionData` desconocida se conserva (I-11)». I-11 no es recursiva: los documentos anidados del diseño no tienen `[JsonExtensionData]`, y sus campos desconocidos se destruyen en silencio en cada reescritura, clásica o del panel. Choca con el paso 6 | P2:282; `OD:110`; `RackProjectDocument.cs:51`; `PushBackDesignDocument.cs:75`; `RackEmbedDocument.cs:74`; `DynamicRackSystemDocument.cs` sin `ExtensionData` | Declarar el alcance real del paso 6: sobre, proyecto y carga útil. Registrar la limitación anidada como preexistente e igual a la del clásico. Antes del contrato de PB-T3, preguntar al Owner si basta |
| R-I-11 | IMPORTANTE | El inventario del bloque M no puede medir lo que prometen M-1 (qué definiciones cambian), M-5 (qué repone cada `U`) y M-6 (el Xrecord de la vista nueva tras una edición) | P2:539-544, P2:560; `run-log-2.md:154` | Añadir al inventario LISP de solo lectura, por definición: la longitud y un resumen del sobre, campos clave (`Name`, `AnnotationScale`, `DimensionStyle`, número de frentes) y el número de entidades y la extensión de la definición |
| R-M-01 | MENOR | D-PB-07: falta el texto literal de la v1. No se registra que la OD anuncia «ALL ten» y trae nueve textos. El reparto de los bloques en D-PB-n no se declara como lectura. A la pregunta le falta el dato de `EnsureId` y `SetName` | P2:117-119, P2:337-345; `OD:252`; P1:73; `RackPushBackSystemWindow.xaml.cs:3463-3464` | Transcribir la D-PB-07 de la v1, registrar la diferencia de recuento y la lectura, y ampliar la pregunta |
| R-M-02 | MENOR | OB-05.3 prohíbe presentar un diff JSON genérico. La OD solo prohíbe exigirlo como experiencia de usuario | P2:293; `OD:129` | Usar la letra de la OD |
| R-M-03 | MENOR | OB-01.5 no recoge las excepciones D-PB-06 y D-PB-08. La ocultación de Insertar se atribuye al permiso de rótulos. «Por orden de D-PB-06» exagera, y PB-TC (+1/3) no está en la lista de la OD | P2:45, P2:159, P2:160-165; `OD:31`, `OD:47-49`, `OD:280-291` | Corregir las redacciones. Atribuir la ocultación a `OD:31` y `OD:45-46`. Declarar PB-TC y su presupuesto en F4B-E01 e informar al Owner |
| R-M-04 | MENOR | Al mapa de OB-05.1 le falta el criterio explícito para `Name`: es del editor, y la identidad es el GUID. Falta la consecuencia ante un renombrado externo | P2:278; `RackEditorIdentity.cs:27`; `OD:209` | Escribirlo, citando el código |
| R-M-05 | MENOR | La D-PB-04 del Owner se decidió con P1:37, que sugería creación en el Actualizar. La v2 lo corrige sin declararlo. OB-04.2 y la fila 3 del §6.4 («edición que exija crear una vista») quedan vacías con H-PB-16 o, mal leídas, hacen el panel más estricto que el clásico | P2:93, P2:235, P2:568-570; P1:37, P1:70 | Declarar la corrección del marco e informar al Owner. Definir el disparador de OB-04.2 o retirarlo |
| R-M-06 | MENOR | El paso 4 de D-PB-06 en el Workspace es vacío en PB-TC: el piloto no pasa por la ventana | P2:330, P2:440-442; MB H-049 | El registro de PB-TC marca el paso 4 (Workspace) pendiente de PB-T2, y el contrato de PB-T2 lo lleva como obligación |
| R-M-07 | MENOR | El criterio de bloqueo de salida vive en la ventana. PB-T3 lo necesita sin duplicarlo | P2:98, P2:434; `RackPushBackSystemWindow.Composite.cs:778-782` | Asignar su extracción, en PB-T1, y la dependencia de PB-T3 |
| R-M-08 | MENOR | Paridad del frontal B: el panel rechaza por disponibilidad y el clásico redibuja. El §6.4 no tiene regla para borrar un frontal B, y «sin cambiar el criterio» choca con borrarlo | P2:94-95; `RackViewAvailability.cs:112-113`; `PushBackPilotUpdatePlan.cs:400-401`; `RackPushBackCommands.cs:286-319` | Añadir al §6.4 una regla de decisión tras M-4, dentro de lo que autoriza `OD:76-77` |
| R-M-09 | MENOR | Al bloque M le faltan detalles: comprobar los SHA-256 de los DLL y de los catálogos frente a `dll-c2.md`; quitar la ruta absoluta local; el procedimiento de carga sin tocar SECURELOAD; los dibujos y la selección; «captura antes de cada clic en el panel» cuando M no usa el panel; las vistas B previas en M-4; un dibujo para el último vínculo en M-2 | P2:540-561; `docs/automation/evidence/I-64-pilot/F4-SMOKE-2/prep/dll-c2.md` | Completar el procedimiento en el contrato de PB-T0 |
| R-M-10 | MENOR | El bloque D se da por «verificado por la revisión independiente», pero esta revisión es una muestra y encontró un error | P2:479 | Rotularlo «muestra revisada» y aplicar la corrección de R-I-03 |
| R-M-11 | MENOR | El corpus de PB-T0 omite `pb-31-v2-modulos.json` (módulos), que existe en `Fixtures/I64` | P2:517; `WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:328` | Incluirlo, solo leído |
| R-M-12 | MENOR | Las pruebas de PB-T0 que fijen (a) y (b) las sustituirá PB-TC enseguida. OB-06.3b solo nombra WLC y T19_H1. Un `Skip` sobre la Theory de WLC retira las 14 filas | P2:331, P2:500, P2:513; `WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:231-252` | Aislar en clases propias las aserciones de pérdida. Exigir que el contrato equivalente cubra todas las filas |
| R-M-13 | MENOR | Falta un censo control → prueba (MB H-029 a H-031) para que PB-T1 conozca su red. H-PB-25 exagera | P2:107, P2:500 | Añadir el censo al informe de PB-T0, con la prueba existente o nueva de cada gesto |
| R-M-14 | MENOR | Riesgos y cierre: faltan PB-T2 y PB-T3 en paralelo, K = 1 con el texto pendiente al abrir «Editar todo» y la paridad del frontal B. El cierre permite «sustituida» sin límite. Smoke-PB no verifica las correcciones en RACKEDITAR. Se perdió IC-06 | P2:632-651; P1:101 | Completar el §8 y el §9 |

## 4. Veredicto

**APTO TRAS CORRECCIONES.** Recuento: **B = 0, I = 11, M = 14**.

**Por qué no es APTO:**
- la autorización condicional exige 0 bloqueantes y 0 importantes sin resolver (`OD:261-266`), y hay 11 importantes abiertos;
- dos de ellos (R-I-01 y R-I-02) meten en el propio PB-T0 decisiones de procedimiento sin autoridad;
- dos más (R-I-03 y R-I-04) cambian lo que PB-T0 tiene que medir.

**Por qué no es NO APTO:**
- los textos del Owner son exactos;
- la estructura cumple la orden;
- todo hallazgo se corrige editando el paquete, sin una decisión de producto previa a PB-T0.

**Lo que debe volver al Owner:**
- **Antes de PB-T0:** nada de producto, si la v3 adopta los valores por defecto conservadores de R-I-01 y R-I-02. Si la sesión quiere extender a F4B la replanificación o los sustitutos de F4, u operar ella el bloque M, es una pregunta de procedimiento al Owner.
- **Antes de PB-TC:** el efecto de la corrección (b), y de (a), en el dibujo de RACKEDITAR, con la medición de PB-T0. Es el disparador de `OD:153-155` (R-I-04).
- **Antes de PB-T1:** los casos alcanzables de (c), de (d) o de una clase nueva de pérdida, si PB-T0 los encuentra (R-I-06).
- **Antes de PB-T2:** D-PB-07, ya prevista. Hay que darle el texto de la v1 y el dato de `EnsureId` (R-M-01).
- **Antes de PB-T3:** si la limitación declarada de la `ExtensionData` anidada basta para el paso 6 de D-PB-05 (R-I-10).
- **Solo informativo:** la corrección del marco de D-PB-04 (R-M-05) y la tarea PB-TC añadida, con su presupuesto (R-M-03).
