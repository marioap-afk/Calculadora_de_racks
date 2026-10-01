# I-52 — CT-21D Owner decisions: Q-O1..Q-O5 and the non-governing host gate (verbatim record)

> Verbatim copy of the Owner's message of 2026-09-30 (chat, after the Owner decision package `docs/initiatives/I-52-ct21d-owner-decision-package-v1.md`). Recorded by the implementer; the decisions log entry is section 233. Not edited. The only addition is this header.

```text
OWNER DECISIONS — I-52 / CT-21D

Como Owner, tomo las siguientes decisiones para BA-10 y para abrir la
compuerta de host no gobernante.

==================================================
Q-O1 — CLASS A
==================================================

p_A = 0.10
alpha_A = 0.01

Interpretación de política:

Para un modo de fallo de Clase A cuya probabilidad por corrida sea al menos
10%, la campaña de repetición debe diseñarse para tener al menos 99% de
probabilidad de observarlo.

Usando:

N >= ln(alpha) / ln(1 - p)

el mínimo correspondiente es:

N_A = 44

No generalices automáticamente 44 a todos los grupos:
aplícalo donde BA-10/contrato diga que esta política de repetición gobierna.

==================================================
Q-O2 — CLASS B
==================================================

p_B = 0.10
alpha_B = 0.05

No es igual que Clase A.

Interpretación:

Para un modo de Clase B con probabilidad por corrida >=10%, usar al menos
95% de probabilidad de detección.

Resultado aproximado:

N_B = 29

Clase B sigue siendo fail-closed según el contrato.
Este parámetro estadístico no cambia su semántica contractual.

==================================================
Q-O3 — EXPECTED-EVENT-SET DETERMINISM
==================================================

p_det = 0.05
alpha_det = 0.01

Interpretación:

Quiero sensibilidad a una inestabilidad del expected-event-set que aparezca
en al menos 5% de las corridas, con al menos 99% de probabilidad de
observarla.

Resultado aproximado:

N_det = 90

Esto es deliberadamente más estricto que Clase B general porque una
inestabilidad del modelo de eventos puede contaminar la autoridad de E-12.

==================================================
Q-O4 — FALSE-REJECTION BOUND
==================================================

u = 0.05
c = 0.95

Objetivo:

la campaña de clean runs debe permitir sostener una cota superior unilateral
de false-rejection rate <=5% con 95% de confianza, bajo el método estadístico
sellado por BA-10.

Si se utiliza el caso de cero falsos rechazos con la fórmula binomial
correspondiente, el orden de magnitud es:

N_clean = 59

No conviertas este número ilustrativo en autoridad si BA-10 especifica otro
método estadístico exacto.

El método final y el cálculo deben quedar reproducibles en BA-10.

==================================================
Q-O5 — MACHINE POLICY
==================================================

Confirmo la política de machine class de BA-10 §2.2 con esta restricción:

Para la primera caracterización CT-21D no se intenta demostrar portabilidad
a una clase amplia de máquinas.

La autoridad se liga a UNA máquina física concreta y estable.

Delego al CAD manager la selección de esa máquina.

El CAD manager debe registrar antes de cualquier host run:

- machine identity;
- Windows version/build;
- AutoCAD version/build;
- AutoCAD profile identity;
- relevant hardware identity/class;
- user/profile identity según el contrato;
- loaded-module/plugin baseline;
- configuration inputs requeridos por la tuple.

Durante la campaña:

- no updates;
- no profile changes;
- no plugin changes;
- no machine-class changes.

Cualquier cambio material:
NEW MACHINE/SESSION TUPLE
y no transferencia silenciosa de evidencia.

No interpretar esta decisión como evidencia de que otras máquinas son
equivalentes.

==================================================
HOST GATE — BA-11 §3.3
==================================================

HOST_GATE_BA11_3_3 = AUTHORIZED

Pero SOLO para:

NON_GOVERNING HOST CHARACTERIZATION / BASELINE-PREPARATION EVIDENCE

en la máquina designada por el CAD manager y en sesiones que satisfagan la
tuple exacta.

Esta autorización incluye únicamente las actividades host que el workflow
actual ya identifica como necesarias para cerrar artefactos de baseline,
por ejemplo:

- library-DWG entity census;
- TOL_SCALE non-governing characterization;
- fixture instantiation/feasibility checks;
- machine/session fact capture;
- otros K-* explícitamente autorizados por BA-11 §3.3.

NO autoriza por sí sola:

- governing CT-21D campaign;
- ADMISSION_EVALUATION;
- product admission;
- G3 reopening;
- CT-50;
- Owner Act 2;
- replacement Freeze;
- acceptance of W-scan residual;
- acceptance of final reduced guarantee.

==================================================
HOST EXECUTION DISCIPLINE
==================================================

Before first host run:

1. CAD manager designates exact machine.
2. Record exact machine/session tuple.
3. Coordinator verifies requested scenario belongs to the authorized
   non-governing host gate.
4. Use exact build/package required by the current baseline-preparation
   authority.
5. Preserve raw evidence.
6. No result is governing merely because it ran in AutoCAD.

Any FAIL / UNKNOWN / INVALID remains governed by the existing contract.
No automatic retries.

==================================================
NEXT WORK
==================================================

With these Owner decisions:

1. Produce BA-07 V8 for V7-01.
2. Produce BA-10 V8 with the values above.
3. Recalculate the cost/run memo from the actual catalog and the policies
   above.
4. Continue the candidate chain in the approved sealing order.
5. Prepare the exact host-gate execution package for Coordinator review
   before starting the first authorized non-governing host run.
6. Continue autonomously through Architect/Coordinator gates where no
   further Owner decision is reserved.

Do not ask the Owner again for p/alpha/u/c unless a later reviewed contract
change makes these parameters inapplicable.

OWNER_ACT2 remains reserved.
```
