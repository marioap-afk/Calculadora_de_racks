# I-62 F6 — Tarjeta del Owner (fronteras humanas; nada de esto lo hace la sesión)

## Tarjeta de la mañana del 2026-10-07 (modo nocturno, decisiones §54) — única orden de acción vigente

Cinco acciones del Owner, en este orden. Entre una y otra, la supervisión trabaja sola. Ninguna sesión del fixture recibe valores esperados, oráculos,
resultados de otras sesiones ni texto de RackCad o de I-62. El modo de permisos de cada sesión lo elige el Owner y la supervisión lo registra. Las
secciones anteriores están en [HISTORY-owner-card-2026-10-06.md](HISTORY-owner-card-2026-10-06.md) y **no se ejecutan**.

**Estado tras decisiones §55 (disposición del Coordinator de I-62):** acción 1 **hecha** (B2 acreditada INVALID_LAUNCH); acción 2 **parcialmente
hecha** (U-01..U-05 decididas; siguen U-06..U-65 y las nuevas U-66..U-71); acción 3 **bloqueada** por A-2 y por el bloque de medición nuevo (no hay
OD-2d-PROBE bajo el tope congelado), con los directorios de sus sondas pendientes (U-71); acciones 4 y 5 **siguen con sus compuertas**. La revisión del Architect independiente de
A-2 (candidata material, sin aplicar) tiene su tarjeta justo debajo: **acción A2-R, la única que el Owner puede hacer ahora**.

### Acción A2-R — Abrir la revisión del Architect de A-2 (decisiones §55, punto 8; HUMAN_LAUNCH_REQUIRED)

| Campo | Valor |
|---|---|
| Escenario | revisión formal del Architect independiente de la enmienda candidata A-2 (`docs/initiatives/I-62-A-2.md`, blob `d47f71b6…`); no es una sesión del fixture |
| Carpeta exacta | `D:\r62-arch-a2` (clon limpio en `4a059afa`, sin remoto); la app crea el worktree de la tarea |
| Runtime / app | app de escritorio de Claude: **un clic** en la tarjeta de tarea «Revisión del Architect de I-62 A-2» que la sesión de I-62 deja en la conversación (se crea una sola vez); no hay que pegar nada |
| Modelo / effort | `claude-opus-5-5` / `xhigh` (si la app los muestra, no los cambies); la supervisión los verifica con `get_session` |
| Texto inicial exacto | lo pone la tarjeta: un mensaje fijo que remite a `D:\r62-arch-a2-run\prompt.md` (23 737 bytes, SHA-256 `22d93ae5464ffbea29d52c1eae7f10e0e82f68eb180a272ba25cd8e3f2d0cfd5`); su texto exacto y su SHA-256 (`4daa606c…`) están en el [README del kit](../../I-62-architect-A-2/R20261008T014941Z-68fe/README.md). La supervisión comprueba que el primer mensaje lo contiene y que la sesión verifica el SHA de `prompt.md` en su Paso 0 |
| Después de abrir | nada. Si la sesión pregunta algo, no contestes: avisa en la sesión de I-62. Los permisos que pida la app los decides tú; la supervisión registra cada respuesta |
| Qué NO pegar | nada: ni A-2, ni el paquete, ni textos o resúmenes de I-62, ni opiniones |
| Evento de compleción | la sesión entrega su veredicto JSON y se detiene; escribe «Architect A-2 terminó» en la sesión de I-62 |
| Lo que sigue sin el Owner | la supervisión acredita la terminación, custodia el resultado y la transcripción por SHA-256, ejecuta el auditor v4-a2.1 custodiado y prepara el informe para el veredicto del Coordinator sobre A-2 |
| Estado (2026-10-08) | **hecha**: CHANGES REQUIRED (A62-A2-01 REQUIRED; O1..O3 OPTIONAL); auditor NOT_ACCREDITED (3 motivos); custodia y registro en evidencia §82. Siguiente: disposición del Coordinator |


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
| Estado (2026-10-07, 21:00Z) | **hecha** (acreditada INVALID_LAUNCH: decisiones §55 U-01a): B2 terminó sin el esquema (KICKOFF_SCHEMA_NOT_DELIVERED, error de la tarjeta del chat); respuesta `74e69f35…` durable en `4ea66233`; comparación v2 FAIL bruto 2/23 (evidencia §78-§79) |
| Lo que sigue sin el Owner | la supervisión acredita la terminación, guarda la respuesta fuera del clon, publica su SHA-256 de forma durable y solo entonces carga el oráculo v2 y compara con el contrato v2; ninguna escritura en el origen del fixture hasta que el Coordinator clasifique FX-04a |
| Disposición (decisiones §55) | **U-01a:** FAIL bruto 2/23 conservado; B2 = INVALID_LAUNCH (KICKOFF_SCHEMA_NOT_DELIVERED probado); FX-04a y C-25a UNVERIFIED; sin PASS ni FAIL de protocolo desde B2. **U-01b:** B3 no autorizada bajo el Freeze actual; solo con A-2 AGREED, y exactamente una. **U-01c:** origen del fixture liberado para trabajo independiente (preparación de FX-02, FX-06 y F7); FX-04a sigue OPEN / UNVERIFIED, sin limitación aceptada; F6 GATE PASS imposible mientras C-25a no se satisfaga. La supervisión conserva una instantánea del origen en QH2 (`D:\r62-fixture\fx04a-qh2-origin.git`); si B3 usa un clon con ese origen o si no se escribe en `fx/u1` antes de B3 lo decide el Coordinator (U-66) |

### Acción 2 — Una sola ronda de disposiciones del Coordinator (incluye la autorización de OD-2d-PROBE)

| Campo | Valor |
|---|---|
| Escenario | clasificación de FX-04a; FX-02 grupo (b); preguntas de FX-06 previas a la ventana; F7; alcance de OD-2d-PROBE |
| Carpeta / runtime / modelo / effort | no aplica: no se abre ninguna sesión del fixture |
| Qué hacer | llevar al Coordinator de I-62 el informe de la supervisión tras B2 ([b2-report-for-coordinator.md](../FX-04a/R20261007T061300Z-fx04a-b2/b2-report-for-coordinator.md): FAIL bruto 2/23 por esquema no entregado; U-01a..U-01c) y la solicitud consolidada [coordinator-disposition-request.md](coordinator-disposition-request.md). Primero van los puntos de compuerta: clasificación de FX-04a; operaciones, directorios y tope de OD-2d-PROBE y la medición `codex sandbox` (P1b); regla de los «continúa». Después, pegar en la sesión de I-62 la disposición que devuelva, con la línea `OD-2d-PROBE = A (…)` si se concede |
| Qué NO pegar | nada en sesiones del fixture; ningún valor del oráculo |
| Evento de compleción | disposición pegada en la sesión de I-62. La supervisión la custodia en las decisiones de I-62 y, si está autorizada y FX-04a ya está clasificada, ejecuta OD-2d-PROBE |
| Estado | **parcialmente hecha**: la disposición devuelta es decisiones §55. DECIDIDAS: U-01 (a, b, c); U-02, con (b) decidida (las sondas de solo lectura de OD-2d del binario vigente son las invocaciones medidas de las celdas del Controller y del Architect; sin segunda medición si no cambia ningún invalidador); U-03 (`-C` del Controller de FX-02 = `D:\r62-fixture\A2`; `-C` del Architect de FX-06 = `D:\r62-fixture\arch`, variante A; nunca A6); U-04 (tope congelado de sondas agotado; nada hasta A-2); U-05 (P1b fuera de OD-2d-PROBE; paquete exacto del Owner tras A-2 si hace falta). **No hubo línea `OD-2d-PROBE`** y no se ejecutó ninguna sonda. Pendientes para la siguiente ronda: U-06..U-65 y las nuevas U-66 (B3 frente a escrituras en `fx/u1`; = FX-02 CD-26, FX-06 OQ-26), U-67 (presupuestos de recuperación de la OV), U-68 (`-C` del Architect de FX-02; FX-02 CD-27), U-69 y U-70 (consumo y orden de P1b, solo si sigue haciendo falta tras A-2; FX-06 OQ-27 y OQ-28) y U-71 (directorios de las sondas del bloque de medición nuevo; FX-02 CD-28) de [coordinator-disposition-request.md](coordinator-disposition-request.md) |

### Acción 3 — OD-2d sobre el paquete medido

| Campo | Valor |
|---|---|
| Escenario | OD-2d (línea base exacta de `codex-cli`); bloquea FX-02 y FX-06 |
| Carpeta / runtime / modelo / effort | no aplica |
| Qué hacer | tras OD-2d-PROBE, la supervisión publica el paquete exacto medido (`../../I-62-prep/owner-decision-packets.md`, §OD-2d vigente). El Owner responde una línea: `OD-2d = A (línea base <huella medida>; binario <SHA-256 medido>)` u `OD-2d = RECHAZAR`. Si la medición no cambia nada, los valores son `9EA26634078B314A72A815B37CDD17657D7D0406E4911FC4CC4FEC42845153C3` y `97c57e4eb64257bcd7a470757950886f2c59eec4aa8537908979c6475d41cc08` |
| Qué NO pegar | contenido de `config.toml` o de credenciales |
| Evento de compleción | línea pegada en la sesión de I-62 |
| Estado | **bloqueada** (decisiones §55 U-04): el tope congelado de sondas está agotado y no hay OD-2d-PROBE bajo él; las autorizaciones anteriores del Owner no lo aumentan. Antes de esta acción: A-2 acordada por el Coordinator y un Architect independiente (problema 2: bloque de medición nuevo de como máximo dos sondas de solo lectura para el BinaryHash/AppVersion exactos), después la petición de ese bloque y su medición; el Owner acepta entonces la huella resultante bajo OD-2 con la línea de esta acción. P1b no entra (U-05). Si la sonda de la celda del Controller del bloque va con `-C D:\r62-fixture\A2` (propuesta del kit de FX-02; U-03 fija solo los `-C` y los directorios de las sondas son la pregunta U-71), esa carpeta solo existe después de publicar la orden FX-U1-O4 en `fx/u1` (FX-02 S02) y de preparar el clon (FX-02 S03): O4 y el clon `A2` van entonces **antes** del bloque de medición del Controller, aunque figuren entre los requisitos de la acción 4, y la escritura de O4 depende de U-66 (la supervisión no decide ninguna de las dos). Hasta entonces, nada que hacer aquí |

### Acción 4 — Abrir A2 (FX-02 / C-24; HUMAN_LAUNCH_REQUIRED)

| Campo | Valor |
|---|---|
| Requisitos (los comprueba la supervisión antes de avisar) | acciones 1-3 hechas; orden FX-U1-O4 publicada en `fx/u1` con la CI registrada; clon preparado; parte 1 de [FX-02/launch-card-A2.md](FX-02/launch-card-A2.md) completa. Tras decisiones §55: la acción 3 sigue bloqueada (A-2); publicar O4 es una escritura en `fx/u1` y depende de U-66; el `-C` del Controller de FX-02 es esta misma carpeta (U-03); si la sonda de la celda del Controller va aquí (U-71), O4 y el clon ya están hechos antes del bloque de medición de la acción 3 (ninguno es una acción del Owner) |
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
| Requisitos (los comprueba la supervisión antes de avisar) | QH de FX-02 con la terminación de A2 acreditada; precondiciones de FX06-F04 comprobadas en el último punto de `fx/u1`; disposiciones de FX06-F05; OD-2d aceptada y celda del Architect re-medida; orden y RLA publicadas tras `prepublish_scan.py` (P5); clon de P6 preparado. Tras decisiones §55: las sondas de solo lectura de OD-2d del binario vigente son la medición de la celda del Architect, sin otra si no cambia ningún invalidador (U-02 b); de FX06-F05 ya están decididas U-03, U-04 y U-05, y entran U-66 y, si P1b sigue haciendo falta tras A-2, U-69 y U-70 |
| Escenario | FX-06: piloto de autonomía real D.8 |
| Carpeta exacta | `D:\r62-fixture\A6` (carpeta del Principal; el Architect trabaja con `-C D:\r62-fixture\arch`, variante A, nunca en A6: decisiones §55 U-03) |
| Runtime / app | app de escritorio de Claude, **sesión nueva** (`claude-desktop-session`) |
| Modelo / effort | `claude-opus-5-5` / `xhigh` |
| Texto inicial exacto | la plantilla neutra de [FX-06/staging/README.md](FX-06/staging/README.md) §5 con `<UNIDAD>` = `FX-U1` (opción A; `FX-U2` si el Coordinator elige la B) |
| Después de abrir | un solo `continúa` cuando la supervisión diga que la designación está publicada (último mensaje humano antes del paso 1). Después, ningún mensaje hasta que la supervisión diga que el paso 7 está custodiado. Hecho medible: cualquier mensaje o aprobación humana entre los pasos 1 y 7 es un candidato a OWNER_CLICK_IN_LOOP / AUTONOMY_GAP, y con él FX-06 no puede ser PASS |
| Qué NO pegar | el oráculo, el defecto sembrado, resultados esperados de los Architects, textos de RackCad o de I-62 |
| Evento de compleción | QU de la ingestión de C (paso 7) custodiado y terminación acreditada; la supervisión avisa |

---

El historial de esta tarjeta está en [HISTORY-owner-card-2026-10-06.md](HISTORY-owner-card-2026-10-06.md) (no ejecutar).
