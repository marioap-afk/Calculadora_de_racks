# I-62 — Paquete de revisión del Architect (Proposal V9)

```text
PROPOSAL V9 — NOT REVIEWED
Architect      = REVIEW REQUIRED: una revisión formal limpia, que autorizará el Coordinator (V6 y V7 CHANGES REQUIRED; V8 no se envió al Architect)
Coordinator    = requisito R62-AUTO-01..20 incorporado (registro I-62-coordinator-requirement-auto.md)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V9 §11.4); sin decisión, no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v9.md                      blob 831e3a6a87a242c872cfa0755f73e282f6c043c8
  docs/initiatives/I-62-coordinator-requirement-auto.md     blob fcf941669782ccbfc1e32efa7343b2be16dd6ac4
Versiones anteriores:
  V6: commit 3888c5c8…, blob 19672958… (Architect: CHANGES REQUIRED)
  V7: commit 3d7ec77f…, blob 9f68c950… (Architect: CHANGES REQUIRED)
  V8: commit d66463a5b04f79911580e2ef6e6f67263b51b5c3, blob 667b59d7a433130e20a42bbc13d3e6f564350049 (histórica; no revisada por el Architect)
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v9.md` = `831e3a6a…`. Si no coincide, revisa la versión designada o rechaza la discordancia.

## 0. Condiciones de la revisión

Las fija el Coordinator al autorizarla, y V9 las propone en §20.3:
- sesión o proceso distinto de la sesión autora, en SEPARATE SESSION; declarar si revisor y autor son la misma persona (LIFECYCLE §5);
- clon o ruta limpios, sin la memoria de proyecto del autor;
- sin la transcripción del autor; solo los insumos canónicos de §2;
- declarar todo contexto inyectado automáticamente;
- solo lectura;
- con esta revisión, el resultado debería seguir ya, en lo posible, la forma de B.10 (`rackcad-review-result/v1`), para que se pueda ingerir sin copia
  manual. Si no es posible, se registra como AUTONOMY_GAP (§20.9).

**Alcance:** V9 completa. El foco mínimo es el capítulo nuevo §20 y su reflejo en los anexos B.8.8, B.9, B.10, C-29..C-39, D.8, F.8 y G.1. Las trazas son
análisis del diseño, no ensayos.

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, fuente o contraejemplo, por qué importa, corrección precisa.
Modo:      SEPARATE SESSION (declarado); si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

## 2. Lectura

1. [Proposal V9](I-62-proposal-v9.md) completa (anexos A-G), en especial §20.
2. [Requisito R62-AUTO del Coordinator](I-62-coordinator-requirement-auto.md); registros del Architect [V6](I-62-architect-review-v6.md) y
   [V7](I-62-architect-review-v7.md); registros del Coordinator [V1](I-62-coordinator-review-v1.md) … [V5](I-62-coordinator-review-v5.md).
3. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt).
4. Autoridades: LIFECYCLE §§2-9; WORKFLOW §§3-4 y 10-11; AGENTS; AUTOMATION_PLAN §§8 y 16; ADR-0046; el Freeze de I-61 ([Proposal V9 de I-61](I-61-proposal-v9.md));
   `routing.md`, `model-catalog.md` y el README de agent-execution.

## 3. Delta V8 → V9

| Área | V8 | V9 |
|---|---|---|
| Transporte entre roles | implícito: el Owner llevaba prompts y resultados entre roles fuera de la unidad delegada | **§20.1:** RELAY automático entre roles IA vinculables; ESCALATION_OWNER solo ante fronteras reales; COORDINATOR_DECISION como autoridad (no relevo); «tiene que ejecutarse otro rol» no es escalada |
| Papel del Principal | custodia, relevos y bindings | **§20.2:** también orquesta las invocaciones de rol elegibles. **AUT-I1:** no gana autoridad (nunca declara AGREED, cierra REQUIRED, elige decisiones del Owner ni declara Freeze o PASS) |
| Contratos | `gate-contract`, `delegation`, `worker-handoff`, `controller-verification`, `relay-record`, `preflight`, `binding` | **B.9 `role-invocation/v1`** (neutral, objeto exacto, permisos mínimos, autorización referenciada) y **B.10 `review-result/v1`** (estructurado, ingerible, coherencia del veredicto) |
| Estado | `next_action` en prosa | **B.8.8 `orchestration`**: bucle, objeto exacto, `next_action` estructurada, invocación pendiente y última, linaje de hallazgos, presupuestos, escalada, AUTONOMY_GAP. **I-S18, I-P13**. `NextAction` única o STOP (P-17) |
| Bucle del Architect | no existía | **§20.5:** máquina de estados con `ReviewLoopAuthorization` del Coordinator; corrección y re-revisión autónomas; linaje; solo el Architect cierra |
| Presupuestos | contadores de §9.3 | **§20.6:** cinco contadores del bucle con topes congelados propuestos (3, 3, 2 por linaje, 2 por invocación) y analogías en 16.8; sin reinicios por cambio de proveedor, modelo, sesión o etiqueta |
| Controller/Worker y Reviewer | relevo de la delegación | **§20.7:** el mismo orquestador, sin relevo del Owner en las iteraciones normales |
| Portabilidad | FX-04a/FX-04b | más **§20.8 y F.8**: un Principal sucesor reanuda un bucle de revisión activo |
| Manual | sin registro | **§20.9 AUTONOMY_GAP** (≠ PASS) |
| Nivel | Level A | **§20.10:** Level A explícito para la orquestación, sin servicio |
| Evidencia real | — | **§20.11:** GAP-01..GAP-06 observados en I-62 y triaje para el backlog de I-61 |
| DIRECT_ONLY | sin maquinaria I62 | **§20.12:** tampoco orquestación (C-28 e) |
| Fallos | P-09..P-16 | **P-17..P-21** |
| Gates, matriz, pilotos, OV | — | F3 (contratos), F4 (estado y bucle), F6 (FX-06); **C-29..C-39**; **FX-06** (D.8); **OV-I62-06**; criterio 15; G.1 |

## 4. Incorporación de R62-AUTO-01..20 (la sesión declara; el cierre lo decide quien revisa)

| Requisito | Dónde | Obligación de verificación |
|---|---|---|
| 01 | §20.1; §13 | C-32, C-33 |
| 02, 14 | §2; §20.2 | C-38 |
| 03, 13 | §20.3; B.9 | C-30 |
| 04, 16 | §20.4; B.8.8 | C-29, C-38 |
| 05, 06, 08 | §20.5; B.10; B.8.8; F.8 | C-31, C-35 |
| 07 | §20.6; B.8.8 | C-34, C-36 |
| 09, 10 | §20.7 | C-31 (como patrón), F.1 |
| 11 | §20.8; F.8 | C-29 |
| 12 | §12; D.8; §18 | C-39; OV-I62-06 |
| 15 | §20.9 | C-37 |
| 17 | §20.10 | — (restricción de diseño) |
| 18 | §20.11 | evidencia §20 |
| 19 | todo V9 | obligaciones 1-9 → C-29 (1), C-30 (2), C-31 (3), C-32 (4), C-33 (5), C-34 (6), C-35 (7), C-36 (8), C-37 (9) |
| 20 | §14.0; §20.12 | C-28 (e) |

## 5. Riesgos y preguntas para la revisión

1. ¿La `ReviewLoopAuthorization` del Coordinator es el mecanismo correcto para que la corrección autónoma no cree autoridad, o el alcance de corrección debe
   fijarse de otro modo?
2. ¿Son adecuados los topes propuestos para el Freeze (§20.6), derivados de las analogías de 16.8?
3. ¿La asignación conservadora de linaje (si es ambigua, mismo linaje) evita reinicios sin bloquear correcciones legítimas?
4. **GAP-03** (el Coordinator es un chat externo sin transporte invocable) queda fuera del alcance como límite declarado. ¿Debe I-62 proponer algo más?
5. FX-06 depende de que exista un Architect invocable con invocación medida (OD-2 u OD-3). ¿Es honesto dejarlo en UNVERIFIED si no?

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor: incluye la **orquestación autónoma** (§20) | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas (A, B, FX-04b, FX-06 con `codex-cli`) |
| OD-3 | autenticar Claude CLI | B; FX-06 si el Architect es `claude-cli` |
| OD-4 | sandbox de Codex para escritura | B y FX-04b |
| OD-5 | permiso de ensayo con la semántica única de §15, incluida la apertura y el consumo de FX-06 | F6 |
| OD-7 | remoto del fixture con CI | cierre de F6, FX-04b y FX-06 (paso 5) |

## 7. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.
