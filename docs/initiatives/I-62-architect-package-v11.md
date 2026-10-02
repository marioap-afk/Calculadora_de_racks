# I-62 — Paquete de revisión del Architect (Proposal V11)

```text
PROPOSAL V11 — NOT REVIEWED
Architect      = REVIEW REQUIRED: UNA revisión formal limpia, con la fidelidad de los insumos probada por el invocador antes de lanzar
Coordinator    = A62-V10-01, -03 y -04 ACCEPTED REQUIRED; A62-V10-02 NOT ACCEPTED (premisa inválida por degradación de la codificación);
                 requisito R62-FIDELITY-01..06; A62-V9-04 pendiente de nueva disposición (registro I-62-architect-review-v10-disposition.md)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V11 §11.4); sin decisión no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v11.md                        blob 3e8fa9d8eb8a875069224e8ed7fd5850145e4d0e
  docs/initiatives/I-62-architect-review-v10-disposition.md    blob 3328d1f5e11a1e3903cfd8ee3624f9d1d966efd8
Versiones anteriores:
  V10: commit 41ded86e6494e3d43e07ce97de9eedf4710a631f, blob 58f88fc602d0d4eaf3900a3514259da6b94faba2 (Architect: CHANGES REQUIRED; revisión formal válida
       con una excepción por hallazgo)
  V9:  commit b0725114…, blob 831e3a6a… (Architect: BLOCKED — OWNER DECISION; no acreditada como revisión formal limpia)
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v11.md` = `3e8fa9d8…`. Si no coincide, revisa la versión designada o rechaza la
> discordancia. Este paquete no lleva su propio blob.

## 0. Condiciones de la revisión limpia (disposición del Owner y del Coordinator, «NEXT ARCHITECT REVIEW»)

V11 tendrá **una** revisión formal limpia del Architect. La invocación debe:
- usar una sesión o un proceso independiente de la sesión autora, en SEPARATE SESSION, y declarar si revisor y autor son la misma persona (LIFECYCLE §5);
- usar un clon limpio y solo lectura;
- usar el **cierre efectivo de insumos** recalculado sobre el commit exacto (Proposal V11 §20.3.1), con todas las lecturas transitivas obligatorias de
  `AGENTS.md`;
- resolver antes la acción `dotnet test` de AGENTS «Leer primero» paso 4 con una exención explícita y acotada de quien autorice la invocación (§20.3.1). La CI
  de publicación puede darse como señal separada, nunca como evidencia equivalente;
- **probar la fidelidad de los insumos antes de lanzar** (§20.3.3) y comprobarla después;
- no usar la transcripción ni la memoria del autor;
- declarar el contexto inyectado;
- registrar el `RuntimeEvidenceRef` y el `InputFidelityEvidenceRef` desde el invocador.

**Prueba de fidelidad prevista para `codex-cli` con PowerShell en Windows.** Es una ilustración, no el contrato; el contrato es el requisito observable de
§20.3.3:
1. **Corpus:** extraer de los bytes canónicos del cierre el conjunto de caracteres no ASCII distintos. Solo la Proposal V11 tiene 36: letras acentuadas y ñ, ≤,
   ≥, ≠, →, ↔, ⇒, ⇔, ∈, ∉, ⊆, ⊇, ∩, ∪, ∅, ∧, ∖, ×, −, ✓, «», —, …, ·, § y otros. El conjunto del cierre se calcula sobre el commit exacto.
2. **Transporte:** configurar en UTF-8 la salida del shell del runtime y la lectura de archivos, y fijar en la cabecera de la invocación la forma de lectura
   que debe usar el revisor.
3. **Preflight:** leer cada insumo del cierre con el **mismo** `pwsh` del runtime de Codex, la misma configuración y la misma forma de lectura. Comparar la
   salida capturada con los bytes canónicos, tras la normalización permitida (fin de línea), y contar cada carácter del corpus. Comparar también el prompt
   renderizado con el que registra el log de sesión. Si algo no coincide: P-24, sin lanzamiento.
4. **Tras la corrida:** comparar con el texto canónico cada segmento de `aggregated_output` de las lecturas del revisor y fijar `FidelityStatus` y
   `DegradedSpans`. Esta comparación es la decisiva, porque el preflight reproduce el shell del runtime pero no la captura interna de Codex.

**Cierre efectivo previsto.** El invocador lo recalcula y lo custodia sobre el commit exacto:
- **`CanonicalInputs`:** los de §2;
- **`AllowedTransitiveInputs`:** los mismos de la revisión de V10, que en esas rutas siguen iguales a `main`:
  - `docs/HANDOFF.md`, `README.md` y `docs/ARCHITECTURE.md`;
  - `docs/context-packs/README.md` y `docs/context-packs/documentation-governance.md`;
  - `docs/ROADMAP.md` y `docs/FOUNDATIONS.md`;
  - `docs/initiatives/README.md` y `docs/initiatives/PROMPT_TEMPLATES.md`;
- **acciones:** permitidas `git log --oneline -10` y las lecturas de Git sobre el cierre; `dotnet test`, eximida o sin lanzamiento.

**Forma del resultado:** en lo posible, `rackcad-architect-review-result/v1` (Proposal V11 B.10.1), con `PremiseRefs` en cada hallazgo y cada disposición,
como representación experimental y no normativa. `OpenFindings`:
- **REQUIRED aceptados:** A62-V10-01, A62-V10-03 y A62-V10-04;
- **pendiente de nueva disposición:** A62-V9-04, contra el texto fiel de V11;
- **arquitectura de R62-FIDELITY-01..06**, como requisito del Coordinator;
- **OPTIONAL abiertos, no bloquean:** O-V7-01, O-V7-02, O-05 y O-07.

**A62-V10-02 no se evalúa como REQUIRED abierto** (no aceptado por premisa inválida).

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, fuente o contraejemplo, por qué importa, corrección precisa, PremiseRefs; disposición de cada hallazgo abierto.
Modo:      SEPARATE SESSION (declarado); si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

Con cero REQUIRED: el Owner decide OD-6, y la misma V11 exacta puede avanzar hacia el Consensus Freeze si el Coordinator también está de acuerdo.

## 2. Lectura (insumos canónicos)

1. [Proposal V11](I-62-proposal-v11.md) completa (anexos A-G); [Proposal V10](I-62-proposal-v10.md) y su [paquete](I-62-architect-package-v10.md), para
   verificar el delta.
2. [Disposición del Owner y del Coordinator sobre la revisión de V10](I-62-architect-review-v10-disposition.md); [registro de la revisión de
   V10](I-62-architect-review-v10.md) y, en `docs/automation/evidence/I-62-architect-v10/R20261002T010757Z-ef59/`, su resultado literal
   ([`output.json`](../automation/evidence/I-62-architect-v10/R20261002T010757Z-ef59/output.json)), su
   [README](../automation/evidence/I-62-architect-v10/R20261002T010757Z-ef59/README.md) y su
   [auditoría de lecturas](../automation/evidence/I-62-architect-v10/R20261002T010757Z-ef59/read-audit.json).
3. [Disposición sobre la revisión de V9](I-62-architect-review-v9-disposition.md), [registro de V9](I-62-architect-review-v9.md) y su
   [`output.json`](../automation/evidence/I-62-architect-v9/R20261002T000511Z-ba10/output.json); [requisito R62-AUTO](I-62-coordinator-requirement-auto.md);
   registros del Architect [V6](I-62-architect-review-v6.md) y [V7](I-62-architect-review-v7.md); registros del Coordinator
   [V1](I-62-coordinator-review-v1.md) … [V5](I-62-coordinator-review-v5.md).
4. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt); [contrato](I-62-portabilidad-coordinador-principal.md);
   [decisiones](../automation/decisions/I-62.md); [evidencia](../automation/evidence/I-62-evidence.md); [estado](../automation/state/I-62.yml).
5. Autoridades: LIFECYCLE; WORKFLOW; AGENTS; AUTOMATION_PLAN completo; ADR-0046; el Freeze de I-61 ([Proposal V9 de I-61](I-61-proposal-v9.md)); los
   documentos y esquemas de `docs/automation/agent-execution/`.

## 3. Delta V10 → V11

| Área | V10 | V11 |
|---|---|---|
| Acciones iniciales incompatibles (A62-V10-01) | la CI exacta del `Target` como «evidencia equivalente» de `dotnet test`, con el Controller de I-61 como precedente | **§20.3.1:** exención explícita y acotada a esa invocación y esa acción; la CI, como `HealthSignals` separada, nunca equivalente a Core local ni propagable a gates, Candidato, cierre o implementación; sin exención, no hay lanzamiento; sin precedente de sustitución (B.9, Anexo A, §18, G.1, C-41) |
| Vigencia de la autorización (A62-V10-03) | `Validity` exigida en cada punto para todo binding materializado; el cierre ARCHITECT_SATISFIED invalidaba el binding que lo acreditaba | **§20.5.1:** vigencia de acción (solo acciones nuevas) frente a acreditación histórica permanente; `AuthorizationRef` con `Commit` y `Blob` de un ancestro (B.2); `loop.action_validity` (B.8.8); regla determinista de los intentos en curso: LAUNCHING anterior al fin se completa, PLANNED o RESERVED pasan a CANCELLED_BEFORE_LAUNCH; I-S18 e I-P13; F.8 con cinco variantes; C-40 (e)-(i) |
| Contratos del Controller (A62-V10-04) | `delegation/v2` en §20.7, pero no en la lista de B.9 | **§20.7:** una fila por acción (PLAN → `rackcad-delegation/v2`; VERIFY → `rackcad-controller-verification/v2`), no intercambiables; B.9 admite exactamente los cinco contratos de la correspondencia; C-30 (g)-(i) |
| Fidelidad de los insumos (R62-FIDELITY-01..06) | no existía | **§20.3.3:** `CanonicalInputFidelity` por insumo y por prompt; codificación canónica UTF-8 según los bytes; preflight por el mismo camino de lectura sobre todos los caracteres no ASCII del corpus; única normalización permitida, el fin de línea; fallo cerrado (P-24, P-25); invalidación acotada por hallazgo con `PremiseRefs` (B.10.1); evidencia del invocador (`input-fidelity/v1`, B.1, B.9, B.8.8); C-42 con los ocho casos; FX-06; G.1; GAP-09 |
| A62-V9-04 | `review_rounds` ≤ `logical_requests` | **sin cambio de semántica**; una ilustración (L1 y L2 sobre X, L3 sobre X2) |
| Matriz | C-29..C-41 | C-29, C-30, C-40 y C-41 ampliados; **C-42** nuevo |

## 4. Disposición de la sesión por hallazgo (la sesión declara; el cierre lo decide quien revisa)

| Hallazgo o requisito | Corrección en V11 | Obligación de verificación |
|---|---|---|
| A62-V10-01 | §20.3.1 (clase ACTION_INCOMPATIBLE, tabla de RackCad, exención acotada); B.9 (`AllowedActions`, `HealthSignals`); Anexo A; §18 OD-1; G.1 | C-41 (a) |
| A62-V10-03 | §20.5.1 (`Validity`, vigencia de acción, acreditación histórica, intentos en curso); B.2; B.8.8 (`action_validity`, `reserved_utc`, `launching_utc`, I-S18, I-P13); §20.8; F.8; G.1 (A7); G.2 | C-40 (e)-(i); C-29 |
| A62-V10-04 | §20.7; B.9 (`OutputContract`) | C-30 (g)-(i) |
| R62-FIDELITY-01 | §20.3.3 (contrato, codificación canónica, preflight, corpus) | C-42 (1)-(3) |
| R62-FIDELITY-02 | §20.3.3 (normalización permitida frente a degradación semántica; fallo cerrado); §13 P-25 | C-42 (4), (5) |
| R62-FIDELITY-03 | §20.3.3 (invalidación acotada, `PremiseRefs`, independencia mecánica; conservador si no se demuestra); B.10.1; B.8.8 (`unaccredited`) | C-42 (5), (6) |
| R62-FIDELITY-04 | §20.3.3 (evidencia del invocador); B.10.0 (`InputFidelityEvidenceRef`, informativa); B.8.8 (`input_fidelity`) | C-42 (7), (8) |
| R62-FIDELITY-05 | §20.3.3 (transporte UTF-8 establecido y verificado; sin comando congelado); §13 P-24 | C-42 (4) |
| R62-FIDELITY-06 | C-42 con los ocho casos; contratos, estado, fallos, matriz y FX-06 (D.8) | C-42; C-39 |
| A62-V9-04 | §20.6 sin cambio de semántica | C-34, C-36 |

## 5. Riesgos y preguntas para la revisión

1. **Independencia mecánica.** La independencia de un hallazgo se demuestra con citas que coinciden con el texto canónico, o con secciones sin tramos
   degradados. ¿Es suficiente y verificable, o hace falta otro criterio?
2. **Normalización permitida.** Solo se admite el fin de línea. ¿Es demasiado estricta, por ejemplo con la normalización Unicode NFC/NFD?
3. **Preflight frente a captura.** El preflight reproduce el shell del runtime, pero no la captura interna del adapter, y la comprobación decisiva es la de
   después de la corrida. ¿Es aceptable que un resultado degradado ya consuma un intento?
4. **Intentos en curso.** Un LAUNCHING anterior a la revocación se completa. ¿Debería una revocación del Coordinator poder exigir además la detención
   inmediata, como una variante explícita de la decisión?
5. **Instante de las acciones.** Se declara en el QU y se contrasta con el push. ¿Basta para la caducidad por `Until`?

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor: incluye la orquestación autónoma (§20), con la materialización autorizada, los contratos por rol, el cierre de insumos con su exención acotada y la fidelidad de los insumos | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas (A, B, FX-04b, FX-06 con `codex-cli`) |
| OD-3 | autenticar Claude CLI | B; FX-06 si el Architect es `claude-cli` |
| OD-4 | sandbox de Codex para escritura | B y FX-04b |
| OD-5 | permiso de ensayo con la semántica única de §15, incluida la apertura y el consumo de FX-06 | F6 |
| OD-7 | remoto del fixture con CI | cierre de F6, FX-04b y FX-06 (paso 5) |

La exención de `dotnet test` para la invocación de esta revisión no es una OD del diseño: es un parámetro de la autorización de esa invocación.

## 7. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.
