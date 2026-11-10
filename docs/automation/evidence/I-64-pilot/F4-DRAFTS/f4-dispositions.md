# I-64 F4 — Decisiones de la sesión

Registro append-only de las decisiones tomadas por la sesión durante F4, bajo la orden `coordinator/owner-f3-ratification-f4-authorization.md`. Solo aplican a I-64.

## F4-E01. Presupuesto de F4

- **Decisión del Owner:** F4 arranca con presupuesto propio, `attempts` = 1 de 3. El 2 de 3 de F3 no pasa a F4 y su historia queda cerrada.
- **Registro:** `automation_state.attempts` pasa a 1 en el commit que incorpora este archivo, antes de cualquier delegación de F4.

## F4-E02. Régimen de revisión de F4 y siguientes

- **Deuda de proceso (Owner):** un `EXECUTION_VERIFIED` 14/14 no estableció la corrección de producto de F3-T2, donde quedaban seis defectos reales.
- **Regla desde F4:** la verificación del Controller sigue siendo necesaria, pero no suficiente. Toda entrega sustancial de UI o de host recibe además revisión directa del código por la sesión, revisión adversarial independiente del diff y Smoke en host cuando el gate exige comportamiento de host.
- **Uso de intentos:** no se gasta un intento por una preferencia cosmética no contractual. Se corrige, gastándolo si hace falta, cuando un hallazgo incumple el Freeze, el contrato, la seguridad o el comportamiento observable.

## F4-E03. Línea base de `config.toml`

El Owner ratificó F3-E02. **Snapshot F** (`AAEB58D8…`) es la línea base activa para el trabajo del Controller. Snapshot E deja de aceptarse en las comprobaciones. Cualquier otro hash se vuelve a medir contra F y no se acepta por inferencia.

## F4-E04. Limitaciones heredadas de F3

| Id | Disposición del Owner |
|---|---|
| R-1 | Aceptada para F3. La semántica de obsolescencia de F4 es su autoridad natural |
| R-2 | Aceptada. Limpieza de texto posterior |
| R-3 | Aceptada. Deuda de experiencia de F5 o F7 |
| R-4 | Caracterización abierta. F4 no cambia la semántica del eco salvo que sea imprescindible |

**Obligaciones que se arrastran:** SN-06c; la parte de pestañas de SN-10; validación en host de que el manejo de avisos no lee el DWG; validación en host de que los racks borrados desaparecen; Owner Validation con racks reales representativos.

## F4-E05. Conformidad del producto de F3 con la redacción aceptada de IC-04

- **Hecho:** en `206e0f7a`, `AutoCadEventBridge.OnIdle` dispara `Drained`, el controlador abre la ventana y anuncia el estado, y la presentación lee el resumen. La lectura ocurre después del drenaje, pero sincrónicamente dentro de la llamada a `Application.Idle`. No ocurre en manejadores de objetos, de selección ni de comando.
- **Valoración:** la orden admite «después del drenaje», pero pide «una operación separada programada o visible para el usuario» y prohíbe leer «dentro de un callback de Idle simplemente porque se disparó». La frase admite dos lecturas y no se da por conforme por inferencia.
- **Decisión:** obligación explícita de F4-T2, que lo resuelve sacando esa lectura de la pila del Idle. Hasta entonces, la validación en host de F3 no se considera cumplida.
