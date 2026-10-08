# I-62 — F7: borrador factual de la entrada de FOUNDATIONS (revisión 2026-10-07 r4; staging, no normativo)

> **Preparación de staging.** Carril «F7 y READY» de la orden nocturna (decisiones §54, fila «F7 y READY»: «borrador factual de FOUNDATIONS … sin
> FOUNDATIONS definitivo»). No se publica ni se edita `docs/FOUNDATIONS.md`: la entrada se publica en el cierre documental (Proposal V14 §17, filas
> «F7» y «Cierre documental»; WORKFLOW §11.4: «El commit de cierre concentra … entradas FOUNDATIONS ya conformadas»). Quien extiende una fundación redacta la
> entrada factual **antes de READY-04** para que la revise la conformidad (LIFECYCLE §4.1). Sustituye, como borrador, la revisión del 2026-10-06 de
> `docs/automation/evidence/I-62-prep/f7-ready-closure.md` §1.1 (se conserva como historial) y la r1 de esta noche. **r2** aplica la revisión del carril:
> alcance de 16.13 para toda unidad, hechos I61 vigentes conservados, formato de una línea por campo y limitaciones sin resultados supuestos. **r3**
> aplica la revisión de completitud F7/READY (§4.2): la entrada solo espera hechos fijados antes de READY-04, sin decisiones de la OV; ADR-0048
> «aceptado» como precondición; identidad del binario en las limitaciones de Codex. **r4** aplica decisiones §55 (§4.3): FX-04a OPEN / UNVERIFIED tras
> B2 (INVALID_LAUNCH), ninguna limitación de FX-04a aceptada (FX-04a solo entra en la entrada con PASS, que exige F6 GATE PASS) y tope congelado de
> sondas agotado (el hecho `read-only` solo se re-mide con el bloque nuevo de A-2, si se acuerda); tras su verificación, `Decision source` lleva el
> `[PENDIENTE]` de A-2 junto a A-1 (§4.3, punto 4).

## 0. Base medida de esta revisión (MEASURED, 2026-10-07, sin fetch: refs locales)

| Hecho | Valor | Cómo |
|---|---|---|
| Punta de I-62 | `e9473425aa9e33dfc8f9cb5a1522a20405fda68f` (= `origin/architecture/portabilidad-coordinador-principal` local) | `git rev-parse` |
| `origin/main` local | `bb0d5522` (I-63 integrada; merge-base de I-62 = `bb0d5522`) | `git merge-base` |
| `MC_I62` | `6f0187cb` (cierre de F4, decisiones §44) | estado `f4_status` |
| Superficies de 16.13 cambiadas desde `MC_I62` hasta la punta | **0 archivos** | `git diff --name-only 6f0187cb e9473425 -- <lista cerrada de 16.13>` |
| C-20b en seco sobre la punta (no es la C-20b de integración) | `EQUAL (MV-2..MV-6)`, 31 archivos, 70 entradas (15 MODIFIED, 53 ADDED, 2 ENTRY) | `python docs/automation/evidence/I-62-F4/compat/clause_map.py check bb0d5522 e9473425 <out.json>` (solo lectura; repetido el 2026-10-07 con el mismo resultado; `MapBlob` `4d49d3e1`) → [measured/c20b-dryrun-e9473425.json](measured/c20b-dryrun-e9473425.json) (SHA-256 `85c01bc8…`) |
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
corchetes `[PENDIENTE: …]` se sustituyen por hechos **antes de READY-04** (LIFECYCLE §4.1: «redacta antes de READY-04 la entrada factual»), solo con
hechos fijados por F6 GATE PASS y OD-1 y con hechos observados en la punta de la rama al redactar la entrada; nada entre corchetes se publica. DC-08 sobre
el SHA rebasado de READY-04 confirma esos hechos (§5); si alguno cambió, la corrección crea un SHA nuevo y reinicia READY-02 (LIFECYCLE §6: «Tras READY-04,
cualquier A-n o decision nueva crea un SHA nuevo y reinicia READY-02»; §8: «Cualquier correccion que cambie SHA invalida la identidad de evidencia y
reinicia READY-02»). El texto que revisa la conformidad de READY-06 es el que se publica, byte a byte (WORKFLOW §11.4: «entradas
FOUNDATIONS ya conformadas»; `closure-integration-checklist.md` §2). Las decisiones del Owner sobre limitaciones y los resultados de la OV sobre
`FINAL_CANDIDATE_SHA` no entran en la entrada: van a la evidencia OV y a la marca de cierre de ROADMAP. Los rótulos «(I61)» e «(I62)» delimitan alcance.

```text
Name: Agent Execution Protocol
Status: STABLE
Authority: `AUTOMATION_PLAN.md` §16 fija la ejecucion delegada (participantes, relevo, aceptacion A1-A8, identidad, conteo, verificacion de 14 comprobaciones y STOP), que rige sin cambios a las unidades I61 toda su vida; desde `I62_EFFECTIVE_SHA` (16.14) [PENDIENTE: SHA del merge efectivo; se registra en el tag integration/I-62, no aqui] y para toda unidad, WORKFLOW §12 obliga a quien evalua un contrato a aplicar el resolver de compatibilidad de §16.13 antes de §16.3, y una unidad I61 solo cambia la revision en la que lee las clausulas del mapa (`I62_EFFECTIVE_SHA^1`); (I62) para las unidades I62_DELEGATED rigen ademas las partes I62 de §16: roles sin proveedor (bloque I62 de 16.1), perfil del Principal y autoverificacion (16.15), `CONFIGURATION_STATUS` (16.16), renderizado (16.17), observacion de capacidad (16.18), adapters (16.19), binding y aceptacion (16.20), independencia (16.21), aceptacion del paquete y verificacion (16.22), invocacion de rol y contratos de salida (16.23), cierre de insumos, identidad y fidelidad (16.24), custodia (16.25), recuperacion (16.26), conteo (16.27), arranque, adopcion y marcadores (16.28), bucles de revision (16.29) y planos y MaterializationClose (16.30); WORKFLOW §3 rige el relevo entre sesiones y AGENTS.md la evidencia.
Persistence: trafico transitorio en `artifacts/orchestration/` (ignorado por Git); (I61) custodia de JSON y MD en `docs/automation/evidence/<unit>-pilot/` y esquemas `rackcad-*/v1` en `docs/automation/agent-execution/schemas/`, que no se retiran; (I62) custodia en `docs/automation/evidence/<unit>-agent/`, incluidos `bootstrap/` y `rebase/<RunId>/rebase-map.json`; esquemas del conjunto I62 en `docs/automation/agent-execution/schemas/` (preflight/v1, relay-record/v2, controller-verification/v2, binding/v1, gate-contract/v2, delegation/v2, role-invocation/v1, input-closure/v1, input-fidelity/v1, architect-review-result/v1, reviewer-result/v1, automation-state/v2, normative-dependency-manifest/v1) y esquemas de hechos por adapter en `schemas/adapters/`; descriptores en `docs/automation/agent-execution/adapters/`; mapa de clausulas `docs/automation/agent-execution/compatibility/I62-clause-map.json` (esquema clause-map/v1 en la misma carpeta), leido en `I62_EFFECTIVE_SHA`, cuyo blob repite el tag integration/I-62.
Mutation contract: las reglas cambian solo en AUTOMATION_PLAN §16 con sus autoridades; un esquema cambia por version nueva (`/v2`) con ADR o A-n; el catalogo de modelos es mutable con fuente y fecha, sin Freeze; (I62) un adapter nuevo se añade con su descriptor y su esquema de hechos sin tocar el nucleo, y un cambio posterior a las superficies normativas de una unidad invalida su MaterializationClose (16.30).
Extension point: entrada de catalogo; clase de tarea o perfil nuevo en `routing.md` o PROMPT_TEMPLATES §G, revisado por el Coordinator contra el Freeze §§4 y 7; version nueva de esquema; regla de §16 (Freeze de I-61 §15); (I62) adapter nuevo (descriptor + esquema de hechos) y requisito por perfil y accion en `routing.md` §8.
Decision source: [ADR-0046](adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md) (aceptado) y Freeze de I-61 ([Proposal V9](initiatives/I-61-proposal-v9.md)); [ADR-0048](adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md) (aceptado) [PENDIENTE: OD-1 = A, precondicion de READY-03 (V14 §18); sin via prevista para «propuesto» (od1-packet.md §6)], sucesor parcial de ADR-0046, y Freeze de I-62 ([Proposal V14](initiatives/I-62-proposal-v14.md), blob `34ad80ea`; [Consensus Freeze](initiatives/I-62-consensus-freeze.md), blob `0f6b8608`) con la enmienda [A-1](initiatives/I-62-A-1.md) AGREED (blob `c01899a7`) [PENDIENTE: A-2 y su blob si se acuerda (decisiones §55); sin A-2 AGREED no hay B3 ni F6 GATE PASS].
Protecting tests: `AgentExecutionProtocolTests` (OBL-01..06 y OBL-11; oraculos intactos); `I61EditedRackNameTests`, `I61CamaEditWiringGuardTests` y `FlowBedEditorWindowTests.I61_P3` (piloto de I-61); controles nc1..nc4, OBL-08 y OBL-09 del piloto de G3 de I-61; `PrincipalPortabilityProtocolTests` (guardas de F1-F3 de I-62); `tests/RackCad.Tests/I62F4*Tests` (validador de state/v2, invariantes de archivo, pares e historia, orquestacion, compatibilidad C-20a, regresion de A-1, controles reproducibles de C-15..C-42 sobre Git real); MC de C-04, C-06, C-07, C-08, C-10, C-20b, C-20c y C-28; controles del fixture (plano c): C-22, C-23 y C-27 PASS [PENDIENTE: estado de C-24, C-25a, C-39, C-42 (7, 8) y las partes F6 de C-32 y C-37 fijado en F6 GATE PASS, con su causa si no es PASS (D.4)].
Known limitations: (I61) independencia parcial de la verificacion con un Worker subagente; efecto de consumo del `service_tier` heredado UNKNOWN; recetas dependientes de Windows y del sandbox unelevated; nivel A, sin scripts; la medicion de procesos da falsos positivos por apps del host y otras sesiones; sin clase de routing para la sesion principal en las unidades I61 y DIRECT_ONLY (seguimiento en ideas-futuras); (I62) el modelo servido detras de un proveedor no es observable; la autoverificacion depende de una fuente RUNTIME_OBSERVED por adapter; la auditoria de lecturas no captura lecturas internas del runtime; Git no prueba quien opera en otra maquina; Worker `codex-cli` en `workspace-write` (medido con `codex-cli 0.160.0`, binario `37762753`, el 2026-10-06): no puede hacer commit (sandbox) y añade a `config.toml` una entrada de confianza del directorio nuevo (P-01), mientras que la receta `read-only` de 16.4 no la añade (medido con `0.160.0`/`37762753` el 2026-10-06 y con `0.160.1`/`3b8f6e33` el 2026-10-07) [PENDIENTE: solo el hecho `read-only`: resultado de las sondas de solo lectura del bloque de medicion nuevo para el binario instalado al redactar la entrada (antes de READY-04; hoy `97c57e4e`, version de la CLI sin medir), que solo existe si A-2 se acuerda (decisiones §55 U-04: tope congelado de sondas agotado); sin ese resultado, se publica solo con esta identidad y fecha, sin generalizar. Los dos hechos de `workspace-write` se publican solo con su identidad medida, sin re-medicion prevista (§3)]; observado en 2 de 2 actualizaciones automaticas de la app de Codex (2026-10-06 y 2026-10-07): reescribieron valores de `config.toml` y sustituyeron el binario, y cada una dejo `codex-cli` en P-01 hasta una linea base exacta nueva; con el catalogo [PENDIENTE: blob de `model-catalog.md` en la punta de la rama al redactar la entrada (antes de READY-04); hoy `166d978d`, entradas Codex verificadas el 2026-09-30; DC-08 lo confirma en el SHA de READY-04] no hay celda Codex elegible de nivel Frontera, asi que un Principal `codex-desktop-session` no cumple PRINCIPAL_COORDINATION; FX-04a [PENDIENTE: solo con FX-04a PASS, condicion de F6 GATE PASS (D.4; decisiones §55 U-01c); un FX-04a sin PASS no es limitacion publicable: deja F6 pendiente y no hay READY-04] medido con una variante de mismo proveedor, sin afirmar portabilidad entre proveedores; [PENDIENTE: estado con causa de FX-03 (topologia B) y de FX-04b fijado en F6 GATE PASS (D.4: UNVERIFIED o UNSUPPORTED con causa), y el de FX-06 si no es PASS; la decision del Owner sobre la limitacion (OV-I62-04, OV-I62-05 b, OV-I62-06) no entra en la entrada].
Last changed by: I-62
```

### 2.1 Esbozo de la Forma N (alternativa de Q3; no se redacta entera hasta que el Coordinator la elija)

- **Entrada I61** (líneas 160-169): sin cambios en sus líneas. `Status`: `STABLE` o `SUPERSEDED -> <entrada nueva>`, con el motivo en la evidencia del cierre.
  Consideración para Q3, no regla: las unidades I61 siguen rigiéndose por esa fundación toda su vida (16.14).
- **Entrada nueva**: `Name: <nombre que fije el Coordinator>`; `Status: STABLE` con las mismas condiciones de §1; `Authority`: el resolver de 16.13 para
  toda unidad desde `I62_EFFECTIVE_SHA` (WORKFLOW §12) y las partes I62 para las unidades I62_DELEGATED (mismo texto que en R, sin la parte I61);
  `Persistence`, `Mutation contract`, `Extension point` y `Known limitations`: solo las partes «(I62)» de R; `Decision source`: ADR-0048 + Freeze de I-62 +
  A-1 (y A-2 si se acuerda: decisiones §55 U-01b); `Protecting tests`: las guardas y controles de I-62 de R; `Last changed by: I-62`.
- Hecho a considerar: hoy el contrato declara `introduces: []` y `extends: [Agent Execution Protocol]`; con N, el Coordinator dispone si esos campos cambian.

## 3. Trazabilidad de cada afirmación nueva, cambiada o conservada (no se publica; sirve a la revisión de READY-06 y a DC-08)

| Afirmación | Evidencia (ruta, SHA) | Estado |
|---|---|---|
| Hechos I61 conservados (Authority I61, Persistence I61, «con ADR o A-n», «sin Freeze», Extension point I61, Decision source I61, Protecting tests I61, Known limitations I61) | `docs/FOUNDATIONS.md` líneas 160-169 en `e9473425` (blob `748c74ba`); 16.14 y V14 §14.1 («Unidades I61: siguen en I61 toda su vida … Los esquemas `/v1` no se retiran»); clases presentes en `e9473425`: `tests/RackCad.Tests/AgentExecutionProtocolTests.cs`, `I61EditedRackNameTests.cs`, `I61CamaEditWiringGuardTests.cs`, `tests/RackCad.UI.Tests/FlowBedEditorWindowTests.cs` (`I61_P3`); `artifacts/` ignorado por `.gitignore` línea 3 | HECHO (I61); DC-08 lo repite en la base de READY-04 |
| 16.13 rige para toda unidad desde `I62_EFFECTIVE_SHA`; WORKFLOW §12 obliga a aplicarlo antes de 16.3; I61 solo cambia la revisión de lectura de las cláusulas del mapa | AUTOMATION_PLAN 16.13 («Rige desde `I62_EFFECTIVE_SHA` … para toda unidad»); 16.14 («La excepción declarada es el resolver de compatibilidad de 16.13 … rige la lectura de autoridades de toda unidad»); V14 §14.1 («toda unidad lee en `MainSha`»; «obliga a quien evalúa un contrato … a aplicar §16.13 antes de 16.3»; «Para unidades I61 solo cambia … la revisión»); V14 §15 plano (b) | HECHO (texto materializado inactivo) |
| Partes I62 solo para I62_DELEGATED | 16.14 («Desde `I62_EFFECTIVE_SHA` rigen solo para las unidades I62 … opt-in»); bloque «Unidades I62» de 16.1 | HECHO (texto materializado inactivo) |
| Blobs del Freeze en `Decision source` | precedente: entrada de I-63 en `docs/FOUNDATIONS.md` línea 177 (commit y blob del Freeze) | HECHO de formato |
| A-2 en `Decision source` | decisiones §55 U-01b (B3 NO AUTORIZADA bajo el Freeze actual; si A-2 es AGREED, exactamente una B3) y U-01c (el cierre de F6 sigue exigiendo FX-04a PASS); LIFECYCLE §6 y §8 READY-09 («alcance y visibilidad de todas las A-n»); `frontiers.json` F-13 y F-16 | PENDIENTE: A-2 candidata, sin aplicar. La entrada solo se publica con F6 GATE PASS (FX-04a PASS), que bajo el Freeze actual exige una B3 y, por tanto, A-2 AGREED: en todo estado en que exista la entrada, A-2 se nombra con su blob junto a A-1 |
| C-22 PASS | decisiones §51; cadena BOOTSTRAP `1746b404`, G0 `5a3a7d69`, QU `1a4fc9c6`, contrato T1 `d30fb6a9` (blob `628d89af`); `I-62-F6/FX-U1-chain/chain.json`; validador `validator-boot-qu.json` (0 en todas las categorías) | HECHO (Coordinator) |
| C-23 PASS | decisiones §51; `I-62-F6/FX-01/R20261006T203145Z-fx01/result.json` (4 preflights; desviación no material: apertura en `xhigh`) | HECHO (Coordinator) |
| C-27 PASS | decisiones §47; `I-62-F6/FX-05/R20261006T072900Z-fx05/fx05-result.json` (corrida 2; la 1 inválida se conserva) | HECHO (Coordinator) |
| F6-OBS-01 no material | decisiones §51; reparación QR `ead6119f` / QH2 `cabed547`, `validator-qr-qh2.json` | HECHO (Coordinator); no entra en la entrada (no es limitación del protocolo) |
| F6-OBS-02 no material | decisiones §53 (B1 INVALID_TEST_ORACLE: defecto del arnés del fixture); `kits/FX-04a/comparison-contract-v2.md`; oráculo v2 `5e7a78d3…` durable en `707b4daa` | HECHO (Coordinator); no entra en la entrada |
| Worker Codex con escritura: de UNKNOWN (I61) a «no puede hacer commit» | `I-62-F6/OD-4/R20261006T072306Z-od41/result.json` (`.git/index.lock` denegado; `Binary.CliVersion` `codex-cli 0.160.0`, `Binary.Sha256` `37762753…`, 2026-10-06) | MEDIDO con esa identidad; sustituye la limitación I61 «Worker Codex con escritura UNKNOWN» (§4.1); el binario vigente `97c57e4e…` no se ha medido (evidencia §75; paquete OD-2d vigente: versión de la CLI «Sin medir») y no hay re-medición prevista (fila siguiente) |
| `workspace-write` crea entrada de confianza; `read-only` no | `I-62-F6/OD-4/R20261006T072306Z-od41/result.json` (`091540ED…` → `9002E854…`, +1 sección; `0.160.0`/`37762753…`); `OD-2b-PROBE/R20261006T150704Z-od2b/result.json` (2 de 2 sin cambio; `0.160.0`/`37762753…`, 2026-10-06); `OD-2/R20261007T013200Z-od2d-probe/result.json` (2 sondas `read-only`, huella `723A6898…` estable; `0.160.1`/`3b8f6e33…`, 2026-10-07) | MEDIDO con esas identidades. Los dos hechos de `workspace-write` (sin commit; entrada de confianza añadida) se publican solo con su identidad medida (`codex-cli 0.160.0`, `37762753…`, 2026-10-06), sin re-medición prevista: decisiones §50 («sin repetir la sonda de escritura»), §51 (OD-2d-PROBE «sin `workspace-write`») y §54 («sin reintentar la escritura de Codex»). Solo el hecho `read-only` puede re-medirse con `97c57e4e…`, y solo con las sondas de solo lectura del bloque de medición nuevo que propone A-2 (decisiones §55 U-04: tope congelado agotado; ninguna sonda de modelo hasta el acuerdo de A-2) |
| Actualizaciones de la app de Codex: valores de `config.toml` y binario, observado en 2 de 2 | 1.ª: `OD-2/R20261006T191621Z-p01/result.json` (`config.toml` escrito 2026-10-06T18:58:51Z, observado 19:16:21Z; `9002E854…` → `723A6898…`; app `26.930.3930.0` → `26.930.7945.0`; evidencia §67). 2.ª: evidencia §71 y §75, `OD-2/R20261007T061915Z-od2d-night-passive/result.json` (binario escrito 2026-10-07T02:14:49Z, `config.toml` 03:29:05Z; `723A6898…` → `9EA26634…`; app `26.1002.6548.0`; binario `97c57e4e…`; 9 claves de `mcp_servers.node_repl` y `notify`) | MEDIDO (2 de 2); sin generalizar a toda actualización |
| Sin celda Codex Frontera elegible | `model-catalog.md` blob `166d978d` (`gpt-6-astra` Frontera de créditos/API, no elegible; `gpt-6.1-sol` Equilibrado; `gpt-6-luna` Eficiente; verificación 2026-09-30); routing §8 (`PRINCIPAL_COORDINATION.level` = Frontera); evidencia §71; decisiones §52 | HECHO de catálogo, con blob y fecha (mutable) |
| «Sin clase de routing para la sesión principal» acotado a I61/DIRECT_ONLY | routing §8 (PRINCIPAL_COORDINATION, unidades I62); 16.15; `ideas-futuras-disposition.md` §1 (residuo para I61 y DIRECT_ONLY) | HECHO (texto materializado inactivo) |
| FX-04a con variante de mismo proveedor | decisiones §52 («no se afirma portabilidad entre proveedores»); B1 INVALID_TEST_ORACLE (§53); B2: resultado mecánico bruto FAIL 2/23 conservado y acreditación INVALID_LAUNCH por KICKOFF_SCHEMA_NOT_DELIVERED (evidencia §78-§79; decisiones §55 U-01a); FX-04a y C-25a UNVERIFIED, sin PASS ni FAIL de protocolo desde B2; B3 no autorizada bajo el Freeze actual, solo con A-2 AGREED (U-01b); V14 D.4, fila «FX-04a PASS», columna «Si falta»: «UNVERIFIED; F6 pendiente» | PENDIENTE: FX-04a OPEN / UNVERIFIED. Sin limitación aceptada (decisiones §55 U-01c): la entrada solo lo recoge con FX-04a PASS, fijado en F6 GATE PASS |
| Estado de FX-03 y FX-04b (y de FX-06 si no hay PASS) | V14 D.4 (UNVERIFIED o UNSUPPORTED «con causa para la decisión del Owner»); hoy: OD-3 = RECHAZAR (§46), C-25b UNSUPPORTED por disposición del Coordinator (§50-§54) sobre la sonda OD-4, con Q16 abierta. La decisión del Owner sobre la limitación es de la OV (V14 §18, columna «Ejecución final sobre FINAL_CANDIDATE_SHA»), posterior a READY-06: no entra en la entrada (va a la evidencia OV y a la marca de cierre de ROADMAP) | PENDIENTE de F6 GATE PASS |
| C-24, C-25a, C-39, C-42 (7, 8), C-32/C-37 (F6) | `I-62-F6/README.md`, matriz de obligaciones | PENDIENTE de F6 GATE PASS |

## 4. Diferencias frente a la revisión del 2026-10-06 (`f7-ready-closure.md` §1.1)

1. **Protecting tests:** C-22 y C-23 pasan a PASS (decisiones §51); C-27 se conserva PASS (§47). La lista de pendientes ya no incluye C-22/C-23 y separa
   las limitaciones (decididas por el Owner) de lo pendiente de ejecución (r3 retira de la entrada las decisiones del Owner: §4.2).
2. **Known limitations:** se añaden, como hechos medidos de F6: lo observado en 2 de 2 actualizaciones automáticas de la app de Codex; la ausencia de celda
   Codex Frontera elegible con el catálogo identificado por blob y fecha; FX-04a con una variante de mismo proveedor, con su resultado pendiente (r4: solo
   con FX-04a PASS, §4.3); el Worker
   Codex sin commit. Se precisa que `read-only` no crea entradas de confianza y `workspace-write` sí.
3. **Decision source:** se nombran los blobs del Freeze y de A-1 en lugar del FREEZE_SHA `b64a3b64`, que ya no es ancestro de la rama (imagen
   `fb49fceb`); ver Q12. Se elimina la nota «sin cambios antes de los pilotos de F6» (disposición de proceso, §47, no hecho de la fundación).
4. **Persistence:** se añaden la ubicación del mapa de cláusulas y las rutas `bootstrap/` y `rebase/` de 16.25 y 16.28.
5. **Retirado de r1** (no son limitaciones de la fundación): «La topología B depende de OD-3 (rechazada por el Owner)», decisión fechada que se sustituye por
   el estado con causa de FX-03 fijado en F6 GATE PASS (D.4; r3, §4.2); «Un mensaje de control de la sesión de supervisión … lo teclea el Owner», observación
   del arnés (plano c), no ratificada por el Coordinator (Q19), que pasa a `ideas-futuras-disposition.md` F6-N05.

### 4.1 Diferencias frente a la entrada vigente (`docs/FOUNDATIONS.md` líneas 160-169, `Last changed by: I-61`)

| Campo | Cambio | Motivo y evidencia |
|---|---|---|
| Authority | se conserva el texto I61; se añaden el resolver de 16.13 para toda unidad desde `I62_EFFECTIVE_SHA` (WORKFLOW §12) y las partes I62 para I62_DELEGATED | 16.13, 16.14, V14 §14.1 y §15 (fila 2 de §3) |
| Persistence | se conserva el texto I61 («ignorado por Git», «de JSON y MD», `/v1` en `schemas/`); se añade lo I62 | 16.25, 16.28; árbol de `agent-execution/` en `e9473425` |
| Mutation contract | se conservan «con ADR o A-n» y «sin Freeze»; se añaden el adapter nuevo y la invalidación del MaterializationClose (I62) | 16.19, 16.30 |
| Extension point | se conserva el texto I61 (PROMPT_TEMPLATES §G, Freeze de I-61 §15); se añaden adapter nuevo y routing §8 (I62) | 16.19; routing §8 |
| Decision source | se conservan ADR-0046 y el Freeze de I-61; se añaden ADR-0048 («aceptado»; OD-1 = A es precondición de READY-03), el Freeze de I-62, A-1 y, si se acuerda, A-2 (`[PENDIENTE]`) | V14 §18 (OD-1); decisiones §31, §43 y §55 (U-01b) |
| Protecting tests | se conservan todas las pruebas y controles I61; se añaden los de I-62 | §3, filas 1 y C-22..C-27 |
| Known limitations | se conservan cinco limitaciones I61 sin cambio. **Cambian dos:** «Worker Codex con escritura UNKNOWN» pasa al hecho medido «no puede hacer commit (sandbox)»; «sin clase de routing para la sesión principal» se acota a las unidades I61 y DIRECT_ONLY. Se añaden las limitaciones I62 | OD-4 (`R20261006T072306Z-od41`); routing §8 y 16.15 |
| Last changed by | `I-61` → `I-62` | — |

Ningún hecho I61 se retira sin fila en esta tabla.

### 4.2 Cambios de r3 (revisión de completitud F7/READY, 2026-10-07)

1. **Momento de los `[PENDIENTE]`:** todos se resuelven antes de READY-04 (LIFECYCLE §4.1) con hechos de F6 GATE PASS y OD-1 y con hechos observados
   en la punta de la rama al redactar la entrada; DC-08 los confirma sobre el SHA rebasado de READY-04, y si alguno cambió, la corrección crea un SHA
   nuevo y reinicia READY-02 (LIFECYCLE §6 y §8; §2).
   Se retiran de la entrada las decisiones del Owner sobre limitaciones (OV-I62-04, OV-I62-05 b, OV-I62-06): según V14 §18 se toman en la ejecución
   final sobre `FINAL_CANDIDATE_SHA`, después de READY-06, y la entrada publicada debe ser la conformada (WORKFLOW §11.4). En su lugar, la entrada
   recoge el estado con causa que fija F6 GATE PASS (D.4).
2. **Decision source:** ADR-0048 «(aceptado)», porque OD-1 = A es precondición de READY-03 y `od1-packet.md` §6 no ve vía prevista para «propuesto».
3. **Known limitations:** los dos hechos de `codex-cli` llevan versión de la CLI, binario y fecha de medición, como ya llevaba el del catálogo; el blob del
   catálogo es el de la punta de la rama al redactar la entrada (antes de READY-04), confirmado por DC-08 en el SHA de READY-04, no el «vigente al
   publicar». La única re-medición prevista es la del hecho `read-only`, con las sondas de solo lectura del OD-2d-PROBE nuevo (§51) y el binario instalado
   al redactar la entrada (antes de READY-04); los hechos de `workspace-write` no se re-miden (§50, §51 y §54; §3).
4. **§0:** la C-20b en seco cita su comando y su resultado, además de la salida en `measured/`.

### 4.3 Cambios de r4 (decisiones §55)

1. **FX-04a:** el `[PENDIENTE]` de Known limitations ya no admite «resultado clasificado» cualquiera. Tras B2 (acreditación INVALID_LAUNCH; FX-04a y
   C-25a UNVERIFIED: decisiones §55 U-01a), FX-04a sigue OPEN / UNVERIFIED; U-01c libera el origen del fixture para trabajo independiente, pero **no** acepta
   ninguna limitación de FX-04a, y el cierre de F6 exige FX-04a PASS (V14 D.4: sin PASS, «UNVERIFIED; F6 pendiente»). Como los `[PENDIENTE]` se resuelven
   con hechos fijados por F6 GATE PASS (§2), la entrada solo puede recoger FX-04a con PASS; un FX-04a sin PASS no llega a READY-04.
2. **Hecho `read-only` de `codex-cli`:** el tope congelado de sondas está agotado (decisiones §55 U-04); la re-medición de r3 «con las sondas de solo
   lectura del OD-2d-PROBE nuevo (§51)» pasa a depender del bloque de medición nuevo que propone A-2 (candidata, material, pendiente del Coordinator y de
   un Architect independiente). Sin A-2 acordada, el hecho se publica solo con su identidad medida.
3. Sin cambio de las demás partes: las limitaciones que D.4 admite para la decisión del Owner siguen siendo FX-03, FX-04b y FX-06, y sus decisiones no
   entran en la entrada (§4.2, punto 1).
4. **Decision source** (corrección de la verificación de r4): junto a A-1 se añade el `[PENDIENTE]` de A-2. Como la entrada solo se publica con FX-04a
   PASS y, bajo el Freeze actual, una sesión B más (B3) exige A-2 AGREED (decisiones §55 U-01b), A-2 está acordada en todo estado en que exista la
   entrada; sin nombrarla, la entrada publicada omitiría una enmienda del Freeze. Lo mismo vale para `amendment_refs` del contrato en el cierre
   (`closure-integration-checklist.md` §2; Q11) y para READY-09 (`ready-dry-run.md`).

## 5. Pendiente antes de publicar (no lo hace esta preparación)

- Q3 (Forma R o N) y, si N, la disposición de los campos `extends`/`introduces` del contrato.
- FX-04a OPEN / UNVERIFIED tras B2 (INVALID_LAUNCH; decisiones §55 U-01a): acuerdo de A-2 (Coordinator + Architect independiente) y, si se acuerda,
  una sola B3 y su clasificación (U-01b); Q21 (B3 frente a escrituras en `fx/u1`); F6 GATE PASS exige FX-04a PASS, sin limitación aceptada (U-01c).
  FX-02/C-24 y FX-06/C-39 (o su estado con causa en F6 GATE PASS); Q16 sobre FX-04b; F6 GATE PASS.
- A-2 en `Decision source`: si se acuerda, su blob junto a A-1 (decisiones §55 U-01b); sin A-2 AGREED no hay B3 ni F6 GATE PASS y la entrada no se
  publica. Su visibilidad en `amendment_refs` sigue a Q11.
- OD-1 = A (precondición de READY-03; `Decision source`) y la decisión de Q1/Q2 (momento de la edición de ADR-0048 e interacción con `MC_I62` y el mapa).
- Blob de `model-catalog.md` en la punta de la rama al redactar la entrada (antes de READY-04); si cambió frente a `166d978d`, se revisa el hecho de
  elegibilidad antes de READY-04.
- Hecho `read-only` de `codex-cli`: resultado de las sondas de solo lectura del bloque de medición nuevo de A-2, si se acuerda (decisiones §55 U-04: el
  tope congelado está agotado), con el binario instalado al redactar la entrada (antes de READY-04), o su publicación solo con la identidad medida
  (Known limitations). Los dos hechos de `workspace-write` se publican solo con su
  identidad medida (`codex-cli 0.160.0`, `37762753`, 2026-10-06), sin re-medición prevista: decisiones §50 «sin repetir la sonda de escritura», §51
  «sin `workspace-write`» y §54 «sin reintentar la escritura de Codex».
- DC-08 sobre la base de READY-04: comprobar que los nombres de clases de prueba y rutas citados existen en el SHA rebasado (LIFECYCLE §4.1) y que los
  hechos del repositorio observados al redactar la entrada (p. ej., el blob del catálogo) siguen siendo los del texto; si alguno cambió, la corrección
  crea un SHA nuevo y reinicia READY-02 (LIFECYCLE §6 y §8).
- Conformidad de READY-06 sobre el texto final (LIFECYCLE §9); ese texto es el que se publica, byte a byte (`closure-integration-checklist.md` §2).
