# I-64 F4B PB-T1b: vuelta al Owner por la parada PBT1B-C08 (segundo rechazo de la planificación, solo por la copia)

**Fecha:** 2026-11-18. La prepara la sesión principal en modo autónomo.

## Los hechos

**PB-T1b está emitida** (F4B-E17): X `1464193f` y BASE `7905b3a9`, con attempts 1 de 3. No ha corrido ningún Worker.

**Planificación** `R20261118T184831Z-aaac`: **RECHAZADA** por la comparación byte a byte, solo por la copia:
- ForbiddenWriteScope sin una entrada (la 72);
- en RequiredTests, el `Project` del índice 18 cambiado;
- en Authorities, el índice 31 con un identificador de CT cambiado.

A1-A8: solo A4 y A5 en fail, por esas mismas diferencias.

**Replanificación única** `R20261118T185409Z-c45c` (PBT1B-C08), con el prompt reforzado que nombra solo las cadenas correctas: **RECHAZADA otra vez**.
- Siete colecciones son idénticas byte a byte al contrato.
- ForbiddenWriteScope tiene **196 entradas en vez de 213**: faltan las 17 últimas, las de la raíz (de `docs/` a `.gitignore`).
- A1-A8: solo A4 en fail, por esas 17 entradas.

**La regla** (PBT1B-C08, [DP] pregunta 2): «un segundo rechazo de fidelidad … es STOP y vuelve al Owner».

**Lo que se ve:** el ForbiddenWriteScope de PB-T1b es el más largo hasta ahora (213 entradas, frente a 131 en PB-T1a). Las dos veces, el Controller se equivocó al copiar esa lista larga, y la segunda la cortó al final. El trabajo propio de la planificación sale bien las dos veces:
- los criterios de aceptación;
- la clase de la tarea;
- el enrutamiento.

## Opciones

- **(A) La sesión pone la lista del contrato, solo en esta delegación.**
  - Se acepta la replanificación `c45c` sustituyendo su ForbiddenWriteScope por la lista exacta del contrato. Es una colección opaca que el Controller solo copia.
  - Las otras siete colecciones ya son idénticas byte a byte.
  - Después se repiten la comparación byte a byte y A1-A8, que deben dar 8/8, y se hace nc4.
  - Queda declarado en el relevo y en `f4b-dispositions.md`. No gasta intento.
  - No cambia la regla para otras tareas.
- **(B) Una planificación más, como excepción a C08,** con la lista partida en bloques numerados en el prompt. Si vuelve a fallar, STOP. No gasta intento.
- **(C) Parar PB-T1b** y revisar antes el procedimiento de copia.
