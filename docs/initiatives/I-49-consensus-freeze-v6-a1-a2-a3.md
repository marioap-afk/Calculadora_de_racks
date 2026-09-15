# I-49 — Consensus Freeze V6 + A1 + A2 + A3-R2 + ADR-0043

Registro normativo del consenso vigente de I-49 (ID22B, motor de expresiones paramétricas). No repite la Proposal,
los Amendments ni el ADR: congela sus blobs exactos, la aceptación explícita del Owner y su precedencia. Es un freeze
nuevo. Los tres freezes anteriores permanecen inmutables y sirven únicamente como evidencia histórica.

## 1. Autoridades exactas

```text
Initiative = I-49 / ID22B

Proposal = V6
V6 blob =
ef4db3aa400483ff25a8f39b2beb93708fa43d1a

Amendment A1 blob =
d62019088b9e7a140d5066799afe6ace6db303ba

Amendment A2 blob =
49a925336dd3775929a35b0f40b73cb7f8c487f7

Amendment = A3-R2
A3-R2 blob =
4da6ef3caa21dcf31140983c6db7e23f02aa3e18
A3-R2 versioning commit =
5210c1c0534d3cf5bee4d23526f24e062bdc77c1
A3-R2 exact consensus record =
addb5223265612785dfe19ef1f353a19307f0a83

ADR = ADR-0043
ADR status = ACCEPTED
ADR proposal SHA =
d54a8d7db830d763af293ab87ccd981c478c6272
ADR proposal blob =
164ac166080d0b2be660473ec721110301a46dbf
ADR acceptance commit =
f6b123414621d8e4b1b8aee36eef0ce22f19eb48   (CI 34994376191, push, success 4/4)
ADR accepted blob =
1cf7b92760e6d357bcc953c58f179be3d5467c40

Previous ADR = ADR-0041 — REPLACED BY ADR-0043
ADR-0041 accepted blob =
c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231
ADR-0041 replaced blob =
31202b3474696bbd0ad7952a953b3c28217538d1

Previous freezes = HISTORICAL ONLY
I-49-consensus-freeze.md             blob 49e8041c489d01b5f8dc20b1c7028d46488933ee
I-49-consensus-freeze-v6-a1.md       blob cb2105770e136b24dd6d096acb49af8f04adf491
I-49-consensus-freeze-v6-a1-a2.md    blob 57736f725663aab79a24b29685ed34e8c3e9ebad

Coordinator = AGREED WITH V6
              AGREED WITH A1
              AGREED WITH A2 / E1
              AGREED WITH A3-R2
              AGREED WITH ADR-0043

Architect   = AGREED WITH V6
              AGREED WITH A1
              AGREED WITH A2
              AGREED WITH A3-R2
              AGREED WITH ADR-0043

Owner       = ADR-0043 ACCEPTED

Consensus = FROZEN — V6 + A1 + A2 + A3-R2 + ADR-0043
```

La decisión vinculante del Owner, Mario Pérez, fue emitida el 2026-09-15 en el canal del Coordinador de I-49:

> «Acepto ADR-0043 para I-49 sobre Proposal V6 + Amendments A1 + A2 + A3-R2.»

La respuesta nombra el ADR, la iniciativa y toda la base técnica. El consenso técnico exacto de ADR-0043 fue previo:
Coordinador y Arquitecto declararon `AGREED WITH ADR-0043` sobre el blob propuesto
`164ac166080d0b2be660473ec721110301a46dbf`; la revisión exacta del Arquitecto no tuvo hallazgos materiales. La
aceptación está registrada en el §18 del [registro de I-49](../automation/decisions/I-49.md).

Fuentes:

- [Proposal V6](I-49-proposal-v6.md), blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`: contrato técnico base.
- [Amendment A1](I-49-proposal-v6-amendment-a1-text-guard.md), blob
  `d62019088b9e7a140d5066799afe6ace6db303ba`: guarda de recurso del parser.
- [Amendment A2](I-49-proposal-v6-amendment-a2-exact-key-qualifier.md), blob
  `49a925336dd3775929a35b0f40b73cb7f8c487f7`: identidad textual de `VariableId`, validez neutral y cualificador de
  clave exacta.
- [Amendment A3-R2](I-49-proposal-v6-amendment-a3-multi-cause-dependency-failures.md), blob
  `4da6ef3caa21dcf31140983c6db7e23f02aa3e18`: fallos con varias causas, raíces completas y recuperación segura.
- [ADR-0043](../adr/0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md): propuesto en
  `d54a8d7db830d763af293ab87ccd981c478c6272`, blob `164ac166080d0b2be660473ec721110301a46dbf`, y aceptado en
  `f6b123414621d8e4b1b8aee36eef0ce22f19eb48`, blob `1cf7b92760e6d357bcc953c58f179be3d5467c40`.
- [ADR-0041](../adr/0041-motor-expresiones-parametricas-identidad-textual-y-cualificador-clave-exacta.md): autoridad aceptada
  anterior, ahora reemplazada entera por ADR-0043. Su contenido técnico y su aceptación histórica permanecen
  inmutables.

## 2. Precedencia exacta

- **V6** es el contrato técnico base.
- **A1** manda sobre V6 únicamente en su alcance de guarda de recurso del parser: P1.9/D8 y los lugares enumerados en
  A1 §3.3. Retira el límite de validez por longitud de texto y lo sustituye por la guarda de complejidad sintáctica.
- **A2** manda sobre V6 únicamente en su alcance de clave exacta e identidad textual de `VariableId`: la validez
  neutral de la clave, la gramática y reglas léxicas del cualificador, `Q(clave)`, su round-trip y los lugares
  enumerados en A2 §7. La identidad sigue siendo el texto con comparación `OrdinalIgnoreCase`; el `Guid` parseado
  nunca es identidad.
- **A3-R2** manda sobre V6 y lo refina únicamente en el alcance que enumera A3:
  - varias causas de `DependencyFailed`;
  - bloqueos semánticos estáticos en la evaluación del registro;
  - cadenas representativas;
  - `RootSignature` y `RootCauses`;
  - propagación de raíces a toda la SCC;
  - distinción de `RecoveryUnit`;
  - recuperación de SCC simples y no simples;
  - precondición de descubrimiento;
  - bloqueo estructurado de recuperación;
  - datos estables de `SymbolResult`;
  - semántica de `Upstream` con `RootCauses` completas;
  - contrato relacionado de pruebas de recuperación y concurrencia.
- **ADR-0043** es la autoridad arquitectónica aceptada que incorpora V6 y esos tres alcances de enmienda en D1–D25
  completos.
- Dentro de su alcance explícito manda cada Amendment sobre V6 y sobre cualquier resumen posterior, incluido este
  freeze. Fuera de los alcances explícitos de A1, A2 y A3-R2, manda V6. Ante una duda de detalle se consulta el blob
  autoritativo correspondiente; este freeze no crea semántica adicional.
- ADR-0041, ADR-0040 y ADR-0038 son historial reemplazado. Los freezes anteriores son historial y no autorizan G7.

## 3. Contratos preservados de A1 y A2

```text
Parser resource guard:
NO finite character-validity limit
MaxSyntacticTokens initial = 4096
minimum                    = 1536
tokens(Format(b))          <= 6n - 3
both qualifier forms       = ONE token

VariableId identity:
key        = accredited VariableId.Value text
comparison = OrdinalIgnoreCase
NO System.Guid value identity
NO canonical-D normalization
NO collapse of D / N / B / P / X

Qualifier Q(key):
D-shaped = #<lowercase-d>
non-D    = #{<escaped exact key>}

P2.5 = PRESERVED STRONG AND UNCONDITIONAL
```

A3-R2 no cambia A1 ni A2. No añade un límite finito de caracteres, no debilita P2.5, no normaliza la identidad y no
reduce el cualificador exacto a la forma D.

## 4. Núcleo de A3 congelado

### 4.1 Diagnósticos locales y `DependencyFailed`

- Los diagnósticos propios de cada símbolo permanecen locales. No se copian a consumidores ni a otros miembros de una
  SCC.
- Un símbolo no evaluado emite un `DependencyFailed` por cada dependencia directa fallida situada fuera de su propia
  SCC, además de sus diagnósticos locales aplicables, con el orden estable de A3-R2.
- Dentro de una SCC no se emiten fallos de dependencia por aristas internas. El diagnóstico de ciclo y los fallos
  locales se modelan según A3-R2, sin multiplicarlos por caminos.

### 4.2 Cadena representativa y causas raíz

- La cadena es una sola cadena representativa determinista por `DependencyFailed`. Sirve como explicación trazable y
  no es `RootCauses` ni demuestra por sí sola todas las causas.
- `RootSignature` identifica una causa raíz por firma estable. `RootCauses` es el conjunto completo de firmas raíz,
  deduplicado y ordenado determinísticamente.
- `RootCauses` se calcula sobre la condensación de SCC y se propaga a todos los miembros de una SCC. Incluye las raíces
  externas que entren por cualquiera de sus miembros y las raíces propias aplicables del ciclo.
- `RootCauses` es dato derivado: no se persiste, no enumera caminos y admite memoización sin cambiar el resultado.

### 4.3 Ciclos y unidades de recuperación

- `CycleRoot` es la unidad sin dueño que representa una SCC cíclica.
- `RecoveryUnit` es la unidad de corrección authored: el dueño corregible de una firma o un `CycleRoot`. Es distinta de
  `RootSignature`; varias firmas del mismo dueño cuentan como una unidad, y un fallo propio de un miembro del ciclo es
  una unidad distinta del `CycleRoot` de esa SCC.
- La garantía de sugerencia para ciclos solo cubre el ciclo simple que define A3-R2. Una SCC no simple no recibe esa
  garantía; su bloqueo usa la razón estructurada correspondiente.

### 4.4 `SymbolResult`, `PlanReadSet` y `Upstream`

- `SymbolResult` conserva sus diagnósticos ordenados y los datos estables de A3-R2: causa directa fallida, cadena
  representativa, firma raíz y conjunto completo `RootCauses`, según el tipo de resultado. El límite RR-1 entre
  resultado calculado y observación de reparación permanece intacto.
- `PlanReadSet` conserva la lectura declarada por V6 y A3-R2. Para cada variable fallida que el rack realmente leyó,
  `Upstream` contiene por separado `SymbolId + RootCauses(variable)` completas.
- Las variables leídas no se colapsan en una unión global. `Upstream` no incorpora la cadena representativa ni la
  `RecoveryUnit`.

### 4.5 Bloqueos estáticos y fallos numéricos latentes

- `InvalidArguments` estático se determina antes de la evaluación numérica y participa como causa local conforme a
  A3-R2.
- Si la evaluación queda bloqueada por un fallo estático o estructural, los fallos numéricos que solo aparecerían al
  evaluar son latentes y no se descubren ni se inventan.

### 4.6 Recuperación segura

Una sugerencia de recuperación exige simultáneamente:

1. que el descubrimiento de consumidores del mismo intent correctivo termine con éxito; un rack `Indeterminate` nunca
   se interpreta como que no consume la fuente;
2. que todas las fuentes fallidas relevantes estén presentes en `Upstream`, por variable leída y con sus
   `RootCauses(variable)` completas;
3. que todas esas causas se reduzcan a una única `RecoveryUnit` común;
4. que, cuando esa unidad sea `CycleRoot`, la SCC cumpla la definición de ciclo simple de A3-R2;
5. que se cumplan las demás precondiciones R1 y §7.2 de A3-R2, incluidas la observación estable, la validación de las
   fuentes retenidas y la ausencia de razones estructuradas de bloqueo.

Si una condición falla, no hay sugerencia. Las razones estructuradas y sus datos siguen el conjunto y el orden exactos
de A3-R2, incluidos `OtherInvalidSources`, `SeveralRecoveryUnits`, `NonSimpleCycle` y `DiscoveryIndeterminate`.

## 5. Decisiones cerradas y alcance

```text
CR-1..CR-5 = CLOSED
FR-1       = CLOSED
RR-1       = CLOSED
A-03       = CLOSED
OPEN A     = CLOSED
OPEN B     = CLOSED
Schema     = V-0

A1 parser-resource-guard tension             = CLOSED
A2 VariableId / qualifier / P2.5 tension      = CLOSED
A3 multi-cause / recovery model               = CLOSED IN CONTRACT

G6 final = 75f18623e7ced836de7eb2e36d6db8e311efb76d
G6       = CLOSED
```

Continúan vigentes D1–D25, P2.5 fuerte, Schema V-0, la extensión de ADR-0034, las expresiones como fuente de una
propiedad vinculable, la persistencia, `PlanReadSet`, `RepairDecisionObservation` y la revisión acotada de V8-R05. El
alcance fuera de I-49 que D25 excluye permanece excluido.

## 6. Regla de invalidación

Cualquier cambio material que altere uno de estos elementos invalida este freeze:

- Proposal V6;
- Amendment A1;
- Amendment A2;
- Amendment A3-R2;
- ADR-0043;
- D1–D25;
- P2.5;
- identidad textual de `VariableId`;
- `Q(key)`, su gramática, reglas léxicas o round-trip;
- guarda de recurso del parser;
- semántica de varias causas;
- `RootSignature` o `RootCauses`;
- `RecoveryUnit`;
- recuperación de ciclos;
- `PlanReadSet` o la semántica de `Upstream`;
- `RepairDecisionObservation`;
- Schema V-0.

Entonces:

```text
STOP IMPLEMENTATION
→ document finding
→ Coordinator review
→ Architect review if material
→ Owner decision if ADR authority changes
```

Nada de ese contrato se corrige en silencio desde la implementación. Un nuevo SHA, por sí solo, tampoco permite
atribuir a otro commit la evidencia exacta de este freeze.

## 7. Gates

```text
G0 = CLOSED
G1 = CLOSED
G2 = CLOSED
G3 = CLOSED
G4 = CLOSED
G5 = CLOSED
G6 = CLOSED

Before exact-SHA green CI for this freeze:
G7 = BLOCKED

Only after exact-SHA green CI for this freeze:
G7 = READY — NOT STARTED

G8 = BLOCKED UNTIL G7 CLOSES
```

Este documento no inicia ni implementa G7. La CI verde del commit de aceptación acredita esa aceptación, pero no
sustituye la CI del SHA exacto que versiona este freeze. Solo el segundo resultado cambia el estado de G7 a
`READY — NOT STARTED`. G8 permanece bloqueado.

## 8. Precondiciones para iniciar G7

Antes de iniciar G7 en una ejecución posterior:

- `git fetch --all --prune`;
- rama reconciliada con el `main` vigente según WORKFLOW;
- ramas y worktrees activos re-medidos, con evaluación de colisiones materiales de producto;
- Proposal V6 exacta, blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`;
- A1 exacto, blob `d62019088b9e7a140d5066799afe6ace6db303ba`;
- A2 exacto, blob `49a925336dd3775929a35b0f40b73cb7f8c487f7`;
- A3-R2 exacto, blob `4da6ef3caa21dcf31140983c6db7e23f02aa3e18`;
- ADR-0043 aceptado, blob `1cf7b92760e6d357bcc953c58f179be3d5467c40`;
- este freeze exacto y su CI `push` verde sobre el mismo SHA;
- el procedimiento de archivos calientes y las guardas vigentes del contrato de I-49.

G7 ejecutará su propio RED → GREEN y sus evidencias en otra orden. Este freeze no cambia producción, pruebas,
ROADMAP ni HANDOFF.

## 9. Auditoría anterior al versionado

La auditoría documental anterior al commit re-mide:

- todos los blobs de V6, A1, A2 y A3-R2 contra las autoridades de §1;
- la respuesta literal del Owner y el consenso técnico exacto;
- ADR-0043 `aceptado`, blob `1cf7b92760e6d357bcc953c58f179be3d5467c40`;
- ADR-0041 `reemplazado por ADR-0043`, blob `31202b3474696bbd0ad7952a953b3c28217538d1`;
- A1, A2 y A3-R2 preservados sin cambio;
- los tres freezes anteriores preservados en sus blobs de §1;
- G6 cerrado, G7 condicionado a la CI exacta de este documento y G8 bloqueado.

Cualquier diferencia material detiene el gate antes del commit.
