# Paquete de cierre — F4B-PBT3C-FULL-WRITE-ERASE (PB-T3c)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `938a35f0f1636b4306d844eb9e34000e8967c05e`, con attempts **1 de 3**.

El escritor de la actualización del Workspace de Push Back puede borrar una vista obsoleta dentro de la única transacción de MUTATE.
- **La fachada caller-owned nueva,** `RackViewBlockEraser.EraseInTransaction` (`src/RackCad.Plugin/Views/RackViewBlockEraser.cs`):
  - borra una definición de vista con sus referencias directas y devuelve las definiciones que anidaba;
  - no toma el bloqueo, no abre ámbito, no confirma y no purga.
- **En WR:**
  - PREPARE resuelve las claves del plan de borrado;
  - el ámbito de MUTATE implementa `IPushBackPilotViewEraser` con `EraseUnit`, que llama a la fachada;
  - las anidadas van a la purga de POST.
- **D-2** («el escritor nunca borra») cambia **sin `Skip`**, con su registro declarado, que este cierre completa. La guarda de F4 `WriterProhibitionViolations` sigue en verde.

Ni Application, ni Domain, ni la UI, ni el anfitrión, ni el clásico cambian, y no se crean vistas. **PB-T3 queda completa.**

**Autoridades:**
- **El contrato:** `F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json`, con el paquete de PB-T3 v4.7, AuthorityRevision `e3c293b1` y BaseSha = ChainBaseSha `7dfc7c21`.
- **Las decisiones del Owner:**
  - D-PB-04 (borrar sí, crear no);
  - F4B-E35 (pregunta 3, la emisión condicional; pregunta 4, RR);
  - F4B-E37 (Snapshot K);
  - F4B-E40 (PD-F4B-01);
  - F4B-E43 (el sustituto de nc2 ampliado);
  - F4B-E46 (el total exacto de cada rama).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `e3c293b1` y BASE `7dfc7c21` | F4B-E47, con la v4.7. CA-T3c dio 0/0/4, y su recomprobación 0/0/1. La compilación de comprobación (precondición 13) da Core 19 124 = 19 118 + 6, con su control negativo |
| Planificación | `R20261120T134718Z-68e5` | Aceptada al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 14 criterios. nc4 `R20261120T134930Z-9c1a` |
| Worker | `R20261120T135026Z-80e5` | IMPLEMENTATION_COMPLETE. RED `e0940a0e` y GREEN `938a35f0`, sin reparación del RED, sin desviaciones y sin preguntas abiertas |
| CI del RED | `44754080713` | tests en failure; ui-tests y build-ui en success; build-plugin omitido. Core 19 129 = 19 119 + 4 Failed + 6 Skipped (CT-PBT3C-01 a 04 fallan por aserción; CT-05 pasa); UI 2 451 = 2 430 + 21 |
| CI del GREEN | `44757675821` | 4/4. Core 19 129 = 19 123 + 0 + 6, las de D-1; UI 2 451 = 2 430 + 21. Los totales exactos de su única rama (F4B-E46) |
| Revisión directa | — | Conforme, sin hallazgos |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/6**, más OWNER-1 a OWNER-3 para el Smoke-PB |
| Verificación | `R20261120T150458Z-e403` | **EXECUTION_VERIFIED** / NONE, 14/14 al primer lanzamiento; `rr_check` PASS |
| nc1 | `R20261120T151131Z-8a51` | **CONTROL PASS** (Identity, STOP) |
| nc2 | `R20261120T151911Z-670e` | **CONTROL PASS** (Scope, STOP) |
| nc3 | `R20261120T152519Z-2017` | **CONTROL PASS** (FreeText, REWORK) |

**Relevos:**
- 0 participantes en toda la cadena;
- la Snapshot K y el binario acreditado;
- `main` = `812baf87`, sin tocar;
- el «limit reached» de los eventos de nc1 y nc2 es el texto de la parada S-11 del contrato.

## 2. Límites, hallazgos y arrastres

| Id | Qué | Va a |
|---|---|---|
| Sin AutoCAD | El borrado real en el dibujo (las referencias, las anidadas, la purga de POST y el `U` único) no se ha ejercido: lo prueban solo guardas de texto | El Smoke-PB de PB-T5 (D-PB-09) |
| El orden de PREPARE | `PrepareUnits` resuelve las claves de borrado antes del bucle de los miembros; el paquete no fija ese orden. Declarado por el Worker, sin efecto observable | Registrado |
| PBT3C-IR-M01 a M06 | La precisión de las guardas nuevas (CT-01 a CT-05) y tres líneas de WR no prescritas, sin efecto observable | `docs/ideas-futuras.md` |
| PBT3C-IR-OWNER-1 a 3 | Riesgos del borrado real: referencias en espacio papel o en bloques del usuario borradas sin aviso; la purga de un nivel; los bloques dinámicos | El Smoke-PB, y se informa al Owner |
| CA-T3c M-02 y M-05 | La nota de V47-06 sobre PBT3B-IR-M01; el límite del corte de argumentos del barrido de sobrecargas, con el mismo «5» en la custodia de PB-T3b | `docs/ideas-futuras.md` |
| La tabla de ejecución de D-2 | Completada en este cierre (cinco líneas), la lección del cierre de PB-T3b | Hecho |
| El filtro de nombres de la custodia | `forceValidity` (API de AutoCAD) lleva dentro, sin distinguir mayúsculas, uno de los nombres de la lista, y daba un falso positivo. Solo ese nombre pasa a palabra completa (con `\b` a los dos lados), como el `(?![*])` del cierre de PB-T0; ningún archivo se excluye | Declarado en F4B-E48 |

**Siguiente:** el paquete de PB-T4, en revisión. Su emisión necesita la orden del Owner.
