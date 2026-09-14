# I-49 — Proposal V6 · Amendment A2: identidad de `VariableId` y cualificador de clave exacta

Amendment documental de la [Proposal V6](I-49-proposal-v6.md), leída con el
[Amendment A1](I-49-proposal-v6-amendment-a1-text-guard.md), para **una sola** incompatibilidad contractual: la clave
textual de `projectVariable` (P3.2, P8.1) frente al cualificador `#` fijado en forma D (P3.9;
[ADR-0040](../adr/0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md) D6 y D7) y la canonicidad
universal `Canonicalize(Bind(Parse(Format(b)))) == b` (P2.5). La encontró la revisión de solo lectura G6-C1 sobre el
candidato de G6 y la confirmó la revisión de identidad del Arquitecto (`Architect = AGREED — A2 REQUIRED`).

```text
Amendment          = A2
Finding            = VARIABLEID / QUALIFIER / P2.5 CONTRACT TENSION

Enmienda a         = Proposal V6, blob ef4db3aa400483ff25a8f39b2beb93708fa43d1a (no se edita)
Junto a            = Amendment A1, blob d62019088b9e7a140d5066799afe6ace6db303ba (no se edita ni se enmienda)
Decisión afectada  = ADR-0040 D6 y D7
                     ADR aceptado e inmutable, blob 9855a6b32a82954839ba2ae67cf351eae118c8ef
Freeze afectado    = docs/initiatives/I-49-consensus-freeze-v6-a1.md
                     blob cb2105770e136b24dd6d096acb49af8f04adf491 (no se reescribe)
Candidato G6       = 5f969cc87acb6cbd8572af3e2b517ba3468e5c41
                     CI 34795083053 success 4/4; G6 NOT CLOSED
Alcance            = SOLO la sintaxis textual del cualificador de identidad, la validez neutral de su clave
                     y su formatter
Identidad          = sin cambio
```

---

## 1. Qué es y qué no es

- **Es** un amendment del Coordinador (`Coordinator = AGREED WITH A2 / E1`) que recoge el modelo E1 recomendado por el
  Arquitecto y queda sometido a su revisión exacta. Repara **una** incompatibilidad y nada más.
- **No** edita V6, A1, ADR-0040, ninguno de los dos Consensus Freeze ni el Discovery: todos siguen en sus blobs exactos.
- **No** cambia la identidad. La clave de `projectVariable` sigue siendo el texto del `VariableId`, con igualdad
  `OrdinalIgnoreCase` (§3.1).
- **No** autoriza implementación: G6 sigue NOT CLOSED sobre `5f969cc` y G7 sigue BLOCKED (§9, §11).
- **Activa** la regla de invalidación del freeze vigente para seguir implementando (§4 de ese registro): cambia D6 y D7
  de ADR-0040 y una decisión cerrada de V6, la forma del cualificador de OPEN A (§4 de V6).
- Como en V6 y en A1, los nombres de tipos y miembros son **ilustrativos**: el contrato es el comportamiento.

---

## 2. Hallazgo

### 2.1 Comportamiento histórico medido de `VariableId`

Medido en la revisión G6-C1 y en la revisión de identidad del Arquitecto, con el código de `main` `104ef3a` y del
candidato `5f969cc`. `VariableId.cs`, `ProjectVariablesStore.cs`, `UsableProjectVariablesRegistry.cs` y
`ProjectVariableMutationPreflight.cs` son byte a byte iguales en `main` `ba497f1` (§12.1).

- **Lectura.** `VariableId.TryParse` acepta exactamente los textos que acepta `System.Guid.TryParse` y guarda el texto
  **recortado** (`src/RackCad.Application/ProjectVariables/VariableId.cs:57,62`).
- **Igualdad.** Igualdad y hash `OrdinalIgnoreCase` sobre ese **texto** (`VariableId.cs:79,83`). Nunca compara GUIDs.
- **Store.** `ProjectVariablesStore` valida cada entrada con `VariableId.TryParse` y no busca duplicados
  (`src/RackCad.Application/Persistence/ProjectVariablesStore.cs:171`).
- **Acreditación.** `UsableProjectVariablesRegistry.Accredit` indexa por la igualdad vigente de `VariableId`: solo dos
  textos iguales sin distinguir mayúsculas dan `AmbiguousIdentity`
  (`src/RackCad.Application/ProjectVariables/UsableProjectVariablesRegistry.cs:144-177`; el duplicado, en `:168`).
- **Acuñación.** La única llamada productiva es `VariableId.New()`
  (`src/RackCad.Application/ProjectVariables/ProjectVariableMutationPreflight.cs:39`), que emite forma D en minúsculas
  (`VariableId.cs:46`). La lectura **nunca** exigió forma D.

| Disposición del texto | Ejemplo (GUID `3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b` salvo nota) | `TryParse` | Texto conservado | ¿Misma identidad que la D del mismo GUID? |
|---|---|---|---|---|
| D | `3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b` | sí | sí | — |
| D en mayúsculas | `3F2B1C9E-8A4D-4E6F-9B0A-1C2D3E4F5A6B` | sí | sí | **sí** |
| N | `3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b` | sí | sí | no |
| B | `{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}` | sí | sí | no |
| P | `(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)` | sí | sí | no |
| X | `{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}` | sí | sí | no |
| X de compatibilidad | un grupo corto (`0xa`), ceros a la izquierda (medido con 4), espacios, `<U+00A0>` o `<U+2028>` interiores, `0x+`, un grupo de 16 bits con 5 dígitos | sí | sí | no, y distintas entre sí |
| X con prefijo `0X` | la X anterior con `0X` | sí | sí | no; es la **misma** identidad que la X con `0x` |
| D, B y P de compatibilidad (GUID `003f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b`) | `0x3f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b`, `+03f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b`, `{0x3f2b1c-…}`, `(+03f2b1c-…)` | sí | sí | no |
| Espacio exterior | tabulador y salto de línea alrededor de la D | sí | **recortado** | sí |
| Rechazadas | `{+0x…}`; N con `0x` o `+`; D con un espacio interior; dígitos, letras o signos no ASCII | no | — | — |

- **Store y acreditación.** N, B, P, X, X con `<U+00A0>`, X con `<U+2028>`, la D con `0x` dentro de un grupo y la X con
  un grupo corto, con ceros a la izquierda o con espacios dan `Readable` en el store y `Usable` en la acreditación, con
  el texto intacto. Para N, B, P y X se midió además que `Serialize` vuelve a escribir el texto igual. Las demás
  disposiciones de la tabla se midieron solo con `VariableId.TryParse`.
- **Búsqueda.** Con el mismo texto el registro encuentra el target; con el texto D del mismo GUID, **no**.
- **Duplicados.** Dos entradas del mismo GUID en D+N, D+B, D+P o N+B acreditan como **dos** identidades. D con la D en
  mayúsculas, o con la D rodeada de espacios, da `AmbiguousIdentity`.
- **Familia.** Seis textos del mismo GUID —D, N, X canónica, X con un grupo corto, X con ceros a la izquierda y X con
  espacios—, todos con `Name = Holgura`, acreditan como **seis** identidades.

### 2.2 Premisa rota: Discovery §4.1

- El [Discovery](I-49-discovery.md), §4.1 (l. 257, FACT), dice que `TryParse` «rechaza lo que no es GUID y guarda
  `value.Trim()`» y que igualdad y hash son `OrdinalIgnoreCase`. Es exacto, pero **no** registró que `Guid.TryParse`
  acepta varias disposiciones textuales del mismo GUID ni que `VariableId` conserva esa representación como identidad.
- V6 construyó sobre esa base: P3 («`VariableId` es GUID con igualdad `OrdinalIgnoreCase`», l. 758), P3.9 (cualificador
  solo en forma D), P2.5 («la sintaxis de §4 representa cualquier identidad», l. 726-727) y §4.6 («El GUID completo no
  colisiona», l. 2558). Con las disposiciones medidas, las dos últimas afirmaciones son falsas.
- El Discovery **no se edita**. A2 es la corrección durable de esa premisa.

### 2.3 Contraejemplo R1

Registro `Readable` y `Usable` (medido):

```text
A: Name = Holgura    VariableId = 3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b        (N)
B: Name = Holgura    VariableId = 0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d    (D)
```

`b = Reference(projectVariable:3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b)`: canónico, sin referencias rotas.

1. Hay homónimos, así que la referencia a A tiene que ir cualificada (P4.2, P7.1): `Holgura` y `{Holgura}` dan
   `AmbiguousName` (medido con homónimos de clave D en `5f969cc`, que no admite la clave N).
2. El cualificador crudo, `Holgura#3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b`, da `InvalidQualifier` bajo V6 (medido).
3. La representación D, `Holgura#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b`, parsea (medido), pero nombra **otra** clave
   textual: con la igualdad vigente el texto D no es el texto N (medido con la igualdad de `VariableId` y con el
   comparador de clave de `SymbolId`), así que por P7.1 el binder da `BrokenReference`.
4. P2.5 falla.

### 2.4 Contraejemplo R2

Registro `Readable` y `Usable`, con **dos** identidades (medido):

```text
A: Name = Holgura    VariableId = 3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b    (D)
B: Name = Holgura    VariableId = 3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b        (N, el mismo System.Guid)
```

- Las dos referencias necesitan cualificador (P4.2, P7.1).
- El cualificador de V6, solo D, emite las dos con **el mismo** texto, `Holgura#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b`:
  la representación D de las dos claves es idéntica (medido).
- Toda variante admitida de ese texto —mayúsculas del GUID, mayúsculas del nombre, llaves en el nombre— enlaza al mismo
  símbolo (medido con la clave D en `5f969cc`).
- Ningún cualificador solo D puede representar las dos identidades: la contradicción es **contractual**.

### 2.5 Por qué no es un defecto de implementación

- **Ningún binder conforme separa R2.** Los textos que P3.9 admite para cualquiera de las dos identidades solo difieren
  en mayúsculas del GUID, mayúsculas del nombre o llaves en el nombre. P3.9 compara el GUID «igual que la igualdad de
  `VariableId`», con un cualificador el nombre solo se valida sin distinguir mayúsculas (P7.1, P7.2) y las llaves no
  cambian el nombre. `Bind(Parse(t))` depende solo de esa clase de textos: no puede devolver los dos `SymbolId`, y P2.5
  falla para uno de ellos.
- **Ningún adaptador lo arregla.** Cualquier espacio de claves en forma D colisiona en R2. Unas claves sustitutas por
  snapshot serían una identidad nueva, inestable entre lecturas, y romperían la forma cualificada que se pega o se
  teclea (§4.2, regla 9).
- **El candidato de G6 lo esquiva rechazando.** `SymbolId` de `5f969cc` exige forma D
  (`src/RackCad.Application/Expressions/SymbolId.cs:97-99`): con el registro de R1 no se puede construir la tabla que
  exigen P6.4 y P8.1. Es la opción A de §8 aplicada dentro del núcleo, no una decisión de implementación, y G6 no se
  puede cerrar así.

---

## 3. Decisión A2

### 3.1 La identidad no cambia

```text
SymbolId projectVariable: clave = el texto del VariableId de la entrada del registro acreditado
Comparador                      = OrdinalIgnoreCase
```

- **No** hay igualdad por valor de `System.Guid`.
- **No** se normaliza la identidad a un valor GUID ni a ninguna disposición.
- **No** se colapsan las representaciones D, N, B, P y X ni las de compatibilidad.
- **No** hay migración.
- **No** se endurece la regla del `VariableId` persistido.
- **No** hay id alias.
- El `System.Guid` que se obtiene al parsear **nunca** es identidad.

P8.1, P3.6 y P6.4 no cambian. P3.2 conserva la identidad y el comparador, y gana la validez neutral de §3.2.

### 3.2 Validez neutral de la clave

Para el núcleo neutral de expresiones, una clave de `SymbolId(projectVariable, clave)` es válida cuando:

```text
clave no es null y no está vacía
clave == clave.Trim()
System.Guid.TryParse(clave, out _) == true
```

- La regla es **neutral**: no nombra ningún tipo de `ProjectVariables` (P5.1, P26.1).
- El conjunto de claves válidas coincide exactamente con el de los `VariableId.Value` posibles (§2.1): ningún
  `VariableId` legible hoy queda fuera, y ningún texto que no pueda ser un `VariableId` entra.
- Sustituye la regla de solo forma D del candidato (§2.5).
- G8 prueba después que su adaptador transporta `VariableId.Value → SymbolId.Key` **sin normalizar** (§9.3, B).
- `Guid.TryParse` **solo** decide la admisibilidad. El `Guid` devuelto **nunca** se usa para igualdad, hash, búsqueda,
  identidad ni para desambiguar en el formatter.

### 3.3 Gramática del cualificador

Enmienda conceptual de la producción `qualifier` de P1.2:

```ebnf
qualifier      = "#" , ( guid-d | braced-key ) ;
guid-d         = guid ;                                   (* forma D completa: 8-4-4-4-12, 36 caracteres *)
braced-key     = "{" , { char - "}" | "}}" } , "}" ;       (* "}}" es un "}" literal *)
```

`guid-d` es la regla vigente de forma D completa, sin cambio. El contenido **desescapado** de `braced-key`:

- no está vacío;
- es igual a su propio `Trim()`;
- satisface `System.Guid.TryParse`;
- se interpreta como el **texto de la clave** del `SymbolId`.

`}}` significa un `}` literal. No hay ningún otro escape.

### 3.4 Comportamiento léxico

- **`#` + forma D.** El comportamiento vigente del cualificador, sin cambio.
- **`#{…}`.** Cualificador de clave exacta:
  - dentro de `braced-key`, todo carácter es literal salvo el escape `}}`;
  - comas, espacios, `<U+00A0>`, `<U+2028>`, paréntesis, `0x`, `+`, `{` anidadas y los demás caracteres que acepta
    `Guid.TryParse` son **datos**, no sintaxis de la expresión; cualquier otro carácter también se lee literal y deja
    un contenido inválido;
  - la llave de cierre `}` lo termina, y `}}` codifica un `}` literal.
- **`#{` sin cerrar.** El diagnóstico vigente `UnterminatedName`.
- **Contenido desescapado inválido** —vacío, con espacio en blanco inicial o final (el de `Trim()`), o rechazado por
  `Guid.TryParse`—: `InvalidQualifier`. Un fragmento, como `#{3f2b1c9e}`, sigue sin parsearse (ALT-29).
- **Cualquier otra cosa tras `#`.** `InvalidQualifier`, como hoy.
- **No** hay códigos de diagnóstico nuevos.

### 3.5 Autoridad de la sintaxis

- La sintaxis del cualificador conserva el **texto de la clave** como su contenido semántico.
- Si la implementación guarda o parsea además un `System.Guid` por comodidad, ese valor **no es autoritativo** y
  **nunca** se usa para resolver identidad. Se prefiere retirar toda dependencia semántica de `QualifierSyntax.Id` y del
  `Guid`, que hoy existen en `src/RackCad.Application/Expressions/ExpressionSyntax.cs:120,127` y
  `src/RackCad.Application/Expressions/ExpressionLexer.cs:358`.
- El binder resuelve **solo** con la clave textual recuperada y el comparador de clave del namespace. El candidato ya
  construye el `SymbolId` desde el texto (`src/RackCad.Application/Expressions/ExpressionBinder.cs:332`) y devuelve el
  `SymbolId` de la entrada de la tabla (`:380`), de modo que lo persistido conserva la grafía del registro.

### 3.6 Formatter

Para `SymbolId(projectVariable, clave)`:

```text
si clave tiene forma D exacta:
    Q(clave) = "#" + minúsculasASCII(clave)
si no:
    Q(clave) = "#{" + minúsculasASCII(clave) con cada "}" sustituida por "}}" + "}"
```

- **Forma D exacta**: 36 caracteres, `-` en las posiciones 9, 14, 19 y 24 y dígitos hexadecimales ASCII en las demás, en
  cualquier combinación de mayúsculas. Una clave de compatibilidad con `0x` o `+` dentro de un grupo **no** la tiene.
- **minúsculasASCII**: solo `A`–`Z` pasan a `a`–`z`; ningún otro carácter cambia.
- Bajar a minúsculas **no** cambia la identidad, porque el comparador es `OrdinalIgnoreCase` (§4), y **no** modifica el
  texto persistido del `VariableId`.

| Clave | Cualificador |
|---|---|
| `3F2B1C9E-8A4D-4E6F-9B0A-1C2D3E4F5A6B` (D) | `#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b` |
| `3F2B1C9E8A4D4E6F9B0A1C2D3E4F5A6B` (N) | `#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}` |
| `{3F2B1C9E-8A4D-4E6F-9B0A-1C2D3E4F5A6B}` (B) | `#{{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}}` |
| `(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)` (P) | `#{(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)}` |
| `{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}` (X) | `#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}` |
| `+03f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b` (D de compatibilidad) | `#{+03f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b}` |

La salida de la clave B, `#{{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}}`, **tiene que probarse** después para demostrar que
no colisiona con `#{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}`, que es la clave textual D escrita entre llaves (§9.3, F).

### 3.7 Presentación de referencias

P4.2 conserva su política de forma mínima:

| Situación | Texto |
|---|---|
| Nombre único y seguro | `Nombre` |
| Nombre único con cualquier otro carácter, o reservado | `{Nombre}` |
| Homónimo | forma del nombre + `Q(clave)` |
| Referencia rota | solo `Q(clave)` |
| Candidatos de `AmbiguousName` | forma del nombre + `Q(clave)`, el mismo cualificador canónico |

Así, un homónimo con clave D se muestra `Holgura#<d>` y uno con clave N, `Holgura#{<n>}`. `#<id>` o `#{<clave>}` sin
nombre, con el id presente, siguen dando `NameRequired` (P7.1; §4.2, regla 11).

### 3.8 Entrada `#{<clave con forma D>}`

- En la entrada del usuario, `#{<clave con forma D>}` es legal si el contenido es una clave válida.
- Resuelve **exactamente** igual que la clave textual D.
- La salida canónica del formatter es `#<d>`, porque la forma corta es la representación canónica mínima de una clave
  con forma D. Del mismo modo, `Nombre#{D}` se muestra `Nombre#d` cuando hace falta cualificar (P4.3).
- No crea una segunda identidad.

---

## 4. Inyectividad: R1 y R2 resueltos

**Premisa.** Todo carácter de una clave admisible (§3.2) es ASCII o espacio en blanco Unicode: en un texto sin espacio
exterior, `Guid.TryParse` rechaza dígitos, letras y signos no ASCII y solo acepta espacio en blanco no ASCII dentro de
la disposición X. Se midió con muestras (§12.1) y la exige la prueba G de §9.3. Con ella, para dos claves admisibles
`K1` y `K2`:

```text
K1 ≡ K2 bajo OrdinalIgnoreCase   ⇔   minúsculasASCII(K1) = minúsculasASCII(K2) en Ordinal
```

porque `OrdinalIgnoreCase` iguala exactamente las letras ASCII que solo difieren en mayúsculas y un espacio en blanco no
tiene mayúsculas.

**`Q` separa identidades.** Sean `K1` y `K2` claves distintas bajo `OrdinalIgnoreCase`.

1. **Dominios disjuntos.** Una clave con forma D exacta da `"#" + 36 caracteres hexadecimales o guiones`; cualquier otra
   da `"#{" + … + "}"`. El segundo carácter de un texto del primer dominio nunca es `{`. Tener forma D no depende de las
   mayúsculas, así que dos grafías de una misma identidad caen siempre en el mismo dominio.
2. **Dentro del dominio D.** `Q(K1) = Q(K2)` ⇔ `minúsculasASCII(K1) = minúsculasASCII(K2)` ⇔ `K1 ≡ K2`.
3. **Dentro del dominio entre llaves.** Duplicar `}` es reversible —el lector de `braced-key` lo deshace—, así que
   `Q(K1) = Q(K2)` ⇔ `minúsculasASCII(K1) = minúsculasASCII(K2)` ⇔ `K1 ≡ K2`.

Por tanto `K1 ≢ K2 ⇒ Q(K1) ≠ Q(K2)` bajo el comparador de `SymbolId`, y `K1 ≡ K2 ⇒ Q(K1) = Q(K2)`: una identidad, un
texto.

**El binder invierte al formatter.** La forma corta recupera `minúsculasASCII(K)` y la forma entre llaves también, tras
deshacer `}}`; en los dos casos el resultado es `≡ K`. `minúsculasASCII(K)` sigue siendo admisible: las únicas letras de
una clave admisible son dígitos hexadecimales y la `x` de `0x`, que `Guid.TryParse` acepta en cualquier combinación de
mayúsculas, y bajar letras no toca los espacios.

**R1 y R2:**

```text
R1   A (N):  Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}
     B (D):  Holgura#0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d

R2   A (D):  Holgura#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b
     B (N):  Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}
```

La familia de seis identidades del mismo GUID de §2.1 da seis cualificadores distintos:

```text
#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b
#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}
#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}
#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0xa,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}
#{{0x00003f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}
#{{0x3f2b1c9e, 0x8a4d, 0x4e6f, {0x9b, 0x0a, 0x1c, 0x2d, 0x3e, 0x4f, 0x5a, 0x6b}}}}}
```

Medición de apoyo (§12.1), con un modelo de `Q` que no es producción: 22 claves medidas en 18 clases de identidad dan un
texto por clase y 18 textos distintos. Las 18 claves sin forma D se recuperan con el lector vigente de nombres entre
llaves como `minúsculasASCII(clave)`, admisible; las 4 con forma D, quitando el `#`.

---

## 5. P2.5 — sin cambio

P2.5 sigue **igual e incondicional**:

```text
Canonicalize(Bind(Parse(Format(b)))) == b
```

para todo `BoundExpression` canónico sin referencias rotas en un mismo snapshot. A2 existe precisamente para
restablecerlo para **toda** identidad de `VariableId` representable hoy: con §3.3, «la sintaxis de §4 representa
cualquier identidad» vuelve a ser cierto.

Corpus obligatorio de la futura prueba de propiedad (P28):

- D, N, B, P y X;
- las disposiciones de compatibilidad medidas (§2.1);
- el mismo `System.Guid` en varias identidades textuales;
- homónimos entre esas identidades;
- referencias rotas;
- candidatos de `AmbiguousName`;
- grafías en mayúsculas y en minúsculas.

No hay excepción de «formato razonable».

---

## 6. A1 — sin cambio

- Las dos alternativas del cualificador cuentan **un** token: el cualificador de A1.3. La lista de tokens de ADR-0040
  D8 (l. 429) se lee igual.
- `MaxSyntacticTokens`, el mínimo de 1536 y `tokens(Format(b)) ≤ 6n − 3` no cambian: la cabeza de una referencia sigue
  siendo el nombre más un cualificador.
- Un `braced-key` puede ser largo —por ejemplo, con ceros a la izquierda—, pero sigue siendo un token, y A1.1 no fija
  límite de caracteres.
- Los requisitos de A1 §5 (lexing lineal, colección de tokens acotada, sin copias cuadráticas y sin recursión
  proporcional a la longitud) se aplican a un `braced-key` igual que a un nombre entre llaves, porque su contenido
  tampoco tiene un tope contractual de caracteres.
- A1 §3.1 enumera lo que **A1** no cambia, entre otras cosas la gramática y las reglas léxicas (P1.2–P1.8) y OPEN A:
  describe el alcance de A1 y no contradice un amendment posterior.
- A2 **no** enmienda A1.

---

## 7. Cláusulas afectadas y lectura conjunta

Cuando exista el nuevo freeze (§11), estos textos se leen con A2. Las líneas son las de los blobs exactos de V6
(`ef4db3a`) y de ADR-0040 (`9855a6b`).

**Regla general de lectura.** Toda otra mención de `#<id>`, `#<GUID>`, `#GUID` o «GUID completo» como cualificador en V6
y en ADR-0040 —resúmenes, estados, ejemplos y consecuencias— se lee como `Q(clave)` en la salida y como cualquiera de
las dos formas de §3.3 en la entrada. V6 P1.9 (l. 639-640), «una referencia cualificada ocupa más de 36 caracteres», se
lee «al menos 36 caracteres» para una referencia cualificada con nombre: `A#{<N>}` mide 36.

### 7.1 V6, enmendadas directamente

| Lugar | Texto vigente | Lectura con A2 |
|---|---|---|
| P1.2, producción `qualifier` (l. 595) | `qualifier = "#" , guid ;` (forma D completa) | `qualifier = "#" , ( guid-d \| braced-key ) ;` (§3.3) |
| P1.7 (l. 625-626) | «Un cualificador que no sea un GUID completo en forma D (`#3f2b1c9e`) es `InvalidQualifier`» | Reglas léxicas de §3.4 |
| P3.9 (l. 777-782) | `#` + `VariableId` completo en forma D; el formatter lo emite siempre en forma D en minúsculas; un fragmento nunca se parsea | §3.2, §3.3, §3.6 y §3.8; el fragmento sigue sin parsearse |
| §4.1, formas (l. 2490-2496) | `Nombre#<GUID completo>`, `{Nombre complejo}#<GUID completo>`, `#<GUID completo>` | Las mismas formas con `Q(clave)`: `#<d>` para una clave con forma D exacta y `#{<clave exacta>}` para las demás |
| §4.2, regla 1 (l. 2502) | «GUID completo, nunca un fragmento. Forma D de 36 caracteres (P3.9)» | Clave completa, nunca un fragmento: forma D exacta o clave exacta entre llaves (§3.3) |
| P28.5, T-V3-02 (l. 2326) | «Solo la forma D de 36 caracteres; `#3f2b1c9e` es `InvalidQualifier`; …» | «Solo la forma D de 36 caracteres» rige la forma corta `#<d>`; `#{<clave>}` es válido según §3.3 y §3.8; `#3f2b1c9e` y `#{3f2b1c9e}` siguen siendo `InvalidQualifier`; además, la matriz de §9.3 |

### 7.2 V6, solo porque consumen la definición del cualificador

| Lugar | Texto vigente | Lectura con A2 |
|---|---|---|
| P4.2, tabla (l. 806, 809) | `#<id>` para un id ausente; forma del nombre + `#<id>` para un homónimo | `Q(clave)` (§3.7) |
| P4.5 (l. 815-817) | La referencia rota se muestra como `#<id>` | Se muestra como `Q(clave)` y sigue sin comprometerse |
| P7.1, tabla (l. 892-895, 898) | `Nombre#<id>`, `…#<id>`, `#<id>` sin nombre; candidatos «en forma cualificada» | Cada `#<id>` es cualquiera de las dos formas de §3.3; los candidatos usan `Q(clave)` |
| §4.2, reglas 9 y 11 (l. 2513, 2515-2516) | La forma cualificada se pega o se teclea; `#GUID` roto y `#<GUID>` sin nombre | Valen para las dos formas |
| §4.2, regla 7 (l. 2511) — censo de A2 | Candidatos «en su forma cualificada» | `Q(clave)` |
| P4.6 (l. 818-819) — censo de A2 | Con un homónimo, `=Holgura#<id>` | `=Holgura` + `Q(clave)` |
| §4.3, tabla (l. 2524-2529) — censo de A2 | `#<id>` | `Q(clave)` |
| §4.4, ejemplos (l. 2542-2544) — censo de A2 | Ejemplos con ids en forma D | Siguen válidos; §3.6 añade los de claves no D |
| §4.6, fila 1 (l. 2558) — censo de A2 | «El GUID completo no colisiona» | La clave exacta no colisiona (§4) |
| P28.5, T-V3-01 y T-V3-03 (l. 2325, 2327) — censo de A2 | `Nombre#<GUID>`, `{Nombre}#<GUID>`; el formatter emite `#<id>` para un id ausente | Con las dos formas de §3.3 |

### 7.3 V6, validez de la clave

| Lugar | Texto vigente | Lectura con A2 |
|---|---|---|
| P3.2 (l. 763-765) | «Para `projectVariable` la clave es el `VariableId`, con su igualdad vigente» | Identidad y comparador sin cambio; gana la validez neutral de §3.2 |

### 7.4 ADR-0040

| Lugar | Texto vigente | Lectura con A2 (en el ADR de reemplazo) |
|---|---|---|
| D6, formas (l. 298-300) | `Nombre#<GUID completo>`, `{Nombre complejo}#<GUID completo>`, `#<GUID completo>` | Formas con `Q(clave)` (§3.7) |
| D6, regla 1 (l. 304-305) | El cualificador es el GUID completo en forma D | §3.3 y §3.4 |
| D6, reglas 6, 7 y 9 (l. 314-322) | Forma cualificada; `#<GUID>` roto; `#<GUID>` sin nombre | Valen para las dos formas |
| D6, formatter (l. 323-326) | «…con el GUID en forma D y minúsculas» | §3.6 |
| D7, producción `qualifier` (l. 348) | `qualifier = "#" , guid ;` (forma D completa) | §3.3 |
| D7, reglas léxicas (l. 355-356) | «un cualificador que no sea un GUID completo en forma D es `InvalidQualifier`» | §3.4 |
| D5 (l. 266-289) | `key = VariableId (igualdad vigente de VariableId)` | Sin cambio; gana la validez neutral de §3.2 |
| D8, lista de tokens (l. 429) | «un cualificador `#<GUID>`» | Sin cambio de conteo: cualquiera de las dos formas cuenta uno (§6) |

### 7.5 No cambian

- P2.5, P8.1, P3.6, P6.4 y la estructura de P17.
- A1.
- Unidades, funciones y límites normativos.
- `PlanReadSet` y `RepairDecisionObservation`.
- Schema V-0.
- La semántica del evaluador.
- P3.5 y P7.2: el nombre nunca es identidad y se compara con igualdad exacta `OrdinalIgnoreCase`.
- La precisión de OPEN A: un cualificador sin nombre nunca enlaza (ALT-28), y un fragmento nunca se parsea (ALT-29).

---

## 8. Opciones rechazadas

| Opción | Qué haría | Por qué no |
|---|---|---|
| **A** — exigir forma D persistida | Solo ids D desde ahora y rechazo de los no D existentes | **Rechazada**: rompe dibujos que hoy se leen y acreditan, y la identidad vigente; cambia la regla vigente de lectura de `VariableId` (P3, Discovery §4.1) en la que se apoyan P3.2 y P8.1; con migración reescribiría ids persistidos |
| **B** — `SymbolId` canónico en forma D | Normalizar la clave al valor GUID | **Rechazada**: colapsa identidades textuales (R2 y la familia de seis) y deja dos semánticas de identidad: texto en el registro y en las referencias directas, GUID en las expresiones |
| **C** — igualdad por valor de GUID | Cambiar la igualdad de `VariableId` | **Rechazada**: cambia la igualdad vigente de `VariableId` en la que se apoyan P3.2 y P8.1, y con ella la identidad estable de ADR-0034 §2; R2 pasaría a `AmbiguousIdentity` en dibujos hoy utilizables, y referencias escritas con otra disposición cambiarían de resolución |
| **D1** — gramática cruda de `Guid.TryParse` tras `#` | Aceptar el texto crudo del id después de `#` | **Rechazada**: duplica dentro del lexer la gramática de compatibilidad de .NET y choca con las reglas de coma, espacios, llaves, paréntesis y `+` (P1.5, P1.7); medido hoy, una X cruda dentro de una expresión da `InvalidQualifier` y `UnterminatedName` |
| **D2** — etiquetas finitas de disposición | Disposición + valor GUID | **Rechazada**: no es sin pérdida; un mismo GUID admite varias identidades de la misma disposición (§2.1) |
| **D3** — clave exacta entre llaves para **todos** los ids | `#{…}` también para las claves D | **Viable, pero rechazada** frente a E1: sustituye sin necesidad la forma corta D aceptada en OPEN A, que es la que emiten todos los ids acuñados hoy |
| **E2** — clave exacta codificada | Forma corta D + un códec ASCII para el resto | **Viable, pero rechazada**: superficie normativa mayor y opaca sin necesidad |
| **Ids sustitutos o alias** | Claves inventadas por snapshot, o alias persistidos | **Rechazada**: identidad nueva en los dos casos; las claves por snapshot son inestables entre lecturas, y los alias persistidos añaden un segundo índice de identidad que habría que mantener |
| **GUID como cualificador y clave exacta solo ante colisión** | Forma D por valor y llaves solo si otra entrada comparte GUID | **Rechazada**: enlazar por GUID no es la igualdad de `VariableId`, y el texto mostrado dependería de otras entradas del snapshot, así que un `Create` ajeno cambiaría a qué enlaza un texto |

---

## 9. Impacto sobre el candidato G6

### 9.1 Corrección futura, tras la aprobación

**No se implementa ahora.** Cuando A2 sea autoritativo (§11), G6 necesita una corrección acotada sobre `5f969cc`:

- relajar la validación de solo forma D de `SymbolId` (`SymbolId.cs:97-99`) a la regla de §3.2;
- que la sintaxis del cualificador lleve el texto exacto de la clave (§3.5);
- que lexer y parser añadan el cualificador entre llaves (§3.3, §3.4); el lector vigente de nombres entre llaves ya
  recupera exactamente esos contenidos (`src/RackCad.Application/Expressions/ExpressionLexer.cs:312-342`; §12.1);
- que el formatter emita la forma corta D o la clave exacta (§3.6). Hoy escribe `#` + la clave en minúsculas
  (`src/RackCad.Application/Expressions/ExpressionFormatter.cs:204,258`); si solo se relajara `SymbolId`, una clave B
  `{…}` se mostraría `#{…}`, que con §3.3 se leería como la clave D interior, y las pruebas F y L de §9.3 lo impiden;
- que el binder resuelva la clave textual exacta (§3.5);
- retirar todo uso semántico de identidad del `Guid` parseado (`ExpressionSyntax.cs:120,127`; `ExpressionLexer.cs:358`).

Todo lo demás de `5f969cc` se conserva salvo que las pruebas demuestren lo contrario.

### 9.2 Pruebas que quedarán superadas

Solo cuando A2 sea autoritativo. **No** se editan ahora.

| Gate | Prueba | Expectativa superada |
|---|---|---|
| G6 | `ExpressionSymbolModelTests.LA_CLAVE_DE_PROJECTVARIABLE_TIENE_QUE_SER_UN_GUID_COMPLETO_EN_FORMA_D` (`tests/RackCad.Tests/ExpressionSymbolModelTests.cs:77`) | Rechazar las claves que no tienen forma D |
| G5 | La fila `"#{" + Guid1 + "}"` → `InvalidQualifier` (`tests/RackCad.Tests/ExpressionSyntaxDiagnosticsTests.cs:150`) | `#{<D>}` pasa a ser un cualificador válido (§3.8) |

Si la implementación retira `QualifierSyntax.Id`, cambian con ella las aserciones y utilidades que lo leen
(`tests/RackCad.Tests/ExpressionSyntaxGrammarTests.cs:227,253`; `tests/RackCad.Tests/ExpressionParserGuardTests.cs:81`;
`tests/RackCad.Tests/ExpressionSyntaxTestSupport.cs:116`); si lo conserva como dato no autoritativo, no.

### 9.3 Matriz de pruebas futura (obligatoria)

| # | Prueba |
|---|---|
| A | `SymbolId` acepta cada disposición representativa que acepta `VariableId`: D, N, B, P, X y las de compatibilidad de §2.1 |
| B | Conformidad del adaptador de G8: `VariableId.Value == SymbolId.Key`, sin ninguna transformación salvo el transporte |
| C | D, N, B, P y X siguen siendo identidades distintas exactamente como con la igualdad vigente de `VariableId` |
| D | D y N del mismo `System.Guid`, homónimas: se formatean distinto y hacen round-trip |
| E | La familia de seis grafías del mismo GUID: las seis hacen round-trip de forma independiente |
| F | Clave B con llaves: escape y desescape exactos; `#{{<d>}}}` y `#{<d>}` recuperan claves distintas |
| G | X exótica: comas, llaves anidadas, espacios interiores, `<U+00A0>`, `<U+2028>`, `0x` y `+`; incluye la premisa de §4 (una clave admisible solo contiene caracteres ASCII o espacio en blanco) |
| H | Inválidos: cualificador entre llaves vacío, espacio exterior en la clave recuperada, fragmento, texto que no es GUID y cualificador sin cerrar |
| I | `#{<D>}` se canonicaliza a la forma corta D |
| J | El formatter de una referencia rota usa la forma correcta del cualificador |
| K | Los candidatos de `AmbiguousName` usan la forma correcta del cualificador |
| L | Corpus generado de P2.5 con disposiciones de clave mezcladas |
| M | La cota de tokens de A1 sigue siendo válida |

---

## 10. ADR

- ADR-0040 está **aceptado** y es inmutable. A2 cambia sus decisiones D6 y D7, así que hace falta un **ADR de reemplazo
  nuevo** antes de implementar.
- **No se asigna número ahora.** Antes de publicarlo se censan de nuevo los números de `main` y de **todas** las ramas
  activas.
- ADR-0040 **no se edita**. Cuando el Owner acepte el ADR de reemplazo, ADR-0040 pasa a `reemplazado por ADR-NNNN`, con
  el mismo patrón que ADR-0038 → ADR-0040.
- **Alcance exigido al ADR de reemplazo.**
  - Conserva íntegras las decisiones de ADR-0040 y, dentro de D6 y D7, sustituye solo lo que enumera §7.4 con el
    contenido de §3; las demás menciones del cualificador se leen con la regla general de §7.
  - D5 conserva la identidad y el comparador, y recoge la validez neutral de §3.2.
  - D8, leída con A1, no cambia (§6).
  - **Precedencia.** Remite a V6 leída con A1 y A2, en sus blobs exactos, para que una duda sobre el cualificador no se
    resuelva hacia la forma D de P3.9.
- **Quién decide.** Solo el Owner lo acepta o lo rechaza; un agente puede redactarlo en estado `propuesto`.

Autoridad futura, tras el consenso exacto y la aceptación del Owner:

```text
V6 + A1 + A2 + ADR de reemplazo + un nuevo Consensus Freeze
```

---

## 11. Freeze

- `docs/initiatives/I-49-consensus-freeze-v6-a1.md` sigue **byte a byte igual**, como evidencia histórica, igual que el
  freeze histórico `docs/initiatives/I-49-consensus-freeze.md`.
- El hallazgo material de G6-C1 lo **invalida para seguir implementando** (§4 de ese registro): toca D6 y D7 y una
  decisión cerrada de V6, la forma del cualificador de OPEN A, sin cambiar la identidad.

  ```text
  STOP IMPLEMENTATION     → G6 NOT CLOSED sobre 5f969cc, sin corrección de producción
  → document finding      → G6-C1 y este amendment
  → Coordinator review    → AGREED WITH A2 / E1
  → Architect review      → AGREED — A2 REQUIRED; PENDING EXACT A2 REVIEW
  → Owner decision        → PENDING (el ADR cambia)
  ```

- Un nuevo freeze declarará como autoridad conjunta V6, A1 y A2 en sus blobs exactos y el ADR de reemplazo aceptado.
  Solo entonces se corrige G6 (§9.1).
- G6 sigue NOT CLOSED. G7 sigue BLOCKED.

---

## 12. Evidencia

### 12.1 Mediciones

| Punto | Resultado |
|---|---|
| Disposiciones de `VariableId` | D, N, B, P, X y las de compatibilidad se aceptan con el texto conservado; D ≠ N, D ≠ B, D ≠ P y D ≠ X para el mismo GUID; la D en mayúsculas es la misma identidad (§2.1) |
| Store y acreditación | N, B, P, X, X con `<U+00A0>`, X con `<U+2028>`, la D con `0x` en un grupo y la X corta, con ceros o con espacios dan `Readable` y `Usable` con el texto intacto; D+N, D+B, D+P y N+B del mismo GUID son dos identidades; D con la D en mayúsculas o con espacios exteriores da `AmbiguousIdentity` |
| Familia del mismo GUID | Seis grafías con `Name = Holgura` acreditan como seis identidades |
| `SymbolId` de `5f969cc` | Acepta D y D en mayúsculas; N, B, P, X y la D con espacios exteriores lanzan `ArgumentException` |
| Cualificador de V6 | `Holgura#<N>`, `Holgura#<B>` y `Holgura#<P>` dan `InvalidQualifier`; `Holgura#<X>` da `InvalidQualifier` y `UnterminatedName` |
| Homónimos D, control | Round-trip exacto con `5f969cc`; las variantes de mayúsculas del GUID y del nombre, y las llaves, enlazan al mismo símbolo; `Holgura` y `{Holgura}` dan `AmbiguousName` con los dos candidatos |
| Aditividad de §3.3 | Todo texto `#{…}` que A2 hace válido da hoy `InvalidQualifier` |
| Lector vigente entre llaves | Recupera exactamente, como un token cada uno, los contenidos B, X, X con espacios, X con `<U+00A0>` y X con `<U+2028>` |
| Premisa de §4 | `Guid.TryParse` rechaza dígitos, letras y signos no ASCII (dígitos de ancho completo y arábigo-índicos, letras de ancho completo, signo Kelvin, `ı`, `+` de ancho completo) y acepta `<U+3000>` dentro de X |
| Modelo de `Q` (no es producción) | 22 claves en 18 clases de identidad: un texto por clase y 18 textos distintos; las 18 claves sin forma D se recuperan con el lector vigente entre llaves como `minúsculasASCII(clave)` admisible, y las 4 con forma D, quitando el `#` |
| Archivos | A2 añade solo este documento; ninguna medición dejó cambios en el árbol |
| Autoridades | V6 `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, A1 `d62019088b9e7a140d5066799afe6ace6db303ba`, ADR-0040 `9855a6b32a82954839ba2ae67cf351eae118c8ef`, freeze vigente `cb2105770e136b24dd6d096acb49af8f04adf491` y freeze histórico `49e8041c489d01b5f8dc20b1c7028d46488933ee`, sin cambio |
| `main` | `ba497f14581d81e83a27514852d6ec082ff57635`, merge de I-54. Avanzó desde `104ef3a` durante la revisión del Arquitecto sin tocar la identidad ni el núcleo: los cuatro archivos de producción citados en §2.1 son byte a byte iguales. La rama no se rebasa por un amendment documental; la reconciliación con `main` sigue WORKFLOW antes del siguiente gate productivo |
| Ramas paralelas | I-52 `deb08cdb81817e9e1fafc3500e542a0ac1c2c886` (Proposal V9, solo documentación). I-53D `a57bd506172bc70e6f415146dae245662baabd28`, rebasada sobre `ba497f1` (G7 del Dinámico), sin solape con A2. I-55 `5f774b459eb8b13732e4ae5c4e93a909874a10c5`, rama nueva `feature/creacion-de-vistas`: reclamo y bootstrap sobre `ba497f1`, solo `docs/ROADMAP.md` y su contrato, sin solape. I-54 integrada en `main`; su rama remota se retiró. Ninguna otra activa |

### 12.2 Corrección de la evidencia RED de G6-B

| | Seleccionadas | Fallidas | Superadas |
|---|---|---|---|
| Informe registrado | 253 | 199 | 54 |
| Reconstrucción corregida | 253 | 198 | 55 |

- De las siete pruebas revisadas, 6 fallan por comportamiento y 1 pasa
  (`ExpressionSymbolModelTests.LOS_AMBITOS_SON_PROJECT_Y_RACK_RESERVADO`).
- Solo **un** fallo del RED original venía de `Cast<int>`; las otras seis pruebas fallaban antes, por aserciones de
  comportamiento.
- Método: repetición exacta, desde el transcript de la sesión, de las escrituras de archivos entre la orden de G6
  reanudado y el RED. `8ed15f7` → RED → `5f969cc` reproduce los 32 archivos del commit sin diferencias. El RED
  reconstruido reproduce el TRX registrado (253 / 199 / 54) prueba a prueba, en resultado y mensaje; con solo la
  comparación de `Cast<int>` corregida da 253 / 198 / 55.
- La historia de commits **no** se edita: A2 registra la corrección para la evidencia de cierre.

---

## 13. Estado

```text
Amendment = A2
Finding   = VARIABLEID / QUALIFIER / P2.5 CONTRACT TENSION

Recommended model = E1 EXACT-KEY QUALIFIER

Coordinator = AGREED WITH A2 / E1
Architect   = PENDING EXACT A2 REVIEW
Owner       = PENDING

Replacement ADR = REQUIRED

Current G6 candidate = 5f969cc87acb6cbd8572af3e2b517ba3468e5c41

G6 = NOT CLOSED
G7 = BLOCKED

PRODUCTION CHANGES = NONE
```
