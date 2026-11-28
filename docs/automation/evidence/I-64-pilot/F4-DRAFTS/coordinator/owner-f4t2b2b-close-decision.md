# I-64 F4-T2B2B-LIFECYCLE: decisión del Owner sobre el cierre del intento 1

- **Fecha:** 2026-11-12 (UTC), durante la verificación del Controller `R20261112T201718Z-060d`.
- **Canal:** pregunta de la sesión al Owner en el chat (AskUserQuestion). La pregunta y la respuesta se transcriben literalmente.

## Pregunta de la sesión

> F4-T2B2B (ciclo de vida de los borradores), intento 1: la revisión directa y la independiente dicen que el PRODUCTO es correcto y cumple tu tabla de veto. La CI del SHA exacto está en verde. La independiente da ACEPTABLE CON MENORES (0/0/5), y todos los menores están en pruebas, guardas o documentación. Pero su M-1 contradice la letra de INV-F4T2B2B-11, la regla I-1: una guarda nueva exige el texto exacto `_bridge.SetLifecycleAdvisor(_host);` en el controlador, y eso impide que F4-T3 envuelva el asesor para vaciar el texto vivo antes del cierre, algo que INV-11 y D-T2B2-10 permiten expresamente. Otros menores son de la misma clase: conteos exactos de archivo entero en el host (`_knownClean.Remove` = 9, `DiscardAndReload` = 1), la interfaz del asesor con exactamente 2 métodos y la cola de avisos fijada como internal. A eso se suman una prueba que fija como 'silent' la pérdida del residual declarado, documentación del enum compartido inexacta para la recarga y dos huecos de cobertura. Es el mismo modo de fallo que en T2B2A, ahora con severidad menor. ¿Qué hago?

### Opciones ofrecidas

1. **Cerrar y arrastrar a T3 (Recomendado).** Completo la verificación y nc1..nc3. Cierro T2B2B como PASS WITH LIMITATIONS y registro como desviación la contradicción de M-1 con INV-11. Los menores pasan a ser cambios declarados del contrato de F4-T3 (aflojar esas guardas y la prueba 'silent', documentar el enum, cubrir Detail/Witness y la no degradación del descarte). Mismo criterio que N-1 de T2B2A, sin gastar el intento 2.
2. **Corrección, intento 2 de 3.** Cadena completa de corrección acotada a pruebas, guardas y la documentación del enum, sin cambiar el comportamiento del producto: planificación, Worker, CI, revisiones, verificación y nc1..nc3. Deja T3 sin guardas que estorben, pero cuesta otra cadena (unas 2-3 horas) y el intento 2/3. No lanzaría nc1..nc3 sobre el intento 1.

## Respuesta del Owner

> Cerrar y arrastrar a T3 (Recomendado)

## Efecto

- **Cierre.** F4-T2B2B se cierra sobre `8f249e3a` como PASS WITH LIMITATIONS, si nc1..nc3 se resuelven, sin gastar el intento 2. La contradicción de M-1 con INV-F4T2B2B-11 queda registrada como desviación.
- **Arrastres a F4-T3** (cambios declarados de su contrato):
  - M-1;
  - M-2 (1 a 4);
  - D-1 de la revisión directa (conteo exacto de archivos de `Drafts/`);
  - M-3 (prueba «silent» del residual);
  - M-4 (documentación de `Waiting` y `NotAdmitted` para la recarga);
  - M-5 (Detail y Witness del NotAdmitted de la recarga; no degradación del descarte).
- **Controles.** Si nc2 o nc3 repiten un modo de fallo de las tareas anteriores, se devuelven al Owner antes de cualquier sustituto (F4-E29).
