# ADR-0017: Validación estructural de cargas diferida a RAM Elements

- **Estado:** propuesto
- **Fecha:** 2026-07-22 (fecha de documentación retroactiva; no es la fecha de la decisión original)
- **Decisores:** el dueño del repositorio acepta o rechaza este registro; solo él puede hacerlo. Redacción retroactiva bajo la iniciativa I-07. La decisión de diferimiento consta como decisión explícita del dueño (ver Referencias); la evidencia conservada no fija su fecha original ni un análisis comparativo detallado.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

RackCad diseña y dibuja racks industriales con su BOM, pero no verifica que una configuración soporte las
cargas a las que se someterá. La validación estructural de cargas y capacidad es un dominio de ingeniería
propio, con herramientas especializadas. La documentación histórica registra que el dueño decidió, de
forma explícita, no implementar esa validación dentro de RackCad y apoyarse en RAM Elements para ese fin,
con la instrucción expresa de no volver a proponerla.

Esta decisión es un diferimiento de alcance que cierra un debate recurrente; el registro de ADR existe
precisamente para que no se re-litigue sin saber por qué se difirió.

Este ADR documenta retroactivamente esa decisión ya vigente. El estado `propuesto` se refiere a este
registro, que aún no ha sido revisado formalmente por el dueño; **no reabre ni pone en duda el
diferimiento**, que sigue vigente por decisión explícita del dueño. La evidencia conservada establece que
el diferimiento es una decisión del dueño y su motivo (usar RAM Elements), pero no permite reconstruir de
forma fiable su fecha original ni un estudio comparativo detallado; este ADR no inventa ninguno de los dos.

## Decisión

No implementar validación estructural de cargas ni de capacidad dentro de RackCad en el alcance vigente.
Esa capacidad se difiere deliberadamente a RAM Elements. No re-proponer la validación de cargas como
trabajo de RackCad dentro del alcance actual; retomarla exigiría una decisión explícita del dueño que
revierta o acote este diferimiento.

## Alternativas consideradas

- **Implementar validación de cargas dentro de RackCad** — descartada por decisión explícita del dueño,
  que la difirió a RAM Elements. La evidencia conservada no documenta un análisis comparativo formal más
  allá de esa decisión, y este ADR no lo reconstruye.

## Consecuencias

- Positivas: el alcance de RackCad permanece acotado a diseño, dibujo y BOM; se evita re-litigar una
  decisión ya tomada; la responsabilidad estructural queda en una herramienta especializada.
- Negativas / costos aceptados: RackCad no advierte por sí mismo sobre configuraciones estructuralmente
  inviables; la verificación de cargas depende de un flujo externo (RAM Elements) fuera de este producto.

## Referencias

- [Auditoría de arquitectura 2026-07 — decisiones vigentes del dueño (validación de cargas va con RAM Elements)](../auditoria-arquitectura-2026-07.md)
- [Historial de HANDOFF preservado — validación de cargas diferida (decisión explícita, no re-proponer)](../archivo/transicion-2026-07/handoff-historial-2026-07.md)
- [HANDOFF §7 — decisiones vigentes pendientes de I-07](../HANDOFF.md)
- [ADR-0018: Optimizador de layout con IA diferido](0018-optimizador-layout-ia-diferido.md)
