# I-62 — Revisión del Coordinator de la Proposal V4 (registro)

```text
Emisor:           Coordinator exclusivo de I-62
Modo:             SEPARATE SESSION respecto de la sesión autora; revisor ≠ autor
Naturaleza:       revisión del Coordinator; no es dictamen del Architect, firma del Owner, decisión del Master, Freeze ni autorización de implementación
Fecha:            2026-10-01
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — COORDINATOR REVIEW OF PROPOSAL V4 / ORDER FOR V5»);
                  no existe archivo de origen en D:\IDs ni hash de archivo. Procedencia: la transcripción literal recibida (6 132 bytes en UTF-8,
                  SHA-256 040606ba4c0ea43dbe9db2c3fae6f4b792b2b005c97c92f8ce6d97147803a4bc), custodiada fuera del repositorio con la
                  transcripción de la sesión; este archivo es su registro recuperable
Objeto revisado:  commit 2960b28614da14ad6ebe9eeaa9631fd787041ac3 (padre 486e45e7…)
                  docs/initiatives/I-62-proposal-v4.md, blob dd9e478d737c76139b0eecc3c9e30d9beae8ccd6
                  docs/initiatives/I-62-architect-package-v4.md, blob 8a2a989585e02874600e2907361f807ee3aa81c1
                  docs/initiatives/I-62-coordinator-review-v3.md, blob 461ef605ffc04b1df85ce6cf60d2a132e7506403
Veredicto:        CHANGES REQUIRED sobre V4 (R62-V4-01..02). OD-6 sigue pendiente del Owner
Architect:        NOT REVIEWED · Frozen: NO
```

Registro redactado por la sesión responsable por orden del emisor (C62-F0-24). El contenido es del Coordinator. No reescribe los registros de
[V1](I-62-coordinator-review-v1.md), [V2](I-62-coordinator-review-v2.md) ni [V3](I-62-coordinator-review-v3.md). La respuesta de la sesión está en la
[Proposal V5](I-62-proposal-v5.md) y en el [paquete V5](I-62-architect-package-v5.md) §4.

## 1. Publicación verificada por el Coordinator

- punta de la rama = commit revisado; padre = `486e45e7…`; `origin/main` observado = `819955d6…`;
- CI 36924676021, attempt 1, evento `push`, `head_sha` exacto: Tests (Domain + Application), Build UI, UI Tests y Build Plugin without AutoCAD en `success`.

## 2. Hallazgos REQUIRED de V4

### R62-V4-01 — La resolución legacy debe ser transparente para los contratos I61

**Problema (resumen fiel).** V4 introduce un resolver de compatibilidad ejecutable, pero su propia traza C-20c muestra que un contrato `/v1` válido (K61)
falla tras la integración de I-62. Hay que reemitirlo como K61' con:
- citas obligatorias de §16.13 y del mapa de compatibilidad;
- citas por sección en lugar de las citas existentes de documento completo.

Eso es un cambio operativo de protocolo para una unidad I61 ya abierta. El mandato del Owner exige que las iniciativas de producto activas conserven su
protocolo durante toda la iniciativa.

**Corrección exigida.** V5 debe hacer la resolución de compatibilidad transparente para un contrato I61 válido existente:
1. sin cambios en los esquemas `/v1`;
2. sin campos `/v1` nuevos obligatorios;
3. sin reemisión obligatoria solo para acogerse al resolver;
4. sin exigir que una cita `EXTERNAL` de documento completo se reescriba solo porque I-62 modificó ese archivo;
5. `MainSha` y `AuthorityRevision` conservan su significado `/v1`;
6. la capa de compatibilidad determina la revisión de las cláusulas modificadas por I-62 sin que el autor del contrato I61 tenga que conocer I-62;
7. las autoridades no modificadas siguen evolucionando con normalidad cuando son compatibles;
8. los metadatos de compatibilidad ausentes o ambiguos siguen fallando cerrado;
9. C-20c parte de un contrato `/v1` I61 real y válido y demuestra que el contrato sin cambios sigue siendo interpretable tras EFF.

Si conservar una cita de documento completo exige resolver de forma conservadora el documento entero a `EFF^1`, hay que declarar ese compromiso de forma
explícita. No se exige en silencio que el consumidor migre su contrato.

### R62-V4-02 — Romper la circularidad bootstrap/G0 en state/v2

**Problema (resumen fiel).**
- B.8.1 hace obligatorio `protocol.g0_decision` y lo define como la decisión del Coordinator que aceptó la clasificación de la unidad. Pero la secuencia
  normal es publicar primero el bootstrap y revisar G0 después: el commit BOOTSTRAP no puede citar una decisión de G0 que aún no existe.
- La misma zona exige en BOOTSTRAP un binding del Principal ACCEPTED y un preflight CUSTODY, sin definir la autoridad de aceptación previa al bootstrap ni la
  ruta durable de la referencia.

**Corrección exigida.** V5 debe definir un orden ejecutable, eligiendo y especificando por completo un modelo coherente:
- **Modelo A — bootstrap y después G0:** BOOTSTRAP guarda la evidencia de clasificación, no una aceptación de G0 inexistente; la aceptación de G0 tiene una
  representación explícita PENDING/NOT_YET_DECIDED; un punto durable posterior registra la decisión real; se define qué campos de `protocol` pueden cambiar
  una sola vez y cuáles son inmutables; la conducta del clasificador antes y después de esa transición es de fallo cerrado y sin ambigüedad.
- **Modelo B — aceptación del Coordinator antes del bootstrap:** se identifica la autoridad que permite ese orden; se define el artefacto de decisión previo
  al bootstrap y su identidad; se muestra cómo lo referencia el bootstrap sin referencias futuras ni propias; se concilia el orden de forma explícita con
  WORKFLOW y LIFECYCLE.

Además, la secuencia de arranque del binding y del preflight del Principal: **observación → propuesta de binding → aceptación → referencia durable**,
indicando quién acepta y dónde existe el artefacto aceptado antes de que `state/v2` lo referencie.

**Restricción:** no se permite ningún SHA futuro, `StateRef` inexistente ni aceptación inferida.

## 3. Disposición de V4 (C62-F0-23)

**Cerrado:**
- R62-V3-03 por completo;
- el defecto principal de exact-SHA y custodia de V2/V3;
- la separación entre intención de tarea y ACCEPTED_OPEN;
- la especificación semántica de `state/v2`, salvo el orden bootstrap/G0 anterior;
- el modelo Q0/Q7/QU/QH/QR como dirección de la custodia;
- S-12, `Routing` advisory, la semántica de `Handoff` y el conteo de correcciones lanzadas;
- la activación del fixture y el delta de `Ci`;
- el momento del índice ADR y la matriz de obligaciones;
- `ActorRef` independiente del `BindingId`;
- RESUME_DECISION separado de CUSTODY.

**Abierto:** R62-V4-01, R62-V4-02 y OD-6 (reservada al Owner).

## 4. Autorización documental (C62-F0-24)

Se autoriza la **Proposal V5** como revisión completa solo documental, `Frozen: NO`. La sesión principal puede:
- crear la Proposal V5 y el paquete del Architect V5;
- registrar esta revisión del Coordinator;
- actualizar solo el contrato, las decisiones, la evidencia y el estado de I-62, según haga falta.

**Preservación:** todas las versiones anteriores de la Proposal, los paquetes, el Discovery, el mandato y los registros de revisión del Coordinator.

**Architect:** no se invoca todavía. Los dos REQUIRED son problemas mecánicos de consistencia del protocolo y deben corregirse antes de gastar la ronda de
revisión del Architect.

**No autorizado:** implementación, esquemas, pruebas, adapters, fixtures, pilotos, modelos delegados, SP-1/SP-2, autenticación, sandbox, `config.toml`,
decisiones del Owner ni ediciones de documentos normativos compartidos.

**OD-6** sigue PENDING. No se infiere su resultado.

**Entrega siguiente esperada:**
- Proposal V5 completa, `Frozen: NO`;
- delta V4→V5;
- disposición de R62-V4-01 y R62-V4-02;
- paquete V5;
- registro recuperable de la revisión;
- identidades exactas de commit y blob;
- CI exacta de la publicación.

## 5. Estado declarado por el emisor

- G0 = GATE PASS;
- Discovery R1 = ACCEPTED AS DESIGN BASIS;
- Archetype = NEW ARCHITECTURE;
- Proposal V4 = CHANGES REQUIRED — Coordinator;
- Proposal V5 = DOCUMENTATION AUTHORIZED;
- Architect = NOT REVIEWED;
- OD-6 = PENDING OWNER DECISION;
- FREEZE = NOT_AGREED;
- IMPLEMENTATION AUTHORIZATION = NO;
- I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED.
