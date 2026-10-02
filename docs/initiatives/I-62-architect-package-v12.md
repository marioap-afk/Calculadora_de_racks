# I-62 — Paquete de revisión del Architect (Proposal V12)

```text
PROPOSAL V12 — NOT REVIEWED
Architect      = REVIEW REQUIRED: UNA revisión formal limpia, con la fidelidad probada por el mismo camino y lecturas acotadas para los archivos grandes
Coordinator    = A62-V10-03 (residuo) y A62-V11-01 ACCEPTED REQUIRED; todo lo demás CLOSED sigue CLOSED (registro I-62-architect-review-v11-disposition.md)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V12 §11.4); sin decisión no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v12.md                        blob 320cecc9bd1b68112509e510b97c768b6d505ba2
  docs/initiatives/I-62-architect-review-v11-disposition.md    blob 9570cceaf7059aaf438b91aa769c906e4a136ce8
Versión anterior:
  V11: commit 26127a69a7dbc1324566b081381cd11db6b20f35, blob 3e8fa9d8eb8a875069224e8ed7fd5850145e4d0e (Architect: CHANGES REQUIRED, revisión formal
       acreditada)
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v12.md` = `320cecc9…`. Si no coincide, revisa la versión designada o rechaza la
> discordancia. Este paquete no lleva su propio blob.

## 0. Condiciones de la revisión limpia (disposición del Owner y del Coordinator, «NEXT REVIEW»)

V12 tendrá **una** revisión formal limpia del Architect. Usa el mecanismo de fidelidad ya probado en la revisión de V11, con una mejora: **lecturas acotadas
por rangos para los archivos muy grandes**, en lugar de capturas enormes del archivo completo (GAP-10). Antes de lanzar:
- el `EffectiveInputClosure` recalculado sobre el commit exacto (§20.3.1);
- el preflight de fidelidad por el **mismo** camino de lectura (§20.3.3), sobre todos los caracteres no ASCII del corpus del cierre;
- la evidencia de identidad del runtime (§20.3.2);
- la exención acotada de `dotnet test` si sigue haciendo falta (§20.3.1), con la CI de publicación solo como señal separada.

**Formas de lectura acreditadas en la revisión de V11** (evidencia `I-62-architect-v11/…/input-fidelity-preflight.json`). Se vuelven a probar sobre el
commit exacto:
- **A, archivo completo** (`cmd /c type <ruta con barras invertidas>`): solo para los archivos pequeños;
- **B, rango** (un `pwsh` anidado tras `chcp 65001`, con `Get-Content … | Select-Object -Skip N -First M`): **la forma por defecto para los archivos grandes**,
  por ejemplo `docs/HANDOFF.md` o la Proposal;
- **C, búsqueda** con salida en cadenas planas `número:línea`;
- **Git**, directamente.

No se fija ningún número de líneas: el requisito observable es que el envoltorio de cada premisa usada llegue fielmente. Tras la corrida, la comprobación de
fidelidad y la de los envoltorios de premisa (§20.3.3) son las decisivas.

**Cierre efectivo previsto.** El invocador lo recalcula sobre el commit exacto:
- **`CanonicalInputs`:** los de §2;
- **`AllowedTransitiveInputs`:** los de la revisión de V11, que en esas rutas siguen iguales a `main`:
  - `docs/HANDOFF.md`, `README.md` y `docs/ARCHITECTURE.md`;
  - `docs/context-packs/README.md` y `documentation-governance.md`;
  - `docs/ROADMAP.md` y `docs/FOUNDATIONS.md`;
  - `docs/initiatives/README.md` y `docs/initiatives/PROMPT_TEMPLATES.md`.

**Forma del resultado:** en lo posible, `rackcad-architect-review-result/v1` (Proposal V12 B.10.1), como representación experimental. Cada `PremiseRef` lleva
`LineStart`, `LineEnd` y la **proposición normativa completa**. `OpenFindings`:
- **REQUIRED aceptados:** A62-V10-03 (residuo) y A62-V11-01;
- **OPTIONAL abiertos, no bloquean:** O-V7-01, O-V7-02, O-05 y O-07.

Hay que reevaluar R62-FIDELITY-02, -03 y -06, que la revisión de V11 dejó en PARTIALLY_SATISFIED por A62-V11-01. Hay que confirmar que todo lo CLOSED sigue
CLOSED, salvo que V12 introduzca una contradicción nueva:
- A62-V10-01, A62-V10-04 y A62-V9-04;
- A62-V9-02, -03, -05 y -06;
- A62-V7-01..03 y A62-V6-01..03;
- los cierres previos del Coordinator;
- SUPERSEDED: A62-V9-01 → A62-V10-03.

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Modo:      SEPARATE SESSION (declarado); si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

Con cero REQUIRED: el Owner decide OD-6, el Coordinator confirma el acuerdo sobre la V12 exacta, y llega el Consensus Freeze.

## 2. Lectura (insumos canónicos)

1. [Proposal V12](I-62-proposal-v12.md) completa; [Proposal V11](I-62-proposal-v11.md) y su [paquete](I-62-architect-package-v11.md), para verificar el delta.
2. [Disposición del Owner y del Coordinator sobre la revisión de V11](I-62-architect-review-v11-disposition.md); [registro de la revisión de
   V11](I-62-architect-review-v11.md) y su evidencia en `docs/automation/evidence/I-62-architect-v11/R20261002T140232Z-cdc5/`:
   - [`output.json`](../automation/evidence/I-62-architect-v11/R20261002T140232Z-cdc5/output.json);
   - [README](../automation/evidence/I-62-architect-v11/R20261002T140232Z-cdc5/README.md);
   - [`input-fidelity-postrun.json`](../automation/evidence/I-62-architect-v11/R20261002T140232Z-cdc5/input-fidelity-postrun.json).
3. Las disposiciones y los registros anteriores: [V10](I-62-architect-review-v10-disposition.md), [V9](I-62-architect-review-v9-disposition.md),
   [requisito R62-AUTO](I-62-coordinator-requirement-auto.md), y los registros del Architect [V6](I-62-architect-review-v6.md) y
   [V7](I-62-architect-review-v7.md).
4. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt); [contrato](I-62-portabilidad-coordinador-principal.md);
   [decisiones](../automation/decisions/I-62.md); [evidencia](../automation/evidence/I-62-evidence.md); [estado](../automation/state/I-62.yml).
5. Autoridades: LIFECYCLE; WORKFLOW; AGENTS; AUTOMATION_PLAN completo; ADR-0046; el Freeze de I-61 ([Proposal V9 de I-61](I-61-proposal-v9.md)); los
   documentos y esquemas de `docs/automation/agent-execution/`.

## 3. Delta V11 → V12

| Área | V11 | V12 |
|---|---|---|
| Intento en LAUNCHING al fin de la vigencia (A62-V10-03, residuo) | sin transición legal LAUNCHING → CANCELLED_BEFORE_LAUNCH; B.1 devolvía a BUDGET_RESERVED aunque I-S18 lo prohíbe con ENDED | **§20.5.1:** LAUNCHING = intención y `RunId` durables, arranque sin confirmar. Decide la evidencia del invocador: si arrancó antes de `ended_utc`, acreditación histórica; si no arrancó, CANCELLED_BEFORE_LAUNCH (legal), sin ingestión posterior, con la reserva contada y sin lanzamiento nuevo; si es indeterminado, LAUNCH_UNCERTAIN; si arrancó después, UNAUTHORIZED_LAUNCH (P-20). §20.6 (LAUNCHING, cancelación, caso B.1), B.8.8 (`ended_utc`, `not_started_evidence`, I-S18, I-P13), F.8 (cuatro variantes), C-38, C-40 (j)-(n) y G.2 |
| Independencia de las premisas (A62-V11-01) | la cita debía coincidir con una subcadena canónica de la sección | **§20.3.3:** `PremiseRefs` con `LineStart`, `LineEnd` y la proposición completa. `PremiseEnvelope` mecánico: unidad estructural completa, expresión con operadores, fila con su cabecera, cadena de títulos y destinos de referencias cruzadas. La independencia exige que todo envoltorio quede fuera de los tramos degradados y sea fiel. `DegradedSpans` alineado, con supresiones y con las clases NEGATION, OPERATOR, QUANTIFIER, MODALITY, IDENTIFIER, TABLE_ASSOCIATION, SCOPE, CROSS_REFERENCE, LETTER, STRUCTURE y OTHER. Si no se demuestra, INVALID_PREMISE; si no se puede determinar, toda la revisión es inválida. Evidencia del invocador (`premise_independence`); B.10.1; C-42 (a)-(g) |
| GAP-10 | medido en la evidencia §25 | documentado en §20.3.3 y §20.11 como evidencia de transporte; se prefieren lecturas acotadas, sin congelar comandos ni números de líneas |

## 4. Disposición de la sesión por hallazgo (la sesión declara; el cierre lo decide quien revisa)

| Hallazgo | Corrección en V12 | Obligación de verificación |
|---|---|---|
| A62-V10-03 (residuo) | §20.5.1 (regla determinista con tabla de destinos); §20.6 (LAUNCHING, CANCELLED_BEFORE_LAUNCH, caso B.1); B.8.8 (`ended_utc`, `not_started_evidence`, `UNAUTHORIZED_LAUNCH`, I-S18, I-P13); F.8; G.2 | C-40 (j) arrancó antes del fin, válido; (k) nunca arrancó, CANCELLED_BEFORE_LAUNCH; (l) indeterminado, LAUNCH_UNCERTAIN; (m) negativo: cancelado que produce un resultado, ingerirlo es INVALID; (n) arrancado después del fin. C-38: la ingestión de un intento cancelado |
| A62-V11-01 | §20.3.3 (proposición completa, `PremiseEnvelope`, independencia, análisis por clases, invalidación parcial o total, evidencia del invocador); B.8.8 (`premise_independence`); B.10.1 (`PremiseRefs` con líneas) | C-42 (a)-(g) |

## 5. Riesgos y preguntas para la revisión

1. **Envoltorio.** El envoltorio toma la unidad estructural entera, que contiene la oración completa. ¿Basta para el texto en prosa con varias oraciones, o
   hace falta una regla más fina?
2. **Referencias cruzadas.** Solo entran los destinos que la evidencia del hallazgo también cita. ¿Basta, o debe entrar todo destino referenciado dentro del
   envoltorio?
3. **Instante de arranque.** La comparación con `ended_utc` usa el instante observado por el invocador (PID + `CreationDateUtc`, o el `session_meta` del
   runtime). ¿Es suficiente cuando el reloj del host y el instante del commit difieren?

## 6. Decisiones del Owner y su frontera real

Sin cambios respecto de V11: OD-6 (acuerdo y Freeze), OD-1 (READY-03 y vigencia), OD-2, OD-3, OD-4, OD-5 y OD-7 (§18 de la Proposal). La exención de
`dotnet test` para la invocación de esta revisión no es una OD del diseño: es un parámetro de la autorización de esa invocación.

## 7. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.
