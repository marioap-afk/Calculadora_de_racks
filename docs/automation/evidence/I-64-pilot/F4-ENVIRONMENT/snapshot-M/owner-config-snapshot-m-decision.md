# I-64 A-14 — Decisión del Owner sobre el config.toml reescrito tras el reinicio (Snapshot M)

**Fecha:** 2026-11-23. **Canal:** el chat de la sesión principal, con una pregunta de opciones, en la quinta ronda del registro de A-14
(`A-14/coordinator/owner-a14-decisions.md`, en la carpeta de trabajo de la sesión; el registro versionado en `ab0170b7` tiene las cuatro
rondas anteriores). La pregunta y la respuesta se transcriben literales desde ese registro.

**Los hechos (medidos al preparar la orden de la revisión, sin lanzar nada):**
- **Qué cambió:** la app de Codex reescribió `config.toml` al arrancar tras el reinicio, el 2026-11-23 a las 19:21:15Z. Su SHA-256 pasó
  de `B71650A6…` (Snapshot L) a `3E9BC35B0E72641A81241EC3E32BF45010B8CC3CC93FDD4E3C5E8188E6D19FFC`.
- **Qué no cambió:** los 105 nombres, en el mismo orden (`OrderedNamesSha256` `3ECF8E07…`); los 69 valores del inventario seguro; la
  versión de la app; y el binario acreditado de la CLI (`bin/5449c1f0f26f1f98`, SHA-256 `4F91AE86…`).

**Texto de la pregunta, literal:** «Para lanzar la revisión del Architect de la A-14: el config.toml de Codex se reescribió hoy a las
19:21 UTC, al arrancar la app tras el reinicio. Su hash ya no es el de la Snapshot L. En todo lo comparable es idéntico a L: mismos 105
nombres, mismos 69 valores normalizados, misma versión de la app y mismo binario 4F91AE86. Lo que cambia es un valor que el registro
guarda tapado, probablemente el identificador opaco de canalización. ¿Cómo seguimos?»

**Respuesta del Owner:** «Sonda neutra, luego M (Recomendado)». Descripción de esa opción, literal: «Como con la L: una sonda neutra con
gpt-6-luna en solo lectura. Si da PASS, esta configuración queda como Snapshot M y se lanza la revisión del Architect.» Opción no elegida:
«Esperar».

**Lo que implica:**
- **La sonda:** la sonda neutra con el binario acreditado, con el procedimiento de la L (F4B-E57). Corre desde el worktree lateral de
  A-14, el directorio de la revisión, que no figura en `[projects]` de `config.toml`. La referencia del contexto efectivo es la sonda
  `R20261122T172638Z-2c14` de la L.
- **Si da PASS:** el `config.toml` actual queda como **Snapshot M** y se lanza la revisión formal del Architect de A-14.
- **Si no da PASS:** STOP, sin lanzar la revisión.
