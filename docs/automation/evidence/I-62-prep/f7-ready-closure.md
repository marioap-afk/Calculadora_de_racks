# I-62 — F7, READY, Candidato y cierre documental (preparación)

> **Preparación.** Orden del Coordinator de [decisiones](../../decisions/I-62.md) §33. No se edita FOUNDATIONS, HANDOFF, ROADMAP ni el índice ADR: según
> WORKFLOW §11.4 y V14 §17 («Cierre documental»), se editan solo en el commit de cierre. No se abre ninguna ventana de ROADMAP.

## 1. F7 (V14 §17: preparación del cierre sin cambio normativo)

### 1.1 Borrador factual de la entrada de FOUNDATIONS

Se publicaría en el cierre documental (WORKFLOW §11.4). Describe la entrada «Agent Execution Protocol» que **extiende** I-62 (Extends, contrato §5).
**Revisión del 2026-10-06** (F1-F4 con GATE PASS; F6 en curso): los «previsto» de F3/F4 se sustituyen por hechos; lo que depende de F6, OD-1 o la
integración queda marcado «pendiente».

```text
Name: Agent Execution Protocol
Status: STABLE
Authority: AUTOMATION_PLAN §16 fija la ejecución delegada. Para las unidades I61 rige el texto de I-61 (leído en EFF^1 en las cláusulas que I-62 modificó,
  16.13). Para las unidades I62 rigen además las subsecciones I62: roles sin proveedor (16.1), vigencia (16.14), perfil del Principal y autoverificación
  (16.15), CONFIGURATION_STATUS (16.16), renderizado (16.17), observación de capacidad (16.18), adapters (16.19), binding y aceptación (16.20),
  independencia (16.21), aceptación del paquete y verificación (16.22), invocación de rol y contratos de salida (16.23), cierre de insumos, identidad y
  fidelidad (16.24), custodia (16.25), recuperación (16.26), conteo (16.27), arranque, adopción y marcadores (16.28), bucles de revisión (16.29) y planos y
  MaterializationClose (16.30); la compatibilidad (16.13) y el punto de entrada de WORKFLOW §12. WORKFLOW §3 rige el relevo entre sesiones y AGENTS.md la
  evidencia.
Persistence: tráfico transitorio en artifacts/orchestration/; custodia en docs/automation/evidence/<unit>-pilot/ (I61) y <unit>-agent/ (I62); esquemas
  rackcad-*/v1 (I61) y el conjunto I62 (preflight/v1, relay-record/v2, controller-verification/v2, esquemas de hechos por adapter, binding/v1,
  gate-contract/v2, delegation/v2, role-invocation/v1, input-closure/v1, input-fidelity/v1, architect-review-result/v1, reviewer-result/v1,
  automation-state/v2, normative-dependency-manifest/v1, clause-map/v1); descriptores en agent-execution/adapters/.
Mutation contract: las reglas cambian solo en AUTOMATION_PLAN §16 con sus autoridades; un esquema cambia por versión nueva; un adapter nuevo se añade con su
  descriptor y su esquema de hechos sin tocar el núcleo; el catálogo es mutable con fuente y fecha; un cambio posterior a las superficies normativas
  invalida el MaterializationClose (16.30).
Extension point: adapter nuevo (descriptor + esquema de hechos); entrada de catálogo; requisito por perfil y acción en routing.md §8; versión nueva de esquema.
Decision source: ADR-0046 (aceptado) y ADR-0048 (sucesor parcial; pendiente de OD-1; sin cambios antes de los pilotos de F6 por disposición del
  Coordinator, decisiones §47); Freeze de I-61 (Proposal V9) y de I-62 (Proposal V14, Consensus
  Freeze b64a3b64) con la enmienda A-1 AGREED.
Protecting tests: AgentExecutionProtocolTests (I-61, sin cambios); PrincipalPortabilityProtocolTests (F1-F3: C-01, C-03, C-05, C-09, C-10, C-11..C-14, C-19);
  guardas de F4 en tests/RackCad.Tests/I62F4*Tests (validador de state/v2, invariantes de archivo, pares e historia, orquestación, compatibilidad, regresión
  de A-1 y controles reproducibles de C-15..C-42 sobre Git real); MC de C-04, C-06, C-07, C-08, C-10, C-20b, C-20c y C-28; F6: C-27 PASS
  (FX-05); pendiente: C-22..C-26, C-39 y la parte F6 de C-32, C-37 y C-42.
Known limitations: el modelo servido detrás de un proveedor no es observable; la autoverificación depende de una fuente RUNTIME_OBSERVED por adapter; la
  auditoría de lecturas no captura lecturas internas del runtime; Git no prueba quién opera en otra máquina; `codex-cli` en `workspace-write` reescribe
  `config.toml` la primera vez que corre en un directorio (entrada de confianza; P-01); en `read-only`, la receta de 16.4, no la reescribe (medido en F6),
  y un reinicio o una actualización de la app de Codex también cambian la huella; el Worker
  `codex-cli` en `workspace-write` no puede hacer commit (sandbox; medido en F6); la topología B depende de OD-3 (rechazada por el Owner el 2026-10-06).
Last changed by: I-62
```

### 1.2 Disposición prevista de `docs/ideas-futuras.md` (blob actual `e5641f0e`)

| Candidato | Disposición propuesta |
|---|---|
| GAP-01, GAP-02, GAP-05 (adapters del Architect por CLI, resultado estructurado, ruta limpia) | se cierran si F3/F4 los cubren; si no, al backlog del protocolo |
| GAP-03 (el Coordinator sin transporte invocable) | límite declarado (§20.11); idea futura |
| GAP-09..GAP-12 (fidelidad y visibilidad del transporte) | cubiertos por §20.3.3 en F3/F4; queda la mitigación operativa de lecturas acotadas |
| Seguimiento de I-61, «sin clase de routing para la sesión principal» | resuelto por routing §8 y AUTOMATION_PLAN 16.15 (F1, F2) |
| SM-02 y SM-03 ([freeze-issues.md](freeze-issues.md)) | si F4 no los adopta, como idea futura |
| Deuda nc2 de I-64 para las unidades I61 | decisión del Master Coordinator (D en [f3-dossier.md](f3-dossier.md) §7); no es de I-62 |
| Lector YAML de las pruebas (DEP-F4-YAML) | según la decisión del Owner |

### 1.3 Estructura del paquete de OV

`docs/automation/evidence/I-62-ov/`:
- `README.md`: matriz OV-I62-01..06, asignación a I-62 y SHA del Candidato;
- un directorio por escenario, con el guion (de [ov-scripts.md](ov-scripts.md)), los artefactos producidos, la captura y el resultado.

La asignación OV (`ov_assignment_ref` del contrato, hoy vacío) la fija el Freeze (V14 §18). Su registro formal es parte de READY-08.

## 2. READY-01..09 (LIFECYCLE §8, en este orden)

| READY | Prerrequisito actual | Evidencia esperada | Autoridad | Bloqueo posible |
|---|---|---|---|---|
| 01 | alcance congelado completo | F0-F4 y F6-F7 cerrados o diferidos por autoridad competente; A-n versionada si cambia el Freeze (FC-01, FC-02) | Coordinator (+ Owner si es OWNER-RESERVED) | A-n pendientes de FC-01 y FC-02 |
| 02 | gates cerrados | GATE PASS de F1-F4, F6 y F7; decisiones y documentos versionados | Coordinator | F6 sin OD-5/OD-7 |
| 03 | sin REQUIRED abierto ni decisión material pendiente | OD-1 aceptada (ADR-0048 y deltas); OD-2..OD-7 resueltas o con su limitación decidida | Owner + Coordinator | OD-1 |
| 04 | fetch, preflight y rebase final (WORKFLOW) | rebase sobre `origin/main`; si existe un cierre previo, su ruta de retorno | sesión + WORKFLOW | conflictos con hermanas integradas (I-52, I-63, I-64) |
| 05 | focales y CI del SHA resultante | Core Full y UI Full locales; CI exacta 4/4 | AGENTS | — |
| 06 | conformidad CONFORMING de Architect + Coordinator sobre ese SHA | revisión de conformidad contra Freeze + A-n. Con OD-6 alternativa 1, es una revisión mayor sin SAME-SESSION ROLE, aunque esa regla no es retroactiva a I-62 (§11.4: «No es retroactiva a I-62») | Architect + Coordinator | NON-CONFORMING |
| 07 | árbol limpio; `HEAD` identificado | `git status` limpio | sesión | — |
| 08 | matriz OV completa | OV-I62-01..06 preparados para el SHA final; ningún escenario retirado sin el Owner | Coordinator + Owner | asignación OV vacía hoy |
| 09 | acuerdo exacto, diff permitido, Freeze inmutable, A-n visibles | comprobación de §6 de LIFECYCLE | Coordinator | — |

## 3. Requisitos de `FINAL_CANDIDATE_SHA`

- Core Full y UI Full sobre el SHA exacto (AGENTS); CI exacta con los cuatro jobs.
- OV-I62-01..06 sobre ese SHA, con el fixture resembrado (D.5).
- OD-1 (antes de READY-03) y las OD de F6 resueltas o con su limitación decidida.
- Conformidad final de Architect + Coordinator (READY-06).
- Después: cierre documental y la integración serializada (WORKFLOW §11.4-11.6).

**Evidencia que solo puede existir más tarde:**
- PRE y POST del merge efectivo;
- el blob del mapa de cláusulas en EFF;
- C-20b sobre el merge local;
- la CI posterior al merge;
- el tag `integration/I-62`.

## 4. Plan de cierre documental e integración

| Elemento | Plan | Conflicto previsto |
|---|---|---|
| FOUNDATIONS | publicar la entrada de §1.1 reescrita con hechos (WORKFLOW §11.4) | otras iniciativas que cierren antes y editen FOUNDATIONS: rebase |
| Índice ADR (`docs/adr/README.md`) | fila 0048 con su estado tras OD-1 | **I-52 ya modifica el índice** (fila 0036) e I-64 añadirá la 0047: serializar según el orden de integración |
| HANDOFF | solo al integrar o cerrar | ediciones de otras integraciones: rebase |
| ROADMAP | fila de I-62 en su ventana, solo en los momentos de WORKFLOW §2 | filas de I-52, I-63 e I-64 en la misma tabla: acuse de ventana, como en el bootstrap |
| Contrato y estado | `status` y estado final; `ov_assignment_ref` | — |
| Merge efectivo | el **único** commit con el trailer `Agent-Protocol-Normative: I-62`, merge `--no-ff` en first-parent de `origin/main`; tabla PRE en el cuerpo (snapshot `git ls-remote --heads origin` con ref, punta, commit de reclamo y `Claim-Id`; WORKFLOW §11.3) | ninguna activación parcial: entrada, §16.13, punteros, mapa y esquema en ese merge |
| Mapa de cláusulas | regenerado sobre el merge local y revisado contra E.5 antes del push (C-20b; E.7) | — |
| Tag `integration/I-62` | POST, ruta y blob del mapa, `FINAL_CANDIDATE_SHA`, `CLOSURE_SHA`, `MERGE_SHA` y la CI posterior al merge (precedente: `integration/I-56` e `integration/I-61`) | — |
