# F4-T4-HOST-C1: revisión directa de la sesión

**Objeto:** `git diff 16ce4445 b86c3cfe` (RED `61ffc828`, GREEN `b86c3cfe`; WorkRunId `R20261116T031655Z-e758`).

**Veredicto:** ACEPTABLE CON MENORES. **0 B / 0 I / 4 m.**

## Alcance

`git diff --name-only 16ce4445 b86c3cfe` da 11 rutas, que son exactamente las 11 del AllowedWriteScope, con las dos pruebas nuevas como `A`. Ninguna ruta cae bajo el ForbiddenWriteScope. El GREEN (`61ffc828..b86c3cfe`) no toca `tests/`, lo que el registro de relevo comprueba por aserción.

## Producto, frente al paquete

- **D-C1-01 / INV-C1-01:**
  - `BuildForMember` comprueba primero member nulo (`ArgumentNullException`);
  - después llama a `Build(member.DesignText, catalog)`, así que catálogo nulo, diseño ilegible y diseño que no es Push Back dan las mismas excepciones que `Build`;
  - deserializa el sobre con `RackEmbedStore`; uno ilegible da `InvalidOperationException(UnreadableEnvelopeReason)`;
  - `system.Name = envelope.Name` en crudo;
  - `Build` no cambia;
  - el escritor cambia una sola línea (`:118`).
- **D-C1-02 / INV-C1-02, 03 y 04:**
  - `ObjectEventSubject { Drawing, AssociativeBookkeeping }` en `BridgeSource.cs`;
  - en el puente, los cinco bloques de objeto tienen la forma exacta y `SubjectOf` es privado, estático y con solo dos pruebas `is`;
  - los demás bloques no cambian.
  - `OnObjectEvent`:
    - olvida la secuencia de cierre justo tras `if (!_started) return false;`;
    - fuera del caso de contabilidad en una fuente de objeto, devuelve `OnSourceEvent(session, source)`;
    - en ese caso hace las mismas dos comprobaciones de sesión que `OnSourceEvent`, devuelve `false` y tiene el mismo `catch` con `Fault`.
  - `OnSourceEvent` no cambia.
- **INV-C1-05, 07, 08 y 09:**
  - sin cambios en `WorkspaceHostCore.cs`, `WorkspaceSession.cs`, `Hints.cs`, la corrida, el adaptador ni la UI;
  - nada de regeneración ni de U, UNDO o REDO;
  - ningún tipo de AutoCAD en Application.

## Cambios de guarda (D-C1-03 / INV-C1-06)

| # | Guarda | Aplicado |
|---|---|---|
| 1 | `DeclaredCoreSurface()` + `OnObjectEvent` | Sí: la lista `c1`, la negativa `real.Where(n => n != "OnObjectEvent")` y un `Assert.Contains` (ver m-1) |
| 2 | Cuenta de `_lastAllowedClose = null;` 16 → 17 | Sí |
| 3 | `ForgettingEntries` + `OnObjectEvent` con la firma exacta | Sí, la firma literal del paquete |
| 4 | `ForgettingEntries.Length` 16 → 17 | Sí |
| 5 | `InlineData("OnObjectEvent")` en la teoría negativa | Sí |

## Pruebas

- **Prueba 1** (`TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope`): discrimina, porque con el cuerpo inerte `Name` es nulo. Cubre:
  - el nombre del sobre de cada miembro;
  - el nombre en crudo, con espacios y en blanco;
  - el nulo;
  - las cuatro excepciones.
- **Prueba 2** (`PrepareUnitsBuildsTheSystemOfEachMemberWithItsRackName`): una guarda de texto con cinco mutaciones del texto real.
- **3a, 3b y 3c:** discriminan, porque con el cuerpo inerte `OnObjectEvent` llega a `OnSourceEvent` y encola una pista ajena. La 3c lleva su propio control con `Drawing`, que sí marca.
- **Fijaciones:**
  - `Drawing` equivale a `OnSourceEvent`, dentro y fuera del ámbito propio;
  - toda fuente que no es de objeto equivale, con cualquier sujeto;
  - la secuencia de cierre se olvida con los dos sujetos;
  - el ámbito, el reposo y el contador no cambian;
  - las sesiones desconocida, degradada y destruida, y un núcleo sin `Start`, no tienen efecto;
  - `U` tras el Actualizar sigue siendo ajeno y marca, y un `U` que solo termina también marca.
- **Prueba 5** (`WorkspaceBridgeObjectSubjectGuardTests`): la forma de los cinco bloques; el clasificador, con 11 mutaciones; los otros bloques; 16/16 suscripciones; 6 `OnSourceEvent` y 5 `OnObjectEvent`; sin `Regen`; `ContainSession` intacta. Cada guarda tiene su negativa sobre el texto real.

## Menores

- **m-1:**
  - en `WorkspaceBridgeFacadeSourceGuardTests.cs`, la línea existente `Assert.Equal(4, declared.Count(...))` pierde el espacio tras la coma (`Assert.Equal(4,declared...`);
  - además, el Worker añade un `Assert.Contains("OnObjectEvent", declared)` que el paquete no pide.

  Es formato y una aserción redundante dentro del mismo método de la negativa del cambio 1. No cambia la semántica de ninguna guarda, así que no es la parada C1-S02. Se declara.
- **m-2:**
  - `OnObjectEvent` olvida la secuencia de cierre y, en la rama ordinaria, `OnSourceEvent` la vuelve a olvidar: un doble `null`, inocuo;
  - el olvido propio de `OnObjectEvent` lo exige la guarda `ForgetsBeforeAnyBranch` (cambio 3).
- **m-3:**
  - en la rama de contabilidad, las dos comprobaciones de sesión no tienen efecto observable, porque todas las salidas devuelven `false`. Es fiel al texto de D-C1-02 («las mismas comprobaciones»), y la prueba de sesión desconocida, degradada o destruida lo fija;
  - el escritor deserializa el sobre dos veces (`:115` y dentro de `BuildForMember`). Es un coste despreciable frente al redibujo.

- **m-4 (origen: el prompt de la sesión, no el Worker). La prueba 2 pasa ya en el RED.**
  - **Qué pasó:** el paso RED del prompt del Worker, redactado por la sesión, ponía `el escritor ya llama a BuildForMember` entre los cuerpos del RED, y el Worker lo cumplió. Por eso `PrepareUnitsBuildsTheSystemOfEachMemberWithItsRackName` (la prueba 2) no puede fallar en el RED.
  - **Qué dice la autoridad:**
    - D-C1-09 no pone la llamada del escritor entre los cuerpos inertes, y cuenta la prueba 2 entre las que fallan en el RED;
    - INV-C1-10 dice en general «las pruebas nuevas fallan por aserción».
  - **El paquete ya era incoherente para la prueba 5:** pone la forma nueva del puente en el RED y a la vez la lista bajo «pruebas RED».
  - **Efecto:**
    - ninguno en el producto;
    - la discriminación de la prueba 2 la demuestra su primera negativa, que sustituye la llamada por exactamente el texto anterior a la corrección (`Build(member.DesignText, _catalog)`) y exige que la guarda salte;
    - la prueba de comportamiento del nombre (prueba 1) sí falla en el RED de la CI (TRX de `43799572094`).
  - **La entrega lo declara:** en su Evidence, «Pruebas que pasan ya en el RED», lista la prueba 2.
  - **Lección:** al redactar el paso RED, no añadir a los cuerpos del RED nada que el paquete no enumere en ellos.

## Residuos declarados por la entrega (IC-10)

- un cambio que solo toque objetos `AssocAction` o `AssocDependency` sigue marcando dentro de una orden, por su fin, y no marca fuera de toda orden;
- las demás clases asociativas (`AssocActionBody`, `AssocDependencyBody`…) siguen contando;
- solo está medido D1.

Son coherentes con IC-10, y Smoke-2 los observará.
