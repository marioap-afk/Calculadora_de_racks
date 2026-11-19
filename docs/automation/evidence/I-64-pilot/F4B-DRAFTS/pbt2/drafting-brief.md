> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: paquete v1 del contrato de PB-T2 (I-64 F4B): aplicar, cerrar y escribir separados; los dos modos

Eres el redactor del paquete de contrato de PB-T2. Trabajas **en solo lectura** y **no emites nada**. El paquete pasará después por una revisión independiente adversarial, y se volverá a medir sobre el cierre de PB-T1c antes de emitirse.

## Lo que pide el Owner (literal, [OD], fila de PB-T2 del §5 del paquete de la fase)

> ACCEPT IN PRINCIPLE. Separate: "apply edits to current editing state" from: "close editor" and from: "write authored DWG state". Workspace mode must return edited state to DraftSession. Classic RACKEDITAR may continue its compatible existing outer workflow. Avoid two independent implementations of field validation.

**El contenido que la fase asigna a PB-T2** (§5 del paquete de la fase):
- separar «aplicar al estado», «cerrar» y «escribir»;
- el modo explícito (§4), con la guarda de «sin camino clásico»;
- el dueño de los subdiálogos lo decide el host;
- una sola validación de campos;
- la prueba UI-STA de OB-01.3;
- el paso 4 de D-PB-06 en WORKSPACE, que PB-TC dejó pendiente (OB-06.3).

**Depende de PB-T1.** PB-T1 son tres subtareas:
- **PB-T1a:** la red de caracterización. CERRADA.
- **PB-T1b:** `PushBackEditableState` en Application, el copiador, el comparador, el mapa de propiedad y `PushBackEditorOutputGate`. En cierre; su `src/` final es el de `2f5d3dec`.
- **PB-T1c:** la ventana guarda su estado en ese tipo (D-PBT1-13). AÚN NO EMITIDA.

**Diseña PB-T2 sobre la ventana que dejará PB-T1c según D-PBT1-13** del paquete de PB-T1 v2.7. Mide hoy sobre `2f5d3dec` y marca cada ancla de W y WC como «a volver a medir sobre el cierre de PB-T1c».

## Decisiones del Owner que condicionan PB-T2

- **D-PB-07** (2026-11-19, literal «BOM y biblioteca (Recomendado)»). En modo WORKSPACE, el editor ofrece «BOM» (con su exportación) y «Guardar en biblioteca», y oculta Insertar y el lote de vistas (D-PB-01, OB-01.6). Obligaciones que el contrato debe llevar:
  - dos costuras de prueba nuevas, para la ventana del BOM y para el diálogo de guardado, sin romper los literales de S-7, S-9 y CT-22. Si no pueden quedar en verde, se vuelve al Owner antes del contrato (OB-T1B.3);
  - la prueba UI-STA de OB-01.3 pulsa BOM y biblioteca en modo WORKSPACE y comprueba:
    - ninguna petición clásica (`InsertRequested` false, `InsertionRequest` null);
    - el borrador sin cambios hasta «Aplicar al borrador»;
    - el GUID del borrador sin cambios;
    - el archivo de biblioteca con los campos desconocidos del rack (I-11);
  - el dueño de esas dos ventanas hijas en WORKSPACE lo diseña PB-T2.

  Registro y expediente: `hs65\pbfull\owner-f4b-dpb07-extensiondata-decisions.md` y `hs65\pbfull\owner-return-dpb07.md`, este último con anclas medidas en `2f5d3dec`.
- **El resto de decisiones de F4B**, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/` y `F4B-DRAFTS/f4b-dispositions.md` (F4B-E01 a F4B-E22), en particular:
  - [OD], `owner-f4b-disposition.md`;
  - [DP], `owner-f4b-procedure-decisions.md`;
  - D2 (`owner-d1-d2-decisions.md`) y su arrastre A-2 a PB-T2, PB-T3 y PB-T4 (§10 del paquete de PB-T1);
  - F4B-E21: las colecciones opacas del contrato las pone la sesión, lo que afecta al tamaño del contrato.

## Cómo leer (obligatorio)

Ahora mismo hay otro proceso trabajando en el worktree de la rama. Está **PROHIBIDO** leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- Lee el repositorio SOLO por objetos git desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show 2f5d3dec:<ruta>`, y también `grep -n <patrón> 2f5d3dec -- <rutas>`, `ls-tree`, `log` y `diff`. No leas los archivos de ese checkout en disco.
- Sin compilar ni ejecutar pruebas. Sin órdenes git que escriban. Sin AutoCAD.
- Python solo de lectura, escrito con la herramienta Write y nunca con `-c` ni heredoc.

## Fuentes

- **El paquete de la fase:** `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1, entero. Lo que te toca:
  - §4: los dos modos, la guarda «sin camino clásico» y la tabla de la línea ~479;
  - OB-01.x, en especial OB-01.1b, 01.3, 01.5 y 01.6;
  - OB-06.3; OB-07.1; D-PB-01; D-PB-06; D-15;
  - H-PB-30 a 33; R-PB-09; R-PB-10.
- **El paquete de PB-T1 v2.7** (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`, también en `hs65\pbt1\I-64-f4b-pbt1-gate-contract-package-v27.md`):
  - el diseño de PB-T1c (D-PBT1-13), sobre el que se apoya PB-T2;
  - D-PBT1-14 y D-PBT1-15 (D2);
  - §4 (las guardas de T1B);
  - §5.5 (el censo);
  - §10 (los arrastres a PB-T2).
- **Los mapas:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/package/map-a-classic-write-path.md` y `map-b-editor-state-testability.md`.
- **PB-T0, PB-TC y PB-TC2:**
  - sus paquetes en `docs/initiatives/I-64-f4b-*`;
  - sus cierres `…/task-close-package.md` y sus arrastres;
  - el paso 4 de D-PB-06 pendiente de PB-TC.
- **El código de hoy:**
  - W y WC, el XAML y `RackEditorSession`;
  - el Plugin: `RackPushBackCommands`, RACKEDITAR y los cuatro sitios;
  - el piloto Workspace de Push Back (`src/RackCad.Application/Systems/PushBack/Pilot/` y `src/RackCad.UI/Workspace/`);
  - el núcleo de borradores (`src/RackCad.Application/Workspace/Drafts/`);
  - `PushBackEditableState` y `PushBackEditorOutputGate`.
- **T1B:** `docs/initiatives/I-64-f4-t1b-gate-contract-package.md` y sus guardas.
- **El protocolo y las plantillas:**
  - `docs/AUTOMATION_PLAN.md` §16;
  - `docs/automation/agent-execution/README.md`;
  - los contratos `docs/automation/evidence/I-64-pilot/F4B-*/coordinator/gate-contract.json` y sus generadores.

## Qué tiene que contener el paquete v1

1. **Los hechos medidos** sobre `2f5d3dec`:
   - hoy, cómo se mezclan aplicar, cerrar y escribir en la ventana (`RequestDraw`, Restaurar, cerrar, `InsertRequested`, `InsertionRequest` y la sesión);
   - cómo vuelve el estado al llamador en RACKEDITAR;
   - dónde se valida cada campo; qué subdiálogos hay y quién es su dueño;
   - qué hace el piloto Workspace hoy y qué le falta para recibir el estado editado;
   - qué guardas de T1B, de texto y de I-55 tocan esas zonas.
2. **El diseño:**
   - el modo explícito (CLASSIC o WORKSPACE): cómo se fija, quién lo elige y la guarda de que WORKSPACE nunca llega al camino clásico de escritura;
   - «Aplicar al borrador», que devuelve el estado editado a DraftSession por un puerto;
   - «Cerrar»;
   - la ocultación de Insertar y del lote en WORKSPACE;
   - BOM y biblioteca en WORKSPACE (D-PB-07), con sus costuras;
   - una sola validación de campos;
   - el dueño de los subdiálogos;
   - el paso 4 de D-PB-06 en WORKSPACE;
   - lo que NO hace PB-T2: «Editar todo» en el panel es de PB-T4, y la escritura completa, de PB-T3.

   Si la tarea es demasiado grande para un Worker, propón subtareas secuenciales (PB-T2a, PB-T2b…), cada una con su 1/3.
3. **La red de pruebas:**
   - lo que sigue en verde sin tocarlo;
   - las pruebas nuevas (CT-PBT2-xx), incluida la UI-STA de OB-01.3 con BOM y biblioteca;
   - el RED y el GREEN, con su forma exacta;
   - el censo actualizado.
4. **Las guardas de T1B y de texto** (OB-T1B.3), una por una. Si alguna no puede seguir en verde, es una decisión del Owner antes del contrato: va al §8.
5. **El contrato propuesto,** por subtarea, con el patrón de PB-T1:
   - TaskId; AllowedWriteScope; ForbiddenWriteScope; RequiredTests;
   - invariantes mecánicos; ExpectedEvidence; StopConditions; EligibleCells;
   - la línea base de la CI.

   Ten en cuenta las lecciones de PB-T1b:
   - una lista cerrada de miembros debe admitir explícitamente los ayudantes privados necesarios, o declararlos parada;
   - la CI del RED con Core en failure deja `build-plugin` omitido;
   - en una corrección, las rutas UNIT_CHANGE ya cambiadas en la base (AUTOMATION_PLAN 16.3).
6. **Los riesgos y las lecciones:**
   - sin latitud para el Worker;
   - corridas temporales con `Assert.Fail`;
   - el aviso de descarte sustituido;
   - nunca la suite completa de UI en local;
   - MAX_PATH;
   - un contrato que no supere el tamaño que admite la salida del Controller (F4B-E21).
7. **§8: las decisiones del Owner,** las tomadas con su cita y **toda decisión material nueva**.
8. **§9: los límites,** con lo que no pudiste medir.

## Salidas, en `<scratchpad>\hs65\pbt2\`

- `I-64-f4b-pbt2-gate-contract-package-v1.md`;
- `pbt2-measurements.md`, con las mediciones y sus órdenes.

Ambos en español, sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Tu respuesta final** es corta:
- las rutas;
- si propones dividir PB-T2, y en qué;
- los recuentos principales;
- **las decisiones materiales nuevas para el Owner**;
- lo que no pudiste medir.
