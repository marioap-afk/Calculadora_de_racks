# I-64 A-14: decisiones del Owner sobre la replaneación (2026-11-23)

Fuente: chat de la sesión principal, 2026-11-23. Las preguntas se hicieron en el borrador «Replaneación de I-64» (§7) y en el
mensaje de la sesión que las resumía. Se transcriben literales.

## Preguntas (texto de la sesión)

1. «¿Apruebas el objetivo y los cambios de §3?» (§3 = tabla «Qué cambia del plan congelado (enmienda A-14)»).
2. «¿La ventana recuerda su tamaño y posición entre sesiones de AutoCAD?» (recomendación de la sesión: sí).
3. «¿RACKCAD abre la ventana?»
4. «La lista de comandos a retirar (esa puede esperar a la fase 9).»

## Respuesta del Owner (literal)

«Sí, apruebo los cambios al plan, la ventana recuerda, rackcad abre la ventana y la lista la vemos después»

## Lectura de la sesión

| # | Decisión | Efecto |
|---|---|---|
| 1 | APROBADOS los cambios de §3 | A-14 se redacta con esos cambios |
| 2 | La ventana recuerda tamaño y posición | se retira el no-objetivo de V5 «no persistir la posición de la paleta» |
| 3 | RACKCAD abre la ventana | el comando RACKCAD pasa a abrir la ventana de RackCad |
| 4 | Lista de comandos a retirar: después | se decide antes de la fase 9; A-14 no retira ningún comando |

**RACKPANEL (2026-11-23, después):** la sesión registró como supuesto que RACKPANEL sigue existiendo hasta la lista de la fase 9 y se
lo dijo al Owner. Respuesta del Owner (literal): «RACKPANEL puede quedarse por ahora». Queda DECIDIDO: RACKPANEL se conserva; su
futuro se ve con la lista de retiro antes de la fase 9.

## Segunda ronda (2026-11-23, AskUserQuestion; preguntas Q-03, Q-09, Q-07 y Q-08 del borrador v1)

Se transcriben la pregunta, la opción elegida y la descripción de esa opción, literales.

**Q-03 (RACKEDITAR).** Pregunta: «Cuando un sistema (por ejemplo Selectivo) tenga su diseño nuevo en la ventana RackCad, ¿qué pasa
con RACKEDITAR para ese sistema?». Respuesta: «Adopta el diseño nuevo (Recomendado)». Descripción: «RACKEDITAR abre la misma
pantalla rediseñada, pero en una ventana aparte. Hay un solo editor por sistema que mantener, como pide el plan congelado. El cambio
se declara cuando ocurra.» Opción no elegida: «Conserva la ventana de hoy».

**Q-09 (integración).** Pregunta: «El plan congelado dice que nada de I-64 llega a main hasta el final (sin integración parcial). Con
unas diez fases por delante, ¿cómo quieres integrar?». Respuesta: «Por tramos (Recomendado)». Descripción: «Integrar en main en hitos
que eliges tú (por ejemplo, tras la ventana con los racks y Push Back). Cada hito lleva su candidato completo y tu validación. Usas
antes lo nuevo y la rama no envejece.» Opción no elegida: «Todo al final».

**Q-07 (alias RK y menú de hoy).** Pregunta: «Hoy RACKCAD y su alias RK abren el menú principal en una ventana que bloquea AutoCAD.
Con RACKCAD abriendo la ventana nueva, ¿qué hacen RK y el menú de hoy?». Respuesta: «RK igual; menú viejo desde la ventana
(Recomendado)». Descripción: «RK abre la ventana nueva igual que RACKCAD. Hasta la fase 9, la ventana tiene una entrada para abrir el
menú de hoy como respaldo. No hace falta ningún comando nuevo.» Opciones no elegidas: «RK igual; el menú viejo desaparece» y «RK sigue
abriendo el menú viejo».

**Q-08 (abrir al iniciar AutoCAD).** Pregunta: «El prototipo muestra en Configuración la opción «Abrir RackCad al iniciar AutoCAD»,
pero nadie la decidió. ¿La incluimos?». Respuesta: «Ahora no; decidir después (Recomendado)». Descripción: «La A-14 no la incluye. Se
decide cuando se diseñe la sección de Configuración. Abrir la ventana al arrancar cambia cómo se restaura RackCad y conviene
probarlo aparte.» Opción no elegida: «Sí, incluirla en la A-14».

## Tercera ronda (2026-11-23, AskUserQuestion; Q-02 y Q-10, tras la revisión independiente del v1)

**Q-02 (variables y «Actualizar todo»).** Pregunta: «"Actualizar todo" escribe de una vez todos los racks con cambios. Las Variables
de RackCad entran en la ventana después, en la fase 5. ¿Cómo encaja eso?». Respuesta: «Primero racks, luego variables (Recomendado)».
Descripción: «La fase 4 entrega "Actualizar todo" solo para racks. Cuando la sección Variables llegue a la ventana, sus cambios serán
un borrador más que "Actualizar todo" también escribe. Tienes "Actualizar todo" antes.» Opción no elegida: «Esperar y salir juntos».

**Q-10 («Abrir en editor clásico»).** Pregunta: «La lista de racks del dibujo llega en la fase 2, pero los editores de Selectivo,
Dinámico, etc. no se migran a la ventana hasta las fases 6 y 7. ¿Adelantamos a la fase 2 un botón "Abrir en editor clásico" en cada
rack?». Respuesta: «Sí, en la fase 2 (Recomendado)». Descripción: «Desde la fase 2, cualquier rack de la lista se puede abrir en su
editor de hoy (como RACKEDITAR). La ventana sirve para los 70 racks desde el principio, aunque esos editores sigan bloqueando hasta que
se migren.» Opción no elegida: «No, se queda en la fase 6».

## Cuarta ronda: decisiones del Owner como Coordinator (2026-11-23, AskUserQuestion; Q-C1 y Q-C2 de la v2.3)

**Q-C1 (dónde se versiona).** Pregunta: «A-14 (la replaneación) ya tiene su texto final revisado. ¿Dónde la guardo en el
repositorio, junto con tus decisiones literales, la replaneación y las revisiones?». Respuesta: «Rama lateral, como A-12/A-13
(Recomendado)». Descripción: «Una rama aparte que parte de la de I-64, con la enmienda en docs/initiatives/I-64-A-14.md y las fuentes
en la evidencia de I-64. No toca la rama de F4B mientras el Smoke-PB sigue abierto; se une después del cierre de F4B.» Opción no
elegida: «En la rama de I-64 ya».

**Q-C2 (celda del Architect).** Pregunta: «La revisión formal del Architect usaba un modelo de Codex que ya no está acreditado tras la
actualización de la app. El único acreditado hoy es gpt-6-luna (alto, solo lectura), el que usan las planificaciones de F4B. ¿Con
cuál revisa la A-14?». Respuesta: «gpt-6-luna, el acreditado (Recomendado)». Descripción: «Revisión formal con la celda acreditada
hoy (binario 4F91AE86…, configuración Snapshot L). Se puede hacer ya, sin sondas nuevas.» Opción no elegida: «Sondear primero
gpt-6.1-sol».

## Decisiones previas del mismo día que A-14 recoge

- Visión: todo RackCad sin bloquear AutoCAD, en una ventana tipo navegador; navegación por la columna lateral (sin pestañas arriba),
  con buscador y filtro porque un dibujo puede tener hasta ~70 racks.
- «Actualizar» por rack y «Actualizar todo» fuera: todo o nada (respuesta «Todo o nada (Recomendado)»).
- «Nuevo» abre el sistema elegido; por ahora, la ventana clásica de ese sistema.
- Secciones: «Todo lo que vaya a tener que ver con RACKCAD, básicamente rackcad funcionaría desde ahí, se eliminaran muchos de los
  comandos pero obvio seguirán existiendo rackproyectar o así, Variables de RackCad, BOM y lista de racks, Biblioteca de diseños,
  Configuración».
- Editores: «Actual primero, simplificar después (Recomendado)».
- Rediseño UI/UX por sistema: «Diseñar antes de migrar (Recomendado)».
- Encaje: terminar el Smoke-PB de F4B cuando el Owner tenga AutoCAD libre.
