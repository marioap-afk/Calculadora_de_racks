# I-49 — Consensus Freeze

Registro normativo del consenso de I-49 (ID22B, motor de expresiones paramétricas). **No** repite la Proposal: la
congela por su blob exacto, junto con el ADR aceptado por el Owner.

## 1. Autoridades

```text
Initiative       = I-49 / ID22B
Proposal Version = V6

V6 blob =
ef4db3aa400483ff25a8f39b2beb93708fa43d1a

V6 operational commit =
1ed93a0525ec11c5092df89c55cbf498c99f8e2a

ADR              = ADR-0038
ADR status       = ACCEPTED
ADR acceptance commit = edafade1188e1defae6edb146e61ca9ace34f3a2

Coordinator = AGREED WITH V6
Architect   = AGREED WITH V6
Owner ADR   = ACCEPTED

Consensus = FROZEN
```

Fuentes:

- [Proposal V6](I-49-proposal-v6.md), blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`; commit histórico
  `048a508e570e210467c3693a11ac4a487f383944` y commit operativo `1ed93a0525ec11c5092df89c55cbf498c99f8e2a`.
- [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md): propuesto en
  `a1605600d0951a87ae218724d82bec57bc3be723` y aceptado en `edafade1188e1defae6edb146e61ca9ace34f3a2`, con blob aceptado
  `59b8383be62c91526c0fe435af41121e1fd21429`.
- [Registro de I-49](../automation/decisions/I-49.md): §10, ADR propuesto; §11, aceptación del Owner («Acepto»,
  2026-09-13).
- [Contrato de I-49](I-49-motor-expresiones-parametricas.md).

## 2. Qué congela

- **Proposal V6**, en el blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, es el **contrato técnico vinculante** de I-49.
- **ADR-0038** es la **decisión arquitectónica aceptada**: D1–D25, incluidas la extensión de ADR-0034, Schema V-0, las
  expresiones como fuente de propiedad, la clasificación de fallos estructurales frente a semánticos, el `PlanReadSet`,
  la `RepairDecisionObservation` y la revisión acotada de la política V8-R05 de I-48.
- **Proposals V1–V5** son únicamente **historial**.
- **CLOSED**: CR-1…CR-5, FR-1 y RR-1; A-03; OPEN A; OPEN B; y las demás decisiones cerradas que enumera V6 §0.11.
- **Schema = V-0.**
- **No existe discrepancia material abierta** entre Coordinator, Architect y Owner.

## 3. Regla de invalidación

Cualquier cambio **material** durante la implementación que altere:

- D1–D25;
- la semántica de Expression;
- la identidad o la persistencia;
- Schema V-0;
- la clasificación de fallos estructurales frente a semánticos;
- el `PlanReadSet`;
- la `RepairDecisionObservation`;
- la extensión de ADR-0034;
- la revisión de V8-R05;
- los contratos R1, R2 y R3;
- cualquier decisión cerrada de V6;

**INVALIDA este freeze**. En ese caso:

```text
STOP IMPLEMENTATION
→ document finding
→ Coordinator review
→ Architect review if material
→ Owner decision if ADR changes
```

El contrato **no** se corrige en silencio desde la implementación.

La integración posterior de otras iniciativas puede mover líneas o archivos, pero **no** modifica por sí sola el
consenso: las citas de V6 se re-verifican contra el `main` vigente, y solo un cambio material de premisa activa esta
regla.

## 4. Gates

```text
G0 = CLOSED
G1 = CLOSED
G2 = CLOSED
G3 = CLOSED after this freeze passes CI

G4 = READY
G4 NOT STARTED
```

- Según V6 §8.1, G4 es **ADR + alineación documental**. El ADR ya está aceptado (G3B.2). Quedan para G4 la alineación
  del contrato (§1, §3.2 y §11.2) y de ROADMAP con la Proposal congelada, y los registros en `docs/ideas-futuras.md`
  que fija V6: L1–L5, la unificación del dominio de la ventana en el descriptor (P24.5) y el factor lb/ft→kg/m de
  `tools/` (P9.6).
- Este freeze **no** implementa nada ni abre G4: G4 empieza solo con una orden explícita, en otra ejecución.
- Los gates productivos G5–G12 siguen la secuencia, las evidencias y las compuertas de V6 §8.

## 5. Requisitos heredados para G4 y siguientes

- **Preflight contra el `main` vigente** y reconciliación de la rama con `main` según WORKFLOW antes del primer gate
  productivo (V6 §8.2, §12.2).
- **Revisión de las ramas activas** y, antes de editar cualquier archivo productivo, el procedimiento de colisión de
  [`decisions/I-49.md`](../automation/decisions/I-49.md) §8.
- **Serialización de archivos calientes** (WORKFLOW §7).
- **I-53 y `RackSelectiveWindow.xaml.cs`**, si aplica: el cambio de un miembro de I-49 (P23.13, G10) se serializa con
  el G5 de la unidad I-53S.
- **I-52 y la duplicación con restamp**, si aplica: la prueba de supervivencia de `expression` (P24.8) se escribe
  contra `RackDuplicationPlan` y el restamp vigentes en `main`.
- **Censos vigentes de comandos y ventanas**: I-49 no añade ninguno (P22.1, P23.14), y sus guardas se escriben contra
  el censo vigente al rebasar.
- **Prueba de composición `expression + DimensionViews`**: un Selectivo con una fuente `expression` en `PropertyValues`
  y `DimensionViews` presentes sobrevive a RACKEDITAR y a guardar y reabrir conservando ambos conceptos (ADR-0038,
  Consecuencias; V6 §12.1).
- **V6 y ADR-0038 exactos** —blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a` y ADR aceptado en
  `edafade1188e1defae6edb146e61ca9ace34f3a2`— son la autoridad de cada gate.
