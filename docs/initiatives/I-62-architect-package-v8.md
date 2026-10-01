# I-62 — Paquete de revisión del Architect (Proposal V8)

```text
PROPOSAL V8 — NOT REVIEWED
Architect      = REVIEW REQUIRED, revisión formal NUEVA (V7: CHANGES REQUIRED, A62-V7-01..03; registro I-62-architect-review-v7.md)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V8 §11.4, dos alternativas delimitadas); sin decisión, no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v8.md           blob 667b59d7a433130e20a42bbc13d3e6f564350049
  docs/initiatives/I-62-architect-review-v7.md   blob 40a6d5205ff3edef9c781d69c4c9c116b76774a9
Versiones anteriores revisadas por el Architect:
  V6: commit 3888c5c8de19b5abaf479983e56836ef2bcef76e, blob 19672958ec095ba9f88e103240d34c62c76ee850
  V7: commit 3d7ec77f07d374704f2bc0b888a07bb182c32203, blob 9f68c9501fb8254792f875a039e11ec7485108ef
Discovery base: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65…, commit 2b7976b0), con §21 añadida en ea055591
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v8.md` = `667b59d7…`. Si no coincide, revisa la versión designada o rechaza la discordancia.

## 0. Condiciones de la revisión (orden del Coordinator)

- **Sesión distinta** de la autora, en SEPARATE SESSION. Declarar el modo y si revisor y autor son la misma persona (LIFECYCLE §5).
- **Clon o ruta limpios, sin la memoria de proyecto del autor.** Una sesión de Claude Code abierta en la carpeta de trabajo de la sesión autora carga su
  memoria de proyecto, con notas de autoría.
- **Sin la transcripción** de la conversación del autor; solo los insumos canónicos de §2.
- **Declarar todo contexto inyectado automáticamente** antes de revisar.
- **Alcance:** V8 completa. El foco mínimo es el delta (§3). Las trazas (§§8.8-8.9, Anexo F) son análisis del diseño, no ensayos.
- **Veredicto:** AGREED (cero REQUIRED sobre la versión exacta), CHANGES REQUIRED o BLOCKED — OWNER DECISION. OD-6 está pendiente a propósito y no es un
  defecto de diseño.

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, fuente o contraejemplo, por qué importa, corrección precisa.
Modo:      SEPARATE SESSION (declarado); si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

## 2. Lectura

1. [Proposal V8](I-62-proposal-v8.md) completa (anexos A-G).
2. Registros del Architect: [V6](I-62-architect-review-v6.md) y [V7](I-62-architect-review-v7.md). Registros del Coordinator:
   [V1](I-62-coordinator-review-v1.md) … [V5](I-62-coordinator-review-v5.md).
3. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt).
4. Autoridades: LIFECYCLE §§2-9; WORKFLOW §§3-4 y 10-11; AGENTS; AUTOMATION_PLAN §§8 y 16 (16.7 en especial); ADR-0046; el Freeze de I-61
   ([Proposal V9](I-61-proposal-v9.md)); `routing.md`, `model-catalog.md` y el README de agent-execution.

## 3. Delta V7 → V8

| Área | V7 | V8 |
|---|---|---|
| Invariantes del rebase (A62-V7-01) | I-P05 prohibía que un QU cambiara `chains`, justo lo que el QU de reconciliación debe hacer. I-H02 no distinguía la ventana activa. El rebase dentro de una ventana no reconciliaba los SHA persistidos de otras cadenas | **I-P05**: el QU ORDINARY conserva la prohibición, y el QU o QR REBASE_RECONCILIATION es la excepción explícita, limitada a los campos de `StateFields` (B.8.7). **I-H02**: solo fuera de una ventana activa. **Rebase dentro de una ventana** (§8.8): vía de 16.7 conservada, sin QU dentro de la ventana; el `RebaseMap` del diario lleva `StateFields[]` con **todo** SHA persistido afectado, incluidas las cadenas de otras tareas o anteriores; el **Q7 o QR de cierre** reconcilia; **I-P12** nueva. `custody.point_kind` (QU o QR) sustituye a `qu_kind`; `last_rebase` generalizado; B.7 añade `RebaseMap.StateFields[]` y `CiRuns[]` al `relay-record/v2`. **C-15**: rebase dentro y fuera de una ventana, cadenas ajenas y clon limpio. **C-18**: pares positivos simultáneos de I-P05, I-P10 e I-P12 |
| QH + Principal nuevo + avance de `main` (A62-V7-02) | T16 exigía un QR para operar y WORKFLOW §4 exigía rebasar antes de escribir: los dos requisitos se contradecían | **§8.9, REBASE_TAKEOVER:** designación acotada (`I62-REBASE-TAKEOVER`) con cinco acciones permitidas: `fetch`, rebase, `--force-with-lease`, `RebaseMap` y **un** QR combinado. Ese **QR REBASE_RECONCILIATION** registra al titular y reconcilia los SHA (y, en T12b con Q0, cierra la ventana ABANDONED). Después ya hay trabajo ordinario o Q0. **T22** nueva; T16 y T12b remiten a T22 cuando `main` avanzó. **I-P02**: QR REBASE_RECONCILIATION desde QH, Q7, QU, QR, BOOTSTRAP o, en T12b, Q0. **§8.1**: semántica del QR. **C-15** y **F.7** (QH + avance de `main` + host nuevo limpio). FX-04b nombra T22 en una línea |
| `ReviewSubject` y operador humano (A62-V7-03) | los autores eran los de la historia completa de las rutas, y el operador humano era una pregunta abierta | `ReviewSubject.Kind` (DESIGN o IMPLEMENTATION), `RangeBase` y un **conjunto acotado a la unidad y al objeto**. En implementación o conformidad: `merge-base(base, commit)..commit`, restringido a las rutas de cambio de la unidad. En diseño: las versiones de la Proposal y los deltas aceptados de la unidad. La historia anterior queda excluida, las sucesiones incluidas y un autor UNKNOWN falla cerrado. `AuthorRef.Operator`: en la alternativa 1, **el humano que dirigió materialmente una sesión IA autora es autor**, así que no puede ser el revisor EXTERNAL HUMAN independiente de ese objeto. **C-13**: casos 6, 7, 10 y 11 |
| OPTIONAL O-V7-03 | sin ubicación de la plantilla de los marcadores de G0 | §8.6: se materializa en F4 en la subsección I62 de AUTOMATION_PLAN §16 sobre arranque y adopción, y la decisión del Coordinator la copia literalmente |
| OPTIONAL O-01 | C-18 solo validaba archivos sintéticos | C-18 valida también los `docs/automation/state/*.yml` reales con `/v2` presentes en el árbol (hoy ninguno), sin historia ni Level B |

**Sin cambio:** la aplicabilidad opt-in, el Modelo A, la compatibilidad legacy y el punto de entrada de WORKFLOW, FX-04a/FX-04b (salvo una línea que nombra
T22), el diseño del rebase fuera de ventana (solo cambian sus invariantes) y los demás cierres.

## 4. Disposición por ID (la sesión declara; el cierre lo decide el emisor)

| ID | Punto exigido | Dónde lo atiende V8 | Cierre verificable que se ofrece (análisis) |
|---|---|---|---|
| A62-V7-01 (1) | QU ORDINARY con la prohibición; QU de reconciliación como excepción limitada a I-P10 y B.8.7 | B.8.4: I-P05, I-P10; B.8.7 | C-18: pares positivos simultáneos |
| A62-V7-01 (2) | I-H02 solo fuera de una ventana Q0 activa | B.8.4: I-H02 | C-15 (rebase dentro de una ventana sin STOP espurio) |
| A62-V7-01 (3) | dentro de una ventana: vía de 16.7; `StateFields` de todo SHA afectado; sin QU; reconciliación en Q7 o QR; invariante de pares | §8.8; B.7; B.8.7; I-P12 | C-15; C-18 |
| A62-V7-01 (4)-(5) | C-15 con cuatro escenarios; C-18 con pares positivos | Anexo C | — |
| A62-V7-02 | un orden ejecutable: designación acotada → rebase → force-with-lease → `RebaseMap` → QR combinado → trabajo; también para T12b | §8.9; T22 (T16 y T12b remiten); §8.1; I-P02 | F.7 y sus variantes; C-15 |
| A62-V7-02 (sin requisitos contradictorios) | el rebase precede al QR; la autoridad viene de la designación, no de un punto durable | §8.9 «Problema que resuelve» | — |
| A62-V7-03 | conjunto acotado por tipo de revisión; sucesiones incluidas; UNKNOWN falla cerrado; el operador humano es autor en la alternativa 1 | B.2 (`ReviewSubject`, `AuthorRef.Operator`); §11.4 | C-13 (6), (7), (10), (11) |
| O-V7-03, O-01 | correcciones locales | §8.6; C-18 | — |

## 5. Seguimientos

- **O-V7-01:** no se aplicó. El Coordinator lo condicionaba a que fuera solo una aclaración de nombres o de historia del estado, y su texto no llegó a la
  sesión autora, que no lo infiere. La generalización `qu_kind` → `point_kind` se hizo por A62-V7-02 (el QR también puede reconciliar). No pretende atender
  O-V7-01.
- **O-V7-02, O-05 y O-07:** sin cambio, por orden del Coordinator.
- **Riesgo residual que se pide juzgar:** la designación REBASE_TAKEOVER añade un marcador más a las decisiones del Coordinator, y una toma interrumpida entre
  el force-push y el QR deja la rama rebasada con el estado sin reconciliar. La recuperación es completar el QR por el mismo designado o por uno nuevo (§8.9).

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor: §16, §16.13, punto de entrada de WORKFLOW, aplicabilidad, rebase dentro y fuera de una ventana, **toma con rebase** | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas (A, B, FX-04b) |
| OD-3 | autenticar Claude CLI | B |
| OD-4 | sandbox de Codex para escritura | B y FX-04b (no FX-04a) |
| OD-5 | permiso de ensayo con la semántica única de §15 | F6 |
| OD-7 | remoto del fixture con CI | cierre de F6 (por FX-02) y FX-04b (no FX-04a) |

## 7. Preguntas para la revisión

1. ¿La lista de `StateFields` (B.8.7) cubre todo SHA persistido que una reescritura puede afectar, dentro y fuera de una ventana?
2. ¿Las cinco acciones de REBASE_TAKEOVER (§8.9) son suficientes y mínimas?
3. ¿El conjunto acotado de `ReviewSubject` en una revisión de diseño (versiones de la Proposal y deltas aceptados de la unidad) es la frontera correcta?

## 8. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.
