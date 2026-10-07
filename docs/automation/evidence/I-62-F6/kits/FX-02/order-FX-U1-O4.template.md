# Plantilla — orden FX-U1-O4 del Coordinator del fixture (titular nuevo por T16 y ventana 1 de T1)

> **Preparación; no publicada.** Solo el bloque entre `<<<BEGIN FX-U1-O4>>>` y `<<<END FX-U1-O4>>>` se añade, ya relleno, al final de
> `docs/automation/decisions/FX-U1.md` de `fx/u1` (push a `origin` y a `github`), en un commit del Coordinator del fixture (como O2 `d30fb6a9` y O3
> `cc21e1d2`). Todo lo demás de este archivo es nota de la supervisión y no se publica.

## Notas de la supervisión (no se publican)

- **Secuencia con FX-04a.** La orden **no** se publica mientras FX-04a siga abierta (frontera F-UP-FX04A de `frontiers.json`; README §1): B2 lee este
  mismo origen y se compara exactamente con un oráculo fijado en el QH2.
- **Sin esperados.** El bloque no contiene valores esperados, oráculos ni disposiciones esperadas de ningún control: describe solo procedimiento,
  autoridad, topes y mutaciones. Los esperados están en `supervision-checks.md` y `negatives.md` (solo supervisión).
- **Sin identificadores reales (P-16; V14 D.7; repositorio público, dec. §48).** Antes de publicar, búsqueda literal en el bloque relleno (y en cada
  texto que vaya a `fx/u1`) de, como mínimo: «I-62», «I-61» y cualquier `I-6x` o `I-<n>` de una unidad real; «RackCad»; `OD-<n>`; «decisiones §»,
  «dec. §», «ev. §», «evidencia §»; referencias a anexos o secciones de la Proposal (`D.<n>`, `B.<n>`, `F.<n>`, `G.<n>`, «Anexo», «V14») y la palabra
  «Proposal»; identificadores propios de este staging (`CD-<n>`, `OQ-<n>`, `S<nn>`, `F-…` de fronteras, «RAE»); rutas `D:\Documentos\…` y `D:\IDs`; ids
  de sesiones reales (`local_…`) y sus etiquetas. Un acierto se reescribe o se retira; nunca se publica. La búsqueda se aplica a los textos nuevos, no a
  las autoridades copiadas byte a byte en el fixture (D.1). Los nombres N4-N10 son etiquetas locales de esta orden.
- **Marcadores de posición** `{…}` y la decisión de la que dependen (CD-nn de `frontiers.json`). Grupo (a): acto procedimental de este Coordinator del
  fixture. Grupo (b): disposición del Coordinator de I-62, registrada en `decisions/I-62.md`; aquí solo se **transcribe**, nunca se decide.

| Marcador | Contenido | Depende de | Grupo |
|---|---|---|---|
| `{EVIDENCE_DIR}` | directorio de evidencia del titular nuevo; propuesta: `docs/automation/evidence/FX-U1-agent/takeover/` | PF-EVIDENCE_DIR | a |
| (bloque aparte) | la autorización de `codex-cli` (huella, binario, celdas medidas, `-C`) **no** va en la orden: se publica después, en un commit propio, con la plantilla «Bloque posterior» del final de este archivo | F-OD-PROBE, F-OD-2D, CD-09, CD-10 | b (transcrito) |
| `{BINDING_ACCEPTANCE_RULE}` | cómo acepta este Coordinator los bindings del Controller (planificación y verificación) y del Worker, con el texto literal del marcador | CD-03, CD-04, CD-20 (lectura); CD-03-marker (texto) | b + a |
| `{IN_WINDOW_ACCEPTANCE_RULE}` | quién evalúa A1'-A8', las aceptaciones de bindings y la repetición de `Scope` dentro de la ventana, donde este Coordinator no puede publicar | CD-03 | b |
| `{EXEMPTIONS}` | exenciones explícitas y acotadas de acciones incompatibles (p. ej., `dotnet test`) para las invocaciones de solo lectura | CD-07 | a |
| `{TRANSIENT_AND_CLEAN_TREE}` | área transitoria y regla de árbol limpio en el fixture | CD-02 | b |
| `{TEST_EVIDENCE_SOURCE}` | fuente de los conteos de pruebas de la CI para `Tests` y `RedPart` | CD-01 | b |
| `{CLOSURE_CUSTODY_RULE}` | dónde y cuándo se custodian los cierres de insumos y los preflights de fidelidad de las invocaciones de la ventana; `Target` de IMPLEMENT | CD-14 | b |
| `{SCOPE_READING}` | lectura de las entradas con `**` del contrato frente a la sintaxis de alcance de 16.5, y regla de commits del Worker que solo tocan evidencia | CD-17 | b |
| `{PUSH_RULE}` | confirmación de que el Worker publica en `origin` y en `github` dentro de su cesión | CD-16 | a |
| `{WORKER_RERUN_RULE}` | cómo cuenta una reejecución BLOCKED del Worker frente a su tope | CD-21 | b |
| `{N_CONTROLS}` | mutaciones aprobadas de N4-N10: **solo** la columna «Mutación exacta» de `negatives.md`, parte A, sin esperados | CD-11 | b |
| `{CAPS_SCOPES}` | `scope` de `counters.invocations[]` para cada tope (transcripción sin cambio de las cifras) | CD-12 | a |
| `{REVIEW_PLACEMENT}` | revisión del Architect antes del Q0 (opción A) o después del Q7 (opción B), con su `ReviewLoopAuthorization` y la representación de la referencia AUTHOR | CD-05, CD-19 | b |
| `{REVIEWER_AUTHORITY}` | si se mantiene: «El Reviewer es un subagente nuevo de tu sesión (`claude-subagent`), distinto del Worker, con una llamada cuya finalización se notifica y tope de 60 min», su autoridad y la regla de su binding; si no, su retirada de esta orden (y de la fila del Reviewer en la tabla de topes) | CD-06, CD-18 | b |
| `{POST_Q7}` | acción tras el Q7 (QH y terminación, o espera con la custodia) | CD-15 | b |
| `{AUTONOMY_GAP_RULE}` | si los `continúa` del Owner tras una decisión se registran como AUTONOMY_GAP | CD-13 | b |

- Un marcador sin decisión se **retira** con su paso y su efecto queda registrado (UNVERIFIED con causa); nunca se publica un marcador vacío.
- La fila del Reviewer de la tabla de topes del bloque se publica solo si `{REVIEWER_AUTHORITY}` lo mantiene (el contrato T1 no tiene REVIEWER; OQ-06).
  Las cifras de la tabla son las de V14 D.3 sin cambio; cualquier otra lectura (p. ej., qué pasa con el lanzamiento del Architect si no se ejecuta) es
  del grupo (b).

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
detente y espera aquí. No uses la conversación como fuente de hechos ni de decisiones.

**Titular nuevo (T16; AUTOMATION_PLAN 16.26 y 16.28)**
1. **Observación.** Tu `rackcad-preflight/v1` para CUSTODY con la observación del runtime ligada a su instante (`get_session`). Con BELOW_REQUIRED o
   UNKNOWN: STOP P-09 y espera.
2. **Propuesta.** Con CUSTODY en MATCH o ABOVE_REQUIRED, tu `rackcad-binding/v1` (rol PRINCIPAL_COORDINATOR, `Scope` UNIT, aceptación PENDING). Publica
   la observación, la propuesta y sus validaciones en `{EVIDENCE_DIR}` de `fx/u1` (push a `origin` y a `github`), **sin** escribir el estado. Después
   espera aquí la designación de este Coordinator con `I62-PRINCIPAL-BINDING: <tu BindingId> ACCEPTED`.
3. **QR ORDINARY** tras la designación: `record_version` siguiente; titular HELD con tu binding aceptado (misma `BindingId`, aceptación decidida) y tu
   preflight custodiados en el mismo commit; la designación por `StateRef`; **todo** `StateRef` a `docs/automation/decisions/FX-U1.md` refrescado al blob
   del archivo en el árbol del QR; `g0_acceptance` sin cambio semántico; `task_intent`, contrato de T1 y contadores sin cambios. En este paso: ni Q0, ni
   planificación, ni invocaciones.

**Transporte `codex-cli`**
4. No invoques `codex-cli` hasta que este Coordinator publique aquí, en un commit posterior, el bloque «Autorización de transporte `codex-cli`». Hoy está
   detenido por un cambio de huella (P-01). Con la autorización: antes de **cada** invocación, el SHA-256 de `~/.codex/config.toml` y el SHA-256 del binario
   iguales a los autorizados; si cambian, STOP P-01 sin aceptar nada nuevo y espera aquí. Solo la receta de 16.4 con `-s read-only`; nunca
   `workspace-write`.

**Bindings (16.18-16.21)**
5. Tras la autorización de transporte: preflight del Controller de planificación (rol EXECUTION_CONTROLLER, acción PLAN) para la celda autorizada y su
   `rackcad-binding/v1` (`Scope` TASK, `TaskId` T1, PENDING). Publícalos en `{EVIDENCE_DIR}` y espera aquí la aceptación. {BINDING_ACCEPTANCE_RULE}
6. Worker: **subagente de tu sesión** (`claude-subagent`), uno por invocación, con una llamada cuya finalización se notifica y tope de 60 min;
   ningún subagente sobrevive a su llamada (16.4). Su binding sigue la regla del punto 5. {WORKER_RERUN_RULE}

**Decisiones de este Coordinator para la ventana (todas antes del Q0; dentro de la ventana no se publica ninguna)**
7. {IN_WINDOW_ACCEPTANCE_RULE}
8. {EXEMPTIONS}
9. {TRANSIENT_AND_CLEAN_TREE}
10. {TEST_EVIDENCE_SOURCE}
11. {CLOSURE_CUSTODY_RULE}
12. {SCOPE_READING}

**Ventana 1 de T1 (16.4, 16.25)**
13. **Q0** con la ventana 1 y `task_intent` T1 con el binding aceptado del Controller de planificación en `planned_roles`; todo `StateRef` a
    `docs/automation/decisions/FX-U1.md` refrescado al blob de su árbol; el cuerpo del commit lleva el resumen de estado. Push a `origin` y a `github`. La
    ventana solo se abre con `HEAD` = `origin/fx/u1` = ese Q0 (`git ls-remote`).
14. **Dentro de la ventana**, en el orden de 16.4: planificación (Controller); control nc4 y aceptación A1'-A8' sin cortocircuito; Worker (commit RED y
    push, commit GREEN y push; cada push a `origin` y a `github`, {PUSH_RULE}); hechos remotos de la CI de los dos commits; verificación (Controller); controles
    negativos. **Ninguna escritura Git tuya** entre el Q0 y el Q7 (W-2). Cada invocación con su `RunId`, sus comprobaciones de salida, cesión y entrada y
    su registro encadenado.
15. **Controles negativos** (no consumen `attempts`; ruta y registro propios; una sola vez). Los que llevan invocación, solo sobre las entradas de la
    verificación `EXECUTION_VERIFIED`; nc4 y N8, en la aceptación; N9 y N10, tras la verificación:
    - nc1, nc2, nc3 y nc4: los de `docs/automation/agent-execution/README.md` §10;
    - con una invocación del Controller cada uno: N4, N5, N6, N7a y N7b;
    - sin invocación: N8, N9 y N10.

    {N_CONTROLS}
16. **Q7**: cierre de la ventana, cadenas, contadores desde el diario y manifiesto de custodia del diario en `docs/automation/evidence/FX-U1-agent/T1/`;
    todo `StateRef` refrescado al blob de su árbol. Push a `origin` y a `github`.

**Revisiones fuera de la ventana**
17. {REVIEW_PLACEMENT}
18. {REVIEWER_AUTHORITY}

**Después del Q7**
19. {POST_Q7}

**Topes del plan de gates de FX-U1 para T1 (16.8; P-07 antes de cada lanzamiento)**

| Ámbito | Lanzamientos base | Reejecuciones | Tope |
|---|---|---|---|
| `codex-cli`: planificación 1, verificación 1, Architect 1 y controles con invocación 8 | 11 | pool de 4, como máximo 2 por fase | 15 |
| Worker (`claude-subagent`) | 1 | +1 solo con una corrección autorizada (cuenta en `attempts`) | 2 |
| Reviewer (`claude-subagent` nuevo) | 1 | +1 | 2 |
| Controles sin invocación (nc4, N8, N9, N10) | 0 | — | 0 |

{CAPS_SCOPES}


**Validación, mensajes y plano**
20. Antes de cada push de estado, valida el punto (esquema, `StateRef` en el árbol del propio commit, par con el punto anterior; README §17.1, paso 4).
    Una violación no se publica: STOP y espera aquí.
21. El Owner solo puede escribirte `continúa`. No le preguntes decisiones: las toma este Coordinator aquí. {AUTONOMY_GAP_RULE}
22. **Plano (c).** No nombres, no configures y no uses ningún repositorio real (P-16).
<<<END FX-U1-O4>>>

## Bloque posterior — «Autorización de transporte `codex-cli`» (plantilla; se publica en un commit aparte, tras OD-2d)

```text
FIXTURE-TRANSPORT-AUTHORIZATION: FX-U1-O4-T
Unit: FX-U1
Adapter: codex-cli
ConfigSha256: {SHA-256 exacto aceptado por el Owner}
BinaryPath: %LOCALAPPDATA%\OpenAI\Codex\bin\{etiqueta}\codex.exe
BinarySha256: {SHA-256 exacto aceptado por el Owner}
Sandbox: read-only
WorkingDirectory: {-C exacto, según CD-10}
Cells: {CellId medidos con este binario, p. ej. codex-cli:<modelo>:<EffortSemantic>, con su registro de medición en el fixture}
MeasurementRecord: {ruta en el fixture del registro saneado de la medición, según CD-09}
```

Texto que acompaña al bloque: «Desde este commit, `codex-cli` queda autorizado en FX-U1 solo con esta huella, este binario, `-s read-only` y las celdas
listadas. Cualquier diferencia antes de una invocación es P-01: STOP sin aceptar nada nuevo.»
