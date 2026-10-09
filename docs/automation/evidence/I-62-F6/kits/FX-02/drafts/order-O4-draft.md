# BORRADOR — no publicado; no es una orden

> Borrador de la orden FX-U1-O4 preparado por la sesión de supervisión (plano a) a partir de `order-FX-U1-O4.template.md` del kit de FX-02 (blob
> `d92f7d3e…`, worktree en `f504f6a9`), según la clasificación de la solicitud de desbloqueo (decisiones §61.3; `FX-02-decision-classification.md`,
> SHA-256 `c7602667…`, §3 fila P4). **Nada de este archivo es una decisión ni una autorización.** Solo se rellenan los marcadores que son transcripción
> pura de decisiones ya registradas (bloque de transporte: decisiones §55 U-03, §58.2 y §59; topes de V14 D.3) y los rellenos G1 del Coordinator del
> fixture que la clasificación permite (PA-01, PA-02, PA-03 y U-13 a). Todo G1⚑, todo G3 (A-4), todo G5 y todo lo del Owner queda con un marcador
> abierto `<PENDIENTE: …>`. Publicar es S02 y espera a la disposición G1⚑, a A-4 AGREED (puntos 5-7) y, si se disponen así, a los commits de U-07 y
> U-08 (clasificación §3, W1).

## Notas de la supervisión (no se publican)

- **Secuencia con FX-04a (actualizada con decisiones §58 y §60).** CD-26 (OQ-33, U-66) queda **resuelta por hecho**: B3 corrió en el clon
  `D:\r62-fixture\B3` cuyo `origin` era la instantánea congelada del QH2 (decisiones §58.1) y FX-04a / C-25a quedó PASS sin B4 (decisiones §60.1).
  Ninguna B3 ni B4 futura leerá `fx/u1`, así que esa causa ya no impide publicar la orden. Falta solo que el Coordinator registre o confirme la
  resolución (clasificación #41, G2).
- **S11 y S12 sustituidos.** El bloque A2-P2 se ejecutó en `D:\r62-fixture\A` y `D:\r62-fixture\arch` (decisiones §58.2; evidencia §93) y OD-2e = A
  fijó la huella y el binario (decisiones §59). La medición que falta para el Controller es la sonda de A4-1 (F-A4-PROBE), solo con A-4 AGREED, su
  disposición de aplicación y `A4-SONDA-CONSUMO = A` (decisiones §61.4).
- **Punto 4 reescrito.** La causa de la parada de `codex-cli` ya no es P-01 (OD-2e lo levantó para la huella), sino F6-OBS-03 (decisiones §59, §60.2 y
  §61.4). El texto publicable la describe sin identificadores de I-62.
- **Sin esperados.** El bloque no contiene valores esperados, oráculos ni disposiciones esperadas de ningún control.
- **Sin identificadores reales (P-16; V14 D.7; repositorio público, dec. §48).** Antes de publicar, búsqueda literal en el bloque relleno de, como
  mínimo: «I-62», «I-61» y cualquier `I-<n>` real; «RackCad»; `OD-<n>`; «decisiones §», «dec. §», «ev. §», «evidencia §»; `D.<n>`, `B.<n>`, `F.<n>`,
  `G.<n>`, «Anexo», «V14», «Proposal»; `CD-<n>`, `OQ-<n>`, `U-<n>`, `A4-<n>`, `S<nn>`, `F-…`, «RAE»; rutas `D:\Documentos\…` y `D:\IDs`; ids de sesiones
  reales. **Todo marcador `<PENDIENTE: …>` contiene identificadores prohibidos a propósito:** ninguno puede quedar en el texto publicado. Los rellenos de
  este borrador se comprobaron con esa lista (ningún acierto fuera de los marcadores); la búsqueda se repite sobre el bloque final.
- **Fallo cerrado.** Un marcador sin decisión se **retira** con su paso y su efecto queda registrado (UNVERIFIED con causa); nunca se publica un
  marcador vacío ni un `<PENDIENTE: …>`.

| Marcador del template | Estado en este borrador | Clasificación (§61.3) | Depende de |
|---|---|---|---|
| `{EVIDENCE_DIR}` | **relleno**: `docs/automation/evidence/FX-U1-agent/takeover/` (propuesta del grupo a; el Coordinator del fixture lo adopta al publicar) | G1, #48 (PA-01) | — |
| (bloque aparte) «Autorización de transporte» | **relleno en parte**: huella, binario, versión de la app y `-C`; shell, celdas y registro de la medición, `<PENDIENTE>` | G2 (§55 U-03, §58.2, §59); A4-1 (G3); U-15 (G1⚑) | F-OBS03, F-A4, F-A4-PROBE, CD-09 |
| `{BINDING_ACCEPTANCE_RULE}` | `<PENDIENTE>` | U-09 a (G1⚑), U-10 b (G1⚑), A4-4 (G3); texto del marcador PA-04 tras U-09 a | CD-03, CD-04, CD-20, CD-03-marker |
| `{IN_WINDOW_BINDING_RULE}` | `<PENDIENTE>` | A4-3 = H-1 (G3), A4-4 = H-2 (G3), U-23 a (G1⚑) | CD-03, CD-04, CD-20, CD-22 |
| `{WORKER_RERUN_RULE}` | `<PENDIENTE>` (sin disposición se retira con el mismo efecto) | U-22 (G5⚑) | CD-21 |
| `{IN_WINDOW_ACCEPTANCE_RULE}` | `<PENDIENTE>` | U-09 e (G3⚑; Q-A4-11 o lectura del Coordinator) | CD-03 |
| `{EXEMPTIONS}` | **relleno** para el Controller; Architect `<PENDIENTE>` | U-13 a (G1, #16); U-13 a' (G5⚑, #17) | CD-07 |
| `{TRANSIENT_AND_CLEAN_TREE}` | `<PENDIENTE>` | U-08 (G1⚑) | CD-02 |
| `{TEST_EVIDENCE_SOURCE}` | `<PENDIENTE>` | U-07 (G1⚑) | CD-01 |
| `{CLOSURE_CUSTODY_RULE}` | `<PENDIENTE>` | U-17 (G1⚑) | CD-14 |
| `{SCOPE_READING}` | `<PENDIENTE>` | U-19 (G1⚑) | CD-17 |
| `{PUSH_RULE}` | **relleno** | G1, #50 (PA-03) | — |
| `{N_CONTROLS}` y colocación de N8-N10 | `<PENDIENTE>` | U-16 1, 2, 4 (G1⚑); U-16 3 (G2, solo confirmación) | CD-11 |
| `{CAPS_SCOPES}` y tabla de topes | **relleno** (cifras sin cambio); fila del Reviewer y contabilidad de la corrección, `<PENDIENTE>` | G1, #49 (PA-02); U-12 (G5⚑); A4-5 (G3) | CD-12 |
| `{REVIEW_PLACEMENT}` | `<PENDIENTE>` | U-11 a, b (G5⚑), U-11 c = A4-2 (G3), U-21 (G5⚑), U-68 (G3) | CD-05, CD-19, CD-27 |
| `{REVIEWER_AUTHORITY}` | `<PENDIENTE>` | U-12 (G5⚑) | CD-06, CD-18 |
| `{Q7_TASK_INTENT}` | `<PENDIENTE>` | U-18 b (G1⚑) | CD-15 |
| `{CORRECTION_RULE}` | `<PENDIENTE>` | U-25 a = A4-5 (G3, separable); U-25 b, c (G5⚑) | CD-24 |
| `{POST_Q7}` | `<PENDIENTE>` | U-18 a (G2), U-18 c (G5⚑) | CD-15 |
| `{AUTONOMY_GAP_RULE}` | `<PENDIENTE>` | U-06 a, b (G1⚑); U-06 c (G2) | CD-13 |
| `{IN_WINDOW_STOP_RULE}` | `<PENDIENTE>` | U-24 (G1⚑) | CD-23 |

- **Topes.** Las cifras de la tabla son las de V14 D.3 sin cambio (11 base + pool 4 = 15 de Codex; Worker 1 + 1 = 2; Reviewer 1 + 1 = 2; controles sin
  invocación 0). Los `scope` propuestos en `{CAPS_SCOPES}` son el relleno G1 de CD-12 (V14 B.8: `counters.invocations[]` «único por `scope` (gate del
  plan de la unidad)»). Con N6 = NOT_APPLICABLE (U-16 2, pendiente) se lanzaría un control menos, pero el tope no cambia. Con A4-2, los lanzamientos del
  Architect cuentan en la fila de Codex aunque los ejecute otro adapter (A4-2, regla 3).
- **Bloque de transporte.** `WorkingDirectory` transcribe decisiones §55 U-03; `ConfigSha256` y `BinarySha256`, la línea literal de OD-2e (decisiones
  §59); `BinaryPath`, la etiqueta del único `codex.exe` medido (evidencia §93). La línea `AppVersion` es un añadido de este borrador al template: es el
  tercer elemento del trío que nombra A4-1, regla 3, y se retira si la disposición de aplicación no la pide. Antes de publicar se comparan los
  invalidadores de U-02(b) con los de la medición de A4-1: si cambia alguno, la medición queda anulada (A4-1, regla 5) y el bloque no se publica.

<<<BEGIN FX-U1-O4>>>
## Orden FX-U1-O4 — titular nuevo (T16 → QR ORDINARY) y ventana 1 de la tarea T1

```text
FIXTURE-ORDER: FX-U1-O4
Unit: FX-U1
Plane: c (TEST-ACTIVATION)
From: QH cabed54738f56e846bd77be321cace10d053094d (record_version 5)
Contract: docs/automation/decisions/FX-U1-T1.gate-contract.json (rackcad-gate-contract/v2, blob 628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0)
```

Eres el titular nuevo de FX-U1. El titular liberado en el QH no vuelve a operar. Toda decisión que necesites está en este archivo; si te falta una,
detente y espera aquí (dentro de la ventana, según el punto 14). No uses la conversación como fuente de hechos ni de decisiones.

**Titular nuevo (T16; AUTOMATION_PLAN 16.26 y 16.28)**
1. **Observación.** Tu `rackcad-preflight/v1` para CUSTODY con la observación del runtime ligada a su instante (`get_session`). Con BELOW_REQUIRED o
   UNKNOWN: STOP P-09 y espera.
2. **Propuesta.** Con CUSTODY en MATCH o ABOVE_REQUIRED, tu `rackcad-binding/v1` (rol PRINCIPAL_COORDINATOR, `Scope` UNIT, aceptación PENDING). Publica
   la observación, la propuesta y sus validaciones en `docs/automation/evidence/FX-U1-agent/takeover/` de `fx/u1` (push a `origin` y a `github`),
   **sin** escribir el estado. Después espera aquí la designación de este Coordinator con `I62-PRINCIPAL-BINDING: <tu BindingId> ACCEPTED`.
3. **QR ORDINARY** tras la designación: `record_version` siguiente; titular HELD con tu binding aceptado (misma `BindingId`, aceptación decidida) y tu
   preflight custodiados en el mismo commit; la designación por `StateRef`; **todo** `StateRef` a `docs/automation/decisions/FX-U1.md` refrescado al blob
   del archivo en el árbol del QR; `g0_acceptance` sin cambio semántico; `task_intent`, contrato de T1 y contadores sin cambios. En este paso: ni Q0, ni
   planificación, ni invocaciones.

**Transporte `codex-cli`**
4. No invoques `codex-cli` hasta que este Coordinator publique aquí, en un commit posterior, el bloque «Autorización de transporte `codex-cli`». Hoy está
   detenido por una limitación medida del runtime: bajo `read-only`, la CLI no usa el `pwsh` que la receta pone primero en el `PATH` y rechaza la shell
   que ella misma selecciona. Con la autorización: antes de **cada** invocación, el SHA-256 de `~/.codex/config.toml` y el SHA-256 del binario iguales a
   los autorizados; si cambian, STOP P-01 sin aceptar nada nuevo y espera aquí (dentro de la ventana, según el punto 14). Solo la receta de 16.4 con
   `-s read-only`; nunca `workspace-write`. <PENDIENTE: A4-1 (G3) — la invocación del rol declara en su texto la shell que nombre el bloque de autorización (propuesta de A-4: cmd.exe); ningún otro elemento de la receta cambia>

**Bindings (16.18-16.21)**
5. Tras la autorización de transporte: preflight del Controller de planificación (rol EXECUTION_CONTROLLER, acción PLAN) para la celda autorizada y su
   `rackcad-binding/v1` (`Scope` TASK, `TaskId` T1, PENDING). Publícalos en `docs/automation/evidence/FX-U1-agent/takeover/` y espera aquí la aceptación.
   <PENDIENTE: U-09 (a) (G1⚑), U-10 (b) (G1⚑) / A4-4 (G3) — {BINDING_ACCEPTANCE_RULE}; texto literal del marcador: PA-04, después de U-09 (a)>
6. Worker: **subagente de tu sesión** (`claude-subagent`), uno por invocación, con una llamada cuya finalización se notifica y tope de 60 min;
   ningún subagente sobrevive a su llamada (16.4). Su preflight y su binding, y los del Controller de verificación (rol EXECUTION_CONTROLLER, acción
   VERIFY), no siguen el punto 5: <PENDIENTE: A4-3 (H-1), A4-4 (H-2) (G3), U-23 (a) (G1⚑) — {IN_WINDOW_BINDING_RULE}> <PENDIENTE: U-22 (G5⚑) — {WORKER_RERUN_RULE}; sin disposición se retira con su efecto registrado>

**Decisiones de este Coordinator para la ventana (todas antes del Q0; dentro de la ventana no se publica ninguna)**
7. <PENDIENTE: U-09 (e) (G3⚑; Q-A4-11 o lectura del Coordinator de I-62) — {IN_WINDOW_ACCEPTANCE_RULE}: quién evalúa A1'-A8', las aceptaciones de bindings y la repetición de Scope dentro de la ventana>
8. **Exención de acciones incompatibles (16.24).** `dotnet test` de «Comandos canonicos» del `AGENTS.md` queda EXEMPTED (ACTION_INCOMPATIBLE) para las
   invocaciones de solo lectura del Controller (planificación, verificación y controles negativos con invocación), con `ExemptionRef` a este punto. La
   exención no alcanza al Worker ni cambia los jobs de la CI. <PENDIENTE: U-13 (a') (G5⚑) / A4-2 (G3) — exención del Architect, aquí o en el bloque de su autorización de revisión, según su adapter y la colocación de U-11>
9. <PENDIENTE: U-08 (G1⚑) — {TRANSIENT_AND_CLEAN_TREE}>
10. <PENDIENTE: U-07 (G1⚑) — {TEST_EVIDENCE_SOURCE}>
11. <PENDIENTE: U-17 (G1⚑) — {CLOSURE_CUSTODY_RULE}>
12. <PENDIENTE: U-19 (G1⚑) — {SCOPE_READING}>

**Ventana 1 de T1 (16.4, 16.25)**
13. **Q0** con la ventana 1 y `task_intent` T1 con el binding aceptado del Controller de planificación en `planned_roles`; todo `StateRef` a
    `docs/automation/decisions/FX-U1.md` refrescado al blob de su árbol; el cuerpo del commit lleva el resumen de estado. Push a `origin` y a `github`. La
    ventana solo se abre con `HEAD` = `origin/fx/u1` = ese Q0 (`git ls-remote`).
14. **Dentro de la ventana**, en el orden de 16.4: planificación (Controller); control nc4 y aceptación A1'-A8' sin cortocircuito; Worker (commit RED y
    push, commit GREEN **con el resumen de estado en su cuerpo** y push; cada push a `origin` y a `github`, que hace el propio Worker dentro de su cesión
    sobre `refs/heads/fx/u1`, porque la CI corre en `github` y tú no escribes en Git dentro de la ventana); hechos remotos de la CI de los dos commits;
    verificación (Controller); controles negativos con invocación. **Ninguna escritura Git tuya** entre el Q0 y el Q7 (W-2): la Salida de 16.4 antes de
    la verificación y de cada control se cumple con el GREEN como último commit. Cada invocación con su `RunId`, sus comprobaciones de salida, cesión y
    entrada y su registro encadenado. Dentro de la ventana este Coordinator no publica nada: ante un STOP o una decisión que falte,
    <PENDIENTE: U-24 (G1⚑) — {IN_WINDOW_STOP_RULE}>
15. **Controles negativos** (no consumen `attempts`; ruta y registro propios; una sola vez). Los que llevan invocación, solo sobre las entradas de la
    verificación `EXECUTION_VERIFIED`; nc4, en la aceptación; N8, N9 y N10 <PENDIENTE: U-16 (3) (G2, solo confirmación) — colocación de N8, N9 y N10: dentro de la ventana, antes del Q7, sobre copias (texto congelado literal), o después del Q7 como proponía el staging>:
    - nc1, nc2, nc3 y nc4: los de `docs/automation/agent-execution/README.md` §10;
    - con una invocación del Controller cada uno: N4, N5, N6, N7a y N7b <PENDIENTE: U-16 (2) (G1⚑) — N6 NOT_APPLICABLE con causa, sin invocación>;
    - sin invocación: N8, N9 y N10.

    <PENDIENTE: U-16 (1) y (4) (G1⚑), con la mutación nueva o NOT_APPLICABLE de N8 — {N_CONTROLS}: solo la columna «Mutación exacta» de negatives.md, parte A, sin esperados; confinamiento uniforme sobre copias>
16. **Q7**: cierre de la ventana, cadenas, contadores desde el diario y manifiesto de custodia del diario en `docs/automation/evidence/FX-U1-agent/T1/`;
    todo `StateRef` refrescado al blob de su árbol; `task_intent`: <PENDIENTE: U-18 (b) (G1⚑) — {Q7_TASK_INTENT}> Push a `origin` y a `github`. Una
    ventana cuya verificación válida no es `EXECUTION_VERIFIED` también se cierra con su Q7 (16.25: «tras la verificación y los controles, o tras el
    cierre declarado de la ventana»). Corrección posterior a un cierre REWORK: <PENDIENTE: A4-5 (G3, separable) / U-25 (b), (c) (G5⚑) — {CORRECTION_RULE}; sin A4-5, ninguna corrección>

**Revisiones fuera de la ventana**
17. <PENDIENTE: U-11 (a), (b) (G5⚑), A4-2 (G3), U-21 (G5⚑), U-68 (G3) — {REVIEW_PLACEMENT}: colocación, autorización del bucle de revisión, adapter y directorio del Architect y referencia AUTHOR>
18. <PENDIENTE: U-12 (G5⚑) — {REVIEWER_AUTHORITY}: autoridad del Reviewer o su retirada de esta orden y de la tabla de topes>

**Después del Q7**
19. <PENDIENTE: U-18 (a) (G2), U-18 (c) (G5⚑) — {POST_Q7}>

**Topes del plan de gates de FX-U1 para T1 (16.8; P-07 antes de cada lanzamiento)**

| Ámbito | Lanzamientos base | Reejecuciones | Tope |
|---|---|---|---|
| `codex-cli`: planificación 1, verificación 1, Architect 1 y controles con invocación 8 <PENDIENTE: A4-2 (G3) — adapter del Architect; sus lanzamientos cuentan en esta fila con cualquier adapter> | 11 | pool de 4, como máximo 2 por fase | 15 |
| Worker (`claude-subagent`) | 1 | +1 solo con una corrección autorizada (cuenta en `attempts`) | 2 |
| <PENDIENTE: U-12 (G5⚑) — fila del Reviewer: «Reviewer (`claude-subagent` nuevo) \| 1 \| +1 \| 2», solo si se mantiene> | | | |
| Controles sin invocación (nc4, N8, N9, N10) | 0 | — | 0 |

`scope` de `counters.invocations[]`, uno por tope (`cap` = la cifra de esta tabla; `cap_source` = este archivo en el blob del árbol del punto durable que
lo registra; P-07 compara `launched` + `uncertain` + 1 con `cap` antes de cada lanzamiento):
- `T1/codex-cli`: tope 15 (los 11 lanzamientos base y las reejecuciones del pool);
- `T1/codex-cli/pool`: tope 4 (reejecuciones); el límite de 2 por fase es el de `counters.blocked_reruns` de cada `phase`;
- `T1/worker`: tope 2;
- <PENDIENTE: U-12 (G5⚑) — `T1/reviewer`, tope 2, solo si se mantiene el Reviewer>
- `T1/session-negatives`: tope 0 (nc4, N8, N9 y N10, sin invocación).

<PENDIENTE: A4-5 (G3, separable) — contabilidad de la planificación y la verificación de una corrección en el pool de su fase>

**Validación, mensajes y plano**
20. Antes de cada push de estado, valida el punto (esquema, `StateRef` en el árbol del propio commit, par con el punto anterior; README §17.1, paso 4).
    Una violación no se publica: STOP y espera aquí (dentro de la ventana, según el punto 14).
21. El Owner solo puede escribirte `continúa`. No le preguntes decisiones: las toma este Coordinator aquí, y dentro de la ventana no publica ninguna
    (punto 14). <PENDIENTE: U-06 (a), (b) (G1⚑); U-06 (c) (G2) — {AUTONOMY_GAP_RULE}>
22. **Plano (c).** No nombres, no configures y no uses ningún repositorio real (P-16).
<<<END FX-U1-O4>>>

## Bloque posterior — «Autorización de transporte `codex-cli`» (se publica en un commit aparte, en S13, tras S10)

Notas de la supervisión (no se publican): F-A2, F-OD-PROBE y F-OD-2D están cerradas (decisiones §57.1, §58.2, §59). Bloquean ahora F-OBS03 y, por la vía
V1, A-4 AGREED (A4-1) y la sonda medida de A4-1 con todas sus operaciones demostradas (F-A4-PROBE); el registro saneado de la medición depende de U-15
(G1⚑). El directorio del Architect de FX-02 no figura aquí (U-68, A4-2).

```text
FIXTURE-TRANSPORT-AUTHORIZATION: FX-U1-O4-T
Unit: FX-U1
Adapter: codex-cli
ConfigSha256: 6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32
BinaryPath: %LOCALAPPDATA%\OpenAI\Codex\bin\9691020b546a15b2\codex.exe
BinarySha256: 3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68
AppVersion: 26.1002.7124.0
Sandbox: read-only
WorkingDirectory: D:\r62-fixture\A2
Shell: <PENDIENTE: A4-1 (G3) — shell declarada que nombre la disposición de aplicación (propuesta de A-4: cmd.exe), con el texto exacto de la declaración usado en la sonda medida>
Cells: <PENDIENTE: A4-1 / F-A4-PROBE — CellId medido por la sonda de A4-1 con este binario (propuesta de A-4: codex-cli:gpt-6-luna, effort high), con su registro de medición en el fixture>
MeasurementRecord: <PENDIENTE: U-15 (G1⚑) — ruta en el fixture del registro saneado de la medición (propuesta: docs/automation/evidence/FX-U1-agent/measurement/)>
```

Texto que acompaña al bloque: «Desde este commit, `codex-cli` queda autorizado en FX-U1 solo con esta huella, este binario, `-s read-only` y las celdas
listadas <PENDIENTE: A4-1 (G3) — «, y con la shell declarada»>. Cualquier diferencia antes de una invocación es P-01: STOP sin aceptar nada nuevo.»
