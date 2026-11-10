# I-64 — F4 Borradores y Actualizar: paquete del contrato de gate (v2)

Preparado por la sesión desde las autoridades reales, por orden del Owner (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f3-ratification-f4-authorization.md`). La v1 pasó una revisión adversarial independiente (§8) y esta versión la incorpora. No emite contratos: cada tarea emite el suyo. Decisiones de la sesión en `F4-DRAFTS/f4-dispositions.md` (F4-E01..E05).

## 1. Autoridades vinculantes

| Autoridad | Qué fija |
|---|---|
| Proposal V5: D-04, D-10..D-17; INV-04..INV-12, 16..21, 25..33, 39; P-03, P-04; §11 fila F4 y secuencia de cierre; §12.1 y §12.4; OV-07, 09..13, 16, 17, 21, 23..26 | Semántica congelada |
| I-55 V5 §3.7 y §4.1..§4.10; `ISiblingRedrawPort`, `RackSiblingRedrawRun`, `SiblingRedrawTransaction`, `RackSiblingMembership`, `CallerOwnedFacadeGuardTests`, `SiblingRedrawDebugFaultInjection` | Mutación atómica y pertenencia |
| ADR-0010; ADR-0029 D9, D11, D13; ADR-0032 D5 y D6 (compromiso en dos fases), con el Anexo B de la Proposal | Coexistencia, adopción, compromiso interno |
| `IWorkspaceHostPort`, `WorkspaceHostCore`, `EventBridgeCore`, `AutoCadEventBridge`, `BrowserSession`, `BrowserSessionRegistry`, `WorkspaceNavigationHost` | Contratos vigentes del Workspace |
| IC-01 a IC-05 | IC-01 **no** cubre Actualizar; IC-04 en su redacción aceptada |
| Estados existentes de Push Back en Application: `PushBackCompositeEditorState`, `PushBackEditorState`, `PushBackEditorInputs`, `PushBackEditorSnapshot`, `PushBackEditorDesignAssembler` | Estado editable del piloto: no hay modelo paralelo (D-10, ADR-0029 D11) |

## 2. Hallazgos del repositorio

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-01 | Push Back no pasa por la costura de I-55: RACKEDITAR confirma por vista. Sus escritores caller-owned no tienen uso en producto (R-09) | La vía del panel es nueva para Push Back; primera prueba física en Smoke-2 |
| H-02 | `RackSiblingRedrawRun` y `SiblingRedrawTransaction` convierten toda excepción de `Mutate`, también la de `Commit()`, en `Discarded` | INV-30, aditiva |
| H-03 | `RackSiblingScan` abre bloqueo y transacción propios y no retiene el JSON crudo; `Section` existe en el sobre pero no en `RackSiblingScanFact` | Proyección aditiva única (§3, T2) y entrada caller-owned (T4A, INV-39) |
| H-04 | `CallerOwnedFacadeGuardTests` cubre 4 fachadas y no las 3 de Push Back | Ampliarla antes de usarlas |
| H-05 | Sin inyección Debug en la frontera de `Commit()`, en POST ni en la relectura de POST | Crearlas en Debug (§12.4: S2-10, S2-11) |
| H-06 | `survivors = 0` con borrados da `DeferToFirstPlacement` | C-F4-05 |
| H-07 | `OnBeginDocumentClose` siempre responde Allow y el manejador nunca llama a `Veto()` | Cableado real |
| H-08 | `MarkDraftsPossiblyStale` es solo un valor; las pistas no llevan rack ni handle; el drenaje ocurre en Idle | C-F4-04 |
| H-09 | Una sesión destruida solo se avisa si era la activa | Señal nueva para la pérdida visible de D-17 |
| H-10 | `RackPushBackSystemWindow`: unas 4500 líneas de code-behind, unos 140 controles con nombre, parte del authored en campos de la ventana, sin ViewModel; barra lateral de 430 frente a un panel redimensionable de 320 inicial y 240 mínimo | C-F4-01 |
| H-11 | «Frontera no bloqueable» no existe en código; `PendingTextField` solo aborta y `Write` no restaura texto como pendiente; Push Back no lo usa | Modelo de texto pendiente nuevo; captura por el host (O-16) |
| H-12 | `SetModalRackCadActive` no tiene llamador en producción | Los modales RackCad salen de comandos: se comprueba por `CommandInProgress` |
| H-13 | La guarda que prohíbe `ExecuteInCommandContextAsync` cubre **todo** `src` y nació de la parada de Smoke-NAV; `async` y `await` están prohibidos en las dos carpetas Workspace, y `Task` y `Dispatcher` en UI/Workspace | C-F4-02 reintroduce una API retirada: excepción por archivo exacto y aviso al Owner |
| H-14 | La guarda de nombres prohíbe `Draft` y `Editor` en tipos del namespace exacto `RackCad.Application.Workspace`; la de texto recorre la carpeta de forma recursiva | `RackCad.Application.Workspace.Drafts`, **con una guarda equivalente** para ese subnamespace |
| H-15 | `IWorkspaceHostPort` es de solo lectura por guarda | Puertos nuevos para Actualizar y descartar (INV-21) |
| H-16 | `PushBackDesign` no tiene igualdad; `PushBackEditorInputs` se lee hoy de los paneles de la ventana | El estado del piloto depende de C-F4-01 |

## 3. Descomposición

Tareas secuenciales, cada una cerrada antes de emitir la siguiente, con RED y GREEN exactos, CI del SHA exacto, verificación del Controller, revisión directa de la sesión, revisión adversarial independiente del diff y controles negativos (F4-E02). La mutación solo entra en T4A y T4B.

| Tarea | Capa | Responsabilidad | Escrituras authored |
|---|---|---|---|
| **F4-T1-DRAFT-CORE** | Application, puro | §6 | Ninguna |
| **F4-T1B-PILOT-CHARACTERIZATION** | Pruebas | Caracterización de `RackPushBackSystemWindow` (INV-19, ADR-0029 D13): Enter, Escape, foco inicial, tabulación, caminos de cierre, `Owner`/`CenterOwner`, recursos, intención y escrituras del puerto. **Antes de tocar el estado del piloto** | Ninguna |
| **F4-T2-DRAFT-READ-HOST** | Application y Plugin | Proyección aditiva **única** del barrido con texto crudo y `Section`; lectura lógica única de la intención con bloqueo; StaleBase con las reglas de §6; admisión (AUTH-13, Cama, sin Id); InitialDraftState del piloto sobre el estado existente; cambio de sobre entre preview e intención; «Comprobar ahora» y recarga perezosa de solo lectura; marcado de obsolescencia; veto real de `BeginDocumentClose` desde memoria; aviso de pérdida; conformidad de IC-04 con su mecanismo y su excepción de guarda declarados | Ninguna |
| **F4-T3-EDIT-UI** | UI y Plugin | Superficie del piloto según C-F4-01; primera pulsación fija la preview; texto pendiente vivo copiado al borrador **antes** de toda frontera no bloqueable, también `BeginDocumentClose`; fronteras; marca «Nombre *»; cambio de pestaña; rematerialización K = 1; Error con Recargar o Descartar (INV-25); «No editable aquí» visible (INV-31); cierre Dirty con Descartar o Cancelar; «Descartar borradores de este documento» y el comando de C-F4-03; mensaje del veto con las dos vías | Ninguna |
| **F4-T4A-MUTATION-SEAM** | Application y Plugin, sin panel | Entrada caller-owned sobre la proyección de T2 (INV-39); frontera de `Commit()` (INV-30) sin cambiar Insertar; inyección Debug en la frontera de `Commit()`; guardas de las fachadas de Push Back; unidades del piloto con su sobre por vista (INV-10); C-F4-05 | Solo en pruebas de costura |
| **F4-T4B-ACTUALIZAR-HOST** | Plugin, Application y UI | Actualizar explícito en el contexto de C-F4-02; **un único `using LockDocument`** alrededor de PREPARE, MUTATE y POST; comprobación previa; relectura y comparación dentro de MUTATE; revalidación de sesión inmediata; ámbito de mutación propia (C-F4-04); POST con relectura; inyecciones Debug de POST y de su relectura; resultados visibles (INV-31) y reconciliación sin fusión automática | **Solo aquí** |
| **F4-SMOKE-2** | Host | Procedimiento y ejecución, §7 | Sobre el Candidato |

## 4. Decisiones

| Id | Decisión | Recomendación | Quién y cuándo |
|---|---|---|---|
| **C-F4-01** | **Superficie del piloto.** D-15 pide separar la **misma** superficie de su `Window` y que la ventana clásica la envuelva, trasladando el code-behind. Con H-10 es una extracción de horizonte largo, sin celda Worker de escritura elegible. Una superficie compacta sobre el mismo estado **no cumple D-15** y la fila F4 («Push Back editable en el panel») solo la admite con un residual enumerado aceptado por el Owner (D-20, OV-07) | (a) Extracción conforme a D-15, con T1B, en varias tareas y una celda Long-horizon que hay que acreditar; o (b) **desviación del Freeze**: superficie compacta sobre el mismo estado (`PushBack*EditorState`), adoptando `NumericField`, la política de pendiente y cierre y `EditorAction`, con un conjunto enumerado de campos y el resto como residual de F5 aceptado. La sesión recomienda (b) para F4 si el Owner acepta el residual; si no, (a) | **Owner, antes de T2**: condición de devolución de la orden (capacidad de editor y decisión material) |
| **C-F4-07** | INV-31, S2-12, OV-25 y D-11 piden «No editable aquí» con el editor clásico **ofrecido**; IC-03 aplazó «Abrir en editor clásico» a F5 | Mostrar en F4 el diagnóstico y la indicación de usar RACKEDITAR, sin lanzar comandos, y conservar la obligación de F5; o adelantar el lanzamiento ligado a la sesión | **Owner, antes de T3** |
| C-F4-03 | Nombre del comando de descarte (D-17 lo reserva al Owner) | `RACKPANELDESCARTAR` | **Owner, antes de T2**, porque el veto lo nombra |
| C-F4-06 | Smoke-2: S2-12 y S2-15 exigen dibujos **del Owner**; §12.1: RELEASE de I-52, una sola `acad.exe`, DLL **Debug** con su SHA-256, inyecciones solo por variable de entorno del proceso, acusadas con I-52; el Owner ejecuta | Dibujos reales del Owner. Cualquier dispensa (representativos, operación por la sesión) se da por separado | **Owner, antes de Smoke-2** |
| C-F4-02 | **Contexto de Actualizar.** D-13 fija comando interno en contexto de comando; IC-01 no lo cubre; H-13 | `ExecuteInCommandContextAsync` como vía declarada, con excepción por archivo exacto y aviso al Owner de que se reintroduce. La medición en host (escrituras y UNDO) **va dentro de T4B**, tras cerrar T1..T3, con las reglas de §12.1. Si UNDO no es un solo paso: A-n material hacia comando registrado (D-13) | Sesión; aviso al Owner; A-n si cambia |
| C-F4-04 | **Ámbito de mutación propia** (D-04, INV-08, INV-28). El `CommandEnded` del propio Actualizar llega después de la relectura de POST, y las pistas no llevan rack | El ámbito se indexa por (sesión, miembros) y se cierra con el fin del **propio comando**, no tras POST; o la pista de objeto lleva el handle para atribuirla a miembros. Se registra como aclaración IC o A-n, porque concreta D-04 | Sesión, con IC o A-n, en T2 (marcado) y T4B (ámbito) |
| C-F4-05 | Cero vistas supervivientes (H-06) | No aplicado, con su causa visible en T4B y sin escrituras. Difiere de RACKEDITAR Push Back, que cuenta supervivientes por bloques: se declara | Sesión |

**Bloqueo de T2:** C-F4-01 y C-F4-03. T1 no depende de ellas.

## 5. Obligaciones de la orden

| N.º | Obligación | Tarea | Prueba |
|---|---|---|---|
| 1 | Dirty no es desigualdad de JSON crudo | T1, T2 | INV-26 con el piloto |
| 2 | Dirty = Current ≠ Initial, o texto pendiente | T1 | Tabla de estados |
| 3 | Una preview no crea `DraftSession` ni StaleBase | T1, T2 | El registro solo nace del resultado de la lectura |
| 4 | Una lectura lógica da StaleBase, admisión e InitialDraftState | T2 | Lector simulado: una lectura |
| 5 | Ninguna escritura antes de Actualizar | T2, T3, T4B | Guardas y puertos simulados |
| 6 | Cambiar de pestaña no escribe | T3 | Escritor simulado |
| 7 | Cerrar o descartar no escribe | T1, T3 | Ídem |
| 8 | Frontera no bloqueable conserva texto inválido | T1, T3 | INV-18 |
| 9 | Frontera bloqueable aborta con texto inválido | T1, T3 | INV-18, ADR-0032 D6 |
| 10 | StaleBase solo detecta cambios | T1, T2 | Guarda |
| 11 | La comprobación relee datos authored | T2, T4B | Lector simulado y entrada caller-owned |
| 12 | Ningún cambio externo se sobrescribe sin detectarse | T1, T4B | INV-06 |
| 13 | La sesión A nunca escribe B | T2, T4B | INV-22 |
| 14 | Destruir el documento retira sesión y borradores | T1, T2 | INV-20, aviso de pérdida |
| 15 | Dirty o pendiente participan en el veto | T1, T2, **T3** | INV-20; el texto vivo llega al borrador antes del veto |
| 16 | Pistas propias no cuentan como externas | T4B | INV-08 |
| 17 | POST viene de una relectura | T1, T4B | INV-11 |
| 18 | Todo objeto y transacción muere dentro | T2, T4A, T4B | Guardas y `TopTransaction == null` |

## 6. Contrato propuesto de F4-T1-DRAFT-CORE

Archivos nuevos en `src/RackCad.Application/Workspace/Drafts/` (namespace `RackCad.Application.Workspace.Drafts`) y pruebas `WorkspaceDraft…` en `tests/RackCad.Tests/Workspace/`. Sin AutoCAD, WPF, escrituras, E/S, eventos ni tareas. Una guarda nueva aplica al subnamespace lo mismo que `WorkspaceModelBoundaryTests` aplica al namespace padre.

- **Estado genérico** `DraftSession<TState>` con comparador inyectado: dirty depende del estado materializado, nunca de JSON crudo. El tipo concreto del piloto será el estado existente de Push Back (T2), no uno paralelo.
- **Componentes:** StaleBase, InitialDraftState, CurrentDraftState, texto pendiente y estado. Limpio y Dirty se derivan; Aplicando, Conflicto, Desconocido y Error son explícitos; Posiblemente obsoleto es una marca ortogonal.
- **Nacimiento:** solo desde el resultado de una lectura de intención admitida. Toda recarga sustituye base e inicial a la vez.
- **Texto pendiente y fronteras** (ADR-0032 D6, Anexo B): compromiso en dos fases, validar todo sin mutar y aplicar todo o nada. Las bloqueables (cambio de pestaña o rack, Actualizar) abortan con el texto en su sitio. Las no bloqueables (cambio de documento, ocultar, desmaterializar, cierre) conservan el texto tal cual, sin comprometer ni reparar.
- **StaleBase y comparación** (D-12, INV-28, INV-29):
  - atribución de **cuatro** valores (MUTABLE, READ_ONLY, BLOCKING, NOT_MEMBER) con `View` y `Section`, **sin** la división Redraw/Erase;
  - claves de la **unión** de base y relectura; una clave ausente equivale a NOT_MEMBER;
  - texto crudo exacto **solo de cada sobre miembro interpretable**;
  - registro de variables crudo cuando aplique;
  - **sin** recuento de referencias;
  - MATCH solo si todo es idéntico, sin normalización, con la lista de lo que cambió.
- **Reconciliación:** descartar y recargar, o conservar mi borrador sobre la versión actual tras confirmación explícita. Sin fusión automática.
- **Actualizar como máquina de estados:** una sola operación en curso. Clasificación de la tabla de D-13. Desconocido tiene dos orígenes: «`Commit()` lanzó», que se resuelve con «Comprobar ahora» (coincide con la base anterior: recupera el estado previo; si no, Conflicto), y «la relectura de POST falló», que pide recargar. Nunca se supone aplicado; POST solo deja Limpio con relectura.
- **Registro por (SessionId, RackId):** independiente entre sesiones, retirado con la sesión, sin resurrección, sobrevive a la desmaterialización.
- **Veto:** decisión pura desde memoria; cada intento se veta mientras haya dirty o pendiente; repetir no es consentimiento; el descarte explícito permite el siguiente cierre; una destrucción con borradores produce un aviso de pérdida.

## 7. Requisitos de cierre de F4

F4 **solo cierra con todas las obligaciones evidenciadas**:
- evidencia automatizada de INV-04..08, 10..12, 16..21, 25..31 y 39;
- Candidato F4: suite de UI completa en local y CI del SHA exacto;
- registro Debug de `TopTransaction == null`;
- **Smoke-2** sobre el Candidato, con S2-01..S2-15 y además lo que pide la orden: abrir un rack real; intención de edición; modificar sin Actualizar; cambiar de pestaña y volver; comprobar con `DBMOD` que no hay mutación; texto pendiente **inválido** en una frontera no bloqueable; Actualizar explícito; modificación externa antes de Actualizar; Conflicto y reconciliación; discordancia de sesión o documento; veto con borrador dirty o pendiente; cierre correcto tras descartar o con el borrador limpio; UNDO y REDO (INV-32);
- revisión del Coordinator o, en su ausencia, la disposición que el Owner fije.

**Paradas** (condiciones de devolución de la orden): cambiar la semántica authored congelada; capacidad de editor que arrastre F5; agotar `attempts`; fallo real de un invariante de seguridad en AutoCAD; detección de cambios externos no fiable; veto que el host no respete en `CLOSE`, `CLOSEALL` o `QUIT`; UNDO que no sea un solo paso sin la A-n de D-13; decisión material nueva de arquitectura.

## 8. Revisión independiente de la v1

Revisión adversarial independiente del 2026-11-10, sobre `442e49c3`: 2 BLOQUEANTE, 11 IMPORTANTE y varias MENOR. Disposición:

| Hallazgo | Disposición en la v2 |
|---|---|
| B1: nadie caracterizaba el piloto (INV-19) | F4-T1B antes de T2 |
| B2: StaleBase abierta a conflictos autoinfligidos | Reglas explícitas en §6 |
| I1: modelo paralelo | `TState` = estado existente; fila de autoridades |
| I2: C-F4-01 era desviación del Freeze | Planteada como tal, al Owner, antes de T2 |
| I3: alcance real de la guarda de `ExecuteInCommandContextAsync`; medición en host dentro de T4 | H-13 y C-F4-02 |
| I4: ámbito de mutación propia | C-F4-04 revisada |
| I5: la proyección con texto crudo se necesita en T2 | Proyección única en T2; T4A solo añade la entrada caller-owned |
| I6: veto, texto pendiente vivo y nombre del comando | T3 en la obligación 15; C-F4-03 antes de T2 |
| I7: INV-10, INV-25, INV-31 e inyecciones de POST mal asignadas | Reasignadas |
| I8: editor clásico ofrecido frente a IC-03 | C-F4-07 al Owner |
| I9: §7 debilitaba el cierre | §7 reescrito |
| I10: dibujos y operador de Smoke-2 | C-F4-06 |
| I11: conformidad de IC-04 sin mecanismo permitido | T2 declara mecanismo y excepción |
| MENOR | Subnamespace con guarda equivalente; cifras de H-03 y H-10; inyección de POST; dos orígenes de Desconocido; sin fusión automática; un único bloqueo en T4B; C-F4-05 declarada; paradas explícitas |
