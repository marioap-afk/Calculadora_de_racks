# I-64 F4B PB-T1b — Decisiones del Owner tras la revisión independiente (REWORK 0/2/15)

**Fecha:** 2026-11-18. **Canal:** chat de la sesión principal, preguntas de opciones; respuestas literales. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-rework.md`.

**Los hechos:** el Worker `R20261118T204715Z-a4e2` (contrato v2.7, intento 1) entregó RED `2d9febc6` y GREEN `a0a5f699`, con la CI 4/4 exacta. La revisión independiente dio **REWORK 0/2/15**:
- el producto está bien;
- **I01:** la guarda G-L1 (CT-14) no aplica la regla 1b a los alias;
- **I02:** un caso de CT-10 no puede fallar;
- **dos menores contra la letra del contrato:**
  - M10: los ayudantes privados fuera de la lista cerrada de INV-PBT1B-03;
  - M14: ExpectedEvidence[2] pide `build-plugin` en success en la CI del RED, cuando depende de `tests`, que falla a propósito.

## Pregunta 1

**Texto, literal:** «PB-T1b: la revisión independiente da REWORK. El producto está bien (API exacta, D2 respetada), pero dos pruebas heredadas del borrador anterior son débiles: la guarda G-L1 no aplica una regla a los alias (una mutación futura «alias + molde» pasaría sin verse) y un caso de CT-10 no puede fallar. ¿Qué camino sigo?»

**Respuesta del Owner:** «Corregir en el intento 2 (Recomendado)».

**Lo que implica:** una delegación de corrección en el intento 2 de 3.
- Arregla I01, I02 y los menores de pruebas M01, M02, M04, M06, M07, M08 y M13.
- El RED vuelve a poner el esqueleto (§6.0) y el GREEN restaura la misma implementación.
- Los menores de producto (M09, M11, M12 y M15) se declaran o pasan como arrastres a PB-T2 y PB-T4.
- El intento 3, si llegara, vuelve al Owner.

## Pregunta 2

**Texto, literal:** «PB-T1b: dos frases del contrato no se pueden cumplir al pie de la letra. (1) Cierra la lista de ayudantes privados, y el comparador correcto necesita más (sin miembros públicos ni campos nuevos). (2) Pide que la compilación del plugin dé éxito en la CI del RED, pero esa compilación depende de Core, que en este RED falla a propósito, así que queda omitida. ¿Las ratificas?»

**Respuesta del Owner:** «Ratificar, solo PB-T1b (Recomendado)».

**Lo que implica:**
- **M10:** se ratifican los ayudantes privados del comparador y del mapa que no figuran en la lista de INV-PBT1B-03. No añaden miembros públicos ni campos a los tipos del censo.
- **M14:** se ratifica que en la CI del RED `build-plugin` salga omitido.
- **Alcance:** las dos ratificaciones valen solo para PB-T1b. Se pasan como disposición a su verificación, y el paquete de PB-T1c corrige el texto.
