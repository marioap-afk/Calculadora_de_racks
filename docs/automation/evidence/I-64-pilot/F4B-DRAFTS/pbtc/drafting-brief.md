> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: redactar el paquete del contrato de PB-TC (I-64 F4B), versión 1

Eres el redactor del paquete del contrato de una tarea de RackCad (plugin de AutoCAD 2025, C#/.NET 8, WPF). La tarea es **PB-TC — Correcciones declaradas** de la fase F4B «Push Back completo». Tu trabajo es medir el código y escribir un paquete preciso, que después revisará un revisor adversarial independiente. Si lo apruebas, un Controller copiará byte a byte sus colecciones y un Worker lo implementará sin más contexto que el paquete. Por eso todo hecho debe ir medido y con `ruta:línea`.

## Dónde leer

- **Worktree:** `<worktree-de-I-64>`, rama `architecture/workspace-persistente-rackcad`, HEAD `f9f2afbba97d0dd56e7165cec02dee276b03731f`.
  - Solo lectura. Usa `git -C <worktree>` y lectura de archivos. No hagas checkout ni commit, y no escribas en el worktree.
  - El producto en HEAD es el de la emisión de PB-T0 (`f2eedd9a`) más 10 archivos de pruebas de PB-T0. No hay otro cambio de `src/`.
- **Puedes compilar o ejecutar pruebas** solo si te hace falta medir algo. Hazlo con el SDK de usuario, `<datos-locales-del-usuario>\Microsoft\dotnet\dotnet.exe`, y nunca escribas fuera de una carpeta temporal propia.
  - **Nunca** corras la suite completa de UI en local: toca el perfil real del usuario.
  - Un filtro concreto de UI sí está permitido.

## Autoridades que mandan (léelas enteras)

1. `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md` (**[OD]**, texto literal del Owner): D-PB-06, D-PB-08 y D-PB-10.
2. `docs/initiatives/I-64-f4b-push-back-completo-package.md` (**[P]**, v3.1):
   - §3: D-PB-06 con OB-06.1 a OB-06.6, D-PB-08 con OB-08.1 a OB-08.4, y D-PB-10;
   - §5: la fila de PB-TC y «Por qué PB-TC va antes de PB-T1»;
   - §7: OB-T1B.1 a OB-T1B.3, OB-RE.1 y el registro declarado `I-64-pilot/F4B-DRAFTS/superseding-characterization.md`;
   - §8 riesgos, §9 y §10.
3. `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md` (**[DP]**, F4B-E03):
   - una replanificación por la comparación literal;
   - sustitutos de nc1 y nc2 solo con la firma EXACTA de PD-F4-04 y PD-F4-01.
4. `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md` (F4B-E01 a F4B-E05).
5. El paquete del contrato de PB-T0, `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`. Es tu **plantilla de estructura y de rigor**: §1 autoridades, §2 hechos medidos, §3 decisiones, §4 pruebas, §5 RED, §6 contrato propuesto (identidad, AllowedWriteScope, ForbiddenWriteScope, RequiredTests, invariantes, ExpectedEvidence, StopConditions, EligibleCells), §7 riesgos y §8 decisiones para el Owner.
   - Su contrato emitido es `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json`, y su generador está en la misma carpeta.
   - PB-TC **no** es una caracterización por mutantes: es RED con pruebas nuevas que demuestran cada pérdida y GREEN con la corrección del producto.
6. El cierre de PB-T0:
   - `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/task-close-package.md` (§5: arrastres a PB-TC, M-05 y la vuelta al Owner);
   - la entrega del Worker, `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/1/R20261117T185616Z-3c1b/worker-handoff.json`: Evidence, tablas T-1 a T-4 y el mapa CT → método;
   - las revisiones de `.../1/R20261117T215307Z-3cff/`.
7. El bloque M: `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/`, sobre todo M-7 (0 de 22 sobres reales con `DimensionStyle`).
8. Las caracterizaciones vigentes que fijan la pérdida:
   - T1B: `docs/initiatives/I-64-f4-t1b-gate-contract-package.md` y `tests/RackCad.*Tests/PushBackClassicCharacterization*`;
   - `WorkspaceDraftPilotWindowLoadCharacterizationTests` (WLC: su Theory y las filas pb-06, pb-07 y pb-08);
   - `T19_H1_TheDimensionStyleStillComesOutNull`;
   - las clases de PB-T0 `PushBackFullEditorCharacterization*`.
9. ADR-0029 (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md`) y el patrón de sustitución de I-39 (búscalo en `docs/`).

## Qué tiene que hacer PB-TC (de [OD] y [P]; no amplíes el alcance)

**(a) Redondeo.** La pérdida está en la ventana clásica de Push Back (H-PB-29 (a)): `NumericField.SetNumber` formatea con «0.###», y cada recálculo vuelve a leer las cajas, así que un valor con más de 3 decimales se redondea aunque el usuario no lo toque.
- La corrección se limita a la **ruta de carga de Push Back** (OB-06.6).
- `NumericField` es un control compartido; Cantilever hace 33 llamadas a `SetNumber`. **No** se toca el control compartido salvo que lo pruebes imprescindible, y entonces es una decisión del Owner.
- PB-TC prueba que Cantilever no cambia.
- **Mide:**
  - qué cajas y campos redondean: las filas F1 a F11 y G1 a G9 de la medición M10 de PB-T0;
  - dónde se releen;
  - qué diseño de corrección conserva el valor exacto mientras el usuario no edite la caja, y respeta su edición cuando la hay.
- Propón el diseño con su `ruta:línea`.

**(b) Estilo de cotas.** `ReadInputs` arma las anotaciones sin `DimensionStyle` (`RackPushBackSystemWindow.xaml.cs` alrededor de `:840-848`), y se pierde el estilo guardado.
- La corrección conserva el estilo cargado.
- Por construcción, eso cambia cómo dibuja las cotas RACKEDITAR: con estilo nombrado, en vez del estilo actual con sobrescrituras (`LateralHeaderDrawer.cs:353-375`, `:381-392`; `DynamicRackSystemResolver.cs:246`). Mídelo y descríbelo exactamente.

**D-PB-08.** Una indicación **no bloqueante**, visible cuando el lado B está declarado y no tiene ningún frente activo, que dice que el lado B no se guardará así.
- Va en los dos modos, porque es la misma ventana.
- Su regla y su caracterización (`PushBackBlankFrontAuthorityTests`) siguen en verde.
- **Propón el texto y el lugar.** Si es una decisión material de producto, márcala para el Owner.
- **Mide qué guardas saltarían** al añadir un elemento a la ventana: los tokens y literales de T-SG en `Systems/PushBack`, el orden de tabulación de T-KB, las guardas de texto de I-55, módulos, desviador, foco y `AnnotationScaleBox`, el censo de ventanas y G-10 (H-PB-32). Un elemento no enfocable puede no afectar al orden de tabulación; compruébalo con el código de la guarda, no lo supongas.

**El paso 4 en el Workspace** (OB-06.3) queda pendiente para PB-T2, con registro. PB-TC prueba el paso 4 solo en RACKEDITAR, es decir, en la ventana en modo clásico.

**La sustitución declarada (OB-T1B.2, OB-06.3b).**
- Las pruebas vigentes que fijan la pérdida no se editan: quedan con `Skip` y un motivo que nombra el registro.
- Se añade un contrato equivalente que sí se ejecuta y cubre todo lo que cubría lo retirado. El de la Theory de WLC cubre sus 14 filas.
- Se crea `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`.
- **Mide y enumera exactamente** qué pruebas fijan hoy la pérdida (a) o (b), con clase, método, fila y `ruta:línea`: WLC, T19_H1, las de PB-T0 (CT-PB0-19 y CT-PB0-20 en UI; mira también CT-PB0-01, CT-PB0-17, CT-PB0-18 y CT-PB0-21) y cualquier otra que encuentres con grep en TODO `tests/`.
- **Mide cuáles siguen válidas** sin cambio. Por ejemplo, CT-PB0-21 calcula el efecto en Core sin la ventana, y CT-PB0-18 fija el plan con estilo nulo y con estilo nombrado. Aplica la lección de F4: busca en todo `tests/` los nombres de tipos, API y cuentas exactas (`Assert.Equal(N, ...)`) que la corrección pueda mover.
- **M-05 de PB-T0:** CT-PB0-01 y CT-PB0-17 asertan listas vacías de líneas (a)/(b). Comprueba si siguen vacías tras corregir.
- T1B debe seguir en verde salvo lo sustituido con registro. Si una guarda de T1B no puede seguir en verde, es una vuelta al Owner antes del contrato: enuméralas.

## Lo que el paquete tiene que traer, además del contrato

1. **§2, hechos medidos** con `ruta:línea`, y un hallazgo nuevo con su número.
2. **El diseño de cada corrección:** qué archivos y qué líneas, por qué queda dentro de la ruta de Push Back y por qué no cambia Cantilever.
3. **La lista de pruebas nuevas:** RED, una o más por pérdida y por D-PB-08, con lo que fijan y por qué fallan en la base por aserción.
4. **La lista de sustituciones** (vieja → nueva, con la cobertura comparada).
5. **El contrato propuesto completo, como en el §6 de PB-T0:**
   - AllowedWriteScope de archivos exactos;
   - ForbiddenWriteScope **compacto**: prefijos seguros (un prefijo solo si ninguna ruta permitida queda debajo, la regla de F4-E21) y la menor lista posible, porque el Controller la copia a mano y en PB-T0 copió mal 3 de 265 entradas;
   - RequiredTests con filtro, MinSelected y ExpectRed por proyecto, contando las pruebas que el RED hace fallar;
   - invariantes;
   - ExpectedEvidence;
   - StopConditions, con la replanificación única de F4B-E03 como parada y los sustitutos de nc1 y nc2 SOLO con firma exacta, citando literalmente las firmas de PD-F4-01 y PD-F4-04 como en PBT0-C11;
   - el STOP de OB-06.4 si aparece un efecto material no medido;
   - el intento 3 vuelve siempre al Owner.
6. **La línea base de la CI** para RequiredTests y para las cuentas: toma las cuentas de la CI de `f9f2afbb` (run `44188678597`: Core 19 070 + 3 de PB-T0 y UI 2 421) o mídelas tú, y dilo.
7. **§8, «Decisiones para el Owner antes de emitir»**, con los datos medidos para cada una:
   - **El efecto de (a):** la tabla T-3 de PB-T0, filas R0 a R11.
     - Son «materiales» R0, R2, R5, R7 y R11.
     - R7 y R11 lo son por el BOM exacto (K3).
     - El máximo |Δ| de 106,988 de R7 es un artefacto del emparejamiento por orden cuando hay K1o (lectura OWNER-4 de la revisión independiente de PB-T0).
     - Explícalo en lenguaje llano: qué cambia en el dibujo y en la lista de materiales.
   - **El efecto de (b):** cambia el aspecto de las cotas cuando el sobre trae un estilo nombrado. M-7 midió 0 de 22 sobres reales con estilo, así que en los dibujos medidos no cambia nada.
   - **H-PB-28** (OWNER-2 de PB-T0): «Restaurar estándar» se pierde al Actualizar. ¿Se corrige como corrección declarada nueva, en PB-TC o en otra tarea, o PB-T1 lo reproduce tal cual? Mide el defecto (T-4 de PB-T0) y propón las opciones con su coste. **No** lo metas en el alcance de PB-TC sin la decisión del Owner.
   - **D-PB-08:** el texto y el lugar propuestos, si son una decisión material.
   - Cualquier guarda de T1B que no pueda seguir en verde.

## Reglas de redacción (lecciones de F4 y de PB-T0)

- **Cada hecho, medido en HEAD** con `ruta:línea`. Nada «por analogía» ni copiado de las lecturas de los mapas sin comprobar.
- **Las invariantes en español con construcciones nominales o «si + indicativo».** Nada de subordinadas que inviten al subjuntivo, porque el Controller las copia y en F4 cambió «es» por «sea».
- **Una guarda del tipo «la tarea no hace X» se limita a los cuerpos propios.** Un conjunto EXACTO de nombres, enums o estados bloquea a la tarea siguiente.
- **Si una prueba depende de teclado o foco WPF, dilo.** En F4, `RaiseEvent` no recorría `WM_KEYDOWN` → `WM_CHAR` → `TextInput`. No fijes una mecánica de entrada sin probarla.
- **El prompt del Worker no puede dar latitudes que el contrato no tiene** (lección de PB-T0). Si quieres que el Worker pueda corregir una prueba en un caso, ponlo en el contrato.
- **Las pruebas de UI** no corren la suite completa en local, no escriben en el perfil del usuario y cierran sus ventanas en `finally`, también si `LoadExisting` lanza (M-01 de PB-T0).
- **Sin rutas locales absolutas** en el paquete, salvo las del repositorio. Sin nombres de clientes.

## Salida

- `<scratchpad>\hs65\pbtc\I-64-f4b-pbtc-gate-contract-package.md`: el paquete v1, en español.
- `...\scratchpad\hs65\pbtc\pbtc-measurements.md`: las mediciones que hiciste (qué, cómo, resultado), para que la revisión pueda reproducirlas.

Tu respuesta final, en pocas líneas:
- el resumen del diseño;
- el número de archivos de AllowedWriteScope;
- las pruebas que se sustituyen;
- las decisiones para el Owner;
- lo que no pudiste medir.
