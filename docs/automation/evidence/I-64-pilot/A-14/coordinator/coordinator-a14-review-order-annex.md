# I-64 A-14 — Anexo a la orden de la revisión del Architect: lo que pasó

**La orden custodiada no se edita.** Es `A-14-review/R20261123T212253Z-ab02/coordinator-a14-review-order.md`, con SHA-256
`c37b9e780e689206a0585482f9b385a672239a539e8f87846467fe3de15008cb`, el que fija su manifiesto. Se custodió tal como se lanzó la
revisión. Este anexo registra, sin reescribir la historia, qué pasó con:
- su cabecera;
- sus campos `[COORDINATOR]`;
- el prompt.

## La cabecera «PREPARADA, SIN EMITIR»

**La orden la emitió la quinta ronda del Owner como Coordinator** (registro literal de A-14, quinta ronda; la decisión está custodiada en
`F4-ENVIRONMENT/snapshot-M/owner-config-snapshot-m-decision.md`):
- **La respuesta:** «Sonda neutra, luego M (Recomendado)».
- **Su descripción:** «Como con la L: una sonda neutra con gpt-6-luna en solo lectura. Si da PASS, esta configuración queda como Snapshot
  M y se lanza la revisión del Architect.»

**Lo que siguió:**
- la sonda `R20261123T211742Z-282c` dio PASS;
- la orden se ajustó a la M con un reemplazo exacto (su sección «Ajuste a la Snapshot M»);
- se lanzó **una** revisión, `R20261123T212253Z-ab02`, sobre `ab0170b7`.

La cabecera de preparación quedó en el texto custodiado. Desde la quinta ronda, la orden está **EMITIDA** y **CONSUMIDA** por esa
revisión.

## Los campos `[COORDINATOR]`

No se respondieron uno a uno antes del lanzamiento. La revisión se lanzó con la propuesta de la sesión en cada campo, que es la de A-14 §9
y §12.

**Después vinieron dos cosas:**
- **El Architect** las evaluó en sus respuestas, sin ningún hallazgo.
- **El Coordinator** dio AGREED (sexta ronda, `coordinator-a14-agreed.md`) «sobre la A-14 tal como está en ab0170b7». Ese acuerdo no
  responde a cada campo por separado.

| Campo de la orden | Texto con el que se lanzó | Lo que pasó |
|---|---|---|
| Q-C3, materialidad | M-01..M-07 = YES; M-08 = YES (por prudencia); NEW ARCHITECTURE (A-14 §9) | Architect: M-01..M-08 = YES; Q01 = YES, conforme. Coordinator AGREED sobre A-14 en `ab0170b7`, que incluye §9 |
| Q-C3, autoridad | Owner + Architect + Coordinator; Owner por lo OWNER-RESERVED (§11); Architect + Coordinator por las M materiales | Architect: Q02 = YES, conforme. Se dieron los tres acuerdos: Architect AGREED, Coordinator AGREED y Owner SATISFIED (`owner-a14-satisfied.md`) |
| §12, fila 2 | tramo 1 = la unidad I-64; los tramos siguientes, unidades nuevas que se nombran al arrancar (Q-C4) | Architect: Q10 = YES, conforme. El Owner firmó la materia (11) con «tramo 1 = fases 0 a 4, como se propone» |
| §12, fila 3 | Applies-to: all; solo se amplía el alcance de A-7; A-1..A-6 y A-8..A-13 no se amplían; Owner informado (§11, punto 15) | Architect: Q05 = YES, conforme. Coordinator AGREED sobre A-14 en `ab0170b7` |
| Si la rama de I-64 se mueve de `fa59bd82` | revisar igual `ab0170b7`; registrar la punta nueva; sin rebase, merge ni cherry-pick | No aplicó: la rama de I-64 siguió en `fa59bd82` a la salida y a la entrada de la revisión (`git-entry.json`) |

**El campo «Owner»** decía: «REQUIRED for the OWNER-RESERVED matters, by the exact choice in Sec.11, only after Architect AGREED and
Coordinator AGREED. Not requested by this order.» Así pasó:
- la firma se pidió después de los dos AGREED, con otra pregunta de la sexta ronda;
- la frase firmada es la exacta de la §11.

## El prompt frente a lo medido

El prompt es `prompt.md`, custodiado. Es `a14-architect-prompt.md` byte a byte, salvo el RunId y el SHA-256 de la orden. Tiene dos
desviaciones, ya declaradas en `review-record.json`:
1. **«config Snapshot L».** La línea 3 del prompt conserva el texto de la decisión Q-C2. La configuración medida fue la **Snapshot M**
   (`3E9BC35B…`): a la salida y a la entrada de la revisión, en `config-exit.json` y `config-entry.json`. Es la que nombra la orden
   ajustada. La M solo difiere de la L en el GUID de canalización (evidencia §294).
2. **La ruta de la orden.** El prompt nombra la orden en `artifacts/orchestration/I-64/A-14-review/<RunId>/`, dentro del worktree
   lateral. Ese archivo no existió allí: el encargo prohibía escribir en el worktree hasta el fin de la revisión. Ninguna orden del
   revisor la pidió, y el `CoordinatorOrderSha256` que devolvió casa con el de la orden custodiada.

Ninguna de las dos cambia el objeto revisado, la celda efectiva ni el veredicto, que fue AGREED, con REQUIRED 0 y OPTIONAL 0.
