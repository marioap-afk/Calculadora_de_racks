# ADR-0049: Parámetros calculados de solo lectura, namespace built-in `rack` del motor de expresiones y resumen de proyecto

- **Estado:** propuesto
- **Fecha:** 2026-10-05 (propuesto)
- **Decisores:** Owner del proyecto (acepta o rechaza; pendiente). Coordinador y Arquitecto de I-63 (consenso técnico
  pendiente sobre este archivo exacto). Claude (redacción, sesión principal de I-63).
- **Sucesor parcial de:** [ADR-0043](0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md).
  **No lo reemplaza.** ADR-0043 sigue `aceptado` y vigente en todo lo que este ADR no modifica o sustituye
  expresamente. Al aceptarse este ADR, ADR-0043 recibe una nota posterior fechada que remite aquí.
- **Iniciativa relacionada:** I-63 (ID20) — `architecture/parametros-calculados-resumen-proyecto`
  ([contrato](../initiatives/I-63-parametros-calculados-resumen-proyecto.md),
  [Discovery](../initiatives/I-63-discovery.md), [registro](../automation/decisions/I-63.md),
  [evidencia](../automation/evidence/I-63-evidence.md)).

## Base exacta

- **Freeze:** [Proposal V3](../initiatives/I-63-proposal-v3.md), congelada en `f61d0aca859a11b15cbe1797069a83cba873ba95`
  (blob `4d5dedce15363fa6378e460c5b63005fe41b858b`). Su anexo A es el borrador cláusula por cláusula de este ADR.
- **Enmiendas, todas solo del Coordinator:**
  - [A-1](../initiatives/I-63-proposal-v3-amendment-a1-verificacion.md): verificación de INV-29 (b), INV-20 e INV-14/34;
  - [A-2](../initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md): secuencia de INV-11 e INV-32 entre G2 y G4;
  - [A-3](../initiatives/I-63-proposal-v3-amendment-a3-inv22-diagnostico.md): resultado diagnóstico de INV-22;
  - [A-4](../initiatives/I-63-proposal-v3-amendment-a4-ready-debts.md): deudas no funcionales antes de READY.
- **Gates funcionales en PASS:**
  - G1: métricas por rack;
  - G2: población, veredicto único y agregados;
  - G3: built-ins en I-49, con revisión de arquitectura CONFORMING;
  - G4: `ProjectSummary`, provenance y rendimiento.
- **Materialidad (Proposal V3 §3):**
  - activados: M-01, M-04, M-05, M-06, M-07 y M-08;
  - no activado: M-02;
  - no activado con las condiciones congeladas: M-03.

## Contexto

- **El hueco que deja I-49.** ADR-0043 entregó el motor de expresiones de I-49 con un solo namespace activo, `projectVariable`. Reservó
  conceptualmente `rack` y `project` para ID20 (D24, D25; V6 P25.2-P25.5).
- **Lo que hacía falta.** RackCad no tenía una autoridad única para:
  - las métricas de un rack: frentes del fondo 0 y frentes vacíos;
  - la población cotizable de un proyecto;
  - sus agregados.

  Existían tres conteos de racks divergentes (RACKLISTA, RACKBOMTOTAL y «N racks · M copias»). Además, la puerta de salida del Push
  Back se componía dentro del handler del Plugin.
- **Las restricciones.** I-63 tenía que:
  - dar esas autoridades en Application pura;
  - exponer las métricas por rack a fórmulas mediante el motor de I-49, sin un segundo motor;
  - no persistir nada nuevo;
  - no cambiar el comportamiento de `projectVariable` ni de los consumidores existentes.

## Decisión

### Se conserva de ADR-0043 sin cambios

- D1 (núcleo neutral). El contexto de rack calculado vive fuera del núcleo, en `RackCad.Application.ComputedParameters`.
- D2 (motor numérico adimensional y fronteras de validez).
- D3 (unidades).
- D7 (gramática y funciones). La gramática `palabra.miembro` ya existía; lexer y parser no cambian.
- D8 (límites).
- D10 (persistencia de variables).
- D11 (persistencia híbrida de propiedades).
- D12 (extensión de ADR-0034).
- D13 (Schema V-0).
- D14 (fallo estructural frente a semántico). Un namespace no persistible en un payload sigue siendo fallo estructural.
- D15 (dependencias, grafo y ciclos). `Computed` es hoja.
- D16 (`RegistryEvaluation`, única autoridad de valores de variables). `Computed` nunca entra.
- D17..D23.
- D5 completo en lo que toca a `projectVariable`: identidad, regla de clave, comparador, cualificador y formato persistido.
- V6 P25.6 y P26: los valores calculados vienen de *snapshots* puros, y el núcleo no lee AutoCAD, Domain ni catálogos.

### Se modifica

| Cláusula | Modificación |
|---|---|
| **D4** | La canonicidad P2.5 (`Format → Parse → Bind` devuelve el mismo árbol) se extiende a árboles con referencias `rack` en los contextos que ofrecen `rack`. Un id `rack` **ausente** tiene una forma diagnóstica que se muestra y nunca enlaza, `Rack.#{token}`. Su resultado concreto lo fija la A-3: para una clave que no es un GUID legible por `Guid.TryParse` (todas las del catálogo cerrado), el lexer rechaza `#{token}` como **un** `InvalidQualifier`, sin árbol |
| **D5** | Hay dos namespaces activos **en memoria**, `projectVariable` y `rack`, y uno **persistible**, `projectVariable`. La validez de clave es por namespace: `rack` admite un *token* ASCII que empieza en minúscula, con letras y dígitos, comparado `Ordinal`. `project` sigue **reservado**: sin miembro, sin *token* y sin regla en el núcleo |
| **D6, regla 4** | `palabra.` sigue siendo sintaxis de namespace. `Rack.<miembro>` solo enlaza donde el contexto ofrece `rack` (tabla con entradas `rack`); en otro caso da `UnknownNamespace`, como antes. Donde se ofrece `rack`, `Rack.` sin miembro da `NameRequired`. `Project.X` y cualquier otra palabra siguen dando `UnknownNamespace` |
| **D6, reglas 5-6 y nombres** | La búsqueda por nombre, los homónimos (`AmbiguousName`) y `OperatorInName` se aplican **por namespace**. Las referencias sin namespace (simples, con llaves o cualificadas) solo buscan en `projectVariable` |
| **D6, formatter** | Una referencia `rack` presente se escribe siempre `Rack.<miembro>`, nunca con `Q(key)` ni llaves. Los homónimos se cuentan solo entre `projectVariable` |
| **D9** | La tabla de *tokens* **persistidos** no cambia: `Namespace` = `projectVariable`. Se añade una tabla **separada** de namespaces en memoria. Leer `"Namespace":"rack"` da `PresentButUnreadable`; escribir un árbol con una referencia `rack` es un error de programación y no escribe nada. Se conserva la regla de evolución: un namespace nuevo en un payload es *fail-closed* |
| **D24** | ID20 deja de ser solo preparación. Son productivos el namespace `rack`, el caso de definición `Computed` y el ámbito `Rack`; `project` sigue reservado. ID23 y ID28/29 siguen como antes, con `RackComputedExpressionContext` como vía de ID23 |
| **D25** | «ID20 productivo» sale de fuera de alcance solo en lo que implementa este ADR |
| **V6 P25.2** | Pasan a productivos `Rack.*`, el ámbito `Rack` y el caso de definición `Computed`; `Project.*` sigue reservado |
| **V6 P25.3** | La reserva gramatical se conserva para `Project` y para los contextos que no ofrecen `rack`. Deja de valer dentro del contexto de rack calculado |
| **V6 P25.4** | «La regla de ámbito… sin crear ningún símbolo `Rack` productivo» deja de ser cierta: existen símbolos `rack` productivos de ámbito `Rack`. La regla de ámbito no cambia: un consumidor `Project` con una entrada `rack` da `ScopeViolation` |
| **V6 P25.1** | Descripción histórica desactualizada, sin cambio normativo. Tras este ADR hay dos namespaces en memoria y el ámbito `Rack` es productivo |

### Se sustituye

| Cláusula | Sustitución y motivo |
|---|---|
| **V6 P25.5** («el contexto de P6.7, evaluación de una propiedad de rack, es donde ID20 añadiría símbolos del rack») | Los símbolos `rack` viven **solo** en `RackComputedExpressionContext`, que se construye después del *resolve* de ese rack. El contexto de propiedad **no** los ofrece. Motivo (R1): `Rack.*` depende del resuelto (Φ3), que depende de las propiedades vinculadas; ofrecerlos ahí crearía ciclos como `VerticalClearance → altura resuelta → fórmula de VerticalClearance` |

### Decisiones nuevas

1. **Dos identidades (Freeze D-02).**
   - `MetricId` = `(MetricScope: Rack | Project, token)` identifica toda métrica del catálogo, en resultados, resumen y provenance.
   - `SymbolId(rack, token)` existe solo para las ComputedParameter dentro del motor.
   - El catálogo declara la correspondencia 1:1 solo para ellas. Las métricas de proyecto nunca son símbolos.
   - Los *tokens* se declaran explícitamente y nunca se derivan de nombres visibles.
2. **Fuente única por métrica (D-07).**
   - `(Rack, frentes)` y `(Rack, frentesVacios)` salen de las bahías del fondo 0 del sistema Selectivo **resuelto**.
   - `totalRacks` y `rackCount` salen de la población de I-63.
   - Los totales de frentes son la suma de los valores por rack de los incluidos.
   - Ningún *builder* de vista, BOM o etiqueta es fuente.
3. **Precedencia única por rack (D-28).** Una tabla total y ordenada (kind → soporte → E5 → E4 → efectivo/resuelto → `Available`) da el
   estado de cada métrica `(Rack, *)`.
   - La usan igual `RackMetricRequest` y `RackSummary.Metrics`, mediante una sola implementación.
   - `NotApplicable`, `NotSupported` y `Unavailable` son estados distintos y nunca se colapsan en `BrokenReference`.
4. **Población cotizable (D-10..D-12, D-26).**
   - Pasos E1..E6 en orden congelado: identidad, colocación, kind coherente, diseño legible, autoridad *authored* y veredicto de salida.
   - Deduplicación por RackId `OrdinalIgnoreCase`, con grafía canónica.
   - Agregados que nunca son parciales: un agregado es `Available` solo con la cobertura acreditada.
   - Un lector de diseño por kind.
5. **Veredicto de salida único (D-27).** `RackOutputVerdict` (Application) compone la puerta de salida del Push Back. El handler del
   Plugin delega en él y conserva su comportamiento observable (*fail-open* y catálogo nulo normalizado a vacío).
6. **Disponibilidad por consumidor (D-14).** `Rack.*` solo existe en `RackComputedExpressionContext`. Las fórmulas de propiedad
   (RACKEDITAR) y las definiciones de variable (RACKVARIABLES) no lo ofrecen. Habilitarlo en otro consumidor exige demostrar que su
   fase es anterior e independiente, con prueba y una enmienda aprobada por Arquitecto y Coordinador.
7. **Ciclos (D-15, R1..R6).**
   - Ningún símbolo `rack` está disponible en Φ2 ni antes.
   - El contexto calculado se construye solo con resultados terminados, y evaluar nunca resuelve.
   - Los *providers* son puros.
   - `Computed` es hoja, y `RegistryEvaluation` y `DependencyGraph` la rechazan como error de programación.
   - Ningún consumidor escribe en un rack a partir de un valor calculado.
   - No hay símbolos `project`.
8. **Persistencia cerrada con dos tablas (D-18).** Ver la modificación de D9.
9. **Resumen de proyecto neutral (D-20).**
   - `Population` devuelve `ProjectPopulation`, sin métricas por rack.
   - `Full` devuelve `ProjectSummary`:
     - `Totals`;
     - `BySystem` con seis sistemas en orden fijo;
     - `Racks` con todos los RackIds atribuibles y sus `Metrics` por la misma tabla D-28;
     - `Diagnostics` en orden determinista.
   - Es puro, inmutable, en memoria y **no persistido**. Los consumidores presentan; no recalculan.
10. **Provenance en memoria (D-21).**
    - Cada métrica del resumen conserva su `MetricId`, el RackId, un identificador de autoridad de una tabla cerrada
      (`selective.resolved.fondo0.bays`, `selective.resolved.fondo0.emptyBays`, `population.cotizable` y `aggregate.sum`), la fase, el
      representante y los *outcomes*.
    - Los agregados conservan los RackIds incluidos y excluidos con su motivo.
    - La provenance queda **fuera** de la igualdad de `MetricValue`.
    - `RackComputedExpressionContext` conserva el árbol evaluado y los `SymbolId` leídos.
11. **Rendimiento (D-24).**
    - Por petición: una captura, una lectura del registro y un catálogo.
    - Una resolución por RackId (Selectivo) en `Full` y ninguna en `Population`; nunca por vista.
    - La caracterización usa contadores como oráculo, y los tiempos solo se registran.
12. **Capas (D-25).**
    - Todo en Application pura.
    - El contrato de entrada (captura, lectura del registro y `CatalogInput` = `Loaded | LoadFailed`) lo construyen hoy las pruebas.
    - Ningún componente nuevo del Plugin ni de la UI.

## Alternativas consideradas

- **Ofrecer `Rack.*` en el contexto de propiedad (V6 P25.5).** Se descartó por los ciclos de R1.
- **Introducir `project` en el núcleo y hacer bindable `Project.*`.** Se descartó por R6: leer un agregado de proyecto desde un rack
  exigiría resolver todo el dibujo.
- **Una sola tabla de namespaces** para memoria y persistencia. Se descartó porque abriría la persistencia a `rack` (M-02).
- **Un índice global de nombres.** Se descartó porque reinterpretaría `{Frentes}` o daría `AmbiguousName` y `OperatorInName` nuevos en
  `projectVariable` (INV-19, INV-20).
- **Un segundo motor de evaluación para las calculadas.** Se descartó: `RackComputedExpressionContext` evalúa con el evaluador del
  núcleo.
- **Contar racks por vista o por definición, o reutilizar RACKLISTA o el BOM como fuente.** Se descartó porque contaría N vistas como
  N racks, y por la fuente única de D-07.
- **Persistir el resumen o sus valores.** Se descartó por M-02: el resumen es un cálculo puro en memoria.

## Consecuencias

- **Positivas:**
  - una autoridad única y probada para las métricas por rack, la población y los agregados;
  - la composición de la puerta de salida está en un solo sitio de Application;
  - las fórmulas del contexto de rack pueden usar `Rack.Frentes` con estados tipados;
  - `projectVariable` y los consumidores existentes no cambian.
- **Costos aceptados y vigilancia:**
  - `Project.*` sigue reservado.
  - No hay todavía adaptador productivo de AutoCAD ni UI que consuma el resumen.
  - El primer consumidor de `RackComputedExpressionContext` hereda la obligación O-G3-4: la procedencia del registro y la variable de
    proyecto fallida.
  - Riesgo R-14: en `Full`, los Selectivos excluidos o no colocados también reciben sus métricas D-28.
  - El coste por rack de `RegistryEvaluation` se mide; no se cambia.
  - Las opcionales O-G3-1..5 quedan diferidas.

## Relación con otros ADR

- **[ADR-0043](0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md):** sucesor parcial; ver «Se modifica»
  y «Se sustituye». ADR-0043 sigue aceptado en todo lo demás.
- **[ADR-0009](0009-identidad-guid-embebida-en-dwg.md):** consumido sin cambios. La identidad del rack lógico es el `Id` embebido.
- **[ADR-0046](0046-protocolo-de-ejecucion-delegada-de-agentes.md):** protocolo con el que se ejecutaron los gates. No lo modifica;
  la deuda de protocolo observada (DEBT-I63-PROTOCOL-01) se registra en `docs/ideas-futuras.md`.

## Referencias

- [Proposal V3](../initiatives/I-63-proposal-v3.md) (Freeze `f61d0aca`), §3, §5-§21 y anexo A.
- Enmiendas A-1..A-4 (ver «Base exacta»).
- [Registro de decisiones de I-63](../automation/decisions/I-63.md) y [evidencia](../automation/evidence/I-63-evidence.md).
- Revisiones del Arquitecto: R1-R3 (Proposal V1-V3) y la revisión de arquitectura de G3 (CONFORMING sobre `eb58a476`).
