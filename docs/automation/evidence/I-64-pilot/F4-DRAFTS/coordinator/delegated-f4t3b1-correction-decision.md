Decisión de la sesión, delegada por la orden del Owner de autonomía nocturna (`owner-overnight-autonomy-2026-11-13.md`), tomada el 2026-11-13 tras las revisiones del intento 1 de F4-T3B1-EDIT-SURFACE. No es una respuesta del Owner: el Owner no la vio.

**Hechos:**
- La revisión directa de la sesión y la revisión adversarial independiente coinciden en un BLOQUEANTE: la caja editable no admite texto escrito con el teclado.
- El `KeyDown` de burbuja marca `Handled` toda tecla salvo Tab, lo que suprime el `WM_CHAR` y el `TextInput`.
- La regla la exige el propio contrato (INV-F4T3B1-04, D-T3B1-04 v3), no es una desviación del Worker.
- Un experimento de WPF de la sesión lo confirma con mensajes reales (`keyboard-experiment/`).
- La revisión independiente da NO ACEPTABLE (1/0/7).

**Opciones consideradas:**
- **Corrección (intento 2 de 3):** un contrato enmendado del intento 1, con el bloqueante y los menores corregibles en el alcance de T3B1.
- **Cerrar y declararlo como riesgo de Smoke-2:** descartado. Un campo editable que no se puede teclear incumple la cobertura 1 del Owner («valid text -> internal draft update») y el objetivo de la tarea.

**Decisión:** corrección, intento 2 de 3 (precedentes F4-E27 y F4-E32).
- Entran el bloqueante (sin el `KeyDown` de burbuja; Enter y Esc siguen en el `PreviewKeyDown` de la caja editable) y los menores 2, 3, 4, 5, 7 y 8 de la revisión independiente, más la observación de la revisión directa (`IsProtected` conservador).
- El menor 6 (los motivos del host en inglés y el Error sin motivo, una limitación del puerto de T3A) pasa a F4-T3C, que toca los mensajes.
- La lista completa de teclas de D-01 en el host sigue siendo una verificación de Smoke-2: D-01 no congela la mecánica.
