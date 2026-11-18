> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: la v2.2 del paquete de PB-T1, con la decisión D2 del Owner (L-1)

Lo da la sesión principal al redactor.

**El Owner resolvió L-1** (F4B-E11; texto literal en `<scratchpad>\hs65\pbtc2\owner-d1-d2-decisions.md`; léelo entero):

> **D2 = ACCEPT TWO-REFERENCE ALIAS EXCEPTION**

Esa decisión incluye **10 condiciones de seguridad** y un **límite**: «This is NOT authorization for general shallow copying… If later PB-T1/PB-T2/PB-T3 needs to mutate either … inside the Workspace draft lifetime: STOP and reopen L-1».

**También aceptó en principio la división en tres subtareas,** con una condición: «Re-measure it against the final PB-TC2 product before emission».

## REGLA ESPECIAL DE ESTE ENCARGO: NO TOQUES EL WORKTREE

Ahora mismo hay un Worker de PB-TC2 trabajando en el worktree. Cualquier orden tuya que mencione la ruta del worktree (`git -C`, `ls`, `grep`, `python` sobre sus archivos…) aparece como proceso participante y **invalida su relevo (P-08)**.

En este encargo trabajas **solo** con los archivos de `…\scratchpad\hs65\pbt1\` y `…\scratchpad\hs65\pbtc2\`: tu v2.1, tus mediciones, las revisiones y las decisiones.

- **Ninguna lectura** del worktree, ni de `artifacts/` ni del checkout principal.
- **Si un punto necesita una medición nueva del código,** márcalo como «A MEDIR EN LA REMEDICIÓN SOBRE EL GREEN DE PB-TC2» y sigue.

## Qué haces

1. Copia la v2.1 a `I-64-f4b-pbt1-gate-contract-package-v22.md`. No toques la v2.1.
2. **Incorpora D2 de forma completa y mecánica:**
   - **L-1** pasa a «decidido: (a), excepción declarada (F4B-E11, D2)», con la cita literal del Owner. El anexo L1-C queda como «no elegido», con una línea.
   - **Las 10 condiciones de seguridad,** una a una. Para cada una:
     - qué subtarea la prueba (PB-T1a, PB-T1b o PB-T1c);
     - con qué CT, invariante o guarda de texto;
     - o, si depende de un concepto que todavía no existe en PB-T1, a qué tarea posterior se arrastra como obligación de contrato, y con qué parada.

     Ese es el caso de `DraftSession`, de `InitialDraftState` y `CurrentDraftState`, y de la operación de texto pendiente del Workspace, que son de PB-T2 y PB-T4. Aun así, PB-T1 debe probar todo lo que su API ya permite. Por ejemplo, que validar, editar el estado, descartar o recargar, o comparar dos estados no mutan los dos objetos compartidos.
   - **La condición 1:** enumera los dos tipos exactos y los miembros que los contienen.
   - **La condición 9:** la prueba que muta todo el estado editable ordinario de la copia y comprueba que el original no cambia.
   - **La condición 10:** una guarda de texto o de código, con su autoprueba en negativo, que detecte cualquier mutación nueva del rack resuelto o del informe de módulos dentro del camino de edición. Defínela con precisión (qué archivos, qué patrones), o marca qué falta medir.
   - **La condición 8:** el ajuste de nombre del Plugin (`system.Name`, en los tres escritores medidos) queda fuera del tiempo de vida de la edición. Di cómo lo prueba o lo declara cada subtarea.
   - **El límite:** una parada en el contrato de cada subtarea («si hiciera falta mutar uno de los dos objetos dentro del tiempo de vida del borrador o de la edición: STOP y se reabre L-1») y el arrastre A-2 a PB-T2, PB-T3 y PB-T4, con el texto literal del Owner.
3. **Las condiciones de emisión:**
   - el cierre de PB-TC2;
   - la remedición sobre su GREEN;
   - si la remedición no cambia ninguna premisa material, la emisión de PB-T1a «normally» (F4B-E11).

   Lista lo que la remedición tiene que comprobar: las anclas de las zonas que toca PB-TC2, los mutantes afectados, la línea base de la CI y los recuentos.
4. **El registro de cambios v2.1 → v2.2,** al principio.
5. **Las decisiones nuevas para el Owner:** si al incorporar D2 aparece alguna (por ejemplo, una condición que PB-T1 no pueda probar ni arrastrar sin cambiar el plan), dilo explícitamente.

**Tu respuesta final** es corta:
- la ruta;
- la tabla «condición → subtarea → prueba, guarda o arrastre»;
- lo que queda a medir en la remedición;
- cualquier decisión nueva.
