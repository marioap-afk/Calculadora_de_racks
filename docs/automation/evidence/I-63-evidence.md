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

## 19. Corrección operativa y relevo directo al Architect (R3)

### 19.1 CI de la entrega anterior

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Proposal V3 y paquete v3 | `fb3c87888ed583d1117ca565ce4751b51f67a81b` | 36941471886: `head_branch` = `architecture/parametros-calculados-resumen-proyecto`, `head_sha` exacto | `success`; los cuatro jobs requeridos de `AGENTS.md` en `success` |

### 19.2 Orden y transporte

- **Orden:** «I-63 — CORRECCIÓN OPERATIVA INMEDIATA», pegada por el usuario en el chat de la sesión responsable el 2026-10-01. No llegó
  como archivo. Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Identidad del Architect, verificada con el gestor de sesiones:** `local_d5787742-7168-40f7-b18f-56abcf3eda7c`, título «I-63 Architect
  Review R1», el mismo de R1 y R2.
- **Transporte disponible:** `SendMessage` entre sesiones locales, con suscripción a su próximo estado inactivo. No hay `AUTONOMY_GAP`.
- **Solicitud R3** (msg `289a2f8a-5ef9-481c-9b6a-9ee7faf39665`, 2026-10-01, hacia las 23:59Z): objeto exacto (`fb3c8788`; Proposal V3, blob
  `e4a94eff`; paquete v3, blob `e195e9ae`; base `819955d6`), pendientes A63-PV1-05, 08 y A63-PV2-01, declaración CLOSED o STILL OPEN,
  posibles A63-PV3-nn, AQ-06/07 y veredicto `AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION`. Sin escrituras ni implementación.
- **Cadena persistente:** bloque `review_chain` del [estado](../state/I-63.yml). Lleva el siguiente rol, el SHA objetivo, la versión,
  los REQUIRED abiertos y cerrados, el resultado esperado, la identidad del Architect, el transporte, el presupuesto y las condiciones
  de STOP y de escalado.
- **Presupuesto de revisión:** 3 rondas (R3..R5). Es un **supuesto** de la sesión, derivado de `automation.max_attempts` del contrato;
  la orden no fijó un número.

## 20. Architect R3: AGREED, READY FOR FREEZE

### 20.1 CI del commit de la cadena de revisión

La corrida `push` de `09637fa64096fe3972674f678ce59bfa71cd3875` (registro de la cadena) es la 36943762295. Su resultado se informa al
Coordinator con la entrega; el commit no cambia la Proposal V3.

### 20.2 Procedencia del veredicto

- **Respuesta directa** del Architect a la solicitud R3 (msg `289a2f8a`). Llegó como mensaje entre sesiones a esta sesión, remitente
  `local_d5787742-7168-40f7-b18f-56abcf3eda7c` («I-63 Architect Review R1»).
- El mismo texto consta como entrada de su `SendMessage` en su transcripción local `fdd2a979-cce9-4391-83aa-e7dfe5effcc4.jsonl`
  (2026-10-02T00:00:50Z). La sesión no escribió otro veredicto en su chat.
- Tamaño: 6 549 caracteres. SHA-256 UTF-8: `190368f59ae67f2ff14f1b2511900829288af27ebbff69269952ab8afe3ac764`.
- **Interpretación de la sesión principal:** AGREED con cero REQUIRED abiertos. Según la corrección operativa, el siguiente rol es el
  **Coordinator**, que decide el Freeze. Ningún hallazgo cambia la materialidad ni el alcance, y los opcionales O-PV3-1..3 no bloquean.
- La Proposal V3 **no** se toca: cualquier cambio crearía otro blob e invalidaría el acuerdo, que vale solo para el blob exacto.

### 20.3 Texto literal

```text
I-63 — ARCHITECT RE-REVIEW R3 (Proposal V3): AGREED, READY FOR FREEZE, sobre fb3c8788 / docs/initiatives/I-63-proposal-v3.md / blob e4a94eff.

Review mode: SEPARATE SESSION; revisor=autor: no; misma sesión que R1/R2: sí (local_d5787742…, «I-63 Architect Review R1»). Mismo modelo de base que la autora: unknown (probable claude-opus-5-5, limitación de independencia registrada desde R1).

1. REVIEW IDENTITY
- Commit fb3c87888ed583d1117ca565ce4751b51f67a81b: único commit sobre dddc215f, solo toca docs/.
- Proposal: docs/initiatives/I-63-proposal-v3.md, blob e4a94effa99f29e06b447a3ce07ec5bcc41f0b6d (comprobado con git rev-parse).
- Paquete: docs/initiatives/I-63-architect-package-v3.md, blob e195e9aed75155bfc2a6a38384f42cd4412c6f67 (comprobado).
- Base: origin/main = 819955d61a6da4c811a11fbd11b5dca13f634b7c, tras git fetch.
- Lectura con git show y diff V2→V3. Sin escrituras en el repositorio.

2. DISPOSICIÓN DE LOS REQUIRED ABIERTOS
- A63-PV1-05 → CLOSED.
  (a) D-27 obliga a delegar y prohíbe conservar la composición propia del handler. INV-33 es una guarda de fuente única, DelegatesToOutputVerdict, sobre el texto del método. Exige la llamada a RackOutputVerdict y a la correspondencia del handler, y prohíbe PushBackResolver y RackBomOutputGate.For. Su control es el texto literal de 819955d6 (PushBackKindHandler.cs:56-71), rechazado por la misma función. Ese RED es observable: hoy el método contiene exactamente ese texto.
  (b) CatalogInput = Loaded | LoadFailed. La vía del handler normaliza null a Loaded(new RackCatalog()), igual que PushBackResolver.cs:30-32, y nunca produce LoadFailed. INV-13 cubre el Push Back que se bloquea con catálogo vacío. CatalogUnavailable solo existe con LoadFailed. D-25 obliga al futuro productor a no normalizar un fallo de carga (RackCatalogLoader.cs:24-29).
- A63-PV1-08 → CLOSED.
  (a) D-28 es una tabla total y única para RackMetricRequest y RackSummary.Metrics, en este orden: kind (E3) → soporte de diseño (D-06, sin leer) → E5 → E4 → efectivo/resuelto → Available. Coincide con el orden de población E3→E5→E4. El ejemplo del Push Back ilegible da NotSupported. INV-34 discrimina la variante «E5 antes del paso 2» y exige el mismo resultado por las dos vías. INV-14 cuenta resoluciones y lecturas.
  (b) INV-32 pasa a G2, con un solo contador en el orquestador real y control positivo por ProjectSummary Full (> 0). §21 retira la aserción de G1.
- A63-PV2-01 → CLOSED.
  - INV-20 usa una entrada rack sintética llamada Frentes-Vacios. La variante global (SymbolTable.cs:148) da OperatorInName; la correcta da UnknownSymbol. La guarda discrimina.
  - INV-29(b) usa una sola función sobre el cierre declarado de los .csproj, con controles UI → WPF y Plugin → AutoCAD.
  - INV-09 usa una sola función parametrizada por texto, con un fixture inválido.
  - §20 fija que el control usa la misma función.
  - No queda ninguna guarda tautológica.
- Cerrados en R2 (A63-PV1-01..04, 06, 07, 09 y 10): siguen cerrados. Las secciones que los satisfacen no cambiaron en el delta V2→V3, y no encontré ningún defecto material nuevo.

3. AGREED POINTS
- Todo lo acordado en R2 sigue vigente.
- D-28 resuelve la ambigüedad sin estados nuevos: Push Back ilegible → NotSupported; Excluded(OutputDenied) → NotSupported en sus frentes.
- La asimetría de hermanas sin colocar (O-PV2-4) es explícita y está motivada.
- INV-35 vigila la deriva del lector D-26 respecto de los handlers, con control.
- Anexo A: P25.4 modificada y P25.1 descriptiva (O-PV2-1).
- OV NOT APPLICABLE se confirma como candidata: la delegación no cambia nada observable (INV-13) y su unicidad está guardada (INV-33). La decisión final es del Freeze.
- I-64: sin dependencia obligatoria, sin snapshot común y sin contrato compartido.

4. DISAGREEMENTS
Ninguno material.

5. MATERIAL RISKS
Riesgos residuales, todos declarados y con dueño:
- R-13: el futuro productor podría normalizar un fallo de carga del catálogo. Queda obligación en D-25.
- R-14: coste de calcular métricas para racks no incluidos. Se mide en G4.
- R-11: deriva del lector D-26. Lo vigila INV-35.
- Limitación de independencia: probablemente el mismo modelo de base.

6. NEW REQUIRED CHANGES
Ninguno.

7. OPTIONAL IMPROVEMENTS (no bloquean el Freeze)
- O-PV3-1 (INV-29 b): RackCad.Plugin.csproj declara AutoCAD por dos vías condicionadas. Con UseAutoCADNuGetReferences=true usa PackageReference AutoCAD.NET; con el valor por defecto false usa Reference AcCoreMgd, AcDbMgd y AcMgd. La función debe clasificar AutoCAD con cualquiera de las dos e ignorar Condition (unión de los ítems); si no, el control fallará al implementar. El Plugin también declara UseWPF=true.
- O-PV3-2 (INV-20): nombrar el control de proyecto que dice «al de control». Debe ser una projectVariable A-B: el texto A-B sigue dando OperatorInName. Así queda dentro de la prueba que el detector no se desactivó globalmente (hoy lo cubre de forma indirecta INV-28, ExpressionBinderTests.cs:301-313).
- O-PV3-3 (INV-14 / INV-34): fijar que el contador de lecturas de diseño está en el costado del lector D-26. El 0 de Push Back y Cabecera debe medirse allí y no en el store.

8. AQ-06 / AQ-07
- AQ-06: ACEPTO el cierre declarado de los .csproj, con la misma función para el objetivo y los dos controles, porque el job de Core no compila UI ni Plugin. Condiciones:
  - aplicar O-PV3-1;
  - declarar que no se inspeccionan las dependencias transitivas de PackageReference. Hoy Application solo tiene ProjectReference a Domain, así que el riesgo es nulo.
- AQ-07: ACEPTO calcular RackSummary.Metrics para todos los RackIds atribuibles, NotPlaced incluidos, con D-28 y el coste medido en G4. Limitar el paso 5 a los Included obligaría a un estado nuevo (NotRequested) que O-04/AQ-05 ya rechazó. Membership deja explícito que esas métricas no entran en los totales.

9. MATERIALITY / ARCHETYPE
- M-01: creador y modificador, efectivo y único (INV-33).
- M-02: no activado (dos tablas, INV-24).
- M-03: no activado; la condición quedó cumplida con INV-13 y el catálogo nulo.
- M-04 a M-08: activados, como en V3 §3.
- Arquetipo: NEW ARCHITECTURE, confirmado.

10. FREEZE READINESS
READY FOR FREEZE. Este veredicto no crea el Freeze; el commit de Freeze lo decide el Coordinator.

11. CONSENSUS STATUS
AGREED: cero REQUIRED abiertos. Válido exclusivamente para:
- commit fb3c87888ed583d1117ca565ce4751b51f67a81b;
- ruta docs/initiatives/I-63-proposal-v3.md;
- blob e4a94effa99f29e06b447a3ce07ec5bcc41f0b6d.

IMPLEMENTATION AUTHORIZATION = NO hasta que Coordinator y Architect hayan acordado el mismo Freeze.
```

## 21. Freeze, A-1 y preparación de G1

### 21.1 CI de los commits de la cadena

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Cadena de revisión | `09637fa64096fe3972674f678ce59bfa71cd3875` | 36943762295 | `success`; cuatro jobs requeridos en `success` |
| Architect R3 AGREED | `8fea037633916c7a8106525ec1ac5086e9c53bba` | 36943950908 | `success`; cuatro jobs requeridos en `success` |

### 21.2 Freeze

- **Orden:** «I-63 — COORDINATOR FREEZE ORDER», pegada por el usuario en el chat de la sesión responsable el 2026-10-02. No llegó como
  archivo. Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Commit de Freeze:** `f61d0aca859a11b15cbe1797069a83cba873ba95`, hijo de `8fea0376`, sin force, con un **solo** archivo.
  - Diff respecto del acuerdo (`fb3c8788:docs/initiatives/I-63-proposal-v3.md`, blob `e4a94eff`): exactamente una línea, `Frozen: NO` →
    `Frozen: YES`.
  - Proposal congelada: blob `4d5dedce15363fa6378e460c5b63005fe41b858b`.
- **CI exact-SHA del Freeze:** corrida 36944741421: `event=push`, rama y `head_sha` exactos, `success` con los cuatro jobs requeridos.
- **Owner Validation:** NOT APPLICABLE para I-63 V1, por la orden de Freeze.

### 21.3 A-1 (Coordinator-only)

- Archivo `docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md`, blob `766d9ba9cf379daa077bed20fc6255b5ca656d60`. Contiene solo O-PV3-1, O-PV3-2 y O-PV3-3, sin activar ninguna M.
- La CI exact-SHA del commit de la A-1 se informa al Coordinator; ese commit no puede registrarse a sí mismo.

### 21.4 Preparación del gate G1 (I-61)

- Borrador de `gate-contract.json` (esquema `rackcad-gate-contract/v1`) en `artifacts/orchestration/I-63/G1/1/R20261002T001347Z-ed7a/gate-contract.json`. `artifacts/` está en `.gitignore`
  (README de ejecución delegada §2).
- Lo prepara la sesión. Lo **emite y autoriza el Coordinator** (README §1); hasta entonces no hay delegación, Controller, Worker ni
  implementación.
- El `AuthorityRevision` del borrador es el commit de la A-1. Su SHA-256 y su validación contra el esquema se informan al Coordinator.

## 22. Gate G1 bajo I-61: emisión del contrato y relevo

### 22.1 CI de la A-1

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| A-1 | `658b35ad498520ba3c832a96eea4389df16dfa60` | 36945143490 | `success`; cuatro jobs requeridos en `success` |

### 22.2 Orden y contrato emitido

- **Orden:** «I-63 — COORDINATOR GATE ORDER — G1», pegada por el usuario en el chat el 2026-10-02. No llegó como archivo. Resumen en
  las [decisiones](../decisions/I-63.md) §2. **F0 = PASS. G1 = AUTHORIZED.**
- **Contrato emitido:** `artifacts/orchestration/I-63/G1-RACK-METRICS/0/R20261002T002817Z-6294/gate-contract.json`, en `artifacts/`, que ignora `.gitignore`.
  - Esquema `rackcad-gate-contract/v1`, válido con `Test-Json`. SHA-256 `87954E41E2F96B992DB7D02205E4102551DAC2A14351A8EBDFDD3838231B2068`.
  - Respecto del borrador validado (SHA-256 `E920076C…6C4`) cambian **solo** `CorrectionsAuthorized` (`false` → `true`), `IssuedBy`
    (`Coordinator I-63`) e `IssuedUtc` (`2026-10-02T00:28:17Z`, recepción de la orden en la sesión). Lo comprobó una comparación campo a
    campo.
  - `AuthorityRevision` `658b35ad`; `MainSha` `819955d6`; alcance `src/RackCad.Application/ComputedParameters/` y
    `tests/RackCad.Tests/ComputedParameters/`.

### 22.3 Hechos del entorno para la delegación

- **Codex CLI:** `%LOCALAPPDATA%\OpenAI\Codex\bin\a51e250fa15c740a\codex.exe`, `codex-cli 0.159.2`, la misma versión de la celda del
  catálogo. Es el único binario presente; la ruta `c6fe824d725f02d7` de I-61 ya no existe.
- **`config.toml`, línea base:** SHA-256 `40C27B570B0056BC6D2B5AAF460628922C5AD39A405751E68B9001FFDF15F74F`. Cada relevo compara salida y
  entrada (P-01).
- **Worker:** subagente lanzado con un workflow de un solo agente con `model` y `effort` explícitos, como en las invocaciones medidas de
  I-61 (evidencia de I-61, `wf_9bb26e7e-466` y `wf_306d36ac-e20`).
- **Tope de invocaciones (P-07):** el plan de gates de la Proposal V3 §21 no fija un tope de invocaciones para I-63. Rigen los topes de
  AUTOMATION_PLAN 16.8: dos reejecuciones por fase, `MaxReworkLoops` = 3 y `attempts` < `automation.max_attempts` = 3.

## 23. G1-RACK-METRICS bajo I-61: cadena hasta el STOP S-04

### 23.1 Invocaciones

| `RunId` | Fase | Participante (solicitado = efectivo) | Resultado |
|---|---|---|---|
| `R20261002T002817Z-6294` | PLANNING | Codex CLI, `gpt-6-luna`, `high` | Delegación válida; aceptación A1-A8 en `pass` |
| `R20261002T003429Z-a139` | CONTROL (nc4) | — (sin invocar) | A3 `fail` y A1, A2, A4-A8 iguales a la real: `REJECTED_BEFORE_INVOCATION`, STOP (P-03) confinado al control |
| `R20261002T003840Z-f180` | WORK | Subagente, `claude-sonnet-5-5`, `medium` | Commit RED `1013449d` empujado; sin GREEN; entrega `BLOCKED` con S-04 |
| `R20261002T005705Z-eccd` | VERIFICATION | Codex CLI, `gpt-6-luna`, `high` | Fallo de transporte (DEV-G1-02): 14 comprobaciones `not_run`, `EXECUTION_BLOCKED/BLOCKED` |
| `R20261002T005944Z-1192` | VERIFICATION (reejecución 1 de 2) | Codex CLI, `gpt-6-luna`, `high` | **`EXECUTION_BLOCKED/STOP`**, `FailureClass` `Ci`, `TriggeredStopConditions` [S-04] |

`config.toml` sin cambios en todas las entradas (SHA-256 `40C27B57…F74F`). No hubo participantes ajenos, huérfanos ni procesos no atribuibles
en las entradas (DEV-G1-01 explica la primera entrada de la planificación).

### 23.2 CI del commit del Worker

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| RED del Worker | `1013449d61b8292b77273ceeac30b82282b975a2` | 36947927245 | `failure`: Core `failure`; Build UI y UI Tests `success`; Build Plugin `skipped` |

TRX de Core (SHA-256 `466E98B4…02F4`; no versionado): 12412 seleccionadas, 12395 superadas y 17 fallidas.
- El filtro del contrato `FullyQualifiedName~RackCad.Tests.ComputedParameters` selecciona 16 pruebas y fallan las 16, por aserción. Cubren
  INV-04, INV-05, INV-09, INV-14, INV-16 e INV-34 (petición).
- La fallida restante es `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace` (I-23). Lo explica §23.3.

### 23.3 Hechos del STOP

- **Qué hizo el Worker:**
  - RED compilable: esqueleto de los siete archivos de producción y dos clases de prueba, con `git add` solo del alcance.
  - Empujó el RED y construyó la implementación GREEN, que pasó 16 de 16 en local.
  - **No** la commiteó: la suite Core completa fallaba en la guarda de I-23. Dejó el árbol limpio en `HEAD` = RED = remoto, y el
    parche sin commitear en `green-candidate/green-over-red.patch` (en `artifacts/`; SHA-256 `3032AE65…02BD`; no versionado).
- **La guarda:** `tests/RackCad.Tests/NamespaceFolderGuardTests.cs:211-248` exige que cada `.cs` de `tests/RackCad.Tests` declare exactamente
  `namespace RackCad.Tests`. Las dos pruebas declaran `RackCad.Tests.ComputedParameters`.
  - Cambiar ese namespace exige modificar archivos RT, prohibido tras el RED en la misma entrega.
  - Cambiar la guarda queda fuera del alcance.
- **Verificación `R20261002T005944Z-1192`:**
  - 11 comprobaciones en `pass`.
  - `Tests` en `pass` con `RedPart` `pass`: **RED acreditado** y `ChainRedFiles` = las dos pruebas de `tests/RackCad.Tests/ComputedParameters/`.
  - `Ci` en `fail`, con `RedPart` `pass`; `Trailer` y `FreeText` en `fail`.
  - S-04 → `STOP`, que precede a `REWORK` (16.9).
  - La sesión comprobó la coherencia mecánica (README §8): par válido; `FailureClass` = primera no `pass`; `VerifiedSha` `null`.

### 23.4 Lecturas y desviaciones de la sesión

- **DEV-G1-01:** la primera entrada de la planificación listó dos `git.exe` del `git fetch` del propio comprobador. Se corrigió el orden
  del script (instantánea de procesos antes de cualquier orden git); la entrada repetida está limpia.
- **DEV-G1-02:** error de la sesión en la receta de Codex. El `PATH` del hijo apuntó a una ruta inexistente; el `pwsh` del runtime está
  en `%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell`. Codex tomó el alias de WindowsApps
  y el sandbox rechazó crear el proceso (`CreateProcessAsUserW failed: 5`).
  - Es un fallo de transporte (16.8), no una denegación de credenciales.
  - La reejecución usa el mismo prompt, con solo el `RunId` y la ruta de las entradas cambiados.
- **CTRL-G1-01:** el `fail` de `Trailer` contradice la propia evidencia del Controller. Su lectura de `inputs/worker-handoff.json`
  devolvió `Worker.Trailer` = `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`, que coincide con el commit RED y con el modelo
  efectivo; la `Evidence` dice `null`. No cambia la disposición.
- **`FreeText`:** el `fail` es correcto. La entrega usa «candidato GREEN», término de 16.10, en cuatro campos de texto libre.
- **Disposición de la entrega:** el Worker declaró `Disposition` `BLOCKED` con S-04, cuyo comportamiento es STOP (16.11). Lo corrige
  la verificación.
- **Pregunta abierta del Worker:** el parche sin commitear devuelve `Unavailable(EnvelopeUnreadable)` cuando la petición no tiene hermanas
  con identidad atribuible.
  - El Freeze no fija ese caso y ningún INV de G1 lo pide.
  - Es un supuesto del Worker, no comprometido en Git.

### 23.5 Causa raíz (propuesta de la sesión para el `analysis.md` del Coordinator)

- **Origen:** la exigencia «namespace `RackCad.Tests.ComputedParameters`» **no** está en el Freeze, la A-1 ni el contrato de gate.
  - El contrato solo fija el filtro `FullyQualifiedName~RackCad.Tests.ComputedParameters`.
  - La introdujo la sesión en el prompt de planificación (`R20261002T002817Z-6294`, `prompt.md`, delta de `AllowedWriteScope`). El
    Controller la copió a `AcceptanceCriteria[0]` y la sesión la repitió en el prompt del Worker.
  - La sesión no contrastó esa exigencia con la guarda vigente de I-23.
- **El filtro del contrato es satisfacible sin tocar la guarda.** El filtro compara por subcadena, así que basta un namespace
  `RackCad.Tests` con clases cuyo nombre empiece por `ComputedParameters` (nombre completo `RackCad.Tests.ComputedParameters…`).
- **Alcance de la corrección:**
  - No cambia el Freeze, la A-1, el alcance, los invariantes ni la materialidad.
  - Cambia archivos RT, así que exige un RED nuevo (16.8: el RED de la corrección incluye todo cambio de las pruebas de la cadena).
  - Es un STOP cuya resolución cambia el trabajo: `attempts` + 1 (README §9).

### 23.6 Custodia

Copias en `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/<RunId>/` y `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS-nc4/R20261002T003429Z-a139/`:
- planificación: `gate-contract.json`, `prompt.md`, `delegation.json`, `acceptance.json`, `acceptance-reasons.json` y `relay-record.json`;
- nc4: `delegation.json`, `acceptance.json`, `acceptance-reasons.json` y `relay-record.json`;
- trabajo: `prompt.md`, `worker-handoff.json` y `relay-record.json`;
- cada verificación: `prompt.md`, `controller-verification.json` y `relay-record.json`.

Eventos, transcripciones, TRX y el parche del Worker no se versionan: quedan sus SHA-256 en los registros de relevo. Tres transitorios
llevan CRLF (`gate-contract.json`, la `delegation.json` de nc4 y `worker-handoff.json`). Su blob normalizado difiere del SHA-256
transitorio por fin de línea (16.12); el contenido es el mismo.

### 23.7 Contadores

- `attempts` = 0.
- Reejecuciones de (G1-RACK-METRICS, VERIFICATION): 1 de 2.
- Correcciones lanzadas: ninguna.
- Controles negativos nc1-nc3: no se ejecutan, porque la cadena no llegó a `EXECUTION_VERIFIED` (README §10).

## 24. Decisión del Coordinator sobre el STOP S-04 y corrección 1

- **Entrada:** «I-63 / G1-RACK-METRICS — STOP S-04 analysis», pegado por el usuario en el chat el 2026-10-02, sin texto propio.
  - Copia literal en `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T005944Z-1192/analysis.md`, SHA-256 `6080f3d008d032d4c6565f5f38df849f294c1a3c981d8ce9c31a14c5259a1fb8` (LF; igual al blob).
  - Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Decisión:** el Coordinator confirma la causa raíz de §23.5 y autoriza la corrección bajo `CorrectionsAuthorized = true`.
  - Namespace `RackCad.Tests` y clases `ComputedParameters*`.
  - Sin tocar la guarda de I-23 ni el contrato ni su filtro.
  - Sin términos de gate en el texto libre del Worker.
  - RED nuevo solo de pruebas antes del GREEN.
  - Aclara D-17: una petición sin ninguna hermana con RackId identificable → `ArgumentException`.
- **Contadores:** `attempts` pasa de 0 a 1 en este commit, antes de la delegación de corrección (16.8; README §9, «STOP resuelto por el
  Coordinator con un cambio del trabajo»). Contador de la clase `Ci`: 1. `AttemptsRemaining` = 2.
- **Cadena:**
  - `ChainBaseSha` `f71de12b` (primera delegación).
  - RED vigente `ChainRedSha` `1013449d` (acreditado en `R20261002T005944Z-1192`).
  - `ChainRedFiles` = `tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs` y `…/RackMetricRequestTests.cs`.
  - `CorrectionOf` = {`R20261002T005944Z-1192`, `Ci`, SHA-256 del `analysis.md`}.
- **Contrato de gate:** sin reemisión. No hubo rebase y `main` sigue en `819955d6`.
- **CI del commit de custodia** `75d803e3`: corrida `push` 36949456768 en `failure`. Era lo esperado: la rama conserva el RED (las 16 pruebas
  focales contra el esqueleto y la guarda de I-23) hasta el GREEN de la corrección.

## 25. Corrección 1: planificación rechazada en la aceptación (STOP P-03)

- **Planificación** `R20261002T011628Z-7097` (Codex CLI, `gpt-6-luna`, `high`, efectivos = solicitados; cesión 01:17:26Z-01:19:03Z, código 0).
  - El prompt sigue el `analysis.md` y lleva la cadena custodiada. Antes de invocar, la sesión comprobó la ruta del `pwsh` del runtime.
  - El contrato de gate no se reemitió: es una copia en el directorio del intento 1, con SHA-256 `87954E41…2068`.
- **Delegación:** válida contra el esquema, con SHA-256 `E9ACB80D…DB53`.
  - Las reglas del `analysis.md` están en `Objective` y `AcceptanceCriteria`: namespace `RackCad.Tests`, clases `ComputedParameters*`, RED
    solo de pruebas y `ArgumentException`.
  - `ForbiddenWriteScope` añade `NamespaceFolderGuardTests.cs`.
  - La cadena y `CorrectionOf` son iguales a los custodiados.
- **Aceptación A1-A8, sin cortocircuito:** A5 en `fail` y las demás en `pass`.
  - Faltan dos `Authorities` del contrato por igualdad exacta de (ruta, sección, clase). La delegación escribe `## Convenciones
    arquitectónicas (obligatorias)` (`AGENTS.md`) y `D1 (núcleo neutral) y D24` (ADR-0043), con tilde.
  - El contrato dice `arquitectonicas` y `nucleo`, y el encabezado real de `AGENTS.md:57` no lleva tilde. El Controller normalizó la
    ortografía de las etiquetas en lugar de copiarlas.
  - La disposición de A5 es **STOP (P-03)** (16.5) y ningún fallo invoca al Worker: no hay prompt ni invocación del Worker.
- **Precedente:** I-64 tuvo un STOP P-03 con la misma forma (A4/A5 por una transcripción de las etiquetas) y lo devolvió a su Coordinator.
- **Contadores:** `attempts` = 1, sin cambio, porque no hubo trabajo nuevo. Planificaciones del intento 1: 1. Control nc4: no aplica, porque
  solo se exige en la primera delegación de la cadena (README §10).
- **Custodia:** `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T011628Z-7097/`, con `gate-contract.json`, `prompt.md`,
  `delegation.json`, `acceptance.json`, `acceptance-reasons.json` y `relay-record.json`. El `gate-contract.json` transitorio lleva CRLF
  (16.12).
- **Propuesta de la sesión:**
  - Que el Coordinator resuelva el STOP sin cambio del trabajo, con una planificación nueva del intento 1 y un `RunId` nuevo.
  - Que el prompt exija copiar `Authorities` del contrato **byte a byte** (ruta, sección y clase, sin normalizar tildes ni ortografía).
  - Que la sesión compare antes de aceptar, como ya hace A5.
  - Sin incremento de `attempts` (README §9: no cambia el trabajo).

## 26. Resolución del STOP P-03 y continuación de la corrección 1

- **Orden del Coordinator**, pegada por el usuario en el chat el 2026-10-02. Resumen en las [decisiones](../decisions/I-63.md) §2.
  - Resuelve P-03 sin cambio del trabajo; `attempts` sigue en 1 (README §9).
  - Planificación nueva con `RunId` nuevo; `Authorities[]` copiadas byte a byte del contrato emitido.
  - A1-A8 deben estar en `pass` antes del Worker. Con `pass`, la cadena sigue de forma autónoma hasta la verificación; con STOP, vuelve al
    Coordinator.
- **Comprobación de la sesión:** además de A5, compara cada `Authorities[i]` de la delegación con el contrato por igualdad de bytes, antes
  de aceptar.

## 27. Corrección 1: Worker y verificación (STOP S-12)

### 27.1 Invocaciones

| `RunId` | Fase | Participante (solicitado = efectivo) | Resultado |
|---|---|---|---|
| `R20261002T061154Z-9f77` | PLANNING | Codex CLI, `gpt-6-luna`, `high` | `Authorities` del contrato iguales byte a byte; A1-A8 en `pass` |
| `R20261002T061533Z-8e64` | WORK | Subagente, `claude-sonnet-5-5`, `medium` | RED `fc30dc6c` (solo pruebas) y GREEN `b5ee157d` (cinco archivos de producción); `IMPLEMENTATION_COMPLETE` |
| `R20261002T062554Z-e271` | VERIFICATION | Codex CLI, `gpt-6-luna`, `high` | **`EXECUTION_BLOCKED/STOP`**, `FailureClass` `Authority`, [S-12] |

`config.toml` sin cambios. Ninguna entrada encontró participantes ajenos, huérfanos ni procesos no atribuibles. El Worker tardó 319 s, con 15
llamadas y 0 denegaciones ni órdenes fallidas. Partió del parche no commiteado del intento 0 y cambió la rama sin hermanas identificables a
`ArgumentException`.

### 27.2 CI

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| RED de la corrección | `fc30dc6cf0d2f97e6732bbb8c512623db91c305d` | 36972851773 | `failure`: Core `failure` con exactamente las 17 pruebas focales fallidas (12396 de 12413 superadas; `NamespaceFolderGuardTests` en verde); Build UI y UI Tests `success`; Plugin `skipped` |
| GREEN de la corrección | `b5ee157d7c42a3bada7d6804cbbe3cd731405e00` | 36973107547 | **`success`**: los cuatro jobs requeridos en `success`; Core 12413 de 12413 |

### 27.3 Verificación `R20261002T062554Z-e271`

- 12 comprobaciones en `pass`, entre ellas `Ci` y `Tests.RedPart`. **RED nuevo acreditado:** `ChainRedSha` `fc30dc6c`; `ChainRedFiles` sin
  cambio.
- **`Authority` en `fail` (S-12 → STOP).** La delegación añadió como `UNIT_DOC` el `analysis.md` del STOP S-04.
  - Ese archivo no existe en `AuthorityRevision` `658b35ad`: se dio de alta en `faa5f16e`, y 16.3 lee las `UNIT_DOC` en `AuthorityRevision`.
  - **Causa:** el prompt de planificación de la sesión lo listaba como autoridad «en HEAD». Ya estaba citado por
    `CorrectionOf.AnalysisSha256`, así que no hacía falta como autoridad.
- **`Tests` en `fail` (REWORK).** D-02 (Proposal V3 §5) exige tokens «declarados explícitamente y probados en los dos sentidos».
  - Las pruebas usan `RackMetricIds`, pero ninguna afirma los seis pares (`MetricScope`, token).
  - El código declara los seis `MetricId` congelados; falta la prueba, no el comportamiento.
- Coherencia mecánica (README §8) comprobada por la sesión: par válido; `FailureClass` = primera no `pass`; STOP sobre REWORK; `VerifiedSha`
  `null`.

### 27.4 Propuesta de la sesión para el `analysis.md` del Coordinator

- **Corrección 2**, con `attempts` 1 → 2 (`AttemptsRemaining` 1). Es otra `FailureClass` (`Authority`), así que va con `analysis.md` previo
  (16.8).
- **Autoridades:** delegación con las `Authorities` del contrato exactas y `AuthorityRevision` `658b35ad`, sin el `analysis.md` como autoridad.
  Los análisis se citan solo por `CorrectionOf`.
  - Alternativa: reemitir el contrato con `AuthorityRevision` `44fb8dd0`. `git diff --name-only 658b35ad 44fb8dd0` solo toca documentos,
    evidencia y estado de la unidad y los nueve archivos del alcance de G1, sin rutas de autoridad `EXTERNAL`.
  - La reemisión cierra la delegación abierta.
- **Prueba de D-02** en un archivo de pruebas **nuevo** dentro del prefijo del contrato, p. ej.
  `tests/RackCad.Tests/ComputedParameters/RackMetricIdsTests.cs`, con namespace `RackCad.Tests` y clase `ComputedParameters*`.
  - Así el diff no toca `ChainRedFiles`, y la entrega no exige RED (16.8; `ChainRedSha` `fc30dc6c` acreditado).
  - Si la prueba fuera a los archivos de la cadena, exigiría un RED. No hay comportamiento que desactivar, así que ese RED no sería
    observable (C-06).
  - La prueba afirma los seis pares (`MetricScope`, token) en los dos sentidos y la forma de los tokens (ASCII, minúscula inicial, letras y
    dígitos, `Ordinal`).
- Controles negativos nc1-nc3: siguen pendientes de una verificación `EXECUTION_VERIFIED`.

### 27.5 Custodia

`docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/<RunId>/` para `R20261002T061154Z-9f77`, `R20261002T061533Z-8e64` y `R20261002T062554Z-e271`. Llevan CRLF los transitorios R20261002T061154Z-9f77/gate-contract.json, R20261002T061533Z-8e64/worker-handoff.json; su blob normalizado
difiere del SHA-256 transitorio por fin de línea (16.12).

## 28. Decisión del Coordinator sobre el STOP S-12 y corrección 2

- **Entrada:** orden del Coordinator pegada por el usuario en el chat el 2026-10-02.
  - El `analysis.md` va materializado literal en `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T062554Z-e271/analysis.md`, SHA-256 `a860e8e959f7ab4e491c7856e661370cb9505af63246d785f47691f248260696` (LF; igual al blob).
  - Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Decisión:** la corrección 2 corrige los dos defectos de §27.3 sin cambiar el Freeze, la A-1, el contrato, `AuthorityRevision` ni el
  alcance.
  - `Authorities[]` byte a byte, sin `analysis.md`.
  - Prueba de D-02 en un archivo nuevo, sin RED nuevo.
  - Producción intacta.
- **Contadores:** `attempts` pasa de 1 a 2 en este commit (`AttemptsRemaining` = 1). Contador de la clase `Authority`: 1 (el de `Ci` sigue en 1).
- **Cadena:**
  - `ChainBaseSha` `f71de12b`.
  - `ChainRedSha` `fc30dc6c` (acreditado en `R20261002T062554Z-e271`).
  - `ChainRedFiles` sin cambio.
  - `CorrectionOf` = {`R20261002T062554Z-e271`, `Authority`, SHA-256 de este `analysis.md`}.

## 29. Corrección 2: planificación rechazada en la aceptación (STOP P-03)

- **Planificación** `R20261002T135956Z-2105` (Codex CLI, `gpt-6-luna`, `high`, efectivos = solicitados; cesión 14:01:27Z-14:03:15Z, código 0).
  - El prompt transcribe literalmente las `Authorities` del contrato y ordena no añadir ninguna.
  - El alcance es solo el archivo nuevo de la prueba de D-02.
- **Delegación:** válida contra el esquema. `Authorities` es igual al contrato byte a byte y sin añadidos; la cadena y `CorrectionOf` son
  iguales a los custodiados.
- **Aceptación A1-A8, sin cortocircuito:** A5 en `fail` y las demás en `pass`.
  - `StopConditions` = [S-03, P-03]: faltan 23 de las 25 del contrato. El Controller copió las paradas de su propia cabecera (línea 28 del
    prompt) en lugar de las del contrato, aunque el delta le pedía «todos los del contrato».
  - La disposición es **STOP (P-03)** y no se invoca al Worker.
- **Defecto adicional**, que A1-A8 no mide: `ForbiddenWriteScope` añade el prefijo `tests/RackCad.Tests/ComputedParameters/`. Ese prefijo
  cubre el único archivo de `AllowedWriteScope`, así que el paquete sería contradictorio para el Worker.
- **Procesos transitorios:** la primera salida y la primera entrada vieron `git.exe` y `bash.exe` efímeros, hijos de otras sesiones de
  Claude Code (`claude.exe` 7356 y 40860).
  - Ya no existían al releerlos por PID y no llevaban la ruta del worktree, así que no son P-02.
  - Las comprobaciones repetidas están limpias. Igual que DEV-G1-01 y la lección de I-61 sobre procesos del host.
- **Contadores:** `attempts` = 2, sin cambio. Planificaciones del intento 2: 1.
- **Custodia:** `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T135956Z-2105/` (`gate-contract.json` transitorio con CRLF, 16.12).
- **Propuesta de la sesión:** resolver el STOP sin cambio del trabajo, con una planificación nueva del intento 2 y un `RunId` nuevo.
  - El prompt transcribe literalmente, además de `Authorities`, las `StopConditions`, `Invariants` y `RequiredTests` del contrato.
  - La cabecera del Controller no usa una línea de paradas que pueda confundirse con las del paquete.
  - `ForbiddenWriteScope` = el del contrato más los nueve archivos exactos, sin prefijos que cubran el archivo nuevo.
  - La sesión añade a la aceptación la comprobación «ninguna entrada de `ForbiddenWriteScope` cubre `AllowedWriteScope`».

## 30. Resolución del STOP P-03 de la corrección 2

- **Orden del Coordinator**, pegada por el usuario en el chat el 2026-10-02. Resumen en las [decisiones](../decisions/I-63.md) §2.
  - Resuelve P-03 sin cambio del trabajo; `attempts` sigue en 2.
  - Seis colecciones copiadas exactamente del contrato: `Authorities`, `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`,
    `RequiredTests` y `StopConditions`.
  - La restricción al archivo nuevo de la prueba de D-02 va en `Objective` y `AcceptanceCriteria`.
  - La sesión comprueba que ninguna entrada prohibida cubra una permitida.
- **Comprobación de la sesión antes del Worker:**
  - A1-A8;
  - igualdad exacta de las seis colecciones con el contrato;
  - coherencia de alcance (A3 ampliada: ninguna entrada de `ForbiddenWriteScope` de la delegación cubre una de `AllowedWriteScope`).

## 31. Corrección 2: segunda planificación rechazada en la aceptación (STOP P-03)

- **Planificación** `R20261002T141601Z-4a2b` (Codex CLI, `gpt-6-luna`, `high`, efectivos = solicitados; cesión 14:16:10Z-14:17:57Z, código 0).
  - El prompt transcribe literalmente las seis colecciones del contrato.
  - Salida y entrada limpias.
- **Igualdad exacta con el contrato** (comprobación de la sesión):
  - `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`, `RequiredTests` y las 25 `StopConditions` son iguales, y la coherencia de
    alcance pasa: las correcciones de §30 funcionaron.
  - `Authorities` vuelve a llevar tilde en dos etiquetas: `## Convenciones arquitectónicas (obligatorias)` (`AGENTS.md`) y `D1 (núcleo
    neutral) y D24` (ADR-0043). El contrato y el JSON literal del prompt las escriben sin tilde.
  - Es la misma normalización de `R20261002T011628Z-7097`. La planificación `R20261002T061154Z-9f77`, con el mismo método, sí las copió bien:
    el modelo no la reproduce de forma estable.
- **Aceptación:** A5 en `fail` y las demás en `pass`. Disposición **STOP (P-03)**; no se invoca al Worker.
- **Contadores:** `attempts` = 2, sin cambio. Planificaciones del intento 2: 2, las dos con STOP P-03.
- **Custodia:** `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T141601Z-4a2b/`.
- **Propuesta de la sesión:**
  - **Remedio para el Controller:** el prompt nombra las dos trampas: las etiquetas `arquitectonicas` y `nucleo` van SIN tilde a propósito,
    porque así está el encabezado real de `AGENTS.md:57`, y no se corrigen.
  - **Regla para el Coordinator:** si A5 falla solo porque `Authorities` normaliza la ortografía de etiquetas (mismas rutas y clases, y las
    demás colecciones exactas), la sesión repite la planificación sin volver al Coordinator, con el tope de 16.8 (dos reejecuciones por
    fase); agotado el tope, STOP.

## 32. Resolución del segundo STOP P-03 y regla de replanificación automática

- **Orden del Coordinator**, pegada por el usuario en el chat el 2026-10-02. Resumen en las [decisiones](../decisions/I-63.md) §2.
  - Resuelve P-03 sin cambio del trabajo; `attempts` sigue en 2.
  - El Controller proyecta las seis colecciones leyendo `gate-contract.json`, con un preflight de igualdad exacta antes de terminar.
- **Regla preautorizada**, solo para este `TaskId` y el intento 2: replanificación automática cuando A5 es el único fallo y se debe a cadenas
  que tenían que copiarse literalmente del contrato, sin cambio semántico ni de `attempts`. Cualquier otro fallo vuelve al Coordinator.
- **Tope que aplica la sesión:** como máximo dos replanificaciones automáticas por (G1-RACK-METRICS, PLANNING, intento 2), por analogía con
  16.8. Agotado el tope, vuelve al Coordinator.

## 33. Corrección 2: Worker, verificación `EXECUTION_VERIFIED` y controles negativos

### 33.1 Invocaciones

| `RunId` | Fase | Participante (solicitado = efectivo) | Resultado |
|---|---|---|---|
| `R20261002T143522Z-0c4d` | PLANNING | Codex CLI, `gpt-6-luna`, `high` | Proyección mecánica del contrato: las seis colecciones iguales (`exactness.json`); alcance coherente; A1-A8 en `pass`. Replanificaciones automáticas usadas: 0 |
| `R20261002T143849Z-3698` | WORK | Subagente, `claude-sonnet-5-5`, `medium` | Commit `4a6c2d88`: solo `tests/RackCad.Tests/ComputedParameters/RackMetricIdsTests.cs`; sin RED (16.8); `IMPLEMENTATION_COMPLETE` |
| `R20261002T144842Z-0cf4` | VERIFICATION | Codex CLI, `gpt-6-luna`, `high` | **`EXECUTION_VERIFIED`**, `VerifiedSha` `4a6c2d889fc2b421076b3165b26b97162439ad57`, 14 de 14 en `pass` |
| `R20261002T145409Z-6c34` | CONTROL nc1 | Codex CLI, `gpt-6-luna`, `high` | `Identity` en `fail`, `EXECUTION_BLOCKED/STOP`; anteriores iguales a la real: **oráculo cumplido** |
| `R20261002T145411Z-2850` | CONTROL nc2 | Codex CLI, `gpt-6-luna`, `high` | `Scope` en `fail`, `EXECUTION_BLOCKED/STOP`; anteriores iguales a la real: **oráculo cumplido** |
| `R20261002T145412Z-f0cd` | CONTROL nc3 | Codex CLI, `gpt-6-luna`, `high` | `FreeText` en `fail`, `EXECUTION_REWORK_REQUIRED/REWORK`; anteriores y posterior iguales a la real: **oráculo cumplido** |

### 33.2 CI de la cadena de G1-RACK-METRICS

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| RED (corrección 1) | `fc30dc6cf0d2f97e6732bbb8c512623db91c305d` | 36972851773 | `failure`: Core con exactamente las 17 pruebas focales fallidas por aserción |
| GREEN (corrección 1) | `b5ee157d7c42a3bada7d6804cbbe3cd731405e00` | 36973107547 | `success`, cuatro jobs; Core 12413/12413 |
| Prueba D-02 (corrección 2) | `4a6c2d889fc2b421076b3165b26b97162439ad57` | 37021876416 | **`success`, cuatro jobs; Core 12423/12423; filtro del contrato 27/27** |

### 33.3 Qué observa la verificación sobre `4a6c2d88`

- `RackMetricRequest` de un rack con los estados D-28 por kind y una sola resolución.
- INV-04, INV-05, INV-09, INV-14 (contadores en los costados de A-1.3), INV-16 e INV-34 por la vía de petición, con sus pruebas en verde.
- D-02 cubierto por `ComputedParametersRackMetricIdsTests`: los seis pares congelados en los dos sentidos, catálogo cerrado y regla del token.
- `NamespaceFolderGuardTests` en verde.
- `Authorities` iguales a las del contrato, sin `analysis.md`.
- RED vigente `fc30dc6c`; `ChainRedFiles` sin cambio. La corrección 2 no tocó producción ni las pruebas protegidas.

### 33.4 Desviaciones y procesos

- **DEV-G1-03:** el ejecutor secuencial de nc1-nc3 no detenía el lanzamiento ante procesos marcados.
  - Afectó a nc2. Su salida (14:58:45Z) vio cuatro `git.exe` no atribuibles, creados a las 14:58:42Z por `claude.exe` 40812, el proceso
    utilitario de la aplicación de escritorio de Claude. Su entrada (15:04:02Z) vio otro igual.
  - Al releerlos por PID, ninguno estaba vivo, y el padre es ajeno al worktree: es el patrón de procesos del host de la lección de I-61.
  - El control es de solo lectura y su oráculo se cumple.
- **Entradas del trabajo:** la primera vio un `git.exe` efímero ya terminado, y el control de huérfanos vio `bash.exe` de otra sesión de
  Claude Code (`claude.exe` 7356, viva, sin la ruta del worktree). La entrada repetida está limpia.
- **Hechos remotos:** el sandbox del Controller no tiene red; usa los del registro de relevo.

### 33.5 Contadores y estado de la cadena

- `attempts` = 2 de 3.
- Correcciones: dos (clase `Ci` 1, clase `Authority` 1).
- Planificaciones del intento 2: 3 (dos STOP P-03 resueltos por el Coordinator y la aceptada).
- Verificaciones: intento 0, 2 (una por transporte); intento 1, 1; intento 2, 1.
- La delegación del intento 2 queda cerrada por una verificación válida registrada.

### 33.6 Custodia

Copias en `docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/<RunId>/` para `R20261002T143522Z-0c4d`, `R20261002T143849Z-3698` y `R20261002T144842Z-0cf4`, y en
`docs/automation/evidence/I-63-pilot/G1-RACK-METRICS-ncN/<RunId>/` para cada control:
- verificaciones y controles: `prompt.md`, `controller-verification.json` y `relay-record.json`;
- cada control añade `input-mutated-*.json` con el campo mutado.

Llevan CRLF los transitorios R20261002T143522Z-0c4d/gate-contract.json, R20261002T143849Z-3698/worker-handoff.json; su blob normalizado difiere del SHA-256 transitorio por fin de línea (16.12).

**Siguiente:** el Coordinator juzga G1 con esta evidencia. G2, G3 y G4 no están autorizados.

## 34. G1 PASS

- **Veredicto del Coordinator**, pegado por el usuario en el chat el 2026-10-02: **G1 — Métricas por rack: PASS**. Resumen en las
  [decisiones](../decisions/I-63.md) §2.
- **Base del juicio:**
  - `VerifiedSha` `4a6c2d889fc2b421076b3165b26b97162439ad57`; verificación `R20261002T144842Z-0cf4`, `EXECUTION_VERIFIED`, 14/14.
  - CI `37021876416` 4/4: Core 12423/12423 y filtro 27/27.
  - RED `fc30dc6c` con 17 focales fallidos; nc1-nc3 conformes.
  - Cierre `2f885759` con CI `37024929926` 4/4.
  - Revisión del diff productivo contra Freeze + A-1.
- **Invariantes satisfechos:** INV-04, INV-05, INV-09, INV-14, INV-16, INV-34 (petición) y D-02.
- **DEV-G1-03:** aceptada como desviación no bloqueante. La lección queda para los gates siguientes: el ejecutor de controles revalida PID y
  pertenencia antes de lanzar.
- **Contadores:** `attempts` usados 2 de 3.
- **Siguiente:** `current_phase` = G2. La sesión prepara el contrato de G2 desde el Freeze + A-1 y vuelve al Coordinator solo para la
  autorización formal. Sin implementación de G2 antes; G3 y G4 no autorizados.

## 35. CI del cierre de G1 y borrador del contrato de G2

### 35.1 CI del cierre de G1

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Cierre documental de G1 | `82b034b8b887e8c5338232be1c91c0508f988e8d` | 37026164283 | `success`; cuatro jobs requeridos en `success` |

### 35.2 Borrador del contrato de G2 (no emitido)

- **Ruta:** `artifacts/orchestration/I-63/G2-POPULATION/draft/R20261002T152210Z-2298/gate-contract.json`, en `artifacts/`, que ignora `.gitignore`.
  Válido contra `rackcad-gate-contract/v1`, SHA-256 `CB6E12169C41BBF98E98759DD531900F6F95B2D6EE8A9A6031EA0D687D2C334E`.
- **Tarea:** `TaskId` `G2-POPULATION`. Una sola tarea para el resultado de G2 de §21 de la Proposal V3: proyección, E1..E6, lector D-26,
  veredicto único con la delegación del handler guardada, `ProjectPopulation`, agregación `BySystem`, contador de población y determinismo.
- **Autoridad:**
  - `AuthorityRevision` propuesta: `82b034b8`, el cierre de G1, que contiene las decisiones de la unidad hasta G1 PASS.
  - `git diff --name-only 658b35ad 82b034b8` solo toca documentos, evidencia y estado de la unidad y los dos prefijos de G1; ninguna ruta de
    autoridad `EXTERNAL` (16.3).
  - `Authorities` lleva las `EXTERNAL` de G1 sin cambios y las `UNIT_DOC` con secciones de G2.
- **Alcance:** los dos prefijos de G1 más **un** archivo del Plugin, `src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs`, la única
  edición del Plugin de §19.
  - `ForbiddenWriteScope` cambia el prefijo `src/RackCad.Plugin/` por los otros siete archivos de `KindHandlers/`, las carpetas `Drawing/`,
    `Systems/` y `Views/` y el `.csproj`, para que ninguna entrada prohibida cubra el archivo permitido.
  - El resto del Plugin queda fuera por `AllowedWriteScope`.
  - Añade `NamespaceFolderGuardTests.cs`.
- **Invariantes:** INV-01, 02, 03, 06, 07, 08, 10, 11, 12, 13, 32, 33 y 35; D-10, D-10a, D-11, D-12, D-26 y D-27; D-20 al nivel Population;
  la conservación de G1, incluida la precondición `ArgumentException`; y la no regresión de consumidores.
- **Pruebas:** filtro `FullyQualifiedName~RackCad.Tests.ComputedParametersPopulation`, `MinSelected` 13 y `ExpectRed` true.
  - Clases `ComputedParametersPopulation*` en el namespace `RackCad.Tests` (guarda de I-23).
  - El filtro no selecciona las pruebas de G1; su regresión la cubre la suite Core completa.
- **Paradas:** las 25 de G1, con C-01, C-02 y C-06 adaptadas a G2, más C-07 (la delegación cambiaría el comportamiento del handler) y C-08
  (haría falta `ProjectSummary` Full, `RackComputedExpressionContext` o el núcleo de Expressions).
- **Celdas, `RoutingEnforcement` y `ExpectedEvidence`:** las celdas y `RoutingEnforcement` son los de G1. `ExpectedEvidence` añade el build
  del Plugin en la CI.
- **Campos del emisor:** `CorrectionsAuthorized` = false, `IssuedBy` = borrador. Los fija el Coordinator al emitir.

### 35.3 Preguntas para la autorización

- **Q-G2-01 (INV-32).** El control positivo congelado encamina una petición «por `ProjectSummary` (`Full`)», que es de G4.
  - Lectura propuesta para G2: el mismo contador sube (> 0) con una petición de `ProjectPopulation` que incluye el rack, y la variante
    `Full` se añade en G4.
  - Precisa la verificación de un comportamiento congelado: A-n solo del Coordinator, como la A-1.
- **Q-G2-02 (INV-11).** El oráculo pide el mismo «`ProjectSummary`» con el orden de entrada invertido.
  - Lectura propuesta para G2: el mismo `ProjectPopulation`, más representante, `DisplayName` y grafía. La comparación del `ProjectSummary`
    completo va en G4.
- **Q-G2-03 (presupuesto).** `attempts` es por unidad y no se reinicia al cerrar un gate (16.8): G2 empieza con `attempts` = 2 y
  `AttemptsRemaining` = 1.
  - Solo cabe una corrección; un segundo REWORK sería STOP (S-11).
  - Que el Coordinator lo confirme o lo ajuste es decisión suya.

## 36. Autorización de G2 y A-2

- **Orden del Coordinator**, pegada por el usuario en el chat el 2026-10-02. Resumen en las [decisiones](../decisions/I-63.md) §2.
- **A-2** (`docs/initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md`), solo del Coordinator, append-only y `Applies-to = all`.
  - Lleva literal el texto autorizado, con los deltas A-2.1 (INV-32) y A-2.2 (INV-11) y la tabla de obligaciones.
  - M-01..M-08 no activados.
- **G2 condicionalmente autorizado.** La condición: la A-2 publicada con CI exact-SHA 4/4 y el contrato reemitido con
  `AuthorityRevision` = el SHA de la A-2.
  - Respecto del borrador `CB6E1216…`, el único delta es: `AuthorityRevision`, la A-2 en `Authorities`, INV-11 e INV-32 según la A-2,
    `CorrectionsAuthorized = true`, `IssuedBy` e `IssuedUtc`.
- **Presupuesto:** `attempts` = 2/3 y `AttemptsRemaining` = 1, sin reinicio por gate.
- La CI de la A-2 y el contrato emitido se registran en la custodia de G2. El commit de la A-2 no puede registrarse a sí mismo.

## 37. G2-POPULATION bajo I-61: `EXECUTION_VERIFIED`

### 37.1 A-2 y contrato emitido

- **CI de la A-2:** `669d8a391f208e1077f136a406fd89058bc0ce6e`, corrida `push` 37035072886, `success`; cuatro jobs requeridos en `success`.
- **Contrato emitido:** `artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164053Z-586f/gate-contract.json`, válido contra el esquema, SHA-256
  `486EB708C2A66F37B854C12D4A634B7B87918EF8BD90D4620D10782B7883329D`, `IssuedUtc` `2026-10-02T16:40:53Z`.
  - Respecto del borrador `CB6E1216…` cambian **solo** `AuthorityRevision` (`669d8a39`), la A-2 en `Authorities`, INV-11 e INV-32 según la
    A-2, `CorrectionsAuthorized` (`true`), `IssuedBy` (`Coordinator I-63`) e `IssuedUtc`. Lo comprueba la emisión, campo a campo.
  - Sin conflicto de alcance.

### 37.2 Invocaciones

| `RunId` | Fase | Participante (solicitado = efectivo) | Resultado |
|---|---|---|---|
| `R20261002T164053Z-586f` | PLANNING | Codex CLI, `gpt-6-luna`, `high` | Seis colecciones proyectadas del contrato e iguales; A1-A8 en `pass` |
| `R20261002T164443Z-9b99` | CONTROL nc4 | — (sin invocar) | A3 en `fail` y las demás iguales a la real: oráculo cumplido |
| `R20261002T164522Z-a109` | WORK | Subagente, `claude-sonnet-5-5`, `high` | RED `cdc8bbd0`; GREEN `844dabb6`; `IMPLEMENTATION_COMPLETE` (1923 s, 87 llamadas) |
| `R20261002T172010Z-cbb2` | VERIFICATION | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_BLOCKED/STOP`, `Ci`, [S-04]: contadores del TRX de UI (ver 37.4) |
| `R20261002T172647Z-0aea` | VERIFICATION (reejecución 1 de 2) | Codex CLI, `gpt-6-luna`, `high` | **`EXECUTION_VERIFIED`**, `VerifiedSha` `844dabb6ffbb8b3ba11a2e9a796b78348857c088`, 14 de 14 en `pass` |
| `R20261002T173233Z-22f2` | CONTROL nc1 | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_BLOCKED/STOP`, `Identity` |
| `R20261002T173234Z-9502` | CONTROL nc2 | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_VERIFIED/NONE`, `NONE` |
| `R20261002T173235Z-861e` | CONTROL nc3 | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_REWORK_REQUIRED/REWORK`, `FreeText` |

`config.toml` sin cambios. La puerta de PID (lección DEV-G1-03) corrió antes y después de cada invocación sin procesos marcados vivos.

### 37.3 CI de la cadena

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| RED | `cdc8bbd08655f8ecdf499eb09dfec6581befc682` | 37038578016 | `failure`: Core con 52 fallidas, todas del filtro del contrato, por aserción. Las 2 superadas del filtro son una estructural de D-20 y un caso `Allow` de INV-12, que tiene otras pruebas fallidas |
| GREEN | `844dabb6ffbb8b3ba11a2e9a796b78348857c088` | 37039086898 | **`success`, cuatro jobs** (incluido «Build Plugin without AutoCAD»); Core 12477/12477; filtro `ComputedParametersPopulation` 54/54 |

### 37.4 STOP S-04 no real de `R20261002T172010Z-cbb2` (lectura de la sesión)

- El Controller vio en los dos `ui.trx` 17 `UnitTestResult` `NotExecuted` con `ResultSummary/Counters notExecuted=0`.
- **No es una contradicción.** El logger TRX de VSTest registra así las pruebas omitidas.
  - `tests/RackCad.UI.Tests` tiene 17 atributos `Skip` (8+3+4+2) en cuatro clases que G2 no toca.
  - `total` 1654 − `executed` 1637 = 17.
- Es el caso del `analysis.md` de I-61 `R20261001T035734Z-74c2`, resuelto entonces sin cambio del trabajo.
- La orden de G2 pide volver solo con `EXECUTION_VERIFIED` o un STOP real. La sesión aplicó ese precedente:
  - completó el registro del trabajo con el hecho y sus órdenes reproducibles;
  - repitió la verificación, como reejecución 1 de 2 de (G2-POPULATION, VERIFICATION), con el mismo prompt;
  - no consumió `attempts`.
- La reejecución dio `EXECUTION_VERIFIED`. **El Coordinator puede revisar esta lectura en su juicio.**

### 37.5 Hallazgos para el juicio del Coordinator

- **H-G2-01:** la guarda existente `tests/RackCad.Tests/PushBackBomCommandGuardTests.cs` (`ThePushBackHandler_ConsumesTheSharedGateAndNotASecondRule`,
  I-47 G13, fuera del alcance) exige el literal `RackBomOutputGate.For(system).Reason` en `PushBackKindHandler.cs`, leyendo el archivo
  con comentarios.
  - Tras la delegación de D-27, que INV-33 exige, ese texto ya no está en el código. El GREEN lo cita en el comentario XML del método, y por
    eso la guarda sigue en verde de forma textual.
  - El comentario es cierto (`RackOutputVerdict` compone esa puerta). Pero la guarda antigua comprueba texto del comentario y no la
    delegación.
  - Corregirla exige tocar un archivo fuera del alcance: reapuntarla a `RackOutputVerdict` necesita la autorización del Coordinator (A-n o
    iniciativa).
  - Lo declaró la entrega y lo registró el Controller como hallazgo, no como parada.
- **H-G2-03 (control negativo nc2 no superado):** nc2 excluía `src/RackCad.Application/ComputedParameters/ProjectPopulation.cs` del
  `AllowedWriteScope` de la delegación copiada (el prefijo se sustituyó por los otros siete archivos del diff, README §10).
  - El Controller devolvió `EXECUTION_VERIFIED` con `Scope` en `pass`, afirmando que los 11 archivos estaban en `AllowedWriteScope`.
  - La comprobación mecánica de la sesión da `Scope` = `fail` para nc2 (ese archivo queda fuera). Para la delegación real da `pass`: los 11
    archivos cubiertos y ninguno prohibido.
  - Es un fallo de discriminación del verificador, no de la entrega. Como la salida es válida, no es un fallo de transporte reejecutable
    (README §10): queda como control no superado para el juicio del Coordinator.
- **H-G2-02:** el fixture de INV-33 es el texto literal del método en `819955d6` con su extensión real, líneas 56-73. La Proposal V3 §20
  cita 56-71.
- **Preguntas abiertas de la entrega** (decisiones del Worker dentro del Freeze; las pruebas pasan):
  - una excepción del *store* al leer el diseño de Push Back en D-27 da `Undetermined(DesignUnreadable)`, por la regla de D-26; solo las del
    resolver o la puerta dan `ResolveFailed`. El handler devuelve `null` en ambos casos;
  - `RepresentativeDefinitionId` queda nulo para un rack que no llega a E4.
- **Ampliaciones de archivos de G1** sin cambio de comportamiento:
  - dos razones de agregado en `UnavailableReasonKind` (D-09);
  - `ClassifyKind` de `RackMetricRequest` pasa a `internal` para reutilizar la regla única de E3;
  - `PresenceRows` en `RackMetricDesignReader` (INV-35).
  - Las pruebas de G1 siguen en verde.

### 37.6 Contadores y custodia

- `attempts` = 2 de 3, sin cambio: no hubo corrección.
- Planificaciones de G2: 1.
- Verificaciones: 2 (una reejecución por el S-04 no real).
- Controles negativos: nc1, nc3 y nc4 con su oráculo cumplido; **nc2 no superado** (ver 37.5, H-G2-03).
- La delegación de G2 queda cerrada por una verificación válida registrada.

Las copias están en `docs/automation/evidence/I-63-pilot/G2-POPULATION/<RunId>/` y `docs/automation/evidence/I-63-pilot/G2-POPULATION-ncN/<RunId>/`. Llevan CRLF los
transitorios R20261002T164522Z-a109/worker-handoff.json; su blob normalizado difiere del SHA-256 transitorio por fin de línea (16.12).

**Siguiente:** el Coordinator juzga G2 con esta evidencia. G3 y G4 no están autorizados.

## 38. G2 PASS

- **Veredicto del Coordinator**, pegado por el usuario en el chat el 2026-10-02: **G2 — Population, output verdict y agregación: PASS**.
  Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Base del juicio:**
  - `VerifiedSha` `844dabb6ffbb8b3ba11a2e9a796b78348857c088`; verificación `R20261002T172647Z-0aea`, `EXECUTION_VERIFIED`, 14/14.
  - CI `37039086898` 4/4: Core 12477/12477 y filtro de G2 54/54.
  - Custodia `abe0eae5`, CI `37043281164` 4/4.
  - Revisión independiente del diff `669d8a39..844dabb6`: 11 archivos dentro del alcance.
- **DEV-G2-01:** nc2 no discriminó una violación de `Scope` conocida. No bloquea y no se repite.
  - Regla para G3 y G4: comprobación mecánica independiente de `Scope` antes de aceptar `EXECUTION_VERIFIED`.
- **DEBT-I63-G2-01:** la guarda legacy `PushBackBomCommandGuardTests` queda satisfecha por un comentario XML; la autoridad es INV-33.
  - Antes de READY: actualizarla o retirarla/sustituirla explícitamente, con una A-n Coordinator-only de solo pruebas.
- **H-G2-02:** editorial, sin enmienda.
- **S-04 de los TRX:** clasificación de la sesión aceptada (STOP no material).
- **Contadores:** `attempts` 2/3 (`AttemptsRemaining` 1).
- **Siguiente:** `current_phase` = G3. La sesión prepara el contrato de G3 y vuelve solo para la autorización formal. G4 no autorizado.

## 39. CI del cierre de G2 y borrador del contrato de G3

### 39.1 CI del cierre de G2

| Commit | SHA | Corrida `push` | Resultado |
|---|---|---|---|
| Cierre documental de G2 | `11207788bca71f173ee8c896c18c5c6af8f8cf5b` | 37046231017 | `success`; cuatro jobs requeridos en `success` |

### 39.2 Borrador del contrato de G3 (no emitido)

- **Ruta:** `artifacts/orchestration/I-63/G3-RACK-BUILTINS/draft/R20261002T181856Z-68f0/gate-contract.json`, válido contra el esquema, SHA-256
  `14C40A9AF502BBE247A1BD51833F4F085A3EBC82A3F70A8F5AA2126C6652B055`. `TaskId` `G3-RACK-BUILTINS`.
- **Autoridad:**
  - `AuthorityRevision` propuesta: `11207788`, el cierre de G2, que contiene G2 PASS, DEV-G2-01, DEBT-I63-G2-01 y la regla de `Scope`.
  - El diff desde la A-2 añade solo el código de G2 y documentos de la unidad; ninguna ruta de autoridad `EXTERNAL` (16.3).
  - `Authorities` lleva Proposal V3 (D-02..D-04, D-14..D-19, D-23, §11-§13, §19-§21), la A-1 (A-1.2 para INV-20), la A-2, el contrato y
    las decisiones. Las `EXTERNAL` son las de G2, y la de ADR-0043 añade D9.
- **Alcance:**
  - Permitido:
    - el núcleo `src/RackCad.Application/Expressions/` (D-16 y la guarda R4);
    - `src/RackCad.Application/ComputedParameters/` (`RackComputedExpressionContext`);
    - el archivo `src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs` (D-18);
    - `tests/RackCad.Tests/ComputedParameters/`;
    - el archivo `tests/RackCad.Tests/ExpressionSymbolModelTests.cs` (Q-G3-02).
  - Prohibido:
    - ProjectVariables, Systems, Bom, Catalogs, Units, Domain, Plugin y UI;
    - las cuatro suites de INV-28, `ExpressionCoreGuardTests`, `ExpressionDiagnosticCatalogTests` y `NamespaceFolderGuardTests`;
    - `docs/` y la configuración del repositorio.
  - Ninguna entrada prohibida cubre una permitida.
- **Invariantes:** INV-15, 17-27 (INV-20 con A-1.2) e INV-28; D-14..D-19; la conservación de G1 y G2.
- **Pruebas:** filtro `FullyQualifiedName~RackCad.Tests.ComputedParametersSymbols`, `MinSelected` 12 y `ExpectRed` true.
- **Paradas:** las 25 de base con C-01, C-02 y C-06..C-08 adaptadas a G3, más C-09 (`projectVariable` o INV-28) y C-10 (código nuevo para
  INV-22).
- **`ExpectedEvidence`:** añade la comprobación mecánica independiente de `Scope` (DEV-G2-01) y el ARCHITECTURE_REVIEW del diff del núcleo
  antes del cierre (§21).
- **Celdas y emisor:** celdas y `RoutingEnforcement` de G2. `CorrectionsAuthorized` = false, `IssuedBy` = borrador.

### 39.3 Hechos medidos que condicionan G3

- **Pruebas que fijan el estado previo a ID20.** `tests/RackCad.Tests/ExpressionSymbolModelTests.cs` afirma:
  - `SymbolNamespaces.All` = [`ProjectVariable`] y valores de `SymbolNamespace` = [1] (líneas 31-32);
  - «rack» no es token activo (38-45, con el comentario «reservados conceptualmente para ID20»);
  - valores de `SymbolDefinitionKind` = [1, 2] (273).
  - D-16 cambia exactamente eso: no hay G3 posible sin actualizar esas aserciones.
- **Catálogo de diagnósticos.** `ExpressionDiagnosticCatalogTests` fija el catálogo V6 completo (29 códigos, de I-49). `NameRequired` ya
  existe. Un código nuevo para INV-22 rompería esa prueba y tocaría el catálogo de I-49.
- **Persistencia.** `PersistedBoundExpressionJson` (Persistence) usa `SymbolNamespaces.TryParseToken` y `.Token`; D-18 lo pasa a la
  tabla persistida de D9.
- **Guarda P26.1.** `ExpressionCoreGuardTests` no prohíbe el término `Rack`. Prohíbe las capas ProjectVariables, Persistence, Systems,
  Bom, Catalogs, Domain, UI, Plugin y Autodesk, y los conceptos dimensionales.

### 39.4 Preguntas para la autorización

- **Q-G3-01 (INV-22).** La Proposal fija el código concreto «en el commit RED de G3, con una A-n solo del Coordinator» (AQ-02).
  - Propuesta: preautorizar que la sesión materialice esa A-n (A-3) tras el RED, con el código que el RED observe, siempre que sea uno del
    catálogo V6 vigente y sin catálogo nuevo (C-10).
  - Si hiciera falta un código nuevo: STOP.
- **Q-G3-02 (pruebas fijadas).** Propuesta: permitir `ExpressionSymbolModelTests.cs` como archivo exacto.
  - Solo para actualizar en el RED las aserciones del estado previo a ID20 que D-16 cambia: miembros de `SymbolNamespace` y
    `SymbolDefinitionKind`, y tabla de namespaces en memoria frente a la persistida.
  - Ningún otro cambio en ese archivo.
- **Q-G3-03 (ARCHITECTURE_REVIEW).** La §21 exige revisar el diff del núcleo antes del cierre de G3: quién y cuándo.
  - Propuesta: el mismo Architect de R1-R3 (sesión separada), de solo lectura, sobre el diff exacto del núcleo verificado, antes de volver
    para el juicio de G3.
- **Q-G3-04 (presupuesto).** `attempts` = 2/3: queda **una** corrección para G3 y G4 juntos, y G3 es el gate de mayor riesgo (el núcleo).
  Se señala para la decisión del Coordinator.

## 40. Autorización escalonada de G3

- **Orden del Coordinator**, pegada por el usuario en el chat el 2026-10-02. Resumen en las [decisiones](../decisions/I-63.md) §2.
  - Respuestas a Q-G3-01..04: A-3 preautorizada con condiciones; `ExpressionSymbolModelTests.cs` autorizado como archivo exacto; revisión
    del mismo Architect; presupuesto 2/3.
- **Secuencia:** G3-T1 (solo pruebas) → RED publicado → análisis de INV-22 → A-3 → CI de la A-3 → contrato de T2 contra la A-3 → GREEN →
  CI → verificación → `Scope` mecánico → revisión del Architect → juicio del Coordinator.
- **Lectura de la sesión para ejecutarla bajo I-61:**
  - T1 y T2 son dos delegaciones del mismo `TaskId` `G3-RACK-BUILTINS`, una cadena (16.8), en el directorio del intento 2.
  - La entrega de T1 solo lleva RED. Se verifica para **acreditar** el RED (`RedPart` = `pass` en `Ci` y `Tests`) y fijar `ChainRedFiles`.
    Por diseño, su `Ci` queda en `fail`, porque `CurrentSha` es el RED.
  - T2 es la continuación autorizada, no una corrección: lleva `CorrectionOf` = null, `ChainRedSha` = el RED de T1 y `ChainRedFiles` = sus
    pruebas. No consume `attempts` (orden del Coordinator).
  - Los controles negativos nc1-nc3 se ejecutan una sola vez, sobre la verificación `EXECUTION_VERIFIED` de T2 (README §10).

## 41. G3-T1 (RED-BUILTINS) bajo I-61 y A-3

### 41.1 Invocaciones

| `RunId` | Fase | Participante (solicitado = efectivo) | Resultado |
|---|---|---|---|
| `R20261002T183445Z-47d4` | PLANNING | Codex CLI, `gpt-6-luna`, `high` | Rechazada: A5 en `fail` por una cadena de invariante con la tilde quitada («propagacion» en INV-15); las demás en `pass` |
| `R20261002T184026Z-55e7` | CONTROL nc4 | — (sin invocar) | Sobre la delegación rechazada; no cuenta |
| `R20261002T184114Z-1c2f` | PLANNING (replanificación automática, regla vigente) | Codex CLI, `gpt-6-luna`, `high` | Seis colecciones proyectadas del contrato e iguales; A1-A8 en `pass` |
| `R20261002T184434Z-3b90` | CONTROL nc4 | — (sin invocar) | A3 en `fail` y las demás iguales a la real: oráculo cumplido |
| `R20261002T184507Z-9800` | WORK | Subagente, `claude-sonnet-5-5`, `high` | RED `637dce7e`; `IMPLEMENTATION_COMPLETE` (2055 s, 88 llamadas) |
| `R20261002T192212Z-e1aa` | VERIFICATION | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_BLOCKED/BLOCKED`, `Remote` (ver 41.3) |
| `R20261002T193101Z-2628` | VERIFICATION (reejecución 1 de 2) | Codex CLI, `gpt-6-luna`, `high` | Fallo de transporte `TIMEOUT` (600 s, código 124), sin salida |
| `R20261002T194212Z-73b1` | VERIFICATION (reejecución 2 de 2) | Codex CLI, `gpt-6-luna`, `high` | **`EXECUTION_REWORK_REQUIRED/REWORK`, `Ci`**, con `RedPart` = `pass` en `Ci` y `Tests`: **RED acreditado** |

- **Contrato de T1:** `artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T184114Z-1c2f/gate-contract.json`, SHA-256 `C35EFC7C2D90014A2E1553AB8EA25242588CAF4E88EA581A80D5D3FBDC404DC8`, `AuthorityRevision`
  `e99621f9`. Delegación aceptada: SHA-256 `A23A12B8994EC89C6FCE830EB7139D74DC235DF485DC1D4AA4CC69677F2C1142`.
- La puerta de PID corrió antes y después de cada invocación sin procesos marcados vivos. `config.toml` sin cambios.
- El REWORK de `R20261002T194212Z-73b1` es el resultado previsto de una entrega solo RED (§40): `Ci` no puede pasar con `CurrentSha` = RED. No es
  una corrección y no consume `attempts`.

### 41.2 RED de la cadena

| Dato | Valor |
|---|---|
| `ChainBaseSha` | `e99621f9e285b1a7ddfcf51cd8ccf970828e0b66` |
| `ChainRedSha` | `637dce7e5e7811992330b3c305358d9a7542b9f6` (corrida `push` 37053113058: Core en `failure` por el RED, Plugin `skipped`) |
| `ChainRedFiles` | Los siete archivos nuevos de `tests/RackCad.Tests/ComputedParameters/` (`ComputedParametersSymbols{Binding,Consumers,Context,Formatting,Identity,Persistence}Tests.cs` y `ComputedParametersSymbolsKit.cs`) y `tests/RackCad.Tests/ExpressionSymbolModelTests.cs` |
| Pruebas | Filtro `RackCad.Tests.ComputedParametersSymbols`: 89 seleccionadas, 86 fallan por aserción o excepción, 3 pasan (filas de INV-22 que fijan el analizador vigente). Core 12566/12478/88: las 86 y las 2 aserciones pre-ID20 autorizadas de `ExpressionSymbolModelTests` |
| Producción | Sin cambios: `git diff e99621f9..637dce7e` solo toca `tests/` |

### 41.3 BLOCKED por `Remote` de `R20261002T192212Z-e1aa` (lectura de la sesión)

- El Controller buscó `Entry.OriginMainSha`, un campo que el esquema `rackcad-relay-record/v1` no tiene en `Entry`. El dato está en
  `RemoteFacts.OriginMainSha` = `819955d6` = `MainSha`.
- Es un error del Controller del tipo de CTRL-G1-01, no un defecto de la entrega.
- Recuperación de BLOCKED (16.11): nota explícita en el registro del trabajo y reejecución con el mismo prompt.
- La reejecución 1 agotó el tope de 600 s sin salida (`TIMEOUT`). La 2 terminó con la salida descrita.
- **Las dos reejecuciones de (G3-RACK-BUILTINS, VERIFICATION) quedan consumidas** (AUTOMATION_PLAN 16.11: «como máximo dos reejecuciones
  por (`TaskId`, fase)»). La sesión lo lee de forma estricta: un BLOCKED o un fallo de transporte en la verificación de T2, que comparte
  `TaskId` y fase, sería STOP P-04.
- Por eso el prompt de verificación de T2 anticipa las dos causas vistas: la señal de `origin/main` en `RemoteFacts` y el tope de 600 s,
  con lecturas filtradas en lugar de volcados completos.

### 41.4 Entrega del Worker: declaraciones

- **Copia desechable.** El Worker creó, solo bajo `artifacts/` (ignorado por git), una copia del repositorio con una implementación
  mínima de D-16, D-17 y D-18, para comprobar que las pruebas del RED son implementables: 133/133 en la copia. La copia se eliminó y el
  parche queda en `work/shadow_patch.py`. No toca `src/` del repositorio.
- **Sonda temporal.** La prueba sonda `ZzProbeInv22.cs` se retiró antes del commit.
- **API que fijan las pruebas del RED.** La entrega la lista en su `Evidence`: núcleo, persistencia y
  `RackComputedExpressionContext`.
- **Pregunta abierta.** «confirmar los nombres de API […] y si el mensaje con Computed de INV-27 y la ausencia de una API nombrada para
  la tabla de tokens persistidos de D9 son aceptables».
  - **Lectura de la sesión:** no es materia de la A-3, que se limita a INV-22.
  - Los nombres son decisiones del Worker dentro del Freeze, fijadas por pruebas protegidas. INV-27 solo exige el rechazo de `Computed`.
  - D-18 exige el comportamiento de la tabla persistida (leer `rack` → `PresentButUnreadable`; escribir → rechazo), no un nombre de API.
  - Se somete a la revisión del Architect (foco D-16 y D-18).
- **TRX de UI.** 17 `NotExecuted` por los 17 `Skip` (8+3+4+2): la convención del logger, anotada en el registro (precedente §37.4).

### 41.5 INV-22 observado

- **Worker** (`work/inv22-probe*.txt`, en custodia): `Rack.#{zzz}` → `Succeeded` = false, un `InvalidQualifier` en 5+6, `Syntax` lanza
  `InvalidOperationException`; igual con `* 2` y `+1`; `Rack.#{frentes}` → 5+10.
- **Sesión** (sonda desechable fuera del worktree contra el DLL Debug; `Expressions/` idéntico en main y en el RED; `A-3-session-probe/`):
  - confirma lo anterior;
  - para `Rack.#{abcdef0123456789abcdef0123456789}` (clave `rack` válida y GUID en forma N) da un `UnexpectedToken` (3) en 5+35, sin árbol.
- Ambos códigos están en el catálogo V6. C-10 no se activa.

### 41.6 A-3

- `docs/initiatives/I-63-proposal-v3-amendment-a3-inv22-diagnostico.md`, Coordinator-only preautorizada.
- **Fija** el resultado del oráculo de INV-22 (§2 de la A-3). **Declara sin fijarlo** el caso de la forma N (§4 de la A-3).
- No cambia D-16, lexer, parser, `projectVariable` ni alcance.
- **Siguiente:**
  - CI exact-SHA de la A-3 (cuatro jobs);
  - reemisión del contrato de T2 con `AuthorityRevision` = SHA de la A-3;
  - planificación, GREEN, verificación, `Scope` mecánico, nc1-nc3 y revisión del Architect.

### 41.7 Contadores y custodia

- `attempts` = 2 de 3, sin cambio.
- Planificaciones de G3-T1: 2 (una replanificación automática por cadenas literales, regla vigente).
- Verificaciones: 3 (original y dos reejecuciones).
- Controles nc4: 2 (solo cuenta el de la delegación aceptada).

Las copias están en `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/<RunId>/`, `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS-nc4/<RunId>/` y `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/A-3-session-probe/`.
Llevan CRLF los transitorios R20261002T184507Z-9800/inv22-probe.txt, R20261002T184507Z-9800/inv22-probe2.txt, A-3-session-probe/a3-session-probe.txt; su blob normalizado difiere del SHA-256 transitorio por
fin de línea (16.12).

## 42. CI de la A-3: la condición «4/4» de G3-T2 no se puede cumplir antes del GREEN

### 42.1 Hechos

- **A-3 publicada:** `ea70e3b333efec49d3c3b9bc1e5d0395adc71a01` (padre: el RED `637dce7e`). El commit solo toca `docs/`, y `src/` y `tests/` son idénticos
  a los del RED (`git diff --quiet 637dce7e ea70e3b3 -- src tests`).
- **Corrida `push` 37058065873 del SHA exacto:** `failure`.

  | Job | Resultado |
  |---|---|
  | UI Tests (WPF controls, net8.0-windows) | `success` |
  | Tests (Domain + Application) | **`failure`** |
  | Build UI (WPF, valida API de Application) | `success` |
  | Build Plugin without AutoCAD | `skipped`, porque depende de Core |

- **Core:** 12566 seleccionadas, 12478 superadas y 88 fallidas, las mismas cifras que la corrida del RED (37053113058).
  - El multiconjunto de nombres de las fallidas es **idéntico** al del RED: las 86 focales de `ComputedParametersSymbols` y las 2
    aserciones pre-ID20 de `ExpressionSymbolModelTests`.
  - Ninguna fallida queda fuera de `ChainRedFiles`.
  - TRX: `core.trx` SHA-256 `00779B64F827F05915F317410E2F371DB8D5B6186A61876BF04910BBA17A7C2D`; `ui.trx` 1654/1637/0.

### 42.2 Lectura de la sesión

- La orden de G3 exige, para G3-T2, «A-3 válida publicada + CI exact-SHA 4/4». La misma orden exige que el RED exista antes de la A-3.
- Mientras el GREEN no exista, todo commit de la rama posterior al RED contiene las pruebas RED, que fallan por diseño. Así que la CI
  exact-SHA de la A-3 **no puede** dar 4/4: Core falla con el conjunto RED acreditado y Plugin queda `skipped`.
- Lo que la CI sí acredita es que la A-3 no cambia código ni pruebas y no introduce ningún fallo nuevo.
- Fijar el sentido de una condición de autorización es del Coordinator, no de la sesión. Por eso **G3-T2 no se planifica ni se
  delega**; vuelta al Coordinator. `attempts` sigue en 2 de 3. No hay Worker ni Controller abiertos.

### 42.3 Opciones para el Coordinator

- **Opción 1 (recomendada):** dar por cumplida la condición con la corrida 37058065873, porque su resultado equivale al del RED
  acreditado:
  - el commit solo toca `docs/`;
  - Core falla solo con el multiconjunto RED idéntico (88);
  - UI y Build UI en `success` y Plugin `skipped` por dependencia.
  - El 4/4 exacto lo exige, como siempre, la corrida del GREEN de T2, que contiene la A-3.
- **Opción 2:** sustituir la condición por «la corrida exact-SHA del GREEN de T2 con 4/4», sin condición de CI propia para la A-3.
- **No se recomienda** revertir el RED para publicar la A-3 en verde: rompe la cadena 16.8 y el orden RED → A-3.

### 42.4 Preparado, sin emitir

- Borrador del contrato de T2: `artifacts/orchestration/I-63/G3-RACK-BUILTINS/draft-t2/R20261002T200818Z-6c4f/gate-contract.json`, SHA-256 `0BA36A4A0112D8966BA0D9E6C22E6A2E4AD5588B584177D734960FA249486E43`. Es válido contra el esquema y no tiene conflicto de
  alcance.
- Respecto del borrador de G3 cambian:
  - `AuthorityRevision`, que pasa a ser el SHA de la A-3;
  - `Authorities`, con la A-3 tras la A-2;
  - el texto de INV-22, con el resultado de la A-3;
  - un invariante: G3-T2 no tiene RED nuevo y no modifica `ChainRedFiles`;
  - la parada C-11: el GREEN exigiría tocar `ChainRedFiles`, o una expectativa del RED es incompatible con el Freeze;
  - `Objective` y `ExpectedEvidence` de T2;
  - `CorrectionsAuthorized`, `IssuedBy` e `IssuedUtc`.
- El alcance (`AllowedWriteScope` y `ForbiddenWriteScope`), `RequiredTests`, `EligibleCells` y `RoutingEnforcement` son los del borrador.
- Delegación de T2 prevista:
  - `ChainBaseSha` `e99621f9`, `ChainRedSha` `637dce7e` y `ChainRedFiles` = los 8 archivos de §41.2;
  - `CorrectionOf` = null, `Attempt` 2 y `AttemptsRemaining` 1;
  - sin nc4, porque no es la primera delegación de la cadena.

### 42.5 Errata de la A-3 (sin editarla)

- La A-3 §4 cita `95690c28` como «(main)». El `main` vigente es `819955d61a6da4c811a11fbd11b5dca13f634b7c`, del que `95690c28` es
  ancestro.
- `src/RackCad.Application/Expressions/` también es idéntico entre `819955d6` y `637dce7e` (comprobado). La afirmación de la A-3 se
  mantiene, y el resultado fijado no cambia.

## 43. Autorización de G3-T2 y errata de la A-3

- **Orden del Coordinator**, pegada en el chat el 2026-10-02 tras §42. Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Condición corregida de la A-3, satisfecha:**
  - la A-3 `ea70e3b333efec49d3c3b9bc1e5d0395adc71a01` está publicada sobre el RED acreditado `637dce7e`;
  - solo toca `docs/`, y `src/` y `tests/` son idénticos a los del RED;
  - su CI exact-SHA, la corrida 37058065873, se inspeccionó: Core falla con exactamente las mismas 88 pruebas del RED y no hay fallas
    nuevas (§42.1).
- **Estado fijado por la orden:** G3-T1 COMPLETE; RED ACCREDITED; A-3 VALID; G3-T2 AUTHORIZED; `attempts` 2/3; G4 NOT AUTHORIZED; sin
  decisión del Owner.
- **INV-22 para T2:**
  - `Rack.#{zzz}` → `InvalidQualifier` (11, `SyntaxAndLimits`), span 5+6, sin árbol sintáctico ni enlazado, determinista;
  - el caso de una clave de 32 caracteres hexadecimales en forma N **no** es requisito de G3;
  - alterarlo de forma accidental es STOP (C-12 del contrato de T2). Si se conserva, se registra como observación.
- **Presupuesto de verificación:** la verificación de T2 tiene su propio tope de dos reejecuciones por (`TaskId`, fase). Esto sustituye
  la lectura estricta de la sesión en §41.3.
- **Errata de la A-3** (sin editarla y sin A-4, porque no cambia ninguna cláusula congelada):
  - `95690c28` no era el `main` vigente;
  - el `main` vigente en la decisión de la A-3 es `819955d61a6da4c811a11fbd11b5dca13f634b7c`;
  - los bytes de `src/RackCad.Application/Expressions/` son idénticos, así que el resultado de INV-22 no cambia.

## 44. Planificación de G3-T2: STOP P-01 (`config.toml` cambió durante la cesión)

### 44.1 Planificación

- **Contrato de T2 emitido** contra la A-3: `artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T212850Z-6064/gate-contract.json`, SHA-256 `2E43EAE22C3254F7CDF31EC9AC0C07B11409E7274E2A6D307DF3B7FB8D889CB7`.
  - Respecto del borrador de G3 cambian `AuthorityRevision` (`ea70e3b3`), `Authorities` (la A-3 tras la A-2) y el texto de INV-22
    (con la A-3 y el caso de 32 caracteres hexadecimales conservado).
  - Se añade un invariante de T2: sin RED nuevo y sin tocar `ChainRedFiles`.
  - Se añaden las paradas C-11 (el GREEN exigiría tocar `ChainRedFiles`, o una expectativa del RED es incompatible) y C-12 (alterar el
    caso de 32 caracteres hexadecimales, orden del Coordinator).
  - Cambian también `Objective`, `ExpectedEvidence`, `CorrectionsAuthorized`, `IssuedBy` e `IssuedUtc`.
  - El alcance, `RequiredTests`, `EligibleCells` y `RoutingEnforcement` son los del borrador.
- **Invocación `R20261002T212850Z-6064`:** Codex CLI, `gpt-6-luna` / `high`, efectivos iguales; cesión 21:30:31Z-21:32:56Z, código 0.
  - Salida válida y las seis colecciones iguales al contrato.
  - A1-A8 en `pass`; A8 en modo de continuación: `ChainBaseSha` `e99621f9`, `ChainRedSha` `637dce7e`, los 8 `ChainRedFiles` y
    `CorrectionOf` null.
  - Celda `anthropic/subagent/claude-sonnet-5-5`, effort `high` (Deep), `ROUTINE_IMPLEMENTATION`.
  - Delegación SHA-256 `858917B63E7EC1B11F8E3EAFD2E60BCE3AA254AB41D5487B690A9852CE93771A`. Sin nc4, porque no es la primera delegación de la cadena.

### 44.2 STOP P-01

| Momento | `TakenUtc` | SHA-256 de `~/.codex/config.toml` |
|---|---|---|
| Salida | 21:30:29Z | `40C27B570B0056BC6D2B5AAF460628922C5AD39A405751E68B9001FFDF15F74F` (el mismo de todas las invocaciones previas del piloto) |
| Entrada | 21:33:17Z | `BB425B03566193815D17C2A9A4A62FDDAF0DC6285C318021BE57C53840DB769F` |
| Después (sin invocación) | — | `155933B32E5178001700B1137A58435075C13AA0179ED9F530791F3F899726D7` (`LastWriteTimeUtc` 21:34:20Z) |

- **Diff de nombres de secciones y claves** (README 3.4): ninguno. Hay 103 nombres en la salida y en la entrada, en el mismo orden; solo
  cambian valores.
- **Causa observada:** la app de escritorio de Codex del Owner se reinició durante la cesión.
  - En la salida, sus procesos tenían como padre el PID 15404 y estaban vivos desde el 2026-10-01 a las 23:08Z.
  - En la entrada hay procesos nuevos con el padre 42692: `ChatGPT.exe` de `OpenAI.Codex_26.930.2377.0`, creado a las 21:31:35Z.
    Son `codex.exe` 42564 y 26088, `codex-computer-use-swift.exe` 38572 y `codex-windows-sandbox-service.exe` 1420.
  - La invocación del Controller ya había empezado (21:30:31Z) y terminó a las 21:32:56Z. `config.toml` volvió a cambiar a las 21:34:20Z
    sin ninguna invocación de la sesión.
  - **Lectura de la sesión:** probable actualización o reinicio de la app, que reescribe valores propios (versión de la app, entorno de
    sus servidores MCP). No es efecto del Controller. Sin embargo, P-01 no distingue la causa.
- **Disposición:** STOP (P-01). La delegación no se abre y no se invoca al Worker. **Ninguna invocación de Codex más hasta la decisión
  del Owner** (README de ejecución delegada 3.4; AUTOMATION_PLAN 16, P-01).
- `attempts` sigue en 2 de 3.

### 44.3 Decisión que se pide al Owner

- **Opción 1 (recomendada):**
  - aceptar el cambio como propio de la app del Owner, con los nombres de claves sin cambios;
  - fijar como línea base el hash vigente al reanudar;
  - repetir la planificación de T2 con el mismo contrato y un `RunId` nuevo, sin consumir `attempts`;
  - mantener la app de Codex sin reinicios ni actualizaciones mientras dure G3-T2.
- **Opción 2:** igual que la 1, pero aceptar la delegación ya planificada (A1-A8 en `pass`) sin repetirla.
- **Opción 3:** otra instrucción del Owner, por ejemplo revisar los valores cambiados antes de reanudar.

Copias en `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/R20261002T212850Z-6064/`.

## 45. Decisión del Owner sobre el STOP P-01 y nueva línea base de `config.toml`

- **Decisión del Owner**, pegada en el chat el 2026-10-02. Resumen en las [decisiones](../decisions/I-63.md) §2.
  - Causa confirmada: la app Codex se actualizó y se reinició durante la planificación.
  - Se aprueba el cambio y se repite la planificación de G3-T2 con el mismo contrato y un `RunId` nuevo, sin consumir `attempts`.
  - No se reutiliza la delegación de `R20261002T212850Z-6064`. Por eso la opción 2 de §44.3 queda descartada; además ya no era viable,
    porque el commit de §44 movió HEAD fuera de su `BaseSha` `f5c4a5a2`.
- **Nueva línea base de `~/.codex/config.toml`:**
  - `155933B32E5178001700B1137A58435075C13AA0179ED9F530791F3F899726D7`, tomada a las 2026-10-02T21:42Z (`LastWriteTimeUtc` 21:34:25Z). Es el mismo valor observado tras la entrada de §44.
  - Procesos de la app: `ChatGPT.exe` de `OpenAI.Codex_26.930.2377.0`, el primero creado a las 21:31:35Z.
- **Regla desde aquí:**
  - cada cesión compara su hash de salida con esta línea base y su hash de entrada con el de salida;
  - cualquier diferencia es STOP P-01 y detiene la cadena;
  - esta decisión no autoriza ningún cambio posterior.

## 46. G3-T2 (GREEN-BUILTINS) bajo I-61: `EXECUTION_VERIFIED` y revisión de arquitectura CONFORMING

### 46.1 Invocaciones

| `RunId` | Fase | Participante (solicitado = efectivo) | Resultado |
|---|---|---|---|
| `R20261002T214309Z-19d7` | PLANNING (repetición ordenada por el Owner, §45) | Codex CLI, `gpt-6-luna`, `high` | Mismo contrato (`2E43EAE2…`); seis colecciones iguales; A1-A8 en `pass` (A8 en modo de continuación); `config.toml` = línea base en salida y entrada |
| `R20261002T214618Z-be6d` | WORK | Subagente, `claude-sonnet-5-5`, `high` | GREEN `eb58a476` sobre `71205235`; sin RED nuevo (`RedSha` null); `IMPLEMENTATION_COMPLETE` (798 s, 47 llamadas) |
| `R20261002T220419Z-26c9` | VERIFICATION (primera de la fase de T2) | Codex CLI, `gpt-6-luna`, `high` | **`EXECUTION_VERIFIED`**, `VerifiedSha` `eb58a4768d5a68892cce8d85b527f2a1ad1bdc64`, 14 de 14 en `pass` (`Ci` y `Tests` con `RedPart` `not_applicable`, citando la corrida del `ChainRedSha`) |
| `R20261002T220928Z-5706` | CONTROL nc1 | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_BLOCKED/STOP`, `Identity`: oráculo cumplido |
| `R20261002T220929Z-1c05` | CONTROL nc2 | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_BLOCKED/STOP`, `Scope` con [C-05]: oráculo cumplido. En G2 no discriminó (DEV-G2-01) |
| `R20261002T220930Z-2504` | CONTROL nc3 | Codex CLI, `gpt-6-luna`, `high` | `EXECUTION_REWORK_REQUIRED/REWORK`, `FreeText`: oráculo cumplido |

- **Binario del Controller:** la ruta verificada del piloto, `bin/a51e250fa15c740a` (`codex-cli 0.159.2`). La actualización de la app añadió
  `bin/be3fd7e5c1969ff6` (`0.159.0-alpha.12.1`), que no se usó.
- **`config.toml`:** la salida de cada cesión se comparó con la línea base del Owner `155933B3…` antes de ceder, y la entrada con la salida.
  No hubo cambios: P-01 no se volvió a disparar.
- **Puerta de PID:** sin procesos marcados vivos. Los dos `bash.exe` efímeros del host que se marcaron ya habían terminado al revalidar.

### 46.2 Entrega y CI

- **GREEN `eb58a4768d5a68892cce8d85b527f2a1ad1bdc64`:** 8 archivos de producción.
  - `src/RackCad.Application/Expressions/`: `SymbolId.cs`, `SymbolTable.cs`, `ExpressionBinder.cs`, `ExpressionFormatter.cs`,
    `DependencyGraph.cs` y `RegistryEvaluation.cs`.
  - `src/RackCad.Application/ComputedParameters/RackComputedExpressionContext.cs` (nuevo).
  - `src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs`.
  - Lexer, parser, evaluador, límites y catálogo de diagnósticos sin cambios.
  - Ningún archivo de `ChainRedFiles`, de las suites de INV-28 ni de las guardas.
- **Corrida `push` 37069993992 del SHA exacto: `success`, cuatro jobs** (incluido «Build Plugin without AutoCAD»).
  - Core 12566/12566; filtro `ComputedParametersSymbols` 89/89.
  - UI 1637 superadas y 17 omitidas por `Skip`.
- **INV-22 tras el GREEN** (observado por el Worker): `Rack.#{zzz}` → un `InvalidQualifier` (11) en 5+6, sin árbol. El caso de 32 caracteres
  hexadecimales conserva su `UnexpectedToken` (3) en 5+35: C-12 no se activa.
- **Comprobación mecánica independiente de `Scope`** (DEV-G2-01), antes de aceptar el resultado: **PASS**.
  - `git diff --name-only 71205235..eb58a476` da 8 archivos.
  - Todos están dentro de `AllowedWriteScope`; ninguno en `ForbiddenWriteScope` ni en `ChainRedFiles`.
  - Detalle en `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/R20261002T214618Z-be6d/scope-check.json`.
- **Limitaciones declaradas por el Worker:**
  - `RackComputedExpressionContext.Create` es interno;
  - los nombres de miembro `Frentes` y `FrentesVacios` se declaran en el contexto.

### 46.3 Revisión de arquitectura

- **Revisor:** el mismo Architect de R1-R3, en la sesión «I-63 Architect Review R1» (`local_d5787742…`).
  - Modo SEPARATE SESSION y de solo lectura.
  - Encargo enviado por SendMessage (`35ba09b6`); se custodia en `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/architect-review/request.txt`.
- **Veredicto: CONFORMING**, sin REQUIRED y con 5 OPCIONALES (O-G3-1..5).
  - Está ligado al SHA `eb58a4768d5a68892cce8d85b527f2a1ad1bdc64`, al diff `ea70e3b3..eb58a476 -- src/` (8 archivos) y al Freeze V3 con A-1, A-2 y A-3.
  - Todos los puntos del foco son conformes: D-16.1-7, R4/INV-27, D-18, D-19, `projectVariable`, INV-28, sin segundo motor, D-17 (4-6),
    INV-22 (A-3), ADR-0043 D1/D9/D24 y AGENTS.
  - Considera aceptables las dos limitaciones del Worker y responde a la pregunta abierta de T1: los nombres de API son aceptables; el
    mensaje con «Computed» es suficiente; es preferible que la tabla de D9 sea privada.
- **Texto literal:** `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/architect-review/verdict.txt`. Es igual byte a byte al `SendMessage` de la transcripción del Architect
  (`fdd2a979-cce9-4391-83aa-e7dfe5effcc4.jsonl`).
- **Opcionales**, que no bloquean y pueden ir en G4 o en la iniciativa del consumidor:
  - O-G3-1: trasladar `Frentes` y `FrentesVacios` al catálogo (`RackMetricIds`), para que D-03 tenga un solo sitio;
  - O-G3-2: error de programación ante homónimos `rack`;
  - O-G3-3: quitar la llamada doble a `RejectComputedEntries`;
  - O-G3-4: la procedencia del registro (D-17.4) y la variable de proyecto fallida, como obligación del primer consumidor junto a la
    apertura de `Create`;
  - O-G3-5: aserción de ámbito `Rack` para las entradas `rack`.

### 46.4 Observación de la sesión

- El comentario de `ExpressionFormatter.cs` sobre `Rack.#{token}` dice que el parser lo lee como «namespace syntax followed by a loose
  qualifier». Es el mecanismo `[RECONSTRUCTED]` de D-16.6.
- Para el oráculo de INV-22, la A-3 fijó que el rechazo ocurre en el lexer (`InvalidQualifier`). Ese mecanismo solo aplica al caso de 32
  caracteres hexadecimales (A-3 §4).
- Es una imprecisión de comentario, sin efecto en el comportamiento, que las pruebas protegidas fijan. Se propone corregirla junto a los
  opcionales.

### 46.5 Contadores y custodia

- `attempts` = 2 de 3, sin cambio: no hubo corrección.
- Planificaciones de G3-T2: 2. La primera se detuvo por P-01; la segunda la ordenó el Owner y no consume.
- Verificaciones de T2: 1, sin reejecuciones.
- Controles: nc1-nc3 cumplidos, una vez cada uno sobre la verificación `EXECUTION_VERIFIED`.
- La delegación de G3-T2 queda cerrada por una verificación válida registrada.
- Copias en `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/<RunId>/`, `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS-ncN/<RunId>/` y `docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/architect-review/`.

**Siguiente:** el Coordinator juzga G3 con esta evidencia. G4 no está autorizado.

## 47. G3 PASS

- **Veredicto del Coordinator**, pegado por el usuario en el chat el 2026-10-02: **G3 — Rack built-ins / Expression Engine: PASS**.
  Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Base del juicio:**
  - `VerifiedSha` `eb58a4768d5a68892cce8d85b527f2a1ad1bdc64`; verificación `R20261002T220419Z-26c9`, `EXECUTION_VERIFIED`, 14/14.
  - CI `37069993992` 4/4: Core 12566/12566 y foco de G3 89/89.
  - `Scope` mecánico PASS; nc1, nc2 y nc3 PASS.
  - Architect CONFORMING con 0 REQUIRED.
  - Custodia `138bc3d4`, CI `37072855039` 4/4.
- **A-3 vigente:** `Rack.#{zzz}` → `InvalidQualifier`, código 11, span 5+6, sin árbol sintáctico ni enlazado. El caso hexadecimal de 32
  caracteres queda fuera.
- **Opcionales O-G3-1..5:** ninguno bloquea y no se autoriza corregirlos ahora.
  - O-G3-1: diferido; solo se evalúa en G4 si `ProjectSummary` lo necesita.
  - O-G3-2: diferido.
  - O-G3-3: diferido.
  - O-G3-4: deuda del primer consumidor de `RackComputedExpressionContext`.
  - O-G3-5: diferido.
- **DEBT-I63-G3-01:** comentario obsoleto de `ExpressionFormatter` para `Rack.#{token}` frente a la A-3. Se resuelve antes de READY, sin
  alterar comportamiento.
- **Contadores:** `attempts` 2/3 (`AttemptsRemaining` 1). DEBT-I63-G2-01 y DEBT-I63-G3-01 quedan abiertas hasta READY.
- **Siguiente:** `current_phase` = G4.
  - La sesión prepara el contrato de G4 y vuelve solo para la autorización formal.
  - La implementación de G4 no está autorizada.

## 48. G4: borrador del contrato de gate (sin emitir)

### 48.1 Cierre de G3

Commit `49ed359e`: G3 PASS, disposición de O-G3-1..5 y DEBT-I63-G3-01; `current_phase` = G4. Corrida `push` 37081933485 `success` (cuatro jobs).

### 48.2 Borrador

- **Archivo:** `artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/draft/R20261003T002705Z-0618/gate-contract.json`.
  - SHA-256 `584FA8AEF955A5FC1897B1F983AB2524C4B07F22A8B8D621724B2A2AC553AE72`; válido contra el esquema; sin conflicto de alcance.
  - `IssuedBy` = borrador; `CorrectionsAuthorized` = false.
  - `TaskId` `G4-PROJECT-SUMMARY`.
- **Alcance:**
  - Permitido: `src/RackCad.Application/ComputedParameters/` y `tests/RackCad.Tests/ComputedParameters/`.
  - Prohibido: Expressions, Persistence, ProjectVariables, Systems, Bom, Catalogs, Units, Domain, Plugin, UI, `tests/RackCad.UI.Tests/`, los
    `.csproj` de Application y de pruebas, las suites de INV-28, `ExpressionCoreGuardTests`, `ExpressionDiagnosticCatalogTests`,
    `ExpressionSymbolModelTests`, `NamespaceFolderGuardTests`, `PushBackBomCommandGuardTests` (DEBT-I63-G2-01), `docs/` y la configuración
    del repositorio.
  - Ninguna entrada prohibida cubre una permitida.
- **Invariantes** (12):
  - parte de G4 de INV-11 (A-2.2) e INV-32 (A-2.1);
  - INV-29, INV-30, INV-31 e INV-34 (resumen);
  - D-20, D-21, D-24 y D-25;
  - conservación de G1, G2 y G3 (sin modificar sus pruebas);
  - sin consumidores, sin persistencia y sin abrir `Create`.
- **Pruebas:** filtro `FullyQualifiedName~RackCad.Tests.ComputedParametersSummary`, `MinSelected` 10 y `ExpectRed` true.
- **Paradas:** las 19 de base (S-/P-) y C-01..C-09 de G4:
  - C-07: la provenance cambiaría la igualdad de G1-G3;
  - C-08: persistir o presentar, o abrir `Create`;
  - C-09: un tiempo usado como oráculo, o N = 1000 que no cabe en la CI.
- **`ExpectedEvidence`:**
  - RED, GREEN y caracterización de D-24 por contadores;
  - comprobación mecánica independiente de `Scope`, ampliada: además de alcance y prohibidos, ningún archivo de prueba existente en
    `BaseSha` se modifica ni se borra (solo archivos nuevos en `tests/`);
  - CI 4/4.
- **Celdas y emisor:** celdas y `RoutingEnforcement` de G2.
- **`AuthorityRevision` propuesta:** el commit que registre la autorización de G4, como en G3 (`e99621f9`). El borrador lleva `49ed359e`
  solo como marcador.

### 48.3 Hechos medidos que condicionan G4

- **Provenance (D-21):**
  - hoy `MetricValue` no tiene provenance y su igualdad compara estado, valor y razón (`RackMetricModels.cs`). Las pruebas de G1 y G2
    comparan `MetricValue` por igualdad. Si la provenance entrara en `Equals`, esas pruebas cambiarían: la provenance debe quedar fuera de
    la igualdad o en una estructura paralela (C-07);
  - `RackComputedEvaluation` (G3) no conserva el `BoundExpression` ni los `SymbolId` leídos que pide D-21.
- **`ProjectPopulationAggregator`:** ya es la función pura de D-12 que «en G4, `ProjectSummary` `Full`» alimenta con las métricas por rack.
- **INV-29 (b), los `.csproj`:**
  - `RackCad.Application.csproj` solo referencia Domain;
  - `RackCad.UI.csproj` tiene `UseWPF`;
  - `RackCad.Plugin.csproj` tiene `UseWPF` y AutoCAD por dos vías condicionales: `PackageReference AutoCAD.NET` (CI) y
    `Reference … HintPath AcCoreMgd/AcDbMgd/AcMgd` (local).
  - El detector debe reconocer las dos vías.
  - Ya existe una guarda parcial de texto (`RegistryCommitAccreditationTests.cs:228-232`), sin cierre recursivo.
- **O-G3-1:** `ProjectSummary` indexa `Racks.Metrics` por `MetricId`, no por nombre de miembro. No hace falta trasladar `Frentes` y
  `FrentesVacios` al catálogo, así que el borrador no lo incluye.

### 48.4 Preguntas para la autorización

- **Q-G4-01 (D-21 en `RackComputedExpressionContext`):** D-21 pide que el contexto conserve el `BoundExpression` y los `SymbolId` leídos.
  Eso exige un cambio aditivo en `RackComputedExpressionContext.cs` (G3), dentro del alcance.
  - Propuesta: permitirlo solo de forma aditiva, sin cambiar el comportamiento ni las pruebas de G3.
  - Alternativa: diferirlo junto a O-G3-4, como obligación del primer consumidor.
- **Q-G4-02 (pruebas existentes):** el prefijo permitido de pruebas cubre las de G1-G3.
  - Propuesta: protegerlas con el invariante, la parada C-01 y la comprobación mecánica ampliada (solo archivos nuevos en `tests/`), sin
    entradas prohibidas dentro del prefijo permitido.
- **Q-G4-03 (D-24 con N = 1000):** la caracterización corre en la suite Core de la CI, con contadores como oráculo y tiempos solo
  registrados.
  - Propuesta: si N = 1000 con tres vistas no cabe en un tiempo razonable de CI, STOP C-09 y vuelta, sin rebajar N por cuenta propia.
- **Q-G4-04 (RED de INV-29 (b)):** el objetivo (Application → ninguna familia) ya pasa hoy. Su RED observable es el control positivo con
  la misma función.
  - Precedentes: INV-09 en G1 e INV-35 en G2.
  - INV-29 (a), INV-30, INV-31, INV-34 (resumen) y las partes de G4 de INV-11 e INV-32 tienen RED por «API inexistente».
  - Se pide confirmar.
- **Q-G4-05 (ejecución):**
  - Propuesta: una sola delegación con RED y GREEN, como G1 y G2 (no escalonada como G3), con `ROUTINE_IMPLEMENTATION` +
    `CHARACTERIZATION` según §21 y `attempts` 2/3 (`AttemptsRemaining` 1).
  - ¿Revisión de arquitectura antes del cierre de G4? La §21 solo la exige en G3.

## 49. Autorización de G4

- **Orden del Coordinator**, pegada por el usuario en el chat el 2026-10-02, tras verificar el cierre de G3 y el borrador de §48. Resumen en
  las [decisiones](../decisions/I-63.md) §2.
- **Respuestas:**
  - **Q-G4-01:** D-21 en `RackComputedExpressionContext` se hace en G4, como cambio aditivo (`BoundExpression` evaluado y `SymbolId`
    leídos). O-G3-4 sigue diferido.
  - **Q-G4-02:** condición mecánica adicional: las pruebas existentes de G1-G3 sin modificar, borrar ni renombrar; G4 solo añade. Lo
    contrario es STOP.
  - **Q-G4-03:** N = 1000 obligatorio; si no cabe en la CI, STOP C-09.
  - **Q-G4-04:** confirmado el RED de INV-29 (b) por control positivo, con la A-1.1.
  - **Q-G4-05:** una cadena RED → GREEN; Architect no exigido por defecto.
- **Provenance:** fuera de `MetricValue.Equals`. Si no es posible, STOP C-07.
- **`AuthorityRevision`:** el SHA de este commit de autorización.
- **Sin mezclar en G4:** DEBT-I63-G2-01, DEBT-I63-G3-01 ni O-G3-1..5.
- **Contadores:** `attempts` 2/3 (`AttemptsRemaining` 1).

## 50. G4-PROJECT-SUMMARY bajo I-61: STOP S-03/C-10 en la verificación

### 50.1 Invocaciones

| `RunId` | Fase | Participante (solicitado = efectivo) | Resultado |
|---|---|---|---|
| `R20261003T005345Z-66f3` | PLANNING | Codex CLI, `gpt-6-luna`, `high` | Seis colecciones iguales al contrato; A1-A8 en `pass`; celda `claude-sonnet-5-5` `high` (Deep), `ROUTINE_IMPLEMENTATION` |
| `R20261003T005657Z-b75c` | CONTROL nc4 | — (sin invocar) | A3 en `fail` y las demás iguales a la real: oráculo cumplido |
| `R20261003T005720Z-2b14` | WORK | Subagente, `claude-sonnet-5-5`, `high` | RED `55d6b274`, GREEN `4f45f446`; `IMPLEMENTATION_COMPLETE` (1509 s, 77 llamadas) |
| `R20261003T012440Z-ef71` | VERIFICATION | Codex CLI, `gpt-6-luna`, `high` | **`EXECUTION_BLOCKED/STOP`, `Contract`, [S-03, C-10]**; las otras 13 en `pass` |

- **Contrato:** emitido contra la autorización (`AuthorityRevision` `527e4b91`, CI `37083561563` 4/4), SHA-256 `923D2B2CE69D3A65F124E307C298D0BC67C4845337E688F1943EEFBC9F523950`.
- **`config.toml`:** igual a la línea base del Owner (`155933B3`) en todas las cesiones.
- **Puerta de PID:** sin procesos marcados vivos.
- **Comprobación de entrada de la verificación:** la primera, a las 01:30Z, falló por un corte de red (`git fetch`: «Could not resolve
  host: github.com») y no escribió `entry.json`. Se reintentó a petición del usuario a las 02:23Z y salió limpia. La cesión del
  Controller no se vio afectada.

### 50.2 Entrega, CI y comprobaciones mecánicas

- **RED `55d6b274`:** corrida 37085428743 en `failure`. Fallan 26 pruebas del filtro (29 seleccionadas) y ninguna fuera de él.
  - Pasan 3 en el RED: las 2 de INV-29 (b), por su control positivo (patrón autorizado), y `D24_UnaSolaPeticionPorRack` (G1 sin
    cambio).
- **GREEN `4f45f446`:** corrida 37085650331, **`success`, cuatro jobs**. Core 12595/12595; filtro 29/29; UI 1637 superadas y 17 omitidas
  por `Skip`.
- **Comprobaciones mecánicas de la sesión** (`docs/automation/evidence/I-63-pilot/G4-PROJECT-SUMMARY/R20261003T005720Z-2b14/scope-check.json`):
  - **`Scope` PASS:** 12 archivos (7 de producción en `ComputedParameters/` y 5 pruebas nuevas), ninguno prohibido.
  - **Pruebas existentes sin cambios PASS:** `git diff --name-status` da 5 altas y 0 modificados, borrados o renombrados entre las 12
    existentes. El GREEN no toca las pruebas del RED.
- **Contadores observados** (oráculo D-24; N con tres vistas):
  - `Population`: 3N lecturas, 1 evaluación de población y 0 resoluciones.
  - `Full`: 3N lecturas, 1 evaluación y N resoluciones, nunca 3N.
  - Para N = 1000: 3000/1/0 y 3000/1/1000.
  - INV-32: `RackMetricRequest` 0; `ProjectPopulation` > 0; `Full` 1.
- **Tiempos registrados** (no son oráculo): N = 1000, `Population` ≈ 406 ms y `Full` ≈ 394 ms. N = 1000 cabe sin rebajar N: C-09 no se
  activa.
- **Provenance:**
  - Hay una `MetricProvenance` por métrica `(Rack, *)` y una `AggregateProvenance` por agregado. La tabla cerrada `MetricAuthorityIds.All`
    tiene 4 identificadores.
  - `MetricValue.Equals` no cambia; se comprueba por reflexión.
  - `RackComputedEvaluation` gana `Expression` y `ReadSymbols` (aditivo; `Evaluate` sin cambios; las 89 pruebas de G3 en verde).

### 50.3 STOP de la verificación (motivo del Controller)

- El GREEN añade el getter público **`RackSummary.RepresentativeDefinitionId`**, que consume la prueba de INV-11.
- La Proposal V3 §14 (D-20) enumera para `RackSummary` solo RackId, KindToken, DisplayName, Membership y Metrics.
- La entrega ya lo planteaba como pregunta abierta.
- El Controller lo trata como extensión pública del modelo congelado, que requiere resolución (S-03 y C-10).

### 50.4 Lectura de la sesión (para el juicio del Coordinator)

- **Hechos:**
  - `PopulationRack.RepresentativeDefinitionId` (`ProjectPopulation.cs:147`) existe desde G2 y se aceptó en G2 PASS. `RackSummary` repite
    ese mismo dato, de solo lectura.
  - D-21 ya congela «la `DefinitionKey` del representante» en la provenance de cada métrica (`MetricProvenance.RepresentativeDefinitionKey`).
  - La orden de G4 define INV-11 como `Full(original)` == `Full(invertido)` «incluyendo […] representative».
- **Lectura:** el campo es aditivo, no persiste nada, no cambia comportamiento ni la semántica de las métricas, y expone un dato que el
  modelo congelado ya contiene. Por eso la sesión lo considera no material (M-01..M-08 no activados). Pero la decisión normativa es del
  Coordinator.
- **Opciones:**
  - **1 (recomendada):** aceptar el miembro como precisión no material de D-20 y repetir la verificación con un `RunId` nuevo citando la
    decisión, sin cambiar el trabajo y sin consumir `attempts`. Si el Coordinator lo prefiere, puede fijarlo con una A-4 solo del
    Coordinator, lo que exigiría reemitir el contrato y replanificar.
  - **2:** exigir que se retire y se observe el representante por la provenance. Es una corrección de trabajo: `attempts` 2 → 3 y, después,
    STOP S-11 ante cualquier otra.
- **Otros hallazgos para la revisión del diff:**
  - H-G4-01: desviación declarada. El Worker validó en local, sin commit, una implementación provisional antes del RED; restauró el
    esqueleto con `git checkout --` y publicó RED y GREEN en ese orden.
  - H-G4-02: en `Full`, un Push Back ilegible **colocado** registra 1 lectura del lector D-26, que es la E5 de la pertenencia (D-10); las
    métricas no añaden ninguna. El oráculo de 0 de INV-34 se cumple literalmente en el caso sin colocar y en la Cabecera.
  - H-G4-03: refactor sin cambio de comportamiento para que `RackMetricRequest` y `RackSummary.Metrics` usen la misma tabla D-28.
    `RackMetricOrchestrator` es nuevo; `RackMetricRequest.cs` y `ProjectPopulation.cs` cambian, con las pruebas de G1 y G2 intactas y en
    verde.
  - H-G4-04: limitaciones declaradas.
    - `MetricProvenance.EffectiveOutcome` solo se informa con `EffectiveFailed`.
    - `AuthorityId` se informa para métricas `Supported` aunque el valor sea `Unavailable`.
    - En `Full`, los Selectivos excluidos o `NotPlaced` también reciben sus métricas D-28 (R-14 de la Proposal).
    - `ProjectSummary.Equals` compara su texto canónico `Describe()`.

### 50.5 Contadores

- `attempts` 2/3, sin cambio.
- Planificaciones de G4: 1. Verificaciones: 1, cerrada en STOP.
- nc4 cumplido. nc1-nc3 no se ejecutan, porque no hay `EXECUTION_VERIFIED`.
- Copias en `docs/automation/evidence/I-63-pilot/G4-PROJECT-SUMMARY/<RunId>/` y `docs/automation/evidence/I-63-pilot/G4-PROJECT-SUMMARY-nc4/<RunId>/`.

## 51. G4: STOP resuelto, P-01 del Owner y `EXECUTION_VERIFIED`

### 51.1 Resoluciones

- **STOP S-03/C-10 de §50:** el Coordinator acepta `RackSummary.RepresentativeDefinitionId` como precisión no material de D-20.
  - No hace falta A-4 ni cambio de trabajo.
  - La sesión comprobó la condición en el código: el valor es `member.RepresentativeDefinitionId` del `PopulationRack`, es decir, el
    representante de E4 de la población (`ProjectSummary.cs:146-153`), sin otra ruta de cálculo.
- **Excepción de `Identity`:** el commit de custodia `7bd5a743`, solo de docs, quedó encima del GREEN.
  - Comprobación mecánica de la sesión: PASS.
  - `4f45f446` es ancestro de `7bd5a743`; el rango tiene 20 rutas, todas en `docs/`; no hay rutas funcionales.
  - Los árboles son idénticos: `src` `5d3bba74…` y `tests` `28bc25cb…`.
  - CI funcional: `37085650331`.
- **Lección de la sesión:** tras un STOP que puede resolverse reverificando el mismo SHA, el commit de custodia espera a la decisión, como
  en 16.7.

### 51.2 P-01 y binario del Controller (Owner)

- **Intento `R20261003T023614Z-e734`:** abortado antes de ceder.
  - `config.toml` = `091540ED…`, distinto de la línea base `155933B3…`.
  - Además, el binario verificado `bin/a51e250fa15c740a` (`codex-cli 0.159.2`) había desaparecido.
  - Causa: actualización de la app Codex a `26.930.3930.0`, el 2026-10-04 a las 21:38Z.
- **Decisión del Owner:**
  - nueva línea base `091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66`;
  - binario nuevo verificado antes de su uso:

```text
path: C:\Users\alejandra-mendoza\AppData\Local\OpenAI\Codex\bin\8aaf1547b825b104\codex.exe
sha256: 37762753B554982EEF1C109303D1BE652B6397F1479E844794353A85650199C6
size: 326872368
mtime: 2026-10-03 12:48:16.614091200 -0600
version: codex-cli 0.160.0
checked_utc: 2026-10-05T01:18:02Z
config_toml_sha256: 091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66
```

- **Primera invocación con el binario nuevo (`R20261005T011832Z-cc42`):**
  - modelo y effort efectivos = solicitados (`gpt-6-luna`, `high`);
  - `cli_version` 0.160.0;
  - `config.toml` igual a la línea base nueva en salida y entrada.

### 51.3 Reverificación y controles

| `RunId` | Fase | Resultado |
|---|---|---|
| `R20261005T011832Z-cc42` | VERIFICATION (tras STOP resuelto) | **`EXECUTION_VERIFIED`**, `VerifiedSha` `4f45f4464f5d434f7fa478fea1117d82283ee997`, 14 de 14 en `pass` (`Identity` con la excepción comprobada por el Controller con `git`) |
| `R20261005T012438Z-27b5` | CONTROL nc1 | `EXECUTION_VERIFIED/NONE`, `NONE` [] |
| `R20261005T012440Z-1e98` | CONTROL nc2 | `EXECUTION_VERIFIED/NONE`, `NONE` [] |
| `R20261005T012441Z-7603` | CONTROL nc3 | `EXECUTION_REWORK_REQUIRED/REWORK`, `FreeText` [] |

- **nc3 CUMPLIDO:** un término de gate en `WorkCompleted` da `FreeText` en `fail` y REWORK. Las demás comprobaciones quedan igual que en la
  real.
- **nc1 NO CUMPLIDO:** la entrega mutada lleva `CurrentSha` `0123…` (inexistente). El oráculo era `Identity` en `fail` con STOP.
  - El Controller devolvió `EXECUTION_VERIFIED` con `VerifiedSha` `4f45f446`.
  - Su `Evidence` de `Handoff` afirma `CurrentSha` = `4f45f446`, contra el valor de la entrega.
- **nc2 NO CUMPLIDO:** la delegación mutada excluye `ProjectPopulation.cs` de `AllowedWriteScope`. El oráculo era `Scope` en `fail` con STOP.
  - El Controller devolvió `EXECUTION_VERIFIED` y afirma que las 12 rutas están permitidas.
  - Es el mismo modo de fallo que DEV-G2-01.
- **Comprobaciones mecánicas de la sesión sobre `527e4b91..4f45f446`:** `Scope` PASS y pruebas existentes sin cambios PASS.
  - Con la delegación de nc2, la misma comprobación da `Scope` en `fail`.
  - El `CurrentSha` real de la entrega (`4f45f446`) existe, es el GREEN y es el `VerifiedSha` de la verificación real.

### 51.4 Lectura de la sesión sobre nc1 y nc2 (para el juicio del Coordinator)

- **nc1:**
  - El prompt de la reverificación nombra el GREEN `4f45f446` como `CurrentSha` de la entrega, en el párrafo de la excepción de
    `Identity`. Los textos literales del Coordinator, incluidos como entradas, también lo nombran.
  - El Controller tomó ese SHA del prompt en lugar del `CurrentSha` de la entrega.
  - Es un defecto de discriminación introducido por cómo la sesión redactó la excepción: justo lo que el Coordinator pidió evitar («la
    excepción no debe debilitar nc1»).
  - La verificación real no queda afectada en los hechos. Su entrega lleva `CurrentSha` `4f45f446`, y la sesión lo comprobó por su
    cuenta: existe, es el GREEN y su CI es 4/4.
  - Pero el Controller de esta configuración no demuestra que una entrega con otro `CurrentSha` fallaría.
- **nc2:**
  - El Controller juzgó `Scope` contra el alcance del contrato, o contra la nota de la comprobación mecánica de la sesión del registro
    del trabajo, y no contra la delegación mutada.
  - DEV-G2-01 ya lo previó. La comprobación mecánica independiente de la sesión, PASS en la real, es la regla vigente que lo cubre.
- **Reejecución:** no procede (README §10). Las salidas son válidas, así que no son fallos de transporte. Quedan como controles no
  superados para el juicio del Coordinator.
- **Recomendación de la sesión:**
  - Aceptar la verificación real, apoyada en las comprobaciones mecánicas independientes de `Scope` y de la excepción de `Identity`.
  - Registrar nc1/nc2 como deuda del protocolo de verificación:
    - un prompt de verificación no debe nombrar el SHA de la entrega;
    - los hechos de la sesión no deben presentarse al Controller como conclusiones.
  - O, si el Coordinator lo exige, repetir los controles con un prompt sin el SHA del GREEN, mediante una decisión explícita, porque
    README §10 no lo prevé.

### 51.5 Contadores y custodia

- `attempts` = 2 de 3, sin cambio: no hubo corrección de trabajo.
- Verificaciones de G4: 2, más 1 intento abortado antes de ceder.
- Controles: nc4 y nc3 cumplidos; **nc1 y nc2 no superados** (ver 51.4).
- Copias en `docs/automation/evidence/I-63-pilot/G4-PROJECT-SUMMARY/<RunId>/` y `docs/automation/evidence/I-63-pilot/G4-PROJECT-SUMMARY-ncN/<RunId>/`. Llevan CRLF los transitorios
  R20261003T023614Z-e734/identity-exception-check.json.

**Siguiente:** el Coordinator juzga G4 con esta evidencia y revisa el diff `527e4b91..4f45f446` contra Freeze + A-1 + A-2 + A-3.

## 52. G4 PASS: gates funcionales completos

- **Veredicto del Coordinator**, pegado por el usuario en el chat: **G4 — ProjectSummary, provenance y rendimiento: PASS**. Resumen en
  las [decisiones](../decisions/I-63.md) §2.
- **Base del juicio:**
  - `VerifiedSha` `4f45f4464f5d434f7fa478fea1117d82283ee997`; verificación `R20261005T011832Z-cc42`, `EXECUTION_VERIFIED`, 14/14.
  - CI `37085650331` 4/4: Core 12595/12595 y foco de G4 29/29.
  - `Scope` mecánico PASS; pruebas existentes de G1-G3 sin cambios PASS.
  - Custodia `fdd4b651`, CI `37253017255` 4/4.
  - Revisión del Coordinator del diff funcional `527e4b91..4f45f446`.
- **Desviaciones (no bloquean):**
  - DEV-I63-G4-02: nc1 no discriminó, porque el prompt aportó el SHA del GREEN como contexto.
  - DEV-I63-G4-03: nc2 no discriminó; el oráculo mecánico sí.
  - nc3 cumplido.
- **DEBT-I63-PROTOCOL-01** (protocolo/I-61): los prompts de verificación no deben inyectar hechos de la entrega como conclusiones, e
  `Identity` y `Scope` deben salir de campos de entrada parseados. Se documenta en READY.
- **Estado:** G1, G2, G3 y G4 = PASS; gates funcionales completos; `attempts` 2/3.
- **Siguiente:** `current_phase` = READY, aún no declarado.
  - La sesión prepara el plan de READY sin ejecutar cambios y vuelve para la autorización formal.

## 53. Plan de READY (preparado, sin ejecutar)

El cierre documental de G4 es `da4fe5ee` (corrida `push` 37253956168 `success` (cuatro jobs)). Punto de partida:
- G1-G4 en PASS; `attempts` 2/3;
- HEAD = remoto = `da4fe5ee`;
- `origin/main` = `819955d6`, sin avance desde el reclamo.

Nada de lo que sigue se ejecuta sin la autorización formal de READY.

### 53.1 Bloque A — Deudas de pruebas y documentación de I-63

Una sola A-n solo del Coordinator (A-4) de pruebas y documentación, sin cambio funcional. Cubre:

- **DEBT-I63-G2-01:** `tests/RackCad.Tests/PushBackBomCommandGuardTests.cs:98-102`
  (`ThePushBackHandler_ConsumesTheSharedGateAndNotASecondRule`). Exige el literal `RackBomOutputGate.For(system).Reason` en el texto de
  `PushBackKindHandler.cs`, y hoy solo lo cumple un comentario XML.
  - Propuesta: que la guarda lea el código sin comentarios, como `CantileverPluginSourceGuardTests` e INV-33, y que afirme la nueva
    autoridad. Es decir, que el handler delegue en la función de Application (`RackOutputVerdict`) y no componga una segunda regla.
  - La autoridad es INV-33; la guarda vieja queda como redundante o se retira explícitamente, según decida el Coordinator.
  - RED natural: con la guarda leyendo el código sin comentarios, la aserción literal vigente falla. GREEN: la aserción contra la
    delegación.
  - Sin tocar el Plugin.
- **DEBT-I63-G3-01:** comentario de `src/RackCad.Application/Expressions/ExpressionFormatter.cs` sobre `Rack.#{token}`.
  - Hay que reescribirlo con el mecanismo de la A-3: para el oráculo, el lexer rechaza la clave como `InvalidQualifier`.
  - Es solo un comentario: sin cambio de comportamiento, con `ExpressionCoreGuardTests` y las suites de INV-28 en verde.
- **Ejecución propuesta:** una cadena I-61 corta (`READY-DEBTS`), con contrato de archivos exactos: la guarda legacy y
  `ExpressionFormatter.cs` (solo comentario). RED → GREEN, CI 4/4 y comprobación mecánica de `Scope`.
  - Es trabajo nuevo autorizado, no una corrección: no consume `attempts`.
  - Si el Coordinator prefiere que lo haga la sesión directamente, AUTOMATION_PLAN §8-9 lo permite con CI exacta.
- **DEBT-I63-PROTOCOL-01 en los prompts:** las plantillas de verificación de esa cadena no nombrarán el SHA de la entrega ni
  presentarán hechos de la sesión como conclusiones.

### 53.2 Bloque B — Deuda de protocolo de I-61 (DEBT-I63-PROTOCOL-01)

- Es del protocolo, no del producto I-63. La entrada «Agent Execution Protocol» de FOUNDATIONS y los documentos normativos de I-61
  (`docs/automation/agent-execution/README.md`, AUTOMATION_PLAN §16) no se editan desde I-63: un consumidor solo verifica.
- **Propuesta:**
  - registrarla en `docs/ideas-futuras.md` y en el HANDOFF del cierre, con DEV-G2-01 y DEV-I63-G4-02/03 como evidencia;
  - tres reglas propuestas para el protocolo:
    - el prompt no inyecta el `CurrentSha` esperado;
    - `Scope` sale de la delegación verificada;
    - `Identity` y `Scope` salen de campos parseados;
  - el Coordinator decide si se avisa a la iniciativa dueña del protocolo (I-61 integrada; I-62 activa sobre el protocolo).

### 53.3 Bloque C — Requisitos READY congelados

- **Contrato de la iniciativa (READY-02):** la cabecera aún tiene `materiality: [UNKNOWN]` y `consumes`, `extends` e `introduces` en
  `[UNKNOWN]`.
  - La materialidad la fija la Proposal V3 §3: M-01 y M-04..M-08 activados; M-02 y M-03 no activados.
  - `consumes`, `extends` e `introduces` no están escritos en el Freeze.
  - Propuesta, para decisión del Coordinator sin inventar valores:
    - `consumes`: el motor de expresiones de I-49 (ADR-0043) y Project Variables;
    - `extends`: el núcleo del motor con el namespace `rack` (M-05);
    - `introduces`: Computed Parameters / Project Summary (M-01, M-06, M-07).
- **ADR (M-08; §23 y anexo A):** sucesor parcial de ADR-0043, con número asignado al integrar.
  - La sesión redacta el ADR desde el anexo A, actualizado con lo integrado (A-1..A-3, D-16..D-21 tal como quedaron).
  - El Architect de R1-R3 lo revisa. Queda aceptado con Architect + Coordinator.
  - El número se fija en la integración para no colisionar con otras iniciativas activas.
- **FOUNDATIONS (LIFECYCLE §4.1):** quien introduce o extiende redacta la entrada factual antes de READY-04.
  - Propuesta: una entrada nueva, «Computed Parameters & Project Summary», con el esquema de entrada completo.
    - Autoridad: `RackMetricRequest`, `ProjectPopulation`, `ProjectSummary` y la tabla D-28.
    - Persistencia: ninguna.
    - Contrato de mutación: solo lectura.
    - Punto de extensión: providers por kind (D-08) y `RackComputedExpressionContext.Create` interno, con O-G3-4 para el primer
      consumidor.
    - Pruebas protectoras: `ComputedParameters*`.
    - Limitaciones conocidas: O-G3-1..5, R-14 y la ausencia de consumidores productivos.
  - Estado `STABLE` solo con el Freeze integrado. Se propone también revisar si la entrada «Project Variables» necesita nota de la
    extensión del núcleo; I-63 solo la verifica.
- **Owner Validation (READY-08):** NOT APPLICABLE, decidido en el Freeze (§22): sin UI, comandos ni cambios de dibujo.
- **Documentos de cierre (WORKFLOW §5 y §8):**
  - HANDOFF §8-12;
  - `docs/ideas-futuras.md`: O-G3-1..5, O-G3-4 como obligación del primer consumidor, R-14 y DEBT-I63-PROTOCOL-01;
  - ROADMAP: archivo caliente, con acuse de ventana de los escritores activos (I-52, I-62, I-64), según la estrategia de coordinación
    del contrato.
- **Secuencia READY-01..09 (LIFECYCLE §8), en orden:**
  - 01: alcance congelado completo; los diferimientos O-G3-1..5 los decidió el Coordinator.
  - 02: gates cerrados; Bloque A, ADR, FOUNDATIONS y documentos de producto versionados.
  - 03: sin REQUIRED ni decisión pendiente.
  - 04: fetch, preflight y, si `main` avanzó, rebase final (WORKFLOW).
  - 05: suites Core y UI en local, builds de UI y Plugin, CI exact-SHA 4/4.
  - 06: conformidad completa CONFORMING de Architect + Coordinator sobre ese SHA.
  - 07: árbol limpio.
  - 08: OV NOT APPLICABLE.
  - 09: identidad del Freeze (`f61d0aca`, blob `4d5dedce`) y A-1..A-n append-only.
  - Solo después, `FINAL_CANDIDATE_SHA` y la sesión de integración (WORKFLOW §4.5), que no forman parte de esta autorización.

### 53.4 Preguntas para la autorización de READY

- **Q-R-01:** ¿A-4 solo del Coordinator para DEBT-I63-G2-01 y DEBT-I63-G3-01? ¿Ejecutada como cadena I-61 corta o por la sesión
  directamente?
  - Para la guarda legacy: ¿reapuntarla a la delegación o retirarla en favor de INV-33?
- **Q-R-02:** ¿Basta registrar DEBT-I63-PROTOCOL-01 en `ideas-futuras` y en el HANDOFF? ¿Se avisa a la iniciativa del protocolo?
- **Q-R-03:** valores de `consumes`, `extends` e `introduces` del contrato; y la materialidad de la §3.
- **Q-R-04:** ADR: ¿redacción por la sesión, revisión del mismo Architect y número al integrar?
- **Q-R-05:** FOUNDATIONS: ¿una entrada nueva «Computed Parameters & Project Summary»?
- **Q-R-06:** orden de ejecución propuesto:
  - Bloque A;
  - después ADR, FOUNDATIONS y documentos;
  - READY-04 (rebase si hace falta);
  - READY-05;
  - conformidad READY-06 del Architect + Coordinator;
  - READY-07..09;
  - vuelta para el Candidato y la integración.

## 54. Autorización de READY y A-4

- **Orden del Coordinator**, pegada en el chat. Resumen en las [decisiones](../decisions/I-63.md) §2.
- **Estado:** READY = AUTHORIZED; Candidato e integración no autorizados.
- **A-4:** `docs/initiatives/I-63-proposal-v3-amendment-a4-ready-debts.md`, solo del Coordinator.
  - Cubre DEBT-I63-G2-01 (guarda legacy reapuntada) y DEBT-I63-G3-01 (comentario).
  - Alcance de dos archivos exactos; la ejecuta la sesión directamente.
- **Secuencia autorizada:** A → B → C → D → E → F (READY-04) → G (READY-05) → H (READY-06) → I (READY-07..09) → J.
  - Único retorno obligatorio intermedio: la aceptación explícita del ADR por el Owner.
- **HANDOFF y ROADMAP:** no se tocan en READY.

## 55. READY bloques A, B y C

### 55.1 Bloque A — A-4 ejecutada por la sesión (sin cadena I-61)

1. **La guarda vieja habría fallado sin el comentario.**
   - Variante temporal, no confirmada: la aserción vieja `Assert.Contains("RackBomOutputGate.For(system).Reason", …)` aplicada al código
     de `PushBackKindHandler.cs` sin comentarios.
   - Resultado: `ThePushBackHandler_ConsumesTheSharedGateAndNotASecondRule` **Failed** con «Not found:
     "RackBomOutputGate.For(system).Reason"». Las otras 3 pruebas de la clase siguen en verde (TRX local `demo-old-guard`).
   - El archivo se restauró con `git checkout --`.
2. **Cambios aplicados:**
   - La guarda lee el código sin comentarios y el cuerpo de `OutputBlockedReason`, que hoy es un cuerpo de expresión que delega.
   - Exige `RackOutputVerdict.` y prohíbe `PushBackResolver` y `RackBomOutputGate.For`. Conserva las aserciones de «no hay una segunda
     regla» sobre el código sin comentarios.
   - Comentario de `ExpressionFormatter.FormatReference` reescrito según la A-3. Comprobado mecánicamente: el código sin comentarios ni
     espacios es idéntico antes y después, sin cambio de tokens.
3. **Focales:** 156/156 en verde.
   - Cubren `PushBackBomCommandGuardTests` (4), `ComputedParametersPopulationGuardTests` (INV-33; 3), `ComputedParametersSymbols*` (89),
     `ExpressionCoreGuardTests` (6) y `ExpressionFormatterTests` (54).
4. **Suites completas y builds locales:**
   - Core 12595/12595;
   - UI 1637 superadas y 17 omitidas por `Skip`;
   - build de UI con 0 errores y 0 advertencias;
   - build del Plugin con 0 errores (solo los MSB3277 conocidos), con salida temporal.
5. **Commit:** `a0835bf79d0e16bbe9dd142049aaab2ba5bb814f` con push, sobre la A-4 `17215ab2`.
6. **CI exact-SHA:** corrida `push` 37255791325, **`success` en los cuatro jobs**.
7. **Diff mecánico:** `git diff --name-only 17215ab2..a0835bf7` da exactamente
   `src/RackCad.Application/Expressions/ExpressionFormatter.cs` y `tests/RackCad.Tests/PushBackBomCommandGuardTests.cs`.

DEBT-I63-G2-01 y DEBT-I63-G3-01 quedan **cerradas**. No consumen `attempts` (2/3).

### 55.2 Bloque B — cabecera del contrato de la iniciativa

Valores aprobados por el Coordinator (Q-R-03):
- `materiality: [M-01, M-04, M-05, M-06, M-07, M-08]`. M-02 no está activado; M-03 tampoco, sujeto a las condiciones congeladas
  (Proposal V3 §3).
- `consumes: [Rack Identity, Project Variables, Authored vs Effective]`.
- `extends: [Expression Engine (ADR-0043)]`.
- `introduces: [Computed Parameters & Project Summary]`.

### 55.3 Bloque C — DEBT-I63-PROTOCOL-01

- Registrada en `docs/ideas-futuras.md` (sección de I-63), con DEV-G2-01 y DEV-I63-G4-02/03 como evidencia y las tres recomendaciones.
- No se editan I-61 ni I-62. El HANDOFF se actualiza en la sesión de integración.

## 56. READY bloque D — ADR-0049 propuesto

- **Reserva del número** (consulta del 2026-10-05):
  - `main` llega hasta ADR-0046;
  - en ramas activas, ADR-0047 está en `architecture/workspace-persistente-rackcad` (I-64) y ADR-0048 en
    `architecture/portabilidad-coordinador-principal` (I-62);
  - ninguna rama, remota o local, usa o reserva ADR-0049 (las coincidencias de «0049» en `docs/` son GUID dentro de TRX);
  - número reservado: **ADR-0049**. Es secuencial y nunca se reutiliza (`docs/adr/README.md`).
- **Archivo:** `docs/adr/0049-parametros-calculados-namespace-rack-y-resumen-de-proyecto.md`, en estado **`propuesto`**.
  - Es sucesor **parcial** de ADR-0043, que sigue aceptado.
  - Deriva del anexo A de la Proposal V3, actualizado con lo integrado en la rama: A-1..A-4 y D-02, D-07, D-10..D-21, D-24, D-25 y D-28
    tal como quedaron en los gates.
  - Índice de `docs/adr/README.md` con la fila 0049 en `propuesto`.
- **Siguiente:**
  1. revisión del mismo Architect R1-R3 sobre el archivo exacto (blob de este commit);
  2. resolver los REQUIRED que haya;
  3. **aceptación explícita del Owner** del ADR exacto (no delegable);
  4. estado `aceptado` y CI exact-SHA.
- **ADR-0043:** recibe su nota posterior fechada solo cuando el Owner acepte el ADR-0049.

## 57. ADR-0049: revisión 1 del Architect y corrección

- **Revisión 1** del mismo Architect R1-R3, en SEPARATE SESSION y de solo lectura, sobre `8466e20b` y el blob `4d0d7e67`: **CHANGES
  REQUIRED**, con un único REQUIRED.
  - **A63-ADR-01:** el punto 10 de «Decisiones nuevas» («la provenance queda fuera de la igualdad de `MetricValue`») era una decisión
    fuera del Freeze y de las A-n. Viene de la orden de G4 y de una prueba, no de D-21.
  - El resto, conforme y fiel al anexo A.
  - Hay 5 opcionales, O-ADR-1..5.
  - Texto literal: `docs/automation/evidence/I-63-pilot/READY/adr-0049/architect-review-1.txt`. Es igual al `SendMessage` de la
    transcripción del Architect, `fdd2a979-cce9-4391-83aa-e7dfe5effcc4.jsonl`.
- **Corrección (opción b):** la línea sale de las decisiones y pasa a «Consecuencias» como hecho de implementación sin carácter
  normativo, con su fuente: la orden de G4 y la prueba `D21_LaProvenanceNoEntraEnLaIgualdadDeMetricValue_NiLaCambia`.
- **Opcionales aplicados con el texto del Freeze:**
  - O-ADR-1: los agregados exigen además todos sus miembros `Available`.
  - O-ADR-2: la vía de I-63 de D-27.
  - O-ADR-3: un punto nuevo para D-17, la operación por rack y vía de ID23; las decisiones se renumeran a 13.
  - O-ADR-4: «tres conteos distintos, sin una autoridad común», según P-08.
  - O-ADR-5: fecha local 2026-10-04.
- **Siguiente:** el Architect cierra A63-ADR-01 sobre el blob nuevo; después, aceptación explícita del Owner.

## 58. ADR-0049: AGREED del Architect; pendiente de la aceptación del Owner

- **Revisión 2 del Architect: AGREED** sobre el commit `238da7a63fb9daafa1077fe29bf79561c8dc52cf`, ruta
  `docs/adr/0049-parametros-calculados-namespace-rack-y-resumen-de-proyecto.md`, blob **`0b43332248523f206f21a8dcbe383748dde8edef`**.
  - A63-ADR-01 queda CLOSED, con la opción (b).
  - Los opcionales O-ADR-1..5 se aplicaron y son fieles al Freeze.
  - No hay REQUIRED.
  - Texto literal: `docs/automation/evidence/I-63-pilot/READY/adr-0049/architect-review-2.txt`, igual al `SendMessage` de la transcripción del Architect.
  - El Architect corrige una errata del mensaje de la sesión: `238da7a6` es **hijo** directo de `8466e20b`, no su padre.
- **Opcional cosmético O-ADR-6** (no bloquea): el salto de línea suelto de «Contexto».
  - No se aplica ahora, porque cambiaría el blob acordado. Puede hacerse después de la aceptación como corrección tipográfica, que el
    README de `docs/adr` permite.
- **CI exact-SHA de los commits de READY:**

  | Commit | Contenido | Corrida | Resultado |
  |---|---|---|---|
  | `17215ab2` | A-4 | 37255044487 | 4/4 |
  | `a0835bf7` | Limpieza | 37255791325 | 4/4 |
  | `d5f64197` | Bloques A, B y C | 37256147153 | 4/4 |
  | `8466e20b` | ADR propuesto | 37256190319 | 4/4 |
  | `238da7a6` | ADR corregido (blob `0b433322`) | 37256414459 | 4/4 |

- **Siguiente: aceptación explícita del Owner** del ADR exacto (blob `0b433322`). Es el único retorno obligatorio al Owner dentro de
  READY.
  - Tras la aceptación: estado `aceptado`, registro de la aceptación, nota posterior en ADR-0043 y fila del índice.
  - Después, CI exact-SHA y el bloque E (FOUNDATIONS).

## 59. ADR-0049 aceptado por el Owner

- **Decisión del Owner** («Owner Decision — ADR-0049», pegada en el chat; texto literal en
  `docs/automation/evidence/I-63-pilot/READY/adr-0049/owner-acceptance.txt`): acepta explícitamente el ADR-0049 sobre el objeto
  exacto revisado por el Architect.
  - Objeto aceptado: commit `238da7a63fb9daafa1077fe29bf79561c8dc52cf`, ruta
    `docs/adr/0049-parametros-calculados-namespace-rack-y-resumen-de-proyecto.md`, blob **`0b43332248523f206f21a8dcbe383748dde8edef`**.
  - Architect = AGREED; REQUIRED = 0; A63-ADR-01 = CLOSED.
  - O-ADR-6 no forma parte de la aceptación. Puede corregirse después solo como corrección tipográfica.
  - Candidato e integración siguen **NO autorizados**.
- **Comprobaciones previas** (antes de editar):
  - `HEAD` = `origin/architecture/parametros-calculados-resumen-proyecto` = `20c1cef20fa5f9deb2e4e2c56cb09b0fde03570a`, árbol limpio.
  - `git rev-parse HEAD:<ruta del ADR>` = `0b43332248523f206f21a8dcbe383748dde8edef`: el ADR no cambió desde `238da7a6`.
  - `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, sin cambios.
  - CI exact-SHA de `20c1cef2` (custodia de la revisión 2): corrida 37256676343, 4/4.
- **Cambios de este commit:**
  - **ADR-0049:** `Estado: aceptado`, la fecha de aceptación, los decisores, la línea de la nota de ADR-0043 en tiempo presente
    y un preámbulo con la aceptación, según el precedente de ADR-0043. El texto desde «## Base exacta» hasta el final es byte a
    byte el del blob aceptado; el script de la sesión lo comprueba antes de escribir.
  - **ADR-0043:** solo se añade al final la sección «Notas posteriores», con una nota fechada que remite a ADR-0049 como
    sucesor parcial. No se marca como reemplazado; su estado sigue `aceptado` y el resto del archivo no cambia.
  - **Índice** (`docs/adr/README.md`): ADR-0049 pasa a `aceptado`.
  - **Decisiones de I-63** §2: fila de la aceptación.
  - **Estado:** READY sigue en curso; el bloque D queda cerrado y el siguiente es el bloque E (FOUNDATIONS).
- **Siguiente:**
  - O-ADR-6, solo tipográfico, en un commit propio.
  - Bloque E (FOUNDATIONS); después READY-04..09 y el retorno al Coordinator sin integración.

## 60. O-ADR-6 y READY bloque E (FOUNDATIONS)

- **CI exact-SHA:**

  | Commit | Contenido | Corrida | Resultado |
  |---|---|---|---|
  | `13c6bee0` | ADR-0049 aceptado (§59) | 37257733640 | 4/4 |
  | `5366777e` | O-ADR-6 | 37257869579 | 4/4 |

- **O-ADR-6** (`5366777e4db37ec7035334e4ffe4c09b48018a2c`): une el salto de línea suelto de «Contexto» del ADR-0049.
  - Toca solo el ADR; blob nuevo `a13732477f1e495408711e4226f1db227a66a686`.
  - `git diff --word-diff=porcelain` no da ninguna palabra añadida ni quitada, y la secuencia de palabras es idéntica: es una
    corrección tipográfica, autorizada por el Owner después de la aceptación, sin cambio normativo.
- **Entrada nueva de FOUNDATIONS «Computed Parameters & Project Summary»**, al final del registro y con el esquema completo. El contenido
  es el que aprobó Q-R-05. No se modifican `Project Variables` ni `Authored vs Effective`, ni ninguna otra entrada.
- **`Status: STABLE`.** Se cumplen las condiciones del esquema:
  - la fuente es un ADR aceptado (ADR-0049, §59);
  - el código existe y lo protegen pruebas identificables;
  - autoridad, persistencia, mutación y extensión coinciden con el código actual;
  - no hay discrepancia de clase A abierta;
  - hay un punto de extensión declarado.
- **Verificación DC-08 contra el código de la rama:**

  | Campo | Hecho comprobado |
  |---|---|
  | Authority | `RackMetricIds` (catálogo y `MetricId`), `RackMetricRequest.Execute` y `ProjectSummary.Evaluate` llaman a `RackMetricOrchestrator.Compute` (D-28). `ProjectPopulation.Evaluate`. `RackMetricProviderRegistry.Default` con seis providers: Selectivo `Supported` (`SelectiveDepthLayout.BaysOfFondo(…, 0)`), Dinámico, Push Back y Cantilever `NotSupported`, Cabecera y Cama `NotApplicable`. `PushBackKindHandler` delega en `RackOutputVerdict.HandlerBlockedReason` |
  | Persistence | El diff de `src/` contra `main` no añade DTO, store ni Xrecord; los lectores solo deserializan con los stores existentes. `PersistedBoundExpressionJson.PersistedNamespaceTokens` solo admite `projectVariable`. `ComputedParametersSymbolsPersistenceTests` comprueba `PresentButUnreadable` al leer y que no se escribe nada |
  | Mutation contract | `RackComputedExpressionContext.Create` solo acepta entradas `projectVariable` que no sean `Computed`, y añade las entradas `rack` del catálogo. `ComputedReferencesNotAvailable` es un resultado de `RackComputedEvaluationOutcome` |
  | Extension point | `RackComputedExpressionContext.Create` es `internal` (visible para `RackCad.Tests` por `InternalsVisibleTo`). Ningún archivo de `src/` fuera de `ComputedParameters/` construye el contexto |
  | Protecting tests | Las 15 clases de `tests/RackCad.Tests/ComputedParameters/`, más `ExpressionSymbolModelTests` (modificada en G3) y `PushBackBomCommandGuardTests` (reapuntada en la A-4) |

- **Siguiente:** READY-04, con fetch, preflight y rebase si `main` avanzó.

## 61. READY = PASS, Candidato final e integración

### 61.1 READY-04..09 sobre `55a66b3c412fa63a445dc1985977ad660ea72dd7`

Ninguno de estos hechos se commiteó antes del Candidato: la punta de producto se publica sola (WORKFLOW §11.5) y los hechos van a este
cierre.

| READY | Evidencia |
|---|---|
| 04 | `git fetch`; `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c` = merge-base, sin rebase; árbol y stash limpios; `HEAD` = upstream |
| 05 | CI `push` 37258162825: `head_sha` = Candidato, los cuatro jobs en `success`. Local, con el SDK 8.0.423 del usuario y `HEAD` y árbol limpio antes y después: Core Full 12595/12595; UI Full 1654 (1637 superadas y 17 omitidas por `Skip`, 0 fallos); build Debug de UI con 0 errores y 0 advertencias; build Debug del Plugin con salida temporal, 0 errores y solo las 2 advertencias MSB3277 conocidas |
| 06 | Architect: **CONFORMING**, REQUIRED = 0 (SEPARATE SESSION, el mismo de R1-R3, G3 y el ADR-0049; literal en `docs/automation/evidence/I-63-pilot/READY/ready-06/architect-ready06.txt`). Coordinator: **CONFORMING** (§61.2) |
| 07 | Árbol limpio; `HEAD` = remoto = Candidato |
| 08 | Owner Validation NOT APPLICABLE, decidida en el Freeze §22: sin UI, comandos, cambios de dibujo ni adaptador. Ningún escenario OV asignado ni retirado |
| 09 | El commit de Freeze `f61d0aca` solo cambia `Frozen: NO` → `Frozen: YES` sobre el blob acordado `e4a94eff` (`fb3c8788`); el blob `4d5dedce` es igual en el Candidato y ningún commit posterior toca la Proposal. A-1..A-4 tienen un solo commit cada una (`658b35ad`, `669d8a39`, `ea70e3b3`, `17215ab2`) y el mismo blob en el Candidato; secuencia continua. Los 67 commits de `819955d6..55a66b3c` son lineales y llevan exactamente un `Co-Authored-By` (57 Opus 5.5 y 10 Sonnet 5.5 del Worker) |

### 61.2 READY-06 y decisión del Coordinator

- **Architect: CONFORMING.**
  - Verificó READY-04, el CI y READY-09; tomó las suites locales como declaración de la sesión.
  - Conformes: autoridad (D-07, D-27 con equivalencia al handler antiguo), población, núcleo, persistencia NONE, legacy (INV-28),
    no-objetivos y puntos de extensión.
  - ADR-0049: desde «Base exacta» solo difiere O-ADR-6. ADR-0043 solo recibe «Notas posteriores».
  - FOUNDATIONS conforme con DC-08 y con Q-R-05.
  - Sobre la observación de la sesión (WORKFLOW §11.4 frente a LIFECYCLE §4.1): **no es desviación**.
  - **DEV-1** NON-MATERIAL (matriz D-06 duplicada en el agregador) y **DEV-2** EDITORIAL (estado atrasado). Opcionales O-R6-1..4.
- **Coordinator: CONFORMING**; READY-01..09 = PASS; **FINAL_CANDIDATE_SHA = `55a66b3c412fa63a445dc1985977ad660ea72dd7`**; integración autorizada (decisiones §2;
  literal en `docs/automation/evidence/I-63-pilot/READY/coordinator-ready-pass.txt`).
  - DEV-1 → **FOLLOWUP-I63-01** en `docs/ideas-futuras.md`, con O-R6-1.
  - DEV-2 → este commit (O-R6-2).
  - O-R6-3 y O-R6-4 diferidos, registrados en `docs/ideas-futuras.md` sin aplicarse.

### 61.3 Preflight de integración

- `git fetch --prune --tags`: `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, sin cambios; `HEAD` = remoto = Candidato; árbol
  limpio.
- No existe el tag `integration/I-63`. El `Claim-Id` del commit de reclamo `cdae6421` es `ce04f47f-ca53-4e75-b983-5ac57ab6c5e3`, el del
  estado.
- `git merge-tree` del Candidato con las ramas activas:
  - I-62 (`architecture/portabilidad-coordinador-principal`) e I-52 (`feature/rackmirror-espejo-semantico`): sin conflicto;
  - I-64 (`architecture/workspace-persistente-rackcad`): conflicto textual en `docs/ROADMAP.md`. Son las filas adyacentes tras I-60
    que el DC-07 ya preveía; lo resuelve I-64 al rebasar. No afecta al merge de I-63 en `main`.
- **Ventanas de los archivos calientes** (`docs/ROADMAP.md` y `docs/HANDOFF.md`, sin push a `main`), pedidas a las tres ramas activas que
  tocan ROADMAP.
  - La orden de READY prohibía enviar mensajes a I-62 en el contexto de la deuda de protocolo (Q-R-02). La petición de ventana es otro
    asunto, exigido por la orden de integración, y no menciona esa deuda.
  - Acuses literales, recibidos por SendMessage antes de escribir ROADMAP y HANDOFF:

```text
[I-62 (sesión «I - 62»)]
ACUSE de I-62 para la ventana de archivos calientes de I-63 (docs/ROADMAP.md y docs/HANDOFF.md), con FINAL_CANDIDATE_SHA 55a66b3c.
Desde este acuse y hasta tu RELEASE por este canal, la sesión de I-62:
- no escribe, rebasa ni publica cambios en docs/ROADMAP.md ni en docs/HANDOFF.md;
- no hace push a main ni rebasa su rama (architecture/portabilidad-coordinador-principal, punta fff3bbb0).
Su único trabajo en curso es esperar la CI de un commit que solo toca archivos propios de I-62 (A-1, evidencia, decisiones y estado de I-62); ninguno es un archivo caliente.
Tras tu RELEASE, I-62 se rebasará sobre el nuevo main cuando toque, según WORKFLOW §4.

[I-64 (sesión «I - 64»)]
ACUSE de I-64 para la ventana de archivos calientes de I-63 (docs/ROADMAP.md y docs/HANDOFF.md).

Desde este acuse hasta tu RELEASE por este canal, la sesión de I-64:
- no escribe, rebasa ni publica cambios en docs/ROADMAP.md ni en docs/HANDOFF.md;
- no hace push a main.

Estado de I-64 al acusar:
- rama architecture/workspace-persistente-rackcad en 39b45f36, árbol limpio;
- F1 está en BLOCKED_PROTOCOL_DEPENDENCY, sin trabajo en curso ni rebase previsto.

La rama de I-64 solo tiene en docs/ROADMAP.md su propia fila, del bootstrap. Tomo nota del posible conflicto textual entre las filas adyacentes de I-63 e I-64 en mi próximo rebase sobre main. Quedo a la espera del RELEASE.

[I-52 (sesión «I - 52»)]
ACUSE (I-52): hasta tu RELEASE no escribo, rebaso ni publico cambios en docs/ROADMAP.md ni docs/HANDOFF.md, y no hago push a main. Mi rama feature/rackmirror-espejo-semantico sigue en espera de host y no toca esos archivos; rebasaré después de tu RELEASE (post-host).
```

### 61.4 Commit documental final

- Contenido:
  - custodia de READY-06 y de la decisión del Coordinator;
  - esta sección;
  - fila de decisiones;
  - estado `integrated` / `completed` (DEV-2);
  - contrato `integrated`;
  - FOLLOWUP-I63-01 y los opcionales diferidos en `docs/ideas-futuras.md`;
  - HANDOFF §1, §2, §4 y §5;
  - fila propia de ROADMAP `integrada (2026-10-04)`.
- DEBT-I63-PROTOCOL-01 ya estaba en `docs/ideas-futuras.md` y en la evidencia; el HANDOFF la cita en §4.
- No se aplican O-R6-3 ni O-R6-4.
- Es un SHA nuevo que no sustituye al Candidato. Se le exige `git diff --name-only 55a66b3c..HEAD` solo bajo `docs/` y CI exacta 4/4.
- `MERGE_SHA`, la CI posterior al merge con cobertura, la cobertura diferida del Candidato y la limpieza se registran en el tag
  `integration/I-63` (WORKFLOW §11.6).
