# I-62 F6 — Tarjeta del Owner (fronteras humanas; nada de esto lo hace la sesión)

## Tarjeta de la mañana del 2026-10-07 (modo nocturno, decisiones §54) — única orden de acción vigente

Cinco acciones del Owner, en este orden. Entre una y otra, la supervisión trabaja sola. Ninguna sesión del fixture recibe valores esperados, oráculos,
resultados de otras sesiones ni texto de RackCad o de I-62. El modo de permisos de cada sesión lo elige el Owner y la supervisión lo registra. Las
secciones anteriores están en [HISTORY-owner-card-2026-10-06.md](HISTORY-owner-card-2026-10-06.md) y **no se ejecutan**.

### Acción 1 — Abrir B2 (FX-04a / C-25a; HUMAN_LAUNCH_REQUIRED)

| Campo | Valor |
|---|---|
| Escenario | FX-04a: segunda sesión limpia del Principal B (RESUME_DECISION), decisiones §53 |
| Carpeta exacta | `D:\r62-fixture\B2` (clon en `cabed547`, árbol limpio, sin sesión previa) |
| Runtime / app | app de escritorio de Claude, **sesión nueva** (`claude-desktop-session`) |
| Modelo / effort | `claude-opus-5-5` / `xhigh` |
| Texto inicial exacto | el contenido íntegro de `D:\r62-fixture\evidence-out\fx04a-b2\B2-kickoff.md` (8 065 bytes, SHA-256 `233b058261d2bdb1e7c2f6f16290a32ea6b100df0e430987f8ea7131c4e7d59e`), como primer y único mensaje |
| Después de abrir | nada. Si B2 pregunta algo, no contestes: avisa en la sesión de I-62 |
| Qué NO pegar | el oráculo o su hash, nada de B1 (respuesta, comparación, análisis), transcripciones, textos de RackCad o de I-62, explicaciones |
| Evento de compleción | B2 entrega su objeto JSON y se detiene; el Owner escribe «B2 terminó» en la sesión de I-62 |
| Estado (2026-10-07, 21:00Z) | **hecha**: B2 terminó sin el esquema (KICKOFF_SCHEMA_NOT_DELIVERED, error de la tarjeta del chat); respuesta `74e69f35…` durable en `4ea66233`; comparación v2 FAIL bruto 2/23 (evidencia §78-§79) |
| Lo que sigue sin el Owner | la supervisión acredita la terminación, guarda la respuesta fuera del clon, publica su SHA-256 de forma durable y solo entonces carga el oráculo v2 y compara con el contrato v2; ninguna escritura en el origen del fixture hasta que el Coordinator clasifique FX-04a |

### Acción 2 — Una sola ronda de disposiciones del Coordinator (incluye la autorización de OD-2d-PROBE)

| Campo | Valor |
|---|---|
| Escenario | clasificación de FX-04a; FX-02 grupo (b); preguntas de FX-06 previas a la ventana; F7; alcance de OD-2d-PROBE |
| Carpeta / runtime / modelo / effort | no aplica: no se abre ninguna sesión del fixture |
| Qué hacer | llevar al Coordinator de I-62 el informe de la supervisión tras B2 ([b2-report-for-coordinator.md](../FX-04a/R20261007T061300Z-fx04a-b2/b2-report-for-coordinator.md): FAIL bruto 2/23 por esquema no entregado; U-01a..U-01c) y la solicitud consolidada [coordinator-disposition-request.md](coordinator-disposition-request.md). Primero van los puntos de compuerta: clasificación de FX-04a; operaciones, directorios y tope de OD-2d-PROBE y la medición `codex sandbox` (P1b); regla de los «continúa». Después, pegar en la sesión de I-62 la disposición que devuelva, con la línea `OD-2d-PROBE = A (…)` si se concede |
| Qué NO pegar | nada en sesiones del fixture; ningún valor del oráculo |
| Evento de compleción | disposición pegada en la sesión de I-62. La supervisión la custodia en las decisiones de I-62 y, si está autorizada y FX-04a ya está clasificada, ejecuta OD-2d-PROBE |

### Acción 3 — OD-2d sobre el paquete medido

| Campo | Valor |
|---|---|
| Escenario | OD-2d (línea base exacta de `codex-cli`); bloquea FX-02 y FX-06 |
| Carpeta / runtime / modelo / effort | no aplica |
| Qué hacer | tras OD-2d-PROBE, la supervisión publica el paquete exacto medido (`../../I-62-prep/owner-decision-packets.md`, §OD-2d vigente). El Owner responde una línea: `OD-2d = A (línea base <huella medida>; binario <SHA-256 medido>)` u `OD-2d = RECHAZAR`. Si la medición no cambia nada, los valores son `9EA26634078B314A72A815B37CDD17657D7D0406E4911FC4CC4FEC42845153C3` y `97c57e4eb64257bcd7a470757950886f2c59eec4aa8537908979c6475d41cc08` |
| Qué NO pegar | contenido de `config.toml` o de credenciales |
| Evento de compleción | línea pegada en la sesión de I-62 |

### Acción 4 — Abrir A2 (FX-02 / C-24; HUMAN_LAUNCH_REQUIRED)

| Campo | Valor |
|---|---|
| Requisitos (los comprueba la supervisión antes de avisar) | acciones 1-3 hechas; orden FX-U1-O4 publicada en `fx/u1` con la CI registrada; clon preparado; parte 1 de [FX-02/launch-card-A2.md](FX-02/launch-card-A2.md) completa |
| Escenario | FX-02: Principal A2 de FX-U1, de QH2 a Q7 de T1 (topología A) |
| Carpeta exacta | `D:\r62-fixture\A2` |
| Runtime / app | app de escritorio de Claude, **sesión nueva** (`claude-desktop-session`) |
| Modelo / effort | `claude-opus-5-5` / `xhigh`; sin cambiar modelo, effort ni modo de permisos durante la sesión |
| Texto inicial exacto | `Eres el Principal de la unidad FX-U1 de este repositorio. Sigue AGENTS.md y la orden FX-U1-O4 de docs/automation/decisions/FX-U1.md.` |
| Después de abrir | solo `continúa`, y solo cuando la supervisión lo indique (cada uno queda registrado). Si A2 pregunta algo, no contestes: avisa en la sesión de I-62. No cierres la sesión hasta que la supervisión lo diga |
| Qué NO pegar | valores esperados, oráculos, resultados o transcripciones de otras sesiones, hechos reconstruidos, textos de RackCad o de I-62, salidas de Codex o de subagentes |
| Evento de compleción | A2 termina según la orden, con `isRunning` = false en dos observaciones (evento E6 de la tarjeta de A2); la supervisión avisa |

### Acción 5 — Abrir el Principal de FX-06 (C-39; HUMAN_LAUNCH_REQUIRED)

| Campo | Valor |
|---|---|
| Requisitos (los comprueba la supervisión antes de avisar) | QH de FX-02 con la terminación de A2 acreditada; precondiciones de FX06-F04 comprobadas en el último punto de `fx/u1`; disposiciones de FX06-F05; OD-2d aceptada y celda del Architect re-medida; orden y RLA publicadas tras `prepublish_scan.py` (P5); clon de P6 preparado |
| Escenario | FX-06: piloto de autonomía real D.8 |
| Carpeta exacta | `D:\r62-fixture\A6` |
| Runtime / app | app de escritorio de Claude, **sesión nueva** (`claude-desktop-session`) |
| Modelo / effort | `claude-opus-5-5` / `xhigh` |
| Texto inicial exacto | la plantilla neutra de [FX-06/staging/README.md](FX-06/staging/README.md) §5 con `<UNIDAD>` = `FX-U1` (opción A; `FX-U2` si el Coordinator elige la B) |
| Después de abrir | un solo `continúa` cuando la supervisión diga que la designación está publicada (último mensaje humano antes del paso 1). Después, ningún mensaje hasta que la supervisión diga que el paso 7 está custodiado. Hecho medible: cualquier mensaje o aprobación humana entre los pasos 1 y 7 es un candidato a OWNER_CLICK_IN_LOOP / AUTONOMY_GAP, y con él FX-06 no puede ser PASS |
| Qué NO pegar | el oráculo, el defecto sembrado, resultados esperados de los Architects, textos de RackCad o de I-62 |
| Evento de compleción | QU de la ingestión de C (paso 7) custodiado y terminación acreditada; la supervisión avisa |

---

El historial de esta tarjeta está en [HISTORY-owner-card-2026-10-06.md](HISTORY-owner-card-2026-10-06.md) (no ejecutar).
