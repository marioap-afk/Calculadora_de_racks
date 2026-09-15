# ADR-0043: Motor de expresiones paramétricas, edición de fórmulas, identidad textual y recuperación segura con causas múltiples

- **Estado:** aceptado
- **Fecha:** 2026-09-15 (propuesto, ADR-0043-P1) · 2026-09-15 (aceptado, ADR-0043-P4)
- **Decisores:** Mario Pérez, Owner del repositorio (**acepta**, 2026-09-15); Coordinador y Arquitecto de I-49
  (**AGREED WITH ADR-0043** sobre el blob propuesto exacto `164ac166080d0b2be660473ec721110301a46dbf`);
  Codex (redacción).
- **Reemplaza a:** [ADR-0041](0041-motor-expresiones-parametricas-identidad-textual-y-cualificador-clave-exacta.md),
  entero; ADR-0041 queda como autoridad histórica, `reemplazado por ADR-0043`.
- **Iniciativa relacionada:** I-49 — `architecture/motor-expresiones-parametricas`
  ([contrato](../initiatives/I-49-motor-expresiones-parametricas.md),
  [Discovery](../initiatives/I-49-discovery.md), [registro](../automation/decisions/I-49.md)).

> **Aceptación del Owner (2026-09-15).** El Owner, Mario Pérez, acepta esta decisión de forma explícita en el canal
> del Coordinador de I-49, con la respuesta literal: «**Acepto ADR-0043 para I-49 sobre Proposal V6 + Amendments A1 +
> A2 + A3-R2.**». La respuesta nombra el ADR, la iniciativa y la base técnica completa. El registro durable está en
> [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) §18.
>
> **Dos actos, dos autoridades.** El Coordinador y el Arquitecto aportaron el consenso técnico exacto, ambos
> `AGREED WITH ADR-0043`, sobre la propuesta versionada en
> `d54a8d7db830d763af293ab87ccd981c478c6272`, blob `164ac166080d0b2be660473ec721110301a46dbf`,
> sin hallazgos materiales en la revisión exacta del Arquitecto. El Owner ejerce aquí el acto distinto de aceptación.
>
> **Contenido aceptado.** Es exactamente el contenido técnico de la propuesta indicada: Proposal V6, blob
> `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, leída con Amendment A1, blob
> `d62019088b9e7a140d5066799afe6ace6db303ba`; Amendment A2, blob
> `49a925336dd3775929a35b0f40b73cb7f8c487f7`; y Amendment A3-R2, blob
> `4da6ef3caa21dcf31140983c6db7e23f02aa3e18`. La aceptación cubre D1–D25 completos, P2.5, la identidad textual de
> `VariableId`, `Q(key)`, la guarda del parser, las causas múltiples, `RootCauses`, `RecoveryUnit`, la recuperación de
> ciclos, `PlanReadSet`/`Upstream`, `RepairDecisionObservation` y Schema V-0.
>
> **Inmutabilidad del contenido técnico.** En esta aceptación solo cambian el encabezado, este preámbulo y el bloque
> «Decisión del Owner». Desde «Base exacta y frontera de decisión» hasta el final, el texto de la propuesta permanece
> byte a byte y desde ahora es inmutable ([README](README.md)). Por eso conserva en su contexto histórico expresiones
> condicionadas a que ADR-0043 siguiera `propuesto`; esa condición ya no se cumple.
>
> **Reemplazo y gates.** La aceptación hace efectivo el reemplazo entero de ADR-0041 por ADR-0043. En el mismo commit,
> ADR-0041 cambia solo su metadata de sucesión y recibe una nota posterior fechada. G6 permanece cerrado. G7 sigue
> bloqueado hasta que un nuevo Consensus Freeze sobre V6 + A1 + A2 + A3-R2 + ADR-0043 aceptado se versione y su CI
> sobre el SHA exacto quede verde; G8 permanece bloqueado. Esta aceptación no implementa G7 ni crea todavía el freeze.

## Decisión del Owner

```text
Proposal Version  = V6 + Amendment A1 + Amendment A2 + Amendment A3-R2
V6 blob           = ef4db3aa400483ff25a8f39b2beb93708fa43d1a
A1 blob           = d62019088b9e7a140d5066799afe6ace6db303ba
A2 blob           = 49a925336dd3775929a35b0f40b73cb7f8c487f7
A3-R2 blob        = 4da6ef3caa21dcf31140983c6db7e23f02aa3e18

ADR proposal SHA  = d54a8d7db830d763af293ab87ccd981c478c6272
ADR proposal blob = 164ac166080d0b2be660473ec721110301a46dbf

Coordinator       = AGREED WITH ADR-0043
Architect         = AGREED WITH ADR-0043

Owner decision    = ACCEPTED
Owner             = Mario Pérez
Fecha             = 2026-09-15
Canal             = Coordinador I-49
Respuesta literal = "Acepto ADR-0043 para I-49 sobre Proposal V6 + Amendments A1 + A2 + A3-R2."

ADR-0041          = REPLACED BY ADR-0043
G6                = CLOSED
G7                = BLOCKED UNTIL NEW CONSENSUS FREEZE IS VERSIONED AND GREEN
G8                = BLOCKED
```

La respuesta es inequívoca y vinculante: nombra ADR-0043, I-49 y la base técnica V6 + A1 + A2 + A3-R2 sobre la que
se acepta. El contenido normativo aceptado es D1–D25 completo con la precedencia declarada por esos cuatro documentos.

## Base exacta y frontera de decisión

| Autoridad | Blob exacto |
|---|---|
| [Proposal V6](../initiatives/I-49-proposal-v6.md) | `ef4db3aa400483ff25a8f39b2beb93708fa43d1a` |
| [Amendment A1](../initiatives/I-49-proposal-v6-amendment-a1-text-guard.md) | `d62019088b9e7a140d5066799afe6ace6db303ba` |
| [Amendment A2](../initiatives/I-49-proposal-v6-amendment-a2-exact-key-qualifier.md) | `49a925336dd3775929a35b0f40b73cb7f8c487f7` |
| [Amendment A3-R2](../initiatives/I-49-proposal-v6-amendment-a3-multi-cause-dependency-failures.md) | `4da6ef3caa21dcf31140983c6db7e23f02aa3e18` |
| ADR-0041 aceptado, base de este sucesor | `c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231` |

```text
Coordinator (base) = AGREED WITH V6
                     AGREED WITH A1
                     AGREED WITH A2 / E1
                     AGREED WITH A3-R2
Architect (base)   = AGREED WITH V6
                     AGREED WITH A1
                     AGREED WITH A2
                     AGREED WITH A3-R2
A3 versioning SHA = 5210c1c0534d3cf5bee4d23526f24e062bdc77c1
A3 exact consensus record = addb5223265612785dfe19ef1f353a19307f0a83
ADR-0043 Coordinator exact review = REQUIRED
ADR-0043 Architect exact review   = REQUIRED
Owner             = PENDING
ADR-0041          = ACCEPTED / CURRENT
ADR-0043          = PROPOSED
G6                = CLOSED
G7                = BLOCKED
G8                = BLOCKED
```

**Consenso técnico != aceptación del Owner.** El consenso de A3-R2 está registrado en
[`decisions/I-49.md`](../automation/decisions/I-49.md) §16, en el commit exacto indicado. Solo su blob final está
acordado: A3 original y A3-R1 recibieron `CHANGES REQUIRED`. Ese consenso no constituye revisión ni acuerdo sobre
el blob de esta propuesta. La CI verde tampoco los constituye. Solo el Owner puede aceptar o rechazar el ADR.

**Por qué hace falta un sucesor completo.** ADR-0041 está aceptado y es inmutable ([regla](README.md)). A3 cambia
materialmente D20 (datos estables y coincidencia de `Upstream`), D18 puntos 3–4 (ciclo simple y bloqueo estructurado)
y D14 decisión 7 (recuperación de ciclos). Este ADR propone reemplazarlo entero y conserva D1–D25: parte del texto
aceptado e incorpora las lecturas de A3 sin reabrir las decisiones ajenas a ese alcance. No es un archivo de parches.
D1–D3, D5–D10, D12–D13, D16–D17, D21, D23 y D25 se conservan textualmente; D8 mantiene A1 y A2 sin cambio.
D4, D11, D14–D15, D18–D20, D22, D24 y Consecuencias explicitan las reglas que A3 §10.3 y §12 les asignan.

**Precedencia y referencias.** La base técnica es V6 leída con A1, A2 y A3-R2 en sus blobs exactos. A1 manda solo en
la guarda de recurso del parser; A2, en la identidad textual conservada de `VariableId`, su validez neutral y la
gramática y el formatter del cualificador; A3, en sus cláusulas enumeradas en A3 §10, sobre fallos, raíces,
recuperación y observaciones. Fuera de esos alcances manda V6. Las referencias `Pn.m`, `DR-n`, `ALT-n`, `T-Vn-…`
y `§n` sin prefijo remiten a V6; `A1 §n`, `A2 §n` y `A3 §n`, a sus respectivos blobs. M1–M10 y DI-1–DI-4 nombran
las reglas de A3, incorporadas aquí; U1–U11, CX-n, N-n y T-A3-n remiten a sus casos y obligaciones. Las líneas
citadas de V6 y de `D…` son las de los blobs base de V6 y ADR-0041, no las líneas de este documento. En las causas
de descubrimiento, las referencias abreviadas `:n` pertenecen a `ProjectVariableConsumerDiscovery.cs`, según el
censo de A3. Los nombres de tipos son ilustrativos; el comportamiento es el contrato.
Los estados pendientes en las cabeceras históricas de V6/A1/A2/A3 no se reescriben: los acuerdos posteriores están
en el registro de decisiones. V1–V5 y las revisiones rechazadas de A3 son historia, no autoridad alternativa.

**Lectura conjunta de A2, intacta.** Toda mención heredada de `#<id>`, `#<GUID>`, `#GUID` o «GUID completo» como
cualificador se lee como `Q(clave)` en la salida y como cualquiera de las dos formas de D7 en la entrada. No se
resuelve una duda hacia la antigua restricción a la forma D. Se conservan P2.5 fuerte e incondicional, A1 y las
identidades textuales de A2, sin normalización a identidad por valor de `Guid`.

**Estado de implementación y siguiente frontera.** G6 cerró en
`75f18623e7ced836de7eb2e36d6db8e311efb76d`. El freeze V6 + A1 + A2, blob
`57736f725663aab79a24b29685ed34e8c3e9ebad`, permanece como evidencia histórica; A3 invalida su autorización para
seguir implementando G7. Esta propuesta no cambia producción, pruebas, ADR-0041 ni ningún freeze. Secuencia pendiente:
propuesta versionada y CI verde; revisión exacta del Coordinador; revisión exacta del Arquitecto; solo con ambos
acuerdos, aceptación explícita del Owner; ADR-0041 marcado reemplazado; nuevo freeze V6 + A1 + A2 + A3 + ADR-0043
aceptado; CI verde de ese SHA exacto; solo entonces G7 RED. G8 permanece bloqueado.

**Numeración.** El censo A3-R3 y el recenso anterior a crear esta propuesta comprobaron archivos e índices de `main`,
ramas activas, tags de archivo y estado local visible. `main` llega a 0039; I-52 usa 0036; I-49, 0038/0040/0041;
I-55, 0042; I-56 no reserva ADR. El borrador local View Foundation de I-55 declara número pendiente y ninguna reserva.
0043 estaba libre. Se vuelve a comprobar antes del commit; una colisión bloquea la publicación sin renumeración tácita.

## Contexto

[ADR-0034](0034-project-variables-autoridad-drawing-level.md), aceptado e inmutable, fundó las variables de proyecto
como autoridad de nivel dibujo: un `VariableId` estable que no depende de `Name` (§2), un único tipo `Length` con
**definición literal** (§3), propiedades que valen `Literal(T)` o `ProjectVariableReference(VariableId)` (§4), una
resolución central única (§6) y una propagación atómica de **profundidad 1** (§7). Su §15 dejó **deliberadamente
fuera** las fórmulas (ID22B, que «extenderá `ProjectVariable.Definition`») y las referencias entre racks (ID21, que
«extenderá `PropertyValue<T>`»). I-48 generalizó la edición vinculable en un único `LinkedPropertyEditor`, donde el
mismo campo acepta un literal o `=Nombre` dentro del protocolo C4.

En el punto de partida documentado por el Discovery, el territorio que ID22A dejó para ID22B no existía: no había parser de expresiones, AST, evaluador,
grafo de dependencias entre variables, detección de ciclos, orden de evaluación ni sintaxis de unidades (Discovery
§15), y el preflight vigente es de profundidad 1 (contrato de I-49 §2). El Owner fijó además requisitos concretos:
**dos superficies productivas** de expresión —la definición de una variable y la fuente de una propiedad vinculable en
el **mismo** `LinkedPropertyEditor`—, un motor **adimensional**, unidades `[mm]`, `[in]` y `[ft]`, las funciones
`MIN`, `MAX` y `ABS`, **un** plan con **una** transacción, **un** commit y **un** `Regen`, un núcleo reutilizable por
ID23 sin depender de Project Variables, y la preparación —no la implementación— de ID20, ID28 e ID29 (DR-1, DR-2;
V6 §1.3).

La Proposal V6 responde a esos requisitos tras seis versiones —de V3 a V6, reconciliadas con las revisiones del
Arquitecto— y la comprobación final del Arquitecto, que cerró el consenso **AGREED WITH V6**. Sus decisiones cambian
contratos persistidos (P17), **extienden ADR-0034**
(§2.3), **revisan de forma acotada** una decisión de I-48 (V8-R05) y toman decisiones de arquitectura (DR-1 a DR-8).
Por eso el ADR es obligatorio y tiene que estar aceptado antes de implementar (V6 §9; WORKFLOW §8; contrato de I-49
§3.1, §11.2 y §12).

**Amendment A1 (2026-09-13).** En la compuerta G6.0, antes de escribir producción de G6, apareció una incompatibilidad
entre dos reglas de V6: la guarda **finita de caracteres** del parser (P1.9; D8 de ADR-0038) y la canonicidad universal
`Canonicalize(Bind(Parse(Format(b)))) == b` (P2.5). Para cualquier límite finito `L` existe una variable legal —hoy un
nombre solo tiene que no estar en blanco— cuyo nombre son `L + 1` letras: `Reference(id)` se formatea con el nombre
completo, ningún texto más corto enlaza con esa variable (P7.1) y el parser rechaza el texto antes del bind. El
[Amendment A1](../initiatives/I-49-proposal-v6-amendment-a1-text-guard.md) sustituye **solo** esa guarda de recurso por
una guarda de complejidad sintáctica, sin tocar P2.5, los nombres ni los límites normativos, y el Coordinador y el
Arquitecto están **AGREED WITH A1**. Ese cambio exigió ADR-0040 como sucesor de ADR-0038, entonces aceptado e inmutable.

**Amendment A2 (2026-09-13).** Con el candidato de G6 en `5f969cc87acb6cbd8572af3e2b517ba3468e5c41` y su CI verde, la
revisión de solo lectura G6-C1 y la revisión de identidad del Arquitecto midieron que `VariableId.TryParse` acepta
exactamente los textos que acepta `System.Guid.TryParse` —las disposiciones D, N, B, P y X y variantes de
compatibilidad— y conserva el texto recortado, con igualdad y hash `OrdinalIgnoreCase` sobre ese **texto**: D, N, B, P
y X del mismo GUID son identidades distintas, y el store y la acreditación las aceptan. El
[Discovery](../initiatives/I-49-discovery.md), §4.1, no registró esas disposiciones, y V6 fijó el cualificador en forma
D (P3.9; D6 y D7 de ADR-0040). Dos contraejemplos medidos rompen P2.5: en **R1**, con una variable cuyo id está en forma
N y un homónimo con id en forma D, ningún texto que V6 admite enlaza con la variable de id N; en **R2**, con dos
homónimos cuyos ids son la forma D y la forma N del mismo GUID, el cualificador solo D emite el mismo texto para las dos
identidades. El candidato de G6 lo esquivaba rechazando en `SymbolId` las claves sin forma D, que es la opción A de
A2 §8 aplicada dentro del núcleo. La revisión de identidad del Arquitecto confirmó la tensión (`AGREED — A2 REQUIRED`,
modelo E1). El
[Amendment A2](../initiatives/I-49-proposal-v6-amendment-a2-exact-key-qualifier.md) conserva la identidad textual,
añade la validez neutral de la clave y el cualificador de clave exacta `#{…}` junto a la forma corta `#<d>`, y deja P2.5
y A1 sin cambio. El Coordinador está **AGREED WITH A2 / E1**, y el Arquitecto, tras su revisión exacta del blob
`49a925336dd3775929a35b0f40b73cb7f8c487f7`, **AGREED WITH A2**, sin hallazgos materiales. Ese cambio exigió ADR-0041 como sucesor de ADR-0040, entonces aceptado e
inmutable.

**Amendment A3-R2 (2026-09-15).** La cadena representativa de un fallo no basta para modelar varias causas,
raíces externas que entran por miembros distintos de un ciclo o cambios de razón durante una reparación. A3 fija
los diagnósticos locales coexistentes, `RootCauses` por SCC, la separación entre firmas y unidades de recuperación,
el ciclo simple y el descubrimiento que permite demostrar una sugerencia. Cambia las decisiones aceptadas señaladas
en el preámbulo; ADR-0043 es el sucesor completo propuesto. Solo los escenarios lineales **sin ciclo** enumerados
en A3 §8 conservan expresamente el resultado de V6: P8.10 caso C/T-V4-01, T-V5-01/02, T-V6-01/02/03, el escenario
de `RepairBrokenRack` de P21.6 y T-V4-05/06/11. No se generaliza esa compatibilidad a cualquier caso con una raíz;
el `CycleRoot` sin dueño es semántica nueva de A3.

## Decisión

### D1 — Núcleo neutral `RackCad.Application.Expressions`

- Existe un núcleo de expresiones en el namespace `RackCad.Application.Expressions` (DR-2).
- El núcleo **no depende** de `ProjectVariables`, `Persistence`, `Systems.*`, `Bom`, `Catalogs`,
  `StructuralSections`, Domain, UI, Plugin ni AutoCAD, y **ninguna** de sus entradas usa un tipo de `ProjectVariables`
  (DR-2, P5.1, P26.1). Una guarda de fuente lo fija (P28.4).
- La dependencia va **de Project Variables hacia el núcleo**, nunca al revés (P26.1).
- Su superficie reutilizable es parsear, enlazar con un contexto que aporta quien llama, comprobar, extraer
  dependencias, construir el grafo, detectar ciclos, ordenar, evaluar, formatear y diagnosticar (P26.2).
- `ExpressionContext` es un valor inmutable **por operación**, construido desde **un** snapshot y sin estado
  ambiental: ni cultura, ni reloj, ni aleatoriedad, ni entorno, ni cachés estáticas, ni AutoCAD, ni sistema de
  archivos (P6.1–P6.3).
- Sin ensamblado separado: la frontera la marcan el namespace y la guarda (P26.5; ALT-17, diferida).
- Queda preparado para ID20 e ID23 **sin implementarlos** (D24).

### D2 — Motor numérico adimensional y tres fronteras de validez

- Todo valor del motor es un **`double` finito**. **No existen** `ExpressionValueType`, `Scalar`, `DimensionMismatch`,
  `ResultTypeMismatch` ni reglas como `Length × Length` (P5.4), y el motor no rechaza ninguna operación por
  dimensiones ni por signo (P5.6).
- **No existe álgebra dimensional obligatoria.** `VariableType` sigue siendo exactamente `{ Length = 1 }`, pertenece al
  **adaptador** de Project Variables y **no** gobierna ningún álgebra dentro del motor (P5.5, P8.9).
- Semántica numérica IEEE-754 sin redondeos intermedios: `x ÷ 0`, incluido `0 ÷ 0`, es `DivisionByZero`; todo
  resultado intermedio o final no finito es `NonFiniteResult`; el mismo snapshot da resultados idénticos bit a bit
  (P14.4–P14.5).
- **Tres fronteras de validez** (DR-5, P5.7, P21.7):

  | Frontera | Qué valida | Dónde | Resultado si falla |
  |---|---|---|---|
  | Motor de expresiones | Todo nodo da un `double` finito; sin dominio físico | Núcleo | `DivisionByZero`, `NonFiniteResult` |
  | Adaptador de Project Variables | Contrato del `VariableType` sobre el **resultado raíz**, igual para literal y expresión | Preflight de las mutaciones de variables | `OutOfRange`, cuyo dueño es la variable |
  | Consumidor de propiedad | Dominio de la propiedad sobre el **valor efectivo** | Resolver único; ruta histórica de la ventana para literales | `OutOfRange`, cuyo dueño es `(RackId, PropertyId)` |

- **Contrato raíz de `Length`** (P8.10). Al escribir rige el `> 0` vigente, aplicado a cada símbolo del cierre `I`
  del estado «después» según su estado «antes»:

  | Caso | Antes | Después | Regla |
  |---|---|---|---|
  | A | Símbolo mutado X | `Success` | La raíz de X cumple `> 0`, sea literal o expresión |
  | B | `Success` que cumplía | `Success` | Tiene que seguir cumpliéndolo |
  | C | `Failed` o sin valor | `Success` | El valor nuevo tiene que cumplirlo |
  | D | Valor evaluable que ya incumplía por corrupción externa, con la definición intacta | `Success` | No bloquea por sí solo; R1 sigue impidiendo que un efectivo inválido llegue a una propiedad |

  Si fallan A, B o C: `OutOfRange` de la variable y plan vacío. Al **leer** rige el contrato histórico —un valor
  finito—, igual para las dos sintaxis. La validez no depende de la sintaxis (P8.10; ALT-21).
- El **dominio del consumidor** lo declara el descriptor **como dato**, nunca como callback, y se aplica al efectivo
  de las tres fuentes, con el comportamiento heredado de los literales y un **endurecimiento declarado**: una
  referencia directa persistida a una variable con valor negativo, alcanzable solo por edición externa, deja de
  resolver (P24.5).

### D3 — Unidades explícitas

- Se admiten **exactamente** `[mm]`, `[in]` y `[ft]`, y **solo** tras un literal numérico: `100[mm]`, `4[in]`,
  `20[ft]` (P1.6, P9.2). Otro token es `UnknownUnit`; un sufijo tras una referencia, una llamada o un paréntesis es
  `UnitNotAllowedHere`; notaciones como `100 mm`, `12"`, `10'6"` o `1 1/8` son `UnitSyntaxNotSupported`, y `1/2` es
  una división válida. Los corchetes quedan **exclusivamente** para unidades.
- Son **conversiones numéricas a pulgadas** al evaluar, con la operación fijada para ser deterministas bit a bit:
  `20[ft]` = `20 × 12`; `100[mm]` = `100 ÷ 25.4` (P9.3). Un número sin unidad es adimensional y un consumidor
  `Length` lo lee en pulgadas por contrato (P9.4). La pulgada sigue siendo la unidad interna y el motor **no** convierte
  el DWG: [ADR-0005](0005-estrategia-de-unidades.md) queda intacto (P9.1).
- **Una sola autoridad neutral de conversión** (ilustrativo: `RackCad.Application.Units.LengthUnits`) declara la tabla
  cerrada de unidades, sus tokens y **únicamente** las conversiones genéricas (P9.5):

  ```text
  MillimetersPerInch = 25.4
  InchesPerFoot      = 12
  ```

  No depende de nada; el núcleo la consume; **no** existe otro parser de unidades en UI, Domain ni Plugin.
- `StructuralSectionUnits` conserva su API y declara `InchesToMillimeters` e `InchesPerFoot` como **alias** de esa
  autoridad, con comportamiento idéntico bit a bit. La dirección es `StructuralSections → Units`, y el núcleo nunca
  referencia `StructuralSections` (P9.7).
- **No se centralizan constantes de dominio** aunque valgan 12: `SelectiveGeometryResolver.FootInches` (redondeo de la
  altura del Selectivo), `DynamicHeaderHeightCalculator.CommercialFoot` (pie comercial) y el `12.0` de
  `CantileverArmFrameResolver` (la notación de pendiente por 12 de ADR-0025 D4) se quedan **locales**. Una constante
  pertenece a la autoridad por su **papel** de conversión, no por su valor, y la guarda correspondiente se apoya en
  papeles declarados, nunca en buscar el número `12` (P9.6–P9.7, P30.23; ALT-31).
- El número se persiste **tal como se escribió**, con su unidad; la conversión ocurre al evaluar (P17.4). Los valores
  se muestran en pulgadas (P9.8).

### D4 — Modelos de expresión

- **Tres modelos inmutables y separados** (P2.1) —la **sintaxis** (ilustrativo: `SyntaxExpression`), el
  **`BoundExpression`** y el **resultado** de evaluación—, producidos desde el texto fuente por esta tubería (V6 §2.2):

  ```text
  texto fuente → sintaxis (con posiciones) → bind → BoundExpression → resultado de evaluación
  ```

- La **sintaxis** sirve solo para escribir y diagnosticar y **nunca se persiste**. El **`BoundExpression`** tiene los
  nodos `Number(double, unidad opcional)`, `Reference(SymbolId)`, `Negate`, `Binary(Add | Subtract | Multiply |
  Divide)` y `Call(FunctionId, args)`, sin posiciones, sin nombres, sin paréntesis redundantes, sin `+` unario y **sin
  anotaciones de tipo**. El **resultado** es el de D16 (P2.1).
- **Solo la forma canónica enlazada** —literal, referencia o `BoundExpression`— tiene autoridad semántica y se persiste
  (P4.1). El **texto tecleado** no es autoridad y **no se persiste**: sería una segunda fuente capaz de contradecir al
  árbol y quedaría obsoleta con cada rename (P4.4; ALT-5, ALT-6). El texto de edición y presentación lo produce **un
  solo formatter** desde la forma canónica y los nombres **actuales** (P4.1–P4.2).
- La comprobación semántica —existencia, aridad, unidades, ámbito, forma canónica y límites— se hace **contra un
  contexto**, no dentro del árbol, y un árbol leído de persistencia se re-comprueba en cada snapshot (P2.2, P7.5).
- **Canonicidad**: para todo `BoundExpression` canónico `b` sin referencias rotas y un mismo snapshot,
  `Canonicalize(Bind(Parse(Format(b)))) == b`, probado como propiedad (P2.5).
- El `=` es la marca de superficie que indica «esto es una expresión»; **no** pertenece a la gramática y **nunca** se
  persiste (P1.8).
- **Tampoco se persisten** el grafo de dependencias (D15), el valor evaluado (P17.5; ALT-7), las trazas ni los
  conjuntos derivados (P27.4, P30.10).

- **Comprobación estática antes de evaluar (A3 M1–M2).** `RegistryEvaluation` identifica los bloqueos propios
  `BrokenReference`, `InvalidArguments` y `NonCanonicalForm` contra el snapshot, sin evaluar numéricamente. También
  bloquean la pertenencia a una SCC cíclica y cualquier dependencia directa fallida (D15). La aridad usa
  `FunctionRegistry`; cada `InvalidArguments` lleva token de función y número de argumentos como datos estables.
  Coexiste con los demás bloqueos aplicables. Un bloqueo impide descubrir fallos numéricos latentes: no se evalúa
  especulativamente. El evaluador numérico de G6 conserva su contrato.

### D5 — Identidad de símbolos

- **`Name` nunca es identidad** (P3.5; ADR-0034 §2). El árbol persistido no contiene nombres.
- `SymbolId = (SymbolNamespace, Key)`. Los namespaces son un conjunto cerrado de tokens comparados en `Ordinal`, y
  cada namespace fija el comparador de su clave (P3.2). Para variables de proyecto:

  ```text
  SymbolId namespace = projectVariable
  key                = VariableId      (igualdad vigente de VariableId)
  ```

- **Clave textual y validez neutral** (Amendment A2, A2 §3.1–§3.2). La clave de `SymbolId(projectVariable, clave)` es
  el **texto** del `VariableId` de la entrada del registro acreditado, con comparador `OrdinalIgnoreCase`: la identidad
  y el comparador de la igualdad vigente **no** cambian, y P8.1, P3.6 y P6.4 tampoco. Para el núcleo neutral de
  expresiones, la clave es **válida** cuando:

  ```text
  clave no es null y no está vacía
  clave == clave.Trim()
  System.Guid.TryParse(clave, out _) == true
  ```

  - La regla es **neutral**: no nombra ningún tipo de `ProjectVariables` (P5.1, P26.1).
  - El conjunto de claves válidas coincide exactamente con el de los `VariableId.Value` posibles (A2 §2.1): ningún
    `VariableId` legible hoy queda fuera, y ningún texto que no pueda ser un `VariableId` entra.
  - `Guid.TryParse` **solo** decide la admisibilidad. El `Guid` devuelto **nunca** se usa para igualdad, hash, búsqueda,
    identidad ni para desambiguar en el formatter.
  - **No** hay igualdad por valor de `System.Guid`; **no** se normaliza la identidad a un valor GUID ni a ninguna
    disposición; **no** se colapsan las representaciones D, N, B, P y X ni las de compatibilidad; y **no** hay id alias.
  - **Compatibilidad hacia atrás**: **no** hay migración; **no** se endurece la regla del `VariableId` persistido ni se
    normaliza su texto; **ningún** dibujo se rechaza porque sus `VariableId` estén en N, B, P, X u otra disposición que
    la lectura vigente acepta; la igualdad de `VariableId` no cambia, y la identidad de la referencia directa
    `ProjectVariableReference(VariableId)` tampoco (A2 §3.1, A2 §3.2 y A2 §8).
  - El adaptador de G8 transporta `VariableId.Value → SymbolId.Key` **sin normalizar**, y una prueba de conformidad lo
    fija (A2 §3.2; A2 §9.3, B).

- La persistencia es **por id estable**. `Rename` es registry-only, no cambia ninguna definición ni fuente, no
  redibuja ningún rack y **no rompe** ninguna expresión, por construcción (P19.1–P19.6).
- Una expresión **no tiene identidad propia**: la de una definición es la de su variable, y la de una propiedad
  pertenece a `(RackId, PropertyId)`. Nada puede referenciar una propiedad (P3.3, P3.8, P13.6).
- Un `VariableId` duplicado da `AmbiguousIdentity` **antes** de construir cualquier tabla de símbolos (P3.6).
- **Única ruta nombre→id, solo al escribir y solo contra el snapshot que el usuario ve** (P7.3, P7.7): en RACKVARIABLES,
  contra el snapshot del workspace; en RACKEDITAR, contra el registro leído antes de abrir la ventana. Lectura,
  evaluación, rename, delete, propagación, resolver, re-lectura de commit y BOM **nunca** resuelven nombres; una
  lectura posterior solo valida ids y las observaciones del `PlanReadSet` (D19). Enlazar contra una lectura posterior
  dejaría que un nombre cruzara una frontera entre lecturas actuando como identidad (ALT-23).
- `Rack.*`, `Project.*` y el ámbito `Rack` quedan **reservados conceptualmente para ID20**, sin registrar, sin
  resolución y sin ningún camino productivo que los cree (P5.2–P5.3, P25.2–P25.3). Hoy existen un namespace activo,
  `projectVariable`, y un ámbito, `Project`.

### D6 — Sintaxis de referencias (OPEN A)

- **Formas** (§4.1, con el cualificador de A2 §3.3 y A2 §3.7):

  ```text
  Nombre                              nombre único y seguro sin llaves
  {Nombre complejo}                   nombre con cualquier otro carácter, o reservado
  Nombre#<d>                          homónimo con clave de forma D exacta, o identidad pegada o tecleada
  Nombre#{<clave exacta>}             homónimo con cualquier otra clave, o identidad pegada o tecleada
  {Nombre complejo}#<d>               homónimo con llaves y clave de forma D exacta
  {Nombre complejo}#{<clave exacta>}  homónimo con llaves y cualquier otra clave
  #<d>                                SOLO representación diagnóstica de una referencia rota con clave de forma D exacta
  #{<clave exacta>}                   SOLO representación diagnóstica de una referencia rota con cualquier otra clave
  ```

  `<d>` es una clave con forma D exacta, y `<clave exacta>`, el texto de una clave válida con cada `}` escrita `}}`
  (D7). El formatter emite con `Q(clave)` las formas de la tabla: `#<d>` para una clave con forma D exacta y
  `#{<clave exacta>}` para cualquier otra. En la entrada, también una clave con forma D se puede escribir entre llaves
  (regla 1), y las reglas que nombran `#{<clave exacta>}` valen para cualquier clave válida.

- **Reglas** (§4.2, P1.7, P3.9, P7.1–P7.2, leídas con A2 §3.3–§3.8):
  1. El cualificador lleva la **clave completa**, nunca un fragmento: `#` seguido de la forma D exacta (36 caracteres) o
     de la clave exacta entre llaves, `#{<clave exacta>}`, cuyo contenido desescapado es el texto de la clave (D7). Un
     fragmento, como `#3f2b1c9e` o `#{3f2b1c9e}`, **nunca** se parsea (`InvalidQualifier`) ni sirve como identidad
     (ALT-29). La sintaxis conserva el **texto de la clave** como su contenido semántico: si la implementación guarda o
     parsea además un `System.Guid` por comodidad, ese valor **no es autoritativo** y **nunca** se usa para resolver
     identidad. El binder resuelve **solo** con la clave textual recuperada y el comparador de clave del namespace, y
     devuelve el `SymbolId` de la entrada de la tabla, de modo que lo persistido conserva la grafía del registro
     (A2 §3.5). `#{<clave con forma D>}` es legal si el contenido es una clave válida: resuelve **exactamente** igual
     que la clave textual D y no crea una segunda identidad (A2 §3.8).
  2. Con cualificador, **el id es la autoridad** y el nombre solo se valida: un nombre distinto del actual da
     `QualifiedNameMismatch`.
  3. Las **llaves** delimitan todo nombre que no sea `palabra { " " palabra }`, y `}}` escapa `}`.
  4. Los nombres de función del registro, `Rack` y `Project` son **reservados sin llaves** (`ReservedName`), sin
     distinguir mayúsculas: se escriben `{MIN}` o `{Rack}`. `palabra.` es sintaxis de namespace para ID20
     (`UnknownNamespace`).
  5. Un **nombre exacto y único** enlaza al comprometer, con igualdad exacta `OrdinalIgnoreCase`, sin recortes ni
     normalización Unicode. Un fragmento parcial **nunca** enlaza (`UnknownSymbol`).
  6. Un **homónimo sin cualificador** da `AmbiguousName`, con cada candidato en su forma cualificada: la forma del
     nombre más `Q(clave)`, el mismo cualificador canónico del formatter (A2 §3.7).
  7. La lista y el selector insertan la forma inequívoca del formatter; la forma cualificada también se puede pegar o
     teclear, con cualquiera de las dos formas del cualificador.
  8. Un tramo contiguo sin llaves ni espacios que coincide con el nombre de una variable (`Holgura-Base`) da
     `OperatorInName`: nunca se reinterpreta como resta.
  9. Un `#<d>` o `#{<clave exacta>}` **roto se puede mostrar, pero no comprometer**: al enlazar da `BrokenReference`,
     así que nunca crea una referencia a un id inexistente. `#<d>` o `#{<clave exacta>}` **sin nombre nunca enlaza**, ni
     siquiera con el id presente (`NameRequired`), para que un texto que representa un estado roto no comprometa una
     identidad que el usuario no ve nombrada (ALT-28).
- **Formatter**: emite la forma **mínima inequívoca dentro del snapshot** —`Q(clave)` para un id ausente, `Nombre`,
  `{Nombre}` o la forma del nombre más `Q(clave)` para un homónimo—, con el cualificador canónico `Q` de A2 §3.6. Si un
  nombre único pasa a ser ambiguo tras un `Create` o un `Rename`, la siguiente presentación lo cualifica; **lo
  persistido no cambia** (§4.3, P4.2, P4.7).

  ```text
  si clave tiene forma D exacta:
      Q(clave) = "#" + minúsculasASCII(clave)
  si no:
      Q(clave) = "#{" + minúsculasASCII(clave) con cada "}" sustituida por "}}" + "}"
  ```

  - **Forma D exacta**: 36 caracteres, `-` en las posiciones 9, 14, 19 y 24 y dígitos hexadecimales ASCII en las demás,
    en cualquier combinación de mayúsculas. Una clave de compatibilidad con `0x` o `+` dentro de un grupo **no** la
    tiene.
  - **minúsculasASCII**: solo `A`–`Z` pasan a `a`–`z`; ningún otro carácter cambia. Bajar a minúsculas **no** cambia la
    identidad, porque el comparador es `OrdinalIgnoreCase`, y **no** modifica el texto persistido del `VariableId`.
  - **Sin canonicalización por `System.Guid`**: `Q` se calcula sobre el texto de la clave, nunca sobre un valor GUID.
  - `#{<clave con forma D>}` se muestra `#<d>`, porque la forma corta es la representación canónica mínima de una clave
    con forma D; del mismo modo, `Nombre#{D}` se muestra `Nombre#d` cuando hace falta cualificar (P4.3; A2 §3.8).
  - Ejemplos:

    | Clave | Cualificador |
    |---|---|
    | `3F2B1C9E-8A4D-4E6F-9B0A-1C2D3E4F5A6B` (D) | `#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b` |
    | `3F2B1C9E8A4D4E6F9B0A1C2D3E4F5A6B` (N) | `#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}` |
    | `{3F2B1C9E-8A4D-4E6F-9B0A-1C2D3E4F5A6B}` (B) | `#{{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}}` |
    | `(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)` (P) | `#{(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)}` |
    | `{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}` (X) | `#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}` |
    | `+03f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b` (D de compatibilidad) | `#{+03f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b}` |

    En las claves B y X cada `}` se escribe `}}`, y la última llave cierra el cualificador. La salida de la clave B,
    `#{{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}}`, **tiene que probarse** para demostrar que no colisiona con
    `#{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}`, que es la clave textual D escrita entre llaves (A2 §9.3, F).
  - **Una identidad, un texto** (A2 §4). Con la premisa de A2 §4 —todo carácter de una clave admisible es ASCII o
    espacio en blanco Unicode, que exige la prueba G de A2 §9.3—, dos claves admisibles son iguales bajo
    `OrdinalIgnoreCase` si y solo si coinciden en `Ordinal` tras `minúsculasASCII`. Una clave con forma D exacta y otra
    sin ella dan textos de dominios disjuntos —el segundo carácter del primero nunca es `{`—, y duplicar `}` es
    reversible. Por tanto, dos identidades distintas nunca comparten `Q(clave)`, y dos grafías de la misma identidad dan
    el mismo texto. El binder invierte al formatter: recupera `minúsculasASCII(clave)`, que sigue siendo admisible. Con
    ello, R1 y R2 tienen texto:

    ```text
    R1   A (N):  Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}
         B (D):  Holgura#0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d

    R2   A (D):  Holgura#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b
         B (N):  Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}
    ```

  - **P2.5, sin cambio** (A2 §5). Con `Q`, la sintaxis vuelve a representar cualquier identidad vigente de `VariableId`,
    y P2.5 (D4) sigue **igual e incondicional**, sin excepción de «formato razonable». El corpus **obligatorio** de su
    futura prueba de propiedad (P28) incluye D, N, B, P y X; las disposiciones de compatibilidad medidas; el mismo
    `System.Guid` en varias identidades textuales; homónimos entre esas identidades; referencias rotas; candidatos de
    `AmbiguousName`; y grafías en mayúsculas y en minúsculas. La corrección de G6 prueba además la matriz obligatoria
    A–M de A2 §9.3.

### D7 — Gramática y funciones

- **Lenguaje**: expresiones aritméticas sobre números con unidad opcional, referencias y llamadas a función. **Sin**
  sentencias, asignaciones, comparaciones, condicionales, booleanos ni cadenas (P1.1). Gramática (P1.2):

  ```ebnf
  expression     = additive ;
  additive       = multiplicative , { ( "+" | "-" ) , multiplicative } ;
  multiplicative = unary , { ( "*" | "/" ) , unary } ;
  unary          = ( "+" | "-" ) , unary | primary ;
  primary        = quantity | call | reference | "(" , expression , ")" ;
  quantity       = number , [ "[" , unit , "]" ] ;
  unit           = "mm" | "in" | "ft" ;
  call           = word , "(" , [ expression , { "," , expression } ] , ")" ;
  number         = digit , { digit } , [ "." , digit , { digit } ] ;
  reference      = [ name ] , qualifier | name ;
  name           = bare-name | braced-name ;
  bare-name      = word , { " " , word } ;                  (* palabras separadas por UN espacio *)
  word           = ( letter | "_" ) , { letter | digit | "_" } ;
  braced-name    = "{" , { char - "}" | "}}" } , "}" ;      (* "}}" escapa "}" *)
  qualifier      = "#" , ( guid-d | braced-key ) ;          (* A2 §3.3 *)
  guid-d         = guid ;                                   (* forma D completa: 8-4-4-4-12, 36 caracteres *)
  braced-key     = "{" , { char - "}" | "}}" } , "}" ;      (* "}}" es un "}" literal *)
  ```

  `letter` es un carácter de categoría letra Unicode; `digit`, un dígito ASCII `0`–`9`.
- **Reglas léxicas** (P1.7): los espacios no son significativos entre tokens —`100 [mm]` se acepta y su forma canónica
  es `100[mm]`—, salvo el espacio **simple** entre dos palabras de un `bare-name` y todo carácter dentro de llaves; una
  `word` seguida de `(` es una llamada; una `word` seguida de `.` es sintaxis de namespace (`UnknownNamespace`); un
  `bare-name` igual a un nombre de función, a `Rack` o a `Project` es `ReservedName`; una llave sin cerrar es
  `UnterminatedName`; y el cualificador sigue sus propias reglas léxicas, en la viñeta siguiente.
- **Reglas del cualificador** (Amendment A2, A2 §3.3–§3.4). `guid-d` es la regla vigente de forma D completa, sin
  cambio.
  - **`#` + forma D.** El comportamiento vigente del cualificador, sin cambio.
  - **`#{…}`.** Cualificador de clave exacta:
    - dentro de `braced-key`, todo carácter es literal salvo el escape `}}`;
    - comas, espacios, `<U+00A0>`, `<U+2028>`, paréntesis, `0x`, `+`, `{` anidadas y los demás caracteres que acepta
      `Guid.TryParse` son **datos**, no sintaxis de la expresión; cualquier otro carácter también se lee literal y deja
      un contenido inválido;
    - la llave de cierre `}` lo termina, y `}}` codifica un `}` literal. No hay ningún otro escape.
  - El contenido **desescapado** de `braced-key` no está vacío, es igual a su propio `Trim()`, satisface
    `System.Guid.TryParse` y se interpreta como el **texto de la clave** del `SymbolId` (D5).
  - **`#{` sin cerrar.** El diagnóstico vigente `UnterminatedName`.
  - **Contenido desescapado inválido** —vacío, con espacio en blanco inicial o final (el de `Trim()`), o rechazado por
    `Guid.TryParse`—: `InvalidQualifier`. Un fragmento, como `#{3f2b1c9e}`, sigue sin parsearse (ALT-29).
  - **Cualquier otra cosa tras `#`.** `InvalidQualifier`, como hoy.
  - **No** hay códigos de diagnóstico nuevos.
  - Los requisitos de recursos del Amendment A1 (A1 §5, recogidos en D8: lexing lineal, colección de tokens acotada,
    sin copias cuadráticas y sin recursión proporcional a la longitud) se aplican a un `braced-key` igual que a un
    nombre entre llaves, porque su contenido tampoco tiene un tope contractual de caracteres (A2 §6).
- `*` y `/` preceden a `+` y `-`, con asociatividad por la izquierda, y el unario liga más fuerte que cualquier binario
  (P1.3). Los números usan **forma invariante** con `.` como único separador decimal, sin exponente, agrupadores,
  punto inicial o final, signo interno, `NaN` ni `Infinity` (P1.4). La `,` es **solo** separador de argumentos: `1,5`
  es `AmbiguousDecimalComma`, de modo que `MAX(Holgura, 1,5)` nunca se lee como `MAX(Holgura, 1, 5)` (P1.5; ALT-18).
- **Funciones productivas iniciales** (P10.3), en un `FunctionRegistry` cerrado e inmutable, declarado en código, sin
  registro en tiempo de ejecución, reflexión, inyección, plugins, funciones de usuario ni scripting (P10.1):

  | Función | Aridad | Semántica |
  |---|---|---|
  | `MIN(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Menor valor |
  | `MAX(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Mayor valor |
  | `ABS(x)` | 1 | Valor absoluto |

  El nombre no distingue mayúsculas a la entrada y el token canónico va en mayúsculas (P10.5).
- **Diferidas**: `ROUND`, `CEILING` y `FLOOR`. Su semántica sigue abierta —punto medio, precisión o múltiplo,
  interacción con unidades y negativos— y su consumidor natural es ID23. Solo pueden añadirse como ampliación
  **cerrada** del registro, con revisión y con ADR si cambian lo que se persiste (P10.6; ALT-12).
- **Excluidas** salvo nueva razón aprobada: `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings, trigonometría y
  macros (P10.7).
- El parser se escribe a mano, determinista e independiente de la cultura; están prohibidos los terceros —`NCalc`,
  `DataTable.Compute`, `System.Linq.Expressions`, Roslyn— y cualquier dependencia NuGet (P1.10;
  [ADR-0012](0012-producto-sin-dependencias-nuget.md); ALT-14). Un **solo** lexer reconoce unidades, nombres y
  cualificadores: el del núcleo.
- **Excepción acotada a [ADR-0015](0015-entrada-numerica-localizada.md)** (P1.11). La gramática invariante rige
  **solo dentro de una expresión**. El texto sin `=` sigue siendo un literal parseado **exactamente como hoy** en cada
  superficie —regla localizada de ADR-0015 en RACKVARIABLES e invariante en el editor vinculable—, sin unidades. La
  fórmula no hereda el parser localizado de literales, e I-49 no hace converger los dos parsers de literales (P30.18).

### D8 — Límites normativos del árbol

- Límites (P1.9, con la guarda de recurso sustituida por el Amendment A1, A1 §3.2):

  | Límite | Valor | Naturaleza | Al escribir | Al leer lo persistido |
  |---|---|---|---|---|
  | Nodos por árbol | ≤ 256 | **Normativo** | `LimitExceeded` | Estructural (D14) |
  | Profundidad del `BoundExpression` (`MaxBoundExpressionDepth`) | ≤ 24 | **Normativo** | `LimitExceeded` | Estructural (D14) |
  | Argumentos por llamada | ≤ 16 | **Normativo** | `LimitExceeded` | Estructural (D14) |
  | Tokens sintácticos | Valor de implementación: inicial **4096**, nunca menor que 6 × nodos normativos (1536; A1 §4) | Guarda del parser | `LimitExceeded` (`SyntacticTokenCount`) | No aplica: lo persistido no tiene texto |
  | Anidamiento sintáctico (paréntesis, unarios y llamadas) | Valor de implementación: inicial 64, nunca menor que 24 | Guarda del parser | `LimitExceeded` | No aplica |

- **Definición normativa de profundidad**:

  ```text
  depth(expression) = número máximo de nodos en cualquier camino root→leaf del BoundExpression,
                      contando la raíz y la hoja
  ```

  `1` y `A` tienen profundidad 1; `-1`, `1 + 2` y `ABS(A)`, 2; `MIN(A, B + C)`, 3. **La asociatividad cuenta**: una
  suma sin paréntesis de *n* operandos tiene profundidad *n*, así que `1 + … + 1` con 25 operandos **excede** el
  límite aunque el texto no tenga paréntesis.
- La profundidad se valida tras el bind y antes de comprometer, en las dos superficies; otra vez sobre la forma
  enlazada que llega al preflight; y al leer un árbol persistido. Un árbol persistido que excede cualquier límite
  normativo **falla cerrado** como error estructural (`MalformedExpression` con submotivo de límite). Superar un límite
  **nunca** trunca.
- Las guardas de tokens y de anidamiento sintáctico solo protegen al parser y **no** sustituyen a la profundidad
  normativa (ALT-32).
- `MaxBoundExpressionDepth` decide qué payloads persistidos son legibles: **cambiarlo exige revisión, la prueba de G8 y
  ADR**. Con profundidad 24 y llamadas en todos los niveles, el peor caso ocupa 51 niveles JSON en el registro y 50 en
  el diseño, dentro del máximo de 64 de `System.Text.Json`; la prueba del peor caso de G8 recorre por descubrimiento los
  puntos reales que serializan, deserializan, clonan o parsean esos documentos.
- **Guarda de complejidad sintáctica** (Amendment A1, A1 §3.2). Sustituye a la guarda de caracteres de ADR-0038:
  - **A1.1 — Sin límite de validez por caracteres.** No existe ningún límite contractual basado en el número total de
    caracteres del texto. `TextLength` deja de ser una condición del contrato y no se emite (A1 §6).
  - **A1.2 — Guarda de complejidad sintáctica.** `MaxSyntacticTokens` acota la cantidad de tokens que genera el lexer.
    - Es un valor de implementación, inicial 4096.
    - Nunca puede bajar de `6 × máximo normativo de nodos`, que con nodos ≤ 256 vale 1536 (A1 §4).
    - Superarla da `LimitExceeded` con tipo `SyntacticTokenCount`.
  - **A1.3 — Qué cuenta como un token.** Cada lexema que delimita el lexer del núcleo, válido o inválido, cuenta **uno**:
    - un número, con todos sus dígitos;
    - un sufijo de unidad `[…]`;
    - un nombre desnudo completo, `word {" " word}`, **con todas sus palabras**;
    - un nombre entre llaves completo, con sus escapes `}}`;
    - un cualificador completo, `#<d>` o `#{<clave exacta>}` con sus escapes `}}` (A2 §6);
    - cada operador `+ - * /`, cada paréntesis, cada coma y el `.` de la sintaxis de namespace;
    - cada lexema mal formado que el lexer diagnostica.

    No cuentan los espacios insignificantes ni la marca interna de fin de texto. **Un token puede contener un nombre
    arbitrariamente largo.** Esta granularidad es normativa: la cota de A1 §4 y el mínimo de A1.2 se calculan con ella.
  - **A1.4 — Cuándo corre.** Mientras se lexea, antes de que el parser construya nada:
    - el lexer se detiene en cuanto delimitaría el token `MaxSyntacticTokens + 1`, y el parser no corre;
    - el resultado tiene **un solo** diagnóstico: `LimitExceeded`, con tipo `SyntacticTokenCount`, máximo
      `MaxSyntacticTokens` y posición desde el inicio de ese token hasta el final del texto;
    - no se informa ningún otro diagnóstico, igual que con la guarda de texto de G5;
    - superar la guarda **nunca** trunca (P1.9).
  - **A1.5 — Separación intacta.** Igual que el anidamiento, la guarda de tokens solo protege al parser. No sustituye a
    los límites normativos del `BoundExpression`, que se siguen validando tras el bind (P1.9, P7.5): ALT-32 sigue
    rechazada.
- **Cota del formatter** (A1 §4).
  - `b` es un `BoundExpression` legal: `n ≤ 256` nodos, profundidad ≤ 24 y ≤ 16 argumentos por llamada.
  - `Format(b)` es el formatter único de P4.2, con paréntesis mínimos y la forma mínima inequívoca de cada referencia.
  - Los tokens se cuentan según A1.3.

  Cada token se carga a exactamente un nodo:

  | Carga del nodo | Máximo | Qué tokens |
  |---|---|---|
  | Cabeza de `Number` | 3 | El signo, si el formatter escribiera un valor negativo; el número; el sufijo de unidad |
  | Cabeza de `Reference` | 2 | El nombre, desnudo o entre llaves; el cualificador |
  | Cabeza de `Negate` | 1 | `-` |
  | Cabeza de `Binary` | 1 | El operador |
  | Cabeza de `Call` | 3 | El nombre de la función, `(` y `)` |
  | Separador | 1 | La `,` que precede al nodo cuando es un argumento que no es el primero |
  | Paréntesis de precedencia | 2 | Como mucho un par alrededor del nodo, porque P4.2 exige paréntesis mínimos |

  Las cargas cubren todo el texto:
  - nombres, números, unidades y cualificadores van a su hoja;
  - cada operador, a su nodo;
  - el nombre de una función y sus paréntesis, a su llamada;
  - cada coma, al argumento que la sigue;
  - cada par de paréntesis de precedencia, a la subexpresión que envuelve.

  El formatter no emite ningún otro token: los espacios no son tokens (P1.7), `=` no pertenece a la gramática (P1.8) y
  ningún namespace está registrado (P5.2).

  ```text
  tokens(Format(b)) ≤ Σ nodos (cabeza ≤ 3 + separador ≤ 1 + paréntesis ≤ 2) = 6n
  la raíz no lleva separador ni paréntesis:   tokens(Format(b)) ≤ 6n - 3
  con n ≤ 256:                                tokens(Format(b)) ≤ 1533 < 1536 = 6 × 256 < 4096 = MaxSyntacticTokens
  ```

  - **La longitud en caracteres no participa.** Ni el nombre, ni sus escapes `}}`, ni la clave del cualificador, en
    cualquiera de sus dos formas, ni la representación decimal: cada uno es un único token (A1.3; A2 §6).
  - **Solo interviene el número de nodos.** La profundidad y los argumentos no entran en la cota de tokens. Siguen
    acotando el anidamiento: el texto canónico anida como mucho tanto como la profundidad, ≤ 24, y
    `MaxSyntacticNesting` nunca es menor que 24 (P1.9, sin cambio).
  - **El valor inicial no depende de detalles de tokenización.** A1.3 fija la granularidad y el mínimo de A1.2 se
    calcula con ella. Aun si se contaran por separado `[`, unidad y `]`, y `#` y GUID, el mismo reparto daría
    `8n - 3 = 2045 < 4096`.
  - **Lo esencial de A1.3.** Un nombre (con todas sus palabras o entre llaves) y un número cuentan **un** token cada
    uno: contar palabras o caracteres reabriría la tensión.
  - **4096 no se eligió por intuición.** Supera el mínimo de 1536, y la cota de 1533, con un margen de 2.67 veces. Si un
    ADR cambiara el máximo normativo de nodos, el mínimo de A1.2 se recalcula; los límites normativos no se tocan para
    cuadrar este número.
- **P2.5, sin cambio** (A1 §7). P2.5 permanece literalmente fuerte:

  > Para todo `BoundExpression` canónico `b` **sin referencias rotas** y un mismo snapshot:
  > `Canonicalize(Bind(Parse(Format(b)))) == b`.

  A1 no añade «si cabe en la guarda», «para nombres razonables» ni «salvo textos muy largos». Lo que cambia es que las
  guardas del parser ya **no pueden** rechazar el texto canónico de un árbol legal:

  - no hay guarda de caracteres (A1.1);
  - `Format(b)` produce como mucho 1533 tokens, menos que `MaxSyntacticTokens` (A1 §4);
  - su anidamiento sintáctico es ≤ 24, que no supera `MaxSyntacticNesting` porque este nunca es menor que 24 (P1.9).

  Tampoco se introduce un bypass del formatter ni un parser alternativo: sigue habiendo un solo formatter (P4.2) y un
  solo lexer y parser del núcleo (P1.10). La propiedad se prueba en G6 reanudado. P28.3 la situaba en G5, y el ajuste de
  calendario G5/G6 del Coordinador, registrado en el commit `c4880af`, la trasladó a G6. Las reproducciones A–D de
  A1 §2.2 son casos obligatorios, con B probado con nombres de 4001 y de 1000001 caracteres.
- **Nombres, sin cambio** (A1 §8):
  - **No** se añade `MaxNameLength` ni ningún otro límite de longitud de nombre.
  - Sigue la regla histórica: el nombre no puede estar en blanco.
  - **No** cambian `Create`, `Rename` ni el store.
  - **No** hay migración.
  - **No** se rechaza ningún dibujo por la longitud de un nombre.
  - `#<GUID>` sin nombre nunca enlaza, ni siquiera con el id presente (`NameRequired`; D6, regla 9), y OPEN A no se
    reabre (A1 §9).
- **Recursos y seguridad** (A1 §5). Quitar la guarda de caracteres **no** deja al parser sin defensa. G6 reanudado
  implementa y prueba estos requisitos:

  1. **Lexing en `O(caracteres de la entrada)`**, sin recursión.
  2. **Parsing en `O(tokens)`**, con `tokens ≤ MaxSyntacticTokens`.
  3. **Descenso recursivo acotado por `MaxSyntacticNesting`**, comprobado antes de descender (P1.9, sin cambio).
  4. **Colección de tokens acotada por `MaxSyntacticTokens`**: nunca se materializan más de `MaxSyntacticTokens + 1`
     tokens. Los diagnósticos léxicos quedan acotados por la misma guarda, porque cada lexema aporta un número constante
     de diagnósticos.
  5. **Ninguna copia cuadrática exigida por el contrato**: las posiciones son desplazamientos, cada nombre se copia un
     número constante de veces (coste `O(longitud)`) y no se re-lexean prefijos.
  6. **Ninguna recursión proporcional a la longitud de un nombre**: los nombres se recorren de forma iterativa; en lexer
     y parser, la única recursión es el descenso acotado por `MaxSyntacticNesting`.
  7. **Sin dependencia de `ExpressionContext` para parsear**: `MaxSyntacticTokens` es un valor del núcleo, como hoy
     `MaxSyntacticNesting` (`ExpressionParser.cs:31,38` en `c4880af`), no un dato del snapshot. Los `ExpressionLimits`
     de `ExpressionContext` (P6.1) no cambian, y el parser sigue sirviendo a ID20 e ID23 sin contexto (P25, P26.2).

  Una entrada con un único nombre enorme cuesta tiempo lineal en la entrada, pero **un** token, anidamiento 0 y un nodo
  sintáctico. La memoria sigue siendo lineal en el tamaño de la entrada. Aclaración de este ADR: ese coste lineal es
  inevitable, porque la cadena ya fue suministrada.

  **Consecuencia para G6, fuera de la guarda.** Sin la guarda de caracteres, un nombre también puede ser muy largo al
  enlazar: las comprobaciones de G6 sobre el texto de los nombres (P7.1, incluido `OperatorInName`) no pueden
  reintroducir un coste cuadrático en la longitud de los nombres.

  Los límites físicos del entorno de ejecución (memoria disponible, tamaño máximo de una cadena .NET) son fallos de
  ejecución, no reglas del contrato: A1 no los convierte en condición de validez ni en excepción de P2.5.
- **Diagnósticos del parser** (A1 §6):
  - **Código.** Sigue siendo `LimitExceeded`, de la clase «Sintaxis y límites» (P15.3), con severidad `Error` y posición.
    El orden determinista de P15.7 no cambia.
  - **Tipos de límite del parser.**

    | Tipo | Con A1 |
    |---|---|
    | `TextLength` | Deja de ser una condición normativa o productiva y **no se emite** |
    | `SyntacticNesting` | Permanece, sin cambio |
    | `SyntacticTokenCount` | **Nuevo**, con máximo `MaxSyntacticTokens` |

  - **`TextLength` sale del contrato productivo.** A1 §6 decide eliminarlo en la rama de I-49 cuando G6 se reanude, y al
    retirarlo G6 no reutiliza su valor numérico para otro tipo de límite. No es una migración de persistencia —resumen
    de la evidencia de A1 §6—: ningún diagnóstico se persiste —lo persistido son formas canónicas (P17), y el núcleo
    devuelve códigos y datos (P15.4)— y G5 no está integrado.
  - **Límites tras el bind.** Los tipos de `LimitExceeded` para nodos, profundidad y argumentos no cambian de contrato y
    los introduce G6.

### D9 — Funciones, unidades y tokens en mundo cerrado

- Cada kind, nodo, operador, unidad, función y namespace tiene su **token persistido explícito y estable**, declarado en
  una tabla **cerrada**, con sus dos sentidos fijados por prueba (P2.4, P17.10):

  | Token | Valores | Comparación al leer |
  |---|---|---|
  | `Kind` de definición | `literal`, `expression` | `OrdinalIgnoreCase`, como `literal` hoy |
  | `Kind` de binding | `projectVariable`, `expression` | `Ordinal`, como `projectVariable` hoy |
  | `Node` | `number`, `ref`, `neg`, `add`, `sub`, `mul`, `div`, `call` | `Ordinal` |
  | `Namespace` | `projectVariable` | `Ordinal` |
  | `Unit` | `mm`, `in`, `ft` | `Ordinal` |
  | `Function` | `MIN`, `MAX`, `ABS` | `Ordinal` |

- **Nunca** se deriva semántica persistida de `enum.ToString()`, `Type.ToString()`, `nameof` ni el nombre de una clase
  C#: renombrar un miembro no puede cambiar lo que se escribe ni lo que se lee (P2.4). El `JsonStringEnumConverter` de
  los stores **no** puede producir ningún token de Expression (P17.10).
- En la rama `Add` del registro que I-49 reescribe, el `Type` se escribe y se lee desde una tabla explícita del
  **mismo** mapping único de I-48, con salida byte a byte `"Length"`: es preservación de la invariante de un solo
  mapping, no un arreglo lateral (P17.10).
- **Los payloads `expression` son de mundo cerrado** (P17.6.7): un campo desconocido, o repetido con la comparación de
  nombres vigente, dentro de la definición `expression`, de la entrada `expression` de `PropertyValues` o de cualquier
  nodo del árbol, a cualquier profundidad, es un **fallo estructural** (D14). También lo es un `Node`, `Namespace`,
  `Function` o `Unit` desconocido, o una `Unit` en un nodo que no es `number` (P17.6).
- La política histórica de `ExtensionData` **no cambia fuera** de los payloads de Expression: la raíz del registro, la
  entrada de variable, la definición `literal`, el binding `projectVariable`, el portador Selectivo y el sobre conservan
  su tolerancia y su preservación de campos desconocidos (P17.12; ALT-26).
- **Regla de evolución** (§6.3): la semántica futura **de Expression** entra **solo** por tokens o discriminadores
  explícitos que la build lectora conozca, o por un cambio de **major** que obligue a las builds anteriores a rechazar
  el documento; **nunca** por campos que una build anterior pudiera ignorar. La regla vincula a las builds futuras en
  todo el documento, no solo dentro del payload, donde además la impone el mundo cerrado (P17.6.7). Todo lo nuevo dentro
  del payload de Expression —nodos, unidades, funciones, namespaces o campos— es fail-closed para esta build, con
  cualquier versión, y cada adición exige revisión y, si cambia la persistencia, ADR (P2.4, P10.8, P18.8). Una función
  nueva amplía las palabras reservadas **solo para escrituras futuras** (P10.9).

### D10 — Persistencia de las variables de proyecto

- `VariableDefinition` gana un segundo caso (P2.6):

  ```text
  VariableDefinition = Literal(double)
                     | Expression(BoundExpression)
  ```

  `VariableType` sigue siendo solo `Length` (§2.3). El acceso al literal lanza fuera de `Literal`, y el de la expresión,
  fuera de `Expression`.
- Forma persistida (P17.2, P17.4): el literal se persiste **exactamente como hoy**, y `expression` va **sin** `Value`.

  ```json
  {"Kind":"literal","Value":6.0}
  {"Kind":"expression","Expression":{"Node":"add",
    "Left":{"Node":"ref","Namespace":"projectVariable","Id":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
    "Right":{"Node":"number","Value":100,"Unit":"mm"}}}
  ```

- `VariableId` sigue siendo la **identidad estable**; cambiar el tipo de definición no cambia el id (P3.1).
- El almacenamiento físico no cambia: el registro sigue en el NOD bajo `RACKCAD_PROJECT`, con el Plugin como único
  lector y escritor (P17.1; [ADR-0006](0006-autocad-solo-en-plugin.md)).
- **Un registro literal heredado sigue legible sin migración**: leer nunca escribe, no se reescribe nada al abrir y no
  hay conversión automática de literales en expresiones ni de expresiones en literales (P17.8, P29.1–P29.2). La primera
  expresión no cambia ninguna versión (D13).

### D11 — Persistencia híbrida de las propiedades vinculables

- La fuente de una propiedad vinculable tiene **tres casos** (P2.7, P23.3, P24.1):

  ```text
  Literal
  ProjectVariableReference(VariableId)
  Expression(BoundExpression)
  ```

- Forma persistida en `PropertyValues` (P17.3): un literal **no** tiene entrada; `projectVariable` sigue **sin cambio**,
  `{"Kind":"projectVariable","VariableId":"<guid>"}`; `expression` es `{"Kind":"expression","Expression":<nodo>}`,
  **sin** `VariableId`. Mientras exista una entrada, el literal authored queda **congelado e inactivo**, igual que con
  una referencia (ADR-0034 §5; regla 20.13 de I-48).
- **La referencia directa histórica no desaparece**: su forma persistida, su inspección y su comprobación de tipo no
  cambian (P24.2, P8.7).
- **Formas canónicas: una sola representación por significado**, aplicadas al comprometer en las dos superficies (P2.8,
  P24.3):

  | Árbol enlazado | Fuente de propiedad | Definición de variable |
  |---|---|---|
  | Solo `Reference(projectVariable X)` | **`ProjectVariableReference(X)`** | `Expression`: una definición no tiene caso referencia |
  | Solo un número sin unidad (`Number` o `Negate(Number)`) | `Literal(±v)` | `Literal(±v)` |
  | Cualquier otro árbol: operación, función o **unidad explícita** | `Expression` | `Expression` |

  Así `=X` **no tiene dos formas persistidas**: `=Holgura` y seleccionar `Holgura` en la lista persisten lo mismo, y
  `=6` persiste lo mismo que `6`; `=4[in]` es `Expression` porque lleva una conversión explícita (ALT-4). Un literal
  canónico obedece la regla de literales de su superficie: canonicalizar no es una vía para eludirla.
- Una forma **no canónica** leída de persistencia —una definición `expression` que solo es un número sin unidad, o una
  fuente `expression` que solo es una referencia o un número sin unidad— es el error **semántico** `NonCanonicalForm`:
  no se evalúa, no vuelve ilegible el registro, se corrige con `ChangeDefinition` o con la reparación del rack y
  **nunca** se normaliza en silencio. Ningún escritor la produce (P2.8; ALT-25).
- La interpretación de una fuente de rack tiene **una sola autoridad**: `LinkedPropertyInspection.InspectBinding`
  (P17.11).
- La duplicación y el restamp conservan `PropertyValues`, entradas `expression` incluidas, con los mismos `VariableId`
  (P24.8). La exportación a biblioteca materializa el efectivo evaluado exacto, quita `PropertyValues` y escribe
  literal-only en la línea `1.0`; un rack afectado no se exporta (P24.9). La autoridad multi-vista compara también
  `PropertyValues`: dos vistas hermanas con expresiones distintas son `Divergent` (P24.10).

- **Lectura de A3.** `NonCanonicalForm` impide la evaluación numérica. Una expresión persistida con aridad errónea
  se comprueba estáticamente, con los mismos datos de token y número de argumentos de D15; `InspectBinding` aplica
  la precedencia de D18. G8 conserva la obligación de cubrir ambos casos por su camino real de persistencia.

### D12 — Extensión de ADR-0034

**Este ADR extiende [ADR-0034](0034-project-variables-autoridad-drawing-level.md); no lo reemplaza.** ADR-0034 está
aceptado, es inmutable y **no se edita** (V6 §9).

| ADR-0034 | Extensión de I-49 | V6 |
|---|---|---|
| §3: definición **literal** | `literal` **o** `expression`; `VariableType` sigue siendo solo `Length` | P2.6, P5.5 |
| §4: `Literal(T)` o `ProjectVariableReference(VariableId)` | Tercer caso `Expression(BoundExpression)` en la fuente de una propiedad | P2.7, P17.3, P24 |
| §15: `Definition` para ID22B; `PropertyValue<T>` para ID21 | ID22B extiende **también** la fuente de propiedad; ID21 sigue fuera | DR-1, §9 |
| §7: profundidad 1 | Cierre transitivo; un plan, una transacción, un commit y un `Regen`; `PlanReadSet` | P21, D19, D23 |
| §11: `Delete` bloqueado con consumidores; reparación de bindings rotos | `Delete` y `UnlinkAllAndDelete` también bloqueados por fórmulas y dependientes; `RepairBrokenRack` repara también fallos semánticos | P20, §7, D17, D18 |
| §13: schema, y `kind` desconocido = error (decisiones de I-47, C4-8 y C4-9) | Kinds `expression` fail-closed en builds anteriores; **V-0**; payload de Expression cerrado | §6, D9, D13 |

- ADR-0034 §15 situaba ID22B en `ProjectVariable.Definition` e ID21 en `PropertyValue<T>`, como tipos distintos y
  deliberadamente separados. Por **requisito del Owner** (DR-1), I-49 extiende **también el lado de la propiedad** que
  §15 asociaba a ID21: habilita `Expression` como **fuente de una propiedad vinculable**, editada con el
  **mismo** `LinkedPropertyEditor` de I-48. Limitar las expresiones a la definición contradecía ese requisito de dos
  superficies (ALT-1), y modelar cada expresión de propiedad como una variable implícita llenaría el registro de
  variables que nadie creó, con rename y delete dependiendo de propiedades (ALT-3).
- La referencia directa se **conserva** y `=X` es su forma canónica (D11).
- Esta extensión **no introduce**: referencias rack→rack (**ID21**); `PropertyValue<T>` como referencia entre racks;
  la extensión a otros sistemas de rack (Dinámico, Push Back, Cama, Cantilever) ni sistemas nuevos; nuevas propiedades
  vinculables; ni comandos o ventanas nuevos (P30.1, P30.15, P22.1, P23.14).
- **Un futuro caso de ID21** no lo define V6, e ID21 sigue fuera (DR-1; contrato de I-49 §4, que lo sitúa con
  ADR-0034 §15 en `PropertyValue<T>`). Lo que V6 fija es que hoy ninguna expresión puede referenciar una propiedad
  (P13.6), que `RackPropertyReference` y `rackProperty` siguen prohibidos (P30.1) y que la evolución de una guarda que
  debilitara la protección de ID21 es condición de parada (P28.4; V6 §13). La forma de ese caso futuro queda para
  ID21.
- **No se toca**: la autoridad y el nodo del registro, la identidad `VariableId`, authored ≠ effective, el congelado
  del literal comprometido (regla 20.13), la autoridad multi-vista, la identidad ambigua fail-closed, el mapping único
  del `Type`, la reparación atómica por rack, la regla R1 y la decisión C4-13 de I-47, las constantes de dominio de los
  sistemas de rack y los censos de comandos y ventanas (§2.3).
- El contrato de I-49 prevé esta vía en su §11.2 y su §12 («salvo ADR nuevo aceptado por el Owner»); su texto se alinea
  en G4 (V6 §1.3, §9).

### D13 — Schema V-0

- **No cambia** la versión de `ProjectVariablesDocument` (sigue `1.0`, major soportado 1) **ni** la del diseño
  Selectivo (`1.0` / `2.0` *sticky*, major de lectura 2) (§6.1, P18).
- **Razón**: las builds I-47/I-48 ya fallan cerradas ante un `Kind` desconocido y no interpretan en silencio la
  semántica nueva (P18.2):

  | Dato nuevo | Comportamiento de una build anterior |
  |---|---|
  | Registro con una definición `expression` | Registro **entero** `PresentButUnreadable`; RACKEDITAR de todo Selectivo, RACKVARIABLES y RACKBOMTOTAL bloqueados |
  | Rack con una fuente `expression` | `FatalMalformedReference(UnknownKind)`; el resolver da `UnknownReferenceKind`; la sonda da `Indeterminate` y el descubrimiento aborta |

  Un major no protegería nada; un minor por contenido (V-1) no cambia el comportamiento de ningún lector y solo añade
  una etiqueta *sticky*; un major (V-2) dejaría el dibujo fuera de las builds anteriores para siempre, aunque ya no
  quedara ninguna expresión (P18.3–P18.5; ALT-13). V-0 **no** se justifica por reducir archivos tocados ni por el
  precedente de I-50 (P18.7).
- **Condiciones obligatorias** (§6.2):

  | # | Condición | Verificación |
  |---|---|---|
  | C1 | Payload de Expression con mundo cerrado (D9) | T-V3-15 |
  | C2 | Semántica nueva solo por tokens o discriminadores cerrados, o por un major (regla de evolución de D9) | Revisión de cada adición (P18.8) |
  | C3 | Los escritores de I-49 **nunca** fijan ni degradan una versión: la calculan con la autoridad vigente | Prueba: un diseño `2.3` editado sigue en `2.3` (P23.15) |
  | C4 | Caracterización del comportamiento heredado **antes** de modificar los stores | G8, paso 1: commit solo de pruebas con su CI (T-V3-21) |

- **C3 preserva C4-9 en la rama que I-49 reescribe**: el reconciliador calcula la versión con
  `SelectiveDesignSchema.ResolveWriteVersion` o la autoridad vigente equivalente y **nunca** fija `"2.0"` (P23.15).
- Quitar todas las expresiones devuelve el dibujo a lo que una build anterior puede usar (P29.3). Una build anterior
  muestra «declara una definición de clase desconocida» o una referencia de kind desconocido, no «versión más nueva»:
  es un coste de comunicación, no de integridad, y se documenta al usuario (P18.9, P29.4).

### D14 — Fallo estructural frente a fallo semántico (OPEN B)

**Principio** (DR-7, §5). Un fallo **estructural** es un dato cuyo significado la build no entiende: no se lee y no se
repara de forma destructiva. Un fallo **semántico** es una fuente que la build entiende pero de la que no obtiene un
efectivo válido: se diagnostica con un resultado tipado y se corrige, **sin fallback** a literal, a cero ni a un valor
anterior (DR-6).

- **Estructural** (§5.1, P17.6, P15.9): `Kind`, `Node`, `Namespace`, token de función o de unidad desconocidos
  persistidos; id o AST mal formado —id ilegible, campo obligatorio ausente, número no finito—; límites normativos
  superados (D8); campo desconocido o repetido dentro de un payload de Expression; dos autoridades en la misma entrada.
  - En el registro: **registro entero** `PresentButUnreadable`, como hoy con un kind desconocido.
  - En el rack: `FatalMalformedReference` y rack `Blocked`, **sin reparación destructiva**. La sonda da `Indeterminate`
    y las mutaciones que necesitan descubrimiento abortan: un dato que la build no entiende nunca se lee como «no
    consume» (P21.4).
- **Semántico** (§5.1, P17.7, P15.3): `BrokenReference`, `Cycle`, `DependencyFailed`, `InvalidArguments`,
  `DivisionByZero`, `NonFiniteResult`, `NonCanonicalForm` y el `OutOfRange` del consumidor. Dan un resultado **por
  símbolo o por fuente**, y el registro sigue estructuralmente `Usable`; un error semántico no invalida el contexto
  (P6.8).
- **Decisiones** (§5.2):
  1. **Alcance del bloqueo: solo el cierre afectado** —los símbolos en error, sus dependientes transitivos y todo rack
     con al menos una fuente no sana—.
  2. **Un rack no afectado funciona con normalidad**; sus opciones no ofrecen variables en error (P23.11).
  3. **Un rack afectado** no tiene efectivo; RACKEDITAR no lo abre y remite a RACKVARIABLES con la causa; su geometría
     existente no se toca; y solo entra en un plan que lo deje resuelto: su reparación o la corrección de la causa
     superior (P24.12).
  4. **RACKBOMTOTAL** aborta si un rack incluido en ese BOM está afectado, como hoy con una referencia rota; una
     variable en error sin consumidor cotizado no bloquea por sí sola, y una fuente no sana nunca produce una cantidad
     (P24.7).
  5. **RACKVARIABLES** abre siempre que el registro sea estructuralmente legible y esté acreditado; con un error
     estructural sigue bloqueado como hoy, sin escribir (D22).
  6. **Intents correctivos**, gobernados por R1–R3 (P21.3):
     - **R1** — todo rack que consuma el cierre tiene que resolver después; si no, plan vacío. Es la regla de
       I-47/I-48, **sin cambios**.
     - **R2** — el símbolo mutado evalúa bien; un símbolo sano no puede fallar después; los que ya fallaban, con su
       definición intacta, no bloquean.
     - **R3** — ningún intent escribe un error nuevo.

     `Rename` siempre está permitido con el registro legible y acreditado (P19.7). Con R1, R2 y la reparación siempre
     hay salida, también con dos errores dentro de un ciclo.
  7. **Ciclos persistidos**: todos los miembros dan `Cycle` con la lista completa ordenada. La recuperación
     garantizada corrigiendo un solo miembro se limita a una SCC que sea un **ciclo dirigido simple** (D15): cada
     miembro tiene una arista interna entrante y una saliente; una autorreferencia sola cuenta. La corrección debe
     superar R1–R3 y P13.3. En una SCC no simple no se promete esa recuperación ni se declara imposible: un miembro
     común a todos sus ciclos podría bastar. Borrar un miembro sigue bloqueado (P13.5).
  8. **Fórmulas de propiedad rotas**: corregir la variable superior, que las recupera sin quitarlas **solo cuando R1 lo
     permite**, o reparar el rack y volver a escribir la fórmula en RACKEDITAR; el diagnóstico nunca indica la primera
     vía si no es demostrable en el estado diagnosticado (D18). Las estructurales no se reparan, para no destruir datos
     que esta build no entiende.
- El catálogo de diagnósticos es **cerrado** (P15.3): sintaxis y límites, enlace, semánticos y contrato de frontera. En
  V6 todo diagnóstico es `Error`, y cualquier `Error` implica sin valor, sin resultado parcial y sin fallback (P15.2,
  P15.5). El núcleo devuelve códigos y datos; los mensajes en español se producen en **una** capa de texto (P15.4).

### D15 — Dependencias, grafo y ciclos

- **Las dependencias se derivan exclusivamente del `BoundExpression`** (P11): `Dependencies(BoundExpression)` devuelve
  las referencias **directas**, sin repetición y en orden determinista —namespace `Ordinal`, después la clave con el
  comparador de su namespace—. Opera sobre el árbol persistido, **sin evaluar y sin nombres**, y funciona igual sobre un
  árbol legible que falla al evaluar. La **misma** función sirve a las dos superficies. Un literal tiene cero
  dependencias y una referencia directa de rack depende de su único `VariableId`.
- **El grafo** (P12):
  - es **derivado**, **en memoria** y construido desde la tabla de símbolos de **un** snapshot; **nunca** es autoridad
    persistida, porque un grafo persistido podría divergir del registro (P12.1; ALT-8);
  - sus nodos son los símbolos; sus aristas van de la dueña a cada **dependencia**, con índice inverso de
    **dependientes** (P12.2);
  - las relaciones propiedad→variable, directas o por expresión, **no** entran en el grafo de variables: salen del
    barrido y se unen al componer el plan (P12.3);
  - una arista hacia un id ausente se registra como `BrokenReference`, sin crear nodo (P12.4);
  - expone dependencias y dependientes directos y transitivos, el **conjunto afectado** y el **conjunto leído**, todos
    derivados en memoria y ninguno persistido (P12.7).
- **Ciclos** (P13): detección **iterativa y determinista** de componentes fuertemente conexas, con cada ciclo
  informado con sus miembros en orden. Un estado «después» con un ciclo que pase por la variable cambiada hace fallar el
  preflight con `Cycle` y **plan vacío**. Los miembros de un ciclo y todos sus dependientes transitivos no son
  evaluables, y **nunca** hay fallback. Un ciclo leído de persistencia es semántico (D14). Las expresiones de propiedad
  no forman ciclos, porque nada puede referenciar una propiedad.
- **Orden de evaluación** (P14.1–P14.3): topológico sobre la parte acíclica, con el desempate de P11.1 e
  **independiente** del orden de las entradas del registro. Las variables se evalúan de forma ansiosa, **una vez por
  snapshot** y con memoización; si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id de la
  causa directa que falla fuera de la SCC del dueño, sin evaluarse; todos los bloqueos locales aplicables coexisten.

#### D15.1 — M1 — Elegibilidad de evaluación

Antes de evaluar, `RegistryEvaluation` aplica a cada definición la **comprobación semántica estática** de V6 (P2.2,
l. 714-716; P7.5, l. 914-917; D4, l. 319-320): sobre el `BoundExpression`, contra el contexto del snapshot y **sin
evaluación numérica**. `ExpressionEvaluator` evalúa la definición de un símbolo **solo** si no tiene **ninguno** de estos
bloqueos:

1. es miembro de una SCC cíclica (P13.1);
2. su propia definición referencia algún id ausente (P12.4);
3. su propia definición tiene alguna llamada con aridad errónea (`InvalidArguments` estático; P7.5, P10.4);
4. su definición es `NonCanonicalForm` (P2.8, l. 750-754; D11, l. 825-828: «no se evalúa»);
5. alguna de sus dependencias directas falla.

- Con cualquier bloqueo, `RegistryEvaluation` lo marca `Failed` **sin evaluación numérica** (P14.3, P13.4).
- El evaluador de G6 nunca recibe un símbolo presente sin valor: su contrato no cambia. Un literal sigue evaluándose a
  su propio valor (P14.6).
- `NonCanonicalForm` de una definición es una `Expression` que solo es un número sin unidad (`Number` o
  `Negate(Number)`). Ningún escritor la produce (P2.8), así que en G7 solo aparece con una tabla sintética; G8 la prueba
  por su camino real de persistencia. Al no tener referencias, nunca coexiste con los bloqueos 1, 2, 3 o 5.
- **Fallos numéricos latentes (N-5).** `DivisionByZero` y `NonFiniteResult` solo existen si el evaluador corre. Si un
  símbolo tiene cualquier bloqueo, los fallos numéricos que aparecerían al quitarlo **no** se descubren en ese snapshot:

  ```text
  A = #<a> + 1 ; D = A + 1 / 0   → D = [DF{A; [A]; (A, BrokenReference, [a])}]; el DivisionByZero de D es latente
  ```

  `RegistryEvaluation` no evalúa especulativamente alrededor de valores que faltan. Una sugerencia de recuperación solo
  se apoya en los diagnósticos conocidos del snapshot, conforme a «ninguna otra fuente inválida conocida» de §7.2.6
  (l. 2792-2797).

#### D15.2 — M2 — Diagnósticos de un símbolo no evaluado

Recibe **todos** los aplicables:

- **A.** un `BrokenReference` por cada id ausente distinto de su propia expresión, con ese id en `RelatedSymbols`;
- **B.** un `Cycle` si pertenece a una SCC cíclica, con la lista completa de miembros en el orden de P11.1 (P13.2);
- **C.** un `DependencyFailed` por cada dependencia directa que falla y está **fuera** de su propia SCC (M4);
- **D.** un `InvalidArguments` estático por cada par distinto (token de la función, número de argumentos) de las llamadas
  de su propia definición con aridad errónea, con esos dos datos estables;
- **E.** `NonCanonicalForm`, cuando aplica.

Ninguno suprime a otro. Los diagnósticos son **locales**: un símbolo solo recibe los que salen de su propia definición y
de sus propias aristas; `RootCauses` (M6) no añade diagnósticos.

Un símbolo evaluado:

- da `Success`; o
- da `Failed` con **un** diagnóstico numérico del evaluador de G6 (`DivisionByZero` o `NonFiniteResult`): el primer fallo
  termina la evaluación. Por M1, el evaluador nunca llega a un `InvalidArguments` en `RegistryEvaluation`.

La precedencia de `InspectBinding` sobre una fuente de rack (P24.6; D18) **no cambia**: M2 trata de símbolos.

#### D15.3 — M3 — Orden de los diagnósticos

1. **Orden primario: P15.7**, posición, código y dueño. Los diagnósticos de evaluación no tienen posición (P15.1) y los
   de un símbolo comparten dueño, así que decide el código en el orden del catálogo cerrado (P15.3), el que ya fijaron G5
   y G6:

   ```text
   BrokenReference 22 < Cycle 23 < DependencyFailed 24 < InvalidArguments 25
                      < DivisionByZero 26 < NonFiniteResult 27 < NonCanonicalForm 28
   ```

2. **Desempate nuevo**, con el mismo dueño y el mismo código:
   - `BrokenReference`: el id ausente, en el orden de P11.1;
   - `DependencyFailed`: el id de la causa directa, en el orden de P11.1;
   - `InvalidArguments`: el token de la función en orden `Ordinal` y, después, el número de argumentos;
   - `Cycle`: un símbolo tiene a lo sumo uno, y su lista de miembros ya va en el orden de P11.1;
   - `DivisionByZero`, `NonFiniteResult` y `NonCanonicalForm`: a lo sumo uno por símbolo.
3. **Nunca** por texto, mensaje, cultura, hash ni orden de las entradas.

Para un símbolo no evaluado, el orden es `BrokenReference` (ids ascendentes) → `Cycle` → `DependencyFailed` (causas
ascendentes) → `InvalidArguments` (token y número ascendentes). Ejemplo:

```text
A = ABS(B, #<m>) + E ; B = A ; E = 1 / 0
A = [BrokenReference(m), Cycle[A, B], DF{E; [E]; (E, DivisionByZero)}, InvalidArguments(ABS, 2)]
```

El «primer diagnóstico» de un símbolo, en M4, es el primero en este orden.

#### D15.4 — M4 — Estructura de `DependencyFailed`

Cada `DependencyFailed` pertenece a **una** dependencia directa `d` que falla, fuera de la SCC de su dueño, y tiene
esta estructura estable:

```text
Causa  = d                                   // el «id de la causa» de P14.3; también en RelatedSymbols
Cadena = [d] + cola
         cola = Cadena del primer diagnóstico de d, si ese diagnóstico es DependencyFailed
         cola = []                           en otro caso
Raíz   = firma (M5) del primer diagnóstico que no es DependencyFailed al que se llega,
         es decir, del primer diagnóstico del último eslabón de la cadena
```

- La cadena es **una** cadena representativa, basada en nodos y aristas: **no** es por SCC. Cumple P15.1 y el dato
  «el id de cada eslabón» de P21.6. **No** es el conjunto de causas raíz (M6).
- Es finita y sin repeticiones: cada causa pertenece a una SCC anterior en el orden de la condensación.
- El primer diagnóstico de un miembro de ciclo nunca es `DependencyFailed`: `Cycle` (23), y en su caso
  `BrokenReference` (22), van antes. Si la cadena llega a un miembro, la raíz es su `CycleRoot` cuando su primer
  diagnóstico es `Cycle`, o la firma agregada de su `BrokenReference` cuando tiene una referencia rota propia.
- En un símbolo no miembro con `DependencyFailed` e `InvalidArguments` propio, el primer diagnóstico es
  `DependencyFailed` (24 < 25): la cadena sigue por él, y la raíz propia de `InvalidArguments` está en `RootCauses`
  (U6(a)).
- La cola se **comparte** con la cadena del diagnóstico de `d`: construir un `DependencyFailed` cuesta O(1), y ninguna
  cadena se copia ni se enumera por caminos.

Ejemplo, con `A = B + 1`, `B = C + 1` y `C = 1 / 0`:

```text
C = [DivisionByZero]
B = [DF{C; [C]; (C, DivisionByZero)}]
A = [DF{B; [B, C]; (C, DivisionByZero)}]
```

#### D15.5 — M5 — Firma de causa raíz

| Raíz | Firma | Notas |
|---|---|---|
| Numérica | `(dueño, DivisionByZero)` o `(dueño, NonFiniteResult)` | Solo el código (P21.6) |
| Aridad | `(dueño, InvalidArguments, [pares (token, número de argumentos)])` | **Agregada por dueño**: la lista ordenada de sus pares distintos (M3); una sola raíz por dueño |
| Forma no canónica | `(dueño, NonCanonicalForm)` | Solo el código (P21.6) |
| Referencia rota | `(dueño, BrokenReference, [ids ausentes en el orden de P11.1])` | **Agregada por dueño**: nunca una raíz distinta por cada id ausente |
| Ciclo | `CycleRoot[miembros de la SCC en el orden de P11.1]` | **Una** unidad: ningún miembro es un dueño privilegiado, y entrar por A o por B da la misma raíz |

- Un símbolo cuyo fallo es solo `DependencyFailed` no es raíz.
- Una firma nunca contiene texto localizado, salida del formatter, posiciones ni la traza opt-in.

#### D15.6 — M6 — `RootCauses`, por SCC

`RootCauses` es una operación **pura y derivada** del núcleo de grafo y evaluación de G7, sobre una
`RegistryEvaluation`. **La unidad de propagación es la SCC.** Definición autoritativa:

```text
RootCauses(s)        = RootCausesScc(SCC(s))                       si s da Failed
RootCauses(s)        = { }                                         si s da Success

RootCausesScc(C)     = RaícesPropias(m)             para cada miembro m de C
                     ∪ { CycleRoot(C) }             si C es cíclica
                     ∪ RootCauses(d)                para cada arista m → d con m ∈ C, d ∉ C y d que falla
```

`RaícesPropias(m)` puede contener:

- la firma agregada de `BrokenReference`, si m tiene alguno;
- la firma agregada de `InvalidArguments`, si m tiene alguno;
- la firma de `NonCanonicalForm`, si aplica;
- la firma numérica, si m se evaluó y falló.

Propiedades:

- en una SCC acíclica (un solo símbolo) se reduce a la regla por símbolo: sus raíces propias y las de sus dependencias
  que fallan;
- en una SCC cíclica, **todos** los miembros tienen el **mismo** `RootCauses`: el `CycleRoot`, las raíces propias de
  cada miembro y las raíces de cada dependencia externa que falla de cualquier miembro;
- vacío si s da `Success`, y no vacío si s da `Failed`;
- **deduplicado** por firma;
- **determinista** y ordenado estructuralmente (A3 §5.7);
- sin enumeración de caminos;
- nunca texto localizado, salida del formatter, posiciones ni la traza opt-in;
- **no se persiste** (P27.4).

`RootCauses` y los diagnósticos son distintos: un miembro sin arista hacia E no recibe `DF{E}`, pero su `RootCauses`
contiene la raíz de E si E entra en su SCC por otro miembro (U4, U5). Es la **única** autoridad del conjunto de causas
raíz. La consumen el contrato futuro de `SymbolResultObservation`, de `Upstream` y de §7.2 (M10), y el panel de
diagnóstico (P22.9). Nadie reimplementa el recorrido.

#### D15.7 — Orden de `RootCauses`

1. El código de la raíz, en el orden del catálogo: `BrokenReference` 22, `Cycle` 23, `InvalidArguments` 25,
   `DivisionByZero` 26, `NonFiniteResult` 27 y `NonCanonicalForm` 28.
2. El id relevante, en el orden de P11.1: el dueño, o el **miembro mínimo** para un `CycleRoot`.
3. Los datos estables: los ids ausentes, la lista completa de miembros, o la lista de pares (token, número de
   argumentos).

Dentro de una `RegistryEvaluation`, los criterios 1 y 2 ya identifican cada raíz: un dueño tiene a lo sumo una raíz de
cada código y las SCC son disjuntas. El criterio 3 completa el orden total. Nunca se ordena por hash. Ejemplo:
`A = 1 / 0 ; B = #<b> + 1 ; D = A + B` da `RootCauses(D) = [(B, BrokenReference, [b]), (A, DivisionByZero)]`, aunque
A < B.

#### D15.8 — M7 — Varias causas raíz

| Situación | Diagnósticos | `RootCauses` |
|---|---|---|
| Raíces detrás de dependencias directas distintas (U1) | Un `DependencyFailed` por dependencia directa que falla | Todas |
| Varias raíces detrás de una sola dependencia (U2) | Un `DependencyFailed`, con su cadena representativa | Todas: ninguna se descarta |
| Una raíz alcanzada por varios caminos (diamante) | Un `DependencyFailed` por dependencia directa que falla | La raíz, una vez |
| Un dependiente que lee varios miembros de un ciclo (U4) | Un `DependencyFailed` por miembro leído | El `CycleRoot` una sola vez, junto con las demás raíces de la SCC |
| Raíces externas que entran en una SCC por miembros distintos (U5) | Solo en el miembro que lee cada una | Todas, en todos los miembros y en sus dependientes |
| Fallo propio y fallo superior en el mismo símbolo (U3, U6) | Todos los aplicables (M2) | Las raíces propias y las superiores |

Nunca hay un diagnóstico por camino.

#### D15.9 — M8 — Coste · M9 — Determinismo

**M8.**

- Construcción del grafo, SCC iterativa, comprobación estática y diagnósticos de todos los símbolos: `O(V + E)` más el
  tamaño de las definiciones (acotado por P1.9) y el coste determinista de ordenar en P11.1.
- Orden topológico con el desempate de P11.1: `O(V log V + E)`.
- A lo sumo un `DependencyFailed` por arista del grafo y un `Cycle` por nodo. Los `BrokenReference` y los
  `InvalidArguments` están acotados por el tamaño de cada definición (P1.9).
- **Listas de miembros compartidas.** Todos los `Cycle` de una SCC y su `CycleRoot` referencian **una** lista inmutable
  de miembros por SCC; copiarla por miembro costaría `O(Σ|C|²)`.
- Cadenas con colas compartidas: `O(1)` por `DependencyFailed`.
- **Coste de `RootCauses` (N-4).** Una derivación sin memoria de `RootCauses(s)` es un recorrido **iterativo** de la
  condensación con visitados, sin recursión en profundidad, que visita cada SCC a lo sumo una vez por llamada:
  `O(V + E)`, más ordenar su resultado. Leída como recursión literal y sin visitados, la definición de M6 sería
  exponencial en diamantes apilados; esa lectura queda excluida. Pedirla sin memoria para muchos o todos los símbolos
  que fallan —el panel de P22.9 o la condición de §7.2— puede costar `O(V · (V + E))`.
- **Memoización por snapshot.** `RegistryEvaluation` **puede** guardar los `RootCauses` ya derivados, por SCC. Esa
  memoria pertenece a la `RegistryEvaluation` inmutable de un snapshot; no cambia ningún resultado, no es global, no se
  persiste y es determinista. Cada `RegistryEvaluation` nueva empieza sin memoria. No es observable semánticamente.
- Ningún camino raíz-nodo se materializa y ningún conjunto de raíces se persiste.

**M9.** Sin paralelismo, cultura ni reloj (P14.5). Cada elección sale de P11.1, de P15.7, del orden `Ordinal` de los
tokens y del orden del catálogo. El resultado no depende del orden de las entradas del registro, del hash, de la
cultura, del hilo ni de la memoización.

#### D15.10 — Ciclos

- La detección sigue basada en SCC (P13.1); un nodo con arista a sí mismo es una SCC cíclica de un miembro.
- Firma de raíz: `CycleRoot[miembros en el orden de P11.1]`.
- Todo miembro recibe `Cycle` con la misma lista ordenada.
- Un miembro puede tener además `BrokenReference` e `InvalidArguments` propios de su definición, y `DependencyFailed`
  por dependencias que fallan fuera de su SCC. **Nunca** tiene `DependencyFailed` por una arista dentro de su SCC,
  tampoco por una autorreferencia.
- `RootCauses` es el mismo para todos los miembros y reúne las raíces de toda la SCC (M6).
- P13.4 («no son evaluables (`Cycle` o `DependencyFailed`)», l. 1182-1183), enmendada por A3 (A3 §10.1), queda así con
  A3: un miembro tiene siempre `Cycle`; un dependiente transitivo que no es miembro tiene al menos un `DependencyFailed`;
  ninguno se evalúa y ninguno tiene fallback.
- P13.5 (l. 1186-1187) se cumple sin cambio: todos los miembros dan `Failed` con `Cycle` y sus dependientes dan
  `DependencyFailed`.

#### D15.11 — Ciclo simple y ruptura de un ciclo

Una SCC cíclica C es un **ciclo simple** si, considerando solo sus aristas internas, cada miembro tiene exactamente una
arista interna saliente y exactamente una entrante. Para una SCC fuertemente conexa equivale a:

```text
número de aristas internas de C == número de miembros de C
```

- Una autorreferencia (un miembro, una arista a sí mismo) es un ciclo simple.
- Se decide sobre las aristas del grafo, nunca sobre la forma textual de las definiciones.
- En un ciclo simple sin otras raíces, cualquier `ChangeDefinition` válido de un miembro, que por R2 tiene que evaluar
  bien, rompe el único ciclo y deja evaluables a todos los miembros.
- En una SCC **no simple**, un `ChangeDefinition` de un miembro puede no romper todos los ciclos internos (U7).
- Las autorreferencias cuentan como aristas internas: en `A = A + B ; B = A` hay tres aristas internas (A→A, A→B,
  B→A) y dos miembros, así que la SCC no es simple; corregir B deja la autorreferencia de A.

La recuperación garantizada con un solo miembro se limita al ciclo simple, como fija D14 decisión 7. En una SCC
no simple pueden hacer falta varias correcciones, cada una gobernada por R1–R3 y P13.3; no se promete ninguna.

#### D15.12 — Orden topológico (DI-1)

Conceptualmente: se elige repetidamente, entre los nodos que no son miembros de un ciclo y cuyas dependencias que tampoco
lo son ya se emitieron, el menor `SymbolId` en el orden de P11.1.

- Se admite cualquier algoritmo equivalente.
- Las aristas hacia miembros de un ciclo y hacia ids ausentes no bloquean.
- Los miembros de un ciclo quedan **fuera** del orden de evaluación de P16.2.
- Los dependientes de un ciclo **aparecen** en ese orden, porque la regla conceptual los emite, y fallan sin evaluación
  numérica (M1).
- El orden no depende de la enumeración de las entradas.

#### D15.13 — Orden de la lista de ciclos (DI-2)

Ascendente por el miembro mínimo de cada ciclo en el orden de P11.1. Las SCC son disjuntas, así que el orden es total
(P12.5).

#### D15.14 — Conjuntos transitivos (DI-3)

- Transitivo = alcanzable por un camino de longitud ≥ 1.
- Un miembro de ciclo puede pertenecer a sus propios conjuntos transitivos de dependencias y dependientes a través de un
  camino no vacío del ciclo.
- Los ids ausentes no son nodos del grafo.
- Coincide con P21.2 (`I = {X} ∪ dependientes transitivos de X`, l. 1530) y con P20.7 (un miembro de ciclo tiene
  dependientes).

#### D15.15 — Conjuntos derivados de G7 (DI-4)

G7 expone:

- `Dependencies(s)` directas;
- `Dependents(s)` directos;
- `Dependencies(s)` transitivas;
- `Dependents(s)` transitivos;
- `RootCauses(s)` (M6);
- para cada ciclo de la lista, si es un ciclo simple (A3 §6.2).

`RootCauses` y el predicado de ciclo simple son las únicas adiciones de A3. **No** entran en G7: el cierre afectado de
racks, el descubrimiento de consumidores, la implementación del `PlanReadSet`, la ejecución de la reparación, la
sugerencia de recuperación, `SourceRoots`, `RecoveryUnits`, las razones de bloqueo ni el `MutationPlan`. El conjunto
afectado y el conjunto leído de P12.7 siguen en P21.2 y P21.6, en G9.

- `RecoveryUnits` (M10(c)) se deriva de `RootCauses` y del dueño o la SCC de cada firma, datos que G7 ya expone; no
  necesita nada más del grafo. Por eso pertenece a la capa de recuperación de Application (G9 y G10) y G7 no lo prueba.
- Las pruebas de G7 de A3 §11.1 se limitan al núcleo de grafo: diagnósticos, orden, cadenas, firmas, `RootCauses`,
  conjuntos y el predicado de ciclo simple.

#### D15.16 — Frontera con el evaluador y aridad estable

`RegistryEvaluation` genera `BrokenReference` desde las dependencias autoritativas, ordenadas por P11.1, y no llama
al evaluador para un símbolo con ids ausentes o aridad errónea. `ExpressionEvaluator` de G6 no se modifica: llamado
directamente conserva su orden de primera aparición de ids ausentes y comprueba aridad al llegar a cada llamada.
Puede dar otro resultado que la evaluación del registro; para sus símbolos manda `RegistryEvaluation`.
El recorrido interno del evaluador no se convierte en otra autoridad del grafo (P11.4–P11.5).

El `InvalidArguments` estático lleva desde G7 un par (token, número de argumentos) por combinación distinta, con
la autoridad de `FunctionRegistry`; se deduplica por par y su raíz se agrega por dueño. `1 / 0 + ABS(1, 2)` da solo
`InvalidArguments(ABS, 2)`: el fallo numérico queda latente. La clasificación de esa fuente en G8 es `Intrinsic`,
salvo precedencia estructural, id ausente o tipo incompatible. El diagnóstico sin datos de una llamada directa al
evaluador de G6 no llega a `RegistryEvaluation` (A3 §7.1–§7.2).

### D16 — `RegistryEvaluation`, única autoridad de los valores

- **Una `RegistryEvaluation` por snapshot acreditado** es la **única** autoridad de los valores de los símbolos (DR-3,
  P14.9). Es inmutable y contiene los resultados por símbolo, el grafo directo e inverso, los ciclos y el orden (P16.2).
- El contexto se construye **solo** desde el registro acreditado: `Absent` da tabla vacía; `Readable` con ids únicos da
  tabla; cualquier otro resultado **no** produce contexto (P6.4).
- Targets acreditados, opciones del editor, workspace de RACKVARIABLES, preflight y RACKBOMTOTAL **transportan** sus
  valores y resultados. **Ninguno** se convierte en un evaluador alterno (P14.9, P16.4).
- **El efectivo de una propiedad lo produce el resolver único del rack** a partir de `RegistryEvaluation`: una
  referencia directa lee el valor de su variable, y una expresión se evalúa **una vez por rack** durante su resolución,
  sobre los valores del mismo snapshot. Un fallo de cualquier propiedad deja **sin efectivo** a todo el rack (P14.8,
  P24.5, P6.7).
- La evaluación ocurre **dentro de Application**; **nunca** en el Plugin, la UI, el ejecutor ni el comando de BOM
  (P14.7). La sesión del editor evalúa un borrador **solo para presentarlo** y avisar pronto; ese valor nunca se escribe
  (P14.9).
- Resultado por símbolo: `Success | Failed`, con un `Value` finito que solo existe con `Success`, diagnósticos si y
  solo si `Failed`, y una traza **opt-in**, acotada, en memoria y desactivable sin cambiar ningún resultado (P16.1,
  P16.3). **Sin campo de tipo** (DR-5).

### D17 — `Delete` y `UnlinkAllAndDelete`

- **Consumidores de X**: toda propiedad cuya fuente sea `ProjectVariableReference(X)` **o** una `Expression` cuyas
  dependencias incluyan X (P20.1). Los dependientes salen del snapshot acreditado, sin barrer el dibujo; los
  consumidores, del barrido (P20.3).
- **`Delete(X)` queda bloqueado** si X tiene **consumidores** o **dependientes** variable→variable. Se informan ambas
  listas; sin cascada y sin materializar nada (P20.2; ALT-16).
- **`UnlinkAllAndDelete(X)`** (§7.1, P20.4):

  ```text
  Referencia directa a X                        → se materializa como hoy
  Expresión de propiedad que depende de X       → BLOQUEADO
  Definición de variable que depende de X       → BLOQUEADO
  ```

  - Una referencia directa se materializa como hoy: el valor evaluado exacto de X pasa a literal y se quita el binding.
  - Una expresión de propiedad o una definición de variable que dependen de X bloquean la operación **entera**.
  - El bloqueo muestra los dependientes (variable y definición formateada) y los consumidores que bloquean (rack,
    propiedad y fórmula canónica). La salida es editar cada fórmula en RACKEDITAR y cada definición en RACKVARIABLES.
  - Todo rack materializado tiene que resolver después (R1).
- **Nunca** se convierte en silencio una fórmula completa en literal como efecto colateral de borrar X, ni se sustituye
  el valor de X dentro de una fórmula (§7.1, P30.21; ALT-20). Una fórmula no sigue a X: sigue a su fórmula, y
  materializarla cortaría dependencias que nadie pidió tocar.
- Borrar una variable definida por expresión, sin consumidores ni dependientes, está permitido y es registry-only
  (P20.5). Con errores semánticos rigen las mismas reglas: se puede borrar una variable en error sin consumidores ni
  dependientes; no se puede borrar un miembro de un ciclo; y un borrado **nunca** fabrica una `BrokenReference`, porque
  consumidores y dependientes lo bloquean antes (P20.7).
- Su commit sigue D19: **ninguna** eliminación espera un valor «después» del símbolo eliminado (P20.8).

### D18 — `RepairBrokenRack`

- **Una única operación**, con el mismo intent `RepairBroken(rackId, confirmed)`: rack-scoped, atómica, explícita y
  confirmada (ADR-0034 §11). **Ningún comando ni operación nuevos** (§7.2, P30.20; ALT-27).
- **Repara fallos semánticos entendidos** (P24.6): toda fuente `RepairableMissingTarget` o `RepairableSemanticFailure`,
  por causa propia (`Intrinsic`), superior (`Upstream`) o de dominio (`Domain`). `InspectBinding` es la única autoridad
  sobre una fuente de rack, con esta precedencia: lo estructural, después el id ausente, después el tipo incompatible y
  por último el fallo semántico (propio, superior y dominio). Nunca da `Healthy` para un fallo de evaluación ni para un
  efectivo fuera de dominio. Un fallo semántico de una fuente es reparable: un estado no reparable para esos fallos
  dejaría el rack sin corrección posible (ALT-19).
- **Efecto**: sobre un clon, quita esas fuentes de `PropertyValues` y deja gobernar el literal congelado **sin
  tocarlo**; el rack produce **una** `RackMutation`, y si aun así no resolviera, el plan queda vacío (§7.2).
- **No repara de forma destructiva lo que la build no entiende**: lo estructural (`FatalMalformedReference`,
  `FatalUnknownProperty`) y `FatalIncompatibleTarget` dejan el rack `Blocked` **entero**, aunque tenga además fuentes
  reparables (§7.2, P24.6).
- **Confirmación consciente de la fuente**, **por fuente**, en el panel de RACKVARIABLES antes de confirmar y en el
  mensaje de confirmación del preflight; la confirmación sigue siendo del **conjunto completo** del rack (§7.2):

  | Campo | Contenido |
  |---|---|
  | `Rack` | Nombre e id del rack |
  | `PropertyId` | Propiedad afectada |
  | `SourceKind` | `projectVariable` o `expression` |
  | `CanonicalExpression` | Fórmula canónica del formatter en el snapshot, con `#<id>` para referencias rotas |
  | `FailureCause` | Código y dueño: id ausente, causa propia o dominio; para causa superior, cada variable leída que falla con su `RootCauses` completo, en el orden de P11.1 |
  | `FrozenLiteral` | Literal congelado que pasará a gobernar |
  | `UpstreamCause` | Si aplica: cada variable leída que falla, en el orden de P11.1, con su `RootCauses` completo en el orden de D15 |
  | Mensaje de recuperación | Condicional, con la regla siguiente |

- **Mensaje de recuperación condicional** (§7.2). El diagnóstico nunca indica sin condiciones «corregir X recuperaría
  esta fórmula». R1 **no** se relaja, así que corregir una variable solo es posible si, después, todos los racks que
  consumen su cierre resuelven.
  1. **Clasificación.** Cada fuente fallida de un rack se clasifica por su causa: **superior corregible**, con su
     conjunto de causas raíz (variables cuya propia definición falla, o un ciclo tomado como una unidad), o **no
     corregible desde arriba**: id ausente, fallo propio, dominio inválido u otra causa que mantendría R1 bloqueado.
  2. **Condición para mostrar la sugerencia de recuperación.** Para una fuente con causa superior y causa raíz única X,
     el diagnóstico puede mostrar la sugerencia **solo** si, en el estado diagnosticado, todas las fuentes fallidas de
     todos los racks que consumen el cierre de X tienen causa superior con causa raíz exactamente X. Solo entonces
     ninguna otra fuente inválida conocida impide la recuperación, y un `ChangeDefinition` de X que supere la validación
     normal del estado resultante deja esos racks resueltos sin reparar.
  3. **Si se cumple**, el mensaje es:

     ```text
     Corregir <X> permitiría recuperar esta fuente sin eliminarla.
     ```

     Si X es un ciclo **simple**, `<X>` nombra a sus miembros: una corrección válida de cualquiera de ellos rompe
     ese único ciclo. Para una SCC no simple no se ofrece la sugerencia (D15).
  4. **Si no se cumple**, no hay sugerencia. Se informan todas las razones estructuradas aplicables, en el orden
     `OtherInvalidSources`, `SeveralRecoveryUnits`, `NonSimpleCycle`, `DiscoveryIndeterminate`, con la definición,
     los datos y los textos de D18.2–D18.3. Solo se muestra el aviso de fórmulas si el rack es `Repairable` y la
     reparación no abortaría; un rack `Blocked` no tiene acción ni aviso de reparación.
  5. **Consecuencia declarada.** Con R1 intacto, un rack con dos fuentes que dependen de causas raíz independientes no
     se recupera corrigiendo una sola variable, porque cada corrección deja la otra fuente inválida. La salida es la
     reparación, que muestra antes las fórmulas que elimina.
  6. **Sin promesas futuras.** «Permitiría» significa **solo** que ninguna otra fuente inválida conocida, de este rack o
     de otro rack que consuma el cierre de X, mantiene por sí sola bloqueada esta recuperación. **No** significa que R1
     vaya a admitir cualquier corrección: la definición nueva de X sigue sujeta a la validación normal del estado
     resultante, que incluye R1 con los dominios de las propiedades evaluados con los valores nuevos, R2 y el contrato
     raíz, y en commit el `PlanReadSet` (D19). **R1 y la decisión C4-13 de I-47 no se relajan** (P30.24; ALT-33,
     ALT-34).
  7. **Rechazo de R1.** Se informa el fallo tipado exacto del estado intentado, sin inventar fallos no evaluados ni
     repetir una sugerencia previa. Si el rack ya estaba afectado, se añaden sus fuentes inválidas previas y las
     razones que entonces aplicaban, como contexto separado; la indicación de reparación previa y el aviso siguen
     las condiciones de D18.2–D18.3, nunca como explicación falsa del estado intentado.
- **Las fuentes que la reparación retira se protegen** con una `RepairDecisionObservation` por fuente (D20).

#### D18.1 — Firmas, unidades y descubrimiento para la recuperación

Pertenece a la capa de recuperación de G9 y G10, en Application, sobre
`RegistryEvaluation`, el descubrimiento de consumidores e `InspectBinding`; **no** es del núcleo de grafo de G7. Para una
fuente clasificada por `InspectBinding` como `RepairableSemanticFailure(Upstream)`:

```text
SourceRoots(fuente)   = ∪ RootCauses(v)   para cada variable v que la fuente lee y que falla      // firmas
RecoveryUnits(firmas) = { unidad(r) | r ∈ firmas }                                                // deduplicado
unidad(r)             = CycleRoot(C)       si r es el CycleRoot de la SCC C
unidad(r)             = el dueño de r      en otro caso
```

- `SourceRoots` y `RecoveryUnits` solo se definen para fuentes `Upstream`.
- **Firma frente a unidad.** `RootCauses` y `SourceRoots` siguen siendo conjuntos de **firmas** (A3 §5.5): son los datos
  que comparan M10(a) y M10(b), y no se debilitan. `RecoveryUnits` solo responde a «qué unidad authored habría que
  corregir»:
  - varias firmas de un mismo dueño son **una** unidad (U11);
  - el `CycleRoot` de una SCC es **una** unidad;
  - un miembro de ciclo con fallos propios es una unidad **distinta** del `CycleRoot` de su SCC: sus fallos propios no
    se funden en el ciclo.
- **Orden de las unidades**, solo para listarlas: por el id relevante en el orden de P11.1 —el dueño, o el miembro mínimo
  del ciclo— y, con el mismo id, la unidad de dueño antes que la de ciclo.
- **Conteo por unidades, como V6.** §7.2.1 cuenta «variables cuya propia definición falla, o un ciclo tomado como una
  unidad» (l. 2759-2761). Un `ChangeDefinition(X)` que evalúe bien (R2) quita a la vez todos los fallos propios de X, así
  que varias firmas de X no impiden la sugerencia. D18 puntos 1-2 se leen con A3 y no cambian (A3 §10.3).

La sugerencia «Corregir <X> permitiría recuperar…» para esa fuente existe **solo** si se cumplen todas estas condiciones:

1. **Precondición de descubrimiento.** El **mismo** descubrimiento de consumidores que necesitaría el intent correctivo
   termina con éxito sobre el snapshot diagnosticado (P21.2 paso 7, l. 1536-1537; P21.4, l. 1557-1563): el del cierre
   `I(X)` para una unidad de dueño, o el del cierre de un miembro, que contiene toda la SCC y sus dependientes, para un
   `CycleRoot`. Si ese descubrimiento abortaría —por la precondición global de sobres no interpretables, una sonda
   `Indeterminate` en cualquier rack, o positivos parciales o autoridad no `Single` en un rack con alguna vista positiva
   (Discovery §7, l. 537-541); son las cuatro únicas causas de aborto de la familia A (P21.4 y Discovery §7)—, no hay
   sugerencia (U10). Un rack cuya sonda da `Indeterminate` **nunca** se lee como «no consume» (D14, l. 914-916; §5.1,
   l. 2601-2603).
2. `RecoveryUnits(SourceRoots(fuente)) = {X}`;
3. si X es un `CycleRoot`, su SCC es un **ciclo simple** (A3 §6.2);
4. **todas** las fuentes fallidas de todos los racks que ese descubrimiento identifica como consumidores del cierre de X
   —la propia fuente incluida— están clasificadas como `RepairableSemanticFailure(Upstream)` **y** tienen
   `RecoveryUnits(SourceRoots) = {X}`, con la misma X;
5. siguen vigentes las demás reglas de §7.2, entre ellas §7.2.6 y R1 sin relajar.

- **Fuentes no `Upstream`.** Cualquier fuente fallida de esos racks clasificada como `RepairableMissingTarget`,
  `Intrinsic`, `Domain`, estructural (`FatalMalformedReference`, `FatalUnknownProperty`) o `FatalIncompatibleTarget`
  hace que la condición **no** se cumpla (U8). Las raíces de las variables que lee no la convierten en una fuente
  `Upstream`.
- **Igualdad.** La condición 4 exige igualdad de unidades: una fuente `Upstream` con unidades `{X, Y}` también la
  incumple, aunque contenga X.
- **Alcance del descubrimiento.** Las condiciones solo miran los racks que el descubrimiento identifica como
  consumidores. Un rack que el mismo descubrimiento clasifica como ajeno al cierre —cero positivos—, aunque tenga fuentes
  semánticas fallidas o una autoridad multi-vista no `Single`, no bloquea (T-A3-57, T-A3-58). Un rack que no se puede
  clasificar nunca es ajeno: bloquea por la condición 1.
- **Rack `Blocked` y descubrimiento.** `FatalIncompatibleTarget` es inalcanzable con un solo `VariableType` (P24.6,
  l. 2164), y toda otra causa de `Blocked` —`FatalUnknownProperty` y `FatalMalformedReference`— hace `Indeterminate` la
  sonda: la semántica de la sonda no cambia (P21.4, l. 1557-1561; Discovery §7, l. 541; D14, l. 914-916;
  `ProjectVariableConsumerProbe.cs:71-84`). Por tanto, en esta build un rack `Blocked` nunca se clasifica como ajeno y
  siempre da además `DiscoveryIndeterminate` para cualquier cierre.
- **Fallo propio estático de una fuente.** Una fuente cuya propia expresión tiene aridad errónea o forma no canónica es
  `Intrinsic` aunque además lea una variable que falla: el fallo propio precede al superior, leído del orden «(propio,
  superior y dominio)» de la precedencia de P24.6 (l. 2168-2169), y hace a la fuente «no corregible desde arriba»
  (§7.2.1). La precedencia anterior de P24.6 se mantiene: por ejemplo, `ABS(#<gone>, 2)` es `RepairableMissingTarget`.
  Con un fallo estático conocido, la fuente no se evalúa numéricamente y su firma `Intrinsic` es solo la de sus fallos
  estáticos: `1 / 0 + ABS(1, 2)` es `Intrinsic(InvalidArguments(ABS, 2))`, y su `DivisionByZero` es latente
  (T-A3-59). Sin fallo estático propio, una fuente que lee una variable que falla tampoco se evalúa: `H + 1 / 0` con H
  fallando es `Upstream`, y su `DivisionByZero` es latente, no un fallo propio conocido (M1, N-5).
- **Ciclo simple.** Si X es un `CycleRoot` de un ciclo simple, `<X>` nombra a sus miembros y «corregir cualquiera de
  ellos rompe el ciclo» es cierto. En una SCC no simple **nunca** hay sugerencia (U7).
- **Sin sugerencias falsas respecto de lo conocido.** Todo se decide con los diagnósticos **conocidos** del snapshot
  (A3 §5.1, N-5) y con el descubrimiento completo de la condición 1. La regla puede ocultar una sugerencia válida y nunca
  muestra una que desmientan las fuentes inválidas conocidas de este rack o de los racks que consumen el cierre de X,
  que es el alcance de §7.2.6. Un fallo numérico latente puede aparecer después de la corrección (T-A3-39).

#### D18.2 — Razones estructuradas de bloqueo

Cuando la condición no se cumple para una fuente `Upstream` f, nunca hay
sugerencia y el diagnóstico lleva, como datos estructurados y no como texto, el **conjunto estable** de todas las razones
que aplican, siempre en este orden:

| Razón | Cuándo | Datos |
|---|---|---|
| `OtherInvalidSources` | Hay al menos una **fuente bloqueante** (abajo) | Las fuentes bloqueantes, con rack, propiedad y causa, como en §7.2.4, y si cada una está en el rack de f o en otro |
| `SeveralRecoveryUnits` | `RecoveryUnits(SourceRoots(f))` tiene más de una unidad | Las unidades, en su orden, y las firmas de `SourceRoots(f)`, en el orden de A3 §5.7 |
| `NonSimpleCycle` | `RecoveryUnits(SourceRoots(f)) = {CycleRoot(C)}` y C no es un ciclo simple | Los miembros de C |
| `DiscoveryIndeterminate` | El descubrimiento de la condición 1 abortaría para alguna unidad de `RecoveryUnits(SourceRoots(f))` | Las causas de aborto —sobre no interpretable, sonda `Indeterminate`, positivos parciales o autoridad no `Single`—, cada una con la definición o el rack que la provoca y las unidades afectadas, en el orden fijado abajo |

El mismo estado diagnosticado da siempre el mismo conjunto de razones: no hay precedencia entre ellas. Para que también
los datos sean estables, las listas van en un orden fijo, que no es semántico y no depende del orden del barrido:

- **Identidad de rack.** Un rack se identifica, se compara y se ordena por su `RackId` con `OrdinalIgnoreCase`, la
  comparación con la que se agrupan sus vistas (`ProjectVariableConsumerDiscovery.cs:232`). Como dato se da la grafía
  mínima en `Ordinal` entre las de sus vistas, nunca la de la primera vista del barrido, y la misma en todas las razones.
- **Fuentes bloqueantes:** por rack (`OrdinalIgnoreCase`) y después por token de propiedad (`Ordinal`, P21.6,
  l. 1658-1659).
- **Causas de aborto:** las cuatro clases de la familia A (P21.4, l. 1557-1561, y Discovery §7, l. 537-541), sin
  duplicados, cada causa con el conjunto de unidades de `RecoveryUnits(SourceRoots(f))` cuyo descubrimiento aborta por
  ella, en el orden de las unidades:
  - `EnvelopeUnclassifiable(DefinitionId)`: una definición con sobre no interpretable. Es la precondición global y no
    depende del cierre (`:104-109`, `:197-216`): si hay alguna, el descubrimiento de **toda** unidad aborta antes de
    agrupar racks, se listan **todas**, una por `DefinitionId` distinto y ordenadas por `DefinitionId` (`Ordinal`),
    cada una con todas las unidades, y **ninguna** causa de rack;
  - si no hay ninguna, para cada par (rack, unidad) cuyo descubrimiento aborta, **una** causa: la primera que aplica en
    el orden de las comprobaciones de la familia A (`:122-149`) —`ProbeIndeterminate(rack)`, `PartialPositives(rack)`,
    `NonSingleAuthority(rack)`— sobre el descubrimiento **por conjunto** de P21.4 del cierre de esa unidad (una vista es
    positiva si alguna de sus fuentes toca ese cierre), no sobre descubrimientos por variable: la equivalencia de P21.4
    (l. 1562-1563) fija el resultado —aborto y consumidores—, no la causa. `ProbeIndeterminate` no depende del cierre
    y es la única causa de su rack; `PartialPositives` y `NonSingleAuthority` sí dependen, así que un mismo rack puede
    aparecer con las dos, cada una con sus unidades. Las causas se agrupan por (rack, clase) y se ordenan por rack y
    después por clase, en ese orden de comprobación.
  - El código de hoy se detiene en la primera definición o en el primer rack que aborta (`:204-212`, `:122-149`); G9
    tiene que evaluar todas las definiciones y todos los racks para obtener el conjunto completo. El conjunto de entradas
    es el del preflight del intent correctivo; la diferencia con el panel, que solo mira entradas colocadas, es el
    hallazgo lateral L1 del Discovery (§7, l. 544), que A3 no resuelve.

**Fuente bloqueante.** Para una fuente `Upstream` f, otra fuente fallida g es bloqueante si está en el rack de f o en un
rack que el descubrimiento identifica como consumidor del cierre de alguna unidad de `RecoveryUnits(SourceRoots(f))`, y
además:

- g no está clasificada como `RepairableSemanticFailure(Upstream)`; o
- g es `Upstream` y `RecoveryUnits(SourceRoots(g))` contiene alguna unidad que **no** está en
  `RecoveryUnits(SourceRoots(f))`.

- **Firmas de una misma unidad** no hacen bloqueante a una fuente: con `H = ABS(#<m>, 2)` y `J = H + 1`, las fuentes
  `f = H + 1` y `g = J * 2` tienen la misma unidad H, aunque g la alcance a través de J.
- **Con `DiscoveryIndeterminate`**, esta regla precisa la definición anterior: las fuentes bloqueantes se buscan en el
  rack de f, cuyo consumo del cierre consta por la propia f, y en los consumidores de las unidades cuyo descubrimiento
  sí terminó; A3 no afirma nada de los racks que el descubrimiento no pudo clasificar. `InspectBinding` solo clasifica
  las fuentes de un rack con autoridad `Single` (`ProjectVariablesWorkspace.cs:436-447`), cuyas vistas son
  estructuralmente iguales, así que el rack de una f clasificada nunca aborta por positivos parciales ni por autoridad no
  `Single`; sí puede abortar por una sonda `Indeterminate` (T-A3-56), y entonces sus demás fuentes fallidas se listan,
  estructurales incluidas.
- **Exactitud.** Con una sola unidad X, «g no es bloqueante» equivale exactamente a la condición 4: corregir X recupera
  toda g cuyas unidades son {X}. Con varias unidades, la lista es **informativa**: excluye las fuentes cuyas unidades
  están todas en las de f, que no afirman nada que `SeveralRecoveryUnits` no diga ya, aunque alguna pueda bloquear la
  corrección de una unidad concreta. Por ejemplo, con `f = A1 + A2` en el rack K y `s3 = A1 + 1` en el rack K3, corregir
  A1 recupera s3 y la corrección queda bloqueada solo por f; en cambio, con `A2 = #<a2> + A1` y `g = A1 + 1` en el mismo
  rack que `f = A2 + 1`, corregir A2 recupera f y R1 bloquea por g.
- **Completitud.** La condición 1 da `DiscoveryIndeterminate`; la 2, `SeveralRecoveryUnits`; la 3, `NonSimpleCycle`, que
  solo se evalúa con una unidad; y la 4 —o, con varias unidades, la definición de fuente bloqueante— da
  `OtherInvalidSources`. La condición 5 no añade condiciones de presentación. `SeveralRecoveryUnits` y `NonSimpleCycle`
  se excluyen, y ninguna lista queda vacía: una SCC no simple tiene al menos dos miembros y un aborto tiene causa.
- **Texto literal de §7.2.4.** «La corrección de <X> no puede aplicarse mientras este rack mantenga otras fuentes
  inválidas» (l. 2778-2779) se usa **solo** si hay `OtherInvalidSources` con al menos una fuente bloqueante en el rack de
  f, y no aplican ni `SeveralRecoveryUnits` ni `NonSimpleCycle`. Entonces `<X>` es la unidad única, y la lista incluye
  también las fuentes bloqueantes de otros racks, como en V6. Si además aplica `DiscoveryIndeterminate`, se informa
  también, con las reglas siguientes.
- **En los demás casos**, la capa de texto nombra con verdad cada razón que aplica y lista las fuentes bloqueantes con su
  rack, las unidades y sus firmas, los miembros o las causas del aborto. **No** dice «este rack» si ninguna fuente
  bloqueante o causa de aborto está en él; **no** afirma que haya otras fuentes inválidas sin `OtherInvalidSources`; y
  con `DiscoveryIndeterminate` **no** afirma que exista una fuente inválida cuyo estado se desconoce ni que el rack no
  clasificado consuma el cierre.
- **Veracidad de `SeveralRecoveryUnits`, `NonSimpleCycle` y `DiscoveryIndeterminate`.** Su redacción solo puede afirmar
  que la recuperación con una corrección **no puede garantizarse ni demostrarse** en el estado actual, nunca que sea
  imposible. Corregir un intermedio común puede recuperarla, como B en U2 o H en CX-2; en una SCC no simple, corregir un
  miembro que esté en todos sus ciclos también; y un descubrimiento hoy indeterminado puede completarse después.
- **Aviso de reparación y rack `Blocked`.** El aviso de §7.2.4 con la fórmula canónica (l. 2781-2784) se conserva
  **solo** si el rack es reparable (`RackRepairability = Repairable`, P24.6). Un rack `Blocked` no ofrece acción ni aviso
  de reparación, aunque alguna de sus fuentes semánticas pareciera reparable (D18, l. 1039-1041); el texto dice que está
  bloqueado sin reparación (§5.1, l. 2601-2603; §7.2, l. 2731-2733). En esta build ese rack da además
  `DiscoveryIndeterminate`. Si un diagnóstico posterior deja el rack `Blocked`, no se repite una sugerencia anterior.
- **Reparación que abortaría.** Tampoco se muestra el **aviso** «Reparar el rack eliminará…» cuando la propia
  resolución del rack que usa `RepairBrokenRack` abortaría (familia B, `ProjectVariableConsumerDiscovery.cs:157-189`;
  Discovery §7, l. 537, 540). Como la fuente clasificada ya exige autoridad `Single`, en la práctica eso solo ocurre con
  la precondición global de sobres no interpretables. El texto no describe una reparación que no puede ejecutarse. A3
  fija solo el texto: **no** cambia la visibilidad de la acción de reparación, que hoy se ofrece también con el dibujo
  indeterminado (`ProjectVariablesWorkspace.cs:284-287`), ni resuelve el hallazgo lateral L1 del Discovery (§7,
  l. 544). El aviso de fórmulas de P21.2 paso 8 y de D18 punto 7 sigue estas dos reglas.
- **Un aborto del descubrimiento solo quita la sugerencia.** Calcular la condición de §7.2 para el mensaje de
  confirmación de la reparación no hace abortar la reparación: `DiscoveryIndeterminate` solo quita la sugerencia y
  añade su razón. Con K9 `Indeterminate` y K `Repairable` (U10), la reparación de K y su aviso siguen disponibles.
- D18.3 fija la redacción en español de los casos nuevos, que implementará la capa de texto de G10 (P15.4).
- **Rechazo de R1.** Si R1 rechaza una corrección intentada:
  - se informa el **fallo tipado exacto del estado resultante intentado**: el rack que no resuelve, con su `OutOfRange`
    o su clasificación (P22.8, P24.5);
  - no se inventan diagnósticos que no se evaluaron;
  - no se repite la sugerencia de recuperación como si el estado intentado la cumpliera;
  - si el rack ya estaba afectado antes, el mensaje **además** indica que hay que repararlo primero, lista sus fuentes
    inválidas del estado diagnosticado **antes** de la corrección, con sus razones de M10(d), y advierte qué fórmulas
    eliminaría la reparación, como exigen P21.2 paso 8 (l. 1538-1541), §7.2.7 (l. 2798-2799) y D18 punto 7; ese
    contexto nunca se presenta como el fallo del estado resultante, y el aviso de fórmulas sigue las reglas de
    «Aviso de reparación y rack `Blocked`» y «Reparación que abortaría». Si antes se cumplía la condición, sus fuentes
    inválidas se listan igual, pero no hay razones de bloqueo previas que mostrar.

#### D18.3 — Textos en español y composición del diagnóstico

Las razones y sus listas se producen en Application. La capa única de texto presenta **todas** las aplicables en
el orden de D18.2 y con sus datos; los textos siguientes no son identidad semántica ni datos de concurrencia.

| Caso | Texto |
|---|---|
| `OtherInvalidSources`, una unidad X, sin `SeveralRecoveryUnits` ni `NonSimpleCycle`, con al menos una fuente bloqueante en el rack de f | «La corrección de <X> no puede aplicarse mientras este rack mantenga otras fuentes inválidas.» |
| `OtherInvalidSources`, mismas condiciones pero bloqueantes solo en otros racks | «La corrección de <X> no puede aplicarse mientras los racks indicados mantengan las fuentes inválidas que se enumeran.» |
| `OtherInvalidSources` cuando también aplica `SeveralRecoveryUnits` o `NonSimpleCycle` | «Se detectaron otras fuentes inválidas en los racks indicados:» |
| `SeveralRecoveryUnits` | «Esta fuente tiene varias unidades de recuperación. No puede garantizarse su recuperación con una sola corrección en el estado actual.» |
| `NonSimpleCycle` | «El ciclo indicado no es simple. No puede garantizarse su recuperación corrigiendo un solo miembro en el estado actual.» |
| `DiscoveryIndeterminate` | «No se pudo completar el descubrimiento de consumidores para las unidades indicadas. No puede demostrarse la recuperación con una corrección en el estado actual.» |
| Rack `Blocked` | «Este rack está bloqueado y no admite reparación. Corrige las causas indicadas antes de continuar.» |

Tras `OtherInvalidSources` se listan rack, propiedad y causa de cada fuente bloqueante; tras `SeveralRecoveryUnits`,
las unidades y firmas; tras `NonSimpleCycle`, sus miembros; tras `DiscoveryIndeterminate`, las causas de aborto con
su definición o rack y las unidades afectadas. Se respetan los órdenes de D18.2. La grafía de rack no depende del
primer elemento del barrido. Las causas indeterminadas no se describen como fuentes inválidas confirmadas ni como
consumidores confirmados. El texto de rack `Blocked` se añade a las razones aplicables, sin sugerencia ni reparación.

El aviso siguiente se añade **solo** con `RackRepairability = Repairable` y si la resolución usada por la reparación
no abortaría; se muestra para cada fórmula que eliminaría, con el formatter canónico `Q(clave)`:

```text
Reparar el rack eliminará también esta fórmula:
<formula canónica>

Guárdala si deseas volver a escribirla después.
```

Un aborto del descubrimiento de consumidores en otro rack solo elimina la sugerencia: no suprime este aviso ni
aborta por sí mismo la reparación del rack reparable. Un sobre global no interpretable sí puede hacer abortar la
resolución usada por la reparación y, entonces, impide el aviso; no se cambia por ello la visibilidad vigente de la
acción. El rack `Blocked` nunca ofrece esa acción ni el aviso.

En el rechazo de R1 se encabeza el fallo real del intento con «La corrección intentada no puede aplicarse:» y su
fallo tipado. El contexto previo, si el rack ya estaba afectado, se separa con «Fuentes inválidas antes de la
corrección:» y sus clasificaciones y razones previas. Si corresponde una reparación, se indica «Repara este rack
antes de aplicar esta corrección.»; si estaba `Blocked`, se usa su mensaje de bloqueo, sin instrucción de reparación.
No se repite la sugerencia previa. El aviso de fórmulas conserva las condiciones anteriores incluso en este rechazo.

### D19 — `PlanReadSet`: revisión ACOTADA de la política V8-R05 de I-48

**I-49 revisa de forma acotada la política de concurrencia V8-R05 de I-48. V8-R05 no queda intacta.** La Proposal V8 de
I-48 (§6) dejó la concurrencia **fuera de alcance**: sin comparar el registro de preflight con el de commit, sin
concurrencia optimista y sin rechazo de planes obsoletos. I-49 introduce una versión **acotada** de las tres cosas,
limitada al `PlanReadSet`, y la aplica también a planes que ya existían: cambiar el valor de X en un rack que además lee
Y por referencia directa aborta si Y cambió. Presentarla como compatible con una V8-R05 intacta está rechazado
(ALT-36). **Esta revisión requiere la aceptación explícita del Owner mediante este ADR antes de implementarse** (DR-8,
§2.3, §9); G9 no implementa el `PlanReadSet` hasta entonces (§8.2).

**Regla** (P21.6):

```text
un valor que participó realmente en el PlanReadSet
cambia entre preflight y commit
→ ABORT BEFORE WRITE
```

«Participó» es el resultado de una observación en su fase, y «cambia» significa que el resultado observado difiere del
esperado. La regla operativa es, por tanto:

```text
observed result != expected result → ABORT BEFORE WRITE
```

y **nunca** «si algún símbolo del `PlanReadSet` falla → ABORT», que contradiría R2 (ALT-37). En una
`RepairDecisionObservation`, lo que participó es la razón reparable de la fuente que se retira, y «cambia» significa que
la inspección recomputada da otra razón u otros datos estables (D20).

**Modelo consensuado** (P21.6):

```text
PlanReadObservation
    = SymbolResultObservation
    | RepairDecisionObservation

SymbolResultObservation
{
    SymbolId,
    Phase          = Before | After,
    ExpectedResult = Success(value) | Failed(fallo tipado con datos estables)
}

RepairDecisionObservation
{
    RackId,
    PropertyId,
    ExpectedRepairReason
}
```

- `Before` es lo que el preflight usó **antes** de aplicar la `RegistryMutation`, sobre el registro «antes» acreditado.
  `After` es lo que usó sobre el estado hipotético «después» que produce `ApplyTo`; ahí viven, entre otros, los valores
  con los que se construyen los racks finales del plan. `ExpectedResult` es el resultado de ese símbolo en la
  `RegistryEvaluation` de esa fase durante el preflight, **transportado como dato**. Un mismo `SymbolId` puede tener
  una observación `Before` y otra `After` si el plan realmente lo usa en las dos.
- **Regla de derivación**: una observación entra en el `PlanReadSet` **si y solo si** el preflight **realmente** usó ese
  resultado o esa clasificación para **decidir el plan**, para **producir algo que el plan escribirá** o para
  **justificar una retirada destructiva**. Todo dato o clasificación semántica que justifica una decisión destructiva
  queda representado por una observación comparable hasta el commit: el resultado de un símbolo en una fase como
  `SymbolResultObservation`, y la clasificación reparable de una fuente que se retira como `RepairDecisionObservation`.
  Los resultados de símbolo que solo sirvieron para esa clasificación no se observan aparte: la razón los resume (D20).
- **Por operación** (tabla de P21.6):

  | Operación | `RegistryMutation` | `SymbolResultObservation` `Before` | `SymbolResultObservation` `After` | `RepairDecisionObservation` | Nunca |
  |---|---|---|---|---|---|
  | `Create(X, d)` | `Add` | — | X, cuyo resultado examina el contrato raíz (caso A) | — | — |
  | `ChangeDefinition(X, d)` | Cambio de definición | Cada símbolo de `I` distinto de X: R2 y el contrato raíz (casos B–D) comparan su estado previo | Cada símbolo de `I` (R2 y contrato raíz) y cada símbolo que leen los racks planificados en su resolución final, en **todas** sus propiedades: referencias directas y dependencias de sus expresiones (R1 y efectivo) | — | — |
  | `Rename(X, n)` | `Rename` | — | — | — | Ningún valor: `Rename` no evalúa ni cambia evaluaciones (P19.6–P19.7) |
  | `Delete(X)` | `Remove` | — | — | — | `After(X)` y cualquier valor: el preflight solo acredita, comprueba el id y bloquea por consumidores o dependientes (P20.2, P20.8) |
  | `UnlinkAllAndDelete(X)` | `Remove` | X, cuyo valor evaluado se materializa en las referencias directas (§7.1); sin consumidores no hay materialización ni observación | Cada símbolo que leen los racks finales, ya sin X | — | `After(X)` |
  | `RepairBrokenRack(rack)` | Ninguna | — | Cada símbolo que leen las fuentes que el rack **conserva** y cuyo efectivo importa | Una por cada fuente que se **elimina**, con su razón reparable | `SymbolResultObservation` por lo que leen solo las fuentes eliminadas; una observación por cada valor intermedio de una fuente eliminada; un símbolo inventado para un id ausente |
  | `Link` y `Unlink` heredados (`SelectiveBindingIntentPreflight.Run`, sin disparador productivo; P29.6) | Ninguna | `Unlink`: la variable cuyo efectivo se materializa | Los demás símbolos que lee el rack final; en `Link`, también la variable enlazada | — | — |

- **Sin dependencias transitivas por separado.** El resultado de un símbolo observado ya incorpora el de sus
  dependencias; un cambio de una dependencia que altera lo que el plan usó cambia ese resultado y aborta, y uno que no
  lo altera no toca nada de lo decidido o escrito (ALT-41). Lo mismo vale para las decisiones de reparación: se compara
  la razón, no cada valor intermedio (ALT-42). **Un id ausente no es un símbolo**: su ausencia viaja en el fallo tipado
  del símbolo que lo referencia o en la razón `MissingTarget(ids)` (ALT-44).
- **Invariantes del plan**, verificados al construirlo:
  - ninguna `SymbolResultObservation` `After` nombra un símbolo que la `RegistryMutation` elimina;
  - no hay dos `SymbolResultObservation` con el mismo `SymbolId` y la misma fase;
  - cada `ExpectedResult` sale de la `RegistryEvaluation` de su fase en el preflight, nunca de una evaluación propia, y
    cada `ExpectedRepairReason` sale de la inspección que hizo el preflight;
  - cada fuente que retira `RepairBrokenRack` tiene exactamente una `RepairDecisionObservation`; ninguna fuente
    conservada la tiene, y ningún símbolo recibe una `SymbolResultObservation` solo porque lo lee una fuente retirada;
  - el orden es determinista: primero las `RepairDecisionObservation`, por `RackId` y por token de propiedad
    (`Ordinal`); después las `Before`, y al final las `After`, cada fase en el orden de P11.1.
- **No son observaciones** las precondiciones propias de la mutación, que conservan su contrato: legibilidad
  estructural, acreditación de identidad (V8-R01), existencia de los ids que el plan usa y forma enlazada. Los ids que
  una `RepairDecisionObservation` espera ausentes no son ids que el plan usa. **El authored de los racks tampoco es una
  observación.**
- **Resultado comparable.** `Success` compara el valor exacto (`double.Equals`). `Failed` compara, en el orden
  determinista de P15.7, el **código** de cada diagnóstico del símbolo y sus **datos estructurados estables**; **nunca**
  mensajes, texto localizado, posiciones ni la traza opt-in (ALT-39):

  | Código | Datos estables que se comparan |
  |---|---|
  | `BrokenReference` | Los ids ausentes, en el orden de P11.1 |
  | `Cycle` | Los miembros del ciclo, en el orden de P11.1 |
  | `DependencyFailed` | La causa directa, los ids de la cadena representativa y la firma de la raíz de D15 |
  | `InvalidArguments` | El token de la función y el número de argumentos recibidos |
  | `DivisionByZero`, `NonFiniteResult`, `NonCanonicalForm` | Solo el código |

  `OutOfRange` no aparece en una observación. Si la implementación necesita más datos para distinguir dos causas, los
  añade como datos estables, nunca como texto. Por el determinismo del motor, una re-lectura sin cambios no produce
  abortos espurios.
  Además, todo `Failed` compara el **conjunto completo `RootCauses(símbolo)`** de D15, firma a firma en su orden
  estructural. Los diagnósticos siguen siendo locales. Raíces {A, B} frente a {A}, o la misma unidad con distintas
  firmas, no coinciden aunque la cadena representativa parezca igual. Si solo cambia la cadena, tampoco coincide
  una `SymbolResultObservation`; `Upstream` se distingue en D20. Ningún dato localizado ni traza entra.
- **Secuencia en commit**, dentro de Application, no en el Plugin, con el orden **exacto** de V6 (P21.6):

  ```text
  sin RegistryMutation y sin observaciones:              no re-lee                   // sin cambio
  lastRead   = Read(...)
  si lastRead no es estructuralmente legible:            ABORT BEFORE WRITE
  accredited = Accredit(lastRead)                         // sin cambio (V8-R01); si falla: ABORT BEFORE WRITE
  si algún id que el plan usa ya no está:                 ABORT BEFORE WRITE          // precondición, no observación

  before     = RegistryEvaluation(accredited)             // solo si hay observaciones Before o de reparación
  para cada RepairDecision(rack, propiedad, esperada):    // solo RepairBrokenRack, sin RegistryMutation
      si InspectBinding(fuente retirada, accredited, before) difiere de esperada:
                                                          ABORT BEFORE WRITE          // registro 0, vistas 0
  para cada Before(s, esperado):
      si Result(before, s) difiere de esperado:           ABORT BEFORE WRITE          // registro 0, vistas 0

  changed    = ApplyTo(accredited.Document)               // sin cambio; sin RegistryMutation, el propio documento
  after      = RegistryEvaluation(changed)                // solo si hay observaciones After;
                                                          // sin RegistryMutation, la misma evaluación que before
  para cada After(s, esperado):
      si Result(after, s) difiere de esperado:            ABORT BEFORE WRITE          // registro 0, vistas 0

  TryWrite(..., lastRead, changed, ...)                   // solo con RegistryMutation (sin cambio)
  escribir las vistas del plan                            // en la misma transacción
  ```

  «Sin cambio» se refiere al comportamiento vigente (I-47/I-48).

  `Result(evaluación, s)` es el resultado de `s` en esa evaluación, o **ausente** si `s` no existe en ese snapshot, y un
  resultado ausente siempre difiere. El aborto es tipado e informa la primera observación que difiere, en el orden de
  los invariantes. Las decisiones de reparación se validan **antes** que las observaciones de símbolo (D20).
- **También sin `RegistryMutation`.** Hoy el ejecutor solo re-lee el registro cuando el plan lo escribe. Con I-49, todo
  plan con observaciones re-lee, re-acredita y compara en la **misma transacción** y **antes** de escribir la primera
  vista, aunque no escriba el registro. Un plan sin ninguna observación no re-lee por el `PlanReadSet`; toda reparación
  retira al menos una fuente, así que lleva al menos una `RepairDecisionObservation` y re-lee.
- **Una sola autoridad.** La lógica vive en Application y el ejecutor del Plugin solo la invoca; el Plugin sigue siendo
  el único que lee el NOD (ADR-0006). Hay una evaluación por snapshot y la reparación se recomputa con la **misma**
  `InspectBinding`. El commit **no** vuelve a ejecutar el preflight: no re-aplica R1, R2 ni el contrato raíz, solo
  comprueba que lo que el preflight usó sigue igual (ALT-40).
- **Un fallo no aborta por sí mismo**:

  | Esperado | Observado en commit | Resultado |
  |---|---|---|
  | `Failed(BrokenReference de Y)` | `Failed(BrokenReference de Y)` | **MATCH**: el commit sigue |
  | `Failed` | `Success` | `ABORT BEFORE WRITE` |
  | `Success` | `Failed` | `ABORT BEFORE WRITE` |
  | `Success(5)` | `Success(6)` | `ABORT BEFORE WRITE` |
  | `Failed(causa A)` | `Failed(causa B)` | `ABORT BEFORE WRITE` |
  | Cualquiera | Ausente | `ABORT BEFORE WRITE` |

  Un fallo preexistente idéntico y admitido por R2 no bloquea el commit.
- **Alcance.** Rige para los planes con `RegistryMutation` y para los planes sin `RegistryMutation` que escriben racks.
  Un cambio **fuera** de las observaciones: **NO ABORT**. Se conserva el contrato central de V8-R05: una re-lectura que
  cambió pero sigue siendo usable **no** se rechaza por el mero hecho de haber cambiado.
- **No introduce** hash de snapshot, token global, comparación completa del registro, rechazo por cambios fuera del
  `PlanReadSet`, locks adicionales ni re-ejecución del preflight en commit (P30.12; ALT-35, ALT-40).
- **No cubre**: el authored de los racks, porque el bucle de RACKVARIABLES es modal y re-lee en cada vuelta, y una
  `RepairDecisionObservation` re-inspecciona la entrada de fuente que transporta el plan, no la del dibujo; un cambio
  de estructura del registro que no altera ningún resultado observado, como un dependiente nuevo creado entre preflight
  y commit, que sigue fuera de alcance como en V8-R05; y el reconciliador de RACKEDITAR, que no produce un
  `MutationPlan` y no lleva `PlanReadSet` (P21.12, P30.25).
- **Escenarios normativos** (P21.6): R2 con un fallo previo que sigue idéntico → commit, y recuperado o con otra causa
  → abort; `Delete` sin observaciones → commit; `UnlinkAllAndDelete` con `Before(X)` → abort si X cambió o desapareció,
  o si cambió un símbolo `After`; y las decisiones de reparación de D20. Los fijan T-V3-10, T-V4-07 a T-V4-09, T-V5-01
  a T-V5-09 y T-V6-01 a T-V6-09 (P28).

#### D19.1 — Datos de las decisiones de reparación (RR-1)

La regla de derivación (D19, l. 1158-1163) exige una observación
comparable para todo dato que justifique una decisión **destructiva**. `RepairBrokenRack` retira las fuentes que
`InspectBinding` clasifica como reparables; el mensaje de §7.2 se muestra antes de confirmar, pero no cambia qué fuentes
se retiran. Inventario tras A3-R2:

| Dato | ¿Decide qué borra `RepairBrokenRack`? | En el `PlanReadSet` |
|---|---|---|
| Clasificación reparable de cada fuente retirada | Sí | `RepairDecisionObservation` (D20), sin cambio |
| `MissingTarget`: ids ausentes de la fuente | Sí | Datos de la razón, sin cambio |
| `Intrinsic`: firma de los fallos propios, incluidos el `InvalidArguments` estático con token y número de argumentos y `NonCanonicalForm` | Sí | Datos de la razón (A3 §7.2) |
| `Upstream`: ids de las variables leídas que fallan y `RootCauses` **completo** de cada una, por variable y en el orden de P11.1 | Sí | Datos de la razón (M10(b)) |
| `Domain` | Sí | Solo la razón, sin cambio |
| `RecoveryUnits`, ciclo simple o no, resultado del descubrimiento de consumidores, clasificación de las fuentes de **otros** racks, razones de M10(d) y aviso de reparación | **No**: solo deciden qué se muestra | **No** se observan |

- Las fuentes hermanas del **rack reparado** sí deciden si hay reparación y qué se retira (`Blocked` o `Repairable`,
  P24.6, l. 2170-2171). Ya quedan cubiertas sin datos nuevos: cada fuente retirada por su
  `RepairDecisionObservation`, las fuentes conservadas por su validación en commit, y el estado estructural del rack por
  las precondiciones propias de la mutación y la re-inspección de `InspectBinding` (P21.6, l. 1661-1663, 1697-1699).

- `RecoveryUnits` se derivan de `RootCauses` y de las SCC, y no se guardan en el `PlanReadSet`: `Upstream` sigue
  comparando firmas. Dos estados que exigen corregir la misma variable X por razones distintas —X con `BrokenReference`
  antes y con `InvalidArguments` después— tienen la misma unidad X y **no** coinciden en `Upstream` (T-A3-53).
- `RepairBrokenRack` es de alcance de rack y no descubre consumidores del cierre (familia B: Discovery §7, l. 540;
  `ProjectVariableConsumerDiscovery.cs:157-160`, «link, unlink, repair»), así que ni el
  ciclo simple ni el descubrimiento deciden lo que borra. Si un diseño futuro de G9 hiciera depender la retirada de alguno
  de esos datos, RR-1 exigiría observarlo; A3 no lo introduce.
- Como ya fija N-3 (M10(b)), `Upstream` no protege las premisas de presentación.

### D20 — `RepairDecisionObservation` (RR-1)

- **Toda fuente que `RepairBrokenRack` decide retirar lleva una `RepairDecisionObservation`** con `RackId`, `PropertyId`
  y su razón reparable estructurada (P21.6). `ExpectedRepairReason` es la clasificación que dio `InspectBinding`
  durante el preflight —la única autoridad semántica sobre un binding persistido, aplicada a todo el rack—, y el plan
  transporta como dato la entrada de fuente que retira: la misma que inspeccionó el preflight. No tiene fase: el plan de
  reparación no tiene `RegistryMutation` y se evalúa sobre un único snapshot.
- **Razón comparable**, con la precedencia de D18 y solo datos estructurados estables; **nunca** texto en español,
  texto del formatter, posiciones, mensajes de UI ni la traza opt-in:

  | Razón | Cuándo | Datos estables que se comparan |
  |---|---|---|
  | `MissingTarget(ids)` | Algún id referido ausente, en referencia o en expresión | Los ids ausentes de la fuente, en el orden de P11.1 |
  | `Intrinsic(firma)` | La fuente falla por sí misma: `InvalidArguments`, `DivisionByZero`, `NonFiniteResult` o `NonCanonicalForm` | El código y los datos estables de cada diagnóstico propio, con la tabla de D19 |
  | `Upstream(firmas por variable)` | Una variable leída falla | Por **cada** variable leída que falla, en el orden de P11.1: su `SymbolId` y `RootCauses(variable)` **completo**, por SCC, firma a firma en el orden de D15. Los eslabones intermedios de la cadena representativa no entran |
  | `Domain` | La fuente evalúa, pero su efectivo queda fuera del dominio de la propiedad | Solo la razón: el valor fuera de dominio no se compara |

- **En commit**, la misma `InspectBinding` inspecciona la misma entrada de fuente contra la re-lectura acreditada y su
  `RegistryEvaluation`. Solo coincide si da la misma razón con los mismos datos estables; otra razón, `Healthy` o un
  estado estructural no coinciden. **Si la premisa de la reparación cambió antes de escribir: `ABORT BEFORE WRITE`**, y
  no se borra ninguna fórmula:
  - `MissingTarget`: siguen ausentes exactamente esos ids → coincide; reaparece alguno, desaparece otro o cambia la
    clasificación → no coincide;
  - `Upstream`: fallan las mismas variables leídas y el `RootCauses` completo de **cada una** es idéntico → coincide;
    si una se recupera, otra empieza a fallar, o alguna firma aparece, desaparece o cambia → no coincide;
    intercambiar raíces entre variables tampoco coincide, aunque la unión sea igual;
  - `Intrinsic`: la firma sigue igual → coincide aunque cambien valores intermedios (`A / (B - C)` pasa de `B = C = 5`
    a `B = C = 6` y sigue en `DivisionByZero`); cambia la firma o la fuente evalúa → no coincide;
  - `Domain`: sigue fuera de dominio → coincide; vuelve al dominio → no coincide.
- **Secuencia de `RepairBrokenRack`**:
  1. re-leer y acreditar el registro, con la precondición de ids;
  2. recomputar cada `RepairDecisionObservation` de las fuentes que el plan va a retirar;
  3. si alguna difiere: `ABORT BEFORE WRITE`;
  4. validar las `SymbolResultObservation` que exigen las fuentes que el plan conserva;
  5. solo entonces, escribir la `RackMutation`.

  Todo ocurre en la misma transacción, antes del primer `destination.Write`. Application es dueña de la inspección, la
  evaluación y la comparación; el Plugin solo lee y escribe físicamente; y no se re-ejecuta el preflight ni la
  reparación completa (ALT-45).
- **No** se observa automáticamente cada valor intermedio ni cada dependencia transitiva (ALT-42), y la premisa de la
  reparación no queda fuera de alcance: si quedara, el commit podría borrar una fórmula que ya volvió a ser válida
  (ALT-43).
- Lo fijan T-V6-01 a T-V6-09: fallo superior idéntico, recuperado o con otra causa; id ausente que sigue ausente o
  reaparece; dominio que sigue fuera o vuelve; fallo propio con valores intermedios cambiados; y una fuente retirada
  junto a una conservada (P28.8).

- **Firmas, no unidades ni caminos (A3 M10(b), M10(e)).** La misma `RecoveryUnit` con otra firma no coincide:
  cambiar X de `BrokenReference` a `InvalidArguments` exige `ABORT BEFORE WRITE`. Con las mismas variables leídas
  fallidas y sus mismos `RootCauses`, un cambio exclusivo de cadena representativa sí coincide en `Upstream`, aunque
  no en una `SymbolResultObservation` (D19). Entrar en una misma SCC por otro miembro no cambia su `CycleRoot` sin
  dueño, pero cambiar la variable directamente leída sí cambia el dato por variable y no coincide.
- **Protección acotada.** La observación protege la razón de la fuente concreta que se retira, no las demás fuentes
  o racks que justificaron la presentación. Con los mismos miembros de un `CycleRoot`, cambiar sus aristas internas
  de simple a no simple no altera esa raíz ni se observa por sí mismo. `RecoveryUnits`, elegibilidad global de la
  sugerencia, descubrimiento y textos no se añaden al `PlanReadSet`. Toda otra obligación debe salir de las
  observaciones exactas del plan. Se mantienen las exclusiones de V6: se re-inspecciona la entrada que transporta el
  plan, sin introducir observación del authored del rack ni cobertura implícita de sus carreras.
- **Pruebas futuras.** A los escenarios vigentes se añaden los de A3 §11.2 asignados a G9: raíces que aparecen,
  desaparecen o cambian; redistribución por variable; firmas distintas de una misma unidad; y cadenas distintas con
  raíces iguales. El registro de estas obligaciones no declara pruebas ejecutadas.

### D21 — `LinkedPropertyEditor`: el mismo control

- **El mismo `LinkedPropertyEditor` de I-48** acepta en el **mismo** campo (P23.1):

  ```text
  6
  =Holgura
  =Holgura + 2
  =(AlturaBase + Holgura) / 2
  ```

  **No** se crea un `FormulaTextBox` ni ningún control paralelo, y la semántica sigue entera en la sesión pura (P30.8;
  ALT-2).
- **Garantías que se preservan** (P23.2):

  | Garantía | Cómo se preserva |
  |---|---|
  | Teclear no muta el dominio | Teclear solo cambia el borrador: ni intent ni mutación |
  | Borrador ≠ comprometido | El borrador y el estado comprometido siguen separados |
  | Commit explícito | Enter ejecuta parse → bind → validate → evaluate → canonicalizar → intent → mutación atómica |
  | Escape | Vuelve exactamente al estado comprometido |
  | LostFocus | Compromete **solo** si el borrador no cambia la fuente (P23.8) |
  | C4 en dos fases | `TryStage` de **todos** los editores y después `ApplyStaged` de todos; **cualquier** cambio de fuente da `Blocked` con «confirma con Enter» (P23.9) |
  | Reconciliación atómica | Estado aislado, un resolve del rack y persistencia conjunta de authored, `PropertyValues` y efectivo; un fallo deja cero persistencia y cero mutación en memoria (P23.6, P23.16) |

- **Pipeline de commit** (P23.6):
  1. parse;
  2. bind contra el snapshot de la sesión: un nombre exacto y único enlaza, y uno parcial, desconocido, ambiguo o
     reservado se rechaza;
  3. validate, con la comprobación de tipo de una referencia directa;
  4. evaluate sobre `RegistryEvaluation`, solo como aviso temprano;
  5. canonicalizar (D11) y comprometer en la sesión, sin mutar el dominio;
  6. la frontera C4 entrega los estados finales;
  7. el reconciliador re-comprueba el árbol **ya enlazado por ids**, sin volver a resolver nombres, contra el snapshot
     del comando.
- La lista de candidatos **filtra, no resuelve** (*contains* `OrdinalIgnoreCase` sobre el fragmento bajo el cursor);
  muestra nombre, valor evaluado y desambiguador obligatorio; ofrece solo variables compatibles cuya evaluación es
  `Success`; **nunca** auto-selecciona; e inserta la forma del formatter (P23.7, P23.11).
- **Revisión deliberada del contrato del editor de I-48** (P23.12):
  - `=` deja de ser solo una consulta de referencia y abre un **borrador de expresión**: `CASO_6` cambia su clase
    esperada a `DraftExpression` y conserva «queda pendiente y no compromete»;
  - Enter compromete una expresión que pasa el pipeline;
  - `CASO_8` se reescribe como «**Enter resuelve un nombre exacto y único; nunca uno parcial ni ambiguo**»;
  - `=Holgura General + 2` se compromete si enlaza y evalúa;
  - **se conservan** la ausencia de auto-selección, el desambiguador obligatorio, LostFocus sin cambio de fuente, C4
    sin conversión silenciosa, Escape y la regla 20.13 (`CASO_7` y `CASO_9` a `CASO_11` intactos).
- **Transiciones del reconciliador** (P23.15): literal → expresión congela el literal comprometido y escribe la
  entrada; referencia ↔ expresión y expresión → otra expresión conservan el literal congelado; expresión → literal
  escribe el literal y quita la entrada, como hoy desde una referencia; en cualquier caso el rack se resuelve **una**
  vez. La versión se calcula con la autoridad vigente y **nunca** se fija `"2.0"`: preservación de C4-9 (D13).
- **Aislamiento de estado** (P23.16): las transiciones nuevas clonan el authored inicial por su forma persistida
  **antes** de `WithDesign` y de `Apply`. Ningún alias de `PropertyValues` alcanzable por la sesión, la ventana o el
  comando puede quedar mutado tras un preflight o una reconciliación fallidos, y se prueban por separado las dos
  garantías: `no persistence happened` y `no in-memory mutation happened`. `WithDesign` y
  `SelectivePalletDesignDocument.cs` no cambian (ALT-30).
- En RACKEDITAR, cambiar la fuente de una propiedad no toca el registro ni propaga a otros racks: el reconciliador
  resuelve **ese** rack una vez, contra el snapshot del comando y sobre estado aislado, y escribe de forma atómica
  (P21.12). En `RackSelectiveWindow`, `Describe` deja de usar `Text.TrimStart('=')` como nombre de variable y muestra el
  texto de estado de la sesión: un cambio de un miembro (P23.13).
- Sin control ni ventana nuevos: el censo de ventanas no cambia (P23.14).

### D22 — RACKVARIABLES

- **Ni comando ni ventana nuevos**: los censos de comandos y ventanas no cambian (P22.1).
- Evoluciona a (P22.2–P22.3, P22.9):
  - una fila con **nombre**, tipo, **definición** —literal o texto del formatter—, **valor evaluado** o código de
    diagnóstico, consumidores directos y por expresión, y dependientes;
  - un solo campo **«Definición (in)»**: un texto sin `=` es un literal con la regla de ADR-0015 y `> 0`, exactamente
    como hoy; un texto con `=` es una expresión, con unidades si hacen falta, cuya raíz cumple el mismo `> 0`;
  - **diagnósticos estructurados**, en vivo, con posiciones, producidos por una función pura de Application contra el
    snapshot del workspace. La ventana no emite el intent mientras haya errores, **nunca** evalúa con autoridad,
    **nunca** resuelve nombres por sí misma y no referencia tipos del núcleo (P22.4).
- Un selector de referencias inserta la forma del formatter del `VariableId` elegido y **nunca** auto-selecciona por el
  nombre tecleado; el detalle muestra «Depende de» y «La usan» (P22.5–P22.6).
- **Intents con forma enlazada** (P22.7): `Create(nombre, definiciónEnlazada)` y `ChangeDefinition(id,
  definiciónEnlazada)` llevan un literal o un `BoundExpression` con ids, producido contra el snapshot del workspace;
  `Rename`, `Delete`, `UnlinkAllAndDelete` y `RepairBroken` conservan su payload. El preflight valida esos ids contra la
  **misma** lectura, y el commit los vuelve a validar antes de comparar el `PlanReadSet`. **Nunca** se vuelve a resolver
  texto humano contra una lectura posterior.
- **Permite diagnosticar y corregir estados semánticos** de un registro estructuralmente legible y acreditado (§5.3):
  su panel de diagnóstico y reparación, fuera del panel editable, muestra las variables en error con su `RootCauses` completo, los
  miembros y la ruta de cada ciclo, las fuentes de rack afectadas con su fórmula canónica y su causa, la reparación por
  rack con la confirmación consciente de la fuente y el mensaje de recuperación condicional de D18 (P22.9). Con el
  registro estructuralmente ilegible mantiene el bloqueo de hoy, sin escribir.
- La presentación puede listar las cadenas representativas y las razones estructuradas de recuperación de D18;
  `RootCauses` se deriva de `RegistryEvaluation`, no de texto ni de la traza. La capa de texto no crea identidad
  semántica ni reimplementa el recorrido del grafo.
- **No** se implementa UI de Explain ni de Impact Preview (P27.6, P30.4).

### D23 — Propagación

`ChangeDefinition(X, d)` generaliza `ChangeValue` a las cuatro transiciones literal↔expresión (P21.1). Un cambio aguas
arriba sigue **una** sola cadena (P21.2, P21.4–P21.5, P21.11):

```text
registro «después»
→ dependencias / dependientes
→ racks afectados
→ cada rack resuelto UNA vez contra el estado final
→ UN MutationPlan coherente
→ UNA transacción
→ UN commit
→ UN Regen
```

- **Preflight puro, todo o nada** (P21.2): acreditar el registro «antes»; validar la forma enlazada que trae el intent,
  nunca texto; aplicar la mutación a un clon, acreditarlo y evaluarlo; calcular el **cierre**
  `I = {X} ∪ dependientes transitivos de X` en el grafo «después»; aplicar **R2** y el **contrato raíz**
  (casos A–D); descubrir los consumidores de rack de **cualquier** variable de `I` —referencias directas y expresiones
  de propiedad— sobre **una** proyección del barrido, deduplicados por `RackId`; aplicar **R1**, resolviendo cada rack
  una sola vez contra el estado final, con todas sus propiedades, sus expresiones evaluadas y el dominio de cada
  propiedad; formar el `PlanReadSet` (D19); y componer **un** `MutationPlan`: una `RegistryMutation`, en la que solo
  cambia X, y una `RackMutation` por rack, con efectivo completo y todas sus vistas. Si algo falla, el plan queda vacío.
- **Descubrimiento por conjunto** (P21.4): la semántica de la sonda no cambia —tri-estado, `Indeterminate` aborta,
  positivos parciales abortan y se exige autoridad `Single`—; una fuente `expression` estructuralmente legible es
  positiva si sus dependencias cortan `I`, aunque falle al evaluar; un kind desconocido o un payload estructuralmente
  mal formado es `Indeterminate`; y el resultado se calcula una vez sobre el barrido y tiene que ser **equivalente** a
  la unión de descubrimientos por variable, probado como oráculo.
- **Una transacción, un commit, un `Regen`** (P21.5): la estructura del ejecutor no cambia (PREPARE / MUTATE / POST),
  **no** hay regen por variable intermedia y el ejecutor **no** parsea ni evalúa.
- Sin límite de profundidad más allá de la aciclicidad y de D8, y **nunca** K planes sucesivos (P21.11).
- `Create` con expresión sigue siendo registry-only, pero su preflight acredita el registro para validar, evaluar y
  aplicar el contrato raíz (P21.8). Solo materializan un efectivo `UnlinkAllAndDelete`, con referencias directas, y la
  exportación a biblioteca, con el valor evaluado exacto (P21.9).

### D24 — Capacidades futuras: preparadas, no implementadas

- **ID20 — parámetros calculados e integrados** (P25). I-49 entrega `SymbolId`, `SymbolNamespace` (solo
  `projectVariable` activo), `SymbolScope` (solo `Project`), la tabla de símbolos, `ExpressionContext`,
  `SymbolResolver`, el evaluador sobre datos puros y el contexto de evaluación de una propiedad de rack. Quedan
  **reservados sin implementación productiva** `Rack.*`, `Project.*`, el ámbito `Rack` y un caso de definición para
  valores calculados:
  `Rack` y `Project` sin llaves son `ReservedName` y `palabra.` da `UnknownNamespace`, así que ID20 podrá añadirlos sin
  cambiar el significado de ningún texto comprometible ni de nada persistido. Los valores calculados tendrán que venir
  de snapshots puros proyectados desde el barrido del Plugin; el núcleo **nunca** lee AutoCAD, sistemas de Domain ni
  catálogos. Las preguntas de frontera de ID20 son del Owner y de ID20.
- **ID23 — BOM calculado o personalizado** (P26). Puede reutilizar parser, evaluador y contexto **sin depender de
  ProjectVariables** (D1). Añadiría sus namespaces y ámbitos, un resultado entero con semántica de redondeo para
  `Quantity`, la persistencia de fórmulas de BOM y su propio ADR. Ningún tipo de I-49 lleva conceptos de BOM.
- **ID28 — Explain y ID29 — Impact Preview** (P27). Se conservan **en memoria**, por snapshot: el `BoundExpression` y
  el `SymbolId` de cada referencia, los diagnósticos con sus cadenas representativas y la traza opt-in; y, antes de
  ejecutar, las dependencias, los dependientes inversos, los consumidores de rack, el **conjunto afectado** con el valor
  antes y después de cada variable, y el **conjunto leído**: las observaciones del `PlanReadSet` con su fase y su
  resultado esperado, y las decisiones de reparación con su razón. La cadena de Explain se deriva sin re-evaluar y
  **nada** se persiste. `RootCauses` completo se deriva de la misma `RegistryEvaluation`, sin re-evaluar, y no forma
  parte de la traza. Las cadenas se referencian, sin copiar. La traza sigue opt-in, acotada y desactivable sin
  alterar resultados; nunca se compara. Las razones de recuperación pueden presentarse sin hacer del texto identidad.
- Estas costuras se apoyan en el núcleo neutral de D1: ID20 añadiría sus símbolos en el contexto de evaluación de una
  propiedad de rack (P25.5), ID23 reutiliza el núcleo sin depender de Project Variables (P26.1–P26.2), e ID28/ID29 usan
  lo que el núcleo, `RegistryEvaluation` y el preflight conservan en memoria (P27).

### D25 — Fuera de alcance

- **ID20 productivo**: símbolos `Rack.*` y `Project.*`, parámetros calculados y ámbito `Rack`; solo quedan reservados
  (P30.2; contrato §4).
- **Custom BOM / Calculated BOM productivo** (ID23), cantidades enteras y fórmulas de BOM (P30.3; contrato §4).
- **Referencias Rack→Rack** (ID21): `RackPropertyReference` y `rackProperty` siguen prohibidos (P30.1; contrato §4).
- **`IF`, booleanos, `AND`, `OR` y comparadores**, además de lookup, arrays, strings, trigonometría y macros; y
  cualquier función distinta de `MIN`, `MAX` y `ABS`, con `ROUND`, `CEILING` y `FLOOR` diferidas (P30.7).
- Comandos o ventanas nuevos, **otros sistemas de rack** (Dinámico, Push Back, Cama, Cantilever) y **nuevas
  propiedades vinculables** (P30.15); tampoco sistemas de rack nuevos.
- UI de ID28 Explain e ID29 Impact Preview (P30.4).
- Sistema de dimensiones físicas, comprobador dimensional, tipos de área, volumen, masa o ángulo, nuevos `VariableType`
  y variables de usuario escalares o con signo (P30.5).
- Unidades distintas de `[mm]`, `[in]` y `[ft]`; unidades sobre referencias, llamadas o paréntesis; sintaxis de
  pies-pulgadas o de fracciones; unidades en literales sin `=`; y conversión del DWG (P30.6).
- Un `FormulaTextBox` o control paralelo (P30.8); parsers o evaluadores externos y dependencias NuGet (P30.9).
- Grafo, valores evaluados, trazas, conjuntos o texto tecleado persistidos (P30.10); sintaxis localizada y completar
  texto sin selección explícita o nombre exacto (P30.11).
- Control de concurrencia más allá del `PlanReadSet` acotado de D19 (P30.12), y `PlanReadSet` en el reconciliador de
  RACKEDITAR (P30.25).
- Transferencia o fusión de variables entre dibujos (P30.13; decisiones de I-47, C2-8); política de unicidad o de
  mayúsculas de nombres, y renombrado de variables existentes (P30.14); herramientas de degradación o exportación a
  versiones anteriores (P30.16).
- Corregir los hallazgos laterales L1–L5, salvo la preservación de invariantes en las ramas que I-49 reescribe: L2 en
  el reconciliador y la tabla explícita del token `Type` (P30.17); hacer converger los parsers de literales (P30.18).
- Cambiar invariantes de I-47/I-48 más allá de lo que enumera V6 §2.3 —recogido en D2, D12, D19, D20 y D21—, que es
  condición de parada y no alcance (P30.19); una operación o comando de reparación nuevos (P30.20);
  convertir de forma implícita una fórmula en literal (P30.21); relajar la validez de una variable definida por
  expresión respecto de la misma variable literal (P30.22); centralizar constantes de dominio que valen 12 (P30.23);
  relajar R1 o la decisión C4-13 de I-47 (P30.24).
- Abortar un commit porque un símbolo observado falle con el mismo resultado que el preflight admitió, esperar un valor
  «después» de un símbolo que la mutación elimina, observar valores que el preflight no usó o comparar texto de
  diagnósticos; y, en `RepairBrokenRack`, observar por separado cada valor intermedio de una fuente cuya razón reparable
  no cambió, convertir un id ausente en símbolo o comparar la razón de reparación por su texto (P30.26).
- El alcance de I-50, cotas independientes por vista (contrato §4).
- **Exclusiones explícitas añadidas por el Coordinador en G3B.1**, que V6 no contempla en ninguno de sus 30 puntos:
  tablas de diseño y plantillas (*design tables / templates*), *Golden DWG*, optimización de CI y costeo.

## Alternativas consideradas

Proceden de V6 §10.1, que conserva el detalle de cada una, y de los puntos de V6 citados en la última columna cuando la
alternativa no tiene número propio.

| Alternativa | Decisión y motivo | V6 |
|---|---|---|
| **Texto de fórmula como autoridad**: persistir el texto con nombres, o texto canónico con ids en una cadena | Rechazada: un rename rompería las expresiones y el nombre pasaría a ser autoridad; una cadena acoplaría la persistencia a la sintaxis y exigiría un parser en cada lectura | ALT-5, ALT-6 |
| **Persistir el grafo de dependencias** | Rechazada: es derivable, y persistido podría divergir del registro | ALT-8 |
| **Usar `Name` como identidad**, enlazar contra el registro re-leído o aceptar un fragmento de GUID o `#<GUID>` sin nombre como entrada | Rechazadas: el nombre cruzaría fronteras entre lecturas; un fragmento colisiona; `#<GUID>` es la presentación de un estado roto | ALT-23, ALT-28, ALT-29 |
| **Motor dimensional obligatorio** (`Length \| Scalar`, `DimensionMismatch`) o `VariableType` como álgebra dentro del motor | Rechazadas por requisito del Owner: el motor es adimensional | ALT-9, ALT-22 |
| **Parser o evaluador externo** (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) | Rechazada: ADR-0012; semántica sensible a cultura o no acotada | ALT-14 |
| **Eliminar las referencias directas y migrar todo a `Expression`**, o una fuente solo `Expression` sin canonicalizar `=X` | Rechazadas: la referencia directa no se elimina y no hay migración ni conversión automática; sin canonicalizar, `=X` tendría dos representaciones persistidas | P24.2, P29.2; ALT-4 |
| **Subir MINOR (V-1) o MAJOR (V-2)** por la presencia de expresiones | Rechazadas: un minor no cambia el comportamiento de ningún lector y solo añade una etiqueta *sticky*; un major excluiría para siempre a las builds anteriores sin evitar ninguna lectura errónea (D13) | P18.4–P18.5; ALT-13 |
| **Convertir fórmulas en literales al borrar**, materializar fórmulas en `UnlinkAllAndDelete` o borrar en cascada | Rechazadas: reescriben en silencio definiciones y fórmulas del usuario y cortan dependencias que nadie pidió tocar | ALT-16, ALT-20 |
| **Concurrencia optimista global** —hash de snapshot, token global, comparación completa del registro o locks—, presentar el `PlanReadSet` como compatible con una V8-R05 intacta o volver a ejecutar el preflight en commit | Rechazadas: la revisión de V8-R05 queda acotada al `PlanReadSet` y tiene que declararse | ALT-35, ALT-36, ALT-40 |
| **Observar transitivamente todo el grafo** aunque el plan no lo use, meter todo `I` en las dos fases u observar cada valor de las dependencias de una fuente que se retira | Rechazadas: crean lecturas que el preflight no usó y abortan por cambios que no alteran nada de lo decidido o escrito | ALT-38, ALT-41, ALT-42 |
| **`PlanReadSet` como lista plana** con aborto si cualquier símbolo falla, o comparar el texto de los diagnósticos | Rechazadas: contradice R2 y haría abortar siempre `Delete`, `UnlinkAllAndDelete` y el `Rename` de una variable en error; el texto localizado no es estable | ALT-37, ALT-39 |
| **Dejar fuera de alcance la premisa de la reparación**, modelar un id ausente como símbolo o re-ejecutar `RepairBrokenRack` completo en commit | Rechazadas: el commit podría borrar una fórmula ya válida, se inventaría un símbolo o se re-ejecutaría el preflight | ALT-43, ALT-44, ALT-45 |
| **Expresiones solo en `ProjectVariable.Definition`**, un `FormulaTextBox` o cada expresión de propiedad como variable implícita | Rechazadas: contradicen el requisito del Owner de dos superficies y del mismo control, o llenan el registro de variables que nadie creó | ALT-1, ALT-2, ALT-3 |
| **Cachear el valor evaluado** | Rechazada: segunda autoridad que puede quedar obsoleta | ALT-7 |
| **Conversión de unidades vía `StructuralSectionUnits`**, un parser de unidades por capa o centralizar todas las constantes que valen 12 | Rechazadas: dependencia invertida, varias autoridades, o constantes de dominio tratadas como conversiones | ALT-10, ALT-11, ALT-31 |
| **`ROUND`, `CEILING` y `FLOOR` ya en I-49** | Diferidas: semántica abierta | ALT-12 |
| **Núcleo dentro de `ProjectVariables`** o en un ensamblado separado | Rechazada la primera: arrastraría esa dependencia a ID23. Diferida la segunda: añade un proyecto a builds, CI y despliegue | ALT-15, ALT-17 |
| **Gramática localizada** (`,` decimal y `;` separador) | Rechazada: doble significado de la coma | ALT-18 |
| **`FatalUnevaluable`** no reparable, una operación nueva de reparación o un aviso de recuperación incondicional | Rechazadas: dejan un rack sin salida, duplican `RepairBrokenRack` o prometen lo que R1 puede impedir | ALT-19, ALT-27, ALT-33 |
| **Sin contrato raíz** para variables definidas por expresión, o **relajar R1** | Rechazadas: la validez dependería de la sintaxis; se reabriría la decisión C4-13 de I-47 | ALT-21, ALT-34 |
| **Comparar en commit solo los valores de `I`** | Rechazada: no cubre lo que el plan leyó | ALT-24 |
| **Forma no canónica como error estructural** | Rechazada: bloquearía todo el dibujo por un estado que la build entiende | ALT-25 |
| **Eliminar `ExtensionData` de forma global** | Rechazada: rompería la preservación histórica fuera de los payloads de Expression | ALT-26 |
| **Arreglar `WithDesign` en el portador** | Rechazada en I-49: el aislamiento se consigue en la frontera de I-49 | ALT-30 |
| **Limitar el anidamiento por paréntesis o por texto** en lugar de la profundidad del `BoundExpression` | Rechazada: no cuenta las cadenas binarias | ALT-32 |
| **Límite contractual de longitud de nombre** (`MaxNameLength`), para que quepa una guarda de caracteres | Rechazada: rompe el contrato histórico y los dibujos existentes, y no es necesario | A1 §9 |
| **Guarda de caracteres relativa al snapshot** | Rechazada: el parser deja de ser neutral e independiente del contexto, y perjudica a ID23 | A1 §9 |
| **Eliminar toda guarda de complejidad del parser**, o fijar la de caracteres por encima de cualquier cadena posible | Rechazada como primera opción: la guarda de tokens conserva la protección del parser | A1 §9 |
| **Debilitar P2.5** («si cabe en la guarda», «para nombres razonables», «salvo textos muy largos») | Rechazada: rompe el round-trip canónico y la editabilidad | A1 §7 y A1 §9 |
| **Permitir que `#<GUID>` sin nombre comprometa una identidad presente** | Rechazada: reabre OPEN A innecesariamente | A1 §9 |
| **Subir la guarda de caracteres a otro valor finito** | Imposible: el contraejemplo `L + 1` existe para cualquier `L` | A1 §2.1 y A1 §9 |

Las seis últimas filas proceden del Amendment A1 y conservan su motivo; en ellas, la última columna remite a A1 y no a
V6.

**Alternativas del Amendment A2.** Proceden de A2 §8 y conservan su texto y su motivo; la única referencia interna de
A2 que contienen se escribe aquí `A2 §2.1`.

| Opción | Qué haría | Por qué no |
|---|---|---|
| **A** — exigir forma D persistida | Solo ids D desde ahora y rechazo de los no D existentes | **Rechazada**: rompe dibujos que hoy se leen y acreditan, y la identidad vigente; cambia la regla vigente de lectura de `VariableId` (P3, Discovery §4.1) en la que se apoyan P3.2 y P8.1; con migración reescribiría ids persistidos |
| **B** — `SymbolId` canónico en forma D | Normalizar la clave al valor GUID | **Rechazada**: colapsa identidades textuales (R2 y la familia de seis) y deja dos semánticas de identidad: texto en el registro y en las referencias directas, GUID en las expresiones |
| **C** — igualdad por valor de GUID | Cambiar la igualdad de `VariableId` | **Rechazada**: cambia la igualdad vigente de `VariableId` en la que se apoyan P3.2 y P8.1, y con ella la identidad estable de ADR-0034 §2; R2 pasaría a `AmbiguousIdentity` en dibujos hoy utilizables, y referencias escritas con otra disposición cambiarían de resolución |
| **D1** — gramática cruda de `Guid.TryParse` tras `#` | Aceptar el texto crudo del id después de `#` | **Rechazada**: duplica dentro del lexer la gramática de compatibilidad de .NET y choca con las reglas de coma, espacios, llaves, paréntesis y `+` (P1.5, P1.7); medido hoy, una X cruda dentro de una expresión da `InvalidQualifier` y `UnterminatedName` |
| **D2** — etiquetas finitas de disposición | Disposición + valor GUID | **Rechazada**: no es sin pérdida; un mismo GUID admite varias identidades de la misma disposición (A2 §2.1) |
| **D3** — clave exacta entre llaves para **todos** los ids | `#{…}` también para las claves D | **Viable, pero rechazada** frente a E1: sustituye sin necesidad la forma corta D aceptada en OPEN A, que es la que emiten todos los ids acuñados hoy |
| **E2** — clave exacta codificada | Forma corta D + un códec ASCII para el resto | **Viable, pero rechazada**: superficie normativa mayor y opaca sin necesidad |
| **Ids sustitutos o alias** | Claves inventadas por snapshot, o alias persistidos | **Rechazada**: identidad nueva en los dos casos; las claves por snapshot son inestables entre lecturas, y los alias persistidos añaden un segundo índice de identidad que habría que mantener |
| **GUID como cualificador y clave exacta solo ante colisión** | Forma D por valor y llaves solo si otra entrada comparte GUID | **Rechazada**: enlazar por GUID no es la igualdad de `VariableId`, y el texto mostrado dependería de otras entradas del snapshot, así que un `Create` ajeno cambiaría a qué enlaza un texto |

## Consecuencias

### Positivas

- **Identidad rename-safe**: lo persistido son ids, así que renombrar nunca rompe una expresión (D5).
- **Núcleo reutilizable**: parser, binder, evaluador, grafo, formatter y diagnósticos sin dependencia de Project
  Variables, listos para ID20 e ID23 (D1, D24).
- **Grafo no duplicado**: una sola función de dependencias y un solo grafo derivado por snapshot, sin copia persistida
  (D15).
- **Fórmulas en variables y en propiedades vinculables**, con el mismo control y una sola representación por significado
  (D10, D11, D21).
- **Atomicidad**: un plan, una transacción, un commit y un `Regen`, sin efectos parciales ni mutación en memoria de
  quien llama (D19, D21, D23).
- **Fail-closed tipado**: lo estructural no se lee ni se repara; lo semántico se diagnostica sin fallback; las builds
  anteriores fallan cerradas sin leer mal (D13, D14).
- **Base para ID20, ID23, ID28 e ID29** sin implementarlas (D24).
- **Un único dueño de los valores** evita que UI, targets o BOM evalúen por su cuenta (D16).
- **El formatter canónico siempre alimenta al parser** para cualquier árbol legal: ninguna guarda del parser rechaza su
  texto, y P2.5 se sostiene sin condiciones añadidas (D8; Amendment A1).
- **Ningún fallo artificial por nombres largos legales**: la longitud de un nombre no invalida una fórmula (D8;
  Amendment A1).
- **El parser conserva guardas estructurales de recurso**: número de tokens y anidamiento sintáctico (D8; Amendment A1).
- **La identidad textual de `VariableId` se conserva**: la clave de `projectVariable` sigue siendo el texto, con
  `OrdinalIgnoreCase`, sin migración, sin normalizar lo persistido y sin rechazar ningún id que hoy se lee (D5;
  Amendment A2).
- **El formatter y el binder no pierden información para ninguna identidad vigente**: `Q(clave)` es inyectivo, R1 y R2
  tienen texto de ida y vuelta, y P2.5 vuelve a sostenerse sin condiciones también para las claves N, B, P y X y las
  disposiciones de compatibilidad (D6, D7; Amendment A2).
- **Los ids que se acuñan hoy conservan la forma corta** `#<d>` de OPEN A (D6; Amendment A2).

### Costes que no se minimizan

- **El AST persistido amplía el formato** del registro y de `PropertyValues` con un payload de mundo cerrado y sus
  tablas de tokens (D9–D11).
- **Las builds anteriores bloquean las fórmulas nuevas**: un registro con una definición `expression` queda ilegible
  entero, y un rack con una fuente `expression` bloquea en todo el dibujo las operaciones de variables que necesitan
  descubrimiento. El mensaje de esas builds no dice «versión más nueva» (D13; P18.2, P18.9).
- **Enlazar por nombre exige binder y formatter**: nace la primera ruta productiva nombre→id, en dos superficies y con
  una sintaxis de identidad (D5, D6).
- **El `PlanReadSet` aumenta la complejidad del commit**: compara observaciones con hasta dos evaluaciones, también en
  planes sin `RegistryMutation`, y en la reparación recomputa la inspección de cada fuente que retira. Eso **revisa
  V8-R05** (D19, D20).
- **La reparación necesita diagnósticos más ricos**: el panel de RACKVARIABLES y la confirmación crecen, con un mensaje
  de recuperación que exige clasificar las fuentes de los racks del cierre (D18, D22).
- **La UI de I-48 cambia deliberadamente** para aceptar expresiones: `CASO_6` y `CASO_8` cambian, con las demás
  garantías preservadas (D21).
- **Más casos de prueba y de compatibilidad**: evolucionan guardas y pines vigentes, nacen guardas nuevas, y la
  compatibilidad se verifica con la build anterior sobre dibujos sin expresiones, con expresiones en el registro y con
  expresiones de propiedad (P28.4, P29.5).
- Con R1 intacto, un rack con dos causas raíz independientes solo sale por la reparación (D18).
- Una referencia directa persistida a una variable negativa, alcanzable solo por edición externa, deja de resolver
  (D2).
- `RackSelectiveWindow`, un archivo caliente, necesita un cambio de un miembro (D21).
- La propagación sobre dibujos grandes sigue **sin medir** hasta G11 (P12.6, R5).
- **El parser ya no rechaza por número de caracteres** un token único enorme: el lexing sigue siendo
  `O(longitud de la entrada)` (D8; Amendment A1, A1 §5).
- **G6 sustituye la guarda `TextLength` de G5 por la guarda de tokens**: la implementación histórica de G5 evoluciona y
  no se revierte (A1 §10).
- **La sintaxis gana una segunda forma de cualificador**, `#{<clave exacta>}`, con su escape `}}` y sus reglas léxicas
  (D7; Amendment A2).
- **El candidato de G6 necesita una corrección acotada antes de cerrarse**: la validación de clave de `SymbolId`, el
  texto exacto de la clave en la sintaxis del cualificador, el lexer y el parser, el formatter y el binder, sin uso
  semántico del `Guid` parseado; una prueba de G6 y una fila de prueba de G5 quedan superadas, y la matriz A–M de
  A2 §9.3 es obligatoria (A2 §9).

### Requisito de conformidad con I-50 (composición, no semántica nueva)

I-50 está integrada en `main`: [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md) persiste la política de cotas
como `DimensionViews` en el diseño del Selectivo, junto a `PropertyValues`. I-49 integra en segundo lugar, así que su
implementación **tiene que demostrar** que un Selectivo que tenga **a la vez**:

- una fuente `expression` en `PropertyValues`, y
- `DimensionViews` presente,

sobrevive a **RACKEDITAR** y a **guardar y reabrir** conservando **ambos** conceptos. Es una prueba de composición entre
dos decisiones ortogonales —la fuente de una propiedad (este ADR) y la visibilidad de cotas por tipo de vista
(ADR-0035)— y **no** introduce semántica nueva. V6 la registra como prueba cruzada en §12.1, y G3B.1 la precisa con
guardar y reabrir. La matriz de pruebas P28 de V6 no la enumera, así que se incorpora como **requisito de evidencia de
implementación** sin modificar V6.

### Qué vigilar

- `RackSelectiveWindow.xaml.cs`: el cambio de I-49 (D21) se serializa con los cambios de otras iniciativas sobre ese
  archivo, con la regla de archivo caliente de WORKFLOW §7 y el procedimiento de colisión de
  [`decisions/I-49.md`](../automation/decisions/I-49.md) §8.
- `RackDuplicationPlan` y el restamp: la prueba de supervivencia de `expression` en la duplicación se escribe contra la
  versión vigente en `main` (P24.8; V6 §12.2).
- Las guardas de censo de comandos y ventanas se escriben contra el censo vigente al rebasar: I-49 no añade comandos ni
  ventanas, y otra iniciativa puede cambiar ese censo antes (P22.1; V6 §12.3).
- Si una evolución de guarda debilitara la protección de ID21, o un gate necesitara cambiar una invariante de I-47/I-48
  más allá de lo que enumera V6 §2.3 —recogido en D2, D12, D19, D20 y D21—, la implementación se **detiene** (V6 §8.2,
  §13).
- La inyectividad del cualificador descansa en la premisa de A2 §4 —todo carácter de una clave admisible es ASCII o
  espacio en blanco Unicode—, que depende de lo que acepta `System.Guid.TryParse`; la prueba G de A2 §9.3 la exige.

### Efectos de A3: causas completas y recuperación demostrable

**Positivos.** La cadena representativa no oculta raíces: `RootCauses` conserva todas, incluidas las externas que
entran por otro miembro de una SCC. La concurrencia detecta cambios materiales de razón por variable leída. El
`CycleRoot` trata el ciclo como unidad sin perder sus fallos externos ni los propios de sus miembros. Varias firmas
del mismo dueño cuentan como una corrección; las sugerencias solo se ofrecen cuando el descubrimiento y los fallos
conocidos las respaldan. No se enumeran caminos exponencialmente.

**Costes.** Aumentan los metadatos estructurados de resultados y la derivación por SCC. Las listas de miembros y colas
se comparten; las consultas repetidas a raíces pueden requerir memoización por snapshot, con los costes de D15.
El descubrimiento puede impedir una sugerencia aunque la recuperación fuese posible; una SCC no simple pierde la
promesa de recuperación con un miembro, sin declararla imposible. Crecen las matrices futuras de G7, G9 y G10;
G8 conserva las obligaciones de persistencia y `InspectBinding`. Ninguna se da por ejecutada en esta propuesta.

**Fronteras de entrega (A3 §6.6 y §11).** G7 añade al núcleo diagnósticos, cadenas, firmas, raíces, determinismo y
predicado de ciclo simple. G8 cubre persistencia e inspección. G9 compone descubrimiento, recuperación y observaciones;
G10 presenta los textos. No se trasladan `SourceRoots`, `RecoveryUnits`, razones, planes ni consumidores al núcleo
G7. No se amplían sistemas, propiedades, lenguaje, BOM ni las exclusiones de D25.

## Relación con otros ADR

- **Sucesor completo propuesto** de [ADR-0041](0041-motor-expresiones-parametricas-identidad-textual-y-cualificador-clave-exacta.md):
  lo reemplazará entero únicamente si el Owner acepta ADR-0043. Mientras tanto, ADR-0041 sigue aceptado, vigente e
  intacto. Se conservan sus decisiones no afectadas y se incorporan A3 y los cambios materiales del preámbulo.
- [ADR-0040](0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md) ya está reemplazado por
  ADR-0041, y [ADR-0038](0038-motor-expresiones-parametricas-y-edicion-formula-aware.md), por ADR-0040. Son historial;
  este ADR no los reabre ni los modifica.
- **Extiende** [ADR-0034](0034-project-variables-autoridad-drawing-level.md), que queda inmutable y no se edita (D12).
- **No sustituye** a:
  - [ADR-0005](0005-estrategia-de-unidades.md): la pulgada sigue siendo la unidad interna y el motor no convierte el DWG
    (D3);
  - [ADR-0006](0006-autocad-solo-en-plugin.md): el Plugin sigue siendo el único dueño del NOD, y la lógica vive en
    Application (D10, D19);
  - [ADR-0012](0012-producto-sin-dependencias-nuget.md): parser y evaluador propios, sin dependencias (D7);
  - [ADR-0015](0015-entrada-numerica-localizada.md): los literales conservan su regla; la gramática invariante es una
    excepción acotada al interior de las expresiones (D7);
  - [ADR-0021](0021-identidad-unidades-y-presentacion-de-secciones.md): `StructuralSectionUnits` conserva su API y solo
    declara alias de la autoridad de unidades (D3);
  - [ADR-0025](0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md): la notación de pendiente por 12 sigue local (D3).
- **Se compone** con [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md) sin cambiarlo (requisito de
  conformidad).

## Referencias

- **ADR-0041 aceptado** — [base completa](0041-motor-expresiones-parametricas-identidad-textual-y-cualificador-clave-exacta.md),
  blob `c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231`: aceptado y vigente mientras ADR-0043 siga propuesto.
- **Amendment A3-R2** — [contrato exacto](../initiatives/I-49-proposal-v6-amendment-a3-multi-cause-dependency-failures.md),
  blob `4da6ef3caa21dcf31140983c6db7e23f02aa3e18`, versionado en `5210c1c0534d3cf5bee4d23526f24e062bdc77c1`;
  consenso exacto en `addb5223265612785dfe19ef1f353a19307f0a83`, [registro §16](../automation/decisions/I-49.md).
  Apoyo: §§5–7 (reglas), §8 (compatibilidad limitada), §10.3 (mapa de este sucesor), §11 (pruebas futuras),
  §12 (necesidad del sucesor) y §13 (nuevo freeze pendiente).
- **Freeze V6 + A1 + A2** — [histórico](../initiatives/I-49-consensus-freeze-v6-a1-a2.md), blob
  `57736f725663aab79a24b29685ed34e8c3e9ebad`: intacto; A3 invalida su autorización para continuar G7.

- **Proposal V6 de I-49** — [`I-49-proposal-v6.md`](../initiatives/I-49-proposal-v6.md). Contrato técnico consensuado:
  blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`; commit histórico `048a508e570e210467c3693a11ac4a487f383944`; commit
  operativo tras el rebase de G3A `1ed93a0525ec11c5092df89c55cbf498c99f8e2a`. `Coordinator = AGREED WITH V6`,
  `Architect = AGREED WITH V6`. Secciones de apoyo directo: DR-1 a DR-8 (§2.1), §2.3, P1–P30, §4 (OPEN A), §5 (OPEN B),
  §6 (V-0), §7 (veredictos de operación), §9 (contenido exigido a este ADR), §10.1 (alternativas) y §12 (coordinación).
  Tras el rebase de A1-R1, V6 está en `8189c7df30ae6c3faaf4138bef02a4161176a2c1`, con el mismo blob; el tag
  `archive/i-49-a1-pre-rebase-71268eb` conserva los SHAs de la rama anteriores a A1-R1, como `1ed93a0`, y
  `048a508` es anterior al rebase de G3A.
  Tras la reconciliación de A1-R3, V6 está en `cd8588017eca13bd14b2724477d6014211b85216`, y tras el rebase de A2-R1, en
  `5cad0d54bf1643f0219fcc26eac58e36f366ae98`, con el mismo blob; los tags `archive/i-49-a1r3-pre-rebase-2eeeab1` y
  `archive/i-49-a2-pre-rebase-5a3714f` conservan la rama anterior a cada una de esas reconciliaciones.
- **Amendment A1 de I-49** —
  [`I-49-proposal-v6-amendment-a1-text-guard.md`](../initiatives/I-49-proposal-v6-amendment-a1-text-guard.md). Enmienda
  la guarda de recurso de P1.9/D8: blob `d62019088b9e7a140d5066799afe6ace6db303ba`; commit revisado
  `71268eb09a3e04231da4e25b985e175c977f8c9e`; commit tras el rebase de A1-R1
  `cae7a9f5ae10e9c86c16a29000c0933e2187873f`. `Coordinator = AGREED WITH A1`, `Architect = AGREED WITH A1`, con registro
  durable en [`decisions/I-49.md`](../automation/decisions/I-49.md) §12. Secciones de apoyo directo: A1 §2 (hallazgo),
  A1 §3 (A1.1 a A1.5 y lectura conjunta), A1 §4 (cota del formatter), A1 §5 (recursos y seguridad), A1 §6
  (diagnósticos), A1 §7 (P2.5), A1 §8 (nombres), A1 §9 (opciones rechazadas), A1 §10 (impacto sobre G5), A1 §11 (ADR)
  y A1 §12 (freeze).
  Tras la reconciliación de A1-R3, A1 está en `c7fabbef7b4d65728bb7bef33a1b3b87441fddb2`, y tras el rebase de A2-R1, en
  `f69e903a4440e0f3c6e22bdf08217ff02c465b9e`, con el mismo blob.
- **Amendment A2 de I-49** —
  [`I-49-proposal-v6-amendment-a2-exact-key-qualifier.md`](../initiatives/I-49-proposal-v6-amendment-a2-exact-key-qualifier.md).
  Conserva sin cambio la identidad textual de `VariableId` como clave de `projectVariable` y enmienda la validez neutral
  de esa clave y el cualificador de P3.9, D6 y D7: blob `49a925336dd3775929a35b0f40b73cb7f8c487f7`; commit revisado
  `5a3714f06aa695e22db0f16455a1ea8dce5195b6`; commit tras el rebase de A2-R1 `7c1eed47f5ea2cb66b91d782bc05f28ae1c144c7`.
  `Coordinator = AGREED WITH A2 / E1`, `Architect = AGREED WITH A2`, sin hallazgos materiales en la revisión exacta, con
  registro durable en [`decisions/I-49.md`](../automation/decisions/I-49.md) §14. Secciones de apoyo directo: A2 §2
  (hallazgo, R1 y R2), A2 §3 (decisión), A2 §4 (inyectividad), A2 §5 (P2.5), A2 §6 (A1), A2 §7 (cláusulas y lectura
  conjunta), A2 §8 (opciones rechazadas), A2 §9 (impacto sobre G6), A2 §10 (ADR), A2 §11 (freeze) y A2 §12 (evidencia).
- **ADR-0040** — [`0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md`](0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md),
  propuesto en `f8dddb6fc570f8294a8c960a6fe5060df1e94a2a` (blob `dd1ad8018a2b6643bd1972907e0a35cd7d402fd1`) y aceptado
  por el Owner el 2026-09-13 (blob aceptado `9855a6b32a82954839ba2ae67cf351eae118c8ef`): antecedente histórico,
  ya reemplazado por ADR-0041.
- **ADR-0038** — [`0038-motor-expresiones-parametricas-y-edicion-formula-aware.md`](0038-motor-expresiones-parametricas-y-edicion-formula-aware.md),
  aceptado por el Owner el 2026-09-13 (blob aceptado `59b8383be62c91526c0fe435af41121e1fd21429`) y
  `reemplazado por ADR-0040` (blob `ab10130b7ce69dcd15ad196017beb27acbfb4c5a`): historial.
- **Consensus Freeze histórico de I-49** — [`I-49-consensus-freeze.md`](../initiatives/I-49-consensus-freeze.md),
  versionado en `364d6c06e44273a63a7b6f6509daf357611ea77a` (tras el rebase de A1-R1,
  `788b00f1d3e70ad43f0a72ce0b9db7f5f88313db`; blob `49e8041c489d01b5f8dc20b1c7028d46488933ee`): no se reescribe, y A1
  activó su regla de invalidación.
  Tras la reconciliación de A1-R3, el freeze histórico está en `5fa71409756d6cb30cecfea70b85d4c22c6bbf17`, y tras el
  rebase de A2-R1, en `6c5319455c032ac642062223719a507bfdfd134d`, con el mismo blob.
- **Consensus Freeze V6 + A1 de I-49** —
  [`I-49-consensus-freeze-v6-a1.md`](../initiatives/I-49-consensus-freeze-v6-a1.md), versionado en
  `8ed15f782f7fc2878693053e54cedca8a041d247` (tras el rebase de A2-R1, `f95464a360c7f8ba77472ef034470605b606aec6`; blob
  `cb2105770e136b24dd6d096acb49af8f04adf491`): no se reescribe, y el hallazgo que A2 resuelve activó su regla de
  invalidación para seguir implementando.
- **Historial, no contrato**: Proposals [V1](../initiatives/I-49-proposal-v1.md) a
  [V5](../initiatives/I-49-proposal-v5.md), con sus revisiones reconciliadas en §0 de V6.
- **Contrato de I-49** —
  [`I-49-motor-expresiones-parametricas.md`](../initiatives/I-49-motor-expresiones-parametricas.md), §1, §3, §4, §11.2
  y §12.
- **Discovery de I-49** — [`I-49-discovery.md`](../initiatives/I-49-discovery.md).
- **Registro de I-49** — [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md).
- **Proposal V8 de I-48** — [`I-48-proposal-v8.md`](../initiatives/I-48-proposal-v8.md), §6 (`V8-R05`), y
  [decisiones de I-48](../automation/decisions/I-48.md) §4.
- **Decisiones de I-47** — [`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md): C2-8, C4-8, C4-9 y
  C4-13.
- [ADR-0034](0034-project-variables-autoridad-drawing-level.md), [ADR-0005](0005-estrategia-de-unidades.md),
  [ADR-0006](0006-autocad-solo-en-plugin.md), [ADR-0012](0012-producto-sin-dependencias-nuget.md),
  [ADR-0015](0015-entrada-numerica-localizada.md),
  [ADR-0021](0021-identidad-unidades-y-presentacion-de-secciones.md),
  [ADR-0025](0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md) y
  [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md).
