# I-62 — Disposición del Owner y del Coordinator sobre la revisión del Architect de la Proposal V9 (registro)

```text
Emisores:         Owner (validez de la revisión) y Coordinator exclusivo de I-62 (aceptación de los hallazgos y orden de la Proposal V10)
Naturaleza:       decisión del Owner sobre la validez formal de una revisión, disposición del Coordinator y orden de corrección; no es dictamen del
                  Architect, Freeze, decisión de OD-6 ni autorización de implementación
Fecha:            2026-10-02 (UTC)
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — PROPOSAL V10 / ARCHITECT V9 REQUIRED CORRECTIONS»);
                  sin archivo de origen. Procedencia: transcripción literal recibida (8 170 bytes en UTF-8, SHA-256
                  c6d1e267b8f239b114284c91dd64a06450eb4cf14e24c7c584cc9864f9522b64), custodiada fuera del repositorio con la transcripción de la sesión
Objeto:           revisión del Architect de la Proposal V9 (commit b0725114e60319079c3abacf10542743ebb9303c, blob 831e3a6a87a242c872cfa0755f73e282f6c043c8);
                  registro I-62-architect-review-v9.md; resultado literal en docs/automation/evidence/I-62-architect-v9/R20261002T000511Z-ba10/
Disposición:      la revisión NO se acredita como revisión formal limpia final; V9 no se repite; A62-V9-01..06 = ACCEPTED REQUIRED; Proposal V10 autorizada
Estado:           Frozen: NO · OD-6 PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 vigente hasta que I-62 se integre
```

Registro redactado por la sesión responsable por orden del Owner y del Coordinator. La respuesta está en la [Proposal V10](I-62-proposal-v10.md) y en el
[paquete V10](I-62-architect-package-v10.md) §§3-4.

## 1. Decisión del Owner (resumen fiel)

- La revisión del Architect de V9 en `b0725114` **no** se acredita como la revisión formal limpia final, porque leyó tres archivos fuera de la lista cerrada de
  insumos.
- **No** se repite la revisión de V9.
- Su resultado queda custodiado como **evidencia técnica**.
- La Proposal V10 es la siguiente versión revisable formalmente. Se preservan todos los cierres previos.

## 2. Hallazgos aceptados por el Coordinator y dirección exigida (resumen fiel)

**A62-V9-01..06 = ACCEPTED REQUIRED.**

| Id | Problema | Dirección exigida |
|---|---|---|
| A62-V9-01 | `ReviewLoopAuthorization` permite rondas nuevas, pero no define cómo un binding ARCHITECT nuevo obtiene aceptación válida | la autorización debe poder autorizar la **materialización** de bindings Architect dentro de límites cerrados, sin decisión del Coordinator por ronda, fijando al menos: Role = ARCHITECT, acciones, capacidades mínimas, independencia, proveedores o celdas elegibles (o criterio cerrado), límites de modelo y effort, permisos, presupuesto, objeto o familia de versiones y alcance temporal. El Principal observa, comprueba, materializa, registra e invoca solo si se satisface exactamente; no puede relajar requisitos; sin binding que satisfaga, STOP / COORDINATOR_DECISION u OWNER según la autoridad. `DecisionRef`/`AuthorizationRef` adecuados, sin fingir una aceptación individual que no ocurrió. Casos: Architect nuevo tras CHANGES REQUIRED; binding no elegible rechazado; binding elegible materializado sin relevo humano |
| A62-V9-02 | `pending_invocation` mezcla significados | separar de forma durable INVOCATION_PLANNED, BUDGET_RESERVED, LAUNCHING / LAUNCH_UNCERTAIN, LAUNCHED, RESULT_RECEIVED y RESULT_INGESTED. Antes de lanzar: identidad lógica fijada y presupuesto reservado. Caídas: (A) antes del lanzamiento confirmado, reanudar sin consumir otro lanzamiento; (B) lanzamiento incierto, no relanzar hasta resolver terminación e identidad y, si no se acredita, conteo conservador; (C) resultado recibido no ingerido, ingestión idempotente; (D) resultado ingerido, nunca volver a contar. Recuperación por un Principal sucesor. Actualizar B.8.8, F.8, C-29 y C-34 |
| A62-V9-03 | sin disposición explícita por hallazgo | `FindingDisposition {FindingId, LineageId, State (OPEN \| CLOSED \| STILL_OPEN \| SUPERSEDED), SupersededBy[], Rationale}`. La omisión de un hallazgo OPEN lo deja OPEN; AGREED no cierra implícitamente un omitido; CLOSED exige autoridad del revisor; la rebaja REQUIRED → OPTIONAL, solo por quien puede según LIFECYCLE; SUPERSEDED conserva el linaje; un resultado de otro objeto o blob no cierra; un resultado de REVIEWER no cierra hallazgos del ARCHITECT. Actualizar I-S18, I-P13 y los negativos de C-38 |
| A62-V9-04 | identidad lógica y presupuesto ambiguos | separar `LogicalReviewRequestId` ≠ `InvocationId` ≠ `RunId`; la solicitud lógica es estable durante sus reejecuciones; cambiar RunId, InvocationId, proveedor, modelo, binding o Principal no crea presupuesto. Fórmula cerrada (ejemplo: MAX_REVIEW_ROUNDS = 3, MAX_ARCHITECT_LOGICAL_REQUESTS = 3, MAX_TRANSPORT_RERUNS_PER_REQUEST = 2, MAX_CORRECTIONS_PER_LINEAGE = 2, o equivalente consistente). La tercera reejecución con máximo 2 se rechaza antes del lanzamiento. Actualizar C-34 y C-36 |
| A62-V9-05 | un REVIEWER podía emitir un veredicto de LIFECYCLE | correspondencia cerrada: ARCHITECT / REVIEW_DESIGN → `rackcad-architect-review-result/v1` → AGREED \| CHANGES REQUIRED \| BLOCKED — OWNER DECISION; REVIEWER / REVIEW_CHANGE → `rackcad-reviewer-result/v1` → hallazgos y recomendaciones, nunca AGREED ni ARCHITECT_SATISFIED; EXECUTION_CONTROLLER → sus contratos existentes. Puede haber un sobre común, con la autoridad discriminada por Role + Action + OutputContract. Un resultado de REVIEWER usado para satisfacer al Architect: INVALID / STOP según la semántica. Casos en C-30 y C-38 |
| A62-V9-06 | F.8 contradice I-P13 | `loop.object` cambia a X2 en el punto durable que representa PUBLISHED; después CI_VERIFIED → REREVIEW_PENDING → ARCHITECT_INVOKED, sin cambio tardío. I-P13 y F.8 describen exactamente la misma transición. Caso positivo completo y negativo con cambio tardío |

## 3. Huecos y opcionales que se corrigen en V10 (resumen fiel)

| Id | Dirección exigida |
|---|---|
| GAP-07, cierre de insumos | una `RoleInvocation` distingue `CanonicalInputs` y `AllowedTransitiveInputs`. Si un insumo canónico contiene una obligación normativa de leer otros archivos, el conjunto efectivo se resuelve **antes** de lanzar. Para RackCad, si `AGENTS.md` exige leer `README.md`, `docs/HANDOFF.md` y `docs/ARCHITECTURE.md`: (A) incluirlos en `AllowedTransitiveInputs`, o (B) usar una autoridad superior válida que permita omitir esa lectura para esta acción. No se pide al revisor que viole AGENTS. Por defecto, A. `EffectiveInputClosure` se calcula y custodia antes del lanzamiento; el Architect puede leer los canónicos, los transitivos permitidos y las instrucciones de runtime o sistema declaradas; una lectura fuera del cierre es INVALID_REVIEW_CONTEXT. Añadir verificación |
| GAP-08, identidad del runtime | separar `ReviewerDeclaredIdentity` de `InvokerObservedRuntimeIdentity`. Modelo, effort y sesión, hilo o runtime efectivos solo se acreditan desde los hechos del invocador o del adapter, al nivel observado. El resultado puede referenciar `RuntimeEvidenceRef`, pero no autodeclarar MATCH. Una contradicción entre revisor y runtime es STOP S-04 o resultado inválido, según corresponda |
| O01, O02 | si siguen siendo locales: reparar la referencia a los 12 retos; incluir OV-I62-06 y la referencia a FX-06 donde corresponde |

## 4. Requisito de autonomía (resumen fiel)

No se degrada el objetivo añadido en V9. Tras las correcciones, el flujo objetivo sigue siendo Principal → Architect → CHANGES REQUIRED → el Principal corrige →
Architect nuevo → …, sin relevo del Owner ni del Coordinator mientras la `ReviewLoopAuthorization` siga vigente, el alcance sea válido, quede presupuesto y no
aparezca una decisión reservada.

## 5. Entrega ordenada y siguiente revisión formal

**Entrega**, producida de forma autónoma:
1. Proposal V10 completa, `Frozen: NO`;
2. paquete del Architect V10;
3. este registro;
4. decisiones, evidencia, estado y contrato propios;
5. delta V9 → V10;
6. commit y blobs exactos;
7. CI exacta.

Límites: no se modifican todavía los documentos normativos compartidos; sin implementación, pilotos, OD-6 ni Freeze. Tras la CI: STOP e informe único.

**Siguiente revisión formal:** V10 tendrá **una** revisión formal limpia del Architect. La invocación debe:
- usar una sesión o un proceso independiente, un clon limpio y solo lectura;
- usar el `EffectiveInputClosure` calculado, con toda lectura transitiva obligatoria de AGENTS;
- no usar la transcripción ni la memoria del autor;
- declarar el contexto;
- registrar el `RuntimeEvidenceRef` desde el invocador.

Con cero REQUIRED, el Owner decide OD-6 y después viene el Consensus Freeze.

## 6. Estado declarado por la disposición

```text
V9 Architect run:     TECHNICAL FINDINGS ACCEPTED · FORMAL CLEAN REVIEW = NOT ACCREDITED
Open REQUIRED:        A62-V9-01, A62-V9-02, A62-V9-03, A62-V9-04, A62-V9-05, A62-V9-06
V10:                  AUTHORIZED
OD-6:                 PENDING
FREEZE:               NOT_AGREED
IMPLEMENTATION AUTHORIZATION = NO · I-61 REMAINS ACTIVE UNTIL I-62 IS INTEGRATED
```
