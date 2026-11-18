> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: revisión adversarial independiente del paquete del contrato de I-64 F4B PB-T1, v1

Eres un revisor **independiente y adversarial**, en solo lectura. No participaste en la redacción.

**El objeto:** el paquete v1 de PB-T1, la tarea que extrae a Application el estado editable de la ventana clásica de Push Back. El redactor propone dividirla en tres subtareas secuenciales:
- **PB-T1a:** la red de caracterización, con un RED por mutantes;
- **PB-T1b:** el tipo `PushBackEditableState` en Application;
- **PB-T1c:** la ventana pasa a usarlo.

Si tu revisión da 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva, el paquete se emite **sin volver al Owner**, por su autorización condicional (F4B-E09, pregunta 4). **Eres la barrera.** Intenta refutar cada decisión.

## Qué pide el Owner (literal, [OD], fila de PB-T1)

> ACCEPT IN PRINCIPLE. PB-T1 extracts a lossless Push Back editable state outside the WPF window. Hard requirements: module state preserved; parked queue preserved; A/B preserved; no aliasing; no WPF control becomes source of truth; state round-trip lossless for supported classic racks; classic RACKEDITAR still works through the extracted state. Do not rewrite the entire editor merely to obtain an MVVM architecture. The goal is state separation, not aesthetic refactoring.

Las decisiones que condicionan PB-T1 (F4B-E09) están en `<scratchpad>\hs65\pbt1\owner-pbt1-pre-decisions.md`:
- c-02 como regla explícita del estado;
- `pb-21-cola` fuera de la garantía;
- la autorización condicional de emisión.

**El estado de PB-TC2,** de la que depende PB-T1: la ha parado su Worker en la medición previa (PBTC2-C14). Está pendiente de una decisión del Owner (`…\hs65\pbtc2\owner-return-c14.md`).

## Qué revisas

En `<scratchpad>\hs65\pbt1\`:
- `I-64-f4b-pbt1-gate-contract-package.md`, la v1;
- `pbt1-measurements.md` y los scripts que cita (`members.py`, `count_tests.py` y `forbidden_scope.py`);
- `drafting-brief.md`: lo que pidió la sesión;
- `ob065-dossier.md`, el expediente de la vuelta al Owner.

**El worktree**, en solo lectura: `<worktree-de-I-64>`, HEAD `eedb069c`, con `git -C`. Allí están las autoridades:
- el paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md`;
- la orden del Owner y las decisiones de procedimiento, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`;
- `F4B-DRAFTS/f4b-dispositions.md`;
- los mapas MA y MB, en `F4B-DRAFTS/package/`;
- los paquetes y cierres de PB-T0 y PB-TC, y el paquete de PB-TC2, en `docs/initiatives/` y `…/I-64-pilot/`;
- T1B;
- el protocolo `docs/AUTOMATION_PLAN.md` §16 y `docs/automation/agent-execution/`;
- el código y las pruebas.

La CI se consulta con `gh`, solo de lectura.

## Reglas

- **Solo lectura:**
  - sin compilar;
  - sin ejecutar pruebas;
  - sin escribir en el worktree;
  - sin operaciones de Git que escriban.
- **Python** solo de lectura, escrito con la herramienta de archivos y nunca con `-c`.
- **Sin AutoCAD.**
- **El informe:** en español, en `pbt1/independent-review-v1.md`, sin rutas absolutas locales ni nombres de usuario o de máquina. Cita el paquete como `T1:n`.

## Qué tienes que atacar como mínimo

1. **La fidelidad a [OD], a la fase y a F4B-E09.**
   - ¿Cumple cada requisito duro? Mira sobre todo «no aliasing», con la lectura L-1 del paquete, y «no WPF control becomes source of truth» tras PB-TC y PB-TC2.
   - ¿Hay algo de MVVM o de estética que sobre?
   - ¿Se cuela algo de PB-T2 (el modo WORKSPACE), de PB-T3 o de PB-T4?
   - ¿OB-03.1, el rechazo del piloto, se respeta?
2. **La división en tres subtareas** (la lectura L-3).
   - ¿Está justificada?
   - ¿Cambia el plan de la fase de un modo que deba decidir el Owner?
   - ¿El procedimiento propuesto (una revisión para las tres y después comprobaciones acotadas) es compatible con [DP], D-PB-10 y OB-10.1?
   - ¿Cada subtarea es cerrable por sí misma y no deja la rama en un estado roto?
3. **PB-T1a:** la red por mutantes.
   - ¿La tabla de mutantes discrimina?
   - ¿Los CT fijan lo que dicen, sin tautologías?
   - ¿Cubren de verdad D-5, M-03, M-04, M-07, D-3 y las huellas de salida que PB-T1c no puede cambiar?
   - ¿Hay mutantes en zonas que PB-TC2 va a tocar? Esos hay que volver a medirlos.
4. **PB-T1b:** el tipo en Application.
   - El contenido de `PushBackEditableState`, sus cargas y su salida frente a la semántica de hoy.
   - El copiador y el comparador completos (módulos, cola y A/B).
   - El mapa de propiedad.
   - La regla de c-02 (la lectura L-2: comparación ordinal frente a `OrdinalIgnoreCase`).
   - La exclusión de `pb-21-cola`.
   - `PushBackEditorOutputGate`.
   - ¿El RED con un esqueleto inerte falla por aserción?
   - ¿La guarda del censo por reflexión del piloto (`WorkspaceDraftPilotStateTests`) se respeta de verdad?
5. **PB-T1c:** la ventana sobre el estado.
   - ¿«Sin cambio observable» es verificable con las huellas de PB-T1a?
   - ¿Los reenvíos con el mismo nombre conservan todas las costuras que usan las pruebas?
   - ¿Las guardas de T1B y de texto siguen en verde con este diseño? Mira S-9, CT-22, CT-25, G-10, Fact3, I-55, el desviador y las demás del §4 del expediente.
   - ¿Alguna exige volver al Owner por OB-T1B.3?
6. **Los contratos propuestos de las tres subtareas.**
   - Los alcances, los `ForbiddenWriteScope`, los `RequiredTests` y su línea base.
   - Los invariantes mecánicos.
   - Las paradas, con las de [DP] literales y la variación de nc1 de PB-TC como riesgo.
   - Las celdas y su vigencia.
7. **Las lecciones de PB-T0, PB-TC y PB-TC2:**
   - sin latitud;
   - evidencia con rutas relativas y `TreeSha` real;
   - corridas temporales con `Assert.Fail`;
   - el aviso de descarte sustituido;
   - nunca la suite completa de UI en local;
   - MAX_PATH;
   - nombres sin choques.
8. **Las anclas:** comprueba una muestra amplia.
9. **Las decisiones materiales nuevas para el Owner.** Sé estricto con L-1, L-2 y L-3, y con todo lo que cambie el plan o un comportamiento observable.

## Formato del informe

1. Objeto, método y límites.
2. Un apartado por punto, del 1 al 9, con su evidencia.
3. La tabla de hallazgos: Id, gravedad (BLOQUEANTE, IMPORTANTE o MENOR), subtarea, contenido, evidencia y corrección.
4. El veredicto, con el recuento B/I/M y las decisiones nuevas para el Owner, cada una con sus opciones y la recomendada:
   - **EMITIBLE**;
   - **EMITIBLE TRAS CORRECCIONES**;
   - **NO EMITIBLE**.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, cuántas decisiones nuevas hay para el Owner y la ruta del informe.
