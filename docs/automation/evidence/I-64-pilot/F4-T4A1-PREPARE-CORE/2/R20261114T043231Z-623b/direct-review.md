# I-64 F4-T4A1-PREPARE-CORE, intento 2: revisión directa de la sesión

**Objeto:** el diff `443b5b3a..bbd9a35e` (RED `921e3a36` y GREEN `bbd9a35e`), con 21 rutas, exactamente el `AllowedWriteScope` del contrato del intento 2.

**Método:** lectura del diff de producción completo y de los cambios de las guardas existentes, frente al paquete v3 (D-T4A1-01 a D-T4A1-08), al contrato del intento 2 y al análisis del intento 1.

**CI exacta:**
- GREEN `43554278196` 4/4: Core 2843/2843 en el filtro (18381/18381 en la suite completa) y UI 458/458;
- RED `43553634960`: Core con 97 fallos por aserción, todos dentro del filtro, y UI 458/458, con `ExpectRed` false;
- el GREEN no toca `tests/`.

## Veredicto

**Sin defectos bloqueantes ni importantes. Hay 3 menores.**

## Comprobado

**Núcleo** (`DraftSession.cs`):
- `ConfirmBase` cumple la tabla de cinco filas.
  - NotApplicable fuera de Clean, Dirty y Conflict.
  - Con un MATCH sin estado explícito, solo limpia `IsPossiblyStale`.
  - En Conflict con MATCH responde Confirmed **y sigue en Conflict**, así que INV-F4T1-11 no cambia.
  - Con una diferencia, deja el estado explícito en Conflict con las diferencias.
  - No toca la base, el inicial, el actual, el pendiente, `LastApplyCause` ni `Warnings`.
- La sobrecarga de «Conservar» sigue el orden de D-T4A1-02:
  - `keepOn` nulo lanza primero;
  - después NotAllowed, NotConfirmed y NotAdmitted;
  - copia el inicial y calcula el sustituto antes de `Rebase`.

**Materializador:** (6) rechaza NaN, infinita, 0 o negativa; (1) compara `current` tal cual contra la copia del inicial con la escala neutralizada; (3) exige que la base rematerializada sea igual a `initial`, con el comparador del piloto.

**Host:**
- **Los tres cuerpos** siguen los pasos 1 a 8, con la aplicabilidad antes de la única lectura y los textos A1-01 y A1-02 como constantes.
- **El diagnóstico de admisión** se lee exactamente como `pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic`, sin alias. La guarda nueva fija que el resultado del piloto solo se llama `pilot` y se asigna una vez por lectura, y su autoprueba detecta el rodeo del intento 1.
- **`_knownClean`** se quita en Conflict, Kept y Reloaded, después de la lectura.
- **Las implementaciones explícitas** ponen la marca de lectura con `try`/`finally`.
- **La vista** lleva `unknownOrigin` (solo en Unknown), `conflictText` y `lastCause`.

**Plan** (`PushBackPilotUpdatePlan.cs`):
- La instantánea se construye con `From` y `ConsideredStatus`.
- Los siete resultados van en su orden. NothingToApply va antes de leer, y Conflict antes de la admisión.
- Invalid cubre los casos (a)-(e), con el origen único por texto.
- **`PreflightInnerSources`** de Application se llama con el proyecto del origen como iniciador.
- **El sistema** se construye con `PushBackCompositeEditorAssembler` y `PushBackResolver`, y la disponibilidad se evalúa con `PushBackViewAvailabilityFacts`. Un miembro no disponible da Invalid: **nunca se borra una vista**.
- **Vista y sección** salen como en RACKEDITAR.
- **El diseño saliente** se escribe por vista con `WithSourceMetadataFrom` sobre el proyecto resuelto.
- **El sobre** se compone sobre el sobre releído, con `source.Id` y `source.Name`, en el único miembro `ComposeMemberEnvelope(RackEmbedDocument source, ...)`.
- **El plan** solo guarda cadenas y valores, con igualdad explícita.
- **Sin escrituras:** ninguna transacción, bloqueo, `Commit` ni API de escritura.

**Guardas existentes:** solo cambian las declaradas.
- La exención de `ConfirmBase` en `WorkspaceDraftRegistryTests.cs`, con el nombre de la prueba actualizado como permite INV-F4T4A1-12.
- TEnv12, de 10 a 11.
- El censo de `Compose`, con su comentario.
- La marca «conocido limpio», con tres operaciones más.
- `AllowedConsumers` y el uso declarado del diagnóstico.
- La contención como Failed sobre los cinco cuerpos.
- `PilotFieldView` con sus tres parámetros opcionales, la tabla de literales de 28 a 30 con las cuentas antes/después y R-3 por cuerpo.
- Las operaciones y las lecturas del puerto.
- C2-M-8 como `Fact`.

Las líneas eliminadas son reescrituras que amplían: ninguna aserción se pierde sin sustituto.

## Menores

| Id | Referencia | Hallazgo | Tratamiento |
|---|---|---|---|
| D-1 | `WorkspaceEditIntentGuardTests.cs` (guarda del alias) | Fija con igualdad exacta 6 asignaciones `var pilot = read.PilotRead;` y 6 llamadas `.ReadIntent(` en el host. Una lectura nueva del host en F4-T4B o F4-T4C tendrá que cambiarla de forma declarada | Se arrastra a los paquetes de F4-T4B y F4-T4C como cambio previsto |
| D-2 | `PushBackPilotUpdatePlan.cs` (`GetHashCode`) | Los hash del plan y de la expectativa solo mezclan cuentas, no contenidos | Coherente con `Equals` y sin uso como clave. Se anota |
| D-3 | `PushBackPilotUpdatePlan.cs` (`UnreadableOriginReason`) | Rama defensiva que el Worker declara inalcanzable, porque (a) falla antes | Aceptable como defensa. Se anota |
