# I-49 — Consensus Freeze V6 + A1 + ADR-0040

Registro normativo del consenso vigente de I-49 (ID22B, motor de expresiones paramétricas). **No** repite la Proposal,
el Amendment ni el ADR: los congela por sus blobs exactos, junto con la aceptación del Owner. Es un freeze **nuevo**: el
[Consensus Freeze histórico](I-49-consensus-freeze.md) sobre V6 y ADR-0038 **no** se reescribe.

## 1. Autoridades

```text
Initiative = I-49 / ID22B

Proposal = V6
V6 blob =
ef4db3aa400483ff25a8f39b2beb93708fa43d1a

Amendment = A1
A1 blob =
d62019088b9e7a140d5066799afe6ace6db303ba
A1 commit =
c7fabbef7b4d65728bb7bef33a1b3b87441fddb2   (operativo)
71268eb09a3e04231da4e25b985e175c977f8c9e   (revisado)

ADR        = ADR-0040
ADR status = ACCEPTED
ADR proposal blob =
dd1ad8018a2b6643bd1972907e0a35cd7d402fd1
ADR acceptance commit =
2eeeab1a2fce3fb33c8151f8ef05d0d109531415   (original; CI 34788811598)
8f6d5831eccf08fab3535ce1dc29f87882624fbb   (operativo, sobre main 104ef3a; CI 34789541390)
ADR accepted blob =
9855a6b32a82954839ba2ae67cf351eae118c8ef

Previous ADR = ADR-0038 — REPLACED BY ADR-0040

Previous freeze =
364d6c06e44273a63a7b6f6509daf357611ea77a
Previous freeze status =
historical; invalidated for further implementation by A1

Coordinator = AGREED WITH V6
              AGREED WITH A1
Architect   = AGREED WITH V6
              AGREED WITH A1
Owner       = ADR-0040 ACCEPTED

Consensus = FROZEN — V6 + A1 + ADR-0040
```

Fuentes:

- [Proposal V6](I-49-proposal-v6.md), blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`: commit operativo
  `cd8588017eca13bd14b2724477d6014211b85216`, tras la reconciliación de A1-R3. El tag
  `archive/i-49-a1r3-pre-rebase-2eeeab1` conserva la rama anterior a esa reconciliación, con
  `8189c7df30ae6c3faaf4138bef02a4161176a2c1`; el tag `archive/i-49-a1-pre-rebase-71268eb`, la rama anterior a A1-R1, con
  el commit operativo `1ed93a0525ec11c5092df89c55cbf498c99f8e2a`; y el commit histórico
  `048a508e570e210467c3693a11ac4a487f383944` es anterior al rebase de G3A.
- [Amendment A1](I-49-proposal-v6-amendment-a1-text-guard.md), blob `d62019088b9e7a140d5066799afe6ace6db303ba`: revisado
  en `71268eb09a3e04231da4e25b985e175c977f8c9e`; tras el rebase de A1-R1, `cae7a9f5ae10e9c86c16a29000c0933e2187873f`; y,
  tras la reconciliación de A1-R3, `c7fabbef7b4d65728bb7bef33a1b3b87441fddb2`.
- [ADR-0040](../adr/0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md): propuesto en
  `f8dddb6fc570f8294a8c960a6fe5060df1e94a2a` (blob `dd1ad8018a2b6643bd1972907e0a35cd7d402fd1`; tras la reconciliación,
  `cc75a18b154ecf9de105607c8ceff9dea072d435`) y aceptado en `2eeeab1a2fce3fb33c8151f8ef05d0d109531415` (tras la
  reconciliación, `8f6d5831eccf08fab3535ce1dc29f87882624fbb`), con blob aceptado
  `9855a6b32a82954839ba2ae67cf351eae118c8ef`. La aceptación solo cambió su encabezado y su bloque «Decisión del Owner»:
  desde «Contexto» hasta el final, el texto aceptado es byte a byte el del blob propuesto.
- [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md): aceptado el 2026-09-13 (blob
  `59b8383be62c91526c0fe435af41121e1fd21429`) y marcado `reemplazado por ADR-0040` en el mismo commit que acepta
  ADR-0040 (blob `ab10130b7ce69dcd15ad196017beb27acbfb4c5a`), con solo su Estado cambiado y una nota posterior fechada.
- [Consensus Freeze histórico](I-49-consensus-freeze.md): versionado en `364d6c06e44273a63a7b6f6509daf357611ea77a`; tras
  el rebase de A1-R1, `788b00f1d3e70ad43f0a72ce0b9db7f5f88313db`; y, tras la reconciliación de A1-R3,
  `5fa71409756d6cb30cecfea70b85d4c22c6bbf17`. Blob `49e8041c489d01b5f8dc20b1c7028d46488933ee`.
- **Reconciliación con `main` (A1-R3).** `main` avanzó durante A1-R3 hasta `104ef3a1b1df249d6e0a56dc4ad3846912b24f12`
  (merge de I-53S E2). Antes de este freeze, la rama se rebasó sobre ese `main`, sin commits de merge. V6, A1, ADR-0040,
  ADR-0038, el freeze histórico, el registro de I-49 y los archivos de G5 conservan sus blobs. El único conflicto, al
  final de `ideas-futuras.md`, se resolvió conservando lo de `main` y añadiendo después el bloque de I-49.
- [Registro de I-49](../automation/decisions/I-49.md): §12, Amendment A1 y ADR-0040 propuesto; §13, aceptación del
  Owner de ADR-0040 («Acepto», 2026-09-13) y reemplazo de ADR-0038.
- [Contrato de I-49](I-49-motor-expresiones-parametricas.md).

## 2. Qué congela

- **Proposal V6**, en el blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, sigue siendo el **contrato técnico base** de
  I-49.
- **Amendment A1**, en el blob `d62019088b9e7a140d5066799afe6ace6db303ba`, modifica **solo** la guarda de recurso del
  parser de P1.9/D8 y los lugares que enumera A1 §3.3.
- **Precedencia.** En esa materia, **A1 manda sobre el texto original de V6**. Fuera de ella, **V6 manda sobre cualquier
  resumen posterior**, este freeze incluido; ante una duda de detalle, también sobre ADR-0040, como fija su «Autoridad
  normativa».
- **ADR-0040** es la **decisión arquitectónica aceptada** y la **sucesora completa** de ADR-0038: D1–D25, con D1–D7 y
  D9–D25 sin cambio de semántica y D8 con la guarda de A1, incluidas la extensión de ADR-0034, Schema V-0, las
  expresiones como fuente de propiedad, la clasificación de fallos estructurales frente a semánticos, el `PlanReadSet`,
  la `RepairDecisionObservation`, la revisión acotada de la política V8-R05 de I-48 y la nueva guarda de complejidad
  sintáctica de D8.
- **ADR-0038** queda como **historial reemplazado**: no es autoridad para implementar.
- **El Consensus Freeze histórico** es evidencia del consenso sobre V6 y ADR-0038; A1 activó su regla de invalidación
  y ya no autoriza seguir implementando.
- **Proposals V1–V5** son únicamente **historial**.
- **No existe ninguna discrepancia abierta** entre Coordinator, Architect y Owner.

## 3. Decisiones cerradas

```text
CR-1..CR-5 = CLOSED
FR-1       = CLOSED
RR-1       = CLOSED
A-03       = CLOSED
OPEN A     = CLOSED
OPEN B     = CLOSED
Schema     = V-0

A1 P1.9/P2.5 tension = CLOSED

Parser resource guard:
NO character validity limit
MaxSyntacticTokens initial = 4096
minimum                    = 6 × MaxBoundNodes
MaxBoundNodes              = 256
minimum current            = 1536

P2.5 = PRESERVED STRONG
```

- Siguen cerradas las demás decisiones que enumera V6 §0.11, leídas con A1 §3.3: su fila «Texto de entrada ≥ 4000» se
  lee como la guarda de tokens de A1.2, y los límites de nodos, argumentos y profundidad siguen CLOSED.
- `MaxBoundNodes` nombra aquí el máximo normativo de nodos del `BoundExpression` de D8 (nodos ≤ 256): el mínimo
  `6 × MaxBoundNodes` es el que A1.2 escribe «6 × máximo normativo de nodos». Como en V6 §1.2, los nombres de tipos y
  miembros son ilustrativos.
- `MaxSyntacticTokens` es un valor de implementación del núcleo: cuenta los tokens con la granularidad normativa de
  A1.3, corta durante el lexing con un único `LimitExceeded` de tipo `SyntacticTokenCount` (A1.4) y no sustituye a los
  límites normativos del `BoundExpression`, que se validan tras el bind (A1.5). La cota del formatter es
  `tokens(Format(b)) ≤ 6n - 3 ≤ 1533` (A1 §4). `TextLength` sale del contrato sin reutilizar su valor numérico (A1 §6).
- P2.5 permanece literalmente fuerte (A1 §7). Los nombres no cambian: no se añade ningún límite de longitud y el nombre
  solo no puede estar en blanco (A1 §8).

## 4. Regla de invalidación

Cualquier cambio **material** durante la implementación que altere:

- D1–D25;
- la Proposal V6;
- el Amendment A1;
- la guarda de recurso del parser;
- P2.5;
- los nombres;
- la semántica de Expression;
- la identidad;
- la persistencia;
- Schema V-0;
- la clasificación de fallos estructurales frente a semánticos;
- el `PlanReadSet`;
- la `RepairDecisionObservation`;
- la extensión de ADR-0034;
- la revisión acotada de V8-R05;
- los contratos R1, R2 y R3;
- cualquier decisión cerrada de V6 o de A1;

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
consenso: las citas de V6, A1 y ADR-0040 se re-verifican contra el `main` vigente, y solo un cambio material de premisa
activa esta regla.

## 5. Gates

```text
G0 = CLOSED
G1 = CLOSED
G2 = CLOSED
G3 = CLOSED
G4 = CLOSED
G5 = CLOSED

G6 = READY AFTER THIS FREEZE HAS GREEN CI
G6 NOT RESUMED IN THIS COMMIT
```

- G4 cerró en `82aa61b20c59f4377b17f57793eac8edcfd9079f` y G5 en `c4880af66586c535954b0ba0bff7b64b887bddb4`; tras la
  reconciliación de A1-R3 están en `535b5bf4ad37ad8b9005d37374e7a06e15e2ed53` y
  `977f87321d705c39ad6d538aa41f2b61ed98aee9`. G5 sigue CLOSED y no se revierte (A1 §10).
- G6 se detuvo en G6.0 por el hallazgo P1.9/P2.5 CONTRACT TENSION, sin producción. Este freeze recoge su resolución: A1
  acordado por Coordinator y Architect y ADR-0040 aceptado por el Owner —las condiciones de A1 §12—, con ADR-0038 ya
  reemplazado.
- G6 pasa a `READY` solo cuando la CI de este freeze esté verde. Este freeze **no** implementa nada ni reanuda G6: G6
  empieza solo con una orden explícita, en otra ejecución. A1-R3 no cambia producción.
- Los gates productivos G6–G12 siguen la secuencia, las evidencias y las compuertas de V6 §8, leídas con A1 §3.3.

## 6. Precondiciones de G6 reanudado

Antes de reanudar G6:

- `git fetch --all --prune`;
- la rama reconciliada con el `main` vigente según WORKFLOW (V6 §8.2, §12.2);
- las ramas activas revisadas;
- el procedimiento de colisión de [`decisions/I-49.md`](../automation/decisions/I-49.md) §8, antes de editar cualquier
  archivo productivo;
- V6 exacta: blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`;
- A1 exacto: blob `d62019088b9e7a140d5066799afe6ace6db303ba`;
- ADR-0040 `aceptado`: blob `9855a6b32a82954839ba2ae67cf351eae118c8ef`, aceptado en
  `2eeeab1a2fce3fb33c8151f8ef05d0d109531415` (operativo `8f6d5831eccf08fab3535ce1dc29f87882624fbb`);
- este freeze exacto, tal como lo versiona el commit que lo crea.

**Primer trabajo de G6**: RED → GREEN para sustituir la guarda de G5:

```text
TextLength / MaxTextLength
→
SyntacticTokenCount / MaxSyntacticTokens
```

Sin revertir G5 completo: G6 reanudado cambia solo la guarda que implementó G5 —los lugares y las pruebas que enumera
A1 §10—, sin tocar ningún otro comportamiento de G5, e implementa y prueba los requisitos de complejidad y seguridad de
A1 §5.

Siguen vigentes los requisitos heredados del freeze histórico (§5), con el de I-53S actualizado a su integración:

- **Serialización de archivos calientes** (WORKFLOW §7).
- **I-53S y `RackSelectiveWindow.xaml.cs`**: I-53S E2 ya está integrada en `main` (`104ef3a`) y cambió esa ventana.
  Por el orden de archivo caliente del Coordinador ([decisiones de I-53](../automation/decisions/I-53.md) §12.3;
  [HANDOFF](../HANDOFF.md)), el G10 de I-49 —el cambio de `Describe(PropertyId, string)` (P23.13, D21)— se libera solo
  tras el merge de I-53S, su CI posterior, las coberturas del `MERGE_SHA` y del Candidato y la limpieza completa.
  Antes de editar, G10 vuelve a localizar `Describe` por firma y semántica contra la ventana vigente en `main`,
  verifica P23.13 y D21 y corre sus RED y guardas.
- **I-52 y la duplicación con restamp**, si aplica: la prueba de supervivencia de `expression` (P24.8) se escribe
  contra `RackDuplicationPlan` y el restamp vigentes en `main`.
- **Censos vigentes de comandos y ventanas**: I-49 no añade ninguno (P22.1, P23.14), y sus guardas se escriben contra
  el censo vigente al rebasar.
- **Prueba de composición `expression + DimensionViews`**: un Selectivo con una fuente `expression` en `PropertyValues`
  y `DimensionViews` presentes sobrevive a RACKEDITAR y a guardar y reabrir conservando ambos conceptos (ADR-0040,
  Consecuencias; V6 §12.1).
- **V6, A1 y ADR-0040 exactos** son la autoridad de cada gate.
