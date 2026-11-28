# Paquete de cierre — F4B-PBT0-CHARACTERIZATION (PB-T0, bloque C)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `28e38d8d51108c22305a5fc41ce84070a2efdbe7`, attempts **1 de 3**.

**Autoridades:**
- el contrato `F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json` (AuthorityRevision `ca4165bb`, BaseSha = ChainBaseSha `f2eedd9a`);
- el paquete `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md` v2.1;
- las decisiones de procedimiento del Owner (F4B-E03);
- la ratificación del Owner sobre CT-PB0-17 (`F4B-DRAFTS/coordinator/owner-pbt0-c16-decision.md`, F4B-E05).

El bloque M se cerró y se custodió aparte (F4B-E04, `block-m/`). No consume intentos.

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Planificación | `R20261117T184331Z-eb30` | **RECHAZADA** solo por la copia en la comparación byte a byte: ForbiddenWriteScope 152-154 con un segmento `Systems/` añadido, y la concordancia de una Section de Authorities. A4 y A5 fail; STOP P-03; sin Worker |
| Replanificación única (PBT0-C08, F4B-E03 pregunta 2) | `R20261117T185020Z-e1ae` | Comparación byte a byte PASS en las 8 colecciones; A1-A8 8/8; 15 AcceptanceCriteria del Controller; sin OpenQuestions. No gasta intento |
| nc4 | `R20261117T185523Z-aa9d` | Oráculo cumplido: solo A3 fail con AllowedWriteScope ampliado |
| Worker (`claude-sonnet-5-5`/high, 437 turnos) | `R20261117T185616Z-3c1b` | RED `8f75a89b`: 21 métodos nuevos y los 20 mutantes de la tabla cerrada en los 6 archivos de MutantScope. GREEN `28e38d8d`: revierte los mutantes |
| CI del RED | `44174025134` | tests y ui-tests en failure, build-ui en success, build-plugin omitido; sin cuelgue ni timeout. Core 22 fallos (3 nuevos + 19 existentes) y UI 123 (18 nuevos + 105 existentes), todos atribuidos a mutantes (T-8) |
| CI del GREEN | `44175320836` | 4/4 success; Core 19 070/19 070; UI 2 404 pasadas y 17 NotExecuted previos; los filtros de T1B, Core 10/10 y UI 55/55, en verde en las dos CI |
| Revisión directa | — | Conforme en alcance, bytes, CI y mutantes. D-1 (proceso) para decisión del Owner; D-2 a D-5 menores |
| Revisión adversarial independiente | — | **REWORK 0/1/6** por I-01 (OWNER-1). Se resuelve con una disposición, sin código; el código, el alcance, el RED y el GREEN son correctos |
| Disposición del Owner | chat | **«Ratificar el cambio (Recomendado)»** (§3) |
| Verificación del Controller | `R20261117T215307Z-3cff` | `EXECUTION_VERIFIED` / NONE, 14/14, con la disposición como entrada |
| nc1 | `R20261117T215737Z-e427` | CONTROL PASS: Identity, S-04 |
| nc2 | `R20261117T220237Z-46b2` | CONTROL PASS: Scope detectado. Sin la firma de PD-F4-01 y sin sustituto |
| nc3 | `R20261117T220616Z-b457` | CONTROL PASS: FreeText, REWORK |

**Relevos:**
- 0 participantes y 0 no atribuibles persistentes en toda la cadena;
- Snapshot I y el binario acreditado `5449c1f0`;
- `main` = `812baf87`, sin tocar.

Las coincidencias «limit reached» de los registros de nc1 y nc3 son el texto de S-11 de AUTOMATION_PLAN que el Controller leyó, no un aviso de límite.

## 2. Identidad del producto y alcance (medido con git)

- `git diff --name-only f2eedd9a 28e38d8d`: los 10 archivos nuevos de pruebas de AllowedWriteScope, ninguno bajo ForbiddenWriteScope.
- `git diff f2eedd9a 28e38d8d -- src`: vacío.
- El RED toca en `src/` exactamente los 6 archivos de MutantScope, y el GREEN no toca `tests/`.
- En cada archivo mutado, el blob de la base = el del GREEN ≠ el del RED.

## 3. Desviaciones y disposición del Owner (F4B-E05)

**La desviación.**
- El Worker añadió a CT-PB0-17 el caso c-16 (`Dimensions = 3`) porque, con c-10 y c-11 (7 y −1), MU-PB0-16 no discriminaba:
  - `DynamicRackSystemDocument.ToDesign` normaliza `Dimensions` con `ValidDimensions` (`DynamicRackSystemDocument.cs:227` y `:429-432`) antes de la ventana;
  - la alternativa W:387 tampoco discrimina, porque `ToDesign` ya pone «kg» (`:205`).
- La cita `:322` de la entrega es la de `ToDomain()`; aquí queda corregida (M-02).
- Además, CT-PB0-17 fija c-12 y c-13 como `RECHAZADO <tipo>: <mensaje>`:
  - c-12 lo rechaza el almacén (`RackProjectStore.cs:343-353`);
  - c-13 hace lanzar al resolver (`DynamicRackSystemResolver.cs:473-474`).

**Las dos revisiones.**
- Coinciden en que la letra del contrato (INV-PBT0-14 y PBT0-C10) mandaba parar.
- La latitud «corrige la prueba si no caracteriza bien» venía solo del paso 3 del prompt del Worker, que redactó la sesión: es un defecto de la sesión.

**La decisión del Owner:** «Ratificar el cambio (Recomendado)».
- c-16 y la forma RECHAZADO quedan como enmienda de CT-PB0-17.
- La fila MU-PB0-16 se apoya en c-16.
- PBT0-C10 no se aplica a este intento, y no se gasta intento.

**Dato que deja la desviación:** W:387, W:406 y W:407 son inalcanzables desde un documento. Las sustituciones de (c) de esas líneas no tienen caso alcanzable (OB-06.5).

**Otras desviaciones declaradas, conformes:**
- la conjunción del RED aplicó MU-10 y MU-18 con la edición equivalente porque comparten contexto con MU-02 y MU-16; los parches aislados son los de la base;
- los filtros del RED se corrieron sobre un árbol idéntico al commit RED antes de crearlo.

## 4. Lo que PB-T0 deja fijado

- Los 21 CT-PB0, con el mapa CT → método de la entrega: 3 en Core y 18 en UI. Pasan a ser la red de PB-TC, PB-T1, PB-T2 y PB-T3, junto con T1B (65 pruebas).
- Las tablas T-1 a T-8 de la entrega (`worker-handoff.json`, Evidence).
  - T-3 (efecto del redondeo, CT-21) es el dato de OB-06.4 para la vuelta al Owner antes de PB-TC.
  - T-4 confirma H-PB-28.
- **El §6.4 de la fase quedó resuelto por el bloque M** (F4B-E04): fila 1. El «Actualizar» clásico no crea vistas, así que PB-T3 iguala sin crear vistas.

## 5. Límites y arrastres

**Límites de PB-T0:**
- solo pruebas nuevas, sin AutoCAD;
- ninguna prueba ejercita el teclado ni el foco real;
- la atribución T-8 de las 105 existentes de UI es por análisis, y la CI del RED es la evidencia de la unión.

| Id | Qué | Va a |
|---|---|---|
| M-01 | Fuga de una ventana inerte en c-13: `Abrir` crea la ventana y `LoadExisting` lanza sin `try` (`PushBackFullEditorCharacterizationSupport.cs:126-133`) | Paquete de PB-T1, como cambio declarado del ayudante (cerrar y volver a lanzar) |
| M-02 | Cita `:322` → `:227` y `:429-432` | Corregida en este registro |
| M-03 | Forma RECHAZADO de c-12 y c-13 | Ratificada (§3) |
| M-04 | CT-08 y CT-09 leen el estado del editor, no `Structure.Fronts` ni `Composite.AbsentSlots*` de la salida | Paquete de PB-T1 |
| M-05 | Aserciones de líneas (a)/(b) vacías en CT-01 y CT-17, fuera de CT-18 a CT-20 | PB-TC: comprobar que siguen vacías tras corregir (a) y (b) |
| M-06 | Falta evidencia guardada de MU-16 sin c-16 y de las mediciones M1-M9 | Lección para los prompts de Worker: guardar la salida de cada medición |
| D-3 | `PalletTolerance = 0`: `LoadExisting` lanza; RACKEDITAR informa del error (`RackCommandSupport.Report`) y no se cierra, pero el rack no se puede reabrir | Paquete de PB-T1 (la carga); prioridad baja |
| D-5 | Hueco de T-1: 20 controles sin una prueba que los cambie | Paquete de PB-T1, que decide si completa la red antes de mover el estado |
| Obs. | En CT-16, aceptar Seguridad sin cambios sobre `pb-05-topes` añade el portador de la familia de botas (W:2754-2763, I-42 S1G) | Paquete de PB-T1: comprobar si dibuja o entra en el BOM |

**Vueltas al Owner previstas, que este cierre alimenta:**
- **Antes de emitir PB-TC:**
  - el efecto medido de corregir (a) y (b) (T-3, con la lectura OWNER-4: R7 y R11 «materiales» por el BOM exacto y el emparejamiento por orden);
  - M-7 = 0/22 sobres con `DimensionStyle` (bloque M);
  - OWNER-2: H-PB-28, si se reproduce o se corrige.
- **Antes del contrato de PB-T1 (OB-06.5):**
  - OWNER-3: `pb-21-cola` pierde 667 hojas, la cola y `SideB`, al Actualizar con RACKEDITAR;
  - los demás casos alcanzables de T-2.

**Lección de la sesión:** el prompt del Worker no puede dar latitudes que el contrato no tiene. La cláusula del paso 3 heredada de T1B se retira de los prompts siguientes, o se lleva al contrato.
