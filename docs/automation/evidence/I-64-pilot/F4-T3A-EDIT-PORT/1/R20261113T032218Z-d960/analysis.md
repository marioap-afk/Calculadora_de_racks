# F4-T3A-EDIT-PORT, intento 1: análisis de las revisiones y disposición

**Planificación:**
- `R20261113T012340Z-4a5e`: rechazada antes del Worker. La comparación byte a byte dio REJECTED en Invariants[6], copiado de indicativo a subjuntivo. Es la parada P-03 y no consume intentos.
- `R20261113T013120Z-cad1`: la replanificación reforzada única. Fidelidad PASS en las ocho colecciones, A1-A8 8/8 y nc4 `R20261113T013522Z-5eab`.

**Entrega:**
- Worker `R20261113T013630Z-f0c9`: RED `2522e894` y GREEN `18471988`.
- CI `43355611879` del GREEN: 4/4.
- CI `43351822533` del RED: Core en rojo con 110 fallos por aserción, todos dentro de `RackCad.Tests.Workspace`; UI 248/248.

**Verificación del Controller** `R20261113T032218Z-d960`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `18471988`. Sin hallazgos y sin términos de 16.10.

## Revisión directa de la sesión

**Producto conforme.** Sin defectos de corrección frente a INV-F4T3A-01..14 ni frente al oráculo crítico. Da dos MENORES, que coinciden con los hallazgos 1 y 2 de la independiente. El informe completo está en `direct-review.md`, junto a este archivo.

## Revisión adversarial independiente

**Veredicto:** ACEPTABLE CON MENORES (0 bloqueantes, 0 importantes, 7 menores). La semántica del puerto, las dos compuertas, la cola de veto, el texto mostrado y el ámbito del modal cumplen el contrato. El informe completo está en `independent-review.md`.

Lo que confirma:
- no hay escritura authored ni lecturas del DWG fuera de las tres operaciones de la intención;
- no hay `+=` nuevos;
- todos los cambios en guardas y pruebas existentes están declarados;
- el RED es inerte;
- T3B, T3C y T4 no quedan bloqueados.

| Id | Severidad | Hallazgo | Tipo |
|---|---|---|---|
| 1 | MENOR | El «escritor simulado» (`PilotEditAuthoredWriterSpy`) no está conectado a nada: sus aserciones no pueden fallar. El oráculo crítico se sostiene por la guarda por cuerpo `WriteViolations`, con autopruebas, y porque Application no tiene dependencia de escritura | Pruebas (evidencia del oráculo) |
| 2 | MENOR | `ThisTaskAddsNoFile...` vuelve a fijar 5 archivos exactos en `Drafts/`, contra D-1, y duplica los 16 `+=`/`-=`/manejadores del puente y los 9 archivos del Plugin, que ya fijan otras guardas (regla I-1) | Guardas |
| 3 | MENOR | Huecos de discriminación:<br>- la frontera bloqueante con un borrador no editable y pendiente (→ `Kept`) no tiene prueba;<br>- ninguna prueba afirma que las operaciones que no mutan conservan la marca «limpio conocido»;<br>- los `catch` externos de `GetField` y `GetRacksAtRisk` no se ejercitan;<br>- una aserción compara dos literales | Pruebas |
| 4 | MENOR | Contención:<br>- `TakeLossNotices` y `TakeCloseNotices` no tienen `try`;<br>- el `catch` externo de `GetRacksAtRisk` devuelve `default`, que se lee como «nada en riesgo». T3C (R-C2) lo consumirá.<br>Hoy es inalcanzable | Host (endurecimiento) |
| 5 | MENOR | `Undecidable` solo aparece sin estado explícito: un borrador en Conflict con un comparador que lanza sale como Conflict con `CanEdit` en true. Es funcionalmente seguro, pero no está documentado ni probado | Host (contrato) |
| 6 | MENOR | En `SessionDestroyed`, `DropCloseNotice` queda fuera de los `try` y antes de la retirada. Hoy es inalcanzable | Host (endurecimiento) |
| 7 | MENOR | Documentación para T3B:<br>- `Kept` no dice que la transición procede;<br>- el `default` de `PilotBoundaryDecision` es `Proceeded` (por el contrato);<br>- `EditCheckStatus.Waiting` no documenta los dos motivos nuevos | Documentación |

**Riesgos para el smoke** (no son defectos de T3A):
- `Authorize` a cada pulsación en CLOSE, CLOSEALL o QUIT y en un cambio de documento;
- drenajes o retiradas durante una lectura, que la marca de reentrada no cubre;
- trabajo diferido de T3B y el constructor de la paleta, que escapan a la compuerta de avisos;
- los avisos de veto, que no emiten `StateChanged`;
- la edición con un comando activo o en modo degradado;
- el orden de la marca modal frente a la ventana;
- la cultura con ajustes de usuario de Windows.

## Disposición

**Pregunta al Owner.** Durante la verificación, la sesión preguntó al Owner si cerrar o corregir (`owner-f4t3a-close-decision.md`).

**Decisión del Owner:** «Cerrar y arrastrar (Recomendado)». En consecuencia:
- se completan nc1..nc3;
- F4-T3A se cierra sobre `18471988` como PASS WITH LIMITATIONS, sin gastar el intento 2;
- los 7 menores pasan a cambios declarados de los contratos siguientes:
  - **a F4-T3C,** que consume el riesgo para su mensaje y su cálculo de pérdida (R-C2): hallazgo 4 (las colas con `try` y el fallo de `GetRacksAtRisk` como incierto) y hallazgo 6 (`DropCloseNotice` dentro de la contención de `SessionDestroyed`);
  - **a F4-T3B:**
    - hallazgo 1: un espía de escritura real, o retirar el nominal y citar la guarda por cuerpo;
    - hallazgo 2: cotas en lugar de los conteos exactos duplicados;
    - hallazgo 3: las pruebas que faltan;
    - hallazgo 5: documentar o extender `Undecidable`;
    - hallazgo 7: la documentación del puerto que su UI consume.
- **Registro en la custodia:** el escritor simulado de D-T3-10 es nominal; el oráculo crítico de T3A es la guarda por cuerpo de las API de escritura más la ausencia de dependencia de escritura de Application.
