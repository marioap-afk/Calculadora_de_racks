# I-62 — F7: borrador factual de la entrada de FOUNDATIONS (revisión 2026-10-07 r2; staging, no normativo)

> **Preparación de staging.** Carril «F7 y READY» de la orden nocturna (decisiones §54, fila «F7 y READY»: «borrador factual de FOUNDATIONS … sin
> FOUNDATIONS definitivo»). No se publica ni se edita `docs/FOUNDATIONS.md`: la entrada se publica en el cierre documental (Proposal V14 §17, filas
> «F7» y «Cierre documental»; WORKFLOW §11.4: «El commit de cierre concentra … entradas FOUNDATIONS ya conformadas»). Quien extiende una fundación redacta la
> entrada factual **antes de READY-04** para que la revise la conformidad (LIFECYCLE §4.1). Sustituye, como borrador, la revisión del 2026-10-06 de
> `docs/automation/evidence/I-62-prep/f7-ready-closure.md` §1.1 (se conserva como historial) y la r1 de esta noche. **r2** aplica la revisión del carril:
> alcance de 16.13 para toda unidad, hechos I61 vigentes conservados, formato de una línea por campo y limitaciones sin resultados supuestos.

## 0. Base medida de esta revisión (MEASURED, 2026-10-07, sin fetch: refs locales)

| Hecho | Valor | Cómo |
|---|---|---|
| Punta de I-62 | `e9473425aa9e33dfc8f9cb5a1522a20405fda68f` (= `origin/architecture/portabilidad-coordinador-principal` local) | `git rev-parse` |
| `origin/main` local | `bb0d5522` (I-63 integrada; merge-base de I-62 = `bb0d5522`) | `git merge-base` |
| `MC_I62` | `6f0187cb` (cierre de F4, decisiones §44) | estado `f4_status` |
| Superficies de 16.13 cambiadas desde `MC_I62` hasta la punta | **0 archivos** | `git diff --name-only 6f0187cb e9473425 -- <lista cerrada de 16.13>` |
| C-20b en seco sobre la punta (no es la C-20b de integración) | `EQUAL (MV-2..MV-6)`, 31 archivos, 70 entradas (15 MODIFIED, 53 ADDED, 2 ENTRY) | `clause_map.py check bb0d5522 e9473425` → [measured/c20b-dryrun-e9473425.json](measured/c20b-dryrun-e9473425.json) |
| ADR-0048 | «propuesto», blob `e1bd8d91` (igual en `MC_I62` y en la punta) | `git rev-parse <rev>:docs/adr/0048-…md` |
| Freeze | Proposal V14 blob `34ad80ea`; registro blob `0f6b8608`; FREEZE_SHA `b64a3b64` con imagen `fb49fceb` en la rama (rebase del 2026-10-05, patch-id igual; evidencia §44.1) | `git merge-base --is-ancestor`; `rebase-map.json` |
| A-1 | AGREED (decisiones §43): commit `ca09ade8`, blob `c01899a7` (ancestro de la punta) | `git rev-parse HEAD:docs/initiatives/I-62-A-1.md` |
| Entrada vigente | `docs/FOUNDATIONS.md` (blob `748c74ba` en la punta), líneas 160-169, «Agent Execution Protocol», `Last changed by: I-61`; una línea por campo, como todas las entradas | lectura del archivo en `e9473425` |
| Catálogo | `docs/automation/agent-execution/model-catalog.md` blob `166d978d` en la punta; entradas Codex con «Fecha de verificación: 2026-09-30» | `git rev-parse HEAD:<ruta>`; lectura |

## 1. Forma de la publicación (abierta: Q3)

- El contrato declara `extends: [Agent Execution Protocol]` e `introduces: []`. LIFECYCLE §4.1 exige que quien extiende redacte antes de READY-04 la entrada
  factual que revisará la conformidad; el «Esquema de entrada» de FOUNDATIONS fija los campos y las condiciones de `STABLE`. **Ninguna de las dos cláusulas
  dice si una extensión reescribe la entrada existente o crea otra.** Es la pregunta abierta Q3 (`closure-integration-checklist.md` §10).
- **Forma R** (texto de §2): reescribir la entrada existente. Conserva todos los hechos I61 que siguen siendo verdad, porque las unidades I61 siguen en I61
  toda su vida y los esquemas `/v1` no se retiran (16.14; V14 §14.1), y añade lo I62. Cada cambio de un hecho I61 queda en §4.1 con su evidencia.
- **Forma N** (esbozo en §2.1): entrada nueva para lo I62; la entrada I61 se conserva `STABLE` o se marca `SUPERSEDED -> <entrada nueva>` (valores de
  `Status` del «Esquema de entrada»), con el motivo en la evidencia del cierre.
- **Discrepancia con la preparación anterior:** `closure-plan.md` §1 dice «entrada nueva»; `f7-ready-closure.md` §1.1 dice «extiende». Este borrador redacta
  R y esboza N; decide el Coordinator.
- `Status: STABLE` exige como fuente un ADR aceptado o un Freeze integrado (FOUNDATIONS, «Esquema de entrada»). Para las partes I61 se cumple con ADR-0046
  aceptado y el Freeze de I-61 integrado. Las partes I62 solo tendrán Freeze integrado tras el merge efectivo, y ADR-0048 aceptado solo tras OD-1.

## 2. Texto del borrador, Forma R

Reglas de redacción: una línea por campo, con la misma disposición y la misma ortografía sin tildes que la entrada vigente (líneas 160-169). Los
corchetes `[PENDIENTE: …]` se sustituyen por hechos antes de publicar; nada entre corchetes se publica. Los rótulos «(I61)» e «(I62)» delimitan alcance.

```text
Name: Agent Execution Protocol
Status: STABLE
Authority: `AUTOMATION_PLAN.md` §16 fija la ejecucion delegada (participantes, relevo, aceptacion A1-A8, identidad, conteo, verificacion de 14 comprobaciones y STOP), que rige sin cambios a las unidades I61 toda su vida; desde `I62_EFFECTIVE_SHA` (16.14) [PENDIENTE: SHA del merge efectivo; se registra en el tag integration/I-62, no aqui] y para toda unidad, WORKFLOW §12 obliga a quien evalua un contrato a aplicar el resolver de compatibilidad de §16.13 antes de §16.3, y una unidad I61 solo cambia la revision en la que lee las clausulas del mapa (`I62_EFFECTIVE_SHA^1`); (I62) para las unidades I62_DELEGATED rigen ademas las partes I62 de §16: roles sin proveedor (bloque I62 de 16.1), perfil del Principal y autoverificacion (16.15), `CONFIGURATION_STATUS` (16.16), renderizado (16.17), observacion de capacidad (16.18), adapters (16.19), binding y aceptacion (16.20), independencia (16.21), aceptacion del paquete y verificacion (16.22), invocacion de rol y contratos de salida (16.23), cierre de insumos, identidad y fidelidad (16.24), custodia (16.25), recuperacion (16.26), conteo (16.27), arranque, adopcion y marcadores (16.28), bucles de revision (16.29) y planos y MaterializationClose (16.30); WORKFLOW §3 rige el relevo entre sesiones y AGENTS.md la evidencia.
Persistence: trafico transitorio en `artifacts/orchestration/` (ignorado por Git); (I61) custodia de JSON y MD en `docs/automation/evidence/<unit>-pilot/` y esquemas `rackcad-*/v1` en `docs/automation/agent-execution/schemas/`, que no se retiran; (I62) custodia en `docs/automation/evidence/<unit>-agent/`, incluidos `bootstrap/` y `rebase/<RunId>/rebase-map.json`; esquemas del conjunto I62 en `docs/automation/agent-execution/schemas/` (preflight/v1, relay-record/v2, controller-verification/v2, binding/v1, gate-contract/v2, delegation/v2, role-invocation/v1, input-closure/v1, input-fidelity/v1, architect-review-result/v1, reviewer-result/v1, automation-state/v2, normative-dependency-manifest/v1) y esquemas de hechos por adapter en `schemas/adapters/`; descriptores en `docs/automation/agent-execution/adapters/`; mapa de clausulas `docs/automation/agent-execution/compatibility/I62-clause-map.json` (esquema clause-map/v1 en la misma carpeta), leido en `I62_EFFECTIVE_SHA`, cuyo blob repite el tag integration/I-62.
Mutation contract: las reglas cambian solo en AUTOMATION_PLAN §16 con sus autoridades; un esquema cambia por version nueva (`/v2`) con ADR o A-n; el catalogo de modelos es mutable con fuente y fecha, sin Freeze; (I62) un adapter nuevo se añade con su descriptor y su esquema de hechos sin tocar el nucleo, y un cambio posterior a las superficies normativas de una unidad invalida su MaterializationClose (16.30).
Extension point: entrada de catalogo; clase de tarea o perfil nuevo en `routing.md` o PROMPT_TEMPLATES §G, revisado por el Coordinator contra el Freeze §§4 y 7; version nueva de esquema; regla de §16 (Freeze de I-61 §15); (I62) adapter nuevo (descriptor + esquema de hechos) y requisito por perfil y accion en `routing.md` §8.
Decision source: [ADR-0046](adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md) (aceptado) y Freeze de I-61 ([Proposal V9](initiatives/I-61-proposal-v9.md)); [ADR-0048](adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md) [PENDIENTE OD-1: «(aceptado), sucesor parcial de ADR-0046» | «(propuesto)»] y Freeze de I-62 ([Proposal V14](initiatives/I-62-proposal-v14.md), blob `34ad80ea`; [Consensus Freeze](initiatives/I-62-consensus-freeze.md), blob `0f6b8608`) con la enmienda [A-1](initiatives/I-62-A-1.md) AGREED (blob `c01899a7`).
Protecting tests: `AgentExecutionProtocolTests` (OBL-01..06 y OBL-11; oraculos intactos); `I61EditedRackNameTests`, `I61CamaEditWiringGuardTests` y `FlowBedEditorWindowTests.I61_P3` (piloto de I-61); controles nc1..nc4, OBL-08 y OBL-09 del piloto de G3 de I-61; `PrincipalPortabilityProtocolTests` (guardas de F1-F3 de I-62); `tests/RackCad.Tests/I62F4*Tests` (validador de state/v2, invariantes de archivo, pares e historia, orquestacion, compatibilidad C-20a, regresion de A-1, controles reproducibles de C-15..C-42 sobre Git real); MC de C-04, C-06, C-07, C-08, C-10, C-20b, C-20c y C-28; controles del fixture (plano c): C-22, C-23 y C-27 PASS [PENDIENTE: C-24, C-25a, C-39, C-42 (7, 8) y las partes F6 de C-32 y C-37; limitaciones decididas por el Owner].
Known limitations: (I61) independencia parcial de la verificacion con un Worker subagente; efecto de consumo del `service_tier` heredado UNKNOWN; recetas dependientes de Windows y del sandbox unelevated; nivel A, sin scripts; la medicion de procesos da falsos positivos por apps del host y otras sesiones; sin clase de routing para la sesion principal en las unidades I61 y DIRECT_ONLY (seguimiento en ideas-futuras); (I62) el modelo servido detras de un proveedor no es observable; la autoverificacion depende de una fuente RUNTIME_OBSERVED por adapter; la auditoria de lecturas no captura lecturas internas del runtime; Git no prueba quien opera en otra maquina; Worker `codex-cli` en `workspace-write`: no puede hacer commit (sandbox) y añade a `config.toml` una entrada de confianza del directorio nuevo (P-01), mientras que la receta `read-only` de 16.4 no la añade; observado en 2 de 2 actualizaciones automaticas de la app de Codex (2026-10-06 y 2026-10-07): reescribieron valores de `config.toml` y sustituyeron el binario, y cada una dejo `codex-cli` en P-01 hasta una linea base exacta nueva; con el catalogo [PENDIENTE: blob de `model-catalog.md` vigente al publicar; hoy `166d978d`, entradas Codex verificadas el 2026-09-30] no hay celda Codex elegible de nivel Frontera, asi que un Principal `codex-desktop-session` no cumple PRINCIPAL_COORDINATION; FX-04a: [PENDIENTE: resultado clasificado por el Coordinator] con una variante de mismo proveedor, sin afirmar portabilidad entre proveedores; [PENDIENTE: decisiones del Owner sobre las limitaciones de OV-I62-04 (topologia B) y OV-I62-05 (b), cuando existan].
Last changed by: I-62
```

### 2.1 Esbozo de la Forma N (alternativa de Q3; no se redacta entera hasta que el Coordinator la elija)

- **Entrada I61** (líneas 160-169): sin cambios en sus líneas. `Status`: `STABLE` o `SUPERSEDED -> <entrada nueva>`, con el motivo en la evidencia del cierre.
  Consideración para Q3, no regla: las unidades I61 siguen rigiéndose por esa fundación toda su vida (16.14).
- **Entrada nueva**: `Name: <nombre que fije el Coordinator>`; `Status: STABLE` con las mismas condiciones de §1; `Authority`: el resolver de 16.13 para
  toda unidad desde `I62_EFFECTIVE_SHA` (WORKFLOW §12) y las partes I62 para las unidades I62_DELEGATED (mismo texto que en R, sin la parte I61);
  `Persistence`, `Mutation contract`, `Extension point` y `Known limitations`: solo las partes «(I62)» de R; `Decision source`: ADR-0048 + Freeze de I-62 +
  A-1; `Protecting tests`: las guardas y controles de I-62 de R; `Last changed by: I-62`.
- Hecho a considerar: hoy el contrato declara `introduces: []` y `extends: [Agent Execution Protocol]`; con N, el Coordinator dispone si esos campos cambian.

## 3. Trazabilidad de cada afirmación nueva, cambiada o conservada (no se publica; sirve a la revisión de READY-06 y a DC-08)

| Afirmación | Evidencia (ruta, SHA) | Estado |
|---|---|---|
| Hechos I61 conservados (Authority I61, Persistence I61, «con ADR o A-n», «sin Freeze», Extension point I61, Decision source I61, Protecting tests I61, Known limitations I61) | `docs/FOUNDATIONS.md` líneas 160-169 en `e9473425` (blob `748c74ba`); 16.14 y V14 §14.1 («Unidades I61: siguen en I61 toda su vida … Los esquemas `/v1` no se retiran»); clases presentes en `e9473425`: `tests/RackCad.Tests/AgentExecutionProtocolTests.cs`, `I61EditedRackNameTests.cs`, `I61CamaEditWiringGuardTests.cs`, `tests/RackCad.UI.Tests/FlowBedEditorWindowTests.cs` (`I61_P3`); `artifacts/` ignorado por `.gitignore` línea 3 | HECHO (I61); DC-08 lo repite en la base de READY-04 |
| 16.13 rige para toda unidad desde `I62_EFFECTIVE_SHA`; WORKFLOW §12 obliga a aplicarlo antes de 16.3; I61 solo cambia la revisión de lectura de las cláusulas del mapa | AUTOMATION_PLAN 16.13 («Rige desde `I62_EFFECTIVE_SHA` … para toda unidad»); 16.14 («La excepción declarada es el resolver de compatibilidad de 16.13 … rige la lectura de autoridades de toda unidad»); V14 §14.1 («toda unidad lee en `MainSha`»; «obliga a quien evalúa un contrato … a aplicar §16.13 antes de 16.3»; «Para unidades I61 solo cambia … la revisión»); V14 §15 plano (b) | HECHO (texto materializado inactivo) |
| Partes I62 solo para I62_DELEGATED | 16.14 («Desde `I62_EFFECTIVE_SHA` rigen solo para las unidades I62 … opt-in»); bloque «Unidades I62» de 16.1 | HECHO (texto materializado inactivo) |
| Blobs del Freeze en `Decision source` | precedente: entrada de I-63 en `docs/FOUNDATIONS.md` línea 177 (commit y blob del Freeze) | HECHO de formato |
| C-22 PASS | decisiones §51; cadena BOOTSTRAP `1746b404`, G0 `5a3a7d69`, QU `1a4fc9c6`, contrato T1 `d30fb6a9` (blob `628d89af`); `I-62-F6/FX-U1-chain/chain.json`; validador `validator-boot-qu.json` (0 en todas las categorías) | HECHO (Coordinator) |
| C-23 PASS | decisiones §51; `I-62-F6/FX-01/R20261006T203145Z-fx01/result.json` (4 preflights; desviación no material: apertura en `xhigh`) | HECHO (Coordinator) |
| C-27 PASS | decisiones §47; `I-62-F6/FX-05/R20261006T072900Z-fx05/fx05-result.json` (corrida 2; la 1 inválida se conserva) | HECHO (Coordinator) |
| F6-OBS-01 no material | decisiones §51; reparación QR `ead6119f` / QH2 `cabed547`, `validator-qr-qh2.json` | HECHO (Coordinator); no entra en la entrada (no es limitación del protocolo) |
| F6-OBS-02 no material | decisiones §53 (B1 INVALID_TEST_ORACLE: defecto del arnés del fixture); `kits/FX-04a/comparison-contract-v2.md`; oráculo v2 `5e7a78d3…` durable en `707b4daa` | HECHO (Coordinator); no entra en la entrada |
| Worker Codex con escritura: de UNKNOWN (I61) a «no puede hacer commit» | `I-62-F6/OD-4/R20261006T072306Z-od41/result.json` (`.git/index.lock` denegado) | MEDIDO; sustituye la limitación I61 «Worker Codex con escritura UNKNOWN» (§4.1) |
| `workspace-write` crea entrada de confianza; `read-only` no | `I-62-F6/OD-4/R20261006T072306Z-od41/result.json` (`091540ED…` → `9002E854…`, +1 sección); `OD-2b-PROBE/R20261006T150704Z-od2b/result.json` (2 de 2 sin cambio) | MEDIDO |
| Actualizaciones de la app de Codex: valores de `config.toml` y binario, observado en 2 de 2 | 1.ª: `OD-2/R20261006T191621Z-p01/result.json` (`config.toml` escrito 2026-10-06T18:58:51Z, observado 19:16:21Z; `9002E854…` → `723A6898…`; app `26.930.3930.0` → `26.930.7945.0`; evidencia §67). 2.ª: evidencia §71 y §75, `OD-2/R20261007T061915Z-od2d-night-passive/result.json` (binario escrito 2026-10-07T02:14:49Z, `config.toml` 03:29:05Z; `723A6898…` → `9EA26634…`; app `26.1002.6548.0`; binario `97c57e4e…`; 9 claves de `mcp_servers.node_repl` y `notify`) | MEDIDO (2 de 2); sin generalizar a toda actualización |
| Sin celda Codex Frontera elegible | `model-catalog.md` blob `166d978d` (`gpt-6-astra` Frontera de créditos/API, no elegible; `gpt-6.1-sol` Equilibrado; `gpt-6-luna` Eficiente; verificación 2026-09-30); routing §8 (`PRINCIPAL_COORDINATION.level` = Frontera); evidencia §71; decisiones §52 | HECHO de catálogo, con blob y fecha (mutable) |
| «Sin clase de routing para la sesión principal» acotado a I61/DIRECT_ONLY | routing §8 (PRINCIPAL_COORDINATION, unidades I62); 16.15; `ideas-futuras-disposition.md` §1 (residuo para I61 y DIRECT_ONLY) | HECHO (texto materializado inactivo) |
| FX-04a con variante de mismo proveedor | decisiones §52 («no se afirma portabilidad entre proveedores»); B1 INVALID_TEST_ORACLE (§53); B2 HUMAN_LAUNCH_REQUIRED (§54, evidencia §75); resultados posibles de B2: PASS, FAIL o UNVERIFIED (§53) | PENDIENTE del resultado clasificado |
| Limitaciones de OV-I62-04 y OV-I62-05 (b) | V14 §18 y D.4 (decisión del Owner sobre la limitación; el escenario no se retira); hoy: OD-3 = RECHAZAR (§46), C-25b UNSUPPORTED por disposición del Coordinator (§50-§54) sobre la sonda OD-4, con Q16 abierta | PENDIENTE de la decisión del Owner |
| C-24, C-25a, C-39, C-42 (7, 8), C-32/C-37 (F6) | `I-62-F6/README.md`, matriz de obligaciones | PENDIENTE |

## 4. Diferencias frente a la revisión del 2026-10-06 (`f7-ready-closure.md` §1.1)

1. **Protecting tests:** C-22 y C-23 pasan a PASS (decisiones §51); C-27 se conserva PASS (§47). La lista de pendientes ya no incluye C-22/C-23 y separa
   las limitaciones (decididas por el Owner) de lo pendiente de ejecución.
2. **Known limitations:** se añaden, como hechos medidos de F6: lo observado en 2 de 2 actualizaciones automáticas de la app de Codex; la ausencia de celda
   Codex Frontera elegible con el catálogo identificado por blob y fecha; FX-04a con una variante de mismo proveedor, con su resultado pendiente; el Worker
   Codex sin commit. Se precisa que `read-only` no crea entradas de confianza y `workspace-write` sí.
3. **Decision source:** se nombran los blobs del Freeze y de A-1 en lugar del FREEZE_SHA `b64a3b64`, que ya no es ancestro de la rama (imagen
   `fb49fceb`); ver Q12. Se elimina la nota «sin cambios antes de los pilotos de F6» (disposición de proceso, §47, no hecho de la fundación).
4. **Persistence:** se añaden la ubicación del mapa de cláusulas y las rutas `bootstrap/` y `rebase/` de 16.25 y 16.28.
5. **Retirado de r1** (no son limitaciones de la fundación): «La topología B depende de OD-3 (rechazada por el Owner)», decisión fechada que se sustituye por
   la decisión del Owner sobre la limitación de OV-I62-04 cuando exista; «Un mensaje de control de la sesión de supervisión … lo teclea el Owner», hecho del
   arnés (plano c) que pasa a `ideas-futuras-disposition.md` F6-N05.

### 4.1 Diferencias frente a la entrada vigente (`docs/FOUNDATIONS.md` líneas 160-169, `Last changed by: I-61`)

| Campo | Cambio | Motivo y evidencia |
|---|---|---|
| Authority | se conserva el texto I61; se añaden el resolver de 16.13 para toda unidad desde `I62_EFFECTIVE_SHA` (WORKFLOW §12) y las partes I62 para I62_DELEGATED | 16.13, 16.14, V14 §14.1 y §15 (fila 2 de §3) |
| Persistence | se conserva el texto I61 («ignorado por Git», «de JSON y MD», `/v1` en `schemas/`); se añade lo I62 | 16.25, 16.28; árbol de `agent-execution/` en `e9473425` |
| Mutation contract | se conservan «con ADR o A-n» y «sin Freeze»; se añaden el adapter nuevo y la invalidación del MaterializationClose (I62) | 16.19, 16.30 |
| Extension point | se conserva el texto I61 (PROMPT_TEMPLATES §G, Freeze de I-61 §15); se añaden adapter nuevo y routing §8 (I62) | 16.19; routing §8 |
| Decision source | se conservan ADR-0046 y el Freeze de I-61; se añaden ADR-0048 (estado según OD-1), el Freeze de I-62 y A-1 | V14 §18 (OD-1); decisiones §31 y §43 |
| Protecting tests | se conservan todas las pruebas y controles I61; se añaden los de I-62 | §3, filas 1 y C-22..C-27 |
| Known limitations | se conservan cinco limitaciones I61 sin cambio. **Cambian dos:** «Worker Codex con escritura UNKNOWN» pasa al hecho medido «no puede hacer commit (sandbox)»; «sin clase de routing para la sesión principal» se acota a las unidades I61 y DIRECT_ONLY. Se añaden las limitaciones I62 | OD-4 (`R20261006T072306Z-od41`); routing §8 y 16.15 |
| Last changed by | `I-61` → `I-62` | — |

Ningún hecho I61 se retira sin fila en esta tabla.

## 5. Pendiente antes de publicar (no lo hace esta preparación)

- Q3 (Forma R o N) y, si N, la disposición de los campos `extends`/`introduces` del contrato.
- Resultado de B2 y clasificación de FX-04a/C-25a por el Coordinator; FX-02/C-24 y FX-06/C-39 (o sus limitaciones decididas); Q16 sobre FX-04b; F6 GATE PASS.
- OD-1 (estado de ADR-0048) y la decisión de Q1/Q2 (momento de su edición e interacción con `MC_I62` y el mapa).
- Blob de `model-catalog.md` vigente al publicar (si cambió, se revisa el hecho de elegibilidad).
- DC-08 sobre la base de READY-04: comprobar que los nombres de clases de prueba y rutas citados existen en el SHA rebasado (LIFECYCLE §4.1).
- Conformidad de READY-06 sobre el texto final (LIFECYCLE §9).
