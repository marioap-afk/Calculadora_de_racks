# I-63 — Paquete para el Architect, ronda 3 (re-revisión de la Proposal V3)

```text
Iniciativa: I-63 / ID20 — Computed Parameters & Project Summary Foundation (NEW ARCHITECTURE)
Revisor: el MISMO Architect de R1 y R2 (SEPARATE SESSION, sesión local_d5787742…, «I-63 Architect Review R1»).
         Solo él puede cerrar o rebajar sus REQUIRED (INITIATIVE_LIFECYCLE §5)
Objeto: docs/initiatives/I-63-proposal-v3.md, blob e4a94effa99f29e06b447a3ce07ec5bcc41f0b6d, en el commit que publica este paquete
        (SHA y CI en el informe al Coordinator)
Historia inmutable:
  V1  772ac242430084918d319a7683ce615daec39452  proposal blob 48a68307be3f046c20c2405252dc8af3ad050342  paquete v1 98e34b183c4c9829c18fec3cc822b342f6753e52
  V2  dddc215f08123b64adbf7fe35de6d3776a241501  proposal blob 9a84546fdccbe77593c78352521ab8bb926a3d96  paquete v2 09f874ddc91848f47ad221d48d22b33b439ccb16
Base de código: origin/main 819955d61a6da4c811a11fbd11b5dca13f634b7c
Estado: re-revisión R3 pedida, NO realizada. Freeze: NO. IMPLEMENTATION AUTHORIZATION = NO
```

## 1. Identidad de las revisiones

| Ronda | Objeto | Modo | Veredicto | Texto |
|---|---|---|---|---|
| R1 | V1 (`772ac242`, blob `48a68307`) | SEPARATE SESSION, sesión `local_d5787742…`; mismo modelo probable, como limitación de independencia | CHANGES REQUIRED: A63-PV1-01..10 | Paquete v2, anexo R1 (SHA-256 `a8ccab34…`) |
| R2 | V2 (`dddc215f`, blob `9a84546f`) | SEPARATE SESSION, **misma sesión** que R1; mismo modelo «unknown», como limitación probable | CHANGES REQUIRED: abiertos A63-PV1-05 y 08; nuevo A63-PV2-01 | Anexo R2 de este paquete (SHA-256 `8901e8b0…`) |

## 2. REQUIRED cerrados por el Architect en R2

**A63-PV1-01, 02, 03, 04, 06, 07, 09 y 10: CLOSED**, según el veredicto R2 (§2 del anexo R2). Los cerró **el Architect**; el autor no
los rebaja ni los reabre. La V3 conserva su contenido, salvo los cambios por A63-PV1-05, A63-PV1-08 y A63-PV2-01 (§4).

## 3. Disposición de los pendientes

Cada pendiente se corrigió con la **resolución vinculante** de la orden PV3 (decisiones de I-63 §2, «Orden PV3»).

| Pendiente | Lo que pidió el Architect en R2 | Corrección en V3 | Dónde |
|---|---|---|---|
| **A63-PV1-05 (a)** | Guarda de fuente con control positivo: el handler llama a `RackOutputVerdict` y a la correspondencia, y no contiene `PushBackResolver` ni `RackBomOutputGate.For`. El texto de `819955d6` no la pasa | La autoridad única es obligatoria para el handler, que pierde su composición propia. INV-33: **una** función `DelegatesToOutputVerdict` aplicada al método actual (debe pasar) y a un *fixture* con el texto literal de `819955d6` (debe rechazarlo) | D-27; INV-33; §21 |
| **A63-PV1-05 (b)** | Catálogo nulo: conservar en el handler la semántica actual (catálogo vacío) o declarar el cambio; añadir el caso a INV-13 | `CatalogInput = Loaded(c) \| LoadFailed`. El handler normaliza `null` → `Loaded(new RackCatalog())`, como hace hoy `PushBackResolver` (`PushBackResolver.cs:30-32`), y nunca produce `LoadFailed`. En I-63, `CatalogUnavailable` solo aparece con `LoadFailed`. El futuro productor no puede convertir un fallo de carga en un catálogo vacío (O-PV2-3) | D-27; D-25; INV-12; INV-13 |
| **A63-PV1-08 (a)** | Tabla total de resultados por rack, la misma para la petición por rack y para `RackSummary.Metrics`; `NotSupported`/`NotApplicable` ganan sin leer datos; INV con un Push Back ilegible y un Selectivo divergente | D-28: kind → soporte de diseño → E5 → E4 → efectivo/resuelto → `Available`. Pasos 1-6 con estado y razón; los pasos de soporte terminan sin leer ni resolver. INV-34 cubre Push Back ilegible (`NotSupported`, 0 lecturas), Cabecera (`NotApplicable`), Selectivo divergente (`Unavailable(SiblingsDivergent)`), Selectivo ilegible y Selectivo válido, por las dos vías | D-28; D-17; §14; INV-14, INV-34 |
| **A63-PV1-08 (b)** | La aserción «0 evaluaciones de población» no puede fallar en G1: pasarla a G2 contra el orquestador real, con control positivo | Retirada de G1 (INV-14 ahora solo cuenta resoluciones y lecturas). INV-32 en G2: un solo contador en el orquestador real; `RackMetricRequest` directo → 0; control con el mismo contador, una petición encaminada por `ProjectSummary` → > 0 | INV-14; INV-32; §21 |
| **A63-PV2-01** | INV-20 con un nombre `rack` con operador; INV-29 con un control que de verdad tenga la referencia prohibida; INV-09 con **una** función de guarda para objetivo y control | **INV-20:** entrada `rack` sintética `Frentes-Vacios`, sin variables con operador; la variante global da `OperatorInName`; un mismo *helper*. **INV-29 (b):** una función `ForbiddenReferenceFamilies` sobre el cierre de referencias declaradas; objetivo Application → ninguna; controles con la misma función: UI → WPF y Plugin → AutoCAD (AQ-06). **INV-09:** una función `ForbiddenProviderDependencies` aplicada a los *providers* reales y a un *fixture* inválido | INV-09, INV-20, INV-29; §20 (regla «misma función») |

**Opcionales de R2:**

| ID | Disposición |
|---|---|
| O-PV2-1 | Incorporado: el anexo A marca **V6 P25.4** como modificada y **P25.1** como descripción histórica desactualizada |
| O-PV2-2 | Incorporado como aclaración: `ProjectSummary.Racks` lleva todos los RackIds atribuibles, también `NotPlaced`, con su `Membership`, y sus métricas siguen D-28 (AQ-07) |
| O-PV2-3 | Incorporado: fallo de carga ≠ catálogo válido vacío; obligación del futuro productor (D-25; R-13) |
| O-PV2-4 | Incorporado: regla explícita de la asimetría E1/E3 de las hermanas sin colocar (§10) |
| O-PV2-5 | Incorporado como aclaración: guarda de conformidad del lector D-26 con los handlers (INV-35), con control positivo |

## 4. Delta V2 → V3

| Sección | Cambio |
|---|---|
| Cabecera e introducción | Identidad de V1 y V2; estado de R2; pendientes |
| §1, §19 (D-25) | Delegación del handler guardada; `CatalogInput` en el contrato de entrada; obligación del productor |
| §2 | Fila «Architect R2 + Orden PV3» |
| §3 | M-01 con unicidad guardada (INV-33); condiciones de M-03 (catálogo nulo, *fail-open*) |
| §8 (D-17) y nueva **D-28** | Precedencia total por rack; ejemplos con Push Back ilegible y Selectivo divergente; la lista de referencias no prioriza entre referencias |
| §10 (D-26, D-27) | Autoridad única obligatoria para el handler; `CatalogInput`; política de cada vía; asimetría E1/E3; guarda de conformidad D-26 |
| §14 | `Racks` con todos los RackIds atribuibles; métricas según D-28 |
| §20 | Regla «el control usa la misma función»; INV-09, 12, 13, 14, 20, 22 y 29 reescritos; nuevos INV-32..35 |
| §21, §22 | Asignación de INV a gates; cierre de la delegación sin «compila»; OV sujeta solo al Freeze |
| §24 | R-13 y R-14; AQ-01..05 respondidas en R2; nuevas AQ-06 y AQ-07 |
| §25 | Tabla de pendientes tras R2 |
| Anexo A | P25.4 modificada; P25.1 desactualizada |

## 5. Preguntas de la re-revisión R3

- **AQ-06:** INV-29 (b) usa las referencias **declaradas** en los `.csproj`, porque el job de Core no compila UI ni Plugin. ¿Es aceptable?
- **AQ-07:** `RackSummary.Metrics` se calcula para todos los RackIds atribuibles, también los `NotPlaced`, con el coste medido en G4. ¿Es
  aceptable, o el paso 5 de D-28 debe limitarse a los racks `Included`?

Material de entrada: el mismo de los paquetes v1 §2 y v2 §4, más las órdenes PV2 y PV3 en las decisiones de I-63 §2 y la evidencia §18.

## 6. Encargo listo para la sesión del Architect (R1/R2)

```text
I-63 — ARCHITECT RE-REVIEW R3 (Proposal V3)
Eres el mismo Architect de R1 y R2. Declara en la primera línea:
Review mode: SEPARATE SESSION; revisor=autor: no; misma sesión que R1/R2: <sí/no>.
Haz git fetch origin y lee SIN cambiar de rama ni escribir, con git show:
  <COMMIT_V3>:docs/initiatives/I-63-proposal-v3.md          (blob e4a94effa99f29e06b447a3ce07ec5bcc41f0b6d)
  <COMMIT_V3>:docs/initiatives/I-63-architect-package-v3.md (disposición, delta V2->V3 y tu veredicto R2 literal)
y el código y las autoridades de origin/main 819955d61a6da4c811a11fbd11b5dca13f634b7c.
Para A63-PV1-05, A63-PV1-08 y A63-PV2-01 declara CLOSED o STILL OPEN, con la razón. Los que cerraste en R2 siguen cerrados salvo
que encuentres un defecto material nuevo, que sería A63-PV3-nn.
Revisa la V3 completa; un hallazgo nuevo es A63-PV3-nn (REQUIRED|OPTIONAL), con evidencia y corrección mínima.
Responde AQ-06 y AQ-07. No implementes ni congeles.
Termina con AGREED POINTS, DISAGREEMENTS, MATERIAL RISKS, REQUIRED CHANGES, OPTIONAL IMPROVEMENTS y
CONSENSUS STATUS = AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION, ligado a <COMMIT_V3> / ruta / blob.
```

`<COMMIT_V3>` es el SHA del commit que publica este paquete; se informa al Coordinator junto con su CI.

## Anexo R2 — Veredicto del Architect R2, literal

**Procedencia:** texto final de la misma sesión revisora (`local_d5787742…`). Se extrajo de su transcripción local
(`fdd2a979-cce9-4391-83aa-e7dfe5effcc4.jsonl`, mensaje de 2026-10-01T23:20:04Z, modelo `claude-opus-5-5`). SHA-256 del texto en UTF-8:
`8901e8b0f034405833c7fb1dbc34985b0c8de269609a5597576fc2ccf889fe44`. No llegó como archivo. La orden PV3 del Coordinator confirma su
identidad (objeto, modo, revisor, veredicto, cerrados y abiertos). El veredicto R1 literal está en el anexo R1 del paquete v2.

```text
# I-63: re-revisión del Architect R2 sobre la Proposal V2

## 1. Review identity

| Campo | Valor |
|---|---|
| Modo | **SEPARATE SESSION** |
| Sesión revisora | La **misma sesión del Architect de R1**: `local_d5787742…`, «I-63 Architect Review R1» |
| ¿Es la sesión autora? | No |
| ¿Mismo modelo de base que la autora? | **unknown**. La sesión autora no aparece en el gestor de sesiones. La revisora usa `claude-opus-5-5`, y el paquete v2 declara la autora como «probable `claude-opus-5-5`». Lo mantengo como **limitación de independencia probable**, sin tratarlo como SAME-SESSION ROLE |
| Commit | `dddc215f08123b64adbf7fe35de6d3776a241501`. Es el único commit sobre `772ac242` y solo toca `docs/` |
| Proposal | `docs/initiatives/I-63-proposal-v2.md`, blob `9a84546fdccbe77593c78352521ab8bb926a3d96` (comprobado con `git rev-parse`) |
| Paquete | `docs/initiatives/I-63-architect-package-v2.md`, blob `09f874ddc91848f47ad221d48d22b33b439ccb16` (comprobado) |
| Base | `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, tras `git fetch` |
| Escrituras | Ninguna. Todo se leyó con `git show` |

El Anexo R1 del paquete reproduce mi veredicto R1 y la disposición de §2 del paquete describe bien lo que pedía. Aun así, he juzgado cada fila contra el texto de V2 y contra el código, no contra el resumen del paquete.

## 2. Disposition of prior REQUIRED

| ID | Disposición | Evidencia |
|---|---|---|
| **A63-PV1-01** | **CLOSED** | Lo que pedía, punto por punto: <br>• D-16.5: las referencias sin namespace solo buscan en `projectVariable`, y `Rack.<m>` solo en `rack`. <br>• D-16.4: índice por nombre y `OperatorNames` por namespace (corrige `SymbolTable.cs:140-148, :184`). <br>• D-03: miembro `Frentes` frente a referencia `Rack.Frentes`. <br>• `Rack.` sin miembro da `NameRequired` (AQ-01). <br>• INV-19 tiene exactamente el escenario pedido y **discrimina**: con un índice global, `{Frentes}` da `AmbiguousName` o enlaza con el calculado. <br>La debilidad del oráculo de INV-20 la trato aparte, en A63-PV2-01 |
| **A63-PV1-02** | **CLOSED** | D-16.6: `rack` presente → siempre `Rack.<miembro>`; ausente → `Rack.#{token}`, que no enlaza. Con el parser vigente, `Rack.` seguido de un cualificador da `NamespaceReferenceSyntax(member: null)` más un token suelto (`ExpressionSyntaxParser.cs:224-228`), así que nunca hay árbol. D-16.7 e INV-23 dejan `Classify` en `Expression`, que coincide con el `default` de `ExpressionFormatter.cs:62-72`. INV-21 cubre el *round-trip* con homónimo y discrimina: un índice global cualificaría `Frentes`. INV-22 discrimina por el texto formateado esperado (`Rack.#{zzz}` frente a `#{zzz}`) |
| **A63-PV1-03** | **CLOSED** | El Anexo A trae las tres listas, cláusula por cláusula. <br>• Se modifican D4, D5, D6 (regla 4, reglas 5-6 y Formatter), D9, D24, D25, P25.2 y P25.3. <br>• Se sustituye P25.5, con el motivo R1. <br>• Se conservan D1-D3, D7, D8, D10-D23, P25.6 y P26. <br>Comprobé que el texto coincide con ADR-0043 D4/D5/D6/D9/D24 y con V6 P25.2-P25.5 en `819955d6`. Una omisión menor, P25.4, queda como O-PV2-1 |
| **A63-PV1-04** | **CLOSED** | Opción (a), tal como la pedí: <br>• D-02 distingue `MetricId` de `SymbolId`, con correspondencia 1:1 solo para las dos ComputedParameter. <br>• `project` queda fuera del núcleo: sin miembro de enum, sin token y sin regla; `Project.X` sigue dando `UnknownNamespace`. <br>• `RacksBySystem` y los valores por sistema nunca son símbolos (§5, §6, D-12, R6). <br>• D-21 usa `MetricId` en el *provenance* |
| **A63-PV1-05** | **STILL OPEN** | El núcleo ya está corregido: D-27 define una función única de Application, la política de I-63 frente a la *fail-open* del handler está declarada, M-01 modificador y M-04 están clasificados, e INV-12 e INV-13 existen. Faltan dos puntos de mi corrección (2: «que la consuman el handler **sin cambiar su comportamiento**» y 4: conformidad del handler), detallados debajo de la tabla |
| **A63-PV1-06** | **CLOSED** | <br>• D-10a: proyección propia con cinco casos y comparador único `Ordinal`, sin pasar por `ProjectVariableScanProjection` (cuyo defecto está bien citado, `:37-47`). <br>• La entrada de E4 se construye solo con hermanas `Known`, coherentes y legibles, así que la proyección vigente nunca ve `KindAbsent` y su `OrdinalIgnoreCase` no puede discrepar. <br>• E5 es real para los seis kinds (D-26). Comprobé que reproduce las comprobaciones de presencia de los handlers: Dynamic con `DynamicDesign ?? DynamicSystem` (`DynamicKindHandler.cs:38-42`), Cantilever, Cabecera, Cama con `FlowBedConfigurationStore` y Selectivo. <br>• INV-06 tiene un caso por kind y discrimina la vía «E4 aprueba la primera vista» |
| **A63-PV1-07** | **CLOSED** | D-11, puntos 1-7: <br>• hermanas = todas las del RackId, colocadas o no, como en `BomTotal.cs:96-104`; <br>• orden canónico por `DefinitionKey` con `Ordinal`, antes de E3-E6; <br>• grafía canónica = la mínima en `Ordinal`; <br>• el representante y `DisplayName` se toman tras ordenar. <br>INV-02 e INV-11 discriminan reordenando la entrada; INV-11 incluye la hermana no colocada y divergente |
| **A63-PV1-08** | **STILL OPEN** | Está bien resuelto lo esencial: `RackMetricRequest` existe (D-17), no recorre el proyecto, la propagación tipada `ComputedReferencesNotAvailable` conserva los tres estados distintos de `BrokenReference`, y los ejemplos de Push Back y Cabecera están congelados. Siguen abiertos dos elementos de mi corrección, detallados debajo de la tabla |
| **A63-PV1-09** | **CLOSED** | Opción (a): no hay adaptador del Plugin (§1, §19). El contrato Φ0 `ProjectCaptureSnapshot` lo construyen las pruebas. §21 dice que ningún gate cierra por compilar, y cada uno de G1-G4 tiene un resultado conductual con INV. La única edición del Plugin, la delegación de D-27, queda bajo A63-PV1-05 |
| **A63-PV1-10** | **CLOSED** | D-12 y D-20: seis *tokens* siempre, en orden fijo. Un sistema ausente da `rackCount` = `Available(0)`. Hay estados explícitos `NotSupported` y `NotApplicable` por métrica, «nada se infiere de `null`», y Σ `rackCount` = `totalRacks`. INV-07 discrimina el mapa que solo lleva los sistemas presentes |

**Lo que falta en A63-PV1-05:**

1. **No hay ninguna prueba de que el handler use la función.** INV-13 prueba una función de correspondencia en Application, pero no que `PushBackKindHandler.OutputBlockedReason` la llame. §21 afirma que la línea del handler «no se valida porque compila», y sin embargo ninguna prueba falla si el handler conserva su `PushBackResolver` propio. Seguirían existiendo dos composiciones, que es justo M-01.
   - El repositorio ya tiene precedente de guardas de fuente sobre el Plugin leído como texto (`tests/RackCad.Tests/CantileverPluginSourceGuardTests.cs:10-18`, ADR-0003).
2. **«Sin cambio de comportamiento» no es exacto con catálogo nulo.**
   - Hoy, `new PushBackResolver(null)` **no falla**: usa `catalog ?? new RackCatalog()` (`PushBackResolver.cs:30-32`), resuelve y puede dar `Deny(m)`.
   - D-27 haría que un catálogo nulo diera `Undetermined(CatalogUnavailable)`, que el handler convertiría en `null`. Un rack que hoy se bloquea dejaría de bloquearse.
   - Por RACKBOMTOTAL no se observa, porque `RackCatalogLoader.Load` nunca devuelve `null` (`RackCatalogLoader.cs:24-29` devuelve un catálogo vacío). Pero D-27 e INV-13 afirman una conservación que ese caso contradice.

**Corrección mínima restante para A63-PV1-05:**

- (a) Añadir un INV de guarda de fuente con control positivo: el texto de `PushBackKindHandler.OutputBlockedReason` llama a `RackOutputVerdict` y a la correspondencia del handler, y ya no contiene `PushBackResolver` ni `RackBomOutputGate.For`. El control es que el texto vigente de `819955d6` no pasa la guarda.
- (b) Para el catálogo nulo, elegir una de dos: conservar en la vía del handler la semántica actual (catálogo vacío) y dejar el `CatalogUnavailable` solo para I-63, o declarar el cambio con su clasificación M. En ambos casos, añadir el caso a INV-13.

**Lo que falta en A63-PV1-08:**

1. **Precedencia de los estados en la petición por rack.** D-17.3 solo trata los fallos de E3. No dice qué devuelven E5 (diseño ilegible) y E4 (divergencia), ni en qué orden frente a `NotSupported` y `NotApplicable`.
   - Ejemplo: un Push Back con diseño ilegible. ¿Devuelve `NotSupported` (D-06 dice «sin leer datos») o `Unavailable(DesignUnreadable)` (D-17.2 aplica E5 antes)?
   - Lo mismo pasa con `RackSummary.Metrics` de un rack `Excluded(SiblingsDivergent)`.
   - Es el contrato congelado que consumirá ID23, y hoy admite dos respuestas.
2. **El contador de población de INV-14 está vacío en G1.** «0 evaluaciones de población» se comprueba en G1, pero la población no existe hasta G2. La aserción no puede fallar, y es exactamente la guarda tautológica que el punto 10 del encargo pide rechazar.

**Corrección mínima restante para A63-PV1-08:**

- (a) Congelar una tabla total de resultados por rack: kind (E3) → soporte de diseño (D-06) → E5 → E4 → efectivo/resuelto. Para cada combinación, fijar el estado y la razón de cada `(Rack, *)`, y que la petición por rack y `RackSummary.Metrics` usen la misma tabla. Recomiendo que `NotSupported` y `NotApplicable` ganen sin leer datos, coherente con D-06. Añadir un INV con un Push Back ilegible y un Selectivo divergente.
- (b) Pasar la aserción «0 evaluaciones de población» a G2, o repetirla allí, contra el orquestador real, con un control positivo: una petición encaminada a propósito por el resumen hace que el contador dé más de 0.

## 3. Agreed points

1. **Namespace `rack`** (D-16.4-5): las referencias sin namespace siguen siendo `projectVariable`, `Rack.X` solo resuelve `rack`, y los homónimos, `OperatorInName` y el formatter van por namespace. Fuera del contexto de rack calculado, la semántica es la de hoy (D-19).
2. **Formatter:** `Rack.<miembro>`, la forma diagnóstica `Rack.#{token}` que no enlaza, `Classify` = `Expression`, y canonicidad P2.5 extendida y probada (INV-21).
3. **Persistencia con dos tablas** (D-18): la tabla de tokens de D9 queda intacta. INV-24 discrimina la variante de una sola tabla (`PersistedBoundExpressionJson.cs:95`). M-02 no se activa.
4. **`project`** queda fuera del motor V1, `MetricId` es distinto de `SymbolId`, y nada no escalar es símbolo.
5. **Cotizabilidad:** la proyección es tipada, E5 es real, E3 se declara regla propia de I-63, no hay circularidad y la tabla comparativa con RACKBOMTOTAL es honesta (§10).
6. **Determinismo** (D-11, D-20, D-21): hermanas colocadas y no colocadas, orden canónico, grafía, representante, `DisplayName` e identificadores de autoridad cerrados (INV-31).
7. **`ProjectSummary`:** seis sistemas, estados explícitos, `null` nunca es un estado, y coherencia de `rackCount` con `totalRacks`.
8. **`Population` y `Full`** como tipos distintos (O-04, AQ-05).
9. **Ciclos R1-R6.** La sustitución de P25.5 está bien motivada: el ciclo `VerticalClearance` hace que ofrecer `Rack.*` en las fórmulas de propiedad sea inseguro.
10. **Frontera con I-64** (D-22): sin dependencia obligatoria, sin *snapshot* común y sin contrato compartido. `MASTER-I63-I64-02` se cita como hecho leído, no como decisión recibida.
11. **Controles positivos que sí discriminan:** INV-18 (la misma expresión enlaza en la tabla sintética y no en el contexto de propiedad), INV-24, INV-19, INV-21, INV-06, INV-07, INV-11 e INV-02. INV-28 es honesto como no regresión.

## 4. Disagreements

- **La delegación del handler se declara verificada sin una prueba que lo demuestre.** Además, «sin cambio de comportamiento» es falso con catálogo nulo (A63-PV1-05).
- **El resultado por rack no es total:** falta la precedencia de E5, E4 y el soporte de diseño (A63-PV1-08).
- **Dos guardas no pueden fallar con la variante incorrecta:** INV-20 y la parte de neutralidad de INV-29 (A63-PV2-01). La de INV-14 la cubre A63-PV1-08.

## 5. Material risks

| Riesgo | Sobre qué |
|---|---|
| MR-R2-1 | El handler conserva su propia composición y existen dos autoridades de la puerta sin que ninguna prueba falle |
| MR-R2-2 | ID23 recibe estados distintos para el mismo rack según el orden en que se implemente la precedencia |
| MR-R2-3 | Las guardas tautológicas dan una falsa sensación de RED→GREEN en G1 y G3, y eso contradice el requisito O-07 |
| MR-R2-4 | El futuro adaptador puede pasar un catálogo vacío en lugar de nulo tras un fallo de carga. `CatalogUnavailable` no se activaría nunca y la puerta denegaría en silencio (O-PV2-3) |

## 6. New required changes

### A63-PV2-01 — Guardas cuyo control no discrimina (O-07)

- **Elemento:** §20, INV-20 e INV-29 (sub-aserción de neutralidad). Afecta también a la precisión de INV-09.
- **Clasificación:** REQUIRED.
- **Razón:**
  - **INV-20:** el escenario dice «variable `A-B` y miembro `rack` **cualquiera**». D-03 obliga a que los miembros `rack` sean palabras simples, así que no contienen operadores. La variante incorrecta, con patrones calculados sobre todas las entradas, también pasa. La guarda es tautológica.
  - **INV-29:** «el ensamblado de Application no referencia WPF ni AutoCAD (control positivo con un tipo de prueba)». Un tipo de prueba no puede crear una referencia de ensamblado en Application, así que ese control no ejercita la guarda.
  - **INV-09:** el control solo funciona si la **misma** función de guarda se aplica tanto a Application como al ensamblado o tipo de prueba. V2 no lo fija.
- **Evidencia:**
  - Proposal V2, filas INV-20, INV-29 e INV-09 y la regla de §20 «Ninguna guarda es tautológica»;
  - D-03 («palabra simple»);
  - `SymbolTable.cs:148` (`OperatorNames` se construye a partir de los nombres);
  - LIFECYCLE §5, «oráculos ciegos».
- **Consecuencia:** obligaciones invariante→prueba sin RED real. Eso es un REQUIRED según LIFECYCLE §5.
- **Corrección mínima:**
  - **INV-20:** usar una entrada `rack` **sintética** con un nombre que contenga un operador (el núcleo no valida nombres, `SymbolEntry`). Por ejemplo `Frentes-Vacios`, sin ninguna variable con operador. Afirmar que `Frentes-Vacios` sin llaves **no** da `OperatorInName`. La variante global sí lo daría.
  - **INV-29:** el control debe ser un ensamblado que de verdad tenga la referencia prohibida. Puede ser un ensamblado *fixture* o la lectura de metadatos de uno conocido. Alternativa: declarar esa sub-aserción como no regresión, si ya existe una guarda equivalente.
  - **INV-09:** fijar que la guarda es **una** función parametrizada por ensamblado o espacio de nombres, aplicada a los dos objetivos.

## 7. Optional improvements

- **O-PV2-1:** añadir al Anexo A que V6 P25.4 queda **modificada** («sin crear ningún símbolo `Rack` productivo» deja de ser cierto) y que P25.1 queda desactualizada en lo descriptivo.
- **O-PV2-2:** fijar qué grupos entran en `ProjectSummary.Racks`. Recomiendo todos los RackIds atribuibles, también los `NotPlaced`, con su `Membership`.
- **O-PV2-3:** en D-25, obligar al futuro productor del *snapshot* a pasar `null` cuando falle la carga del catálogo. `RackCatalogLoader.Load` devuelve un catálogo vacío ante un error (`RackCatalogLoader.cs:24-29`).
- **O-PV2-4:** declarar como regla explícita la asimetría de E1/E3: una hermana sin colocar con `EnvelopeUnreadable` se ignora, pero una sin colocar con `KindAbsent(Id)` participa en E3 y puede dejar `totalRacks` en `Unavailable`.
- **O-PV2-5 (AQ-03):** una lista de conformidad que relacione cada fila de D-26 con la línea del handler que reproduce, mantenida por una guarda de fuente. Así R-11 no deriva sin que nadie lo vea.

## 8. AQ-01..AQ-05 responses

- **AQ-01:** **acepto `NameRequired`** para `Rack.` sin miembro en un contexto que ofrece `rack`. `UnknownNamespace` diría algo falso: el namespace sí existe ahí. Fuera de esos contextos debe seguir dando `UnknownNamespace`. El lapso es el de toda la referencia.
- **AQ-02:** **es suficiente para el Freeze**, siempre que INV-22 congele las propiedades: el texto exacto formateado `Rack.#{token}`, ningún árbol enlazado y diagnósticos deterministas. El código concreto se fija en el commit RED de G3 y se registra con una A-n solo del Coordinator, que añade una prueba sobre un comportamiento ya congelado (LIFECYCLE §6).
- **AQ-03:** **aceptable en V1** que los handlers no adopten el lector D-26. Hacerlo tocaría la vía del BOM, lo que supone más ediciones del Plugin y riesgo de M-03. Queda como deuda declarada (R-11), con O-PV2-5 recomendado.
- **AQ-04:** **sí**: es M-04 sin M-03. La política nueva solo existe en la superficie nueva de I-63, y RACKBOMTOTAL no cambia. Pero esa clasificación solo se sostiene cuando se cierre A63-PV1-05: la guarda de delegación y la equivalencia con catálogo nulo.
- **AQ-05:** **sí**: los tipos distintos bastan, y un estado `NotRequested` sería peor, porque metería una ausencia disfrazada de estado.

## 9. Materiality / archetype assessment

| ID | V2 | Evaluación |
|---|---|---|
| M-01 | Creador y modificador (D-27) | **De acuerdo**. La modificación solo es efectiva y única con A63-PV1-05 (a) |
| M-02 | No activado | **De acuerdo** (dos tablas, INV-24) |
| M-03 | No activado | **De acuerdo**, condicionado a A63-PV1-05 (b) |
| M-04 | Activado | De acuerdo |
| M-05 | Activado | De acuerdo: alcance completo en D-16 |
| M-06 / M-07 | Activados | De acuerdo: registro cerrado, sin *framework* |
| M-08 | Activado | De acuerdo: delta completo, salvo O-PV2-1 |

**Arquetipo:** NEW ARCHITECTURE, confirmado.

**OV:** **confirmo NOT APPLICABLE como candidata.** No hay UI, comandos ni adaptador del host, y la delegación del handler no tiene efecto observable una vez se cierre A63-PV1-05 (b). La decisión final es del Coordinator en el Freeze.

**I-64:** sin dependencia obligatoria, sin *snapshot* común y sin contrato compartido. **De acuerdo.**

## 10. Freeze readiness

**NOT READY FOR FREEZE.** Este veredicto no crea ningún Freeze.

## 11. Consensus status

**CHANGES REQUIRED.** Quedan abiertos:

- **A63-PV1-05**, solo en los puntos (a) y (b) descritos;
- **A63-PV1-08**, solo en los puntos (a) y (b) descritos;
- **A63-PV2-01**.

Están cerrados A63-PV1-01, 02, 03, 04, 06, 07, 09 y 10. Nada de lo pendiente requiere una decisión del Owner, y los tres puntos son correcciones localizadas.

El veredicto vale **exclusivamente** para:

- commit `dddc215f08123b64adbf7fe35de6d3776a241501`;
- ruta `docs/initiatives/I-63-proposal-v2.md`;
- blob `9a84546fdccbe77593c78352521ab8bb926a3d96`.
```
