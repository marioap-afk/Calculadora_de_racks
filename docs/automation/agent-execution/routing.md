# Enrutamiento estable de la ejecución delegada

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Procedimiento de decisión del Controller de planificación (y de la sesión para elegir al propio Controller). **No nombra modelos**: la traducción de nivel y effort a un modelo concreto
está en [model-catalog.md](model-catalog.md), que es mutable y no normativo. Origen: Freeze de I-61 ([Proposal V9](../../initiatives/I-61-proposal-v9.md) §4).

## 1. Clases de tarea

| Clase | Perfil | Effort semántico de partida | Capacidades mínimas |
|---|---|---|---|
| Implementación mecánica | ROUTINE_IMPLEMENTATION | Routine | `tool-use`, `write-commit-push` |
| Propagación repetitiva / cableado | ROUTINE_IMPLEMENTATION | Routine | `tool-use`, `write-commit-push` |
| Implementación de pruebas | ROUTINE_IMPLEMENTATION | Balanced | `tool-use`, `write-commit-push` |
| Documentación | DOCUMENTATION | Routine | `write-commit-push` |
| Caracterización | CHARACTERIZATION | Balanced | `read`, `tool-use` |
| Depuración | DEBUGGING | Balanced | `read`, `tool-use`; `write-commit-push` si corrige |
| Causa raíz incierta | DEBUGGING | Deep | `read`, `tool-use` |
| Implementación transversal a capas | ROUTINE_IMPLEMENTATION (corta) / LONG_HORIZON_IMPLEMENTATION (larga) | Deep | `tool-use`, `write-commit-push` |
| Revisión de arquitectura / conformidad adversarial | ARCHITECTURE_REVIEW | Deep | `read` |
| Implementación agéntica larga | LONG_HORIZON_IMPLEMENTATION | Long-horizon | `tool-use`, `write-commit-push` |
| Controller: planificación / verificación | CONTROLLER_PLANNING / CONTROLLER_VERIFICATION | Balanced | `read`, `tool-use`; salida estructurada |

## 2. Dimensiones

Cada delegación registra siete dimensiones, cada una `Low`, `Medium` o `High`: ambigüedad, sensibilidad arquitectónica, amplitud, uso de herramientas, coste de fallo, repetición
mecánica y horizonte.

1. Si **alguna** de ambigüedad, sensibilidad arquitectónica o coste de fallo es `High`, el effort sube **un solo escalón**, con tope en `Deep`.
2. Repetición mecánica `High` lo baja un escalón; si coincide con la subida, se compensan. `Routine` es el suelo.
3. Horizonte `High` fija `Long-horizon`, por encima del tope.
4. Uso de herramientas `High` exige la capacidad `tool-use`.
5. Amplitud solo se registra; no cambia el effort.

## 3. Effort semántico y nivel de capacidad

Escala: `Routine` < `Balanced` < `Deep` < `Long-horizon` < `Maximum`.

| Effort semántico | Nivel de capacidad |
|---|---|
| `Routine` | Eficiente |
| `Balanced` | Equilibrado, o Eficiente con más effort |
| `Deep` | Equilibrado o Frontera |
| `Long-horizon`, `Maximum` | Frontera |

## 4. Algoritmo

1. Clasificar la tarea (§1) y puntuar las dimensiones (§2).
2. Filtrar por **elegibilidad** (§5) entre las celdas del contrato de gate. Si no queda ninguna: `EXECUTION_BLOCKED` de planificación, sin subir de nivel en silencio.
3. Elegir el nivel más bajo adecuado y el nivel de transporte **más preferido** (número menor de la jerarquía del mandato) disponible para esa celda. Registrar `RoutingReason`, con la
   fecha de verificación de cada entrada del catálogo que se use.
4. **Escalar** solo con `ModelEscalationReason` respaldado por evidencia, en este orden: effort → nivel → long-horizon → maximum, sin obligación de recorrerlos todos. La elegibilidad,
   incluido el consumo cubierto, vale también para el escalado.
5. **Enrutar hacia abajo** está permitido, con la razón registrada.
6. Escalar, bajar o cambiar de modelo, rol o sesión no reinicia `attempts` ni los contadores de clase.

**Sin celda elegible**, el Coordinator elige: (a) el siguiente nivel de transporte, con la limitación documentada; (b) una A-n de Coordinator + Architect que acepte
`RoutingEnforcement: advisory`, con la limitación declarada; o (c) BLOCKED — OWNER DECISION.

## 5. Elegibilidad de una celda

Una **celda** es modelo × transporte × capacidad (`read`, `tool-use`, `write-commit-push`, `effort-applied`). `effort-applied` se registra por valor de effort probado; en una celda
sin control de effort (por su modelo o por su transporte) se registra el effort **heredado medido**, que es el que la delegación pide.

Es elegible si, en la fecha de la delegación:

- está publicada según una fuente oficial;
- está instalada y autenticada localmente;
- tiene invocación probada (medida) para esa capacidad y ese effort;
- tiene el **consumo cubierto**: por una fila oficial de disponibilidad del plan con el que está autenticada la herramienta (URL y fecha versionadas), o por una invocación medida de esa
  celda con la autenticación existente por suscripción, sin clave de API de pago y sin aviso de límite de uso o de créditos. Lo desconocido no es elegible;
- no está `STALE`: mín(verificación + 90 días, retiro anunciado); un retiro con precisión de mes o trimestre cuenta desde el primer día de ese periodo.

**Sondas:** una celda con consumo desconocido solo se sondea si la autenticación es por suscripción, sin clave de API de pago, y ninguna fuente oficial la marca como de créditos o
API. Una sonda completada sin aviso de límite deja la celda como medida. La regla es igual para el Controller y para el Worker.

El `service_tier` heredado de la configuración del Owner no entra en este criterio: es un riesgo registrado, con tope de invocaciones y STOP P-06.

## 6. Exigencia del enrutamiento

`RoutingEnforcement` lo fija el contrato y lo copia la delegación:

- `required`: un modelo o effort efectivo distinto del solicitado hace fallar la comprobación `Routing` y la verificación no puede ser VERIFIED;
- `advisory`: `Routing` queda en `pass` y la discrepancia se anota en su `Evidence`, en `Findings` y en `Deviations` de la entrega.

## 7. El propio Controller

Su modelo y su effort los elige la sesión con este procedimiento (perfiles CONTROLLER_*), entre celdas elegibles. Se pasan con `-m` y `-c model_reasoning_effort=…`, y solicitado y
efectivo quedan en el registro de relevo. Una diferencia se registra como desviación; si el modelo efectivo no corresponde a una celda elegible, la salida es inválida.
