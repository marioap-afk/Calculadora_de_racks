# I-62 — OD-1: paquete refrescado (2026-10-07; staging, **no es una solicitud**)

> **Preparación de staging.** Orden nocturna, decisiones §54 (fila «F7 y READY»: «OD-1 si es útil … sin aceptar OD-1 … sin tocar ADR-0048»). OD-1 no se
> pide todavía: bloquea READY-03 y la vigencia, no F6 (V14 §18; decisiones §45: «no pedir OD-1»). Refresca la nota de
> `owner-decision-packets.md` («Recordatorios», OD-1, preparación del 2026-10-06).

## 1. Hechos medidos

| Hecho | Valor | Cómo |
|---|---|---|
| ADR | `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md` | — |
| Estado | **«propuesto»** (cabecera: «Decisores: Owner del repositorio, único que lo acepta (OD-1)») | lectura del archivo en `e9473425` |
| Blob | **`e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e`**, igual en `MC_I62` (`6f0187cb`) y en la punta (`e9473425`) | `git rev-parse <rev>:<ruta>` |
| Cambios desde §47 | ninguno: decisiones §47, «ADR-0048 = NO CHANGE BEFORE F6 PILOTS» (editar `docs/adr/` invalidaría `MC_I62`, C-21) | historial del archivo |
| En el mapa de cláusulas | fila ADDED con `EffBlob` `e1bd8d91`; la guarda Core C-20a comprueba que `EffBlob` = blob del árbol | `I62-clause-map.json`; `I62F4CompatibilityGuardTests` |
| Índice ADR | sin fila (V14 §17 fila F1: «sin fila de índice: el índice se actualiza en el cierre documental») | `docs/adr/README.md` |
| Relación con A-1 | el ADR no cita A-1; A-1 §6 declara que no cambia ninguna decisión del Owner (OD-1..OD-7) ni la matriz OV; §47: «expresar A-1 no exige corregir ADR-0048»; la relación factual puede documentarse en el material de cierre o de historial | A-1 blob `c01899a7` §6; decisiones §47 |

## 2. Qué decide OD-1 (definición congelada, V14 §18)

«ADR sucesor (ampliación de §16, §16.13, fila de WORKFLOW §10, sección de WORKFLOW «Coexistencia de protocolos de ejecución delegada», punto de entrada de
compatibilidad, aplicabilidad DIRECT_ONLY / I62_DELEGATED con su marcador de G0 y la adopción posterior, rebase fuera de ventana con QU de reconciliación,
reconciliación en el cierre de una ventana con rebase y toma con rebase (REBASE_TAKEOVER), orquestación autónoma de roles (§20) …; con OD-6 alternativa 1,
LIFECYCLE)». **Bloquea:** READY-03 y vigencia. **Momento:** antes de READY-03.

## 3. Qué significa aceptarlo

- ADR-0048 pasa de «propuesto» a «aceptado»; como ADR, sucede **parcialmente** a ADR-0046, que sigue «aceptado» en lo que ADR-0048 conserva (#2-#9) y para
  las unidades I61 (ADR-0048, «Decisión»).
- **No activa nada por sí solo:** aceptado, rige solo desde `I62_EFFECTIVE_SHA` (ADR-0048, «Vigencia»; 16.14), es decir, desde el merge efectivo de la
  integración. Antes, todo texto materializado de I-62 sigue inactivo.
- No cambia el Freeze (V14 + A-1; A-2 es una candidata sin aplicar, decisiones §55), ni las OD-2..OD-7, ni la matriz OV; no convierte ningún UNKNOWN en
  MATCH (ADR-0048 punto 4). Si A-2 se acuerda antes de la solicitud, el paquete se rehace con el Freeze vigente.
- Satisface la parte OD-1 de READY-03 (LIFECYCLE §8: «sin … decision material/Owner pendiente»).

## 4. Deltas OWNER-RESERVED que la aceptación abarca (ADR-0048, «Amplía #1 (OWN-L)»)

| Delta | Dónde está materializado (inactivo) |
|---|---|
| obligaciones de la sesión principal fuera de una delegación | AUTOMATION_PLAN 16.15-16.16; routing §8 |
| fila de WORKFLOW §10 | WORKFLOW §10 (modificada por I-62) |
| punto de entrada de compatibilidad en una sección nueva de WORKFLOW, que rige la evaluación de los contratos de toda unidad desde `I62_EFFECTIVE_SHA` | WORKFLOW §12; AUTOMATION_PLAN 16.13 |
| predicado de independencia de LIFECYCLE (OD-6 = alternativa 1; no retroactivo) | LIFECYCLE §5 y §9 |
| orquestación autónoma de roles (§20): RELAY automático, ESCALATION solo ante fronteras reales, materialización autorizada de bindings del Architect, contratos de salida por rol, cierre efectivo de insumos con la exención acotada, fidelidad de los insumos, identidad observada por el invocador | AUTOMATION_PLAN 16.23, 16.24, 16.29; README §14-§18; esquemas B.9-B.10 |

La lista de OD-1 en V14 §18 nombra además la aplicabilidad DIRECT_ONLY / I62_DELEGATED, el rebase fuera de ventana con QU de reconciliación y
REBASE_TAKEOVER (16.25, 16.28; V14 §8.7-§8.9), cubiertos por el ADR en «Decisión» (opt-in) y por remisión al Freeze.

## 5. Interacción con el calendario (sin decidir; ver `closure-integration-checklist.md` §10)

- **Q2:** precedentes I-61 (`b69c3463`, ADR-0046 aceptado en READY-03) e I-63 (`13c6bee0`, ADR-0049 aceptado antes del Candidato) cambian el `Estado` antes
  del Candidato; `closure-plan.md` lo pone en el cierre documental. En I-62, cambiar el `Estado` cambia el blob listado en el mapa: hay que regenerar el
  mapa (C-20a) y aparece la pregunta Q1 (invalidación de `MC_I62`, resiembra y pilotos afectados).
- Si la edición se hace antes de la OV, la resiembra de D.5 copia el mapa regenerado (el mapa es `COPIED` en el manifiesto del fixture); si se hace en el
  cierre, la OV corre con el mapa de `MC_I62` y el cierre lo regenera.
- Tras READY-04, cualquier decisión nueva crea un SHA nuevo y reinicia READY-02 (LIFECYCLE §6).

## 6. Opciones (para cuando el Coordinator pida la decisión)

| Opción | Efecto |
|---|---|
| A — aceptar ADR-0048 tal cual (blob `e1bd8d91`) | READY-03 puede cerrar su parte OD-1; el `Estado` se edita en el momento que fije Q2 |
| Rechazar | READY-03 no puede satisfacerse; el Freeze no prevé otra vía: cualquier alternativa exigiría una decisión del Coordinator o del Owner y, si cambia lo congelado, una A-n (LIFECYCLE §6) |

**Sintaxis prevista (no enviar ahora):** `OD-1 = A (ADR-0048 aceptado, blob e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e)` · `OD-1 = RECHAZAR`.
Si antes de la solicitud el blob cambia (p. ej., por una edición autorizada), el paquete se rehace con el blob nuevo.
