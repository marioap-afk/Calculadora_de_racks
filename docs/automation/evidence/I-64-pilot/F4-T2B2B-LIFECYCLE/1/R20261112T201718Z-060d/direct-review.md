# Revisión directa de la sesión — I-64 F4-T2B2B-LIFECYCLE, intento 1

- **Rango:** `32369c7b..8f249e3a`, con el RED `a2e0ec13` y el GREEN `8f249e3a`; 12 archivos, todos dentro del `AllowedWriteScope` exacto del contrato.
- **CI del SHA exacto:**
  - GREEN `8f249e3a`: success. Core: filtro 1919/1919, suite 17457/17457. UI: filtro 248/248, suite 1902 (1885 pasan, 0 fallan).
  - RED `a2e0ec13`: failure, con 128 fallos, todos dentro del filtro `RackCad.Tests.Workspace` y ninguno fuera.
- **Modo:** lectura del diff, de las autoridades del contrato y del paquete v8 (D-T2B2-09 a 15).

## Veredicto

**Producto conforme.** Sin defectos de corrección frente a INV-F4T2B2B-01..09 ni frente a la tabla de veto del Owner (F4-E29). Hay dos menores en las guardas nuevas, a contrastar con la revisión independiente.

## Lo comprobado

### Puente (`EventBridgeCore`)
- `SetLifecycleAdvisor` es el único miembro público nuevo: lanza `ArgumentNullException` con null, una segunda llamada sustituye al asesor y no olvida la secuencia (es configuración).
- `_lastAllowedClose` se olvida en las 13 entradas públicas que mutan estado, cada una tras su retorno por no iniciado. `OnBeginDocumentClose`:
  - olvida al entrar;
  - sin asesor permite;
  - con asesor adopta su decisión;
  - una excepción del asesor da Veto, registrada con `Log`, sin `Fault` y sin degradar;
  - solo recuerda un Allow.
- `OnDocumentToBeDestroyed`:
  - toma el valor recordado y lo olvida;
  - calcula `sequenceIntact` (la misma sesión y no degradada) ANTES de `_states.Remove`;
  - llama al asesor después de `_registry.Destroy` y de limpiar `_active`, y antes de `TrySyncIdle`, en su propio `try/catch` con `Log`.

### Plugin
- El manejador de cierre conserva en su línea `handler = (`, `ContainSession(` y `_core.OnBeginDocumentClose(a.Session)`, y llama a `e.Veto()` solo con Veto.
- Siguen los 16 `+=`, 16 `-=` y 16 manejadores.
- El controlador asigna el host como asesor una vez, en el constructor, sin `+=` nuevos.

### Registro (`DraftRegistry`)
- Las cuatro operaciones son aditivas y las ordinarias no cambian (no se debilita el registro en general).
- `DiscardByUser`, `DiscardAllByUser` y `RetireKnownSafe` no evalúan `IsDirty` ni llaman al comparador.
- `RetireConservative`:
  - mira primero el texto pendiente (perdido y nunca incierto);
  - después consulta el comparador, contenido borrador a borrador; si lanza, el borrador queda perdido e incierto, con el mensaje;
  - nunca infiere «limpio».
- `DraftLossNotice` gana `UncertainRackIds` y `UncertainReasons` (diccionario de solo lectura y ordinal) sin perder su constructor. Ninguna operación libera la ranura de Aplicando, como la semántica existente («la ranura es de la operación»).

### Host (`WorkspaceHostCore`)
- **Asesor explícito.** `AdviseClose` es `EvaluateClose`:
  - el texto pendiente veta sin comparador, porque `IsDirty` mira `_pending` primero;
  - una excepción del comparador se contiene como Veto;
  - la marca se pone solo como última acción de un Allow, y un Veto o un fallo la quitan.
- **`SessionDestroyed`.** Aplica `RetireKnownSafe` solo con la marca Y `closeSequenceIntact`, y `RetireConservative` en otro caso. Luego encola y registra el aviso solo si `HasLoss`, en dos `try` separados.
- **Tabla de veto del Owner:**

  | Caso | Resultado |
  |---|---|
  | Sucio conocido | VETO |
  | Texto pendiente | VETO |
  | Sucio desconocido (excepción) | VETO |
  | Limpio conocido | Allow |
  | Descartado explícitamente | `DiscardAllByUser` sin comparador, y el cierre siguiente permite aunque el comparador lance |

- **Marcado IC-08.** Ocurre antes de las salidas tempranas del drenaje y solo marca los borradores del piloto de la sesión. Todo drenaje quita la marca «limpio».
- **`TryReloadPilotEdit`.**
  - Sigue los pasos 1 a 8 con `PassGate` y una sola lectura que solo llega a `DiscardAndReload`.
  - La traducción de `DraftReloadStatus` es la de D-T2B2-12 (NotAllowed → Waiting en los dos casos).
  - Los estados NotFound, NotNeeded y Reloaded van al final del enum y `default` sigue siendo `Unavailable`.
- **`DiscardPilotEdits`.** Solo exige `Authorize`, no llama al comparador, quita la marca y contiene las excepciones como Failed. El `default` de `EditDiscardOutcome` es Rejected, con listas nunca null.
- **`NotePilotEdit`.** Sin compuerta, solo quita la marca.
- **N-4.** La documentación de `Rejected` nombra las tres vías.

### Cambios declarados en pruebas existentes (INV-F4T2B2B-12)
- La lista y el comentario de `TheCoreAddsExactlyTwoPublicMembersToWhatT2Left`, con el nombre conservado.
- El prefijo ordenado de N-1, más `default == Unavailable`.
- Nada más cambia en pruebas existentes.

## Hallazgos (menores)

### D-1 MENOR — `WorkspaceLifecycleGuardTests.cs:568` y `:680`: `Assert.Equal(5, Src.Files("src/RackCad.Application/Workspace/Drafts/").Count)`
Es una cota exacta nueva: antes solo había `NotEmpty`, en `WorkspaceDraftPilotIntentGuardTests.cs:235`. Por la regla I-1 bastaría una cota inferior o la existencia de los cinco archivos. Una tarea posterior que añada un archivo a `Drafts/` tendría que declarar el cambio.

### D-2 MENOR — `WorkspaceLifecycleGuardTests.cs:386`: `Assert.Equal(9, Src.Count(host, "_knownClean.Remove("))` sobre el archivo entero del host
Quitar la marca siempre es conservador, así que una cota inferior protegería lo mismo.

No bloquea a F4-T3 ni a F4-T4: su obligación es llamar a `NotePilotEdit`, que no cuenta. Sí obligaría a declarar un cambio si una operación nueva del host quitara la marca directamente.

Los demás conteos exactos de archivo entero ya existían en guardas anteriores; las nuevas los duplican:
- 16 `+=`, 16 `-=` y 16 manejadores del puente;
- 3 `+=` del controlador;
- 9 archivos del Plugin;
- 45 pruebas de `WorkspaceHostSourceGuardTests`.

## Riesgos para Smoke-2 (no son defectos)
- Si el orden real de `BeginDocumentClose` y `DocumentToBeDestroyed` en CLOSEALL y QUIT intercala los cierres de varios documentos, la secuencia nunca queda intacta. La retirada sería conservadora (segura), pero con posibles falsos inciertos si el comparador falla. Aplica la regla de devolución al Owner.
- Un cierre cancelado sin entradas posteriores, seguido de una edición sin `NotePilotEdit` y de una destrucción sin su cierre, es el residual declarado. Lo cubre la obligación de `NotePilotEdit` en F4-T3 y F4-T4.
- `Stop()` destruye sesiones sin el asesor (residual declarado en D-T2B2-09).
