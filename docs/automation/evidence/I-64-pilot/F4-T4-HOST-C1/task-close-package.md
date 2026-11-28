# I-64 — F4-T4-HOST-C1: paquete de cierre

**Resultado: CLOSED / PASS WITH LIMITATIONS**, en el intento 1 de 3.

**Autoridades del cierre:**
- la orden del Owner de corregir los errores de Smoke-2 antes de seguir (`F4-DRAFTS/coordinator/owner-f4-smoke2-fix-order.md`, F4-E74);
- el paquete v2 (`docs/initiatives/I-64-f4-t4-host-c1-gate-contract-package.md`, §8: «el cierre, por la regla D2»);
- la decisión D2 del Owner (`owner-f4t4c-decisions-d1-d6.md`).

Se cumplen todas las condiciones de D2:
- las dos revisiones con 0 bloqueantes y 0 importantes;
- la CI exacta y la verificación en verde;
- nc1 por el sustituto de la firma EXACTA de PD-F4-04, conservando su salida ordinaria, y nc2 y nc3 por su oráculo ordinario;
- ningún modo de fallo nuevo.

## 1. Estado resultante

**El producto:** `b86c3cfe`. Corrige dos de los hallazgos de la ejecución parcial de Smoke-2:
- **F4-E72, el nombre del rack:**
  - `PushBackPilotSystemRebuild.BuildForMember(member, catalog)` reconstruye el sistema de un miembro con el `Name` del sobre de ese mismo miembro, en crudo;
  - el escritor del piloto lo usa en `PrepareUnits`.
  - El «Actualizar» del panel deja de borrar el rótulo del nombre en las vistas redibujadas.
- **F4-S2-01 (y F4-S2-02, su consecuencia), la contabilidad asociativa:**
  - el puente clasifica el objeto de cada evento de objeto solo por tipo (`AssocAction` o `AssocDependency`), sin abrirlo ni leer nada de él;
  - el núcleo (`EventBridgeCore.OnObjectEvent`) descarta esos eventos sin pista ni marca.
  - Todo lo demás tiene exactamente el efecto de `OnSourceEvent`. Una orden de AutoCAD sigue marcando los borradores por su fin, y la secuencia de cierre sigue olvidándose.
  - IC-10 lo documenta.

**Lo que queda fuera:**
- **F4-S2-03** (la vista no se refresca tras `U` hasta `REGEN`) espera la decisión del Owner (D-C1-04, `F4-DRAFTS/c1/owner-decision-request-s2-03.md`);
- **el rendimiento del «Actualizar»** no cambia (D-C1-05).

**El resto:**
- **`attempts`:** 1 de 3;
- **main** sigue en `812baf87`.

## 2. Corridas

**La planificación** `R20261116T030940Z-6019`:
- fidelidad byte a byte PASS en las 8 colecciones, a la primera;
- A1-A8 8/8;
- **nc4** `R20261116T031345Z-c6d6`: A3 en fail, con el oráculo cumplido.

**El Worker** `R20261116T031655Z-e758`:
- `claude-sonnet-5-5`/high, 220 turnos, 0 denegaciones;
- prompt por archivo, con comprobación previa del SHA-256;
- RED `61ffc828` y GREEN `b86c3cfe`, con 11 rutas, exactamente las del AllowedWriteScope;
- el GREEN no toca `tests/`.

**La CI exacta:**
- **GREEN** `43801607010`: 4/4 en success. Core completa 19063/19063; Core Workspace 3482/3482; UI Workspace 694/694;
- **RED** `43799572094`:
  - Core en failure, con 11 fallos de 3482, todos dentro del filtro de Workspace y todos de pruebas nuevas o cambiadas: los cambios de guarda 2 a 5, las pruebas 3a, 3b y 3c, y la del nombre;
  - UI en success;
  - Build UI en success;
  - Build Plugin omitido por su needs.

**La verificación** `R20261116T042032Z-ba30`: **EXECUTION_VERIFIED, 14/14**, `VerifiedSha` `b86c3cfe`.

## 3. Revisiones y controles

**Revisiones** (`16ce4445..b86c3cfe`):
- **la directa de la sesión** (`direct-review.md`): **ACEPTABLE CON MENORES, 0/0/4**;
- **la adversarial independiente** (`independent-review.md`): **ACEPTABLE CON MENORES, 0/0/3**, sin hallazgos OWNER.
  - No encuentra ningún caso en que la clasificación oculte un cambio real del dibujo, fuera del residuo 1 declarado de IC-10.
  - Encuentra por su cuenta el mismo M-1 que la directa (m-4).

**Controles** sobre las entradas de `ba30`:

| Control | Corrida | Salida ordinaria | Resultado |
|---|---|---|---|
| nc1 | `R20261116T042516Z-ed82` | CONTROL NOT PASSED: Handoff primero y luego Identity, con BLOCKED/STOP; solo difieren Handoff, Identity y Ci | **Firma exacta de PD-F4-04**, en los cinco elementos literales del Owner. Sustituto `F4T4HOSTC1-NC1-IDENTITY-SIGNAL-01`: PASS, determinista, 7/7 |
| nc2 | `R20261116T043014Z-9a0e` | **CONTROL PASS:** Scope en fail con la ruta quitada | ordinario; PD-F4-01 no se reprodujo |
| nc3 | `R20261116T043436Z-8dcf` | **CONTROL PASS:** FreeText como unica comprobacion en fail, con REWORK | ordinario |

**Dos hechos de los controles, declarados:**
- **En nc1, `TriggeredStopConditions` sale vacía.** En las ocurrencias anteriores de PD-F4-04 llevaba S-04. La firma que fijó el Owner (`owner-f4t3-authorization.md`) no incluye identificadores de parada: sus cinco elementos se cumplen, así que no es un modo de fallo nuevo. El sustituto es el de T4C2, clonado solo con su id, su alcance y su etiqueta.
- **En nc2, la primera medición de procesos de entrada contó dos `bash.exe` de la propia sesión como participantes.** Su orden llevaba la ruta del worktree: el falso P-02 conocido. Se repitió desde su script, con 0 participantes. La medición falsa se conserva en `F4-T4-HOST-C1-nc2-recheck/1/R20261116T043014Z-9a0e/processes-entry-false-p02.json`, con SHA-256 `82dfdec54c0315fecc4f270293ec212be04a41a9509975496d38fb1e97d1a08c`.

## 4. Limitaciones

**Los menores de las revisiones.** Ninguno cambia el producto ni el comportamiento ratificado.
- **M-1 / m-4 (origen: el prompt de la sesión).** La prueba 2 (`PrepareUnitsBuildsTheSystemOfEachMemberWithItsRackName`) pasa ya en el RED.
  - El paso RED del prompt del Worker ponía la llamada del escritor a `BuildForMember` entre los cuerpos del RED. D-C1-09 no la enumera ahí, y cuenta la prueba 2 entre las que fallan en el RED.
  - Su discriminación la demuestra la primera negativa, que sustituye la llamada por el texto exacto anterior a la corrección.
  - La prueba de comportamiento del nombre sí falla en el RED.
- **M-2 / m-1:**
  - en una aserción existente de `WorkspaceBridgeFacadeSourceGuardTests.cs`, el Worker quita un espacio tras la coma;
  - además añade un `Assert.Contains` redundante en el método de la negativa del cambio 1.

  Ninguno de los dos cambia la semántica, así que no es la parada C1-S02.
- **M-3:** el comentario de la teoría negativa de `WorkspaceLifecycleGuardTests.cs:197-198` sigue hablando de tres entradas.
- **m-2:** el doble olvido de la secuencia de cierre en la rama ordinaria de `OnObjectEvent` es inocuo.
- **m-3:** dos detalles inocuos:
  - las comprobaciones de sesión de la rama de contabilidad no tienen efecto observable;
  - el escritor deserializa el sobre dos veces.

**Los residuos de IC-10:**
- un cambio que solo toque objetos `AssocAction` o `AssocDependency` sigue marcando dentro de una orden, por su fin, y no marca fuera de toda orden;
- las demás clases asociativas siguen contando;
- solo está medido D1;
- el evento asociativo posterior a `QUIT` deja la secuencia de cierre sin intacta, como antes.

**Fuera de esta tarea:** F4-S2-03, que espera la decisión del Owner, y el rendimiento.

**La validación física:** las pruebas no usan AutoCAD. La primera comprobación en el host es la reanudación de Smoke-2.

## 5. Siguiente

1. **La sesión:**
   - construye el DLL Debug del Plugin sobre `b86c3cfe` con AutoCAD cerrado;
   - prepara copias nuevas de los dibujos de Smoke-2;
   - cierra el anexo de reanudación de la guía v3. La guía no se modifica, por orden del Owner; el anexo solo dice qué esperados cambian.
2. **El Owner:**
   - decide sobre F4-S2-03;
   - opera la reanudación de Smoke-2 desde el bloque 1.
3. **RACKPROYECTAR** queda para su propia iniciativa (F4-E73).
