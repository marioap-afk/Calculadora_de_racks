I-63 — decisión sobre STOP G4 S-03/C-10
Acepto `RackSummary.RepresentativeDefinitionId` como precisión no material necesaria para hacer observable INV-11 en G4.
No requiere A-4 y no requiere cambio de trabajo.
La razón es que no introduce una nueva autoridad ni un nuevo dato de dominio:

* el representante ya existe en `ProjectPopulation` desde G2;
* D-21 ya exige conservar la `DefinitionKey` del representante en provenance;
* la autorización de G4 exige comprobar que `Full(original)` y `Full(reversed)` conservan el mismo representative;
* el getter únicamente expone ese dato ya calculado en el `RackSummary`;
* es read-only, en memoria y no persistido.

Interpreto D-20 como el contenido semántico mínimo obligatorio de `RackSummary`, no como una prohibición de propiedades auxiliares read-only que exponen información ya congelada y necesaria para verificar sus invariantes.
Restricciones
`RepresentativeDefinitionId` queda aceptado únicamente bajo estas condiciones:

```
RackSummary.RepresentativeDefinitionId

source =
  same representative already selected by ProjectPopulation / E4

semantics =
  informational projection only

must NOT:
- select a representative independently
- alter Membership
- alter Metrics
- alter MetricValue
- participate in persistence
- create another authority
- introduce another traversal/resolution
```

Su valor debe ser exactamente el representante ya determinado por la población; si G4 estuviera recalculándolo por una ruta distinta, eso sí sería STOP.
Disposición del STOP
Por tanto:

```
S-03 = RESOLVED BY COORDINATOR
C-10 = NOT ACTIVATED after clarification

work change = NO
attempts = 2 / 3
```

Repite la verificación con:

* nuevo `RunId`;
* mismo GREEN `4f45f446`;
* mismo contrato;
* esta decisión como resolución normativa;
* sin replanificar;
* sin Worker;
* sin consumir `attempts`.

Si da `EXECUTION_VERIFIED`, continúa con:

```
mechanical Scope PASS
existing-tests-unchanged PASS
nc1
nc2
nc3
custody
→ volver al Coordinator para G4 PASS
```

Hallazgos H-G4
También dejo su disposición para evitar otro round trip:

* H-G4-01: no bloqueante. Hubo una prueba local provisional antes del RED, pero el Worker restauró el árbol y la cadena publicada conserva RED → GREEN. Registrar como desviación operativa; no repetir.
* H-G4-02: aceptado. El `1` en Push Back colocado corresponde a E5 de población; el requisito de cero lecturas para la métrica sigue cumpliéndose. No interpretes INV-34 como prohibición de la lectura de población necesaria para clasificar el rack.
* H-G4-03: aceptado. `RackMetricOrchestrator` es consistente con el objetivo congelado de una sola tabla D-28, siempre que G1/G2 permanezcan verdes.
* H-G4-04: las cuatro limitaciones son compatibles con el Freeze y no bloquean G4. En particular, métricas de Selectivos excluidos/`NotPlaced` son coherentes con R-14.

Estado

```
G4 GREEN = 4f45f446
CI = 4/4
STOP S-03/C-10 = RESOLVED
correction = none

attempts = 2 / 3
AttemptsRemaining = 1

Next =
verification rerun on same GREEN
```

No hagas A-4 ni modifiques el código por este punto.
