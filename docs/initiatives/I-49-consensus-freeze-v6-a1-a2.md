# I-49 — Consensus Freeze V6 + A1 + A2 + ADR-0041

Registro normativo del consenso vigente de I-49 (ID22B, motor de expresiones paramétricas). **No** repite la Proposal,
los Amendments ni el ADR: los congela por sus blobs exactos, junto con la aceptación del Owner. Es un freeze **nuevo**:
ni el [Consensus Freeze histórico](I-49-consensus-freeze.md), sobre V6 y ADR-0038, ni el
[Consensus Freeze V6 + A1](I-49-consensus-freeze-v6-a1.md), sobre V6, A1 y ADR-0040, se reescriben. Los dos quedan
como historial.

## 1. Autoridades

```text
Initiative = I-49 / ID22B

Proposal = V6
V6 blob =
ef4db3aa400483ff25a8f39b2beb93708fa43d1a

Amendment A1 blob =
d62019088b9e7a140d5066799afe6ace6db303ba
A1 commit =
f69e903a4440e0f3c6e22bdf08217ff02c465b9e   (operativo, tras el rebase de A2-R1)
71268eb09a3e04231da4e25b985e175c977f8c9e   (revisado)

Amendment A2 blob =
49a925336dd3775929a35b0f40b73cb7f8c487f7
A2 commit =
7c1eed47f5ea2cb66b91d782bc05f28ae1c144c7   (operativo, tras el rebase de A2-R1; CI 34806719419)
5a3714f06aa695e22db0f16455a1ea8dce5195b6   (revisado; CI 34803272375)

ADR        = ADR-0041
ADR status = ACCEPTED
ADR proposal blob =
7689f82c3e915d49c7bd3e1b8a92e56b8ce63fc4
ADR proposal commit =
8cefd591bffaf0ac9099796d858a643f3df1e28c   (CI 34810029689)
ADR acceptance commit =
2e8090a07df604f79ea3491fd75dd54368b5db28   (CI 34854645845)
ADR accepted blob =
c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231

Previous ADR = ADR-0040 — REPLACED BY ADR-0041

Previous freezes = historical only
                   I-49-consensus-freeze.md         blob 49e8041c489d01b5f8dc20b1c7028d46488933ee
                   I-49-consensus-freeze-v6-a1.md   blob cb2105770e136b24dd6d096acb49af8f04adf491

Coordinator = AGREED WITH V6
              AGREED WITH A1
              AGREED WITH A2 / E1
Architect   = AGREED WITH V6
              AGREED WITH A1
              AGREED WITH A2
Owner       = ADR-0041 ACCEPTED

Consensus = FROZEN — V6 + A1 + A2 + ADR-0041
```

Fuentes:

- [Proposal V6](I-49-proposal-v6.md), blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`: commit operativo
  `5cad0d54bf1643f0219fcc26eac58e36f366ae98`, tras el rebase de A2-R1. Los tags `archive/i-49-a2-pre-rebase-5a3714f`,
  `archive/i-49-a1r3-pre-rebase-2eeeab1` y `archive/i-49-a1-pre-rebase-71268eb` conservan la rama anterior a cada
  reconciliación, con V6 en `cd8588017eca13bd14b2724477d6014211b85216`, `8189c7df30ae6c3faaf4138bef02a4161176a2c1` y
  `1ed93a0525ec11c5092df89c55cbf498c99f8e2a`; el commit histórico `048a508e570e210467c3693a11ac4a487f383944` es
  anterior al rebase de G3A.
- [Amendment A1](I-49-proposal-v6-amendment-a1-text-guard.md), blob `d62019088b9e7a140d5066799afe6ace6db303ba`: revisado
  en `71268eb09a3e04231da4e25b985e175c977f8c9e`; tras el rebase de A1-R1, `cae7a9f5ae10e9c86c16a29000c0933e2187873f`;
  tras la reconciliación de A1-R3, `c7fabbef7b4d65728bb7bef33a1b3b87441fddb2`; y, tras el rebase de A2-R1,
  `f69e903a4440e0f3c6e22bdf08217ff02c465b9e`.
- [Amendment A2](I-49-proposal-v6-amendment-a2-exact-key-qualifier.md), blob `49a925336dd3775929a35b0f40b73cb7f8c487f7`:
  revisado en `5a3714f06aa695e22db0f16455a1ea8dce5195b6` (CI 34803272375) y, tras el rebase de A2-R1, en
  `7c1eed47f5ea2cb66b91d782bc05f28ae1c144c7` (CI 34806719419). El Arquitecto lo acordó en su revisión exacta de ese
  blob, sin hallazgos materiales; A2 conserva en su §13 el estado con que se publicó, y el acuerdo consta en el §14 del
  registro de I-49.
- [ADR-0041](../adr/0041-motor-expresiones-parametricas-identidad-textual-y-cualificador-clave-exacta.md): propuesto en
  `8cefd591bffaf0ac9099796d858a643f3df1e28c` (blob `7689f82c3e915d49c7bd3e1b8a92e56b8ce63fc4`; CI 34810029689) y
  aceptado en `2e8090a07df604f79ea3491fd75dd54368b5db28` (CI 34854645845), con blob aceptado
  `c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231`. La aceptación solo cambió su encabezado, con su preámbulo, y su bloque
  «Decisión del Owner»: desde «Contexto» hasta el final, el texto aceptado es byte a byte el del blob propuesto.
- [ADR-0040](../adr/0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md): aceptado el 2026-09-13
  (blob `9855a6b32a82954839ba2ae67cf351eae118c8ef`; commit original `2eeeab1a2fce3fb33c8151f8ef05d0d109531415`, tras la
  reconciliación de A1-R3 `8f6d5831eccf08fab3535ce1dc29f87882624fbb` y tras el rebase de A2-R1
  `a86c958dd706912b80eb2d4f063d1745fae13661`) y marcado `reemplazado por ADR-0041` en el mismo commit que acepta
  ADR-0041 (blob `c0e7b7314d529e2d91a60ee48b06d8b1c8f4c5ab`), con solo su Estado cambiado y una nota posterior fechada.
- [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md): aceptado el 2026-09-13 y
  `reemplazado por ADR-0040` (blob `ab10130b7ce69dcd15ad196017beb27acbfb4c5a`); historial.
- [Consensus Freeze histórico](I-49-consensus-freeze.md): versionado en `364d6c06e44273a63a7b6f6509daf357611ea77a`; tras
  el rebase de A1-R1, `788b00f1d3e70ad43f0a72ce0b9db7f5f88313db`; tras la reconciliación de A1-R3,
  `5fa71409756d6cb30cecfea70b85d4c22c6bbf17`; y, tras el rebase de A2-R1, `6c5319455c032ac642062223719a507bfdfd134d`.
  Blob `49e8041c489d01b5f8dc20b1c7028d46488933ee`.
- [Consensus Freeze V6 + A1](I-49-consensus-freeze-v6-a1.md): versionado en `8ed15f782f7fc2878693053e54cedca8a041d247`
  (CI 34789875181) y, tras el rebase de A2-R1, en `f95464a360c7f8ba77472ef034470605b606aec6`. Blob
  `cb2105770e136b24dd6d096acb49af8f04adf491`.
- **Reconciliación con `main`.** La rama se rebasó en A2-R1 sobre `main` `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093`
  (merge de I-53D E3). `main` no avanzó desde entonces, así que A2-R3 no rebasa: la aceptación y este freeze se apoyan
  en esa misma base.
- [Registro de I-49](../automation/decisions/I-49.md): §14, Amendment A2 y ADR-0041 propuesto; §15, aceptación del Owner
  de ADR-0041 («Acepto ADR-0041 para I-49 sobre Proposal V6 + Amendments A1 + A2.», 2026-09-14) y reemplazo de
  ADR-0040.
- [Contrato de I-49](I-49-motor-expresiones-parametricas.md).

## 2. Qué congela y precedencia

- **Proposal V6**, en el blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, sigue siendo el **contrato técnico base** de
  I-49.
- **Amendment A1**, en el blob `d62019088b9e7a140d5066799afe6ace6db303ba`, manda sobre V6 **solo** en la guarda de
  recurso del parser: el tema de P1.9/D8 y los lugares que enumera A1 §3.3.
- **Amendment A2**, en el blob `49a925336dd3775929a35b0f40b73cb7f8c487f7`, manda sobre V6 y sobre ADR-0040 **solo** en:
  - la identidad textual de `VariableId` como clave de `projectVariable`, que conserva sin cambio (A2 §3.1);
  - la validez neutral de esa clave (A2 §3.2);
  - la gramática del cualificador (A2 §3.3);
  - las reglas léxicas del cualificador (A2 §3.4, §3.8);
  - el formatter `Q(clave)` (A2 §3.6, §3.7);
  - el comportamiento de round-trip de la clave exacta (A2 §3.5, §4, §5);

  en los lugares que enumera A2 §7, leídos con su regla general de lectura.
- **Cada uno dentro de su alcance, A1 y A2**, en sus blobs, mandan sobre cualquier resumen posterior, este freeze
  incluido, y, ante una duda de detalle, también sobre ADR-0041. **Fuera de esos dos alcances explícitos manda V6**,
  del mismo modo sobre cualquier resumen posterior y, ante una duda de detalle, sobre ADR-0041, como fija su
  «Autoridad normativa». Una duda sobre el cualificador **no** se resuelve hacia la forma D de P3.9 (A2 §10).
- **ADR-0041** es la **decisión arquitectónica aceptada** y la **sucesora completa** de ADR-0040: D1–D25, con D1–D4 y
  D9–D25 sin cambio de semántica, D5 con la validez neutral de la clave, D6 y D7 con el cualificador de clave exacta y
  su formatter `Q(clave)`, y D8 con la guarda de A1 y solo la redacción del cualificador. Incluye la extensión de
  ADR-0034, Schema V-0, las expresiones como fuente de propiedad, la clasificación de fallos estructurales frente a
  semánticos, el `PlanReadSet`, la `RepairDecisionObservation` y la revisión acotada de la política V8-R05 de I-48.
- **ADR-0040** y **ADR-0038** quedan como **historial reemplazado**: no son autoridad para implementar.
- **Los dos Consensus Freeze anteriores** son evidencia histórica: A1 activó la regla de invalidación del histórico, y
  el hallazgo que A2 resuelve, la del Consensus Freeze V6 + A1. Ninguno autoriza seguir implementando.
- **Proposals V1–V5** son únicamente **historial**.
- **No existe ninguna discrepancia abierta** entre Coordinator, Architect y Owner.

## 3. Contrato de identidad congelado (A2)

```text
projectVariable SymbolId key = VariableId.Value text from accredited registry
comparison                   = OrdinalIgnoreCase

NO System.Guid value identity
NO canonical-D normalization
NO collapsing D / N / B / P / X
NO migration
NO alias identity
NO old drawing rejection solely because id representation is non-D

Neutral key validity:
non-null
non-empty
key == key.Trim()
Guid.TryParse(key) succeeds

Returned Guid is never identity.

Qualifier Q(key):
D-shaped : #<lowercase-d>
non-D    : #{<escaped exact key>}
           with } → }}

P2.5 = PRESERVED STRONG AND UNCONDITIONAL
```

- **Clave e igualdad** (A2 §3.1). La clave de `SymbolId(projectVariable, clave)` es el texto del `VariableId` de la
  entrada del registro acreditado, con igualdad `OrdinalIgnoreCase`. Tampoco se endurece la regla del `VariableId`
  persistido. P8.1, P3.6 y P6.4 no cambian.
- **Validez neutral** (A2 §3.2). La regla no nombra ningún tipo de `ProjectVariables`, y el conjunto de claves válidas
  coincide exactamente con el de los `VariableId.Value` posibles. `Guid.TryParse` **solo** decide la admisibilidad: el
  `Guid` devuelto nunca se usa para igualdad, hash, búsqueda, identidad ni para desambiguar en el formatter. G8 prueba
  que su adaptador transporta `VariableId.Value → SymbolId.Key` sin normalizar.
- **Gramática y reglas léxicas** (A2 §3.3, §3.4). `qualifier = "#" , ( guid-d | braced-key )`, con
  `braced-key = "{" , { char - "}" | "}}" } , "}"` y `}}` como único escape. El contenido desescapado no está vacío, es
  igual a su `Trim()`, satisface `Guid.TryParse` y es el texto de la clave. `#{` sin cerrar da `UnterminatedName`; un
  contenido inválido o cualquier otra cosa tras `#`, `InvalidQualifier`; no hay códigos de diagnóstico nuevos, y un
  fragmento sigue sin parsearse.
- **Autoridad de la sintaxis** (A2 §3.5). La sintaxis del cualificador conserva el texto de la clave; un `Guid` que la
  implementación guarde o parsee por comodidad no es autoritativo y nunca resuelve identidad. El binder resuelve solo
  con la clave textual y el comparador del namespace, y devuelve el `SymbolId` de la entrada de la tabla.
- **Formatter `Q(clave)`** (A2 §3.6). Una clave con **forma D exacta** —36 caracteres, `-` en las posiciones 9, 14, 19
  y 24 y dígitos hexadecimales ASCII en las demás, en cualquier combinación de mayúsculas— da
  `"#" + minúsculasASCII(clave)`; cualquier otra, `"#{" + minúsculasASCII(clave)` con cada `}` sustituida por `}}` +
  `"}"`. `minúsculasASCII` solo pasa `A`–`Z` a `a`–`z`: no cambia la identidad ni el texto persistido del `VariableId`.
- **Presentación** (A2 §3.7, §3.8). Un homónimo se muestra con la forma del nombre + `Q(clave)`; una referencia rota,
  solo con `Q(clave)`; los candidatos de `AmbiguousName`, con la forma del nombre + `Q(clave)`. La entrada
  `#{<clave con forma D>}` es legal, resuelve igual que la clave textual D, se muestra `#<d>` y no crea una segunda
  identidad.
- **Inyectividad y P2.5** (A2 §4, §5). Con la premisa de A2 §4 —todo carácter de una clave admisible es ASCII o
  espacio en blanco Unicode, que exige la prueba G de A2 §9.3—, claves distintas bajo `OrdinalIgnoreCase` dan
  cualificadores distintos, y dos grafías de una misma identidad dan el mismo: una identidad, un texto. El binder
  invierte al formatter, así que los contraejemplos R1 y R2 de A2 §2.3 y §2.4 quedan resueltos.
  `Canonicalize(Bind(Parse(Format(b)))) == b` rige para todo `BoundExpression` canónico sin referencias rotas en un
  mismo snapshot, sin excepción de «formato razonable», con el corpus obligatorio de A2 §5.

## 4. Guarda del parser congelada (A1)

```text
NO character validity limit

MaxSyntacticTokens initial = 4096
minimum                    = 1536
bound                      = tokens(Format(b)) <= 6n - 3

Both qualifier forms = ONE token
```

- El mínimo es el de A1.2: `6 × máximo normativo de nodos`, que con nodos ≤ 256 vale 1536. Como en V6 §1.2, los nombres
  de tipos y miembros son ilustrativos.
- `MaxSyntacticTokens` es un valor de implementación del núcleo: cuenta los tokens con la granularidad normativa de
  A1.3, corta durante el lexing con un único `LimitExceeded` de tipo `SyntacticTokenCount` (A1.4) y no sustituye a los
  límites normativos del `BoundExpression`, que se validan tras el bind (A1.5). Con n ≤ 256, la cota del formatter da
  `tokens(Format(b)) ≤ 1533` (A1 §4). `TextLength` sale del contrato sin reutilizar su valor numérico (A1 §6).
- Las dos formas del cualificador, `#<d>` y `#{<clave exacta>}`, cuentan **un** token: el cualificador de A1.3. Un
  `braced-key` largo sigue siendo un token, A1.1 no fija límite de caracteres, y los requisitos de A1 §5 se aplican a un
  `braced-key` igual que a un nombre entre llaves (A2 §6). A2 **no** enmienda A1.
- P2.5 permanece literalmente fuerte (A1 §7). Los nombres no cambian: no se añade ningún límite de longitud y el nombre
  solo no puede estar en blanco (A1 §8).

## 5. Decisiones cerradas

```text
CR-1..CR-5 = CLOSED
FR-1       = CLOSED
RR-1       = CLOSED
A-03       = CLOSED
OPEN A     = CLOSED   (qualifier form read with A2)
OPEN B     = CLOSED
Schema     = V-0

A1 P1.9/P2.5 tension                      = CLOSED
A2 VARIABLEID / QUALIFIER / P2.5 tension  = CLOSED IN CONTRACT
                                            implementation correction pending = G6-C2

Identity           = UNCHANGED
Qualifier          = "#" , ( guid-d | braced-key )
P2.5               = PRESERVED STRONG AND UNCONDITIONAL
Parser token guard = UNCHANGED BY A2
```

- Siguen cerradas las demás decisiones que enumera V6 §0.11, leídas con A1 §3.3 y A2 §7: su fila «Texto de entrada
  ≥ 4000» se lee como la guarda de tokens de A1.2, la forma del cualificador de OPEN A se lee con A2 §3.3, y los
  límites de nodos, argumentos y profundidad siguen CLOSED.
- La tensión de A2 queda cerrada en el contrato: A2 acordado por Coordinator y Architect y ADR-0041 aceptado por el
  Owner. El candidato de G6 todavía implementa la regla de clave solo D anterior a A2; su corrección es G6-C2 (§7).

## 6. Regla de invalidación

Cualquier cambio **material** durante la implementación que altere:

- la Proposal V6;
- el Amendment A1;
- el Amendment A2;
- ADR-0041;
- D1–D25;
- P2.5;
- la identidad de `VariableId` como clave de `projectVariable`;
- el cualificador `Q(clave)`, su gramática o sus reglas léxicas;
- la validez de la clave;
- la guarda de recurso del parser;
- Schema V-0;
- el `PlanReadSet`;
- la `RepairDecisionObservation`;
- los contratos R1, R2 y R3 de P21.3 (OPEN B);
- los nombres;
- la semántica de Expression;
- la persistencia;
- la clasificación de fallos estructurales frente a semánticos;
- la extensión de ADR-0034;
- la revisión acotada de V8-R05;
- cualquier decisión cerrada de V6, de A1 o de A2;

**INVALIDA este freeze**. En ese caso:

```text
STOP IMPLEMENTATION
→ document finding
→ Coordinator review
→ Architect review if material
→ Owner decision if ADR changes
```

El contrato **no** se corrige en silencio desde la implementación.

La integración posterior de otras iniciativas puede mover líneas o archivos, pero **no** modifica por sí sola el
consenso: las citas de V6, A1, A2 y ADR-0041 se re-verifican contra el `main` vigente, y solo un cambio material de
premisa activa esta regla.

## 7. Gates

```text
G0 = CLOSED
G1 = CLOSED
G2 = CLOSED
G3 = CLOSED
G4 = CLOSED
G5 = CLOSED

G6 candidate = 5f969cc87acb6cbd8572af3e2b517ba3468e5c41   (original; CI 34795083053)
               988ab9c30c2f90fbf72363f5c237e098d6ff2e95   (rebased in A2-R1; same blobs)
G6           = NOT CLOSED
Reason       = candidate still implements pre-A2 D-only key rule

G6-C2 = READY AFTER THIS FREEZE HAS GREEN CI
G6-C2 NOT STARTED IN THIS COMMIT
G7    = BLOCKED UNTIL G6-C2 CLOSES G6
```

- G4 cerró en `82aa61b20c59f4377b17f57793eac8edcfd9079f` y G5 en `c4880af66586c535954b0ba0bff7b64b887bddb4`; tras el
  rebase de A1-R1 están en `6ffe964a0517eab83f7f4ad9d6fe794cccc34fa6` y `fb9f6308662c98ac011b4123435247102e3fc9c0`;
  tras la reconciliación de A1-R3, en `535b5bf4ad37ad8b9005d37374e7a06e15e2ed53` y
  `977f87321d705c39ad6d538aa41f2b61ed98aee9`; y tras el rebase de A2-R1, en
  `0c29f510bd9ecb7285ca4e8a58ed264f18455ea5` y `892a6010951f023fc2ab6822b758cdad406a55d2`. G5 sigue CLOSED y no se
  revierte (A1 §10): G6-C2 solo supera la fila de prueba de G5 que enumera A2 §9.2 y, si retira
  `QualifierSyntax.Id`, también cambian las aserciones y utilidades que lo leen (§8).
- G6 produjo su candidato sobre el Consensus Freeze V6 + A1. La revisión de solo lectura G6-C1 encontró sobre ese
  candidato la VARIABLEID / QUALIFIER / P2.5 CONTRACT TENSION, que confirmó la revisión de identidad del Arquitecto,
  y G6 quedó sin cerrar. Este freeze recoge su resolución: A2 acordado por Coordinator y Architect y ADR-0041 aceptado
  por el Owner, con ADR-0040 ya reemplazado.
- **G6-C2** es la corrección acotada de G6 que fija A2 §9.1 sobre ese candidato. Pasa a `READY` solo cuando la CI de
  este freeze esté verde. Este freeze **no** implementa nada ni inicia G6-C2: G6-C2 empieza solo con una orden
  explícita, en otra ejecución. A2-R3 no cambia producción ni pruebas.
- G6 se cierra solo a través de G6-C2; la CI verde del candidato anterior a A2 no lo cierra. G7 sigue bloqueado hasta
  que G6-C2 cierre G6.
- Los gates productivos G6 —cuyo cierre pasa por G6-C2— a G12 siguen la secuencia, las evidencias y las compuertas de
  V6 §8, leídas con A1 §3.3 y A2 §7. Para G6, eso incluye la evidencia de V6 §8.1: RED→GREEN y dorados geométricos
  intactos.

## 8. Precondiciones de G6-C2

Antes de iniciar G6-C2:

- `git fetch --all --prune`;
- la rama reconciliada con el `main` vigente según WORKFLOW (V6 §8.2, §12.2);
- las ramas activas revisadas;
- el procedimiento de colisión de [`decisions/I-49.md`](../automation/decisions/I-49.md) §8, antes de editar cualquier
  archivo productivo;
- V6 exacta: blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`;
- A1 exacto: blob `d62019088b9e7a140d5066799afe6ace6db303ba`;
- A2 exacto: blob `49a925336dd3775929a35b0f40b73cb7f8c487f7`;
- ADR-0041 `aceptado`: blob `c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231`, aceptado en
  `2e8090a07df604f79ea3491fd75dd54368b5db28`;
- este freeze exacto, tal como lo versiona el commit que lo crea.

**Trabajo de G6-C2**: RED → GREEN de la corrección acotada de A2 §9.1 sobre el candidato de G6, con la matriz de
pruebas obligatoria A–M de A2 §9.3 (ADR-0041, D6):

```text
SymbolId key validity : D-only                        → neutral key validity (A2 §3.2)
qualifier syntax      : parsed Guid                   → exact key text (A2 §3.5)
lexer / parser        : "#" , guid                    → "#" , ( guid-d | braced-key ) (A2 §3.3, §3.4)
formatter             : "#" + lowercase key           → Q(key) (A2 §3.6)
binder                : resolves the exact textual key (A2 §3.5)
parsed System.Guid    : no semantic identity use      (A2 §3.5)
```

- Su prueba B, la conformidad del adaptador, la hace G8 (A2 §3.2; ADR-0041, D5).
- Todo lo demás del candidato se conserva salvo que las pruebas demuestren lo contrario (A2 §9.1).
- Las pruebas superadas son solo las que enumera A2 §9.2; si la implementación retira `QualifierSyntax.Id`, cambian con
  ella las aserciones y utilidades que lo leen, y si lo conserva como dato no autoritativo, no.
- Sin códigos de diagnóstico nuevos, sin migración y sin cambio de A1 (A2 §3.1, §3.4, §6).

Siguen vigentes los requisitos heredados del Consensus Freeze V6 + A1 (§6), con las integraciones de `main` al día:

- **Serialización de archivos calientes** (WORKFLOW §7).
- **`RackSelectiveWindow.xaml.cs` y el G10 de I-49**: I-53S está integrada en `main` y, con su limpieza, se liberó
  el G10 de I-49 ([decisiones de I-53](../automation/decisions/I-53.md) §13.1). Antes de editar la ventana, G10 vuelve
  a localizar `Describe(PropertyId, string)` por firma y semántica contra la ventana vigente en `main`, verifica P23.13
  y D21 y corre sus RED y guardas.
- **I-52 y la duplicación con restamp**, si aplica: la prueba de supervivencia de `expression` (P24.8) se escribe
  contra `RackDuplicationPlan` y el restamp vigentes en `main`.
- **Censos vigentes de comandos y ventanas**: I-49 no añade ninguno (P22.1, P23.14), y sus guardas se escriben contra
  el censo vigente al rebasar.
- **Prueba de composición `expression + DimensionViews`**: un Selectivo con una fuente `expression` en `PropertyValues`
  y `DimensionViews` presentes sobrevive a RACKEDITAR y a guardar y reabrir conservando ambos conceptos (ADR-0041,
  Consecuencias; V6 §12.1).
- **V6, A1, A2 y ADR-0041 exactos** son la autoridad de cada gate.
