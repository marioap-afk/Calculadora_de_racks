# I-52 — Architect review package: Proposal V22 / Materialization + Snapshot R + CT-DA admission

Actúa exclusivamente como Arquitecto par de I-52 — ID16 — RACKMIRROR.

Realiza revisión exact-SHA del commit que contiene este paquete. El ejecutor debe suministrar SHA, blobs y CI.

## Objetos

- `docs/initiatives/I-52-proposal-v22.md`
- `docs/automation/decisions/I-52.md` §§125–126
- este paquete

Contrasta contra:

```text
Starting I-52 / Proposal V21 SHA = d13c0390f2064b333b4030491788ae12ecfd2f91
Proposal V21 blob = ff037f02c5e26136a47610ca2de848102c6d4ca9
Architect package V21 blob = c16a52d3131ebdc2c9b1afaa6d32b1020d8355c3
Decisions V21 blob = 1b84289e5eaf1b99025fb4e8ec39f38f22f7a7f2
Base main = 7097057cf8685bf5ecc09083cba37379d4a4aae8
I-55 re-fetched = 67779989233ef190810d2c5a61e1f1e499e31983
```

## Estado propuesto, no aprobado

```text
V20 architecture = PRESERVED / RESTRICTED
V21 delta = SUPERSEDED BY V22
Selected alternative = ALT-21C / CORRECTED ADMISSION CONTRACT
CT-DA research contract = READY FOR REVIEW
V18 = GOVERNING
Technical Consensus = NOT REACHED
G3 = STOPPED
```

## Límites

No modifiques archivos. No commit, rebase, Freeze, Owner decision, ADR gate, G3, G3B, CT-49R, CT-50, CT-DA o
implementación. No aceptes ADR-0036.

## Intenta refutar

1. Comprueba que `OverallSuccess` exige simultáneamente `SemanticMirrorCorrect`, `PersistedSemanticCorrect`,
   `MaterializationCorrect` y `MaterializationCommitSucceeded`.
2. Busca un estado físico incorrecto que todavía satisfaga `MaterializationCorrect`; revisa siblings, addresses,
   identity, transforms, determinant/scale, definitions/resources, dynamic properties, grouping, orphans, linkage y
   metadata per-view.
3. Refuta la independencia del verifier físico: no debe confiar en plan, writer o estado preparado.
4. Decide si `R_pre/R_post` tienen un punto de observación consistente real o siguen siendo scans secuenciales.
5. Ejecuta el contraejemplo A=`D'`, callback muta B, B=`D''`; ningún conjunto mixto puede admitirse.
6. Revisa si `M_pre/M_post` necesitan mecanismo distinto de R y si staged puede inferir committed.
7. Verifica que V22 no invente global DB version, transactional snapshot, fence de callbacks o command-completion.
8. Revisa T1..T15: clases IN/OUT, semánticas/materiales y mapping completo a DA-P.
9. Intenta producir una amenaza IN no cubierta por DA-P1..10. Cualquier hueco hace CTDA_PASS insuficiente.
10. Revisa DA-P1: afiliación/prevention/detection de writes same-context.
11. Revisa DA-P2/3: callbacks de commit y secondary/nested transactions.
12. Revisa DA-P4/5: read-your-own-writes y consistencia de snapshots semánticos.
13. Revisa DA-P6: frontera final hasta command-completion exacto.
14. Revisa DA-P7/9: cobertura y consistencia del verifier físico.
15. Revisa DA-P8: NOD en la misma DB/T/snapshot/commit/undo.
16. Revisa DA-P10: mutación fuente causal same-command después de S.
17. Confirma `CTDA_PASS iff ALL PASS + coverage + exact tuple + fixtures`; FAIL/UNKNOWN nunca admiten.
18. Confirma que handler controlado prueba semántica de su clase host, no ausencia o identidad de plugins.
19. Revisa exact-host tuple e invalidación por cualquier cambio.
20. Decide si `DOCUMENTED CONTRACT/BEHAVIOR`, `RUNTIME OBSERVATION` e `INFERENCE` tienen política honesta.
21. Revisa CT-DA-01..15, logs obligatorios y mapping a las diez propiedades.
22. Confirma que CT-DA es research-only, separado de G3 y no autoriza código productivo.
23. Refuta la secuencia B. Freeze final, Owner y ADR deben ocurrir sólo después de CTDA_PASS.
24. Revisa `FAILURE_COMMITTED_CORRUPT`: no product success, no rollback, no compensación implícita.
25. En multi-rack, un solo fallo postcommit debe fallar toda la invocación y anular Candidate.
26. Confirma que Selective sólo es fixture avanzada y que CTDA_PASS no la hace product-ready.
27. Confirma que V20/V21 sólo se preservan/reemplazan en el delta declarado.
28. Confirma V18, Freeze V18, O-1 V18 y ADR-0036 gobernando; G3 cerrado.
29. Confirma Foundation/I-49 sin cambio, `PlanReadSet` por variable y AUTH-15 no implementada.
30. Confirma I-55 `NON-MATERIAL` contractual / `MATERIAL` coordinación, sin transferencia de evidencia.

## Preguntas decisivas

1. ¿Puede `OverallSuccess=true` con materialización incorrecta?
2. ¿Puede R/M aceptar una mezcla que nunca existió?
3. ¿Existe una amenaza soportada sin DA-P?
4. ¿Puede cualquier `UNKNOWN` producir CTDA_PASS?
5. ¿Se generaliza una observación más allá del tuple/clase probados?
6. ¿Se solicita Freeze/Owner/ADR antes del resultado?
7. ¿Se convierte un fallo committed en éxito parcial?
8. ¿CT-DA autoriza implementación o reapertura G3?
9. ¿ALT-21C sigue siendo acotada y distinta de aislamiento universal?

Una respuesta afirmativa a 1–8 o negativa a 9 exige cambios.

## Output

Reporta SHA, refs, blobs, CI, `BLOCKER/HIGH/MEDIUM/LOW`, modelo formal, snapshots R/M, threat model, DA-P, matriz,
probes, política de evidencia, secuencia B, disposiciones y si se necesita Proposal V23.

Termina exactamente con uno:

```text
Architect: AGREED WITH PROPOSAL V22 — CT-DA RESEARCH CONTRACT READY
```

o:

```text
Architect: CHANGES REQUIRED — PROPOSAL V23
```

Y siempre:

```text
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
