# I-49 — Proposal V2: motor de expresiones paramétricas (Expression Engine, ID22B)

> # ⚠ COORDINATOR PROPOSAL V2 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Coordinator    = PROPOSED (V2)
> Architect      = PENDING — Architect Review READY, todavía NO solicitada
> Implementation = BLOCKED
> ADR            = REQUIRED (número NO asignado)
> Proposal V1    = b15e40a — conservada como historial; NO aceptada para Architect Review
> ```
>
> Gate **G2A.1**. V2 conserva lo válido de [V1](I-49-proposal-v1.md) y corrige **cuatro desviaciones
> materiales** respecto del contrato original del Owner y de la Proposal del Coordinador (§0). Son
> **correcciones del Coordinador**, no hallazgos del Arquitecto: la revisión del Arquitecto no se ha
> solicitado ni sobre V1 ni sobre V2. Siguen abiertas **OPEN FOR ARCHITECT A** (§4) y
> **OPEN FOR ARCHITECT B** (§5).
>
> ```text
> Discovery G1     = 4cf02b167f183fb66d93988c9843296838277dc4   (I-49-discovery.md)
> Proposal V1      = b15e40a076d7157ce8ca73537af9659049bf8576   (I-49-proposal-v1.md; CI 34719931193 4/4; GREEN por instrucción del Owner)
> Código auditado  = a4d88f18a1f42263d366c44dc05dd18a6786f152   (origin/main; la rama no contiene producción)
> Contrato         = I-49-motor-expresiones-parametricas.md
> Override vigente = OWNER_OVERRIDE_I49_I50_PARALLEL   (docs/automation/decisions/I-49.md)
> I-50 observado   = 9b592e934e9293612de255b543f81943933bb6bd   (solo docs/; Proposal V1.2 congelada, ADR-0035 aceptado, G4 no iniciado)
> I-51 observado   = f8cf4c9f2024f3494d977f4bfbd2c2f9816b8c73   (G4: producción en Plugin — restamp y RACKDUPLICAR)
> Estado de gates  = G0 HECHA · G1 HECHA · G2A V1 (historial) · G2A.1 V2 EN CURSO · G3 PENDIENTE
> ```

---

## 0. V1 → V2 reconciliation

### 0.1 Naturaleza de las correcciones

Las cuatro correcciones de esta sección son del **Coordinador**, contra el **contrato del Owner** para I-49 y contra
la **Proposal del Coordinador** que V1 debía desarrollar. **No** son hallazgos del Arquitecto: su revisión no
se ha solicitado. V1 queda intacta en `b15e40a` como historial y **no** se somete a revisión.

### 0.2 Los cuatro BLOCKER

| # | Desviación de V1 | Por qué es material | Corrección en V2 | Dónde |
|---|---|---|---|---|
| **BLOCKER 1** — Property expressions / `LinkedPropertyEditor` | DR-1 de V1: «expresiones **solo** en `ProjectVariable.Definition`»; P23.2 de V1 prohibía expresiones en el campo y P30.1 de V1 las declaraba fuera de alcance | El contrato del Owner incluye **dos** superficies productivas de expresión. V1 eliminó una para acomodarse a ADR-0034 §15 | **Dos superficies**: definición de variable (`AlturaFinal = AlturaBase + Holgura`) y el **mismo** control de I-48 (`6`, `=Holgura`, `=Holgura + 2`, `=(AlturaBase + Holgura) / 2`), sin control paralelo. Fuente de propiedad **híbrida** `Literal \| ProjectVariableReference(VariableId) \| Expression(BoundExpression)`; la referencia directa **se conserva**; `=X` se **canoniza** como la referencia existente. Pipeline explícito parse → bind → validate → evaluate → intent → mutación atómica. La extensión de ADR-0034 §15 queda **asignada al ADR nuevo** (§8), sin retirar el requisito | DR-1, P2.7–P2.8, P17.3, P23, P24, §8 |
| **BLOCKER 2** — Unidades opcionales | P1.6 y P9.2 de V1 rechazaban cualquier sufijo; P30.6 de V1 los declaraba fuera de alcance | El contrato del Owner incluye `[mm]`, `[in]` y `[ft]` | Sufijos `[mm]`, `[in]` y `[ft]` sobre literales numéricos, como **conversión numérica** a pulgadas (`100[mm]` → 100 ÷ 25.4; `4[in]` → 4; `20[ft]` → 240). **Una sola** autoridad neutral de conversión, un solo lexer y cero dependencia de `StructuralSections` | P1.6, P9, P17.4, P28.4 |
| **BLOCKER 3** — Motor adimensional | P5.4–P5.5 de V1 introducían `ExpressionValueType = Length \| Scalar`, `DimensionMismatch`, `ResultTypeMismatch` y reglas `Length × Length` y `Length ÷ Length` | El contrato del Owner pide un motor **escalar y adimensional**, en el que las unidades son solo conversiones | Motor sobre `double` finito, **adimensional por defecto**. Se **eliminan** el tipo interno, `DimensionMismatch`, `ResultTypeMismatch` y toda regla dimensional. `6 + 2`, `A + 2`, `A * B`, `A / B` y `100[mm] + 2` son válidas. `VariableType.Length` **sigue existiendo** como metadata y contrato del consumidor, no como álgebra | DR-5, P5.4–P5.6, P8.8, P15.3, P16 |
| **BLOCKER 4** — Conjunto de funciones | P10.3 de V1 proponía solo `min` y `max` | El contrato del Owner pide como mínimo `MIN`, `MAX` y `ABS`, y la evaluación documentada de `ROUND`, `CEILING` y `FLOOR` | `MIN`, `MAX` y `ABS` en el conjunto inicial; `ROUND`, `CEILING` y `FLOOR` **evaluados y diferidos**, con las preguntas que los bloquean; `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings, trigonometría y macros **excluidos** salvo nueva razón aprobada | P10, P30 |

### 0.3 Cambios derivados de las cuatro correcciones

No son BLOCKER nuevos, pero V2 no sería coherente sin ellos:

- **Versión de schema re-justificada.** V1 fijó `1.1` para el registro **sin** contrastarlo con la política real.
  V2 analiza registro y diseño Selectivo contra esa política, propone **no cambiar ninguna versión**
  (alternativa V-0) y deja la decisión en **revisión explícita del Arquitecto** por duda material (P18, §6).
- **Contrato del editor de I-48.** Aceptar expresiones en el mismo campo **revisa** comportamientos fijados por los
  pines G20 (Discovery §14.3). V2 enumera qué se conserva y qué cambia (P23.12), y lo asigna al ADR (§8).
- **Consumidores de rack.** Una variable ya no solo la consumen referencias directas: también las expresiones
  de propiedad que la nombran. Afecta a descubrimiento, `Delete`, `UnlinkAllAndDelete` y propagación (P20, P21).
- **Inspección de bindings.** `InspectBinding` pasa a interpretar también `expression` (P24.6).
- **Canonicalización.** Una forma persistida por significado, en las dos superficies (P2.8).

### 0.4 Qué conserva V2 de V1

Identidad por id en todo lo persistido; núcleo neutral `RackCad.Application.Expressions`; gramática aritmética con
la regla de la coma; tres modelos (sintaxis, `BoundExpression`, resultado); binder como única ruta nombre→id;
grafo derivado y no persistido; detección iterativa de ciclos; orden determinista; catálogo cerrado de
diagnósticos (actualizado); `Rename` registry-only; `Delete` bloqueado por dependientes; cierre transitivo en un
plan; re-evaluación en commit; UX de RACKVARIABLES (ampliada); puntos de extensión ID20, ID23 e ID28/ID29; evolución
explícita de guardas; OPEN A y OPEN B; gates G0–G12; ADR REQUIRED sin número.

---

## 1. Cómo leer esta Proposal

### 1.1 Qué es y qué no es

- **Es** la Proposal V2 del Coordinador y la versión que queda **lista** para Architect Review. Cada regla está
  **propuesta**; ninguna está decidida.
- **No es** consenso, **no** autoriza implementación, **no** es un ADR, **no** modifica V1 y **no** modifica el
  Discovery. La compuerta **NO IMPLEMENTATION BEFORE CONSENSUS** del contrato (§12) sigue vigente.
- **Parte de hechos.** Toda afirmación sobre el código vigente remite al [Discovery G1](I-49-discovery.md) o a una
  ruta verificada en `a4d88f1`. Lo que era INFERENCE sigue siéndolo.

### 1.2 Convenciones

| Marca | Significado |
|---|---|
| **Pn.m** | Regla propuesta *m* del punto *n* de §3. Solo sería vinculante tras G3 |
| **DR-n** | Decisión rectora propuesta (§2) |
| **Discovery §x** | Hecho verificado en G1; remite a la sección *x* de [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1` |
| **REQUISITO DEL OWNER** | Requisito del contrato del Owner, tal como lo fija el encargo de G2A.1. No se negocia en la Proposal |
| **OWNER INPUT** | Aclaración del Owner recibida en G1 (ID20, ID23, ID28/ID29) |
| **OPEN FOR ARCHITECT** | Cuestión sin decidir a propósito (§4, §5). Sus requisitos están congelados; su forma exacta no |
| **ARCHITECT REVIEW REQUIRED** | Decisión propuesta con duda material que el Arquitecto debe confirmar o cambiar (§6) |
| **INFERENCE** | Estimación o deducción, no hecho |
| Nombres de tipos y miembros | **Ilustrativos**: el contrato es el comportamiento |

### 1.3 Entradas vinculantes

- **Contrato de I-49** — [`I-49-motor-expresiones-parametricas.md`](I-49-motor-expresiones-parametricas.md): las
  invariantes de I-47/I-48 no se reabren sin ADR y decisión del Owner (§3.2); quedan fuera Custom BOM, ID20
  productivo e ID21 (§4). Su §11.2 sitúa la capacidad en `ProjectVariable.Definition` «salvo ADR nuevo aceptado
  por el Owner»: con BLOCKER 1, ese ADR es obligatorio (§8) y la alineación del texto del contrato queda para G4.
- **[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)**, aceptado e inmutable. Su §15 separa
  `ProjectVariable.Definition` (ID22B) de `PropertyValue<T>` (ID21). V2 **extiende** la fuente de propiedad con un
  caso `Expression` y lo declara como decisión que el ADR nuevo tiene que formalizar (§2.3, §8).
- **REQUISITOS DEL OWNER (encargo de G2A.1)**: dos superficies de expresión; mismo control de I-48; fuente
  híbrida con canonicalización de `=X`; unidades `[mm]`, `[in]`, `[ft]` con una autoridad neutral; motor
  adimensional; `MIN`, `MAX`, `ABS` y evaluación de `ROUND`, `CEILING`, `FLOOR`; OPEN A y OPEN B mantenidas;
  propagación en un plan; núcleo neutral; reserva conceptual de `Rack.*` y `Project.*`; preservación para
  ID28/ID29; ADR REQUIRED sin número.
- **OWNER INPUT de G1** (Discovery §1): ID20, ID23 e ID28/ID29, sin cambio respecto de V1.
- **`OWNER_OVERRIDE_I49_I50_PARALLEL`** — [`decisions/I-49.md`](../automation/decisions/I-49.md), con su procedimiento
  de colisión (§8 de ese registro).

---

## 2. Decisiones rectoras y arquitectura en una página

### 2.1 Decisiones rectoras propuestas

- **DR-1 — Dos superficies productivas de expresión** (REQUISITO DEL OWNER):
  - **A. Definición de variable de proyecto**: `literal | expression`. Ejemplo: `AlturaFinal = AlturaBase + Holgura`.
  - **B. Fuente de una propiedad vinculable**, escrita en el **mismo** `LinkedPropertyEditor` de I-48:
    `Literal | ProjectVariableReference(VariableId) | Expression(BoundExpression)`. Acepta `6`, `=Holgura`,
    `=Holgura + 2` y `=(AlturaBase + Holgura) / 2`.

  La referencia directa **no se elimina**, y `=X` tiene **una sola** representación persistida: la referencia
  existente (P2.8). ID21 (referencias rack→rack) sigue fuera.
- **DR-2 — Núcleo neutral.** Parser, binder, evaluador, grafo y formatter viven en `RackCad.Application.Expressions`,
  puro y **sin dependencia** de Project Variables, persistencia, sistemas, BOM, catálogos, `StructuralSections`,
  Domain, UI, Plugin ni AutoCAD. Project Variables **consume** el motor; nunca al revés. Las conversiones de
  unidades vienen de **una** autoridad neutral de Application (P9.5), igual de independiente.
- **DR-3 — Evaluación en Application, antes de que geometría y BOM lean nada.** Las variables se evalúan **una
  vez** por snapshot acreditado, con memoización. Las expresiones de propiedad se evalúan dentro del **resolver
  único**, al resolver cada rack, sobre los valores ya evaluados de ese snapshot. Geometría, BOM y preview siguen
  consumiendo **solo el efectivo** (ADR-0034 §6; guarda G11, Discovery §14.3).
- **DR-4 — Identidad por id y una forma canónica por significado.** Todo lo persistido lleva `VariableId`, nunca
  nombres. El nombre solo interviene al **escribir** (binder) y al **mostrar** (formatter).
- **DR-5 — Motor adimensional** (REQUISITO DEL OWNER): valores `double` finitos, sin sistema de dimensiones
  físicas. Las unidades explícitas son **solo** conversiones numéricas a pulgadas. `VariableType.Length` es
  metadata y contrato del **consumidor**, no una regla del evaluador.
- **DR-6 — Fail-closed tipado en toda la cadena** (ADR-0034 §8): cada fallo es un resultado tipado, un plan fallido
  es vacío, y no existe fallback a literal, a cero ni a un valor anterior.

### 2.2 Tuberías propuestas

```text
Lectura y consumo
NOD RACKCAD_PROJECT (Plugin)          único lector/escritor físico                         (sin cambio)
  → ProjectVariablesStore             legibilidad ESTRUCTURAL de definiciones              (P17)
  → UsableProjectVariablesRegistry    identidad: VariableId único                          (sin cambio)
  → Evaluación del registro           tabla de símbolos, grafo, ciclos, orden y valores    (P6–P16)
  → SelectiveEffectiveDesignResolver  resolver único: referencias directas → valor
                                      evaluado; expresiones de propiedad → evaluadas aquí  (P14.8, P24)
  → geometría / BOM / preview         solo efectivo                                        (sin cambio)
```

```text
Escritura explícita (las dos superficies)
texto tecleado → parse → bind → validate → evaluate → canonicalizar → intent → mutación atómica
                 (P1)    (P7)   (P7.5)     (P14)      (P2.8)          (P22,   (P21; reconciliador
                                                                        P23)   de I-48, P23.15)
```

```text
RackCad.Application.Expressions  (núcleo neutral, DR-2)
  texto          → lexer/parser             → sintaxis con posiciones                  (P1, P2)
  sintaxis       → binder + SymbolResolver  → BoundExpression (ids, funciones, unidades) (P7, P9, P10)
  BoundExpression→ comprobación semántica   → existencia, aridad, ámbito                (P7.5)
  BoundExpression→ dependencias → grafo → ciclos → orden                                (P11–P14)
  orden + tabla  → evaluador (double)       → EvaluationResult + traza opcional         (P14, P16)
  BoundExpression→ formatter                → texto editable                            (P4)
```

### 2.3 Lo que V2 extiende o revisa

V2 **no retira** ninguna garantía de I-47/I-48 que el Owner no haya pedido cambiar. Lo que extiende o revisa,
por eso, exige ADR (§8):

| Invariante o contrato vigente | Extensión o revisión propuesta | Punto |
|---|---|---|
| ADR-0034 §3: definición **literal** | Definición `literal` **o** `expression`; `VariableType` sigue siendo solo `Length` | P2.6, P5.5 |
| ADR-0034 §4: una propiedad es `Literal(T)` o `ProjectVariableReference(VariableId)` | **Tercer caso** `Expression(BoundExpression)` en la fuente de propiedad | P2.7, P17.3, P24 |
| ADR-0034 §15: ID22B extiende `Definition`; `PropertyValue<T>` queda para ID21 | ID22B extiende **también** la fuente de propiedad; ID21 sigue fuera y su caso futuro no se reserva aquí | §8 |
| ADR-0034 §7: propagación de **profundidad 1** | Cierre transitivo; **un** plan, **una** transacción, **un** commit y **un** `Regen` | P21 |
| ADR-0034 §11: `Delete` bloqueado **con consumidores** | Bloqueado también con **dependientes**; los consumidores incluyen expresiones de propiedad | P20 |
| ADR-0034 §13, C4-8 y C4-9: versiones y kinds desconocidos | Kinds nuevos `expression`, fail-closed en builds anteriores; **sin cambio de versión** propuesto, bajo ARCHITECT REVIEW REQUIRED | P18, §6 |
| Decisiones de I-48 §4 (V8-R01): re-acreditación en commit | La re-lectura de commit **re-evalúa** el cierre impactado | P21.6 |
| Contrato del editor de I-48 (HANDOFF §1; pines G20) | `=` deja de ser solo consulta de referencia; se conservan borrador ≠ comprometido, Escape, LostFocus, C4, no auto-selección y desambiguador | P23.12 |
| Contrato de I-49 §1, §11.2 y §12 | La capacidad sale de `Definition` por decisión del Owner; el ADR es la vía que el propio contrato prevé | §8, §12 |

**No se toca**: autoridad y nodo del registro, identidad `VariableId`, authored ≠ effective, congelado del literal
comprometido (20.13), autoridad multi-vista, `RepairBrokenRack` rack-scoped, identidad ambigua fail-closed, mapping
único del `Type` y los censos de comandos y ventanas.

---

## 3. Los 30 puntos

### 3.0 Tabla de control

| # | Punto | Propuesta en una línea | Estado |
|---|---|---|---|
| 1 | grammar | Aritmética, paréntesis, llamadas, números invariantes, sufijos `[mm]`/`[in]`/`[ft]`, `,` solo separa argumentos | PROPOSED · producción `reference` → **OPEN A** |
| 2 | AST/model | Sintaxis, `BoundExpression` persistible y resultado; formas canónicas por superficie | PROPOSED |
| 3 | identity | `VariableId` intacto; `SymbolId = (namespace, key)`; las expresiones no tienen identidad propia | PROPOSED |
| 4 | source text | Texto tecleado ≠ árbol persistido ≠ texto del formatter; un formatter para las dos superficies | PROPOSED · referencias → **OPEN A** |
| 5 | symbol model | Namespace `projectVariable`, ámbito `Project`, dominio numérico adimensional | PROPOSED |
| 6 | ExpressionContext | Valor inmutable por operación desde UN snapshot; contexto de evaluación por propiedad de rack | PROPOSED |
| 7 | SymbolResolver/binder | Única ruta nombre→id, solo al escribir, en las dos superficies; nunca elige entre homónimos | PROPOSED · sintaxis → **OPEN A** |
| 8 | project-variable symbol binding | Tabla desde el registro acreditado; `VariableType` como metadata del consumidor | PROPOSED |
| 9 | units | `[mm]`, `[in]`, `[ft]` como conversión numérica a pulgadas; una autoridad neutral | PROPOSED |
| 10 | function registry | Registro cerrado; `MIN`, `MAX`, `ABS`; `ROUND`/`CEILING`/`FLOOR` evaluados y diferidos | PROPOSED |
| 11 | dependency extraction | Función pura sin nombres ni evaluación, para definiciones y fuentes de propiedad | PROPOSED |
| 12 | dependency graph | Derivado por snapshot, solo variable→variable; consumidores de rack unidos en el plan | PROPOSED |
| 13 | cycle detection | Componentes fuertemente conexas, iterativo; ciclo = plan vacío; las propiedades no forman ciclos | PROPOSED · persistidos → **OPEN B** |
| 14 | evaluation order | Topológico determinista; variables una vez; expresiones de propiedad al resolver cada rack | PROPOSED |
| 15 | diagnostics | Catálogo cerrado; sin códigos dimensionales; `UnknownUnit` y `OutOfRange` de consumidor | PROPOSED |
| 16 | EvaluationResult | `Success`/`Failed` con `double`, diagnósticos y traza opcional | PROPOSED |
| 17 | persistence | Definición `literal \| expression`; fuente `literal \| projectVariable \| expression`; árbol con ids | PROPOSED · frontera estructural revisable en **OPEN B** |
| 18 | schema evolution | Política real contrastada; V-0 sin cambio de versión, fail-closed por kind | PROPOSED · **ARCHITECT REVIEW REQUIRED** |
| 19 | rename | Registry-only en las dos superficies | PROPOSED |
| 20 | delete | Bloqueado por consumidores (directos o expresiones) o dependientes; sin cascada | PROPOSED · estado de error → **OPEN B** |
| 21 | propagation | Dependientes transitivos → consumidores de cualquiera → un resolve por rack → un plan | PROPOSED |
| 22 | RACKVARIABLES UX | Campo «Definición» con unidades, diagnóstico en vivo, selector, dependencias y consumidores | PROPOSED · token → **OPEN A** · errores → **OPEN B** |
| 23 | LinkedPropertyEditor UX | Expresiones en el mismo control, con borradores, Escape, LostFocus y C4 de I-48 | PROPOSED · sintaxis → **OPEN A** |
| 24 | direct-reference compatibility | Modelo híbrido; referencia directa intacta; `=X` canónico; sexto resultado tipado | PROPOSED · alcance del bloqueo → **OPEN B** |
| 25 | ID20 extension point | `SymbolId`, namespaces, ámbitos y contexto listos; `Rack.*` y `Project.*` reservados | PROPOSED |
| 26 | ID23 extension point | Núcleo y autoridad de unidades sin dependencia de Project Variables | PROPOSED |
| 27 | ID28/29 information preservation | `BoundExpression`, `SymbolId`, diagnósticos, dependencias, dependientes y traza opcional | PROPOSED |
| 28 | tests | Criterio de comportamiento; evolución explícita de guardas y pines | PROPOSED |
| 29 | migration | Sin migración de datos ni de versión en V-0 | PROPOSED |
| 30 | non-goals | Exclusiones explícitas, alineadas con los cuatro BLOCKER | PROPOSED |

### P1 — grammar

**Base.** No existe ningún parser de expresiones en el repositorio (Discovery §15, búsqueda 1). La coma ya tiene
dos significados: decimal en la regla localizada de ADR-0015 y separador de lista en otras superficies (Discovery
§11.7). El editor vinculable parsea literales en cultura invariante y RACKVARIABLES con la regla localizada
(Discovery §11.2–§11.3). No existe sintaxis de unidades (Discovery §11.7).

- **P1.1 — Lenguaje.** Expresiones aritméticas sobre números (con unidad opcional), referencias y llamadas a
  función. Sin sentencias, asignaciones, comparaciones, condicionales, booleanos ni cadenas.
- **P1.2 — Gramática** (EBNF; la producción `reference` es **OPEN FOR ARCHITECT A**, §4):

  ```ebnf
  expression     = additive ;
  additive       = multiplicative , { ( "+" | "-" ) , multiplicative } ;
  multiplicative = unary , { ( "*" | "/" ) , unary } ;
  unary          = ( "+" | "-" ) , unary | primary ;
  primary        = quantity | call | reference | "(" , expression , ")" ;
  quantity       = number , [ "[" , unit , "]" ] ;
  unit           = "mm" | "in" | "ft" ;
  call           = function-name , "(" , [ expression , { "," , expression } ] , ")" ;
  number         = digit , { digit } , [ "." , digit , { digit } ] ;
  function-name  = letter , { letter | digit } ;
  reference      = (* OPEN FOR ARCHITECT A — §4 *) ;
  ```

- **P1.3 — Precedencia.** `*` y `/` sobre `+` y `-`; asociatividad por la izquierda; el unario liga más fuerte
  que cualquier binario.
- **P1.4 — Números.** Forma invariante con `.` como único separador decimal. Sin exponente, sin agrupadores, sin
  punto inicial ni final (`.5`, `5.`), sin signo dentro del número (el signo es operador unario) y sin `NaN` ni
  `Infinity`.
- **P1.5 — Coma.** `,` es **solo** separador de argumentos. Una coma entre dos dígitos (`1,5`) es el error léxico
  `AmbiguousDecimalComma`: nunca se lee como decimal ni como dos argumentos, de modo que `MAX(Holgura, 1,5)` no
  puede convertirse en silencio en `MAX(Holgura, 1, 5)`.
- **P1.6 — Unidades** (REQUISITO DEL OWNER, BLOCKER 2):
  - un sufijo `[mm]`, `[in]` o `[ft]` se admite **solo** tras un literal numérico: `100[mm]`, `4[in]`, `20[ft]`;
  - los tokens de unidad son exactamente `mm`, `in` y `ft`. Cualquier otro (`[cm]`, `[MM]`, `[m]`) es
    `UnknownUnit`;
  - un sufijo tras una referencia, una llamada o un paréntesis (`Holgura[mm]`, `(2 + 3)[mm]`) es
    `UnitNotAllowedHere`;
  - las demás notaciones (`100 mm`, `12"`, `10'6"`, `1 1/8`) son `UnitSyntaxNotSupported`. `1/2` no es una
    fracción: es una división válida.

  El significado numérico de cada unidad está en P9.
- **P1.7 — Espacios.** No significativos entre tokens: `100 [mm]` se acepta, y su forma canónica es `100[mm]`.
  Dentro de una referencia, según OPEN A.
- **P1.8 — El `=` no pertenece a la gramática.** Es la marca de superficie que indica «esto es una expresión», en
  RACKVARIABLES (P22) y en el editor vinculable (P23). Nunca se persiste.
- **P1.9 — Límites** (valores iniciales, ajustables en revisión): texto ≤ 1000 caracteres, anidamiento ≤ 64,
  nodos ≤ 256 y argumentos por llamada ≤ 16. Superarlos es `LimitExceeded`, **nunca** truncado.
- **P1.10 — Implementación.** Parser escrito a mano, determinista, independiente de la cultura y con el límite de
  anidamiento comprobado **antes** de descender. Prohibidos terceros (ADR-0012), `NCalc`, `DataTable.Compute`,
  `System.Linq.Expressions` y Roslyn (Discovery §15, búsqueda 3). **Un solo** lexer reconoce unidades: el del
  núcleo.
- **P1.11 — Texto sin `=`.** Sigue siendo un literal parseado **exactamente como hoy** en cada superficie: regla
  localizada de ADR-0015 en RACKVARIABLES e invariante en el editor vinculable. V2 no resuelve esa divergencia
  vigente (Discovery §11.2) ni admite unidades sin `=`. La gramática invariante de P1.4 solo rige dentro de una
  expresión, y esa excepción acotada a ADR-0015 tiene que constar en el ADR (§8).

### P2 — AST/model

**Base.** `VariableDefinition` tiene un caso y payload `double` (Discovery §4.1). `LinkedPropertySourceKind` es un
enum de dos miembros cuyo propio comentario anticipa que «una fórmula» añadirá un tercero
(`src/RackCad.Application/ProjectVariables/LinkedPropertySource.cs:8-13`). No hay AST (Discovery §15, búsqueda 2).

- **P2.1 — Tres modelos inmutables y separados.**
  1. **Sintaxis** (salida del parser): nodos con `SourceSpan` y referencias en forma textual. Solo sirve para
     escribir y diagnosticar. **Nunca se persiste.**
  2. **`BoundExpression`** (salida del binder o de la lectura de persistencia): `Number(double, unidad opcional)`,
     `Reference(SymbolId)`, `Negate(operand)`, `Binary(Add | Subtract | Multiply | Divide, left, right)` y
     `Call(FunctionId, args)`. Sin posiciones, sin nombres, sin paréntesis redundantes y sin `+` unario. **Sin
     anotaciones de tipo** (DR-5). Es lo que se persiste (P17), se evalúa (P14) y se formatea (P4).
  3. **Resultado** de evaluación (P16).
- **P2.2 — Comprobación semántica separada.** Existencia de referencias, aridad, unidades y ámbito se comprueban
  sobre un `BoundExpression` **contra un contexto** (P7.5), no dentro del árbol. El mismo árbol leído de
  persistencia se re-comprueba en cada snapshot.
- **P2.3 — Igualdad estructural por valor.** Ordinal para tokens y `double.Equals` para números, igual que la
  igualdad exacta vigente (Discovery §11.4). La unidad forma parte de la igualdad: `100[mm]` ≠ `3.937…`.
- **P2.4 — Conjunto de nodos cerrado.** Añadir un tipo de nodo, una unidad o una función cambia lo que las builds
  anteriores pueden leer (P18.9).
- **P2.5 — Canonicidad.** Para todo `BoundExpression` canónico `b` y un mismo contexto:
  `Canonicalize(Bind(Parse(Format(b)))) == b`. Se prueba como propiedad (P28). Con homónimos se sostiene solo si
  la sintaxis de OPEN A cumple su requisito de representación editable (§4.1).
- **P2.6 — `VariableDefinition`.** Gana un segundo caso, `Expression = 2`, con payload `BoundExpression`.
  `LiteralValue` sigue lanzando fuera de `Literal`, y el acceso a la expresión lanza fuera de `Expression`.
- **P2.7 — `LinkedPropertySource`.** Gana el tercer miembro `Expression = 3`, con payload `BoundExpression`. El
  acceso a `VariableId` sigue lanzando fuera de `ProjectVariableReference`, y el acceso a la expresión lanza fuera
  de `Expression`. `LinkedPropertyEditState` conserva su forma `{ CommittedLiteral, Source }`.
- **P2.8 — Formas canónicas: una sola representación por significado** (DR-4). Se aplican **al comprometer**, en
  las dos superficies:

  | Árbol enlazado | Fuente de propiedad (superficie B) | Definición de variable (superficie A) |
  |---|---|---|
  | Solo `Reference(projectVariable X)` | **`ProjectVariableReference(X)`**, la referencia existente (REQUISITO DEL OWNER) | `Expression`: una definición no tiene caso referencia |
  | Solo un número **sin unidad**, con signo opcional | `Literal(±v)` | `Literal(±v)` |
  | Cualquier otro árbol: operación, función o **unidad explícita** | `Expression` | `Expression` |

  Así `=Holgura` y la selección de `Holgura` en la lista persisten lo mismo, y `=6` persiste lo mismo que `6`.
  `=4[in]` es `Expression` aunque valga 4, porque lleva conversión explícita (REQUISITO DEL OWNER). Al **leer**,
  una forma no canónica (una `expression` que sea solo una referencia o solo un número sin unidad) es error
  estructural (P17.6), **nunca** normalización silenciosa.

### P3 — identity

**Base.** `VariableId` es GUID con igualdad `OrdinalIgnoreCase` (Discovery §4.1). La identidad ambigua es
fail-closed también en la re-lectura de commit (Discovery §4.2).

- **P3.1** — La identidad de una variable sigue siendo su `VariableId` (ADR-0034 §2). Cambiar el tipo de su
  definición no cambia el id.
- **P3.2** — `SymbolId = (SymbolNamespace, Key)`. Los namespaces son tokens de un conjunto cerrado comparados en
  `Ordinal`, y cada namespace fija el comparador de su clave. Para `projectVariable` la clave es el `VariableId`,
  con su igualdad vigente.
- **P3.3** — Una expresión de definición **no tiene identidad propia**: es la definición de su variable. Los nodos
  del grafo son `SymbolId`.
- **P3.4** — `FunctionId` es un token canónico en **mayúsculas** (`MIN`, `MAX`, `ABS`) comparado en `Ordinal`; la
  entrada no distingue mayúsculas.
- **P3.5** — El nombre **nunca** es identidad: el árbol persistido no contiene nombres (P17) y la única resolución
  por nombre ocurre al escribir (P7.3).
- **P3.6** — Un `VariableId` duplicado da `AmbiguousIdentity` **antes** de construir cualquier tabla de símbolos
  (decisiones de I-48 §4): nada se enlaza ni se evalúa sobre un registro ambiguo.
- **P3.7** — Una variable que se referencia a sí misma forma un ciclo de longitud 1 (P13).
- **P3.8** — Una expresión de propiedad **tampoco** tiene identidad propia: pertenece a `(RackId, PropertyId)`,
  igual que hoy un binding. Ninguna expresión puede referenciarla (P13.6).

### P4 — source text

**Base.** HANDOFF §4 exige persistir la referencia y «nunca el texto» (`docs/HANDOFF.md:1579`). `"0.###"` es formato
de **presentación** y redondea (Discovery §11.5, hallazgo lateral L5).

- **P4.1 — Tres textos, tres papeles**, en las dos superficies:

  | Texto | Origen | ¿Autoridad? | ¿Se persiste? |
  |---|---|---|---|
  | Texto tecleado | Usuario | No | No |
  | Forma canónica persistida (literal, referencia o `BoundExpression`) | Binder + canonicalización | **Sí** | Sí (P17) |
  | Texto de edición y presentación | Formatter, desde la forma canónica y los nombres **actuales** | No | No |

- **P4.2 — Un solo formatter.** Determinista e invariante: números en su representación más corta que hace
  *round-trip* (nunca `"0.###"`); unidad pegada al número (`100[mm]`); un espacio a cada lado de un operador
  binario; `MIN(a, b)`; paréntesis **mínimos** según precedencia y asociatividad; funciones en mayúsculas;
  referencias según OPEN A.
- **P4.3 — Normalización visible.** Espacios, paréntesis redundantes y mayúsculas de funciones no se conservan.
  Tras comprometer, el usuario ve la forma canónica.
- **P4.4 — El texto tecleado no se persiste.** Sería una segunda fuente capaz de contradecir al árbol y quedaría
  obsoleta con cada rename (ADR-0034 §2).
- **P4.5 — Referencia rota.** El formatter representa una referencia cuyo id no está en el registro de forma
  reconocible y no comprometible, **sin** tomar un nombre de ningún otro sitio. La forma exacta depende de OPEN A.
- **P4.6** — Una referencia directa se muestra como `=` seguido de la referencia formateada (hoy `=Nombre`,
  Discovery §9.2); una expresión, como `=` seguido del formatter.

### P5 — symbol model

**Base.** No existe `SymbolId`, scope ni namespace, y el registro acreditado se indexa solo por `VariableId`
(Discovery §15, búsquedas 7–9; §16.2).

- **P5.1** — `SymbolEntry { SymbolId, SymbolScope, DisplayName, ConsumerType, Definition }`, con
  `Definition = Literal(double) | Expression(BoundExpression)`. `ConsumerType` transporta el `VariableType`
  declarado como metadata (P5.5).
- **P5.2 — Namespaces.** En V2 hay **uno** activo, `projectVariable`. Quedan **reservados conceptualmente, sin
  registrar y sin resolución**: `rack` y `project` (ID20, P25). El binder devuelve `UnknownNamespace` para
  cualquier namespace no registrado.
- **P5.3 — Ámbitos.** Existe **uno** activo, `Project`: un valor por dibujo. Queda reservado `Rack`: un valor por
  rack (ID20). Una definición de ámbito `Project` **no puede** depender de un símbolo `Rack` (`ScopeViolation`). Una
  expresión de propiedad se evalúa en el contexto de su rack y en V2 solo ve símbolos `Project`.
- **P5.4 — Dominio de valores adimensional** (REQUISITO DEL OWNER, BLOCKER 3). Todo valor del motor es un `double`
  finito. **No existe** `ExpressionValueType`, ni `Scalar`, ni `DimensionMismatch`, ni `ResultTypeMismatch`, ni
  reglas del tipo `Length × Length` o `Length ÷ Length`.
- **P5.5 — `VariableType` como metadata del consumidor.** `VariableType` sigue siendo exactamente `{ Length = 1 }`
  (las guardas G6–G10 no cambian, Discovery §14.3). Dice **cómo interpreta el consumidor** el número (pulgadas);
  **no** gobierna ninguna álgebra dentro del evaluador.
- **P5.6 — Sin comprobador dimensional.** Son válidas, y lo fijan pruebas de comportamiento (P28.3): `6 + 2`,
  `A + 2`, `A * B`, `A / B`, `100[mm] + 2` y `MAX(A, 4[in])`. El motor no rechaza **ninguna** operación por
  dimensiones. El único rango que existe es el del consumidor (P21.7, P23.6).

### P6 — ExpressionContext

**Base.** RACKBOMTOTAL lee el registro **una** vez y no resuelve por su cuenta (Discovery §14.3, guarda G12). La
costura pura `FromTargets` existe solo para tests (Discovery §4.3).

- **P6.1** — `ExpressionContext` es un valor inmutable **por operación**: `SymbolTable` + la única instancia
  productiva de `FunctionRegistry` + la autoridad de unidades (P9.5) + `ExpressionLimits`.
- **P6.2 — Sin estado ambiental**: ni cultura, ni reloj, ni aleatoriedad, ni entorno, ni cachés estáticas, ni
  AutoCAD, ni sistema de archivos.
- **P6.3 — Un snapshot por operación**, el mismo para toda ella (ADR-0034 §10).
- **P6.4 — Construcción para Project Variables**: solo desde `UsableProjectVariablesRegistry` acreditado. `Absent`
  da tabla vacía; `Readable` con ids únicos da tabla; cualquier otro resultado **no** produce contexto (Discovery
  §4.1, `Accredit`).
- **P6.5 — Costura de test**: la construcción desde entradas sintéticas se permite en el núcleo y en tests, y no
  es alcanzable desde Plugin ni UI, igual que `FromTargets`.
- **P6.6** — ID20 e ID23 construirán sus propios contextos desde sus propios snapshots puros (P25, P26).
- **P6.7 — Contexto de una propiedad de rack.** Para evaluar la expresión de una propiedad, el resolver usa el
  contexto del snapshot y los valores **ya evaluados** de sus variables (`RegistryEvaluation`, P16.2). En V2 no
  añade símbolos propios del rack.

### P7 — SymbolResolver/binder

**Base.** Hoy **ninguna** ruta de producción resuelve nombre→id; el editor filtra por *contains*
`OrdinalIgnoreCase` y compromete una referencia solo por `VariableId` (Discovery §13.3; `TrySelect` es «the ONLY way
a reference is ever committed», `LinkedPropertyEditSession.cs:302-306`). Los homónimos son legales (Discovery §13.1).

- **P7.1 — `SymbolResolver`.** Traduce una referencia textual, con la sintaxis que fije OPEN A, a **exactamente un**
  `SymbolId`, o a un fallo tipado: `UnknownSymbol`, `AmbiguousName` (con **todos** los ids candidatos) o
  `UnknownNamespace`. **Nunca** elige uno entre varios ni completa un nombre parcial.
- **P7.2 — Comparador de nombres** (propuesto, revisable dentro de OPEN A): igualdad exacta `OrdinalIgnoreCase`, sin
  recortes ni normalización Unicode, coherente con los comparadores vigentes (Discovery §13.3). El *contains* del
  editor sigue siendo un **filtro** y no una regla de resolución (Discovery §13.4).
- **P7.3 — Única ruta nombre→id de producción, y solo al escribir.** El binder corre únicamente al comprometer texto
  del usuario en las dos superficies (P22, P23). Lectura, evaluación, rename, delete, propagación, resolver y BOM
  **nunca** resuelven nombres.
- **P7.4 — Binder total y fail-closed.** Devuelve un árbol solo con **cero** diagnósticos; si no, la lista de
  diagnósticos, en orden determinista y con un tope (20, ajustable).
- **P7.5 — Comprobación semántica** (P2.2): existencia de referencias, aridad (P10), unidad conocida (P9), ámbito
  (P5.3) y autorreferencia (P3.7). Sobre un árbol leído de persistencia corre **sin** resolver nombres.
- **P7.6 — Dónde vive.** En Application. El control WPF **no** resuelve nombres: la guarda G19 sigue exigiendo que
  `LinkedPropertyEditor.cs` no compare nombres (Discovery §14.3).

### P8 — project-variable symbol binding

**Base.** El `Type` persistido tiene un mapping único compartido (decisiones de I-48 §4; `VariableTypes.TryParseToken`,
Discovery §4.1). La compatibilidad de tipo al vincular vive en `InspectBinding` y `Link` (Discovery §4.1, §8.1).

- **P8.1** — Namespace `projectVariable`; clave = el texto del `VariableId` de la entrada del registro; comparación
  `OrdinalIgnoreCase`.
- **P8.2** — Fuente única: el registro acreditado (P6.4), con el mapping único del `Type`. **No** se crea un segundo
  mapping.
- **P8.3** — `DisplayName = Name`, solo para mostrar y para resolver al escribir.
- **P8.4** — `Definition` = el `Literal` o la `Expression` de la entrada.
- **P8.5** — Un árbol que referencia un id ausente produce `BrokenReference`. Al escribir no puede ocurrir, porque el
  binder solo emite ids de la tabla; en un árbol persistido es error semántico (OPEN B).
- **P8.6** — Una referencia directa de rack (`ProjectVariableReference`) **no** es un símbolo de expresión: es un
  consumidor que lee el valor evaluado de su variable (P24).
- **P8.7 — Referencia directa.** Conserva **sin cambio** la comprobación vigente entre el `VariableType` de la
  variable y el de la propiedad (V4-R01 de I-48).
- **P8.8 — Expresión de propiedad o de definición.** Puede referenciar cualquier variable de `VariableType`
  numérico; en V2 todos lo son, porque solo existe `Length`. El resultado es un `double` que el consumidor
  interpreta según **su** `VariableType` declarado. Qué pasa al combinar tipos distintos se decidirá cuando exista
  un segundo `VariableType`, con ADR.

### P9 — units

**Base.** Pulgadas internas como `double` sin unidad (ADR-0005, ADR-0021; Discovery §12.1–§12.2). No hay autoridad
neutral de conversión ni parser de unidades (Discovery §12.4; §15, búsqueda 12). `StructuralSectionUnits` es
específico del catálogo (Discovery §12.5).

- **P9.1** — La pulgada sigue siendo la unidad interna canónica. El motor **no convierte el DWG** (ADR-0005 §4) y
  todo resultado está en pulgadas.
- **P9.2 — Unidades admitidas** (REQUISITO DEL OWNER): `[mm]`, `[in]` y `[ft]`, solo sobre literales numéricos
  (P1.6).
- **P9.3 — Semántica**: conversión **numérica** a pulgadas en la evaluación, con factores exactos.

  | Escrito | Cálculo | Resultado en pulgadas |
  |---|---|---|
  | `4[in]` | `4` | `4` |
  | `20[ft]` | `20 × 12` | `240` |
  | `100[mm]` | `100 ÷ 25.4` | `3.937007874015748…` |

  La operación está **fijada** (multiplicar por 12, dividir entre 25.4) para que el resultado sea determinista bit
  a bit.
- **P9.4 — Número sin unidad.** Es un número adimensional. Un consumidor `Length` lo lee en pulgadas por contrato
  (P5.5); el motor no infiere ninguna unidad.
- **P9.5 — Una sola autoridad neutral de conversión.** Un único componente de Application (ilustrativo:
  `RackCad.Application.Units.LengthUnits`) declara la tabla cerrada de unidades, sus tokens y los factores exactos
  `MillimetersPerInch = 25.4` e `InchesPerFoot = 12`. No depende de nada: ni del núcleo, ni de `StructuralSections`,
  ni de sistemas. El núcleo la consume; **no** existe otro parser de unidades en UI, Domain ni Plugin.
- **P9.6 — Auditoría de los factores exactos existentes** (hechos verificados en `a4d88f1`):

  | Constante | Ubicación | Papel |
  |---|---|---|
  | `InchesToMillimeters = 25.4`, pública | `src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs:18-19` | Datos de secciones del catálogo |
  | `InchesPerFoot = 12d`, privada | `StructuralSectionUnits.cs:27` | Peso por longitud de secciones |
  | `FootInches = 12.0` | `src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs:31` | Redondeo al pie de la altura (`:422`) |
  | `CommercialFoot = 12.0` | `src/RackCad.Application/Systems/Dynamic/DynamicHeaderHeightCalculator.cs:49` | Redondeo al pie comercial (`:147-154`) |

  Además, el factor lb/ft→kg/m está duplicado en `tools/`, y una guarda prohíbe `25.4` en el materializador del
  Plugin (Discovery §12.3).
- **P9.7 — Reutilización sin acoplar.** La autoridad neutral usa **los mismos valores exactos y la misma fuente
  normativa** (1 in = 25.4 mm, 1 ft = 12 in; ADR-0021, factores exactos). Una prueba fija la paridad con
  `StructuralSectionUnits.InchesToMillimeters`. El núcleo **nunca** referencia `StructuralSections` (guarda, P28.4).
  Hacer que `StructuralSectionUnits`, `FootInches` o `CommercialFoot` deleguen en la autoridad neutral es un
  refactor **fuera** de V2: son constantes con papel de dominio, y `SelectiveGeometryResolver.cs` está en el plan de
  I-50. Se registra como seguimiento en G4.
- **P9.8 — Presentación.** El texto de una expresión conserva la unidad escrita (`=100[mm] + Base`). Los valores se
  muestran en pulgadas con `"0.###"`, y las etiquetas «(in)» se mantienen.
- **P9.9** — Sin áreas, volúmenes, masas ni ángulos, y sin unidades distintas de `mm`, `in` y `ft` (P30).

### P10 — function registry

**Base.** No existe registro de funciones (Discovery §15, búsqueda 6). ADR-0012 prohíbe dependencias NuGet de
producto sin acuerdo del Owner y ADR (Discovery §15).

- **P10.1** — `FunctionRegistry` **cerrado e inmutable**, declarado en código dentro del núcleo, con una sola
  instancia productiva. Sin registro en tiempo de ejecución, reflexión, inyección, plugins, funciones de usuario ni
  scripting.
- **P10.2 — Contrato de una función**: `FunctionId`, aridad mínima y máxima, implementación pura y determinista
  sobre `double` finitos. Un resultado no finito es `NonFiniteResult`.
- **P10.3 — Conjunto inicial** (REQUISITO DEL OWNER, BLOCKER 4):

  | Función | Aridad | Semántica |
  |---|---|---|
  | `MIN(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Menor valor |
  | `MAX(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Mayor valor |
  | `ABS(x)` | 1 | Valor absoluto |

- **P10.4** — Al escribir: nombre desconocido → `UnknownFunction`; aridad errónea → `ArityMismatch`. En un árbol
  persistido: `FunctionId` desconocido → ilegibilidad **estructural** (P17.6); función conocida con aridad errónea →
  error **semántico** (OPEN B).
- **P10.5** — Nombre sin distinguir mayúsculas a la entrada (`max`, `Max`); mayúsculas canónicas en el árbol y en
  el formatter.
- **P10.6 — Candidatos evaluados y DIFERIDOS: `ROUND`, `CEILING`, `FLOOR`.**

  | Pregunta que bloquea | Por qué no se decide en V2 |
  |---|---|
  | Regla del punto medio | `Math.Round` de .NET redondea al par por defecto y las hojas de cálculo suelen alejarse del cero: una u otra cambia resultados sin avisar |
  | Precisión o múltiplo | ¿Entero? ¿Dígitos? ¿Un múltiplo, como `CEILING(x, 6)`? Cada firma es un contrato persistido distinto |
  | Interacción con unidades | Tras `100[mm]`, redondear a pulgada entera no es lo que el usuario escribió en milímetros |
  | Negativos | Sentido de `CEILING` y `FLOOR` con valores negativos |
  | Consumidor natural | Las cantidades **enteras** del BOM (`Quantity`, Discovery §16.3) son de ID23, que tendrá que fijar su semántica |

  Pueden añadirse después como ampliación **cerrada** del registro, con revisión, y con ADR si cambian lo que se
  persiste. Una build anterior las rechaza de forma estructural (P18.9).
- **P10.7 — Excluidos salvo nueva razón aprobada**: `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings,
  trigonometría y macros.
- **P10.8** — Añadir una función es un cambio de código con revisión y, para las builds anteriores, un cambio de lo
  que pueden leer (P18.9).

### P11 — dependency extraction

**Base.** Hoy no existe ninguna relación variable→variable (Discovery §6.2). `Targets()` ordena por id
`OrdinalIgnoreCase` (Discovery §13.3).

- **P11.1** — `Dependencies(BoundExpression) → IReadOnlyList<SymbolId>`: referencias **directas**, sin repetición y
  en orden determinista (namespace `Ordinal`; después clave con el comparador de su namespace, el mismo orden que
  `Targets()`).
- **P11.2** — Opera sobre el árbol persistido **sin evaluar** y **sin nombres**.
- **P11.3** — Un literal tiene cero dependencias. Una referencia directa de rack depende de su único `VariableId`.
- **P11.4** — La consumen el grafo (P12), `Delete` (P20), la propagación (P21), RACKVARIABLES (P22) y la
  preservación para ID28/ID29 (P27).
- **P11.5** — La **misma** función extrae las dependencias de una expresión de definición y de una expresión de
  propiedad: no hay una segunda implementación por superficie.

### P12 — dependency graph

**Base.** ADR-0034 rechazó persistir un grafo en ID22A porque «el grafo pertenece a ID22B/ID21» (Discovery §6.2). La
relación propiedad→variable vive en el authored de cada rack y se descubre barriendo el dibujo (Discovery §7).

- **P12.1 — Derivado, nunca persistido.** Se construye desde la tabla de símbolos de UN snapshot. El registro es la
  única autoridad: un grafo persistido sería una segunda fuente capaz de divergir.
- **P12.2** — Nodos: los símbolos de la tabla (en V2, las variables de proyecto). Aristas: dueña → dependencia, con
  índice inverso dependencia → dependientes.
- **P12.3 — Consumidores de rack fuera del grafo.** Las relaciones propiedad→variable, directas **o** por expresión,
  no entran en el grafo de variables: se obtienen del barrido vigente y se unen al componer el plan (P21.4). Es la
  separación que el Discovery dedujo para su pregunta 8 (Discovery §2.2).
- **P12.4** — Una arista hacia un id ausente se registra como `BrokenReference`, sin crear nodo.
- **P12.5** — Iteración en el orden de P11.1.
- **P12.6** — Coste `O(V + E)` por snapshot. Que los registros reales sean pequeños es **INFERENCE**; se mide en G11.

### P13 — cycle detection

**Base.** No existe detección genérica de ciclos (Discovery §15, búsqueda 5). El plan vacío ante fallo es invariante
(Discovery §4.2).

- **P13.1** — Detección **iterativa** (sin riesgo de profundidad de pila) y determinista de componentes fuertemente
  conexas. Un ciclo es una componente de más de un nodo o un nodo con arista a sí mismo.
- **P13.2** — Informa **cada** ciclo con sus miembros en el orden de P11.1; cada miembro recibe `Cycle` con la lista
  completa.
- **P13.3** — Al escribir o mutar, un estado «después» con un ciclo que pase por la variable cambiada hace fallar el
  preflight con `Cycle` y plan **vacío** (P21).
- **P13.4** — Los miembros de un ciclo y **todos** sus dependientes transitivos no son evaluables (`Cycle` o
  `DependencyFailed`), y tampoco las propiedades que los consumen. **Nunca** hay fallback a un literal ni a cero.
- **P13.5** — Con P13.3, esta build no escribiría un ciclo. El contrato cuando se **lee** uno persistido es **OPEN
  FOR ARCHITECT B** (§5).
- **P13.6** — Las expresiones de propiedad **no** forman ciclos: nada puede referenciar una propiedad en V2 (ID21
  fuera). Solo pueden quedar afectadas por un ciclo de variables.

### P14 — evaluation order

**Base.** Hoy el resolver escribe `Target.LiteralValue` y ningún valor se calcula a partir de otro (Discovery §6.2).
La igualdad es exacta (Discovery §11.4).

- **P14.1** — Orden topológico sobre la parte acíclica, con el desempate de P11.1, **independiente** del orden de las
  entradas en el registro. Una prueba lo demuestra con permutaciones (P28).
- **P14.2** — Las **variables** se evalúan de forma **ansiosa**, una vez por operación (`RegistryEvaluation`) y con
  memoización: cada una se evalúa una sola vez.
- **P14.3** — Cortocircuito: si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id de la
  causa, sin evaluarse.
- **P14.4 — Semántica numérica.** `double` IEEE-754 sin redondeos intermedios. `x ÷ 0`, incluido `0 ÷ 0`, es
  `DivisionByZero`. Cualquier resultado intermedio o final no finito es `NonFiniteResult`. Las unidades se
  convierten al evaluar su literal (P9.3).
- **P14.5 — Determinismo.** Sin paralelismo, cultura ni reloj: el mismo snapshot da resultados idénticos bit a bit.
- **P14.6** — Un literal se evalúa a su propio valor, sin cambio. Por eso todo dibujo de I-47/I-48 produce
  **exactamente** los mismos efectivos que hoy (P24.11).
- **P14.7 — Dónde.** Dentro de Application: las variables tras la acreditación, y las expresiones de propiedad dentro
  del resolver único (DR-3). **Nunca** en el Plugin, la UI, el ejecutor ni el comando de BOM (guardas extendidas,
  P28.4).
- **P14.8 — Expresiones de propiedad.** Se evalúan **una vez por rack** durante su resolución, sobre los valores de
  `RegistryEvaluation` de ese mismo snapshot. Un fallo de cualquier propiedad deja **sin efectivo** a todo el rack,
  igual que hoy un binding roto (Discovery §4.2: el resolver valida todo antes de escribir un campo).

### P15 — diagnostics

**Base.** La doctrina exige estados tipados (ADR-0034 §8). El texto de reparación ya vive en una sola capa
(`ProjectVariableRepairText`, Discovery §3.4).

- **P15.1** — Catálogo **cerrado** de códigos. Cada diagnóstico lleva código estable, clase, severidad, dueño (una
  variable, o un par `(RackId, PropertyId)`), posición opcional (solo al escribir) e ids relacionados.
- **P15.2 — Severidad.** En V2 todo diagnóstico es `Error`: no hay avisos que permitan continuar.
- **P15.3 — Catálogo de V2.**

  | Clase | Códigos |
  |---|---|
  | Sintaxis | `EmptyExpression`, `UnexpectedCharacter`, `UnexpectedToken`, `UnbalancedParenthesis`, `InvalidNumber`, `AmbiguousDecimalComma`, `UnknownUnit`, `UnitNotAllowedHere`, `UnitSyntaxNotSupported`, `LimitExceeded` |
  | Enlace | `UnknownSymbol`, `AmbiguousName`, `UnknownNamespace`, `UnknownFunction`, `ArityMismatch`, `ScopeViolation` |
  | Grafo | `BrokenReference`, `Cycle`, `DependencyFailed` |
  | Evaluación | `DivisionByZero`, `NonFiniteResult` |
  | Contrato del consumidor | `OutOfRange` (P21.7, P23.6) |

  **Eliminados respecto de V1** (BLOCKER 3): `DimensionMismatch` y `ResultTypeMismatch`.
- **P15.4** — El núcleo devuelve códigos y datos. Los mensajes en español se producen en **una** capa de texto de
  Project Variables o de UI.
- **P15.5** — Cualquier `Error` implica: sin valor, sin resultado parcial y sin fallback.
- **P15.6** — Los estados de nivel registro conservan sus resultados vigentes (`PresentButUnreadable`,
  `IncompatibleMajor`, `AmbiguousIdentity`; Discovery §4.1). Los diagnósticos **no** los sustituyen.
- **P15.7** — Orden determinista: posición, código y dueño.

### P16 — EvaluationResult

**Base.** El target acreditado tiene hoy forma de literal (`VariableTargetSnapshot`, Discovery §4.1 y §4.4).

- **P16.1 — Por símbolo**: `EvaluationResult { SymbolId, Outcome = Success | Failed, Value, Diagnostics, Trace }`.
  `Value` es un `double` finito que solo existe con `Success`; acceder a él en otro caso **lanza**, con la misma
  disciplina que `LiteralValue`. `Diagnostics` no está vacío si y solo si `Failed`. **Sin** campo de tipo (DR-5).
- **P16.2 — Por snapshot**: `RegistryEvaluation { resultados por símbolo, grafo directo e inverso, ciclos, orden de
  evaluación }`, inmutable.
- **P16.3 — Traza opcional** (P27): definición enlazada, dependencias directas con el valor que tenían y cadena de
  causas de un fallo. Tamaño acotado, solo en memoria y desactivable sin cambiar ningún resultado.
- **P16.4** — Los consumidores leen `Value` solo tras comprobar `Success`. El target acreditado pasa a llevar
  **valor evaluado y resultado de evaluación**, en lugar de un valor con forma de literal.
- **P16.5** — Una variable literal da `Success(literal)` con traza trivial.
- **P16.6 — Por propiedad de rack**: el resolver produce, de forma transitoria, un resultado equivalente por
  `(RackId, PropertyId)`. Lo consumen la resolución, el preflight y el texto de estado del editor.

### P17 — persistence

**Base.** El registro es un `Xrecord` troceado en el NOD bajo `RACKCAD_PROJECT`, con opciones JSON por defecto y
propiedades en PascalCase (Discovery §5.1). Los bindings del Selectivo viven en el mapa opcional `PropertyValues`
del portador authored (Discovery §5.1). El store del registro rechaza todo kind distinto de `literal` (Discovery
§5.3), y `SelectivePropertyValueDocument` declara que un kind desconocido «is NOT ignored and does NOT fall back to
the literal» (`src/RackCad.Application/Persistence/SelectivePropertyValueDocument.cs:17-20`).

- **P17.1** — Cambian dos contratos persistidos: la **definición** en el registro y la **fuente** en `PropertyValues`.
  El almacenamiento físico de ambos no cambia.
- **P17.2 — Definición de variable: `literal | expression`.**

  ```json
  {"Kind":"literal","Value":6.0}
  {"Kind":"expression","Expression":{"Node":"add",
    "Left":{"Node":"ref","Namespace":"projectVariable","Id":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
    "Right":{"Node":"ref","Namespace":"projectVariable","Id":"8c1d7e20-4b5a-4c6d-9e7f-102132435465"}}}
  ```

  El literal se persiste **exactamente** como hoy. `expression` va **sin** `Value`.
- **P17.3 — Fuente de propiedad: `literal | projectVariable | expression`.**
  - **literal**: **sin** entrada en `PropertyValues`; el número es el campo authored, como hoy.
  - **projectVariable**: `{"Kind":"projectVariable","VariableId":"<guid>"}`, **sin cambio**.
  - **expression**: `{"Kind":"expression","Expression":<nodo>}`, **sin** `VariableId`.

  ```json
  "PropertyValues":{
    "selective.verticalClearance":{"Kind":"projectVariable","VariableId":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
    "selective.palletTolerance":{"Kind":"expression","Expression":{"Node":"add",
      "Left":{"Node":"ref","Namespace":"projectVariable","Id":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
      "Right":{"Node":"number","Value":100,"Unit":"mm"}}}}
  ```

  Mientras exista una entrada, el literal authored queda **congelado e inactivo**, igual que con una referencia
  (ADR-0034 §5; regla 20.13 de I-48).
- **P17.4 — Esquema de nodos**, común a las dos superficies:

  ```json
  {"Node":"number","Value":2.5}
  {"Node":"number","Value":100,"Unit":"mm"}
  {"Node":"ref","Namespace":"projectVariable","Id":"<guid>"}
  {"Node":"neg","Operand":{}}
  {"Node":"add","Left":{},"Right":{}}
  {"Node":"call","Function":"MAX","Args":[{},{}]}
  ```

  `add` representa también `sub`, `mul` y `div`; `{}` señala un nodo anidado. El número se persiste **tal como se
  escribió**, con su unidad: la conversión ocurre al evaluar (P9.3), así el texto editable conserva `100[mm]`.
- **P17.5 — El valor evaluado NO se persiste**, ni en el registro ni en el rack. Se deriva en cada snapshot (DR-3).
  Un valor cacheado sería una segunda autoridad capaz de quedar obsoleta, y no ayuda a las builds anteriores, que
  rechazan el kind (P18.2).
- **P17.6 — Validación ESTRUCTURAL.** En el registro, un fallo da `PresentButUnreadable`; en el rack, el binding es
  `FatalMalformedReference` (P24.6). Frontera propuesta, **revisable dentro de OPEN B**:
  - `Kind` desconocido (sin cambio, C4-8);
  - `expression` con `Value` o `VariableId`, o sin `Expression`; `literal` o `projectVariable` con `Expression`: dos
    autoridades en la misma entrada;
  - `Node` desconocido, campo obligatorio ausente o número no finito;
  - `Unit` fuera de `mm`, `in` y `ft`, o `Unit` en un nodo que no es `number`;
  - `Namespace` desconocido o `Id` no parseable;
  - `Function` desconocida;
  - límites de P1.9 superados;
  - **cualquier campo desconocido DENTRO del árbol**: a diferencia de raíz, entrada y definición, donde
    `ExtensionData` se conserva como hoy, un campo desconocido en un nodo podría cambiar su significado;
  - forma **no canónica** para su superficie (P2.8).
- **P17.7 — Los errores semánticos NO son errores de lectura.** `BrokenReference`, `Cycle`, `ArityMismatch`,
  `DivisionByZero`, `NonFiniteResult`, `DependencyFailed` y `OutOfRange` se detectan al evaluar. Su contrato es
  OPEN B.
- **P17.8** — Leer nunca escribe (Discovery §5.7). Solo escriben una `RegistryMutation` o una `RackMutation` tras
  un preflight, o el reconciliador de RACKEDITAR.
- **P17.9** — Los números se serializan con `System.Text.Json` por defecto, así que `6.0` puede reescribirse como `6`,
  igual que hoy (Discovery §5.1).
- **P17.10 — Comparadores.**
  - Nombres de propiedad JSON: sin distinguir mayúsculas, por la opción vigente `PropertyNameCaseInsensitive`
    (Discovery §5.1).
  - `Kind` de definición: `OrdinalIgnoreCase`, como `literal` hoy. `Kind` de binding: `Ordinal`, como
    `projectVariable` hoy (Discovery §13.3).
  - Tokens de `Node`, `Namespace`, `Unit` y `Function`: `Ordinal` sobre su forma canónica; cualquier otra grafía es
    error estructural.
  - `Id`: el parseo y la igualdad vigentes de `VariableId` (Discovery §4.1).
- **P17.11 — Una sola autoridad de interpretación en el rack.** La lectura de una fuente `expression` vive en
  `LinkedPropertyInspection.InspectBinding` (Discovery §4.1), no dispersa por el portador authored ni por el Plugin.
  Así se evita tocar `SelectivePalletDesignDocument.cs` más allá de lo imprescindible (§11).

### P18 — schema evolution

> **ARCHITECT REVIEW REQUIRED.** V2 propone una decisión, pero hay duda material y el Arquitecto debe confirmarla o
> cambiarla (§6).

- **P18.1 — Política real** (hechos con fuente):
  1. **Un major protege a las builds que leerían MAL.** La línea 2.0 del Selectivo existe para que una build
     anterior «REFUSES to open it… instead of silently unbinding the rack»
     (`SelectivePalletDesignDocument.cs:26-32`; decisiones de I-47, C4-9).
  2. **Un minor mayor del mismo major se preserva** al re-guardar, con sus campos desconocidos (C4.6-8, C4-9;
     `SchemaVersionPolicy.ResolveWriteVersion`, Discovery §5.4).
  3. **Un kind discriminado hace de un caso nuevo algo que NO rompe el schema**, y un kind desconocido nunca se
     ignora. Binding: «Discriminating from the first day makes that a new case instead»
     (`SelectivePropertyValueDocument.cs:11-20`). Definición: kind desconocido = ERROR, «ID22B decidirá su
     evolución» (C4-8).
  4. **Monotonía, sin oscilación por contenido**: la versión sube y nunca baja; decidir el major por el contenido
     del momento se rechaza expresamente (`src/RackCad.Application/Persistence/SelectiveDesignSchema.cs:6-19`).
  5. **Precedente en el mismo portador**: la Proposal V1.2 de I-50, congelada y con ADR-0035 aceptado en su rama
     @ `9b592e9`, añade un campo al diseño Selectivo y fija «**Ninguna `SchemaVersion` cambia.**»
     (`docs/initiatives/I-50-proposal-v1.2.md:175` en esa rama).
- **P18.2 — Qué hace hoy una build I-47/I-48** ante lo que V2 persistiría (hechos del código en `a4d88f1`):

  | Dato nuevo | Comportamiento de la build anterior | Evidencia |
  |---|---|---|
  | Registro con una definición `expression` | Registro **entero** `PresentButUnreadable`; RACKEDITAR de todo Selectivo, RACKVARIABLES y RACKBOMTOTAL bloqueados | `ProjectVariablesStore.cs:193-197`; Discovery §5.3 |
  | Rack con una fuente `expression` | `FatalMalformedReference(UnknownKind)`; el resolver da `UnknownReferenceKind`; la sonda da `Indeterminate` y el descubrimiento **aborta** | `BindingInspection.cs:205`; `SelectiveEffectiveDesignResolver.cs:174-176`; `ProjectVariableConsumerProbe.cs:74,83`; `ProjectVariableConsumerDiscovery.cs:122` |

  Ninguna lee **mal**: las dos fallan cerradas. Coste real: en una build anterior, un rack con expresión bloquea las
  operaciones de variables que necesitan descubrimiento en **todo** el dibujo (INFERENCE sobre el código citado).
- **P18.3 — Alternativas.**

  | | V-0: sin cambio de versión | V-1: minor por contenido, monótono | V-2: major |
  |---|---|---|---|
  | Registro | Reglas vigentes: nuevo `1.0`, minor mayor preservado | `1.1` al escribir la primera expresión; nunca baja | `2.0` *sticky* |
  | Diseño Selectivo | Reglas vigentes: `PropertyValues` ⇒ línea `2.0` | `2.1` al escribir la primera fuente `expression`; nunca baja | `3.0` *sticky* |
  | Build anterior con expresiones | Falla cerrada por kind (P18.2) | Igual que V-0 | Falla cerrada por versión, con el mensaje «versión más nueva» |
  | Build anterior sin expresiones | Igual que hoy | Igual que hoy (lee cualquier `1.x` y `2.x`) | Igual que hoy **solo** si nunca hubo expresiones |
  | Quitar después todas las expresiones | Vuelve a ser usable por builds anteriores | Igual que V-0 | **Nunca** vuelve a serlo |
  | Coste | Mínimo | Reglas de escritura por contenido en dos stores; **L2 pasa a ser obligatorio** (el reconciliador fija `"2.0"`, `LinkedPropertyReconciler.cs:216-218`, y degradaría un `2.1`); cambiar constantes del portador que I-50 también toca | Máximo e irreversible |
  | Señal de capacidad | Ninguna | Sí | Sí |

- **P18.4 — Propuesta: V-0.** Las expresiones son **casos nuevos de discriminadores diseñados para eso** (política
  3). Las builds anteriores ya los rechazan sin leer mal (P18.2), así que un major no protege nada (política 1). Un
  minor no añade seguridad y sí coste: arrastra L2 al alcance y toca `SelectivePalletDesignDocument.cs`, que I-50
  modificará en su G5. V-0 es además coherente con el precedente de I-50 sobre el mismo portador (política 5).
- **P18.5 — Por qué no V-1 ahora.** Su único beneficio es declarar la capacidad en la versión. Si el Arquitecto lo
  prefiere, V-1 es viable con dos condiciones: L2 corregido **dentro** de I-49 y coordinación explícita con I-50 sobre
  el portador authored.
- **P18.6 — Por qué no V-2.** Dejaría un dibujo fuera del alcance de las builds anteriores **para siempre**, aunque se
  quitaran todas las expresiones, sin evitar ninguna lectura errónea.
- **P18.7 — Cambio frente a V1.** V1 fijó `1.1` para el registro sin este contraste. V2 lo **retira** como decisión y
  lo reubica como alternativa V-1.
- **P18.8 — Mensajes.** Con V-0 o V-1, una build anterior muestra «declara una definición de clase desconocida» o una
  referencia de kind desconocido, no «versión más nueva» (Discovery §5.6). Es un coste de comunicación, no de
  integridad, y se documenta en el ADR y al usuario.
- **P18.9 — Futuro.** Todo lo nuevo **dentro** del árbol (nodos, unidades, funciones, namespaces) es fail-closed para
  esta build (P17.6), con cualquiera de las tres alternativas. Cada adición exige revisión y, si cambia
  persistencia, ADR.

### P19 — rename

**Base.** `Rename` es registry-only: sin barrido, sin racks y sin redibujo (Discovery §8.1; decisiones de I-47
C4.6-7). Hoy valida reconstruyendo un literal (Discovery §5.5, fila 10).

- **P19.1** — Sigue siendo registry-only.
- **P19.2** — Ni las definiciones ni las fuentes de propiedad cambian, porque refieren ids. El formatter muestra el
  nombre nuevo de inmediato (P4).
- **P19.3** — La validación de `Rename` conserva la definición vigente, literal o expresión, en lugar de reconstruir
  un literal.
- **P19.4** — Las reglas de nombre no cambian: no en blanco, homónimos legales y sin política de unicidad, mayúsculas
  ni caracteres (Discovery §13.1). Un rename puede volver **ambiguo** un nombre para escrituras **futuras**
  (`AmbiguousName`, OPEN A), pero nunca cambia el significado de nada persistido.
- **P19.5** — El requisito «rename no rompe» de OPEN A se cumple por construcción.
- **P19.6** — Un rename no redibuja ningún rack, aunque lo consuma una expresión: su efectivo no cambia.

### P20 — delete

**Base.** `Delete` solo cuenta consumidores propiedad→variable directos, y `UnlinkAllAndDelete` materializa los
consumidores directos (Discovery §8.1, §8.2). ADR-0034 §11 rechaza borrar materializando automáticamente.

- **P20.1 — Consumidores de X**: toda propiedad cuya fuente sea `ProjectVariableReference(X)` **o** una `Expression`
  cuyas dependencias (P11) incluyan X.
- **P20.2** — `Delete(X)` queda **bloqueado** si X tiene consumidores **o** dependientes variable→variable (P12). Se
  informan ambas listas. Sin cascada y sin materializar nada.
- **P20.3** — Los dependientes salen del snapshot acreditado **sin** barrer el dibujo (Discovery §6.3). Los
  consumidores salen del barrido.
- **P20.4 — `UnlinkAllAndDelete(X)`.** Queda bloqueado si X tiene dependientes variable→variable. Sin ellos,
  materializa **todo** consumidor de X —referencias directas **y** expresiones que la nombran— escribiendo el efectivo
  actual como literal y quitando esa fuente; el resto de propiedades del rack no cambia.
- **P20.5** — Borrar una variable definida por expresión, sin consumidores ni dependientes, está permitido y es
  registry-only.
- **P20.6** — Resultado tipado `BlockedByDependents`, o `BlockedByConsumers` ampliado con la lista tipada de
  dependientes. El nombre es ilustrativo.
- **P20.7** — `Delete` sobre un registro con errores semánticos persistidos: **OPEN B**.

### P21 — propagation

**Base.** Traza vigente de `ChangeValue` en 14 pasos (Discovery §6.1). La profundidad 1 vive en el descubrimiento, el
plan, la resolución, `Delete` y RACKVARIABLES (Discovery §6.2). `MutationDestinationBinding` aborta si una definición
aparece dos veces (Discovery §6.3). RACKVARIABLES hace hoy un descubrimiento por variable al abrir (Discovery §7).

- **P21.1** — `ChangeDefinition(X, d)` generaliza `ChangeValue` a las cuatro transiciones de una definición:
  literal→literal, literal→expresión, expresión→literal y expresión→expresión.
- **P21.2 — Preflight** (puro, todo o nada), con la cadena que exige el Owner:
  1. acreditar el registro «antes» (sin cambio);
  2. si `d` llega como texto: parse → bind → validate contra el contexto «antes» (P7);
  3. «después» = aplicar la mutación a un clon del documento acreditado, acreditarlo y evaluar sus variables (P14);
  4. rechazar si hay un ciclo que pase por X o si falla cualquier variable del cierre `I`, incluido `OutOfRange`
     (P21.7). Un ciclo persistido que no pasa por X ya existía: es OPEN B;
  5. **dependientes transitivos**: `I = {X} ∪ dependientes transitivos de X` en el grafo «después», en el orden de
     P11.1;
  6. **consumidores de rack de cualquier variable de `I`**: referencias directas **y** expresiones de propiedad
     cuyas dependencias toquen `I`, sobre **una** proyección del barrido y deduplicados por `RackId` (P21.4);
  7. **cada rack se resuelve UNA sola vez contra el estado final**: variables «después» y todas sus propiedades,
     evaluando sus expresiones (P14.8). Un solo fallo, incluido un `OutOfRange` de propiedad, vacía el plan;
  8. **un `MutationPlan` coherente**: **una** `RegistryMutation` (en el registro solo cambia X) + **una**
     `RackMutation` por rack, con efectivo completo y todas sus vistas.
- **P21.3** — Toda variable de `I` tiene que dar `Success` en «después». Qué ocurre si el snapshot contiene errores
  semánticos persistidos **fuera** de `I` es **OPEN B** (§5.5, preguntas e e i).
- **P21.4 — Descubrimiento por conjunto.** La semántica de la sonda no cambia: tri-estado, `Indeterminate` aborta,
  positivos parciales abortan y se exige autoridad `Single` (Discovery §7). Una fuente `expression` es positiva si
  sus dependencias (P11) cortan `I`; un kind desconocido o un árbol mal formado es `Indeterminate`. Se calcula
  **una vez** sobre el barrido y tiene que ser **equivalente** a la unión de descubrimientos por variable; esa
  equivalencia se prueba como oráculo (P28). Evita el coste por variable del riesgo R5.
- **P21.5 — Una transacción, un commit, un `Regen`.** La estructura del ejecutor no cambia (PREPARE / MUTATE / POST;
  Discovery §6.1) y **no** hay regen por variable intermedia (REQUISITO DEL OWNER). El ejecutor **no** parsea ni
  evalúa (guarda G13 extendida, P28.4).
- **P21.6 — Re-lectura de commit.** Se amplía la secuencia de V8-R01 de I-48, **dentro de Application**
  (`RegistryCommit.Prepare`, Discovery §3.1), no en el Plugin:

  ```text
  lastRead   = Read(...)
  accredited = Accredit(lastRead)                       // sin cambio (V8-R01)
  changed    = ApplyTo(accredited.Document)             // sin cambio
  evaluated  = Evaluate(changed) sobre el cierre I      // NUEVO
  si evaluated falla, o algún valor de I difiere del calculado en el preflight:
      Abort ANTES de TryWrite                           // registro 0, vistas 0
  si no:
      TryWrite(..., lastRead, changed, ...)             // sin cambio
  ```

  El plan transporta los valores esperados de `I` (los mismos que expone P27.2). Como el efectivo de cada rack es
  función determinista de esos valores y de su authored, que no cambia, la comparación basta. Una re-lectura que
  cambió **en otro sitio** pero conserva los valores de `I` **no** se rechaza: la política de V8-R05 queda intacta
  para lo que ya existía.
- **P21.7 — Rango del consumidor, solo para valores producidos por expresiones.**
  - Variable de `I` definida por expresión: su valor tiene que ser `> 0`, la regla que RACKVARIABLES aplica hoy a los
    literales (Discovery §10.3, §11.4).
  - Propiedad con fuente `expression`: su valor tiene que cumplir el dominio de la propiedad, hoy `>= 0` para las dos
    propiedades Selectivas, que es lo que la ventana exige al construir el diseño (Discovery §9.5). Para comprobarlo
    en Application también durante la propagación, el descriptor declara ese dominio **como dato**, nunca como
    callback.
  - Si no se cumple: `OutOfRange` y plan vacío. Los literales y las referencias directas conservan exactamente las
    reglas de hoy, así que el comportamiento de I-47/I-48 no cambia.
- **P21.8 — `Create` con expresión.** Sigue siendo registry-only, porque una variable nueva no tiene consumidores ni
  dependientes. Pero su preflight recibe y acredita el registro para enlazar y evaluar, algo que hoy `Create` no
  hace (Discovery §8.1).
- **P21.9 — Materialización.** Donde hoy se materializa un efectivo (`UnlinkAllAndDelete`, exportación a
  biblioteca y la transición a literal en el reconciliador), V2 escribe el **valor evaluado exacto**, sin redondear,
  así que el efecto geométrico sigue siendo nulo. Se acepta el ruido de coma flotante del riesgo R16.
- **P21.10 — `RepairBrokenRack`** sigue siendo rack-scoped y explícito (Discovery §8.1). Quita **toda** fuente con un
  target ausente, sea referencia directa o expresión (P24.6), y vuelve a gobernar el literal congelado, con aviso.
  Un target existente pero no evaluable **no** está ausente.
- **P21.11** — Sin límite de profundidad más allá de la aciclicidad y de P1.9, y **nunca** K planes sucesivos
  (Discovery §2.2, pregunta 16).
- **P21.12 — Edición desde RACKEDITAR.** Cambiar la fuente de una propiedad no toca el registro ni propaga a otros
  racks, porque una propiedad no es un símbolo. El reconciliador resuelve **ese** rack una vez y escribe de forma
  atómica (P23.15).

### P22 — RACKVARIABLES UX

**Base.** Comando y bucle con re-lectura en cada vuelta (Discovery §10.1). Workspace y filas con `LiteralValue`
(Discovery §10.2). Lista sin id, campos «Nombre» y «Valor (in)» y panel de referencias rotas fuera del panel
editable (Discovery §10.3). Intents con `double` (Discovery §10.4). Todo lo que asume literal, `Length` y
consumidores directos (Discovery §10.5).

- **P22.1** — **Ni comando nuevo ni ventana nueva**: los censos de 33 comandos y 29 ventanas no cambian (guardas G15
  y G16, Discovery §14.3).
- **P22.2 — Fila de la lista**: nombre · tipo · definición (literal o texto canónico de la expresión) · valor
  evaluado (`"0.###"`) · estado (correcto o código de diagnóstico) · número de consumidores (directos y por
  expresión) · número de dependientes.
- **P22.3 — Un solo campo «Definición (in)»** en lugar de «Valor (in)»:
  - un texto que **no** empieza por `=` es un literal, parseado con la regla de ADR-0015 y `> 0`, exactamente como
    hoy;
  - un texto que empieza por `=` es una expresión (P1), con unidades si hacen falta: `=AlturaBase + Holgura`,
    `=100[mm] + Base`.
- **P22.4 — Diagnóstico en vivo.** La ventana llama a una función pura de Application, expuesta por el DTO del
  workspace, que parsea y enlaza contra el snapshot del workspace y devuelve diagnósticos con posiciones. La ventana
  los muestra y **no** emite el intent mientras haya errores. La ventana **nunca** evalúa con autoridad, **nunca**
  resuelve nombres y **no** referencia tipos del núcleo (guarda, P28.4). La única autoridad sigue siendo el preflight
  del bucle.
- **P22.5 — Selector de referencias.** Una lista con nombre, valor evaluado y desambiguador, igual que los candidatos
  del editor (Discovery §13.2), inserta en el cursor la referencia inequívoca del `VariableId` elegido. **Nunca**
  auto-selecciona por el nombre tecleado. El token exacto depende de **OPEN A**.
- **P22.6 — Detalle de la variable seleccionada**: «Depende de» (dependencias directas) y «La usan» (variables
  dependientes directas, más los racks y propiedades que la consumen, directamente o por expresión).
- **P22.7 — Intents y pipeline.** `Create(nombre, textoDefinición)` y `ChangeDefinition(id, textoDefinición)`;
  `Rename`, `Delete`, `UnlinkAllAndDelete` y `RepairBroken` conservan su payload. El texto recorre parse → bind →
  validate → evaluate en el preflight, **contra el registro re-leído** y nunca contra el snapshot de la ventana, y
  solo entonces llega a la mutación atómica (P21).
- **P22.8** — Los fallos de preflight (`Cycle`, `BlockedByDependents`, `OutOfRange`, `AmbiguousName`, etc.) usan el
  mecanismo vigente del bucle (Discovery §10.1).
- **P22.9 — Panel de errores semánticos persistidos**, de variables y de fuentes de rack, fuera del panel editable
  como hoy «Referencias rotas». Qué diagnostica y qué corrige es **OPEN B**.
- **P22.10** — Pruebas de UI sin interfaz, con ids reales del catálogo y sin un `MessageBox` de producción alcanzable
  en los flujos probados.

### P23 — LinkedPropertyEditor UX

**Base.** Toda la semántica de edición vive en `LinkedPropertyEditSession`, pura, y el control solo pinta, lista,
mueve la selección y reporta el foco (Discovery §9.1). Reglas vigentes: Enter compromete literales y rechaza
consultas; LostFocus compromete solo si no cambia la fuente; `TrySelect` es la única vía de una referencia; C4 no
convierte un cambio de fuente en commit (`LinkedPropertyEditSession.cs:253-357`; Discovery §9.3–§9.4). Nueve
ubicaciones asumen que todo `=` es consulta (Discovery §9.6).

- **P23.1 — El mismo control** (REQUISITO DEL OWNER). El `LinkedPropertyEditor` de I-48 acepta en el **mismo**
  campo:

  ```text
  6
  =Holgura
  =Holgura + 2
  =(AlturaBase + Holgura) / 2
  ```

  **No** se crea un `FormulaTextBox` ni ningún control paralelo. La semántica sigue entera en la sesión pura.
- **P23.2 — Garantías que se preservan** (REQUISITO DEL OWNER):

  | Garantía | Cómo se preserva |
  |---|---|
  | texto pendiente ≠ estado comprometido | El borrador y `LinkedPropertyEditState` siguen separados |
  | teclear no muta el dominio | `Type` solo cambia el borrador: ni intent ni mutación |
  | Escape restaura el comprometido | `Cancel` vuelve exactamente al estado comprometido, como hoy |
  | LostFocus no cambia implícitamente la fuente | P23.8 |
  | C4 valida todos los borradores antes de aplicar alguno | P23.9 |
  | commit explícito parse → bind → validate → evaluate → intent → mutación atómica | P23.6 |

- **P23.3 — Modelo de fuente**: `Literal | ProjectVariableReference(VariableId) | Expression(BoundExpression)`
  (P2.7). `CommittedLiteral` sigue siendo el último literal **comprometido** y queda congelado mientras la fuente no
  es literal (regla 20.13, Discovery §9.1).
- **P23.4 — Clasificación del borrador.** `DraftReferenceQuery` se sustituye por `DraftExpression`:

  | Texto | Clase |
  |---|---|
  | Sin cambios | `Committed` |
  | Número válido, con la regla vigente del editor (P1.11) | `DraftLiteral` |
  | Empieza por `=`, tras recortar espacios iniciales como hoy | `DraftExpression` |
  | Cualquier otro | `InvalidDraft` |

  Un `DraftExpression` puede parsearse y enlazarse bajo demanda para diagnosticar y ofrecer candidatos: es puro y no
  muta nada.
- **P23.5 — Gestos.**

  | Gesto | Resultado |
  |---|---|
  | Teclear | Borrador; ninguna mutación; diagnóstico en vivo puro |
  | Lista de candidatos | Se ofrece cuando el cursor está sobre un fragmento de referencia de un borrador `=` (P23.7) |
  | Seleccionar un candidato con el borrador = `=` + fragmento | Compromete `ProjectVariableReference(X)`, **como hoy** (`TrySelect`) |
  | Seleccionar un candidato dentro de una expresión mayor | Inserta la referencia inequívoca de X en el texto, **sin** comprometer |
  | Enter con `DraftLiteral` | Compromete el literal, también desde una referencia o una expresión, como hoy |
  | Enter con `DraftExpression` | Pipeline P23.6: si pasa, compromete la forma canónica; si no, rechaza con diagnósticos y conserva el borrador |
  | Escape | Vuelve exactamente al estado comprometido |
  | Perder el foco | P23.8 |
  | Frontera C4 | P23.9 |

- **P23.6 — Pipeline de commit explícito** (REQUISITO DEL OWNER):
  1. **parse** (P1): diagnósticos de sintaxis y unidades;
  2. **bind** (P7) contra el snapshot de la sesión: el registro acreditado y su evaluación, transportados por las
     opciones de apertura (Discovery §9.1);
  3. **validate** (P7.5): existencia, aridad, unidad, ámbito, forma canónica y compatibilidad de tipo de una
     referencia directa (P8.7);
  4. **evaluate** (P14) sobre el mismo snapshot, con el dominio de la propiedad (P21.7): `OutOfRange` si no se
     cumple;
  5. **canonicalizar** (P2.8) y comprometer en la sesión, **sin** mutar el dominio;
  6. **intent**: la frontera C4 de la ventana entrega los estados finales (`LinkedPropertyFinalStates`, Discovery
     §9.4);
  7. **mutación atómica**: el reconciliador escribe authored, `PropertyValues` y efectivo en una sola
     reconciliación, re-comprobando y re-evaluando el árbol **ya enlazado por ids** —sin volver a resolver
     nombres— contra el snapshot del comando; cualquier fallo deja **cero** mutación (P23.15).
- **P23.7 — Candidatos e inserción.**
  - El filtro sigue siendo *contains* `OrdinalIgnoreCase` sobre el fragmento bajo el cursor: filtra, **no** resuelve
    (Discovery §13.3).
  - La lista muestra nombre, valor evaluado y desambiguador obligatorio (Discovery §13.2).
  - Solo se ofrecen variables compatibles cuya evaluación es `Success` (P23.11).
  - **Sin auto-selección**: nunca se completa un fragmento parcial ni se elige entre homónimos. Enter con un candidato
    **navegado** lo selecciona, como hoy (Discovery §9.3).
  - La forma exacta del token insertado es **OPEN A**.
- **P23.8 — LostFocus.** Compromete **solo** si el borrador no cambia la fuente, la regla general vigente
  (`LinkedPropertyEditSession.cs:279-300`): un `DraftLiteral` sobre una fuente literal se compromete; un
  `DraftExpression` que no enlaza, o cuya forma canónica difiere de la comprometida, se rechaza y el borrador
  sobrevive. Mover el foco a la lista no es perder el foco (Discovery §9.3).
- **P23.9 — Frontera C4.** Dos fases, como hoy: `TryStage` de **todos** los editores y después `ApplyStaged` de
  todos. Un `DraftLiteral` sin cambio de fuente da `Ready`. **Cualquier** borrador que cambie la fuente
  (literal↔expresión, referencia↔expresión, expresión→otra expresión, expresión o referencia→literal) da `Blocked`
  con «confirma con Enter», porque una frontera genérica nunca convierte un cambio de fuente en commit
  (`LinkedPropertyEditSession.cs:349-352`). Un `DraftExpression` cuya forma canónica es la comprometida da `Clean`.
  Un bloqueo aborta la frontera entera y enfoca al primer editor culpable (Discovery §9.4).
- **P23.10 — Presentación y efectivo.** Literal → `"0.###"`; referencia → `=` + referencia formateada; expresión →
  `=` + formatter (P4). El efectivo de una referencia es el valor evaluado de la variable, y el de una expresión, su
  evaluación en el snapshot de la sesión. El texto de estado bajo el campo muestra el valor y, si hay expresión, la
  fórmula, solo como presentación.
- **P23.11 — Opciones.** `LinkedPropertyOptions.ForProperty` ofrece las variables de tipo compatible cuya evaluación
  es `Success`, con su **valor evaluado** y una marca para las definidas por expresión. Las variables en error
  **no** se ofrecen; su diagnóstico vive en RACKVARIABLES (OPEN B).
- **P23.12 — Qué cambia respecto del contrato de I-48**, y que el ADR tiene que formalizar (§8):

  | Comportamiento de I-48 | V2 | Pines afectados (Discovery §14.2–§14.3) |
  |---|---|---|
  | Todo `=` es consulta de referencia que solo filtra (`IsQuery`) | `=` abre un borrador de expresión; la lista sigue filtrando fragmentos; teclear sigue sin comprometer nada | `CASO_6_UNA_CONSULTA_DE_REFERENCIA_QUEDA_PENDIENTE_Y_NO_COMPROMETE_NINGUNA_REFERENCIA` (`"="`, `"=H"`, `"=Holg"`, `"=Holgura General"`): cambia la clase esperada a `DraftExpression`; «queda pendiente y no compromete» se conserva |
  | Enter sobre `=…` siempre se rechaza (`LinkedPropertyEditSession.cs:269-271`) | Enter compromete una expresión que pasa el pipeline; un fragmento parcial, desconocido o ambiguo sigue rechazándose | Pin nuevo: una expresión válida se compromete con Enter |
  | Ni con Enter se resuelve un nombre tecleado; `TrySelect` es la única vía de una referencia | Una expresión que canoniza a `Reference(X)` también compromete la referencia | `CASO_8_ENTER_SIN_CANDIDATO_SELECCIONADO_NO_RESUELVE_POR_TEXTO` depende de **OPEN A**: se conserva si la sintaxis no admite nombres tecleados, y se reescribe («Enter resuelve un nombre exacto y único; nunca uno parcial ni ambiguo») si los admite |
  | `=Holgura General + 2` no se puede comprometer (Discovery §9.6, INFERENCE) | Se compromete si enlaza y evalúa | Pin nuevo |
  | **Se conserva**: sin auto-selección; desambiguador obligatorio; LostFocus sin cambio de fuente; C4 sin conversión silenciosa; Escape; regla 20.13 | Sin cambio | Intactos, entre otros `CASO_7_UN_FILTRO_CON_UN_UNICO_CANDIDATO_NO_LO_SELECCIONA` y `LinkedPropertyEditSessionTests.cs:55-89` y `:134` |

- **P23.13 — `RackSelectiveWindow`.** Su `Describe` usa hoy `Text.TrimStart('=')` como nombre de la variable
  (Discovery §9.6). V2 lo sustituye por el texto de estado que produce la sesión: un cambio de un miembro. `pendingAll`,
  C4 y los estados finales no cambian. Es un archivo caliente que I-50 también editará en su G3: antes de tocarlo se
  aplica el procedimiento de colisión (§11).
- **P23.14** — Sin control nuevo: el censo de 29 ventanas no cambia (G16).
- **P23.15 — Transiciones del reconciliador** (amplía Discovery §9.4):

  | Transición | Qué escribe |
  |---|---|
  | literal → expresión | `WriteAuthored(literal comprometido)` (congela) + `PropertyValues[p] = expression` + promoción de línea como hoy |
  | referencia → expresión, expresión → referencia, expresión → otra expresión | Literal congelado conservado + `PropertyValues[p]` = fuente nueva |
  | expresión → literal | `WriteAuthored(b)` y se quita la entrada, como hoy desde una referencia |
  | Cualquiera | El rack se resuelve **una** vez con todas sus propiedades finales; un fallo no muta nada |

### P24 — direct-reference compatibility

**Base.** Binding persistido `{"Kind":"projectVariable","VariableId":"<guid>"}` (Discovery §5.1). Cinco resultados de
`InspectBinding`: `Healthy`, `RepairableMissingTarget`, `FatalIncompatibleTarget`, `FatalUnknownProperty` y
`FatalMalformedReference` (Discovery §4.1). Duplicar conserva el `VariableId` y exportar materializa (Discovery §5.8).

- **P24.1 — Modelo híbrido** (REQUISITO DEL OWNER): `Literal | ProjectVariableReference(VariableId) |
  Expression(BoundExpression)`.
- **P24.2 — La referencia directa NO se elimina.** Su forma persistida, su inspección y su comprobación de tipo no
  cambian.
- **P24.3 — `=X` canónico.** Un árbol que es solo `Reference(projectVariable X)` se persiste como la referencia
  existente (P2.8): `=X` no tiene dos representaciones. Una `expression` persistida que sea solo una referencia es
  error estructural (P17.6).
- **P24.4 — Formas persistidas en el rack** (P17.3): sin entrada (literal), `projectVariable` o `expression`.
- **P24.5 — Resolución.** Referencia directa → valor evaluado de X; expresión → su evaluación (P14.8). Resolver único
  (G11).
- **P24.6 — Resultados de `InspectBinding`**, única autoridad sobre una fuente de rack (nombres ilustrativos):

  | Situación | Resultado | ¿Reparable en el rack? |
  |---|---|---|
  | Todos los ids presentes, tipo compatible y evaluación `Success` | `Healthy` | — |
  | Algún id referido ausente, en referencia o en expresión | `RepairableMissingTarget` | Sí: `RepairBrokenRack` quita la fuente y vuelve a gobernar el literal congelado (ADR-0034 §11) |
  | Referencia directa con tipo incompatible | `FatalIncompatibleTarget` | No |
  | `PropertyId` desconocido | `FatalUnknownProperty` | No |
  | Kind desconocido, id ilegible, árbol mal formado o no canónico | `FatalMalformedReference` | No |
  | Target presente pero no evaluable, o expresión que falla al evaluar (división por cero, no finito, dependencia fallida, `OutOfRange`) | **`FatalUnevaluable`** (nuevo, sexto) | No |

  Si una misma fuente cumple varias filas, gana la primera que aplique en este orden: mal formada, id ausente, tipo
  incompatible y no evaluable. Así una expresión con un id ausente es reparable aunque además no pudiera evaluarse,
  porque la reparación quita la fuente entera. **Nunca** `Healthy` ni `RepairableMissingTarget` para un fallo de
  evaluación sin ids ausentes. Hasta dónde se extiende el bloqueo más allá de ese rack es **OPEN B**.
- **P24.7 — BOM.** RACKBOMTOTAL evalúa una vez por snapshot con la misma tubería, y una fuente no evaluable **nunca**
  produce una cantidad: sin fallback. Si eso aborta el total entero o solo cuando un rack cotizado consume el cierre
  afectado es **OPEN B** (§5.5, pregunta d). Hoy una referencia rota aborta el total (ADR-0034 §10).
- **P24.8 — Duplicación y restamp.** La copia conserva `PropertyValues`, entradas `expression` incluidas, con los
  mismos `VariableId` (ADR-0034 §12; Discovery §5.8). I-51 cambió en `f8cf4c9` la entrada del restamp en el Plugin;
  V2 no toca ese camino y prueba en un archivo **propio** que una entrada `expression` sobrevive al restamp (§11).
- **P24.9 — Exportación a biblioteca.** Materializa el efectivo evaluado, quita `PropertyValues` y escribe
  literal-only en la línea `1.0`, como hoy (Discovery §5.8).
- **P24.10 — Autoridad multi-vista.** La comparación estructural de árboles persistidos incluye `PropertyValues`
  (Discovery §7): dos vistas hermanas con expresiones distintas son `Divergent` y fallan cerradas, igual que hoy con
  referencias distintas.
- **P24.11 — Dibujos de I-47/I-48.** Con solo literales y referencias directas, dan los mismos efectivos, conservan
  sus formas persistidas y no se reescriben al abrir. En una build anterior, un dibujo con expresiones falla cerrado
  (P18.2).

### P25 — ID20 extension point

**OWNER INPUT.** I-49 **no** implementa `Rack.Frentes`, `Rack.Niveles`, `Rack.Altura`, `Project.TotalRacks` ni
`Project.TotalFrentes`; prepara `SymbolId`, scopes/namespaces, `ExpressionContext`, resolución de símbolos y los
datos que necesita el evaluador.

- **P25.1 — Entregado por I-49**: `SymbolId`, `SymbolNamespace` (solo `projectVariable` activo), `SymbolScope` (solo
  `Project`), `SymbolTable`/`SymbolEntry`, `ExpressionContext`, `SymbolResolver`, evaluador sobre datos puros y el
  contexto de evaluación de una propiedad de rack (P6.7).
- **P25.2 — Reservado conceptualmente, sin implementación productiva** (REQUISITO DEL OWNER): `Rack.*` y `Project.*`,
  el ámbito `Rack` y un caso de definición para valores calculados. La reserva consta en el ADR; **ningún** camino
  productivo los crea, el binder responde `UnknownNamespace` y una prueba fija que `Rack.Frentes` y
  `Project.TotalRacks` **no** resuelven en producción.
- **P25.3** — La regla de ámbito de P5.3 se fija con una prueba del núcleo sobre entradas **sintéticas de test**, sin
  crear ningún símbolo `Rack` productivo.
- **P25.4** — Las expresiones de propiedad son el consumidor natural de un futuro ámbito `Rack`: el contexto de P6.7
  es donde ID20 añadiría símbolos del rack. V2 no añade ninguno.
- **P25.5 — Frontera de datos para ID20.** Los valores calculados tendrán que venir de snapshots puros proyectados
  desde el barrido del Plugin, igual que `ProjectVariableScanEntry` (Discovery §16.2). El núcleo **nunca** lee
  AutoCAD, sistemas de Domain ni catálogos.
- **P25.6 — Lo que ID20 tendrá que resolver, y V2 no**: `Rack.Frentes` es un conteo **por fondo**, `Rack.Niveles`
  admite al menos dos lecturas, `Rack.Altura` requiere catálogo y redondea al pie, no existe ningún agregado
  `Project.*` y `RackCount` solo cuenta racks con BOM (Discovery §16.2).
- **P25.7** — Las preguntas de frontera de ID20 son del Owner y de ID20 (contrato §12), no de esta Proposal.

### P26 — ID23 extension point

**OWNER INPUT.** ID23 debe poder reutilizar parser, evaluador y contexto **sin depender de ProjectVariables**.

- **P26.1** — El núcleo no depende de `ProjectVariables`, `Persistence`, `Systems.*`, `Bom`, `Catalogs`,
  `StructuralSections`, Domain, UI, Plugin ni AutoCAD. Lo fija una guarda de fuente (P28.4), y la dirección de
  dependencia es de Project Variables hacia el núcleo, **nunca** al revés.
- **P26.2 — Superficie reutilizable**: parsear, enlazar con un contexto que aporta quien llama, comprobar, extraer
  dependencias, grafo, ciclos, orden, evaluar, formatear y diagnosticar.
- **P26.3** — La autoridad de unidades (P9.5) es igual de neutral y reutilizable.
- **P26.4 — Lo que añadiría ID23, y V2 no**: sus namespaces y ámbitos, un resultado entero y la semántica de redondeo
  para `Quantity` (hoy `int`, Discovery §16.3; P10.6), la persistencia de fórmulas de BOM y su propio ADR.
- **P26.5** — Sin ensamblado separado en V2 (§9, ALT-17): la guarda marca la frontera.
- **P26.6** — Ningún tipo de I-49 lleva conceptos de BOM, y no se persiste ninguna fórmula de BOM.

### P27 — ID28/29 information preservation

**OWNER INPUT**, precisado por el encargo de G2A.1. Hoy la procedencia se pierde en cuanto el resolver escribe el
número en Domain, y nada del impacto se persiste (Discovery §16.4).

- **P27.1 — Se conserva, por snapshot y en memoria**, para cada variable y cada fuente `expression` de rack:
  - `BoundExpression`;
  - `SymbolId` de cada referencia;
  - diagnósticos (P15);
  - **Dependencies**: dependencias directas (P11);
  - **Dependents**: dependientes variable→variable (P12.2) y consumidores de rack del plan (P21.2);
  - traza opcional (P16.3).
- **P27.2** — El resultado del preflight expone el **cierre impactado** (variables con valor antes y después) y los
  racks con sus propiedades, directas o por expresión, **antes** de ejecutar; hoy solo expone los racks (Discovery
  §16.4). V2 lo usa en los mensajes de RACKVARIABLES.
- **P27.3** — La cadena de Explain se deriva **sin** re-evaluar: propiedad → fuente (referencia o expresión) →
  `SymbolId` → `EvaluationResult` con traza opcional, en el mismo snapshot.
- **P27.4** — **Nada se persiste**: ni trazas, ni grafo, ni valores.
- **P27.5** — El determinismo de P14.5 hace reproducibles las trazas.
- **P27.6** — Sin UI de ID28 ni de ID29 en I-49 (P30).

### P28 — tests

**Base.** Marco xUnit sin mocks; el Plugin solo se verifica con guardas de texto (Discovery §14.1). Inventario por
área y escenarios de ID22B que faltan (Discovery §14.2). Veinte guardas y pines que un motor tocaría (Discovery
§14.3). Lección de I-48: un criterio sobre tokens puede certificar una implementación corrupta
([Proposal V2 de I-48](I-48-proposal-v2.md), R-01).

- **P28.1** — El **criterio de aceptación es de comportamiento**. Las guardas de fuente son defensa secundaria y
  nunca el criterio.
- **P28.2 — Evidencia por gate**: RED demostrado en local antes de GREEN para todo comportamiento nuevo; suites focal
  y de impacto; las **dos** suites en local sobre el Candidato; CI verde sobre el SHA exacto (AGENTS.md; WORKFLOW §4
  y §5).
- **P28.3 — Familias de pruebas.**

  | Gate | Qué se prueba |
  |---|---|
  | G5 | Lexer y parser por tabla (válidos, inválidos, posiciones); unidades (`[mm]`, `[in]`, `[ft]`, `UnknownUnit`, `UnitNotAllowedHere`, `UnitSyntaxNotSupported`); `AmbiguousDecimalComma`; límites; orden de diagnósticos; formatter canónico y *round-trip* P2.5 sobre un corpus |
  | G6 | Resolver (desconocido, ambiguo, namespace); **motor adimensional**: `6 + 2`, `A + 2`, `A * B`, `A / B`, `100[mm] + 2` y `MAX(A, 4[in])` aceptadas con valores exactos; conversiones bit a bit (`4[in]` = 4, `20[ft]` = 240, `100[mm]` = 100 ÷ 25.4); paridad con `StructuralSectionUnits.InchesToMillimeters`; `MIN`, `MAX` y `ABS` (aridad y valores); semántica numérica; regla de ámbito sintética; `Rack.*` y `Project.*` no resuelven; tabla de canonicalización P2.8 |
  | G7 | Extracción sin repetición y ordenada; grafo con diamante, autorreferencia, ciclo de 2, componente mayor y aristas rotas; orden independiente de permutaciones; cortocircuito `DependencyFailed` |
  | G8 | Round-trip del store real de definiciones `expression` y de fuentes `expression` en `PropertyValues`; cada regla estructural de P17.6, formas no canónicas incluidas; comportamiento de versión de la alternativa que fije §6; **fixture dorado**: el JSON literal-only que escribe la build nueva es idéntico al de `a4d88f1`; los seis resultados de `InspectBinding` para los dos kinds; resolver con fuentes `expression`; RACKBOMTOTAL con un solo snapshot; restamp que conserva una entrada `expression`; exportación que la materializa |
  | G9 | Las cuatro transiciones; cierre impactado; consumidores por expresión; una `RegistryMutation`; racks deduplicados y resueltos una vez; `Delete` y `UnlinkAllAndDelete` con consumidores por expresión y con dependientes; ciclo rechazado; `OutOfRange` de variable y de propiedad; `Create` y `Rename` con expresiones; aborto en commit antes de `TryWrite`; oráculo: descubrimiento por conjunto = unión por variable |
  | G10 | Sesión del editor: todos los gestos de P23.5 con éxito y con fallo del pipeline; las garantías de P23.2 con borradores `=`; `=X` persiste lo mismo que seleccionar X; `=6` lo mismo que `6`; inserción de candidatos; sin auto-selección; reconciliador (P23.15); workspace y ventana de RACKVARIABLES; pruebas del control sin interfaz |
  | G11 | Pruebas reales de cadena X → Y → expresión de propiedad → geometría y firma de BOM sobre catálogo real; un rack con una propiedad por referencia y otra por expresión; suites de I-47/I-48 verdes, sin más cambios de aserción que los de P28.4 |

- **P28.4 — Evolución explícita de guardas y pines** (Discovery §14.3). Se hace en el **mismo** gate que el
  comportamiento, con RED→GREEN, y **nunca** renombrando tipos para esquivarlas:

  | Guarda o pin | Evolución propuesta | Gate |
  |---|---|---|
  | G1 `NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS` | `RackPropertyReference`, `rackProperty` y `FormulaParser` siguen prohibidos en Application, Plugin y UI; `ExpressionParser` y `DependencyGraph` pasan a permitirse **solo** en Application (núcleo y adaptadores) y siguen prohibidos en Plugin y UI | G5, G7 |
  | G2 `LA_DEFINICION_DE_UNA_VARIABLE_SIGUE_TENIENDO_UN_SOLO_CASO` | Se sustituye: exactamente `Literal = 1` y `Expression = 2`; `Formula = ` sigue prohibido | G8 |
  | G3 `"expression"` = error duro | Se sustituye: `"expression"` válido solo con árbol válido y canónico; kinds desconocidos (`"formula"`, `"futureKind"`) siguen siendo error duro | G8 |
  | **Nueva** — kinds de binding | Exactamente `projectVariable` y `expression`; los fixtures `rackProperty` y `futureKind` siguen fallando cerrados (Discovery §15, búsqueda 10) | G8 |
  | G4 Domain sin variables | Sin cambio; se añade: Domain sin tokens del núcleo | G5 |
  | G5 capa pura sin AutoCAD | Se extiende al núcleo y a la autoridad de unidades | G5 |
  | G6–G10 gramática de `VariableType` | Sin cambio | — |
  | G11 y G12 resolver único y BOM | Sin cambio; se añade: RACKBOMTOTAL no parsea ni evalúa | G8 |
  | G13 ejecutor | Se añaden `ExpressionParser`, `ExpressionEvaluator` y `DependencyGraph` como ausentes | G9 |
  | G14 comando RACKVARIABLES | Sin cambio; se añade: el comando no evalúa | G9 |
  | G15 y G16 censos | Sin cambio: 33 comandos y 29 ventanas | — |
  | G17 catálogo cerrado | Sin cambio: dos propiedades | — |
  | G18 Plugin y UI no interpretan vínculos | Se extiende: sin tipos del núcleo en Plugin ni UI; `RackSelectiveWindow.xaml.cs` sigue sin `VariableId`, resolver ni parser | G10 |
  | G19 el control no compara nombres | Sin cambio | — |
  | G20 pines de `=` | Evolucionan **exactamente** según P23.12 | G10 |
  | **Nueva** — independencia del núcleo | El núcleo y la autoridad de unidades no nombran `ProjectVariables`, `Persistence`, `Systems`, `Bom`, `Catalogs`, `StructuralSections`, `RackCad.Domain`, `RackCad.UI`, `RackCad.Plugin` ni `Autodesk` | G5 |
  | **Nueva** — sin comprobador dimensional | El criterio son las pruebas de P5.6; defensa secundaria: el núcleo no declara tipos ni códigos dimensionales | G6 |
  | **Nueva** — una autoridad de unidades | Tokens y factores de unidad solo en la autoridad neutral; ningún parser de unidades en UI, Domain ni Plugin | G6 |

- **P28.5** — V2 no exige refactorizar los fixtures duplicados que el Discovery contó (≈25 clases, Discovery §14.1).
  Las pruebas nuevas pueden introducir un único constructor de registros y fuentes con expresiones.
- **P28.6** — Checklist de validación del Owner: §7.3.

### P29 — migration

**Base.** Leer nunca escribe y re-escribir no conserva bytes, pero sí versión, `ExtensionData` y contenido
(Discovery §5.7).

- **P29.1** — **Sin migración de datos**: ni registros, ni bindings de rack, ni bibliotecas se reescriben al abrir.
- **P29.2** — Sin conversión automática de literales o referencias en expresiones, y sin detección de «valores
  iguales».
- **P29.3 — Versiones.** Con V-0, la primera expresión **no** cambia ninguna versión y quitar todas las expresiones
  devuelve el dibujo a lo que una build anterior puede usar. Si el Arquitecto elige V-1, la primera expresión promueve
  a `1.1` o `2.1` y nunca se degrada (P18).
- **P29.4** — En una build anterior, un dibujo con expresiones falla cerrado (P18.2). Se documenta en el ADR y en la
  comunicación al usuario; sin herramienta de degradación (P30).
- **P29.5** — La compatibilidad se verifica en G12 con el DLL de `a4d88f1`, sobre tres dibujos: sin expresiones, con
  expresiones en el registro y con expresiones de propiedad.
- **P29.6** — Los caminos heredados `SelectiveBindingOptions.ForLength` y `BindingIntent`, sin disparador en producción
  (Discovery §8.1), **no** se extienden (riesgo R15).

### P30 — non-goals

- **P30.1** — ID21: referencias rack→rack. `RackPropertyReference` y `rackProperty` siguen prohibidos.
- **P30.2** — ID20 productivo: símbolos `Rack.*` y `Project.*`, parámetros calculados y ámbito `Rack`; solo quedan
  reservados (P25.2).
- **P30.3** — ID23: Custom / Calculated BOM, cantidades enteras y fórmulas de BOM.
- **P30.4** — UI de ID28 Explain y de ID29 Impact Preview.
- **P30.5** — Sistema de dimensiones físicas, comprobador dimensional, tipos de área, volumen, masa o ángulo y nuevos
  `VariableType` (BLOCKER 3).
- **P30.6** — Unidades distintas de `[mm]`, `[in]` y `[ft]`; unidades sobre referencias, llamadas o paréntesis;
  sintaxis de pies-pulgadas o de fracciones; unidades en literales sin `=`; conversión del DWG.
- **P30.7** — Funciones distintas de `MIN`, `MAX` y `ABS`: `ROUND`, `CEILING` y `FLOOR` quedan diferidas (P10.6), e
  `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings, trigonometría y macros, excluidos (P10.7).
- **P30.8** — Un `FormulaTextBox` o cualquier control de entrada paralelo (P23.1).
- **P30.9** — Parsers o evaluadores externos (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) y
  cualquier dependencia NuGet (ADR-0012).
- **P30.10** — Grafo, valores evaluados, trazas o texto tecleado persistidos.
- **P30.11** — Sintaxis localizada (`;` como separador y `,` decimal) y completar texto sin selección explícita.
- **P30.12** — Control de concurrencia más allá de P21.6.
- **P30.13** — Transferencia o fusión de variables entre dibujos (decisiones de I-47, C2-8).
- **P30.14** — Política de unicidad o de mayúsculas de nombres, y renombrado de variables existentes.
- **P30.15** — Comandos o ventanas nuevos, otros sistemas de rack (Dinámico, Push Back, Cama, Cantilever) y nuevas
  propiedades vinculables.
- **P30.16** — Herramientas de degradación o de exportación a versiones anteriores.
- **P30.17** — Corregir los hallazgos laterales L1–L5 (Discovery §7, §5.8, §8.1, §11.5). Se registran en
  `docs/ideas-futuras.md` (contrato §4); WORKFLOW §8 pide hacerlo «al detectarlo», pero cada gate documental estuvo
  limitado a un único archivo, así que **el registro sigue pendiente** y se asigna a G4 como muy tarde. **Excepción**:
  si el Arquitecto elige V-1, L2 entra en el alcance (P18.5).
- **P30.18** — Hacer converger los parsers de literales del editor y de RACKVARIABLES (Discovery §11.2).
- **P30.19** — Cambiar invariantes de I-47/I-48 más allá de lo que enumera §2.3: es condición de parada, no alcance.

---

## 4. OPEN FOR ARCHITECT A — Duplicate-name source syntax

```text
Estado    = OPEN FOR ARCHITECT (se mantiene desde V1)
V2        = NO fija la sintaxis exacta de identidad cualificada
Congelado = el requisito semántico (abajo)
```

### 4.1 Requisitos CONGELADOS

Texto del encargo de G2A.1:

1. **homónimo termina en VariableId inequívoco**
2. **Name no es autoridad**
3. **formatter puede representar fórmula editable**
4. **rename no rompe**

Queda abierta **la sintaxis exacta**, no estos cuatro requisitos.

### 4.2 Hechos que acotan la decisión

- Los homónimos son **legales** y no hay política de unicidad, mayúsculas ni normalización; Application no recorta
  y la ventana central sí (Discovery §13.1).
- El único desambiguador visible hoy son los **8 primeros caracteres** del `VariableId`, y solo en la lista de
  candidatos del editor. El campo, el estado bajo el campo y la lista de RACKVARIABLES no muestran id (Discovery
  §13.2).
- Ese fragmento es un **prefijo** del texto guardado: dos ids con el mismo primer grupo hexadecimal se verían
  iguales (Discovery §13.3, INFERENCE).
- **Ninguna** ruta de producción resuelve nombre→id, y una guarda lo protege en el control (Discovery §13.3).
- Un filtro *contains* sin distinguir mayúsculas no sirve como regla de coincidencia exacta dentro de una fórmula
  (Discovery §13.4, INFERENCE).
- Los nombres admiten espacios («Holgura General») y ningún carácter está prohibido (Discovery §13.1, §13.4).
- **Nuevo en V2**: las fórmulas también se teclean en el `LinkedPropertyEditor` (P23), y los corchetes ya tienen un
  uso fijado por el Owner, el sufijo de unidad `[mm]` (P1.6).

### 4.3 Lo que V2 ya fija con independencia de la sintaxis

- Todo lo persistido lleva **ids**, nunca nombres (P17), así que el requisito 4 se cumple por construcción (P19.5).
- El binder es la **única** ruta nombre→id y solo corre al comprometer (P7.3).
- **Nunca** se elige entre homónimos ni se completa un fragmento parcial (P7.1, P23.7).
- El formatter es el único productor de texto editable (P4.2), en las dos superficies.
- `=X` y la selección de X persisten la misma referencia (P2.8).

El comparador de nombres de P7.2 es una propuesta **revisable dentro de A**.

### 4.4 Preguntas que la decisión tiene que responder

a. Forma textual de una referencia **no** ambigua.

b. Forma cualificada de un homónimo y qué la cualifica: fragmento de id, id completo u otra cosa, incluida la
   longitud y qué pasa si coincide con cero ids o con varios.

c. Cómo se escriben nombres con espacios, operadores, paréntesis, comas, corchetes o los propios delimitadores, y si
   hay escape.

d. Cómo se evita cualquier ambigüedad con el sufijo de unidad `[mm]` (P1.6).

e. Qué emite el formatter cuando un nombre es único y qué emite cuando pasa a ser ambiguo tras un `Create` o un
   `Rename`. En particular, si la forma mostrada puede depender del estado **actual** del registro.

f. Cómo se muestra una referencia rota (P4.5).

g. Si un nombre **tecleado** puede enlazarse al comprometer, o si una referencia solo entra por selección en la
   lista. De esta respuesta depende `CASO_8` (P23.12).

h. Sensibilidad a mayúsculas del nombre y del cualificador (P7.2).

i. Interacción con nombres de función (`MIN`, `MAX`, `ABS`) y con los namespaces reservados `rack` y `project`
   (P5.2).

### 4.5 Familias ilustrativas

No son exhaustivas, **ninguna es preferida**, y cada una se evalúa contra los mismos cuatro requisitos y los mismos
hechos de §4.2:

- **A-1** — nombre sin delimitar cuando es único; forma cualificada solo para homónimos.
- **A-2** — nombre siempre delimitado; cualificador solo para homónimos.
- **A-3** — referencia siempre cualificada por identidad en el texto, con presentación decorada.
- **A-4** — referencias solo insertables por selección, como token opaco, sin tecleo libre de identidad.

### 4.6 Qué depende de A, y qué no

- **Depende**: P1.2 (producción `reference`), P4.2 y P4.5 (forma de las referencias), P7.1–P7.2 (entrada del
  resolver), P22.5 y P23.7 (token insertado), P23.12 (`CASO_8`) y las pruebas de sintaxis de G5, G6 y G10.
- **No depende**: persistencia, versión, grafo, ciclos, evaluación, unidades, funciones, propagación, `Delete`,
  modelo híbrido ni puntos de extensión.

---

## 5. OPEN FOR ARCHITECT B — Structural readability vs semantic evaluability

```text
Estado    = OPEN FOR ARCHITECT (se mantiene desde V1)
V2        = NO fija el contrato exacto
Congelado = el requisito (abajo)
```

### 5.1 La cuestión

El contrato exacto cuando una expresión persistida es **estructuralmente legible** —P17.6 la acepta— pero contiene
`BrokenReference`, `Cycle` u otro error semántico persistido (`ArityMismatch`, `DivisionByZero`, `NonFiniteResult`,
`DependencyFailed`, `OutOfRange`). En V2 puede ocurrir en las **dos** superficies: una definición del registro o una
fuente `expression` de un rack.

### 5.2 Requisitos CONGELADOS

Texto del encargo de G2A.1:

1. **geometry/BOM jamás fallback**
2. **RACKVARIABLES debe poder diagnosticar/corregir estados semánticamente rotos que esta build puede leer
   estructuralmente**

### 5.3 Hechos que acotan la decisión

- Hoy un registro presente e ilegible bloquea **todo el dibujo**: RACKEDITAR de cualquier Selectivo, RACKVARIABLES,
  RACKBOMTOTAL, opciones de vínculo y commits de registro (Discovery §5.3).
- La identidad ambigua bloquea en la acreditación y en la re-lectura de commit (Discovery §4.2).
- Las referencias rotas de rack se muestran en un panel **fuera** del panel editable, con reparación rack-scoped que
  se puede usar aunque el registro no sea editable (Discovery §10.3).
- Un fallo de cualquier binding deja al rack **sin efectivo**, porque el resolver valida todo antes de escribir
  (Discovery §4.2), y RACKEDITAR no abre ese rack: `SelectiveEditorOpen` bloquea la apertura si algo no resuelve
  (Discovery §9.1).
- ADR-0034 §10: una referencia rota **aborta** el total de BOM en vez de degradarse.
- Decisiones de I-47, C4-8: **nunca** ignorar datos que la build no entiende.
- **INFERENCE** — Con V2, la build nueva no escribiría ninguno de estos estados: su preflight rechaza ciclos (P13.3),
  bloquea borrar con consumidores o dependientes (P20.2) y exige éxito en el cierre impactado y en cada rack
  resuelto (P21.2). Solo podrían aparecer por escrituras que no pasan por ese preflight: ediciones externas del NOD
  o del sobre, otra build o un defecto.

### 5.4 Lo que V2 ya fija con independencia de B

- Un símbolo en error, **todos** sus dependientes transitivos y las propiedades que los consumen no producen valor
  (P13.4, P14.3, P14.8).
- **Sin** fallback a literal, a cero ni a un valor anterior (ADR-0034 §8 y §11; DR-6).
- Catálogo de códigos (P15) y separación entre ilegibilidad estructural y error semántico (P17.6–P17.7).
- Un fallo de evaluación en un rack, sin ids ausentes, es `FatalUnevaluable`: nunca `Healthy` ni reparable por
  rack (P24.6).

La frontera concreta de P17.6 es **revisable dentro de B**.

### 5.5 Preguntas que la decisión tiene que responder

a. **Alcance del bloqueo**: todo el dibujo, solo el cierre afectado (variables en error, sus dependientes
   transitivos y los racks que los consumen) u otro.

b. Resultado de nivel registro en ese estado: ¿`Usable` con resultados fallidos por símbolo, o un resultado nuevo de
   nivel registro?

c. RACKEDITAR de un Selectivo que **no** consume nada afectado, de uno que **sí**, y de uno cuya propia fuente
   `expression` falla.

d. RACKBOMTOTAL: ¿aborta el total entero, o solo si algún rack cotizado consume el cierre afectado o tiene una fuente
   en error?

e. Qué intents admite RACKVARIABLES en ese estado —solo correctivos sobre lo que está en error, cualquiera cuyo
   cierre impactado quede limpio, u otros— y cómo se garantiza que una corrección nunca produzca un plan parcial.

f. Cómo se presenta y se corrige un ciclo persistido de varias variables, donde cambiar una sola definición puede
   bastar.

g. Cómo diagnostica y corrige RACKVARIABLES una fuente `expression` de rack que falla al evaluar, y su relación con
   `RepairBrokenRack` y con `FatalUnevaluable` (P24.6).

h. Si algún caso de P17.6 debe cambiar de lado, por ejemplo la aridad o la forma no canónica.

i. Qué hace la re-lectura de commit (P21.6) si contiene errores semánticos **fuera** del cierre impactado.

### 5.6 Familias ilustrativas

No son exhaustivas y **ninguna es preferida**:

- **B-1** — Bloqueo de nivel dibujo, equivalente a ilegible para todo consumidor, con RACKVARIABLES en modo de
  diagnóstico que solo admite intents correctivos.
- **B-2** — Bloqueo limitado al cierre afectado: el resto del dibujo opera con normalidad y RACKVARIABLES admite
  correcciones.
- **B-3** — Híbrido: geometría y BOM se bloquean por cierre, y las mutaciones se limitan a correctivas mientras exista
  cualquier error.

### 5.7 Qué depende de B

P17.6 (frontera), P20.7, P21.3 y P21.6 (errores fuera del cierre), P22.9, P23.11 (opciones), P24.6–P24.7 y las
pruebas de esos estados en G8, G9 y G10.

---

## 6. Revisión explícita del Arquitecto sobre decisiones propuestas

A diferencia de OPEN A y OPEN B, aquí V2 **sí propone** una decisión.

### 6.1 ARCHITECT REVIEW REQUIRED — versión de schema (P18)

- **Propuesta**: V-0, sin cambio de versión en el registro ni en el diseño Selectivo; las builds anteriores fallan
  cerradas por kind.
- **Por qué hay duda material**: V1 fijó `1.1`; el encargo de G2A.1 menciona `1.1` y `2.1` como candidatas; V-1 da una
  señal de capacidad que V-0 no da, a cambio de arrastrar L2 y tocar el portador authored que I-50 modificará.
- **Qué debe devolver el Arquitecto**: V-0, V-1 o V-2. Si elige V-1, L2 entra en el alcance (P30.17) y la coordinación
  con I-50 sobre `SelectivePalletDesignDocument.cs` se vuelve obligatoria (§11).

### 6.2 Focos de revisión

Decisiones propuestas sin duda material declarada, en las que una segunda lectura tiene más valor:

| Foco | Propuesta de V2 | Punto |
|---|---|---|
| Canonicalización al leer | Una forma no canónica persistida es error estructural, nunca normalización silenciosa | P2.8, P17.6 |
| Dominio del consumidor | `> 0` para variables definidas por expresión y `>= 0` para las dos propiedades, declarado como dato del descriptor y aplicado solo a valores de expresiones | P21.7 |
| Frontera C4 del editor | Todo borrador que cambie la fuente se bloquea; uno canónicamente igual al comprometido es `Clean` | P23.9 |
| Autoridad de unidades | Componente neutral propio con paridad probada frente a `StructuralSectionUnits`; delegación de las constantes existentes fuera de V2 | P9.5–P9.7 |
| Alias en definiciones | `AlturaFinal = AlturaBase` es una `expression` de una sola referencia | P2.8 |

---

## 7. Gates G0–G12

> **Propuestos, NO autorizados.** Solo se abre el gate siguiente con evidencia revisable del anterior (contrato
> §8). La división de G5–G10 puede ajustarse en la revisión.

### 7.1 Secuencia

| Gate | Nombre | Entregable | Evidencia exigida | Estado |
|---|---|---|---|---|
| **G0** | Reclamo + bootstrap | Reclamo atómico `77262fe`; contrato, decisión del Owner y fila de ROADMAP `f2d28a2` | Push aceptado sin force | **HECHA** |
| **G1** | Discovery read-only | [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1` | CI 4/4 en ese SHA | **HECHA** |
| **G2** | Proposal | V1 `b15e40a` (historial, no aceptada para revisión) → **V2** (G2A.1) → las versiones que pida la revisión, hasta una congelada con A, B y §6.1 cerradas | Architect Reviews | **EN CURSO** (G2A.1) |
| **G3** | Consenso + freeze | `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal, más la decisión del Owner | Registro en `decisions/I-49.md` | pendiente — **compuerta** |
| **G4** | ADR + alineación documental | ADR aceptado por el Owner, con número asignado **en ese momento**; contrato (§1, §3.2, §11.2) y ROADMAP alineados con la Proposal congelada; L1–L5 y el seguimiento de P9.7 registrados en `ideas-futuras.md` | ADR en estado `aceptado` | NO AUTORIZADA |
| **G5** | Núcleo sintáctico | Lexer con unidades, parser, sintaxis, formatter, límites y diagnósticos de sintaxis; guardas G1 (parcial), G4, G5 e independencia del núcleo | RED→GREEN; focal + impacto | NO AUTORIZADA |
| **G6** | Núcleo semántico | `SymbolId`, namespaces, ámbitos, `ExpressionContext`, tabla de símbolos, `SymbolResolver` y binder con la sintaxis de A, autoridad de unidades, `FunctionRegistry` (`MIN`, `MAX`, `ABS`), evaluador adimensional y `EvaluationResult`; guardas de unidades y sin comprobador dimensional | RED→GREEN | NO AUTORIZADA |
| **G7** | Núcleo de grafo | Extracción de dependencias, grafo, ciclos, orden y evaluación por snapshot; guarda G1 completa | RED→GREEN | NO AUTORIZADA |
| **G8** | Persistencia + integración | `VariableDefinition.Expression`, fuente `expression` en `PropertyValues`, stores y validación estructural (P17), versión según §6.1, evaluación tras acreditación, target evaluado, `InspectBinding` con seis resultados, resolver con expresiones de propiedad, contrato de B, BOM, restamp y exportación; guardas G2, G3, G12 y kinds de binding | RED→GREEN; fixture dorado literal-only | NO AUTORIZADA |
| **G9** | Mutaciones y propagación | `ChangeDefinition`, `Create` y `Rename` con expresiones, `Delete` y `UnlinkAllAndDelete` con consumidores por expresión y con dependientes, cierre impactado, descubrimiento por conjunto con oráculo, un resolve por rack, re-lectura de commit (P21.6); guardas G13 y G14 | RED→GREEN | NO AUTORIZADA |
| **G10** | UX | Sesión y control del `LinkedPropertyEditor` con expresiones (P23), reconciliador (P23.15), RACKVARIABLES (P22) y `Describe` de la ventana Selectiva tras el procedimiento de colisión; guardas G18 y G20 | RED→GREEN; suite UI | NO AUTORIZADA |
| **G11** | Candidato | Pruebas reales de cadena hasta geometría y BOM; las dos suites en local; builds Debug de UI y Plugin; CI verde sobre el SHA exacto; medición de coste de la propagación (R5) | Clases de evidencia de AGENTS.md y WORKFLOW §4–§5 | NO AUTORIZADA |
| **G12** | Owner Validation + integración | Checklist de §7.3 en AutoCAD 2025; integración **serializada** con I-50; cierre documental (HANDOFF y ROADMAP); CI posterior al merge | WORKFLOW §4.5 | NO AUTORIZADA |

### 7.2 Reglas de la secuencia

- G5, G6 y G7 **no** tocan la producción de Project Variables, ni el Plugin, ni la UI: el núcleo nace aislado, y solo
  evolucionan las guardas que P28.4 asigna a esos gates. G8 es el primer gate que toca producción de Project Variables
  o de persistencia, y G10 el primero que toca UI.
- Antes de editar **cualquier** archivo productivo, cada gate de G5 a G10 ejecuta el procedimiento de colisión de
  [`decisions/I-49.md`](../automation/decisions/I-49.md) §8 contra I-50 y la misma comprobación contra I-51, que ya
  tiene producción (§11).
- El Plugin se toca solo donde G9 o G10 lo exijan, archivo por archivo y con la comprobación anterior.
- Un gate que descubra que necesita cambiar una invariante de I-47/I-48 más allá de §2.3 **se detiene** (§12).

### 7.3 Checklist propuesto de Owner Validation (G12, AutoCAD 2025)

1. Crear `AlturaBase` literal y `AlturaFinal = AlturaBase + Holgura` por expresión; comprobar el valor evaluado en
   RACKVARIABLES.
2. En RACKEDITAR, escribir en el **mismo** campo `6`, `=Holgura`, `=Holgura + 2` y `=(AlturaBase + Holgura) / 2`,
   confirmando cada uno con Enter; geometría y BOM reflejan el valor.
3. Teclear una expresión y perder el foco: **no** se aplica y el borrador sigue ahí. Pulsar Escape: vuelve lo
   comprometido.
4. Con dos campos con borradores, uno inválido, pulsar Actualizar: **no** se aplica ninguno y el foco va al culpable.
5. Unidades: `=100[mm] + 2`, `=20[ft]` y `=4[in]` dan los valores esperados en pulgadas.
6. `=Holgura` y seleccionar `Holgura` en la lista dejan **el mismo** vínculo guardado.
7. Cambiar `AlturaBase`: se redibujan los racks de `AlturaFinal` y los que usan una expresión con ella, con **un**
   solo `Regen`.
8. Renombrar `AlturaBase`: las expresiones muestran el nombre nuevo, sin redibujo.
9. Intentar borrar `AlturaBase` con dependientes o con consumidores por expresión: bloqueado, con las listas.
10. Intentar crear un ciclo, o escribir una expresión con error de sintaxis: rechazado, sin mutación.
11. Homónimos: la referencia queda ligada al `VariableId` elegido, con la sintaxis que fije A.
12. Guardar, cerrar y reabrir: definiciones, fuentes, valores y vínculos intactos.
13. Con el DLL de `a4d88f1`: un dibujo sin expresiones funciona como hoy; uno con expresiones en el registro o en un
    rack falla cerrado, sin escritura.
14. RACKBOMTOTAL con racks ligados por referencia y por expresión.
15. Estados semánticos rotos preparados como fixture: se comportan según el contrato de B.

---

## 8. ADR REQUIRED — número NO asignado

- **Por qué es obligatorio.** V2 cambia contratos persistidos (P17), extiende ADR-0034 (§2.3) y toma decisiones de
  arquitectura (DR-1 a DR-6). WORKFLOW §8 y el contrato §3.1 exigen que el ADR esté **aceptado antes de implementar**.
- **Número NO asignado.** Se asigna en G4 contra la numeración remota de ese momento. ADR-0035 es de I-50 y ya está
  **aceptado** en su rama (`9b592e9`). I-49 no reserva ningún número de antemano.
- **No se escribe en G2A.1.**
- **Lo que tendrá que explicar de forma explícita**, tras G3:
  - **la extensión de ADR-0034 que exigen las fórmulas en fuentes de propiedad**: un tercer caso
    `Expression(BoundExpression)` junto a `Literal(T)` y `ProjectVariableReference(VariableId)` (ADR-0034 §4), por
    qué ID22B extiende también el lado de la propiedad que §15 asociaba a ID21, cómo quedaría un futuro caso de ID21 y
    que la referencia directa se conserva con `=X` canónico;
  - las dos superficies de expresión y la revisión del contrato del editor de I-48 (P23.12);
  - el motor adimensional y el papel de `VariableType` como metadata del consumidor (DR-5);
  - la sintaxis y la autoridad neutral de unidades, con ADR-0005 intacto (P9);
  - el conjunto de funciones inicial y los candidatos diferidos (P10);
  - la versión de schema que resulte de §6.1, con su efecto sobre las builds anteriores (P18);
  - la frontera estructural/semántica y el contrato de B (§5);
  - la sintaxis de A (§4);
  - la gramática numérica invariante como excepción acotada a ADR-0015 (P1.11);
  - los namespaces y el ámbito reservados para ID20 (P25.2);
  - las alternativas rechazadas (§9.1).
- **Relación con los ADR vigentes.** Extiende ADR-0034; no sustituye ADR-0005, ADR-0006, ADR-0012, ADR-0015 ni
  ADR-0021. ADR-0034 está aceptado y es **inmutable**: no se edita.
- **Contrato de I-49.** Su §11.2 y su §12 prevén exactamente esta vía («salvo ADR nuevo aceptado por el Owner»). La
  implementación sigue bloqueada hasta que ese ADR esté aceptado, y el texto del contrato se alinea en G4.

---

## 9. Alternativas y coste

### 9.1 Alternativas consideradas

| # | Alternativa | Decisión en V2 |
|---|---|---|
| ALT-1 | Expresiones **solo** en `ProjectVariable.Definition` (DR-1 de V1) | **Rechazada**: contradice el requisito del Owner de dos superficies (BLOCKER 1) |
| ALT-2 | Un **`FormulaTextBox`** o control paralelo para fórmulas | **Rechazada** por requisito del Owner: se usa el mismo control de I-48 (P23.1) |
| ALT-3 | Cada expresión de propiedad como **variable implícita** del registro (opción b de Discovery §9.7) | **Rechazada**: llena el registro de variables que nadie creó, con identidad y ciclo de vida por rack, y hace que rename y delete dependan de propiedades |
| ALT-4 | Fuente de propiedad **solo `Expression`**, sin canonicalizar `=X` | **Rechazada**: `=X` tendría dos representaciones persistidas (requisito del Owner, P2.8) |
| ALT-5 | Persistir el **texto con nombres** | **Rechazada**: un rename rompería las expresiones y el nombre pasaría a ser autoridad (ADR-0034 §2, HANDOFF §4) |
| ALT-6 | Persistir **texto canónico con ids** en una cadena | Viable. V2 prefiere el árbol JSON para no acoplar la persistencia a la sintaxis de OPEN A ni exigir un parser en la lectura. El Arquitecto puede reabrirlo |
| ALT-7 | **Cachear el valor evaluado** | **Rechazada**: segunda autoridad que puede quedar obsoleta; no ayuda a las builds anteriores (P17.5) |
| ALT-8 | **Persistir el grafo** de dependencias | **Rechazada**: derivable, y persistido podría divergir (P12.1) |
| ALT-9 | **Sistema de tipos dimensional** (`Length \| Scalar`, `DimensionMismatch`), como proponía V1 | **Rechazada** por requisito del Owner (BLOCKER 3) |
| ALT-10 | Conversión de unidades vía **`StructuralSectionUnits`** | **Rechazada**: haría depender el núcleo de un helper de catálogo (Discovery §12.5; P9.7) |
| ALT-11 | Un **parser de unidades por capa** (UI, evaluador, Domain) | **Rechazada** por requisito del Owner: un lexer y una autoridad (P1.10, P9.5) |
| ALT-12 | **`ROUND`, `CEILING` y `FLOOR`** ya en V2 | **Diferida**: semántica abierta (P10.6) |
| ALT-13 | Versión **V-1** (minor por contenido) o **V-2** (major) | Ver P18; bajo **ARCHITECT REVIEW REQUIRED** (§6.1) |
| ALT-14 | Evaluador de **terceros** (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) | **Rechazada**: ADR-0012; semántica sensible a cultura o no acotada (P1.10) |
| ALT-15 | Núcleo **dentro** de `RackCad.Application.ProjectVariables` | **Rechazada**: arrastraría esa dependencia a ID23, contra OWNER INPUT (Discovery §16.3) |
| ALT-16 | **Borrado en cascada** o materialización automática de dependientes | **Rechazada**: reescribe en silencio definiciones del usuario; ADR-0034 §11 ya rechaza la versión de consumidores (P20.2) |
| ALT-17 | Núcleo en un **ensamblado separado** | **Diferida**: frontera más dura, pero añade un proyecto a builds, CI y despliegue del Plugin; V2 usa namespace + guarda (P26.5) |
| ALT-18 | Gramática **localizada** (`,` decimal y `;` separador) | **Rechazada**: doble significado de la coma (Discovery §11.7); V2 lo mitiga con `AmbiguousDecimalComma` (P1.5) |

### 9.2 Coste estimado (INFERENCE, se valida gate a gate)

| Área | Superficie estimada | Base |
|---|---|---|
| Núcleo nuevo | ≈14–20 archivos en `RackCad.Application.Expressions`, más la autoridad de unidades | P1–P16, P9.5 |
| Project Variables y persistencia | ≈20–26 archivos existentes: las ubicaciones que asumen literal; sesión, fuente, estado, opciones y reconciliador del editor; inspección, kernel y sonda; descubrimiento, preflight, commit, workspace e intents; `SelectivePropertyValueDocument` | Discovery §5.5, §9.7, §3.1–§3.2 |
| Selectivo en Application | ≈3: resolver, apertura del editor y exportación | Discovery §3.2 |
| Plugin | 0–3 | Discovery §3.3 |
| UI | ≈4: `LinkedPropertyEditor` (inserción de candidatos), `RackProjectVariablesWindow.xaml` y `.cs`, texto de reparación; `RackSelectiveWindow`, un miembro | Discovery §3.4; P23.13 |
| Pruebas | Núcleo nuevo, ≈20 archivos existentes y la evolución de guardas y pines de P28.4 | Discovery §14 |
| Comandos / ventanas nuevos | 0 / 0 | P22.1, P23.14 |

**Costes que no se minimizan:**

- Nace la **primera** ruta productiva nombre→id, ahora en dos superficies (P7.3).
- Se revisa el contrato del editor de I-48 que el Owner validó (P23.12).
- El contrato de B puede ampliar la UX de errores.
- El commit re-evalúa el cierre impactado (P21.6).
- En una build anterior, un rack con expresión bloquea en todo el dibujo las operaciones de variables que necesitan
  descubrimiento (P18.2).
- La propagación sobre dibujos grandes sigue **sin medir** hasta G11 (ADR-0034, riesgos).
- Evolucionan varias guardas y pines vigentes, y nacen guardas nuevas (P28.4).
- `RackSelectiveWindow`, archivo caliente compartido con I-50, necesita al menos un cambio (P23.13).

---

## 10. Reconciliación con el Discovery G1

### 10.1 Las 23 preguntas abiertas (Discovery §18)

| # | Pregunta del Discovery | Respondida en |
|---|---|---|
| 1 | Dónde vive una expresión | DR-1; P2.7–P2.8; P23; P24 |
| 2 | Representación persistida | P4; P17.2–P17.4; ALT-5, ALT-6 |
| 3 | Versión del registro | P18; §6.1 |
| 4 | Token y payload del nuevo kind; `ExtensionData`; `PropertyNameCaseInsensitive` | P17.2–P17.4, P17.6, P17.10 |
| 5 | Evaluación: dónde, cuándo y si se persiste | DR-3; P14.2, P14.7–P14.8; P17.5; P21.2, P21.6 |
| 6 | Grafo y ciclos | P12; P13; P15.3 |
| 7 | Propagación transitiva | P21 |
| 8 | Ciclo de vida con dependientes | P19; P20; P21; **OPEN B** |
| 9 | Tipos | DR-5; P5.4–P5.6; P8.7–P8.8 |
| 10 | Unidades | P1.6; P9 |
| 11 | Gramática numérica | P1.4–P1.5, P1.11; P22.3 |
| 12 | Nombres dentro de expresiones | P7; **OPEN A** |
| 13 | Editor | P23 |
| 14 | RACKVARIABLES | P22 |
| 15 | ID20 (preparar) | P5.2–P5.3; P25 |
| 16 | ID23 (independencia) | DR-2; P26 |
| 17 | ID28/ID29 (preservar) | P16.3; P27 |
| 18 | Guardas de fuente | P28.4 |
| 19 | Compatibilidad de DWG I-47/I-48 | P18.2; P24.11; P28.3 (G8); P29; §7.3 (13) |
| 20 | ADR | §8 |
| 21 | Alcance de sistemas y propiedades | DR-2; P28.4 (G17); P30.15 |
| 22 | Semántica numérica del resultado | P14.4; P21.7 |
| 23 | Hallazgos laterales L1–L5 | P30.17; G4 |

### 10.2 Los 16 riesgos (Discovery §17)

| Riesgo | Tratamiento en V2 |
|---|---|
| R1 — legibilidad en builds anteriores | Fail-closed por kind, sin lectura errónea (P18.2); coste declarado; verificación con el DLL de `a4d88f1` (§7.3, punto 13) |
| R2 — segunda autoridad de valor | Una evaluación por snapshot y expresiones de propiedad dentro del resolver único; sin valores cacheados (DR-3; P14.7–P14.8; P17.5; P24.5) |
| R3 — guardas de fuente | Evolución explícita, en el mismo gate y sin renombrar para esquivarlas (P28.4) |
| R4 — profundidad 1 embebida | Cierre transitivo, consumidores de cualquier variable afectada, un resolve por rack y un plan (P21.2) |
| R5 — coste sin medir | Descubrimiento por conjunto en un barrido (P21.4); grafo `O(V + E)` (P12.6); medición en G11 |
| R6 — nombres en fórmulas | Binder como única ruta nombre→id, solo al comprometer, sin elegir entre homónimos (P7); sintaxis en **OPEN A** |
| R7 — gramática numérica divergente | Gramática invariante con `AmbiguousDecimalComma`; los literales sin `=` conservan su regla (P1.4–P1.5, P1.11); convergencia fuera de alcance (P30.18) |
| R8 — unidades | `[mm]`, `[in]` y `[ft]` como conversión numérica con una autoridad neutral (P9) |
| R9 — acoplamiento al Selectivo | Núcleo y autoridad de unidades neutrales, protegidos por guarda (DR-2; P26) |
| R10 — semántica actual de `=` | **Revisada por requisito del Owner**, con las garantías de I-48 preservadas y la lista exacta de pines que cambian (P23.2, P23.12) |
| R11 — pérdida de trazabilidad | `BoundExpression`, `SymbolId`, diagnósticos, dependencias, dependientes y traza opcional en memoria (P27) |
| R12 — colisión potencial con I-50 | Sin tocar los portadores `From`/`WithDesign`; `RackSelectiveWindow` limitado a un miembro; V-0 evita cambiar constantes del portador; procedimiento de colisión (P17.11; P23.13; P18.4; §11) |
| R13 — defectos laterales heredados | Fuera de alcance, con registro pendiente, salvo L2 si se elige V-1 (P30.17) |
| R14 — extensión muerta | Pruebas por el camino operativo store → acreditación → evaluación → resolver → geometría y BOM, en las dos superficies (P28.3, G8 y G11) |
| R15 — caminos heredados sin disparador | No se extienden (P29.6) |
| R16 — materializar valores calculados | Valor evaluado exacto, sin redondeo, con el ruido de coma flotante aceptado (P21.9) |

---

## 11. Coordinación con I-50 e I-51

**Observado en el preflight de G2A.1** (`git fetch --all --prune`, 2026-09-12):

```text
origin/architecture/motor-expresiones-parametricas = b15e40a076d7157ce8ca73537af9659049bf8576  (= Proposal V1)
origin/main                                        = a4d88f18a1f42263d366c44dc05dd18a6786f152
I-50  origin/feature/cotas-independientes-por-vista  = 9b592e934e9293612de255b543f81943933bb6bd
I-51  origin/feature/rackduplicar-multiples-origenes = f8cf4c9f2024f3494d977f4bfbd2c2f9816b8c73
```

### 11.1 I-50

- **FACT** — Desde V1 avanzó dos commits, los dos solo en `docs/`: `0e91c52` (Proposal V1.2) y `9b592e9` (congela el
  consenso sobre V1.2 y registra ADR-0035 como **aceptado**). Su siguiente gate es G4, no iniciado. **Sin colisión
  productiva real.**
- **FACT (doc de I-50)** — Su plan congelado toca `SelectivePalletDesignDocument.cs` solo en `From` y `ToDomain`,
  «nunca `PropertyValues` ni `WithDesign`», en su G5, y las tres ventanas ricas, `RackSelectiveWindow` incluida, en su
  G3, tras el protocolo de coordinación con I-49 (`docs/initiatives/I-50-proposal-v1.2.md:627-640` en su rama).
- **Cruces potenciales con V2:**

  | Archivo | I-49 V2 | I-50 | Tratamiento |
  |---|---|---|---|
  | `RackSelectiveWindow.xaml.cs` | Un miembro, `Describe` (P23.13), en G10 | `BuildDesign` / `LoadDesign` en su G3 | Procedimiento de colisión antes de editar; integración serializada |
  | `SelectivePalletDesignDocument.cs` | **Ninguno** con V-0: la lectura de `expression` vive en `InspectBinding` (P17.11) | `From` / `ToDomain` en su G5 | Con V-1 habría cambio de constantes y se aplicaría el procedimiento |
  | `SelectivePropertyValueDocument.cs`, editor vinculable, Project Variables y núcleo | Sí | No: I-50 declara que «no cambia» `LinkedPropertyEditor`, Project Variables ni Expression Engine (`I-50-proposal-v1.2.md:58-59` en su rama) | Sin cruce |

- **Condiciones de `OWNER_OVERRIDE_I49_I50_PARALLEL`**: G2A.1 solo crea este documento, así que la condición de
  colisión **no se activa**.

### 11.2 I-51

- **FACT** — Desde V1 avanzó a `f8cf4c9` («I-51 G4: restamp con Guid del llamador, PREPARE fuera de la mutacion y
  guardas G-R1..G-R6»). Su producción acumulada frente a `main`: `src/RackCad.Application/Persistence/RackDuplicationPlan.cs`,
  `src/RackCad.Plugin/RackDuplicarCommands.cs`, `src/RackCad.Plugin/RackEnvelopeRestamp.cs` y tres archivos de
  pruebas, entre ellos `SelectiveDuplicationFailClosedTests.cs`.
- **FACT** — Su planificador usa `SelectiveAuthoredAuthority.IsSameAuthority`, `SelectivePalletDesignStore`,
  `SelectivePalletDesignDocument` y `RackEmbedDocument`, y sus pruebas T14–T15 fijan que dos vínculos reales
  sobreviven al restamp.
- **Sin colisión productiva real**: G2A.1 no toca producción, y V2 no propone cambiar ninguno de esos archivos ni esas
  API. La prueba de que una entrada `expression` sobrevive al restamp (P24.8) va en un archivo **propio** de I-49.
- **Condición de parada**: si un gate de I-49 necesitara cambiar la API o la semántica de
  `SelectiveAuthoredAuthority`, del restamp, del portador o del store del Selectivo, o esos archivos de pruebas, se
  detiene **antes de editar** y lo reporta (contrato §12). El override **no** cubre este par.

### 11.3 Conflictos documentales previstos

- Las tres ramas insertan su fila de ROADMAP tras I-48.
- I-50 e I-51 modifican `docs/ideas-futuras.md`, donde I-49 registrará L1–L5 y el seguimiento de P9.7 (G4).
- Numeración de ADR: 0035 es de I-50, ya aceptado en su rama (§8).

La integración es **serializada**, y la segunda en integrar se reconcilia con `main`.

---

## 12. Condiciones para detenerse

Se heredan todas las del contrato §12. Sobre la que dice «salir del punto de extensión de ADR-0034 §15: detenerse; eso
es ADR nuevo y decisión del Owner», V2 deja constancia de que **la decisión del Owner existe** (encargo de G2A.1) y de
que **el ADR es obligatorio** antes de cualquier producción (§8). Además:

- Si cerrar **A**, **B** o **§6.1** exige **cambiar** una invariante de I-47/I-48 más allá de lo que enumera §2.3:
  detenerse; eso es ADR y decisión del Owner.
- Si el núcleo o la autoridad de unidades no pueden quedar **independientes** de Project Variables y de
  `StructuralSections` (P26.1): detenerse.
- Si la evolución de una guarda debilitara la protección de **ID21** (P28.4): detenerse.
- Si aceptar expresiones en el editor exigiera renunciar a alguna garantía de P23.2: detenerse. No se relaja una
  garantía para acomodar la implementación.
- Si la medición de G11 muestra un coste de propagación inaceptable: detenerse y escalar.
- Colisión productiva material con I-50 o con I-51: detenerse **antes de editar** (§11).
- Cualquier edición de producción antes de G3 y del ADR aceptado en G4: **prohibida**.

---

## 13. Estado

```text
COORDINATOR PROPOSAL V2 — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator    = PROPOSED (V2)
Architect      = PENDING — Architect Review READY, todavía NO solicitada
Implementation = BLOCKED
ADR            = REQUIRED (número NO asignado)
Proposal V1    = b15e40a — historial, NO aceptada para Architect Review

V1 → V2 (§0):  BLOCKER 1 property expressions en el mismo LinkedPropertyEditor   CORREGIDO
               BLOCKER 2 unidades [mm] [in] [ft] con una autoridad neutral          CORREGIDO
               BLOCKER 3 motor adimensional, sin tipado dimensional                  CORREGIDO
               BLOCKER 4 MIN, MAX, ABS; ROUND, CEILING, FLOOR evaluados y diferidos  CORREGIDO

30 puntos      = P1..P30 (§3)
Gates          = G0..G12 (§7): G0 y G1 HECHAS · G2 EN CURSO · G3 PENDIENTE · G4..G12 NO AUTORIZADAS

OPEN FOR ARCHITECT:
  A. Duplicate-name source syntax                            (§4)
  B. Structural readability vs semantic evaluability         (§5)
ARCHITECT REVIEW REQUIRED:
  versión de schema: V-0 propuesta frente a V-1 / V-2        (§6.1, P18)

Base: Discovery G1 @ 4cf02b167f183fb66d93988c9843296838277dc4
      Proposal V1  @ b15e40a076d7157ce8ca73537af9659049bf8576 (historial)

Siguiente paso: Architect Review sobre V2. G2A.1 no la solicita.
```

**Historial conservado:** [V1](I-49-proposal-v1.md) queda **intacta**.
