> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: revisión adversarial independiente del paquete del contrato de I-64 F4B PB-TC (correcciones declaradas), v1, antes de emitirlo

Eres un revisor **independiente y adversarial**. Te dan el paquete de contrato de una tarea que todavía no se ha emitido: corrige dos pérdidas de la ventana clásica de Push Back (el redondeo de once cajas y el estilo de cotas) y añade un aviso no bloqueante. Tu trabajo es encontrar los defectos que harían fracasar la tarea o que violarían la orden del Owner o el paquete de la fase:
- un diseño de la corrección que no corrige todo lo que dice, o que cambia más de lo declarado;
- pruebas que no fijan lo que dicen, que no fallan en la base por aserción o cuyas líneas esperadas no se cumplirían tras la corrección;
- sustituciones de pruebas existentes incompletas, de más o sin la forma del patrón de I-39;
- alcances de escritura incompletos o demasiado anchos;
- invariantes que no se pueden verificar;
- conflictos con guardas y pruebas existentes;
- anclas falsas;
- contradicciones con el paquete de la fase o con la orden del Owner;
- decisiones sin autoridad.

**Intenta refutar cada decisión. No es tu trabajo confirmar.** El paquete se redactó leyendo código sin compilar ni ejecutar nada: sus líneas esperadas son predicciones. No te fíes de ninguna.

## Qué revisas

En `<scratchpad>\hs65\pbtc\`:
- **El paquete:** `I-64-f4b-pbtc-gate-contract-package.md`, la v1.
- **Su registro de mediciones:** `pbtc-measurements.md`.

**El worktree**, en solo lectura: `<worktree-de-I-64>`, HEAD `f9f2afbb`. Allí están las autoridades:
- **El paquete de la fase**, v3.1: `docs/initiatives/I-64-f4b-push-back-completo-package.md`. Sobre todo OB-06.1 a OB-06.6, OB-08.1 a OB-08.4, la fila de PB-TC del §5, OB-T1B.1 a OB-T1B.3, OB-RE.1 y el §9.
- **La orden literal del Owner:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md` (D-PB-06 con sus seis pasos, D-PB-08 y D-PB-10).
- **Las decisiones de procedimiento:** `…/F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md` y `…/F4B-DRAFTS/f4b-dispositions.md` (F4B-E01 a F4B-E05).
- **La plantilla y la tarea anterior:**
  - el paquete de PB-T0, `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`;
  - su contrato, `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json`;
  - su cierre, `…/F4B-PBT0-CHARACTERIZATION/task-close-package.md`, con los arrastres a PB-TC y las lecciones;
  - la entrega de su Worker, `…/F4B-PBT0-CHARACTERIZATION/1/R20261117T185616Z-3c1b/worker-handoff.json`;
  - su revisión independiente, `…/1/R20261117T215307Z-3cff/independent-review.md`.
- **El patrón de sustitución de I-39:** `docs/automation/evidence/I-39B-caracterizacion-base-vs-contrato.md` y los `Skip` de `tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs`.
- **El protocolo:** `docs/AUTOMATION_PLAN.md` §16. **El catálogo de celdas:** `docs/automation/agent-execution/model-catalog.md`. **Los esquemas:** `docs/automation/agent-execution/schemas/`.
- **La CI de la rama** se puede consultar con `gh run view` y `gh run list` (solo lectura).

## Reglas

- **Solo lectura:**
  - no compiles;
  - no ejecutes pruebas;
  - no modifiques nada del worktree;
  - ninguna operación de Git que escriba.
- **Se permite:**
  - `git -C <worktree>` con `show`, `diff`, `log`, `grep`, `ls-files` y `rev-parse`;
  - leer archivos;
  - `gh` de solo lectura;
  - scripts de Python de solo lectura en la carpeta del informe, escritos con la herramienta de archivos y no con un heredoc.
- **Sin AutoCAD.**
- **El informe:** en español, en `independent-review-v1.md` dentro de esa misma carpeta. Sin rutas absolutas locales ni nombres de usuario o de máquina. Cita las líneas del paquete como `TC:n`. No crees nada más.

## Qué tienes que atacar como mínimo

1. **Cobertura de la orden y de la fase.**
   - ¿El paquete cumple los seis pasos de D-PB-06 y OB-06.1 a OB-06.6, y D-PB-08 con OB-08.1 a OB-08.4?
   - ¿Es legítimo dejar para PB-T2 el paso 4 de D-PB-06 y el aviso en modo WORKSPACE? Contrástalo con la letra de la orden («in both modes») y de la fase.
   - ¿Hay algo que la fase asigne a PB-TC y el paquete no haga, o algo que haga sin que nadie se lo asigne?
2. **El diseño de (a), el redondeo** (D-PBTC-01).
   - ¿La regla `Keep` corrige las once cajas en todas las rutas: carga, «Restaurar», selección de celda, «Ambos lados» y `SetMixed`, alcance y datos del frente?
   - Busca **todo** lector de esas once cajas en `src/` (`.Value`, `.Text`, validaciones, espejos, diálogos de edición masiva o de alcance, comandos). ¿Alguno queda fuera y sigue redondeando o pasa a comportarse distinto?
   - Con la selección de celda: ¿en qué orden se ejecutan `CommitCurrentCell`, la siembra de `LoadSelectedFront` y la pasada? ¿Podría `Keep` comparar la caja con el valor de **otra** celda o de otro lado y escribir un valor equivocado?
   - ¿El cambio de `OptionalVal` conserva su semántica de nulo (el comentario de W:3615-3625)?
   - ¿La cultura de la comparación es correcta frente a la de `SetNumber`?
   - ¿«Escribir el mismo número que se ve conserva el exacto» y las demás consecuencias declaradas son decisiones que el paquete puede tomar o son del Owner?
3. **El diseño de (b), el estilo de cotas** (D-PBTC-02).
   - ¿Pasan todas las cargas por `LoadFromModel`? ¿Algún camino reinicia o reconstruye `Annotations` sin el portador?
   - ¿La cadena del estilo hasta cada cota que cita H-12 es la que usa **Push Back**, o la del Dinámico? Comprueba qué constructor de decoraciones usa Push Back y si sus cotas llevan `DimensionStyleName`.
   - ¿Es correcta la descripción del efecto en el dibujo (H-13) que se da al Owner?
4. **D-PB-08, el aviso** (D-PBTC-03).
   - ¿La regla de visibilidad coincide con la regla de capacidad sin presencia del estado (`PushBackCompositeEditorState`) en todos los casos, incluido un lado B dormido?
   - ¿Se evalúa tras todo gesto que cambia la presencia? ¿Hay un gesto que la cambie sin pasar por una pasada efectiva de `Recompute`?
   - ¿Los rótulos que cita el texto («Lado B», «En blanco») existen así en la interfaz? ¿Es exacto «así no se guardará»?
   - ¿El elemento propuesto choca con alguna guarda del XAML o del shell visual: tokens de I-30 o I-39 (tamaños de letra, márgenes, pinceles), recursos, censos de elementos con nombre, el censo control → prueba de PB-T0? Busca en `tests/` todo lo que lea el XAML de la ventana o enumere sus elementos.
5. **Cada CT** (CT-PBTC-01 a CT-PBTC-09).
   - ¿Falla en la base **por aserción**, y no por excepción, con la primera diferencia que dice el §5.1?
   - ¿Sus líneas esperadas se cumplirían de verdad tras la corrección? Rehaz por el código, al menos:
     - CT-PBTC-03 paso a paso;
     - las líneas de CT-PBTC-07, frente a lo que miden de verdad CT-PB0-07 y CT-PB0-09;
     - el `delta=[]` de CT-PBTC-06 en las 14 filas, frente a lo que filtra o no el recorrido de WLC y el de S0;
     - CT-PBTC-02 (iii) y CT-PBTC-05.
   - ¿Es determinista, sin catálogo de máquina, orden de iteración ni cultura?
   - ¿Usa solo costuras existentes? ¿Respeta las reglas de ventanas cerradas en `finally`, sin `MessageBox` real, BOM, configurador real, «Guardar en biblioteca» ni «Insertar»?
   - ¿CT-PBTC-09 prueba de verdad que Cantilever no cambia, o es decorativo?
6. **Las sustituciones** (S-01 a S-03).
   - ¿Son exactamente las pruebas que chocan? Busca en `tests/` cualquier otra que fije el redondeo, la pérdida del estilo, la ausencia de un elemento con nombre o un recuento que el cambio movería.
   - ¿La prueba nueva cubre todo lo que cubría la vieja (filas, ramas, construcción)?
   - ¿La forma (solo la línea del atributo, el literal del `Skip`, el registro) sigue el patrón de I-39?
   - ¿Cómo cuenta xUnit una `Theory` con `Skip` y `MemberData`? ¿Cuadran los recuentos del §6.4 y del §6.9?
7. **El contrato.**
   - ¿El `AllowedWriteScope` basta y no sobra?
   - ¿El `ForbiddenWriteScope` cubre lo demás con el método de T1B y PB-T0, sin dejar fuera nada que importe? ¿Existen todas sus rutas?
   - ¿Los invariantes son verificables mecánicamente? ¿El criterio del RED es aplicable a estos ocho métodos?
   - ¿`RequiredTests`: sus filtros seleccionan lo que dicen, sin colisiones, y los `MinSelected` salen de lo que de verdad hay? Comprueba el namespace y los nombres de las clases de Cantilever, de T1B y de PB-T0.
   - ¿Las paradas cubren [DP], D-PB-10 y las vueltas al Owner? ¿Las transcripciones de PD-F4-01 y PD-F4-04 son literales frente a sus fuentes?
   - ¿Las celdas elegibles siguen vigentes?
   - ¿La línea base de la CI y la de avisos son las que dicen las fuentes?
8. **Guardas existentes.** ¿Alguna prueba o guarda del repo fallaría con el RED o con el GREEN? Revisa, además de las del H-18, todo lo que lea el código o el XAML de la ventana. ¿El paquete las lista todas?
9. **Lecciones de PB-T0** (su cierre, §5). ¿El paquete aplica las que importan?
   - M-01: las ventanas vivas si la carga lanza.
   - M-05: las listas de CT-PB0-01 y CT-PB0-17.
   - La lección «el prompt del Worker no puede dar latitudes que el contrato no tiene»: ¿hay algún punto del contrato que deje al Worker una elección material?
   - Las demás de T1B y PB-T0 que apliquen (F4-E59, MAX_PATH, teclado y foco).
10. **Las anclas.** Comprueba una muestra amplia de `ruta:línea` del paquete y del registro de mediciones.
11. **La vuelta al Owner** (§8).
    - ¿Las cuatro decisiones están planteadas de forma completa, fiel a lo medido y sin sesgo?
    - Contrasta la tabla de OD-PBTC-1 con lo que midió de verdad PB-T0 (T-3 en la entrega del Worker y CT-PB0-21), y la lectura OWNER-4 de su revisión independiente.
    - ¿H-PB-37 («el panel del Workspace ya escribe hoy los valores exactos y el estilo») es cierto?
    - ¿Hay alguna decisión material, de producto o de procedimiento, que el paquete tome sin autoridad y que deba ir al Owner antes de emitir?

## Formato del informe

1. Objeto y método, y lo que no pudiste verificar.
2. Un apartado por cada punto, del 1 al 11, con su evidencia.
3. La tabla de hallazgos: Id, gravedad (BLOQUEANTE, IMPORTANTE o MENOR), contenido, evidencia y corrección propuesta.
4. El veredicto, con el recuento B/I/M y lo que deba volver al Owner:
   - **EMITIBLE**;
   - **EMITIBLE TRAS CORRECCIONES**;
   - **NO EMITIBLE**.

**Tu respuesta final**, que vuelve a la sesión, es una sola línea: el veredicto, el recuento y la ruta del informe.
