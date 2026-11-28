# Encargo: revisión adversarial independiente del paquete v2 de I-64 F4B (Push Back completo)

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`, `<worktree>`, `<carpeta-del-DLL-de-C2>`, `<datos-locales-del-usuario>`). El resto no cambia.

Eres un revisor **independiente y adversarial**. Te dan el paquete de una fase nueva que todavía no ha emitido ningún contrato. Tu trabajo es encontrar los defectos que harían fracasar la fase o que violarían la orden del Owner:
- textos del Owner mal copiados, debilitados o ampliados;
- obligaciones sin mecanismo;
- tareas que dejan un estado intermedio inseguro;
- anclas falsas;
- conflictos con guardas y caracterizaciones existentes;
- decisiones materiales que la sesión toma sin autoridad;
- una tarea de caracterización que no puede medir lo que promete.

**Intenta refutar cada decisión. No es tu trabajo confirmar.**

## Qué revisas

Todo lo que sigue está en la carpeta `<carpeta-del-paquete>\`:
- **El paquete:** `I-64-push-back-completo-package-v2.md`.
- **La orden que manda:** `owner-f4b-disposition.md`, la disposición literal del Owner y del Coordinator. Léela entera. El paquete debe cumplir cada uno de sus apartados.
- **La v1** que el Owner dispuso: `I-64-push-back-completo-package-v1.md`.
- **Los mapas de hechos** en que se apoya: `map-a-classic-write-path.md` y `map-b-editor-state-testability.md`. **No te fíes de ellos:** comprueba en el código todo lo que funde una decisión.

**El worktree**, en solo lectura: `<worktree>`, HEAD `5ccaa770`. Las autoridades que el paquete cita están en `docs/`. Las principales:
- `docs/initiatives/I-64-proposal-v5.md`: D-08, D-10, D-13, D-15, D-16, los INV y el §11;
- `docs/initiatives/I-64-implementation-clarifications.md`;
- `docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md`;
- `docs/initiatives/I-64-f4-t1b-gate-contract-package.md`;
- `docs/automation/evidence/I-64-pilot/F4-DRAFTS/gate-close.md` y `f4-dispositions.md`;
- `docs/AUTOMATION_PLAN.md` §16.

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
- **El informe:** en español, en `independent-review-v2.md` dentro de esa misma carpeta. Sin rutas absolutas locales ni nombres de usuario o de máquina; solo rutas relativas al repositorio, y `P2:n` para la línea n del paquete. No crees nada más.

## Qué tienes que atacar como mínimo

1. **Fidelidad a la orden.**
   - Compara **carácter a carácter** cada bloque citado como «Texto del Owner (literal)» con `owner-f4b-disposition.md`. ¿Falta, sobra o cambia algo?
   - ¿Cada obligación OB-n se deduce del texto, o lo amplía o lo debilita?
   - ¿Hay un apartado de la orden que el paquete omite: el modo explícito, «ZERO authored DWG writes», «Do not admit A/B racks before that state is lossless», los siete pasos de la reconciliación, «Do not require a generic JSON diff», los seis pasos de D-PB-06, la lista mínima de PB-T0, el aviso de T1B, RACKEDITAR o la autorización condicional?
2. **D-PB-07.** La orden no trae texto para D-PB-07.
   - ¿Es correcto dejarla abierta?
   - ¿Bloquea PB-T0?
   - ¿Hay algo en la orden que ya la decida y que el paquete no vea?
   - ¿Hay que volver al Owner antes de PB-T0?
3. **Los dos modos (§4).**
   - ¿El diseño garantiza que una invocación en modo WORKSPACE no puede llegar al camino clásico de escritura?
   - ¿El modo es de verdad explícito?
   - ¿Los rótulos de OB-01.6 y ocultar Insertar en WORKSPACE son compatibles con T1B en CLASSIC, y están declarados como exige la orden?
4. **La descomposición (§5).**
   - ¿PB-TC antes de PB-T1 es coherente con la orden («Do not hide these corrections inside PB-T1»)?
   - ¿Es una decisión material que tendría que tomar el Owner?
   - ¿La razón técnica (el redondeo) es cierta en el código?
   - ¿Puede PB-TC probar el punto 4 de D-PB-06 («prove both Workspace; RACKEDITAR») con lo que existe en su momento?
   - ¿Alguna tarea deja un estado intermedio inseguro: A/B admitidos antes de tiempo, una escritura sin barrera, un modo a medias?
   - ¿Faltan dependencias?
5. **D-PB-05.**
   - ¿El mapa de propiedad editor/sobre se puede derivar del código, o hay campos ambiguos?
   - ¿Qué pasa con `ExtensionData`, la identidad, el esquema y la sesión en el código actual de `ComposeForWrite` y `KeepPilotEditsOn`?
   - ¿El reparto PB-T1/PB-T3/PB-T4 deja algún paso de los siete sin dueño?
6. **PB-T0 (§6).**
   - ¿Cubre los catorce puntos de la lista de la orden y los nueve de las vistas?
   - ¿El método de cada punto (prueba automática, mapa o medición física) puede medir de verdad lo que promete, sin cambiar producto?
   - ¿La parte física es segura y está bien definida: qué DLL; quién la opera; sobre qué copias; los originales nunca se abren; nada se guarda sobre un original; las reglas del operador?
   - ¿Qué se fija con pruebas y qué solo se describe? ¿Es suficiente para que PB-T1, PB-T2 y PB-T3 no rompan nada sin que se vea?
   - ¿El modo de RED que propone es viable?
   - ¿Choca con T1B o con alguna guarda?
7. **Las reglas de proceso (§7).**
   - ¿Son correctas las lecturas de F4-E31, F4-E43, PD-F4-01 y PD-F4-04? Compruébalas en `f4-dispositions.md`.
   - ¿El patrón de I-39 que se cita existe tal como se describe?
   - ¿OB-T1B.2 respeta «Do not silently weaken the old guard»?
8. **Los hallazgos (§2) y los mapas.** Comprueba en el código una muestra amplia de anclas `ruta:línea` de los mapas y del paquete. ¿Son exactas? ¿Hay algún hecho falso que funde una decisión?
9. **Riesgos y cierre (§8 y §9).** ¿Falta algún riesgo material? ¿Los requisitos de cierre bastan?
10. **Vuelta al Owner.** ¿Hay alguna decisión **material de producto nueva** que la sesión esté tomando o que la revisión haga aparecer? Distínguela de las decisiones de procedimiento.

## Formato del informe

1. Objeto y método, y lo que no pudiste verificar.
2. Un apartado por cada punto, del 1 al 10, con su evidencia (`ruta:línea` o `P2:n`).
3. La tabla de hallazgos: Id, gravedad (BLOQUEANTE, IMPORTANTE o MENOR), contenido, evidencia y corrección propuesta.
4. El veredicto, con el recuento B/I/M y la lista de lo que debe volver al Owner:
   - **APTO**: se puede preparar el contrato de PB-T0;
   - **APTO TRAS CORRECCIONES**;
   - **NO APTO**.

**Tu respuesta final**, que vuelve a la sesión, es una sola línea: el veredicto, el recuento y la ruta del informe.
