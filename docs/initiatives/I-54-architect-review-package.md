# I-54 — Paquete de revision exact-SHA: Proposal V3 (G2F, limitada)

> Paquete **autonomo** para revisar [I-54-proposal-v3.md](I-54-proposal-v3.md) sin la conversacion que la
> produjo. No autoriza implementar ni sustituye a la Proposal: la resume y dice como atacarla. Las versiones de
> este paquete que servian a las revisiones anteriores viven en `7c197af` (V1) y en `36c337b` (V2).

## 0. Que se pide y que no

**Se pide** una revision **exact-SHA** de la Proposal V3, por el Coordinador y por el Arquitecto, cada uno con su
veredicto, sobre el **mismo** SHA (§1). La revision esta **limitada** a:

1. Los **ocho cambios vinculantes de G2D**, C-1..C-8 (Proposal §0 y §17.1): que cada uno este incorporado en todas
   las secciones que afecta y que no se contradigan entre si ni con el resto de V3.
2. **AR-54-04**, reabierto en G2D y marcado `CLOSED IN V3`.
3. **AR-54-V2-01..06**, los hallazgos de G2D, marcados `CLOSED IN V3` (Proposal §18.1).
4. Las precisiones **PR-03, PR-06, PR-07, PR-12 y PR-13** (Proposal §21).
5. Las **RD afectadas**: RD-01, RD-03, RD-04, RD-07, RD-10, RD-13, RD-19 y RD-20 (Proposal §19).
6. El **alcance candidato del ADR modificado** (Proposal §15, marcas [7A], [7B] y [7C]).

«CLOSED IN V3» es la disposicion del ejecutor; los cierra esta revision.

**No se pide**:
- reabrir lo acordado en G2D: AR-54-01, -02, -03 y -05 `CLOSED`; V2-1..V2-22 verificados o resueltos por C-n;
  PR-01, -02, -04, -05, -08, -09, -10 y -11 `AGREE`; las RD con `AGREE`; `Kind` en blanco = opcion A;
  `ADR REQUIRED = YES`; la clasificacion de las preguntas al Owner;
- implementar, estimar, redactar el ADR o editar la Proposal;
- tocar `docs/HANDOFF.md` o `docs/ROADMAP.md`;
- revisar el alcance de I-49, I-50, I-52 o I-53.

Una seccion de V3 que ningun C-n toca es identica a V2; el diff de §1 lo demuestra.

## 1. Identificacion exacta

```text
Repositorio    = marioap-afk/Calculadora_de_racks
Rama           = architecture/propiedades-personalizadas
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main al reclamar y al abrir G2E)
MAIN_AL_PUBLICAR = f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (merge de I-50; V3 sin rebase: Proposal §2)
CLAIM_SHA      = 143490d8ecfb1c5f3011cf752c5cfcd11af13784   (Claim-Id d4b871e9-8a5d-4e67-bc11-a8f3c023788e)
BOOTSTRAP_SHA  = f908b2f4ba508bf365e55adbda78cd4eac505295
G1_SHA         = 195964b00006d2be74eefb8b6ae4f5f46de1cfc9   (Discovery)
PROPOSAL_V1    = 7c197af81b91df88366c873eddf9e11ddc5e87bb   (historica)
PROPOSAL_V2    = 36c337b84c47f9ac7c97d860fce42d3a5f4370e8   (historica)
REVIEW_V2      = Architect Review — I-54 Proposal V2 @ 36c337b = AGREED WITH CHANGES
                 AR-54-01/02/03/05 CLOSED · AR-54-04 REOPENED · 2 MATERIAL · 4 MINOR · cambios C-1..C-8
                 El Coordinador acepta C-1..C-8
PROPOSAL_V3    = el commit que introduce docs/initiatives/I-54-proposal-v3.md   ← SHA REVISADO
```

Un documento no puede contener el SHA del commit que lo crea, asi que el SHA revisado se fija **antes** de leer:

```bash
git fetch origin
git log -1 --format=%H origin/architecture/propiedades-personalizadas -- docs/initiatives/I-54-proposal-v3.md
```

Para acotar la lectura a lo que cambio, con `<V3_SHA>` = el SHA anterior:

```bash
git diff 36c337b84c47f9ac7c97d860fce42d3a5f4370e8:docs/initiatives/I-54-proposal-v2.md <V3_SHA>:docs/initiatives/I-54-proposal-v3.md
```

El veredicto vale **solo** para ese SHA. Cualquier cambio de la Proposal es otro SHA y otra revision. Este paquete
apunta **exclusivamente** a la V3.

## 2. Contexto minimo

- **RackCad**: plugin de AutoCAD 2025 (.NET 8), con capas `Domain ← Application ← UI ← Plugin`. Solo el Plugin
  toca AutoCAD y **ninguna suite de pruebas carga el Plugin** (ADR-0003).
- **Un rack** = una o varias **definiciones** de bloque (una por vista) que comparten un `RackId`. Cada una lleva
  un **sobre** JSON (`RackEmbedDocument`) en un Xrecord `RACKCAD_SELECTIVE` de su diccionario de extension
  (ADR-0009, ADR-0010). La doctrina es `UNKNOWN/UNREADABLE ≠ ABSENT` (ADR-0034 §8).
- **La propuesta en una frase**:
  - registros `{ Id GUID, Name, Value texto }` con `ExtensionData`;
  - Proyecto en un Xrecord nuevo `RACKCAD_CUSTOM_PROPERTIES` del NOD;
  - Rack en un miembro `CustomProperties: JsonElement?` del sobre, heredado por `Compose`;
  - una autoridad propia por `RackId` que nunca elige hermana y solo escribe sobre un kind conocido;
  - cota de profundidad 16 como constante del formato 1.x;
  - ninguna recuperacion destructiva;
  - una UI separada.

## 3. Que cambio desde la revision de V2

### 3.1 Los ocho cambios

La tabla completa esta en la Proposal §0 y la traza por seccion en §17.1.

| Cambio | Hallazgo | V2 | V3 | Donde, sobre todo |
|---|---|---|---|---|
| **C-1** | AR-54-V2-01 | Un kind no vacio desconocido y comun era escribible (PR-12) | **`UnknownKind`, de solo lectura**; Application recibe `isKnownKind` inyectado; PR-12 rechazada; `Kind` en blanco sigue siendo `MixedKind` | D-09.5, D-09.8, D-22.2, INV-22, T-AUT-10 |
| **C-2** | AR-54-V2-02, AR-54-04 | Cota 16 normativa pero fuera del ADR; «eliminar siempre se permite» | **16 = constante del formato 1.x**, subirla exige MAJOR; documento mas profundo = **`DepthLimitExceeded`**, sin ninguna escritura; eliminar solo se libera frente a conteo y longitud | D-04.3, D-05.6, D-05.7, D-07.4, D-08.4 B, INV-20, §9, T-STO-09 |
| **C-3** | AR-54-V2-03 | «El store nunca lanza» sin clases; NFC y serializacion sin validar | **3A** clases capturadas explicitas, sin `catch (Exception)`; **3B** UTF-16 validado antes de NFC y de serializar; **3C** residual preexistente del sobre declarado, F-14 al freeze | D-05.1, D-05.4, D-07.4, D-08.4 A y G, T-STO-08, -16, -18, -20 |
| **C-4** | AR-54-V2-04 | Nombres repetidos solo en la raiz y en la entrada | **Cualquier objeto y cualquier profundidad** → `PresentButUnreadable`, antes de mapear; residuales del sobre precisados | D-01.5, D-07.4, D-08.4, INV-24, T-STO-12 |
| **C-5** | AR-54-V2-05 | Origen `Absent` sin definir; solo se revalidaba el origen; INV-05 fisica | **5A** origen `Absent` = vacio canonico; **5B** revalidacion de la forma mostrada de **cada** miembro; **5C** atomicidad logica | D-09.10, D-22.5, INV-05, T-MUT-07, T-MUT-10 |
| **C-6** | AR-54-V2-06 | «`Id` exacto» | «`Id` igual por valor; su representacion persistida se escribe en `D` minusculas» | D-10.1, T-MUT-02 |
| **C-7** | AR-54-V2-01, -02, -04 | 15 puntos; valor de la cota excluido | 16 puntos; **[7A]** cota 16; **[7B]** regla de `Kind`; **[7C]** igualdad canonica completa | §15 |
| **C-8** | — | Matrices de V2 | §0, §2, §3, §12.9, §13, §14, §17..§19, §21, §22; este paquete; contrato; Discovery §2.7 | — |

### 3.2 Orden unico de resultados de autoridad (D-09.8)

1. `XrefRejected`
2. `NoIdentity`
3. `IndeterminateMembership`
4. `MixedKind`
5. `UnknownKind`
6. `CustomPropertiesReadOnly` (`PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor` o
   `DepthLimitExceeded`)
7. `Divergent`
8. `Single`

Todo resultado distinto de `Single` es de solo lectura, salvo la unificacion segura desde `Divergent`.

### 3.3 Clasificacion del store (D-07.4)

1. Validacion del texto y del arbol completo (JSON, transcodificacion, raiz objeto, nombres repetidos a
   cualquier profundidad, strings no decodificables, forma de `SchemaVersion`) → `PresentButUnreadable`.
2. Major mayor que 1 → `IncompatibleMajor`.
3. Estructura de `Entries` y de cada entrada → `PresentButUnreadable`.
4. Profundidad mayor que 16 → `DepthLimitExceeded`.
5. Ids repetidos por valor → `AmbiguousIdentity`.
6. `Readable`.

## 4. Lectura obligatoria, en este orden

1. **Proposal V3**:
   - §0, §17.1, §18.1, §19, §21 y §22: que cambio y como se traza;
   - C-1: D-09.5, D-09.8, D-09.10, D-18.3, D-18.5, D-22.2, D-22.3, D-22.5, INV-22 y §9;
   - C-2: D-04.3, D-05.6, D-05.7, D-07.3, D-07.4, D-08.4 B, D-09.6, D-09.11, D-10.1, D-13, INV-06, INV-20 y OQ-02;
   - C-3: D-05.1, D-05.2, D-05.4, D-07.4 (tabla de excepciones), D-08.4 A y G con residuales, INV-23 y RP-11;
   - C-4: D-01.5, D-07.4, D-08.4 (residuales), D-09.7 e INV-24;
   - C-5: D-09.10, D-18.4, D-22.1, D-22.5 e INV-05;
   - C-6: D-10.1;
   - C-7: §15;
   - pruebas: §12.9 y las filas que cita.
2. **Discovery**: §2.7 (addendum G2E).
3. **Codigo** en `BASE_SHA` (`git show 46fcac2:<ruta>`):

   | Archivo | Lineas | Para |
   |---|---|---|
   | `src/RackCad.Application/Persistence/RackDuplicationPlan.cs` | 222-243, 410-421 | C-1: `isKnownKind` inyectado; kind en blanco y kind no reconocido → inutilizable |
   | `src/RackCad.Plugin/RackDuplicarCommands.cs` | 65-69 | C-1: construccion del predicado en el Plugin |
   | `src/RackCad.Plugin/KindHandlers/KindHandlerRegistry.cs` | 26, 36-41 | C-1: registro `internal` del Plugin; busqueda con y sin mayusculas |
   | `src/RackCad.Plugin/KindHandlers/KindHandlerDispatch.cs` | 17-40 | C-1: «tipo no reconocido» como error visible |
   | `src/RackCad.Plugin/RackMenuCommands.cs` | 138-146 | C-1: `RACKEDITAR` resuelve el handler antes de editar |
   | `src/RackCad.Plugin/RackLayoutCommands.cs` | 60-67 | C-1: `RACKLAYOUT` rechaza un kind no reconocido |
   | `src/RackCad.Plugin/RackEnvelopeRestamp.cs` | 105-108 | C-1: el restamp lanza con un kind sin handler |
   | `src/RackCad.Plugin/ProjectVariableMutationExecutor.cs` | 207-210 | C-1: el ejecutor de variables solo compone `KindSelective` |
   | `src/RackCad.Application/Persistence/RackEmbedDocument.cs` | 86-118 | C-3C: `Deserialize` solo captura `JsonException` (`:98`) |

4. **Historia**: `git show 61cbff1 -- src/RackCad.Application/Persistence/RackEmbedDocument.cs` (C-1).
5. **Contrato y proceso**: `docs/initiatives/I-54-propiedades-personalizadas.md` §4 y §7 frente a
   `docs/WORKFLOW.md` §8 (C-3C: cuando se registra F-14).

## 5. Hechos decisivos y como verificarlos

### 5.1 Codigo e historia

| # | Hecho | Ruta : lineas @ `BASE_SHA` | Si fuera falso, cae |
|---|---|---|---|
| H-1 | `RackDuplicationPlan.Build` recibe `Func<string, bool> isKnownKind` y lo exige no nulo | `RackDuplicationPlan.cs:225-243` | precedente de D-09.5 y D-22.2 |
| H-2 | El plan declara inutilizable un sobre con `Kind` en blanco y otro con kind no reconocido | `RackDuplicationPlan.cs:412-420` | P-28 |
| H-3 | El Plugin construye el predicado con `KindHandlerRegistry.Default.TryGetIgnoreCase` | `RackDuplicarCommands.cs:66-69` | D-22.2 |
| H-4 | `KindHandlerRegistry` es `internal sealed` del Plugin, asi que Application no puede depender de el | `KindHandlerRegistry.cs:26` | D-22.2 («Application no depende») |
| H-5 | `RACKEDITAR` y `RACKLAYOUT` resuelven el handler antes de operar; el restamp lanza sin handler; el ejecutor de variables compone con `KindSelective` constante | `RackMenuCommands.cs:141`; `RackLayoutCommands.cs:64`; `RackEnvelopeRestamp.cs:105-108`; `ProjectVariableMutationExecutor.cs:208-210` | P-28 |
| H-6 | `61cbff1` añadio `KindPushBack` y `CurrentSchemaVersion` siguio en `"1.0"` | `git show 61cbff1 -- …/RackEmbedDocument.cs` | P-29 |
| H-7 | `RackEmbedStore.Deserialize` solo captura `JsonException` | `RackEmbedDocument.cs:94-101` | residual de C-3C |
| H-8 | El contrato de I-54 lleva los hallazgos laterales a `ideas-futuras.md` «al fijar el contrato», que la Proposal situa en G2-FREEZE (§14), y limita G0..G2 a `docs/initiatives/I-54-*.md` | contrato §4 y §7 en la rama | momento de F-14 |

### 5.2 Hechos ejecutados [X] y como reproducirlos

Son de sondas locales de .NET 8.0.29 **fuera del repositorio**, con copias literales de `RackEmbedDocument`,
`RackEmbedStore`, `SchemaVersionPolicy` y `RackEmbedComposer` de `BASE_SHA` («BASE») y la variante con el miembro
`CustomProperties` («variante»), como en el paquete de V2 (`36c337b`, §5.2). Los casos X-1..X-20 de ese paquete
siguen valiendo; en particular **X-5**: 63 niveles dentro del miembro se leen y 64 dejan el sobre en `null`.

| # | Caso | Resultado observado | Sostiene |
|---|---|---|---|
| Y-1 | Documento 1.3 conforme salvo un campo informativo a profundidad 20, con `JsonDocument.Parse` | se parsea sin error | C-2, P-30 |
| Y-2 | `JsonDocument.Parse(string)` y `JsonDocument.Parse(string, opciones)` sobre texto C# que contiene un surrogate alto suelto **sin escapar**; y 65 arrays anidados con `MaxDepth = 64` | con el surrogate, `ArgumentException` (no `JsonException`) en las dos sobrecargas; con 65 niveles, `JsonException` | C-3A, P-31, tabla de D-07.4 |
| Y-3 | JSON con la **secuencia de escape** de un surrogate suelto en un nombre de miembro, y en un string anidado dentro de un campo desconocido; se leen `JsonProperty.Name`, `GetString()`, `ValueEquals` y `GetRawText()` | `Parse` no lanza; `Name`, `GetString` y `ValueEquals` lanzan `InvalidOperationException`; `GetRawText` no lanza | C-3A, P-31 |
| Y-4 | Par de surrogates valido escapado | `GetString` correcto y bien formado | C-3, control |
| Y-5 | BASE `Deserialize`: surrogate suelto sin escapar en el texto del sobre; surrogate escapado en un miembro declarado (`Name`); surrogate escapado dentro de `CustomProperties` | lanza `ArgumentException`; devuelve `null`; el sobre sigue legible | C-3C, P-31 |
| Y-6 | `string.Normalize(NormalizationForm.FormC)` con un surrogate suelto | `ArgumentException` | C-3B, P-32 |
| Y-7 | `JsonSerializer` y `JsonNode` serializan un string con un surrogate suelto y se relee | los dos escriben la secuencia de escape de U+FFFD; el valor releido contiene U+FFFD y no es el recibido | C-3B, P-32 |
| Y-8 | Recorrer el texto con `Rune.DecodeFromUtf16` sobre «Área», un surrogate alto suelto al final, uno bajo suelto al principio y un emoji | solo los dos casos con surrogate suelto son mal formados | C-3B, D-05.1 |
| Y-9 | `{"a":1,"a":2}` frente a `{"a":2}` con un mapa «gana el ultimo» y uno «gana el primero» | el primero los da por iguales y el segundo por distintos | C-4, P-33 |
| Y-10 | Sobre con `CustomProperties` y `customproperties` a la vez | BASE conserva ambas claves en `ExtensionData` y las reemite; la variante mapea las dos al miembro y se queda con la ultima | C-4, D-08.4 residuales |
| Y-11 | Matching sin mayusculas de `System.Text.Json` frente a `OrdinalIgnoreCase` con ı, İ, ſ, K (Kelvin), letras de ancho completo y espacio de ancho cero | coinciden en todos los casos | P-34, D-01.3 |
| Y-12 | `JsonDocumentOptions { MaxDepth = 64 }` constante con `Parse("{}")` | no lanza | C-3A: `ArgumentException` de `Parse` solo por contenido |

## 6. Hallazgos a re-verificar

| Hallazgo | Sev. | Donde lo cierra V3 | Que re-verificar |
|---|---|---|---|
| AR-54-04 | MATERIAL | D-05.7, D-08.4 B, INV-20, §9, §15.5 | Que la cota 16 sea constante del formato en todas las secciones; que un documento legible mas profundo sea `DepthLimitExceeded` y nunca `PresentButUnreadable` ni `IncompatibleMajor`; que no quede afirmacion de aislamiento sin la condicion de profundidad |
| AR-54-V2-01 | MATERIAL | D-09.5, D-09.8, D-18.5, D-22.2, D-22.5, INV-22, T-AUT-10 | Que ningun camino escriba sobre `UnknownKind`; que el predicado sea inyectado y Application no dependa del registro; que PR-12 no sobreviva en ninguna seccion |
| AR-54-V2-02 | MATERIAL | D-04.3, D-05.6, D-05.7, D-13, D-18, §9, §15, INV-20, T-STO-09, U-02 | Que «eliminar siempre se permite» quede limitado a conteo y longitud; que ninguna escritura, eliminar y vaciar incluidos, alcance `DepthLimitExceeded`; que el estado se muestre desde la lectura |
| AR-54-V2-03 | MINOR | D-05.1, D-05.4, D-07.4, D-08.4 A y G, T-STO-08, -16, -18, -20 | Que las clases capturadas sean explicitas y cada una solo pueda venir del contenido; que NFC y el serializador nunca reciban UTF-16 mal formado; que el residual preexistente este bien descrito y F-14 tenga momento de registro |
| AR-54-V2-04 | MINOR | D-01.5, D-07.4, D-09.7, D-08.4, T-STO-12, T-AUT-04 | Que la deteccion alcance cualquier objeto a cualquier profundidad antes de mapear; que la igualdad solo vea documentos sin duplicados; que los residuales del sobre no afirmen «gana la ultima en todos los builds» |
| AR-54-V2-05 | MINOR | D-09.10, D-22.5, INV-05, T-MUT-07, T-MUT-08, T-MUT-10 | Que el origen `Absent` sea un documento concreto; que se revaliden el conjunto y la forma de cada miembro; que la atomicidad sea la del estado logico sin reescribir BTR ya iguales |
| AR-54-V2-06 | MINOR | D-10.1, T-MUT-02 | Que no quede ninguna exigencia de `Id` textual exacto en mutaciones ni pruebas |

## 7. Precisiones y RD afectadas

| # | Estado en V3 | Por que |
|---|---|---|
| PR-03 | **RECONCILED** | C-2: constante del formato 1.x con `DepthLimitExceeded`, no rechazo al escribir |
| PR-06 | **RECONCILED** | C-4: cualquier objeto y cualquier profundidad; residuales del sobre precisados |
| PR-07 | **RECONCILED** | C-5C: atomicidad logica; C-5A: origen `Absent` canonico |
| PR-12 | **REJECTED / RECONCILED** | C-1: un kind desconocido comun es `UnknownKind`, de solo lectura |
| PR-13 | **RECONCILED** | C-5B: se revalidan todos los miembros mostrados y el conjunto |

| RD | G2D | V3 | Cambio |
|---|---|---|---|
| RD-01 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | Filas MAJOR de profundidad y de dependencia del `Kind` (C-1, C-2) |
| RD-03 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | `Id` igual por valor en mutaciones (C-6) |
| RD-04 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | Eliminar frente a conteo y longitud (C-2); UTF-16 al escribir (C-3) |
| RD-07 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | Cota del formato, clases de excepcion, duplicados a cualquier profundidad (C-2, C-3, C-4) |
| RD-10 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | `UnknownKind`; igualdad sin duplicados sobre todo el contenido salvo el minor; orden unico (C-1, C-4, C-7C) |
| RD-13 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | Origen `Absent`, revalidacion completa, atomicidad logica (C-5) |
| RD-19 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | Solo lectura por `UnknownKind` y `DepthLimitExceeded`; origen «vacio / sin propiedades» (C-1, C-2, C-5A) |
| RD-20 | AGREE WITH CHANGE | ACCEPTED WITH RECONCILIATION | Alcance del ADR con [7A], [7B] y [7C] (C-7) |

Las demas RD quedan `ACCEPTED`, como las acordo G2D. **DEFERRED: ninguna. OPEN DISAGREEMENT: ninguno.** Preguntas
al Owner: sin cambios, salvo la aclaracion de OQ-02 (la cota 16 no pertenece a los limites de tuning).

## 8. Preguntas de ataque para V3

Sugerencias, no conclusiones.

| # | Cambio | Pregunta | Pista |
|---|---|---|---|
| A3-01 | C-1 | ¿Queda algun camino (crear, renombrar, cambiar valor, eliminar, vaciar, unificar) que escriba sobre un rack `UnknownKind`, en la autoridad, el ejecutor, la UI, las pruebas o los gates? | D-09.5, D-09.10, D-18.5, D-22.5; T-AUT-10, T-AUT-14, T-MUT-07; buscar `desconocido` y `UnknownKind` |
| A3-02 | C-1 | ¿Es total el orden de D-09.8, y remiten a el D-13, D-18.5 y T-AUT-15 sin otro orden? ¿Un kind conocido junto a uno desconocido da `MixedKind`? | D-09.5 (tabla), D-09.8; T-AUT-10 (d) |
| A3-03 | C-1 | ¿La inyeccion de `isKnownKind` deja la decision en Application sin romper capas ni T-GRD-04? | H-1, H-3, H-4; D-22.2, D-22.3 |
| A3-04 | C-2 | ¿Queda algun texto que trate la cota 16 como tuning, como rechazo al escribir o como `PresentButUnreadable`? | Buscar `16`, `profundidad` y `cota`; D-04.3, INV-20, §9, §15.5, OQ-02 |
| A3-05 | C-2 | ¿El orden de D-07.4 da un resultado unico para cada combinacion (p. ej. major 2 profundo, minor futuro profundo, profundo con ids repetidos)? | D-07.4; T-STO-09, T-STO-14; X-5, Y-1 |
| A3-06 | C-2 | ¿«Eliminar se permite» queda limitado a conteo y longitud en D-05.6, D-10.1, T-STO-18 y U-02? | D-05.6, D-10.1 (precondicion comun) |
| A3-07 | C-3A | ¿La tabla de clases capturadas cubre todo lo que el contenido externo provoca en el pipeline elegido, sin `catch (Exception)` y acotada a la operacion que lo produce? | D-07.4; Y-2, Y-3, Y-12 |
| A3-08 | C-3B | ¿NFC y el serializador nunca reciben UTF-16 mal formado, ni al escribir ni al leer? | D-05.1, D-05.2, D-05.4; Y-6, Y-7, Y-8 |
| A3-09 | C-3C | ¿Es exacta la descripcion del residual preexistente y correcto registrar F-14 en G2-FREEZE y no ahora? | D-08.4; H-7, H-8; Y-5 |
| A3-10 | C-4 | ¿El recorrido unico detecta nombres repetidos en cualquier objeto antes de mapear, y los residuales del sobre estan enunciados sin sobreafirmar? | D-01.5, D-07.4, D-08.4; Y-9, Y-10 |
| A3-11 | C-5A | ¿El vacio canonico como origen `Absent` es coherente con la condicion de minor de D-09.10 y con la igualdad de D-09.7? | D-09.7, D-09.10; T-AUT-14, T-MUT-10 |
| A3-12 | C-5B | ¿El intent de unificacion lleva lo necesario para revalidar cada miembro (forma canonica o huella determinista) sin que el Plugin decida? | D-22.5; T-GRD-04 |
| A3-13 | C-5C | ¿INV-05, D-09.10, D-22.1, §15.10 y T-MUT-08 expresan la misma atomicidad logica? | buscar `atomic` |
| A3-14 | C-6 | ¿Queda alguna frase o prueba que exija el texto exacto del `Id` en una mutacion? | D-10.1, T-MUT-02; buscar `exacto` |
| A3-15 | C-7 | ¿§15 congela [7A], [7B] y [7C] completos y excluye solo UI, nombres de comando, 50/80/1000, gates y tuning? | §15 |
| A3-16 | C-8 | ¿Las trazas de §17.1, §18, §19 y §21 corresponden al texto, y ninguna seccion no tocada difiere de V2? | diff de §1 |

## 9. Ramas paralelas (medidas al publicar V3)

| Rama | SHA | Que cambio desde G2D | Cruce con I-54 |
|---|---|---|---|
| `main` | `f8deb675c6d1ef0e64693b157d69c4cc170d7b24` | Integracion de I-50; ADR-0035 `aceptado` | Ninguno productivo: no cambia ningun archivo de codigo del mapa de I-54 ni ninguno de los citados aqui; `src/` sigue con 1 `new RackEmbedDocument`, 7 `Compose` y 33 comandos; `git merge-tree` sin conflictos. V3 no se rebaso (Proposal §2) |
| I-49 `architecture/motor-expresiones-parametricas` | `048a508e570e210467c3693a11ac4a487f383944` | +1 commit solo docs: Proposal V6 | Semantico: `Rack`/`Project` reservados para ID20 (`I-49-proposal-v6.md:622-624`); censos 33/29 intactos; sin dependencia |
| I-50 `feature/cotas-independientes-por-vista` | rama remota retirada | Integrada en `main` | Ver `main` |
| I-52 `feature/rackmirror-espejo-semantico` | `04457183fc2d7dcda5d6c1b988a4789ed4ea2f8f` | +1 commit solo docs: Proposal V2 y ADR-0036 corregido | Cumple D-21 (`I-52-proposal-v2.md:458-459`); portadores intactos, propiedades de rack incluidas (`0036-…md:88-90`); censos 33 → 34 y T-GRD-02 7 → 8 al integrar |
| I-53 `feature/cabeceras-configurables-multidestino` | `d7f17addb47ea943b61ff50b4b4a5b23010d6fab` | +1 commit solo docs: Proposal V2 y ADR-0037 `propuesto` | Ninguno: sin datos persistidos; numeracion de ADR |

Si al revisar alguna rama se movio, citar su SHA nuevo y medir solo si invalida C-1..C-8 o una costura de I-54.

## 10. Formato del veredicto

```text
Architect Review — I-54 Proposal V3 @ <SHA de 40 hex>
Global = AGREED | AGREED WITH CHANGES | NOT AGREED

Cambios de G2D
  C-1 = INCORPORATED | NOT INCORPORATED: <motivo>
  ...
  C-8 = ...

Hallazgos
  AR-54-04    = CLOSED | STILL OPEN: <motivo>
  AR-54-V2-01 = CLOSED | STILL OPEN: <motivo>
  ...
  AR-54-V2-06 = ...

Hallazgos nuevos (dentro del alcance de §0)
  [BLOCKER | MATERIAL | MINOR] <id> — C-n / RD-xx / PR-xx — <afirmacion> — <evidencia ruta:linea @ SHA> — <cambio requerido>

Precisiones
  PR-03 = AGREE | AGREE WITH CHANGE: <cambio> | DISAGREE: <motivo>
  PR-06 = ...
  PR-07 = ...
  PR-12 = ...
  PR-13 = ...

RD afectadas
  RD-01 = AGREE | AGREE WITH CHANGE: <cambio> | DISAGREE: <motivo>
  RD-03, RD-04, RD-07, RD-10, RD-13, RD-19, RD-20 = ...

ADR Candidate Scope = AGREE | AGREE WITH CHANGE: <cambio>
Required Proposal V4 changes = NONE | <lista numerada vinculante>
```

El Coordinador emite su veredicto con el mismo encabezado («Coordinator Review — I-54 Proposal V3 @ <SHA>»).

| Severidad | Significado |
|---|---|
| **BLOCKER** | Inimplementable tal como esta escrito, o contradice un ADR aceptado o una doctrina vigente |
| **MATERIAL** | Cambia una decision, un invariante o una superficie |
| **MINOR** | Precision de redaccion o de evidencia que no cambia decisiones |

## 11. Reglas de la revision

- La evidencia de codigo se cita sobre `BASE_SHA`. `origin/main` ya avanzo a `f8deb67` sin cambiar ningun archivo
  de codigo citado en §4 y §5; si vuelve a avanzar al revisar, se citan ambos SHAs.
- Una afirmacion de C-1..C-8 sin cita verificable es, por si misma, un hallazgo.
- Fuera del alcance de §0 solo se levanta un BLOCKER; cualquier otra observacion se anota aparte y no es
  vinculante para G2F.
- Una guarda de texto no es criterio de aceptacion: el criterio es el comportamiento (leccion de I-48).
- **La revision no desbloquea la implementacion.** Si converge, se abre G2-FREEZE; el consenso exige
  `Coordinator = AGREED` y `Architect = AGREED` sobre el mismo SHA, el ADR en estado `propuesto` y la aprobacion
  del Owner.
