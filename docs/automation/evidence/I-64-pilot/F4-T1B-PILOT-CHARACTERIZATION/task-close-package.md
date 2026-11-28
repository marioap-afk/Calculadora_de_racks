# I-64 — F4-T1B-PILOT-CHARACTERIZATION: paquete de cierre

Lo prepara la sesión responsable el 2026-11-14. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E58 y F4-E59.

**Autoridades del cierre:**
- La autorización de F4-T1B v5 del Owner y del Coordinator (`F4-DRAFTS/coordinator/owner-f4t1b-authorization.md`). Fija la cadena entera, «Controller 14/14 alone is insufficient», la tabla CERRADA de mutantes, la contención del configurador y la prueba de identidad del producto como centro del cierre.
- La disposición del Owner antes del cierre (`F4-DRAFTS/coordinator/owner-f4t1b-close-decision.md`): I-01 «Adenda y cerrar», I-02 «Ratificar + regla» e I-03 «Ratificar contenida».

Este paquete cierra una subtarea, no un gate. No declara GATE PASS. La caracterización fija el comportamiento actual de la ventana clásica de Push Back como línea base de compatibilidad para T4B y T4C, **sin respaldar sus rarezas** (OD-1..OD-13).

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `1d36d0065ed3a469004f571ad8e3bc0fdc0a0d02`; CI `43650040527` push 4/4 en success |
| Identidad del producto | `git diff 0a6f84c2 1d36d006 -- src` vacío. En los 14 archivos de MutantScope, el blob de la base = el del GREEN ≠ el del RED. `--name-only` = los siete archivos nuevos de prueba |
| Verificación | `R20261114T233845Z-3df7`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha = BaseSha `0a6f84c2` (contrato); RED `92f5f5db` (pruebas y mutantes); GREEN `1d36d006` (inverso exacto) |
| Contrato | `F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json` (AuthorityRevision `24e3f414`, el paquete v5) |
| Intentos | 1 de 3 |
| Config | Snapshot I (`0B47701A…`) en todos los relevos; binario de Codex acreditado `5449c1f0` |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E59; las limitaciones en los §5 a §7) |

## 2. Corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `851e` (planificación) | aceptada a la primera | Comparación byte a byte PASS en las 8 colecciones, A1-A8 8/8. La nota de la sesión sobre AcceptanceCriteria[4] (S-2 no es un ancla) va en el registro de relevo |
| `15d5` (nc4) | oráculo cumplido | Solo A3 en fail con la copia que amplía AllowedWriteScope con `src/RackCad.UI/` |
| `c077` (Worker) | IMPLEMENTATION_COMPLETE | `claude-sonnet-5-5`/high, 227 turnos y 0 denegaciones. **RED** `92f5f5db`: 7 clases y los mutantes de la tabla cerrada. **GREEN** `1d36d006`: `git restore --source=BaseSha` de los 14 archivos |
| `3df7` (verificación) | `EXECUTION_VERIFIED` / NONE, 14/14 | Sin hallazgos |

**CI exacta:**
- **RED** `43649940464`: tests y ui-tests en failure, build-ui en success y build-plugin omitido por su `needs`. Termina con normalidad, sin cuelgue ni host caído (Core 18471 resultados, UI 2310).
  - El criterio mecánico de INV-F4T1B-02 lo midieron la sesión y la revisión independiente: UI 55/55 y Core 10/10 en Failed, sin prefijo de tipo, con la primera línea de la traza bajo `tests/` y sin NotExecuted ni Skipped.
  - Las pruebas existentes rotas, como unión, son tres y están atribuidas: `PushBackRoundTripSourceGuardTests…EveryRedrawRegenFalse` ← MU-25, `RichEditorCharacterizationTests.TheDefaultActionOfEachRichEditorIsWhatItIsToday` ← MU-01 y `PushBackTopeSectionInSafetyTests.MovingControl_DoesNotChangeTopeGeometryOrBom` ← MU-17.
- **GREEN** `43650040527`: los cuatro jobs en success. Filtros Core 10/10 y UI 55/55. Suites completas: Core 18471/18471 y UI 2293, con las 17 omitidas de la línea base.

**La prueba por mutante (INV-F4T1B-15).**
- Hay 45 corridas aisladas: 42 variantes de 26 filas, más tres repeticiones sobre el GREEN tras retocar tres métodos antes del RED.
- La revisión independiente comprobó que la unión de los 42 parches es **exactamente** el diff de `src/` del RED, y que cada fila discrimina al menos lo que INV-F4T1B-14 le asigna.
- MU-04 no se aplica (CT-02 usa el gesto, que superó su control positivo) y MU-18 tampoco (CT-18, opcional, no se escribe).
- Los parches se custodian en `c077/tests/m*/mutant.patch`.

## 3. Revisiones y controles

- **Revisión directa** (`3df7/direct-review.md`): conforme, con 4 menores (D-1..D-4) y el hallazgo previo H-1.
- **Revisión adversarial independiente** (`3df7/independent-review.md`): **ACEPTABLE CON MENORES (0/3/10)**. Las tres importantes (I-01..I-03) las dispuso el Owner (§5 y §6).
- **Análisis de la sesión para el Owner:** `3df7/analysis.md`.

**Controles** sobre las entradas de `3df7`, conservando cada salida ordinaria:

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261114T234208Z-6d40` | **CONTROL PASS**: Identity es la única en fail, con `EXECUTION_BLOCKED` / STOP |
| nc2 (sin la primera ruta del diff en el alcance) | `R20261114T234618Z-842a` | **CONTROL NOT PASSED**, con la firma exacta de PD-F4-01: Contract en fail solo por la cláusula de exactitud de la lista blanca, con la contradicción derivada de `byte-fidelity-check.json` igual que en F4-T4A2; Scope en fail sobre `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`; `EXECUTION_BLOCKED` / STOP (S-04, P-03). Se conserva tal cual |
| sustituto de nc2, `F4T1B-NC2-EXACT-WHITELIST-01` | `R20261114T235423Z-083e` | **PASS**: 10/10 propiedades y autoprueba determinista. Es copia del de F4-T4A2, con solo el identificador y el ámbito cambiados. nc2 = SATISFIED_UNDER_F4T1B_EXACT_WHITELIST_SUBSTITUTE |
| nc3 («GATE PASS» en `WorkCompleted`) | `R20261114T235440Z-c45c` | **CONTROL PASS**: FreeText es la única en fail, con `EXECUTION_REWORK_REQUIRED` / REWORK |

- **Participantes:** 0 en todas las comprobaciones de salida y de entrada de la cadena.
- **Avisos de límite** en los registros de nc1..nc3: los «limit reached» que cuentan son el texto de la parada S-11 que el Controller leyó de AUTOMATION_PLAN; no son avisos del CLI.

## 4. Lo que entrega F4-T1B

- **Siete clases nuevas**, sin editar ningún archivo existente:
  - seis en `tests/RackCad.UI.Tests`: Keyboard, Close, Intent, Owner, Resource y SourceGuard;
  - una en `tests/RackCad.Tests`: PluginGuard.
  - Dan 55 resultados de UI y 10 de Core.
- **El mapa CT → método** es Evidence[0] de la entrega.
- **Cubren:**
  - CT-01..CT-17 y CT-19..CT-28, con CT-04 promovido tras tres corridas estables e independiente de la activación;
  - CT-18, opcional, no se escribe.
- **Mediciones del paso 1**, sobre la base:
  - Escape y Enter por `ProcessKey`, con su control positivo;
  - la doble pregunta en modal (OD-7);
  - el texto sin comprometer de un campo que se pierde (OD-13);
  - el foco y la tabulación efectivos, solo como evidencia: la barra de acciones se recorre **después** del panel lateral (OD-9).
- **Las tres clases de escritura** (INV-F4T1B-16):
  - **A**, solo por fuente: la biblioteca y la disposición del configurador;
  - **B**, en ejecución sobre el contrato público;
  - **C**, solo por fuente en RackCad.Tests (RACKPUSHBACK, RACKEDITAR, el menú y las fachadas).
- **Contención:** ninguna prueba instancia `RackFrameConfiguratorWindow` ni ejerce PromptSaveToLibrary, el BOM, `RackProjectStore.Save`, `SaveLibrary_Click` ni `InsertBatch_Click` con entradas válidas. La prueba de limpieza alrededor de las corridas de T1B no cambia nada.
- **Deuda D-T1B-06 (OD-1..OD-13)**, caracterizada sin corregir, según Evidence[10] de la entrega.

## 5. Adenda de la sesión: las anclas (I-01, disposición del Owner «Adenda y cerrar»)

La entrega (Evidence[2]) no declara todas las aserciones sin mutante propio. **Toda aserción va en un método que un mutante de la tabla detecta**, así que la parada F4T1B-C04 (3) no se activa (disposición del Owner). La lista que sigue la midió la revisión independiente sobre `1d36d006` y completa la de la entrega.

**Las aserciones sin mutante propio que la entrega no declara** (todas en métodos detectados):

| CT | Aserciones | Dónde | Método detectado por |
|---|---|---|---|
| CT-05 | Las 10 aserciones del contrato inicial (InsertRequested, UpdateOnly, InsertView, InsertSection, InsertionRequest, SystemToInsert, DesignToInsert, SourceProjectToInsert, RackId y la pregunta no hecha) | `PushBackClassicCharacterizationCloseTests.cs:516-529` | MU-05 (`RackName`, `:547` y `:571`) |
| CT-07 | `InsertRequested`, `UpdateOnly` y el GUID de `newIdFactory` | `PushBackClassicCharacterizationIntentTests.cs:129-130` y `:140-141` | MU-06 y MU-07 |
| CT-04 | El estado final: pendiente, visible, sin petición y cerrada por el respaldo | `CloseTests.cs:489-492` y `:507-510` | MU-02 |
| CT-02 y CT-03 | Que «Sí» no confirma la edición | `CloseTests.cs:250-251` y `:370-371` | MU-02 |
| CT-14 | `Owner` nulo del diálogo construido | `PushBackClassicCharacterizationOwnerTests.cs:179` | MU-14 L/D/C |
| CT-17 | Las selecciones de Seguridad y la cuenta de llamadas a la costura | `IntentTests.cs:368-369` | MU-17 |
| CT-21 | Los dos `ShowDialog()` y la petición condicional | `PushBackClassicCharacterizationSourceGuardTests.cs:315-318` | MU-21 |
| CT-22 (a) | El orden `EnsureId` < `SetName` < proyecto < `Save` | `SourceGuardTests.cs:337-340` | MU-22 W |
| CT-26 | Los pasos que siguen al tercero | `PushBackClassicCharacterizationPluginGuardTests.cs:284-290` | MU-26 M |
| CT-27 | Las firmas de `DrawAndPlace`, `PrepareRedraw` y `RedrawInTransaction`, y los `Lacks` de los demás archivos | `PluginGuardTests.cs:322-347` y `:380-381` | MU-27 S/F/P y D |
| CT-01 | Lo que sigue al gesto de Enter: no cierra, no pregunta y no cambia `RecomputePassesForTest`. MU-01 lo enmascara, porque la aserción de declaración de `:69` va antes del gesto | `PushBackClassicCharacterizationKeyboardTests.cs:74-79` | MU-01 y MU-03 (control positivo de Escape) |

**Las dos declaraciones inexactas de la entrega**, corregidas:
- **CT-28:** la entrega declara como ancla «el campo conserva el valor tecleado», y esa aserción **no existe** (`CloseTests.cs:683-700`). Las anclas reales de CT-28 son la ausencia de pregunta y de petición. El método lo detecta MU-28.
- **CT-23 (fuente):** la entrega declara «las cinco vistas fijas del lote», y la guarda fija **tres** (`SourceGuardTests.cs:372-374`).

**Otras precisiones de la entrega** (M-10 y M-01 de la revisión independiente):
- Deviations[2] dice «dos pruebas»: fueron tres métodos (`EnterOnTheAccessKey…`, `TheWindowMerges…` y `TheShellDoesNot…`).
- Evidence[2] afirma, sobre CT-09, que ningún mutante cambia el tooltip de Actualizar, y MU-09 T sí lo cambia sin AutoCAD (`.xaml.cs:3533`).
- Varias referencias `archivo:línea` de Evidence[1] están desplazadas una o dos líneas:

  | Mutante | Dice | Real |
  |---|---|---|
  | MU-05 | `:1203` | `:1204` |
  | MU-06 | `:3376` | `:3377-3378` |
  | MU-10 C | `:314` | `:315` |
  | MU-20 W | `:2863` y `:3045` | `:2865` y `:3047` |
  | MU-24 S | `:47` | `:48` |
  | MU-24 D | `:56` | `:57` |
  | MU-22 C | `:602` | `:604` |

  Los cambios exactos están en los parches custodiados.

## 6. Desviaciones ratificadas por el Owner

- **I-02, «Ratificar + regla».**
  - **Lo que pasó.** El prompt de la sesión pedía las suites completas en local. La suite de UI existente instancia el configurador real: con `new RackFrameConfiguratorWindow(` en 9 archivos de `tests/RackCad.UI.Tests`, y por la costura `HeaderConfiguratorPresenter` en `PushBackHeaderAuthorityWindowTests.cs`. Al cerrarse, escribe `RackFrameConfigurator.layout` en `LocalApplicationData`, una ruta `static readonly` de `RackFrameConfiguratorLayoutStore.cs:26-29`. La fecha del archivo real cambió dos veces, con el mismo SHA-256, en las suites completas `rf` y `gf`. El Worker no se paró, como pedía el prompt: acotó la prueba a sus corridas y lo registró como limitación.
  - **La ratificación.** Las pruebas de T1B no escriben (INV-F4T1B-06 se cumple). La escritura fue una **desviación de la delegación causada por el prompt de la sesión**.
  - **La regla, desde ahora:** en una tarea con contención de perfil, nunca se piden suites completas de UI en local; las suites completas solo corren en la CI.
  - **H-1** (la suite existente escribe en el perfil real en cada corrida completa) queda como **deuda aparte**, a decisión del Owner. Puede resolverse con una costura del almacén de disposición en una tarea propia, que no es de T1B.
- **I-03, «Ratificar contenida».**
  - **Lo que pasó.** Una sonda temporal de medición disparó `InsertBatch_Click` con entradas válidas sobre una ventana sin mostrar. Lanzó en `Owner = owner` (`RackViewBatchDialogPresenter.cs:59`) antes de `ShowDialog`, no escribió nada y no llegó a ningún commit.
  - **La ratificación.** Se acepta como desviación contenida y declarada.
  - **La regla, desde ahora:** los prompts prohíben las sondas con entradas válidas en caminos con diálogo o escritura.

## 7. Limitaciones y arrastres

- **Menores de la revisión independiente:**
  - M-02: las guardas de orden del Plugin son débiles (`InOrder`, `UpdateOnly`, `DrawPushBackView` tras `ShowModalWindow`);
  - M-03: el alcance y la fragilidad de las guardas de texto, incluido el falso positivo de CT-27 (2) si T4B añade un llamador ajeno a Push Back en archivos compartidos;
  - M-04: la cultura no se fija en los diagnósticos con `0.##`;
  - M-05: CT-09 deja cinco ventanas sin mostrar y sin cerrar;
  - M-06: falta «INCUMPLIMIENTO CARACTERIZADO» en OD-4;
  - M-07: aserciones redundantes;
  - M-08: CT-08 eleva `ClickEvent` sobre botones deshabilitados;
  - M-09: el subshell en segundo plano.

  **Destino:** la reconciliación previa a T4B (§8 del paquete) y F5. Ninguno toca `tests/` sin RED.
- **Menores de la revisión directa:** D-1 (es I-03), D-2 (procesos en segundo plano), D-3 (varias instancias por fila) y D-4 (repeticiones aisladas). Todos aceptados.
- **KnownLimitations de la entrega:**
  - S-2 y OD-3 solo por fuente;
  - el enrutado desde el elemento con foco;
  - el dueño efectivo, WM_CLOSE y Alt+F4 reales y el DWG, solo en el host;
  - las guardas de texto que F5 sustituye según I-39;
  - el riesgo aceptado de CT-04;
  - el uso del equipo.
- **Candidatos a Smoke-2** (D-T1B-07): el dueño efectivo con `ShowModalWindow`, WM_CLOSE y Alt+F4, el orden Z, el foco y la tabulación efectivos, S-2 y OD-3 en ejecución, y Escape y Enter desde el elemento con foco.
- **Observación para T4B** (revisión independiente, punto 3): en la base, la `LastComputation` del fixture compuesto difiere de la que da un recálculo.

## 8. Siguiente

Por la autorización de T1B, el orden es:
1. Volver al paquete v3 de F4-T4B.
2. Repetir su validación y su revisión contra la rama posterior a T1B.
3. Incorporar los hallazgos de T1B a sus supuestos: la línea base caracterizada, M-02, M-03, la observación de `LastComputation`, la regla de I-02 y la de I-03.
4. Solo entonces puede emitirse T4B, por la vía propia del piloto y sin volver a la costura gruesa de I-55.

T4C y Smoke-2 siguen pendientes.
