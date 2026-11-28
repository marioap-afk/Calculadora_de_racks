# Revisión directa de la sesión — I-64 F4-T2C-IC04-READ-SCHEDULING, intento 1

- **Rango:** `d5d94a76..33e43f9f`, con el RED `703bda9d` y el GREEN `33e43f9f`; 7 archivos, exactamente el `AllowedWriteScope` del contrato.
- **Pruebas de la entrega:**
  - RED: 83 fallos por aserción en el filtro de Core;
  - GREEN: Core 2071/2071 en el filtro y 17609/17609 en la suite; UI 248/248.
- **Modo:** lectura del diff, del paquete v2 (D-T2C-01 a D-T2C-08) y de las órdenes del Owner.

## Veredicto

**Producto conforme.** Sin defectos de corrección frente a INV-F4T2C-01..11 ni frente a los puntos 1-13 del Owner.

## Lo comprobado

### `EventBridgeCore`
- `OnIdle` conserva su olvido `_lastAllowedClose = null;` justo tras el retorno por no iniciado.
- El predicado de elegibilidad sale a `TryGetDrainable`, compartido con `CanDrainNow`: iniciado, sesión activa en estado y en registro, no degradada, pistas pendientes, reposo, sin modal y panel visible. `TryDrain` lo vuelve a comprobar.
- La salida de `OnIdle` es la misma de antes: con condiciones falsas, `TryDrain` tampoco drenaba.
- `CanDrainNow`:
  - es una consulta contenida que devuelve false ante una excepción;
  - lee `_lastAllowedClose.HasValue` en una sentencia aparte de `_states`;
  - no asigna ningún campo.

### `WorkspaceIdleScheduler`
- **`OnIdleTick`**, en el orden de D-T2C-02: programado → sin post → `canRunNow`; después marca y llama al post una vez. Nunca ejecuta por sí mismo.
- **`Run`:** desmarca, vuelve a evaluar y solo entonces ejecuta. Una comprobación falsa o fallida suma `ContinuationsSkipped`, y `Run` no se reprograma.
- **Contención:** todo contenido, incluido el propio `logFault`.
- **Pureza:** sin tipos de host, sin estado estático y sin eventos.

### `AutoCadEventBridge`
- El manejador de Idle es `Contain("Idle", OnIdleTick)`, y `OnIdleTick` solo llama al planificador.
- `OnIdle` queda intacto y solo lo invoca `RunIdleContinuation` (`Contain("IdleContinuation", OnIdle)`), seguido de la comprobación Debug.
- El planificador se crea una vez en el constructor, con:
  - `_core.CanDrainNow`;
  - `LogFault`;
  - `SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action))`;
  - `_dispatcher = Dispatcher.CurrentDispatcher`.
- La comprobación Debug:
  - es `[Conditional("DEBUG")]` y estática;
  - usa `TransactionManager.TopTransaction` y no `.Database`;
  - registra con `LogFault`;
  - no tiene `Debug.Assert`.
- Ningún `+=` ni `-=` nuevo: siguen 16 manejadores, 16 `+=` y 16 `-=`.

### Cambios declarados
- **(1):** `CanDrainNow` entra en la lista y el comentario lo explica; el nombre de la prueba se conserva.
- **(2):** la excepción por archivo exacto para el puente (`BridgePath`), con la expresión del paquete. Se conservan la excepción de la paleta, su literal `string.Equals(path, PaletteHostPath, StringComparison.Ordinal)` y el número de pruebas.

### Pruebas
**Arnés de profundidad:** `Callback` sube y baja la profundidad, los lectores dobles la registran y un post doble encola. Hay una autoprueba del arnés: un post que se ejecuta en el acto lee a profundidad 1. Cubre:
- A-J y las unidades del planificador;
- la paridad `CanDrainNow` frente a `TryDrain`;
- el cierre pendiente, de la sesión activa y de una no activa, y que la secuencia de cierre sigue intacta;
- la desconexión de Idle.

**Guardas G-1 a G-10:** cada una con su autoprueba. Son negativas o se limitan a cuerpos propios, y no hay conteos exactos de archivo entero nuevos.

## Hallazgos

Ninguno de corrección.

## Riesgos para el smoke o Smoke-2 (no son defectos)
- La continuación real (`BeginInvoke`, prioridad Background):
  - su latencia y su orden;
  - si siempre llega; si se perdiera, `IsScheduled` quedaría en true y no habría más drenajes;
  - el bombeo dentro de bucles modales y de cierre.
- La temporización de pila en el host real no está demostrada. Es la limitación declarada en D-T2C-05.
