# I-64 — Re-revisión focalizada adversarial del paquete de F4-T4B-MUTATE-POST (v2)

**Objeto:** `I-64-f4-t4b-gate-contract-package.md` v2, frente a la revisión de la v1 (`independent-review.md`, «IR») y a `_package-v1.md`. Los mapas M1 a M3 se usan como insumo, no como verdad.

**Base medida:** `c3ae1c970c908ef541840fc583680b86376cae0b`, solo lectura de fuentes y `git`. No se compiló ni se ejecutó ninguna prueba.

**Rutas:** relativas a la raíz del repositorio. `T4P`, `A2P`, `FRZ` y `TW` como en el paquete. «v2» = el paquete revisado, citado por número de línea del propio archivo.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Recuento: **0 bloqueantes, 7 importantes y 14 menores.**

- Los cuatro bloqueantes de la v1 dejan de serlo. El rediseño del puerto en ámbitos es implementable con las fachadas reales, y la corrida pura sí decide INV-06, INV-12 e INV-30 sobre valores de Application. No es una tautología.
- **Quedan siete importantes.** Ninguno obliga a otro rediseño, pero tres afectan al centro del diseño:
  - la puerta de reposo dentro de `TryUpdatePilot` choca con R-T4-06 (R-I-1);
  - la corrida no es total después de `Seal` (R-I-3);
  - la regla del origen vive en el Plugin y no fija el origen de la relectura de POST (R-I-5, R-I-6).
- **Las correcciones son de texto y de decisión.** Basta una comprobación dirigida de la v3 contra esta lista, sin otra revisión completa, salvo que el Owner elija en R-I-1 una alternativa que cambie el reparto con T4C.
- **Independiente del contenido:** las precondiciones de emisión del propio paquete siguen sin cumplirse (T4A2 en STOP y T1B sin ejecutar, v2 `:8-11`).

## Disposición de los hallazgos de la v1

| Hallazgo v1 | Estado | Motivo |
|---|---|---|
| **B-1.** `Operations` 15 → 16 | **Resuelto** | `TryUpdatePilot` es público y no entra en `Operations` (`TW/WorkspacePilotEditGuardTests.cs:44-61`, `:236-241`, `:305`). Quedan dos trampas de censo: R-M-3 y R-M-5 |
| **B-2.** El puerto como campo | **Resuelto para T4B** | Es un parámetro, y ningún campo del núcleo es de `Systems` (`TW/WorkspaceEditIntentHostTests.cs:1001-1021`). El cableado se traslada a T4C sin declararlo (R-M-12) |
| **B-3.** Granularidad y estado entre llamadas | **Parcial** | La forma en ámbitos es implementable y la corrida decide de verdad. Faltan: la totalidad después de `Seal` (R-I-3), las capas bloqueadas (R-I-4), los supervivientes calculados en el Plugin (R-I-5) y la forma de los valores (R-M-7) |
| **B-4.** `ReadForPrepare` sin entradas | **Parcial** | La regla es la de Build (b) y `input.InitialDraftState.Origin` existe. Pero la regla vive en el escritor, sin oráculo de Core y sin causa para «varios» (R-I-5). El origen de POST no está definido (R-I-6). La promesa de D-T4A2-06 solo se cumple en parte (R-M-11) |
| **I-1.** Avisos | **Resuelto** | Las faltantes llegan por `WriteUnit`, el `Regen` no se traga la excepción (`src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:157-163`) y la purga queda declarada (§9 I-5). Falta fijar cómo se compara el nombre (R-M-10) |
| **I-2.** Unknown(PostReread) en todas las ramas | **Resuelto** | La tabla va por rama. Error es alcanzable y compatible con R-3 y M-4 si se respeta la forma. Hay una trampa de texto exacto (R-M-2) y una inexactitud en H-01 (R-M-1) |
| **I-3.** Alcance incoherente | **Resuelto** | Por ruta exacta, sin condicionales, con `RackSiblingScan.cs` permitido, el soporte de pruebas permitido y UI.Tests, los `.csproj` y Fixtures prohibidos |
| **I-4.** `RequiredTests` | **Parcial** | La selección es la correcta, pero el filtro 2 no es un filtro (R-I-7) |
| **I-5.** Debug sin compilar en la CI | **Resuelto** | `[Conditional("DEBUG")]` más el build Debug como evidencia. Falta concretar la evidencia (R-M-14) |
| **I-6.** El movimiento de `Traverse` | **Resuelto** | Declarado, con su guarda y con Smoke-2. Es compatible con las cuentas de `TW/WorkspaceEditIntentGuardTests.cs:88-128` |
| **I-7.** Transacción ajena | **Parcial** | Existe la precondición `TOP_TRANSACTION_OPEN`, pero no la forma del rechazo (R-M-7) |
| **I-8.** La vía de D-13 | **Resuelto** | El §9 I-4 está reescrito con OV-RED no transferida y las dos alternativas |

Los M-1 a M-14 de la v1 están aplicados. Hay tres excepciones parciales:
- **M-2:** las causas sin texto (R-M-6);
- **M-6:** el enumerado sin nombres (R-I-2);
- **M-8:** los literales con constantes inexistentes (R-M-4).

## Respuestas a las comprobaciones pedidas

1. **Guardas del host (B-1/B-2):**
   - Ninguna guarda prohíbe nombrar `RackCad.Application.Systems` en el núcleo:
     - el núcleo ya tiene `using RackCad.Application.Systems.PushBack.Pilot;` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:5`);
     - la guarda de acoplamiento lo admite (`TW/WorkspaceEditIntentGuardTests.cs:509-523`);
     - la de campos solo mira campos (`TW/WorkspaceEditIntentHostTests.cs:1001-1021`);
     - la de firmas solo mira los métodos que nombra (`:957-999`).
   - `PassGate`, el texto prohibido en comentarios, `PortMutatingOperations`, `ChangesADraft`, el cuerpo de la guarda de escrituras, la contención como Failed y el drenaje y el Idle quedan bien declarados.
   - **No declarados:**
     - el corte `Skip(5)` (R-M-3);
     - las cuentas exactas de literales (R-M-4);
     - la excepción de firma a INV-F4T2B2A-06 (R-M-5);
     - la incompatibilidad de la puerta de reposo con T4C (R-I-1).
2. **Ámbitos (B-3):**
   - **Son implementables:**
     - `PrepareRedraw` fija `BlockId` y `PayloadJson` en `PreparedViewRedraw` (`src/RackCad.Plugin/Systems/Shared/PreparedViewRedraw.cs:24-49`), así que el texto del plan se liga en `PrepareUnits` y `WriteUnit(key)` solo elige la unidad;
     - las tres fachadas de Push Back tienen las dos primitivas (`src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs:56-64`, `PushBackPlantaDrawService.cs:51-58`, `PushBackSystemDrawService.cs:58-64`).
   - **La corrida necesita**, como valores de Application:
     - los hechos `RackSiblingScanFact` (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:16-52`);
     - los textos crudos por clave;
     - la evidencia de completitud;
     - y, para las capas, `RackSiblingLayerRequirement` (`src/RackCad.Application/Views/Redraw/ISiblingRedrawPort.cs:12-22`).
   - Con ellos construye `PushBackPilotIntentScan` y llama a `BuildStaleBase` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:64-68`). Todos existen.
   - La tabla por frontera coincide con D-13 (`FRZ:521-532`) en cuatro filas. Se aparta en una: la de «`Dispose` de `m` lanza tras un `Seal` que volvió» (R-I-3).
3. **Origen (B-4):**
   - La clasificación no depende del origen (`PushBackPilotIntentReader.cs:113-146`, `:119`), así que la StaleBase es la misma con cualquier elección.
   - Cuando la base coincide, la admisión da lo mismo que con el origen de navegación en todas las reglas que no dependen del origen: AUTH-13, `RackSiblingMembership`, la de tipo y la de materialización.
   - Las reglas 2, 5 y 6 sí dependen del origen (R-M-11).
   - OriginMissing con cero coincidencias no llega a verse: antes se da Conflicto (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:310-311`).
4. **I-1..I-8:** ver la tabla anterior.
   - `[Conditional("DEBUG")]` no choca con `DebugFaultInjectionGuardTests`, que solo lee `SiblingRedrawDebugFaultInjection.cs` y `SiblingRedrawTransaction.cs` (`tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:37-56`).
   - Las clases del filtro 2 existen con esos nombres; las de T1B, todavía no.
5. **§6:**
   - no necesita `IWorkspaceEditIntentReader.cs`, `WorkspacePilotSupport.cs` ni los dobles existentes del puerto, porque el puerto no cambia;
   - `TW/WorkspaceHostHardeningGuardTests.cs` no está permitido, y por eso las cuentas de literales tienen que quedar fijas de verdad (R-M-4);
   - una parada C03 puede saltar en falso por R-M-2, R-M-3 o R-M-5.
6. **Decisiones para el Owner que no están en el §9:** las recoge la sección final.

## Hallazgos nuevos

### Importantes

#### R-I-1. La puerta de reposo dentro de `TryUpdatePilot` choca con R-T4-06, y T4C tendría que reescribir el oráculo de la ranura

**Evidencia:**
- **v2, D-T4B-07, paso 1:** «La validación exacta … con `PassGate`».
- **`PassGate` exige** (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:1341-1374`):
  - un lector de la intención (`:1344-1348`), que `TryUpdatePilot` no usa;
  - y el reposo (`:1361-1365`), que el Plugin define como `Editor.IsQuiescent && !IsCommandActiveInActiveSession` (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:365-373`).
- **R-T4-06** (`T4P:552-558`) pide dos cosas:
  - «O-04 se hace dentro, en contexto de comando, con una vía de lectura **sin la puerta de reposo** y con la revalidación de la sesión»;
  - «`TryBeginApply` va antes de PREPARE, y `CompleteApply` en `finally`».
- **D-13, paso 2** (`FRZ:510-511`) pone PREPARE, MUTATE y POST dentro del contexto de comando.

**Problema:**
- `TryUpdatePilot` contiene a la vez la puerta, `TryBeginApply`, PREPARE, MUTATE, POST y `CompleteApply`. Para que MUTATE corra en contexto de comando, T4C tiene que llamar a `TryUpdatePilot` **dentro** del callback.
- Allí la puerta de reposo da Waiting («Hay un comando en curso.») si el editor no está en reposo, que es lo esperable en un comando. T4C lo mide (R-T4-05).
- Entonces T4C tendría dos salidas:
  - reescribir el método de T4B, que es el oráculo de INV-F4T1-20;
  - o dejar MUTATE fuera del contexto de comando, contra C-F4-02.

**Corrección (elegir una y llevarla al §9):**
- **(a)** `TryUpdatePilot` usa una puerta propia, **sin reposo ni lector**: el documento activo, el modal de RackCad y `Authorize` de la sesión por la forma exacta de `PassGate`.
  - Su guarda va a la manera de `PassGateViolations` (`TW/WorkspacePilotReconciliationGuardTests.cs:526-541`).
  - La puerta de reposo pasa a la operación del puerto de T4C, antes de entrar en el contexto de comando.
- **(b)** Se mantiene `PassGate` y se declara que T4C la sustituirá, con los cambios de guarda correspondientes.

#### R-I-2. `EditUpdateOutcome` no tiene nombres, orden ni correspondencia

**Evidencia:**
- **v2 `:319-321`** solo dice «su propio enumerado … con las diferencias como textos, la causa y los avisos».
- **El precedente fija por guarda** los nombres y el orden exactos de cada resultado del host, y que su valor por defecto es inerte (`TW/WorkspaceEditIntentHostTests.cs:1023-1047`).
- **Quedan sin correspondencia:**
  - los rechazos de `TryBeginApply` (`NotRegistered`, `InvalidStatus`, `PendingText` y `AnotherUpdateInProgress`; `src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs:108-123`);
  - los tres resultados de la comprobación previa;
  - los ocho `DraftApplyOutcomeKind`, más `ClosedDetached` y `NotApplying` de `DraftApplyResult` (`:139-146`);
  - el respaldo Error de §9 I-10.
- **`T4P:401`:** «Con PendingText, antes se cruza la frontera Update». R-T4-09 da `PilotBoundaryKind.Update` a T4C, así que en T4B `TryUpdatePilot` tiene que responder un estado de pendiente que el paquete no nombra.

**Problema:** el Worker inventaría la superficie que consumirá la UI de T4C (C07, la regla de fidelidad), y el revisor no tendría contra qué medirla.

**Corrección:**
- una tabla «situación → `EditUpdateStatus` (nombre y orden) → `Reason`, `Differences` y `Warnings`»;
- valor por defecto `Unavailable` inerte;
- `IsValueType`;
- y una prueba como la de `:1023-1047`.
- Las diferencias se leen de `draft.ConflictDifferences` con `DescribeDifferences` después de `CompleteApply`. Esa forma no toca `BaseOrReadUse` (`TW/WorkspaceEditIntentGuardTests.cs:647`).

#### R-I-3. La corrida no es total después de `Seal`, y el `finally` del host no tiene un resultado definido

**Evidencia:**
- **La tabla (v2 `:196-202`)** solo clasifica las excepciones del puerto. Quedan fuera tres casos:
  - **las de la propia corrida pura después de `Seal`:**
    - el comparador de D-T4B-05;
    - `Applied(null)` lanza (`DraftApplyOutcome.cs:87-91`);
    - `AppliedPostRereadFailed` con causa vacía lanza (`:101-104`);
  - **la del `Dispose` del ámbito exterior** después de `Post`;
  - **«`Seal` volvió y `Dispose` de `m` lanza»**, que la tabla da como CommitThrew, contra la fila congelada «`Commit()` retornó → Aplicado» (`FRZ:527`). Con la escritura ya hecha, «Comprobar ahora» daría un Conflicto falso seguro (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:612-630`).
- **El host (paso 6)** dice «`CompleteApply` en `finally`, exactamente una vez», pero no con qué resultado si la corrida lanza.
- **La guarda de contención** exige `catch (Exception ex)` → `.Failed(ex.Message)` (`TW/WorkspaceEditIntentGuardTests.cs:583-596`). La UI vería Failed para un Actualizar que pudo escribir.

**Corrección:**
- **la corrida es total:**
  - un indicador de `Seal` devuelto;
  - toda excepción posterior lleva a `Post` si aún no corrió, y si no a `AppliedPostRereadFailed(causa, avisos)`;
  - los `Dispose` que lanzan tras `Seal` se convierten en aviso;
  - la corrida nunca lanza;
- **el host** pasa a `CompleteApply` el resultado de la corrida, y `NotAppliedFailure` solo si la corrida no llegó a `Begin`;
- **el `EditUpdateOutcome`** sale del `DraftApplyResult`, nunca de un `Failed` suelto después de `TryBeginApply`;
- **dobles de Core:**
  - `Dispose` de `m` que lanza tras `Seal` → `Post`;
  - un comparador que lanza → `AppliedPostRereadFailed`;
  - `Post` sin relectura → `AppliedPostRereadFailed`.

#### R-I-4. Faltan las capas bloqueadas, que D-13 congela en PREPARE

**Evidencia:**
- **`FRZ:513`:** el paso 3 de PREPARE incluye «capas bloqueadas (I-55 V5 §4.5; O-05)», y `FRZ:1103` da O-05 como adoptada.
- **`T4P:407`:** «Fuera del plan, para T4B y T4C, … las capas bloqueadas, las piezas faltantes, la importación y los supervivientes». La v2 cubre las tres últimas y no las capas en ningún apartado (§2 a §8).
- **El precedente:** `LOCKED_LAYER` en `src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs:71-77`.
- **El barrido ya las recoge** por clave: `src/RackCad.Plugin/Views/RackSiblingScan.cs:61`, `:128-139` y `:155`.

**Problema:** el contrato no cumple un punto congelado. Sin la comprobación, una capa bloqueada solo se ve como excepción de `WriteUnit`, después de importar: queda un residuo OM-5 evitable y no hay una causa legible.

**Corrección:**
- `ReadForPrepare` y `Reread` devuelven las capas por clave `Mutable`;
- la corrida da `NotAppliedPreflight("LOCKED_LAYER: …")` antes de importar y otra vez en MUTATE;
- con una prueba de Core, una fila en el §5 y un escenario en Smoke-2;
- **o** se declara al Owner que pasa a T4C.

#### R-I-5. La regla del origen y los supervivientes viven en el escritor: sin oráculo de Core y sin causa para «varios»

**Evidencia:**
- **v2 `:172-176`:** el escritor elige el `OriginKey`, y `ReadForPrepare` devuelve «el resultado del piloto» y «el número de supervivientes» ya calculados en el Plugin (`:152-153`).
- **La corrida podría hacerlo en puro:**
  - `BuildStaleBase` sobre un barrido sin origen;
  - después, la elección por texto exacto entre los `Mutable`, como Build (b) (`PushBackPilotUpdatePlan.cs:341-345`);
  - después, `PushBackPilotIntentReader.Read` con esa clave;
  - y los supervivientes con `RackSiblingMember.IsSurvivor` (`RackSiblingMembership.cs:64`).
- **Nada lo impide:** la guarda que prohíbe `.Entries` y `.EnvelopeText` solo cubre los tres archivos del lector (`TW/WorkspaceDraftPilotIntentGuardTests.cs:102-133`), y no hay censo de llamadores de `PushBackPilotIntentReader.Read`.
- **«Varios» no tiene causa:**
  - con la clave vacía, el lector da OriginMissing, «el origen ya no está en el dibujo» (`PushBackPilotIntentReader.cs:162-166`), que es engañoso;
  - `SeveralOriginsReason` y `NoOriginReason` (`PushBackPilotUpdatePlan.cs:271-273`) quedan muertas en el camino de Actualizar.

**Problema:** reaparece, en la admisión, la tautología que la v2 quitó de INV-06: el doble de Core «decide» lo que el Plugin ya decidió. Una regla material de B-4 queda cubierta solo por Smoke-2.

**Corrección:**
- `ReadForPrepare` devuelve valores del barrido: hechos, textos, capas y catálogo;
- la corrida elige el origen, cuenta los supervivientes, comprueba las capas y construye el resultado del piloto;
- «varios» da una causa propia (por ejemplo `SeveralOriginsReason` como Preflight), declarada;
- pruebas de Core con cero, uno y varios;
- el escritor solo comprueba el documento y lee.

#### R-I-6. El origen de la relectura de POST no está definido: riesgo de Unknown(PostReread) en cada Actualizar correcto

**Evidencia:**
- **`Post()` no recibe nada** (v2 `:159-160`), y POST se relee «con `CaptureDrawing` sin cambios» (`:236`).
- **El comparador** exige la relectura de POST admitida (D-T4B-05, `:267`; `DraftSession.cs:578-597`), y la admisión exige un `OriginKey` (`PushBackPilotIntentReader.cs:162-166`).
- **Después de MUTATE** el sobre del origen ya no es `OriginText`: es el `EnvelopeText` del plan para `plan.OriginKey`.

**Problema:** si el escritor reutiliza la regla del texto con `OriginText`, no encuentra ninguna coincidencia, la relectura no se admite y todo Actualizar correcto termina en Unknown(PostReread).

**Corrección:**
- el origen de POST es `plan.OriginKey`, la clave de la definición, que no cambia;
- la corrida construye el barrido de POST con esa clave a partir de los valores que devuelve `Post`;
- una prueba de Core: un Actualizar correcto con el texto del origen cambiado termina Applied.

#### R-I-7. El filtro 2 de `RequiredTests` no es un filtro

**Evidencia:**
- **El esquema** da a cada fila un único `Filter` (`docs/automation/agent-execution/schemas/gate-contract.schema.json`, `RequiredTests`).
- **Los precedentes de I-64** usan una sola condición `FullyQualifiedName~<subcadena>` (por ejemplo `docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/delegation.json`).
- **El verificador de filtros de I-64 solo admite una condición** (`docs/automation/evidence/I-64-protocol-bridge/tools/verify_testfilter_bridge.py:51`, `:718-720`, `UNSUPPORTED_FILTER`).
- **La v2 (`:419`)** pone 14 nombres en una fila, sin la cadena del filtro.
  - `SiblingRedraw` y `RackSiblingRedraw` se solapan.
  - Con el prefijo de espacio de nombres (`RackCad.Tests`, `tests/RackCad.Tests/NamespaceFolderGuardTests.cs:210-248`), `RackCad.Tests.SiblingRedraw` no selecciona `RackSiblingRedrawUnitsTests`.

**Problema:** la fila no puede cotejarse en A5 sin inventar el filtro: STOP P-03 o una selección distinta de la prometida.

**Corrección:** una fila por clase, `FullyQualifiedName~RackCad.Tests.<Clase>`, con su `MinSelected` de la CI de la base y `ExpectRed` false. Las clases exactas son:
- `RackSiblingRedrawUnitsTests`, `DebugFaultInjectionGuardTests` y `SiblingRedrawSeamGuardTests` (`tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:7`, `:37`, `:58`);
- `RackSiblingMembershipTests`, `RackSiblingRedrawPlanTests` y `RackSiblingRedrawRunTests` (`tests/RackCad.Tests/RackSiblingRedrawTests.cs:9`, `:81`, `:114`);
- y las demás de la fila tal como se llaman.

### Menores

- **R-M-1. H-01 sobredimensiona el riesgo del comparador.**
  - En `ClosedDetached` (`DraftSession.cs:539-540`) `_explicitStatus` sigue en Applying, así que `Status` no evalúa el comparador. El error viene de IR I-2.
  - La fila «Todas» de D-T4B-06 sigue siendo correcta.
- **R-M-2. Trampa de texto exacto en `Drafts/`.**
  - `TW/WorkspacePilotPrepareGuardTests.cs:303-306` busca literalmente, con la sangría de 12 espacios, el bloque del error diferido de `DraftSession.cs:550-551`. Reestructurar `CompleteApply` con `try` puede cambiar la sangría, romper esa prueba y saltar C03.
  - **Corrección:** el bloque diferido queda idéntico byte a byte, y el respaldo Error usa la forma `_explicitStatus = DraftStatus.Error;` seguida de `ErrorReason = …;` en el mismo bloque (`:268-294`).
- **R-M-3. El corte `Skip(5)`** (`TW/WorkspaceLifecycleGuardTests.cs:527-534`).
  - Al añadir `TryUpdatePilot` al final de `PortMutatingOperations`, su cuerpo tiene que contener literalmente `_knownClean.Remove(request.Session)`.
  - Insertarlo en otra posición desplaza el corte.
  - **Corrección:** al final, y el paso 7 dice literalmente `_knownClean.Remove(request.Session)`, o se declara el cambio del corte.
- **R-M-4. Literales.** La tabla fija cuentas exactas (`TW/WorkspaceHostHardeningGuardTests.cs:421-453`, `:473-479`).
  - El rack en blanco (`:431`), el testigo sin clave (`:426`) y «se está aplicando o ya no está registrado» (`:438`) son literales en línea, no constantes (`WorkspaceHostCore.cs:374-379`, `:504`). «Usa las constantes existentes» no se puede cumplir para ellos.
  - **Corrección:** fijar el texto de cada salida temprana. El rack en blanco usa `NoDraftReason`; los rechazos usan constantes de la corrida. Declarar también que `ChosenReference` y `Witness` se ignoran y que no se llama a `HasWitnessWithoutKey`.
- **R-M-5. La excepción de firma no está declarada.**
  - D-T2B2-06 (INV-F4T2B2A-06) quiere las firmas del host sin tipos del piloto (`TW/WorkspaceEditIntentHostTests.cs:957-999`), y el segundo parámetro de `TryUpdatePilot` es de `Systems`.
  - Ninguna guarda lo detecta, pero «se añade al censo» puede leerse como meter el método en el bucle `:970-979`, que entonces falla.
  - **Corrección:** `EditUpdateOutcome` entra en los portadores (`:982`), `TryUpdatePilot` no entra en el bucle, y la excepción se declara (B-2 (a), §9 I-2).
- **R-M-6. Causas y comprobación previa.**
  - `Without(kind)` deja `Reason` nulo (`PushBackPilotUpdatePlan.cs:121-122`), y `WithCause` lanza con una causa vacía (`DraftApplyOutcome.cs:101-104`). Faltan las constantes de NothingToApply, NotApplicable y PendingText bajo Applying, de cero supervivientes, del destino, del documento equivocado y del texto de `TOP_TRANSACTION_OPEN`.
  - Además, `Prepare(From(draft), null, null)` da `Invalid(MissingInputReason)` para todo Dirty sin pendiente (`:308`). Hay que decir que la comprobación previa solo para en los tres tipos y que en cualquier otro sigue.
- **R-M-7. La forma de los valores del puerto.**
  - Los campos de `PushBackPilotUpdateRequest`, `PrepareRead`, `ImportResult`, `MutationRead`, `UnitResult` y `PostResult` están solo en prosa.
  - `BeginMutation()` devuelve un ámbito, pero tiene que poder «devolver un rechazo». Si es una excepción, cae en NotAppliedFailure, no en Preflight.
  - No se dice de dónde sale `AuthorizedDocumentNativeId` (¿`_probe.ActiveDocumentNativeId` tras la puerta?).
  - **Corrección:** la lista exacta de campos de cada tipo y un `PushBackPilotMutationStart { Scope?, RejectionCode? }`.
- **R-M-8. La pureza frente a los comentarios.**
  - El boceto del puerto dice «TopTransaction» en un comentario (v2 `:157`).
  - Si la guarda nueva lee el texto con comentarios, como su precedente (`TW/WorkspaceDraftPilotIntentGuardTests.cs:26-47`), salta con `Transaction`.
  - **Corrección:** decir qué lee la guarda. Ningún comentario del puerto, de la corrida ni de la ayuda dice `Transaction`; `TOP_TRANSACTION_OPEN` en mayúsculas sí pasa.
- **R-M-9. Las clases del escritor.**
  - «Implementa los tres interfaces» (`:216`) admite una sola clase. Entonces el estado de AutoCAD viviría en la instancia del puerto que T4C conserva, contra INV-14 e INV-F4T4B-08.
  - **Corrección:** tres clases; la del puerto sin campos de AutoCAD; `Begin` devuelve una instancia nueva; los campos `Transaction`, `ObjectId`, `Document` y `PreparedViewRedraw` solo en las dos clases de ámbito.
- **R-M-10. La comprobación del nombre.**
  - `SyncName` sanea con `BlockNaming.SanitizeBlockName`, compara con `OrdinalIgnoreCase` y se salta un nombre ocupado (`src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:30-46`).
  - El nombre esperado depende de la vista (`src/RackCad.Plugin/RackPushBackCommands.cs:282`, `:300-310`).
  - **Corrección:** fijar el esperado como `SanitizeBlockName(RackViewBaseName.Linked…(base))`, comparado sin distinguir mayúsculas, y con un `Name` en blanco no hay `SyncName` ni aviso. Si no, habrá avisos falsos.
- **R-M-11. La promesa de D-T4A2-06 (`A2P:349`) se cumple solo en parte.**
  - Las reglas 2, 5 y 6 dependen del origen (`PushBackPilotIntentReader.cs:162-188`). Con el origen de navegación de «Comprobar ahora» (`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:232-248`) pueden dejar un diagnóstico persistente, mientras que Actualizar, con el origen por texto (siempre `Mutable`), admite.
  - S2-R-05 y S2-R-06 son Conflicto y `TryBeginApply` los rechaza (`DraftSession.cs:512-513`), así que Smoke-2 no lo vería.
  - **Corrección:** decirlo en el §9 I-9.
- **R-M-12. El cableado de T4C queda sin declarar.**
  - Con la opción (a), la operación del puerto de T4C no tiene de dónde sacar la instancia:
    - el núcleo no puede tener un campo de `Systems` (`TW/WorkspaceEditIntentHostTests.cs:1001-1021`);
    - el puerto no puede llevar tipos del piloto (`TW/WorkspacePilotEditGuardTests.cs:180-206`);
    - el soporte no puede nombrar el piloto (`TW/WorkspaceEditIntentGuardTests.cs:509-523`);
    - el adaptador del controlador reenvía literalmente a la compuerta (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:51`, `:278-295`).
  - **Corrección:** decir en el §8 y en el §9 I-2 que T4C necesitará un cambio de guarda declarado, por ejemplo la opción B-2 (b).
- **R-M-13. Trazabilidad.**
  - INV-11 (`FRZ:749`, la clasificación de D-13 en Core) no está en el §1 ni en el §5, aunque las pruebas de la corrida son su oráculo.
  - La parte de escritura de INV-10 solo la cubren la guarda y Smoke-2: `WriteUnit(key)` no lleva el texto, así que el doble no puede ver qué texto va con qué clave.
  - **Corrección:** trazar INV-11, y `WriteUnit(key, envelopeText)` con comprobación de igualdad en el escritor, o declarar esa limitación.
- **R-M-14. La evidencia del build Debug.**
  - Fijar la orden, la configuración y una salida temporal, para no chocar con el DLL Debug que carga AutoCAD en Smoke-2.
  - Comprobar que la base ya compila sin avisos en Debug; si no, exigir «sin avisos nuevos».

## Decisiones materiales para el Owner que el §9 no recoge

1. **R-I-1:** la puerta de `TryUpdatePilot`. Sin reposo y en T4C (a), o `PassGate` con su cambio en T4C (b).
2. **R-I-3:** que «`Seal` volvió y su `Dispose` lanzó» vaya a POST, como pide la fila «`Commit()` retornó» de D-13, y no a CommitThrew.
3. **R-I-4:** las capas bloqueadas, en T4B o en T4C.
4. **R-I-5:** «varios orígenes» como Preflight con causa propia, en lugar de NotAdmitted.
5. **R-M-11:** la promesa de D-T4A2-06 solo para las causas que no dependen del origen.
6. **R-M-12:** el cableado del puerto en T4C, con su cambio de guarda.

Ninguna contradice la orden del Owner:
- la relectura queda dentro de MUTATE (TOCTOU, `owner-f4t4a2-authorization.md:307-317`);
- el plan sigue inmutable y de valores (`:320-334`);
- la escritura authored sigue siendo de T4B, con T4C como envoltorio del mismo escritor (§9 I-1, `:395-400`).

## Lo comprobado y correcto

- **Las guardas de D-T4B-07 que se declaran** existen tal como se citan:
  - `PortMutatingOperations` y `ChangesADraft` (`TW/WorkspaceLifecycleGuardTests.cs:463-480`);
  - `ReconciliationBodies` y su autoprueba por índice (`TW/WorkspacePilotEditGuardTests.cs:686-691`, `:765-778`);
  - la contención y el drenaje (`TW/WorkspaceEditIntentGuardTests.cs:583-640`);
  - el texto prohibido en comentarios (`:1002-1015`; `TW/WorkspacePilotEditGuardTests.cs:838-844`).
- **`TryUpdatePilot` no rompe:**
  - `BaseOrReadUse`, `PilotForms` ni la cuenta de 6 lecturas (`TW/WorkspaceEditIntentGuardTests.cs:647-729`, `:823-841`), si no nombra `pilot` ni `.Entries`;
  - la puerta por forma exacta, que solo mira el cuerpo de `PassGate` (`TW/WorkspacePilotReconciliationGuardTests.cs:526-541`).
- **El respaldo Error** cumple R-3 y M-4 si respeta la forma.
  - `Drafts/` no puede nombrar `Pilot` ni `PushBack` (`TW/WorkspaceDraftPilotIntentGuardTests.cs:232-244`), ni `Debug.`, `Transaction` ni `Commit(` (`TW/WorkspaceDraftBoundaryTests.cs:104-110`). La v2 no lo hace.
  - El parámetro opcional de avisos no rompe ninguna prueba existente: solo cambia la `Theory` declarada.
- **El movimiento de `Traverse`** es compatible con las cuentas de firma, construcción y `Commit` (`TW/WorkspaceEditIntentGuardTests.cs:88-128`). Ningún censo del producto cuenta llamadas a `CaptureDrawing` fuera de los archivos que ya las tienen.
- **`CallerOwnedFacadeGuardTests.Facades`** ampliada con las tres fachadas de Push Back pasa en la base. Sus cuerpos `=>` no contienen `Commit()`, `StartTransaction`, `LockDocument`, `Regen` ni importación (`tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:94-181`).
- **El filtro 1** funciona por prefijo: el espacio de nombres es `RackCad.Tests` y las clases empiezan por `Workspace`.
- **La inyección Debug con `[Conditional("DEBUG")]`** tiene precedente en `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:195`. `SEAL=before` y `SEAL=after` dan los dos caminos de S2-R-10.
- **Privacidad:** el paquete v2 no lleva rutas locales ni nombres de usuario.
