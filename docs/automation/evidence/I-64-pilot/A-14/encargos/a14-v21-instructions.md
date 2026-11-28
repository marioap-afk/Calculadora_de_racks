# Instrucciones para la v2.1 de I-64 A-14

Siguen todas las reglas del encargo original. W es solo lectura; escribes solo en S. Aplica la fidelidad: lo que no tenga fuente queda
PENDING. El repo es público.

## Entrada nueva

`S/independent-review-a14-v2-check.md`. Es la comprobación acotada de la v2 por un revisor independiente nuevo, con este resultado:
- EMITIBLE TRAS CORRECCIONES;
- 28 hallazgos resueltos y 1 parcial (R2-02);
- hallazgos nuevos: 0 bloqueantes, 5 importantes (N-01..N-05) y 14 menores.

## Qué hacer

1. **Cada hallazgo** (R2-02 y todos los nuevos) se aplica, o se rechaza con un motivo verificable en las fuentes.
2. **N-01:** «Applies-to» como en las A-n de I-63. Declara que A-1..A-13 aplican también a las unidades de los tramos 2 y siguientes.
3. **N-02 (corte de tramos):** propón el tramo 1 = fases 0 a 4, que es la salida que ya trae la v2.
   - Deja la alternativa de las fases 0 a 3 (el ejemplo del Owner) como opción visible en la firma. El Owner confirma o corrige los
     hitos al firmar.
   - Lo que pase si los corrige se declara.
4. **N-03:** cada tramo lleva su Owner Validation con OV propios, que cubran lo que entra en ese tramo (borrador, Actualizar, foco,
   «Actualizar todo»…). Q-09 dice «tu validación» en cada hito.
5. **N-04:** sustituye SOLO las frases de D-PB-01 sobre la ventana modal. Conserva el modo explícito, no ejecutar por accidente la
   escritura clásica y no tener un segundo editor. Cita el texto exacto que se sustituye y el que se conserva.
6. **N-05:** cada punto abierto que se deja al Coordinator (Smoke-PERF por tramo, el momento de aceptar ADR-0047 y los demás) debe
   llevar el documento que lo cierra y cuándo: la orden de revisión del Architect, el paquete de la fase, etc. Así el texto congelado
   no queda con huecos sin dueño.
7. **Las dos deducciones de la sesión que el revisor señala** (extender Q-03 a los comandos de creación; tomar la aprobación del
   prototipo como la decisión de ADR-0029 D13): márcalas como deducciones de la sesión y llévalas a la firma para que el Owner las
   confirme. No las presentes como decisiones ya tomadas.
8. **La lectura de I-09** («posición» incluye acoplada o flotante y el lado): márcala como lectura de la sesión que el Owner confirma
   al firmar.
9. **La firma (§11):** que sea exacta y completa. Añade IC-03, el alcance real de lo que se sustituye de D-PB-01 y qué pasa si el
   Owner corrige los hitos. Debajo, añade una lista numerada en lenguaje llano para el Owner: qué aprueba, en frases cortas y sin
   identificadores sueltos.
10. **Las decisiones nuevas del redactor** que el revisor lista con su autoridad: recógelas en una tabla «Decisiones pendientes de
    autoridad», con quién decide cada una y cuándo.

## Salida

- `S/I-64-A-14-draft-v2.1.md`: la enmienda completa.
- `S/a14-v21-disposition.md`: la tabla de hallazgos.
- `S/a14-drafting-notes.md`: actualizado, con una §11 nueva.

Tu respuesta final, breve: las líneas, el recuento de aplicados y rechazados, lo que queda para cada autoridad y cualquier decisión
nueva que hayas tomado.
