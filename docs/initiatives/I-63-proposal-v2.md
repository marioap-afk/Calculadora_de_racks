# I-63 — Proposal V2 — ID20, Computed Parameters & Project Summary Foundation

```text
Frozen: NO
Versión: V2 (2026-10-01). Sustituye a V1 como propuesta vigente; V1 queda histórica e inmutable
         (772ac242430084918d319a7683ce615daec39452, blob 48a68307be3f046c20c2405252dc8af3ad050342)
Autor: sesión principal responsable de I-63, directamente; sin Controller, Worker ni subagentes
Base: I-63 772ac242430084918d319a7683ce615daec39452; origin/main 819955d61a6da4c811a11fbd11b5dca13f634b7c
Orden: «I-63 — COORDINATOR ORDER: PROPOSAL V2 + ARCHITECT RE-REVIEW» (decisiones §2, «Orden PV2»)
Revisión previa: Architect R1, SEPARATE SESSION, CHANGES REQUIRED (A63-PV1-01..10); paquete v2
Arquetipo: NEW ARCHITECTURE
IMPLEMENTATION AUTHORIZATION = NO
```

Propuesta autocontenida para la re-revisión del **mismo** Architect. **No** es Freeze. Corrige A63-PV1-01..10 con las resoluciones
vinculantes de la orden PV2 e incorpora O-01..O-06; O-07 es requisito (todo INV de guarda tiene un RED observable). El delta y la
disposición por hallazgo están en el [paquete v2](I-63-architect-package-v2.md).

Convenciones:
- clases de afirmación de [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4;
- **D-nn** = decisión; **INV-nn** = invariante congelable con su obligación de prueba;
- código citado como `ruta:línea` en la base.

## 1. Objetivo, alcance V1 y no-objetivos

**Objetivo.** Una capa de **métricas calculadas de solo lectura** por rack y por proyecto, con una fuente única por métrica, de la
que un subconjunto se integra en el motor de I-49 como símbolos *built-in* (sin un segundo motor), y un **`ProjectSummary` neutral**
que ID30, reportes, la evolución de RACKLISTA, el BOM calculado o los diagnósticos puedan consumir sin recalcular.

**Alcance V1.** Todo en **Application, pura**, con contratos de entrada que construyen las pruebas. **No** hay adaptador nuevo del
Plugin (A63-PV1-09). La única edición del Plugin es la delegación sin cambio de comportamiento de la puerta de salida de Push Back
(D-27).

1. Catálogo cerrado de métricas con identidad estable; un subconjunto, además, como símbolos `rack` (D-02..D-06).
2. Métricas por rack del Selectivo: `Rack.Frentes` y `Rack.FrentesVacios`. `SystemKind` es atributo (D-05, D-07).
3. **Operación por rack** para ID23 (D-17) y **población cotizable** con deduplicación y agregados de proyecto (D-10..D-12).
4. Estados explícitos: vocabulario de **soporte de diseño** separado del **estado en ejecución** (D-09).
5. Integración en I-49: solo el namespace `rack`, búsqueda por namespace, formatter, contexto de rack calculado fuera del núcleo y
   persistencia cerrada con dos tablas (D-13..D-19).
6. `ProjectSummary` neutral con los seis sistemas siempre presentes, *provenance* estable (D-20, D-21) y caracterización de rendimiento
   (D-24).

**No-objetivos:** los del mandato («OUT OF SCOPE») más:
- el namespace `project` en el núcleo;
- un adaptador de captura del Plugin;
- el contrato común o el *snapshot* con I-64;
- un recorrido de instancias alcanzables;
- métricas de copias;
- migrar RACKLISTA o RACKBOMTOTAL;
- cachés;
- cualquier comando, ventana o paleta.

## 2. Entradas cerradas

| Fuente | Lo que aplica |
|---|---|
| Discovery R2 aceptado (`23eeefc5`, CD-I63-F0-R2-04) y §§24-26 (`a0654ae2`) | Hechos DC-01..09, EXP y matriz; decisiones del Owner |
| **P-01** (Owner) | `TotalRacks` = población **cotizable**. Cada condición se clasifica como pertenencia, cobertura no acreditada (`Unavailable`) u otra métrica |
| **P-02a / P-02b** (Owner) | `Kind` ausente o desconocido → excluido; sin clasificación en `RacksBySystem` |
| **P-03** (Owner) | Cobertura no acreditada → `Unavailable`; nunca un parcial como total exacto |
| **P-04** (Owner) | Colocación = al menos una referencia directa activa; la limitación de los anidados se conserva |
| **P-05** (Owner + orden PV1) | `Rack.Frentes` = todos los frentes estructurales del fondo 0. Métrica aparte de vacíos: fondo 0, `Levels.Count == 0` y `FloorPalletCount <= 0` |
| **P-14** (Owner) | Mecanismos independientes; sin contrato común; convergencia diferida. I-64 V3 (`1f1530be`) registra `MASTER-I63-I64-02`, alineado (§16) |
| **Orden PV2** (Coordinator) | Resoluciones vinculantes de A63-PV1-01..10; O-01..O-06 si no amplían el alcance; O-07 como requisito; materialidad |
| I-49 | V6 + A1 + A2 + A3-R2 + [ADR-0043](../adr/0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md) + Freeze `I-49-consensus-freeze-v6-a1-a2-a3-r1.md` |
| ADR-0039 §13 e I-54 D-16 | Custom Properties: texto; la relación con ID20 se decide aquí (D-23) |

## 3. Arquetipo y materialidad

**NEW ARCHITECTURE** (confirmado por el Architect R1 y por la orden PV2).

| ID | Estado | Evidencia y condición |
|---|---|---|
| M-01 | **Activado** | **Crea** la autoridad de métricas y de población cotizable (D-07, D-10). Además **modifica**: la composición de la puerta de salida pasa del handler del Plugin a una función única de Application que consumen el handler e I-63 (D-27) |
| M-02 | **No activado** | Nada nuevo se persiste. La tabla de namespaces **persistibles** (`projectVariable`) está separada de la de namespaces en memoria (O-05), y INV-24 lo prueba en lectura y en escritura |
| M-03 | **No activado** | Fuera del contexto de rack calculado, la semántica es la de hoy: las referencias sin namespace solo buscan en `projectVariable`, `Rack.X` da `UnknownNamespace`, y el formatter y `OperatorInName` no cambian (A63-PV1-01/02; INV-20..23). La delegación de la puerta conserva las razones del handler (INV-13) |
| M-04 | **Activado** | Semántica de fallo nueva de I-63: `Unavailable` por cobertura y `Undetermined` de la puerta. La política *fail-open* del handler **no cambia** (D-27) |
| M-05 | **Activado** | Contratos del núcleo: namespace `rack`, validez de clave por namespace, caso `Computed`, búsqueda por nombre y homónimos por namespace, formatter (D-16) |
| M-06 | **Activado (creador)** | Catálogo cerrado y *providers* por kind (D-08) |
| M-07 | **Activado** | Registro cerrado de *providers* y orquestador de fases; no es un *framework* (D-08) |
| M-08 | **Activado** | ADR-0043 D4, D5, D6, D9, D24 y D25; V6 P25.2, P25.3 y P25.5 (§23 y anexo A, cláusula por cláusula) |

## 4. Conceptos separados — D-01

| Concepto | Quién lo escribe | Persistencia | Símbolo de I-49 | Autoridad |
|---|---|---|---|---|
| ProjectVariable | El usuario | Registro del dibujo | Sí: `projectVariable`, ámbito `Project` | I-48, I-49 |
| **ComputedMetric** | RackCad (derivada) | **Ninguna** | Solo si además es ComputedParameter | Catálogo y *providers* de I-63 |
| **ComputedParameter** | RackCad | Ninguna | **Sí**: `rack`, ámbito `Rack` (D-02) | Ídem |
| CustomProperty | El usuario (metadatos) | Sobre o registro (ADR-0039) | **No** en V1 | ADR-0039 |

- **ComputedMetric** es toda métrica del catálogo, por rack o de proyecto.
- **ComputedParameter** es una ComputedMetric de ámbito rack, escalar y *bindable*. En V1 son exactamente `frentes` y `frentesVacios`.
- Ninguna se convierte en ProjectVariable ni entra en el registro.
- Ningún nombre de variable se reinterpreta: `{Rack}` sigue siendo una variable; `Rack` y `Project` sin llaves siguen siendo
  `ReservedName` (`ExpressionBinder.cs:322`).

## 5. Identidad, namespaces, ámbito y nombres — D-02, D-03, D-04 (P-10; A63-PV1-01, A63-PV1-04)

**D-02 — Dos identidades** (A63-PV1-04, opción a):

| Identidad | Forma | Para qué |
|---|---|---|
| **`MetricId`** | `(MetricScope: Rack \| Project, token)` | Toda métrica del catálogo, en `RackMetricResults`, `ProjectSummary` y *provenance* |
| **`SymbolId`** | `(rack, token)` | Solo las ComputedParameter, dentro del motor de I-49 |

- El catálogo declara la correspondencia 1:1 `MetricId(Rack, t)` ↔ `SymbolId(rack, t)` solo para las ComputedParameter.
- `RacksBySystem`, los mapas y las métricas de proyecto **nunca** son `SymbolId`.
- *Tokens*: ASCII, empiezan por minúscula, solo letras y dígitos, comparador `Ordinal`, declarados explícitamente y probados en los dos
  sentidos. **Nunca** se derivan del nombre visible, de `enum.ToString()` ni de `nameof` (ADR-0043 D9).
- Catálogo V1:

  | `MetricId` | `SymbolId` |
  |---|---|
  | `(Rack, frentes)` | `(rack, frentes)` |
  | `(Rack, frentesVacios)` | `(rack, frentesVacios)` |
  | `(Project, totalRacks)` | — |
  | `(Project, rackCount)` (por sistema) | — |
  | `(Project, totalFrentes)` (por sistema) | — |
  | `(Project, totalFrentesVacios)` (por sistema) | — |

**Namespaces del núcleo en V1:**
- `projectVariable` (persistible, sin cambios) y **`rack`** (en memoria, no persistible).
- **`project` no entra en el núcleo:** sin miembro de enum, sin *token* ni regla de clave. `Project` sin llaves sigue siendo
  `ReservedName`, y `Project.X` sigue dando `UnknownNamespace` en todos los contextos. La reserva se conserva para una iniciativa futura,
  sin prometer persistencia ni *binding*.

**Validez de clave por namespace:** `projectVariable` conserva la regla A2 sin cambiar un carácter (`SymbolId.cs:85-108`); `rack` usa la
regla de *token* anterior. El núcleo **no** conoce el catálogo, solo la regla.

**D-03 — Nombres.**
- El **nombre de miembro** del catálogo es `Frentes` y `FrentesVacios`; el definitivo de la segunda se fija en el Freeze.
- La **referencia textual completa** es `Rack.Frentes`.
- El nombre de miembro solo sirve para mostrar y para resolver al escribir. Debe ser una palabra simple y es único dentro del
  namespace, con `OrdinalIgnoreCase`. Cambiarlo no cambia ni la clave ni el árbol enlazado (INV-26).

**D-04 — Ámbito.** Las entradas `rack` son de `SymbolScope.Rack`, que pasa a ser productivo. Un consumidor `Project` con una entrada
`rack` en su tabla da `ScopeViolation` (regla vigente, `ExpressionBinder.cs:376-379`; INV-25). La disponibilidad por consumidor la fija
D-14, no el ámbito.

**Alternativa rechazada** (con la que coincide el Architect R1): claves GUID preasignadas. Serían opacas para diagnósticos, *provenance*
y la futura persistencia de ID23, y no evitan la validez por namespace (`SymbolId.cs:89-92`).

## 6. Catálogo V1 y matriz por sistema — D-05, D-06

**D-05 — Catálogo** (cerrado; ampliarlo exige una A-n o una iniciativa):

| `MetricId` | Símbolo | Tipo | Fuente (D-07) |
|---|---|---|---|
| `(Rack, frentes)` | `Rack.Frentes` | conteo (entero como `double` finito) | Selectivo resuelto, fondo 0 |
| `(Rack, frentesVacios)` | `Rack.FrentesVacios` | conteo | Ídem, con el predicado de P-05 |
| `(Project, totalRacks)` | — | conteo | Población cotizable (D-10) |
| `(Project, rackCount)` por sistema | — | conteo | Ídem, particionada por kind |
| `(Project, totalFrentes)` por sistema | — | conteo | Σ `(Rack, frentes)` |
| `(Project, totalFrentesVacios)` por sistema | — | conteo | Σ `(Rack, frentesVacios)` |

- `SystemKind` es un atributo de texto del rack (`KindToken`), no una métrica numérica.
- **Diferidas** (requieren una decisión de producto): niveles, posiciones, altura, fondo, los frentes de los demás sistemas y los
  totales **entre** sistemas.

**D-06 — Soporte de diseño** (O-01: estado del **catálogo** por kind, independiente de los datos; correspondencia con el mandato:
«Supported» → `Supported`, «NeedsContract» → `NotSupported`, «NotApplicable» → `NotApplicable`):

| Métrica | selective | dynamic | pushback | cantilever | cabecera | cama |
|---|---|---|---|---|---|---|
| `(Rack, frentes)` / `(Rack, frentesVacios)` | Supported | NotSupported | NotSupported | NotSupported | NotApplicable | NotApplicable |
| `(Project, totalFrentes)` / `(Project, totalFrentesVacios)` | Supported | NotSupported | NotSupported | NotSupported | NotApplicable | NotApplicable |
| `(Project, rackCount)` | Supported | Supported | Supported | Supported | Supported | Supported |

En ejecución, una celda `Supported` produce `Available` o `Unavailable(razón)`; una `NotSupported` o `NotApplicable` produce ese mismo
estado, sin leer datos (D-09).

## 7. Fuente única por métrica — D-07

| Métrica | Fuente única | No es fuente |
|---|---|---|
| `(Rack, frentes)` | `SelectiveDepthLayout.BaysOfFondo(system, 0).Count` sobre el `SelectiveRackSystem` resuelto (`SelectiveDepthLayout.cs:23`) | `Bays.Count` leído por otra vía; el BOM; los *builders* de vista |
| `(Rack, frentesVacios)` | Las mismas bahías: número con `Levels.Count == 0 && FloorPalletCount <= 0` | La regla de Planta `Levels.Count == 0` (`SelectivePlantaBuilder.cs:371-373`), propia de esa representación |
| `(Project, totalRacks)`, `(Project, rackCount)` | Evaluación de población de I-63 (D-10) | `RackListBuilder`, `ConsolidatedBom.RackCount`, etiquetas |
| `(Project, totalFrentes)`, `(Project, totalFrentesVacios)` | Suma de los valores por rack de los incluidos | Recontar bahías en el agregado |
| `SystemKind` | *Token* `Kind` coherente de las hermanas | `KindLabel`, `BomLabel`, `LibraryLabel` |

**Equivalencia con el diseño** `[RECONSTRUCTED]` (comprobada también por el Architect R1). El resolver crea una bahía resuelta por
bahía de diseño y fondo (`SelectiveGeometryResolver.cs:248-328`):
- una bahía de diseño sin celdas queda vacía (`:268-273`);
- con larguero a piso, la celda 0 da un nivel (`:281-293`);
- con una sola celda y sin larguero a piso hay tarima de piso (`:294-300`);
- con más celdas y sin larguero a piso hay tarima de piso y niveles (`:307`).

La fuente congelada es la **resuelta**, como define la orden. La equivalencia queda como oráculo (INV-04).

## 8. *Providers* y operación por rack — D-08, D-17 (A63-PV1-08)

**D-08 — *Provider*.**
- Es una función **pura** por kind: `Compute(RackMetricInput) → RackMetricResults`.
- La entrada lleva solo lo que el orquestador ya resolvió para ese rack.
- La salida tiene un `MetricValue` por **cada** métrica `(Rack, *)` del catálogo.
- Cada *provider* declara la fase mínima de cada métrica `Supported`. Las `NotSupported` y `NotApplicable` se emiten sin fase ni coste.
- **Registro cerrado:** una tabla explícita por los seis *tokens* de `RackEmbedDocument.Kind*`, construida con `KindDispatch`. Sin
  reflexión, configuración ni descubrimiento.
- **Prohibido en un *provider***: leer AutoCAD, catálogos o el registro; llamar a resolvers, a `RegistryEvaluation` o al binder;
  invocar a otro *provider*; tener estado o caché. INV-09 lo prueba con un control positivo.
- Los nombres de tipos son ilustrativos y no se congelan; la forma y las reglas sí.

**D-17 — Operación por rack, congelada.**

```text
RackMetricRequest(hermanas de UN RackId, lectura del registro, catálogo)
  → RackMetricResults
  → RackComputedExpressionContext
```

1. **No** enumera el proyecto, **no** construye `ProjectSummary` y **no** evalúa la población global.
2. Aplica a las hermanas recibidas el mismo canal de §10:
   - la proyección y el orden canónico;
   - E3: coherencia de kind;
   - E5: diseño legible del kind;
   - E4: autoridad *authored*.

   Después resuelve **como máximo ese rack**, con una sola Φ2+Φ3, y solo si alguna métrica `Supported` de su kind lo necesita.
3. Si el kind no es coherente, está ausente o es desconocido, cada métrica del rack es `Unavailable(KindIncoherent | KindAbsent |
   KindUnknown)`. Es un resultado por rack; la exclusión de pertenencia solo existe en la población.
4. `RackComputedExpressionContext` vive **fuera** de `RackCad.Application.Expressions` (O-03; guarda de P26.1).
   - Su tabla tiene las entradas `projectVariable` del **mismo documento de registro** que usó la Φ2 de ese rack, más las entradas
     `rack` del catálogo (`Computed`).
   - Se enlaza con ámbito `Rack`.
5. **Propagación por estado** cuando una expresión lee `Rack.X`:
   - el enlace **no** depende del kind: `Rack.Frentes` enlaza en cualquier rack;
   - antes de evaluar, el contexto revisa el estado de **cada** referencia `rack` del árbol (`BoundExpressionDependencies`, en orden de
     `SymbolId`);
   - si todas están `Available`, evalúa con `ExpressionEvaluator.Evaluate` y esos valores;
   - si alguna no lo está, devuelve sin evaluar `ComputedReferencesNotAvailable`, con la lista ordenada de `(SymbolId, estado, razón?)`
     de **todas** las que no están `Available`;
   - `NotApplicable`, `NotSupported` y `Unavailable(razón)` se conservan como estados distintos, sin colapsarse ni precedencia;
   - `BrokenReference` solo significa «`SymbolId` ausente de la tabla», como hoy;
   - nunca hay valor parcial.
6. Ejemplos congelados:
   - Push Back: `Rack.Frentes * 2` → `ComputedReferencesNotAvailable[(rack/frentes, NotSupported)]`;
   - Cabecera: → `[(rack/frentes, NotApplicable)]`;
   - Selectivo con variable rota: → `[(rack/frentes, Unavailable(EffectiveFailed(BrokenProjectVariableReference)))]`;
   - Selectivo legible con fondo 0 de 4 frentes: → `8`.

## 9. Estados y diagnósticos — D-09

**Estado en ejecución de un `MetricValue`** (cerrado; nunca se infiere de un `null` o de una ausencia):

| Estado | Valor |
|---|---|
| `Available` | `double` finito |
| `NotApplicable` | — |
| `NotSupported` | — |
| `Unavailable(razón)` | — |

**Razones de `Unavailable`** (cerradas; son datos, no texto):
- de rack: `EnvelopeUnreadable`, `DesignUnreadable`, `SiblingsDivergent`, `KindIncoherent`, `KindAbsent`, `KindUnknown`,
  `RegistryUnreadable`, `EffectiveFailed(outcome)`, `CatalogUnavailable`, `ResolveFailed`;
- de agregado: `CoverageNotAccredited`, `MemberMetricUnavailable(RackIds)`.

**Pertenencia de un rack:** `Included`; `Excluded(NotPlaced | KindAbsent | KindUnknown | SiblingsDivergent | OutputDenied)`; o
`Undetermined(razón)`.

**Diagnósticos del resumen:**
- cada exclusión salvo `NotPlaced`, cada `Undetermined` y cada definición colocada sin identidad atribuible tienen un diagnóstico con
  código, clave de definición, RackId canónico (si existe) y razón;
- una exclusión por regla **no** es parcialidad.

**Regla de agregación:** un agregado es `Available` solo con la cobertura acreditada y todos sus miembros `Available` en esa métrica; si
no, `Unavailable`, nunca un parcial (INV-08).

## 10. Población cotizable, deduplicación y agregación — D-10, D-11, D-12, D-27 (A63-PV1-05/06/07)

**D-10a — Proyección de definición** (A63-PV1-06). Una función pura clasifica cada definición capturada:

```text
(DefinitionKey, envelopeJson, DirectReferenceCount) →
    EnvelopeUnreadable(probeId?)     sobre que no deserializa (RackEmbedStore.Deserialize = null)
  | IdAbsent                         sobre legible con Id vacío o en blanco
  | KindAbsent(Id)                   sobre legible con Id y Kind vacío o en blanco
  | KindUnknown(Id, token)           Kind no vacío fuera de los seis tokens
  | Known(Id, kindToken, envelope)   uno de los seis tokens
```

- **Comparador único de kind:** `Ordinal` contra los seis *tokens*, el mismo del despacho de handlers (`KindHandlerRegistry.TryResolveAll` →
  `KindDispatch`).
- **No** se usa `ProjectVariableScanProjection` para clasificar: convierte `KindAbsent` en sobre ilegible y compara `selective` con
  `OrdinalIgnoreCase` (`ProjectVariableScanProjection.cs:37-47`).
- `probeId` (`RackEnvelopeIdProbe`) solo es diagnóstico: nunca identifica ni fusiona.

**D-11 — Agrupación, orden canónico y determinismo** (A63-PV1-07):
1. Igualdad de RackId: `OrdinalIgnoreCase` (Rack Identity; `BomTotal.cs:73-75`).
2. **Grafía canónica** del grupo: la mínima en `Ordinal` de las grafías observadas.
3. **Hermanas:** todas las definiciones del RackId, colocadas o no, como hace RACKBOMTOTAL (`BomTotal.cs:96-104`).
4. **Orden canónico** por `DefinitionKey` con `Ordinal`, **antes** de E3, E4, E5 y E6.
5. El **representante** es el que devuelve E4 sobre la lista en orden canónico (la primera).
6. **`DisplayName`** = primer `Name` no vacío en orden canónico, con `Trim()`, solo para presentación.
7. *Provenance* y resumen usan solo identidades canónicas (D-21).

**D-10 — Condiciones de «cotizable», en este orden** (la primera que decide gana):

| # | Condición | Autoridad | Si no se cumple |
|---|---|---|---|
| E1 | Identidad atribuible | D-10a | Una definición **colocada** `EnvelopeUnreadable` o `IdAbsent` → **cobertura no acreditada** (afecta a `TotalRacks`, no a un rack). Sin colocar → se ignora |
| E2 | Colocado: alguna hermana con `DirectReferenceCount > 0` (P-04) | Recuento de la captura | **Exclusión** `NotPlaced`, sin diagnóstico |
| E3 | Kind coherente: todas las hermanas comparten clasificación | Regla **propia de I-63** (O-02: RACKBOMTOTAL toma el kind de `siblings[0]` sin comprobarlo) | Todas `KindAbsent` → **exclusión** `KindAbsent`. Todas `KindUnknown` con el mismo *token* → **exclusión** `KindUnknown` (P-02a). Cualquier mezcla → **`Undetermined(KindIncoherent)`** |
| E5 | Diseño del kind legible, para **cada** hermana | **Lector puro por kind** (D-26) | `Undetermined(DesignUnreadable)`. Nunca se excluye ni se cuenta en silencio (orden PV2) |
| E4 | Autoridad *authored* | `BomAuthoredAuthority.Resolve` con entrada tipada (abajo) | `DivergentSiblings` → **exclusión** `SiblingsDivergent`; `UnreadableSibling` → `Undetermined(DesignUnreadable)` |
| E6 | Veredicto de salida | **Función única** de D-27 | `Deny` → **exclusión** `OutputDenied`; `Undetermined` → **`Undetermined(razón)`** |
| — | Registro, variables, efectivo, *resolve* para métricas | Resolver efectivo y geométrico | **Otra métrica** (`Unavailable` en frentes). **No** toca la pertenencia (P-03) |

**Entrada tipada de E4.** La lista, en orden canónico, de `ProjectVariableScanEntry` que produce `ProjectVariableScanProjection.Project`
**solo** para hermanas ya clasificadas `Known` con un kind coherente (E3) y diseño legible (E5). Así la proyección vigente nunca ve un
caso `KindAbsent`, y su comparador de `selective` coincide con el `Ordinal` de E3 (solo llega el *token* exacto).

**No circularidad:** E1..E6 son anteriores al BOM y a cualquier métrica. «Cotizable» **no** significa «salió en un listado».

**D-26 — Lector de diseño por kind** (E5). Una función pura de Application, que solo reutiliza los *stores* vigentes:

| Kind | Lectura legible |
|---|---|
| selective | `SelectivePalletDesignStore.Deserialize` sin excepción y no nulo |
| dynamic | `RackProjectStore.Deserialize` con `DynamicDesign` o `DynamicSystem` (*legacy*) no nulo |
| pushback | Ídem, con `PushBackDesign` no nulo |
| cantilever | Ídem, con `CantileverLineDesign` no nulo |
| cabecera | Ídem, con `Header` no nulo |
| cama | `FlowBedConfigurationStore.Deserialize` no nulo |

- Cualquier excepción del *store* da «ilegible».
- La tabla reproduce las comprobaciones de presencia de los `BuildBom` de los handlers (`DynamicKindHandler.cs:39-43`,
  `PushBackKindHandler.cs:42-43`, `CantileverKindHandler.cs:49-51`, `CabeceraKindHandler.cs:37-38`, `CamaKindHandler.cs:39-40`,
  `SelectiveKindHandler.cs:43-54`).
- V1 **no** cambia los handlers para que la consuman. Queda como riesgo declarado (R-11) y pregunta para el Architect (AQ-03).

**D-27 — Veredicto de salida único** (A63-PV1-05). Una sola función pura de Application:

```text
RackOutputVerdict(kindToken, designJson, catalog) → Allow | Deny(motivo) | Undetermined(razón)
```

- `pushback`:
  - `RackProjectStore.Deserialize`; si no hay `PushBackDesign` → `Undetermined(DesignUnreadable)`;
  - catálogo nulo → `Undetermined(CatalogUnavailable)`;
  - `PushBackResolver(catalog).Resolve` → `RackBomOutputGate.For(system)` (`RackBomOutputGate.cs:49`): con motivo → `Deny(motivo)`, sin
    motivo → `Allow`;
  - cualquier excepción → `Undetermined(ResolveFailed)`.
- Los demás kinds → `Allow`, sin leer nada (hoy sus handlers devuelven `null`).
- **Política del handler (comportamiento observable vigente, conservado):** `PushBackKindHandler.OutputBlockedReason` pasa a delegar en
  esta función y aplica `Deny(m) → m`, `Allow → null` y `Undetermined → null` (*fail-open*, como hoy; `PushBackKindHandler.cs:56-71`). Esa
  correspondencia es una función pura de Application, probada (INV-13).
- **Política de I-63:** `Undetermined → Undetermined(razón)`, es decir, cobertura no acreditada.
- Es la **única** definición de la puerta para I-63. La composición del Plugin no se duplica.

**D-12 — Agregados.**
- `(Project, totalRacks)` = `Available(n)` (n = RackIds `Included`) **si y solo si** la cobertura está acreditada: ninguna definición
  colocada sin identidad atribuible (E1) y ningún rack `Undetermined`. Si no, `Unavailable(CoverageNotAccredited)` con sus diagnósticos.
- `BySystem` (A63-PV1-10): **siempre** los seis *tokens*, en orden fijo `selective`, `dynamic`, `pushback`, `cantilever`, `cabecera`,
  `cama`. Por cada uno:
  - `(Project, rackCount)` = `Available(n ≥ 0)` si `totalRacks` es `Available`, con 0 si no hay racks; si no, `Unavailable(CoverageNotAccredited)`;
  - `(Project, totalFrentes)` y `(Project, totalFrentesVacios)`:
    - `selective`: `Available(Σ)` o `Unavailable` según la regla de agregación;
    - `dynamic`, `pushback` y `cantilever`: `NotSupported`;
    - `cabecera` y `cama`: `NotApplicable`;
    - siempre presentes.
  - Σ `rackCount` = `totalRacks` cuando `totalRacks` es `Available` (INV-07).
- `RacksBySystem` **es** la columna `rackCount` de `BySystem`. No es un símbolo.

**Relación con los conteos existentes (P-08):** son magnitudes distintas, sin migrar ninguna.

| Caso | RACKBOMTOTAL hoy | `totalRacks` |
|---|---|---|
| Sobre colocado ilegible o sin `Id` | Aborta | `Unavailable` |
| `Kind` ausente o desconocido, colocado | Aborta | Excluido |
| Kinds mezclados entre hermanas | Usa el de la primera | `Unavailable` |
| Diseño ilegible, cualquier kind | Lo salta con aviso | `Unavailable` |
| Selectivo divergente (también con una hermana sin colocar) | Lo salta con aviso | Excluido |
| Push Back bloqueado | Aborta | Excluido |
| Push Back con puerta indeterminada | Lo cotiza (*fail-open*) | `Unavailable` |
| Variable rota | Aborta | Incluido; frentes `Unavailable` |

## 11. Fases y disponibilidad de símbolos — D-13, D-14

**D-13 — Fases.**

| Fase | Contenido | Autoridades |
|---|---|---|
| Φ0 | Captura: definiciones, sobres, recuentos | Contrato de entrada (D-25) |
| Φ1 | *Authored*: proyección D-10a, lector D-26, autoridad E4 | *Stores*, `BomAuthoredAuthority` |
| Φ2 | Efectivo | `SelectiveEffectiveDesignResolver.ResolveAccredited` |
| Φ3 | Resuelto (y veredicto E6 de Push Back) | `SelectiveGeometryResolver`, `PushBackResolver` |
| Φ4 | Población y agregados | I-63 |

Fases mínimas:
- `(Rack, frentes)` y `(Rack, frentesVacios)`: Φ3;
- pertenencia: Φ1, más Φ3 solo para la puerta de Push Back;
- `(Project, totalRacks)`: Φ4.

**D-14 — Disponibilidad** (fail-closed):

| Consumidor | Ámbito | `Rack.*` | `Project.*` | Si se escribe |
|---|---|---|---|---|
| Definición de variable (RACKVARIABLES) | Project | No | No | `UnknownNamespace`, como hoy |
| Fórmula de propiedad vinculada (RACKEDITAR) | Rack | No | No | `UnknownNamespace`, como hoy |
| `RackComputedExpressionContext` (D-17) | Rack | **Sí**, del rack de la petición | No | `Rack.X` enlaza; `Project.X` da `UnknownNamespace` |

Habilitar un símbolo en otro consumidor exige demostrar que su fase es anterior e independiente, con prueba y una A-n aprobada por
Architect y Coordinator.

## 12. Ciclos y reentrada — D-15

- **R1.** Ningún símbolo `rack` está disponible en Φ2 ni antes (D-14). Rompe los ciclos `Rack.Altura → altura → fórmula de altura` y
  `VerticalClearance → altura resuelta → fórmula de VerticalClearance`.
- **R2.** `RackComputedExpressionContext` solo se construye con resultados ya terminados de la petición. Evaluar nunca dispara una
  resolución.
- **R3.** Los *providers* son puros (D-08).
- **R4.** Las entradas `Computed` son hojas: no tienen expresión ni aristas, y `RegistryEvaluation` y `DependencyGraph` las rechazan
  como error de programación (INV-27).
- **R5.** Ningún consumidor V1 escribe en un rack a partir de un valor calculado.
- **R6.** No hay símbolos `project` (D-02). Un agregado de proyecto leído desde un rack exigiría resolver todo el dibujo.

## 13. Integración con el Expression Engine — D-16, D-18, D-19 (A63-PV1-01/02)

**D-16 — Cambios en `RackCad.Application.Expressions`** (neutrales: ningún tipo del núcleo nombra racks, sistemas ni métricas):

1. **Namespaces en memoria:** `SymbolNamespace` gana `Rack`; la tabla de namespaces en memoria tiene `projectVariable` y `rack`. Es
   distinta de la tabla de *tokens* persistidos de D9, que no cambia (D-18, O-05).
2. **Validez de clave por namespace** (D-02). La regla y la API de `projectVariable` no cambian.
3. **`SymbolDefinitionKind.Computed`:** hoja sin valor en la tabla; el caso reservado por P25.2.
4. **Índices por namespace en `SymbolTable`:**
   - la búsqueda por nombre visible y los patrones de `OperatorInName` se calculan **por namespace**;
   - hoy el índice y `OperatorNames` ignoran el namespace (`SymbolTable.cs:140-148`, `:184`).
5. **Binder** (`ExpressionBinder.cs:220-222`, `:317-362`):
   - **referencias sin namespace** —simples, con llaves o cualificadas con `#`—: buscan **solo** en `projectVariable`. `OperatorInName`
     solo mira nombres de `projectVariable`;
   - **`NamespaceReferenceSyntax`** con la palabra `Rack` (`OrdinalIgnoreCase`):
     - si la tabla del contexto **no tiene** entradas `rack` → `UnknownNamespace` (como hoy);
     - si las tiene y falta el miembro (`Rack.`, `ExpressionSyntaxParser.cs:226-228`) → **`NameRequired`** sobre el lapso de la
       referencia;
     - si el miembro existe → búsqueda exacta `OrdinalIgnoreCase` **solo** entre las entradas `rack` → `UnknownSymbol` si no hay ninguna.
       No puede haber homónimos, porque el catálogo los prohíbe;
     - después, la regla de ámbito vigente;
   - **cualquier otra palabra**, incluida `Project` → `UnknownNamespace` (como hoy).
6. **Formatter** (`ExpressionFormatter.cs:268-279`):
   - `projectVariable`: sin cambios, salvo que los homónimos se cuentan solo entre `projectVariable`;
   - `rack` presente en la tabla → **siempre** `Rack.<miembro>`, sin `Q(key)` ni llaves;
   - `rack` **ausente** → forma diagnóstica `Rack.#{<token>}`. Se muestra y **nunca** enlaza: el parser produce
     `NamespaceReferenceSyntax` sin miembro seguida de un cualificador suelto (`ExpressionSyntaxParser.cs:224-228`), y eso es un error de
     sintaxis `[RECONSTRUCTED]`. El código exacto lo fija INV-22.
7. **Clasificación canónica:** `CanonicalShape.Classify` de un árbol con una referencia `rack` = `Expression`, nunca `DirectReference`, que
   sigue reservado a `projectVariable` (`ExpressionFormatter.cs:64`; el código ya cae en `default`). Congelado por INV-23.

No cambian el lexer, el parser, el evaluador, los límites, `DependencyGraph` ni `RegistryEvaluation`, más allá de la guarda R4.

**D-18 — Persistencia cerrada con dos tablas** (O-05):
- `PersistedBoundExpressionJson` (`:95`, `:170`) deja de usar la tabla de namespaces en memoria y pasa a usar **solo** la de *tokens*
  persistidos de D9: `projectVariable`.
- **Lectura:** `"Namespace":"rack"` sigue siendo un fallo estructural, y el registro queda `PresentButUnreadable`, como fija hoy
  `G8PersistenceIntegrationContractTests`.
- **Escritura:** un árbol con un namespace no persistible lanza un error de programación; nunca se escribe.
- Persistir referencias calculadas queda para ID23, con su ADR.

**D-19 — Compatibilidad de textos.**
- Antes de V1 ningún texto con `palabra.miembro` era comprometible, así que ningún documento lo contiene.
- Fuera de `RackComputedExpressionContext`, toda la semántica de nombres, formatter y diagnósticos es la de hoy (M-03 no activado).

## 14. `ProjectSummary` neutral — D-20 (A63-PV1-10, O-04)

```text
ProjectSummary
├── Totals:      (Project, totalRacks) : MetricValue
├── BySystem:    los seis tokens, en orden fijo →
│                { (Project, rackCount), (Project, totalFrentes), (Project, totalFrentesVacios) } : MetricValue, siempre presentes
├── Racks:       RackSummary { RackId canónico, KindToken?, DisplayName?, Membership,
│                              Metrics: (Rack, *) → MetricValue, siempre presentes }
└── Diagnostics: { Code, RackId?, DefinitionKey?, Reason }, en orden determinista
```

- Puro, inmutable, en memoria y **no persistido**. Orden: RackId canónico `Ordinal`; diagnósticos por (código, clave de definición
  `Ordinal`).
- Sin UI, sin textos localizados obligatorios y sin dependencias de WPF ni del Plugin.
- **Nivel de la petición** (O-04, para no resolver de más):
  - `Population` devuelve `ProjectPopulation`, un tipo distinto **sin** campos de métricas por rack: solo pertenencia,
    `(Project, totalRacks)` y `(Project, rackCount)`. Nunca ejecuta Φ2 ni Φ3 de métricas (solo la Φ3 de E6 en Push Back);
  - `Full` devuelve `ProjectSummary` con todo el catálogo.

  Así ningún estado se infiere de la ausencia de un campo.
- Un consumidor (ID30, reportes, RACKLISTA, BOM calculado) **presenta**; no recalcula.

## 15. *Provenance* — D-21 (O-06)

Cada `MetricValue` conserva en memoria, sin persistir (ADR-0043 D24):
- el `MetricId` (y el `SymbolId`, si es ComputedParameter);
- el RackId canónico;
- un **identificador de autoridad fuente** de una tabla cerrada y probada: `selective.resolved.fondo0.bays`,
  `selective.resolved.fondo0.emptyBays`, `population.cotizable` y `aggregate.sum`;
- la fase;
- la `DefinitionKey` del representante;
- los *outcomes* de E4 y E6 y el *outcome* efectivo.

Cada agregado conserva los RackIds canónicos incluidos y los excluidos con su motivo. `RackComputedExpressionContext` conserva el
`BoundExpression` y los `SymbolId` leídos. No hay UI ni motor de dependencias nuevo.

## 16. Frontera con I-64 — D-22 (P-14)

- I-63 tiene su propio mecanismo: proyección, población y agregación sobre contratos de entrada de Application. I-64 tiene el suyo: el
  inventario runtime, la navegación, la selección y la UI.
- Sin contrato común ni *snapshot*, y sin consumir código no integrado.
- **Hecho de coordinación:** la V3 de I-64 (`1f1530be`) registra `MASTER-I63-I64-02`, que retira la fundación compartida y la autoría de
  I-63 y elimina la dependencia de I-64 respecto de I-63. Coincide con P-14. Lo leído es un resumen de I-64, no una decisión recibida
  directamente.
- I-64, como cualquier consumidor, podrá **presentar** `ProjectSummary` cuando esté integrado.

## 17. Compatibilidad — D-23

- **ProjectVariable:** identidad, regla de clave, comparador, ida y vuelta, cualificador y formato persistido sin cambios (INV-19..INV-24).
- **CustomProperty:** sigue siendo texto y no es símbolo en V1. La relación ID24 ↔ ID20 exige su propia decisión.
- **Legacy:** sin RackId no hay rack; no se inventa identidad. `{Rack}` y `{Project}` siguen siendo variables.

## 18. Rendimiento — D-24 (O-04)

**Estrategia:**
- una petición `Population` o `Full` usa **un** contrato de captura, **una** lectura del registro y **un** catálogo;
- por RackId: una proyección y una autoridad;
- una Φ2+Φ3 **solo** para los Selectivos con `Full`, y un veredicto E6 por cada Push Back;
- nunca por vista;
- sin cachés ni resolución repetida dentro de una petición;
- `RackMetricRequest` resuelve como máximo un rack.

**Coste heredado** `[MEASURED]`: `SelectiveEffectiveDesignResolver.ResolveWith` evalúa el registro por rack
(`SelectiveEffectiveDesignResolver.cs:134`). Se mide; no se cambia.

**Caracterización** (pruebas sintéticas en Core; los relojes nunca son oráculo, los contadores inyectados sí):
- N = 1, 10, 100 y 1000 racks, con tres vistas cada uno;
- `Population` frente a `Full`;
- lecturas repetidas;
- una sola petición por rack.

Se registran los tiempos medidos, sin umbrales inventados.

## 19. Capas — D-25 (A63-PV1-09, opción a)

- **Application (pura):**
  - catálogo, proyección, lector y veredicto únicos, *providers*, orquestador, población y agregación;
  - `RackMetricRequest`, `RackComputedExpressionContext` y `ProjectSummary`;
  - cambios neutrales del núcleo.
- **Contrato de entrada (Φ0):** `ProjectCaptureSnapshot` = lista de `(DefinitionKey, envelopeJson, DirectReferenceCount)`, más la lectura
  del registro (`ProjectVariablesReadResult`) y el catálogo (`RackCatalog` o nulo). Lo construyen las **pruebas** en V1.
- **Plugin:** **ningún** componente nuevo. El primer consumidor productivo futuro adaptará la captura de AutoCAD (por ejemplo sobre
  `ScanEnvelopes`) y planificará su OV. La única edición es la delegación de `PushBackKindHandler.OutputBlockedReason` en D-27: una línea
  que llama a una función de Application probada y conserva el comportamiento (INV-13).
- **UI:** nada.

## 20. Invariantes → obligaciones de prueba

En `tests/RackCad.Tests` (Core). Toda **guarda** tiene un RED observable que falla antes del cambio correcto (O-07, requisito):
- o la API todavía no existe;
- o un **control positivo** en la misma prueba, que la variante incorrecta no supera.

Ninguna guarda es tautológica.

| ID | Invariante | Escenario / oráculo | RED observable | Gate |
|---|---|---|---|---|
| INV-01 | N vistas o colocaciones → 1 rack | 3 definiciones con referencias 2/1/0 → `totalRacks` 1 | API inexistente; un conteo por vista da 3 | G2 |
| INV-02 | Igualdad `OrdinalIgnoreCase`, grafía canónica, el tanteo no fusiona | `Rack-A` y `rack-a` → 1, con grafía `Rack-A` (mínima `Ordinal`); un sobre ilegible con tanteo `Rack-A` → `Unavailable`, no un rack más | `Ordinal` da 2; «primera grafía» da otra grafía al reordenar | G2 |
| INV-03 | E1..E6: cada caso da su clasificación | Una prueba por fila de D-10, por caso de D-10a y por mezcla de E3 | API inexistente | G2 |
| INV-04 | `Rack.Frentes` = bahías del fondo 0, vacías incluidas | Fondo 0 con 4 (1 vacío), fondo 1 con 6 → 4; equivalencia con «diseño sin celdas» | API inexistente; «máximo» da 6; «sin vacíos» da 3 | G1 |
| INV-05 | `Rack.FrentesVacios` | Tarima de piso sin larguero → **no** vacío; sin celdas → vacío | La regla de Planta cuenta el primero como vacío | G1 |
| INV-06 | Diseño ilegible por kind → `Undetermined` | Un caso por kind (selective, dynamic, pushback, cantilever, cabecera, cama) con un diseño que su *store* no lee o sin su sección → `Undetermined(DesignUnreadable)` y `totalRacks` `Unavailable` | API inexistente; la vía «E4 aprueba la primera vista» incluiría los no selectivos | G2 |
| INV-07 | `BySystem` | Seis *tokens* siempre, en orden fijo; sistema ausente → `rackCount` `Available(0)`; Push Back `totalFrentes` = `NotSupported`; Cabecera = `NotApplicable`; Σ `rackCount` = `totalRacks` cuando es `Available` | API inexistente; un mapa con solo los presentes no tiene seis claves | G2 |
| INV-08 | Un agregado nunca es parcial | Un incluido con frentes `Unavailable` → `totalFrentes` `Unavailable`; `totalRacks` sigue `Available` | Un agregado que salta miembros da un número | G2 |
| INV-09 | *Providers* puros | Prueba arquitectónica de dependencias **con control positivo**: un tipo de prueba que referencia un resolver dentro del espacio de los *providers* debe ser detectado | Sin la guarda, el control no se detecta | G1 |
| INV-10 | Una variable rota no cambia `totalRacks` | Selectivo incluido con referencia rota → `Included`; frentes `Unavailable(EffectiveFailed(BrokenProjectVariableReference))` | Un `totalRacks` dependiente del efectivo baja o pasa a `Unavailable` | G2 |
| INV-11 | Hermanas y determinismo | Hermana **no colocada y divergente** → `Excluded(SiblingsDivergent)`; con el orden de entrada invertido, mismo representante, `DisplayName`, grafía y `ProjectSummary` | El orden de barrido cambia representante, nombre o grafía | G2 |
| INV-12 | Veredicto E6 único | `RackOutputVerdict` → `Allow` (Push Back sin motivo y los demás kinds), `Deny(m)` (Push Back con motivo de `RackBomOutputGate`) y `Undetermined` (diseño ilegible, catálogo nulo, excepción del resolver) | API inexistente | G2 |
| INV-13 | El handler conserva su política | La correspondencia del handler: `Deny(m) → m`, `Allow → null`, `Undetermined → null`, sobre los mismos casos de INV-12. El handler la llama | API inexistente; una correspondencia que propague `Undetermined` cambiaría RACKBOMTOTAL | G2 |
| INV-14 | Operación por rack | `RackMetricRequest` de un Selectivo → **1** resolución y **0** evaluaciones de población (contadores) | API inexistente; una vía por el resumen evalúa la población | G1 |
| INV-15 | Propagación por estado | Push Back → `ComputedReferencesNotAvailable[(rack/frentes, NotSupported)]`; Cabecera → `NotApplicable`; variable rota → `Unavailable(...)`; legible → `Rack.Frentes * 2` = 8; dos referencias con estados distintos → lista de ambas en orden de `SymbolId` | API inexistente; colapsar en `BrokenReference` o lanzar falla | G3 |
| INV-16 | Kind no coherente en la petición por rack | Mezcla → métricas `Unavailable(KindIncoherent)`; `Kind` ausente → `Unavailable(KindAbsent)` | API inexistente | G1 |
| INV-17 | Disponibilidad D-14 | En el contexto de rack: `Rack.Frentes` enlaza y `Project.TotalRacks` da `UnknownNamespace` | Hoy todo da `UnknownNamespace` (`ExpressionBinder.cs:220-222`) | G3 |
| INV-18 | Fórmulas de propiedad y definiciones de variables sin `Rack.*` | Autoría de una propiedad con `=Rack.Frentes` y de una variable → `UnknownNamespace`, con los mismos mensajes. **Control positivo:** la misma expresión enlaza en una tabla sintética con entradas `rack` | Antes del cambio el control falla (no hay namespace `rack`). La variante incorrecta «ofrecer `rack` en el contexto de propiedad» rompe la aserción principal | G3 |
| INV-19 | **Homónimos entre namespaces** (A63-PV1-01) | Contexto de rack con la variable `Frentes` y `rack/frentes`: `{Frentes}` → variable; `Frentes` → variable (reglas vigentes); `Rack.Frentes` → calculado; sin `AmbiguousName` ni `OperatorInName` nuevos. Sin la variable homónima: `{Frentes}` → `UnknownSymbol`, nunca el calculado | Un índice sin namespace da `AmbiguousName` o reinterpreta `{Frentes}` | G3 |
| INV-20 | `OperatorInName` por namespace | Variable `A-B` y miembro `rack` cualquiera: el patrón solo nace de `projectVariable`; miembros `rack` nunca generan `OperatorInName` | Patrones calculados sobre todas las entradas | G3 |
| INV-21 | `Format → Parse → Bind` con homónimo (A63-PV1-02) | Árbol `Rack.Frentes * {Frentes}` con la variable `Frentes` presente → formatea `Rack.Frentes * Frentes` y vuelve al **mismo** árbol | El formatter con homónimos globales cualifica `Frentes`, o escribe `#{frentes}` para el calculado (`InvalidQualifier`) | G3 |
| INV-22 | Id `rack` ausente: se muestra y nunca enlaza | Árbol con `rack/zzz` ausente → `Rack.#{zzz}`; parsearlo y enlazarlo produce diagnósticos deterministas y ningún árbol. El primer código se fija al implementar | `Q(key)` daría `#{zzz}`, que se parsearía como cualificador de `projectVariable` | G3 |
| INV-23 | Clasificación canónica | `CanonicalShape.Classify(Rack.Frentes)` = `Expression` | Una clasificación por «referencia sola» la marcaría `DirectReference` | G3 |
| INV-24 | Persistencia cerrada, en los dos sentidos | Leer `"Namespace":"rack"` → `PresentButUnreadable` (caso G8 existente); escribir un árbol `rack` → excepción. **Control positivo:** la tabla en memoria **sí** conoce `rack` | Con una sola tabla, el lector aceptaría `rack` (`PersistedBoundExpressionJson.cs:95`) | G3 |
| INV-25 | Regla de ámbito para `rack` | Consumidor `Project` con una entrada `rack` en la tabla → `ScopeViolation` | La entrada no existe | G3 |
| INV-26 | El nombre visible no es identidad | Renombrar el miembro en una tabla de prueba no cambia el `SymbolId` ni el árbol | API inexistente | G3 |
| INV-27 | `Computed` es hoja fuera del registro | `RegistryEvaluation` o `DependencyGraph` con una entrada `Computed` → error de programación | El caso no existe | G3 |
| INV-28 | `projectVariable` intacto | `ExpressionRoundTripTests`, `ExpressionFormatterTests`, `ExpressionBinderTests` y `G8PersistenceIntegrationContractTests` siguen verdes | No regresión. Su RED efectivo es el de INV-19..21 y 24, que fallan si cambia la semántica | G3 |
| INV-29 | Resumen determinista y neutral | Mismo *snapshot* en otro orden → mismo `ProjectSummary`; `Population` no tiene campos de métricas; el ensamblado de Application no referencia WPF ni AutoCAD (control positivo con un tipo de prueba) | API inexistente | G4 |
| INV-30 | Una resolución por RackId y petición | N racks con 3 vistas → N resoluciones del Selectivo en `Full` y 0 en `Population` | Una por vista da 3N; `Population` con métricas resuelve | G4 |
| INV-31 | Identificadores de autoridad estables | La tabla cerrada de D-21 coincide con la de la prueba, en los dos sentidos | API inexistente | G4 |

## 21. Gates funcionales y plan de I-61

Cada gate entrega un resultado conductual observado por pruebas con RED→GREEN (LIFECYCLE §7). **Ninguno cierra por compilar.**

| Gate | Resultado observable | INV | Perfil inicial (`routing.md` §1) |
|---|---|---|---|
| **G1** Métricas por rack | `RackMetricRequest` de un rack devuelve frentes y vacíos con sus estados para cada kind, con una sola resolución | 04, 05, 09, 14, 16 | ROUTINE_IMPLEMENTATION (Balanced) |
| **G2** Población, veredicto único y agregados | `ProjectPopulation` y `BySystem` sobre *snapshots* sintéticos: proyección, E1..E6, lector por kind, veredicto único (con la delegación del handler) y determinismo | 01-03, 06-08, 10-13 | ROUTINE_IMPLEMENTATION (Deep, transversal) |
| **G3** *Built-ins* en I-49 | `Rack.Frentes * 2` en el contexto de rack con estados tipados; homónimos y formatter por namespace; persistencia cerrada; consumidores existentes sin cambios | 15, 17-28 | LONG_HORIZON_IMPLEMENTATION o ROUTINE (Deep); ARCHITECTURE_REVIEW del diff del núcleo antes del cierre |
| **G4** `ProjectSummary` y rendimiento | Resumen `Full` determinista sobre G1+G2; caracterización con contadores | 29-31 | ROUTINE_IMPLEMENTATION + CHARACTERIZATION |
| **READY** | Conformidad Architect + Coordinator, entrada factual de FOUNDATIONS y ADR aceptado | — | — |

- El Coordinator emite un contrato por gate. El Controller clasifica, elige ejecutor, modelo y *effort* (`routing.md` §§4-5) y verifica
  exact-SHA. Los Workers implementan y no declaran GATE PASS.
- Todo gate tiene RED real, así que C-F0-RED no lo bloquea.
- Orden G1 → G2 → G3 → G4. G3 solo depende de G1 y puede adelantarse a G2 por A-n.
- **Plugin:** solo la delegación de D-27, dentro de G2. El gate cierra con INV-12/13 (Application); la línea del handler no se valida
  «porque compila», sino porque su lógica entera es la función probada.

## 22. Owner Validation

**Candidata: NOT APPLICABLE para I-63 V1.** No hay UI, comandos, cambios de dibujo ni integración host nueva. La delegación de D-27 no
cambia ningún comportamiento observable (INV-13). Queda **sujeta a la re-revisión del Architect y al Freeze**. Si el Freeze añadiera
cualquier comando, resumen visible o adaptador del host, se planifica OV.

## 23. ADR necesario (M-08)

Un ADR nuevo, sucesor parcial de ADR-0043, con número asignado al integrar. Su borrador, cláusula por cláusula, está en el anexo A.
Es insumo para el Architect, no un archivo en `docs/adr`.

## 24. Riesgos y preguntas para el juicio adversarial

| ID | Riesgo | Postura V2 |
|---|---|---|
| R-01 | Segunda autoridad de conteo o cotización | E4 y E6 son autoridades únicas; E6 se centraliza (D-27) |
| R-03 | Deuda de `BomAuthoredAuthority` en kinds no selectivos | Se hereda solo para la pertenencia y E5 la compensa: el diseño ilegible ya no pasa |
| R-04 | Frentes en Φ3: una variable rota los deja `Unavailable` | Elegido por definición del predicado; alternativa *authored* descrita (§7) |
| R-06 | Coste por rack de `RegistryEvaluation` | Se mide en G4 |
| R-08 | Nombre de `FrentesVacios` | Se fija en el Freeze |
| R-11 | El lector D-26 reproduce comprobaciones de presencia de los handlers | Sin cambiar los handlers en V1; INV-06 fija los casos |
| R-12 | Diferencias declaradas con RACKBOMTOTAL (§10) | Magnitudes distintas; sin migración |

Preguntas para el Architect:
- **AQ-01.** ¿`NameRequired` es el diagnóstico adecuado para `Rack.` sin miembro en un contexto con `rack`, o prefiere `UnknownNamespace`?
- **AQ-02.** ¿Basta INV-22 con «diagnóstico determinista, código fijado al implementar», o exige congelar el código ya?
- **AQ-03.** ¿El lector D-26 sin adopción por los handlers es aceptable en V1, o pide que los `BuildBom` lo consuman (más ediciones en
  el Plugin)?
- **AQ-04.** ¿La política I-63 «Push Back con puerta indeterminada → `Unavailable`», frente al *fail-open* del handler, queda bien
  clasificada como M-04 sin M-03?
- **AQ-05.** ¿Es suficiente separar `Population` y `Full` como tipos para O-04, sin un estado `NotRequested`?

## 25. Trazado

| Obligación del mandato | Dónde |
|---|---|
| Identidad; namespaces; ámbito | §5 (D-02..D-04) |
| Contrato de *provider* | §8 (D-08) |
| Fuente única por métrica | §7 (D-07) |
| Fases; disponibilidad | §11 (D-13, D-14) |
| Ciclos y reentrada | §12 (D-15) |
| Matriz por sistema; primer conjunto | §6 (D-05, D-06) |
| Agregación; deduplicación | §10 (D-10..D-12, D-26, D-27) |
| Estados `Unavailable` / `NotApplicable` | §9 (D-09) |
| Modelo neutral | §14 (D-20) |
| Rendimiento | §18 (D-24) |
| ID23 | §8 (D-17) |
| Frontera con ID30 | §16 (D-22) |

| REQUIRED del Architect R1 | Resuelto en |
|---|---|
| A63-PV1-01 | §5 D-03; §13 D-16.4-5; INV-19, INV-20 |
| A63-PV1-02 | §13 D-16.6-7; INV-21..23 |
| A63-PV1-03 | §23; anexo A |
| A63-PV1-04 | §5 D-02; §6; §12 R6 |
| A63-PV1-05 | §10 D-27; §3 M-01/M-04; INV-12, INV-13 |
| A63-PV1-06 | §10 D-10a, E5, D-26; INV-06 |
| A63-PV1-07 | §10 D-11; INV-02, INV-11, INV-29 |
| A63-PV1-08 | §8 D-17; INV-14..16 |
| A63-PV1-09 | §19 D-25; §21; §22 |
| A63-PV1-10 | §10 D-12; §14; INV-07 |

| Criterio de éxito | Cómo |
|---|---|
| 1. Separación de ProjectVariable | D-01, D-23 |
| 2. Autoridad explícita | D-07, D-10, D-27 |
| 3. Deduplicación lógica | D-11; INV-01, INV-02 |
| 4. Primer conjunto en todos los sistemas | D-06; INV-07, INV-15 |
| 5. *Built-ins* sin segundo motor | D-16, D-17 |
| 6. Sin recursión oculta | D-15; INV-17, INV-18, INV-27 |
| 7. Estados explícitos | D-09; INV-07 |
| 8. Resumen neutral | D-20 |
| 9. ID30 sin recalcular | D-20, D-22 |
| 10. ID23 consume `Rack.*` | D-17; INV-14, INV-15 |

---

## Anexo A — Delta del ADR sobre ADR-0043 e I-49 V6 (borrador, cláusula por cláusula)

```text
Título: Métricas calculadas de solo lectura y namespace built-in rack del motor de expresiones
Estado: borrador para la re-revisión del Architect (I-63 Proposal V2)
```

**Se conserva sin cambios**:
- D1 (núcleo neutral). El contexto de rack calculado vive fuera del núcleo.
- D2 (motor numérico adimensional y fronteras de validez).
- D3 (unidades).
- D7 (gramática y funciones). La gramática de `palabra.miembro` ya existía.
- D8 (límites).
- D10 (persistencia de variables).
- D11 (persistencia híbrida de propiedades).
- D12 (extensión de ADR-0034).
- D13 (Schema V-0).
- D14 (fallo estructural frente a semántico). Un namespace no persistible en un payload sigue siendo estructural.
- D15 (dependencias, grafo y ciclos). `Computed` es hoja.
- D16 (`RegistryEvaluation`, única autoridad de valores de variables). `Computed` nunca entra.
- D17..D23.
- D5 completo en lo que toca a `projectVariable` (A2).
- V6 P25.6 (los valores calculados vienen de *snapshots* puros; el núcleo no lee AutoCAD, Domain ni catálogos) y P26.

**Se modifica**:

| Cláusula | Modificación |
|---|---|
| **D4** | El dominio de la canonicidad P2.5 se extiende a árboles con referencias `rack` en contextos que ofrecen `rack`: `Format → Parse → Bind` devuelve el mismo árbol. El id `rack` ausente tiene forma diagnóstica que nunca enlaza (`Rack.#{token}`) |
| **D5** | Namespaces: `projectVariable` y `rack` activos en memoria. Validez de clave por namespace (`rack`: *token* ASCII lowerCamel, `Ordinal`). `project` sigue **reservado**. «Hoy existe un namespace activo» pasa a ser «dos en memoria, uno persistible» |
| **D6, regla 4** | `palabra.` sigue siendo sintaxis de namespace. `Rack.<miembro>` solo enlaza donde el contexto ofrece `rack`; en otro caso, `UnknownNamespace`, como antes. `Rack.` sin miembro da `NameRequired` donde se ofrece `rack`. `Project.X` sigue dando `UnknownNamespace` |
| **D6, reglas 5-6 y nombres** | La búsqueda por nombre, los homónimos (`AmbiguousName`) y `OperatorInName` se aplican **por namespace**. Las referencias sin namespace solo buscan en `projectVariable` |
| **D6, Formatter** | Una referencia `rack` siempre se escribe `Rack.<miembro>`, nunca con `Q(key)`. Los homónimos se cuentan por namespace |
| **D9** | La tabla de *tokens* **persistidos** no cambia (`Namespace` = `projectVariable`). Se añade una tabla **separada** de namespaces en memoria. La regla de evolución se conserva: un namespace nuevo en un payload es fail-closed |
| **D24** | ID20 deja de ser solo preparación. Se implementan el namespace `rack`, el caso `Computed` y el ámbito `Rack` productivo; `project` sigue reservado. ID23 y ID28/29 siguen como antes, con `RackComputedExpressionContext` como vía de ID23 |
| **D25** | «ID20 productivo» sale de fuera de alcance solo en lo que implementa este ADR |
| **V6 P25.2** | Pasan a productivos `Rack.*`, el ámbito `Rack` y el caso de definición calculado; `Project.*` sigue reservado |
| **V6 P25.3** | La reserva gramatical se conserva para `Project` y para los contextos que no ofrecen `rack`; deja de valer dentro del contexto de rack calculado |

**Se sustituye**:

| Cláusula | Sustitución y motivo |
|---|---|
| **V6 P25.5** («el contexto de P6.7, evaluación de una propiedad de rack, es donde ID20 añadiría símbolos del rack») | Los símbolos `rack` viven **solo** en `RackComputedExpressionContext`, posterior al *resolve* de ese rack. El contexto de propiedad **no** los ofrece. **Motivo: R1**, porque `Rack.*` depende de Φ3, que depende de las propiedades vinculadas (ciclo `VerticalClearance → altura → fórmula`) |

**Decisiones nuevas**:
- disponibilidad por consumidor (D-14);
- reglas de ciclo R1..R6;
- fuente única por métrica (D-07);
- población cotizable (D-10..D-12, D-26, D-27);
- identidad `MetricId` frente a `SymbolId` (D-02);
- persistencia cerrada con dos tablas (D-18).
