> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: revisión adversarial independiente del paquete del contrato de I-64 F4B PB-T0 (bloque C), v1, antes de emitirlo

Eres un revisor **independiente y adversarial**. Te dan el paquete de contrato de una tarea de caracterización que todavía no se ha emitido. Tu trabajo es encontrar los defectos que harían fracasar la tarea o que violarían la orden del Owner o el paquete de la fase:
- puntos de caracterización que no fijan lo que dicen;
- mutantes que no discriminan, que no compilan o que cambian algo fuera de su clase;
- alcances de escritura incompletos o demasiado anchos;
- invariantes que no se pueden verificar;
- conflictos con guardas y pruebas existentes;
- anclas falsas;
- contradicciones con el paquete de la fase;
- decisiones sin autoridad.

**Intenta refutar cada decisión. No es tu trabajo confirmar.**

## Qué revisas

Todo lo que sigue está en `<scratchpad>\hs65\pbfull\`:
- **El paquete:** `I-64-f4b-pbt0-gate-contract-package.md`, la v1.
- **El paquete de la fase que manda,** v3.1: `I-64-push-back-completo-package-v3.md`. Sobre todo:
  - §6, PB-T0: §6.1 la forma, §6.2 la tabla del bloque C, el corpus, el censo y las reglas;
  - §7.2: en F4B, un rechazo de la planificación y un control nc fallido vuelven al Owner, y no hay sustitutos;
  - §7.3: T1B;
  - OB-06.3b, OB-06.5 y OB-06.4.
- **La orden literal del Owner:** `owner-f4b-disposition.md`.
- **Los mapas:** `map-a-classic-write-path.md` y `map-b-editor-state-testability.md`. No te fíes de ellos.

**El worktree**, en solo lectura: `<worktree-de-I-64>`, HEAD `67e28452`. El producto es el de `5ccaa770`: entre los dos, `src/` y `tests/` no cambian. Allí están:
- la plantilla que el paquete dice seguir: `docs/initiatives/I-64-f4-t1b-gate-contract-package.md` y `docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/` (`coordinator/gate-contract.json` y `task-close-package.md`, con las lecciones);
- el protocolo: `docs/AUTOMATION_PLAN.md` §16;
- el catálogo de celdas: `docs/automation/agent-execution/model-catalog.md`;
- el esquema del contrato y de la delegación: `docs/automation/agent-execution/schemas/`.

## Reglas

- **Solo lectura:**
  - no compiles;
  - no ejecutes pruebas;
  - no modifiques nada del worktree;
  - ninguna operación de Git que escriba.
- **Se permite:**
  - `git -C <worktree>` con `show`, `diff`, `log`, `grep`, `ls-files` y `rev-parse`;
  - leer archivos;
  - scripts de Python de solo lectura en la carpeta del informe, escritos con la herramienta de archivos y no con un heredoc.
- **Sin AutoCAD.**
- **El informe:** en español, en `pbt0-independent-review-v1.md` dentro de esa misma carpeta. Sin rutas absolutas locales ni nombres de usuario o de máquina. Cita las líneas del paquete como `C1:n`. No crees nada más.

## Qué tienes que atacar como mínimo

1. **Cobertura del §6.2 de la fase.**
   - ¿Cada fila que «se fija» tiene su CT, y cada CT fija de verdad lo que la fila pide?
   - Revisa la propiedad del estado con la segunda pasada, los módulos, la cola, A/B, frentes con cortes y compuesto, los módulos por reconstrucción y H-PB-28, renombrar, la decisión pura de borrado, los subdiálogos, las dependencias de catálogo y la enumeración de (c), (d) y clases nuevas, el `DimensionStyleName`, el redondeo campo a campo, el efecto del redondeo y el censo control → prueba.
   - ¿Son justos los recortes que hace el paquete (CT-PB0-18, la mitad Core de módulos)? Comprueba en el código que la cobertura existente que cita fija eso de verdad.
2. **Cada CT.**
   - ¿La aserción es concreta y determinista?
   - ¿Pasa sobre la base, que es lo que hay que caracterizar?
   - ¿Usa solo costuras existentes?
   - ¿Respeta las reglas: nada en `Fixtures/I64`, sin `Skip`, sin editar pruebas existentes, sin «Guardar en biblioteca», BOM, configurador real ni `MessageBox` real, `EditorDiscardPromptCollection` y determinismo?
   - ¿Alguna prueba dependería del catálogo de una máquina, del orden de iteración o de la cultura?
   - ¿«Leer el BOM calculado en Application no es ejecutar el BOM» es una lectura defendible con la regla del paquete de la fase y con INV-F4T1B-06?
3. **La tabla cerrada de mutantes.** Para cada mutante:
   - ¿existe la línea que cita;
   - ¿compila sin avisos nuevos (CS0162, CS0168, CS0169, CS0219, CS0414 o CS8321);
   - ¿lo detecta al menos un CT en su corrida aislada sobre la base, razonándolo por el código;
   - ¿es de la clase que su archivo permite;
   - ¿se revierte exactamente?
   - ¿Hay algún CT sin mutante que lo discrimine?
   - ¿Mutar Application rompe pruebas existentes de forma imposible de atribuir? Estima cuántas fallarían en el RED, y si eso hace inviable el criterio mecánico del RED de T1B.
4. **El contrato.**
   - ¿El `AllowedWriteScope` basta y no sobra?
   - ¿El `ForbiddenWriteScope` cubre todo lo demás con el método de T1B («prefijo solo si ninguna ruta permitida queda debajo»), sin dejar fuera a T1B, los helpers ni `Fixtures/`?
   - ¿Los invariantes son verificables mecánicamente?
   - ¿`RequiredTests`, sus filtros y `MinSelected` salen de lo que de verdad seleccionan los nombres de clase propuestos, sin colisionar con clases existentes? Comprueba que no colisionen con `PushBackClassicCharacterization*`.
   - ¿Las paradas cubren el §7.2 de la fase, es decir, la planificación y los nc fallidos que vuelven al Owner?
   - ¿Siguen vigentes las celdas elegibles en el catálogo?
5. **Guardas existentes.**
   - ¿Alguna guarda del repo fallaría con los archivos nuevos o con los mutantes? Revisa el corpus de WLC, RAM Elements, el namespace único, INV-14, el censo de ventanas, G-10, T-SG, T-KB y otras.
   - ¿El paquete las lista todas?
6. **Lecciones de T1B** (su `task-close-package.md`). ¿El paquete aplica las que importan?
   - El fallo estructural de nc2.
   - La atribución de las pruebas existentes que rompe el RED.
   - F4-E59.
   - MAX_PATH.
   - Las lecciones de teclado y foco.

   ¿El riesgo R-01, que nc2 casi seguro se para y vuelve al Owner, está bien planteado? ¿Hay forma de evitarlo dentro de las reglas?
7. **Las anclas.** Comprueba una muestra amplia de `ruta:línea` del paquete.
8. **La vuelta al Owner.** ¿Hay alguna decisión material, de producto o de procedimiento, que el paquete tome sin autoridad o que deba ir al Owner antes de emitir?

## Formato del informe

1. Objeto y método, y lo que no pudiste verificar.
2. Un apartado por cada punto, del 1 al 8, con su evidencia.
3. La tabla de hallazgos: Id, gravedad (BLOQUEANTE, IMPORTANTE o MENOR), contenido, evidencia y corrección propuesta.
4. El veredicto, con el recuento B/I/M y lo que deba volver al Owner:
   - **EMITIBLE**;
   - **EMITIBLE TRAS CORRECCIONES**;
   - **NO EMITIBLE**.

**Tu respuesta final**, que vuelve a la sesión, es una sola línea: el veredicto, el recuento y la ruta del informe.
