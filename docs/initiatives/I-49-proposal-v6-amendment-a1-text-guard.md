# I-49 — Proposal V6 · Amendment A1: guarda de complejidad del parser (P1.9/D8) y canonicidad (P2.5)

Amendment documental de la [Proposal V6](I-49-proposal-v6.md) para **una sola** incompatibilidad contractual: la guarda
**finita de caracteres** del parser (P1.9; [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md)
D8) frente a la **canonicidad universal** `Canonicalize(Bind(Parse(Format(b)))) == b` (P2.5; ADR-0038 D4). Se detectó en
la compuerta G6.0, antes de escribir producción de G6.

```text
Amendment          = A1
Finding            = P1.9/P2.5 CONTRACT TENSION

Enmienda a         = Proposal V6, blob ef4db3aa400483ff25a8f39b2beb93708fa43d1a (no se edita)
Decisión afectada  = ADR-0038 D8: fila «Texto de entrada» y la frase que nombra esa guarda
                     ADR aceptado e inmutable, blob 59b8383be62c91526c0fe435af41121e1fd21429
Freeze afectado    = Consensus Freeze 364d6c06e44273a63a7b6f6509daf357611ea77a
                     blob 49e8041c489d01b5f8dc20b1c7028d46488933ee (no se reescribe)
Base               = c4880af66586c535954b0ba0bff7b64b887bddb4 (G5 CLOSED)
Alcance            = SOLO la guarda de recurso del parser de P1.9/D8
```

---

## 1. Qué es y qué no es

- **Es** un amendment del Coordinador (`Coordinator = AGREED WITH A1`) sometido a la revisión del Arquitecto. Repara
  **una** incompatibilidad y nada más.
- **No** edita V6, ADR-0038 ni el freeze de `364d6c0`. V6 sigue siendo el contrato técnico en su blob exacto. A1
  sustituye los textos que enumera §3.3 y añade las reglas de la guarda de §3.2 a §7; solo rige cuando exista el nuevo
  freeze de §12.
- **No** autoriza implementación: G5 sigue CLOSED en `c4880af` y G6 sigue BLOCKED.
- **Activa** la regla de invalidación del freeze (§3 de ese registro): cambia D1–D25 (D8) y una decisión cerrada de V6
  (§0.11, «Texto de entrada ≥ 4000»).
- Como en V6, los nombres de tipos y miembros son **ilustrativos**: el contrato es el comportamiento.

---

## 2. Hallazgo vinculante

### 2.1 Contraejemplo matemático

Para **cualquier** límite finito de caracteres `L`:

1. **Nombre legal.** Existe una Project Variable legal cuyo nombre son `L + 1` letras `A`:
   - `ProjectVariable.Create` solo rechaza un nombre en blanco
     (`src/RackCad.Application/ProjectVariables/ProjectVariable.cs:69-74`);
   - el store solo declara ilegible una entrada sin nombre
     (`src/RackCad.Application/Persistence/ProjectVariablesStore.cs:177-180`);
   - RACKVARIABLES recorta espacios sin limitar la longitud (`src/RackCad.UI/RackProjectVariablesWindow.xaml:66`;
     `src/RackCad.UI/RackProjectVariablesWindow.xaml.cs:143,155`).

   Citas en `c4880af`; esos archivos siguen idénticos en `main` `1b091be`.
2. **Árbol legal.** `b = Reference(id)`: 1 nodo, profundidad 1, 0 argumentos, id presente. Es canónico y no tiene
   referencias rotas.
3. **El formatter tiene que emitir el nombre completo.** Es la forma de P4.2 para un nombre único y seguro, y **ningún
   texto más corto enlaza con `b`**:
   - con cualificador, el id manda y el nombre se valida contra el actual (`QualifiedNameMismatch`; §4.2, regla 2);
   - `#<id>` sin nombre, con el id presente, da `NameRequired` y no es comprometible (P7.1; §4.2, regla 11);
   - un nombre parcial nunca enlaza (`UnknownSymbol`; §4.2, regla 5), y la comparación es la igualdad exacta
     `OrdinalIgnoreCase`, sin recortes ni normalización (P7.2).
4. **El parser lo rechaza.** `Format(b)` mide `L + 1` caracteres, así que `Parse(Format(b))` da
   `LimitExceeded(TextLength)` y `Bind` no llega a correr.
5. **Conclusión.** `Canonicalize(Bind(Parse(Format(b)))) == b` es imposible con cualquier guarda finita de caracteres.

Variantes del mismo hecho:
- `NombreLargo + 1`, que es `Expression` en las dos superficies (P2.8);
- un nombre de `L - 36` caracteres que cabe mientras es único y deja de poder escribirse en cuanto aparece un homónimo,
  porque su forma cualificada mide `L + 1` (P4.2, P4.7).

V6 no lo resolvía:
- P1.9 reconcilia con P2.5 **solo** el anidamiento sintáctico: «nunca es menor que 24, para que el texto canónico de
  cualquier árbol legal vuelva a parsear». Para la longitud no hay reconciliación: su fila es «≤ 4000 caracteres (nunca
  menos)», y §0.11 da por cerrada la decisión «Texto de entrada ≥ 4000».
- Subir `L` a otro valor finito reproduce el contraejemplo con `L + 1`.
- Una guarda de caracteres mayor que cualquier cadena posible nunca dispararía: sería eliminarla (§9).

### 2.2 Reproducciones medidas

Son **evidencia**, no la demostración. Muestran que la guarda de 4000 rechaza textos canónicos de árboles legales y que
una guarda finita mayor resuelve las dimensiones acotadas: estructura, números y cualificadores. La imposibilidad la da
§2.1.

Se midieron con pruebas de la compuerta G6.0 sobre la producción intacta de `c4880af`:
- **Construcción.** Aún no existen formatter ni binder, así que cada caso escribe a mano el texto que fijan P4.2 y P1.4
  y lo entrega a `ExpressionParser.Parse`: números en su forma más corta que hace round-trip y sin exponente, unidad
  pegada, `MIN(a, b)`, un espacio a cada lado del operador y la forma mínima inequívoca de cada referencia.
- **Sin versionar.** Por orden de G6, el RED no se versionó; los parámetros de esta tabla bastan para reconstruirlo.
- **Notación.** `ε` es `double.Epsilon` escrito sin exponente (326 caracteres).

| Caso | Árbol legal | Caracteres | Guarda 4000 | Guarda 1000000 (temporal) |
|---|---|---|---|---|
| A | `MIN` de 15 `MIN` de 16 `Holgura#<id>` (hay dos variables `Holgura`); 256 nodos, profundidad 3, 16 argumentos | 11118 | `LimitExceeded` en 4000, longitud 7118 | Parsea |
| B, nombre seguro | `A` × (L + 1); 1 nodo | L + 1 | `LimitExceeded` en 4000, longitud 1 | `LimitExceeded` en 1000000, longitud 1 |
| B, llaves | `}` × (L/2 + 1), entre llaves y con cada `}` escrito `}}`; 1 nodo | L + 4 | `LimitExceeded`, longitud 4 | `LimitExceeded`, longitud 4 |
| B, homónimo | `A` × (L - 36) + `#<id>`; 1 nodo | L + 1 | `LimitExceeded`, longitud 1 | `LimitExceeded`, longitud 1 |
| C, suma | 24 × `ε`; 47 nodos, profundidad 24 | 7893 | `LimitExceeded`, longitud 3893 | Parsea |
| C, `MIN` | `MIN` de 15 `MIN` de 16 `ε`; 256 nodos, profundidad 3 | 78798 | `LimitExceeded`, longitud 74798 | Parsea |
| D | `MAX` de 16 argumentos: una cadena de 23 `{a}}b}#<id>` y 15 `MIN` de 13 `ε[mm]`; 256 nodos, profundidad 24, 16 argumentos | 65875 | `LimitExceeded`, longitud 61875 | Parsea |

- **Guarda 4000.** 14 pruebas seleccionadas: 7 fallan, todas por aserción y todas con `LimitExceeded(TextLength)`, y 7
  controles pasan.
  - Seis doubles extremos, cada uno solo, parsean con los mismos bits: `double.MaxValue` en 309 caracteres,
    `double.Epsilon` y el mínimo normal en 326, `1E-300` en 302, `1E+20` en 21 y `1E-05` escrito `0.00001`.
  - Las tres formas de referencia del oráculo las lee así el parser de G5.
- **Guarda 1000000.** Edición temporal de `ExpressionParser.MaxTextLength`, ejecutada junto con las pruebas de guardas de
  G5: 34 seleccionadas, 27 pasan y 7 fallan. Fallan las tres formas de B y, como se esperaba al subir la guarda, las 4
  pruebas de G5 fijadas a 4000: `LAS_GUARDAS_TIENEN_LOS_VALORES_DE_V6`, `UN_TEXTO_DE_4001_CARACTERES_DA_LIMITEXCEEDED`,
  `LA_GUARDA_DE_TEXTO_CORTA_ANTES_DE_LEER_EL_TEXTO` y `EL_EXCESO_UNICODE_SE_SENALA_DESDE_EL_CARACTER_4001`.
- **Restauración byte a byte.**
  - Tras el experimento, `git hash-object src/RackCad.Application/Expressions/ExpressionParser.cs` coincide con el blob de
    HEAD, `060d568709607c929a847e6b5cd7e4cb76b4a38b`.
  - El ensamblado recompilado después lleva `InformationalVersion = 1.0.0+c4880af66586c535954b0ba0bff7b64b887bddb4` y
    `MaxTextLength = 4000`.

---

## 3. Decisión propuesta A1

### 3.1 Lo que no cambia

- P2.5, completo y literal (§7).
- La semántica de nombres: el nombre no es identidad; la resolución es exacta y única, con `OrdinalIgnoreCase` sin
  recortes ni normalización (P3.5, P7.1, P7.2, §4).
- `#<GUID>` sin nombre, solo como forma diagnóstica de una referencia rota (P4.5, P7.1, §4.2).
- `MaxBoundExpressionDepth = 24`, nodos ≤ 256 y argumentos ≤ 16, con su naturaleza normativa, la definición de
  profundidad y sus ejemplos (P1.9).
- `MaxSyntacticNesting`: valor de implementación, inicial 64 y nunca menor que 24 (P1.9).
- El parser neutral, escrito a mano, determinista e independiente de la cultura (P1.10), que no depende del contexto: en
  la tubería de §2.2 el lexer/parser va antes del binder, y el contexto lo aporta quien llama al enlazar (P26.2;
  ADR-0038 D1). Parsear no requiere `ExpressionContext`.
- `SourceSpan` (P2.1), con las unidades UTF-16 que implementó G5.
- La gramática y las reglas léxicas (P1.2–P1.8).
- OPEN A (§4).
- D1–D7 y D9–D25 de ADR-0038 y, dentro de D8, todo lo que no es la guarda de texto.

### 3.2 Lo único que cambia: la guarda de recurso de P1.9/D8

| | Límite | Valor | Naturaleza | Al escribir | Al leer lo persistido |
|---|---|---|---|---|---|
| **Antes** | Texto de entrada | ≤ 4000 caracteres (nunca menos) | Guarda del parser | `LimitExceeded` (`TextLength`) | No aplica: lo persistido no tiene texto |
| **Después** | Tokens sintácticos | Valor de implementación: inicial **4096**, nunca menor que 6 × nodos normativos (1536; §4) | Guarda del parser | `LimitExceeded` (`SyntacticTokenCount`) | No aplica: lo persistido no tiene texto |

- **A1.1 — Sin límite de validez por caracteres.** No existe ningún límite contractual basado en el número total de
  caracteres del texto. `TextLength` deja de ser una condición del contrato y no se emite (§6).
- **A1.2 — Guarda de complejidad sintáctica.** `MaxSyntacticTokens` acota la cantidad de tokens que genera el lexer.
  - Es un valor de implementación, inicial 4096.
  - Nunca puede bajar de `6 × máximo normativo de nodos`, que con nodos ≤ 256 vale 1536 (§4).
  - Superarla da `LimitExceeded` con tipo `SyntacticTokenCount`.
- **A1.3 — Qué cuenta como un token.** Cada lexema que delimita el lexer del núcleo, válido o inválido, cuenta **uno**:
  - un número, con todos sus dígitos;
  - un sufijo de unidad `[…]`;
  - un nombre desnudo completo, `word {" " word}`, **con todas sus palabras**;
  - un nombre entre llaves completo, con sus escapes `}}`;
  - un cualificador `#<GUID>`;
  - cada operador `+ - * /`, cada paréntesis, cada coma y el `.` de la sintaxis de namespace;
  - cada lexema mal formado que el lexer diagnostica.

  No cuentan los espacios insignificantes ni la marca interna de fin de texto. **Un token puede contener un nombre
  arbitrariamente largo.** Esta granularidad es normativa: la cota de §4 y el mínimo de A1.2 se calculan con ella.
- **A1.4 — Cuándo corre.** Mientras se lexea, antes de que el parser construya nada:
  - el lexer se detiene en cuanto delimitaría el token `MaxSyntacticTokens + 1`, y el parser no corre;
  - el resultado tiene **un solo** diagnóstico: `LimitExceeded`, con tipo `SyntacticTokenCount`, máximo
    `MaxSyntacticTokens` y posición desde el inicio de ese token hasta el final del texto;
  - no se informa ningún otro diagnóstico, igual que con la guarda de texto de G5;
  - superar la guarda **nunca** trunca (P1.9).
- **A1.5 — Separación intacta.** Igual que el anidamiento, la guarda de tokens solo protege al parser. No sustituye a los
  límites normativos del `BoundExpression`, que se siguen validando tras el bind (P1.9, P7.5): ALT-32 sigue rechazada.

### 3.3 Lectura conjunta de V6 y ADR-0038

Cuando exista el nuevo freeze, estos textos se leen con A1. Las líneas son las del blob exacto de cada documento.

| Documento y lugar | Texto vigente | Lectura con A1 |
|---|---|---|
| V6 §0.10, fila A-16 (l. 318) | «texto de entrada ≥ 4000» | Historial de V3: la decisión vigente es la de §3.2 |
| V6 §0.11 (l. 340) | «Texto de entrada ≥ 4000; límites de nodos, argumentos y profundidad», CLOSED | Guarda de tokens de A1.2; los límites de nodos, argumentos y profundidad siguen CLOSED |
| V6 P1.9, tabla (l. 636) | Fila «Texto de entrada», ≤ 4000 caracteres (nunca menos) | Fila «Tokens sintácticos» de §3.2 |
| V6 P1.9, «Guardas del parser, separadas» (l. 668-671) | «La longitud del texto y el anidamiento sintáctico se comprueban antes de descender…» | «El número de tokens y el anidamiento sintáctico…»; el resto de la viñeta no cambia |
| V6 P1.10 (l. 690-691) | «con las guardas de texto y de anidamiento sintáctico de P1.9» | «con las guardas de tokens y de anidamiento sintáctico de P1.9» |
| V6 P28.3, fila G5 (l. 2288) | «guardas del parser (texto y anidamiento sintáctico)» | «guardas del parser (tokens y anidamiento sintáctico)», implementadas en G6 reanudado (§10) |
| V6 §8.1, fila G5 (l. 2817) | «guardas del parser (texto y anidamiento sintáctico)» | Igual que P28.3 |
| ADR-0038 D8, tabla (l. 349) | Fila «Texto de entrada» | La sustituye el ADR de reemplazo (§11) con la fila de §3.2 |
| ADR-0038 D8 (l. 366-367) | «Las guardas de texto y de anidamiento sintáctico solo protegen al parser…» | «Las guardas de tokens y de anidamiento sintáctico…», en el ADR de reemplazo |

Siguen válidos **sin cambio**:
- V6 P1.9, l. 639-640: «Superar un límite nunca trunca. La longitud del texto no es un límite del árbol…». Con A1
  tampoco es una guarda.
- V6 §0.7, fila CR-3 (l. 216): «guardas del parser separadas».
- V6 T-V4-04 (l. 2358): la suma plana de 25 operandos tiene 49 tokens y anidamiento 0, así que las guardas del parser
  siguen sin saltar.
- ALT-32 (V6 l. 2976; ADR-0038 l. 1173).

---

## 4. Cota del formatter

### 4.1 Hipótesis

- `b` es un `BoundExpression` legal: `n ≤ 256` nodos, profundidad ≤ 24 y ≤ 16 argumentos por llamada.
- `Format(b)` es el formatter único de P4.2, con paréntesis mínimos y la forma mínima inequívoca de cada referencia.
- Los tokens se cuentan según A1.3.

### 4.2 Reparto: cada token se carga a exactamente un nodo

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

### 4.3 Resultado

```text
tokens(Format(b)) ≤ Σ nodos (cabeza ≤ 3 + separador ≤ 1 + paréntesis ≤ 2) = 6n
la raíz no lleva separador ni paréntesis:   tokens(Format(b)) ≤ 6n - 3
con n ≤ 256:                                tokens(Format(b)) ≤ 1533 < 1536 = 6 × 256 < 4096 = MaxSyntacticTokens
```

- **La longitud en caracteres no participa.** Ni el nombre, ni sus escapes `}}`, ni el GUID, ni la representación
  decimal: cada uno es un único token (A1.3).
- **Solo interviene el número de nodos.** La profundidad y los argumentos no entran en la cota de tokens. Siguen acotando
  el anidamiento: el texto canónico anida como mucho tanto como la profundidad, ≤ 24, y `MaxSyntacticNesting` nunca es
  menor que 24 (P1.9, sin cambio).
- **El valor inicial no depende de detalles de tokenización.** A1.3 fija la granularidad y el mínimo de A1.2 se calcula
  con ella. Aun si se contaran por separado `[`, unidad y `]`, y `#` y GUID, el mismo reparto daría
  `8n - 3 = 2045 < 4096`.
- **Lo esencial de A1.3.** Un nombre (con todas sus palabras o entre llaves) y un número cuentan **un** token cada uno:
  contar palabras o caracteres reabriría la tensión.
- **4096 no se eligió por intuición.** Supera el mínimo de 1536, y la cota de 1533, con un margen de 2.67 veces. Si un ADR
  cambiara el máximo normativo de nodos, el mínimo de A1.2 se recalcula; los límites normativos no se tocan para cuadrar
  este número.

### 4.4 Medición de apoyo con el lexer de G5

Tokens que produce el lexer del núcleo en `c4880af` (ensamblado `1.0.0+c4880af…`), sin contar la marca de fin. Apoya la
cota; no la sustituye.

| Texto | Caracteres | Tokens | Nodos | Tokens por nodo |
|---|---|---|---|---|
| A | 11118 | 767 | 256 | 3.00 |
| B, nombre seguro (L = 4000) | 4001 | 1 | 1 | 1.00 |
| B, llaves | 4004 | 1 | 1 | 1.00 |
| B, homónimo | 4001 | 2 | 1 | 2.00 |
| C, suma | 7893 | 47 | 47 | 1.00 |
| C, `MIN` | 78798 | 527 | 256 | 2.06 |
| D | 65875 | 701 | 256 | 2.74 |
| Nombre seguro de 1000001 caracteres | 1000001 | 1 | 1 | 1.00 |
| Nombre desnudo de 100000 palabras | 199999 | 1 | 1 | 1.00 |
| Nombre entre llaves de 1000000 caracteres | 1000000 | 1 | 1 | 1.00 |

A es el caso más denso medido: 256 `Name`, 240 `Qualifier`, 239 `Comma` y 16 pares de paréntesis. Ningún texto produjo
diagnósticos léxicos.

---

## 5. Complejidad y seguridad

Quitar la guarda de caracteres **no** deja al parser sin defensa. G6 reanudado implementa y prueba estos requisitos:

1. **Lexing en `O(caracteres de la entrada)`**, sin recursión.
2. **Parsing en `O(tokens)`**, con `tokens ≤ MaxSyntacticTokens`.
3. **Descenso recursivo acotado por `MaxSyntacticNesting`**, comprobado antes de descender (P1.9, sin cambio).
4. **Colección de tokens acotada por `MaxSyntacticTokens`**: nunca se materializan más de `MaxSyntacticTokens + 1`
   tokens. Los diagnósticos léxicos quedan acotados por la misma guarda, porque cada lexema aporta un número constante de
   diagnósticos.
5. **Ninguna copia cuadrática exigida por el contrato**: las posiciones son desplazamientos, cada nombre se copia un
   número constante de veces (coste `O(longitud)`) y no se re-lexean prefijos.
6. **Ninguna recursión proporcional a la longitud de un nombre**: los nombres se recorren de forma iterativa; en lexer y
   parser, la única recursión es el descenso acotado por `MaxSyntacticNesting`.
7. **Sin dependencia de `ExpressionContext` para parsear**: `MaxSyntacticTokens` es un valor del núcleo, como hoy
   `MaxSyntacticNesting` (`ExpressionParser.cs:31,38` en `c4880af`), no un dato del snapshot. Los `ExpressionLimits`
   de `ExpressionContext` (P6.1) no cambian, y el parser sigue sirviendo a ID20 e ID23 sin contexto (P25, P26.2).

Una entrada con un único nombre enorme cuesta tiempo lineal en la entrada, pero **un** token, anidamiento 0 y un nodo
sintáctico. La memoria sigue siendo lineal en el tamaño de la entrada.

**Consecuencia para G6, fuera de la guarda.** Sin la guarda de caracteres, un nombre también puede ser muy largo al
enlazar: las comprobaciones de G6 sobre el texto de los nombres (P7.1, incluido `OperatorInName`) no pueden reintroducir
un coste cuadrático en la longitud de los nombres.

Los límites físicos del entorno de ejecución (memoria disponible, tamaño máximo de una cadena .NET) son fallos de
ejecución, no reglas del contrato: A1 no los convierte en condición de validez ni en excepción de P2.5.

---

## 6. Diagnósticos

- **Código.** Sigue siendo `LimitExceeded`, de la clase «Sintaxis y límites» (P15.3), con severidad `Error` y posición.
  El orden determinista de P15.7 no cambia.
- **Tipos de límite del parser.**

  | Tipo | Con A1 |
  |---|---|
  | `TextLength` | Deja de ser una condición normativa o productiva y **no se emite** |
  | `SyntacticNesting` | Permanece, sin cambio |
  | `SyntacticTokenCount` | **Nuevo**, con máximo `MaxSyntacticTokens` |

- **Decisión sobre `TextLength`: opción A, eliminarlo en la rama de I-49** cuando G6 se reanude. No existe consumidor
  publicado que lo requiera:
  - el namespace `RackCad.Application.Expressions` solo existe en esta rama: no está en `main` (`1b091be`) ni en las
    demás ramas remotas (I-52, I-54 e I-53S), e I-53 ya está integrada en ese `main`;
  - sus únicos usos son `ExpressionParser.cs` y pruebas de G5;
  - ningún diagnóstico se persiste: lo persistido son formas canónicas (P17), y el núcleo devuelve códigos y datos
    (P15.4);
  - UI y Plugin no nombran el núcleo.

  Al retirarlo, G6 no reutiliza su valor numérico para otro tipo de límite.
- **Límites tras el bind.** Los tipos de `LimitExceeded` para nodos, profundidad y argumentos no cambian de contrato y los
  introduce G6.
- **Sin código ahora.** Todo esto es implementación posterior.

---

## 7. P2.5 — sin cambio

P2.5 permanece literalmente fuerte:

> Para todo `BoundExpression` canónico `b` **sin referencias rotas** y un mismo snapshot:
> `Canonicalize(Bind(Parse(Format(b)))) == b`.

A1 no añade «si cabe en la guarda», «para nombres razonables» ni «salvo textos muy largos». Lo que cambia es que las
guardas del parser ya **no pueden** rechazar el texto canónico de un árbol legal:

- no hay guarda de caracteres (A1.1);
- `Format(b)` produce como mucho 1533 tokens, menos que `MaxSyntacticTokens` (§4);
- su anidamiento sintáctico es ≤ 24, que no supera `MaxSyntacticNesting` porque este nunca es menor que 24 (P1.9).

La propiedad se prueba en G6 reanudado. P28.3 la situaba en G5, y el ajuste de calendario G5/G6 del Coordinador,
registrado en el commit `c4880af`, la trasladó a G6. Las reproducciones A–D de §2.2 son casos obligatorios, con B probado
con nombres de 4001 y de 1000001 caracteres.

---

## 8. Nombres — sin cambio

- **No** se añade `MaxNameLength` ni ningún otro límite de longitud de nombre.
- Sigue la regla histórica: el nombre no puede estar en blanco.
- **No** cambian `Create`, `Rename` ni el store.
- **No** hay migración.
- **No** se rechaza ningún dibujo por la longitud de un nombre.

---

## 9. Opciones rechazadas

| Opción | Veredicto | Motivo |
|---|---|---|
| Límite contractual de longitud de nombre | **REJECTED** | Rompe el contrato histórico y los dibujos existentes, y no es necesario |
| Guarda relativa al snapshot | **REJECTED** | El parser deja de ser neutral e independiente del contexto, y perjudica a ID23 |
| Eliminar toda guarda de complejidad, o fijar la de caracteres por encima de cualquier cadena posible | **REJECTED** como primera opción | La guarda de tokens conserva la protección del parser |
| Debilitar P2.5 | **REJECTED** | Rompe el round-trip canónico y la editabilidad |
| Permitir `#GUID` sin nombre | **REJECTED** | Reabre OPEN A innecesariamente |
| Subir la guarda de caracteres a otro valor finito | **Imposible** | El contraejemplo de §2.1 existe para cualquier `L` |

---

## 10. Impacto sobre G5

- **G5 permanece CLOSED** y `c4880af` **no** se revierte.
- Cuando se cumplan las condiciones de §12 (A1 acordado por Coordinador y Arquitecto, ADR de reemplazo aceptado por el
  Owner y nuevo freeze), **G6 reanudado** cambia la guarda que implementó G5, con RED→GREEN y sin tocar ningún otro
  comportamiento de G5. Referencias en `c4880af`:
  - `src/RackCad.Application/Expressions/ExpressionParser.cs`: `MaxTextLength = 4000` y su documentación (l. 27-31), la
    comprobación previa al lexer (l. 47-58), `TryFindTextExcess` (l. 77-109) y la tubería que describe (l. 20-23);
  - `src/RackCad.Application/Expressions/ExpressionDiagnostic.cs`: el tipo `TextLength` (l. 26-27);
  - `src/RackCad.Application/Expressions/ExpressionLexer.cs`: la documentación que dice que lee el texto entero (l. 9) y
    `Tokenize` (l. 63-70), que hoy reúne todos los tokens antes de parsear;
  - `tests/RackCad.Tests/ExpressionParserGuardTests.cs`: la documentación de la clase (l. 9) y la de l. 160, que nombran
    la guarda de texto; las pruebas fijadas a esa guarda, `LAS_GUARDAS_TIENEN_LOS_VALORES_DE_V6`,
    `UN_TEXTO_VALIDO_DE_4000_CARACTERES_PASA_LA_GUARDA`, `UN_NOMBRE_ENTRE_LLAVES_DE_4000_CARACTERES_PASA_LA_GUARDA`,
    `LA_GUARDA_CUENTA_CARACTERES_UNICODE_Y_NO_UNIDADES_UTF16`, `UN_TEXTO_DE_4001_CARACTERES_DA_LIMITEXCEEDED`,
    `LA_GUARDA_DE_TEXTO_CORTA_ANTES_DE_LEER_EL_TEXTO` y `EL_EXCESO_UNICODE_SE_SENALA_DESDE_EL_CARACTER_4001`; y la cota
    superior de `LA_GUARDA_DE_ANIDAMIENTO_CORTA_ANTES_DE_DESCENDER` (l. 170).
- De esas siete pruebas, las cuatro de §2.2 fallan al subir la guarda; las otras tres siguen pasando, pero pierden su
  sentido sin guarda de caracteres.
- Las entradas de `LA_GUARDA_DE_ANIDAMIENTO_CORTA_ANTES_DE_DESCENDER` tienen como mucho 4000 tokens, así que siguen
  dentro de la guarda de tokens y conservan su sentido.

---

## 11. ADR

- **ADR-0038 está ACCEPTED e INMUTABLE.** Según el [README de ADR](../adr/README.md), «Cuándo modificar / reemplazar», un
  ADR aceptado es inmutable en su contenido: solo cambian su Estado y sus enlaces, con correcciones tipográficas y
  «Notas posteriores» fechadas.
- **Cambiar su decisión exige un ADR nuevo** que la reemplace, y el viejo pasa a `reemplazado por ADR-NNNN`. Nunca se borra
  un ADR, y un número nunca se reutiliza, ni siquiera el de uno rechazado.
- **No se asigna número todavía.** Antes de publicar ese ADR se censan de nuevo los números de `main` y de **todas** las
  ramas activas.
- **Alcance exigido al ADR de reemplazo.**
  - Conserva íntegras D1–D7 y D9–D25, sin reabrirlas.
  - Dentro de D8 sustituye solo la fila de texto y la frase que la nombra, con el contenido de §3.2 y §4.
  - Como `reemplazado por` se aplica al ADR entero, esas decisiones tienen que seguir teniendo autoridad en el ADR nuevo.
  - **Precedencia.** ADR-0038 remite a V6 en su blob y fija que «ante una duda de detalle manda V6» (l. 45). El ADR de
    reemplazo remite a V6 leída con A1, en sus blobs exactos, para que una duda sobre la guarda no se resuelva hacia la
    fila de 4000 caracteres de P1.9.
- **Quién decide.** Solo el Owner lo acepta o lo rechaza; un agente puede redactarlo en estado `propuesto`.

---

## 12. Freeze

- El Consensus Freeze de `364d6c0` **no se reescribe**: es evidencia histórica del consenso sobre V6 y ADR-0038.
- A1 **activa su regla de invalidación** (§3 del freeze), porque altera D8 y una decisión cerrada de V6 (§0.11):

  ```text
  STOP IMPLEMENTATION        → G6 BLOCKED en G6.0, sin producción
  → document finding         → este amendment
  → Coordinator review       → AGREED WITH A1
  → Architect review         → PENDING (el cambio es material)
  → Owner decision           → PENDING (el ADR cambia)
  ```

- Se crea un **nuevo** freeze cuando se cumplan las tres condiciones: `Coordinator = AGREED WITH A1`,
  `Architect = AGREED WITH A1` y la aceptación del ADR de reemplazo por el Owner. Ese freeze declara como autoridad
  conjunta:
  - la Proposal V6 en su blob exacto;
  - A1 en su blob y SHA exactos;
  - el ADR de reemplazo aceptado.
- Solo entonces G6 puede reanudarse.

---

## 13. Evidencia

| Punto | Resultado |
|---|---|
| Contraejemplo `L + 1` | §2.1: 1 nodo, profundidad 1, válido para cualquier `L` finito |
| Guarda 4000 | 14 pruebas seleccionadas: 7 fallan con `LimitExceeded(TextLength)`, todas por aserción; 7 controles pasan (§2.2) |
| Guarda 1000000, temporal | A, C y D parsean; las tres formas de B siguen fallando; las 4 pruebas de G5 fijadas a 4000 fallan, como se esperaba (§2.2) |
| Restauración | `ExpressionParser.cs` byte a byte igual al blob de HEAD `060d568709607c929a847e6b5cd7e4cb76b4a38b`; ensamblado recompilado `1.0.0+c4880af…` con `MaxTextLength = 4000` |
| Tokens | Con el lexer de G5, como mucho 767 tokens para 256 nodos, y 1 token para un nombre de un millón de caracteres (§4.4) |
| Archivos productivos | Cero. A1 añade solo este documento; ni el experimento ni la medición dejaron cambios en el árbol |
| G5 | Al redactar A1, la rama remota de I-49 apunta a `c4880af66586c535954b0ba0bff7b64b887bddb4`; A1 se publica como hijo directo de ese commit y no toca archivos de G5 |
| V6, ADR-0038 y freeze | Blobs `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, `59b8383be62c91526c0fe435af41121e1fd21429` y `49e8041c489d01b5f8dc20b1c7028d46488933ee`, sin cambio |
| `main` | `1b091bedafceb67ca57054a9eb3bf5259efff774`, merge de I-53 E1, que avanzó desde `f8deb67` durante esta ejecución. De lo que A1 cita solo cambia el índice de `docs/adr/README.md`, que gana la fila de ADR-0037; la sección que cita §11 no cambia. La rama no se rebasa por un amendment documental: la reconciliación con `main` sigue WORKFLOW antes del siguiente gate productivo |
| Ramas paralelas | I-52 `e998a2b714eaccf6ec7bd4d8434423c572030342`. I-53 integrada en `main` por `1b091be`; su rama remota se retiró. I-53S, rama nueva `feature/cabeceras-multidestino-selectivo` en `fddbffecf67105eb7924f3fa4cc7fdf015def2fc`: un reclamo vacío sobre `main`, sin solape. I-54 `78e6696e64db50ffcfbcd7e87d7dfe0a9ca78417`, su G4B, que preserva `CustomProperties` en el envelope (`RackEmbedComposer.cs` y `RackEmbedDocument.cs`), sin solape con A1. Ninguna otra activa |

---

## 14. Estado

```text
Amendment = A1
Finding   = P1.9/P2.5 CONTRACT TENSION

Coordinator = AGREED WITH A1
Architect   = PENDING REVIEW
Owner       = PENDING
Replacement ADR = REQUIRED

G6 = BLOCKED
PRODUCTION CHANGES = NONE
```
