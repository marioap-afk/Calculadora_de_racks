# I-62 — Paquete de revisión del Architect (Proposal V7)

```text
PROPOSAL V7 — NOT REVIEWED
Architect      = REVIEW REQUIRED (V6: CHANGES REQUIRED, A62-V6-01..03; registro I-62-architect-review-v6.md)
Coordinator    = la corrección se ordenó sin otro bucle amplio de revisión del Coordinator antes del Architect, salvo que V7 viole la orden de forma mecánica
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V7 §11.4, dos alternativas delimitadas); sin decisión, no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v7.md           blob 9f68c9501fb8254792f875a039e11ec7485108ef
  docs/initiatives/I-62-architect-review-v6.md   blob 2217239781eb47ec297c58d4313d511807c4e326
Versión anterior revisada por el Architect:
  V6: commit 3888c5c8de19b5abaf479983e56836ef2bcef76e, blob 19672958ec095ba9f88e103240d34c62c76ee850
Discovery base: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65…, commit 2b7976b0), con §21 añadida en ea055591
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v7.md` = `9f68c950…`. Si no coincide, revisa la versión designada o rechaza la discordancia.

## 0. Condiciones de la revisión (orden del Coordinator)

- **Modo:** SEPARATE SESSION. Declarar el modo real y si revisor y autor son la misma persona (LIFECYCLE §5).
- **Insumos:** solo los canónicos y versionados de §2. Sin la transcripción de la sesión autora ni su memoria.
- **Contexto inyectado:** antes de revisar, declarar todo contexto inyectado automáticamente (instrucciones globales, `CLAUDE.md` o `AGENTS.md` del directorio,
  memoria del proyecto). Una sesión de Claude Code abierta en la carpeta de trabajo de la sesión autora carga su memoria de proyecto, con notas de autoría: debe
  abrirse en un clon limpio en otra ruta, o con otro runtime.
- **Alcance:** todo V7, no solo el delta. Las trazas de V7 (§§8.6-8.8, E.3.0, E.6, Anexo F) son análisis del diseño, no ensayos.
- **Veredicto:** AGREED (solo con cero REQUIRED sobre la versión exacta), CHANGES REQUIRED o BLOCKED — OWNER DECISION. OD-6 ya es conocida y está pendiente a
  propósito: no es un defecto de diseño.

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, fuente o contraejemplo, por qué importa, corrección precisa.
Modo:      SEPARATE SESSION (declarado); si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

## 2. Lectura

1. [Proposal V7](I-62-proposal-v7.md) completa (anexos A-G).
2. [Registro de la revisión del Architect de V6 y disposición del Coordinator](I-62-architect-review-v6.md).
3. Registros del Coordinator: [V1](I-62-coordinator-review-v1.md) … [V5](I-62-coordinator-review-v5.md).
4. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt).
5. Autoridades: LIFECYCLE §§2-9; WORKFLOW §§3-4 y 10-11; AGENTS; AUTOMATION_PLAN §§8 y 16 (16.7 en especial); ADR-0046; el Freeze de I-61
   ([Proposal V9](I-61-proposal-v9.md)); `routing.md`, `model-catalog.md` y el README de agent-execution.

## 3. Delta V6 → V7

| Área | V6 | V7 |
|---|---|---|
| Rebase fuera de una ventana (A62-V6-01) | solo el rebase de 16.7 dentro de una ventana estaba especificado; un rebase de apertura dejaba en el estado SHAs de la rama reescritos sin regla | **§8.8**: secuencia completa. (1) `fetch` y registro de `branch_before`; (2) rebase; (3) `RebaseMap` con imágenes y `patch-id`; (4) si algo no se acredita, la rama local vuelve atrás, no se publica nada y STOP (16.7); (5) `--force-with-lease=<rama>:<branch_before>`, que **no** es punto durable; (6) **QU REBASE_RECONCILIATION** obligatorio antes de cualquier Q0 o acción delegada, con el `RebaseMap` custodiado (B.8.7) y una tabla de todos los campos SHA: imagen, sin cambio para los hechos históricos, o reemisión del contrato. Contadores y `TaskId` intactos. Ningún SHA se descarta en silencio. RED, `chain_red_files`, commits sin verificar y la cita de la corrida de CI por el SHA original quedan definidos. Funciona en otra máquina sin los objetos originales. **B.8**: `custody.qu_kind`, `custody.last_rebase`; I-S17, I-P02 (rebase), I-P10; clase **con historia** I-H01 e I-H02. **T19**. **G.1**: fila de 16.7. **C-15**: escenario completo (F.6) con clon limpio en otra máquina y variantes |
| Aplicabilidad (A62-V6-02) | toda unidad posterior a EFF quedaba implícitamente dentro del protocolo I62 | **§14.0**: la ejecución delegada sigue siendo **opt-in**. Se separan A (operación de la iniciativa) y B (adopción delegada). Marcador de G0 `I62-DELEGATED-EXECUTION: DIRECT_ONLY \| I62_DELEGATED`. DIRECT_ONLY: estado `/v1` ordinario, sin maquinaria I62 y sin contratos (P-15); el trabajo directo no depende de nada de I-62. **§8.7**: adopción en G0 o a mitad de iniciativa (T20) con el mismo orden que el Modelo A; rechazo → `/v1` (T21); sin transición inversa; las unidades ANTERIORES siguen en I61. **§4.1**: CUSTODY acotada a la custodia del protocolo delegado; no es permiso de trabajo directo (E13). Conciliación explícita con «Ninguna otra iniciativa lo adopta por estar escrito». **E.2**: resultado DIRECT_ONLY. **B.8**: `protocol.basis.adoption_at`; I-P11. **G.1** y **OD-1**. **C-28** (nueva) |
| Independencia de LIFECYCLE (A62-V6-03) | la alternativa 1 de OD-6 solo expresaba la evidencia de un revisor runtime frente al Principal actual | **§11.4**: ambas alternativas conservan los tres modos. Alternativa 1: el conjunto de referencia es `ReviewSubject` (todas las identidades de autor de la versión exacta, con sucesiones y rebindings; autor no establecible → UNKNOWN, no satisface). Predicado por modo: SEPARATE SESSION (`ActorRef`, `SessionRef`, insumos canónicos y contexto inyectado declarado); EXTERNAL HUMAN (`HumanReviewerRef` distinto de todo autor humano, `ReviewInstanceRef` propia, insumos canónicos; sin referencias runtime ficticias); SAME-SESSION ROLE sigue vigente, pero no basta para estas revisiones mayores. Alcance: unidades I62_DELEGATED. **B.2**: `AuthorRef`, `ReviewSubject`, `HumanReviewerRef`, `ReviewInstanceRef`. **§11.3**: la referencia de §16 es el conjunto de autores del rango evaluado, incluidos los `SupersededCommits`. **C-13**: casos 6-9 |
| OPTIONAL O-02 | I-S15 mezclaba una condición con historia en la clase sin historia | I-S15 sin historia; la condición pasa a I-H01 (con historia, C-15); C-18 excluye de forma explícita la clase con historia |
| OPTIONAL O-03 | la sesión pasaba al Controller su propio registro como dato de contraste | el Controller recibe solo la instrucción normativa y los encabezados; calcula de forma independiente; el Coordinator compara después |
| OPTIONAL O-04 | el punto de entrada se localizaba por línea exacta | misma semántica de `Match` que E.3.1 (línea normalizada, nivel, fuera de bloques de código) para el punto de entrada y para §16.13 |
| OPTIONAL O-06 | no se decía dónde se registran las PREFERRED de la alternativa 2 | en la cabecera del registro recuperable de la revisión, con el modo, si revisor y autor son la misma persona y el estado de cada dimensión |

**Sin cambio:** el Modelo A (solo se añade el marcador de aplicabilidad a la decisión de G0), la transparencia para los contratos I61 y el punto de entrada de
WORKFLOW (solo O-03 y O-04), la lectura compuesta, FX-04a/FX-04b y los demás cierres.

## 4. Disposición por ID (la sesión declara; el cierre lo decide el emisor)

| ID | Punto exigido | Dónde lo atiende V7 | Cierre verificable que se ofrece (análisis) |
|---|---|---|---|
| A62-V6-01 (1) | rebase fuera de ventana con `--force-with-lease` y SHA esperado explícito; el force-push no es punto durable | §8.8 pasos 1-5; T19 | F.6 pasos 3-4 |
| A62-V6-01 (2)-(3) | QU dedicado antes de Q0; `RebaseMap` como `StateRef`; todos los campos SHA; contadores y `TaskId`; nada descartado | §8.8 paso 6 y tabla de campos; B.8.7; I-P10 | F.6 paso 5 |
| A62-V6-01 (4) | imagen o parche no acreditable → STOP, sin Q0 | §8.8 paso 4; B.8.7 | variantes de F.6 |
| A62-V6-01 (5) | transición legal y explícita | `qu_kind`; I-S17; I-P02; I-H02 | C-15; C-18 |
| A62-V6-01 (6) | 16.7 en la matriz | G.1 | — |
| A62-V6-01 (7) | escenario de C-15 con clon limpio en otra máquina | Anexo C, C-15; F.6 | sin objetos del host anterior: patch-ids recalculados sobre las imágenes; CI citada por el SHA original |
| A62-V6-02 (1)-(2) | DIRECT_ONLY sin maquinaria; I62_DELEGATED explícito y durable | §14.0; §8.7 | C-28 (a)-(c) |
| A62-V6-02 (3) | adopción a mitad de iniciativa | §8.7 (T20, T21); I-P11 | C-28 (d); C-15 (adopción) |
| A62-V6-02 (4) | CUSTODY acotada | §4.1; E13 | C-28 (a) |
| A62-V6-02 (5) | conciliación con AUTOMATION_PLAN §16 | §14.0; Anexo A | — |
| A62-V6-02 (6) | delta en G.1 bajo OD-1 | G.1; §18 (OD-1) | — |
| A62-V6-02 (7) | comprobación de la unidad nueva DIRECT_ONLY | Anexo C, C-28 | C-28 |
| A62-V6-03 | revisores runtime y EXTERNAL HUMAN; conjunto de referencia con todos los autores; sin retirar modos; OD-6 pendiente | §11.4; B.2; §11.3 | C-13 (6)-(9) |
| O-02, O-03, O-04, O-06 | correcciones locales | B.8.4; E.3.0; §11.4 | C-18; C-20c |

## 5. Seguimientos documentados (sin ampliar V7)

- **O-01, O-05 y O-07:** el Coordinator permite dejarlos como seguimientos. Su texto no se transmitió a la sesión autora, así que esta no los parafrasea ni
  los infiere. Quedan registrados por id para la revisión del Architect de V7, que tiene el dictamen original.
- **Riesgos residuales que se piden juzgar:**
  - la lectura compuesta y el punto de entrada de WORKFLOW, ya descritos en el paquete V6;
  - el coste para el Coordinator de los marcadores de G0, que ahora son tres en las unidades I62_DELEGATED y uno en las DIRECT_ONLY;
  - que una unidad DIRECT_ONLY no tenga estado `/v2` obliga, para adoptar, a un BOOTSTRAP de adopción. Es intencional: el estado `/v2` solo existe donde hay
    protocolo delegado;
  - en la alternativa 1, establecer `AuthorRef` exige que las sesiones autoras de una unidad I62_DELEGATED queden identificables en su custodia; un autor no
    establecible bloquea (fail-closed).

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor: §16, §16.13, punto de entrada de WORKFLOW, **aplicabilidad DIRECT_ONLY / I62_DELEGATED** y **rebase fuera de ventana** | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas (A, B, FX-04b) |
| OD-3 | autenticar Claude CLI | B |
| OD-4 | sandbox de Codex para escritura | B y FX-04b (no FX-04a) |
| OD-5 | permiso de ensayo con la semántica única de §15 | F6 |
| OD-7 | remoto del fixture con CI | cierre de F6 (por FX-02) y FX-04b (no FX-04a) |

## 7. Preguntas para la revisión

1. ¿Debe contar como autor, para la independencia de una revisión EXTERNAL HUMAN, el humano que operó las sesiones IA autoras? V7 exige declararlo y lo deja
   abierto (§11.4).
2. ¿El QU REBASE_RECONCILIATION cubre todos los campos con valor SHA que sobreviven a un rebase, o falta alguno en la tabla de §8.8?
3. ¿Es correcto limitar la alternativa 1 de OD-6 a las unidades I62_DELEGATED, en coherencia con el carácter opt-in, en lugar de a todas las posteriores?
4. ¿El BOOTSTRAP de adopción posterior (§8.7) reutiliza correctamente el Modelo A, o hace falta un punto distinto?

## 8. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.
