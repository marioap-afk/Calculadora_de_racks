> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: paquete v1 del contrato de PB-T3 (I-64 F4B): la escritura completa del Workspace

Eres el redactor del paquete de contrato de PB-T3. Trabajas **en solo lectura** y **no emites nada**. El paquete pasará después por una revisión independiente adversarial, y se volverá a medir sobre el cierre de PB-T1 antes de emitirse.

## Lo que pide el Owner (literal, [OD], fila de PB-T3 del §5 del paquete de la fase)

> ACCEPT IN PRINCIPLE. The Workspace authored writer may expand to full Push Back design. It must eventually support, as characterized/authorized: field changes; modules/fronts; side B; rename; deleting obsolete views; blocking authored write on diagnostics/errors; view creation only if PB-T0 subsequently authorizes it. Still require: PREPARE -> MUTATE -> POST and one logical Update.

**El contenido que la fase asigna a PB-T3** (§5 del paquete de la fase):
- el plan y la corrida generalizados;
- borrar vistas (caller-owned); renombrar;
- la barrera de diagnósticos con el criterio extraído en PB-T1 (`PushBackEditorOutputGate`, ya en `src/RackCad.Application/Systems/PushBack/` en `2f5d3dec`);
- `ComparePost` genérico;
- la composición y el clasificador de D-PB-05, sin conectarlos todavía a «Conservar mi borrador» (OB-05.2);
- crear vistas solo si PB-T0 lo autoriza (§6.4). El bloque M de PB-T0 lo resolvió: H-PB-16 confirmado, «Actualizar» NO crea vistas, ni con +frente ni con A/B; la fila 1 del §6.4 da PB-T3 sin crear vistas, sin volver al Owner;
- PREPARE → MUTATE → POST y un solo «Actualizar» lógico.

**Depende de PB-T1,** en paralelo con PB-T2 (R-PB-09: solo quiere decir independiente; las delegaciones son sucesivas).
- PB-T1b (en cierre) deja en Application, con su `src/` final en `2f5d3dec`:
  - `PushBackEditableState`, con el copiador, el comparador y el mapa de propiedad;
  - `PushBackEditorOutputGate`.
- PB-T1c solo cambia la ventana.

## Decisiones del Owner que condicionan PB-T3

- **La ExtensionData anidada y el paso 6 de D-PB-05** (2026-11-19, literal «Basta así (Recomendado)»):
  - PB-T3 sigue;
  - el panel mantiene el rechazo con aviso de AUTH-13 (I-58) a todo campo desconocido a cualquier nivel, y el contrato de PB-T3 lo fija con una prueba;
  - la pérdida silenciosa de RACKEDITAR queda declarada como limitación conocida anterior a F4B, sin tocar el clásico (D-16, INV-19).

  Registro: `hs65\pbfull\owner-f4b-dpb07-extensiondata-decisions.md`. Expediente con las anclas medidas en `2f5d3dec`: `hs65\pbfull\owner-return-extensiondata.md`. Lee sus notas: la línea ~314 del paquete de la fase hay que precisarla, y la promesa de OB-05.2 de nivel superior no se ejercita hoy.
- **D2 del Owner** (`owner-d1-d2-decisions.md`): las dos referencias compartidas (la línea base resuelta y el informe de módulos de cada lado), con sus diez condiciones y su límite: STOP si hubiera que mutarlas. Su arrastre A-2 a PB-T3 está en el §10 del paquete de PB-T1.
- **El resto de decisiones de F4B**, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/` y `F4B-DRAFTS/f4b-dispositions.md` (F4B-E01 a F4B-E22):
  - [OD], con D-PB-05 modificada: conservar es una elección destructiva informada, clasificada y sin diff JSON; sobre, identidad, esquema, sesión y ExtensionData no vuelven atrás;
  - [DP];
  - F4B-E21: las colecciones opacas del contrato las pone la sesión.

## Cómo leer (obligatorio)

Ahora mismo hay otro proceso trabajando en el worktree de la rama. Está **PROHIBIDO** leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- Lee el repositorio SOLO por objetos git desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show 2f5d3dec:<ruta>`, y también `grep -n <patrón> 2f5d3dec -- <rutas>`, `ls-tree`, `log` y `diff`. No leas los archivos de ese checkout en disco.
- Sin compilar ni ejecutar pruebas. Sin órdenes git que escriban. Sin AutoCAD.
- Python solo de lectura, escrito con la herramienta Write y nunca con `-c` ni heredoc.

## Fuentes

- **El paquete de la fase:** `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1, entero. Lo que te toca:
  - D-PB-05 y sus pasos;
  - OB-05.x;
  - §6.4 y el bloque M de PB-T0 (H-PB-16);
  - H-PB-x sobre el escritor del Workspace;
  - R-PB-09;
  - la fila de PB-T3 y la de PB-T4, con OB-05.5: la composición se conecta en PB-T4.
- **El escritor de hoy del piloto Workspace de Push Back:**
  - `src/RackCad.Application/Systems/PushBack/Pilot/`: el materializador, el plan, la corrida y `ComposeForWrite`;
  - `src/RackCad.Application/Workspace/`: DraftSession, Update, PREPARE, MUTATE, POST, `ComparePost` y conflictos;
  - el Plugin del Workspace;
  - las pruebas de T4 de F4: `tests/RackCad.Tests/Workspace/` y `tests/RackCad.UI.Tests/Workspace/`;
  - los paquetes `docs/initiatives/I-64-f4-t4*`.
- **El paquete de PB-T1 v2.7** (`hs65\pbt1\I-64-f4b-pbt1-gate-contract-package-v27.md`):
  - D-PBT1-02 a D-PBT1-11: el tipo, el copiador, el comparador, el mapa y el criterio extraído;
  - D-PBT1-15 (D2);
  - §10 (arrastres).
- **El código de PB-T1b** en `2f5d3dec`: `PushBackEditableState*.cs` y `PushBackEditorOutputGate.cs`.
- **PB-T0:** el paquete, el cierre y el bloque M: `docs/automation/evidence/I-64-pilot/F4B-PBT0-*` y `block-m-analysis.md`.
- **I-58 (AUTH-13)** y las pruebas de su matriz, citadas en el expediente de la ExtensionData.
- **El protocolo y las plantillas:**
  - `docs/AUTOMATION_PLAN.md` §16;
  - `docs/automation/agent-execution/README.md`;
  - los contratos `docs/automation/evidence/I-64-pilot/F4B-*/coordinator/gate-contract.json` y sus generadores.

## Qué tiene que contener el paquete v1

1. **Los hechos medidos** sobre `2f5d3dec`, sobre lo que escribe hoy el piloto y lo que le falta para cada punto del [OD]:
   - campos;
   - módulos y frentes;
   - lado B;
   - renombrar;
   - borrar vistas obsoletas;
   - la barrera de diagnósticos;
   - el PREPARE, MUTATE y POST de hoy y su Deshacer;
   - `ComparePost`;
   - el rechazo AUTH-13;
   - los conflictos.
2. **El diseño:**
   - el plan y la corrida generalizados sobre `PushBackEditableState`;
   - la barrera con `PushBackEditorOutputGate`, sin duplicar el criterio;
   - borrar vistas y renombrar;
   - `ComparePost` genérico;
   - la composición y el clasificador de D-PB-05, sin conectar;
   - la prueba que fija AUTH-13;
   - D2 respetada;
   - qué NO hace: no crear vistas, no tocar RACKEDITAR, no tocar la ventana (eso es PB-T2 y PB-T4).

   Si es demasiado grande para un Worker, propón subtareas secuenciales (PB-T3a, PB-T3b…), cada una con su 1/3.
3. **La red de pruebas:**
   - lo que sigue en verde sin tocarlo;
   - las pruebas nuevas (CT-PBT3-xx);
   - el RED y el GREEN, con su forma exacta;
   - qué se puede probar sin AutoCAD y qué queda para el Smoke-PB de PB-T5 (D-PB-09).
4. **Las guardas** (T1B, I-55, T4 de F4 y las de PB-T1): si se mantienen en verde. Si alguna no puede, va al §8.
5. **El contrato propuesto,** por subtarea, con el patrón de PB-T1:
   - TaskId; AllowedWriteScope; ForbiddenWriteScope; RequiredTests;
   - invariantes mecánicos; ExpectedEvidence; StopConditions; EligibleCells;
   - la línea base de la CI.

   Ten en cuenta las lecciones de PB-T1b:
   - una lista cerrada de miembros debe admitir explícitamente los ayudantes privados necesarios, o declararlos parada;
   - la CI del RED con Core en failure deja `build-plugin` omitido;
   - en una corrección, las rutas UNIT_CHANGE ya cambiadas en la base (AUTOMATION_PLAN 16.3);
   - el tamaño del contrato (F4B-E21).
6. **Los riesgos y las lecciones:**
   - sin latitud para el Worker;
   - nada de AutoCAD en el Worker;
   - corridas temporales con `Assert.Fail`;
   - MAX_PATH;
   - un solo «Actualizar» lógico.
7. **§8: las decisiones del Owner,** las tomadas con su cita y **toda decisión material nueva**.
8. **§9: los límites.**

## Salidas, en `<scratchpad>\hs65\pbt3\`

- `I-64-f4b-pbt3-gate-contract-package-v1.md`;
- `pbt3-measurements.md`.

Ambos en español, sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Tu respuesta final** es corta:
- las rutas;
- si propones dividir PB-T3, y en qué;
- los recuentos principales;
- **las decisiones materiales nuevas para el Owner**;
- lo que no pudiste medir.
