# I-64 — Paquete de revisión del Architect (Proposal V1)

```text
PROPOSAL V1 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED (revisión NO realizada; no hay revisor asignado en este paquete)
Consensus      = NOT REACHED
Freeze         = NONE
Implementation = NOT AUTHORIZED

Objeto de la revisión = docs/initiatives/I-64-proposal-v1.md, en el commit que publica este paquete (rama
                        architecture/workspace-persistente-rackcad; el SHA no se escribe aquí para no autorreferenciarse:
                        se toma de `git log -1 -- docs/initiatives/I-64-proposal-v1.md`)
Discovery base        = docs/initiatives/I-64-discovery.md, versión D1-R1, blob 3565015c21a44b08df564b3277b5546a8a6ce013 (commit c6c44828)
Decisiones de partida = aceptación de D1-R1 por el Coordinator y MASTER-I63-I64-01 (evidencia §13)
Base de main          = 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Qué es este paquete.** El material para revisar la Proposal V1 sin reconstruir la sesión. La sesión autora **no** emite ni simula el
> veredicto del Architect, **no** declara consenso, Freeze ni GATE PASS, y **no** afirma la independencia de nadie. Quien asigne la revisión
> decide su forma (SAME-SESSION ROLE, SEPARATE SESSION o EXTERNAL HUMAN) y la declara ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §5). NEW
> ARCHITECTURE exige rondas Coordinator ↔ Architect hasta acuerdo sobre la misma versión.

## 1. Veredicto que se solicita

```text
Architect: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL — sección o D-nn, evidencia (archivo:línea, hecho medido o fuente), cambio exigido
Modo de la revisión: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN, y si revisor y autor son la misma persona
Versión: el veredicto vale solo para el SHA y el blob exactos revisados
```

Criterio de `REQUIRED` (LIFECYCLE §5): activa M-01..M-08 sobre un elemento congelable; deja un invariante sin obligación de prueba;
produce comportamiento observable distinto; o revela un elemento congelable materialmente incorrecto, no ejecutable o no verificable. Eso
incluye oráculos ciegos y planes de gates imposibles. `AGREED` exige cero REQUIRED abiertos.

## 2. Lectura mínima

1. [Proposal V1](I-64-proposal-v1.md), completa, con los anexos A (borrador del ADR propio) y B (notas a ADR-0010 y ADR-0032).
2. [Discovery D1-R1](I-64-discovery.md):
   - §1 (hallazgos), §3 (recorridos de edición, intención y `BindingIntent`), §4 (autoridades y trampa de vocabulario);
   - §§7-8 (EXP-04 y EXP-05), §9.1-9.3 (PaletteSet, eventos y documentos), §9.4 (inventario y las dos reglas de pertenencia);
   - §9.7 (alternativas de obsolescencia y H-LOCK), §§9.12-9.14 (fallos, lecturas y escrituras, normalización);
   - §§10-12 (DC-07, DC-08 y materialidad).
3. [Brief](I-64-owner-brief.txt): «PROPOSAL MUST DEFINE», «ARCHITECT REVIEW», «FUNCTIONAL GATES», «OWNER VALIDATION» y «SUCCESS CRITERIA».
4. [Evidencia](../automation/evidence/I-64-evidence.md) §13: aceptación de D1-R1, MASTER-I63-I64-01, respuesta de I-52 y metadatos de la
   API leídos.
5. Fuentes integradas que la Proposal consume o complementa:
   - ADR-0006, ADR-0009, ADR-0010, ADR-0019, ADR-0029, ADR-0032 y ADR-0044;
   - Freeze I-48 (`RegistryCommit`, reconciliación de vínculos);
   - Freeze I-55 V5 §3.7 y §4.1 (las dos reglas de pertenencia);
   - Freeze I-58 (AUTH-13);
   - LIFECYCLE §§3, 5-8; WORKFLOW §8 y §11.4; `routing.md` §§1-3.
6. Código de la base `819955d6`: `RackSiblingMembership` (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs`), las seis
   `EditX` de `src/RackCad.Plugin/Rack*Commands.cs`, `RackEditorSession`, `EditorPendingWork` y `RackBlockFinder`.

## 3. Resumen de la propuesta (detalle en la Proposal)

| Tema | Decisión | Dónde |
|---|---|---|
| Host | un `PaletteSet` por proceso, sin `toolID` ni `Load`/`Save`; `ShowModelessWindow` como contingencia | D-01 |
| Propiedad | controlador por proceso; UI sin AutoCAD; puerto con claves y valores; operaciones acotadas y solo en reposo | D-02 |
| Documentos | una sesión por `Document` nativo; claves (documento, RackId) | D-03 |
| Eventos | un puente; una suscripción por fuente; pistas coalescidas en reposo; nunca autoridad | D-04 |
| Inventario | índice runtime de I-64 sobre el snapshot de I-63, en F2 y solo integrado | D-05 |
| Selección | AutoCAD → contexto; nunca contexto → selección; idempotente | D-06, D-07 |
| Pestañas | registros ligeros; una preview que se fija al editarse; una superficie viva | D-08 |
| Multiselección | Selección (N) sin pestañas; conjunto estable para lotes futuros | D-09 |
| Borrador | `DraftSession` como evolución de lo existente; texto pendiente nunca perdido | D-10 |
| Pertenencia | `RackSiblingMembership.Classify` para el panel; RACKEDITAR conserva `FindRackBlocks` | D-11 |
| Obsolescencia | relectura cruda exacta en la transacción de escritura; sin normalización ni revisión persistida | D-12 |
| Actualizar | PREPARE / MUTATE / POST; todo o nada; siete resultados | D-13 |
| Editores | escalonado por adaptadores; piloto Push Back en F4; F5-A y F5-B | D-15 |
| Coexistencia | RACKEDITAR y comandos clásicos sin cambio | D-16 |
| Cierre de documento | veto en el primer intento y descarte con aviso en el segundo | D-17 |
| Rendimiento | P-01..P-09 y los nueve escenarios con línea base | D-18 |
| ID3 | Foundation dentro; lotes como residual; `PARTIAL` esperado | D-20 |
| ADR | ADR propio; complementos fechados de ADR-0010 y ADR-0032 D6 | D-21, anexos |

## 4. Los quince retos del brief («ARCHITECT REVIEW»)

| # | Reto | Dónde lo trata la Proposal | Riesgo residual que se pide juzgar |
|---|---|---|---|
| 1 | PaletteSet frente a una ventana modeless arbitraria | D-01; contingencia por A-n | que el teclado de WPF alojado obligue a cambiar de host después de F1 |
| 2 | La UI convertida en autoridad de persistencia | §2; D-12; D-22; INV-21 | que «Conservar mi borrador sobre la versión actual» se perciba como sobrescritura implícita |
| 3 | Bucles de reentrada de eventos | D-04 (ámbito de mutación propia, manejadores sin efectos); INV-08 | escrituras de otros complementos durante la transacción propia (fuera de H-LOCK) |
| 4 | Bucles de retroalimentación de la selección | D-06, D-07 (selección → contexto, nunca al revés; idempotencia); INV-03 | Seleccionar con copias en otro espacio |
| 5 | Suscripciones duplicadas | D-04 (una por fuente); INV-02 con guarda de fuente | suscripciones por documento y por base, que son por fuente y no por pestaña |
| 6 | Fuga de estado entre documentos | D-03 (identidad nativa; claves compuestas); INV-07 | el mismo DWG abierto dos veces |
| 7 | Borradores obsoletos que sobrescriben | D-12 (relectura cruda en la transacción); INV-06 | la frecuencia de conflictos falsos y si basta con dos opciones de reconciliación |
| 8 | Carga anticipada de todos los racks | D-05 (sin enumeración hasta F2; refresco bajo demanda); P-02, P-05 | el coste de la recaptura completa en dibujos grandes |
| 9 | Demasiados árboles WPF vivos | D-08 (una superficie viva); INV-15 | el coste de rematerializar editores de 3 500 líneas en cada cambio |
| 10 | Llamadas a AutoCAD desde la capa de UI | D-02 (puerto sin tipos de AutoCAD); INV-13, INV-14 | — |
| 11 | Fuga de la vida de transacciones | D-02 (operaciones acotadas); INV-14 | la transacción externa de D-13 sobre primitivas que abren las suyas |
| 12 | Foco y teclado de los comandos | D-01, D-07; S1-06 | la recuperación del foco sin APIs internas |
| 13 | Semántica de undo/redo | D-04, D-13; S2-05 | el grupo de deshacer de un comando interno (`INFERENCE`) |
| 14 | Duplicación entre los seis editores | D-15 (contrato común, adaptadores en secuencia) | que Cabecera y Cama necesiten un adaptador distinto en esencia |
| 15 | Explosión de alcance | §1 (no-objetivos), D-23 (extensión sin implementación), D-20 | la exclusión de Insertar y `BindingIntent` del panel |

## 5. Disposición de los hallazgos y decisiones previos por ID

| ID | Origen | Disposición | Dónde |
|---|---|---|---|
| C64-D1-01..07 | revisión de D1 por el Coordinator | resueltos en D1-R1; el Coordinator aceptó D1-R1 y cerró el Discovery | Discovery §17; evidencia §13 |
| EXP-02 (escalada) | Discovery §9.4 | resuelta como propuesta: `RackSiblingMembership.Classify` para el panel | D-11 |
| MASTER-I63-I64-01 | Master | aplicada: frontera, propiedad, prohibiciones y secuencia | D-05, §11, §17 |
| Reglas de smoke (Master) | Master | aplicadas | §12.1 |
| Respuesta tardía de I-52 | I-52 | registrada | evidencia §13; §12.1, §15 |
| M-02 y M-03 (EXP-09) | Discovery §12.2 | resolución propuesta: no activados, con condiciones | §14 |
| Alternativas de obsolescencia A..E | Discovery §9.7 | E con texto crudo; A solo como admisión; B, C y D rechazadas como compuerta | D-12 |
| Semántica de fallo (M-04) | Discovery §9.12, §15 | todo o nada en la vía del panel; RACKEDITAR sin cambio | D-13 |

## 6. Preguntas que se piden expresamente al Architect

1. **Host (D-01).** ¿`PaletteSet` sin `toolID` como host principal y `ShowModelessWindow` como contingencia por A-n es la elección correcta,
   o el Freeze debe exigir una comparación medida antes de F1?
2. **Pertenencia (D-11).** ¿Es correcto que el panel, como consumidor nuevo que muta, use `RackSiblingMembership.Classify` (fail-closed)
   mientras RACKEDITAR conserva `FindRackBlocks`? ¿Hace falta elevar esta elección a quien gobierna el Freeze de I-55?
3. **Obsolescencia (D-12).** ¿Es aceptable la comparación por texto crudo exacto, que solo admite conflictos falsos? ¿Debe acotarse la
   dependencia del registro en Selectivo a las variables que usa el rack?
4. **Atomicidad (D-13).** ¿Es viable y seguro envolver las primitivas de redibujo existentes en una transacción externa, con la
   importación dentro, para obtener todo o nada sin reescribirlas? ¿Qué prueba de host lo acredita?
5. **Cierre de documento (D-17).** ¿Vetar el primer intento es preferible a avisar después? ¿Debe decidirlo el Owner antes del Freeze?
6. **Alcance (§1, D-13).** ¿La exclusión de Insertar y `BindingIntent` del panel en V1 es una decisión de diseño o un cambio de alcance
   `OWNER-RESERVED`?
7. **Plan (§11).** ¿Es coherente el piloto Push Back dentro de F4, la división F5-A/F5-B y que F2 y F3 avancen antes de Smoke-1 mientras F4
   y F5 lo esperan?
8. **Materialidad (§14).** ¿Se aceptan las resoluciones propuestas para M-02 y M-03, con sus condiciones?
9. **Texto pendiente (D-10).** ¿Conservar el texto pendiente en las fronteras no bloqueables es compatible con ADR-0032 D6, o requiere más
   que una nota posterior?
10. **Dependencia de I-63 (D-05, §11).** ¿Es suficiente la resecuenciación por A-n si el contrato de I-63 no está integrado a tiempo?

## 7. Lo que este paquete no hace

No asigna revisor. No realiza la revisión. No declara AGREED, consenso, Freeze, GATE PASS ni independencia. No autoriza implementación,
delegaciones, sondas, pilotos ni ningún uso de AutoCAD. `IMPLEMENTATION AUTHORIZATION = NO`.
