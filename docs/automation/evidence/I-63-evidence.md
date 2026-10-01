# I-63 — Evidencia de la unidad (ID20, Computed Parameters & Project Summary Foundation)

Unit / Initiative / Workflow: `I-63` / I-63 / V2. Claim-Id `ce04f47f-ca53-4e75-b983-5ac57ab6c5e3`.
Contrato: [I-63-parametros-calculados-resumen-proyecto.md](../../initiatives/I-63-parametros-calculados-resumen-proyecto.md).
Decisiones: [I-63.md](../decisions/I-63.md).

Estado de esta evidencia: G0 (reclamo y bootstrap). Las secciones son registros de su momento y no se reescriben
([WORKFLOW](../../WORKFLOW.md) §11.4). Los hechos posteriores al commit que publica este archivo (su CI exacta, el RELEASE de la
ventana) se informan al Coordinator y se registran en un commit posterior: un archivo no puede citar el SHA del commit que lo contiene.

## 1. Base de reclamo y clasificación de transición

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `819955d61a6da4c811a11fbd11b5dca13f634b7c` (recién obtenida antes del reclamo) | `git fetch` + `git rev-parse origin/main` |
| Commit de reclamo | `cdae642111373afb6df524480c65ade8c5ebf245` (vacío; 2026-10-01T09:47:33-06:00) | `git log` |
| Primer push aceptado (reclamo) | `architecture/parametros-calculados-resumen-proyecto` nueva en `origin`, sin force | salida de `git push -u` |
| Rama / worktree | `architecture/parametros-calculados-resumen-proyecto` / `%USERPROFILE%\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto` (fuera del árbol versionado; distinto del principal del Owner) | `git worktree list` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364`: único commit con el trailer `Workflow-V2-Normative: I-56` (`afd1077c`) y derivación de §11.2 (primer merge first-parent cuyo segundo padre lo alcanza y el primero no); coincide con el tag `integration/I-56`; ancestro de la base | `git log --grep`, `git rev-list --first-parent --merges`, `git merge-base --is-ancestor` |
| Fin de la pausa de activación | `integration/I-56` (tag anotado): `Claim pause … end=2026-09-17T22:30:00Z` | `git cat-file -p integration/I-56` |
| Clasificación | **V2, T4** (posterior probado, base con el SHA efectivo, sin pausa). T8 evaluado negativo: ADR-0043 trata ID20 como iniciativa separada de I-49 | [WORKFLOW](../../WORKFLOW.md) §11.3 |

## 2. Fuentes recibidas

Cadena de custodia: Owner → Coordinator → sesión responsable. Las órdenes llegaron por el chat de la sesión responsable; las de
archivo, como adjuntos locales fuera del repositorio.

| Fuente | Bytes | SHA-256 de transporte | Nota |
|---|---:|---|---|
| `ID20-owner-mandate.original.txt` (mandato del Owner) | 15 760 | `3B3C1D49846685DD83EA6D9B946433F45AEDFA5BFD8778B6185076062B46F6E0` | UTF-8 sin BOM, solo LF, sin LF final. Coincide con la huella que el Coordinator recalculó desde el paquete (CD-ID20-G0-04). Se versiona una sola copia: [`I-63-owner-mandate.original.txt`](../decisions/I-63-owner-mandate.original.txt) |
| `ID20_G0_Prompt.txt` (orden G0) | 10 156 | `0E91B4F5EB25A19C87686E216B0C5CE1647F805D10BD16646CC6FF1372B7ADF0` | coincide con el valor del Coordinator; no se versiona |
| `ID20_G0_R1_Respuesta_a_Claude.txt` (resolución G0-R1) | 11 838 | `B80D5F4AF6555F361EF9888F3BD653F07D0586711A9AD4372728144271205B4E` | no se versiona; resumen por ID en las decisiones |
| `ID20_Solicitud_numeracion_Maestro.txt` (solicitud al Master) | 1 616 | `88EF873B41A5F21B3D02D3D5C545A74915C61244F42F3C1A3F895ED59AAE1EF9` | no se versiona; quedó sin efecto con la corrección de la orden G0 |
| Corrección de la orden G0 | — | — | texto pegado en el chat; sin archivo ni huella; resumen en las decisiones |

**Blob de Git del mandato:** la identidad durable es el blob versionado. Con `core.autocrlf=true`, un checkout en Windows puede
convertir el archivo a CRLF en disco; el SHA-256 del blob (LF) es el que se compara con el de transporte. La comprobación sobre el blob
del commit de bootstrap se informa con su CI.

## 3. Preflight

**Primer preflight (2026-10-01, 14:47Z–14:54Z): STOP antes del reclamo.**
- `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`; ramas remotas: `main` y `feature/rackmirror-espejo-semantico` (I-52).
- Bloqueos devueltos: B-01 (número no acreditable sin asignación del Master), B-02 (sin acuse para la ventana de ROADMAP) y B-03
  (huella de transporte del mandato sin referencia). B-03 lo cerró el Coordinator (CD-ID20-G0-04).
- No se creó rama, worktree, commit ni push. La única escritura fue `git fetch --prune origin`, que solo actualizó referencias remotas.

**Segundo preflight (tras la corrección de la orden G0, 15:39:53Z):**
- `origin/main` sin cambio (`819955d6…`); ramas remotas sin cambio; ningún tag `I-62..I-69`.
- `git log --all --grep='I-6[2-9]'`: vacío. Fila más alta de ROADMAP en `origin/main`: I-61. Ningún archivo `docs/**/I-62..I-69*`.
- Worktree principal del Owner: no tocado.

**Número.** I-62 está reservado (portabilidad del Principal). Sesiones abiertas en este repositorio: «I - 52», «I - 62» (detenida
antes del reclamo) y «Orden y matriz de intake» (ID30, detenida antes del reclamo). ID30 avisó de una colisión posible: su orden
dice «I-63 es SOLO candidato», pero el paquete del Owner para ID30 está en la carpeta `D:\IDs\I-64`. La sesión responsable escaló la
elección al usuario del chat de ID20, que eligió **I-63** para ID20. Después, ID30 confirmó que no tiene I-63 reservado ni reclamado
(§4).

## 4. Ventana de `docs/ROADMAP.md`

Canal: mensajería entre sesiones locales de Claude. El canal no registra hora; las horas son del reloj de la sesión receptora.

| Escritor | Solicitud (msg_id) | Respuesta | Alcance |
|---|---|---|---|
| I-62 (sesión «I - 62») | `5339d7bd-…` (fila de I-64) y `9fc9745c-…` (cambio a I-63) | **ACUSE** y confirmación para I-63 (≈15:40Z–15:44Z) | solo `docs/ROADMAP.md`; hasta el RELEASE de ID20 |
| ID30 (sesión «Orden y matriz de intake») | `221816a0-…` (fila de I-64) y `e1b40e57-…` (cambio a I-63) | **ACUSE** y confirmación para I-63 (≈15:40Z–15:44Z) | solo `docs/ROADMAP.md`; hasta el RELEASE de ID20 |
| I-52 (sesión «I - 52») | consulta `3c17d3d9-…` (informativa); petición `6f1559ca-…` y actualización `4dcf8a69-…` | **ACUSE** (≈15:45Z) y confirmación para I-63 (≈15:47Z) | solo `docs/ROADMAP.md`, una fila nueva; sin HANDOFF, índice de ADR ni host de I-52 |

Textos recibidos (literales):

```text
[I-62, acuse inicial]
ACUSE — I-62 no escribe, rebasa ni publica cambios en docs/ROADMAP.md desde este mensaje hasta tu «RELEASE» por este canal.
Motivo: I-62 sigue detenida antes del reclamo (falta su mandato original) y no tiene escritura de ROADMAP en curso.

[I-62, confirmación para I-63]
ACUSE — mi acuse af5b2e5c sigue vigente y cubre la fila de I-63 de ID20. Motivo: nada de I-62 usa I-63, I-64 ni tu rama,
e I-62 sigue detenida antes del reclamo.

[ID30, acuse inicial]
ACUSE solo para la ventana de ROADMAP. El número I-64 no lo puedo dar por libre: hay un posible conflicto que no me toca resolver.
Ventana: desde este acuse hasta tu «RELEASE», esta sesión (ID30) no escribe, rebasa ni publica cambios en docs/ROADMAP.md.

[ID30, confirmación para I-63]
ACUSE: cubre la fila de I-63 sin conflicto. ID30 no tiene I-63 reservado ni reclamado; en su orden solo figuraba como candidato,
y no hay claim, rama ni push de ID30.

[I-52, acuse]
ACUSE (I-52) para I-64: desde este mensaje hasta tu RELEASE, I-52 no escribe, rebasa ni publica cambios que toquen docs/ROADMAP.md.
Motivo: no tengo escritura de ROADMAP en curso ni planeada; mi rebase sobre main está diferido hasta después de las corridas de host.
Mi actividad de host y mis demás superficies siguen sin cambio.

[I-52, confirmación para I-63]
ACUSE (I-52) para I-63 (antes I-64; ID20): sigue vigente mi acuse con el número corregido. Desde mi acuse hasta tu RELEASE, I-52 no
escribe, rebasa ni publica cambios que toquen docs/ROADMAP.md. Motivo: sin escritura de ROADMAP en curso ni planeada; mi rebase está
diferido hasta después de las corridas de host.
```

Antecedente: antes de la corrección de la orden G0, la sesión de ID20 dio a I-62 un acuse sobre ROADMAP (msg `fc1b0c05-…`); I-62
lo cerró con RELEASE sin escribir ni reclamar.

## 5. Bootstrap

Archivos nuevos: el contrato `docs/initiatives/I-63-parametros-calculados-resumen-proyecto.md`; `docs/automation/decisions/I-63.md`
y `docs/automation/decisions/I-63-owner-mandate.original.txt`; `docs/automation/state/I-63.yml`; este archivo. Archivo modificado:
`docs/ROADMAP.md` (una fila en la tabla de la Fase 6, tras I-60). **Nada más**: sin cambios en normas globales, HANDOFF, índice de
ADR, FOUNDATIONS, `src/`, `tests/`, `assets/`, `eng/`, CI ni configuración. `automation.enabled: false`.

## 6. Validación de G0

- Comprobación documental: `git diff --name-only` del bootstrap contra la base, todo bajo `docs/`; se informa con el commit.
- CI exigible: la corrida `push` del SHA exacto del bootstrap ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental); es un hecho
  posterior al commit y se informa al Coordinator.
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a G0** (sin cambios de producto); no se ejecutaron.

## 7. Capacidades observadas para la ejecución delegada (sin instalar ni configurar nada)

- `gh` 2.96.0 autenticado (permisos `repo`, `workflow`); SDK .NET 8.0.423 de usuario.
- Codex CLI 0.159.2 (`%LOCALAPPDATA%\OpenAI\Codex\bin\…\codex.exe`), «Logged in using ChatGPT», fuera del `PATH`.
- `~/.codex/config.toml`: SHA-256 `42E15A039EFD7E1A9197423AFB14B4358C7D60C89FEDDD734583891DB6E732A5`, modificado
  2026-10-01T05:30:20Z; distinto de los hashes registrados por I-61. Sin delegación abierta no es P-01 (CD-ID20-G0-07); motivo UNKNOWN.
- Sesión responsable: `claude-opus-5-5` / `xhigh` (observado con `get_session`).
- Ningún Controller, Worker, Architect ni subagente se invocó en G0.

## 8. Métricas, conformidad, Owner Validation y tag

Métricas: UNKNOWN (sin gates funcionales). Arquetipo inicial: NEW ARCHITECTURE provisional (M-07 UNKNOWN). Conformidad: no aplica
todavía. Owner Validation: por determinar; no aplica a G0. Tag de integración: no existe.

## 9. Registros posteriores a la publicación del bootstrap

Continuación documental ordenada por el Coordinator («Continuación G0» en las decisiones). Registra en un solo commit lo ocurrido
después del bootstrap. La CI del commit que contiene este archivo se informa al Coordinator.

### 9.1 Publicación y CI

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Reclamo | `cdae642111373afb6df524480c65ade8c5ebf245` | 36887041637: `ref` = `refs/heads/architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |
| Bootstrap | `cba24838e58249ae1782c8fd922ca31f67a0fb32` | 36887229797: misma `ref`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

- Push del bootstrap: fast-forward `cdae6421..cba24838`, sin force, hacia las 15:49:15Z. Después, `HEAD` = `ls-remote` = `cba24838` y
  el árbol quedó limpio.
- `git diff --name-only 819955d6 cba24838`: las seis rutas del §5, todas bajo `docs/`.
- Mandato: blob `876bdd50f10f76bb73eed7c8e686e5cb9b8972c6`, 15 760 bytes. El SHA-256 del contenido del blob es
  `3B3C1D49846685DD83EA6D9B946433F45AEDFA5BFD8778B6185076062B46F6E0`, igual a la huella de transporte. B-03 queda cerrado en local.

### 9.2 RELEASE de la ventana de I-63

Enviado hacia las 15:49Z, tras el push del bootstrap, a I-52 (msg `1d0346df-52bb-4ee9-84a7-b6efd763ca55`), a I-62
(`8b499bad-522f-4895-8ba1-f0fa77820a12`) y a ID30 (`4c6e5105-e914-4207-a43b-9833c63261e0`). El canal no confirma la lectura.

### 9.3 Acuses dados por I-63 después de su bootstrap

| Destinatario | Acuse de I-63 (msg_id del envío) | Cierre recibido y hechos que informó el destinatario |
|---|---|---|
| I-62 | `095567e3-e632-46a1-a4b7-b9be8f3e33b4` | RELEASE sin reclamo ni publicación; le faltaba el acuse de I-52 |
| I-64 (ID30) | `0bf27797-5448-49a0-a489-52d13af27828` | RELEASE; reclamo `6ea9b7e8` y bootstrap `d8a02163` en `architecture/workspace-persistente-rackcad`, fila tras I-60 |
| I-62 | `4d31d969-893d-48ac-a6c9-7595ca64300e` (acuse nuevo) | RELEASE; reclamo `a851841a` (Claim-Id `5b661a17-8c18-4183-8554-3866059cba2b`) y bootstrap `85ae4324` en `architecture/portabilidad-coordinador-principal`, fila en Engineering Productivity tras I-61 |

**Identificadores discrepantes, conservados sin afirmar equivalencias:**
- En sus RELEASE, I-62 cita los acuses de I-63 como `31a18cf6` y `0ca4a000`. Los msg_id que registró el envío de I-63 son
  `095567e3-…` y `4d31d969-…`. No se afirma que nombren los mismos mensajes.
- I-62 cita su propio acuse a ID20 como `af5b2e5c` (§4). El canal no entrega a la sesión receptora el msg_id de los mensajes
  recibidos, así que no hay identificador propio con que compararlo.

### 9.4 Preflight de la continuación (2026-10-01T16:20:03Z)

- `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, sin cambio y ancestro de `HEAD`: **sin rebase**.
- `git ls-remote --heads origin`:

  ```text
  cba24838e58249ae1782c8fd922ca31f67a0fb32  refs/heads/architecture/parametros-calculados-resumen-proyecto
  85ae4324988ac7564719664f8f05b3fa9c6cafbe  refs/heads/architecture/portabilidad-coordinador-principal
  d8a0216306f40e4d502b77310c1632cdbb84d70d  refs/heads/architecture/workspace-persistente-rackcad
  c1982b2a4b483f98beccc161e9635f92e1504895  refs/heads/feature/rackmirror-espejo-semantico
  819955d61a6da4c811a11fbd11b5dca13f634b7c  refs/heads/main
  ```

- Reclamos observados en Git: I-64, commit `6ea9b7e8` con `Claim-Id: 614371d5-441f-4f14-bac9-f97017105610`; I-62, commit `a851841a` con
  `Claim-Id: 5b661a17-8c18-4183-8554-3866059cba2b`.
- Cruce textual medido con `git merge-tree --write-tree`: I-63 × I-64 da **conflicto de contenido en `docs/ROADMAP.md`** (las dos filas
  se insertan tras I-60); I-63 × I-62 no da conflicto; I-63 × I-52 no se midió. **Cruce funcional: no inspeccionado**; es DC-07 de F0.
- Efecto local de esa medición: `git merge-tree --write-tree` escribió objetos *tree* sin ref en el almacén del repositorio local
  (`2b6cc6bd588795a1fc562f01a5b965918d7a0d5c`, `0e20d47dd9b5403191154128a3ba83c7b0d568ec`). No son commits ni refs; no se ejecutó
  `gc`/`prune` y no se repitió.
- Esta continuación no toca `docs/ROADMAP.md`, así que no pidió ventana.
- Ningún Controller, Worker, Architect ni subagente se invocó.

## 10. G0 PASS y F0-DISCOVERY

Orden recibida: `I63_G0_PASS_F0_Discovery.txt` del Coordinator (10 068 bytes, SHA-256
`26fa3b877257c5853509398f82a912aecaa824d11379b35e22d5327159d1e946`; no se versiona). Resumen en las
[decisiones](../decisions/I-63.md) §2 (CD-I63-G0-10, CD-I63-F0-01, CD-I63-F0-02, F0-DISCOVERY).

### 10.1 CI de la continuación de G0

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Continuación G0 | `b557ea3be4adeb496c353d9915b38fffaf2d9316` | 36891560504: `ref` = `refs/heads/architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

Sobre ese SHA, el Coordinator declaró G0 = PASS (CD-I63-G0-10).

### 10.2 Preflight de F0-DISCOVERY (2026-10-01T16:40:05Z)

- `HEAD` = `ls-remote` = `b557ea3b`; árbol limpio; rama `architecture/parametros-calculados-resumen-proyecto`.
- `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, ancestro de `HEAD`: **sin rebase**. Fuera de `docs/`, `b557ea3b` no
  difiere de `819955d6`.
- Ramas remotas: I-52 `c1982b2a`, I-62 `85ae4324`, I-63 `b557ea3b`, I-64 `d8a02163`, `main` `819955d6`. Ninguna ventana de
  ROADMAP abierta; esta entrega no toca ROADMAP.

### 10.3 Método y ejecución

- Discovery hecho **directamente** por la sesión principal (CD-I63-F0-01): lectura de código, documentos y Git con `grep`, `git show`
  y `git rev-parse`. **Ningún** Controller, Worker, Architect, subagente ni otro proceso de IA; ninguna delegación §16; ningún
  `EXECUTION_*`.
- Blobs de I-49 medidos en `origin/main`, iguales a los que declara su Freeze correctivo R1 (Discovery §9.1).
- **Pruebas existentes ejecutadas** (sin editar fuentes; SDK de usuario 8.0.423; árbol limpio antes y después):
  - orden: `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter` con ocho cláusulas `FullyQualifiedName~RackCad.Tests.<Clase>`;
  - inicio 2026-10-01T16:48:09Z, fin 16:49:16Z;
  - resultado: **179 seleccionadas, 179 superadas, 0 fallos, 0 omitidas**;
  - selección por clase: `ExpressionBinderTests` 65, `ExpressionSymbolModelTests` 44, `SelectiveBomAuthorityTests` 31,
    `ProjectVariableScanProjectionTests` 17, `RackListBuilderTests` 10, `RackEmbedDocumentTests` 7,
    `RackCountInvariantCharacterizationTests` 3 y `ConsolidatedBomBuilderTests` 2;
  - TRX fuera del repositorio, SHA-256 `F5A9D461424B4C2B88130384E88E992ED039A414A87FA4051FA42404FE23DA21`;
  - son **pruebas focales para responder preguntas del Discovery**, no evidencia Full ni de gate funcional.
- Salidas versionadas, todas dentro de las cinco rutas autorizadas: [`I-63-discovery.md`](../../initiatives/I-63-discovery.md)
  (nuevo), el contrato, las decisiones, esta evidencia y el estado. Ningún cambio en `src/`, `tests/`, `assets/`, `eng/`, `tools/`,
  `deploy/`, CI, ROADMAP, HANDOFF, FOUNDATIONS ni ADR.
- `gate-contract.F0-INV.draft.json` sigue sin emitir (CD-I63-F0-02). La CI exacta del commit que contiene este archivo se informa al
  Coordinator.

## 11. F0-DISCOVERY R1

Órdenes recibidas del Coordinator (no se versionan; resumen en las [decisiones](../decisions/I-63.md) §2):

| Archivo | Bytes | SHA-256 |
|---|---:|---|
| `I63_F0_R1_Revision_y_Continuacion.txt` | 14 915 | `c0241b82e75697ac7d46bd7ba4ec6f26d56d533d901bc45a335e22607144a78b` |
| `I63_I64_Consulta_Contrato_Compartido.txt` (consulta para el Master) | 4 227 | `e4ca7e8b6656390c42361d5be2005e13a93a04e0725e619e8f533f01f935e930` |

### 11.1 CI de la entrega F0 (R0)

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Discovery R0 | `dbfe1150007b7ea6819df7277e35666fe67af056` | 36895741267: `ref` = `refs/heads/architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 11.2 Preflight de R1 (2026-10-01T17:16:34Z)

- `HEAD` = `ls-remote` = `dbfe1150`; árbol limpio.
- `origin/main` = `819955d6`, ancestro de `HEAD`: **sin rebase**.
- Ramas remotas: I-52 `c1982b2a`, I-62 `f25dd1d5`, I-63 `dbfe1150`, I-64 `cf034e59`. Coinciden con lo que observó el Coordinator.
- Las ramas de I-62 e I-64 solo cambian `docs/` respecto de `main`. No se repitió `merge-tree` ni se limpiaron objetos.

### 11.3 Método y ejecución

- Discovery R1 directo; ningún participante de IA y ninguna delegación §16.
- **Fuente externa consultada** (EXP-06): referencia oficial de Autodesk de `BlockTableRecord.GetBlockReferenceIds`,
  `https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_BlockTableRecord_GetBlockReferenceIds__MarshalAsUnmanagedType_U1__bool__MarshalAsUnmanagedType_U1__bool.html`,
  consultada el 2026-10-01. Es una página de 2022; la de 2025 no se verificó. Es fuente externa, no ejecución en el host.
- **Pruebas existentes ejecutadas** para preguntas nuevas (sin editar fuentes; SDK de usuario 8.0.423; árbol limpio antes y después):
  - primer intento (17:19:35Z-17:19:51Z, con `--no-build`): binarios compilados en `b557ea3b`. **No cuenta** como evidencia de
    `dbfe1150`;
  - corrida que cuenta (17:20:06Z-17:21:25Z), compilada sobre `dbfe1150` (`RackCad.Application.dll` = `1.0.0+dbfe1150…`);
  - filtro: `FullyQualifiedName~RackCad.Tests.G8PersistenceIntegrationContractTests.UNKNOWN_OR_MALFORMED_EXPRESSION_PAYLOAD_IS_STRUCTURALLY_UNREADABLE`
    y `FullyQualifiedName~RackCad.Tests.SharedPhysicalFactsTests.AUTH07_DEDUPES_IN_STABLE_ORDER_AND_GROUPS_ONLY_OBSERVED_RACK_IDS`;
  - resultado: **9 seleccionadas, 9 superadas**, incluido el caso `"Namespace":"rack"`;
  - TRX fuera del repositorio, SHA-256 `A1F5A8FA2A8DB24F505BC3CCF7BDE3C6678985EBB225F8D9DB8657DC4B7560E9`.
- Salidas: el Discovery reescrito como R1 (la R0 queda en `dbfe1150`, blob `d9426070…`), el contrato, las decisiones, esta evidencia
  y el estado. Nada fuera de las cinco rutas autorizadas.

### 11.4 Coordinación

- **Consulta al Master:** **no enviada** por esta sesión, que no tiene canal con él. `ListAgents` a las 17:16Z mostró solo «I - 64»,
  «I - 62», «I - 52» y «ARC - 02». Se entrega al Owner. Estado: sin respuesta.
- **Canal con I-64** (sesión `local_1c89299c…`):
  - recibida su consulta de frontera, informativa y sin pedir pausa ni ventana;
  - respuesta de I-63 (msg `7d6433c4-ca21-41f6-b9a4-6e1370f665dc`): consulta al Master no enviada; sin Proposal; mi lectura de la
    frontera; corrección R63-DISC-02; hechos de las dos capturas y de la documentación externa;
  - acuse de I-64: registra la respuesta, adopta la corrección y declara que no diseña el mínimo común.

  Ningún mensaje compromete diseño ni decisiones.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.

## 12. F0-DISCOVERY R2

Documentos del Coordinator recibidos tras R1 (no se versionan; resumen en las [decisiones](../decisions/I-63.md) §2):

| Archivo | Bytes | SHA-256 |
|---|---:|---|
| `I63_Revision_R1_Correcciones_Localizadas.txt` (revisión de R1: CD-I63-F0-R2-01..03) | 10 535 | `9e22ecd830231b8ff813ed945112c4a44afe9273c9e4bd8c7cfdfc38c54e3be4` |
| `I63_I64_Consulta_Master_Actualizada_R1.txt` (consulta actualizada para el Master; documento nuevo) | 6 486 | `4c5f284303df41984d18d39a5197b601c6d1a7e38c7a0ab6d90f513915bb6c8d` |

**Procedencia.**
- En el chat de la sesión responsable, el usuario adjuntó la consulta actualizada y, otra vez, `I63_F0_R1_Revision_y_Continuacion.txt`.
  Este último mantiene el SHA-256 `c0241b82…` registrado en §11: no cambió.
- La revisión de R1 estaba en la misma carpeta de órdenes, con la misma fecha de modificación que la consulta, y no se adjuntó. La
  consulta adjunta la cita: dice que la revisión deja dos precisiones documentales autorizadas sin esperar respuesta.
- La sesión actuó solo dentro de lo que autorizan los documentos adjuntos: correcciones en las cinco rutas, sin participantes, sin
  pruebas nuevas y sin envíos.

### 12.1 CI de la entrega R1

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Discovery R1 | `852932735aca5b03c13a17064a67347ec8c78952` | 36899699293: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` (Build UI, Tests Domain + Application, UI Tests, Build Plugin without AutoCAD) |

### 12.2 Preflight de R2 (2026-10-01T19:15:18Z)

- `HEAD` = `origin/architecture/parametros-calculados-resumen-proyecto` = `85293273`; árbol limpio.
- `origin/main` = `819955d6`, ancestro de `HEAD`: **sin rebase**.
- Ramas remotas: I-52 `88138f01`, I-62 `ea055591`, I-63 `85293273`, I-64 `c6c44828`. Coinciden con lo que observó el Coordinator.
- Deltas pertinentes (detalle en el Discovery §9.3):
  - I-64 `cf034e59..c6c44828` y I-62 `f25dd1d5..ea055591`: solo `docs/`;
  - I-52 `c1982b2a..88138f01`: avance *fast-forward*. Su diff de tres puntos contra `main` no toca `src/` ni `tests/` (208 archivos en
    `eng/research`, 4 en `eng/validation`). Los seis archivos de `src/` y `tests/` del diff de dos puntos vienen de I-61, que entró en
    `main` después de la base de fusión `95690c28`.
- No se repitió `merge-tree` ni se limpiaron objetos.

### 12.3 Método y ejecución

- Correcciones directas de la sesión principal; ningún participante de IA y ninguna delegación §16.
- **Ninguna prueba ejecutada:** CD-I63-F0-R2-02 no las pide para estas correcciones de redacción y razonamiento. Las pruebas
  existentes que se citan nuevas en el Discovery (`RackListBuilderTests`, `RackEnvelopeIdProbeTests`, `RackSiblingMembershipTests`)
  se **leyeron**.
- **Código leído** (solo lectura): `RackListBuilder.cs`, `RackInventarioCommands.cs`, `RackInventarioCommands.BomTotal.cs`,
  `ProjectVariableScanProjection.cs`, `ProjectVariableScanEntry.cs`, `RackEmbedDocument.cs`, `RackEnvelopeIdProbe.cs`,
  `RackSiblingScan.cs`, `RackSiblingMembership.cs`, `RackCommandSupport.cs` y `RackBlockFinder.cs`, y el Discovery de I-64 §9.4 y
  §10.2 en `c6c44828`.
- **Fuente externa:** la misma de §11.3 (página 2022), sin nueva consulta. El Coordinator informa que también la corroboró.
- Salidas: el Discovery reescrito como R2 (R1 queda en `85293273`, blob `5224349b…`), el contrato, las decisiones, esta evidencia y el
  estado. Nada fuera de las cinco rutas autorizadas.

### 12.4 Coordinación

- **Consultas al Master:** la original (§11) y la actualizada (tabla de §12) están entregadas al Owner. Esta sesión **no** las envió.
- Conforme a CD-I63-F0-R2-03, en R2 no se volvieron a enumerar sesiones buscando un canal y no se envió ningún mensaje entre sesiones.
- Estado: **sin respuesta**.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.

## 13. Aceptación de R2 y decisión del Owner sobre P-14

### 13.1 Revisión de R2

| Archivo | Bytes | SHA-256 |
|---|---:|---|
| `I63_F0_R2_Revision_Aceptada.txt` (CD-I63-F0-R2-04..06) | 6 565 | `014d47576b62c78f05755f8eedbeb4d1b0bf49395b828baf2baf0ceed405e8fa` |

- Adjuntado por el usuario en el chat de la sesión responsable, junto con otra copia de la consulta actualizada, que es idéntica a la de
  §12 (SHA-256 `4c5f2843…`).
- La sesión comprobó que el blob que cita la revisión, `df74e7fe8f86302493e6cd3312206eaf304b1891`, es el de
  `docs/initiatives/I-63-discovery.md` en `23eeefc5`.
- Más tarde el usuario volvió a adjuntar la misma consulta sin texto: mismo SHA-256 y fecha de modificación 13:46 local. No hubo ninguna
  acción ni envío.

### 13.2 CI de la entrega R2

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Discovery R2 | `23eeefc5f5ae0c50c1addd041865ceb6f69139fc` | 36914667772: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 13.3 Decisión del Owner (texto literal)

Procedencia: el usuario la pegó en el chat de la sesión responsable el 2026-10-01, poco antes del preflight de 19:54:59Z. No llegó como
archivo a `D:\IDs\I-63` ni a través del Coordinator. Texto tal como se recibió:

```text
I-63 — OWNER DECISION: frontera con I-64 / P-14
El Owner decide NO crear ni exigir en esta iniciativa un contrato común de inventario/enumeración entre I-63 e I-64.
Decisión:

1. I-63 / ID20 puede implementar su propio mecanismo necesario para:
   * enumeración lógica usada por métricas;
   * deduplicación por RackId;
   * población y admisión definidas por sus contratos;
   * ProjectSummary y agregaciones.
2. I-64 / ID30 puede implementar de forma independiente su mecanismo necesario para:
   * inventario runtime del Workspace;
   * navegación;
   * selección;
   * representación de sesión/UI.
3. I-63 no consume código no integrado de I-64.
I-64 no se convierte en autoridad de las métricas de I-63.
4. No se exige que ambos mecanismos tengan la misma estructura interna ni que compartan una nueva foundation.
5. La posible duplicación queda aceptada conscientemente para estas dos iniciativas.
6. Después de que ambas implementaciones existan e idealmente estén integradas, una iniciativa arquitectónica separada podrá revisar:
   * duplicación;
   * divergencia semántica;
   * rendimiento;
   * mantenimiento;
   * posibilidad de extraer una foundation común;
   * posibilidad de que un mecanismo sustituya al otro.
7. Esa futura revisión puede decidir:
   * mantener ambos mecanismos;
   * unificarlos;
   * hacer que uno consuma al otro;
   * extraer una tercera autoridad compartida.

Consecuencias para I-63:

* EXP-07 / P-14 deja de bloquear el diseño de I-63.
* La consulta preparada para el Master queda SUPERADA / NO ENVIAR.
* No hace falta respuesta del Master.
* Registrar esta decisión como decisión explícita del Owner.
* No borrar la historia del Discovery que identificó el posible solapamiento.
* Reformular el riesgo como duplicación conscientemente diferida a una futura iniciativa arquitectónica.
* Continuar con P-01..P-05 y, después, con Proposal + revisión del Architect conforme al workflow.
* IMPLEMENTATION AUTHORIZATION continúa en NO hasta acuerdo Coordinator + Architect sobre el mismo Freeze.

No implementar producto todavía solo por esta decisión.
```

### 13.4 Conflicto con `MASTER-I63-I64-01` y elección del Owner

- **Preflight (2026-10-01T19:54:59Z):**
  - `HEAD` = remoto = `23eeefc5`; árbol limpio; `origin/main` = `819955d6`, ancestro de `HEAD`: **sin rebase**.
  - Ramas: I-52 `d8078ef3`, I-62 `a1f5e003`, I-64 `dcc16bed`. Las tres son *fast-forward* desde lo observado en §12.2, y ninguna
    cambia `src/` ni `tests/` frente a `main` (diff de tres puntos).
- **Hecho encontrado:** la evidencia §13 de I-64 en `dcc16bed`, su Proposal V1 y su paquete del Architect registran
  `MASTER-I63-I64-01`, recibida por el Coordinator de I-64:
  - fundación mínima común de *snapshot* lógico neutral por RackId;
  - I-63 como autor inicial del contrato puro común;
  - la F2 de I-64 lo consume cuando esté integrado.

  I-63 no recibió esa decisión; la conoce solo por esa lectura, que es un resumen saneado de I-64.
- **Pregunta al Owner** (herramienta de preguntas de la sesión):
  - registro de su decisión → eligió «La del Owner prevalece»: decisión explícita del Owner que sustituye a `MASTER-I63-I64-01` para
    I-63, citando ambas fuentes;
  - quién avisa a I-64 → eligió «Yo, aviso informativo».

### 13.5 Canal con I-64

- **Aviso de I-63** (msg `e3ec5404-ec4d-46ba-b3c1-0ac7e180a8cc`), informativo y sin peticiones de pausa, ventana ni cambios:
  - resumen de la decisión del Owner;
  - el conflicto con `MASTER-I63-I64-01`;
  - la elección del Owner;
  - las consecuencias para I-63.

  Dice que qué rige para I-64 lo deciden su Coordinator y el Owner.
- **Acuse de I-64** (sesión `local_1c89299c…`):
  - lo eleva a su Coordinator y al Owner sin cambiar nada; su Proposal V1 (`dcc16bed`) sigue como está publicada;
  - no toma como decisión para I-64 un hecho que le llega por este canal;
  - no exige a I-63 ningún contrato, ventana ni pausa;
  - registrará el aviso en su próximo commit útil.
- No se envió nada al Master ni se buscó un canal con él.

### 13.6 Método

- Registro directo de la sesión principal; ningún participante de IA y ninguna delegación §16.
- Sin pruebas ni host. Nada fuera de las cinco rutas autorizadas.
- Esta entrega agrupa la aceptación de R2, la CI de `23eeefc5` y la decisión del Owner, conforme a la cadencia de CD-I63-F0-R2-06.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.

## 14. Decisiones del Owner P-01..P-05

### 14.1 CI de la entrega anterior

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Aceptación de R2 + decisión del Owner sobre P-14 | `f0b063e95ae6376abdd126873e66bee20189d748` | 36918481354: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 14.2 Texto literal

Procedencia: el usuario la pegó en el chat de la sesión responsable el 2026-10-01, antes del preflight de 20:19:02Z. No llegó como
archivo ni a través del Coordinator. Texto tal como se recibió:

```text
I-63 — OWNER DECISIONS P-01..P-05
Registrar las siguientes decisiones explícitas del Owner para ID20:
P-01 — población de Project.TotalRacks:
ELECCIÓN = c) racks cotizables.
La población de TotalRacks se define por la población cotizable conforme al contrato que se congele. No se cuentan simplemente todas las definiciones ni todos los racks con una referencia activa. La Proposal debe formalizar exactamente qué condiciones convierten un rack en cotizable y evitar depender accidentalmente de detalles de presentación del BOM.
P-02a — admisión de Kind ausente o desconocido:
ELECCIÓN = b) excluir.
Un rack con Kind ausente o Kind desconocido queda fuera de la población de TotalRacks.
P-02b — clasificación:
NO APLICA para esos racks como consecuencia de P-02a, porque al estar excluidos no forman parte de RacksBySystem.
P-03 — cobertura incompleta:
ELECCIÓN interpretada = segunda alternativa para cobertura no acreditada: Unavailable.
Cuando no pueda acreditarse la cobertura completa de la población que define el total, no presentar un valor numérico como total exacto. Usar estado Unavailable/diagnóstico según el contrato que congele la Proposal.
Esta decisión no implica que el fallo de una métrica individual convierta automáticamente TotalRacks en Unavailable: si la pertenencia a la población cotizable y su identidad están acreditadas, otras métricas pueden fallar de forma independiente según sus propios estados.
P-04 — criterio de colocación:
ELECCIÓN = a) al menos una referencia directa activa.
No se exige para V1 demostrar una instancia alcanzable recorriendo bloques contenedores. Mantener documentada la limitación de referencias anidadas identificada en Discovery.
P-05 — Rack.Frentes para Selectivo:
ELECCIÓN = a) número de frentes del fondo 0.
No usar el máximo entre fondos ni sumar los fondos.
PENDIENTE DEL OWNER:

* P-05 todavía requiere una decisión independiente sobre si los frentes vacíos del fondo 0 cuentan dentro de Rack.Frentes. No asumirla.

Consecuencias:

* P-01..P-04 quedan resueltas conforme a estas decisiones.
* P-05 queda resuelta en cuanto a qué fondo gobierna, pero mantiene pendiente el tratamiento de frentes vacíos.
* Estas decisiones son insumos para la Proposal; no son por sí mismas Freeze ni autorización de implementación.
* P-14 ya está resuelta por la decisión anterior del Owner: mecanismos independientes I-63/I-64.
* No redactar Proposal ni invocar Architect hasta nueva orden del Coordinator.

IMPLEMENTATION AUTHORIZATION = NO.
```

### 14.3 Preflight (2026-10-01T20:19:02Z)

- `HEAD` = remoto = `f0b063e9`; árbol limpio; `origin/main` = `819955d6`, ancestro de `HEAD`: **sin rebase**.
- Ramas: I-52 `d8078ef3` e I-64 `dcc16bed`, sin cambios desde §13.4; I-62 `486e45e7`, *fast-forward* desde `a1f5e003`.
- El delta de I-62 es solo `docs/` (Proposal V3). Sus menciones a I-63 son solo referencias observadas y una cláusula de no
  retroactividad de su régimen de delegación. Ninguna rama cambia `src/` ni `tests/` frente a `main` (diff de tres puntos).

### 14.4 Método

- Registro directo de la sesión principal; ningún participante de IA y ninguna delegación §16.
- Sin pruebas ni host, sin mensajes entre sesiones. Nada fuera de las cinco rutas autorizadas.
- No hubo nada que preguntar al Owner: ninguna fuente leída contradice estas decisiones. Lo que la Proposal debe formalizar está en el
  Discovery §25 como insumo, sin decidir.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.

## 15. Cierre de P-05 por el Owner

### 15.1 CI de la entrega anterior

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Decisiones del Owner P-01..P-05 | `f6da276874bb53ba481a2c798747a9a5a4d16a55` | 36920968703: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 15.2 Texto literal

Procedencia: el usuario la pegó en el chat de la sesión responsable el 2026-10-01, antes del preflight de 20:30:51Z. No llegó como
archivo ni a través del Coordinator. Texto tal como se recibió:

```text
I-63 — OWNER DECISION: cierre de P-05
P-05 queda resuelta por el Owner:

1. `Rack.Frentes` usa el fondo 0.
2. `Rack.Frentes` cuenta todos los frentes estructurales del fondo 0, incluidos los frentes vacíos.
3. Los frentes vacíos deben exponerse como una métrica calculada separada, para que los consumidores puedan distinguir:
   * frentes totales;
   * frentes vacíos;
   * y, cuando corresponda, derivar frentes ocupados = totales - vacíos.
4. El nombre definitivo de la métrica de frentes vacíos se decide en Proposal/Freeze.
5. La Proposal debe definir, con la autoridad existente del modelo Selectivo, el predicado exacto que determina cuándo un frente se considera “vacío”. No inventar una regla nueva de producto.
6. No sumar frentes de distintos fondos y no usar el fondo con mayor cantidad: la autoridad de esta métrica es el fondo 0.

Con esta decisión, P-01..P-05 quedan resueltas por el Owner.
Estas decisiones son insumos para Proposal/Freeze y no autorizan implementación.
IMPLEMENTATION AUTHORIZATION = NO.
```

### 15.3 Preflight (2026-10-01T20:30:51Z)

- `HEAD` = remoto = `f6da2768`; árbol limpio; `origin/main` = `819955d6`, ancestro de `HEAD`: **sin rebase**.
- Ramas: I-52 `d8078ef3` e I-62 `486e45e7`, sin cambios desde §14.3; I-64 `ab4efe86`, *fast-forward* desde `dcc16bed`. Ninguna cambia
  `src/` ni `tests/` frente a `main` (diff de tres puntos).
- **I-64 `ab4efe86`** (Proposal V2, solo documentos):
  - registra el aviso de I-63 sobre P-14;
  - sigue aplicando `MASTER-I63-I64-01` y declara abierto el conflicto con P-14 en su §15;
  - su D-05 deja que solo su F6 consuma el *snapshot* de I-63, con STOP al Master si no llega.

  Es un hecho de coordinación; I-63 no envió ningún mensaje nuevo (Discovery §9.5).

### 15.4 Método

- Registro directo de la sesión principal; ningún participante de IA y ninguna delegación §16.
- Sin pruebas ni host. Nada fuera de las cinco rutas autorizadas.
- **Código leído** para documentar los predicados de «vacío» que ya existen, sin elegir ninguno (Discovery §26):
  `SelectivePlantaBuilder.cs:371-373`, `SelectiveDesviadorPlan.cs:266`, `SelectiveTopePlan.cs:115` y
  `SelectiveGeometryResolver.cs:268-273`.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.

## 16. Orden PV1: Proposal V1 y paquete del Architect

### 16.1 CI de la entrega anterior

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Cierre de P-05 por el Owner | `a0654ae29f21532c1a97bf12d0fa3abc4ba1211f` | 36922395732: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 16.2 Orden

- **Procedencia:** «I-63 — COORDINATOR ORDER: PROPOSAL V1 + ARCHITECT PACKAGE», pegada por el usuario en el chat de la sesión
  responsable el 2026-10-01. No llegó como archivo, así que no tiene hash.
- Resumen en las [decisiones](../decisions/I-63.md) §2 («Orden PV1»), con la semántica de «frente vacío» que fija el Coordinator.

### 16.3 Preflight (2026-10-01T22:15:43Z y 22:30:33Z)

- `HEAD` = remoto = `a0654ae2`; árbol limpio; `origin/main` = `819955d6`, ancestro de `HEAD`: **sin rebase**.
- Ramas ajenas a las 22:30:33Z: I-52 `352cc3de`, I-62 `acd88eaf`, I-64 `1f1530be`. Las tres son *fast-forward* desde lo observado
  en §15.3 (y I-62 desde `486e45e7`), y ninguna cambia `src/` ni `tests/` frente a `main` (diff de tres puntos).
- **I-64 `1f1530be`** (Proposal V3) registra en su evidencia §15, como resumen de una orden de su Coordinator, `MASTER-I63-I64-02`:
  - retira la fundación o *snapshot* compartido y la autoría inicial de I-63;
  - conserva la frontera;
  - I-64 no depende de la integración de I-63;
  - una fundación común futura exige una decisión nueva del Master.

  I-63 no recibió esa decisión directamente; la conoce por esa lectura. Coincide con P-14, y la Proposal V1 lo refleja en §16 y R-07.

### 16.4 Método y salidas

- Redacción directa de la sesión principal; ningún participante de IA, ninguna delegación §16, sin pruebas ni host.
- Código leído para fijar hechos de diseño (sin editarlo):
  - núcleo de I-49: `SymbolId`, `SymbolTable`, `ExpressionContext`, `ExpressionBinder`, `ExpressionEvaluator`, `ExpressionFormatter`,
    `RegistryEvaluation`, `DependencyGraph`;
  - persistencia: `PersistedBoundExpressionJson`;
  - Selectivo: `SelectiveEffectiveDesignResolver`, `SelectiveGeometryResolver`, `SelectiveDepthLayout`, `SelectivePalletDesign`,
    `SelectiveRackSystem`;
  - población y BOM: `BomAuthoredAuthority`, `RackBomOutputGate`, `PushBackKindHandler`, `KindHandlerRegistry`, `KindDispatch`.
- Fuentes de autoridad leídas: el mandato; ADR-0043 D5, D6, D9, D24 y D25; I-49 V6 P25; ADR-0039 §13 e I-54 D-16;
  INITIATIVE_LIFECYCLE; PROMPT_TEMPLATES §C y el perfil ARCHITECTURE_REVIEW; `routing.md` §1.
- Salidas:

  | Archivo | Blob |
  |---|---|
  | `docs/initiatives/I-63-proposal-v1.md` (Frozen: NO) | `48a68307be3f046c20c2405252dc8af3ad050342` |
  | `docs/initiatives/I-63-architect-package-v1.md` | `98e34b183c4c9829c18fec3cc822b342f6753e52` |

  Además: el contrato, las decisiones, esta evidencia y el estado.
- **Revisión del Architect:** pedida, **no realizada** en este commit. El modo se declarará en la revisión.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.

## 17. Architect R1 y orden PV2: Proposal V2 y paquete de re-revisión

### 17.1 CI de la entrega anterior

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Proposal V1 y paquete v1 | `772ac242430084918d319a7683ce615daec39452` | 36935682563: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 17.2 Revisión del Architect R1 (procedencia)

- La revisión se hizo en una **sesión separada** que abrió el Owner (sesión local `local_d5787742…`, «I-63 Architect Review R1»),
  con el encargo del paquete v1, ampliado por el Owner o el Coordinator.
- **Esta sesión no participó en la revisión.**
- El veredicto no llegó como archivo a `D:\IDs\I-63`. La orden PV2 exige incluir el veredicto completo en el paquete, así que la
  sesión autora lo extrajo, **solo leyendo**, de la transcripción local de la sesión revisora:
  - archivo `fdd2a979-cce9-4391-83aa-e7dfe5effcc4.jsonl`;
  - mensaje del asistente de 2026-10-01T22:50:30Z, modelo `claude-opus-5-5`;
  - 24 721 caracteres; SHA-256 UTF-8 `a8ccab34a911429603cafcc2cb7c5a4008e95871c166b0667e5d46fa447526dc`.
- La orden PV2 confirma su identidad: modo SEPARATE SESSION, commit `772ac242`, blobs `48a68307` y `98e34b18`, CHANGES REQUIRED y
  A63-PV1-01..10.
- El texto se reproduce literal en el anexo R1 del paquete v2.

### 17.3 Orden PV2

- Pegada por el usuario en el chat de la sesión responsable el 2026-10-01. No llegó como archivo, así que no tiene hash.
- Resumen en las [decisiones](../decisions/I-63.md) §2.

### 17.4 Preflight (2026-10-01T23:02:17Z)

- `HEAD` = remoto = `772ac242`; árbol limpio; `origin/main` = `819955d6`, ancestro de `HEAD`: **sin rebase**.
- Ramas: I-52 `7a8e622d` (lectura de pre-vuelo), I-62 `3888c5c8` (Proposal V6) e I-64 `e87ba32c` (Proposal V4, A64-PV3-01..06).
  Ninguna cambia `src/` ni `tests/` frente a `main` (diff de tres puntos).
- I-64 `e87ba32c` registra la V1 de I-63 como alineada con `MASTER-I63-I64-02`. Ningún hecho nuevo contradice P-14.

### 17.5 Método y salidas

- Redacción directa de la sesión principal; ningún participante de IA, ninguna delegación §16, sin pruebas ni host.
- Código leído para resolver los REQUIRED (sin editarlo):
  - `SymbolTable` (índices por nombre y `OperatorNames`);
  - `ExpressionBinder` (`Resolve`, `NamespaceReferenceSyntax`);
  - `ExpressionSyntaxParser` (`palabra.` sin miembro);
  - `ExpressionFormatter` (`FormatReference`, `CanonicalShape`);
  - `ExpressionDiagnosticCode`;
  - los seis `*KindHandler.BuildBom` y `PushBackKindHandler.OutputBlockedReason`;
  - `KindDispatch`, `KindHandlerRegistry` y `BomAuthoredAuthority`;
  - la lista de decisiones D1..D25 de ADR-0043.
- Salidas:

  | Archivo | Blob |
  |---|---|
  | `docs/initiatives/I-63-proposal-v2.md` (Frozen: NO) | `9a84546fdccbe77593c78352521ab8bb926a3d96` |
  | `docs/initiatives/I-63-architect-package-v2.md` | `09f874ddc91848f47ad221d48d22b33b439ccb16` |

  La V1 y el paquete v1 no se tocan. Además: el contrato, las decisiones, esta evidencia y el estado.
- **Re-revisión R2:** pedida al **mismo** Architect, **no realizada** en este commit.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.

## 18. Architect R2 y orden PV3: Proposal V3 y paquete de re-revisión R3

### 18.1 CI de la entrega anterior

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Proposal V2 y paquete v2 | `dddc215f08123b64adbf7fe35de6d3776a241501` | 36938698110: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 18.2 Revisión del Architect R2 (procedencia)

- La re-revisión la hizo el **mismo** Architect, en la misma sesión separada (`local_d5787742…`). **Esta sesión no participó.**
- El veredicto no llegó como archivo. La orden PV3 exige el veredicto R2 completo en el paquete, así que la sesión autora lo extrajo,
  **solo leyendo**, de la transcripción local de la sesión revisora:
  - archivo `fdd2a979-cce9-4391-83aa-e7dfe5effcc4.jsonl`;
  - mensaje de 2026-10-01T23:20:04Z, modelo `claude-opus-5-5`;
  - 18 329 caracteres; SHA-256 UTF-8 `8901e8b0f034405833c7fb1dbc34985b0c8de269609a5597576fc2ccf889fe44`.
- La orden PV3 confirma su identidad: objeto `dddc215f`/`9a84546f`/`09f874dd`, SEPARATE SESSION, mismo Architect, CHANGES REQUIRED,
  cerrados y abiertos.
- El texto se reproduce literal en el anexo R2 del paquete v3.

### 18.3 Orden PV3

- Pegada por el usuario en el chat de la sesión responsable el 2026-10-01. No llegó como archivo, así que no tiene hash.
- Resumen en las [decisiones](../decisions/I-63.md) §2.

### 18.4 Preflight (2026-10-01T23:32:35Z)

- `HEAD` = remoto = `dddc215f`; árbol limpio salvo los dos archivos nuevos de esta entrega; `origin/main` = `819955d6`, ancestro de
  `HEAD`: **sin rebase**.
- Ramas: I-52 `51a66245`, I-62 `d66463a5` (Proposal V8) e I-64 `9e3d289a` (Proposal V5). Ninguna cambia `src/` ni `tests/` frente a
  `main` (diff de tres puntos).

### 18.5 Método y salidas

- Redacción directa de la sesión principal; ningún participante de IA, ninguna delegación §16, sin pruebas ni host.
- Código y configuración leídos para resolver los pendientes (sin editarlos):
  - `PushBackResolver.cs:30-32` (`catalog ?? new RackCatalog()`);
  - `RackCatalogLoader.cs:24-29` (catálogo vacío ante un error);
  - `tests/RackCad.Tests/CantileverPluginSourceGuardTests.cs` (precedente de guarda de fuente);
  - `.github/workflows/ci.yml`: el job de Core solo compila `RackCad.Tests`;
  - las referencias de `RackCad.Tests.csproj`, `RackCad.UI.Tests.csproj` y `RackCad.Plugin.csproj`.
- Salidas:

  | Archivo | Blob |
  |---|---|
  | `docs/initiatives/I-63-proposal-v3.md` (Frozen: NO) | `e4a94effa99f29e06b447a3ce07ec5bcc41f0b6d` |
  | `docs/initiatives/I-63-architect-package-v3.md` | `e195e9aed75155bfc2a6a38384f42cd4412c6f67` |

  V1, V2 y los paquetes v1 y v2 no se tocan. Además: el contrato, las decisiones, esta evidencia y el estado.
- **Re-revisión R3:** pedida al **mismo** Architect, **no realizada** en este commit.
- La CI exacta del commit que contiene este archivo se informa al Coordinator.
