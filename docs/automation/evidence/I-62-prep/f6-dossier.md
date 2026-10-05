# I-62 — Plan de F6 (fixture y pilotos; preparación, nada se crea ahora)

> **Preparación, no ejecución.** Orden del Coordinator de [decisiones](../../decisions/I-62.md) §33 (clase B). No se crea el fixture, no se abren sesiones
> ni se invoca ningún runtime. Fuente: Proposal V14 Anexo D (D.1-D.8), §12, §15 y §18 (blob `34ad80ea`).

## 1. Escenarios

| Escenario | Propósito | Prerrequisitos | Decisión del Owner | Runtime | Estado del fixture | PASS esperado | UNVERIFIED / UNSUPPORTED | Evidencia exacta |
|---|---|---|---|---|---|---|---|---|
| **FX-01** (C-23, OV-I62-01) | autoverificación del Principal en tres observaciones dentro de una sesión, con un cambio de effort por el Owner | fixture arrancado (D.1); FX-U1 en I62 tras G0 | OD-5 | `claude-desktop-session` | FX-U1 con el contrato de T1 | effort inferior → BELOW_REQUIRED → STOP P-09; effort correcto → MATCH; observación por `get_session` ligada al instante | sin OD-5 o sin la sesión A: UNVERIFIED | tres `preflight/v1`, validaciones, instantes de `get_session` |
| **FX-02** (C-24, OV-I62-03) | topología A completa: Controller `codex-cli`, Worker subagente, Reviewer subagente, Architect `codex-cli`, ocho negativos con invocación y cuatro de sesión | FX-01; binario medido (sondas con OD-2); CI del fixture | OD-5, **OD-7**, OD-2 | `claude-desktop-session`, `codex-cli`, `claude-subagent` | ventana k: Q0 → planificación → aceptación → Worker → verificación → Q7 | VERIFIED sobre G con `Ci` de los jobs del fixture; negativos con su clasificación de D.3; Q7 conforme a B.8.4 | sin OD-7: `Ci` = `not_run` → nunca VERIFIED → UNVERIFIED (F6 no cierra) | diario custodiado, Q0 y Q7, verificaciones, corridas de CI del fixture |
| **FX-03** (C-26, OV-I62-04) | topología B: Principal `codex-desktop-session`; Reviewer y Architect `claude-cli`; Worker `codex-cli` con escritura | OD-3 (CLI autenticada), OD-4 (escritura); sondas | OD-3, OD-4, OD-2, OD-5 | `codex-desktop-session`, `codex-cli`, `claude-cli` | igual que FX-02 en la topología B | VERIFIED + Q7 | sin OD-3 u OD-4: UNVERIFIED; escritura denegada por el sandbox: UNSUPPORTED (el escenario no se retira) | igual que FX-02 |
| **FX-04a** (C-25a, OV-I62-05 a) | portabilidad: B reconstruye desde QH sin contexto privado y produce la siguiente decisión | FX-02 hasta su Q7 (o QH tras BOOTSTRAP con T1 planificada, registrado así); terminación de A acreditada; oráculo fuera del host | **solo OD-5** | Principal B (`codex-desktop-session`, o una sesión nueva de Claude como variante registrada aparte); **0 invocaciones de modelo** | QH con T2 planificada | igualdad total campo a campo con el oráculo y aislamiento no UNVERIFIED (D.6) | terminación no acreditada, B en BELOW_REQUIRED o UNKNOWN para RESUME_DECISION, o aislamiento UNVERIFIED → UNVERIFIED; diferencia o lectura prohibida → FAIL | SHA-256 del oráculo publicado antes; respuesta de B; preflight de B; enumeración de entradas automáticas; auditoría de lecturas; N11 (hecho retirado) |
| **FX-04b** (C-25b, OV-I62-05 b) | continuación por B hasta VERIFIED y Q7 | FX-04a; `repo-write` de B para CUSTODY; Worker lanzable desde B con escritura | OD-5, OD-2, OD-4, OD-7 | B + `codex-cli` (Controller y Worker con escritura) | QR → Q0 de la ventana k+1 | VERIFIED sobre G' y Q7 conforme a B.8.4 | falta una OD: UNVERIFIED; capacidad medida ausente: UNSUPPORTED. No altera FX-04a | QR, Q0, verificación de G', Q7 |
| **FX-05** (C-27) | el plano (c) no actúa sobre el plano (a) | fixture | OD-5 | — | — | rechazo (P-16) de un artefacto o acción que nombre una unidad real; estado real intacto (refs remotas, `state/I-62.yml`, decisiones antes y después) | sin OD-5: UNVERIFIED | comparación del estado real antes y después; búsqueda de identificadores reales |
| **FX-06** (C-39, OV-I62-06) | autonomía real: A publica X con un defecto sembrado; Architect B → CHANGES REQUIRED; corrección; Architect C materializado sin decisión intermedia → AGREED | `ReviewLoopAuthorization` del Coordinator del fixture; binding de Architect con invocación medida | OD-5, OD-7 y (OD-2 u OD-3) | A `claude-desktop-session`; B y C `codex-cli` (o `claude-cli` con OD-3) | unidad de prueba I62_DELEGATED con la autorización | pasos 1-8 con `OWNER_AS_MESSAGE_BUS` = false en 1-7 | falta una OD, binding sin invocación medida, o B no produce CHANGES REQUIRED: UNVERIFIED; ningún runtime puede ser Architect independiente: UNSUPPORTED; relevo manual: AUTONOMY_GAP, nunca PASS | estado `orchestration` en cada QU, resultados custodiados, auditoría de transporte |

## 2. Grafo de dependencias

```text
FX-01 ─┬─ OD-5 ─ F1 (16.15-16.16) + F2 (preflight/v1, claude-desktop-session) + F4 (BOOTSTRAP/G0/QU) ─ fixture D.1 ─ sin CI ─ PASS | UNVERIFIED
FX-02 ─┼─ OD-5 + OD-7 + OD-2 ─ F2 + F3 (binding, contratos /v2, 14 comprobaciones) + F4 (Q0/Q7, diario) ─ ventana k ─ CI del fixture ─ PASS | UNVERIFIED
FX-03 ─┼─ OD-5 + OD-3 + OD-4 + OD-2 ─ F2 + F3 + F4 ─ topología B ─ CI del fixture ─ PASS | UNVERIFIED | UNSUPPORTED
FX-04a ┼─ OD-5 ─ F4 (QH, B.8.5 caso 1, NextAction) + F2 (preflight RESUME_DECISION de B) ─ QH ─ sin CI ─ PASS | FAIL | UNVERIFIED
FX-04b ┼─ OD-5 + OD-2 + OD-4 + OD-7 ─ F3 + F4 (QR, T16/T22) ─ ventana k+1 ─ CI del fixture ─ PASS | FAIL | UNVERIFIED | UNSUPPORTED
FX-05 ─┼─ OD-5 ─ F4 (planos, P-16) ─ fixture ─ sin CI ─ PASS | UNVERIFIED
FX-06 ─┴─ OD-5 + OD-7 + (OD-2 | OD-3) ─ F3 (role-invocation, resultados, binding materializado) + F4 (orchestration, intentos, fidelidad, B.11) ─ autorización ─ CI del fixture (paso 5) ─ PASS | FAIL | UNVERIFIED | UNSUPPORTED
```

## 3. Qué puede avanzar sin OD-3, OD-4, OD-5 u OD-7

- **Sin ninguna OD:** el diseño del fixture (D.1) y su `FIXTURE-MANIFEST.json` **como plan**. Que la ruta de los oráculos esté preparada. La regla de que el
  oráculo de FX-04a se calcula desde el estado canónico en QH, y se publica solo su SHA-256, ya está fijada en D.3.
- **FX-04a necesita solo OD-5**, más que FX-02 (o un QH tras el BOOTSTRAP) exista en el fixture. **No** depende de OD-3, OD-4, OD-7 ni de la escritura de B (D.4).
  Ruta mínima: OD-5 → fixture → FX-U1 hasta QH con T1 planificada → terminación de A atestada por el Owner → B en un clon limpio → respuesta → comparación.
  Sin FX-02, el resultado se registra «con hechos más pobres» (D.3).
- **FX-01 y FX-05:** solo OD-5.
- **Bloqueados sin OD-7:** FX-02, FX-04b y FX-06 (sin CI no hay VERIFIED); el cierre de F6 depende de OD-7 (D.2).
- **Bloqueados sin OD-2:** toda invocación de `codex-cli` (FX-02, FX-03, FX-04b, FX-06 por `codex-cli`, sondas).
- **Bloqueados sin OD-3 u OD-4:** FX-03; FX-04b sin OD-4.

## 4. Topes (D.3; ninguno autoriza gasto ahora)

| Ronda | Codex | Subagentes Claude | Claude CLI | Sesiones de Principal |
|---|---|---|---|---|
| A (FX-01, FX-02, FX-05) | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |
| FX-04a | 0 | 0 | 0 | ≤ 2 (+1 B2) |
| FX-04b | ≤ 6 | 0 | 0 | 0 nuevas |
| B (FX-03) | ≤ 12 | 0 | ≤ 6 | ≤ 2 |
| FX-06 | Architect ≤ 4 | 0 | (alternativa) | ≤ 2 |

> **Revisión del 2026-10-04:** las recetas ejecutables y el diseño del fixture están en [f6/recipes.md](f6/recipes.md); el ensayo mecánico de FX-04a, en
> [f6/fx04a/](f6/fx04a/fx04a_proto.py).
