# I-49 — Proposal V1: motor de expresiones paramétricas (Expression Engine, ID22B)

> # ⚠ COORDINATOR PROPOSAL V1 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Coordinator    = PROPOSED
> Architect      = PENDING
> Implementation = BLOCKED
> ADR            = REQUIRED (número NO asignado)
> ```
>
> Primera Proposal de I-49, gate **G2A**. Somete a revisión una arquitectura completa del motor de
> expresiones en **30 puntos** (§2), una secuencia de gates **G0–G12** (§5) y **dos cuestiones marcadas
> `OPEN FOR ARCHITECT`** que esta versión **no decide** (§3 y §4). La revisión del Arquitecto **no se ha
> solicitado todavía**.
>
> ```text
> Discovery G1     = 4cf02b167f183fb66d93988c9843296838277dc4   (I-49-discovery.md)
> Código auditado  = a4d88f18a1f42263d366c44dc05dd18a6786f152   (origin/main; la rama no contiene producción)
> Contrato         = I-49-motor-expresiones-parametricas.md
> Override vigente = OWNER_OVERRIDE_I49_I50_PARALLEL   (docs/automation/decisions/I-49.md)
> I-50 observado   = 0d7670c1c1e3de6257270d9723d925c1b687d378   (solo docs/)
> I-51 observado   = 4c79e4a46b2adf45aee87ffb399c786f25d5d274   (G3: primer código productivo; ninguno en el plan de I-49, §9.2)
> Estado de gates  = G0 HECHA · G1 HECHA · G2A EN CURSO (esta V1) · G3 PENDIENTE
> ```

---

## 0. Cómo leer esta Proposal

### 0.1 Qué es y qué no es

- **Es** la Proposal V1 del Coordinador: el contrato de trabajo que se somete a revisión. Cada regla
  está **propuesta**; ninguna está decidida.
- **No es** consenso, **no** autoriza implementación, **no** es un ADR y **no** modifica el Discovery. La
  compuerta **NO IMPLEMENTATION BEFORE CONSENSUS** del contrato (§12) sigue vigente.
- **Parte de hechos, no de hipótesis.** Toda afirmación sobre el código vigente remite al
  [Discovery G1](I-49-discovery.md), auditado sobre `4cf02b1` con el código de `a4d88f1`. Donde el Discovery
  clasificó algo como INFERENCE, aquí sigue siendo INFERENCE.

### 0.2 Convenciones

| Marca | Significado |
|---|---|
| **Pn.m** | Regla propuesta *m* del punto *n* de §2. Solo sería vinculante tras G3 |
| **DR-n** | Decisión rectora propuesta (§1). Condiciona varios puntos |
| **Discovery §x** | Hecho verificado en G1; la cita remite a la sección *x* de [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1`, que contiene las rutas y líneas exactas |
| **OWNER INPUT** | Aclaración vinculante del Owner recibida en G1 (ID20, ID23, ID28/ID29). No es un hecho del código |
| **OPEN FOR ARCHITECT** | Cuestión que V1 deja abierta a propósito (§3, §4). Sus requisitos semánticos están **congelados**; su forma exacta no |
| **INFERENCE** | Estimación o deducción, no hecho |
| Nombres de tipos y miembros | **Ilustrativos.** El contrato es el comportamiento, no el nombre |

### 0.3 Entradas vinculantes

- **Contrato de I-49** — [`I-49-motor-expresiones-parametricas.md`](I-49-motor-expresiones-parametricas.md):
  la capacidad vive en `ProjectVariable.Definition` (§1 y §11.2), las invariantes de I-47/I-48 no se
  reabren (§3.2) y quedan fuera Custom BOM, ID20 productivo e ID21 (§4).
- **[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)**, aceptado e inmutable: §15
  designa `ProjectVariable.Definition` como punto de extensión de ID22B y reserva `PropertyValue<T>` para
  ID21.
- **OWNER INPUT de G1** (Discovery §1): ID20 = Computed / Built-in Parameters (`Rack.Frentes`,
  `Rack.Niveles`, `Rack.Altura`, `Project.TotalRacks`, `Project.TotalFrentes`), que I-49 **prepara sin
  implementar**; ID23 = Custom / Calculated BOM (`Guías.Quantity = Rack.Frentes * 2`), que debe poder
  reutilizar parser, evaluador y contexto **sin depender de ProjectVariables**; ID28 Explain / Trace e
  ID29 Impact Preview, cuya información hay que **preservar**.
- **`OWNER_OVERRIDE_I49_I50_PARALLEL`** — [`decisions/I-49.md`](../automation/decisions/I-49.md), con su
  procedimiento de colisión (§8 de ese registro).

---

## 1. Decisiones rectoras y arquitectura en una página

### 1.1 Decisiones rectoras propuestas

- **DR-1 — Las expresiones viven SOLO en la definición de una variable de proyecto.** Una propiedad de
  rack sigue siendo `Literal(T)` o `ProjectVariableReference(VariableId)`, con la misma persistencia
  (`PropertyValues`, kind `projectVariable`). Resuelve la tensión documental que el Discovery dejó abierta
  (Discovery §9.7) con su opción **(c)**: expresiones solo en definiciones de variable. Fundamento: ADR-0034
  §15 y contrato §11.2; la dirección tipo Excel de HANDOFF §4 es **no normativa** y «no autoriza» adelantar
  nada (`docs/HANDOFF.md:1591-1594`).
- **DR-2 — Núcleo neutral.** Parser, binder, evaluador, grafo y formatter viven en un namespace propio de
  Application (`RackCad.Application.Expressions`, nombre ilustrativo), puro y **sin dependencia** de Project
  Variables, persistencia, sistemas, BOM, Domain, UI, Plugin ni AutoCAD. Project Variables es **cliente**
  del núcleo; nunca al revés. Es la condición de OWNER INPUT para ID23 (Discovery §16.3).
- **DR-3 — Una sola evaluación por snapshot acreditado, dentro de Application y antes del resolver
  único.** Geometría, BOM y preview siguen consumiendo **solo el efectivo** (ADR-0034 §6); el resolver
  único se conserva (Discovery §4.2; guarda G11 en Discovery §14.3).
- **DR-4 — Identidad por id en todo lo persistido.** Las expresiones persisten `VariableId`, nunca nombres.
  El nombre solo interviene al **escribir** una expresión (binder) y al **mostrarla** (formatter).
- **DR-5 — Fail-closed tipado en toda la cadena** (ADR-0034 §8): cada fallo es un resultado tipado, un plan
  fallido es vacío y no existe fallback a literal, a cero ni a un valor anterior.

### 1.2 Tubería propuesta

```text
NOD RACKCAD_PROJECT (Plugin)          único lector/escritor físico                      (sin cambio)
  → ProjectVariablesStore             legibilidad ESTRUCTURAL: el árbol de expresión     (P17, P18)
                                      está bien formado
  → UsableProjectVariablesRegistry    identidad: VariableId único                        (sin cambio)
  → Evaluación del registro           tabla de símbolos, grafo, ciclos, orden y valores  (P6–P16)
                                      → un EvaluationResult por variable
  → SelectiveEffectiveDesignResolver  resolver único; consume valores EVALUADOS          (P24)
  → geometría / BOM / preview         solo efectivo                                      (sin cambio)
```

```text
RackCad.Application.Expressions  (núcleo neutral, DR-2)
  texto          → lexer/parser             → árbol de sintaxis (con posiciones)         (P1, P2)
  sintaxis       → binder + SymbolResolver  → BoundExpression (ids, funciones)           (P5, P7, P10)
  BoundExpression→ comprobación semántica   → tipos, aridad, existencia y ámbito         (P5, P7)
  BoundExpression→ extracción de dependencias → grafo → ciclos → orden                   (P11–P14)
  orden + tabla  → evaluador                → EvaluationResult + traza                   (P14, P16)
  BoundExpression→ formatter                → texto editable                             (P4)
```

### 1.3 Lo que V1 extiende, y lo que no toca

V1 **no cambia** ninguna invariante del contrato §3.2. **Extiende** cinco: cuatro de ADR-0034, por su punto
de extensión §15, y una de las decisiones de I-48. Por eso exige ADR (§6):

| Invariante vigente | Extensión propuesta | Punto |
|---|---|---|
| ADR-0034 §3: definición **literal** | Definición `Literal` **o** `Expression`; `VariableType` sigue siendo solo `Length` | P2, P5, P9 |
| ADR-0034 §7: propagación de **profundidad 1** | Cierre transitivo en **un** plan, **un** commit y **un** `Regen` | P21 |
| ADR-0034 §11: `Delete` bloqueado **con consumidores** | Bloqueado también con **dependientes variable→variable** | P20 |
| ADR-0034 §13 y C4-8: `Definition.kind` desconocido = ERROR; «ID22B decidirá su evolución» | Nuevo kind `expression` en **minor `1.1`**, condicionado al contenido | P17, P18 |
| Decisiones de I-48 §4 (V8-R01): re-acreditación de la re-lectura de commit | La re-lectura de commit también **re-evalúa** el cierre impactado | P21 |

**No se toca**: autoridad y nodo del registro, identidad `VariableId`, persistencia de bindings de rack,
authored ≠ effective, autoridad multi-vista, `RepairBrokenRack` rack-scoped, identidad ambigua
fail-closed, mapping único del `Type`, la semántica de `=` en `LinkedPropertyEditor` y el protocolo C4.

---

## 2. Los 30 puntos

### 2.0 Tabla de control

| # | Punto | Propuesta en una línea | Estado |
|---|---|---|---|
| 1 | grammar | Aritmética `+ - * /`, unario, paréntesis y llamadas; números invariantes con `.`; `,` solo separa argumentos | PROPOSED · producción `reference` → **OPEN A** |
| 2 | AST/model | Tres modelos inmutables: sintaxis, `BoundExpression` persistible y resultado | PROPOSED |
| 3 | identity | `VariableId` intacto; `SymbolId = (namespace, key)`; la expresión no tiene identidad propia | PROPOSED |
| 4 | source text | Texto tecleado ≠ árbol persistido ≠ texto del formatter; solo el árbol es autoridad | PROPOSED · referencias → **OPEN A** |
| 5 | symbol model | Namespace `projectVariable`, ámbito `Project`, tipos internos `Length` y `Scalar` | PROPOSED |
| 6 | ExpressionContext | Valor inmutable por operación, construido desde UN snapshot acreditado | PROPOSED |
| 7 | SymbolResolver/binder | Única ruta nombre→id de producción, solo al escribir; nunca elige entre homónimos | PROPOSED · sintaxis → **OPEN A** |
| 8 | project-variable symbol binding | Tabla de símbolos desde `UsableProjectVariablesRegistry`, mapping único del `Type` | PROPOSED |
| 9 | units | Pulgadas internas, sin sufijos ni conversión; coherencia `Length`/`Scalar` | PROPOSED |
| 10 | function registry | Registro cerrado en código; V1 = `min` y `max` | PROPOSED |
| 11 | dependency extraction | Función pura, sin nombres ni evaluación, orden determinista | PROPOSED |
| 12 | dependency graph | Derivado por snapshot, nunca persistido; solo variable→variable | PROPOSED |
| 13 | cycle detection | Componentes fuertemente conexas, iterativo y determinista; ciclo = plan vacío | PROPOSED · persistidos → **OPEN B** |
| 14 | evaluation order | Topológico con desempate determinista; evaluación ansiosa y memoizada | PROPOSED |
| 15 | diagnostics | Catálogo cerrado de códigos tipados; mensajes en una sola capa | PROPOSED |
| 16 | EvaluationResult | `Success`/`Failed` por símbolo + `RegistryEvaluation` + traza en memoria | PROPOSED |
| 17 | persistence | `{"Kind":"expression","Expression":<árbol>}` sin valor cacheado | PROPOSED · frontera estructural revisable en **OPEN B** |
| 18 | schema evolution | Minor `1.1` solo si hay expresiones; nunca degrada; literal-only sigue en `1.0` | PROPOSED |
| 19 | rename | Registry-only; las expresiones no cambian; el formatter muestra el nombre nuevo | PROPOSED |
| 20 | delete | Bloqueado por consumidores **o** dependientes; sin cascada | PROPOSED · estado de error → **OPEN B** |
| 21 | propagation | `ChangeDefinition` con cierre transitivo, un plan y re-evaluación en commit | PROPOSED |
| 22 | RACKVARIABLES UX | Campo «Definición», diagnóstico en vivo, selector de referencias, dependencias | PROPOSED · token → **OPEN A** · panel de errores → **OPEN B** |
| 23 | LinkedPropertyEditor UX | `=` sigue siendo consulta; opciones con valor evaluado; sin expresiones en el campo | PROPOSED |
| 24 | direct-reference compatibility | Bindings de rack intactos; efectivo = valor evaluado; sexto outcome tipado | PROPOSED · alcance del bloqueo → **OPEN B** |
| 25 | ID20 extension point | `SymbolId`, namespaces, ámbitos y contexto listos; `rack`/`project` reservados y no activos | PROPOSED |
| 26 | ID23 extension point | Núcleo sin dependencia de Project Variables, protegido por guarda | PROPOSED |
| 27 | ID28/29 information preservation | Traza, grafo inverso y cierre impactado en memoria; nada persistido | PROPOSED |
| 28 | tests | Criterio de comportamiento; guardas como defensa; evolución explícita de G1–G20 | PROPOSED |
| 29 | migration | Sin migración de datos; promoción a `1.1` en la primera expresión | PROPOSED |
| 30 | non-goals | 17 exclusiones explícitas | PROPOSED |

### P1 — grammar

**Base.** No existe ningún parser de expresiones en el repositorio (Discovery §15, búsqueda 1). La coma ya
tiene dos significados: decimal en la regla localizada de ADR-0015 y separador de lista en otras
superficies (Discovery §11.7). El editor vinculable admite exponente y RACKVARIABLES no (Discovery §11.3).

- **P1.1 — Lenguaje.** Expresiones aritméticas sobre números, referencias y llamadas a función. Sin
  sentencias, asignaciones, comparaciones, condicionales, booleanos ni cadenas.
- **P1.2 — Gramática** (EBNF; la producción `reference` es **OPEN FOR ARCHITECT A**, §3):

  ```ebnf
  expression     = additive ;
  additive       = multiplicative , { ( "+" | "-" ) , multiplicative } ;
  multiplicative = unary , { ( "*" | "/" ) , unary } ;
  unary          = ( "+" | "-" ) , unary | primary ;
  primary        = number | call | reference | "(" , expression , ")" ;
  call           = function-name , "(" , [ expression , { "," , expression } ] , ")" ;
  number         = digit , { digit } , [ "." , digit , { digit } ] ;
  function-name  = letter , { letter | digit } ;
  reference      = (* OPEN FOR ARCHITECT A — §3 *) ;
  ```

- **P1.3 — Precedencia.** `*` y `/` sobre `+` y `-`; asociatividad por la izquierda; el unario liga más
  fuerte que cualquier binario.
- **P1.4 — Números.** Forma invariante con `.` como único separador decimal. Sin exponente, sin
  agrupadores, sin punto inicial ni final (`.5`, `5.`), sin signo dentro del número (el signo es operador
  unario) y sin `NaN` ni `Infinity`. Coincide con ADR-0015 en rechazar exponente y agrupadores.
- **P1.5 — Coma.** `,` es **solo** separador de argumentos. Una coma entre dos dígitos (`1,5`) es el error
  léxico `AmbiguousDecimalComma`: **nunca** se lee como decimal ni como dos argumentos. Así `min(Holgura, 1,5)`
  no puede convertirse en silencio en `min(Holgura, 1, 5)`.
- **P1.6 — Unidades.** Cualquier sintaxis de unidad (`in`, `"`, `'`, `mm`, `ft`, `10'6"`) es
  `UnitSyntaxNotSupported` (P9). `1/2` no es una fracción: es una división válida.
- **P1.7 — Espacios.** No significativos entre tokens. Dentro de una referencia, según OPEN A.
- **P1.8 — El `=` no pertenece a la gramática.** Es la marca de superficie que indica «esto es una
  expresión» (P22). Nunca se persiste.
- **P1.9 — Límites** (valores iniciales, ajustables en revisión): texto ≤ 1000 caracteres, anidamiento ≤ 64,
  nodos ≤ 256 y argumentos por llamada ≤ 16. Superarlos es `LimitExceeded`, **nunca** truncado.
- **P1.10 — Implementación.** Parser escrito a mano, determinista, independiente de la cultura y con el
  límite de anidamiento comprobado **antes** de descender. Prohibidos terceros (ADR-0012), `NCalc`,
  `DataTable.Compute`, `System.Linq.Expressions` y Roslyn (Discovery §15, búsqueda 3).
- **P1.11 — Relación con ADR-0015.** La entrada **sin** `=` sigue siendo un literal parseado con la regla
  localizada de ADR-0015, exactamente como hoy (P22.3). La gramática invariante solo rige dentro de una
  expresión, y esa excepción acotada tiene que constar en el ADR de I-49 (§6).

### P2 — AST/model

**Base.** `VariableDefinition` tiene un solo caso y payload `double`; `LiteralValue` lanza fuera de `Literal`
(Discovery §4.1). No hay AST (Discovery §15, búsqueda 2).

- **P2.1 — Tres modelos inmutables y separados.**
  1. **Sintaxis** (salida del parser): nodos con `SourceSpan` y referencias en su forma textual. Solo sirve
     para escribir y diagnosticar. **Nunca se persiste.**
  2. **`BoundExpression`** (salida del binder o de la lectura del store): `Number(double)`,
     `Reference(SymbolId)`, `Negate(operand)`, `Binary(Add | Subtract | Multiply | Divide, left, right)` y
     `Call(FunctionId, args)`. Sin posiciones, sin nombres, sin paréntesis redundantes y sin `+` unario. Es
     lo que se persiste (P17), se evalúa (P14) y se formatea (P4).
  3. **Resultado** de evaluación (P16).
- **P2.2 — Comprobación semántica separada.** Tipos, aridad, existencia de referencias y ámbito (P5, P7) se
  comprueban sobre un `BoundExpression` **contra un contexto**; no se guardan dentro del árbol. Así el
  mismo árbol leído del store se re-comprueba en cada snapshot.
- **P2.3 — Igualdad estructural por valor.** Ordinal para tokens y `double.Equals` para números, igual que
  la igualdad exacta vigente (Discovery §11.4).
- **P2.4 — Conjunto de nodos cerrado.** Añadir un tipo de nodo cambia el formato persistido (P18.6).
- **P2.5 — Canonicidad.** Para todo `BoundExpression` válido `b` y un mismo contexto:
  `Bind(Parse(Format(b))) == b`. Es una propiedad probada (P28), y se sostiene también con homónimos solo
  si la sintaxis de OPEN A cumple su requisito 3 (§3.1).
- **P2.6 — `VariableDefinition`.** Gana un segundo caso, `Expression = 2`, cuyo payload es un
  `BoundExpression`. `LiteralValue` sigue lanzando fuera de `Literal`, y el acceso a la expresión lanza
  fuera de `Expression`.

### P3 — identity

**Base.** `VariableId` es GUID con igualdad `OrdinalIgnoreCase` (Discovery §4.1). La identidad ambigua es
fail-closed y se acredita también en la re-lectura de commit (Discovery §4.2).

- **P3.1** — La identidad de una variable sigue siendo su `VariableId` (ADR-0034 §2). Pasar de literal a
  expresión, o al revés, **no** cambia el id.
- **P3.2** — `SymbolId = (SymbolNamespace, Key)`. Los namespaces son un conjunto cerrado de tokens
  comparados en `Ordinal`; cada namespace fija el comparador de su clave. Para `projectVariable` la clave es
  el `VariableId`, con su igualdad vigente.
- **P3.3** — Una expresión **no tiene identidad propia**: es la definición de su variable dueña. Los nodos
  del grafo son `SymbolId`.
- **P3.4** — `FunctionId` es un token canónico en minúsculas comparado en `Ordinal`; la entrada no
  distingue mayúsculas.
- **P3.5** — El nombre **nunca** es identidad: el árbol persistido no contiene nombres (P17) y la única
  resolución por nombre ocurre al escribir (P7.3).
- **P3.6** — Un `VariableId` duplicado da `AmbiguousIdentity` **antes** de construir cualquier tabla de
  símbolos (decisiones de I-48 §4): nada se enlaza ni se evalúa sobre un registro ambiguo.
- **P3.7** — Una variable que se referencia a sí misma forma un ciclo de longitud 1 (P13).

### P4 — source text

**Base.** HANDOFF §4 exige persistir la referencia y «nunca el texto» (`docs/HANDOFF.md:1579`). `"0.###"` es
formato de **presentación** de valores y redondea (Discovery §11.5, hallazgo lateral L5).

- **P4.1 — Tres textos, tres papeles.**

  | Texto | Origen | ¿Autoridad? | ¿Se persiste? |
  |---|---|---|---|
  | Texto tecleado | Usuario | No | No |
  | Árbol persistido (`BoundExpression`) | Binder | **Sí**, junto con el registro | Sí (P17) |
  | Texto de edición y presentación | Formatter, desde el árbol y los nombres **actuales** | No | No |

- **P4.2 — El formatter es el único productor de texto editable.** Determinista e invariante: números en su
  representación más corta que hace *round-trip* (nunca `"0.###"`); un espacio a cada lado de un operador
  binario; `min(a, b)`; paréntesis **mínimos** según precedencia y asociatividad; funciones en minúsculas;
  referencias según OPEN A.
- **P4.3 — Normalización visible.** Espacios, paréntesis redundantes y mayúsculas tecleados no se conservan.
  Tras comprometer, el usuario ve la forma canónica.
- **P4.4 — El texto tecleado no se persiste.** Sería una segunda fuente capaz de contradecir al árbol y
  quedaría obsoleta con cada rename (ADR-0034 §2).
- **P4.5 — Referencia rota.** El formatter debe representar una referencia cuyo id no está en el registro de
  forma reconocible y no comprometible, **sin** tomar un nombre de ningún otro sitio. Su forma exacta depende
  de OPEN A.

### P5 — symbol model

**Base.** No existe `SymbolId`, scope ni namespace de símbolos, y el registro acreditado se indexa solo por
`VariableId` (Discovery §15, búsquedas 7–9; §16.2).

- **P5.1** — `SymbolEntry { SymbolId, SymbolScope, ExpressionValueType, DisplayName, Definition }`, con
  `Definition = Literal(double) | Expression(BoundExpression)`.
- **P5.2 — Namespaces.** En V1 hay **uno** activo: `projectVariable`. Quedan **reservados, sin registrar y sin
  resolución**: `rack` y `project` (ID20, P25). El binder devuelve `UnknownNamespace` para cualquier
  namespace no registrado.
- **P5.3 — Ámbitos.** En V1 existe **uno**: `Project`, un valor por dibujo. Queda reservado `Rack`, un valor
  por rack (ID20). Regla preparada desde ya: una definición de ámbito `Project` **no puede** depender de un
  símbolo de ámbito `Rack` (`ScopeViolation`), porque una variable de proyecto tiene un único valor por
  dibujo.
- **P5.4 — Tipos internos.** `ExpressionValueType = Length | Scalar`. `Scalar` existe **solo dentro** de las
  expresiones (factores, razones y constantes). `VariableType` persistido sigue siendo exactamente
  `{ Length = 1 }`, de modo que las guardas G6–G10 no cambian (Discovery §14.3).
- **P5.5 — Reglas de tipo** (fail-closed). Una subexpresión sin referencias es **constante**.

  | Operación | Resultado |
  |---|---|
  | literal numérico | `Scalar` constante |
  | `Length ± Length` | `Length` |
  | `Scalar ± Scalar` | `Scalar` |
  | `Length ± Scalar` constante | `Length` (la constante se lee en pulgadas, P9) |
  | `Length ± Scalar` no constante | `DimensionMismatch` |
  | `Length × Scalar`, `Scalar × Length` | `Length` |
  | `Scalar × Scalar` | `Scalar` |
  | `Length × Length` | `DimensionMismatch` (áreas no soportadas) |
  | `Length ÷ Scalar` | `Length` |
  | `Length ÷ Length` | `Scalar` |
  | `Scalar ÷ Scalar` | `Scalar` |
  | `Scalar ÷ Length` | `DimensionMismatch` |
  | `-x` | tipo y constancia de `x` |
  | `min` / `max` | todos los argumentos del mismo tipo tras adaptar constantes; si no, `DimensionMismatch` |
  | raíz | debe coincidir con el `VariableType` de la dueña; una raíz `Scalar` constante se adapta a `Length`; si no, `ResultTypeMismatch` |

### P6 — ExpressionContext

**Base.** RACKBOMTOTAL lee el registro **una** vez y no resuelve por su cuenta (Discovery §14.3, guarda G12).
La costura pura `FromTargets` existe solo para tests (Discovery §4.3).

- **P6.1** — `ExpressionContext` es un valor inmutable **por operación**: `SymbolTable` + la única instancia
  productiva de `FunctionRegistry` + `ExpressionLimits`.
- **P6.2 — Sin estado ambiental**: ni cultura, ni reloj, ni aleatoriedad, ni entorno, ni cachés estáticas,
  ni AutoCAD, ni sistema de archivos.
- **P6.3 — Un snapshot por operación**, igual para toda ella (ADR-0034 §10).
- **P6.4 — Construcción para Project Variables**: solo desde `UsableProjectVariablesRegistry` acreditado.
  `Absent` da tabla vacía; `Readable` con ids únicos da tabla; cualquier otro resultado **no** produce
  contexto (Discovery §4.1, `Accredit`).
- **P6.5 — Costura de test**: la construcción pura desde entradas sintéticas se permite en el núcleo y en
  tests, y no es alcanzable desde Plugin ni UI, igual que `FromTargets`.
- **P6.6** — ID20 e ID23 construirán sus propios contextos desde sus propios snapshots puros (P25, P26).

### P7 — SymbolResolver/binder

**Base.** Hoy **ninguna** ruta de producción resuelve nombre→id; el editor filtra por *contains*
`OrdinalIgnoreCase` y compromete solo por `VariableId`; ni un nombre exacto ni un candidato único se
auto-seleccionan (Discovery §13.3). Los homónimos son legales (Discovery §13.1).

- **P7.1 — `SymbolResolver`.** Traduce una referencia textual (con la sintaxis que fije OPEN A) a
  **exactamente un** `SymbolId`, o a un fallo tipado: `UnknownSymbol`, `AmbiguousName` (con **todos** los
  ids candidatos) o `UnknownNamespace`. **Nunca** elige uno entre varios.
- **P7.2 — Comparador de nombres** (propuesto, revisable dentro de OPEN A): igualdad exacta
  `OrdinalIgnoreCase`, sin recortes ni normalización Unicode, coherente con los comparadores vigentes
  (Discovery §13.3). El *contains* del editor **no** es una regla de resolución (Discovery §13.4).
- **P7.3 — Única ruta nombre→id de producción, y solo al escribir.** El binder corre únicamente al
  comprometer texto del usuario en RACKVARIABLES (P22). Lectura, evaluación, rename, delete, propagación,
  resolver y BOM **nunca** resuelven nombres.
- **P7.4 — Binder total y fail-closed.** Devuelve un árbol solo con **cero** diagnósticos; si no, la lista
  de diagnósticos, en orden determinista y con un tope (20, ajustable).
- **P7.5 — Comprobación semántica** (P2.2): tipos (P5.5), aridad (P10), existencia de referencias, ámbito
  (P5.3) y autorreferencia (P3.7). Sobre un árbol leído del store corre **sin** resolver nombres.

### P8 — project-variable symbol binding

**Base.** El `Type` persistido tiene un mapping único compartido (decisiones de I-48 §4;
`VariableTypes.TryParseToken`, Discovery §4.1).

- **P8.1** — Namespace `projectVariable`; clave = el texto del `VariableId` tal como está en la entrada del
  registro; comparación `OrdinalIgnoreCase`.
- **P8.2** — Fuente única: el registro acreditado (P6.4). `VariableType.Length → ExpressionValueType.Length`
  pasando por el mapping único; **no** se crea un segundo mapping.
- **P8.3** — `DisplayName = Name`, solo para mostrar y para resolver al escribir.
- **P8.4** — `Definition` = el `Literal` o la `Expression` de la entrada.
- **P8.5** — Un árbol que referencia un id ausente produce `BrokenReference`. Al escribir no puede ocurrir,
  porque el binder solo emite ids de la tabla; en un árbol persistido es un error semántico (OPEN B).
- **P8.6** — Las referencias directas de rack (`ProjectVariableReference`) **no** son símbolos de expresión:
  son consumidores que leen valores evaluados (P24).
- **P8.7** — La compatibilidad de tipo al vincular sigue en `InspectBinding` y `Link` (Discovery §4.1, §8.1),
  con el `VariableType` declarado de la variable.

### P9 — units

**Base.** Pulgadas internas como `double` sin unidad (ADR-0005, ADR-0021; Discovery §12.1–§12.2). No hay
autoridad neutral de conversión ni parser de unidades (Discovery §12.4, §15 búsqueda 12).
`StructuralSectionUnits` es específico del catálogo (Discovery §12.5).

- **P9.1** — Un valor `Length` es un número de pulgadas. El motor **no convierte** nada.
- **P9.2** — Sin sufijos de unidad en V1 (P1.6).
- **P9.3** — Una constante en contexto `Length` son pulgadas (P5.5). Las etiquetas «(in)» de la UI se
  mantienen.
- **P9.4** — La coherencia dimensional se reduce a `Length`/`Scalar` (P5.5). Área, volumen, masa y ángulo no
  están soportados.
- **P9.5** — No se reutiliza `StructuralSectionUnits` ni se crea una autoridad de unidades. Si una iniciativa
  futura necesita sufijos o conversiones, eso exige ADR (ADR-0005 §4).
- **P9.6** — `VariableType` no cambia.

### P10 — function registry

**Base.** No existe registro de funciones (Discovery §15, búsqueda 6). ADR-0012 prohíbe dependencias NuGet
de producto sin acuerdo del Owner y ADR (Discovery §15).

- **P10.1** — `FunctionRegistry` **cerrado e inmutable**, declarado en código dentro del núcleo, con una sola
  instancia productiva. Sin registro en tiempo de ejecución, reflexión, inyección, plugins, funciones de
  usuario ni scripting.
- **P10.2** — Cada función declara `FunctionId`, aridad mínima y máxima, regla de tipo e implementación pura
  y determinista sobre `double` finitos. Un resultado no finito es `NonFiniteResult`.
- **P10.3 — Funciones de V1**: `min(x₁, …, xₙ)` y `max(x₁, …, xₙ)`, con 2 ≤ n ≤ 16 y tipo según P5.5.
- **P10.4** — Al escribir: nombre desconocido → `UnknownFunction`; aridad errónea → `ArityMismatch`. En un
  árbol persistido: `FunctionId` desconocido → ilegibilidad **estructural** (P17.5); función conocida con
  aridad errónea → error **semántico** (OPEN B).
- **P10.5** — Nombre sin distinguir mayúsculas a la entrada; minúsculas canónicas en el árbol y en el
  formatter.
- **P10.6** — Añadir una función (por ejemplo, redondeos para las cantidades enteras de ID23) es cambio de
  código con revisión, y para las builds anteriores es un cambio de formato (P18.6).

### P11 — dependency extraction

**Base.** Hoy no existe ninguna relación variable→variable (Discovery §6.2). `Targets()` ordena por id
`OrdinalIgnoreCase` (Discovery §13.3).

- **P11.1** — `Dependencies(BoundExpression) → IReadOnlyList<SymbolId>`: referencias **directas**, sin
  repetición y en orden determinista (namespace `Ordinal`; después clave con el comparador de su namespace,
  el mismo orden que `Targets()`).
- **P11.2** — Opera sobre el árbol persistido **sin evaluar** y **sin nombres**.
- **P11.3** — Una definición literal tiene cero dependencias.
- **P11.4** — La consumen el grafo (P12), `Delete` (P20), RACKVARIABLES (P22) y la traza (P27).

### P12 — dependency graph

**Base.** ADR-0034 rechazó persistir un grafo en ID22A porque «el grafo pertenece a ID22B/ID21»
(Discovery §6.2). La relación propiedad→variable vive en el authored de cada rack y se descubre barriendo el
dibujo (Discovery §7).

- **P12.1 — Derivado, nunca persistido.** Se construye desde la tabla de símbolos de UN snapshot. El
  registro es la única autoridad: un grafo persistido sería una segunda fuente capaz de divergir.
- **P12.2** — Nodos: todos los símbolos de la tabla (en V1, las variables de proyecto). Aristas: dueña →
  dependencia, con índice inverso dependencia → dependientes.
- **P12.3** — Las relaciones propiedad→variable **no** entran en el grafo: se siguen obteniendo con el
  barrido vigente y se unen al componer el plan (P21). Es la separación que el Discovery dedujo para la
  pregunta 8 (Discovery §2.2).
- **P12.4** — Una arista hacia un id ausente se registra como `BrokenReference`, sin crear nodo.
- **P12.5** — Iteración en el orden de P11.1.
- **P12.6** — Coste `O(V + E)` por snapshot. Que los registros reales sean pequeños es **INFERENCE**: se mide
  en G11.

### P13 — cycle detection

**Base.** No existe detección genérica de ciclos (Discovery §15, búsqueda 5). El plan vacío ante fallo es
invariante (Discovery §4.2).

- **P13.1** — Detección **iterativa** (sin riesgo de profundidad de pila) y determinista de componentes
  fuertemente conexas. Un ciclo es una componente de más de un nodo o un nodo con arista a sí mismo.
- **P13.2** — Informa **cada** ciclo con sus miembros en el orden de P11.1; cada miembro recibe `Cycle` con
  la lista completa.
- **P13.3** — Al escribir o mutar, un estado «después» con ciclo hace fallar el preflight con `Cycle` y un
  plan **vacío** (P21).
- **P13.4** — Los miembros de un ciclo y **todos** sus dependientes transitivos no son evaluables (`Cycle` o
  `DependencyFailed`). **Nunca** hay fallback a un literal ni a cero.
- **P13.5** — Con P13.3, esta build no escribiría un ciclo. El contrato cuando se **lee** uno persistido
  es **OPEN FOR ARCHITECT B** (§4).

### P14 — evaluation order

**Base.** Hoy el resolver escribe `Target.LiteralValue` y ningún valor se calcula a partir de otro
(Discovery §6.2). La igualdad es exacta (Discovery §11.4).

- **P14.1** — Orden topológico sobre la parte acíclica, con el desempate de P11.1, **independiente** del
  orden de las entradas en el registro. Una prueba lo demuestra con permutaciones (P28).
- **P14.2** — Evaluación **ansiosa** de todo el snapshot, una vez por operación (`RegistryEvaluation`) y
  memoizada: cada símbolo se evalúa una sola vez.
- **P14.3** — Cortocircuito: si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id
  de la causa, sin evaluarse.
- **P14.4 — Semántica numérica.** `double` IEEE-754 sin redondeos intermedios. `x ÷ 0`, incluido `0 ÷ 0`, es
  `DivisionByZero`. Cualquier resultado intermedio o final no finito es `NonFiniteResult`.
- **P14.5 — Determinismo.** Sin paralelismo, cultura ni reloj: el mismo snapshot da resultados idénticos bit
  a bit.
- **P14.6** — Un literal se evalúa a su propio valor, sin cambio. Por eso todo registro de I-47/I-48 produce
  **exactamente** los mismos efectivos que hoy (P24.8).
- **P14.7 — Dónde.** Dentro de Application, tras la acreditación y antes del resolver (DR-3). **Nunca** en el
  Plugin, la UI, el ejecutor ni el comando de BOM (guardas extendidas, P28.4).

### P15 — diagnostics

**Base.** La doctrina exige estados tipados (ADR-0034 §8). El texto de reparación ya vive en una sola capa
(`ProjectVariableRepairText`, Discovery §3.4).

- **P15.1** — Catálogo **cerrado** de códigos. Cada diagnóstico lleva código estable, clase, severidad, símbolo
  dueño, posición opcional (solo al escribir) e ids relacionados.
- **P15.2 — Severidad.** En V1 todo diagnóstico es `Error`: no hay avisos que permitan continuar.
- **P15.3 — Catálogo de V1.**

  | Clase | Códigos |
  |---|---|
  | Sintaxis | `EmptyExpression`, `UnexpectedCharacter`, `UnexpectedToken`, `UnbalancedParenthesis`, `InvalidNumber`, `AmbiguousDecimalComma`, `UnitSyntaxNotSupported`, `LimitExceeded` |
  | Enlace | `UnknownSymbol`, `AmbiguousName`, `UnknownNamespace`, `UnknownFunction`, `ArityMismatch`, `ScopeViolation` |
  | Tipo | `DimensionMismatch`, `ResultTypeMismatch` |
  | Grafo | `BrokenReference`, `Cycle`, `DependencyFailed` |
  | Evaluación | `DivisionByZero`, `NonFiniteResult`, `OutOfRange` (P21.7) |

- **P15.4** — El núcleo devuelve códigos y datos. Los mensajes en español se producen en **una** capa de
  texto de Project Variables o de UI.
- **P15.5** — Cualquier `Error` implica: sin valor, sin resultado parcial y sin fallback.
- **P15.6** — Los estados de nivel registro conservan sus resultados vigentes (`PresentButUnreadable`,
  `IncompatibleMajor`, `AmbiguousIdentity`; Discovery §4.1). Los diagnósticos **no** los sustituyen.
- **P15.7** — Orden determinista: posición, código y símbolo.

### P16 — EvaluationResult

**Base.** El target acreditado tiene hoy forma de literal (`VariableTargetSnapshot`, Discovery §4.1 y §4.4).

- **P16.1 — Por símbolo**: `EvaluationResult { SymbolId, Outcome = Success | Failed, Value, ValueType,
  Diagnostics, Trace }`. `Value` solo existe con `Success`, y acceder a él en otro caso **lanza**, con la
  misma disciplina que `LiteralValue`. `Diagnostics` no está vacío si y solo si `Failed`.
- **P16.2 — Por snapshot**: `RegistryEvaluation { resultados por símbolo, grafo directo e inverso, ciclos,
  orden de evaluación }`, inmutable.
- **P16.3 — Traza** (P27): definición enlazada, dependencias directas con el valor que tenían y cadena de
  causas de un fallo. Tamaño acotado y solo en memoria.
- **P16.4** — Los consumidores leen `Value` solo tras comprobar `Success`. El target acreditado evoluciona
  para llevar **valor evaluado y resultado de evaluación** en lugar de un valor con forma de literal.
- **P16.5** — Una variable literal da `Success(literal)` con traza trivial.

### P17 — persistence

**Base.** El registro es un `Xrecord` troceado en el NOD bajo `RACKCAD_PROJECT`, con opciones JSON por defecto
y propiedades en PascalCase (Discovery §5.1). `ExtensionData` existe en raíz, entrada y definición (Discovery
§5.4). El store rechaza todo kind distinto de `literal` (Discovery §5.3).

- **P17.1** — Solo cambia el registro (`ProjectVariablesDocument`). El almacenamiento físico no cambia.
- **P17.2** — Una definición literal se persiste **exactamente** como hoy: `{"Kind":"literal","Value":6.0}`.
- **P17.3 — Definición por expresión**: `{"Kind":"expression","Expression":<nodo>}`, **sin** `Value`. Esquema
  de nodos:

  ```json
  {"Node":"number","Value":2.5}
  {"Node":"ref","Namespace":"projectVariable","Id":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"}
  {"Node":"neg","Operand":{}}
  {"Node":"add","Left":{},"Right":{}}
  {"Node":"call","Function":"min","Args":[{},{}]}
  ```

  `add` representa también `sub`, `mul` y `div`; `{}` señala un nodo anidado. Ejemplo completo:

  ```json
  {"SchemaVersion":"1.1","Variables":[
    {"VariableId":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44","Name":"Base","Type":"Length",
     "Definition":{"Kind":"literal","Value":6.0}},
    {"VariableId":"8c1d7e20-4b5a-4c6d-9e7f-102132435465","Name":"Holgura","Type":"Length",
     "Definition":{"Kind":"expression","Expression":{"Node":"add",
       "Left":{"Node":"ref","Namespace":"projectVariable","Id":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
       "Right":{"Node":"number","Value":2}}}}]}
  ```

- **P17.4 — El valor evaluado NO se persiste.** Se deriva en cada snapshot (DR-3). Un valor cacheado sería una
  segunda autoridad capaz de quedar obsoleta, y no ayuda a las builds anteriores, que rechazan el kind entero
  (Discovery §5.6).
- **P17.5 — Validación ESTRUCTURAL en el store** (fallo ⇒ `PresentButUnreadable`). Frontera propuesta,
  **revisable dentro de OPEN B**:
  - `Kind` desconocido (sin cambio, C4-8);
  - `expression` con `Value` presente o sin `Expression`, y `literal` con `Expression` presente: dos
    autoridades en la misma definición;
  - `Node` desconocido, campo obligatorio ausente o número no finito;
  - `Namespace` desconocido o `Id` no parseable;
  - `Function` desconocida;
  - límites de P1.9 superados;
  - **cualquier campo desconocido DENTRO del árbol**: a diferencia de raíz, entrada y definición, donde
    `ExtensionData` se conserva como hoy, un campo desconocido en un nodo podría cambiar su significado;
  - `expression` en un documento de versión inferior a `1.1` (P18.3).
- **P17.6 — Los errores semánticos NO son errores del store.** `BrokenReference`, `Cycle`,
  `DimensionMismatch`, `ResultTypeMismatch`, `ArityMismatch`, `DivisionByZero` y `NonFiniteResult` se
  detectan al evaluar. Su contrato es OPEN B.
- **P17.7** — Leer nunca escribe (Discovery §5.7). Solo escribe una `RegistryMutation` tras un preflight.
- **P17.8** — La persistencia del lado rack **no cambia**: `PropertyValues`, kind `projectVariable` y
  promoción *sticky* 2.0 del Selectivo (Discovery §5.1, §5.8).
- **P17.9** — Los números se serializan con `System.Text.Json` por defecto, así que `6.0` puede reescribirse
  como `6`, igual que hoy (Discovery §5.1).
- **P17.10 — Comparadores.**
  - Nombres de propiedad JSON: sin distinguir mayúsculas, por la opción vigente `PropertyNameCaseInsensitive`
    (Discovery §5.1).
  - Kind `expression`: `OrdinalIgnoreCase`, igual que `literal` hoy (Discovery §13.3).
  - Tokens de `Node`, `Namespace` y `Function`: `Ordinal` sobre su forma canónica; cualquier otra grafía es
    error estructural.
  - `Id`: el parseo y la igualdad vigentes de `VariableId` (Discovery §4.1).

### P18 — schema evolution

**Base.** Las builds I-47/I-48 leen **cualquier** `1.x`, conservan un minor mayor al re-guardar y rechazan el
registro **entero** si una entrada tiene kind distinto de `literal` (Discovery §5.4, §5.6). Re-guardar nunca
degrada un minor del mismo major (Discovery §5.4). El Selectivo ya condiciona su versión al contenido: la
promoción *sticky* de `SelectiveDesignSchema.ResolveWriteVersion` depende de si hay vínculos (Discovery §3.2,
§5.8).

- **P18.1 — Evolución por MINOR.** Las expresiones introducen `1.1`. `SupportedMajor` sigue siendo `1`.
- **P18.2 — Regla de escritura**, condicionada al contenido y sin degradar nunca:
  - registro **sin** expresiones → `ResolveWriteVersion(stored, "1.0")`, exactamente como hoy;
  - registro **con** alguna expresión → `ResolveWriteVersion(stored, "1.1")`, así que `1.0 → 1.1` y
    `1.7 → 1.7`;
  - al crear, la versión se estampa explícitamente: `1.0` sin expresiones y `1.1` con alguna (ADR-0034 §13).
- **P18.3 — Regla de lectura.** `expression` solo se admite en `1.x` con minor ≥ 1. En `1.0` es
  `PresentButUnreadable`: la versión declarada tiene que cubrir el contenido. Una versión ausente ya es
  ilegible hoy (Discovery §5.2; ADR-0034 §13, «missing ≠ explicit 1.0»).
- **P18.4 — Efecto sobre las builds I-47/I-48** (hechos de Discovery §5.6):
  - registro **con** alguna expresión → lo rechazan entero con «declara una definición de clase
    desconocida»: fail-closed, sin lectura silenciosa y sin escritura;
  - registro **sin** expresiones, aunque sea `1.1` → lo siguen leyendo, y al re-guardar conservan `1.1`.
- **P18.5 — Por qué minor y no un major 2.0 sticky.** (a) C4-8 ya convierte un `Definition.kind`
  desconocido en error duro en las builds anteriores, justo para que un kind nuevo sea fail-closed sin
  cambiar de major. (b) Un major *sticky* dejaría el dibujo ilegible para ellas **para siempre**, aunque se
  quitaran todas las expresiones; el minor conserva la reversibilidad. (c) Un registro literal-only sigue en
  `1.0`, con la misma forma JSON. **Coste aceptado**: una build anterior muestra «clase desconocida» y no
  «versión más nueva» (Discovery §5.6, INFERENCE). La alternativa rechazada está en §7.
- **P18.6** — Todo lo nuevo que aparezca **dentro** del árbol en el futuro (nodos, funciones, namespaces) es
  fail-closed para esta build (P17.5). Cada adición exige revisión y, si cambia persistencia, ADR.
- **P18.7** — La línea de versión del diseño Selectivo no se toca.

### P19 — rename

**Base.** `Rename` es registry-only: sin barrido, sin racks y sin redibujo (Discovery §8.1; decisiones de I-47
C4.6-7). Hoy valida reconstruyendo un literal (Discovery §5.5, fila 10).

- **P19.1** — Sigue siendo registry-only.
- **P19.2** — Las expresiones no cambian, porque refieren ids; el formatter muestra el nombre nuevo de
  inmediato (P4).
- **P19.3** — La validación de `Rename` conserva la definición vigente, literal o expresión, en lugar de
  reconstruir un literal.
- **P19.4** — Las reglas de nombre no cambian: nombre no en blanco, homónimos legales y sin política de
  unicidad, mayúsculas ni caracteres (Discovery §13.1). Un rename puede volver **ambiguo** un nombre para
  escrituras **futuras** (`AmbiguousName`, OPEN A), pero nunca cambia el significado de nada persistido.
- **P19.5** — El requisito «rename no rompe» de OPEN A se cumple por construcción.

### P20 — delete

**Base.** `Delete` solo cuenta consumidores propiedad→variable, y `UnlinkAllAndDelete` materializa los
consumidores directos (Discovery §8.1, §8.2). ADR-0034 §11 rechaza borrar materializando automáticamente.

- **P20.1** — `Delete(X)` queda **bloqueado** si X tiene (a) consumidores propiedad→variable, como hoy, **o**
  (b) dependientes variable→variable en el grafo (P12). Se informan ambas listas. Sin cascada y sin
  materializar dependientes.
- **P20.2** — Los dependientes salen del snapshot acreditado **sin** barrer el dibujo, porque viven en el
  registro (Discovery §6.3). El barrido sigue haciendo falta para (a).
- **P20.3** — `UnlinkAllAndDelete(X)` queda bloqueado si X tiene dependientes variable→variable. Sin ellos,
  conserva su semántica vigente: materializa **todo** P en cada rack.
- **P20.4** — Borrar una variable que es a su vez una expresión, sin consumidores ni dependientes, está
  permitido y es registry-only.
- **P20.5** — Resultado tipado `BlockedByDependents`, o `BlockedByConsumers` ampliado con la lista tipada de
  dependientes. El nombre es ilustrativo.
- **P20.6** — `Delete` sobre un registro con errores semánticos persistidos: **OPEN B**.

### P21 — propagation

**Base.** Traza vigente de `ChangeValue` en 14 pasos (Discovery §6.1). La profundidad 1 vive en el
descubrimiento, el plan, la resolución, `Delete` y RACKVARIABLES (Discovery §6.2). `MutationDestinationBinding`
aborta si una definición aparece dos veces (Discovery §6.3). RACKVARIABLES hace hoy un descubrimiento por
variable al abrir (Discovery §7).

- **P21.1** — `ChangeDefinition(X, d)` generaliza `ChangeValue` a las cuatro transiciones: literal→literal,
  literal→expresión, expresión→literal y expresión→expresión.
- **P21.2 — Preflight** (puro, todo o nada):
  1. acreditar el registro «antes» (sin cambio);
  2. si `d` llega como texto, parsear, enlazar y comprobar contra el contexto «antes» (P7);
  3. «después» = aplicar la mutación a un clon del documento acreditado, acreditarlo y evaluarlo (P14);
  4. rechazar si «después» tiene un ciclo que pase por X, si falla cualquier variable del cierre impactado
     (P21.3) o si hay `OutOfRange` (P21.7). Un ciclo persistido que no pasa por X ya existía antes: es
     OPEN B;
  5. cierre impactado `I = {X} ∪ dependientes transitivos de X` en el grafo «después», en el orden de P11.1;
  6. consumidores de rack de **todo** `I`, descubiertos sobre **una** proyección del barrido (P21.4) y
     deduplicados por `RackId`;
  7. por rack, resolver contra la evaluación «después» con el resolver único; un solo fallo vacía el plan;
  8. plan = **una** `RegistryMutation` (en el registro solo cambia X) + **una** `RackMutation` por rack, con
     efectivo completo y todas sus vistas.
- **P21.3** — Toda variable de `I` tiene que dar `Success` en «después». Qué ocurre si el snapshot contiene
  errores semánticos persistidos **fuera** de `I` es **OPEN B** (§4.5, preguntas e e i).
- **P21.4 — Descubrimiento por conjunto.** La semántica de la sonda no cambia: tri-estado, `Indeterminate`
  aborta, positivos parciales abortan y se exige autoridad `Single` (Discovery §7). Se calcula **una vez** sobre
  el barrido y tiene que ser **equivalente** a la unión de descubrimientos por variable; esa equivalencia se
  prueba como oráculo (P28). Evita el coste por variable del riesgo R5.
- **P21.5** — La estructura del ejecutor no cambia: PREPARE / MUTATE / POST, una transacción, un commit y un
  `Regen` (Discovery §6.1). El ejecutor **no** parsea ni evalúa (guarda G13 extendida, P28.4).
- **P21.6 — Re-lectura de commit.** Se amplía la secuencia de V8-R01 de I-48, **dentro de Application**
  (`RegistryCommit.Prepare`, Discovery §3.1), no en el Plugin:

  ```text
  lastRead = Read(...)
  accredited = Accredit(lastRead)                       // sin cambio (V8-R01)
  changed = ApplyTo(accredited.Document)                // sin cambio
  evaluated = Evaluate(changed) sobre el cierre I       // NUEVO
  si evaluated falla, o algún valor de I difiere del calculado en el preflight:
      Abort ANTES de TryWrite                           // registro 0, vistas 0
  si no:
      TryWrite(..., lastRead, changed, ...)             // sin cambio
  ```

  Para poder comparar, el plan transporta los valores esperados de `I` (los mismos que expone P27.2). La
  comparación se limita a esos valores: una re-lectura que cambió **en otro sitio** pero los conserva **no** se
  rechaza, así que la política de concurrencia de V8-R05 queda intacta para lo que ya existía.
- **P21.7 — Rango.** Toda variable de `I` cuya definición sea una expresión tiene que evaluar a un valor
  `> 0`, la misma regla que RACKVARIABLES aplica hoy a los literales (Discovery §10.3, §11.4). Si no,
  `OutOfRange`. La regla de la UI para literales no se mueve, de modo que el comportamiento de I-47/I-48 con
  literales no cambia.
- **P21.8 — `Create` con expresión.** Sigue siendo registry-only, porque una variable nueva no tiene
  consumidores ni dependientes. Pero su preflight recibe y acredita el registro para enlazar y evaluar, algo
  que hoy `Create` no hace (Discovery §8.1).
- **P21.9 — `Link`, `Unlink`, `UnlinkAllAndDelete` y exportación.** Vincular a una variable definida por
  expresión se permite si el tipo es compatible y el resultado es `Success`; la regla de congelado 20.13 de
  I-48 no cambia (Discovery §9.1). `Unlink`, `UnlinkAllAndDelete` y la exportación a biblioteca materializan el **valor
  evaluado exacto**, sin redondear, así que el efecto geométrico sigue siendo nulo. Se acepta el ruido de
  coma flotante del riesgo R16.
- **P21.10** — `RepairBrokenRack` no cambia: rack-scoped y solo para targets **ausentes** (Discovery §8.1). Un
  target existente pero no evaluable **no** está ausente (P24.4).
- **P21.11** — Sin límite de profundidad más allá de la aciclicidad y de P1.9, y **nunca** K planes
  sucesivos: un plan da un `Regen` (Discovery §2.2, pregunta 16).

### P22 — RACKVARIABLES UX

**Base.** Comando y bucle con re-lectura en cada vuelta (Discovery §10.1). Workspace y filas con `LiteralValue`
(Discovery §10.2). Lista sin id, campos «Nombre» y «Valor (in)», panel de referencias rotas fuera del panel
editable (Discovery §10.3). Intents con `double` (Discovery §10.4). Todo lo que asume literal, `Length` y
consumidores directos (Discovery §10.5).

- **P22.1** — **Ni comando nuevo ni ventana nueva**: los censos de 33 comandos y 29 ventanas no cambian
  (guardas G15 y G16, Discovery §14.3).
- **P22.2 — Fila de la lista**: nombre · tipo · definición (literal o texto canónico de la expresión) · valor
  evaluado (`"0.###"`) · estado (correcto o código de diagnóstico) · número de racks directos · número de
  dependientes.
- **P22.3 — Un solo campo «Definición (in)»** en lugar de «Valor (in)». Un texto que **no** empieza por `=` es
  un literal, parseado con la regla de ADR-0015 y `> 0`, exactamente como hoy. Un texto que empieza por `=`
  es una expresión (P1).
- **P22.4 — Diagnóstico en vivo.** La ventana llama a una función pura de Application, expuesta a través del
  DTO del workspace, que parsea y enlaza contra el snapshot del workspace y devuelve diagnósticos con
  posiciones. La ventana los muestra y **no** emite el intent mientras haya errores. La ventana **nunca**
  evalúa con autoridad, **nunca** resuelve nombres por su cuenta y **no** referencia tipos del núcleo (guarda,
  P28.4). La única autoridad sigue siendo el preflight del bucle.
- **P22.5 — Selector de referencias.** Una lista con nombre, valor evaluado y desambiguador, igual que los
  candidatos del editor (Discovery §13.2), inserta en el cursor el token canónico del `VariableId` elegido.
  **Nunca** auto-selecciona por el nombre tecleado. El token exacto depende de **OPEN A**. Sin autocompletado
  mientras se escribe (P30).
- **P22.6 — Detalle de la variable seleccionada**: «Depende de» (dependencias directas) y «La usan»
  (variables dependientes directas, más los racks y propiedades que hoy muestra «Racks que la usan»).
- **P22.7 — Intents.** `Create(nombre, textoDefinición)` y `ChangeDefinition(id, textoDefinición)`;
  `Rename`, `Delete`, `UnlinkAllAndDelete` y `RepairBroken` conservan su payload. El texto se enlaza en el
  preflight contra el registro **re-leído**, nunca contra el snapshot de la ventana.
- **P22.8** — Los fallos de preflight (`Cycle`, `BlockedByDependents`, `OutOfRange`, `AmbiguousName`, etc.)
  usan el mecanismo vigente del bucle (Discovery §10.1).
- **P22.9 — Panel de errores semánticos persistidos**, fuera del panel editable como hoy «Referencias rotas».
  Qué acciones admite es **OPEN B**.
- **P22.10** — Pruebas de UI sin interfaz, con ids reales del catálogo y sin un `MessageBox` de producción
  alcanzable en los flujos probados.

### P23 — LinkedPropertyEditor UX

**Base.** Nueve ubicaciones de la sesión, el control y la ventana asumen que todo texto que empieza por `=` es
una consulta de referencia (Discovery §9.6). Pines de comportamiento G20 (Discovery §14.3). Opciones con `LiteralValue` y
efectivo de una referencia = `option.LiteralValue` (Discovery §5.5, filas 12-13). Las opciones se calculan una
vez y se comparten entre editores (Discovery §9.1).

- **P23.1** — El significado de `=` **no cambia**: consulta que filtra y nunca resuelve; compromiso solo por
  selección explícita de un `VariableId`; reglas de C4, de LostFocus y de no auto-selección intactas. Todos
  los pines de G20 se conservan.
- **P23.2** — **Sin expresiones en el campo** (DR-1, P30). `=Holgura General + 2` sigue siendo una consulta sin
  candidatos que no se puede comprometer. Hoy eso solo está deducido del código (Discovery §9.6,
  INFERENCE); V1 añade un pin que lo fija.
- **P23.3 — Opciones.** `LinkedPropertyOptions.ForProperty` ofrece las variables de tipo compatible cuya
  evaluación es `Success`, con el **valor evaluado** en el texto del candidato (`Nombre = valor [8
  caracteres]`) y una marca para las definidas por expresión. Las variables con error **no** se ofrecen.
- **P23.4** — El efectivo mostrado para una referencia es el **valor evaluado**. El texto de estado bajo el
  campo añade la definición cuando la variable es una expresión, solo como presentación.
- **P23.5** — Compartir las opciones entre editores sigue siendo seguro: las dos propiedades vinculables son
  `Length`.
- **P23.6** — `LinkedPropertySourceKind` conserva sus dos miembros: I-49 no añade una tercera fuente
  (Discovery §9.7).
- **P23.7** — Cambios en `RackSelectiveWindow`: mínimos o **ninguno**, porque los textos vienen de las opciones
  de Application. Es un archivo caliente compartido con I-50 (ADR-0035 propuesto de I-50, Discovery §19.2):
  cualquier edición pasa antes por el procedimiento de colisión (§9).

### P24 — direct-reference compatibility

**Base.** Binding persistido `{"Kind":"projectVariable","VariableId":"<guid>"}` (Discovery §5.1). Cinco
resultados de `InspectBinding`: `Healthy`, `RepairableMissingTarget`, `FatalIncompatibleTarget`,
`FatalUnknownProperty` y `FatalMalformedReference` (Discovery §4.1). Duplicar conserva el `VariableId` y
exportar materializa (Discovery §5.8).

- **P24.1** — El binding persistido de rack **no cambia**: ni kind nuevo de valor de propiedad, ni cambio en las
  reglas de versión del Selectivo, ni migración.
- **P24.2** — `ProjectVariableReference(X)` → efectivo = **valor evaluado** de X. Con X literal es idéntico a
  hoy; con X expresión, es el valor derivado.
- **P24.3** — El resolver único se conserva (G11) y consume targets evaluados (P16.4).
- **P24.4** — `InspectBinding` gana un **sexto** resultado tipado para un target que existe y es compatible pero
  cuya evaluación falló (ilustrativo: `FatalUnevaluableTarget`). **Nunca** es `Healthy` ni
  `RepairableMissingTarget`, así que el rack queda `Blocked` para reparación (Discovery §4.1, `Assess`).
  Hasta dónde se extiende el bloqueo más allá de ese rack lo decide **OPEN B**.
- **P24.5** — RACKBOMTOTAL evalúa una vez por snapshot con la misma tubería, y un target no evaluable **nunca**
  produce una cantidad: sin fallback. Si eso aborta el total entero o solo cuando un rack cotizado consume el
  cierre afectado es **OPEN B** (§4.5, pregunta d). Hoy una referencia rota aborta el total (ADR-0034 §10).
- **P24.6** — Duplicación y restamp **no cambian**: la copia conserva el `VariableId`. I-51, en
  `4c79e4a`, caracteriza con T14–T15 que dos vínculos reales sobreviven al restamp; V1 no toca ese camino.
- **P24.7** — La exportación a biblioteca materializa el valor evaluado exacto (P21.9).
- **P24.8** — Un registro de I-47/I-48, literal-only, da **los mismos** efectivos, se escribe en `1.0` y
  conserva la misma forma JSON (P14.6, P18.2; fixture dorado en P28).
- **P24.9** — La autoridad multi-vista no cambia: el registro no es por vista.

### P25 — ID20 extension point

**OWNER INPUT.** I-49 **no** implementa `Rack.Frentes`, `Rack.Niveles`, `Rack.Altura`, `Project.TotalRacks`
ni `Project.TotalFrentes`; solo prepara `SymbolId`, scopes/namespaces, `ExpressionContext`, resolución de
símbolos y los datos que necesita el evaluador.

- **P25.1 — Entregado por I-49**: `SymbolId`, `SymbolNamespace` (solo `projectVariable` activo), `SymbolScope`
  (solo `Project`), `SymbolTable`/`SymbolEntry`, `ExpressionContext`, `SymbolResolver`, reglas de tipo y
  evaluador sobre datos puros.
- **P25.2 — Reservado y no implementado**: namespaces `rack` y `project`, ámbito `Rack` y un tercer caso de
  definición para valores calculados. La reserva consta en el ADR; **ningún** camino productivo los crea, el
  binder responde `UnknownNamespace` y una prueba fija que `Rack.Frentes` y `Project.TotalRacks` **no**
  resuelven en producción.
- **P25.3** — La regla de ámbito de P5.3 se fija con una prueba del núcleo sobre entradas **sintéticas de
  test**, sin crear ningún símbolo `Rack` productivo.
- **P25.4 — Frontera de datos para ID20.** Los valores calculados tendrán que venir de snapshots puros
  proyectados desde el barrido del Plugin, igual que `ProjectVariableScanEntry` (Discovery §16.2). El núcleo
  **nunca** lee AutoCAD, sistemas de Domain ni catálogos.
- **P25.5 — Lo que ID20 tendrá que resolver, y V1 no**: `Rack.Frentes` es un conteo **por fondo**,
  `Rack.Niveles` admite al menos dos lecturas, `Rack.Altura` requiere catálogo y redondea al pie, no existe
  ningún agregado `Project.*` y `RackCount` solo cuenta racks con BOM (Discovery §16.2).
- **P25.6** — Las preguntas de frontera de ID20 son del Owner y de ID20 (contrato §12), no de esta Proposal.

### P26 — ID23 extension point

**OWNER INPUT.** ID23 debe poder reutilizar parser, evaluador y contexto **sin depender de ProjectVariables**.

- **P26.1** — El núcleo no depende de `ProjectVariables`, `Persistence`, `Systems.*`, `Bom`, `Catalogs`,
  Domain, UI, Plugin ni AutoCAD. Lo fija una guarda de fuente (P28.4), y la dirección de dependencia es de
  Project Variables hacia el núcleo, **nunca** al revés.
- **P26.2 — Superficie reutilizable**: parsear, enlazar con un contexto que aporta quien llama, comprobar,
  extraer dependencias, grafo, ciclos, orden, evaluar, formatear y diagnosticar.
- **P26.3 — Lo que añadiría ID23, y V1 no**: sus namespaces y ámbitos, un tipo de resultado entero y funciones
  de redondeo para `Quantity` (hoy `int`, Discovery §16.3), la persistencia de fórmulas de BOM y su propio ADR.
- **P26.4** — Sin ensamblado separado en V1 (§7, ALT-9): la guarda marca la frontera.
- **P26.5** — Ningún tipo de I-49 lleva conceptos de BOM, y no se persiste ninguna fórmula de BOM.

### P27 — ID28/29 information preservation

**OWNER INPUT.** Preservar lo que ID28 Explain / Trace e ID29 Impact Preview necesitarán, sin implementar sus
superficies. **Base**: hoy la procedencia se pierde en cuanto el resolver escribe el número en Domain, y nada
del impacto se persiste (Discovery §16.4).

- **P27.1** — Por snapshot y en memoria (`RegistryEvaluation`, P16.2): árbol por variable, dependencias
  directas, aristas inversas, orden de evaluación, valor o diagnósticos por variable, ciclos y cadena de
  causas (`DependencyFailed` → causa raíz).
- **P27.2** — El resultado del preflight expone el **cierre impactado** (variables con valor antes y después) y
  los racks con sus propiedades **antes** de ejecutar; hoy solo expone los racks (Discovery §16.4). V1 lo usa
  en los mensajes de RACKVARIABLES.
- **P27.3** — La cadena de Explain se deriva **sin** re-evaluar: propiedad → `VariableId` (resolución) →
  `EvaluationResult` con traza, en el mismo snapshot.
- **P27.4** — **Nada se persiste**: ni trazas, ni grafo, ni valores.
- **P27.5** — El determinismo de P14.5 hace reproducibles las trazas.
- **P27.6** — Sin UI de ID28 ni de ID29 en I-49 (P30).

### P28 — tests

**Base.** Marco xUnit sin mocks; el Plugin solo se verifica con guardas de texto (Discovery §14.1). Inventario
por área y escenarios de ID22B que faltan (Discovery §14.2). Veinte guardas y pines que un motor tocaría
(Discovery §14.3). Lección de I-48: un criterio sobre tokens puede certificar una implementación corrupta
([Proposal V2 de I-48](I-48-proposal-v2.md), R-01).

- **P28.1** — El **criterio de aceptación es de comportamiento**. Las guardas de fuente son defensa
  secundaria y nunca el criterio.
- **P28.2 — Evidencia por gate**: RED demostrado en local antes de GREEN para todo comportamiento nuevo;
  suites focal y de impacto; las **dos** suites en local sobre el Candidato; CI verde sobre el SHA exacto
  (AGENTS.md; WORKFLOW §4 y §5).
- **P28.3 — Familias de pruebas.**

  | Gate | Qué se prueba |
  |---|---|
  | G5 | Parser y lexer por tabla (válidos, inválidos, posiciones), `AmbiguousDecimalComma`, unidades rechazadas, límites, orden de diagnósticos; formatter canónico y *round-trip* P2.5 sobre un corpus |
  | G6 | Resolver (desconocido, ambiguo, namespace), matriz completa de P5.5 en ambos sentidos, aridad y tipos de `min`/`max`, semántica numérica (división por cero, desbordamiento a infinito, `NaN`), regla de ámbito con entradas sintéticas, `Rack.*` y `Project.*` no resuelven |
  | G7 | Extracción sin repetición y ordenada; grafo con diamante, autorreferencia, ciclo de 2, componente mayor y aristas rotas; orden independiente de permutaciones; cortocircuito `DependencyFailed` |
  | G8 | Round-trip del store real con expresiones; cada regla estructural de P17.5 da `PresentButUnreadable`; reglas de versión de P18; **fixture dorado**: el JSON literal-only que escribe la build nueva es idéntico al que escribe `a4d88f1`; efectivos iguales a hoy para registros literal-only; sexto resultado de `InspectBinding`; RACKBOMTOTAL con un solo snapshot |
  | G9 | Las cuatro transiciones; cierre impactado; una `RegistryMutation`; racks deduplicados; `Delete` y `UnlinkAllAndDelete` bloqueados por dependientes; ciclo rechazado; `OutOfRange`; `Create` y `Rename` con expresiones; aborto en commit antes de `TryWrite`; oráculo: descubrimiento por conjunto = unión por variable |
  | G10 | DTOs del workspace, diagnóstico en la ventana, el selector inserta un token ligado a id, intents; opciones del editor con valor evaluado; variables con error no ofrecidas; pin nuevo: `=Base + 2` no se compromete |
  | G11 | Pruebas reales de cadena X → Y → propiedad → geometría y firma de BOM sobre catálogo real; dos propiedades ligadas a variables dependientes; suites de I-47/I-48 verdes sin cambios de aserción salvo las guardas de P28.4 |

- **P28.4 — Evolución explícita de las guardas** (Discovery §14.3). Se hace en el **mismo** gate que el
  comportamiento, con RED→GREEN, y **nunca** renombrando tipos para esquivarlas:

  | Guarda | Evolución propuesta | Gate |
  |---|---|---|
  | G1 `NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS` | `RackPropertyReference`, `rackProperty` y `FormulaParser` siguen prohibidos en Application, Plugin y UI; `ExpressionParser` y `DependencyGraph` pasan a permitirse **solo** en Application (núcleo y adaptador de Project Variables) y siguen prohibidos en Plugin y UI | G5, G7 |
  | G2 `LA_DEFINICION_DE_UNA_VARIABLE_SIGUE_TENIENDO_UN_SOLO_CASO` | Se sustituye: exactamente `Literal = 1` y `Expression = 2`; `Formula = ` sigue prohibido | G8 |
  | G3 `"expression"` = error duro | Se sustituye: `"expression"` válido solo con árbol válido y versión ≥ `1.1`; kinds desconocidos (`"formula"`, `"futureKind"`) siguen siendo error duro | G8 |
  | G4 Domain sin variables | Sin cambio; se añade: Domain sin tokens del núcleo | G5 |
  | G5 capa pura sin AutoCAD | Se extiende al namespace del núcleo | G5 |
  | G6–G10 gramática de `VariableType` | Sin cambio | — |
  | G11 y G12 resolver único y BOM | Sin cambio; se añade: RACKBOMTOTAL no parsea ni evalúa | G8 |
  | G13 ejecutor | Se añaden `ExpressionParser`, `ExpressionEvaluator` y `DependencyGraph` como ausentes | G9 |
  | G14 comando RACKVARIABLES | Sin cambio; se añade: el comando no evalúa | G9 |
  | G15 y G16 censos | Sin cambio: 33 comandos y 29 ventanas | — |
  | G17 catálogo cerrado | Sin cambio: dos propiedades | — |
  | G18 Plugin y UI no interpretan vínculos | Se extiende: sin tipos del núcleo en Plugin ni UI | G10 |
  | G19 y G20 identidad y `=` | Sin cambio, más el pin de P23.2 | G10 |
  | **Nueva** — independencia del núcleo | Los archivos del núcleo no nombran `ProjectVariables`, `Persistence`, `Systems`, `Bom`, `Catalogs`, `RackCad.Domain`, `RackCad.UI`, `RackCad.Plugin` ni `Autodesk` | G5 |

- **P28.5** — V1 no exige refactorizar los fixtures duplicados que el Discovery contó (≈25 clases, Discovery
  §14.1). Las pruebas nuevas pueden introducir un único constructor de registros con expresiones.
- **P28.6** — Checklist de validación del Owner: §5.3.

### P29 — migration

**Base.** Leer nunca escribe y re-escribir no conserva bytes, pero sí versión, `ExtensionData` y contenido
(Discovery §5.7).

- **P29.1** — **Sin migración de datos**: ni registros, ni bindings de rack, ni bibliotecas se reescriben al
  abrir.
- **P29.2** — Sin conversión automática de literales en expresiones y sin detección de «valores iguales».
- **P29.3** — La primera escritura que contiene una expresión promueve el registro a `1.1` (P18.2). Quitar
  después todas las expresiones mantiene `1.1`, que sigue siendo legible por las builds I-47/I-48 (P18.4).
- **P29.4** — En una build anterior, un dibujo con expresiones deja sus variables bloqueadas fail-closed. Se
  documenta en el ADR y en la comunicación al usuario; sin herramienta de degradación (P30).
- **P29.5** — La compatibilidad se verifica en G12 con el DLL de `a4d88f1`, sobre un dibujo con expresiones y
  otro sin ellas.
- **P29.6** — Los caminos heredados `SelectiveBindingOptions.ForLength` y `BindingIntent`, sin disparador en
  producción (Discovery §8.1), **no** se extienden (riesgo R15).

### P30 — non-goals

- **P30.1** — Expresiones en campos de propiedad o en `PropertyValues`, una tercera `LinkedPropertySourceKind`
  y la entrada de fórmulas tipo Excel en RACKEDITAR. HANDOFF §4 sigue siendo dirección no normativa.
- **P30.2** — ID21, referencias rack→rack: `RackPropertyReference` y `rackProperty` siguen prohibidos.
- **P30.3** — ID20 productivo: símbolos `Rack.*` y `Project.*`, parámetros calculados y ámbito `Rack`.
- **P30.4** — ID23, Custom / Calculated BOM: cantidades enteras, funciones de redondeo y fórmulas de BOM.
- **P30.5** — UI de ID28 Explain y de ID29 Impact Preview.
- **P30.6** — Nuevos `VariableType` (`Scalar`, ángulo, masa…), áreas y volúmenes, sufijos de unidad,
  conversiones y sintaxis de pies-pulgadas o de fracciones.
- **P30.7** — Funciones distintas de `min` y `max`; condicionales, comparaciones, booleanos, cadenas,
  funciones de usuario, scripting y registro en tiempo de ejecución.
- **P30.8** — Parsers o evaluadores externos (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`,
  Roslyn) y cualquier dependencia NuGet (ADR-0012).
- **P30.9** — Grafo, valores evaluados, trazas o texto tecleado persistidos.
- **P30.10** — Sintaxis de expresión localizada (`;` como separador y `,` decimal) y autocompletado mientras
  se escribe.
- **P30.11** — Control de concurrencia más allá de P21.6.
- **P30.12** — Transferencia o fusión de variables entre dibujos (decisiones de I-47, C2-8).
- **P30.13** — Política de unicidad o de mayúsculas de nombres, y renombrado de variables existentes.
- **P30.14** — Comandos o ventanas nuevos, otros sistemas de rack (Dinámico, Push Back, Cama, Cantilever) y
  nuevas propiedades vinculables.
- **P30.15** — Herramientas de degradación o de exportación a versiones anteriores.
- **P30.16** — Corregir los hallazgos laterales L1–L5 (Discovery §7, §5.8, §8.1, §11.5). Se registran en
  `docs/ideas-futuras.md` (contrato §4). WORKFLOW §8 pide hacerlo «al detectarlo», pero G1 y G2A estaban
  limitados cada uno a un único archivo, así que **el registro sigue pendiente** y se asigna al primer gate
  documental autorizado, a más tardar G4 (§5.1).
- **P30.17** — Cambiar cualquier invariante de I-47/I-48 (contrato §3.2): es condición de parada, no alcance.

---

## 3. OPEN FOR ARCHITECT A — Duplicate-name source syntax

```text
Estado   = OPEN FOR ARCHITECT
V1       = NO fija la sintaxis exacta de identidad cualificada
Congelado = el requisito semántico (abajo)
```

### 3.1 Requisito semántico CONGELADO

Texto del Coordinador:

1. **un homónimo debe terminar ligado a un `VariableId` inequívoco;**
2. **`name` no es autoridad;**
3. **el formatter debe poder representar la fórmula para edición;**
4. **rename no rompe.**

Lo que queda abierto es **la sintaxis exacta de la identidad cualificada**, no estos cuatro requisitos.

### 3.2 Hechos que acotan la decisión

- Los homónimos son **legales** y no hay política de unicidad, mayúsculas ni normalización; Application no
  recorta y la ventana central sí (Discovery §13.1).
- El único desambiguador visible hoy son los **8 primeros caracteres** del `VariableId`, y solo en la lista de
  candidatos del editor. El campo, el estado bajo el campo y la lista de RACKVARIABLES no muestran id
  (Discovery §13.2).
- Ese fragmento es un **prefijo** del texto guardado: dos ids con el mismo primer grupo hexadecimal se verían
  iguales (Discovery §13.3, INFERENCE).
- **Ninguna** ruta de producción resuelve nombre→id, y una guarda lo protege en el control (Discovery §13.3).
- Un filtro *contains* sin distinguir mayúsculas no sirve como regla de coincidencia exacta dentro de una
  fórmula (Discovery §13.4, INFERENCE).
- Los nombres admiten espacios («Holgura General») y ningún carácter está prohibido (Discovery §13.1, §13.4).
  Los mismos caracteres ya tienen significados distintos en otras superficies (Discovery §11.7).

### 3.3 Lo que V1 ya fija con independencia de la sintaxis

- El árbol persistido lleva **ids** y nunca nombres (P17.3), así que el requisito 4 se cumple por construcción
  (P19.5).
- El binder es la **única** ruta nombre→id y solo corre al escribir (P7.3).
- **Nunca** se elige entre homónimos (P7.1).
- El formatter es el único productor de texto editable (P4.2).

El comparador de nombres de P7.2 es una propuesta **revisable dentro de A**.

### 3.4 Preguntas que la decisión tiene que responder

a. Forma textual de una referencia **no** ambigua.

b. Forma cualificada de un homónimo y qué la cualifica: fragmento de id, id completo u otra cosa. Incluye la
   longitud y qué pasa si un fragmento coincide con cero ids o con varios.

c. Cómo se escriben nombres con espacios, operadores, paréntesis, comas o los propios delimitadores, y si hay
   escape.

d. Qué emite el formatter cuando un nombre es único y qué emite cuando pasa a ser ambiguo tras un `Create` o
   un `Rename`. En particular, si la forma mostrada puede depender del estado **actual** del registro.

e. Cómo se muestra una referencia rota (P4.5).

f. Si la identidad cualificada puede **teclearse** o solo insertarse con el selector (P22.5).

g. Sensibilidad a mayúsculas del nombre y del cualificador (P7.2).

h. Interacción con nombres de función (`min`, `max`) y con los namespaces reservados `rack` y `project`
   (P5.2).

### 3.5 Familias ilustrativas

No son exhaustivas, **ninguna es preferida** y cada una tiene que evaluarse contra los mismos cuatro
requisitos y los mismos hechos de §3.2:

- **A-1** — nombre sin delimitar cuando es único; forma cualificada solo para homónimos.
- **A-2** — nombre siempre delimitado; cualificador solo para homónimos.
- **A-3** — referencia siempre cualificada por identidad en el texto, con presentación decorada.
- **A-4** — referencias solo insertables por selección, como token opaco, sin tecleo libre de identidad.

### 3.6 Qué depende de A, y qué no

- **Depende**: P1.2 (producción `reference`), P4.2 y P4.5 (forma de las referencias), P7.1–P7.2 (entrada del
  resolver), P22.5 (token del selector) y las pruebas de sintaxis de G5–G6.
- **No depende**: persistencia, versión, grafo, ciclos, evaluación, propagación, `Delete`, compatibilidad de
  referencias directas ni puntos de extensión.

---

## 4. OPEN FOR ARCHITECT B — Structural readability vs semantic evaluability

```text
Estado    = OPEN FOR ARCHITECT
V1        = NO fija el contrato exacto
Congelado = el requisito (abajo)
```

### 4.1 La cuestión

El contrato exacto cuando una expresión persistida es **sintácticamente / schema-readable** —P17.5 la acepta—
pero contiene:

- `BrokenReference`;
- `Cycle`;
- otro error semántico persistido: `DimensionMismatch`, `ResultTypeMismatch`, `ArityMismatch`,
  `DivisionByZero`, `NonFiniteResult` o `DependencyFailed`.

### 4.2 Requisito CONGELADO

Texto del Coordinador:

1. **geometría/BOM nunca reciben fallback;**
2. **RACKVARIABLES debe seguir teniendo una ruta segura para diagnosticar/corregir estados que esta build
   entiende estructuralmente.**

### 4.3 Hechos que acotan la decisión

- Hoy un registro presente e ilegible bloquea **todo el dibujo**: RACKEDITAR de cualquier Selectivo,
  RACKVARIABLES, RACKBOMTOTAL, opciones de vínculo y commits de registro (Discovery §5.3).
- La identidad ambigua bloquea en la acreditación y en la re-lectura de commit (Discovery §4.2).
- Las referencias rotas de rack se muestran en un panel **fuera** del panel editable, con reparación
  rack-scoped que se puede usar aunque el registro no sea editable (Discovery §10.3).
- ADR-0034 §10: una referencia rota **aborta** el total de BOM en vez de degradarse.
- Decisiones de I-47 C4-8: **nunca** ignorar datos que la build no entiende.
- **INFERENCE** — Con V1, la build nueva no escribiría ninguno de estos estados: su preflight rechaza ciclos
  (P13.3), bloquea borrar con dependientes (P20.1) y exige éxito en el cierre impactado (P21.3). Solo podrían
  aparecer por escrituras que no pasan por ese preflight: ediciones externas del NOD, otra build o un defecto.

### 4.4 Lo que V1 ya fija con independencia de B

- Un símbolo en error y **todos** sus dependientes transitivos no producen valor (P13.4, P14.3).
- **Sin** fallback a literal, a cero ni a un valor anterior (ADR-0034 §8 y §11; DR-5).
- Catálogo de códigos (P15) y separación entre ilegibilidad estructural y error semántico (P17.5–P17.6).

La frontera concreta de P17.5 es **revisable dentro de B**.

### 4.5 Preguntas que la decisión tiene que responder

a. **Alcance del bloqueo**: todo el dibujo, solo el cierre afectado (variables en error, sus dependientes
   transitivos y los racks que los consumen) u otro.

b. Resultado de nivel registro en ese estado: ¿`Usable` con resultados fallidos por símbolo, o un resultado
   nuevo de nivel registro?

c. RACKEDITAR de un Selectivo que **no** consume nada afectado, y de uno que **sí**.

d. RACKBOMTOTAL: ¿aborta el total entero, o solo si algún rack cotizado consume el cierre afectado?

e. Qué intents admite RACKVARIABLES en ese estado —solo correctivos sobre los símbolos en error, cualquiera
   cuyo cierre impactado quede limpio, u otros— y cómo se garantiza que una corrección nunca produzca un
   plan parcial.

f. Cómo se presenta y se corrige un ciclo persistido de varias variables, donde cambiar una sola definición
   puede bastar.

g. Relación con `RepairBrokenRack` y con el sexto resultado de `InspectBinding` (P24.4).

h. Si algún caso de P17.5 debe cambiar de lado, por ejemplo la aridad.

i. Qué hace la re-lectura de commit (P21.6) si contiene errores semánticos **fuera** del cierre impactado.

### 4.6 Familias ilustrativas

No son exhaustivas y **ninguna es preferida**:

- **B-1** — Bloqueo de nivel dibujo, equivalente a ilegible para todo consumidor, con RACKVARIABLES en modo de
  diagnóstico que solo admite intents correctivos.
- **B-2** — Bloqueo limitado al cierre afectado: el resto del dibujo opera con normalidad y RACKVARIABLES
  admite correcciones.
- **B-3** — Híbrido: geometría y BOM se bloquean por cierre, y las mutaciones se limitan a correctivas
  mientras exista cualquier error.

### 4.7 Qué depende de B

P17.5 (frontera), P20.6, P21.6 (errores fuera del cierre), P22.9, P23.3 (apertura y opciones), P24.4–P24.5 y
las pruebas de esos estados en G8–G10.

---

## 5. Gates G0–G12

> **Propuestos, NO autorizados.** Solo se abre el gate siguiente con evidencia revisable del anterior
> (contrato §8). La división de G5–G10 puede ajustarse en la revisión.

### 5.1 Secuencia

| Gate | Nombre | Entregable | Evidencia exigida | Estado |
|---|---|---|---|---|
| **G0** | Reclamo + bootstrap | Reclamo atómico `77262fe`; contrato, decisión del Owner y fila de ROADMAP `f2d28a2` | Push aceptado sin force | **HECHA** |
| **G1** | Discovery read-only | [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1` | CI 4/4 en ese SHA | **HECHA** |
| **G2** | Proposal | V1 (G2A) y las versiones que pida la revisión, hasta una congelada con A y B cerradas | Architect Reviews | **EN CURSO** (G2A) |
| **G3** | Consenso + freeze | `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal, más la decisión del Owner | Registro en `decisions/I-49.md` | pendiente — **compuerta** |
| **G4** | ADR + alineación documental | ADR aceptado por el Owner, con número asignado **en ese momento**; contrato y ROADMAP alineados con la Proposal congelada; L1–L5 registrados en `ideas-futuras.md` si no se hizo antes | ADR en estado `aceptado` | NO AUTORIZADA |
| **G5** | Núcleo sintáctico | Lexer, parser, sintaxis, formatter, límites y diagnósticos de sintaxis; guardas G1 (parcial), G4, G5 e independencia del núcleo | RED→GREEN; focal + impacto | NO AUTORIZADA |
| **G6** | Núcleo semántico | `SymbolId`, namespaces, ámbitos, `ExpressionContext`, tabla de símbolos, `SymbolResolver` y binder con la sintaxis de A, reglas de tipo, `FunctionRegistry`, evaluador y `EvaluationResult` | RED→GREEN | NO AUTORIZADA |
| **G7** | Núcleo de grafo | Extracción de dependencias, grafo, ciclos, orden y evaluación por snapshot; guarda G1 completa | RED→GREEN | NO AUTORIZADA |
| **G8** | Integración con Project Variables + persistencia | `VariableDefinition.Expression`, DTO y store (P17), versión (P18), evaluación tras acreditación, target evaluado, resolver y sexto resultado (P24), contrato de B, BOM y exportación; guardas G2, G3 y G12 | RED→GREEN; fixture dorado literal-only | NO AUTORIZADA |
| **G9** | Mutaciones y propagación | `ChangeDefinition`, `Create` y `Rename` con expresiones, `Delete` y `UnlinkAllAndDelete` con dependientes, cierre impactado, descubrimiento por conjunto con oráculo, re-lectura de commit (P21.6); guardas G13 y G14 | RED→GREEN | NO AUTORIZADA |
| **G10** | UX | RACKVARIABLES (P22) y `LinkedPropertyEditor` (P23); guardas G18 y G20 | RED→GREEN; suite UI | NO AUTORIZADA |
| **G11** | Candidato | Pruebas reales de cadena hasta geometría y BOM; las dos suites en local; builds Debug de UI y Plugin; CI verde sobre el SHA exacto; medición de coste de la propagación (R5) | Clases de evidencia de AGENTS.md y WORKFLOW §4–§5 | NO AUTORIZADA |
| **G12** | Owner Validation + integración | Checklist de §5.3 en AutoCAD 2025; integración **serializada** con I-50; cierre documental (HANDOFF y ROADMAP); CI posterior al merge | WORKFLOW §4.5 | NO AUTORIZADA |

### 5.2 Reglas de la secuencia

- G5, G6 y G7 **no** tocan la producción de Project Variables, ni el Plugin, ni la UI: el núcleo nace
  aislado, y solo evolucionan las guardas que P28.4 asigna a esos gates. G8 es el primer gate que toca
  producción de Project Variables, y G10 el primero que toca UI.
- Antes de editar **cualquier** archivo productivo, cada gate de G5 a G10 ejecuta el procedimiento de colisión
  de [`decisions/I-49.md`](../automation/decisions/I-49.md) §8 contra I-50, y la misma comprobación contra I-51,
  que ya tiene producción desde `4c79e4a` (§9).
- El Plugin se toca solo donde G9 o G10 lo exijan, archivo por archivo y con la comprobación anterior.
- Un gate que descubra que necesita cambiar una invariante de I-47/I-48 **se detiene** (§10).

### 5.3 Checklist propuesto de Owner Validation (G12, AutoCAD 2025)

1. Crear una variable literal y otra definida por expresión que dependa de ella; vincular una propiedad de un
   Selectivo a la dependiente; geometría y BOM reflejan el valor evaluado.
2. Cambiar la variable base: se redibujan los racks de la dependiente, con un solo `Regen`.
3. Renombrar la base: la expresión muestra el nombre nuevo, sin redibujo.
4. Intentar borrar la base con una dependiente: bloqueado, con la lista de dependientes.
5. Intentar crear un ciclo: rechazado, sin mutación.
6. Escribir una expresión con error de sintaxis: diagnóstico visible, sin mutación.
7. Homónimos: la referencia queda ligada al `VariableId` elegido, con la sintaxis que fije A.
8. Guardar, cerrar y reabrir: definiciones, valores y vínculos intactos.
9. Abrir con el DLL de `a4d88f1` un dibujo **con** expresiones: variables bloqueadas fail-closed, sin
   escritura.
10. Abrir con el DLL de `a4d88f1` un dibujo literal-only guardado por la build nueva: funciona como hoy.
11. RACKBOMTOTAL con racks ligados a variables derivadas.
12. Un registro con un error semántico persistido (fixture preparado): se comporta según el contrato de B.

---

## 6. ADR REQUIRED — número NO asignado

- **Por qué es obligatorio.** V1 cambia persistencia y schema (P17, P18) y toma decisiones de arquitectura
  (DR-1 a DR-5, frontera del núcleo). WORKFLOW §8 y el contrato §3.1 exigen que el ADR esté **aceptado antes
  de implementar**.
- **Número NO asignado.** Se asigna en G4 contra la numeración remota de ese momento. **ADR-0035 ya lo usa
  I-50**, en estado propuesto, en `origin/feature/cotas-independientes-por-vista` @ `0d7670c` (Discovery
  §19.1). I-49 no lo reutiliza ni reserva otro de antemano.
- **No se escribe en G2A.**
- **Qué tendrá que registrar**, tras G3:
  - DR-1 a DR-5 y la frontera del núcleo (P26.1);
  - la evolución a `1.1`, su efecto sobre las builds anteriores y la alternativa major rechazada (P18);
  - la frontera estructural/semántica y el contrato de B (P17.5, §4);
  - la sintaxis de A (§3);
  - la gramática numérica invariante como excepción **acotada** a ADR-0015 (P1.11);
  - la ausencia de conversión de unidades, con ADR-0005 intacto (P9);
  - los namespaces y el ámbito reservados para ID20 (P25.2);
  - las alternativas rechazadas (§7.1).
- **Relación con los ADR vigentes.** Extiende ADR-0034 en los puntos de §1.3, por su §15. No sustituye ADR-0005,
  ADR-0006, ADR-0012, ADR-0015 ni ADR-0021. ADR-0034 está aceptado y es **inmutable**: no se edita.

---

## 7. Alternativas y coste

### 7.1 Alternativas consideradas

| # | Alternativa | Por qué no en V1 |
|---|---|---|
| ALT-1 | Expresiones en el **campo de propiedad** (`=Holgura General + 2` en RACKEDITAR), dirección de HANDOFF §4 | Sale del punto de extensión de ADR-0034 §15, que reserva `PropertyValue<T>` para ID21. Cambia el contrato de `=` fijado por G20 y validado con el Owner en I-48 (riesgo R10). Toca la persistencia de rack y el restamp que I-51 caracteriza (T14) y los archivos calientes que I-50 declara. Exigiría ADR nuevo |
| ALT-2 | Persistir el **texto con nombres** | Un rename rompería las expresiones y el nombre pasaría a ser autoridad (ADR-0034 §2, HANDOFF §4) |
| ALT-3 | Persistir **texto canónico con ids** en una cadena | Viable. V1 prefiere el árbol JSON para no acoplar la persistencia a la sintaxis de OPEN A ni exigir un parser en la lectura. El Arquitecto puede reabrirlo |
| ALT-4 | **Cachear el valor evaluado** en el registro | Segunda autoridad que puede quedar obsoleta; no ayuda a las builds anteriores, que rechazan el kind entero (P17.4) |
| ALT-5 | **Persistir el grafo** de dependencias | Es derivable, y persistido podría divergir del registro (P12.1) |
| ALT-6 | **Major `2.0` sticky** | Dejaría el dibujo ilegible para siempre en builds anteriores aunque se quitaran las expresiones; C4-8 ya hace fail-closed un kind nuevo sin major (P18.5) |
| ALT-7 | Evaluador de **terceros** (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) | ADR-0012; semántica sensible a cultura o no acotada; sin tipado dimensional (P1.10) |
| ALT-8 | Núcleo **dentro** de `RackCad.Application.ProjectVariables` | Arrastraría esa dependencia a ID23, contra OWNER INPUT (Discovery §16.3) |
| ALT-9 | Núcleo en un **ensamblado separado** | Frontera más dura, pero añade un proyecto a builds, CI y despliegue del Plugin. V1 prefiere namespace + guarda (P26.4); el Arquitecto puede pedirlo |
| ALT-10 | Evaluar **dentro del resolver**, por rack | Multiplicaría evaluaciones y rompería «un snapshot por operación» y las guardas G11/G12 (P14.2, P14.7) |
| ALT-11 | **Borrado en cascada** o materialización automática de dependientes | Reescribe en silencio definiciones del usuario; ADR-0034 §11 ya rechaza la versión de consumidores (P20.1) |
| ALT-12 | **Sufijos de unidad** en V1 | Exige decisión de unidades con ADR (ADR-0005 §4); fuera del alcance mínimo (P9) |
| ALT-13 | Gramática **localizada** (`,` decimal y `;` separador) | Doble significado de la coma (Discovery §11.7) y forma canónica dependiente de la cultura; V1 lo mitiga con `AmbiguousDecimalComma` (P1.5) |

### 7.2 Coste estimado (INFERENCE, se valida gate a gate)

| Área | Superficie estimada | Base |
|---|---|---|
| Núcleo nuevo | ≈12–18 archivos en un namespace nuevo de Application | P1–P16 |
| Project Variables y persistencia | ≈15–20 archivos existentes: las ubicaciones que asumen literal, más descubrimiento, preflight, commit, workspace e intents | Discovery §5.5, §3.1–§3.2 |
| Selectivo en Application | ≈3: resolver, apertura del editor y exportación | Discovery §3.2 |
| Plugin | 0–3 | Discovery §3.3 |
| UI | ≈3 (`RackProjectVariablesWindow.xaml` y `.cs`, texto de reparación); `RackSelectiveWindow` idealmente 0 | Discovery §3.4; P23.7 |
| Pruebas | Núcleo nuevo, ≈15 archivos existentes y la evolución de guardas de P28.4 | Discovery §14 |
| Comandos / ventanas nuevos | 0 / 0 | P22.1 |

**Costes que no se minimizan:**

- Nace la **primera** ruta productiva nombre→id (P7.3).
- El contrato de B puede ampliar la UX de errores.
- El commit re-evalúa el cierre impactado (P21.6).
- Las builds anteriores pierden las variables de un dibujo con expresiones (P18.4).
- La propagación sobre dibujos grandes sigue **sin medir** hasta G11 (ADR-0034, riesgos).
- Evolucionan varias guardas vigentes y nace una nueva (P28.4).
- La ventana de RACKVARIABLES crece.

---

## 8. Reconciliación con el Discovery G1

### 8.1 Las 23 preguntas abiertas (Discovery §18)

| # | Pregunta del Discovery | Respondida en |
|---|---|---|
| 1 | Dónde vive una expresión | DR-1; P23.2; P30.1 |
| 2 | Representación persistida | P4; P17.3; ALT-2, ALT-3 |
| 3 | Versión del registro | P18 |
| 4 | Token y payload del nuevo kind; `ExtensionData`; `PropertyNameCaseInsensitive` | P17.3, P17.5, P17.10 |
| 5 | Evaluación: dónde, cuándo y si se persiste | DR-3; P14.2, P14.7; P17.4; P21.2, P21.6 |
| 6 | Grafo y ciclos | P12; P13; P15.3 |
| 7 | Propagación transitiva | P21 |
| 8 | Ciclo de vida con dependientes | P19; P20; P21; **OPEN B** |
| 9 | Tipos | P5.4–P5.5; P9.4 |
| 10 | Unidades | P9 |
| 11 | Gramática numérica | P1.4–P1.5, P1.11; P22.3 |
| 12 | Nombres dentro de expresiones | P7; **OPEN A** |
| 13 | Editor | P23 |
| 14 | RACKVARIABLES | P22 |
| 15 | ID20 (preparar) | P5.2–P5.3; P25 |
| 16 | ID23 (independencia) | DR-2; P26 |
| 17 | ID28/ID29 (preservar) | P16.3; P27 |
| 18 | Guardas de fuente | P28.4 |
| 19 | Compatibilidad de DWG I-47/I-48 | P18.4; P24.8; P28.3 (G8); P29; §5.3 (9–10) |
| 20 | ADR | §6 |
| 21 | Alcance de sistemas y propiedades | P30.14; P23.5 |
| 22 | Semántica numérica del resultado | P14.4; P21.7 |
| 23 | Hallazgos laterales L1–L5 | P30.16; G4 |

### 8.2 Los 16 riesgos (Discovery §17)

| Riesgo | Tratamiento en V1 |
|---|---|
| R1 — legibilidad en builds anteriores | Minor `1.1` solo con expresiones y literal-only en `1.0` (P18); verificación con el DLL de `a4d88f1` (§5.3, puntos 9–10) |
| R2 — segunda autoridad de valor | Una evaluación por snapshot antes del resolver único; sin valor cacheado (DR-3; P14.7; P17.4; P24.3) |
| R3 — guardas de fuente | Evolución explícita, en el mismo gate y sin renombrar para esquivarlas (P28.4) |
| R4 — profundidad 1 embebida | Cierre transitivo deduplicado por rack en un solo plan (P21.2, P21.11) |
| R5 — coste sin medir | Descubrimiento por conjunto en un barrido (P21.4); grafo `O(V + E)` (P12.6); medición en G11 |
| R6 — nombres en fórmulas | Binder como única ruta nombre→id, solo al escribir, sin elegir entre homónimos (P7); sintaxis en **OPEN A** |
| R7 — gramática numérica divergente | Gramática invariante con `AmbiguousDecimalComma`; la entrada literal conserva ADR-0015 (P1.4–P1.5, P1.11, P22.3) |
| R8 — unidades | Pulgadas, sin sufijos ni conversión (P9) |
| R9 — acoplamiento al Selectivo | Núcleo neutral protegido por guarda (DR-2; P26) |
| R10 — semántica actual de `=` | Sin cambio, más un pin nuevo (P23.1–P23.2) |
| R11 — pérdida de trazabilidad | Traza y grafo inverso en memoria, cierre impactado en el preflight (P27) |
| R12 — colisión potencial con I-50 | DR-1 no toca los portadores authored; `RackSelectiveWindow` mínimo o sin cambios; procedimiento de colisión (P23.7; §9) |
| R13 — defectos laterales heredados | Fuera de alcance; registro pendiente (P30.16) |
| R14 — extensión muerta | Pruebas sobre el camino operativo store → acreditación → evaluación → resolver → geometría y BOM, no solo sobre el modelo (P28.3, G8 y G11) |
| R15 — caminos heredados sin disparador | No se extienden (P29.6) |
| R16 — materializar valores calculados | Valor evaluado exacto, sin redondeo, con el ruido de coma flotante aceptado (P21.9) |

---

## 9. Coordinación con I-50 e I-51

**Observado en el preflight de G2A** (`git fetch --all --prune`, 2026-09-12):

```text
origin/architecture/motor-expresiones-parametricas = 4cf02b167f183fb66d93988c9843296838277dc4  (= DISCOVERY_SHA)
origin/main                                        = a4d88f18a1f42263d366c44dc05dd18a6786f152
I-50  origin/feature/cotas-independientes-por-vista    = 0d7670c1c1e3de6257270d9723d925c1b687d378  (sin cambios desde G1)
I-51  origin/feature/rackduplicar-multiples-origenes   = 4c79e4a46b2adf45aee87ffb399c786f25d5d274  (avanzó desde c4cc2e4)
```

### 9.1 I-50

- **FACT** — Solo `docs/`: ROADMAP, ADR-0035 propuesto, índice de ADR, `ideas-futuras.md`, su contrato, su
  Discovery y sus Proposals V1 y V1.1. **Sin colisión productiva real.**
- **FACT (doc de I-50)** — Su ADR-0035 propuesto prevé `int? DimensionViews` en `SelectivePalletDesignDocument`
  y `DynamicRackSystemDocument`, declara calientes los tres editores y dice que los cambios en
  `RackSelectiveWindow` «se coordinan con I-49» (Discovery §19.2).
- **Diseño de V1 frente a ese cruce**: DR-1 **no** toca el portador authored (`From`/`WithDesign`) ni
  `PropertyValues`, y P23.7 reduce a mínimo, o a nada, los cambios en `RackSelectiveWindow`.
- **Condiciones de `OWNER_OVERRIDE_I49_I50_PARALLEL`**: G2A solo crea este documento, así que la condición de
  colisión **no se activa**. Desde G5, cada edición productiva pasa antes por el procedimiento de §8 del
  registro de decisión.

### 9.2 I-51

- **FACT** — `4c79e4a` («I-51 G3: planificador puro de duplicacion multiorigen y pruebas T1-T15») añade
  producción:
  - `src/RackCad.Application/Persistence/RackDuplicationPlan.cs` (nuevo);
  - `tests/RackCad.Tests/RackDuplicationPlanTests.cs` (nuevo);
  - `tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs` (modificado: T14–T15 caracterizan que dos
    vínculos reales sobreviven al restamp).
- **FACT** — El planificador usa `SelectiveAuthoredAuthority.IsSameAuthority`, `SelectivePalletDesignStore`,
  `SelectivePalletDesignDocument` y `RackEmbedDocument`.
- **Sin colisión productiva real**: G2A no toca producción, y V1 no propone cambiar ninguno de esos archivos
  ni esas API (DR-1; P24.6).
- **Condición de parada**: si un gate de I-49 necesitara cambiar la API o la semántica de
  `SelectiveAuthoredAuthority`, del portador o del store del Selectivo, o de `SelectiveDuplicationFailClosedTests.cs`,
  se detiene **antes de editar** y lo reporta (contrato §12). El override **no** cubre este par.

### 9.3 Conflictos documentales previstos

- Las tres ramas insertan su fila de ROADMAP tras I-48.
- I-50 e I-51 modifican `docs/ideas-futuras.md`, donde I-49 registrará L1–L5 (P30.16).
- La numeración de ADR: 0035 es de I-50 (§6).

La integración es **serializada**, y la segunda en integrar se reconcilia con `main`.

---

## 10. Condiciones para detenerse

Se heredan todas las del contrato §12. Además:

- Si cerrar **A** o **B** exige **cambiar** una invariante de I-47/I-48 en lugar de extenderla: detenerse;
  eso es ADR nuevo y decisión del Owner.
- Si el núcleo no puede quedar **independiente** de Project Variables (P26.1): detenerse.
- Si la evolución de una guarda debilitara la protección de **ID21** (P28.4): detenerse.
- Si la implementación necesitara expresiones en `PropertyValues` o una tercera fuente de edición (fuera de
  DR-1): detenerse.
- Si la medición de G11 muestra un coste de propagación inaceptable: detenerse y escalar.
- Colisión productiva material con I-50 o con I-51: detenerse **antes de editar** (§9).
- Cualquier edición de producción antes de G3 y del ADR aceptado en G4: **prohibida**.

---

## 11. Estado

```text
COORDINATOR PROPOSAL V1 — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator    = PROPOSED
Architect      = PENDING
Implementation = BLOCKED
ADR            = REQUIRED (número NO asignado)

30 puntos      = P1..P30 (§2)
Gates          = G0..G12 (§5): G0 y G1 HECHAS · G2 EN CURSO · G3 PENDIENTE · G4..G12 NO AUTORIZADAS

OPEN FOR ARCHITECT:
  A. Duplicate-name source syntax                            (§3)
  B. Structural readability vs semantic evaluability         (§4)

Base: Discovery G1 @ 4cf02b167f183fb66d93988c9843296838277dc4
      docs/initiatives/I-49-discovery.md

Siguiente paso: revisión del Arquitecto sobre V1. G2A no la solicita.
```
