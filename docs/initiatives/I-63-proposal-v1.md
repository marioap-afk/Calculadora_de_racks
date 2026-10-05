# I-63 — Proposal V1 — ID20, Computed Parameters & Project Summary Foundation

```text
Frozen: NO
Versión: V1 (2026-10-01)
Autor: sesión principal responsable de I-63, directamente; sin Controller, Worker ni subagentes
Base: I-63 a0654ae29f21532c1a97bf12d0fa3abc4ba1211f; origin/main 819955d61a6da4c811a11fbd11b5dca13f634b7c
Orden: «I-63 — COORDINATOR ORDER: PROPOSAL V1 + ARCHITECT PACKAGE» (decisiones §2, «Orden PV1»)
Arquetipo: NEW ARCHITECTURE
IMPLEMENTATION AUTHORIZATION = NO
```

Propuesta autocontenida para la revisión adversarial del Architect. **No** es Freeze. Resuelve P-06..P-13 y las obligaciones de
«PROPOSAL / FREEZE MUST DEFINE» del [mandato](../automation/decisions/I-63-owner-mandate.original.txt) con las decisiones del Owner
ya cerradas (§2). Las afirmaciones sobre código citan `ruta:línea` en la base; las clases son las de
[INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4 (`MEASURED`, `RECONSTRUCTED`, `INFERENCE`, `UNKNOWN`). Lo que la
Proposal decide se escribe como **D-nn**; lo que se congelaría, como invariante **INV-nn** con su obligación de prueba.

## 1. Objetivo, alcance V1 y no-objetivos

**Objetivo.** Una capa de **parámetros calculados de solo lectura** por rack y por proyecto, con una fuente única por métrica,
integrada en el motor de I-49 como símbolos *built-in* sin un segundo motor, y un **`ProjectSummary` neutral** que ID30, reportes,
la evolución de RACKLISTA, el BOM calculado o los diagnósticos puedan consumir sin recalcular.

**Alcance V1** (todo en Application, salvo el adaptador de captura del Plugin, D-25):

1. catálogo cerrado de parámetros calculados con identidad estable (D-02..D-06);
2. métricas por rack del Selectivo: `Rack.Frentes` y `Rack.FrentesVacios` (D-05, D-07); `SystemKind` como atributo del resumen;
3. población **cotizable**, deduplicación por RackId y agregados de proyecto: `TotalRacks`, `RacksBySystem` y, por sistema,
   `TotalFrentes` y `TotalFrentesVacios` del Selectivo (D-10..D-12);
4. estados explícitos `Available`, `NotApplicable`, `NotSupported` y `Unavailable` con razones cerradas (D-09);
5. integración en I-49: namespaces `rack` y `project`, resolución de `Rack.<miembro>` en un **contexto de rack calculado**
   posterior al *resolve*, persistencia cerrada (D-13..D-19);
6. `ProjectSummary` neutral, en memoria, con *provenance* para ID28/ID29 (D-20, D-21);
7. caracterización de rendimiento con pruebas sintéticas (D-24).

**No-objetivos** (mandato «OUT OF SCOPE» más decisiones del Owner): UI del Workspace, `ProjectSummaryWindow`, comandos o paletas
nuevos; BOM calculado (ID23) y su persistencia de fórmulas; referencias entre racks (ID21); UI de Explain o Impact Preview (ID28 e
ID29); reglas de ingeniería; fórmulas en Custom Properties; editores nuevos; cambios geométricos; reglas de producto nuevas;
**contrato común de inventario con I-64** y el *snapshot* de `MASTER-I63-I64-01` (P-14); recorrido de instancias alcanzables
(P-04); una métrica de copias; migrar RACKLISTA o RACKBOMTOTAL; cachés.

## 2. Entradas cerradas

| Fuente | Contenido que la Proposal aplica |
|---|---|
| Discovery R2 aceptado ([I-63-discovery.md](I-63-discovery.md), `23eeefc5`, CD-I63-F0-R2-04) | DC-01..09, EXP, matriz sistema × métrica, riesgos de ciclo; §§24-26 registran las decisiones del Owner |
| **P-01** (Owner) | `Project.TotalRacks` usa la población **cotizable**; cada condición se clasifica como pertenencia, cobertura no acreditada (`Unavailable`) o fallo de otra métrica (D-10) |
| **P-02a / P-02b** (Owner) | `Kind` ausente o desconocido → excluido; sin clasificación en `RacksBySystem` |
| **P-03** (Owner) | Cobertura no acreditada → `Unavailable`; nunca un parcial como total exacto; el fallo de otra métrica no lo provoca por sí solo |
| **P-04** (Owner) | Colocación = al menos una referencia directa activa; se conserva la limitación de los anidados |
| **P-05** (Owner + orden PV1) | `Rack.Frentes` = todos los frentes estructurales del fondo 0, vacíos incluidos. Métrica aparte de vacíos con el predicado: fondo 0, `Levels.Count == 0` y `FloorPalletCount <= 0` |
| **P-14** (Owner) | Sin contrato común con I-64; mecanismo propio de I-63; sin el *snapshot* de `MASTER-I63-I64-01`; convergencia diferida |
| I-49 | Proposal V6 + A1 + A2 + A3-R2 + [ADR-0043](../adr/0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md) + Freeze correctivo `I-49-consensus-freeze-v6-a1-a2-a3-r1.md` |
| [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md) §13 / D-16 de I-54 | Custom Properties: texto, sin namespaces ni ámbitos; la relación con ID20 se decide aquí (D-23) |

## 3. Arquetipo y materialidad

**NEW ARCHITECTURE** se mantiene. No se rebaja por elegir un catálogo cerrado: la iniciativa **crea** una autoridad transversal de
métricas y de población (M-01 creador) y un mecanismo de *providers* (M-07).

| ID | Estado | Evidencia y decisión |
|---|---|---|
| M-01 | **Activado (creador)** | Autoridad nueva de métricas por rack y de población cotizable (D-07, D-10). No sustituye a ninguna existente: consume `BomAuthoredAuthority`, `RackBomOutputGate`, el resolver del Selectivo y la identidad de rack |
| M-02 | **No activado** | V1 no persiste nada nuevo: ni referencias `rack`/`project`, ni el resumen, ni campos en sobres o registros. La tabla de namespaces **persistibles** sigue siendo `projectVariable` (D-18, INV-15) |
| M-03 | **No activado** | RACKLISTA, RACKBOMTOTAL, RACKEDITAR y RACKVARIABLES no cambian. En fórmulas de propiedad y definiciones de variables, `Rack.X` sigue dando `UnknownNamespace` (INV-12, INV-13) |
| M-04 | **Activado** | Semántica de fallo nueva y explícita del resumen (`Unavailable` por cobertura, exclusiones con diagnóstico; D-09). No cambia la de ningún consumidor existente |
| M-05 | **Activado** | Cambian contratos consumidos del núcleo: `SymbolNamespace`, la validez de clave de `SymbolId` por namespace, un caso de definición `Computed` y la resolución de `palabra.miembro` en el binder (D-16) |
| M-06 | **Activado (creador)** | Punto de extensión: catálogo cerrado de parámetros calculados y *providers* por kind (D-08) |
| M-07 | **Activado** | Registro cerrado de *providers* por token de kind y orquestador de fases. No es un *framework* abierto (D-08) |
| M-08 | **Activado** | ADR-0043 D5 («un namespace activo»), D9 (tabla de tokens) y D24/D25 (ID20 reservado) se modifican → ADR nuevo (§23, anexo A) |

## 4. Conceptos separados — D-01

| Concepto | Quién lo escribe | Persistencia | Puede ser símbolo de I-49 | Autoridad |
|---|---|---|---|---|
| **ProjectVariable** | El usuario (RACKVARIABLES) | Registro del dibujo | Sí, namespace `projectVariable`, ámbito `Project` | `ProjectVariablesStore` / I-48, I-49 |
| **ComputedParameter** | RackCad (derivado) | **Ninguna** en V1 | Sí, namespaces `rack` y `project` (D-13) | Catálogo y *providers* de I-63 |
| **CustomProperty** | El usuario (metadatos) | Sobre / registro (ADR-0039) | **No** en V1 (D-23) | ADR-0039 |

- Un ComputedParameter **nunca** se convierte en ProjectVariable. No se registra en el registro de variables, no se asigna, no se
  renombra y no aparece en RACKVARIABLES.
- Un nombre de variable de proyecto **nunca** se reinterpreta como parámetro calculado. `{Rack}` sigue siendo una variable; `Rack` y
  `Project` sin llaves siguen siendo `ReservedName` (`ExpressionBinder.cs:322`; ADR-0043 D6 regla 4).

## 5. Identidad, namespaces, ámbito y nombre visible — D-02, D-03, D-04 (P-10)

**D-02 — Identidad.** La identidad de un parámetro calculado **es** su `SymbolId = (namespace, clave)`:

| Namespace (token persistible en el futuro) | Palabra de sintaxis | Ámbito | Clave |
|---|---|---|---|
| `rack` | `Rack` | `SymbolScope.Rack` | *token* del catálogo |
| `project` | `Project` | `SymbolScope.Project` | *token* del catálogo |

- **Validez neutral de la clave** para `rack` y `project`: texto no vacío, igual a su `Trim()`, ASCII, que empieza por minúscula y
  solo contiene letras y dígitos. Comparador `Ordinal`. El núcleo **no** conoce el catálogo: solo esta regla.
- La regla de `projectVariable` (texto GUID, `OrdinalIgnoreCase`, A2) **no cambia ni un carácter** (INV-14).
- Los *tokens* del catálogo son contrato congelado, declarados explícitamente, con prueba en los dos sentidos. **Nunca** se derivan
  del nombre visible, de `enum.ToString()` ni de `nameof` (ADR-0043 D9).

**D-03 — Nombre visible.** Cada entrada del catálogo declara un nombre visible separado de su clave (por ejemplo `Frentes`). El nombre
solo sirve para mostrar y para resolver **al escribir**, igual que en ADR-0043 D5: `Rack.Frentes` → `SymbolId(rack, frentes)`. Cambiar
un nombre visible no cambia la clave ni rompe nada (INV-16).

**D-04 — Ámbito.** `rack` solo admite consumidores de ámbito `Rack`, y `project` solo `Project` o `Rack` (INV-11). Los ámbitos no son
la frontera de fase: la disponibilidad la fija D-14.

**Alternativa rechazada — claves GUID preasignadas (Discovery §16.3 A).** También conservaría las reglas de `projectVariable`, pero
convierte un catálogo cerrado en identidades opacas para diagnósticos, para la traza de ID28 y para la futura persistencia de ID23.
Y no ahorra cambios: el constructor de `SymbolId` ya rechaza cualquier namespace no registrado (`SymbolId.cs:87-92`), así que la
validez por namespace hace falta en ambos casos. Los *tokens* cerrados siguen el precedente de las funciones (`MIN`, `MAX`, `ABS`) en
ADR-0043 D9.

## 6. Primer conjunto de métricas y matriz por sistema — D-05, D-06

**D-05 — Catálogo V1** (cerrado; añadir una entrada exige una A-n o una iniciativa):

| Id | Nombre visible | Ámbito | Tipo | Unidad | Bindable en V1 | Fuente (D-07) |
|---|---|---|---|---|---|---|
| `rack`/`frentes` | `Rack.Frentes` | Rack | conteo (entero como `double`) | — | Sí, en el contexto de rack calculado | Selectivo resuelto, fondo 0 |
| `rack`/`frentesVacios` | `Rack.FrentesVacios` (nombre propuesto; el definitivo, en Freeze) | Rack | conteo | — | Sí, ídem | Ídem, con el predicado de P-05 |
| `project`/`totalRacks` | `Project.TotalRacks` | Project | conteo | — | **No** (D-14) | Población cotizable (D-10) |
| `project`/`racksBySystem` | — (no escalar) | Project | mapa kind → conteo | — | No | Ídem |
| `project`/`totalFrentes` | — (por sistema) | Project | conteo por sistema | — | No | Σ `rack`/`frentes` |
| `project`/`totalFrentesVacios` | — (por sistema) | Project | conteo por sistema | — | No | Σ `rack`/`frentesVacios` |

- **`SystemKind`** del rack es un atributo de texto del resumen (`KindToken`), no un símbolo: el motor solo maneja `double` finitos
  (ADR-0043 D5, `SymbolTable.cs` `FromLiteral`). Su fuente única es el *token* `Kind` coherente de las hermanas (D-10, E3).
- **Diferidas** (NeedsContract en el Discovery §14; requieren una decisión de producto que no existe): niveles, posiciones de tarima,
  altura, fondo, frentes de Dinámico, Push Back y Cantilever, y cualquier total de frentes **entre** sistemas. No se fabrican números
  equivalentes.

**D-06 — Matriz V1** (estado por kind y métrica; `NotSupported` = la métrica podría aplicar, pero V1 no la define):

| Métrica | Selectivo | Dinámico | Push Back | Cantilever | Cabecera | Cama |
|---|---|---|---|---|---|---|
| `Rack.Frentes` | **Available** o `Unavailable(razón)` | NotSupported | NotSupported | NotSupported | NotApplicable | NotApplicable |
| `Rack.FrentesVacios` | **Available** o `Unavailable(razón)` | NotSupported | NotSupported | NotSupported | NotApplicable | NotApplicable |
| `SystemKind` | `selective` | `dynamic` | `pushback` | `cantilever` | `cabecera` | `cama` |
| Pertenencia a `TotalRacks` | D-10 | D-10 | D-10 (+ puerta de salida, E6) | D-10 | D-10 | D-10 |

`NotApplicable` sigue al Discovery §14 (Cabecera y Cama no tienen frentes de carga).

## 7. Fuente única por métrica — D-07

| Métrica | Fuente única | Entradas | No es fuente |
|---|---|---|---|
| `Rack.Frentes` | `SelectiveDepthLayout.BaysOfFondo(system, 0).Count` sobre el `SelectiveRackSystem` resuelto del rack (`SelectiveDepthLayout.cs:23`) | Autoridad *authored* (E4) → `SelectiveEffectiveDesignResolver.ResolveAccredited` → `SelectiveGeometryResolver` con catálogo | `Bays.Count` del diseño leído por otra vía; el BOM; los *builders* de vista |
| `Rack.FrentesVacios` | Mismas bahías del fondo 0: cuenta las que cumplen `Levels.Count == 0 && FloorPalletCount <= 0` | Ídem | La regla de Planta `Levels.Count == 0` (`SelectivePlantaBuilder.cs:371-373`), que es propia de esa representación (orden PV1) |
| `TotalRacks`, `RacksBySystem` | Evaluación de población de I-63 (D-10) | Captura (D-25), autoridades E1..E6 | `RackListBuilder` (población definida), `ConsolidatedBom.RackCount` (resultado de una corrida), etiquetas |
| `TotalFrentes`, `TotalFrentesVacios` (Selectivo) | Suma de los valores por rack de los incluidos | Resultados por rack | Recontar bahías en el agregado |
| `SystemKind` | *Token* `Kind` coherente de las hermanas legibles | Captura | `KindLabel`, `BomLabel`, `LibraryLabel` (presentación) |

- **Equivalencia con el diseño** `[RECONSTRUCTED]`: el resolver crea una bahía resuelta por bahía de diseño y fondo
  (`SelectiveGeometryResolver.cs:248-328`). Una bahía resuelta sin niveles ni tarima de piso corresponde a una bahía de diseño sin
  celdas (`:268-273`):
  - con larguero a piso, la celda 0 da un nivel (`:281-293`);
  - con una sola celda y sin larguero a piso hay tarima de piso (`:294-300`);
  - con más celdas y sin larguero a piso hay tarima de piso y niveles (`:307`). Se usa la fuente **resuelta** porque la orden define el predicado sobre esos campos. La equivalencia queda como
  oráculo (INV-05), no como segunda definición.
- **Alternativa rechazada:** calcular desde el diseño (fase *authored*, más barata y robusta ante una variable rota). Se rechaza en V1
  porque crearía una segunda definición del predicado que hay que mantener sincronizada. Si el Architect la prefiere, el cambio
  queda acotado a la fase de las dos métricas.

## 8. Contrato de *provider* — D-08

- **Forma.** Una función **pura** por kind productivo: `Compute(RackMetricInput) → RackMetricResults`.
  - `RackMetricInput` lleva lo que el **orquestador** ya resolvió para ese rack: identidad, *token* de kind, resultado de la autoridad
    de hermanas y, si el *provider* lo declaró, el resultado efectivo y el sistema resuelto.
  - `RackMetricResults` lleva un `MetricValue` por **cada** entrada `rack` del catálogo, con su estado (D-09).
  - Los nombres de tipos son ilustrativos y no se congelan; sí se congelan la forma y las reglas.
- **Declaración de fase.** Cada *provider* declara, por métrica, la fase mínima que necesita (D-13). El orquestador solo resuelve un
  rack si algún *provider* lo necesita: en V1, solo el Selectivo. Los demás devuelven `NotSupported` o `NotApplicable` sin coste.
- **Registro cerrado.** Una tabla explícita indexada por los seis *tokens* de `RackEmbedDocument.Kind*`, construida con
  `KindDispatch` (sin reflexión, sin configuración, sin descubrimiento). Un kind sin *provider* es un error de programación en
  construcción. No hay registro abierto ni *plugins*.
- **Prohibido en un *provider***: leer AutoCAD, catálogos o el registro de variables; llamar a resolvers, a la evaluación del
  registro o al binder; invocar a otro *provider*; tener estado o caché. Los recibe resueltos o no los usa (INV-09).
- **Sin *framework* universal:** no hay métricas genéricas por reflexión, ni expresiones sobre métricas dentro de los *providers*, ni
  composición dinámica. Una métrica nueva es código y entrada de catálogo revisados.

## 9. Estados, razones y diagnósticos — D-09

**Estado de un `MetricValue`** (cerrado):

| Estado | Significado | Tiene valor |
|---|---|---|
| `Available` | Calculado con su fuente única | Sí, `double` finito |
| `NotApplicable` | La semántica no existe para este kind | No |
| `NotSupported` | Podría aplicar, pero V1 no la define | No |
| `Unavailable(razón)` | Debería existir y no se pudo acreditar | No |

**Razones de `Unavailable`** (cerradas en V1; se transportan como datos, no como texto):
- `EnvelopeUnreadable`: sobre ilegible, con o sin `Id` tanteable;
- `DesignUnreadable`: el *store* del kind no lee el diseño;
- `SiblingsDivergent`: hermanas legibles que no describen el mismo estado *authored*;
- `RegistryUnreadable`: el registro de variables existe y no se puede leer;
- `EffectiveFailed(outcome)`: fallo del resolver efectivo con su `SelectiveEffectiveOutcome` (por ejemplo
  `BrokenProjectVariableReference`);
- `CatalogUnavailable`;
- `ResolveFailed`;
- `KindIncoherent`: hermanas con *tokens* de kind distintos;
- `CoverageNotAccredited`: solo agregados;
- `MemberMetricUnavailable`: solo agregados, con los RackIds afectados.

**Pertenencia de un rack:** `Included`, `Excluded(motivo)` (motivos: `NotPlaced`, `KindAbsent`, `KindUnknown`, `SiblingsDivergent`,
`OutputBlocked`) o `Undetermined(razón)`. Un rack `Undetermined` hace que la cobertura no esté acreditada.

**Diagnósticos del resumen:** cada exclusión salvo `NotPlaced`, cada `Undetermined` y cada definición colocada que no se puede atribuir
generan un diagnóstico con su clave de definición, el RackId (si se conoce) y la razón. Una exclusión por regla **no** es parcialidad.

**Regla de agregación** (INV-07): un agregado es `Available` solo si la cobertura de su población está acreditada y cada miembro que
aporta tiene la métrica `Available`. En otro caso es `Unavailable`, nunca un número parcial.

## 10. Población cotizable, deduplicación y agregación — D-10, D-11, D-12

**D-10 — «Cotizable» V1.** Un rack lógico (RackId) es cotizable si cumple **E1 ∧ E2 ∧ E3 ∧ E4 ∧ E6**. Cada condición tiene una
autoridad existente y una clasificación. Ninguna se define por el éxito de construir un BOM: no hay circularidad.

| # | Condición | Autoridad (existente) | Si falla |
|---|---|---|---|
| E1 | Identidad acreditada: sobre legible con `Id` no vacío | `RackEmbedStore.Deserialize` (`RackEmbedDocument.cs:97-129`); Rack Identity | Definición **colocada** sin identidad atribuible (casos A y B del Discovery §16.1) → **cobertura no acreditada**. Sin colocar → se ignora |
| E2 | Colocado: ≥ 1 referencia directa activa en alguna vista (P-04) | Recuento de `ScanEnvelopes` (`RackBlockFinder.cs:84-86`) | **Exclusión** `NotPlaced` (sin diagnóstico) |
| E3 | Kind presente, conocido y coherente entre hermanas legibles | Los seis *tokens* de `RackEmbedDocument`, con la igualdad **ordinal** del despacho de handlers (`KindHandlerRegistry.TryResolveAll` → `KindDispatch.TryGet`) | Todas sin kind → **exclusión** `KindAbsent`; todas con el mismo *token* desconocido → **exclusión** `KindUnknown` (P-02a); mezcla → `Undetermined(KindIncoherent)` |
| E4 | Autoridad *authored* para cotizar | `BomAuthoredAuthority.Resolve` (`Bom/BomAuthoredAuthority.cs`): compara el Selectivo con `SelectiveAuthoredAuthority`; en los demás kinds aprueba la primera vista | `DivergentSiblings` → **exclusión** `SiblingsDivergent`; `UnreadableSibling` → `Undetermined(DesignUnreadable)` |
| E6 | Sin bloqueo de salida | `RackBomOutputGate.For` (`Bom/RackBomOutputGate.cs:49`); hoy solo Push Back tiene puerta (`PushBackKindHandler.cs:56-71`) | Motivo de bloqueo → **exclusión** `OutputBlocked`; no se puede resolver o falta el catálogo → `Undetermined(ResolveFailed / CatalogUnavailable)` |
| — | Registro legible, sin variables rotas, diseño Selectivo resoluble | `SelectiveEffectiveDesignResolver` | **Fallo de otra métrica** (frentes y vacíos `Unavailable`). **No** toca la pertenencia (P-03) |

(E5, «diseño legible», no es una condición aparte: se manifiesta en E4 como `UnreadableSibling` o en E6 como fallo de *resolve*.)

- **Por qué no es circular:** E1..E6 se evalúan con autoridades anteriores al cálculo del BOM y de cualquier métrica. «Cotizable»
  **no** significa «salió en un listado».
- **Por qué E4 usa `BomAuthoredAuthority`:** es la autoridad vigente que decide si un rack tiene un estado *authored* cotizable y no
  es presentación. Usar otra (por ejemplo los comparadores AUTH-13 para los kinds no selectivos) crearía una **segunda autoridad de
  cotización**. Su deuda conocida (aprueba la primera vista en kinds no selectivos; Discovery §18) se hereda solo para la pertenencia
  y queda declarada (R-03). No influye en los valores de V1, que solo existen para el Selectivo, cuya autoridad sí compara.
- **Lo que no es autoridad:** `ConsolidatedBom`, `Racks.Count`, `TotalCopies`, `BomLabel` o las etiquetas de la ventana.

**D-11 — Deduplicación por RackId.** La población se agrupa por `Id` con `StringComparer.OrdinalIgnoreCase`, la igualdad vigente de
Rack Identity, igual que `RackListBuilder.Build` y `BomTotal`. Sin normalizar GUIDs y sin inventar identidad. Un `Id` tanteado en un
sobre ilegible **no** identifica ni fusiona (Discovery §16.1, caso B; INV-02).

**D-12 — Agregados.**
- `TotalRacks` = `Available(n)`, con n el número de RackIds `Included`, si y solo si la cobertura está acreditada: ninguna definición
  colocada sin atribuir (E1) y ningún rack `Undetermined`. En otro caso, `Unavailable(CoverageNotAccredited)` con los diagnósticos.
- `RacksBySystem` = mapa *token* de kind → número de incluidos. Es `Available` si y solo si `TotalRacks` lo es. Suma `TotalRacks` por
  construcción: todo incluido tiene un kind conocido y coherente (INV-06).
- `TotalFrentes` y `TotalFrentesVacios` del Selectivo: suma por rack sobre los incluidos de ese kind, con la regla de agregación de
  D-09. No hay total de frentes entre sistemas en V1.

**Relación con los conteos existentes (P-08):** `TotalRacks`, `ConsolidatedBom.RackCount` y las filas de RACKLISTA son magnitudes
declaradas distintas. Ninguno se migra.

| Caso | RACKBOMTOTAL hoy | `TotalRacks` V1 |
|---|---|---|
| Sobre colocado ilegible o sin `Id` | Aborta | `Unavailable` |
| `Kind` ausente, colocado | Aborta (inclasificable) | Excluido `KindAbsent` |
| `Kind` desconocido, colocado | Aborta (sin handler) | Excluido `KindUnknown` |
| Selectivo con hermanas divergentes | Lo salta con aviso | Excluido `SiblingsDivergent` |
| Diseño ilegible | Lo salta con aviso (listado parcial) | `Unavailable` |
| Push Back bloqueado | Aborta | Excluido `OutputBlocked` |
| Variable rota | Aborta | Incluido; frentes `Unavailable` |

`[INFERENCE]` En los casos en que **ambos** producen resultado, el conjunto incluido es el mismo: el BOM sin avisos cotiza exactamente
los racks que cumplen E1..E6. Esa coincidencia no se congela como invariante, porque RACKBOMTOTAL no se toca.

## 11. Fases de evaluación y disponibilidad de símbolos — D-13, D-14

**D-13 — Fases** (frontera real del código, sin capas nuevas de motor):

| Fase | Contenido | Autoridades |
|---|---|---|
| Φ0 Captura | Definiciones, sobres, recuento de referencias directas | `ScanEnvelopes` |
| Φ1 *Authored* | Diseño tipado y autoridad de hermanas | *Stores* del kind, `BomAuthoredAuthority` |
| Φ2 Efectivo | Evaluación del registro + propiedades vinculadas → diseño efectivo | `SelectiveEffectiveDesignResolver` |
| Φ3 Resuelto | Sistema resuelto con catálogo | `SelectiveGeometryResolver`, `PushBackResolver` |
| Φ4 Agregado | Población y agregados sobre los resultados por rack | I-63 |

Fase mínima por métrica: `Rack.Frentes` y `Rack.FrentesVacios`, Φ3; pertenencia, Φ1 (Φ3 solo para la puerta de Push Back);
`TotalRacks`, Φ4.

**D-14 — Disponibilidad por consumidor** (fail-closed; INV-10..INV-13):

| Consumidor | Ámbito | `Rack.*` | `Project.*` | Comportamiento si se escribe |
|---|---|---|---|---|
| Definición de variable de proyecto (RACKVARIABLES) | Project | No | No | `UnknownNamespace`, como hoy |
| Fórmula de propiedad vinculada del Selectivo (RACKEDITAR) | Rack | No | No | `UnknownNamespace`, como hoy |
| **Contexto de rack calculado** (nuevo, para ID23) | Rack | **Sí**: valores Φ3 de **ese** rack | No | Resuelve `Rack.<miembro>`; `Project.*` da `UnknownNamespace` |
| `ProjectSummary` | — | (datos, no expresiones) | (datos) | — |

Habilitar un símbolo en otro consumidor exige demostrar que su fase es anterior e independiente de lo que ese consumidor produce, con
prueba y A-n aprobada por Architect y Coordinator. Ese es el mecanismo del mandato «no habilitar hasta demostrar».

## 12. Ciclos y reentrada — D-15

Reglas (todas con prueba en §20):

- **R1.** Ningún símbolo `rack` o `project` está disponible en Φ2 ni antes. Las definiciones de variables y las fórmulas de propiedad
  no los ven (D-14). Esto rompe por construcción el ciclo del mandato `Rack.Altura → altura efectiva → fórmula de altura`, y el de
  holgura `VerticalClearance → altura resuelta → fórmula de VerticalClearance` (Discovery §16.2).
- **R2.** El contexto de rack calculado solo se construye a partir de resultados Φ3 **ya terminados** de un rack. Evaluar una expresión
  nunca dispara una resolución: no hay resolución perezosa ni reentrada.
- **R3.** Los *providers* son puros y no invocan al motor, a resolvers ni a otros *providers* (D-08).
- **R4.** Una entrada `Computed` es una **hoja**: no tiene expresión, `DependencyGraph` no le crea aristas y `RegistryEvaluation` nunca
  la ve. Un contexto de registro que la contenga es un error de programación (INV-17).
- **R5.** Ningún consumidor V1 escribe en el rack a partir de un valor calculado: el contexto de rack calculado es de solo lectura.
  Un futuro escritor (ID23) hereda R1 y R2 y su propia revisión.
- **R6.** `Project.*` no es *bindable* en V1. Un agregado de proyecto leído desde un rack exigiría resolver todo el dibujo y abriría el
  ciclo rack → proyecto → rack.

## 13. Integración con el Expression Engine — D-16..D-19

**D-16 — Cambios en el núcleo** (`RackCad.Application.Expressions`; neutrales: ningún tipo del núcleo nombra racks, sistemas ni
métricas):

1. `SymbolNamespace` gana `Rack` y `Project`, y la tabla de *tokens* gana `rack` y `project` (`SymbolId.cs:16-69`).
2. La validez de clave pasa a ser **por namespace** (D-02). `SymbolId.ProjectVariable` y su regla no cambian (`SymbolId.cs:85-108`).
3. `SymbolDefinitionKind.Computed`, el caso reservado por P25.2, sin valor dentro de la tabla.
4. **Binder:** `NamespaceReferenceSyntax` (hoy siempre `UnknownNamespace`, `ExpressionBinder.cs:220-222`) resuelve así:
   - palabra `Rack` o `Project` (sin distinguir mayúsculas) → namespace;
   - si la tabla del contexto **no tiene ninguna entrada** de ese namespace → `UnknownNamespace` (comportamiento actual);
   - si la tiene → búsqueda del miembro por nombre visible exacto `OrdinalIgnoreCase`, o `UnknownSymbol`;
   - después, la regla de ámbito vigente (`ExpressionBinder.cs:376-379`).

   No hay cualificador `#` en referencias de namespace.
5. **Formatter:** escribe `SymbolId(rack, k)` como `Rack.<NombreVisible>` desde la tabla; hoy solo trata `ProjectVariable`
   (`ExpressionFormatter.cs:64`).

No cambian el lexer, el parser, el evaluador, los límites, `DependencyGraph` ni `RegistryEvaluation`, más allá de las guardas de R4.

**D-17 — Contexto de rack calculado (vía para ID23).** Una API pura de Application:
- se construye a partir de un rack ya evaluado (Φ3) y, opcionalmente, de la evaluación del registro de la misma lectura;
- su tabla tiene las entradas del registro (`projectVariable`) más las entradas `rack` del catálogo (`Computed`);
- se enlaza con ámbito de consumidor `Rack`, y `Rack.Frentes * 2` se enlaza y se evalúa contra los valores de **ese** rack;
- **antes** de llamar a `ExpressionEvaluator.Evaluate`, que exige un valor finito por símbolo presente (`ExpressionEvaluator.cs:124-175`),
  el contenedor comprueba los estados: si alguna referencia `rack` no está `Available`, devuelve `Unavailable` con el `SymbolId` y la
  razón, sin evaluar. `BrokenReference` conserva su significado.

En V1 no hay consumidor productivo: lo ejercitan las pruebas. Es la vía estable que pide el mandato para ID23.

**D-18 — Persistencia cerrada.** `PersistedBoundExpressionJson` lee y escribe con `SymbolNamespaces.TryParseToken` y `Token`
(`:95`, `:170`). V1 introduce un conjunto explícito de **namespaces persistibles** = `{ projectVariable }`:
- al leer, `"Namespace":"rack"` o `"project"` sigue siendo un fallo estructural. El registro queda `PresentButUnreadable`, como fija hoy
  `G8PersistenceIntegrationContractTests` (Discovery §5.2);
- al escribir, un árbol con un namespace no persistible es un error de programación, nunca un archivo.

Así, ningún documento nuevo contiene *tokens* nuevos, y la compatibilidad con *builds* anteriores no cambia (INV-15). Persistir
referencias calculadas es decisión de ID23, con su ADR (ADR-0043 D24).

**D-19 — Compatibilidad de textos.** Antes de V1 ningún texto con `palabra.miembro` era comprometible (`UnknownNamespace`), así que
ningún documento existente lo contiene. Los consumidores existentes siguen dando `UnknownNamespace` (D-14). No se reinterpreta nada.

## 14. `ProjectSummary` neutral — D-20

```text
ProjectSummary
├── Totals:      TotalRacks : MetricValue
├── BySystem:    kindToken → { RackCount : MetricValue, TotalFrentes? : MetricValue, TotalFrentesVacios? : MetricValue }
├── Racks:       RackSummary { RackId, KindToken?, DisplayName (solo presentación), Membership, Metrics: Id → MetricValue }
└── Diagnostics: { Code, RackId?, DefinitionKey?, Reason }
```

- Puro, inmutable, en memoria y **no persistido**. Orden determinista: RackId `OrdinalIgnoreCase` y, después, `Ordinal`.
- Sin UI, sin textos localizados obligatorios (los códigos son datos) y sin dependencias de WPF ni del Plugin.
- Un consumidor (ID30, reportes, RACKLISTA, BOM calculado) **presenta** el resumen; no lo recalcula (criterio de éxito 9).
- `DisplayName` = primer nombre no vacío de las hermanas, solo para mostrar; nunca es identidad.

## 15. *Provenance* para ID28 / ID29 — D-21

Cada `MetricValue` conserva en memoria, sin persistir (ADR-0043 D24):
- el `SymbolId`;
- el RackId;
- la autoridad fuente (un identificador estable, por ejemplo `selective.resolved.fondo0`);
- la fase;
- la definición representativa;
- las entradas usadas (por ejemplo el resultado de `BomAuthoredAuthority` y el *outcome* efectivo).

Cada agregado conserva los RackIds incluidos y los excluidos con su motivo. El contexto de rack calculado conserva el `BoundExpression`
y los `SymbolId` leídos (la traza opt-in del evaluador). Basta para que Explain diga de dónde sale un valor e Impact Preview sepa qué
lo consume. No se implementa ninguna UI ni un motor de dependencias nuevo.

## 16. Frontera con I-64 — D-22 (P-14)

- I-63 tiene **su propio** mecanismo: la captura de D-25, la población D-10 y la agregación. I-64 tiene el suyo: el inventario
  runtime, la navegación, la selección y la UI.
- No hay contrato común, ni *snapshot* de `MASTER-I63-I64-01`, ni consumo de código no integrado en ningún sentido.
- I-64 puede **presentar** `ProjectSummary` cuando esté integrado en `main`, como cualquier consumidor.
- **Hecho externo de coordinación, ya alineado:** la Proposal V2 de I-64 (`ab4efe86`) dependía de un *snapshot* de I-63. Su V3
  (`1f1530be`, evidencia §15 de I-64, resumen de I-64 de una orden de su Coordinator) registra `MASTER-I63-I64-02`:
  - retira la fundación o *snapshot* compartido y la autoría de I-63;
  - conserva la misma frontera;
  - I-64 no depende de la integración de I-63;
  - una fundación común futura exigiría una decisión nueva del Master.

  Coincide con P-14. I-63 no consume la representación interna de I-64 y no diseña I-64 (R-07).

## 17. Compatibilidad — D-23

- **ProjectVariable:** la identidad, la regla de clave, el comparador, la ida y vuelta, el cualificador `#` y el formato persistido no
  cambian (INV-14). El registro no gana entradas calculadas.
- **CustomProperty:** sus valores son texto (ADR-0039 §13; I-54 D-16), **no** se exponen como símbolos en V1 y no ocupan los namespaces
  `rack` ni `project`. Una relación futura ID24 ↔ ID20 exige su propia decisión.
- **Legacy:** sin RackId no hay rack; nunca se inventa identidad (Rack Identity). `{Rack}` y `{Project}` siguen siendo variables.

## 18. Rendimiento — D-24

**Estrategia.**
- Una lectura del resumen = **una** captura (una travesía de definiciones), **una** lectura del registro y **una** carga de catálogo.
- Por rack incluido o candidato: una evaluación de autoridad.
- Una resolución Φ2+Φ3 **solo** para el Selectivo (métricas) y para Push Back (puerta de salida), una vez por RackId, no por vista.
- Sin caché ni resolución repetida dentro de una lectura; lecturas repetidas recalculan, sin caché antes de medir (mandato).

**Coste existente heredado** `[MEASURED]`: `SelectiveEffectiveDesignResolver.ResolveWith` evalúa el registro **por rack**
(`SelectiveEffectiveDesignResolver.cs:134`). V1 no lo cambia (sería M-05 sobre I-48 e I-49). Lo mide y lo deja como dato para una
decisión posterior.

**Caracterización (pruebas nuevas en G4, sintéticas y en Core):**
- métrica de un rack por fase;
- agregación con N = 1, 10, 100 y 1000 racks y varias vistas por rack;
- número de resoluciones por RackId y por lectura, contado con un contador inyectado en los costados puros, nunca con relojes como
  oráculo;
- lecturas repetidas.

Se registran tiempos medidos, sin umbrales inventados.

## 19. Capas — D-25

- **Application** (100 % puro): catálogo, *providers*, orquestador de fases, población, agregación, `ProjectSummary`, contexto de rack
  calculado y cambios neutrales del núcleo.
- **Plugin:** un **adaptador de captura** delgado. Recorre `ScanEnvelopes(includeReferenceCount: true)` y entrega a Application
  definiciones, sobres crudos y recuentos, más la lectura del registro y el catálogo cargado. Solo transporta. Es el mecanismo propio
  de I-63 (P-14). No hay comando ni UI: lo compila el job «Build Plugin without AutoCAD» y no tiene consumidor productivo en V1 (R-05).
- **UI:** nada.

## 20. Invariantes → obligaciones de prueba

Todas en `tests/RackCad.Tests` (Core), salvo indicación. «RED esperado» = cómo falla hoy, o con la alternativa incorrecta, antes del
código del gate.

| ID | Invariante | Escenario / oráculo | RED esperado | Gate |
|---|---|---|---|---|
| INV-01 | N vistas o colocaciones de un RackId aportan 1 a `TotalRacks` | 3 definiciones (frontal, lateral ×2) con referencias 2/1/0 → 1 | La API no existe; un conteo por vista da 3 | G2 |
| INV-02 | Igualdad de RackId = `OrdinalIgnoreCase`, sin normalizar; el `Id` tanteado no fusiona | `Rack-A` / `rack-a` → 1; un sobre ilegible con tanteo `Rack-A` → `Unavailable`, no un rack más | Ídem; `Ordinal` da 2 | G2 |
| INV-03 | E1..E6: cada caso da exactamente su clasificación de D-10 | Una prueba por fila de la tabla de D-10 y por caso A-F del Discovery §16.1 | Ídem | G2 |
| INV-04 | `Rack.Frentes` = bahías del fondo 0, vacías incluidas, sin sumar ni maximizar fondos | Fondo 0 con 4 frentes (1 vacío) y fondo 1 con 6 → 4 | La API no existe; «máximo» da 6; «sin vacíos» da 3 | G1 |
| INV-05 | `Rack.FrentesVacios` cuenta `Levels.Count == 0 && FloorPalletCount <= 0` en el fondo 0 | Frente con tarima de piso y sin larguero → **no** vacío; frente sin celdas → vacío; equivalencia con «diseño sin celdas» | La regla de Planta cuenta el de tarima de piso como vacío | G1 |
| INV-06 | `RacksBySystem` suma `TotalRacks` cuando ambos son `Available` | Mezcla de kinds con excluidos | — (propiedad sobre G2) | G2 |
| INV-07 | Un agregado nunca es un número parcial | Un incluido con frentes `Unavailable` → `TotalFrentes` `Unavailable`; `TotalRacks` sigue `Available` | Un agregado que salta miembros devuelve un número | G2 |
| INV-08 | Una variable rota no cambia `TotalRacks` | Selectivo incluido con referencia rota | Un `TotalRacks` que dependa del efectivo baja o pasa a `Unavailable` | G2 |
| INV-09 | Los *providers* son puros | Prueba arquitectónica de dependencias: el espacio de los *providers* no referencia `Persistence`, `ProjectVariables` ni resolvers; un contador demuestra cero resoluciones dentro de un *provider* | La guarda no existe | G1 |
| INV-10 | Disponibilidad D-14 en el contexto de rack calculado | `Rack.Frentes * 2` → 8 para el rack de INV-04; `Project.TotalRacks` → `UnknownNamespace` | Hoy `UnknownNamespace` (`ExpressionBinder.cs:220-222`) | G3 |
| INV-11 | Regla de ámbito para `rack` | Consumidor `Project` con una entrada `rack` → `ScopeViolation` | La entrada no existe | G3 |
| INV-12 | Las fórmulas de propiedad no ven `Rack.*` | Autoría de una propiedad vinculada con `=Rack.Frentes` → `UnknownNamespace`; sin cambio de mensajes | Característica: verde hoy; debe seguir verde tras G3 (prueba de no regresión escrita antes del cambio) | G3 |
| INV-13 | Las definiciones de variables no ven `Rack.*` ni `Project.*` | Ídem en RACKVARIABLES | Ídem | G3 |
| INV-14 | `projectVariable` intacto | `ExpressionRoundTripTests`, `ExpressionFormatterTests` y `G8PersistenceIntegrationContractTests` siguen verdes; clave GUID con la regla A2 | Verdes hoy (no regresión) | G3 |
| INV-15 | Persistencia cerrada | Lectura de `"Namespace":"rack"` → `PresentButUnreadable` (caso existente); escribir un árbol con `rack` → excepción | El escritor actual escribiría el *token* si existiera en la tabla | G3 |
| INV-16 | El nombre visible no es identidad | Renombrar el nombre visible en una tabla de prueba no cambia el `SymbolId` ni el árbol enlazado | La API no existe | G3 |
| INV-17 | Las entradas `Computed` son hojas fuera del registro | `RegistryEvaluation` o `DependencyGraph` con una entrada `Computed` → error de programación | No existe el caso | G3 |
| INV-18 | Valor no disponible ≠ referencia rota | Contexto con `Rack.Frentes` `Unavailable` → resultado `Unavailable` con razón, no `BrokenReference` | El evaluador lanzaría por falta de valor | G3 |
| INV-19 | El resumen es determinista y neutral | Mismo *snapshot* en otro orden de entrada → mismo `ProjectSummary`; el ensamblado de Application no referencia WPF ni AutoCAD | La API no existe | G4 |
| INV-20 | Una resolución por RackId y lectura | Contador: N racks con 3 vistas → N resoluciones del Selectivo | Una por vista daría 3N | G4 |

## 21. Gates funcionales y plan de I-61

Cada gate entrega un resultado conductual verificable por pruebas (LIFECYCLE §7). La forma del mandato (F1..F6) se reagrupa porque
«modelo + contrato» solo sería un gate de capa.

| Gate | Resultado observable | Invariantes | Perfil I-61 inicial (routing §1) |
|---|---|---|---|
| **G1** Métricas del rack | Para Selectivos sintéticos, `Rack.Frentes` y `Rack.FrentesVacios` dan los valores de INV-04 e INV-05 con sus estados; los demás kinds dan `NotSupported` o `NotApplicable` explícitos | INV-04, 05, 09 | ROUTINE_IMPLEMENTATION (Balanced; pruebas primero) |
| **G2** Población y agregados | `TotalRacks`, `RacksBySystem` y los totales del Selectivo sobre *snapshots* de captura sintéticos, con E1..E6, deduplicación y `Unavailable` | INV-01..03, 06..08 | ROUTINE_IMPLEMENTATION (Deep, transversal a capas) |
| **G3** *Built-ins* en I-49 | `Rack.Frentes * 2` se evalúa en el contexto de rack calculado; fórmulas de propiedad y variables sin cambios; persistencia cerrada | INV-10..18 | LONG_HORIZON_IMPLEMENTATION o ROUTINE (Deep); ARCHITECTURE_REVIEW del diff del núcleo antes del cierre |
| **G4** `ProjectSummary` + captura + rendimiento | Resumen determinista desde un *snapshot*; el adaptador del Plugin compila; caracterización registrada | INV-19, 20 | ROUTINE_IMPLEMENTATION + CHARACTERIZATION |
| **READY** | Conformidad Architect + Coordinator (READY-06), FOUNDATIONS (entrada factual), ADR aceptado | — | — |

- **I-61:** el Coordinator emite un contrato de gate por gate. El Controller clasifica y elige ejecutor, modelo y *effort* conforme a
  `routing.md` §§4-5 y verifica exact-SHA. Los Workers implementan y prueban, y no declaran GATE PASS.
- Cada gate tiene RED→GREEN real (comportamiento nuevo), así que C-F0-RED no lo bloquea. INV-12..14 son pruebas de **no regresión**
  que deben escribirse y verse verdes **antes** del cambio del núcleo. Se declaran así para no fabricar RED.
- Orden: G1 → G2 → G3 → G4. G3 puede ir antes que G2 si el Coordinator lo resecuencia por A-n, porque no dependen entre sí.

## 22. Owner Validation

| Escenario | Aplica | Motivo |
|---|---|---|
| Comportamiento visible en AutoCAD o UI nuevos | **No** | V1 no añade comando, ventana, paleta ni cambio de dibujo (no-objetivos) |
| Checklist general de la guía manual | Lo decide el workflow normal en el Freeze | La guía es aditiva; no se reduce |

Propuesta: **OV NOT APPLICABLE** para V1, a decidir por el Coordinator en el Freeze conforme a la guía. No se añade UI para forzarla
(mandato). Si el Freeze añadiera cualquier comando o resumen visible, se planifica OV en ese momento.

## 23. ADR necesario (M-08)

Un **ADR nuevo**, sucesor parcial de ADR-0043 en D5, D9, D24 y D25. El número se asigna al integrar, según WORKFLOW; no se reserva.
Fija:
- namespaces `rack` y `project`;
- regla de clave por namespace;
- caso `Computed`;
- resolución de `palabra.miembro`;
- disponibilidad por consumidor y reglas de ciclo R1..R6;
- persistencia cerrada (persistibles = `projectVariable`);
- fuente única por métrica y población cotizable.

El borrador está en el anexo A. Es un insumo para el Architect, no un ADR propuesto en `docs/adr`.

## 24. Riesgos y puntos que el Architect debe desafiar

| ID | Riesgo o decisión discutible | Postura de V1 |
|---|---|---|
| R-01 | Segunda autoridad de conteo o de cotización | E4 y E6 consumen las autoridades del BOM vigentes; `ConsolidatedBom` no es autoridad (D-10, P-08) |
| R-02 | Definición circular de «cotizable» | E1..E6 son anteriores al BOM y a las métricas (D-10) |
| R-03 | Deuda de `BomAuthoredAuthority` en kinds no selectivos | Se hereda solo para la pertenencia; declarada; no se arregla al pasar (EXP-08) |
| R-04 | `Rack.Frentes` en Φ3: una variable rota lo deja `Unavailable` aunque la estructura se conozca | Elegido por la definición del predicado sobre campos resueltos; alternativa *authored* descrita (§7) |
| R-05 | El adaptador del Plugin no tiene consumidor ni prueba en el host | Transporte delgado; compila en CI; su primer consumidor lo ejercitará con su OV |
| R-06 | Coste por rack de `RegistryEvaluation` dentro del resolver efectivo | Se mide en G4; no se cambia sin decisión |
| R-07 | Coordinación con I-64 | Alineada: `MASTER-I63-I64-02` (I-64 `1f1530be`) retira el *snapshot* y la dependencia; convergencia diferida (P-14) |
| R-08 | Nombre definitivo de `FrentesVacios` | Se fija en el Freeze |
| R-09 | `Project.*` no *bindable* en V1 | Evita la resolución de todo el proyecto y el ciclo rack → proyecto (R6) |
| R-10 | *Tokens* textuales en lugar de GUID para las claves | §5, alternativa rechazada |

Lista de desafíos de la orden PV1, con su sección:
- segunda autoridad: §10, R-01;
- «cotizable» circular: §10, R-02;
- BOM como autoridad: §§7 y 10;
- `Rack.Frentes` y `FrentesVacios`: §7, INV-04 e INV-05;
- semántica por sistema: §6;
- *built-ins* y documentos anteriores: §13, D-19;
- persistencia de namespaces: D-18;
- ciclos: §12;
- resolución completa y ansiosa: §§11 y 18, R6;
- *framework*: D-08;
- separación de conceptos: §4;
- duplicación con I-64: §16;
- RED→GREEN e I-61: §21;
- OV: §22.

## 25. Trazado

| Obligación del mandato («PROPOSAL / FREEZE MUST DEFINE») | Dónde |
|---|---|
| ComputedParameter identity; namespaces; scope | §5 (D-02..D-04) |
| Provider contract | §8 (D-08) |
| Metric source-of-truth | §7 (D-07) |
| Evaluation phases; expression availability | §11 (D-13, D-14) |
| Cycle/reentrancy contract | §12 (D-15) |
| System matrix; first metric set | §6 (D-05, D-06) |
| Project aggregation; RackId dedupe | §10 (D-10..D-12) |
| Unavailable/not-applicable semantics | §9 (D-09) |
| Summary neutral model | §14 (D-20) |
| Performance strategy | §18 (D-24) |
| ID23 extension point | §13 (D-17) |
| ID30 integration boundary | §16 (D-22) |

| Decisión de arquitectura del Discovery (§20.2) | Resuelta en |
|---|---|
| P-06 autoridad de hermanas | D-10 (E4) para la pertenencia; los valores V1 solo existen para el Selectivo, con su autoridad |
| P-07 estados por métrica | D-09 |
| P-08 relación con `ConsolidatedBom.Racks.Count` | §10, magnitudes distintas, sin migración |
| P-09 clave de `RacksBySystem` | *Token* de kind (D-12) |
| P-10 identidad de símbolos | D-02 |
| P-11 persistencia de referencias | D-18 (cerrada en V1) |
| P-12 fases por consumidor | D-13, D-14 |
| P-13 modelo neutral y unidades | D-20; V1 solo tiene conteos sin unidad |

| Criterio de éxito del mandato | Cómo se cumple |
|---|---|
| 1. Separación de ProjectVariable | D-01, D-23 |
| 2. Autoridad explícita | D-07, D-10 |
| 3. Deduplicación lógica | D-11, INV-01, INV-02 |
| 4. Primer conjunto en todos los sistemas | D-06 (estados explícitos por kind) |
| 5. *Built-ins* sin segundo motor | D-16, D-17 |
| 6. Sin recursión oculta | D-15, INV-10..13, INV-17 |
| 7. Estados explícitos | D-09 |
| 8. Resumen neutral | D-20 |
| 9. ID30 sin recalcular | D-20, D-22 |
| 10. ID23 consume `Rack.*` | D-17, INV-10 |

---

## Anexo A — Borrador del ADR (no propuesto todavía)

```text
Título: Parámetros calculados de solo lectura y namespaces built-in del motor de expresiones
Estado: borrador para la revisión del Architect (I-63 Proposal V1)
Sucede parcialmente a: ADR-0043 D5 (un solo namespace activo), D9 (tabla de tokens de namespace),
                      D24/D25 (ID20 reservado)

Contexto: I-49 reservó Rack.*, Project.*, el ámbito Rack y un caso de definición calculado para ID20.

Decisiones:
1. Namespaces: projectVariable (persistible), rack y project (activos en memoria, NO persistibles en
   este ADR). Palabras de sintaxis Rack y Project; tokens rack y project; ámbitos Rack y Project.
2. Clave por namespace: projectVariable conserva la regla A2 exacta. rack y project usan tokens ASCII
   lowerCamel de un catálogo cerrado, comparador Ordinal; nunca se derivan del nombre visible.
3. SymbolDefinitionKind.Computed: hoja sin expresión; jamás en un contexto de registro.
4. Binder: palabra.miembro resuelve contra las entradas del namespace ofrecidas por el contexto; si el
   contexto no ofrece el namespace, UnknownNamespace (comportamiento previo).
5. Disponibilidad: ningún símbolo calculado en definiciones de variables ni en fórmulas de propiedad;
   Rack.* solo en el contexto de rack calculado, posterior al resolve de ese rack; Project.* no bindable.
6. Persistencia: el conjunto de namespaces persistibles sigue siendo {projectVariable}. Persistir
   referencias calculadas exige un ADR posterior (ID23).
7. Fuente única por métrica, catálogo cerrado y población cotizable según I-63 Proposal/Freeze.

Consecuencias: sin cambio de formato persistido ni de comportamiento de los consumidores existentes;
ID23 obtiene una vía de evaluación por rack; ID28/ID29 obtienen provenance en memoria.
```
