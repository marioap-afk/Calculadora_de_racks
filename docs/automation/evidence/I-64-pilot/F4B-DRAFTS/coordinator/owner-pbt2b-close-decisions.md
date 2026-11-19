# I-64 F4B — Decisiones del Owner antes del cierre de PB-T2b (intento 1), dentro de F4B-E38

**Fecha:** 2026-11-19. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación, en una ronda de dos preguntas.

**Contexto:**
- sobre las entradas de la verificación `R20261119T182732Z-422f` (EXECUTION_VERIFIED / NONE, 14/14), nc2 `R20261119T183325Z-14dd` y nc3 `R20261119T183456Z-a3bc` dieron CONTROL PASS;
- nc1 `R20261119T183134Z-bfda` dio **CONTROL NOT PASSED**;
- la revisión independiente del código dio ACEPTABLE CON MENORES 0/0/6, con un hallazgo OWNER (OWNER-01).

Las preguntas y las respuestas se transcriben **literales**.

## Pregunta 1

> Control nc1 de PB-T2b. El Controller detectó el SHA falso y paró (STOP, S-04), que es el objetivo. Pero no es la firma exacta PD-F4-04: Handoff pasó, y fallaron Identity, Remote, Ci, Tests y Trailer. Tests y Trailer fallan además porque leyó mal dos campos de la entrega, que están bien: Worker.Trailer y RedRepairSha en Deviations. La verificación real los leyó bien y dio 14/14. Es la tercera variación en F4B (PB-TC, PB-T2a, PB-T2b); en las dos anteriores elegiste aceptarla solo esa vez. El resto de PB-T2b está en verde: CI 4/4, revisión 0/0/6, nc2 y nc3 PASS. ¿Qué hago?

**Opciones:**
- **«Aceptarlo solo esta vez (Recomendado)»:** Lo registro como decisión tuya solo para PB-T2b (nc1 = SATISFIED_BY_OWNER_DISPOSITION), sin cambiar las firmas, y cierro PB-T2b. La próxima variación volverá a ti.
- **«Repetir nc1 una vez»:** Conservo este resultado y lanzo otro nc1 (unos 5 minutos). Si da CONTROL PASS o la firma exacta, cierro. Si no, vuelvo a ti.
- **«Ampliar la firma para F4B»:** Para el resto de F4B, acepto nc1 cuando detecta el SHA falso y para con STOP/S-04, aunque fallen otras comprobaciones. Es una firma nueva: lleva su sustituto con autoprueba y revisión.

**Respuesta del Owner:** «Aceptarlo solo esta vez (Recomendado)».

**Registro:** `nc1 = SATISFIED_BY_OWNER_DISPOSITION`, solo para F4B-PBT2B-WORKSPACE-MODE-WINDOW, intento 1. Las firmas no cambian. La salida ordinaria de nc1 se conserva tal cual.

## Pregunta 2

> Hallazgo de la revisión de PB-T2b, en la ventana de Push Back del borrador. Si al pulsar 'Cerrar' hay a la vez un cambio de módulo sin confirmar y otros cambios sin aplicar al borrador (por ejemplo, la tolerancia), solo pregunta por el módulo ('Hay cambios de módulo sin confirmar…'). Si contestas 'Sí', también se pierde lo no aplicado, sin que la pregunta lo diga. El código sigue al pie de la letra el paquete. Ese modo aún no se usa en AutoCAD: lo conecta PB-T4. ¿Qué hago?

**Opciones:**
- **«Corregir en PB-T4 (Recomendado)»:** Cierro PB-T2b así y lo anoto como arrastre para PB-T4, que conecta el modo: allí 'Cerrar' hará una sola pregunta que nombre las dos cosas, con su prueba.
- **«Corregir antes de seguir»:** Abro una corrección de PB-T2b (intento 2 de 3), con su análisis, RED y GREEN, antes de cerrarla. Retrasa PB-T2c y PB-T3.
- **«Dejarlo como está»:** Se considera aceptable: la pregunta del módulo basta. Solo queda documentado.

**Respuesta del Owner:** «Corregir en PB-T4 (Recomendado)».

**Registro:** el arrastre **A-T2b-OWNER-01** va a PB-T4. Allí, «Cerrar» en WORKSPACE, con un módulo escenificado y cambios sin aplicar al borrador a la vez, hará una sola pregunta que nombre las dos cosas, con su prueba. PB-T2b se cierra con el código del paquete (D-PBT2-07).
