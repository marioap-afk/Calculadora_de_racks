# I-64 F4B PB-T1b: decisión del Owner sobre la planificación (PBT1B-C08)

**Fecha:** 2026-11-18. **Canal:** chat de la sesión principal, con una pregunta de opciones; la respuesta es literal. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-planning.md`.

**Los hechos:**
- **La planificación** `R20261118T184831Z-aaac` se rechazó solo por la copia, en tres colecciones.
- **La replanificación única** `R20261118T185409Z-c45c` se rechazó otra vez solo por la copia. Las otras siete colecciones salieron idénticas; en ForbiddenWriteScope faltan las 17 últimas entradas (213 → 196). En A1-A8, solo A4 dio fail.

**Texto de la pregunta, literal:** «PB-T1b (ya emitida, sin Worker todavía): las dos planificaciones del Controller se rechazaron solo por errores al copiar el contrato. En la segunda, todo es idéntico salvo que la lista de 213 archivos prohibidos salió cortada (faltan las 17 últimas). Es la lista más larga hasta ahora. La regla (C08) dice que un segundo rechazo vuelve a ti. ¿Qué hago?»

**Respuesta del Owner:** «La sesión pone la lista (Recomendado)».

**Lo que implica:**
- **Solo en esta delegación,** la sesión acepta la replanificación `c45c` sustituyendo su ForbiddenWriteScope por la lista exacta del contrato. Es una colección opaca que el Controller solo copia.
- **Lo que no se toca:** la salida original del Controller (`output.json`) y su comparación rechazada se conservan sin cambios. La delegación reparada es `delegation.json`.
- **Las comprobaciones se repiten** sobre la delegación reparada: la comparación byte a byte (las ocho colecciones), A1-A8 (deben dar 8/8) y nc4.
- **Se declara** en el relevo de la planificación y en `f4b-dispositions.md`.
- **No gasta intento** y no cambia PBT1B-C08 para otras tareas.
