# FX-02 — referencias AUTHOR para la independencia del Architect (decisiones §63.5; CD-19 / OQ-24 / U-21)

> Preparación de la supervisión (plano a), sobre RackCad `d5b454fd`. **No es una disposición**, no fija valores observados y no publica nada en el
> fixture. Donde un valor solo puede observarlo la sesión de supervisión queda un marcador `<observado por la supervisión: …>`. Ningún archivo de
> esta preparación lleva nombres, correos ni rutas de perfil en claro (rutas con `%USERPROFILE%`).

Regla que se aplica (§63.5): «La referencia AUTHOR para la independencia se deriva de `ActorRef` y `SessionRef` observados de los autores reales del
objeto. Se exige evidencia custodiada, identidad del runtime y comparación con el Architect ejecutado. UNKNOWN en cualquier dimensión REQUIRED no
satisface la independencia. Ni identidades sintéticas ni basadas solo en `BindingId`.»

## 0. Objeto y conjunto de referencia

| Elemento | Valor | Fuente |
|---|---|---|
| Objeto | contrato de T1: commit `d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99`, `docs/automation/decisions/FX-U1-T1.gate-contract.json`, blob `628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0` | fixture `fx/u1`; kit FX-02, `controller-contracts.md` §7 |
| Requisito | ARCHITECT / REVIEW_DESIGN frente a AUTHOR: Actor, Session y Context REQUIRED; Provider PREFERRED | `RoleRequirements` del contrato |
| Commits que produjeron el objeto | solo `d30fb6a9` (2026-10-06T22:53:17Z; `git log -- <ruta>` en `fx/u1`); ningún cambio posterior de la ruta | fixture |
| Identidad Git del commit | sintética (P-16) y **sin trailer de IA** | fixture |
| Autor declarado | `IssuedBy` = «Coordinator del fixture (sesión de supervisión, plano a…)»; RackCad `FX-U1-chain/chain.json`: «By: Coordinator del fixture (supervisión)» | etiquetas, no `ActorRef` |

Conjunto de referencia (RAE §14.5; V14 B.2 `ReviewSubject`/`AuthorRef`): cada actor que escribió `d30fb6a9` o su contenido, observado. Ni la identidad
Git sintética, ni la etiqueta `IssuedBy`, ni un `BindingId` sirven como referencia (§63.5).

**⚑ F-1 (Coordinator).** V14 B.2 (`AuthorRef`) dice «sin trailer de IA → el autor humano». Leída al pie de la letra, `d30fb6a9` tendría un autor HUMAN
con identidad sintética, lo que §63.5 prohíbe. Esta preparación propone `Kind` = AI_SESSION, derivado de la evidencia custodiada de la unidad (la
transcripción de la sesión autora y `chain.json`), con `Operator` = el Owner. La rama «según la evidencia de la unidad» de B.2 está escrita para
commits **con** trailer: confirmar que §63.5 autoriza esta derivación para un commit del fixture sin trailer por P-16.

## 1. Observación de `ActorRef` y `SessionRef` del autor real (a)

### 1.1 Procedimiento (solo la supervisión; plano a)

1. **Identificar la sesión autora.** Se busca en el registro de sesiones de la app de escritorio la transcripción que contiene la creación de
   `d30fb6a9` (la llamada a herramienta cuyo resultado muestra el commit en `fx/u1` y su push). No se presupone que sea la sesión actual: el commit es
   del 2026-10-06 y desde entonces hubo relevos de la supervisión. Si hay más de una candidata, o ninguna, la autoría no es establecible (§1.3).
2. **Leer los metadatos de esa sesión.** Si la sesión autora es la propia, `get_session("self")`; si no, `get_session(<sessionId de la autora>)`.
   Fuente: operación 2 del descriptor `claude-desktop-session` («`get_session` devuelve `sessionId`, `model`, `effort` e `isRunning`»), con
   introspección RUNTIME_OBSERVED medida en F2.
3. **Subagentes.** Si la transcripción muestra que un subagente de la sesión autora escribió el contenido del contrato, ese subagente es otro actor
   de referencia: `ActorRef` con `AdapterId` `claude-subagent` y su id, y `SessionRef` = la de la sesión padre (B.2: «un subagente comparte la de su
   padre»).
4. **Alias de la misma sesión.** Si los metadatos exponen además el identificador de la sesión subyacente de Claude Code (nombre del archivo de su
   transcripción), distinto del `sessionId` de la app, se observa y se custodia como alias del mismo actor (ver F-3).
5. **Enlace commit → sesión.** Se registra la evidencia del enlace: ruta de la transcripción (con `%USERPROFILE%`), su SHA-256 en el instante de la
   observación y la ubicación del resultado de herramienta que muestra `d30fb6a9`.

### 1.2 Registro (marcadores; ningún valor inventado)

| Campo | Valor |
|---|---|
| `AuthorRef.Kind` | AI_SESSION (pendiente de F-1) |
| `Actor.AdapterId` | `claude-desktop-session` |
| `Actor.InstanceId` | `<observado por la supervisión: get_session(self)>` (o `get_session(<sessionId de la sesión autora>)` si no es la propia) |
| `Actor.InstanceIdSource` | `get_session.sessionId` (como en los preflights de F2) |
| `Actor.Assurance` | RUNTIME_OBSERVED solo si la herramienta responde para esa sesión; si no, NONE |
| `Session.SessionId` | `<observado por la supervisión: get_session(self)>` (sesión de escritorio de nivel superior: igual al `sessionId`) |
| `Session.Assurance` | como `Actor.Assurance` |
| Identidad del runtime | `AdapterVersion` `<observado por la supervisión: versión que informa la app>`; `ReportedModel` / `ReportedEffort` `<observado por la supervisión: get_session(self)>` |
| Alias (paso 4) | `<observado por la supervisión: id de la sesión subyacente, si la app lo expone>` o «no expuesto» |
| Actores subagente (paso 3) | `<observado por la supervisión: transcripción de la sesión autora>` o «ninguno» |
| Enlace commit → sesión | `<observado por la supervisión: ruta con %USERPROFILE%, SHA-256 y ubicación del resultado con d30fb6a9>` |
| `Operator` | etiqueta registrada del Owner, sin datos personales, o `"UNKNOWN"`; no entra en la comparación de un revisor IA (RAE §14.5) |
| `ObservedUtc` | `<observado por la supervisión>` |

### 1.3 Niveles de garantía (RAE §14.5; AP 16.21; V14 B.2)

- Actor y Sesión solo quedan SATISFIED con los dos lados en RUNTIME_OBSERVED o superior. REQUESTED (p. ej., un `--session-id` asignado antes de
  verlo en la corrida) o CONFIGURED no bastan; `Assurance` NONE deja la dimensión en UNKNOWN.
- Autoría no establecible (transcripción no disponible, sesión ambigua o metadatos que no responden): `AuthorRef.Kind` = UNKNOWN → Actor y Sesión
  UNKNOWN → no hay independencia (§63.5) → el paso del Architect queda UNVERIFIED con esa causa.

## 2. Custodia sin romper P-16 (b)

| Dónde | Qué |
|---|---|
| RackCad (evidencia real) | la observación en claro: `AuthorRef` completo de §1.2, enlace commit → sesión, identidad del runtime, `ObservedUtc` y los digests de §2.1. Ruta propuesta: `docs/automation/evidence/I-62-F6/FX-02/author-ref/author-observation.json`. Precedente: la evidencia de RackCad ya custodia `sessionId` reales (preflights de F2; auditoría de la revisión de A-4). Saneado previo (RAE §13.4) y sin rutas de perfil en claro |
| Fixture (repositorio público) | solo digests: `ReferenceActor` = `{AdapterId: "claude-desktop-session", InstanceId: "sha256:<64 hex>", InstanceIdSource: "get_session.sessionId (supervisión)", Assurance: "RUNTIME_OBSERVED"}`, transcrito por el Coordinator del fixture en la orden O4 o en la RLA, y copiado por A2 en `Independence.Requirements[AUTHOR].ReferenceActor` del binding del Architect (V14 B.5). Antes de publicar, la búsqueda literal P-16 de la plantilla de la orden (L15-L19) |

`ReferenceActor` de B.5 es un `ActorRef` y no lleva `SessionRef`. Para la sesión de escritorio, `SessionId` = `InstanceId` y el mismo digest sirve para
las dos dimensiones; con un actor subagente, la sesión de referencia es la del padre, que también está en el conjunto como actor (escribió el commit).
Con varios actores de referencia: una fila de `Requirements` por actor y `Satisfaction` única por dimensión, SATISFIED solo si lo es frente a cada uno.

### 2.1 Regla del digest (propuesta)

- `D(x)` = SHA-256 de los bytes UTF-8 de `x` en minúsculas, en hex minúsculas, escrito `sha256:<hex>`. Misma función en los dos lados y para
  todos los identificadores (instancia, sesión y alias). Precedente: los hashes del host de RAE §13.1 (SHA-256 en minúsculas, nunca en claro).
- Solo para identificadores de alta entropía (los `sessionId`, `session_id` y `thread_id` basados en UUID). Con un identificador de baja entropía
  (PID + `CreationDateUtc`) el digest sería reversible por fuerza bruta y expondría un identificador real: no se usa, y el paso queda pendiente de una
  decisión del Coordinator.
- **Comparación:** Actor SATISFIED si `D(InstanceId del Architect)` no está en el conjunto de digests de cada actor de referencia (instancia y
  alias) y los dos lados tienen RUNTIME_OBSERVED o superior; NOT_SATISFIED si coincide; UNKNOWN en otro caso. Sesión, igual con `SessionId`.

### 2.2 ¿Un digest de un identificador real observado es «no sintético» (§63.5)?

- **A favor.** Se deriva 1:1 de una observación real RUNTIME_OBSERVED que queda custodiada en claro en RackCad. Con una función resistente a
  colisiones, la igualdad de digests equivale a la igualdad de identificadores, así que la comparación es la misma. No se inventa ni se basa en un
  `BindingId`. V14 ya representa así identidades que no pueden ir en claro: `HostLabelHash` y `HostInstanceHash` (B.2; RAE §13.1) y
  `HumanReviewerRef` («SHA-256 de una identidad verificada», B.2).
- **En contra.** B.2 define `ActorRef.InstanceId` como el valor «observado del runtime» y, a diferencia de `HostRef` y `HumanReviewerRef`, no prevé una
  forma con hash. A2 no puede reproducir la observación del autor (D.6 prohíbe leer otras sesiones): recibe un digest publicado por el Coordinator del
  fixture y solo calcula el del Architect. La búsqueda de identificadores reales de D.7 no detecta un digest.
- **Lectura de esta preparación:** no sintético en sustancia, pero **dudoso** en la forma → **⚑ F-2 (Coordinator)**: decidir si (i) el digest en
  `InstanceId` del fixture, (ii) con la observación en claro custodiada en RackCad y (iii) la reproducción de S29 cumplen «evidencia custodiada» e
  «identidad del runtime» de §63.5. Si no lo admite, el paso del Architect queda UNVERIFIED con esa causa (U-21).

## 3. Comparación en la Entrada del Architect (c)

Dependencias: A-4 (A4-4 para aceptar el binding antes de invocar; A4-2 si el Architect va por `claude-cli`; §63.1, opción B: aceptación individual
real fuera de la ventana) y A4-CLAUDE-FX02-CONSUMO con `claude-cli`. Sin ellas, este paso no se ejecuta.

1. **Aceptación del binding** (fuera de la ventana, decisión individual del Coordinator del fixture). `ReferenceActor` = el `ActorRef` en forma de
   digest de §2. La candidata no ha arrancado: Actor y Sesión UNKNOWN por NOT_STARTED, admitidos solo con A4-4, regla 1 (a)-(d); la condición (a),
   «cada actor de referencia tiene `ActorRef` y `SessionRef` RUNTIME_OBSERVED o superior», la cumple la observación de §1, no una etiqueta.
2. **Lanzamiento** en una sesión nueva de nivel superior. `claude-cli`: `--session-id` = uuid4 nuevo asignado por el kit, comprobación previa de que no
   existe ninguna transcripción con ese id y prohibición de `--resume` / `--continue` (precedente: kit de la revisión de A-4). `codex-cli`: invocación
   nueva con `thread_id` propio. Contexto: el cierre efectivo de insumos excluye la transcripción, la memoria y el razonamiento de la sesión autora; las
   entradas automáticas se enumeran (`claude-cli` comparte el perfil del usuario: sin `CLAUDE.md`, memoria ni ganchos, como en la caracterización C1;
   auditoría de lecturas).
3. **Entrada** (A2, después de la corrida; A4-4, regla 3):
   - observa la identidad del Architect en la propia corrida: `claude-cli`, el `session_id` de los mensajes `system/init` y `result` del stream-json
     (igual al asignado; la asignación sola es REQUESTED); `codex-cli`, el `thread_id` de sus eventos o de su registro de sesión. Identidad del
     runtime: `AdapterId`, SHA-256 y versión del binario, modelo y effort observados;
   - calcula `D(·)` con la regla de §2.1 y lo registra en `relay-record/v2` (`Participant.Actor` / `Session` en forma de digest; filas de
     `Participant.Observation` con `Source` y `SourceSha256` del extracto saneado), nunca el identificador en claro;
   - compara por dimensión con cada actor de referencia: Actor y Sesión por §2.1; Contexto por AP 16.21 (cierre, entradas automáticas, lecturas);
     Proveedor PREFERRED (con `claude-cli` no se cumple: se registra);
   - toda dimensión REQUIRED distinta de SATISFIED → salida INVALID: no se ingiere, el lanzamiento cuenta sin devolución y rige «Si falta un
     REQUIRED» de AP 16.21 (A4-4, regla 4).
4. **Ingestión:** I-S18 («todo resultado ingerido de ARCHITECT tiene un `runtime_evidence` cuyo actor observado difiere del de cada autor IA del
   objeto») se comprueba con la misma comparación de digests.
5. **S29 (supervisión, RackCad):** reproduce con los valores en claro: (i) `D(observación custodiada del autor)` = el digest publicado en el fixture;
   (ii) `D(id del Architect en su transcripción o stream)` = el digest del `relay-record`; (iii) comparación en claro, alias incluidos; (iv) el enlace
   commit → sesión sigue valiendo. Cualquier diferencia se propone al Coordinator como independencia no acreditada (D.4).

## 4. Banderas para el Coordinator

| Id | Asunto |
|---|---|
| F-1 | `d30fb6a9` sin trailer de IA y con identidad Git sintética: autor AI_SESSION derivado de la evidencia, frente a la letra de B.2 («sin trailer → el autor humano») |
| F-2 | si el digest de un identificador real observado cuenta como «no sintético» y como «evidencia custodiada» e «identidad del runtime» de §63.5 (§2.2) |
| F-3 | alias de una misma sesión (`sessionId` de la app frente al id de la sesión subyacente de Claude Code): comparar un solo digest no detecta la misma sesión con otra grafía. Mitigación: custodiar los alias (§1.1, paso 4), sesión nueva por receta y prohibición de reanudar (§3, paso 2) y comparación en claro en S29 |
| F-4 | A2 no puede verificar que el digest AUTHOR corresponde a una observación real (D.6): confía en la publicación del Coordinator del fixture y en la reproducción de S29 |
| F-5 | la sesión autora puede ser una sesión anterior de la supervisión; si su autoría no es establecible, el paso del Architect queda UNVERIFIED con esa causa |
| F-6 | `ReferenceActor` (B.5) solo lleva `ActorRef`: la `SessionRef` de AUTHOR se cubre con el mismo digest para la sesión de escritorio y con el padre como actor cuando hay un subagente; confirmar que basta |
