# I-62 — Paquete de revisión del Architect (enmienda A-1: FC-01 y FC-02)

```text
A-1            = PROPUESTA — NOT REVIEWED
Architect      = REVIEW REQUIRED: una revisión de la enmienda exacta (cambio MATERIAL: LIFECYCLE §6 exige Architect + Coordinator)
Coordinator    = clasificación MATERIAL y dirección de FC-01 y FC-02 (decisiones §34); veredicto PENDING
Owner          = sin decisión identificada; si aparece una consecuencia OWNER-RESERVED, la enmienda se detiene
Invocación     = NOT LAUNCHED: la sesión se detiene en la frontera de invocación (sin autorización para lanzar una sesión nueva del Architect)
Implementación = solo F3 autorizada; la materialización de F4 afectada por A-1 está bloqueada hasta su veredicto

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-A-1.md                                          blob 09ca93285975c4b7af6471d6ae91bfa12c94a1fc
Freeze que enmienda:
  FREEZE_SHA b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43
  docs/initiatives/I-62-proposal-v14.md   commit 4c617e82b32b6c810b68d75fc19472efed22b393   blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta.** El revisor comprueba `git rev-parse <commit>:docs/initiatives/I-62-A-1.md` = `09ca9328…` en el commit del recibo de publicación.
> Si no coincide, revisa la versión designada o rechaza la discordancia. Este paquete no lleva su propio blob.

## 1. Veredicto que se solicita (LIFECYCLE §5 y §6)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Modo:      declarado (SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN) y si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

AGREED sobre la A-1 exacta, más el veredicto del Coordinator, la convierte en enmienda acordada del Freeze y desbloquea la materialización de F4 afectada.
Un REQUIRED abierto lo cierra o lo rebaja solo quien lo emitió. La sesión no declara ningún veredicto.

## 2. Lectura (insumos canónicos)

| Insumo | Blob | Para qué |
|---|---|---|
| `docs/initiatives/I-62-A-1.md` | `09ca9328…` | el objeto: cláusulas anteriores, delta exacto (D1-1..D1-9, D2-1..D2-8), materialidad, efecto en pruebas |
| `docs/initiatives/I-62-proposal-v14.md` en `4c617e82` | `34ad80ea…` | las cláusulas que se enmiendan: §8.8, §9.3, §20.5, §20.6, B.8.4 (I-P05, I-P10, I-H02), B.8.7 y B.8.8 (I-S18, I-P13) |
| `docs/automation/evidence/I-62-A1/a1-counterexamples.py` | `d7a5d1ef…` | trazas exactas: contra el texto literal y contra el delta |
| `docs/automation/evidence/I-62-A1/a1-counterexamples-result.json` | `9fa501b6…` | resultado de las 15 trazas (todas con su esperado) |
| `docs/automation/evidence/I-62-prep/freeze-issues.md` | `7be7f8e9…` | hallazgo original de FC-01 y FC-02, con las opciones |
| `docs/automation/evidence/I-62-prep/f4-dossier.md` §4 | `6ae3f9fe…` | auditoría de transiciones que los encontró |
| `docs/INITIATIVE_LIFECYCLE.md` §3 y §6 | `f19896a8…` | formato de A-n y disparadores M-01..M-08 |
| `docs/automation/decisions/I-62.md` §34 | (en el mismo commit) | clasificación y dirección del Coordinator |

Para leer las cláusulas de V14 basta su sección: el insumo es un archivo grande, y no hace falta expandir los documentos que su prosa cita.

## 3. Resumen del delta

**FC-01.** Los presupuestos del bucle pasan a ser por `ReviewLoopAuthorization` (`budgets[]` append-only, una entrada por autorización, sin reinicio por
cambio de proveedor, modelo, sesión ni Principal). Se añade la transición legal **LOOP_CLOSED** (desde ARCHITECT_SATISFIED, desde ESCALATE_OWNER resuelta o
con la vigencia agotada → NONE), la única en la que `loop.object` pasa a `null`. Un bucle nuevo exige una autorización nueva del Coordinator. Así, la
conformidad de READY-06 puede ser un bucle posterior e independiente. Los linajes siguen siendo de unidad (sin cambio).

**FC-02.** `loop.object.commit`, el `object.commit` de las solicitudes OPEN y el `Target.commit` de los intentos no lanzados entran en `StateFields` y en
I-H02. En REBASE_RECONCILIATION pasan a su imagen probada con `path`, `blob`, fase, contadores y linajes intactos (excepción de rebase en I-P13, I-P05 e I-P10).
Un intento no lanzado se replanifica con `InvocationId` nuevo dentro de la misma reserva; uno lanzado conserva su `Target` histórico; uno completado nunca se
reescribe.

## 4. Preguntas para la revisión

1. **Origen de LOOP_CLOSED.** ¿Bastan los tres orígenes de D1-5 (ARCHITECT_SATISFIED; ESCALATE_OWNER con la decisión registrada y una decisión del
   Coordinator que cierra; vigencia EXHAUSTED con la escalada resuelta), o hay un camino legítimo de cierre que falta (p. ej., una revocación sin
   agotamiento)?
2. **Linajes de unidad.** A-1 deja `findings[]` y la condición de ARCHITECT_SATISFIED de I-S18 en el ámbito de la unidad: un bucle nuevo hereda los linajes
   abiertos. ¿Es correcto para un bucle sobre otro objeto (p. ej., la conformidad de READY-06 tras un diseño satisfecho, donde no queda ningún REQUIRED
   abierto), o hace falta acotar los linajes por familia de objeto?
3. **`corrections_by_lineage` por autorización.** Al estar dentro de `budgets[]`, un linaje heredado empieza con su contador en cero en el bucle nuevo, por
   decisión explícita del Coordinator. ¿Es aceptable, o el tope por linaje debe ser de unidad?
4. **Históricos fuera de I-H02.** Las solicitudes terminales y los intentos en LAUNCHING o posteriores conservan su commit original, que un clon limpio puede
   no tener; su identidad revisada es el `blob`. ¿Es suficiente, o deben registrarse también en `StateFields` con su imagen (sin reescribirlos)?
5. **INVOCATION_PLANNED → INVOCATION_PLANNED** solo en REBASE_RECONCILIATION (D2-6). ¿Basta así, o la replanificación de un intento no reservado debe
   valer en general, como la de BUDGET_RESERVED?
6. **Materialidad.** ¿Coincide la tabla M-01..M-08 de §4 del registro, en particular M-08 = no (ADR-0046 #7 rige `attempts`)?
7. **Consecuencias del Owner.** ¿Hay alguna consecuencia OWNER-RESERVED que la sesión no identificó? Si la hay, la enmienda se detiene.

## 5. Condiciones de la invocación (para quien la autorice)

- **Frontera:** lanzar una sesión nueva del Architect no está autorizado en la orden vigente (decisiones §34). La sesión entrega el paquete completo y se
  detiene aquí; no elige runtime ni lo invoca.
- **Protocolo:** I-61 sigue siendo el protocolo activo; la invocación sigue sus reglas y las del registro de la revisión (LIFECYCLE §5). OD-2 no está
  resuelta: la invocación no usa el runtime que OD-2 bloquea.
- **Independencia:** el revisor no es la sesión autora de A-1 (que es la sesión principal de I-62) ni comparte su contexto; el modo y si revisor y autor son
  la misma persona se declaran.
- **Insumos:** los de §2, en el commit del recibo de publicación, sin la transcripción ni la memoria de la sesión autora.

## 6. Lo que este paquete no hace

- no aplica A-1: ningún texto congelado, materializado o de producción cambia;
- no declara AGREED, CHANGES REQUIRED ni ningún otro veredicto;
- no materializa F4 ni ninguna parte de `state/v2` o de la orquestación;
- no repara la deuda nc2 de I-64 (unidad I61) ni crea otra A-n.
