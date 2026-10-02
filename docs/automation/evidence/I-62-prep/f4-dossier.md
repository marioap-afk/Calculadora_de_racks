# I-62 — Dossier de ejecución de F4 (preparación; F4 no está abierto)

> **Preparación, no materialización.** Orden del Coordinator de [decisiones](../../decisions/I-62.md) §33 (clase B). Sin cambios en superficies normativas
> compartidas. Referencias «V14 §…» a la Proposal congelada (blob `34ad80ea`). Los conflictos posibles del Freeze están en
> [freeze-issues.md](freeze-issues.md).

## 1. Resultado congelado de F4 (V14 §17)

**Custodia:**
- `state/v2` completo con su validador semántico (B.8) y el arranque BOOTSTRAP → G0 → QU (§8.6);
- aplicabilidad y adopción (§8.7, §14.0); rebase dentro y fuera de ventana (§8.8) y toma con rebase (§8.9);
- puntos BOOTSTRAP, Q0, Q7, QU, QH y QR; diario encadenado; reconstrucción sin diario; commits sin verificar; transiciones; presupuestos.

**Orquestación autónoma (§20):**
- `orchestration`, `NextAction`, el bucle del Architect y la materialización autorizada;
- intentos con reserva y recuperación, presupuestos, linaje y disposiciones;
- cierre y fidelidad de los insumos, identidad observada y AUTONOMY_GAP.

**Adopción:** punto de entrada de WORKFLOW, §16.13, punteros, mapa de cláusulas y su esquema (Anexo E).

**Planos.** El cierre de F4 es **MC_I62**. Obligaciones: C-15..C-21 (C-20a, C-20b, C-20c), C-28, C-29..C-38, C-40, C-41 y C-42.

## 2. Mapa de archivos

`AE/` = `docs/automation/agent-execution/`. Blobs actuales en `1eddbf48`.

| Path | CurrentBlob | Cláusula | Acción | Obligación | Conflicto posible |
|---|---|---|---|---|---|
| `docs/AUTOMATION_PLAN.md` | `f0e0fd31` | E.1 (16.13, ENTRY de encabezado fijo `### 16.13 Compatibilidad de protocolos de ejecución delegada`); E.1 (punteros: primera frase de §16 y última de 16.3); §8 (`state/v2`, MODIFIED); §3.1 (subsecciones de custodia, recuperación, conteo y adopción, con los marcadores literales de §8.6/§8.9, y la subsección «Orquestación de roles»); §13 (P-12, P-13, P-15..P-25) | MODIFY: 16.13 ENTRY; punteros; `## 8` (formato `/v2`, solo unidades I62); subsecciones I62 nuevas 16.23 custodia, 16.24 recuperación, 16.25 conteo y presupuestos, 16.26 arranque, adopción y marcadores, 16.27 orquestación de roles | C-15..C-18, C-20a, C-28..C-38 | ninguno hoy |
| `docs/WORKFLOW.md` | `884ed305` | E.1/E.3.0: `## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)` (ENTRY, texto congelado en E.3.0); §3 fila «WORKFLOW §§3-4»; §3 fila «WORKFLOW §10» | MODIFY: `## 12` nueva (ENTRY); `## 4` (referencia «al abrir» → autoverificación de 16.15 para I62_DELEGATED); `## 10` (fila «Operación del ejecutor y ejecución delegada» ampliada) | C-20a, C-20c, C-28 | autoridad caliente (WORKFLOW §7): cualquier hermana que toque WORKFLOW obliga a coordinar |
| `AE/compatibility/clause-map.schema.json` | — | E.1, E.4 (`rackcad-clause-map/v1`) | ADD (archivo ENTRY) | C-20a | ninguno |
| `AE/compatibility/I62-clause-map.json` | — | E.1, E.4, E.5, E.7 | ADD: derivado en F4 (base `origin/main` → punta de F4) y **regenerado sobre el merge local** antes del push de integración (C-20b) | C-20a, C-20b | se regenera en la integración |
| `AE/schemas/automation-state.v2.schema.json` | — | B.8.1 (forma), B.8.8 | ADD | C-18, C-38 | ninguno |
| `AE/README.md` | `94efcb3d` | B.8.2-B.8.7 y §20.3-§20.9 (procedimientos); E.5 (§§1, 3, 5, 6 y 11) | MODIFY: secciones I62 nuevas (custodia y diario, reconstrucción, rebase y toma, bucle del Architect, intentos, cierre de insumos, fidelidad, AUTONOMY_GAP); las previsiones MODIFIED de §§1, 3, 5, 6 y 11, solo si hace falta (E.7) | C-15..C-17, C-29..C-42 | ninguno |
| `AE/routing.md` | `9dd94dfe` | E.5 (§7 «El propio Controller» → adapter) | MODIFY solo si F3 no lo resolvió | — | ninguno |
| `docs/automation/state/<unit>.yml` (I62) | — | B.8 | ninguno real en F4: I-62 sigue con `/v1` (I61) | — | — |
| `tests/RackCad.Tests/PrincipalPortabilityProtocolTests.cs` (o una clase I62 aparte por gate) | `bd9bd2c2` | Anexo C | MODIFY: guardas C-18 (validador de archivo y de pares), C-20a (entrada, resolver y mapa) y C-38 (`orchestration`) | C-18, C-20a, C-38 | ninguno |
| `tests/RackCad.Tests/` (lector YAML) | — | B.8 (el estado es YAML); AGENTS «Dependencias» | ver §5 (decisión de dependencia) | C-18 | — |
| `docs/automation/evidence/I-62-F4/` | — | Anexo C | ADD: MC con historia (C-15, C-16, C-17, C-20b, C-20c, C-21, C-28, C-29..C-37, C-40, C-41, C-42) | — | ninguno |

**AGENTS.md, CLAUDE.md, FOUNDATIONS, HANDOFF, ROADMAP e índice ADR:** sin cambios (E.5: AGENTS y CLAUDE «ninguna»). Los esquemas `/v1` se validan sin cambios
(C-19).

## 3. Orden de materialización (cada paso con su RED antes del GREEN)

1. **Lector y validador de `state/v2`** (forma B.8.1, I-S01..I-S17, pares I-P01, I-P02, I-P04..I-P07, I-P09..I-P12) + guarda C-18. RED: esquema y validador
   ausentes.
2. **Textos de custodia, recuperación y conteo** (16.23-16.25) y de arranque y adopción con los **marcadores literales** (16.26). MC C-15, C-16 y C-17 en un
   repositorio Git desechable.
3. **`orchestration`** (B.8.8) en el esquema y en el validador (I-S18, I-P13) + guarda C-38; texto 16.27. MC C-29..C-37, C-40 (parte de F4), C-41 y C-42.
4. **Compatibilidad:** `### 16.13` (Classify, Resolve, Validate, mapa), los punteros, `## 12` de WORKFLOW, el esquema del mapa y el mapa derivado + guarda
   C-20a. MC C-20b y C-20c.
5. **Aplicabilidad:** MC C-28 (DIRECT_ONLY sin la maquinaria).
6. **Cierre de F4 = MC_I62** (§15): el último cambio a superficies normativas o de plantilla, revisado por el Coordinator contra el Freeze. Desde aquí, un
   cambio posterior a esas superficies lo invalida (C-21).

## 4. Auditoría de la máquina de estados (matriz de transiciones)

### 4.1 Puntos durables (§8.1, §9.2, I-P02)

| FROM | EVENT | TO | REQUIRED EVIDENCE | COUNTER EFFECT | AUTHORITY | INVALID TRANSITIONS |
|---|---|---|---|---|---|---|
| — (`/v1` DIRECT_ONLY o reclamo) | bootstrap I62 (§8.6, pasos 1-3) o adopción T20 | BOOTSTRAP | preflight CUSTODY + binding PENDING custodiados en el mismo commit; `protocol.basis` | contadores vacíos; `record_version` 1 | sesión | BOOTSTRAP que cite la decisión de G0 |
| BOOTSTRAP | decisión de G0 con los tres marcadores (T0) | QU | entrada en decisiones + binding con `Acceptance` decidida, en el mismo commit | ninguno | Coordinator / P | sin marcadores → no hay transición |
| BOOTSTRAP | decisión DIRECT_ONLY (T21) | `/v1` | marcador `I62-DELEGATED-EXECUTION: DIRECT_ONLY` | se conservan los nueve campos | Coordinator / P | con `window.seq` > 0 |
| BOOTSTRAP, Q7, QU, QR | intención nueva (paso 1 de 16.4) | Q0 | ambas aceptaciones en ACCEPTED (I-S15); contrato `gate-contract/v2` custodiado | `attempts` +1 y `correction_launches` si es CORRECTION | P | BOOTSTRAP → Q0 (inalcanzable: SM-01); Q0 con PENDING |
| Q0 | T1..T7 (diario) y Q7 | Q7 | `last_window` + manifiesto del diario | contadores de ventana desde el diario | P | Q0 → Q0, Q0 → QU, Q0 → QH (W-2) |
| Q0 | retirada (T18) | Q7 WITHDRAWN | decisión del Coordinator | sin cambio | Coordinator / P | con cesión registrada |
| Q0 | rechazo de aceptación (T3') | Q7 NOT_ACCEPTED | alguna de A1'-A8' en fail | — | Coordinator | — |
| Q0 | huérfano con diario íntegro (T12a) | Q7 con TRANSFER | designación + terminación acreditada | titular nuevo | Coordinator / N | escritura Git dentro de la ventana |
| Q0 | huérfano sin diario (T12b) | QR ABANDONED | B.8.5 (R-1 o R-2) + designación | conteo conservador; `unverified_commits` | Coordinator / N | contadores menores que los probados |
| BOOTSTRAP, Q7, QU, QR | actualización sin ventana | QU ORDINARY | — | ninguno de ventana | P | QU que toca `window`, `last_window`, `chains` o contadores |
| BOOTSTRAP, Q7, QU, QR | rebase fuera de ventana (T19) | QU REBASE_RECONCILIATION | `RebaseMap` con imágenes y `patch-id`; force-with-lease | sin cambio; `TaskId` igual | P | Q0 entre el force-push y el QU (I-H02) |
| BOOTSTRAP, Q7, QU, QR | liberación (T17) | QH | último punto ≠ Q0 | — | P | QH desde Q0 |
| QH | transferencia sin avance de `main` (T16) | QR ORDINARY | terminación acreditada de P + designación con binding aceptado | titular nuevo | Coordinator / B | QH → QU, QH → Q0 |
| QH, T12b, T16 | toma con avance de `main` (T22) | QR REBASE_RECONCILIATION | designación `I62-REBASE-TAKEOVER`; `RebaseMap`; force-with-lease | sin cambio | Coordinator / designado | cualquier acción fuera de las cinco de §8.9 antes del QR |
| cualquiera | sin observación (T11), cambio de máquina (T14), contradicción (T15), recuperación concurrente (T13) | — | — | — | Coordinator | tomar posesión, reset o borrado |

### 4.2 Intentos del bucle de revisión (§20.6, I-P13)

| FROM | EVENT | TO | EVIDENCE | COUNTER EFFECT | AUTHORITY | INVALID |
|---|---|---|---|---|---|---|
| — | `RoleInvocation` construida | INVOCATION_PLANNED (opcional) | invocación custodiada | ninguno | Principal | — |
| INVOCATION_PLANNED o — | reserva | BUDGET_RESERVED | topes comprobados **antes** (P-18) | primera reserva: `architect_launches` +1; `logical_requests` +1 si es el primer intento de la solicitud; `review_rounds` +1 si es la primera solicitud de la versión; `transport_reruns` +1 si k ≥ 2 | Principal (autorización vigente) | reserva con la vigencia terminada |
| BUDGET_RESERVED | replanificación | BUDGET_RESERVED | `InvocationId` nuevo | ninguno (`reserved_at` fijo) | Principal | — |
| BUDGET_RESERVED | write-ahead | LAUNCHING | `RunId` durable; preflight de fidelidad FAITHFUL | ninguno | Principal | lanzar sin LAUNCHING durable |
| LAUNCHING | arranque acreditado | LAUNCHED | operación 7 ligada al `RunId` | ninguno | Principal | — |
| LAUNCHING | no arrancó, vigencia abierta (B.1) | BUDGET_RESERVED | prueba de no arranque custodiada | ninguno | Principal | — |
| LAUNCHING | no arrancó, vigencia terminada | CANCELLED_BEFORE_LAUNCH | prueba custodiada | la reserva sigue contada | Principal | ingerir un resultado después (S-04) |
| LAUNCHING, LAUNCHED | sin acreditación | LAUNCH_UNCERTAIN | — | cuenta como lanzado | Principal | relanzar en silencio |
| LAUNCHING, LAUNCHED | resultado | RESULT_RECEIVED | resultado custodiado, terminación, `RuntimeEvidenceRef`, auditoría de lecturas, fidelidad tras la corrida | ninguno | Principal | ingestión antes de la custodia |
| RESULT_RECEIVED | ingestión idempotente | RESULT_INGESTED | disposiciones, linaje, fase | ninguno | Principal | otro blob para el mismo intento (S-04) |
| INVOCATION_PLANNED, BUDGET_RESERVED | fin de la vigencia | CANCELLED_BEFORE_LAUNCH | — | ninguno liberado | — | — |

Terminales: RESULT_INGESTED, LAUNCH_UNCERTAIN y CANCELLED_BEFORE_LAUNCH.

### 4.3 Fases del bucle (§20.5)

NONE → REVIEW_PENDING (fija `loop.object`) → ARCHITECT_INVOKED → RESULT_INGESTED → {AGREED → ARCHITECT_SATISFIED; CHANGES_REQUIRED → CORRECTING →
PUBLISHED (único cambio de `loop.object`; `correction_rounds` +1) → CI_VERIFIED → REREVIEW_PENDING → ARCHITECT_INVOKED; BLOCKED_OWNER → ESCALATE_OWNER;
INVALID → reejecución dentro del tope o STOP P-19}.

### 4.4 Resultado de la auditoría

| Comprobación | Resultado |
|---|---|
| Estados inalcanzables | arista BOOTSTRAP → Q0 inalcanzable (SM-01, no material); **no hay ruta** desde ARCHITECT_SATISFIED o ESCALATE_OWNER resuelto hasta NONE (FC-01, material) |
| Transiciones con significado duplicado | ninguna: T16 y T22 se distinguen por el avance de `main`, T12a y T12b por el diario, y T18 y T3' por su causa |
| Caídas sin ruta de recuperación | caída entre el force-push y el QU con el host inaccesible: recuperable solo si el `RebaseMap` se publica antes del QU (SM-02); la regla vigente falla cerrado (STOP) |
| Reinicio de presupuestos | `attempts` es de unidad y nunca decrece; los contadores por clase siguen a `TaskId`, y solo el Coordinator abre una `TaskId` nueva (§9.3); los del bucle son de unidad (FC-01) |
| SHAs obsoletos tras un rebase | `orchestration.loop.object.commit` y los commits de las solicitudes abiertas no están en `StateFields` ni en I-H02, e I-P13 impide actualizarlos (FC-02, material) |
| `NextAction` incoherente | I-S18 la exige derivable de forma única y ata `escalation` a su rol; doble `next_action` (prosa y estructurada, SM-03, no material) |

## 5. Plan de validadores

- **Dónde:** código de prueba en `tests/RackCad.Tests` (Nivel A: sin scripts operativos ni servicio). Una clase estática `StateV2Validator`, con la forma, las
  invariantes de archivo y las de pares, la consume la guarda C-18 sobre archivos sintéticos y sobre todo `docs/automation/state/*.yml` con
  `schema: rackcad-automation-state/v2` (hoy ninguno: el conjunto vacío pasa). La clase con historia (I-P03, I-P08, I-H01, I-H02) la ejecuta el arnés MC de
  C-15 en un repositorio Git desechable, nunca en Core (la CI hace un checkout superficial).
- **Lector YAML (decisión necesaria antes de F4).** El estado es YAML (B.8) y el proyecto de pruebas no tiene ningún lector YAML. AGENTS («Dependencias»): «No
  agregar dependencias sin acuerdo explícito del usuario». Opciones:
  - **A (recomendada):** un lector mínimo del subconjunto YAML que escribe el protocolo (mapas y listas en bloque, escalares planos o entre comillas, `null`),
    con su guarda y rechazo de lo que no entienda. Sin dependencia nueva;
  - **B:** YamlDotNet en `RackCad.Tests`, con el acuerdo del Owner.

  Paquete de decisión en [owner-decision-packets.md](owner-decision-packets.md) (DEP-F4-YAML).
- **Validador de `orchestration`** (I-S18, I-P13): misma clase, y la guarda C-38 con un positivo (la secuencia completa de F.8) y los negativos del Anexo C.
- **Validador del mapa** (E.4, MV-1..MV-7): la guarda Core C-20a, sin historia, comprueba la forma, la unicidad, `Surfaces`, ENTRY y los punteros. MV-3..MV-6
  con historia van al MC de C-20b.

## 6. Plan de compatibilidad

- **§16.13** (texto en el plano b, inactivo): `Classify` (E.2), `Resolve` (E.3.1, pasos 1-5, R61 a-d), `Validate` (E.4), ruta fija del mapa y la lista
  cerrada `Surfaces`.
- **Punto de entrada de WORKFLOW:** el texto congelado de E.3.0, literal, bajo `## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)`.
- **Punteros:** primera frase de `## 16` («Antes de aplicar esta sección, toda unidad aplica §16.13») y última de 16.3; 16.3 conserva literalmente el texto de
  I-61.
- **Esquema del mapa:** la forma de E.4 (`Schema`, `Protocol`, `LegacyProtocol`, `Surfaces`, `Files[] {Path, FileKind, BaseBlob, EffBlob}`, `Entries[]
  {Path, Section, Level, Kind}`).
- **Generación** (determinista):
  1. `git diff --name-only <base> <tip> -- <Surfaces>`, menos el mapa → `Files`;
  2. por cada MODIFIED, los encabezados de todos los niveles más el preámbulo, en la base y en la punta, fuera de bloques de código;
  3. la sección va hasta el siguiente encabezado de nivel igual o menor, con CRLF → LF y espacios colapsados;
  4. MODIFIED = H1 ∩ H2 con texto distinto; REMOVED = H1 ∖ H2; ADDED ∪ ENTRY = H2 ∖ H1; ENTRY = exactamente `### 16.13 …` y el `## 12` de WORKFLOW.

  **Previsión con lo materializado hasta F2** (MEASURED en la rama): AUTOMATION_PLAN `## 16.` y `### 16.1` MODIFIED, `### 16.14`-`### 16.19` ADDED;
  PROMPT_TEMPLATES `## 2.` MODIFIED; README `## 12.` y `## 13.` ADDED; routing `## 8.` ADDED; adapters y esquemas nuevos como archivos ADDED. Todo
  coincide con E.5.

  **Prototipo medido** (script local, no entregable, `origin/main` → `1eddbf48`): 18 archivos (4 MODIFIED, 14 ADDED), con salida idéntica en dos
  ejecuciones (SHA-256 `2e900f01…`, determinista). Entradas MODIFIED:
  - los cuatro títulos de nivel 1 (SM-04);
  - AUTOMATION_PLAN `## 16.` y `### 16.1`;
  - PROMPT_TEMPLATES `## 2.`.

  ADDED: 16.14-16.19, README `## 12.`, `## 13.` y 13.1-13.4, y routing `## 8.`. Ningún preámbulo (texto anterior al primer `##`) cambió.
- **C-20b:** MV-3..MV-6 entre `origin/main` y la punta de F4, comparado con E.5. Cada diferencia se clasifica como corrección o como A-n; candidata conocida:
  routing §§4-5 y 7 sin modificar si F3 añade una sección nueva (E.7). Se repite sobre el merge local `I62_EFFECTIVE_SHA^1` → merge antes del push.
- **C-20c** (clon desechable con la historia real):
  - el contrato real de I-61 G3 (`docs/automation/evidence/I-61-pilot/g3-cama-d1a/R20261001T032333Z-4a2d/gate-contract.json`, **blob `9b5ef6df`
    verificado**). Tiene 17 citas, y cada encabezado citado existe **una sola vez** en `main` y en la rama (MEASURED);
  - PRE de prueba: I-61 con `Claim-Id` `0e2923de-e1a7-41bf-b7db-50ec84217850` y, para C-20c-2, I-64 con `614371d5-441f-4f14-bac9-f97017105610`;
  - M2 = EFF + X1 (`## 3. Limites de seguridad`) + X2 (`### 16.4 …`) + X3 (una `##` nueva en WORKFLOW);
  - casos C-20c-1 (a) y (b) y C-20c-2, y los negativos N-a..N-s, con los esperados de E.6. El arnés ejecuta `Evaluate` desde E1, nunca `Resolve`.

## 7. Plan de pruebas de F4

| Obligación | Clase | Arnés | Positivos | Negativos y mutaciones (Anexo C) | RED antes de F4 | GREEN |
|---|---|---|---|---|---|---|
| C-15 | MC con historia | repositorio Git desechable con SHAs reales locales | BOOTSTRAP → G0 → QU; F.1, F.3, F.5 (R-0..R-3); T12a frente a T12b; T16/T17; QU entre ventanas; rebase entre ventanas (F.6) con clon limpio en otra máquina; rebase dentro de una ventana; toma con rebase (F.7); adopción §8.7 | las 12 mutaciones sembradas de C-15 detectadas por su invariante | validador y textos ausentes | `DS`, invariantes y disposiciones = Anexo F, §8.6 y B.8 |
| C-16 | MC | ídem | correcciones lanzadas frente a verificaciones; `ContinuesTaskId` | lanzamiento incierto; contador ausente o contradictorio; conteo de B.8.5 → S-04 donde toca | ídem | §9.3 y B.8.5 |
| C-17 | MC | procesos ficticios | `pwsh` en la ruta → detectado | efímero no vivo tras la relectura | README §3.2 sin ampliar | ídem |
| C-18 | (i) Core RG | `StateV2Validator` | archivos y pares sintéticos, con positivos simultáneos de los tres tipos | omitir I-S03, omitir I-S15, invertir I-P02, `attempts` decreciente, dos transiciones de `g0_acceptance` | validador ausente | GREEN + mutaciones |
| C-20a | (i) Core RG | sin historia | entrada única que nombra §16.13; mapa válido; punteros | entrada borrada o duplicada; entrada de mapa duplicada; puntero borrado; ENTRY extra | textos y mapa ausentes | ídem |
| C-20b | MC con historia | base → punta de F4; después, el merge local | mapa = derivación | cada diferencia clasificada | mapa ausente | igualdad |
| C-20c | MC con historia | §6 | C-20c-1 y C-20c-2 | N-a..N-s | entrada y resolver ausentes | tablas de E.6 |
| C-21 | revisión del Coordinator | — | — | un cambio normativo posterior a MC_I62 → invalidación | — | regla aplicada |
| C-28 | MC | unidad DIRECT_ONLY posterior a EFF | (a) trabajo directo sin STOP | (b) contrato → P-15; (c) sin autoverificación, sin efecto; (d) T20 y después contrato; (e) sin `orchestration` | clasificador ausente | esperados de C-28 |
| C-29..C-37 | MC | artefactos sintéticos y F.8 | secuencia de F.8, caídas en cada frontera, reconstrucción en un clon limpio | presupuestos (C-34), salida inválida (C-35), cambio de proveedor (C-36), AUTONOMY_GAP (C-37), escalada (C-33) | `orchestration` ausente | esperados del Anexo C |
| C-38 | (i) Core RG | validador de `orchestration` | F.8 completa | los 16 negativos del Anexo C | validador ausente | GREEN + mutaciones |
| C-40 (parte F4) | MC | — | (e) caducidad tras AGREED; (g) acreditación histórica; (i) LAUNCHED con revocación | (f), (j)-(n) | — | esperados de C-40 |
| C-41 | MC | `AGENTS.md` real de la `AuthorityRevision` | (a) cierre esperado (tres lecturas, Context Pack, `git log` permitido, `dotnet test` con o sin exención) | (b)-(f) | procedimiento de cierre ausente | esperados de C-41 |
| C-42 | MC (+ FX en F6) | corpus real y capturas sintéticas; manifiesto B.11 | (1)-(3), (6), (7), (a), (f), (i), (k), (m), (r), (t), (v), (x), (z) | el resto de (1)-(8), (a)-(z) | fidelidad y manifiesto ausentes | esperados exactos de C-42 |

**Dependencia de las posibles A-n:** si el Coordinator dispone FC-01 o FC-02 antes de F4, los esperados de C-29, C-31, C-34, C-36, C-38 y C-15 (rebase) se
derivan del texto enmendado.
