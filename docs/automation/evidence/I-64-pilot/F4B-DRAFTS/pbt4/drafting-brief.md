> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: paquete v1 del contrato de PB-T4 (I-64 F4B): «Editar todo» en el panel del Workspace

Eres el redactor del paquete de contrato de PB-T4. Trabajas **en solo lectura** y **no emites nada**. El paquete pasará después por una revisión independiente adversarial, y se volverá a medir sobre el cierre de PB-T3c antes de emitirse.

## Lo que pide el Owner (literal, [OD], fila de PB-T4 del §5 del paquete de la fase)

> ACCEPT. Add "Editar todo" to the Workspace only after PB-T1/T2 establish the draft-based editor seam. Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.

**El contenido que la fase asigna a PB-T4** (fila de PB-T4 del §5 del paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1):
- abrir la ventana en modo WORKSPACE sobre el borrador (D-PB-01: modal, draft-in/draft-out, cero escrituras en el DWG; una invocación WORKSPACE nunca llega al camino clásico);
- el puerto generalizado;
- los pasos 1 a 3 de D-PB-05 y OB-05.4, con la composición de PB-T3 conectada (OB-05.5). Conservar es una elección destructiva informada: confirmación explícita después de la presentación; sin elección no se escribe. Hasta PB-T4 sigue conectada la semántica actual de «Conservar mi borrador» (`KeepPilotEditsOn`), y PB-T4 la cambia en el mismo paso que la presentación y la elección;
- la guarda de texto de OB-01.3: el abridor de «Editar todo» no llama a ninguna ruta clásica (`RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, ni las entradas de escritura de las fachadas `DrawAndPlace` y `RedrawInPlace`) ni lee `InsertRequested` ni `InsertionRequest`, con su autoprueba en negativo;
- R-PB-10: el texto pendiente del panel se confirma o se rechaza antes de abrir (la frontera de F4); mientras el editor está abierto, el panel no edita ese borrador;
- el resumen y «Modificado»;
- OB-03.2: quitar el rechazo es un paso explícito, con su prueba, en la primera tarea cuyo contrato lo autorice (previsto en PB-T3 o PB-T4). Mide si PB-T3 lo hizo; si no, es de PB-T4.

**Depende de PB-T2 y PB-T3.** PB-T3c está ahora en ejecución (Worker en el worktree). Su contrato está emitido en `7dfc7c21` (paquete de PB-T3 v4.7). Mide sobre `7dfc7c21` y marca con `<tras el cierre de PB-T3c>` todo lo que dependa del GREEN de PB-T3c (la fachada `RackViewBlockEraser` y `EraseUnit` en WR).

## Arrastres y decisiones del Owner que condicionan PB-T4

Búscalos y cítalos con su ruta y línea en `7dfc7c21`:
- **A-T2b-OWNER-01** (F4B-E38, «Corregir en PB-T4 (Recomendado)»; `F4B-DRAFTS/coordinator/owner-pbt2b-close-decisions.md:36`, `f4b-dispositions.md:1121`, `docs/ideas-futuras.md:1871`): «Cerrar» en WORKSPACE con un módulo escenificado y cambios sin aplicar a la vez hace una sola pregunta que nombre las dos cosas.
- **Q-PBT2-3** (`F4B-DRAFTS/coordinator/owner-pbt2-decisions.md` o donde esté custodiada; F4B-E28): la validación única se unifica en PB-T4, con la prueba de concordancia que ya existe.
- **D-1 de PB-T3** (F4B-E29, «Un solo código», variante (A)) y el §5.3 del paquete de PB-T3: al quitar el piloto, PB-T4 sustituye con registro E-3, E-5 y E-6. Mide si PB-T4 quita el piloto según la fase; si la fase no lo dice, es una pregunta para el Owner (§8), no una decisión tuya.
- **D2 del Owner** (F4B-E11) y su arrastre A-2, con L-5 (el límite se extiende a PB-T4): las dos referencias compartidas no se mutan; STOP si hubiera que hacerlo.
- **La ExtensionData anidada** («Basta así»): el rechazo AUTH-13 del panel sigue.
- **La nota del Owner sobre la interfaz** (`F4B-DRAFTS/coordinator/` o la custodia de PB-T0: «la interfaz o botones extra … los arreglamos después cuando se finalice la primera etapa, lo importante es la funcionalidad»): PB-T4 no añade botones ni campos que no pida el contrato.
- **Las limitaciones y menores abiertos** que toquen la ventana o la escritura en WORKSPACE (`docs/ideas-futuras.md` y los paquetes de cierre de PB-T1c a PB-T3b: OWNER-01 de Restaurar, PBT3B-IR-M01 con `DivergentDiagnostic`/OWNER-2, el hallazgo del layout del configurador que escriben las pruebas de UI). Clasifícalos: dentro de PB-T4, fuera con su motivo, o pregunta para el Owner.
- **El resto de decisiones de F4B**, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/` y `F4B-DRAFTS/f4b-dispositions.md` (F4B-E01 a F4B-E47), en especial F4B-E21 (colecciones opacas), F4B-E25 (UNIT_CHANGE en las correcciones), F4B-E35 (la regla RR del RED) y F4B-E46 (el total exacto de cada rama).

**La emisión de PB-T4 NO está autorizada.** Las autorizaciones condicionales cubren PB-TC2, PB-T1, PB-T2 (F4B-E28 Q4) y PB-T3 (F4B-E35 Q3). El §8 del paquete tiene que traer la pregunta de emisión para el Owner, y cualquier otra decisión material, agrupadas en una sola vuelta (cuatro preguntas como máximo, con la opción recomendada primero).

## Cómo leer (obligatorio)

Ahora mismo hay un Worker trabajando en el worktree de la rama. Está **PROHIBIDO** leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- Lee el repositorio SOLO por objetos git desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show 7dfc7c21:<ruta>`, y también `grep -n <patrón> 7dfc7c21 -- <rutas>`, `ls-tree`, `log` y `diff`. No leas los archivos de ese checkout en disco.
- Sin compilar ni ejecutar pruebas. Sin órdenes git que escriban. Sin `fetch`. Sin AutoCAD.
- Python solo de lectura, con `-I`, escrito con la herramienta Write y nunca con `-c`, `python -` ni heredoc.
- Escribe solo en `hs65\pbt4\`.

## Fuentes

- **El paquete de la fase** v3.1, entero; en especial D-PB-01, D-PB-05 con sus pasos, OB-01.x, OB-03.2, OB-05.x, OB-06.x, R-PB-09, R-PB-10 y las filas de PB-T4 y PB-T5.
- **Los paquetes de PB-T2 y PB-T3** custodiados (`docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md` y `…-pbt3-gate-contract-package.md` v4.7), con sus registros de mediciones y sus contratos (`docs/automation/evidence/I-64-pilot/F4B-PBT2*/coordinator/gate-contract.json`, `F4B-PBT3*/…`).
- **El código en `7dfc7c21`:**
  - el panel del Workspace de Push Back (Plugin y UI) y dónde iría «Editar todo»;
  - la ventana de Push Back en modo WORKSPACE (PB-T2b) y el puerto (PB-T2a);
  - el estado editable y la puerta de salida (PB-T1);
  - la composición completa, el plan y la corrida del escritor (PB-T3a y PB-T3b), y la semántica de hoy de «Conservar mi borrador»;
  - las pruebas de esas piezas y las guardas (T1B, I-55, T4 de F4, PB-T1 a PB-T3).
- **Los cierres de F4B** (`task-close-package.md` de cada tarea) y `docs/ideas-futuras.md`.
- **El protocolo y las plantillas:** `docs/AUTOMATION_PLAN.md` §16, `docs/automation/agent-execution/README.md` y los contratos y generadores de F4B.

## Qué tiene que contener el paquete v1

1. **Los hechos medidos** sobre `7dfc7c21` para cada punto de la fila de PB-T4 y de cada arrastre: qué existe ya, qué falta, y dónde (ruta:línea).
2. **El diseño:** cómo se abre «Editar todo» y con qué estado; la presentación y la elección de los pasos 1 a 3 de D-PB-05; la conexión de la composición de PB-T3; la guarda de OB-01.3; R-PB-10; el resumen y «Modificado»; A-T2b-OWNER-01; la validación única; qué NO hace (no tocar RACKEDITAR clásico, no crear vistas, no inventar traducción de estado). Si es demasiado grande para un Worker, propón subtareas secuenciales (PB-T4a, PB-T4b…), cada una con su 1/3.
3. **La red de pruebas:** lo que sigue en verde; las CT nuevas (un `[Fact]` por CT salvo motivo, por F4B-E46); el RED y el GREEN con su forma exacta y los totales previstos de cada rama; qué se puede probar sin AutoCAD y qué queda para el Smoke-PB de PB-T5 (D-PB-09).
4. **Las guardas:** si se mantienen en verde. Las que no puedan, al §8 con su registro declarado (como D-1 de PB-T3).
5. **El contrato propuesto, por subtarea,** con el patrón de PB-T3: TaskId; AllowedWriteScope; ForbiddenWriteScope; RequiredTests; invariantes mecánicos; ExpectedEvidence; StopConditions; EligibleCells; la línea base de la CI. Aplica las lecciones de F4B:
   - una lista cerrada de miembros admite explícitamente los ayudantes privados necesarios, o los declara parada;
   - la CI del RED con Core en failure deja `build-plugin` omitido;
   - compilar el esqueleto sobre una copia exportada y ejecutar la suite completa de Core antes de emitir (las sobrecargas ambiguas y la reflexión por nombre no las ve un barrido por nombre);
   - el total exacto de cada rama de recuento (F4B-E46);
   - el tamaño del contrato (F4B-E21);
   - nada en el texto del esqueleto que lean las guardas de texto.
6. **Los riesgos y las lecciones:** sin latitud para el Worker; nada de AutoCAD en el Worker; corridas temporales con `Assert.Fail`; MAX_PATH; un solo «Actualizar» lógico; las pruebas de UI que crean ventanas reales y escriben archivos del perfil (el layout del configurador).
7. **§8: las decisiones del Owner:** las tomadas, con su cita, y toda decisión material nueva, con la pregunta de emisión. Redáctalas como preguntas autosuficientes (el Owner solo ve el diálogo), con 2 a 4 opciones y la recomendada primero.
8. **§9: los límites.**

## Lo que entregas

- `hs65\pbt4\I-64-f4b-pbt4-gate-contract-package.md` (v1);
- `hs65\pbt4\pbt4-measurements.md`, el registro de lo medido, con la fuente de cada hecho;
- los guiones de medición que escribas, con sus salidas.

**Responde con:** la división en subtareas, las CT por subtarea, las decisiones para el Owner (texto de cada pregunta y opciones), los riesgos principales y lo que quede abierto.
