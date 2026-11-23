# Instrucciones para la v2 de la enmienda I-64 A-14

Las reglas del encargo original (`a14-drafting-brief.md`) siguen todas: W solo lectura, escribir solo en S, fidelidad (PENDING en
vez de inventar), repo público (sin rutas locales, nombres ni datos de clientes), Python solo con Write + `python -I`.

## Entradas nuevas

1. `S/independent-review-a14-v1.md`: revisión independiente del v1, APTO TRAS CORRECCIONES (0 bloqueantes propios, 11 importantes,
   14 menores; más 2 bloqueantes y 2 menores derivados de la segunda ronda del Owner).
2. `S/owner-a14-decisions.md`: ahora con la segunda ronda (Q-03, Q-07, Q-08, Q-09) y la tercera (Q-02, Q-10), literales.

## Qué hacer

1. **Cada hallazgo de la revisión** se aplica o se rechaza con motivo verificable en las fuentes. Ninguno se ignora.
2. **Las decisiones del Owner** se incorporan al texto y salen de las preguntas pendientes:
   - **Q-03, RACKEDITAR adopta el diseño nuevo.** Hay que enmendar explícitamente lo que la revisión señala: D-16, el STOP de M-03, INV-19,
     OV-17 y lo que corresponda del brief. La caracterización vieja no se edita; se sustituye por un registro declarado, con el
     precedente de F4B (`superseding-characterization.md`). El cambio se declara en la fase de cada sistema.
   - **Q-09, integración por tramos.** Escribe cómo funciona un tramo: es una unidad de entrega con su propio READY, Candidato, Owner
     Validation y OV, y se integra según `docs/WORKFLOW.md`, limpieza incluida (§4.5.6 y §4.5.7).
     - **Hitos:** propón los hitos como PROPUESTA para que el Owner los confirme al firmar. El ejemplo del Owner es «tras la ventana con
       los racks y Push Back».
     - **Puntos abiertos:** ADR-0047, la clasificación de ID3, Smoke-PERF por tramo y la limpieza. Resuelve con las fuentes lo que se
       pueda; lo que no, déjalo para el Coordinator.
   - **Q-07:** RK abre la ventana igual que RACKCAD. Hasta la fase 9, la ventana tiene una entrada que abre el menú de hoy como
     respaldo. Ningún comando nuevo.
   - **Q-08:** «Abrir RackCad al iniciar AutoCAD» queda FUERA de A-14 y se decide con el diseño de Configuración.
   - **Q-02:** la fase 4 entrega «Actualizar todo» solo para racks. Cuando la sección Variables llegue a la ventana, sus cambios serán
     un borrador más que «Actualizar todo» también escribe.
   - **Q-10:** «Abrir en editor clásico» se adelanta a la fase 2, para cualquier rack de la lista. Ese editor sigue bloqueando hasta
     que su sistema se migre; declara esa limitación.
3. **Q-04, Q-05 y Q-06:** según la revisión, se resuelven con las fuentes o van en el texto de la firma. Hazlo así, y si alguna sigue
   necesitando una elección del Owner, dilo.
4. **«Actualizar todo»** (hallazgo de la costura de I-55): no inventes la solución técnica. Declara que es una composición nueva sobre
   la costura de un rack de I-55. Fija las condiciones que el paquete de la fase 4 tendrá que demostrar:
   - una transacción y un Deshacer;
   - el coste de N barridos;
   - el texto pendiente en borradores no abiertos;
   - la elegibilidad por estado (Conflicto, Desconocido, Error).

   Fija también la autoridad que debe aprobar la composición.
5. **La lista «Sin cambio»:** corrígela con las decisiones e invariantes que la revisión enumera. Para cada uno, di si cambia, cómo y
   con qué autoridad.

## Salida

- `S/I-64-A-14-draft-v2.md`: la enmienda completa, no un diff.
- `S/a14-v2-disposition.md`: una tabla con cada hallazgo de la revisión (id, severidad, aplicado/rechazado, dónde y el motivo).
- `S/a14-drafting-notes.md`: actualizado.

En tu respuesta final, breve: las líneas, el recuento de aplicados y rechazados, lo que queda para el Owner, el Coordinator y el
Architect, y cualquier decisión nueva que hayas tenido que tomar.
