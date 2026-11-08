# I-64 — Apertura del gate F2 y contrato de F2-T1-NAVIGATION-CORE (literal)

Recibida el 2026-11-08 por el chat de la sesión responsable, después de la custodia `90f5872e` y su CI `42504919895`. La pegó el
Owner. El bloque JSON del contrato está, byte a byte, en `gate-contract.json` (SHA-256
`A2EF2055F749DCDBECEAC4EBB44D6105DED33964D3A0BEE9982F9051D8E12E3A`) y aquí se omite para no duplicarlo. Diferencias de forma
con el original:
- se quitaron las vallas de código internas del mensaje, porque no se pueden anidar en este bloque;
- el texto es el original, sin otra diferencia.

```text
Correcto. La custodia `90f5872e89660655a93606e16d50f4bc19aa3c9c` quedó verificada y la CI `42504919895` pasó 4/4. Con esto cierro formalmente la transición desde F1/integración y emito el contrato inicial de F2 — Context Navigation.
Decido no entregar F2 entero a un solo Worker de una vez. Lo divido en dos tareas dentro del mismo gate:

F2-T1-NAVIGATION-CORE
→ autoridad NavigationTargets + caché + políticas puras

F2-T2-NAVIGATION-HOST-UI
→ adaptador AutoCAD + acciones + UI + eco + foco

Solo F2-T1 queda autorizado ahora. T2 requiere reemisión del contrato después de que T1 quede `EXECUTION_VERIFIED`. Esto reduce la superficie de una tarea Deep sin crear micro-gates: F2 sigue siendo un único gate.
Coordinator — contrato de F2

I-64 — COORDINATOR F2 GATE OPEN

Disposition:

F2 = OPEN

F1 = PASS
Smoke-1 = PASS
I-65 synchronization = PASS
POST-I65 HOST SANITY = PASS
P-01 = CLEARED / SNAPSHOT D
POST-I65 INTEGRATION GATE = CLOSED

Current branch:

90f5872e89660655a93606e16d50f4bc19aa3c9c

Current origin/main:

a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60

No F2 product work has been performed yet.

F2 remains one lifecycle gate.

Execution decomposition:

F2-T1-NAVIGATION-CORE
F2-T2-NAVIGATION-HOST-UI

Only F2-T1 is authorized by this contract.

F2-T2 requires a new Coordinator gate-contract after F2-T1 is
EXECUTION_VERIFIED.

El `gate-contract.json` que debe guardar la sesión para T1 es:

[bloque JSON: ver gate-contract.json]

Enrutamiento obligatorio
Para F2-T1:

Clase:
Implementación transversal a capas — corta

Effort semántico:
Deep

Worker elegible:
claude-sonnet-5-5
subagente
high
write-commit-push

Controller:
gpt-6-luna
Codex CLI read-only
high

`RoutingEnforcement = required`.
No se autoriza cambiar de modelo, effort o transporte silenciosamente.
Presupuesto nuevo de F2
Los `attempts 3/3` agotados pertenecen a F1 y no se trasladan a F2.
Para `F2-T1-NAVIGATION-CORE`:

Attempt inicial = 1
MaxReworkLoops = 3
AttemptsRemaining al iniciar = 2
CorrectionsAuthorized = true

Sin reinicios: escalar modelo, rebasear o cambiar transporte no restaura intentos consumidos.
Secuencia autorizada ahora

1. Guardar gate-contract.json
2. comprobar Snapshot D exacta
3. relevo 16.4
4. Controller planning
5. comprobaciones A1..A8
6. Worker RED
7. push RED
8. Worker GREEN
9. push GREEN
10. CI exact-SHA
11. Controller verification
12. custodia/estado
13. volver al Coordinator

La parada será:

COORDINATOR_F2_T1_CLOSE_REQUIRED

No debe empezar `F2-T2` automáticamente.
Qué no está autorizado todavía
En T1 no se toca AutoCAD, Plugin ni WPF.
Por tanto todavía no se implementan realmente:

* `SetImpliedSelection`;
* zoom;
* centrar la vista;
* Localizar vista en AutoCAD;
* foco de regreso al dibujo;
* botones de navegación de la paleta;
* Smoke-NAV.

Eso es F2-T2.
El objetivo de T1 es que, cuando lleguemos al host, haya una sola autoridad pura, probada y difícil de usar incorrectamente para resolver qué objetos puede navegar RackCad.
Si T1 cierra correctamente, emitiré inmediatamente el contrato de `F2-T2-NAVIGATION-HOST-UI`; después de T2 y la revisión del gate completo vendrá el Candidate de F2 y Smoke-NAV.
```
