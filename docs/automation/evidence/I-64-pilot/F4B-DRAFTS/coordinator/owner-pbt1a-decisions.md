# I-64 F4B PB-T1a — Decisiones del Owner tras el Worker del intento 1

**Fecha:** 2026-11-18. **Canal:** chat de la sesión principal, con una pregunta de opciones (las respuestas, literales).

**La vuelta:** `F4B-DRAFTS/pbt1a/owner-return-pbt1a.md`, preparada con la revisión directa y con la independiente (REWORK 0/1/9).

## Pregunta 1 — CT-02 (I-01)

**Texto de la pregunta, literal:** «PB-T1a, prueba CT-02: el paquete suponía que elegir «lb» en la caja de unidad llega solo al diseño guardado, pero no llega. El contrato mandaba parar; el Worker, en cambio, añadió pasos a la prueba (cambiar el fondo del pallet para que la unidad se escriba). Lo hizo midiendo, antes de las pruebas definitivas, y lo declaró. La prueba sigue fijando lo que pedía su fila, más dos líneas medidas. ¿Ratificas la prueba tal como está?»

**Respuesta del Owner:** «Ratificar (Recomendado)».

**Lo que implica:**
- CT-02 queda tal como lo entregó el Worker, solo para este caso.
- I-01 se cierra sin tocar código, y la revisión independiente pasa a ACEPTABLE CON MENORES (0/0/9).
- Se sigue con la verificación en el intento 1.
- No se amplía ninguna regla: la regla SIN LATITUD y PBT1A-C04 siguen igual para las demás subtareas.

## Pregunta 2 — c-09 al reconstruir (OWNER-2)

**Texto de la pregunta, literal:** «c-09: un rack guardado con la unidad «ton» la conserva al cargar y al pulsar Actualizar. Pero si el usuario edita otra cosa que reconstruye la estructura (por ejemplo, el fondo del pallet), la unidad pasa a «kg» con el mismo número (1000 ton → 1000 kg), porque la caja solo ofrece kg y lb. ¿Qué hace PB-T1 con esto?»

**Respuesta del Owner:** «Como hoy, regla explícita (Recomendado)».

**Lo que implica:**
- Igual que c-02 (F4B-E09, pregunta 2), PB-T1b reproduce el comportamiento de hoy como una regla explícita del estado, con su prueba.
- RACKEDITAR no cambia.
- El paquete de PB-T1b añade esa regla y su CT antes de emitirse.

## Pregunta 3 — Los otros hallazgos (OWNER-3 y OWNER-4; INV-PBT1A-13, precondición 5)

**Texto de la pregunta, literal:** «Otros dos hallazgos de PB-T1a. (1) Elegir «lb» o editar la caja de fondos de pallet no llega al diseño guardado si nada reconstruye la estructura: la elección se pierde en silencio. (2) Aceptar Seguridad sin cambios en el rack de topes añade el portador de botas al diseño guardado, sin cambiar el BOM. ¿Qué hace PB-T1 con ellos?»

**Respuesta del Owner:** «Tal cual, anotar mejoras (Recomendado)».

**Lo que implica:**
- PB-T1 los reproduce sin cambiar RACKEDITAR.
- Se anotan en `docs/ideas-futuras.md` para arreglarlos después.
- Con las preguntas 2 y 3 respondidas, la precondición 5 de PB-T1b queda satisfecha para la lista de INV-PBT1A-13.
